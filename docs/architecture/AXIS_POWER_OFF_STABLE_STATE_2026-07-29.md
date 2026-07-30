# Axis Power Off exact-once stable-state facade

Date: 2026-07-29
Updated: 2026-07-30

## Scope

The SDK exposes a split accepted-continuation path and a composed convenience
facade. `BeginPowerOffWaitForStableStateAsync` sends one Axis Power request with
`enabled=false` and returns after a valid successful acknowledgement is stored
in a session-bound `LMCAxisPowerOffWaitContinuation`.
`ResumePowerOffWaitForStableStateAsync` accepts that continuation and performs
status-only polling. `PowerOffAndWaitForStableStateAsync` composes those two
phases with one total deadline.

Default completion requires three consecutive successful `0x2028` results with:

```text
PowerOn = false
Standstill = true
Axis read successful and AxisErrorId = 0 (`status.IsSuccess`)
```

The Power Off acknowledgement proves command acceptance only. It does not prove
that the axis has finished stopping or that drive power is physically removed.

## Wire and failure contract

1. The Begin phase acquires only the per-session/per-axis mutation gate and
   holds it through ACK publication and pending-continuation installation. It
   does not acquire the status-observation gate.
2. Send `0x2023` with `enabled=false` exactly once and record the process-local
   axis mutation generation at the may-have-been-sent boundary.
3. Atomically publish the successful ACK, Power Off mutation generation, and
   latest pending continuation for the originating connection session and
   send-priority generation before returning it.
4. The Resume phase acquires the status-observation gate, verifies the original
   Power Off mutation generation, and polls only `0x2028`; it never sends
   `0x2023`.
5. Reset the stable counter on any valid observation that does not match the
   expected Power Off and Standstill state, including a nonzero AxisErrorId.
6. Never replay `0x2023` after response loss, timeout, cancellation, or
   send-priority result discard.

A newer accepted Power Off atomically supersedes the older pending Power Off
continuation. An older or different connection/session/axis continuation is
rejected before a status request reaches the wire. Timeout, cancellation,
status failure, and priority preemption preserve the accepted continuation for
status-only resume; its next resume starts a fresh consecutive stable counter.
Concurrent Begin calls therefore publish continuations in wire-ACK order, so
only the most recently transmitted accepted Power Off remains pending.
Concurrent Resume calls are serialized by the status gate; once one completes
the exact continuation, the queued call is revalidated and rejected zero-wire.

Immutable result and exception evidence separates:

- submission outcome: `NotAttempted`, `Rejected`, `OutcomeUncertain`, or
  `Accepted`;
- whether the command may have reached the wire;
- the accepted or rejected acknowledgement;
- the last observed status;
- poll, stable-sample, required-sample, and elapsed counts;
- `PowerOffMutationGeneration`, `ObservedMutationGeneration`, and
  `InterveningMutationDetected`;
- whether a post-write total deadline invalidated the transport.

The exchange normally drains a response after the write boundary and publishes
results only for the originating connection session and send-priority
generation. If an ACK or status response does not arrive before the total
deadline, it invalidates the connection as `Faulted` instead of reusing an
ambiguous RPC stream. Evidence records `TransportInvalidatedAtDeadline=true`
and the caller must reconnect. A missing Begin ACK remains
`OutcomeUncertain`; a missing Resume status preserves the accepted continuation
as immutable evidence and never replays `0x2023`. Because the continuation is
bound to the faulted session, it is rejected after reconnect; recovery requires
a new explicit exact-identity decision rather than automatic command replay.

The final status sample is published and resolved only while the originating
connection session, send-priority generation, Power Off mutation generation,
and deadline are still current. A cancellation or deadline observed before
that publication keeps the accepted continuation pending. A late cancellation
or deadline arriving after final proof commit does not overturn success.

## Pending Power On interaction

Status samples collected by this facade continue to update an already accepted
Axis Power On continuation. A stable Power Off result does not silently resolve
that continuation. The caller must still invoke the explicit, identity-checked
Power On recovery resolution path.

Each Resume captures the exact pending Power On continuation under the shared
status gate and resets its Power Off/Standstill proof before polling. Timeout,
cancellation, status failure, or priority preemption resets that same target's
proof again. Stable samples from disconnected Resume epochs are never combined.

## WPF integration

The WPF uses one direction-aware durable Axis Power journal. Version 2 stores
`ExpectedPowerOn`; a legacy version-1 record is read as Power On and is upgraded
on its next write. A fresh Power Off is durably armed before `0x2023(false)`.
The SDK accepted observer changes the same record to `AcceptedAwaitingProof`
after atomic ACK/continuation publication and before the first status request.

The WPF Power Off button calls Begin inside `RunSafetyCommandAsync`. The command
gate and priority scope cover exact-identity checks, one `0x2023`, ACK
publication, durable Axis Power acceptance, and motion-recovery safety-command
acceptance. It then releases the command gate and calls Resume inside the
preemptible `RunSafetyMonitorAsync` phase. An accepted-boundary timeout or
cancellation records the motion safety command before status-only proof starts.

While `0x2028` verification is running, another Power Off button click is
rejected zero-wire and Stop remains available as an explicit newer safety
command. After a transient timeout, cancellation, or status failure, the next
Power Off click resumes the exact pending continuation with status-only reads;
it never replays `0x2023`.

If the SDK reports `LMCAxisPowerOffInterferenceException`, the UI records a
confirmed-interference state and changes the button to `Power Off Again
(Confirmed Interference)`. Only that explicit click may send one replacement
`0x2023`; an accepted replacement supersedes the old continuation, while a
rejected replacement preserves the old exact pending continuation and the
confirmed flag. The monitor reservation is released exactly once on every
success or failure path.

The continuation is valid only for its originating connection session and
axis. Connection loss clears the WPF reference, but the durable direction and
identity remain. Accepted, Armed, or `RecoveryRequired` Power Off records use
exact endpoint, axis/reference, DiagnosticsBootId, and MapRevision checks before
restart status-only proof. They never replay `0x2023`. An uncertain Power On
record instead requires an explicit Power Off safety takeover, which atomically
replaces the On record with an Off record.

If the Axis Power journal cannot be opened, locked, or updated, new live
mutations are fail-closed. Explicit safety Power Off remains available with a
process-local degraded record; read-only and safety paths remain available.
Stale accepted observers, resolved tombstones, and connection-loss promotion
races cannot revive or fault a newer/safety-dominant durable record.

The process regression kills the real WPF child after the Power Off ACK while
the first `0x2028` is held. It proves the live writer lock, lock reacquisition
after termination, `AcceptedAwaitingProof` with `ExpectedPowerOn=false`, zero
`0x2023` requests in the restarted session, exactly three `0x2028` samples, and
the same identity resolved.

## Attribution boundary

The SDK shares a process-local axis mutation generation scoped by connection
session and `AxisReference`. Raw synchronous/asynchronous Power On/Off, Reset,
Stop, Move Absolute/Relative/Velocity, and accepted-wait writes issued through
`LMCSingleAxis` advance it at the may-have-been-sent boundary. Validation,
cancellation, or preemption that remains zero-wire does not advance it, and a
different `AxisReference` does not interfere.

Power Off Resume checks the original generation before a status wire, at status
publication, and at final resolution. A later same-axis mutation throws
`LMCAxisPowerOffInterferenceException`, preserves expected/observed/intervening
evidence and the pending continuation, and never replays `0x2023`. External PLC
logic, another RPC client, direct SDO writes, and group operations remain outside
this attribution boundary.

The current `0x2028 StatusWord` field remains reserved zero. It is not a DS402
status word and is not part of this completion predicate.

Current PC verification:

- SDK Debug/Release full suite: `906/906 PASS`
- Axis Power Off dedicated contract tests: `35 PASS`
- WPF Release actual-control smoke: `156/156 PASS`
- LASAL SourceOnly `Phase5TransportClean / StaticTopologyOnly`: PASS

These results prove the PC request/evidence contract only. LASAL build/download,
physical-axis completion timing, and packet capture remain unverified.

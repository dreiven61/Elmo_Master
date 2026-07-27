# Phase 1 WPF Test Runbook - 2026-07-27

## 1. Test boundary

This runbook covers the first PC/WPF update only:

- read-only transport qualification
- Group Enable, Buffered, and deterministic Stop-first qualification
- 24-entry Bulk snapshot/lifecycle and one-slave-offline checkpoint flow
- Recorder Single/Ring/soak and reconnect adoption flow
- read-only D5 SDO abort -> recovery qualification
- D5 unresolved-ticket/submission quarantine interlocks shared by qualification,
  manual SDO Read, and Drive Operation Mode/Status reads

No tracked LASAL `.st`, `Network`, or `Include` source was changed in this
slice. A new LASAL build/copy is therefore not required only for this slice.
The PLC used for the test must already contain the current D5 general-inline
SDO Read implementation and the current Bulk/Recorder implementation.

PC build/test proof is not PLC, drive, EtherCAT mailbox, or packet-capture
proof. Save the WPF `QTEST` log and capture separately.

## 2. Build and launch

Solution:

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.sln`

Configuration:

- `Debug | Any CPU` for the first live run
- `Release | Any CPU` only after Debug behavior is confirmed

Default Debug executable after a normal build:

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe`

Current PC baseline: API tests `Debug 223/223 PASS`, `Release 223/223
PASS`; WPF `Debug/Release Rebuild PASS`.

PI Write is deliberately disabled in the Phase 1 WPF UI and handler. The SDK
write allowlist is also empty. Do not treat PI Write as a Phase 1 test item.

Do not run `negative-wire --execute-live` as part of this first test. The
negative-wire live mode intentionally sends invalid protocol requests and
requires a separate approved test window and packet capture.

## 3. Common preflight

Before every live scenario:

1. Use the test PLC/project, not a production controller.
2. Confirm the selected physical axes can be stopped safely.
3. Connect WPF and run Diagnostics Capabilities.
4. Confirm nonzero `DiagnosticsBootId` and `MapRevision`.
5. Load the PI Catalog when the scenario requires it.
6. Confirm no previous WPF operation or qualification is running.
7. Save each scenario to a separate `QTEST` log.

If a scenario can move an axis, verify the working envelope and keep Stop and
Power Off available before starting it.

## 4. Recommended order

### 4.1 Read-only smoke

Run in this order:

1. `Run Read-only 0x2045 RPC`
2. Diagnostics Capabilities
3. EtherCAT Health
4. Load PI Catalog
5. Read selected PI signals

Stop if BootId or MapRevision changes unexpectedly, a response envelope is
malformed, or the selected object/reference does not match the physical axis.

### 4.2 D5 SDO abort -> recovery

Prerequisites:

- selected `_LMCAxis1..4` reference exactly equals the selected slave number
- selected axis `PowerOn=False` and `Standstill=True`
- three consecutive actual-position reads are identical
- capabilities include `SDORead` and `SDOReadGeneralInline`
- no manual D5 ticket is pending

Use only a manufacturer-approved nonexistent read-only object/sub-index as the
abort candidate. The UI default `0xFFFF:0 UInt32/4` is a candidate, not proof
that the drive vendor approves it.

Press `Run D5 Abort -> Recovery`.

PASS requires all of the following:

- baseline `0x6061:0 Int8/1` completes successfully
- abort ticket is terminal `Failed/Failed`
- `OperationErrorId=-32000`
- `OperationDetail` is the actual nonzero raw EtherCAT SDO abort code
- abort result length is zero
- BootId and MapRevision remain stable through the normal abort flow
- a distinct recovery ticket reads `0x6061:0 Int8/1`
- recovery value exactly equals the baseline value
- final `QTEST event=END verdict=PASS`

Local validation failure, TCP failure, runner timeout, or user cancellation is
not SDO-abort PASS evidence.

If the UI shows `Resolve D5 Quarantine`:

- do not send Power On, Reset, motion, new diagnostics configuration, or Close
- Stop, Power Off, read-only checks, and existing Bulk/Recorder cleanup remain
  available
- if the connection was externally lost, reconnect first
- press `Resolve D5 Quarantine` and save the resulting log

Manual `Submit SDO Read`, `Get Drive Operation Mode`, and `Read Drive Status`
use the same fail-closed D5 tracker. A response loss or polling timeout in any
of these paths can therefore enable `Resolve D5 Quarantine` and block new
state-changing work until resolution. This is expected safety behavior, not a
request to bypass the interlock.

The drive facade does not yet expose whether a generic command exception
happened before submission or during ticket status polling. Such an exception
is therefore quarantined conservatively even when no ticket may have been
accepted. Save the log; do not classify this false-positive possibility as a
PLC failure without packet evidence.

External manual/drive tracking lines use their own
`scenario=D5ExternalTracking:<stage>` run ID. They must not inherit the run ID
or scenario of the previously completed Group/Bulk/Recorder qualification.

Recovery uses two distinct tickets with exact type, length, and byte equality:
`0x6061:0 Int8/1` when GeneralInline is available, or the fixed legacy
`0x1000:0 UInt32/4` read on an SDORead-only PLC. `TicketNotFound` during known
ticket cleanup means the PLC terminal slot was replaced: it proves the prior
ticket had become terminal, but its outcome remains `UNKNOWN`. A stale local
connection session is quarantined instead of being treated as terminal.

`same_session_executor_reuse` proves only that two distinct known-valid reads
can run after an uncertain submit outcome. It does not prove disconnect/orphan
cleanup or the old ticket's terminal state. `new_connection_session` with
`newConnectionRecovery=true` proves the new
connection can submit two valid reads, but it still does not by itself prove
the PLC's internal orphan/late-callback path. WPF always records
`orphanQualified=false`; that matrix needs separate PLC instrumentation and/or
approved capture evidence. `new_diagnostics_boot_session` is a separate PLC
Boot recovery case.

Suggested capture name:

`23a_SDO_Abort_Recovery_7E50_03.pcapng`

### 4.3 Bulk qualification

Run the non-fault cases first:

1. `Run 24-entry Snapshot Soak`
2. `Run Configure/Read/Release Soak`

PASS requires the exact 24-entry Catalog topology, valid entries, stable
identity, requested iteration count, and confirmed release.

Run `Run One-Slave-Offline Partial` only after all four axes are powered off
and stationary. At the WPF checkpoint, change exactly one approved slave to
offline, then use the UI resume action. The partial snapshot must mark exactly
that slave's six entries offline while the other 18 remain valid. Restore the
same slave, resume again, and require all 24 entries to recover as valid.

Do not disconnect multiple slaves or change WPF/PLC source during the
checkpoint.

### 4.4 Recorder qualification

Run in this order:

1. `Run Single Manual`
2. `Run Ring Forced Trigger`
3. `Run Trigger Lifecycle Soak` with a small iteration count
4. `Run Reconnect Exact Adopt`
5. `Run Reconnect 0/0 Discovery` only when the advertised single-bank
   capability permits it

PASS requires terminal state/metadata, contiguous chunk download, stable
identity and hashes, and confirmed cleanup. A Recorder `Fault`, unknown adopted
identity, or failed release is not PASS; preserve the resource and report the
recovery-required log.

### 4.5 Motion qualification last

Run Group motion qualification only after the user's simple axis/group drive
test is already stable:

1. `Run Enable ACK -> Locked`
2. `Run Buffered A -> B`
3. `Run Deterministic Stop-First`

Stop on any unexpected position, group state, axis membership, PowerOn state,
or profile-lock transition. Do not interpret an accepted command ACK as final
motion completion.

## 5. Report format

Report each failure without retrying over it:

```text
Scenario:
WPF configuration: Debug/Release
PLC project/build identity:
Axis/slave/group:
QTEST final event:
Observed state/outcome/error/detail:
Connection state and BootId:
PCAP file or NOT_CAPTURED:
Exact error text:
```

For a build failure, send the first compiler error and the final error count.
For a live failure, send the full saved QTEST log; a screenshot alone is not
enough to reconstruct ticket, BootId, and cleanup state.

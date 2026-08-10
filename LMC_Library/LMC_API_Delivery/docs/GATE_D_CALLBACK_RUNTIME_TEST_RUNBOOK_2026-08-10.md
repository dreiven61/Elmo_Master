# Gate D D5 callback runtime test runbook

Date: 2026-08-10

## Evidence boundary

Gate D adds a non-authoritative UDP wake for a terminal D5 operation. UDP never
completes an operation. Only the generation-pinned TCP `0x7E03` response may
update the retained ticket, UI, or journal.

On 2026-08-10 the LASAL IDE ran an incremental C78/ARM `Build project`. It
compiled the three changed classes and finished the internal link with zero
compiler errors. The source-warning histogram was `W0069=28`, `W0070=21`,
`W0072=11`. The first Download and PLC link reported `Download Ok`. A second
Download was aborted after a CPU-state timeout; a later connection succeeded
and the PLC reported `Project successfully loaded`. This is useful incremental
Build/Download evidence, but it is not a strict clean C78 Rebuild checkpoint and
it is not callback runtime proof.

The later LASAL PID 4832 session issued two `Rebuild project` commands. The first
is invalid because two `Classes.lcb` persistence error records report an
`ios_base::failure` and a write failure. The bounded second Rebuild window is a
clean C78/ARM source build with 76 coded warnings (`W0069=35`, `W0070=21`,
`W0072=17`, `W0073=3`), zero source errors, `Compiler Done`, `Linker Done`, and
zero `CInvalidArgException`. Its generated `Classes.lcb` is 8,549,773 bytes and
has SHA-256
`3AC3D938DC1520FAEA6C3693161ABDB280CC873A97C60CF79B3F716C7F064C22`.
The focused `VerifyCurrent` check exits zero and reports the actual tree as a
`CAPTURE TerminalWakeBrokerCandidate` static state. The bootstrap `ValidateOnly`
result at that historical stage was `UNTRUSTED` with `outputCreated=false`; that
run did not create a sequence-4 physical checkpoint or grant production approval.

PID 4832 is not the required isolated strict-build evidence session. It contains
the failed first Rebuild, the clean second Rebuild, and later Connect, Reset, and
Restart actions. There was no post-build `Find in Implementation` action and no
Download in this session. That Find action applies only to Object Network
Server/Client rows and is not applicable to ordinary class method rows, so its
absence is not an incomplete-method gate for the three Gate D methods.
Reset/Restart only ran the old PLC image. A live Gate D callback from that
artifact therefore remained untested.

The retained pre-commit strict checkpoint is the `GateDVisualLayout` PID 480 / Rebuild TID
3396 session. It records one canonical project load, exactly one C78/ARM
`Rebuild project`, no Connect or Download, and a normal project close/IDE exit.
The accepted command window has 76 coded warnings (`W0069=35`, `W0070=21`,
`W0072=17`, `W0073=3`), zero errors, `Compiler Done` twice, `Linker Done` once,
six post-result C82 compatibility warnings, and zero `CInvalidArgException`.
At that checkpoint identity, `VerifyBuild` reported
`C78/ARM errors=0 warnings=76 compilerDone=2 linkerDone=1
postResultCompatibilityWarnings=6/C82 profile=GateDVisualLayout
inputsEquivalent=true; rawInputsUnchanged=10/10 replayEquivalentSt=0
regeneratedOutputsBound=2 evidenceSource=bounded-repository`.

The retained baseline is 6,887 bytes with SHA-256
`247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE`.
Its exact raw range `[6532176,7298848)` is 766,672 bytes with SHA-256
`B918E51279360E27780D212650361AF361FFFC391C5F24854447BE0F3F9ABD17`;
the 1,574-byte sidecar manifest has SHA-256
`7928BC0D641FEA79444EDE8AD49FC10C15C28D453DB75DAF82C21B9D303D1DFC`.
The derived transcript is 30,111 bytes with SHA-256
`F32122D318DBFD8F53BC9E5AD0FF693F9B6F05368D40FC64138A010A1BC810AF`.
The Rebuild/checkpoint-bound `Classes.lcb` is 8,549,773 bytes with SHA-256
`24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`;
the second regenerated output, `Network/Networks.lcb`, is 242,363 bytes with
SHA-256
`C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F`.

PID 7288 and its D71E... `Classes.lcb` remain historical superseded evidence.
They must not be used as the current Gate D identity.

The checkpoint-focused verifier is 545,566 canonical-LF bytes with SHA-256
`FBF1A8582E85039377AC39F26D8BBA64C0EB62665424DE150083CFC412CC7CA3`.
The capture self-test passes positive `46` and negative `94`. The earlier
bootstrap `ValidateOnly` passed that tree as `UNTRUSTED` with
`outputCreated=false`; it planned
`gate_d_terminal_wake_broker_candidate_checkpoint.json` at 3,225,878 bytes and
SHA-256
`E0490DC348B861FBE47AB4C2E9C558BE679E865787A014860EBA45B3E0E508E4`.
That bootstrap run created no physical manifest.

Trust-anchor commit `bb5fd93` was followed by commit `5543579`, which atomically
committed the sequence-4 physical manifest plus the exact seven production paths
listed below. The manifest binds `Classes.lcb` to `24402BFA...` and records
`ProductionApproved=false`, `NeedsRebaseline=true`.

After `5543579`, LASAL PID 34656 ran C78/ARM `Rebuild project`, compiled
`LMCDiagnosticsService`, `LMCUdpCallbackSender`, and `TCPMotionInterface`, and
reported `Compiler Done`, `Linker Done`, and command success. Download then
reported `Download Ok` and `Project successfully loaded`. A later Reset/Restart
also succeeded and loaded the project again. These are IDE/online-operation facts,
not callback causal proof.

That Rebuild regenerated current `Classes.lcb` as 8,549,773 bytes with SHA-256
`6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712`,
which differs from the sequence-4 checkpoint `24402BFA...`. Current focused
`VerifyCurrent` and C78 input-equivalence checks therefore fail. The exact diff
is 99 bytes in 58 contiguous runs across 36 opaque vendor class records. The
four Gate D class records and protected dependency records are byte-exact, but
the changed vendor fields and generator checksum semantics remain undefined.
Do not normalize, allowlist, or pin `6E115876...` as semantically equivalent
from this evidence. All callback observations against this artifact are
exploratory until the checkpoint identity is reproduced or a separate reviewed
strict-evidence transition accepts the regenerated artifact; the decision
remains `ProductionApproved=false` and `NeedsRebaseline=true`.

PC reconnect correction commit `66b5cf2` preserves the exact short-failure
`ErrorId=-1` and only for the canonical v2 failure envelope waits 20 ms and
retries `0x8080` once
on the same socket. Legacy and other failures do not retry. Current Release PC
evidence is SDK `1117/1117` and WPF `334/334`. The GUI retains the RPC-init
attempt count, canonical-retry decision, and final ACK evidence after cleanup,
and displays the accepted version-2 BootId, SessionEpoch, cookie, listener
generation, expected source, event mask, PC receiver counters, and last receiver
decision. The WPF total includes a deterministic old-session statistics action
queued across connection replacement; it cannot alter the replacement owner,
counters, last decision, or listener summary. These values are PC-side evidence
only; they do not replace the pcap,
PLC `RpcCallbackLastDisarmResult`, or PLC producer/sender counters. Negative PLC
disarm preservation remains intentional and fail-closed; do not force-clear the
callback tuple.

Before formal Gate D runtime qualification, preserve this sequence/evidence split:

1. PID 480 / TID 3396 has supplied the `GateDVisualLayout` one-Rebuild raw log,
   and the exact
   bounded delta retained in the repository has passed `VerifyBuild`. The
   mutable local log is no longer required to replay this build evidence. Do not
   rerun Rebuild.
2. PID 480 contains no method-specific UI proof; that remains a fact about the
   isolated Rebuild session. `Find in Implementation` applies
   only to Object Network Server/Client rows and is not applicable to these class
   method rows. The user separately attested that the row-level Find action works
   normally; that does not prove a method row was opened. For a method row, use
   `Edit Method`, Enter, or a direct open and confirm the exact Implementation
   tab/header. The user has contemporaneously
   attested that `LMCDiagnosticsService::TryTakeD5TerminalWake`,
   `LMCUdpCallbackSender::PublishEvent`, and
   `TCPMotionInterface::PublishD5TerminalWake` each opened with the correct
   Implementation display and that LASAL was then closed. Record this UI check as
   `exactMethodOpen=manual-attested`; do not ask the user to repeat it merely
   because the Rebuild session has no Find action. `Lasal2.log` records only a
   class-level Open Implementation token, which can also result from automatic
   session restore, and cannot prove the selected method. A separate automated
   method-smoke JSON/log result remains pending and nonblocking; it must be
   labeled as automated evidence, not as a prerequisite that invalidates the
   manual attestation.
3. Completed: `bb5fd93` froze the reviewed trust-anchor tools and `5543579`
   committed the trusted sequence-4 physical manifest atomically with these
   exact seven production transition paths:
   `Class/Classes.lcb`,
   `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`,
   `Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`,
   `Class/TCPMotionInterface/TCPMotionInterface.st`,
   `Class/_UDPTransceiver/_UDPTransceiver.st`,
   `Network/Comm_Network/Comm_Network.lcn`, and `Network/Networks.lcb`.
4. Completed but not identity-equivalent: PID 34656 performed the post-commit
   Rebuild/Download and later Reset/Restart, while that Rebuild changed
   `Classes.lcb` from manifest identity `24402BFA...` to `6E115876...`. Do not
   rerun Download merely to repeat this step, and do not rebaseline the opaque
   99-byte vendor-record drift by hash alone. First reproduce the checkpoint
   identity or complete a separate reviewed strict-evidence transition; then
   collect fresh BootId, counter deltas, WPF log, and packet trace for formal
   qualification.

The retained pre-drift C78 evidence was replayed from the canonical repository
root with:

```powershell
& 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalC78RebuildEvidence.ps1' `
  -VerifyBuild `
  -EvidenceProfile GateDVisualLayout `
  -RepositoryRoot (Get-Location).Path `
  -EvidencePath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/build_baseline_gate_d_visual_layout.json' `
  -BuildTranscriptPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/derived_build_transcript_gate_d_visual_layout.txt' `
  -BoundedLogDeltaPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/bounded_lasal2_delta_gate_d_visual_layout.raw.txt' `
  -BoundedLogDeltaManifestPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/bounded_lasal2_delta_gate_d_visual_layout.manifest.json'
```

At the checkpoint identity, the exact command above was rerun with
`-RunFullStatic` and exited `0` in 247.8 seconds. A current rerun is expected to
fail input equivalence until `Classes.lcb` is rebaselined. The historical passing
run emitted both
`PASS LASAL.StaticContract (Phase5TransportClean; ... diagnostics D1-D5 ...)`
and
`PASS LASAL.C78RebuildEvidence.Verify ... profile=GateDVisualLayout
inputsEquivalent=true; rawInputsUnchanged=10/10 replayEquivalentSt=0
regeneratedOutputsBound=2 evidenceSource=bounded-repository`.

The first full-static attempt exposed a local-scope defect in the verification
tool, not a production-source failure: `$stage87AdapterCallPattern` was defined
in the wrong function-local scope in `Verify-LasalContract.ps1`. The definition
was moved into `Assert-LasalDs402OwnerReceiptProviderMutationFences`, after which
Windows PowerShell 5.1 and PowerShell 7 AST checks and strict self-test `67/67`
passed before the successful rerun.

Any callback run while current `Classes.lcb` remains outside the sequence-4
identity is exploratory evidence and must be labelled as such. The current
decision remains `ProductionApproved=false` and `NeedsRebaseline=true`.

Those decision flags are release/evidence metadata, not a runtime feature gate.
The broker is executable behind its session/BootId fences after Download, and
the WPF application activates version 2 only through explicit opt-in.

Keep these evidence classes separate:

- `Actual PLC`: packet and counter evidence from the downloaded PLC;
- `Hybrid`: an actual PLC packet deliberately dropped, delayed, duplicated, or
  reordered by an approved network proxy;
- `PC fake`: codec, fence, WPF, or fake-RPC/fake-UDP evidence with no PLC
  producer;
- `Static/IDE`: source, verifier, C78 Build/Rebuild, or generated-file evidence.

The repository has PC fake-peer tests. It does not provide a production-network
packet injector. Do not describe fake-peer results as PLC runtime proof.

## Safe test setup

Before any Download or takeover case, obtain a site-approved maintenance window.
All axes, coordinated groups, and the robot must be idle and powered off, with no
active or queued motion/diagnostics command, recorder transfer, recovery action,
or safety-drain operation. The single-axis Standstill check below is additional
read-only test setup; it does not replace this system-wide prerequisite.

1. Use the WPF test application built from the source under qualification. Its
   connection explicitly requests callback version 2 with event mask bit `1`
   and maximum datagram size `52`.
2. Select an axis whose drive is powered off and in stable Standstill. Record
   the read-only status evidence used for this check. Do not use SDO Write for
   any test in this runbook.
3. In the low-level SDO panel select `Read`, Slave `1..4`, index `0x6061`,
   sub-index `0`, `Int8`, length `1`, timeout `1000`, and use only
   `Submit SDO Read` for the causal callback cases. Do not use
   `Read SDO Inline (wait terminal)` in those cases: that helper polls by itself
   and cannot prove the UDP-to-TCP causal query.
4. Start a packet capture for the control TCP connection and the advertised UDP
   callback port. Preserve the raw pcapng and the WPF execution log. If a host
   firewall drops UDP, note that a host-side capture may still show the incoming
   datagram; prove the drop by the receiver counters, lack of WPF dispatch, and
   lack of an automatic `0x7E03`.
5. After Connect, confirm `Connected` and
   `Listening <endpoint>, rejected=<count>`. Record the GUI RPC-init evidence
   (`0x8080Attempts`, `Retry`, and retained `LastACK`) and version-2 registration
   evidence (BootId, SessionEpoch, cookie, listener generation, expected source,
   and event mask). In the capture, still verify the exact 32-byte version-2
   `0x405C` request and successful 20-byte response, including nonzero accepted
   Diagnostics BootId and SessionEpoch. The GUI values are parsed PC evidence
   and do not prove the accepted PLC wire exchange by themselves.
6. Record all counters as before/after deltas. Do not reset or force private PLC
   variables. When LASAL Online watch is available, include:
   - `TCPMotionInterface1.SessionEpoch`
   - `TCPMotionInterface1.PendingClosedSessionEpoch`
   - `TCPMotionInterface1.RpcCallbackRegistered`
   - `TCPMotionInterface1.RpcCallbackProtocolVersion`
   - `TCPMotionInterface1.RpcCallbackSessionEpoch`
   - `TCPMotionInterface1.RpcCallbackBootId`
   - `TCPMotionInterface1.RpcCallbackLastDisarmResult`
   - `TCPMotionInterface1.D5TerminalWakeAttemptCount`
   - `TCPMotionInterface1.D5TerminalWakeEnqueuedCount`
   - `TCPMotionInterface1.D5TerminalWakeRejectedCount`
   - `TCPMotionInterface1.TakeoverCount`
   - `TCPMotionInterface1.TakeoverRejectCount`
   - `TCPMotionInterface1.LastTakeoverResult`
   - `LMCUdpCallbackSender1.QueueDepth`
   - `LMCUdpCallbackSender1.QueuedCount`
   - `LMCUdpCallbackSender1.RingAcceptedCount`
   - `LMCUdpCallbackSender1.AdmissionRetryCount`
   - `LMCUdpCallbackSender1.QueueFullDropCount`
   - `LMCUdpCallbackSender1.AdmissionErrorDropCount`
   - `LMCUdpCallbackSender1.DisarmClearedCount`
   - `LMCUdpCallbackSender1.TransportErrorCount`
   - `LMCUdpCallbackSender1.LastAdmissionResult`
7. The WPF callback diagnostics expose `AcceptedCallbackWakeHintCount`,
   `RejectedCallbackCount`, `DuplicateCallbackWakeHintCount`,
   `OutOfOrderCallbackWakeHintCount`, and the last receiver decision/protocol
   error. Record their before/after values. They are PC receiver evidence and do
   not replace the pcap or PLC producer/sender counters. A WPF semantic drop
   after a valid envelope is not added to `RejectedCallbackCount`.

For a bounded interval in which none of the three producer counters saturates,
the deltas must satisfy:

```text
delta(D5TerminalWakeAttemptCount)
  = delta(D5TerminalWakeEnqueuedCount)
  + delta(D5TerminalWakeRejectedCount)
```

`RingAcceptedCount` proves vendor-ring admission, not an emitted network packet.
Only the packet capture can supply that wire evidence.

## P0: actual PLC cases

### GD-01 normal Completed wake

1. Connect the WPF application and complete the registration checks above.
2. Enter the read-only `0x6061:0 Int8/1`, timeout `1000` request and press
   `Submit SDO Read` once.
3. Do not press `Refresh Ticket` or `Read SDO Inline (wait terminal)` while
   waiting.

PASS requires all of the following:

- the submit returns one nonzero D5 TicketId;
- the PLC-origin capture contains exactly one valid 52-byte UDP datagram for
  that TicketId;
- the WPF log contains the exact prefixes
  `D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x`
  and
  `Callback D5 authoritative TCP status processed. TicketId=0x`
  for the same TicketId;
- exactly one callback-triggered TCP `0x7E03` query follows the UDP hint;
- receiving UDP alone does not change the operation state; the UI changes only
  after the TCP response;
- `TextOperationState` becomes `Callback D5 status refresh completed`;
- the operation summary contains `State=Completed, Outcome=Success`,
  `ResultType=Int8`, `ResultLength=1`, and the one-byte result;
- Attempt and Enqueued each increase by one; Rejected does not increase;
- sender Queued and RingAccepted each increase by one, QueueDepth returns to
  zero, and no drop/error counter increases. AdmissionRetry normally remains
  unchanged; a bounded `-4` admission retry must be recorded separately and may
  not produce an extra datagram. It makes the transport run non-clean, but does
  not by itself invalidate a single causal UDP-to-TCP result.

If the valid wake arrives while the WPF is still busy completing Submit, the
expected log is
`D5 terminal wake skipped while busy; manual/poll refresh remains available. TicketId=0x`.
That run proves a valid wake and the busy fence, but it is INCONCLUSIVE for the
automatic causal `0x7E03` requirement. Use GD-05A to recover the ticket and
repeat GD-01 with a distinct ticket; do not resubmit the same operation
automatically.

The UDP hint can also arrive before the awaited Submit continuation has retained
the returned ticket. In that race the SDK envelope is accepted, but WPF logs
`D5 terminal wake ignored: no exact current retained ticket, EventId=0x`, sends
no automatic `0x7E03`, and must not construct a ticket from UDP. Classify this
run as INCONCLUSIVE, wait for Submit to return and retain its exact ticket, use
GD-05A once to recover it, then repeat GD-01 with a distinct ticket.

### GD-02 two distinct tickets

Run the same low-level `Submit SDO Read` twice, waiting until the first ticket's
authoritative TCP response has made it terminal before submitting the second.

PASS requires two distinct nonzero TicketIds, one PLC-origin UDP wake per
TicketId, one authoritative `0x7E03` per TicketId, and no replay of the first
ticket. Sequence may advance by more than one if an earlier sender admission was
dropped; a forward gap is not itself a receiver rejection.

### GD-03 conditional Failed wake and recovery

Do not run this case until the drive manufacturer or the approved site test plan
identifies the exact nonexistent read-only object/sub-index and its expected SDO
abort. This repository deliberately does not invent that target. Without that
approval, record GD-03 as `NOT RUN`, not FAIL or PASS.

When an approved target exists:

1. Capture a valid `0x6061:0 Int8/1` baseline.
2. Enter the approved nonexistent read-only object/sub-index and press low-level
   `Submit SDO Read` once. Do not use the automatic abort runner for the causal
   measurement because that runner also polls.
3. After the failed ticket has been collected, submit the valid
   `0x6061:0 Int8/1` request as a distinct recovery ticket.

PASS requires the approved abort ticket to reach `State=Failed,
Outcome=Failed`, preserve the expected raw EtherCAT abort, and produce one valid
wake plus one authoritative status query. The recovery ticket must reach
`Completed/Success` and produce its own single wake. Value equality with the
baseline is a separate same-value qualification and is required only if the
approved site plan freezes the drive mode during this case.

### GD-04 reconnect/session fence

1. Complete GD-01.
2. Close and reconnect the WPF application.
3. Verify a new local `LMCConnection`, a new accepted callback SessionEpoch and
   cookie, and a fresh successful `0x405C` registration. Do not infer these from
   the WPF connection label alone.
4. Submit a new low-level `0x6061:0 Int8/1` read.

PASS requires the new ticket to complete normally through the new callback
tuple. Replaying a previous-session packet is a separate PC fake or approved
proxy case: it must be rejected by the listener/session fence and must not
create a TCP `0x7E03` or change the current UI.

An old D5 ticket is not queryable from the new WPF connection. Session close may
clear or orphan that ticket. Use the existing disconnect/orphan recovery flow
and a new read; never treat reconnect as authority to query or replay the old
ticket.

### GD-05A lost wake and manual fallback

Use a reversible firewall rule scoped only to the advertised UDP callback port,
or an approved proxy, to drop one valid wake while leaving the control TCP
connection untouched. Record the rule/proxy configuration and remove it after
the case.

PASS requires:

- the PLC producer records one Attempt and one Enqueued result;
- PLC-side or pre-drop capture shows the valid LMC2 packet;
- the PC records no accepted wake and sends no automatic `0x7E03`;
- UDP alone causes no terminal UI update;
- clicking `Refresh Ticket` once sends one exact `0x7E03`, sends no new
  `0x7E50`, and applies the terminal TCP response;
- restoring UDP does not replay the already attempted PLC wake.

Manual `Refresh Ticket` is the fallback for this retained ticket. The Inline
helper submits and polls its own ticket; it does not recover this one.

### GD-05B independent polling fallback

With UDP still deliberately unavailable, start a separate
`Read SDO Inline (wait terminal)` for `0x6061:0 Int8/1`. PASS requires its own
nonzero ticket and terminal result through bounded TCP `0x7E03` polling, with no
PLC CancelOperation and no automatic `0x7E50` replay. This proves the polling
fallback, not UDP-to-TCP causality.

If a terminal slot was replaced before status retrieval, `TicketNotFound` is a
stale-hint result. It is not a connection fault and it does not establish a new
operation outcome.

### GD-06 clean Close and callback disarm

1. Connect and verify an armed version-2 callback tuple.
2. With no intentionally pending D5 ticket, press WPF `Close` once.
3. Capture the TCP `0x405D` exchange and the subsequent connection teardown.

PASS requires `RpcCallbackLastDisarmResult` to be `0` (matched clear) or `1`
(already disarmed/empty), the RPC callback tuple to be cleared, sender
QueueDepth to be zero, and the WPF to show a stopped/disconnected listener.
`DisarmClearedCount` increases only by the queue depth actually cleared.

Do not require Attempt or Rejected to increase on a clean close. CyWork may
notify Diagnostics and clear/orphan the old ticket before the broker can claim
it. In a separately captured race, if a terminal tuple was already claimed
after the local publish fence closed, the allowed producer delta is Attempt
`+1`, Rejected `+1`, Enqueued `+0`. If it was already enqueued before disarm,
the sender may instead clear that queued frame. Classify the branch from the
packet and counter evidence; do not force it.

## P1: negative and lifecycle matrix

Ordinary WPF buttons cannot safely generate malformed envelopes, EventId zero,
or internal sender mismatches. Use the mode stated below. Never force private
PLC words in the production project.

| ID | Mode | Condition | Required result |
|---|---|---|---|
| GD-N01 | PC fake or approved proxy | Exact duplicate UDP packet, same sequence | First copy may dispatch. Second copy adds `RejectedCallbackCount +1` and `DuplicateCallbackWakeHintCount +1`; no WPF handler call, second TCP query, or UI transition. PLC producer counters do not change because of replay. |
| GD-N01B | PC fake | Same current TicketId in two valid packets with distinct forward sequences while the first `0x7E03` is held | Both envelopes count as SDK accepted. WPF logs `D5 terminal wake skipped while busy; manual/poll refresh remains available. TicketId=0x` for the second and sends only one `0x7E03`. |
| GD-N02 | PC fake or approved proxy | Sequence `N+1` accepted before older `N` | The older packet adds `RejectedCallbackCount +1` and `OutOfOrderCallbackWakeHintCount +1`; no handler call or additional TCP query. The first valid packet is a baseline, so merely starting above sequence 1 is not reorder. |
| GD-N03 | Hybrid or PC fake | Drop sequence `N`, then deliver valid higher sequence | The higher sequence is forward and may be accepted. No duplicate, out-of-order, or dedicated loss counter increases. In Hybrid mode recover the actual retained ticket through GD-05A. In PC-fake mode verify only the forward-gap fence/counters; do not claim PLC runtime or ticket recovery. |
| GD-N04 | PC fake | Valid current envelope with `EventId=0` | Parser rejects `EventIdentifierNotApproved`; aggregate Rejected increases, application dispatch and TCP query remain zero. This proves the PC fence only. |
| GD-N04P | Static/IDE, or separately approved PLC test harness | Direct sender policy check with `EventId=0`, an armed endpoint, matching `ProducerSessionEpoch`, and a valid zero-payload tuple | Production Diagnostics cannot emit this tuple because zero TicketId is not claimable. Under these preconditions the current source requires `PublishEvent` result `-6` before queue/sequence mutation. An unarmed endpoint, stale epoch, or invalid payload returns the earlier `-4`, `-8`, or `-7` and is a different case. Runtime proof requires a dedicated harness and must not be claimed from the production broker path. |
| GD-N05 | PC fake | Valid current envelope but wrong nonzero TicketId | `AcceptedCallbackWakeHintCount +1`; `RejectedCallbackCount` does not increase. WPF logs the exact template `D5 terminal wake ignored: no exact current retained ticket, EventId=0x{EVENT_ID_8HEX}, BootId=0x{BOOT_ID_8HEX}`, creates no ticket, sends no `0x7E03`, and does not mutate UI state. |
| GD-N06 | PC fake or approved proxy | Wrong BootId | `RejectedCallbackCount +1` with `StaleBootId`; no application dispatch, TCP query, or retained-ticket change. |
| GD-N07 | PC fake or approved proxy | Old SessionEpoch or cookie | Aggregate Rejected increases before application dispatch; no TCP query or UI mutation. |
| GD-N08 | PC fake or approved proxy | Foreign source IPv4 | Aggregate Rejected increases with unexpected-source decision even if the envelope is otherwise valid; no application dispatch. |
| GD-N09 | PC fake | Wrong event type, mask, delivery class, payload length, or flags | Parser/policy rejection; no authoritative query or UI mutation. |
| GD-N10A | PC fake, or a separately approved raw-registration harness against the actual PLC | Registration uses TCP source IPv4 A but advertises different valid callback IPv4 B | First prove both addresses in the capture. `0x405C` then returns failure and does not arm the new tuple. Any previously armed different tuple/FIFO is preserved, but proving preservation requires a harness that can re-register on the same session. The current WPF cannot create this mismatch because its selected local address binds both the TCP source and advertised callback address; changing that field is not this test. |
| GD-N10B | Separately approved PLC test harness | Claimed terminal tuple while CallbackSender is unavailable or the local RPC/session/owner/BootId tuple mismatches | Broker Attempt `+1`, Rejected `+1`, Enqueued `+0`; sender queue and wire unchanged; no retry of that tuple after recovery. This condition is not safely operator-generated by the current WPF. |
| GD-N11 | Actual PLC race, exploratory | Terminal tuple during pending close | No enqueue to the retiring session. If close notification clears/orphans before claim, all three producer deltas may be zero. If the broker already claims behind a closed local fence, Attempt `+1`, Rejected `+1`, Enqueued `+0`. Ordinary WPF controls may not create this timing deterministically; never force private words or require the latter branch unconditionally. |
| GD-N12 | Actual PLC | Clean WPF Close/disarm | Use GD-06. Old-session callback state is cleared and no late packet may update the new/current UI. Do not claim that an old ticket remains queryable after reconnect. |
| GD-N13 | Actual PLC with a same-host approved second raw client or separate Windows session; a site-approved NAT setup is allowed only when callback UDP routability is also proved | Concurrent same-IP takeover under the system-wide maintenance prerequisite above | The WPF process mutex prevents two ordinary instances in one Windows session, so that is not a valid setup. Never assign a duplicate static IP to two hosts. Prove in the capture that both concurrent TCP clients present the exact same source IPv4 and that the registered callback path remains routable. With valid peer lookup, `TakeoverCount +1` and `LastTakeoverResult=2`; old callback is disarmed, SessionEpoch advances, and only a freshly initialized/registered new owner may wake. A late old-socket disconnect must not clear the new owner. |
| GD-N14 | Actual PLC with an approved second raw client, two Windows sessions, or two hosts that present genuinely different TCP source IPv4 addresses | Concurrent different-IP takeover attempt under the system-wide maintenance prerequisite above | The WPF process mutex prevents two ordinary instances in one Windows session. Prove both concurrent source IPv4 addresses in the capture. With both peer lookups valid, `TakeoverRejectCount +1` and `LastTakeoverResult=-4`; candidate closes and active owner/callback tuple remains unchanged. Editing only the WPF local-IP text or reconnecting one client is not a concurrent different-peer takeover test. |

The PLC sender's `QueuedCount` and `RingAcceptedCount` are unrelated to PC
duplicate/reorder counters. Likewise, a wrong-ticket WPF semantic drop is not a
PLC producer rejection.

## Exact UDP packet checks

All fields are little-endian. For the initial Gate D event, the datagram must be
exactly 52 bytes:

| Offset | Field | Required value |
|---:|---|---|
| 0 | Magic | ASCII `LMC2` |
| 4 | ProtocolVersion | `2` |
| 6 | HeaderBytes | `52` |
| 8 | DatagramBytes | `52` |
| 10 | EventType | `1` |
| 12 | EventMaskBit | `1` |
| 16 | BootId | exact accepted Diagnostics BootId |
| 20 | SessionEpoch | exact accepted callback session |
| 24/28 | CookieLo/CookieHi | exact registration-request cookie |
| 32/36 | SequenceLo/SequenceHi | sender sequence; first new-arm enqueue is `1` |
| 40 | EventId | exact nonzero D5 TicketId |
| 44 | PlcTimeMs | enqueue-time `ops.tAbsolute` snapshot |
| 48 | PayloadBytes | `0` |
| 50 | DeliveryClass | `0` |
| 51 | Flags | `0` |

An exact duplicate registration preserves the next sequence. A disarm followed
by a new arm resets it to `1`. Queue-full rejection consumes no sequence;
admission drop may leave a forward gap.

## Evidence to return

For each executed case, save:

- case ID, evidence class (`Actual PLC`, `Hybrid`, `PC fake`, or `Static/IDE`),
  and start/end time;
- exact source/build/checkpoint identity and downloaded PLC BootId;
- WPF execution log;
- pcapng and the capture location relative to any firewall/proxy;
- submitted TicketId, BootId, callback SessionEpoch, and cookie;
- before/after PLC producer/sender and PC receiver counters;
- final `0x7E03` response state, outcome, error, detail, type, length, and data;
- the exact reversible fault-injection rule, proxy action, or fake-peer test used
  and confirmation that it was removed/stopped;
- `PASS`, `FAIL`, `INCONCLUSIVE`, or `NOT RUN`, with the first mismatching or
  unavailable field.

Static, PC fake-peer, C78 Build/Rebuild, PLC Download, and live callback packet
results must remain separate in the final qualification report.

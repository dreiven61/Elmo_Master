# Callback listener design

Date: 2026-07-09

P0 ownership/session-provenance update: 2026-07-31

UDP sender implementation handoff: 2026-08-07

Gate A import/build evidence: 2026-08-07

Gate B1/B2 and Gate C candidate evidence: 2026-08-08

## Reason

`RpcInitConnection` now sends the captured RPC callback registration frame
`0x405C`. That frame contains:

- event mask
- callback port
- local PC IPv4 address

If the library registers a callback endpoint but does not listen on that
endpoint, the RPC connection is only partially implemented. Any controller-side
asynchronous event or notification sent to that endpoint would be lost.

## Decision

`LMCConnection` owns two runtime resources:

1. Command TCP socket
2. Callback listener socket

The command TCP socket is used for request/response packets:

- `0x8080` session init
- `0x405C` callback registration
- axis/group lookup
- motion/read/control commands
- `0x405D` close connection

The callback listener is opened before sending `0x405C`, so the advertised
callback endpoint is already available when the controller accepts the callback
registration.

## Transport

The callback transport is UDP. This is confirmed by the Maestro API manual:
the connection parameter names the callback port as `uiCbUdpPort`, and the
callback function is described as a UDP callback.

The captured `0x405C` frame confirms that the controller receives a PC IPv4
address and UDP port. No actual Maestro callback datagram has been captured.
The existing listener therefore continues to expose legacy payloads as raw
bytes. The `LMC2` format below is an explicitly approved project-local version-2
schema, not a reverse-engineered Maestro payload. It is available only through
an explicit PC opt-in; it is not the default and has no production PLC publisher.

The active/default legacy `0x405C` wire shape is unchanged: its payload remains
exactly 12 bytes (`event mask UDINT + callback port DINT + IPv4 BYTE[4]`) and the
response remains the existing 4-byte command-result ACK. The P0 ownership change
itself did not introduce a new command, payload field, or typed event schema.

## PLC endpoint ownership

The tracked `TCPMotionInterface` treats the callback endpoint as part of the
current TCP RPC session:

- `CurrentPeerValid` must be true.
- The requested callback IPv4 must exactly equal the current TCP peer IPv4.
- The requested UDP port must be in `1..65535`.
- The handler parses into temporary values and commits the first valid
  `(event mask, port, IPv4)` tuple only after every check passes.
- Repeating the exact accepted tuple is idempotent.
- A different re-registration is rejected and the previously accepted tuple is
  preserved unchanged.

Thus a client cannot register a third-party callback address, change the
accepted endpoint in place, or destroy a valid registration with a malformed
retry. A new tuple requires a new RPC session.

The byte-order comparison has static vendor evidence. The installed SIGMATEK
`GetBroadCastData.st` example converts an IPv4 `UDINT` to text by passing the
least-significant byte first, followed by shifts of 8, 16, and 24 bits, to
`OS_TCP_USER_TOIP`. This is consistent with copying the four `0x405C` IPv4 bytes
into a LASAL `UDINT` and comparing that value with `OS_TCP_USER_GETPEERIP`.
This evidence is source-level only; a PLC download and packet capture must still
prove the comparison on the target runtime.

## Public API

`LMCConnection` exposes:

- `CallbackReceived`: raw callback payload event
- `CallbackListenerError`: background listener error event
- `IsCallbackListenerRunning`
- `CallbackLocalEndPoint`
- `RejectedCallbackCount`
- `LMCConnectionOptions.ValidateCallbackSourceAddress` (default `true`)
- `LMCCallbackEventArgs.SessionGeneration`
- `LMCCallbackEventArgs.BelongsTo(connection)`
- `LMCCallbackEventArgs.BelongsToCurrentSession(connection)`
- `LMCConnectionOptions.CallbackRegistrationMode` (default `LegacyRaw`)
- `LMCConnectionOptions.CallbackRequestedMaxDatagramBytes` (default `512`)
- `CallbackWakeHintReceived`: typed version-2 wake-hint event
- `RpcCallbackRegistrationV2Response`: immutable accepted registration data
- `RejectedCallbackCount` (shared raw/version-2 total),
  `AcceptedCallbackWakeHintCount`, `DuplicateCallbackWakeHintCount`, and
  `OutOfOrderCallbackWakeHintCount`
- `LMCCallbackWakeHintEventArgs.WakeHint`, `RemoteEndPoint`, `ReceivedAtUtc`, and
  `SessionGeneration`
- `LMCCallbackWakeHintEventArgs.BelongsTo(connection)` and
  `BelongsToCurrentSession(connection)`

The payload is intentionally raw bytes. `LMCCallbackEventArgs.Payload` returns
a defensive copy, and the default listener accepts datagrams only from the
configured controller IPv4. Do not parse the bytes until real callback captures
exist. `CallbackReceived` is a `LegacyRaw` event only and is never raised by the
version-2 path. Both raw and typed event endpoint properties, and
`CallbackLocalEndPoint`, return defensive endpoint copies.

Each raw callback event also carries the RPC session generation captured by its
owning listener. `BelongsTo` checks the owning `LMCConnection` instance;
`BelongsToCurrentSession` additionally requires that the captured generation is
still the connection's current session. These members establish provenance for
raw bytes only and do not assign those bytes a typed meaning.

## Lifecycle

### Open

`RpcInitConnection(...)` performs this order:

1. Validate all new connection addresses, ports and options. Invalid reconnect
   input leaves the current session running.
2. Close any previous command socket and callback listener.
3. Open command TCP socket.
4. Send `0x8080`.
5. Bind callback UDP on `localAddress:callbackPort`.
6. Read the listener's actual bound port. This differs when the caller requested
   port `0` and the OS selected an ephemeral port.
7. In `LegacyRaw` mode, start the existing receive thread and exchange the exact
   12-byte `0x405C` request and 4-byte ACK.
8. In `Version2WakeHint` mode, exchange the exact 32-byte request and 20-byte
   response, validate and install the accepted fence, then start a receive thread
   behind a closed gate. UDP sent before the TCP response stays kernel-queued.
9. Mark the connection RPC-initialized and `Connected`.
10. Release the version-2 receive gate. No typed callback dispatch occurs while
    the connection is still `Connecting`.

After session replacement begins, any initialization failure closes the new
command socket and callback listener and records `LastInitializationException`.

### Close

`CloseConnection()` and `Dispose()` perform this order:

1. Send `0x405D` on the command TCP socket if possible and validate its ACK.
2. Close the command TCP socket.
3. Stop the callback listener.
4. Clear connection state and cached handshake responses.

A nonzero close ACK is preserved and reported to the caller after local cleanup.

### Reconnect

Calling `RpcInitConnection(...)` again with valid parameters closes both sockets
from the previous session, then starts a new session. Axis/group objects created
for the previous session generation are rejected after reconnect.

Each receive thread captures its own UDP listener and expected controller
address. Stop atomically detaches the current listener/thread/endpoint before
closing and joining that generation. If a callback handler outlives the join
timeout, the old thread cannot consume the next session's UDP socket or clear
the new session's listener fields when it eventually exits. Callback delivery,
handler-error reporting, and rejected-source counting also require the exact
listener object and connection-lifetime generation. A late exception from an
old handler is therefore suppressed after a replacement session starts and
cannot contaminate that session's error event or rejected count.

Typed provenance and version-2 counter recording additionally require a
`Connected` state and the exact published TCP client lifetime/session tuple. A
safety-preemption detach therefore invalidates wake hints before its later UDP
listener stop, even during that deliberate detach-to-stop window.

The example WPF application repeats the provenance check after its dispatcher
queue is reached. It drops a callback unless the sender is still the active
connection and `BelongsToCurrentSession` is true. An event accepted by an old
listener therefore cannot become a raw log entry after Close/reconnect merely
because it was already queued for the UI thread.

## Current implemented baseline and limitation

The canonical LASAL project contains the two exact vendor UDP classes, the
derived `LMCUdpCallbackSender`, both callback Network objects and links, and the
five contract-scoped `TCPMotionInterface` lifecycle bodies. Gate B1
`DerivedDeclaration`, Gate B2 `DerivedWired`, and Gate C `DerivedCandidate` have
their own committed physical checkpoint manifests. The current static verifier
resolves the actual tree as `DerivedCandidate`, with
`ProductionApproved=false` and `NeedsRebaseline=true`.

The Gate C sender code can build and queue the bounded version-2 datagram, but
there are zero production `PublishEvent(...)` call sites. This is dormant
candidate code, not proof that the PLC emits a callback. The PC delivery source
now keeps `LMCCallbackRegistrationMode.LegacyRaw` as the default and provides an
explicit `Version2WakeHint` opt-in. The opt-in path sends the exact 32-byte
request, parses the exact 20-byte response, installs the accepted
source/BootId/session/cookie/length/policy/sequence fence, and dispatches only
`LMCCallbackWakeHintEventArgs`; it never raises the raw `CallbackReceived` event.
Its fixed receive buffer is bounded by the accepted maximum, and an oversized
UDP datagram is rejected without an attacker-sized allocation. The connection
layer deliberately performs no authoritative state mutation or automatic TCP
query. Authoritative TCP follow-up and a production PLC publisher remain
unwired and unqualified.

No PLC runtime result is claimed by this document. PLC download, a live UDP
datagram and receiver/dispatch capture, exact duplicate/mismatch registration
capture, and loss/duplicate/reorder qualification remain pending. The sections
below preserve the approved contract and historical Gate A/B evidence; the
recorded Gate C result is static/build candidate evidence only.

## 2026-08-07 implementation decision

TCP remains the control and authoritative query transport. UDP is added only as
a bounded callback notification path. A UDP datagram may wake the PC and tell it
which authoritative state should be queried over TCP, but it must never by
itself complete a motion command, clear an ownership record, acknowledge a
safety action, or establish a terminal result.

Wire activation is split into two independently qualified phases.

1. Phase 1 preserves the current legacy `0x405C` 12-byte request and 4-byte
   response. The fixture macro is reserved at zero, but the initial Gate C
   sender does not implement a version-1 publication path. An exact raw fixture
   must first be frozen in PLC/C# tests and approved as a separate change.
2. Phase 2 adds an explicitly negotiated `0x405C` version-2 request/response and
   the `LMC2` datagram envelope. Only Phase 2 provides PLC BootId, PLC session
   epoch, 64-bit client cookie, and 64-bit datagram sequence fencing. Its initial
   production policy permits only event type 1, event-mask bit 1, delivery class
   0, and a zero-byte payload. The opt-in PC receiver now enforces the complete
   fence and wake-hint policy; PLC production publication remains inactive.

Phase 1 must not be described as same-IP/same-port stale-datagram safe. The
current C# listener object/lifetime checks stop an old receive thread from
contaminating a new listener, but an already emitted UDP datagram from the same
PLC IP can still arrive after the PC rebinds the same port. Phase 2 closes that
wire-provenance gap.

## Vendor import boundary

Gate A imported exactly these two SIGMATEK classes from
`Lasal_PRG/MotionTCPDemo`:

1. `_UDPTransceiver`, source revision `1.2`, current physical CRLF source
   SHA-256 `C3713C1E76E0027F6E90007268BFC2DFA8962F778A7B2EE2B3E50C11F520C321`
   and canonical LF SHA-256
   `D0D35828725B41B0E1C2323FE2120A1F492F7C6DA56254CAF9A10D07E7492DD1`
2. `_UDPTransceiverInterface`, source revision `1.3`, current LF source SHA-256
   `9575ED267B9629D811E18C9A5156EC4089F8223464D42DA4ADA6F1F8E8188D80`

Do not import `UDPTransmission`, TCP/DataManager classes, SafetyUDP, `_StdLib`,
`CriticalSection`, or any common dependency offered by the import dialog. The
canonical project already has `_StdLib`, `CriticalSection`, and
`Source/interfaces/lsl_st_tcp_user.h`. The demo and canonical dependency source
hashes are not equal:

- demo/canonical `_StdLib`: `5E729CED...` / `53DA7E45...`
- demo/canonical `CriticalSection`: `AFA7E2C8...` / `752ED613...`

The protected canonical dependency hashes after import are:

- `_StdLib.st`:
  `53DA7E459AE214D28AB8D77CC2F1FDED9E2F7D8D552C91D71488D63DD22050EA`
- `CriticalSection.st`:
  `752ED61394D1B708176613DE8B002E197ED46D46EDB3C0BA497560D222A8B9EE`
- `Source/interfaces/lsl_st_tcp_user.h`:
  `2DEC7C124CEC1B44766367188D5F00F6B2B812F372A3868EA1604F19C9621EDD`

These files must remain canonical. Any later import that cannot exclude them is
cancelled rather than resolved by copying a demo dependency.

### Gate A verified evidence

- The final C78/ARM Rebuild ran from `2026-08-07 17:59:52` through `18:00:16`.
  Compiler error diagnostics were zero; both compiler passes and the linker
  reported Done, and LASAL reported `Last command succeeded` in `23683.9 ms`.
- The numbered build warnings were `W0069=35`, `W0072=17`, and `W0073=3`.
  The six separate C78/C82 library-version warnings already existed before the
  import; they are recorded separately from the zero-error result.
- The Gate A verifier snapshot committed in `e287d07` is `180340` bytes with SHA-256
  `F020F310CE6DFB79D504E1099E36C6F5D5703B0A75789C51AE55A5569B5BACBD`.
  Its self-test passed `61/61`, and its `VendorImported` check passed the
  imported source, generated declarations, protected dependencies,
  class/project registries, and Network boundary.
- The exact true generated `Classes.lcb` chunks are `_UDPTransceiver`, `52552` bytes,
  SHA-256
  `958A2EC0945A01878261A7B055A25EBB5A44AFCADDD3BE7A2309744B69F90FAB`,
  and `_UDPTransceiverInterface`, `25583` bytes, SHA-256
  `7FC931079DCFBB894D29EC1A92B291E67D21A01F250B0B1639B22A82BEB614EB`.
- The final repository-wide
  `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1` run passed in
  `250.494 s` with `Axis1 PASS`; the focused verifier SHA-256 was identical
  before and after that run.
- `_UDPTransceiver::Init` and `_UDPTransceiverInterface::AddSocket` opened
  directly in their qualified implementations. No new `CInvalidArgException`
  was recorded after that smoke. This is complete Gate A method direct-open
  evidence. With no UDP object/link in Gate A, the class-definition client/server
  menus have no `Find in Implementation`. That channel smoke becomes possible
  after Gate B2 wiring. The later Gate C smoke used direct implementation editor
  opens instead because the high-level computer-use operation failed; it did not
  execute a client/server `Find in Implementation` search.
- `ConfigObjects.st` and `Networks.lcb` changed only in the IDE-generated class
  registry needed to register the two imported classes. All Network topology
  files and links remained unchanged; no UDP object was added in Gate A.
- The verifier snapshot committed in `e287d07` proves only the Gate A
  `VendorImported` boundary and rejects its then-existing higher-state mode.
  At that checkpoint, `DerivedDeclaration` and `DerivedWired` were later
  verifier work and were not attributable to the 61-fixture snapshot; their own
  frozen hashes and tests still had to be recorded before Gate B1 could start.

### Expanded verifier hardening and completed-gate evidence

- The committed `e287d07` / `F020F310...` / `61/61` / `250.494 s` evidence
  above remains the historical Gate A snapshot. It is not replaced or
  retroactively attributed to the later Gate B/C state-ladder work.
- The later expanded focused verifier is `409934` bytes and `9859` lines with
  SHA-256
  `E5211F3D44712ADE1B4CDE5F6AB72729993AEF530152BC36BDD695C81CDFE6FC`.
  Its direct self-test and the main
  `Verify-LasalContract.ps1 -UdpCallbackVerifierSelfTestOnly` wiring each passed
  `249/249`. In that historical snapshot, the then-current canonical
  `VendorImported` check also passed with `ProductionApproved=true` and
  `NeedsRebaseline=false`.
- A later full
  `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1` run passed in
  `242.1 s` with `Axis1 PASS`; the expanded focused verifier SHA-256 was exactly
  `E5211F3D...E6FC` before and after the run. An independent read-only review of
  that exact snapshot returned GO.
- These later tests validate the state resolver and fail-closed capture
  boundaries; they did not approve a synthetic higher state. The later physical
  gate records are distinct snapshots and do not rewrite the historical Gate A
  evidence above.
- Gate B1 `DerivedDeclaration` was recorded by commit `95c76fe`. Its
  `gate_b1_derived_declaration_checkpoint.json` is `3117351` bytes with SHA-256
  `F0A7DD7D192F5DE6E23F2CA921F6C0145249DE06A1F8D7B333C0FE49C7B2BFA2`.
  The manifest records the `446686`-byte verifier
  `D126AC214DE701754CEF862167887EC0A8405BBCB6FDF59B607639DA75E00788`
  and its `259/259` self-test. It is capture-only:
  `ProductionApproved=false`, `NeedsRebaseline=true`.
- Gate B2 tooling was frozen by `85e9592`, and the wired source plus manifest
  were committed by `cc83311`. The B2 manifest is `3122685` bytes with SHA-256
  `96DA05F0E45E3129E27C082FADE7C4C8EF48D9AFF06E0ED058803BC1C0BCC39F`.
  It records the `467485`-byte verifier
  `F553EE5D986272A9460FB6C5DB2CE18D3491FD34922EE2F1C83A1CC3665B9600`
  and its `262/262` self-test. Its state is `DerivedWired`, also with
  `ProductionApproved=false` and `NeedsRebaseline=true`.
- Gate C tooling commit `9d0b8c9` freezes the `478281`-byte verifier
  `C0B95B5D6A6220C701C30B7EB379473C4BA43761F70D2DD5DB280AFA40FDCF12`
  and the `401786`-byte capture tool
  `34ED0649382A4006830B714C8F57915BED786E7A22068BBFE5CC41EE3E0CB8DA`.
  Their self-tests passed `263/263` and `38` positive / `64` negative cases.
  Trusted `ValidateOnly` passed in `762.1 s` with `outputCreated=false`; trusted
  `Capture` passed in `912.0 s`.
- The resulting `gate_c_derived_candidate_checkpoint.json` is `3121423` bytes
  with SHA-256
  `94632D03946F337F0925D0D873A47E09162E606835DDB726F5EB1644AF407366`.
  It records an actual-tree `DerivedCandidate` PASS and the complete
  Gate A -> B1 -> B2 -> C lineage. Commit `70c08ea` records that candidate and
  manifest. The decision remains `ProductionApproved=false` and
  `NeedsRebaseline=true`; this is not production or PLC-runtime approval.
- Main-wrapper parameter wiring began in `a3a419e` and was finalized in
  `a6b5d5f`. The committed wrapper explicitly supports
  `-UdpCallbackExpectedState DerivedCandidate -AllowUdpCallbackDerivedCapture`.
  Its current physical identity is `2061458` bytes with SHA-256
  `DC9E0B02851E73265A75F2F90F1B9BA385A2E571B010BD1D8EDC4F06F36E306F`.
  The full
  `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1 -UdpCallbackExpectedState DerivedCandidate -AllowUdpCallbackDerivedCapture`
  run passed in `233.562 s`, with that wrapper hash unchanged before and after.
  This proves full SourceOnly wrapper consumption of the candidate only; it does
  not change the capture-only/nonproduction decision above.
- A .NET SDK MSBuild Release build of `LasalMotionControlLib.Tests` succeeded,
  and
  the custom test runner reported `TOTAL 1110, PASSED 1110, FAILED 0`. This set
  includes callback codec, `InitialV2WakeHint`, session fencing, and opt-in
  `LmcConnection` TCP/UDP fake-peer integration coverage. It is PC-side evidence,
  not proof of a downloaded PLC publisher or live machine packet path.

## Derived sender class contract

Create `LMCUdpCallbackSender` as a derived class of
`_UDPTransceiverInterface`. It is a non-RT cyclic class with a fixed 10 ms cycle,
owns the application transmit pump, and inherits the vendor `ClassSvr`,
`_UDPTransceiver`, and `CriticalSection_UDP` channels. New names, declarations,
comments, and implementation source remain 7-bit ASCII.

Class settings are exact: `RealtimeTask=false`, `CyclicTask=true`,
`DefCyclictime=10 ms`, `BackgroundTask=false`, and `Sigmatek=false`. Preserve
the generated defaults `OSInterface=false`, `HighPriority=false`,
`Automatic=false`, `UpdateMode=Prescan`, and `SharedCommandTable=true`. Do not
add `#pragma pack`;
the following structures are storage, not wire overlays. Do not add private
variable initializers. A fresh object's zero state is used, and a new arm
explicitly sets `NextSequenceLo=1` and `NextSequenceHi=0`.

The LASAL-derived internal `_base` Network is mandatory. Preserve exactly the
generated `_UDPTransceiverInterface` base relation, the `ClassSvr`, `ErrorCode`,
`ErrorMessage`, `ErrorState`, and `State` server exposure, the external
`_UDPTransceiver` client, and its six generated connections.
`CriticalSection_UDP` remains internal and must not be exposed or relinked.

Use this exact TYPE, server, and variable declaration order:

```st
//{{LSL_DEFINES
#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE
#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0
#endif
//}}LSL_DEFINES

#pragma using _UDPTransceiverInterface

TYPE
  _LMC_UDP_ACTIVE_ENDPOINT : STRUCT
    Armed : BOOL;
    ProtocolVersion : UINT;
    EventMask : UDINT;
    CallbackIPv4 : UDINT;
    CallbackPort : DINT;
    SessionEpoch : UDINT;
    BootId : UDINT;
    CookieLo : UDINT;
    CookieHi : UDINT;
    MaxDatagramBytes : UDINT;
  END_STRUCT;

  _LMC_UDP_TX_SLOT : STRUCT
    InUse : BOOL;
    ProtocolVersion : UINT;
    DatagramBytes : UDINT;
    DestinationIPv4 : UDINT;
    DestinationPort : UDINT;
    SessionEpoch : UDINT;
    BootId : UDINT;
    CookieLo : UDINT;
    CookieHi : UDINT;
    SequenceLo : UDINT;
    SequenceHi : UDINT;
    PlcTimeMs : UDINT;
    RetryCount : UDINT;
    Data : ARRAY [0..511] OF BYTE;
  END_STRUCT;
END_TYPE

QueueDepth : SvrCh_UDINT;
QueuedCount : SvrCh_UDINT;
RingAcceptedCount : SvrCh_UDINT;
AdmissionRetryCount : SvrCh_UDINT;
QueueFullDropCount : SvrCh_UDINT;
AdmissionErrorDropCount : SvrCh_UDINT;
DisarmClearedCount : SvrCh_UDINT;
TransportErrorCount : SvrCh_UDINT;
LastAdmissionResult : SvrCh_DINT;

ActiveEndpoint : _LMC_UDP_ACTIVE_ENDPOINT;
TxSlots : ARRAY [0..7] OF _LMC_UDP_TX_SLOT;
ReadIndex : UDINT;
WriteIndex : UDINT;
Depth : UDINT;
NextSequenceLo : UDINT;
NextSequenceHi : UDINT;
```

There are no new client channels. The derived class uses the inherited
`_UDPTransceiver` client and inherited `Socket : DINT` and `State` storage.
`CallbackIPv4` remains a raw `UDINT`; do not add a string copy or separate socket
handle/state.

The standard cyclic method is separate from every user function and has this
exact LASAL ABI:

```text
FUNCTION VIRTUAL GLOBAL CyWork
  EAX : UDINT
  state (EAX) : UDINT
```

The generated command table must contain
`vmt.CmdTable.CyWork := #CyWork();`. Omitting this standard method leaves the
sender queue without a cyclic pump even when the three public user functions are
callable.

Add these three public `GLOBAL` functions in this exact order. They are callable
through `LMCUdpCallbackSender1.ClassSvr`.

```text
ArmEndpoint
  ProtocolVersion : UINT
  EventMask : UDINT
  CallbackIPv4 : UDINT
  CallbackPort : DINT
  SessionEpoch : UDINT
  BootId : UDINT
  CookieLo : UDINT
  CookieHi : UDINT
  MaxDatagramBytes : UDINT
  Result : DINT

DisarmEndpoint
  ExpectedSessionEpoch : UDINT
  ExpectedCookieLo : UDINT
  ExpectedCookieHi : UDINT
  Result : DINT

PublishEvent
  EventMaskBit : UDINT
  EventType : UINT
  DeliveryClass : UINT
  EventId : UDINT
  ProducerSessionEpoch : UDINT
  pPayload : ^void
  PayloadBytes : UDINT
  Result : DINT
```

Add these private functions with the exact inputs, outputs, types, and order
below. Do not select `GLOBAL` or `VIRTUAL GLOBAL`:

```text
EnsureSocketReady
  Result : DINT

ValidateEndpoint
  ProtocolVersion : UINT
  EventMask : UDINT
  CallbackIPv4 : UDINT
  CallbackPort : DINT
  SessionEpoch : UDINT
  BootId : UDINT
  CookieLo : UDINT
  CookieHi : UDINT
  MaxDatagramBytes : UDINT
  Result : DINT

BuildDatagram
  SlotIndex : UDINT
  EventMaskBit : UDINT
  EventType : UINT
  DeliveryClass : UINT
  EventId : UDINT
  ProducerSessionEpoch : UDINT
  pPayload : ^void
  PayloadBytes : UDINT
  Result : DINT

FindFreeSlot
  SlotIndex : DINT

ServiceTransmitQueue

SendSlot
  SlotIndex : UDINT
  VendorResult : DINT

RetryOrDropSlot
  SlotIndex : UDINT
  VendorResult : DINT

ClearPendingFrames

FenceMatches
  ExpectedSessionEpoch : UDINT
  ExpectedCookieLo : UDINT
  ExpectedCookieHi : UDINT
  Matches : BOOL
```

Override the inherited vendor callback with its exact three-input ABI. This is a
`VIRTUAL GLOBAL` override, not one of the private helpers:

```text
ErrorCallback
  FSM_UDP : _UDPTransceiver::_FSM_UDP_USER
  UdpError : _UDPTransceiver::_UDP_ERROR
  ErrCode : DINT
```

Do not depend on an uncertain base-call syntax. `ErrorCallback` takes
`CriticalSection_UDP` once, repeats the vendor base behavior as the three exact
assignments `ErrorState := FSM_UDP`, `ErrorMessage := UdpError`, and
`ErrorCode := ErrCode`, saturating-increments `TransportErrorCount`, and releases
the lock. An asynchronous vendor send error is not correlated back to an
application slot and is not retried by the sender.

### Public result domains

No public method may return an undocumented value.

| Method | Result | Exact meaning |
|---|---:|---|
| `ArmEndpoint` | `0` | new endpoint committed |
| | `1` | exact duplicate; endpoint, queue, and sequence preserved |
| | `-1` | invalid endpoint or fence input |
| | `-2` | `AddSocket` failed; endpoint, FIFO, and sequence are unchanged |
| | `-3` | conflicting endpoint already armed; old endpoint preserved |
| | `-6` | unsupported protocol or policy |
| | `-9` | internal failure |
| `DisarmEndpoint` | `0` | matching endpoint and queue cleared |
| | `1` | already disarmed and empty |
| | `-8` | stale fence; active endpoint and queue preserved |
| | `-9` | internal failure |
| `PublishEvent` | `0` | 52-byte initial-policy datagram enqueued in the application FIFO |
| | `-2` | `AddSocket` failed; endpoint, FIFO, and sequence are unchanged |
| | `-4` | endpoint not armed |
| | `-5` | FIFO full; the new event was dropped |
| | `-6` | unsupported event, protocol, or delivery policy |
| | `-7` | invalid payload pointer or length |
| | `-8` | stale producer session |
| | `-9` | internal failure |

Validation is fail-closed and does not partially mutate an endpoint. The
initial sender accepts protocol version 2 only. It requires nonzero
`SessionEpoch` and `BootId`, a valid nonzero IPv4, a port in `1..65535`,
`(EventMask AND 16#00000001) = 16#00000001`, a nonzero 64-bit cookie, and a
maximum in `52..512`. Protocol version 1 returns `-6` regardless of the reserved
fixture macro until exact raw bytes and their tests are approved. Exact
duplicate comparison covers all nine `ArmEndpoint` inputs.

`ArmEndpoint` accepts `SessionEpoch` and `BootId` only from trusted
`TCPMotionInterface` state, never from a UDP payload. `PublishEvent` requires a
subscribed `EventMaskBit`, a `ProducerSessionEpoch` equal to the armed epoch, a
valid pointer for nonzero payload, and a payload within the protocol limit. The
initial production version-2 policy accepts only `EventMaskBit=1`,
`EventType=1`, `DeliveryClass=0`, and `PayloadBytes=0`. A structurally invalid
pointer/length returns `-7`; an otherwise valid but unapproved event/payload
policy returns `-6`. Under that type-1 policy, every `EventId` value in the full
`UDINT` domain, including zero, is valid opaque producer correlation. The sender
does not generate it, and it never becomes completion authority.
`DisarmEndpoint` clears an endpoint and its pending slots only when all three
supplied fence values match; a stale caller cannot disarm a newer endpoint.

### Observable channels

Add these server channels in this exact order and with no Network connection:

```text
QueueDepth : SvrCh_UDINT
QueuedCount : SvrCh_UDINT
RingAcceptedCount : SvrCh_UDINT
AdmissionRetryCount : SvrCh_UDINT
QueueFullDropCount : SvrCh_UDINT
AdmissionErrorDropCount : SvrCh_UDINT
DisarmClearedCount : SvrCh_UDINT
TransportErrorCount : SvrCh_UDINT
LastAdmissionResult : SvrCh_DINT
```

Every channel has `Initialize=true`, `DefValue=0`, `WriteProtected=true`,
`Retentive=false`, and `Visualized=false`. `QueueDepth` is the current FIFO
depth. `QueuedCount` counts successful application enqueues,
`RingAcceptedCount` counts vendor-ring admissions, `AdmissionRetryCount` counts
retained buffer-full retries, `QueueFullDropCount` counts new events rejected at
full depth, `AdmissionErrorDropCount` counts head slots dropped after bounded
admission failure, and `DisarmClearedCount` counts pending slots cleared by a
matched disarm. It saturating-adds the number of slots in the pre-clear `Depth`;
it does not increment once per disarm call. `LastAdmissionResult` stores the
latest immediate `SendData` return. Every UDINT count saturates at
`16#FFFFFFFF`.

## Fixed memory and transport limits

The application-owned queue is an exact FIFO of eight fixed 512-byte datagram
slots. Its bounded state uses `ReadIndex`, `WriteIndex`, and `Depth`. A full FIFO
drops only the newly submitted event and does not disturb any queued slot. The
initial PLC version-2 path accepts only `PayloadBytes=0`, builds exactly the
52-byte `LMC2` header synchronously in the selected slot, and sets
`DatagramBytes=52`. It does not dereference, copy, or retain `pPayload`.
Nonzero PLC payload copying is not implemented; a structurally valid nonzero
payload is rejected by policy before slot mutation. Slot metadata contains the
destination, endpoint fence, 64-bit sequence, enqueue timestamp, and admission
retry count. `LMCUdpCallbackSender` must not call `Malloc`, `MallocV1`,
`Realloc`, or `Free`. `FindFreeSlot` returns `WriteIndex` only when `Depth<8` and
`TxSlots[WriteIndex].InUse=FALSE`; otherwise it fails without changing queue
state. Only `DatagramBytes` bytes are sent, so stale bytes beyond that length
never reach the wire.

The imported vendor class internally allocates its configured general buffers
during initialization and a socket send ring during `AddSocket`; that vendor
behavior is not a per-event application allocation. Configure only one sender
socket and these exact Network values:

- `LMCUdpTransceiver1.cSizeOfRXBuffer = 512`
- `LMCUdpTransceiver1.cSizeOfTXBuffer = 8 kb`
- negotiated and local maximum UDP datagram = `512` bytes
- version-2 header = `52` bytes
- maximum version-2 payload = `460` bytes

`SendData` uses the vendor queued path (`bDirect := FALSE`). The local eight-slot
queue remains the admission/fencing boundary; the vendor ring is only the
transport queue. `ServiceTransmitQueue` examines only the FIFO head and invokes
`SendData` at most once in one sender `CyWork`. Result `0` removes the head and
increments `RingAcceptedCount`. Vendor result `-4`
(`UDP_CLT_SEND_BUFFER_FULL`) retains the head for at most three retries, so one
slot receives at most four total admission attempts. Each of the first three
retained retries increments `AdmissionRetryCount`; a fourth `-4` drops the head
and increments `AdmissionErrorDropCount`. Any other negative vendor result drops
the head immediately and increments `AdmissionErrorDropCount`. A retained head
blocks every later slot. No scan waits for a UDP acknowledgement.

Gate C uses the inherited interface wrapper exactly as
`SendData(pData, udSize, bDirect, udIpAddress, udPort)`; it does not pass a
socket argument or call the lower-level transceiver ABI directly. The null
payload test is `pPayload = NIL`, never a numeric zero, and the header byte at
offset 50 uses `TO_USINT(DeliveryClass)`, not `TO_BYTE`.

The sender owns exactly one send-only socket. `EnsureSocketReady` uses the
inherited `Socket : DINT`. If `Socket=0`, it calls `AddSocket`; a returned handle
of `0` is failure `-2`, while every nonzero DINT bit pattern, including a
negative signed value, is a valid handle to poll. It then returns `0` when
`IsOpen()` is true and `1` while open is pending. It never calls `BindSocket` or
`DelSocket`. Disarm clears the endpoint and application FIFO but preserves the
socket for reuse.

A matched `DisarmEndpoint` snapshots the old `Depth`, zeroes the complete
`ActiveEndpoint`, and calls `ClearPendingFrames`. That helper zeroes every byte
of all eight `TxSlots` and resets `ReadIndex`, `WriteIndex`, `Depth`, and
`QueueDepth`. The matched disarm then saturating-adds the saved depth to
`DisarmClearedCount`. An already-disarmed or stale-fence call performs none of
these mutations.

`CyWork` calls `EnsureSocketReady` every scan even while disarmed, so it can
precreate the socket. A new `ArmEndpoint` commits when that helper returns ready
`0` or pending `1`; both cases return public result `0`. A helper result of `-2`
rejects the arm without endpoint mutation. An exact duplicate arm returns `1`
before socket-state handling and preserves the endpoint, FIFO, and sequence.
`PublishEvent` likewise enqueues on helper result `0` or `1`, preserving events
while the OS socket opens, and returns `-2` without mutation only on helper
failure.

All shared endpoint, FIFO, sequence, and counter state is protected by the
inherited `CriticalSection_UDP`. Each public function, `CyWork`, and
`ErrorCallback` calls `CriticalSection_UDP.SectionStart()` and
`CriticalSection_UDP.SectionStop()` exactly once. It calls the private helpers
while already locked, does not re-enter that lock, and does not execute `RETURN`
while locked. `CyWork` calls `EnsureSocketReady` under that lock and calls
`ServiceTransmitQueue` only when the helper returned `0`, the endpoint is armed,
and `Depth>0`; it then returns `state := READY`. The sender and vendor transceiver
are two separate 10 ms non-RT pumps: the sender admits at most one head slot to
the vendor ring per scan, and the vendor transceiver performs the OS send in its
own `CyWork`. Queued `SendData` uses the vendor ring's separate critical
section. Vendor `CyWork` releases that lock before the OS send and before any
`ErrorCallback`, so the queued call cannot synchronously re-enter the sender
lock.

On a new arm the next sequence is reset to `1`. An exact duplicate arm preserves
it. Queue-full rejection does not consume a sequence; successful enqueue assigns
the current sequence and advances it. Vendor-ring admission failure may therefore
leave a sequence gap, while retries keep the same sequence. Disarm and the next
new arm reset it to `1`; low-word wrap increments the high word and high-word
wrap explicitly returns it to zero. At enqueue, the slot takes
`PlcTimeMs := ops.tAbsolute` and copies that exact value to header offset 44. It
does not synthesize time by adding the 10 ms cycle. The wire timestamp naturally
wraps modulo `2^32` and is never command authority.

## Phase 1: legacy raw transport proof

Phase 1 does not change the current wire contract:

| `0x405C` legacy request payload | Offset | Size |
|---|---:|---:|
| EventMask | 0 | 4 |
| CallbackPort | 4 | 4 |
| CallbackIPv4 `BYTE[4]` | 8 | 4 |

The request payload stays exactly 12 bytes and its ACK payload stays exactly
4 bytes. `TCPMotionInterface` continues its current exact-peer, valid-port,
first-commit, exact-duplicate-idempotent, mismatch-preserve behavior. Normal
production builds do not arm or publish through the version-1 sender path. The
reserved compile-time fixture flag is not an activation switch in the initial
Gate C implementation; changing it alone must not make version 1 armable.

The exact proof-only fixture byte sequence is still pending and is not a current
contract or test vector. Before a dedicated qualification build, freeze one
sequence in both PLC and C# tests, send it through the fixed queue to the
registered IPv4/port, and verify `CallbackReceived.Payload` byte-for-byte. That
future change must add its own guarded sender implementation and verifier.
Protocol version 1 has no `LMC2` envelope, cookie, typed event, or authoritative
completion meaning. It is not allowed to become a production event stream.
`LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE` remains reserved at `0`; version-1 arm
or publication is rejected with `-6` and does not mutate the endpoint or FIFO.

For version-2 lifecycle integration, request validation and sender arming precede
`RpcCallbackRegistered := TRUE` and the success response. If the connected
sender does not return `0` or `1`, the registration tuple is not committed.
Close, disconnect, failed initialization, and takeover pass the old
session/cookie fence to `DisarmEndpoint` before incrementing or clearing the TCP
session fields. The `CallbackSender` client is optional at the class boundary so
an import-only checkpoint can still load; a version-2 request fails closed when
that client is not connected. Gate C preserves the legacy 12/4 lifecycle bytes
and shape lock but does not arm the UDP sender or publish a version-1 event.

Gate B2 adds these private `TCPMotionInterface` variables in exact order, with
no inline initializers:

```st
RpcCallbackProtocolVersion : UINT;
RpcCallbackAcceptedMaxDatagram : UINT;
RpcCallbackSessionEpoch : UDINT;
RpcCallbackBootId : UDINT;
RpcCallbackCookieLo : UDINT;
RpcCallbackCookieHi : UDINT;
RpcCallbackLastDisarmResult : DINT;
```

It also adds private, non-virtual `DisarmRpcCallbackEndpoint()` with no input
and `Result : DINT` output. Gate B2 keeps that helper's generated implementation
stub empty. `RpcCallbackProtocolVersion` is the per-RPC-session registration
shape lock. Inside an initialized owning RPC session, the first exact 12- or
32-byte registration attempt sets it to `1` or `2` before semantic validation;
a rejected attempt still blocks the opposite
shape while the same shape may retry. `RpcCallbackRegistered` remains the
successful-commit/armed flag. The helper calls the sender only when
`(RpcCallbackProtocolVersion=2) & (RpcCallbackRegistered=TRUE)`. Any other tuple
has no armed sender endpoint: it returns `1` and clears the residual TCP tuple.
For an armed v2 tuple it requires `IsClientConnected(#CallbackSender)` or
returns `-9`, then forwards the exact stored session epoch and two cookie words.
It records every result in `RpcCallbackLastDisarmResult`. Result `0` or `1`
permits clearing the complete legacy and v2 TCP tuple; a negative result
preserves the complete v2 tuple so a later initialization retries the same
disarm and remains fail-closed. A repeated `0x8080` initialization does not
report success when that retry fails. `RpcCallbackLastDisarmResult` is the
diagnostic latch and is never part of the cleared tuple.

`0x8080` validates its one-byte initialization shape and socket ownership
before calling the disarm helper. The validation rejection itself does not
disarm or mutate an active callback tuple. Its existing failure frame still
uses TCP `SendData`; a partial or failed failure-frame send follows the general
forced transport-quarantine path and may then disarm and advance the session.
On an accepted `0x405D`, the old nonzero
`SessionEpoch` is copied to `PendingClosedSessionEpoch` when that latch is zero
before the close response is sent or the epoch is incremented. If that direct
send already quarantines and advances the session, the close handler must not
advance it a second time.

The v2 registration path requires `IsClientConnected(#Diagnostics)`, obtains the
current value from `Diagnostics.GetDiagnosticsBootId()`, and rejects zero before
calling `ArmEndpoint`. It commits the complete TCP tuple and
`RpcCallbackRegistered=TRUE` before the full 28-byte TCP frame, containing the
20-byte response payload, enters TCP `SendData` with `udSize=28`.
That ordering lets a partial direct send disarm the exact endpoint. Every
semantically valid v2 arm attempt, including an exact duplicate, calls the
sender's complete nine-input
`ArmEndpoint` contract; `RpcCallbackAcceptedMaxDatagram : UINT` is passed as
`UDINT`. A session cannot switch between the legacy and v2 registration shapes.

## Phase 2: version-2 registration

Phase 2 continues to use command ID `0x405C` and dispatches by exact payload
length. A 12-byte request receives the legacy 4-byte ACK. A 32-byte request is
version 2 and receives a 20-byte response. Any other length is rejected without
mutating the active endpoint.

Legacy remains the PC default. `LMCConnectionOptions.CallbackRegistrationMode`
must be set explicitly to `Version2WakeHint` to select version 2; the requested
maximum defaults to 512 bytes and is constrained to `52..512`. The opt-in path
generates a nonzero cryptographic cookie before replacing an existing session,
binds UDP before registration, installs the accepted fence, starts a gated
receiver, and releases that gate only after `Connected` publication completes.
A connection chooses one registration shape before endpoint commit; it does not
downgrade inside the same TCP session after a version-2 rejection. This prevents
an ambiguous partial v2/legacy endpoint. A fallback attempt, if approved later,
starts a new RPC session and a new listener generation.

All multi-byte fields are little-endian. The exact 32-byte request payload is:

| Offset | Size | Field | Required value/rule |
|---:|---:|---|---|
| 0 | 4 | EventMask `UDINT` | bit 1 set: `(EventMask AND 1) = 1` |
| 4 | 4 | CallbackPort `DINT` | `1..65535` |
| 8 | 4 | CallbackIPv4 `BYTE[4]` | exact current TCP peer |
| 12 | 2 | ProtocolVersion `UINT` | `2` |
| 14 | 2 | RequestedMaxDatagram `UINT` | `52..512` |
| 16 | 4 | ClientCookieLo `UDINT` | part of nonzero 64-bit cookie |
| 20 | 4 | ClientCookieHi `UDINT` | part of nonzero 64-bit cookie |
| 24 | 4 | Flags `UDINT` | `0` |
| 28 | 4 | Reserved `UDINT` | `0` |

The request intentionally contains no client-supplied PLC session epoch. The
PLC binds the endpoint to its current TCP `SessionEpoch` and current diagnostics
BootId. The exact 20-byte response payload is:

| Offset | Size | Field |
|---:|---:|---|
| 0 | 2 | Status `UINT` |
| 2 | 2 | ErrorId `INT` |
| 4 | 2 | AcceptedVersion `UINT` |
| 6 | 2 | AcceptedMaxDatagram `UINT` |
| 8 | 4 | DiagnosticsBootId `UDINT` |
| 12 | 4 | SessionEpoch `UDINT` |
| 16 | 4 | AcceptedFlags `UDINT` |

A new arm result `0` and exact-duplicate result `1` both map to `Status=0` and
`ErrorId=0`. The remaining fields then carry the complete accepted fence:
`AcceptedVersion=2`, the accepted requested maximum, the current nonzero
Diagnostics BootId, the current nonzero TCP SessionEpoch, and
`AcceptedFlags=0`. Every failure maps to `Status=1`, `ErrorId=-1`, and zeros for
all remaining 16 bytes; it preserves the prior endpoint and FIFO. The outer TCP
frame status remains zero and the response payload is exactly 20 bytes.

Success requires version 2, a maximum in `52..512`, nonzero BootId and session
epoch, `(ClientCookieLo OR ClientCookieHi) <> 0`, and accepted flags zero. The PC
stores this response with the owning UDP listener generation and its generated
cookie. Exact duplicate registration returns the existing accepted fence. Any
different tuple in the same TCP session is rejected while the previous endpoint
and queue remain unchanged. A new cookie requires a new RPC session.

## Phase 2: `LMC2` datagram envelope

Every version-2 datagram begins with this exact 52-byte header. There is no CRC
field in version 2.

| Offset | Size | Field | Rule |
|---:|---:|---|---|
| 0 | 4 | Magic | ASCII `LMC2`, bytes `4C 4D 43 32` |
| 4 | 2 | ProtocolVersion `UINT` | `2` |
| 6 | 2 | HeaderBytes `UINT` | `52` |
| 8 | 2 | DatagramBytes `UINT` | `52 + PayloadBytes`, max `512` |
| 10 | 2 | EventType `UINT` | initial production value `1` only |
| 12 | 4 | EventMaskBit `UDINT` | initial production value `1` only |
| 16 | 4 | BootId `UDINT` | exact accepted response value |
| 20 | 4 | SessionEpoch `UDINT` | exact accepted response value |
| 24 | 4 | CookieLo `UDINT` | exact client cookie |
| 28 | 4 | CookieHi `UDINT` | exact client cookie |
| 32 | 4 | SequenceLo `UDINT` | low word of monotonic 64-bit sequence |
| 36 | 4 | SequenceHi `UDINT` | high word of monotonic 64-bit sequence |
| 40 | 4 | EventId `UDINT` | any value, including zero; producer correlation only |
| 44 | 4 | PlcTimeMs `UDINT` | PLC timestamp, not PC authority |
| 48 | 2 | PayloadBytes `UINT` | format permits `0..460`; initial policy requires `0` |
| 50 | 1 | DeliveryClass `BYTE` | initial production value `0` only |
| 51 | 1 | Flags `BYTE` | `0` in version 2 |

The version-2 `LmcConnection` receive path invokes the receiver-fence codec and
validates source IPv4, exact datagram length, all fixed header fields, BootId,
PLC session epoch, cookie, event-mask bit, payload length, and sequence. Source
validation remains mandatory even when the legacy
`ValidateCallbackSourceAddress` option is false. The live receiver object stays
private so application code cannot advance its sequence. Public scalar counters
report accepted wake hints, total rejections, duplicates, and out-of-order
packets. `LMCCallbackWakeHintEventArgs.SessionGeneration` and
`BelongsToCurrentSession` provide the separate PC-side provenance fence. The
sender assigns `1` to the first enqueue, but the receiver accepts its first valid
datagram as the baseline because earlier UDP datagrams may be lost. Thereafter
unsigned delta `0` is duplicate, delta `>= 0x8000000000000000` is out of order,
and a delta in `1..0x7FFFFFFFFFFFFFFF` is a forward sequence, including a loss
gap.

The named `LMCCallbackProtocolPolicy.InitialV2WakeHint` policy and its targeted
tests require registration mask bit 1, `EventMaskBit=1`, `EventType=1`,
`DeliveryClass=0`, zero payload, and registration `Status=0/ErrorId=0`, while
accepting every `EventId : UDINT`. Socket integration tests now prove opt-in PC
negotiation, strict receive rejection, typed dispatch, handler-failure
continuation, and reconnect invalidation against a fake TCP/UDP peer. They do
not prove a PLC publisher, a real packet path, or authoritative TCP follow-up.

The connection handler treats even a valid envelope only as a wake hint. It does
not infer a TCP command from opaque `EventId` and does not query automatically,
because a reconnect between event receipt and a query would create a stale-wake
TOCTOU hazard. The next application tranche must bind an approved event meaning
to a generation-pinned authoritative TCP query and use only that TCP response to
update command completion or safety state. UDP loss, duplication, or reordering
therefore changes notification latency and counters, not command truth.

## Exact IDE and Network handoff

The verifier state ladder is exact and monotonic:

`VendorImported -> DerivedDeclaration -> DerivedWired -> DerivedCandidate`.

No state may be skipped or inferred from a synthetic fixture. Production
approval at every transition requires hashes captured from the actual canonical
project after the stated LASAL Save All/exit checkpoint. Synthetic generated
text is planning and verifier-self-test input only; it is never production
evidence.

### Gate A: import compatibility

1. The canonical C78 project contains only the two requested UDP imports and
   retains canonical `_StdLib`, `CriticalSection`, and `lsl_st_tcp_user.h`.
2. Save All and C78/ARM Rebuild completed with zero compiler errors; both
   qualified methods opened directly and no post-smoke `CInvalidArgException`
   appeared.
3. The source/generated/protected-boundary focused verifier passes and Network
   topology is unchanged. No UDP object or link exists in this gate.
4. Qualified method direct-open smoke is complete. Class-definition
    client/server menus cannot run `Find in Implementation` without object/link
    instances. Gate B2 later made that channel-index smoke possible, but the
    completed Gate C run did not execute a `Find in Implementation` search. It
    directly opened the `TCPMotionInterface` and `LMCUdpCallbackSender`
    implementation editors after Rebuild, with no post-smoke
    `CInvalidArgException`; the missing Gate A search does not keep Gate A open.

### Gate B1: `DerivedDeclaration`

This gate creates declarations only. All new method implementation regions stay
as the empty LASAL-generated stubs.

1. In LASAL create `LMCUdpCallbackSender` derived from
   `_UDPTransceiverInterface` as a non-RT cyclic class. Add the separate standard
   `CyWork`/command-table entry, three public functions, nine private helpers,
   `ErrorCallback` override, observable channels, and fixed FIFO state exactly as
   specified above.
2. Preserve the generated internal `_base` Network exactly as specified above.
   Do not expose or relink `CriticalSection_UDP`.
3. Leave `TCPMotionInterface` and every top-level Network file byte-exact to the
   approved Gate A state.
4. Save All without Rebuild and exit LASAL. Capture exact actual raw/canonical-LF
   sender source hashes plus the class registry, include set, and root project
   registry hashes. `DerivedDeclaration` approval is impossible until those
   post-IDE values replace every planning placeholder in the verifier.

### Gate B2: `DerivedWired`

This gate preserves the B1 empty sender method stubs and changes only the exact
TCP client/fence declarations, one empty TCP helper stub, and top-level callback
Network wiring listed below.

1. Add exactly one optional external client
   `CallbackSender : LMCUdpCallbackSender` to `TCPMotionInterface` with
   `Required=false` and `Internal=false`, immediately after `ControlCommands`.
2. Add the seven exact private callback-fence variables and the private
   `DisarmRpcCallbackEndpoint() -> Result : DINT` declaration specified above.
   The variables are immediately after `RpcCallbackIPv4`; the helper declaration
   is immediately after `HandleRpcLifecycleCommands`, is neither GLOBAL nor
   VIRTUAL, and its generated implementation stub remains empty.
3. In `Comm_Network`, add exactly these two UDP objects:
   - `LMCUdpTransceiver1 : _UDPTransceiver`
   - `LMCUdpCallbackSender1 : LMCUdpCallbackSender`
4. Set both objects to a 10 ms non-RT cyclic time. Set the transceiver buffer
   clients to `512` and `8 kb`, then add exactly these links:
   - `LMCUdpCallbackSender1._UDPTransceiver -> LMCUdpTransceiver1.sControl`
   - `TCPMotionInterface1.CallbackSender -> LMCUdpCallbackSender1.ClassSvr`
5. These are exactly two added UDP objects and exactly two added callback links.
   Do not add direct Control, Diagnostics, Recorder, axis, robot, EtherCAT, or
   critical-section links. `coStdLib` keeps its vendor automatic/optional
   behavior, and all unrelated topology remains byte-exact to B1.
6. Save All without Rebuild and exit LASAL. Capture exact actual hashes for
   `TCPMotionInterface`, its generated declaration, both callback Network
   objects/links, and the affected generated project/Network files.
   `DerivedWired` approval requires those post-IDE hash ratchets and must reject
   any nonempty new implementation body.

### Gate C: `DerivedCandidate`

Keep the exact B2 declarations and wiring. The permitted candidate source
surface is the exact top-level `//{{LSL_DEFINES` macro region plus 14 sender
implementation bodies, and five TCP implementation bodies. The macro region is
the sole custom-source exception to the implementation-region-only rule. LASAL
Save propagates exactly these four generated lines to
`Network/Comm_Network/ONE_Comm_Network_Table.st`:

```text
//Define part of class LMCUdpCallbackSender
#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE
#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0
#endif
```

Implement the sender and TCP lifecycle bodies in tracked custom implementation
regions outside the IDE.
The exact TCP implementation set is `ConnSocketInfo`, `SendData`,
`HandleControlSafetyDrainPending`, `HandleRpcLifecycleCommands`, and the new
`DisarmRpcCallbackEndpoint`; no other TCP method belongs to this Gate C delta.
Each of the four existing methods calls the helper before its callback tuple is
cleared or `SessionEpoch` is incremented. The verifier freezes all five
canonical-LF function bodies independently.
For voluntary repeated `0x8080` initialization and explicit `0x405D` close, a
negative disarm produces the existing-shaped failure and does not clear the
callback tuple or advance the session. Forced owner loss/takeover, direct-send
failure, and safety-drain close still retire the TCP transport after calling the
helper, but a negative result preserves the complete callback tuple so the next
initialization must retry and cannot arm a different endpoint.
Malformed or non-owner `0x8080` requests fail before the helper, so validation
alone does not mutate callback state. If sending that failure frame itself
fails, the normal forced partial-send quarantine still applies. A successful
`0x405D` captures the old epoch in `PendingClosedSessionEpoch` before sending
the ACK and performs at most one epoch advance even when that send itself
enters the partial-send quarantine path.
Capture the complete source hash plus canonical-LF per-function hashes and make
the verifier reject partial, mixed-phase, or unknown bodies. Run SourceOnly
validation. Then reopen LASAL once, Save All, C78 Rebuild, inspect the sender and
lifecycle implementations, and exit. The completed run reported 128 compile
lines, zero errors, Compiler/Linker completion, and UI summary
`0 error(s), 76 warning(s)`. The log delta contained no `ERROR`, `FATAL`,
`CInvalidArgException`, or `failed` record. The 21 new `W0070` diagnostics were
retained as nonblocking warning debt after a precedence/truth-table audit; this
is not a warning-clean build.

Because the high-level computer-use operation failed, the smoke opened the
`TCPMotionInterface` and `LMCUdpCallbackSender` implementation editors directly;
it did not run a client/server `Find in Implementation` search. No new IDE
exception followed those opens, and `Lasal2` exited with code 0. PLC download
and packet proof remain separate final gates.

## Files and test status

The candidate tranche is limited to these production surfaces:

- LASAL imports:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiver/_UDPTransceiver.st`,
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiverInterface/_UDPTransceiverInterface.st`
- new custom source:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`
- lifecycle integration:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
- IDE-generated registration/Network files:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb`,
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb`,
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn`,
  and its generated table
- PC protocol/listener: `LMC_Library/LMC_API_Delivery/src/LmcCallbackProtocol.cs`,
  `LMC_Library/LMC_API_Delivery/src/LmcCallbackModels.cs`,
  `LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`,
  `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`, and explicit `<Compile>`
  entries in `LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj`
- wire documentation:
  `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` and this document

The focused LASAL verifier
`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
is wired into the adjacent `Verify-LasalContract.ps1`. The Gate C tooling and
candidate are committed in `9d0b8c9` and `70c08ea`. The current actual tree
passes as `DerivedCandidate`; its manifest and verifier identities are recorded
above. It remains capture-only with `ProductionApproved=false` and
`NeedsRebaseline=true`, not a production approval.
`LmcCallbackProtocol.cs`, `LmcCallbackModels.cs`,
`CallbackProtocolTests.cs`, `CallbackSessionFencingTests.cs`,
`CallbackV2ConnectionTests.cs`, the response-envelope tests in
`ResponseParserTests.cs`, the 20-byte transport ceiling in
`ResponsePayloadLimitTests.cs`, the raw endpoint-copy regression in
`RpcIntegrationTests.cs`, and the opt-in `LmcConnection` negotiation/receive
path are implemented. The default remains legacy `12/4`; version 2 is selected
only by
`LMCCallbackRegistrationMode.Version2WakeHint`.
`CallbackV2ConnectionTests.cs` owns nine focused connection tests covering the
exact request/response, typed dispatch, strict rejection matrix, bounded
oversized receive, gate ordering and ownership, handler failure, reentrant
close, safety-detach provenance, no downgrade, and reconnect invalidation. No
production `PublishEvent(...)` caller or approved event-to-authoritative-query
mapping exists. PLC download and live UDP receiver/dispatch packet proof remain
required.

Minimum acceptance matrix:

1. Legacy `0x405C` request/ACK stays byte-exact `12/4`. The proof-only raw
   fixture remains default-off and cannot be tested until its exact bytes are
   frozen; after that, it must arrive byte-exact from the configured PLC IPv4.
2. Version-2 `32/20` registration and the 52-byte header pass little-endian
   golden tests. Success is exactly `Status=0/ErrorId=0` with all accepted fields;
   every failure is `Status=1/ErrorId=-1` with the remaining 16 bytes zero.
3. Wrong length, peer IP, port, version, maximum, zero cookie, flags, or reserved
   fields do not mutate an accepted endpoint.
4. Exact duplicate registration is idempotent; mismatched re-registration
   preserves the accepted tuple and pending queue.
5. Socket fixtures cover `AddSocket=0` failure, a nonzero negative DINT handle,
   pending and ready `IsOpen`, precreation while disarmed, pending arm commit,
   pending publish enqueue, and socket-failure publish with no mutation.
6. A successful matched disarm during close, disconnect, failed initialization,
   or same-peer takeover fences and clears the exact old queue. A negative
   disarm preserves the callback tuple and queue debt so the next initialization
   retries the same fence and fails closed until it succeeds. After a successful
   reconnect, a delayed old-cookie/session/BootId datagram is rejected even at
   the same IP and port.
7. FIFO pressure proves strict eight-slot drop-new behavior. A sender `CyWork` invokes
   `SendData` no more than once; head-only `-4`
   handling retains three retries, drops on the fourth total failed attempt, and
   blocks the tail. Hard admission failure, matched-disarm full endpoint/slot
   zeroing, cleared-depth saturation, asynchronous `ErrorCallback`'s three
   inherited-channel assignments, and counter saturation prove every observable
   channel.
8. Sequence fixtures cover low/high wrap and retry identity. An enqueue at
   `ops.tAbsolute=16#FFFFFFFF` writes that exact value to `PlcTimeMs`; a
   460-byte generic PC codec payload passes and 461 bytes fail encoding, while
   the initial PLC sender rejects every nonzero payload before enqueue.
9. Production-policy tests accept every `EventId : UDINT` for `EventType=1` and
   reject a different type, nonzero delivery class, or nonzero production
   payload. UDP loss, duplication, and reordering cannot mark an operation
   complete. A
   valid wake hint causes an authoritative TCP query, and only the TCP response
   changes application state.
10. C78 Rebuild, implementation smoke, PLC download, source/destination packet
   capture, reconnect/takeover capture, and a bounded loss/duplicate/reorder
   test all pass before activation.

## Recorder exclusion

Recorder development remains frozen. Existing recorder code and evidence stay
unchanged. Do not route recorder samples, recorder blocks, or recorder completion
through `LMCUdpCallbackSender`, do not add a Recorder client link, and do not use
this UDP work to reopen recorder protocol or bandwidth design. Any future
recorder transport requires a prior design update and explicit user restart of
that workstream.

See `RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`.

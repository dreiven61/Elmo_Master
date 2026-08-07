# Callback listener design

Date: 2026-07-09

P0 ownership/session-provenance update: 2026-07-31

UDP sender implementation handoff: 2026-08-07

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
address and UDP port. No actual callback datagram has been captured yet, so the
payload remains raw and LASAL event sending is not defined in this phase.

The `0x405C` wire shape is unchanged: its payload remains exactly 12 bytes
(`event mask UDINT + callback port DINT + IPv4 BYTE[4]`) and the response remains
the existing 4-byte command-result ACK. The P0 change tightens ownership; it does
not introduce a new command, payload field, or typed event schema.

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
This evidence is source-level only; a current PLC compile/download and packet
capture must still prove the comparison on the target runtime.

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

The payload is intentionally raw bytes. `LMCCallbackEventArgs.Payload` returns
a defensive copy, and the default listener accepts datagrams only from the
configured controller IPv4. Do not parse the bytes until real callback captures
exist.

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
5. Start callback UDP listener on `localAddress:callbackPort`.
6. Read the listener's actual bound port. This differs when the caller requested
   port `0` and the OS selected an ephemeral port.
7. Send `0x405C` with the same local address and actual bound port.
8. Mark the connection as RPC-initialized.

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

The example WPF application repeats the provenance check after its dispatcher
queue is reached. It drops a callback unless the sender is still the active
connection and `BelongsToCurrentSession` is true. An event accepted by an old
listener therefore cannot become a raw log entry after Close/reconnect merely
because it was already queued for the UI thread.

## Current implemented baseline and limitation

The listener does not interpret callback payloads yet. The tracked LASAL phase-1
handler validates and owns event mask, UDP port, and PC IPv4 but does not send
event datagrams. Consumers can log or capture current-session
`CallbackReceived.Payload` and `CallbackReceived.RemoteEndPoint` after a sender
is added to build the parser. A typed sender/parser remains explicitly excluded
until a real callback payload is captured or an approved local schema is added.

No PLC runtime result is claimed by this document. LASAL IDE compile, PLC
download, exact duplicate/mismatch registration capture, and real callback
datagram capture remain pending.

The statements above describe the current implementation. Everything below is
a proposed implementation handoff. None of the vendor imports, derived class,
new Network objects, version-2 wire fields, typed parser, or queue described
below exists in the canonical project yet.

## 2026-08-07 implementation decision

TCP remains the control and authoritative query transport. UDP is added only as
a bounded callback notification path. A UDP datagram may wake the PC and tell it
which authoritative state should be queried over TCP, but it must never by
itself complete a motion command, clear an ownership record, acknowledge a
safety action, or establish a terminal result.

Implementation is split into two independently qualified phases.

1. Phase 1 preserves the current legacy `0x405C` 12-byte request and 4-byte
   response and proves only that the PLC can send a raw datagram to the already
   implemented PC listener. The received bytes remain opaque. The proof payload
   is a qualification fixture, not a public event schema, and its producer is
   disabled after the transport capture.
2. Phase 2 adds an explicitly negotiated `0x405C` version-2 request/response and
   the `LMC2` datagram envelope. Only Phase 2 provides PLC BootId, PLC session
   epoch, 64-bit client cookie, and 64-bit datagram sequence fencing.

Phase 1 must not be described as same-IP/same-port stale-datagram safe. The
current C# listener object/lifetime checks stop an old receive thread from
contaminating a new listener, but an already emitted UDP datagram from the same
PLC IP can still arrive after the PC rebinds the same port. Phase 2 closes that
wire-provenance gap.

## Vendor import boundary

Import exactly these two SIGMATEK classes from `Lasal_PRG/MotionTCPDemo`:

1. `_UDPTransceiver`, source revision `1.2`, current source SHA-256
   `B3883DF82C942196EB2AA4313DEDBD7BE9430C850052140BAE323B35B272D95D`
2. `_UDPTransceiverInterface`, source revision `1.3`, current source SHA-256
   `6FC3C64D84DDE21EEA8ADC44E89CEF3966A2597D03CD87AB799B344935E7A505`

Do not import `UDPTransmission`, TCP/DataManager classes, SafetyUDP, `_StdLib`,
`CriticalSection`, or any common dependency offered by the import dialog. The
canonical project already has `_StdLib`, `CriticalSection`, and
`Source/interfaces/lsl_st_tcp_user.h`. The demo and canonical dependency source
hashes are not equal:

- demo/canonical `_StdLib`: `5E729CED...` / `53DA7E45...`
- demo/canonical `CriticalSection`: `AFA7E2C8...` / `752ED613...`

If LASAL cannot import the two UDP classes without overwriting one of those
existing dependencies, cancel the import. Do not resolve the conflict by
copying the demo dependency over the canonical project. The demo is a C80
project while the target remains C78, so an import-only C78 Rebuild is a required
compatibility gate; the source revision date is not C78 proof.

## Derived sender class contract

Create `LMCUdpCallbackSender` as a derived class of
`_UDPTransceiverInterface`. It owns a cyclic, non-RT transmit pump and inherits
the vendor `ClassSvr` and `_UDPTransceiver` client. New names, declarations,
comments, and implementation source remain 7-bit ASCII.

Add these three proposed public `GLOBAL` functions in this exact order. They
are callable through `LMCUdpCallbackSender1.ClassSvr`.

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

Add these private functions; do not select `GLOBAL` or `VIRTUAL GLOBAL`:

```text
EnsureSocketReady
ValidateEndpoint
BuildDatagram
FindFreeOrVictimSlot
ServiceTransmitQueue
SendSlot
RetryOrDropSlot
ClearPendingFrames
FenceMatches
```

The exact numeric `Result` constants, event type IDs, delivery-class IDs, retry
limits, and victim policy are not approved by this handoff. They must be added
as a fail-closed code/test contract in the same implementation tranche; do not
invent them in the IDE. Until that contract exists, unknown values, unsupported
delivery classes, and queue-policy ambiguity must reject without endpoint or
queue mutation.

`ArmEndpoint` accepts `SessionEpoch` and `BootId` only from trusted
`TCPMotionInterface` state, never from a UDP payload. `PublishEvent` requires a
single subscribed `EventMaskBit`, a `ProducerSessionEpoch` equal to the armed
epoch, a valid pointer for nonzero payload, and a payload within the active
protocol limit. `DisarmEndpoint` clears an endpoint and its pending slots only
when all supplied fence values match; a stale caller cannot disarm a newer
endpoint.

## Fixed memory and transport limits

The application-owned queue is exactly eight fixed slots of 512 bytes. Slot
payload is copied at `PublishEvent` time, and slot metadata contains the endpoint
fence and retry/drop state. `LMCUdpCallbackSender` must not call `Malloc`,
`MallocV1`, `Realloc`, or `Free`, and it must not retain the caller's payload
pointer after `PublishEvent` returns.

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
transport queue. Queue-full, send-error, retry, coalesce, and drop counters must
be bounded and observable. No scan may wait for a UDP acknowledgement.

## Phase 1: legacy raw transport proof

Phase 1 does not change the current wire contract:

| `0x405C` legacy request payload | Offset | Size |
|---|---:|---:|
| EventMask | 0 | 4 |
| CallbackPort | 4 | 4 |
| CallbackIPv4 `BYTE[4]` | 8 | 4 |

The request payload stays exactly 12 bytes and its ACK payload stays exactly
4 bytes. `TCPMotionInterface` continues its current exact-peer, valid-port,
first-commit, exact-duplicate-idempotent, mismatch-preserve behavior. On the
first accepted registration it may arm the sender as protocol version 1; close,
disconnect, and same-peer takeover disarm it before clearing the stored
registration tuple.

The raw proof sends one known fixture byte sequence through the fixed queue to
the registered IPv4/port and verifies the existing C#
`CallbackReceived.Payload` byte-for-byte. Protocol version 1 has no `LMC2`
envelope, cookie, typed event, or authoritative completion meaning. It is not
allowed to become a production event stream. The exact proof bytes are fixed in
the test that introduces the temporary trigger and are removed or disabled when
the capture is complete.

For lifecycle integration, request validation and sender arming precede
`RpcCallbackRegistered := TRUE` and the success ACK. If the connected sender
does not return an approved success result, the registration tuple is not
committed. Close, disconnect, failed initialization, and takeover pass the old
session/cookie fence to `DisarmEndpoint` before incrementing or clearing the TCP
session fields. The `CallbackSender` client is optional at the class boundary so
an import-only checkpoint can still load; a version-2 request must fail closed
when that client is not connected.

## Phase 2: version-2 registration

Phase 2 continues to use command ID `0x405C` and dispatches by exact payload
length. A 12-byte request receives the legacy 4-byte ACK. A 32-byte request is
version 2 and receives a 20-byte response. Any other length is rejected without
mutating the active endpoint.

Legacy remains the PC default until the version-2 tranche is explicitly
activated. A connection chooses one registration shape before endpoint commit;
it does not downgrade inside the same TCP session after a version-2 rejection.
This prevents an ambiguous partial v2/legacy endpoint. A fallback attempt, if
approved later, starts a new RPC session and a new listener generation.

All multi-byte fields are little-endian. The exact 32-byte request payload is:

| Offset | Size | Field | Required value/rule |
|---:|---:|---|---|
| 0 | 4 | EventMask `UDINT` | nonzero approved mask |
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

Success requires version 2, a maximum in `52..512`, nonzero BootId and session
epoch, a nonzero client-generated cookie that remains bound to the endpoint, and
accepted flags zero. The PC stores this response with the owning UDP listener
generation and its generated cookie. Exact duplicate registration is
idempotent. Any different tuple in the same TCP session is rejected while the
previous endpoint and queue remain unchanged. A new cookie requires a new RPC
session.

## Phase 2: `LMC2` datagram envelope

Every version-2 datagram begins with this exact 52-byte header. There is no CRC
field in version 2.

| Offset | Size | Field | Rule |
|---:|---:|---|---|
| 0 | 4 | Magic | ASCII `LMC2`, bytes `4C 4D 43 32` |
| 4 | 2 | ProtocolVersion `UINT` | `2` |
| 6 | 2 | HeaderBytes `UINT` | `52` |
| 8 | 2 | DatagramBytes `UINT` | `52 + PayloadBytes`, max `512` |
| 10 | 2 | EventType `UINT` | approved ID only |
| 12 | 4 | EventMaskBit `UDINT` | one bit, included in registered mask |
| 16 | 4 | BootId `UDINT` | exact accepted response value |
| 20 | 4 | SessionEpoch `UDINT` | exact accepted response value |
| 24 | 4 | CookieLo `UDINT` | exact client cookie |
| 28 | 4 | CookieHi `UDINT` | exact client cookie |
| 32 | 4 | SequenceLo `UDINT` | low word of monotonic 64-bit sequence |
| 36 | 4 | SequenceHi `UDINT` | high word of monotonic 64-bit sequence |
| 40 | 4 | EventId `UDINT` | producer correlation only |
| 44 | 4 | PlcTimeMs `UDINT` | PLC timestamp, not PC authority |
| 48 | 2 | PayloadBytes `UINT` | `0..460` |
| 50 | 1 | DeliveryClass `BYTE` | approved class only |
| 51 | 1 | Flags `BYTE` | `0` in version 2 |

The PC validates source IPv4, exact datagram length, all fixed header fields,
BootId, PLC session epoch, cookie, event-mask bit, payload length, and sequence
before raising a version-2 notification. Duplicate, stale, malformed, wrong-
cookie, wrong-session, wrong-boot, and unknown-class datagrams increment rejected
counters and do not reach application handlers. Local
`LMCCallbackEventArgs.SessionGeneration` remains a separate PC-side listener
provenance fence.

Even after a valid envelope, the handler treats the event as a wake hint. It
uses the TCP API to read the referenced status/outcome and uses only that TCP
response to update command completion or safety state. UDP loss, duplication,
or reordering therefore changes notification latency and counters, not command
truth.

## Exact IDE and Network handoff

Do the work in these gates; do not combine them into one unverified import.

### Gate A: import compatibility

1. Close LASAL, record a read-only canonical project baseline, and stage an
   import package that contains only `_UDPTransceiver` and
   `_UDPTransceiverInterface`.
2. Open the canonical C78 project and import exactly those two classes. Keep the
   canonical `_StdLib`, `CriticalSection`, and `lsl_st_tcp_user.h`.
3. Save All, run a C78 Rebuild, open both imported implementations, then exit
   LASAL. Do not create a UDP object or change Network in this gate.
4. Accept Gate A only with zero compiler errors, no new
   `CInvalidArgException`, both exact imported class sources present, and the
   protected dependency hashes unchanged.

### Gate B: derived declaration and Network

1. In LASAL create `LMCUdpCallbackSender` derived from
   `_UDPTransceiverInterface`, with cyclic non-RT execution, and add the three
   public functions and nine private functions in the exact order above.
2. Add optional external client `CallbackSender` to `TCPMotionInterface`.
3. In `Comm_Network`, add exactly these objects:
   - `LMCUdpTransceiver1 : _UDPTransceiver`
   - `LMCUdpCallbackSender1 : LMCUdpCallbackSender`
4. Set the two buffer clients to `512` and `8 kb`, then add exactly these links:
   - `LMCUdpCallbackSender1._UDPTransceiver -> LMCUdpTransceiver1.sControl`
   - `TCPMotionInterface1.CallbackSender -> LMCUdpCallbackSender1.ClassSvr`
5. Do not add direct Control, Diagnostics, Recorder, axis, robot, or EtherCAT
   links. The inherited `CriticalSection_UDP` objects stay internal and
   `coStdLib` keeps its vendor automatic/optional behavior.
6. Save All without Rebuild and exit LASAL. Capture generated declarations,
   `Classes.lcb`, project `.lcb`, and Network before external implementation.

### Gate C: source implementation and rebuild

Implement only tracked custom implementation regions outside the IDE, add the
static verifier, and run SourceOnly validation. Then reopen LASAL once, Save
All, C78 Rebuild, directly open the sender and lifecycle implementations, run
the existing client/server `Find in Implementation` smoke, and exit. PLC
download and packet proof are separate final gates.

## Planned files and tests

The implementation tranche is limited to these production surfaces:

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

Add a focused LASAL verifier
`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalUdpCallbackContract.ps1`
and wire it into the adjacent `Verify-LasalContract.ps1`. Add C# tests
`CallbackProtocolTests.cs` and `CallbackSessionFencingTests.cs`, then extend
`RequestGoldenTests.cs`, `ResponseParserTests.cs`, `RpcIntegrationTests.cs`,
`RpcLifecycleConcurrencyTests.cs`, and `FakeRpcServer.cs`.

Minimum acceptance matrix:

1. Legacy `0x405C` request/ACK stays byte-exact `12/4`; the Phase-1 known raw
   fixture arrives byte-exact from the configured PLC IPv4.
2. Version-2 `32/20` registration and the 52-byte header pass little-endian
   golden tests; 460-byte payload passes and 461-byte payload fails before send.
3. Wrong length, peer IP, port, version, maximum, zero cookie, flags, or reserved
   fields do not mutate an accepted endpoint.
4. Exact duplicate registration is idempotent; mismatched re-registration
   preserves the accepted tuple and pending queue.
5. Close, disconnect, failed initialization, and same-peer takeover fence and
   clear the exact old queue. A delayed old-cookie/session/BootId datagram is
   rejected after reconnect to the same IP and port.
6. Queue pressure, send failure, retry, victim/drop selection, and sequence-wrap
   fixtures prove fixed memory, bounded work per scan, and observable counters.
7. UDP loss, duplication, and reordering cannot mark an operation complete. A
   valid wake hint causes an authoritative TCP query, and only the TCP response
   changes application state.
8. C78 Rebuild, implementation smoke, PLC download, source/destination packet
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

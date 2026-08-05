# Callback listener design

Date: 2026-07-09

P0 ownership/session-provenance update: 2026-07-31

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

## Current Limitation

The listener does not interpret callback payloads yet. The tracked LASAL phase-1
handler validates and owns event mask, UDP port, and PC IPv4 but does not send
event datagrams. Consumers can log or capture current-session
`CallbackReceived.Payload` and `CallbackReceived.RemoteEndPoint` after a sender
is added to build the parser. A typed sender/parser remains explicitly excluded
until a real callback payload is captured or an approved local schema is added.

No PLC runtime result is claimed by this document. LASAL IDE compile, PLC
download, exact duplicate/mismatch registration capture, and real callback
datagram capture remain pending.

See `RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`.

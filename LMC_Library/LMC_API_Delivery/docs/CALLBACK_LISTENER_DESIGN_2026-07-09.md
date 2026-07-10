# Callback listener design

Date: 2026-07-09

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

## Public API

`LMCConnection` exposes:

- `CallbackReceived`: raw callback payload event
- `CallbackListenerError`: background listener error event
- `IsCallbackListenerRunning`
- `CallbackLocalEndPoint`
- `RejectedCallbackCount`
- `LMCConnectionOptions.ValidateCallbackSourceAddress` (default `true`)

The payload is intentionally raw bytes. `LMCCallbackEventArgs.Payload` returns
a defensive copy, and the default listener accepts datagrams only from the
configured controller IPv4. Do not parse the bytes until real callback captures
exist.

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
the new session's listener fields when it eventually exits.

## Current Limitation

The listener does not interpret callback payloads yet. The tracked LASAL phase-1
handler stores event mask, UDP port, and PC IPv4 but does not send event
datagrams. Consumers can log or capture `CallbackReceived.Payload` and
`CallbackReceived.RemoteEndPoint` after a sender is added to build the parser.

See `RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`.

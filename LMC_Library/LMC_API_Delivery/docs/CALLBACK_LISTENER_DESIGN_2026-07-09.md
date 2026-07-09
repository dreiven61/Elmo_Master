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

The first implementation uses a UDP listener bound to the callback IP and port.

Reason:

- The captured `0x405C` payload carries a callback port and IPv4 address.
- Maestro documents expose UDP-channel operations per RPC/IPC connection.
- No captured callback payload format is currently available.

If future captures prove that LASAL/Elmo sends callback data over TCP instead,
the listener implementation must be extended, but the public connection
ownership model should stay the same.

## Public API

`LMCConnection` exposes:

- `CallbackReceived`: raw callback payload event
- `CallbackListenerError`: background listener error event
- `IsCallbackListenerRunning`
- `CallbackLocalEndPoint`

The payload is intentionally raw bytes. Do not parse it until real callback
captures exist.

## Lifecycle

### Open

`RpcInitConnection(...)` performs this order:

1. Close any previous command socket and callback listener.
2. Open command TCP socket.
3. Send `0x8080`.
4. Start callback UDP listener on `localAddress:callbackPort`.
5. Send `0x405C` with the same local address and callback port.
6. Mark the connection as RPC-initialized.

If any step fails, the command socket and callback listener are both closed.

### Close

`CloseConnection()` and `Dispose()` perform this order:

1. Send `0x405D` on the command TCP socket if possible.
2. Close the command TCP socket.
3. Stop the callback listener.
4. Clear connection state and cached handshake responses.

### Reconnect

Calling `RpcInitConnection(...)` again first closes both sockets from the
previous session, then starts a new session.

## Current Limitation

The listener does not interpret callback payloads yet. Consumers can log or
capture `CallbackReceived.Payload` and `CallbackReceived.RemoteEndPoint` to
build the parser later.


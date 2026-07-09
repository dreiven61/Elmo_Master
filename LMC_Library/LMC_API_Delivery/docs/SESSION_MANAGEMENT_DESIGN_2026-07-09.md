# LMC session management design

Date: 2026-07-09

## Scope

This document defines the session-management direction for the LASAL-oriented
`LasalMotionControlLib` TCP protocol when more than one PC can connect to the
same controller.

This is not an Elmo PMAS/MMCLib compatibility requirement. The target is a
LASAL-fit protocol that keeps the current LMC object model:

1. `LMCConnection` owns the TCP connection.
2. `LMCSingleAxis` and `LMCGroupAxis` resolve a name once.
3. Motion/read methods use the stored axis or group reference.

## Verified Current State

- `LMCConnection.RpcInitConnection(...)` opens a `TcpClient`; it does not
  perform an RPC/session handshake.
- The current request header is 8 bytes:
  - `[0] UINT CommandId`
  - `[4] UINT PayloadLength`
  - `[6] UINT Reference`
- The current header has no session id field.
- LASAL `TCPMotionInterface.Response(pData, udSize, dSock)` receives the socket
  number from the TCP server callback.
- LASAL `TCPMotionInterface.ConnSocketInfo(dSock, InfoPara1, InfoPara2)` is
  called on socket connect/disconnect.
- The current LASAL implementation has a global `CurrentSock` and many response
  paths call `SendData(..., dSocket:=CurrentSock, ...)`.

## Problem

With one PC, a global `CurrentSock` is usually enough by accident.

With more than one PC, it is not enough:

- Responses can be sent to the wrong socket if `CurrentSock` is overwritten by a
  later connection or callback.
- Axis/group references are not session-scoped.
- Two PCs can issue conflicting motion commands to the same axis or group.
- Disconnect cleanup cannot reliably release command ownership without a session
  table.

## Decision

Use a lightweight LMC session layer, not a full Elmo-style RPC/callback stack.

Initial implementation should be socket-scoped:

- Keep the existing 8-byte command header unchanged.
- Treat one TCP socket as one controller session.
- Use `dSock` as the authoritative lookup key on the LASAL side.
- Store an optional generated `SessionId` for diagnostics and API visibility,
  but do not add `SessionId` to every motion packet yet.

This avoids changing every payload offset in `LmcProtocol.cs` and
`TCPMotionInterface.st`.

## Session Lifecycle

### Connect

On `ConnSocketInfo(... TCP_SVR_SOCK_INFO_CONNECT ...)`:

- Allocate or refresh a session slot for `dSock`.
- Assign a monotonically increasing `SessionId`.
- Store connection state and last activity.
- Do not assign axis/group ownership yet.

### Request

On `Response(pData, udSize, dSock)`:

- Resolve the session by `dSock`.
- Set the active response socket from `dSock`, not from the last connected
  client.
- Validate frame length and command id.
- For read-only commands, execute without ownership changes.
- For motion/control commands, check target ownership.

### Close

On `LMC_CloseConnection` / command `0x405D` or socket disconnect:

- Release all axis/group ownership held by that session.
- Clear the session slot for `dSock`.
- Close only that socket/session, not every connected client.

### Timeout

If a session is idle past the configured timeout:

- Release its axis/group ownership.
- Mark the session inactive.
- Let the TCP server close the socket or reject later packets with an invalid
  session error.

## Ownership Policy

Read-only commands should be allowed from every session:

- `GetAxisByName`
- `GetGroupByName`
- `AxisInfo`
- `ReadStatus`
- `ReadPosition`
- group read/status commands

Control/motion commands need ownership:

- `Power`
- `Reset`
- `Stop`
- `MoveAbsolute`
- `MoveRelative`
- `MoveVelocity`
- group enable/disable/reset/stop/move commands

Recommended first implementation:

- Use implicit ownership.
- The first control/motion command for a free axis/group assigns ownership to
  the session.
- A different session receives a busy response for that target.
- `Stop` should be accepted from the owner. A later safety policy can decide
  whether emergency stop is globally accepted.
- Ownership is released by `CloseConnection`, disconnect, timeout, or explicit
  release commands if added later.

## Optional Administrative Commands

The existing `0x405D` close command should remain the close-session command.

Additional local LMC administrative commands can be added later after checking
for command-id collisions:

- `OpenSession`: return protocol version and generated `SessionId`.
- `Heartbeat`: keep the session active without touching motion state.
- `AcquireAxis` / `ReleaseAxis`: explicit ownership instead of implicit lock.
- `AcquireGroup` / `ReleaseGroup`: explicit group ownership.

Do not add these commands until the LASAL parser and PC DLL are changed in the
same commit.

## Required PC DLL Changes

Minimum:

- Add `LMCConnection.SessionId` and `LMCConnection.IsConnected`.
- Keep `RpcInitConnection(...)` as the public connection entry point.
- Keep motion packets unchanged.
- Make `CloseConnection()` send `0x405D` once the LASAL close-session handler is
  implemented.

Optional:

- Send `OpenSession` after TCP connect when the LASAL side supports it.
- Parse returned protocol version/session id.

## Required LASAL Changes

Minimum:

- In `Response(pData, udSize, dSock)`, route responses to the same `dSock` that
  sent the request.
- Add a session table keyed by `dSock`.
- On connect/disconnect callbacks, register and unregister only that socket.
- Check ownership before executing control/motion commands.
- Return deterministic busy/invalid-session errors.

Do not rely on one global `CurrentSock` as the identity of the active client.

## Verification

PC side:

- `dotnet build LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.sln -c Release`
- Confirm motion frame offsets in `LmcProtocol.cs` are unchanged.

LASAL side:

- Confirm all `SendData(... dSocket:=...)` response paths use the request socket
  or a session socket derived from it.
- Test two PC clients:
  - PC A connects and resolves `a01`.
  - PC B connects and resolves `a01`.
  - PC A sends a read command and receives its own response.
  - PC B sends a read command and receives its own response.
  - PC A starts a motion/control command.
  - PC B receives busy for conflicting motion/control on the same axis/group.
  - PC A disconnect releases ownership.


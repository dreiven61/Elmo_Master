# RPC connection packet decision

Date: 2026-07-09

## Source Evidence

Primary packet analysis already exists here:

- `LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md`
- `LMC_Library/LMC_API/Elmo_API_Packet2/TXT/MMC_RpcInitConnection.txt`
- `LMC_Library/LMC_API/Elmo_API_Packet2/TXT/MMC_CloseConnection.txt`

This delivery document records the subset used by `LasalMotionControlLib`.

## Captured RPC Connection Sequence

All multi-byte numeric fields are little-endian except the IPv4 address bytes,
which are copied in normal IPv4 byte order.

### Step 1: RPC Session Init

Request:

```text
80 80 00 00 01 00 00 00 00
```

Parsed request:

| Offset | Size | Value | Meaning |
|---:|---:|---|---|
| 0 | 2 | `0x8080` | RPC session init command |
| 2 | 2 | `0x0000` | reserved |
| 4 | 2 | `0x0001` | payload length |
| 6 | 2 | `0x0000` | reference |
| 8 | 1 | `0x00` | init payload byte |

Captured normal response is 32 bytes and uses response payload length `0x0018`:

```text
00 00 18 00 00 00 00 00
40 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00
```

Payload offset `0` is the observed DWORD `64`. It may be a handle, but its
meaning is not confirmed by one capture. The LASAL phase-1 implementation
returns the captured shape and uses request `dSock` as the actual session key.

### Step 2: RPC Callback Registration

Captured request:

```text
5c 40 00 00 0c 00 00 00 ff ff ff ff 8b 13 00 00 c0 a8 63 0e
```

Parsed request:

| Offset | Size | Value | Meaning |
|---:|---:|---|---|
| 0 | 2 | `0x405C` | callback registration command |
| 2 | 2 | `0x0000` | reserved |
| 4 | 2 | `0x000C` | payload length |
| 6 | 2 | `0x0000` | reference |
| 8 | 4 | `0xFFFFFFFF` | event mask |
| 12 | 4 | `5003` | callback port |
| 16 | 4 | `192.168.99.14` | local IPv4 address bytes |

Captured normal response uses response payload length `0x0004`. The 4-byte
payload is `UINT16 Status` followed by `INT16 ErrorId`.

### Close Connection

Request:

```text
5d 40 00 00 01 00 00 00 00
```

Parsed request:

| Offset | Size | Value | Meaning |
|---:|---:|---|---|
| 0 | 2 | `0x405D` | close connection command |
| 2 | 2 | `0x0000` | reserved |
| 4 | 2 | `0x0001` | payload length |
| 6 | 2 | `0x0000` | reference |
| 8 | 1 | `0x00` | close payload byte |

Captured normal response uses response payload length `0x0004` and the same
4-byte status/error payload as callback registration.

## Implementation Decision

`LMCConnection.RpcInitConnection(...)` must now perform the captured RPC
connection sequence:

1. Open TCP.
2. Send `0x8080` session init.
3. Send `0x405C` callback registration.
4. Mark the connection RPC-initialized only after both responses are successful.

`LMCConnection.CloseConnection()` and `Dispose()` must send `0x405D` before
closing the socket.

Because `0x405C` advertises a callback endpoint, `LMCConnection` must also own
and close the callback listener socket. See
`CALLBACK_LISTENER_DESIGN_2026-07-09.md`.

Motion, axis lookup, and group packets keep the existing 8-byte request header.
No session id is added to motion frames in this step.

## Implementation Update 2026-07-10

- PC DLL starts the UDP listener before `0x405C` and advertises the listener's
  actual bound port, including when the caller requested port `0`.
- PC DLL parses the 4-byte `0x405C` acknowledgement and rejects nonzero
  status/error.
- Reconnect performs a best-effort `0x405D` before creating the new TCP socket.
- The tracked LASAL `TCPMotionInterface` now validates the request header and
  implements `0x8080`, `0x405C`, and `0x405D` for one active RPC session.
- LASAL IDE compilation and real PLC packet verification are still required.

See `RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`.

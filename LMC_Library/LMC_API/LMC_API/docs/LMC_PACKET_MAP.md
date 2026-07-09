# LASAL LMC Packet Map

기준 구현은 `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`이다.

이 문서는 현재 LASAL 전용 DLL이 실제로 생성하는 request frame을 정리한다.
기존 Elmo/PMAS 패킷의 double/float payload 크기를 그대로 따르지 않는다.
현재 DLL은 호출자가 이미 변환한 LASAL internal DINT 값을 little-endian
`Int32`로 쓴다.

## Header

Request header는 8바이트다.

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 2 | Command ID |
| 2 | 2 | 예약 영역. 현재 request builder는 0으로 둔다. |
| 4 | 2 | Payload length |
| 6 | 2 | Axis 또는 group reference |

Response header도 8바이트로 파싱한다.

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 2 | Header status |
| 2 | 2 | Payload length |
| 4 | 4 | Header reserved |

Payload 시작 offset은 항상 8이다.

## 구현된 request packet

| Public API / 내부 동작 | Command ID | Payload | Request size | Payload layout |
|---|---:|---:|---:|---|
| `RpcInitConnection` session init | `0x8080` | 1 | 9 | payload 1 byte, zero |
| `RpcInitConnection` callback register | `0x405C` | 12 | 20 | `UInt32 eventMask`, `Int32 callbackPort`, IPv4 4 bytes |
| `CloseConnection` | `0x405D` | 1 | 9 | payload 1 byte, zero |
| axis name lookup | `0x103C` | 80 | 88 | ASCII axis name, max 79 bytes, zero padded |
| group name lookup | `0x1042` | 80 | 88 | ASCII group name, max 79 bytes, zero padded |
| `LMCSingleAxis` constructor axis info | `0x202B` | 12 | 20 | `Int32 mode=5`, reserved, `Int32 enable=1` |
| `PowerOn` / `PowerOff` | `0x2023` | 8 | 16 | `Int32 1`, enable byte, captured control flag bytes |
| `Reset` | `0x2024` | 1 | 9 | execute byte `1` |
| `ReadStatus` | `0x2028` | 8 | 16 | `Int32 axisReference`, `Int32 1` |
| `GetActualPosition` | `0x202E` | 1 | 9 | payload 1 byte, zero |
| `Stop` | `0x2022` | 16 | 24 | `Int32 deceleration`, `Int32 jerk`, `Int32 1`, `Int32 1` |
| `MoveAbsoluteEx` | `0x209F` | 32 | 40 | `Int32 position`, `velocity`, `acceleration`, `deceleration`, `jerk`, `direction`, `1`, `1` |
| `MoveRelativeEx` | `0x20A0` | 32 | 40 | `Int32 distance`, `velocity`, `acceleration`, `deceleration`, `jerk`, `direction`, `1`, `1` |
| `MoveVelocityEx` | `0x20A2` | 24 | 32 | `Int32 velocity`, `acceleration`, `deceleration`, `jerk`, `direction`, `1` |
| `GetGroupMembersInfo` | `0x20D2` | 1 | 9 | execute byte `1` |
| `GroupReadStatus` | `0x2045` | 8 | 16 | `Int32 0`, `Int32 1` |
| `GroupEnable` | `0x2047` | 1 | 9 | execute byte `1` |
| `GroupDisable` | `0x2048` | 1 | 9 | execute byte `1` |
| `GroupReset` | `0x2049` | 1 | 9 | execute byte `1` |
| `GroupStop` | `0x2085` | 16 | 24 | `Int32 deceleration`, `Int32 jerk`, `Int32 1`, `Int32 1` |
| `MoveLinearAbsoluteEx` | `0x20A4` | 96 | 104 | 16 axis positions as `Int32`, `velocity`, `acceleration`, `deceleration`, `jerk`, transition/reserved fields, `1`, `1` |

## Response parsing

`LMC_Response` stores the raw response, header status, payload length, header
reserved field, payload, and optional command result.

Current value parsers:

- `ReadStatus` parses `UInt32` from response payload offset 0.
- `GetActualPosition` parses `Int32` from response payload offset 0.
- acknowledgement responses use payload offset 4 as command status and offset 6
  as error id when the payload is at least 8 bytes.

## 캡처됐지만 현재 DLL 함수가 없는 packet

| Packet | Command ID | 현재 상태 |
|---|---:|---|
| `GroupReadActualPosition` | `0x2051` | command 상수만 있음. public API, frame builder, 16-axis vector response parser 없음. |
| `SetKinTransformCartesian4Axis` | `0x20E7` | public API와 frame builder 없음. LASAL group 운용에서 필요 확정 시 구현. |
| `PowerMembers` | 없음 | 단일 packet이 아니라 사용자 프로그램이 멤버 축을 순회하며 `PowerOn`/`PowerOff`를 호출하는 helper 동작. |

## 비사용 legacy 기준

이 문서는 현재 LASAL DINT DLL 기준이다. 아래 기준은 더 이상 현재 DLL 완료
기준으로 쓰지 않는다.

- `LMC_*Cmd` public method name
- PMAS/Elmo double payload size
- `Stop`/`GroupStop` float deceleration/jerk payload
- API 내부 unit converter

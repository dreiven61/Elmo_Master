# LASAL LMC Packet Map

기준 구현은 `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`이다.
검토 기준일은 2026-07-10이다.

이 문서는 현재 LASAL 전용 DLL이 실제로 생성하는 request frame을 정리한다.
기존 Elmo/PMAS 패킷의 double/float payload 크기를 그대로 따르지 않는다.
현재 DLL은 호출자가 이미 변환한 LASAL internal DINT 값을 little-endian
`Int32`로 쓴다. 호출자는 `physical value x LMC_Units.<UNIT>`로 변환하고
read 결과는 같은 UNIT으로 나눈다. 상세 규칙은
[`UNIT_CONVERSION_MANUAL_2026-07-10.md`](../../../LMC_API_Delivery/docs/UNIT_CONVERSION_MANUAL_2026-07-10.md)를
따른다.

중요:

- 이 문서의 '구현'은 C# request serializer 기준이다.
- tracked LASAL `TCPMotionInterface`와의 end-to-end 지원을 뜻하지 않는다.
- tracked LASAL source에는 RPC phase-1 handler와 request header 교정이
  반영됐지만 LASAL IDE/PLC 검증 전이며 motion command type/dispatch는
  아직 현재 DLL과 다르다.
- 상세 backlog는
  [`API_DEVELOPMENT_BACKLOG_2026-07-10.md`](../../../LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md)를
  따른다.

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

## C#에서 생성하는 request packet

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
| `MoveLinearAbsoluteEx` | `0x20A4` | 96 | 104 | `Int32 position[16]`, velocity, acceleration, deceleration, jerk, coordinate=`0`, transition=`0`, buffer=`1`, execute=`1` |

### C# request와 캡처/LASAL의 차이

- `GroupReadStatus(0x2045)`의 PMAS capture는 payload 첫 DINT에 group
  reference `0x0100`을 넣지만 C# builder는 `0`을 쓴다.
- PMAS capture의 MoveAbsolute/Relative/Linear는 LREAL, Stop/GroupStop은
  REAL이다. 현재 C#은 의도적으로 LASAL DINT payload를 만든다.
- tracked LASAL은 legacy header와 LREAL offset을 사용하고 `_Edit`도
  motion payload는 LREAL이 남아 있다.
- `MoveLinearAbsoluteEx`의 public API는 coordinate/transition/buffer mode를
  받지 않고 위 값을 hard-code한다.
- DINT contract의 실제 PLC request/response capture가 아직 없다.

## Response parsing

`LMC_Response` stores the raw response, header status, payload length, header
reserved field, payload, and optional command result.

Current value parsers:

- `ReadStatus` parses `UInt32` from response payload offset 0.
- `GetActualPosition` parses `Int32` from response payload offset 0.
- 4-byte acknowledgement는 payload offset 0/2의 status/error를 읽는다.
- 8-byte 이상 acknowledgement는 payload offset 4/6을 읽는다.

현재 parser는 완료 상태가 아니다.

- 4-byte/8-byte ACK 분기는 2026-07-10에 교정됐다. 다만 모든 command가
  generic ACK 구조를 쓰는 것은 아니므로 command별 parser가 계속 필요하다.
- `GetGroupMembersInfo(0x20D2)`의 1350-byte structured response를 현재
  `ParseAcknowledgement`로 처리해 member reference를 status/error로 오인한다.
- AxisInfo response는 읽고 버린다.
- ReadStatus/position/group-status의 value 뒤 error tail을 충분히 해석하지
  않는다.
- value parser 실패도 숫자 `0`을 반환하므로 정상값 0과 구분되지 않는다.
- callback payload는 raw bytes이며 typed parser가 없다.

`0x20D2` capture response layout:

| Payload offset | Size | 의미 |
|---:|---:|---|
| 0 | 32 | AxisReference `UINT16[16]` |
| 32 | 32 | DeviceId `UINT16[16]` |
| 64 | 2 | Status |
| 66 | 2 | Error ID |
| 68 | 1280 | AxisName `CHAR[16][80]` |
| 1348 | 1 | Axis count |
| 1349 | 1 | padding |

## 부분 구현 또는 현재 DLL 함수가 없는 packet

| Packet | Command ID | 현재 상태 |
|---|---:|---|
| `GetGroupMembersInfo` | `0x20D2` | request/public API는 있으나 structured response parser가 잘못됨 |
| `GroupReadStatus` | `0x2045` | request payload가 capture와 다르고 value tail parser가 부분적 |
| `GroupReadActualPosition` | `0x2051` | command 상수만 있음. public API, frame builder, vector response parser 없음 |
| `SetKinTransformEx/Cartesian` | `0x20E7` | 1320-byte layout은 확인했으나 상수, public API, frame builder, LASAL handler 없음 |
| `PowerMembers` | 없음 | 단일 packet이 아니라 application/test helper에서 축별 `PowerOn/Off` 반복 |

`0x2051` captured request는 coordinate DINT + enable BYTE + padding이고
response는 LREAL[16] + status/error + padding이다. `0x20E7`은
`MC_KIN_NODE_DEF[16]`, `MC_KIN_REF`, kinematic type, buffer, execute로 구성된
`MMC_SETKINTRANSFORMEX_IN`/Cartesian wrapper다. 자세한 offset은
`PACKET_ANALYSIS.md`를 따른다.

## Live LASAL compatibility

| 영역 | tracked project | untracked `_Edit` |
|---|---|---|
| Request header | v1 `[Cmd@0, Len@4, Ref@6]`와 단일-socket stream accumulator 반영 | 새 header 일부 반영 |
| RPC `0x8080/405C/405D` | 단일-session phase-1 코드 반영, IDE/PLC 미검증 | 없음 |
| Axis/group lookup | 없음 | axis 임시 매핑만, group 없음 |
| Motion payload | LREAL/legacy | LREAL과 DINT header가 혼재 |
| 4-axis dispatch | `_LMCAxis1` 한 축 연결 | `_LMCAxis1` 한 축 연결 |
| Group commands | 일부 legacy | case만 있고 실행 주석 다수 |

현재 C# DINT DLL과 LASAL motion path는 아직 end-to-end 호환되지 않는다.
tracked project의 RPC phase-1을 LASAL IDE/PLC에서 검증한 뒤 lookup과 target
dispatch를 완료해야 한다.

## 비사용 legacy 기준

이 문서는 현재 LASAL DINT DLL 기준이다. 아래 기준은 더 이상 현재 DLL 완료
기준으로 쓰지 않는다.

- `LMC_*Cmd` public method name
- PMAS/Elmo double payload size
- `Stop`/`GroupStop` float deceleration/jerk payload
- API 내부 unit converter

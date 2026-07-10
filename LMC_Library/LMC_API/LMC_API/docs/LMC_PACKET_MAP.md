# LASAL LMC Packet Map

기준 구현: `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`

검토 기준일: 2026-07-10

이 문서의 "PC 구현"은 C# request serializer와 response parser를 뜻한다.
LASAL IDE/PLC end-to-end 완료를 뜻하지 않는다. 현재 PC는 대상 command
23/23을 만들 수 있지만 실제 PLC 재캡처 완료는 0/23이다.

## 공통 규칙

- motion numeric field: little-endian signed `DINT`/`Int32`
- 단위 변환: DLL에서 하지 않음
- 호출: `physical value * LMC_Units.<UNIT>`를 반올림·DINT 범위 검사 후 전달
- read 표시: raw DINT를 같은 UNIT으로 나눔
- request reference: 이름 lookup으로 받은 opaque `UINT16` descriptor
- callback: UDP raw datagram, typed payload 미정

PMAS capture의 LREAL/REAL payload는 구조 분석 근거일 뿐 LASAL-DINT v1
response type으로 자동 수용하지 않는다.

## Header

Request header는 8 bytes다.

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 2 | Command ID `UINT16` |
| 2 | 2 | reserved `0` |
| 4 | 2 | payload length `UINT16` |
| 6 | 2 | axis/group reference `UINT16` |

Response header도 8 bytes다.

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 2 | header status |
| 2 | 2 | payload length |
| 4 | 4 | header reserved |

Payload offset은 frame 시작 기준 8이다.

## PC request map

| Public API / 내부 동작 | Command | Payload | Frame | Payload layout |
|---|---:|---:|---:|---|
| session init | `0x8080` | 1 | 9 | zero byte |
| callback register | `0x405C` | 12 | 20 | event mask `UInt32`, port `Int32`, IPv4 4B |
| close | `0x405D` | 1 | 9 | zero byte |
| axis lookup | `0x103C` | 80 | 88 | strict ASCII name, NUL padded |
| group lookup | `0x1042` | 80 | 88 | strict ASCII name, NUL padded |
| AxisInfo | `0x202B` | 12 | 20 | mode `5`, reserved, enable `1` |
| PowerOn/Off | `0x2023` | 8 | 16 | control DINT `1`, enable byte, captured flags |
| Reset | `0x2024` | 1 | 9 | execute `1` |
| ReadStatus | `0x2028` | 8 | 16 | axis reference DINT, enable DINT `1` |
| GetActualPosition | `0x202E` | 1 | 9 | zero byte |
| Stop | `0x2022` | 16 | 24 | deceleration, jerk, buffer `1`, execute `1` |
| MoveAbsoluteEx | `0x209F` | 32 | 40 | position, velocity, acceleration, deceleration, jerk, direction, buffer `1`, execute `1` |
| MoveRelativeEx | `0x20A0` | 32 | 40 | distance와 나머지는 MoveAbsoluteEx와 동일 |
| MoveVelocityEx | `0x20A2` | 24 | 32 | velocity, acceleration, deceleration, jerk, direction, execute `1` |
| GetGroupMembersInfo | `0x20D2` | 1 | 9 | execute `1` |
| GroupReadStatus | `0x2045` | 8 | 16 | group reference DINT, enable DINT `1` |
| GroupEnable | `0x2047` | 1 | 9 | execute `1` |
| GroupDisable | `0x2048` | 1 | 9 | execute `1` |
| GroupReset | `0x2049` | 1 | 9 | execute `1` |
| GroupStop | `0x2085` | 16 | 24 | deceleration, jerk, buffer `1`, execute `1` |
| MoveLinearAbsoluteEx | `0x20A4` | 96 | 104 | position DINT[16], 4 dynamics DINT, coordinate, transition, buffer, execute |
| GroupReadActualPosition | `0x2051` | 8 | 16 | coordinate DINT, enable byte `1`, padding 3B |
| SetKinTransformCartesian4Axis | `0x20E7` | 1320 | 1328 | exact Cartesian4 profile, 아래 상세 |

`MoveLinearAbsoluteEx` position은 1..16개만 허용하고 나머지 slot을 0으로
채운다. coordinate/transition/buffer enum은 `LMCGroupMotionOptions`로
명시하며 잘못된 enum 값은 frame 생성 전에 거부한다.

## `0x2051 GroupReadActualPosition`

### Request payload: 8 bytes

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 4 | `LMC_COORD_SYSTEM` DINT (`None=0`, `ACS=1`, `MCS=2`, `PCS=3`) |
| 4 | 1 | enable `1` |
| 5 | 3 | padding `0` |

위 enum은 PMAS capture 기준 wire 값이다. LASAL `_LMCRobotBase`의
base-coordinate index와 같은 의미라고 가정하면 안 되며 LASAL handler에서
명시적으로 mapping해야 한다.

### LASAL-DINT v1 success response payload: exactly 68 bytes

| Offset | Size | 의미 |
|---:|---:|---|
| 0 | 64 | actual position `DINT[16]` |
| 64 | 2 | function status `UINT16` |
| 66 | 2 | error ID `INT16` |

4-byte command-error envelope는 unsuccessful typed result로 보존한다.
PMAS capture response는 136 bytes
(`LREAL[16] + UINT16 status + INT16 error + ABI padding 4B`)다. 이것은
LASAL-DINT v1이 아니며 PC typed parser가 명시적으로 거부한다.

## `0x20E7 SetKinTransformCartesian4Axis`

두 PMAS capture의 1,328-byte application frame은 byte-identical했다.
PC serializer는 그 frame의 1,320-byte payload layout을 정확히 재현한다.
captured full-frame SHA-256은
`678d4844a881e6978f83dadbcf7e27a92b19ac940a9973241c7013956b2d34cf`다.
이 hash golden은 capture의 node handle `0/1/2/3`을 넣은 internal serializer
검증값이다. public API는 현재 session lookup에서 받은 axis reference를 쓰므로
해당 값이 다르면 layout은 같아도 전체 frame hash는 달라진다.

| Payload offset | Size | 의미 |
|---:|---:|---|
| 0..639 | 640 | `MC_KIN_NODE_DEF[16]`, node당 40B |
| 640 | 4 | `iNumAxes = 4` |
| 644..1303 | 660 | `MC_KIN_REF` union aligned remainder |
| 1304 | 4 | `eKinType = 0` Cartesian |
| 1308 | 4 | buffer mode `2` (`Buffered`) |
| 1312 | 1 | execute `1` |
| 1313..1319 | 7 | ABI tail padding `0` |

각 40-byte node:

| Node offset | Size | 의미 |
|---:|---:|---|
| 0 | 8 | backward ratio `double` |
| 8 | 8 | forward ratio `double` |
| 16 | 8 | backward shift `double` |
| 24 | 4 | transform function `Int32` |
| 28 | 4 | axis reference/handle `UInt32` |
| 32 | 4 | axis type `Int32` |
| 36 | 4 | padding |

공개 API는 캡처된 profile만 허용한다.

- node 0..3: X/Y/Z/U
- 같은 connection에서 lookup한 고유 axis reference 4개
- ratios `1.0`, shift `0.0`, transform `Shift(1)`
- node 4..15와 union remainder `0`
- Cartesian `0`, `Buffered(2)`, execute `1`
- 응답 payload는 캡처와 같은 4-byte `status/error`만 허용하며 generic
  8-byte ACK는 거부

generic node count, 다른 coefficient/type/buffer 조합은 추가 캡처가 없어
지원 완료로 표시하지 않는다. 현재 LASAL receive buffer/handler도 이 큰
command를 처리하지 못하므로 PC serializer 단독으로는 E2E 동작하지 않는다.

## Response map

| Command | Payload | Typed parsing |
|---|---:|---|
| short ACK | exactly 4 | status `UINT16[0]`, error `INT16[2]` |
| normal ACK/AxisInfo | exactly 8 | status `UINT16[4]`, error `INT16[6]` |
| Axis ReadStatus | exactly 12 | state `UDINT[0]`, status/error `[4]/[6]`, axis error `[8]`, status word `[10]` |
| Axis GetActualPosition | exactly 8 | position `DINT[0]`, status/error `[4]/[6]` |
| Group ReadStatus | exactly 12 | state `UDINT[0]`, status/error `[4]/[6]`, group error `[8]` |
| Group ReadActualPosition | exactly 68 | DINT[16], status/error `[64]/[66]` |
| GetGroupMembersInfo | exactly 1350 | 아래 구조 |

`GetGroupMembersInfo(0x20D2)` response:

| Payload offset | Size | 의미 |
|---:|---:|---|
| 0 | 32 | AxisReference `UINT16[16]` |
| 32 | 32 | DeviceId `UINT16[16]` |
| 64 | 2 | function status |
| 66 | 2 | error ID |
| 68 | 1280 | AxisName `CHAR[16][80]` |
| 1348 | 1 | axis count 0..16 |
| 1349 | 1 | padding |

Typed parser는 truncated/trailing payload와 legacy response shape를 정상값
`0`으로 숨기지 않고 `InvalidDataException` 또는 typed error result로
구분한다. ACK parser도 4/8 bytes만 허용한다.

## PC/PLC 상태

| 구분 | 상태 |
|---|---|
| PC request/public path | 23/23 |
| PC 자동 테스트 | 42/42 PASS |
| LASAL static source contract | PASS |
| tracked LASAL handler | 21/23 |
| `0x2049`, `0x2085` | handler는 있으나 unsupported `-5` |
| `0x2051`, `0x20E7` | PC 구현, LASAL handler 없음 |
| callback | PC raw UDP listener; LASAL sender/payload 없음 |
| multi-PC ownership | LASAL policy/구현 필요 |
| PLC E2E 재캡처 | 0/23 |

실제 motion 전에는 LASAL command queue/RtWork, large-command staging,
coordinate mapping, ownership, IDE build와 PLC 재캡처를 완료해야 한다.

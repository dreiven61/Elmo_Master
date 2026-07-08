# Elmo API Packet2 Packet Analysis

작성일: 2026-07-08

분석 대상:

- `TXT/*.txt`: Wireshark에서 뽑은 Ethernet hex dump
- `WireShark/*.pcapng`: 원본 캡처

분석 방법:

- TCP payload 길이는 Ethernet frame 길이가 아니라 `IP total length - IP header length - TCP header length`로 계산했다.
- ACK-only packet은 payload 분석에서 제외했다.
- 멀티바이트 값은 little-endian 기준으로 해석했다.

## 핵심 결론

현재 캡처는 "모든 motion 값을 하나의 고정 비율로 DINT 변환해서 LASAL에 보낸다"는 구조와 맞지 않는다.

확인된 사실:

- PMAS/MMCLib 패킷은 요청 프레임과 응답 프레임의 header layout이 다르다.
- axis/group lookup 후 reference를 받아서 후속 command header offset `[6]`에 넣는다.
- `MoveAbsoluteEx`, `MoveRelativeEx`, `MoveLinearAbsoluteEx`의 motion 값은 8-byte `LREAL/double`이다.
- `Stop`, `GroupStop`의 deceleration/jerk 값은 4-byte `REAL/float`이다.
- `MoveVelocityEx`는 캡처상 header의 payload length와 실제 TCP payload 길이가 일치하지 않는다.
- 단순히 `8388608 -> 3600000` 같은 고정 변환으로 DLL을 만드는 것은 근거가 부족하다.

판단 업데이트:

- PMAS/MMCLib의 axis interface 단위 정보와 LASAL application unit 체계는 1:1로 대응된다고 보면 안 된다.
- LASAL 이식 DLL은 우선 LASAL 프로젝트의 `Include/unit.h` define을 기준으로 application unit 값을 DINT로 변환해야 한다.
- 현재 캡처의 `0x202B AxisInfo` 응답만으로는 LASAL `unit.h` 기준 단위 변환식을 확인할 수 없다. `unit.h`와 실제 `_LMCAxis`/`_LMCRobot` 메서드 인자 단위 주석을 같이 봐야 한다.

## 공통 헤더

### Request Header

요청 프레임은 아래 구조로 보인다.

| Offset | Size | Type | 의미 |
|---:|---:|---|---|
| 0 | 2 | UINT16 | Command ID |
| 2 | 2 | UINT16 | Reserved, 캡처상 대부분 `0` |
| 4 | 2 | UINT16 | Payload length |
| 6 | 2 | UINT16 | Axis 또는 group reference |
| 8 | N | payload | command별 payload |

예: `MoveAbsoluteEx`

```text
9f 20 00 00 38 00 00 00 ...
cmd=0x209F, payloadLength=0x0038(56), reference=0
```

예: group command

```text
47 20 00 00 01 00 00 01 01
cmd=0x2047, payloadLength=1, reference=0x0100
```

### Response Header

응답 프레임은 요청과 다르다. payload length가 offset `[2]`에 있다.

| Offset | Size | Type | 의미 |
|---:|---:|---|---|
| 0 | 2 | UINT16 | 응답 status 또는 reserved. 정상 응답 캡처는 `0` |
| 2 | 2 | UINT16 | Response payload length |
| 4 | 4 | UINT32 | Reserved, 캡처상 `0` |
| 8 | N | payload | 응답 데이터 |

예:

```text
00 00 08 00 00 00 00 00 ...
responsePayloadLength=8
```

주의:

- 요청은 length를 `[4]`에서 읽어야 한다.
- 응답은 length를 `[2]`에서 읽어야 한다.
- 같은 parser로 request/response header를 처리하면 틀린다.

## Reference Lookup

### `0x103C GetAxisByName`

캡처 파일:

- `WireShark/MMC_MoveAbsoluteExCmd.pcapng`
- `WireShark/MMC_ReadActualPositionCmd.pcapng`
- `WireShark/MMC_PowerCmd_On_1Axis.pcapng`

Request:

| Offset | Type | 값 |
|---:|---|---|
| 0 | UINT16 | `0x103C` |
| 4 | UINT16 | `80` |
| 8 | ASCII[80] | axis name, 예: `a01` |

Response:

| Offset | Type | 의미 |
|---:|---|---|
| 2 | UINT16 | response payload length = `6` |
| 12 | UINT16 | axis reference |

캡처상 reference:

| Name | Reference |
|---|---:|
| `a01` | `0` |
| `a02` | `1` |
| `a03` | `2` |
| `a04` | `3` |

### `0x1042 GetGroupByName`

캡처 파일:

- `WireShark/MMC_GetGroupMembersInfo.pcapng`
- `WireShark/MMC_GroupEnableCmd.pcapng`
- `WireShark/MMC_MoveLinearAbsoluteExCmd.pcapng`

Request:

| Offset | Type | 값 |
|---:|---|---|
| 0 | UINT16 | `0x1042` |
| 4 | UINT16 | `80` |
| 8 | ASCII[80] | group name, 예: `v01` |

Response:

| Offset | Type | 의미 |
|---:|---|---|
| 2 | UINT16 | response payload length = `6` |
| 12 | UINT16 | group reference |

캡처상 `v01` group reference는 `0x0100`이다.

## Axis Interface 관련 관찰

### `0x202B AxisInfo`

axis lookup 후 축별로 `0x202B`가 호출된다.

Request 예:

```text
2b 20 00 00 0c 00 00 00 05 00 00 00 00 00 00 00 01 00 00 00
```

| Offset | Type | 값 |
|---:|---|---:|
| 0 | UINT16 | `0x202B` |
| 4 | UINT16 | `12` |
| 6 | UINT16 | axis reference |
| 8 | UINT32 | `5` |
| 12 | UINT32 | `0` |
| 16 | UINT32 | `1` |

Response는 16 bytes, response payload length는 `8`이다.

캡처만 놓고 보면 `0x202B` 응답에서 LASAL `unit.h` 스케일을 직접 확인할 수 없다. 따라서 단위 변환을 정확히 구현하려면 아래 근거를 같이 봐야 한다.

- LASAL 프로젝트 `Include/unit.h`
- `_LMCAxis`/`_LMCRobot` 메서드의 인자 단위 주석
- 필요한 경우 `0x202B` payload 인자의 의미를 Maestro/MMCLib API 문서에서 확인

## Motion Command Layout

### `0x209F MoveAbsoluteEx`

캡처 파일: `WireShark/MMC_MoveAbsoluteExCmd.pcapng`

Request:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 0 | UINT16 | Command ID | `0x209F` |
| 4 | UINT16 | Payload length | `56` |
| 6 | UINT16 | Axis reference | `0` |
| 8 | LREAL | Position | `838860800.0` |
| 16 | LREAL | Velocity | `83886080.0` |
| 24 | LREAL | Acceleration | `167772160.0` |
| 32 | LREAL | Deceleration | `167772160.0` |
| 40 | LREAL | Jerk | `83886080.0` |
| 48 | DINT | Direction | `2` |
| 52 | DINT | Buffer mode | `1` |
| 56 | DINT | Execute | `1` |
| 60 | DINT | Reserved | `0` |

Raw payload 시작:

```text
9f 20 00 00 38 00 00 00
00 00 00 00 00 00 c9 41
00 00 00 00 00 00 94 41
...
```

### `0x20A0 MoveRelativeEx`

캡처 파일: `WireShark/MMC_MoveRelativeExCmd.pcapng`

`MoveAbsoluteEx`와 layout이 같다. offset `[8]`만 absolute position이 아니라 relative distance다.

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 4 | UINT16 | Payload length | `56` |
| 8 | LREAL | Distance | `838860800.0` |
| 16 | LREAL | Velocity | `83886080.0` |
| 24 | LREAL | Acceleration | `167772160.0` |
| 32 | LREAL | Deceleration | `167772160.0` |
| 40 | LREAL | Jerk | `83886080.0` |
| 48 | DINT | Direction | `2` |
| 52 | DINT | Buffer mode | `1` |
| 56 | DINT | Execute | `1` |
| 60 | DINT | Reserved | `0` |

### `0x20A2 MoveVelocityEx`

캡처 파일: `WireShark/MMC_MoveVelocityExCmd.pcapng`

Request:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 0 | UINT16 | Command ID | `0x20A2` |
| 4 | UINT16 | Payload length field | `48` |
| 6 | UINT16 | Axis reference | `0` |
| 8 | LREAL | Velocity | `83886080.0` |
| 16 | LREAL | Acceleration | `167772160.0` |
| 24 | LREAL | Deceleration | `167772160.0` |
| 32 | LREAL | Jerk | `83886080.0` |
| 40 | DINT | Direction | `1` |
| 44 | DINT | Buffer mode | `1` |
| 48 | BYTE | Execute | `1` |

주의:

- 실제 TCP payload 길이는 `49` bytes였다.
- header의 payload length field는 `48`이다.
- `8 + 48 = 56`이므로 header length field와 실제 TCP payload 길이가 맞지 않는다.
- 이 캡처 기준으로는 `MoveVelocityEx`를 strict length parser로 처리하면 막힐 수 있다.

### `0x2022 Stop`

캡처 파일: `WireShark/MMC_StopCmd.pcapng`

Request:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 0 | UINT16 | Command ID | `0x2022` |
| 4 | UINT16 | Payload length | `16` |
| 6 | UINT16 | Axis reference | `0` |
| 8 | REAL | Deceleration | `1000000.0` |
| 12 | REAL | Jerk | `20000000.0` |
| 16 | DINT | Buffer mode | `1` |
| 20 | DINT | Execute | `1` |

`Stop`은 LREAL이 아니라 4-byte REAL을 사용한다.

### `0x2085 GroupStop`

캡처 파일: `WireShark/MMC_GroupStopCmd .pcapng`

Request:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 0 | UINT16 | Command ID | `0x2085` |
| 4 | UINT16 | Payload length | `16` |
| 6 | UINT16 | Group reference | `0x0100` |
| 8 | REAL | Deceleration | `167772160.0` |
| 12 | REAL | Jerk | `167772160.0` |
| 16 | DINT | Buffer mode | `1` |
| 20 | DINT | Execute | `1` |

### `0x20A4 MoveLinearAbsoluteEx`

캡처 파일: `WireShark/MMC_MoveLinearAbsoluteExCmd.pcapng`

Request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | Command ID = `0x20A4` |
| 4 | UINT16 | Payload length = `304` |
| 6 | UINT16 | Group reference = `0x0100` |
| 8 | LREAL[16] | Position vector |
| 136 | LREAL | Velocity |
| 144 | LREAL | Acceleration |
| 152 | LREAL | Deceleration |
| 160 | LREAL | Jerk |
| 168 | LREAL[16] | Transition parameters |
| 296 | DINT | Buffer mode |
| 300 | DINT | Coordinate system |
| 304 | DINT | Transition mode |
| 308 | BYTE | Superimposed |
| 309 | BYTE | Execute |
| 310 | BYTE[2] | Padding/reserved |

캡처 예:

- `Position[0..15]`: 캡처 파일의 이 호출은 앞쪽 vector가 대부분 `0.0`
- `Velocity`: `83,886,080.0`
- `Acceleration`: `83,886,080.0`
- `Deceleration`: `167,772,160.0`
- `Jerk`: `167,772,160.0`
- `Buffer mode`: `2`
- `Coordinate system`: `0`
- `Transition mode`: `1`
- `Superimposed`: `0`
- `Execute`: `1`

### `0x20E7 SetKinTransform`

캡처 파일:

- `WireShark/MMC_SetKinTransform .pcapng`
- `WireShark/Prepare Group MCS.pcapng`

Request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | Command ID = `0x20E7` |
| 4 | UINT16 | Payload length = `1320` |
| 6 | UINT16 | Group reference = `0x0100` |
| 8 | LREAL... | kinematic transform data |

캡처에서 `0x20E7` payload는 1320 bytes로 크다. 이 문서에서는 전체 구조를 확정하지 않았다. `SetKinTransform`은 별도 문서 또는 Maestro API 구조체 정의와 대조해야 한다.

## Read Command Layout

### `0x2028 ReadStatus`

캡처 파일:

- `WireShark/MMC_ReadStatusCmd_1Axis.pcapng`
- `WireShark/MMC_ReadStatusCmd_Group.pcapng`

Axis request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | `0x2028` |
| 4 | UINT16 | Payload length = `8` |
| 6 | UINT16 | Axis reference |
| 8 | DINT | Axis reference |
| 12 | DINT | Execute = `1` |

Axis response:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 2 | UINT16 | Response payload length | `12` |
| 8 | DWORD | Status register | `0x40000080` 또는 유사 값 |
| 12 | DWORD | 추가 값 또는 reserved | `0` |
| 16 | DWORD | Error/status tail | 캡처별 상이 |

### `0x202E ReadActualPosition`

캡처 파일: `WireShark/MMC_ReadActualPositionCmd.pcapng`

Request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | `0x202E` |
| 4 | UINT16 | Payload length = `1` |
| 6 | UINT16 | Axis reference |
| 8 | BYTE | `0` |

Response:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 2 | UINT16 | Response payload length | `16` |
| 8 | LREAL | Actual position | `132.0` |
| 16 | LREAL 또는 tail | `0.0` |

### `0x2045 GroupReadStatus`

캡처 파일: `WireShark/MMC_GroupReadStatusCmd.pcapng`

Request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | `0x2045` |
| 4 | UINT16 | Payload length = `8` |
| 6 | UINT16 | Group reference = `0x0100` |
| 8 | DINT | Group reference = `0x0100` |
| 12 | DINT | Execute = `1` |

Response payload length는 `12`이다. 캡처의 body 첫 4 bytes는 status 값으로 해석해야 한다.

### `0x2051 GroupReadActualPosition`

캡처 파일: `WireShark/MMC_GroupReadActualPosition.pcapng`

Request:

| Offset | Type | 의미 |
|---:|---|---|
| 0 | UINT16 | `0x2051` |
| 4 | UINT16 | Payload length = `8` |
| 6 | UINT16 | Group reference = `0x0100` |
| 8 | DINT | 캡처값 `2` |
| 12 | DINT | Execute = `1` |

Response:

| Offset | Type | 의미 | 캡처 예 |
|---:|---|---|---:|
| 2 | UINT16 | Response payload length | `136` |
| 8 | LREAL | Position[0] | `838857339.0` |
| 16 | LREAL | Position[1] | `838858578.0` |
| 24 | LREAL | Position[2] | `838858591.0` |
| 32 | LREAL | Position[3] | `838858970.0` |
| 40.. | LREAL... | 나머지 vector/tail | 캡처상 대부분 `0` |

## Command Summary

| Command | ID | Request payload length | 실제 TCP request length | 주요 타입 |
|---|---:|---:|---:|---|
| RpcInitConnection step 1 | `0x8080` | 1 | 9 | BYTE |
| RpcInitConnection callback | `0x405C` | 12 | 20 | DWORD, DWORD, IPv4 |
| CloseConnection | `0x405D` | 1 | 9 | BYTE |
| GetAxisByName | `0x103C` | 80 | 88 | ASCII[80] |
| GetGroupByName | `0x1042` | 80 | 88 | ASCII[80] |
| AxisInfo | `0x202B` | 12 | 20 | DWORD[3] |
| Power | `0x2023` | 8 | 16 | DWORD + BYTE flags |
| Reset | `0x2024` | 1 | 9 | BYTE |
| Stop | `0x2022` | 16 | 24 | REAL, REAL, DINT, DINT |
| ReadStatus | `0x2028` | 8 | 16 | DINT, DINT |
| ReadActualPosition | `0x202E` | 1 | 9 | BYTE |
| MoveAbsoluteEx | `0x209F` | 56 | 64 | LREAL[5], DINT[3] |
| MoveRelativeEx | `0x20A0` | 56 | 64 | LREAL[5], DINT[3] |
| MoveVelocityEx | `0x20A2` | 48 | 49 | LREAL[4], DINT[2], BYTE |
| GetGroupMembersInfo | `0x20D2` | 1 | 9 | BYTE |
| GroupEnable | `0x2047` | 1 | 9 | BYTE |
| GroupDisable | `0x2048` | 1 | 9 | BYTE |
| GroupReset | `0x2049` | 1 | 9 | BYTE |
| GroupStop | `0x2085` | 16 | 24 | REAL, REAL, DINT, DINT |
| GroupReadStatus | `0x2045` | 8 | 16 | DINT, DINT |
| GroupReadActualPosition | `0x2051` | 8 | 16 | DINT, DINT |
| MoveLinearAbsoluteEx | `0x20A4` | 304 | 312 | LREAL[34], DINT[3], BYTE[4] |
| SetKinTransform | `0x20E7` | 1320 | 1328 | 구조체, LREAL 중심 |

## DLL 구현 시 주의점

1. PMAS count 기준 고정 비율 변환을 DLL에 박으면 안 된다.
   - 캡처는 PMAS/MMCLib가 축 reference와 axis info를 먼저 얻고 그 뒤 command를 보낸다는 것을 보여준다.
   - LASAL 이식에서는 `unit.h`에 정의된 application unit scale을 기준으로 변환해야 한다.

2. request와 response parser를 분리해야 한다.
   - request length: offset `[4]`
   - response length: offset `[2]`

3. command별 numeric type을 구분해야 한다.
   - `MoveAbsoluteEx`, `MoveRelativeEx`, `MoveLinearAbsoluteEx`: LREAL
   - `Stop`, `GroupStop`: REAL
   - 상태/enable/buffer/execute 계열: DINT 또는 BYTE 혼재

4. `MoveVelocityEx`는 캡처 기준 예외 처리가 필요하다.
   - header payload length field는 `48`
   - 실제 TCP payload length는 `49`
   - 마지막 execute가 1 byte로 붙어 있고 7-byte padding이 없다.

5. LASAL 이식에서는 PLC가 PMAS 구조를 흉내낼지, PC DLL이 PMAS 구조를 흉내낼지 먼저 정해야 한다.
   - PLC 부하를 줄이려면 PC DLL이 axis interface 단위 정보를 읽고 변환해야 한다.
   - 그래도 wire protocol은 이 캡처처럼 command별 타입과 offset을 유지해야 한다.

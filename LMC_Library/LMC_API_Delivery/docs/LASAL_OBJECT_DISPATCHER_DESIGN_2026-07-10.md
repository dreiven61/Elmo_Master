# LASAL Object Dispatcher Design

작성일: 2026-07-10

최종 갱신: 2026-07-14

> dispatcher의 descriptor/name registry 결정은 유지한다. 실행 경로는
> `Response -> queue -> CyWork -> approved client call`이며
> TCPMotionInterface의 RtWork/RT mailbox는 사용하지 않는다. 상세 task 기준은
> `LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md`를 따른다.

## 결정

`_LMCAxis1`, `_LMCAxis2` 같은 실제 object name은 LASAL 프로젝트가
소유한다. PC DLL은 그 이름을 하드코딩하거나 PLC pointer를 알지 않는다.

호출 흐름은 아래로 고정한다.

```text
caller target name
  -> 0x103C/0x1042
  -> LASAL actual-name registry
  -> opaque UINT16 descriptor
  -> PC object stores descriptor
  -> later command header/payload carries descriptor
  -> LASAL validates descriptor and selects typed client channel
```

## 구현

canonical 대상은 Git에 추적된
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis`다.

`TCPMotionInterface` client channel:

| Slot | 연결 object | descriptor |
|---|---|---:|
| `LMCAxis1` | `_LMCAxis1.Control` | 1 |
| `LMCAxis2` | `_LMCAxis2.Control` | 2 |
| `LMCAxis3` | `_LMCAxis3.Control` | 3 |
| `LMCAxis4` | `_LMCAxis4.Control` | 4 |
| `LMCRobot` | `_LMCRobotBase1.Control` | `0x0100` |

lookup request를 CyWork에서 실제 처리할 때 연결된 typed client의 `pCmd`에
`_GetObjName`을 호출해 이름을 새로 읽는다. 호출 전 256-byte buffer를 0으로
초기화하고 반환 길이가 1..79인지 확인한 뒤에만 비교한다. 주기적인 name
discovery/retry는 실행하지 않으므로 1 ms CyWork에 문자열 polling 부하가 없다.
Axis 1 lookup은 Axis 1 client만 준비되면 처리하며 Axis 2~4 또는 Robot 준비
여부에 의존하지 않는다. `GetGroupMembersInfo`는 요청을 처리할 때만 다섯
client의 현재 연결과 이름을 모두 다시 확인하고 그 요청의
`ObjectRegistryReady`를 결정한다. `_Linker` 전체 검색은 사용하지 않는다.

`0x103C`와 `0x1042`는 request의 NUL-terminated ASCII name을 registry와
비교한다. LASAL 생성 테이블은 runtime object symbol을 `_LMCAXIS1`,
`_LMCROBOTBASE1`처럼 대문자로 저장하므로 `_GetObjName` 결과와 PC 입력은
`_stricmp`로 대소문자를 구분하지 않고 비교한다. PC는 IDE에 표시되는
`_LMCAxis1`, `_LMCRobotBase1` 표기를 그대로 사용할 수 있다. 일치하면
descriptor를 반환하고, unknown name은 header status
`1`, error `-2`로 거부한다. 축 lookup은 해당 축 client의 runtime 연결과
이름 길이 1..79를 개별 확인하고, group lookup도 Robot client만 개별
확인한다.

PC serializer는 빈 이름, NUL/non-ASCII 문자와 79바이트 초과 이름을 전송
전에 거부한다. LASAL lookup 응답은 정확히 6바이트여야 하며 descriptor
`0`은 실패로 처리한다. PLC가 short error를 반환하면 PC API 예외는
header status, payload length, command status, error ID, raw hex를 보존한다.
따라서 `-2`가 registry entry 미준비인지 이름 불일치인지 실제 응답으로
확인할 수 있다.

AxisInfo도 4바이트 short error ACK를 유효한 실패 응답으로 먼저 해석한다.
성공 응답은 계속 정확히 8바이트여야 하지만, short error는 payload shape 오류로
가리지 않고 `Status`와 `ErrorId`를 호출자에게 전달한다.

descriptor는 pointer가 아니다. `pCmd` 주소를 wire에 보내면 project
rebuild/restart 뒤 무효가 되고 주소도 노출되므로 금지한다. 새 RPC 연결마다
lookup을 다시 수행한다.

## 현재 dispatch 범위

source-first 구현은 `Response()` callback이 완성 frame을 depth-8 queue에
publish하고, non-RT `CyWork()`가 command를 분류한 뒤 승인된
client call을 직접 실행·응답하는 구조다. interface RT task와 mailbox는 없다.

현재 실제 client-call 허용 범위는 아래 16개다.

- Axis descriptor 1..4: `0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`,
  `0x209F`, `0x20A0`, `0x20A2`
- Group descriptor `0x0100`: `0x2047`, `0x2048`, `0x2045`, `0x2049`,
  `0x2085`, `0x20A4`, `0x2051`, `0x20E7`

이전 `-5` group gate는 2026-07-14 source에서 제거했다. 다만 group command는
static 4축/identity와 승인된 mode만 허용하며 범위 밖 인자는 `-7`이다.
`GroupReset`은 axis/hardware error reset, `GroupStop` ACK는 command 접수 의미다.

RPC lifecycle, axis/group lookup, AxisInfo, GetGroupMembersInfo처럼 motion client
call이 없는 command도 `CyWork()`에서 처리한다. 이 source 구현은 LASAL IDE
rebuild와 PLC 검증 전이므로 실제 motion 승인 상태가 아니다.

모든 DINT motion field는 caller가 변환한 값을 그대로 `_LMCAxis` 또는
`_LMCRobotBase` method에 전달한다. PLC에서 LREAL 변환이나 UNIT 재적용을
하지 않는다.

## 오류 계약

| 조건 | 처리 |
|---|---|
| 해당 client/name entry 준비 전 또는 unknown name | lookup 실패 `-2` |
| descriptor 0 또는 범위 밖 | invalid reference `-3` |
| unknown command | unsupported command `-4` |
| 승인된 LASAL 대응 method 없음 | unsupported operation `-5` |
| 32-bit axis command error의 상위 16비트 발생 | truncation 방지 generic error `-6` |
| 지원하지 않는 direction/deceleration/execute 조합 | invalid motion arguments `-7` |

legacy `0x2081..0x2084` handler는 제거했다. 정식 DINT command가 아니므로
case default의 `-4`로 거부한다.

## 남은 검증

이 저장소에서는 LASAL IDE build와 PLC 다운로드를 실행할 수 없다.

반드시 LASAL IDE에서 다음을 수행한다.

1. `TCPMotionInterface` class model의 첫 client가 `LMCAxis1`이고 RT task가
   비활성화됐는지 확인한 뒤 CodeGenerator로 재생성한다.
2. 생성 결과가 tracked `.st`의 client descriptor/hash와 일치하는지 본다.
   class table header의 client count도 6이어야 한다.
3. `TCPMotionInterface1.LMCAxis1 -> _LMCAxis1.Control`과 나머지 3축 client
   연결을 확인한다.
4. CyclicTime `1 ms`, RealTime assignment 부재, server `Config=0`,
   `MaxConnections=1`을 적용한다. interface CyWork는 TCP server CyWork와 같은
   cyclic task에 두고 axis RT thread와 같은 CPU core에서 같거나 낮은 priority로
   실행한다.
5. PLC에서 실제 name lookup, descriptor 1~4의 axis 8개 command와 group
   descriptor `0x0100`의 활성 3개 command routing을 재캡처한다.
   mixed-case `_LMCAxis1`/`_LMCRobotBase1`와 runtime canonical uppercase
   `_LMCAXIS1`/`_LMCROBOTBASE1`가 같은 descriptor를 반환하는지도 확인한다.
   Online Debugger에서 `AxisObjectName1..4`, `GroupObjectName`,
   `ObjectRegistryReady`도 함께 확인한다.
6. invalid name/reference가 정해진 오류로 끝나고 다른 축을 호출하지 않는지
   시험한다.
7. unsupported 5개 command가 client를 호출하지 않고 `-5`로 끝나는지 확인한다.

상세 IDE/network 적용 순서와 검증 gate는
`LASAL_SOURCE_QUEUE_AND_NETWORK_APPLY_PLAN_2026-07-10.md`를 따른다.

CodeGenerator header를 수동 수정한 현재 소스만으로 production 완료를
선언하면 안 된다.

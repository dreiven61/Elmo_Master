# LASAL Object Dispatcher Design

작성일: 2026-07-10

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
| `LMCAxis` | `_LMCAxis1.Control` | 1 |
| `LMCAxis2` | `_LMCAxis2.Control` | 2 |
| `LMCAxis3` | `_LMCAxis3.Control` | 3 |
| `LMCAxis4` | `_LMCAxis4.Control` | 4 |
| `LMCRobot` | `_LMCRobotBase1.Control` | `0x0100` |

RT 초기화에서 각 typed client의 `pCmd`에 `_GetObjName`을 호출해 실제
이름을 256-byte 안전 buffer에 먼저 저장한다. wire name 한계에 맞춰 길이가
1..79인 경우에만 registry를 ready로 만든다. TCP request마다 `_Linker`
전체를 검색하지 않는다.

`0x103C`와 `0x1042`는 request의 NUL-terminated ASCII name을 registry와
비교한다. 일치하면 descriptor를 반환하고, unknown name은 header status
`1`, error `-2`로 거부한다.

PC serializer는 빈 이름, NUL/non-ASCII 문자와 79바이트 초과 이름을 전송
전에 거부한다. LASAL lookup 응답은 정확히 6바이트여야 하며 descriptor
`0`은 실패로 처리한다.

descriptor는 pointer가 아니다. `pCmd` 주소를 wire에 보내면 project
rebuild/restart 뒤 무효가 되고 주소도 노출되므로 금지한다. 새 RPC 연결마다
lookup을 다시 수행한다.

## 현재 dispatch 범위

아래 handler와 routing source는 반영했지만 현재 `Response()` callback에서
직접 실행된다. `_LMCAxis` method의 same-core 요구를 만족시키는 command
queue/`RtWork` 실행으로 옮기기 전에는 실제 motion 승인 상태가 아니다.

- Axis: AxisInfo, Power, Reset, Stop, ReadStatus, ReadActualPosition,
  MoveAbsolute, MoveRelative, MoveVelocity
- Group: lookup, GetGroupMembersInfo, Enable, Disable, ReadStatus,
  MoveLinearAbsolute
- GroupReset/GroupStop: 안전하게 대응되는 LASAL method가 확정되지 않아
  error `-5`를 반환한다. false-success나 무응답으로 처리하지 않는다.

모든 DINT motion field는 caller가 변환한 값을 그대로 `_LMCAxis` 또는
`_LMCRobotBase` method에 전달한다. PLC에서 LREAL 변환이나 UNIT 재적용을
하지 않는다.

## 오류 계약

| 조건 | 처리 |
|---|---|
| registry 준비 전/unknown name | lookup 실패 `-2` |
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

1. `TCPMotionInterface` class model에 `LMCAxis2..4` client와 registry 변수를
   등록하고 CodeGenerator로 재생성한다.
2. 생성 결과가 tracked `.st`의 client descriptor/hash와 일치하는지 본다.
   class table header의 client count도 6이어야 한다.
3. `Motion_Network.lcn`의 4개 client 연결을 확인한다.
4. PLC에서 실제 name lookup, descriptor 1~4, 축별 read/motion routing을
   재캡처한다.
5. invalid name/reference가 정해진 오류로 끝나고 다른 축을 움직이지 않는지
   시험한다.

CodeGenerator header를 수동 수정한 현재 소스만으로 production 완료를
선언하면 안 된다.

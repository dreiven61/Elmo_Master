# Axis Home 및 TEST ONLY 엔코더 오류 리셋 구현 설계

작성일: 2026-07-31

> **폐기됨 (2026-08-03):** 이 문서의 switch-search `MMC_Home`,
> `MoveReference()`, DS402 method 35/임의 offset 및 generic `0x7E50` TW20
> 설계는 더 이상 현재 계약이 아니다. 현재 계약은
> `LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md`와
> `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`를 따른다. 이 파일의
> 나머지 내용은 과거 결정 추적용으로만 보존한다. 아래 남아 있는 `MMC_*`
> 표기는 폐기된 과거 alias이며 새 코드, UI, wire 이름 또는 문서에 복사하지 않는다.

## 1. 결론

Home과 Elmo 엔코더 오류 리셋은 서로 다른 세 기능으로 분리한다.

| 기능 | Public 의미 | PLC backend | 현재 활성화 원칙 |
|---|---|---|---|
| `MMC_Home` | LASAL-native reference | `_LMCAxis.MoveReference()` | 물리 reference 입력과 PLC watchdog 검증 전 capability OFF |
| `MMC_HomeDS402` | drive DS402 homing | SDO 설정 + DS402 ControlWord/StatusWord state machine | outcome retire 계약, 공통 축 소유권, PLC bench proof 전 capability OFF |
| `TestEndat22Id30ErrorWarningReset` | EnDat 2.2 ID30 센서 오류/경고 리셋 | `TW[20]`, `0x20FC:0x02`, UInt16, 2-byte SDO Write | 호환 엔코더 확인과 지속 motor-off 소유권 전까지 capability OFF |

`MMC_Home`과 `MMC_HomeDS402`를 같은 구현으로 취급하지 않는다. 또한 Home 시작
응답은 완료가 아니다. 실제 완료는 PLC가 기록한 동일 intent의 terminal outcome으로만
판정한다.

## 2. MMC_Home: LASAL MoveReference

`MMC_Home`은 Admin `0x7D13 StartAxisReference`의 stable alias다. backend 후보는
`_LMCAxis.MoveReference()`이고 DS402 homing method 번호를 받지 않는다.

v1 recipe는 다음 두 개만 유지한다.

| Recipe | 의미 |
|---|---|
| `1` | negative 방향 reference switch 탐색 후 backoff |
| `2` | positive 방향 reference switch 탐색 후 backoff |

현재 Motion Network에서 `HWMin`, `HWMax`, `RefSwitch`, `ZImpulse`, `LatchPos`의 실제
source가 연결되지 않았다. 이 상태에서 native call을 허용하면 입력이 없는 reference motion을
시작할 수 있으므로 capability bit 4는 계속 OFF다.

활성화하려면 다음을 먼저 고정해야 한다.

1. 축 1..4의 실제 reference switch source, active level, debounce와 단선 상태를 정의한다.
2. recipe별 `_LMCAXIS_REFMODE` bit 조합을 축별 bench에서 확인한다.
3. `MaxTravel`과 `TimeoutMs`를 PC wait가 아닌 PLC cyclic watchdog으로 구현한다.
4. timeout/travel 초과 시 PLC가 독립적으로 controlled Stop을 실행한다.
5. accepted/running provenance와 `IsReferenced`, `Standstill`, axis error, position의 terminal
   outcome을 남긴다.

상세 wire와 dormant gate는
`docs/architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md`를 따른다.

### 2.1 현재 source 상태

현재 `LMCControlCommandService`에는 `0x7D13` request parser뿐 아니라
`_LMCAxis.MoveReference()`를 호출하는 LASAL-native reference state machine도 구현되어 있다.
진행 상태와 watchdog/outcome 값은 `ReferenceState[0..18]`에 보존하며,
`ProcessAxisReference()`가 매 scan timeout, 누적 이동거리, 축 오류, reference 완료 및
3-scan stable position을 검사한다. timeout 또는 travel 초과와 terminal failure에서는
`StopMove()`를 한 번 요청한다. DINT 위치 wrap도 이동거리 1 count로 계산하도록 처리한다.

`TCPMotionInterface.CyWork()`는 session close 처리 뒤, 새 request dequeue/parser보다 먼저
`ControlCommands.ProcessAxisReference()`를 호출한다. 따라서 시작을 수락한 뒤 PC 연결이
끊겨도 PLC cyclic watchdog과 terminal 처리는 계속 수행할 수 있는 구조다.

다만 `#define LMC_ADMIN_AXIS_REFERENCE_ENABLED FALSE`이고 Admin capability 값은
`0x00000007`로 bit 4가 OFF다. 따라서 현재 배포 설정에서 정상 형식의 `0x7D13` 요청은
fail-closed로 거부되며 실제 `MoveReference()` motion은 시작하지 않는다. state machine의
존재와 기능 광고/실행 허용을 혼동하면 안 된다.

PC SDK의 `LMCPreparedAxisReference.PersistentIdentity`는 schema, RequestId, axis, recipe와
모든 전송 파라미터를 그대로 보존한다. 다만 `0x7D13` v1 wire에는 ClientIntent,
DiagnosticsBuild/BootId/MapRevision 및 outcome query가 없다. 따라서 SDK는
`HasWireClientIntent=FALSE`, `HasDiagnosticsIdentity=FALSE`,
`SupportsOutcomeQuery=FALSE`를 명시하며 존재하지 않는 wire identity를 만들어 내지 않는다.
WPF durable journal은 MMC Home의 ClientIntent 네 word를 모두 0으로 저장하고, 같은 PLC인지
확인하기 위해 전송 직전에 관측한 Build/Boot/Map을 별도 필드로 보존한다. 이전 format v1의
active MMC Home record는 authoritative prepared identity가 없으므로 자동 변환하거나
덮어쓰지 않고 fail-closed quarantine한다.

`LMCControlCommandService`의 `LMCAxis1..4 : CltChCmd__LMCAxis`와 다음 연결은 이미
존재한다.

| Client | Existing destination |
|---|---|
| `LMCControlCommandService1.LMCAxis1` | `_LMCAxis1.Control` |
| `LMCControlCommandService1.LMCAxis2` | `_LMCAxis2.Control` |
| `LMCControlCommandService1.LMCAxis3` | `_LMCAxis3.Control` |
| `LMCControlCommandService1.LMCAxis4` | `_LMCAxis4.Control` |

그러나 Motion Network의 `RefSwitch`, `ZImpulse`, `LatchPos`, `HWMin`, `HWMax`에는 실제
`Connection`이 없다. 이 입력 source가 확정되기 전에는 Network를 임의 연결하지 않는다.

### 2.2 구현 구조와 활성화 체크리스트

LASAL declaration에는 다음 compact state와 global function이 존재한다.

```text
ReferenceState : ARRAY [0..18] OF DINT
FUNCTION GLOBAL ProcessAxisReference
END_FUNCTION
```

`ReferenceState`는 phase, axis, recipe, request parameter, watchdog 기준값과 terminal
status/error/detail/native state를 한 배열에 보존한다. `HandleAdminCommands()`는 gate가 켜진
경우에만 `MoveReference()`를 시작하고 state를 초기화하며, 이후 진행은
`ProcessAxisReference()`가 담당한다.

`MoveReference()` 인자는 recipe별로 다음 의미를 고정한다.

| Argument | Recipe 1 | Recipe 2 |
|---|---:|---:|
| `Mode.NoZImpulse` | `1` | `1` |
| `Mode.RefDirection` | `0` | `1` |
| `Position` | request `Position` | request `Position` |
| `VRef1` | `SearchVelocity` | `SearchVelocity` |
| `VRef2` | `BackoffVelocity` | `BackoffVelocity` |
| `Accel` | `Acceleration` | `Acceleration` |
| `PositionWindow` | request `PositionWindow` | request `PositionWindow` |
| `RefJerk` | request `Jerk` | request `Jerk` |

이 매핑은 최종 배선 및 극성의 bench proof 전까지 설계값일 뿐이다. `ProcessAxisReference()`는
매 scan 이동거리와 elapsed time을 검사하고 초과 시 `StopMove()`를 호출한다. terminal
success는 `IsReferenced=TRUE`, `Standstill=TRUE`, axis error 없음이 동시에 확인된 경우만
기록한다. 구현과 cyclic 호출은 source에 반영되었지만 physical input 연결과 PLC/실축 proof가
없으므로 capability bit 4와 `LMC_ADMIN_AXIS_REFERENCE_ENABLED`는 계속 OFF로 유지한다.

## 3. MMC_HomeDS402

### 3.1 API 범위

Admin start command는 `0x7D15`, read-only outcome query는 `0x7D16`이다. basic
`MMC_HomeDS402`만 구현하고 `HomeDS402Ex`는 이번 범위에 포함하지 않는다.

입력 의미는 Maestro manual을 그대로 유지한다.

- `Position`: home offset이다. homing 완료 뒤 absolute position은 `-Position`이다.
- `Velocity`: basic API의 단일 속도다. PLC는 `0x6099:01`과 `0x6099:02`에 같은 값을 쓴다.
- `Acceleration`: `0x609A:00`에 쓴다.
- `HomingMethod`: Elmo Gold는 `1..14`, `17..30`, `33..35`만 허용한다. `15`, `16`, `31`, `32`, `36`은 허용하지 않는다.
- `DistanceLimit`: method `-1/-2` 전용인데 v1 method 범위에 없으므로 반드시 `0`이다.
- `TorqueLimit`: v1에서 구현하지 않으므로 반드시 `0`이다.
- `BufferMode`: PLC queue/blending을 구현하지 않으므로 `MC_ABORTING_MODE(1)`만 받는다.
- `TimeoutMs`: PLC state machine의 필수 elapsed-time bound다.

### 3.2 Wire 계약

`0x7D15` request payload는 72 byte다.

| Offset | Type | Field |
|---:|---|---|
| `P+0` | U16 | SchemaVersion = 1 |
| `P+2` | U16 | Flags = 0 |
| `P+4` | U32 | RequestId, nonzero |
| `P+8` | U32 | DiagnosticsBuild |
| `P+12` | U32 | DiagnosticsBootId |
| `P+16` | U32 | MapRevision |
| `P+20..32` | 4 x U32 | 128-bit ClientIntentId |
| `P+36` | I32 | HomingMethod |
| `P+40` | I32 | Position/home offset |
| `P+44` | I32 | Velocity |
| `P+48` | I32 | Acceleration |
| `P+52` | I32 | DistanceLimit = 0 |
| `P+56` | I32 | TorqueLimit = 0 |
| `P+60` | U16 | BufferMode = 1 |
| `P+62` | U16 | Reserved = 0 |
| `P+64` | U32 | TimeoutMs, nonzero |
| `P+68` | U32 | ExecuteToken = `0x32303448` |

start response payload는 24 byte다. 이는 수락 또는 시작 거부만 나타내며 Home 완료 증거가
아니다.

`0x7D16` request payload는 44 byte다.

| Offset | Type | Field |
|---:|---|---|
| `P+0` | U16 | SchemaVersion = 1 |
| `P+2` | U16 | Flags = 0 |
| `P+4` | U32 | Query RequestId, nonzero |
| `P+8` | U32 | DiagnosticsBuild |
| `P+12` | U32 | DiagnosticsBootId |
| `P+16` | U32 | MapRevision |
| `P+20` | U32 | Original start RequestId |
| `P+24..36` | 4 x U32 | Original 128-bit ClientIntentId |
| `P+40` | I32 | Original HomingMethod |

header `Reference`도 원래 physical axis `1..4`와 같아야 한다. success response payload는
92 byte다.

| Offset | Type | Field |
|---:|---|---|
| `P+0..12` | common 16 | Schema, status/error, query RequestId, detail |
| `P+16` | U16 | State: 1 Running, 2 Succeeded, 3 Failed, 4 Aborted |
| `P+18` | U16 | Reserved = 0 |
| `P+20` | U32 | DiagnosticsBuild |
| `P+24` | U32 | DiagnosticsBootId |
| `P+28` | U32 | MapRevision |
| `P+32` | U32 | Original start RequestId |
| `P+36..48` | 4 x U32 | Original ClientIntentId |
| `P+52` | U16 | Physical axis |
| `P+54` | U16 | Reserved = 0 |
| `P+56` | I32 | HomingMethod |
| `P+60` | U16 | Original command status |
| `P+62` | I16 | Original ErrorId |
| `P+64` | U32 | Original Detail |
| `P+68` | U16 | Last StatusWord |
| `P+70` | U16 | Reserved = 0 |
| `P+72` | I32 | Last actual position |
| `P+76` | U32 | StartCycle |
| `P+80` | U32 | CompletionCycle |
| `P+84` | U32 | NativeState |
| `P+88` | U32 | Generation |

query failure detail은 `25 NotFound`, `26 Indeterminate`, `27 StoreCorrupt`,
`28 KeyMismatch`, `29 StorageUnavailable`을 사용한다. stored terminal failure detail은
`30 Ds402HomeExecutionFailed`, `31 Ds402HomeAborted`를 사용한다.

`0x7D16`은 원래 request identity와 intent를 exact-match하는 read-only query다. query는
Home을 다시 실행하지 않는다. record는 현재 PLC boot 안에서만 유효하고 reconnect 뒤에도
동일 boot/identity라면 조회할 수 있지만 자동 replay하지 않는다. boot/build/map 또는 어느
identity field라도 다르면 `KeyMismatch`로 fail-closed한다.

### 3.3 PLC state machine

drive별 SDO executor는 한 번에 하나의 operation만 소유한다. 다음 순서를 건너뛰거나
병렬 실행하지 않는다.

1. exact diagnostics identity, one-operation ownership, physical axis 1..4, fresh coherent
   EtherCAT snapshot을 검증한다.
2. master/drive OP, online, AL status 0, axis error 없음,
   `(StatusWord AND 0x006F)=0x0027`, target reached와 3개 actual-position stable sample을
   확인한다. active speed PDO가 없으므로 한 sample만으로 Standstill을 추정하지 않는다.
3. 현재 boot의 per-axis outcome slot에 full identity/intent를 먼저 Running으로 기록하고
   readback한다.
4. `0x6061:00` signed8을 읽고 현재 mode가 `8`인지 확인한다.
5. `0x6098:00` signed8 homing method를 쓴다.
6. `0x607C:00` signed32 home offset을 쓴다.
7. `0x6099:01`과 `0x6099:02` uint32 velocity를 순차로 쓴다.
8. `0x609A:00` uint32 acceleration을 쓴다.
9. RT owner mailbox를 통해 ControlWord bit 4를 clear/hold한다.
10. `0x6060:00` signed8 값을 `6`으로 쓰고 `0x6061:00`을 읽어 `6`인지 확인한다.
11. fresh StatusWord에서 bit 12(homing attained)와 bit 13(homing error)가 모두 clear인
    pre-start sample을 확인한다. 이전 Home의 bit 12가 남아 있으면 시작하지 않는다.
12. RT owner mailbox를 통해 ControlWord bit 4를 set/hold한다.
13. bit 4 상승 이후 fresh StatusWord bit 12의 새로운 `0 -> 1` edge를 관찰해야만 attained로
    인정한다. bit 13, bit 10(target reached)와 actual position도 cyclic으로 감시한다.
14. 성공 또는 실패 시 bit 4를 RT owner에서 clear한다.
15. CSP 복귀 전에 mailbox의 raw drive actual position이 현재 `DriveN.ActPos`와 같은지 다시
    확인하고, `_LMCAxis.ReadPosition()`으로 `ACTPOS_APPUNIT`와 `ACTPOS_INTUNIT`를 캡처한다.
    raw drive DINT를 application unit 값으로 간주해 `SetPosition()`에 직접 넣지 않는다.
16. 같은 RT scan에서
    `_LMCAxis.SetPosition(Mode:=LMCAXIS_SET_SETPOS_APPUNIT_DEST,
    Position:=captured ACTPOS_APPUNIT)`를 정확히 한 번 호출한다. `_LMCAXIS_CMDERROR`에서는
    `PowerOff`와 `NoReference` 두 bit만 임시로 허용하고 나머지 bit가 하나라도 있으면 실패한다.
17. `SetPosition()`을 호출한 scan에서는 mailbox `AppliedSequence`를 publish하지 않는다. 다음
    RT scan에서 `SETPOS_APPUNIT`, `ACTPOS_APPUNIT`, `SETPOS_INTUNIT`, `ACTPOS_INTUNIT`,
    `DESTPOS_INTUNIT`, `MASTERPOS_INTUNIT`를 다시 읽고 저장한 application/internal 값과 모두
    exact-match할 때만 alignment 성공을 publish한다.
18. `0x6060:00=8`을 쓰고 `0x6061:00=8`을 확인한다.
19. exact terminal outcome을 storage에 commit/readback한 다음 RPC에서 조회 가능하게
    만든다.

`0x6040`을 SDO로 쓰지 않는다. 이 프로젝트의 ControlWord는 cyclic PDO이고
`ECAT_DS402Base`가 bits 0..3/7을 관리한다. background TCP task가 전체 ControlWord를 직접
read-modify-write하는 것도 금지한다. bit 4 변경과 setpoint alignment는 axis realtime task와
같은 core에서 실행되는 mailbox consumer를 통한다.

timeout, SDO abort, homing-error bit, connection loss, identity change 또는 CSP restore 실패는
성공으로 추정하지 않는다. 가능한 경우 bit 4 clear와 controlled PowerOff를 수행하고 terminal
failure/indeterminate record를 남긴다.

### 3.4 현재 source 구현과 비활성화 상태

현재 source에는 다음 구현이 들어가 있다.

- `TCPMotionInterface.MsgPaser()`가 `0x7D15`, `0x7D16`을 diagnostics route로 보낸다.
- `LMCDiagnosticsService.HandleRequest()`가 두 request를 각각
  `HandleAxisDs402HomeStart()`와 `HandleAxisDs402HomeOutcome()`으로 위임한다.
- `Ds402HomeState[0..127]`가 축별 durable outcome과 실행 중 SDO/mailbox/cleanup state를
  보존한다.
- `ProcessAxisDs402Home()`가 SDO 설정, ControlWord bit 4 edge, StatusWord bit 12/13,
  CSP 복귀와 terminal commit을 비동기 cyclic state machine으로 처리한다.
- `LMCEcatInputLatch.RtWork()`의 command 5가 LMC actual position capture, mode 10
  `SetPosition()`과 다음 scan의 application/internal set/actual/destination/master readback을
  수행한다.
- generic D5 SDO와 DS402 Home은 동일 executor를 동시에 소유하지 못하도록 상호 배제한다.
- session close는 완료를 추정하거나 자동 replay하지 않으며, 동일 boot/identity의 read-only
  outcome query만 허용한다.

그러나 `#define LMC_DIAG_DS402_HOME_ENABLED FALSE`이고 diagnostics capability 값
`0x0000613F`의 bit 6은 OFF다. 즉 구현은 존재하지만 현재 PLC build에서는 `0x7D15` 실행을
허용하거나 광고하지 않는다. PLC/실축 bench proof 없이 gate를 켜면 안 된다.

### 3.5 realtime mailbox 및 Network 상태

`LMCEcatInputLatch`에는 다음 구조가 구현되어 있다.

- `Ds402HomeMailbox[0..11]` 12-slot mailbox
- `Ds402HomeAlignmentState[0..7]` two-scan alignment state
- request/applied sequence, owner token, physical axis, command, applied result readback
- `SubmitDs402HomeControl()`, `GetDs402HomeControlState()`,
  `SubmitDs402HomeSetpointAlignment()`
- realtime `RtWork()`에서만 ControlWord bit 4를 유지/변경하는 single-owner 처리

control command 값은 `1 AcquireAndHoldLow`, `2 SetHigh`, `3 SetLow`,
`4 ReleaseAfterLow`, `5 SetpointAlignment`다. command 5는 owner token, physical axis,
ControlWord bit 4 low readback, drive/axis client 연결과 raw `DriveN.ActPos` 일치를 먼저
검사한다. 그 뒤 LMC application/internal actual position을 캡처하고
`LMCAXIS_SET_SETPOS_APPUNIT_DEST`로 set/destination을 맞춘다. 같은 scan에는 완료를 publish하지
않으며 다음 RT scan의 여섯 position readback이 모두 exact-match한 경우에만 `Result=0`을
publish한다. axis client disconnect는 `Result=-3`, raw position/identity/position readback 또는
허용되지 않은 command-state bit 불일치는 `Result=-5 ReadbackMismatch`로 fail-closed한다.

command 5는 다음 상태를 보존한다.

| `Ds402HomeAlignmentState` index | 의미 |
|---:|---|
| `0` | pending, `0/1` |
| `1` | owner operation token |
| `2` | physical axis `1..4` |
| `3` | 요청 시 raw `DriveN.ActPos` |
| `4` | 캡처한 `LMCAXIS_ACTPOS_APPUNIT` |
| `5` | 캡처한 `LMCAXIS_ACTPOS_INTUNIT` |
| `6` | raw `_LMCAXIS_CMDERROR` |
| `7` | reserved, `0` |

`LMCEcatInputLatch`의 required client와 Motion Network 연결은 다음과 같다. 기존
`_LMCRobotBase1.LMCAxisN -> _LMCAxisN.Control` 연결도 그대로 유지한다.

| Client | Destination |
|---|---|
| `LMCEcatInputLatch1.LMCAxis1` | `_LMCAxis1.Control` |
| `LMCEcatInputLatch1.LMCAxis2` | `_LMCAxis2.Control` |
| `LMCEcatInputLatch1.LMCAxis3` | `_LMCAxis3.Control` |
| `LMCEcatInputLatch1.LMCAxis4` | `_LMCAxis4.Control` |

기존 Comm/SDO/Drive 연결은 유지한다. Diagnostics에 `Elmo_1` direct client를 추가하지 않으며,
bit 4 변경과 alignment는 `LMCEcatInputLatch.RtWork()`의 owner token과 request/applied sequence
readback을 통해서만 수행한다.

이 source 구현은 실제 axis의 no-jump 결과나 LASAL `AxisStatus.IsReferenced` 전환을 증명하지
않는다. PLC bench에서 same-core/equal-or-lower-priority 호출 조건, task 실행 순서, bit 4가 이후
realtime 로직에 의해 덮어써지지 않는지, CSP 복귀 target jump와 `IsReferenced` 정책을 확인해야
한다. 이 proof 전까지 `LMC_DIAG_DS402_HOME_ENABLED`와 diagnostics capability bit 6은 OFF다.

또한 현재 server에는 축별 cross-command interlock이 없다. 즉 MMC Home 또는 DS402 Home이
진행 중인 같은 축으로 ordinary Motion/Power command가 들어왔을 때 이를 공통 owner 상태로
거부하는 계약이 아직 구현되지 않았다. Home active 상태를 축별로 공유하고 ordinary
Motion/Power 진입점에서 fail-closed로 거부하는 interlock을 구현하고 동시 요청을 bench에서
검증하기 전에는 MMC/DS402 Home gate와 capability를 켜면 안 된다.

### 3.6 현재 LASAL IDE build 상태

command 5가 fail-closed였던 이전 단계의 LASAL IDE build 이력은 다음과 같다.

1. 첫 build는 `5 errors / 35 warnings`였다. 오류는 불필요하게 required로 선언된
   `LMCEcatInputLatch.LMCAxis1..4` client의 unconnected 4건과 table 오류 1건이었다.
2. 당시 command 5가 `-5`로 fail-closed였으므로 네 client 선언과 Network 연결 시도를
   LASAL IDE에서 제거했다.
3. 그 뒤 incremental build는 `0 errors / 35 warnings`로 완료됐다.
4. full rebuild는 `0 errors / 55 warnings`로 끝났지만 CodeGenerator가 새로 복원한 custom
   implementation body를 이전 stub으로 덮어썼다. 따라서 이 full rebuild 결과는 현재 기능의
   정상 build 증거로 사용하지 않는다.
5. implementation body를 다시 복원한 뒤 실행한 당시 최종 incremental F9 build는
   `0 errors / 26 warnings`로 완료됐다. 이 결과를 현재 복원 source가 LASAL compiler/linker를
   통과한 당시 source/build 증거로 사용했다.

후속 작업에서는 command 5를 two-scan alignment로 구현하면서
`Ds402HomeAlignmentState[0..7]`, `LMCAxis1..4` required client와 네 개의 정확한 Motion Network
연결을 다시 추가했다. 2026-07-31 현재 LASAL IDE save에서 배열 범위 `[0..7]`, 네 client와
`_LMCAxis1..4.Control` 연결을 확인했고, 후속 incremental F9 build는
`0 errors / 25 warnings`로 완료됐다. F9 전후 `LMCEcatInputLatch.st` SHA-256은
`1C102229B5302333227E01CFB66B01CA66F9C4B89CB6F5AAA719A5A3A9B88D23`으로 같아 custom
implementation이 다시 덮어써지지 않았음을 확인했다.

재생성된 `ONE_Motion_Network_Table.st`에는 축 1..4마다 기존 robot owner와 latch owner가
동일한 `_LMCAxisN.Control` target을 공유하는 두 internal-channel entry가 존재한다.
`Verify-LasalContract.ps1 -ExpectedSdoWriteAxis 1`의 SourceOnly와 full static contract도 모두
PASS했다. 후속 `Find in Implementation` smoke도 시작 시각 `22:55:00.535` 기준 PASS했다.
`LMCAxis1` 38 hits, `InputLatch` 45 hits, `Drive1` 19 hits, `ControlCommands` 5 hits였고
각각 1 matched file / 2 searched였다. 시작 이후 로그 10줄에 새 `CInvalidArgException`은
없었다. 따라서 위의 과거 `0 errors / 26 warnings`와 과거 implementation-index smoke를 현재
후속 변경의 증거로 재사용하지 않는다.

이전 fail-closed source 복구 후 IDE implementation index도 별도로 확인했다. 실제 Network channel에서
`Find in Implementation`을 실행해 아래 네 class의 구현 위치로 모두 정상 이동했다.

- `LMCControlCommandService`: `LMCAxis1`
- `LMCDiagnosticsService`: `InputLatch`
- `LMCEcatInputLatch`: `Drive1`
- `TCPMotionInterface`: `ControlCommands`

스모크 시작 시각 `21:01:13` 이후 `%TEMP%\Lasal2.log`에는 네 건의
`Searching implementation`과 각각의 `Last command succeeded`가 기록됐고 새
`CInvalidArgException`은 0건이었다.

이 결과에는 PLC download와 실제 축 실행이 포함되지 않았다. 따라서 기능 gate와 capability
bit 4/6/18/19는 계속 OFF이며, Home 또는 TW19/TW20의 실장 동작이 검증됐다는 의미가 아니다.

### 3.7 terminal outcome 보존과 retire 계약

현재 `Ds402HomeState`는 축별 23 DINT record 네 개와 공용 실행 state 하나를 가진다. 실행 중에는
공용 state가 새 Home을 막지만, terminal 뒤에는 공용 state가 idle로 돌아간다. 이때 같은 축의
다음 `0x7D15`가 기존 축 record를 초기화하므로 아직 `0x7D16`으로 회수하지 않은 terminal
identity와 결과가 덮어써질 수 있다. WPF의 로컬 journal 차단만으로는 raw SDK client나 다른
client의 요청을 막을 수 없으므로 PLC protocol 자체에서 보호해야 한다.

최소 안전 계약은 다음과 같다.

- `0x7D16`은 side effect 없는 read-only query로 유지한다.
- exact identity와 `RecordGeneration`을 받는 idempotent
  `0x7D17 RetireAxisDs402HomeOutcome`을 추가한다.
- terminal record는 retire 전까지 새 같은 축 Home을 거부한다.
- retire 응답 손실 뒤에도 재조회와 retire 재시도가 가능하도록 record는 tombstone으로 남긴다.
- WPF journal은 `0x7D16 terminal 확인 -> 0x7D17 retire 확인 -> Resolve` 순서로만 해제한다.

이 변경에는 `LMCDiagnosticsService` 내부 handler 1개와 `TCPMotionInterface`의 command route,
C# sync/async API, packet map, verifier와 회귀 테스트가 필요하다. 새 LASAL client/server/channel,
Network 연결 또는 `Ds402HomeState` 크기 변경은 필요 없다. 이 계약과 축별 cross-command
interlock이 구현·검증되기 전에는 `LMC_DIAG_DS402_HOME_ENABLED`와 capability bit 6을 켜지
않는다.

### 3.8 2026-08-03 terminal retire 구현 체크포인트

현재 canonical LASAL source에는 `0x7D15/0x7D16/0x7D17` triad가 모두 구현·라우팅돼 있다.
`0x7D15`는 같은 축의 unretired terminal record를 detail 32로 거부하고, exact retired
tombstone만 다음 start가 교체할 수 있다. `0x7D16`은 base state `2/3/4`와 내부 tombstone
`0x8002/0x8003/0x8004`를 동일한 masked terminal snapshot으로 읽되 상태를 변경하지 않는다.
`0x7D17`은 exact identity와 nonzero `RecordGeneration`을 요구하고, 최초 성공에서만
tombstone marker를 기록하며 응답 손실 뒤 exact retry에는 같은 92-byte snapshot을 돌려준다.

WPF recovery 순서는 `0x7D16 terminal query -> 0x7D17 retire -> journal Resolve`로 고정했다.
Resolve 전에는 recovery key와 Home parameter 전체, record state, 원본 status/error/detail,
DS402 StatusWord, actual position, start/completion cycle, native state, generation이 두 응답에서
모두 정확히 같아야 한다. 하나라도 다르면 `InvalidDataException`으로 fail-closed하고 journal을
보존한다. retire 응답 유실 뒤 새 `MainWindow`와 새 connection으로 tombstone을 다시 query하고
retire를 재시도해 Resolve하는 경로도 Home replay 없이 검증했다.

현재 검증 결과는 다음과 같다.

- LASAL F9 build: `0 errors / 24 warnings`. 경고는 dormant constant condition과 기존
  C78 project/C81 library·compiler mismatch이며 PLC download는 수행하지 않았다.
- LASAL SourceOnly contract: PASS. 0x7D17 음성 fixture `19/19` 거부 PASS.
- C# Release PC tests: `1077/1077` PASS.
- WPF focused DS402 Home tests: `9/9` PASS. 전체 Release smoke: `326/326` PASS.
- 이 exact checkpoint의 `LMCDiagnosticsService.InputLatch` `Find in Implementation`은
  `09:39:07`에 실행되어 45 hits, 1 matched file / 2 searched files로 완료됐고
  `Last command succeeded (219.3 ms)`가 기록됐다. 시작 이후 신규
  `CInvalidArgException`은 0건이다. 큰 구현 블록 전체가 출력돼 결과가 길지만 검색 실패는 아니다.

따라서 이것은 source/static/PC/IDE build 증거다. PLC download, 실제 축 실행, pcap 기반
`0x7D15 -> 0x7D16 -> 0x7D17` 증거는 없다. `LMC_DIAG_DS402_HOME_ENABLED=FALSE`, Admin
feature bit 6 OFF, `ErrorCatalogVersion=1`을 유지한다. triad의 live proof와 축별 cross-command
ownership interlock, `ErrorCatalogVersion>=4`가 모두 확인되기 전에는 활성화하지 않는다.

### 3.9 2026-08-03 common ownership 설계 checkpoint

축별 cross-command interlock의 authoritative dormant 계약은
`AXIS_COMMON_OWNERSHIP_INTERLOCK_DORMANT_CONTRACT_2026-08-03.md`로 분리했다. 현재 source를
추적한 결과 `TCPMotionInterface`가 ordinary/Admin/Group과 Diagnostics 요청을 모두 직렬화하는
유일한 공통 dispatch 지점이다. 그러나 Power/Reset/Stop/Move handler는 native 반환을 ACK로
바꾼 뒤 retained owner를 보존하지 않으므로 ACK 시점의 단순 busy flag로는 양방향 충돌을
막을 수 없다.

새 계약은 같은 축 Home/TW20과 ordinary mutation을 양방향으로 배제하고, Stop/PowerOff는
거부하지 않고 safety preemption으로 처리한다. Group mutation은 member 전체를 원자 획득해야
한다. `LMCRobot`에는 Axis1..9가 연결되고 `0x20D2`도 9축을 게시하지만 Profile Lock/Move는
Axis1..4만 선택한다. 따라서 robot-wide mask `0x01FF`와 profile mask `0x000F`를 command별로
구분해야 한다. owner declaration과 final-prewire admission token을 LASAL IDE에서 추가하고
source/static/IDE/PLC 검증이 끝나기 전까지 bit 4/6/18/19는 계속 OFF다.

또한 MMC Home `ReferenceState`, DS402 Home active stage, Diagnostics SDO executor는 각각
capacity-1 공용 resource다. 현재 구조에서는 축별 owner만으로 Axis1/Axis2의 동시 Home/TW20을
허용하지 않고 engine별 global lease도 고정 순서로 함께 획득한다. Move terminal은 dispatch
직후의 기존 Standstill을 완료로 오인하지 않도록 post-acquire activity를 먼저 관찰해야 한다.
Stop/PowerOff는 Home cleanup 또는 SDO drain이 끝날 때까지 지연하지 않고 즉시 한 번 dispatch한
뒤 cleanup과 safe-state 확인을 병행한다.

### 3.10 2026-08-03 10:02 LASAL save 이후 current-source blocker

3.8의 `0 errors / 24 warnings`와 SourceOnly PASS는 09:08 source checkpoint 증거다. 이후
10:02:20에 production LASAL project가 다시 저장되면서 현재
`LMCEcatInputLatch.st`의 `SubmitDs402HomeControl`, `GetDs402HomeControlState`,
`SubmitDs402HomeSetpointAlignment` implementation body가 빈 stub으로 바뀌었다. 현재 파일은
919 lines, SHA256
`3FE903D60D3B06CB8E2BDE6158528AC1E96F5ED90978A54D437BCDD64B08042D`다.

10:02 build log도 세 함수에서 parameter unused와 `Result` 미정의 `W0073/W0076`을 기록했다.
현재 SourceOnly는 `LMCEcatInputLatch Drive1 owner-token mailbox path` assertion에서 실패한다.
따라서 09:08 PASS를 current source PASS라고 기록하면 안 된다.

read-only recovery audit에서 pre-save candidate Git blob
`39e25c3e241a7a20eeda727dd972331d8ab6efb9`를 찾았다. 이 blob은 49,053 bytes, 1,457 lines,
SHA256 `6F62B2104CC0FA260494421307C7D4B5A613EDF72101C536646022A98F56F20D`이며 loose-object
mtime은 09:33:45다. 현재 file과 declaration은 normalized exact-match하고, RT owner-token
mailbox, ControlWord bit 4 write/readback, command 5 SetPosition/alignment와 세 public method의
실제 body를 포함한다. 다만 현재 열려 있을 수 있는 LASAL IDE가 다시 덮어쓸 수 있으므로 사용자
확인 전에는 working file에 복원하지 않았다.

복원 뒤 SourceOnly, LASAL F9, 변경 class `Find in Implementation`, smoke 시작 이후 신규
`CInvalidArgException=0`을 다시 확인해야 한다. 이 재검증 전에는 3.8을 current activation
근거로 사용하지 않는다.

## 4. TEST ONLY Elmo 엔코더 유지보수

> 2026-08-04 correction: feedback socket을 write value로 쓰는 계약과 gate-OFF manifest 방식은
> 폐기했다. 현재 exact contract는 [TW19/TW20 fixed-one activation](./LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md)이다.

Elmo NetHelp의 의미를 그대로 구분한다.

| 명령 | 전용 object | 의미 | 현재 상태 |
|---|---|---|---|
| `TW[19]` | `0x20FC:0x01`, UInt16 | Panasonic, Tamagawa 또는 EnDat 2.2의 serial absolute multi-turn position 초기화 | 전용 TEST ONLY source 활성, PLC 실기 미검증 |
| `TW[20]` | `0x20FC:0x02`, UInt16 | EnDat 2.2 ID30 센서 오류/경고 리셋 | 전용 TEST ONLY source 활성, PLC 실기 미검증 |

따라서 `TW[20]`을 일반 "멀티턴 위치 리셋"이라고 표시하지 않는다. `TW[19]`와 `TW[20]`은
서로 다른 전용 action이다. 선택한 drive가 해당 exact command를 지원하는지 실기 전에 확인한다.
두 명령 모두 vendor 문서상 motor off가 필수다.

전용 action만 아래 write를 생성할 수 있다.

| Field | 고정값/범위 |
|---|---|
| Object | TW19 `0x20FC:0x01`, TW20 `0x20FC:0x02` |
| Type/length | UInt16 / 2 bytes |
| Axis/slave | physical `1..4` |
| Write value | UInt16 `1` |
| `TimeoutMilliseconds` | `1..60000` overall service budget |
| Transport | dedicated `0x7E53` Start, `0x7E54` Outcome, `0x7E55` Retire |

generic SDO editor allowlist와 분리한다. 범용 `TW[]` LONG alias `0x3204:0x13/0x14`는
이 public contract에서 사용하지 않는다. `0x20FC` 전용 object와 alias를 자동 대체하거나
fallback·이중 전송하지 않는다. Diagnostics capability bit 18/19는 각각 TW20/TW19 gate를
따르며 2026-08-04 current source에서는 모두 ON이다.

전송 직전 PLC가 다음을 다시 검사한다.

- fresh coherent EtherCAT snapshot
- master와 대상 drive가 OP
- online이고 AL status가 0
- ControlWord EnableOperation bit가 0
- StatusWord OperationEnabled bit가 0

encoder fault를 지우기 위한 action이므로 기존 `AxError` 또는 DS402 Fault가 있다는 이유만으로
차단하지 않는다. 다만 SDO terminal success는 object write 완료만 뜻한다. 이후 DS402 Fault
Reset을 별도로 실행하고 `StateWord.Fault`, LASAL `AxError`, `0x603F` drive error, 실제 위치와
Home 필요 여부를 다시 확인해야 한다.

WPF에서는 `TEST ONLY` 경고, 축 표시, exact `0x20FC` command 지원 확인,
명시적 2단계 확인과 durable no-replay journal을 요구한다. response loss 뒤에는 자동
재전송하지 않는다.

### 4.1 현재 PLC source 상태

`LMCDiagnosticsService`와 PC SDK/WPF에는 TW20/TW19 전용 parser, retained outcome 및 실행
경로가 구현되어 있고 current source에서 활성화됐다.

- `LMC_DIAG_ENCODER_TW20_ENABLED`와 `LMC_DIAG_ENCODER_TW19_ENABLED`는 모두 `TRUE`다.
  축별 profile/socket/evidence manifest는 제거했고 capability bit 18/19를 광고한다.
- object는 exact `0x20FC:0x02/0x01`, UInt16, length `2`만 허용한다.
- drive reference는 `1..4`, write value는 exact `1`만 허용한다.
- `0x3204:0x13/0x14`와 generic `0x7E50` bypass는 허용하지 않는다.
- 전송 직전 fresh snapshot과 motor-off gate를 다시 검사한다.
- 전용 lifecycle/drain flag를 사용해 generic SDO와 구분한다.

공통 축 소유권의 reserve/validate/commit/publish와 executor drain 순서는 source에 구현됐다.
그러나 current source의 clean LASAL build/download와 실제 drive의 write/fault 또는 position
변화는 아직 검증되지 않았다. Source activation은 실기 효과 증거가 아니다.

## 5. 현재 검증 경계

현재 PC 검증 결과는 다음과 같다.

| 대상 | Debug | Release |
|---|---:|---:|
| SDK build + protocol/static tests | `1066/1066 PASS` | `1066/1066 PASS` |
| WPF build | PASS | PASS |
| WPF focused tests | `8 + 5 + 1 + 1 + 1 PASS` | `8 + 5 + 1 + 1 + 1 PASS` |
| WPF maintenance journal/UI regression | `12/12 PASS` | `12/12 PASS` |

WPF focused 범위는 localization 8건, maintenance UI 5건, axis qualification 1건,
local SDO draft editing 1건, exact second-click write confirmation 1건이다. 이 결과는 PC frame,
parser, UI gate와 no-replay 동작을 검증하지만 PLC/drive 동작 증거는 아니다.
maintenance 12건에는 당시 MMC/DS402/TW20의 confirmed rejection이 durable recovery record를
정상 해제하는 journal 회귀 검증이 포함된다. transport uncertainty와 일반 예외는 계속
fail-closed quarantine으로 남긴다.

PC SDK/WPF build와 protocol/static test로 확인한 범위는 다음과 같다.

- exact frame bytes와 strict parser
- capability/identity fail-closed 및 zero-wire rejection
- prepared command one-shot/no-replay
- start ACK와 terminal outcome의 분리
- 당시 TEST ONLY TW20 allowlist와 capability-OFF gate; 현재 TW19/TW20 계약은 4장의 0x20FC 기준을 따른다.

당시 LASAL source에는 MMC Home, DS402 Home, command 5 two-scan alignment와 TW20 실행 경로가
구현되어 있었다. 당시 incremental F9는 `0 errors / 25 warnings`, SourceOnly와 full static
contract는 PASS했고 latch source hash도 보존됐다. 새 implementation-index smoke도 PASS했으며,
기능 gate와 capability bit 4/6/18은 모두 OFF였다. 현재 TW19 bit 19도 OFF이며, 이 문단의 F9는
2026-08-03 변경 뒤 build 증거로 재사용하지 않는다. 3.6의 이전
`0 errors / 26 warnings`와 stale stub을 만든 full rebuild 결과는 현재 기능 build 증거로
재사용하지 않는다. PLC download와 실제 축 실행은 수행하지 않았다.

다음은 LASAL IDE와 실제 PLC/drive에서 별도 확인해야 한다.

- current PLC download
- task/core/priority와 mailbox 실행 순서
- MMC/DS402 Home active 축으로 ordinary Motion/Power command가 들어오면 서버에서 거부하는
  per-axis cross-command interlock 구현과 동시 요청 검증
- 이후 realtime 로직이 ControlWord bit 4를 덮어쓰지 않는지 PDO readback 확인
- command 5 첫 scan의 `SetPosition()`과 다음 scan의 여섯 position exact readback
- `LMCEcatInputLatch.LMCAxis1..4`가 정확한 `_LMCAxis1..4.Control`에 연결됐는지 online 확인
- SDO `0x6060/0x6061` mode 전환
- ControlWord bit 4 edge와 StatusWord bit 12/13
- CSP 복귀 시 target jump 없음
- DS402 Home 뒤 LASAL `AxisStatus.IsReferenced` 상태와 후속 motion 허용 정책
- 축별 MMC Home physical switch 동작
- TW20 대상 EnDat 2.2 ID30 호환성과 write 뒤 실제 encoder fault 및 `0x603F` 변화

따라서 source/static PASS만으로 Home 또는 엔코더 오류 리셋의 실장 동작 완료라고 기록하지 않는다.

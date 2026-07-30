# Group API 구현 상태

작성일: 2026-07-14

최종 갱신: 2026-07-23 (`0x7D22 GroupMoveLinearRelative` source/static)

대상:

- PC: `LMC_Library/LMC_API_Delivery/src/LmcGroup.cs`
- protocol: `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`
- PLC: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`

## 결론

현재 공개 `LMCGroupAxis` API에 group power와 profile enable 역할을 분리했다.
`GroupPowerOn/Off`는 SIGMATEK robot power state machine을 시작하고,
`GroupEnable/Disable`은 이미 구성된 static 4축 profile을 lock/unlock한다.

`0x204A`, `0x204B`는 Maestro packet capture에서 가져온 ID가 아니라 이
LASAL-DINT adapter가 추가한 project-local extension이다.

| ID | 공개 API | LASAL 동작 |
|---:|---|---|
| `0x204A` | `GroupPowerOn` | `LMCRobot.RobotOn(Mode:=_ACTIVE)` |
| `0x204B` | `GroupPowerOff` | `LMCRobot.RobotOff()` |
| `0x2047` | `GroupEnable` | configured X/Y/Z/U profile을 `LockProfile` |
| `0x2048` | `GroupDisable` | `UnlockProfile` |
| `0x2049` | `GroupReset` | `LMCRobot.AxQuitError(AxisNo:=0)` |
| `0x2085` | `GroupStop` | `LMCRobot.StopMove(Mode:=3, Decel, Jerk)` |
| `0x20A4` | `MoveLinearAbsoluteEx` | 승인된 static 4축 인자를 `LMCRobot.MoveLinearCoord`에 전달 |
| `0x7D22` | `MoveLinearRelativeEx` | Admin capability 뒤 승인된 4축 distance를 `LMCRobot.MoveRelativeCoord`에 전달 |
| `0x2051` | `GroupReadActualPosition` | static axis-order identity 위치를 읽어 68-byte DINT payload 반환 |
| `0x20E7` | `SetKinTransformCartesian4Axis` | exact X/Y/Z/U identity 요청을 검증하고 static mapping을 등록 |

이 상태는 source 구현 완료다. 2026-07-22 Phase 0 기준 C# 105/105와 LASAL
source-only/full-network static
contract와 WPF VS2019 Debug build는 통과했다. 2026-07-23 Phase 2 통합 기준은 C#
Debug/Release 148/148, LASAL SourceOnly/full static과 WPF Debug/Release build/startup
smoke를 통과했다. 현재 LASAL source는 IDE에서
Rebuild/Link하거나 PLC에 download하지 않았다. CPU core/priority와 실제 장비
동작도 검증하지 않았다.

## 정상 호출 순서

1. `GroupPowerOn`
2. `GroupReadStatusResult.IsPowerOn`이 `true`인지 확인. 이 값은 LASAL adapter
   전용 `0x00040000` Power Ready 비트다.
3. `SetKinTransformCartesian4Axis`
4. `GroupEnable`
5. `GroupReadStatusResult.IsStandby`가 `true`인지 확인
6. group motion
7. Group Stop과 in-position을 확인한 뒤 `GroupDisable`
8. `GroupPowerOff`

`GroupPowerOn/Off`의 ACK는 MotionLib state machine 시작 접수다. 최종 servo
상태가 아니다. `GroupEnable`은 power-on 완료와 mapping 등록을 전제로 하며
`LockProfile` 성공을 ACK로 반환한다.

## 명령별 계약

### GroupPowerOn / GroupPowerOff

- request: payload 1 byte, `Execute=1`, group descriptor `0x0100`
- `GroupPowerOn`: `RobotOn(Mode:=_ACTIVE)` 호출
- `GroupPowerOff`: `RobotOff()` 호출
- 둘 다 비동기 robot mode 전환을 시작한다. ACK 성공을 최종 servo 상태로
  해석하지 않는다.
- `GroupPowerOff` 전에 motion 정지와 `GroupDisable`을 완료하는 것이 정상 순서다.

### GroupEnable / GroupDisable

- request: payload 1 byte, `Execute=1`, group descriptor `0x0100`
- `GroupEnable`: power-on 완료와 identity mapping 등록을 확인한 뒤
  `LockProfile(Axis1..4:=1)` 호출
- `GroupDisable`: `ProfileInPosition(_LMCPROF_ProfileFinished)`를 확인한 뒤
  `UnlockProfile()` 호출. motion 중이거나 완료 상태가 확인되지 않으면 `-6` 거부
- `GroupEnable`은 servo power를 켜지 않고 `GroupDisable`은 servo power를 끄지
  않는다.
- `NC_GROUP_STANDBY_MASK(0x00020000)`와
  `NC_GROUP_DISABLED_MASK(0x00010000)`는 Maestro group-state mask 값을
  유지한다. 이 LASAL adapter는 power ready + profile locked + in-position을
  만족할 때 standby를, unlock 상태일 때 disabled를 설정한다.
- `0x00040000` Power Ready와 이를 노출하는 `IsPowerOn`은 이 프로젝트의
  LASAL adapter 전용 확장이다.

### GroupReset

- request: payload 1 byte, `Execute=1`, group descriptor `0x0100`
- action: group에 연결된 전체 축을 대상으로 `AxQuitError(AxisNo:=0)` 호출
- 주의: 이 호출은 축/하드웨어 오류를 초기화한다. robot profile 오류 전체를
  지우는 API가 아니며, ACK 뒤 `GroupReadStatusResult`로 `_ROBOT_ERROR`와
  `GroupErrorId`가 실제로 해제됐는지 확인해야 한다.
- servo enable이나 motion 재시작 명령이 아니다.

### GroupStop

- request: deceleration DINT, jerk DINT, `BufferMode=Aborting(1)`, `Execute=1`
- validation: `Deceleration>=0`, `Jerk>=0`; `Jerk>0`이면 `Deceleration>0`
- action: `StopMove(Mode:=3, Decel, Jerk)`
- `StopMove()` 반환 `UDINT StopCmdNo`는 정지가 끝날 profile-buffer command
  index이며 error/acceptance code가 아니다.
- ACK 성공은 입력 검증, robot client 연결과 stop dispatch를 뜻한다. 실제
  standstill과 profile error는 `GroupReadStatusResult`로 확인한다.

### MoveLinearAbsoluteEx

- wire position은 `DINT[16]`이지만 현재 프로젝트는 static 4축이다.
- position 1..4는 X/Y/Z/U axis order로 사용하고 5..16은 반드시 0이다.
- velocity, acceleration, deceleration은 양수, jerk는 0 이상이다.
- nonzero group jerk는 robot profile도 `_JERK_PROFILE`이어야 적용된다. 현재
  canonical network의 `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`로
  저장했으며 실제 다운로드된 PLC 설정은 실기 전에 다시 확인한다.
- coordinate system: `None(0)`만 허용
- transition: `ExactStop(0)` 또는 `ContinuousDirect(2)`
- buffer: `Aborting(1)` 또는 `Buffered(2)`
- C# serializer도 위 topology, dynamics와 option whitelist를 RPC 전 fail-fast한다.
- `Aborting`은 `CmdConfig.MoveImmediately` bit 값 16으로, `Buffered`는 0으로
  mapping한다.
- ACK 성공은 move 완료가 아니다. group status의 standby/in-position을 별도로
  확인한다.

### MoveLinearRelativeEx

- Admin v1 request payload는 104 bytes다. 공통 prefix 뒤 distance `DINT[16]`,
  velocity/acceleration/deceleration/jerk/coordinate/transition/buffer/execute를 둔다.
- distance 1..4는 X/Y/Z/U이고 slot 5..16은 0이어야 한다. dynamics와 options는
  absolute와 같은 whitelist를 사용한다.
- PLC는 Robot/Axis1..4 client, `GroupKinematicReady`, Robot power와 profile lock을
  확인한 뒤 `LMCRobot.MoveRelativeCoord`를 직접 호출한다. PC current-position 합산은 없다.
- Admin detail 9는 motion parameter, 10은 준비 상태, 11은 native profile reject다.
  detail 11의 representable positive GroupProfile code를 `ErrorId`에 보존하고 그 밖은
  adapter fallback `-6`을 사용한다.
- success ACK는 queue 수락이며 완료는 `GroupReadStatusResult`로 확인한다.
- WPF safety gate는 `0x7D00` preflight를 gate 밖에서 수행하고 session/connection-bound
  prepared capability overload로 gate 안에서 단일 `0x7D22`만 전송한다.

### GroupReadActualPosition

- request coordinate enum: `None(0)`, `ACS(1)`만 지원
- success response payload: `DINT[16] + UINT16 status + INT16 error`, exact 68 bytes
- 현재 프로젝트에는 dynamic `CalcModel`이 연결되지 않았다. None/ACS는
  `GetRobotPosition(Mode:=_ACTPOS_APPUNITS, CoordSystem:=0)`의 동일 static
  member-slot alias로 읽는다. MCS/PCS는 C#에서 `NotSupportedException`, PLC에서
  `ErrorId=-7`로 거부한다. 정의되지 않은 enum은 `-3`이다.
- 현재 tracked handler는 `_LMCPROF_POS` 36 bytes(`Pos1..Pos9`)를 응답 slot
  1..9에 복사하고 slot 10..16을 0으로 남긴다. 이는 현재 9개 software member
  metadata와 일치하며, Move/SetKin/Lock의 physical X/Y/Z/U 4축 제한을 넓히지 않는다.
- 결과의 `CoordinateSystem`은 PLC 응답 필드가 아니라 요청 enum의 PC-side echo다.
- ACS alias의 실물 동등성은 PLC smoke/packet capture가 남아 있다.
- PMAS legacy 136-byte LREAL response는 현재 PC parser가 거부한다.

### SetKinTransformCartesian4Axis

- request payload: exact 1,320 bytes
- X/Y/Z/U 4개 identity-shift node, axis reference 1/2/3/4, Cartesian type,
  `Buffered(2)`, `Execute=1`만 허용한다.
- unused node/union/tail byte가 모두 0인지 포함해 전체 shape를 검사한다.
- receive accumulator 2,048 bytes, request buffer 1,328 bytes, queue payload
  1,320 bytes로 같은 CyWork queue에서 처리한다.
- 성공하면 현재 static 4축 axis order mapping을 configured 상태로 등록한다.
  실제 coupling은 이후 `GroupEnable`이 `LockProfile`로 수행한다.
- 이 구현은 dynamic kinematic model을 생성하거나 임의 coefficient를 적용하는
  기능이 아니다.

## 오류와 범위

| ErrorId | group API에서의 의미 |
|---:|---|
| `-2` | 필요한 robot/axis client가 연결되지 않음 |
| `-3` | descriptor, payload 길이 또는 request 형식 오류 |
| `-6` | LASAL return code를 16-bit error로 보존할 수 없음 |
| `-7` | 현재 승인하지 않은 motion/kinematic 인자 조합 |

mapping, robot active 또는 profile lock이 준비되지 않은 state rejection도
`-6`으로 반환한다. 양수 MotionLib 오류는 16-bit 범위에서 그대로 전달한다.

공개 범위는 packet capture로 확인한 23개 command와 LASAL adapter 전용
`0x204A`/`0x204B` 2개를 합친 25개다. 위 group command를 더 이상 `-5`로
고정 차단하지 않는다. 단, source에 존재한다는 사실만으로 장비 지원 완료가
되지는 않는다.

`MoveCircle`은 현재 공개 C# API와 승인된 LASAL-DINT command ID, request/response
wire 계약이 없다. vendor `_LMCRobotBase.MoveCircleCoord` method가 존재한다는
이유만으로 임의 protocol을 만들지 않았으며 이번 구현 범위가 아니다.

## 남은 검증

1. canonical LASAL 프로젝트 Rebuild/Link
2. 변경 method의 Find in Implementation smoke와 이후 `Lasal2.log` 확인
3. `TCPIPServer1`/`TCPMotionInterface1` CyWork와 motion RT thread의 core/priority 확인
4. read-only group lookup/member/status/actual-position PLC smoke와 packet 재캡처
5. Reset을 실제 axis error와 profile error 상태에서 각각 검증하고, Reset이
   profile error까지 해제했다고 가정하지 않음
6. Stop ACK 뒤 실제 in-position/standstill을 별도로 확인
7. 무부하, 저속, 짧은 거리로 MoveLinear absolute/relative와 mode rejection 검증
8. exact SetKin request와 mapping ACK 검증
9. PowerOn -> `IsPowerOn` -> SetKin -> Enable -> `IsStandby` 순서와 역순
   rejection 검증
10. Disable -> PowerOff 뒤 실제 unlock/passive 상태 확인

위 항목을 통과하기 전에는 이 group API를 production 지원 완료로 표시하지 않는다.

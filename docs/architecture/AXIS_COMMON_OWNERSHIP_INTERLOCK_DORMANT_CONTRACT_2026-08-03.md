# Axis common ownership interlock dormant contract (2026-08-03)

> 2026-08-04 TW-only supersession: 이 문서의 TW19/TW20 gate OFF 및 축별
> encoder/socket manifest 판정은 dormant checkpoint 기록이다. 현재 두 gate는 exact
> `0x20FC:0x01/0x02`, UInt16 value `1` 계약으로 활성화됐다. 자세한 내용은
> [TW19/TW20 fixed-one activation](./LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md)을
> 따른다. Ordinary ownership, LMC Home 및 DS402 Home gate 상태는 별개다.

## 1. 목적과 현재 판정

이 문서는 LASAL server에서 axis 단위 mutation 소유권을 통합하기 위한 activation 전
계약이다. 대상은 ordinary Axis/Group Motion과 Power, LMC_Home, DS402 Home,
`TW[20]` 및 `TW[19]` encoder maintenance다.

현재 판정은 다음과 같다.

- production source에는 공통 owner table, startup reconciler, ordinary Axis/Group payload
  classifier, two-phase admission/final fence 및 terminal observer가 dormant 상태로 구현돼 있다.
- ordinary 경로의 paired source gate `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED`는
  `TCPMotionInterface`와 `LMCControlCommandService`에서 모두 `FALSE`다. 따라서 구현 존재가
  production 활성화를 뜻하지 않는다.
- `LMC_ADMIN_AXIS_HOME_ENABLED`, `LMC_DIAG_DS402_HOME_ENABLED`,
  `LMC_DIAG_ENCODER_TW20_ENABLED`, `LMC_DIAG_ENCODER_TW19_ENABLED`도 모두 `FALSE`다.
- Admin capability bit 4/6은 완전한 triad와 각각의 gate가 원자적으로 일치해야 한다.
  Diagnostics capability bit 18/19는 축별 encoder/socket 정보가 확인되기 전까지 OFF다.
- 현재 Admin `FeatureBits=0x00000007`, `ErrorCatalogVersion=1`이다. provisional adapter error
  `-9`는 SDK catalog v1에 symbolic 항목으로 노출하지 않았다.
- 이 문서는 source/static/IDE build checkpoint이며 PLC download, 실제 축 동시 요청 또는
  EtherCAT/pcap proof가 아니다.
- bit 4/6의 source 광고는 protocol availability일 뿐 PLC 실기 검증이 아니다. bit 18/19는
  아래 계약, 축별 호환 정보, download 및 PLC 동시 요청 검증 전에는 켜지 않는다.

2026-08-03 10:02:20의 LASAL save가 빈 stub으로 만든
`LMCEcatInputLatch.SubmitDs402HomeControl`, `GetDs402HomeControlState`,
`SubmitDs402HomeSetpointAlignment`은 09:33 pre-save blob에서 exact 복구했다. 현재 복구된
`LMCEcatInputLatch.st`는 1,457 lines, 50,510 bytes이고 SourceOnly 정적 계약은 PASS했다.
이 문장은 당시 복구 checkpoint다. 이후 canonical project의 최신 LASAL C78 ARM rebuild는
`0 error(s), 42 warning(s)`로 완료됐다. 최신 implementation 검색 smoke는
`RequiredPhase` 15 matches/2 files, `LMC_OWNER_PHASE_ACTIVE` 4 matches/1 file,
`LMC_DIAG_OWNER_PHASE_ACTIVE` 5 matches/1 file이었고,
`2026-08-04T01:33:02.6059607+09:00` 이후 새 `%TEMP%\Lasal2.log`
`CInvalidArgException`은 0건이다. 같은 source의 `SourceOnly`는 의도된 D5 Axis1 qualification
상태를 `-ExpectedSdoWriteAxis 1`로 명시해 PASS했고, ownership/encoder 음성 fixture는 각각
`114/114`, `52/52`, VS2019 Release PC test는 `1075/1075` PASS했다. PLC
download/runtime proof는 아직 없다.

## 2. 확인된 현재 source 사실

### 2.1 단일 dispatch 지점

`TCPMotionInterface`는 현재 다음 조건을 모두 만족하는 유일한 class다.

- `ControlCommands`와 `Diagnostics` client를 함께 가진다.
- 하나의 request queue를 `CyWork()`에서 직렬 처리한다.
- ordinary/Admin/Group command와 Diagnostics command를 두 service로 분기한다.
- 현재 Network에는 두 client 연결이 이미 존재한다.

따라서 TCP RPC mutation의 최초 admission과 축별 owner table의 논리적 system owner는
`TCPMotionInterface`로 고정한다. PC별 `LMCConnection`이나 WPF gate는 multi-PC PLC
ownership 근거가 아니다.

단, dispatcher 검사만으로 끝내지 않는다. 최종 native/SDO 호출 직전에 service가 exact
admission token을 확인해야 한다. token 없이 service를 직접 호출하는 새 LASAL 경로는
fail-closed해야 한다. activation 전 전체 project source에서 `_LMCAxis`/`LMCRobot` mutation이
이 admission을 우회하는 경로가 0건인지 별도로 검증한다.

### 2.2 ACK와 dormant retained owner

각 ordinary native handler 자체는 다음 native 호출의 반환값을 해당 RPC의 immediate
acceptance/return response로 바꾼다.

- Axis PowerOn/PowerOff `0x2023`
- Axis Reset `0x2024`
- Axis Stop `0x2022`
- Axis MoveAbsolute/Relative/Velocity `0x209F/0x20A0/0x20A2`
- Group Enable/Disable/Reset/PowerOn/PowerOff/Stop
  `0x2047/0x2048/0x2049/0x204A/0x204B/0x2085`
- Group MoveLinearAbsolute/Relative `0x20A4/0x7D22`

그 위에 `OwnershipState[0..351]`와 `OwnershipObserverState[0..107]`를 사용하는 retained
owner/admission/terminal observer가 구현돼 있다. TCP와 service 양쪽의 exact gate가 현재
`FALSE`이므로 ordinary request에는 아직 적용되지 않는다. 활성화 뒤에도 ACK는 owner 해제
또는 command 완료 증거가 아니며 ACK에서 owner를 해제하는 구현은 금지한다.

### 2.3 특수 operation의 현재 상태

- LMC_Home의 기존 switch-search `ReferenceState`/`MoveReference` 설계는 폐기한다. 새 계약은
  `ZeroHomeState`, RT InputLatch mailbox와 `SetPosition` current-position-zero 경로를 사용한다.
- DS402 Home은 RT mailbox release와 cleanup 뒤 terminal record를 게시한다. stage `101`
  quarantine에서는 owner를 해제하면 안 된다.
- `0x7D16` outcome query와 `0x7D17` retire는 구현됐지만 active motion owner는 아니다.
- TW20/TW19는 dedicated `0x7E53/0x7E54/0x7E55` lifecycle에서 motor-off를 검사하고 SDO
  terminal과 executor drain까지 PowerOn을 막는 공통 owner를 유지해야 한다.

### 2.4 Group의 두 axis 범위

Motion Network에서 `LMCRobot.Control`에는 Axis1..9가 모두 연결돼 있다. Axis1..4는 physical,
Axis5..9는 현재 simulation 설정이다. `0x20D2`가 Axis1..9, count 9를 게시하는 것은 이
robot-connected 범위와 일치한다.

반면 `0x2047 LockProfile`은 `Axis1..4:=1`, `Axis5..9:=0`으로 호출되고 Cartesian
MoveLinearAbsolute/Relative도 첫 네 좌표를 사용한다. 따라서 하나의 member mask를 모든 Group
command에 재사용하면 안 된다.

```text
RobotConnectedMask = 0x01FF  // Axis1..9
ProfileAxisMask     = 0x000F  // Axis1..4
```

`RobotOn`, `RobotOff`, `AxQuitError(AxisNo:=0)`처럼 robot 전체에 적용되는 명령은
source/vendor 의미와 runtime 관찰이 더 좁은 범위를 증명하기 전까지 `0x01FF`를 보수적으로
사용한다. Profile Lock/Unlock/Move/Stop은 explicit locked profile 범위 `0x000F`를 사용한다.
`0x20D2`는 robot-connected snapshot 근거이지 profile mask 근거가 아니다.

`UnlockProfile`, Profile Stop과 coordinate Move의 실제 영향 범위는 current coupled/locked
mask다. 현재 TCP가 만든 유일한 lock은 `0x000F`이므로 그 generation 안에서는 exact
`0x000F`다. restart 또는 외부 LASAL 직접 Lock 뒤 공개 boolean LockState만으로 mask를
복원할 수 없으면 `0x01FF` administrative quarantine을 사용한다. 이것은 StopMove가 9축을
실제로 움직인다는 뜻이 아니라 coordinator가 미확정 축 충돌을 놓치지 않기 위한 범위다.

## 3. command 분류 계약

분류는 payload 의미까지 본다. 같은 CommandId라도 PowerOn과 PowerOff는 다르다.

| 분류 | 의미 | admission |
|---|---|---|
| `A Ordinary` | 일반 mutation | 대상 축이 모두 `Idle`이거나 5.2의 exact command별 Group lease 전이여야 함 |
| `B Safety` | Stop/PowerOff/안전 해제 | 기존 owner 때문에 거부하지 않고 safety preemption으로 전이 |
| `C Read` | side-effect 없는 상태/결과 읽기 | axis owner conflict 때문에 차단하지 않음 |
| `D Lifecycle/Config` | Home/encoder-maintenance lifecycle, cancel, config | 명령별 전용 규칙 적용 |

| Command | 분류 | 축 범위와 owner 규칙 |
|---|---|---|
| `0x2023 PowerOn` | A | exact axis; stable powered/operation-enabled 관찰까지 유지 |
| `0x2023 PowerOff` | B | exact axis; safe Off stable 관찰까지 safety owner 유지 |
| `0x2024 AxisReset` | A | exact axis; fault와 AxisError stable clear까지 유지 |
| `0x2022 AxisStop` | B | exact axis; stable Standstill까지 safety owner 유지 |
| `0x209F/0x20A0` | A | exact axis; post-acquire activity 뒤 stable Standstill까지 유지 |
| `0x20A2 MoveVelocity` | A | exact axis; 자연 terminal 없음. Stop/PowerOff preemption까지 유지 |
| `0x2028/0x202E` | C | Axis status/position read. axis owner 때문에 차단하지 않음 |
| `0x2047 GroupEnable` | A | Idle `0x000F` 원자 획득. stable locked standby 뒤 exact Group lease로 전이 |
| `0x2048 GroupDisable` | B | exact lease와 fresh profile-finished에서 `0x000F` release. 이동 중에는 먼저 GroupStop 필요 |
| `0x2049 GroupReset` | A | `AxisNo=0`이므로 보수적 `0x01FF`; group/member error stable clear까지 유지 |
| `0x204A GroupPowerOn` | A | 보수적 `0x01FF`; stable powered 상태까지 유지 |
| `0x204B GroupPowerOff` | B | 보수적 `0x01FF` safety preemption; stable Off까지 유지 |
| `0x2085 GroupStop` | B | exact active lease mask safety preemption. mask를 복원할 수 없으면 quarantine |
| `0x20A4/0x7D22` | A | exact `0x000F` lease에서만 4-axis Group motion 허용 |
| `0x2045/0x2051/0x20D2` | C | Group status/position/member read. axis owner 때문에 차단하지 않음 |
| `0x20E7 SetKinTransform` | D | config mutation; 모든 member가 idle이고 Group lease가 없을 때만 허용 |
| `0x7D12 SetAxisPosition` | D/A | dormant; activation 시 exact axis exclusive owner 필요 |
| `0x7D13 LMC_Home Start` | D/Home | exact axis Home owner; current-position-zero, no motion/switch |
| `0x7D18 LMC_Home Outcome` | C | side-effect 없는 exact retained-outcome query |
| `0x7D19 LMC_Home Retire` | D | terminal ledger lifecycle mutation; 물리 축 owner를 획득하지 않음 |
| `0x7D15 DS402 Home Start` | D/Home | dormant; exact axis Home owner 필요 |
| `0x7D16 Home Outcome` | C | side-effect 없는 query. exact diagnostics identity 검사는 유지 |
| `0x7D17 Home Retire` | D | terminal ledger lifecycle mutation; 물리 축 owner는 획득하지 않음 |
| `0x7E53` TW20/TW19 Start | D/Encoder | exact axis motor-off owner를 executor drain까지 유지 |
| `0x7E54` Encoder Outcome | C | side-effect 없는 exact retained-outcome query |
| `0x7E55` Encoder Retire | D | terminal ledger lifecycle mutation; generic SDO를 전송하지 않음 |
| `0x7E50` generic SDO | D/SDO | TW19/TW20 special bypass 금지; 기존 exact generic allowlist만 유지 |
| `0x7E03 SDO status` | C | read-only. ticket와 OwnerSessionEpoch 검사는 유지 |
| `0x7E04 SDO cancel` | D | lifecycle abort. Stop/PowerOff와 같은 안전 완료로 간주하지 않음 |
| `0x7E20 ReadPI` | C | read-only. 이름이 비슷해도 TW20이 아님 |

object lookup, capability, parameter read와 기타 side-effect 없는 명령도 C로 취급한다. generic
SDO Write와 PI/DO Write는 이 표로 자동 승인하지 않는다. 각 diagnostics owner와 allowlist의
기존 정책을 유지하며, 물리 축 상태를 바꾸는 write가 추가되면 별도 분류를 승인해야 한다.

C Read는 axis mutation owner conflict 때문에 거부하지 않는다는 뜻이다. 기존 RPC session,
frame, object connection, build/BootId/map, record generation, ticket와 caller session 검증을
우회하지 않는다.

failure parser도 owner 종류별 exact allow-list를 유지한다. `LMC_Home` Start는 common envelope
외에 detail `10/13/15/16/17/18/40/41/42`만 허용하고 `11 NativeCommandRejected`는 허용하지
않는다. `LMC_Home` Outcome/Retire는 identity `16/17/18`과 outcome `33..37`만, DS402 Home
Outcome/Retire는 identity `16/17/18`과 outcome `25..29`만 16-byte failure envelope로
허용한다. identity mismatch는 query/retire가 축 mutation을 실행했다는 뜻이 아니다.

## 4. 축별 owner record

축 table은 현재 연결 가능한 Axis1..9를 표현할 수 있어야 한다. Home/TW20/TW19의 activation 대상은
Axis1..4로 제한한다. 최소 record는 다음 필드를 가진다.

```text
AxisReference
State
OwnerKind
CommandId
AdmissionToken
OwnerGeneration
SessionEpoch
AcquireCycle
LastObservationCycle
StableSinceCycle
GroupAxisMask
GroupReference
OperationIdentity0..N
```

`OperationIdentity`는 명령별 exact identity다. DS402 Home은 build/BootId/map/request/intent/
record generation을, TW20/TW19는 intent/sequence/BootId와 exact SDO target을, Group command는
group reference와 member mask를 보존한다.

필수 논리 state는 다음과 같다.

```text
Idle
Reserved
DirectOperationActive
GroupLeaseActive
GroupOperationActive
LmcHomeActive
Ds402HomeActive
Tw20Queued
Tw20Running
Tw20Draining
SafetyPreempting
Quarantined
```

`Reserved`는 dispatcher admission과 final native/SDO 호출 사이의 짧은 two-phase 상태다.
service의 확정적인 pre-wire rejection에서만 rollback할 수 있다. write 시도 여부 또는 응답
발행 여부가 불확실하면 `Idle`로 되돌리지 않고 `Quarantined`로 전이한다.

### 4.1 현재 구현된 exact storage

`OwnershipState`는 `ARRAY [0..351] OF DINT`다. global `0..27`은 table magic, token/generation
counter, BootId/startup proof, 세 singleton engine lease, synchronous request context, closed
session/process time 및 global quarantine를 보존한다. Axis `n`의 base는
`28 + (n - 1) * 36`이고 record offset은 다음과 같다.

```text
0 AxisReference                 1 State
2 OwnerKind                     3 CommandId
4 AdmissionToken                5 OwnerGeneration
6 SessionEpoch                  7 RequestSequence
8 AcquireCycle                  9 LastObservationCycle
10 StableSinceCycle             11 ExactAxisMask
12 Reference                    13 ResourceKind
14 AdmissionMode                15 IdentitySizeBytes
16..31 RawIdentityWords         32 ReportKind
33 ReportValue0                 34 ReportValue1
35 RecordMagic
```

global engine tuple은 LMC Home `7..9`, DS402 Home `10..12`, Diagnostics SDO `13..15`이며 각
tuple은 token/generation/exact mask다. `Commit`은 전체 mask를 한 번 검증한 뒤에만 두 번째
loop에서 mutation한다. `Validate`, `Rollback`, `Publish`는 mask 밖의 같은 token/generation과
singleton tuple 불일치를 mutation 전에 거부한다. `Rollback(Reason=0)`만 확정적인 pre-wire
거부를 Idle로 되돌리고, 그 외 rollback과 uncorrelated response는 resource를 유지한 채
quarantine한다. terminal publish 성공 뒤에만 axis record와 해당 engine tuple을 함께 비운다.

`ValidateAxisOwnership`은 마지막 입력 `RequiredPhase : UINT`와
`LMC_OWNER_PHASE_RESERVED=1`, `LMC_OWNER_PHASE_ACTIVE=2`를 사용한다. command/reference/mask,
owner/resource/admission, session/sequence/token/generation뿐 아니라 요청 phase와 retained state도
같이 검증한다. 현재 호출은 정확히 10개다. Control의 request/commit/Home start는 `RESERVED`,
Zero Home dispatch는 `ACTIVE`를 요구한다. Diagnostics의 encoder/DS402 start는 `RESERVED`,
encoder dispatch와 DS402 main SDO 및 cleanup stage 94/96 SDO는 `ACTIVE`를 요구한다. 따라서
예약만 성공한 tuple이나 잘못 결합된 Home/DS402/Encoder resource가 native/SDO 직전 fence를
통과하지 못한다. 이 정적 fence는 PLC에서 실제 preemption/cleanup을 증명했다는 뜻은 아니다.

`OwnershipObserverState`는 `ARRAY [0..107] OF DINT`, 즉 Axis1..9 각각 12 slots다. 현재
ordinary observer는 `InputLatch.CopyAxisOwnershipSnapshot`의 동일 seqlock cycle에서 Axis1..4
status/error와 DS402 `0x6041`을 읽고, 그 뒤에 `LMCRobot` 상태와 `ops.tAbsolute` service time을
결합한다. startup reconciler도 같은 latch cycle과 Diagnostics drain proof를 사용한다. 따라서
과거의 incoherent observer source blocker는 닫혔다. 다만 `3 samples`, `100 ms` stability와
`120000 ms` timeout은 PLC task/EtherCAT 주기에서 아직 측정되지 않았으므로 activation proof는
아니다.

현재 startup required proof는 exact `0x0000000F`다: bit 0 BootId, bit 1 physical axes stable
idle, bit 2 Group/profile idle, bit 3 Home/DS402/SDO executor와 mailbox drained. 과거
BootId-only `ReportAxisOwnershipStartup` caller는 삭제됐다. private
`LMCDiagnosticsService.ProcessAxisOwnershipStartup`가 seqlock 48-byte latch snapshot과 exact
Diagnostics drain `0x0000001F`을 모아 `ReconcileAxisOwnershipStartup`를 호출한다. reconciler는
서로 다른 fresh observation cycle 3개와 `ops.tAbsolute` 100 ms의 동일 signature를 만족한
경우에만 zero 또는 exact prior-boot idle table을 초기화한다. same-BootId의 이미 완료된 table은
정상 operation 중 transient non-idle을 startup failure로 재해석하지 않는다. 이 source 경로는
BootId-only permanent quarantine blocker를 제거하지만 100 ms 값은 아직 PLC에서 측정되지 않았고,
단순 gate 변경으로 기능을 활성화할 수 없다.

## 5. admission과 token 계약

### 5.1 direct axis

1. `TCPMotionInterface`가 payload까지 해석해 A/B/C/D를 분류한다.
2. A/D mutation은 exact axis record를 `Idle -> Reserved`로 compare-and-set한다.
3. nonzero `AdmissionToken`과 증가한 `OwnerGeneration`을 만든다.
4. service는 final native/SDO 직전에 axis, CommandId, token, generation, phase를 확인한다.
5. 검증 성공 뒤 service가 dispatch 결과와 operation identity를 coordinator에 게시한다.
6. native/SDO가 호출됐을 가능성이 있으면 response status와 무관하게 terminal observer가
   완료할 때까지 owner를 유지한다.

같은 축 owner가 충돌하면 correlated rejection을 반환하고 native/SDO 호출은 정확히 0회여야
한다. direct ordinary Axis operation은 다른 축에서 독립적으로 허용한다.

현재 LMC_Home engine의 `ZeroHomeState`, DS402 Home의 active stage, Diagnostics SDO
executor는 각각 capacity-1 global resource다. per-axis owner와 별도로
`LmcHomeEngineLease`, `Ds402HomeEngineLease`, `DiagnosticsSdoEngineLease`를 획득한다. 이
singleton implementation을 refactor하기 전에는 서로 다른 축 요청도 같은 engine 안에서는
직렬화한다. axis reservation 뒤 global resource가 busy임을 발견하는 순서 경쟁은 금지하며,
항상 global resource와 axis owner를 고정된 순서로 함께 획득한다.

### 5.2 Group

Group mutation은 command별 axis mask의 모든 축을 작은 축 번호 순서로 원자 획득한다. 한
축이라도 충돌하면 획득한 축을 남기지 않고 전체를 거부하며 native call은 0회여야 한다.
`RobotConnectedMask`와 `ProfileAxisMask`를 호출 의미에 따라 구분하며, 범위가 미확정인
robot-wide 명령은 더 넓은 `0x01FF`를 사용한다.

`GroupLeaseActive`는 exclusive 정지 상태가 아니라 exact group/generation/mask에 한정된
profile lease다. generic "같은 lease면 모든 A command 허용" 규칙은 사용하지 않는다.

- GroupEnable: Idle `0x000F`에서 시작해 stable locked standby 뒤 exact lease `0x000F` 유지
- GroupMove: exact lease `0x000F`에서만 transient operation으로 전이하고 정상 terminal 뒤
  같은 lease로 복귀
- GroupStop: lease/move에서 허용. exact mask, lock 상태와 error가 일치할 때만 같은 lease로 복귀
- GroupDisable: exact matching lease와 fresh ProfileFinished에서만 Unlock 뒤 Idle
- GroupPowerOn/GroupReset: `0x01FF`; 기존 profile lease와의 호환성을 별도 증명하기 전 거부
- GroupPowerOff: `0x01FF` safety preemption 뒤 기존 lease 파기
- Group 일반 명령 시작: mask 밖을 포함해 non-Group owner가 하나라도 있으면 전역 거부. 기존
  direct safety owner도 예외가 아니며 snapshot/token/identity 변경 전에 거부
- direct axis mutation, Home, TW20/TW19 시작: Group lease와 mask 밖의 축에서도 전역 충돌
- 한 member에 direct safety preemption이 들어오면 group mask 전체를 safety/quarantine 범위로
  승격하고 Group lease를 정상 상태로 남기지 않음

이미 Group이 성립한 뒤 들어오는 mask 밖의 direct safety만 one-way 예외다. Group owner를 덮지
않고 `TailSizeBytes=0`인 자기 identity 경로가 suffix를 한 byte도 쓰지 않는 경우에만 독립적으로
허용한다. cleanup도 실제 소유한 tail byte만 검증하며 사용하지 않는 8-byte slot의 zero 상태를
요구하지 않는다. safety가 먼저 active인 상태에서 새 ordinary/lifecycle Group 명령을 시작하는
역방향 예외는 없다.

live Group이 없는 fresh Group safety는 exact requested mask 밖의 non-Group owner가 하나라도 있으면
mutation 전에 거부한다. mask 안의 owner만 safety preemption 대상으로 허용한다. 이미 성립한 Group의
safety transition은 기존 preemption root와 zero-tail 규칙을 모두 만족할 때만 완전히 disjoint한
direct safety owner를 보존할 수 있다.

`0x20D2` count 9를 profile axis count 4로 바꾸어 해석하거나, 반대로 profile 명령을 9축
명령으로 확장해서는 안 된다. command별 mask를 명시적으로 선택한다.

## 6. safety preemption

다음 명령은 기존 owner 때문에 차단하지 않는다.

- Axis Stop
- Axis PowerOff
- Group Stop
- Group PowerOff
- 조건을 만족한 Group Disable

처리 순서는 다음과 같다.

1. 관련 owner를 `SafetyPreempting`으로 원자 전이하고 기존 operation의 신규 단계 진행을
   동결한다.
2. exact Stop 또는 PowerOff를 같은 cyclic admission에서 정확히 한 번 즉시 dispatch한다.
3. Home abort/DS402 bit 4 low/CSP 복귀/RT mailbox release 또는 TW20/TW19 cancel/drain을 병행한다.
4. fresh status의 승인된 안전 상태와 owner-specific cleanup이 모두 stable한 경우에만
   release한다.
5. physical safe state는 확인됐지만 cleanup이 불확실하면 `Idle`이 아니라
   `Quarantined`로 남긴다.
6. timeout, response loss, impossible state, stale identity 또는 cleanup 불일치는
   `Quarantined`로 남긴다.

DS402 Home의 overall/cleanup timeout은 RT latch cycle이 아니라 `ops.tAbsolute` service
time으로 진행한다. runtime slot 118은 service start, slot 119는 cleanup start다. fresh latch는
stable evidence에만 필요하며 `newCycle AND timeout` 조건은 금지한다.

Axis Stop이 active Group member 하나에 들어오거나 Axis PowerOff가 Group lease member에
들어오면 해당 axis만 해제하지 않는다. group mask 전체의 기존 operation/lease를 무효화하고
명시적인 Group Stop/Disable/PowerOff 및 상태 재검증 전까지 fail-closed한다.

2026-08-04 현재 source에는 별도 preempted owner/identity bank와 144-byte copy header가 추가돼
이전 special owner의 kind, resource, admission, session, request sequence와 raw identity를 보존한다.
LMC Home은 56-byte identity를 포함한 exact 200-byte snapshot을 검증하고, safety native handler 전
RT cancel을 게시한 뒤 commit 후 terminal drain과 preemption cleanup을 수행하도록 연결됐다.
이 경로는 SourceOnly 정적 계약만 PASS했으며 변경 뒤 C78/PLC runtime proof는 아직 없다.
DS402 bit 4 low/CSP 복귀, TW20/TW19 cancel 및 executor drain, Group lease 파기는 동일 overlay의
consumer 계약이 아직 닫히지 않았으므로 safety와 해당 special gate를 함께 활성화하지 않는다.

## 7. terminal과 release 조건

공통 원칙은 다음과 같다.

- ACK에서 release하지 않는다.
- 단일 status read에서 release하지 않는다.
- terminal observer는 owner generation과 exact operation identity가 일치해야 한다.
- Move는 dispatch 뒤 실제 activity transition을 관찰하기 전 terminal로 release하지 않는다.
- 최소 3개의 연속 fresh sample과 승인된 최소 안정 시간 창을 모두 만족해야 한다.
- 안정 시간 값은 PLC task cycle과 EtherCAT update를 측정한 뒤 상수로 고정한다. 측정 전에는
  gate를 켜지 않는다.

현재 source 상수는 ordinary terminal `3 samples`, `100 ms`, timeout `120000 ms`다. 이 값은
compile proof만 있고 PLC task/EtherCAT 주기에서 측정되지 않았다. 또한 handler entry를 넘은
뒤 response code만으로 native 호출 전/후를 구분할 수 없으므로 current final fence는 accepted가
아닌 모든 결과를 보수적으로 quarantine한다. 정확한 rollback/quarantine 분리를 위해 handler의
실제 native-call 직전 또는 직후 marker가 추가돼야 한다.

명령별 최소 조건은 다음과 같다.

| owner | 최소 terminal/release 조건 |
|---|---|
| PowerOn | `AxisStatus.PowerOn=1`, `AxisError=0`, stable sample window |
| Reset | Fault clear와 AxisError=0, stable sample window |
| MoveAbsolute/Relative | accepted 뒤 moving 또는 InPosition true-to-false activity를 먼저 관찰한 후 Standstill stable. activity 없는 no-op은 exact target-equal 증거 없이는 자동 release 금지 |
| MoveVelocity | Stop/PowerOff safety terminal 외 자동 release 없음 |
| AxisStop | post-dispatch Standstill stable. AxisError가 남으면 safe-fault/quarantine |
| AxisPowerOff | `AxisStatus.PowerOn=0` stable |
| GroupPowerOn | exact member 모두 power-ready와 group power state stable |
| GroupReset | group error=0과 exact member AxisError=0 stable |
| GroupMove | owner generation 뒤 profile-active transition을 먼저 관찰한 후 ProfileFinished/InPosition과 exact affected member Standstill stable |
| GroupStop | post-dispatch ProfileFinished/InPosition과 exact affected member Standstill stable. activity transition은 불필요 |
| GroupEnable | locked standby stable; 이후 Group lease로 유지 |
| GroupDisable | disabled/unlocked stable; moving 상태의 Disable ACK만으로 release 금지 |
| GroupPowerOff | exact member power-off와 group power state stable |
| LMC_Home | exact retained terminal, RT mailbox release, stable current-position-zero evidence |
| DS402 Home | RT owner release와 cleanup 완료 뒤 exact terminal record publish |
| TW20/TW19 | exact retained terminal, executor drain/release, motor-off stable |

`0x2028`만으로 DS402 OperationEnabled/Disabled를 증명했다고 기록하지 않는다. 그 predicate가
필요하면 InputLatch의 fresh coherent `0x6041` snapshot과 cycle/identity를 별도로 결합한다.
Group terminal도 `0x2045` 한 번으로 member 상태를 증명하지 않고 affected mask의 각
`0x2028`을 함께 관찰한다.

## 8. session, response loss와 restart

- TCP/RPC session close는 active owner의 release 조건이 아니다.
- response loss 뒤 original mutation을 자동 replay하지 않는다.
- active identity를 재구성할 수 없으면 `Quarantined`다.
- 새 PC session은 read/recovery/adopt 계약 없이 기존 owner를 덮어쓰지 않는다.
- PLC restart는 RAM owner table을 지울 수 있으므로 새 BootId에서 physical/group/diagnostics
  상태를 재관찰한다. 움직임, power transition, profile lock, Home/TW20/TW19 executor 상태를
  `Idle`이라고 증명하지 못하면 startup `Quarantined`로 시작한다.
- `0x2045`의 boolean profile lock 상태만으로 exact coupled mask 또는 이전 Group lease를
  복원하지 않는다.

durable PC journal은 recovery UX를 제공할 뿐 PLC owner를 대체하지 않는다.

## 9. wire rejection 계약

owner conflict는 framing failure가 아니라 correlated command rejection이다.

- Admin Home/SetPosition/Reference start: 기존 `InvalidState`, detail `10`,
  `ErrorId=-31000`을 사용한다. DS402 Home의 detail `32`는 unretired terminal slot 전용이며
  active owner conflict에 재사용하지 않는다.
- Diagnostics TW20/TW19: dedicated start의 `ResourceBusy`, detail `9`를 사용한다.
- ordinary Axis/Group: 기존 4-byte status/error payload 형태를 유지하되 ownership 전용
  adapter error가 필요하다.

adapter error `-6`은 `NativeErrorNotRepresentable`, `-7`은
`UnsupportedArgumentCombination`, `-8`은 `QueueOrFramingError`다. 세 값을 ownership
conflict로 재사용하지 않는다. SDK adapter catalog v2에는 `-9 AxisOwnershipConflict`를 exact
symbolic entry로 추가했다. 이 값은 ordinary ownership gate가 활성화된 paired PLC/SDK build에서만
production response로 사용한다.

현재 LASAL source의 dormant rejection `-9`, SDK adapter catalog v2와 C# test는 정적으로 일치한다.
PLC admin catalog version `5`와 SDK adapter catalog version `2`는 서로 다른 catalog이므로 같은
숫자로 묶지 않는다. C78/download/runtime paired rollout 전에는 ordinary gate를 `TRUE`로 바꾸지
않는다.

## 10. 구현 및 activation 순서

2026-08-04 기준 아래 원래 순서 중 source/IDE declaration, implementation 및 build/search
smoke 일부는 수행됐다. 이 절의 초기 blocker였던 record당 16 DINT identity 제한은 이후
`OwnershipIdentityState` compact prefix-and-tail overlay로 대체됐다. 현재 source/static 계약은
`0x20E7`의 1320-byte payload까지 byte-exact identity를 보존하고, Group transition 전 lease의 full
record와 identity도 별도 bank에 보존한다. 이 완료는 C78/download/runtime 증거를 대신하지 않는다.

1. safety preemption에서 이전 special owner 전체 identity를 보존하는 overlay는 source/static으로
   구현됐다. 변경 뒤 C78와 PLC runtime 검증은 남아 있다.
2. large payload의 exact full identity storage/validation은 source/static으로 구현됐다.
3. Group transition 전 lease의 full record/identity 보존과 byte-exact restore는 source/static으로
   구현됐다.
4. LMC Home, DS402 Home same-RESERVED drain, TW19/TW20 및 Group lease cleanup consumer는 각각의
   source/static checkpoint까지 구현됐다. C78와 실제 동시 safety/runtime 검증은 남아 있다.
5. 같은 safety lineage의 반복 Stop coalescing과 Stop/Disable-to-PowerOff monotonic escalation은
   source/static으로 구현됐다. 기존 preemption root/cleanup evidence를 손상하지 않는 negative
   fixture도 고정됐다. private IDE ABI, C78 build와 PLC runtime proof는 남아 있다.
6. `100 ms` stability와 `120000 ms` timeout을 PLC에서 측정해 확정한다.
7. source/static 완료된 SDK catalog v2/symbolic `-9`를 PLC activation build와 paired rollout한다.

coherent InputLatch observer와 handler native-call marker의 과거 source/C78 build-search smoke는
최신 repeat helper 변경의 build 증거가 아니다. 현재 checkpoint는 다시 private IDE ABI부터 검증해야
하며, 어떤 정적 결과도 PLC runtime proof를 대신하지 않는다.

원래 구현 순서는 다음과 같다.

1. 이 dormant 계약과 command matrix를 정적 fixture의 기준으로 고정한다.
2. `RobotConnectedMask=0x01FF`와 `ProfileAxisMask=0x000F`를 command별로 고정하고
   Network/source/runtime에서 검증한다.
3. owner record, admission/token, terminal observation에 필요한 LASAL declaration을 IDE에서
   추가한다.
4. `TCPMotionInterface`에 serialized admission과 group atomic acquire를 구현한다.
5. `LMCControlCommandService`와 `LMCDiagnosticsService`의 final-prewire token 검증과
   owner observation 게시를 구현한다.
6. same-axis 양방향 conflict, different-axis 허용, read 허용, zero-native-call rejection,
   group 부분 획득 금지, safety preemption, session loss quarantine를 음성 fixture로 검증한다.
7. LASAL F9 build 뒤 Object Network Server/Client는 `Find in Implementation`으로 class-index
   연결을 확인하고, 변경 function/method는 `Edit Method` 또는 `Enter`로 직접 열어 exact
   Implementation header를 확인한다. smoke 시작 이후 `%TEMP%\Lasal2.log` 신규
   `CInvalidArgException=0`을 확인한다.
8. PLC download 뒤 pcap과 status polling으로 동시 요청 및 terminal release를 검증한다.
9. 모든 activation 조건을 만족한 paired PLC/SDK build에서만 capability와 error catalog를
   갱신한다.

## 11. 사용자에게 요청할 LASAL IDE 작업 경계

새 type, class variable, function signature 또는 Network 연결이 필요해지는 시점에는 먼저 다음을
정확히 확정한다.

- 변경 class와 정확한 declaration 이름/type/visibility
- 추가 또는 변경할 function input/output
- 필요한 object/client/server/channel과 연결 대상
- 저장 뒤 수행할 F9 build, 검색어와 확인할 log 시간

사용자가 2026-08-04에 최종 조정한 LASAL IDE 직접 제어 시간은 다음과 같다.

- 평일: 17:30부터 다음 날 08:00까지
- 토요일, 일요일, 대한민국 공휴일: 종일
- 그 밖의 평일 시간: 사용자에게 IDE 작업을 요청한 뒤 진행

공휴일 여부가 필요한 날에는 대한민국 표준시와 해당 연도 공휴일을 확인한다. 이 시간 허용은
IDE 조작 권한만 정하며, safety gate 활성화, PLC download 또는 실기 운전 승인으로 확대 해석하지
않는다.

기존 implementation body는 추적된 `.st` source에서 수정하고 정적 계약을 먼저 검증한다.

## 12. activation 검증 matrix

최소 PLC bench/pcap case는 다음과 같다.

1. DS402/LMC_Home active 축에 Axis Move/PowerOn/Reset: correlated reject, native call 0회.
2. Axis Move/PowerOn/Reset active 축에 Home/TW20/TW19: correlated reject, native/SDO call 0회.
3. Axis1 direct ordinary owner active 중 Axis2 direct ordinary command: 허용.
4. Home/TW20/TW19 active member를 포함한 Group mutation: 전체 reject, 부분 owner 0개.
5. Group lease/motion active member에 direct Home/TW20/TW19: reject.
6. Home/motion 중 Axis Stop/PowerOff: 차단하지 않고 safety cleanup 후 stable release.
7. Group motion 중 Group Stop/PowerOff: 한 번만 dispatch하고 member 전체 stable release.
8. Group member 하나의 direct safety preemption: group lease를 정상 상태로 남기지 않음.
9. response loss/session close: owner 자동 release와 mutation replay 모두 0회.
10. stale token/generation, impossible state, executor orphan: quarantine.
11. read/status, `0x7D16`, exact `0x7D17`, `0x7E03`: 기존 identity/session 검사를 유지하면서
    axis owner conflict 때문에 차단하지 않음.
12. PLC restart/new BootId: physical state가 idle임을 증명하기 전 mutation zero-wire.
13. Axis1 safety preemption snapshot에 복사된 Axis2 active lifecycle token: Axis2의 exact live
    tuple/identity가 유지되면 `NONE`, partial overlap 또는 증거 불일치는 quarantine.
14. preempted encoder/DS402 executor가 idle이어도 exact completion 또는 exact-token orphan/drain
    증거가 없으면 singleton을 해제하지 않고 incomplete quarantine.
15. active Group identity `0x7D22` suffix bytes `0..39`가 있는 동안 mask 밖 Axis5
    `TailSizeBytes=0` safety lifecycle: Group suffix byte-exact 보존, suffix write 0회.
16. completed old singleton cleanup 뒤 같은 engine의 coherent replacement owner를 획득한 상태에서
    old cleanup replay: `Result=1`, replacement record/identity/singleton byte 변경 0회. partial 또는
    mixed old tuple은 corruption.

정적/PC PASS와 IDE build PASS는 위 PLC runtime proof를 대신하지 않는다.

## 13. 2026-08-04 same-RESERVED safety drain checkpoint

DS402 Home bit-4 LOW/readback이 한 scan에서 끝나지 않을 때 safety Stop/PowerOff reservation을
rollback한 뒤 새 token으로 다시 예약하지 않는다. `TCPMotionInterface`는 exact axis mask,
admission token, owner generation, session epoch와 request sequence를 active request tail에 primary/copy
tuple과 checksum으로 보존한다. internal result `-10`은 wire로 보내지 않고 같은 `RESERVED` tuple로
다음 scan의 `HandleRequest`를 재호출한다. terminal ownership finalize는 service만 수행한다.

다음 failure는 모두 native dispatch 전에 fail closed한다.

- retained marker 또는 tuple/checksum corruption
- 1000 ms pending timeout
- disconnect/takeover
- managed command의 classifier-invalid shape
- asynchronous socket close 지연 또는 실패

비동기 close 요청 전 old session epoch를 `PendingClosedSessionEpoch`에 latch하고 current epoch를 즉시
증가시켜 이미 READY인 old request도 stale로 만든다. `Reserved` marker 한 DINT가 손실돼도 five safety
form과 tail magic/dual tuple이 일치하면 pending continuation으로 복구하며 두 번째 Reserve를 금지한다.

`MsgPaser`가 32768-byte ceiling을 넘지 않도록 이 로직은 private implementation method
`HandleControlSafetyDrainPending`로 분리했다. pre-IDE source staging은 전체 static contract와
negative mutation `25/25`를 통과했다. 기본 verifier는 아래 generated declaration과 `Classes.lcb`
metadata가 없으면 의도적으로 실패한다.

```text
HandleControlSafetyDrainPending
  Phase : UINT
  EffectiveAxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  Result : DINT
```

현재 gate endpoint는 계속 all-dormant다.

| component | dormant | active candidate |
|---|---:|---:|
| TCP ordinary ownership | `FALSE` | `TRUE` |
| Control ordinary ownership | `FALSE` | `TRUE` |
| Diagnostics DS402 Home | `FALSE` | `TRUE` |
| InputLatch startup bit-4 sweep | `FALSE` | `TRUE` |
| AdminFeatures | `0x17` | `0x57` |

중간 조합은 허용하지 않는다. all-active 형태를 정적으로 허용하는 activation matrix를 추가하더라도
C78 compile, cold download, single-axis DS402 drain, LMC Home 연속 축, TW19/TW20과 physical safety
terminal evidence 전에는 실제 source gate와 Admin bit 6을 dormant 값에서 바꾸지 않는다.

2026-08-04 atomic activation verifier는 다음 두 endpoint만 허용하도록 구현됐다.

- all-dormant: `FALSE/FALSE/FALSE/FALSE + 0x00000017`
- all-active: `TRUE/TRUE/TRUE/TRUE + 0x00000057`

TCP/Control/Diagnostics/InputLatch gate의 exact definition/use inventory, Admin bit 6, SDK
`AxisDs402Home = 1u << 6`, SDK adapter catalog v2의 symbolic `-9`, PLC admin catalog version `5`를
서로 독립적으로 검증한다. dormant/active baseline과 단일 flip, 잘못된 literal, 누락/중복/nonliteral,
reachability guard 제거, repeated-safety root 검증을 포함한 ownership negative fixture `247/247`가
거부됐다. 이 중 repeated-safety 신규 fixture는 `27`개다. TCP safety-drain과 Control safety-repeat
helper waiver를 함께 적용한 당시 SourceOnly도 PASS했다. 실제 source endpoint는 all-dormant 그대로다.

## 14. 2026-08-04 repeated safety and TCP method-size checkpoint

같은 safety lineage의 반복 Stop은 escalation 전 byte-exact payload만 coalesce한다. Axis Stop 또는
Group Disable/Stop에서 PowerOff가 한 번 수락된 뒤에는 그 PowerOff가 포함하는 유효한 Stop/Disable/
PowerOff 반복을 native call 없이 coalesce한다. 최초 command/token/generation/session/sequence/identity와
preemption root는 바꾸지 않는다. root magic이 있으면 conflict 반환 전에도 complete preemption copy
검증을 먼저 수행한다. escalation bit를 게시한 뒤 기존 Standstill evidence를 모두 지우며, terminal
observer는 Axis reference PowerOff 또는 Group power-state zero와 all-member PowerOff를 새로 증명해야
한다.

LASAL Save All이 tracked implementation의 line ending을 CRLF로 정규화한 전례가 있으므로 raw/LF
크기만으로 `32768`-byte method ceiling을 판단하지 않는다. 기존 `MsgPaser`는 LF `32439` bytes였지만
all-CRLF `33354` bytes로 ceiling을 넘었다. ownership과 무관한 RPC lifecycle arm `0x8080/0x405C/0x405D`
를 private no-I/O `HandleRpcLifecycleCommands`로 byte-exact 이동했다. in-memory reverse transform으로
추출 전 TCP source SHA-256 `BC089DB425166D14D121D4A165CD280F918EC2535EBAF400C594A549E71DC459`를
완전히 복원해 이동 외 byte 변경이 없음을 확인했다.

현재 source/static size는 다음과 같다.

| method | raw | LF | all-CRLF |
|---|---:|---:|---:|
| `TCPMotionInterface.MsgPaser` | `28439` | `28439` | `29209` |
| `TCPMotionInterface.HandleRpcLifecycleCommands` | `4249` | `4249` | `4403` |
| `TCPMotionInterface.HandleControlSafetyDrainPending` | `16468` | `16468` | `16916` |
| `LMCControlCommandService.HandleRequest` | `32410` | `31727` | `32573` |
| `LMCControlCommandService.HandleAxisOwnershipSafetyRepeat` | `24340` | `24340` | `25055` |

ownership self-test `247/247`, callback negative fixture 기존 `8`개와 신규 RPC helper/route/ABI/size
fixture `7`개가 모두 거부됐다. 아래 세 pre-IDE waiver를 함께 사용한 full SourceOnly는 PASS했다.

```powershell
Verify-LasalContract.ps1 `
  -RepositoryRoot C:\work\Elmo\Elmo_Master `
  -SourceOnly `
  -ExpectedSdoWriteAxis 1 `
  -AllowPendingTcpSafetyHelperDeclaration `
  -AllowPendingControlSafetyRepeatHelperDeclaration `
  -AllowPendingTcpRpcLifecycleHelperDeclaration
```

waiver를 하나씩 제외한 세 실행은 각각 대응하는 private declaration/`Classes.lcb` metadata 누락으로
독립 fail-closed했다. 다음 activation 순서는 고정한다.

1. 세 private helper ABI를 한 번의 LASAL IDE 작업으로 추가한다.
2. Save All 뒤 Rebuild하지 않고 IDE를 종료한다.
3. generated declaration/metadata, actual/LF/all-CRLF method size와 source hash를 외부 검사한다.
4. 세 waiver 없는 default SourceOnly PASS 뒤에만 C78 Rebuild를 수행한다. Object Network
   Server/Client는 `Find in Implementation`으로 class-index/source 연결을 확인하고, 변경
   function/method는 `Edit Method` 또는 `Enter`로 직접 열어 exact Implementation header를
   확인한다.
5. cold download와 single-axis/연속-axis/runtime safety 검증 전에는 ordinary gate, Admin bit 6 또는
   capability/catalog를 활성화하지 않는다.

이 checkpoint에서는 설계/IDE handoff 문서만 갱신한다. 사용자/API/배포 매뉴얼은 C78와 runtime
evidence가 안정될 때까지 갱신하지 않는다.

# LMC_Home / DS402Home 구현 설계 - 2026-09-11

## 1. 결론

`LMC_Home`과 기본 `DS402Home`의 SDK, wire, PLC retained lifecycle, RT executor,
WPF one-shot/recovery 골격은 현재 source에 이미 구현돼 있다. 다음 구현은 상태 머신을 새로
작성하는 작업이 아니라, 2026-09-11의 1 physical axis testbed에 맞게 physical eligibility와
Admin capability를 정합화하고 동일 PLC image에서 동작을 검증하는 작업이다.

현재 구현 대상은 다음 두 기능이다.

| 기능 | 의미 | 이동 | current wire |
|---|---|---:|---|
| `LMC_Home` | LASAL application position을 현재 위치 기준 0으로 재정의 | 없음 | `0x7D13 / 0x7D18 / 0x7D19` |
| `DS402Home` | DS402 method 37, Home offset 0으로 현재 위치를 0으로 확정 | 없음 | `0x7D15 / 0x7D16 / 0x7D17` |

switch, limit 또는 index를 찾으면서 축을 이동하는 Homing은 이 문서의 기본 `DS402Home`이
아니다. 그것은 별도 `HomeDS402Ex` 설계와 qualification 대상이다.

가장 중요한 제어 규칙은 다음과 같다.

> Home/Referenced가 FALSE라는 이유로 Servo On을 거부하면 안 된다. Servo On과 Home은
> 독립된 상태다. 이동형 Homing은 `PowerOn -> Operation enabled -> Homing` 순서가 필요할 수
> 있으므로 Home 완료를 Servo On의 선행조건으로 만들지 않는다.

---

## 2. 기준과 판정 범위

- branch: `dev`
- inspected HEAD: `420c20f75c6a4ac8b946a7834588e173a39ec170`
- HEAD summary: `dev : update hw`
- current EtherCAT intent: CREVIS + Elmo 1 drive / 1 physical axis
- production posture: **NO-GO**

이 문서는 current tracked source를 기준으로 작성했다. 작업 시점의 working tree에는 이 문서와
무관한 사용자 변경이 존재하므로 실제 구현자는 변경 직전에 `git status`, current generated
tables, target PLC image identity를 다시 확인해야 한다.

이 문서에서 확인한 것은 source/static 구조다. 다음 증거는 아직 이 문서로 확보되지 않는다.

- LASAL IDE Compile/Rebuild
- generated C78/`Classes.lcb` 동일성
- PLC download와 BootId 변경
- Axis1 runtime 정상/실패 matrix
- drive Statusword/SDO readback
- 물리축 무이동 또는 이동 여부

### 2.1 2026-09-11 first changeset 구현 상태

이 문서의 13장에 정의한 첫 changeset인 H-CFG와 deterministic Axis2 rejection을
tracked source에 반영했다.

| 항목 | 반영 결과 |
|---|---|
| physical mask | TCP Home pre-admission, InputLatch, ownership, Diagnostics 모두 `0x00000001` |
| Admin capability | `PhysicalAxisCount=1`, AxisHome bit 4 ON, AxisDs402Home bit 6 ON |
| LMC Home Axis2..4 | TCP에서 owner reservation 전에 detail 4로 거부하고 service dispatch하지 않음 |
| service 방어 | `ReserveAxisOwnership`과 LMC Home Start handler가 같은 physical subset을 재검증 |
| DS402 Home Axis2..4 | Diagnostics preflight에서 intent stage 87 전에 detail 4로 거부 |
| Servo Power | `SetResolvedGroupAxesPower`에 Home/Referenced predicate가 없음을 정적 검사 |
| ordinary ownership | TCP/Control global ordinary gate는 `FALSE` 유지 |
| PC 선차단 | Admin `PhysicalAxisCount=1`에서 Axis2 LMC Home과 DS402 Home prepare가 모두 zero-Start-wire `NotSupportedException` |

추가한 `tools/Verify-HomeOneAxisImplementation.ps1`은 71개 source/static 계약을 통과했고,
`tools/Verify-HomeDs402H37CurrentDevRegression.ps1`의 Home/H37 통합 계약도 통과했다.
SDK Debug/Release 빌드는 통과했으며 새 Axis2 선차단 테스트도 두 구성에서 통과했다.

전체 SDK 결과는 Debug와 Release 모두 `1203 total / 1199 passed / 4 failed`다. 실패한 항목은
`Response.DiagnosticsCapabilities.MalformedRejected`와 Diagnostics parser deterministic fuzz 3개다.
이들은 이번 Home 변경 경로가 아니므로 Home-focused 결과와 전체 회귀 결과를 분리한다. 전체 회귀는
PASS가 아니다.

현재 환경에는 Visual Studio MSBuild가 없고 `dotnet msbuild`가 기존 WPF 프로젝트의 XAML
generated partial class를 만들지 못해 WPF smoke build는 실행되지 않았다. LASAL IDE compile,
generated C78 동기화, PLC download, fresh BootId, Axis1 runtime과 물리축 증거도 여전히 미확보다.

또한 current generated EtherCAT table은 `CREVIS index 0 + Elmo Axis1 index 1`이지만 기존
Diagnostics static topology serializer와 과거 `Verify-CurrentPhysicalTopology.ps1`은 two-Elmo
inventory를 유지한다. 이번 첫 changeset은 Home physical eligibility를 current table에 맞춘
범위이며, 전체 topology serializer revision 재생성과 PC topology golden 갱신은 별도 changeset으로
수행해야 한다.

---

## 3. 세 기능의 경계

### 3.1 `LMC_Home`

`LMC_Home`은 `_LMCAxis.SetPosition`의
`LMCAXIS_SET_ACTPOS_APPUNIT_DEST` mode를 사용해 현재 좌표계를 0으로 재정의한다.
`MoveAbsolute`, `MoveRelative`, velocity motion 또는 Home switch search를 호출하지 않는다.

완료는 native call return만으로 판정하지 않는다. RT executor가 아래 값을 다시 읽어 일치시킨다.

- raw drive position drift가 허용 범위 안에 있음
- application actual position = 0
- application set position = 0
- internal actual position = 0
- internal set position = 0
- destination position = 0
- master position = 0
- Standstill 유지

따라서 이 기능은 좌표 재정의이며 물리 원점 탐색 증거가 아니다.

### 3.2 기본 `DS402Home`

기본 `DS402Home`은 method 37만 허용한다.

- `0x607C Home offset = 0`
- `0x6098 Homing method = 37`
- velocity, acceleration, distance limit, torque limit = 0
- DS402 mode `0x6060 = 6` 진입
- controlword bit 4 rising edge
- Home attained, no error, method-37 completion status 확인
- Switch On Disabled에서 method 37 attained가 확인되면 Target reached는 필수로 강제하지 않음
- ActualPosition은 raw drive count 기준 0 +/- 1을 fresh stable sample로 확인
- bit 4 low 복구
- LASAL setpoint alignment
- CSP mode `0x6060 = 8` 복귀와 readback

method 37은 switch search motion을 수행하지 않는다. Start ACK는 접수 증거일 뿐이며
`0x7D16` terminal outcome과 exact retirement 전에는 완료로 처리하지 않는다.

### 3.3 이동형 `HomeDS402Ex`

다음 요구가 있으면 기본 `DS402Home`을 확장하지 말고 `HomeDS402Ex`를 사용한다.

- Home switch 또는 limit switch 탐색
- index pulse 탐색
- positive/negative direction travel
- detection velocity, search velocity, acceleration 또는 travel limit
- torque/block detection
- nonzero Home offset

이 경로는 배선, polarity, debounce, approved method allowlist, travel limit, scale와 drive fault
behavior가 확정돼야 한다. 실제 이동형 Homing sequence는 보통 다음과 같다.

```text
PowerOn
-> DS402 Operation enabled 확인
-> HomeDS402Ex Start
-> method-specific motion/detection
-> terminal outcome
-> exact retirement
```

따라서 `Home 완료 -> PowerOn` 의존성을 만들면 이동형 Homing을 구조적으로 실행할 수 없다.

---

## 4. current source inventory

### 4.1 SDK / protocol

| 영역 | current source | 역할 |
|---|---|---|
| public LMC Home | `LMC_Library/LMC_API_Delivery/src/LmcHome.cs` | typed parameter와 prepare surface |
| axis LMC Home | `LMC_Library/LMC_API_Delivery/src/LmcAxisHome.cs` | start/query/retire axis facade |
| LMC Home admin | `LMC_Library/LMC_API_Delivery/src/LmcAdminHome.cs` | identity, capability, recovery orchestration |
| LMC Home wire | `LMC_Library/LMC_API_Delivery/src/LmcAdminHomeProtocol.cs` | `0x7D13/18/19` serializer/parser |
| DS402 Home API | `LMC_Library/LMC_API_Delivery/src/LmcAxisDs402Home.cs` | method37 axis facade |
| DS402 Home admin | `LMC_Library/LMC_API_Delivery/src/LmcAdminDs402Home*.cs` | start/outcome/retire와 models |
| command IDs | `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` | frozen command mapping |
| wire map | `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` | byte-level contract |

### 4.2 PLC / RT

| 영역 | current source | 역할 |
|---|---|---|
| TCP admission | `Class/TCPMotionInterface/TCPMotionInterface.st` | exact request shape와 specialized ownership reservation |
| LMC Home lifecycle | `Class/LMCControlCommandService/LMCControlCommandService.st` | retained record, start/outcome/retire, preemption, finalization |
| DS402 Home lifecycle | `Class/LMCDiagnosticsService/LMCDiagnosticsService.st` | preflight, retained record, SDO/mode state machine |
| RT execution | `Class/LMCEcatInputLatch/LMCEcatInputLatch.st` | controlword, SetPosition, alignment, drain |

위 표의 `Class/...` 경로 기준 루트는
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis`다.

### 4.3 WPF

`MainWindow.MaintenanceActions.cs`에는 다음 surface가 존재한다.

- LMC Home one-shot confirmation
- `PrepareLMC_Home`와 `LMC_HomeAsync`
- exact `ReadLMC_HomeOutcomeAsync` / `RetireLMC_HomeOutcomeAsync`
- DS402 Home method 37 selector
- Start 전 durable recovery journal
- uncertain transport 이후 original Start replay 금지
- reconnect/restart 후 query/retire-only recovery

따라서 UI도 신규 screen을 만드는 것이 아니라 capability/axis eligibility와 runtime evidence를
정합화하는 것이 우선이다.

---

## 5. current HEAD에서 확인된 구현 차단점

### 5.1 physical topology가 1축인데 Home 관련 상수는 2축이다

current generated EtherCAT/Motion table은 다음 상태다.

- `GL_9086_11.SlaveIndex = 0`
- `Elmo_11.SlaveIndex = 1`
- `Elmo_21`, `Elmo_31`, `Elmo_41` deactivated
- Axis1 `_LMCAxis1.SimulateMode` initial/configured value = 0
- Axis2 `_LMCAxis2.SimulateMode` initial value는 0이지만
  `SimulationSetup1.Axis_2 = 1`이 first scan에서 적용되는 구성
- Axis3..9 `SimulationSetup1.Axis_3..9 = 1`

따라서 Axis2..9 simulation은 source configuration intent이며 PLC runtime readback 증거는 아니다.
또한 generated Motion table에는 deactivated `Elmo_21`을 향하는 일부 optional connection이 남아
있으므로 H-CFG에서 connection과 startup readiness 영향도 함께 확인해야 한다.

반면 Home/ownership source에는 다음 값이 남아 있다.

| 파일 | current value | 1-axis target |
|---|---:|---:|
| `LMCEcatInputLatch.st` | `LMC_CONFIGURED_PHYSICAL_DRIVE_MASK = 0x00000003` | `0x00000001` |
| `LMCControlCommandService.st` | `LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK = 0x00000003` | `0x00000001` |
| `LMCDiagnosticsService.st` | `LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK = 0x00000003` | `0x00000001` |
| Admin capability response | `PhysicalAxisCount = 2` | `1` |

이 상태에서는 Axis2가 simulated/deactivated인데도 Home-capable physical axis처럼 보일 수 있다.
특히 SDK의 Admin precondition은 `axisReference <= PhysicalAxisCount`를 사용하므로 잘못된 count는
PC 단계에서 Axis2 요청을 통과시킨다.

### 5.2 기존 문서의 five-value activation 규칙이 current source와 충돌한다

과거 설계는 아래 값을 모두 ON으로 묶었다.

- TCP ordinary ownership gate
- Control ordinary ownership gate
- DS402 Home runtime gate
- startup sweep gate
- Admin capability bit

current HEAD에서는 두 ordinary gate가 의도적으로 FALSE다.

- `TCPMotionInterface.st`: `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE`
- `LMCControlCommandService.st`: `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE`

이 값은 일반 Axis/Group native command를 retained ownership adapter로 분류할지 결정한다.
`LMC_Home`과 `DS402Home`의 specialized reservation은 별도 경로다.

- `LMC_Home 0x7D13`: ordinary classifier 앞에서 OwnerKind 3, ResourceKind 2,
  AdmissionMode lifecycle로 직접 예약
- `DS402Home 0x7D15`: diagnostics preflight 성공 뒤 OwnerKind 4, ResourceKind 3,
  AdmissionMode lifecycle로 직접 예약

따라서 Home 구현을 위해 global ordinary ownership gate를 다시 TRUE로 만들면 안 된다. 그렇게
하면 이미 정상화된 일반 Power/Move command admission을 다시 변경하는 별도 회귀가 생긴다.

### 5.3 capability가 runtime readiness보다 앞서 있다

current Admin feature mask `0x00000757`은 다음을 advertise한다.

- bit 4: AxisHome
- bit 6: AxisDs402Home

동시에 runtime gates도 TRUE다.

- `LMC_ADMIN_AXIS_HOME_ENABLED TRUE`
- `LMC_DIAG_DS402_HOME_ENABLED TRUE`
- `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED TRUE`

하지만 physical mask/count가 current hardware와 불일치하고 fresh C78/download/runtime proof가
없다. 따라서 source에서 capability bit가 ON이라는 사실은 현재 PLC에서 기능이 Active라는
증거가 아니다.

### 5.4 identity namespace를 섞으면 안 된다

Admin/diagnostics mutation identity의 `MapRevision`은 `0x957F101E`이고 topology inventory
revision은 `0x96FC461C`이다. 두 값은 역할이 다르다.

- `0x957F101E`: durable mutation recovery key와 protocol map identity
- `0x96FC461C`: configured EtherCAT/CREVIS topology inventory identity

구현 중 topology revision을 mutation `MapRevision`으로 대체하지 않는다. 다만 one-drive
topology 변경이 mutation ABI에 영향을 주면 `MapRevision`을 올리고 C#, PLC, journal/tests를
한 changeset에서 함께 갱신한다.

### 5.5 LMC Home EvidenceFlags 문서가 source와 불일치했다

current PLC와 SDK의 성공 계약은 `0x0000003B`이다.

- `LMCEcatInputLatch.st`: `LMC_ZERO_HOME_EVIDENCE_COMPLETE = 0x0000003B`
- `LMCControlCommandService.st`: `LMC_HOME_EVIDENCE_COMPLETE = 0x0000003B`
- `LmcAdminHomeModels.cs`: `RequiredEvidenceFlags = 0x0000003B`

기존 `DINT_PACKET_MAP.txt`의 `0x3F` 문구는 잘못된 문서 값이므로 이 설계 보완과 함께
`0x3B`로 정정한다. wire offset은 바뀌지 않는다. 구현자는 bit 2가 비어 있는 현재 정의를
임의로 채우지 않고 exact `0x3B`를 성공 조건으로 사용한다.

---

## 6. 목표 구조

### 6.1 physical eligibility 단일화

Home 관련 세 class와 Admin response가 서로 다른 literal을 보유하지 않도록 하나의 승인된
configured physical axis 계약을 사용한다. LASAL 구조상 실제 shared constant를 만들기 어렵다면
동일 literal과 static verifier를 사용하되 의미와 변경 절차는 하나로 관리한다.

2026-09-11 target은 다음이다.

```text
ConfiguredPhysicalAxisMask = 0x00000001
PhysicalAxisCount          = 1
Home-capable physical axis = Axis1 only
Axis2..Axis4               = deterministic unavailable
Axis5..Axis9               = outside current Home wire range
```

모든 Start handler는 retained record 또는 RT mailbox를 변경하기 전에 다음 predicate를
통과해야 한다.

```text
Reference in supported wire range
AND AxisMask intersects ConfiguredPhysicalAxisMask
AND axis/drive clients connected
AND current subsystem gate enabled
AND identity/build/BootId/MapRevision exact
```

Axis2 요청은 durable journal, owner reservation, SDO write, controlword write, SetPosition 호출
없이 명시적으로 unavailable을 반환해야 한다.

### 6.2 Servo On과 Home 분리

Servo Power admission에는 다음 조건을 넣지 않는다.

- Home/Referenced bit TRUE
- LMC Home terminal record 존재
- DS402 Home terminal record 존재

Servo Power는 drive/axis connected, hardware error, DS402 state, safety/ownership conflict 등
Power command 자체의 조건으로 판단한다. Home/Referenced FALSE는 상태 표시일 뿐 PowerOn
거부 사유가 아니다.

Home Start는 반대로 active motion/ownership과 충돌해야 한다. qualification 초기 정책은
`PowerOff + Standstill + no axis error`에서 non-moving Home을 수행하는 것으로 고정한다.
이는 안전한 commissioning 정책이지 Servo On의 일반 선행조건이 아니다. 나중에 drive vendor
proof를 확보해 method37을 PowerOn 상태에서도 허용하려면 별도 requirement와 failure matrix로
변경한다.

### 6.3 specialized ownership 유지

두 Home은 서로 다른 retained engine을 사용하지만 같은 physical axis를 동시에 변경할 수 없다.

```text
LMC_Home owner      : OwnerKind 3 / ResourceKind 2 / lifecycle
DS402Home owner     : OwnerKind 4 / ResourceKind 3 / lifecycle
HomeDS402Ex owner   : separate approved profile, DS402 Home resource mutually exclusive
```

Start가 accepted 된 후 owner는 terminal record와 cleanup receipt가 확정될 때까지 release하지
않는다. safety PowerOff/Stop이 preempt한 경우 cancellation/drain 결과가 불확실하면 quarantine하고
사용자가 flag를 강제 clear하지 않는다.

### 6.4 current activation set

과거 five-value 묶음을 다음 feature-specific set으로 교체한다.

| 항목 | LMC Home | DS402Home method37 |
|---|---:|---:|
| configured physical mask | Axis1 | Axis1 |
| Admin `PhysicalAxisCount` | 1 | 1 |
| Admin capability | bit 4 | bit 6 |
| feature runtime gate | `LMC_ADMIN_AXIS_HOME_ENABLED` | `LMC_DIAG_DS402_HOME_ENABLED` |
| RT startup/drain gate | ZeroHome startup integrity | `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED` |
| specialized reservation | OwnerKind 3 / Resource 2 | OwnerKind 4 / Resource 3 |
| global ordinary gate | 관계없음, FALSE 유지 | 관계없음, FALSE 유지 |

feature bit만 ON이고 mask/count/runtime gate가 다르면 activation failure다. 반대로 source set이
모두 맞아도 fresh build/download/Axis1 proof 전에는 production Active가 아니다.

### 6.5 deterministic nonphysical rejection

PLC Start endpoint는 owner 또는 retained state를 만들기 전에 다음과 같이 판정한다.

```text
axisMask = 1 << (Reference - 1)
if Reference outside 1..4:
    reject detail 4
else if (axisMask AND ConfiguredPhysicalAxisMask) = 0:
    reject detail 4 before reservation/journal/mailbox
else:
    continue exact identity and ownership admission
```

current schema에서는 detail 4 `InvalidReference`를 physical-unavailable axis에도 사용한다.
별도 `PhysicalAxisUnavailable` detail을 추가하려면 ErrorCatalogVersion, C# enum/parser/tests와 PLC
response를 함께 변경해야 하므로 H-CFG의 필수 범위에는 넣지 않는다.

---

## 7. frozen wire와 recovery 계약

### 7.1 `LMC_Home`

#### Start `0x7D13`

- request payload: 56 bytes
- Reference: Axis1..4 wire shape, runtime physical eligibility 별도 적용
- DiagnosticsBuild, BootId, MapRevision exact
- nonzero 128-bit ClientIntentId
- semantic: `CurrentPositionZero = 1`
- expected actual position: preflight에서 읽은 exact CAS 값
- target position: 0
- timeout: 100..5000 ms
- execute token: `HOME`

#### Outcome `0x7D18`

- request payload: 56 bytes
- success response payload: 144 bytes
- exact original identity와 retained terminal generation
- before/after raw/application/internal/set/destination/master positions
- EvidenceFlags exact `0x0000003B`와 native call count 포함

#### Retire `0x7D19`

- request payload: 60 bytes
- `0x7D18` exact key + nonzero generation
- exact retry idempotent

### 7.2 `DS402Home`

#### Start `0x7D15`

- request payload: 72 bytes
- HomingMethod = 37
- Home offset/velocity/acceleration/distance/torque = 0
- BufferMode = Aborting
- nonzero timeout
- execute token: `H402`

#### Outcome `0x7D16`

- request payload: 44 bytes
- success response payload: 92 bytes
- exact identity, terminal state, statusword, actual position, cycles, native state,
  generation 포함

#### Retire `0x7D17`

- request payload: 48 bytes
- exact identity + nonzero generation
- success response payload: 92 bytes
- exact retry idempotent

### 7.3 공통 no-replay 규칙

다음 경계 이후 original Start를 다시 전송하지 않는다.

- TCP write가 일부라도 수행됐을 가능성이 있음
- Start ACK 수신 전에 timeout/disconnect
- PLC가 accepted 했지만 PC가 ACK를 잃음
- WPF/process가 accepted 이후 종료됨

복구 순서는 항상 다음과 같다.

```text
fresh connection
-> diagnostics identity 확인
-> exact recovery key로 Outcome query
-> Running이면 poll only
-> terminal이면 결과 보존
-> exact generation Retire
-> local durable journal resolve
```

BootId 또는 MapRevision이 다르면 이전 operation 부재를 추정하지 않는다. stale record로 격리해
operator-visible하게 유지하고 original Home을 replay하지 않는다.

---

## 8. PLC 상태 머신

### 8.1 `LMC_Home`

```text
Validate request + identity + Axis1 eligibility
-> reserve specialized owner
-> publish retained RESERVED/RUNNING identity
-> exact actual-position stale guard
-> submit AxisZeroHome RT mailbox once
-> LMCAxis.SetPosition(CurrentPositionZero) once
-> read all position domains and Standstill
-> require stable verification samples
-> publish terminal result
-> finalize owner and cleanup receipt
-> preserve terminal until exact retire
```

실패 시 native call을 반복하지 않는다. terminal 이전에 cancellation 결과, mailbox sequence 또는
owner identity가 불확실하면 success로 만들지 않고 failure/quarantine으로 닫는다.

### 8.2 `DS402Home` method 37

```text
Validate request + identity + Axis1 eligibility
-> publish pre-reservation intent
-> reserve specialized owner
-> promote exact intent to durable RESERVED
-> read 0x6061 baseline
-> write 0x607C = 0
-> write 0x6098 = 37
-> acquire RT control owner
-> write 0x6060 = 6 and verify 0x6061 = 6
-> raise controlword bit4 once
-> verify attained + no error + method-37 completion status + ActualPosition 0 +/- 1 count
-> lower bit4
-> align LASAL setpoint
-> write 0x6060 = 8 and verify 0x6061 = 8
-> release RT control owner
-> fresh post-release ActualPosition 0 +/- 1 count
-> publish terminal and preserve until exact retire
```

실패 cleanup은 최소한 controlword bit4 low, RT owner release, mode restore attempt와 결과 기록을
포함한다. cleanup은 최대 5초의 별도 bounded window를 사용하며 최초 실패 원인을 cleanup의
후속 오류로 덮어쓰지 않는다. cleanup이 증명되지 않으면 새 Home을 받지 않고 quarantine한다.

---

## 9. 파일별 구현 작업

### H-CFG: current one-axis topology 정합

| 파일 | 변경 |
|---|---|
| `LMCEcatInputLatch.st` | physical drive mask를 Axis1로 정합; startup sweep가 deactivated Axis2를 요구하지 않게 함 |
| `LMCControlCommandService.st` | configured physical axis mask와 Admin `PhysicalAxisCount`를 1로 정합 |
| `LMCDiagnosticsService.st` | DS402 Home physical drive mask와 topology inventory를 current generated table과 대조 |
| Admin capability response | AxisHome/AxisDs402Home bit와 physical count가 동일 image에서 일치하도록 함 |
| topology/static verifier | Axis1 accept, Axis2 deterministic reject, masks/count 일치 검사 추가 |

세 mask 중 하나만 바꾸는 부분 수정은 금지한다.

Admin feature bits 4/6, feature-specific runtime gates와 startup gate는 함께 검토하되 global
`LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` 두 값은 FALSE로 유지한다.

### H-LMC: LMC Home completion

1. `0x7D13` specialized reservation 전에 Axis1 physical eligibility를 확인한다.
2. `HandleAxisZeroHomeStart` 내부에서도 같은 predicate를 재검증한다.
3. Axis2..4가 retained `ZeroHomeState`, owner 또는 RT mailbox를 변경하지 않는지 검사한다.
4. Home/Referenced bit를 Start precondition으로 사용하지 않는다.
5. active motion, existing owner, axis error, non-standstill은 deterministic reject한다.
6. exact expected-position CAS와 one native `SetPosition` 규칙을 유지한다.
7. terminal result의 EvidenceFlags exact `0x0000003B`와 all-position-domain zero를 검증한다.

### H-37: DS402 Home method37 completion

1. diagnostics preflight에서 Axis1 physical eligibility를 확인한다.
2. unavailable Axis2는 intent stage 87도 남기지 않고 reject한다.
3. `0x607C`, `0x6098`, `0x6060` write/readback 순서를 유지한다.
4. controlword bit4는 exact request당 한 번만 rising edge를 만든다.
5. attained/no-error와 method-37 completion status를 fresh stable sample로 검증한다.
   Switch On Disabled에서는 target reached를 강제하지 않으며 ActualPosition은 raw count
   기준 0 +/- 1만 허용한다.
6. bit4 low, CSP mode 8, setpoint alignment, RT owner release를 terminal 전에 확인한다.
7. startup sweep가 Axis1만 drain하고 deactivated Axis2 때문에 quarantine하지 않게 한다.

### H-PC: SDK/WPF 정합

1. Admin `PhysicalAxisCount=1`을 받아 Axis2 Home prepare가 PC에서 거부되는지 확인한다.
2. feature bit가 OFF면 UI는 Start를 arm하지 않는다.
3. capability refresh 전 cached 상태로 Start하지 않는다.
4. LMC Home과 DS402 Home journal key에 build/BootId/MapRevision/axis/intent가 모두 보존되는지 확인한다.
5. lost ACK/reconnect/restart tests에서 Start frame count가 1을 넘지 않는지 확인한다.

### H-DOC: 문서 정합

다음 문서의 historical two-axis/five-value activation 표현을 current 문서로 override한다.

- `HOME_DS402_DESIGN.md`
- `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
- `HOME_DS402_H37_OPERATOR_ACTIVATION_IMPLEMENTATION_20260902.md`
- `DINT_PACKET_MAP.txt`의 오래된 "gate OFF" 설명
- `docs/api/design/README.md`의 two-drive baseline

wire layout 자체는 변경하지 않는다.

---

## 10. 오류와 경계 조건

| 조건 | 기대 결과 | 금지 동작 |
|---|---|---|
| Home/Referenced = FALSE, PowerOn 요청 | Power 자체 조건이 정상이면 허용 | Home 미완료 이유로 거부 |
| Axis2 Home 요청 | physical unavailable | owner/journal/SDO/native mutation |
| axis moving | Home Start reject | motion 중 좌표 재정의 |
| existing retained terminal | exact Outcome/Retire 요구 | 새 Start로 덮어쓰기 |
| existing active owner | ownership conflict | flag force-clear |
| BootId mismatch | stale identity/quarantine | operation 미실행 추정 또는 replay |
| startup drain incomplete | unavailable/quarantine | capability-ready 판정 |
| Start ACK lost | Outcome query only | original Start retransmission |
| safety PowerOff preemption | cancel/drain/final outcome | Home success 추정 |
| mode 8 restore 실패 | terminal failure/quarantine | DS402 Home success |

---

## 11. 검증 계획

### H-S0: source/static

- current generated EtherCAT table: CREVIS index 0, Elmo Axis1 index 1
- Axis1 non-simulation, `SimulationSetup1.Axis_2..9 = 1`, first-scan 적용 구조
- three physical masks = `0x00000001`
- Admin `PhysicalAxisCount = 1`
- AxisHome bit 4와 AxisDs402Home bit 6 의도 확인
- specialized reservation이 ordinary gate FALSE에서도 연결됨
- Home/Referenced predicate가 Servo Power admission에 없음
- `git diff --check`

이 단계 PASS는 PLC 또는 물리축 PASS가 아니다.

### H-S1: PC protocol/tests

- exact request/response byte lengths와 offsets
- LMC Home successful EvidenceFlags exact `0x0000003B`
- malformed/mismatched identity rejection
- Axis2 capability precondition rejection
- Start one-shot/no-replay
- Outcome parser terminal integrity
- exact generation retire와 idempotent retry
- WPF durable journal restart/reconnect recovery

이 단계 PASS는 LASAL compile/download 증거가 아니다.

### H-C78: LASAL IDE/image

- current tracked `.st`와 IDE implementation 동일성
- Compile/Rebuild 0 errors
- 새 C78/generated artifact identity 기록
- `%TEMP%\Lasal2.log`에 smoke 시작 이후 새 `CInvalidArgException` 없음
- exact PLC download 완료
- download 전/후 BootId와 executable/image identity 기록

### H-RUN-LMC: Axis1 `LMC_Home`

1. Servo Off, Standstill, Home/Referenced FALSE 상태 확보
2. nonzero current position에서 Read Status/Position 캡처
3. `0x7D13` exactly once
4. `0x7D18` terminal까지 poll
5. physical encoder raw position이 이동하지 않았는지 별도 확인
6. LASAL position domains가 0으로 정렬됐는지 확인
7. `0x7D19` exact retire와 retry idempotence 확인
8. 이후 Home/Referenced state와 Servo On 가능 여부를 별도로 확인

### H-RUN-37: Axis1 `DS402Home`

1. Servo Off, Standstill, Home/Referenced FALSE 상태 확보
2. baseline `0x6060/0x6061/0x607C/0x6098`, Statusword, ActualPosition 캡처
3. `0x7D15` exactly once
4. SDO/mode/controlword sequence packet 또는 PLC trace 캡처
5. `0x7D16` terminal과 attained/no-error/method-37 completion/position 0 +/- 1 count 확인
6. bit4 low, mode 8, setpoint alignment, owner release 확인
7. `0x7D17` exact retire와 retry idempotence 확인
8. 실제 축 무이동 여부를 독립적으로 확인

### H-RUN-NEG: 실패 matrix

- Axis2 unavailable and zero mutation
- active motion 중 Start
- duplicate ClientIntentId with changed payload
- stale expected actual position
- existing unretired terminal
- concurrent LMC Home vs DS402 Home
- Stop/PowerOff safety preemption
- disconnect before ACK, after ACK, during Running, before Retire ACK
- PLC reboot/BootId change
- mode 6 진입 실패
- bit4 low 복구 실패
- mode 8 restore 실패
- startup sweep/drain 실패

각 결과는 `PC`, `wire`, `PLC retained state`, `drive/physical` evidence를 분리해 기록한다.

---

## 12. 완료 기준

다음을 모두 만족해야 해당 기능을 Active로 판정한다.

- [ ] current one-axis physical masks/count/capability가 한 image에서 일치
- [ ] Axis1만 Home-capable이고 Axis2는 mutation 없이 unavailable
- [ ] Servo On이 Home/Referenced FALSE에서 Home 조건 때문에 거부되지 않음
- [ ] ordinary ownership gates를 재활성화하지 않고 specialized Home reservation 동작
- [ ] SDK Debug/Release relevant tests PASS
- [ ] WPF no-replay/recovery tests PASS
- [ ] LASAL Compile/Rebuild 0 errors
- [ ] current C78/generated artifact와 source identity 보존
- [ ] exact PLC download와 fresh BootId 증거
- [ ] Axis1 LMC Home 정상/실패 matrix PASS
- [ ] Axis1 DS402Home method37 정상/실패 matrix PASS
- [ ] Start frame exactly once와 Outcome/Retire lifecycle packet proof
- [ ] physical movement/no-movement를 PLC status와 별도로 확인
- [ ] cleanup/quarantine/reboot recovery matrix PASS

하나라도 없으면 판정은 다음처럼 분리한다.

```text
source implemented
PC/static qualified
LASAL build qualified
PLC image qualified
Axis1 runtime qualified
physical behavior qualified
production Active
```

앞 단계 PASS를 뒤 단계 PASS로 승격하지 않는다.

---

## 13. 구현 순서

```text
H-CFG one-axis topology/mask/count 정합
-> H-S0 source/static
-> H-LMC physical eligibility와 Servo/Home 분리 확인
-> H-37 method37 eligibility/startup sweep 정합
-> H-PC capability/no-replay tests
-> H-C78 LASAL build/generated artifact/download
-> H-RUN-LMC Axis1 qualification
-> H-RUN-37 Axis1 qualification
-> H-RUN-NEG failure/recovery matrix
-> 문서/API progress 갱신
```

첫 구현 changeset은 H-CFG와 deterministic Axis2 rejection까지만 포함하는 것이 안전하다.
HomeDS402Ex moving runtime, arbitrary method, switch wiring과 Servo Power 정책 변경을 같은
changeset에 섞지 않는다.

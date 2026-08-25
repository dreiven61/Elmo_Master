# HomeDS402Ex 최우선 개발 설계

- 대상: No.22 `MMC_HomeDS402ExCmd`
- 현재 진행 상태: SDK wire/lifecycle + WPF durable no-replay recovery qualification 완료, LASAL runtime 미구현
- current C#: frozen wire contract, strict Start/Outcome/Retire parser, one-shot lifecycle facade, read-only recovery rehydrate 구현
- current WPF: pre-dispatch durable journal, startup interlock, read-only exact-key recovery와 exact-generation retire 구현; Start UI는 닫힘
- 신규 command: `0x7D1B Start`, `0x7D1C ReadOutcome`, `0x7D1D Retire`
- activation: independent Admin feature bit 11, PLC advertisement/current runtime OFF

## 1. 목적과 분리 원칙

`HomeDS402Ex`는 method 37 전용 `0x7D15/16/17`을 확장하지 않는다. 별도 command,
recovery key, outcome record와 capability를 가진다. 두 API는 같은 축의 DS402 Home engine과
SDO executor resource를 공유해 동시에 실행되지 않게 한다.

current C#에는 `LMCAxisDs402HomeExParameters` 입력 model에 더해 frozen DINT execution plan,
`0x7D1B/1C/1D` wire contract, strict response parser, one-shot Start/Outcome/Retire facade와
read-only durable recovery-key rehydrate가 구현되어 있다. engineering-unit public Prepare는
axis별 scale/profile 승인 전까지 의도적으로 닫혀 있다. WPF에는 Start를 재구성하지 않는
pre-dispatch durable journal과 startup interlock, exact-key outcome query 및 exact-generation retire
recovery만 구현되어 있다. LASAL route/state/store는 아직 구현하지 않았다.

## 2. v1 범위

- physical axis 1..4
- Distributed mode, stable Standstill와 PowerOn 상태
- axis별 승인된 standard DS402 Homing method allowlist
- application unit를 checked DINT drive/application value로 변환한 frozen plan
- Aborting execution만
- start/outcome/retire와 no-auto-replay recovery
- 완료 뒤 CSP 8, setpoint alignment와 owner release

Gold block-search method `-4..-1`, reserved `15/16/31/32`, obsolete 35와 unknown method는
v1에서 거부한다. current candidate ranges `1..14`, `17..30`, `33..34`도 axis profile에서
승인된 method만 실제 Start할 수 있다.

Standard method에서 vendor-specific DetectionVelocityLimit, DistanceLimit와 TorqueLimit이
사용되지 않으면 0만 허용한다. 이를 사용하는 method는 object mapping, scale와 fault behavior가
별도로 승인된 뒤 활성화한다.

## 3. 단위와 profile revision

public model의 `double/float`를 PLC에서 임의로 IEEE 재해석하지 않는다. SDK의 Prepare 단계가
axis별 scale, rounding, min/max와 overflow를 적용해 wire용 DINT plan을 만든다.

- position: application position units
- velocity: application units/s
- acceleration: application units/s^2
- torque: axis profile에서 정의한 signed scale
- DINT 최소값의 부호 반전과 모든 overflow를 Start 전에 거부
- Maestro 완료 위치 의미는 `ActualPosition == -Position`
- scale/allowlist 변경은 Diagnostics MapRevision과 paired update한다.

current SDK frozen-plan constructor는 `Position == Int32.MinValue`를 Start intent 생성 전에
거부한다. engineering-unit scale/rounding/range 자체는 HOMEEX-01/02 승인 전까지 public Prepare로
노출하지 않는다.

scale과 wiring은 코드에 숨은 상수로 두지 않고 axis homing profile 표로 관리한다. profile은
Home switch, positive/negative limit, index/block source, active level, debounce, travel direction,
max travel, torque range와 method mask를 포함한다.

## 4. wire contract

`HOMEEX-03` SDK golden-byte test에서 아래 layout을 고정했다. PLC/LASAL route는 아직 OFF이며
packet map/LASAL paired activation은 별도 단계로 진행한다.

### Start `0x7D1B`, 116 bytes

| Offset | Type | Field |
|---:|---|---|
| P8/P12/P16 | U32 | Expected build/BootId/MapRevision |
| P20..P32 | 4 x U32 | ClientIntentId, 128-bit 전체가 0이면 거부 |
| P36 | I32 | HomingMethod |
| P40 | I32 | Position |
| P44 | I32 | DetectionVelocityLimit |
| P48 | I32 | Acceleration |
| P52 | I32 | VelocityHigh |
| P56 | I32 | VelocityLow |
| P60 | I32 | DistanceLimit |
| P64 | I32 | TorqueLimit |
| P68 | U16 | BufferMode = Aborting |
| P70 | U16 | Reserved = 0 |
| P72 | U32 | OverallTimeoutMilliseconds |
| P76 | U32 | DetectionTimeoutMilliseconds |
| P80..P111 | 32 bytes | Spare, 모두 0 |
| P112 | U32 | ExecuteToken `0x58453448` (`H4EX`) |

Start ACK는 24 bytes이며 echoed method와 NativeCommandState만 반환한다. well-shaped Start의
domain success/failure는 24 bytes이고 malformed common-frame failure는 16 bytes다. ACK는
terminal 완료가 아니다.

### ReadOutcome/Retire

복구 key는 collision 가능한 짧은 hash가 아니라 다음 full exact tuple을 사용한다.

`build + BootId + map + original RequestId + ClientIntentId[4] + axis + 모든 변환된 실행 parameter`

- `0x7D1C` request: 116 bytes. Start의 P112 ExecuteToken 자리를 없애고 P20에
  OriginalRequestId를 넣은 뒤 나머지 exact parameter를 4 bytes 뒤로 이동한다.
- `0x7D1D` request: full key + P116 nonzero generation, 120 bytes
- query/retire는 Home을 실행하거나 parameter SDO를 다시 쓰지 않는다.

| Query offset | Type | Field |
|---:|---|---|
| P8/P12/P16 | U32 | Expected build/BootId/MapRevision |
| P20 | U32 | Original Start RequestId |
| P24..P36 | 4 x U32 | ClientIntentId |
| P40 | I32 | HomingMethod |
| P44..P71 | 7 x I32 | Position부터 TorqueLimit까지 |
| P72/P74 | U16/U16 | BufferMode / Reserved=0 |
| P76/P80 | U32/U32 | Overall / Detection timeout |
| P84..P115 | 32 bytes | Spare, 모두 0 |
| P116 | U32 | Retire에만 존재하는 ExpectedRecordGeneration |

Query의 common P4는 새 Query RequestId이고 P20이 original Start RequestId다. Start의 constant
ExecuteToken은 recovery key가 아니므로 Query에서 반복하지 않는다.

Outcome success response는 176 bytes로 고정하고 다음 layout을 사용한다.

| Offset | Type | Field |
|---:|---|---|
| P16/P18 | U16/U16 | RecordState / Reserved=0 |
| P20..P32 | 4 x U32 | build/BootId/map/original RequestId |
| P36..P48 | 4 x U32 | ClientIntentId |
| P52/P54 | U16/U16 | AxisReference / Reserved=0 |
| P56..P84 | 8 x I32 | method와 7개 converted parameter |
| P88/P90 | U16/U16 | BufferMode / Reserved=0 |
| P92/P96 | U32/U32 | Overall / Detection timeout |
| P100..P131 | 32 bytes | Spare echo, 모두 0 |
| P132/P134 | U16/I16 | OriginalCommandStatus / OriginalErrorId |
| P136 | U32 | OriginalDetailCode |
| P140/P142 | U16/U16 | DS402StatusWord / Reserved=0 |
| P144/P148 | I32/I32 | ActualPosition / ExpectedFinalPosition |
| P152/P156 | U32/U32 | StartCycle / CompletionCycle |
| P160/P164 | U32/U32 | NativeCommandState / RecordGeneration |
| P168/P172 | U32/U32 | CleanupProofFlags / SdoExecutorToken |

RecordState는 1 Running, 2 Succeeded, 3 Failed, 4 Aborted다. cleanup 불확실/quarantine은
Succeeded response로 만들지 않고 query detail 54 또는 62로 unresolved 상태를 보존한다.

Running은 CompletionCycle=0이다. terminal은 CompletionCycle>=StartCycle과 full key echo가
필수다. Query/Retire domain failure는 16-byte common envelope만 반환한다.

`CleanupProofFlags`는 다음 6개를 safe-terminal 필수 proof로 고정한다.

- start bit low
- CSP 8 복원
- setpoint alignment 완료
- RT owner release
- 임시 homing parameter 복원
- SDO callback/orphan drain 해소

ErrorCatalogVersion 7 detail은 `53 outcome not found`, `54 indeterminate`, `55 store corrupt`,
`56 exact-key mismatch`, `57 storage unavailable`, `58 runtime execution failed`, `59 aborted`,
`60 slot occupied`, `61 invalid profile/method/scale`, `62 cleanup incomplete`로 C# enum과 중앙
error catalog에 등록했다. 공통 owner conflict/quarantine는 기존 41/42를 재사용한다.

## 5. 상태 머신

```text
Validate exact request/profile/wiring
  -> Reserve shared DS402 Home resource
  -> Stable Standstill/Power/Fault/position preflight
  -> Snapshot current mode and homing parameters
  -> Program approved homing parameters
  -> Acquire RT control owner
  -> 6060=6 write and 6061=6 verification
  -> Raise controlword bit 4
  -> Observe method-specific detection/travel/torque/watchdogs
  -> Terminal motion evidence on 3 fresh cycles
  -> Lower bit 4
  -> Restore temporary parameters and CSP 8
  -> Align setpoint and release RT owner
  -> Fresh ActualPosition == -Position proof
  -> Commit outcome/readback
  -> Release shared owner and response
```

overall timeout와 detection timeout은 SDO timeout과 분리한다. service time은 RT latch cycle이
멈춰도 진행해야 한다. travel/torque/detection 한계를 넘으면 start bit를 내리고 cleanup으로
가되, cleanup proof가 불확실하면 terminal safe failure가 아니라 quarantine이다.

## 6. ownership과 복구

- HomeDS402와 HomeDS402Ex는 같은 physical axis와 DS402 Home engine resource를 상호 배제한다.
- SetOpMode, SetPosition, motion, Power/Stop/Reset, encoder maintenance와 generic SDO도 충돌한다.
- write dispatch 뒤 disconnect 시 original Start를 replay하지 않는다.
- same-BootId recovery는 ReadOutcome만 사용한다.
- BootId가 바뀐 unresolved journal은 자동 해결하지 않고 operator recovery로 남긴다.
- terminal record는 exact generation retire 뒤에만 다음 Home을 허용한다.
- current ordinary/warm memory를 cold-durable store로 주장하지 않는다.

SDK는 Start write-boundary 뒤 응답 유실 시 prepared command를 consumed 상태로 유지하고 session을
invalid/faulted 처리한다. persisted full key는 `LMCAxisDs402HomeExRecovery.Rehydrate`로 다시 만들 수
있지만 이 key로 Start prepared command를 생성할 수는 없다. 재접속 뒤 Query는 exact same
Build/BootId/MapRevision과 current capability observation을 요구한다. Retire 응답 유실 뒤에는
같은 recovery key와 같은 nonzero record generation으로만 exact retry할 수 있다.

WPF durable recovery journal은 future Start integration이 write boundary를 넘기 전에 exact intent를
먼저 durable arm하도록 API를 제공한다. startup에서 unresolved `ArmedBeforeDispatch`는
`RecoveryRequired`로 승격하고 일반 mutation UI를 interlock한다. journal은 Start sender를 갖지
않으며, recovery panel도 capability refresh와 exact `0x7D1C` query / `0x7D1D` retire만 제공한다.
terminal outcome은 durable proof로 먼저 기록한 뒤 exact-generation retirement가 성공해야만
Resolved가 된다. BootId를 포함한 recovery identity가 달라지면 자동 복구/해제를 하지 않는다.

## 7. capability

Admin feature bit 11 `AxisDs402HomeEx`를 C# protocol에 고정했다. 이 한 bit는
Start/ReadOutcome/Retire와 ErrorCatalogVersion 7 전체를 indivisible하게 의미한다.
SetOpMode가 예약한 bits 8..10과 충돌하지 않는다. SDK parser는 bit 11 + catalog 7 미만 조합,
physical axis count 범위 위반과 unknown feature bit를 fail-closed한다.

PLC/LASAL capability advertisement와 runtime route는 physical qualification 전까지 OFF다.

## 8. 변경 대상과 current SDK implementation

### C# 구현됨

- `LmcAdminDs402HomeExProtocol.cs`
- `LmcAdminDs402HomeExOutcomeProtocol.cs`
- `LmcAdminDs402HomeExLifecycleProtocol.cs`
- `LmcAdminDs402HomeExWireModels.cs`
- `LmcAdminDs402HomeExOutcomeModels.cs`
- `LmcAdminDs402HomeExLifecycleModels.cs`
- `LmcAdminDs402HomeExRecovery.cs`
- `LmcAdminDs402HomeEx.cs`
- `LmcAxisDs402HomeEx.cs`
- `LmcProtocol.cs`, `LmcAdminModels.cs`, `LmcAdminProtocol.cs`
- `LmcErrorCatalog.cs`, response payload limits
- golden/parser/capability/recovery/lifecycle/public-surface/retire-retry contract tests

### C# 의도적으로 미개방

- public engineering-unit `PrepareDs402HomeEx`
- arbitrary raw DINT execution-plan construction

두 surface는 HOMEEX-01/02 axis profile, scale, rounding, range, wiring 승인이 끝난 뒤에만 연다.

### LASAL 미구현

- `TCPMotionInterface.st`: route와 two-phase owner admission
- `LMCDiagnosticsService.st`: independent handler/state/outcome record
- `LMCEcatInputLatch.st`: Ex RT mailbox 또는 shared Home mailbox versioning
- `LMCControlCommandService.st`: shared resource conflict/preemption
- `Verify-LasalContract.ps1`: dormant/active atomic gate와 mutation fixtures

### WPF recovery 구현됨

- `AxisDs402HomeExRecoveryJournal.cs`: pre-dispatch durable arm, startup promotion, exact-key persistence, terminal proof와 exact-retirement resolution
- `MainWindow.AxisDs402HomeExRecovery.cs`: startup unresolved-record interlock와 read-only Query/Retire recovery panel
- recovery journal/record에는 Start sender가 없고 recovery key로 Start를 재구성하지 않음
- active recovery 중 ordinary mutation UI는 차단하고 safety/read-only/recovery action만 allowlist
- endpoint + Build/BootId/MapRevision + full recovery key exact match 요구
- terminal outcome/generation을 먼저 durable 저장한 뒤 exact `0x7D1D` 성공 후에만 resolve
- dedicated journal + MainWindow integration smoke tests와 Debug/Release workflow

HomeDS402Ex Start UI와 engineering-unit confirmation surface는 HOMEEX-13 paired activation 전까지
의도적으로 닫혀 있다.

## 9. 작업 체크리스트

- [ ] `HOMEEX-01` 축 1~4 wiring, active level/debounce와 method allowlist 승인
- [ ] `HOMEEX-02` scale/rounding/range/overflow와 MapRevision profile 승인
- [x] `HOMEEX-03` `0x7D1B/1C/1D` exact offsets, full recovery key와 SDK capability bit 고정
- [x] `HOMEEX-04` C# approved-plan Prepare/one-shot Start/Outcome/Retire, capability-off zero-wire와 public raw-plan gate 구현
- [ ] `HOMEEX-05` golden bytes, malformed, overflow, duplicate intent와 disconnect test 구현
  - SDK golden/malformed/overflow/start-response-loss/reconnect-read-only/exact-retire-retry/public-surface tests는 PASS
  - duplicate-intent retained-store behavior는 LASAL outcome store가 없으므로 아직 미검증
- [ ] `HOMEEX-06` LASAL parser/state/outcome scaffold를 gate OFF로 구현
- [ ] `HOMEEX-07` shared Home/mode/SDO/axis ownership과 startup reconciliation 구현
- [ ] `HOMEEX-08` parameter snapshot/program/restore와 CleanupProofFlags 구현
- [ ] `HOMEEX-09` SourceOnly, method-size, C78와 generated artifact PASS
- [ ] `HOMEEX-10` Axis1 normal/timeout/limit/SDO abort/preempt/reconnect/retire matrix PASS
- [ ] `HOMEEX-11` Axis2~4와 승인 method matrix PASS
- [x] `HOMEEX-12` WPF pre-dispatch journal/startup no-replay recovery와 smoke test PASS
- [ ] `HOMEEX-13` capability bit 11과 WPF UI paired activation

## 10. current software qualification evidence

PR #20 SDK qualification rerun on the current code tranche passed:

- Debug build PASS, 0 warnings / 0 errors
- Debug full suite: 1187 / 1187 PASS
- Release build PASS
- Release full suite: 1187 / 1187 PASS
- `git diff --check` PASS
- HomeDS402Ex parameter/wire/parser/capability/recovery/lifecycle/public-surface/exact-retire-retry contracts PASS

An earlier identical-head attempt observed one pre-existing `GroupDisableWait.Observer.ConcurrentResumeIsZeroWire`
timing/socket failure; the identical job rerun passed the complete Debug and Release suites. No HomeDS402Ex
safety invariant was weakened to address that unrelated flaky test.

HOMEEX-12 WPF recovery qualification on head `0012f67f7b38e633421cf7f9cdf989cc3f6537f5` passed:

- workflow run `32802902270`, job `97667184835`
- Debug WPF smoke build PASS, 0 warnings / 0 errors
- Debug HomeDS402Ex recovery smoke: 11 / 11 PASS
- Release WPF smoke build PASS, 0 warnings / 0 errors
- Release HomeDS402Ex recovery smoke: 11 / 11 PASS
- `git diff --check` PASS
- same-head SetOperationMode WPF recovery workflow `32802902209` PASS

Detailed HOMEEX-12 evidence is recorded in
`docs/api/design/evidence/HOME_DS402_EX_HOMEEX12_WPF_RECOVERY_20260825.md`.

This evidence qualifies the current C# SDK and WPF recovery tranches only. It is not LASAL build/runtime,
EtherCAT packet, hardware or production activation evidence.

## 11. activation 금지 조건

scale, wiring 또는 method allowlist가 비어 있거나, temporary parameter 복원과 CSP 8 복귀가
증명되지 않거나, ActualPosition `-Position` overflow/결과가 불명확하면 capability를 켜지 않는다.
기존 method37 성공을 확장 method 증거로 사용하지 않는다.

# HomeDS402Ex 최우선 개발 설계

- 대상: No.22 `MMC_HomeDS402ExCmd`
- 현재 진행도: 0%
- current 상태: typed input model only, runtime `Missing`
- 신규 command 예약: `0x7D1B Start`, `0x7D1C ReadOutcome`, `0x7D1D Retire`
- activation: independent Admin feature bit 11, current OFF

## 1. 목적과 분리 원칙

`HomeDS402Ex`는 method 37 전용 `0x7D15/16/17`을 확장하지 않는다. 별도 command,
recovery key, outcome record와 capability를 가진다. 두 API는 같은 축의 DS402 Home engine과
SDO executor resource를 공유해 동시에 실행되지 않게 한다.

current C#에는 `LMCAxisDs402HomeExParameters`와 validation test만 있다. public 실행 API,
wire, capability, LASAL route/state/store와 WPF 실행 경로는 없다.

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

scale과 wiring은 코드에 숨은 상수로 두지 않고 axis homing profile 표로 관리한다. profile은
Home switch, positive/negative limit, index/block source, active level, debounce, travel direction,
max travel, torque range와 method mask를 포함한다.

## 4. wire 제안

source 구현 전 `HOMEEX-03` golden-byte test에서 아래 layout을 최종 고정하고 packet map에
승격한다.

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

`build + BootId + map + original RequestId + ClientIntentId[4] + 모든 변환된 실행 parameter`

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

`CleanupProofFlags`는 최소 다음을 포함한다.

- start bit low
- CSP 8 복원
- setpoint alignment 완료
- RT owner release
- 임시 homing parameter 복원
- SDO callback/orphan drain 해소

ErrorCatalogVersion 7 후보 detail은 `53 outcome not found`, `54 indeterminate`, `55 store
corrupt`, `56 exact-key mismatch`, `57 storage unavailable`, `58 runtime execution failed`,
`59 aborted`, `60 slot occupied`, `61 invalid profile/method/scale`, `62 cleanup incomplete`로
예약한다. 공통 owner conflict/quarantine는 기존 41/42를 재사용한다. source 반영 시 packet
map과 C#/LASAL enum을 같은 changeset에서 고정한다.

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

## 7. capability

Admin feature bit 11 `AxisDs402HomeEx`를 예약한다. 이 한 bit는 Start/ReadOutcome/Retire와
current error catalog 전체를 indivisible하게 의미한다. SetOpMode가 예약한 bits 8..10과
충돌하지 않는다. strict SDK와 PLC를 paired 배포하며 physical qualification 전까지 OFF다.

## 8. 변경 대상

### C# 신규

- `LmcAdminDs402HomeExProtocol.cs`
- `LmcAdminDs402HomeExOutcomeProtocol.cs`
- `LmcAdminDs402HomeExOutcomeRetirementProtocol.cs`
- `LmcAdminDs402HomeEx.cs`
- `LmcAxisDs402HomeEx.cs`
- `LmcAxisDs402HomeExOutcomeRetirement.cs`

### C# 수정

- `LmcProtocol.cs`, `LmcAdminModels.cs`, `LmcAdminProtocol.cs`
- `LmcAdminDs402HomeExModels.cs`
- response payload limits, packet map와 golden/parser/recovery tests

### LASAL

- `TCPMotionInterface.st`: route와 two-phase owner admission
- `LMCDiagnosticsService.st`: independent handler/state/outcome record
- `LMCEcatInputLatch.st`: Ex RT mailbox 또는 shared Home mailbox versioning
- `LMCControlCommandService.st`: shared resource conflict/preemption
- `Verify-LasalContract.ps1`: dormant/active atomic gate와 mutation fixtures

### WPF recovery

- 신규 `AxisDs402HomeExRecoveryJournal.cs`
- `MainWindow.xaml`과 `MainWindow.xaml.cs`의 explicit confirmation/interlock
- startup unresolved record는 exact BootId가 같아도 ReadOutcome만 허용하고 Start를 replay하지 않음
- BootId가 바뀌면 operator recovery-required로 유지
- exact terminal과 generation을 journal에 저장한 뒤 retire 성공 후에만 resolve
- 전용 journal unit test와 MainWindow smoke test

## 9. 작업 체크리스트

- [ ] `HOMEEX-01` 축 1~4 wiring, active level/debounce와 method allowlist 승인
- [ ] `HOMEEX-02` scale/rounding/range/overflow와 MapRevision profile 승인
- [ ] `HOMEEX-03` `0x7D1B/1C/1D` exact offsets, full recovery key와 capability bit 고정
- [ ] `HOMEEX-04` C# Prepare/Start/Outcome/Retire와 capability-off zero-wire 구현
- [ ] `HOMEEX-05` golden bytes, malformed, overflow, duplicate intent와 disconnect test 구현
- [ ] `HOMEEX-06` LASAL parser/state/outcome scaffold를 gate OFF로 구현
- [ ] `HOMEEX-07` shared Home/mode/SDO/axis ownership과 startup reconciliation 구현
- [ ] `HOMEEX-08` parameter snapshot/program/restore와 CleanupProofFlags 구현
- [ ] `HOMEEX-09` SourceOnly, method-size, C78와 generated artifact PASS
- [ ] `HOMEEX-10` Axis1 normal/timeout/limit/SDO abort/preempt/reconnect/retire matrix PASS
- [ ] `HOMEEX-11` Axis2~4와 승인 method matrix PASS
- [ ] `HOMEEX-12` WPF pre-dispatch journal/startup no-replay recovery와 smoke test PASS
- [ ] `HOMEEX-13` capability bit 11과 WPF UI paired activation

## 10. activation 금지 조건

scale, wiring 또는 method allowlist가 비어 있거나, temporary parameter 복원과 CSP 8 복귀가
증명되지 않거나, ActualPosition `-Position` overflow/결과가 불명확하면 capability를 켜지 않는다.
기존 method37 성공을 확장 method 증거로 사용하지 않는다.

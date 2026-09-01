# 남은 API 구현 통합 설계 — 2026-09-01

- 기준 branch: `dev`
- 설계 기준 source: `dev@f8eaf42d6ea837d36621aceb949c0409b2a7bf36`
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active** — 본 문서의 신규 구현 대상 아님
- current P0: Generic SDO completion / qualification
- production posture: **NO-GO**

이 문서는 SetOperationMode 완료 이후 ElmoMaster에 남은 기능을 어떤 순서와 경계로 구현할지
정하는 **current implementation master plan**이다. 기능별 frozen wire/세부 의미는 기존 설계문서를
우선하고, 이 문서는 기능 사이의 dependency, 실제 남은 source delta, activation 순서와 완료 gate를
통합한다.

관련 상세 설계:

- `HOME_DS402_DESIGN.md`
- `HOME_DS402_EX_DESIGN.md`
- `SET_POSITION_DESIGN.md`
- `../../architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md`
- `../../architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md` — historical redesign evidence

---

## 1. 구현 전체 원칙

남은 mutation 기능은 아래 계약을 공통으로 유지한다.

1. **semantic owner 우회 금지**
   - drive controlword/mode/setpoint/torque/position 등 전용 owner가 있는 object를 Generic SDO나
     PI Write로 우회하지 않는다.
2. **mutation boundary 이후 automatic replay 0회**
   - Start 또는 write가 target에 도달했는지 불명확하면 같은 mutation을 자동 재전송하지 않는다.
   - recovery는 exact outcome/readback/retire만 사용한다.
3. **current identity 고정**
   - connection session, Build, DiagnosticsBootId, MapRevision과 기능별 TopologyRevision /
     record generation을 mutation 직전에 다시 검증한다.
4. **terminal-before-release**
   - native/SDO/RT mutation 성공만으로 owner를 먼저 풀지 않는다.
   - required terminal proof와 durable record/readback이 완료된 뒤 owner를 release한다.
5. **capability는 실행 계약**
   - source가 존재한다는 이유로 capability bit를 켜지 않는다.
   - PC/source/C78/PLC/hardware gate가 닫힌 기능만 paired activation한다.
6. **generated ABI/hash를 추측하지 않는다**
   - LASAL IDE/CodeGenerator output, generated `.lcb/.lcn`, vendor CRC와 hardware profile은 실제
     evidence 없이 손으로 합성하거나 blind ratchet하지 않는다.
7. **SetOperationMode 완료 계약은 회귀 대상으로만 사용한다**
   - 이후 기능이 `0x6060`, DS402 Home resource, RT owner 또는 SDO executor를 공유하더라도
     SetOperationMode의 one-shot/no-replay/Generic-SDO-6060-deny 계약을 약화하지 않는다.

Generic SDO permanent raw blocklist는 계속 고정한다.

```text
0x6040  Controlword
0x6060  Modes of operation
0x607A  Target position
0x60FF  Target velocity
0x6071  Target torque
0x3204  dedicated semantic owner
0x20FC  dedicated semantic owner
```

---

## 2. 남은 구현 분류와 dependency

| 순서 | 기능 | current 상태 | 실제 남은 성격 | dependency |
|---:|---|---|---|---|
| P0 | Generic SDO | Limited | runtime 재설계보다 physical completion + contention/recovery qualification | 없음 |
| P1-A | HomeDS402 | Dormant | runtime 구현 완료, artifact/hardware/paired activation | issue #32 |
| P1-B | HomeDS402Ex | Dormant | **actual parameter SDO + RT homing runtime 구현 필요** | issue #28 + #35 |
| P1-C | SetPosition | Dormant | **durable backend + RT exactly-once runtime 구현 필요** | issue #44 |
| P2-A | EtherCAT NodeHealth / DigitalIO Read | Dormant | read-owner live qualification + bits 15/16 activation | topology identity |
| P2-B | Digital Output Write | Missing runtime | **LASAL route + RT CAS mailbox + ticket execution 신규 구현** | P2-A |
| P2-C | PI Write | Dormant | writable semantic allowlist + PLC executor/ownership qualification | approved writable signals |
| P2-D | Recorder DoubleBank | Dormant | existing dormant source/WPF recovery qualification + bit 6 activation | RAM/jitter/fault matrix |
| P2-E | Extended SDO Result | Dormant | chunk source + fault matrix + bit 12 activation | D5 ticket lifecycle |
| REL | Distribution / release | Blocked | same-source artifact set + manual/WPF/distribution sync | 위 기능별 승인 |

`SetPosition`의 외부 prerequisite가 없어도 Generic SDO, HomeDS402와 HomeDS402Ex의 독립 tranche는
진행할 수 있다. HomeDS402Ex는 profile 승인 전에는 hardware-dependent object/scale 값을 구현하지
않는다.

권장 실행 순서:

```text
Generic SDO completion
        |
        +--> HomeDS402 artifact/hardware/activation
        |
        +--> HomeDS402Ex profile+artifact -> runtime -> hardware -> activation
        |
        +--> SetPosition vendor/IDE prerequisites -> durable runtime -> hardware -> activation
        |
        v
Read-only diagnostics activation
        -> Digital Output Write
        -> PI Write / Recorder Double / Extended SDO
        -> distribution / production release
```

---

# 3. P0 — Generic SDO completion 설계

## 3.1 current source를 다시 설계하지 않는다

현재 `LMCSdoExecutor`는 Manual Server와 programmatic request를 `RequestSource`와 atomic state로
직렬화하고, SDK는 physical axis 1..4의 canonical 1/2/4-byte scalar Write를 semantic blocklist
방식으로 허용한다. Axis1 `0x2F00:24`는 qualification preset일 뿐 generic address allowlist가
아니다.

따라서 다음 tranche는 새로운 unrestricted SDO engine을 만드는 작업이 아니다. current engine을
**실제 Manual/Programmatic/Failure 조건에서 완료 판정**하고 evidence에서 발견된 결함만 좁게
교정한다.

## 3.2 SDO-Q01 — exact safe-object qualification matrix

각 physical axis에서 승인된 non-semantic object를 시험 전에 별도 기록한다. object를 임의로
선정해 write하지 않는다.

Axis1에서 먼저 다음 shape를 각각 한 건씩 고정한다.

```text
1-byte scalar
2-byte scalar
4-byte scalar
```

각 case의 실행 순서:

```text
fresh capability
-> fresh ReadDriveStatus
-> Standstill=True / Fault=False / OperationEnabled=False
-> exact request preview
-> durable journal ArmBeforeDispatch
-> SubmitSdo exactly once
-> terminal ticket
-> same object/type/width ReadSdoInline
-> exact byte/value readback
-> journal terminal proof
```

PASS 조건:

- accepted write ticket가 exactly one submission에 대응
- target object/index/subindex/type/width가 request와 exact match
- terminal success 뒤 exact readback match
- semantic blocklist object는 zero-wire local/PLC reject
- `0x27` OperationEnabled 및 Fault/unsafe state에서 zero mutation

Axis1 1/2/4-byte가 닫힌 뒤 같은 matrix를 Axis2~4에 확대한다.

## 3.3 SDO-Q02 — Manual Server / programmatic contention

Class View `ParaReadWrite=0/1` manual path와 SDK programmatic path를 동시에 걸어 다음을 증명한다.

- `IDLE`에서 먼저 claim한 source만 실제 `_base` SDO를 시작
- 두 번째 source는 `BUSY` 또는 equivalent fail-closed 결과
- active request의 Index/SubIndex/Length/Direction이 다른 source에 의해 overwrite되지 않음
- completion token/source identity가 바뀌지 않음
- manual completion 뒤 programmatic request가 정상 재사용 가능
- programmatic completion 뒤 manual request가 정상 재사용 가능
- contention 동안 hidden SDO write 0회

production source에 debug-only bypass를 추가하지 않는다. 필요한 경우 별도 verifier/evidence collector만
추가한다.

## 3.4 SDO-Q03 — uncertainty / no-replay matrix

다음 fault를 각각 별도 case로 둔다.

- submission response loss
- TCP disconnect after submit
- SDO timeout
- SDO abort
- readback mismatch
- callback/orphan delayed completion
- process restart with unresolved durable journal

mutation boundary를 넘은 case는 original Submit을 다시 보내지 않는다.

```text
unknown write result
-> recover accepted ticket if provable
-> status/readback only
-> exact terminal/readback proof
-> resolve or remain RecoveryRequired
```

readback만으로 original ticket identity를 새로 만들어내지 않는다. exact mutation identity가 복구되지
않으면 operator-visible indeterminate로 남긴다.

## 3.5 Generic SDO 완료 gate

Generic SDO를 `Active`로 승격하려면 다음을 한 evidence set에서 닫는다.

- Axis1 1/2/4-byte physical exact write/readback
- Manual/Programmatic contention BUSY/no-race
- timeout/disconnect/abort/readback-mismatch no-replay
- Axis2~4 확대
- SDK/WPF/PLC Build/BootId/MapRevision 동일성
- semantic blocklist negative matrix

이 단계에서 `0x6060`을 Generic SDO에 열지 않는다.

---

# 4. P1-A — HomeDS402 completion 설계

HomeDS402 method37 core lifecycle은 이미 구현돼 있으므로 H37 state machine을 다시 작성하지 않는다.
남은 것은 **artifact -> hardware -> paired activation**이다.

## 4.1 H37-05/06 — current exact tree artifact closure

issue #32 절차를 그대로 실행한다.

1. exact current `dev` source SHA 기록
2. build 시작 UTC 기록
3. LASAL C78/ARM Rebuild + Link, 0 error
4. `Classes.lcb`, project `.lcb`, `Networks.lcb` freshness 확인
5. HomeDS402 Start/Outcome/Retire/Process direct-open 확인
6. Network smoke 확인
7. generated artifact identity를 수동 review
8. justified identity만 SourceOnly ratchet에 반영
9. full SourceOnly green

새 hash를 맞추기 위해 verifier를 완화하지 않는다.

## 4.2 H37-07/08 — hardware qualification

Axis1:

- method37 normal
- timeout
- SDO abort
- Stop preemption
- PowerOff preemption
- disconnect
- response loss
- unresolved recovery/query/retire

정상 성공은 기존 frozen contract대로 homing attained/target reached, ActualPosition=0, start bit low,
CSP8 restore, setpoint alignment, RT owner release, pending SDO/orphan 없음까지 요구한다.

Axis1 완료 뒤 Axis2~4 동일 matrix를 반복한다.

## 4.3 H37-09 — atomic activation

다음 다섯 값은 **한 changeset에서만** OFF -> ON한다.

```text
TCPMotionInterface.LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED
LMCControlCommandService.LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED
LMCDiagnosticsService.LMC_DIAG_DS402_HOME_ENABLED
LMCEcatInputLatch.LMC_DS402_HOME_STARTUP_SWEEP_ENABLED
Admin capability bit 6
```

activation commit에는 mixed-state negative verifier를 유지하고 WPF가 capability를 보고 UI를 여는지
smoke test를 추가한다. activation 전 hardware PASS를 source PASS로 대체하지 않는다.

---

# 5. P1-B — HomeDS402Ex actual runtime 구현 설계

HomeDS402Ex는 frozen SDK/wire/recovery/retained outcome store까지 존재하지만 실제 homing mutation은
아직 no-op이다. 이 기능이 남은 범위 중 가장 큰 PLC runtime 구현 tranche다.

## 5.1 선행 gate

두 gate가 모두 닫히기 전 physical runtime 구현을 시작하지 않는다.

### issue #28 — HOMEEX-01/02

축1~4별로 다음이 `HOME_DS402_EX_AXIS_PROFILE.json`에서 `approved`여야 한다.

- home/positive-limit/negative-limit/index/block source
- active level/polarity/debounce
- permitted direction/max travel
- approved homing-method allowlist
- position/velocity/acceleration/torque scale
- rounding/range/overflow
- vendor-specific DetectionVelocityLimit/DistanceLimit/TorqueLimit object mapping 또는 explicit disabled
- MapRevision paired update

### issue #35 — HOMEEX-09

- fresh C78/ARM artifact
- generated identity review
- full SourceOnly closure
- current HOMEEX retained store/static regression green

## 5.2 HOMEEX-08R — parameter SDO engine

frozen Start/Outcome/Retire wire는 변경하지 않는다. `LMCDiagnosticsService` 내부에서 current
approved frozen DINT plan을 소비하는 runtime layer를 추가한다.

한 operation은 다음 snapshot을 먼저 확보한다.

```text
current operation mode
current homing method
current homing offset/position parameter
current homing speed(s)
current homing acceleration
approved profile이 사용하는 optional vendor-specific parameters
```

program 단계는 approved profile에 존재하는 object만 쓴다. optional field가 disabled이면 해당
vendor object를 쓰지 않는다. object index/scale/range를 generic default로 추측하지 않는다.

각 SDO 단계는 다음 contract를 가진다.

```text
Start exact SDO once
-> wait exact executor token
-> validate callback/result/actual length
-> readback when semantic verification is defined
-> next stage
```

SDO write dispatch 뒤 timeout/abort/disconnect는 같은 write를 replay하지 않고 cleanup/recovery state로
간다.

## 5.3 HOMEEX-08R — RT execution mailbox

`LMCEcatInputLatch`에 HomeDS402Ex용 versioned execution mailbox를 추가하거나 existing shared Home
mailbox를 version-up한다. generated class/client declaration이 필요하면 LASAL IDE에서 먼저 생성한다.

mailbox에 최소 다음 exact frozen identity가 들어가야 한다.

- owner/session generation
- axis
- record/recovery identity
- homing method
- converted position/velocity/acceleration/limit values
- overall/detection deadline

RT owner는 exact request를 claim한 뒤에만 controlword bit4를 조작한다. non-RT service가 직접
controlword를 쓰지 않는다.

상태 흐름:

```text
Preflight
-> SnapshotParameters
-> ProgramParameters
-> AcquireRtOwner
-> Mode6Write
-> Mode6Verify
-> StartBitHigh
-> HomingObserve
-> StartBitLow
-> RestoreParameters
-> RestoreCsp8
-> AlignSetpoint
-> ReleaseRtOwner
-> FinalPositionProof
-> CommitTerminalOutcome
```

## 5.4 HOMEEX-08R — detection/safety observer

RT/non-RT 경계에서 다음을 별도 evidence로 추적한다.

- homing attained / target reached
- homing error / DS402 fault
- approved travel direction
- maximum travel
- detection timeout
- overall timeout
- approved limit input
- approved torque/detection limit when enabled
- `ActualPosition == -Position` final convention

success는 서로 다른 fresh cycle의 terminal evidence를 요구한다. 한 번의 상태 word sample로 PASS하지
않는다.

## 5.5 HOMEEX cleanup은 success보다 우선한다

실패/abort/preemption에서도 가능한 범위에서 다음 cleanup을 시도한다.

```text
start bit low
restore temporary parameters
restore CSP8
align setpoint
release RT owner
resolve SDO callback/orphan
```

`CleanupProofFlags` 6개가 모두 충족되지 않으면 clean Failed로 축소하지 않고
Indeterminate/Quarantine로 보존한다. original Start와 parameter write는 재실행하지 않는다.

## 5.6 method-size 분할

LASAL 32 KiB method limit 때문에 하나의 `ProcessAxisDs402HomeEx`에 모든 상태를 넣지 않는다.
implementation은 최소 다음 책임으로 분리한다.

```text
ProcessAxisDs402HomeEx               // dispatcher only
ProcessAxisDs402HomeExPreflight
ProcessAxisDs402HomeExParameterStages
ProcessAxisDs402HomeExRtStages
ProcessAxisDs402HomeExCleanupStages
ProcessAxisDs402HomeExRecoveryStages
```

정확한 method 이름은 구현 시 기존 class ABI와 generator 제약에 맞출 수 있으나, 한 method가
snapshot/program/run/cleanup/recovery 전체를 소유하는 구조는 금지한다.

## 5.7 HOMEEX qualification / activation

순서:

1. PC golden/malformed/frozen-plan regression
2. LASAL source/static + method-size
3. fresh C78/generated artifact
4. Axis1 approved method normal
5. timeout/limit/fault/SDO abort/Stop/PowerOff/preempt/disconnect
6. Axis2~4 및 approved method matrix
7. WPF Start surface qualification
8. `LMC_DIAG_DS402_HOME_EX_ENABLED` + Admin bit11 + WPF UI paired activation

HomeDS402 method37 PASS를 HomeDS402Ex PASS로 재사용하지 않는다.

---

# 6. P1-C — SetPosition durable / exactly-once runtime 설계

SetPosition은 issue #44가 열려 있는 동안 storage/runtime ABI를 추측 구현하지 않는다.

## 6.1 SP-01B prerequisite

반드시 먼저 확보한다.

- real vendor `CheckSum.CRC32` golden vectors
- LASAL IDE/CodeGenerator가 만든 `_FileSys` class/client ABI
- 같은 tree의 fresh C78/generated artifact evidence

CRC 이름만 보고 IEEE CRC32를 대입하지 않는다. `_FileSys` client/channel declaration을 text로
손작성하지 않는다.

## 6.2 SP-01B — dual-file durable backend

prerequisite 완료 뒤 `SET_POSITION_DESIGN.md`의 frozen 2 x 2048-byte A/B layout을 구현한다.

핵심 contract:

```text
inactive slot full image write with marker=0
-> request-specific completion
-> close
-> reopen/full 2048-byte readback
-> CRC/header/body/generation validation
-> marker+inverse 8-byte write
-> close
-> reopen/full readback
-> publish generation
```

기존 active slot은 새 slot의 두 번째 readback이 성공하기 전까지 active로 유지한다. partial/torn/
I/O uncertainty는 이전 active generation을 수정하지 않고 `StorageDegraded`로 신규 mutation을 막는다.

Store public/lifecycle contract는 current `Begin/Commit/Read/Retire` 의미를 유지하고 backend만 durable
implementation으로 교체한다.

## 6.3 SP-02 — RT exactly-once executor

LASAL IDE에서 versioned execution mailbox/result ABI를 생성한 뒤 다음 순서로 구현한다.

```text
Control publishes exact tuple + Store generation
-> RT validates current owner/state/limit/tuple
-> RT publishes Claimed
-> one logical native call site
   SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST,
               Position:=TargetPosition)
-> stable 3 fresh RT samples
-> terminal candidate
```

`NativeCount`는 한 exact tuple에서 `0 -> 1`만 허용한다. duplicate/reconnect/recovery로 같은 tuple이
재관찰되면 저장된 executor state를 반환하고 `.SetPosition()`을 다시 호출하지 않는다.

## 6.4 SP-03 — durable terminal before release

고정 순서:

```text
RT terminal proof
-> durable terminal commit
-> full backend readback
-> ownership release
-> TCP response
```

terminal durable commit/readback이 불확실하면 owner를 먼저 release하거나 success response를 만들지
않는다.

## 6.5 SP-04 — WPF recovery integration

MainWindow mutation UI에 current `AxisSetPositionRecoveryJournal`을 실제 연결한다.

- wire 전에 `ArmedBeforeDispatch`
- startup unresolved -> `RecoveryRequired`
- exact capability/endpoint/Build/BootId/MapRevision 확인
- original Start replay 금지
- `0x7D14` exact outcome query
- terminal generation durable 저장
- `0x7D1A` exact retire
- retirement success 뒤에만 `Resolved`

## 6.6 SP-05/06/07 qualification과 activation

- SourceOnly / method-size
- fresh C78/generated artifact
- storage cold power-cycle / reopen
- response loss/query/retire
- native duplicate-zero mutation tests
- Axis1 small approved correction
- fault/reconnect/packet matrix
- Axis2~4 approved max-jump matrix

모두 완료된 뒤에만 다음을 paired activation한다.

```text
LMC_ADMIN_SET_POSITION_STORE_CONFIGURED TRUE
ordinary ownership SetPosition admission ON
Axis1..4 approved SetPositionMaxJump > 0
Admin capability bits 3/5/7 ON
native executor enabled
WPF mutation UI enabled
```

---

# 7. P2-A — EtherCAT NodeHealth / DigitalIO Read

read-only 기능부터 활성화한다. current source에는 `LMCEcatInputLatch`의
`Coupler/InputSlot/OutputSlot`, extended snapshot과 read-owner 구조가 존재하므로 새 topology wire를
재설계하지 않는다.

## 7.1 NodeHealth bit15

`0x7E13`에서 다음 negative/quality matrix를 먼저 닫는다.

- exact TopologyRevision
- configured node lookup
- online/offline
- PREOP/SAFEOP/OP
- identity mismatch
- source client disconnect
- master missed-frame/stale
- drive DS402 data normalization

invalid/defaulted snapshot을 value=0 정상 데이터로 오인하지 않는 것을 packet test로 고정한다.

## 7.2 DigitalIORead bit16

`0x7E22`에서 current CREVIS input/output-shadow 32-bit reference를 검증한다.

- Input value + full valid mask
- Output shadow + nonzero OutputRevision
- stale frame -> ValidMask=0
- node offline -> fault quality
- source unavailable -> fault quality
- stale TopologyRevision/IOReference/direction/width reject

NodeHealth와 DigitalIORead가 동일 generated topology/image에서 PASS한 뒤 bits15/16을 paired activation한다.

---

# 8. P2-B — Digital Output Write `0x7E23`

이 기능은 C# surface만 있고 LASAL route가 없으므로 실제 신규 runtime 구현 대상이다.

## 8.1 frozen wire 유지

기존 SDK 설계의 exact 40-byte request를 그대로 사용한다.

```text
TopologyRevision
IOReference
Value U64
Mask U64
ExpectedOutputRevision
DiagnosticsBootId
```

SDK에서 생성된 topology-bound valid Output snapshot만 mutation request의 provenance가 될 수 있다.

## 8.2 LASAL route / ticket owner

변경 대상:

- `TCPMotionInterface.st`: `0x7E23` diagnostics route
- `LMCDiagnosticsService.st`: validate/submit/status owner
- `LMCEcatInputLatch.st`: RT output mailbox consumer
- generated network/client ABI: 필요 시 LASAL IDE에서 생성
- capability bit17 advertisement

`0x7E03 GetOperationStatus` / `0x7E04 CancelOperation`의 기존 ticket lifecycle을 재사용한다.

## 8.3 RT CAS write

RT는 output value 관찰 뒤 같은 cycle owner에서 다음을 실행한다.

```text
ExpectedOutputRevision == CurrentOutputRevision
Mask != 0
Mask subset of OutputValidMask
Value has no bits outside Mask
NewOutput = (OldOutput & ~Mask) | (Value & Mask)
```

검증과 네 output byte write를 같은 RT owner claim 안에서 수행한다. PC read-modify-write는 금지한다.

mailbox는 기존 설계의 state를 유지한다.

```text
IDLE -> WRITING_REQUEST -> READY -> RUNNING
     -> WRITING_COMPLETION -> COMPLETION_READY -> IDLE
```

RT가 `READY -> RUNNING`을 먼저 claim한 뒤 cancel은 물리 적용을 취소했다고 주장하지 않는다.
completion 소비 전 다음 request를 받지 않는다.

## 8.4 output write safety policy

첫 activation에서 writable reference는 current CREVIS output word 하나와 승인된 `WritableMask`만 둔다.
전체 32-bit를 자동 writable로 간주하지 않는다.

필요 gate:

- operator explicit confirmation
- TopologyRevision/BootId current
- output snapshot Valid
- nonzero ExpectedOutputRevision
- compile-time SDK allowlist + PLC allowlist exact match
- stale revision mutation 0회
- mask 밖 bit mutation 0회

physical feedback이 없는 경우 output readback은 **PLC output shadow**라는 의미를 유지한다. terminal
success를 실제 단자 전압 증거로 표현하지 않는다.

---

# 9. P2-C — PI Write

현재 SDK `SubmitPIWrite`와 packet model은 있으나 `AllowedPIWriteSignalIds`가 empty이고 capability bit7은
OFF다. 따라서 먼저 **writable semantic catalog를 승인**해야 한다.

## 9.1 writable signal 승인

SignalId를 단순히 current 24-entry PI catalog에서 골라 자동 writable로 만들지 않는다. 각 writable
signal에 다음 metadata를 별도 승인한다.

- SignalId / alias
- value type
- unit/scale
- min/max
- writable bit mask if bitfield
- allowed axis/machine state
- owner/resource
- readback source
- whether restart persistence is allowed

motion target/controlword/opmode/torque 등 기존 semantic owner와 충돌하는 signal은 PI Write에 넣지
않는다.

## 9.2 implementation

- SDK `AllowedPIWriteSignalIds`를 approved manifest에서 생성/고정
- PLC `LMCDiagnosticsService`에서 동일 allowlist와 MapRevision 검증
- shared mutation owner를 reserve한 뒤 only-one write
- terminal ticket 후 PI image/readback exact 확인
- timeout/disconnect 뒤 automatic replay 금지

SDK와 PLC allowlist가 다르면 activation verifier가 실패하도록 한다. bit7은 allowlist가 비어 있으면
항상 OFF다.

---

# 10. P2-D — Recorder DoubleBank

DoubleBank는 SDK/WPF qualification surface와 dormant source가 있으므로 새 recorder protocol을 만들지
않는다.

activation 전 검증:

- exactly two physical banks
- bank identity/generation exact
- active/frozen bank 교대 시 overwrite 없음
- Ring/Trigger와 Double 조합
- disconnect/adopt/release
- stale record/buffer identity
- Trigger/Stop race
- 1 ms RT jitter/overrun
- free RAM margin
- long soak

Double capability bit6은 `RecorderSingleBank + RecorderTrigger` dependency와 exactly two buffers가
동시에 만족할 때만 advertise한다. single-bank PASS를 double-bank PASS로 승격하지 않는다.

---

# 11. P2-E — Extended SDO Result bit12

SDK의 `ReadSdoResultChunk` surface는 이미 존재한다. bit12 activation 전에 PLC side chunk producer가
실제 `MaxSdoDataBytes`보다 큰 result contract를 지원하는지 명확히 결정한다.

v1 Generic scalar Read/Write가 최대 4 bytes로 충분하면 bit12를 억지로 활성화하지 않는다. 요구사항상
large SDO result가 필요할 때만 다음 tranche를 연다.

- ticket terminal 뒤 immutable result buffer freeze
- chunk offset/count bounds
- exact ticket/BootId identity
- stale/retired ticket reject
- disconnect read-only continuation policy
- buffer overwrite 방지
- chunk sequence/LastChunk
- fault/timeout/cancel/orphan matrix

즉 bit12는 존재한다는 이유만으로 production scope에 강제하지 않고 requirement gate를 먼저 둔다.

---

# 12. ApplicationPhaseSnapshot / ExtendedWkcDiagnostics

capability bit10/11 surface는 current high-priority release 계획에 포함돼 있지 않다. 구현 여부를 먼저
requirements coverage에서 재확인한다.

- 외부 요구사항이 없으면 `Dormant / deferred`로 문서화하고 production release blocker로 만들지 않는다.
- 요구사항이 남아 있으면 read-only 기능으로 별도 design tranche를 만든 뒤 D1/D2와 동일한
  coherent-snapshot/quality contract를 사용한다.

새 mutation 기능보다 우선하지 않는다.

---

# 13. 공통 test / evidence 구조

모든 신규 tranche는 다음 계층을 분리한다.

## 13.1 PC contract

- golden packet
- malformed length/reserved/enum
- stale capability/session
- duplicate/replay local guard
- parser domain failure

## 13.2 Source/static

- route exists / route count
- capability pair
- owner/resource identity
- exact single mutation site
- no replay site
- method size < 32768
- semantic blocklist/allowlist

## 13.3 C78/generated artifact

- exact source SHA
- build started UTC
- 0 errors/link success
- generated artifact freshness
- direct-open
- Network smoke
- reviewed physical identity ratchet

## 13.4 PLC/hardware

- normal
- busy/contention
- fault/abort
- timeout
- disconnect
- response loss
- restart/warm/cold boundary as applicable
- exact readback/terminal proof

## 13.5 activation

activation commit에서는 반드시 다음을 한 번에 확인한다.

```text
source gate
capability bit
SDK dependency
WPF UI gate
manual current status
regression verifier
```

---

# 14. 구현 tranche / commit 권장 단위

큰 기능을 한 commit에서 전부 열지 않는다.

```text
R1  Generic SDO qualification tooling / evidence-only correction
R2  HomeDS402 artifact closure
R3  HomeDS402 hardware matrix
R4  HomeDS402 paired activation

R5  HomeDS402Ex approved-profile freeze
R6  HomeDS402Ex parameter SDO runtime
R7  HomeDS402Ex RT mailbox / physical state machine
R8  HomeDS402Ex cleanup/recovery + method-size
R9  HomeDS402Ex hardware matrix
R10 HomeDS402Ex paired activation

R11 SetPosition vendor/IDE prerequisite import
R12 SetPosition durable A/B backend
R13 SetPosition RT exactly-once executor
R14 SetPosition terminal-before-release + WPF recovery
R15 SetPosition hardware/cold-cycle
R16 SetPosition paired activation

R17 NodeHealth/DigitalIO read qualification + bits15/16
R18 Digital Output Write route/mailbox
R19 Digital Output Write physical qualification + bit17
R20 PI Write approved allowlist/runtime
R21 Recorder DoubleBank qualification
R22 Extended SDO Result only if requirement confirmed
R23 distribution/release qualification
```

각 tranche는 `dev`를 source truth로 하고 검증 완료 뒤 장기 작업 branch를 남기지 않는다.

---

# 15. production release gate

전체 API를 production candidate로 올리기 전에 최소 다음 조건이 필요하다.

1. SetOperationMode 완료 계약 회귀 green
2. Generic SDO P0 완료
3. HomeDS402 activation 또는 명시적 production 제외 결정
4. HomeDS402Ex activation 또는 명시적 production 제외 결정
5. SetPosition activation 또는 명시적 production 제외 결정
6. production scope에 포함할 Diagnostics dormant/missing surface의 구현/제외 결정
7. 같은 source SHA에서 fresh C78/generated artifact
8. same-image PLC load/runtime evidence
9. WPF/SDK distribution binary identity
10. `API_MANUAL.md`, `API_DEVELOPMENT_PROGRESS.md`, DINT packet map과 distribution docs sync
11. source-only/PC/Debug/Release/current qualification green
12. production capability mask와 실제 runtime gate가 exact match

모든 dormant 기능을 무조건 ON해야 production release가 가능한 것은 아니다. 다만 **public API가 존재하는데
PLC route가 없거나 capability가 OFF인 기능은 manual과 capability에서 명확히 unsupported/deferred로
표현**해야 한다. production capability가 광고한 기능에는 source/PLC/hardware evidence가 반드시 있어야
한다.

---

# 16. 다음 실제 작업

현재 즉시 시작할 구현/qualification 순서는 다음으로 고정한다.

1. **Generic SDO SDO-Q01~Q03**를 완료해 issue #46을 닫는다.
2. 병행 가능하면 **HomeDS402 issue #32 fresh C78/generated artifact**를 닫는다.
3. HomeDS402 H37-07/08 hardware 후 H37-09 activation.
4. issue #28/#35가 준비되면 HomeDS402Ex R6~R10 actual runtime.
5. issue #44 prerequisite가 준비되면 SetPosition R12~R16.
6. 이후 read-only NodeHealth/DigitalIO -> Digital Output Write -> PI/Recorder/Extended SDO 순서.

이 순서에서 hardware/vendor/IDE evidence가 필요한 단계가 대기 상태가 되더라도 다른 독립 tranche는
진행한다. 단, blocker를 없애기 위해 안전 gate, generated ABI, hardware profile 또는 durable semantics를
추정 구현하지 않는다.

# 남은 API 구현 통합 설계 — 2026-09-02

- 기준 branch: `dev`
- current source baseline: `dev@5666497c9baef01ee84e534b7041cf0bbb96baf5` (`dev : add SimulationSetup`)
- Generic SDO Write: **FEATURE IMPLEMENTATION COMPLETE**
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0-A: **2-drive topology + SimulationSetup regression freeze**
- current P0-B: **HomeDS402 completion / activation**
- current P1: **SetPosition durable runtime implementation**
- production posture: **NO-GO**

이 문서는 2026-09-02 이후 남은 기능 구현의 current master다.
latest topology change 이후의 exact override는 `CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`를 우선한다.

2026-09-02 사용자 요청으로 HomeDS402 Method 37 source/UI candidate는 atomic ON으로
구현됐다. 아래의 `activation OFF` 및 OFF-state preparation 문구는 원래 단계 계획이며,
current PLC image가 활성화됐다는 뜻으로 사용하지 않는다. fresh LASAL build/link/download와
Axis1/2 runtime/hardware qualification 전 production posture는 계속 NO-GO다.

current detailed references:

- `CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`
- `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
- `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
- `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md`

기존 frozen wire/state-machine 상세는 계속 다음 문서를 참조한다.

- `HOME_DS402_DESIGN.md`
- `SET_POSITION_DESIGN.md`
- `../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`

---

## 1. current 상태 재분류

| 우선순위 | 기능/영역 | current 상태 | 실제 남은 개발 |
|---:|---|---|---|
| 완료 | SetOperationMode | Active | release regression 유지 |
| 완료 | Generic SDO Write | 구현 완료 | current 2-drive physical matrix / release evidence |
| P0-A | Topology / SimulationSetup | 구현 반영 | cold boot + ownership + nonphysical-target regression freeze |
| P0-B | HomeDS402 | source/UI candidate ON | fresh C78 -> Axis1/2 hardware -> runtime qualification |
| P1 | SetPosition | Dormant / SP-C0 complete | SP-C1 prerequisites -> durable backend -> RT exactly-once -> recovery -> activation |
| P2 | HomeDS402Ex | Dormant | profile/artifact prerequisite 뒤 physical runtime |
| P3 | 기타 dormant/missing | Dormant/Missing | DigitalIO/DO/PI/Double/Extended SDO 등 |

Generic SDO를 신규 구현 P0로 되돌리지 않는다.

---

## 2. current topology contract

current source는 logical axis와 physical EtherCAT drive를 분리한다.

```text
logical axes             = Axis1..Axis9
physical Elmo drives     = Axis1, Axis2
physical mask            = 0x00000003
simulation axes          = Axis3..Axis9
```

latest source sequence:

```text
b746252c  two-drive encoder/ownership admission support
570fddd5  ownership file-local physical-latch define fix
5666497c  SimulationSetup + Motion Network wiring
```

`SimulationSetup` current behavior:

- Axis1..9 settings are `Retentive=File`
- first scan writes all retained Axis_N values to corresponding `SimulateMode`
- live server Write also immediately propagates
- Motion Network configured default: Axis1/2 non-simulation, Axis3..9 simulation

앞으로 physical-drive-dependent command는 axis number 범위가 아니라 physical mask 기준으로 admission한다.

---

## 3. 공통 mutation / topology 원칙

HomeDS402와 SetPosition 및 drive-level diagnostics는 다음을 지킨다.

1. original mutation automatic replay 0
2. current session/build/BootId/MapRevision identity 확인
3. shared owner를 terminal proof 전에 release하지 않음
4. response loss는 status/query/recovery로 처리
5. source 구현과 LASAL compile / PLC boot / hardware PASS를 구분
6. capability/gate는 hardware qualification 전까지 OFF
7. generated LASAL ABI/artifact/hash를 추측하거나 blind ratchet하지 않음
8. logical axis와 physical drive를 분리
9. simulation PASS를 physical hardware PASS로 간주하지 않음
10. physical-drive count 변경 시 topology regression을 먼저 재수행
11. existing SetOperationMode/Generic SDO/ownership no-replay 계약을 약화하지 않음

---

# 4. P0-A — TOPO-C0 current topology freeze

HomeDS402/SetPosition 신규 tranche 전에 먼저 current `5666497+` tree를 고정한다.

실행/확인:

```text
LASAL Compile/Rebuild 0 errors
-> SimulationSetup direct-open
-> cold/restart boot SimulateMode verification
-> ownership startup proof with Axis1/2 only
-> Encoder Maintenance Axis1/2 positive
-> Encoder Maintenance Axis3/4 nonphysical negative
-> topology/mask static verifier
```

PASS 조건:

- Axis1/2 `SimulateMode=0`
- Axis3..9 `SimulateMode=1`
- retained setting이 first scan에 재적용
- `LMCEcatInputLatch`, `LMCControlCommandService`, `LMCDiagnosticsService` physical mask = `0x03`
- absent physical Drive3/4가 startup physical proof를 막지 않음
- file-local macro compile error 없음
- HomeDS402 source candidate activation은 atomic ON, SetPosition activation은 OFF

권장 verifier 추가:

`tools/Verify-CurrentPhysicalTopology.ps1`

최소 assertion:

- SimulationSetup Axis1..9 object/channel/wiring
- Axis1/2 physical vs Axis3..9 simulation defaults
- three runtime mask constants = `0x03`
- HomeDS402 gates atomic all-OFF/all-ON; current source all-ON
- SetPosition gates OFF

---

# 5. P0-B — HomeDS402

HomeDS402 core state machine은 구현돼 있다. 새로운 Home engine을 만들지 않는다.

기존 hardware-independent qualification은 historical evidence로 유지하지만,
latest topology patches가 shared owner/latch/diagnostics/network를 변경했으므로 current tree에서 재실행한다.

revised execution order:

```text
TOPO-C0
-> H37-C0R current-dev regression re-run
-> H37-C1 fresh C78/generated artifact + SourceOnly
-> H37-C2 activation candidate source/UI qualification
-> H37-C3 Axis1 hardware normal/failure
-> H37-C4 Axis2 hardware + Axis3/4 nonphysical rejection
-> H37-C5 five-value atomic activation runtime qualification
```

### H37-C0R

기존 verifier를 latest tree에서 재실행한다.

```powershell
./tools/Verify-HomeDs402H37Activation.ps1
./tools/Verify-HomeDs402H37Ownership.ps1
./tools/Verify-HomeDs402H37MethodSize.ps1
./tools/Verify-HomeDs402H37WpfRecovery.ps1
```

확인 포인트:

- physical mask 변경이 owner/preemption contract를 깨지 않음
- SimulationSetup boot 설정이 H37 startup proof와 race하지 않음
- SDO executor changes가 method37 exact sequence를 바꾸지 않음
- duplicate Start/replay 0
- activation mixed-state rejection 유지

### H37-C1

exact latest tree로 LASAL C78/ARM Rebuild + Link 후 current generated artifacts를 다시 capture한다.
`5666497`에서 `Classes.lcb`, `Networks.lcb`, Motion Network generated table이 변경됐으므로 과거 artifact hash를 재사용하지 않는다.

### H37-C3/C4 hardware scope

current physical topology에서 hardware matrix는 Axis1/2다.

- Axis1 normal/fault/timeout/disconnect/response-loss
- Axis2 normal/fault/timeout/disconnect/response-loss
- Axis3/4는 physical HomeDS402가 필요할 경우 deterministic unavailable/fail-fast
- Axis3/4 simulation PASS는 DS402 hardware evidence로 사용하지 않음

Elmo3/4가 향후 복구되면 physical mask와 H37 hardware matrix를 별도 changeset으로 재확장한다.

### H37 source activation candidate is ON

- TCP ordinary ownership ON
- Control ordinary ownership ON
- Diagnostics `LMC_DIAG_DS402_HOME_ENABLED = TRUE`
- InputLatch `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED = TRUE`
- Admin bit6 ON (`0x00000757`)
- Diagnostics operational mask `0x0000613F` remains unchanged
- current PLC image/runtime/hardware activation remains unqualified

---

# 6. P1 — SetPosition

SetPosition은 wire/lifecycle scaffold는 있지만 production mutation engine이 미완료다.

SP-C0는 완료됐다.

- evidence: `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md`
- validating commit: `08e85d456ad118abaec8405fe9ab1c1ec3baa974`
- verifier: 37 checks PASS
- WPF `AxisSetPositionJournal`: 11/11 PASS

latest topology patch가 frozen SetPosition store/wire ABI를 폐기하지는 않는다.
다만 SP-C1 착수 직전 SP-C0 verifier를 latest current tree에서 재실행한다.

next execution order:

```text
SP-C0R current-tree smoke
-> SP-C1 CRC golden + IDE _FileSys ABI
-> SP-C2 durable A/B backend
-> SP-C3 Store durable adapter
-> SP-C4 RT claim-before-native exactly-once executor
-> SP-C5 terminal-before-release integration
-> SP-C6 WPF recovery completion
-> SP-C7 source/C78 qualification
-> SP-C8 Axis1/2 hardware + Axis3/4 simulation matrix
-> SP-C9 paired activation
```

### SP-C1 external evidence boundary

필수:

1. vendor `CheckSum.CRC32` golden fixture
2. LASAL IDE-generated `_FileSys` class/client/channel ABI

금지:

- CRC 알고리즘 추측
- `_FileSys` generated ABI hand-authoring
- evidence 전 A/B backend 완료 판정
- Store configured/ownership/max-jump/capability activation

### topology impact

기존 SetPosition public scope가 Axis1..4이면 scope는 자동으로 1..9로 확장하지 않는다.

- Axis1/2: physical/native coordinate qualification
- Axis3/4: simulation exactly-once/lifecycle/recovery qualification
- Axis5..9: out-of-current-contract unless separately designed

---

# 7. HomeDS402Ex 위치

HomeDS402Ex는 P2다.

- issue #28: axis profile/wiring/polarity/method/scale/range
- issue #35: fresh C78/generated artifact/SourceOnly
- 이후 parameter SDO + RT physical homing runtime

current topology에서는 physical HomeDS402Ex hardware expansion 역시 Axis1/2를 우선한다.
Axis3+ simulation을 physical homing proof로 사용하지 않는다.

---

# 8. branch / commit policy

- integration source truth는 `dev`
- 기능별 변경은 작은 tranche로 커밋
- topology/mask 변경 commit과 activation commit 분리
- generated artifact/hash update는 exact C78 evidence와 함께 기록
- activation commit은 구현 commit과 분리
- temporary workflow/helper는 검증 뒤 제거
- source SHA / LASAL artifact / PLC image / WPF binary identity를 같은 evidence set으로 기록

권장 다음 commit 단위:

```text
test(topology): freeze two-drive simulation startup contract
test(h37): rebaseline HomeDS402 after topology update
chore(h37): record fresh C78 artifact evidence
fix(h37): <only if current hardware evidence finds defect>
feat(h37): atomically activate HomeDS402

test(setposition): rerun SP-C0 on current topology
feat(setposition): add durable file backend
feat(setposition): connect durable store lifecycle
feat(setposition): add RT exactly-once executor
feat(setposition): enforce terminal-before-release
feat(wpf): complete SetPosition recovery interlock
```

---

# 9. completion definition

## TOPO-C0 complete

- latest LASAL Compile/Rebuild PASS
- SimulationSetup boot/restart behavior proven
- Axis1/2 physical + Axis3..9 simulation role verified
- ownership startup ready with absent Drive3/4
- drive-only command nonphysical fail-fast behavior verified
- physical mask static contract frozen

## HomeDS402 implementation complete

- TOPO-C0 PASS
- latest current-tree H37 regression PASS
- fresh current-tree artifact closure
- Axis1/2 normal/fault/timeout/disconnect/response-loss matrix
- Axis3/4 nonphysical target deterministic handling
- query/retire no-replay recovery
- five activation values atomic ON
- WPF/API/C78/PLC same-image evidence

## SetPosition implementation complete

- SP-C0 current-tree regression PASS
- vendor/IDE prerequisites reviewed
- durable A/B Store cold-cycle evidence
- native SetPosition logical call exactly once
- stable observer
- terminal durable readback-before-release
- WPF restart/query/retire no-replay
- Axis1/2 physical approved correction matrix
- Axis3/4 simulation lifecycle matrix
- Store/ownership/max-jump/capability paired activation

전체 production release는 이 기능 완료만으로 자동 승인하지 않는다. Distribution/release gate는 별도다.

# Current Implementation Handoff — 2026-09-02

- integration branch: `dev`
- current source baseline: `dev@5666497c9baef01ee84e534b7041cf0bbb96baf5` (`dev : add SimulationSetup`)
- production posture: **NO-GO**
- purpose: 2-physical-drive topology, startup Simulation setup, HomeDS402 and SetPosition remaining implementation을 현재 source에 맞춰 다시 고정한다.

이 문서는 `REMAINING_IMPLEMENTATION_DESIGN_20260902.md`,
`HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`,
`SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`의 frozen wire/state-machine을 유지하면서,
**2026-09-02 최신 topology 변경 이후의 current implementation state와 다음 개발 순서를 override하는 handoff**다.

문서 충돌 시 current `dev` source와 본 문서를 우선한다.

---

## 1. latest source changes incorporated

### 1.1 Two-physical-drive admission fix

`b746252c3341bab2fe6b1c57db41963908e91f47`
`fix: support encoder maintenance with two physical drives`

현재 physical drive contract를 `0x00000003` = Axis/Drive1 + Axis/Drive2로 고정했다.

- `LMCEcatInputLatch`: startup physical proof가 configured physical drive만 요구
- `LMCControlCommandService`: startup `physicalIdle`가 configured physical axis만 요구
- `LMCDiagnosticsService`: SDO executor startup proof가 configured physical drive만 요구
- Encoder Maintenance:
  - ownership admission failure detail `43`
  - non-physical drive target detail `44`
- logical axis count와 physical EtherCAT drive count를 분리

### 1.2 File-local ownership latch define fix

`570fddd5ba06c718cb17aebe85b302cd8ebd358f`
`fix: define physical startup latch in ownership service`

`LMCControlCommandService.st`에서 사용하는
`LMC_OWNER_STARTUP_LATCH_PHYSICAL = 0x00000001`을 file-local define으로 명시했다.
LASAL ST preprocessor symbol은 다른 ST file의 define을 자동 공유한다고 가정하지 않는다.

### 1.3 SimulationSetup

`5666497c9baef01ee84e534b7041cf0bbb96baf5`
`dev : add SimulationSetup`

새 `SimulationSetup` class가 추가됐다.

- server: `Axis_1` ... `Axis_9`
- 각 server는 `Initialize=true`, `Retentive=File`
- client: `Simul_Axis_1` ... `Simul_Axis_9`
- `_FirstScan = 1`일 때 Axis1~9의 저장 값을 각 `SimulateMode` client에 즉시 전달
- server Write 시에도 해당 axis `SimulateMode`에 즉시 전달

Motion Network current configured defaults:

| Logical Axis | SimulationSetup network value | Current role |
|---:|---:|---|
| Axis1 | 0 / no explicit nonzero value | physical Elmo drive |
| Axis2 | 0 / no explicit nonzero value | physical Elmo drive |
| Axis3 | 1 | simulation |
| Axis4 | 1 | simulation |
| Axis5 | 1 | simulation |
| Axis6 | 1 | simulation |
| Axis7 | 1 | simulation |
| Axis8 | 1 | simulation |
| Axis9 | 1 | simulation |

`SimulationSetup1.Simul_Axis_N`은 `_LMCAxisN.SimulateMode`에 각각 연결된다.

따라서 current topology의 정본 표현은 다음과 같다.

```text
logical axes: 1..9
physical Elmo drives: Axis1, Axis2
configured physical-drive/axis mask: 0x00000003
simulation axes: Axis3..Axis9
```

Axis3/4의 과거 Elmo object/client가 LASAL network에 남아 있더라도, current EtherCAT physical topology와
startup proof에서는 physical drive로 요구하지 않는다.

---

## 2. topology invariants for all further development

앞으로 HomeDS402, Encoder Maintenance, Generic SDO, SetPosition 개발은 logical axis count만으로
physical capability를 판단하지 않는다.

### T-01 physical drive source of truth

current product contract:

```text
PhysicalDriveMask = 0x00000003
```

현재 이 값은 최소 다음 runtime layer와 일치해야 한다.

- `LMCEcatInputLatch.st`
- `LMCControlCommandService.st`
- `LMCDiagnosticsService.st`
- Motion Network / `SimulationSetup1` configured role

동일 의미의 mask가 서로 다른 값으로 drift하면 activation을 금지한다.

### T-02 physical-drive-only command admission

실제 EtherCAT drive SDO/DS402 mutation이 필요한 command는 configured physical target만 승인한다.

- Encoder Maintenance: Axis/Drive1,2 only
- HomeDS402: current activation candidate는 Axis1,2 physical qualification only
- physical SDO qualification: Slave1,2 only

Axis3+ target에서 physical drive가 필요하면 timeout으로 흘리지 말고 명시적 unavailable/not-ready 계열로
fail-fast해야 한다.

### T-03 simulation is not hardware evidence

Axis3..9 Simulation PASS를 Axis1/2 hardware PASS로 대체하지 않는다.

반대로 logical/SDK/owner lifecycle 회귀에는 simulation axis를 적극 사용해도 된다.

### T-04 boot ordering

`SimulationSetup::Init`의 first-scan 적용이 ownership startup / command admission보다 늦어서
초기 cycle에 잘못된 physical/simulation 상태를 관찰하지 않도록 실제 PLC에서 boot ordering을 확인한다.

---

## 3. mandatory current-topology regression tranche — TOPO-C0

HomeDS402/SetPosition 신규 구현을 계속하기 전에 current tree `5666497` 기준으로 아래를 닫는다.

### Source / LASAL

- LASAL Compile/Rebuild 0 errors
- `SimulationSetup` class direct-open 가능
- generated `Classes.lcb`, `Networks.lcb`, Motion network table current-tree와 일치
- file-local startup latch symbols 전부 resolve

### Cold/restart boot

- Axis1 `SimulateMode=0`
- Axis2 `SimulateMode=0`
- Axis3..9 `SimulateMode=1`
- retained `SimulationSetup.Axis_N` 변경 후 restart 시 의도한 값이 first scan에서 재적용

### Ownership startup

- Axis3/4 physical EtherCAT absence가 startup physical proof를 막지 않음
- configured physical Axis1/2가 Online/OP/AL-clear일 때 physical latch 생성 가능
- final ownership startup proof가 정상 ready 상태까지 진행

### Encoder Maintenance regression

- Axis1 TW19/TW20 admission 정상
- Axis2 TW19/TW20 admission 정상
- Axis3/4 physical maintenance 요청은 명시적 `PhysicalDriveUnavailable` 계열 reject
- admission failure가 generic downstream reject로 가려지지 않음

### Static verifier addition

새 verifier는 최소 다음을 assert한다.

- physical mask `0x03` 일치
- SimulationSetup Axis1/2 default physical, Axis3..9 simulation
- `SimulationSetup1.Simul_Axis_N -> _LMCAxisN.SimulateMode` 1:1 wiring
- HomeDS402 activation OFF
- SetPosition activation OFF

TOPO-C0는 feature activation이 아니다. topology baseline freeze다.

---

## 4. HomeDS402 current status after topology change

상태: **Dormant / core lifecycle implemented / current-topology rebaseline required**

기존 method37 lifecycle은 다시 작성하지 않는다.

기존 software qualification evidence는 유효한 historical regression evidence지만,
`b746252c`, `570fddd5`, `5666497c`에서 HomeDS402가 공유하는 다음 영역이 변경됐다.

- `LMCEcatInputLatch`
- `LMCControlCommandService`
- `LMCDiagnosticsService`
- Motion Network/generated artifact

따라서 latest tree에서 H37-C0를 다시 실행한다.

### revised HomeDS402 sequence

```text
TOPO-C0 current topology freeze
-> H37-C0R current-dev regression re-run on 5666497+
-> H37-C1 fresh C78/generated artifact closure
-> H37-C2 activation-candidate OFF-state qualification
-> H37-C3 Axis1 physical hardware matrix
-> H37-C4 Axis2 physical expansion + Axis3/4 non-physical rejection matrix
-> H37-C5 atomic activation
```

### H37-C4 scope correction

과거 문서의 `Axis2..4 hardware expansion`을 current topology에 그대로 적용하지 않는다.

current requirement:

- Axis1: physical hardware normal/failure matrix
- Axis2: physical hardware normal/failure matrix
- Axis3/4: physical HomeDS402 command를 지원하지 않는다면 deterministic fail-fast 검증
- Axis3/4 simulation 결과를 physical DS402 Home evidence로 인정하지 않음

향후 Elmo3/4가 다시 물리 topology에 편입되면 mask와 hardware matrix를 별도 changeset으로 재확장한다.

### activation remains OFF

- TCP ordinary ownership OFF
- Control ordinary ownership OFF
- Diagnostics `LMC_DIAG_DS402_HOME_ENABLED = FALSE`
- InputLatch `LMC_DS402_HOME_STARTUP_SWEEP_ENABLED = FALSE`
- Admin HomeDS402 bit6 OFF

---

## 5. SetPosition current status after topology change

상태: **Dormant / SP-C0 complete / SP-C1 next**

SP-C0 source inventory는 완료됐다.

- evidence: `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md`
- validating commit: `08e85d456ad118abaec8405fe9ab1c1ec3baa974`
- verifier: 37 checks PASS
- WPF `AxisSetPositionJournal`: 11/11 PASS

latest topology changes가 Store ABI나 frozen wire를 변경한 것은 아니므로 SP-C0를 폐기하지 않는다.
다만 SP-C1 코드 작업 직전 current-tree verifier를 한 번 재실행해 drift가 없는지 확인한다.

### next SetPosition step remains SP-C1

필수 external prerequisite:

1. vendor `CheckSum.CRC32` golden fixture
2. LASAL IDE-generated `_FileSys` class/client/channel ABI

이 두 evidence 없이 durable file backend를 추측 구현하지 않는다.

### topology impact on SetPosition qualification

frozen public scope가 Axis1..4라면:

- Axis1/2: hardware/native SetPosition qualification
- Axis3/4: simulation lifecycle/exactly-once/recovery qualification
- simulation PASS는 Axis1/2 physical coordinate evidence를 대체하지 않음

Axis5..9는 SimulationSetup에 존재하지만 SetPosition public contract를 자동 확장하지 않는다.
범위 확대는 별도 requirement/design changeset으로 처리한다.

SetPosition activation remains OFF:

```text
LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = FALSE
LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED = FALSE
LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS1..4 = 0
Admin capability bits 3/5/7 = OFF
```

---

## 6. next development order

현재 우선순위는 다음으로 고정한다.

```text
P0-A TOPO-C0
  current 2-drive + SimulationSetup boot/topology regression

P0-B HomeDS402
  H37-C0R -> C78 -> Axis1/2 hardware -> nonphysical fail-fast -> activation

P1 SetPosition
  SP-C0 current-tree smoke -> SP-C1 prerequisites -> durable backend -> RT exactly-once -> recovery -> activation

P2 HomeDS402Ex
  current priorities가 닫힌 뒤 진행
```

---

## 7. completion / release rule

- source PASS != LASAL Compile PASS
- LASAL Compile PASS != PLC boot PASS
- PLC boot PASS != hardware mutation PASS
- simulation PASS != physical drive PASS
- generated artifact/hash는 exact current tree의 C78 결과만 사용
- topology mask 변경과 feature activation은 같은 commit에 섞지 않음
- physical drive count가 바뀌면 TOPO-C0를 다시 수행
- production release는 계속 **NO-GO**

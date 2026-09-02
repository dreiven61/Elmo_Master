# 최우선 API 개발 설계

- 기준일: 2026-09-02
- current integration / qualification source: `dev`
- current source baseline: `dev@5666497c9baef01ee84e534b7041cf0bbb96baf5` (`dev : add SimulationSetup`)
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- production release posture: **NO-GO**

## current implementation master

**2026-09-02 최신 topology 변경 이후 신규 구현은 다음 문서를 정본으로 사용한다.**

1. `CURRENT_IMPLEMENTATION_HANDOFF_20260902.md` — 2 physical drives + SimulationSetup 이후 current source truth / 다음 작업
2. `REMAINING_IMPLEMENTATION_DESIGN_20260902.md` — 남은 기능 구현 순서/의존성 master
3. `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` — HomeDS402 frozen lifecycle + completion handoff
4. `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` — SetPosition durable runtime handoff
5. `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md` — SP-C0 current source inventory PASS evidence
6. `TOPOLOGY_STATIC_QUALIFICATION_RESULT_20260902.md` — TOPO-C0 source/network/generated-table static tranche PASS evidence
7. `HOME_DS402_H37_OPERATOR_ACTIVATION_IMPLEMENTATION_20260902.md` — Method 37 UI/source activation implementation and operator procedure

기존 상세 문서는 frozen wire/state-machine 또는 historical evidence로 계속 참조한다.

- `HOME_DS402_DESIGN.md`
- `SET_POSITION_DESIGN.md`
- `../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`
- `REMAINING_IMPLEMENTATION_DESIGN_20260901.md` — SDO Write 완료 이전 계획
- `SDO_WRITE_DETAILED_DESIGN_20260901.md`
- `SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`

문서가 충돌하면 current `dev` source와 `CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`를 우선한다.

---

## 1. current topology baseline

current Motion/LASAL topology는 logical axis와 physical drive를 분리한다.

```text
Logical axes              : Axis1..Axis9
Physical Elmo drives      : Axis1, Axis2
Configured physical mask  : 0x00000003
Simulation axes           : Axis3..Axis9
```

latest source changes:

- `b746252c...` — 2-drive startup/encoder-maintenance admission 대응
- `570fddd5...` — ownership service의 file-local `LMC_OWNER_STARTUP_LATCH_PHYSICAL` define 보완
- `5666497c...` — `SimulationSetup` class + Motion Network wiring 추가

`SimulationSetup`은 Axis1..9의 retentive 설정을 first scan에서 `_LMCAxisN.SimulateMode`에 즉시 전달한다.
Motion Network current configured value는 Axis1/2 = non-simulation, Axis3..9 = simulation이다.

따라서 이후 physical-drive-dependent 기능은 `axis <= 4` 같은 과거 가정이 아니라 configured physical mask를 사용한다.

상세 current override:

`CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`

---

## 2. 완료 기능

### SetOperationMode

상태: **IMPLEMENTATION COMPLETE / Active**

- `0x7D23 Start / 0x7D24 Outcome / 0x7D25 Retire`
- PP/PV/IP/CSP 지원
- exact requested-mode ACK
- one-shot `0x6060`
- read-only `0x6061` settling
- terminal owner publish/release
- durable no-replay outcome/retire
- WPF terminal/query/retire 처리

### Generic SDO Write

상태: **FEATURE IMPLEMENTATION COMPLETE**

- qualification proof 없이 direct manual Arm/Confirm
- canonical 1/2/4-byte scalar
- nonzero ObjectIndex generic policy
- baseline Read
- immutable two-click confirmation
- pre-Write guard
- journal v4
- identity-pinned one-shot submit
- terminal tracking
- mandatory exact readback
- no automatic replay

현재 physical topology는 Elmo Drive1/2만 존재하므로 physical SDO qualification/release evidence는 Slave1/2 기준으로 수행하고,
비물리 target은 deterministic unavailable 처리 여부를 TOPO-C0에서 확인한다.

---

## 3. P0-A — current topology freeze / regression

HomeDS402 구현을 계속하기 전에 `TOPO-C0`를 먼저 닫는다.

필수 확인:

- LASAL Compile/Rebuild 0 errors
- SimulationSetup first-scan 적용
- Axis1/2 `SimulateMode=0`
- Axis3..9 `SimulateMode=1`
- physical mask `0x03`이 InputLatch / Ownership / Diagnostics와 일치
- Axis3/4 EtherCAT absence가 ownership startup을 막지 않음
- Encoder Maintenance Axis1/2 정상 admission
- Encoder Maintenance Axis3/4 physical request 명시적 unavailable

이 단계는 activation이 아니라 topology baseline freeze다.

---

## 4. P0-B — HomeDS402

대상: No.19 `MMC_HomeDS402Cmd`

상태: **Method 37 source/UI activation implemented / fresh PLC image and hardware qualification required**

HomeDS402는 state machine 신규 구현 대상이 아니다. 기존 method37 lifecycle을 유지한다.

latest topology 변경이 `LMCEcatInputLatch`, `LMCControlCommandService`, `LMCDiagnosticsService`, Motion Network를 수정했으므로
기존 software qualification을 latest tree에서 다시 확인한다.

```text
TOPO-C0
-> H37-C0R current-dev regression on latest tree
-> H37-C1 fresh C78/generated artifact
-> H37-C2 activation candidate source/UI qualification
-> H37-C3 Axis1 hardware normal/failure matrix
-> H37-C4 Axis2 hardware + Axis3/4 nonphysical rejection matrix
-> H37-C5 five-value atomic activation runtime qualification
```

과거 `Axis2..4 hardware expansion` 문구는 current topology에서 그대로 적용하지 않는다.
physical HomeDS402 hardware qualification 대상은 Axis1/2다.

현재 tracked source의 activation values는 모두 ON이다.

- TCP ordinary ownership
- Control ordinary ownership
- Diagnostics HomeDS402 gate
- InputLatch startup sweep gate
- Admin capability bit 6

Admin HomeDS402 capability는 `0x00000757`의 bit 6이다. Diagnostics capability
`0x0000613F`의 bit 6은 RecorderDoubleBank이므로 Home 판정에 사용하지 않는다.
현재 실행 중 PLC/WPF가 이 변경 이전 image/process이면 capability를 새로 읽어도 사용할 수 없다.
fresh LASAL build/link/download와 WPF 재시작이 필요하다.

WPF는 현재 검증된 Method 37만 선택할 수 있다. 이는 축 이동 없이 현재 actual position을
0으로 정의하는 방식이다. switch/index를 찾는 이동형 homing은 `HomeDS402Ex`이며 아직 미구현/비활성이다.

상세 frozen lifecycle:

`HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

current topology override:

`CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`

---

## 5. P1 — SetPosition

대상: No.58 `MMC_SetPositionCmd`

상태: **Dormant / SP-C0 COMPLETE / SP-C1 NEXT**

SP-C0 current source inventory는 완료됐다.

- validating commit: `08e85d456ad118abaec8405fe9ab1c1ec3baa974`
- verifier: 39 checks PASS on the current shared-ownership state
- WPF AxisSetPositionJournal: 11/11 PASS
- evidence: `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md`

다음 단계는 SP-C1 prerequisite capture다.

```text
current-tree SP-C0 smoke
-> vendor CRC golden + IDE-generated _FileSys ABI
-> durable A/B backend
-> Store durable adapter
-> RT claim-before-native exactly-once executor
-> terminal-before-release integration
-> WPF recovery completion
-> C78/source qualification
-> Axis1/2 hardware + Axis3/4 simulation qualification
-> paired activation
```

외부 evidence 전 금지:

- vendor CRC 알고리즘 추측
- `_FileSys` generated ABI hand-authoring
- Store configured/ownership/max-jump/capability activation

Axis5..9가 SimulationSetup에 존재한다고 해서 SetPosition public contract를 자동 확장하지 않는다.

상세:

`SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

---

## 6. HomeDS402Ex

HomeDS402Ex는 별도 P2다.

- issue #28: wiring/polarity/method/scale/range profile
- issue #35: fresh C78/generated artifact + SourceOnly
- 이후 parameter SDO + RT physical homing runtime

current P0/P1이 닫히기 전 HomeDS402Ex activation은 OFF 유지한다.

---

## 7. 공통 구현 원칙

- `dev`가 유일한 current integration source truth
- logical axis와 physical drive를 분리
- physical capability는 configured physical mask 기준
- simulation PASS를 hardware PASS로 승격하지 않음
- mutation wire/native boundary 이후 original mutation replay 0
- terminal proof 전에 shared owner release 금지
- capability/gate source candidate와 runtime/production Active 판정을 구분함
- hardware qualification 전 source gate를 켠 예외는 exact build/download/runtime 증거 전까지 NO-GO 유지
- generated LASAL ABI/artifact/hash를 추측하지 않음
- source PASS / LASAL compile / C78 artifact / PLC boot / hardware PASS를 분리 기록
- topology 변경과 feature activation을 같은 changeset에 섞지 않음
- physical drive count가 바뀌면 TOPO-C0를 다시 수행

---

## 8. current 문서 우선순위

1. `CURRENT_IMPLEMENTATION_HANDOFF_20260902.md`
2. `REMAINING_IMPLEMENTATION_DESIGN_20260902.md`
3. `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
4. `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
5. `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md`
6. `../API_DEVELOPMENT_PROGRESS.md`
7. historical detailed design/evidence

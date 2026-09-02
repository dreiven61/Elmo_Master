# 최우선 API 개발 설계

- 기준일: 2026-09-02
- current integration / qualification source: `dev`
- current source baseline: `dev@90a86a795773d5f8eca211368aac3f0d64944a32` (`dev : SDO Write Func Complete`)
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- production release posture: **NO-GO**

## current implementation master

**2026-09-02 이후 신규 구현은 다음 문서를 정본으로 사용한다.**

1. `REMAINING_IMPLEMENTATION_DESIGN_20260902.md` — 남은 기능 구현 순서/의존성 master
2. `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` — current P0 HomeDS402 완료/활성화 handoff
3. `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` — current P1 SetPosition durable runtime handoff

기존 상세 문서는 frozen wire/state-machine 또는 historical evidence로 계속 참조한다.

- `HOME_DS402_DESIGN.md`
- `SET_POSITION_DESIGN.md`
- `../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`
- `REMAINING_IMPLEMENTATION_DESIGN_20260901.md` — SDO Write 완료 이전 계획
- `SDO_WRITE_DETAILED_DESIGN_20260901.md`
- `SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`

문서가 충돌하면 current `dev` source와 위 current implementation master 순서를 우선한다.

---

## 1. 완료 기능

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

current source baseline `90a86a7`에서 direct manual Generic SDO Write가 구현 완료됐다.

- qualification proof 없이 direct manual Arm/Confirm
- axis/slave 1..4
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

SDO의 추가 physical/release evidence는 신규 기능 구현 P0가 아니라 qualification/release backlog로 관리한다.

---

## 2. current P0 — HomeDS402

대상: No.19 `MMC_HomeDS402Cmd`

상태: **Dormant / core lifecycle implemented / activation pending**

HomeDS402는 state machine 신규 구현 대상이 아니다. 기존 method37 lifecycle을 유지하고 다음 순서로
완료한다.

```text
current-dev regression
-> fresh C78/generated artifact + SourceOnly
-> activation candidate OFF-state qualification
-> Axis1 hardware normal/failure matrix
-> Axis2..4 expansion
-> 5-value atomic activation
```

tracker: issue #32

현재 5개 activation value는 모두 OFF 유지한다.

- TCP ordinary ownership
- Control ordinary ownership
- Diagnostics HomeDS402 gate
- InputLatch startup sweep gate
- Admin capability bit 6

상세 정본:

`HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

---

## 3. current P1 — SetPosition

대상: No.58 `MMC_SetPositionCmd`

상태: **Dormant / durable runtime and exactly-once native execution missing**

이미 존재:

- SDK Start/Query/Retire
- `0x7D12/0x7D14/0x7D1A` wire
- volatile Store ABI
- observation-only RT preflight
- async lifecycle scaffold
- WPF recovery journal model
- deployment receipt/readback tooling 일부

실제 남은 구현 순서:

```text
current source inventory
-> vendor CRC golden + IDE-generated _FileSys ABI
-> durable A/B file backend
-> Store durable Begin/Commit/Retire adapter
-> RT claim-before-native exactly-once executor
-> stable terminal observer
-> terminal durable readback-before-owner-release
-> WPF restart/query/retire no-replay completion
-> source/C78 qualification
-> storage + Axis1..4 hardware matrix
-> paired activation
```

tracker: issue #44

현재 OFF 유지:

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED`
- ordinary ownership activation
- Axis1..4 `SetPositionMaxJump`
- Admin capability bits 3/5/7
- production native SetPosition execution

상세 정본:

`SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

---

## 4. HomeDS402Ex

`HomeDS402`와 `HomeDS402Ex`를 혼동하지 않는다.

HomeDS402Ex는 별도 P2다.

- issue #28: axis1..4 wiring/polarity/method/scale/range profile
- issue #35: fresh C78/generated artifact + SourceOnly
- 이후 parameter SDO + RT physical homing runtime 구현

현재 요청 범위의 우선순위는 HomeDS402와 SetPosition이다.

---

## 5. 공통 구현 원칙

- `dev`가 유일한 current integration source truth
- mutation wire/native boundary 이후 original mutation replay 0
- terminal proof 전에 shared owner release 금지
- capability/gate는 hardware qualification 전에 활성화하지 않음
- generated LASAL ABI/artifact/hash를 추측하지 않음
- source PASS / C78 PASS / PLC load / hardware PASS / production release를 분리 기록
- 기능 변경은 작은 tranche로 커밋하고 activation은 별도 commit으로 분리
- source SHA + generated artifact + PLC image + WPF/SDK identity를 같은 evidence set으로 기록

---

## 6. current 문서 우선순위

1. `REMAINING_IMPLEMENTATION_DESIGN_20260902.md`
2. `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
3. `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
4. `../API_DEVELOPMENT_PROGRESS.md`
5. `HOME_DS402_DESIGN.md` / `SET_POSITION_DESIGN.md`
6. architecture 상세 설계
7. historical implementation/result/evidence 문서

Generic SDO 관련 신규 구현 판단에는 source baseline `90a86a7` 이후 상태를 사용하고,
2026-09-01 문서의 SDO P0 표기를 current 우선순위로 사용하지 않는다.

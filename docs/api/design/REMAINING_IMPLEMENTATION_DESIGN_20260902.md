# 남은 API 구현 통합 설계 — 2026-09-02

- 기준 branch: `dev`
- source baseline: `dev@90a86a795773d5f8eca211368aac3f0d64944a32` (`dev : SDO Write Func Complete`)
- Generic SDO Write: **FEATURE IMPLEMENTATION COMPLETE**
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0: **HomeDS402 completion / activation**
- current P1: **SetPosition durable runtime implementation**
- production posture: **NO-GO**

이 문서는 2026-09-02 이후 남은 기능 구현의 current master다. 기존
`REMAINING_IMPLEMENTATION_DESIGN_20260901.md`는 SDO Write 완료 이전 계획이므로 historical snapshot으로
남기고, 신규 구현 작업은 본 문서와 아래 두 상세 설계를 우선한다.

- `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`
- `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

기존 frozen wire/state-machine 상세는 계속 다음 문서를 참조한다.

- `HOME_DS402_DESIGN.md`
- `SET_POSITION_DESIGN.md`
- `../../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`

---

## 1. current 상태 재분류

| 우선순위 | 기능 | current 상태 | 실제 남은 개발 |
|---:|---|---|---|
| 완료 | SetOperationMode | Active | release regression만 유지 |
| 완료 | Generic SDO Write | 구현 완료 | physical/release evidence는 별도 qualification backlog |
| P0 | HomeDS402 | Dormant | fresh artifact -> hardware -> 5-gate atomic activation |
| P1 | SetPosition | Dormant | durable A/B backend -> RT exactly-once -> recovery -> activation |
| P2 | HomeDS402Ex | Dormant | profile/artifact prerequisite 뒤 physical runtime |
| P3 | 기타 dormant/missing | Dormant/Missing | DigitalIO/DO/PI/Double/Extended SDO 등 |

Generic SDO를 더 이상 신규 구현 P0로 두지 않는다.

---

## 2. 공통 mutation 원칙

HomeDS402와 SetPosition 모두 다음을 지킨다.

1. original mutation automatic replay 0
2. current session/build/BootId/MapRevision identity 확인
3. shared owner를 terminal proof 전에 release하지 않음
4. response loss는 status/query/recovery로 처리
5. source 구현과 hardware PASS를 구분
6. capability/gate는 hardware qualification 전까지 OFF
7. generated LASAL ABI/artifact/hash를 추측하거나 blind ratchet하지 않음
8. 기존 SetOperationMode/Generic SDO executor 계약을 약화하지 않음

---

# 3. P0 — HomeDS402

HomeDS402는 core state machine 구현이 끝난 기능이다. 다음 개발자는 새로운 Home engine을 만들지 않는다.

실행 순서:

```text
H37-C0 current-dev regression
-> H37-C1 fresh C78/generated artifact + SourceOnly
-> H37-C2 activation candidate OFF-state qualification
-> H37-C3 Axis1 hardware normal/failure
-> H37-C4 Axis2..4 expansion
-> H37-C5 five-value atomic activation
```

핵심 tracker는 issue #32다.

### 개발자가 바로 할 일

- current dev에서 4개 H37 verifier 재실행
- SDO Write 변경 이후 shared executor regression 확인
- activation candidate test/bit6 WPF gating 확인
- evidence collector/source verifier 보강이 필요한지 판단

### 사용자/LASAL IDE가 필요한 경계

- fresh C78/ARM Rebuild + Link
- generated artifact review
- PLC download
- actual axis hardware matrix

해당 evidence가 없으면 gate를 켜지 않는다.

상세: `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

---

# 4. P1 — SetPosition

SetPosition은 wire/lifecycle scaffold는 있지만 production mutation engine이 미완료다.

실행 순서:

```text
SP-C0 current source inventory
-> SP-C1 CRC golden + IDE _FileSys ABI
-> SP-C2 durable A/B backend
-> SP-C3 Store durable adapter
-> SP-C4 RT claim-before-native exactly-once executor
-> SP-C5 terminal-before-release integration
-> SP-C6 WPF recovery completion
-> SP-C7 source/C78 qualification
-> SP-C8 storage/axis hardware matrix
-> SP-C9 paired activation
```

핵심 tracker는 issue #44다.

### 개발자가 지금 할 수 있는 작업

- current source inventory / duplicate implementation 방지
- Store serialization/regression tests 정리
- host deployment receipt tools regression
- WPF recovery integration inventory/tests
- `_FileSys` backend interface seam와 test fixture 구조 준비
- RT mailbox/one-shot verifier 설계 준비

### 외부 evidence 전 금지

- vendor CRC 알고리즘 추측
- `_FileSys` generated ABI hand-authoring
- A/B image generator 완료 판정
- Store configured/ownership/max-jump/capability activation

상세: `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md`

---

# 5. HomeDS402Ex 위치

사용자가 현재 요청한 `HomeDS402`는 method37 current-position-zero 기능이며 `HomeDS402Ex`와 구분한다.

HomeDS402Ex는 다음 prerequisite가 따로 남는다.

- issue #28: axis1..4 wiring/polarity/method/scale/range profile
- issue #35: fresh C78/generated artifact/SourceOnly

P0 HomeDS402와 P1 SetPosition을 진행하는 동안 HomeDS402Ex activation은 OFF로 유지한다.

---

# 6. branch / commit 정책

- integration source truth는 `dev`
- 기능별 변경은 작은 tranche로 커밋
- generated artifact/hash update는 evidence commit과 분리하지 않음
- activation commit은 구현 commit과 분리
- temporary workflow/helper는 검증 뒤 제거
- source SHA / LASAL artifact / PLC image / WPF binary identity를 같은 evidence set으로 기록

권장 commit 단위:

### HomeDS402

```text
test(h37): rebaseline HomeDS402 on current dev
chore(h37): record fresh C78 artifact evidence
fix(h37): <only if hardware evidence finds defect>
feat(h37): atomically activate HomeDS402
```

### SetPosition

```text
test(setposition): freeze current source inventory
feat(setposition): add durable file backend
feat(setposition): connect durable store lifecycle
feat(setposition): add RT exactly-once executor
feat(setposition): enforce terminal-before-release
feat(wpf): complete SetPosition recovery interlock
test(setposition): qualify storage and runtime
feat(setposition): atomically activate SetPosition
```

---

# 7. completion definition

## HomeDS402 implementation complete

- fresh current-tree artifact closure
- Axis1~4 normal/fault/timeout/disconnect/response-loss matrix
- query/retire no-replay recovery
- five activation values atomic ON
- WPF/API/C78/PLC same-image evidence

## SetPosition implementation complete

- vendor/IDE prerequisites reviewed
- durable A/B Store cold-cycle evidence
- native SetPosition logical call exactly once
- stable observer
- terminal durable readback-before-release
- WPF restart/query/retire no-replay
- Axis1~4 approved correction matrix
- Store/ownership/max-jump/capability paired activation

전체 production release는 이 두 기능 완료만으로 자동 승인하지 않는다. Distribution/release gate는 별도다.

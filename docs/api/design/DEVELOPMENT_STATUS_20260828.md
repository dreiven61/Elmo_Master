# 개발 상태 스냅샷 — 2026-08-28

- current production branch: `dev`
- current source baseline: `dev@996e7df0be215f3c485612427ccfd3db76c30b98`
- production 판정: **NO-GO**
- current P0 tracking: issue #46

이 문서는 2026-08-28 기준으로 **현재까지 실제로 통합된 내용**과 **앞으로 닫아야 할 qualification/release gate**를 분리해 기록한다. 여기서 source baseline은 MODE-11E/11F가 통합되고 current-state 설계가 동기화된 코드/설계 기준점이며, 이후 문서-only commit은 구현 완료 판정을 바꾸지 않는다. source 구현, PC/static test, generated artifact, C78 build, PLC load, hardware/packet evidence와 production activation은 서로 다른 완료 단계로 취급한다.

---

## 1. 현재 요약

| 영역 | release-oriented 진행도 | current `dev` 상태 | 핵심 남은 gate |
|---|---:|---|---|
| HomeDS402 | 50% | H37 software/source/WPF qualification 완료, activation OFF | fresh C78/artifact -> Axis1 -> Axis2..4 -> activation |
| SetOperationMode | **65%** | PP/PV/IP/CSP multi-mode source + MODE-11E/11F + durable recovery, production gate OFF | current-image C78/PLC -> Axis1 physical matrix -> failure matrix -> Axis2..4 -> activation |
| HomeDS402Ex | 40% | SDK/retained store/ownership/WPF recovery/source-static 존재, physical runtime OFF | hardware profile + fresh artifact/C78 -> physical runtime/hardware -> activation |
| SetPosition | 25% | lifecycle + WPF durable recovery 존재, native runtime dormant | durable A/B backend -> RT exactly-once -> C78/PLC/hardware -> activation |
| Generic SDO | redesign 진행 중 | `LMCSdoExecutor` manual/programmatic dual-entry source 통합 | manual bench closure -> SDO-R03/R04/R05 |

진행률은 checklist 단순 비율이 아니다. 특히 SetOperationMode는 software/source 구현이 약 80% 수준이지만 PP/PV/IP actual transition과 current-image hardware evidence가 없으므로 release-oriented 수치는 65%로 유지한다.

---

## 2. 2026-08-28까지 진행 완료된 핵심 작업

### 2.1 SetOperationMode multi-mode source

현재 `dev`에는 다음이 통합돼 있다.

- PP(1), PV(3), IP(7), CSP(8) requested-mode path
- `0x6061` preflight -> 필요 시 requested mode exact 1-byte `0x6060` write -> `0x6061` exact verify
- same-mode zero-write
- cross-mode safe-state preflight
  - PhysicalValid
  - Standstill
  - Fault clear
  - OperationEnabled clear
  - owner / SDO conflict clear
- immutable `0x7D23 Start` / `0x7D24 ReadOutcome` / `0x7D25 Retire`
- write-dispatch 이후 original Start/`0x6060` no-replay
- safety Stop/Power preemption cleanup/quarantine
- generic raw D5 `0x6060` permanent deny
- PLC/SDK/WPF `SetOperationModeSupportedMask` contract
- WPF selector + live PLC capability/mask fail-closed Start admission
- MODE-13 durable WPF startup/reconnect no-replay recovery

### 2.2 MODE-11E 완료

`dev@10e7ba11e99770d1d62988007df6ed444604b33f`에서 PP/PV/IP/CSP warm-start recovery를 일반화했다.

완료 내용:

- CSP-only warm-start candidate 제거
- PP/PV/IP/CSP requested mode 복구 후보 지원
- exact record/admission/owner/session/sequence identity fence 강화
- multi-candidate fail-closed
- write-dispatch 이후 recovery에서 `0x6060` write 0회 유지
- exact `0x6061` read-only 확인만으로 terminal 판정

이 단계의 PASS는 source/static/PC regression 증거이며 physical activation 근거는 아니다.

### 2.3 MODE-11F 완료

`dev@45fc528c48723cbe3ba20fb11c3a2d7ec0e7ef0b`에서 WPF preflight/rejection/write evidence diagnostics를 통합했다.

표시 항목:

- Requested / Preflight(Previous) / Observed mode
- CommandStatus / ErrorId / symbolic+numeric DetailCode
- Build / BootId / MapRevision
- DS402 StatusWord / Fault / OperationEnabled / PhysicalValid
- EvidenceFlags
- WriteRequested / WriteDispatched
- VerifyReadDispatched / VerifyReadCompleted
- OwnerReleased / ExecutorReusable
- ContextCheck / QuarantineReason / RecordGeneration

Korean/English localization round-trip과 WPF Debug/Release recovery workflow가 통과했다.

### 2.4 설계/current-state 동기화

`dev@996e7df0be215f3c485612427ccfd3db76c30b98`에서 MODE-11E/11F 이후 SetOperationMode current-state와 설계 인덱스를 동기화했다.

현재 production boundary는 그대로다.

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- Admin capability bits 8/9/10 OFF
- production `SetOperationModeSupportedMask = 0x0000`
- 신규 SetOperationMode mutation은 production에서 dormant

---

## 3. SetOperationMode에서 아직 완료되지 않은 것

### 3.1 qualification baseline 재생성

기존 `codex/setopmode-mode11-bench-activation@eae31dd...`는 current `dev`와 diverged되어 다음 실기 기준으로 사용할 수 없다.

다음 qualification branch는 반드시 current `dev@996e7df0...` 또는 그 후속 exact current source head에서 새로 만든다.

허용되는 production 대비 delta는 원칙적으로 다음뿐이다.

1. `LMC_DIAG_SET_OPERATION_MODE_ENABLED = TRUE`
2. Admin bits 8/9/10 ON
3. 시험할 mode만 `SetOperationModeSupportedMask`에 명시

implementation/source stale delta가 섞이면 qualification image로 인정하지 않는다.

### 3.2 current-image build/load evidence

아직 필요한 것:

- LASAL IDE generated source / `Classes.lcb` current source 기준 재생성
- fresh C78/ARM Rebuild + Link
- generated artifact identity 기록
- exact PLC image download/load
- Build/BootId/MapRevision 기록
- WPF same-tree build

과거 MODE-10 C78 또는 stale qualification branch 결과는 current-image 증거로 승격하지 않는다.

### 3.3 Axis1 정상 mode matrix

다음 순서로 packet/readback evidence를 확보한다.

- MODE-11A: CSP -> CSP same-mode, `WriteRequested=0`, `WriteDispatched=0`
- MODE-11B: CSP -> PP -> CSP
- MODE-11C: CSP -> PV -> CSP
- MODE-11D: CSP -> IP -> CSP

cross-mode 각 transition에서 다음을 확인한다.

- requested mode와 exact 1-byte `0x6060` write value 일치
- successful intent당 irreversible write 1회 이하
- `0x6061` exact requested-mode readback 후에만 success
- duplicate/recovery에서 추가 `0x6060` write 0회
- WPF displayed evidence와 packet/outcome 일치

### 3.4 failure/recovery matrix

MODE-12A에서 Axis1 기준 다음을 닫는다.

- unsafe-state rejection / zero-wire
- timeout before dispatch
- timeout after dispatch
- reconnect / response loss
- verify mismatch
- executor/source mismatch
- owner loss / safety preemption
- quarantine
- exact-generation retire / retire retry
- restart/warm-start recovery

### 3.5 확장 및 release

Axis1 PASS 후에만 다음을 진행한다.

- MODE-11G mode별 qualification evidence ledger
- qualification mask와 future production mask coupling
- MODE-12B Axis2..4 확대
- MODE-14 capability bits 8/9/10 + production-qualified mask paired activation
- distribution/manual/WPF/artifact release sync

---

## 4. Generic SDO 현재 상태와 다음 작업

### 현재 완료

`LMCSdoExecutor` dual-entry source가 `dev`에 통합돼 있다.

- manual Server entry
- tokenized programmatic entry
- shared arbitration
- callback source dispatch
- manual `ClassState` / `ErrorCode` / `ParaLength` publication
- focused source verifier
- C78 rebuild/link 및 PLC basic smoke의 과거/current tranche evidence 존재

### 아직 미완료

manual/programmatic source 구현을 generic arbitrary Write 완료로 해석하지 않는다.

현재 남은 순서:

1. manual Server physical bench qualification closure
2. SDO-R03 exact target allowlist 제거 + capability/request-validity/owner 기반 generic Write policy
3. 1/2/4-byte arbitrary Write tests
4. semantic reserved object deny policy 유지 (`0x6060` 포함)
5. SDO-R04 WPF arbitrary-target editor
6. SDO-R05 exact-request durable no-replay recovery

SetOperationMode semantic lifecycle과 generic raw SDO Write는 계속 분리한다.

---

## 5. 다른 P0 API의 current boundary

### HomeDS402

완료:

- H37 five-gate static contract
- lifecycle PC contract
- ownership/preemption
- method-size/source qualification
- WPF durable no-replay recovery

남음:

- fresh generated artifact/C78 H37-05/06
- Axis1 H37-07 hardware/packet
- Axis2..4 H37-08
- H37-09 paired activation

Admin bit 6과 activation values는 OFF다.

### HomeDS402Ex

완료된 software tranche:

- wire/SDK lifecycle
- retained full outcome store
- full owner identity
- approved-plan preparation
- source/static collector
- WPF durable recovery

남음:

- issue #28 actual axis hardware profile approval
- issue #35 same-tree fresh artifact/C78 closure
- parameter/mode/controlword/homing physical runtime
- HOMEEX hardware matrix
- bit 11 activation

### SetPosition

현재:

- SDK/wire/route/P1 lifecycle
- WPF SP-04A durable no-replay recovery

남음:

- `_FileSys` fixed dual-file A/B durable backend
- vendor CRC / generated ABI prerequisite
- RT claim/native exactly-once execution
- C78/PLC/hardware
- capability activation

---

## 6. 다음 작업 우선순위

현재 P0 실행 순서는 다음으로 고정한다.

```text
1. SetOperationMode current-dev qualification branch 재생성
2. activation delta-only diff 확인
3. current-source LASAL generated artifact + C78/ARM rebuild/link
4. exact PLC load/image identity 확보
5. MODE-11A Axis1 CSP same-mode zero-write
6. MODE-11B/C/D Axis1 PP/PV/IP round-trip
7. MODE-12A Axis1 failure/recovery matrix
8. MODE-11G mode별 evidence-mask coupling
9. MODE-12B Axis2..4 확대
10. MODE-14 paired production activation review
11. Generic SDO manual bench closure
12. SDO-R03 -> R04 -> R05
13. HomeDS402 / HomeDS402Ex / SetPosition remaining physical/runtime gates
14. distribution/manual/WPF/docs/artifact release sync
```

실장비 접근이나 C78 작업 순서 때문에 11~13의 일부를 병렬로 수행할 수는 있지만, 각 API의 activation gate를 건너뛰지는 않는다.

---

## 7. 공통 Definition of Done

API를 `Active` 또는 production 완료로 올리려면 최소 다음 evidence를 같은 승인 세트에서 연결한다.

1. public/model/wire contract
2. malformed/golden/PC tests
3. LASAL parser/runtime source
4. ownership/arbitration
5. durable no-replay/recovery
6. source/static/method-size
7. IDE-generated artifact identity
8. fresh C78/ARM build/link
9. exact PLC load identity + Build/BootId/MapRevision
10. hardware/packet normal + negative/fault/restart matrix
11. mode/feature별 evidence와 advertised capability/mask 일치
12. distribution/manual/WPF/docs sync
13. paired production activation review

한 단계의 PASS를 다음 단계의 PASS로 추정하지 않는다.

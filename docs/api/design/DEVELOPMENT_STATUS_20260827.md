# 개발 상태 스냅샷 — 2026-08-27

이 문서는 `dev@5e6fd2f2a98d8e970fb85ecd7754ae76c9b999f0` 기준의 최우선 API 설계/구현 진행 상태를 정리한다.

설계 완료, PC/SDK 구현, LASAL source/static qualification, IDE/generated artifact, PLC load/runtime,
hardware/packet qualification과 production activation은 서로 다른 gate다. 아래 진행도는 기존 문서의
release-oriented 수치를 유지하며, 체크리스트 완료 개수와 동일한 의미로 사용하지 않는다.

## 1. current baseline

- 기준 branch/HEAD: `dev@5e6fd2f2a98d8e970fb85ecd7754ae76c9b999f0`
- production 판정: **NO-GO**
- HomeDS402 activation 5-gate: OFF
- SetOperationMode `LMC_DIAG_SET_OPERATION_MODE_ENABLED`: `FALSE`
- SetOperationMode Admin bits 8/9/10: OFF
- HomeDS402Ex `LMC_DIAG_DS402_HOME_EX_ENABLED`: `FALSE`
- HomeDS402Ex Admin bit 11: OFF
- HomeDS402Ex actual axis 1..4 profile approval: issue #28 OPEN
- HomeDS402Ex fresh C78/generated-artifact closure: issue #35 OPEN

`dev`에는 2026-08-26 기준 HomeDS402Ex retained outcome store, C78 evidence collector,
distribution WPF 동기화와 최근 recovery-panel Korean localization이 추가로 통합됐다.

## 2. 최우선 API 진행도

| API | 문서 진행도 | software/source 상태 | current production gate | 다음 핵심 작업 |
|---|---:|---|---|---|
| `HomeDS402` | 50% | existing method-37 source/runtime 존재. Draft PR #31에서 H37-02/03/04/10 software qualification 완료했으나 `dev` 미통합 | Dormant / activation 5-gate OFF | PR #31 정리 후 fresh SourceOnly/C78 artifact, Axis1 hardware matrix |
| `SetOperationMode` | 60% | MODE-02/06/07/08/09 source, MODE-10 source/static, MODE-13 WPF durable recovery 완료. PR #17 fresh C78 artifact checkpoint와 PR #18 bench tooling/evidence 준비 존재 | Dormant / compile gate FALSE / bits 8..10 OFF | current `dev` exact-image C78 재검증, MODE-11/12 physical evidence, MODE-14 activation |
| `HomeDS402Ex` | 40% | HOMEEX-03/04/05/06/07/12 완료, HOMEEX-08 preparation partial, HOMEEX-09 source/static + collector partial | Dormant / runtime no-op / bit 11 OFF | issue #28 profile 승인 + issue #35 fresh C78 closure 후 actual SDO/homing runtime 검토 |
| `SetPosition` | 25% | P1 async lifecycle/volatile Store/retirement contract 존재, runtime fail-closed | Dormant | durable A/B backend + RT exactly-once/native execution evidence |

### 판정 해석

- `software/source 상태`가 높아도 C78/PLC/hardware evidence가 없으면 production readiness로 승격하지 않는다.
- Draft PR 또는 qualification branch에서 PASS한 항목은 `dev`에 merge되기 전까지 current 완료 체크박스로 바꾸지 않는다.
- 한 API의 C78/hardware PASS를 다른 API의 current source evidence로 재사용하지 않는다.

## 3. HomeDS402 current boundary

`dev`의 설계 체크리스트는 H37-01만 완료 상태로 유지한다.

Draft PR #31 `codex/home-ds402-h37-source-qualification`에서는 다음 software qualification이 확보됐다.

- H37-02 atomic 5-value activation contract: 43 checks PASS
- H37-03 exact `0x7D15/0x7D16/0x7D17` PC Start/Outcome/Retire sequence PASS
- H37-04 shared axis ownership/preemption regression: 21 checks PASS
- HomeDS402 method-size verifier: 10 checks PASS
- Debug/Release API full suite: 1194/1194 PASS
- WPF H37 recovery smoke PASS
- H37-10 durable WPF recovery qualification PASS

하지만 PR #31은 **OPEN Draft / 미병합**이다. 따라서 `HOME_DS402_DESIGN.md`의 current 체크박스는
완료로 올리지 않고 branch-qualified 상태를 별도 표시한다.

잔존 blocker:

`LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`

즉 H37-05/06 fresh generated artifact와 C78 closure, H37-07/08 hardware matrix, H37-09 activation은 미완료다.

## 4. SetOperationMode current boundary

현재 `dev`에는 다음 software/source 계약이 존재한다.

- OwnerKind 6 / Diagnostics SDO ResourceKind 4 / active state 12
- exact 56-byte Start identity
- `6061 -> 6060 -> 6061` runtime
- same-mode zero-write path
- irreversible write-dispatch 이후 read-only no-replay recovery
- safety preemption cleanup/quarantine
- generic D5 `0x6060` permanent deny
- MODE-10 three-way processor split + source/static verifier
- MODE-13 WPF pre-dispatch durable journal + startup/reconnect no-replay recovery

PR #17에서 fresh C78 artifact checkpoint와 generated artifact identity를 기록했고, PR #19는 PR #18의
MODE-11 software-side bench/evidence 준비 상태를 `dev` 문서에 기록했다. 다만 이후 HomeDS402Ex
retained-store/source 변경이 `dev`에 추가됐으므로 SetOperationMode production activation 전에는
**현재 exact `dev` source tree 기준 fresh C78/PLC evidence를 다시 묶어야 한다.**

Open Draft PR #18은 계속 `DO NOT MERGE` physical bench qualification branch다.

남은 gate:

1. current exact source C78/ARM rebuild/link + artifact identity review
2. MODE-11A CSP same-mode zero-write packet/hardware evidence
3. MODE-11B independently approved non-CSP exact-one-write/readback evidence
4. MODE-12 axis 1..4 timeout/disconnect/mismatch/quarantine/retire matrix
5. MODE-14 paired capability activation

## 5. HomeDS402Ex current boundary

`dev`에 통합된 software tranche:

- HOMEEX-03 wire/capability contract
- HOMEEX-04 approved-plan C# lifecycle gate
- HOMEEX-05 exact retained outcome/recovery/retire software store
- HOMEEX-06 gate-OFF LASAL parser/state scaffold
- HOMEEX-07 full 116-byte owner identity + shared DS402 Home engine admission
- HOMEEX-12 WPF durable no-replay recovery
- HOMEEX-08 approved-profile -> frozen DINT preparation gate
- HOMEEX-09 source/static verifier + C78 evidence collector

PR #33 retained store 결과:

- `Ds402HomeExState[0..319]`
- 4 x 40-DINT active records
- 4 x 40-DINT retired full-outcome records
- 176-byte Outcome/Retire exact serialization
- duplicate/replay blocking
- exact-generation Retire retry
- retained-store verifier 48/48 PASS

PR #34는 fresh C78/ARM evidence collector를 통합했다. collector 자체는 PASS지만 real LASAL rebuild evidence는
아직 없고 issue #35가 OPEN이다.

PR #36은 위 상태를 설계문서에 동기화했다.

현재 actual physical runtime은 계속 미개방이다.

- parameter snapshot/program/restore 미구현
- mode 6 / controlword bit 4 physical execution 미구현
- RT owner + homing observation 미구현
- CleanupProofFlags production proof 미구현
- capability bit 11 OFF
- WPF Start UI OFF

### external blockers

- issue #28: axis 1..4 wiring/polarity/method allowlist/scale/range/rounding/MapRevision actual approval
- issue #35: same-tree fresh C78/generated artifact review + SourceOnly ratchet closure

두 blocker 전에는 HOMEEX-08 physical runtime을 열지 않는다.

## 6. WPF distribution / localization current state

`dev`에는 다음 UI/distribution 작업이 추가됐다.

- `eac82efb52bf9ce58aa52bc9360a8b4a0d85bd24` — distribution example + Korean UI sync
- `52bd4cc120812c2510f8ac99d2d6a42576133d67` — recent recovery panels localization
- `5e6fd2f2a98d8e970fb85ecd7754ae76c9b999f0` — dynamic recovery panel traversal, Korean label alignment, and WPF recovery localization regression coverage

Open PR #37 `codex/wpf-recent-recovery-localization-coverage`는 SetOperationMode/HomeDS402Ex의 동적
recovery panel에서 남은 Korean label coverage와 CI path gap을 보완하는 후속 작업이다.

PR #37의 오래된 branch는 아직 미병합이다. 해당 범위는 current `dev`에 재적용됐고 local Debug/Release
SetOperationMode 13/13, HomeDS402Ex 12/12 WPF recovery smoke가 PASS했다. 이 source-only 결과는
GitHub workflow 결과나 설계/프로토콜/PLC runtime 완료를 뜻하지 않는다. `dev`의 no-replay recovery
semantics와 capability activation 상태에는 변화가 없다.

## 7. open work / branch 상태

current open PR 기준:

- PR #14 `codex/mode-10-c78-finalize` — C78 history/review branch
- PR #18 `codex/setopmode-mode11-bench-activation` — `DO NOT MERGE`, physical bench evidence
- PR #31 `codex/home-ds402-h37-source-qualification` — Draft, H37 software qualification
- PR #37 `codex/wpf-recent-recovery-localization-coverage` — WPF localization/CI follow-up

merged 작업 branch가 원격에 남아 있더라도 current 설계 상태의 기준은 항상 `dev`다.
branch 존재 자체를 feature completion evidence로 사용하지 않는다.

## 8. 다음 작업 우선순위

### 우선순위 A — current blocker 해소

1. current `dev` WPF localization 변경의 GitHub workflow 결과를 기록하고 PR #37 branch 처리 경계를 정리
2. PR #31 H37 software qualification 내용을 `dev` 기준으로 재검토하고 merge 가능 상태 정리
3. issue #35 HomeDS402Ex fresh LASAL C78/generated artifact evidence 수집
4. issue #28 HomeDS402Ex axis 1..4 hardware profile 승인

### 우선순위 B — physical qualification

1. HomeDS402 Axis1 H37-07 matrix
2. SetOperationMode MODE-11/12 Axis1 evidence
3. HomeDS402Ex HOMEEX-08 actual runtime implementation은 issue #28/#35 완료 후에만 진행
4. HomeDS402Ex HOMEEX-10 Axis1 matrix
5. 각 API Axis2~4 확대

### 우선순위 C — activation

- HomeDS402 H37-09 five-gate paired activation
- SetOperationMode MODE-14 bits 8/9/10 paired activation
- HomeDS402Ex HOMEEX-13 bit 11 + WPF Start UI paired activation

각 activation은 해당 API의 current source/image/hardware proof가 모두 같은 승인 세트일 때만 허용한다.

## 9. 결론

2026-08-27 기준 `dev`는 HomeDS402Ex의 hardware-independent wire/ownership/profile-gate/retained-store/
WPF recovery/source-static 준비와 SetOperationMode의 source/no-replay/WPF recovery 준비가 상당 부분 통합된
상태다. HomeDS402는 별도 Draft PR #31에서 source/PC qualification이 진행됐다.

그러나 세 API 모두 production activation에 필요한 current C78/PLC/hardware gate가 남아 있다.
특히 HomeDS402Ex는 issue #28과 #35가 모두 닫히기 전까지 physical runtime을 열지 않는다.
따라서 current production 판정은 계속 **NO-GO**다.

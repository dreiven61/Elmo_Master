# 개발 상태 스냅샷 — 2026-08-27

이 문서는 `dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd` 기준의 최우선 API 설계/구현 진행 상태를 정리한다.

설계 완료, PC/SDK 구현, LASAL source/static qualification, IDE/generated artifact, PLC load/runtime,
hardware/packet qualification과 production activation은 서로 다른 gate다. 아래 진행도는 기존 문서의
release-oriented 수치를 유지하며, 체크리스트 완료 개수와 동일한 의미로 사용하지 않는다.

## 1. current baseline

- 기준 branch/HEAD: `dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd`
- production 판정: **NO-GO**
- HomeDS402 activation 5-gate: OFF
- HomeDS402 Admin bit 6: OFF
- SetOperationMode `LMC_DIAG_SET_OPERATION_MODE_ENABLED`: `FALSE`
- SetOperationMode Admin bits 8/9/10: OFF
- HomeDS402Ex `LMC_DIAG_DS402_HOME_EX_ENABLED`: `FALSE`
- HomeDS402Ex Admin bit 11: OFF
- HomeDS402Ex actual axis 1..4 profile approval: issue #28 OPEN
- HomeDS402Ex fresh C78/generated-artifact closure: issue #35 OPEN

2026-08-27 current `dev`에는 WPF dynamic recovery localization의 GitHub CI qualification과
HomeDS402 H37 hardware-independent source/PC/WPF qualification이 추가 통합됐다.

## 2. 최우선 API 진행도

| API | 문서 진행도 | software/source 상태 | current production gate | 다음 핵심 작업 |
|---|---:|---|---|---|
| `HomeDS402` | 50% | H37-01/02/03/04/10 current `dev` 완료. activation/ownership/method-size/WPF no-replay qualification green | Dormant / five-gate + bit 6 OFF | H37 fresh C78 artifact evidence와 H37-05/06 closure 후 Axis1 matrix |
| `SetOperationMode` | 65% | 기존 lifecycle/no-replay + PP/PV/IP/CSP software target SDK/PLC/WPF path 통합; MODE-10 static, MODE-13 durable recovery 유지 | Dormant / compile gate FALSE / bits 8..10 OFF | SupportedModeMask, current exact-image C78, MODE-11/12 physical evidence, MODE-14 activation |
| `HomeDS402Ex` | 40% | HOMEEX-03/04/05/06/07/12 완료, HOMEEX-08 preparation partial, HOMEEX-09 source/static + collector partial, WPF localization CI green | Dormant / physical runtime no-op / bit 11 OFF | issue #28 profile 승인 + issue #35 fresh C78 closure 후 actual SDO/homing runtime 검토 |
| `SetPosition` | 25% | P1 async lifecycle/volatile Store/retirement contract 존재, runtime fail-closed | Dormant | durable A/B backend + RT exactly-once/native execution evidence |

### 판정 해석

- software/source 상태가 높아도 C78/PLC/hardware evidence가 없으면 production readiness로 승격하지 않는다.
- current 완료는 `dev`에 merge되고 해당 evidence grade에서 qualification된 것만 표시한다.
- 한 API의 C78/hardware PASS를 다른 API의 current source evidence로 재사용하지 않는다.

## 3. HomeDS402 current boundary

PR #40 `test(h37): qualify HomeDS402 source and recovery on current dev`가
`1f741bfd08e9d75a52f7edd03862ef26ac562edd`로 `dev`에 squash merge됐다.

current 완료:

- H37-01 receipt/safety-drain 문서/source 정합
- H37-02 five-value atomic activation contract: **43 checks PASS**
- H37-03 exact `0x7D15/0x7D16/0x7D17` PC lifecycle contract PASS
- H37-04 shared ownership/preemption contract: **21 checks PASS**
- H37-10 WPF durable no-replay recovery: **36 checks PASS** + restart reconstruction smoke PASS
- HomeDS402 method-size: **10 checks PASS**
  - largest checked method `ProcessAxisDs402Home`: 29,497 bytes < 32,768
- API Debug/Release full suites PASS
- WPF Debug/Release H37 smoke PASS
- diff hygiene PASS

qualification evidence:

- PR #40 qualified head `f39fe0e9b56b0994619aed3f68b22c33a86d3b24`
- workflow run `33026506170`
- successful rerun job `98369296568`

full SourceOnly source/static gate는 통과했으며 다음 exact generated-artifact boundary에 도달한다.

`LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`

따라서 아직 미완료:

- H37-05 fresh generated-artifact ratchet closure
- H37-06 fresh C78/ARM rebuild/link + direct-open/network smoke
- H37-07 Axis1 hardware/packet matrix
- H37-08 Axis2~4 matrix
- H37-09 paired activation

HomeDS402 capability bit 6과 다섯 activation value는 계속 OFF다.

## 4. SetOperationMode current boundary

현재 `dev`에는 다음 software/source 계약이 존재한다.

- OwnerKind 6 / Diagnostics SDO ResourceKind 4 / active state 12
- exact 56-byte Start identity
- SDK/WPF software target allow-list PP(1)/PV(3)/IP(7)/CSP(8)
- PLC requested-mode-driven `6061 -> 6060 -> 6061` dormant runtime
- requested-mode same-mode zero-write path
- irreversible write-dispatch 이후 read-only no-replay recovery
- safety preemption cleanup/quarantine
- generic D5 `0x6060` permanent deny
- MODE-10 three-way processor split + source/static verifier
- MODE-13 WPF pre-dispatch durable journal + startup/reconnect no-replay recovery

WPF dynamic recovery localization은 PR #39에서 actual dynamic contents의 Korean -> English -> Korean
round-trip을 GitHub Actions로 qualification했다.

- SetOperationMode WPF recovery run `33026189333`: SUCCESS
- HomeDS402Ex WPF recovery run `33026189342`: SUCCESS
- PR #39 merge commit `64ff6dfb2d6f7bc3554436ae3ea686e74509b4d4`

stale PR #37은 superseded로 CLOSED / unmerged다.

SetOperationMode production activation 전에는 current exact source tree 기준 fresh C78/PLC evidence가
필요하다. Open Draft PR #18은 계속 `DO NOT MERGE` physical bench qualification branch다.

남은 gate:

1. PLC-advertised SupportedModeMask + WPF selector intersection/fail-closed
2. current exact source C78/ARM rebuild/link + artifact identity review
3. MODE-11 same-mode zero-write / cross-mode exact-one-write packet-hardware evidence
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

retained store:

- `Ds402HomeExState[0..319]`
- 4 x 40-DINT active records
- 4 x 40-DINT retired full-outcome records
- 176-byte Outcome/Retire exact serialization
- duplicate/replay blocking
- exact-generation Retire retry
- retained-store verifier 48/48 PASS

현재 actual physical runtime은 계속 미개방이다.

- parameter snapshot/program/restore 미구현
- mode 6 / controlword bit 4 physical execution 미구현
- RT owner + homing observation 미구현
- CleanupProofFlags production proof 미구현
- capability bit 11 OFF
- WPF Start UI OFF

external blockers:

- issue #28: axis 1..4 wiring/polarity/method allowlist/scale/range/rounding/MapRevision actual approval
- issue #35: same-tree fresh C78/generated artifact review + SourceOnly ratchet closure

두 blocker 전에는 HOMEEX-08 physical runtime을 열지 않는다.

## 6. current open work / stale branch 정리

정리 완료:

- PR #37 WPF localization branch: superseded by current `dev` + PR #39 qualification, CLOSED unmerged
- PR #31 HomeDS402 H37 stale Draft: current-dev PR #40으로 selective transplant/requalification 후 CLOSED unmerged

의도적으로 유지되는 qualification/history work:

- PR #14 `codex/mode-10-c78-finalize` — C78 history/review
- PR #18 `codex/setopmode-mode11-bench-activation` — `DO NOT MERGE`, physical bench evidence

현재 기능 상태 판정은 항상 `dev`를 기준으로 한다.

## 7. 다음 작업 우선순위

### 우선순위 A — current artifact blocker

1. HomeDS402 H37 fresh C78 evidence collector를 current `dev`에 통합
2. same-tree actual C78/ARM rebuild/link와 generated artifact review로 H37-05/06 진행
3. issue #35 HomeDS402Ex fresh C78/generated artifact evidence 수집
4. issue #28 HomeDS402Ex axis 1..4 hardware profile 승인

### 우선순위 B — physical qualification

1. HomeDS402 H37-07 Axis1 normal/timeout/fault/disconnect/response-loss matrix
2. SetOperationMode MODE-11/12 Axis1 evidence
3. HomeDS402Ex HOMEEX-08 actual runtime은 issue #28/#35 완료 후에만 구현
4. HomeDS402Ex HOMEEX-10 Axis1 matrix
5. 각 API Axis2~4 확대

### 우선순위 C — activation

- HomeDS402 H37-09 five-gate + bit 6 paired activation
- SetOperationMode MODE-14 bits 8/9/10 paired activation
- HomeDS402Ex HOMEEX-13 bit 11 + WPF Start UI paired activation

각 activation은 해당 API의 current source/image/hardware proof가 모두 같은 승인 세트일 때만 허용한다.

## 8. 결론

2026-08-27 기준 HomeDS402 H37의 hardware-independent source/PC/WPF qualification은 current `dev`에
통합됐다. SetOperationMode/HomeDS402Ex dynamic recovery localization도 current `dev`에서 GitHub CI
evidence까지 닫혔다.

그러나 HomeDS402의 fresh C78/generated artifact와 hardware matrix, SetOperationMode current-image
C78/hardware, HomeDS402Ex issue #28/#35 및 physical runtime이 남아 있다. 따라서 production 판정은
계속 **NO-GO**다.

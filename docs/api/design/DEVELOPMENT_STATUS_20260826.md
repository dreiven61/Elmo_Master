# 개발 상태 스냅샷 — 2026-08-26 최종 통합

이 문서는 2026-08-26 branch 정리 및 HomeDS402Ex software-side 통합 작업 이후의 `dev` 상태를 기록한다. 설계 완료, source/static qualification, generated artifact qualification, hardware qualification, production activation은 서로 다른 gate로 취급한다.

## 1. 통합 기준점

- 이 스냅샷 작성 직전 integration baseline: `dev@1c8ea03683a083d1856bda38bfb54826e7cb5d7d`
- HomeDS402Ex production posture: **NO-GO / capability OFF / runtime OFF**
- `LMC_DIAG_DS402_HOME_EX_ENABLED`: `FALSE` 유지
- Admin HomeDS402Ex capability bit 11: OFF 유지
- HOMEEX-01/02 실제 axis 1..4 hardware profile 승인: issue #28에서 계속 미완료
- hardware-dependent wiring, polarity, method allowlist, scale, range, rounding, vendor-specific mapping, MapRevision 값은 승인 없이 추정하지 않는다.

## 2. 이번 통합에서 `dev`에 merge된 HomeDS402Ex tranche

### PR #26 — HOMEEX-07 full-identity ownership admission

Merge commit: `2fc9bb208762b8f8c8e8ffad8be3e48e13103a2f`

통합 범위:

- OwnerKind 7 / active state 13
- legacy HomeDS402와 HomeDS402Ex가 ResourceKind 3 DS402 Home engine 공유
- 116-byte Start identity를 64-byte prefix + 52-byte per-axis identity tail로 보존
- TCP reservation / Diagnostics exact identity validation
- runtime gate OFF에서는 reservation을 deterministic rollback
- HomeDS402Ex `CommitAxisOwnership`, SDO runtime, controlword/mode/setpoint mutation은 열지 않음

### PR #27 — HOMEEX-01/02 axis profile approval gate

Merge commit: `003e82c5eadbca5256b1066785d7017e3e2fb9ce`

통합 범위:

- physical axis 1..4 profile manifest
- `Pending` / `Approved` 분리 검증
- hardware-dependent field를 승인 없이 채우지 않는 fail-closed profile gate
- pending manifest structural validation
- approved activation gate는 실제 승인값과 paired MapRevision이 없으면 통과하지 못함

현재 profile 값은 의도적으로 `pending/null` 상태다. 이 merge는 hardware profile 승인 완료를 의미하지 않는다.

### PR #29 — HOMEEX-09 source/static identity qualification

Merge commit: `2b32920ec563e793f3d59ba21d8df2707d91e654`

통합 범위:

- `ValidateAxisOwnershipPreemptionReplacement`와 `PrepareAxisOwnershipPublishDecision`의 stale per-axis `* 8` identity-tail stride를 `LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES`로 수정
- 64-byte identity prefix는 그대로 유지
- 52-byte per-axis identity-tail geometry를 legacy compact identity/preemption self-test에 반영
- persisted `OwnershipState` read fence `44 -> 47` re-baseline; identity/pointer fence는 12 유지
- SetOperationMode와 HomeDS402Ex 각각의 Reserved-state classifier를 legacy disjoint proof에서 명시적으로 검증
- HOMEEX-08 runtime/capability fail-closed invariant 유지

Qualification:

- final `dev`-based PR workflow run `32921289354`: **SUCCESS**
- dedicated HOMEEX-09 static verifier: **37/37 PASS**
- legacy SourceOnly은 HomeDS402Ex source/static ratchet을 통과한 뒤 다음 exact generated-artifact boundary에 도달:
  - `LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`
- diff hygiene: PASS

중요: 위 결과는 **HOMEEX-09 전체 완료가 아니다**. 설계의 HOMEEX-09 완료 조건인 method-size / C78 / fresh generated artifact qualification은 계속 남아 있다. known `Classes.lcb` physical-identity boundary를 source regression으로 숨기지 않고 별도 downstream gate로 유지한다.

### PR #30 — HOMEEX-08 approved-profile execution-plan gate

Merge commit: `64bfe52ae5f28120dd61536db24a12f8f62ca28a`

통합 범위:

- approved axis profile + engineering parameters -> frozen DINT execution plan
- per-axis approved method allowlist 검증
- Diagnostics MapRevision exact match 검증
- explicit approved scale / rounding / converted DINT range 적용
- `Position == Int32.MinValue` overflow 차단
- vendor-specific `DetectionVelocityLimit` / `DistanceLimit` / `TorqueLimit` zero policy
- arbitrary raw-plan 및 public engineering-unit Prepare surface는 계속 닫힘

Qualification:

- approved-plan workflow run `32921714514`, rerun job `98036885071`: Debug/Release build + full tests + diff hygiene **SUCCESS**
- same merge-ref SDK wire workflow run `32921714525`: Debug/Release full tests + diff hygiene **SUCCESS**
- same merge-ref WPF recovery workflow run `32921714585`: Debug/Release full tests + diff hygiene **SUCCESS**

첫 approved-plan Release 실행에서 `GroupEnableWait.PreWire.CancelAndTimeoutAreZeroWire` 단일 timing/socket failure가 있었으나, HomeDS402Ex approved-profile tests는 통과했고 동일 commit의 병렬 SDK Release suite는 전체 PASS였다. 실패 job만 재실행한 결과 전체 green으로 재현되지 않았다. product code workaround는 추가하지 않았다.

## 3. 현재 HomeDS402Ex 경계

software-side 준비 코드가 `dev`에 통합됐더라도 다음 경계는 열지 않는다.

- actual HomeDS402Ex parameter SDO snapshot/program/restore
- DS402 homing controlword bit 4 execution
- mode 6 runtime switching / verification
- RT owner activation 및 physical homing
- terminal cleanup proof를 사용하는 production outcome execution
- Admin feature bit 11 activation
- issue #28 승인 없는 engineering-unit public Prepare
- hardware profile 값 자동 추정
- unresolved Start 자동 replay

따라서 현재 상태는 **source-safe / preparation-gated integration**이며 production-ready 또는 hardware-qualified 상태가 아니다.

## 4. HOMEEX 설계 체크포인트

- HOMEEX-06 parser/state/outcome scaffold: merge 완료, runtime OFF
- HOMEEX-07 full-identity ownership admission: `dev` merge 완료
- HOMEEX-01/02 profile approval gate implementation: `dev` merge 완료
- HOMEEX-01/02 actual hardware approval: **미완료 — issue #28**
- HOMEEX-08 approved-profile DINT plan preparation gate: `dev` merge 완료
- HOMEEX-08 actual LASAL SDO/homing runtime: **미개방**
- HOMEEX-09 source/static identity qualification: 통합 완료
- HOMEEX-09 method-size/C78/generated artifact qualification: **미완료**
- HOMEEX-10 Axis1 hardware matrix: 미완료
- HOMEEX-11 Axis2~4 / approved method matrix: 미완료
- HOMEEX-13 capability bit 11 / WPF UI paired activation: 미완료

## 5. SetOperationMode 남은 외부/물리 gate

SetOperationMode software implementation은 `dev`에 누적되어 있으나 production activation은 완료로 표시하지 않는다.

Open Draft PR #18 `codex/setopmode-mode11-bench-activation`은 physical bench evidence용 **DO NOT MERGE** branch로 보존한다.

남은 외부 gate:

- fresh C78/ARM exact image build/load evidence
- MODE-11A same-mode zero-write packet/hardware evidence
- MODE-11B independently approved non-CSP exact-one-write/readback evidence
- durable WPF recovery journal과 실제 TCP/SDO evidence 결합
- 후속 activation gate

Open Draft PR #14 `codex/mode-10-c78-finalize`도 자동 삭제하지 않고 기존 C78 history/review branch로 보존한다.

## 6. branch cleanup 결과

One-shot cleanup workflow run `32922104520`: **SUCCESS**

- confirmed merged / closed-obsolete / fully superseded `codex/*` branch 31개 삭제
- workflow 내부에서 삭제 대상 ref 부재를 재검증
- 보존 대상 branch 존재를 재검증
- one-shot workflow 자체를 `dev`에서 제거
- cleanup workflow 제거 commit: `1c8ea03683a083d1856bda38bfb54826e7cb5d7d`

cleanup 후 원격 `codex/*` branch는 다음 두 개만 남는다.

- `codex/mode-10-c78-finalize` — PR #14, open Draft
- `codex/setopmode-mode11-bench-activation` — PR #18, open Draft / physical bench evidence

기본 integration branch `dev`와 repository default branch `main`은 당연히 보존한다.

## 7. 다음 작업 우선순위

1. issue #28에서 axis 1..4 wiring / polarity / homing method allowlist / scale / range / rounding / vendor-specific mapping / MapRevision 실제 승인 증거 확보
2. HOMEEX-09 fresh method-size / C78 / generated `Classes.lcb` artifact qualification 완료
3. 위 두 gate가 모두 닫힌 뒤에만 HOMEEX-08 LASAL parameter SDO/runtime 구현 검토
4. Axis1 HOMEEX-10 hardware matrix 후 Axis2~4 HOMEEX-11 matrix 진행
5. physical qualification 완료 전에는 capability bit 11과 WPF Start UI를 활성화하지 않음

## 8. 결론

2026-08-26 기준 HomeDS402Ex의 hardware-independent ownership/profile/source-static/approved-plan 준비 tranche는 `dev`에 통합됐다. branch stack은 정리됐고 active `codex/*` branch는 의도적으로 보존한 PR #14/#18 두 개뿐이다.

그러나 issue #28 hardware profile approval과 fresh C78/generated artifact evidence가 남아 있으므로 HomeDS402Ex runtime/capability/production activation은 계속 **NO-GO**다.

# 개발 상태 스냅샷 — 2026-08-26

이 문서는 현재 `dev` 기준으로 이미 merge된 기능과 아직 Draft/open 브랜치에서 검증 중인 기능을 분리해 기록한다. 설계 완료와 실제 source/CI/hardware qualification을 같은 의미로 취급하지 않는다.

## 1. 기준점

- integration baseline: `dev@b7ddb6cc843004c74d6c4a05421b09a866bf1cec`
- baseline 의미: HOMEEX-06 LASAL gate-OFF scaffold 및 진행상태 동기화까지 `dev`에 merge된 상태
- HomeDS402Ex production posture: **NO-GO / capability OFF / runtime OFF**
- `LMC_DIAG_DS402_HOME_EX_ENABLED`: `FALSE` 유지
- Admin HomeDS402Ex capability bit 11: OFF 유지
- HOMEEX-01/02 실제 axis wiring/scale/profile 승인: issue #28에서 미완료

## 2. `dev`에 merge된 주요 개발

### SetOperationMode

MODE-06 이후 software-side 구현과 qualification tranche가 `dev`에 누적 merge되어 있다.

- dedicated SetOperationMode ownership/admission 및 diagnostics executor
- `0x6061 -> 0x6060 -> 0x6061` 실행/검증 경로
- same-mode zero-write 처리
- write-dispatch 이후 no-replay recovery
- SetOperationMode와 기존 axis owner/generic SDO 간 충돌 및 safety-preemption 처리
- generic D5 `0x6060` 영구 차단
- static/source qualification, define-order regression gate, C78 artifact evidence tooling
- WPF durable recovery 및 deterministic reject resolution
- production activation은 계속 OFF

MODE-11 bench/hardware packet evidence와 최종 activation은 아직 별도 gate다.

### HomeDS402Ex C# SDK / WPF

merge 완료:

- PR #20: dormant HomeDS402Ex SDK wire/lifecycle contract
  - `0x7D1B Start`, `0x7D1C ReadOutcome`, `0x7D1D Retire`
  - strict request/response parser
  - one-shot prepared lifecycle
  - exact recovery key / exact-generation retire
  - public raw-plan 및 engineering-unit Prepare는 fail-closed
- PR #21: WPF durable no-replay recovery journal
  - pre-dispatch durable arm
  - startup interlock
  - recovery에서는 Start replay 금지
  - exact-key Query / exact-generation Retire만 허용
- PR #22: dormant LASAL baseline verifier/readiness
- PR #23: OwnerKind 7 / ResourceKind 3 ownership ABI 설계 고정
- PR #24: HOMEEX-06 LASAL gate-OFF parser/state/outcome scaffold
  - `0x7D1B/1C/1D` route
  - dedicated scaffold state
  - strict parser
  - no-op runtime processor
  - capability/runtime OFF 유지
- PR #25: HOMEEX-06 진행상태 문서 동기화

## 3. 현재 진행 중인 HomeDS402Ex stack

### PR #26 — HOMEEX-07 full-identity ownership admission

Branch: `codex/home-ds402ex-homeex07-ownership`

구현 범위:

- OwnerKind 7 admission
- active state 13 예약
- HomeDS402Ex와 legacy HomeDS402가 ResourceKind 3 DS402 Home engine 공유
- 116-byte Start identity를 64-byte prefix + 52-byte per-axis identity tail로 보존
- TCP reservation / Diagnostics exact validation / fail-closed rollback

상태: **Draft 유지**. HOMEEX-09 review에서 stale 8-byte tail-stride 잔재가 발견되어 qualification closure가 필요하다.

### PR #27 — HOMEEX-01/02 axis profile approval gate

Branch: `codex/home-ds402ex-homeex01-02-profile-gate`

구현 범위:

- axis 1..4 profile manifest
- pending/approved 상태 검증
- wiring/method allowlist/scale/range/rounding/vendor-specific field를 승인 없이 추정하지 않는 fail-closed gate
- pending manifest structural validation

상태: **Draft / hardware-profile approval 대기**.

Issue #28에서 axis 1..4의 실제 wiring, polarity, homing method allowlist, scale, DINT range, rounding, vendor-specific mapping과 MapRevision 증거가 승인되어야 한다.

### PR #29 — HOMEEX-09 static / SourceOnly identity qualification

Branch: `codex/home-ds402ex-homeex09-static-qualification`

현재 실제 source repair commit:

- `b808e0a8a2be46d8513b9f0bd7a507043ace680e`
- `LMCControlCommandService.st`
- `ValidateAxisOwnershipPreemptionReplacement`의 stale `probeAxisIndex * 8` tail stride를 `LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES`로 수정
- `PrepareAxisOwnershipPublishDecision`의 동일 stale tail stride도 같은 macro로 수정

추가 qualification 범위:

- ownership persisted-read SourceOnly fence 44 -> 47 re-baseline
- 52-byte identity-tail contract
- HOMEEX-08 runtime/capability fail-closed 검증
- legacy SourceOnly semantic/lexical ratchet 정리

상태: **Draft / qualification 미완료**. source 수정은 실제 브랜치에 들어갔지만 final legacy SourceOnly closure는 아직 완료로 기록하지 않는다.

### PR #30 — HOMEEX-08 approved-profile execution-plan gate

Branch: `codex/home-ds402ex-homeex08-approved-plan-gate`

구현 범위:

- approved axis profile + engineering parameters -> frozen DINT execution plan
- method allowlist 검증
- Diagnostics MapRevision exact match
- explicit scale/rounding/range 적용
- `Position == Int32.MinValue` overflow 차단
- vendor-specific field zero policy
- arbitrary raw-plan / public engineering-unit Prepare surface는 닫힘

PR 기록 기준 Debug/Release API build 및 full test suite는 PASS다. 그러나 #29 ownership/SourceOnly closure와 issue #28 hardware profile 승인이 끝나기 전에는 runtime integration-ready로 간주하지 않는다.

## 4. 아직 열지 않는 경계

다음은 구현되었거나 준비 코드가 있어도 production-ready로 간주하지 않는다.

- HomeDS402Ex 실제 SDO parameter snapshot/program/restore
- DS402 homing controlword/mode/setpoint 실행
- RT owner activation 및 physical homing
- Admin capability bit 11 activation
- hardware-dependent axis 1..4 wiring/scale 값 자동 추정
- issue #28 승인 없는 public engineering-unit Prepare
- unresolved Start의 자동 replay

## 5. SetOperationMode 남은 외부/물리 gate

Open PR #18 `codex/setopmode-mode11-bench-activation`은 **DO NOT MERGE bench tooling**으로 유지한다.

남은 항목:

- fresh C78/ARM exact image build/load evidence
- MODE-11A same-mode zero-write packet/hardware evidence
- MODE-11B independently approved non-CSP exact-one-write/readback evidence
- durable WPF recovery journal과 실제 TCP/SDO evidence 결합
- 후속 MODE-12 / MODE-14 activation gate

따라서 SetOperationMode software implementation이 많이 완료되었더라도 production activation 완료로 표시하지 않는다.

## 6. 브랜치 유지 기준

현재 삭제하면 안 되는 active branch:

- `dev`
- `codex/setopmode-mode11-bench-activation` — PR #18, physical bench evidence용
- `codex/home-ds402ex-homeex07-ownership` — PR #26
- `codex/home-ds402ex-homeex01-02-profile-gate` — PR #27
- `codex/home-ds402ex-homeex09-static-qualification` — PR #29
- `codex/home-ds402ex-homeex08-approved-plan-gate` — PR #30

`codex/mode-10-c78-finalize` — PR #14는 오래된 open/Draft이며 현재 `dev`와 diverged 상태다. 자동 삭제하지 않고 별도 review/closure 대상으로 둔다.

## 7. merge 완료 후 삭제 가능한 branch 후보

아래 branch는 대응 PR이 merge 완료된 것으로 확인되므로, 별도 보존 정책이 없다면 원격 branch 삭제 후보로 본다.

HomeDS402Ex:

- `codex/home-ds402ex-homeex06-progress-sync` — PR #25 merged
- `codex/home-ds402ex-lasal-scaffold` — PR #24 merged
- `codex/home-ds402ex-owner-abi` — PR #23 merged
- `codex/home-ds402ex-lasal-scaffold-readiness` — PR #22 merged
- `codex/home-ds402ex-wpf-recovery` — PR #21 merged
- `codex/home-ds402ex-sdk-wire` — PR #20 merged

SetOperationMode:

- `codex/setopmode-mode11-software-evidence-sync` — PR #19 merged
- `codex/setopmode-mode10-artifact-review` — PR #17 merged
- `codex/setopmode-c78-evidence-capture` — PR #16 merged
- `codex/setopmode-mode13-reject-resolution` — PR #15 merged
- `codex/mode-10-c78-artifact-qualification` — PR #13 merged
- `codex/mode-10-c78-define-order-fix` — PR #12 merged
- `codex/mode-10-sourceonly-finalize-permanent` — PR #11 merged
- `codex/mode-10-sourceonly-finalize` — PR #10 merged
- `codex/mode-10-sourceonly-cleanup` — PR #9 merged
- `codex/mode-10-method-split` — PR #8 merged
- `codex/mode-10-static-qualification-v2` — PR #7 merged
- `codex/mode-09-block-generic-6060` — PR #6 merged
- `codex/mode-08-owner-conflicts` — PR #5 merged
- `codex/mode-07-no-replay-recovery` — PR #4 merged
- `codex/mode-06-diagnostics-executor` — PR #2 merged
- `codex/mode-06-axis-operation-mode` — PR #1 merged

Closed without merge but obsolete cleanup branch:

- `codex/mode-06-gate-cleanup` — PR #3 closed without merge; cleanup intent was handled directly on `dev`, no LASAL source change from this PR was merged

## 8. 다음 정리 순서

1. PR #29 legacy SourceOnly/static qualification closure
2. #26/#27/#29 stack 정합성 재확인
3. issue #28 axis profile approval 전까지 HOMEEX runtime/capability OFF 유지
4. #30 approved-plan gate는 hardware-independent 준비 코드로 유지
5. merged branch를 삭제해 branch 목록을 active work 중심으로 축소
6. PR #14는 branch history가 `dev`와 diverged되어 있으므로 자동 삭제 대신 별도 폐기/보존 판단
7. PR #18은 physical bench qualification 완료 전 유지

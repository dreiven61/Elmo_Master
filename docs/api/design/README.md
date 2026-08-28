# 최우선 API 개발 설계

- 기준일: 2026-08-28
- current integration branch: `dev`
- current documented baseline: `dev@72b14dbb280f95b97211988612106157a0f519b0`
- corrective functional baseline: `687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
- production posture: **NO-GO**
- active P0 tracking: issue #46
- current status snapshot: `DEVELOPMENT_STATUS_20260828.md`

이 폴더의 current 판정은 **branch 이름/PR 개수보다 `dev` 실제 실행 경로를 우선**한다. `DEVELOPMENT_STATUS_20260827.md`는 2026-08-27 시점 기록으로 보존하며, current 구현 상태는 `DEVELOPMENT_STATUS_20260828.md`가 우선한다.

---

## 1. 현재 최우선 작업

### P0-A — SetOperationMode physical closure

현재 `dev`에는 다음 software/source 구현이 통합돼 있다.

- PP(1), PV(3), IP(7), CSP(8) lifecycle
- PLC `SupportedModeMask=0x018A` contract
- SDK/WPF fail-closed capability coupling
- durable Start/no-replay recovery/outcome/retire
- MODE-11E warm-start recovery
- MODE-11F rejection/preflight/write evidence diagnostics
- PR #58 fresh cross-mode preflight 및 live WPF Start-gate diagnostics

현재 남은 핵심은 source 추가가 아니라 **current `dev`와 실기 qualification image의 identity를 다시 맞춘 physical qualification**이다.

실행 순서:

1. 기존 `codex/setopmode-mode11-bench-activation`을 current `dev`에서 재생성
2. activation-only delta 확인
3. fresh LASAL C78/ARM build + PLC load
4. 동일 source의 WPF로 `AdminTriad`, `0x018A`, Diagnostics identity 및 Start gate 확인
5. Axis1 PP/PV/IP/CSP same-mode/cross-mode matrix
6. timeout/disconnect/quarantine/recovery
7. Axis2..4
8. production activation review

CSP -> CSP `SucceededNoWrite`는 실제 `0x6060` write 성공으로 세지 않는다.

### P0-B — Generic SDO physical closure

Generic SDO는 더 이상 `R03/R04/R05 구현 예정` 단계가 아니다.

- SDO-R02 dual-entry executor: source 통합
- SDO-R03 generic scalar 1/2/4-byte Write policy: 통합
- SDO-R04 arbitrary WPF editor/preview/reserved warning: 통합
- SDO-R05 durable exact-request no-replay recovery: 통합
- PR #58 ordinary Write safe-state corrective policy: 통합

현재 남은 gate:

1. current `dev` C78/PLC image
2. safe non-semantic object의 physical 1/2/4-byte Write + exact readback
3. manual/programmatic contention
4. timeout/disconnect/readback mismatch recovery
5. Axis2..4 확대

raw semantic/dedicated-owner blocklist는 유지한다: `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`.

---

## 2. current 문서 우선순위

SetOperationMode:

- `SET_OPERATION_MODE_DESIGN.md`
- `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`의 2026-08-28 corrective tranche

Generic SDO:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`
- `DEVELOPMENT_STATUS_20260828.md`

전체 current queue:

- `DEVELOPMENT_STATUS_20260828.md`

문서 간 충돌이 있으면 최신 current status/addendum을 우선하고, 오래된 branch-local PASS를 current hardware PASS로 승격하지 않는다.

---

## 3. 다른 P0 경계

### HomeDS402

software/source/WPF qualification은 통합돼 있으나 activation은 OFF다. fresh generated artifact/C78와 physical matrix가 남아 있다.

### HomeDS402Ex

SDK/ownership/retained store/WPF recovery/approved-plan source는 존재하지만 physical runtime과 capability activation은 OFF다. hardware profile 승인과 fresh C78가 선행돼야 한다.

### SetPosition

lifecycle 및 WPF durable recovery, host factory receipt/readback tooling은 존재한다. 실제 A/B runtime backend는 issue #44의 vendor CRC golden fixture와 LASAL IDE generated `_FileSys` ABI가 없어서 계속 외부 blocker다. 이 경계를 추측으로 우회하지 않는다.

---

## 4. 브랜치 운영 원칙

- 기능 integration truth는 `dev` 하나로 유지한다.
- 이미 존재하는 qualification 목적에는 새 branch를 계속 만들지 않는다.
- 실기 branch는 current `dev`에서 재생성하고 activation/bench delta만 남긴다.
- temporary promotion helper/workflow는 source commit 뒤 제거한다.
- source CI PASS와 hardware PASS를 구분한다.
- 실기 failure 분석 전에 loaded PLC image와 source SHA identity를 먼저 확인한다.

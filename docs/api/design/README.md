# 최우선 API 개발 설계

- 기준일: 2026-08-28
- current integration / qualification source: `dev`
- corrective functional baseline: `687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
- current source truth: **SetOperationMode + Generic SDO qualification gates are ON**
- production release posture: **NO-GO**
- active P0 tracking: issue #46
- current status snapshot: `DEVELOPMENT_STATUS_20260828.md`

이 폴더의 current 판정은 branch 이름/PR 개수보다 `dev`의 실제 source를 우선한다. `DEVELOPMENT_STATUS_20260827.md`는 historical snapshot이며 current 구현/activation truth는 `DEVELOPMENT_STATUS_20260828.md`가 우선한다.

---

## 1. P0-A — SetOperationMode

현재 `dev` source:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE`
- `LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE`
- Admin feature mask `0x00000717`
- supported mode mask `0x018A` = PP(1), PV(3), IP(7), CSP(8)

즉 현재 단계는 “기능 source OFF”가 아니라 **qualification-active source / hardware qualification open**이다.

통합된 software path:

- PP/PV/IP/CSP lifecycle
- SDK/WPF capability fail-closed
- durable Start/no-replay recovery/outcome/retire
- MODE-11E warm-start recovery
- MODE-11F rejection/preflight/write evidence
- PR #58 current bench corrective preflight
- WPF actual Start-gate diagnostics

다음 순서:

1. exact current `dev` fresh LASAL C78/ARM build
2. exact artifact PLC load
3. same-source WPF로 gate 상태 확인
4. Axis1 PP/PV/IP/CSP physical matrix
5. failure/recovery matrix
6. Axis2..4
7. production release activation/deactivation review

CSP -> CSP `SucceededNoWrite`는 실제 `0x6060` cross-mode write PASS가 아니다.

기존 `codex/setopmode-mode11-bench-activation` / PR #18은 old gate-OFF baseline에서 만든 activation branch라 현재는 obsolete다. 별도 stale qualification branch 대신 exact `dev`를 사용한다.

---

## 2. P0-B — Generic SDO

현재 `dev` source:

- `LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE`
- Axis1 UI24 qualification gate TRUE
- ordinary generic scalar policy는 R03 이후 UI24 exact preset과 분리

software tranche:

- SDO-R02 dual-entry executor: 통합
- SDO-R03 generic 1/2/4-byte scalar Write: 통합
- SDO-R04 arbitrary WPF editor/preview/reserved warning: 통합
- SDO-R05 durable exact-request no-replay recovery: 통합
- PR #58 ordinary Write safe-state correction: 통합

PR #58 이후 ordinary generic Write는 `Standstill=True`, Fault=False, OperationEnabled=False를 요구하고 PLC는 generic non-semantic 대상에 대해 DS402 base `0x40`, `0x21`, `0x23`만 허용한다.

raw semantic/dedicated-owner blocklist는 유지한다:

`0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`.

다음 순서:

1. exact current `dev` C78/PLC image
2. Axis1 safe non-semantic 1/2/4-byte Write + exact readback
3. manual/programmatic BUSY/no-wire contention
4. timeout/disconnect/readback mismatch recovery
5. Axis2..4

---

## 3. current 문서 우선순위

전체 current truth:

- `DEVELOPMENT_STATUS_20260828.md`

SetOperationMode:

- `SET_OPERATION_MODE_DESIGN.md`
- `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`의 2026-08-28 corrective tranche

Generic SDO:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`

문서 간 activation 상태가 충돌하면 current `dev` source를 기준으로 판정하고 이 README/20260828 status를 갱신한다.

---

## 4. 다른 P0 경계

### HomeDS402

software/source/WPF qualification은 통합돼 있으나 activation은 OFF다. fresh generated artifact/C78 및 physical matrix가 남아 있다.

### HomeDS402Ex

SDK/ownership/retained store/WPF recovery/approved-plan source는 존재하지만 physical runtime과 capability activation은 OFF다. hardware profile 승인과 fresh C78가 선행돼야 한다.

### SetPosition

lifecycle, WPF durable recovery, host factory receipt/readback tooling은 존재한다. 실제 A/B runtime backend는 issue #44의 vendor CRC golden fixture와 LASAL IDE-generated `_FileSys` ABI가 없어서 외부 blocker다. 이를 추측으로 우회하지 않는다.

---

## 5. 브랜치/qualification 운영 원칙

- current integration truth는 `dev` 하나다.
- 현재 `dev`가 qualification-active인 기능은 별도 오래된 activation branch를 사용하지 않는다.
- 같은 기능의 새 qualification branch를 계속 만들지 않는다.
- 실기 전 source SHA + generated artifact + PLC loaded image + WPF source를 하나의 identity set으로 기록한다.
- source CI PASS와 physical PASS를 구분한다.
- temporary promotion helper/workflow는 source commit 후 제거한다.
- production 배포 전 qualification-active gate를 별도 release review에서 반드시 재판정한다.

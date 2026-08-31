# 최우선 API 개발 설계

- 기준일: 2026-08-31
- current integration / qualification source: `dev`
- current status snapshot: `DEVELOPMENT_STATUS_20260831.md`
- current SetOperationMode implementation result: `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md`
- current Detail 49 observability implementation result: `SET_OPERATION_MODE_DETAIL49_OBSERVABILITY_IMPLEMENTATION_RESULT_20260831.md`
- implementation contract: `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md`
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- production release posture: **NO-GO**
- active P0 tracking: issue #46

이 폴더의 current 판정은 branch 이름/PR 개수보다 `dev`의 실제 source와 최신 실기 evidence를 우선한다. `DEVELOPMENT_STATUS_20260827.md`, `DEVELOPMENT_STATUS_20260828.md`는 historical snapshot으로 보존하고, 현재 상태는 `DEVELOPMENT_STATUS_20260831.md` 및 최신 implementation result를 우선한다.

---

## 1. P0-A — SetOperationMode

current `dev` source truth:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

지원 target은 PP(1), PV(3), IP(7), CSP(8)다. Homing(6)은 HomeDS402/HomeDS402Ex가 소유한다.

통합 software path:

- PP/PV/IP/CSP lifecycle
- durable Start/no-replay recovery/outcome/retire
- same-target `SucceededNoWrite`와 real cross-mode 구분
- cross-mode fresh drive-status preflight
- WPF actual Start-gate diagnostics
- raw Generic SDO `0x6060` permanent deny

### Software blocker A — Diagnostics capability freshness ordering: CLOSED

2026-08-28 17:28 실기에서는 preflight의 inline D5 `0x6041`/`0x6061` read가 Diagnostics observation sequence를 진행시킨 뒤 old observation으로 `PrepareSetOperationMode()`를 호출해 다음 host exception이 발생했다.

```text
The supplied diagnostics capabilities are not the current observation.
```

2026-08-31 functional commit `d4ce1b2f9c2a41f5117e0bd769533d0483c1ff91`에서 순서를 다음으로 수정했다.

```text
Admin capability / selected mode 확인
-> GetPhysicalAxis
-> fresh ReadDriveStatus preflight
-> FINAL Diagnostics capability refresh
-> capability/admission 확인
-> PrepareSetOperationMode(final current observation)
-> durable ArmBeforeDispatch
-> SetOperationModeAsync exactly once
-> outcome/recovery
```

FINAL Diagnostics refresh와 Prepare 사이에는 capability-producing/read helper를 삽입하지 않는다. `requireCurrentObservation=true`, Build/BootId/MapRevision identity 및 Standstill/Fault/OperationEnabled fence는 그대로 유지한다.

### Software blocker B — Start Click handler ownership: CLOSED

기존에는 button 생성 시 `ButtonStartAxisSetOperationMode_Click()`을 등록한 뒤 `InitializeReadOnlyApiUi()`에서 detach하고 `ButtonStartAxisSetOperationModeWithRejectResolution_Click()`으로 교체했다.

현재는 다음 하나의 runtime UI path만 유지한다.

```text
Start button
-> ButtonStartAxisSetOperationMode_Click
-> RunOperationAsync
-> StartAxisSetOperationModeOnceAsync
```

`ButtonStartAxisSetOperationModeWithRejectResolution_Click()`은 제거했다. definitive rejection archival/active-journal clear/UI update는 canonical handler에 통합했다.

software qualification evidence:

```text
API Debug full                       1200/1200 PASS
WPF SetOperationModeRecovery Debug       7/7 PASS
WPF AxisSetOperationModeJournal Debug    7/7 PASS
WPF SetOperationModeSdk Debug            1/1 PASS
Generic SDO Wpf.Sdo Debug               17/17 PASS
API Release build                       PASS
WPF Release build                       PASS
WPF focused SetOperationMode Release    PASS
git diff --check                        PASS
Start execution verifier               PASS
```

상세 구현 결과는 `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md`를 따른다.

### 현재 P0 gate — physical qualification

software blocker가 닫혔다고 physical mode-change PASS로 판정하지 않는다. exact updated source/image에서 다음 순서로 진행한다.

1. Axis1 CSP -> CSP no-write 확인
2. Axis1 CSP -> PP/PV/IP real `0x6060` 최대 1회 + 최종 `0x6061` 확인
3. PP/PV/IP -> CSP 확인
4. failure/recovery matrix
5. Axis2..4 확대

CSP -> CSP `SucceededNoWrite`는 실제 `0x6060` cross-mode Write PASS가 아니다.

---

## 2. P0-B — Generic SDO

current `dev` source:

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

통합 tranche:

- SDO-R02 dual-entry executor
- SDO-R03 generic physical axis 1..4 scalar Write, canonical 1/2/4-byte width
- SDO-R04 arbitrary WPF editor / exact preview / reserved warning
- SDO-R05 durable exact-request no-replay recovery
- PR #58 ordinary Write safe-state correction

ordinary Generic SDO Write 요구조건:

- Standstill=True
- DS402 Fault=False
- DS402 OperationEnabled=False

PLC generic safe base state:

- `0x40` Switch On Disabled
- `0x21` Ready To Switch On
- `0x23` Switched On

semantic/dedicated-owner raw blocklist:

```text
0x6040
0x6060
0x607A
0x60FF
0x6071
0x3204
0x20FC
```

Axis1 UI24 same-value four-ticket 경로는 특정 qualification preset이다. Generic SDO 전체의 유일 target으로 해석하지 않는다.

현재 source/PC regression은 통과했으나 physical safe-object Write/readback PASS는 아직 아니다.

다음 gate:

1. Axis1 safe non-semantic 1/2/4-byte Write + exact readback
2. manual/programmatic BUSY/no-wire contention
3. timeout/disconnect/readback mismatch durable recovery
4. Axis2..4 확대

---

## 3. current 문서 우선순위

전체 current truth:

- `DEVELOPMENT_STATUS_20260831.md`
- `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md`
- `../API_DEVELOPMENT_PROGRESS.md`
- `../API_MANUAL.md`

SetOperationMode 상세:

- `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md` — **현재 구현 결과**
- `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md` — 구현 계약
- `SET_OPERATION_MODE_DESIGN.md`
- `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`

Generic SDO 상세:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`

문서 간 current activation/지원 상태가 충돌하면 `dev` source와 최신 current snapshot/implementation result를 기준으로 판정하고 문서를 다시 동기화한다.

---

## 4. 다른 P0 경계

### HomeDS402

software/source/WPF qualification은 통합돼 있으나 activation은 OFF다. fresh generated artifact/C78 및 physical matrix가 남아 있다.

### HomeDS402Ex

SDK/ownership/retained store/WPF recovery/approved-plan source는 존재하지만 physical runtime과 capability activation은 OFF다. hardware profile 승인과 fresh C78가 선행돼야 한다.

### SetPosition

lifecycle, WPF durable recovery, host factory receipt/readback tooling은 존재한다. 실제 A/B runtime backend는 issue #44의 vendor CRC golden fixture와 LASAL IDE-generated `_FileSys` ABI가 없어서 외부 blocker다. 이를 추측으로 우회하지 않는다.

---

## 5. Repository / qualification 운영 원칙

- remote branch는 `main`, `dev` 두 개만 유지한다.
- `dev`가 유일한 current integration / qualification source truth다.
- 과거 29개 `codex/*` branch는 모두 `dev` ancestor임을 확인한 뒤 삭제했다.
- 기능 작업 branch가 필요하면 작업 -> 검증 -> `dev` merge -> 즉시 삭제한다.
- 같은 기능의 qualification branch를 장기간 누적하지 않는다.
- 실기 전 source SHA + generated artifact + PLC loaded image + WPF EXE/SDK identity를 하나의 evidence set으로 기록한다.
- source CI PASS와 physical PASS를 분리한다.
- production 배포 전 qualification-active gate를 별도 release review에서 반드시 재판정한다.

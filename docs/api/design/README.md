# 최우선 API 개발 설계

- 기준일: 2026-08-31
- current integration / qualification source: `dev`
- current status snapshot: `DEVELOPMENT_STATUS_20260831.md`
- current SetOperationMode implementation plan: `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md`
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- production release posture: **NO-GO**
- active P0 tracking: issue #46

이 폴더의 current 판정은 branch 이름/PR 개수보다 `dev`의 실제 source와 최신 실기 evidence를 우선한다. `DEVELOPMENT_STATUS_20260827.md`, `DEVELOPMENT_STATUS_20260828.md`는 historical snapshot으로 보존하고, 현재 상태는 `DEVELOPMENT_STATUS_20260831.md`를 우선한다.

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

### 현재 blocker A — Diagnostics capability freshness ordering

2026-08-28 17:28 실기에서 Axis1 current CSP(8) -> PP/PV/IP 요청이 모두 `StatusWord=0x02D0`으로 cross-mode preflight를 통과했다. 이후 동일하게 다음 host exception으로 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

원인:

```text
RefreshDiagnosticsCapabilities -> observation N
ReadDriveStatusAsync
  -> 0x6041 inline D5 -> GetCapabilities -> N+1
  -> 0x6061 inline D5 -> GetCapabilities -> N+2
PrepareSetOperationMode(cached N)
  -> requireCurrentObservation=true
  -> stale reject
```

따라서 현재 로그에서는 `0x7D23`과 실제 `0x6060` mutation까지 도달하지 않았다.

### 현재 blocker B — Start Click handler ownership이 불명확함

현재 WPF는 `MainWindow.AxisSetOperationModeRecovery.cs`에서 button 생성 시

```text
ButtonStartAxisSetOperationMode_Click
```

을 등록한 뒤, `MainWindow.ReadOnlyApi.cs`의 `InitializeReadOnlyApiUi()`에서 다시 detach하고

```text
ButtonStartAxisSetOperationModeWithRejectResolution_Click
```

으로 교체한다. 따라서 이름상 canonical handler인 `ButtonStartAxisSetOperationMode_Click()`은 runtime에서 호출되지 않는다.

이 구조는 기능 미구현 gate가 아니라 **불필요한 handler indirection / dead-handler 구조**다. 구현 시 다음으로 단일화한다.

```text
Start button
-> ButtonStartAxisSetOperationMode_Click            // 유일한 UI handler
-> StartAxisSetOperationModeOnceAsync               // 유일한 Start orchestration
-> preflight
-> FINAL Diagnostics capability refresh
-> PrepareSetOperationMode
-> durable ArmBeforeDispatch
-> SetOperationModeAsync exactly once
-> outcome/recovery
```

`ButtonStartAxisSetOperationModeWithRejectResolution_Click()`은 제거하고 definitive rejection resolution을 canonical handler에 통합한다.

상세 구현 계약은 `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md`를 따른다.

corrective ordering:

```text
Admin capability / selected mode 확인
-> GetPhysicalAxis
-> fresh ReadDriveStatus preflight
-> FINAL Diagnostics capability refresh
-> capability/admission 확인
-> PrepareSetOperationMode
-> durable ArmBeforeDispatch
-> Start exactly once
```

freshness fence, Build/BootId/MapRevision identity, Standstill/Fault/OperationEnabled fence, one-shot confirmation과 no-replay를 완화하지 않는다.

다음 gate:

1. canonical Start Click handler 단일화 + obsolete handler 제거
2. preflight -> final Diagnostics refresh -> Prepare ordering fix
3. single-handler / stale-old / current-final capability regression
4. API/WPF Debug/Release regression
5. Axis1 PP/PV/IP/CSP physical `0x6060`/`0x6061` matrix
6. failure/recovery matrix
7. Axis2..4 확대

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
- `../API_DEVELOPMENT_PROGRESS.md`
- `../API_MANUAL.md`

SetOperationMode 상세:

- `SET_OPERATION_MODE_START_EXECUTION_REFACTOR_PLAN_20260831.md` — **현재 구현 지시서**
- `SET_OPERATION_MODE_DESIGN.md`
- `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`

Generic SDO 상세:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`

문서 간 current activation/지원 상태가 충돌하면 `dev` source와 최신 current snapshot을 기준으로 판정하고 문서를 다시 동기화한다.

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

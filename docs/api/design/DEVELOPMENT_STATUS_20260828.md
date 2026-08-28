# 개발 상태 스냅샷 — 2026-08-28

- current integration / qualification source: `dev`
- current functional baseline: `dev@687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
- current qualification posture: **SetOperationMode + Generic SDO gates are ON in source; hardware PASS is NOT established**
- production release posture: **NO-GO**
- active P0 tracking: issue #46
- supersedes the current-state portions of `DEVELOPMENT_STATUS_20260827.md`

이 문서는 branch 수나 PR 수가 아니라 **현재 `dev`의 실제 실행 경로**를 기준으로 상태를 판정한다. `source/PC test`, `fresh LASAL C78 artifact`, `PLC load`, `physical wire/effect`, `production release`는 각각 별도 gate다.

---

## 1. 현재 P0 요약

| 영역 | current `dev` 구현 상태 | 현재 gate 상태 | 다음 gate |
|---|---|---|---|
| SetOperationMode | PP/PV/IP/CSP lifecycle, mode mask, durable no-replay, MODE-11E/11F, PR #58 bench corrective preflight까지 통합 | **qualification activation ON / hardware PASS 아님** | exact current-dev C78/PLC -> Axis1 PP/PV/IP/CSP physical matrix |
| Generic SDO | dual-entry executor + R03 generic scalar Write + R04 editor/preview + R05 durable recovery + PR #58 safe-state correction 통합 | **global Write ON / hardware PASS 아님** | exact current-dev C78/PLC -> safe non-semantic 1/2/4-byte Write/readback |
| HomeDS402 | H37 software/source/WPF qualification 통합 | activation OFF | fresh artifact/C78 -> hardware matrix |
| HomeDS402Ex | SDK, ownership, retained store, approved-plan gate, WPF recovery 존재 | physical runtime/activation OFF | approved profile + fresh C78 -> runtime/hardware qualification |
| SetPosition | lifecycle + WPF recovery + host factory receipt/readback tooling 존재 | native runtime activation OFF | issue #44 vendor CRC + generated `_FileSys` ABI -> A/B backend -> RT exactly-once |

---

## 2. SetOperationMode — 실제 source truth

### 2.1 current `dev` activation state

현재 source는 더 이상 `gate OFF`가 아니다.

`LMCDiagnosticsService.st`:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE`
- `LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE`

`LMCControlCommandService.st` AdminCapabilities:

- SetOperationMode Start/Outcome/Retire triad가 포함된 feature mask `0x00000717`
- software supported mode mask `0x018A` = PP(1), PV(3), IP(7), CSP(8)

따라서 WPF에서 `AdminTriad=True`, `SupportedModeMask=0x018A`가 보이는 것은 현재 source와 일치한다. 과거 문서의 “production gate OFF / bits 8..10 OFF” 문구는 **현재 source에 대해서는 stale**이다.

이 activation은 아직 production release 승인을 뜻하지 않는다. 정확한 표현은 **qualification-active / production NO-GO**다.

### 2.2 구현 완료 software path

- target: PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- SDK/WPF가 live PLC advertised mode와 capability triad를 확인
- durable pre-dispatch arm / exact outcome / exact-generation retire
- Start no-replay recovery
- PP/PV/IP/CSP warm-start recovery
- requested / preflight / observed mode 및 DS402/write evidence diagnostics
- raw Generic SDO `0x6060` 금지; mode mutation은 SetOperationMode lifecycle만 소유

### 2.3 2026-08-28 bench corrective (#58)

1. **CSP same-target 오판 제거**
   - CSP -> CSP는 current `0x6061`이 이미 8이면 `SucceededNoWrite`가 될 수 있다.
   - 이것을 실제 `0x6060` cross-mode Write 성공으로 세지 않는다.

2. **cross-mode fresh preflight**
   - durable Start 전에 fresh drive status / `0x6041` / `0x6061`을 읽는다.
   - real cross-mode transition은 `Standstill=True`, Fault=False, OperationEnabled=False에서만 허용한다.
   - OperationEnabled safety fence는 유지한다.

3. **WPF gate visibility**
   - stale 고정 activation 문구를 제거했다.
   - `AdminTriad`, `SupportedModeMask`, `DiagnosticsIdentity`, `Confirmed`, `SelectedModeAdvertised`, `AdmissionAllowed`, `JournalReady`를 status에 직접 표시한다.
   - 캡처에서 one-shot confirmation checkbox가 unchecked이면 `Confirmed=False`이므로 Start는 비활성인 것이 정상이다.

### 2.4 아직 hardware 완료가 아닌 것

- PR #58 source가 포함된 exact current `dev`의 fresh C78/ARM Rebuild + Link
- 그 exact artifact의 PLC load
- Axis1 PP/PV/IP/CSP same-mode/cross-mode matrix
- cross-mode `0x6060:0` exact-one-write evidence
- timeout/disconnect/quarantine/recovery physical matrix
- Axis2..4
- qualification 종료 후 production activation/deactivation 정책 결정

### 2.5 qualification branch 정리

`codex/setopmode-mode11-bench-activation` / PR #18은 과거 `dev`가 gate OFF였을 때 필요한 activation-only branch였다. 현재 `dev`가 이미 qualification-active이므로 이 branch는 **기능적으로 obsolete**다. 실기는 별도 stale branch가 아니라 exact current `dev`를 사용한다.

---

## 3. Generic SDO — 실제 source truth

### 3.1 current activation state

`LMCDiagnosticsService.st`:

- `LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE`
- `LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE`

Axis1 UI24 qualification preset은 ON이며, R03 이후 ordinary generic scalar admission은 UI24 exact-address preset과 분리된 global policy를 사용한다. 즉 current source는 Generic SDO Write를 단순 stub 상태로 두고 있지 않다.

### 3.2 완료된 software tranche

- **SDO-R02**: `LMCSdoExecutor` manual Server / programmatic API dual-entry arbitration
- **SDO-R03**: physical axis 1..4 generic scalar Write policy, canonical 1/2/4-byte width
- **SDO-R04**: WPF arbitrary request editor, exact preview, reserved/semantic warning
- **SDO-R05**: v3 durable exact-request identity, endpoint/build/BootId/MapRevision binding, restart read-only recovery, legacy v1/v2 zero-wire

따라서 이전 문서의 `R03 -> R04 -> R05 구현 예정` 표기는 current 상태가 아니다.

### 3.3 2026-08-28 bench corrective (#58)

실기 Write가 wire 전에 차단될 수 있었던 두 조건을 수정했다.

- ordinary editor가 same-value qualification용 `PowerOn=False` 요구를 재사용하던 경로 분리
- PLC가 safe state를 DS402 `0x40` 하나로만 제한하던 정책 수정

현재 ordinary generic Write 요구조건:

- `Standstill=True`
- DS402 Fault=False
- DS402 OperationEnabled=False

PLC가 허용하는 generic non-semantic DS402 base state:

- `0x40` Switch On Disabled
- `0x21` Ready To Switch On
- `0x23` Switched On

계속 거부:

- `0x27` Operation Enabled 및 기타 unsafe state
- semantic/dedicated-owner raw object `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`

### 3.4 software evidence

PR #58 corrective qualification:

- API Debug full suite: **1200/1200 PASS**
- Generic SDO WPF focused smoke: **17/17 PASS**
- API/WPF Debug + Release build: PASS
- permanent corrective verifier: PASS
- Generic SDO policy verifier: PASS
- diff hygiene: PASS

이 evidence는 source/PC contract다. 실제 drive object Write/readback hardware PASS를 대신하지 않는다.

### 3.5 다음 hardware gate

1. exact current `dev`로 C78/ARM rebuild + PLC load
2. Axis1 safe non-semantic object 1/2/4-byte Write
3. exact readback value/width
4. manual/programmatic BUSY/no-wire contention
5. timeout/disconnect/readback mismatch durable recovery
6. Axis2..4 확대

---

## 4. 이번 재검토에서 확인된 운영 문제

기능이 안 된 가장 큰 관리 문제는 **구현 source, 설계문서, qualification branch, 실제 PLC image가 서로 다른 시점을 가리킨 것**이다.

- 문서는 gate OFF라고 적혀 있었지만 current source는 이미 gate ON이었다.
- PR #18은 old baseline의 activation branch였는데 이후 source 구현이 계속 `dev`에 들어갔다.
- 실기에서 CSP same-target no-write가 실제 mode write처럼 보일 수 있었다.
- Generic SDO R03/R04/R05가 merge된 뒤에도 status 문서는 “아직 구현 예정”으로 남아 있었다.
- WPF status도 실제 live gate 대신 오래된 activation 설명을 출력했다.

이후 규칙:

- `dev`가 유일한 integration / current qualification source truth다.
- 같은 기능의 qualification branch를 새로 만들지 않는다.
- 실기 전 source SHA + C78 artifact + PLC loaded image + WPF source를 하나의 identity set으로 묶는다.
- stale branch image는 qualification evidence로 사용하지 않는다.
- source CI PASS와 hardware PASS를 분리한다.
- production 배포 전에는 qualification-active define/mask를 별도 release review에서 반드시 재판정한다.

---

## 5. 현재 작업 순서

1. **DONE** — Operation Mode / Generic SDO corrective source를 `dev` merge (#58, `687a78c6...`)
2. **DONE** — stale WPF Start status를 actual gate diagnostics로 교체
3. **DONE** — R03/R04/R05 및 current activation truth에 맞춰 status 문서 재작성
4. **NOW** — obsolete PR #18 / branch를 current `dev`와 정렬하고 별도 qualification delta 제거
5. exact current `dev` fresh LASAL C78/ARM Rebuild + Link
6. exact artifact PLC load + same-source WPF
7. Axis1 SetOperationMode physical matrix
8. Axis1 Generic SDO physical Write/readback matrix
9. failure/recovery matrix -> Axis2..4
10. production release activation review

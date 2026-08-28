# 개발 상태 스냅샷 — 2026-08-28

- current integration / qualification source: `dev`
- current analyzed baseline before this documentation update: `dev@cf92ef0e6891b227ac4c6da55256a524302b43ae`
- current qualification posture: **SetOperationMode + Generic SDO gates are ON in source; hardware PASS is NOT established**
- production release posture: **NO-GO**
- active P0 tracking: issue #46
- supersedes the current-state portions of `DEVELOPMENT_STATUS_20260827.md`

이 문서는 branch 수나 PR 수가 아니라 **현재 `dev`의 실제 실행 경로**를 기준으로 상태를 판정한다. `source/PC test`, `fresh LASAL C78 artifact`, `PLC load`, `physical wire/effect`, `production release`는 각각 별도 gate다.

---

## 1. 현재 P0 요약

| 영역 | current `dev` 구현 상태 | 현재 gate 상태 | 다음 gate |
|---|---|---|---|
| SetOperationMode | PP/PV/IP/CSP lifecycle, mode mask, durable no-replay, PR #58 preflight 통합 | **host capability freshness ordering defect OPEN** | preflight 이후 final Diagnostics capability refresh -> focused regression -> physical matrix |
| Generic SDO | dual-entry executor + R03 generic scalar Write + R04 editor/preview + R05 durable recovery + PR #58 safe-state correction 통합 | **global Write ON / hardware PASS 아님** | safe non-semantic 1/2/4-byte Write/readback 실기 |
| HomeDS402 | H37 software/source/WPF qualification 통합 | activation OFF | fresh artifact/C78 -> hardware matrix |
| HomeDS402Ex | SDK, ownership, retained store, approved-plan gate, WPF recovery 존재 | physical runtime/activation OFF | approved profile + fresh C78 -> runtime/hardware qualification |
| SetPosition | lifecycle + WPF recovery + host factory receipt/readback tooling 존재 | native runtime activation OFF | issue #44 vendor CRC + generated `_FileSys` ABI -> A/B backend -> RT exactly-once |

---

## 2. SetOperationMode — 현재 판정

### 2.1 activation/source truth

현재 `dev` source는 qualification-active 상태다.

`LMCDiagnosticsService.st`:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE`
- `LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE`

`LMCControlCommandService.st` AdminCapabilities:

- feature mask `0x00000717`
- software supported mode mask `0x018A` = PP(1), PV(3), IP(7), CSP(8)

따라서 WPF의 `AdminTriad=True`, `SupportedModeMask=0x018A`, `DiagnosticsIdentity=True`는 source와 일치한다.

### 2.2 구현된 software path

- target: PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- live PLC advertised mode 확인
- durable pre-dispatch arm / exact outcome / exact-generation retire
- Start no-replay recovery
- requested / preflight / observed mode 및 DS402/write evidence diagnostics
- raw Generic SDO `0x6060` 금지
- cross-mode fresh drive-status safety preflight

### 2.3 17:28 실기 로그에서 새로 확인된 blocker

실행 identity:

```text
LasalMotionControlApiExample.exe Version=0.9.1.0
BuildUtc=2026-08-28 08:27:44 UTC
LasalMotionControlLib.dll BuildUtc=2026-08-28 08:27:41 UTC
```

Axis1 current mode CSP(8)에서 PP/PV/IP 요청 모두:

```text
SetOperationMode cross-mode preflight passed
StatusWord=0x02D0
```

까지 도달했다. 따라서 이번 재현은 software supported-mode selector나 cross-mode safety preflight reject가 아니다.

그 직후 모든 요청이 동일 오류로 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

source trace 결과:

```text
RefreshDiagnosticsCapabilitiesAsync()
    -> cached capability observation N
ReadDriveStatusAsync()
    -> inline D5 0x6041 submission
       -> Diagnostics.GetCapabilities() -> N+1
    -> inline D5 0x6061 submission
       -> Diagnostics.GetCapabilities() -> N+2
PrepareSetOperationMode(cached N)
    -> requireCurrentObservation=true
    -> N != current N+2
    -> InvalidOperationException
```

즉 **PR #58에서 추가한 fresh preflight가 자체적으로 Diagnostics capability observation sequence를 진행시켜, 직전에 캐시한 capability를 stale로 만드는 execution-order bug**다.

이 실패는 `PrepareSetOperationMode()` capability validation 단계에서 발생하므로 현재 로그에서는:

- durable journal arm 전
- `0x7D23 Start` 전
- 실제 `0x6060` Write 전

이다. 따라서 PP/PV/IP가 PLC에서 거부됐다고 판정하면 안 된다. 아직 mutation wire까지 도달하지 못했다.

### 2.4 D5 terminal wake 로그 판정

각 시도 주변에 다음 로그가 반복된다.

```text
D5 terminal wake ignored: no exact current retained ticket
```

fresh preflight는 `0x6041`과 `0x6061`을 inline D5 SDO ticket으로 읽으며, 이 activity가 terminal wake callback을 발생시킬 수 있다. 현재 재현에서는 해당 메시지를 primary fault로 보지 않는다. primary fault는 위 capability observation sequence mismatch다.

### 2.5 corrective direction

freshness fence 자체를 제거하지 않는다. 수정 순서는 다음으로 고정한다.

```text
Admin capability refresh / requested mode advertise 확인
-> GetPhysicalAxis
-> ReadDriveStatusAsync fresh preflight
-> FINAL Diagnostics capability refresh
-> Ensure capability/admission ready
-> PrepareSetOperationMode
-> durable ArmBeforeDispatch
-> Start exactly once
```

유지할 계약:

- `requireCurrentObservation=true`
- stable Build/BootId/MapRevision
- Standstill/Fault/OperationEnabled fence
- one-shot confirmation
- durable no-replay
- raw `0x6060` Generic SDO block

추가 regression은 old observation이 preflight 후 stale이 되는 사실과, final refresh 후 Prepare가 성공하는 ordering을 직접 고정해야 한다.

---

## 3. Generic SDO — 실제 source truth

### 3.1 current activation state

`LMCDiagnosticsService.st`:

- `LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE`
- `LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE`

### 3.2 완료된 software tranche

- **SDO-R02**: `LMCSdoExecutor` manual Server / programmatic API dual-entry arbitration
- **SDO-R03**: physical axis 1..4 generic scalar Write policy, canonical 1/2/4-byte width
- **SDO-R04**: WPF arbitrary request editor, exact preview, reserved/semantic warning
- **SDO-R05**: v3 durable exact-request identity, endpoint/build/BootId/MapRevision binding, restart read-only recovery, legacy v1/v2 zero-wire

### 3.3 PR #58 safe-state correction

ordinary Generic SDO Write 요구조건:

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

이 evidence는 source/PC contract다. 실제 drive object Write/readback hardware PASS를 대신하지 않는다.

---

## 4. repository / 운영 상태

2026-08-28 branch cleanup 결과 remote branch는 현재:

- `main`
- `dev`

두 개만 유지한다. 기존 29개 `codex/*` branch는 모두 `dev` ancestor임을 확인한 뒤 삭제했다.

운영 규칙:

- `dev`가 유일한 integration / current qualification source truth다.
- 실기 전 source SHA + C78 artifact + PLC loaded image + WPF source를 하나의 identity set으로 묶는다.
- source CI PASS와 hardware PASS를 분리한다.
- qualification 중 발견된 blocker는 branch를 늘리기보다 current `dev` 설계/status에 먼저 반영한다.

---

## 5. 현재 작업 순서

1. **DONE** — Operation Mode / Generic SDO corrective source `dev` merge (#58)
2. **DONE** — stale qualification branch 및 29개 `codex/*` branch 정리
3. **DONE** — 17:28 live failure를 host capability freshness ordering defect로 root-cause 분석
4. **NOW** — SetOperationMode preflight 이후 final Diagnostics capability refresh ordering 수정
5. focused API/WPF regression 추가: stale old observation reject + final-current observation Prepare success
6. updated `dev` WPF/API build
7. 필요 시 exact source로 LASAL C78/PLC identity 재확인
8. Axis1 SetOperationMode PP/PV/IP/CSP physical matrix
9. Axis1 Generic SDO physical Write/readback matrix
10. failure/recovery matrix -> Axis2..4
11. production release activation review

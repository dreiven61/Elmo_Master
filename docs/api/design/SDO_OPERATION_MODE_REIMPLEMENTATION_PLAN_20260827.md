# Generic SDO / SetOperationMode 재구현 및 qualification 계획

- 최초 기준일: 2026-08-27
- current revision: 2026-08-28
- current integration / qualification source: `dev`
- corrective functional baseline: `687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
- tracking: issue #46
- production release posture: **NO-GO**

이 문서는 2026-08-27에 시작한 Generic SDO / SetOperationMode 재구현 계획을 현재 source truth에 맞게 갱신한다. 과거 branch-local 상태표는 Git history에 남기고, 이 파일은 앞으로 **현재 `dev` 구현과 남은 physical/release gate만 관리**한다.

---

## 0. 사용자 요청 3건 current audit

| ID | 사용자 요청 | current `dev` 상태 | 완료 판정 |
|---|---|---|---|
| P0-1 | SetOperationMode를 CSP 외 PP/PV/IP로 실제 전환 | PP/PV/IP/CSP lifecycle, mask, recovery, corrective preflight 구현 및 qualification activation ON | **software 구현 완료 / physical 미완료** |
| P0-2 | Generic SDO Write를 `0x2F00:24` 외 arbitrary safe object에 사용 | generic 1/2/4-byte scalar policy + WPF editor + durable exact-request recovery + corrective safe-state policy 통합 | **software 구현 완료 / physical 미완료** |
| P0-3 | `LMCSdoExecutor` 기존 Server Read/Write와 programmatic API 공존 | dual-entry arbitration source 통합, C78/basic smoke 이력 존재 | **source 구현 완료 / detailed bench 미완료** |

현재 병목은 R03/R04/R05 source 구현이 아니라 **PR #58까지 포함한 exact current `dev`를 fresh C78/PLC image로 만들고 물리 동작을 검증하는 것**이다.

---

## 1. tranche status

| ID | 내용 | current 상태 | software evidence | 다음 gate |
|---|---|---|---|---|
| SDO-R01 | regression fixture | **완료** | generic width/policy/WPF/durable regression 존재 | physical regression에 재사용 |
| SDO-R02 | `LMCSdoExecutor` dual-entry | **source 구현 완료** | source verifier + prior C78/basic PLC smoke | manual/programmatic physical contention |
| SDO-R03 | Generic Write policy 일반화 | **완료** | PR #55 + policy verifier | safe-object physical Write/readback |
| SDO-R04 | WPF arbitrary-target editor / preview | **완료** | PR #56 + focused WPF smoke | physical operator path |
| SDO-R05 | Generic Write durable recovery | **완료** | PR #57 + API full suite | timeout/disconnect physical recovery |
| SDO-R06 | 2026-08-28 live-bench corrective safe-state policy | **완료** | PR #58, corrective verifier | current-image physical retest |
| MODE-R01 | supported mode capability + diagnostics | **완료** | `0x018A`, WPF/SDK coupling, MODE-11F | current-image live check |
| MODE-R02 | PP/PV/IP/CSP software execution | **완료** | requested-mode lifecycle, warm recovery | physical cross-mode matrix |
| MODE-R03 | live-bench corrective preflight/gate visibility | **완료** | PR #58 | physical cross-mode matrix |
| REL-R01 | release/distribution | **미완료** | qualification-active source | physical matrix 후 release review |

---

## 2. current activation truth

### SetOperationMode

`LMCDiagnosticsService.st`:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
```

`LMCControlCommandService.st`:

```text
Admin FeatureMask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

현재 `dev`는 SetOperationMode에 대해 **qualification-active**다. 과거 `gate OFF` 설명은 current source에 적용하지 않는다.

### Generic SDO

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

R03 이후 arbitrary generic scalar Write admission은 Axis1 UI24 exact-address preset과 분리된 global policy를 사용한다.

qualification activation ON은 production release 승인과 동의어가 아니다.

---

## 3. SDO-R02 — `LMCSdoExecutor` dual-entry

current source contract:

- `RequestSource = NONE / MANUAL_SERVER / PROGRAMMATIC`
- manual `ParaReadWrite::Write`가 실제 vendor Read/Write dispatch
- programmatic `TryStartRead/TryStartWrite`
- manual/programmatic BUSY arbitration
- callback source identity 분기
- programmatic ticket/session/token identity fence
- orphan/drain/reusable contract

남은 bench:

1. manual `0x6061` Read
2. safe manual Write + exact readback
3. manual request 중 programmatic request BUSY/no-wire
4. programmatic request 중 manual request BUSY/no-wire
5. completion 후 executor reusable

---

## 4. SDO-R03 — Generic scalar Write policy

### 구현 완료

physical axis 1..4의 generic scalar shape:

- 8-bit: 1 byte
- 16-bit: 2 bytes
- 32-bit: 4 bytes

request ValueType와 wire width가 canonical하게 일치해야 한다.

Bool raw value는 canonical 0/1만 허용한다.

### semantic/dedicated-owner raw blocklist

다음 object는 generic raw Write에서 계속 fail-closed다.

- `0x6040` Controlword
- `0x6060` Modes of operation
- `0x607A` Target position
- `0x60FF` Target velocity
- `0x6071` Target torque
- `0x3204`
- `0x20FC`

SetOperationMode의 mode mutation을 generic SDO로 우회하지 않는다.

---

## 5. SDO-R04 — WPF arbitrary-target editor

구현 완료:

- slave/axis reference
- object index
- subindex
- value type
- scalar value
- timeout
- exact request preview
- little-endian WriteData preview
- semantic/reserved Write warning
- submission 전 동일 policy boundary 재검사

WPF 화면에 입력 surface가 있다고 hardware Write가 성공한 것으로 간주하지 않는다.

---

## 6. SDO-R05 — durable exact-request no-replay recovery

new durable record는 v3 identity를 사용한다.

- endpoint IP/port
- DiagnosticsBuild
- BootId
- MapRevision
- exact slave/index/subindex/type/length/value

recovery 원칙:

- original Write 자동 replay 금지
- exact readback으로 terminal proof
- identity mismatch는 zero-wire
- legacy v1/v2는 읽을 수 있어도 automatic recovery wire는 금지
- semantic/dedicated-owner target은 durable generic recovery 대상으로 만들지 않음

---

## 7. SDO-R06 — 2026-08-28 Generic SDO bench corrective

실기에서 ordinary Write가 동작하지 않은 software 원인을 두 층에서 수정했다.

### WPF

ordinary Write가 same-value qualification helper의 `PowerOn=False`를 재사용하던 것을 분리했다.

same-value qualification:

```text
PowerOn=False
Standstill=True
Fault=False
OperationEnabled=False
```

ordinary generic Write:

```text
Standstill=True
Fault=False
OperationEnabled=False
```

### PLC

기존 generic policy는 safe axis state를 사실상 DS402 base `0x40` 하나로 제한했다.

현재 허용:

- `0x40` Switch On Disabled
- `0x21` Ready To Switch On
- `0x23` Switched On

계속 거부:

- `0x27` Operation Enabled
- fault/unsafe state
- semantic/dedicated-owner blocklist

안전 fence를 제거한 것이 아니라 ordinary engineering Write에 필요한 non-enabled safe states를 정확히 분리한 것이다.

---

## 8. MODE-R01/R02 — SetOperationMode current path

지원 mode:

- PP = 1
- PV = 3
- IP = 7
- CSP = 8

Homing(6)은 별도 semantic owner가 담당한다.

normal semantic sequence:

```text
fresh 0x6061 / axis status / 0x6041 preflight
    -> same target: no 0x6060 Write
    -> cross-mode: exact one-byte 0x6060 Write(requested mode)
    -> 0x6061 verify
    -> terminal outcome
    -> exact-generation retire
```

post-write uncertainty에서는 original Start/`0x6060` Write를 replay하지 않는다.

---

## 9. MODE-R03 — 2026-08-28 SetOperationMode bench corrective

### CSP-only 오판

CSP -> CSP는 current `0x6061`이 이미 8이면 `SucceededNoWrite`가 가능하다. 이 결과는 실제 `0x6060` Write evidence가 아니다.

### cross-mode fresh preflight

다른 mode로 전환할 때 WPF가 durable Start 전에 fresh drive status를 읽고 다음을 요구한다.

- `Standstill=True`
- DS402 Fault=False
- DS402 OperationEnabled=False

OperationEnabled에서 mode change를 허용하도록 완화하지 않는다.

### actual WPF gate diagnostics

Start status는 다음을 직접 표시한다.

- `AdminTriad`
- `SupportedModeMask`
- `DiagnosticsIdentity`
- `Confirmed`
- `SelectedModeAdvertised`
- `AdmissionAllowed`
- `JournalReady`

one-shot confirmation checkbox가 unchecked이면 `Confirmed=False`이며 Start disabled가 정상이다.

---

## 10. software validation checkpoint

PR #58 corrective source 기준:

- API Debug full suite: **1200/1200 PASS**
- Generic SDO WPF focused smoke: **17/17 PASS**
- API Debug/Release build: PASS
- WPF Debug/Release build: PASS
- `Verify-HardwareFindingFix.ps1`: PASS
- `Verify-LasalGenericSdoWrite.ps1`: PASS
- diff hygiene: PASS

이 checkpoint는 hardware PASS를 주장하지 않는다.

---

## 11. branch 정리

기존 `codex/setopmode-mode11-bench-activation` / PR #18은 old gate-OFF baseline에서 activation-only 용도로 만들어졌다.

2026-08-28 재검토 결과:

- current `dev` 자체가 qualification-active
- PR #18 closed as superseded
- branch ref를 current `dev`와 동일하게 정렬
- independent diff 제거

추가 SetOperationMode qualification branch를 만들지 않는다.

---

## 12. physical qualification plan

### 12.1 exact-source identity gate

실기 전 하나의 evidence set에 기록:

- current `dev` SHA
- LASAL build start/end
- fresh `Classes.lcb` / project lcb / Network lcb identity
- C78/ARM zero-error/link success
- loaded PLC image identity
- WPF source SHA

이 identity가 어긋나면 기능 failure 분석을 진행하지 않고 먼저 stale-image 문제를 해결한다.

### 12.2 Axis1 SetOperationMode matrix

| current | requested | expected mutation |
|---|---|---|
| CSP | CSP | zero-write / `SucceededNoWrite` |
| CSP | PP | exact one `0x6060=1` |
| CSP | PV | exact one `0x6060=3` |
| CSP | IP | exact one `0x6060=7` |
| PP/PV/IP | CSP | exact one `0x6060=8` |
| target X | same X | zero-write |

각 cross-mode row는 `0x6061` exact readback까지 확인한다.

### 12.3 Axis1 Generic SDO matrix

safe non-semantic object를 선정해:

- 1-byte Write + exact readback
- 2-byte Write + exact readback
- 4-byte Write + exact readback
- manual/programmatic contention
- timeout/disconnect/readback mismatch recovery

object는 semantic/dedicated-owner blocklist 밖이어야 한다.

### 12.4 확대

Axis1 정상/실패 matrix가 닫힌 뒤 Axis2..4로 확대한다.

---

## 13. release boundary

physical qualification이 모두 PASS하더라도 current qualification-active 값을 그대로 production release로 간주하지 않는다.

release review에서 별도로 결정:

- SetOperationMode activation 유지 여부
- Generic SDO global Write activation 범위
- per-axis qualification 범위
- generated artifact identity
- distribution/documentation
- rollback/deactivation strategy

production release 전까지 판정은 **NO-GO**다.

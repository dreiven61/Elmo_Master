# 개발 상태 스냅샷 — 2026-08-28

- current integration branch: `dev`
- current source baseline: `dev@687a78c6e97616870c4fec4a5da043046bb735f6`
- production posture: **NO-GO**
- active P0 tracking: issue #46
- supersedes the current-state portions of `DEVELOPMENT_STATUS_20260827.md`

이 문서는 branch 수나 PR 수가 아니라 **현재 `dev`의 실제 실행 경로**를 기준으로 상태를 판정한다. `source/PC test`, `fresh LASAL C78 artifact`, `PLC load`, `physical wire/effect`, `production activation`은 각각 별도 gate다. source와 CI가 완료돼도 해당 source로 C78/PLC 이미지를 다시 만들고 실기 검증하기 전에는 hardware PASS로 올리지 않는다.

---

## 1. 현재 P0 요약

| 영역 | current `dev` 구현 상태 | 실기/배포 상태 | 다음 gate |
|---|---|---|---|
| SetOperationMode | PP/PV/IP/CSP lifecycle, supported-mode mask, durable no-replay recovery, MODE-11E/11F diagnostics, 2026-08-28 bench corrective preflight까지 통합 | **hardware PASS 아님** | current-dev qualification image 재생성 -> fresh C78/PLC -> Axis1 PP/PV/IP/CSP physical matrix |
| Generic SDO | dual-entry executor + R03 generic scalar Write + R04 arbitrary editor/preview + R05 durable exact-request recovery + bench corrective safe-state policy 통합 | **hardware PASS 아님** | current-dev C78/PLC -> safe non-semantic 1/2/4-byte Write/readback -> contention/recovery matrix |
| HomeDS402 | H37 software/source/WPF qualification 통합 | activation OFF | fresh generated artifact/C78 -> hardware matrix |
| HomeDS402Ex | SDK, ownership, retained store, approved-plan gate, WPF recovery 존재 | physical runtime/activation OFF | approved hardware profile + fresh C78 -> runtime/hardware qualification |
| SetPosition | lifecycle + WPF durable no-replay recovery + host factory receipt/readback tooling 존재 | native runtime activation OFF | issue #44 vendor CRC + generated `_FileSys` ABI -> A/B backend -> RT exactly-once |

---

## 2. SetOperationMode — 현재 구현 truth

### 2.1 `dev`에 실제 들어간 것

- supported software target: PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유이며 SetOperationMode에서 받지 않음
- Admin capability triad + `SetOperationModeSupportedMask`; known software mask `0x018A`
- SDK/WPF Start는 live PLC가 선택 mode를 광고하지 않으면 zero-wire fail-closed
- exact one-shot durable journal / outcome / exact-generation retire / Start no-replay recovery
- warm-start recovery를 PP/PV/IP/CSP로 일반화
- requested / preflight / observed mode 및 DS402/write evidence diagnostics
- generic raw SDO `0x6060`은 계속 금지; mode mutation은 SetOperationMode lifecycle만 소유

### 2.2 2026-08-28 실기 피드백으로 수정한 것

PR #58 / `dev@687a78c6...`에서 다음을 수정했다.

1. **CSP 성공 오판 제거**
   - CSP -> CSP는 current `0x6061`이 이미 target이면 `SucceededNoWrite`가 될 수 있다.
   - 이 결과를 실제 `0x6060` cross-mode Write 성공 증거로 취급하지 않는다.

2. **cross-mode fresh preflight 추가**
   - WPF가 durable Start를 arm하기 전에 fresh drive status / `0x6041` / `0x6061`을 읽는다.
   - 다른 mode로 전환할 때는 `Standstill=True`, DS402 Fault=False, OperationEnabled=False를 요구한다.
   - 이 안전 fence는 완화하지 않는다.

3. **WPF Start gate 가시화**
   - 기존의 고정 문구 `activation 때문에 Start disabled`를 제거했다.
   - status에 `AdminTriad`, `SupportedModeMask`, `DiagnosticsIdentity`, `Confirmed`, `SelectedModeAdvertised`, `AdmissionAllowed`, `JournalReady`를 표시한다.
   - Start는 표시된 gate + valid axis/timeout + idle 조건이 모두 참일 때만 활성화된다.

### 2.3 아직 완료가 아닌 것

- current `dev` corrective source를 포함한 fresh C78/ARM Rebuild + Link
- 그 exact image의 PLC load
- Axis1 PP/PV/IP/CSP same-mode / cross-mode 실기 결과
- cross-mode `0x6060:0` exact-one-write evidence
- timeout/disconnect/quarantine/recovery physical matrix
- Axis2..4 확장
- production activation review

**중요:** 기존 `codex/setopmode-mode11-bench-activation`은 corrective merge 전 source에서 갈라진 stale qualification branch였으므로 current `dev` 기준으로 재생성해야 한다. 과거 branch에서 얻은 CSP-only 결과를 current source completion으로 간주하지 않는다.

---

## 3. Generic SDO — 현재 구현 truth

### 3.1 완료된 software tranche

- **SDO-R02**: `LMCSdoExecutor` manual Server / programmatic API dual-entry arbitration
- **SDO-R03**: arbitrary physical axis 1..4 generic scalar Write policy, canonical 1/2/4-byte width
- **SDO-R04**: WPF arbitrary request editor, exact request preview, semantic/reserved Write warning
- **SDO-R05**: v3 durable exact-request identity, endpoint/build/BootId/MapRevision binding, restart read-only recovery, legacy v1/v2 zero-wire

따라서 이전 문서의 `R03 -> R04 -> R05 구현 예정` 표기는 더 이상 current 상태가 아니다.

### 3.2 2026-08-28 실기 피드백으로 수정한 것

ordinary Generic SDO Write가 same-value qualification용 `PowerOn=False` 사전조건을 그대로 재사용해 valid write를 wire 전에 막고 있었고, PLC generic policy도 DS402 base `0x40`만 허용하고 있었다.

PR #58에서:

- same-value qualification은 기존 strict PowerOff 조건 유지
- ordinary generic Write는 `Standstill=True + Fault=False + OperationEnabled=False`로 분리
- PLC는 semantic/dedicated-owner object가 아닌 generic scalar Write에 한해 다음 safe non-enabled DS402 base state를 허용
  - `0x40` Switch On Disabled
  - `0x21` Ready To Switch On
  - `0x23` Switched On
- `0x27` Operation Enabled, fault/unsafe state는 계속 거부
- semantic/dedicated-owner raw blocklist 유지:
  - `0x6040`, `0x6060`, `0x607A`, `0x60FF`, `0x6071`, `0x3204`, `0x20FC`

### 3.3 software evidence

corrective promotion 기준:

- API Debug full suite: **1200/1200 PASS**
- Generic SDO WPF focused smoke: **17/17 PASS**
- API/WPF Debug build: PASS
- API/WPF Release build: PASS
- `Verify-HardwareFindingFix.ps1`: PASS
- `Verify-LasalGenericSdoWrite.ps1`: PASS
- diff hygiene: PASS

이 evidence는 source/PC contract다. 실제 slave object Write/readback hardware PASS를 대신하지 않는다.

### 3.4 다음 실기 gate

1. exact current `dev` source로 LASAL C78/ARM rebuild + PLC load
2. physical axis1, safe non-semantic object 대상으로 1/2/4-byte Write
3. exact readback value/width 확인
4. manual/programmatic BUSY arbitration 및 no-wire 확인
5. timeout/disconnect/readback mismatch durable recovery
6. Axis2..4 확대

---

## 4. 실기용 branch 운영 규칙

기존 문제는 구현 branch와 qualification branch가 장기간 분리되면서 **실기 이미지가 최신 구현 source를 포함하지 않는 상태**가 된 것이다. 이후 규칙을 다음처럼 고정한다.

- 기능 source는 `dev`가 유일한 integration truth다.
- 실기용 qualification branch는 새 기능 branch를 계속 만들지 않고, 기존 qualification branch를 **현재 `dev`에서 재생성**한다.
- qualification branch delta는 activation/bench-only 값으로 최소화한다.
- 실기 전 `dev SHA`, qualification SHA, LASAL build artifact, PLC loaded image를 같은 evidence set으로 묶는다.
- 실기 실패가 나오면 먼저 exact loaded SHA/source identity를 확인한 뒤 source bug와 stale-image 문제를 분리한다.
- qualification PASS 전 production `dev` activation gate를 열지 않는다.

---

## 5. 현재 작업 순서

1. **DONE** — 2026-08-28 Operation Mode / Generic SDO corrective source를 `dev` merge (#58)
2. **NOW** — 기존 SetOperationMode qualification branch를 `dev@687a78c6...`에서 재생성하고 activation-only delta로 제한
3. fresh C78/ARM Rebuild + Link + exact artifact identity
4. PLC load 후 WPF를 동일 source 기준으로 실행
5. Axis1 Operation Mode PP/PV/IP/CSP physical matrix
6. Generic SDO safe-object physical Write/readback matrix
7. 실패/복구 matrix
8. 결과에 따라 source correction 또는 qualification closure

새로운 기능 branch를 만드는 것은 위 순서에서 독립적인 source 변경이 실제로 필요한 경우에만 한다.

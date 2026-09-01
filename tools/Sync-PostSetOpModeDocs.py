from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def read(rel):
    return (ROOT / rel).read_text(encoding="utf-8")


def write(rel, text):
    (ROOT / rel).write_text(text, encoding="utf-8", newline="\n")


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise SystemExit(f"{label}: expected one exact match, found {count}")
    return text.replace(old, new, 1)


def replace_regex(text, pattern, replacement, label, flags=re.S):
    updated, count = re.subn(pattern, replacement, text, count=1, flags=flags)
    if count != 1:
        raise SystemExit(f"{label}: expected one regex match, found {count}")
    return updated


# ---------------------------------------------------------------------------
# API manual
# ---------------------------------------------------------------------------
manual_path = "docs/api/API_MANUAL.md"
manual = read(manual_path)
manual = replace_once(manual, "문서 버전: 2.5-development", "문서 버전: 2.6-development", "manual version")
manual = replace_once(manual, "기준일: 2026-08-31", "기준일: 2026-09-01", "manual date")
manual = replace_once(
    manual,
    "| 2.5-development | 2026-08-31 | SetOperationMode PP/PV/IP/CSP qualification-active 계약, Generic SDO R03~R05, branch cleanup, 17:28 capability freshness ordering blocker와 current 실기 절차 반영 |",
    "| 2.5-development | 2026-08-31 | SetOperationMode PP/PV/IP/CSP qualification-active 계약, Generic SDO R03~R05, branch cleanup, 17:28 capability freshness ordering blocker와 current 실기 절차 반영 |\n"
    "| 2.6-development | 2026-09-01 | SetOperationMode 구현 완료 상태, exact requested-mode ACK, one-shot 0x6060/read-only settling, bounded owner publish, durable outcome/retire 및 남은 기능 로드맵 정렬 |",
    "manual revision row",
)

setop_manual = r'''### 3.11.1 SetOperationMode

2.6-development의 SetOperationMode는 current `dev`에서 구현 완료된 single-axis operation-mode
변경 lifecycle이다. 지원 mode는 PLC가 `SetOperationModeSupportedMask=0x018A`로 광고하는
PP(1), PV(3), IP(7), CSP(8)이며 Homing(6)은 HomeDS402/HomeDS402Ex가 소유한다.

```csharp
public LMCPreparedAxisSetOperationMode PrepareSetOperationMode(
    LMCDriveOperationMode requestedMode,
    uint timeoutMilliseconds,
    LMCAdminCapabilities verifiedCapabilities,
    LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
    LMCAxisSetOperationModeExecuteToken executeToken)

public LMCAxisSetOperationModeStartAcknowledgement SetOperationMode(
    LMCPreparedAxisSetOperationMode preparedCommand)
public Task<LMCAxisSetOperationModeStartAcknowledgement> SetOperationModeAsync(
    LMCPreparedAxisSetOperationMode preparedCommand,
    CancellationToken cancellationToken)

public LMCAxisSetOperationModeOutcomeResult ReadSetOperationModeOutcome(
    LMCAxisSetOperationModeRecoveryKey recoveryKey,
    LMCAdminCapabilities verifiedCapabilities,
    LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)

public LMCAxisSetOperationModeOutcomeRetirementResult RetireSetOperationModeOutcome(
    LMCAxisSetOperationModeRecoveryKey recoveryKey,
    uint recordGeneration,
    LMCAdminCapabilities verifiedCapabilities,
    LMCDiagnosticCapabilities verifiedDiagnosticCapabilities)
```

비동기 Outcome/Retire overload와 terminal-outcome 기반 Retire overload도 제공한다. wire command는
Start `0x7D23`, exact outcome query `0x7D24`, exact-generation retirement `0x7D25`다.
`PrepareSetOperationMode`는 wire를 보내지 않으며 current capability/identity와 execute token을
검증한다. prepared Start는 one-shot이다.

| 항목 | current 계약 |
|---|---|
| 지원 mode | PP(1), PV(3), IP(7), CSP(8) |
| same-target | 이미 `0x6061`이 requested mode면 `SucceededNoWrite` 가능 |
| cross-mode preflight | Standstill=True, DS402 Fault=False, OperationEnabled=False |
| mode write | exact requested mode를 `0x6060:0`에 최대 1회 dispatch |
| verify | `0x6061:0` exact readback; 반영 지연은 original deadline 안에서 read-only 재확인 |
| recovery | original `0x7D23`/`0x6060` replay 금지; exact-key query만 사용 |
| terminal | owner release와 executor reusable evidence를 포함한 terminal outcome |
| retire | terminal record의 exact generation과 identity가 일치할 때만 `0x7D25` |

Start ACK는 completion evidence가 아니다. TCP 계층은 ACK와 well-shaped domain failure에서
CSP 상수값을 기대하지 않고 **exact requested mode**를 echo/검증한다. 따라서 PP/PV/IP/CSP가
동일한 wire 계약을 사용한다.

### 3.11.2 실행 완료와 durable recovery

WPF current path는 Start 전에 physical axis와 fresh drive status를 읽고, preflight가 내부 D5
capability observation sequence를 변경한 뒤 **FINAL Diagnostics capability refresh**를 수행한다.
그 current observation으로 Prepare한 뒤 durable journal을 먼저 arm하고 Start를 정확히 한 번만
보낸다. freshness, Build/BootId/MapRevision, Standstill/Fault/OperationEnabled fence는 생략하지 않는다.

Start가 Running이면 WPF는 exact recovery key로 `0x7D24`를 반복 조회한다. 이 조회는 read-only이며
original Start를 replay하지 않는다.

- `Succeeded`: terminal evidence 저장 -> exact-generation `0x7D25` retire -> PASS.
- `Failed` / `Aborted`: terminal evidence 저장과 retire를 완료한 뒤 실패로 반환한다. 실패를 PASS로
  바꾸지 않는다.
- Running 지속, outcome query reject 또는 indeterminate: durable record와 mutation interlock을
  유지한다. `CloseConnection`은 허용되지만 motion stop, 결과 확정 또는 record 해제를 뜻하지 않는다.

PLC는 정상 `0x6061` readback이 requested mode와 다르더라도 original operation deadline을 늘리지
않은 채 최소 50 ms 간격으로 read-only verify를 재시도한다. write callback/owner 상태가 불확정이면
fail-closed quarantine을 유지한다. terminal owner publish/release도 original deadline 안에서만 bounded
retry하며 이 과정에서 새 SDO Write를 만들지 않는다.

raw Generic SDO Write로 `0x6060`을 직접 우회하는 경로는 permanent deny 상태다. SetOperationMode의
과거 capability-freshness, readback settling, owner publish 및 CSP-fixed ACK 원인 분석은
`design/SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md`에 historical evidence로 보존한다.
SetOperationMode 기능 구현 완료와 전체 `0.9.1-preview` production release 승인은 별개다.

'''
manual = replace_regex(
    manual,
    r"### 3\.11\.1 SetOperationMode current qualification contract.*?(?=## 3\.12 Move 완료와 restart recovery 경계)",
    setop_manual,
    "manual SetOperationMode section",
)
write(manual_path, manual)


# ---------------------------------------------------------------------------
# Development progress
# ---------------------------------------------------------------------------
progress_path = "docs/api/API_DEVELOPMENT_PROGRESS.md"
progress = read(progress_path)
progress = replace_once(progress, "- 문서 버전: 1.3-current", "- 문서 버전: 1.4-current", "progress version")
progress = replace_once(progress, "- 기준일: 2026-08-31", "- 기준일: 2026-09-01", "progress date")
progress = replace_regex(
    progress,
    r"- reviewed source baseline before this docs sync: `[^`]+`",
    "- current source baseline: `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)",
    "progress baseline",
)
progress = replace_regex(
    progress,
    r"- SetOperationMode는 PP/PV/IP/CSP lifecycle, supported mask `0x018A`.*?\n- Generic SDO는",
    "- SetOperationMode는 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`에서 구현 완료로 닫혔다. PP/PV/IP/CSP, exact requested-mode ACK, one-shot `0x6060`, read-only `0x6061` settling, bounded owner publish, durable no-replay outcome/retire와 WPF terminal 처리까지 current Active 계약이다.\n- Generic SDO는",
    "progress SetOp summary",
)
progress = replace_regex(
    progress,
    r"- WPF dynamic SetOperationMode/HomeDS402Ex recovery localization.*?\n- full SourceOnly의 current known downstream blocker.*?\n  artifact identity는 fresh C78 build \+ review 없이 자동 갱신하지 않는다\.",
    "- SetOperationMode 기능 구현은 완료됐지만 repository qualification hygiene는 별도 관리한다. current SetOperationMode static run은 기능/안전 contract 93개가 PASS하고 LASAL metadata Client 순서와 generated declaration 순서 불일치 1건에서 멈췄다. current WPF workflow failure는 hosted runner의 MSBuild 탐색 단계에서 발생해 test body가 실행되지 않은 CI 환경 문제다.\n- generated LASAL artifact identity와 repository-wide SourceOnly ratchet은 HomeDS402/HomeDS402Ex 등 남은 기능의 release qualification에서 계속 명시적으로 검토하며 자동 갱신하지 않는다.",
    "progress CI hygiene summary",
)
progress = replace_once(
    progress,
    "- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home, SetOperationMode), Dormant 1(HomeDS402Ex)",
    "- High-priority 21개 관점: Active 18, Partial 2(SetPosition, DS402 Home), Dormant 1(HomeDS402Ex)",
    "progress high-priority tally",
)
progress = replace_regex(
    progress,
    r"\| SetOperationMode \| `0x7D23/7D24/7D25` \| Limited \|.*?\|",
    "| SetOperationMode | `0x7D23/7D24/7D25` | Active | 구현 완료: PP/PV/IP/CSP, exact requested-mode ACK, one-shot write/read-only verify, durable no-replay outcome/retire |",
    "progress SetOp table row",
    flags=0,
)

setop_progress = r'''## 6. SetOperationMode 완료 checkpoint

SetOperationMode feature implementation은 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`
(`dev : SetOpMode Complete`)에서 완료 상태로 닫는다.

current source contract:

- qualification/runtime activation ON
- Admin Start/Outcome/Retire triad ON
- `SetOperationModeSupportedMask=0x018A` = PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- Start `0x7D23`, ReadOutcome `0x7D24`, Retire `0x7D25`
- cross-mode fresh drive preflight: Standstill=True, Fault=False, OperationEnabled=False
- same-target `SucceededNoWrite`와 real cross-mode write 구분
- exact requested mode ACK/domain-failure echo; CSP(8) 고정 ACK 판정 제거
- `0x6060` exact requested-mode write 최대 1회
- normal verify mismatch는 original deadline 안에서 50ms 이상 간격의 `0x6061` read-only settling
- write-dispatched 이후 original Start/`0x6060` automatic replay 0회
- terminal owner publish/release는 original deadline 안에서 bounded retry, 추가 SDO write 0회
- terminal owner released + executor reusable evidence를 outcome에 포함
- WPF Running은 PASS가 아니며 exact-key `0x7D24` polling 후 terminal proof를 보존
- Succeeded는 exact-generation retire 이후에만 PASS
- Failed/Aborted는 terminal evidence/retire 후 실패로 반환
- indeterminate/query reject는 durable record와 mutation fence 유지
- raw Generic SDO `0x6060` permanent deny

2026-08-28 capability freshness blocker, 2026-08-31 readback/owner-publish 조사와 CSP-fixed ACK root cause는
모두 current implementation에서 corrective가 반영된 **historical investigation**이다. 상세 chronology는
`design/SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md`에 보존한다.

current CI를 feature implementation status와 혼동하지 않는다.

- SetOperationMode C78 evidence tool run `33455821803`: SUCCESS
- SetOperationMode static run `33455821841`: functional/safety checks **93 PASS**, metadata Client/generated declaration order mismatch **1 FAIL**
- SetOperationMode WPF run `33455821887`: hosted runner `Locate MSBuild` 실패로 test body 미실행

후자의 두 항목은 repository/CI qualification hygiene로 남기되 SetOperationMode 기능 구현을 다시
미완료 상태로 되돌리지 않는다. 전체 API production release 판정은 다른 미완료 기능과 release gate
때문에 계속 NO-GO다.

'''
progress = replace_regex(
    progress,
    r"## 6\. SetOperationMode current checkpoint.*?(?=## 7\. HomeDS402Ex current checkpoint)",
    setop_progress,
    "progress SetOperationMode checkpoint",
)

priority_progress = r'''## 9. current 개발 우선순위

SetOperationMode 구현 완료 후 우선순위는 다음으로 재정렬한다.

1. **Generic SDO 완료 — issue #46의 잔여 범위**
   - Axis1 safe non-semantic 1/2/4-byte Write + exact readback
   - Manual Server / programmatic dual-entry BUSY arbitration과 race/no-wire 검증
   - timeout/disconnect/readback-mismatch durable no-replay recovery
   - Axis2..4 확대
2. **HomeDS402 — issue #32**
   - current exact `dev` C78/generated artifact review와 SourceOnly ratchet closure
   - same-image PLC/hardware 정상/fault/timeout matrix
   - activation은 독립 release gate 통과 전까지 OFF 유지
3. **HomeDS402Ex — issue #28 + #35**
   - 축1..4 wiring/polarity/method/scale profile 승인
   - fresh C78/generated artifact + SourceOnly closure
   - 이후에만 physical parameter program/restore와 homing runtime 진행
4. **SetPosition — issue #44**
   - vendor `CheckSum.CRC32` golden fixture 확보
   - LASAL IDE-generated `_FileSys` ABI 확보
   - 두 외부 prerequisite 없이는 durable A/B backend를 추측 구현하지 않음
5. **후순위 dormant/missing surface**
   - PI Write, Recorder Double, Dynamic node/DI, Extended SDO result activation 검토
   - `0x7E23` Digital Output Write LASAL route/owner/allowlist 구현
6. **Repository/release hygiene**
   - hosted Windows MSBuild discovery workflow 정리
   - LASAL metadata/generated declaration order 및 generated artifact ratchet 정합화
   - 기능별 source SHA / artifact / PLC image / WPF binary evidence set 정리

'''
progress = replace_regex(
    progress,
    r"## 9\. current 개발 우선순위.*?(?=## 10\. branch / qualification 상태)",
    priority_progress,
    "progress priorities",
)
progress = replace_once(
    progress,
    "현재 HomeDS402, SetOperationMode, HomeDS402Ex, SetPosition은 이 전체 gate를 닫지 못했으므로 production\n판정은 계속 **NO-GO**다.",
    "SetOperationMode feature implementation은 완료됐지만 Generic SDO physical completion, HomeDS402,\nHomeDS402Ex, SetPosition과 dormant/missing surface가 이 전체 gate를 닫지 못했다. 따라서 전체 API의\nproduction 판정은 계속 **NO-GO**다.",
    "progress production conclusion",
)
write(progress_path, progress)


# ---------------------------------------------------------------------------
# Design index: replace with a concise current source-of-truth index.
# ---------------------------------------------------------------------------
readme_path = "docs/api/design/README.md"
readme = r'''# 최우선 API 개발 설계

- 기준일: 2026-09-01
- current integration / qualification source: `dev`
- current baseline: `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)
- current status snapshot: `DEVELOPMENT_STATUS_20260901.md`
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0 implementation: **Generic SDO 잔여 범위** (issue #46)
- production release posture: **NO-GO**

이 폴더의 current 판정은 `dev` source와 최신 current snapshot을 우선한다.
`DEVELOPMENT_STATUS_20260827.md`, `DEVELOPMENT_STATUS_20260828.md`,
`DEVELOPMENT_STATUS_20260831.md`와 각 blocker 문서는 historical evidence로 보존한다.

---

## 1. 완료 — SetOperationMode

SetOperationMode는 `0afbc2a79dff1b63f908b1bde3bd2502843045ff`에서 구현 완료로 닫는다.

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

지원 mode는 PP(1), PV(3), IP(7), CSP(8)이며 Homing(6)은 HomeDS402/HomeDS402Ex가 소유한다.
current completion contract:

- `0x7D23` Start / `0x7D24` exact outcome / `0x7D25` exact-generation retire
- fresh drive-status preflight와 FINAL Diagnostics capability refresh
- same-target `SucceededNoWrite` / cross-mode write 구분
- exact requested-mode ACK/domain-failure echo; CSP 고정 판정 제거
- `0x6060` exact requested-mode write 최대 1회
- `0x6061` verify mismatch는 original deadline 안에서 read-only settling
- write-dispatched 이후 Start/`0x6060` replay 금지
- terminal owner publish/release bounded retry, 추가 SDO write 없음
- WPF Running polling, terminal evidence, exact retirement, false PASS 방지
- indeterminate/query reject durable fence 유지
- stale recovery operator retirement은 PLC success를 조작하지 않음
- Generic SDO raw `0x6060` permanent deny

상세 구현/원인 추적:

- `SET_OPERATION_MODE_DESIGN.md` — current implementation contract
- `SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md` — capability/readback/owner/ACK historical investigation
- `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md` — Start execution corrective
- `SET_OPERATION_MODE_DETAIL49_OBSERVABILITY_IMPLEMENTATION_RESULT_20260831.md` — admission/storage observability

SetOperationMode 구현 완료를 전체 API production 승인으로 확대 해석하지 않는다.

---

## 2. P0 — Generic SDO

issue #46은 SetOperationMode 부분을 완료 처리하고 **Generic SDO 잔여 범위만** 추적한다.

current source:

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

이미 통합된 범위:

- SDO-R02 Manual Server + tokenized programmatic dual-entry 기반
- SDO-R03 physical axis 1..4 generic 1/2/4-byte scalar Write policy
- SDO-R04 arbitrary WPF editor / exact preview / reserved warning
- SDO-R05 durable exact-request no-replay recovery
- ordinary Write safe-state correction

ordinary Generic SDO Write gate:

- Standstill=True
- DS402 Fault=False
- DS402 OperationEnabled=False
- PLC safe base state `0x40`, `0x21`, `0x23`

permanent semantic/dedicated-owner raw blocklist:

```text
0x6040
0x6060
0x607A
0x60FF
0x6071
0x3204
0x20FC
```

남은 완료 gate:

1. Axis1 safe non-semantic 1/2/4-byte Write + exact readback
2. Manual/programmatic simultaneous access -> BUSY/no race/no hidden write
3. timeout/disconnect/readback mismatch durable no-replay
4. Axis2..4 확대

Axis1 UI24 four-ticket path는 qualification preset일 뿐 generic API의 유일 target이 아니다.

---

## 3. P1 — HomeDS402

software/source/WPF qualification은 통합돼 있으나 activation은 OFF다.

- tracker: issue #32
- next: exact current `dev` C78/ARM Rebuild + Link
- generated `Classes.lcb`/project/network artifact identity review
- full SourceOnly ratchet closure
- same-image PLC/hardware normal/fault/timeout matrix
- 독립 activation review 전 bit 6/five-value activation은 OFF 유지

---

## 4. P1 — HomeDS402Ex

SDK/ownership/retained store/WPF recovery/profile-preparation source는 존재하지만 physical runtime과
capability activation은 OFF다.

- issue #28: axis1..4 wiring/polarity/homing method/scale/range profile 승인
- issue #35: fresh C78/generated artifact + SourceOnly closure

두 prerequisite가 닫히기 전에 hardware-dependent 값을 추측하거나 physical homing path를 열지 않는다.

---

## 5. Blocked — SetPosition

lifecycle, WPF durable recovery와 host factory receipt/readback tooling은 존재한다. runtime/native
exactly-once와 durable A/B backend는 fail-closed 상태다.

issue #44의 외부 prerequisite:

- vendor `CheckSum.CRC32` golden fixture
- LASAL IDE-generated `_FileSys` class/client ABI

이 두 항목 없이 CRC 의미를 추정하거나 generated ABI를 손으로 작성하지 않는다.

---

## 6. 후순위 backlog

| 영역 | current 상태 | 다음 구현 |
|---|---|---|
| PI Write | Dormant | capability/semantic allowlist review |
| Recorder Double | Dormant | D4 capability/route proof |
| Dynamic node/DI | Dormant | bits 15/16 activation qualification |
| Extended SDO result | Dormant | bit 12 qualification |
| Digital Output Write `0x7E23` | Missing runtime | LASAL route/owner/allowlist 구현 |

---

## 7. current 문서 우선순위

1. `DEVELOPMENT_STATUS_20260901.md` — 전체 current snapshot
2. `../API_DEVELOPMENT_PROGRESS.md` — 구현률/남은 작업/current qualification
3. `../API_MANUAL.md` — public/current API 사용 계약
4. `SET_OPERATION_MODE_DESIGN.md` — 완료된 SetOperationMode implementation contract
5. 기능별 historical evidence 문서

문서가 충돌하면 current `dev` source와 위 순서를 기준으로 정리한다.

---

## 8. Repository / qualification 원칙

- remote branch는 `main`, `dev`만 유지한다.
- `dev`가 유일한 integration/current qualification source truth다.
- source implementation 완료, PC test, C78 build, PLC load, physical effect, production release를 서로 다른 판정으로 기록한다.
- 기능 작업 branch가 필요하면 작업 -> 검증 -> `dev` merge -> 즉시 삭제한다.
- source SHA + generated artifact + PLC loaded image + WPF EXE/SDK identity를 같은 evidence set으로 남긴다.
- temporary workflow/helper는 검증 종료 후 제거한다.
'''
write(readme_path, readme)


# ---------------------------------------------------------------------------
# SetOperationMode design: update current-facing sections; keep historical
# checkpoints below as evidence.
# ---------------------------------------------------------------------------
design_path = "docs/api/design/SET_OPERATION_MODE_DESIGN.md"
design = read(design_path)
new_design_header = r'''# SetOperationMode 구현 설계

- 대상: No.33 `MMC_ChngOpMode`
- current baseline: `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`
- implementation status: **100% / COMPLETE**
- runtime status: **Active**
- supported modes: PP(1), PV(3), IP(7), CSP(8), mask `0x018A`
- commands: `0x7D23 Start`, `0x7D24 ReadOutcome`, `0x7D25 Retire`
- activation: `LMC_DIAG_SET_OPERATION_MODE_ENABLED=TRUE`, software modes TRUE, Admin capability triad ON
- safety: cross-mode Standstill / no Fault / no OperationEnabled, exact identity, durable no-replay
- completion commit: `0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)

이 문서는 SetOperationMode의 current implementation contract와 historical development checkpoints를
함께 보존한다. `8.1` 이후 날짜/PR 기반 checkpoint는 당시 상태를 기록한 historical evidence이며,
현재 지원/activation 판정은 위 metadata와 1~8절 current contract를 우선한다.

'''
design = replace_regex(design, r"^# SetOpMode 최우선 개발 설계.*?(?=## 1\. 정확한 API 의미)", new_design_header, "design header")
design = replace_once(
    design,
    "현재 C#에는 기존 `0x6061:0 Int8/1` read API와 별도로 SetOperationMode의 immutable\nprepare/start/query/retire SDK contract가 있다. LASAL에는 `0x7D23/0x7D24/0x7D25` route와\nhandler, `AxisOperationMode` owner, 전용 outcome state와 `0x6060/0x6061` SDO executor runtime이\n구현돼 있다. public activation은 아직 하지 않는다. capability bits 8/9/10과\n`LMC_DIAG_SET_OPERATION_MODE_ENABLED`는 C78/PLC/hardware 검증 완료 전까지 OFF로 유지한다.\n`0x6060/0x6061` PDO도 current Elmo object에서 disabled다.",
    "현재 C#에는 기존 `0x6061:0 Int8/1` read API와 별도로 SetOperationMode immutable\nprepare/start/query/retire SDK contract가 있다. LASAL에는 `0x7D23/0x7D24/0x7D25` route와\nhandler, `AxisOperationMode` owner, 전용 outcome state와 `0x6060/0x6061` SDO executor runtime이\n구현돼 있다. current `dev`는 capability bits 8/9/10과\n`LMC_DIAG_SET_OPERATION_MODE_ENABLED`를 활성화하고 supported mask `0x018A`를 광고한다.\nraw Generic SDO `0x6060` 우회 Write는 계속 permanent deny한다.",
    "design section1 activation",
)
section2 = r'''## 2. 지원 범위

current implementation은 physical axis 1..4와 Immediate lifecycle을 지원한다.

- requested mode PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex가 소유하므로 SetOperationMode에서 거부
- 이미 `0x6061`이 requested mode이면 `SucceededNoWrite` 가능
- cross-mode는 exact one-byte `0x6060:0=<requested>` Write를 최대 한 번만 dispatch
- Start 전에 Standstill=True, Fault=False, OperationEnabled=False를 요구
- exact `0x6061` readback과 owner/executor terminal evidence 후에만 success
- write dispatch 이후 uncertain outcome은 Start/Write를 replay하지 않고 read-only recovery만 사용
- terminal record는 exact generation으로 retire

PP/PV/IP/CSP는 동일한 ACK/outcome contract를 사용하며 TCP 계층은 requested mode를 exact echo/비교한다.
CSP(8) 상수에 고정된 ACK 분류는 current completion에서 제거됐다.

'''
design = replace_regex(design, r"## 2\. 1차 지원 범위.*?(?=## 3\. wire 설계)", section2, "design support section")
design = design.replace("warm-start/identity, MODE-08 preemption, activation-OFF,", "warm-start/identity, MODE-08 preemption, activation-gate handling,")
design = replace_once(
    design,
    "축의 current mode가 8이 아니거나 mode outcome이 unresolved이면 ordinary LMC motion을\n승인하지 않는 interlock은 activation 전에 실기 검증한다. PLC startup에서는 6061을\nread-only로 확인하고 자동으로 6060을 쓰지 않는다.",
    "mode outcome이 unresolved이면 durable recovery fence를 유지하고 original Start/`0x6060`을\n자동 replay하지 않는다. PLC startup/reconnect recovery도 `0x6061` 및 exact outcome을 read-only로\n확인하며 임의의 새로운 `0x6060` Write를 만들지 않는다.",
    "design unresolved fence",
)
section6 = r'''## 6. capability / activation

Admin capability triad:

- bit 8 `AxisSetOperationModeStart`
- bit 9 `AxisSetOperationModeOutcomeRead`
- bit 10 `AxisSetOperationModeOutcomeRetire`

세 bit는 indivisible하며 current `dev`에서 paired activation 상태다. supported-mode mask는 `0x018A`로
PP/PV/IP/CSP를 광고한다. SDK는 triad/current observation/Build/BootId/MapRevision을 검증하고 stale
capability로 Prepare하지 않는다.

compile-time/runtime OFF 경로는 fail-closed contract로 계속 소스에 남아 있으며, 이미 dispatch된
Running record의 read-only recovery/cleanup과 신규 mutation gate를 구분한다. current completion
baseline에서는 `LMC_DIAG_SET_OPERATION_MODE_ENABLED=TRUE`다.

'''
design = replace_regex(design, r"## 6\. capability.*?(?=## 7\. 변경 대상)", section6, "design capability section")
checklist = r'''## 8. 구현 체크리스트 — COMPLETE

- [x] `MODE-01` No.33 Immediate-only `MMC_ChngOpMode` 의미 고정
- [x] `MODE-02` command/capability/owner ABI freeze
- [x] `MODE-03` immutable prepare/start/query/retire model 및 sync/async API
- [x] `MODE-04` exact frame/parser/capability zero-wire contract
- [x] `MODE-05` `0x7D23/24/25` LASAL route/handler
- [x] `MODE-06` `6061 -> 6060 -> 6061` runtime state machine
- [x] `MODE-07` irreversible write 이후 no-replay/read-only recovery
- [x] `MODE-08` ownership conflict와 safety preemption
- [x] `MODE-09` Generic D5 raw `0x6060` permanent deny
- [x] `MODE-10` 32KiB method split 및 source/static contract
- [x] `MODE-11` PP/PV/IP/CSP exact requested-mode ACK/write/readback contract
- [x] `MODE-12` timeout/uncertainty/quarantine/terminal outcome/exact retire lifecycle
- [x] `MODE-13` WPF durable pre-dispatch journal, Running polling, no false PASS, no-replay recovery
- [x] `MODE-14` capability triad + supported mask paired activation

SetOperationMode feature implementation은 완료다. 아래 8.1 이후 checkpoint는 개발 과정의 historical
evidence로 보존한다. 전체 API production release와 repository-wide CI/artifact hygiene는 별도 gate다.

### 8.1 2026-08-20 PC/SDK checkpoint
'''
design = replace_regex(design, r"## 8\. 작업 체크리스트.*?### 8\.1 2026-08-20 PC/SDK checkpoint\n", checklist, "design checklist")
write(design_path, design)


# ---------------------------------------------------------------------------
# New current development snapshot.
# ---------------------------------------------------------------------------
snapshot_path = "docs/api/design/DEVELOPMENT_STATUS_20260901.md"
snapshot = r'''# 개발 상태 스냅샷 — 2026-09-01

- current integration / qualification source: `dev`
- current baseline: `0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0: **Generic SDO 잔여 구현/qualification**
- production release posture: **NO-GO**

이 문서는 SetOperationMode 완료 후의 current 개발 순서와 기능 상태를 고정한다. 구현 완료, CI,
C78/generated artifact, PLC load, physical effect와 production release를 서로 다른 판정으로 기록한다.

---

## 1. SetOperationMode 완료

current supported contract:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
Modes = PP(1), PV(3), IP(7), CSP(8)
Commands = 0x7D23 / 0x7D24 / 0x7D25
```

완료된 핵심:

- exact requested-mode ACK/domain-failure echo; CSP-fixed ACK 분류 제거
- fresh cross-mode drive preflight + FINAL Diagnostics capability refresh
- same-target no-write와 cross-mode write 구분
- exact requested mode `0x6060` one-shot dispatch
- original deadline 안의 `0x6061` read-only settling
- write-dispatched 이후 automatic Start/Write replay 금지
- terminal owner publish/release bounded retry without new SDO write
- exact terminal outcome + owner released/executor reusable evidence
- exact-generation retirement
- WPF durable pre-dispatch journal
- Running exact-key polling; premature PASS 금지
- Failed/Aborted terminal archive/retire 후 failure 반환
- indeterminate/query reject durable fence 유지
- stale recovery operator retirement에서 PLC completion proof를 조작하지 않음
- Generic SDO raw `0x6060` permanent deny

과거 17:28 capability freshness, Detail46/readback, owner publish reason6 및 CSP-fixed ACK 문제는
`SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md`에 historical investigation으로 남긴다.

### CI / repository hygiene

feature implementation 완료와 아래 CI 상태를 구분한다.

- C78 evidence tool run `33455821803`: SUCCESS
- static qualification run `33455821841`: 93 PASS / 1 FAIL
  - remaining fail: LASAL metadata Client order vs generated declaration order
- WPF recovery run `33455821887`: hosted runner MSBuild discovery 단계 실패; tests 미실행

이 항목들은 repository qualification hygiene이며 current SetOperationMode 기능 설계를 다시 열지 않는다.

---

## 2. P0 — Generic SDO

issue #46의 SetOperationMode 범위는 완료됐고 issue는 Generic SDO 잔여 범위만 추적한다.

현재 구현:

- Manual Server + tokenized programmatic dual-entry 기반
- physical axis1..4 generic scalar 1/2/4-byte Write source policy
- arbitrary WPF editor / exact request preview
- durable exact-request journal / no automatic replay
- ordinary safe-state gate

계속 차단하는 semantic/dedicated-owner object:

```text
0x6040 0x6060 0x607A 0x60FF 0x6071 0x3204 0x20FC
```

다음 완료 순서:

1. Axis1 safe non-semantic 1/2/4-byte exact Write/readback
2. Manual/programmatic 동시 접근 BUSY arbitration
3. timeout/disconnect/readback mismatch durable recovery
4. Axis2..4 matrix

---

## 3. P1 — HomeDS402

tracker: issue #32.

software/source/WPF qualification은 존재하지만 activation은 OFF다. 다음은 current exact `dev` tree에서
fresh C78/ARM Rebuild+Link, generated artifact identity review, full SourceOnly closure를 묶어 수행한다.
그 same-image로 PLC/hardware normal/fault/timeout matrix를 진행한 뒤 별도 activation review한다.

---

## 4. P1 — HomeDS402Ex

trackers: issue #28, issue #35.

먼저 axis1..4 wiring/polarity/homing method/scale/range를 evidence와 함께 승인한다. 그 profile과 같은
source tree에서 fresh C78/generated artifact/SourceOnly를 닫은 뒤 parameter SDO snapshot/program/restore와
physical homing runtime으로 진행한다. 그 전까지 capability bit11과 physical runtime은 OFF다.

---

## 5. Blocked — SetPosition

tracker: issue #44.

현재 lifecycle/WPF recovery/host factory tooling은 있으나 durable A/B runtime backend와 native
exactly-once execution은 fail-closed다. 다음 두 external prerequisite가 필요하다.

- real LASAL/vendor `CheckSum.CRC32` golden vectors
- LASAL IDE/CodeGenerator가 생성한 `_FileSys` class/client ABI

CRC 알고리즘이나 generated ABI를 추측해서 우회하지 않는다.

---

## 6. 후순위 기능

| 기능 | 상태 | next |
|---|---|---|
| PI Write | Dormant | capability/allowlist qualification |
| D4 Recorder Double | Dormant | route/capability proof |
| Dynamic node/DI | Dormant | bits15/16 qualification |
| Extended SDO result | Dormant | bit12 qualification |
| Digital Output Write `0x7E23` | Missing | LASAL handler/owner/allowlist |

---

## 7. 개발 순서

```text
SetOperationMode COMPLETE
        |
        v
Generic SDO completion (#46 remainder)
        |
        +--> HomeDS402 artifact/hardware (#32)
        |
        +--> HomeDS402Ex profile + artifact (#28, #35)
        |
        +--> SetPosition external prerequisites (#44)
        |
        v
Dormant/Missing API activation + distribution/release qualification
```

SetPosition의 외부 prerequisite가 준비되지 않아도 Generic SDO/HomeDS402/HomeDS402Ex의 독립 작업은 진행할
수 있다. 반대로 hardware/profile evidence가 필요한 값을 추측으로 채우지는 않는다.

---

## 8. production release

SetOperationMode feature implementation은 완료됐지만 전체 API는 아직 preview다. production release에는
남은 기능별 PC/source/C78/PLC/hardware gate와 distribution/manual sync가 동일 source/artifact set에서
닫혀야 한다. 따라서 project-level 판정은 계속 **NO-GO**다.
'''
if (ROOT / snapshot_path).exists():
    write(snapshot_path, snapshot)
else:
    write(snapshot_path, snapshot)


# ---------------------------------------------------------------------------
# Historical SetOperationMode investigation: append final closure marker.
# ---------------------------------------------------------------------------
history_path = "docs/api/design/SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md"
history = read(history_path)
if "## 11. 2026-09-01 SetOperationMode 구현 완료" not in history:
    history = history.rstrip() + r'''

## 11. 2026-09-01 SetOperationMode 구현 완료

current completion baseline은 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`
(`dev : SetOpMode Complete`)이다. 위 1~10절은 문제를 좁혀 간 historical investigation으로 보존하고,
current 판정은 다음으로 고정한다.

- PP/PV/IP/CSP exact requested-mode ACK/response contract 적용
- CSP(8) hardcoded ACK 판정 제거
- exact requested-mode `0x6060` one-shot + `0x6061` read-only settling
- irreversible dispatch 이후 no replay
- terminal owner publish/release bounded retry
- durable exact outcome/generation retirement
- WPF Running polling / premature PASS 방지 / indeterminate fence
- supported mask `0x018A`, capability triad와 runtime gate active

따라서 SetOperationMode **feature implementation은 완료**다. 이후 이 기능에서 발견되는 CI runner,
metadata/generated artifact 정합성은 repository qualification hygiene로 분리한다. 전체 API production
release 여부는 Generic SDO, HomeDS402, HomeDS402Ex, SetPosition 등 나머지 gate와 함께 별도 판정한다.
''' + "\n"
write(history_path, history)


# ---------------------------------------------------------------------------
# Basic current-marker checks.
# ---------------------------------------------------------------------------
checks = {
    "docs/api/API_MANUAL.md": ["2.6-development", "SetOperationMode 기능 구현 완료와 전체"],
    "docs/api/API_DEVELOPMENT_PROGRESS.md": ["1.4-current", "## 6. SetOperationMode 완료 checkpoint", "Generic SDO 완료 — issue #46"],
    "docs/api/design/README.md": ["SetOperationMode: **IMPLEMENTATION COMPLETE / Active**", "DEVELOPMENT_STATUS_20260901.md"],
    "docs/api/design/SET_OPERATION_MODE_DESIGN.md": ["implementation status: **100% / COMPLETE**", "MODE-14` capability triad + supported mask paired activation"],
    "docs/api/design/DEVELOPMENT_STATUS_20260901.md": ["SetOperationMode COMPLETE", "issue #44"],
}
for rel, markers in checks.items():
    text = read(rel)
    for marker in markers:
        if marker not in text:
            raise SystemExit(f"{rel}: missing marker {marker!r}")

print("Post-SetOperationMode documentation sync applied successfully.")

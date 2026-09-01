# 최우선 API 개발 설계

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

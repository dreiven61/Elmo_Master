# 최우선 API 개발 설계

- 기준일: 2026-09-01
- current integration / qualification source: `dev`
- current baseline: `dev@eeebda2b36a52a442f4919cbe70011536103b7be` (`dev: finalize generic SDO Write session qualification`)
- current status snapshot: `DEVELOPMENT_STATUS_20260901.md`
- **remaining implementation master: `REMAINING_IMPLEMENTATION_DESIGN_20260901.md`**
- **current P0 detailed design: `SDO_WRITE_DETAILED_DESIGN_20260901.md`**
- **current P0 manual-write override: `SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`**
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**
- current P0 implementation: **Generic SDO Write direct-manual enablement + physical qualification**
- production release posture: **NO-GO**

이 폴더의 current 판정은 `dev` source와 최신 current snapshot을 우선한다. SetOperationMode 이후
남은 기능의 구현 순서, dependency, source delta와 activation gate는
`REMAINING_IMPLEMENTATION_DESIGN_20260901.md`를 master로 사용한다. Generic SDO Write의 전체
1/2/4-byte/journal/readback 구조는 `SDO_WRITE_DETAILED_DESIGN_20260901.md`를 사용하되,
**ordinary/manual Write의 Same-Value Qualification 선행 의무와 ObjectIndex denylist에 대해서는
`SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`가 우선한다.**

즉 current 요구사항은 다음과 같다.

> Same-Value Qualification은 선택적 engineering diagnostic이며 ordinary Generic SDO Write의
> admission gate가 아니다. Generic SDO Write는 ObjectIndex `0x0000`만 invalid이고, 나머지
> valid ObjectIndex는 address 자체로 차단하지 않는다. current request마다 capability/session,
> safe-axis, two-click confirmation, baseline/pre-write guard, durable no-replay journal과 exact
> readback을 통과해 실행한다.

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

`SetOperationMode`는 operation-mode 변경에 대한 higher-level semantic lifecycle API다. 그러나 current
Generic SDO low-level surface는 `0x6060`을 포함한 valid nonzero ObjectIndex를 address만으로 영구
차단하지 않는다. raw Generic SDO를 통해 semantic owner API를 우회할 수 있다는 점은 engineering
surface의 책임 경계로 문서화하고, raw address 자체를 denylist로 막지 않는다.

상세 구현/원인 추적:

- `SET_OPERATION_MODE_DESIGN.md` — current implementation contract
- `SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md` — capability/readback/owner/ACK historical investigation
- `SET_OPERATION_MODE_START_EXECUTION_IMPLEMENTATION_RESULT_20260831.md` — Start execution corrective
- `SET_OPERATION_MODE_DETAIL49_OBSERVABILITY_IMPLEMENTATION_RESULT_20260831.md` — admission/storage observability

SetOperationMode 구현 완료를 전체 API production 승인으로 확대 해석하지 않는다.

---

## 2. P0 — Generic SDO Write

issue #46은 SetOperationMode 부분을 완료 처리하고 Generic SDO 잔여 범위만 추적한다.
전체 상세 구현 정본은 `SDO_WRITE_DETAILED_DESIGN_20260901.md`, ordinary/manual admission의
current override는 `SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`다.

current source:

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
```

이미 통합된 범위:

- SDO-R02 Manual Server + tokenized programmatic dual-entry 기반
- SDO-R03 physical axis 1..4 generic 1/2/4-byte scalar Write policy
- SDO-R04 arbitrary WPF editor / exact preview
- SDO-R05 durable exact-request no-replay recovery
- ordinary Write safe-state correction
- UI24 preset authorization과 generic policy 분리
- baseline/pre-write guard + journal evidence + identity-pinned one-shot submit
- SDK/WPF/LASAL ObjectIndex denylist 제거: `ObjectIndex == 0`만 invalid

ordinary Generic SDO Write gate에서 **제거할 항목**:

```text
Same-Value Qualification current-session proof mandatory admission
Run Same-Value Qualification First button lock
HasCurrentSdoWriteActivationQualificationProof(...) runtime hard gate
```

ordinary Generic SDO Write에서 **유지할 항목**:

- connected/current session
- SDO Read/Write capability와 fresh BootId/MapRevision
- canonical 1/2/4-byte scalar shape
- `ObjectIndex != 0`
- safe-axis preflight
- first-click exact baseline + immutable Arm, Write 0회
- second-click exact confirmation
- final pre-write guard와 baseline equality
- durable mutation journal / no automatic replay
- one-shot Write
- terminal polling
- mandatory exact readback 후에만 VERIFIED

Same-Value UI24 four-ticket path는 optional transport canary/diagnostic이다. manual editor unlock key가 아니다.

current detailed implementation/qualification order:

1. `SWR-01` UI24 preset authorization과 generic policy 분리 — **SOURCE/PC COMPLETE (2026-09-01)**
2. `SWR-02` image/session transport proof — **SOFTWARE COMPLETE, diagnostic evidence only**
3. `SWR-03` baseline -> confirmation -> pre-write guard -> journal v4 -> one-shot Write -> exact readback — **SOFTWARE COMPLETE**
4. `SWR-04` shared executor/no-replay source contract — **SOFTWARE COMPLETE; physical regression pending**
5. `DMW-01` proof-based UI button lock 제거 — **PENDING**
6. `DMW-02` ordinary handler의 qualification proof mandatory checks 제거 — **PENDING**
7. `DMW-03` UI/title/localization을 optional qualification 의미로 정리 — **PENDING**
8. `DMW-04` no-proof direct Write WPF regression + distribution mirror — **PENDING**
9. `SWR-05/06` Axis1 1/2/4-byte physical/failure matrix — pending user qualification
10. `SWR-07` Axis2..4 physical matrix — pending user qualification
11. `SWR-08` issue/distribution/release sync — pending physical evidence

SWR-01 결과는 `SDO_WRITE_SWR01_IMPLEMENTATION_RESULT_20260901.md`, current software 전체 결과와
검증 경계는 `SDO_WRITE_SOFTWARE_IMPLEMENTATION_RESULT_20260901.md`에 기록한다. DMW-01~04 구현은
`SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`의 완료조건과 회귀시험을 따른다.

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

두 prerequisite가 닫힌 뒤 `REMAINING_IMPLEMENTATION_DESIGN_20260901.md`의 HOMEEX-R tranche에 따라
parameter SDO snapshot/program/restore, RT execution mailbox, physical homing observer와 cleanup proof를
구현한다. 그 전에는 hardware-dependent 값을 추측하거나 physical homing path를 열지 않는다.

---

## 5. Blocked — SetPosition

lifecycle, WPF durable recovery와 host factory receipt/readback tooling은 존재한다. runtime/native
exactly-once와 durable A/B backend는 fail-closed 상태다.

issue #44의 외부 prerequisite:

- vendor `CheckSum.CRC32` golden fixture
- LASAL IDE-generated `_FileSys` class/client ABI

이 두 항목 없이 CRC 의미를 추정하거나 generated ABI를 손으로 작성하지 않는다. prerequisite가
준비되면 master plan의 SP-01B -> SP-02 -> SP-03 -> SP-04 -> hardware -> activation 순서로 진행한다.

---

## 6. 후순위 backlog

| 영역 | current 상태 | 다음 구현 |
|---|---|---|
| EtherCAT NodeHealth / DigitalIO Read | Dormant | read-owner qualification 후 bits 15/16 paired activation |
| Digital Output Write `0x7E23` | Missing runtime | LASAL route/ticket/RT CAS mailbox/allowlist 구현 |
| PI Write | Dormant | approved writable semantic catalog + owner/readback |
| Recorder Double | Dormant | two-bank identity/RT jitter/fault matrix + bit 6 |
| Extended SDO result | Dormant | requirement 확인 후 chunk producer/bit 12 qualification |
| ApplicationPhase/WKC | Deferred | requirement coverage 재확인 후 read-only tranche 여부 결정 |

---

## 7. current 문서 우선순위

1. `SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md` — **ordinary Generic SDO Write admission current override**
2. `REMAINING_IMPLEMENTATION_DESIGN_20260901.md` — SetOperationMode 이후 남은 기능 구현 master plan
3. `SDO_WRITE_DETAILED_DESIGN_20260901.md` — Generic SDO Write 전체 구조/physical qualification 상세 설계
4. `SDO_WRITE_SOFTWARE_IMPLEMENTATION_RESULT_20260901.md` — SWR-01~04 software 결과와 physical 경계
5. `DEVELOPMENT_STATUS_20260901.md` — 전체 current status snapshot
6. `../API_DEVELOPMENT_PROGRESS.md` — 구현률/남은 작업/current qualification
7. `../API_MANUAL.md` — public/current API 사용 계약
8. 기능별 `*_DESIGN.md` — frozen wire/state machine/세부 계약
9. 기능별 historical evidence 문서

문서가 충돌하면 current `dev` source와 위 순서를 기준으로 정리한다. 특히 SDO Write에 대해서는
`SDO_WRITE_DETAILED_DESIGN_20260901.md`의 proof-mandatory/semantic denylist 문구보다
`SDO_WRITE_DIRECT_MANUAL_ENABLEMENT_20260901.md`를 우선한다.

---

## 8. Repository / qualification 원칙

- remote branch는 `main`, `dev`만 유지한다.
- `dev`가 유일한 integration/current qualification source truth다.
- source implementation 완료, PC test, C78 build, PLC load, physical effect, production release를 서로 다른 판정으로 기록한다.
- 기능 작업 branch가 필요하면 작업 -> 검증 -> `dev` merge -> 즉시 삭제한다.
- source SHA + generated artifact + PLC loaded image + WPF EXE/SDK identity를 같은 evidence set으로 남긴다.
- temporary workflow/helper는 검증 종료 후 제거한다.

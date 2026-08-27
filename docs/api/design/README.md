# 최우선 API 개발 설계

- 기준일: 2026-08-27
- current production baseline: `dev@cd89d189a3dd574c1fc1147eba07dff88effc54a`
- production posture: **NO-GO**
- active P0 redesign: issue #46 / PR #47

이 폴더는 current `dev`의 release boundary와 branch-local qualification을 분리해서 관리한다. 최신 통합 요약은 `DEVELOPMENT_STATUS_20260827.md`를 따른다.

2026-08-27 Generic SDO / SetOperationMode 재설계 문서:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`
- `evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md`

---

## 1. 최우선 API

| 순서 | API | release-oriented 진행도 | current production 상태 | 설계 |
|---:|---|---:|---|---|
| 1 | HomeDS402 | 50% | dormant / bit6 + five-gate OFF | `HOME_DS402_DESIGN.md` |
| 2 | SetOperationMode | 60% | dormant / bits8-10 OFF | `SET_OPERATION_MODE_DESIGN.md` + SDO/Mode redesign |
| 3 | HomeDS402Ex | 40% | dormant / physical runtime + bit11 OFF | `HOME_DS402_EX_DESIGN.md` |
| 4 | SetPosition | 25% | dormant | `SET_POSITION_DESIGN.md` |

진행도는 checklist 비율이 아니라 release-oriented 수치다. Branch-local source/C78 구현이 있어도 current `dev`에 통합되고 해당 evidence grade가 닫히기 전에는 진행도를 자동 상승시키지 않는다.

---

## 2. 2026-08-27 P0 사용자 요구 audit

세 요청의 현재 상태:

1. **SetOperationMode PP/PV/IP/CSP actual transition: 부분 구현 / 미완료**
   - qualification branch에 multi-mode request/UI 존재
   - 이전 bench definitive reject
   - PLC `SupportedModeMask` 미구현
   - actual multi-mode physical PASS 없음

2. **Generic arbitrary-target SDO Write: 미구현**
   - request model은 arbitrary address를 표현 가능
   - actual submission policy는 여전히 Axis1 `0x2F00:24` exact allowlist
   - arbitrary-target WPF editor 없음

3. **LMCSdoExecutor manual Server Read/Write: source 구현 완료 / bench 미완료**
   - dual-entry source 구현
   - C78 Rebuild/Link 0 errors
   - PLC download/basic smoke 확인
   - manual physical Read/Write/arbitration/D5 regression 미완료

세 항목의 상세 완료 조건은 `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`가 정본이다.

---

## 3. command lifecycle current 상태

| API | Start | ReadOutcome | Retire | current 상태 |
|---|---:|---:|---:|---|
| HomeDS402 | `0x7D15` | `0x7D16` | `0x7D17` | source/PC/WPF H37 qualification, activation OFF |
| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | route/owner/retained store, physical runtime OFF |
| SetOperationMode | `0x7D23` | `0x7D24` | `0x7D25` | lifecycle/no-replay 존재, production activation OFF |
| SetPosition | `0x7D12` | `0x7D14` | `0x7D1A` | lifecycle + WPF durable recovery, native runtime activation OFF |

Wire byte offset 정본은 `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`다.

---

## 4. current P0 개발 큐

### Wave A — PR #47 / SDO-R02 close

- diff hygiene / focused CI green
- Axis1..4 executor direct-open
- Class View manual `0x6061:0` Read
- safe manual Write + exact readback
- manual/programmatic BUSY/no-wire arbitration
- D5 programmatic regression
- same-image generated artifact identity

### Wave B — Generic SDO

- SDO-R03 exact target allowlist 제거 및 capability/owner policy
- SDO-R04 WPF arbitrary-target editor
- SDO-R05 exact-request durable no-replay recovery

### Wave C — SetOperationMode multi-mode

- MODE-R01 PLC `SupportedModeMask`
- typed SDK capability
- WPF selector = SDK-known ∩ PLC-supported
- detailed rejection/current mode diagnostics
- MODE-R02 Axis1 PP/PV/IP/CSP matrix
- Axis2..4 확대

### Wave D — release

- REL-R01 generated/C78/distribution/docs sync
- production activation 별도 review

---

## 5. current feature boundaries

### HomeDS402

Current software qualification:

- H37 five-gate static contract
- lifecycle PC tests
- ownership/preemption
- method-size
- WPF durable no-replay

Remaining:

- fresh artifact/C78 H37-05/06
- Axis1 H37-07
- Axis2..4 H37-08
- activation H37-09

### SetOperationMode

Current `dev`:

- OwnerKind 6 / Diagnostics SDO resource 4
- `6061 -> 6060 -> 6061`
- same-mode zero-write
- irreversible dispatch no-replay
- safety preemption
- MODE-10 split/static
- MODE-13 WPF durable recovery

2026-08-27 redesign adds a new requirement: lifecycle capability와 actual mode support를 분리한다. Multi-mode completion은 `SupportedModeMask` + physical matrix가 닫혀야 한다.

### HomeDS402Ex

Current software:

- SDK wire/lifecycle
- retained full outcome
- full owner identity
- WPF durable recovery
- approved-plan preparation
- source/static/C78 collector

Remaining external gates:

- issue #28 actual axis profile
- issue #35 fresh C78/artifact
- physical homing runtime/matrix
- bit11 activation

### SetPosition

Current:

- P1 lifecycle
- WPF SP-04A durable no-replay recovery

Remaining:

- durable A/B store
- vendor CRC / IDE-generated `_FileSys` prerequisites
- RT exactly-once/native execution
- C78/PLC/hardware
- activation

---

## 6. 공통 안전 계약

- mutation은 동일 resource/axis에서 단일 owner만 가진다.
- wire dispatch 가능 경계를 넘은 뒤 original mutation을 자동 replay하지 않는다.
- Start ACK는 terminal completion이 아니다.
- exact endpoint/Build/BootId/MapRevision/request/intent identity를 유지한다.
- timeout/disconnect/corrupt/owner drift를 success로 축소하지 않는다.
- generic raw access가 semantic owner lifecycle을 우회하지 않는다.
- capability activation은 final approved image/hardware evidence와 paired한다.

---

## 7. Definition of Done

API/기능을 Active/완료로 표시하려면 최소 다음을 구분해서 닫는다.

1. public/model/wire contract
2. malformed/golden tests
3. LASAL parser/runtime
4. ownership/arbitration
5. durable no-replay
6. source/static/method-size
7. IDE-generated artifact identity
8. fresh C78 build/link
9. exact PLC load identity
10. hardware/packet normal + negative matrix
11. distribution/docs sync
12. paired capability/activation

한 단계의 PASS를 다음 단계의 PASS로 추정하지 않는다.

---

## 8. branch 규칙

- 기능 current 판정은 `dev` 기준이다.
- PR #47은 P0 redesign/implementation qualification branch이며 merge 전 전체 regression을 다시 확인한다.
- PR #18은 과거 MODE-11 qualification history로 유지하며 새 redesign의 completion evidence로 사용하지 않는다.
- generated artifact hash는 fresh same-source evidence 없이 자동 ratchet하지 않는다.
- user-reported PLC smoke는 해당 smoke만 의미하며 packet/feature matrix PASS로 확대하지 않는다.

# 최우선 API 개발 설계

- 기준일: 2026-08-28
- current production baseline: `dev@45fc528c48723cbe3ba20fb11c3a2d7ec0e7ef0b`
- production posture: **NO-GO**
- active P0 tracking: issue #46

이 폴더는 current `dev`의 release boundary와 branch-local qualification을 분리해서 관리한다. SetOperationMode의 current-state 정본은 `SET_OPERATION_MODE_DESIGN.md`와 `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md`를 함께 따른다.

2026-08-27 Generic SDO / SetOperationMode 재설계 문서:

- `../../architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`
- `evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md`

---

## 1. 최우선 API

| 순서 | API | release-oriented 진행도 | current production 상태 | 설계 |
|---:|---|---:|---|---|
| 1 | HomeDS402 | 50% | dormant / bit6 + five-gate OFF | `HOME_DS402_DESIGN.md` |
| 2 | SetOperationMode | **65%** | dormant / bits8-10 OFF / production mode mask 0 | `SET_OPERATION_MODE_DESIGN.md` + `SET_OPERATION_MODE_IMPLEMENTATION_UPDATE_20260828.md` |
| 3 | HomeDS402Ex | 40% | dormant / physical runtime + bit11 OFF | `HOME_DS402_EX_DESIGN.md` |
| 4 | SetPosition | 25% | dormant | `SET_POSITION_DESIGN.md` |

진행도는 checklist 비율이 아니라 release-oriented 수치다. Branch-local source/C78 구현이 있어도 current `dev`에 통합되고 해당 evidence grade가 닫히기 전에는 진행도를 자동 상승시키지 않는다.

---

## 2. P0 사용자 요구 audit — 2026-08-28

### 2.1 SetOperationMode PP/PV/IP/CSP

현재 판정: **software/source 약 80%, release-oriented 약 65%, physical multi-mode 미완료**

완료된 것:

- PP(1)/PV(3)/IP(7)/CSP(8) request path
- requested mode exact `0x6060` 1-byte mutation + `0x6061` exact verify source path
- same-mode zero-write
- cross-mode safe-state preflight
- PLC `SetOperationModeSupportedMask` wire/SDK/WPF contract
- WPF selector와 live PLC mask의 fail-closed Start admission
- MODE-11E PP/PV/IP/CSP warm-start recovery 일반화
- write-dispatch 이후 original Start/`0x6060` no-replay
- MODE-11F requested/preflight/observed, DetailCode, DS402/evidence/write-dispatch diagnostics

남은 것:

- current `dev@45fc528c...`에서 qualification branch 재생성
- fresh current-image C78/ARM Rebuild+Link + generated artifact identity + PLC load
- Axis1 CSP same-mode formal packet evidence
- Axis1 `CSP -> PP/PV/IP -> CSP` physical packet/readback matrix
- timeout/disconnect/mismatch/quarantine/retire matrix
- mode별 qualification evidence ↔ production mask coupling
- Axis2..4 확대
- MODE-14 production paired activation

기존 qualification branch `codex/setopmode-mode11-bench-activation@eae31dd...`는 current `dev`와 diverged하므로 다음 bench 기준으로 사용하지 않는다.

### 2.2 Generic arbitrary-target SDO Write

현재 판정은 별도 `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md`를 따른다. SetOperationMode semantic owner와 generic raw Write policy는 계속 분리한다. `0x6060`은 semantic SetOperationMode lifecycle을 우회하는 일반 raw path로 허용하지 않는다.

### 2.3 LMCSdoExecutor manual Server Read/Write

`LMCSdoExecutor` dual-entry source는 `dev`에 통합돼 있다. manual Server entry와 tokenized programmatic entry는 shared arbitration을 사용하며, 실제 bench evidence는 generic SDO qualification 문서에서 별도로 추적한다.

---

## 3. command lifecycle current 상태

| API | Start | ReadOutcome | Retire | current 상태 |
|---|---:|---:|---:|---|
| HomeDS402 | `0x7D15` | `0x7D16` | `0x7D17` | source/PC/WPF H37 qualification, activation OFF |
| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | route/owner/retained store, physical runtime OFF |
| SetOperationMode | `0x7D23` | `0x7D24` | `0x7D25` | multi-mode source + MODE-11E/11F, production activation OFF |
| SetPosition | `0x7D12` | `0x7D14` | `0x7D1A` | lifecycle + WPF durable recovery, native runtime activation OFF |

Wire byte offset 정본은 `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`다.

---

## 4. current P0 개발 큐

### Wave A — SetOperationMode test baseline

- current `dev@45fc528c...`에서 qualification branch 재생성
- activation delta-only diff 검증
- LASAL IDE generated source / `Classes.lcb` 재생성
- fresh C78/ARM Rebuild+Link
- exact image identity + PLC load
- WPF same-tree build

### Wave B — SetOperationMode Axis1 physical matrix

- MODE-11A CSP same-mode zero-write
- MODE-11B `CSP -> PP -> CSP`
- MODE-11C `CSP -> PV -> CSP`
- MODE-11D `CSP -> IP -> CSP`
- exact-one-write / duplicate-zero / `0x6061` exact readback

### Wave C — SetOperationMode failure/release evidence

- MODE-12 timeout/disconnect/mismatch/quarantine/retire
- MODE-11G mode별 evidence ledger + qualification/release mask coupling
- Axis2..4 확대
- MODE-14 paired production activation review

### Wave D — Generic SDO

- SDO-R03 generic Write policy
- SDO-R04 WPF arbitrary-target editor
- SDO-R05 exact-request durable no-replay recovery

### Wave E — release sync

- generated/C78/distribution/docs sync
- production activation은 별도 review

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
- PP/PV/IP/CSP requested-mode handling
- `6061 -> 6060 -> 6061`
- same-mode zero-write
- cross-mode PhysicalValid/Standstill/Fault/OperationEnabled/conflict preflight
- irreversible dispatch no-replay
- safety preemption
- `SupportedModeMask` wire/SDK/WPF
- MODE-11E multi-mode warm-start recovery
- MODE-11F WPF preflight/rejection/write-evidence diagnostics
- MODE-13 WPF durable recovery

Current release boundary:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- Admin bits 8/9/10 OFF
- production `SetOperationModeSupportedMask = 0`
- qualification mask는 test enable일 뿐 physical PASS 증거가 아님
- PP/PV/IP physical matrix와 current exact C78/PLC evidence가 닫히기 전 production support로 기록하지 않음

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

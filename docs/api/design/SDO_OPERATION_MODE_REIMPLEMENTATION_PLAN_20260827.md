# Generic SDO / SetOperationMode 재구현 및 qualification 계획

- 최초 기준일: 2026-08-27
- current revision: 2026-08-31
- current integration / qualification source: `dev`
- tracking: issue #46
- production release posture: **NO-GO**

이 문서는 Generic SDO / SetOperationMode의 current source truth와 남은 physical/release gate를 관리한다.

---

## 0. current executive status

| ID | 목표 | current 상태 | 완료 판정 |
|---|---|---|---|
| P0-1 | SetOperationMode PP/PV/IP/CSP 실제 전환 | software lifecycle은 구현됨. Axis1 CSP->PP는 host preflight/Prepare/journal까지 PASS하지만 PLC Start가 Detail 49로 reject | **software path 구현 / PLC Start admission blocker OPEN** |
| P0-2 | Generic SDO arbitrary safe object Write | generic 1/2/4-byte scalar policy + editor + durable recovery + safe-state correction 통합 | **software 구현 완료 / physical 미완료** |
| P0-3 | `LMCSdoExecutor` manual/programmatic 공존 | dual-entry arbitration source 통합 | **source 구현 완료 / detailed bench 미완료** |

SetOperationMode의 current blocker는 더 이상 host capability freshness ordering이 아니다. 2026-08-31 physical retest에서 BootId `0x66`, `0x67`, `0x68` 모두 PLC Start가 `SetOperationModeOutcomeStorageUnavailable(49)`로 reject됐다.

---

## 1. tranche status

| ID | 내용 | current 상태 | 다음 gate |
|---|---|---|---|
| SDO-R01 | regression fixture | 완료 | physical regression 재사용 |
| SDO-R02 | `LMCSdoExecutor` dual-entry | source 구현 완료 | manual/programmatic physical contention |
| SDO-R03 | Generic Write policy 일반화 | 완료 | safe-object physical Write/readback |
| SDO-R04 | WPF arbitrary-target editor/preview | 완료 | physical operator path |
| SDO-R05 | Generic Write durable recovery | 완료 | timeout/disconnect physical recovery |
| SDO-R06 | ordinary Write safe-state correction | 완료 | current-image physical retest |
| MODE-R01 | supported mode capability + diagnostics | 완료 | live mask/identity 유지 |
| MODE-R02 | PP/PV/IP/CSP software execution | 완료 | PLC Start admission blocker 해소 후 physical matrix |
| MODE-R03 | cross-mode fresh preflight/final diagnostics refresh | 완료 | Axis1 CSP->PP host path PASS |
| MODE-R04 | Detail 49 physical blocker analysis | **software observability implemented** | fresh PLC image에서 Detail 63/64 discriminator 확인 |
| REL-R01 | release/distribution | 미완료 | physical matrix 후 release review |

---

## 2. current activation truth

### SetOperationMode source expectation

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin FeatureMask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

지원 mode:

- PP = 1
- PV = 3
- IP = 7
- CSP = 8

Homing(6)은 별도 semantic owner가 담당한다.

### Generic SDO

```text
LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE
```

qualification activation ON은 production release 승인과 동의어가 아니다.

---

## 3. Generic SDO contract

physical axis 1..4 generic scalar shape:

- 8-bit: 1 byte
- 16-bit: 2 bytes
- 32-bit: 4 bytes

request ValueType/wire width가 canonical하게 일치해야 한다.

semantic/dedicated-owner raw blocklist는 계속 fail-closed다.

- `0x6040`
- `0x6060`
- `0x607A`
- `0x60FF`
- `0x6071`
- `0x3204`
- `0x20FC`

ordinary Generic SDO Write safe-state:

```text
Standstill=True
Fault=False
OperationEnabled=False
```

PLC 허용 DS402 base state:

- `0x40`
- `0x21`
- `0x23`

`0x27` Operation Enabled 및 unsafe/fault state는 거부한다.

---

## 4. SetOperationMode semantic lifecycle

normal sequence:

```text
fresh 0x6061 / axis / 0x6041 preflight
-> final Diagnostics identity refresh
-> Prepare one-shot request
-> durable journal ArmBeforeDispatch
-> Start exactly once
-> PLC ownership/outcome admission
-> exact one-byte 0x6060 Write(requested mode)
-> 0x6061 verify
-> terminal outcome
-> exact-generation retire
```

same target는 zero-write `SucceededNoWrite`가 가능하다.

cross-mode는 다음을 요구한다.

- Standstill=True
- DS402 Fault=False
- DS402 OperationEnabled=False

post-write uncertainty에서는 Start/`0x6060` Write를 replay하지 않는다.

---

## 5. 2026-08-31 physical finding

latest evidence:

```text
WPF BuildUtc=2026-08-31 02:33:22 UTC
SDK BuildUtc=2026-08-31 02:33:19 UTC
Axis=1
currentMode=8
requestedMode=1
StatusWord=0x02D0
Build=1
BootId=0x00000068
MapRevision=0x957F101E
Prepare PASS
journal arm PASS
PLC Start reject
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

BootId history:

```text
0x66 -> Detail 49
0x67 -> Detail 49
0x68 -> Detail 49
```

아직 `0x6060` Write는 도달하지 못했다.

---

## 6. owner-channel correction 재분류

commit `c670bd6fbc816116eacbe19b94199479d1a8cacf`:

- embedded LASAL client metadata order 정렬;
- disconnected `AxisOwnership`을 Detail 52로 분리;
- SDK/error catalog/static verifier 동기화.

이 변경은 source consistency correction으로 유지하지만, BootId `0x68`에서도 Detail 49가 재현됐으므로 physical root-cause fix로 간주하지 않는다.

current corrected Start admission semantics:

```text
zero CallerSessionEpoch / RequestSequence /
AdmissionToken / OwnerGeneration -> 49

AxisOwnership disconnected -> 52

ownership identity validate/commit failure -> 42

runtime feature gate OFF -> 49
```

따라서 corrected image가 실행된다는 전제에서는 현재 49를 AxisOwnership disconnected로 해석하지 않는다.

---

## 7. MODE-R04 — Detail 49 observability redesign

### 목적

현재 49는 적어도 다음 두 원인을 구분하지 못한다.

1. runtime SetOperationMode feature disabled;
2. ownership admission identity tuple zero.

원인 discriminator 없이 safety/ownership functional path를 계속 수정하지 않는다.

### proposed detail split

63/64 detail split은 current `dev`에 구현됐다.

```text
49 SetOperationModeOutcomeStorageUnavailable
   실제 outcome/storage infrastructure unavailable 전용

52 SetOperationModeOwnershipChannelUnavailable
   기존 구현 유지

63 SetOperationModeAdmissionIdentityUnavailable
   session/sequence/token/generation 중 하나가 zero

64 SetOperationModeFeatureDisabled
   runtime feature activation OFF
```

### required evidence

current implementation은 다음 zero/nonzero discriminator를 separate producer detail로 분리한다.

- FeatureEnabled
- CallerSessionEpochNonZero
- RequestSequenceNonZero
- AdmissionTokenNonZero
- OwnerGenerationNonZero
- AxisOwnershipConnected
- OwnershipIdentityValidated
- OwnershipCommitted

정상 token raw value를 운영 로그에 노출할 필요는 없다.

### decision gate

- FeatureDisabled -> generated/runtime activation path 수정
- AdmissionIdentityUnavailable -> TCP reserve -> Diagnostics forwarding contract 수정
- Detail 52 -> LASAL network/channel binding 수정
- Detail 42 -> ownership identity/state mismatch 분석
- Start accepted -> physical mode SDO lifecycle qualification 진행

---

## 8. safety contract — 변경 금지

MODE-R04 observability 작업에서도 다음은 유지한다.

- `requireCurrentObservation=true`
- Build/BootId/MapRevision identity fence
- Standstill/Fault/OperationEnabled cross-mode fence
- one-shot confirmation
- durable pre-dispatch journal
- no-replay invariant
- ownership validation/commit
- raw Generic SDO `0x6060` block

admission token/generation을 임의 생성하거나 ownership을 우회하지 않는다.

---

## 9. physical qualification plan

### 9.1 SetOperationMode blocker close criteria

먼저 Axis1 CSP->PP에서 PLC Start가 accepted되어야 한다.

그 전까지 PP/PV/IP physical matrix는 BLOCKED로 취급한다.

### 9.2 Axis1 normal matrix

| current | requested | expected mutation |
|---|---|---|
| CSP | CSP | zero-write / `SucceededNoWrite` |
| CSP | PP | exact one `0x6060=1` |
| CSP | PV | exact one `0x6060=3` |
| CSP | IP | exact one `0x6060=7` |
| PP/PV/IP | CSP | exact one `0x6060=8` |
| target X | same X | zero-write |

각 cross-mode row는 `0x6061` exact readback까지 확인한다.

### 9.3 Generic SDO matrix

safe non-semantic object를 선정해:

- 1-byte Write + exact readback
- 2-byte Write + exact readback
- 4-byte Write + exact readback
- manual/programmatic contention
- timeout/disconnect/readback mismatch recovery

### 9.4 확대

Axis1 정상/실패 matrix가 닫힌 뒤 Axis2..4로 확대한다.

---

## 10. current work order

1. **DONE** — host preflight/final Diagnostics ordering correction
2. **DONE** — BootId `0x66/0x67/0x68` physical Detail 49 reproduction 기록
3. **DONE** — owner-channel correction을 physical fix에서 source-consistency correction으로 재분류
4. **NOW / DESIGN ONLY** — MODE-R04 Detail 49 observability redesign 문서화
5. 다음 implementation에서 feature-disabled / admission-identity-zero를 별도 detail로 분리
6. 원인 확정 후 해당 path만 수정
7. Start accepted 후 Axis1 PP/PV/IP/CSP physical matrix
8. Generic SDO hardware matrix
9. recovery/failure matrix
10. Axis2..4 확대
11. production release review

이번 단계에서는 사용자 요청에 따라 functional source를 추가 수정하지 않는다.

---

## 11. release boundary

physical qualification이 모두 PASS하더라도 qualification-active 값을 그대로 production release로 간주하지 않는다.

release review에서 별도로 결정한다.

- SetOperationMode activation 유지 여부
- Generic SDO global Write activation 범위
- per-axis qualification 범위
- generated artifact identity
- distribution/documentation
- rollback/deactivation strategy

production release 전까지 판정은 **NO-GO**다.

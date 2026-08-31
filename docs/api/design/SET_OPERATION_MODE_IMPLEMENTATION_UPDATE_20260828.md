# SetOperationMode 구현 설계 보완 — 2026-08-28 / 2026-08-31 update

> 상태: current implementation / physical-finding addendum
>
> integration / qualification source: `dev`
>
> physical release posture: **NO-GO**
>
> 추적: issue #46
>
> 기존 historical design은 보존하되, 현재 physical blocker와 다음 수정 방향은 이 문서와 `SET_OPERATION_MODE_PHYSICAL_FINDING_OUTCOME_STORAGE_20260831.md`를 우선한다.

---

## 1. 현재 판정

SetOperationMode software에는 PP/PV/IP/CSP lifecycle, supported-mode mask, durable no-replay recovery, cross-mode preflight, final Diagnostics refresh와 operator diagnostics가 통합돼 있다.

2026-08-31 실기에서는 host freshness-ordering 문제를 넘어 다음 단계까지 도달한다.

```text
cross-mode preflight PASS
final Diagnostics refresh PASS
Prepare PASS
durable journal arm PASS
Start dispatch
PLC definitive reject
```

하지만 PLC는 Start를 다음으로 거절한다.

```text
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

확인된 BootId는 `0x66`, `0x67`, `0x68`이며 세 boot에서 같은 blocker가 재현됐다.

현재 판정:

- host capability freshness ordering: **FIXED / no longer current blocker**
- host cross-mode preflight: **PASS on Axis1 CSP -> PP**
- durable pre-dispatch arm/no-replay rejection handling: **PASS**
- PLC SetOperationMode Start acceptance: **FAIL / Detail 49**
- actual `0x6060` dispatch: **NOT REACHED**
- physical mode-change PASS: **OPEN**
- production release: **NO-GO**

---

## 2. activation/source truth

current source expectation:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin FeatureMask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

`0x018A` = PP(1), PV(3), IP(7), CSP(8).

WPF live capability refresh가 성공하고 requested PP가 advertised 상태인 것은 host-side supported-mode admission이 정상임을 의미한다. 그러나 source constant가 TRUE라는 사실만으로 loaded/generated runtime image의 exact activation state까지 증명하지는 않는다.

---

## 3. normal semantic lifecycle

정상 cross-mode contract는 유지한다.

```text
fresh 0x6061 / axis / 0x6041 preflight
-> final Diagnostics identity refresh
-> Prepare one-shot intent
-> durable ArmBeforeDispatch
-> Start exactly once
-> PLC ownership/outcome admission
-> exact one-byte 0x6060 Write(requested mode)
-> 0x6061 verify
-> terminal outcome
-> exact-generation retire
```

same-target는 `SucceededNoWrite`가 허용된다.

cross-mode는 다음 안전 조건을 유지한다.

- Standstill=True
- DS402 Fault=False
- DS402 OperationEnabled=False

raw Generic SDO `0x6060` Write는 계속 금지한다.

---

## 4. 2026-08-31 physical blocker

최신 evidence:

```text
WPF BuildUtc=2026-08-31 02:33:22 UTC
SDK BuildUtc=2026-08-31 02:33:19 UTC
Build=1
BootId=0x00000068
MapRevision=0x957F101E
currentMode=8
requestedMode=1
StatusWord=0x02D0
RequestId=3
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

Start는 definitive reject이며 retained PLC terminal outcome은 만들어지지 않았다. host durable journal은 rejection evidence를 archive하고 recovery interlock를 clear했다. 자동 Start replay는 없었다.

따라서 최신 failure point는 **PLC Start admission, actual mode SDO write 이전**이다.

---

## 5. owner-channel correction의 재평가

commit `c670bd6fbc816116eacbe19b94199479d1a8cacf`는 다음 source correction을 포함한다.

- embedded LASAL client metadata order를 generated declaration/class table과 정렬;
- `AxisOwnership` disconnected를 Detail 52로 분리;
- SDK/detail catalog/static verifier 동기화.

이 correction은 source inconsistency를 제거했지만 physical blocker를 해결하지 못했다. BootId `0x68` 재시험에서도 Detail 49가 유지됐다.

따라서 설계상 이 commit을 다음처럼 분류한다.

```text
source consistency / diagnostic discrimination: COMPLETE
physical root-cause correction: NOT COMPLETE
```

---

## 6. corrected Detail semantics와 남은 candidate

현재 Start admission source는 다음을 구분한다.

```text
zero CallerSessionEpoch / RequestSequence /
AdmissionToken / OwnerGeneration
    -> 49

AxisOwnership disconnected
    -> 52

ownership identity validate/commit failure
    -> 42
```

그리고 runtime SetOperationMode gate OFF는 Detail 64를 반환한다.

따라서 exact corrected image가 실행된다는 전제에서는 최신 49를 AxisOwnership disconnected로 다시 추정하지 않는다.

현재 남은 주요 후보:

1. **runtime activation/generation mismatch** — source는 gate ON이지만 generated/loaded runtime에서 OFF 또는 다른 artifact;
2. **ownership admission tuple forwarding defect** — TCP reserve 이후 Diagnostics에 전달되는 session/sequence/token/generation 중 하나가 zero.

현재 response Detail 49만으로 두 후보를 분리할 수 없다.

---

## 7. Detail 49 observability — implemented

Detail 49 ambiguity split은 safety-path 변경 없이 current `dev`에 구현됐다.

### 7.1 protocol/detail split proposal

아래 63/64는 current protocol contract다.

```text
49 SetOperationModeOutcomeStorageUnavailable
   실제 outcome infrastructure unavailable 전용

52 SetOperationModeOwnershipChannelUnavailable
   AxisOwnership client disconnected

63 SetOperationModeAdmissionIdentityUnavailable
   session/sequence/token/generation 중 하나가 zero

64 SetOperationModeFeatureDisabled
   runtime feature gate OFF
```

SDK enum, parser Start rejection acceptance, error catalog, WPF symbolic logging, static verifier를 하나의 protocol change로 반영했다.

### 7.2 evidence fields

Start reject diagnostic은 최소 다음 boolean/zero-state evidence를 남겨야 한다.

- FeatureEnabled
- CallerSessionEpochNonZero
- RequestSequenceNonZero
- AdmissionTokenNonZero
- OwnerGenerationNonZero
- AxisOwnershipConnected
- OwnershipIdentityValidated
- OwnershipCommitted

정상 token 원문 값 자체를 출력할 필요는 없다.

### 7.3 decision rule

원인 discriminator 없이 functional code를 추가 변경하지 않는다.

- FeatureDisabled -> generated/runtime activation path 수정
- AdmissionIdentityUnavailable -> TCP reserve/forwarding contract 수정
- OwnershipChannelUnavailable(52) -> LASAL network/channel binding 수정
- AxisOwnership conflict/quarantine(42) -> ownership identity/state 분석
- Start accepted -> 실제 `0x6060` lifecycle qualification 진행

---

## 8. regression/qualification requirement

이번 implementation의 software regression requirement:

1. 각 detail producer가 다른 원인으로 정확히 분리됨;
2. zero admission tuple은 mutation wire 없이 fail;
3. disconnected AxisOwnership은 Detail 52 유지;
4. gate OFF는 dedicated feature-disabled detail로 분리;
5. valid tuple + connected owner는 Validate/Commit path로 진행;
6. 기존 Standstill/Fault/OperationEnabled fence 유지;
7. accepted/uncertain Start no-replay 유지;
8. raw Generic SDO `0x6060` block 유지.

physical qualification은 Start accepted 이후에만 PP/PV/IP/CSP matrix로 진행한다.

---

## 9. current physical matrix status

| current | requested | 현재 결과 | 판정 |
|---|---|---|---|
| CSP(8) | PP(1) | preflight/Prepare/journal PASS -> PLC Detail 49 | BLOCKED before `0x6060` |
| CSP(8) | PV(3) | latest blocker 기준 미완료 | OPEN |
| CSP(8) | IP(7) | latest blocker 기준 미완료 | OPEN |
| CSP(8) | CSP(8) | same-target no-write path 별도 | physical mutation evidence 아님 |
| PP/PV/IP | CSP(8) | 미수행 | OPEN |

---

## 10. 작업 정책

현재 사용자 요청에 따라 **이번 단계에서는 design/status 문서만 갱신하고 functional source는 추가 수정하지 않는다.**

다음 구현은 Detail 49 내부 원인을 observable하게 분리하는 설계가 승인/진행될 때 시작한다.

새 장기 qualification branch는 만들지 않고 `dev`를 current integration source로 유지한다.

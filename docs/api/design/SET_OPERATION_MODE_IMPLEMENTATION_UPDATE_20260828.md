# SetOperationMode 구현 설계 보완 — 2026-08-28 / 2026-08-31 admission-boundary update

> 상태: **CURRENT IMPLEMENTATION / PHYSICAL BLOCKER ANALYSIS**
>
> integration / qualification source: `dev`
>
> analyzed implementation baseline: `dev@1ab539c4b82918d1e2095e73c03799415d9d06d0`
>
> latest physical evidence: `Build=1 / BootId=0x0000006A / MapRevision=0x957F101E`
>
> production release posture: **NO-GO**
>
> 추적: issue #46
>
> 기존 historical design은 보존하되, 현재 SetOperationMode blocker와 다음 구현 방향은 이 문서와 `SET_OPERATION_MODE_PHYSICAL_FINDING_OUTCOME_STORAGE_20260831.md`를 우선한다.

---

## 1. 현재 판정

SetOperationMode software에는 PP/PV/IP/CSP lifecycle, supported-mode mask, durable no-replay recovery, canonical Start UI path, cross-mode preflight, final Diagnostics refresh와 Detail 49 observability split이 통합돼 있다.

2026-08-31 13:18 실기에서 Axis1 CSP(8) -> PP(1)는 다음 host path를 정상 통과했다.

```text
SetOperationMode Start UI handler entered
cross-mode preflight PASS
  currentMode=8
  requestedMode=1
  StatusWord=0x02D0
final Diagnostics refresh PASS
  Build=1
  BootId=0x0000006A
  MapRevision=0x957F101E
Prepare PASS
  RequestId=4
durable journal arm PASS
```

그 뒤 PLC Start가 mutation 전에 다음으로 definitive reject됐다.

```text
ErrorId=-31000
Detail=SetOperationModeAdmissionIdentityUnavailable(63)
```

따라서 current 판정은 다음과 같다.

- canonical UI Start path: **PASS**
- host capability freshness ordering: **FIXED / PASS**
- Axis1 CSP -> PP safety preflight: **PASS**
- final Diagnostics current observation / Prepare: **PASS**
- durable pre-dispatch arm / no-replay rejection handling: **PASS**
- Detail 49 discriminator implementation: **WORKING — physical result narrowed to Detail 63**
- PLC Start admission identity transfer: **FAIL / P0 blocker**
- actual `0x6060` dispatch: **NOT REACHED**
- physical mode-change PASS: **NOT ESTABLISHED**
- production release: **NO-GO**

이번 결과는 이전 Detail 49와 동일한 오류로 취급하지 않는다. 63이 반환됐다는 것은 current implementation의 분리 진단이 실제 PLC에서 동작했고, 원인이 **feature disabled(64), owner channel disconnected(52), ownership validate/commit failure(42)**가 아니라 admission identity zero-state branch까지 좁혀졌다는 의미다.

---

## 2. current implementation baseline audit

현재 구현 브랜치는 별도 qualification branch가 아니라 `dev`이며 분석 baseline은 다음 commit이다.

```text
1ab539c4b82918d1e2095e73c03799415d9d06d0
message: dev : SetOperationMode update
```

이 commit은 SetOperationMode Start detail을 다음처럼 분리한다.

```text
49 SetOperationModeOutcomeStorageUnavailable
52 SetOperationModeOwnershipChannelUnavailable
63 SetOperationModeAdmissionIdentityUnavailable
64 SetOperationModeFeatureDisabled
42 ownership identity validation/commit failure
```

Detail 63 producer는 `LMCDiagnosticsService.HandleAxisSetOperationModeStart()`의 다음 condition이다.

```text
CallerSessionEpoch == 0
OR RequestSequence == 0
OR AdmissionToken == 0
OR OwnerGeneration == 0
    -> Detail 63
```

따라서 latest run에서 **위 네 값 중 적어도 하나가 Diagnostics Start entry에서 0이었다는 사실은 proven evidence**다.

현재 응답은 네 값 중 어느 필드가 0인지까지 구분하지 않는다. 즉 Detail 63 구현은 49 ambiguity를 한 단계 줄였지만, admission tuple 내부 observability는 아직 부족하다.

---

## 3. admission path source trace

### 3.1 TCP caller sequence

`TCPMotionInterface`의 diagnostics Start path는 admission output 변수를 0으로 초기화한 뒤 `ControlCommands.ReserveAxisOwnership()`을 호출한다.

```text
diagnosticsAdmissionToken = 0
diagnosticsOwnerGeneration = 0
diagnosticsAdmissionResult = -1

ControlCommands.ReserveAxisOwnership(
    CallerSessionEpoch = ActiveRequest.SessionEpoch,
    RequestSequence = ActiveRequest.Sequence,
    pAdmissionToken = &diagnosticsAdmissionToken,
    pOwnerGeneration = &diagnosticsOwnerGeneration,
    ...)
```

그리고:

```text
if diagnosticsAdmissionResult == 0
    diagnosticsReserved = TRUE
```

SetOperationMode는 admission result가 0일 때만 Diagnostics로 전달된다.

```text
if diagnosticsOperationModeStartValid
   AND diagnosticsAdmissionResult == 0
   AND Diagnostics connected
then
    Diagnostics.HandleRequest(
        CallerSessionEpoch = ActiveRequest.SessionEpoch,
        RequestSequence = ActiveRequest.Sequence,
        AdmissionToken = diagnosticsAdmissionToken,
        OwnerGeneration = diagnosticsOwnerGeneration,
        ...)
```

즉 TCP source에는 SetOperationMode용으로 Reserve 성공 후 token/generation을 의도적으로 0으로 다시 만드는 assignment가 없다.

### 3.2 ownership server success contract

`LMCControlCommandService.ReserveAxisOwnership()`은 entry에서 output pointer가 유효한지 검사하고 output 값을 0으로 초기화한다.

입력 validation은 다음을 포함한다.

```text
CallerSessionEpoch != 0
RequestSequence != 0
```

둘 중 하나가 0이면 `Result=0` 정상 admission으로 진행해서는 안 된다.

normal reservation path에서 server는:

```text
nextToken = OwnershipState[...] + 1
if nextToken == 0 -> nextToken = 1

nextGeneration = ... + 1
if nextGeneration == 0 -> nextGeneration = 1

*pAdmissionToken = nextToken
*pOwnerGeneration = nextGeneration
Result = 0
```

으로 종료한다.

repeat/resume path도 valid repeat record의 nonzero token/generation을 output에 기록한 뒤 반환한다.

### 3.3 핵심 invariant

따라서 같은 generated ABI와 정상 marshalling을 전제로 하면 다음 조합은 성립하면 안 된다.

```text
ReserveAxisOwnership Result == 0
        AND
Diagnostics.HandleRequest dispatched
        AND
(session == 0 OR sequence == 0 OR token == 0 OR generation == 0)
```

그런데 BootId `0x6A` 실기는 바로 이 impossible state를 Detail 63으로 관측했다.

**현재 root-cause class는 `admission identity generation failure`로 단정하지 않는다.** server source는 successful return 전에 nonzero identity를 만드는 계약을 갖고 있기 때문이다.

현재 가장 높은 우선순위의 결함 범주는 다음이다.

1. `CltChCmd_LMCControlCommandService` client/server generated ABI 또는 command-table mismatch;
2. `ReserveAxisOwnership`의 pointer output marshalling이 server와 TCP caller 사이에서 일치하지 않음;
3. server에서는 nonzero reservation이 만들어졌지만 caller-side output variable에 반영되지 않음;
4. Reserve return 이후 Diagnostics call 전의 tuple corruption/overwrite;
5. 낮은 우선순위로 exact generated artifact/runtime mismatch.

이 중 어느 하나인지 current Detail 63만으로는 확정할 수 없다.

---

## 4. generated-source consistency audit

현재 `dev`에서 추가로 확인할 source-consistency 항목이 있다.

### 4.1 LMCDiagnosticsService declaration ordering

current file의 embedded LASAL metadata client order:

```text
AxisOwnership
InputLatch
RecorderStore
...
```

실제 ST class declaration order:

```text
InputLatch
AxisOwnership
RecorderStore
...
```

이 순서 불일치는 과거 physical blocker의 직접 원인으로 증명되지 않았으며, Detail 63은 AxisOwnership connectivity 검사보다 먼저 발생한다. 따라서 **Detail 63의 직접 root cause라고 단정하지 않는다.**

하지만 파일이 LASAL2 CodeGenerator 생성물임을 고려하면 hand-edited metadata/declaration drift는 generated ABI qualification에서 허용하지 않는다. 다음 implementation tranche에서는 code generator를 통해 source/declaration/table을 한 번에 재생성하고 fingerprint를 고정해야 한다.

### 4.2 TCPMotionInterface client declaration ordering

embedded metadata와 ST declaration의 client ordering도 완전히 동일하지 않다. 현재 communication network generated table은 이름 기준으로 다음 연결을 갖고 있다.

```text
TCPMotionInterface1.Diagnostics
    -> LMCDiagnosticsService1.ClassSvr
TCPMotionInterface1.ControlCommands
    -> LMCControlCommandService1.ClassSvr
LMCDiagnosticsService1.AxisOwnership
    -> LMCControlCommandService1.ClassSvr
```

따라서 network name wiring 자체는 존재한다. 다음 단계에서 문제 삼아야 할 것은 연결 이름의 존재 여부가 아니라 **generated command-method ABI / argument marshalling identity**다.

---

## 5. 이번 실기로 배제되거나 낮아진 원인

latest Detail 63 기준으로 다음 원인을 current primary blocker로 두지 않는다.

- unsupported PP mode: preflight/advertisement 통과;
- DS402 unsafe state: `StatusWord=0x02D0`, cross-mode preflight PASS;
- stale Diagnostics observation: final refresh와 Prepare PASS;
- feature gate OFF: current implementation이면 Detail 64여야 함;
- `LMCDiagnosticsService.AxisOwnership` disconnected: Detail 52여야 함;
- ownership Validate/Commit mismatch: Detail 42 path이며 현재는 그 이전에서 fail;
- drive `0x6060` reject: 아직 `0x6060` dispatch까지 도달하지 않음;
- `0x6061` verification mismatch: mutation 이전이므로 해당 없음.

---

## 6. 수정 설계 — P0 admission-boundary evidence first

다음 functional patch는 token을 임의 생성하거나 safety check를 제거하는 수정이 아니다. **Reserve server -> TCP caller -> Diagnostics entry 세 경계가 같은 admission identity를 보고 있는지 먼저 증명**한다.

### 6.1 Boundary A — Reserve server exit evidence

`LMCControlCommandService.ReserveAxisOwnership()`의 successful exit 직전에 다음 non-sensitive evidence를 기록한다.

```text
ReservationResult = 0
SessionNonZero
SequenceNonZero
AdmissionTokenNonZero
OwnerGenerationNonZero
EffectiveAxisMaskNonZero
CommandId
Reference
```

raw token/generation 값을 일반 운영 로그에 출력할 필요는 없다. 필요하면 qualification-only hashed/correlation fingerprint를 사용한다.

### 6.2 Boundary B — TCP post-call evidence

`ControlCommands.ReserveAxisOwnership()` 직후, `diagnosticsAdmissionResult == 0`일 때 Diagnostics dispatch 전에 같은 bitmap을 계산한다.

```text
ReserveResult
SessionNonZero
SequenceNonZero
AdmissionTokenNonZero
OwnerGenerationNonZero
EffectiveAxisMaskNonZero
```

A에서 모두 TRUE인데 B에서 token/generation이 FALSE라면 **client/server output ABI 또는 pointer marshalling defect로 확정**한다.

### 6.3 Boundary C — Diagnostics entry evidence

`LMCDiagnosticsService.HandleAxisSetOperationModeStart()` entry에서 Detail 63을 만들기 전에 같은 zero/nonzero bitmap을 capture한다.

B는 모두 TRUE인데 C에서 값이 사라지면 **TCP -> Diagnostics generated method ABI/marshalling defect**로 확정한다.

### 6.4 admission bitmap contract

qualification log에는 예를 들어 다음처럼 값 자체 대신 bitmask만 남긴다.

```text
bit0 = SessionNonZero
bit1 = SequenceNonZero
bit2 = AdmissionTokenNonZero
bit3 = OwnerGenerationNonZero
bit4 = EffectiveAxisMaskNonZero
```

expected valid bitmap:

```text
0x1F
```

Detail 63 발생 시 bitmap을 함께 기록해야 한다. 현재처럼 `63`만 남기는 상태는 다음 root cause를 구분하기에 부족하다.

---

## 7. 수정 설계 — generated ABI qualification

### 7.1 hand-edit 금지 / full regeneration

`LMCControlCommandService`, `LMCDiagnosticsService`, `TCPMotionInterface`는 generated class declaration/command channel ABI에 관여한다.

다음 corrective build에서는:

1. embedded/generated declaration을 수동 순서 수정하지 않는다;
2. LASAL2 CodeGenerator로 관련 class interface를 재생성한다;
3. `CltChCmd_LMCControlCommandService`와 `CltChCmd_LMCDiagnosticsService`가 current server GLOBAL function signature에서 생성됐음을 확인한다;
4. fresh `Classes.lcb`, project `.lcb`, communication network artifact를 Rebuild + Link한다;
5. generated table/class fingerprint를 evidence로 남긴다;
6. 그 exact artifact만 PLC에 다운로드한다.

### 7.2 ReserveAxisOwnership ABI freeze

server/client contract에서 최소 다음 signature를 freeze한다.

```text
CommandId            UINT
Reference            UINT
RequestedAxisMask    UDINT
OwnerKind            UINT
ResourceKind         UINT
AdmissionMode        UINT
CallerSessionEpoch   UDINT
RequestSequence      UDINT
pIdentity            ^void
IdentitySize         UDINT
pEffectiveAxisMask   ^UDINT
pAdmissionToken      ^UDINT
pOwnerGeneration     ^UDINT
Result               DINT
```

static verification은 source text 존재 여부만 검사하지 말고, generated client/server command ABI가 동일 generation에서 생성됐다는 artifact identity까지 검사해야 한다.

---

## 8. fail-closed hardening 설계

Boundary B에서 다음 impossible state가 보이면 Diagnostics Start를 보내지 않는다.

```text
ReserveResult == 0
AND admission bitmap != 0x1F
```

단, server가 이미 reservation을 만들었을 가능성이 있으므로 단순히 local error를 반환하고 끝내면 ownership leak이 생길 수 있다.

따라서 guard 구현 전 다음 두 경우를 분리한다.

1. **server evidence도 incomplete**: reservation 자체가 정상 생성되지 않은 것 — server reservation path 수정;
2. **server evidence complete / caller evidence incomplete**: output marshalling defect — generated ABI를 수정하고, reservation cleanup은 server-side exact session/sequence evidence를 이용할 수 있는 안전한 cleanup/reconciliation 경로를 설계한다.

missing token을 임의로 만들어 `RollbackAxisOwnership()`에 넣거나 새 Start를 replay하지 않는다.

---

## 9. correction decision tree

```text
A(server exit) bitmap != 0x1F
    -> ReserveAxisOwnership server logic defect

A = 0x1F, B(TCP after call) != 0x1F
    -> ControlCommands generated client/output marshalling defect

A = 0x1F, B = 0x1F, C(Diagnostics entry) != 0x0F
    -> TCP -> Diagnostics HandleRequest ABI/marshalling defect

A/B/C all valid
    but ValidateAxisOwnershipIdentity fails
    -> ownership identity/hash/resource contract defect (Detail 42)

A/B/C all valid + Validate/Commit pass
    -> proceed to one-shot 0x6060 mutation lifecycle
```

여기서 C는 session/sequence/token/generation 네 필드 기준 `0x0F`가 expected다.

---

## 10. regression requirement

다음 구현 완료 조건은 최소 다음을 포함한다.

1. Detail 63 fixture가 각 zero field를 구분하는 bitmap evidence를 생성;
2. Reserve server success path가 session/sequence/token/generation nonzero invariant를 보장;
3. TCP caller가 successful Reserve 후 동일 nonzero invariant를 확인;
4. TCP impossible-state guard는 zero mutation wire;
5. generated client/server ABI fingerprint mismatch는 build/qualification gate에서 fail;
6. Diagnostics entry가 TCP post-call tuple과 동일 nonzero 상태임을 검증;
7. existing Detail 52/63/64/42 producer separation 유지;
8. Standstill/Fault/OperationEnabled fence 유지;
9. durable pre-dispatch journal/no-replay 유지;
10. Generic SDO `0x6060` block 유지.

PC unit test만으로 generated LASAL command-channel ABI를 증명했다고 간주하지 않는다. 최소 fresh C78/ARM generated artifact + PLC runtime boundary evidence가 필요하다.

---

## 11. physical qualification restart gate

다음 physical CSP -> PP 재시험은 아래가 준비된 뒤 진행한다.

```text
exact dev SHA
fresh generated class interfaces
fresh Classes.lcb / project lcb / Comm network identity
C78/ARM zero-error + Link PASS
PLC fresh BootId
WPF/SDK exact build identity
Boundary A/B/C admission bitmap logging
```

첫 목표는 mode 변경 성공이 아니라 다음 invariant 확인이다.

```text
A = 0x1F
B = 0x1F
C = 0x0F
```

이 invariant가 확인되기 전에는 `0x6060` lifecycle을 수정하지 않는다.

그 뒤 Start가 accepted되면 비로소:

```text
exact one 0x6060=1
-> exact 0x6061=1 verify
-> retained terminal outcome
-> exact-generation retire
```

을 physical PASS 조건으로 사용한다.

---

## 12. safety boundaries

이번 원인 분석은 다음 변경을 허용하지 않는다.

- admission token / owner generation 임의 생성;
- ownership reservation/validation/commit 우회;
- missing identity 상태에서 Diagnostics mutation 강행;
- `requireCurrentObservation=true` 제거;
- OperationEnabled 상태 cross-mode 허용;
- Standstill/Fault fence 제거;
- accepted/uncertain Start replay;
- raw Generic SDO `0x6060` mutation;
- physical PASS 전 production activation 승인.

현재 production 판정은 계속 **NO-GO**다.

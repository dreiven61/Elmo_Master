# SetOperationMode physical finding — admission identity unavailable after Detail 49 split

- Date: 2026-08-31
- analyzed implementation: `dev@1ab539c4b82918d1e2095e73c03799415d9d06d0`
- latest WPF BuildUtc: `2026-08-31 04:17:26 UTC`
- latest SDK BuildUtc: `2026-08-31 04:17:24 UTC`
- latest PLC identity: `Build=1 / BootId=0x0000006A / MapRevision=0x957F101E`
- latest request: Axis1 CSP(8) -> ProfilePosition(1), RequestId=4
- status: **HOST PATH PASS / PLC ADMISSION IDENTITY REJECT / 0x6060 NOT REACHED**
- production release: **NO-GO**

## 1. Physical reproduction history

이전 fresh boot에서는 SetOperationMode Start가 Detail 49로 거절됐다.

| BootId | result | interpretation at that time |
|---|---|---|
| `0x00000066` | Detail 49 | storage/admission ambiguity |
| `0x00000067` | Detail 49 | stale-running-boot explanation weakened |
| `0x00000068` | Detail 49 | owner-channel correction did not solve physical blocker |

그 후 current `dev`에 observability split이 구현됐다.

```text
49 = SetOperationModeOutcomeStorageUnavailable
52 = SetOperationModeOwnershipChannelUnavailable
63 = SetOperationModeAdmissionIdentityUnavailable
64 = SetOperationModeFeatureDisabled
42 = ownership validation/commit failure
```

latest BootId `0x0000006A`에서 결과가 처음으로 Detail 63으로 좁혀졌다.

```text
[13:18:04.205] cross-mode preflight passed: currentMode=8, requestedMode=1, StatusWord=0x02D0
[13:18:04.208] final Diagnostics refreshed: Build=1, BootId=0x0000006A, MapRevision=0x957F101E
[13:18:04.212] prepared: RequestId=4
[13:18:04.220] journal armed before dispatch
[13:18:04.334] definitive Start rejection
  ErrorId=-31000
  Detail=SetOperationModeAdmissionIdentityUnavailable(63)
```

성공 Start acknowledgement는 없으며 실제 `0x6060` Write evidence도 없다.

## 2. What Detail 63 proves

current PLC source에서 Detail 63은 다음 조건에만 사용된다.

```text
CallerSessionEpoch == 0
OR RequestSequence == 0
OR AdmissionToken == 0
OR OwnerGeneration == 0
```

따라서 latest physical evidence가 직접 증명하는 것은:

> `LMCDiagnosticsService.HandleAxisSetOperationModeStart()`가 실행될 때 네 admission identity field 중 최소 하나가 zero였다.

반대로 이번 run에서 current primary cause로 보지 않는 것:

- feature gate OFF: dedicated Detail 64가 존재;
- AxisOwnership disconnected: dedicated Detail 52가 존재;
- ownership validation/commit rejection: Detail 42 path;
- host stale capability: final refresh/Prepare PASS;
- unsupported/unsafe cross-mode: preflight PASS;
- drive 0x6060 failure: 아직 write stage 미도달.

## 3. Source contradiction found

`TCPMotionInterface`는 diagnostics admission outputs를 zero로 초기화하고 `ControlCommands.ReserveAxisOwnership()`을 호출한다.

SetOperationMode는 다음 조건에서만 `Diagnostics.HandleRequest()`로 전달된다.

```text
diagnosticsOperationModeStartValid
AND diagnosticsAdmissionResult == 0
AND Diagnostics connected
```

전달 값은:

```text
CallerSessionEpoch = ActiveRequest.SessionEpoch
RequestSequence = ActiveRequest.Sequence
AdmissionToken = diagnosticsAdmissionToken
OwnerGeneration = diagnosticsOwnerGeneration
```

반면 `LMCControlCommandService.ReserveAxisOwnership()`의 contract는:

- zero `CallerSessionEpoch` / `RequestSequence`를 valid reservation으로 허용하지 않음;
- normal success에서 token을 증가시키고 wrap-to-zero이면 1로 교정;
- generation도 증가시키고 wrap-to-zero이면 1로 교정;
- success 직전에 `pAdmissionToken` / `pOwnerGeneration`에 값을 기록;
- 최종 `Result := 0`.

repeat reservation path도 valid existing nonzero token/generation을 output에 기록한 뒤 반환한다.

따라서 정상 동일 ABI라면 다음 상태는 모순이다.

```text
Reserve Result == 0
-> Diagnostics Start dispatched
-> Diagnostics sees a zero admission identity field
```

이번 Detail 63은 바로 이 **cross-service admission-boundary invariant violation**을 관측한 것이다.

## 4. Root-cause classification

### Proven

- PLC Start rejection point는 ownership admission identity validation 전의 zero-tuple gate다.
- 적어도 한 admission field가 Diagnostics entry에서 zero다.
- no `0x6060` mutation occurred.

### Not yet proven

어느 field가 zero인지 current response/log는 알려주지 않는다.

### Highest-priority candidate class

1. `CltChCmd_LMCControlCommandService` generated client/server ABI mismatch;
2. `ReserveAxisOwnership` output-pointer marshalling mismatch;
3. server successful reservation output이 TCP caller variables에 반영되지 않음;
4. TCP caller에서 Reserve return 후 Diagnostics forwarding 전 tuple corruption;
5. generated artifact/runtime identity mismatch.

현재 source만으로 이 중 하나를 단정하지 않는다.

## 5. Generated source observations

current generated ST files에는 source consistency drift가 남아 있다.

`LMCDiagnosticsService` embedded metadata:

```text
AxisOwnership
InputLatch
RecorderStore
```

actual class declaration:

```text
InputLatch
AxisOwnership
RecorderStore
```

또한 `TCPMotionInterface` embedded metadata client ordering과 actual ST client declaration ordering도 완전히 동일하지 않다.

communication generated table에는 이름 기준으로 다음 connection이 존재한다.

```text
TCPMotionInterface1.ControlCommands -> LMCControlCommandService1.ClassSvr
TCPMotionInterface1.Diagnostics -> LMCDiagnosticsService1.ClassSvr
LMCDiagnosticsService1.AxisOwnership -> LMCControlCommandService1.ClassSvr
```

따라서 이 finding은 단순 missing network line으로 결론 내리지 않는다. generated code가 수동 편집/부분 regeneration 없이 **하나의 code-generator generation에서 client/server method ABI까지 동기화됐는지**가 다음 qualification 대상이다.

## 6. Required corrective evidence

다음 코드 수정 전에 세 boundary에서 admission bitmap을 확보한다.

### A. Reservation server successful exit

```text
SessionNonZero
SequenceNonZero
AdmissionTokenNonZero
OwnerGenerationNonZero
EffectiveAxisMaskNonZero
```

expected bitmap = `0x1F`.

### B. TCP immediately after Reserve call

동일 five-bit bitmap을 기록한다.

- A=0x1F, B!=0x1F -> ControlCommands client/output marshalling defect.

### C. Diagnostics Start entry

session/sequence/token/generation four-bit bitmap을 기록한다.

expected = `0x0F`.

- A=0x1F, B=0x1F, C!=0x0F -> TCP -> Diagnostics method ABI/marshalling defect.

raw admission token/generation을 일반 log에 노출하지 않는다.

## 7. Corrective design order

1. Boundary A/B/C nonzero bitmap instrumentation.
2. current LASAL generated class declarations/command interface full regeneration.
3. generated `CltChCmd_LMCControlCommandService` / `CltChCmd_LMCDiagnosticsService` ABI fingerprint verification.
4. fresh `Classes.lcb` / project `.lcb` / communication artifact Rebuild + Link.
5. exact generated artifact PLC download.
6. single CSP->PP retry.
7. evidence에 따라 server logic / ControlCommands marshalling / Diagnostics forwarding 중 한 경로만 수정.

이 순서 전에는 mutation lifecycle이나 drive SDO 로직을 수정하지 않는다.

## 8. Fail-closed rule

`ReserveResult==0`인데 TCP post-call admission bitmap이 invalid라면 이것은 정상 Start가 아니다.

그러나 server가 이미 ownership reservation을 만들었을 가능성이 있으므로 missing token을 임의 생성해 rollback하거나 새 Start를 replay해서는 안 된다.

correction은 server-side reservation evidence와 exact session/sequence reconciliation을 이용해 ownership leak 없이 fail-closed하도록 설계해야 한다.

## 9. Physical qualification boundary

현재 physical PASS로 인정되는 항목:

- connection/topology;
- Admin/SetOperationMode capabilities refresh;
- CSP current-mode observation;
- CSP->PP cross-mode preflight;
- final Diagnostics refresh;
- Prepare;
- durable pre-dispatch journal arm;
- definitive rejection handling/no automatic replay;
- Detail 63 discriminator physical operation.

아직 PASS가 아닌 항목:

- admission identity transfer;
- Start acceptance;
- `0x6060` one-write;
- `0x6061` target verification;
- terminal outcome/retire;
- accepted/uncertain recovery matrix.

## 10. Safety boundaries retained

금지:

- admission token/generation 임의 생성;
- ownership validation bypass;
- missing identity 상태에서 Diagnostics mutation 강행;
- Standstill/Fault/OperationEnabled fence 완화;
- current-observation freshness 제거;
- accepted/uncertain Start replay;
- Generic SDO로 `0x6060` mutation.

현재 release 판정은 계속 **NO-GO**다.

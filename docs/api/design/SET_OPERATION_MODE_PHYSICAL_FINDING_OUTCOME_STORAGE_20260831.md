# SetOperationMode physical finding — repeated OutcomeStorageUnavailable(49)

- Date: 2026-08-31
- latest evidence: Axis1 CSP(8) -> ProfilePosition(1), WPF BuildUtc `2026-08-31 02:33:22 UTC`, SDK BuildUtc `2026-08-31 02:33:19 UTC`
- latest PLC identity: `Build=1 / BootId=0x00000068 / MapRevision=0x957F101E`
- status: **HOST PREFLIGHT PASS / PLC START REJECT / 0x6060 NOT REACHED**
- physical mode-change PASS: **NOT ESTABLISHED**
- production release: **NO-GO**

## 1. Physical reproduction history

Axis1 current mode CSP(8)에서 ProfilePosition(1) cross-mode 요청은 host-side preflight를 통과한다.

```text
SetOperationMode Start UI handler entered
SetOperationMode cross-mode preflight passed
  axis=1
  currentMode=8
  requestedMode=1
  StatusWord=0x02D0
SetOperationMode final Diagnostics refreshed
SetOperationMode prepared
SetOperationMode journal armed before dispatch
```

그 뒤 PLC Start가 mutation 전에 definitive reject된다.

```text
Status=1
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

확인된 boot sequence:

| BootId | 결과 | 비고 |
|---|---|---|
| `0x00000066` | Detail 49 반복 | RequestId 3/5/7/9에서 재현 |
| `0x00000067` | Detail 49 | 새 PLC boot/download 후 재현 |
| `0x00000068` | Detail 49 | owner-channel diagnostic correction 이후 재현 |

`0x66 -> 0x67 -> 0x68`의 BootId 변화 때문에 단순히 동일 runtime boot가 남아 있었다는 설명은 더 이상 주 원인으로 취급하지 않는다.

최신 `0x68` 로그에서도 성공 Start acknowledgement가 없으므로 실제 `0x6060` Write evidence는 없다.

## 2. 이전 owner-channel correction 판정

commit `c670bd6fbc816116eacbe19b94199479d1a8cacf`에서 다음을 수행했다.

1. `LMCDiagnosticsService` embedded LASAL metadata의 client 순서를 generated declaration/class table과 일치시킴;
2. `AxisOwnership` runtime disconnected를 Detail 49에서 분리하여 Detail 52 `SetOperationModeOwnershipChannelUnavailable`로 정의;
3. SDK enum/error catalog와 static verifier를 동기화.

이 수정은 source consistency와 fault discrimination 관점에서는 유효하지만, **physical blocker를 해결했다는 증거는 없다.** 최신 boot `0x68`에서도 결과는 여전히 Detail 49다.

따라서 이 correction의 현재 판정은 다음과 같다.

```text
source consistency correction: PASS
physical SetOperationMode fix: NOT PROVEN / latest retest FAILED
```

향후 문서/분석에서 이 commit을 SetOperationMode physical fix로 표현하지 않는다.

## 3. Detail 49가 현재 의미하는 범위

현재 corrected `HandleAxisSetOperationModeStart()`에서 owner admission 직전 분기는 다음과 같이 분리돼 있다.

```text
CallerSessionEpoch == 0
or RequestSequence == 0
or AdmissionToken == 0
or OwnerGeneration == 0
    -> Detail 49

AxisOwnership client disconnected
    -> Detail 52

ValidateAxisOwnershipIdentity failure
    -> Detail 42
```

또한 SetOperationMode feature gate가 runtime image에서 OFF이면 Detail 49가 사용된다.

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE
    -> Detail 49
```

반면 outcome record corruption/occupied와 SDO executor/safety 문제는 각각 다른 detail로 분기된다.

따라서 **corrected image가 실제로 로드됐다는 전제에서 최신 Detail 49를 `AxisOwnership` disconnected로 계속 해석하면 안 된다.** 남은 high-value candidate는 다음 두 부류다.

- runtime feature activation/generation identity가 source expectation과 다름;
- TCP ownership reservation 이후 Diagnostics로 전달되는 admission tuple 중 하나가 0이거나 손실됨.

현재 로그만으로 둘 중 어느 쪽인지 확정할 수 없다.

## 4. 설계 결론 — 추가 추측성 functional patch 중단

세 번의 fresh-boot reproduction 후에는 증거 없이 다른 safety/ownership 코드를 계속 변경하지 않는다.

다음 functional correction은 **Detail 49 내부 원인을 추가로 분리하는 observability가 확보된 후** 결정한다.

특히 다음 항목은 원인 확인 없이 수정하지 않는다.

- SetOperationMode enable gate를 강제로 우회;
- ownership reservation/validation 제거;
- admission token/generation을 임의 생성;
- Standstill/Fault/OperationEnabled fence 완화;
- raw Generic SDO로 `0x6060` Write;
- retained outcome/no-replay 계약 제거.

## 5. 다음 수정 설계안 — 구현 전 문서 상태

현재 Detail 49는 서로 다른 조건을 다시 하나로 묶고 있다. 다음 구현에서는 이 ambiguity를 제거하는 방향으로 설계한다.

### 5.1 proposed diagnostic split

다음 숫자는 **설계 예약안이며 아직 구현된 protocol contract가 아니다.**

```text
49  SetOperationModeOutcomeStorageUnavailable
    실제 retained outcome infrastructure unavailable 전용

52  SetOperationModeOwnershipChannelUnavailable
    기존 구현 유지: AxisOwnership client disconnected

63  SetOperationModeAdmissionIdentityUnavailable   [PROPOSED]
    CallerSessionEpoch / RequestSequence /
    AdmissionToken / OwnerGeneration 중 하나가 zero

64  SetOperationModeFeatureDisabled                [PROPOSED]
    runtime SetOperationMode activation gate OFF
```

구현 시 SDK parser/error catalog/WPF log/static contract를 함께 갱신해야 한다.

### 5.2 required diagnostic evidence

다음 code change 전/후에 Start reject evidence가 최소 다음을 구분해야 한다.

- feature gate runtime value;
- caller session epoch zero/nonzero;
- request sequence zero/nonzero;
- admission token zero/nonzero;
- owner generation zero/nonzero;
- AxisOwnership connected/disconnected;
- ownership identity validate result;
- ownership commit result.

민감하지 않은 boolean/zero-nonzero 상태만 로그에 노출하고, 정상 token 값을 운영 로그에 그대로 출력할 필요는 없다.

### 5.3 implementation decision gate

다음 physical run에서 원인이 분리된 뒤에만 수정 방향을 정한다.

- feature-disabled가 확인되면 generated/runtime activation path를 수정;
- admission-identity zero가 확인되면 TCP reserve -> Diagnostics forwarding contract를 수정;
- Detail 52가 확인되면 LASAL channel/network binding을 수정;
- Detail 42가 확인되면 exact ownership identity mismatch를 분석;
- Start가 accepted되면 그때부터 `0x6060` one-write / `0x6061` verify lifecycle을 qualification한다.

## 6. physical qualification boundary

현재까지 physical PASS로 인정할 수 있는 것은 다음뿐이다.

- connection/topology load;
- SetOperationMode capability refresh;
- Axis1 CSP(8) current-mode observation;
- CSP -> PP cross-mode host preflight with `StatusWord=0x02D0`;
- final Diagnostics identity refresh;
- durable pre-dispatch journal arm;
- definitive PLC Start rejection handling/no automatic replay.

아직 PASS가 아닌 것:

- SetOperationMode Start acceptance;
- `0x6060=1/3/7/8` mutation;
- `0x6061` target verification;
- terminal outcome/retire lifecycle;
- restart/reconnect recovery after an accepted or uncertain mutation.

## 7. Safety boundaries retained

이 finding은 다음을 허용하지 않는다.

- `requireCurrentObservation=true` 제거;
- OperationEnabled 상태 cross-mode 허용;
- Standstill/Fault fence 제거;
- ownership validation 우회;
- retained outcome storage 우회;
- accepted/uncertain Start replay;
- Generic SDO를 통한 `0x6060` mutation.

현재 release 판정은 계속 **NO-GO**다.

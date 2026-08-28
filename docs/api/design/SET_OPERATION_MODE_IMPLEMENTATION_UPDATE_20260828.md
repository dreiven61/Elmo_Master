# SetOperationMode 구현 설계 보완 — 2026-08-28

> 상태: current implementation addendum
>
> current functional baseline: `dev@687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
>
> current integration / qualification source: `dev`
>
> qualification PR #18: **SUPERSEDED / closed / branch == current dev**
>
> 추적: issue #46
>
> 기존 `SET_OPERATION_MODE_DESIGN.md`의 historical evidence는 보존한다. 현재 source/activation 상태가 충돌하면 이 문서와 `DEVELOPMENT_STATUS_20260828.md`를 우선한다.

---

## 1. 현재 판정

SetOperationMode는 더 이상 CSP-only software scaffold가 아니다. current `dev`에는 PP/PV/IP/CSP multi-mode lifecycle, durable no-replay recovery, supported-mode advertisement, operator diagnostics와 2026-08-28 live-bench corrective preflight가 통합돼 있다.

하지만 **hardware qualification은 아직 완료되지 않았다.**

현재 상태를 다음처럼 구분한다.

- source/PC path: **implemented + corrective merged**
- qualification activation: **ON in current dev**
- fresh current-image C78/PLC evidence after PR #58: **OPEN**
- PP/PV/IP real cross-mode physical PASS: **OPEN**
- failure/recovery physical matrix: **OPEN**
- production release: **NO-GO**

---

## 2. current activation truth

현재 `dev` source는 다음과 같다.

### LMCDiagnosticsService

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
```

### LMCControlCommandService AdminCapabilities

```text
FeatureMask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

`0x018A`는 PP(1), PV(3), IP(7), CSP(8)를 의미한다.

따라서 live WPF에 다음이 보이면 source contract와 일치한다.

```text
AdminTriad=True
SupportedModeMask=0x018A
DiagnosticsIdentity=True
```

과거 문서의 `dev gate FALSE`, `bits 8/9/10 OFF`, `PR #18 activation branch 필요` 설명은 current source에는 적용하지 않는다.

현재 source는 **qualification-active**다. 이것을 production release 승인으로 해석하지 않는다.

---

## 3. software execution path

### 3.1 요청 mode

지원 target:

- ProfilePosition = 1
- ProfileVelocity = 3
- InterpolatedPosition = 7
- CyclicSynchronousPosition = 8

Homing(6)은 SetOperationMode가 소유하지 않는다.

### 3.2 Start admission

WPF/SDK Start는 최소 다음을 확인한다.

- connected / idle
- valid physical axis
- valid timeout
- explicit one-shot confirmation
- Admin capability triad
- selected mode가 live `SupportedModeMask`에 포함
- stable Diagnostics Build/BootId/MapRevision
- common diagnostics admission
- durable journal arm 가능

PR #58 이후 WPF status는 이 gate를 숨기지 않고 직접 표시한다.

```text
AdminTriad
SupportedModeMask
DiagnosticsIdentity
Confirmed
SelectedModeAdvertised
AdmissionAllowed
JournalReady
```

캡처에서 one-shot confirmation checkbox가 unchecked이면 `Confirmed=False`이므로 Start disabled가 정상이다.

### 3.3 preflight

Start를 durable arm하기 전에 current axis에서 fresh drive status를 읽는다.

- LASAL AxisStatus
- DS402 StatusWord `0x6041`
- Modes of operation display `0x6061`

same-target와 cross-mode를 구분한다.

#### same-target

`0x6061 == requestedMode`이면 PLC lifecycle은 write 없이 성공할 수 있다.

```text
SucceededNoWrite
```

특히 CSP -> CSP는 이 경로가 쉽게 발생한다. 따라서 CSP 성공만으로 `0x6060` Write가 정상이라고 판정하지 않는다.

#### cross-mode

`0x6061 != requestedMode`이면 실제 mode mutation 후보다. 다음 조건을 요구한다.

- `Standstill=True`
- DS402 Fault=False
- DS402 OperationEnabled=False

조건이 안 맞으면 WPF에서 Start를 보내지 않는다. PLC도 동일 안전 fence를 유지한다.

OperationEnabled 상태에서 mode 변경을 허용하도록 완화하지 않는다.

---

## 4. PLC lifecycle

정상 cross-mode 실행의 semantic sequence는 다음과 같다.

```text
0x6061 preflight Read
    -> exact one-byte 0x6060:0 Write(requestedMode)
    -> 0x6061 verify Read
    -> terminal outcome
    -> exact-generation retire
```

핵심 계약:

- mode Write는 maximum one dispatch
- raw Generic SDO로 `0x6060` mutation 금지
- terminal success는 observed mode == requested mode
- post-write timeout/disconnect/uncertain callback은 original `0x6060` Write를 replay하지 않음
- recovery는 read-only `0x6061` observation + outcome/retire만 사용

---

## 5. durable no-replay recovery

persisted identity는 endpoint와 diagnostics identity, client/request/axis/requested mode를 묶는다.

복구 시 original Start 또는 `0x6060` Write를 재전송하지 않는다.

가능한 recovery action:

- exact outcome query
- current mode observation
- durable terminal proof
- exact-generation retire

불확실 상태에서는 새로운 mutation Start를 자동 replay하지 않는다.

---

## 6. 2026-08-28 live-bench finding과 수정

### Finding A — CSP만 되는 것처럼 보임

원인 후보가 하나의 버그가 아니라 두 층으로 나뉜다.

1. CSP -> CSP는 실제 Write 없이 성공 가능
2. PP/PV/IP는 real cross-mode이므로 안전 preflight 및 실제 `0x6060` path를 통과해야 함

PR #58은 PC에서 이 차이를 명시하고 fresh preflight를 추가했다.

### Finding B — Start disabled 원인이 UI에 안 보임

과거 status는 live gate가 이미 TRUE여도 다음 stale 문구를 출력했다.

```text
Current PLC activation is expected to keep Start disabled ...
```

PR #58에서 제거하고 actual gate 값을 표시하도록 수정했다.

### Finding C — stale qualification branch

PR #18은 과거 gate-OFF dev에서 activation만 켜기 위해 만든 branch였다. 이후 구현이 dev에 계속 들어가면서 branch와 실제 current source가 분리됐다.

2026-08-28 재검토에서:

- PR #18 closed
- branch ref를 exact current dev에 정렬
- ahead=0 / behind=0

이후 physical qualification은 stale branch가 아니라 exact current `dev`를 사용한다.

---

## 7. software evidence

PR #58 corrective qualification:

- API Debug full suite: 1200/1200 PASS
- Generic SDO WPF focused smoke: 17/17 PASS
- API Debug/Release build: PASS
- WPF Debug/Release build: PASS
- corrective source verifier: PASS
- Generic SDO policy verifier: PASS
- diff hygiene: PASS

이 결과는 SetOperationMode physical PASS가 아니다. source/PC regression gate다.

---

## 8. 다음 qualification matrix

### Axis1 normal matrix

| current mode | requested mode | 기대 path | 필요한 evidence |
|---|---|---|---|
| CSP | CSP | no-write | `SucceededNoWrite`, no `0x6060` dispatch |
| CSP | PP | cross-mode | one `0x6060=1`, `0x6061=1` |
| CSP | PV | cross-mode | one `0x6060=3`, `0x6061=3` |
| CSP | IP | cross-mode | one `0x6060=7`, `0x6061=7` |
| PP/PV/IP | CSP | cross-mode | one `0x6060=8`, `0x6061=8` |
| any target | same target | no-write | no `0x6060` dispatch |

### failure matrix

- unsafe preflight
- OperationEnabled
- DS402 fault
- Write start reject
- Write timeout
- disconnect after dispatch
- verify timeout
- verify mismatch
- outcome response loss
- retire response loss
- restart/reconnect recovery

모든 uncertain mutation 결과는 original Start replay 없이 처리한다.

---

## 9. 다음 실제 작업 순서

1. exact current `dev` SHA 확정
2. fresh LASAL C78/ARM Rebuild + Link
3. generated artifact identity 기록
4. exact artifact PLC load
5. same source WPF 실행
6. live gate 확인
7. Axis1 matrix 수행
8. failure/recovery matrix 수행
9. Axis2..4 확대
10. qualification 결과를 근거로 production release activation/deactivation을 별도 결정

새 SetOperationMode qualification branch를 추가로 만들지 않는다.

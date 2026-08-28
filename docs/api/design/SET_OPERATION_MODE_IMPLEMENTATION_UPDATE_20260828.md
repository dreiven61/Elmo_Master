# SetOperationMode 구현 설계 보완 — 2026-08-28

> 상태: current implementation addendum
>
> current functional baseline: `dev@687a78c6e97616870c4fec4a5da043046bb735f6` (PR #58)
>
> current analyzed dev: `dev@cf92ef0e6891b227ac4c6da55256a524302b43ae`
>
> current integration / qualification source: `dev`
>
> qualification PR #18: **SUPERSEDED / closed**
>
> 추적: issue #46
>
> 기존 `SET_OPERATION_MODE_DESIGN.md`의 historical evidence는 보존한다. 현재 source/activation 상태가 충돌하면 이 문서와 `DEVELOPMENT_STATUS_20260828.md`를 우선한다.

---

## 1. 현재 판정

SetOperationMode는 더 이상 CSP-only software scaffold가 아니다. current `dev`에는 PP/PV/IP/CSP multi-mode lifecycle, durable no-replay recovery, supported-mode advertisement, operator diagnostics와 2026-08-28 live-bench corrective preflight가 통합돼 있다.

하지만 **hardware qualification은 아직 완료되지 않았다.** 2026-08-28 17:28 실기 로그에서 PP/PV/IP 모두 cross-mode safety preflight까지 통과했지만, `PrepareSetOperationMode()` 직전 host-side Diagnostics capability freshness ordering bug로 차단되는 것이 확인됐다.

현재 상태를 다음처럼 구분한다.

- source/PC path: **implemented, but current-observation ordering defect OPEN**
- qualification activation: **ON in current dev**
- fresh current-image C78/PLC evidence: **OBSERVED**
- PP/PV/IP real cross-mode `0x6060` dispatch: **NOT REACHED**
- physical mode-change PASS: **OPEN**
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

`0x6061 != requestedMode`이면 실제 mode mutation 후보이다. 다음 조건을 요구한다.

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

## 6. 2026-08-28 live-bench findings

### Finding A — CSP만 되는 것처럼 보임

원인 후보가 하나의 버그가 아니라 두 층으로 나뉜다.

1. CSP -> CSP는 실제 Write 없이 성공 가능
2. PP/PV/IP는 real cross-mode이므로 안전 preflight 및 실제 `0x6060` path를 통과해야 함

PR #58은 PC에서 이 차이를 명시하고 fresh preflight를 추가했다.

### Finding B — Start disabled 원인이 UI에 안 보임

과거 status는 live gate가 이미 TRUE여도 stale activation 문구를 출력했다. PR #58에서 actual gate 값을 표시하도록 수정했다.

### Finding C — stale qualification branch

과거 activation-only branch/PR은 current `dev`와 분리되어 source/image identity 혼란을 만들었다. 2026-08-28 branch cleanup 후 remote branch는 `main`, `dev`만 유지한다.

### Finding D — 17:28 live log: Diagnostics capability observation self-invalidation

#### 재현 로그

실행 identity:

```text
Version=0.9.1.0
BuildUtc=2026-08-28 08:27:44 UTC
SdkBuildUtc=2026-08-28 08:27:41 UTC
```

Axis1 / current CSP(8)에서 다음 결과가 반복됐다.

```text
requestedMode=3 -> cross-mode preflight passed, StatusWord=0x02D0
requestedMode=1 -> cross-mode preflight passed, StatusWord=0x02D0
requestedMode=7 -> cross-mode preflight passed, StatusWord=0x02D0
requestedMode=8 -> same-target no-write candidate
```

모든 시도는 이후 동일하게 실패했다.

```text
The supplied diagnostics capabilities are not the current observation.
```

#### 실패 위치

현재 WPF 순서:

```text
Admin.GetCapabilitiesAsync()
-> RefreshDiagnosticsCapabilitiesAsync()          // cached diagnosticCapabilities = observation N
-> GetPhysicalAxisAsync()
-> ReadDriveStatusAsync()                         // preflight
   -> D5 Read 0x6041
      -> SubmitInlineSdoRead()
         -> Diagnostics.GetCapabilities()         // observation N+1
   -> D5 Read 0x6061
      -> SubmitInlineSdoRead()
         -> Diagnostics.GetCapabilities()         // observation N+2
-> PrepareSetOperationMode(... cached observation N ...)
-> ValidateAxisSetPositionDiagnosticCapabilities(requireCurrentObservation=true)
-> N != CurrentCapabilityObservationSequence(N+2)
-> host exception / ZERO mutation wire
```

`LMCDiagnostics.GetCapabilities()`는 capability object를 만들 때 `NextCapabilityObservationSequence()`를 증가시킨다. `ReadDriveStatusAsync()`가 사용하는 inline D5 SDO helper는 각 SDO submission 전에 다시 `GetCapabilities()`를 수행한다. 따라서 preflight 자체가 WPF가 직전에 캐시한 `diagnosticCapabilities`를 stale로 만든다.

#### 판정

이번 실패는 다음이 아니다.

- PP/PV/IP unsupported reject 아님
- DS402 unsafe-state reject 아님
- PLC SetOperationMode Start reject 아님
- `0x6060` verify mismatch 아님

이번 실패는 **host-side capability freshness ordering defect**다.

실패 시점은 `PrepareSetOperationMode()` 내부 capability validation이므로:

- durable SetOperationMode journal arm 전
- `0x7D23 Start` 전
- `0x6060 Write` 전

즉 이 로그에서는 실제 mode mutation wire가 발생하지 않았다.

`D5 terminal wake ignored: no exact current retained ticket` 메시지는 preflight가 생성한 D5 read ticket completion과 시간적으로 대응한다. 현재 로그에서 이것은 primary failure가 아니라 preflight read activity에 동반된 callback noise로 분류한다. 별도 D5 retained-ticket 오류로 확대 해석하지 않는다.

---

## 7. corrective design for Finding D

### 7.1 단기 수정 원칙

fresh drive preflight가 Diagnostics capability observation을 소비한다는 사실을 execution ordering에 반영한다.

권장 순서:

```text
1. Admin capability refresh / selected mode advertise 확인
2. GetPhysicalAxis
3. ReadDriveStatusAsync fresh preflight
4. FINAL Diagnostics capability refresh
5. EnsureAxisSetOperationModeCapabilitiesReady
6. PrepareSetOperationMode
7. durable journal ArmBeforeDispatch
8. Start exactly once
```

핵심은 **마지막 Diagnostics capability refresh 이후 Prepare 사이에 `GetCapabilities()`를 발생시키는 D5 helper를 넣지 않는 것**이다.

### 7.2 유지할 safety contract

이 버그를 고치기 위해 다음을 완화하지 않는다.

- `requireCurrentObservation=true` freshness fence
- Build/BootId/MapRevision identity validation
- Standstill/Fault/OperationEnabled cross-mode preflight
- one-shot confirmation
- durable pre-dispatch journal
- no-replay invariant
- raw Generic SDO `0x6060` block

즉 해결 방향은 freshness 검증 제거가 아니라 **final observation ordering 수정**이다.

### 7.3 regression test requirement

수정 완료 조건에 다음 fixture를 추가한다.

1. diagnostics observation N 취득
2. `ReadDriveStatusAsync()` 또는 동등한 두 inline D5 read로 observation sequence가 진행됨을 재현
3. old observation N을 Prepare에 사용하면 zero-wire reject되는 기존 safety contract 확인
4. preflight 후 final diagnostics refresh한 observation N+2/N+3을 Prepare에 사용하면 준비 성공
5. Prepare 성공 전에는 journal/Start mutation 없음
6. final refresh 이후 별도 capability-producing call이 삽입되면 test가 실패

가능하면 WPF focused smoke에도 execution ordering을 고정하는 source/runtime assertion을 둔다.

---

## 8. software evidence

PR #58 corrective qualification:

- API Debug full suite: 1200/1200 PASS
- Generic SDO WPF focused smoke: 17/17 PASS
- API Debug/Release build: PASS
- WPF Debug/Release build: PASS
- corrective source verifier: PASS
- Generic SDO policy verifier: PASS
- diff hygiene: PASS

하지만 위 test들은 **preflight가 capability observation을 스스로 stale시키는 순서 문제를 잡지 못했다.** 따라서 Finding D regression fixture가 추가되기 전에는 SetOperationMode software qualification을 완전 PASS로 보지 않는다.

---

## 9. qualification matrix

Finding D가 소프트웨어에서 닫히기 전에는 physical matrix를 수행해도 `0x6060` 단계에 도달할 수 없으므로 먼저 host ordering을 수정한다.

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
- stale/future Diagnostics capability observation
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

## 10. 다음 실제 작업 순서

1. **NOW — Finding D host capability freshness ordering 수정**
2. focused API/WPF regression으로 preflight -> final diagnostics refresh -> Prepare 순서 고정
3. exact updated `dev` SHA 확정
4. fresh LASAL C78/ARM Rebuild + Link
5. generated artifact identity 기록
6. exact artifact PLC load
7. same source WPF 실행
8. Axis1 PP/PV/IP/CSP matrix 수행
9. failure/recovery matrix 수행
10. Axis2..4 확대
11. qualification 결과를 근거로 production release activation/deactivation을 별도 결정

새 SetOperationMode qualification branch를 추가로 만들지 않는다.

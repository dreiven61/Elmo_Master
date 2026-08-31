# SetOperationMode Start 실행 경로 정리 및 구현 계획 — 2026-08-31

> 상태: **IMPLEMENTATION READY / P0**
>
> 기준 source: `dev@b87e850046a81e339e91cb0f3dfb62a1afb286a5`
>
> 추적: issue #46
>
> 목적: WPF `Start Selected Mode Once (0x7D23)`의 UI event 경로를 단일화하고, 2026-08-28 17:28 실기에서 확인된 Diagnostics capability freshness ordering defect를 같은 실행 경로 안에서 수정한다.

---

## 1. 결론

현재 SetOperationMode Start 기능은 SDK/PLC protocol까지 구현되어 있으나 WPF execution path가 두 가지 문제를 가진다.

1. `ButtonStartAxisSetOperationMode_Click()`은 button 생성 시 등록되지만 이후 `InitializeReadOnlyApiUi()`에서 제거되고 `ButtonStartAxisSetOperationModeWithRejectResolution_Click()`으로 교체된다. 따라서 이름상 canonical handler처럼 보이는 `ButtonStartAxisSetOperationMode_Click()`은 실제 runtime에서 호출되지 않는 dead handler다.
2. 실제 handler가 호출한 `StartAxisSetOperationModeOnceAsync()`는 drive-status preflight 전에 취득한 Diagnostics capability를 preflight 뒤에도 사용한다. preflight의 inline D5 read가 capability observation sequence를 진행시키므로 `PrepareSetOperationMode()`에서 stale observation으로 거부된다.

두 문제를 한 번에 정리한다.

**Target:**

```text
Start Selected Mode Once button
  -> ButtonStartAxisSetOperationMode_Click          // 유일한 UI handler
     -> RunOperationAsync
        -> StartAxisSetOperationModeOnceAsync       // 유일한 Start orchestration
           -> UI/admission snapshot
           -> Admin capability check
           -> GetPhysicalAxis
           -> fresh drive-status preflight
           -> FINAL Diagnostics capability refresh
           -> PrepareSetOperationMode
           -> durable ArmBeforeDispatch
           -> SetOperationModeAsync exactly once
           -> accepted: read-only outcome recovery
           -> rejected: definitive rejection archival/clear
           -> uncertain: no replay, recovery interlock 유지
```

`ButtonStartAxisSetOperationModeWithRejectResolution_Click()`은 제거한다.

---

## 2. AS-IS source 구조

### 2.1 button 생성

파일:

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs`

현재 button은 runtime에서 생성된다.

```csharp
buttonStartAxisSetOperationMode = new Button
{
    Content = "Start Selected Mode Once (0x7D23)",
    IsEnabled = false
};
buttonStartAxisSetOperationMode.Click +=
    ButtonStartAxisSetOperationMode_Click;
```

이 시점만 보면 `ButtonStartAxisSetOperationMode_Click()`이 실제 handler처럼 보인다.

### 2.2 초기화 후 handler 교체

파일:

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs`

`InitializeReadOnlyApiUi()`가 recovery UI 생성 후 다음 작업을 수행한다.

```csharp
buttonStartAxisSetOperationMode.Click -=
    ButtonStartAxisSetOperationMode_Click;
buttonStartAxisSetOperationMode.Click +=
    ButtonStartAxisSetOperationModeWithRejectResolution_Click;
```

결과적으로 runtime event graph는 다음과 같다.

```text
ButtonStartAxisSetOperationMode_Click                 // source에는 존재, runtime에서는 detach
ButtonStartAxisSetOperationModeWithRejectResolution_Click // 실제 runtime handler
```

이 구조는 다음 문제를 만든다.

- breakpoint를 canonical name에 걸어도 hit되지 않음
- source search만으로 실제 call graph 파악이 어려움
- handler ownership이 `AxisSetOperationModeRecovery.cs`와 `ReadOnlyApi.cs`에 나뉨
- 초기화 순서 변경 시 double-subscribe / wrong-subscribe 회귀 가능
- reject-resolution이 왜 별도 UI handler에 있어야 하는지 contract가 불명확

### 2.3 실제 Start core는 이미 존재

`ButtonStartAxisSetOperationModeWithRejectResolution_Click()`은 결국 `StartAxisSetOperationModeOnceAsync()`를 호출한다.

SDK path도 존재한다.

```text
LMCSingleAxis.SetOperationModeAsync
  -> LMCAdmin.StartAxisSetOperationModeAsync
     -> LMC_AdminFrame.StartAxisSetOperationMode
        -> connection.ExchangeAsync
```

따라서 이번 작업은 신규 protocol 구현이 아니라 **WPF orchestration 정리 + freshness ordering 수정**이다.

---

## 3. TO-BE UI event ownership

### 3.1 canonical handler는 하나만 둔다

button 생성 시 처음부터 다음 handler 하나만 등록한다.

```csharp
buttonStartAxisSetOperationMode.Click +=
    ButtonStartAxisSetOperationMode_Click;
```

`InitializeReadOnlyApiUi()`에서는 SetOperationMode Start handler를 detach/rebind하지 않는다.

삭제 대상:

```text
ButtonStartAxisSetOperationModeWithRejectResolution_Click
buttonStartAxisSetOperationMode.Click -= ButtonStartAxisSetOperationMode_Click
buttonStartAxisSetOperationMode.Click += ButtonStartAxisSetOperationModeWithRejectResolution_Click
```

### 3.2 reject-resolution 책임은 canonical handler 안으로 이동

목표 handler 구조:

```csharp
private async void ButtonStartAxisSetOperationMode_Click(
    object sender,
    RoutedEventArgs e)
{
    await RunOperationAsync(
        "Set Operation Mode Selected Mode Once",
        async () =>
        {
            try
            {
                await StartAxisSetOperationModeOnceAsync();
            }
            catch (LMCAxisSetOperationModeRejectedException error)
            {
                ResolveDefinitiveAxisSetOperationModeStartRejection(...);
                throw;
            }
        });
}
```

기존 definitive rejection archival/evidence/active-journal clear 동작은 삭제하지 않고 그대로 canonical handler에 보존한다.

### 3.3 역할 경계

`ButtonStartAxisSetOperationMode_Click()` 책임:

- WPF event 진입점
- `RunOperationAsync()` wrapping
- definitive Start rejection UI/evidence resolution
- 예외를 정상 log pipeline으로 다시 전달

`StartAxisSetOperationModeOnceAsync()` 책임:

- Start admission 및 UI input snapshot
- physical axis 획득
- transition preflight
- final capability identity 획득
- command preparation
- durable pre-dispatch arm
- exactly-once Start dispatch
- accepted/uncertain lifecycle 진입

SDK 책임:

- prepared command ownership/freshness validation
- mutation gate
- `0x7D23` request 생성/전송
- write-boundary 이후 uncertain session semantics

PLC 책임:

- physical-axis/mode/state admission
- maximum one `0x6060` dispatch
- `0x6061` verification
- retained outcome/retirement

---

## 4. TO-BE Start execution ordering

### 4.1 현재 결함 순서

2026-08-28 17:28 실기에서 확인된 현재 흐름:

```text
Diagnostics refresh -> observation N
GetPhysicalAxis
ReadDriveStatusAsync
  -> 0x6041 inline D5 -> Diagnostics.GetCapabilities -> N+1
  -> 0x6061 inline D5 -> Diagnostics.GetCapabilities -> N+2
PrepareSetOperationMode(cached N)
  -> requireCurrentObservation=true
  -> reject: "The supplied diagnostics capabilities are not the current observation."
```

이 실패는 journal arm/`0x7D23`/`0x6060` 이전이다. mutation wire count는 0이다.

### 4.2 수정 순서

반드시 다음 순서를 사용한다.

```text
Phase 0  UI click / single canonical handler
Phase 1  RequireConnection + journal/admission/input validation
Phase 2  Admin capability refresh/check + selected-mode advertisement check
Phase 3  GetPhysicalAxis
Phase 4  VerifyAxisSetOperationModeTransitionPreflightAsync
         - LASAL AxisStatus
         - DS402 0x6041
         - DS402 0x6061
Phase 5  FINAL Diagnostics capability refresh
Phase 6  final Diagnostics identity/admission validation
Phase 7  PrepareSetOperationMode
Phase 8  durable journal ArmBeforeDispatch
Phase 9  currentAxis.SetOperationModeAsync(prepared) EXACTLY ONCE
Phase 10 accepted -> RecoverAxisSetOperationModeAsync (read-only outcome path)
```

**Hard invariant:** Phase 5와 Phase 7 사이에 `Diagnostics.GetCapabilities*()`, inline D5 read 또는 capability observation sequence를 증가시키는 helper를 호출하지 않는다.

### 4.3 final capability 변수는 별도 이름을 사용한다

preflight 이전 cached field와 혼동하지 않도록 local 이름을 명시한다.

권장:

```csharp
var finalDiagnosticCapabilities =
    await RefreshDiagnosticsCapabilitiesForSetOperationModeAsync(...);
```

`PrepareSetOperationMode()`에는 반드시 이 local final observation을 넘긴다.

기존 UI 표시용 `diagnosticCapabilities` field를 그대로 재사용할 경우에도 **final refresh 이후 field가 현재 observation임을 즉시 검증한 후 Prepare**해야 한다. 구현 가독성상 local final variable을 권장한다.

---

## 5. safety contract — 수정하면서 절대 완화하지 않을 것

이번 문제를 해결하기 위해 safety fence를 제거하지 않는다.

유지:

- `requireCurrentObservation=true`
- Diagnostics Build/BootId/MapRevision exact identity
- Admin capability triad
- PLC-advertised `SetOperationModeSupportedMask`
- one-shot operator confirmation
- valid nonzero timeout
- physical axis 1..4
- cross-mode `Standstill=True`
- DS402 Fault=False
- DS402 OperationEnabled=False
- durable journal must be armable before dispatch
- Start exactly once
- accepted/uncertain result에서 original `0x7D23` automatic replay 금지
- raw Generic SDO `0x6060` mutation 금지

OperationEnabled 상태에서 mode 변경을 허용하는 방식으로 문제를 우회하지 않는다.

---

## 6. same-target / cross-mode 의미 유지

### same-target

`0x6061 == requestedMode`

- preflight는 same-target임을 log한다.
- PLC lifecycle은 `SucceededNoWrite`로 끝날 수 있다.
- CSP -> CSP 성공은 실제 `0x6060` Write 증거가 아니다.

### cross-mode

`0x6061 != requestedMode`

- PP/PV/IP/CSP advertised target만 허용한다.
- preflight safety fence 통과 후 final Diagnostics refresh를 수행한다.
- Prepare/journal arm 성공 후에만 `0x7D23`을 한 번 전송할 수 있다.
- physical PASS는 최종 `0x6061 == requestedMode` evidence가 있어야 한다.

---

## 7. logging / breakpoint contract

구현 후 operator와 개발자가 call graph를 직접 확인할 수 있도록 phase log를 명확히 한다.

최소 log:

```text
SetOperationMode Start UI handler entered.
SetOperationMode preflight passed: axis=..., currentMode=..., requestedMode=..., StatusWord=...
SetOperationMode final Diagnostics refreshed: Build=..., BootId=..., MapRevision=..., Observation=...
SetOperationMode prepared: RequestId=..., ClientIntentId=...
SetOperationMode journal armed before dispatch: ...
SetOperationMode 0x7D23 dispatch boundary crossed once.
SetOperationMode Start ACK accepted once. This is not completion evidence.
```

실패 시 phase를 구분한다.

```text
PREPARE-BEFORE-WIRE FAILED
START-REJECTED-BEFORE-RETAINED-OUTCOME
START-OUTCOME-UNCERTAIN-NO-REPLAY
RECOVERY-QUERY
TERMINAL-OUTCOME
RETIRE
```

개발 시 우선 breakpoint:

1. `ButtonStartAxisSetOperationMode_Click`
2. `StartAxisSetOperationModeOnceAsync`
3. `VerifyAxisSetOperationModeTransitionPreflightAsync`
4. final Diagnostics refresh 직후
5. `PrepareSetOperationMode`
6. `LMCSingleAxis.SetOperationModeAsync`
7. `LMCAdmin.StartAxisSetOperationModeAsync`
8. mutation boundary / `connection.ExchangeAsync`

`ButtonStartAxisSetOperationModeWithRejectResolution_Click`에는 더 이상 breakpoint 위치가 존재하면 안 된다.

---

## 8. 구현 파일 범위

### 반드시 수정

#### WPF

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs`

- button event는 canonical `ButtonStartAxisSetOperationMode_Click` 하나만 등록
- canonical handler에 definitive rejection resolution 통합
- `StartAxisSetOperationModeOnceAsync()` ordering 수정
- final Diagnostics refresh -> Prepare 인접성 보장
- phase log 추가

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs`

- SetOperationMode handler detach/rebind 제거
- `ButtonStartAxisSetOperationModeWithRejectResolution_Click` 제거
- definitive rejection helper 자체는 recovery 파일로 이동하거나 현재 위치에 남길 수 있으나 UI event handler ownership은 남기지 않는다.

### SDK는 원칙적으로 semantic 변경 없음

`LMC_Library/LMC_API_Delivery/src/LmcAxisSetOperationMode.cs`

`LMC_Library/LMC_API_Delivery/src/LmcAdminSetOperationMode.cs`

- existing `Prepare -> SetOperationModeAsync -> StartAxisSetOperationModeAsync` contract 유지
- stale-current observation fence 유지
- regression instrumentation/test seam이 필요할 경우에만 최소 변경

### PLC 변경 없음이 기본

이번 defect는 host ordering과 handler ownership 문제이므로 PLC mode admission, `0x6060` write/verify lifecycle을 바꾸지 않는다.

실기 재검증에서 PLC-side evidence가 새롭게 나오기 전에는 PLC safety logic을 수정하지 않는다.

---

## 9. 필수 regression test

### R1 — single handler wiring

- button creation 후 Start Click subscription은 canonical handler 하나뿐이어야 한다.
- `InitializeReadOnlyApiUi()`가 handler를 detach/rebind하지 않아야 한다.
- source에 `ButtonStartAxisSetOperationModeWithRejectResolution_Click` symbol이 남아 있지 않아야 한다.
- button click 1회당 `StartAxisSetOperationModeOnceAsync()` 진입 1회.

### R2 — rejection resolution 보존

`LMCAxisSetOperationModeRejectedException` 발생 시:

- exact active recovery record 사용
- definitive rejection evidence persist
- active journal clear/reopen
- operator status 갱신
- original exception rethrow/log

기존 semantics와 동일해야 한다.

### R3 — stale observation 재현

1. Diagnostics observation N 획득
2. preflight inline D5 read로 current sequence 진행
3. old N으로 Prepare 시도
4. 기존 safety contract대로 zero-wire stale reject 확인

### R4 — final refresh 성공

1. preflight 완료
2. final Diagnostics observation M 획득
3. capability-producing call 없이 즉시 Prepare
4. Prepare 성공
5. journal arm 전 mutation wire 0회

### R5 — exactly-once dispatch

- prepared command 1개에 `SetOperationModeAsync()` dispatch 최대 1회
- accepted ACK 이후 recovery가 자동 Start replay하지 않음
- uncertain exception에서도 original Start replay 0회

### R6 — UI gate 회귀

Start enable 조건 유지:

```text
Connected
Idle
AdmissionAllowed
JournalReady
AdminTriad
DiagnosticsIdentity
Confirmed
TimeoutValid
AxisSelected
ModeSelected/Advertised
```

handler 정리를 위해 gate를 생략하지 않는다.

### R7 — build/test gate

최소:

- API Debug build
- API full test suite
- WPF Debug build
- SetOperationMode focused WPF tests
- WPF Release build
- API Release build
- `git diff --check`

Generic SDO focused tests도 같은 WPF/Diagnostics shared path 회귀가 없는지 확인한다.

---

## 10. 구현 완료 판정

### Software DONE

다음을 모두 만족해야 한다.

- `ButtonStartAxisSetOperationMode_Click()`이 실제 runtime의 유일한 Start Click handler
- obsolete `...WithRejectResolution_Click` 제거
- breakpoint로 canonical handler -> core -> Prepare -> SDK Start 경로 추적 가능
- preflight 후 final Diagnostics refresh 수행
- final refresh와 Prepare 사이 capability-producing call 0회
- stale observation regression PASS
- final current observation Prepare regression PASS
- exactly-once/no-replay regression PASS
- definitive rejection archival regression PASS
- Debug/Release build/test PASS

### Physical DONE와 구분

Software DONE은 physical mode-change PASS가 아니다.

Physical DONE에는 exact updated source/image에서 최소 다음 evidence가 필요하다.

| current | requested | 기대 |
|---|---|---|
| CSP | CSP | `SucceededNoWrite` 가능, `0x6060` dispatch 0회 |
| CSP | PP | `0x6060=1` 최대 1회, `0x6061=1` |
| CSP | PV | `0x6060=3` 최대 1회, `0x6061=3` |
| CSP | IP | `0x6060=7` 최대 1회, `0x6061=7` |
| PP/PV/IP | CSP | `0x6060=8` 최대 1회, `0x6061=8` |

실기 전에는 source SHA + LASAL generated artifact + PLC loaded image + WPF EXE/DLL identity를 같은 evidence set으로 기록한다.

---

## 11. 구현 순서

1. `MainWindow.ReadOnlyApi.cs`의 detach/rebind 제거
2. `ButtonStartAxisSetOperationModeWithRejectResolution_Click`의 rejection-resolution body를 canonical `ButtonStartAxisSetOperationMode_Click`으로 통합
3. obsolete handler 제거
4. `StartAxisSetOperationModeOnceAsync()`에서 preflight 이전 Diagnostics capability 사용 제거/제한
5. preflight 직후 final Diagnostics capability refresh 추가
6. final observation으로 `PrepareSetOperationMode()` 호출
7. phase logging 추가
8. R1~R6 regression 추가
9. API/WPF Debug/Release 검증
10. software DONE 후 exact build identity로 실기 matrix 수행

새 qualification branch를 장기간 만들지 않는다. `dev`에서 구현/검증하고, 작업 branch가 필요하면 merge 직후 삭제한다.

---

## 12. 금지되는 수정 방향

다음은 이번 defect의 해결책으로 인정하지 않는다.

- `requireCurrentObservation=false`로 바꾸기
- stale capability validation 제거
- preflight 삭제
- OperationEnabled mode change 허용
- journal arm 없이 Start 전송
- Start retry/replay 추가
- raw Generic SDO로 `0x6060` 우회
- UI에서만 버튼을 강제로 enable하고 admission을 건너뛰기
- `ButtonStartAxisSetOperationMode_Click`과 `...WithRejectResolution_Click` 두 handler를 동시에 유지

이번 corrective의 핵심은 **single UI entry + explicit phase ownership + preflight 뒤 final current observation + exactly-once mutation**이다.

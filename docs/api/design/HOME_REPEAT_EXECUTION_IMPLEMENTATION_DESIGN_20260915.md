# LMC_Home / DS402Home 반복 실행 구현 설계 - 2026-09-15

## 1. 문서 목적

이 문서는 `LMC_Home`과 `LMC_HomeDS402`를 한 번 수행한 뒤 동일 축에서 다시 Home을 수행할 수 있도록 만드는 구현 설계다.

기준 branch는 `dev`, 설계 기준 HEAD는 `b2022a0b2d91a01aa87416e2c21beeec58641fec` (`dev : LMC_Home Implementation`)이다.

현재 사용자 확인 상태는 다음과 같이 구분한다.

- `DS402 Home` 기본 동작: 사용자 실기 확인 완료
- `LMC_Home Direct`: 사용자 실기 확인 완료
- `LMC_Home`의 다른 Homing Mode: 사용자 실기 확인 완료
- `LMC_Home` 반복 실행: 미완료
- `DS402Home` 반복 실행: 미완료
- production release: **NO-GO**

이 문서는 구현 설계이며 LASAL IDE build, PLC download, 장비/실축 테스트 결과를 포함하지 않는다.

---

## 2. 현재 문제의 본질

현재 Home 계열은 단순한 `Start -> Done` 구조가 아니다.

두 기능 모두 mutation replay를 막기 위해 다음 lifecycle을 사용한다.

```text
Prepare
-> Start once
-> durable recovery record 유지
-> Outcome query
-> terminal 확인
-> exact Retire
-> local recovery journal Resolve
-> 다음 Start 허용
```

따라서 첫 Home이 물리적으로 끝났더라도 아래 중 하나가 남아 있으면 다음 Home은 의도적으로 차단된다.

1. PLC terminal record가 retire되지 않음
2. PC `MaintenanceActionRecoveryJournal`이 unresolved 상태로 남음
3. ownership이 release되지 않음
4. 이전 Home의 exact identity/generation retirement가 실패함

반복 실행 기능은 이 안전 계약을 제거해서 구현하면 안 된다. 핵심은 **이전 Home 한 건을 완전히 종료하고 새 generation의 Home을 시작할 수 있게 만드는 것**이다.

---

## 3. current source에서 확인된 비대칭

### 3.1 LMC_Home

현재 WPF `ButtonLmcHome_Click()`은 Start 성공 후 다음 흐름을 이미 실행한다.

```text
PrepareLMC_Home
-> LMC_HomeAsync
-> MonitorAndRetireLmcHomeAsync
-> ReadLMC_HomeOutcomeAsync
-> terminal이면 RetireLMC_HomeOutcomeAsync
-> MaintenanceActionRecoveryJournal.Resolve
```

또한 새 LMC Home 클릭 시 동일 action의 이전 recovery record가 남아 있으면 먼저 exact outcome을 읽고, unresolved 상태가 해소된 경우에만 새 Start로 넘어간다.

즉 LMC_Home 반복 실행 실패는 단순히 monitor가 없는 문제로 볼 수 없다.

현재 우선 조사 대상은 다음 네 단계다.

```text
0x7D18 terminal 확인
-> 0x7D19 retirement 성공 여부
-> journal Resolve 성공 여부
-> 다음 0x7D13 ownership/admission 성공 여부
```

### 3.2 DS402Home

현재 `ButtonDs402Home_Click()`은 다음까지만 수행한다.

```text
PrepareLMC_HomeDS402
-> LMC_HomeDS402Async
-> Start ACK 표시
```

Start 직후 자동 `Outcome polling -> Retire -> journal Resolve`가 없다.

따라서 첫 실행이 끝나도 durable recovery record가 남을 수 있고, 다음 클릭에서는 이전 outcome을 수동 recovery path로 처리해야 한다.

현재 `ReadExactDs402HomeOutcomeAsync()` 자체에는 다음 기능이 이미 존재한다.

```text
0x7D16 Outcome
-> terminal 확인
-> 0x7D17 Retire
-> terminal snapshot exact match 검증
-> MaintenanceActionRecoveryJournal.Resolve
```

그러므로 DS402Home은 새로운 protocol을 만드는 것이 아니라 LMC_Home과 같은 자동 completion monitor를 연결하는 것이 1차 수정이다.

---

## 4. 반복 실행의 목표 상태 머신

PC 측 Home 한 건의 lifecycle을 다음 상태로 통일한다.

```text
Idle
  |
  v
Prepared
  |
  v
JournalArmed
  |
  v
StartAccepted
  |
  v
Monitoring
  |
  +---- transport uncertain ----> RecoveryOnly
  |
  v
TerminalObserved
  |
  v
Retired
  |
  v
JournalResolved
  |
  v
ReadyForNextHome
```

핵심 규칙은 다음과 같다.

- `ReadyForNextHome`에 도달하기 전에는 새 Start를 보내지 않는다.
- `StartAccepted` 이후 통신 결과가 불명확해도 original Start를 자동 replay하지 않는다.
- `RecoveryOnly`에서는 동일 recovery key로 Outcome/Retire만 수행한다.
- terminal failure/abort도 exact retire가 끝나면 다음 수동 Home 요청은 허용한다.
- failure 이후 새 Home을 자동 retry하지 않는다.
- 새 Home은 항상 새 `Prepare`를 수행해 새 request/intent/generation을 사용한다.

---

## 5. DS402Home 수정 설계

### 5.1 WPF 자동 monitor 추가

`MainWindow.MaintenanceActions.cs`에 아래 메서드를 추가한다.

```csharp
private async Task MonitorAndRetireDs402HomeAsync(
    LMCSingleAxis currentAxis,
    MaintenanceActionRecoveryRecord recovery,
    int timeoutMilliseconds)
```

동작은 현재 `MonitorAndRetireLmcHomeAsync()`와 동일한 구조를 사용한다.

```text
deadline = Timeout + monitor margin
while recovery record active and deadline not expired
    200 ms delay
    ReadExactDs402HomeOutcomeAsync(axis, recovery)

if unresolved after deadline
    recovery record 유지
    Start replay 금지
    사용자에게 recovery pending 표시
```

`ButtonDs402Home_Click()`에서 `LMC_HomeDS402Async()` Start ACK를 받은 직후 다음 호출을 추가한다.

```csharp
await MonitorAndRetireDs402HomeAsync(
    currentAxis,
    recovery,
    parameters.TimeoutMilliseconds);
```

### 5.2 기존 exact retirement 재사용

`ReadExactDs402HomeOutcomeAsync()`의 current contract는 유지한다.

```text
ReadDs402HomeOutcomeAsync
-> outcome.IsTerminal 확인
-> RetireDs402HomeOutcomeAsync
-> terminal snapshot exact match
-> journal Resolve
```

새 구현에서 PLC state를 강제로 clear하거나 recovery record를 임의 폐기하지 않는다.

### 5.3 PLC DS402 record 계약 유지

current DS402 record state 정책은 반복 실행의 안전 기준으로 유지한다.

```text
0                  reusable / empty
1                  running
2                  terminal success
3                  terminal failure
4                  terminal cleanup/abort
0x00008002         retired success tombstone
0x00008003         retired failure tombstone
0x00008004         retired cleanup tombstone
```

새 Start는 `2/3/4` terminal record가 그대로 남아 있으면 거절되어야 한다.

`0x7D17 Retire`가 exact identity와 generation을 검증한 뒤 tombstone 상태로 바꿔야 다음 Start가 허용된다.

Retire request는 최소 다음 identity를 이전 Outcome과 exact 일치시킨다.

- DiagnosticsBuild
- DiagnosticsBootId
- MapRevision
- original RequestId
- ClientIntentId 4 DINT
- HomingMethod
- RecordGeneration

### 5.4 DS402 repeat 성공 조건

한 번의 DS402 Home이 반복 실행 가능한 상태로 끝났다고 판정하는 조건은 다음과 같다.

```text
terminal outcome 확보
AND
0x7D17 retirement confirmed
AND
retired snapshot == terminal snapshot
AND
MaintenanceActionRecoveryJournal inactive
AND
다음 Prepare 시 fresh request/intent 생성 가능
```

---

## 6. LMC_Home 수정 설계

LMC_Home은 이미 자동 monitor가 있으므로 lifecycle을 다시 작성하지 않는다.

반복 실행이 막히는 위치를 정확히 구분하고, terminal retirement 완료 후 다음 Start까지 상태가 닫히도록 보강한다.

### 6.1 단계별 진단 로그

다음 단계마다 명확한 로그를 남긴다.

```text
LMC_HOME_REPEAT PREVIOUS_RECOVERY
LMC_HOME_REPEAT OUTCOME_RUNNING
LMC_HOME_REPEAT OUTCOME_TERMINAL
LMC_HOME_REPEAT RETIRE_REQUEST
LMC_HOME_REPEAT RETIRE_CONFIRMED
LMC_HOME_REPEAT JOURNAL_RESOLVED
LMC_HOME_REPEAT PRESTART_READY
LMC_HOME_REPEAT START_ACCEPTED
LMC_HOME_REPEAT START_REJECTED
```

각 로그에는 가능한 범위에서 다음 값을 포함한다.

- AxisReference
- original RequestId
- DiagnosticsBuild / BootId / MapRevision
- ClientIntentId
- RecordGeneration
- terminal RecordState
- retirement confirmation
- current recovery journal active 여부
- Start rejection status/detailCode

이 로그로 두 번째 Home 실패를 아래 네 gate 중 하나로 즉시 분류할 수 있어야 한다.

1. previous outcome non-terminal
2. `0x7D19` retire failure
3. journal resolve failure
4. 새 `0x7D13` admission/ownership rejection

### 6.2 terminal -> retire -> resolve 순서 고정

현재 `ReadExactLmcHomeOutcomeAsync()`가 가진 순서를 유지한다.

```text
0x7D18 terminal
-> 0x7D19 Retire
-> terminal/retirement snapshot exact match
-> journal Resolve
```

`0x7D18 terminal`만 확인하고 journal을 먼저 Resolve하지 않는다.

또한 PLC terminal record를 PC가 임의로 삭제하거나 `ZeroHomeState`를 강제로 0으로 덮어쓰지 않는다.

### 6.3 ownership release 순서

PLC side의 Home completion은 다음 ordering을 만족해야 한다.

```text
native Home completion
-> cleanup/standstill 처리
-> Home owner release 완료
-> terminal outcome commit
-> Retire 허용
```

terminal outcome이 이미 반환됐는데 ownership이 계속 RESERVED라면 다음 Start가 막히므로 이 경우는 PLC lifecycle defect로 판정한다.

반복 실행을 위해 ownership table을 외부에서 force-clear하지 않는다.

### 6.4 다음 Start는 항상 새 generation

이전 `prepared` 객체나 recovery key를 재사용하지 않는다.

두 번째 Home은 반드시 다시 아래 순서를 탄다.

```text
Read current parameters
-> PrepareLMC_Home(...)
-> fresh RecoveryKey
-> journal ArmBeforeDispatch
-> LMC_HomeAsync(prepared)
```

첫 Home의 RequestId 또는 ClientIntentId를 재사용하지 않는다.

---

## 7. 공통 WPF 구조 정리

두 Home의 UI lifecycle을 가능한 한 동일하게 만든다.

권장 내부 helper는 다음 두 개다.

```csharp
CompletePendingHomeRecoveryAsync(...)
MonitorAndRetireDs402HomeAsync(...)
```

`CompletePendingHomeRecoveryAsync()`는 버튼 클릭 시작 시 현재 durable record가 동일 Home action이면 다음만 수행한다.

```text
exact Outcome query
-> terminal이면 exact Retire
-> journal Resolve
```

결과가 아직 Running이면 새 Start를 보내지 않고 종료한다.

다른 action의 unresolved record가 남아 있으면 기존 fail-closed 정책대로 차단한다.

### 7.1 UI 문구

현재 `Execute LMC Home Once`, `Execute DS402 Home Once`의 `Once`는 한 RPC가 one-shot이라는 의미이지만 사용자는 프로그램 lifetime에서 한 번만 실행되는 것으로 받아들일 수 있다.

반복 테스트 목적에는 다음 문구를 권장한다.

```text
Execute LMC Home
Execute DS402 Home
```

완료 후 상태는 다음처럼 표시한다.

```text
Home completed and retired. Ready for next Home.
```

### 7.2 confirmation 정책

`CheckHomeOneShotConfirmed`는 제거하지 않는다.

정책은 다음과 같이 해석한다.

- parameter 변경 시 confirmation 해제
- 동일 parameter를 그대로 반복 수행하는 동안 confirmation 유지 가능
- 매 실행은 사용자의 명시적 버튼 클릭이 필요
- 자동 반복/자동 retry는 금지

---

## 8. API / protocol 변경 범위

이번 반복 실행 수정은 public API 또는 wire command를 새로 만들 필요가 없다.

### LMC_Home

```text
PrepareLMC_Home
LMC_Home / LMC_HomeAsync
ReadLMC_HomeOutcome / Async
RetireLMC_HomeOutcome / Async

wire:
0x7D13 Start
0x7D18 Outcome
0x7D19 Retire
```

### DS402Home

```text
PrepareLMC_HomeDS402
LMC_HomeDS402 / LMC_HomeDS402Async
ReadDs402HomeOutcome / Async
RetireDs402HomeOutcome / Async

wire:
0x7D15 Start
0x7D16 Outcome
0x7D17 Retire
```

기존 exact identity/no-replay contract를 유지한다.

---

## 9. 수정 대상 파일

### 9.1 우선 수정

`LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs`

- DS402 자동 monitor 추가
- LMC/DS402 공통 pending recovery 처리 정리
- retirement/journal/pre-start 단계 로그 추가
- retirement 완료 후 UI를 repeat-ready 상태로 갱신
- 새 실행마다 fresh Prepare 보장

### 9.2 조건부 수정

`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st`

- `0x7D17`이 실제 terminal record를 retired tombstone으로 전환하지 못하는 경우에만 수정
- identity/generation validation은 약화하지 않음

`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st`

- `0x7D19` retirement 이후 active Home record가 새 Start를 계속 막는 경우에만 수정
- terminal commit 전에 owner release가 누락되는 경우 ordering 수정
- ownership force-clear 방식은 사용하지 않음

`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`

- 두 번째 Start의 ownership reservation 거절이 확인될 때만 detail logging/admission path를 수정

### 9.3 SDK 변경 원칙

`LMC_Library/LMC_API_Delivery/src/**`

현재 Start/Outcome/Retire API가 이미 있으므로 public surface 변경은 원칙적으로 하지 않는다.

retirement snapshot parser 또는 recovery key comparison defect가 확인되는 경우에만 최소 수정한다.

---

## 10. 구현 순서

### Phase 1 - DS402 WPF lifecycle 대칭화

1. `MonitorAndRetireDs402HomeAsync()` 추가
2. DS402 Start ACK 후 monitor 호출
3. terminal에서 기존 exact retire + journal resolve 재사용
4. timeout/transport uncertain 시 record 유지
5. next click에서 previous recovery completion 후 fresh Start 허용

### Phase 2 - LMC_Home repeat gate 식별

1. current `MonitorAndRetireLmcHomeAsync()` 유지
2. outcome/retire/journal/pre-start 단계 로그 추가
3. 두 번째 `0x7D13`이 실제 전송되는지 확인 가능하게 함
4. rejection 시 status/detailCode를 UI/log에 보존

### Phase 3 - PLC defect가 확인된 경우에만 수정

1. LMC `0x7D19` record retirement 상태 확인
2. DS402 `0x7D17` tombstone 상태 확인
3. terminal commit 전 ownership release 확인
4. exact generation 보존 확인
5. force-clear 없이 next Start admission 가능하게 수정

---

## 11. 반복 실행 acceptance criteria

사용자 실축 테스트 기준은 아래와 같다.

### LMC_Home

```text
Home #1 Start
-> terminal
-> Retire confirmed
-> Ready for next Home
-> Home #2 Start accepted
```

최소 확인 항목:

- Direct 2회 연속
- AbsoluteSwitch 2회 연속
- LimitSwitch 2회 연속
- ReferencePulse 2회 연속
- Block은 current 지원 정책에 따라 별도 판정

### DS402Home

```text
DS402 Home #1 Start
-> 0x7D16 terminal
-> 0x7D17 Retire confirmed
-> Ready for next Home
-> DS402 Home #2 Start accepted
```

최소 확인 항목:

- Method 37 2회 연속
- moving method는 사용자가 method별 qualification 시 반복 확인

### recovery case

- Start ACK 이후 통신 단절 시 original Start 자동 replay 없음
- reconnect 후 exact recovery key로 Outcome/Retire만 수행
- retirement 완료 후에만 새 Home 실행 가능

---

## 12. 금지 사항

반복 실행을 빠르게 만들기 위해 다음 방식은 사용하지 않는다.

- `ZeroHomeState` / `Ds402HomeState` terminal record 강제 0 clear
- ownership RESERVED 강제 해제
- unresolved recovery journal 무조건 삭제
- previous Start의 RequestId/intent 재사용
- timeout 시 Start 자동 재전송
- terminal proof 없이 다음 Home Start

이 방식은 현재의 no-replay 및 ownership 안전 경계를 깨므로 반복 실행 기능으로 인정하지 않는다.

---

## 13. 최종 설계 결론

이번 문제는 Home 알고리즘 자체의 재작성 문제가 아니다.

핵심은 각 Home 실행을 아래와 같이 완전한 transaction으로 닫는 것이다.

```text
Start once
-> terminal Outcome
-> exact Retire
-> owner/recovery release
-> fresh Prepare
-> next Start once
```

DS402Home은 current WPF의 자동 monitor/retire 누락을 우선 보완한다.

LMC_Home은 이미 자동 monitor가 있으므로 `0x7D19 Retire`, journal Resolve, ownership release, 두 번째 `0x7D13` admission 중 어느 단계가 막히는지를 식별 가능하게 만든 뒤, 실제 defect가 확인된 계층만 수정한다.

이 구조를 지키면 Home을 여러 번 반복 테스트할 수 있으면서도 기존 durable recovery와 no-replay 안전 계약을 유지할 수 있다.

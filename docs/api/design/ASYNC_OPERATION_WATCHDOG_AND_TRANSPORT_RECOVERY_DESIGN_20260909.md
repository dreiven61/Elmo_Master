# Async operation watchdog and transport recovery design

- 작성일: 2026-09-09
- 대상: `LasalMotionControlApiExample` WPF command orchestration과 `LasalMotionControlLib` RPC transport
- 상태: A0/A1/A2 공통 gate-owner fence 구현 / A3 명령별 SDK 확대와 A4 자동 reconnect 보류
- production posture: **NO-GO until PC fault-injection and PLC/physical qualification complete**

## 1. 결론

현재 WPF의 공통 명령 실행 경로에는 무기한 대기가 존재한다.

- `RunOperationAsync`는 `action()`이 끝날 때까지 `operationRunning`을 해제하지 않는다.
- `RunSafetyCommandAsync`, `SendLiveCommandAsync`, `SendSerializedCommandAsync`는
  deadline 없는 `commandSendGate.WaitAsync()`를 사용한다.
- 명령 송신 delegate가 반환하지 않으면 gate도 해제되지 않는다.
- 다수 UI handler는 SDK에 `CancellationToken.None`을 전달한다.

따라서 단순히 UI에서 `Task.WhenAny`로 기다림만 중단해서는 안 된다. RPC write가 시작된 뒤
기존 TCP를 유지한 채 gate를 풀면 늦게 도착한 이전 응답을 다음 명령의 응답으로 잘못 해석할 수 있다.

해결 원칙은 다음 네 가지다.

1. 모든 대기를 bounded deadline으로 제한한다.
2. `wire write 전` 취소와 `wire write 후` timeout을 구분한다.
3. write 후 결과가 불명확하면 해당 TCP session을 폐기하고 재사용하지 않는다.
4. mutation은 자동 재전송하지 않으며 Stop/Power Off만 명시적인 safety takeover 절차로 실행한다.

## 2. 현재 구현에서 확인한 사실

### 2.1 WPF 공통 경로

대상:

- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs`
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs`

확인 사항:

- `RunOperationAsync`의 `finally`는 `action()`이 반환하거나 예외를 던져야 실행된다.
- 공통 command gate 획득에는 취소 token이나 timeout이 없다.
- safety command도 같은 gate 뒤에서 기다리므로 일반 RPC가 멈추면 safety 우선순위 예약만 되고
  실제 송신은 시작하지 못할 수 있다.
- 일부 qualification 경로는 이미 cancellable 또는 timed gate wait를 사용하지만 일반 운전 UI에는
  일관되게 적용되지 않았다.

### 2.2 SDK transport

대상:

- `LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs`

확인 사항:

- 기본 Connect/Receive/Send timeout은 각각 3000 ms다.
- `ExchangeAsync`는 blocking socket exchange를 `Task.Run`에서 실행한다.
- `ExchangeAsyncDrainAfterWrite`는 pre-write cancellation과 post-write deadline을 분리한다.
- post-write deadline에서는 transport를 invalidation하고 `LMCPostWriteDeadlineException`을 낸다.
- `AbortTransportForSafetyPreemption(expectedSessionGeneration)`이라는 production escape hatch가
  이미 존재한다.
- Axis Stop recovery의 일부 경로는 session-pinned abort, reconnect, exact axis reload를 이미 사용한다.

즉 transport primitive는 상당 부분 준비돼 있지만 WPF 공통 scheduler가 그 계약을 모든 명령에
적용하지 않는 것이 핵심 결함이다.

## 3. 절대 지켜야 할 불변 조건

| ID | 조건 |
|---|---|
| A1 | UI operation과 command gate는 무기한 대기하지 않는다. |
| A2 | write 전 취소는 zero-wire로 끝나야 한다. |
| A3 | write가 시작됐을 수 있는 mutation은 timeout 후 자동 재전송하지 않는다. |
| A4 | post-write timeout이 발생한 TCP session은 다음 RPC에 재사용하지 않는다. |
| A5 | 늦게 끝난 old task는 새 session/UI/journal 상태를 갱신하지 못한다. |
| A6 | Stop/Power Off는 일반 명령의 무기한 gate 대기에 종속되지 않는다. |
| A7 | ACK는 접수 증거일 뿐 stable state 또는 물리 동작 완료 증거가 아니다. |
| A8 | reconnect 뒤 axis/group identity를 다시 lookup하기 전 명령을 보내지 않는다. |

## 4. 명령 상태 모델

```text
Created
  -> WaitingForGate
  -> PreWriteValidated
  -> WriteMayHaveStarted
  -> ResponseAccepted
  -> AwaitingStableProof
  -> Completed
```

종료/이탈 상태:

```text
WaitingForGate/PreWriteValidated -> CancelledZeroWire
WriteMayHaveStarted              -> OutcomeUncertainTransportRetired
ResponseAccepted                 -> AcceptedAwaitingProof
AwaitingStableProof              -> ProofTimeoutStatusOnlyRecovery
any state                        -> Rejected / Faulted / SupersededBySafety
```

`WriteMayHaveStarted` 이후에는 일반 `Cancelled`로 표현하지 않는다. 실제 PLC 수신 여부를 알 수
없으므로 반드시 `OutcomeUncertain` 또는 accepted continuation으로 기록한다.

## 5. 제안 구조

### 5.1 `UiOperationSupervisor`

WPF의 `operationRunning`, `safetyCommandRunning`, `commandSendGate` 사용을 한 곳에서 관리한다.

필수 입력:

- operation name/kind
- read-only, mutation, safety 구분
- gate deadline
- total ACK deadline
- stable-proof deadline
- expected connection session generation
- cancellation token

필수 출력:

- 현재 stage와 elapsed time
- wire 시작 가능성
- ACK/continuation 유무
- transport invalidation 여부
- UI에 적용 가능한 current operation generation

각 operation에 증가하는 UI generation을 부여한다. await가 늦게 끝나더라도 generation이 현재와
다르면 log evidence만 남기고 버튼, loaded object, journal, 최근 결과를 변경하지 않는다.

### 5.2 deadline 계층

초기 기본값은 설정 객체로 두고 테스트 결과로 조정한다.

| 구간 | 초기 권장값 | 만료 시 처리 |
|---|---:|---|
| 일반 command gate 획득 | 1000 ms | zero-wire Busy/Timeout |
| safety gate grace | 250 ms | safety takeover 검토/수행 |
| TCP connect | 현재 3000 ms | 연결 실패 |
| TCP send/receive | 현재 3000 ms | transport fault |
| 단일 ACK total | 5000 ms | pre/post-write evidence에 따라 분기 |
| stable-state proof | 명령별 5~60 s | continuation/status-only recovery |
| UI hard watchdog | SDK total deadline + 1000 ms | SDK task fencing 후 recovery UI 전환 |

UI hard watchdog은 SDK deadline의 대체물이 아니다. SDK가 deadline을 자체 소유하고 transport를
정리하는 것이 원칙이며, UI watchdog은 잘못 구현되거나 반환하지 않는 SDK task로부터 UI를
보호하는 마지막 경계다.

### 5.3 일반 명령 처리

1. cancellable timed wait로 command gate를 획득한다.
2. gate 획득 뒤 session/generation/identity를 최종 확인한다.
3. SDK에 pre-write token과 post-write deadline을 전달한다.
4. pre-write cancel이면 zero-wire로 종료하고 gate를 해제한다.
5. post-write deadline이면 old transport가 Faulted/retired인지 확인하고 `OutcomeUncertain`을 기록한다.
6. mutation은 재전송하지 않는다. read-only만 새 연결에서 명시적으로 다시 읽을 수 있다.
7. task 완료/실패와 무관하게 UI generation fence를 확인한 뒤 화면을 갱신한다.

### 5.4 Stop/Power Off safety takeover

1. 버튼 입력 시 safety generation을 즉시 예약한다.
2. 일반 command에 250 ms의 gate 양보 시간을 준다.
3. gate를 얻으면 기존 priority scope에서 Stop/Power Off를 1회 전송한다.
4. gate를 얻지 못했고 in-flight RPC의 exact session generation을 증명할 수 있으면
   `AbortTransportForSafetyPreemption(expectedSessionGeneration)`을 호출한다.
5. old connection을 detach/dispose하고 fresh TCP로 reconnect한다.
6. endpoint, BootId, MapRevision, axis/group name/reference를 다시 확인한다.
7. safety command만 정확히 1회 보낸다.
8. 중단된 일반 mutation은 자동 replay하지 않고 `SupersededBySafety` 또는
   `OutcomeUncertain` evidence로 남긴다.

session generation을 증명하지 못하거나 identity가 달라지면 safety command도 추정 송신하지
않고 recovery-required로 종료한다.

### 5.5 UI 동작

- 일반 명령 중에도 Stop/Power Off는 활성 상태를 유지한다.
- Cancel 버튼은 write 전에는 `Cancel Before Send`, write 후에는
  `Abort Transport / Result Unknown`으로 의미와 문구를 바꾼다.
- 화면에는 operation, stage, elapsed, session generation, wire-may-have-started를 표시한다.
- timeout 후 전체 앱을 계속 Busy로 두지 않는다. loaded handle을 폐기하고 Reconnect/Read-only
  Recovery 상태로 전환한다.
- 창 닫기는 정상 RPC Close를 무기한 기다리지 않는다. bounded graceful close 뒤 local transport
  dispose를 수행하되 unresolved mutation record는 삭제하지 않는다.

## 6. 명령 종류별 재시도 정책

| 종류 | pre-write 실패 | post-write 불확실 | accepted 후 proof 실패 |
|---|---|---|---|
| Read-only | 재시도 가능 | reconnect 후 재시도 가능 | 해당 없음 |
| Motion/Power/Reset/Home mutation | 명시적 재시도 가능 | 자동 재시도 금지 | status/terminal read-only resume만 허용 |
| SDO/SetPosition 등 durable mutation | 새 ticket 판단 가능 | durable recovery만 허용 | exact outcome/query/retire 절차 사용 |
| Stop/Power Off | 명시적 재시도 가능 | 새 safety takeover는 별도 사용자/정책 결정 | stable-state status-only resume |

## 7. 구현 순서

### A0 - 관측성 고정

- 모든 WPF command log에 operation id, UI generation, connection session generation,
  gate wait, write boundary, deadline kind를 추가한다.
- 현재 기본 timeout 값과 실제 적용 값을 startup log에 기록한다.

### A1 - 무기한 gate 제거

- WPF의 인자 없는 `commandSendGate.WaitAsync()`를 모두 cancellable timed wait helper로 교체한다.
- gate timeout은 zero-wire evidence로 처리한다.
- 이 단계에서는 safety abort를 자동 수행하지 않고 문제를 bounded failure로 바꾼다.

### A2 - 공통 supervisor와 UI generation fence

- `RunOperationAsync`, `RunSafetyCommandAsync`, send helper를 supervisor로 통합한다.
- Cancel/Abort UI와 late completion 무효화를 구현한다.

### A3 - SDK post-write deadline 적용 확대

- 모든 state-changing async facade가 `ExchangeAsyncDrainAfterWrite` 또는 동등한 계약을 사용하게 한다.
- post-write timeout 시 connection Faulted와 typed evidence를 검증한다.

### A4 - safety takeover 일반화

- 현재 Axis Stop 전용 abort/reconnect/identity reload 흐름을 Axis/Group Stop과 Power Off의 공통
  component로 추출한다.
- 중단된 ordinary mutation 무재전송을 durable recovery와 연결한다.

### A5 - close/reconnect 운용 정리

- window close, manual disconnect, reconnect를 bounded lifecycle로 통합한다.
- old task와 callback이 새 connection 상태를 변경하지 못하도록 lifetime fence를 검증한다.

## 8. 필수 자동 시험

fake RPC가 아래 위치에서 영구 정지하도록 fault injection한다.

1. gate 획득 전
2. write 직전
3. request 일부/전체 write 후 ACK 전
4. ACK 뒤 첫 status 전
5. status polling 중
6. graceful Close 응답 전

각 시험은 다음을 확인한다.

- UI dispatcher가 응답하며 지정 deadline 뒤 버튼 상태가 복구된다.
- semaphore count가 1을 초과하지 않고 영구 점유되지 않는다.
- pre-write cancel은 request count 0이다.
- post-write timeout 뒤 같은 TCP에 다음 request가 0이다.
- ordinary mutation replay count는 0이다.
- 늦은 old response/task가 새 UI/session/journal을 변경하지 않는다.
- safety takeover는 old transport 폐기 후 새 session에서 Stop/Power Off만 1회 전송한다.
- identity mismatch에서는 safety mutation이 zero-wire다.

## 9. 검증 단계와 판정

| 단계 | 증거 | 판정 범위 |
|---|---|---|
| ASYNC-C0 | source review, static verifier | 무기한 wait 제거와 불변 조건 정합 |
| ASYNC-C1 | Debug/Release build | PC compile |
| ASYNC-C2 | fake-RPC hang/delay/late-response tests | PC scheduler/transport 계약 |
| ASYNC-C3 | 실제 WPF 강제 network loss/reconnect | PC process recovery |
| ASYNC-C4 | LASAL C78 build/download | PLC image 정합 |
| ASYNC-C5 | PLC Watch + packet capture | request count, session, no-replay |
| ASYNC-C6 | 실제 Axis/Group Stop/Power Off | 물리 안전 동작 |

`ASYNC-C0..C3 PASS`를 PLC 또는 물리 안전 PASS로 표현하지 않는다. 최종 운전 적용은
`ASYNC-C4..C6`까지 별도로 확인해야 한다.

## 10. 금지 사항

- `Task.WhenAny`에서 timeout된 뒤 기존 task를 방치하고 gate만 해제
- post-write timeout 후 같은 TCP session 재사용
- 앱 응답성을 이유로 mutation 자동 replay
- 모든 exception을 단순 `Operation failed`로 축약
- Stop/Power Off를 일반 operation 완료까지 무기한 disable
- timeout 시 retained/durable recovery record 수동 삭제

## 11. 2026-09-09 구현 반영

반영 완료:

- 일반 WPF command gate 대기를 1000 ms로 제한했다.
- Stop/Power Off safety gate grace를 250 ms로 제한했다.
- gate owner의 exact connection/session generation을 기록한다.
- safety grace 만료 시 owner identity가 정확히 일치할 때만
  `AbortTransportForSafetyPreemption`으로 old TCP를 폐기한다.
- abort된 ordinary operation의 UI generation을 fence하고 loaded handle을 폐기한다.
- safety 명령은 old transport 폐기와 같은 동작에서 자동 전송하지 않는다. 로그와 오류는
  reconnect, exact identity 재조회, safety 명령 1회 실행을 요구한다.
- motion uncertainty의 final identity gate도 bounded wait로 변경했다.
- gate timeout/cancellation 최소 smoke를 추가했다.

의도적으로 보류:

- 모든 mutation facade에 대한 SDK post-write deadline 확대는 명령별 accepted-continuation과
  durable journal 정합 검토가 필요하므로 한 번에 일괄 변경하지 않았다.
- 공통 자동 reconnect 후 safety 명령 전송은 closure가 old axis/group handle을 캡처하는 현재
  구조에서는 안전하지 않다. Axis Stop의 기존 exact takeover처럼 각 safety command가 fresh
  handle과 durable predecessor를 인자로 받도록 바꾼 뒤 적용해야 한다.

현재 동작은 시스템을 무기한 기다리게 두는 대신 bounded failure와 명시적 reconnect로 전환한다.
이는 자동 safety completion이 아니라 UI/transport recovery tranche다.

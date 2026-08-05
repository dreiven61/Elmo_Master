# Axis Stop Stable Standstill 계약

날짜: 2026-07-29

## 결론

SDK에 `StopAndWaitForStableStandstillAsync`를 추가했다. 이 API는 Axis Stop `0x2022`를 정확히
한 번만 전송하고 valid success ACK 뒤 Axis Read Status `0x2028`만 반복한다. 기본 완료 조건은
`status.IsSuccess && status.IsStandstill` 3회 연속이다.

이 구현은 source-only SDK facade다. LASAL handler/protocol은 기존 local Stop validation과 status
응답을 그대로 사용하며 변경하지 않았다. WPF는 Begin을 priority safety-send 단계에, Resume을
preemptible status-monitor 단계에 연결했다. accepted Stop monitor 중 동일 Stop 버튼 replay는
차단하고 더 높은 우선순위의 Power Off만 예약할 수 있다.

## Public API

```csharp
LMCAxisStopWaitContinuation PendingStopWaitContinuation { get; }

Task<LMCAxisStopWaitContinuation> BeginStopWaitForStableStandstillAsync(
    int deceleration,
    int jerk,
    CancellationToken cancellationToken);

Task<LMCAxisStopWaitResult> ResumeStopWaitForStableStandstillAsync(
    LMCAxisStopWaitContinuation continuation,
    CancellationToken cancellationToken);

Task<LMCAxisStopWaitResult> StopAndWaitForStableStandstillAsync(
    int deceleration,
    int jerk,
    CancellationToken cancellationToken);

Task<LMCAxisStopWaitResult> StopAndWaitForStableStandstillAsync(
    int deceleration,
    int jerk,
    LMCAxisStopWaitOptions options,
    CancellationToken cancellationToken);
```

기본 옵션은 total deadline 5000 ms, poll interval 50 ms, stable sample 3이다. Stop 입력은 기존
계약과 동일하게 `deceleration > 0`, `jerk >= 0`이어야 한다. 전체 request frame을 gate/wire 전에
생성하므로 잘못된 입력은 zero-wire로 거부된다. options를 받는 Begin/Resume overload도 제공한다.
간편 Resume overload는 continuation의 `RequiredStableSampleCount`를 상속하므로 custom sample 수를
기본값 3으로 덮어쓰지 않는다.

## Wire와 완료 순서

1. 옵션과 Stop motion parameter를 로컬 검증한다.
2. Begin이 axis mutation gate를 획득한다.
3. `0x2022`를 정확히 한 번 전송하고 may-have-been-sent boundary에서 process-local axis
   mutation generation을 기록한다.
4. valid success ACK를 session/send-priority publication 뒤 `Accepted`로 확정하고, 같은 gate 안에서
   session/axis-bound continuation을 latest pending으로 게시한다. 새 accepted Stop은 이전 pending
   Stop continuation을 supersede한다.
5. Begin이 mutation gate를 해제한다.
6. Resume이 axis status-observation gate를 획득하고 원 Stop mutation generation을 확인한 뒤
   `0x2028`만 반복해
   `IsSuccess && IsStandstill`을 연속 확인한다. Resume은 `0x2022`를 보내지 않는다.
7. 비정지 상태, native AxisError 또는 다른 predicate mismatch가 나오면 stable count를 0으로
   초기화한다.
8. 필요한 연속 sample 수에 도달하면 continuation을 완료하고 immutable result를 반환한다.

ACK는 Stop 접수 증거일 뿐 완료 증거가 아니다. timeout, cancel, response loss, status failure와
send-priority result discard가 발생해도 이 호출은 `0x2022`를 자동 replay하지 않는다.

## Evidence

`LMCAxisStopWaitEvidence`는 다음을 보존한다.

- 요청한 `Deceleration`, `Jerk`
- `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted` submission outcome
- `CommandMayHaveBeenSent`
- Stop ACK
- 마지막으로 publication된 Axis status
- status poll count
- 현재/필요 stable standstill sample count
- `StopMutationGeneration / ObservedMutationGeneration`
- `InterveningMutationDetected`
- total elapsed milliseconds

pre-wire cancel은 `NotAttempted`와 zero Stop wire를 보존한다. write boundary 뒤 ACK response가
유실되거나 stale priority generation으로 결과가 폐기되면 `OutcomeUncertain`이다. valid success
ACK 뒤 timeout/cancel/status failure는 `Accepted` ACK와 관찰된 status evidence를 유지한다.

`TimeoutMilliseconds`는 status gate 대기, Stop ACK, 각 status 응답과 poll delay를 모두 포함하는
total deadline이다. RPC write boundary 뒤 PLC 응답이 deadline까지 오지 않으면 stream alignment를
추정으로 재사용하지 않고 해당 connection을 `Faulted`로 전환한다. 이 경우
`TransportInvalidatedAtDeadline=true`를 보존하며 reconnect 전에는 다음 명령을 보낼 수 없다.
Stop ACK 무응답이면 submission은 `OutcomeUncertain`, accepted Stop 뒤 status 무응답이면
`Accepted`를 유지한다.

## 명시적 경계

- 완료 predicate는 LASAL `0x2028`의 native axis status다. current `StatusWord` slot은 reserved 0이며
  DS402 `0x6041` 또는 물리적 정지의 독립 증거가 아니다.
- SDK는 connection session과 `AxisReference`로 범위를 나눈 process-local axis mutation
  generation을 공유한다. `LMCSingleAxis` raw sync/async Power On/Off, Reset, Stop, Move
  Absolute/Relative/Velocity와 accepted-wait write는 may-have-been-sent boundary에서 generation을
  증가시킨다. validation/선점으로 zero-wire인 호출은 증가시키지 않고 다른 AxisReference의
  mutation도 간섭하지 않는다.
- Resume은 status 송신 전, status publication과 final resolution에서 generation을 재검사한다.
  later same-axis mutation은 `LMCAxisStopInterferenceException`으로 끝나고 원 continuation은
  pending으로 남으며 `0x2022`는 replay되지 않는다. 이 귀속 범위에는 외부 PLC logic, 다른 RPC
  client, direct SDO write와 group operation이 포함되지 않는다.
- compound API는 같은 elapsed deadline으로 Begin과 Resume을 조합한다. WPF는 compound를 safety
  send gate 안에서 호출하지 않고 Begin/Resume을 분리한다. accepted Stop status monitor 중에는
  동일 Stop 재클릭을 막고 Power Off만 기존 monitor를 선점하도록 한다.
- timeout/cancel/status/preemption 뒤 accepted continuation은 pending으로 남는다. stale session,
  superseded, completed continuation과 concurrent second Resume은 wire 전에 거부한다. ACK 게시 직후
  Begin deadline 경계에서 발생한 typed exception도 continuation을 보존하며 WPF는 이를 회수해
  `0x2028` status-only monitor로 이어간다.
- status sample은 기존 pending Power On continuation에도 전달된다. PowerOff/Standstill proof가
  누적되어도 Stop helper는 pending Power On을 자동 해제하지 않는다.
- 실제 PLC build/download, Elmo drive 동작, packet capture와 물리 정지 시간은 아직 검증하지 않았다.

## Durable WPF 복구와 Reset takeover

WPF는 Stop을 보내기 전에 endpoint, axis name/reference, diagnostics BootId/MapRevision,
deceleration/jerk와 stable sample 수를 `AxisCommandRecoveryJournal`에
`ArmedBeforeDispatch`로 기록한다. success ACK는 accepted observer에서 첫 status보다 먼저
`AcceptedAwaitingProof`로 기록한다. 같은 process에서 observer 저장이 실패해도 SDK의 exact pending
continuation을 회수해 `0x2022`를 재전송하지 않고 status-only monitor를 계속한다. process가
accepted 기록 뒤 종료되면 새 process는 exact endpoint/D0/axis identity를 다시 확인하고
`0x2028`만 3회 이상 읽는다. 마지막 stable sample 뒤에도 D0를 live refresh한 후에만 journal을
`Resolved`로 바꾼다.

active Reset 중 Stop을 누르면 Reset record를 Stop `ArmedBeforeDispatch` record로 원자 교체한 뒤,
Reset continuation의 session generation에 고정해 기존 transport를 local abort한다. 이 abort는 RPC
Close를 보내지 않으며 concurrent normal Close가 held RPC 뒤에서 기다리는 경우에도 lifecycle lock에
막히지 않는다. 새 `LMCConnection` 객체에서 RPC init, D0, axis lookup/info와 active Motion journal
identity를 모두 확인한 뒤 `0x2022`를 한 번만 보낸다. 이전 객체의 늦은 state/result event는 새
Stop journal이나 UI를 변경할 수 없다.

- abort/session/identity/lookup 또는 Stop pre-write 실패: Stop wire 0, exact predecessor Reset 복원
- structurally valid Stop NACK: Stop wire 1과 mutation reservation rollback. predecessor Reset이 아직
  pending이면 exact Reset을 복원하고, 이미 완료됐으면 Reset을 다시 활성화하지 않고 Stop tombstone을 resolve
- Stop write 뒤 response loss: Stop wire 1, Stop `RecoveryRequired`, Reset 미복원, 자동 replay 금지
- Reset continuation이 abort 전 또는 fresh reconnect 뒤 먼저 완료돼도 Reset clear를 Standstill로
  간주하지 않는다. 이미 arm한 동일 Stop identity를 유지하고 `0x2022` 1회와 stable status proof를 계속한다.
- abort session mismatch: 현재 transport를 끊지 않고 stale Reset continuation을 폐기한 뒤 durable
  Reset을 cross-session status-only 복구 대상으로 복원
- Motion과 accepted Stop record가 함께 있으면 exact identity를 결합해 Motion record를 먼저
  resolve하고 Stop record를 나중에 resolve한다. 두 resolve 사이에 process가 종료돼도 다음 실행은
  Stop status-only proof만 반복한다.

`ArmedBeforeDispatch` 상태에서 process가 종료되면 ACK 여부를 알 수 없으므로 자동 Stop replay나
status-only 완료를 주장하지 않고 `RecoveryRequired`로 남긴다. PLC transaction ID가 없는 현재
protocol에서는 wire 전후 crash를 완전히 구분할 수 없다.

## 자동 검증

2026-07-29 당시 Release에서 SDK 전체 `974/974 PASS`를 확인했다. 이 수치는 historical checkpoint이며
current 전체 수치는 status 문서를 따른다. Stop/Reset 및 transport safety 회귀는 다음을
고정한다.

- one Stop + three stable standstill status
- non-standstill과 AxisError 각각의 stable counter reset
- rejected ACK 뒤 status zero-wire
- pre-wire cancel의 `NotAttempted`/zero-wire
- 최종 pre-write hook에서 취소된 commit-window의 `NotAttempted`/zero-wire와 connection 재사용
- accepted ACK 뒤 timeout/cancel evidence
- Stop ACK 무응답 deadline의 `OutcomeUncertain`, transport invalidation과 no-replay
- accepted Stop 뒤 status 무응답 deadline의 `Accepted`, transport invalidation과 no-replay
- unsuccessful parsed status의 typed failure
- accepted ACK 뒤 status response loss의 `Accepted` evidence와 no-replay
- accepted ACK 뒤 status publication discard의 `Accepted` evidence와 no-replay
- Stop ACK response loss의 `OutcomeUncertain`과 no-replay
- send-priority result discard의 `OutcomeUncertain`
- pending Power On proof 관찰과 자동 해제 금지
- Begin 1회 뒤 Resume status-only 완료와 timeout 뒤 no-replay 재개
- 새 accepted Stop의 이전 continuation supersede
- status poll 선점 뒤 새 priority Stop 완료
- concurrent Begin의 wire/continuation publication 순서와 concurrent Resume second zero-wire
- reconnect 뒤 stale continuation zero-wire
- Resume epoch 실패 시 Stop 및 pending Power On stable proof counter 초기화
- custom `StableSampleCount=5` Begin 뒤 간편 Resume의 `0x2022` 1회/`0x2028` 5회
- ACK 게시 직후 Begin deadline exception의 pending continuation 보존과 status-only 재개
- 같은 AxisReference의 다른 handle에서 보낸 Move 및 accepted-wait mutation의 typed interference,
  status publication race 폐기와 pending continuation 보존
- validation/선점 zero-wire mutation의 generation 불변과 다른 AxisReference mutation의 비간섭

같은 2026-07-29 checkpoint의 WPF Release build는 warning/error 0/0, actual-control smoke는
`206/206 PASS`다. Axis Stop/Reset
통합 회귀는 `18/18`, journal은 `9/9`, 실제 child-process recovery는 `4/4`를 통과했다. delayed
Stop status poll을 명시적 priority Power Off가 선점한 뒤에도 Stop `0x2022`는 1회뿐이며,
Power Off `0x2023` 1회와 후속 safe-state `0x2028` 3회가 완료되는 경로를 고정한다. ACK 뒤 durable
MarkAccepted 직전 process Kill은 동일 identity의 `ArmedBeforeDispatch -> RecoveryRequired`, command
총 1회, restart command/status replay 0회를 확인한다. completed Reset을 대체한 Stop이 valid NACK면
final D0의 endpoint/axis/reference/BootId/MapRevision까지 일치해야만 tombstone을 resolve하고,
mismatch에서는 Stop과 predecessor identity를 `RecoveryRequired`로 보존한다. 이는 PC/fake-RPC 증거이며
실제 축의 정지 성능이나 safety certification 증거가 아니다.

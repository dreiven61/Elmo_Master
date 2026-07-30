# Axis Reset stable error-clearance contract

## 1. 구현 범위

Axis Reset completion은 accepted-once Begin/Resume과 이를 조합한 compound facade로 제공한다.
`ResetAndWaitForStableErrorClearanceAsync(...)`는 아래 두 단계를 같은 total elapsed deadline으로
조합한다.

1. total deadline, poll interval과 stable sample count를 검증한다.
2. `BeginResetWaitForStableErrorClearanceAsync(...)`가 Axis Reset `0x2024`를 정확히 한 번
   송신하고 status는 읽지 않는다.
3. valid success ACK, latest pending continuation과 Reset mutation generation을 connection
   session/send-priority publication 안에서 원자적으로 게시한다.
4. `ResumeResetWaitForStableErrorClearanceAsync(...)`가 `0x2028`만 반복하며 `0x2024`를
   재전송하지 않는다.
5. `IsReadSuccessful && AxisErrorId == 0`을 기본 3회 연속 관찰하면 완료한다.

`AxisErrorId != 0`은 정상적으로 파싱된 "아직 오류가 남은 상태"이므로 stable count를 0으로
되돌리고 polling을 계속한다. function/transport status가 실패하거나 response를 파싱할 수
없으면 `LMCAxisResetStatusException`으로 종료한다.

각 Resume epoch 시작과 미완료 종료에서는 stable clear count를 0으로 되돌리지만 누적 status
poll count와 마지막 status evidence는 유지한다. Reset ACK는 `QuitError()` 호출 수락일 뿐 오류
해제 완료가 아니다. timeout, cancellation, response loss, send-priority 선점 또는 status 실패
뒤에도 `0x2024`를 자동 재전송하지 않는다.

invalid, foreign, stale-session, superseded, completed continuation과 concurrent second Resume은
wire 전에 거부된다. 새 accepted Reset만 이전 pending Reset을 supersede한다.

## 2. API evidence

`LMCAxisResetWaitEvidence`는 다음 값을 immutable evidence로 보존한다.

- `SubmissionOutcome`: `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`
- `CommandMayHaveBeenSent`
- `ResetAcknowledgement`
- `LastObservedStatus`
- `StatusPollCount`
- `StableErrorClearSampleCount / RequiredStableSampleCount`
- `ResetMutationGeneration / ObservedMutationGeneration`
- `InterveningMutationDetected`
- `TransportInvalidatedAtDeadline`
- `ElapsedMilliseconds`

write boundary 뒤 response가 유실되거나 accepted result publication이 safety generation에 의해
폐기되면 `OutcomeUncertain`이다. ACK 뒤 timeout/cancel은 `Accepted` ACK와 마지막 status를
보존한다. pre-wire timeout/cancel은 `NotAttempted`다.

total deadline은 gate 대기, Reset ACK, 각 status 응답과 poll delay를 모두 포함한다. write
boundary 뒤 ACK 또는 status 응답이 deadline까지 오지 않으면 stream alignment를 추정으로
재사용하지 않고 connection을 `Faulted`로 전환한다. ACK 무응답은 `OutcomeUncertain`, accepted
Reset 뒤 status 무응답은 `Accepted` evidence를 유지하고 두 경우 모두
`TransportInvalidatedAtDeadline=true`와 reconnect 필요성을 표시한다. 최종 pre-write hook에서
취소되면 `NotAttempted`, zero Reset wire이며 기존 connection을 재사용할 수 있다.

final status는 session, send-priority generation, Reset mutation generation과 deadline을 한
publication 경계에서 다시 확인한다. final proof가 먼저 commit된 뒤 도착한 cancel/deadline은
완료를 뒤집지 않는다. 반대로 cancel/deadline이 final proof publication보다 먼저 관찰되면
성공으로 확정하지 않고 accepted continuation을 pending으로 남긴다.

## 3. 축 mutation 귀속 경계

SDK는 connection session과 `AxisReference`로 범위를 나눈 process-local axis mutation
generation을 공유한다. `LMCSingleAxis`를 통한 raw sync/async Power On/Off, Reset, Stop,
Move Absolute/Relative/Velocity와 accepted-wait write는 실제 전송됐을 가능성이 생기는
may-have-been-sent boundary에서 generation을 증가시킨다. validation 또는 선점으로 zero-wire인
호출은 증가시키지 않으며 다른 AxisReference의 mutation도 간섭하지 않는다.

Reset Resume은 status 송신 전, status publication과 final resolution에서 원 Reset generation을
다시 확인한다. 나중의 같은 축 mutation이 확인되면 `LMCAxisResetInterferenceException`으로
귀속을 거부하고 continuation을 pending으로 보존하며 Reset을 replay하지 않는다. 의도적인
post-Reset Power On도 이 규칙상 원 Reset 귀속을 무효화하므로 이후 오류 해제 귀속이 필요하면
명시적으로 새 Reset을 보내야 한다.

이 generation은 현재 프로세스의 `LMCSingleAxis` write만 포괄한다. 외부 PLC logic, 다른 RPC
client, direct SDO write와 group operation은 귀속 범위 밖이다.

## 4. WPF 동작

Axis `Reset` 버튼은 Begin을 live-command gate 안에서 실행하고 accepted continuation을 gate 반환
전에 저장한다. 이후 Resume은 gate를 점유하지 않는 preemptible status-only monitor에서 실행한다.
timeout, cancellation, status failure 또는 safety preemption 뒤에는 accepted continuation을
그대로 재개하며 `0x2024`를 자동 replay하지 않는다. 새 live mutation은 unresolved accepted Reset
동안 interlock된다. 단, SDK가 `LMCAxisResetInterferenceException`으로 나중의 같은 축 mutation을
확정한 경우에만 버튼을 `Reset Again (Confirmed Interference)`로 바꾸고 이후 사용자의 명시적
클릭으로 새 Reset을 허용한다.

성공 시 마지막 Axis status와 Reset submission, poll 수, stable clear count 및 expected/observed
mutation generation을 표시한다. typed 실패도 evidence가 있으면 같은 필드를 화면에 남긴다.
WPF는 Reset을 보내기 전에 endpoint, axis name/reference, diagnostics BootId/MapRevision과 stable
sample 수를 durable journal에 기록한다. success ACK는 accepted observer에서 첫 status보다 먼저
`AcceptedAwaitingProof`로 저장한다. 같은 session에서 observer 저장이 실패하면 SDK의 exact pending
continuation을 회수해 `0x2024` 재전송 없이 status-only Resume을 허용한다. process가 accepted 저장
뒤 종료되면 새 process는 exact endpoint/D0/axis identity를 확인한 뒤 `0x2028`만 3회 이상 읽고,
마지막 D0 live refresh까지 일치해야 journal을 resolve한다. `ArmedBeforeDispatch` 또는 post-write
outcome-uncertain record는 자동 Reset replay나 status-only 완료 대상으로 승격하지 않는다.

active Reset을 Stop으로 대체할 때는 durable predecessor identity를 Stop record에 보존한다. 기존
transport abort, 새 session 연결 또는 identity 확인이 Stop wire 전에 실패하거나 Stop이 valid NACK를
반환하면 아직 pending인 exact Reset record를 복원한다. 이미 완료된 Reset은 다시 활성화하지 않는다.
Stop write 뒤 결과가 불명확하면 Reset을 복원하지 않고 Stop을
`RecoveryRequired`로 유지한다. abort session mismatch는 현재 transport를 건드리지 않으며 stale
same-session Reset continuation을 폐기하고 durable cross-session status-only 복구만 허용한다.

화면에는 아래 경계를 명시한다.

```text
Boundary: this proves the LASAL AxisErrorId observation only;
DS402 Fault and drive error-register clearance are not proven.
```

## 5. 자동 검증

- SDK Release 전체: `974/974 PASS`
- WPF Release build: warning 0, error 0
- WPF Release actual-control smoke: `206/206 PASS`

Axis Reset 전용 SDK 33개 회귀는 exact one Reset, Begin zero status/status-only Resume,
ACK reject 뒤 zero status, timeout/cancel evidence, Resume epoch reset과 누적 poll, AxisError 재발에
따른 stable count reset, unsuccessful status typed failure, response-loss `OutcomeUncertain`, delayed
ACK send-priority discard, ACK/status 무응답 deadline transport invalidation, commit 직전 cancel
zero-wire, invalid/concurrent/stale continuation, accepted publication/Close race, hard gate/compound
deadline, response-loss Begin이 prior pending을 보존하되 generation으로 무효화하는 경계, final
proof의 early/late cancel 경계와 같은 축 mutation 간섭을 검사한다.

신규 WPF 회귀는 실제 button handler와 fake TCP를 통해 Axis Reset 전용 `7/7`, Axis Stop/Reset
통합 `18/18`, journal `9/9`, 실제 child-process recovery `4/4`를 확인한다. `0x2024` 1회,
status-only Resume, 실패 뒤 no-replay 재개, confirmed interference 뒤의 exact-identity 명시적 새 Reset,
ACK 뒤 durable MarkAccepted 직전 Kill의 command/status zero-replay를 포함한다. completed Reset 뒤 Stop
NACK는 별도 final D0가 일치할 때만 durable Stop을 resolve하고 MapRevision mismatch에서는 Reset을
복원하지 않은 채 exact Stop/predecessor record를 `RecoveryRequired`로 유지한다.

## 6. 검증 경계와 남은 실기

- current PLC `0x2028`의 `StatusWord` slot은 0으로 채우는 reserved 값이다. DS402 `0x6041`
  Fault bit 증거로 사용하지 않는다.
- 이 facade는 drive error register `0x603F`를 읽지 않는다.
- 결과는 LASAL `AxisErrorId == 0`의 연속 관찰이다. encoder multiturn error나 Elmo-specific
  recovery가 완료됐다는 증거가 아니다.
- 같은 process/session/AxisReference의 `LMCSingleAxis` mutation은 generation으로 검출한다. 외부
  PLC command, 다른 client, group command 또는 SDO가 상태를 바꾼 원인까지 귀속하지 않는다.
- 실제 PLC/축에서는 packet capture로 `0x2024`가 한 번만 송신되고 뒤에 `0x2028`이 이어지는지,
  그리고 WPF가 표시한 마지막 `AxisErrorId`와 실제 축 상태가 일치하는지 확인해야 한다.

# Axis Power On accepted-once 복구 계약

작성일: 2026-07-29

## 목적

Axis Power On `0x2023`의 성공 ACK는 요청 접수 증거이지 최종 `PowerOn=True` 증거가 아니다.
ACK 뒤 상태 확인이 timeout, 연결 손실, 프로세스 종료로 끊겼을 때 같은 Power On을 다시
보내면 명령이 중복될 수 있다. 이 변경은 Power On을 정확히 한 번만 보내고 이후에는
`0x2028` 상태 읽기만 재개하도록 SDK와 WPF 계약을 고정한다.

이 변경에서 LASAL source와 wire packet 형식은 바꾸지 않았다.

## Wire 계약

1. 새 Power On은 `0x2023`을 한 번 전송한다.
2. 성공 ACK를 받은 뒤에는 동일 session/axis에 pending continuation을 남긴다.
3. pending 설치와 mutation gate 해제 직후, 취소·deadline·첫 `0x2028`보다 먼저
   accepted observer를 정확히 한 번 호출한다. WPF는 이 경계에서 journal을
   `AcceptedAwaitingProof`로 동기 저장한다.
4. 완료 확인은 `0x2028`의 성공 응답에서 `PowerOn=True`를 기본 3회 연속 확인한다.
5. timeout 또는 취소 뒤 재개는 continuation을 사용해 `0x2028`만 보낸다.
6. pending continuation이 있는 동안 기존 `PowerOn()`/`PowerOnAsync()`도
   `LMCAxisPowerOnPendingException`을 발생시키며 `0x2023`을 보내지 않는다.
7. Power On 완료를 확인하지 못한 상태를 안전하게 해제하려면 명시적 Power Off ACK 뒤
   `PowerOn=False && Standstill=True`를 3회 연속 확인해야 한다.
8. mutation/status gate 대기, `0x2023` ACK, 각 `0x2028` 응답과 poll delay는 한 total deadline에
   포함된다. write 뒤 응답이 deadline까지 오지 않으면 stream을 추정으로 재사용하지 않고
   connection을 `Faulted`로 전환한다.
9. Power On write의 may-have-been-sent 경계에서 process-local axis mutation generation을 기록한다.
   Resume 전과 각 status publication에서 같은 session/AxisReference의 later mutation을 확인하며,
   달라졌으면 `LMCAxisPowerOnInterferenceException`과 pending continuation을 반환하고 status를 원
   Power On에 귀속하지 않는다.
10. 마지막 stable status, cancel, deadline과 mutation generation은 coordinator lock 안에서 한 번에
    결정한다. proof publication 뒤 late cancel/deadline은 성공을 뒤집지 않고, 먼저 관찰된
    cancel/deadline/interference는 pending을 보존한다.

## SDK API

- `PowerOnAndWaitForStableStateAsync(...)`
  - `0x2023` accepted-once 전송과 `0x2028` stable proof를 한 작업으로 수행한다.
  - `(options, acceptedContinuationObserver, cancellationToken)` 오버로드는 accepted
    continuation을 설치하고 mutation gate를 해제한 뒤 observer를 호출한다. observer가
    실패해도 continuation은 pending으로 남고 status poll은 시작하지 않는다.
- `PendingPowerOnWaitContinuation`
  - 같은 connection session과 axis reference에 속한 미완료 accepted evidence다.
- `ResumePowerOnWaitForStableStateAsync(...)`
  - continuation을 검증하고 `0x2028` polling만 재개한다.
- `WaitForPowerStateAsync(...)`
  - 프로세스 재시작처럼 SDK continuation이 메모리에 없을 때 사용하는 read-only helper다.
- `ResolvePowerOnWaitAfterStablePowerOff(...)`
  - 해당 continuation에서 관찰한 stable Power Off/Standstill proof로만 pending을 해제한다.
- `LMCAxisPowerOnWaitEvidence`
  - `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted` submission outcome, ACK, 마지막 status,
    poll/stable count, expected/observed mutation generation, interference 여부, elapsed time과
    `TransportInvalidatedAtDeadline`을 보존한다.
- `LMCAxisPowerOnInterferenceException`
  - 원 Power On 뒤 같은 process/session/AxisReference의 다른 mutation이 wire 경계에 도달했음을
    typed evidence로 알리고 exact continuation을 pending으로 유지한다.
- `LMCAxisPowerOnSubmissionException`
  - write가 시작됐을 수 있으나 valid ACK를 게시하지 못한 response loss/send-priority discard를
    continuation 없는 `OutcomeUncertain`으로 보고한다.

다른 session, 다른 axis handle 또는 이미 해제된 continuation은 wire 송신 전에 거부한다.
동시에 두 status wait가 pending ownership을 바꾸지 못하도록 axis별 coordinator와 gate를
사용한다. ACK 뒤 취소는 transport를 중단하지 않고 응답을 drain한 뒤 typed cancellation과
continuation을 반환한다. observer 실행 thread는 보장하지 않으므로 WPF observer는 journal
영속화만 수행하고 UI/log 갱신은 await 복귀 뒤 처리한다.

최종 pre-write cancellation은 `NotAttempted`, zero `0x2023`, reusable connection이다. write 뒤
ACK 무응답 deadline은 `OutcomeUncertain`, continuation 없음, observer 0회와 `Faulted` transport를
보존한다. accepted ACK 뒤 status 무응답 deadline은 `Accepted`와 exact pending continuation을
유지하면서 transport를 `Faulted`로 격리한다. ACK 또는 status response가 새 safety generation으로
폐기될 때도 submission/result-discard와 accepted/status-discard를 구분한다. restart용
`WaitForPowerStateAsync` 역시 `0x2028` write 뒤 무응답에 같은 deadline/transport 격리를 적용하며
`0x2023`을 전송하지 않는다.

## WPF durable journal

기본 위치는 아래와 같다.

`%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\AxisPowerOnRecoveryJournal\v1`

journal은 단일 writer lock, SHA-256 checksum, write-through temporary file과 atomic replace를
사용한다. 저장 identity는 endpoint IPv4/port, axis name/reference, DiagnosticsBootId,
MapRevision, GUID와 UTC timestamp다.

| 상태 | 의미 | 허용 복구 |
|---|---|---|
| `ArmedBeforeDispatch` | wire 송신 전 journal 기록은 끝났지만 ACK 결과를 모름 | 재시작 시 `RecoveryRequired`로 승격 |
| `AcceptedAwaitingProof` | SDK에서 성공 ACK continuation을 확인함 | exact identity에서 `0x2028` status-only 재개 |
| `RecoveryRequired` | Power On 결과를 증명할 수 없음 | 명시적 Power Off + 3회 safe-state proof |
| `Resolved` | stable Power On 또는 명시적 safe-state proof 완료 | 새 Power On 허용 |

active record가 있으면 다른 endpoint, 다른 axis name/reference, 다른 BootId/MapRevision을
wire 송신 전에 차단한다. SDO Write, Digital Output Write, Bulk/Recorder와 새 qualification
mutation, 연결된 상태의 reconnect, clean close와 window close도 unresolved 상태를 무시하고
진행하지 않는다. Stop, Power Off, 기존 resource cleanup, 일반 read-only와 필요한 exact
readback은 계속 허용한다.

WPF가 `AcceptedAwaitingProof`로 재시작하면 Power On 버튼은
`Resume Power On Verification (No 0x2023 Replay)`로 바뀐다. 이 버튼은 새 Power On을 보내지
않고 `WaitForPowerStateAsync(true)`만 실행한다. `ArmedBeforeDispatch` 재시작은 자동 Power On을
시도하지 않으며 사용자가 명시적으로 Power Off를 실행해야 한다. 같은 WPF 프로세스에서 ACK 뒤
연결이 끊겨도 old-session continuation을 폐기하고 exact reconnect 뒤 같은 status-only 경로를
사용한다.

## 자동 검증 결과

2026-07-29 로컬 fake RPC와 process test 기준 결과다.

- SDK Debug/Release build와 전체 contract test: 각각 `876/876 PASS`
- WPF Release build: warning 0, error 0
- WPF smoke Release build: warning 0, error 0
- WPF 전체 smoke test: `125/125 PASS`
- 실제 WPF child process에서 첫 `0x2028` 응답을 보류하고 journal이
  `AcceptedAwaitingProof`, UI 작업이 running인 상태에서 강제 종료한 뒤 재시작한 시험:
  첫 session `0x2023` 1회, 두 번째 session `0x2023` 0회, status-only recovery PASS
- 같은 프로세스 disconnect/reconnect: 두 번째 session `0x2023` 0회, `0x2028` 3회 PASS
- accepted observer: 정상/전송 후 취소 1회, 전송 전 취소/ACK 거절 0회, observer 실패 시
  pending 보존과 status poll 0회 PASS
- Power On ACK 무응답: `OutcomeUncertain`, observer/continuation 없음, transport `Faulted` PASS
- accepted Power On status 무응답: `Accepted`, exact continuation pending, transport `Faulted` PASS
- 최종 pre-write commit-window 취소: `NotAttempted`, zero Power On wire, connection 재사용 PASS
- ACK result discard: `OutcomeUncertain`; status result discard: `Accepted` pending continuation PASS
- ACK parse 뒤 continuation publication 전 same-axis Move: Power On 1회, Move 1회, status 0회,
  expected/observed generation 차이와 typed interference/pending PASS
- Resume 전 same-axis mutation, status publication race, final proof의 early/late cancel·deadline,
  zero-wire/different-axis 비간섭 PASS
- restart read-only status 무응답: zero Power On wire와 transport `Faulted` PASS
- Axis Power On unresolved diagnostics admission: mutation 차단과 safety/read-only/cleanup 허용 PASS
- endpoint mismatch: TCP 연결 전 차단 PASS
- journal reopen, checksum, single-writer lock, Armed/Accepted 상태 전이 PASS

## 아직 증명하지 않은 범위

위 결과는 PC build, fake RPC, 실제 WPF process 강제 종료/재시작에 대한 증거다. PLC download,
실제 EtherCAT 축의 Power On/Off, cable loss, PLC reboot, BootId/MapRevision 변화와 packet capture는
아직 검증하지 않았다. 실제 장비 시험에서는 `0x2023` 개수와 이어지는 `0x2028` status sequence를
capture로 확인해야 한다.

WPF는 typed Power On 실패 evidence의 submission, ACK 유무, status poll/stable count와 transport
invalidation을 표시한다. durable journal 판정은 그대로다. continuation이 있으면
`AcceptedAwaitingProof`, ACK를 증명하지 못하면 `RecoveryRequired`를 유지한다.

connection-loss Axis journal 전이 실패가 다른 motion/group/diagnostics cleanup을 막지 않도록
예외를 격리했다. 실제 journal 파일을 잠가 `File.Replace`를 `IOException`으로 실패시킨 WPF
통합 회귀에서 Axis record의 fail-closed 유지와 motion/group/diagnostics cleanup, topology
초기화 및 UI 갱신을 함께 확인했다.

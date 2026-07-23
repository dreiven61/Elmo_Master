# SIGMATEK 다음 Runtime Qualification 및 Test UI 설계

- 작성일: 2026-07-23
- 대상 PC 앱: `LMC_Library/LasalApiWpfTestApp`
- 대상 API: `LMC_Library/LMC_API_Delivery/src`
- 대상 PLC: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- 상태: 1차 source/UI 구현과 PC build 완료, LASAL IDE/download 및 PLC live qualification 대기
- 최신 packet 판정:
  [SIGMATEK Phase 1/2 Live Packet Capture Analysis](SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md)

## 1. 목적과 경계

이 문서는 현재까지 확보한 live packet 증거를 기준선으로 고정하고 다음 source 수정,
Test UI 자동화, PLC 재캡처 순서를 정한다. 목표는 기능 수를 늘리는 것이 아니라 이미
구현된 기능을 반복 가능하고 판정 가능한 runtime evidence로 닫는 것이다.

다음 세 상태는 계속 분리한다.

1. C#/LASAL source와 정적 계약이 구현된 상태
2. WPF build/startup이 통과한 상태
3. 실제 PLC packet과 최종 상태가 합격한 상태

이 설계는 Homing, SetPosition, unrestricted PI/SDO Write 또는 장비 safety approval을
새로 허용하지 않는다. motion 시험은 사용자가 장비 limit, Home, E-stop과 시험 거리를
확인한 뒤에만 실행한다.

## 2. 현재 기준선

| 영역 | 현재 판정 | 남은 핵심 gate |
|---|---|---|
| PC API | Debug/Release 148/148 계약 시험 PASS | 새 runner helper의 순수 로직 시험 추가 |
| 개발 WPF | 공통 runner, Group/Bulk/Recorder qualification UI source와 Debug/Release build PASS; Debug visual/startup smoke PASS | 실제 PLC scenario 실행 |
| LASAL source/network | `0x2047` accepted-then-poll source 수정과 SourceOnly/full static contract PASS | IDE Rebuild/Link/smoke/download 및 live ACK 재검증 |
| Admin `0x7D00/10/20` | live happy path PASS | invalid/stale/fault |
| Group absolute/relative | 기존 acceptance/per-axis/dynamic-timeout live PASS; true Buffered/stop-first runner code와 WPF build PASS | runner의 실제 PLC packet/final-position 검증과 전체 fault matrix |
| Group PowerOff | final `PowerOn=False` packet/log PASS | button label/enable visual assertion |
| Group position `0x2051` | source/static + `09b` live static-alias PASS | true ACS transform는 미구현이며 MCS/PCS live negative는 별도 |
| GroupEnable `0x2047` | same-cycle 상태 read 제거와 정적 계약 PASS | 새 PLC build/download 뒤 acceptance ACK 0 및 후속 `0x2045` poll live 확인 |
| D1/D2 PI/Bulk | 기존 24-entry Catalog, 4 PI, 4-entry snapshot/release live PASS; 24-entry snapshot/lifecycle soak code와 WPF build PASS | 100회 live 실행, stale/fault/partial external workflow |
| D3/D4 Recorder | Single/Ring forced-trigger/trigger-soak runner code와 WPF build PASS | PLC live Single/Ring/100회; reconnect/adopt, fault, RAM/jitter와 Double은 별도 |
| D5 SDO Read | general-inline 1/2/4-byte와 TypeMismatch recovery PASS | offline/abort, timeout, queued cancel, orphan, contention |

최신 세부 사실은 다음과 같다.

- `04b`: 55.034초 계산 한도로 20.152초 stable InPosition PASS. TXT가 0 byte라 exact
  UI timeout 문자열은 미증명이다.
- `08c`: Disable -> PowerOff -> Read Status `PowerOn=False` PASS. visual button state는
  screenshot 또는 UI automation이 필요하다.
- `09`: `0x2051` 0건, `0x2045` 2건인 시험 절차 불성립 기록이다.
- `09b`: coordinate 0/1의 `0x2051` 두 건이 byte-identical 68-byte typed payload를
  반환해 현재 None/ACS static member-slot alias를 live PASS했다. true ACS transform나
  MCS/PCS를 증명한 것은 아니다.
- `12`: Ticket 13 UInt32/4 성공, Ticket 14 TypeMismatch 실패, 같은 BootId 8의
  Ticket 15 Int8/1 복구 성공이다. 전체 D5 fault matrix 완료는 아니다.

### 2.1 2026-07-23 구현 체크포인트

현재 working source에는 다음이 구현됐고 PC 쪽 Debug/Release build가 통과했다.

- `TCPMotionInterface.st`: `0x2047`은 `LockProfile` native acceptance만 ACK하고 같은 cycle의
  `_LMCPROF_LockState` 완료 검사를 제거했다. 정적 계약은 PASS지만 LASAL IDE build/download와
  PLC의 ACK 0 packet은 아직 확인하지 않았다.
- `MainWindow.Qualification.cs`: 단일 active scenario, cancel, `QTEST` 구조화 로그,
  GroupEnable 후속 poll, true Buffered A/B, deterministic stop-first와 cleanup을 구현했다.
- `MainWindow.Qualification.Bulk.cs`: exact 24-entry Snapshot soak와
  Configure -> Active -> Snapshot -> Release lifecycle soak를 구현했다.
- `MainWindow.Qualification.Recorder.cs`: 4-channel Single Manual, Ring forced trigger,
  100-cycle 기본 trigger soak, frozen metadata/byte count/SHA-256와 handle/buffer cleanup을
  구현했다.
- `MainWindow.xaml`과 project file에는 위 시나리오의 입력, 실행/cancel/save UI와 partial
  source 등록을 반영했다.
- Debug 실행 visual/startup smoke에서 Group/Bulk/Recorder qualification panel 렌더와
  prerequisite가 없는 초기 상태의 실행 버튼 disabled를 확인했다. 이는 WPF 렌더와
  fail-closed UI gate 확인이며 PLC RPC, packet 또는 scenario runtime PASS는 아니다.

이 체크포인트의 `구현/빌드 완료`는 PLC runtime PASS가 아니다. Group packet 14/15,
Bulk packet 16/17, Recorder packet 19/20/22와 companion `QTEST` 로그가 생기기 전에는
각 scenario를 live 완료로 표시하지 않는다. Recorder reconnect/adopt, 한 slave offline,
별도 raw negative, RT RAM/jitter 및 D4 Double은 이 구현 체크포인트에 포함되지 않는다.

## 3. 구현 결정

### 3.1 기존 화면을 확장한다

별도 WPF 프로그램을 만들지 않는다. 현재 Group Motion, Bulk Snapshot, Recorder,
SDO / Write Policy 탭에 `Qualification automation` 영역을 추가한다. 사용자는 같은
입력, 연결 상태, capability, Catalog와 resource identity를 수동 시험과 자동 시험에서
공유한다.

자동화 코드는 공통/Group의 `MainWindow.Qualification.cs`, Bulk의
`MainWindow.Qualification.Bulk.cs`, Recorder의 `MainWindow.Qualification.Recorder.cs`
partial file에 격리한다. 기존 public SDK를 우회하거나 async UI handler를 서로 호출하지
않는다.

### 3.2 일반 UI와 negative raw-wire 시험을 분리한다

public SDK는 stale handle, stale session과 double release를 송신 전에 차단한다. 이 보호를
약화해서 PLC 오류 응답을 만들지 않는다.

- 일반 Test UI: public API happy path, soak, local guard와 external fault checkpoint
- 별도 내부 도구: 고의의 stale MapRevision/ConfigRevision/BootId, raw duplicate release,
  malformed payload

별도 도구는 배포 예제에 포함하지 않고 diagnostics read/resource 명령만 허용한다.
motion raw frame은 만들지 않는다.

### 3.3 motion 취소는 Stop으로 끝낸다

diagnostics 반복 시험은 각 RPC 사이에서 `CancellationToken`을 확인해 중단할 수 있다.
motion 시험은 token 취소만으로 완료 처리하지 않는다. 취소·실패 시 반드시
`GroupStop`을 송신하고 stable Group InPosition 3회를 확인한다. 확인 실패 시
motion-uncertain 경고를 유지한다.

## 4. 구현 순서

| 순서 | 작업 | 이유 |
|---:|---|---|
| 1 | 완료: `09b` None/ACS static-alias 재캡처 | 현재 `0x2051` runtime 계약을 source 변경 없이 닫음 |
| 2 | 코드/정적 완료, live 대기: PLC `0x2047` accepted-then-poll 수정 | 새 IDE build/download와 ACK 재캡처 필요 |
| 3 | 코드/빌드 완료: 공통 qualification runner와 구조화 로그 | 이후 반복 시험의 공통 기반; live log는 아직 없음 |
| 4 | 코드/빌드 완료, live 대기: true Buffered chaining | A 동작 중 B queue와 누적 endpoint packet 검증 필요 |
| 5 | 코드/빌드 완료, live 대기: deterministic stop-first preemption | Move 0건/Stop 1건 packet 증명 필요 |
| 6 | 코드/빌드 완료, live 대기: Bulk 24-entry/100 snapshot 및 lifecycle soak | 실제 100회 resource/sequence 검증 필요 |
| 7 | 미구현/수동 대기: Bulk partial external-fault workflow | 한 slave offline checkpoint와 복구는 사용자 승인 필요 |
| 8 | 부분 코드/빌드 완료: Recorder Single/Ring/Trigger soak | PLC live 실행 대기; reconnect/adopt, RT evidence와 Double은 아직 범위 밖 |
| 9 | D5 나머지 fault matrix | 외부 fault와 timing hook이 필요한 항목을 분리 실행 |
| 10 | 별도 negative-wire 도구 | SDK 보호를 유지한 뒤 PLC raw rejection만 검증 |

## 5. `09b` Group Position 재캡처 결과

### 5.1 절차

1. group을 load하고 member 1..4가 X/Y/Z/U인지 확인한다.
2. motion이 없고 stable InPosition인지 확인한다.
3. Coordinate `None`으로 `Read Position`을 1회 실행한다.
4. Coordinate `ACS`로 같은 명령을 1회 실행한다.
5. 중간에 `Read Status`를 대신 누르지 않는다.

### 5.2 packet 합격 기준

- capture에 client request `0x2051`이 정확히 2건 존재한다.
- 첫 request coordinate DINT는 0, 두 번째는 1이다.
- 두 response payload는 각각 exact 68 bytes다.
- `HeaderStatus=0`, `FunctionStatus=0x4000`, `ErrorId=0`이다.
- slot 1..4는 동일한 X/Y/Z/U raw DINT 순서이고 허용 tolerance 안에서만 변한다.
- slot 10..16은 0이다.
- MCS/PCS 변환을 증명했다고 기록하지 않는다.

파일명은 `09b_Group_ReadPosition_None_ACS_2051.pcapng/.txt`로 고정한다.

### 5.3 완료 결과

- frame 1/4의 요청은 group `0x0100`, payload 8, Execute 1이며 coordinate만 0/1로 다르다.
- frame 2/5의 응답 시간은 각각 0.715 ms, 2.365 ms다.
- 두 typed payload 68 bytes는 byte-identical하다.
- DINT slot 1..4는 `[-999997, -999998, -999997, -999998]`, slot 5..16은 0이다.
- TXT는 두 번의 `Read Group Position PASS`를 기록한다.

따라서 첫 구현 순서의 `09b` gate는 완료다. 이 판정은 의도한 static alias의 실기
확인일 뿐 true ACS transform 구현이나 MCS/PCS runtime 동작을 승인하지 않는다.

## 6. `0x2047 GroupEnable` 수정 설계(source/static 구현 완료, live 대기)

### 6.1 원인

수정 전 `TCPMotionInterface.st`의 `0x2047` handler는 다음 순서였다.

1. `GroupReadErrorId := -6`으로 시작
2. `LMCRobot.LockProfile(...)` 호출
3. native return이 `_LMCPROF_NoError`여도 같은 CyWork 호출에서
   `_LMCPROF_LockState`를 즉시 읽음
4. 아직 state가 갱신되지 않았으면 초기값 `-6`을 ACK

SIGMATEK `_LMCProfileBase` revision note는 active CyTask/EndProfile의 lock 처리가
CyWork로 넘겨질 수 있음을 설명한다. 따라서 same-cycle `LockState`는 최종 완료 조건으로
사용할 수 없다. live capture의 약 2초 간격은 사용자가 다음 Read Status를 누른 시점이며
실제 lock latency가 2초라는 증거가 아니다.

### 6.2 PLC 변경

수정 대상은
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`의
`0x2047` case다.

```st
GroupReadRetCode := LMCRobot.LockProfile(...);

if GroupReadRetCode = _LMCPROF_NoError then
  // Native request accepted. Final lock is verified by 0x2045 only.
  GroupReadErrorId := 0;
elsif GroupReadRetCode$UDINT <= 32767 then
  GroupReadErrorId := GroupReadRetCode$DINT;
end_if;
```

같은 `0x2047` case의 `ReadProfileParameter(_LMCPROF_LockState)` 완료 검사는 제거한다.
다음 선행 조건은 그대로 유지한다.

- valid client와 fixed group reference `0x0100`
- `GroupKinematicReady = TRUE`
- `RobotIsOn() <> 0`
- 축 1..4 lock, 축 5..9 off
- native nonzero return의 signed 16-bit 전달과 overflow fallback `-6`

최종 lock 완료는 기존 `0x2045 GroupReadStatus`의 PowerOn + LockState + InPosition으로만
판정한다. CyWork에서 busy-wait, delay 또는 반복 poll을 추가하지 않는다.

### 6.3 정적 계약 변경

`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1`
의 기존 `0x2047` regex는 결함 있는 same-cycle LockState read를 필수로 요구한다.
다음 계약으로 바꾼다.

- `LockProfile`과 축 1..4 mask는 필수
- native `_LMCPROF_NoError`에서 `GroupReadErrorId := 0` 필수
- `0x2047` case 안의 `_LMCPROF_LockState` read 금지
- `0x2045 GroupReadStatus`의 LockState/PowerOn/InPosition mapping은 계속 필수
- native nonzero signed error와 overflow fallback 보존

C# ACK parser 또는 `LMCGroupAxis.GroupEnableAsync`에서 `-6`을 성공으로 바꾸지 않는다.

### 6.4 WPF 동작

현재 WPF는 성공 ACK 뒤 `groupProfileLockVerificationPending=true`로 두고
Enabled/Locked Standby 전까지 Move를 비활성화한다. 수동 Read Status flow는 유지했고,
새 `GroupEnableAcceptedThenLocked` qualification은 `0x2047` 1회 뒤 `0x2045`를 50 ms
간격, 5초 deadline과 stable sample 조건으로 poll한다. source/build는 완료됐지만 아래
live 합격 기준의 packet은 아직 없다.

### 6.5 live 합격 기준

파일명: `13_GroupEnable_AcceptedThenLocked_2047_2045.pcapng/.txt`

1. PowerOn과 identity 완료 뒤 GroupEnable을 한 번만 실행한다.
2. `0x2047` response는 `Status=0, ErrorId=0`이다.
3. ACK 직후 Move는 비활성이다.
4. `0x2045`를 최대 5초 poll하여 `PowerOn=True`, `IsStandby=True`, error 0을 확인한다.
5. Standby 확인 후에만 Move가 활성화된다.
6. 작은 relative move, Disable, PowerOff, final `PowerOn=False` 회귀가 통과한다.

native nonzero error인데 ACK 0, Standby 미확인 Move 활성화, 5초 미도달 자동 성공은
모두 실패다.

## 7. 공통 Qualification Runner

### 7.1 상태와 파일 구조

새 partial file에 다음 상태를 둔다.

```text
qualificationRunning
qualificationCancellation
qualificationRunId
qualificationScenario
qualificationStep
qualificationLogLines
```

runner state는 `Idle -> Preflight -> Running -> Cleanup -> Pass/Fail/Aborted`다.
동시에 하나의 scenario만 실행한다. XAML에는 각 탭의 scenario 입력, `Run`, `Cancel Test`,
진행률, 결과 요약/구조화 text와 `Save QTEST Log`를 추가한다. 전체 실행 결과는 공통
log collection을 공유한다.

권장 model:

- `QualificationRun`: RunId, Scenario, StartedUtc, FinishedUtc, Endpoint, Verdict
- `QualificationStepResult`: Step, CommandId, StartedUtc, ElapsedMs, Expected, Actual,
  Verdict, Error
- `QualificationVerdict`: Running, Pass, Fail, Aborted

### 7.2 기존 직렬화와 safety 우선권 재사용

- 모든 qualification wire dispatch는 기존 `commandSendGate`를 거치고 gate 안에서
  runner 시작 시점의 `safetyRequestGeneration`과 scenario token을 다시 검사한다.
- Recorder download는 header/chunk별로 gate를 다시 얻어 chunk 사이에서 safety가
  선점할 수 있게 한다. Bulk Catalog public helper는 cancellation이 connection을 강제
  종료할 수 있어 `CancellationToken.None`으로 bounded compound operation 전체를
  gate 안에서 완료한다.
- Stop/PowerOff는 generation을 먼저 증가시킨 뒤 gate를 기다리는 현재 우선권을 유지한다.
- qualification 중 ordinary live buttons는 막되 Group Stop과 PowerOff는 계속 허용한다.
- cleanup RPC는 cancellation과 분리하되 진행 중인 safety send/monitor가 끝난 뒤 같은
  gate를 통해 전송한다.
- runner 내부에서 `RunOperationAsync`를 중첩 호출하지 않는다. async void button handler도
  호출하지 않고 public API와 기존 Task helper를 직접 사용한다.
- `UpdateUiState`의 idle 조건에 `!qualificationRunning`을 포함한다.

### 7.3 구조화 로그

Wireshark와 timestamp로 맞출 수 있도록 한 줄 고정 형식을 사용한다.

```text
QTEST|utc=2026-07-23T04:00:00.0000000Z|elapsedMs=0|run=...|scenario=...|step=1|event=BEGIN
QTEST|utc=...|elapsedMs=15|run=...|scenario=GroupBuffered|step=4|cmd=0x7D22|sequence=A|buffer=2|deltaRaw=10000,0,0,0
QTEST|utc=...|elapsedMs=2500|run=...|scenario=GroupBuffered|step=9|event=ASSERT|name=FinalPosition|expected=...|actual=...|verdict=PASS
QTEST|utc=...|elapsedMs=2600|run=...|scenario=GroupBuffered|event=END|verdict=PASS
```

BEGIN에는 endpoint, group reference, capability bits, BootId, MapRevision, 선택 signal ID와
UNIT을 기록한다. command별로 API가 노출하는 RequestId, TicketId, BulkId,
ConfigRevision, RecordId, BufferId, cycle/sequence/status/error/detail을 기록한다.
API가 노출하지 않는 ID를 추정해 쓰지 않는다.

100회 시험은 매 회 전체 UI text를 다시 그리지 않는다. 10회마다 progress summary를
남기고 실패는 즉시 전체 context와 함께 기록한다. Save dialog의 기본 파일명은
`yyyyMMdd_HHmmss_<scenario>.txt`다.

### 7.4 cleanup 원칙

- Group: 실패/취소 -> GroupStop -> stable InPosition 3회
- Bulk: allocated handle이 있으면 finally에서 Release 시도
- Recorder: 상태 확인 -> 필요 시 Stop -> `Ready` 또는 이미 frozen download가 시작된
  `Uploading`에서만 buffer/config Release. `Fault`는 자동 Release하지 않고 identity와
  resource를 보존한 채 명시적 recovery/manual investigation 대상으로 남김
- SDO: queued이면 Cancel 시도, terminal이면 identity 해제; active/orphan이면 명시적으로
  결과와 다음 복구 절차를 남김
- cleanup 실패는 원래 scenario 실패와 별도 step으로 보존하고 PASS로 덮지 않음

## 8. Group 자동 qualification

### 8.1 True Buffered chaining

Group Motion 탭에 다음을 추가한다.

- Axis X/Y/Z/U
- Delta A, Delta B
- velocity, acceleration, deceleration, tolerance
- `Run Buffered A -> B`
- `Cancel Test`

기본값은 한 축에 A `+10000`, B `+20000` raw DINT다. 최종 기대 위치는
`Start + A + B`다. 같은 크기의 반대 delta는 최종 합이 0이라 second command가 first를
대체해도 판별력이 약하므로 qualification 기본값으로 쓰지 않는다.

절차:

1. Coordinate None, group PowerOn/identity/Locked Standby/stable InPosition preflight
2. 시작 위치와 선택 축 software min/max를 읽음
3. 선택 축의 시작 위치, A endpoint와 A+B endpoint가 tolerance margin 안인지 검사
4. A를 Buffer=2로 송신하고 ACK 확인
5. `0x2045`에서 Non-InPosition을 적어도 한 번 확인
6. stable InPosition이 나오기 전에 B를 Buffer=2로 송신하고 ACK 확인
7. 전송 단계가 끝난 뒤 position/status monitor
8. final position `Start+A+B`, 다른 세 축 tolerance, error 0, stable InPosition 3회 확인
9. 별도 Aborting cleanup move로 시작점 복귀 후 stable InPosition 확인

합격 packet 조건:

- chaining 판정 시점까지 `0x7D22` 정확히 두 건, 두 request 모두 Buffer DINT 2
- B request timestamp가 첫 stable completion보다 앞섬
- 두 ACK 모두 success와 일치하는 RequestId
- 최종 누적 위치가 `Start+A+B`
- 최종 위치 검증 뒤 선택 축에만 `-(A+B)`를 보내는 `0x7D22` Buffer DINT 1 cleanup
  한 건이 추가되고 captured start tolerance를 확인해야 함
- 성공 경로에는 GroupStop이 없어야 함

파일명: `14_Group_TrueBuffered_Chaining_7D22.pcapng/.txt`

### 8.2 Deterministic stop-first preemption

이 시험은 PLC에 Stop 뒤 Move를 보내는 위험한 race가 아니다. PC gate에서 대기 중인 Move가
Stop 요청 때문에 wire 전에 취소되는지를 검증한다.

1. runner가 `commandSendGate`를 임시 보유한다.
2. 현재 generation으로 Move task를 먼저 시작해 gate에 대기시킨다.
3. Stop task를 시작한다. `RunSafetyCommandAsync`가 generation을 즉시 증가시킨다.
4. gate를 해제한다.
5. Move는 gate 획득 뒤 generation mismatch로 송신 전 취소된다.
6. Stop만 송신하고 stable InPosition을 확인한다.

합격 기준:

- Move delegate 실행 0회
- packet에 `0x7D22`/`0x20A4` 0건
- `0x2085` 정확히 1건과 success ACK
- 로그에 `cancelled before transmission because Stop or Power Off was requested`
- final stable InPosition 3회

파일명: `15_Group_StopFirst_Preemption_2085_NoMove.pcapng/.txt`

## 9. Bulk qualification

### 9.1 24-entry / 100 snapshot soak

Bulk Snapshot 탭에 다음을 추가한다.

- fresh Catalog의 24개 BulkReadable 자동 선택
- Iterations 기본 100
- Interval 기본 10 ms
- `Run Snapshot Soak`
- `Run Lifecycle Soak`
- `Cancel Test`

preflight에서 Catalog의 BulkReadable count가 정확히 24인지 확인한다. 다르면 임의로
부족한 signal을 채우지 않고 FAIL한다.

Snapshot soak:

1. capability/Catalog 재확인
2. 24개 exact order로 Configure
3. Active까지 bounded poll
4. snapshot 100회
5. finally Release

각 snapshot 조건:

- EntryCount 24와 configured signal order exact
- BulkId/ConfigRevision/MapRevision identity 일치
- SameCycle + InputMapped, even SnapshotSequence
- cycle/timestamp 비감소
- Partial false, 24 entry 모두 Valid/detail 0
- parser exception 0

결과에는 100회 success count, min/avg/max RPC latency, cycle delta, partial/invalid/error
count를 기록한다.

Lifecycle soak는 `Configure -> Active -> Snapshot -> Release`를 100회 반복한다. 100/100
cleanup 뒤 새 Configure가 다시 성공해야 한다.

파일명:

- `16_Bulk_24Entry_100Snapshot_Soak_7E30_33.pcapng/.txt`
- `17_Bulk_100Lifecycle_ReleaseReuse_7E30_33.pcapng/.txt`

### 9.2 partial external-fault workflow

UI가 EtherCAT fault를 만들지 않는다. group PowerOff/Disabled와 no-motion을 확인한 뒤
`Inject one slave offline, then Resume` checkpoint를 표시하고 사용자가 승인된 방법으로
한 slave를 non-OP/offline 처리한다.

합격 기준:

- baseline: Partial=false, 24 Valid
- fault: RPC envelope success, Partial=true
- 해당 축 6 entry만 Invalid + Detail 18 `SlaveOffline`
- 나머지 18 entry Valid
- 복구: Partial=false, 24 Valid
- Release 성공

파일명: `18_Bulk_Partial_OneSlaveOffline_7E32.pcapng/.txt`

### 9.3 stale와 double-release 경계

일반 UI에서 검증할 항목:

- 동일 handle 두 번째 Release는 local `InvalidOperationException`
- packet에는 Release 한 건만 존재
- reconnect 뒤 old handle은 wire 전에 stale-session 거부
- release 뒤 새 Configure 성공

일반 UI에서 보내지 않을 항목:

- 고의의 old ConfigRevision/MapRevision/BootId
- raw duplicate `0x7E33`
- capability-off Double request

이 raw rejection은 마지막 단계의 internal-only protocol tool에서
`HandleOrGenerationStale(10)`와 `MapRevisionMismatch(3)`를 별도 확인한다.

## 10. Recorder qualification

### 10.1 D3 Single Manual

- 4 channels, SamplePeriod 1, Capacity 1000
- Configure -> Start -> Ready poll -> Header -> Download A -> Download B -> Release
- StopReason `SampleCountComplete`
- SampleCount=AcceptedCapacity
- data bytes=`samples * channels * 4`
- 두 download raw bytes SHA-256 동일
- identity/header/channel order exact

파일명: `19_Recorder_SingleManual_Lifecycle_7E40_48.pcapng/.txt`

### 10.2 D4 Ring forced trigger

- Ring + Edge, pre 100, post 899, capacity 1000
- Start 뒤 pre-history가 찰 시간을 확보
- Trigger Now -> Ready -> Header -> Download -> Release
- StopReason `TriggerComplete`
- TriggerIndex=100
- SampleCount=`100 + 1 + 899 = 1000`
- chunk coverage exact, duplicate/gap 0

파일명: `20_Recorder_Ring_ForcedTrigger_7E42.pcapng/.txt`

### 10.3 reconnect/adopt

1. Ring Recorder start 뒤 identity를 보존하고 connection close
2. 같은 BootId PLC에 reconnect/capability refresh
3. exact RecordId/BufferId Adopt 시험
4. 별도 run에서 RecordId=0/BufferId=0 discovery Adopt 시험
5. 새 OwnerSessionEpoch로 Status -> 필요 시 Stop -> Header -> Download -> Release

old BootId, identity mismatch 또는 다른 active resource를 성공으로 adopt하면 실패다.

파일명: `21_Recorder_Reconnect_ExactAndDiscovery_7E49.pcapng/.txt`

### 10.4 100-cycle soak와 외부 RT evidence

- 4 channels, pre 16, post 15, capacity 32 forced trigger
- configure/start/trigger/status/header/download/release 100회
- 100/100 terminal/cleanup, ResourceBusy 0, chunk gap 0
- 매 run header/data hash와 identity 기록

`DroppedSamples=0` 또는 reserved `OverflowCount=0`만으로 loss 없음이라 판정하지 않는다.
free RAM, 1 ms task jitter/overrun과 1.28 MB bank hash 불변성은 LASAL Data Analyzer 또는
승인된 PLC profiling evidence로 별도 보존한다. WPF RPC latency가 RT jitter 증거는 아니다.

파일명: `22_Recorder_100Cycle_TriggerSoak.pcapng/.txt`

모든 Recorder 자동 cleanup은 최종 Status를 다시 읽고 release 가능 상태를 판정한다.
`Ready` 또는 이미 frozen download가 시작된 `Uploading`에서만 buffer를 먼저 Release하고
configuration handle을 반환한다. `Fault`는 frozen success로 간주하지 않으며 자동
Release하지 않는다. 이 경우 QTEST에 recovery-required를 남기고 identity/resource를
보존한 뒤 명시적 Status/error 진단과 수동 복구를 수행해야 한다.

## 11. D5 남은 fault matrix

이미 닫힌 항목:

- Int8/1, BitField16/2, UInt32/4 success
- TypeMismatch terminal failure
- 같은 BootId에서 failure 후 다음 ticket success

남은 항목:

| 시나리오 | 실행 방법 | 합격 경계 |
|---|---|---|
| SDO abort | 존재하지 않는 read-only object/subindex를 안전하게 조회 | terminal Failed, Detail `SdoAbort`, abort code 보존, 다음 valid read 성공 |
| slave offline | no-motion/PowerOff 뒤 외부 checkpoint로 한 slave offline | offline/failed terminal 또는 명시적 submit reject, 다른 축과 복구 후 valid read 성공 |
| timeout | 승인된 느린/offline 조건과 작은 nonzero TimeoutCycles | terminal Expired/TimedOut, error envelope 일관성, resource 회수 |
| queued cancel | ticket이 실제 Queued인 구간에서 Cancel | Cancelled/Cancelled, result 없음, 다음 submit 성공 |
| disconnect/orphan | long-running read 직후 owner session disconnect/reconnect | old ticket/owner 재사용 차단, executor orphan cleanup 뒤 새 submit 성공 |
| deliberate contention | first ticket terminal 전 second submit | second `ResourceBusy`, first terminal 뒤 third submit 성공 |
| duplicate/late callback | instrumented test build 또는 장시간 race soak | 이전 ticket 결과가 새 ticket에 섞이지 않고 owner/ticket identity 유지 |

queued cancel과 duplicate/late callback은 수동 클릭만으로 deterministic하지 않을 수 있다.
재현 가능한 PLC test hook 또는 timing evidence 없이 PASS로 표시하지 않는다. EtherCAT SDO
mailbox frame을 독립 확인하려면 PC-LASAL TCP capture가 아니라 EtherCAT link capture가
필요하다.

파일명은 한 파일에 뭉뚱그리지 않고 다음처럼 분리한다.

- `23a_SDO_Abort_Recovery_7E50_03.pcapng/.txt`
- `23b_SDO_Offline_Recovery_7E50_03.pcapng/.txt`
- `23c_SDO_Timeout_Recovery_7E50_03.pcapng/.txt`
- `23d_SDO_QueuedCancel_7E04.pcapng/.txt`
- `23e_SDO_DisconnectOrphan_Recovery.pcapng/.txt`
- `23f_SDO_Contention_ResourceBusy_Recovery.pcapng/.txt`

## 12. 구현 파일 변경

| 파일 | 변경 |
|---|---|
| `Lasal_PRG/.../TCPMotionInterface/TCPMotionInterface.st` | `0x2047` same-cycle state read 제거, native acceptance ACK |
| `LMC_Library/.../tests/.../Verify-LasalContract.ps1` | 새 accepted-then-poll 정적 계약과 금지 패턴 |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.xaml` | 기존 탭별 Qualification GroupBox와 결과 영역 |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.xaml.cs` | qualification state/UI gate, 필요한 공통 Task helper 추출 |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.Diagnostics.cs` | 직접 변경 없이 partial class의 기존 diagnostics resource 상태/formatter 재사용 |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.Qualification.cs` | 공통 runner, Group scenario, logging, safety cleanup |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.Qualification.Bulk.cs` | 24-entry snapshot/lifecycle soak와 release cleanup |
| `LMC_Library/LasalApiWpfTestApp/.../MainWindow.Qualification.Recorder.cs` | Single/Ring/trigger soak, hash assertion과 buffer/config cleanup |
| `LMC_Library/LasalApiWpfTestApp/.../LasalApiWpfTestApp.csproj` | 새 Compile item 등록 |
| `LMC_Library/.../tests/LasalMotionControlLib.Tests` | 기존 148개 회귀 PASS; qualification 전용 fake-RPC test는 아직 추가하지 않음 |
| 관련 README/DESIGN/current-status 문서 | 실제 구현/packet 결과만 단계별 갱신 |

SDK public API 변경은 첫 qualification slice에 필요하지 않다. 구현 중 public API가
부족하다고 판단되면 먼저 기존 facade로 표현 가능한지 확인하고, test-only raw method를
배포 API에 추가하지 않는다.

## 13. 검증 gate

### 13.1 PC 변경

1. [완료] Debug/Release API tests 148/148
2. [완료] `Verify-LasalContract.ps1` SourceOnly/full
3. [완료] WPF Debug/Release build
4. [완료] Debug qualification UI visual/startup smoke: Group/Bulk/Recorder panel 렌더와
   prerequisite 미충족 초기 실행 버튼 disabled 확인
5. [대기] scenario helper fake-RPC success/failure/cancel/cleanup tests
6. [완료] `git diff --check`

### 13.2 LASAL 변경

LASAL IDE를 종료한 상태에서 tracked `.st` implementation을 외부 편집한다. 이후 사용자가
IDE에서 Reload/Reopen, Rebuild/Link, download를 수행한다. 변경 class의
`Find in Implementation` smoke와 smoke 시작 이후 `%TEMP%/Lasal2.log`의 신규
`CInvalidArgException` 0건을 보존한다.

### 13.3 live packet

각 scenario는 pcap과 같은 시간대의 구조화 TXT를 한 쌍으로 보존한다. packet이 없으면
wire PASS라고 쓰지 않고, screenshot이 없으면 visual state PASS라고 쓰지 않는다.
기능 성공 뒤 cleanup packet과 최종 상태도 같은 capture에 포함한다.

## 14. 첫 구현 slice의 Definition of Done

첫 slice는 다음을 모두 만족할 때 완료다.

- [완료] `09b`에 coordinate 0/1의 `0x2051` 두 건과 exact 68-byte response가 있음
- [코드/정적 완료, live 미검증] `0x2047`은 native acceptance만 ACK하고 final lock은
  WPF가 `0x2045`로 poll한다.
- [코드/빌드 완료, live 미검증] WPF는 Standby 전 Move를 차단하고 Group qualification을
  ordinary operation과 직렬화한다.
- [코드/빌드 완료, live 미검증] true Buffered A/B와 deterministic stop-first runner가 있다.
- [코드/빌드 완료, live 미검증] Group/Bulk 실패·취소 path와 Recorder의 state-gated cleanup
  결과를 구조화 로그로 남긴다. Recorder 자동 Release는 `Ready`/`Uploading`에서만
  수행하며 `Fault`는 resource를 보존하고 명시적 복구가 필요하다.
- [완료] 기존 PC tests, 새 LASAL static contract, WPF Debug/Release build와 Debug
  qualification UI visual/startup smoke가 PASS했다.
- [대기] LASAL IDE build/download/smoke와 Group/Bulk/Recorder live capture가 필요하다.

추가로 Bulk 24-entry snapshot/lifecycle와 Recorder Single/Ring/trigger soak는 code/build
단계까지 구현됐지만 Definition of Done의 PLC runtime gate는 아직 열려 있다.

다음 순서는 구현된 Group/Bulk/Recorder runner의 PLC capture를 확보하고, 이후 수동
external-fault, Recorder reconnect/adopt, D5 fault matrix와 별도 raw/RT evidence를
진행하는 것이다. 이 gate가 끝나기 전에는 Homing, SetPosition, PI/SDO Write 또는
production version 승격을 시작하지 않는다.

# LASAL Motion Control API Example

루트 `Codex_LASAL_WPF`의 기능을 현재 `LasalMotionControlLib` 기반의 간단한
실제 PLC 예제로 다시 구성한 프로젝트다. 기존 `LasalMotionControlLibTestApp`은
이 예제로 대체되어 제거했다.

## 빌드

Visual Studio 2019에서 `LasalApiWpfTestApp.sln`을 열고 `Debug|Any CPU` 또는
`Release|Any CPU`로 빌드한다. solution 표시는 Any CPU지만 실행 프로젝트의
`PlatformTarget`은 x64다. 출력 파일 이름은 `LasalMotionControlApiExample.exe`다.

프로젝트는 아래 공용 API 소스를 직접 참조한다.

```text
../LMC_API_Delivery/src/LasalMotionControlLib.csproj
```

## 권장 시험 순서

1. 장비의 physical E-stop, software limit와 이동 가능 범위를 확인한다.
2. PLC IP, PC local IPv4와 callback UDP port를 입력하고 Connect한다.
3. `_LMCAxis1` 같은 실제 LASAL object name을 입력하고 Load Axis를 누른다.
4. Read Status와 Read Position을 먼저 실행한다.
5. 대상 축을 다시 확인하고 Power On을 실행한다. 버튼 클릭 시 확인창 없이 즉시 송신된다.
6. 다시 Read Status로 PowerOn 상태를 확인한다.
7. UNIT과 작은 motion 값을 확인하고 Move Absolute/Relative를 시험한다.
8. Move Velocity는 마지막에 시험하고 반드시 Stop 또는 Power Off로 끝낸다.
9. Group 기능은 실제 `_LMCRobotBase1`이 PLC network에 연결된 경우에만 사용한다.
10. Load Group 뒤 Get Members와 Read Position으로 대상 구성을 먼저 확인한다.
    Read Position은 `None`과 `ACS`를 지원하지만 group motion은 계속 `None`만
    지원한다. `ACS` 선택 중에는 두 Move 버튼이 비활성화된다.
11. `1 Power On`을 누른다. PASS는 mode-change 요청이 수락됐다는 뜻이며
    `_ROBOT_ACTIVE` 완료를 뜻하지 않는다.
12. `2 / 5 Read Status (Power Ready / Lock Ready)`를 반복해 `PowerOn=True`를 확인한다.
    프로젝트 로컬 확장 state `0x00040000`만 Power On 완료를 뜻한다.
    확인 전에는 Set Identity, profile lock과 Move 버튼이 활성화되지 않는다.
13. 네 축 이름을 확인하고 `Home Check (X/Y/Z/U)`로 각 축의
    `Home/Referenced=True`를 확인한다. 이 버튼은 진단용이며 생략해도 된다.
14. `3 Set Identity (Auto Home Check + Configure)`를 실행한다. Set Identity는
    같은 Home Check를 다시 수행하며, 한 축이라도 reference되지 않았으면 PLC에
    kinematics 설정 명령을 보내지 않는다.
15. `4 Enable (Lock Profile)`을 실행한다. PASS는 Lock API 성공이며 최종
    Locked/Standby 확인은 아니다.
16. `2 / 5 Read Status (Power Ready / Lock Ready)`를 다시 실행해
    `Enabled/LockedStandby=True`를 확인한다. 이 확인 뒤에만 Move가 활성화된다.
17. 작은 X/Y/Z/U 목표로 `6 Move Linear Absolute`를 먼저 시험한다.
18. `0x7D00`에 `GroupLinearRelative`가 광고된 최신 PLC에서 X/Y/Z/U를 작은
    delta로 바꿔 `6 Move Linear Relative`를 시험한다. PASS는 profile queue 수락이며
    화면의 Group InPosition monitor가 완료될 때까지 기다린다. monitor timeout은
    XYZU 거리, velocity, acceleration과 deceleration으로 계산하며 15~600초로 제한한다.
    축 순서 검증 capture는 나머지 세 delta를 0으로 두고 한 축씩 왕복한다.
19. 종료 순서는 Group Stop 및 InPosition 확인, `Disable (Unlock Profile)`,
    `7 Power Off`, `7 Verify Power Off (Read Status)`에서 `PowerOn=False` 확인이다.
    Power Off 확인이 끝날 때까지 Read Position은 비활성화되고 Read Status로 focus가
    이동한다.

`Move Linear Relative`는 PC에서 현재 위치와 delta를 더하지 않는다. Admin `0x7D22`로
delta를 보내고 PLC가 `MoveRelativeCoord`를 호출한다. 2026-07-23 실기 capture에서
Aborting 수락, Buffered 수락, X/Y/Z/U 축별 왕복, Stop 및 PowerOff 최종 상태를 확인했다.
이 수동 capture는 각 명령의 수락 확인이며 진짜 Buffered queue chaining이나 stop-first
우선순위의 증거는 아니다. 두 시나리오를 재현하는 Qualification runner는 구현됐지만
실물 PLC와 packet capture 합격 판정은 아직 수행하지 않았다. fault/stale-session matrix도
계속 별도 검증 항목이다.
기존 `09_Group_ReadPosition_None_ACS_2051`은 Read Position이 아니라 Read Status를 두 번
실행해 `0x2051`이 없으므로 None/ACS 증거가 아니다. 수정한 `09b`에서는 None과 ACS의
`0x2051` request/response가 모두 PASS했고 두 결과가 같은 static member-slot 순서임을
확인했다. 이는 실제 좌표 변환이나 MCS/PCS 지원 증거가 아니다.

## Qualification 자동화

Group Motion, Bulk Snapshot, Recorder와 SDO / Write Policy 탭에는 한 번에 하나만 실행되는 Qualification
runner가 있다. 실행 전 Connect를 완료한다. Group Enable 시험은 Power Ready/Set Identity
후 Disabled/Unlocked에서 시작하고, Buffered/Stop-first는 Locked Standby까지 준비한다.
Diagnostics는 `Refresh Capabilities`와 `Load PI Catalog`까지 완료한다.
수동 UI가 보유한 Bulk/Recorder resource는 먼저 Release해야 한다. 각 단계는
`QTEST|utc=...|elapsedMs=...|run=...|scenario=...` 형식으로 Execution Log와 탭별
결과창에 기록되며 `Save QTEST Log`로 저장할 수 있다.

- `Group Enable accepted -> locked`는 PowerOn + Disabled/Unlocked 3회 안정 상태에서
  `0x2047` ACK를 한 번 받은 뒤 `0x2045`의 Locked Standby를 3회 확인한다. 성공 후
  profile은 잠긴 상태로 남으며 runner가 자동 Disable/PowerOff하지 않는다.
- `True Buffered A -> B`는 선택한 X/Y/Z/U 한 축의 software min/max와 group dynamics
  limit를 먼저 확인한다. 같은 방향의 제한된 raw delta A/B를 `Buffered`로 보내되 A가
  아직 InPosition이 아닐 때 B를 송신하고, 누적 endpoint와 stable InPosition을 확인한
  뒤 Aborting relative move로 시작 위치에 복귀한다. 실패나 취소로 motion 가능성이
  남으면 Group Stop을 보내고 stable InPosition까지 확인하며, 이 cleanup도 실패하면
  안전 상태 미확정으로 FAIL한다.
- `Deterministic Stop-first`는 app send gate를 잡은 상태에서 zero-delta Move와 Group
  Stop을 순서대로 대기시킨다. safety generation이 Move delegate를 wire 송신 전에
  취소했는지와 Group Stop ACK/stable Standby 3회를 확인한다. Stop 전송, local assertion,
  stable-state 검증 중 하나라도 실패하면 gate를 먼저 반환하고 non-cancelable fallback
  Group Stop + stable Standby 3회를 실행한다. fallback 실패는 원 오류와 함께 보존한다. 로그의
  `moveWireExpected=0`은 local assertion이며 실제 `0x7D22` 0건/`0x2085` 1건 판정은
  Wireshark capture가 필요하다.
- Bulk Snapshot Soak는 fresh capability/Catalog에서 정확히 24개 BulkReadable signal을
  Catalog 순서로 configure하고 Active까지 bounded poll한 뒤 기본 100회 snapshot을
  읽는다. identity, 24-entry order/type/valid/detail, SameCycle + InputMapped flags,
  even sequence, wrap-aware cycle/timestamp/sequence와 latency를 검사하고 `finally`에서
  Release한다. Lifecycle Soak는 기본 100회 `Configure -> Active -> Snapshot -> Release`,
  종료 후 새 Configure 재사용과 두 번째 Release의 local 차단까지 검사한다.
- Bulk One-Slave-Offline Partial은 먼저 loaded Group의 `PowerOn=False`, `Disabled=True`,
  `Standby=False`를 확인하고, baseline 직후 checkpoint를 열기 전에 같은 상태와 4축
  actual-position 3회 동일값을 다시 확인한다. 첫 checkpoint에서 사용자가 승인된 외부 방법으로 정확히
  한 slave를 `Online=False`로 만든 뒤 Resume하고, 24-entry 응답 중 그 SourceIndex의
  6개만 `SlaveOffline` bit/Detail 18이며 나머지 18개가 exact Valid인지 확인한다.
  PLC가 OR하는 `SlaveNotOperational`/`AlError` 같은 추가 상태 bit는 허용하지만 Valid bit는
  허용하지 않는다. 첫 Partial이 다른 축까지 invalid이면 즉시 실패한다. 두 번째
  checkpoint에서 같은 slave를 OP로 복구한 뒤 Resume하면 최대 15초 동안 최종
  24 Valid를 기다리고 Release한다. 프로그램은 EtherCAT fault를 직접 만들지 않는다.
- Recorder Single은 Catalog 순서의 첫 4개 Recordable signal, period 1 cycle, capacity
  1000으로 자연 `SampleCountComplete`를 기다린다. Header와 두 번의 Download가 같은
  identity/order/16,000 bytes/SHA-256인지 확인하고 buffer/configuration을 Release한 뒤
  두 번째 Release의 local 차단을 검사한다.
- Recorder Ring은 같은 4 channel에 capacity 1000, pre 100, post 899의 Edge 설정을
  사용한다. 자동 edge가 일어나지 않는 threshold로 구성하고 pre-history 뒤
  `TriggerRecorderAsync`를 보내 `TriggerComplete`, TriggerIndex 100, 1000 samples와
  exact download coverage를 검사한다. Trigger Lifecycle Soak는 capacity 32,
  pre 16/post 15를 지정 횟수 반복하며 ResourceBusy, dropped, overflow가 모두 0인지
  집계한다.
- Recorder Reconnect Exact/0/0 Discovery는 같은 Ring 설정을 active 상태로 만든 뒤
  BootId/RecordId/BufferId/config revision/signal order를 보존하고 앱 RPC connection을
  실제로 close/reopen한다. 새 capability의 BootId/MapRevision을 대조하고 exact 또는
  single-bank 0/0 discovery로 새 OwnerSessionEpoch를 얻은 뒤 Status, 필요 시 Stop,
  Header, Download와 adopted identity 기반 buffer/configuration Release를 수행한다.
  Start ACK 직후 exact recovery identity를 먼저 보존하므로 pre-history Status의 transport
  fault도 실제 connection 상태에 따라 exact reconnect cleanup으로 회수한다. 두 버튼은
  별도 run이다. RecorderDoubleBank와 fault 주입은 이 runner 범위가 아니다.
- D5 SDO Abort -> Recovery는 SDO Read만 사용한다. 선택 slave에 대응하는 `_LMCAxis1..4`가
  `PowerOn=False`, `Standstill=True`이고 actual position 3회가 동일한지 확인한 뒤,
  `0x6061:0 Int8/1` baseline을 읽고 사용자가 제조사 기준으로 선택한 존재하지 않는
  read-only object/subindex를 조회한다. PASS는 abort ticket이 `Failed/Failed`,
  `OperationErrorId=-32000`, `OperationDetail`에 실제 nonzero raw EtherCAT SDO abort code,
  result 없음이고, 같은 BootId/MapRevision의 새 `0x6061:0` ticket이 baseline과 같은 값으로
  성공해야 한다. Cancel Runner는 PLC Stop을 보내지 않는다. 실제 Queued ticket만 cancel하고
  Running ticket은 원래 terminal deadline을 고려한 15~120초 bounded wait로 회수한다.
  코드는 build/test 완료했지만 실제 PLC abort/recovery와 pcap은 아직 검증하지 않았다.
- D5 Submit 호출 직전에 outcome guard를 먼저 등록한다. 명시적인 PLC rejection이면 제거하지만,
  submit 응답이 유실되거나 transport 결과가 불명확하면 ticket ID를 모르는 evidence도
  quarantine에 보존한다. ticket 응답을 받은 뒤 guard 제거가 확인돼야 일반 active-ticket
  추적으로 전환한다.
- 보존 ticket을 같은 `LMCConnection`으로 Resolve할 때도 capability BootId를 먼저 대조한다.
  BootId가 바뀌었거나 status가 정확히 `BootIdMismatch`를 반환하면 old ticket을 조회·해제한
  것으로 간주하지 않고 stale-session quarantine으로 이동한다. local ticket의 session
  generation이 stale인 경우도 quarantine한다. 같은 Boot/session에서 status가 정확히
  `TicketNotFound`이면 one-terminal-slot 교체 계약상 이전 ticket terminal만 확정하고
  `TERMINAL_INFERRED`, outcome `UNKNOWN`으로 해당 ticket을 해제한다.
  여러 known/unknown evidence는 그대로 유지한 채 stable BootId/MapRevision 아래 current
  capability가 GeneralInline이면 서로 다른 두 `0x6061:0 Int8/1`, legacy SDORead-only이면
  서로 다른 두 `0x1000:0 UInt32/4` ticket의 exact type/length/bytes를 모두 확인해야 해제된다.
  proof scope는 정확히
  `same_owner_connection_recovery`, `new_diagnostics_boot_session`, `new_connection_session`
  세 가지다. 같은 owner object+Boot의 unknown outcome 또는 PLC
  `HandleOrGenerationStale(10)`은 첫 scope이며 old terminal이나
  disconnect/orphan 증거가 아니다. 같은 owner의 Boot 변화는 둘째 scope이고 역시 orphan
  PASS가 아니다. 모든 격리 evidence의 owner `LMCConnection`이 현재 owner와 달라 셋째
  scope가 성립하면 `newConnectionRecovery=true`로 기록한다. WPF는 항상
  `orphanQualified=false`를 기록한다. 셋째 scope는 새 RPC connection에서 application
  recovery가 성립했다는 뜻일 뿐 PLC 내부 orphan cleanup이나 late callback을 증명하지 않는다.
  실제 orphan PASS에는 known Running old ticket, 실제 owner loss와 별도 PLC hook/capture가
  필요하다. QTEST는 `evidenceBootIds`, `recoveryBootId`, `proofScope`,
  `newConnectionRecovery`, `orphanQualified=false`를 분리 기록한다.
- unresolved ticket/evidence가 하나라도 있으면 Configure Bulk, Recorder Configure/Adopt/
  Start/Trigger, Group Disable, motion/PowerOn/Reset, manual SDO/PI, Close와 모든 다른
  qualification 같은 새 mutation을 차단한다. 기존 resource의 Bulk Release, Recorder
  Stop/Release, queued diagnostic Cancel, motion Stop/PowerOff와 read-only는 허용한다.
  `Resolve Preserved D5 Ticket`은 같은 session/새 Boot session에서도 바로 실행할 수 있다.
  reconnect 자체는 외부 connection loss 뒤에만 허용하며 새 connection에서 Resolve한다.
  `D5SdoPendingCleanup` Resolve는 기존 `qualificationLogLines`를 지우지 않고 이어 쓰며
  `D5_LOG_CONTINUATION`을 기록한다. 따라서 원래 `FAIL`/`OUTCOME_UNCERTAIN`과 resolution
  proof가 같은 저장 QTEST log에 남는다.
- qualification 밖의 manual SDO/Drive read tracker는 직전 qualification의 run/scenario를
  재사용하지 않는다. 별도 `D5ExternalTracking:<stage>` run ID와 step/elapsed 문맥으로
  기록하고, unresolved evidence가 생기면 그 원본 문맥을 Resolve log와 함께 보존한다.
- `GetDriveOperationMode[Async]`/`ReadDriveStatus[Async]`의 diagnostics domain command 실패는
  `LMCSdoReadCommandException`이 `CapabilityPreflight`/`Submission`/`StatusPolling`을 구분한다.
  WPF는 앞의 두 pre-ticket rejection이면 guard를 해제하고, status failure이면 exception의
  accepted ticket을 보존한다. transport/malformed/local-session 같은 나머지 예외에는 아직
  all-failure stage/ticket context가 없으므로 unknown evidence로 안전하게 fail-closed한다.

Recorder qualification의 자동 cleanup은 final Status가 `Ready` 또는 이미 frozen
download가 시작된 `Uploading`일 때만 buffer와 configuration을 Release한다. `Fault`는
frozen success로 취급하지 않고 자동 Release하지 않는다. QTEST에 recovery-required를
남기고 identity/resource를 보존하므로 사용자가 Status/error를 진단한 뒤 명시적으로
복구해야 한다. 보존된 resource는 수동 UI에서 격리되며 Status 확인 전 Stop/Release를
막는다. 확인 상태가 Armed/Recording이면 사용자가 `Release Recorder`를 눌렀을 때
Status -> Stop -> Ready/Uploading poll -> buffer/configuration Release를 수행한다.
Fault/Empty는 mutation을 계속 막고, buffer가 이미 해제된 config-only tail은 Status 없이
Release retry를 허용한다.

`Cancel Test`는 다음 RPC 전 취소를 요청하는 기능이다. Reconnect qualification의 명시적
close/reopen 단계를 제외하면 PLC transport를 중간에 끊지 않는다. Reconnect close 뒤
취소/실패가 발생하면 보존한 exact identity로 non-cancelable reconnect/adopt cleanup을
시도한다. Qualification의 각 wire dispatch는 공용 send gate를 얻은 뒤 시작 시점의
safety generation과 token을 다시 확인한다. 이미 dispatch된 단일 RPC는
`CancellationToken.None`으로 결과를 끝까지 받고, Recorder download는 header와 각 chunk
사이에서 gate/token을 다시 확인한다. Bulk Catalog public helper는 연결 강제 종료를 피하기
위해 하나의 bounded compound operation으로 gate 안에서 완료한다. cleanup은 별도
non-cancelable gate 경로에서 진행 중인 Stop/Power Off send/monitor가 끝난 뒤 실행한다.
Group Stop/Power Off를 누르면 진행 중 Group
qualification을 취소하고 외부 safety 결과를 먼저 검증하며 필요하면 cleanup Group Stop으로
fallback한다. 화면/SDK build와 정적 계약 통과는 실제 queue 실행, RT sample 무손실,
packet 순서 또는 장비 안전을 대신하지 않는다.

현행 Debug visual/startup smoke에서는 Group/Bulk/Recorder qualification panel 렌더와
prerequisite 미충족 초기 실행 버튼 disabled를 확인했다. D5 runner 포함 Debug/Release
build도 PASS했지만 D5 panel visual smoke는 대기 중이다.
이 smoke/build는 실제 PLC qualification 실행이나 packet 검증 결과가 아니다.

## EtherCAT / PI / Bulk / Recorder 시험 순서

1. Connect 뒤 `Refresh Capabilities`를 먼저 누른다. PLC가 광고하지 않은 기능의
   버튼은 활성화되지 않는다.
   현재 internal test source의 정상 retained 경로는 `CapabilityBits=0x0000213F`,
   `MapRevision=0x957F101E`, nonzero `DiagnosticsBootId`, `MaxSdoDataBytes=4`다. bit 5
   `RecorderTrigger`, bit 8 `SDORead`와 bit 13 `SDOReadGeneralInline`은 활성이고 bit 6 `RecorderDoubleBank` 및
   D5 bit 7 `PIWrite`, bit 9 `SDOWrite`, bit 12 `ExtendedSdoResultChunk`는 0이다.
   Phase 1 PI Write는 이 capability-off와 별도로 SDK compile-time allowlist가 empty이고,
   WPF도 `Phase1AllowsPiWrite=false`로 입력/button을 끈 뒤 click handler에서 다시 거부한다.
   BootId 5 축 1~4 capture는 당시 `0x13F`와 `0x1000:0` UInt32 4-byte legacy 경로를
   확인했다. 최신 BootId 8 `0x213F` capture는 general-inline Int8/1,
   BitField16/2, UInt32/4와 동일 BootId TypeMismatch 후 복구를 확인했다.
2. `Read EtherCAT Health`에서 master state, invalid-cycle counter와 slave 1~4의
   Online/AL/DS402 상태를 확인한다.
3. `Load PI Catalog`로 현재 map revision과 active PDO signal을 받은 뒤 사용할
   signal의 `Use`를 체크한다. 기본 선택은 네 축의 `actual_position`이다.
4. `Read Selected PI`는 SDO가 아니라 PLC가 publish한 최신 cyclic image를 읽는다.
   Raw Value와 Entry Status를 함께 확인한다.
5. Bulk 탭은 `1 Configure Selected` -> `2 Refresh Status` -> `3 Read Snapshot` ->
   `4 Release` 순서다. Status가 Active인지 확인한 뒤 snapshot을 읽는다. 모든
   entry의 cycle/timestamp는 하나다.
6. Recorder 탭은 선택된 `Recordable` signal, sample period와 capacity로 Recorder를
   configure/start한다. `Single + Manual`은 D3 기본 경로다. 현재 D4 경로는 한 개의
   물리 bank를 사용하는 `Ring + Edge/Window/Mask`이며 RT signal 조건 또는
   `Trigger Now`로 발생한다. Double mode는 bit 6이 0이므로 Configure할 수 없다.
   Window는 `TriggerValue=lower bound`, `TriggerMask=upper bound` wire 계약을 사용한다.
   public SDK는 Int16/UInt16/Int32/UInt32 bound를 검증하지만 현재 24-entry PLC Catalog에서
   Window로 실행 가능한 signal은 Int32다. 현재 PLC에서 Edge는
   Int32/BitField16/BitField32, Mask는 BitField16/BitField32 signal을 사용한다.
   lower가 upper보다 크면 송신 전에 거부한다. Mask trigger는 `TriggerValue=0`으로
   강제되며 사용자가 입력하는 값은 nonzero `TriggerMask`뿐이다.

   현재 고정 bank는 1,280,000 bytes다. capability의 `MaxRecorderSamples=320000`은
   1채널 절대 상한이며 실제 sample 상한은 Configure 응답의 `AcceptedCapacity`다.
   계산식은 `floor(1280000 / (channelCount * 4))`이고 16채널은 20,000,
   24채널은 13,333 samples까지다.

   `Trigger Now`는 locally configured non-Manual Ring recorder를
   `TriggerRecorderAsync`로 명시적으로 trigger할 때 사용한다. pre-trigger history가
   채워진 뒤 RT sample 경로가 요청 sequence를 적용한다. reconnect로 Adopt한 identity와
   Manual configuration에는 사용할 수 없다. 자동 Edge/Window/Mask 조건은 EtherCAT
   master가 OP이고 consecutive invalid cycle이 0이며, trigger 축이 Online/OP이고 AL code가
   0일 때만 평가한다. 조건이 유효하지 않은 cycle은 이전 trigger history를 지워 잘못된
   edge/window 전이를 만들지 않는다. `Trigger Now`는 이 입력 유효성 gate와 무관하게
   허용되지만 pre-trigger sample이 준비된 뒤 RT에서 적용된다.

   `Trigger Now`와 `Stop`의 성공 ACK는 RT 적용 완료가 아니라 각각 request sequence를
   publish했다는 뜻이다. 다음 RT
   `AppendSnapshot`이 Stop을 관찰하면 sample copy 전에 즉시 freeze하므로 Stop 관찰
   cycle의 sample은 추가되지 않는다. sample이 이미 있으면 Header의 End cycle/timestamp는
   마지막으로 복사된 sample을 가리킨다. 자연 trigger 완료, Trigger Now, Stop이 terminal
   전환 근처에서 경합할 수 있으므로 최종 결과는 이후 `Read Status`의 `StopReason`과
   `TriggerIndex`를 기준으로 판단한다.
7. finite capture가 끝나거나 Stop한 뒤 Status가 Ready인지 확인하고 `Read Header` 또는
   `Download`를 실행한다. Header에는 sample/cycle/trigger/CRC metadata가 표시된다.
   현재 PLC는 `DataCrcPolicy=None`, `DataCrcPresent=false`, chunk `DataCrc32=0`이다.
   `DroppedSamples`와 `OverflowCount`도 현재 예약 필드라 항상 0이며, 이 값만으로 실기
   손실 없음이 증명되지는 않는다.
8. Start가 표시한 `DiagnosticsBootId`, `RecordId`, `BufferId`는 disconnect 후에도
   입력칸에 유지된다. 같은 PLC boot로 reconnect한 뒤 Capabilities를 갱신하고
   nonzero RecordId를 그대로 두고 `Adopt`하면 저장한 exact identity로 frozen Recorder 또는
   active Ring의 control을 회수한다. 앱 crash나 Start response 유실로 RecordId를 모르면
   BootId만 확인한 뒤 RecordId와 BufferId를 모두 0으로 입력하고 `Adopt`한다. 이 경로는
   `AdoptActiveRecorderAsync`로 현재 한 개의 single-bank Recorder를 discover하고 새 session이
   control을 넘겨받는다. active Ring은 Status/Stop, frozen Recorder는
   Status/Header/Download/Release를 계속한다. PLC가 reboot되어 BootId가 달라졌다면 두
   adoption 경로 모두 의도적으로 거부된다.

   zero-ID discovery는 `RecorderBufferCount=1`인 현재 internal test build에만 정의된다.
   RecordId=0, BufferId=nonzero 조합은 거부되며 Double bank가 활성화되기 전까지 한 번에
   하나의 current Recorder만 발견한다. exact recovery와 운영 추적을 위해 Start 성공 시
   identity를 로그나 별도 상태 파일에 보존하는 방식도 계속 권장한다.

   SDK는 이 앱에서 Configure/Start한 local identity에 대해서는
   `TriggerIndex=PreTriggerSamples`와 TriggerComplete의
   `SampleCount=PreTriggerSamples+1+PostTriggerSamples`를 Status/Header에서 검증한다.
   Adopt response에는 원래 pre/post shape가 없으므로 adopted identity에는 이 exact shape를
   추측해 적용하지 않고 frozen wire invariant만 검증한다.
9. download된 immutable PC data는 signal별 downsample plot으로 확인하고 CSV로
   저장할 수 있다. CSV 앞부분에는 Recorder identity, map/cycle/timestamp와 채널별
   SignalId/alias/type/unit/scale metadata가, 이어서 `sample_index`,
   `relative_time_us`, channel raw value가 기록된다. PLC buffer/config는 `Release`로
   명시적으로 반환한다. 단, 자동 반환은 Status가 `Ready` 또는 이미 frozen download가
   시작된 `Uploading`일 때만 허용한다. `Fault`면 자동 Release하지 않고 identity와
   resource를 보존해 명시적 진단/복구 대상으로 남긴다. Adopt한 Recorder는 `Release`가
   필요할 경우 Status metadata를 먼저 복구한 뒤 같은 상태 gate를 적용한다.
10. SDO 탭의 general-inline flow는 `Submit SDO Read -> Refresh Ticket` 순서다. 새 PLC
    test build를 download하고 `Refresh Capabilities`에서 bit 8 `SDORead`, bit 13
    `SDOReadGeneralInline`과 `MaxSdoDataBytes=4`를 확인하면 Submit이 활성화된다.
    Slave 1~4, nonzero ObjectIndex, 임의 U8 SubIndex를 입력하고 ValueType에 맞춰
    1-byte(Bool/Int8/UInt8/BitField8), 2-byte(Int16/UInt16/BitField16) 또는
    4-byte(Int32/UInt32/Real32/BitField32) Read를 제출한다. terminal 상태까지
    `Refresh Ticket`을 반복하고 inline 결과를 `Save Result`로 저장할 수 있다.
    SDO Write, 8/12-byte Read, `0x7E51` extended result와 PI Write는 계속 비활성이다.
    축 1~4 `0x1000:0` UInt32 4-byte legacy와 general-inline 1/2/4-byte Read의
    PC-PLC ticket/inline success를 확인했다. `12_SDO_GeneralInline_4Byte_FailureRecovery`
    에서는 TypeMismatch 실패 뒤 같은 BootId 복구도 PASS했다. read-only abort -> recovery
    qualification은 code/build와 analyzer test만 완료했다. 실제 abort/pcap, offline, timeout,
    queued cancel, disconnect/orphan, deliberate contention과 EtherCAT mailbox frame
    독립 관측은 production qualification으로 남아 있다.

## Read-only API 시험 순서

이 탭은 Phase 1의 신규 읽기 API를 실물 PLC에서 확인하기 위한 화면이다. motion이나
write command는 없다. `0x7D00/0x7D10/0x7D20`과 physical axis 1~4 drive read의
2026-07-23 happy-path wire capture는 PASS다. 새 PLC build에서는 아래 순서를 다시 실행한다.

1. Connect 뒤 `Refresh Admin Capabilities`를 먼저 실행한다. 성공 응답의 feature,
   axis/group mask, physical axis count, fixed group reference `0x0100`과 error catalog
   version을 확인한다. 이 응답이 없거나 기능 bit가 없으면 후속 Admin 버튼은
   fail-closed 상태로 남는다.
2. `Read Axis Parameter`에서 physical axis 1~4와 6개 semantic key를 순서대로 읽는다.
   결과의 axis reference, key, signed Int32 value, type과 unit을 같이 기록한다.
   `EndPositionToleranceWindow`는 profile in-position 상태가 아니라 축의 end-position
   tolerance parameter다.
3. `Read Group Parameters`에서 `PathVelocityLimit`, `PathAccelerationLimit`, `JerkTime`,
   `All`을 각각 실행한다. 현재 v1 group reference는 `0x0100`으로 고정이다.
4. 탭 아래쪽 `Physical drive reads`까지 scroll한다. `1 Get Drive Operation Mode`는
   선택 축의 D5 SDO `0x6061:0 Int8/1` 결과와 ticket을 표시한다.
   `2 Read Drive Status`는 LASAL axis status, D5 SDO `0x6041:0`,
   `0x6061:0`을 순서대로 읽는다.
5. Drive Status는 같은 EtherCAT cycle의 atomic snapshot이 아니다. LASAL position-limit,
   axis error flag와 DS402 internal-limit bit는 서로 다른 출처이므로 한 값으로 합쳐
   원인을 추정하지 말고 화면에 표시된 각 source를 따로 확인한다.

현재 happy-path PASS를 invalid axis/key/selection, stale session, timeout과 fault 결과까지
확대 해석하지 않는다. 각 실패 경로는 별도 runtime matrix로 검증한다.

현재 PLC가 D0 capability(`CapabilityBits=0`)만 반환하면 EtherCAT/PI, Bulk, Recorder와
SDO 진단 기능은 정상적으로 비활성화된다. UI와 SDK가 존재한다는 사실이 PLC runtime
구현 완료를 뜻하지 않는다. Admin 기능은 별도 `0x7D00` capability 응답으로 판정한다.

## Load Axis 실패 진단

`_LMCAxis1`이 network의 실제 object name인데도 Load Axis가 실패하면 Execution
Log의 lookup 응답을 확인한다.

- `HeaderStatus=1`, `CommandStatus=1`, `ErrorId=-2`: 해당 LASAL client/name
  registry entry가 아직 준비되지 않았거나 입력 이름이 실제 object name과 다르다.
- `FrameValid=False`: PLC 배포본과 PC API의 response framing이 다르거나 TCP
  응답이 잘렸다.
- `HeaderStatus=0`, `PayloadLength=6`, `Reference=0`: 현재 LASAL dispatcher가
  허용하지 않는 구형/잘못된 descriptor 응답이다.

LASAL Online Debugger에서는 Load Axis 요청 직후
`TCPMotionInterface1.AxisObjectName1`, `GroupObjectName`과 각 client 연결
상태를 확인한다. `ObjectRegistryReady`는 Get Group Members 요청 때만 9축과
group entry가 모두 유효한지를 나타낸다. 축 5~9 이름도 CodeGenerator에 등록된
`AxisObjectName1`을 순차 scratch buffer로 사용한다. PC API와 LASAL 소스를 수정한 뒤에는 둘 다
rebuild하고 PLC에 최신 프로그램을 다시 download해야 한다.

LASAL runtime 생성 테이블은 object symbol을 대문자로 저장한다. 현재
dispatcher는 대소문자를 구분하지 않으므로 `_LMCAxis1`과 `_LMCAXIS1`,
`_LMCRobotBase1`과 `_LMCROBOTBASE1`을 각각 같은 이름으로 처리한다. 이 수정이
반영되지 않은 이전 PLC 배포본에서는 임시 확인용으로 전체 대문자 이름을 입력한다.

## 중요한 규칙

- DLL은 UNIT을 곱하거나 나누지 않는다. 이 예제의 Axis/Group UNIT 콤보가
  송신 전에 호출자 측 변환 방식을 선택한다.
- 기본 선택 `mm (x10000)`은 현재 저장된 `_LMCAxis1..9`의 `1 mm` macro와
  일치한다. `8,388,608`은 encoder 측 `ExUnits`이며 PC API UNIT이 아니다.
- `None / raw DINT`를 선택하면 정수 입력을 변환 없이 보낸다. 예를 들어
  `117440512`를 입력하면 같은 DINT가 전송된다. `mm` 선택에서 같은 값을
  전송하려면 `11744.0512`를 입력한다. Raw 모드는 소수 입력을 거부한다.
- UNIT 콤보는 PC 변환만 바꾼다. PLC의 software limit, MaxModulo, DS402 범위나
  실제 장비의 허용 이동 범위를 변경하지 않는다.
- 현재 Git 추적 PLC transmission은 `ExUnits=8388608`,
  `IntUnits=1 mm(10000)`다. offset 0 기준 external signed-DINT 좌표 상한은
  약 `255.9999 mm`이며, 기존 `+0x40000000` BinOffset이 남아 있으면 양의
  headroom은 약 `128 mm`다. 스케일 변경 후
  절대엔코더를 재참조하고 MaxModulo/BinOffset을 읽기 전에는 큰 이동을 시험하지
  않는다.
- 단축 continuous/endless motion은 비활성 SW limit 상태에서 MaxModulo overflow
  뒤 남은 거리를 계속 이동할 수 있다. Group `_LMCProfile`은 기본적으로
  명시적 SW limit가 없어도 `±MaxModulo`를 final endpoint로 검사하므로 별도다.
- Jerk 입력 단위는 `_LMCAxis`가 정의한 `axis application unit/s^3/1000`이다.
  UI는 입력값에 UNIT을 곱해 DINT로 보내며 기본값 `0`도 허용한다. 예를 들어
  물리 jerk가 `1000 mm/s^3`이면 Jerk 칸에 `1`을 입력하고 UNIT `10000`을 사용한다.
- 현재 저장된 `_LMCAxis1..9`는 `_JERK_PROFILE`, `JMax=75000 mm`다. nonzero
  Jerk 시험 전 다운로드된 PLC의 MoveType/JMax와 장비 허용 범위를 다시 확인한다.
- Group UNIT 콤보도 PC UI가 적용한다. 현재 static group은 X/Y/Z/U 4축이다.
  Read Position은 `Coordinate=None/ACS` member-slot alias를 지원하고 motion은
  `Coordinate=None`, `ExactStop`/`ContinuousDirect`, `Aborting`/`Buffered`만
  지원한다. `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`로 저장돼 있다.
- Group Power On/Off는 각각 `0x204A`/`0x204B`의 별도 API다. 두 ACK 모두
  mode-change 시작 접수일 뿐 최종 상태가 아니다. Group Read Status에서 각각
  `PowerOn=True`/`PowerOn=False`를 확인해야 한다.
- Group Read Status의 `0x00040000`은 Power Ready, `0x00010000`은
  Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Enable/Disable은 robot power 명령이 아니라 configured profile의
  Lock/Unlock 명령이다. Enable ACK 뒤 `Read Status`의
  `Enabled/LockedStandby=True`를 확인해야 Move가 활성화된다. Disable은 Stop이
  아니며, PLC는 group in-position이 확인되지 않으면 unlock을 `-6`으로 거부한다.
- Enable 뒤 성공한 Read Status가 `Disabled/Unlocked`를 3회 연속 보고하면 lock
  확인 대기를 해제하고 Enable 재시도를 허용한다. Read Status 자체가 실패하면
  화면의 Power Ready와 lock 판정을 무효화하고, 진행 중이던 lock 확인은 보존한다.
  성공한 Read Status로 상태를 새로 읽기 전에는 Power On과 Move를 허용하지 않는다.
  이후 `PowerOn=False`가 확인되면 identity와 lock 준비 상태도 지운다.
- Group Reset은 axis/hardware error reset이며 profile error 전체 reset이 아니다.
  Group Stop ACK도 정지 완료가 아니므로 두 명령 뒤 Group Read Status를 확인한다.
- Set Identity Kinematics는 generic kinematic transform 생성이 아니라 현재 4축의
  identity configuration을 준비하는 제한 구현이다. 실제 profile lock은 그 다음
  Group Enable에서 수행한다. Group 화면의 Home Check와 Set Identity 자동 검사는
  선택한 X/Y/Z/U 네 축의 `_LMCAXIS_STATUS.IsReferenced`(`0x00000002`)를 읽는다.
  상태 조회 실패와 `Home/Referenced=False`를 구분해 표시하며, 후자의 경우에도
  Set Identity 전송을 차단한다. 가상축 5~9는 Cartesian identity 대상이 아니므로
  이 검사에 자동 포함하지 않는다.
- Group Read Position은 wire상 DINT[16]이다. current PLC source는
  `_LMCPROF_POS`의 Pos1..Pos9를 slot 1..9에 복사하지만 기존 문서는 4축-only로
  설명했다. PLC 재캡처로 계약을 확정하기 전에는 slot 5..9를 production 값으로
  사용하지 않는다. 이 readback 문제는 4축 Move/SetKin/Lock 범위를 넓히지 않는다.
- Single Axis 탭은 object name 자유 입력 방식이므로 `_LMCAxis1`부터
  `_LMCAxis9`까지 한 축씩 Load해 동일한 Power/Read/Move/Stop/Reset API를 시험한다.
  이 지원 범위는 9축 동시 Cartesian group motion을 뜻하지 않는다.
- `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 없어 이 예제에 없다.
- 속도, 가속도와 감속도는 양수를 입력한다. Stop은 Stop deceleration과 현재
  Jerk 입력만 사용하므로 다른 motion 입력값과 독립적으로 실행된다.
- 유한 이동 완료는 non-standstill 관측 후 Standstill 3회로 판단하며, 감시 중에도
  Stop과 Power Off를 사용할 수 있다.
- Group Stop은 LASAL `StopMove(Mode:=3)`으로 감속 정지하고 기존 profile buffer를
  폐기한다. 정지 뒤 새 Move는 허용된다. Move 응답 `ErrorId=7`은 재시작 금지가
  아니라 `_LMCPROF_SWE_ERROR`이며, 해당 시점의 목표가 런타임 software end
  position 검사에 걸렸다는 뜻이다. 예제 로그의 `StartRaw`/`TargetRaw`와 LASAL의
  `AxReadSWEndPos`, `ReadProfileError().SubErrorNo`를 대조한다.
- Close와 창 닫기는 Stop이 아니다.
- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 체크박스나
  확인창 없이 버튼 클릭 시 즉시 송신된다.
- motion 가능성이 남은 상태에서 창을 닫아도 확인창이나 자동 Stop 없이 연결을
  종료한다. 실제 축 정지는 사용자가 Stop, Power Off 또는 외부 장치로 확인한다.
- motion/제어 command의 실행 중 Cancel 기능은 제공하지 않는다. API timeout은 기본
  3초다. Recorder의 `Cancel Download`는 이미 frozen된 데이터를 PC로 복사하는 작업만
  취소하며 PLC recording이나 motion을 정지시키지 않는다.
- callback log는 raw UDP diagnostic data이며 motion 완료 판정이 아니다.

활성 command mapping은 `API_MAPPING.md`, 구현 판단과 안전 설계는 `DESIGN.md`를
참조한다.

# LASAL Motion Control API Automated Tests

작성일: 2026-07-10

최종 결과 재확인: 2026-07-31

## 구성

외부 NuGet package가 없는 .NET Framework 4.8 console runner다.

경로:
`LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests`

실패가 하나라도 있으면 process exit code `1`을 반환한다.

## 범위

- 모든 현재 request builder의 literal golden bytes
- caller DINT가 DLL에서 재스케일되지 않는지 확인
- common envelope와 malformed/truncated/trailing frame
- exact 4-byte/8-byte ACK
- name lookup의 exact 6-byte/nonzero reference, `LMCLookupResult`와 structured
  `LMCLookupException`, sync/async Axis/Group 실패 및 raw 방어 복사
- typed Axis ReadStatus, DINT ReadActualPosition, GroupReadStatus
- captured PMAS ReadStatus/GroupReadStatus raw golden for envelope/offset
  compatibility; canonical LASAL state-bit semantics are documented separately
- exact 1350-byte `0x20D2`, count 0..16, name/array defensive copy
- `0x2051` None/ACS coordinate request, MCS/PCS/unknown enum 거부와 exact 68-byte
  LASAL-DINT `DINT[16]+status/error` typed result, slot 1..9/10..16 zero 및 배열 defensive copy
- legacy `0x2051` 136-byte LREAL response와 malformed/trailing payload 거부
- `0x20E7` exact 1320-byte Cartesian4 payload, X/Y/Z/U axis reference와
  captured application-frame SHA-256 golden
- group position 1..16 길이와 slot 5..16 nonzero 거부, group coordinate/transition/
  buffer/execute whitelist 및 velocity/acceleration/deceleration/jerk validation
- GroupStop deceleration/jerk validation, LASAL `StopCmdNo` 비오류 계약과
  `GroupStopAndWaitForStableStandbyAsync`의 one-dispatch/status-only stable proof,
  reject/timeout/cancel/status failure/ACK 유실 evidence 및 zero automatic replay
- Group Reset stable member error-clearance 계약 시험을 current suite에 포함한다. valid
  `0x20D2` observed snapshot -> `0x2049` exactly once -> Resume round마다
  `0x2045` 1회와 pinned member 전원의 `0x2028` 1회, group/member all-clear 3회 연속,
  timeout/status failure의 same-session continuation 보존, accepted/outcome-uncertain
  Stop/PowerOff/Disable takeover의 terminal supersede, valid safety NACK restore와 Reset no-replay를
  고정한다. generic snapshot validation은 `1..16`개 nonzero/unique reference를 허용하며 expected
  topology/current PLC build attestation이 아니다. durable 범위는 prepared observer의 command-before
  record, exact endpoint/build/BootId/Map/group/member reconnect, fresh `0x20D2` attach와 recovery
  `0x2049` zero-replay를 포함한다.
- Axis Reset, Admin `GroupMoveLinearRelative`, D5 `SubmitSdo`/`CancelOperation` 및 Recorder
  Trigger/Stop의 delayed-ACK
  `ResultDiscarded`, priority GroupStop wire order와 connection reuse. accepted Submit은 exact
  ticket/BootId/MapRevision을 보존하고 Recorder buffer/configuration/recovered/adopted Release는
  `OutcomeUnverified`로 격리해 재사용과 destructive retry를 차단한다.
- Group Enable wait 35개: mutation/status gate, `0x2047`, 모든 `0x2045`와 delay의 단일 total
  deadline, pre-write `NotAttempted` zero-wire/reusable/no-mutation, post-write caller cancel drain과
  accepted publication, ACK 무응답 `OutcomeUncertain`/no-continuation/`Faulted`, status 무응답
  `Accepted`/exact-continuation/`Faulted`, 두 no-response의 `TransportInvalidatedAtDeadline`,
  rejected/no-continuation 및 commit-bound mutation/proof reset.
- RPC init, fragmented response, 실제 ephemeral UDP callback, close
- callback payload defensive copy와 controller IP가 아닌 UDP source 거부
- init status/shape, callback ACK status/shape와 truncated-response 실패 후
  socket/listener state cleanup
- options clone/timeout validation과 invalid reconnect 시 기존 session 유지
- close nonzero ACK 예외, response/error 보존과 local cleanup
- receive timeout 뒤 transport 폐기, `Faulted` 전이와 재사용 차단
- queued cancellation이 active RPC를 보존하고 in-flight cancellation은
  해당 transport만 폐기하는지 검증
- async init/close와 취소 가능한 axis/group factory 성공, reconnect 뒤 stale
  group handle 및 generation-bound exchange 거부
- axis lookup 뒤 AxisInfo success/malformed/command-error
- LASAL static contract: generated client count/entries, 9-axis network links,
  C#-ST critical offsets, 32-bit error truncation guards, legacy command block,
  `_JERK_PROFILE`/nonzero JMax와 Stop/Move Jerk 수신·전달 경로
- diagnostics D0 capability와 D1 Health/Catalog/PI Read, D2 Bulk, D3 single-bank
  Recorder request/parser 및 source contract
- 서로 다른 두 RPC connection session에서 Recorder exact/0/0 discovery Adopt,
  preserved identity, 새 OwnerSessionEpoch, Status metadata 복구, immutable download와
  adopted identity 기반 buffer/configuration Release 순서
- D4 single-bank Ring/Edge/Window/Mask/forced Trigger와 D5 general-inline SDO Read
  request/status/queued-cancel active LASAL source contract; D5 Write는 Axis 1 exact
  `0x2F00:24 Int32/4` active, Axis 2..4와 비승인 target fail-closed contract; D4 Double과
  extended result는 fail-closed
- immutable `EvaluateSdoWritePolicy`의 cached zero-wire evaluation, read-only approved-target
  snapshot과 connection/capability/identity/payload blocker matrix, PLC bit 9와 SDK target 승인의
  독립 표시 및 Axis 2..4/비승인 target sync/async Submit zero-wire
- Phase 1 Admin `0x7D00/0x7D10/0x7D20` golden/parser/fake-RPC, semantic key/mask,
  RequestId/session/capability와 LASAL source offset/method mapping
- Phase 2 Admin `0x7D22 GroupMoveLinearRelative` exact 104-byte golden,
  4-axis/parameter whitelist, strict ACK/native reject parser, sync/async fake-RPC,
  capability no-dispatch, stale generation과 LASAL `MoveRelativeCoord` state gate
- Admin `0x7D12 SetAxisPosition` dormant 계약은 exact 56-byte request/36-byte response,
  fresh DiagnosticsBuild/BootId/MapRevision, 4 x U32 client intent, target+expected
  actual-position CAS, prepare-time pinned RequestId와 atomic one-shot consume,
  owner/session/capability pre-wire gate, concurrent/pre-canceled zero-wire, valid dormant
  `InvalidState/detail 10`, no-replay와 outcome-uncertain exact-session fault를 검증한다. Detail 11
  `NativeCommandRejected`는 common `ErrorId=-6`만 허용하고 payload `P+24`의 full
  `_LMCAXIS_CMDERROR` `UINT32` bitfield를 typed 예외에 보존한다. positive/other ErrorId나
  모순된 applied/native 값은 malformed다. final publication race는 추가 command registration
  없이 exact old session만 invalidate하고 newer reconnect를 보존하도록 강화했다.
- Admin `0x7D14 ReadAxisSetPositionOutcome`은 restart-safe public recovery key, repeatable
  read-only 56-byte query, exact 92-byte terminal response, identity/intent/tuple echo와
  Succeeded/Rejected 조합을 검증한다. capability bit 5는 bit 3과 독립적으로 읽기만 먼저
  광고할 수 있지만 strict parser는 `bit 3 => bit 5`를 강제한다. current PLC는 bit 5와
  route/store가 모두 OFF다.
- Admin `0x7D13 StartAxisReference` dormant 계약 시험 범위: exact 56-byte request
  frame/48-byte payload golden과 32-byte response frame/24-byte payload strict parser,
  recipe `1`/`2`, mandatory `MaxTravel>0`/`TimeoutMs>0`, typed start-ACK-only surface,
  owner/session/capability pinning, execute-token/prepared-command one-shot, concurrent single
  dispatch와 pre-canceled zero-wire를 검사한다. capability bit 4 OFF, valid dormant
  `InvalidState/detail 10`, native reject full `_LMCAXIS_CMDERROR` bitfield, response-loss/
  malformed/publication failure의 exact-session fault와 newer-session 보존도 포함한다. LASAL
  static 범위는 exact offset/length/`REFR` token/parameter validation, physical reference input
  source 부재와 `_LMCAxis.MoveReference` call 0을 고정한다. WPF 실행 노출과
  `HomeDS402`/`HomeDS402Ex` 동등성은 시험 범위가 아니다. 이 범위는 신규 16개 계약 시험으로
  등록했다. 상세 기준은
  [Axis Reference LASAL-native dormant 계약](../../../docs/architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md)을 따른다.
- `GetDriveOperationMode`/`ReadDriveStatus`/`GetDriveErrorCode`의 physical axis 1..4,
  exact `0x6061`/`0x6041`/`0x603F` type과 width, DS402 Fault bit projection, terminal
  success/failure, one-attempt error-code evidence, `TimeoutCycles+32` bounded poll과
  ticket-preserving cancellation
- PI alias와 Bulk builder/reader의 exact MapRevision, entry validation, latest
  snapshot lookup, stale session/release 및 PC-local error domain catalog
- internal `negative-wire` 모드의 live 확인 gate, 고정 5개 diagnostics raw scenario,
  arbitrary/motion/Admin/write/SDO Submit/Recorder 차단과 exact 16-byte 오류 envelope 9개 계약
- D5 SDO abort terminal과 same-Boot known-valid recovery 순수 판정 12개: 기존 8개에
  generic exact recovery의 UInt32 성공과 type/value/length 거절 4개를 추가했다. abort는
  `Failed/Failed`, `OperationErrorId=-32000`, nonzero raw abort code, result 없음이며
  recovery는 새 ticket의 `0x6061:0 Int8/1` exact baseline 값 성공
- WPF build 범위에는 Submit 전 outcome guard/unknown-ticket quarantine, same-connection
  BootId 또는 MapRevision change, exact `BootIdMismatch`와 stale local session quarantine이
  포함된다. 같은
  Boot/session의 exact `TicketNotFound`는 terminal-slot 교체 계약상 이전 ticket terminal만
  증명하고 outcome `UNKNOWN`으로 해제한다. multi-evidence recovery는 GeneralInline이면
  서로 다른 두 `0x6061:0 Int8/1`, legacy SDORead-only이면 서로 다른 두
  `0x1000:0 UInt32/4` ticket의 exact type/length/bytes를 검증한다. unresolved Group Disable
  포함 새 mutation gate와 15~120초 deadline-aware cleanup도 포함된다.
  기존 Bulk/Recorder/queued-ticket cleanup, Stop/PowerOff와 read-only는 계속 허용한다.
  UI 독립 `D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합만으로
  scope를 순수 판정하고 MainWindow는 proof 시작/PASS 로그에 같은 decision을 사용한다.
  owner+BootId+MapRevision이 동질인 경우에만
  `same_owner_connection_recovery`, `new_diagnostics_identity_session`,
  `new_connection_session`으로 나뉜다. owner 또는 submission identity가 섞인 evidence는
  `mixed_evidence_sessions`이며 same/new session으로 세지 않는다. mixed도 two-ticket
  application recovery proof와 성공 시 quarantine clear는 허용한다. 모든 evidence가 한
  previous owner+identity에 속하는 `new_connection_session`만 decision의
  `NewConnectionRecovery=true`와 로그의 `newConnectionRecovery=true`를 뜻한다.
  WPF는 `orphanQualified=false`를 고정 기록하며 실제 orphan PASS에는 known Running old
  ticket, 실제 owner loss와 별도 PLC hook/capture가 필요하다. `D5SdoPendingCleanup`
  Resolve는 기존 qualification log에 `D5_LOG_CONTINUATION`을 이어 써 원래
  `FAIL`/`OUTCOME_UNCERTAIN`을 보존한다. 이 항목은
  analyzer 12개의 순수 상태 판정 범위를 넘어서는 UI/runtime contract이며 PLC live 증거가 아니다.
- D5 same-value SDO Write qualification 9개는 initial baseline Read -> fresh capability -> 첫 safe
  check -> operator confirmation -> unchanged exact pre-Write guard Read -> 최종 두 번째 safe check ->
  durable journal -> byte-identical Write 1회 -> guarded exact Readback 순서를 검사한다. baseline,
  guard, Write, Readback은 서로 다른 네 ticket이다. empty allowlist, capability off와 다른 target은
  preflight zero-wire이며 baseline/safety/confirmation/journal 실패와 changed guard value도 mutation
  0건이다. 반환된 Write ticket은 semantic validation 전에 durable journal/quarantine에 먼저
  adopt해야 한다. uncertain Write는 재시도하지 않고 readback mismatch 또는 journal arm 이후
  cancellation은 unresolved evidence를 남긴다. sentinel, 자동 restore와 Write replay는 없다.
  두 번째 Read는 race window를 좁히지만 compare-and-write를 원자화하지 않으므로 네 번째 operator
  confirmation인 명시적 single-writer 작업창이 필수다.
- Phase 1 facade의 diagnostics domain command 실패는
  `LMCSdoReadCommandException`이 `CapabilityPreflight`/`Submission`/`StatusPolling`과 accepted
  ticket을 구분한다. sync/async와 composite 두 번째 SDO status failure까지 PC 계약으로
  검사한다. 추가 all-failure 계약은 예외를 wrapper로 바꾸지 않고 원래 객체와
  타입을 보존하며 `LMCDriveReadFailureContext.TryGet`으로 읽는다.
  drive phase는 `FacadePreflight`/`AxisStatusRead`/`CapabilityPreflight`/`Submission`/
  `StatusPolling`/`ResultMaterialization`, `GenericSubmissionOutcome`은 공용
  `LMCSdoSubmissionOutcome` (`NotAttempted`/`Rejected`/`OutcomeUncertain`/`Accepted`)이다.
  기존 `SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`도 같은 값과 getter 형식을 유지하는지
  검사한다. 각 SDO attempt는 확보된 실제
  `DiagnosticsBootId`/`MapRevision`과 request, accepted ticket, 마지막 status를 스냅샷으로
  보존하며 capability identity 확보 전에는 0 sentinel을 사용한다.
- WPF external-read failure router 7개는 no-submit/명시적 rejection/terminal status에서
  guard 해제, `OutcomeUncertain`에서 quarantine, `Accepted` nonterminal에서 exact ticket
  보존, context 누락·불일치에서 fail-closed를 검사한다.
- raw manual `SubmitSdo[Async]` context registration 7개는 원래 exception 객체/타입/stack
  보존과 `LMCSdoSubmissionFailureContext.TryGet`, `LMCSdoSubmissionPhase`
  (`RequestValidation`/`SessionPreflight`/`CapabilityPreflight`/`Submission`/
  `PostSubmissionValidation`), 공통 `LMCSdoSubmissionOutcome`, capability identity 확보 후의
  실제 `DiagnosticsBootId`/`MapRevision` 및 accepted ticket을 검사한다. identity 확보 전
  실패는 두 값이 `0` sentinel인지 검사한다. read/write model compatibility 항목의 write
  accepted context의 failure/race fixture는 synthetic tracker model로도 검사하며 실제 hardware
  write를 만들지 않는다. Axis 1 exact singleton은 fake-RPC로 request/adoption contract를 검사하고
  Axis 2..4와 비승인 target은 pre-wire로 거부한다. accepted-session-race 항목은 public sync/async `SubmitSdo` 호출에서
  실제 close 경합을 만들어 post-submit context 부착 순서를 검사한다. manual failure router
  1개는 no-submit/rejected/uncertain/accepted의
  disposition callback과 accepted ticket 보존-before-disarm 순서를 검사한다. MainWindow의
  실제 field 변경, identity reconcile log, UI 동작을 실행하는 시험은 아니다.
- D5 quarantine ledger 5개는 unknown arm/exact-once disarm, immutable evidence snapshot과
  identity reconcile revision, accepted ticket의 owner와
  `DiagnosticsBootId`/`SubmissionMapRevision` exact 전이,
  foreign/stale handle 및 duplicate ticket 거부, proof 중 두 accepted 임시 guard 허용,
  persistent evidence 변경과 candidate 이후 ABA 거부, PASS log callback 실패 시 무삭제를
  검사한다. `LMCOperationTicket.BelongsTo`의 owner reference 및 제출
  `SubmissionMapRevision` 계약도 이 경로에서 확인한다.
  PI Write는 SDK compile-time allowlist empty와 WPF button/handler 이중 차단을 적용한다.
  이 항목은 code/test 계약이며 PLC live/pcap 증거가 아니다.
- D5 recovery scope policy 7개는 same-owner exact/new identity, homogeneous previous owner,
  current+foreign owner 혼합, multiple previous owner 혼합, submission identity 혼합과 invalid
  input fail-closed를 검사한다. 이 UI 독립 policy source를 MainWindow와 PC test가 함께 쓴다.
- D5 quarantine ledger deterministic concurrency 4개는 각 등록 test를 50회 반복한다.
  candidate snapshot 뒤 clear 전 competing Arm이면 clear를 거부하고 두 evidence를 보존하는지,
  atomic clear callback 뒤 기다리던 Arm만 남는지, callback 예외 뒤 waiter가 진행하고 기존
  evidence와 ledger 재사용성이 보존되는지, concurrent Disarm이 성공 1회/stale 1회인지
  검증한다. 모든 동기화는 bounded wait이며 `Thread.Sleep`을 사용하지 않는다. 이 4개는 PC
  test 강화일 뿐 production/wire/LASAL 변경이나 PLC live/pcap 증거가 아니다.
- D5 pending cleanup orchestrator 9개는 owner/current connection, ticket owner와 저장
  `SubmissionMapRevision`을 dispatch 전 fail-closed하는지 검사한다. current capability는
  BootId를 먼저 판정하고 MapRevision mismatch와 함께 status/cancel 무송신 quarantine을
  보장한다. cached terminal은 status/cancel을 생략하고 cached pending은 refresh한다.
  Queued에서만 cancel하며 `InvalidState` race는 terminal까지 기다리고 Running은 cancel 없이
  기다린다. cancel accepted 뒤 exact `Cancelled/Cancelled`, fresh status의 caller 보존,
  command exception 원본 보존도 검사한다. wait 계산은 최소 15초, 남은 deadline+1초, 최대
  120초이며 elapsed가 timeout과 같은 `<=` 경계 poll도 고정한다. production WPF cleanup
  adapter가 이 UI 독립 source를 호출하지만 wire/LASAL 변경은 없고 PLC live/pcap 증거도 아니다.
- D5 disconnect/orphan UI 독립 코어 28개는 terminal-before-loss, Running/Queued nonterminal,
  distinct new owner/session, fresh capability 2회와 final sample, exact `0x6061:0 Int8/1`
  two-ticket recovery, capability cycle/payload drift, monotonic retry deadline, quarantine
  ABA/identity/cancellation/PASS-log failure 보존을 검사한다. old executor drain 중에는 request
  timeout + 5초, 최대 120초의 monotonic retry-admission budget에서 25 ms 간격 exact
  `Rejected/ResourceBusy`만 retry하고 accepted/outcome-uncertain은 재시도하지 않는다. 이미 시작된
  단일 RPC의 소요시간은 이 budget의 wall-clock 상한이라고 주장하지 않는다.
  production WPF adapter는 local TCP zero-linger close를 사용하며 RPC Close `0x405D`를 보내지
  않고, PASS log-before-clear 뒤 distinct new connection을 adopt해 CREVIS topology를 auto-load한다.
  결과는 Running witness가 있어도 항상 `ApplicationRecoveryOnly`/
  `orphanQualified=false`다. PLC `MarkOrphan`/executor token/late callback durable witness와 live
  PLC/pcap은 자동 시험 범위가 아니다.

PMAS legacy `0x202E` LREAL 16-byte와 `0x2051` LREAL 136-byte response는
LASAL-DINT typed parser가 명시적으로 거부한다. DINT actual-position
golden은 PLC 재캡처 전까지 contract 기반 synthetic vector다.

## 실행

PC C# test만 실행:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunPcTests /p:Configuration=Release /nologo
```

LASAL source static contract만 실행:

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunLasalContract /p:Configuration=Release /nologo
```

Topology/I/O 검사의 current project 기본값은 `IntegratedReadOwnerDormant`다. 명시적으로
같은 checkpoint를 재검증하려면 다음과 같이 실행한다.

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunLasalContract /p:Configuration=Release `
  /p:LasalTopologyIoCheckpoint=IntegratedReadOwnerDormant /nologo
```

full `RunLasalNetworkContract`에도 같은 property를 넘긴다. 이 checkpoint는 정확한 3개
client/network owner, 464-byte coherent snapshot, `0x7E13/0x7E22` route/handler와 최신
`Classes.lcb`/`Networks.lcb` 저장 증거를 요구한다. 동시에 capability bits 15~17은 OFF,
`0x7E23` route/owner는 부재하도록 강제한다. `AllowStaleLasalBinaryMetadata` 우회는 허용하지
않는다. `StaticTopologyOnly`와 `IdeStructureReady`는 구현 단계 경계 검증용으로 남아 있지만,
외부 read-owner implementation이 들어간 current source에서는 의도적으로 실패한다.

internal `topology-io-qualify` V2 dry-run도 별도 preflight다. explicit scope/mode를 요구하고
`integrated-read-owner-dormant`에서 정확히 17개 planned frame만 생성하며 network I/O는 0이다.
`0x7E23`은 allowlist 밖이고 실행파일/SDK SHA-256을 기록한다. live mode는 create-new report,
`HEAD/TRACKED/UNTRACKED` source fingerprint, nonzero BootId/build, exact
`MapRevision=0x957F101E`와 cleanup/result 뒤 2초 retention을 추가로 요구한다. 이는 PLC live
PASS가 아니며 실행법과 pcap 연계는 `TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md`를 따른다.

PC C# test, LASAL source static contract와 현재 WPF example build를 순서대로
실행하려면 target을 `/t:RunTests`로 바꾼다. 제거된 legacy
`LasalMotionControlLibTestApp`은 이 target에 포함하지 않는다.

현재 결과:

- `RunLasalContract`/`RunLasalNetworkContract`: current `Phase5TransportClean /
  IntegratedReadOwnerDormant`, `ExpectedSdoWriteAxis=1`와 dormant Admin `0x7D12`
  SourceOnly/full PASS. valid request는 `InvalidState/detail 10`, capability bit 3 OFF,
  native SetPosition call 0을 12개 negative source fixture와 함께 고정한다. LASAL IDE
  Rebuild/Link `0 error(s), 20 warning(s)`, Linker `Done`과 `LMCEcatInputLatch`,
  `LMCDiagnosticsService`, `TCPMotionInterface` direct implementation smoke, 신규
  `CInvalidArgException=0`은 이번 `0x405C` callback ownership/`0x7D12`/`0x7D13`
  편집 전 checkpoint다. current callback+`0x7D12`+`0x7D13` source는 fresh IDE Save/Rebuild/Link와
  `TCPMotionInterface`/`LMCControlCommandService` smoke가 대기 중이며, 두 결과 모두
  current PLC download/runtime 증거가 아니다.
- 신규 dormant `0x7D13` LASAL source/static contract는 exact 56/32-byte frame,
  recipe 1/2, capability bit 4 OFF, valid request `InvalidState/detail 10`, physical reference
  input source 부재와 native `MoveReference` call 0을 검사 대상으로 추가했고 current
  SourceOnly/full이 PASS했다.
- `RunPcTests` 대상의 2026-08-08 current Release PC suite는 .NET SDK 6.0.428
  (`dotnet build`, MSBuild 17.3.4+a400405ba)에서 warning 0/error 0이고 standalone
  runner `1111/1111` PASS다. callback tranche A는
  `CallbackProtocol.InitialV2WakeHint.EventAndDeliveryPolicy`의 D5 terminal EventId
  zero/nonzero 경계와 `Rpc.CallbackV2.D5TerminalTicketCorrelation`의 exact
  owner/session/BootId/ticket/type/mask/delivery/payload fail-closed matcher를 포함한다.
  이 결과는 PC fake-RPC/UDP 계약 검증이며 PLC callback publisher, PLC download/runtime 또는
  live packet capture 증거가 아니다. fake-RPC request/session을 한 lock에서 기록하고 stable snapshot으로
  관측하는 신규 회귀 1개, Reference 16개와 SetPosition 18개, Recorder accepted-result 6경로의
  sync/async 지연 ACK 12개는 exact typed resource/context, recovery-only normal-use 차단,
  configuration/lease의 Release-only cleanup, identity의 Status -> 필요 시 Stop ->
  Buffer/Configuration Release와 wire 1회/no-replay를 고정한다. Axis/Group typed lookup의 성공/실패,
  overlength 진단 의미와 public bounded inline SDO Read의
  typed terminal, pre-wire rejection, terminal-before-cancel, failure/timeout/cancel accepted-ticket와
  exact last-status 보존 및 immutable
  result 회귀 9개와 Axis/Admin/D5/Recorder delayed-ACK의 drain/
  `ResultDiscarded`, accepted Submit evidence 보존, Recorder Release `OutcomeUnverified`
  quarantine, 동일 handle의 concurrent Start/Release zero-extra-wire guard와 네 Release surface의
  `BeforeWire` usable rollback/retry, immutable zero-wire SDO Write policy/readiness matrix, proof reset, accepted Enable
  continuation 보존 및 `0x2047` zero-replay 회귀를 포함한다. config-only Double Configure 3개는
  Start 없는 exact retain, ambiguous-result recovery publication, checkpoint-failure exact lease 보존을
  고정한다.
  Group Reset 전용 계약은 `PendingGroupResetWaitContinuation`과 Begin/Resume/compound facade의
  `0x20D2 -> 0x2049 once -> (0x2045 + each 0x2028)` order, all-clear 3회, Resume epoch reset,
  group/member status fail-fast, same-session identity, same-group unsafe zero-wire interlock,
  pinned-member generation attribution, safety preemption/observer handoff, valid NACK continuation restore와
  no-replay를 포함한다. exact captured-member safety reconciliation의 generation mismatch
  terminalize/NACK rollback 보존/repeat false와 ACK parse 뒤 result-publication preemption, delayed NACK와
  active Resume 경합도 고정한다. raw `GroupReset[Async]`는 ACK-only로 남는다. 추가 durable 계약은
  prepared observer throw/reentrant zero-wire, defensive copy, exact member mismatch, invalid prior outcome,
  OperationId duplicate/concurrent/실패 뒤 재시도, generation 0 baseline과 group/member interference,
  attach cancel/timeout을 고정한다. attach는 fresh `0x20D2` 1회 뒤 continuation만 게시하며 Resume의
  `0x2049`는 0회다.
  Axis Reset accepted-once 전용 33개 계약은 Begin의 `0x2024` exact once/zero status, accepted
  continuation의 session/send-priority-bound atomic publication, Resume의 `0x2028`-only stable
  AxisError-clear proof, Resume epoch stable-count reset와 누적 poll, invalid/concurrent/stale
  continuation zero-wire, hard mutation/status/compound deadline, timeout/cancel/status-failure
  no-replay, response-loss/priority discard, same-axis mutation interference, status publication race,
  prior pending을 보존하되 response-loss mutation generation으로 무효화하는 Begin과 final proof의
  early/late cancel·deadline 선형화를 포함한다.
  Axis Power On accepted-once 계약은 mutation/status gate, ACK/status exchange와 poll delay를
  포함하는 total deadline, final pre-write cancel의 zero-wire `NotAttempted`/connection reuse,
  post-write cancel의 ACK drain/continuation/accepted-observer 선행 publication, ACK/status 무응답의
  `Faulted` + `TransportInvalidatedAtDeadline`을 고정한다. send-priority ACK discard는
  `OutcomeUncertain`, status discard는 accepted continuation pending을 보존한다. read-only
  `WaitForPowerStateAsync`의 no-status deadline도 transport를 invalidate하며, 성공 결과는 power
  command를 재사용하지 않았으므로 ACK/continuation 없음과
  `ReusedAcceptedAcknowledgement=false`를 확인한다.
  Power On write generation과 continuation의 게시, ACK parse 뒤 publication 전 same-axis mutation,
  Resume 전/status publication same-axis interference, expected/observed generation, final proof의
  early/late cancel·deadline 선형화와 zero-wire/different-axis 비간섭도 포함한다.
  Group Enable wait 전용 35개는 mutation/status gate, fresh `0x2047`, 모든 `0x2045`와 delay에
  하나의 total deadline을 적용한다. pre-write cancel/deadline은 `NotAttempted`, zero wire,
  reusable connection, mutation/proof 불변이다. post-write caller cancel은 response와 accepted
  ACK/status publication을 끝낸 뒤 typed cancel이며 connection을 재사용한다. ACK no-response는
  `OutcomeUncertain`, continuation 없음, `Faulted`; status no-response는 `Accepted`, exact pending
  continuation, `Faulted`이고 둘 다 `TransportInvalidatedAtDeadline=true`다. rejected ACK는
  `Rejected`, continuation 없음으로 고정한다.
  Axis Stop 전용 32개 split 계약은 Begin의 `0x2022` exact once/zero status, Resume의 `0x2028`-only stable
  Standstill proof, accepted-publication deadline continuation 보존, timeout resume no-replay,
  no-status transport invalidation, 새 accepted Stop supersede, priority preemption 뒤 새 Stop 완료,
  concurrent Begin publication order, concurrent Resume second zero-wire, stale-session zero-wire와
  interrupted Resume의 pending Power On proof reset, same-axis other-handle/accepted-wait mutation의
  typed interference, status publication race와 pending 보존, zero-wire/different-axis 비간섭을
  포함한다. 기존 compound facade는 같은 Begin+Resume을 한 total deadline으로 조합한다.
  Group Stop 전용 34개 계약은 마지막 `0x2045` publication에서 mutation generation,
  early cancel/deadline과 stable Standby proof를 한 번에 결정하고 late cancel/deadline이 완료 결과를
  뒤집지 않음을 고정한다. pre-canceled Resume과 compound ACK 직후 cancel은 accepted evidence와
  continuation을 가진 typed cancellation이며, post-write deadline flag도 continuation lock으로
  게시한다. same-group final-decision race는 typed interference/pending, zero-wire/different-group은
  비간섭이다.
  Axis PowerOff 전용 35개 계약은 `0x2023(enable=false)` exact once, ACK+mutation
  generation+continuation atomic publication, success ACK 뒤 successful PowerOff/Standstill 3회 연속,
  mismatch reset, reject/pre-wire/invalid Resume zero-poll, timeout/cancel/status failure,
  response-loss/priority discard no-replay, same-axis interference/status race,
  expected/observed/intervening evidence, final cancel/deadline/generation 선형화와 pending PowerOn proof의
  명시적 resolve 경계를 포함한다.
  Drive read 계약은 실제 `0x6041` bit 3의 DS402 Fault projection과 별도 exact one-attempt
  `0x603F:0 UInt16/2` read, little-endian result, capability/identity/physical-axis zero-wire
  gate 및 failure context를 포함한다.
  2026-07-27의 269-test checkpoint는 이전 revision의 역사적 결과이며
  current total로 사용하지 않는다.
- `RunLasalContract`:
  current `PASS LASAL.StaticContract.SourceOnly` (Admin read, `0x7D22`와 dormant
  `0x7D12/0x7D13`, 9축, CyWork-only, D1~D3와 D4
  single-bank Ring/Trigger 및 D5 general-inline SDO Read active source,
  Axis 1 exact D5 Write active, Axis 2..4/비승인 D5 Write와 D4 Double·extended fail-closed wire)다.
- `RunLasalNetworkContract`: `PASS LASAL.StaticContract`; `LMCDiagnosticsService` constructor의
  38-state 이름/타입, 37개 scalar, 24-entry Bulk array,
  no-control-flow/final-`C_OK` exact gate와
  `LMCRecorderStore` constructor exact 초기화/publish-last negative fixture,
  `Classes.lcb` general `TryStartRead` declaration,
  4축 executor network와 generated metadata 포함
- `BuildSimpleExampleApp`: D5 runner 포함 `LMC_Library/LasalApiWpfTestApp`의 2026-08-08
  current Release는 VS2019 MSBuild 16.11.6.22506 Rebuild warning 0/error 0이고 full
  smoke runner `332/332` PASS다. 신규
  `Wpf.CallbackV2.D5TerminalWakeSingleFlightUsesAuthoritativeStatus`는 WPF가 callback v2
  `eventMask=1`/max datagram 52로 등록하고, exact retained D5 ticket wake만 UI에 queue한 뒤
  authoritative TCP `0x7E03`을 single-flight로 1회 조회하며 UDP hint 자체는 operation status를
  변경하지 않는 causal 경계를 고정한다.
  `Wpf.CallbackV2.StaleD5StatusCompletionPreservesNewerOwnership`은 old callback-triggered
  `0x7E03` 응답을 보류한 동안 retained ticket/callback token을 newer current-session token으로
  교체하고, old 응답 해제 뒤 stale continuation/finally가 newer token, operation-running gate,
  operation UI와 status를 덮어쓰지 못함을 고정한다. 이는 fake PC peer 검증이며 PLC callback publisher나
  PLC live/runtime 증거가 아니다. 그 밖의 smoke는 UI English/Korean 언어 전환·저장과 Axis Reset
  Begin `0x2024` 1회/Resume `0x2028`-only, failure 뒤 no-replay 재개, confirmed interference 뒤에만
  explicit new Reset을 허용하는 completion UI에 더해 status-only Resume의 PowerOff 선점 pending
  보존, accepted ACK 뒤 outer-safety preemption에서도 pending publication, safety preemption 뒤
  session cleanup clear 3개를 확인한다. Axis PowerOff는 accepted Begin timeout/cancel handoff,
  transient status failure의 두 번째 클릭 `0x2028`-only Resume, monitor 중 zero-wire 재클릭,
  confirmed interference 뒤 explicit replacement 1회, replacement reject의 exact old pending/flag 보존,
  Axis PowerOn durable recovery에서 Power Off Again 완료 뒤 journal/Close 해제를 고정한다.
  Group Reset smoke는 accepted failure 뒤 status-only Resume과 `0x2049` 1회, valid NACK/pre-write
  failure의 기존 preparation 보존, ACK response-loss outcome-uncertain interlock, Connect/Close/new
  mutation 차단, Stop/PowerOff/safe Disable terminal supersede, captured-member Axis Stop/PowerOff의
  accepted/NACK exact reconciliation, delayed Reset result와 Group Stop NACK 경합, final LockedStandby의
  Disable-only recovery를 고정한다. durable WPF smoke는 journal Arm/MarkAccepted/Promote/Resolve exact
  CAS, corruption/version/single-writer, endpoint/build/BootId/Map/group/member mismatch zero-status-wire,
  exact attach의 fresh `0x20D2` 1회와 Reset 0회, safety supersede 및 process-kill/restart 경계를 고정한다.
  그 밖에
  Admin capability/axis/group,
  Drive mode/non-atomic status의 exact fake-RPC와 typed UI 및 one-click Inline Read의
  typed/raw terminal, accepted-timeout/cancel ticket과 last-status 보존/수동 Refresh,
  pre-accept cancel/capability-off zero-wire 및 terminal failure guard 해제와 Axis 2..4/미승인 target에서
  same-value handler를 강제 호출해도 PLC request가 0건인 fail-closed zero-wire, bit 9와
  SDK target 승인의 독립 readiness 표시, Axis 1 exact `0x2F00:24 Int32/4` same-value 4-ticket
  PASS 전 일반 수동 Write의 zero-wire 강제-handler 차단, connection/session/build/boot/map/target
  proof 불일치 및 reconnect 폐기, proof 뒤 첫 클릭 arm/편집 시 re-arm/동일 요청 두 번째
  클릭만 consume하는 비모달 confirmation state, cached matrix refresh의 wire 0회와 마지막
  `PASS`/`RECOVERY REQUIRED` 결과 보존을 확인한다. Recorder smoke는 manual Double을 ordinary
  Configure/ordinary field와 분리하고 durable route가 닫힌 동안 두 delegate 모두 zero-call임을
  확인한다. 별도 config-only adapter smoke 5개는 exact configuration-only retain/cleanup,
  accepted-result preemption 뒤 exact retain/cleanup, Configure response-loss의 unknown scope 보존,
  cleanup response-loss의 outcome-unverified/no-replay와 invalid pre-arm의 zero-Configure/no-journal을
  검증한다. delayed ordinary Configure ACK가
  safety reservation 뒤 `ResultDiscarded`되면 exact
  recovery-only handle을 MainWindow에 보존하고 명시 Release로 정리하는 전체 UI 경로도 포함한다.
  Axis motion-recovery smoke는 Stop Begin `0x2022` 1회 뒤 status-only Resume을 시작하고, monitor
  중 더 새 Power Off가 priority generation을 선점해도 Stop을 replay하지 않은 채 Power Off 3-sample
  proof를 완료하며 선점된 Stop continuation을 cleanup 전까지 보존하는 경로를 포함한다.
- Group Power 회귀는 `0x204A`/`0x204B` 명령을 각각 한 번만 전송한 뒤 `0x2045`의
  기대 `IsPowerOn` 값을 성공 응답 3회 연속 확인하는 완료 계약을 고정한다. pending 검증
  재개는 `0x2045`만 송신하고 원 power command를 replay하지 않는다. 수동 `Read Status` 한 번만으로
  pending Power On/Off 또는 Enable continuation을 완료하거나 ACTIVE/profile lock을 승격하지
  않는다. 다만 safety generation 검증을 통과한 성공 응답은 상태에 맞는 pending Enable
  continuation proof에 누적되고 Locked Standby proof가 3/3이면 기존 ACK를 재사용한 zero-wire
  Resume으로 완료할 수 있다.
  Stop/PowerOff safety 예약은 accepted Group Enable의 누적 status proof를 즉시 초기화하되 ACK와
  pending continuation을 보존한다. 예약 뒤 도착한 status response는 drain 후
  `ResultDiscarded`되어 observe되지 않는다. 예약 전에 SDK completion publication이 끝났지만 WPF
  적용 전에 safety가 예약된 좁은 경우만 recovery-required로 승격한다. connected unresolved
  상태에서는 group 이름 변경, group 재조회, clean connection/window close, connected reconnect와
  새 Power On을 차단한다. 외부 connection loss 뒤 reconnect 진입에서는 원 exact group 이름을
  보존한 recovery로 승격하고 새 session에서 그 이름의 group만 다시 조회한다. accepted pending은
  같은 group/session에서 성공한 명시적 `0x2048 GroupDisable` ACK, PowerOn=True +
  Disabled/Unlocked 3회 연속 또는 PowerOn=False 3회 연속 proof로 해제한다. recovery-required는
  성공한 `0x2048 GroupDisable` ACK 또는 보존한 exact recovery group의 PowerOn=False 3회 연속
  proof로만 해제한다. Power On 성공만으로는 해제되지 않으며 어느 경로도 `0x2047`을 replay하지
  않는다. 별도 Group profile-lock journal 회귀는 `0x2047` 전 durable arm, restart의
  Armed-to-RecoveryRequired 승격, endpoint mismatch의 TCP/RPC 0회, group-reference mismatch의
  lookup-only/mutation 0회, exact Disable resolve와 identity 응답 중 safety reservation의
  durable recovery 보존을 검사한다. 이 항목은 deterministic
  PC/fake-TCP와 WPF smoke 증거이지 PLC runtime 검증이 아니다.
- `BuildDistributionExampleApp`: binary-reference distribution example build PASS
- full distribution preview pipeline: manifest `56/56`, semantic policy `28/28`/15 checks,
  transaction/manual snapshot/provenance `86/86` PASS. temporary standalone example Debug/Release
  build, forbidden internal-reference scan, cleanup과 DLL hash identity를 포함한다.
  검토한 `2.0-candidate` DOCX/PDF exact bytes를 사용한 실제 sibling
  `LMC_API_Distribution_candidate_20260731_manual_2_0_provenance`도 schema 2 manifest와
  semantic preflight를 통과했다. 이 결과는 `dirty-preview` PC/package 증거이며 canonical
  승격이나 PLC/runtime/production 승인이 아니다.

target을 분리했기 때문에 PC C# 실패와 LASAL static source contract 실패를
구분할 수 있다. 자동 테스트 통과는 serializer/parser/connection lifecycle와
source contract 검증이며 LASAL IDE compile, PLC download와 실제
EtherCAT/motion 동작 검증을 대체하지 않는다.

현재 단계 구분:

| 단계 | 자동/정적 계약 상태 | 실제 PLC 상태 |
|---|---|---|
| D0 | current `IntegratedReadOwnerDormant`: 464-byte snapshot과 `0x7E13/0x7E22` route/handler 구현, Axis1 SDO Write를 포함한 current test profile `CapabilityBits=0x0000633F`, bit 14 on과 bits 15~17 off 계약 | 과거 `Test2`의 `0x0000613F`와 exact 7-entry static topology 응답만 live PASS. current `0x0000633F` PLC download, dormant raw read, disconnect/recovery와 physical DI correlation은 미실시이며 `0x7E23`은 없음 |
| D1~D2 | active source, cleanup orchestration과 one-slave-partial 순수 판정 테스트 포함 | `11_PI_Bulk_Regression`의 Catalog/PI/Bulk happy path PASS; 100회 soak와 operator partial live/capture 대기 |
| D3 | active source와 PC contract 테스트 포함 | 기존 Recorder happy path 캡처 존재; trigger/fault/soak는 별도 gate |
| D4 | single-bank Ring/Trigger active, Double source/API/config-only WPF adapter dormant/gate-off 계약 포함 | Double PLC runtime 미실시 |
| D5 | general-inline Read submit/status/cancel, executor release/race와 abort/recovery analyzer 12개, disconnect/orphan application-recovery 코어 28개, four-ticket same-value Write qualification 9개와 WPF adapter 포함 | `10`/`12` happy path와 TypeMismatch recovery live PASS. fake-RPC 2-session WPF handler E2E와 Axis 1 exact Write 전송/Axis 2..4·미승인 target zero-wire는 PC 계약이다. PLC/SDK gate는 Axis 1 `0x2F00:24 Int32/4`만 활성이고 second safety, changed pre-Write zero-mutation, returned-ticket adoption-before-validation을 검사한다. pre-callback/pre-`0x7D12`/pre-`0x7D13` IDE Rebuild/Link와 implementation smoke는 PASS했지만 current source 재빌드와 실제 Write는 아직이다. sentinel/자동 restore/replay가 없고 single-writer 작업창이 필수다. PLC durable orphan witness/pcap, mailbox/물리 증거 및 offline/timeout/cancel/contention live 대기 |
| Phase 1 facade | typed drive read, PI/Bulk builder/reader와 error catalog PC contract 포함 | `10_DriveRead_Axis1to4`, `11_PI_Bulk_Regression` happy path PASS |
| Admin/group | active `0x7D00/10/20/22`와 dormant `0x7D12/0x7D13` C# golden/parser/fake-RPC 및 LASAL SourceOnly mapping 범위 | active 4개는 `01`~`08c`, `04b`, `09b` happy path 근거가 있다. SetPosition은 bit 3 OFF/native call 0/WPF 미노출이다. Reference는 bit 4 OFF/native call 0/WPF 미노출이고 56/32-byte frame, recipe 1/2, positive MaxTravel/Timeout 계약만 고정한다. 둘 다 current IDE/download/live proof 전 활성화 금지; `0x2047` accepted-then-poll 수정본, queue/race/fault gate 잔존 |

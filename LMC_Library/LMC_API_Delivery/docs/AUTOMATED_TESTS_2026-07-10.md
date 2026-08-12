# LASAL Motion Control API Automated Tests

작성일: 2026-07-10

최종 결과 재확인: 2026-08-11 (`cbf2548`; verifier compatibility `ad4af91`;
reconnect policy `14ccf58`)

Owner-loss retirement 및 fixed-port same-window 회귀 범위 갱신: 2026-08-12

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
- 같은 WPF window와 같은 fixed UDP callback port에서 explicit Close 후 새
  `LMCConnection`으로 Connect하는 정상 ACK/`ErrorId=-1` close 두 fake-RPC 회귀
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
  Succeeded/Rejected 조합을 검증한다. `0x7D1A RetireAxisSetPositionOutcome`은 같은 exact
  recovery key와 nonzero generation을 넣은 60-byte request, 동일 92-byte terminal snapshot,
  lost-response exact retry와 malformed-generation session fault를 검증한다. capability bit 5는
  query-only, bit 7은 retirement이며 strict parser는 `bit 7 => bit 5`,
  `bit 3 => bit 5 + bit 7`을 강제한다. current PLC는 bit 3/5/7과 route/store가 모두 OFF다.
  old ErrorCatalog와 operation-inapplicable detail도 fail-closed하며, 이 tranche를 포함한
  2026-08-12 SDK Debug/Release 전체 회귀는 각각 `1151/1151` PASS다.
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

실제 WPF EXE 종료/재실행 gate는 full smoke와 분리되어 있으며 exact EXE path가
필수다.

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj' `
  /t:RunWpfExecutableRelaunchTest /p:Configuration=Release /p:Platform=AnyCPU `
  /p:WpfExecutableRelaunchExe='<absolute-path-to-LasalMotionControlApiExample.exe>' /nologo
```

이 target은 actual EXE의 제한된 loopback probe mode만 사용한다. 일반 사용자 journal이나
PLC endpoint를 사용하지 않는다.

현재 결과와 historical contract coverage:

- `RunLasalContract`/`RunLasalNetworkContract`: historical `GateDVisualLayout` checkpoint에서
  `Phase5TransportClean / IntegratedReadOwnerDormant`, `ExpectedSdoWriteAxis=1`와 dormant Admin
  `0x7D12` SourceOnly/full coverage가 PASS했다. `ad4af91` exact STOP은 아래 기록의
  pre-approval historical 결과다. Latest `d4204b4` clean tracked SourceOnly 결과가 우선하며 main
  working tree 사용자 `Classes.lcb`는 별도 exact identity reject 상태다. Historical valid request는
  `InvalidState/detail 10`, capability bit 3 OFF,
  native SetPosition call 0을 12개 negative source fixture와 함께 고정한다. LASAL IDE
  Rebuild/Link `0 error(s), 20 warning(s)`, Linker `Done`과 `LMCEcatInputLatch`,
  `LMCDiagnosticsService`, `TCPMotionInterface` direct exact-method
  Implementation-tab/header smoke, 신규
  `CInvalidArgException=0`은 이번 `0x405C` callback ownership/`0x7D12`/`0x7D13`
  편집 전 historical checkpoint다. current callback+`0x7D12`+`0x7D13` source 전체의 strict
  격리 단일-Rebuild raw log는 `GateDVisualLayout` PID 480/TID 3396에서 생성됐고
  derived-transcript
  `VerifyBuild`는 PASS했다. 이후 세 Gate D method의 exact Implementation UI 확인은
  사용자 동시점 진술로 `exactMethodOpen=manual-attested`다. 자동 method-smoke
  JSON/log artifact는 별도 pending/nonblocking이다.
  current Gate D만의 incremental Build/Download 결과는 아래 별도 항목으로 기록하며, 이
  historical checkpoint 자체는 current PLC download/runtime 증거가 아니다.
- 신규 dormant `0x7D13` LASAL source/static contract는 exact 56/32-byte frame,
  recipe 1/2, capability bit 4 OFF, valid request `InvalidState/detail 10`, physical reference
  input source 부재와 native `MoveReference` call 0을 검사 대상으로 추가했고 historical
  checkpoint의 SourceOnly/full coverage가 PASS했다. Latest target outcome은 아래
  `d4204b4` exact tracked approval과 main dirty rejection 기록이 우선한다.
- 2026-08-10 Gate D source/static checkpoint는
  `Verify-LasalUdpCallbackContract.ps1` self-test `288/288`, 실제
  `TerminalWakeBrokerCandidate` tree에서 exit 0의 `CAPTURE` 판정을 냈다. 실제
  판정은 `IDEClosed=true`,
  `ProductionApproved=false`, `NeedsRebaseline=true`다. checkpoint focused verifier는
  canonical-LF 545,566 bytes, SHA-256
  `FBF1A8582E85039377AC39F26D8BBA64C0EB62665424DE150083CFC412CC7CA3`이고,
  capture self-test는 positive `46` / negative `94` PASS다. 일반
  `Verify-LasalContract.ps1`의 격리 양성 1개와 신규 음성 fixture `7/7`, Gate D
  expected-state/derived-capture 옵션을 명시한 full SourceOnly도 PASS했다. 선언 전용
  verifier는 Windows PowerShell 5.1과
  PowerShell 7에서 각각 `19/19`을 PASS했고, 선언 checkpoint의 source reverse
  delta, `Classes.lcb` 변수/method ABI, root project `.lcb`의 exact one-byte delta를
  고정한다. declaration-only `VerifyCurrent` 대상이 아니다. 현재 generated
  `Classes.lcb`는 checkpoint identity에서 drift해 focused/C78 current verification도
  실패한다. 이 결과는 source/generated ABI 정적 증거이며 Gate D C78 Rebuild,
  PLC download/runtime 또는 live packet 증거가 아니다.
- 위 `288/288`, canonical-LF 545,566 bytes / `FBF1A858...`는 sequence-4 당시의
  historical checkpoint 결과다. 현재 portability ratchet의 focused verifier는
  canonical-LF 564,360 bytes / SHA-256
  `20BDC1E49B3ED329143F0C36576F118F369383B3DA922069FDD2DD8B1909CC90`이며 Windows
  PowerShell 5.1 self-test의 negative fixture `290/290`을 모두 거부했다. exact generated
  Network artifact 8개만 채운 clean detached `5543579` worktree에서 focused `CAPTURE`가
  exit `0`으로 재현됐고, 일반 `Verify-LasalContract.ps1` SourceOnly도 249.3초에 PASS했다.
  generated source/include와 derived Comm table은 pinned exact LF/CRLF physical
  representation 두 종류만 허용한다. 보호된 Network text 6개만 예외적으로 bare CR을
  거부한 뒤 LF, CRLF 또는 둘의 혼합을 byte 수준 canonical-LF로 비교한다. 이때
  `0x0D 0x0A`의 `0x0D`만 제거하고 high byte를 그대로 보존하며 canonical byte count와
  SHA-256이 exact해야 한다. 나머지 Network binary identity, topology, path inventory와
  count는 strict하고, Gate D full/tracked raw Network aggregate도 pinned IDE-layout 또는
  clean-checkout count/SHA tuple만 허용한다.
  checkpoint capture tool은 기존 `HistoricalGateD` pin을 보존하면서 current pin을 별도로
  고정했고 self-test positive `50` / negative `99`, actual sequence-4 manifest revalidation이
  PASS했다. 이 support-tool/evidence 변경은 production 변경과 분리된 tooling/evidence
  changeset이며 production approval을 의미하지 않는다. 그 changeset 당시 main
  worktree의 formal gate는 `Classes.lcb=6E115876...` 대 checkpoint
  `24402BFA...` 차이 하나 때문에 실패했다.
- Pre-approval commit `afdf6a3`의 checkout-safe UDP verifier physical SHA-256은
  `A6244374803C622A7F115C21A30039C38A4FA4297AD2D0C4A1B47518515A0DE5`이고 PS5.1/PS7
  self-test가 각각 `296/296` PASS했다. Derived function parser는 LF/CRLF에서 exact function
  inventory와 lexical token equivalence를 함께 검사한다. Pure-Git Network aggregate
  `15` / `239F71DC2BD04491582735AB424BCFB71E87BC3E88F2D7F0BEC21C592363FA22`와
  tracked aggregate `15` /
  `6FF1BDAED41EE9F2AE017891BBF23CACBFA0FB510BEF07EAA4C7619DDA49DA38`을 exact
  checkout tuple로 추가했고 기존 seeded/clean-checkout tuple은 제거하지 않았다. Explicit
  `-ExpectedState TerminalWakeBrokerCandidate -AllowDerivedCapture` focused run만 `CAPTURE`,
  `ProductionApproved=false`, `NeedsRebaseline=true`를 반환한다. 기본 production invocation은
  approved physical snapshot ratchet이 없으면 계속 blocker를 반환한다.
- Commit `d4204b4`의 current verifier는 572,974 bytes / SHA-256
  `F036B9B3F2D3E173D38BFB6CBBAB05EC4F877CCDF9B972E95C6ED35B7DE34E37`이다. 기존
  source/generated/network/layout exact 검사를 모두 통과한 clean tracked
  `TerminalWakeBrokerCandidate`의 `Classes.lcb` 8,549,773 bytes / SHA-256
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861` tuple만
  `ProductionApproved=true`, `NeedsRebaseline=false`로 승인한다. PS5.1/PS7 self-test는 각각
  `296/296`, clean detached SourceOnly `Phase5TransportClean / IntegratedReadOwnerDormant`는 두
  host에서 PASS했다. 당시 Main working tree 사용자 `Classes.lcb` SHA-256
  `13EA5823DF0887D6042408E2A884E9F8DF50304443227353B9BDCA9AD2ECBFD9`는 exact sanctioned
  identity drift로 reject됐다. 당시 post-approval full/network static target은 실행하지 않았다.
- 2026-08-12 owner-loss retirement 변경은 위 `d4204b4`/`296/296`을 historical evidence로
  남긴다. 새 focused verifier는 기존 negative fixture 9개를 그대로 복원·유지하고
  owner-loss 경계 negative fixture 9개를 추가하므로 self-test 계약은 `305/305`다.
  Windows PowerShell 5.1 long self-test는 exit `0`, `305/305` PASS,
  elapsed `238039 ms`였고 PowerShell 7은 exit `0`, `305/305` PASS,
  elapsed `566196 ms`였다. 이 dual-host verifier 결과는 source/static 계약 증거다. 이후 같은
  source의 격리 LASAL incremental Build도 compile/link는 PASS했지만 generated artifact gate에서
  STOP했으며 PLC Download/runtime은 실행하지 않았다.
- Follow-up commit `bbe8a8d` reuses the existing
  `RpcCallbackLastDisarmResult` field as a boundary-correlated owner-loss Watch
  latch; it adds no declaration, TCP frame, or UDP datagram field. Each allowed
  owner boundary resets the latch, and sentinel plus confirmation success
  re-latches exact `-8` after tuple clear. A dormant helper no-op preserves that
  evidence. Sentinel failure, pre-sentinel sender loss, confirmation `-9`,
  confirmation mismatch `-8`, clean matched disarm, and ordinary mismatch are
  modeled separately. The final PS5.1 and PS7 long self-tests both reject
  `311/311` negative fixtures (elapsed `239.9 s` and `546.2 s`). Final `-8` alone
  is never a runtime PASS; the allowed socket boundary, tuple/queue transition,
  and next registration/reception are mandatory. During this verification,
  `VerifyCurrent` was not forced past the user-owned LASAL WTR session (PID 41504), and this follow-up
  has no LASAL build, sanctioned generated artifact, PLC Download, or live result.
- 2026-08-12 `e3c9365` tree `47e6c141673d5bc29901a2604e377f849e50fe44`는 strict
  `Auto` child-state parser를 `12/12` positive와 `15/15` fail-closed negative fixture로
  검증했다. 승인 상태의 `PASS/ProductionApproved=true/NeedsRebaseline=false` tuple과
  `vendor=` evidence tail을 exact로 묶고 모순 tuple을 거부한다. 같은 clean tree의 default
  `Auto` SourceOnly는 PS5.1과 PS7에서 모두 `TerminalWakeBrokerCandidate`를 wrapper topology로
  전파해 PASS했다. Full Distribution도 preflight `14/14`, files `94`, semantic policy `18`,
  actual EXE relaunch와 candidate transaction을 PASS했다. Candidate는 `84` files /
  `9,519,684` bytes이고 release manifest SHA-256은
  `F94608C10E9BB1EEFE780D7323F5BB2DBC1BBB1E6CED1EFCBE6422B32165D710`이다. 이 결과에도
  LASAL IDE build, PLC download와 live callback runtime은 포함되지 않는다.
- 같은 날 main의 dirty `Classes.lcb`를 건드리지 않기 위해 exact input hash를 묶은 격리
  worktree에 approved
  `Classes.lcb` SHA-256
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`과 위 tree의 두
  owner-loss `.st`를 배치하고 LASAL Class 2 `02.03.002`로 C78/ARM 프로젝트를 열었다.
  load 단계의 자동 link/scan 결과가
  `Done - 1 error(s), 6 warning(s)`였다. Exact error는 active `Hardware` library의
  `_DriveMngBase/DriveComL2.h`를 `MotionLib/Include/global.h(15)`에서 읽지 못한 `E0015`이고,
  warning 6개는 linked `MotionLib` class change/`ReducedClientDependency=false` 1개와 설치
  library 5개(`Hardware`, `MotionLib`, `OS Interface`, `System`, `Tools`)의 `C82` 대 current
  project/compiler `C78` 차이다. Repository의 tracked와 ignored project image 및 timestamp를
  맞춘 exact input에서 load command는 succeeded로 끝나 project가 열렸다. Active tree에 없는
  header를 다른 설치 세대에서 복사하지 않았다.
- 같은 격리 session에서 source compiler 결과를 분리하기 위해 incremental `Build project`를
  정확히 1회 실행했다. Build window는 22,798 bytes / 214 non-empty lines / SHA-256
  `B7A1F6FAFB162A9CCCC6CA429F32EFB22A1016737F449B791EF13B7B73ED24C5`이고,
  `LMCUdpCallbackSender.st`와 `TCPMotionInterface.st`를 모두 compile했다. 결과는
  `Done - 0 error(s), 24 warning(s)`, `Compiler Done` 2회, `Linker Done` 1회,
  `Last command succeeded (5317.3ms)`다. Source warning은 sender `W0070` 10건,
  TCP `W0070` 13건 + `W0069` 1건이다. Result 뒤 current project C78 1건과 설치 library
  C82/C78 5건이 추가되어 bounded log warning line은 30건이다. 새 `CInvalidArgException`,
  Connect, Download는 0건이다.
- Build 뒤 두 대상 ST, `Networks.lcb` `C307547E...`, project `.lcp` `C84DE...`는 불변이다.
  Generated `TCPMotionInterface.lba`는 539,035 bytes /
  `2A5AC668E540B0BC05B6F164ACA8DC5FBAEF22FF3320F8F463777A59F0D12AEF`,
  `LMCUdpCallbackSender.lba`는 255,550 bytes /
  `3D1CEE13AC95C125CBC46BD9AB267A8F8C483F281C5CC412C609FFC7A0EDAC33`이다.
  반면 `Classes.lcb`는 approved 8,549,773 bytes / `24402BFA...`에서 8,549,824 bytes /
  `5337BBAFE88DB10D47308ED2BED89F7B7C22BFE66D7CA739D3A872276DA308E5`로 51 bytes 증가했다.
  Official comparator는 checkpoint `e3c9365` 대비 `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`, exit `3`,
  Gate D target unequal, protected dependency equal로 판정했다. LASAL 종료 뒤 focused
  `VerifyCurrent -ExpectedState Auto`도 `Classes.lcb sanctioned Gate D identity drifted`로 exit `1`이다.
  Library 제거 prompt는 거부했고 `Close Project` succeeded, LASAL process 0을 확인한 뒤 격리
  worktree만 제거했다. Compile/link PASS와 별개로 generated artifact는 승인되지 않았으며
  Download는 금지다.
- exact `GateDVisualLayout` C78 `VerifyBuild ... -RunFullStatic` 재실행은 247.8초에
  exit `0`으로 끝났고,
  `PASS LASAL.StaticContract (Phase5TransportClean; ... diagnostics D1-D5 ...)`와
  `PASS LASAL.C78RebuildEvidence.Verify ... profile=GateDVisualLayout
  inputsEquivalent=true; rawInputsUnchanged=10/10 replayEquivalentSt=0
  regeneratedOutputsBound=2 evidenceSource=bounded-repository`를 모두 출력했다.
  첫 full-static 시도는 production source 실패가 아니라
  `Verify-LasalContract.ps1`의 `$stage87AdapterCallPattern`이 잘못된 function-local
  scope에 있던 verifier tool defect를 드러냈다. 해당 정의를
  `Assert-LasalDs402OwnerReceiptProviderMutationFences` 안으로 이동한 뒤 Windows
  PowerShell 5.1/PowerShell 7 AST와 strict self-test `67/67`을 PASS하고 위 재실행이
  성공했다.
- 2026-08-10 10:35 Gate D LASAL evidence는 C78/ARM incremental `Build project`가
  변경된 `LMCDiagnosticsService`, `LMCUdpCallbackSender`, `TCPMotionInterface` 세 class를
  compile하고 source warning 60개(`W0069=28`, `W0070=21`, `W0072=11`), compiler
  error 0, `Linker Done`으로 끝난 것을 기록한다. 첫 Download는 세 class LBA와 PLC link가
  성공해 `Download Ok`였고, 두 번째 Download는 CPU-state timeout 뒤 aborted됐다.
  reconnect는 성공했고 `Project successfully loaded`가 확인됐다. 이는 strict C78 Rebuild,
  sequence-4 physical checkpoint 또는 live UDP-to-`0x7E03` causal packet 증거가 아니다.
  static state는 계속 `TerminalWakeBrokerCandidate`, `ProductionApproved=false`,
  `NeedsRebaseline=true`다.
- 2026-08-10 이전 PID 4832 LASAL session의 첫 `Rebuild project`는
  `Classes.lcb` persistence `ios_base::failure`와 write-failed 두 error record가 있어
  무효다. 두 번째 Rebuild의 bounded window는 C78/ARM source warning 76개
  (`W0069=35`, `W0070=21`, `W0072=17`, `W0073=3`), source error 0,
  `Compiler Done`, `Linker Done`, `CInvalidArgException=0`을 확인했다. 그 PID
  4832 두 번째 Rebuild 산출물의 `Classes.lcb`는 8,549,773 bytes, SHA-256
  `3AC3D938DC1520FAEA6C3693161ABDB280CC873A97C60CF79B3F716C7F064C22`다.
  그 시점의 focused `VerifyCurrent`는 exit 0의 `CAPTURE TerminalWakeBrokerCandidate`를
  출력했고 bootstrap
  `ValidateOnly`는 `UNTRUSTED`, `outputCreated=false`다. 따라서 그 bootstrap run은
  sequence-4 physical output을 생성하지 않았고 승인 상태는 `ProductionApproved=false`,
  `NeedsRebaseline=true`다.
- PID 4832는 Rebuild 2회와 후속 Connect/Reset/Restart를 함께 포함하므로
  `Verify-LasalC78RebuildEvidence.ps1` 기준의 격리 build session이 아니다.
  post-build `Find in Implementation` action은 없었고 Download는 0회였으며,
  Reset/Restart는 기존 PLC image만 다시 실행했다. 이 Find action은 Object Network의
  Server/Client 행에만 적용되고 일반 method 행에는 해당하지 않으므로, Find 부재 자체는
  세 Gate D method의 미완료 사유가 아니다.
- retained `GateDVisualLayout` PID 480 / Rebuild TID 3396 raw log는 canonical
  project load 1회, `Rebuild project` 딱 1회, Connect/Download 0회, 정상
  close/exit를 기록했다. Rebuild-command window는 C78/ARM warning 76개
  (`W0069=35`, `W0070=21`, `W0072=17`, `W0073=3`), error 0,
  `Compiler Done=2`, `Linker Done=1`, post-result C82 compatibility warning 6개,
  `CInvalidArgException=0`이다. Rebuild/checkpoint-bound `Classes.lcb`는 8,549,773 bytes, SHA-256
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`다.
  baseline은 6,887 bytes /
  `247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE`,
  exact raw range `[6532176,7298848)`는 766,672 bytes /
  `B918E51279360E27780D212650361AF361FFFC391C5F24854447BE0F3F9ABD17`,
  sidecar manifest는 1,574 bytes /
  `7928BC0D641FEA79444EDE8AD49FC10C15C28D453DB75DAF82C21B9D303D1DFC`,
  derived transcript는 30,111 bytes /
  `F32122D318DBFD8F53BC9E5AD0FF693F9B6F05368D40FC64138A010A1BC810AF`다.
  `VerifyBuild`는 `profile=GateDVisualLayout`, `inputsEquivalent=true`,
  `rawInputsUnchanged=10/10`, `regeneratedOutputsBound=2`,
  `evidenceSource=bounded-repository`로 checkpoint identity에서 PASS했다. PID
  7288/D71E...는 superseded historical evidence로만 보존한다.
- PID 480에 method-specific UI proof가 없는 것은 isolated Rebuild session의
  fact다. `Find in Implementation`은 Object Network
  Server/Client 행에만 적용되고 일반 class function/method 행에는 해당하지 않는다.
  사용자는 이 row-level Find action의 정상 동작을 별도로 확인했지만 이는 selected
  method open 증거가 아니다. method 행은 `Edit Method`, Enter 또는 direct open으로
  열고 exact Implementation tab/header를 확인한다. 사용자는 이후
  `LMCDiagnosticsService::TryTakeD5TerminalWake`,
  `LMCUdpCallbackSender::PublishEvent`, `TCPMotionInterface::PublishD5TerminalWake`
  세 method의 정확한 Implementation 표시가 정상임과 LASAL 종료를 직접 확인했다.
  따라서 UI evidence는 `exactMethodOpen=manual-attested`이며 동일 동작을 다시 요청하지
  않는다. `Lasal2.log`의 Open Implementation은 class-level token이고 자동 session
  restore에서도 생길 수 있어 selected method를 증명하지 못한다. 자동 method-smoke
  JSON/log artifact는 별도 pending/nonblocking으로 유지하고, log delta는 session 경계,
  `CInvalidArgException`, 기록된 금지 명령 audit에만 사용한다.
- 이전 bootstrap `ValidateOnly`는
  `gate_d_terminal_wake_broker_candidate_checkpoint.json`을 3,225,878 bytes,
  SHA-256
  `E0490DC348B861FBE47AB4C2E9C558BE679E865787A014860EBA45B3E0E508E4`로
  계획했지만 `UNTRUSTED`, `outputCreated=false`였고 그 run에서는 physical manifest가
  생성되지 않았다. 이후 trust-anchor commit `bb5fd93` 다음의 `5543579`가 sequence-4
  manifest와 exact production transition `Class/Classes.lcb`,
  `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`,
  `Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`,
  `Class/TCPMotionInterface/TCPMotionInterface.st`,
  `Class/_UDPTransceiver/_UDPTransceiver.st`,
  `Network/Comm_Network/Comm_Network.lcn`, `Network/Networks.lcb` exact 7개를
  원자적으로 commit했다. manifest는 `Classes.lcb=24402BFA...`,
  `ProductionApproved=false`, `NeedsRebaseline=true`를 기록한다.
- `5543579` 뒤 LASAL PID 34656은 C78/ARM Rebuild에서 변경된 세 class compile,
  `Compiler Done`, `Linker Done`, command success를 기록했다. 이어진 Download는
  `Download Ok`, `Project successfully loaded`였고 이후 Reset/Restart와 project loaded도
  성공했다. 그러나 그 session의 regenerated `Classes.lcb`는 8,549,773 bytes, SHA-256
  `6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712`로
  manifest의 `24402BFA...`와 달랐고 focused `VerifyCurrent`와 C78
  input-equivalence는 당시 실패했다. Frozen historical `99014DD9...` artifact와
  post-STOP incident-time `13EA5823...` artifact에서도 gate는 계속 실패한다. `d4204b4`의 static
  approval은 exact tracked `24402BFA...`만 허용하며 이 mismatched artifact와 당시 runtime
  관측을 승인하지 않는다.
- commit `7038445`는 `6E115876...`-start baseline과 exact reversible
  `24402BFA... -> 6E115876...` binary patch를 production source 변경 없이 보존한다.
  commit `79f03d36f89c34b26325666a4a3eddb9306c4674`의 fail-closed 비교기는
  `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Compare-LasalClassesArtifact.ps1`이며,
  physical 79,592 bytes / SHA-256
  `B91BFB5AFE131F0ECB3F23DC00373BEC7FC91B2C37CF626D128E912F633EBBA4`다.
  Windows PowerShell 5.1/PowerShell 7 AST가 PASS했고 self-test는 positive `6` /
  negative `14`를 PASS했다. checkpoint commit `5543579`와 당시 `6E115876...` candidate의 실제
  PS5/PS7 비교 stdout은 각각 51,102 bytes / SHA-256
  `9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639`로
  byte-identical하다. commit `2e8ca8a84a141390424ce859ac8c315a90ec3430`은 이 exact
  CreateNew JSON
  `classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json`을 보존한다.
- 실제 `24402BFA... -> 6E115876...` 판정은 exit `2`,
  `REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT`다. changed byte/run/owner는 각각
  `99/58/36`이고, 120-record inventory, Gate D target 4개, protected dependency
  record 2개는 exact이며 unmapped run은 `0`이다. 이 bounded equality는 artifact
  전체를 승인하지 않는다. 판정은 `ProductionApproved=false`,
  `SemanticEquivalenceProven=false`로 고정된다.
- retained PID 480/TID 3396 checkpoint를 다시 실행하지 않는다. 별도 isolated
  classification은 새 LASAL process와 canonical `.lcp`에서 `Rebuild project` 정확히 1회,
  정상 close/exit, Connect/Download 0회로 완료됐다. frozen `Lasal2.log`는 9,554,717 bytes /
  SHA-256 `25F6A3FA913FD2BF57117C19D0C4489399F5A4FD296CF86C1508AEA07BA02A8C`,
  그 isolated classification의 frozen `Classes.lcb` snapshot은 8,549,773 bytes /
  `99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD`,
  `Networks.lcb`는 242,363 bytes /
  `C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F`이고
  finalization 전 LASAL process는 0개였다. 이는 제3 Classes hash 분류 입력이며
  Rebuild와 이미 완료한 manual exact-method smoke를 반복하지 않는다.
- `b2019db` bundle의 historical producer는 `fa2a456` revision의
  `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Finalize-LasalClassesRebuildCandidate.ps1`,
  physical 187,443 bytes / SHA-256
  `1551A121D49C3C3169B0DADA45B4EEAAFDD8F8636425E470D1A6840159CBC0D5`,
  Git blob `5495e5636462d8aa67e13abb70c310a1ee8f9e67`이다. 당시 PowerShell 7
  self-test positive `26` / negative `76`, Windows PowerShell 5.1 AST/self-test
  positive `24` / negative `74`가 PASS했다. Published manifest의 producer tuple은
  이 역사 identity로 유지한다.
- current future-run fix `29811c4`의 finalizer는 physical 188,693 bytes / SHA-256
  `817E1A416C1484E1AE897140B2C56D8A7DDDF1F4158AC7DED2B59F28C5050116`이고
  PowerShell 7 self-test positive `27` / negative `77`가 PASS한다. Status를
  `Console.Out`으로 분리하고 final result를 exact `System.Int32` `{0,2,3}`으로 제한해
  process exit `3`을 보존한다. Directory ADS evidence 때문에 production은 계속
  PowerShell 7-only다. 아래는 future candidate용 exact command이며 `b2019db` bundle에는
  다시 실행하지 않는다.

  ```powershell
  & pwsh.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Finalize-LasalClassesRebuildCandidate.ps1 -FinalizeCandidate -RepositoryRoot (Get-Location).Path
  $LASTEXITCODE
  ```

  Exit `0`은 exact checkpoint `24402BFA...`의 static replay만 허용한다. Exit `2`는
  known `6E115876...` reproducibility/review만 뜻한다. Exit `3`은 제3 hash의 unstable
  generator이므로 중지하고, exit `4`는 blocked/no accepted publication이다. 모든 exit에서
  `ProductionApproved=false`, `onlineRuntimeQualificationPermitted=false`이며 Download하지
  않는다. finalizer가 허용할 수 있는 build error는 exact load-only
  `DriveComL2.h` `E0015` 최대 1개뿐이다. 다른 error 또는 추가 error record는 중지다.
  첫 real third-hash production run은 pre-`fa2a456` finalizer로 atomic-publish
  named-identity recheck까지 진행한 뒤 `OrderedDictionary` key를
  `PSObject.Properties[...].Value`로 읽은 버그 때문에 exit `4`로 중단됐다. bundle은
  publish되지 않았고 exact-owned stage cleanup은 완료됐다. `fa2a456`은 exact-case
  `IDictionary`/`PSCustomObject` accessor와 production-shape ordered-report 회귀를
  추가했다. 이 exit `4`는 accepted exit `3` 판정이 아니다. Frozen log와 outputs가
  그대로였기 때문에 Rebuild 없이 finalizer만 한 번 다시 실행했고, `fa2a456`은 bundle을
  publish했다. Manifest는 `UNSTABLE_THIRD_CLASSES_HASH_STOP`, exit `3`을 정확히
  기록했지만 status `Write-Output`과 반환 integer가 success pipeline에서 배열이 되어 host
  process는 잘못 `0`을 반환했다. `29811c4`가 이 future exit-code bug를 고쳤으며 historical
  bundle을 변경하거나 republish하지 않는다. Finalizer와 Rebuild는 더 실행하지 않는다.
- Commit `b2019db`는 published exact 8-file directory를 한 commit으로 보존한다. Manifest는
  `Classes.lcb=99014DD9...`, `Networks.lcb=C307547E...`, exit `3`,
  `ProductionApproved=false`, `staticReplayPermitted=false`,
  `onlineRuntimeQualificationPermitted=false`다. Checkpoint comparison은 changed
  byte/run/opaque-owner `96/52/34`, unmapped run `0`, Gate D target 4개와 protected
  dependency 2개 exact를 기록한다. 이는 semantic equivalence 또는 Download 근거가 아니다.
- initial `531abdd`에서 시작한 validator의 current commit `c48e403`은
  `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Verify-LasalClassesRebuildFinalizationBundle.ps1`,
  physical 189,867 bytes / SHA-256
  `DB8B046DF00900140E1AB97B83EF1E7AD13EFB44AC2768EE54B219160D8CE6B0`다.
  PowerShell 7 self-test positive `5` / negative `32`, Windows PowerShell 5.1
  AST가 PASS한다. PS5 production은 bundle evidence를 읽기 전에 exit `4`이므로 production
  `-VerifyBundle`은 PowerShell 7-only다. finalizer exit `0`/`2`/`3`이
  `candidate_finalization_gate_d_rebaseline_6e115876`를 publish한 뒤 그 directory를
  delete/overwrite하지 않고 finalizer를 다시 실행하지 않는다. exact inventory는
  `.finalizer-owner.json`, `Classes.post-rebuild.snapshot.lcb`,
  `Networks.post-rebuild.snapshot.lcb`,
  `derived_build_transcript_gate_d_rebaseline_6e115876.txt`,
  `bounded_lasal2_delta_gate_d_rebaseline_6e115876.raw.txt`,
  `bounded_lasal2_delta_gate_d_rebaseline_6e115876.manifest.json`,
  `classes_lcb_gate_d_rebuild_candidate.comparison.json`,
  `classes_lcb_gate_d_rebuild_candidate.finalization.json` exact 8개다. canonical repository
  root에서 exact command는 다음과 같다.

  ```powershell
  & pwsh.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Verify-LasalClassesRebuildFinalizationBundle.ps1 -VerifyBundle -RepositoryRoot (Get-Location).Path
  $LASTEXITCODE
  ```

  Earlier validator는 동일 command text의 두 정상 load-restoration record를 중복으로
  오판했다. `29811c4`는 raw `commandLineIndex` identity로 이를 교정했다. 다음에는 historical
  converter의 CRLF physical tuple과 Git LF blob, C78 verifier의 mixed-EOL physical tuple과
  Git LF blob을 같은 raw tuple로 오판했다. `c48e403`은 그 두 exact path에만
  physical bytes/SHA/blob OID/canonical-LF dual tuple을 적용하며 broad EOL relaxation은
  하지 않는다. Bundle은 두 validator fix 동안 byte-unchanged였고 current validator는
  committed `b2019db` bundle에서 exit `0` PASS했다.

  validator exit `0`은 현재 bundle integrity/cross-file contract만 증명한다. 과거 atomic
  move, complete manifest written-last ordering, PLC/runtime 또는 approval을 증명하지 않는다.
  validator PASS 뒤에만 exact 8-file directory 전체를 한 Git commit으로 원자 commit한다.
  failure면 bundle을 그대로 보존하고 중지한다. finalizer exit `0`은 static exact replay와
  별도 review만, exit `2`는 vendor semantics 보존/review만 허용하며 hash-only rebaseline은
  금지한다. exit `3`/`4`는 중지다. 모든 경우 Download 금지,
  `ProductionApproved=false`, `onlineRuntimeQualificationPermitted=false`다.
- pinned historical triad analyzer commit
  `998e7132c0892788db79a0868c5b129fb20edd96`의
  `Compare-LasalClassesVolatilityTriad.ps1`은 physical 139,073 bytes / SHA-256
  `E3E2C586C62379339EECFD8038189D9959C655CD206A4E894B846A2D79783663` /
  Git blob `a7dd4dba67e30c4adc80549a1d9b6a4d1acb6bce`다. PowerShell 7 self-test
  positive `7` / negative `16`, Windows PowerShell 5.1 positive `3` / negative
  `2` delegated가 PASS한다. Evidence commit
  `e7c812ad7cfc6ef2162ed1197dc615e2aebe45db`는 schema
  `LasalClassesVolatilityTriadEvidence/v1`의 exact report
  `classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_99014dd9.volatility.json`을
  physical 29,412 bytes / SHA-256
  `09C76BB3BC313642C3012A915C14C022EDF75965A8A431B87F26B463005489DC` /
  Git blob `3c4411e26493043b80828a5355bdc8b621457e09`로 보존한다.

  분석 범위는 pinned A/B/C `24402BFA...` / `6E115876...` /
  `99014DD9...`뿐이다. Pairwise changed byte/run/owner는 `99/58/36`,
  `96/52/34`, `105/61/36`이고, structural candidate `157`개 중 observed
  volatile 16-bit slot `66`개, stable candidate `91`개다. 모든 changed offset은
  marker-follower `35`개와 owner-end-minus-48 `31`개의 두 fixed 16-bit slot family에
  정확히 매핑된다. Candidate table SHA-256은
  `AD8A7FC5D6CB2277819FF28A7B7994C0FD6EAFBE6940419159662B8EFE83924D`,
  volatile-slot table SHA-256은
  `9D12A54145C409AC257F011C88F782108BCB3D73E9EDCCD8D2653A387F0F193C`다.
  이는 fixed slot structure만 증명하며 field meaning은
  `UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT`다. `99014DD9...` 반복성은
  증명하지 않는다.

  LASAL executable/compiler, vendor library set, generator cache state,
  filesystem timestamps, process session state의 implicit input 6개는 모두
  `UNPROVEN`이고 `allGeneratorInputsEquivalent=false`다. Publication trust
  boundary는 `NON_ADVERSARIAL_WORKSPACE`이며 `handleRelativeCreationUsed=false`,
  `concurrentParentReplacementResistance=false`다. Analyzer diagnostic exit `2`는
  `ProductionApproved=false`, `SemanticEquivalenceProven=false`,
  `requiresReviewedTransition=true`, `rebaselinePermitted=false`를 바꾸지 않는다.
  Focused/C78는 계속 FAIL이고 finalizer는
  `UNSTABLE_THIRD_CLASSES_HASH_STOP` exit `3` STOP이다. Download, runtime
  qualification, normalization, future artifact acceptance와 hash-only rebaseline은
  모두 금지다.
- pinned historical slot-corpus analyzer commit
  `731a01e428bdc9282edbf727f1d76a7a63cd24a3`의
  `Analyze-LasalClassesHistoricalSlotCorpus.ps1`은 physical 156,472 bytes / SHA-256
  `90BDD86EFC9C5032788C2603755A3560CC2871672E638935C4CD955B705EA080` / Git blob
  `30379ef2a50e093bbb28d768b9df77d091199de6`이고 PS7 self-test positive `12` /
  negative `18`, PS5 delegated positive `5` / negative `1`이 PASS한다. Evidence
  commit `43a85319905fbb5a42418b4b1ef9cd364c0bf44d`의 schema
  `LasalClassesHistoricalSlotCorpusEvidence/v1` report는 physical 157,999 bytes /
  SHA-256 `F306022CECD6C71BB7EA2B3DF309556A2621821B6C2CD287BC3FFFF4FA5A1B6A` /
  Git blob `edad859d03ac0c33f21ca42996a377dda3ee7b79`다.

  Canonical first-parent selector는 occurrence `22`개 / unique artifact `20`개 /
  topology `9`개이고 H/H+C/H+C+B unique는 `20/21/22`다. Full layer `2,501`
  records / `814` marker samples에서 exact-other varying tail은
  `87 groups/227 samples/31 owners/202 unequal pairs`, marker는
  `95/282/34/369`다. Mainline adjacent `21`개 transition의 common `2,378`은 raw
  identical `1,155` + candidate-only `538` + outside-target `685`이며 added/removed는
  `18/2`, candidate partition은 tail/marker/both `55/97/386`이다. B는 committed
  oracle에서 복원하고 C는 committed `b2019db` snapshot만 읽으므로 mutable current
  `Classes.lcb`와 local `bd47dd96...` object를 사용하지 않는다.

  Exact-other counterexample은 bounded stateless record-local hypothesis `20`개만
  claim scope 안에서 refute한다. Field meaning은 계속 unclassified다. 이 historical
  diagnostic은 `ProductionApproved=false`, `SemanticEquivalenceProven=false`,
  `rebaselinePermitted=false`, Download/runtime false,
  `requiresReviewedTransition=true`를 바꾸지 않는다. Focused/C78 FAIL,
  finalizer exit `3` STOP, Rebuild 반복 금지는 그대로다.
- Post-STOP incident evidence commit
  `5319352dbe389038b56f00cdaccf7cc14a80bf64`는 exact 네 artifact를 보존한다:
  `LASAL_POST_STOP_13EA_DOWNLOAD_INCIDENT_2026-08-11.md` 9,773 bytes / SHA-256
  `7299D2FF74CBD7986AAEEA1FAA42F6B592D5C531FAF5C2CDB524D400A128DD51`,
  `bounded_lasal2_delta_post_stop_13ea_download.manifest.json` 4,679 bytes /
  `BD9A72E76BC3526B9546001E8EB75E89F88BDB01B99D3AF4637CF969DF0930E1`,
  `bounded_lasal2_delta_post_stop_13ea_download.raw.txt` 1,490,589 bytes /
  `CAA408D3997182495023DBC1FA9719462447D2F822C459990CE4BECB6EA4E69C`,
  `classes_lcb_post_stop_13ea5823.comparison.json` 50,060 bytes /
  `DBC54235BDB505D9E7A198B3DCFA2CBD63F8AAC19728D1349FDC46DD5FA6CEC5`다.
  Capture 시 current `Classes.lcb`는 8,549,773 bytes /
  `13EA5823DF0887D6042408E2A884E9F8DF50304443227353B9BDCA9AD2ECBFD9`,
  `Networks.lcb`는 unchanged 242,363 bytes / `C307547E...`, appended
  `Lasal2.log`는 11,045,306 bytes / `CEC2256A...`다. PID 26200은 C78/ARM
  Rebuild, Connect, 282-LBA Download와 PLC link를 성공했고 PID 21016은 이후
  Connect/standalone Reset/Restart를 성공했지만 Rebuild/Download는 없었다. Action
  origin은 unproven이고 pre-Download Classes hash 또는 downloaded-payload hash
  manifest가 없어 `13EA...`와 Download의 관계는 `TIME_CORRELATION_ONLY`다.
  Checkpoint comparator는 `_AxisBase` boundary drift 때문에 exit `3`
  `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`, changed byte/run/owner `90/57/35`,
  unmapped `0`, Gate D `4/4`와 protected `2/2` exact를 기록한다. 변경 slot은
  marker/tail `30/27`; D를 unaccepted diagnostic으로 더한 volatile/stable union만
  `71/86`이며 committed A/B/C `66/91`은 그대로다. `b2019db` 990 bundle은 immutable
  historical
  evidence이고 finalizer/Rebuild/Download를 반복하지 않는다. Approval, reviewed
  rebaseline, exact artifact-to-Download binding과 PLC/runtime qualification은 없다.
- PC reconnect correction commit `66b5cf2`를 포함한 `RunPcTests` 대상의 2026-08-10
  당시 Debug/Release PC suite는 Visual Studio 2019
  MSBuild 16.11.6에서 warning 0/error 0이고 standalone runner가 각각
  `1117/1117` PASS다. `0x8080` short failure 회귀 6개는
  `ParseAcknowledgement`가 outer `Status=1`, command `Status=1`, `ErrorId=-1`을
  보존하는지, legacy가 재시도하지 않는지, `Version2WakeHint`의 exact
  `HeaderReserved=0`/4-byte/command 1/error -1 envelope만 20 ms cancellation-aware
  대기 뒤 같은 socket에서 한 번 더 시도하는지를 확인한다. transient 성공 순서는
  `0x8080 -> 0x8080 -> 0x405C`이며, persistent second failure는 `Faulted` cleanup,
  다른 ErrorId와 nonzero reserved는 zero-retry다. retry-delay cancellation은 두 번째
  request 전에 중단되고 `Cancelled` evidence와 첫 `-1` ACK를 보존한다. 이는 PLC의 지속적인 disarm result
  `-8`/`-9` root fix가 아니다. 일반 RPC/lifecycle negative disarm은 tuple을 보존하고
  다음 init이 같은 fence를 재시도하는 PLC fail-closed 계약을 유지한다. PLC source의
  별도 owner-loss retirement는 accepted owner transition 또는 definitive current-socket
  disconnect에서 ordinary helper가 정확히 `-8`을 반환한 경우만 internal `(0,0,0)`
  sentinel을 사용한다. `-9`, different-IP/unknown candidate, failed takeover와 late
  retiring-old disconnect는 해당 경로를 사용할 수 없다.
  callback tranche A는
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
- commit `bff3bc7`은 standalone runner에 exact mode `callback-ownership-wire`와
  전용 회귀 16개를 추가했다. 기본 호출과 `--dry-run`은 network access 없이
  `all` 또는 지정한 `gd-n10a`, `gd-n13-candidate`, `gd-n14-candidate` 계획만 출력한다.
  Current `cbf2548` SDK Debug/Release direct `RunPcTests`는 각각 `1133/1133`
  PASS다. `bff3bc7` 당시 독립 reviewer의 Release 재실행도 `1133/1133`이었고, 그
  commit에서 WPF source가 바뀌지 않은 채 재실행한 `335/335`는 historical checkpoint다.
  live parser는 `--execute-live`, exact case-sensitive
  `--confirm PLC-CALLBACK-OWNERSHIP`, concrete `--scenario`(`all` 금지), explicit
  `--host`/`--owner-local`/`--candidate-local` IPv4, 세 40/64-hex Git object로 구성한 declared
  `--source-fingerprint HEAD/TRACKED/UNTRACKED`, 존재하지 않는 `--output`을 모두
  요구한다. unspecified/broadcast IPv4는 거부하며, N13은 owner/candidate source IPv4
  동일, N10A/N14는 서로 다름을 요구한다. N10A candidate callback port는 `0`이어야
  actual owner UDP endpoint를 재사용한 advertised-IPv4-only mismatch가 된다. output
  예약과 fingerprint 형식 preflight는 network보다 먼저 실행되고 기존 output은 덮어쓰지
  않는다. fingerprint는 선언값을 기록하는 guard이며 tool이 worktree 또는 downloaded PLC
  identity와 일치함을 독립 증명하지는 않는다.
  allowlist는 byte-exact `0x8080`, mask `1`/max `52`/nonzero cookie/zero
  flags-reserved인 version-2 `0x405C`, authoritative owner의 `0x405D`뿐이고 retry는
  0회다. arbitrary command/payload/retry/downgrade/write/motion/reset/Download option은
  없다. N10A는 same-socket A success -> callback IPv4 only B failure -> byte-identical A
  duplicate/same fence, N13은 same-IP owner -> candidate takeover/epoch advance -> old-owner
  retirement -> candidate duplicate/same fence, N14는 different-IP candidate가 `0x8080`
  뒤 clean peer close되고 candidate `0x405C/0x405D` zero-wire -> owner duplicate/same
  fence를 검사한다. timeout/aborted/shutdown 또는 N13 old-owner retirement 부재는
  INCONCLUSIVE다.
  report는 `LMC_CALLBACK_OWNERSHIP_WIRE_V1`, mode/scenario, executable SHA-256,
  Git HEAD/checkpoint, declared fingerprint, endpoint/timeout, request/response bytes/SHA-256/
  hex, `RETRY_COUNT=0`, PASS/FAIL/INCONCLUSIVE와 exception을 보존한다. 동시에
  `EVIDENCE_CLASS=PC_RAW_WIRE_HARNESS`, `PEER_IDENTITY=UNVERIFIED`, pcap/PLC Watch
  not captured, `QUALIFICATION_COMPLETE=FALSE`,
  `INCOMPLETE_WITHOUT_PCAP_AND_PLC_WATCH`를 고정한다. 따라서 16 tests와 tool PASS는
  PC-only wire 계약이다. reviewed rebaseline, exact downloaded checkpoint, site maintenance,
  correlated pcap과 PLC Watch 없이는 PLC qualification 또는 runtime PASS가 아니다.
- `RunLasalContract` historical successful-checkpoint coverage(latest clean tracked 결과는 아래
  `d4204b4` SourceOnly PASS 기록이 우선함):
  `PASS LASAL.StaticContract.SourceOnly` (Admin read, `0x7D22`와 dormant
  `0x7D12/0x7D13`, 9축, CyWork-only, D1~D3와 D4
  single-bank Ring/Trigger 및 D5 general-inline SDO Read active source,
  Axis 1 exact D5 Write active, Axis 2..4/비승인 D5 Write와 D4 Double·extended fail-closed wire)다.
- `RunLasalNetworkContract` historical successful-checkpoint coverage(post-approval full/network
  target은 미실행): `PASS LASAL.StaticContract`;
  `LMCDiagnosticsService` constructor의
  38-state 이름/타입, 37개 scalar, 24-entry Bulk array,
  no-control-flow/final-`C_OK` exact gate와
  `LMCRecorderStore` constructor exact 초기화/publish-last negative fixture,
  `Classes.lcb` general `TryStartRead` declaration,
  4축 executor network와 generated metadata 포함
- `BuildSimpleExampleApp`: D5 runner 포함 `LMC_Library/LasalApiWpfTestApp`의
  `f337fec`/`ad7c8b1` 2026-08-10 Release 스냅샷은 `334/334` PASS였다.
  2026-08-11 `af4ab63` historical Release는 VS2019 MSBuild 16.11.6.22506 Rebuild
  warning 0/error 0이고 full smoke runner `335/335` PASS였다.
  `Wpf.CallbackV2.PersistentInitFailureCleansUpAndManualReconnectUsesNewSession`은 첫
  Connect의 exact short failure 2회 뒤 `Disconnected`/`Stopped`, Connect 재활성,
  내부 connection 제거와 session-1의 `0x405C`/`0x405D` zero-wire를 확인하고, 다음 수동
  Connect가 새 TCP session에서 `0x8080 -> 0x405C`로 성공하는 것을 고정한다.
  신규 `Wpf.CallbackV2.ErrorZeroInitFailureCleansUpAndManualReconnectUsesNewSession`은
  같은 short ACK에서 `ErrorId=0`이면 `0x8080`을 정확히 1회만 보내고 canonical retry를
  사용하지 않으며, full cleanup 뒤 다음 수동 Connect가 새 TCP socket/session을 쓰는 것을
  고정한다. 두 회귀 모두 요청값 `RequestedCallback=127.0.0.1:0`을 보존한다. 실패
  evidence는 `BoundCallback=not-bound`, 성공 evidence는 실제 양수 ephemeral endpoint를
  `BoundCallback`으로 보존한다.
  `Wpf.CallbackV2.ExplicitCloseFixedPortThenReconnectSucceeds`와
  `Wpf.CallbackV2.ExplicitCloseMinusOneFixedPortThenReconnectSucceeds`는 같은
  `MainWindow`에서 첫 version-2 connection이 사용한 fixed UDP port를 explicit Close 뒤
  실제로 다시 bind할 수 있고, 두 번째 Connect가 새 `LMCConnection`/TCP session에서 같은
  requested/bound port로 `0x8080 -> 0x405C`를 완료하는지 검사한다. 두 번째 회귀는 첫
  `0x405D`의 `ErrorId=-1`과 `LastCloseException`을 보존하면서도 local listener/TCP cleanup을
  완료하는 경계를 추가한다. 두 회귀는 loopback fake-RPC/UDP와 PC local-port 수명주기
  증거일 뿐 PLC disarm, owner-loss sentinel, same-IP takeover 또는 live reconnect 증거가
  아니다. 기존 `339/339` historical count에 소급 포함시키지 않는다.
  Reconnect policy `14ccf58`은 Debug/Release Rebuild PASS, full smoke `339/339`, reconnect
  targeted filter `6/6`을 PASS했다. 독립 callback/reconnect filter도 `9/9`, P0/P1
  없음이다. 초기 및 동일 프로세스 내 후속 Connect에서 첫 candidate가 exact canonical `-1`을
  두 번 받아 `Outcome=Failed`, `AttemptCount=2`, `CanonicalRetryUsed=true`이고
  RPC/callback 미시작인 경우만 `RPC_INIT_FRESH_TCP_ONCE_V1`이 그 candidate를
  retire/`Dispose`한다. fixed 100 ms 뒤 fresh `LMCConnection`/TCP를 정확히 한 번 열고
  두 번째 candidate 실패는 terminal이다. `ErrorId=0`, 다른 ErrorId,
  malformed/transport/cancellation/callback-stage failure는 outer retry가 없다. 사용자
  operation 하나의 상한은 TCP 2개/`0x8080` 4회이고 `0x405C`는 init 성공 뒤에만 나간다.
  정상 registration ACK까지 받아야 Connect가 성공하며 `0x405C` 실패는 terminal이고
  outer retry가 없다.
  내부 replacement와 창 X의 공용 cleanup은 최대 두 번 `Dispose` 후 complete local
  disconnected postcondition을 요구하고 old `RpcCloseResponse`/`LastCloseException`을
  보존한다. X는 미완료 시 취소되며 strict Close 버튼은 close 실패 시 cleanup 뒤 그
  오류를 throw한다.
  startup evidence는 `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`, `SdkPath`,
  `SdkBuildUtc`를 기록하고 topology marker V5는 유지한다.
  100 ms는 PLC readiness proof가 아니고 wire `-1`은 internal disarm `-8`/`-9`와 다른
  lifecycle/ownership rejection을 구분하지 못한다. 이 historical fake restart는 같은
  process의 새 `MainWindow`를 사용한 회귀로 계속 보존한다.
  2026-08-12 predecessor `e3c9365` owner-loss retirement source의 격리 LASAL incremental Build는 두 ST
  compile/link를 `0 error(s), 24 warning(s)`로 PASS했다. Post-build `Classes.lcb=5337BBAF...`는
  comparator exit `3`으로 거부됐고 PLC Download/runtime 재검증은 실행하지 않았다.
  Current `cbf2548` 별도 actual-EXE relaunch gate는 Debug/Release 각각 `1/1` PASS했다.
  Runner는 actual PID/HWND에 외부 `WM_SYSCOMMAND/SC_CLOSE`를 보내 owner의 X close,
  close ACK exact `-1` 뒤 process exit, 같은 exact EXE successor의 default named mutex
  재획득을 확인한다. Successor의 첫 TCP candidate는 `0x8080` exact `-1` 두 번,
  `0x405C/0x405D` 0회이고 fresh candidate는 init/registration/close를 성공한다. Exact
  fake-RPC session/request는 `3/28 (13,2,13)`이다. malformed probe는 exit `64`, owned
  temp write `0`, TCP session `0`이고 live owner 중 mutex contender는 exit `2`, TCP
  session `0`이다. EXE/DLL/optional config의 path/length/SHA-256은 전후 동일하다.
  Development Debug EXE/DLL SHA-256은 각각
  `62B2AB41B90024C8CD07328927EED5D325471EC0E6666F1C5E0DD66521F62A99` /
  `E64B49E2F7532B23886288B263985E4A906F30840CCFCB088197BE936877C621`, Release는
  `BB91C40C4D60AEB7FB9959A8A3E4F8877490BDBE719F968CAA9F454A3D24ACB5` /
  `7D179781BCE9EB2FE6DB071C3D45F085A5BC127F9DBD0E15300E38A6181A7ED8`이었다.
  이 gate도 PC loopback process/mutex/wire 증거뿐이다. PLC cleanup/disarm/readiness,
  100 ms runtime 적정성, MotionLib/축 상태나 사용자 PLC 재접속은 검증하지 않는다. PC local
  cleanup은 PLC disarm 성공이 아니며 private state를 force-clear하지 않는다.
  `Wpf.CallbackV2.QueuedOldSessionStatisticsCannotMutateReplacementUi`는 old-session
  statistics action을 Dispatcher에 먼저 queue한 뒤 connection을 교체하고, action 처리 후에도
  replacement의 네 counter가 0, last decision이 `None`, summary가 `rejected=0`, active owner가
  replacement로 유지되는 것을 결정적으로 고정한다. 신규
  `Wpf.CallbackV2.D5TerminalWakeSingleFlightUsesAuthoritativeStatus`는 WPF가 callback v2
  `eventMask=1`/max datagram 52로 등록하고, exact retained D5 ticket wake만 UI에 queue한 뒤
  authoritative TCP `0x7E03`을 single-flight로 1회 조회하며 UDP hint 자체는 operation status를
  변경하지 않는 causal 경계를 고정한다. 같은 smoke는 callback 등록의 BootId/Epoch/cookie/listener
  generation/source/mask와 PC receiver의 accepted/rejected/duplicate/out-of-order 및 last decision을
  동일 event snapshot으로 표시하는 것도 확인한다. 이 GUI evidence는 pcap이나 PLC
  `RpcCallbackLastDisarmResult`/producer/sender counter를 대체하지 않는다.
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
- `BuildDistributionExampleApp`: historical binary-reference distribution example build PASS
- 2026-07-31 historical full distribution preview pipeline: manifest `56/56`, semantic policy `28/28`/15 checks,
  transaction/manual snapshot/provenance `86/86` PASS. temporary standalone example Debug/Release
  build, forbidden internal-reference scan, cleanup과 DLL hash identity를 포함한다.
  검토한 `2.0-candidate` DOCX/PDF exact bytes를 사용한 실제 sibling
  `LMC_API_Distribution_candidate_20260731_manual_2_0_provenance`도 schema 2 manifest와
  semantic preflight를 통과했다. 이 결과는 `dirty-preview` PC/package 증거이며 canonical
  승격이나 PLC/runtime/production 승인이 아니다.
- Current `cbf2548`에서 `Build-LmcApiDistribution.ps1`은 candidate `Run`에
  binary-reference EXE/DLL을 복사한 직후, manifest 전에 actual-EXE gate를 호출하고,
  이후 transaction 완료 전 tested EXE와 final EXE SHA-256 equality를 다시 검사한다.
  별도 temp binary-reference candidate는 `ProjectReference=0`, optional config absent 상태로 gate
  `1/1` PASS했다. EXE SHA-256은
  `829AC3314E1B5113696DFA06E64418A95C305035335F73DEB4404449CF910F79`, SDK SHA-256은
  `7D179781BCE9EB2FE6DB071C3D45F085A5BC127F9DBD0E15300E38A6181A7ED8`이고 전후 identity도
  같았다. 그러나 2026-08-11 historical full Distribution attempt는 SDK Debug `1133/1133` 뒤 기존
  `Verify-LasalContract.ps1:7571` `$macroMatches[-1]`의 PowerShell 5.1 비호환 tooling
  bug에서 중단되어 script의 copy 직후 gate와 manifest 단계에 도달하지 않았다. pwsh7은
  last Match를 반환하지만 powershell 5.1은 null을 반환해 `lastMacroEnd=0`과 false
  macro-to-custom drift를 만들었다. PLC/source/Classes/`cbf2548` blocker가 아니다.
  Transaction residue는 `0`이다. 후속 pwsh7 focused
  `-AxisOwnershipReserveVerifierSelfTestOnly`는 exit `0`, negative fixture `62/62` reject와
  comment-only fixture accept를 64.3초에 PASS했다. Compatibility commit `ad4af91`은 exact
  한 verifier file의 PS5.1 negative-index 접근만 수정했고 targeted PS5/PS7 Publish+Reserve를
  PASS했다. 수정 뒤 PS5.1 Release `RunLasalContract`는 해당 macro 경계를 통과한 다음
  177.7초에, `RunLasalNetworkContract`는 174.9초에 기존 intentional
  `LASAL.UdpCallbackContract blocker: Classes.lcb sanctioned Gate D identity drifted`로 각각
  exit `1`이었다. 사용자 current `Classes.lcb`는 수정하지 않았다. 따라서 full Distribution
  prerequisite가 STOP이고 script의 new EXE gate/manifest에는 도달하지 않아 full
  Distribution, manifest 또는 candidate publish PASS로 기록하지 않는다.
- Commit `88f1c57`은 staged `LasalApiWpfTestApp.sln`의 exact C# project 1개,
  project-file GUID 일치, Debug/Release `Any CPU`의 `ActiveCfg`/`Build.0`을 검사하고 동일
  solution을 Debug와 Release로 Rebuild한 뒤에만 `Run` copy/gate를 실행한다.
  `Test-LmcApiDistributionPipeline.ps1`은 PS5.1/PS7 모두 `129/129` PASS다. Commit
  `bf31030`은 release input fingerprint를 exact root `.lcp/.lcb`, tracked
  Class/Include/Source와 tracked+physical Network 전체로 확장했다. ignored seeded Network
  `.lba/.lob` 8개를 포함한 5개 post-populate drift 시나리오는 candidate 미생성, canonical
  hash 불변, stage/lock residue `0`으로 fail-closed했고 PS5.1/PS7 pipeline은 각각
  `192/192` PASS했다. Commit
  `d735446`의 Control `HandleRequest` focused verifier는 PS5.1/PS7 `13/13` PASS다. 후속
  `d6ddf05`는 method-size parser를 checkout/EOL-stable하게 고쳐 main mixed-EOL과 clean
  detached의 current scan을 동일한 `101/98/3`으로 만들었고 exact-current-baseline self-test는
  PS5.1/PS7 `16/16` PASS다. 먼저 `88f1c57`, `d735446`, `afdf6a3`이 포함된 clean detached
  `afdf6a3`에서 exact `2.3-candidate` manuals로 full Distribution build를 실행했고 약 `214`초
  뒤 첫 Debug `RunTests`의 no-approved-ratchet Gate D STOP을 확인했다. 후속 `d6ddf05`와
  `bf31030`까지 포함한 clean detached `bf31030`에서 direct Windows PowerShell로 다시 실행한
  결과도 exit `1`, `214.415`초에 같은 Debug `RunTests` wrapper에서 중단됐다. Direct focused
  `Verify-LasalUdpCallbackContract.ps1 -VerifyCurrent`는 exit `1`, `10.320`초에
  `TerminalWakeBrokerCandidate is structurally valid but has no approved physical snapshot ratchet`
  blocker를 반환했다. 두 실행은 tracked clean이지만 noncanonical manual 입력 때문에
  `-AllowDirty`/`dirty-preview` policy였다. Latest run의 sibling candidate/stage/lock은 없고
  canonical snapshot SHA-256 `17310B7E386BE7FBC03E5D57AFA52CC3C5703F13561C8465E66F6325213A291F`
  (`76` records)와 manual hashes가 불변이었다. actual-EXE gate, manifest와 publish/final rename에는
  도달하지 않았고 LASAL IDE, PLC Download/runtime은 실행하지 않았다.
- Historical predecessor commit `febb1b0`은 `Build-LmcApiDistribution.ps1`의 manual/canonical 경로 확정,
  `vswhere`/Python 등 tool discovery와 transaction보다 먼저 mandatory dual-host tooling
  preflight를 실행한다. Windows PowerShell 5.1과 PowerShell 7은 각각 Pipeline `245/245`,
  SemanticPolicy `50/50` + policy check `18`, ReleaseManifest `56/56`, method-size `16/16`,
  UDP callback `296/296`, Control `HandleRequest` `13/13`의 exact 6-suite를 PASS했다.
  Worker는 poisoned inherited `PSModulePath` 대신 exact `$PSHOME\Modules`만 사용하고,
  suite별 expected evidence 정확히 1개, exact terminal line, stderr 없음과 exit `0`을 요구한다.
  Timeout은 해당 PID process tree에 `taskkill /T /F`를 적용한다. Final Windows PowerShell 5.1
  parent run은 `802.8`초에 `PASS LMC.DistributionToolingHostParity 12/12 (PS5=6/6;
  PS7=6/6) files=92
  SHA256=99D6D27101C126D7D03018763067A2D8A2C02B7FBFF41450641822488305DC62`를 반환했다.
  92-file repository-relative path/length/SHA-256 ordinal digest는 transaction input tree와
  prepared-input/promotion drift check에 묶였다. 이는 PC/tooling predecessor evidence일 뿐이며
  당시 full Distribution은 Gate D STOP으로 actual-EXE/current manifest/publish 전에 멈췄다. LASAL IDE,
  PLC Download/runtime은 실행하지 않았다. 이 92-file evidence는 아래 current 94-file/schema 3
  보강의 historical predecessor로 보존한다.
- Historical predecessor commit `39c3e6f`는 ReleaseManifest artifact의 PS5.1/PS7 ordinal ordering을 고정하고
  schema 3으로 올렸다. Manifest record는 절대경로 없는 8-role
  `role|version|SHA-256`로 구성된다. Git은 실제 core executable, C# compiler는 선택된
  Roslyn/csc 전체 inventory를 해시한다. SDK test/WPF smoke/SDK library/staged example의
  네 실제 `.csproj`에 `CscToolPath`, `CscToolExe`, `RoslynTargetsPath`,
  `CSharpCoreTargetsPath`, `UseSharedCompilation`의 다섯 property를 강제하고
  `UseSharedCompilation=false`를 실행 증거로 확인한다. Python은 `site-packages`를
  제외한 base runtime, `python-docx` 221개, `pypdf` 117개 distribution inventory를
  독립 role로 묶는다. PS5.1/PS7 executable SHA-256 host attestation과 toolchain snapshot은
  transaction fingerprint에 묶이고 promotion 직전 physical re-resolution/hash drift를 fail-closed한다.
  Final PS5.1-parent aggregate는 `808.553`초에 `12/12`, files `94`, digest
  `C25A61055F83B7F171B5FFB7A4F6B821CBC5642EDB2614A9E6D95C7BFBE9F543`을 PASS했다.
  Host attestation SHA-256은
  `A83A038227732EE777F0CDDB1549158633DC0E438B2464200A6EC1ABE0A78215`, toolchain
  SHA-256은 `9EC464FA97755C202D8DF895767889228169678C16364B21507BAC7A5BDE419D`다.
  Mandatory aggregate는 호스트별 exact six suite/`12/12`로 ToolchainProvenance test를 실행하지
  않고 파일만 monitored inventory에 포함한다. 별도 focused PS5.1/PS7은 각각
  provenance `44/44`, manifest `94/94`, pipeline `284/284`를 PASS했다. 이 94-file/schema 3
  결과는 아래 current mandatory seventh-suite gate의 historical predecessor로 보존한다.
- Historical implementation commit `1b9be6a`가 ToolchainProvenance를 host별 일곱 번째
  mandatory child suite로 통합했고 documentation commit `4867096`이 그 증거를 기록했다.
  둘은 current 13-role gate의 8-role predecessor다. Windows PowerShell 5.1과 PowerShell 7은
  각각 Pipeline `286/286`,
  SemanticPolicy `50/50` + policy check `18`, ReleaseManifest `100/100`,
  ToolchainProvenance `49/49`, method-size `16/16`, UDP callback `296/296`, Control
  `HandleRequest` `13/13`을 통과했다. Final PS5.1-parent aggregate는 `831331ms`에
  `PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94
  SHA256=F2B6DE0D9A595983D94D9E0B58B62BDE4B3FAFBE7F24EE1B6114354C3E7848D8`을 반환했다.
  Host attestation SHA-256은
  `CE3D330EE2198070A48D923B43DB33A5E9177D9B4A147B3F46D1772027B34B36`, toolchain
  SHA-256은 `C3219FED42CD96590BAC56A25702599763284D117DBC0A680CE92AB0F8C15A18`이다.
  별도 focused PS5.1/PS7도 각각 ToolchainProvenance `49/49`, ReleaseManifest `100/100`,
  Pipeline `286/286`을 PASS했다.
- Current commit `3c63dea`는 actual DOCX/PDF workload의 exact seven root package owner를
  schema 3 pathless 13-role provenance와 promotion drift fence에 묶었다. Final PS5.1-parent
  aggregate는 `PASS LMC.DistributionToolingHostParity 14/14 (PS5=7/7; PS7=7/7) files=94
  SHA256=F687FDE9198C9F0CDF8AB4106FAB0C3B5059DF49B55C8E9B34DEC99859CDB4CA`를 반환했다.
  Host attestation/toolchain SHA-256은 각각
  `FBAD123C4E3DEC4E9018885559E1645A69E47E69DE0E83D1116F8581D27B787D` /
  `91E56793F99B5D17D9325D425308179FB780161CFFD9D29613653737C2D6F7EB`이다. 두 host
  focused 결과는 각각 ToolchainProvenance `84/84`, ReleaseManifest `108/108`, Pipeline
  `291/291`, SemanticPolicy `52/52` + policy check `18`이다. Exact inventory는
  `CSharpCompiler=108`, `Git=1`, `MSBuild=1`, `PowerShell=1`, `PyPdf=117`, `Python=2489`,
  `PythonCffi=53`, `PythonCryptography=195`, `PythonDocx=221`, `PythonLxml=208`,
  `PythonPillow=219`, `PythonTypingExtensions=7`, `VsWhere=1`이다.
- Active owner set은 cffi/cryptography/lxml/pillow/typing-extensions/python-docx/pypdf exact
  seven이다. cffi metadata의 `Scripts` entrypoint는 Python root-relative path로 normalize한다.
  Base `Python` inventory에서는 `Scripts`/`Lib/site-packages`를 제외하고 package-owner
  inventory로 exact file을 다시 포함하며 active `.pyc`는 보존한다. Ownerless module은
  bounded built-in/frozen/runtime-root/synthetic 계약만 허용하고 `Scripts`, `site-packages`,
  runtime 외부 path는 fail-closed한다. 미로드 `pycparser`와 unrelated package는 제외한다.
  Toolchain probe, semantic extraction, PDF validation, DOCX validation의 네 Python 실행 path는
  모두 `-B`를 강제한다.
- Commit `bcc6a9c`는 reviewed `2.3-candidate` pair를 tracked canonical release input으로
  승격했다. DOCX는 `91,103` bytes / SHA-256
  `F3DC33521A8DB623641FA07A2C1B161009BCF3F01622DC037442A9726900F8DD`, PDF는
  `1,002,300` bytes / SHA-256
  `317A87FC42EF5A845202FFDB384C3AC23247C1B7A73530488C96FF0D805D2880`이다. Word/OpenXML
  validation error `0`, A4 PDF `43`쪽, DOCX heading `66`/table `109`, all fonts embedded와
  43-page visual defect `0`을 확인했고 extracted-text manual policy는 `3/3` PASS했다. 두 host
  focused Pipeline `291/291`, SemanticPolicy `52/52` + policy check `18`, ReleaseManifest
  `108/108`도 PASS했다. Clean detached `bcc6a9c`에서 default resolver canonical 선택,
  worktree clean과 manual policy `3/3`을 다시 확인했다.
- Commit `f304e8b`는 canonical package/example README에 preview/production NO-GO와 current
  SDO 안전 범위를 맞추고 semantic regression을 한 case 늘렸다. Production template와 build
  logic은 바꾸지 않았다. PS5.1/PS7은 각각 SemanticPolicy `53/53` + policy check `18`,
  Pipeline `291/291`, ToolchainProvenance `84/84`를 PASS했다.
- Commit `978597b`는 위 active closure, initial canonical manual과 README policy를 current
  release-input documentation baseline으로 기록했다.
- Commit `d4204b4`는 모든 기존 exact 검사를 통과한 clean tracked `Classes.lcb`
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861` tuple만 PC/static
  `ProductionApproved=true`, `NeedsRebaseline=false`로 승인했다. PS5.1/PS7 self-test는 각각
  `296/296`, clean detached SourceOnly는 두 host에서 PASS했다. 당시 Main working tree 사용자
  `13EA5823DF0887D6042408E2A884E9F8DF50304443227353B9BDCA9AD2ECBFD9`는 reject됐다.
- Commit `5d5aebe`는 Gate D 경계를 반영한 current canonical manual을 게시했다. Markdown은
  `94,108` bytes / SHA-256
  `D7DE1AF51A548AA7361614167D546A7057C8D03260CE92CFA9335964A611C022`, DOCX는 `92,229`
  bytes / SHA-256 `57D17650D1F24E9350830E784EFE94E00CB1A89CB126CD9A05865580A9708B46`, PDF는
  `1,003,309` bytes / SHA-256
  `83A57CC4B15D4E0BA4E0D9A54FD044C82A131168D16B36F2694F76AF098232E0`이다. `bcc6a9c`의
  이전 hash와 검토 결과는 initial promotion historical evidence다.
- 위 결과는 PC/tooling 및 release-input 문서 증거다. Canonical source snapshot direct semantic
  run은 manual/README policy를 지난 뒤 `CANDIDATE_WPF_SOURCE_SET`에서 멈췄지만 fresh staged
  candidate가 아니므로 full Distribution 또는 current candidate PASS가 아니다. Canonical
  tracked Gate D static 승인 뒤 full/network static target과 clean full Distribution/current
  generated schema 3 candidate/full-build actual EXE gate/manifest/publish는 실행하거나 생성하지
  않았다. LASAL IDE, PLC, Download/runtime도 실행하지 않았고 production NO-GO는 그대로다.

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
| D5 | general-inline Read submit/status/cancel, executor release/race와 abort/recovery analyzer 12개, disconnect/orphan application-recovery 코어 28개, four-ticket same-value Write qualification 9개와 WPF adapter 포함 | `10`/`12` happy path와 TypeMismatch recovery live PASS. fake-RPC 2-session WPF handler E2E와 Axis 1 exact Write 전송/Axis 2..4·미승인 target zero-wire는 PC 계약이다. PLC/SDK gate는 Axis 1 `0x2F00:24 Int32/4`만 활성이고 second safety, changed pre-Write zero-mutation, returned-ticket adoption-before-validation을 검사한다. pre-callback/pre-`0x7D12`/pre-`0x7D13` IDE Rebuild/Link와 exact-method Implementation-tab/header smoke는 PASS했지만 current source 재빌드와 실제 Write는 아직이다. sentinel/자동 restore/replay가 없고 single-writer 작업창이 필수다. PLC durable orphan witness/pcap, mailbox/물리 증거 및 offline/timeout/cancel/contention live 대기 |
| Phase 1 facade | typed drive read, PI/Bulk builder/reader와 error catalog PC contract 포함 | `10_DriveRead_Axis1to4`, `11_PI_Bulk_Regression` happy path PASS |
| Admin/group | active `0x7D00/10/20/22`와 dormant `0x7D12/0x7D13` C# golden/parser/fake-RPC 및 LASAL SourceOnly mapping 범위 | active 4개는 `01`~`08c`, `04b`, `09b` happy path 근거가 있다. SetPosition은 bit 3 OFF/native call 0/WPF 미노출이다. Reference는 bit 4 OFF/native call 0/WPF 미노출이고 56/32-byte frame, recipe 1/2, positive MaxTravel/Timeout 계약만 고정한다. 둘 다 current IDE/download/live proof 전 활성화 금지; `0x2047` accepted-then-poll 수정본, queue/race/fault gate 잔존 |

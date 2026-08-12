# LASAL Motion Control API Example 설계

## 1. 목적

기존 WPF 화면의 레이아웃 장점은 유지하되, legacy transport와 simulation 동작을
제거하고 현재 `LasalMotionControlLib`가 PLC와 실제로 교환하는 기능만 제공한다.
처음 사용하는 개발자가 연결, 축 조회, 저속 단축 시험과 그룹 상태 확인 순서를
화면만 보고 이해할 수 있어야 한다.

프로세스 시작 시 Windows session 단위 named Mutex를 `MainWindow` 생성보다 먼저 획득한다.
두 번째 프로세스는 recovery journal이나 callback/TCP port를 열기 전에 종료하여, 부분적인
journal lock 실패 상태로 실행되는 것을 막는다.

## 2. 화면 범위

- 상단 Connection: PLC IP/port, PC local IPv4, callback UDP port, Connect/Close
- Single Axis: object lookup, Power/Reset/Stop, status/position, 3가지 motion
- Group Motion: object/member 조회, Power On/Off, profile Lock/Unlock,
  Reset/Stop, status/position, static 4축 absolute/relative Move Linear와 identity configuration
- EtherCAT / PI: capability, master/slave health, Connect 직후 자동 로드되는 CREVIS configured
  topology, 선택 node health/digital I/O, active signal Catalog와 typed PI value
- Bulk Snapshot: 선택 signal의 same-cycle snapshot, entry status와 raw value
- Recorder: capability-gated Single/Ring 및 Manual/Edge/Window/Mask configuration,
  start/stop/status/header, reconnect adoption, chunk download, CSV와 dependency-free
  downsample plot. Double은 retained qualification/explicit cleanup WPF adapter, exact
  0x7E4A/4B reconnect, token-qualified 0x7E4C/4D Configure-response-loss와 durable release
  crash-window와 config-only manual Configure 계약까지 구현됐다. `ManualActions`,
  `ManualConfigureRoute`, `QualificationExecution`, `ReconnectRecovery` proof/route gate는 PLC
  qualification 전까지 모두 false
- SDO/Write Policy: general-inline SDO Read와 exact allowlist 기반 SDO Write의 ticket
  submit/status/queued cancel, Read의 typed 1/2/4-byte inline 결과 표시/save, Write의
  same-value four-ticket activation proof, 그 proof 후의 safe-axis preflight와 명시적
  two-click 확인, capability 및 write allowlist로 차단되는 임의 Write와 extended result 확인
- Read-only API: Admin capability를 선행 확인한 뒤 physical axis 1~4의 semantic
  parameter, fixed group `0x0100` parameter, typed drive operation mode와 non-atomic
  drive status를 읽는 Phase 1 실기 검증 화면
- Qualification: Group Enable accepted-then-locked, true Buffered A/B, deterministic
  Stop-first, 24-entry Bulk snapshot/lifecycle soak, Recorder Single/Ring/trigger
  lifecycle과 reconnect exact/0/0 discovery, read-only D5 SDO abort -> recovery 및 local
  TCP abrupt disconnect -> distinct-connection application recovery를
  공통 상태·progress·구조화 로그로 실행하는 반복 시험 화면. Group Enable
  qualifier는 일반 Enable과 동일한 durable accepted-once journal/continuation 경로를 사용하며
  `0x2047` replay를 허용하지 않음
- Execution Log: connection state, response 결과와 raw callback diagnostic

runtime `Language` selector의 번역 경계는 UI chrome, 동적 Power/Reset/Stop/Group 복구
action과 안전 guidance, 시작/안전 확인 창과 파일 저장 창이다. `UiLocalizationService`는 원본
English chrome을 보존해 English/한국어를 즉시 왕복 적용하고 기존 WPF data binding을
제거하지 않는다. raw log, result/evidence payload, TextBox 입력값과 protocol token은 번역하지 않는다. selector는
process culture를 변경하지 않아 기존 `InvariantCulture` 숫자 parsing/formatting을 유지한다.
표준 MessageBox/SaveFileDialog의 시스템 버튼 caption은 Windows 표시 언어 소유다.
preference는
`%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\ui-language.txt`에 원자적으로 저장하며,
시험은 주입된 임시 경로를 사용해 사용자 preference와 격리한다. XAML chrome coverage 시험은
새 사용자 표시 문자열에 한국어 catalog 항목이 없으면 fail-closed하며, 동적 안전/복구 action과
대표 guidance는 별도 회귀 시험으로 고정한다.

Motion command는 현재 PLC 활성 경로만 노출한다. Diagnostics command는 SDK surface를
노출하되 `GetCapabilities` 결과로 기능별 버튼을 fail-closed한다. 따라서 PC build에
화면이 포함돼도 PLC가 bit를 광고하지 않으면 실행할 수 없다.

## 3. 실제 API 연결

WPF 프로젝트는 `../LMC_API_Delivery/src/LasalMotionControlLib.csproj`를
`ProjectReference`로 직접 참조한다. command ID, frame offset과 response parser의
기준은 공용 API 소스 하나다.

축과 그룹 object는 이름 lookup으로 얻은 reference를 보관한다. 연결을 닫거나
재연결하면 기존 object를 즉시 폐기하고 다시 Load해야 한다.

callback registration의 library default는 legacy `0x405C` 12-byte payload/4-byte ACK다.
이 WPF는 `Version2WakeHint`를 명시해 32-byte request/20-byte response와 strict 52-byte
`LMC2` typed wake를 사용한다. tracked LASAL interface는 `CurrentPeerValid`, requested
callback IPv4와 TCP peer의 exact match, port `1..65535`를 모두 확인한 다음에만 최초
endpoint tuple을 commit한다. exact duplicate는 멱등 성공하지만 event mask/port/IP가
다른 re-registration은 실패하고 기존 tuple을 보존한다. 설치된 SIGMATEK
`GetBroadCastData.st`의 LSB-first 복원이 peer 비교 byte-order의 정적 근거이며 target PLC
wire capture는 별도다.

SDK는 canonical v2 `0x8080` short failure만 같은 TCP socket에서 20 ms 뒤 한 번
재시도한다. WPF policy commit `14ccf58`은 초기 또는 동일 프로세스 내 후속 Connect에서
그 두 시도가 모두 exact `-1`로 실패하고 `Outcome=Failed`, `AttemptCount=2`,
`CanonicalRetryUsed=true`, RPC/callback 미시작인
경우에만 failed candidate를 retire/`Dispose`한다. 100 ms 뒤 새
`LMCConnection`/TCP를 정확히 한 번 열고 두 번째 candidate 실패를 terminal로 처리한다.
`ErrorId=0`, 다른 ErrorId, malformed/transport/cancellation/callback-stage 실패는 outer
retry 대상이 아니다. 한 UI Connect의 최대치는 TCP 2개/`0x8080` 4회이며 `0x405C`는
init 성공 뒤에만 전송된다. 정상 registration ACK까지 받아야 Connect가 성공하며
`0x405C` 실패는 terminal이고 outer retry가 없다.

SDK `LMCCallbackEventArgs`의 raw provenance와
`LMCCallbackWakeHintEventArgs`/`CallbackWakeHintReceived`의 typed non-authoritative
provenance는 모두 current session에 bind된다. UI handler는 dispatcher 실행 시 active
`LMCConnection`, session, BootId와 retained D5 TicketId를 다시 대조하고 exact match일
때만 authoritative TCP `0x7E03`을 single-flight로 조회한다. old/stale wake는 새 UI나
operation state를 바꾸지 않는다. Gate D source에는 one-attempt broker와 production-path
candidate `PublishEvent(...)`가 있지만 live 52-byte UDP와 causal TCP capture는 미완료다.

내부 replacement와 창 X는 공용 최대 2회 `Dispose` cleanup과 complete local
disconnected postcondition을 사용한다. close response/exception은 진단용으로 보존하며 X는
postcondition 미완료 시 취소되고 strict Close 버튼은 close 실패 시 cleanup 뒤 그 오류를
다시 throw한다.
startup identity는 `ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`, `SdkPath`,
`SdkBuildUtc`를 기록하고 topology marker V5는 유지한다. Historical same-process
새-`MainWindow` smoke는 유지한다. Current executable-gate commit `cbf2548`은 별도
loopback-only probe를 supplied actual example EXE process로 실행한다. Malformed probe는 default
mutex, journal과 network 전에 exit `64`/temp write `0`/TCP `0`으로 닫고, live owner 중
동일 EXE contender는 default named mutex에서 exit `2`/TCP `0`으로 닫는다. Runner는 실제
owner PID/HWND에 외부 `WM_SYSCOMMAND/SC_CLOSE`를 보내 X 경로를 실행하고, `0x405D`
exact `-1` 뒤 process exit와 같은 EXE successor의 default mutex 재획득을 요구한다.
Successor의 session 2는 `0x8080` exact `-1` 두 번 뒤 registration/close 없이 폐기하고,
session 3에서 fresh init/registration/close를 성공한다. 전체 wire는 TCP session/request
`3/28 (13,2,13)`이며 EXE/SDK DLL/optional config identity는 시험 전후 같아야 한다.

`Build-LmcApiDistribution.ps1`은 binary-reference candidate EXE/DLL을 candidate `Run`에
복사한 직후, manifest 작성 전에 `RunWpfExecutableRelaunchTest`를 실행하고, 이후
transaction 완료 전 tested EXE SHA-256과 최종 EXE SHA-256 equality를 다시 확인하도록
fail-closed한다. 다만 2026-08-11
full Distribution 첫 실행은 그 단계보다 앞선 `Verify-LasalContract.ps1:7571`의
PowerShell 5.1 `MatchCollection[-1]` 비호환 tooling bug로 중단됐고 transaction residue는
`0`이다. pwsh7에서는 last Match를 반환하지만 powershell 5.1에서는 null이 되어
`lastMacroEnd=0`과 false macro-to-custom drift를 만들었다. 이는 PLC/source/Classes 또는
`cbf2548` 결함이 아니다. Compatibility commit `ad4af91`은 verifier 한 파일만 PS5.1과
동일 의미로 수정했고 targeted PS5/PS7 Publish+Reserve self-test는 PASS했다. pwsh7 Reserve
run은 exit `0`, negative fixture `62/62` reject와 comment-only fixture accept를 64.3초에
PASS했다. 수정 뒤 PS5.1 Release `RunLasalContract`와 `RunLasalNetworkContract`는 해당
macro 경계를 통과한 다음 각각 177.7초/174.9초에 기존 intentional
`LASAL.UdpCallbackContract blocker: Classes.lcb sanctioned Gate D identity drifted`에서
exit `1`이었다. 사용자 current `Classes.lcb`는 수정하지 않았다. 따라서 full Distribution의
prerequisite가 STOP이고 script의 new EXE gate/manifest 단계에는 도달하지 않았으므로
candidate gate/manifest 또는 full Distribution PASS로 기록할 수 없다. 별도로 만든
binary-reference temp candidate(`ProjectReference=0`, config absent)는 actual-EXE gate
`1/1`을 PASS했고 EXE SHA-256은
`829AC3314E1B5113696DFA06E64418A95C305035335F73DEB4404449CF910F79`, SDK SHA-256은
`7D179781BCE9EB2FE6DB071C3D45F085A5BC127F9DBD0E15300E38A6181A7ED8`이었다.

Current `cbf2548` 검증은 SDK Debug/Release direct `1133/1133`, WPF Debug/Release
Rebuild PASS, 기존 full smoke `339/339`, reconnect targeted `6/6` PASS다. 별도 actual-EXE
relaunch gate도 Debug/Release 각각 `1/1` PASS했고 독립 callback/reconnect review는 `9/9`,
P0/P1 없음이다. 이 결과는 PC loopback process/mutex/wire 증거다. fixed 100 ms, local
cleanup과 EXE 재실행은 PLC readiness/cleanup/disarm/runtime, 실제 MotionLib/축 상태 또는
사용자 PLC 재접속 성공을 증명하지 않는다. private PLC state를 force-clear하지 않는다.

`GetSignalCatalog[Async]`가 반환한 immutable Catalog는 diagnostics owner와 connection session
generation에 bind된다. alias PI Read, Bulk builder 생성/Configure와 PI Write submit은 unbound,
foreign, reconnect-stale Catalog를 wire 전에 거부한다. 로컬 alias 조회는 historical/static
snapshot에도 허용한다. Bulk/Recorder handle도 connection session에 귀속되므로 Close나
reconnect 시 UI의 live aggregate와 handle은 폐기한다. 단, Recorder Start에서 받은
`DiagnosticsBootId + RecordId + BufferId` 텍스트는 reconnect 뒤 adoption 시험을 위해
보존한다. 같은 PLC boot에서 Capabilities를 다시 읽고 `AdoptRecorderAsync`로 새
connection 소유 identity를 만든 뒤 Status 또는 Header를 읽어 configuration metadata를
복구한다. 활성 connection 안에서는 Catalog reload 전에 Bulk/Recorder resource를 먼저
release해야 한다.

CREVIS configured topology는 Connect 성공 뒤 `0x7E00` capability를 갱신하고 bit 14가 있으면
`0x7E11/0x7E12`를 자동 호출한다. 수동 Reload도 capability부터 다시 읽는다. load 시작과 실패
시 기존 topology/selection/output shadow를 지워 이전 session 또는 이전 성공 결과를 current로
오해하지 않게 하고, 실패 영역에 capability/BootId/MapRevision과 오류를 직접 표시한다. 자동
load 자체는 read-only topology 조회뿐이며 motion 또는 mutation command를 보내지 않는다. 창 제목과
시작 로그는 실행 파일 경로, 버전, build UTC와
`CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5` marker를 남겨
오래된 GUI binary를 즉시 구분한다.
`LMC_API_Distribution` 아래 복제본은 이 current source/session-proof 계약과 동기화되지
않은 stale artifact이므로 현재 실행/배포 기준으로 사용하지 않는다.
SDK topology aggregate는 owner/session-bound이며 topology-bound Health/Digital I/O는 unbound,
foreign, reconnect-stale topology를 capability/read RPC 전에 거부하고 원 session generation을
exchange까지 유지한다. raw topology/I/O overload와 로컬 validator는 observation-only 호환 경로다.

현재-session aggregate와 별도로 `ConfiguredTopologySnapshot`은 header 9개 field와 ordered entry의
모든 public field를 detached immutable canonical text로 복제하고 SHA-256을 계산한다. 같은 normalized
PLC IP/port의 마지막 성공 snapshot은 disconnect/reconnect와 load 실패를 넘어 WPF process 수명 동안
유지되며 다음 성공 load를 `INITIAL/UNCHANGED/CHANGED`로 분류한다. endpoint가 바뀌면 비교하지 않고
새 `INITIAL`로 시작한다. stale-session gate를 통과한 성공 commit의 마지막 단계에서만 baseline과
evidence를 함께 교체하므로 실패/늦은 response는 마지막 성공 결과를 오염시키지 않는다. GUI는 짧은
상태와 ordered diff를 분리해 표시하고 `Save Configured Evidence`로 UTF-8 no-BOM TXT를 저장한다.
evidence에는 configured schema only 경계와 runtime discovery, physical cable order, live health/DI/DO
증거가 아니라는 문구를 고정한다.

`Auto refresh live state` monitor는 configured schema와 live sample을 섞지 않는다. DataGrid의
`CFG` 열은 bit 14 topology snapshot을 유지하고 `LIVE` 열만 bit 15 node health와 bit 16 selected
digital input으로 갱신한다. 한 timer tick은 shared command gate를 nonblocking으로 얻고,
owner/session-bound cached capability snapshot을 pinned SDK overload에 전달한다. eligible tick의
실제 wire는 추가 capability refresh 없이 `0x7E13` 또는 `0x7E22` 1회다. 기존 non-pinned API의
capability refresh+read 계약은 유지한다. 7개 node를 round-robin하고 선택된 input read를 사이에
넣는다. foreground,
safety, qualification, in-flight와 bounded failure backoff 중에는 skip한다. disconnect, topology
reload와 selection 변경은 generation을 무효화해 늦은 response를 폐기하며 transport request를
취소하지 않는다. bit 15/16이 모두 off인 현재 source에서는 timer가 실행돼도 wire request는 0회다.
background monitor는 output shadow를 읽거나 `selectedDigitalOutputShadow`를 갱신하지 않으므로
output write 승인 provenance는 명시적 사용자 read에만 생긴다.
수동 Health/DI read도 시작 시 capture한 owner/current-session capability snapshot을 pinned
overload에 전달해 data request 앞의 추가 `0x7E00`을 제거한다. Auto/Manual Health/DI의 성공과
실패는 current-session commit gate를 통과한 read attempt만 process-local FIFO journal에 기록한다.
journal은 최대 4,096개를 보존하고 overflow 때 oldest record를 폐기해 dropped count를 누적한다.
failure record에는 과거 sample field를 복제하지 않는다. `Save Live Evidence`는 immutable snapshot을
TXT/CSV UTF-8 no-BOM으로 내보내며 retained/dropped count와 endpoint/session/BootId/MapRevision/
capability/topology/node identity를 보존한다. capability-off 경로는 새 wire와 record가 모두
0이고, stale/late response는 원 request가 이미 송신됐을 수 있지만 record로 commit하지 않는다.
이 evidence는 parsed current-session PLC response/read failure의
PC 기록일 뿐 physical cable order, 실제 DI 접점, physical DO feedback 또는 PLC 구현 완전성 증거가
아니다.
current LASAL source에는 `0x7E13/0x7E22` handler, 464-byte snapshot과 CREVIS read-owner가
있고 fresh IDE Rebuild/Link/static smoke를 통과했다. bit 15/16은 의도적으로 OFF이며 PLC
download/runtime/actual-hardware proof는 없다. `0x7E23` output owner와 bit 17은 구현하지 않았다.

Recorder plot은 외부 chart package를 사용하지 않는다. downloaded immutable raw
buffer에서 화면 폭에 맞게 sample을 downsample하고 WPF `Canvas/Polyline`으로 그린다.
CSV export와 plot은 PLC live object를 다시 읽지 않는다. `Cancel Download`는 PC-side
chunk download token만 취소하며 recorder stop/release를 대신하지 않는다.

Recorder Trigger는 Configure payload의 signal/operator/value/mask 조건을 RT recorder가
판정한다. `Trigger Now`는 `TriggerRecorderAsync(0x7E42)`를 호출해 locally configured
non-Manual D4 recorder를 명시적으로 trigger한다. Adopt identity에는 configuration
shape가 없으므로 사용하지 않는다. Ring은 trigger capture에만 사용한다. Double은 PLC
capability만으로 선택할 수 없고 exact external-session-loss recovery/reset까지 준비된 뒤에만
공유 live gate를 열 수 있다. 그 전에는 mode 목록과 Configure, mode-ambiguous Adopt를
wire 전에 막는다.
PC send-priority 계약은 Recorder Trigger/Stop 지연 ACK를 `ResultDiscarded`로 폐기한다.
buffer/configuration/recovered/adopted identity Release 지연 ACK는 이미 실행됐을 수 있으므로
각 resource를 `OutcomeUnverified`로 격리하고 재사용 및 destructive retry를 차단한다.
Configure, recoverable Configure, Start, exact/active Adopt, empty-configuration Adopt의 accepted
typed result가 publication 단계에서 선점되면 SDK는 원 예외에 exact recovery-only resource context를
붙인다. Start failure는 source configuration도 함께 격리한다. WPF 일반/qualification/Double 경로는
SDK context 예외뿐 아니라 SDK 정상 반환 뒤 UI result-application 검사에서 생기는 race도 callback으로
먼저 local recovery scope에 보존한 다음 원 예외를 그대로 전파한다.

Window trigger의 기존 wire 필드는 `TriggerValue=lower bound`,
`TriggerMask=upper bound`로 해석한다. Window signal은 Int16/UInt16/Int32/UInt32로
제한하고 signed type은 signed ordering으로 `lower <= upper`를 검사한다. Edge는
TriggerMask를 항상 0으로 보내고 Mask는 BitField16/32와 non-zero TriggerMask를
요구해 세 경로의 의미를 섞지 않는다.

현재 SDO UI는 slave 1~4, nonzero ObjectIndex, 임의 U8 SubIndex와 ValueType에 정확히
맞는 1/2/4-byte Read를 제출한다. general-inline 활성화에는 bit 8 `SDORead`와 bit 13
`SDOReadGeneralInline`이 모두 필요하다.
`GetOperationStatusAsync`의 terminal `ResultData`를 raw bytes로 표시하고 저장한다.
SDK와 WPF에 extended result parser/download scaffold가 있더라도 current inline policy와
PLC capability가 8/12-byte 및 `0x7E51` 경로를 차단하므로 현재 화면 계약에 포함하지
않는다. SDO Write 인프라는 `0x7E50`의 exact Int32/4-byte request, SDK target descriptor,
PLC global+per-axis compile-time gate와 제출 직전 DS402 상태 재검사, WPF PowerOff/Standstill/stable
  position preflight와 비모달 2단계 confirmation state까지 구현했다. 첫 클릭은 exact
  connection/session/BootId/MapRevision과 immutable request만 arm하고, 같은 요청의 두 번째 클릭만
  confirmation을 소비한다. 요청 필드를 편집하면 기존 confirmation을 즉시 폐기하고 버튼을
  `Arm SDO Write`로 되돌린다. 편집된 요청이나 새 session은 기존 confirmation을 소비하지 않고
  새로 arm한다. 현재 제한 승인 target은 축 1 `UI[24] 0x2F00:24`, Int32/4-byte,
  값 범위 `-1073741823..1073741823` 한 건이다.
  SDK와 PLC의 global 및 axis 1 gate만 TRUE이고 axis 2~4 gate는 FALSE다. 변경 LASAL source를
  Rebuild/Link하고 PLC에 download한 뒤 fresh capability bit 9와 새 BootId/MapRevision이 확인돼야
  same-value qualification의 Write precondition이 열린다. 일반 manual Submit은 그 qualification의
  current-session proof까지 추가로 필요하다. PLC의 DS402 검사는 async
  mailbox 실행 시점까지 상태를 고정하는 hard interlock이 아니라 submit-time precondition이다.
  same-value qualification은 baseline Read 뒤 사용자 확인과 값 불변 pre-Write guard Read를
  수행하고, 최종 두 번째 안전검사, byte-identical Write 1회, guarded exact Readback을 서로 다른
  4개 ticket으로 고정한다. fresh bit 9가 없거나 네 operator confirmation 중 하나라도 없으면
  handler까지 zero-wire이며 이 PC 계약은 PLC/live Write 성공 증거가 아니다.
  PASS가 만든 process-local activation proof는 현재 `LMCConnection` reference/session
  generation, `DiagnosticsBuild`, `DiagnosticsBootId`, `MapRevision`과 exact target tuple/range에
  귀속된다. 일반 manual editor Write는 이 proof 전에 fail-closed하고, 재연결이나
  PLC build/BootId/MapRevision/target 변경 후에 예전 proof를 재사용하지 않는다. mismatch나
  disconnect를 한 번 관측한 proof는 영구 revoke되어 A -> B -> A에서도 살아나지 않는다.
  second-click 실제 전송은 proof-bound capability/target을 SDK의 identity-pinned overload에
  넘기며, SDK mutation gate 안의 fresh Build/BootId/MapRevision exact 비교가 실패하면
  `NotAttempted`와 `0x7E50` 0회로 끝난다.
  public policy evaluation은 immutable approved-target snapshot과 cached
  connection/capability/identity/payload blocker matrix만 사용해 wire를 보내지 않는다. WPF
  readiness는 `EVALUATION_WIRE=NONE`, PLC bit 9와 `NoApprovedTarget`을 각각 표시한다. SDK target
  목록은 Axis1 exact tuple 하나뿐이며, bit 9가 없는 기존 PLC, 축 2~4와 다른 tuple은 wire 전에
  fail-closed한다. Phase 1 PI Write는 SDK compile-time allowlist가 empty인
것에 더해
`Phase1AllowsPiWrite=false`가 input/button을 비활성화하고 click handler도 다시 거부한다.

Read-only API 탭의 Admin 흐름은 `GetAdminCapabilitiesAsync(0x7D00)` 성공 결과를
connection-local UI cache로 보관한 뒤에만 axis/group 버튼을 연다. capability refresh를
시작하면 기존 cache를 먼저 폐기하고, 실패한 응답 뒤 stale capability로 read를 계속하지
않는다. axis parameter는 physical reference 1~4와 6개 semantic key만, group parameter는
reference `0x0100`과 3개 key의 선택 mask만 허용한다. Close/reconnect에서는 capability
cache와 표시 결과를 모두 지운다.

Drive read는 선택한 physical reference와 현재 loaded axis가 일치하면 그 handle을
재사용하고, 아니면 `_LMCAxisN`을 새 session에서 lookup한 뒤 반환 reference를 다시
검증한다. `ReadDriveStatusAsync`는 LASAL axis status, DS402 `0x6041:0`, operation mode
`0x6061:0`을 순차 실행하므로 atomic same-cycle snapshot으로 표시하거나 해석하지 않는다.
`HasDs402Fault`는 그 SDO `0x6041` bit 3에서만 계산한다. 별도
`GetDriveErrorCodeAsync`는 `0x603F:0 UInt16/2`를 exact one-attempt tracked D5 read로
실행하며 기존 Drive Status composite의 2-SDO 계약을 바꾸지 않는다. `0x2028`의 reserved
`StatusWord=0`, LASAL `AxisErrorId`, DS402 Fault와 drive error code는 UI에서도 분리한다.
이 탭에는 motion/write control을 추가하지 않는다. `0x7D12 SetAxisPosition`은 SDK
wire/one-shot 계약과 PLC fail-closed parser만 존재하고 capability bit 3과 native mutation이
꺼져 있다. SetPosition은 diagnostics identity와
4 x U32 intent를 보존하는 독립 journal core, SDK `0x7D14` exact read-only query와
`0x7D1A` nonzero-generation terminal retirement 계약까지 구현한다. bit 5는 query-only,
bit 7은 retirement 전용이다. journal은 wire 전 Arm, 재시작 시 Armed를
RecoveryRequired로 보수 승격하고 자동 replay하지 않지만, current PLC에는 bit 5/7,
retained store/query/retirement route가 없다. journal을 MainWindow
초기화/dispatch/interlock에 단독 연결하지 않는다. PLC outcome
store/query/retirement, unified ownership, authoritative max-jump와 task/core/priority,
PLC proof가 끝나기 전에는 SetPosition 버튼을 추가하지 않는다.

`0x7D13`의 current 의미는 switch-search `StartAxisReference`가 아니라 Single Axis 탭의
`LMC_Home CurrentPositionZero`다. 실행 직전 actual position을 stale-read guard로 고정하고
target 0을 요청하며 motion enable이나 Home/limit switch 탐색은 하지 않는다. Admin feature
bit 4는 source에서 ON이다. WPF는 one-shot start 뒤 `0x7D18` exact terminal outcome을
조회하고 terminal record를 `0x7D19`로 retire하며, `LMC Home outcome:` 로그에 record state,
original status/error/detail, axis status/error, raw/application/internal position set,
native/evidence/stop/runtime/generation을 기록한다. Start ACK나 `Read Home Status` RPC PASS만으로
완료를 판정하지 않는다. 성공 raw feedback 창은 wrap-safe `-2/-1/0/+1/+2 count`이고 `+/-3 count`
이상은 fail-closed한다. Axis2의 `8382700 -> 8382701`과 Axis1의 `8027834 -> 8027836` false `-7`을 반영한 current
source는 아직 C78 Rebuild/Download되지 않았다.

별도 DS402 method 37 current-position-zero source는 `0x7D15/0x7D16/0x7D17`에 있으나
`LMC_DIAG_DS402_HOME_ENABLED=FALSE`, Admin bit 6 OFF다. Diagnostics encoder-maintenance
`0x7E53/0x7E54/0x7E55`는 source-on이며 TW[20] `0x20FC:0x02 <- UInt16 1`, TW[19]
`0x20FC:0x01 <- UInt16 1`만 허용한다. 두 경로 모두 terminal protocol evidence와 실제 drive
효과를 구분한다. 최신 Home ownership receipt 수정 뒤 C78 Rebuild/Download, 새 BootId와
한 축 단독 runtime proof는 아직 남아 있다. Admin `0x7D00/10/20`은 LASAL IDE
build/download가 일치해야 한다. 화면은 2026-07-23 happy-path PASS와 아직 남은
invalid/stale/fault 검증 경계를 함께 명시한다.

## 4. UNIT 규칙

API 입력은 LASAL internal DINT다. Axis와 Group 화면은 숫자 배율을 직접
입력하지 않고 application UNIT 콤보에서 변환 방식을 선택한다. 기본 선택은
현재 PLC 축 설정과 같은 `mm (x10000)`이다.

```csharp
var raw = checked((int)Math.Round(
    engineeringValue * unitMultiplier,
    MidpointRounding.AwayFromZero));
```

- 선택 가능한 application UNIT은 `mm`, `m`, `deg`다. 이 화면은 하나의 축
  application UNIT을 모든 motion 인자에 공통 적용하므로 `RPM`, force, time,
  memory UNIT은 노출하지 않는다.
- `None / raw DINT`는 배율 1의 engineering unit이 아니다. 이미 변환된 정수
  DINT를 그대로 송수신하는 모드이며 소수 입력은 거부한다.
- NaN, Infinity와 DINT 범위 초과는 송신 전에 거부한다. 선택 UNIT이 있으면
  actual position은 `raw / UNIT`, Raw 모드이면 raw DINT만 표시한다.
- `mm (x10000)`은 PC application UNIT이다. 현재 Git 추적 축 transmission은
  `ExUnits=8388608`, `IntUnits=1 mm(10000)`이며 두
  설정을 같은 값으로 취급하지 않는다.
- `117440512 DINT`는 `mm` 선택에서 `11744.0512`, Raw 선택에서
  `117440512`로 입력한다. 이 변환 가능 범위와 PLC/장비 motion limit는 별개다.
- Absolute/Relative는 `Shortest`, Relative 방향은 distance 부호를 사용한다.
- Velocity는 Positive/Negative만 사용하고 deceleration 인자는 0으로 보낸다.
  제어 감속은 Stop 입력으로 전달한다.
- velocity, acceleration, deceleration은 0보다 커야 한다. UNIT 변환 후
  1 DINT count 미만이 되는 양수도 송신 전에 거부한다.
- Jerk 화면값은 `_LMCAxis` 입력 단위인 `axis application unit/s^3/1000`이며
  `Jerk DINT = 화면값 x UNIT`으로 변환한다. 물리 jerk를 직접 알고 있으면 먼저
  `1000`으로 나눈 값을 화면에 입력한다. `0`은 허용하고 음수는 거부한다.
- 현재 저장된 축 설정은 `_JERK_PROFILE`, `JMax=75000 mm`다. 실제 시험에서는
  다운로드된 PLC 설정과 장비 제한을 별도로 확인한다.
- Group position/dynamics도 별도 Group UNIT 콤보로 같은 DINT 변환을 수행한다.
  Read Position은 static member-slot alias인 `None/ACS`를 허용한다. Move Linear
  Absolute는 X/Y/Z/U를 target으로, Relative는 같은 입력을 delta로 해석하며 두 motion
  경로는 coordinate `None`만 허용한다. ACS 선택 중에는 Move 버튼을 비활성화한다.
- Relative는 Admin `0x7D22` capability가 없는 PLC에서 API가 송신 전에 거부한다.
  PLC가 `MoveRelativeCoord`를 직접 호출하므로 UI는 현재 위치를 합산해 absolute target을
  만들지 않는다. Admin ACK는 queue 수락이며 완료는 기존 Group InPosition monitor다.
- Group finite-motion monitor timeout은 첫 네 축의 absolute distance 합과
  velocity/acceleration/deceleration으로 보수적으로 계산하고 25% 및 5초 여유를 더한 뒤
  15~600초로 제한한다. timeout 뒤에도 motion-uncertain 상태와 Group Stop 경로는 유지한다.
- group Jerk 입력도 `group application unit/s^3/1000` 값으로 보고 UNIT을 곱한다.
  canonical `_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`다.

## 5. 안전 상태

- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 arm 체크박스와
  확인창 없이 버튼 클릭 시 입력 및 상태 검사를 통과하면 즉시 송신한다.
- Group 준비 순서는 `1 Power On -> 자동 0x2045 poll의 PowerOn=True 3회 연속 확인 ->
  3 Set Identity -> 4 Enable(Lock Profile) -> 자동 0x2045 poll의 PowerOn +
  Enabled/Locked Standby 3회 연속 확인 -> 6 Move`다. 종료는
  `Disable(Unlock Profile) -> 7 Power Off -> 자동 0x2045 poll의 PowerOn=False 3회
  연속 확인` 순서다.
- Group Power On/Off 응답은 mode-change 요청 수락만 뜻한다. 화면은
  `BeginGroupPowerOnWaitForStableStateAsync` 또는
  `BeginGroupPowerOffWaitForStableStateAsync`로 `0x204A`/`0x204B`를 정확히 한 번 보내고,
  `ResumeGroupPowerStateWaitForStableStateAsync`의 `0x2045`-only 3회 연속 proof 뒤에만 Power Ready
  또는 Power Off를 확정한다. success ACK와 continuation은 SDK의 session-bound priority
  publication에서 원자 설치되고 accepted observer는 첫 status 전에 WPF journal을 갱신한다.
  timeout/cancel/status failure와 typed interference는 exact continuation/evidence를 보존하며 Power
  command를 자동 replay하지 않는다. stale/resolved/concurrent Resume과 pending transition 위 fresh
  Begin은 wire 전에 typed failure로 끝난다. 일반 Read Status 한 번만으로 Power 또는 Enable pending
  transition을 완료하지 않는다. 다만 safety generation 검증을 통과한 성공 응답은 상태에 맞는
  pending Enable continuation proof에 누적되고 Locked Standby proof가 3/3이면 기존 ACK를 재사용한
  zero-wire Resume으로 완료할 수 있다. Power Off pending 동안 다른 group 준비 명령과 Read Position은
  막는다. `0x00010000`은 Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Power durable journal은 endpoint IP/port, group name/reference, DiagnosticsBootId,
  MapRevision, expected direction과 `ArmedBeforeDispatch`, `AcceptedAwaitingProof`,
  `RecoveryRequired`, `Resolved` 상태를 공용 record로 보존한다. fresh On/Off는 wire 전에 arm하고
  accepted observer가 durable Accepted를 먼저 기록한다. process restart의 Accepted는 exact endpoint,
  BootId/MapRevision과 read-only lookup reference가 일치할 때 status-only로 확인한다. startup의 Armed
  또는 outcome-uncertain Power On은 RecoveryRequired이고 Power On replay/status-only resolve를 금지한다.
  사용자가 명시한 Power Off takeover만 On record를 Off record로 원자 교체하며 최종 PowerOn=False
  stable proof가 필요하다. uncertain Off는 먼저 status-only false proof를 수행하고, typed interference
  또는 exact successful PowerOn=True 관찰 뒤에만 explicit replacement를 허용한다. replacement
  reject/pre-wire failure는 이전 Off record와 replacement 권한을 보존한다. active record는 endpoint와
  group 편집, 새 mutation, connected clean Close/reconnect를 막고 exact-identity recovery와 safety/read-only
  동작만 허용한다. journal open/write/lock failure도 new Group Power를 fail-closed한다.
- On/Off accepted ACK 뒤 첫 status를 보류한 상태에서 child process를 강제 종료하는 회귀는
  journal single-writer lock 재획득, 새 process의 `0x204A`/`0x204B` zero-replay,
  `0x2045` 3회 stable proof와 동일 identity `Resolved` tombstone을 검증한다.
- Group Enable/Disable은 power가 아니라 configured profile Lock/Unlock이다.
  Enable ACK만으로 lock 완료를 판정하지 않는다. 같은 connection/session/group-reference의
  `Read Status`에서 PowerOn과 `0x00020000` Enabled/Locked Standby를 3회 연속 확인한 뒤에만 Move를 활성화한다.
  현재 추적 PLC `0x2047` handler는 `LockProfile`의 `_LMCPROF_NoError`를 request
  acceptance로 ACK하고 같은 cycle의 `LockState`를 완료 판정에 사용하지 않는다.
  최종 완료는 PC가 `0x2045`를 poll해 판단하는 accepted-then-poll 계약이다. timeout/cancel/status
  실패 뒤에는 accepted continuation을 보존하고 Resume에서 `0x2047` 없이 `0x2045`만 다시 보낸다.
  동일 connection/session/group-reference의 다른 handle도 같은 pending/status proof를 공유한다.
  Stop/PowerOff safety 예약은 누적 status proof를 즉시 초기화하되 accepted ACK와 pending
  continuation을 보존한다. 예약 뒤 도착한 status response는 drain 후 `ResultDiscarded`로
  폐기하고 observe하지 않는다. 예약 전에 SDK completion publication이 끝났지만 WPF 적용 전에
  safety가 예약된 좁은 경우만 recovery-required로 승격한다. connected unresolved 상태에서는
  group 이름 변경, group 재조회, clean connection/window close, connected reconnect와 새 Power On을
  차단한다. 외부 connection loss 뒤 reconnect 진입에서는 원 exact group 이름을 보존한 recovery로
  승격하고 새 session에서 그 이름의 group만 다시 조회한다. 명시적 `0x2048 GroupDisable` ACK는
  Unlock 요청 접수만 뜻하며 pending/recovery를 해제하지 않는다. accepted pending과
  recovery-required는 exact group identity에서 PowerOn=True + Disabled/Unlocked 3회 연속 또는
  PowerOn=False 3회 연속 proof가 끝난 뒤에만 해제한다. Power On 성공만으로는 해제되지 않으며
  어느 경로도 `0x2047`을 replay하지 않는다.
  fresh Enable은 endpoint IP/port, group name/reference, DiagnosticsBootId와 MapRevision의 exact
  identity를 별도 durable journal에 `0x2047`보다 먼저 기록한다. 재시작의 Armed record는
  RecoveryRequired로 승격한다. reconnect endpoint는 RPC 전에, BootId/MapRevision은 connect 뒤,
  group reference는 read-only lookup 뒤 검증한다. verified Enable/Disable/PowerOff는 identity
  refresh 뒤 safety generation을 다시 검사하고 durable resolve를 volatile clear보다 먼저 수행한다.
  mismatch 또는 post-identity safety reservation은 recovery를 유지하고 mutation/Enable replay를
  보내지 않는다.
  journal은 기존 `ArmedBeforeDispatch=1`, `RecoveryRequired=2`, `Resolved=3`을 유지하고
  `AcceptedAwaitingProof=4`를 추가해 format-version 1 record와 backward-compatible하다. SDK
  Begin accepted observer는 ACK와 exact continuation publication 뒤 첫 `0x2045` 전에 이 상태를
  기록한다. process-local continuation이 없는 restart는 exact endpoint/group name/reference,
  DiagnosticsBootId와 MapRevision을 다시 확인하고 public `WaitForLockedStandbyAsync`의
  `0x2045`-only 3-sample proof만 사용한다. child-process Kill/restart 회귀는 journal lock 재획득,
  새 session의 `0x2047` 0회, `0x2045` 3회와 동일 identity `Resolved`를 고정한다. 복구 성공도
  process-local Set Identity/Home Check를 복원하지 않으므로 Move는 fail-closed하고 Disable 뒤 준비
  절차를 다시 수행해야 한다. Armed는 accepted status-only 대상이 아니며 계속 safety-only recovery다.
  fresh Disable은 같은 journal에 `ExpectedProfileLocked=false`로 arm하고 accepted observer가
  `0x2048` ACK와 continuation을 첫 status 전에 durable `AcceptedAwaitingProof`로 기록한다. restart는
  exact identity 확인 뒤 cross-session `WaitForStableDisabledAsync`로 `0x2045`만 3회 연속 확인하며,
  child-process Kill 회귀는 `0x2048` 0회, `0x2045` 3회, journal lock 재획득과 `Resolved`를 고정한다.
  stable PowerOff는 더 새로운 safety proof로 pending Disable을 retire하지만 Disable completion으로
  보고하지 않는다. `0x2048` NACK도 Unlock side effect 가능성을 배제할 수 없어 recovery를 유지한다.
  이 변경은
  source/static contract에는 반영됐지만 새 LASAL build/download와 실물 capture로 아직
  재검증하지 않았다.
  Status 조회가 실패하면 local Power Ready와 lock 판정을
  무효화하되 진행 중인 lock 확인은 보존하고, 다음 성공한 Status 조회 전에는
  Power On과 Move를 막는다. 단일 `PowerOn=False` 관찰은 identity를 지우지만 recovery-required
  상태 해제에는 같은 connection session의 3회 연속 proof가 필요하다.
- Group Disable은 motion stop 명령이 아니다. UI는 local motion-uncertain 상태에서
  버튼을 막고, PLC handler가 실제 `ProfileInPosition`을 확인한 뒤에만 unlock한다.
- Stop과 Power Off는 확인창 없이 실행하며 유한 motion 및 standstill 감시 중에도
  사용할 수 있다. 다른 safety 송신 또는 연결 전환이 진행 중인 짧은 구간에는
  중복 송신을 막는다.
- Axis Power On은 `PowerOnAndWaitForStableStateAsync`가 `0x2023`을 한 번만 보내고 success
  ACK를 same connection/session/axis continuation으로 보존한 뒤 `0x2028 PowerOn=true`를 기본
  3회 연속 확인한다. mutation/status gate, ACK/status exchange와 poll delay는 하나의 total
  deadline을 공유한다. write 전 취소는 `NotAttempted`/zero-wire이며, write 뒤 취소는 ACK를
  drain하고 accepted observer가 journal을 먼저 기록한 뒤 typed cancellation으로 반환한다.
  ACK/status 무응답 deadline은 connection을 `Faulted`로 만들고
  `TransportInvalidatedAtDeadline` evidence를 남긴다. same-process continuation Resume과
  reconnect/restart의 read-only `WaitForPowerStateAsync(true)`는 `0x2028`만 보내며 Power On을
  replay하지 않는다. 순수 read-only 결과는 ACK/continuation이 없고
  `ReusedAcceptedAcknowledgement=false`다.
- Axis Power Off는 방향을 포함한 공용 durable v2 journal을 `0x2023(false)` 전에 arm한다.
  SDK accepted observer는 ACK+continuation publication 뒤 첫 `0x2028` 전에 journal을
  `AcceptedAwaitingProof`로 갱신한다. accepted/RecoveryRequired Off의 reconnect/restart는 exact
  endpoint/axis/reference/BootId/MapRevision을 확인한 뒤 status-only 3-sample proof로만 resolve하며
  Power Off를 자동 replay하지 않는다. Power On 불확실성에 대한 명시적 Off takeover는 같은 파일에서
  On identity를 Off identity로 원자 교체한다. journal unavailable은 새 live mutation을 막되 명시적
  safety Off만 process-local degraded tracking으로 허용한다.
- Axis Stop은 `BeginStopWaitForStableStandstillAsync`를 priority safety-send phase에서 실행해
  `0x2022` success ACK와 latest pending continuation을 먼저 보존한다. app send gate를 반환한 뒤
  `ResumeStopWaitForStableStandstillAsync`를 preemptible monitor phase에서 실행하며, Resume은
  `0x2028`만 보내 `IsSuccess && IsStandstill`을 기본 3회 연속 확인한다. 새 accepted Stop은 이전
  continuation을 supersede하고, timeout/cancel/status failure/priority preemption은 continuation과
  typed evidence를 남기되 `0x2022`를 replay하지 않는다. WPF는 accepted Stop monitor 중 동일 Stop
  버튼을 비활성화하고 Power Off만 다음 safety generation을 예약하게 한다. same-session/AxisReference의 later `LMCSingleAxis`
  mutation은 typed interference로 원 Stop 귀속을 막고 pending을 유지한다. zero-wire mutation과
  다른 AxisReference는 간섭하지 않는다.
- Axis Reset은 `BeginResetWaitForStableErrorClearanceAsync`를 live-command gate 안에서 실행해
  `0x2024` success ACK와 latest pending continuation을 gate 반환 전에 보존한다. 이후
  `ResumeResetWaitForStableErrorClearanceAsync`를 preemptible monitor에서 실행하며 `0x2028`의
  successful `AxisErrorId == 0`을 기본 3회 연속 확인한다. failure/preemption 뒤에는 accepted
  continuation을 status-only로 재개하고 Reset을 replay하지 않는다. unresolved accepted Reset
  중에는 Resume을 계속 허용하면서 새 live mutation은 interlock한다. typed confirmed same-axis
  interference 뒤에만 사용자의 명시적 새 Reset을 허용한다. success와 typed failure 모두
  submission/ACK/마지막 status/poll 및 expected/observed mutation generation을 표시한다.
- Axis Stop/Reset은 command-before durable journal과 ACK-before-first-status accepted observer를 사용한다.
  accepted restart는 exact endpoint/D0/axis identity와 final D0 live refresh 뒤 `0x2028` 3-sample
  status-only proof로 resolve한다. Armed/outcome-uncertain record는 자동 replay하지 않는다. active
  Reset을 Stop으로 대체할 때는 predecessor identity를 원자 보존하고 accepted Reset session에 pin된
  local transport abort 뒤 새 connection object에서 RPC/D0/axis/Motion identity를 검증한다. pre-wire
  failure 또는 valid Stop NACK는 아직 pending인 Reset만 복원하고, 완료된 Reset은 다시 활성화하지
  않는다. post-write uncertainty는 Stop recovery를 유지한다.
  Motion과 Stop journal은 Motion -> Stop 순서로 resolve하며 그 사이 process kill에도 안전하다. 이
  값은 LASAL axis error/standstill 관찰이며 DS402 Fault, drive error register 또는 물리 정지의 독립
  증거가 아니다.
- Group Stop도 확인창 없이 실행하고 group motion 감시 중 사용할 수 있다. ACK는
  정지 완료가 아니므로 `0x2045`의 stable Group Standby를 다시 확인한다. PLC의
  `StopMove(Mode:=3)`은 기존 profile buffer를 폐기하며, 정지 뒤 새 Move를 금지하는
  명령이 아니다.
- SDK `BeginGroupStopWaitForStableStandbyAsync`는 `0x2085` ACK와 exact
  connection/session/group/latest-pending continuation을 반환하고 status를 읽지 않는다.
  `ResumeGroupStopWaitForStableStandbyAsync`는 `0x2045`만 보내 Standby를 기본 3회 연속 확인한다.
  timeout/cancel/status failure/preemption에도 accepted continuation과 evidence를 보존하고 Stop을
  replay하지 않는다. stale/superseded/completed continuation과 concurrent second Resume은
  zero-wire로 거부되며 compound API는 두 phase의 elapsed total deadline을 공유한다. Stop의 actual
  write boundary에서
  pending Enable proof를 reset하고 per-group mutation generation을 고정한다. 이후 다른 group
  mutation이 actual write boundary에 도달하면 원 Stop의 stable proof를 무효화하며, final status
  publication도 원 connection session에 bind한다. WPF 일반 버튼과 qualification은 safety generation을
  gate 대기 전에 예약하고 Begin만 command gate 안에서 실행한다. accepted continuation과 recovery
  evidence를 gate 반환 전에 저장한 뒤 Resume은 preemptible status-only monitor에서 실행한다.
  성공 proof에서만 pending을 지우며 실패/preemption에서는 exact continuation을 보존한다. 따라서 새
  Group Stop/Power Off는 monitor 중 다음 generation을 예약하고 이전 Resume을 선점할 수 있다. 일반
  Group Reset, Axis Reset, Admin
  `GroupMoveLinearRelative`와 D5 `SubmitSdo`/`CancelOperation`의 지연
  ACK도 session-bound priority publication을 거쳐 drain 후 `ResultDiscarded`된다. accepted
  Submit은 exact ticket/BootId/MapRevision evidence를 보존하며 Cancel ACK는 stale success로
  적용하지 않는다. current SDK Debug/Release direct runner는 각각 1133/1133 PASS했고
  WPF Debug/Release Rebuild도 PASS했다. WPF full smoke는 339/339, 별도 actual-EXE
  relaunch gate는 Debug/Release 각각 1/1이며 PLC/runtime 안전 증거는 별도다.
- Relative move도 absolute와 같은 motion-uncertain tracking을 사용한다. valid Admin
  rejection만 local tracking을 해제하고 timeout, malformed response 또는 연결 손실은
  상태가 불확실하므로 Stop/PowerOff recovery 경로를 유지한다.
- Move Linear 응답 `ErrorId=7`은 `_LMCPROF_SWE_ERROR`다. 예제는 송신 직전의
  X/Y/Z/U `StartRaw`, `TargetRaw`와 dynamics를 로그에 남기고, runtime software
  end position 위반임을 명시한다. 어느 축이 위반했는지는 현재 wire 응답에
  `SubErrorNo`가 없으므로 LASAL의 `AxReadSWEndPos`와 `ReadProfileError()`로 확인한다.
- raw Group Reset `GroupReset[Async]`의 `0x2049` 성공은
  `LMCRobot.AxQuitError(AxisNo:=0)` dispatch acceptance일 뿐 member error-clear 완료가 아니다.
  구현된 `BeginGroupResetWaitForStableErrorClearanceAsync`는 valid `0x20D2` observed member
  snapshot 뒤 `0x2049`를 정확히 한 번 보내고 accepted continuation을 게시한다. SDK는 정상
  응답의 `1..16`개 nonzero/unique reference를 허용한다. 이는 expected topology나 현재 PLC
  build가 추적 LASAL source의 9-member 구성과 같다는 attestation이 아니다.
  `ResumeGroupResetWaitForStableErrorClearanceAsync`는 reset을 replay하지 않고 각 round에
  `0x2045` 한 번과 pinned member 전원의 `0x2028`을 snapshot 순서대로 보낸다. group
  `GroupErrorId`와 모든 member `AxisErrorId`가 0인 full-clear round를 기본 3회 연속 확인해야
  완료다. compound API만 Begin+Resume total deadline을 공유하며, 나중의 split Resume은 새
  status-only timeout epoch와 stable count로 시작한다. timeout/cancel/status failure는 pending
  continuation을 보존한다. accepted/outcome-uncertain group 또는 pinned-member mutation은 terminal
  supersede이고 structurally valid safety NACK와 pre-wire failure는 Reset continuation을 보존한다.
  Stop/PowerOff/safe Disable은 현재 full round 뒤 monitor를 선점하며 Reset proof는
  power/profile-lock/motion-ready를 승격하지 않는다.
- WPF Group Reset 버튼은 위 stable API를 사용한다. accepted Reset 즉시 cached group
  power-active, kinematic identity, Home과 profile-lock readiness를 무효화하며, proof 성공 뒤에도
  자동 복원하지 않는다. final status의 LockedStandby는 motion 준비가 아니라 safe Disable을
  열기 위한 관찰값으로만 쓴다. exact live pending 또는 submission-outcome-uncertain 중에는 새
  Reset, Power On, Enable, SetKin, Move, mutation qualification, Connect/Reconnect,
  connection/window Close를 차단한다. status-only Resume, read-only inspection, Group Stop,
  Power Off, safe Disable은 허용한다. 일반 Read Group Status는 proof를 전진시키지 않는다.
  prepared observer는 `0x20D2` 뒤 `0x2049` write 직전에 immutable operation/member evidence를
  제공한다. WPF는 이 경계에서 remote/local/callback endpoint, DiagnosticsBuild/BootId/MapRevision,
  group/ref, old session과 ordered members를 durable `ArmedBeforeDispatch` record로 먼저 저장한다.
  ACK 뒤에는 exact CAS로 `AcceptedAwaitingProof`를 기록한다. spontaneous disconnect/process restart는
  `RecoveryRequired`로 승격하고, exact reconnect와 Load Group 뒤 SDK가 fresh `0x20D2` count/order/name/
  reference/device를 1회 검증한 경우에만 status-only recovery continuation을 게시한다. recovery
  Resume은 `0x2049`를 보내지 않는다. mismatch는 record를 유지한 채 fail-closed한다.
- Stop은 position, velocity, acceleration 입력을 읽지 않고 Stop deceleration과
  Jerk만 변환한다. 다른 motion 입력의 오타가 Stop을 막지 않아야 한다.
- Axis Stop은 successful Standstill 3회, Power Off는 successful
  `PowerOn=false && Standstill=true` 3회를 확인해야 안전 확인을 통과한다. 한 번의 상태
  sample만으로 정지 또는 전원 차단 완료를 판단하지 않는다.
- motion 전에 Read Status로 PowerOn을 확인한다.
- 유한 motion은 ACK 뒤 non-standstill을 관측한 후 stable standstill 3회를 확인한다.
  대기 중에도 Stop과 Power Off는 실행할 수 있다.
- motion 송신 직전에 endpoint, target kind/name/reference, DiagnosticsBootId와
  MapRevision을 `MotionUncertaintyJournal`에 `ArmedBeforeDispatch`로 먼저 flush한다.
  journal flush가 실패하면 Move는 wire에 쓰지 않는다. valid rejection 또는
  `LMCSendPreemptedException(BeforeWire)`만 durable `Resolved`로 전환할 수 있고,
  success ACK, timeout, malformed response, cancellation 또는 연결 손실은
  `RecoveryRequired`로 유지한다. Move ACK는 실행 완료 증거가 아니다.
- 재시작 시 active record는 보수적으로 `RecoveryRequired`가 된다. 기록된 remote
  endpoint 외에는 TCP 연결 전 차단하고, reconnect 뒤 동일 BootId/MapRevision과
  동일 target name/reference를 read-only lookup으로 확인한 경우에만 해당 target의
  Read Status, Stop 또는 Power Off를 허용한다. Move는 자동 replay하지 않는다.
  stable Standstill/InPosition 또는 PowerOn=false 3회 proof가 끝나면 durable
  `Resolved`를 먼저 flush하고 그 다음 volatile interlock을 해제한다.
- Stop/Power Off 요청은 app-level send gate를 기다리기 전에 safety generation을
  선예약한다. WPF가 생성하거나 reconnect하는 모든 connection은 같은 opt-in
  `LMCSendPriorityCoordinator`를 사용하고 ordinary operation, diagnostics와 qualification
  흐름은 captured generation의 preemptible scope를 유지한다. SDK `ExchangeCore`가 각
  `stream.Write` 직전에 generation을 검사하므로 아직 송신되지 않은 motion/diagnostics와
  compound helper의 후속 RPC는 `LMCSendPreemptedException`으로 zero-wire 거부된다.
  이미 최종 검사를 통과해 in-flight가 된 RPC는 transport를 취소하지 않고 결과/timeout을
  확정하며, 그 뒤 Stop/Power Off send가 같은 직렬 경로를 얻는다. 앞선 RPC가 transport를
  fault로 전환하면 safety send의 성공도 보장하지 않는다. coordinator를 주입하지 않았거나
  scope 밖에서 직접 호출한 SDK 사용자는 이 우선순위 계약의 적용 대상이 아니다.
- safety ACK가 수락되면 app send gate를 반환하기 전에 monitor admission을 예약하고, 호출자가
  받은 exact safety generation으로 Standstill/InPosition 또는 PowerOn=false를 확인한다. 이
  reservation은 ACK와 monitor 시작 사이의 ordinary command 진입을 막는다. 더 새 Stop/Power Off가
  예약되면 이전 generation monitor의 다음 RPC도 write 직전에 거부되며 이전 monitor 실패 로그는
  안전 상태를 추정하지 않는다.
- motion 가능성이 남아 있는 동안 UNIT, 위치, 속도, 가속도와 방향은 잠그고
  Stop deceleration과 Jerk만 수정할 수 있게 한다.
- 일반 motion Cancel 버튼은 제공하지 않는다. Qualification의 `Cancel Test`는 다음
  RPC 전 취소와 scenario cleanup을 요청할 뿐이며, in-flight RPC를 끊거나 Stop을
  대신하지 않는다.
- motion 가능성이 남아 있으면 일반 Close Connection과 창 닫기를 차단한다.
  자동 Stop은 보내지 않는다. 프로세스 강제 종료나 전원 손실은 막을 수 없으므로
  다음 시작에서 durable record의 exact-identity recovery만 허용한다.
- Move는 target lookup 때 고정한 BootId/MapRevision을 wire 직전 fresh capability와
  대조한 뒤 durable journal을 arm한다. startup/invalid-response 복구는 status-only
  evidence로 끝낼 수 없고, fresh exact-identity Stop 또는 Power Off ACK와 연속 3회
  safe-state proof가 필요하다. safe-state proof 뒤에도 capability identity를 다시 읽으며,
  drift/RPC 실패 시 journal과 volatile latch를 유지하고 explicit safety ACK를 다시 요구한다.

## 6. Qualification runner

### 6.1 공통 실행 계약

- runner는 한 번에 하나만 실행한다. ordinary operation, safety verification 또는 다른
  qualification이 실행 중이면 시작하지 않으며, 기존 `motionMayBeActive`가 남아 있어도
  차단한다.
- 공통 상태는 `BEGIN -> PASS/FAIL/SKIP/ABORTED`이며 progress와 최근 구조화 로그를
  Group/Bulk/Recorder 세 탭에 같이 표시한다. 로그 한 줄은 UTC, elapsed ms, run GUID,
  scenario, step과 assertion/cleanup field를 포함하고 사용자가 파일로 저장할 수 있다.
- `NotSupportedException`으로 판정한 capability 부재는 `SKIP`, 고정 Catalog/identity/
  limit 계약 불일치는 `FAIL`이다. Recorder의 명시적 capability/bank limit precheck는
  `SKIP`으로 분류한다. UI에 버튼이 있다는 사실만으로 PLC가 해당 기능을 지원한다고
  간주하지 않는다.
- 각 qualification wire dispatch는 공용 send gate를 얻은 뒤 runner 시작 시점의
  safety generation과 scenario token을 다시 확인한다. 송신을 시작한 단일 RPC에는
  `CancellationToken.None`을 넘겨 응답/timeout을 확정한다. Recorder download는 SDK의
  compound helper를 한 번에 호출하지 않고 header/chunk별 gate를 다시 얻어 chunk 사이
  취소와 safety 선점을 허용한다. Bulk Catalog public helper는 cancellation이 connection을
  강제 종료할 수 있으므로 `CancellationToken.None`으로 하나의 bounded compound
  operation을 완료한다. cleanup은 cancellation과 독립적이며 진행 중인 safety
  send/monitor를 먼저 통과시킨 뒤 같은 gate로 직렬화한다.
- qualification의 ordinary RPC 또는 compound follow-up이 SDK write 직전 generation 검사에서
  선점되면 `LMCSendPreemptedException`을 `FAIL`이 아닌 `ABORTED`로 기록한다. SDO submit은 이
  경우 `LMCSdoSubmissionFailureContext`의 `Phase=Submission`,
  `SubmissionOutcome=NotAttempted`, ticket 없음 상태이므로 미확정 전송으로 quarantine하지
  않는다.
- 실행 중 기존 Axis/Group Stop 또는 Power Off는 계속 사용할 수 있다. 이 safety
  요청은 qualification token을 취소하며, Group motion scenario는 외부 Group Stop의
  stable InPosition 또는 Group Power Off의 PowerOn=False 3회를 확인한다. 확인에
  실패하면 자체 Group Stop cleanup으로 fallback한다.

위 priority/zero-wire/상태 분류는 source와 deterministic fake-TCP PC test로 검증한
application/SDK 계약이다. 실제 PLC scheduler, EtherCAT 반응, 물리 정지 시간 또는 장비 안전
인증을 대신하지 않는다.

### 6.2 Single Axis live qualification

- UI는 실제 명령임을 명시하고 E-stop/STO 및 travel, exact axis/raw unit/target, exclusive owner와
  evidence capture의 세 확인을 모두 요구한다. 확인 전 또는 잘못된 입력은 RPC 0건이다. raw
  relative delta는 nonzero/절대값 1,000,000 이하, velocity/acceleration/deceleration은 양수,
  첫 slice Jerk는 0이고 final tolerance는 양수다. 입력, axis name/load/session 변경과 run 종료는
  확인을 전부 무효화한다.
- preflight는 connection instance와 generation, endpoint, loaded Axis instance/name/reference,
  capability owner/session과 nonzero `DiagnosticsBuild`/`BootId`/`MapRevision`, lookup 시점의
  BootId/MapRevision을 고정한다. Power/Stop/PowerOff 직전 capability를 다시 읽으며 Move는 공용
  durable motion dispatch가 wire 직전 fresh identity를 다시 확인한다. drift 시 다른 PLC에
  cleanup mutation을 보내지 않고 아직 active인 durable record를 유지한다.
- runner는 Power On 전 `AxisQualificationRecoveryJournal`을 arm하고 `PowerOnAccepted`,
  `PowerOnStable`, `MovePrepared`, `MoveAccepted`, `MoveStable`, `StopAccepted`, `StopStable`,
  `PowerOffAccepted`, `SafeResolved`를 단조 저장한다. current running qualification이 arm에서
  받은 process-local record GUID와 동일한 owner session을 함께 만족할 때만 이 부모 record 아래의
  내부 mutation admission을 통과한다. restart record는
  자동 replay가 없고 exact identity의 read-only/status-only 확인과 명시적 safety recovery만 허용한다.
  `ArmedBeforePowerOn`/`MovePrepared` crash는 may-have-been-sent 상태로 보수 승격한다.
  단, startup의 immutable retirement ledger exact decision은 이 승격보다 먼저 적용해 commit과
  journal resolve 사이 process loss가 원본-byte CAS를 깨지 않게 한다.
- happy path는 `0x2023(true)` exactly once와 PowerOn stable 3회, fresh successful/error-free
  PowerOn+Referenced+Standstill, `0x202E` start, checked `start+delta`, `0x20A0` exactly once,
  non-Standstill 1회 이상과 뒤따르는 Standstill 3회, target tolerance 및 sample range 안의
  `0x202E` final 3회, `0x2022` exactly once와 Standstill 3회, `0x2023(false)` exactly once와
  PowerOff+Standstill 3회다. 계획 이동 완료 뒤 Stop이므로 in-motion halt proof는 별도다.
- accepted Power/Move/Stop은 기존 durable journal을 그대로 사용한다. Move 뒤 cancel/failure는
  Move replay 없이 non-cancelable Stop/PowerOff cleanup을 수행한다. Stop proof 실패 후에도 exact
  PowerOff를 시도하며 PowerOff stable proof가 없으면 safe-state-unproven FAIL이다. PowerOff는
  증명됐지만 motion/Stop journal 정리가 실패한 경우 safe state와 journal failure를 구분해 FAIL한다.
  외부 Axis Stop이 개입하면 runner Stop은 생략하고 status-only Standstill proof 뒤 PowerOff만 한
  번 보내며, 외부 Axis PowerOff이면 status-only PowerOff+Standstill proof 뒤 runner Stop/PowerOff를
  모두 생략한다. fixed safety generation과 pre-wire check로 그 사이의 새 safety 예약도 재검증한다.
  QTEST는 실제 command/read count, start/target/final, motion/stable samples, replay zero와 최종
  safe state를 남기며 PASS 뒤 capture를 최소 2초 더 유지하도록 표시한다.
- 부모 record는 stable Power Off proof에서 `SafeResolved`를 먼저 기록한 뒤 child Power/Stop/Motion
  record를 해제한다. process-kill 회귀는 `PowerOnStable`에서 재시작한 session이 관찰 barrier까지
  mutation 0건이고, 운영자의 명시 Power Off 뒤에만 `0x2023(false)` 1회와 status 3회를 실행함을
  검증한다.

### 6.3 Group qualification

- Enable scenario는 PowerOn + Disabled/Unlocked 3회 안정 preflight,
  `0x2047 ErrorId=0`, 이어지는 `0x2045` PowerOn + Locked Standby 3회를 요구한다.
  SDK는 mutation/status gate, `0x2047`, 모든 `0x2045`와 delay를 한 total deadline으로 제한한다.
  pre-write cancel/deadline은 zero-wire `NotAttempted`/reusable/no-mutation이고, write 뒤 caller
  cancel은 response와 accepted evidence를 게시한 뒤 typed cancellation이다. ACK 무응답은
  `OutcomeUncertain`/no-continuation/`Faulted`, status 무응답은
  `Accepted`/exact-continuation/`Faulted`이며 둘 다 `TransportInvalidatedAtDeadline=true`다. 성공
  상태는 profile locked이며 자동 Unlock/PowerOff하지 않는다. 취소도 lock
  transition을 되돌리는 명령이 아니므로 이후 Read Status와 명시적 Disable이 필요하다.
- Buffered scenario는 Admin capability와 fixed group reference, 4-member mapping,
  선택 축 software min/max, group velocity/acceleration limit, initial stable InPosition을
  확인한다. 첫 live slice는 동일 부호의 A/B, 각 delta 절대값 최대 1,000,000 raw,
  `Jerk=0`, `Coordinate=None`, `ExactStop`, `BufferMode=Buffered`로 제한한다.
  A의 non-InPosition을 관측한 뒤 B를 보내고 B ACK 직후에도 non-InPosition인지 확인한
  다음 `start + A + B` endpoint/tolerance와 stable InPosition을 검사한다. 성공 시
  Aborting relative command로 captured start에 복귀한다. motion 중 오류/취소는
  Group Stop + stable Standby cleanup 대상이며 cleanup failure는 원 오류와 합쳐
  안전 상태 미확정 FAIL로 보고한다.
- Stop-first scenario는 실제 이동을 만들지 않는다. shared send gate를 보유한 채
  zero-delta Move task를 먼저 대기시키고 Stop task가 safety generation을 선예약한 뒤 gate를 연다.
  Begin만 priority scope/gate lease에서 `0x2085` ACK와 continuation을 보존한다. gate 반환 뒤 Resume은
  preemptible scope에서 stable Standby 3회를 status-only로 확인한다. Move delegate invocation 0과
  pre-transmission cancellation을 요구한다. 새 외부 Group Stop/Power Off는 proof 중에도 다음
  generation을 예약해 이전 Resume을 폐기할 수 있다. accepted Resume 실패의 cleanup은 exact pending
  continuation만 재사용하며 fresh `0x2085`를 보내지 않는다. accepted evidence가 없을 때만 새 Stop을
  허용하고 cleanup failure는 primary와 aggregate한다. deterministic fake-RPC는 외부 Power Off
  선점에서 `0x2085=1`, `0x204B=1`, `0x2045=4`, accepted status failure cleanup에서
  `0x2085=1`, `0x2045=4`를 증명한다. 실제 PLC packet
  순서와 정지 시간은 pcap/runtime으로 별도 확인한다.

### 6.4 Bulk qualification

- 시작 시 capability와 Catalog를 다시 읽고 stable nonzero DiagnosticsBootId,
  동일 MapRevision, CatalogEntryCount 24, BulkReadable 24개, MaxBulkSignals >= 24와
  전 entry InputMapped phase를 요구한다. manual Bulk/Recorder resource가 남아 있으면
  시작하지 않는다.
- Snapshot Soak는 revision-bound public builder에 24개를 Catalog 순서 그대로 넣고
  Active까지 최대 5초 bounded poll한다. 기본 100회, 10 ms 간격으로 읽으며 각 응답의
  Boot/config/map identity, entry count/stride/order/type, SameCycle + InputMapped flags,
  even SnapshotSequence, Partial=false, 모든 entry Valid/detail 0을 검사한다. cycle,
  timestamp와 sequence는 unsigned wrap-aware nondecreasing으로 검사하고 RPC latency
  min/avg/max와 cycle delta를 기록한다.
- Lifecycle Soak는 지정 횟수마다 새 builder로
  `Configure -> Active -> Snapshot -> Release`를 수행한다. 끝난 뒤 새 Configure/Active/
  Release가 다시 성공해야 하며, released reader의 두 번째 Release는 local
  `InvalidOperationException`으로 막혀 wire request가 없어야 한다.
- 모든 생성 reader는 `finally` Release 대상이다. cleanup 실패는 PASS로 숨기지 않는다.
- one-slave offline partial은 Group PowerOff/Disabled와 4축 actual-position 3회 동일값을
  checkpoint 직전에 확인한 뒤 프로그램이 fault를 만들지 않는 두 operator checkpoint로
  구현한다. baseline 24 Valid, 한 SourceIndex의 6개 `SlaveOffline` bit/Detail 18과
  나머지 18 exact Valid, 같은 slave 복구 뒤 24 Valid를 요구한다. offline 축의 status는
  PLC가 OR하는 추가 invalid bit를 허용하되 Valid bit는 금지하며, 첫 Partial에서 다른 축도
  invalid이면 즉시 실패한다.
  reconnect stale handle과 raw old revision/BootId rejection은 별도 내부 시험이다.

### 6.5 Recorder qualification

- fresh capability와 이미 load한 동일 MapRevision Catalog를 대조하고 Catalog 순서의
  첫 4개 Recordable signal을 고정 channel order로 사용한다. Single은
  RecorderSingleBank, Ring/soak는 RecorderTrigger도 요구한다. Double qualification adapter는
  RecorderSingleBank + RecorderDoubleBank, exactly two buffers, 네 개 이상의 Recordable signal과
  기존 capacity 조건을 요구하지만 닫힌 proof gate를 첫 guard로 재검사한다.
- Single Manual은 SamplePeriod 1 cycle, capacity 1000이다. 자연
  `SampleCountComplete`와 1000 samples, zero dropped/overflow를 기다린 뒤 Header,
  Download A/B의 identity/revision/channel order, 16-byte stride, 16,000-byte data와
  raw SHA-256 일치를 검사한다. cleanup 후 buffer/configuration 각각의 두 번째 Release가
  local guard에서 막히는지도 검사한다.
- Ring forced trigger는 capacity 1000, pre 100/post 899, Edge와 자동 trigger가
  도달하지 않는 threshold를 사용한다. Recording에서 pre-history 100개 이상을 확인한
  뒤 `TriggerRecorderAsync`를 보내 `TriggerComplete`, TriggerIndex 100, 1000 samples,
  Header/data identity와 qualification per-RPC gated exact chunk coverage를 검사한다.
- Trigger Lifecycle Soak는 capacity 32, pre 16/post 15로 같은 forced-trigger lifecycle을
  기본 100회(입력 상한 1000회) 반복한다. 매 회 buffer를 먼저 terminal/frozen 상태로
  만든 뒤 buffer, configuration 순서로 Release하며 completed count, ResourceBusy,
  dropped/overflow를 집계한다. WPF가 기록하는 `rtEvidence=NOT_MEASURED_BY_WPF`처럼 이
  결과만으로 PLC RT jitter나 sample 무손실을 독립 증명하지 않는다.
- Reconnect qualification은 Start ACK 직후 BootId/RecordId/BufferId, old OwnerSessionEpoch,
  config/map revision과 signal order를 snapshot으로 보존하고, Ring Recorder가
  Recording/pre-history 상태임을 확인한 뒤 앱 RPC connection을 실제 close/reopen한다. fresh capability의
  same BootId/MapRevision을 확인한 뒤 exact와 single-bank 0/0 discovery를 별도 run으로
  실행한다. 반환 identity가 snapshot과 일치하고 OwnerSessionEpoch가 새 값인지 확인한 뒤
  Status -> 필요 시 Stop -> Header -> exact-coverage Download 순서로 검증한다. discovery가
  다른 active resource를 반환하면 Adopt 응답 identity 검증에서 즉시 실패하며 해당
  resource에 Status/Stop/Release를 보내지 않는다.
- Double qualification은 새 recovery Guid의 첫 4 bytes를 little-endian `uint`로 변환하고
  zero이면 1로 보정한 결정적 `RequestedConfigId`를 사용한다. Configure 전 durable journal을
  arm하고 recoverable `0x7E4C` Configure 뒤 A/B를 freeze/download하며 third Start의 exact
  ResourceBusy와 A semantic header/data 불변성을 확인한다. core non-durable orchestrator의 release
  primitive는 exact unexpected-third handle이 반환된 경우 명시적 unexpected third -> B -> A ->
  configuration 해제를 지원한다. 이는 WPF durable cleanup 계약이 아니다.
- WPF는 성공/실패/cancel scope를 same-session retained state로 남기고 자동 Release하지 않는다.
  same-session cleanup은 `ThirdStartExactBusyConfirmed=true`이고 unexpected-third handle이 없을 때만
  허용한다. exact journal/session/identity/order와 checkbox preflight가 끝난 뒤 확인을 소비하고
  Status -> 필요 시 Stop -> Ready/Uploading 확인 뒤 B -> A -> configuration 순서로 durable
  intent/confirmed cleanup을 수행한다. 이후 실패하면 다음 시도 전에 checkbox를 다시 확인한다.
  unexpected third success 또는 ambiguous outcome이면 모든 same-session Release는 zero-wire다.
  disconnect/reconnect 뒤 token-qualified exact inventory inspection만 허용하며 durable two-bank
  evidence와 충돌하면 external/manual recovery 대상으로 남기고 자동 Release하지 않는다.
- confirmed-not-applied bank/configuration intent가 pending이면 동일 target의 exact intent만
  재사용한다. 새 intent 또는 다른 target Release는 금지한다. retained handle이 이미 ACK-success면
  Release wire를 replay하지 않고 해당 bank를 durable confirm하거나 configuration journal을
  resolve한다.
- Double reconnect/startup recovery는 ordinary `NewLiveOrMutation` admission과 별도인 exact
  lifecycle admission을 사용한다. recovery용 capability 계약도 global mutation interlock과
  Recordable Catalog 조건에서 분리해 active journal 자체가 recovery를 deadlock하지 않게 한다.
  ConfigRevision=0이면 `0x7E4D -> 0x7E4A`, 그 외에는 `0x7E4A`부터 시작하며 occupied bank는
  exact `0x7E49`, empty configuration은 `0x7E4B`로 채택한다. 일부 bank Adopt 뒤 실패한 handle은
  즉시 retained state에 보존해 다음 명시적 실행에서 나머지를 이어간다. adopted bank는 Status로
  config identity를 복구하고 Armed/Recording이면 Stop -> Ready/Uploading을 확인한 뒤 B -> A ->
  동일한 Buffer 0 identity의 configuration 순서로 Release한다.
- reconnect 확인은 그 시점의 journal identity, BootId/MapRevision, ConfigId/ConfigRevision과 exact
  bank 집합을 immutable snapshot으로 만든다. 4D/4A read-only discovery와 journal merge 뒤 실제
  plan이 snapshot에 없던 revision/bank를 포함하면 required pre-mutation callback이 4B/49/Release
  전에 중단한다. UI는 갱신된 plan을 표시하고 checkbox를 다시 요구한다. partial adoption과 retained
  result도 다음 mutation/Release 전에 같은 policy를 통과해야 한다.
- Single/Ring cancellation/실패 cleanup은 active recorder에 Stop을 시도하고 final Status를 확인한다.
  buffer와 configuration handle의 자동 Release는 `Ready` 또는 이미 frozen download가
  시작된 `Uploading`에서만 수행한다. `Fault`는 releasable frozen state로 취급하지 않고
  identity/resource를 보존하며 recovery-required QTEST failure를 남긴다. reconnect
  close 전 transport fault를 포함한 cancellation/실패 cleanup은 실제 original connection
  상태와 보존 expectation을 기준으로 route를 선택하고, 필요하면 exact identity로
  connection/adoption을 복구한 뒤 adopted identity로 buffer와 configuration을 해제한다. 이후 명시적
  Status/error 진단과 수동 복구가 필요하다. 보존 ownership은 manual UI에서 quarantine하고
  Status 확인 전 mutation을 막는다. 확인 상태가 Armed/Recording이면 명시적 Release가
  Stop -> Ready/Uploading poll -> buffer/configuration Release를 수행하며 Fault/Empty는
  계속 보존한다. buffer/configuration 중 configuration만 남은 tail은 Status 없이 retry한다.
  external fault와 BufferOverwritten는 자동 Release 범위 밖이다. Double adapter도 Fault,
  identity mismatch와 non-Busy third Start를 zero-Release로 보존하며 journal/retained state를
  자동 재생하지 않는다.

### 6.6 D5 SDO abort -> recovery qualification

- runner는 SDO Read만 생성하며 write와 EtherCAT fault injection은 하지 않는다.
- 선택 slave와 `_LMCAxis1..4`를 일치시키고 `PowerOn=False`, `Standstill=True`, actual
  position 3회 동일값을 확인한 뒤 D5 ticket을 제출한다.
- 먼저 `0x6061:0 Int8/1` baseline을 읽는다. 사용자가 제조사 기준으로 선택한 존재하지 않는
  read-only object/subindex가 실제 abort를 반환한 뒤 같은 BootId/MapRevision의 새
  `0x6061:0` ticket이 baseline과 같은 값을 반환해야 한다.
- abort PASS 계약은 status RPC 성공, terminal `Failed/Failed`,
  `OperationErrorId=-32000`, `OperationDetail`의 nonzero raw EtherCAT SDO abort code와
  result 없음이다. local transport error, timeout과 cancel은 abort 증거가 아니다.
- pending cleanup은 UI 독립 orchestrator가 active/current `LMCConnection`, ticket owner와
  저장 MapRevision을 dispatch 전에 fail-closed한다. current capability BootId mismatch를
  MapRevision보다 우선 quarantine하고 두 mismatch 모두 status/cancel을 보내지 않는다.
  cached terminal은 재조회하지 않고 cached pending은 먼저 refresh한다. 실제 `Queued`에서만
  public Cancel을 보내며 `InvalidState` race면 terminal까지 기다리고 `Running`이면 cancel하지
  않는다. cancel accepted 뒤에는 exact `Cancelled/Cancelled`만 허용하고 마지막 fresh status를
  active state에 보존한다. PLC Stop이나 transport close 없이 원래 terminal deadline의 남은
  시간+1초를 반영하며 cleanup wait는 최소 15초, 최대 120초이고 `<=` 경계에서도 한 번 더
  poll한다. 끝나지 않으면 ticket identity를 지우지 않은 채 cleanup timeout으로 실패한다.
- Submit wire 호출 전에 ticket ID 0의 outcome evidence를 먼저 quarantine ledger에 넣는다.
  명시적 PLC command rejection이면 제거하고, 응답 유실/transport 예외면 unknown-ticket
  evidence로 보존한다. ticket 응답을 받았으면 active ticket/owner connection/deadline을
  먼저 저장한 뒤 outcome evidence 제거 성공을 확인한다.
- 모든 pending-ticket cleanup은 status/cancel 전에 같은 `LMCConnection`의 current capability
  BootId/MapRevision을 old ticket의 `DiagnosticsBootId`/`SubmissionMapRevision`과 먼저
  비교한다. 둘 중 하나가 변경됐거나 status가 exact `BootIdMismatch`면 old terminal을 추정하지 않고
  known ticket을 stale-session quarantine으로 이동한다. local ticket의 session generation이
  stale인 예외도 quarantine한다. 같은 Boot/session의 exact `TicketNotFound`는 terminal-slot
  교체 계약상 이전 ticket terminal만 증명하므로 `TERMINAL_INFERRED`, outcome `UNKNOWN`으로
  해당 ticket을 해제한다.
- Write terminal이 exact `Completed/Success`이면 성공 전송 자체로 mutation을 다시 열지 않는다.
  immutable request fingerprint를 pending readback으로 보존하고 동일
  Slave/Index/SubIndex/Type/Length의 SDO Read만 허용한다. 그 Read가 exact terminal success이며
  result type/length/4-byte 값까지 Write 값과 일치할 때만 mutation/Close interlock을 해제한다.
  mismatch/failure는 pending을 유지하고 Stop, PowerOff, 기존 resource cleanup만 예외로 둔다.
  일반 SDO Read/Write submit은 버튼 클릭 시 immutable `LMCSdoRequest`를 먼저 만들므로 in-flight
  RPC 중에도 다음 요청의 editor 값을 바꿀 수 있다. operation submit 자체는 기존 single-slot으로
  직렬화한다. 성공 Write 뒤 exact readback interlock 동안에도 draft editor는 계속 편집할 수 있다.
  원 target identity는 별도 immutable verification context에 보존하고, `Load Required Exact Readback`
  동작이 그 요청을 editor에 명시적으로 불러온다. 불러오기 전 draft는 process-local
  connection/session-bound snapshot으로 보존하고, VERIFIED 뒤 editor가 불러온 exact tuple 그대로일
  때만 복원한다. 불러온 뒤의 사용자 편집 또는 reconnect/session drift는 snapshot보다 우선하며
  자동으로 덮어쓰지 않는다. 실제 readback 송신은 admission에서 원 owner/session/BootId/MapRevision
  및 exact request가 모두 일치할 때만 허용한다.
  SDO/output Write는 dispatch 전 crash-safe journal을 arm하고 accepted/terminal/readback 상태를
  원자적으로 저장한다. journal format v2는 SDO의 Slave/Object/SubIndex/Type/Length/Timeout과
  expected bytes를 checksum 범위에 typed metadata로 보존한다. 시작 시 미해결 record는
  command/ticket 또는 Write를 재생하지 않는다. legacy v1 SDO record와 승인되지 않은 target은
  protocol recovery를 wire 전에 거부한다. terminal-success v2 record가 current SDK allowlist의
  exact target과 일치할 때만 운영자가 1회 read-only recovery를 실행할 수 있다. Read 전후의
  fresh BootId/MapRevision과 exact read bytes가 일치하고 같은 record/state의 atomic CAS가
  성공하면 Resolved tombstone을 먼저 영속화한 뒤 성공을 반환한다. 그 외 record는 target의 물리
  확인과 명시적 recovery ACK로만 해소한다. checksum corruption과
  current-process digital output outcome이 불명확하면 별도 물리 출력/PLC output-shadow 확인
  checkbox와 경고 확인창을 모두 통과해야 ACK할 수 있다. 새 write, shadow read, tuple/selection
  변경과 uncertainty 상태 전환은 이 checkbox를 초기화하며, ACK는 GUI interlock만 해제하고
  이전 write의 성공 증거로 기록하지 않는다.
  두 번째 writer, open/runtime persistence fault는 새 live/mutation command를 fail-closed한다.
  tracked D5 read도 차단하고 일반 non-D5 read-only inspection, Stop/PowerOff/Group Stop과
  정상 종료는 active durable evidence가 없을 때만 허용하고, active evidence가 남아 있으면
  connection/window Close도 해소 전까지 차단한다.
- `DiagnosticsOperationAdmissionPolicy`는 mutable WPF 상태를 immutable snapshot으로 받아
  작업 종류별 allow/deny reason을 순수 판정한다. 일반 live/mutation과 tracked D5 submit,
  Connect/Reconnect, connection/window Close, 정상 qualification의 UI와 handler가 같은
  decision을 사용한다. 필수 exact SDO Write readback은 pending, operation slot, D5/DO conflict,
  owner/current session을 별도로 검사하는 좁은 recovery 예외다. fresh BootId/MapRevision과 결과
  값 검증, restart recovery의 post-read identity와 atomic CAS는 core가 맡으며 durable
  resolve-before-volatile-clear 순서는
  바뀌지 않는다. safety, non-D5 read-only와 기존 resource cleanup은 mutation interlock 예외다.
- Connect 뒤 active Axis Power, Axis Stop/Reset, Motion, Group Profile Lock, Group Power 또는
  Group Reset record의 recovery identity가 current PLC와 다르면 transport failure로 취급하지 않고
  해당 `LMCConnection`에 귀속된 recovery-identity read-only quarantine으로 전환한다. 허용 범위는
  Axis/Group의 ordinary non-D5 info/status/position/member 조회, 로컬 engineering/identity/SDO
  draft 편집과 Close/Exit다. 조회는 transient lookup handle만 사용하고 application control handle을
  보존하지 않는다. status inspection은 RPC/function read 성공과 native Axis/Group error 상태를
  분리하여 오류 ID도 진단 화면에 표시한다. 모든 control(Stop/PowerOff 포함), tracked D5 read/submit, exact write readback,
  cleanup, mutation, qualification은 중앙 admission과
  safety-send boundary에서 이중 차단한다. connection loss handler는 이 quarantine connection의
  close를 새 recovery event로 승격하지 않으며 기존 journal을 replay/resolve/replace하지 않는다.
  endpoint mismatch, capability read 실패, zero identity, journal 불능은 이 완화 대상이 아니고
  기존처럼 fail-closed 및 connection close를 유지한다.
- recovery-identity quarantine에서 현재 endpoint의 active Axis Power, Axis Stop/Reset, Motion,
  Group Profile Lock, Group Power, Group Reset record 중 current identity와 다른 stale subset에만
  운영자 retirement를 허용한다. 물리 상태 확인 checkbox와 경고 확인창 전후로
  Capabilities를 두 번 읽고 connection/session/endpoint/identity, 증가한 observation sequence와
  full active exact source-byte evidence vector를 다시 비교한다. current identity record는
  `KEEP EXACT CURRENT`, 다른 endpoint record는 `KEEP OTHER ENDPOINT`로 남겨 후속
  exact recovery/manual 대상으로 보존한다. journal/ledger fault 또는 occupied operation
  slot은 commit 전에 fail-closed한다.
- 기존 다섯 journal의 identity/retirement는 BootId/MapRevision 기준을 유지한다. nonzero
  `DiagnosticsBuild`를 저장하는 Group Reset record는 Build/BootId/MapRevision 세 필드를 모두
  비교하며 Build-only mismatch도 stale retirement 대상이지만 exact current Build는 retire할 수 없다.
- `RecoveryRecordRetirementLedger`는 각 원 journal 전체 바이트, source SHA-256, 운영자와 current
  PLC identity를 `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\RecoveryRecordRetirementLedger\v1`
  아래 immutable entry로 보존한다. entry publish는 flushed temp의 same-directory
  `MoveFileExW(MOVEFILE_WRITE_THROUGH)` no-replace와 final exact byte/hash 검증을 사용한다. format 2
  entry는 Build-bearing source/current identity를 보존하고 format 1 entry도 read 호환한다. 모든
  ledger entry가 commit된 뒤에만 각 journal의 full-byte CAS `Resolved`를 수행하며 crash 뒤에는
  committed exact-source decision만 startup에서 idempotent하게 마무리한다. old command outcome은
  항상 `UNKNOWN`이고 이 경로의 wire는 두 read-only capability query뿐이다. Motion, Power, SDO,
  Write, replay, cleanup은 보내지 않는다. Diagnostics mutation과 Recorder double record는 폐기하지
  않는다. 성공 시 connection close, restart-required, app exit 순서로 같은-process 제어 재입장을
  차단한다.
- quarantine은 known ticket과 submit-outcome unknown evidence를 여러 개 보존할 수 있다.
  모두 같은 slave여야 자동 recovery proof가 가능하다. stable BootId/MapRevision 아래 서로
  다른 두 ticket을 사용하되 GeneralInline capability면 `0x6061:0 Int8/1`, legacy
  SDORead-only면 `0x1000:0 UInt32/4`를 선택한다. 두 결과의 exact type/length/bytes가 같고 proof
  동안 evidence 목록이 불변일 때만 quarantine 전체를 해제한다. UI 독립
  `D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합만으로 scope를 순수
  판정하며 MainWindow는 proof 시작 로그와 PASS 로그에 같은 decision을 사용한다.
  owner+BootId+MapRevision이
  동질인 경우에만 scope는 `same_owner_connection_recovery`,
  `new_diagnostics_identity_session`, `new_connection_session` 중 하나다. current owner와
  current identity를 모두 공유하면 첫 scope, current owner와 한 previous identity를 공유하면
  둘째 scope, 모두 current owner와 다르면서 한 previous owner+identity를 공유하면 셋째
  scope다. owner 또는 submission identity가 섞이면 `mixed_evidence_sessions`이며 same/new
  session 증거로 세지 않는다. mixed도 two-ticket application recovery proof와 성공 시
  quarantine clear는 허용한다. 첫 scope와 둘째 scope는 orphan PASS가 아니다. 한 previous
  owner+identity로 동질인 셋째 scope만 decision의 `NewConnectionRecovery=true`이고
  `newConnectionRecovery=true`로 기록한다. WPF는 항상
  `orphanQualified=false`를 기록한다. 이 scope는 새 RPC connection에서 application recovery가
  성립했다는 뜻일 뿐 PLC 내부 orphan cleanup이나 late callback을 증명하지 않는다. 실제
  orphan PASS에는 known Running old ticket, 실제 owner loss와 별도 PLC hook/capture가 필요하다.
  QTEST는 `evidenceBootIds`/`evidenceMapRevisions`,
  `recoveryBootId`/`recoveryMapRevision`, `proofScope`, `mapChangedEvidence`,
  `sameIdentityEvidence`, `mixedEvidenceSessions`, `newConnectionRecovery`,
  `orphanQualified=false`를 따로 기록한다.
- unresolved가 하나라도 있으면 Configure Bulk, Recorder Configure/Adopt/Start/Trigger,
  Group Disable, motion/PowerOn/Reset, manual SDO/PI, Close와 모든 다른 qualification 같은
  새 mutation을 차단한다. 기존 resource cleanup인 Bulk Release, Recorder Stop/Release와
  queued diagnostic Cancel, motion Stop/PowerOff 및 read-only는 허용한다. reconnect는 외부
  connection loss 뒤에만 허용한다. Resolve는 reconnect 없이 same-session/new-Boot proof에도
  사용하고, 외부 loss 후에는 새 connection proof에 사용한다.
- `D5SdoPendingCleanup` Resolve는 기존 `qualificationLogLines`를 clear하지 않고 append하며
  `D5_LOG_CONTINUATION`을 기록한다. 원래 `FAIL`/`OUTCOME_UNCERTAIN`과 resolution proof를
  같은 저장 QTEST log에 보존한다.
- manual SDO와 Drive read의 external tracker event는 마지막 qualification context에 붙이지
  않고 별도 `D5ExternalTracking:<stage>` run ID/step/elapsed context를 사용한다. unresolved
  상태에서는 이 원본 context를 유지하고 Resolve가 끝난 뒤에만 close한다.
- Phase 1 drive-read facade는 원래 exception type/stack을 그대로 다시 던지고
  `LMCDriveReadFailureContext.TryGet`으로 typed all-failure context를 제공한다. phase는
  `FacadePreflight`, `AxisStatusRead`, `CapabilityPreflight`, `Submission`, `StatusPolling`,
  `ResultMaterialization`의 6개이고, 각 SDO attempt의 `GenericSubmissionOutcome`은 공용
  `LMCSdoSubmissionOutcome`의 `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`이다.
  기존 `SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`은 호환용으로 같은 값을 유지한다.
  snapshot은 실제 capability의
  `DiagnosticsBootId`/`MapRevision`, ticket, 마지막 status를 불변 snapshot으로 보존한다.
  이전 attempt가 terminal이 아니면 다음 attempt를 만들 수 없다. WPF orchestrator는
  no-submit/rejected/accepted-terminal context의 guard를 해제하고, uncertain은 실제 Submit
  identity로 unknown evidence를 보정해 quarantine하며, accepted nonterminal은 exact ticket을
  보존하고 guard를 해제한다. context 누락, 둘 이상의 nonterminal ticket 또는 불일치 상태는
  fail-closed한다.
- 수동 `Submit SDO Read/Write`가 직접 호출하는 `LMCDiagnostics.SubmitSdo[Async]`는 원래 exception에
  `LMCSdoSubmissionFailureContext`를 연결하며 `TryGet`으로 조회한다. phase는
  `RequestValidation`, `SessionPreflight`, `CapabilityPreflight`, `Submission`,
  `PostSubmissionValidation`의 5개이고 같은 `LMCSdoSubmissionOutcome`을 사용한다. dispatch된
  attempt는 실제 capability `DiagnosticsBootId`/`MapRevision`을 보존하고 accepted failure는
  같은 `DiagnosticsBootId`/`SubmissionMapRevision`을 가진 exact ticket을 보존한다. manual router는 no-submit/rejected를 disarm하고 uncertain identity를
  reconcile해 quarantine한다. accepted ticket은 이전 manual status/result/cancel flag를
  초기화하고 manual operation state와 D5 tracker 양쪽에 보존한 뒤 disarm하며, context
  누락/불일치는 fail-closed한다. quarantine evidence에는 Read/Write operation kind를 함께
  보존한다. Read recovery proof는 Write uncertainty를 해제할 수 없으며, Write 결과가
  불명확하면 자동 복구 없이 quarantine을 유지한다.
- 성공 Write의 exact manual readback은 원 Write ticket/owner를 불변 보존하고 guarded
  `SubmitSdo[Async]` overload로만 제출한다. 이 계약은 public
  `LMCSdoWriteVerificationContext`로 SDK에 고정됐고 WPF도 같은 context를 사용한다. context factory는
  accepted ticket의 immutable submitted request와 supplied Write request를 flags/target/type/
  length/timeout/value까지 exact 대조하고, 같은 owner/session의 exact ticket/SubmitCycle/BootId에
  bind된 `Completed+Success` Write terminal status까지 요구한다. owner/current session은 capability RPC 전에,
  fresh `DiagnosticsBootId`/`MapRevision`은 `0x7E50` 전에 검사한다. terminal 해제도 Read
  ticket/status/fresh capability가 같은 owner/session provenance와 원 identity에 모두 일치하고
  fresh capability observation sequence가 context baseline보다 크며 exact 4-byte 값이 같을 때만
  허용한다. 이 public context는 Axis1-only SDK SDO Write allowlist를 우회하지 않으며 bit 9 off,
  축 2~4 또는 다른 tuple에서는 새 Write가 계속 fail-closed다.
- D5 quarantine은 UI field의 mutable list가 아니라 `D5SdoQuarantineLedger`가 소유한다.
  owner-bound opaque handle, immutable evidence snapshot, entry/global revision과 exact-once
  disarm을 사용한다. accepted ticket은 `LMCOperationTicket.BelongsTo`로 owner connection을
  검증하고 ticket의 `DiagnosticsBootId`/`SubmissionMapRevision`을 actual BootId/MapRevision과
  exact match한 뒤 전이한다. recovery는 proof 자체의 두 임시 accepted
  guard는 허용하지만 persistent evidence 변경이나 candidate 이후 ABA를 거부하며, PASS log
  callback 성공과 clear를 같은 ledger lock에서 commit한다.
  UI 독립 deterministic concurrency 4개는 각 등록 test를 50회 반복해 candidate snapshot 뒤
  clear 전 mutation, atomic clear 뒤 Arm 보존, callback 예외 뒤 waiter/ledger 재사용과
  concurrent Disarm exact-once를 bounded wait로 검증하며 `Thread.Sleep`을 사용하지 않는다.
  이는 PC test 강화이며 production/wire/LASAL 변경이나 PLC live 증거가 아니다.
  별도 UI 독립 pending cleanup orchestrator 9개는 owner/ticket/Map preflight, Boot 우선·Map
  mismatch quarantine 무송신, cached terminal/pending, Queued cancel과 race, Running wait,
  exact cancelled terminal, status/exception 보존 및 15~120초 `<=` 경계를 검사한다. production
  WPF adapter는 이 orchestrator를 호출하지만 wire/LASAL 변경이나 PLC live/pcap 증거는 아니다.

### 6.7 D5 SDO queued cancel -> recovery qualification

- runner는 canonical read-only `Slave 1..4 / 0x6061:0 / Int8 / 1 byte`만 허용한다.
  powered-off/stationary 3-sample preflight, stable nonzero BootId/MapRevision과 baseline
  `Completed/Success`를 target Submit 전에 완료한다.
- target Submit 다음에는 status를 먼저 읽지 않고 `CancelOperation`을 한 번만 보낸다. Cancel
  success response와 target status는 동일 ticket/kind/BootId/SubmitCycle의
  `Cancelled/Cancelled`, error/detail/result 0이어야 한다. 그 뒤 distinct recovery ticket이
  baseline과 exact 같은 Int8 값을 `Completed/Success`로 반환해야 PASS다.
- exact `InvalidState`는 PLC가 이미 Queued를 벗어난 race다. Cancel을 재시도하지 않고 같은
  ticket을 terminal까지 drain하며 `INCONCLUSIVE`로 기록한다. 이 경로에서는 recovery Submit을
  보내지 않는다.
- Cancel transport 결과가 불명확하면 accepted target ticket을 quarantine하고 자동 Cancel과
  Submit을 replay하지 않는다. operator-triggered D5 quarantine resolution만 이후에 허용한다.
- UI 독립 orchestrator test와 actual-control capability/in-flight gate는 PC 계약일 뿐이다.
  전용 10개 회귀는 Submit-Cancel-Wait 순서, exact Cancelled/Cancelled, InvalidState race의
  무재시도 terminal drain, 불명확 응답 보존과 distinct same-value recovery를 검사한다.
  실제 PLC PASS에는 QTEST의 ticket/cycle/identity와 pcap의 immediate `0x7E04`, exact
  `Cancelled/Cancelled`, distinct recovery sequence가 함께 필요하다. current LASAL scheduling의
  Queued window가 매우 짧으므로 Running race는 실패가 아니라 미검증 결과로 분리한다.

### 6.8 D5 abrupt disconnect -> application recovery qualification

- `Run D5 Abrupt Disconnect -> App Recovery`는 selected axis의 PowerOff/Standstill/position-stable
  preflight와 stable D5 capability, exact `0x6061:0 Int8/1` baseline을 먼저 확인한다.
- 사용자가 선택한 read-only probe가 nonterminal이면 old `LMCConnection`의 TCP socket을
  zero-linger로 닫는다. 이 qualification 전용 경로는 RPC Close `0x405D`를 보내지 않는다.
  old ticket이 loss 전에 terminal이면 transport를 닫지 않고 `INCONCLUSIVE`로 끝낸다.
- recovery는 old object를 재사용하지 않는 distinct new `LMCConnection`을 열고 fresh
  owner/session-bound capability를 두 번 읽는다. BootId/MapRevision,
  DiagnosticsBuild/CapabilityBits, BaseCycleTimeUs, MaxSDO와 request/response payload limit이 stable한
  상태에서 서로 다른 exact `0x6061:0 Int8/1` ticket 두 개가 baseline과 같은 값을 반환하고, 이후
  final capability sample도 같은 identity/shape여야 한다.
- old executor가 drain되는 동안 recovery Submit이 exact `Rejected/ResourceBusy`면 probe request
  timeout에 5초를 더한 최대 120초의 monotonic retry-admission budget에서 25 ms 간격으로만
  재시도한다. 이미 시작된 단일 RPC의 소요시간은 이 budget의 wall-clock 상한이 아니다. accepted
  또는 outcome-uncertain submission과 다른 오류는 즉시 보존형 실패이며 자동 재시도하지 않는다.
- PASS log callback은 quarantine ledger lock 안에서 clear보다 먼저 동기 commit한다. log 실패,
  cancellation, identity drift, ABA, owner-state race 또는 uncertain submission이면 old/recovery
  evidence를 보존한다. clear가 끝난 proof commit 뒤 늦은 cancellation은 PASS를 `ABORTED`로
  뒤집지 않는다. 성공 뒤에만 GUI가 새 connection을 adopt하고 CREVIS topology auto-load를 실행한다.
- exact Running 표본이 있어도 PC socket loss만으로 PLC의 `MarkOrphan`, executor token과 late
  callback drain을 증명할 수 없다. 따라서 결과는 항상 `ApplicationRecoveryOnly`,
  `orphanQualified=false`, `PASS_APPLICATION_RECOVERY`다. 실제 orphan qualification에는 PLC에
  남는 durable lifecycle witness와 live PLC/pcap이 필요하다.
- UI 독립 코어의 deterministic 회귀는 28개다. 이는 application/ledger 계약이며 PLC runtime
  증거가 아니다.
- actual-control full-handler smoke는 fake RPC의 old/new TCP 두 세션을 사용해 old 세션의
  `0x405D` 부재, distinct connection adoption, exact recovery ticket 두 개, quarantine clear와
  새 topology revision의 CREVIS 재로딩을 한 경로로 확인한다. 실제 wire RST와 PLC orphan
  lifecycle 증거는 아니다.

### 6.9 검증 경계

Qualification UI와 assertion/cleanup 코드는 구현돼 있고 C# build와 정적 계약으로
검사할 수 있다. current WPF Debug/Release Rebuild는 PASS했고 별도 STA actual-control
full smoke는 339/339 PASS다. Admin
capability/axis/group read와 Drive mode/non-atomic status를 exact request, non-default axis
lookup/AxisInfo payload 및 typed UI로 고정한다. 실제 Connect
event, fake RPC의 bit-14-only 7-node topology, CREVIS 3행 렌더, 초기 bit 14 OFF 뒤 수동
Reload의 CREVIS 복구, bit 15~17 live 버튼 차단과 일반 diagnostics RPC 진행 중 SDO Write
editor의 편집/값 유지 및 Submit 직렬화, one-click Inline Read typed/raw terminal,
pre-accept PC 취소의 zero-submit, accepted timeout/PC 취소의 ticket 및 last-status 보존/수동
Refresh, terminal 실패의 정확한 status 표시와 guard 해제, capability-off zero-wire 및
D5 contention/timeout/queued-cancel/abrupt-disconnect 버튼 gate를 검증한다. Axis safety smoke는
  Stop/Reset command-before durable journal, accepted observer, restart zero-replay/status-only proof와
  실제 child-process Kill 뒤 journal lock 재획득을 확인한다. ACK 뒤 durable MarkAccepted 직전 Kill은
  동일 physical identity의 `ArmedBeforeDispatch -> RecoveryRequired`와 command replay/status poll 0회를
  고정한다. Reset -> Stop takeover는 old transport를 session-pinned abort한 뒤 별도 connection에서
  Stop 1회와 status 3회/final D0를 수행한다. completed Reset 뒤 Stop NACK도 final D0 identity가
  일치할 때만 resolve하며 mismatch는 exact Stop/predecessor record를 `RecoveryRequired`로 유지한다.
Group Enable smoke는 accepted observer의 durable state 4 기록과 restart exact-identity status-only
복구를 포함한다. 실제 child process를 ACK 뒤 첫 status에서 Kill한 뒤 journal lock을 재획득하고
새 session에서 `0x2047` 0회, `0x2045` 3회로 resolve하며, Set Identity/Home Check 미복원과 Move
fail-closed를 함께 확인한다. 이는 fake-RPC/PC 계약이며 PLC runtime이나 hardware proof가 아니다.
Group Reset smoke는 accepted 뒤 일반 Read Status와 status-only Resume을 분리하고 fresh path의
`0x2049` 1회를 고정한다. timeout/status failure, valid NACK, ACK response loss, reconnect/Close/mutation
interlock, safe Disable/PowerOff/Stop supersede, final LockedStandby의 Disable-only recovery를 검증한다.
durable smoke는 command-before Arm/ACK MarkAccepted/resolve-first, endpoint/build/BootId/Map/group/member
mismatch quarantine, fresh `0x20D2` exact attach, recovery `0x2049` 0회와 process-kill/restart 경계를
검증한다. captured-member Axis Stop/PowerOff가 accepted 또는 outcome-uncertain이면
`SupersedePendingGroupResetAfterCapturedMemberSafetyMutation`으로 SDK pending도 exact terminalize하고,
valid NACK rollback은 false로 보존함을 검증한다. 이는 fake-RPC/PC 계약이며 PLC runtime이나 hardware
proof가 아니다.
추가 topology smoke는 첫 성공 `INITIAL`, 동일 reload `UNCHANGED`, 유효한 entry 변경
`CHANGED`, endpoint 변경 시 새 `INITIAL`을 검증한다. ordinary 실패와 delayed stale-session
response는 baseline object/hash/evidence를 바꾸지 않으며 disconnect 뒤 저장 버튼 유지와 UTF-8
no-BOM TXT의 configured-only 경계도 확인한다.
same-value Write의 readiness와 마지막 실행 결과는 별도 control에 표시해
checkbox reset/UI refresh 뒤 `PASS` 또는 `RECOVERY REQUIRED` evidence를 보존한다.
bits 14~16 fake RPC 경로는 background `0x7E13` Health와 선택 input `0x7E22`를 행/상세에
반영하되 output shadow를 자동 조회하지 않는지, selection/session/topology가 바뀐 늦은 수동
응답이 새 선택 UI나 write shadow를 오염시키지 않는지, Health/DI 오류와 cycle이 채널별로
독립인지 확인한다. mixed-I/O 행에서는 자동 DI가 output proof detail을 보존하고 수동 DI 또는
read 실패가 기존 shadow/confirmation을 해제한다. 모두 fake RPC PC 계약이며 PLC live 증거는
아니다. Double smoke는 bit 6 + exactly two
buffers + 실제 4-entry Recordable Catalog로 `DoubleContractReady=True`를 만든 뒤에도 live
버튼 disabled, 수동 Double mode 미노출, 강제 주입 Configure와 dormant recovery 범위의
`0x7E40..0x7E4F` zero-wire를
확인한다. exact Adopt도 대상 mode를 wire 전에 알 수 없으므로 Double capability가 광고된
상태에서는 handler와 버튼을 막고 같은 smoke에서 `0x7E49` zero-wire를 확인한다. 다섯 번째
smoke는 recovery command-range guard 자체를 검사한다. 결정적 Guid -> nonzero ConfigId와 active
journal에서도 global mutation interlock과 독립된 `DoubleRecoveryContractReady=True`를 검증하고,
semantic journal conflict 뒤 usable 상태와 실제 runtime I/O failure 분류를 확인한다. 네 proof/route
gate가 닫힌 상태에서 강제 recovery handler가 zero-wire인지 확인한다. 잠긴 D4 Double journal을 두 번째
WPF가 열지 못할 때 신규 mutation admission이 `MutationJournalUnavailable`로 닫히는 것도
검증한다. 실제 WPF child process는 SDO/DO unresolved record와 D4 Double active record를
각각 두 번 재시작해 single-writer lock, Close interlock,
`0x7E50/0x7E23/0x7E40..0x7E4F` zero-replay, 강제 종료 뒤 byte-identical journal과 동일
identity/state 재복구를 검증한다. typed v2 SDO record의 재시작 복구를 강제 호출해도
capability-off 상태에서 capability/SDO wire가 추가되지 않는 검사도 유지한다. D4 journal은
WPF 시작 시 상태만 복구하며 inventory/adopt/release를 자동 실행하지 않는다. 전용
`Recover Double Journal` control과 handler는 `ReconnectRecovery` proof gate를 첫 실행 guard로
재검사한다. qualification, same-session retained cleanup, partial-Adopt continuation과 reconnect
recovery와 config-only manual Configure adapter는 구현됐지만 `ManualActions`,
`ManualConfigureRoute`, `QualificationExecution`, `ReconnectRecovery` 네 gate는 모두 false다.
시험별 임시 mutation journal을 사용하며 실제 PLC/SDO Write는 송신하지 않는다. 이 과정에서
MainWindow XAML 생성 중 조기 전체 UI 갱신 NRE와 disconnected close 재진입도 회귀로 막았다.
기존 Debug visual/startup smoke에서는 Group/Bulk/Recorder panel 렌더와 prerequisite 미충족
초기 실행 버튼 disabled를 확인했다. 이는 WPF 렌더와 fail-closed gate 확인일 뿐이다.
이전 API 검증 스냅샷의 Debug/Release는 649/649 PASS다. Axis/Group typed lookup과 public bounded inline SDO Read의 typed
  terminal/pre-wire rejection/terminal-before-cancel/failure/timeout/cancel evidence 9개,
  nonterminal `LastObservedStatus` 보존과 기존 pending cleanup 계약에
  SDO Write target/격리/terminal cleanup, exact manual readback interlock, stale-session 물리 확인
  recovery, durable resolve-before-clear 순서, process-termination journal reopen, CREVIS exact
  status-cause matrix, topology/D5 fixed-seed parser property와 topology-bound health/I/O 및
  raw-output write 차단, public SDO verification provenance와 pinned capability single-wire
  health/DI, Catalog/Topology aggregate provenance와 PI Write pre-wire, auto live monitor 및
  send-priority ordering/zero-wire/SDO·DO `NotAttempted` 회귀 8개, diagnostics admission
  exhaustive truth-table 7개, RPC lifecycle deterministic race 회귀 19개,
  Double-bank retained lifecycle/release uncertainty, exact 0x7E4A/4B reconnect,
  token-qualified 0x7E4C/4D Configure-response-loss, final
  configuration Release의 typed canonical-empty absence, durable v3 token identity와
  v2 legacy fail-closed, intent/confirmed crash-window, exact pending bank/config intent 재사용과
  retained ACK-success zero-replay durable confirm/resolve 회귀, Recorder recoverable parser
  deterministic property와 opt-in eight-family parser-stress CLI 계약이 추가됐다.
  Double core non-durable orchestrator는 성공/실패/cancel 시 resource를 자동 해제하지 않고 exact
  handle이 있는 명시적 unexpected third -> B -> A -> configuration release primitive를 제공한다.
  durable WPF는 third Start exact ResourceBusy인 same-session scope에서만 B -> A -> configuration을
  허용하고 unexpected success/ambiguous에서는 Release를 zero-wire로 막는다. ambiguous
  Configure/Start/Release도 destructive retry 없이 보존한다. final Release 응답 유실은
  exact nonzero identity의 absence proof로 journal을 resolve한다. external-session-loss reset/rebind와
  explicit cleanup adapter도 WPF에 연결됐지만 PLC build/RAM/jitter/live/pcap proof가 없어
  네 proof/route gate는 열지 않았다.
  Group queue chaining/Stop-first wire
order, 수정된 `0x2047`,
Bulk 100회와 one-slave-offline partial/recovery, Recorder Single/Ring/soak/reconnect-adopt,
D5 abort/recovery는 해당 PLC build를 다운로드한 실물 장비에서
아직 실행·packet capture하지 않았다. 따라서 runner의 `PASS`와 지정 capture의 wire
조건을 모두 얻기 전에는 production qualification 완료로 표시하지 않는다.

## 7. callback 범위

Connect가 callback listener와 endpoint 등록까지 처리한다. legacy raw payload는 시각,
remote endpoint, 길이와 최대 48-byte hex preview로 기록한다. current WPF의 version-2
typed wake는 non-authoritative hint이며, exact retained ticket의 TCP `0x7E03` 결과만
terminal state로 반영한다. Gate D source에 sender/broker와 production-path candidate
caller가 있어도 exact downloaded producer와 live UDP/TCP causal capture가 없으므로
motion-complete 또는 production runtime 증거로 해석하지 않는다.

## 8. 검증 기준

- Debug/Release solution rebuild
- `LasalMotionControlLib` project reference 출력 DLL 확인
- legacy transport와 제거 화면 class 참조가 신규 프로젝트에 남지 않았는지 정적 검색
- Jerk 입력 활성화, DINT 범위 검사와 Stop/Move API 전달 확인
- LASAL static contract에서 `_JERK_PROFILE`, nonzero JMax, Jerk 수신 offset과
  `_LMCAxis` 및 `_LMCRobotBase1` 전달 경로 확인
- Group Power On/Off, profile Lock/Unlock, Reset/Stop/Read Position,
  Move Linear Absolute/Relative 및 Set Identity Kinematics의 UI-to-API handler와 group InPosition
  monitor 확인
- 실제 실행 창과 모든 탭의 layout/accessibility smoke test
- diagnostics capability fail-closed 상태, Catalog selection, Bulk resource lifecycle,
  Recorder mode/trigger capability gate, Ready/Header gate, reconnect adoption,
  download progress/cancel, metadata CSV와 plot smoke test
- general-inline SDO Read와 exact allowlist SDO Write ticket submit/status/queued cancel,
  terminal Read typed 1/2/4-byte inline result/save, Write safe-axis/비모달 exact-second-click
  confirmation/quarantine 및
  PI Write/extended result gate. Read live packet은 1/2/4-byte와
  동일 BootId TypeMismatch recovery까지 PASS했다. read-only abort/recovery runner와
  analyzer 및 Axis1-only Write 경로는 code/build/test와 fresh LASAL IDE Rebuild/Link까지
  완료됐지만 PLC download, UI[24] 소유권, PLC live/pcap과 나머지 fault matrix는 별도다.
- Read-only API의 Admin capability fail-closed, axis/group semantic allowlist,
  physical axis lookup/reference 검증과 drive status non-atomic 표기
- 실제 PLC 시험은 Read Status/Position부터 시작하고 motion은 마지막에 수행
- `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 생기기 전까지 UI에 추가하지 않음

구현된 runtime qualification UI의 원 설계와 단계별 packet 합격 기준은
`../../docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md`를
따른다.

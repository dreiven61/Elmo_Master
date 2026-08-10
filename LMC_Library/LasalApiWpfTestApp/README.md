# LASAL Motion Control API Example

루트 `Codex_LASAL_WPF`의 기능을 현재 `LasalMotionControlLib` 기반의 간단한
실제 PLC 예제로 다시 구성한 프로젝트다. 기존 `LasalMotionControlLibTestApp`은
이 예제로 대체되어 제거했다.

## 빌드

Visual Studio 2019에서 `LasalApiWpfTestApp.sln`을 열고 `Debug|Any CPU` 또는
`Release|Any CPU`로 빌드한다. solution 표시는 Any CPU지만 실행 프로젝트의
`PlatformTarget`은 x64다. 출력 파일 이름은 `LasalMotionControlApiExample.exe`다.
현재 실행 파일은 시작 로그의
`CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5` marker로 구분한다.
`LMC_API_Distribution` 아래 복제본은 이 current source/session-proof 계약과 동기화되지
않은 stale artifact이므로 현재 예제 실행이나 배포 기준으로 사용하지 않는다.
앱은 `MainWindow`와 recovery journal을 열기 전에 Windows session 단위 named Mutex를
획득한다. 이미 실행 중이면 두 번째 프로세스는 경고를 표시하고 journal 또는 network port를
열지 않은 채 종료한다.

## 런타임 UI 언어

상단 `Language` selector는 실행 중에 UI chrome(창 제목, 탭/그룹 제목, label, button,
tooltip, table header), 상태에 따라 바뀌는 Power/Reset/Stop/Group 복구 action, 안전 안내,
시작/안전 확인 창과 파일 저장 창을 `English`와 `한국어` 사이에서 바꾼다. Data binding은
유지되며 source text가 갱신될 때 선택 언어가 다시 적용된다. raw RPC/callback log,
result/evidence payload와 사용자가 입력한 값은 영어 원문과 protocol token을 그대로 유지한다.
언어 전환은 `CurrentCulture`/`CurrentUICulture`를 바꾸지 않으므로 기존
`InvariantCulture` 숫자 parsing/formatting과 raw DINT 계약도 바뀌지 않는다.
Windows 표준 MessageBox와 파일 저장 창의 `Yes`/`No`/`OK`/`Cancel` 같은 시스템 버튼
문구는 selector가 아니라 Windows 표시 언어를 따른다.

선택값은 `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\ui-language.txt`에 저장되어
다음 실행에 복원되며, 파일이 없거나 읽을 수 없거나 값이 잘못되면 English로 시작한다.
자동 시험은 주입된 임시 preference 경로만 사용하므로 사용자 LocalAppData 파일을 읽거나
덮어쓰지 않는다. XAML에 사용자 표시 문자열을 추가하고 한국어 catalog 등록을 빠뜨리면
UI localization coverage 시험이 실패한다. 동적 안전/복구 action과 guidance도 별도 catalog
회귀 시험으로 확인한다.

프로젝트는 아래 공용 API 소스를 직접 참조한다.

```text
../LMC_API_Delivery/src/LasalMotionControlLib.csproj
```

## Callback endpoint와 session 귀속

SDK library의 `0x405C` 기본은 기존 legacy raw 12-byte request/4-byte ACK다. 이 WPF
example은 명시적으로 `Version2WakeHint`의 32-byte request/20-byte response와 maximum
52-byte datagram을 선택한다. PLC source는 callback IPv4가 현재 valid TCP peer와 exact
match하고 UDP port가 `1..65535`일 때만 최초 tuple을 저장한다. 같은 tuple의 재등록은
성공하지만 하나라도 다른 re-registration은 실패하고 이전 tuple을 그대로 보존한다.

session init `0x8080`의 exact short failure
`01 00 04 00 00 00 00 00 01 00 FF FF`는 outer `Status=1`, command
`Status=1`, `ErrorId=-1`이다. SDK는 이를 `ParseAcknowledgement`로 읽어 UI/예외에
`ErrorId=-1`을 보존한다. 이 example이 선택한 `Version2WakeHint`에서는 frame valid,
`HeaderReserved=0`, payload 4 bytes, command `Status=1`, `ErrorId=-1`이 모두 맞을
때만 20 ms cancellation-aware 대기 뒤 같은 TCP socket으로 session init을 한 번 더
시도한다. persistent second failure는 `Faulted`와 TCP/UDP cleanup으로 끝나고, 다른
ErrorId, nonzero reserved, malformed response는 재시도하지 않는다. 따라서
`ErrorId=0`인 non-canonical short ACK도 재시도 0회이며 TCP/UDP/listener와 WPF
connection을 정리한다. 이 동작은 PLC의
지속적인 callback disarm result `-8`/`-9`를 수정하는 것이 아니다.
WPF 회귀는 첫 Connect의 canonical 두 init 시도가 모두 실패한 경우와
non-canonical `ErrorId=0`이 첫 init에서 실패한 경우 모두
`Disconnected`/`Stopped`, Connect 재활성, 내부 connection 제거를 확인하고, 다음 수동
Connect가 새 TCP session에서 `0x8080 -> 0x405C`로 성공하는 것을 고정한다.

GUI는 connection cleanup 뒤에도 RPC init 시도 횟수, canonical retry 사용 여부와 마지막
ACK를 Active/Retired evidence로 보존한다. `af4ab63`부터 입력 tuple은
`RequestedCallback`, 실제 UDP endpoint는 `BoundCallback`으로 표시한다. init 실패 전
bind가 없으면 `BoundCallback=not-bound`이고, callback port `0`으로 성공하면
`RequestedCallback`의 `:0`과 별도로 실제 양수 ephemeral port를 표시한다. 성공한
version-2 등록에서는 BootId,
SessionEpoch, cookie, listener generation, expected source와 event mask를 표시하고, PC
receiver의 accepted/rejected/duplicate/out-of-order 누계와 마지막 decision/protocol error를
표시한다. `af4ab63` 현 시점 PC 회귀는 SDK `1117/1117`, WPF `335/335` PASS다. 이 표시는 PC측
관측 증거이며 pcap, PLC `RpcCallbackLastDisarmResult`, PLC producer/sender counter를
대체하지 않는다. `0x8080` 응답만으로 PLC의 `-8`/`-9` disarm 원인을 판별할 수도 없다.

`LMCCallbackWakeHintEventArgs`에는 typed non-authoritative wake와 session provenance가
있다. EventType 1은 `DiagnosticsOperationTerminalAvailable`, EventId는 nonzero D5
TicketId다. WPF는 UI dispatcher에서 callback event와 retained
`LMCOperationTicket`이 동일 active connection/local session/DiagnosticsBootId/TicketId에
속하는지 다시 검증한다. exact match일 때만 ticket별 single-flight
`GetOperationStatusAsync` (`0x7E03`)를 실행하고, 성공한 TCP response만 기존
terminal/journal UI path에 반영한다. UDP로 ticket 또는 state를 만들지 않는다.
unknown/stale/busy wake는 버리며 manual refresh/polling fallback은 유지한다.

설치된 SIGMATEK `GetBroadCastData.st`가 IPv4 UDINT를 LSB-first octet 순서로 변환하는
source는 peer 비교 byte order의 정적 근거다. 2026-08-10 10:35 C78/ARM incremental
`Build project`는 변경 class `LMCDiagnosticsService`, `LMCUdpCallbackSender`,
`TCPMotionInterface`를 compile했고 source warning 60개(`W0069=28`, `W0070=21`,
`W0072=11`), compiler error 0, `Linker Done`으로 끝났다. 첫 `Download Project`는
세 class LBA와 PLC link가 성공해 `Download Ok`였고, 두 번째 Download는 CPU-state
timeout 뒤 aborted됐다. reconnect는 성공했고 `Project successfully loaded`가 확인됐다.
이는 strict C78 Rebuild 증거가 아니며 registration duplicate/mismatch packet과 실제
UDP callback capture도 없으므로 runtime PASS로 보지 않는다.

같은 날 이전 LASAL PID 4832 session에서 첫 번째 `Rebuild project`는
`Classes.lcb` 저장의 `ios_base::failure` 예외와 write-failed 두 error record가
있어 무효다. 두 번째 Rebuild 구간은 C78/ARM source warning 76개
(`W0069=35`, `W0070=21`, `W0072=17`, `W0073=3`), source error 0,
`Compiler Done`, `Linker Done`, `CInvalidArgException=0`으로 끝났다. 생성된
`Classes.lcb`는 8,549,773 bytes, SHA-256
`3AC3D938DC1520FAEA6C3693161ABDB280CC873A97C60CF79B3F716C7F064C22`다.
focused `VerifyCurrent`는 exit 0의 `CAPTURE TerminalWakeBrokerCandidate`를 출력했고,
bootstrap `ValidateOnly`는 `UNTRUSTED`, `outputCreated=false`로 끝났다.

그러나 PID 4832 session에는 Rebuild가 두 번 있었고 뒤에 Connect, Reset,
Restart가 실행됐다. post-build `Find in Implementation` action은 없었고 Download는
0회였다. Find는 Object Network Server/Client 행에만 적용되고 일반 method 행에는
해당하지 않으므로 이 historical 부재 자체는 세 Gate D method의 미완료 사유가 아니다.
Reset/Restart는 기존 PLC image만 다시 실행한 것이다. 따라서 이 session 자체는 격리된
strict build evidence, exact-method UI open, 신규 Download, live callback 증거를
완료하지 않았고 `ProductionApproved=false`,
`NeedsRebaseline=true`를 유지한다.

retained pre-drift strict evidence는 `GateDVisualLayout` PID 480 / Rebuild TID 3396이다.
canonical project load 1회, C78/ARM `Rebuild project` 딱 1회,
Connect/Download 0회, 정상 project close/IDE exit를 기록했다. command window는
warning 76개(`W0069=35`, `W0070=21`, `W0072=17`, `W0073=3`), error 0,
`Compiler Done=2`, `Linker Done=1`, post-result C82 compatibility warning 6개,
`CInvalidArgException=0`이다. `VerifyBuild`는 `profile=GateDVisualLayout`,
`inputsEquivalent=true`, `rawInputsUnchanged=10/10`,
`regeneratedOutputsBound=2`, `evidenceSource=bounded-repository`로 PASS했다.
exact identities는 baseline 6,887 bytes /
`247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE`,
raw range `[6532176,7298848)` 766,672 bytes /
`B918E51279360E27780D212650361AF361FFFC391C5F24854447BE0F3F9ABD17`,
manifest 1,574 bytes /
`7928BC0D641FEA79444EDE8AD49FC10C15C28D453DB75DAF82C21B9D303D1DFC`,
transcript 30,111 bytes /
`F32122D318DBFD8F53BC9E5AD0FF693F9B6F05368D40FC64138A010A1BC810AF`다.
이 checkpoint에 묶인 `Classes.lcb`는 8,549,773 bytes, SHA-256
`24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`다.
PID 7288/D71E...는 superseded historical evidence로만 보존한다.

checkpoint focused verifier pin은 canonical-LF 545,566 bytes /
`FBF1A8582E85039377AC39F26D8BBA64C0EB62665424DE150083CFC412CC7CA3`이고,
capture self-test는 positive `46` / negative `94` PASS다. latest bootstrap
`ValidateOnly`는 `gate_d_terminal_wake_broker_candidate_checkpoint.json`을
3,225,878 bytes /
`E0490DC348B861FBE47AB4C2E9C558BE679E865787A014860EBA45B3E0E508E4`로
계획했지만 그 bootstrap run은 `UNTRUSTED`, `outputCreated=false`였다. 이후 trust-anchor
commit `bb5fd93`과 sequence-4 commit `5543579`가 physical manifest와 exact 7개
production path를 원자적으로 commit했다.

PID 480의 isolated Rebuild session에는 method-specific UI proof가 없다.
`Find in Implementation`은 Object Network
Server/Client 행에만 적용되고 일반 class function/method open에는 해당하지 않는다.
사용자는 이 row-level Find action의 정상 동작을 별도로 확인했지만 이는 selected method
open 증거가 아니다. method 행은 `Edit Method`, Enter 또는 direct open으로 열고 exact
Implementation tab/header를 확인한다. 사용자는 이후
`LMCDiagnosticsService::TryTakeD5TerminalWake`,
`LMCUdpCallbackSender::PublishEvent`, `TCPMotionInterface::PublishD5TerminalWake`
세 method의 정확한 Implementation 표시가 정상임과 LASAL 종료를 직접 확인했다. 이 UI
evidence는 `exactMethodOpen=manual-attested`이며 동일 UI 동작을 다시 요청하지 않는다.
`Lasal2.log`의 Open Implementation은 class-level token이고 자동 session restore에서도
생길 수 있어 selected method를 증명하지 못한다. 자동 method-smoke JSON/log artifact는
별도 pending/nonblocking이며, log delta는 session 경계, `CInvalidArgException`, 기록된 금지 명령
audit에만 사용한다. trusted sequence-4 checkpoint와
`Class/Classes.lcb`, `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`,
`Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`,
`Class/TCPMotionInterface/TCPMotionInterface.st`,
`Class/_UDPTransceiver/_UDPTransceiver.st`,
`Network/Comm_Network/Comm_Network.lcn`, `Network/Networks.lcb`의 exact 7개
production transition path의 atomic commit은 `5543579`에서 완료됐다. 그 뒤 PID 34656의
C78 Rebuild/Download는 `Download Ok`까지 기록했지만 현재 `Classes.lcb`를 동일 길이의
`6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712`로 바꿨다.
이는 manifest-bound `24402BFA...`와 다르므로 focused/C78 current verification은 실패한다.
reviewed rebaseline 전 runtime 테스트는 exploratory이며, 추가 Download를 반복하지 않는다.
`ProductionApproved=false`, `NeedsRebaseline=true`를 유지한다.

실제 `MainWindow` 컨트롤과 fake RPC를 사용하는 별도 STA smoke runner는 다음과 같이
실행한다. 사용자 기본 mutation journal이 아니라 시험별 임시 journal을 사용하며 PLC에는
접속하지 않는다.

```powershell
MSBuild.exe .\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj /t:RunWpfSmokeTests /p:Configuration=Debug /p:Platform=AnyCPU
```

2026-08-10 `f337fec`/`ad7c8b1` Release 스냅샷은 smoke `334/334` PASS였다.
2026-08-11 `af4ab63` 기준 VS2019 MSBuild Release rebuild는 경고 0, 오류 0이고
smoke는 `335/335` PASS다. 이번 tranche에서는 Debug build/smoke를 다시 실행하지 않았다.
Connect 뒤 bit 14만 광고된 7-node
topology가 자동으로 7행/CREVIS 3행을 표시하고 bit 15~17 live 버튼은 비활성인 경로,
초기 bit 14 OFF에서 자동 topology 요청 없이 실패 상태를 표시한 뒤 수동 Load가 capability를
다시 읽어 CREVIS 3행을 복구하는 경로, configured snapshot의
`INITIAL/UNCHANGED/CHANGED`, endpoint reset, 실패/stale baseline 보존과 UTF-8 no-BOM evidence
경로, 일반 diagnostics 요청과 exact Write readback 대기 중에도
다음 SDO draft를 편집하고 capability 갱신 뒤에도 보존하며 Submit만 직렬화되는 경로를 검증한다.
same-value Write의 readiness/다음 시도 상태와 마지막 실행 결과도 분리해 checkbox reset이나
`UpdateUiState()` 뒤 `PASS`/`RECOVERY REQUIRED` evidence가 사라지지 않는지 확인한다.
`Load Required Exact Readback`은 wire를 보내지 않고 pending Write의 exact Read 요청만 명시적으로
editor에 복원하며, 사용자가 바꾼 non-exact 요청은 wire 전 차단한다. bit 14~16을 광고한 fake PLC
경로에서는 background monitor가 `0x7E13` Health와 선택된 input의 `0x7E22`를 실제로 한 번씩
보내 행/상세를 갱신하고 output shadow를 polling하지 않는지 확인한다. 수동 Health/DI/DO의 늦은
응답은 이전 행 cache만 갱신하고 새 선택 상세나 output shadow를 덮지 않으며, Health와 DI의
오류/stale/cycle은 채널별로 분리된다. 같은 연결에서 bit 15/16이 내려가면 이전 live 표본은
`UNAVAILABLE`로 폐기되고 상단/상세 summary도 현재 capability로 다시 계산된다. mixed-I/O 행에서는 자동 DI가 write-authorizing output
shadow 증거를 가리지 않고 명시적 수동 DI 또는 read 실패가 shadow/confirmation을 fail-closed로
해제한다. 이 경로는 fake RPC PC 검증이며 PLC live 증거가 아니다. typed v2 SDO restart
recovery의 capability-off zero-wire, full-ready Double contract의 live/manual/recovery
진입점과 `0x7E40..0x7E4F` zero-wire도 검사한다. D4 추가 회귀는 recovery Guid의 첫
4 bytes를 little-endian nonzero `RequestedConfigId`로 만드는 결정적 변환과, active Double
journal이 ordinary qualification interlock을 만들더라도 recovery용 two-buffer capability 계약은
독립적으로 `True`가 되며 닫힌 proof gate가 모든 Recorder wire를 계속 차단하는지, semantic
journal conflict는 journal을 계속 사용할 수 있게 보존하고 실제 I/O/runtime failure만 journal
runtime failure로 분류하는지를 검사한다.
잠긴 Double journal을 두 번째 WPF가
열지 못하면 신규 mutation admission이 fail-closed하는 것도 확인한다. 실제 WPF child
process에서는 SDO/DO unresolved record와 D4 Double active record를 각각 두 번 재시작해
single-writer lock, Close 차단, `0x7E50/0x7E23/0x7E40..0x7E4F` zero-replay와 강제 종료 뒤
byte-identical journal 및 동일 identity/state 재복구를 검사한다. 실제 SDO/DO Write,
Double inventory/adopt/release 송신이나 PLC runtime 증거는 아니다.

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
15. `4 Enable (Lock Profile)`을 실행한다. GUI는 `0x2047`을 정확히 한 번 보낸 뒤
    `0x2045`를 자동 poll하고 PowerOn + Locked Standby가 3회 연속일 때만 PASS로 끝낸다.
16. timeout, status 오류 또는 Stop/Power Off 우선순위 선점 뒤에는 버튼이
    `Resume Lock Verification (No Enable Replay)`로 바뀐다. 이 버튼은 기존 ACK를 재사용해
    `0x2045`만 다시 보내며 `0x2047`을 재전송하지 않는다. 수동 Read Status 한 번만으로
    continuation을 완료하지는 않지만 safety generation 검증을 통과한 성공 응답은 상태에 맞는
    pending continuation proof에 누적된다. Locked Standby proof가 3/3이면 기존 ACK를 재사용한
    zero-wire Resume으로 완료할 수 있으며, 완료 뒤에만 Move가 활성화된다.
17. 작은 X/Y/Z/U 목표로 `6 Move Linear Absolute`를 먼저 시험한다.
18. `0x7D00`에 `GroupLinearRelative`가 광고된 최신 PLC에서 X/Y/Z/U를 작은
    delta로 바꿔 `6 Move Linear Relative`를 시험한다. PASS는 profile queue 수락이며
    화면의 Group InPosition monitor가 완료될 때까지 기다린다. monitor timeout은
    XYZU 거리, velocity, acceleration과 deceleration으로 계산하며 15~600초로 제한한다.
    축 순서 검증 capture는 나머지 세 delta를 0으로 두고 한 축씩 왕복한다.
19. 종료 순서는 Group Stop 및 stable Standby 확인, `Disable (Unlock Profile)`,
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
  profile은 잠긴 상태로 남으며 runner가 자동 Disable/PowerOff하지 않는다. SDK는 gate, command,
  status와 delay 전체를 한 deadline으로 제한한다. pre-write cancel/deadline은 zero-wire
  `NotAttempted`, post-write caller cancel은 response/accepted evidence를 drain·게시한 뒤 typed
  cancellation이다. ACK/status 무응답은 transport를 `Faulted`로 만들고 각각
  `OutcomeUncertain`/no-continuation과 `Accepted`/exact-continuation 및
  `TransportInvalidatedAtDeadline=true`를 남긴다. WPF runner는 fresh Enable 전 endpoint,
  group name/reference, `DiagnosticsBootId`, `MapRevision`을 durable Group Profile Lock journal에
  `ArmedBeforeDispatch`로 기록하고 accepted observer가 첫 `0x2045` 전
  `AcceptedAwaitingProof`를 저장한다. 재시작은 exact identity의 status-only proof만
  허용하며 `0x2047`을 replay하지 않는다.
- `True Buffered A -> B`는 선택한 X/Y/Z/U 한 축의 software min/max와 group dynamics
  limit를 먼저 확인한다. 같은 방향의 제한된 raw delta A/B를 `Buffered`로 보내되 A가
  아직 InPosition이 아닐 때 B를 송신하고, 누적 endpoint와 stable InPosition을 확인한
  뒤 Aborting relative move로 시작 위치에 복귀한다. 실패나 취소로 motion 가능성이
  남으면 Group Stop을 보내고 stable Standby까지 확인하며, 이 cleanup도 실패하면
  안전 상태 미확정으로 FAIL한다.
- `Deterministic Stop-first`는 app send gate를 잡은 상태에서 zero-delta Move를 대기시키고 Group
  Stop safety generation을 먼저 예약한다. gate를 연 뒤 Begin이 `0x2085` ACK와 exact continuation을
  보존하면 gate를 반환하고, Resume이 `0x2045`만 보내 stable Standby 3회를 확인한다. Stop 전송,
  local assertion 또는 stable-state 검증이 실패하면 accepted continuation이 있는 경우 cleanup도
  같은 continuation의 status-only Resume만 사용하고 새 `0x2085`를 보내지 않는다. accepted evidence가
  전혀 없을 때만 fresh fallback Stop을 허용하며 cleanup 실패는 원 오류와 함께 보존한다. 로그의
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
  별도 run이다. RecorderDoubleBank와 fault 주입은 이 live runner 범위가 아니다.
  별도 PC send-priority 회귀는 Recorder Trigger/Stop의 지연 ACK를 `ResultDiscarded`로 폐기하고,
  buffer/configuration/recovered/adopted identity Release의 지연 ACK는 각 resource를
  `OutcomeUnverified`로 격리해 재사용과 destructive retry를 차단한다.
- `RecorderDoubleBankQualificationOrchestrator`와 이를 호출하는 WPF adapter는 bit 6,
  exactly two buffers와 nonzero BootId를 preflight하고 한 번의 Configure 뒤 bank A/B
  capture/freeze/download, third Start의 exact ResourceBusy, A header/data SHA-256 불변성을
  검사한다. core non-durable orchestrator의 release primitive는 exact unexpected-third handle이
  반환된 경우 명시적 unexpected third -> B -> A -> configuration 해제를 지원하지만, 이 계약은
  durable WPF cleanup 동작이 아니다. WPF adapter는 새 recovery Guid에서 결정적으로 nonzero
  `RequestedConfigId`를 만들고 Configure 전에 journal을 arm하며 성공/실패/cancel scope를 retained
  state로 남기고 자동 Release하지 않는다.
  same-session WPF cleanup은 third Start가 exact ResourceBusy로 확인된 경우에만 허용한다. 이때
  checkbox를 포함한 identity/order preflight가 끝난 뒤 확인을 소비하고 Status -> 필요 시 Stop ->
  Ready/Uploading 확인 뒤 B -> A -> configuration 순서로 해제한다. 시도 중 실패하면 checkbox를
  다시 확인해야 한다. unexpected third success 또는 ambiguous outcome이면 같은 session의 bank와
  configuration Release는 모두 zero-wire다. disconnect/reconnect 뒤 token-qualified exact inventory
  inspection만 허용하고, inventory가 durable two-bank evidence와 충돌하면 external/manual recovery로
  남기며 자동 Release하지 않는다.
  confirmed-not-applied bank/configuration Release intent가 pending이면 동일 target의 exact intent만
  재사용하고 새 intent나 다른 target을 금지한다. retained handle이 이미 ACK-success 상태이면 Release
  wire를 replay하지 않고 journal을 durable confirm/resolve한다. external-session-loss 뒤 exact
  `0x7E4A/0x7E4B` 재채택, token-qualified `0x7E4C/0x7E4D` Configure-response-loss와 durable
  v3 Release intent/confirmed 재시작 복구도 PC 계약으로 구현했다. 4C는 one-shot이고 응답 유실
  후에는 read-only 4D만 재시도한다. legacy v2 ConfigRevision=0은 wire token 증거가 없어 계속
  zero-wire다. final configuration Release 응답 유실은 nonzero exact identity의 canonical-empty
  detail 32를 `LMCRecorderConfigurationAbsentException`으로 받아 journal을 zero-mutation resolve한다.
  reconnect/startup은 active journal을 자동 재생하지 않고 명시적 실행에서만 `0x7E4D -> 0x7E4A
  -> 0x7E49/0x7E4B`로 진행한다. bank Adopt가 일부만 끝난 실패도 반환된 handle을 즉시 보존해
  다음 명시적 recovery에서 나머지 exact Adopt를 이어간다. 확인 시점의 journal/config/exact bank
  집합은 immutable snapshot으로 고정한다. read-only 4D/4A가 새 revision 또는 bank를 발견하면
  journal만 갱신하고 Adopt/Release 전에 `confirmation required`로 중단하며 갱신된 계획을 다시
  확인해야 한다. partial/retained recovery도 mutation 전에 같은 snapshot을 재검사한다.
  다만 `ManualActions`, `ManualConfigureRoute`, `QualificationExecution`, `ReconnectRecovery`
  네 proof/route gate는 모두
  `false`다. PLC build/RAM/jitter/live proof 전에는 모든 Double 버튼이 wire 전에 닫히고
  수동 Double mode도 노출되지 않는다.
  Double capability가 광고되면 exact Adopt도 target mode를 wire 전에 판별할 수 없어 수동
  WPF handler와 버튼에서 함께 막는다.
- D5 SDO Abort -> Recovery는 SDO Read만 사용한다. 선택 slave에 대응하는 `_LMCAxis1..4`가
  `PowerOn=False`, `Standstill=True`이고 actual position 3회가 동일한지 확인한 뒤,
  `0x6061:0 Int8/1` baseline을 읽고 사용자가 제조사 기준으로 선택한 존재하지 않는
  read-only object/subindex를 조회한다. PASS는 abort ticket이 `Failed/Failed`,
  `OperationErrorId=-32000`, `OperationDetail`에 실제 nonzero raw EtherCAT SDO abort code,
  result 없음이고, 같은 BootId/MapRevision의 새 `0x6061:0` ticket이 baseline과 같은 값으로
  성공해야 한다. Cancel Runner는 PLC Stop을 보내지 않는다. 실제 Queued ticket만 cancel하고
  Running ticket은 원래 terminal deadline을 고려한 15~120초 bounded wait로 회수한다.
  코드는 build/test 완료했지만 실제 PLC abort/recovery와 pcap은 아직 검증하지 않았다.
- D5 SDO Contention -> Recovery도 `0x6061:0 Int8/1` Read만 사용한다. stable
  BootId/MapRevision과 powered-off/stationary 상태에서 baseline을 읽고, 첫 ticket을 받은 직후
  status poll 전에 같은 Read를 다시 Submit한다. PASS는 두 번째 Submit의 SDK failure context가
  exact `Submission/Rejected`이고 PLC DetailCode가 `ResourceBusy`인 경우에만 인정한다. 그 뒤
  첫 ticket의 exact `Completed/Success`와 동일 값, 서로 다른 ID의 세 번째 ticket
  `Completed/Success`를 요구한다. 두 번째가 예상 밖으로 accepted되거나 응답 결과가 불명확하면
  해당 ticket/unknown evidence를 quarantine하고 세 번째 Submit을 보내지 않는다. UI 독립 core
  12개와 GUI capability/in-flight gate smoke는 PASS했지만 실제 PLC `23f` 실행과 pcap은 아직 없다.
- D5 SDO Timeout -> Recovery는 같은 canonical Read의 정상 timeout baseline을 먼저 확보한 뒤
  `TimeoutCycles=1` ticket을 제출한다. PASS는 exact `Expired/TimedOut`,
  `OperationErrorId=0`, `OperationDetail=0x05040000`, result 없음과 같은 BootId/MapRevision을
  요구한다. 늦은 callback drain 동안 recovery Submit이 exact `Submission/Rejected +
  ResourceBusy`, 동일 request/identity, ticket 없음인 경우에만 25 ms 간격으로 최대 600회
  재시도한다. 다른 오류, accepted-context 또는 outcome-uncertain이면 자동 재시도를 즉시
  중단하고 evidence를 보존한다. drain 뒤 서로 다른 recovery ticket이 baseline과 같은 Int8 값을
  `Completed/Success`로 반환해야 PASS다. 실제 실행은 QTEST log와
  `23g_SDO_Timeout_Drain_Recovery.pcapng`를 함께 보존한다. PLC timeout/drain/recovery packet은
  아직 없다.
- D5 SDO Queued Cancel -> Recovery도 같은 `0x6061:0 Int8/1` Read만 사용한다. baseline과
  stable BootId/MapRevision을 확인한 뒤 target ticket Submit 직후 status poll 없이
  `CancelOperation`을 정확히 한 번 보낸다. PASS는 Cancel 응답과 같은 ticket의 terminal이
  모두 exact `Cancelled/Cancelled`, error/detail/result가 0이고, 서로 다른 recovery ticket이
  baseline과 같은 값을 `Completed/Success`로 반환한 경우뿐이다. Cancel이 exact
  `InvalidState`이면 ticket이 이미 Running으로 전이한 race이므로 재-cancel하지 않고 terminal을
  drain한 뒤 `INCONCLUSIVE`로 끝낸다. transport 결과가 불명확하면 ticket을 보존하고 자동
  Cancel/Submit replay를 하지 않는다. 현재 LASAL의 Queued window는 매우 짧으므로 PC
  code/build/test 통과와 실제 PLC queued-cancel PASS를 구분하며, live 판정에는 QTEST log와
  `0x7E50 -> 0x7E04 -> 0x7E03 -> distinct 0x7E50` packet 증거가 필요하다.
- D5 Submit 호출 직전에 outcome guard를 먼저 등록한다. 명시적인 PLC rejection이면 제거하지만,
  submit 응답이 유실되거나 transport 결과가 불명확하면 ticket ID를 모르는 evidence도
  quarantine에 보존한다. ticket 응답을 받은 뒤 guard 제거가 확인돼야 일반 active-ticket
  추적으로 전환한다.
- 보존 ticket의 모든 cleanup은 status/cancel 전에 같은 `LMCConnection`의 capability
  BootId/MapRevision을 먼저 대조한다. 둘 중 하나가 바뀌었거나 status가 정확히
  `BootIdMismatch`를 반환하면 old ticket을 조회·해제한
  것으로 간주하지 않고 stale-session quarantine으로 이동한다. local ticket의 session
  generation이 stale인 경우도 quarantine한다. 같은 Boot/session에서 status가 정확히
  `TicketNotFound`이면 one-terminal-slot 교체 계약상 이전 ticket terminal만 확정하고
  `TERMINAL_INFERRED`, outcome `UNKNOWN`으로 해당 ticket을 해제한다.
  여러 known/unknown evidence는 그대로 유지한 채 stable BootId/MapRevision 아래 current
  capability가 GeneralInline이면 서로 다른 두 `0x6061:0 Int8/1`, legacy SDORead-only이면
  서로 다른 두 `0x1000:0 UInt32/4` ticket의 exact type/length/bytes를 모두 확인해야 해제된다.
  UI 독립 `D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합만으로
  scope를 순수 판정한다. MainWindow는 proof 시작 로그와 PASS 로그에 같은 decision을 쓴다.
  owner+BootId+MapRevision이 동질인 경우에만
  `same_owner_connection_recovery`, `new_diagnostics_identity_session`,
  `new_connection_session` 중 하나다. current owner+identity와 모두 같으면 첫 scope,
  current owner와 한 previous BootId+MapRevision을 공유하면 둘째 scope, 모두 current owner와
  다르면서 한 previous owner+identity를 공유하면 셋째 scope다. owner 또는 submission
  identity가 섞이면 `mixed_evidence_sessions`이며 same/new session 증거로 세지 않는다.
  mixed도 two-ticket application recovery proof와 성공 시 quarantine clear는 허용한다. 첫
  scope는 old terminal이나 disconnect/orphan 증거가 아니고 둘째도 orphan PASS가 아니다.
  한 previous owner+identity로 동질인 셋째 scope만 decision의
  `NewConnectionRecovery=true`이고 `newConnectionRecovery=true`로 기록한다. WPF는 항상
  `orphanQualified=false`를 기록한다. 셋째 scope는 새 RPC connection에서 application
  recovery가 성립했다는 뜻일 뿐 PLC 내부 orphan cleanup이나 late callback을 증명하지 않는다.
  실제 orphan PASS에는 known Running old ticket, 실제 owner loss와 별도 PLC hook/capture가
  필요하다. QTEST는 `evidenceBootIds`/`evidenceMapRevisions`,
  `recoveryBootId`/`recoveryMapRevision`, `proofScope`, `mapChangedEvidence`,
  `sameIdentityEvidence`, `mixedEvidenceSessions`, `newConnectionRecovery`,
  `orphanQualified=false`를 분리 기록한다.
- `Run D5 Abrupt Disconnect -> App Recovery`는 production WPF에 연결된 read-only
  application-recovery 시험이다. old owner의 probe가 nonterminal이면 local TCP를 zero-linger로
  닫고 RPC Close `0x405D`는 보내지 않는다. 이어서 기존 객체를 재사용하지 않는 distinct
  `LMCConnection`을 열고 fresh owner/session-bound capability를 두 번 읽는다. stable
  BootId/MapRevision, DiagnosticsBuild/CapabilityBits, BaseCycleTimeUs, MaxSDO와 request/response
  payload limit 아래 exact `0x6061:0 Int8/1` ticket 두 개가 baseline과 같은 값을 반환하고 마지막
  capability sample도 동일해야 한다. old executor drain으로 Submit이 막히면 probe request
  timeout에 5초를 더한 최대 120초의 monotonic retry-admission budget에서 25 ms 간격 exact
  `Rejected/ResourceBusy`만 자동 재시도한다. accepted 또는 outcome-uncertain submission은
  재시도하지 않는다. 이미 시작된 단일 RPC의 소요시간은 이 budget의 wall-clock 상한이 아니다.
  PASS log가 quarantine clear 전에 성공해야 하며, clear 이후 늦은 cancel은 PASS를 `ABORTED`로
  뒤집지 않는다. 이후 새 connection을
  GUI가 adopt하고 CREVIS topology를 자동 load한다. old ticket이 loss 전에 terminal이면 TCP를
  닫지 않고 `INCONCLUSIVE`다. old status가 Running이었어도 PC가 증명하는 것은
  `PASS_APPLICATION_RECOVERY`뿐이고 `orphanQualified=false`다. PLC의 exact `MarkOrphan`, executor
  token과 late callback drain을 남기는 durable witness 및 live PLC/pcap이 없으므로 orphan PASS로
  기록하지 않는다.
- unresolved ticket/evidence가 하나라도 있으면 Configure Bulk, Recorder Configure/Adopt/
  Start/Trigger, Group Disable, motion/PowerOn/Reset, manual SDO/PI, Close와 모든 다른
  qualification 같은 새 mutation을 차단한다. 기존 resource의 Bulk Release, Recorder
  Stop/Release, queued diagnostic Cancel, motion Stop/PowerOff와 read-only는 허용한다.
  `Resolve Preserved D5 Ticket`은 같은 session/새 diagnostics identity session에서도 바로 실행할 수 있다.
  reconnect 자체는 외부 connection loss 뒤에만 허용하며 새 connection에서 Resolve한다.
  `D5SdoPendingCleanup` Resolve는 기존 `qualificationLogLines`를 지우지 않고 이어 쓰며
  `D5_LOG_CONTINUATION`을 기록한다. 따라서 원래 `FAIL`/`OUTCOME_UNCERTAIN`과 resolution
  proof가 같은 저장 QTEST log에 남는다.
- qualification 밖의 manual SDO/Drive read tracker는 직전 qualification의 run/scenario를
  재사용하지 않는다. 별도 `D5ExternalTracking:<stage>` run ID와 step/elapsed 문맥으로
  기록하고, unresolved evidence가 생기면 그 원본 문맥을 Resolve log와 함께 보존한다.
- `GetDriveOperationMode[Async]`/`ReadDriveStatus[Async]`는 원래 예외 형식과 stack을 바꾸지
  않고 caught exception을 `LMCDriveReadFailureContext.TryGet`에 전달해 전체 시도 문맥을
  조회한다. phase는 `FacadePreflight`, `AxisStatusRead`, `CapabilityPreflight`, `Submission`,
  `StatusPolling`, `ResultMaterialization`이고 각 SDO 시도의 `GenericSubmissionOutcome`은
  공용 `LMCSdoSubmissionOutcome`의 `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`
  중 하나다. 기존 `SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`은 호환용으로 같은 값을
  유지한다. 실제 Submit에 사용한 capability `DiagnosticsBootId`와 `MapRevision`, accepted
  ticket의 실제 제출 `SubmissionMapRevision`과 마지막 status도 snapshot에 함께 보존한다. WPF는 submit 없음/명시적
  rejection/terminal이면 guard를 해제하고,
  `OutcomeUncertain`이면 실제 capability identity로 unknown evidence를 보정해 quarantine한다.
  accepted nonterminal은 정확한 ticket을 보존한 뒤 guard를 해제한다. context가 없거나 서로
  모순되면 기존 unknown evidence로 fail-closed한다. drive-read context의 phase는 계속 위의
  6개다.
- 수동 `Submit SDO Read/Write`의 raw `LMCDiagnostics.SubmitSdo[Async]`도 원래 exception을 유지하면서
  `LMCSdoSubmissionFailureContext.TryGet`으로 별도 all-failure context를 제공한다. phase는
  `RequestValidation`, `SessionPreflight`, `CapabilityPreflight`, `Submission`,
  `PostSubmissionValidation`의 5개이고 outcome은 같은 `LMCSdoSubmissionOutcome`을 사용한다.
  실제 dispatch에는 capability `DiagnosticsBootId`/`MapRevision`을 기록하고 `Accepted`에는
  같은 `DiagnosticsBootId`/`SubmissionMapRevision`을 가진 exact ticket을 보존한다. WPF manual router는 `NotAttempted`와 `Rejected`에서 guard를
  해제하고, `OutcomeUncertain`은 실제 identity로 evidence를 보정해 quarantine한다. `Accepted`
  failure는 이전 manual status/result/cancel flag를 지우고 exact ticket을 manual operation
  state와 D5 tracker에 모두 보존한 뒤 guard를 해제한다. context 누락이나 불일치는
  fail-closed한다.
- 성공 Write 뒤 exact Readback은 원 Write ticket/owner/session/`DiagnosticsBootId`/
  `MapRevision`을 보존하고 guarded `SubmitSdo[Async]`로만 제출한다. owner/session mismatch는
  capability RPC 전, Boot/map mismatch는 `0x7E50` 전 거부한다. terminal status와 exact
  4-byte 결과까지 같은 identity일 때만 pending interlock을 해제한다.
- D5 tracker의 quarantine evidence는 UI field 안의 mutable list가 아니라 잠금된 ledger에
  저장한다. opaque handle은 생성 ledger와 exact entry에 귀속되고, evidence snapshot은
  TicketId/BootId/MapRevision/owner/stage/reason/revision을 불변 복사한다. accepted 전이는
  `LMCOperationTicket.BelongsTo`로 owner connection을 확인하고 ticket의
  `DiagnosticsBootId`/`SubmissionMapRevision`을 ledger BootId/MapRevision과 exact match한다. recovery proof는 시작
  snapshot과 최종 snapshot의 전체 내용·순서·revision을 비교하되 proof 자체의 두 임시
  accepted ticket arm/disarm은 허용한다. PASS log callback과 clear는 같은 ledger lock에서
  commit하므로 로그 실패나 concurrent mutation이면 evidence를 삭제하지 않는다.
  UI 독립 deterministic concurrency 4개는 각 등록 test를 50회 반복해 candidate snapshot 뒤
  clear 전 mutation, atomic clear 뒤 Arm 보존, callback 예외 뒤 waiter/ledger 재사용과
  concurrent Disarm exact-once를 bounded wait로 검증하며 `Thread.Sleep`을 사용하지 않는다.
  이 추가분은 PC test뿐이고 production/wire/LASAL 변경이나 PLC live 증거가 아니다.
- D5 pending-ticket cleanup은 WPF와 PC test가 같은 UI 독립 orchestrator를 쓴다. active/current
  `LMCConnection`, ticket owner와 저장 `SubmissionMapRevision`을 dispatch 전에 fail-closed하고,
  current capability BootId mismatch를 MapRevision보다 먼저 판정해 두 경우 모두 status/cancel
  없이 quarantine한다. cached terminal은 무송신으로 끝내고 cached pending은 refresh한다.
  `Queued`에서만 cancel하며 `InvalidState` race와 `Running`은 terminal까지 기다린다. cancel
  accepted면 exact `Cancelled/Cancelled`만 허용하고 fresh status를 active state에 보존한다.
  wait는 최소 15초, 남은 원 deadline+1초, 최대 120초이며 timeout과 같은 `<=` 경계 poll도
  유지한다. 이 checkpoint는 production WPF adapter를 변경했지만 wire/LASAL 변경이나 PLC
  live/pcap 증거는 아니다.

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
qualification을 취소하고 외부 safety 결과를 먼저 검증한다. 이미 accept된 Group Stop이 남으면
cleanup은 exact pending continuation을 status-only로 재개하고 fresh Stop으로 fallback하지 않는다.
accepted evidence가 없을 때만 새 cleanup Group Stop을 보낼 수 있다. 화면/SDK build와 정적 계약 통과는 실제 queue 실행, RT sample 무손실,
packet 순서 또는 장비 안전을 대신하지 않는다.

Axis/Group Stop과 Power Off는 app send gate를 기다리기 전에 공용
`LMCSendPriorityCoordinator`의 safety generation을 선예약한다. WPF가 생성하거나 reconnect하는
모든 connection은 이 opt-in coordinator를 공유하며, SDK는 각 command를 `stream.Write`하기
직전에 ordinary scope의 generation을 검사한다. 따라서 아직 쓰지 않은 foreground/diagnostics
RPC와 compound helper의 후속 RPC는 stale이면 zero-wire로 거부된다. 최종 검사를 이미 통과한
in-flight RPC는 강제 취소하지 않고 결과/timeout을 확정하며, 그 뒤 safety send가 같은 직렬
경로를 얻는다. 앞선 RPC가 transport를 fault로 전환하면 safety send 성공도 보장하지 않는다. safety ACK 뒤에는
같은 exact generation의 상태 monitor 자리를 미리 예약하므로 ordinary command가 ACK와 monitor
시작 사이에 진입하지 않는다. 더 새 safety 요청은 이전 monitor의 다음 write를 stale로 만든다.
qualification의 이 선점은 `ABORTED`로 기록하며, SDO submit이 write 전에 선점되면 tracker는
`Submission/NotAttempted`와 ticket 없음으로 분류해 pre-submission guard를 해제한다. 이 동작은
deterministic PC test 범위이고 PLC/실축에서 확인한 safety timing 또는 인증 결과가 아니다.

Axis Power On은 `PowerOnAndWaitForStableStateAsync`로 `0x2023`을 한 번만 보내고 success ACK 뒤
same-session continuation을 durable journal에 먼저 기록한 다음 `0x2028`의 `PowerOn=true`를 기본
3회 연속 확인한다. mutation/status gate, ACK/status exchange와 delay가 하나의 total deadline을
공유한다. write 전 취소는 zero-wire `NotAttempted`, write 뒤 사용자 취소는 ACK drain과 accepted
observer publication 뒤 typed cancellation이다. ACK/status 무응답 deadline은 transport를
`Faulted`로 만들고 `TransportInvalidatedAtDeadline` evidence를 남긴다. 같은-process Resume은
continuation으로, reconnect/restart recovery는 read-only `WaitForPowerStateAsync(true)`로
`0x2028`만 보내며 어느 쪽도 `0x2023`을 replay하지 않는다. read-only 결과는 power command를
재사용한 것이 아니므로 ACK/continuation이 없고 `ReusedAcceptedAcknowledgement=false`다.
accepted Power On은 write generation과 observed generation을 함께 보존한다. 같은
session/AxisReference의 later mutation이 끼면 typed interference와 pending으로 끝나고, 마지막
status proof/cancel/deadline/generation은 한 publication decision으로 선형화한다.

Single Axis 탭의 `Run LIVE Axis Qualification`은 실제 PLC 명령을 전송한다. 세 물리 안전 확인과
raw 입력 검사를 wire 전에 통과한 뒤 exact connection owner/session, endpoint, Axis name/reference,
`DiagnosticsBuild`, `BootId`, `MapRevision`을 고정한다. 실행 순서는 Power On `0x2023(true)` 1회,
error-free PowerOn/Referenced/Standstill preflight, 시작 위치, Move Relative `0x20A0` 1회,
non-Standstill 관측과 Standstill 3회, 목표 허용오차 안의 최종 위치 3회, Stop `0x2022` 1회와
Standstill 3회, Power Off `0x2023(false)` 1회와 PowerOff+Standstill 3회다. Stop은 계획 이동이
끝난 뒤 보내므로 in-motion halt 증거로 주장하지 않는다. Move 이후 취소/실패는 Move를 재전송하지
않고 같은 durable identity에만 cancellation-independent Stop/Power Off cleanup을 수행한다. Power Off
stable proof가 없으면 FAIL이며 safe state를 추정하지 않는다. 입력, 축 또는 session이 바뀌거나
외부 Axis Stop/Power Off가 개입하면 fixed safety generation으로 runner mutation을 선점 차단한다.
외부 Stop은 Standstill 3회 status-only proof 뒤 runner Power Off만 한 번 보내고, 외부 Power Off는
PowerOff+Standstill 3회 proof 뒤 runner Stop/Power Off를 모두 생략한다. runner 전체는 별도
`AxisQualificationRecoveryJournal`을 Power On 전에 arm하고 PowerOn/Move/Stop/PowerOff의
accepted/stable checkpoint와 최종 `SafeResolved`를 저장한다. 현재 실행이 arm에서 받은 exact
record GUID와 동일 session을 모두 만족할 때만 부모 record 아래의 의도된 Power/Move를 진행할 수
있다. process restart는 자동 replay 없이 exact
identity의 status-only 관찰과 명시적 Stop/Power Off만 허용한다. 부모 `SafeResolved`를 먼저
durable 저장한 뒤 command-level 자식 record를 해제한다.
실행이 끝나면 세 확인은 모두 해제된다. 이 PC runner와 fake-RPC 회귀는 실제 travel/STO/정지시간,
PLC download 또는 packet capture 합격을 대신하지 않는다.

Axis Power Off도 Begin의 `0x2023(enable=false)`와 Resume의 `0x2028`-only stable proof로 분리한다.
일반 timeout/cancel/status failure 뒤 다음 클릭은 exact continuation의 status-only Resume이며,
monitor 중 재클릭은 zero-wire다. later same-axis mutation으로 interference가 확인된 경우에만
`Power Off Again (Confirmed Interference)`를 열어 replacement 1회를 허용한다. replacement reject는
기존 pending/confirmed 상태를 보존하고, accepted replacement 또는 정확한 stable proof만 이를
해제한다. Stop은 명시적 newer safety로 계속 허용한다. WPF의 공용 Axis Power durable v2 journal은
`ExpectedPowerOn` 방향을 기록하고 legacy v1 record는 Power On으로 읽는다. fresh Off는 wire 전에
arm되며 accepted observer가 첫 status 전에 `AcceptedAwaitingProof`를 기록한다. accepted 또는
outcome-uncertain Off 재시작은 exact endpoint/axis/reference/BootId/MapRevision을 확인한 뒤
`0x2028`만 사용한다. Power On의 불명확한 결과는 자동 재송신하지 않고 명시적 Off takeover가
기존 On record를 Off record로 원자 교체한다. journal open/lock/runtime failure 동안 새 live mutation은
fail-closed하지만 명시적 safety Power Off는 process-local degraded record로 허용한다.

Group Power On/Off도 accepted-once split API를 사용한다.
`BeginGroupPowerOnWaitForStableStateAsync`/`BeginGroupPowerOffWaitForStableStateAsync`는 각각
`0x204A`/`0x204B`를 한 번만 보내고 ACK+continuation을 원 session/group publication에서 원자적으로
설치한다. `ResumeGroupPowerStateWaitForStableStateAsync`는 exact continuation으로 `0x2045`만 보내
기대 PowerOn 값을 기본 3회 연속 확인한다. compound On/Off facade는 두 phase의 elapsed total
deadline을 공유한다. accepted timeout/cancel/status failure는 pending과 immutable evidence를 보존하고,
later same-group mutation은 typed interference로 proof 귀속을 거부한다. stale/resolved/concurrent Resume과
pending 위 fresh Begin은 zero-wire typed failure이며 power command를 자동 replay하지 않는다.

WPF는 Group Power command 전에 endpoint, group name/reference, DiagnosticsBootId, MapRevision과
방향을 공용 durable journal에 `ArmedBeforeDispatch`로 기록하고, accepted observer에서 첫 status 전에
`AcceptedAwaitingProof`로 바꾼다. restart의 Accepted는 exact identity status-only 확인만 허용한다.
startup Armed 또는 outcome-uncertain Power On은 `RecoveryRequired`로 승격하며 Power On replay와
status-only resolve를 금지한다. 이때 명시적 Power Off takeover가 durable On record를 Off record로
원자 교체하고 PowerOn=False stable proof로 끝낸다. uncertain Off는 status-only false proof를 먼저
시도하고 typed interference 또는 successful PowerOn=True 관찰 뒤에만 `Power Off Again`을 허용한다.
replacement reject/pre-wire failure는 이전 record와 권한을 보존한다. active record 동안 endpoint/group
편집, 새 mutation과 connected clean Close/reconnect를 차단하고 exact-identity recovery 및
safety/read-only 동작만 허용한다. journal open/lock/write failure도 new Group Power를 fail-closed한다.
On/Off accepted ACK 직후 child process를 강제 종료하는 회귀도 포함하며, 종료 뒤 journal lock 재획득,
새 process의 `0x204A`/`0x204B` zero-replay, `0x2045` 3회 stable proof와 동일 identity의
`Resolved` tombstone을 확인한다.

Axis Stop 버튼은 `BeginStopWaitForStableStandstillAsync`를 priority safety-send phase에서 실행해
`0x2022` success ACK와 exact continuation을 먼저 보존하고, app send gate를 반환한 뒤
`ResumeStopWaitForStableStandstillAsync`를 preemptible monitor phase에서 실행한다. Resume은
`0x2028`만 보내 `IsSuccess && IsStandstill`을 기본 3회 연속 확인한다. timeout/cancel/status failure,
priority preemption 또는 더 새 accepted Stop에도 원 `0x2022`를 replay하지 않는다. WPF는 accepted
Stop의 status-only monitor 중 동일 Stop 버튼을 비활성화하고 Power Off만 다음 safety generation을
예약하게 한다. 선점된 continuation은 volatile evidence로 남아 object/session cleanup 때 제거된다. same-session/AxisReference의 later
`LMCSingleAxis` mutation은 typed interference로 원 Stop 귀속을 막으며, zero-wire mutation과 다른
AxisReference는 간섭하지 않는다.

Axis Reset 버튼도 Begin에서 `0x2024` ACK와 exact pending continuation을 live-command gate 반환
전에 저장한다. Resume은 gate 밖의 preemptible monitor에서 `0x2028`만 보내 successful
`AxisErrorId == 0`을 기본 3회 연속 확인한다. timeout/cancel/status failure와 safety preemption은
accepted continuation을 보존하고 자동 Reset replay 없이 다음 클릭에서 status-only Resume한다.
unresolved accepted Reset 동안 새 live mutation은 interlock되지만 Resume 버튼은 계속 사용할 수
있다. SDK가 `LMCAxisResetInterferenceException`으로 later same-axis mutation을 확인한 경우에만
`Reset Again (Confirmed Interference)`로 전환하고 이후 사용자의 명시적 클릭으로 새 `0x2024`를
허용한다. intentional post-Reset Power On도 기존 Reset 귀속을 무효화한다.

Stop과 Reset은 공용 durable Axis command journal을 command 전에 arm하고 success ACK를 첫 status
전에 `AcceptedAwaitingProof`로 기록한다. accepted observer 저장 실패 시 같은 session의 SDK pending을
회수해 command 재전송 없이 Resume한다. process restart에서는 endpoint, D0 BootId/MapRevision과 axis
name/reference를 다시 확인한 뒤 `0x2028`만 3회 이상 읽고, final D0 live refresh가 같을 때만 resolve한다.
startup `ArmedBeforeDispatch`와 post-write outcome uncertainty는 자동 replay하거나 status-only로
완료하지 않는다.

active Reset 중 Stop은 durable Reset predecessor를 Stop record로 원자 교체하고 Reset session에 pin된
transport abort를 수행한다. abort는 RPC Close를 보내지 않는다. 기존 connection object를 detach/dispose한
뒤 event handler를 아직 붙이지 않은 새 `LMCConnection`에서 RPC init, D0, axis와 active Motion identity를
확인하고 `0x2022`를 한 번만 보낸다. pre-wire failure와 valid NACK는 아직 pending인 exact Reset을
복원하지만 이미 완료된 Reset은 다시 활성화하지 않는다. completed Reset 뒤 valid Stop NACK는
final D0 physical identity가 일치할 때만 Stop tombstone을 resolve하며, 실패/mismatch는 exact
Stop/predecessor record를 `RecoveryRequired`로 유지한다. post-write loss는 Stop을
`RecoveryRequired`로 유지한다. session mismatch는 현재 transport를 끊지 않고 stale
same-session Reset continuation만 폐기한다. Motion과 accepted Stop record가 함께 있으면 Motion을 먼저
resolve하며, 두 journal resolve 사이 process kill 뒤에도 Stop status-only proof만 다시 수행한다.

SDK의 `BeginGroupStopWaitForStableStandbyAsync`는 `0x2085` ACK 뒤 exact
connection/session/group/latest-pending continuation을 반환하고 status를 읽지 않는다.
`ResumeGroupStopWaitForStableStandbyAsync`는 그 continuation으로 `0x2045`만 보내 Standby를 기본
3회 연속 확인한다. 응답 유실/timeout/cancel/status failure/preemption에도 accepted continuation과
evidence를 보존하고 Stop을 자동 재전송하지 않는다. stale/superseded/completed continuation과
concurrent second Resume은 zero-wire로 거부되며 compound API는 두 phase의 elapsed deadline을 공유한다.
Stop이 실제 wire write 경계에 도달하면 같은 group의 pending Enable proof를 초기화하고
per-group mutation generation을 기록한다. 그 뒤 다른 group mutation이 실제 write 경계에
도달하거나 connection session이 바뀌면 마지막 status publication을 성공으로 귀속하지 않고
typed interference/session failure로 끝낸다. WPF 일반 버튼과 Stop-first qualification은 gate를
기다리기 전에 safety generation을 예약하고 Begin만 command gate 안에서 수행한다. ACK, pending
continuation과 recovery evidence를 gate 반환 전에 보존한 뒤 Resume은 preemptible status-only
monitor에서 수행한다. 성공 proof에서만 pending을 지우며, 실패/preemption에서는 cleanup이 exact
pending continuation을 재사용하고 accepted Stop 뒤 fresh `0x2085`를 금지한다. 더 새 Group Stop/Power
Off는 monitor 중 다음 generation을 예약할 수 있고 이전 proof는 다음 write/publication 또는 최종 UI
적용 전에 폐기된다. 일반 Group
Reset, Axis Reset, Admin `GroupMoveLinearRelative`와 D5
`SubmitSdo`/`CancelOperation`의 지연 ACK도 exact connection session과 priority generation에
  bind되어 drain 뒤 `ResultDiscarded`된다. accepted Submit은 exact ticket/BootId/MapRevision
  evidence를 보존하고 Cancel ACK는 stale success로 적용하지 않는다. current SDK
  Debug/Release runner는 각각 1117/1117 PASS했다. fake-RPC는 외부 Power Off 선점에서
  `0x2085=1`, `0x204B=1`, `0x2045=4`, accepted status failure 뒤 cleanup에서
  `0x2085=1`, `0x2045=4`를 확인했다. 이는 PLC 정지 완료나 장비 안전 성능 증거가 아니다.

Recorder Configure/Start/Adopt 계열은 SDK가 accepted typed result를 파싱한 뒤 safety 선점으로
정상 반환하지 못해도 exact handle/identity/lease를 recovery-only context로 보존한다. WPF의 일반
Recorder UI, Single/Ring/Reconnect/Soak qualification, Double-bank qualification/reconnect recovery는
이 context를 소비한다. SDK 반환 뒤 WPF result-application 검사 전에 safety generation이 바뀌는
별도 race도 callback으로 먼저 로컬 recovery scope에 저장한다. release-only configuration에는
Start 버튼을 열지 않으며, 보존 자원은 Status/Stop/Release 경로로만 정리한다.
SDK accepted-result 계약은 sync/async 12개 회귀로 확인했고 WPF 소비 경로는 Release build로
확인했다. WPF actual-control smoke는 Configure accepted result를 강제로 경합시켜 exact handle을
recovery-only 상태로 보존한 뒤 명시 Release로 정리하는 경로까지 확인한다. Start/Adopt의 동일한
강제 경합 actual-control smoke는 아직 없다.

manual Double Configure의 durable config-only adapter도 구현됐다. source 설정의 모든 필드를
복제하고 recovery Guid에서 만든 nonzero RequestedConfigId만 교체한다. owner/session-bound exact
capability snapshot과 전체 설정을 먼저 검증한 뒤 같은 BootId/MapRevision으로 Configure할 때만
journal을 arm한다. 성공 결과와 accepted-result preemption의 exact lease는 ordinary
`recorderConfiguration`이 아닌 Double recovery scope에 보존하고, Configure 또는 Release 응답
유실은 자동 재전송하지 않는다. config-only exact Release는 full qualification gate가 아닌 manual
route와 같이 열리도록 분리했다. 현재 네 proof/route gate는 모두 `false`다.

`af4ab63` 현행 WPF actual-control smoke는 VS2019 MSBuild current Release에서 335/335
PASS다. 이번 tranche에서는 Debug smoke를 다시 실행하지 않았다. Admin capability/axis/group과
Drive mode/non-atomic status의 exact fake-RPC, non-default axis lookup/AxisInfo payload 및 typed 표시,
CREVIS 자동 7행/3행 표시,
초기 bit 14 OFF 뒤 수동 Load CREVIS 복구, live capability-off 버튼 차단과 capability downgrade
시 이전 LIVE 표본의 `UNAVAILABLE` 전환, 일반 diagnostics RPC 및 exact readback pending 중
SDO draft/editor 유지, required exact Read를 화면에 불러오기 전 draft의 same-session volatile
보존, VERIFIED 뒤 untouched exact editor에서만 자동 복원하며 불러온 뒤의 사용자 편집은 덮지
않는 경로, one-click Inline Read의 typed/raw terminal 표시, pre-accept PC 취소의
zero-submit, accepted timeout/PC 취소의 ticket 및 last-status 보존/수동 Refresh, terminal 실패의
정확한 status 표시와 guard 해제, capability-off zero-wire, 명시적 exact Read 복원과 non-exact zero-wire 차단,
실제 child process의 SDO/DO recovery
zero-replay와 강제종료 재복구, typed v2 SDO record의 capability-off 강제 recovery zero-wire,
D5 contention/timeout/queued-cancel 버튼 gate를 검증한다. bits 14~16 fake RPC의 실제
`0x7E13/0x7E22` health/selected-DI 표시, output-shadow background poll 0회, manual late-response
selection guard, mixed-I/O output proof 보존/해제와 Health/DI channel별 stale/error도 포함한다.
bit 6 + two buffers + 실제 4-entry Recordable
Catalog로 `DoubleContractReady=True`를 만든 상태에서도 live 버튼 disabled, 수동 Double mode
미노출, mode-ambiguous Adopt와 강제 주입 Configure의 Recorder zero-wire를 검증한다. D4
recovery journal은 별도 경로에서 WPF 수명주기와 single-writer lock에 연결되며 active record는
전역 mutation/Close interlock과 exact identity 상태 표시를 만든다. 잠긴 journal은 신규 mutation을
fail-closed하고, 실제 child process 강제 종료/두 번 재시작에서도 `0x7E40..0x7E4F` 자동 replay 없이
byte-identical record를 보존한다. deterministic Guid -> nonzero ConfigId 변환, active journal의
독립 `DoubleRecoveryContractReady=True`와 semantic conflict 뒤 journal usable/runtime I/O failure
분류도 검증한다. Double qualification/recovery/explicit cleanup과 config-only manual Configure
adapter는 MainWindow에 연결됐지만 `ManualActions`, `ManualConfigureRoute`,
`QualificationExecution`, `ReconnectRecovery` 네 gate가 모두 `false`라 실제 runner와 Release
control은 계속 닫혀 있다.
이 runner에서 MainWindow 생성 중 조기 `TextChanged` 전체 갱신과
disconnected window close 재진입도 회귀로 차단한다. 기존 Group/Bulk/Recorder visual/startup
smoke에서는 qualification panel 렌더와 prerequisite 미충족 초기 실행 버튼 disabled를
  확인했다. Axis Stop Begin 1회/status-only Resume과 더 새 Power Off의 monitor 선점 시
  `0x2022` zero-replay, Group Power On/Off의 command 1회 + 3 stable status와 ACK 직후
  child-process Kill/restart zero-replay, 실제 Stop/PowerOff 선점 뒤
  status-only Resume, Axis Reset status-only Resume의 PowerOff 선점 pending 보존, accepted Reset ACK
  뒤 outer-safety preemption의 pending publication, safety preemption 뒤 session cleanup clear 3개,
  수동 Read Status 단독 비완료와 동일 pending Enable proof 누적을 확인한다.
safety 예약은 proof를 즉시 0으로 초기화하면서 ACK와 continuation을 보존하고, 예약 뒤 늦게
도착한 수동 Group Status 응답은 drain 후 `ResultDiscarded`되어 적용되지 않는다. SDK completion
publication 뒤 WPF 적용 전 safety 예약 race만 recovery-required로 승격하며 `0x2047`은 replay하지
않는다. connected unresolved 상태에서는 group 이름 변경, group 재조회, clean connection/window
close, connected reconnect와 새 Power On을 차단한다. 외부 connection loss 뒤 reconnect 진입에서 원
exact group 이름을 보존한 recovery로 승격한다. 명시적 `0x2048` Disable ACK는 Unlock 요청 접수만
뜻하며 pending/recovery를 해제하지 않는다. accepted pending과 recovery-required는 exact group
identity에서 PowerOn=True + Disabled/Unlocked 3회 또는 PowerOn=False 3회 연속 proof가 끝난 뒤에만
해제하며 Power On 성공만으로는 해제되지 않는다. 이전 API
검증 스냅샷의 Debug/Release는 649/649 PASS다. Axis/Group typed lookup과 bounded public inline SDO Read의
typed terminal/pre-wire rejection/terminal-before-cancel/failure/timeout/cancel evidence 9개,
nonterminal `LastObservedStatus` 보존, SDO Write target policy,
operation-kind별 quarantine, 성공 Write 뒤 exact
manual readback interlock, stale-session 물리 확인 recovery와 topology-bound health/I/O
응답 및 raw-output snapshot write 차단, public SDO verification provenance, pinned capability
single-wire health/DI, Catalog/Topology aggregate provenance와 PI Write pre-wire, auto live
monitor, send-priority 8개, RPC lifecycle race 19개, Double-bank retained/release uncertainty,
`0x7E4A..0x7E4D` response-loss 재접속, typed canonical-empty absence, durable v3 Release
crash-window, pending bank/config intent의 exact-target 재사용과 retained ACK-success의
zero-replay durable confirm/resolve 회귀,
PLC core reference model 1개, transport response를 배제한 `LMCRHDR1` semantic header
canonicalization 5개, D5 contention provenance/recovery 12개, D5 timeout exact Expired/drain/recovery 14개,
D5 queued-cancel one-shot/race/quarantine/recovery 10개, D5 disconnect application-recovery/
quarantine 회귀 28개 및
opt-in parser-stress CLI 계약 3개 회귀를
포함한다. D5 runner 포함 current Debug build도 PASS했고 contention/timeout/queued-cancel 및
abrupt-disconnect 버튼의 capability-off, full-capability 및 ordinary in-flight 직렬화 smoke도
PASS했다. abrupt-disconnect full-handler smoke는 fake RPC server의 old/new TCP 두 세션에서
old 세션 `0x405D` 0회, 새 connection 채택, exact recovery ticket 두 개, quarantine clear와
다른 topology revision의 CREVIS 재로딩을 PC에서 확인하지만 실제 PLC owner-loss를 만들지는 않는다. 실제 contention/timeout runner의
PLC live/packet smoke는 대기 중이다.
이 smoke/build는 실제 PLC qualification 실행이나 packet 검증 결과가 아니다. 현재 SDO Write
Axis1 source gate와 fresh LASAL IDE Rebuild/Link는 반영됐지만 PLC download, UI[24] 소유권,
실축 및 EtherCAT mailbox는 검증하지 않았다.

## EtherCAT / PI / Bulk / Recorder 시험 순서

1. Connect 뒤 `Refresh Capabilities`를 먼저 누른다. PLC가 광고하지 않은 기능의
   버튼은 활성화되지 않는다.
   현재 internal test source의 정상 retained 경로는 base `0x0000613F`에 SDO Write bit 9와
   TW[20]/TW[19] bit 18/19를 더한 `CapabilityBits=0x000C633F`,
   `MapRevision=0x957F101E`, nonzero `DiagnosticsBootId`, `MaxSdoDataBytes=4`다. bit 5
   `RecorderTrigger`, bit 8 `SDORead`와 bit 13 `SDOReadGeneralInline`은 활성이고 bit 6 `RecorderDoubleBank` 및
   D5 bit 9 `SDOWrite`는 축 1 gate 때문에 1이고 bit 7 `PIWrite`, bit 12
   `ExtendedSdoResultChunk`는 0이다. bit 14
   `EtherCATTopology`는 활성이고 bit 15 `EtherCATNodeHealth`, bit 16 `DigitalIORead`, bit 17
   `DigitalIOWrite`는 아직 0이다. bit 18/19는 전용 TW[20]/TW[19] source capability다.
   Phase 1 PI Write는 이 capability-off와 별도로 SDK compile-time allowlist가 empty이고,
   WPF도 `Phase1AllowsPiWrite=false`로 입력/button을 끈 뒤 click handler에서 다시 거부한다.
   SDO Write는 PLC와 SDK의 global gate 및 UI[24] axis 1 gate만 `TRUE`, axis 2~4 gate는
   `FALSE`다. SDK approved target은 축 1 Gold UI[24] `0x2F00:24`, Int32/4-byte,
   값 범위 `-1073741823..1073741823` 한 건이며
   축 2~4나 다른 tuple은 GUI/API에서 선택 또는 제출할 수 없다. target과
   Slave/Index/SubIndex/Type/Length/Value draft 필드는 로컬 편집할 수 있지만,
   same-value qualification Write와 manual Submit은 변경 PLC를 Rebuild/Link/download하고 fresh
   bit 9를 확인하기 전까지 비활성이다. manual Submit은 이 조건에 더해 현재
   session의 same-value four-ticket PASS proof가 필요하다. 사용자
   drive program에서 축 1 UI[24]가 실제 미사용인지도 실기 시험 전에 별도로 확인한다.
   BootId 5 축 1~4 capture는 당시 `0x13F`와 `0x1000:0` UInt32 4-byte legacy 경로를
   확인했다. 마지막 BootId 8 `0x213F` capture는 general-inline Int8/1,
   BitField16/2, UInt32/4와 동일 BootId TypeMismatch 후 복구를 확인했다.
2. `Read EtherCAT Health`에서 master state, invalid-cycle counter와 slave 1~4의
   Online/AL/DS402 상태를 확인한다. 이 legacy 표는 네 Elmo 전용이며 CREVIS를 포함하지 않는다.
3. `Connect` 성공 뒤 GUI가 capability와 정적 7-node schema를 자동으로 읽는다. 실패하면 이전
   topology 행을 모두 지우고 capability/BootId/MapRevision과 오류를 같은 영역에 표시한다.
   연결이 끊기면 행과 선택 health/I/O/output shadow를 즉시 폐기하며, 끊긴 session의 늦은
   response도 commit 직전 connection identity 검사에서 거부한다. 이 topology aggregate는
   diagnostics owner/current session에 bind되며 재접속 뒤 재사용할 수 없다. topology-bound
   Health/Digital I/O는 unbound, foreign, stale aggregate를 capability/read RPC 전에 거부한다.
   별도의 detached configured snapshot은 WPF process 안에서 마지막 성공 결과만 보존한다.
   같은 PLC IP/port의 다음 성공 load는 header와 ordered entry의 모든 public field를 SHA-256과
   ordinal equality로 비교해 `INITIAL`, `UNCHANGED`, `CHANGED`와 ordered diff를 표시한다.
   다른 endpoint는 새 `INITIAL`이며, load 실패나 이전 connection의 늦은 response는 이 baseline과
   마지막 성공 evidence를 교체하지 않는다. `Save Configured Evidence`는 disconnect 뒤에도 마지막
   성공 TXT를 UTF-8 no-BOM으로 저장한다. 이 파일은 bit 14 `0x7E11/0x7E12` configured schema
   증거일 뿐 runtime discovery, 실제 cable order, live Online/AL/DS402, DI 또는 physical DO
   feedback 증거가 아니다.
   `Load CREVIS / Topology`는 capability부터 다시 읽어 수동 재시도한다. 현재 응답은 CREVIS coupler,
   input/output slot과 네 Elmo를 상수 schema로 반환하므로 실제 bus 상태나 자동 탐색 결과가 아니다.
   창 제목의 `[CREVIS topology / Axis1 UI24 SDO Write]`와 상단 quick status에서 현재 GUI인지
   먼저 확인한다. `Configured CREVIS entries=3`과 세 `GL_9086_1*` 행을 확인한다. legacy
   Elmo health의 `Legacy slot`은 기존 0..3 wire slot이고 `CFG slave`는 현재 topology의
   Elmo slave 1..4다. bit 15~17이 0인 동안
   선택 node health, digital input/output과 output write 버튼이 비활성인 것은 정상이다.
   `CFG` 열은 이 static schema이고 `LIVE` 열만 동적 sample이다. 기본 선택된
   `Auto refresh live state`는 bit 15 node health 또는 bit 16 selected DI가 있을 때
   owner/session-bound cached capability snapshot을 pinned overload에 전달한다. eligible tick은
   추가 `0x7E00` 없이 `0x7E13` 또는 `0x7E22`를 정확히 1회만 전송해 7개 node와 선택 input을
   순환한다. 일반 non-pinned API의 capability refresh+read 계약은 유지한다. 현재 bit 15/16이 모두 0이므로 자동
   monitor의 wire request는 0회다. background monitor는 output shadow를 읽거나 write용 shadow를
   갱신하지 않는다.
   수동 Health와 DI read도 클릭 시점의 owner/current-session capability snapshot을 pinned
   overload에 전달하므로 data request 앞에 추가 `0x7E00`을 보내지 않는다. Auto/Manual
   Health/DI 결과는 current-session commit gate를 통과한 실제 read attempt만 4,096-entry FIFO
   journal에 기록하며, overflow는 가장 오래된 record를 버리고 dropped count를 증가시킨다.
   failure record에는 이전 성공 sample 값을 복제하지 않는다. `Save Live Evidence`는 retained/dropped
   count와 identity를 포함한 TXT 또는 CSV를 UTF-8 no-BOM으로 저장한다. capability bit 15/16이
   off이면 새 live wire와 새 evidence record가 모두 0이다. 이 export는 PC가 파싱한 PLC response와
   read failure 기록이지 물리 cable order, 실제 DI 접점, physical DO feedback 또는 PLC 구현
   완전성 증거가 아니다.
   current LASAL source에는 `0x7E13/0x7E22` handler, 464-byte snapshot과 CREVIS read-owner가
   있고 fresh IDE Rebuild/Link/static smoke를 통과했다. 다만 bit 15/16은 의도적으로 OFF이고
   PLC download/runtime/actual-hardware proof는 없다. `0x7E23` output write는 구현하지 않았다.
4. `Load PI Catalog`로 현재 map revision과 active PDO signal을 받은 뒤 사용할
   signal의 `Use`를 체크한다. 기본 선택은 네 축의 `actual_position`이다. Catalog도
   owner/current session에 bind되므로 reconnect 뒤 다시 Load해야 하며 alias PI Read, Bulk
   Configure와 PI Write submit은 stale/foreign/unbound Catalog를 wire 전에 거부한다.
5. `Read Selected PI`는 SDO가 아니라 PLC가 publish한 최신 cyclic image를 읽는다.
   Raw Value와 Entry Status를 함께 확인한다.
6. Bulk 탭은 `1 Configure Selected` -> `2 Refresh Status` -> `3 Read Snapshot` ->
   `4 Release` 순서다. Status가 Active인지 확인한 뒤 snapshot을 읽는다. 모든
   entry의 cycle/timestamp는 하나다.
7. Recorder 탭은 선택된 `Recordable` signal, sample period와 capacity로 Recorder를
   configure/start한다. `Single + Manual`은 D3 기본 경로다. 현재 D4 경로는 한 개의
   물리 bank를 사용하는 `Ring + Edge/Window/Mask`이며 RT signal 조건 또는
   `Trigger Now`로 발생한다. 현재 PLC는 bit 6이 0이다. Double qualification, same-session
   retained cleanup, external-session-loss recovery와 config-only manual Configure adapter는
   구현됐지만 네 proof/route gate가 모두
   닫혀 있으므로 수동 Double mode를 목록에 표시하지 않고 Configure handler도 wire 전에
   거부한다. 같은 상태에서는 exact Adopt 대상의 mode도 송신 전에 알 수 없으므로 수동 Adopt
   버튼과 handler를 함께 막는다. Double은 전용 qualification/cleanup/recovery control만 사용하며
   proof gate가 열리기 전에는 이 control도 zero-wire다.
   gate가 열린 뒤에도 same-session cleanup은 third Start exact ResourceBusy 확인이 있어야 한다.
   unexpected third success/ambiguous면 Release하지 말고 disconnect/reconnect exact inventory만
   확인하며, conflicting inventory는 external/manual recovery로 넘긴다. cleanup/recovery 확인은
   preflight 뒤 매 시도 소비되므로 실패 뒤 checkbox를 다시 선택해야 한다.
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

   이 절차는 bit 6이 꺼지고 `RecorderBufferCount=1`인 현재 PLC에만 해당한다. Double
   capability가 광고되면 exact Adopt도 대상 mode를 송신 전에 판별할 수 없으므로 현재 WPF는
   버튼과 handler 양쪽에서 zero-wire로 거부한다. Double용 external-session-loss recovery는
   별도 durable journal과 `Recover Double Journal` adapter로 구현됐으며 수동 Adopt로 우회하지
   않는다. active journal의 recovery capability 계약은 ordinary mutation interlock과 분리되지만
   `ReconnectRecovery=false`인 현재는 inventory/adopt/release를 송신하지 않는다.

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
10. SDO 탭의 활성 general-inline flow는 `Submit SDO Read -> Refresh Ticket` 순서다. 새 PLC
    test build를 download하고 `Refresh Capabilities`에서 bit 8 `SDORead`, bit 13
    `SDOReadGeneralInline`과 `MaxSdoDataBytes=4`를 확인하면 Submit이 활성화된다.
    Slave 1~4, nonzero ObjectIndex, 임의 U8 SubIndex를 입력하고 ValueType에 맞춰
    1-byte(Bool/Int8/UInt8/BitField8), 2-byte(Int16/UInt16/BitField16) 또는
    4-byte(Int32/UInt32/Real32/BitField32) Read를 제출한다. terminal 상태까지
    `Refresh Ticket`을 반복하고 inline 결과를 `Save Result`로 저장할 수 있다.
    8/12-byte Read, `0x7E51` extended result와 PI Write는 계속 비활성이다.
    SDO Write UI/API/PLC source 인프라는 `OperationFlags=1`, exact 36-byte request,
    `ValueType=Int32(4)`, `DataLength=4`, `OperationKind=SDOWrite(3)` 계약으로 준비됐다.
    public immutable SDO Write policy는 cached capability/target snapshot의 blocker matrix를
    wire 없이 평가하고 WPF에 `EVALUATION_WIRE=NONE`, bit 9와 `NoApprovedTarget`을 각각 표시한다.
    현재 SDK allowlist는 축 1 Gold UI[24] `0x2F00:24`, Int32/4-byte,
    값 범위 `-1073741823..1073741823` 한 건이다. target은
    표시되지만 현재 연결 PLC의 bit 9가 0이면 Submit은 `SdoWriteCapabilityMissing`으로 차단된다.
    변경 PLC를 Rebuild/Link/download하고 fresh bit 9와 새 BootId/MapRevision을 확인한 뒤에만
    다음 안전 gate로 진행한다. 축 2~4와 다른 tuple은 계속 승인되지 않는다.
    임의 SDO address와 DS402 motion/control object는 계속 차단된다.

    source gate가 열려 있어도 일반 manual editor의 Write Submit은 바로 열리지 않는다.
    먼저 same-value qualification이 baseline Read, pre-Write guard Read, byte-identical Write 1회,
    guarded exact Readback을 서로 다른 4개 ticket으로 PASS해야 한다. PASS가 만든
    process-local activation proof는 현재 `LMCConnection` reference/session generation,
    `DiagnosticsBuild`, `DiagnosticsBootId`, `MapRevision`과 exact approved target tuple/range에
    귀속된다. 재연결하거나 이 중 하나라도 바뀌면 manual Write는 다시
    `Run Same-Value Qualification First`로 fail-closed한다. mismatch나 disconnect를 한 번
    관측한 proof는 영구 폐기되므로 A -> B -> A로 identity가 돌아와도 다시 활성화되지 않는다.

    이 current-session proof 후의 manual Write submit도 exact SDK target만 선택할 수 있다. WPF는 capability를 다시
    확인하고 `_LMCAxisN`의 `PowerOn=False`, `Standstill=True`, actual position 3회 안정을
    검사한 뒤 첫 클릭에서 target/value/wire bytes의 immutable snapshot만 화면에 arm한다.
    modal 확인창은 없고 편집기는 계속 열린다. 동일 connection/session/BootId/MapRevision과
    byte-identical 요청으로 `Confirm & Submit SDO Write`를 다시 눌러야 두 번째 안전검사,
    journal arm과 실제 전송으로 진행한다. 중간에 요청을 편집하면 이전 snapshot을 즉시 폐기하고
    버튼을 `Arm SDO Write`로 되돌리며, 변경된 요청을 새로 arm한다. same-value qualification은 baseline Read, 실행 전 네 operator
    checkbox 확인 뒤 baseline 값이 바뀌지 않았음을
    확인하는 pre-Write guard Read, 최종 두 번째 안전검사, 동일 4-byte Write 1회, guarded exact
    Readback을 서로 다른 4개 ticket으로 수행한다. 현재 Axis1-only source gate에서는 네 operator
    확인 중 하나라도 없거나 PLC bit 9가 없으면 강제 handler 호출도 zero-wire이며 실제
    PLC/live Write 증거는 아직 없다. activation proof는 PC 세션 admission 증거일 뿐
    PLC download, EtherCAT mailbox completion, pcap 순서나 물리 무변화를 대신 증명하지 않는다.
    actual second-click은 proof의 capability/target을 SDK identity-pinned submit에 넘긴다. SDK는
    mutation 직렬화 구간에서 fresh capability를 다시 읽고 Build/BootId/MapRevision을 exact
    비교하며, drift면 `NotAttempted`/`0x7E50` 0회로 종료한다.
    실제 Submit 진입 시 GUI가 immutable request snapshot을 사용하기 때문에 ordinary in-flight
    Read/Write와 성공 Write 뒤 exact readback pending 중에도 다음 요청 값을 편집할 수 있다.
    capability/allowlist 갱신도 선택 대상과 draft를 자동 적용해 덮지 않는다. 두 번째 submit은
    operation slot이 끝날 때까지 계속 막는다. exact readback interlock은 editor를 고정하지 않고
    admission에서 원 owner/session/BootId/MapRevision과 exact request만 허용한다. `Load Required
    Exact Readback`은 현재 draft를 process-local same-session snapshot으로 보존한 뒤 exact Read를
    불러온다. readback이 VERIFIED되고 editor가 불러온 exact 값 그대로일 때만 이전 draft를
    복원하며, 그 사이 사용자가 한 편집이나 reconnect 뒤의 stale snapshot은 자동 복원하지 않는다.
    submit 직전 operation-kind별 quarantine guard를 등록하며, 결과가 불명확한 Write는
    Read recovery proof로 자동 해제하지 않는다. Write가 `Completed/Success`여도 GUI는 현재
    draft를 보존한다. 운영자가 `Load Required Exact Readback`을 누르면 동일
    Slave/Index/SubIndex/Type/Length의 Read 요청을 로컬 editor에 복원하며, Submit에서 원
    owner/session/BootId/MapRevision을 재검증한다. WPF는 public `LMCSdoWriteVerificationContext`로 accepted Write ticket의 immutable
    submitted request와 supplied request를 flags/target/type/length/timeout/value까지 exact 대조하고,
    같은 owner/session에 bind된 exact `Completed+Success` Write terminal status까지 context 생성에
    요구한다. 그 context의 guarded readback submit과 owner/session-bound fresh capability/Read status
    terminal 판정을 사용하며 capability observation sequence는 context 생성 baseline보다 커야 한다.
    public context도 Axis1-only SDK allowlist를 우회하지 않는다. terminal
    result의 type/length와 4-byte 값이 Write 값과 정확히 일치할 때까지
    다른 mutation과 Close를 차단한다. Stop,
    PowerOff와 기존 resource cleanup은 계속 허용한다. SDO/output Write는 실제 dispatch 전에
    `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\DiagnosticsMutationJournal\v1`에
    process-crash recovery record를 기록하고 accepted/terminal/readback 상태를 원자적으로 갱신한다.
    v2 record는 exact Slave/Object/SubIndex/Type/Length/Timeout/expected bytes를 checksum 범위에
    저장하고 legacy v1은 typed 정보가 없으므로 protocol recovery를 zero-wire로 거부한다. 미해결
    record가 남은 재시작에서는 Write를 재전송하지 않는다. v2 SDO record가 terminal success이고
    SDK allowlist의 exact target과 일치할 때만 운영자가 read-only recovery를 한 번 실행할 수 있으며,
    Read 전후의 fresh BootId/MapRevision과 exact bytes가 일치하고 같은 record/state의 atomic
    CAS가 성공하면 durable tombstone을 먼저 쓴다. 그 외 record는
    물리 확인과 명시적 recovery acknowledgement로만 해소한다. current-process digital
    output 결과 불명 ACK는 별도 checkbox에서 물리 출력과 PLC output
    shadow를 독립 확인한 뒤 경고 확인창에서도 승인해야 한다. 새 write나 shadow/tuple/selection
    변경은 checkbox를 지우며, ACK는 GUI interlock 해제일 뿐 이전 write 성공 증거가 아니다.
    PC test child의 안정 지점 강제 종료 뒤 journal reopen과 armed interlock 보존, 실제 WPF
    process의 SDO/DO unresolved startup·zero-replay·single-writer·Close 차단·강제종료 재복구는
    자동 회귀를 통과했다. 실제 전원 손실 durability와 장비에서의 물리 확인/ACK 절차는
    production release gate로 남아 있다.
    journal open/runtime fault, checksum
    corruption 또는 두 번째 app writer가 확인되면 새 live/mutation command를 차단한다.
    tracked D5 read도 차단하며, 일반 non-D5 read-only inspection, Stop, PowerOff,
    Group Stop은 계속 허용한다. active durable evidence가 없을 때만 정상 종료를 허용하고,
    active evidence가 남아 있으면 connection/window Close도 해소 전까지 차단한다.
    이 분류는 UI 독립 `DiagnosticsOperationAdmissionPolicy`의 immutable state/decision으로
    통합했다. 일반 tracked D5 submit, live/mutation, Connect/Reconnect, connection/window Close와
    정상 qualification의 UI 및 handler가 같은 decision을 다시 평가한다. 성공 Write 뒤 필수 exact
    SDO readback은 pending 존재, 다른 D5/DO 미해결 없음, operation slot, 원 connection session을
    모두 만족할 때만 일반 interlock을 좁게 우회한다. fresh BootId/MapRevision 검증과 restart
    recovery의 post-read identity/CAS는 계속 SDK/WPF core에서 수행하며, durable journal resolve가
    성공하기 전에는 volatile pending을 지우지 않는다. 기존 D5 cleanup, 일반 non-D5 read-only와
    safety command 예외는 유지한다.
    단, Connect 뒤 확인된 recovery record의 recovery identity가 현재 PLC와
    다르면 연결을 끊지 않고 connection-scoped read-only quarantine으로 전환한다. 이 상태에서는
    Axis/Group의 일반 non-D5 info/status/position/member 조회와 로컬 engineering/identity/SDO draft
    편집, Close/Exit를 허용한다. 조회는 매번 transient handle을 사용하며 application control handle을
    보존하지 않는다. RPC/function read가 성공했다면 native `AxisErrorId`/`GroupErrorId`가 0이 아니어도
    진단 결과에 그대로 표시한다. 축/그룹 Stop·PowerOff를 포함한 모든 control, tracked D5 read/submit,
    resource cleanup, mutation, qualification은 wire 전에 차단한다.
    stale record를 현재 PLC 상태로 판정하거나 자동 replay/resolve/replace하지 않으며, quarantine
    연결을 닫아도 durable record의 state와 identity는 그대로 보존한다.
    현재 endpoint는 같지만 active Axis Power, Axis Stop/Reset, Motion, Group Profile Lock,
    Group Power 또는 Group Reset record의 identity가 현재 PLC와 다른 경우에만
    `Archive and Retire Stale Recovery` 절차를 사용할 수 있다. 운영자가 장비와 드라이브의 물리
    상태를 독립 확인했다는 checkbox와 경고 확인창을 모두 승인해야 한다. 확인 전후에 read-only
    Capabilities를 다시 읽어 동일 connection/session/endpoint와 nonzero BootId/MapRevision,
    더 최신 observation 및 byte-identical full active journal vector를 재검증한다. 현재
    endpoint에 속하며 current PLC identity와 다른 record만 `RETIRE STALE`로 commit/resolve한다.
    same-endpoint exact-current record는 `KEEP EXACT CURRENT`로 남겨 새 process에서 exact
    status-only recovery를 먼저 완료하고, 다른 endpoint record는 `KEEP OTHER ENDPOINT`로
    보존한다. journal/ledger fault 또는 실행 중 operation이 있으면 아무 record도
    폐기하지 않는다.
    기존 다섯 record는 BootId/MapRevision을 비교하고, nonzero `DiagnosticsBuild`를 저장하는 Group
    Reset record는 Build/BootId/MapRevision을 모두 비교하므로 Build-only mismatch도 retire할 수
    있다. Group Reset record가 있을 때는 confirmation 전후 current Build도 nonzero/exact여야 한다.
    승인된 stale record는 원 journal 전체 바이트와 SHA-256, 운영자/현재 PLC identity를 먼저
    `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\RecoveryRecordRetirementLedger\v1`에
    immutable entry로 저장한다. format 2 entry는 source/current DiagnosticsBuild를 보존하고 기존
    format 1 entry도 읽는다. temp flush 뒤 Windows write-through rename과 최종 byte/hash 검증을
    통과한 record만 원 journal의 full-byte CAS로 `Resolved` 처리한다. 이전 명령의 결과는 계속
    `UNKNOWN`이며 이 절차는 Motion, Power, SDO, Write, replay 또는 cleanup을 보내지 않는다.
    Diagnostics mutation journal과 Recorder double journal은 이 절차의 폐기 대상이 아니다.
    성공하면 quarantine connection을 닫고 앱을 종료하며 같은 process의 reconnect를 금지한다.
    새 앱에서 reconnect해야 Motion/Power admission을 다시 평가할 수 있다. Axis 1 SDO Write
    source/PC gate와 변경 LASAL 프로젝트의 Rebuild/Link·exact-method
    Implementation-tab/header smoke는 PASS했다.
    실제 전송은 current PLC download와 fresh bit 9 확인 뒤에만 열리며, 아직 PLC download,
    실축 또는 packet capture 합격 증거는 없다.
    축 1~4 `0x1000:0` UInt32 4-byte legacy와 general-inline 1/2/4-byte Read의
    PC-PLC ticket/inline success를 확인했다. `12_SDO_GeneralInline_4Byte_FailureRecovery`
    에서는 TypeMismatch 실패 뒤 같은 BootId 복구도 PASS했다. read-only abort -> recovery
    qualification은 code/build와 analyzer test만 완료했다. deliberate contention -> exact
    `ResourceBusy` -> recovery, timeout -> late-callback drain -> recovery와 queued-only
    Cancel -> recovery runner도 code/build/test까지만 구현 범위다. abrupt disconnect -> distinct
    new connection -> two-ticket application recovery WPF adapter도 구현됐지만
    `orphanQualified=false`로 고정한다. 실제 abort/pcap, offline, timeout, queued cancel,
    PLC orphan lifecycle witness, contention 실행과 EtherCAT mailbox frame 독립
    관측은 production qualification으로 남아 있다.

## Home / Encoder Maintenance 현재 계약

- Single Axis의 `LMC Home - Current Position Zero`는 Admin `0x7D13` one-shot start,
  `0x7D18` retained outcome query와 `0x7D19` exact terminal retirement를 사용한다.
  기존 switch-search `ReferenceAxis` 의미가 아니며 motion enable, Home switch 또는 limit
  switch 탐색 없이 현재 actual position을 stale-read guard로 고정하고 target 0을 요청한다.
  Admin feature bit 4는 current source에서 ON이고 WPF의 `Execute LMC Home Once`와
  `Read Home Status (no replay)` control이 활성 대상이다.
- Start ACK와 `Read Home Status` RPC PASS 자체는 완료 증거가 아니다. terminal outcome의
  `HomeSucceeded`와 exact retirement를 확인해야 한다. WPF의 `LMC Home outcome:` 로그는
  `RecordState`, `HomeSucceeded`, original status/error/detail, axis status/error,
  raw drive before/after, application/internal actual/set/destination/master,
  `NativeCommandState`, `EvidenceFlags`, `StopState`, `RuntimePhase`, `RecordGeneration`을
  한 줄로 기록한다. current source의 임시 SetPosition-only mode는 실제 raw before/after를
  그대로 기록하지만 delta를 성공 조건으로 사용하지 않으며 `EvidenceFlags=0x3B`로 이를
  구분한다. 축별 native `SetPosition`은 정확히 한 번만 호출하고, Standstill/AxisError와
  application/internal 좌표 6개의 zero 상태는 3회 확인한다. 기존 raw-qualified
  `EvidenceFlags=0x3F`와 wrap-safe `+/-2 count` gate는 임시 source에서 제거됐으며 원복은
  변경 이력으로 수행한다. `StopState`는 legacy wire 이름이며 이 경로에서는 retained failure
  code다. 별도 Stop 명령이 실행됐다는 뜻이 아니다.
- 별도 `DS402 Home`은 `0x7D15/0x7D16/0x7D17`, method 37, Home offset 0의
  non-moving current-position-zero source가 있지만 `LMC_DIAG_DS402_HOME_ENABLED=FALSE`,
  Admin feature bit 6 OFF다. valid Start 전용 admission detail 41/42 분리와 owner release 뒤 fresh
  terminal evidence, stage 87/88/89 warm reconcile, rollback-complete receipt와 bit-4 safety drain은
  source에 반영됐다. cleanup stage `90..99`는 1초 뒤 fail-closed quarantine로 제한한다. 다만
  quarantine ownership publication 결과를 소비하지 않는 cleanup caller와 cold-restart/runtime matrix가
  남아 있으므로 WPF control이 존재해도 current runtime 실행 가능을 뜻하지 않는다.
- `TEST ONLY - Encoder Maintenance`의 `0x7E53/0x7E54/0x7E55` source는 활성이다.
  TW[20]은 `0x20FC:0x02 <- UInt16 1`, TW[19]는 `0x20FC:0x01 <- UInt16 1`로 고정된다.
  start ACK, terminal outcome과 retirement는 분리되며 terminal RPC 결과만으로 drive의 정확한
  error/warning 또는 multi-turn position 변화가 증명되지는 않는다.
- 이전 BootId의 Axis1 raw `8028436 -> 8028440`은 downloaded raw gate에서 `-7` quarantine을
  만들었고 후속 축 admission도 막았다. 그 checkpoint는 현재 runtime 판정으로 사용하지 않는다.
  `0x3B` 임시 mode는 C78 `0 errors / 55 warnings`, canonical download와 새 BootId `0x1B`에서
  Axis1..4 연속 terminal `Succeeded`, exact retirement, generation `1 -> 4`와 Group Identity Home
  Check `4/4`까지 PASS했다. raw delta는 각각 `0/0/+1/+1`이며 성공 gate가 아니다. 이 결과는
  temporary SetPosition-only Home의 runtime proof이지만 actual in-motion Stop, rebase word의
  restart/power-loss retention 또는 DS402 Home/TW19/TW20 physical effect까지 증명하지 않는다.

## Read-only API 시험 순서

이 탭은 Phase 1의 신규 읽기 API를 실물 PLC에서 확인하기 위한 화면이다. motion이나
write command는 없다. `0x7D12 SetAxisPosition`은 SDK contract와 fail-closed PLC parser만
있고 capability bit 3이 OFF이므로 화면에 노출하지 않는다. SetPosition은 diagnostics
identity와 128-bit intent를 기록하는 독립 durable journal
core 및 SDK `0x7D14` exact read-only query 계약까지만 추가했다. current PLC의 bit 5,
retained store/query route/terminal retirement와 MainWindow dispatch/interlock 연결은 없다.
이들을 unified ownership과 같은 slice에서 연결한 뒤에만 활성화를 검토한다. SetPosition의
authoritative max-jump, 공통 task/core/priority 및 PLC proof가 먼저다. LMC Home과 DS402
Home/encoder-maintenance 상태는 위 별도 절을 따른다. `0x7D00/0x7D10/0x7D20`과 physical axis 1~4 drive read의
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
   `0x6061:0`을 순서대로 읽고 실제 `0x6041` bit 3의 `DS402Fault`를 표시한다.
   `3 Get Drive Error Code`는 별도 D5 ticket으로 `0x603F:0 UInt16/2`를 한 번 읽는다.
5. Drive Status는 같은 EtherCAT cycle의 atomic snapshot이 아니다. LASAL position-limit,
   axis error flag, DS402 Fault/internal-limit bit와 `0x603F`는 서로 다른 출처이므로 한
   값으로 합쳐 원인을 추정하지 말고 화면에 표시된 각 source를 따로 확인한다.
   `0x2028 StatusWord`는 current LASAL의 reserved 0이며 DS402 Fault 값이 아니다.

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
- Group Power On/Off는 각각 `BeginGroupPowerOnWaitForStableStateAsync`와
  `BeginGroupPowerOffWaitForStableStateAsync`로 `0x204A`/`0x204B`를 한 번만 보낸다. 두 ACK 모두
  mode-change 시작 접수일 뿐 최종 상태가 아니다. `ResumeGroupPowerStateWaitForStableStateAsync`는
  accepted continuation으로 `0x2045`만 poll해 각각 `PowerOn=True`/`PowerOn=False`를 기본 3회 연속
  확인한다. timeout/cancel/status 오류와 typed interference 뒤에도 continuation/evidence를 보존하고
  command를 자동 replay하지 않는다. 재시작 뒤 durable Accepted는 exact identity status-only,
  uncertain Power On은 explicit Power Off takeover, uncertain Power Off는 status-only false 확인을
  먼저 요구한다. 일반 Group Read Status 한 번만으로 pending Power 또는 Enable transition을 완료하지
  않는다. 다만 safety generation 검증을 통과한 성공 응답은 상태에 맞는 pending Enable continuation
  proof에 누적되고 Locked Standby proof가 3/3이면 기존 ACK를 재사용한 zero-wire Resume으로 완료할 수
  있다.
- Group Read Status의 `0x00040000`은 Power Ready, `0x00010000`은
  Disabled/Unlocked, `0x00020000`은 Enabled/Locked Standby로 표시한다.
- Group Enable/Disable은 robot power 명령이 아니라 configured profile의
  Lock/Unlock 명령이다. Enable ACK 뒤 같은 connection/session/group-reference에서
  `Read Status`의 PowerOn + `Enabled/LockedStandby=True`가 3회 연속이어야 Move가 활성화된다.
  accepted 뒤 timeout/cancel/status 실패는 continuation을 보존하며 Resume은 status만 poll한다.
  Stop/PowerOff safety 예약은 누적 status proof를 즉시 초기화하되 accepted ACK와 pending
  continuation을 보존한다. 예약 뒤 도착한 status response는 drain 후 `ResultDiscarded`되어
  observe되지 않는다. 예약 전에 SDK completion publication이 끝났지만 WPF 적용 전에 safety가
  예약된 좁은 경우만 recovery-required로 승격한다. connected unresolved 상태에서는 group 이름
  변경, group 재조회, clean connection/window close, connected reconnect와 새 Power On을 차단한다.
  외부 connection loss 뒤 reconnect 진입에서는 원 exact group 이름을 보존한 recovery로 승격하고
  새 session에서 그 이름의 group만 다시 조회한다. 명시적 `0x2048 GroupDisable` ACK는 Unlock 요청
  접수만 뜻하며 pending/recovery를 해제하지 않는다. accepted pending과 recovery-required는 exact
  group identity에서 PowerOn=True + Disabled/Unlocked 3회 연속 또는 PowerOn=False 3회 연속
  proof가 끝난 뒤에만 해제한다. Power On
  성공만으로는 해제되지 않으며 어느 경로도 `0x2047`을 replay하지 않는다. Disable은 Stop이 아니며,
  PLC는 group in-position이 확인되지 않으면 unlock을 `-6`으로 거부한다.
- fresh Group Enable은 endpoint IP/port, group name/reference, DiagnosticsBootId와 MapRevision을
  별도 durable journal에 먼저 기록한 뒤 `0x2047`을 송신한다. 시작 시 남은
  `ArmedBeforeDispatch`는 `RecoveryRequired`로 승격하고 endpoint mismatch는 TCP/RPC 전에,
  group reference mismatch는 read-only lookup 뒤 mutation 전에 차단한다. verified Enable,
  명시적 Disable 또는 stable Power Off proof는 fresh identity와 post-identity safety generation을
  다시 확인한 뒤 durable `Resolved`를 먼저 기록한다. active record 동안 endpoint/group 편집,
  Group Reset/Set Identity와 clean Close를 차단하며 자동 `0x2047` replay는 없다.
- Group Profile Lock journal은 기존 상태 값 1~3을 바꾸지 않고
  `AcceptedAwaitingProof=4`를 추가했으므로 기존 format-version 1 record와 호환된다.
  SDK `BeginGroupEnableWaitForLockedStandbyAsync`의 accepted observer는 ACK와 exact
  continuation publication 뒤, 첫 `0x2045` 전에 이 상태를 durable하게 기록한다. 새 process에는
  process-local continuation이 없으므로 exact endpoint IP/port, group name/reference,
  DiagnosticsBootId와 MapRevision을 모두 다시 확인한 뒤 public
  `WaitForLockedStandbyAsync`로 `0x2045`만 3회 연속 확인한다. 실제 child-process Kill/restart
  smoke는 journal single-writer lock 재획득, 새 session의 `0x2047` 0회와 `0x2045` 3회,
  동일 identity의 `Resolved`를 확인했다. 이는 PC/fake-RPC 증거이며 PLC profile lock 또는
  장비 동작 증거가 아니다.
- fresh Group Disable도 같은 방향성 journal에 `ExpectedProfileLocked=false`로 wire 전에 arm한다.
  SDK accepted observer는 `0x2048` ACK와 exact continuation publication 뒤 첫 `0x2045` 전에
  `AcceptedAwaitingProof`를 durable하게 기록한다. 같은 process는 continuation Resume, 새 process는
  exact endpoint/group/reference/BootId/MapRevision 확인 뒤 `WaitForStableDisabledAsync`만 사용하며
  둘 다 `0x2045`의 PowerOn + Disabled + !Standby를 기본 3회 연속 확인한다. 실제 child-process
  Kill/restart smoke는 새 session의 `0x2048` 0회, `0x2045` 3회, journal lock 재획득과 동일 identity
  `Resolved`를 확인했다. stable PowerOff는 더 새로운 safety proof로 pending Disable을 retire하지만
  Disable 완료로 보고하지 않는다. `0x2048` NACK도 Unlock side effect 가능성을 배제할 수 없으므로
  recovery를 유지한다.
- accepted status-only 복구가 끝나도 process-local Set Identity/Home Check는 복원하지 않는다.
  따라서 Move는 계속 fail-closed하며 사용자가 Disable 후 Home Check와 Set Identity를 다시 수행해야
  한다. `ArmedBeforeDispatch`는 accepted로 간주하지 않고 기존처럼 `RecoveryRequired`로 승격해
  Disable 또는 stable Power Off 같은 safety-only 복구만 허용한다.
- Read Status 자체가 실패하면 화면의 Power Ready와 lock 판정을 무효화하고, 진행 중이던
  lock 확인은 보존한다.
  성공한 Read Status로 상태를 새로 읽기 전에는 Power On과 Move를 허용하지 않는다.
  이후 단일 `PowerOn=False`가 관찰되면 identity와 lock 준비 상태는 지우지만 recovery-required를
  해제하려면 보존한 exact recovery group의 성공 응답 3회 연속 proof가 필요하다.
- raw Group Reset은 axis/hardware error dispatch ACK이며 profile error 전체 reset이 아니다.
  WPF Group Reset 버튼은 `0x20D2` observed snapshot을 고정하고 `0x2049`를 한 번만 보낸 뒤,
  `0x2045` + pinned member별 `0x2028` full-clear를 기본 3회 연속 확인한다. timeout/cancel/status
  failure 뒤 버튼은 status-only Resume이며 Reset을 replay하지 않는다. accepted Reset 즉시 cached
  Power/Identity/Home/Profile readiness를 무효화하고 proof 성공 뒤에도 복원하지 않는다.
  exact pending 또는 submission-outcome-uncertain 중에는 새 Reset/PowerOn/Enable/SetKin/Move,
  mutation qualification, reconnect와 Close를 차단하고 read-only inspection, Stop, PowerOff,
  safe Disable만 허용한다. command 직전 prepared observer는 exact endpoint, DiagnosticsBuild/BootId/
  MapRevision, group/ref, old session, ordered members와 stable count를 durable journal에
  `ArmedBeforeDispatch`로 저장하고 ACK 뒤 `AcceptedAwaitingProof`로 바꾼다. disconnect/restart는 record를
  `RecoveryRequired`로 승격한다. exact reconnect와 Load Group 뒤 SDK durable attach가 fresh `0x20D2`
  count/order/name/reference/device를 1회 검증한 경우에만 status-only Resume을 열며 `0x2049`를
  replay하지 않는다. mismatch/corruption은 record를 유지하고 fail-closed한다.
  accepted/outcome-uncertain safety takeover는 terminal supersede이고 valid safety NACK는 Reset
  continuation을 보존한다. captured-member Axis Stop/PowerOff는 accepted 또는 outcome-uncertain일 때
  `SupersedePendingGroupResetAfterCapturedMemberSafetyMutation`으로 SDK pending도 exact terminalize하고,
  valid NACK rollback은 false로 보존한다. Group Stop ACK도 정지 완료가 아니므로 stable status를 확인한다.
- Axis Power On 버튼은 `0x2023` success ACK를 완료로 표시하지 않는다. accepted continuation을
  먼저 journal에 보존하고 `0x2028 PowerOn=true` 3회 연속 뒤에만 verified로 전환한다. timeout,
  취소, reconnect와 재시작 복구는 status-only이며 Power On을 자동 재송신하지 않는다.
- Axis Power Off 버튼도 command 전에 방향성 journal을 arm하고 ACK를 첫 status 전에 durable
  `AcceptedAwaitingProof`로 보존한다. 앱이 ACK 직후 강제 종료돼도 재시작은 동일 identity를
  read-only로 확인한 뒤 `0x2028` 3회만 사용하며 `0x2023(false)`를 재송신하지 않는다. 실제
  child-process Kill 회귀는 종료 전 journal lock, 종료 뒤 lock 재획득과 `Resolved`까지 확인한다.
- Axis Stop 버튼은 `0x2022` ACK를 accepted-once continuation으로 보존한 뒤 `0x2028`만 재개해
  successful Standstill 3회 연속을 확인한다. accepted Stop monitor 중 동일 Stop 버튼은 replay를
  막기 위해 비활성화하고 Power Off만 계속 허용한다. 선점된 monitor의 실패를 정지 실패나 성공으로
  추정하지 않는다.
- Axis Reset 버튼은 Begin에서 `0x2024`를 한 번만 보내고 accepted continuation을 gate 반환 전에
  저장한다. 이후 Resume은 `0x2028`의 successful `AxisErrorId == 0`을 기본 3회 연속 확인한다.
  failure/preemption 뒤에는 status-only Resume만 허용하고, typed confirmed interference 뒤에만
  사용자의 명시적 새 Reset을 허용한다. 화면은 submission outcome, 명령 전송 가능성, 마지막
  status, poll/stable count와 expected/observed mutation generation을 남긴다. 이는 LASAL
  AxisErrorId-clear 관찰이며 DS402 Fault bit나 drive error register 해제 증거가 아니다.
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
- 같은 탭의 live qualification runner는 사용자가 별도로 확인한 한 축에 대해서만
  Power On -> Relative Move -> Stop -> Power Off 전체 accepted-once/safe-cleanup 경로를 실행한다.
  실행 전 세 안전 확인이 모두 필요하며 raw delta는 nonzero, 절대값 최대 1,000,000이고 첫
  live slice의 Jerk는 0이다. PASS 뒤 packet capture를 최소 2초 더 유지한다.
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
- Close와 창 닫기는 Stop이 아니다. motion 가능성이 남으면 둘 다 차단되며 자동
  Stop을 보내지 않는다.
- Power On, Reset, motion과 Group Power/Configure/Lock 명령은 체크박스나
  확인창 없이 버튼 클릭 시 즉시 송신된다.
- 모든 Axis/Group Move는 wire 송신 직전에 remote endpoint, target name/reference,
  DiagnosticsBootId와 MapRevision을 fresh capability RPC로 다시 확인한다. target lookup
  시점의 identity와 정확히 일치할 때만 durable motion journal을 먼저 기록하고 Move를
  한 번 송신한다. Move ACK는 완료가 아니며 timeout, 응답 유실, disconnect 또는 강제
  종료 뒤 Move를 자동 replay하지 않는다.
- 재시작 복구는 journal에 기록된 endpoint로만 연결하고 동일 BootId/MapRevision과
  동일 Axis/Group reference를 read-only로 확인한 뒤 Stop 또는 Power Off를 실행한다.
  startup 또는 불명확한 Move 결과는 status-only 표본으로 해제할 수 없으며, exact identity를
  fresh 확인한 Stop/Power Off의 정상 ACK가 먼저 필요하다. Axis Power Off는
  `PowerOn=False && Standstill=True`가 연속 3회여야 한다. stable safe-state proof 뒤에도
  BootId/MapRevision을 다시 읽어 durable record와 일치할 때만 `Resolved`를 기록한다.
  이 최종 identity가 drift하거나 RPC가 실패하면 explicit safety 명령을 다시 요구하고,
  그 전까지 새 Move, 다른 target lookup, 일반 Close와 창 닫기를 계속 차단한다.
- motion/제어 command의 실행 중 Cancel 기능은 제공하지 않는다. API timeout은 기본
  3초다. Recorder의 `Cancel Download`는 이미 frozen된 데이터를 PC로 복사하는 작업만
  취소하며 PLC recording이나 motion을 정지시키지 않는다.
- callback version 2는 D5 terminal query를 깨우는 hint일 뿐 완료 판정이 아니다. exact
  retained ticket과 current-session provenance가 없는 wake는 폐기하고, authoritative
  `0x7E03` TCP response만 UI/journal 상태를 바꾼다. current source에는 네 terminal
  state의 one-attempt receipt와 production-path candidate `PublishEvent(...)` call을 가진
  Gate D `TerminalWakeBrokerCandidate`가 있다. 그러나 `ProductionApproved=false`,
  `NeedsRebaseline=true`이고 live UDP packet 증거도 없으므로 runtime callback PASS가 아니다.

활성 command mapping은 `API_MAPPING.md`, 구현 판단과 안전 설계는 `DESIGN.md`를
참조한다.

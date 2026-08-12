# RPC 초기화와 UDP Callback 구현 기록

작성일: 2026-07-10

P0 endpoint ownership/session provenance 갱신: 2026-07-31

Version 2 wake-hint transport 및 D5 의미 계약 갱신: 2026-08-08

PC reconnect evidence 및 callback receiver observability 갱신: 2026-08-10

WPF bounded fresh-session recovery 갱신 (`14ccf58`): 2026-08-11

실제 EXE 종료/재실행 gate 갱신 (`cbf2548`): 2026-08-11

PowerShell 5.1 verifier compatibility 갱신 (`ad4af91`): 2026-08-11

PLC owner-loss callback retirement 계약 갱신: 2026-08-12

## 결론

RPC 연결은 단순 TCP connect가 아니다. 현재 사용 순서는 아래와 같다.

```text
PC                                      LASAL PLC TCP :4000
 |--- TCP connect -------------------------->|
 |--- 0x8080 Session Init ----------------->|
 |<-- 32-byte init response ----------------|
 |    PC UDP listener open                  |
 |--- 0x405C mask + UDP port + PC IP ------>|
 |<-- 12-byte ACK --------------------------|
 |--- axis/group command ------------------>|
 |--- 0x405D Close ------------------------>|
 |<-- 12-byte ACK --------------------------|
 |    TCP/UDP close                         |
```

Callback transport는 UDP다. Maestro 매뉴얼은 connection parameter의
`uiCbUdpPort`와 callback function을 UDP로 명시한다. 현재 PC library는
기본값인 legacy 12-byte registration/raw delivery와 명시적 opt-in인 version 2
32-byte registration/strict typed wake delivery를 분리한다. Version 2 UDP
datagram은 52-byte `LMC2` envelope로 고정돼 있다. 다만 target PLC에서 생산
publisher를 실행한 packet capture는 아직 없으므로 이 문서는 static/build
계약을 live wire 증거로 취급하지 않는다.

## 패킷 계약

### `0x8080` Session Init

요청은 9 bytes다.

```text
80 80 00 00 01 00 00 00 00
```

정상 응답은 32 bytes다.

```text
00 00 18 00 00 00 00 00
40 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00
```

payload 첫 DWORD `64`는 캡처에서 확인된 값이지만 의미는 확정되지 않았다.
따라서 현재 LASAL 구현은 캡처값을 반환하되 이를 공개 SessionId로
해석하지 않는다. 세션 식별은 TCP callback의 `dSock`을 사용한다.

PLC가 session init을 거부할 때 확인된 exact short failure는 12 bytes다.

```text
01 00 04 00 00 00 00 00 01 00 FF FF
```

outer `Status=1`, `HeaderReserved=0`, payload length `4`, command `Status=1`,
`ErrorId=-1`이다. SDK는 `0x8080` 응답도 `ParseAcknowledgement`로 파싱해 이
command error를 보존한다. 이전 일반 parser 경로는 short ACK의 `ErrorId=-1`을
읽지 않아 예외에 `ErrorId=0`을 표시했다.

Commit `66b5cf2`의 reconnect correction은 명시적 `Version2WakeHint`에서만 frame
valid, outer `Status=1`,
`HeaderReserved=0`, payload length `4`, command `Status=1`, `ErrorId=-1`인 exact
envelope에 한해 20 ms cancellation-aware 대기 뒤 같은 TCP socket으로 `0x8080`을
한 번 더 전송한다. 두 번째도 실패하면 해당 SDK connection instance가 연결/listener를
정리하고 `Faulted`로 끝난다. legacy mode, 다른 ErrorId, nonzero reserved, malformed frame은 재시도하지
않고 기존 실패/cleanup 경로를 따른다. 이 bounded PC recovery는 callback sender
준비 직후의 일시 상태만 흡수하며, PLC에서 계속 발생하는 disarm result `-8`/`-9`의
원인을 고치거나 숨기는 동작이 아니다. 일반 `0x8080`/`0x405D`와 transport/safety
경로의 negative disarm은 callback tuple을 보존하고 같은 fence를 재시도하는
fail-closed 계약을 유지한다. 별도의 PLC owner-loss retirement는 아래의 확정된 두
socket-owner 경로에서 exact `-8`만 처리하며, PC retry가 그 동작을 대신하지 않는다.

Commit `af4ab63`의 historical WPF 회귀는 나머지 필드는 같고 `ErrorId=0`인
non-canonical short ACK가 canonical retry를 사용하지 않고 `0x8080` 1회에서 끝나는
것과, cleanup 뒤 다음 수동 Connect가 새 TCP socket/session을 쓰는 것을 고정한다.

Policy commit `14ccf58`은 초기 및 동일 프로세스 내 후속 Connect의 첫 candidate에서 두 exact canonical
`-1` ACK가 모두 돌아와 `Outcome=Failed`, `AttemptCount=2`,
`CanonicalRetryUsed=true`이고 RPC/callback registration이 시작되지 않은 경우만 failed
`LMCConnection`을 retire/`Dispose`한다. fixed 100 ms 뒤 새 `LMCConnection`/TCP를 정확히
한 번 열며 두 번째 candidate 실패는 terminal이다. `ErrorId=0`, 다른 ErrorId,
malformed/transport/cancellation 또는 callback-stage failure에는 WPF outer retry가 없다.
한 UI Connect의 상한은 TCP 2개/`0x8080` 4회이고 `0x405C`는 init 성공 뒤에만 나간다.
정상 registration ACK까지 받아야 Connect가 성공하며 `0x405C` 실패는 terminal이고
WPF outer retry가 없다.

내부 replacement와 창 X는 공용 최대 2회 `Dispose` cleanup 뒤 complete local
disconnected postcondition을 요구한다. 이전 `RpcCloseResponse`와 `LastCloseException`은
보존한다. X는 postcondition 미완료 시 종료를 취소하고 명시적 Close 버튼은 local cleanup
뒤에도 close 오류를 throw한다. startup identity는
`ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V1`, `SdkPath`, `SdkBuildUtc`를 기록하고 topology
marker V5를 유지한다.

100 ms는 PC fixed backoff이지 PLC readiness/timing proof가 아니다. canonical wire `-1`은
internal disarm `-8`/`-9` 또는 다른 lifecycle/ownership rejection일 수 있다. Historical
fake restart는 같은 test process의 새 `MainWindow`를 쓰는 별도 회귀로 유지한다.

현재 LASAL source의 owner-loss retirement는 `LMCUdpCallbackSender.DisarmEndpoint`의
exact internal sentinel `(ExpectedSessionEpoch, ExpectedCookieLo,
ExpectedCookieHi)=(0,0,0)`을 사용한다. 유효 endpoint는 zero epoch와 양 cookie zero를
허용하지 않는다. `TCPMotionInterface`는 먼저 저장된 nonzero fence로 일반 disarm을
시도하고 결과가 정확히 `-8`일 때만, accepted owner transition 또는
`CurrentSock=dSock`인 definitive disconnect에서 sentinel을 한 번 호출한다. Sentinel
결과가 `0` 또는 `1`일 때만 일반 helper를 다시 호출해 local tuple을 정리한다.
`-9`, 다른/미확인 peer candidate 거절, failed takeover, 새 owner 게시 뒤 도착한 old
retiring socket disconnect에서는 sentinel을 호출하지 않는다. 이 source 변경에 대한
LASAL IDE build, PLC download와 PLC runtime 재접속 검증은 2026-08-12 현재 실행하지
않았다.

Current `cbf2548` actual-EXE relaunch gate는 Debug/Release 각각 `1/1` PASS했다. Parent
runner가 supplied actual example EXE의 PID/HWND에 외부 `WM_SYSCOMMAND/SC_CLOSE`를 보내 첫
process의 X close를 실행한다. 그 process는 `0x405D` exact `-1` ACK 뒤 bounded local
cleanup과 exit를 완료한다. live owner 중 contender는 default named mutex에서 exit `2`, TCP
session `0`이고, owner exit 뒤 같은 exact EXE successor는 mutex를 재획득한다. Successor의
첫 TCP candidate는 `0x8080` exact `-1` 두 번 뒤 `0x405C/0x405D` 없이 폐기되고 fresh
candidate가 init/registration을 성공한다. 전체 exact fake wire는 session/request
`3/28 (13,2,13)`이다. malformed probe는 mutex/journal/network 전에 exit `64`, owned temp
write `0`, TCP session `0`이다. EXE, SDK DLL과 optional config의 path/length/SHA-256도
시험 전후 동일하다.

이 결과는 PC loopback process/default-mutex/local-cleanup/wire 증거다. PLC
cleanup/disarm/readiness, fixed 100 ms의 PLC 적정성, MotionLib/축 상태 또는 사용자 PLC의
실제 종료 후 재접속을 증명하지 않는다. PC cleanup은 PLC disarm 성공이 아니며 private PLC
state를 force-clear하지 않는다.

Distribution build는 binary-reference candidate `Run` copy 직후, manifest 전에 이 gate를
호출하고 이후 transaction 완료 전에 tested/final EXE SHA-256 equality를 검사한다. 별도
temp candidate는
`ProjectReference=0`, config absent 상태로 gate를 PASS했고 EXE SHA-256은
`829AC3314E1B5113696DFA06E64418A95C305035335F73DEB4404449CF910F79`, SDK SHA-256은
`7D179781BCE9EB2FE6DB071C3D45F085A5BC127F9DBD0E15300E38A6181A7ED8`이었다. Full
Distribution 첫 attempt는 `Verify-LasalContract.ps1:7571`의
`$macroMatches[-1]` PowerShell 5.1 비호환 tooling bug에서 중단되어 gate/manifest 단계에
도달하지 않았고 transaction residue는 `0`이다. pwsh7에서는 last Match지만 powershell
5.1에서는 null이 되어 `lastMacroEnd=0`과 false macro-to-custom drift를 만들었다. 이는
PLC/source/Classes/`cbf2548` blocker가 아니다. 후속 pwsh7 focused
`-AxisOwnershipReserveVerifierSelfTestOnly`는 exit `0`, negative fixture `62/62` reject와
comment-only fixture accept를 64.3초에 PASS했다. Compatibility commit `ad4af91`은
verifier의 PS5.1 negative-index 접근만 수정했고 targeted PS5/PS7 Publish+Reserve를
PASS했다. 수정 뒤 PS5.1 Release `RunLasalContract`와 `RunLasalNetworkContract`는 해당
경계를 통과한 다음 각각 177.7초/174.9초에 기존 intentional
`LASAL.UdpCallbackContract blocker: Classes.lcb sanctioned Gate D identity drifted`로 exit
`1`이었다. 사용자 current `Classes.lcb`는 수정하지 않았다. 따라서 full Distribution
prerequisite가 STOP이고 new EXE gate/manifest에는 도달하지 않아 Distribution/manifest
PASS는 아니다.

Commit `f337fec`은 각 init 시도의 immutable
`LMCRpcSessionInitializationEvidence`를 `LastRpcSessionInitializationEvidence`에
보존한다. 여기에는 local `SessionGeneration`, 시작/완료 UTC, 정확한 `0x8080`
attempt count, canonical retry 사용 여부, 첫 failure ACK, 마지막 수신 ACK,
`Succeeded`/`Failed`/`Cancelled` outcome과 failure type/message가 포함된다. 실패한
transport가 정리된 뒤에도 이 evidence는 남는다. `CurrentSessionGeneration`은 같은
local generation의 public read-only view다.

WPF의 RPC init evidence는 입력 tuple을 `RequestedCallback`, 실제 UDP bind 결과를
`BoundCallback`으로 구분한다. init 실패 전 bind가 없으면 `not-bound`를 기록하고,
요청 port가 0인 성공 경로는 실제 양수 ephemeral port를 별도로 기록한다.

같은 commit의 `CallbackV2StatisticsChanged` event는 current published session에서
한 datagram decision이 counter에 반영된 직후 immutable
`LMCCallbackV2StatisticsChangedEventArgs` snapshot을 전달한다. Snapshot에는
`DecisionKind`, `ProtocolError`, accepted/rejected/duplicate/out-of-order 네 counter,
`SessionGeneration`, `BelongsTo`와 `BelongsToCurrentSession` provenance가 들어간다.
WPF는 dispatcher 도착 뒤 sender와 current session을 다시 확인한다. 이전 session의
queued wake는 diagnostic `ignored` execution-log line을 남길 수 있지만 retained ticket,
operation summary/state, callback counter를 바꾸거나 `0x7E03`을 전송할 수 없다. 이
diagnostic line 자체는 authoritative UI mutation이 아니다.

### `0x405C` UDP Callback 등록

Legacy payload는 12 bytes다.

| Payload offset | Type | 의미 |
|---:|---|---|
| 0 | `UDINT` | event mask |
| 4 | `DINT` | PC가 listen하는 UDP port |
| 8 | `BYTE[4]` | PC local IPv4 |

정상 응답은 header 8 bytes와 아래 4-byte ACK payload다.

```text
UINT16 Status
INT16  ErrorId
```

2026-07-31 P0 변경에서도 이 wire shape는 그대로다. command ID, request 12-byte
payload와 response 4-byte ACK에 새 필드를 추가하지 않았다.

Version 2는 같은 command ID의 별도 exact shape다. 32-byte 요청 payload에는
event mask, port, IPv4, protocol version, requested maximum datagram, 64-bit cookie,
flags와 reserved가 들어간다. `SessionEpoch`와 `DiagnosticsBootId`는 PC 요청값이
아니며, PLC가 현재 신뢰 상태에서 취득해 20-byte response payload로 반환하는
fence다. PC는 `Version2WakeHint`를 명시한 경우에만 이 shape를 사용한다. Outer
response status는 항상 zero여야 하고, success 또는 canonical negative response만
허용한다. Exact offsets는 `DINT_PACKET_MAP.txt`가 기준이다.

LASAL handler는 request payload length로 두 shape를 분리하고 request 값을 바로
persistent state에 쓰지 않는다.

1. 12-byte legacy branch는 event mask, port와 IPv4를 임시값으로 읽고 현재 RPC
   owner, valid peer, exact peer IPv4와 port `1..65535`를 검증한다. 이미 등록됐다면
   이 세 필드의 exact duplicate만 멱등 성공한다.
2. 32-byte v2 branch는 아홉 request 필드를 임시값으로 읽고 protocol 2, mask bit
   1, peer/port, datagram `52..512`, nonzero cookie, zero flags/reserved를 검증한다.
   현재 `SessionEpoch`와 `Diagnostics.GetDiagnosticsBootId()`를 신뢰 fence로 사용해
   `CallbackSender.ArmEndpoint`를 호출하고, result `0` 또는 exact-duplicate result
   `1`일 때만 accepted tuple을 commit한다.
3. 다른 payload length, malformed request, peer mismatch 또는 다른
   re-registration은 실패 ACK를 반환하고 기존 tuple와 sender FIFO를 보존한다.
   endpoint 변경은 새 RPC session에서만 가능하다.

IPv4 raw value 비교의 정적 근거는 설치된 SIGMATEK 예제
`GetBroadCastData\GetBroadCastData.st`다. 이 예제는 `UDINT`를
`value & 16#ff`, `(value SHR 8) & 16#ff`, `(value SHR 16) & 16#ff`,
`value SHR 24` 순서로 `OS_TCP_USER_TOIP`에 전달한다. 즉 주소 octet을 UDINT
LSB부터 복원한다. 이는 `0x405C`의 `BYTE[4]`를 UDINT로 읽어
`OS_TCP_USER_GETPEERIP` 결과와 비교하는 현재 구현의 byte order 근거다. 다만 이는
설치 source 검토 결과이며 target PLC runtime wire 검증은 아니다.

### `0x405D` Close

payload byte 하나가 `0`인 9-byte 요청을 보내고 4-byte ACK를 받은 뒤 PC가
TCP와 UDP listener를 닫는다. LASAL은 ACK를 보내기 전에 session state를
삭제하거나 socket을 강제로 닫지 않는다.

## 2026-07-10 구현 범위

### PC DLL

`LmcConnection.cs`에 다음을 반영했다.

- 새 연결 인자/IP/port를 먼저 검증하고, 유효할 때만 이전 연결에 `0x405D`
  후 새 연결 시작. invalid reconnect input은 기존 session 유지
- UDP listener를 `0x405C`보다 먼저 개방
- remote/local address를 구체적인 IPv4 주소로 조기 검증
- callback port `0`을 사용하면 실제 할당된 ephemeral port를 등록
- `0x405C`의 legacy 4-byte ACK 또는 explicit version-2 20-byte response를 mode별
  status/error 및 accepted fence로 파싱
- 등록 성공 후 실제 callback port와 event mask를 public state에 저장
- `LMCConnectionOptions`로 connect/read/send/callback join timeout 설정
- `ConnectionStateChanged`와
  `Disconnected/Connecting/Connected/Closing/Faulted` 상태 전이 제공
- initialization protocol, transport, close 오류를 각각
  `LastInitializationException`, `LastTransportException`,
  `LastCloseException`에 분리 보존
- `LastRpcSessionInitializationEvidence`에 `0x8080` attempt/retry/first failure
  ACK/final ACK/outcome을 transport cleanup 뒤에도 보존하고
  `CurrentSessionGeneration`을 public read-only evidence로 제공
- close nonzero ACK는 `RpcCloseResponse`를 보존하고 local TCP/UDP cleanup 뒤
  호출자에게 예외 전달
- callback remote source-address 기본 검증, rejected count와 payload 방어 복사
- raw `LMCCallbackEventArgs`에 listener가 소유한 positive `SessionGeneration`과
  `BelongsTo`/`BelongsToCurrentSession` provenance 제공
- legacy raw mode는 기본값으로 유지하고, 명시적 `Version2WakeHint`에서만
  32/20-byte registration, nonzero CSPRNG cookie, strict 52-byte `LMC2` parser,
  bounded receive와 typed-only `CallbackWakeHintReceived`를 활성화
- version 2 listener는 bind-before-register 후 `Connected` publication까지 receive
  dispatch를 gate한다. typed provenance와 decision counters는 exact published TCP
  lifetime/session에 귀속하며 close/reconnect/safety detach 뒤 stale hint를 거부
- `CallbackV2StatisticsChanged`가 current-session receiver decision과 네 counter의
  immutable snapshot을 제공하며 event args가 owner/current-session provenance를 확인
- `EventType=1`은 `DiagnosticsOperationTerminalAvailable`로 고정하고 nonzero
  `EventId`는 이미 보존 중인 D5 `TicketId`와만 대조. UDP로 ticket을 합성하지 않으며
  `MatchesD5OperationTerminalTicket`가 owner/session/BootId/TicketId를 모두 확인
- 취소 가능한 init/close/axis/group async API
- timeout/전송 오류와 in-flight 취소는 해당 transport generation을 폐기하고
  `Faulted`로 전환. queue 대기 중 취소는 active request를 닫지 않음
- reconnect 성공 뒤 이전 session에서 생성한 axis/group object를 stale
  generation으로 거부
- UDP callback receive/error/rejected-count 경로를 listener 객체와 connection
  lifetime generation에 귀속해 bounded join 뒤 늦게 끝난 이전 handler가 replacement
  session의 error event나 rejected count를 오염시키지 않음
- WPF sample은 shared connection factory에서 version 2를 명시하고 typed wake만
  구독한다. 이미 보존한 current D5 ticket과 exact match한 hint만 single-flight
  `0x7E03 GetOperationStatus`로 조회하며 UDP 자체로 상태를 변경하지 않는다

### Tracked LASAL

기준 소스는 아래 파일이다.

`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`

다음을 반영했다.

- 1024-byte receive buffer 범위 검사
- request header의 payload length offset `4`, reference offset `6` 교정
- 한 socket의 TCP 분할 수신 누적과 여러 frame이 합쳐진 수신 순차 처리
- 응답 socket을 마지막 접속 socket이 아니라 요청 `dSock`으로 선택
- callback 등록을 끝낸 owner socket만 lookup/read/motion command 허용
- `0x8080` init response
- `0x405C` validate-then-commit endpoint ownership: `CurrentPeerValid`, exact TCP-peer
  IPv4, port `1..65535`, first-valid commit, exact-duplicate idempotence, mismatch
  re-registration rejection과 기존 tuple 보존
- `0x405D` ACK 후 session/callback state 정리
- socket disconnect 시 해당 RPC state 정리
- accepted owner transition 또는 definitive current-socket disconnect에서 ordinary
  disarm exact `-8`만 internal `(0,0,0)` sender retirement 후 helper로 재확인
- 일반 `0x8080`/`0x405D` mismatch와 `-9`는 기존 tuple을 보존해 fail-closed

현재 구현은 한 개의 stable active RPC owner만 허용한다. `MaxConnections=2`의
두 번째 slot은 일반 다중-client 공유가 아니라 reconnect candidate용이다. 기존 owner가
있을 때 candidate와 active owner의 TCP peer IPv4가 모두 확인되고 서로 같으면 old socket에
custom command `100` shutdown을 요청한다. 그 요청이 성공하면 candidate를 즉시 새
owner/SessionEpoch로 게시해 `0x8080` 초기화를 진행할 수 있다. peer lookup이 실패하거나
IPv4가 다르거나 old-owner shutdown 요청이 실패하면 candidate에도 command `100` close를
요청하고 기존 owner를 보존한다. 이 source 경로가 확인하는 것은 각 shutdown/close 요청의
return value이며 실제 peer close 완료 자체는 아니다. 수신 누적 버퍼도 현재 owner 한 socket만
소유한다. 여러 PC의 동시 RPC 공유를 지원하려면 `dSock` key 기반 session table과 socket별
receive accumulator로 별도 확장해야 한다.

주의: 이 `.st` 파일은 LASAL CodeGenerator export다. 이번 변경의 class
변수는 생성 declaration 영역에 들어가 있으므로 LASAL IDE의 class model에도
같은 변수를 등록하고 재생성해야 한다. IDE model을 동기화하지 않으면 다음
CodeGenerator 실행에서 declaration이 사라지고 구현부 compile이 깨진다.

## 현재 구현 상태와 아직 검증하지 않은 부분

Version 2 등록, strict typed parser/fence, D5 retained-ticket matcher와 WPF의
authoritative TCP query 경로는 구현됐다. Gate D source candidate도 네 terminal state
`Completed/Failed/Cancelled/Expired`의 one-attempt receipt, 두 broker invocation,
production-path candidate `PublishEvent(...)` call, armed endpoint/matching epoch/valid
payload 조건에서 `EventId=0`의 sender result `-6`,
sender result별 no-retry/operation-truth 불변 계약까지 구현됐다. focused/general static
verifier는 sequence-4 checkpoint tree를 `CAPTURE TerminalWakeBrokerCandidate`로
고정했고 판정은 `ProductionApproved=false`, `NeedsRebaseline=true`다. 현재 generated
`Classes.lcb`는 해당 checkpoint에서 drift해 focused/C78 current verification이 실패한다.

2026-08-10 10:35 C78/ARM incremental `Build project`는 변경된 세 class를 compile했고
source warning 60개(`W0069=28`, `W0070=21`, `W0072=11`), compiler error 0,
`Linker Done`으로 끝났다. 첫 Download와 PLC link는 성공해 `Download Ok`였고, 두 번째
Download는 CPU-state timeout 뒤 aborted됐다. reconnect와 `Project successfully loaded`는
확인됐다. 이는 strict C78 Rebuild나 callback runtime proof가 아니다. 이 증거의 경계와
현재 미완료 runtime 항목은 아래와 같다.

- focused verifier physical pin은 고정됐고 sequence-4 capture 당시
  `VerifyCurrent`는 exit 0의 `CAPTURE TerminalWakeBrokerCandidate` static state를 출력했다.
  checkpoint focused verifier pin은 canonical-LF 545,566 bytes, SHA-256
  `FBF1A8582E85039377AC39F26D8BBA64C0EB62665424DE150083CFC412CC7CA3`이고
  capture self-test는 positive `46` / negative `94` PASS다. 이전 bootstrap
  `ValidateOnly`는 `gate_d_terminal_wake_broker_candidate_checkpoint.json`을
  3,225,878 bytes, SHA-256
  `E0490DC348B861FBE47AB4C2E9C558BE679E865787A014860EBA45B3E0E508E4`로
  계획했지만 `UNTRUSTED`, `outputCreated=false`였으므로 그 bootstrap run에서는
  sequence-4 physical checkpoint가 생성되지 않았다. 이후 trust-anchor `bb5fd93`과
  commit `5543579`가 physical manifest와 exact 7개 production path를 원자적으로
  commit했다. manifest는 `ProductionApproved=false`, `NeedsRebaseline=true`다.
- 이전 PID 4832의 첫 Rebuild는 `Classes.lcb` persistence 예외와 write-failed
  두 error record 때문에 무효다. 두 번째 Rebuild 구간 자체는 C78/ARM
  source warning 76개(`W0069=35`, `W0070=21`, `W0072=17`, `W0073=3`),
  source error 0, `Compiler Done`, `Linker Done`, `CInvalidArgException=0`이다.
  이때 `Classes.lcb`는 8,549,773 bytes, SHA-256
  `3AC3D938DC1520FAEA6C3693161ABDB280CC873A97C60CF79B3F716C7F064C22`다.
- PID 4832에는 post-build `Find in Implementation` action이 없었고 Download는
  0회다. Find는 Object Network Server/Client 행에만 적용되고 일반 method 행에는
  해당하지 않으므로 이 부재 자체는 세 Gate D method의 미완료 사유가 아니다. Connect
  후의 Reset/Restart는 기존 PLC image를 다시 실행했을 뿐이다.
  이 session은 두 Rebuild와 online 동작을 함께 포함하므로 격리된 strict
  build-evidence session으로 승인할 수 없다.
- retained `GateDVisualLayout` PID 480 / Rebuild TID 3396 raw log는 canonical
  load 1회, C78/ARM Rebuild 딱 1회, Connect/Download 0회, 정상 close/exit를
  기록했다. Rebuild-command window는 warning 76개(`W0069=35`, `W0070=21`,
  `W0072=17`, `W0073=3`), error 0, `Compiler Done=2`, `Linker Done=1`,
  post-result C82 compatibility warning 6개, `CInvalidArgException=0`이다.
  `VerifyBuild`는 `profile=GateDVisualLayout`, `inputsEquivalent=true`,
  `rawInputsUnchanged=10/10`, `regeneratedOutputsBound=2`,
  `evidenceSource=bounded-repository`로 PASS했다. exact evidence identities는
  baseline 6,887 bytes /
  `247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE`,
  raw range `[6532176,7298848)` 766,672 bytes /
  `B918E51279360E27780D212650361AF361FFFC391C5F24854447BE0F3F9ABD17`,
  manifest 1,574 bytes /
  `7928BC0D641FEA79444EDE8AD49FC10C15C28D453DB75DAF82C21B9D303D1DFC`,
  transcript 30,111 bytes /
  `F32122D318DBFD8F53BC9E5AD0FF693F9B6F05368D40FC64138A010A1BC810AF`다.
  이 Rebuild/checkpoint-bound `Classes.lcb`는 8,549,773 bytes, SHA-256
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`다.
  PID 7288/D71E...는 superseded historical evidence로만 보존한다.
- PLC download 뒤 exact 32/20-byte registration과 52-byte UDP capture
- UDP wake와 authoritative `0x7E03` TCP response의 causal capture, disconnect,
  takeover, stale cookie/session/BootId, loss/duplicate/reorder 검증

PID 480/TID 3396 derived transcript의 `VerifyBuild`는 PASS했다. 이 isolated
Rebuild session에 method-specific UI proof가 없는 것은 별도 fact다.
`Find in Implementation`은 Object Network Server/Client 행에만 적용되며 일반 class
function/method open에는 해당하지 않는다. 사용자는 이 row-level Find action의 정상
동작을 별도로 확인했지만 이는 selected method open 증거가 아니다. method 행은
`Edit Method`, Enter 또는 direct open으로 열고 exact Implementation tab/header를
확인한다. 사용자는 이후
`LMCDiagnosticsService::TryTakeD5TerminalWake`,
`LMCUdpCallbackSender::PublishEvent`, `TCPMotionInterface::PublishD5TerminalWake`
세 method의 정확한 Implementation 표시가 정상임과 LASAL 종료를 직접 확인했다. 이 UI
evidence는 `exactMethodOpen=manual-attested`이며 동일 UI 동작을 다시 요청하지 않는다.
`Lasal2.log`의 Open Implementation은 class-level token이고 자동 session restore에서도
생길 수 있으므로 selected method를 증명하지 못한다. 자동 method-smoke JSON/log
artifact는 별도 pending/nonblocking이며 log delta는 session 경계,
`CInvalidArgException`, 기록된
금지 명령 audit에만 사용한다. Commit `5543579`의 trusted sequence-4 checkpoint는
`Class/Classes.lcb`, `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`,
`Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`,
`Class/TCPMotionInterface/TCPMotionInterface.st`,
`Class/_UDPTransceiver/_UDPTransceiver.st`,
`Network/Comm_Network/Comm_Network.lcn`, `Network/Networks.lcb`의 exact 7개
production transition path와 manifest를 원자적으로 포함한다. 그 뒤 PID 34656이
C78/ARM Rebuild를 수행해 세 변경 class를 compile하고 `Compiler Done`, `Linker Done`,
command success를 기록했다. Download는 `Download Ok`, `Project successfully loaded`를
기록했고 이후 Reset/Restart와 project loaded도 성공했다. 그러나 이 Rebuild가 그 시점
`Classes.lcb`를 8,549,773 bytes / SHA-256
`6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712`로
바꿔 manifest의 `24402BFA...`와 불일치한다. focused `VerifyCurrent`와 C78
input-equivalence는 당시 실패했고, frozen historical `99014DD9...`와 post-STOP
current `13EA5823...` artifact에서도 gate는 계속 실패한다. Reviewed transition 전
runtime 결과는 exploratory다.

별도 isolated classification은 새 LASAL process와 canonical `.lcp`에서 Rebuild 정확히
1회, 정상 close/exit, Connect/Download 0회로 완료됐다. frozen `Lasal2.log`는
9,554,717 bytes / SHA-256
`25F6A3FA913FD2BF57117C19D0C4489399F5A4FD296CF86C1508AEA07BA02A8C`, 그
isolated classification의 frozen `Classes.lcb` snapshot은 8,549,773 bytes /
`99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD`,
`Networks.lcb`는 242,363 bytes /
`C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F`이며
finalization 전 LASAL process는 0개였다. 이 제3 Classes hash를 얻은 Rebuild는 반복하지
않는다.

`b2019db` bundle의 historical producer는 `fa2a456` revision의
`Finalize-LasalClassesRebuildCandidate.ps1`, physical 187,443 bytes / SHA-256
`1551A121D49C3C3169B0DADA45B4EEAAFDD8F8636425E470D1A6840159CBC0D5`,
Git blob `5495e5636462d8aa67e13abb70c310a1ee8f9e67`이다. 당시 PowerShell 7
self-test positive `26` / negative `76`, Windows PowerShell 5.1 AST/self-test
positive `24` / negative `74`가 PASS했다. Published manifest는 이 producer tuple을
역사 identity로 유지한다.

Current future-run fix `29811c4`는 physical 188,693 bytes / SHA-256
`817E1A416C1484E1AE897140B2C56D8A7DDDF1F4158AC7DED2B59F28C5050116`,
PowerShell 7 self-test positive `27` / negative `77`다. Status를 `Console.Out`으로
분리하고 exact `System.Int32` `{0,2,3}`만 top-level exit로 허용한다. Production은
계속 PowerShell 7-only이며 current committed bundle에는 다시 실행하지 않는다.
Finalizer의 isolated classification이 exit `0`/`2`/`3`으로
`candidate_finalization_gate_d_rebaseline_6e115876`를 publish하면 그 directory를
delete/overwrite하거나 finalizer를 재실행하지 않는다. finalizer가 허용할 수 있는 build
error는 exact load-only `DriveComL2.h` `E0015` 최대 1개뿐이고 다른 또는 추가 error는
중지다. bundle exact 8개는 `.finalizer-owner.json`,
`Classes.post-rebuild.snapshot.lcb`, `Networks.post-rebuild.snapshot.lcb`,
`derived_build_transcript_gate_d_rebaseline_6e115876.txt`,
`bounded_lasal2_delta_gate_d_rebaseline_6e115876.raw.txt`,
`bounded_lasal2_delta_gate_d_rebaseline_6e115876.manifest.json`,
`classes_lcb_gate_d_rebuild_candidate.comparison.json`,
`classes_lcb_gate_d_rebuild_candidate.finalization.json`이다.

첫 real third-hash production run은 pre-`fa2a456` finalizer로 atomic-publish
named-identity recheck까지 진행한 뒤 `OrderedDictionary` key를
`PSObject.Properties[...].Value`로 읽은 버그 때문에 exit `4`로 중단됐다. bundle은
publish되지 않았고 exact-owned stage cleanup은 완료됐다. `fa2a456`은 exact-case
`IDictionary`/`PSCustomObject` accessor와 production-shape 회귀를 추가했다. 이 exit `4`는
accepted exit `3` 판정이 아니다. frozen log와 generated outputs가 그대로이므로 Rebuild가
아니라 finalizer만 한 번 다시 실행했다. `fa2a456`은 bundle을 publish했고 manifest는
`UNSTABLE_THIRD_CLASSES_HASH_STOP`, exit `3`을 기록했다. 다만 old status
`Write-Output`과 반환 integer가 success pipeline에서 배열이 되어 host process는 잘못
`0`을 반환했다. `29811c4`가 이 future process-exit bug를 고쳤으며 historical bundle을
변경하거나 republish하지 않는다. Finalizer와 Rebuild는 더 실행하지 않는다.

Commit `b2019db`는 published exact 8-file bundle을 원자적으로 보존한다. Manifest는
`Classes.lcb=99014DD9...`, unchanged `Networks.lcb=C307547E...`, finalizer exit `3`,
`ProductionApproved=false`, `staticReplayPermitted=false`,
`onlineRuntimeQualificationPermitted=false`다. Checkpoint comparison은 changed
byte/run/opaque-owner `96/52/34`, unmapped run `0`, Gate D target 4개와 protected
dependency 2개 exact를 기록한다. 이는 semantic equivalence나 Download 근거가 아니다.

Initial `531abdd`에서 시작한 bundle validator의 current commit `c48e403`은
`test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Verify-LasalClassesRebuildFinalizationBundle.ps1`,
physical 189,867 bytes / SHA-256
`DB8B046DF00900140E1AB97B83EF1E7AD13EFB44AC2768EE54B219160D8CE6B0`다.
PowerShell 7 self-test positive `5` / negative `32`, Windows PowerShell 5.1
AST가 PASS한다. PS5 production은 bundle evidence read 전에 exit `4`이므로 production
검증은 PowerShell 7-only다. canonical repository root에서 exact command는 다음과 같다.

```powershell
& pwsh.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Verify-LasalClassesRebuildFinalizationBundle.ps1 -VerifyBundle -RepositoryRoot (Get-Location).Path
$LASTEXITCODE
```

Earlier validator는 동일 command text의 두 정상
`Open Network Editor for 'Comm_Network'` load-restoration record를 중복으로 오판했다.
`29811c4`는 raw `commandLineIndex` identity로 이를 교정했다. 다음 defect는 historical
converter CRLF physical tuple과 Git LF blob, C78 verifier mixed-EOL physical tuple과 Git
LF blob을 raw-equal로 요구한 것이었다. `c48e403`은 두 exact path에만 physical
bytes/SHA/blob OID/canonical-LF dual tuple을 적용하며 broad EOL relaxation은 하지 않는다.
Bundle은 byte-unchanged였고 current validator는 committed `b2019db` bundle을 exit `0`으로
PASS했다.

validator exit `0`은 현재 8-file bundle integrity/cross-file contract만 증명하고 과거
atomic move, complete manifest written-last ordering, PLC/runtime 또는 approval은 증명하지
않는다. PASS 뒤에만 directory 전체를 한 Git commit으로 원자 commit한다. failure면 bundle을
그대로 보존하고 중지한다. finalizer exit `0`은 exact static replay와 별도 review, exit
`2`는 vendor semantics 보존/review이며 hash-only rebaseline은 금지다. exit `3`/`4`는
중지다. 전부 no-Download, `ProductionApproved=false`,
`onlineRuntimeQualificationPermitted=false`다.

Commit `998e7132c0892788db79a0868c5b129fb20edd96`의 pinned historical
`Compare-LasalClassesVolatilityTriad.ps1` analyzer는 physical 139,073 bytes /
SHA-256 `E3E2C586C62379339EECFD8038189D9959C655CD206A4E894B846A2D79783663` /
Git blob `a7dd4dba67e30c4adc80549a1d9b6a4d1acb6bce`다. PowerShell 7 self-test
positive `7` / negative `16`, Windows PowerShell 5.1 positive `3` / negative `2`
delegated가 PASS했다. Evidence commit
`e7c812ad7cfc6ef2162ed1197dc615e2aebe45db`는 schema
`LasalClassesVolatilityTriadEvidence/v1`의 exact report
`classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_99014dd9.volatility.json`을
physical 29,412 bytes / SHA-256
`09C76BB3BC313642C3012A915C14C022EDF75965A8A431B87F26B463005489DC` /
Git blob `3c4411e26493043b80828a5355bdc8b621457e09`로 보존한다.

Pinned A/B/C `24402BFA...` / `6E115876...` / `99014DD9...` pairwise changed
byte/run/owner는 `99/58/36`, `96/52/34`, `105/61/36`이다. Structural candidate
`157`개 중 observed volatile 16-bit slot은 `66`개, stable candidate는 `91`개다.
Changed offset 전체가 marker-follower `35`개와 owner-end-minus-48 `31`개의 두 fixed
16-bit slot family에 정확히 매핑된다. Candidate table SHA-256은
`AD8A7FC5D6CB2277819FF28A7B7994C0FD6EAFBE6940419159662B8EFE83924D`,
volatile-slot table SHA-256은
`9D12A54145C409AC257F011C88F782108BCB3D73E9EDCCD8D2653A387F0F193C`다.
이 결과는 fixed slot structure만 증명한다. Field meaning은
`UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT`이고 `99014DD9...` 반복성은
증명되지 않았다.

LASAL executable/compiler, vendor library set, generator cache state, filesystem
timestamps, process session state 6개 implicit input은 모두 `UNPROVEN`이고
`allGeneratorInputsEquivalent=false`다. Publication은
`NON_ADVERSARIAL_WORKSPACE`, `handleRelativeCreationUsed=false`,
`concurrentParentReplacementResistance=false` 경계다. Analyzer diagnostic exit `2`는
`ProductionApproved=false`, `SemanticEquivalenceProven=false`,
`requiresReviewedTransition=true`, `rebaselinePermitted=false`를 유지한다. Focused/C78는
계속 FAIL이고 finalizer `UNSTABLE_THIRD_CLASSES_HASH_STOP` exit `3`가 controlling
STOP이다. Download, runtime qualification, future artifact acceptance, normalization,
hash-only rebaseline은 허용되지 않는다.

Commit `731a01e428bdc9282edbf727f1d76a7a63cd24a3`의 pinned historical
`Analyze-LasalClassesHistoricalSlotCorpus.ps1` analyzer는 physical 156,472 bytes /
SHA-256 `90BDD86EFC9C5032788C2603755A3560CC2871672E638935C4CD955B705EA080` /
Git blob `30379ef2a50e093bbb28d768b9df77d091199de6`이고 PS7 self-test positive
`12` / negative `18`, PS5 delegated positive `5` / negative `1`이 PASS했다.
Evidence commit `43a85319905fbb5a42418b4b1ef9cd364c0bf44d`는 schema
`LasalClassesHistoricalSlotCorpusEvidence/v1` report를 physical 157,999 bytes /
SHA-256 `F306022CECD6C71BB7EA2B3DF309556A2621821B6C2CD287BC3FFFF4FA5A1B6A` /
Git blob `edad859d03ac0c33f21ca42996a377dda3ee7b79`로 보존한다.

Canonical selector는 occurrence/unique/topology `22/20/9`, H/H+C/H+C+B unique는
`20/21/22`다. Full layer는 records/marker samples `2,501/814`이고 exact-other
varying tail은 `87 groups/227 samples/31 owners/202 unequal pairs`, marker는
`95/282/34/369`다. Adjacent transition `21`개의 common `2,378`은 raw identical
`1,155`, candidate-only `538`, outside-target `685`로 나뉘며 added/removed는
`18/2`, candidate tail/marker/both는 `55/97/386`이다. B는 committed oracle에서
복원하고 C는 committed `b2019db` snapshot을 사용하며 mutable current
`Classes.lcb`와 local `bd47dd96...`는 읽지 않는다. Bounded stateless
record-local hypothesis `20`개의 refutation은 exact claim scope에만 유효하고 field
meaning은 unclassified다.

이 commit들은 분석 도구와 역사적 evidence만 추가했다. RPC command ID, request/response
layout, callback registration, UDP datagram, TCP `0x7E03`, LASAL production source와 wire
behavior는 변경하지 않았다. 따라서 `ProductionApproved=false`,
`SemanticEquivalenceProven=false`, `rebaselinePermitted=false`, Download/runtime false,
`requiresReviewedTransition=true`이고 focused/C78 FAIL, finalizer exit `3` STOP 및
Rebuild 반복 금지는 유지된다.

Post-STOP incident evidence commit
`5319352dbe389038b56f00cdaccf7cc14a80bf64`는 exact 네 artifact를 보존한다.
`LASAL_POST_STOP_13EA_DOWNLOAD_INCIDENT_2026-08-11.md`는 9,773 bytes / SHA-256
`7299D2FF74CBD7986AAEEA1FAA42F6B592D5C531FAF5C2CDB524D400A128DD51`,
`bounded_lasal2_delta_post_stop_13ea_download.manifest.json`은 4,679 bytes /
`BD9A72E76BC3526B9546001E8EB75E89F88BDB01B99D3AF4637CF969DF0930E1`,
`bounded_lasal2_delta_post_stop_13ea_download.raw.txt`는 1,490,589 bytes /
`CAA408D3997182495023DBC1FA9719462447D2F822C459990CE4BECB6EA4E69C`,
`classes_lcb_post_stop_13ea5823.comparison.json`은 50,060 bytes /
`DBC54235BDB505D9E7A198B3DCFA2CBD63F8AAC19728D1349FDC46DD5FA6CEC5`다.

Capture 시 current `Classes.lcb`는 8,549,773 bytes /
`13EA5823DF0887D6042408E2A884E9F8DF50304443227353B9BDCA9AD2ECBFD9`,
`Networks.lcb`는 unchanged 242,363 bytes / `C307547E...`, appended
`Lasal2.log`는 11,045,306 bytes / `CEC2256A...`다. PID 26200은 C78/ARM
Rebuild, Connect, 282-LBA Download와 PLC link를 성공했고 PID 21016은 Connect 및
standalone Reset/Restart를 성공했지만 Rebuild/Download는 없었다. Action origin은
unproven이고 exact pre-Download Classes hash 또는 downloaded-payload hash manifest가
없으므로 `13EA...` association은 `TIME_CORRELATION_ONLY`다. Comparator는
`_AxisBase` boundary drift로 exit `3` `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`, changed
byte/run/owner `90/57/35`, unmapped `0`, Gate D `4/4`, protected `2/2` exact를
기록한다. Slot partition은 marker/tail `30/27`; unaccepted D를 diagnostic으로 더한
volatile/stable union만 `71/86`이고 committed A/B/C `66/91`은 그대로다. `b2019db`
990 bundle을
계속 immutable historical evidence로 보존하고 finalizer/Rebuild/Download를 반복하지
않는다. Approval, reviewed rebaseline, exact artifact-to-Download binding과 PLC/runtime
qualification은 없다.

초기 production 의미는 D5 operation terminal availability다. Event mask bit 1,
`EventType=1`, delivery class 0, payload 0, nonzero `EventId=TicketId`이며
`ProducerSessionEpoch=OwnerSessionEpoch`다. UDP는 polling latency를 줄이는 hint일
뿐이고, operation truth는 generation-pinned `0x7E03` response만 변경할 수 있다.

## 검증 항목

1. LASAL IDE에서 tracked project compile
2. PC에서 `RpcInitConnection()` 호출 시 두 응답이 설정된 timeout 안에 도착
3. `IsRpcInitialized == true`
4. `RpcCallbackRegistrationResponse.HasCommandResult == true`
5. callback port `0` 호출 시 wire의 port가 0이 아닌 실제 listener port
6. 잘못된 port/session 요청에서 nonzero status/error 확인
7. `CloseConnection()`의 ACK 후 TCP FIN 확인. nonzero ACK는 local cleanup 뒤
   호출자에게 예외 전달되는지 확인
8. 재연결 시 기존 `0x405D` 후 새 `0x8080` 순서 확인
9. `0x8080`을 여러 TCP segment로 나누거나 `0x8080+0x405C`를 합쳐 보내도
   각각 한 번만 처리되는지 확인
10. callback 등록 전 motion 요청과 다른 socket의 motion 요청이 nonzero
    header status로 거부되는지 확인
11. `CurrentPeerValid=FALSE`, peer IPv4 mismatch와 port `0`/`65536`이 실패하고
    accepted tuple을 바꾸지 않는지 확인
12. 최초 valid tuple과 exact duplicate는 성공하고 event mask/port/IP 중 하나라도
    다른 re-registration은 실패하면서 최초 tuple을 보존하는지 확인
13. reconnect 직전 UI dispatcher에 queued된 raw callback이 새 session log에 나타나지
    않는지 확인
14. opt-in version 2의 32-byte request와 exact 20-byte success/canonical negative
    response, malformed outer status 거부를 확인
15. TCP response 전에 도착한 UDP가 `Connected` publication까지 dispatch되지 않고
    kernel queue에 보존되는지 확인
16. wrong source/cookie/session/BootId, zero/unknown EventId, oversized/truncated datagram,
    duplicate/out-of-order/forward-gap을 typed event 전에 거부하는지 확인
17. safety detach, close와 reconnect의 stale listener가 typed event 또는 version 2
    counters를 새 published session에 귀속시키지 않는지 확인
18. D5 wake가 exact retained ticket과 일치할 때만 한 개의 generation-pinned
    `0x7E03` query를 만들고 TCP response 전에는 UI/operation truth를 바꾸지 않는지 확인
19. unknown/stale/wrong BootId ticket, busy duplicate와 query failure는 drop/log하고
    manual polling fallback을 보존하는지 확인
20. legacy default mode의 12/4-byte request/ACK와 raw callback 동작이 그대로인지 확인
21. 초기/동일 프로세스 내 후속 WPF Connect의 exact persistent `-1`에서 TCP 최대 2개와
    `0x8080` 최대 4회, second outer failure terminal, success-init 뒤에만 `0x405C`가
    나가고 정상 registration ACK 뒤에만 Connect가 성공하는지 확인
22. ErrorId 0/other, malformed/transport/cancellation/callback-stage failure가 fresh-TCP
    outer retry를 사용하지 않는지 확인
23. 내부 replacement와 X가 최대 2회 Dispose로 complete local postcondition을 만들고
    close diagnostics를 보존하는지, strict Close는 close 실패 시 cleanup 뒤 그 오류를
    throw하며 X는 cleanup 미완료 시 취소되는지 확인

Version 2 sender Candidate의 이전 IDE Rebuild/Link checkpoint는 완료됐다. 현재 Gate D
broker declaration과 source, production-path candidate call site도 반영됐고 위의 C78
incremental Build와 첫 Download/PLC link까지 확인됐다. PID 4832와 PID 7288은
historical evidence로 남고, PID 480/TID 3396은 retained `GateDVisualLayout` 단일-Rebuild
derived-transcript가 checkpoint identity에서 `VerifyBuild` PASS했다. 이후 exact-method UI
확인은
`manual-attested`이고 자동 method-smoke artifact는 별도 pending/nonblocking이다.
Sequence-4 commit과 PID 34656 post-commit Rebuild/Download는 완료됐지만 regenerated
`Classes.lcb`가 manifest에서 drift했다. 따라서 reviewed rebaseline과 실제 PLC callback
재캡처 전에는 E2E 완료로 표시하지 않는다. 특히 endpoint ownership, D5 wake causal
query와 loss/duplicate/reorder는 static source/PC 계약으로 runtime 승인할 수 없다.

## PC 로컬 검증 결과

2026-07-10 fake TCP server로 다음을 확인했다.

- 성공 요청 순서: `0x8080/9B -> 0x405C/20B -> 0x405D/9B`
- callback port `0` 요청 시 실제 UDP listener port가 `0x405C`에 기록됨
- event mask `0xA5A55A5A`와 IPv4 `127.0.0.1` byte 일치
- callback/close 4-byte ACK의 `HasCommandResult`, `IsSuccess` 판정 성공
- 실패 ACK `Status=16`, `ErrorId=-8`을 연결 성공으로 처리하지 않고 예외 발생
- `0.0.0.0` callback local address 조기 거부
- options clone/validation, invalid reconnect session 보존
- receive timeout/in-flight cancellation transport 폐기와 `Faulted` 전이
- queued cancellation active request 보존, async init/close 성공
- reconnect 뒤 stale group object 거부
- close nonzero ACK 예외/response/error 보존과 local cleanup
- PC runner 46/46 PASS (2026-07-15 재검증)
- Debug/Release library와 WPF test app build 성공

2026-08-10 Release SDK suite는 1117/1117 PASS했고 이 당시 결과는 historical evidence로 유지된다. 추가
회귀는 exact short
failure의 `ErrorId=-1` 보존과 legacy zero-retry, v2 same-socket
`0x8080 -> 0x8080 -> 0x405C` 성공, 지속 실패의 추가 1회 제한과 `Faulted` cleanup,
다른 ErrorId 및 nonzero reserved의 zero-retry를 고정한다. `f337fec` 회귀는 cleanup 뒤
init evidence 보존, cancellation의 두 번째 `0x8080` 차단, current-session v2 decision
snapshot과 dispatcher 뒤 stale-event 거부를 고정한다. 이는 fake TCP server 기반 PC
계약이며 PLC의 persistent disarm `-8`/`-9` 원인이나 live reconnect 성공 증거가 아니다.
`af4ab63` historical WPF Release smoke runner는 `335/335` PASS였다. 이 run은 canonical
persistent init failure cleanup, non-canonical `ErrorId=0`의 zero-retry/full cleanup,
cleanup 뒤 수동 Connect가 새 session/socket을 사용하는 회귀, `RequestedCallback`과
actual `BoundCallback`/`not-bound` evidence panel, old-session 통계 Dispatcher action이
replacement UI를 바꾸지 못하는 회귀를 포함한다.

commit `bff3bc7`의 PC-only raw-wire harness 16개가 추가된 current SDK Debug/Release
direct suite는 각각 `1133/1133` PASS했고 당시 독립 reviewer Release 재실행도
`1133/1133` PASS다. 그 commit에서 WPF code/test는 바뀌지 않았으며 당시 reviewer의
Release `335/335`는 historical checkpoint다. Current `cbf2548` WPF Debug/Release
Rebuild는 PASS했고 기존 full smoke는 `339/339`, reconnect targeted는 `6/6`, 독립
callback/reconnect review는 `9/9` PASS이고 P0/P1은 없다. 별도 actual-EXE relaunch
gate도 Debug/Release 각각 `1/1` PASS했다. exact runner mode
`callback-ownership-wire`는 인자가 없거나 `--dry-run`이면 network에 연결하지 않고
`all` 또는 `gd-n10a`/`gd-n13-candidate`/`gd-n14-candidate` 계획만 출력한다.

live parser는 실행 승인이 아니라 fail-closed guard다. exact `--execute-live`,
case-sensitive `--confirm PLC-CALLBACK-OWNERSHIP`, concrete `--scenario`(`all` 금지), PLC host와
owner/candidate local IPv4를 지정하는 `--host`/`--owner-local`/`--candidate-local`,
세 40/64-hex Git object로 구성한 declared
`--source-fingerprint HEAD/TRACKED/UNTRACKED`, 기존에 없는 `--output` 파일을 모두 요구한다.
unspecified/broadcast IPv4는 금지하고, N13 source IPv4는 동일, N10A/N14는 서로 달라야
한다. N10A candidate callback port는 `0`이어야 accepted A의 actual owner UDP port를
재사용해 advertised callback IPv4만 B로 바꾼다. output/fingerprint 형식 preflight는
network access보다 먼저 수행하고 기존 파일은 덮어쓰지 않는다. fingerprint는 선언값을
report에 남기지만 tool 자체가 current worktree나 downloaded PLC image와의 일치를
증명하지 않는다.

wire allowlist는 exact `0x8080`, version-2 mask `1`/max `52`/nonzero cookie/zero
flags-reserved `0x405C`, current authoritative owner의 `0x405D`뿐이며 retry는 0회다.
N10A는 same session A success -> IPv4-only B failure -> byte-identical A duplicate/same
fence, N13은 same-IP replacement와 BootId 보존/SessionEpoch advance/old-owner retire 뒤
candidate duplicate, N14는 different-IP candidate의 `0x8080` 뒤 clean peer close와
candidate `0x405C`/`0x405D` zero-wire, retained-owner duplicate를 검사한다. N13 retirement
부재나 N14 timeout/aborted/shutdown은 INCONCLUSIVE다.

report는 `LMC_CALLBACK_OWNERSHIP_WIRE_V1`, PC raw-wire evidence class, executable와
Git HEAD/checkpoint/declared fingerprint, endpoint/timeout, request/response bytes/SHA-256/
hex, PASS/FAIL/INCONCLUSIVE와 exception을 보존한다. 동시에 peer identity unverified,
pcap/PLC Watch not captured, qualification false/incomplete를 명시한다. 따라서 tool PASS는
PLC qualification이 아니다. reviewed rebaseline, exact downloaded checkpoint, site-approved
maintenance, pcap 및 PLC Watch counter가 여전히 필요하며 그 전에는 실제 live command를
제공하거나 실행하지 않는다.

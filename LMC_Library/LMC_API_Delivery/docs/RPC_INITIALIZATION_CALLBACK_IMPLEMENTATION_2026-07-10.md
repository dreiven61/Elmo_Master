# RPC 초기화와 UDP Callback 구현 기록

작성일: 2026-07-10

P0 endpoint ownership/session provenance 갱신: 2026-07-31

Version 2 wake-hint transport 및 D5 의미 계약 갱신: 2026-08-08

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
- `0x405C`의 4-byte ACK를 status/error로 파싱
- 등록 성공 후 실제 callback port와 event mask를 public state에 저장
- `LMCConnectionOptions`로 connect/read/send/callback join timeout 설정
- `ConnectionStateChanged`와
  `Disconnected/Connecting/Connected/Closing/Faulted` 상태 전이 제공
- initialization protocol, transport, close 오류를 각각
  `LastInitializationException`, `LastTransportException`,
  `LastCloseException`에 분리 보존
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

현재 구현은 한 개의 활성 RPC session만 허용하는 1단계 구현이다. 다른
socket이 이미 초기화된 상태에서 `0x8080`을 보내면 오류를 반환한다.
수신 누적 버퍼도 한 socket만 소유한다. 다중 PC 지원은 `dSock` key 기반
session table과 socket별 receive accumulator로 확장해야 한다.

주의: 이 `.st` 파일은 LASAL CodeGenerator export다. 이번 변경의 class
변수는 생성 declaration 영역에 들어가 있으므로 LASAL IDE의 class model에도
같은 변수를 등록하고 재생성해야 한다. IDE model을 동기화하지 않으면 다음
CodeGenerator 실행에서 declaration이 사라지고 구현부 compile이 깨진다.

## 아직 구현하거나 검증하지 않은 부분

Version 2 등록, strict typed parser/fence, D5 retained-ticket matcher와 WPF의
authoritative TCP query 경로는 구현됐다. 다음은 아직 완료되지 않았다.

- PLC의 D5 terminal transition을 exactly-once-attempt wake로 broker하는 production
  `PublishEvent` call site와 그 LASAL IDE declaration/generated metadata
- `Completed/Failed/Cancelled/Expired` 전체 terminal 경로 및 sender result별
  no-retry/operation-truth 불변 정적 계약
- 새 declaration에 대한 LASAL IDE Save/Rebuild/Link, implementation smoke,
  verifier/checkpoint rebaseline
- PLC download 뒤 exact 32/20-byte registration과 52-byte UDP capture
- UDP wake와 authoritative `0x7E03` TCP response의 causal capture, disconnect,
  takeover, stale cookie/session/BootId, loss/duplicate/reorder 검증

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

Version 2 sender Candidate의 IDE Rebuild/Link는 완료됐지만 production D5 broker
declaration과 call site는 아직 반영 전이다. 새 broker를 포함한 IDE compile, PLC
download와 실제 PLC 재캡처를 수행하기 전에는 E2E 완료로 표시하지 않는다. 특히
endpoint ownership, D5 wake causal query와 loss/duplicate/reorder는 static source/PC
계약만으로 runtime 승인할 수 없다.

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

# RPC 초기화와 UDP Callback 구현 기록

작성일: 2026-07-10

P0 endpoint ownership/session provenance 갱신: 2026-07-31

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
`uiCbUdpPort`와 callback function을 UDP로 명시한다. 다만 현재 저장소의
패킷에는 실제 callback UDP datagram이 없으므로 event payload 구조는 아직
정의하지 않는다.

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

payload는 12 bytes다.

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

LASAL handler는 request 값을 바로 persistent state에 쓰지 않고 임시
`callbackEventMask`, `callbackPort`, `callbackIPv4`에 먼저 읽는다. 아래 조건을 모두
통과할 때 최초 tuple만 commit한다.

1. payload가 정확히 12 bytes이고 현재 socket이 초기화된 RPC owner다.
2. `CurrentPeerValid = TRUE`다.
3. 요청 IPv4가 `CurrentPeerIPv4`와 exact match한다.
4. port가 `1..65535`다.

이미 tuple이 등록된 경우에는 event mask, port, IPv4가 모두 같은 exact duplicate만
멱등 성공한다. 하나라도 다른 re-registration은 실패 ACK를 반환하고 기존 tuple은
변경하지 않는다. malformed/peer mismatch 요청도 기존 등록을 지우지 않는다. endpoint
변경은 새 RPC session에서만 가능하다.

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
- 취소 가능한 init/close/axis/group async API
- timeout/전송 오류와 in-flight 취소는 해당 transport generation을 폐기하고
  `Faulted`로 전환. queue 대기 중 취소는 active request를 닫지 않음
- reconnect 성공 뒤 이전 session에서 생성한 axis/group object를 stale
  generation으로 거부
- UDP callback receive/error/rejected-count 경로를 listener 객체와 connection
  lifetime generation에 귀속해 bounded join 뒤 늦게 끝난 이전 handler가 replacement
  session의 error event나 rejected count를 오염시키지 않음
- WPF dispatcher queue에 들어온 raw callback도 active connection identity와
  `BelongsToCurrentSession`을 다시 확인하고 stale session이면 log/UI 반영 없이 폐기

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

## 아직 구현하지 않은 부분

`0x405C`는 callback endpoint 등록까지다. 다음은 아직 완료되지 않았다.

- LASAL 일반 event용 UDP sender object/network 연결
- motion complete, error, emergency 등 event 정의
- event mask bit와 event 종류 매핑
- UDP callback datagram payload 구조
- C# typed callback parser
- 실제 PLC callback capture와 재전송/유실 정책 검증

근거 없이 callback payload를 만들면 PMAS 형식과 LASAL local 형식이 다시
섞이므로 실제 callback 캡처 또는 승인된 local protocol 명세 후 진행한다.

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

LASAL IDE compile, PLC download와 실제 PLC 재캡처는 아직 수행하지 않았으므로
E2E 완료로 표시하지 않는다. 특히 endpoint ownership은 tracked source/PC 계약이며
target의 `OS_TCP_USER_GETPEERIP` byte order와 duplicate/mismatch ACK는 live capture가
필요하다.

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

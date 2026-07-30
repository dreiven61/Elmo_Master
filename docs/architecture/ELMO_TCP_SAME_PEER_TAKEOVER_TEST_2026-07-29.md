# Elmo TCP 동일 IP 소켓 교체 시험

> 2026-07-29 마스터 반영 상태: 사용자 PLC 시험에서 성공한 테스트 프로젝트의
> `TCPIPServer`, `TCPMotionInterface`, `Comm_Network.lcn`, 프로젝트 등록 경로와
> 사람이 검토하는 생성 테이블을 `Elmo_Master` 개발 소스에 선별 반영했다. 테스트
> 프로젝트 전체나 `.lba/.lob/.ldi/ProjectInternal`은 복사하지 않았다. 마스터
> 프로젝트에서 LASAL Save/Rebuild/Link 및 `Find in Implementation`을 다시 수행해야
> IDE database와 build 산출물까지 현재 소스와 일치한다.

## 1. 목적

LAN 단선 후 기존 TCP 소켓이 서버에 남아 있어도, 같은 클라이언트 IP에서 새 연결이 들어오면 기존 소켓을 정리하고 새 소켓을 통신 소유자로 교체한다.

이 기능은 단선을 즉시 검출하는 heartbeat 기능이 아니다. 새 연결 요청이 들어왔을 때 stale 소켓을 회수하는 재접속 복구 기능이다.

## 2. 적용 구조

- TCP 포트: `4000`
- `MaxConnections`: `2`
- `ConnectionsPerRun`: `1` 유지
- 실제 서버 객체: 편집 가능한 `TCPIPServer` (`_TCPIPServer` 상속)
- 동일 IP 판정 및 세션 교체: 편집 가능한 `TCPMotionInterface`
- `TCPIPServer`에 별도로 만들었던 RTTask(`RtWork`)/CycleTask(`CyWork`) 사용자 구현은 삭제하고, takeover 명령 처리용 `SetSocketParameter`만 유지
- 클래스 설정은 `RealtimeTask=false`, `CyclicTask=true`이며 실제 서버 FSM의 cyclic 처리는 상속된 `_TCPIPServer::CyWork`가 담당
- `_TCPIPServer.st`, `_TCPIPServerInterface.st`: 수정하지 않음

동작 순서는 다음과 같다.

1. 첫 연결의 peer IPv4와 소켓을 현재 owner로 저장한다.
2. 두 번째 연결을 먼저 accept한다.
3. 두 소켓의 peer IPv4가 같으면 `TCPIPServer`에 기존 소켓 종료를 요청한다.
4. `TCPIPServer`는 기존 슬롯을 `_STATE_SHUTDOWN`으로만 전환한다.
5. 실제 `CLOSESOCKET`, 연결 목록 삭제, DISCONNECT callback은 원본 `_TCPIPServer` FSM이 수행한다.
6. 새 소켓을 owner로 게시하고 기존 세션의 큐, 수신 누적 버퍼, RPC 상태를 초기화한다.
7. 다른 IP 또는 IP 조회 실패이면 새 소켓을 종료하고 기존 owner를 유지한다.

소스 포트는 비교하지 않는다. 비정상 종료 후 새 TCP 연결의 source port는 보통 달라져야 한다.

## 3. 변경 파일

- `Class/TCPIPServer/TCPIPServer.st`
  - 사용자 클래스명을 `_TCPIPServer_RT`에서 `TCPIPServer`로 변경
  - 불필요한 사용자 RTTask(`RtWork`)/CycleTask(`CyWork`) 구현 삭제
  - 상속된 `_TCPIPServer`의 task/FSM 처리 사용
  - 사용자 명령 `Cmd=100, SubCmd=0` 추가
  - 지정 소켓 슬롯을 inherited `_STATE_SHUTDOWN`으로 전환
- `Class/TCPMotionInterface/TCPMotionInterface.st`
  - peer IPv4 조회 및 동일 IP 판정
  - owner 세션 교체
  - retiring/rejected 소켓의 수신 데이터 격리
  - 늦게 들어오는 기존 소켓 DISCONNECT가 새 owner를 지우지 않도록 처리
- `Network/Comm_Network/Comm_Network.lcn`
  - 서버 객체 `TCPIPServer1`의 클래스를 `TCPIPServer`로 변경
  - `MaxConnections=2`
- `Network/Comm_Network/ONE_Comm_Network_Table.st`
  - 외부 테스트 프로젝트에서 LASAL 재생성된 사람이 검토 가능한 table source를 반영
  - 테스트 프로젝트의 `.lba`는 마스터로 복사하지 않음

## 4. 온라인 확인 변수

`TCPMotionInterface1`에서 다음 변수를 Watch에 추가한다.

| 변수 | 정상 기대값/의미 |
|---|---|
| `CurrentSock` | 현재 명령을 받을 owner 소켓 |
| `ConnectedClients` | 정상 안정 상태 `1`; 교체 중 잠시 `2` 가능 |
| `CurrentPeerValid` | peer IP 조회 성공 시 `TRUE` |
| `CurrentPeerIPv4` | 현재 owner의 IPv4 원시값 |
| `RetiringSock` | 종료 대기 중인 기존 소켓; 정리 후 `0` |
| `TakeoverCount` | 동일 IP old-socket shutdown 요청 수락 뒤 새 owner를 게시한 횟수; 최종 정리 완료 횟수는 아님 |
| `TakeoverRejectCount` | 다른 IP/조회 실패/종료 요청 실패로 거절한 횟수 |
| `LastCandidateSock` | 마지막 신규 소켓 |
| `LastCandidatePeerIPv4` | 마지막 신규 소켓의 IPv4 원시값 |
| `LastCandidatePeerLookupRet` | `0` 이상이면 신규 peer 조회 성공 |
| `LastActivePeerLookupRet` | `0` 이상이면 기존 peer 재조회 성공 |
| `LastOwnerDisconnectRequestRet` | `0`이면 기존 소켓 종료 요청 성공 |
| `LastCandidateDisconnectRequestRet` | `0`이면 거절한 신규 소켓 종료 요청 성공 |
| `LastTakeoverResult` | 아래 결과 코드 참조 |

`LastTakeoverResult`:

| 값 | 의미 |
|---:|---|
| `1` | 첫 owner 연결 수락 |
| `2` | 동일 IP old-socket shutdown 요청 수락 및 새 owner 게시; 최종 완료는 아래 안정 조건으로 별도 확인 |
| `-2` | 신규 소켓 peer IP 조회 실패 |
| `-3` | 기존 owner peer IP를 확보하지 못함 |
| `-4` | 기존/신규 peer IP가 다름 |
| `-5` | 기존 소켓 종료 요청 실패 |
| `-7` | 현재 descriptor에 대한 중복 CONNECT callback |

## 5. 필수 시험 순서

### A. 정상 첫 연결

1. PLC에 변경 프로젝트를 내려 실행한다.
2. PC 테스트 프로그램으로 `TCP 4000`에 연결한다.
3. 초기화와 명령/응답을 한 번 정상 수행한다.
4. 다음을 확인한다.
   - `ConnectedClients = 1`
   - `CurrentSock <> 0`
   - `CurrentPeerValid = TRUE`
   - `LastTakeoverResult = 1`

### B. 현장 재현 순서

1. 소켓 연결 상태에서 LAN 케이블을 뽑는다.
2. PC 테스트 프로그램을 종료한다.
3. LAN 케이블을 다시 연결하고 링크 및 IP 복구를 기다린다.
4. 테스트 프로그램을 다시 실행해 같은 PLC IP/포트로 연결한다.
5. 연결 직후 초기화/첫 명령을 다시 보낸다.

정상 판정:

- 새 TCP handshake가 성립한다.
- `CurrentSock`이 새 descriptor로 바뀐다.
- `LastTakeoverResult = 2`
- `TakeoverCount`가 1 증가한다.
- `RetiringSock`은 잠시 기존 descriptor였다가 `0`이 된다.
- `ConnectedClients`는 잠시 `2`가 될 수 있으나 다시 `1`이 된다.
- 새 연결에서 초기화와 명령/응답이 정상 동작한다.

### C. 다른 IP 거절

1. 첫 PC가 정상 연결된 상태를 유지한다.
2. 다른 IP를 가진 두 번째 PC에서 `TCP 4000`에 연결한다.
3. 다음을 확인한다.
   - 기존 `CurrentSock` 유지
   - `LastTakeoverResult = -4`
   - `TakeoverRejectCount` 증가
   - `LastCandidateDisconnectRequestRet = 0`
   - 두 번째 연결은 서버에서 종료됨

## 6. 패킷 확인

Wireshark 필터:

```text
tcp.port == 4000
```

반드시 확인할 항목:

- 재접속 시 새 `SYN`과 PLC의 `SYN, ACK`
- 첫 연결과 재접속 연결의 PC source port
- 새 연결의 첫 요청 payload와 PLC 응답
- 기존 연결 종료 패킷은 네트워크 단선 상태에 따라 캡처되지 않을 수 있음

클라이언트 연결/첫 응답 timeout `10초`는 충분하다. 서버가 기존 연결을 처리하면서 accept로 복귀하는 데 약간의 지연이 있으므로 최소 1초 이상은 두는 것이 안전하다.

## 7. 실패 시 판정

- 새 `SYN`은 보이지만 PLC의 `SYN, ACK`가 없고 `ConnectedClients`가 계속 `1`
  - 변경된 Comm Network가 PLC에 내려가지 않았거나 `MaxConnections=2`가 적용되지 않은 상태를 먼저 의심한다.
- `LastTakeoverResult = -4`
  - PLC가 본 peer IP가 서로 다르다. VPN, 복수 NIC, NAT 경로를 확인한다.
- `LastTakeoverResult = -2` 또는 `-3`
  - `LastCilTcpUserRet`, `LastCandidatePeerLookupRet`, `LastActivePeerLookupRet`를 기록한다.
- `LastTakeoverResult = -5`이고 종료 요청 반환값이 `-10`
  - PLC에서 실제 서버 객체가 편집 가능한 `TCPIPServer`가 아니라 원본 `_TCPIPServer`로 구성되어 custom command를 모르는 상태일 가능성이 크다.
- `LastTakeoverResult = 2`인데 첫 명령이 실패
  - TCP 교체는 성공했다. 클라이언트가 새 연결에서 초기화/RPC 등록 순서를 다시 수행하는지 확인한다.

## 8. 제한사항

- 같은 IP의 정상 중복 접속도 기존 연결을 교체한다.
- NAT 뒤의 여러 클라이언트가 같은 peer IP로 보이면 서로 교체할 수 있다.
- 새 연결 시도가 없으면 stale 소켓은 이 기능만으로 즉시 검출되지 않는다.
- 거절 candidate의 `Cmd=100` 종료 요청이 실패하면 현재 구현은 자동 재시도하지 않는다.
  이때 non-owner data는 격리되지만 두 번째 slot과 `ConnectedClients=2`가 남아 후속 reconnect를
  막을 수 있다. `LastCandidateDisconnectRequestRet`를 확인하고 PLC/network를 복구해야 한다.
- `LastTakeoverResult=2`와 `TakeoverCount` 증가는 종료 완료보다 앞선다. 완료 판정은
  `RetiringSock=0`, `ConnectedClients=1`, 새 socket의 초기화 및 실제 명령 응답을 모두 요구한다.
- 동일 IP 재접속 takeover는 2026-07-29 사용자 시험에서 정상 동작을 확인했다.
- 다른 IP 거절, peer IP 조회 실패, NAT/복수 NIC 조건은 별도 PLC 런타임 시험이 필요하다.

## 9. 현재 구현 및 검증 결과

- 외부 테스트 프로젝트의 `TCPIPServer.st`: 클래스명 변경 및 불필요한 사용자 RTTask(`RtWork`)/CycleTask(`CyWork`) 제거 후 새 `.lba` 생성
- 외부 테스트 프로젝트의 `TCPMotionInterface.st`: 변경 코드 컴파일 완료, 새 `.lba` 생성
- 외부 테스트 프로젝트의 Comm Network: `TCPIPServer`, `MaxConnections=2`, `ConnectionsPerRun=1`로 `.st/.lba` 재생성
- 마스터에는 검토 가능한 `.st/.lcp/.lcn`과 `ONE_Comm_Network_Table.st`까지 반영했다. 외부 시험본과 executable source 및 XML 구조가 일치한다.
- 2026-07-30 재확인에서 현재 외부 테스트 폴더와 마스터의 아래 네 파일은 byte-identical이었다.
  - `TCPMotionInterface.st`: SHA-256 `77D76156B1D9D4DF6A7F50D0B23C500954C7B6ECE68414F30D07A83E816812EA`
  - `TCPIPServer.st`: SHA-256 `7628E1E3D484681316CFD8BF29D181391F697CF6FBDE9BAA2BDDC04A87936DB6`
  - `ONE_Comm_Network_Table.st`: SHA-256 `E06D1E7AFE87826361F2D8C89CC3719CF67482D7BE6F61CAC9D3FA736640C8F4`
  - `Comm_Network.lcn`: SHA-256 `594AE060C87542A83745832BCA95B68EC55D370F7233CAA21D6A9AAB14351F5D`
- 마스터 SourceOnly 정적 계약은 `Phase5TransportClean / StaticTopologyOnly`로 PASS했다. 이는 LASAL
  IDE build/download 또는 PLC runtime 재검증이 아니다.
- 마스터의 `Classes.lcb`, `Networks.lcb`, `TCPIPServer.lba`, `TCPMotionInterface.lba`, Comm Network `.lba`, root `.lcb`, `MaeExp.*`, `MultiMasterExp.mme`는 아직 이전 `_TCPIPServer_RT` 기준 생성물이다. 외부 시험본 생성물을 복사하지 말고 마스터 LASAL에서 Save/Generate와 Rebuild/Link로 재생성해야 한다.
- 2026-07-29 사용자 PLC 런타임 시험: 동일 IP 재접속 takeover 정상 확인
- 이전 Codex 전체 `Rebuild All`은 설치 라이브러리의 `DriveComL2.h` 경로 오류와
  `LMCRecorderStore.st`의 `DINT/UDINT` 비교 3건 때문에 실패했다.
- 테스트 프로젝트에 있던 Recorder 비교식 3건은 마스터 반영 시
  `TO_UDINT(configureHeaderSize)` 형태로 명시 변환했다. 마스터 LASAL Rebuild 전에는
  이 빌드 게이트가 해소됐다고 판정하지 않는다.

동일 IP 재접속 통신 경로는 외부 시험 프로젝트 PLC 런타임까지 확인됐다. 마스터 프로젝트 Save/Generate/Rebuild/Link,
다른 IP 거절과 나머지 제한 조건은 아직 미검증이다.

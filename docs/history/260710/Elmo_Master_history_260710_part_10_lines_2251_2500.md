
그래서 차이는 이거다.

- **TCP**: 단순 전송 통로
- **Elmo RPC handshake**: TCP 위에서 Elmo 서버가 client/session/callback/event를 관리하기 위한 프로토콜 초기화

LASAL 쪽에서 callback/event/session registry가 필요 없고, 단일 TCP 서버가 command frame만 처리하면 된다면 Elmo식 RPC handshake는 필요 없다.

> 지금 구조에서 RPC가 필요없는건가? 아니면 RPC로 구현하기위해 멀 추가해야되는거지?

지금 구조에서는 **RPC가 필수는 아니다.**
현재 LASAL 쪽이 “TCP 서버가 패킷 받고 바로 명령 실행/응답” 구조라면, 지금처럼 TCP connect 후 command frame을 보내는 방식이면 충분하다.

RPC가 필요한 경우는 이럴 때다.

```text
1. 여러 PC client를 세션별로 구분해야 함
2. connection handle을 PLC/LASAL 쪽에서도 관리해야 함
3. 비동기 event/callback을 PC로 따로 보내야 함
4. motion done, error, homing end 같은 이벤트를 polling 없이 push해야 함
5. command request/response와 event channel을 분리해야 함
6. reconnect 후 이전 session 정리, client ownership 관리가 필요함
```

현재 API/PLC 구조가 아래 정도라면 RPC는 과하다.

```text
PC -> TCP connect
PC -> GetAxisByName
PLC -> AxisReference
PC -> MoveAbsoluteEx
PLC -> ACK/response
PC -> ReadStatus/GetActualPosition polling
```

RPC로 구현하려면 추가해야 할 것은 대략 이거다.

```text
PC side
- RpcInitConnection handshake frame
- client/session id 관리
- callback IP/port 또는 event port 등록
- callback listener socket
- event packet parser
- connection handle/session state
- CloseConnection 시 session unregister

LASAL side
- session table
- client id / connection handle 발급
- axis/group reference를 session과 연결
- callback/event 송신 로직
- motion done/error/homing end event packet 생성
- disconnect/reconnect cleanup
```

현재 우리가 만든 `RpcInitConnection`은 이름만 RPC고 실제로는 TCP connect다.
진짜 RPC로 만들려면 `RpcInitConnection` 안에서 TCP 연결만 하는 게 아니라, 연결 직후 PLC에 **세션 등록 패킷**을 보내고 응답으로 **connection handle/session id**를 받아 저장해야 한다.

예상 구조는 이렇게 된다.

```text
RpcInitConnection()
  -> TCP connect
  -> Send RpcInit request
  -> Receive session handle
  -> optional Send callback registration
  -> store ConnectionHandle
```

그리고 이후 command frame에도 필요하면:

```text
Command frame
  -> session handle
  -> command id
  -> axis/group ref
  -> payload
```

하지만 이걸 하려면 PC DLL만 바꾸면 안 되고, **LASAL TCP 서버도 같은 RPC handshake와 session table을 구현해야 한다.**

현 단계 판단:
- 단순 motion command/response 목적이면 RPC 불필요
- Elmo처럼 callback/event/session ownership까지 흉내내려면 RPC 필요
- 지금 작업 목적이 LASAL 부하 줄이고 PC에서 값 만들어 보내는 거라면, 우선은 현재 TCP command protocol로 가는 게 맞다.

> 니말대로 여려개의 pc가 붙었을때 세션관리가 있어야 될꺼 같은데

<details><summary>14 previous messages</summary>

> 맞습니다. 다중 PC 접속을 고려하면 지금처럼 “TCP 연결만 열고 command를 바로 보내는 구조”로는 ownership과 충돌 처리가 약합니다. 현재 코드가 세션을 어디까지 갖고 있는지 먼저 정확히 확인해서 기준을 잡겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞다. 여러 PC가 붙는 구조면 지금처럼 “TCP 연결만 열고 축 ref를 쓰는 방식”은 세션 경계가 약하다. 먼저 현재 PC DLL과 LASAL TCP 쪽에서 세션을 넣을 위치를 확인하고, 문서로 설계 근거를 남긴 뒤 필요한 최소 변경 범위를 잡겠다.
>
> 현재 근거는 확인됐다. `LMC_API_Delivery` 쪽은 세션 필드가 없고 `RpcInitConnection`도 실제로는 TCP connect만 한다. 다음은 LASAL 쪽 `TCPMotionInterface.Response(pData, udSize, dSock)`가 socket 값을 받는지, 그 값을 세션 키로 쓸 수 있는지 확인하는 단계다.
>
> LASAL 코드도 확인했다. `TCPMotionInterface`에는 `Response(..., dSock)`와 `ConnSocketInfo(dSock, InfoPara1)`가 이미 있어서, 새 패킷 헤더를 만들지 않고도 socket 기반 세션 테이블을 붙일 수 있다. 이 방향이 기존 offset을 덜 흔든다.
>
> 이제 문서부터 남긴다. 코드는 아직 건드리지 않고, 확인된 현재 구조와 바꿔야 할 최소 범위를 `LMC_API_Delivery/docs` 아래에 세션 설계 문서로 고정하겠다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+195 -0)
>
> </details>
>
> 세션 설계 문서는 추가했다. 기존 API 구조 결정 문서에도 이 세션 문서를 참조하게 만들어서, 나중에 같은 결정을 두 군데서 따로 해석하지 않게 연결해두겠다.
>
> 기존 문서 문구가 내가 예상한 것과 달라서 패치가 한 번 빗나갔다. 파일 내용을 확인해서 실제 위치에 맞춰서 다시 넣겠다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+5 -0)
>
> </details>
>
> 문서 연결까지 끝났다. 이제 변경 범위와 현재 LASAL 코드에서 바로 보이는 위험만 확인해서, 코드까지 들어갈지 판단할 수 있게 정리하겠다.
>
> 변경은 문서 두 개뿐이다. 다른 작업 중인 파일은 건드리지 않고, 세션 설계 문서만 따로 스테이징해서 커밋하겠다.
>
> 스테이징과 공백 검사는 통과했다. 이제 커밋을 만든다.
>
> 스테이징된 건 세션 설계 문서 2개뿐이다. 기존 다른 변경들은 그대로 놔둔다.
</details>

맞다. 여러 PC를 붙일 거면 세션 관리는 필요하다. 단, 지금 단계에서는 Elmo식 full RPC가 아니라 **socket 기반 LMC 세션**으로 가는 게 맞다. 현재 DLL은 [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:23)에서 TCP만 열고, [LmcProtocol.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:55)의 8바이트 헤더에는 세션 필드가 없다. LASAL 쪽은 [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis_Edit/Class/TCPMotionInterface/TCPMotionInterface.st:417)에서 `dSock`을 받으니 이걸 세션 키로 쓰면 된다.

설계 문서로 정리해서 커밋했다: [SESSION_MANAGEMENT_DESIGN_2026-07-09.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md:1). 핵심은 기존 패킷 offset은 유지하고, LASAL에서 `dSock` 기준 세션 테이블과 axis/group ownership을 관리하는 방식이다. 다음 코드 작업은 `CurrentSock` 의존 제거, session table 추가, motion/control 명령 busy 처리 순서로 가면 된다.

검증: 문서 변경만 있어서 빌드는 돌리지 않았고, `git diff --check`는 통과했다. 커밋은 `f8abcd6 Document LMC session management design`.

> 그럼 RPC Connection으로 한다는 가정하에 lib를 수정해봐. 구성은 패킷 분석한 내용을 토대로해. 패킷 분석한거 md파일로가지고 있나? 없으면 md파일로 저장해서 진행해

<details><summary>40 previous messages</summary>

> RPC Connection 가정으로 DLL 쪽을 수정하겠다. 먼저 기존 패킷 분석 MD가 있는지 확인하고, 실제 분석 문서와 기존 delivery 코드/구 LMC_API 구현을 같이 대조해서 handshake를 넣겠다.
>
> 패킷 분석 MD는 이미 있다: `LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md`. 이제 `MMC_RpcInitConnection.txt` 원본 캡처와 구 LMC_API 구현을 대조해서 delivery DLL에 넣을 handshake 범위를 정하겠다.
>
> 캡처상 RPC init은 `0x8080` 9바이트 요청 후 응답, 이어서 `0x405C` callback 등록 20바이트 요청으로 보인다. close는 `0x405D` 9바이트다. 이 세 개를 delivery DLL에 넣되, 기존 motion 프레임은 그대로 유지하는 쪽으로 수정한다.
>
> 구현 전 범위는 명확하다. `LMCConnection`에 `0x8080` session init과 `0x405C` callback registration을 넣고, `CloseConnection()`은 `0x405D`를 실제로 보내게 바꾼다. 기존 axis/group motion 패킷은 건드리지 않는다.
>
> 이제 파일을 수정한다. `RpcInitConnection`은 기존 3개 인자 호출을 유지하면서 기본 callback port `5003`, event mask `0xFFFFFFFF`로 캡처와 같은 2단계 RPC handshake를 수행하게 만든다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+42 -1)
>
> </details>
>
> 프로토콜 쪽에 `0x8080`, `0x405C`, `0x405D` 프레임 빌더를 넣었다. 다음은 connection 객체에서 실제 connect 직후 handshake를 보내고 close 때 종료 프레임을 보내도록 바꾼다.
>
> 큰 패치가 끝부분 문맥 차이 때문에 들어가지 않았다. 같은 변경을 작은 덩어리로 나눠서 적용한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+9 -5)
>
> </details>
>
> 상태 속성과 기본 RPC callback 값을 넣었다. 이제 connect 메서드 본문을 handshake 포함 구조로 교체한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+48 -2)
>
> </details>
>
> `RpcInitConnection`은 이제 TCP connect 뒤 캡처 기반 RPC session init과 callback 등록까지 수행한다. 다음은 wrapper와 close 동작을 맞춘다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+23 -3)
>
> </details>
>
> close 경로도 `0x405D`를 보내도록 바꿨다. 이제 handshake 응답 status 검사 함수를 넣는다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+16 -0)
>
> </details>
>
> 코드 변경은 들어갔다. 이제 컴파일 기준으로 문법과 공개 API를 확인하고, 문서/샘플을 RPC handshake 기준으로 갱신하겠다.
>
> 문서가 아직 “handshake 없음” 기준이라 이제 코드와 맞지 않는다. 세션 문서와 README를 RPC connection 기준으로 고치고, delivery 폴더 안에도 연결 패킷 결정 문서를 별도로 남긴다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md` (+97 -0)
>
> </details>
>
> 연결 패킷 근거 문서를 추가했다. 이제 기존 구조/세션 문서에서 “TCP만 연결”이라고 되어 있던 부분을 현재 결정으로 바꾼다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_STRUCTURE_DECISION_2026-07-09.md` (+4 -0)
>
> </details>
>
> API 결정 문서는 RPC handshake 기준으로 갱신했다. 세션 설계 문서는 이전 상태 설명이 남아 있어서 이어서 정리한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+33 -15)
>
> </details>
>
> 세션 문서가 일부 “구현됨” 항목을 과하게 잡았다. 실제 코드와 맞게 `SessionId`는 아직 optional로 남기도록 문구를 바로잡겠다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+2 -1)
>
> </details>
>
> 이제 README와 샘플 호출을 새 API 형태에 맞춘다. 샘플은 기존 3인자 호출도 동작하지만, callback port와 event mask를 명시해 RPC connection 의도를 보이게 하겠다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -1)
>
> </details>
>
> README에서 “handshake 없음” 설명을 제거했다. 샘플도 callback port와 event mask를 명시한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+5 -1)
>
> </details>
>
> 문서와 샘플까지 맞췄다. 작은 품질 수정으로 handshake 응답이 null이면 성공처럼 넘어가지 않게 처리한 뒤 빌드하겠다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+6 -1)
>
> </details>
>
> 이제 빌드와 정적 검사를 돌린다. 빌드가 통과하면 최종 DLL도 `LMC_API_Delivery/bin`에 갱신한다.

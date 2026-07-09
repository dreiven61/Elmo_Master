>
> 빌드는 통과했다. 최종 배포 DLL 경로도 갱신하겠다. `bin`은 무시 대상이라 커밋에는 소스/문서만 들어간다.
>
> DLL은 갱신했다. 이제 reflection으로 내부 프레임 빌더가 실제 캡처 바이트와 같은지 확인한다.
>
> 세션 init과 close는 캡처 바이트와 일치했다. callback 검증은 PowerShell의 `0xffffffff` 캐스팅 문제라서 값 표현만 고쳐 다시 확인한다.
>
> 프레임 바이트 검증도 통과했다. `0x8080`, `0x405C`, `0x405D`가 캡처 문서의 바이트와 그대로 나온다. 이제 전체 diff를 확인하고 커밋 범위를 `LMC_API_Delivery`로만 묶겠다.
>
> diff 범위는 의도대로 `LMC_API_Delivery` 안의 소스/문서/샘플이다. 최종 커밋 전에 obsolete 문구가 남았는지 한 번 더 검색하고 스테이징한다.
>
> `CloseConnection(false)`는 reconnect/실패 정리용 내부 경로라 의도된 예외다. 이제 `LMC_API_Delivery` 변경만 스테이징하고 커밋한다.
>
> 스테이징 완료했고 `git diff --check`도 통과했다. 스테이징된 범위를 한 번 확인한 뒤 커밋한다.
>
> 커밋 대상은 `LMC_API_Delivery` 7개 파일로만 제한되어 있다. 기존 다른 변경들은 건드리지 않는다.
>
> 커밋은 생성됐다. 마지막으로 현재 상태와 커밋 해시만 확인하고 마무리한다.
</details>

수정 완료했다. 패킷 분석 MD는 이미 있었고, 근거는 [PACKET_ANALYSIS.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API/Elmo_API_Packet2/PACKET_ANALYSIS.md:431)에 있었다. Delivery 쪽에는 별도 근거 문서 [RPC_CONNECTION_PACKET_DECISION_2026-07-09.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md:1)를 추가했다.

변경 내용:
- [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:27)
  - `RpcInitConnection()`이 TCP connect 후 `0x8080` session init 전송
  - 이어서 `0x405C` callback registration 전송
  - 기본 callback port는 `5003`, event mask는 `0xFFFFFFFF`
  - `CloseConnection()`/`Dispose()`가 `0x405D` close frame 전송
- [LmcProtocol.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:29)
  - `0x8080`, `0x405C`, `0x405D` 프레임 빌더 추가
- README, API 구조 문서, 세션 설계 문서, 샘플 코드도 RPC connection 기준으로 갱신

검증:
- `dotnet build LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release` 성공, warning/error 0
- reflection으로 생성 프레임 확인:
  - `80 80 00 00 01 00 00 00 00`
  - `5c 40 00 00 0c 00 00 00 ff ff ff ff 8b 13 00 00 c0 a8 63 0e`
  - `5d 40 00 00 01 00 00 00 00`
- 최종 DLL 갱신: [LasalMotionControlLib.dll](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/bin/LasalMotionControlLib.dll)

커밋 완료: `4a044b0 Implement captured RPC connection handshake`

주의할 점은 이제 LASAL 서버 쪽도 `0x8080`, `0x405C`, `0x405D`를 처리해야 한다. 아직 LASAL 파서가 이 명령을 응답하지 않으면 PC DLL은 `RpcInitConnection()`에서 timeout 난다.

> RPC를 하게되면 기존 TCP랑 무슨차이인거야? 소켓을 따로 열어서 관리하나?

RPC는 TCP의 대체가 아니다. **TCP 위에 얹는 세션/호출 규약**이다.

현재 우리가 넣은 구조 기준으로는:

- 기존 TCP 방식:
  `connect()` 후 바로 `GetAxisByName`, `MoveAbsolute` 같은 command packet을 보냄.

- RPC Connection 방식:
  `connect()` 후 먼저 아래 handshake를 보냄.
  - `0x8080`: RPC session init
  - `0x405C`: callback/event 등록 정보 전송
  - 이후 기존 command packet 전송
  - 종료 시 `0x405D`

즉, **motion command용 소켓은 따로 더 여는 게 아니라 같은 TCP 소켓을 쓴다.**

다만 `0x405C` 안에 local IP와 callback port `5003`을 넘긴다. 이건 컨트롤러가 나중에 이벤트/notification을 PC로 보내기 위한 별도 callback 채널을 등록하는 의미로 보인다. 진짜 Elmo식 event callback까지 구현하려면 PC 쪽에서 `5003` 포트를 listen하는 별도 UDP/TCP 소켓이 필요할 수 있다.

현재 우리가 수정한 DLL은 여기까지다:

1. TCP socket open
2. 같은 TCP socket으로 `0x8080`
3. 같은 TCP socket으로 `0x405C`
4. 같은 TCP socket으로 축/그룹 명령
5. close 때 `0x405D`

아직 별도 callback socket listener는 만들지 않았다. 지금 목적이 다중 PC 세션 식별/초기화라면 여기까지가 1단계고, 비동기 이벤트까지 필요하면 callback socket을 추가해야 한다.

> callback socket listener가 있어야 되는거 아닌가? Elmo도 그럼 RPC 세션관리하는건가? 클로즈할때 둘다 닫음?

<details><summary>1 previous message</summary>

> 질문은 맞다. 방금 DLL에 넣은 건 “callback 등록 패킷”까지이고, 실제 callback을 받을 listener는 아직 없다. Elmo 캡처와 문서 기준으로 callback 포트가 어떤 의미인지 다시 확인해서, 지금 구조에서 필요한 범위를 정확히 말하겠다.
</details>

맞다. **callback까지 RPC라고 부를 거면 listener가 있어야 한다.**
내가 방금 넣은 건 정확히 말하면 **RPC handshake 송신부만 구현한 상태**다. `0x405C`로 callback port `5003`과 PC IP를 등록하지만, PC에서 `5003`을 열고 이벤트를 받는 listener는 아직 없다.

구조는 이렇게 보는 게 맞다.

```text
PC
 ├─ Command TCP socket
 │   ├─ TCP connect
 │   ├─ 0x8080 RPC session init
 │   ├─ 0x405C callback 등록
 │   ├─ GetAxisByName / MoveAbsolute / ReadStatus ...
 │   └─ 0x405D close
 │
 └─ Callback listener socket
     └─ controller event / async notification 수신
```

Elmo도 RPC 세션 관리를 한다고 봐야 한다. 근거는 Maestro API가 `MMC_RpcInitConnection(..., pHndl)`로 connection handle을 만들고, 이후 API가 `hConn`을 계속 받는 구조다. 문서에도 `MMC_RpcInitConnection`은 RPC connection 시작, `MMC_CloseConnection`은 Maestro connection 종료로 되어 있다. 그리고 캡처에는 `0x8080`, `0x405C`, `0x405D`가 따로 잡혀 있다.

중요한 차이는 이거다.

- **명령 송수신 TCP socket**: motion/read/write RPC command를 주고받는 주 채널
- **callback listener**: controller가 PC로 비동기 이벤트를 밀어주는 보조 채널
- `0x405C`는 “내 callback은 이 IP:port로 보내라”는 등록 패킷이다.
- 그래서 `0x405C`를 보내면서 listener가 없으면 구조상 반쪽이다. 이벤트가 오면 못 받거나 controller 쪽에서 callback 실패가 날 수 있다.

close할 때는 우리 lib 기준으로는 **둘 다 닫는 게 맞다**.

1. command TCP socket으로 `0x405D` 전송
2. command TCP socket close
3. callback listener socket close
4. connection/session 상태, axis/group handle cache 정리

Elmo 쪽은 `MMC_CloseConnection(hConn)`이 RPC connection 단위 정리 역할이고, 별도로 `MMC_OpenUdpChannel` / `MMC_CloseUdpChannel`도 문서에 있다. 그래서 명시적으로 연 UDP channel은 별도 close 대상이다. 다만 `0x405C` callback 등록으로 생긴 listener는 connection 객체가 소유하게 설계하는 게 맞다.

현재 우리 DLL 상태는:
- `0x8080` 보냄
- `0x405C` 보냄
- `0x405D` 보냄
- **callback listener 없음**

그래서 다음 수정은 `LMCConnection`이 callback listener를 소유하도록 만드는 게 맞다. 우선은 `UdpClient`로 `CallbackPort`를 bind하고 background receive loop를 돌리는 구조가 가장 현실적이다. 이벤트 payload format은 아직 캡처가 없으니, 처음에는 raw event를 `event Action<byte[]> CallbackReceived` 식으로 노출하고 나중에 패킷이 확보되면 parser를 붙이는 게 안전하다.

> 그럼 수정된 구조로 설계문서를 만들고 소스코드 구현해

<details><summary>30 previous messages</summary>

> 설계 문서부터 갱신하고 그 기준으로 DLL 소스를 바꾸겠다. 이번 변경은 `LMCConnection`이 command TCP socket과 callback listener를 같이 소유하고, close 때 둘 다 정리하는 구조로 간다.
>
> 현재 메모리 레지스트리에는 이번 RPC/callback 구현에 직접 연결되는 추가 항목이 없었다. 작업 대상은 `LMC_API_Delivery`로 한정하고, 다른 dirty 파일은 건드리지 않는다.
>
> 문서는 별도 callback listener 설계로 남긴다. 여기서는 callback payload 포맷이 아직 캡처되지 않았으므로 parser를 만들지 않고 raw UDP event를 사용자 이벤트로 올리는 구조로 고정한다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+97 -0)
>
> </details>
>
> callback listener 설계 문서를 추가했다. 기존 RPC/세션 문서와 README에도 이 문서를 연결해서 구조 기준을 하나로 맞춘다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -0)
>
> </details>
>
> 문서 기준은 정리됐다. 이제 소스에 raw callback 이벤트 타입과 UDP listener 수명주기를 넣는다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+28 -0)
>
> </details>
>
> 이벤트 인자 타입을 추가했다. 다음은 `LMCConnection`이 UDP listener thread를 열고 닫도록 구현한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+14 -0)
>
> </details>
>
> 연결 객체에 listener 상태/이벤트를 넣었다. handshake 순서를 `0x8080 -> listener open -> 0x405C`로 바꾼다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -0)
>
> </details>
>
> listener를 `0x405C` 전에 열도록 했다. 이제 close/reconnect 실패 시 command socket과 listener가 둘 다 정리되도록 close 경로를 수정한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+8 -6)
>
> </details>
>
> close 경로가 listener만 살아 있는 경우도 정리하도록 바뀌었다. 이제 receive loop와 error event를 추가한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+103 -0)
>
> </details>
>
> listener 구현은 들어갔다. 샘플에도 raw callback 수신 이벤트를 연결해서 사용자가 실제 payload를 로그로 볼 수 있게 만든다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+9 -0)
>
> </details>
>
> 소스 구현이 끝났다. 이제 빌드하면서 컴파일 에러와 callback frame 기존 검증이 깨지지 않았는지 확인한다.
>
> 빌드는 통과했고 공백 검사도 통과했다. 배포 DLL을 갱신하고, 기존 RPC 프레임 바이트가 그대로 유지되는지 다시 확인한다.
>
> 기존 RPC 프레임 바이트는 그대로다. 한 가지 더 보강해서, 사용자 callback handler가 예외를 던져도 listener thread가 죽지 않도록 이벤트 호출을 보호한다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+15 -2)
>
> </details>
>
> listener 이벤트 보호까지 반영했다. 최종 빌드를 다시 돌린다.
>
> 최종 빌드 통과했다. 배포 DLL을 다시 갱신하고 변경분을 스테이징한다.
>
> 코드 diff는 의도대로다. 실제 callback listener loop도 로컬 UDP로 최소 동작 검증을 해보겠다. 컨트롤러 없이 private listener를 reflection으로 열고 loopback UDP payload를 보내는 방식이다.
>
> 로컬 UDP callback 검증도 통과했다. raw payload 3바이트를 listener event로 받았다. 이제 변경 범위 확인 후 커밋한다.
>
> 변경 대상은 `LMC_API_Delivery`의 소스/문서/샘플뿐이다. 다른 dirty 파일은 그대로 두고 이 범위만 스테이징한다.
>
> 스테이징과 공백 검사는 통과했다. 스테이징된 파일 목록을 확인하고 커밋한다.
>
> 스테이징된 범위는 의도대로 7개 파일이다. 커밋을 만든다.
>
> 커밋 완료했다. 마지막으로 커밋 해시와 작업트리 상태를 확인한다.
</details>

구현 완료했다. 구조 문서는 [CALLBACK_LISTENER_DESIGN_2026-07-09.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md:1)에 추가했고, RPC/세션 문서와 README도 같이 맞췄다.

소스 변경은 [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:1) 중심이다.
- `RpcInitConnection()` 순서: TCP connect -> `0x8080` -> callback UDP listener open -> `0x405C`
- `CloseConnection()`/`Dispose()` 순서: `0x405D` 송신 -> TCP close -> callback listener close
- raw callback 수신 이벤트 추가: `CallbackReceived`
- listener 에러 이벤트 추가: `CallbackListenerError`
- listener 상태 노출: `IsCallbackListenerRunning`, `CallbackLocalEndPoint`
- callback payload 포맷은 아직 캡처가 없어서 parser 없이 raw bytes로 올린다.

검증:
- `dotnet build LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.sln -c Release` 성공, warning/error 0
- RPC frame 바이트 재검증 성공
- local UDP loopback으로 callback event 수신 검증 성공
- 배포 DLL 갱신: [LasalMotionControlLib.dll](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/bin/LasalMotionControlLib.dll)

커밋 완료: `5d7f176 Add RPC callback listener ownership`

남은 주의점: 이 구현은 UDP callback listener 기준이다. 실제 컨트롤러 callback이 TCP로 들어오는 캡처가 나오면 listener transport만 확장해야 한다.

> 지금 LMC_Response 이 구조가 맞는건지 궁금해. 이걸 써서 괜찮은건가? 아니면 임시방편일 뿐이냐? 장단점을 설명해주고 어떤 방향으로 가는게 맞는지 설계해봐

<details><summary>11 previous messages</summary>

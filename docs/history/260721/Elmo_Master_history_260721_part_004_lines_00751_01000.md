
필요한 구현은 대략 다음입니다.

1. LASAL이 `0x405C`로 받은 callback IP/port를 저장
2. Motion 완료, 오류, 상태 변경 등을 감지
3. 이벤트 종류·Axis reference·상태·오류를 payload로 구성
4. 등록된 IP/port로 UDP 전송
5. PC의 `CallbackReceived`에서 payload 해석

PC 쪽에서는 다음처럼 구독하게 됩니다.

```csharp
connection.CallbackReceived += (sender, e) =>
{
    // e.Payload 해석
    // 주의: UI 변경은 Dispatcher를 사용해야 함
};

await connection.RpcInitConnectionAsync(
    targetIp,
    tcpPort,
    localCallbackIp,
    callbackPort,
    LMCConnection.DefaultEventMask,
    cancellationToken);
```

중요한 점은:

> `MoveAbsoluteExAsync()` 같은 Async API의 결과를 UDP로 받게 만드는 것이 아니라, 명령 결과는 계속 TCP로 받고 동작 완료·오류 같은 이벤트만 UDP로 별도 통지하는 구조가 맞습니다.

현재 상태는 **PC UDP 리스너와 callback 주소 등록까지 구현됐고, LASAL UDP 이벤트 송신과 payload 규격은 아직 미구현**입니다.

> 그럼 지금 RPC Init하면 연결하고 PC UDP 리스너를 열어서 연결까지가 구현인가? 아님 아직 UDP 연결은 미구현인가?

<details><summary>1 previous message</summary>

> 초기화 성공 범위를 “TCP 연결”, “PC의 UDP 수신 준비”, “LASAL의 콜백 목적지 등록”, “실제 UDP 송수신”으로 나눠 다시 대조하겠습니다.
</details>

정확히는 다음 상태입니다.

> **PC가 UDP 수신 소켓을 열고, 그 주소와 포트를 LASAL에 등록하는 것까지 구현돼 있습니다. 하지만 LASAL이 실제 UDP 데이터를 보내는 부분은 아직 없습니다.**

RPC 초기화 과정은 다음과 같습니다.

```text
1. PC → LASAL TCP 연결
2. PC → LASAL 0x8080 RPC 초기화 요청
3. PC가 지정된 IP:포트로 UDP 소켓 bind
4. PC가 UDP 수신 스레드 시작
5. PC → LASAL 0x405C로 UDP 수신 주소·포트 전달
6. LASAL이 주소·포트를 저장하고 TCP로 ACK
7. RpcInitConnectionAsync 완료
```

코드에서도 실제로 UDP 소켓을 엽니다.

```csharp
var listener = new UdpClient(
    new IPEndPoint(parsedLocalAddress, callbackPort));

thread.Start(); // UDP 수신 스레드
```

`callbackPort`가 `0`이면 운영체제가 빈 포트를 자동으로 정하고, 실제 선택된 포트를 LASAL에 전달합니다.

다만 UDP에는 TCP처럼 “연결 완료”라는 개념이 없습니다. 현재 초기화가 확인하는 것은:

- PC가 UDP 포트에 정상적으로 bind했는가
- LASAL이 UDP 목적지 정보를 저장했는가

여기까지입니다. 다음은 확인하지 않습니다.

- LASAL에서 PC로 UDP 패킷이 실제 도착하는가
- 방화벽이 UDP를 허용하는가
- LASAL이 callback 이벤트를 발생시키는가

현재 구현 상태를 표로 정리하면:

| 기능 | 상태 |
|---|---|
| PC UDP 소켓 열기 | 구현 |
| PC UDP 수신 스레드 | 구현 |
| UDP 데이터 수신 및 `CallbackReceived` 호출 | 구현 |
| PC 주소·포트를 LASAL에 등록 | 구현 |
| LASAL이 주소·포트 저장 | 구현 |
| LASAL이 이벤트 패킷 생성 | 미구현 |
| LASAL → PC UDP 전송 | 미구현 |
| UDP 왕복 연결 확인/heartbeat | 미구현 |

따라서 지금 `RpcInitConnectionAsync()`가 성공해도:

```text
TCP RPC 통신: 사용 가능
PC UDP 수신 준비: 완료
LASAL → PC UDP callback: 아직 발생하지 않음
```

입니다. 즉, **UDP 수신 준비와 등록까지는 구현됐지만 end-to-end callback 기능은 미완성**입니다.

> LMCConnection 연결할 때 LMCConnection()만 호출하면 타임아웃은 설정안되나? 아님 기본값으로 설정되냐?

<details><summary>1 previous message</summary>

> 기본 생성자가 어떤 옵션 객체를 만드는지와 실제 connect/send/receive timeout 적용 위치를 코드에서 확인하겠습니다.
</details>

`new LMCConnection()`만 호출해도 **기본 타임아웃이 자동 설정됩니다.**

```csharp
var connection = new LMCConnection();
```

내부적으로 다음과 같습니다.

```csharp
public LMCConnection()
    : this(new LMCConnectionOptions())
{
}
```

기본값은:

| 항목 | 기본값 |
|---|---:|
| TCP 연결 timeout | 3,000 ms |
| TCP 송신 timeout | 3,000 ms |
| TCP 응답 수신 timeout | 3,000 ms |
| UDP callback 스레드 종료 대기 | 500 ms |
| UDP callback 송신 IP 검증 | 활성화 |

타임아웃을 변경하려면 생성할 때 옵션을 전달해야 합니다.

```csharp
var connection = new LMCConnection(
    new LMCConnectionOptions
    {
        ConnectTimeoutMilliseconds = 5000,
        SendTimeoutMilliseconds = 3000,
        ReceiveTimeoutMilliseconds = 10000,
        CallbackThreadJoinTimeoutMilliseconds = 1000,
        ValidateCallbackSourceAddress = true
    });
```

주의할 점은 다음처럼 수정하면 적용되지 않는다는 것입니다.

```csharp
var connection = new LMCConnection();

// Options가 복사본이므로 실제 connection 설정은 바뀌지 않음
connection.Options.ReceiveTimeoutMilliseconds = 10000;
```

반드시 생성자에 넘겨야 합니다.

또한 다음의 `CancellationToken.None`은 timeout을 끄는 의미가 아닙니다.

```csharp
await connection.RpcInitConnectionAsync(..., CancellationToken.None);
```

이 경우 사용자 취소만 사용하지 않는 것이고, 기본 연결·송신·수신 timeout 3초는 그대로 적용됩니다.

근거는 [LmcConnection.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:68)와 [LmcConnectionModels.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs:31)입니다.

> RpcInitConnection에서 eventMask랑 cancellationToken은 어떻게 쓰는거야?

<details><summary>1 previous message</summary>

> 두 인자가 실제 프레임에 어떻게 들어가고, 현재 LASAL에서 어디까지 효력이 있는지 분리해서 확인하겠습니다.
</details>

두 인자는 용도가 완전히 다릅니다.

## `eventMask`

LASAL에서 PC로 보낼 UDP 이벤트의 종류를 선택하기 위한 32비트 비트마스크입니다.

```csharp
LMCConnection.DefaultEventMask
```

현재 기본값은:

```csharp
0xFFFFFFFF
```

즉, 정의된 모든 이벤트를 받겠다는 의미로 설계돼 있습니다.

초기화할 때 다음 12바이트가 TCP `0x405C` 명령으로 LASAL에 전달됩니다.

```text
eventMask       4 bytes
callbackPort    4 bytes
PC IPv4         4 bytes
```

LASAL은 이를 다음 변수에 저장합니다.

```text
RpcCallbackEventMask
RpcCallbackPort
RpcCallbackIPv4
```

하지만 현재는 LASAL의 UDP 이벤트 송신부가 없기 때문에:

> `eventMask`는 전달·저장까지만 되고, 실제 이벤트 필터링에는 아직 사용되지 않습니다.

따라서 지금은 `DefaultEventMask`를 그대로 사용하면 됩니다.

```csharp
LMCConnection.DefaultEventMask
```

나중에 이벤트 종류를 정의하면 다음처럼 사용할 수 있습니다.

```csharp
const uint MotionCompleted = 0x00000001;
const uint AxisFault       = 0x00000002;
const uint RecorderReady   = 0x00000004;

uint eventMask = MotionCompleted | AxisFault;
```

## `cancellationToken`

`RpcInitConnectionAsync()`가 실행되는 도중 사용자가 연결 작업을 취소하기 위한 값입니다.

```csharp
CancellationToken.None
```

을 넘기면:

> 사용자 취소 기능을 사용하지 않겠다는 뜻입니다. 기본 TCP timeout은 그대로 적용됩니다.

취소 기능을 사용하려면:

```csharp
var cts = new CancellationTokenSource();

try
{
    await connection.RpcInitConnectionAsync(
        plcIp,

        tcpPort,
        pcIp,
        callbackPort,
        LMCConnection.DefaultEventMask,
        cts.Token);
}
catch (OperationCanceledException)
{
    // 연결 취소 처리
}
```

다른 버튼이나 종료 처리에서:

```csharp
cts.Cancel();
```

하면 현재 연결 작업이 취소됩니다.

시간 기준으로 전체 초기화를 취소할 수도 있습니다.

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10));

await connection.RpcInitConnectionAsync(
    plcIp,
    tcpPort,
    pcIp,
    callbackPort,
    LMCConnection.DefaultEventMask,
    cts.Token);
```

취소되면 라이브러리는:

1. 진행 중인 TCP 연결 또는 통신을 중단
2. 열었던 TCP 소켓 정리
3. UDP callback listener 정리
4. 연결 상태를 `Disconnected`로 변경
5. `OperationCanceledException` 발생

순서로 처리합니다.

## Timeout과 Cancellation 차이

| 항목 | 의미 |
|---|---|
| `ConnectTimeoutMilliseconds` | TCP 연결이 일정 시간 안에 안 되면 실패 |
| `SendTimeoutMilliseconds` | TCP 송신이 일정 시간 안에 안 되면 실패 |
| `ReceiveTimeoutMilliseconds` | TCP 응답이 일정 시간 안에 안 오면 실패 |
| `CancellationToken` | 사용자가 원할 때 전체 연결 작업 취소 |
| `eventMask` | 향후 UDP로 받을 이벤트 종류 선택 |

일반적인 사용은 다음이면 충분합니다.

```csharp
await newConnection.RpcInitConnectionAsync(
    plcIp,
    tcpPort,
    pcIp,
    callbackPort,
    LMCConnection.DefaultEventMask,
    CancellationToken.None);
```

현재 상태에서는 `eventMask`는 기본값을 쓰고, 연결 취소 버튼이나 화면 종료 중 취소가 필요할 때만 실제 `CancellationToken`을 전달하면 됩니다.

> 이 두 기능은 원래 Elmo API에도 구현되어 있는거야?

<details><summary>1 previous message</summary>

> `eventMask`와 `CancellationToken`을 분리해서 원본 Elmo/Maestro 헤더·매뉴얼과 현재 이식 API를 대조하겠습니다.
</details>

둘 중 하나는 Elmo 원본 기능이고, 하나는 이번 C# 이식 API에서 추가한 기능입니다.

| 기능 | Elmo 원본 | 현재 LASAL API |
|---|---|---|
| `eventMask` | 있음 | Elmo 구조를 이식 |
| `CancellationToken` | 없음 | C# Async 지원을 위해 추가 |
| UDP callback port/function | 있음 | PC 수신·등록까지만 이식 |
| Async/Task API | 확인한 Elmo API에는 없음 | `Task.Run` 방식으로 추가 |

### `eventMask`: Elmo 원본 기능

Elmo의 원본 C++ 헤더에도 있습니다.

```cpp
unsigned int ConnectRPC(
    char* cHostIP,
    char* cDestIP,
    int iEventMask,
    MMC_CB_FUNC fpClbk);
```

Elmo의 기존 .NET 테스트 프로그램도 다음처럼 사용합니다.

```csharp
MMCConnection.ConnectRPC(
    rIP,
    4000,
    lIP,
    5000,
    new cbFunc(UserCallback),
    0xEFFFFFFF,
    out hndl);
```

즉, 다음 기능은 Elmo 원본 구조입니다.

```text
TCP 연결
+ UDP callback port
+ callback 함수
+ eventMask
```

근거:

- [Elmo MMCConnection.h](/C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/Maestro_275/GMAS_275/includes/CPP/MMCConnection.h:72)
- [Elmo MMCLibDotNET 테스트 코드](/C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/Maestro_275/Gold%20Maestro%20MMCLibDotNET_Test_2010/MMCLibDotNET_Test_2010/Form1.cs:51)

다만 기본 mask 값은 완전히 동일하지 않습니다.

```text
Elmo C++ 예제       : 0x7FFFFFFF
Elmo .NET 예제      : 0xEFFFFFFF
현재 LASAL API 기본값: 0xFFFFFFFF
```

현재는 LASAL UDP 이벤트 송신이 없으므로 이 차이가 실제 동작에 영향을 주지 않지만, 이벤트 송신을 구현할 때는 Elmo event ID 정의와 맞춰 다시 결정해야 합니다.

### `CancellationToken`: 현재 C# 이식 API에서 추가

Elmo 2.75/2.92와 MMCLibDotNET 테스트 코드에서는 다음 형태만 확인됩니다.

```csharp
MMCConnection.ConnectRPC(...);
```

다음과 같은 API는 없습니다.

```csharp
ConnectRPCAsync(...)
RpcInitConnectionAsync(...)
CancellationToken
Task<LMC_Response>
```

`CancellationToken`은 현재 라이브러리에서 WPF 프로그램이 연결 작업을 취소할 수 있도록 추가한 PC 전용 기능입니다.

```text
CancellationToken
  └─ C# PC 내부에서만 사용
      ├─ 대기 취소
      ├─ TCP 소켓 닫기
      └─ UDP listener 정리

LASAL로는 전송되지 않음
```

반면 `eventMask`는 실제 `0x405C` TCP 프레임에 포함되어 LASAL로 전송됩니다.

한 문장으로 정리하면:

> `eventMask`와 UDP callback 구조는 Elmo 원본 API에서 가져왔고, `CancellationToken`과 Async API는 현재 LASAL용 C# 라이브러리에서 추가한 편의·취소 기능입니다.

> elmo는 EventMask로 어떤걸 받아?

<details><summary>2 previous messages</summary>

> Elmo 헤더의 비트 정의와 Maestro 매뉴얼의 이벤트 표를 함께 대조해서, mask 값별로 어떤 callback이 들어오는지 정리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

정확히는 **Elmo가 EventMask로 데이터를 받는 게 아니라**, PC가 Maestro로부터 **UDP Callback으로 받을 이벤트 종류를 선택**하는 32비트 구독 마스크입니다.

| Bit | 값 | 수신 이벤트 |
|---:|---:|---|
| 0 | `0x00000001` | 비동기 명령 결과 `ASYNC_REPLY` |
| 1~3 | — | 미정의/예약 |
| 4 | `0x00000010` | 펌웨어 다운로드 진행 상태 |
| 5 | `0x00000020` | Emergency `EMCY` |
| 6 | `0x00000040` | Motion 완료 |
| 7 | `0x00000080` | Heartbeat 오류 |
| 8 | `0x00000100` | PDO 수신 |
| 9 | `0x00000200` | Drive 오류 |
| 10 | `0x00000400` | EMIT 사용자 이벤트 |
| 11 | `0x00000800` | Homing 완료/실패 |
| 12 | `0x00001000` | Maestro 시스템 오류¹ |
| 13 | `0x00002000` | Modbus Register Write |
| 14 | `0x00004000` | Touch Probe 완료 |
| 15 | `0x00008000` | Node 오류 |
| 16 | `0x00010000` | Limit에 의한 정지 |
| 17 | `0x00020000` | PVT/Table Underflow |
| 18 | `0x00040000` | 성공한 Async 명령도 Callback 발생 |
| 19 | `0x00080000` | Node 재연결 |
| 20 | `0x00100000` | Global Async 명령 완료 |
| 21 | `0x00200000` | Node 초기화 완료/실패 |
| 22 | `0x00400000` | Function Block Notification |
| 23 | `0x00800000` | Error Policy 실행 종료 |

¹ 해당 Maestro 매뉴얼 버전에서는 `SYSTEM_ERROR`가 아직 미구현이라고 표시돼 있습니다.

중요한 부분은 bit 18입니다. 이건 별도의 이벤트가 아니라 동작 옵션입니다.

- Async 명령 실패: EventMask와 관계없이 오류 Callback 전송
- Async 명령 성공: bit 18이 켜져 있어야 성공 Callback 전송

예를 들어 Motion 완료, Emergency, Heartbeat 오류, Drive 오류만 받으려면:

```csharp
uint eventMask =
    (1u << 5) |  // Emergency
    (1u << 6) |  // MotionEnded
    (1u << 7) |  // HeartbeatError
    (1u << 9);   // DriveError

// 0x000002E0
```

정의된 모든 EventMask 비트의 합은 `0x00FFFFF1`입니다. Elmo 예제의 `0xEFFFFFFF`나 현재 LASAL API의 `0xFFFFFFFF`는 정의된 이벤트를 사실상 모두 선택하며, 차이는 예약/미정의 비트까지 켰다는 점입니다.

또한 Mask만 설정한다고 무조건 이벤트가 발생하는 것은 아닙니다.

- MotionEnded: 대상 축에서 Motion End 이벤트를 별도로 활성화해야 함
- PDO: PDO 및 이벤트 모드를 먼저 구성해야 함
- Policy: Error Policy 등록 필요
- Homing/Touch Probe 등: 해당 기능이 실제 실행돼야 이벤트 발생

그리고 **현재 LASAL 이식본은 `eventMask`, Callback IP, UDP Port를 PLC에 저장하는 단계까지만 구현**돼 있습니다. Elmo처럼 위 이벤트들을 실제 UDP 패킷으로 생성·전송하고 Mask로 필터링하는 부분은 아직 없습니다.

근거는 [Maestro EventMask 표](</C:/work/Elmo/Elmo_Master/output/pdf/maestro_api_md/chunks/049_p1252-p1290_Chapter-14-API-Events-C-C++.md:786>)와 [Elmo 2.75 헤더](</C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/Maestro_275/GMAS_275/includes/MMC_events_API.h:224>)입니다.

> HeartBeat는 CAN에서만 쓰는거 아니냐? EtherCAT도 HeartBeat를 쓰냐? 여기서 HeartBeat는 Master가 Slave에 HeartBeat를 못받아서 발생되는 에러냐. 아니면 마스터랑 pc랑 연결할때 연결 상태를 보는 HeartBeat 냐

<details><summary>2 previous messages</summary>

> 맞습니다. 여기서는 CANopen Heartbeat와 EtherCAT 감시 기능, 그리고 PC–Maestro 연결 감시를 분리해서 봐야 합니다. Elmo 헤더·매뉴얼과 현재 LASAL 코드에서 `HBEAT_EVT`의 실제 발생 주체를 확인해 정확히 구분하겠습니다.
>
> <details><summary>Searched the web</summary>
>
> <details><summary>Searched the web</summary>
>

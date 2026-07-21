> - Searched the web for `site:elmosoftware.com MMC_RegisterHeartBeatErrorEvent HBEAT_EVT Maestro heartbeat EtherCAT ...`
> - Searched the web for `"MMC_RegisterHeartBeat" Maestro ...`
>
> </details>
>
> </details>
</details>

맞습니다. **CANopen의 Heartbeat 프로토콜은 CAN용**입니다. EtherCAT에는 CANopen처럼 Slave가 주기적으로 Heartbeat 메시지를 보내는 구조가 없습니다.

다만 Elmo API가 `HBEAT_EVT`라는 이름을 두 경우에 재사용해서 혼동되는 겁니다.

| 네트워크 | 실제 감시 방법 | `HBEAT_EVT` 의미 |
|---|---|---|
| CANopen | Drive가 주기적으로 Heartbeat 전송 | Maestro가 Drive Heartbeat를 제한 시간 안에 못 받음 |
| EtherCAT | Cyclic frame, Working Counter, missed frame, PHY/link, Hot Plug 상태 감시 | Maestro가 EtherCAT Slave 단절을 감지함 |
| PC–Maestro | TCP/RPC 연결 | `HBEAT_EVT`와 무관 |

즉 질문하신 두 가지 중에서는:

> **Master와 Slave/Drive 사이의 연결 상태를 보는 이벤트입니다. Master와 PC 사이의 연결 상태를 보는 Heartbeat가 아닙니다.**

CANopen에서는 진짜로 다음 흐름입니다.

```text
Drive/Slave ── CANopen Heartbeat ──> Maestro
                     X
              설정 시간 동안 미수신
                     ↓
Maestro ── UDP HBEAT_EVT(AxisRef) ──> PC
```

Elmo 매뉴얼도 `MMC_SetHeartBeatConsumer`를 CANbus 기능으로 분류하고, Drive가 정해진 시간 안에 Heartbeat를 보내지 않으면 Host로 Heartbeat Error Event를 보낸다고 설명합니다. [CAN Heartbeat Consumer 설명](</C:/work/Elmo/Elmo_Master/output/pdf/maestro_api_md/chunks/058_p1449-p1487_19.9.10-MMC_ConfigEventModePDO4.md:1067>)

EtherCAT에서는 실제 Heartbeat 패킷을 받는 게 아닙니다.

```text
EtherCAT Master
    ├─ Working Counter 감소
    ├─ 응답 없는 cyclic frame
    ├─ PHY/Lost Link
    ├─ Hot Plug 연결 상태
    └─ AL/ESM 상태
           ↓
    Slave 단절 판단
           ↓
    Elmo API에서는 HBEAT_EVT로 PC에 통지
```

Elmo 매뉴얼에도 EtherCAT 연결 상태는 Hot Plug 메커니즘으로 감시하고, Slave가 끊기면 “Heartbeat error event”를 보낸다고 적혀 있습니다. 즉 **이벤트 이름만 Heartbeat이고, EtherCAT 선로에서 Heartbeat telegram이 흐르는 것은 아닙니다.** [EtherCAT 연결 오류 감시](</C:/work/Elmo/Elmo_Master/output/pdf/maestro_api_md/chunks/009_p0174-p0192_4.15.1-General.md:194>)

현재 LASAL 이식 구현에서는 다음처럼 매핑하는 게 맞습니다.

- `Online: TRUE → FALSE` 또는 Slave 통신 단절: `HBEAT_EVT`
- `Online: FALSE → TRUE`: `NODE_CONNECTED_EVT`
- `AL_StatusCode`, DS402 Fault 등: `NODE_ERROR_EVT`
- PC–PLC TCP 연결 단절: 별도의 Connection/Faulted 상태로 처리

LASAL EtherCAT 객체에도 이미 `Online`, `EtherCATState`, `SlaveState`, `AL_StatusCode`가 존재합니다. [ECAT_DS402Base 상태 채널](</C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/ECAT_DS402Base/ECAT_DS402Base.st:89>)

그리고 현재 PC–PLC 쪽에는 주기적으로 Ping을 보내는 Session Heartbeat가 구현돼 있지 않습니다. 명령 송수신 중 TCP 오류나 timeout이 발생해야 `Faulted`를 알게 되는 구조입니다. 따라서 `EventMask` bit 7을 PC 연결 감시로 구현하면 Elmo 원본 의미와 달라집니다.

> elmo에서도 LMCConnection()랑 RpcInitConnectionAsync나눠져 있냐?

<details><summary>1 previous message</summary>

> Elmo 원본의 `.NET MMCConnection`과 C++ `MMCConnection`을 각각 확인해서, 생성자와 실제 RPC 연결 단계가 분리돼 있는지, 그리고 Elmo에도 `Async` 메서드가 따로 있는지 구분하겠습니다.
</details>

정확히는 **언어별로 다릅니다.**

### Elmo C++

우리 구조처럼 나뉘어 있습니다.

```cpp
CMMCConnection connection;          // 객체만 생성
connection.ConnectRPC(...);         // 여기서 실제 RPC 연결
```

즉:

- `CMMCConnection()` → 객체와 내부 상태 준비
- `ConnectRPC()` → 실제 TCP/RPC 연결과 Callback 등록

[C++ MMCConnection 헤더](</C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/Maestro_275/GMAS_275/includes/CPP/MMCConnection.h:62>)

### Elmo .NET

우리와 다릅니다. `MMCConnection`이 정적 클래스라서 객체를 생성하지 않습니다.

```csharp
int handle;

MMCConnection.ConnectRPC(
    remoteIp,
    4000,
    localIp,
    5000,
    callback,
    eventMask,
    out handle);
```

즉 Elmo .NET에는 다음 코드가 없습니다.

```csharp
new MMCConnection();
```

`ConnectRPC()` 한 번으로 내부 Connection 객체 생성, TCP/RPC 연결, UDP Callback 포트 등록, Handle 반환까지 처리합니다. [Elmo 공식 .NET 예제](</C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/MMCLibDotNET Test App_V3.0.0.7/MMCLibDotNETTest/MMCLibDotNET_Test/ConnectivityControl.cs:241>)

그리고 Elmo .NET에는 `ConnectRPCAsync`나 `RpcInitConnectionAsync`가 없습니다. `ConnectRPC()`는 동기 호출입니다. `ConnectRPCEx()`도 Async가 아니라 Callback 형식이 다른 확장 버전입니다.

### 현재 LASAL API

```csharp
var connection = new LMCConnection();  // 아직 네트워크 연결 안 됨

await connection.RpcInitConnectionAsync(...); // 여기서 실제 연결
```

- `LMCConnection()` → 옵션, timeout, 상태 객체만 준비
- `RpcInitConnection()` → 동기 실제 연결
- `RpcInitConnectionAsync()` → 비동기 실제 연결

현재 `RpcInitConnectionAsync()`는 Elmo 원본에 있던 기능이 아니라, 우리가 `OpenRpcConnection()`을 `Task.Run()`으로 실행하도록 추가한 .NET 편의 API입니다. [현재 구현](</C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:157>)

결론은 **현재 구조는 Elmo C++ 방식과는 유사하지만, Elmo .NET API를 그대로 복제한 구조는 아닙니다.**

> elmo까는 MMCConnection static으로 박아놓고 쓰는데?

맞습니다. **Elmo .NET의 `MMCConnection`은 static 클래스**입니다. 제가 앞에서 언어별 차이를 설명했지만, 지금 비교 대상인 Elmo .NET만 보면 현재 LASAL API와 구조가 다릅니다.

Elmo 방식은 다음과 같습니다.

```csharp
int handle;

MMCConnection.ConnectRPC(
    remoteIp,
    4000,
    localIp,
    callbackPort,
    callback,
    eventMask,
    out handle);
```

내부적으로는:

```text
static MMCConnection
    └─ Dictionary<handle, MMCConnectionObject>
          ├─ handle 1 → 실제 Connection 객체
          ├─ handle 2 → 실제 Connection 객체
          └─ handle 3 → 실제 Connection 객체
```

즉 `MMCConnection` 자체는 static이지만, 연결은 내부의 `MMCConnectionObject`와 `handle`로 각각 관리합니다. 그래서 이후 API도 대부분 다음처럼 handle을 받습니다.

```csharp
MMCConnection.GetVersion(handle, ...);
MMCConnection.RegisterHBeatEventCallback(handle, callback);
MMCConnection.CloseConnection(
    MMCConnection.GetConnection(handle));
```

[Elmo 공식 연결 예제](</C:/work/Elmo/Elmo_Master/docs/Elmo_Lib/MMCLibDotNET Test App_V3.0.0.7/MMCLibDotNETTest/MMCLibDotNET_Test/ConnectivityControl.cs:241>)

반면 현재 LASAL API는:

```csharp
var connection = new LMCConnection();
await connection.RpcInitConnectionAsync(...);

var axis = LMCSingleAxis.Create(connection, "_LMCAxis1");
```

각 `LMCConnection` 인스턴스가 직접 TCP 소켓, UDP Listener, 상태, timeout을 소유합니다.

따라서 정확한 판정은:

- Elmo .NET: `static MMCConnection + handle`
- 현재 LASAL API: `LMCConnection 인스턴스`
- 현재 API는 Elmo .NET의 외부 사용 형태를 그대로 복제한 것이 아님
- `RpcInitConnectionAsync`도 우리가 추가한 기능

Elmo API 호환성을 최우선으로 한다면 `MMCConnection.ConnectRPC(..., out handle)` 형태의 static 호환 계층을 추가해야 합니다. 다만 내부 구현까지 static으로 바꿀 필요는 없고, 현재 `LMCConnection` 인스턴스를 static Dictionary에 등록해 handle로 노출하는 방식이 가장 안전합니다.

> Static 계층으로 바꾸는 것은 나중에 할 수 있게 설계안에 나중에 구현으로 남겨놔.
>
> 이제 API 구조 검증이 끝났으니 EtherCAT 기능과 Recoder 기능을 구현할 수 있게 설계해보자.
>
> 권장 구조
> EtherCAT RT cycle
>   ├─ EtherCAT Health
>   ├─ Signal Catalog / PI
>   ├─ 동일 cycle의 Bulk Snapshot
>   └─ RT Recorder → 고정 Ring/Double Buffer
>                          ↓
>                   Non-RT TCP chunk upload
>                          ↓
>                    WPF Plot / CSV Export
> 핵심 원칙은 다음과 같습니다.
> RT Recorder는 PDO 갱신 직후 전용 RT 경로에서 샘플링
> 동적 메모리, 문자열, TCP 송신, 파일 저장, SDO를 RT 경로에 넣지 않음
> Signal ID, 타입, 단위, scale, PDO index/subindex, read/write 권한을 정적 Catalog로 관리
> Bulk 결과에 cycleCounter, timestamp, map revision, entry별 상태 포함
> PI Write는 기본 비활성화하고 allowlist 적용
> ControlWord와 TargetPosition 직접 Write는 기존 DS402 모션 경로와 충돌하므로 기본 금지
> SDO는 요청 ticket을 반환하고 PLC 주기에서 비동기로 처리
> 현재 TCP request payload는 1320 bytes, send staging buffer는 2048 bytes입니다. Recorder 데이터는 한 응답에 담을 수 없으므로 recordId + bufferId + offset + count + sequence 기반 chunk upload가 필수입니다.
> 근거: [TCPMotionInterface.st (line 113)](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:113)
> 시간 해상도와 메모리
> 현재 EtherCAT bus와 축 RT 주기는 1 ms입니다. 따라서 LASAL Recorder의 현실적인 최고 해상도도 1 ms입니다.
> Elmo와 비슷하게 구성하면:
> 32채널 × 4 bytes × 31,250 samples
> = 4,000,000 bytes
> ≈ 3.81 MiB
> CP313은 공식 사양상 dual-core와 256 MB RAM을 가지고 있어 이 정도 정적 버퍼는 구조적으로 가능해 보입니다. 다만 실제 PLC free RAM과 RT jitter를 측정한 뒤 상한을 확정해야 합니다. CP313 공식 사양
> SIGMATEK 기존 기능 활용
> SIGMATEK 자체도 LASAL Data Analyzer와 PLC Trace View를 제공합니다. LVD DataAnalyzer는 빠른 채널 기록, 채널별 주기·시간, 트리거, 커서, 장기 기록 기능을 제공합니다. 따라서 개발·시운전 진단은 이것을 먼저 사용하는 것이 가장 빠릅니다. LASAL 공식 소개, LVD DataAnalyzer 공식 자료
> 다만 이것은:
> 현재 WPF API와 직접 호환되지 않음
> Elmo PI/Bulk API 대체물이 아님
> 배포 라이선스와 PLC 런타임 포함 여부를 SIGMATEK에 확인해야 함
> 권장 구현 순서
> EtherCAT Health + Read-only PI Catalog
> Master/Slave 상태, AL code, 누락 frame, frame time
> 현재 활성 PDO 목록과 typed value
>
> Bulk Snapshot
> 동일 cycle의 shadow snapshot
> 물리축 1~4와 software축 5~9 명확히 구분
>
> Recorder 1차
> 고정 채널
> manual/no-trigger
> 단일 정적 버퍼
> status/header/chunk upload
>
> Recorder 2차
> pre-trigger
> edge/window/mask trigger
> ring/double buffer
> PC 그래프 및 CSV export
>
> 제한적 PI/SDO Write
> 타입·범위·축 상태·소유권 검사 후 허용

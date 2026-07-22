# LASAL Motion Control API 기능 설명서

문서 버전: 1.6
적용 API: LasalMotionControlLib 0.9.1-preview
대상 환경: Windows, .NET Framework 4.8
발행일: 2026-07-23

\pagebreak

# 개정 이력

| 문서 버전 | 날짜 | 내용 |
|---|---|---|
| 1.0 | 2026-07-15 | 최초 작성 |
| 1.1 | 2026-07-16 | 공개 API 레퍼런스 작성 |
| 1.2 | 2026-07-16 | API 기능, 인자 UNIT과 반환값 중심으로 간소화 |
| 1.3 | 2026-07-16 | preview 안전 경고, 응답 판정과 4축 group 제한 보완 |
| 1.4 | 2026-07-16 | group position read 계약 불일치와 static identity 제한 명시 |
| 1.5 | 2026-07-22 | read-only Admin, typed drive status, PI/Bulk facade와 local error catalog 추가 |
| 1.6 | 2026-07-23 | `0x7D22` GroupMoveLinearRelative API, wire/state 제한과 runtime 검증 경계 추가 |

이 문서는 `LasalMotionControlLib.dll`의 API 기능과 호출 인자, UNIT, 반환값을
설명하는 빠른 참조다. 모든 공개 diagnostic event/property를 열거한 완전한 API
reference는 아니다.

> **Preview/안전 경고:** `0.9.1-preview`는 production 승인본이 아니다. PC 시험과
> LASAL 정적 계약은 통과했지만 기존 motion PLC command E2E/재캡처는 `0/25`다.
> `LMC_Response.IsSuccess`는 frame과 command 수락 결과이지 motion, power 전이,
> Stop 완료가 아니다. typed status/position을 polling한다. `CloseConnection`,
> `Dispose`, timeout과 cancellation은 Stop을 보내지 않는다. 실제 장비에서는 E-stop,
> HW/SW limit, UNIT, Home/Reference와 이동 범위를 별도로 승인한다.
> Admin `0x7D00/10/20/22`는 source와 정적 시험까지 완료했지만 LASAL IDE build,
> PLC download와 실물 parameter 값/UNIT/relative motion은 아직 검증하지 않았다.

> **출판 상태:** 이 Markdown 원본은 문서 버전 `1.6`지만 현재 Distribution의
> DOCX/PDF는 아직 문서 버전 `1.0`이다. 아래 안전·group-read 보완은 외부 manual
> 재생성 전까지 package README와 함께 전달해야 한다.

\toc

# 1. 공통 사항

## 1.1 Assembly

| 항목 | 값 |
|---|---|
| DLL | `LasalMotionControlLib.dll` |
| Namespace | `LasalMotionControlLib` |
| Framework | `.NET Framework 4.8` |
| API version | `0.9.1-preview` |

## 1.2 UNIT 변환

API의 motion 인자는 `Int32` DINT다. API는 UNIT을 자동으로 곱하거나 나누지 않는다.

```text
송신 DINT = 물리값 x PLC application UNIT
수신 물리값 = 수신 DINT / PLC application UNIT
Jerk 송신 DINT = (물리 jerk / 1000) x PLC application UNIT
```

| 값 | API 인자 UNIT | 설명 |
|---|---|---|
| Position / Distance | PLC application UNIT DINT | 예: mm UNIT이 10000이면 1 mm는 10000 |
| Velocity | PLC application UNIT/s DINT | 예: 1 mm/s는 UNIT 10000 기준 10000 |
| Acceleration / Deceleration | PLC application UNIT/s² DINT | PLC에 설정된 application UNIT 사용 |
| Jerk | PLC application UNIT/s³/1000 DINT | 1000 mm/s³는 입력값 1에 UNIT을 곱함 |
| Actual position | PLC application UNIT DINT | 반환값을 UNIT으로 나누어 물리값 계산 |

주요 상수는 다음과 같다. 실제 PLC에 설정된 UNIT이 다르면 그 값을 사용한다.

| Constant | 값 | 의미 |
|---|---:|---|
| `LMC_Units.MM` | 10000 | Millimeter |
| `LMC_Units.M` | 10000000 | Meter |
| `LMC_Units.DEG` | 10000 | Degree |
| `LMC_Units.MMPSEC` | 10000 | Millimeter/second |
| `LMC_Units.RPM` | 1000 | Revolution/minute |

## 1.3 공통 반환값

제어와 motion 명령은 `LMC_Response`를 반환한다.

| Property | Type | 설명 |
|---|---|---|
| `IsSuccess` | `bool` | frame과 command 결과 성공 여부; motion/power/stop 완료를 뜻하지 않음 |
| `Raw` / `Payload` | `byte[]` | 방어 복사된 원본 frame/payload |
| `HeaderStatus` | `ushort` | response envelope 상태 |
| `PayloadLength` | `ushort` | header에 선언된 payload 길이 |
| `HeaderReserved` | `uint` | header reserved 값 |
| `IsFrameValid` | `bool` | header와 command별 payload shape 검증 결과 |
| `HasCommandResult` | `bool` | command status/error field 존재 여부 |
| `CommandStatus` | `ushort` | command/function status |
| `Status` | `ushort` | command result가 있으면 CommandStatus, 없으면 HeaderStatus |
| `ErrorId` | `short` | 반환된 error ID, 정상은 0 |

비동기 메서드는 동일한 결과를 `Task<LMC_Response>`로 반환한다.

## 1.4 Enum 값

| Enum | 사용 값 | 설명 |
|---|---|---|
| `LMC_DIRECTION` | `Shortest` | Absolute / Relative motion |
| `LMC_DIRECTION` | `Positive`, `Negative` | Velocity motion 방향 |
| `LMC_COORD_SYSTEM` | `None`, `Acs`, `Mcs`, `Pcs` | Group 좌표계 |
| `LMC_BUFFER_MODE` | `Aborting`, `Buffered` | 현재 배포 PLC에서 사용하는 buffer mode |
| `LMC_GROUP_TRANSITION_MODE` | `ExactStop`, `ContinuousDirect` | 현재 배포 PLC에서 사용하는 transition mode |

# 2. Connection API

## 2.1 LMCConnection

Connection 객체를 생성한다. 생성만으로 PLC에 연결되지는 않는다.

```csharp
public LMCConnection()
public LMCConnection(LMCConnectionOptions options)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `options` | `LMCConnectionOptions` | - | Connection timeout 설정 |

| Return | 설명 |
|---|---|
| `LMCConnection` | 생성된 connection 객체 |

## 2.2 LMCConnectionOptions

Connection timeout과 callback 검증 값을 설정한다.

| Property | Type | UNIT | Default |
|---|---|---|---:|
| `ConnectTimeoutMilliseconds` | `int` | ms | 3000 |
| `ReceiveTimeoutMilliseconds` | `int` | ms | 3000 |
| `SendTimeoutMilliseconds` | `int` | ms | 3000 |
| `CallbackThreadJoinTimeoutMilliseconds` | `int` | ms | 500 |
| `ValidateCallbackSourceAddress` | `bool` | - | `true` |

## 2.3 RpcInitConnection

PLC TCP 연결, RPC 초기화와 callback 등록을 수행한다.

```csharp
public void RpcInitConnection(
    string remoteAddress,
    int remotePort,
    string localAddress)

public void RpcInitConnection(
    string remoteAddress,
    int remotePort,
    string localAddress,
    int callbackPort,
    uint eventMask)

public Task RpcInitConnectionAsync(
    string remoteAddress,
    int remotePort,
    string localAddress,
    CancellationToken cancellationToken)

public Task RpcInitConnectionAsync(
    string remoteAddress,
    int remotePort,
    string localAddress,
    int callbackPort,
    uint eventMask,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT / 범위 | 설명 |
|---|---|---|---|
| `remoteAddress` | `string` | IPv4 | PLC IP address |
| `remotePort` | `int` | 1~65535 | PLC TCP port |
| `localAddress` | `string` | IPv4 | PLC와 연결되는 PC NIC address |
| `callbackPort` | `int` | 0~65535 | PC UDP callback port, 기본값 5003 |
| `eventMask` | `uint` | Bit mask | Callback event mask, 기본값 `0xFFFFFFFF` |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `void` | 동기 초기화 완료 |
| `Task` | 비동기 초기화 작업 |

## 2.4 CloseConnection

RPC connection과 local TCP/UDP resource를 닫는다.

```csharp
public void CloseConnection()
public Task CloseConnectionAsync(CancellationToken cancellationToken)
public void Dispose()
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `void` | 동기 종료 완료 |
| `Task` | 비동기 종료 작업 |

# 3. Single Axis API

## 3.1 LMCSingleAxis 생성

LASAL axis object name으로 axis reference를 가져온다.

```csharp
public LMCSingleAxis(
    LMCConnection connection,
    string axisName)

public static Task<LMCSingleAxis> CreateAsync(
    LMCConnection connection,
    string axisName,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `connection` | `LMCConnection` | - | 초기화된 RPC connection |
| `axisName` | `string` | ASCII string | PLC에 등록된 axis object name |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMCSingleAxis` | 조회된 axis 객체 |
| `Task<LMCSingleAxis>` | 비동기로 조회된 axis 객체 |

`LMCAxis`는 `LMCSingleAxis`의 호환 이름이다.

## 3.2 PowerOn

Axis Power On을 요청한다.

```csharp
public LMC_Response PowerOn()
public Task<LMC_Response> PowerOnAsync(
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | Power On command 결과 |
| `Task<LMC_Response>` | 비동기 Power On command 결과 |

## 3.3 PowerOff

Axis Power Off를 요청한다.

```csharp
public LMC_Response PowerOff()
public Task<LMC_Response> PowerOffAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Power Off command 결과 |
| `Task<LMC_Response>` | 비동기 Power Off command 결과 |

## 3.4 Reset

Axis error reset을 요청한다.

```csharp
public LMC_Response Reset()
public Task<LMC_Response> ResetAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Reset command 결과 |
| `Task<LMC_Response>` | 비동기 Reset command 결과 |

## 3.5 Stop

현재 axis motion의 정지를 요청한다.

```csharp
public LMC_Response Stop(
    int deceleration,
    int jerk)

public Task<LMC_Response> StopAsync(
    int deceleration,
    int jerk,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `deceleration` | `int` | PLC application UNIT/s² DINT | 정지 감속도 |
| `jerk` | `int` | PLC application UNIT/s³/1000 DINT | 정지 jerk |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | Stop command 결과 |
| `Task<LMC_Response>` | 비동기 Stop command 결과 |

## 3.6 ReadStatus

Axis 상태를 읽는다.

```csharp
public uint ReadStatus()
public uint ReadStatus(out LMC_Response response)

public LMCReadStatusResult ReadStatusResult()
public Task<LMCReadStatusResult> ReadStatusResultAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `uint` | Raw axis state |
| `LMCReadStatusResult` | Axis 상태와 error 정보 |
| `Task<LMCReadStatusResult>` | 비동기 axis 상태와 error 정보 |

## 3.7 GetActualPosition

Axis actual position을 읽는다.

```csharp
public int GetActualPosition()
public int GetActualPosition(out LMC_Response response)

public LMCReadActualPositionResult GetActualPositionResult()
public Task<LMCReadActualPositionResult> GetActualPositionResultAsync(
    CancellationToken cancellationToken)
```

| Return | UNIT | 설명 |
|---|---|---|
| `int` | PLC application UNIT DINT | Raw actual position |
| `LMCReadActualPositionResult` | PLC application UNIT DINT | Actual position과 error 정보 |
| `Task<LMCReadActualPositionResult>` | PLC application UNIT DINT | 비동기 actual position 결과 |

## 3.8 MoveAbsoluteEx

Axis를 absolute position으로 이동시킨다.

```csharp
public LMC_Response MoveAbsoluteEx(
    int position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction = LMC_DIRECTION.Shortest)

public Task<LMC_Response> MoveAbsoluteExAsync(
    int position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `position` | `int` | PLC application UNIT DINT | Absolute target position |
| `velocity` | `int` | PLC application UNIT/s DINT | Velocity |
| `acceleration` | `int` | PLC application UNIT/s² DINT | Acceleration |
| `deceleration` | `int` | PLC application UNIT/s² DINT | Deceleration |
| `jerk` | `int` | PLC application UNIT/s³/1000 DINT | Jerk |
| `direction` | `LMC_DIRECTION` | `Shortest` | 이동 방향 |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | MoveAbsolute command 결과 |
| `Task<LMC_Response>` | 비동기 MoveAbsolute command 결과 |

## 3.9 MoveRelativeEx

현재 위치에서 지정한 distance만큼 이동시킨다.

```csharp
public LMC_Response MoveRelativeEx(
    int distance,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction = LMC_DIRECTION.Shortest)

public Task<LMC_Response> MoveRelativeExAsync(
    int distance,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `distance` | `int` | PLC application UNIT DINT | Signed relative distance |
| `velocity` | `int` | PLC application UNIT/s DINT | Velocity |
| `acceleration` | `int` | PLC application UNIT/s² DINT | Acceleration |
| `deceleration` | `int` | PLC application UNIT/s² DINT | Deceleration |
| `jerk` | `int` | PLC application UNIT/s³/1000 DINT | Jerk |
| `direction` | `LMC_DIRECTION` | `Shortest` | Distance 부호에 따른 이동 |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | MoveRelative command 결과 |
| `Task<LMC_Response>` | 비동기 MoveRelative command 결과 |

## 3.10 MoveVelocityEx

지정한 방향과 속도로 axis를 구동한다.

```csharp
public LMC_Response MoveVelocityEx(
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction)

public Task<LMC_Response> MoveVelocityExAsync(
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMC_DIRECTION direction,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `velocity` | `int` | PLC application UNIT/s DINT | Velocity magnitude |
| `acceleration` | `int` | PLC application UNIT/s² DINT | Acceleration |
| `deceleration` | `int` | 0 | 0을 전달하고 정지는 `Stop` 사용 |
| `jerk` | `int` | PLC application UNIT/s³/1000 DINT | Jerk |
| `direction` | `LMC_DIRECTION` | `Positive`, `Negative` | 이동 방향 |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | MoveVelocity command 결과 |
| `Task<LMC_Response>` | 비동기 MoveVelocity command 결과 |

## 3.11 Drive operation mode와 composite status

physical axis/slave 1..4의 CiA 402 operation mode와 status를 D5 SDO Read로 조회한다.

```csharp
public LMCDriveOperationModeResult GetDriveOperationMode()
public LMCDriveOperationModeResult GetDriveOperationMode(uint timeoutCycles)
public Task<LMCDriveOperationModeResult> GetDriveOperationModeAsync(
    CancellationToken cancellationToken)
public Task<LMCDriveOperationModeResult> GetDriveOperationModeAsync(
    uint timeoutCycles,
    CancellationToken cancellationToken)

public LMCDriveStatus ReadDriveStatus()
public LMCDriveStatus ReadDriveStatus(uint timeoutCycles)
public Task<LMCDriveStatus> ReadDriveStatusAsync(
    CancellationToken cancellationToken)
public Task<LMCDriveStatus> ReadDriveStatusAsync(
    uint timeoutCycles,
    CancellationToken cancellationToken)
```

`GetDriveOperationMode`는 `0x6061:0 Int8/1`을 읽어 typed `Mode`와 signed `RawValue`를
반환한다. unknown/manufacturer-specific 값은 `IsKnownMode=false`여도 `RawValue`에 보존된다.

`ReadDriveStatus`는 LASAL `ReadStatus`, DS402 `0x6041:0 BitField16/2`,
`0x6061:0 Int8/1`을 순차 실행한다. 같은 EtherCAT cycle의 atomic snapshot이 아니므로
`IsAtomicSnapshot`은 항상 false다. `AxisStatus`, `Ds402StatusWord`, `OperationModeResult`와
software/hardware/DS402 limit indication을 source별로 확인한다.

`timeoutCycles`는 각 PLC SDO operation timeout이며 기본값은 1000 cycles다. library의
terminal status poll 간격은 capability의 `BaseCycleTimeUs`에서 계산하며 최대 poll 수는
`timeoutCycles+32`다. `BaseCycleTimeUs=0`이면 요청 전에 실패한다. terminal 실패는
`LMCSdoReadOperationException`, PC poll 한계는 `LMCSdoReadPollingTimeoutException`으로
ticket/status를 보존한다. 제출 뒤 async cancellation은 ticket을 포함한
`LMCSdoReadWaitCanceledException`을 발생시키고 PC wait만 중단하며, 이미 제출한 PLC
ticket을 자동 cancel하지 않는다. 이미 진행 중인 status RPC는 응답을 끝까지 수신한 뒤
취소를 보고하므로 connection은 유지되고 보존된 ticket을 다시 조회할 수 있다.

# 4. Group API

## 4.1 LMCGroupAxis 생성

LASAL group object name으로 group reference를 가져온다.

```csharp
public LMCGroupAxis(
    LMCConnection connection,
    string groupName)

public static Task<LMCGroupAxis> CreateAsync(
    LMCConnection connection,
    string groupName,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `connection` | `LMCConnection` | - | 초기화된 RPC connection |
| `groupName` | `string` | ASCII string | PLC에 등록된 group object name |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMCGroupAxis` | 조회된 group 객체 |
| `Task<LMCGroupAxis>` | 비동기로 조회된 group 객체 |

`LMCGroup`은 `LMCGroupAxis`의 호환 이름이다.

Group Power/Enable/Reset/Stop/Move의 성공 ACK는 method 호출 접수 결과다. 완료
상태는 `GroupReadStatusResult`와 필요 시 `GroupReadActualPosition`으로 확인한다.
현재 SetKin/Lock/Move는 X/Y/Z/U 축 1~4에만 적용된다. 이것은 9축 동시 group
interpolation API가 아니다.

## 4.2 GetGroupMembersInfo

Group에 연결된 axis member 정보를 읽는다.

```csharp
public LMC_Response GetGroupMembersInfo()

public LMCGroupMembersInfoResult GetGroupMembersInfoResult()
public Task<LMCGroupMembersInfoResult> GetGroupMembersInfoResultAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Member 조회 command 결과 |
| `LMCGroupMembersInfoResult` | Axis count, reference, device ID와 axis name |
| `Task<LMCGroupMembersInfoResult>` | 비동기 member 정보 결과 |

## 4.3 GroupPowerOn

Group member axis의 Power On을 요청한다.

```csharp
public LMC_Response GroupPowerOn()
public Task<LMC_Response> GroupPowerOnAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Group Power On command 결과 |
| `Task<LMC_Response>` | 비동기 Group Power On command 결과 |

## 4.4 GroupPowerOff

Group member axis의 Power Off를 요청한다.

```csharp
public LMC_Response GroupPowerOff()
public Task<LMC_Response> GroupPowerOffAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Group Power Off command 결과 |
| `Task<LMC_Response>` | 비동기 Group Power Off command 결과 |

## 4.5 GroupEnable

Group motion profile을 lock한다.

```csharp
public LMC_Response GroupEnable()
public Task<LMC_Response> GroupEnableAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Profile lock command 결과 |
| `Task<LMC_Response>` | 비동기 profile lock 결과 |

## 4.6 GroupDisable

Group motion profile을 unlock한다.

```csharp
public LMC_Response GroupDisable()
public Task<LMC_Response> GroupDisableAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Profile unlock command 결과 |
| `Task<LMC_Response>` | 비동기 profile unlock 결과 |

## 4.7 GroupReset

Group error reset을 요청한다.

```csharp
public LMC_Response GroupReset()
public Task<LMC_Response> GroupResetAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `LMC_Response` | Group Reset command 결과 |
| `Task<LMC_Response>` | 비동기 Group Reset command 결과 |

## 4.8 GroupStop

현재 group motion의 정지를 요청한다.

```csharp
public LMC_Response GroupStop(
    int deceleration,
    int jerk)

public Task<LMC_Response> GroupStopAsync(
    int deceleration,
    int jerk,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `deceleration` | `int` | Group application UNIT/s² DINT | 정지 감속도 |
| `jerk` | `int` | Group application UNIT/s³/1000 DINT | 정지 jerk |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | Group Stop command 결과 |
| `Task<LMC_Response>` | 비동기 Group Stop command 결과 |

`deceleration`/`jerk` 조합은 PLC 계약에 맞게 RPC 전에 검사한다. success ACK는
입력 검증, robot client 연결과 `StopMove(Mode:=3)` dispatch를 뜻하며 정지 완료가
아니다. `StopMove()` 반환 `StopCmdNo`는 오류 코드가 아니라 정지가 끝날 buffer
command index다. 실제 완료와 profile error는 `GroupReadStatusResult`로 확인한다.

## 4.9 GroupReadStatus

Group power와 profile 상태를 읽는다.

```csharp
public uint GroupReadStatus()
public uint GroupReadStatus(out LMC_Response response)

public LMCGroupReadStatusResult GroupReadStatusResult()
public Task<LMCGroupReadStatusResult> GroupReadStatusResultAsync(
    CancellationToken cancellationToken)
```

| Return | 설명 |
|---|---|
| `uint` | Raw group state |
| `LMCGroupReadStatusResult` | Group power, profile 상태와 error 정보 |
| `Task<LMCGroupReadStatusResult>` | 비동기 group 상태 결과 |

## 4.10 GroupReadActualPosition

Group actual position을 읽는다.

```csharp
public LMCGroupReadActualPositionResult GroupReadActualPosition(
    LMC_COORD_SYSTEM coordinateSystem)

public Task<LMCGroupReadActualPositionResult> GroupReadActualPositionAsync(
    LMC_COORD_SYSTEM coordinateSystem,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `coordinateSystem` | `LMC_COORD_SYSTEM` | Enum | `None` 또는 `Acs` member-slot alias |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | UNIT | 설명 |
|---|---|---|
| `LMCGroupReadActualPositionResult` | Group application UNIT DINT | slot 1..9 member position, slot 10..16 zero와 error 정보 |
| `Task<LMCGroupReadActualPositionResult>` | Group application UNIT DINT | 비동기 group position 결과 |

현재 adapter는 no-CalcModel static identity에서 `None/Acs`를 같은
`GetRobotPosition(CoordSystem:=0)` member-slot read alias로 처리한다. `Mcs/Pcs`는
지원하지 않아 C#에서 `NotSupportedException`이 발생하며, 구 SDK 요청은 PLC가
`ErrorId=-7`로 거부한다. slot 1..9는 software group member 순서이고 slot
10..16은 0이다. `Acs` alias의 실물 동등성은 PLC 시험이 남아 있다.

## 4.11 SetKinTransformCartesian4Axis

4개 axis를 Cartesian X/Y/Z/U로 설정한다.

```csharp
public LMC_Response SetKinTransformCartesian4Axis(
    LMCSingleAxis axisX,
    LMCSingleAxis axisY,
    LMCSingleAxis axisZ,
    LMCSingleAxis axisU)

public Task<LMC_Response> SetKinTransformCartesian4AxisAsync(
    LMCSingleAxis axisX,
    LMCSingleAxis axisY,
    LMCSingleAxis axisZ,
    LMCSingleAxis axisU,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `axisX` | `LMCSingleAxis` | - | Cartesian X axis |
| `axisY` | `LMCSingleAxis` | - | Cartesian Y axis |
| `axisZ` | `LMCSingleAxis` | - | Cartesian Z axis |
| `axisU` | `LMCSingleAxis` | - | Cartesian U axis |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | Kinematic transform command 결과 |
| `Task<LMC_Response>` | 비동기 transform command 결과 |

이 helper는 exact X/Y/Z/U identity payload만 만든다. generic/dynamic kinematic
transform 계산이나 profile lock을 수행하지 않는다.

## 4.12 MoveLinearAbsoluteEx

Group을 Cartesian absolute position으로 linear 이동시킨다.

```csharp
public LMC_Response MoveLinearAbsoluteEx(
    int[] position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk)

public LMC_Response MoveLinearAbsoluteEx(
    int[] position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMCGroupMotionOptions options)

public Task<LMC_Response> MoveLinearAbsoluteExAsync(
    int[] position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    CancellationToken cancellationToken)

public Task<LMC_Response> MoveLinearAbsoluteExAsync(
    int[] position,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMCGroupMotionOptions options,
    CancellationToken cancellationToken)
```

| Parameter | Type | UNIT | 설명 |
|---|---|---|---|
| `position` | `int[]` | Group application UNIT DINT | wire 16개; 현재 PLC는 X/Y/Z/U slot 1..4만 사용하고 5..16은 0이어야 함 |
| `velocity` | `int` | Group application UNIT/s DINT | Path velocity |
| `acceleration` | `int` | Group application UNIT/s² DINT | Path acceleration |
| `deceleration` | `int` | Group application UNIT/s² DINT | Path deceleration |
| `jerk` | `int` | Group application UNIT/s³/1000 DINT | Path jerk |
| `options` | `LMCGroupMotionOptions` | - | Coordinate, transition, buffer와 execute 설정 |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | 설명 |
|---|---|
| `LMC_Response` | MoveLinearAbsolute command 결과 |
| `Task<LMC_Response>` | 비동기 MoveLinearAbsolute command 결과 |

현재 C#과 PLC가 함께 허용하는 범위는 position slot 1..4 X/Y/Z/U, 나머지 0,
양수 velocity/acceleration/deceleration, 0 이상 jerk, coordinate `None`, transition
`ExactStop`/`ContinuousDirect`, buffer `Aborting`/`Buffered`, `Execute=true`다.
정의됐지만 지원하지 않는 option은 RPC 전에 `NotSupportedException`으로 거부한다.

## 4.13 MoveLinearRelativeEx

Group profile의 마지막 buffered target을 기준으로 Cartesian relative distance를
원자적으로 적재한다. PC에서 현재 위치를 읽어 absolute target으로 변환하지 않는다.

```csharp
public LMCAdminResponse MoveLinearRelativeEx(
    int[] distance,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk)

public LMCAdminResponse MoveLinearRelativeEx(
    int[] distance,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMCGroupMotionOptions options)

public Task<LMCAdminResponse> MoveLinearRelativeExAsync(
    int[] distance,
    int velocity,
    int acceleration,
    int deceleration,
    int jerk,
    LMCGroupMotionOptions options,
    CancellationToken cancellationToken)
```

`distance`와 dynamics/options의 UNIT 및 허용 범위는 absolute move와 같다. 현재 PLC는
X/Y/Z/U slot 1..4만 사용하고 slot 5..16=0, coordinate `None`, transition
`ExactStop`/`ContinuousDirect`, buffer `Aborting`/`Buffered`, `Execute=true`만 허용한다.

반환형은 `LMCAdminResponse`다. valid command rejection은
`LMCAdminCommandException.Response`에 Admin detail과 native error를 보존한다. success는
`MoveRelativeCoord`가 profile queue에 명령을 수락했다는 뜻이며 완료가 아니다. 이후
`GroupReadStatusResult[Async]`에서 InPosition/profile error를 확인한다.

일반 overload는 같은 session의 `0x7D00` capability를 먼저 확인한다. Stop/PowerOff
우선순위 gate가 있는 UI는 gate 밖에서 `GetCapabilitiesAsync`를 수행한 뒤 아래 prepared
overload를 gate 안에서 사용한다. 전달한 capability는 같은 `LMCConnection`과 session,
feature/group reference가 아니면 wire 송신 전에 거부된다.

```csharp
LMCAdminCapabilities capabilities =
    await connection.Admin.GetCapabilitiesAsync(cancellationToken);

LMCAdminResponse accepted = await group.MoveLinearRelativeExAsync(
    distance,
    velocity,
    acceleration,
    deceleration,
    jerk,
    options,
    capabilities,
    cancellationToken);
```

prepared capability를 받는 sync overload도 같은 인자 순서로 제공한다.

## 4.14 LMCGroupMotionOptions

MoveLinearAbsolute/Relative의 좌표계와 motion mode를 설정한다.

| Property | Type | UNIT / Default | 설명 |
|---|---|---|---|
| `CoordinateSystem` | `LMC_COORD_SYSTEM` | `None` | Coordinate system |
| `TransitionMode` | `LMC_GROUP_TRANSITION_MODE` | `ExactStop` | Transition mode |
| `BufferMode` | `LMC_BUFFER_MODE` | `Aborting` | Buffer mode |
| `Execute` | `bool` | `true` | Command execute |

현재 PLC adapter가 승인하는 조합은 `CoordinateSystem=None`,
`TransitionMode=ExactStop/ContinuousDirect`, `BufferMode=Aborting/Buffered`,
`Execute=true`뿐이다. public enum에 다른 값이 있어도 현재 PLC 지원을 뜻하지 않는다.

# 5. Return Type

## 5.1 LMCReadStatusResult

`ReadStatusResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `IsReadSuccessful` | `bool` | - | RPC/function read 성공; native axis error 존재 여부와 분리 |
| `State` | `uint` | Bit field | Raw axis state |
| `FunctionStatus` | `ushort` | Bit field | MotionLib function status |
| `HasCommandError` | `bool` | - | FunctionStatus command-error bit 여부 |
| `IsPowerOn` | `bool` | - | Axis Power On 상태 |
| `IsReferenced` | `bool` | - | Home / Reference 완료 상태 |
| `IsStandstill` | `bool` | - | Standstill 상태 |
| `AxisErrorId` | `ushort` | Error ID | Axis error |
| `HasAxisError` | `bool` | - | `AxisErrorId != 0` |
| `AxisErrorFlags` | `ushort` | Bit field | raw LASAL `_LMCAXIS_ERROR`; DS402 statusword bit가 아님 |
| `StatusWord` | `ushort` | Reserved | 현재 LASAL adapter는 0을 반환하며 DS402 statusword로 사용하지 않음 |
| `ErrorId` | `short` | Error ID | Command error |

## 5.2 LMCReadActualPositionResult

`GetActualPositionResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `PositionRaw` | `int` | PLC application UNIT DINT | Actual position |
| `FunctionStatus` | `ushort` | Bit field | MotionLib function status |
| `HasCommandError` | `bool` | - | FunctionStatus command-error bit 여부 |
| `ErrorId` | `short` | Error ID | Command error |

## 5.3 LMCGroupReadStatusResult

`GroupReadStatusResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `State` | `uint` | Bit field | Raw group state |
| `IsPowerOn` | `bool` | - | Group power 상태 |
| `IsStandby` | `bool` | - | Profile locked 상태 |
| `IsEnabled` | `bool` | - | `IsStandby` 호환 alias; servo power와 다름 |
| `IsDisabled` | `bool` | - | Profile unlocked 상태 |
| `FunctionStatus` | `ushort` | Bit field | MotionLib function status |
| `HasCommandError` | `bool` | - | FunctionStatus command-error bit 여부 |
| `GroupErrorId` | `ushort` | Error ID | Group / profile error |
| `ErrorId` | `short` | Error ID | Command error |

## 5.4 LMCGroupReadActualPositionResult

`GroupReadActualPosition`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `CoordinateSystem` | `LMC_COORD_SYSTEM` | Enum | 요청에 사용한 좌표계의 PC-side echo; PLC 응답 필드가 아님 |
| `PositionsRaw` | `int[16]` | Group application UNIT DINT | slot 1..9 software member position, slot 10..16 zero |
| `FunctionStatus` | `ushort` | Bit field | MotionLib function status |
| `HasCommandError` | `bool` | - | FunctionStatus command-error bit 여부 |
| `ErrorId` | `short` | Error ID | Command error |

현재 tracked PLC source는 `_LMCPROF_POS`의 Pos1..Pos9를 response slot 1..9에
복사하고 slot 10..16을 0으로 유지한다. Move/SetKin/Lock의 physical 4축 제한은
그대로다.

## 5.5 LMCGroupMembersInfoResult

`GetGroupMembersInfoResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `AxisCount` | `byte` | Count | Group member 수; 현재 tracked source는 9 |
| `Members` | `LMCGroupMemberInfo[]` | - | Member 정보 배열 |
| `AxisReferences` / `DeviceIds` / `AxisNames` | 배열 | - | 방어 복사된 원본 member 배열 |
| `FunctionStatus` | `ushort` | Bit field | MotionLib function status |
| `HasCommandError` | `bool` | - | FunctionStatus command-error bit 여부 |
| `ErrorId` | `short` | Error ID | Command error |

`LMCGroupMemberInfo`의 반환 필드는 다음과 같다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Index` | `int` | Index | 0-based member index |
| `AxisReference` | `ushort` | Reference | Axis reference |
| `DeviceId` | `ushort` | Device ID | PLC device ID |
| `AxisName` | `string` | ASCII string | LASAL axis object name |

## 5.6 LMCAdminResponse

LASAL-local Admin 명령의 공통 16-byte response를 보존한다.

| Property | Type | 설명 |
|---|---|---|
| `TransportResponse` | `LMC_Response` | outer transport frame |
| `SchemaVersion` / `ResponseFlags` | `ushort` | Admin schema와 reserved flags |
| `CommandStatus` | `ushort` | 0 success, 1 domain rejection |
| `ErrorId` | `short` | Admin `-31000`, positive GroupProfile code 또는 adapter fallback `-6` |
| `RequestId` | `uint` | request echo |
| `DetailCode` / `DetailCodeValue` | enum / `uint` | typed/raw Admin detail |
| `IsSuccess` | `bool` | status/error/detail이 모두 success인지 여부 |

# 6. Admin과 Diagnostics facade

## 6.1 Admin capability와 semantic parameter read

연결된 `LMCConnection.Admin`에서 LASAL-local read-only API를 사용한다. 각 parameter
read는 먼저 `0x7D00` capability와 허용 mask를 확인한다.

```csharp
LMCAdminCapabilities capabilities = connection.Admin.GetCapabilities();

LMCAxisParameterResult axisParameter = connection.Admin.ReadAxisParameter(
    axis,
    LMCAxisParameterKey.MaxVelocity);

LMCGroupParametersResult groupParameters = connection.Admin.ReadGroupParameters(
    group,
    LMCGroupParameterSelection.All);
```

async 메서드는 `GetCapabilitiesAsync`, `ReadAxisParameterAsync`,
`ReadGroupParametersAsync`이며 `CancellationToken`을 받는다. 축/group 객체 overload 외에
`ushort` reference overload도 있다. 다른 connection 또는 reconnect 전 stale 객체는
거부한다.

axis read 제한:

- physical AxisReference 1..4
- key: `SoftwareMinPosition`, `SoftwareMaxPosition`,
  `EndPositionToleranceWindow`, `MaxVelocity`, `MaxAcceleration`, `ReferencePosition`
- 한 호출에 한 key, 반환 value type `Int32`

`EndPositionToleranceWindow`는 profile의 in-position 상태가 아니라 축
end-position tolerance parameter다. 결과의 `Unit`과 `Value`를 함께 확인한다.

group read 제한:

- GroupReference `0x0100`
- selection: `PathVelocityLimit`, `PathAccelerationLimit`, `JerkTime` 또는 조합
- 최대 3개; unit은 각각 application UNIT/s, application UNIT/s2, milliseconds

지원하지 않는 PLC/schema/capability는 `LMCAdminNotSupportedException` 또는
`NotSupportedException`, valid admin error response는 `LMCAdminCommandException`으로
보고하며 `Response`에 status/error/detail을 보존한다. 이 API는 read-only이고 motion을
생성하지 않는다.

## 6.2 PI alias와 Bulk builder/reader

먼저 `GetSignalCatalog`로 얻은 같은 boot/map의 catalog를 사용한다.

```csharp
LMCSignalValue value = connection.Diagnostics.ReadPI(
    catalog,
    "axis1.actual_position");

LMCPIBulkBuilder builder = connection.Diagnostics.CreatePIBulkBuilder(catalog);
builder.AddEntry("axis1.actual_position");
builder.AddEntry("axis2.actual_position");
LMCPIBulkReader reader = builder.Configure();
LMCBulkSnapshot snapshot = reader.Upload();
LMCSignalValueEntry entry = reader.GetEntry("axis1.actual_position");
reader.Release();
```

builder는 readable catalog entry, 중복, 최대 32개와 exact `MapRevision`을 검사한다.
Configure 성공 뒤 builder는 frozen된다. `GetEntry/TryGetEntry`는 마지막 성공 Upload의
snapshot을 조회하며 새 PLC read를 수행하지 않는다. 별도 compatibility wire는 없고
D1 PI Read와 D2 Bulk command를 재사용한다. sync 메서드에는 대응하는
`ConfigureAsync`, `UploadAsync`, `ReadStatusAsync`, `ReleaseAsync`가 있다.

## 6.3 Project-local error catalog

```csharp
LMCErrorDescription description;
if (LMCErrorCatalog.TryDescribe(
        LMCErrorDomain.AdapterCommand,
        response.ErrorId,
        out description))
{
    Console.WriteLine(description.Symbol);
    Console.WriteLine(description.Resolution);
}
```

지원 domain은 `AdapterCommand`, `AdminDetail`, `DiagnosticsDetail`, `GroupProfile`이다.
같은 숫자라도
domain마다 의미가 다르므로 domain을 추측하지 않는다. 반환 객체는 `Description`,
`Resolution`, `CatalogVersion`, `SourceVersion`을 제공한다. unknown domain/value는 false를
반환한다. 이 catalog는 현재 project-local 계약이며 Elmo Maestro Personality 전체 error
database가 아니다.

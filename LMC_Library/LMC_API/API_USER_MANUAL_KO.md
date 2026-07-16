# LASAL Motion Control API 기능 설명서

문서 버전: 1.2
적용 API: LasalMotionControlLib 0.9.1-preview
대상 환경: Windows, .NET Framework 4.8
발행일: 2026-07-16

\pagebreak

# 개정 이력

| 문서 버전 | 날짜 | 내용 |
|---|---|---|
| 1.0 | 2026-07-15 | 최초 작성 |
| 1.1 | 2026-07-16 | 공개 API 레퍼런스 작성 |
| 1.2 | 2026-07-16 | API 기능, 인자 UNIT과 반환값 중심으로 간소화 |

이 문서는 `LasalMotionControlLib.dll`의 API 기능과 호출 인자, UNIT, 반환값만 설명한다.

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
| `IsSuccess` | `bool` | `true`: 명령 성공, `false`: 명령 실패 |
| `Status` | `ushort` | 반환된 command status |
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
| `coordinateSystem` | `LMC_COORD_SYSTEM` | Enum | 읽을 좌표계 |
| `cancellationToken` | `CancellationToken` | - | 비동기 호출 취소 token |

| Return | UNIT | 설명 |
|---|---|---|
| `LMCGroupReadActualPositionResult` | Group application UNIT DINT | 16개 actual coordinate와 error 정보 |
| `Task<LMCGroupReadActualPositionResult>` | Group application UNIT DINT | 비동기 group position 결과 |

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
| `position` | `int[]` | Group application UNIT DINT | 1~16개 absolute coordinate |
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

## 4.13 LMCGroupMotionOptions

MoveLinearAbsolute의 좌표계와 motion mode를 설정한다.

| Property | Type | UNIT / Default | 설명 |
|---|---|---|---|
| `CoordinateSystem` | `LMC_COORD_SYSTEM` | `None` | Coordinate system |
| `TransitionMode` | `LMC_GROUP_TRANSITION_MODE` | `ExactStop` | Transition mode |
| `BufferMode` | `LMC_BUFFER_MODE` | `Aborting` | Buffer mode |
| `Execute` | `bool` | `true` | Command execute |

# 5. Return Type

## 5.1 LMCReadStatusResult

`ReadStatusResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `State` | `uint` | Bit field | Raw axis state |
| `IsPowerOn` | `bool` | - | Axis Power On 상태 |
| `IsReferenced` | `bool` | - | Home / Reference 완료 상태 |
| `IsStandstill` | `bool` | - | Standstill 상태 |
| `AxisErrorId` | `ushort` | Error ID | Axis error |
| `StatusWord` | `ushort` | Bit field | DS402 statusword |
| `ErrorId` | `short` | Error ID | Command error |

## 5.2 LMCReadActualPositionResult

`GetActualPositionResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `PositionRaw` | `int` | PLC application UNIT DINT | Actual position |
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
| `IsDisabled` | `bool` | - | Profile unlocked 상태 |
| `GroupErrorId` | `ushort` | Error ID | Group / profile error |
| `ErrorId` | `short` | Error ID | Command error |

## 5.4 LMCGroupReadActualPositionResult

`GroupReadActualPosition`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `CoordinateSystem` | `LMC_COORD_SYSTEM` | Enum | 반환 좌표계 |
| `PositionsRaw` | `int[16]` | Group application UNIT DINT | Actual coordinate 배열 |
| `ErrorId` | `short` | Error ID | Command error |

## 5.5 LMCGroupMembersInfoResult

`GetGroupMembersInfoResult`의 반환값이다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Response` | `LMC_Response` | - | 원본 command response |
| `IsSuccess` | `bool` | - | 전체 결과 성공 여부 |
| `AxisCount` | `byte` | Count | Group member 수 |
| `Members` | `LMCGroupMemberInfo[]` | - | Member 정보 배열 |
| `ErrorId` | `short` | Error ID | Command error |

`LMCGroupMemberInfo`의 반환 필드는 다음과 같다.

| Property | Type | UNIT | 설명 |
|---|---|---|---|
| `Index` | `int` | Index | 0-based member index |
| `AxisReference` | `ushort` | Reference | Axis reference |
| `DeviceId` | `ushort` | Device ID | PLC device ID |
| `AxisName` | `string` | ASCII string | LASAL axis object name |

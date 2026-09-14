# LMC_Home 범용 구현 설계 - 2026-09-14

## 1. 문서 지위

이 문서는 `dev@c8fa8bb6cd063fbb2f694561ef787ca7b223fdb4`를 기준으로 작성한
`LMC_Home` 범용화 **구현 목표 설계**다.

현재 `dev` source의 `LMC_Home`은 아직 범용 Home이 아니다.
현재 구현은 `LMCHomeSemanticMode.CurrentPositionZero` 하나만 허용하고,
`ExpectedActualPosition`을 stale-read guard로 사용한 뒤
`SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=0)`을 수행하는
정적 좌표 재정의 기능이다.

따라서 이 문서는 현재 구현 완료 상태를 설명하는 문서가 아니라 다음 구현 changeset의 기준이다.
source/API/wire/PLC/WPF가 이 문서대로 반영되기 전까지 current runtime 의미는 기존
`CurrentPositionZero` 계약을 따른다.

`LMC_HomeDS402`는 별도 기능이다. DS402 Homing Method와 `0x607C/0x6098/0x6099/0x609A`를
직접 다루는 계약은 `DS402_HOME_IMPLEMENTATION_DESIGN_20260914.md`를 따른다.

---

## 2. 설계 목표

목표는 Elmo Maestro/MMCLib의 `MMC_Home` 의미를 현재 SIGMATEK/LASAL API 구조에 맞게
범용 public API로 제공하는 것이다.

핵심 목표는 다음과 같다.

1. Home 완료 시 부여할 `Position`을 호출자가 입력할 수 있어야 한다.
2. 이동 Home은 `Velocity`, `Acceleration`, `DistanceLimit`, `TorqueLimit`을 입력할 수 있어야 한다.
3. `HomingMode`, `Direction`, `SwitchMode`, `BufferMode`, `Timeout`을 typed parameter로 전달한다.
4. 현재 DS402 Home과 동일하게 `Prepare -> Start once -> Outcome -> Retire` lifecycle을 사용한다.
5. write boundary 이후 original Start를 자동 replay하지 않는다.
6. 기존 `CurrentPositionZero` 호출자를 즉시 깨지 않도록 v1 호환 경로를 유지한다.
7. wire에는 MMCLib/LASAL native enum 숫자를 직접 노출하지 않고 project semantic enum만 사용한다.
8. moving Home의 완료 판정과 static Direct Home의 완료 판정을 분리한다.
9. `LMC_Home`과 `LMC_HomeDS402`의 책임을 섞지 않는다.

production release 판정은 이 설계문서 작성과 별개다. moving Home은 실제 switch/limit/index 배선과
이동 방향을 포함하므로 implementation 완료 후에도 mode별 실축 qualification이 필요하다.

---

## 3. Elmo `MMC_Home` 기준 계약

Maestro API의 `MMC_HOME_IN`은 다음 구조를 가진다.

```text
ucExecute
Position
Acceleration
Velocity
DistanceLimit
TorqueLimit
HomingMode
BufferMode
Direction
SwitchMode
TimeLimit
```

원본 문서의 핵심 의미는 다음과 같다.

- `Position`: reference signal이 검출될 때 축에 부여할 절대 좌표
- `Velocity`: Home 검색 최대 속도
- `Acceleration`: Home 검색 가속도
- `DistanceLimit`: 검색 이동 거리 제한, `0`은 제한 없음
- `TorqueLimit`: torque 제한, `0`은 제한 없음
- `HomingMode`: `MC_ABS_SWITCH`, `MC_LIMIT_SWITCH`, `MC_REF_PULSE`, `MC_DIRECT`, `MC_BLOCK`
- `Direction`: Home 검색 방향
- `SwitchMode`: sensor 완료 조건
- `TimeLimit`: Home 완료 watchdog, ms

`MC_DIRECT`는 물리 검색 이동 없이 사용자 기준 좌표를 강제로 설정하는 static homing이다.
`MC_ABS_SWITCH`, `MC_LIMIT_SWITCH`, `MC_REF_PULSE`, `MC_BLOCK`은 실제 축 이동이 발생할 수 있다.

Maestro `MMC_Home`은 Standstill에서 시작하는 generic Home 기능이며, Homing 종료 후 Standstill로
복귀하는 계약이다.

원본 문서에는 `MC_ABORTING_MODE`가 Homing에서 지원되지 않는다고 명시되어 있다. 따라서 본 프로젝트
v2 첫 구현은 BufferMode를 `Buffered` 하나로 제한한다. blending 계열은 별도 qualification 없이
활성화하지 않는다.

---

## 4. Public API 목표 구조

### 4.1 신규 generic parameter

신규 호출자는 다음 typed parameter를 사용한다.

```csharp
public sealed class LMCHomeParameters
{
    public int Position { get; }
    public int Velocity { get; }
    public int Acceleration { get; }
    public int DistanceLimit { get; }
    public int TorqueLimit { get; }

    public LMCHomeMode HomingMode { get; }
    public LMCHomeBufferMode BufferMode { get; }
    public LMCHomeDirection Direction { get; }
    public LMCHomeSwitchMode SwitchMode { get; }

    public int TimeoutMilliseconds { get; }
}
```

모든 motion numeric field는 현재 project wire 원칙대로 signed 32-bit DINT를 사용한다.
단위는 다음과 같이 고정한다.

| Parameter | project 단위 | 의미 |
|---|---|---|
| `Position` | application unit | reference 검출 시 부여할 절대 좌표 |
| `Velocity` | application unit/s | Home 검색 속도 |
| `Acceleration` | application unit/s^2 | Home 검색 가속도 |
| `DistanceLimit` | application unit | 검색 이동 거리 제한, `0`은 제한 없음 |
| `TorqueLimit` | project torque unit | torque 제한, `0`은 제한 없음 |
| `TimeoutMilliseconds` | ms | Home watchdog |

`Position`은 `MoveAbsolute`의 목적지와 동일한 의미가 아니다. moving Home에서는 reference event가
발생한 순간 적용되는 좌표 기준값이다. 감속 또는 후속 Home sequence 때문에 Home 종료 시점의
ActualPosition이 항상 `Position`과 exact 일치한다고 가정하지 않는다.

### 4.2 project semantic enum

wire enum 값은 native MMCLib/LASAL enum 번호와 분리한다.

```csharp
public enum LMCHomeMode : ushort
{
    Direct = 1,
    AbsoluteSwitch = 2,
    LimitSwitch = 3,
    ReferencePulse = 4,
    Block = 5
}

public enum LMCHomeBufferMode : ushort
{
    Buffered = 1
}

public enum LMCHomeDirection : ushort
{
    NotApplicable = 0,
    Positive = 1,
    Negative = 2,
    SwitchPositive = 3,
    SwitchNegative = 4
}

public enum LMCHomeSwitchMode : ushort
{
    NotApplicable = 0,
    On = 1,
    Off = 2,
    EdgeOn = 3,
    EdgeOff = 4,
    EdgeSwitchPositive = 5,
    EdgeSwitchNegative = 6
}
```

위 숫자는 **project wire semantic value**다. native enum 숫자가 아니다. PLC adapter가 native API를
호출할 때만 해당 플랫폼 enum으로 변환한다.

### 4.3 public lifecycle

public API 이름은 기존 구조를 유지한다.

```text
PrepareLMC_Home(...)
  -> LMC_Home(...) / LMC_HomeAsync(...)
  -> ReadLMC_HomeOutcome(...) / ReadLMC_HomeOutcomeAsync(...)
  -> RetireLMC_HomeOutcome(...) / RetireLMC_HomeOutcomeAsync(...)
```

호출 예시는 다음 형태다.

```csharp
var parameters = new LMCHomeParameters(
    position,
    velocity,
    acceleration,
    distanceLimit,
    torqueLimit,
    homingMode,
    LMCHomeBufferMode.Buffered,
    direction,
    switchMode,
    timeoutMilliseconds);

var prepared = axis.PrepareLMC_Home(
    parameters,
    adminCapabilities,
    diagnosticCapabilities,
    LMCHomeExecuteToken.Create());

var ack = await axis.LMC_HomeAsync(prepared, cancellationToken);
var outcome = await axis.ReadLMC_HomeOutcomeAsync(
    prepared.RecoveryKey,
    adminCapabilities,
    diagnosticCapabilities,
    cancellationToken);

if (outcome.IsTerminal)
{
    await axis.RetireLMC_HomeOutcomeAsync(
        outcome,
        adminCapabilities,
        diagnosticCapabilities,
        cancellationToken);
}
```

Start ACK는 접수 증거일 뿐 terminal completion 증거가 아니다.

---

## 5. v1 `CurrentPositionZero` 호환 정책

현재 SDK에는 다음 호출자가 존재할 수 있다.

```csharp
new LMCHomeParameters(expectedActualPosition, timeoutMilliseconds)
PrepareLMC_Home(expectedActualPosition, timeoutMilliseconds, ...)
```

이 호출은 즉시 제거하지 않는다.

호환 정책은 다음과 같다.

- 기존 constructor/overload는 `[Obsolete]` 대상으로 유지한다.
- 기존 호출은 wire v1 `CurrentPositionZero`로 계속 전송한다.
- v1은 기존 exact `ExpectedActualPosition` stale-read guard와 `Position=0`을 그대로 유지한다.
- 신규 full `LMCHomeParameters` constructor만 wire v2 GenericHome을 사용한다.
- v1 packet을 v2 의미로 묵시적으로 재해석하지 않는다.
- 신규 WPF Home editor는 v2를 사용한다.

v2의 `Direct + Position=0`은 기능적으로 CurrentPositionZero와 유사하지만 v1의 exact stale-read CAS까지
동일한 계약은 아니다. 따라서 binary/source compatibility 기간에는 두 계약을 명시적으로 구분한다.

---

## 6. Parameter validation

SDK와 PLC는 같은 validation matrix를 각각 수행한다. UI validation만으로 안전 조건을 대신하지 않는다.

### 6.1 공통 조건

- physical axis만 허용
- current session / DiagnosticsBuild / BootId / MapRevision 일치
- AxisHome capability 필요
- Home 시작 시 axis는 Standstill
- active axis error가 없어야 함
- moving mode는 servo/operation enabled 상태 필요
- `TimeoutMilliseconds > 0`
- v2 초기 권장 범위는 `100..300000 ms`
- `BufferMode = Buffered`만 허용
- reserved enum 값은 fail-closed

### 6.2 mode별 조건

| Mode | Position | Velocity | Acceleration | DistanceLimit | TorqueLimit | Direction | SwitchMode |
|---|---:|---:|---:|---:|---:|---|---|
| `Direct` | any DINT | `0` | `0` | `0` | `0` | `NotApplicable` | `NotApplicable` |
| `AbsoluteSwitch` | any DINT | `>0` | `>0` | `0` | `0` | Positive / Negative / SwitchPositive / SwitchNegative | On / Off / EdgeOn / EdgeOff / EdgeSwitchPositive / EdgeSwitchNegative |
| `LimitSwitch` | any DINT | `>0` | `>0` | `0` | `0` | Positive / Negative | `NotApplicable` |
| `ReferencePulse` | any DINT | `>0` | `>0` | `0` | `0` | Positive / Negative | `NotApplicable` |
| `Block` | any DINT | `>0` | `>0` | `0` 또는 signed limit | `>=0` | Positive / Negative | `NotApplicable` |

`TorqueLimit`의 nonzero 활성화는 LASAL/native torque unit mapping이 확인된 뒤 허용한다. mapping이
미확정인 changeset에서는 SDK/PLC가 nonzero 값을 fail-closed 해야 한다.

`DistanceLimit`도 native backend가 실제 travel stop에 사용한다는 증거가 없는 mode에서는 `0`으로
제한한다.

---

## 7. Wire contract v2

### 7.1 command ID

기존 lifecycle ID를 유지한다.

| 단계 | Command | 의미 |
|---|---:|---|
| Start | `0x7D13` | LMC_Home Start once |
| Outcome | `0x7D18` | retained outcome query |
| Retire | `0x7D19` | exact terminal generation retirement |

새 command ID를 추가하지 않는다.

기존 v1 binary와 공존하기 위해 command-local contract version과 exact payload length를 함께 사용한다.
Admin common schema 자체를 전체 API에 대해 변경하지 않는다.

### 7.2 Start v2 payload

Admin common P0..P7은 기존 구조를 유지한다.

```text
P8   ExpectedDiagnosticsBuild U32
P12  OriginalDiagnosticsBootId U32
P16  ExpectedMapRevision U32
P20  ClientIntentId0 U32
P24  ClientIntentId1 U32
P28  ClientIntentId2 U32
P32  ClientIntentId3 U32
P36  HomeContractVersion U16 = 2
P38  HomingMode U16
P40  Position DINT
P44  Velocity DINT
P48  Acceleration DINT
P52  DistanceLimit DINT
P56  TorqueLimit DINT
P60  BufferMode U16
P62  Direction U16
P64  SwitchMode U16
P66  Reserved U16 = 0
P68  TimeoutMilliseconds U32
P72  ExecuteToken U32 = existing LMC_Home token
```

v2 Start payload length은 `76 bytes`다.

현재 v1 Start payload `56 bytes`는 그대로 유지한다. PLC parser는 `56`과 `76` 두 exact shape만
허용하며 다른 길이는 reject한다.

### 7.3 Outcome query v2 payload

query는 original execution parameter를 exact key 일부로 다시 전달한다.

```text
P8   ExpectedDiagnosticsBuild U32
P12  OriginalDiagnosticsBootId U32
P16  ExpectedMapRevision U32
P20  CurrentDiagnosticsBootId U32
P24  OriginalRequestId U32
P28  ClientIntentId0 U32
P32  ClientIntentId1 U32
P36  ClientIntentId2 U32
P40  ClientIntentId3 U32
P44  HomeContractVersion U16 = 2
P46  HomingMode U16
P48  Position DINT
P52  Velocity DINT
P56  Acceleration DINT
P60  DistanceLimit DINT
P64  TorqueLimit DINT
P68  BufferMode U16
P70  Direction U16
P72  SwitchMode U16
P74  Reserved U16 = 0
P76  TimeoutMilliseconds U32
```

v2 Outcome request payload length은 `80 bytes`다.

### 7.4 Retire v2 payload

`0x7D19`는 exact v2 Outcome key 뒤에 terminal generation을 추가한다.

```text
P80 RecordGeneration U32
```

v2 Retire payload length은 `84 bytes`다.

### 7.5 Start ACK / Outcome response

v2 Start ACK는 다음 최소 정보를 echo한다.

```text
P16 HomeContractVersion U16 = 2
P18 HomingMode U16
P20 NativeCommandState U32
```

`NativeCommandState`는 Start acceptance 시 `0`이어야 한다. 실제 native execution failure는 retained
Outcome에서 보고한다.

v2 Outcome response는 다음을 포함해야 한다.

- exact identity / ClientIntent / AxisReference
- `HomeContractVersion`
- 전체 Home parameter echo
- original command status/error/detail
- axis status/error
- raw drive position before/after
- application/internal/set/destination/master position after
- native command state
- evidence flags
- start/completion time
- stop state / runtime phase
- record generation

구현 시 `DINT_PACKET_MAP.txt`에 byte offset을 확정하고 C# serializer/parser와 PLC parser를 같은
changeset에서 갱신한다.

---

## 8. RecoveryKey / Prepared command

v2 `LMCHomeRecoveryKey`는 parameter 전체를 보존한다.

```csharp
public sealed class LMCHomeRecoveryKey
{
    public ushort SchemaVersion { get; }
    public ushort HomeContractVersion { get; }

    public uint OriginalRequestId { get; }
    public uint DiagnosticsBuild { get; }
    public uint OriginalDiagnosticsBootId { get; }
    public uint MapRevision { get; }

    public LMCHomeClientIntentId ClientIntentId { get; }
    public ushort AxisReference { get; }
    public LMCHomeParameters Parameters { get; }
}
```

`LMCPreparedHome`도 `RecoveryKey.Parameters`를 source of truth로 사용한다.
기존처럼 `TargetPosition=0`, `ExpectedActualPosition`, `SemanticMode=CurrentPositionZero`를 v2 객체에
개별 고정값으로 중복 저장하지 않는다.

v1 recovery key는 기존 shape를 유지한다. query/retire serializer는 `HomeContractVersion`에 따라 v1/v2
shape를 선택한다.

prepared command는 현재 one-shot 정책을 그대로 유지한다.

- prepare 단계에서는 wire를 보내지 않는다.
- Start write boundary를 한 번 넘으면 prepared command를 consumed 처리한다.
- timeout/disconnect가 발생해도 original Start를 replay하지 않는다.
- reconnect/restart 후 recovery key로 Outcome/Retire만 수행한다.

---

## 9. PLC 실행 구조

### 9.1 admission

Start admission은 native Home 실행 전에 다음을 확인한다.

```text
request exact shape
-> command contract version
-> diagnostics identity
-> physical axis eligibility
-> typed parameter validation
-> owner/quarantine check
-> Standstill / axis error check
-> moving mode operation-enabled check
-> retained outcome slot availability
-> specialized owner reserve
-> RT dispatch
```

owner reservation 전 deterministic rejection이 가능한 조건은 먼저 reject한다.

### 9.2 native execution adapter

LMC_Home v2는 PLC 내부에 Home 전용 native adapter boundary를 둔다.

```text
LMC generic Home semantic
        |
        v
LMCHomeNativeAdapter
        |
        +-- Direct
        |    -> SetPosition(..., Position)
        |
        +-- AbsoluteSwitch / LimitSwitch / ReferencePulse / Block
             -> LASAL reference/homing native boundary
```

current LASAL export에는 `_LMCAxisBase` command list의 `CMoveReference`가 존재한다. 그러나 현재
tracked `LMC_Home` source는 moving Home을 호출하지 않으며, 이 문서 작성 시점에 exact callable
method signature, parameter mapping, return-state 계약은 current implementation으로 qualification되지
않았다.

따라서 moving mode 구현 전에 반드시 다음을 확인한다.

1. current target `_LMCAxis`의 실제 reference/home callable method signature
2. 각 input의 unit과 enum mapping
3. asynchronous command state / Done / Error readback
4. reference switch, limit switch, encoder pulse source가 어느 axis configuration에 연결되는지
5. same-core/task 호출 제약

이 확인 없이 PMAS `MMC_HOME_*` enum 숫자를 LASAL wire/native call에 그대로 복사하지 않는다.

### 9.3 Direct 실행

`Direct`는 현재 CurrentPositionZero executor를 일반화한다.

```text
Standstill evidence
-> raw/application/internal position snapshot
-> SetPosition(LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position)
-> native result capture
-> fresh coordinate readback
-> owner release
-> terminal outcome commit
```

물리축 이동이 없어야 한다.

### 9.4 moving Home 실행

```text
Standstill + OperationEnabled
-> parameter/native mapping
-> Home command arm
-> native Home execute
-> Homing/Reference motion monitor
-> native Done 또는 terminal Error/Abort
-> fresh Standstill + Referenced/Home state
-> post-home position snapshot
-> owner release
-> terminal outcome commit
```

moving Home에서 PLC service loop가 동기 blocking wait를 수행하지 않는다. RT/native command 상태를
scan별로 관찰하는 state machine으로 진행한다.

---

## 10. Completion contract

### 10.1 공통 원칙

Start ACK는 completion이 아니다. terminal outcome은 RT/native execution과 fresh post-condition을
확인한 뒤 commit한다.

공통 success 조건은 다음과 같다.

- retained record state = Succeeded
- original command status = success
- axis error = 0
- native command terminal success
- Standstill 확인
- pending native/RT dispatch 없음
- shared owner 정상 release
- terminal record generation commit 완료

`TargetReached`와 `MasterPosition`을 공통 LMC_Home success gate로 사용하지 않는다.

### 10.2 Direct

Direct는 coordinate assignment 자체가 목적이므로 다음을 추가로 확인한다.

- raw drive position이 stationary tolerance 안에서 유지
- application actual/set position이 요청 `Position`과 일치
- internal actual/set/destination이 coordinate update 결과와 정합
- physical movement evidence가 없음

현재 CurrentPositionZero의 `Position=0` exact readback 계약을 arbitrary Position으로 일반화한다.

### 10.3 moving mode

moving mode에서 `Position`은 reference event 시 부여되는 coordinate이므로 terminal ActualPosition이
항상 `Position`과 exact 일치한다고 가정하지 않는다.

moving mode success는 다음을 사용한다.

- native Home/Reference Done
- fresh Home/Referenced state
- Standstill
- no axis/native error
- configured Home sequence가 terminal 상태에 도달
- post-home coordinate snapshot 확보

reference 검출 뒤 감속 또는 추가 sequence가 있는 backend에서는 final ActualPosition과 입력
`Position`의 차이는 진단값으로 보존한다. mode/backend별 문서가 exact final-position predicate를
보장하는 경우에만 별도 tolerance gate를 추가한다.

---

## 11. Failure / abort / recovery

다음은 terminal failure 또는 quarantine 대상이다.

- native Home Error
- axis fault/error
- Timeout
- parameter/native mapping failure
- owner loss 또는 owner release uncertainty
- retained record commit/readback uncertainty
- exact recovery key mismatch
- DiagnosticsBuild/BootId/MapRevision mismatch
- moving mode 중 unexpected Disabled/Stopping 상태

Home 실행 중 stop/power-off/preemption이 들어오면 original failure 원인을 보존한 뒤 cleanup 상태를
별도로 기록한다. cleanup 오류가 최초 원인을 덮어쓰지 않는다.

write boundary 이후 transport uncertainty 규칙은 기존 LMC_Home/DS402 Home과 동일하다.

```text
Start sent
-> response lost / disconnect
-> original 0x7D13 replay 금지
-> reconnect
-> exact 0x7D18 query
-> Running이면 계속 query
-> terminal이면 0x7D19 exact retire
```

---

## 12. WPF parameter editor

현재 LMC Home UI를 generic editor로 확장한다.

표시/편집 항목은 다음과 같다.

- Home Position
- Homing Mode
- Velocity
- Acceleration
- Distance Limit
- Torque Limit
- Direction
- Switch Mode
- Buffer Mode
- Timeout

mode 선택에 따라 불필요한 입력을 disable한다.

| Mode | UI 동작 |
|---|---|
| Direct | Position/Timeout만 활성, motion field는 0으로 고정 |
| AbsoluteSwitch | Position/Velocity/Acceleration/Direction/SwitchMode 활성 |
| LimitSwitch | Position/Velocity/Acceleration/Positive-or-Negative Direction 활성 |
| ReferencePulse | Position/Velocity/Acceleration/Positive-or-Negative Direction 활성 |
| Block | Position/Velocity/Acceleration/Distance/Torque/Direction 활성 |

WPF는 application unit 선택값을 사용해 DINT로 변환하고, Start 직전 SDK validation을 다시 통과해야 한다.
UI에서 disabled된 field도 packet에 임의 값이 남지 않도록 canonical `0`/`NotApplicable`로 normalize한다.

moving mode는 실제 축 이동을 일으키므로 기존 one-shot confirmation과 별도로 화면에서 mode/direction/
velocity/distance를 명확히 보여준다.

---

## 13. 예상 source 변경 범위

### SDK / protocol

- `LMC_Library/LMC_API_Delivery/src/LmcHome.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAxisHome.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAdminHome.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAdminHomeModels.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAdminHomeProtocol.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`
- `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`

### WPF

- `LMC_Library/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs`
- LMC Home parameter editor 관련 XAML / binding source
- `LMC_Library/LasalApiWpfTestApp/API_MAPPING.md`

### PLC / LASAL

- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st`
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st`
- 필요 시 Home native adapter 전용 class/method

### 문서

- `docs/api/API_MANUAL.md`
- `docs/api/API_DEVELOPMENT_PROGRESS.md`
- `docs/api/design/README.md`
- 파생 HTML/DOCX/PDF/XLSX는 source 문서 반영 후 재생성

---

## 14. 구현 changeset 순서

### H1 - typed model / validation

- generic `LMCHomeParameters`
- project semantic enum
- v1 compatibility constructor/overload 유지
- mode별 SDK validation

이 단계에서는 PLC runtime behavior를 바꾸지 않는다.

### H2 - wire v2 / recovery

- `0x7D13` 76-byte v2 Start
- `0x7D18` 80-byte v2 query
- `0x7D19` 84-byte v2 retire
- v1/v2 dual exact parser
- v2 RecoveryKey 전체 parameter 보존
- serializer/parser automated contract test

### H3 - PLC admission / retained record

- v2 request parser
- mode별 fail-closed validation
- retained record에 v2 parameters 저장
- v1 CurrentPositionZero path 유지

### H4 - Direct arbitrary Position

- current SetPosition executor를 `Position=0` 고정에서 arbitrary Position으로 일반화
- no-motion/readback evidence
- v2 Direct end-to-end qualification

### H5 - moving native adapter

- LASAL Home/reference native signature 확정
- AbsoluteSwitch / LimitSwitch / ReferencePulse / Block mapping
- scan-driven state machine
- timeout/abort/fault cleanup

mode는 한 번에 모두 활성화하지 않는다. native mapping과 실축 증거가 확보된 mode부터 allowlist에 추가한다.

### H6 - WPF editor

- mode별 parameter editor
- unit conversion
- mode-aware enable/disable
- durable recovery journal에 v2 RecoveryKey 저장

### H7 - documentation / qualification

- API Manual / Progress / packet map 동기화
- mode별 실축 qualification matrix 작성
- production release 판정은 별도 수행

---

## 15. 검증 기준

### 15.1 static / PC

- v1 packet golden 유지
- v2 Start/Outcome/Retire byte-exact golden
- reserved/invalid enum rejection
- mode별 validation positive/negative test
- one-shot prepared command replay rejection
- reconnect query/retire recovery
- v1/v2 exact-key mismatch rejection
- `git diff --check`, `git diff --cached --check`

### 15.2 LASAL IDE / PLC

- C78 compile/rebuild
- generated artifact identity 확인
- PLC download 후 fresh BootId/MapRevision 확인
- v1 CurrentPositionZero regression
- v2 Direct arbitrary Position runtime
- moving mode state transition / timeout / abort / fault 확인

### 15.3 physical qualification

각 moving mode는 독립적으로 판정한다.

- 시작 방향
- switch/limit/index polarity
- search velocity
- acceleration
- distance limit
- torque limit 사용 시 실제 unit/limit
- reference 검출 위치와 Position 적용
- final Standstill/Referenced
- timeout/stop/fault cleanup

한 mode의 PASS를 다른 mode의 PASS로 확대하지 않는다.

---

## 16. `LMC_Home`과 `LMC_HomeDS402` 경계

| 항목 | `LMC_Home` | `LMC_HomeDS402` |
|---|---|---|
| 목적 | controller/application generic Home | drive DS402 Homing Method 실행 |
| 주요 입력 | Position + generic Home mode/direction/switch | 0x6098 Method + 0x607C/6099/609A |
| mode 의미 | Direct / AbsSwitch / LimitSwitch / RefPulse / Block | DS402 standard method number |
| wire | `0x7D13/18/19` | `0x7D15/16/17` |
| native 구현 | LASAL Home/reference adapter | drive SDO + ControlWord Homing state machine |
| 완료 기준 | native Home/Referenced + Standstill | HomingAttained/no-error + DS402 cleanup |

두 기능의 parameter object와 retained outcome lifecycle은 비슷하게 유지하되 native execution 의미는
분리한다.

---

## 17. 현재 판정

2026-09-14 기준 상태는 다음과 같다.

- generic LMC_Home 설계: **DESIGN COMPLETE**
- current source의 generic parameter/API: **NOT IMPLEMENTED**
- current wire v2: **NOT IMPLEMENTED**
- arbitrary Position Direct Home: **NOT IMPLEMENTED**
- moving Home native adapter: **OPEN**
- WPF generic Home editor: **NOT IMPLEMENTED**
- current v1 CurrentPositionZero: 기존 구현 유지
- physical qualification: **OPEN**
- production release: **NO-GO**

다음 구현 시작점은 H1 typed model/validation이다. H1/H2에서 public contract와 wire를 먼저 고정한 뒤
Direct를 arbitrary Position으로 일반화하고, moving Home은 native LASAL reference boundary가 확인된
mode부터 순차적으로 활성화한다.

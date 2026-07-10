# LASAL-DINT API 단위 변환 배포 매뉴얼

작성일: 2026-07-10

적용 대상: `LasalMotionControlLib`

## 반드시 지켜야 할 계약

단위 변환 책임은 API를 호출하는 PC 프로그램에 있다.

1. 호출자는 LASAL에서 같은 값을 작성할 때 쓰는 engineering value에
   축과 인자별로 승인된 `LMC_Units` 상수를 곱한다.
2. 호출자는 반올림과 DINT 범위 검사를 수행한 뒤 `int`로 API에 전달한다.
3. DLL은 받은 `int`를 다시 변환하지 않고 little-endian DINT로 직렬화한다.
4. LASAL PLC는 수신 DINT를 다시 변환하지 않고 `_LMCAxis` 또는
   `_LMCRobot`의 해당 인자로 전달한다.
5. 읽기 API는 LASAL internal DINT를 그대로 반환한다. 화면 표시가 필요하면
   PC 프로그램이 송신 때 선택한 UNIT으로 나눈다.

즉, 계약은 아래와 같다.

```text
송신 DINT = 물리값 x UNIT
표시 물리값 = 수신 DINT / UNIT
```

WPF test app의 `8,388,608 count/rev`는 호출자 측에서 선택한 23-bit encoder
더미 profile이다. DLL 자동 변환이 아니다. 실제 배포 프로그램은 이 값을
그대로 복사하지 말고 PLC에 등록된 UNIT 또는 scale을 사용한다. DLL 내부
정·역변환은 사용하지 않는다.

## UNIT 선택

`LMC_Units`는
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/unit.h`를 C# 상수로 옮긴
것이다. DLL 내부 packet builder는 이 상수를 참조하지 않는다.

| 용도 | UNIT | 배율 | 주의 |
|---|---|---:|---:|
| 직선 위치 | `LMC_Units.MM` | 10000 | 해당 축이 mm profile일 때만 사용 |
| 회전 application unit | `LMC_Units.DEG` | 10000 | 현재 `_LMCAxis1`~`_LMCAxis4` profile에서 사용 |
| 직선 path 속도 | `LMC_Units.MMPSEC` | 10000 | mm/s profile에서만 사용 |
| 직선 가속도 | `LMC_Units.MMPSEC2` | 1 | mm/s2 profile에서만 사용 |
| 회전수 | `LMC_Units.RPM` | 1000 | PLC 인자가 명시적으로 RPM일 때만 사용 |

`_LMCAxis.MoveAbsolute.Speed`는 RPM이 아니라 `Application units / s`다.
따라서 회전축이라는 이유만으로 `LMC_Units.RPM`을 사용하면 안 된다.

현재 `Elmo_EtherCAT_Test_4Axis`의 `_LMCAxis1..4`는 Motion Network에서 아래처럼
모두 `deg` macro를 사용한다.

| 항목 | 현재 PLC 설정 | PC 호출 UNIT |
|---|---|---|
| Position / IntUnits | `360 deg` | `LMC_Units.DEG` |
| Speed / VMax | `18000 deg` | `LMC_Units.DEG` |
| Accel, Decel / AMax | `180000 deg` | `LMC_Units.DEG` |
| Jerk / JMax | `180000 deg` | 아래 주의사항 적용 |

`_LMCAxis` 문서는 jerk를 `Application units / sec^3 / 1000`으로 정의하지만
`unit.h`에는 jerk 전용 UNIT이 없다. 따라서 nonzero jerk의 물리값 변환식은
PLC motion profile과 실제 시험으로 별도 확정해야 한다. 확정 전 배포 예제는
jerk를 `0`으로 사용한다. 근거 없이 `MMPSEC2`나 고정 배율 `1`을 jerk에
적용하지 않는다.

프로젝트마다 아래 축/인자 profile을 배포 전에 채워야 한다.

| Target | Position | Velocity | Accel | Decel | Jerk |
|---|---|---|---|---|---|
| `_LMCAxis1`~`_LMCAxis4` | `DEG` | `DEG` | `DEG` | `DEG` | nonzero 검증 전 `0` |
| `_LMCRobotBase1` coordinate | kinematic 축별 확정 필요 | path profile 확정 필요 | 확정 필요 | 확정 필요 | nonzero 검증 전 `0` |

Group/Robot position 배열은 축별로 단위가 다를 수 있으므로 각 원소를
해당 축 UNIT으로 개별 변환한다.

## 권장 변환 함수

```csharp
static int ToDint(double physicalValue, int unit)
{
    if (double.IsNaN(physicalValue) || double.IsInfinity(physicalValue))
    {
        throw new ArgumentOutOfRangeException("physicalValue");
    }

    return checked((int)Math.Round(
        physicalValue * unit,
        MidpointRounding.AwayFromZero));
}

static double FromDint(int internalValue, int unit)
{
    if (unit == 0)
    {
        throw new ArgumentOutOfRangeException("unit");
    }

    return (double)internalValue / unit;
}
```

`checked`를 빼면 큰 값이 DINT 범위를 벗어났을 때 잘못된 값으로 전송될 수
있다. API 호출 전에 축의 software limit도 별도로 검사해야 한다.

## 현재 `_LMCAxis1` 회전축 호출 예

```csharp
var position = ToDint(90.0, LMC_Units.DEG);
var velocity = ToDint(50.0, LMC_Units.DEG);
var acceleration = ToDint(100.0, LMC_Units.DEG);
var deceleration = ToDint(100.0, LMC_Units.DEG);
var jerk = 0; // nonzero physical conversion is not approved yet

// PowerOn ACK 이후 project-specific ready 상태를 확인한 뒤 실행한다.
var moveResponse = axis.MoveAbsoluteEx(
    position,
    velocity,
    acceleration,
    deceleration,
    jerk);
if (!moveResponse.IsSuccess)
{
    throw new InvalidOperationException("MoveAbsoluteEx failed");
}

LMC_Response readResponse;
var actualPositionDint = axis.GetActualPosition(out readResponse);
if (!readResponse.IsSuccess)
{
    throw new InvalidOperationException("GetActualPosition failed");
}
var actualPositionDeg = FromDint(actualPositionDint, LMC_Units.DEG);
```

API 호출부에서 직접 곱해도 단위 계약은 같다. 아래 코드는 인자 변환 예이며
Power/ready/response 확인 순서를 생략한 전체 실행 예가 아니다.

```csharp
axis.MoveAbsoluteEx(
    checked((int)Math.Round(90.0 * LMC_Units.DEG)),
    checked((int)Math.Round(50.0 * LMC_Units.DEG)),
    checked((int)Math.Round(100.0 * LMC_Units.DEG)),
    checked((int)Math.Round(100.0 * LMC_Units.DEG)),
    0);
```

## 책임 분리

| 계층 | 책임 | 하지 않는 일 |
|---|---|---|
| PC application | UNIT 선택, 곱셈/나눗셈, 반올림, 범위·축 limit 검사 | DLL이 대신 변환할 것이라고 가정하지 않음 |
| `LasalMotionControlLib` | 전달받은 `int`를 DINT payload에 그대로 기록 | 자동 UNIT 선택, CPR 변환, 재스케일 |
| LASAL PLC | 수신 DINT 검증과 motion block 호출 | PMAS count 변환, LREAL 재변환 |

이 정책은 과거 히스토리에 남아 있는 DLL 내부 unit conversion 제안을
대체한다.

## 근거 파일

- UNIT 상수: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/unit.h`
- `_LMCAxis1`~`_LMCAxis4` 현재 profile:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st`
- motion 인자 의미:
  `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxis/_LMCAxis.st`

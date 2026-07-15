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
Jerk 송신 DINT = (물리 jerk / 1000) x 축 application UNIT
```

현재 LASAL Motion Network의 `ExUnits=8,388,608`은 DS402/encoder 측 변환
설정이다. PC API의 application UNIT이 아니며 PC 송신값에 곱하지 않는다.
2026-07-14 저장 설정은 사용자가 제시한 `모터 1회전 = 10 mm` 기구비에 맞춰
`_LMCAxis1..4.IntUnits=10 mm(100000)`로 변경했다. PC application UNIT은
계속 `1 mm = LMC_Units.MM = 10000 DINT`다. DLL 내부 정·역변환은 사용하지 않는다.

## UNIT 선택

`LMC_Units`는
`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/unit.h`를 C# 상수로 옮긴
것이다. DLL 내부 packet builder는 이 상수를 참조하지 않는다.

| 용도 | UNIT | 배율 | 주의 |
|---|---|---:|---:|
| 직선 위치 | `LMC_Units.MM` | 10000 | 해당 축이 mm profile일 때만 사용 |
| 회전 application unit | `LMC_Units.DEG` | 10000 | deg profile에서만 사용 |
| 직선 path 속도 | `LMC_Units.MMPSEC` | 10000 | mm/s profile에서만 사용 |
| 직선 가속도 | `LMC_Units.MMPSEC2` | 1 | mm/s2 profile에서만 사용 |
| 회전수 | `LMC_Units.RPM` | 1000 | PLC 인자가 명시적으로 RPM일 때만 사용 |

`_LMCAxis.MoveAbsolute.Speed`는 RPM이 아니라 `Application units / s`다.
따라서 회전축이라는 이유만으로 `LMC_Units.RPM`을 사용하면 안 된다.

### WPF 예제의 UNIT 콤보와 Raw DINT

예제 프로그램은 Axis와 Group에 각각 application UNIT 콤보를 제공한다.

| 선택 | 입력 의미 | 송신 규칙 |
|---|---|---|
| `mm (x10000)` | 물리값 mm | `입력 x LMC_Units.MM` |
| `m (x10000000)` | 물리값 m | `입력 x LMC_Units.M` |
| `deg (x10000)` | 물리값 deg | `입력 x LMC_Units.DEG` |
| `None / raw DINT` | 이미 변환된 PLC DINT | 정수 입력을 그대로 전송 |

`None / raw DINT`는 소수를 반올림하는 배율 1 모드가 아니다. DINT 정수만
허용한다. 예를 들어 software-limit와 같은 raw 값을 시험할 때 두 입력은
같은 wire DINT를 만든다.

```text
mm 선택:              11744.0512 -> 117440512 DINT
None / raw DINT 선택: 117440512  -> 117440512 DINT
```

UNIT 선택은 PC 표시/변환만 바꾼다. PLC의 software limit, MaxModulo,
encoder 범위와 실제 장비 이동 한계를 확대하지 않는다.

### PC application UNIT과 축 transmission ratio를 구분한다

PC의 `1 mm = 10000 DINT` 계약과 `_LMCAxis.ExUnits/IntUnits`는 역할이 다르다.
MotionLib의 축 변환식은 다음과 같다.

```text
InternalPosition [application unit]
    = ExternalPosition [encoder value] x IntUnits / ExUnits
```

현재 프로젝트는 `ExUnits=8388608`, `IntUnits=10 mm(100000)`다. 즉
`8,388,608 encoder count`가 `10 mm` 이동에 해당한다. 기구비를 확인하지 않고
`ExUnits`를 PC UNIT인 `10000`으로 바꾸거나 MaxModulo만 확대하면 실제 이동량이
틀어진다.

이 비율에서 application 좌표와 external signed-DINT 위치의 관계는 다음과 같다.

```text
1280 mm -> 12,800,000 application DINT -> 0x40000000 external
2500 mm -> 25,000,000 application DINT -> 0x7D000000 external
2560 mm -> 25,600,000 application DINT -> 0x80000000 external (양의 범위 초과)
```

따라서 external offset이 0일 때 한쪽 좌표 상한은 약 `2559.9999 mm`이고
signed-DINT 전체 창의 폭은 약 `5120 mm`다. 기존 절대엔코더/BinOffset이
`+0x40000000`만큼 남아 있으면 양의 headroom이 절반이므로 약 `1280 mm`에서
경계에 도달한다. 이는 SIGMATEK 시스템의 고정 이동거리 한계가 아니라 현재
external scale과 좌표 offset의 조합이다. 스케일을 바꾼 뒤 기존 retentive
absolute position offset을 그대로 쓰지 말고 정식 재참조해야 한다.

SIGMATEK MotionLib의 `_LMCAxis.SetParameter()` 문서는
`LMCAXIS_PAR_SET_MAXMODULO`에 대해 아래 조건을 명시한다.

```text
Value x ExUnits / IntUnits > 2147483647 -> CommandError
```

이 값은 internal unit으로 지정하며 마지막 Init cycle 이전에만 변경할 수 있다.
현재 10 mm/rev 비율에서 설정 가능한 양의 최대값은 `25,599,999 DINT`, 즉
`2559.9999 mm`다. `Resolution`을 키우면서 `IntUnits`도 같은 배수로 키우라는
매뉴얼 규칙 때문에 두 값은 변환식에서 약분된다. Resolution은 저속 프로파일
정밀도 조정용이며 좌표 범위 확대 수단이 아니다.

MotionLib `_LMCAxis` 변경 이력은 SW end position이 비활성인 continuous/endless
축에서 target이 내부 overflow 위치를 넘어도 overflow 위치까지 이동하고 좌표를
보정한 뒤 남은 거리를 계속 이동한다고 명시한다. 현재 프로젝트의 `_LMCAxis
1.120`은 이 기능이 추가된 `1.65`보다 최신이다. 따라서 `1280 mm`나
`2560 mm`가 모든 단축 motion의 총 이동거리 한계는 아니다.

단, 현재 Group motion의 `_LMCProfile`은 기본값
`_LMCPROF_ChkEndPosForSwLimit=1`로 연결 축의 최종 허용 위치를 사전 검사한다.
명시적 SW limit이 없으면 `±MaxModulo`를 최종 허용 위치로 사용하므로 target이
이를 넘으면 `_LMCPROF_SWE_ERROR(7)`가 발생한다. 이 검사를 0으로 끄는 기능은
있지만 유한 스트로크 4축 장비에서 기계 한계 대체책 없이 자동 적용하지 않는다.

유한축 Group에서 약 `5120 mm`보다 넓은 절대좌표 창이 필요하면 Elmo 드라이브의
DS402 position scaling/electronic gearing으로 `0x6064`와 `0x607A`의 external
unit 자체를 재설계하고 LASAL `ExUnits`를 그 값과 같이 바꿔야 한다. Elmo도
[DS402 user-defined scaling](https://www.elmomc.com/capabilities/servo-technology/special-functionality/scaling-factors/)을
지원한다. 정확한 drive object 값과 실제 기계 SW limit가 확정되기 전에는 이
저장소의 EtherCAT startup parameter를 추정값으로 수정하지 않는다.

현재 저장된 `Elmo_EtherCAT_Test_4Axis`의 `_LMCAxis1..4`는 Motion Network에서
아래처럼 `mm` macro와 `_JERK_PROFILE`을 사용한다. 실제 PLC에 다운로드된 설정이
같은지는 live motion 전에 확인한다.

| 항목 | 현재 PLC 설정 | PC 호출 UNIT |
|---|---|---|
| Position / IntUnits | `10 mm/rev` | `LMC_Units.MM` |
| Speed / VMax | `75 mm` | `LMC_Units.MM` |
| Accel, Decel / AMax | `7500 mm` | `LMC_Units.MM` |
| Motion profile | `_JERK_PROFILE` | nonzero Jerk 적용 가능 |
| Jerk / JMax | `75000 mm` | `(물리 jerk / 1000) x LMC_Units.MM` |

### 10 mm/rev 설정 변경 후 필수 확인

1. Servo off와 Group unlock 상태에서 새 네트워크를 다운로드하고 PLC를 재시작한다.
2. 네 축 모두 `IntUnits=100000`, `Resolution=1`인지 확인한다.
3. 기존 절대엔코더 offset을 임의로 0으로 쓰지 말고 장비의 실제 기준점에서
   정식 재참조한다.
4. 각 축의 `LMCAXIS_PAR_RD_MAX_MODULO`, `LMCAXIS_PAR_RD_BINOFFSET`,
   `_LMCABSEncoder.PosOffset`, `PosOffsetOk`를 읽는다.
5. Group을 다시 LockProfile한 뒤 profile의 최종 SW min/max를 읽는다.
6. 실제 기계 SWMin/SWMax가 확정되기 전에는 1280 mm 이상 실기 이동을 수행하지
   않는다.

`_LMCAxis` 문서는 jerk를 `Application units / sec^3 / 1000`으로 정의한다.
`unit.h`에는 jerk 전용 UNIT이 없으므로 위치와 같은 축 application-unit 상수를
사용하되 물리 jerk를 먼저 `1000`으로 나눈다. 예를 들어 `1000 mm/s^3`은
`(1000 / 1000) x 10000 = 10000 DINT`다. `MMPSEC2`, `RPM`이나 고정 배율 `1`을
대신 사용하지 않는다. nonzero 값은 `_JERK_PROFILE`에서만 효과가 있으므로
MoveType, JMax, software limit와 실제 장비 동작을 함께 검증한다.

프로젝트마다 아래 축/인자 profile을 배포 전에 채워야 한다.

| Target | Position | Velocity | Accel | Decel | Jerk |
|---|---|---|---|---|---|
| `_LMCAxis1`~`_LMCAxis4` | `MM` | `MM` | `MM` | `MM` | `(물리값 / 1000) x MM` |
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

static int ToJerkDint(double physicalJerkPerSecondCubed, int axisUnit)
{
    return ToDint(physicalJerkPerSecondCubed / 1000.0, axisUnit);
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

## 현재 저장된 `_LMCAxis1` 선형축 호출 예

```csharp
var position = ToDint(1.0, LMC_Units.MM);
var velocity = ToDint(1.0, LMC_Units.MM);
var acceleration = ToDint(10.0, LMC_Units.MM);
var deceleration = ToDint(10.0, LMC_Units.MM);
var jerk = ToJerkDint(1000.0, LMC_Units.MM); // 1000 mm/s^3 -> 10000 DINT

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
var actualPositionMm = FromDint(actualPositionDint, LMC_Units.MM);
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

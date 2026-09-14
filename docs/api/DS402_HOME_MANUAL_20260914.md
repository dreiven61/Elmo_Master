# DS402 Home 사용 매뉴얼 - 2026-09-14

대상 API는 `LMC_HomeDS402`다. 이 문서는 current `dev@97e3ae4` 기준으로 WPF와 SDK에서 DS402 Homing을 설정하고
실행하는 방법을 설명한다. 전체 API 공통 사용법은 `API_MANUAL.md`를 따른다.

## 1. 지원 Homing Method

현재 basic DS402 Home은 다음 method를 허용한다.

- `1..14`
- `17..30`
- `33`
- `34`
- `37`

Method `37`은 현재 위치 기반 Home이며 switch/limit search를 수행하지 않는다. 그 외 허용 method는 drive가 실제로 움직일 수 있으므로
사용할 drive의 DS402/Elmo Homing Method 정의, switch/index 배선, 방향과 기구 한계를 먼저 확인한다.

Method `35`와 reserved/vendor-specific method는 current basic API에서 허용하지 않는다.

## 2. WPF 입력 항목

`DS402 Home` 화면에서 다음 값을 설정한다.

| UI 항목 | Object | 설명 |
|---|---|---|
| Homing Method | `0x6098` | 사용할 DS402 Homing Method |
| Home Offset | `0x607C` | raw drive unit의 Home offset |
| Home Velocity1 | `0x6099:01` | 첫 번째 Homing 속도 |
| Home Velocity2 | `0x6099:02` | 두 번째 Homing 속도 |
| Home Acceleration | `0x609A:00` | Homing 가속도 |
| Timeout | project watchdog | 전체 Homing 동작 watchdog |

입력 규칙은 다음과 같다.

- Method 37: `Home Velocity1 = 0`, `Home Velocity2 = 0`, `Home Acceleration = 0`
- Moving method: `Home Velocity1 > 0`, `Home Velocity2 > 0`, `Home Acceleration > 0`
- TorqueLimit: basic API에서는 `0`
- BufferMode: `Aborting`
- Home Offset: raw drive DINT. current SDK 허용 범위는 `-2000000000..2000000000`

SDK 내부에서 기존 72-byte protocol과의 호환 때문에 `Velocity`가 HomeVelocity1,
`DistanceLimit`가 HomeVelocity2를 저장한다. 사용자 관점에서는 HomeVelocity1/2로 이해하면 된다.

## 3. 실행 전 확인

실제 축을 움직이는 method에서는 최소한 다음을 먼저 확인한다.

- 대상 axis가 현재 physical Home-capable axis인지
- drive/axis connection과 DS402 fault 상태
- 해당 Homing Method가 요구하는 Home/limit/index input 배선과 polarity
- 이동 방향과 기구 stroke가 안전한지
- Velocity1/2와 Acceleration이 drive raw unit 기준으로 적절한지
- 현재 운전 모드가 PP(1), PV(3), IP(7), CSP(8) 중 하나인지

Home/Referenced 상태 자체를 Servo On의 일반 선행조건으로 사용하지 않는다.

## 4. SDK 호출 순서

신규 코드는 아래 정식 API 이름을 사용한다.

```csharp
var parameters = new LMCAxisDs402HomeParameters(
    homingMethod,
    homeOffset,
    homeVelocity1,
    homeAcceleration,
    homeVelocity2,   // legacy DistanceLimit wire field
    0,               // TorqueLimit
    LMCDs402HomeBufferMode.Aborting,
    timeoutMilliseconds);

var prepared = axis.PrepareLMC_HomeDS402(
    parameters,
    adminCapabilities,
    diagnosticCapabilities,
    executeToken);

var ack = await axis.LMC_HomeDS402Async(prepared, cancellationToken);
```

여기서 `ack`는 Start가 접수됐다는 뜻이다. Home 완료로 처리하면 안 된다.

이후 `prepared.RecoveryKey`에 해당하는 key로 terminal outcome을 조회한다.

```csharp
var outcome = await axis.ReadDs402HomeOutcomeAsync(
    recoveryKey,
    adminCapabilities,
    diagnosticCapabilities,
    cancellationToken);
```

`Running`이면 같은 recovery key로 poll만 계속한다. terminal이면 결과를 확인한 뒤 exact generation으로 retire한다.

```csharp
await axis.RetireDs402HomeOutcomeAsync(
    recoveryKey,
    recordGeneration,
    adminCapabilities,
    diagnosticCapabilities,
    cancellationToken);
```

`PrepareDs402Home`, `Ds402Home`, `Ds402HomeAsync`는 호환 wrapper다. 신규 구현에서는 정식 `LMC_` 이름을 사용한다.

## 5. PLC 내부 동작

응용 프로그램은 ControlWord/StatusWord를 직접 시퀀싱하지 않는다. public API 요청을 받은 PLC backend가 다음을 수행한다.

```text
현재 0x6061 mode 저장
-> 0x607C / 0x6098 / 0x6099:01 / 0x6099:02 / 0x609A 설정
-> 0x6060 = 6
-> 0x6061 = 6 확인
-> ControlWord bit 4 rising edge
-> fresh HomingAttained/no-error/position 확인
-> bit 4 LOW
-> LASAL setpoint/destination 정렬
-> 저장한 mode를 0x6060에 복원
-> 0x6061 exact readback
-> owner release
-> terminal outcome 기록
```

Home 시작 전 mode가 PP(1)이면 PP(1), PV(3)이면 PV(3), IP(7)이면 IP(7), CSP(8)이면 CSP(8)로 돌아간다.
CSP로 강제 복귀하지 않는다.

## 6. 완료 상태 해석

current completion rule은 다음과 같다.

- Homing attained bit 12: 필수
- Homing error bit 13: clear 필수
- Fault bit 3: clear 필수
- ActualPosition: expected position `+/-32 raw count`
- Target reached bit 10: 진단값으로 기록하지만 terminal gate는 아님

ExpectedPosition은 다음과 같다.

- Method 37: `HomeOffset`
- Moving method: `-HomeOffset`

Master Position은 current success 조건이 아니다. PLC의 setpoint alignment가 Master Position 변경을 보장하지 않으므로
setpoint/destination readback과 raw ActualPosition을 완료 근거로 사용한다.

## 7. Method 37 사용 예

현재 위치를 Home으로 사용하는 경우:

```text
Homing Method     = 37
Home Offset       = 필요한 값
Home Velocity1    = 0
Home Velocity2    = 0
Home Acceleration = 0
```

Method 37은 Home/limit switch를 찾기 위해 축을 이동하지 않는다. 2026-09-14 기준 사용자가 기존 DS402 Home 정상 구동을 확인했다.
다만 fault/disconnect/restart 전체 qualification을 완료했다는 뜻은 아니다.

## 8. Moving method 사용 시

Method `1..14`, `17..30`, `33`, `34`는 개별 장비에서 먼저 시험한다. Method 번호만 보고 방향이나 어떤 switch를 찾는지 추정하지 말고
사용 중인 Elmo drive의 Homing Method 정의와 실제 input assignment를 대조한다.

권장 시험 순서는 다음과 같다.

1. 낮은 안전 속도/가속도와 충분한 기구 여유를 확보한다.
2. Home/limit/index input이 online에서 실제로 변하는지 확인한다.
3. 한 method만 선택해 실행한다.
4. 실제 이동 방향과 switch/index 검출 위치를 확인한다.
5. `0x7D16` terminal outcome, StatusWord, ActualPosition을 확인한다.
6. bit 4 LOW와 pre-Home mode 복귀를 확인한다.
7. `0x7D17` retire까지 완료한다.

한 method의 성공 결과를 다른 method에 그대로 적용하지 않는다.

## 9. Timeout / disconnect / 실패 처리

Start write 이후 timeout 또는 connection loss가 발생했다고 같은 Start를 다시 보내지 않는다.
Start가 PLC에 전달됐는지 불확실할 수 있기 때문이다.

복구는 다음 순서다.

```text
reconnect
-> fresh diagnostics identity 확인
-> original recovery key로 ReadDs402HomeOutcome
-> Running이면 poll
-> terminal이면 결과 보존
-> exact generation Retire
```

다음 경우는 새 Home을 바로 재시작하지 말고 terminal/failure 상태를 먼저 해소한다.

- saved mode restore 실패
- ControlWord bit 4 LOW 확인 실패
- RT owner release 불확실
- SDO drain 불완전
- BootId/MapRevision mismatch
- unretired terminal record 존재

## 10. 현재 검증 범위

- 요청한 public API 구조: 구현 완료
- Homing Method/Offset/Velocity1/Velocity2/Acceleration 편집: 구현 완료
- Method 37 기존 동작: 사용자 실기 정상 확인
- 다른 moving method: 개별 테스트 대기

개별 실축 테스트는 사용자 시험 결과를 기준으로 method별 qualification 기록을 추가한다.

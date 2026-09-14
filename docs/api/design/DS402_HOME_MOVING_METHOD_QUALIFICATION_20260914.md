# DS402 Home moving method qualification - 2026-09-14

## 1. 목적

이 문서는 `LMC_HomeDS402`의 moving Homing Method를 실제 장비에서 method별로 검증할 때 사용하는 기록 양식이다.

현재 판정은 다음과 같다.

- Method 37: 사용자 실기 정상 동작 확인 완료
- Moving method `1..14`, `17..30`, `33`, `34`: 개별 실축 검증 대기
- 전체 DS402 Home production release: NO-GO

Method 37의 PASS를 다른 method의 PASS로 확대하지 않는다. Moving method는 실제 drive wiring, switch/index 구성과 적용 가능한 homing method를 drive manual에서 확인한 뒤 개별 검증한다.

## 2. current contract

정식 호출 순서는 다음과 같다.

```text
PrepareLMC_HomeDS402(...)
-> LMC_HomeDS402(...) / LMC_HomeDS402Async(...)
-> ReadDs402HomeOutcome(...) / ReadDs402HomeOutcomeAsync(...)
-> RetireDs402HomeOutcome(...) / RetireDs402HomeOutcomeAsync(...)
```

wire lifecycle:

| 단계 | Command |
|---|---:|
| Start | `0x7D15` |
| Outcome | `0x7D16` |
| Retire | `0x7D17` |

편집 가능한 DS402 Homing object:

| 항목 | Object | Moving method 규칙 |
|---|---|---|
| Homing Method | `0x6098` | `1..14`, `17..30`, `33`, `34` |
| Home Offset | `0x607C` | raw drive DINT |
| Home Velocity1 | `0x6099:01` | `> 0` |
| Home Velocity2 | `0x6099:02` | `> 0` |
| Home Acceleration | `0x609A:00` | `> 0` |
| Timeout | project watchdog | `> 0` |

Moving method의 expected raw ActualPosition은 current contract에서 `-HomeOffset`이며 허용 범위는 `+/-32 count`다.

`TargetReached(bit10)`은 진단값이다. terminal success 필수 조건으로 사용하지 않는다. Master Position도 success predicate가 아니다.

Home 시작 전 operating mode는 `0x6061`에서 저장한다. 지원 pre-Home mode는 PP(1), PV(3), IP(7), CSP(8)이며 Home 종료 후 exact saved mode로 복원되어야 한다.

## 3. 1회 시험 기록

시험마다 아래 값을 먼저 기록한다.

| 항목 | 기록 |
|---|---|
| 날짜/시간 | |
| Axis | |
| Drive / firmware | |
| Homing Method (`0x6098`) | |
| Home Offset (`0x607C`) | |
| Home Velocity1 (`0x6099:01`) | |
| Home Velocity2 (`0x6099:02`) | |
| Home Acceleration (`0x609A`) | |
| Timeout | |
| 시작 전 `0x6061` mode | |
| switch/index/limit 구성 | |
| 예상 이동 방향 | |

실행 중 확인 항목:

| 순서 | 확인 항목 | PASS 기준 | 결과 |
|---:|---|---|---|
| 1 | Prepare | parameter/capability/identity admission 성공 | |
| 2 | Start `0x7D15` | Start ACK accepted | |
| 3 | Homing mode | `0x6061 = 6` 확인 | |
| 4 | Start edge | ControlWord bit 4 rising edge 수행 | |
| 5 | Homing 진행 | 의도한 switch/index/limit 및 방향으로 진행 | |
| 6 | Homing 완료 | StatusWord bit12 HomingAttained = 1 | |
| 7 | Homing error | bit13 = 0 | |
| 8 | Drive fault | bit3 = 0 | |
| 9 | Raw position | `ActualPosition = -HomeOffset +/-32 count` | |
| 10 | Cleanup | ControlWord bit4 LOW | |
| 11 | Mode restore | `0x6061`이 시작 전 saved mode와 정확히 일치 | |
| 12 | Outcome `0x7D16` | terminal Succeeded | |
| 13 | Retire `0x7D17` | exact terminal generation retire 성공 | |

`TargetReached(bit10)` 값은 기록할 수 있지만 PASS/FAIL 판정에는 사용하지 않는다.

## 4. 실패 시 최소 보존 값

실패하면 재시도 전에 아래 값을 보존한다.

| 값 | 기록 |
|---|---|
| Homing Method | |
| Home Offset / Velocity1 / Velocity2 / Acceleration | |
| pre-Home mode | |
| final `0x6061` | |
| DS402 StatusWord | |
| HomingAttained bit12 | |
| HomingError bit13 | |
| Fault bit3 | |
| TargetReached bit10 | |
| ActualPosition `0x6064` | |
| `0x7D16` RecordState | |
| `0x7D16` OriginalDetailCode | |
| `0x7D16` OriginalErrorId | |
| NativeCommandState | |
| 발생 시점 / 물리 위치 | |

Start write boundary 이후 timeout/disconnect가 의심되면 original `0x7D15`를 자동 재전송하지 않는다. exact recovery key로 `0x7D16` Outcome을 조회하고 terminal record가 확인된 뒤 `0x7D17`로 retire한다.

## 5. method별 qualification 현황

`OPEN`은 아직 실축 PASS가 없다는 뜻이다. 실제 장비에 적용 불가능한 method는 `N/A`와 사유를 기록한다.

| Method | 상태 | Axis | Parameter set | 결과/비고 |
|---:|---|---|---|---|
| 1 | OPEN | | | |
| 2 | OPEN | | | |
| 3 | OPEN | | | |
| 4 | OPEN | | | |
| 5 | OPEN | | | |
| 6 | OPEN | | | |
| 7 | OPEN | | | |
| 8 | OPEN | | | |
| 9 | OPEN | | | |
| 10 | OPEN | | | |
| 11 | OPEN | | | |
| 12 | OPEN | | | |
| 13 | OPEN | | | |
| 14 | OPEN | | | |
| 17 | OPEN | | | |
| 18 | OPEN | | | |
| 19 | OPEN | | | |
| 20 | OPEN | | | |
| 21 | OPEN | | | |
| 22 | OPEN | | | |
| 23 | OPEN | | | |
| 24 | OPEN | | | |
| 25 | OPEN | | | |
| 26 | OPEN | | | |
| 27 | OPEN | | | |
| 28 | OPEN | | | |
| 29 | OPEN | | | |
| 30 | OPEN | | | |
| 33 | OPEN | | | |
| 34 | OPEN | | | |
| 37 | PASS - user runtime confirmation | | Velocity1/2=0, Acceleration=0 | 기존 DS402 Home 정상 구동 확인 |

## 6. 판정 규칙

한 method를 PASS로 기록하려면 최소한 다음이 한 시험에서 함께 확인되어야 한다.

- 의도한 homing 방향과 switch/index/limit 동작
- HomingAttained bit12
- HomingError bit13 clear
- Fault bit3 clear
- expected raw ActualPosition `+/-32 count`
- bit4 LOW cleanup
- pre-Home operating mode exact restore
- `0x7D16` terminal Succeeded
- `0x7D17` exact retire

Method별 정상 동작을 확인한 뒤에도 timeout, fault, Stop/PowerOff, disconnect/restart, response-loss와 packet causal proof는 별도 release qualification으로 남는다.

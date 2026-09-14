# DS402 Home 구현 설계 - 2026-09-14

## 1. 문서 지위

이 문서는 `dev@97e3ae433ef470ce815a79c9b8d8fda0201bd367`의 DS402 Home 구현을 기준으로 한 current 설계 정본이다.
`LMC_HOME_AND_DS402_HOME_IMPLEMENTATION_DESIGN_20260911.md`의 `DS402Home = Method 37 only` 설명과
`HOME_DS402_DESIGN.md`의 Method 37 전용 설명은 DS402 Home에 한해 이 문서가 우선한다.
`LMC_Home` 자체의 CurrentPositionZero 설계는 기존 문서를 그대로 따른다.

이 문서는 구현 구조를 설명한다. source 구현 완료, PLC runtime 확인, 실축 qualification과 production release는 같은 판정이 아니다.

## 2. 현재 기능 범위

현재 public 기능명은 `LMC_HomeDS402`다. Elmo Maestro/MMCLib의 `HomeDS402(MMC_HOMEDS402...)` 의도를
SIGMATEK/LASAL 구조에 맞춰 SDK public API + TCP lifecycle + PLC DS402 state machine으로 이식한다.
Maestro C++ 함수를 PLC 안에서 직접 호출하는 구조가 아니다.

지원 Homing Method는 다음과 같다.

- 표준 이동 method: `1..14`, `17..30`, `33`, `34`
- 현재 위치 method: `37`
- method `35`는 obsolete로 취급한다.
- reserved/vendor-specific method는 fail-closed 한다.

기본 API에서 지원하는 DS402 Homing object는 다음과 같다.

| 의미 | DS402 object | SDK / wire 표현 | current 규칙 |
|---|---|---|---|
| Homing Method | `0x6098` | `HomingMethod` | 위 allowlist만 허용 |
| Home Offset | `0x607C` | `Position` | raw drive DINT, `-2000000000..2000000000` |
| Home Velocity 1 | `0x6099:01` | `Velocity` / `HomeVelocity1` | 이동 method는 `> 0`, method 37은 `0` |
| Home Velocity 2 | `0x6099:02` | `DistanceLimit` / `HomeVelocity2` | 기존 72-byte wire 호환 필드 사용; 이동 method는 `> 0`, method 37은 `0` |
| Home Acceleration | `0x609A:00` | `Acceleration` | 이동 method는 `> 0`, method 37은 `0` |
| Torque limit | project field | `TorqueLimit` | basic API에서는 `0` 고정 |
| Buffer mode | project field | `BufferMode` | `Aborting`만 허용 |
| Overall watchdog | project field | `TimeoutMilliseconds` | nonzero PLC-side Home watchdog |

`DistanceLimit`라는 기존 wire 이름은 ABI 호환을 위해 유지하지만 current DS402 Home에서는 HomeVelocity2 저장소다.
distance-limit homing semantics는 basic API에서 지원하지 않는다.

## 3. Public API 계약

응용 프로그램/WPF는 아래 API를 직접 사용한다.

```text
PrepareLMC_HomeDS402(...)
  -> LMC_HomeDS402(...) / LMC_HomeDS402Async(...)
  -> ReadDs402HomeOutcome(...) / ReadDs402HomeOutcomeAsync(...)
  -> RetireDs402HomeOutcome(...) / RetireDs402HomeOutcomeAsync(...)
```

wire lifecycle은 고정한다.

| 단계 | Public API | Command | 의미 |
|---|---|---:|---|
| Prepare | `PrepareLMC_HomeDS402` | 없음 | parameter, capability, identity admission. Start 전송 없음 |
| Start once | `LMC_HomeDS402[Async]` | `0x7D15` | Start acknowledgement. 완료 증거 아님 |
| Outcome | `ReadDs402HomeOutcome[Async]` | `0x7D16` | Running/terminal retained result 조회 |
| Retire | `RetireDs402HomeOutcome[Async]` | `0x7D17` | exact terminal generation 폐기 |

`PrepareDs402Home`, `Ds402Home`, `Ds402HomeAsync`는 기존 호출자 호환 wrapper다. 신규 WPF/응용 코드는
`PrepareLMC_HomeDS402`, `LMC_HomeDS402[Async]`를 사용한다.

## 4. PLC 실행 상태 머신

현재 핵심 상태 흐름은 다음과 같다.

```text
request/identity/axis admission
-> specialized owner reserve
-> current 0x6061 read + supported pre-Home mode 저장
-> homing parameters write
   0x607C Home Offset
   0x6098 Homing Method
   0x6099:01 Home Velocity1
   0x6099:02 Home Velocity2
   0x609A:00 Home Acceleration
-> RT control owner acquire
-> 0x6060 = 6 (Homing mode)
-> 0x6061 = 6 readback 확인
-> ControlWord bit 4 rising edge
-> fresh HomingAttained/no-error/position snapshot 확인
-> ControlWord bit 4 LOW
-> LASAL setpoint/destination alignment
-> 저장한 pre-Home mode를 0x6060에 복원
-> 0x6061 exact readback 확인
-> RT owner release
-> fresh post-release snapshot
-> terminal outcome commit
-> exact 0x7D17 retirement 대기
```

Home 전에 저장/복원 가능한 mode는 PP `1`, PV `3`, IP `7`, CSP `8`이다. 따라서 mode 1에서 Home을 시작했으면
cleanup 뒤 mode 1로 돌아간다. CSP `8`로 고정 복귀하지 않는다.

## 5. 완료 판정

Start ACK는 완료가 아니다. terminal success는 drive가 실제 Homing 결과를 갱신한 뒤의 fresh snapshot으로 판정한다.

필수 조건은 다음과 같다.

- StatusWord Homing attained bit 12 확인
- Homing error bit 13 clear
- Fault bit 3 clear
- expected raw ActualPosition 기준 `+/-32 count` 범위
- bit 4 LOW cleanup 완료
- LASAL setpoint/destination alignment 완료
- 저장한 pre-Home mode exact restore + `0x6061` readback 완료
- RT owner release 후 fresh snapshot 확보
- pending SDO/drain/mode-restore/uncertainty가 없음

ExpectedPosition은 current contract에서 다음과 같다.

- method 37: `HomeOffset`
- 이동 method: `-HomeOffset`

TargetReached(bit 10)는 captured StatusWord에 진단값으로 보존하지만 terminal success gate로 사용하지 않는다.
드라이브별 bit 10 타이밍 차이 때문에 HomingAttained 이후 불필요하게 실패시키지 않기 위한 현재 계약이다.

Master Position도 success predicate가 아니다. current LASAL alignment가 사용하는
`LMCAXIS_SET_SETPOS_APPUNIT_DEST`는 Master Position 갱신을 보장하지 않으므로 setpoint/destination과
raw ActualPosition을 완료 근거로 사용한다.

## 6. 실패와 복구

write boundary 이후 응답 timeout/disconnect가 발생하면 original `0x7D15` Start를 자동 replay하지 않는다.
복구는 exact recovery key로 `0x7D16`을 조회하고 terminal이면 `0x7D17`로 retire하는 query/retire-only 흐름을 사용한다.

cleanup 실패는 성공으로 축소하지 않는다. 특히 다음은 terminal failure 또는 quarantine 대상이다.

- bit 4 LOW 복구 실패
- saved mode restore/readback 실패
- RT owner release 불확실
- pending SDO/drain 불확실
- identity/BootId/MapRevision 불일치

최초 Home 실패 원인은 cleanup 후속 오류로 덮어쓰지 않는다.

## 7. WPF parameter editor

WPF `DS402 Home` UI는 아래 필드를 직접 편집한다.

- Homing Method
- Home Offset
- Home Velocity1
- Home Velocity2
- Home Acceleration
- Timeout

Method 37 선택 시 Velocity1/2와 Acceleration은 모두 `0`이어야 한다. 이동 method는 세 값이 모두 양수여야 한다.
입력은 SDK `LMCAxisDs402HomeParameters` validation을 다시 통과하므로 UI만으로 허용 범위를 우회하지 않는다.

## 8. 검증 상태와 경계

2026-09-14 현재 판정은 다음처럼 분리한다.

- source/API/state-machine/parameter editor: 구현 완료
- WPF public API direct call: 구현 완료
- 기존 Method 37 DS402 Home: 사용자가 실기에서 정상 구동 확인
- Method 37의 packet/fault/restart 전체 qualification matrix: 별도 미완료
- 이동 Homing Method `1..14`, `17..30`, `33`, `34`: 개별 실축 검증 대기
- switch/index polarity, method별 배선과 실제 이동 방향/종단 조건: 장비별 검증 필요
- production 전체 승인: 별도 판정

따라서 Method 37의 사용자 실기 성공을 다른 moving method 전체 PASS로 확대하지 않는다.

## 9. current source references

- `LMC_Library/LMC_API_Delivery/src/LmcAxisDs402Home.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAdminDs402Home.cs`
- `LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeModels.cs`
- `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`
- `LMC_Library/LasalApiWpfTestApp/API_MAPPING.md`
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st`
- `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st`

이 문서와 source가 충돌하면 current `dev` source와 frozen packet map을 우선한다.

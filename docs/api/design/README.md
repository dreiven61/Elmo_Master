# 최우선 API 개발 설계

## 2026-09-14 current entrypoint

현재 `dev`의 DS402 Home 설계 정본은 다음 순서로 본다.

1. `DS402_HOME_IMPLEMENTATION_DESIGN_20260914.md` - current DS402 Home public API, parameter, PLC sequence, completion/recovery 계약
2. `DS402_HOME_MOVING_METHOD_QUALIFICATION_20260914.md` - moving method별 실축 qualification 기록 양식과 PASS 기준
3. `LMC_HOME_AND_DS402_HOME_IMPLEMENTATION_DESIGN_20260911.md` - LMC Home과 2026-09-11 시점 DS402 Home 통합 설계의 historical baseline
4. `HOME_DS402_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` - frozen Start/Outcome/Retire lifecycle과 qualification history
5. `HOME_DS402_DESIGN.md` - 초기 HomeDS402 wire/state-machine 설계와 과거 qualification history

문서 간 설명이 충돌하면 current `dev` source와
`DS402_HOME_IMPLEMENTATION_DESIGN_20260914.md`를 우선한다.

- current integration branch: `dev`
- DS402 Home implementation baseline: `dev@97e3ae433ef470ce815a79c9b8d8fda0201bd367`
- current API progress: `../API_DEVELOPMENT_PROGRESS.md`
- current API manual: `../API_MANUAL.md`
- production release posture: **NO-GO**

## 1. 2026-09-14 DS402 Home current contract

DS402 Home은 더 이상 Method 37 전용 surface가 아니다. current basic API는 다음 standard method를
허용한다.

- `1..14`
- `17..30`
- `33`, `34`
- `37`

Method 35와 reserved/vendor-specific method는 fail-closed한다.

편집 가능한 drive object는 다음과 같다.

| UI/API parameter | DS402 object | current 의미 |
|---|---|---|
| Homing Method | `0x6098` | 허용 standard method 선택 |
| Home Offset | `0x607C` | raw drive DINT |
| Home Velocity1 | `0x6099:01` | moving method search velocity 1 |
| Home Velocity2 | `0x6099:02` | moving method search velocity 2; legacy wire `DistanceLimit` 저장소 사용 |
| Home Acceleration | `0x609A:00` | moving method acceleration |

moving method는 Velocity1/2와 Acceleration이 모두 양수여야 하고, Method 37은 세 값이 모두 0이어야 한다.
TorqueLimit는 0, BufferMode는 Aborting을 유지한다.

## 2. current public API / wire lifecycle

응용 프로그램은 다음 public API를 사용한다.

```text
PrepareLMC_HomeDS402(...)
-> LMC_HomeDS402(...) / LMC_HomeDS402Async(...)
-> ReadDs402HomeOutcome(...) / ReadDs402HomeOutcomeAsync(...)
-> RetireDs402HomeOutcome(...) / RetireDs402HomeOutcomeAsync(...)
```

wire command는 다음과 같다.

| 단계 | Command |
|---|---:|
| Start | `0x7D15` |
| Outcome | `0x7D16` |
| Retire | `0x7D17` |

`PrepareDs402Home`, `Ds402Home`, `Ds402HomeAsync`는 기존 호출자 호환 wrapper다. 신규 WPF/SDK 호출은
정식 `LMC_` public API를 사용한다.

Start ACK는 completion evidence가 아니다. write boundary 이후 결과가 불명확해도 original Start를
자동 replay하지 않고 동일 recovery key로 Outcome/Retire만 수행한다.

## 3. PLC Homing sequence

current backend sequence는 다음이다.

```text
pre-Home 0x6061 mode 저장
-> Homing parameter SDO 설정
-> 0x6060 = 6
-> 0x6061 = 6 확인
-> ControlWord bit 4 rising edge
-> fresh HomingAttained/no-error/expected raw position 확인
-> bit 4 LOW
-> LASAL setpoint/destination alignment
-> 저장한 pre-Home mode를 0x6060에 복원
-> 0x6061 exact readback
-> owner release
-> terminal outcome commit
```

pre-Home mode는 PP(1), PV(3), IP(7), CSP(8)를 지원한다. Home 종료 시 CSP로 고정 복귀하지 않고
시작 전에 저장한 exact mode로 돌아간다.

## 4. completion contract

terminal success의 핵심 predicate는 다음이다.

- fresh HomingAttained bit 12
- HomingError bit 13 clear
- Fault bit 3 clear
- raw ActualPosition이 expected position `+/-32 count`
- bit 4 LOW cleanup
- LASAL setpoint/destination alignment
- saved pre-Home mode exact restore/readback
- owner release와 terminal record commit

expected raw position은 Method 37에서 `HomeOffset`, moving method에서 `-HomeOffset`이다.
TargetReached bit 10은 진단 StatusWord에 보존하지만 terminal success gate로 사용하지 않는다.
Master Position은 success predicate가 아니다.

## 5. current validation state

2026-09-14 사용자 확인으로 기존 DS402 Home 정상 구동이 확인됐다. 이 evidence는 Method 37 실기
확인으로 기록한다. moving method는 사용자가 method별로 개별 실축 테스트한다.

따라서 current 판정은 다음과 같다.

- public API / WPF parameter editor / PLC state-machine implementation: **완료**
- Method 37 physical/runtime: **사용자 확인 완료**
- moving method individual qualification: **대기**
- full fault/disconnect/restart/packet matrix: **대기**
- production release: **NO-GO**

한 method의 실축 PASS를 다른 method 또는 전체 production PASS로 확대하지 않는다.

## 6. historical document interpretation

2026-09-11 및 2026-09-02 문서의 다음 문구는 historical 설계다.

- `Method 37 only`
- `Home Offset = 0 only`
- moving Home은 `HomeDS402Ex`만 사용
- cleanup 뒤 CSP(8) 고정 복귀
- TargetReached가 terminal success 필수 gate
- Master Position exact update가 성공 조건

frozen `0x7D15/16/17` lifecycle, exact identity, durable recovery, no-replay, terminal retirement와
source/runtime/hardware evidence 분리 원칙은 계속 유효하다.

## 7. 다른 current design 문서

- `ASYNC_OPERATION_WATCHDOG_AND_TRANSPORT_RECOVERY_DESIGN_20260909.md` - WPF async/gate timeout과 transport recovery
- `CURRENT_IMPLEMENTATION_HANDOFF_20260907.md` - 2026-09-07 historical integration handoff
- `REMAINING_IMPLEMENTATION_DESIGN_20260902.md` - remaining feature dependency plan
- `SET_POSITION_COMPLETION_IMPLEMENTATION_DESIGN_20260902.md` - SetPosition durable runtime handoff
- `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md` - SetPosition source inventory
- `SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md` - SetOperationMode historical investigation

## 8. 공통 구현 원칙

- `dev`를 current integration source truth로 사용한다.
- static/PC, LASAL build, PLC load/runtime, hardware/packet evidence를 분리한다.
- mutation write 이후 original command 자동 replay를 하지 않는다.
- terminal proof 전에 shared owner를 release하지 않는다.
- generated LASAL artifact/hash를 근거 없이 rebaseline하지 않는다.
- physical drive count/topology가 바뀌면 topology baseline을 다시 확인한다.
- 개별 method/axis PASS를 다른 method/axis 또는 production 전체 PASS로 확대하지 않는다.

# DS402 Home 개발 진행도 - 2026-09-14

- 기준 branch: `dev`
- 기준 source: `97e3ae433ef470ce815a79c9b8d8fda0201bd367` (`dev : DS402Home implementation`)
- 대상: `LMC_HomeDS402`, wire `0x7D15 / 0x7D16 / 0x7D17`
- 기능 구현 판정: **요청 범위 구현 완료**
- 전체 qualification / production 판정: **별도 진행**

이 문서는 DS402 Home의 current 진척도를 정리한다. 전체 API 진척도와 충돌하면 DS402 Home 항목에 한해
이 문서를 우선하고, 다른 API는 `API_DEVELOPMENT_PROGRESS.md`를 따른다.

## 1. 완료된 개발 항목

| 항목 | 상태 | current 내용 |
|---|---|---|
| Public API 정식 명칭 | 완료 | `PrepareLMC_HomeDS402`, `LMC_HomeDS402[Async]`, `ReadDs402HomeOutcome[Async]`, `RetireDs402HomeOutcome[Async]` |
| 기존 API 호환 | 완료 | `PrepareDs402Home`, `Ds402Home[Async]`는 compatibility wrapper 유지 |
| WPF 호출 구조 | 완료 | WPF가 정식 public API를 직접 호출 |
| Homing Method 편집 | 완료 | `1..14`, `17..30`, `33`, `34`, `37` |
| Home Offset | 완료 | `0x607C`, raw drive DINT |
| Home Velocity1 | 완료 | `0x6099:01` |
| Home Velocity2 | 완료 | `0x6099:02`; legacy wire `DistanceLimit` 필드 사용 |
| Home Acceleration | 완료 | `0x609A:00` |
| Method별 validation | 완료 | moving method는 Velocity1/2/Acceleration `>0`; method 37은 모두 `0` |
| Mode 6 진입/readback | 완료 | `0x6060=6`, `0x6061=6` 확인 |
| Completion logic | 완료 | fresh HomingAttained/no-error + expected position `+/-32 count` |
| TargetReached 처리 | 완료 | 진단 StatusWord에는 보존, terminal gate에서는 제외 |
| Master Position 처리 | 완료 | success predicate에서 제외; setpoint/destination alignment 사용 |
| 이전 운전 모드 복귀 | 완료 | Home 전 PP1/PV3/IP7/CSP8 저장 후 exact mode 복원/readback |
| Start/Outcome/Retire lifecycle | 완료 | Start ACK와 terminal proof를 분리; exact retire 적용 |
| no-replay recovery | 완료 | post-write 불확실성에서 original Start 자동 재전송 금지 |

## 2. 현재 검증 상태

2026-09-14 사용자 확인으로 기존 DS402 Home 동작은 정상 구동이 확인됐다. 이 시점의 실제 확인 범위는
기존 Method 37 기반 동작으로 취급한다. 이는 사용자 실기 확인이며 전체 packet/fault/restart qualification을 대신하지 않는다.

현재 검증 상태를 층별로 나누면 다음과 같다.

| Evidence layer | 상태 | 판정 |
|---|---|---|
| Source/API | 완료 | standard method parameter와 정식 API surface가 current `dev`에 구현됨 |
| WPF 기능 | 완료 | parameter editor + public API direct call 구현됨 |
| PLC DS402 state machine | 구현 완료 | parameter SDO, Homing mode, bit4, completion, cleanup, saved-mode restore 포함 |
| Method 37 실기 | 사용자 확인 완료 | 정상 구동 확인. 전체 negative/fault matrix는 별도 |
| Moving methods 개별 실기 | 대기 | 사용자가 method별로 진행 예정 |
| Packet causal proof | 대기 | 필요 시 method별 캡처 |
| Fault / disconnect / restart matrix | 대기 | release qualification 시 수행 |
| Production release | 미승인 | 다른 method의 배선/방향/종단 조건까지 확대 검증 필요 |

## 3. 기존 진행도 문서에서 변경된 점

기존 `API_DEVELOPMENT_PROGRESS.md`와 2026-09-11 이전 설계의 아래 표현은 current DS402 Home에는 더 이상 맞지 않는다.

- `Method 37 only`
- `Home offset 0 only`
- `moving Home은 반드시 HomeDS402Ex`
- `CSP 8로 고정 복귀`
- `Master Position exact update가 성공 조건`
- `TargetReached가 terminal success의 필수 gate`

current 구현은 standard method allowlist와 editable Home parameters를 지원하고, Home 전 운전 모드를 저장해 exact mode로 복귀한다.

## 4. 남은 검증 작업

사용자가 개별 테스트를 수행한다. 다음 테스트는 구현 완료 판정과 분리한다.

1. Method `1..14`, `17..30`, `33`, `34` 중 실제 장비에서 사용할 method별 배선과 동작 확인
2. Home switch / limit / index의 polarity와 실제 방향 확인
3. Home Velocity1/2 및 Acceleration의 장비 단위/안전 범위 확인
4. nonzero Home Offset에 대해 terminal ActualPosition이 current expected-position contract와 맞는지 확인
5. timeout, drive fault, homing error, Stop/PowerOff preemption 확인
6. Start ACK loss, disconnect, reconnect/restart에서 Start replay 없이 Outcome/Retire로 복구되는지 확인
7. saved pre-Home mode가 PP/PV/IP/CSP 각각에서 exact 복귀하는지 확인

이 문서 갱신 작업에서는 개별 PLC/실축 테스트를 실행하지 않는다.

## 5. 다음 판정 기준

개별 moving method는 다음 증거가 있을 때 해당 method만 runtime qualified로 올린다.

- Start가 정확히 한 번 전송됨
- 실제 drive가 Homing mode로 진입함
- method-specific switch/index/limit 동작이 의도와 일치함
- HomingAttained/no-error terminal 확인
- expected raw ActualPosition 범위 확인
- bit 4 LOW cleanup 완료
- pre-Home mode exact restore/readback 확인
- terminal `0x7D16` 조회와 `0x7D17` retire 완료

한 method의 PASS를 다른 method 또는 production 전체 PASS로 확대하지 않는다.

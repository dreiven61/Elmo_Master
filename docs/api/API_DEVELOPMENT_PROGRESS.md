# LASAL Motion Control API 개발 진척도

- 문서 버전: 1.6-current
- 기준일: 2026-09-14
- API: `LasalMotionControlLib 0.9.1-preview`
- current integration branch: `dev`
- DS402 Home implementation baseline: `dev@97e3ae433ef470ce815a79c9b8d8fda0201bd367`
- 릴리스 판정: **production NO-GO**

이 문서는 API 구현률, current qualification, 제한과 다음 작업의 정본이다. API 사용법은
[API 설명서](API_MANUAL.md), byte offset과 frame shape는
[DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt), 상세 설계는
[design/README.md](design/README.md)를 따른다.

## 1. 2026-09-14 current override

DS402 Home은 `dev@97e3ae4`에서 기존 Method 37 전용 경로에서 standard DS402 homing method와
parameter editor를 지원하는 공개 API 형태로 확장됐다.

- 정식 public API: `PrepareLMC_HomeDS402`, `LMC_HomeDS402[Async]`,
  `ReadDs402HomeOutcome[Async]`, `RetireDs402HomeOutcome[Async]`
- compatibility wrapper: `PrepareDs402Home`, `Ds402Home[Async]`
- wire lifecycle: Start `0x7D15`, Outcome `0x7D16`, Retire `0x7D17`
- 지원 method: `1..14`, `17..30`, `33`, `34`, `37`
- 편집 object: `0x6098`, `0x607C`, `0x6099:01`, `0x6099:02`, `0x609A:00`
- moving method는 Home Velocity1/2와 Acceleration이 모두 양수여야 한다.
- Method 37은 Home Velocity1/2와 Acceleration이 모두 0이어야 한다.
- Home 진입 전 PP(1), PV(3), IP(7), CSP(8) 중 현재 `0x6061` mode를 저장하고 cleanup 뒤 exact mode로 복귀한다.
- 완료는 fresh HomingAttained(bit12), HomingError clear(bit13), Fault clear와 expected raw position `+/-32 count`로 판정한다.
- TargetReached(bit10)는 진단값이며 terminal success gate가 아니다.
- Master Position은 success predicate가 아니다.
- Start ACK는 completion evidence가 아니며 post-write 불확실성에서 original Start를 replay하지 않는다.

사용자는 2026-09-14 기존 DS402 Home의 정상 구동을 확인했다. 현재 evidence 범위는 기존 Method 37
실기 확인으로 기록한다. 다른 moving method는 사용자가 method별 개별 테스트를 진행할 예정이며,
한 method의 PASS를 다른 method 또는 production 전체 PASS로 확대하지 않는다.

## 2. 판정 기준

`구현`, `빌드`, `다운로드`, `실행`, `실축 효과`를 같은 PASS로 취급하지 않는다.

| 등급 | 뜻 | 이 등급만으로 증명하지 않는 것 |
|---|---|---|
| PC | C# build, fake-RPC, parser, WPF 회귀 | PLC source 실행, 실축 효과 |
| Source/static | LASAL source/route/ABI/mutation contract | C78 compile, PLC runtime |
| IDE/artifact | C78 compile/link와 generated artifact | PLC download 후 실행 |
| PLC load | PLC link/download/SystemInit/project load | command terminal, motion, fault/soak |
| PLC runtime | online state와 command terminal/readback | 물리 좌표, switch/index, packet causal proof |
| Hardware/packet | 실축 결과와 packet causal evidence | 다른 축/method/fault/restart matrix 전체 |

상태 표기는 다음으로 제한한다.

- `Active`: current route와 gate가 열려 지원 계약으로 호출 가능
- `Limited`: route는 열려 있으나 축/대상/qualification 범위 제한
- `Dormant`: source/runtime이 있어도 capability/gate가 닫혀 정상 호출 금지
- `Missing`: public surface 또는 요구사항에 대응하는 current LASAL 실행 경로 없음

## 3. current 요약

- C# protocol ID는 77개, current LASAL route는 76개다. C#-only command는 `0x7E23 SubmitDigitalOutputWrite` 하나다.
- 요구사항 workbook 65개 중 완전/적응 구현은 `41/65`, 부분 포함은 `53/65`다. 이는 semantic coverage이며 PLC/hardware 시험 통과율이 아니다.
- Connection, Axis/Group core, Admin read, LMC Home, DS402 Home, Diagnostics D1/D2/D3와 제한된 D5는 source-active다.
- DS402 Home은 요청된 public API/parameter/state-machine 구현이 완료됐다. Method 37은 사용자 실기 확인 완료, moving method는 개별 실축 qualification 대기다.
- SetOperationMode는 PP/PV/IP/CSP, exact requested-mode ACK, one-shot `0x6060`, read-only `0x6061` settling, durable no-replay outcome/retire까지 구현 완료다.
- Generic SDO Write는 source/static 구현이 완료됐지만 physical Write/readback matrix는 미완료다.
- SetPosition은 SDK/wire/route/P1 async lifecycle이 있으나 durable backend와 native exactly-once execution이 미완료라 `Dormant`다.
- HomeDS402Ex는 scaffold/store/profile preparation은 있으나 physical runtime이 no-op이고 capability bit 11은 OFF다.
- production 판정은 계속 **NO-GO**다.

## 4. 요구사항 커버리지

| 분류 | 개수 | 의미 |
|---|---:|---|
| D | 17 | 직접 대응 구현 |
| E | 24 | SIGMATEK/LASAL 방식으로 적응 구현 |
| P | 12 | 부분 구현 또는 qualification 제한 |
| G | 8 | 설계/게이트만 존재 |
| X | 4 | current scope 제외 또는 미구현 |
| 합계 | 65 | workbook 요구사항 수 |

- 완전/적응 구현: `41/65 = 63.1%`
- 부분 포함: `53/65 = 81.5%`
- High-priority 21개 관점: Active 19, Partial 1(SetPosition), Dormant 1(HomeDS402Ex)

## 5. 기능별 current 상태

| 영역 | Command / API | 상태 | current 경계 |
|---|---|---|---|
| Connection/RPC | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` | Active | bounded reconnect; callback은 wake hint, final state는 TCP readback |
| Axis core | `0x2022/23/24/28/2E/209F/20A0/20A2` | Active/Limited | accepted-once wait/no-replay recovery; 전체 축/fault/race matrix 미완료 |
| Group core | `0x20D2/2045/2047-4B/2051/2085/20A4/20E7/7D22` | Active/Limited | Cartesian4 profile, software robot Axis1..9 power lifecycle; 전체 live matrix 미완료 |
| Admin read | `0x7D00/7D10/7D20/7D22` | Active | capability + allowlisted semantic key |
| LMC Home | `0x7D13/7D18/7D19` | Active/Limited | `0x7D19`가 snapshot만 반환하고 `ZeroHomeState`를 비우지 않던 same-boot 반복 실행 직접 원인 수정, generation 보존 actual retire 반영; old-boot 완전 terminal startup 정리 및 v2 receipt layout 수정; MoveReference mode 2..4 입력 구현, WPF moving timeout 기본 60000 ms; Block fail-closed; IDE build/download 및 반복/mode별 실기 확인 대기 |
| DS402 Home | `0x7D15/7D16/7D17` | Active/Limited | standard method/API/parameter 구현 완료; Method 37 사용자 실기 확인, moving method 개별 qualification 대기 |
| SetPosition | `0x7D12/7D14/7D1A` | Dormant | volatile store, runtime/native execution fail-closed |
| HomeDS402Ex | `0x7D1B/7D1C/7D1D` | Dormant | physical runtime no-op, bit 11 OFF |
| SetOperationMode | `0x7D23/7D24/7D25` | Active | PP/PV/IP/CSP, one-shot write/read-only verify, durable outcome/retire |
| Diagnostics capability | `0x7E00` | Active | 매 connection fresh BootId/MapRevision/mask 필요 |
| D1/D2 | `0x7E01/02/10/20`, `0x7E30-33` | Active/Limited | typed catalog/PI/Bulk; fault/partial/soak 확대 필요 |
| D3 Recorder | `0x7E40-49` | Active/Limited | Single/Ring/Trigger, single recorder owner |
| D4 Double | `0x7E4A-4D` | Dormant | capability/proof gate OFF |
| D5 SDO Read | `0x7E50` read | Active/Limited | general inline read, exact ticket identity 필요 |
| D5 SDO Write | `0x7E50` write | Limited | generic 1/2/4-byte scalar policy/source 완료, physical write/readback matrix 미완료 |
| Encoder maintenance | `0x7E53/54/55` | Active/Limited | TW20/TW19 fixed payload; terminal과 physical effect 구분 |
| Static topology | `0x7E11/12` | Active | configured inventory, runtime health 증거 아님 |
| Dynamic node/DI | `0x7E13/22` | Dormant | route/source 존재, capability OFF |
| Digital output write | `0x7E23` | Missing | C# surface만 존재, LASAL route/bit17 없음 |
| PI Write | `0x7E21` | Dormant | capability/allowlist OFF |
| Extended SDO result | `0x7E51` | Dormant | bit 12 OFF |

## 6. DS402 Home current checkpoint

### 6.1 구현 완료 범위

| 항목 | 상태 |
|---|---|
| 정식 public API 호출 구조 | 완료 |
| compatibility wrapper | 완료 |
| WPF parameter editor | 완료 |
| Method allowlist | 완료 |
| Home Offset / Velocity1 / Velocity2 / Acceleration | 완료 |
| Mode 6 진입/readback | 완료 |
| bit 4 start sequence | 완료 |
| HomingAttained/no-error/position completion | 완료 |
| TargetReached diagnostic-only 처리 | 완료 |
| Master Position success predicate 제거 | 완료 |
| pre-Home mode exact restore | 완료 |
| Start/Outcome/Retire durable lifecycle | 완료 |
| post-write no-replay recovery | 완료 |

### 6.2 현재 실기 evidence

| Evidence | 상태 |
|---|---|
| Method 37 정상 동작 | 사용자 확인 완료 |
| PP/PV/IP/CSP 각각의 restore matrix | 개별 확대 검증 대기 |
| Method `1..14`, `17..30`, `33`, `34` | 사용자가 개별 테스트 예정 |
| switch/index/limit polarity와 direction | method별 확인 필요 |
| nonzero Home Offset | method별 확인 필요 |
| timeout/fault/Stop/PowerOff matrix | release qualification 시 확인 |
| disconnect/restart no-replay | release qualification 시 확인 |
| packet causal proof | 필요 시 method별 캡처 |

## 7. SetOperationMode checkpoint

SetOperationMode는 구현 완료 상태를 유지한다.

- Start `0x7D23`, Outcome `0x7D24`, Retire `0x7D25`
- PP(1), PV(3), IP(7), CSP(8)
- exact requested-mode ACK/readback
- `0x6060` one-shot write
- original deadline 안의 read-only `0x6061` settling
- terminal owner publish/release
- durable no-replay recovery
- raw Generic SDO `0x6060` 우회 금지

## 8. HomeDS402Ex / SetPosition

HomeDS402Ex는 DS402 Home standard-method 확장으로 대체된 항목과 별개로, 별도 approved-profile/
retained lifecycle 실험 경로로 `Dormant`를 유지한다. capability bit 11과 physical execution은 열지 않는다.

SetPosition은 public SDK/wire가 존재하지만 durable backend/native exactly-once execution이 없으므로
current 지원 API로 승격하지 않는다.

## 9. current 개발 우선순위

1. DS402 Home moving method 중 실제 장비에서 사용할 method부터 개별 실축 검증
2. method별 switch/index/limit polarity, direction, velocity, acceleration, offset 검증
3. PP/PV/IP/CSP pre-Home mode restore matrix 확인
4. timeout, fault, Stop/PowerOff, disconnect/restart no-replay matrix 확대
5. Generic SDO physical write/readback qualification
6. SetPosition durable backend/native executor prerequisite 확보
7. repository/release 문서와 배포 artifact 동기화

사용자가 개별 DS402 Home 실축 테스트를 수행하므로 이 문서 갱신에서 별도 PLC/실축 테스트를 실행하지 않는다.

## 10. production release gate

production `Active` 승격에는 API별로 같은 승인 세트에서 다음이 모두 필요하다.

1. PC contract
2. Source/static + method-size
3. fresh C78/ARM + generated artifact review
4. same-image PLC load/runtime
5. normal/fault/timeout/disconnect/response-loss hardware/packet matrix
6. paired capability/gate activation
7. manual/progress/WPF release synchronization

DS402 Home 기능 구현 완료와 Method 37 정상 구동 확인은 위 전체 release gate 완료를 뜻하지 않는다.
Generic SDO physical completion, moving Home method qualification, HomeDS402Ex, SetPosition과 dormant/missing
surface가 남아 있으므로 전체 API의 production 판정은 계속 **NO-GO**다.

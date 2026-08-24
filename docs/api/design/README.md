# 최우선 API 개발 설계

- 기준일: 2026-08-24
- 범위: 개발 진행표의 우선순위 `상`이면서 진행도 75% 미만인 4개 API
- 상태: 구현 착수 가능, production 활성화는 각 문서의 최종 gate 통과 전까지 금지

이 폴더는 아래 4개 API의 current 설계와 실행 순서를 한곳에서 관리한다. 실제 구현된
byte offset은 `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`가 정본이며, 이 폴더의
신규 command offset은 source와 packet map에 반영되기 전까지 설계 예약값이다.

## 1. 대상과 우선순위

| 순서 | API | 현재 진행도 | 개발 성격 | 설계 |
|---:|---|---:|---|---|
| 1 | `HomeDS402` | 50% | 기존 `0x7D15/16/17` 경로의 활성화·실축 적격화 | [HOME_DS402_DESIGN.md](HOME_DS402_DESIGN.md) |
| 2 | `SetOpMode` | 25% | PC/SDK+dormant route 완료; `AxisOperationMode` owner ABI는 동결, Control/SDO lifecycle source 미구현 | [SET_OPERATION_MODE_DESIGN.md](SET_OPERATION_MODE_DESIGN.md) |
| 3 | `HomeDS402Ex` | 0% | 기존 Home과 분리된 확장 Homing 신규 구현 | [HOME_DS402_EX_DESIGN.md](HOME_DS402_EX_DESIGN.md) |
| 4 | `SetPosition` | 25% | P1 async lifecycle/volatile Store까지 완료; durable backend와 RT exactly-once 후속 구현 | [SET_POSITION_DESIGN.md](SET_POSITION_DESIGN.md) |

순서는 단순한 중요도 순위가 아니라 의존성 순서다. `HomeDS402`는 이미 존재하는 가장 짧은
실축 완료 경로다. `SetOpMode`의 owner/resource numeric ABI는 고정됐고, 이 ABI를 실제
Control/Diagnostics source에 반영해야 `HomeDS402Ex`가 DS402 mode와 SDO executor 공유 규칙을
안전하게 재사용할 수 있다. `SetPosition`은 별도 작업 흐름으로 즉시 시작하되,
내구 저장소와 RT task 증거가 필요하므로 activation은 가장 마지막에 수행한다.

## 2. command ID 예약

| API | Start | ReadOutcome | Retire | 상태 |
|---|---:|---:|---:|---|
| HomeDS402 | `0x7D15` | `0x7D16` | `0x7D17` | current source/wire에 존재 |
| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | 이 설계에서 예약, 아직 source 미반영 |
| SetOpMode | `0x7D23` | `0x7D24` | `0x7D25` | C# source/golden wire와 LASAL dormant route 반영; capability OFF |
| SetPosition | `0x7D12` | `0x7D14` | `0x7D1A` | current source/wire에 존재, runtime fail-closed |

`0x7D21`은 향후 Group parameter write 후보이고 `0x7D22`는 이미 Group relative move가
사용하므로 예약 범위에서 제외했다. 신규 ID는 C# protocol, TCP route, LASAL dispatch,
packet map과 golden-byte test를 한 변경 단위로 반영한다.

## 3. 즉시 개발 큐

### Wave 0 - 같은 기준선 고정

1. current source/static, method-size, C78, generated artifact와 PLC load tuple을 기록한다.
2. 시험축, E-stop, software/hardware limit, encoder scale와 정지 상태를 확인한다.
3. 4개 API에 공통으로 사용할 axis mutation ownership과 no-auto-replay 규칙을 고정한다.
4. 다른 신규 API는 이 4개가 최소 dormant-source DoD를 통과할 때까지 후순위로 둔다.

### Wave 1 - 즉시 병렬 착수

- HomeDS402: 기존 5개 activation switch의 정합 verifier와 axis 1 bench runner를 준비한다.
- SetOpMode: PC/SDK, dormant fail-closed route와 owner/resource numeric ABI freeze까지 완료했다.
  다음 단계는 Control/Diagnostics의 owner kind 6 + shared Diagnostics SDO resource 4 계약을
  source에 반영하고 `6061 -> 6060 -> 6061` executor의 dormant lifecycle을 구현하는 것이다.
- SetPosition: `_FileSys` dual-file A/B backend와 축 1~4 task/core/priority 증거를 준비한다.

### Wave 2 - source 완성

- HomeDS402: C78 candidate와 method 37 normal/fault/recovery matrix를 닫는다.
- SetOpMode: exact SDO write/readback, no-replay recovery와 terminal store를 구현하되 capability는 OFF로 유지한다.
- HomeDS402Ex: 승인된 axis profile과 method allowlist를 입력으로 dormant source를 구현한다.
- SetPosition: durable A/B journal과 RT claim/native/stable-3 observer를 구현한다.

### Wave 3 - 장비 적격화와 paired activation

각 API를 축 1부터 독립적으로 검증하고 축 2~4로 확대한다. command start ACK, terminal
outcome, retire, packet, physical effect와 fault/reconnect를 모두 묶은 증거가 있어야 해당
capability를 켠다. 한 API의 PASS를 다른 API의 activation 근거로 사용하지 않는다.

## 4. 공통 안전 계약

- mutation command는 같은 축에서 단일 owner만 가진다.
- TCP write 경계를 넘은 뒤 결과가 불확실하면 원 명령을 자동 재전송하지 않는다.
- start ACK는 완료가 아니다. terminal query와 generation-bound retire가 완료 증거다.
- session, socket, request, diagnostics build/BootId/MapRevision과 128-bit intent를 정확히 묶는다.
- terminal proof 전에 owner release, ordinary response 또는 다음 mutation을 허용하지 않는다.
- timeout, disconnect, corrupt result와 owner drift는 성공/일반 실패로 축소하지 않고
  quarantine 또는 recovery-required로 보존한다.
- capability OFF와 deterministic fail-closed response를 먼저 구현한다.
- PC/static, C78, PLC load, PLC runtime, hardware/packet 결과를 별도 등급으로 기록한다.

## 5. 공통 Definition of Done

각 API는 다음 항목을 모두 충족해야 `Active`로 승격한다.

1. C# public API, immutable model/result와 synchronous/async API가 있다.
2. exact golden request/response와 malformed/truncated/trailing parser test가 있다.
3. TCP route와 LASAL handler가 exact reference/identity/size를 fail-closed 검증한다.
4. axis mutation ownership, timeout, disconnect, close와 no-replay mutation test가 있다.
5. LASAL custom method가 32 KiB 미만이고 full SourceOnly가 같은 tree에서 PASS한다.
6. LASAL IDE C78 Rebuild/Link와 generated declaration/artifact가 PASS한다.
7. 같은 image의 PLC link/download/project load가 PASS한다.
8. 정상, 거부, timeout, fault, disconnect, response-loss와 recovery/retire를 실축에서 검증한다.
9. packet capture와 final state/readback이 물리 효과와 일치한다.
10. capability, packet map, API manual, 개발 진척도와 WPF 노출을 paired release로 갱신한다.

## 6. 진행 관리 규칙

- 작업 ID는 각 설계문서의 체크리스트 ID를 그대로 issue/commit 제목에 사용한다.
- 한 commit에는 한 작업 ID의 source, verifier와 문서만 넣는 것을 원칙으로 한다.
- capability와 compile-time gate 변경은 별도 activation commit으로 분리한다.
- source 구현 완료와 hardware qualification 완료를 같은 진행률로 기록하지 않는다.
- current 상태와 다음 gate는 [API 개발 진척도](../API_DEVELOPMENT_PROGRESS.md)에만 기록한다.

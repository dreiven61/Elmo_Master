# 최우선 API 개발 설계

- 기준일: 2026-08-25
- 범위: 개발 진행표의 우선순위 `상`이면서 진행도 75% 미만인 4개 API
- 상태: source 구현과 qualification을 병행하되 production 활성화는 각 문서의 최종 gate 통과 전까지 금지
- active development branch: `dev`

이 폴더는 아래 4개 API의 current 설계와 실행 순서를 한곳에서 관리한다. 실제 구현된
byte offset은 `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`가 정본이며, 이 폴더의
신규 command offset은 source와 packet map에 반영되기 전까지 설계 예약값이다.

## 1. 대상과 우선순위

| 순서 | API | 현재 진행도 | 개발 성격 | 설계 |
|---:|---|---:|---|---|
| 1 | `HomeDS402` | 50% | 기존 `0x7D15/16/17` 경로의 activation·실축 적격화 | [HOME_DS402_DESIGN.md](HOME_DS402_DESIGN.md) |
| 2 | `SetOpMode` | 60% | owner/SDO/no-replay/preemption/D5 deny source와 MODE-10 static, MODE-13 PC/WPF recovery PASS; fresh C78/PLC/hardware 남음 | [SET_OPERATION_MODE_DESIGN.md](SET_OPERATION_MODE_DESIGN.md) |
| 3 | `HomeDS402Ex` | 0% | HOMEEX-06 `SCAFFOLD_OFF` source/static PASS; full-identity ownership/runtime 후속 | [HOME_DS402_EX_DESIGN.md](HOME_DS402_EX_DESIGN.md) |
| 4 | `SetPosition` | 25% | P1 async lifecycle/volatile Store까지 완료; durable backend와 RT exactly-once 후속 구현 | [SET_POSITION_DESIGN.md](SET_POSITION_DESIGN.md) |

순서는 단순한 중요도 순위가 아니라 의존성 순서다. `HomeDS402`는 이미 존재하는 가장 짧은
실축 완료 경로다. `SetOpMode`의 owner/resource와 SDO lifecycle source가 `dev`에 들어왔고
MODE-13 PC/WPF durable recovery도 PASS했으므로 `HomeDS402Ex`는 이 공유 규칙을 후속 설계
입력으로 사용할 수 있다. 다만 SetOpMode activation은 latest source의 C78/PLC/hardware proof
전까지 닫아 둔다. `SetPosition`은 별도 작업 흐름으로 내구 저장소와 RT task 증거를 준비한다.

## 2. command ID 예약

| API | Start | ReadOutcome | Retire | 상태 |
|---|---:|---:|---:|---|
| HomeDS402 | `0x7D15` | `0x7D16` | `0x7D17` | current source/wire에 존재 |
| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | LASAL diagnostics route/scaffold 존재, runtime gate/capability OFF |
| SetOpMode | `0x7D23` | `0x7D24` | `0x7D25` | C#/LASAL lifecycle + WPF durable recovery 구현; compile gate/capability OFF |
| SetPosition | `0x7D12` | `0x7D14` | `0x7D1A` | current source/wire에 존재, runtime fail-closed |

`0x7D21`은 향후 Group parameter write 후보이고 `0x7D22`는 이미 Group relative move가
사용하므로 예약 범위에서 제외했다. 신규 ID는 C# protocol, TCP route, LASAL dispatch,
packet map과 golden-byte test를 한 변경 단위로 반영한다.

## 3. 즉시 개발 큐

### Wave 0 - 같은 기준선 고정

1. current source/static, method-size, C78, generated artifact와 PLC load tuple을 분리 기록한다.
2. 시험축, E-stop, software/hardware limit, encoder scale와 정지 상태를 확인한다.
3. axis mutation ownership과 no-auto-replay 규칙을 공통 계약으로 유지한다.
4. `codex/*` 임시 branch의 unique diff/evidence는 개발 완료 전까지 보존한다.

### Wave 1 - current 병렬 작업

- HomeDS402: 기존 activation switch 정합 verifier와 axis 1 bench runner를 준비한다.
- SetOpMode source: MODE-02/06/07/08/09와 MODE-10 method split/static qualification까지 `dev`에 반영됐다.
- SetOpMode PC/WPF: MODE-13 pre-dispatch journal, startup/reconnect no-replay recovery와 definitive-reject durable archive가 PASS했다.
- SetOpMode IDE/hardware: latest source를 fresh C78/ARM Rebuild/Link하고 artifact identity를 검토한 뒤
  MODE-11/12 packet/hardware matrix로 진행한다.
- SetPosition: `_FileSys` dual-file A/B backend와 축 1~4 task/core/priority 증거를 준비한다.

### Wave 2 - source/PC 완성

- HomeDS402: C78 candidate와 method 37 normal/fault/recovery matrix를 닫는다.
- SetOpMode: MODE-13 PC/WPF gate는 닫혔다. compile gate와 capability는 OFF 유지하고 MODE-11/12 장비 증거를 준비한다.
- HomeDS402Ex: HOMEEX-06 dormant parser/state/outcome scaffold는 완료했다. HOMEEX-07에서 full 116-byte owner identity bank와 OwnerKind 7/ResourceKind 3 admission을 paired 구현한다.
- SetPosition: durable A/B journal과 RT claim/native/stable-3 observer를 구현한다.

### Wave 3 - 장비 적격화와 paired activation

각 API를 축 1부터 독립적으로 검증하고 축 2~4로 확대한다. command start ACK, terminal
outcome, retire, packet, physical effect와 fault/reconnect를 모두 묶은 증거가 있어야 해당
capability를 켠다. 한 API의 PASS를 다른 API의 activation 근거로 사용하지 않는다.

## 4. SetOpMode current qualification boundary

현재 `dev` source/PC에는 아래가 존재한다.

- `AxisOperationMode OwnerKind=6`, Diagnostics SDO `ResourceKind=4`, lifecycle admission 4
- active owner state 12, exact Start identity 56 bytes
- `6061 -> 6060 -> 6061` runtime과 same-mode no-write path
- irreversible write dispatch evidence와 read-only no-replay recovery
- safety preemption cleanup/quarantine
- generic D5 `0x6060` permanent deny
- processor 3-way method split
- static verifier/workflow
- WPF pre-dispatch exact durable journal과 startup/reconnect no-replay recovery
- definitive `0x7D23` rejection의 checksum-protected evidence archive와 exact-identity fail-closed 해제

최종 qualification branch의 source/static checkpoint는 `57 checks PASS`, 세 processor 모두
32 KiB 미만, `0x6060` write site main 0 / mutation 4 / recovery 0이다. MODE-13 Windows
qualification은 Debug/Release 각각 `12/12 PASS`, build `0 warnings / 0 errors`, diff hygiene PASS다.
상세 증거는 [MODE-13 WPF recovery evidence](evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md)에 기록했다.
full SourceOnly는 source gate 이후 기존 `Classes.lcb` physical identity ratchet에서 STOP했다.

이 증거로 fresh C78/PLC/hardware PASS를 주장하지 않는다. current final source에 대해 다음은
아직 별도 수행해야 한다.

1. C78/ARM Rebuild/Link
2. generated artifact identity/ABI review
3. same image BootId/MapRevision/build tuple
4. MODE-11 same-mode/no-write 및 exact one-write/readback packet
5. MODE-12 timeout/disconnect/mismatch/quarantine/retire 축 1..4 matrix
6. MODE-14 paired capability activation

## 5. 공통 안전 계약

- mutation command는 같은 축에서 단일 owner만 가진다.
- TCP write 경계를 넘은 뒤 결과가 불확실하면 원 명령을 자동 재전송하지 않는다.
- start ACK는 완료가 아니다. terminal query와 generation-bound retire가 완료 증거다.
- session, socket, request, diagnostics build/BootId/MapRevision과 128-bit intent를 정확히 묶는다.
- terminal proof 전에 owner release, ordinary response 또는 다음 mutation을 허용하지 않는다.
- timeout, disconnect, corrupt result와 owner drift는 성공/일반 실패로 축소하지 않고
  quarantine 또는 recovery-required로 보존한다.
- capability OFF와 deterministic fail-closed 경계를 activation 전까지 유지한다.
- PC/static, C78, PLC load, PLC runtime, hardware/packet 결과를 별도 등급으로 기록한다.

## 6. 공통 Definition of Done

각 API는 다음 항목을 모두 충족해야 `Active`로 승격한다.

1. C# public API, immutable model/result와 synchronous/async API가 있다.
2. exact golden request/response와 malformed/truncated/trailing parser test가 있다.
3. TCP route와 LASAL handler가 exact reference/identity/size를 fail-closed 검증한다.
4. axis mutation ownership, timeout, disconnect, close와 no-replay mutation test가 있다.
5. LASAL custom method가 32 KiB 미만이고 full SourceOnly의 source gate가 같은 tree에서 PASS한다.
6. LASAL IDE C78 Rebuild/Link와 generated declaration/artifact가 PASS한다.
7. 같은 image의 PLC link/download/project load가 PASS한다.
8. 정상, 거부, timeout, fault, disconnect, response-loss와 recovery/retire를 실축에서 검증한다.
9. packet capture와 final state/readback이 물리 효과와 일치한다.
10. capability, packet map, API manual, 개발 진척도와 WPF 노출을 paired release로 갱신한다.

## 7. 진행 관리 규칙

- current 개발 기준은 `dev` branch다. 기능 구현과 문서 업데이트는 원칙적으로 `dev`에 모은다.
- `codex/*` 작업 branch는 구현·시험용 임시 branch로 보고 current 상태 판정이나 정본 링크에
  사용하지 않는다.
- 개발 중인 임시 branch는 unique diff와 시험 흔적을 보존하기 위해 유지한다. 개발 완료 후
  해당 변경이 `dev`에 반영되었거나 폐기되었고 필요한 증거가 보존된 것을 확인한 뒤 stale
  branch를 일괄 정리한다.
- 작업 ID는 각 설계문서의 체크리스트 ID를 그대로 issue/commit 제목에 사용한다.
- capability와 compile-time gate 변경은 별도 activation commit으로 분리한다.
- source 구현 완료와 hardware qualification 완료를 같은 진행률로 기록하지 않는다.
- current 상태와 다음 gate는 [API 개발 진척도](../API_DEVELOPMENT_PROGRESS.md)에만 기록한다.

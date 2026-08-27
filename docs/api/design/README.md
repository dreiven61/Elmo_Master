# 최우선 API 개발 설계

- 기준일: 2026-08-27
- 기준 branch/HEAD: `dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd`
- 범위: 개발 진행표의 우선순위 `상`이면서 진행도 75% 미만인 4개 API
- 상태: source 구현과 qualification을 병행하되 production 활성화는 각 문서의 최종 gate 통과 전까지 금지
- active development branch: `dev`

이 폴더는 아래 4개 API의 current 설계와 실행 순서를 한곳에서 관리한다. 최신 통합 상태 요약은
[DEVELOPMENT_STATUS_20260827.md](DEVELOPMENT_STATUS_20260827.md)를 따른다. 2026-08-26 snapshot은
[DEVELOPMENT_STATUS_20260826.md](DEVELOPMENT_STATUS_20260826.md)에 historical checkpoint로 유지한다.
실제 구현된 byte offset은 `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`가 정본이다.

## 1. 대상과 우선순위

| 순서 | API | 현재 진행도 | 개발 성격 | 설계 |
|---:|---|---:|---|---|
| 1 | `HomeDS402` | 50% | H37-01/02/03/04/10 current-dev qualification 완료; fresh C78/artifact와 hardware/activation 후속 | [HOME_DS402_DESIGN.md](HOME_DS402_DESIGN.md) |
| 2 | `SetOpMode` | 65% | PP/PV/IP/CSP software target SDK/PLC/WPF path까지 통합; activation OFF, SupportedModeMask/C78/PLC/hardware 후속 | [SET_OPERATION_MODE_DESIGN.md](SET_OPERATION_MODE_DESIGN.md) |
| 3 | `HomeDS402Ex` | 40% | HOMEEX-05/06/07/12 + profile/approved-plan gate + source/static/collector 통합; issue #28/#35와 actual runtime/hardware 후속 | [HOME_DS402_EX_DESIGN.md](HOME_DS402_EX_DESIGN.md) |
| 4 | `SetPosition` | 25% | P1 async lifecycle/volatile Store까지 완료; durable backend와 RT exactly-once 후속 구현 | [SET_POSITION_DESIGN.md](SET_POSITION_DESIGN.md) |

이 진행도는 checklist 완료 개수의 단순 비율이 아니라 release-oriented 개발 진행 수치다. PC/SDK,
source/static, C78/generated artifact, PLC load/runtime, hardware/packet과 activation을 별도 gate로 본다.

## 2. command ID current 상태

| API | Start | ReadOutcome | Retire | 상태 |
|---|---:|---:|---:|---|
| HomeDS402 | `0x7D15` | `0x7D16` | `0x7D17` | source/wire + H37 software qualification 존재, activation OFF |
| HomeDS402Ex | `0x7D1B` | `0x7D1C` | `0x7D1D` | route + full-identity ownership + retained store + profile/approved-plan preparation 존재, physical runtime/capability OFF |
| SetOpMode | `0x7D23` | `0x7D24` | `0x7D25` | PP/PV/IP/CSP software mutation/recovery + WPF selector 구현, compile gate/capability OFF |
| SetPosition | `0x7D12` | `0x7D14` | `0x7D1A` | source/wire 존재, runtime fail-closed |

## 3. current 개발 큐

### Wave 0 — evidence grade 고정

1. current source/static, method-size, C78/generated artifact, PLC load/runtime, hardware/packet을 분리 기록한다.
2. mutation ownership과 no-auto-replay 규칙을 공통 계약으로 유지한다.
3. capability/gate activation은 hardware qualification과 별도 final changeset으로 유지한다.
4. 상태 판정은 항상 current `dev`를 기준으로 한다.

### Wave 1 — current blocker 해소

- **HomeDS402**: PR #40으로 H37-02/03/04/10을 current `dev`에 통합했다. 다음은 H37 fresh C78 evidence collector와 H37-05/06 artifact closure다.
- **SetOpMode**: PP/PV/IP/CSP software implementation까지 통합. 다음은 PLC SupportedModeMask, current exact-image C78/PLC artifact와 MODE-11/12 hardware evidence다.
- **HomeDS402Ex**: issue #28 axis profile 승인과 issue #35 fresh C78/generated-artifact closure가 actual runtime의 선행 조건이다.
- **SetPosition**: `_FileSys` durable A/B backend와 RT exactly-once/native evidence를 준비한다.

### Wave 2 — physical qualification

- HomeDS402 H37-07 Axis1 matrix -> H37-08 Axis2~4
- SetOperationMode MODE-11/12 Axis1 -> Axis2~4
- HomeDS402Ex HOMEEX-08 actual runtime은 issue #28/#35 완료 후 구현 -> HOMEEX-10/11

### Wave 3 — paired activation

- HomeDS402 H37-09 five-gate + bit 6
- SetOperationMode MODE-14 bits 8/9/10
- HomeDS402Ex HOMEEX-13 bit 11 + WPF Start UI

한 API의 C78/hardware PASS를 다른 API의 activation 근거로 사용하지 않는다.

## 4. HomeDS402 current qualification boundary

PR #40 current-dev qualification 결과:

- H37 atomic activation: `43 checks PASS`
- ownership/preemption: `21 checks PASS`
- method-size: `10 checks PASS`, `ProcessAxisDs402Home` 29,497 bytes
- WPF durable no-replay: `36 checks PASS`
- API Debug/Release full suites PASS
- WPF Debug/Release H37 smoke PASS
- diff hygiene PASS
- workflow run `33026506170`, successful rerun job `98369296568`

full SourceOnly은 source/static gate를 통과한 뒤 다음 exact artifact boundary에 도달한다.

`LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`

따라서 H37-05/06은 아직 미완료다. fresh C78/ARM rebuild + generated artifact identity review 없이
artifact ratchet을 자동 갱신하지 않는다.

## 5. SetOperationMode current qualification boundary

현재 `dev`에는 다음이 존재한다.

- `AxisOperationMode OwnerKind=6`, Diagnostics SDO `ResourceKind=4`, active state 12
- exact Start identity 56 bytes
- `6061 -> 6060 -> 6061` runtime과 same-mode no-write path
- irreversible write dispatch 뒤 read-only no-replay recovery
- safety preemption cleanup/quarantine
- generic D5 `0x6060` permanent deny
- MODE-10 three-way method split + source/static verifier
- MODE-13 WPF pre-dispatch durable journal + startup/reconnect recovery

WPF dynamic recovery localization은 PR #39에서 양쪽 recovery workflow Debug/Release를 통과했다.
이 UI qualification은 C78/PLC/hardware evidence가 아니다.

다음 gate:

1. current source C78/ARM rebuild/link + artifact review
2. MODE-11 same-mode zero-write / exact one-write packet evidence
3. MODE-12 fault/disconnect/quarantine/retire axis matrix
4. MODE-14 paired activation

## 6. HomeDS402Ex current qualification boundary

현재 `dev`에는 HOMEEX-03/04/05/06/07/12 software tranche와 HOMEEX-08 preparation,
HOMEEX-09 source/static + evidence collector가 존재한다.

- full 116-byte owner identity
- 4축 active + retired full outcome retained store
- exact 176-byte Outcome/Retire serialization
- duplicate/replay blocking + exact Retire retry
- approved profile -> checked frozen DINT plan internal gate
- WPF durable no-replay recovery
- C78 evidence collector

하지만 issue #28 actual axis profile과 issue #35 current-image artifact closure가 미완료다.
따라서 `LMC_DIAG_DS402_HOME_EX_ENABLED FALSE`, Admin bit 11 OFF, WPF Start UI OFF를 유지한다.

## 7. 공통 안전 계약

- mutation command는 같은 축에서 단일 owner만 가진다.
- TCP write 경계를 넘은 뒤 결과가 불확실하면 원 명령을 자동 재전송하지 않는다.
- Start ACK는 완료가 아니다. terminal query와 generation-bound retire가 완료 증거다.
- session/request/Diagnostics build/BootId/MapRevision/intent identity를 정확히 묶는다.
- timeout, disconnect, corrupt result와 owner drift는 성공으로 축소하지 않는다.
- capability OFF와 deterministic fail-closed 경계를 activation 전까지 유지한다.

## 8. 공통 Definition of Done

각 API는 다음을 모두 충족해야 `Active`로 승격한다.

1. public API와 immutable model/result
2. exact wire golden/malformed tests
3. TCP/LASAL exact parser/identity validation
4. ownership + no-replay recovery
5. method-size/source-static qualification
6. fresh C78 Rebuild/Link + generated artifact review
7. same-image PLC load/runtime
8. normal/fault/timeout/disconnect/response-loss hardware matrix
9. packet과 physical final state 일치
10. capability/UI/manual/progress paired release

## 9. branch / 진행 관리 규칙

- current 개발 기준은 `dev`다.
- merged/closed branch 존재 자체를 completion evidence로 사용하지 않는다.
- stale PR #31은 current-dev PR #40으로 selective requalification 후 CLOSED unmerged다.
- stale PR #37은 current-dev localization + PR #39 CI qualification으로 superseded되어 CLOSED unmerged다.
- PR #14는 C78 history/review, PR #18은 `DO NOT MERGE` SetOperationMode physical bench evidence로 유지한다.
- 작업 ID는 각 설계문서 체크리스트 ID를 issue/commit 제목에 사용한다.
- source 구현 완료와 hardware qualification 완료를 같은 진행률로 기록하지 않는다.

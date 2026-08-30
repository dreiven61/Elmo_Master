# LASAL Motion Control API 개발 진척도

- 문서 버전: 1.3-current
- 기준일: 2026-08-31
- API: `LasalMotionControlLib 0.9.1-preview`
- current integration branch: `dev`
- reviewed source baseline before this docs sync: `db954731c27c30f43f706b101276b81b022bd60a`
- 릴리스 판정: **production NO-GO**

이 문서는 API 구현률, latest current qualification, 제한과 다음 작업의 current 정본이다.
API 사용법은 [API 설명서](API_MANUAL.md), exact byte offset과 frame shape는
[DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt), 최우선 API 상세 설계는
[design/README.md](design/README.md)를 따른다. 과거 tranche별 상세 evidence는 design/evidence와
history 문서에 남기고 이 문서에는 current 판정만 유지한다.

## 1. 판정 기준

`구현`, `빌드`, `다운로드`, `실행`, `실축 효과`를 같은 PASS로 취급하지 않는다.

| 등급 | 뜻 | 이 등급만으로 증명하지 않는 것 |
|---|---|---|
| PC | C# build, fake-RPC, parser, WPF 회귀 | PLC source 실행, 실축 효과 |
| Source/static | LASAL source/route/ABI/mutation contract | C78 compile, PLC runtime |
| IDE/artifact | C78 compile/link와 generated artifact | PLC download 후 실행 |
| PLC load | PLC link/download/SystemInit/project load | command terminal, motion, fault/soak |
| PLC runtime | online state와 command terminal/readback | 물리 좌표·토크·encoder 효과 |
| Hardware/packet | 실축 결과와 packet causal evidence | 다른 축·fault·restart matrix 전체 |

상태 표기는 다음으로 제한한다.

- `Active`: current route와 gate가 열려 지원 계약으로 호출 가능
- `Limited`: route는 열려 있으나 축/대상/기능 또는 qualification 범위 제한
- `Dormant`: source/runtime이 있어도 capability/gate가 닫혀 정상 호출 금지
- `Missing`: public surface 또는 요구사항에 대응하는 current LASAL 실행 경로 없음

## 2. current 요약

- C# protocol ID는 77개, current LASAL route는 76개다. C#-only command는
  `0x7E23 SubmitDigitalOutputWrite` 하나다.
- 요구사항 workbook 65개 중 완전/적응 구현 `40/65`, 부분 포함 `53/65`다. 이는 semantic
  coverage이며 PLC/hardware 시험 통과율이 아니다.
- Connection, Axis/Group core, Admin read, LMC Home, Diagnostics D1/D2/D3와 제한된 D5는
  source-active다.
- SetPosition은 SDK/wire/route/P1 async lifecycle이 있으나 execution/native exactly-once와 durable
  backend가 미완료라 `Dormant`다.
- HomeDS402는 method 37 source/runtime에 더해 H37-02/03/04/10 hardware-independent qualification이
  current `dev`에 통합됐다. fresh C78/generated artifact, PLC/hardware와 activation은 미완료다.
- SetOperationMode는 PP/PV/IP/CSP lifecycle, supported mask `0x018A`, durable no-replay recovery와 live cross-mode preflight까지 qualification-active다. 17:28 실기에서 preflight 후 Diagnostics capability observation이 stale되는 host ordering blocker가 확인됐으며 실제 `0x6060` mutation은 아직 미도달이다.
- Generic SDO는 R03 generic 1/2/4-byte scalar Write, R04 exact editor/preview, R05 durable no-replay recovery와 safe-state corrective가 통합됐다. source gate는 ON이지만 physical Write/readback PASS는 아직 아니다.
- HomeDS402Ex는 wire/SDK/WPF/scaffold/full-identity ownership/retained store/profile preparation/source-static
  collector까지 통합됐지만 physical runtime은 no-op이고 bit 11은 OFF다.
- WPF dynamic SetOperationMode/HomeDS402Ex recovery localization은 current `dev`에서 양쪽 Debug/Release
  workflow가 green이다.
- full SourceOnly의 current known downstream blocker는 generated `Classes.lcb` physical identity ratchet이다.
  artifact identity는 fresh C78 build + review 없이 자동 갱신하지 않는다.
- production 판정은 계속 **NO-GO**다.

## 3. 요구사항 커버리지

| 분류 | 개수 | 의미 |
|---|---:|---|
| D | 16 | 직접 대응 구현 |
| E | 24 | SIGMATEK/LASAL 방식으로 적응 구현 |
| P | 13 | 부분 구현 또는 제한 활성화 |
| G | 8 | 설계/게이트만 존재 |
| X | 4 | current scope 제외 또는 미구현 |
| 합계 | 65 | workbook 요구사항 수 |

- 완전/적응 구현: `40/65 = 61.5%`
- 부분 포함: `53/65 = 81.5%`
- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home, SetOperationMode), Dormant 1(HomeDS402Ex)

## 4. 기능별 current 상태

| 영역 | Command / API | 상태 | current 경계 |
|---|---|---|---|
| Connection/RPC | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` | Active | bounded fresh-TCP reconnect; callback은 wake hint이며 final state는 TCP readback |
| Axis core | `0x2022/23/24/28/2E/209F/20A0/20A2` | Active/Limited | accepted-once wait/no-replay recovery; 전체 축/fault/race matrix 미완료 |
| Group core | `0x20D2/2045/2047-4B/2051/2085/20A4/20E7/7D22` | Active/Limited | X/Y/Z/U static identity; 전체 live matrix 미완료 |
| Admin read | `0x7D00/7D10/7D20/7D22` | Active | capability + allowlisted semantic key |
| LMC Home | `0x7D13/7D18/7D19` | Active/Limited | Admin bit 4 ON; no-motion CurrentPositionZero |
| SetPosition | `0x7D12/7D14/7D1A` | Dormant | volatile store, runtime/native execution fail-closed |
| DS402 Home | `0x7D15/7D16/7D17` | Dormant | H37 software/source qualification current-dev PASS, five activation gates + bit 6 OFF, fresh C78/hardware 미완료 |
| HomeDS402Ex | `0x7D1B/7D1C/7D1D` | Dormant | full identity/retained store/profile preparation/source-static 존재; physical runtime no-op, bit 11 OFF |
| SetOperationMode | `0x7D23/7D24/7D25` | Limited | PP/PV/IP/CSP qualification-active; current blocker는 preflight 뒤 Diagnostics capability freshness ordering, physical `0x6060` dispatch 미도달 |
| Diagnostics capability | `0x7E00` | Active | 매 connection fresh BootId/MapRevision/mask 필요 |
| D1/D2 | `0x7E01/02/10/20`, `0x7E30-33` | Active/Limited | typed catalog/PI/Bulk; fault/partial/soak 확대 필요 |
| D3 Recorder | `0x7E40-49` | Active/Limited | Single/Ring/Trigger, single recorder owner |
| D4 Double | `0x7E4A-4D` | Dormant | capability/proof gate OFF |
| D5 SDO Read | `0x7E50` read | Active/Limited | general inline read, exact ticket identity 필요 |
| D5 SDO Write | `0x7E50` write | Limited | generic safe non-semantic 1/2/4-byte scalar policy + durable recovery 통합; OperationEnabled/semantic raw object deny, hardware readback matrix 미완료 |
| Encoder maintenance | `0x7E53/54/55` | Active/Limited | TW20/TW19 fixed payload; terminal과 actual drive effect 구분 |
| Static topology | `0x7E11/12` | Active | configured inventory, runtime health 증거 아님 |
| Dynamic node/DI | `0x7E13/22` | Dormant | route/source 존재, bits 15/16 OFF |
| Digital output write | `0x7E23` | Missing | C# surface만 존재, LASAL route/bit17 없음 |
| PI Write | `0x7E21` | Dormant | capability/allowlist OFF |
| Extended SDO result | `0x7E51` | Dormant | bit 12 OFF |
| Distribution | SDK/WPF candidate | Blocked | development manual/semantic policy/distribution artifact release alignment 필요 |

## 5. HomeDS402 H37 current checkpoint

PR #40 `test(h37): qualify HomeDS402 source and recovery on current dev`가
`dev@1f741bfd08e9d75a52f7edd03862ef26ac562edd`로 통합됐다.

qualification:

- atomic five-value activation contract: **43 checks PASS**
- exact method37 `0x7D15/16/17` PC Start/Outcome/Retire contract: PASS
- shared ownership/preemption: **21 checks PASS**
- method-size verifier: **10 checks PASS**
  - `HandleAxisDs402HomeStart`: 22,041 bytes
  - `HandleAxisDs402HomeOutcome`: 7,255 bytes
  - `HandleAxisDs402HomeRetire`: 4,221 bytes
  - `ProcessAxisDs402Home`: 29,497 bytes < 32,768
- WPF durable no-replay source contract: **36 checks PASS**
- unresolved durable journal startup recovery-key reconstruction: PASS
- API Debug/Release full suites: PASS
- WPF Debug/Release MaintenanceJournal + Ds402Home smoke: PASS
- diff hygiene: PASS

Evidence:

- qualified head `f39fe0e9b56b0994619aed3f68b22c33a86d3b24`
- workflow run `33026506170`
- successful rerun job `98369296568`

첫 attempt는 four verifier PASS 후 hosted Windows runner MSBuild discovery에서만 중단됐고, 동일 head의
failed job rerun이 전체 green이었다. product/source workaround는 추가하지 않았다.

full SourceOnly은 source/static contract를 통과한 뒤 exact known generated-artifact boundary에서 멈춘다.

`LASAL.UdpCallbackContract blocker: SetPosition-augmented Classes.lcb physical identity drifted.`

따라서 current 판정:

- H37-01/02/03/04/10: 완료
- H37-05: method-size/source portion PASS, generated artifact closure 미완료
- H37-06: fresh C78/direct-open/network smoke 미완료
- H37-07/08: hardware matrix 미완료
- H37-09: activation 미완료

HomeDS402 capability bit 6과 five activation values는 계속 OFF다.

## 6. SetOperationMode current checkpoint

current `dev` source truth:

- qualification activation ON
- Admin Start/Outcome/Retire triad ON
- `SetOperationModeSupportedMask=0x018A` = PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- durable pre-dispatch arm, exact outcome, exact-generation retirement
- Write-dispatched 이후 original Start/`0x6060` replay 금지
- raw Generic SDO `0x6060` permanent deny
- same-target `SucceededNoWrite`와 real cross-mode를 구분
- cross-mode preflight: Standstill=True, Fault=False, OperationEnabled=False

PR #58 software evidence:

- API Debug full suite 1200/1200 PASS
- Generic SDO WPF focused smoke 17/17 PASS
- API/WPF Debug + Release build PASS
- corrective/static verifier PASS

이 evidence는 physical SetOperationMode PASS가 아니다.

2026-08-28 17:28 live finding:

```text
Axis1 currentMode=8 -> requestedMode=3 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=1 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=7 : preflight PASS, StatusWord=0x02D0
Axis1 currentMode=8 -> requestedMode=8 : same-target no-write candidate
```

모든 시도는 이후 다음 host exception으로 종료됐다.

```text
The supplied diagnostics capabilities are not the current observation.
```

root cause는 capability freshness ordering이다.

```text
RefreshDiagnosticsCapabilities -> observation N
ReadDriveStatusAsync
  -> 0x6041 inline D5 -> Diagnostics.GetCapabilities -> N+1
  -> 0x6061 inline D5 -> Diagnostics.GetCapabilities -> N+2
PrepareSetOperationMode(cached N)
  -> requireCurrentObservation=true
  -> stale reject / ZERO mutation wire
```

따라서 현재 blocker는 PLC supported-mode reject가 아니며 `0x7D23`/`0x6060`까지 도달하지 않았다.
corrective는 freshness fence 제거가 아니라 preflight 뒤 FINAL Diagnostics capability refresh를 수행하는
ordering fix다.

완료 조건:

1. old observation이 preflight 후 stale reject되는 safety regression 유지
2. preflight 후 final Diagnostics refresh한 current observation으로 Prepare 성공
3. final refresh와 Prepare 사이 capability-producing call 없음
4. Prepare 성공 전 journal/Start mutation 0회
5. software regression green 후 Axis1 PP/PV/IP/CSP physical matrix 재개

## 7. HomeDS402Ex current checkpoint

current `dev` software tranche:

- HOMEEX-03 wire/capability contract
- HOMEEX-04 approved-plan lifecycle gate
- HOMEEX-05 retained exact-key/duplicate/replay/retire-retry store
- HOMEEX-06 gate-OFF parser/state scaffold
- HOMEEX-07 full 116-byte owner identity + shared DS402 Home resource admission
- HOMEEX-12 WPF durable no-replay recovery
- HOMEEX-08 approved-profile -> frozen DINT internal preparation gate
- HOMEEX-09 source/static verifier + C78 evidence collector

retained store:

- 4축 active 40-DINT + retired full-outcome 40-DINT records
- 176-byte Outcome/Retire exact serialization
- retained-store verifier 48/48 PASS

current hard gates:

- issue #28 actual axis 1..4 wiring/polarity/method/scale/range/rounding/MapRevision approval
- issue #35 same-tree fresh C78/generated artifact review + SourceOnly ratchet closure

그 전에는 다음을 열지 않는다.

- parameter SDO snapshot/program/restore
- mode 6 / controlword bit4 physical execution
- RT owner + physical homing observation
- capability bit 11
- WPF HomeDS402Ex Start UI

## 8. SetPosition current checkpoint

- SDK/wire/route와 P1 async lifecycle source 존재
- volatile `VAR_GLOBAL` backing 유지
- current production candidate에서 special RETAIN allocation 사용 안 함
- durable backend target은 `_FileSys` fixed dual-file A/B
- RT claim/native execution/stable observer와 exactly-once execution은 미완료
- capability/runtime은 fail-closed 유지

## 9. current 개발 우선순위

1. **SetOperationMode capability freshness ordering fix** — fresh drive preflight 뒤 FINAL Diagnostics capability refresh
2. focused regression — stale old observation reject + final-current observation Prepare success + zero-wire boundary
3. updated `dev` API/WPF Debug/Release validation
4. Axis1 SetOperationMode PP/PV/IP/CSP physical matrix (`0x6060` exact-one-write / `0x6061` readback)
5. Axis1 Generic SDO safe non-semantic 1/2/4-byte Write + exact readback matrix
6. SetOperationMode/Generic SDO timeout, disconnect, response-loss, durable no-replay recovery matrix
7. Axis2..4 확대
8. HomeDS402 fresh C78/generated artifact + hardware matrix
9. HomeDS402Ex approved profile / artifact closure
10. SetPosition issue #44 external blocker closure 후 durable A/B backend + RT exactly-once

## 10. branch / qualification 상태

- remote branch는 현재 `main`, `dev` 두 개만 유지한다.
- 2026-08-28 cleanup에서 기존 `codex/*` 29개가 모두 `dev` ancestor임을 확인한 뒤 삭제했다.
- 열린 PR은 현재 없다.
- `dev`가 유일한 integration / current qualification source truth다.
- qualification 중 blocker를 찾았다는 이유로 장기 branch를 새로 누적하지 않는다.
- 기능 작업 branch가 필요한 경우 작업 -> 검증 -> `dev` merge -> 즉시 삭제 원칙을 적용한다.
- source SHA, generated artifact, PLC loaded image, WPF binary identity를 같은 qualification evidence set으로 기록한다.

## 11. production release gate

production `Active` 승격에는 API별로 같은 승인 세트에서 다음이 모두 필요하다.

1. PC contract
2. Source/static + method-size
3. fresh C78/ARM + generated artifact review
4. same-image PLC load/runtime
5. normal/fault/timeout/disconnect/response-loss hardware/packet matrix
6. paired capability/gate activation
7. manual/progress/WPF release synchronization

현재 HomeDS402, SetOperationMode, HomeDS402Ex, SetPosition은 이 전체 gate를 닫지 못했으므로 production
판정은 계속 **NO-GO**다.

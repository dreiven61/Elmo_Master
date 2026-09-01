# LASAL Motion Control API 개발 진척도

- 문서 버전: 1.4-current
- 기준일: 2026-09-01
- API: `LasalMotionControlLib 0.9.1-preview`
- current integration branch: `dev`
- current source baseline: `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff` (`dev : SetOpMode Complete`)
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
- SetOperationMode는 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`에서 구현 완료로 닫혔다. PP/PV/IP/CSP, exact requested-mode ACK, one-shot `0x6060`, read-only `0x6061` settling, bounded owner publish, durable no-replay outcome/retire와 WPF terminal 처리까지 current Active 계약이다.
- Generic SDO는 R03~R05와 SWR-01~04 software 구현이 통합됐다. image/session transport proof,
  ordinary baseline/pre-write guard, journal v4 evidence, identity-pinned one-shot submit 및 PLC 1/2/4-byte
  parser가 source/static 기준 완료됐다. source gate는 ON이지만 physical Write/readback PASS는 아직 아니다.
- HomeDS402Ex는 wire/SDK/WPF/scaffold/full-identity ownership/retained store/profile preparation/source-static
  collector까지 통합됐지만 physical runtime은 no-op이고 bit 11은 OFF다.
- SetOperationMode 기능 구현은 완료됐지만 repository qualification hygiene는 별도 관리한다. current SetOperationMode static run은 기능/안전 contract 93개가 PASS하고 LASAL metadata Client 순서와 generated declaration 순서 불일치 1건에서 멈췄다. current WPF workflow failure는 hosted runner의 MSBuild 탐색 단계에서 발생해 test body가 실행되지 않은 CI 환경 문제다.
- generated LASAL artifact identity와 repository-wide SourceOnly ratchet은 HomeDS402/HomeDS402Ex 등 남은 기능의 release qualification에서 계속 명시적으로 검토하며 자동 갱신하지 않는다.
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
- High-priority 21개 관점: Active 18, Partial 2(SetPosition, DS402 Home), Dormant 1(HomeDS402Ex)

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
| SetOperationMode | `0x7D23/7D24/7D25` | Active | 구현 완료: PP/PV/IP/CSP, exact requested-mode ACK, one-shot write/read-only verify, durable no-replay outcome/retire |
| Diagnostics capability | `0x7E00` | Active | 매 connection fresh BootId/MapRevision/mask 필요 |
| D1/D2 | `0x7E01/02/10/20`, `0x7E30-33` | Active/Limited | typed catalog/PI/Bulk; fault/partial/soak 확대 필요 |
| D3 Recorder | `0x7E40-49` | Active/Limited | Single/Ring/Trigger, single recorder owner |
| D4 Double | `0x7E4A-4D` | Dormant | capability/proof gate OFF |
| D5 SDO Read | `0x7E50` read | Active/Limited | general inline read, exact ticket identity 필요 |
| D5 SDO Write | `0x7E50` write | Limited | SWR-01~04 + DMW-01~04 software 완료: qualification proof 없이 direct manual Arm/confirm 가능, generic nonzero ObjectIndex 1/2/4-byte policy + baseline/prewrite/journal v4 + one-shot/durable recovery. Read 뒤 `CapabilityObservationNotCurrent` UI 재잠금과 1-byte Write의 pre-submit quarantine evidence 4-byte 고정 결함을 2026-09-02 수정하고 focused smoke 18/18 PASS; 실기 로그의 `DetailCode=23`은 교체된 이전 terminal ticket 재조회로 확인해 stale ticket 폐기 적용. PLC/physical Write terminal 및 exact readback matrix 미완료 |
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

## 6. SetOperationMode 완료 checkpoint

SetOperationMode feature implementation은 `dev@0afbc2a79dff1b63f908b1bde3bd2502843045ff`
(`dev : SetOpMode Complete`)에서 완료 상태로 닫는다.

current source contract:

- qualification/runtime activation ON
- Admin Start/Outcome/Retire triad ON
- `SetOperationModeSupportedMask=0x018A` = PP(1), PV(3), IP(7), CSP(8)
- Homing(6)은 HomeDS402/HomeDS402Ex 소유
- Start `0x7D23`, ReadOutcome `0x7D24`, Retire `0x7D25`
- cross-mode fresh drive preflight: Standstill=True, Fault=False, OperationEnabled=False
- same-target `SucceededNoWrite`와 real cross-mode write 구분
- exact requested mode ACK/domain-failure echo; CSP(8) 고정 ACK 판정 제거
- `0x6060` exact requested-mode write 최대 1회
- normal verify mismatch는 original deadline 안에서 50ms 이상 간격의 `0x6061` read-only settling
- write-dispatched 이후 original Start/`0x6060` automatic replay 0회
- terminal owner publish/release는 original deadline 안에서 bounded retry, 추가 SDO write 0회
- terminal owner released + executor reusable evidence를 outcome에 포함
- WPF Running은 PASS가 아니며 exact-key `0x7D24` polling 후 terminal proof를 보존
- Succeeded는 exact-generation retire 이후에만 PASS
- Failed/Aborted는 terminal evidence/retire 후 실패로 반환
- indeterminate/query reject는 durable record와 mutation fence 유지
- raw Generic SDO `0x6060` permanent deny

2026-08-28 capability freshness blocker, 2026-08-31 readback/owner-publish 조사와 CSP-fixed ACK root cause는
모두 current implementation에서 corrective가 반영된 **historical investigation**이다. 상세 chronology는
`design/SET_OPERATION_MODE_READBACK_SETTLING_FIX_20260831.md`에 보존한다.

current CI를 feature implementation status와 혼동하지 않는다.

- SetOperationMode C78 evidence tool run `33455821803`: SUCCESS
- SetOperationMode static run `33455821841`: functional/safety checks **93 PASS**, metadata Client/generated declaration order mismatch **1 FAIL**
- SetOperationMode WPF run `33455821887`: hosted runner `Locate MSBuild` 실패로 test body 미실행

후자의 두 항목은 repository/CI qualification hygiene로 남기되 SetOperationMode 기능 구현을 다시
미완료 상태로 되돌리지 않는다. 전체 API production release 판정은 다른 미완료 기능과 release gate
때문에 계속 NO-GO다.

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

SetOperationMode 구현 완료 후 우선순위는 다음으로 재정렬한다.

1. **Generic SDO 완료 — issue #46의 잔여 범위**
   - SWR-01~04 software implementation은 완료
   - fresh C78 Rebuild/Link와 PLC download
   - Axis1 safe non-semantic 1/2/4-byte Write + exact readback
   - Manual Server / programmatic dual-entry BUSY arbitration과 race/no-wire 검증
   - timeout/disconnect/readback-mismatch durable no-replay recovery
   - Axis2..4 확대
2. **HomeDS402 — issue #32**
   - current exact `dev` C78/generated artifact review와 SourceOnly ratchet closure
   - same-image PLC/hardware 정상/fault/timeout matrix
   - activation은 독립 release gate 통과 전까지 OFF 유지
3. **HomeDS402Ex — issue #28 + #35**
   - 축1..4 wiring/polarity/method/scale profile 승인
   - fresh C78/generated artifact + SourceOnly closure
   - 이후에만 physical parameter program/restore와 homing runtime 진행
4. **SetPosition — issue #44**
   - vendor `CheckSum.CRC32` golden fixture 확보
   - LASAL IDE-generated `_FileSys` ABI 확보
   - 두 외부 prerequisite 없이는 durable A/B backend를 추측 구현하지 않음
5. **후순위 dormant/missing surface**
   - PI Write, Recorder Double, Dynamic node/DI, Extended SDO result activation 검토
   - `0x7E23` Digital Output Write LASAL route/owner/allowlist 구현
6. **Repository/release hygiene**
   - hosted Windows MSBuild discovery workflow 정리
   - LASAL metadata/generated declaration order 및 generated artifact ratchet 정합화
   - 기능별 source SHA / artifact / PLC image / WPF binary evidence set 정리

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

SetOperationMode feature implementation은 완료됐지만 Generic SDO physical completion, HomeDS402,
HomeDS402Ex, SetPosition과 dormant/missing surface가 이 전체 gate를 닫지 못했다. 따라서 전체 API의
production 판정은 계속 **NO-GO**다.

# LASAL Motion Control API 개발 진척도

- 문서 버전: 1.0-current
- 기준일: 2026-08-25
- API: `LasalMotionControlLib 0.9.1-preview`
- 기준 branch/HEAD: `dev@5a98162b5d48` + HOMEEX-06 current-progress 동기화
- 릴리스 판정: **production NO-GO**

이 문서는 API 구현률, 최신 검증 결과, artifact identity, 제한과 다음 작업의 단일 current
정본이다. API 사용법은 [API 설명서](API_MANUAL.md), byte offset과 frame shape는
[DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)을 따른다.
설계 이유와 과거 실험은 architecture/history 문서에 남기고 이 문서에 override를 누적하지
않는다.

현재 active development 기준은 `dev` branch다. `codex/*` 작업 branch는 구현·시험 과정의
임시 branch이며 current source of truth로 사용하지 않는다. 개발이 끝날 때까지 unique diff와
시험 흔적 보존을 위해 유지하고, `dev` 반영 또는 명시적 폐기와 증거 보존을 확인한 뒤 stale
branch를 일괄 정리한다.

## 1. 판정 기준

`구현`, `빌드`, `다운로드`, `실행`, `실축 효과`를 같은 PASS로 취급하지 않는다.

| 등급 | 뜻 | 현재 문서에서 증명하지 않는 것 |
|---|---|---|
| PC | C# build, fake-RPC, parser, WPF 회귀 | PLC source 실행, 실축 효과 |
| Source/static | LASAL source/route/ABI/mutation contract | C78 compile, PLC runtime |
| IDE/artifact | C78 compile/link와 generated artifact | PLC download 후 실행 |
| PLC load | PLC link/download/SystemInit/project load | command terminal, motion, fault/soak |
| PLC runtime | online state와 command terminal/readback | 물리 좌표·토크·encoder 효과 |
| Hardware/packet | 실축 결과와 packet causal evidence | 다른 축·fault·restart matrix 전체 |

상태 표기는 다음 네 가지로 제한한다.

- `Active`: current route와 gate가 열려 있고 지원 계약으로 호출 가능
- `Limited`: route는 열려 있지만 축/대상/기능 또는 적격성 범위가 제한됨
- `Dormant`: source/runtime이 있어도 capability/gate가 닫혀 정상 호출 금지
- `Missing`: public surface 또는 요구사항에 대응하는 current LASAL 실행 경로가 없음

## 2. 요약 결론

- C# protocol ID는 77개, current LASAL route는 76개다. C#-only 명령은
  `0x7E23 SubmitDigitalOutputWrite` 하나다.
- 요구사항 workbook 65개 중 완전/적응 구현은 `40/65`, 부분 구현 포함은 `53/65`다.
  이는 semantic coverage이며 PLC 시험 통과율이 아니다.
- Connection, Axis/Group core, Admin read, LMC Home, Diagnostics D1/D2/D3와 제한된 D5는
  source-active다. 각 기능의 실제 장비 적격성 범위는 서로 다르다.
- SetPosition `0x7D12/0x7D14/0x7D1A`는 SDK, wire, route와 P1 async lifecycle source를
  구현했지만 current runtime은 의도적으로 fail-closed다.
- SetOperationMode `0x7D23/0x7D24/0x7D25`는 PC/SDK lifecycle뿐 아니라 LASAL의
  `AxisOperationMode` owner, `6061 -> 6060 -> 6061` executor, write-dispatch 이후 no-replay
  recovery, safety preemption, outcome lifecycle과 generic D5 `0x6060` deny까지 source에 있다.
  compile-time gate `LMC_DIAG_SET_OPERATION_MODE_ENABLED`와 capability bits 8/9/10은 계속 OFF다.
- SetOperationMode MODE-10 source/static qualification은 최종 qualification branch에서
  57 checks PASS했고 common ownership/source gate도 통과했다. 해당 canonical source/verifier를
  `dev`에 복구했다. 그러나 이 최종 source의 fresh C78/ARM Rebuild/Link, PLC download/runtime,
  hardware/packet proof는 아직 없다.
- HomeDS402Ex `0x7D1B/0x7D1C/0x7D1D`는 HOMEEX-06에서 LASAL diagnostics route, 전용 scaffold state와 strict Start/Outcome/Retire parser를 구현했고 67-check `SCAFFOLD_OFF` source/static qualification을 통과했다. runtime gate와 Admin bit 11은 OFF이며 OwnerKind 7/full 116-byte owner identity, SDO/RT/motion execution은 HOMEEX-07 이후로 닫혀 있다.
- SetOperationMode MODE-13 PC/WPF recovery는 current Windows PR qualification에서
  Debug/Release 각각 `12/12 PASS`, build `0 warnings / 0 errors`, diff hygiene PASS다.
  Start 전 durable exact identity, startup/reconnect no-replay, terminal generation 저장 후
  exact `0x7D25` retire와 definitive Start rejection durable archive를 검증했다.
- full SourceOnly의 잔존 blocker는 source semantics가 아니라 기존 `Classes.lcb` physical
  identity ratchet mismatch다. artifact identity는 fresh C78 build와 의미 검토 전 자동 갱신하지 않는다.
- RETAIN 할당 실패를 없애기 위해 SetPosition backing은 1,344-byte ordinary volatile
  `VAR_GLOBAL`이다. 2026-08-20 historical image에서는 이전 `alloc for retain var failed`가
  재현되지 않았다.
- production 판정은 계속 **NO-GO**다.

## 3. 요구사항 커버리지

| 분류 | 개수 | 의미 |
|---|---:|---|
| D | 16 | 직접 대응 구현 |
| E | 24 | SIGMATEK/LASAL 방식으로 적응 구현 |
| P | 13 | 부분 구현 또는 제한 활성화 |
| G | 8 | 설계/게이트만 존재 |
| X | 4 | current scope에서 제외 또는 미구현 |
| 합계 | 65 | workbook 요구사항 수 |

- 완전/적응 구현: `D + E = 40/65 = 61.5%`
- 부분 포함: `D + E + P = 53/65 = 81.5%`
- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home, SetOperationMode),
  Dormant 1(`HomeDS402Ex`)

## 4. 기능별 current 상태

| 영역 | Command / API | 상태 | current 경계 |
|---|---|---|---|
| Connection/RPC | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` | Active | bounded fresh-TCP reconnect와 callback registration; callback은 wake hint이며 final state는 TCP readback 필요 |
| Axis core | `0x2022/23/24/28/2E/209F/20A0/20A2` | Active/Limited | accepted-once wait와 no-replay recovery 제공; 전체 축/fault/race 적격성 미완료 |
| Group core | `0x20D2/2045/2047-4B/2051/2085/20A4/20E7/7D22` | Active/Limited | X/Y/Z/U static identity와 bounded profile 계약; 전체 live matrix 미완료 |
| Admin read | `0x7D00/7D10/7D20/7D22` | Active | capability와 allowlisted semantic key를 사용; raw parameter passthrough 아님 |
| LMC Home | `0x7D13/7D18/7D19` | Active/Limited | Admin bit 4 ON; no-motion CurrentPositionZero이며 switch-search Home이 아님 |
| SetPosition | `0x7D12/7D14/7D1A` | Dormant | Store/ownership FALSE, max-jump 0, bits 3/5/7 OFF, volatile backing, native call 0, detail 24 |
| DS402 Home | `0x7D15/7D16/7D17` | Dormant | method 37 source, gate FALSE, Admin bit 6 OFF |
| HomeDS402Ex | `0x7D1B/7D1C/7D1D` | Dormant | HOMEEX-06 diagnostics route + dedicated scaffold state + strict Start/Outcome/Retire parser; 67-check `SCAFFOLD_OFF` PASS; runtime gate/bit 11 OFF, owner/SDO/RT/motion 미구현 |
| SetOperationMode | `0x7D23/7D24/7D25` | Dormant | owner kind 6/resource 4, SDO lifecycle, no-replay recovery, preemption, outcome, D5 `0x6060` deny와 MODE-13 WPF durable recovery 구현; compile gate/bits 8/9/10 OFF; fresh C78/PLC/hardware 미검증 |
| Diagnostics capability | `0x7E00` | Active | 매 connection에서 fresh `BootId`, `MapRevision`, mask를 읽어야 함 |
| D1/D2 | `0x7E01/02/10/20`, `0x7E30-33` | Active/Limited | typed catalog/PI/Bulk 경로; fault/partial/soak 확대 필요 |
| D3 Recorder | `0x7E40-49` | Active/Limited | Single/Ring/Trigger, single recorder owner; capture 적격성 확대 필요 |
| D4 Double | `0x7E4A-4D` | Dormant | capability/proof gate OFF |
| D5 SDO Read | `0x7E50` read | Active/Limited | general inline read; ticket terminal과 exact identity 필요 |
| D5 SDO Write | `0x7E50` write | Limited | Axis1 exact `0x2F00:24 Int32/4`만; `0x6060`은 permanent deny |
| Encoder maintenance | `0x7E53/54/55` | Active/Limited | TW20/TW19 fixed payload만; terminal과 실제 drive effect를 구분 |
| Static topology | `0x7E11/12` | Active | configured inventory; runtime health 증거가 아님 |
| Dynamic node/DI | `0x7E13/22` | Dormant | route/source는 있으나 bits 15/16 OFF |
| Digital output write | `0x7E23` | Missing | C# surface만 있고 LASAL route 없음, bit 17 OFF |
| PI Write | `0x7E21` | Dormant | capability/allowlist OFF |
| Extended SDO result | `0x7E51` | Dormant | bit 12 OFF |
| Distribution | SDK/WPF candidate | Blocked | 새 2.4-development manual과 기존 2.3 semantic policy/DOCX/PDF가 달라 build가 fail-fast |

추가 미구현/제한 항목은 SetOperationMode 실축 activation, generic Axis `SetParameter`, Group
parameter write, Axis override, typed emergency callback producer, profile conditioning pair와
SDO Real64/8-byte다.

## 5. 이번 개발 내용

### 5.1 SetPosition P1 async Control/TCP

- `ProcessAdminSetPositionAsync`를 별도 private method로 분리해 LASAL 32 KiB method budget을 지켰다.
- TCP는 `-13` pending, `-14` quarantine close와 기존 `-12` terminal durability uncertainty를 구분한다.
- duplicated pending tail, exact session/socket/request identity, closed-session notification,
  no-response quarantine와 queue retention 계약을 구현했다.
- RT preflight mailbox는 atomic publication, first coherent sequence, exact tuple revalidation과
  claim/native-zero를 검사한다.
- P1 ready path는 ownership active까지 진행한 뒤 pending으로 멈춘다. execution/native
  exactly-once와 durable backend는 아직 없다.

### 5.2 SetPosition Store와 RETAIN 결정

- Store ABI와 336-UDINT layout은 유지했다.
- backing은 ordinary volatile `VAR_GLOBAL`이다.
- target PLC의 special SRAMRETAIN allocation 부족으로 1,344-byte RETAIN 선언이 runtime link를
  실패시켰기 때문에 current production 후보에서 RETAIN을 제거했다.
- durable query/replay/retirement 다음 경로는 `_FileSys` fixed A/B file이다.

### 5.3 SetOperationMode MODE-02/06/07/08/09 source

current `dev`에는 다음이 반영돼 있다.

- `LMC_OWNER_KIND_AXIS_OPERATION_MODE = 6`
- shared Diagnostics SDO `ResourceKind = 4`
- lifecycle admission 4, active owner state 12, exact Start identity 56 bytes
- `0x6061` preflight -> 필요 시 exact one-byte `0x6060=8` -> `0x6061` verify
- same-mode `6061=8` no-write success path
- `WriteDispatched` 이후 original `0x6060` 자동 재전송 금지
- recovery는 `0x6061` read-only
- safety Stop/Power preemption snapshot/cleanup/quarantine source
- generic D5 Write의 `0x6060` permanent deny(detail 8)
- `AxisOperationModeState : ARRAY [0..191] OF DINT`
- terminal outcome/query/retire lifecycle

`AxisOperationMode`는 좌표 원점을 바꾸지 않으므로 Encoder maintenance와 달리
AxisRebaseRequired barrier를 set/clear하지 않는다.

### 5.4 SetOperationMode MODE-10 method split/static qualification

LASAL 32 KiB 제한을 위해 processor를 다음 3개로 분할했다.

- `ProcessAxisSetOperationMode`
- `ProcessAxisSetOperationModeMutationStages`
- `ProcessAxisSetOperationModeRecoveryStages`

qualification checkpoint:

| 검증 | 결과 | 증거 등급 |
|---|---:|---|
| SetOperationMode static verifier | `57 checks PASS` | Source/static |
| main/mutation/recovery method size | `19,895 / 19,731 / 14,251 bytes` LF 기준 | Source/static |
| `0x6060` write sites | main 0 / mutation 4 / recovery 0 | Source/static |
| common ownership/source ratchets | PASS | Source/static |
| LASAL 7-bit ASCII | PASS | Source/static |
| `git diff --check` | PASS | Source/static |
| full SourceOnly | STOP at existing `Classes.lcb` physical identity ratchet | Source/artifact boundary |

위 결과는 최종 qualification branch에서 검증된 canonical source semantics이고, 해당 source와
verifier/workflow를 `dev`에 복구했다. source/static PASS를 current C78/PLC/hardware PASS로
확대하지 않는다.

### 5.5 SetOperationMode MODE-13 WPF durable/no-replay recovery

PR #15 / squash commit `01bc9ed80b77a901b57afc8ee32a7b446a1f7f85`에서 다음을 닫았다.

- Start write 전 endpoint/build/BootId/map/128-bit intent/mode exact durable journal
- startup `ArmedBeforeDispatch -> RecoveryRequired` 승격과 endpoint lock
- accepted/uncertain 결과 이후 original `0x7D23` replay 금지
- recovery key의 Build/BootId/MapRevision exact match 및 mismatch zero-wire
- terminal outcome + exact RecordGeneration durable 저장 후 `0x7D25` retire
- retire 성공 이후에만 active journal resolve
- deterministic Start rejection은 PLC retained outcome 생성 전 failure ACK라는 source 의미를 확인
- definitive rejection의 exact identity/response/active journal bytes를 checksum-protected evidence로
  write-through 저장한 뒤에만 recovery interlock 해제
- reject identity mismatch/evidence failure는 fail-closed 유지

상세 checkpoint는
[MODE-13 WPF recovery evidence](design/evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md)에 기록했다.

## 6. 최신 검증 현황

### 6.1 PC와 source/static

| 검증 | 결과 | 증거 등급 | 제한 |
|---|---:|---|---|
| SDK Debug | `1164/1164 PASS` | PC historical | SetOperationMode 11개 포함; current restored LASAL build 증거 아님 |
| SDK Release | `1164/1164 PASS` | PC historical | 위와 동일 |
| WPF Debug | `356/356 PASS` | PC historical | MODE-13 구현 전 full-suite checkpoint |
| WPF Release | `356/356 PASS` | PC historical | 위와 동일 |
| SetOperationMode WPF Debug | `12/12 PASS` | PC current | PR run `32789073664`; build 0 warnings / 0 errors |
| SetOperationMode WPF Release | `12/12 PASS` | PC current | PR run `32789073664`; build 0 warnings / 0 errors |
| SetPosition P1 mutation | `84/84 PASS` | Source/static historical | current Control/TCP 계약 포함 |
| SetOperationMode static | `57 PASS` | Source/static | final qualified source semantics |
| SetOperationMode method split | PASS | Source/static | 세 processor 모두 < 32 KiB |
| SetOperationMode no-replay/D5 deny | PASS | Source/static | recovery 0x6060 write 0, generic D5 0x6060 deny |
| Full SourceOnly | **STOP** | Source/artifact | source gate 이후 `Classes.lcb` physical identity mismatch |

focused gate PASS를 full SourceOnly PASS, C78 PASS 또는 hardware PASS로 대신하지 않는다.

### 6.2 IDE build와 artifact — historical image only

아래 값은 2026-08-20 historical image의 증거이며 **현재 restored SetOperationMode final source의
fresh build 결과가 아니다.**

| 항목 | historical 결과 |
|---|---|
| Target | C78 / ARM |
| Compiler | `0 errors / 79 warnings` |
| Linker | `Done` |
| `Class/Classes.lcb` | 8,610,206 bytes / SHA-256 `568FE55148D734BE4DB0BB5ED9AF4D7800DB33672A5FCE21ECCFE15EE3CAC5A7` |
| project `.lcb` | 634,865 bytes / SHA-256 `FE640A0683466FC1C68537A1CF5E9B96EEFBBBC5EE4885A78F25AF2557193A0E` |
| UDP identity | **FAIL**: verifier pin은 `33C1C2A6...B649A8`; artifact는 `568FE551...C5A7` |

새 source를 LASAL IDE에서 Rebuild/Link하기 전에는 위 hash를 current artifact identity로
승격하거나 verifier ratchet을 갱신하지 않는다.

### 6.3 PLC link/download/load — historical image only

2026-08-20 image에서 다음은 확인됐다.

| 시각 | 확인 로그 | 판정 |
|---|---|---|
| 13:06:22 | `Linking at the PLC successful` | PLC link PASS |
| 13:06:23 | `Download Ok`, `SystemInit: OK` | download/startup PASS |
| 13:06:30 | `Project successfully loaded` | project load PASS |
| 13:06:41 | IDE 종료 후 offline/disconnect | 사용자 종료 동작; PLC fault 증거 아님 |

이 결과를 current restored SetOperationMode runtime의 compile/download/hardware PASS로 확대하지 않는다.

### 6.4 아직 없는 증거

- latest `dev` source의 fresh C78/ARM Rebuild/Link와 generated `Classes.lcb`
- fresh artifact identity에 대한 source/generated ABI 검토와 ratchet 승인
- same image의 fresh `BootId`, `MapRevision`, Diagnostics build tuple
- SetOperationMode same-mode no-write와 exact one-write/readback packet
- 축 1~4 timeout/disconnect/mismatch/quarantine/retire hardware matrix
- SetPosition `0x7D12` native execution/coordinate effect
- latest image 전체 Axis/Group power/stop/fault/reconnect/soak matrix

## 7. Release blockers와 우선순위

### P0-API - 우선순위 상, 75% 미만 4개

| 순서 | API | 진행도 | 다음 gate |
|---:|---|---:|---|
| 1 | [HomeDS402](design/HOME_DS402_DESIGN.md) | 50% | 기존 method 37 lifecycle의 atomic activation과 축 1~4 실기 적격화 |
| 2 | [SetOpMode](design/SET_OPERATION_MODE_DESIGN.md) | 60% | MODE-13 PC/WPF PASS; fresh C78/ARM Rebuild/Link와 artifact review 후 MODE-11/12 hardware qualification |
| 3 | [HomeDS402Ex](design/HOME_DS402_EX_DESIGN.md) | 0% | axis profile/scale 승인 후 `0x7D1B/1C/1D` dormant lifecycle |
| 4 | [SetPosition](design/SET_POSITION_DESIGN.md) | 25% | `_FileSys` dual-file backend, RT claim/native와 terminal durability |

capability와 native mutation activation은 각 설계의 hardware gate 전까지 OFF다.

### P0 - current baseline / SetOperationMode qualification

1. current `dev` source/static verifier와 MODE-13 PC/WPF evidence를 보존한다.
2. 최신 source를 LASAL IDE에서 fresh C78/ARM Rebuild/Link한다.
3. 생성된 `Classes.lcb`를 source/generated ABI와 대조하고 의미 변화가 없다는 검토 뒤에만
   physical identity ratchet 승인 여부를 결정한다.
4. 같은 image의 `BootId`, `MapRevision`, Diagnostics build와 artifact tuple을 기록한다.
5. 축 1부터 MODE-11/12 hardware/packet matrix를 수행한 뒤 축 2~4로 확대한다.
6. 위 증거 전에는 `LMC_DIAG_SET_OPERATION_MODE_ENABLED`와 capability bits 8/9/10을 켜지 않는다.

### P1 - SetOperationMode MODE-13 — PC/WPF PASS

- Start 전 exact durable identity 기록: PASS
- reconnect/startup recovery의 `0x7D24`/read-only no-replay: PASS
- original `0x7D23` 자동 replay 금지: PASS
- terminal generation 저장 후 exact `0x7D25` retire: PASS
- definitive rejection durable evidence/archive와 identity mismatch fail-closed: PASS
- Windows Debug/Release focused smoke `12/12`: PASS

MODE-13 PASS는 PLC runtime/hardware PASS가 아니며 capability activation 근거로 사용하지 않는다.

### P2 - SetPosition durable backend와 executor

1. stock `RAMex UseFile=1`은 request별 completion/physical readback 부재로 production에서 제외한다.
2. `_FileSys` 기반 2 x 2,048-byte fixed A/B file, request별 completion, committed write,
   reopen/readback, CRC, marker-last와 exact tombstone을 구현·검증한다.
3. axis task/core/priority를 확인하고 claim-before-native exactly-once executor를 구현한다.
4. 3-sample terminal proof, terminal-before-response/release, crash/quarantine와 WPF journal recovery를 완성한다.

### P3 - 미완료 API와 배포

- `0x7E23`과 나머지 generic parameter/override API 범위를 결정한다.
- D4, dynamic node/DI, PI Write와 SDO Write의 capability와 실기 적격성을 순차 승인한다.
- 새 `API_MANUAL.md`를 입력으로 generator를 재검증하고 2.4 release candidate 승격 시
  semantic policy와 배포 artifact를 함께 갱신한다.

## 8. System of record와 문서 관리

| 사실 | 정본 |
|---|---|
| Active development branch | `dev`; `codex/*`는 임시 구현/시험 branch |
| Public API signature/behavior | `LMC_API_Delivery/src/**/*.cs` + [API 설명서](API_MANUAL.md) |
| LASAL route/gate/runtime source | `TCPMotionInterface.st`, `LMCControlCommandService.st`, `LMCDiagnosticsService.st` |
| Wire offset/frame | [DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt) |
| Current 구현률/시험/artifact/next step | 이 문서 |
| SetOperationMode source contract | [SetOperationMode 설계](design/SET_OPERATION_MODE_DESIGN.md) |
| SetOperationMode method split/static evidence | [MODE-10 설계](design/SET_OPERATION_MODE_MODE10_METHOD_SPLIT_DESIGN.md) |
| SetOperationMode WPF recovery evidence | [MODE-13 evidence](design/evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md) |
| 설계 이유 | `docs/architecture/**` |
| 원시 작업 이력 | `docs/history/**` |

이 문서만 current 진척도를 가진다. branch cleanup은 current 상태 기록과 분리한다. 개발 중에는
`codex/*`의 unique diff를 보존하고, 개발 완료 시 각 branch가 `dev`에 포함됐는지 또는 폐기
가능한지 확인한 뒤 일괄 정리한다.

## 9. 직접 근거

- [API command ID](../../LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs)
- [Control service](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)
- [TCP interface](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)
- [Diagnostics service](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st)
- [SetOperationMode static verifier](../../tools/Verify-SetOperationModeStatic.ps1)
- [SetOperationMode static workflow](../../.github/workflows/set-operation-mode-static-qualification.yml)
- [SetOperationMode WPF recovery workflow](../../.github/workflows/set-operation-mode-wpf-recovery.yml)
- [SetOperationMode MODE-13 PC/WPF evidence](design/evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md)
- [SetPosition public API](../../LMC_Library/LMC_API_Delivery/src/LmcAxisSetPosition.cs)
- [SetPosition volatile global](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/global_LMCSetPositionStore.st)

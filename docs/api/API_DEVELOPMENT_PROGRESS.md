# LASAL Motion Control API 개발 진척도

- 문서 버전: 1.0-current
- 기준일: 2026-08-20
- API: `LasalMotionControlLib 0.9.1-preview`
- 기준 branch/HEAD: `main@5c80afe94685` + 현재 working tree
- 릴리스 판정: **production NO-GO**

이 문서는 API 구현률, 최신 검증 결과, artifact identity, 제한과 다음 작업의 단일 current
정본이다. API 사용법은 [API 설명서](API_MANUAL.md), byte offset과 frame shape는
[DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)을 따른다.
설계 이유와 과거 실험은 architecture/history 문서에 남기고 이 문서에 override를 누적하지
않는다.

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
- `Dormant`: source와 route 일부가 있어도 capability/gate가 닫혀 정상 호출 금지
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
- SetOperationMode `0x7D23/0x7D24/0x7D25`는 PC/SDK immutable lifecycle, exact frame/parser와
  LASAL dormant failure route를 구현했다. owner/SDO executor는 없으며 capability bits 8/9/10
  preflight가 Start를 wire 전에 차단한다.
- RETAIN 할당 실패를 없애기 위해 SetPosition backing을 1,344-byte ordinary volatile
  `VAR_GLOBAL`로 변경했다. current build/download/project load에서 이전
  `alloc for retain var failed, size=1344`가 재현되지 않았다.
- 최신 C78 build와 PLC link/download/SystemInit/project load는 성공했다. 이 결과를
  SetPosition 실행, 전체 motion 회귀 또는 production 승인으로 확대하지 않는다.
- 마지막 IDE build가 `Classes.lcb`를 다시 생성해 current SHA가 승인 ratchet과 달라졌다.
  따라서 UDP artifact verifier와 이를 포함한 full SourceOnly는 현재 **FAIL/STOP**이다.

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
- High-priority 21개 관점: Active 17, Partial 3(SetPosition, DS402 Home,
  SetOperationMode PC/SDK), Missing 1(`HomeDS402Ex`)

## 4. 기능별 current 상태

| 영역 | Command / API | 상태 | current 경계 |
|---|---|---|---|
| Connection/RPC | `0x8080`, `0x405C`, `0x405D`, `0x103C`, `0x1042`, `0x202B` | Active | bounded fresh-TCP reconnect와 callback registration; callback은 wake hint이며 final state는 TCP readback 필요 |
| Axis core | `0x2022/23/24/28/2E/209F/20A0/20A2` | Active/Limited | accepted-once wait와 no-replay recovery 제공; 전체 축/fault/race 적격성 미완료 |
| Group core | `0x20D2/2045/2047-4B/2051/2085/20A4/20E7/7D22` | Active/Limited | X/Y/Z/U static identity와 bounded profile 계약; 전체 live matrix 미완료 |
| Admin read | `0x7D00/7D10/7D20/7D22` | Active | capability와 allowlisted semantic key를 사용; raw parameter passthrough 아님 |
| LMC Home | `0x7D13/7D18/7D19` | Active/Limited | Admin bit 4 ON; no-motion CurrentPositionZero이며 switch-search Home이 아님 |
| SetPosition | `0x7D12/7D14/7D1A` | Dormant | Store/ownership FALSE, max-jump 0, bits 3/5/7 OFF, volatile backing, native call 0, detail 24 |
| DS402 Home | `0x7D15/7D16/7D17` | Dormant | method 37 source, gate FALSE, Admin bit 6 OFF; `HomeDS402Ex` 실행은 없음 |
| SetOperationMode | `0x7D23/7D24/7D25` | Dormant route | PC/SDK lifecycle과 deterministic failure route 구현; owner/SDO/store 없음, bits 8/9/10 OFF, SDK wire 송신 전 차단 |
| Diagnostics capability | `0x7E00` | Active | 매 connection에서 fresh `BootId`, `MapRevision`, mask를 읽어야 함 |
| D1/D2 | `0x7E01/02/10/20`, `0x7E30-33` | Active/Limited | typed catalog/PI/Bulk 경로; fault/partial/soak 확대 필요 |
| D3 Recorder | `0x7E40-49` | Active/Limited | Single/Ring/Trigger, single recorder owner; capture 적격성 확대 필요 |
| D4 Double | `0x7E4A-4D` | Dormant | capability/proof gate OFF |
| D5 SDO Read | `0x7E50` read | Active/Limited | general inline read; ticket terminal과 exact identity 필요 |
| D5 SDO Write | `0x7E50` write | Limited | Axis1 exact `0x2F00:24 Int32/4`만; Axis2~4와 다른 target 차단 |
| Encoder maintenance | `0x7E53/54/55` | Active/Limited | TW20/TW19 fixed payload만; terminal과 실제 drive effect를 구분 |
| Static topology | `0x7E11/12` | Active | configured inventory; runtime health 증거가 아님 |
| Dynamic node/DI | `0x7E13/22` | Dormant | route/source는 있으나 bits 15/16 OFF |
| Digital output write | `0x7E23` | Missing | C# surface만 있고 LASAL route 없음, bit 17 OFF |
| PI Write | `0x7E21` | Dormant | capability/allowlist OFF |
| Extended SDO result | `0x7E51` | Dormant | bit 12 OFF |
| Distribution | SDK/WPF candidate | Blocked | 새 2.4-development manual과 기존 2.3 semantic policy/DOCX/PDF가 달라 build가 fail-fast |

추가 미구현/제한 항목은 SetOperationMode PLC runtime, generic Axis `SetParameter`, Group parameter write,
Axis override, typed emergency callback producer, profile conditioning pair와 SDO Real64/8-byte다.

## 5. 이번 개발 내용

### 5.1 SetPosition P1 async Control/TCP

- `ProcessAdminSetPositionAsync`를 별도 private method로 분리해 LASAL 32 KiB method budget을
  지켰다.
- `HandleAdminSetPosition`은 async processor로 위임하고 Control lifecycle은 cross-cycle
  context로 유지한다.
- TCP는 `-13` pending, `-14` quarantine close와 기존 `-12` terminal durability uncertainty를
  구분한다.
- duplicated pending tail, exact session/socket/request identity, closed-session notification,
  no-response quarantine와 queue retention 계약을 구현했다.
- RT preflight mailbox는 atomic publication, first coherent sequence, exact tuple revalidation과
  claim/native-zero를 검사한다.
- P1의 ready path는 ownership active까지 진행한 뒤 pending으로 멈춘다. execution/native
  exactly-once 단계는 아직 구현하지 않았다.

### 5.2 SetPosition Store와 RETAIN 결정

- Store ABI와 336-UDINT layout은 유지했다.
- `VAR_GLOBAL RETAIN`과 header의 `RETAIN` qualifier를 제거해 backing을 ordinary volatile
  `VAR_GLOBAL`로 바꿨다.
- 이유는 target PLC에서 special SRAMRETAIN allocation이 0이고 1,344-byte RETAIN 선언이
  runtime link를 실패시켰기 때문이다.
- `autoexec.lsl`의 `SET SRAMRETAIN`은 장비별 배포 설정이 되므로 수정하지 않았다.
- durable query/replay/retirement 계약은 현재 충족하지 않는다. stock `RAMex UseFile=1`은
  request별 write 결과와 physical reopen/readback ABI가 없어 production 후보에서 제외했다.
  다음 경로는 `_FileSys` 기반 fixed A/B file, committed async write, request별 completion,
  reopen/readback, power-cut, CRC/marker-last와 tombstone durability 검증이다.

### 5.3 안전한 비활성 경계

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = FALSE`
- ordinary ownership gate `FALSE`
- 축 1~4 `SetPositionMaxJump = 0`
- Admin feature mask `0x00000017`, bits 3/5/7 OFF
- Admin SetPosition processor 안 native `.SetPosition()` call 0
- exact raw `0x7D12/14/1A` 요청은 detail 24 `StorageUnavailable`

이 값은 결함이 아니라 current image에서 좌표 mutation과 비내구성 replay를 막는 의도된
release fence다. static/build PASS를 이유로 gate를 켜면 안 된다.

## 6. 최신 검증 현황

### 6.1 PC와 source/static

| 검증 | 결과 | 증거 등급 | 제한 |
|---|---:|---|---|
| SDK Debug | `1164/1164 PASS` | PC | SetOperationMode 11개 포함; PLC 증거 아님 |
| SDK Release | `1164/1164 PASS` | PC | 위와 동일 |
| WPF Debug | `356/356 PASS` | PC | fake-RPC/UI smoke |
| WPF Release | `356/356 PASS` | PC | fake-RPC/UI smoke |
| SetPosition P1 mutation | `84/84 PASS` | Source/static | current Control/TCP 계약 포함 |
| Close-without-response | `40/40 PASS` | Source/static | `-12/-13/-14` transport 분리 |
| Store scan | `293/293 PASS` | Source/static | volatile global/header contract 포함 |
| Generated Store wiring | `41/41 PASS` | Source/static | object/network reference |
| Method budget self-test | `16/16 PASS` | Source/static | verifier 자체 시험 |
| LASAL method budget | 6 classes / 108 methods / 105 under / 3 accepted debt | Source/static | 새 P1 method는 모두 32 KiB 미만 |
| UDP callback self-test | `336/336` 마지막 fixture checkpoint | Source/static | current artifact 승인과 별개 |
| UDP current artifact | **FAIL** | Source/artifact | current `Classes.lcb` SHA가 ratchet의 `33C1...49A8`과 다름 |

독립 Debug 전체 실행에서 기존 `GroupDisableWait.Compound.ResumeOwnershipIsReserved`가
한 번 실패해 `1163/1164`가 기록됐고, 같은 tree의 즉시 재실행과 최종 Debug/Release 실행은
모두 `1164/1164 PASS`였다. SetOperationMode 11개는 모든 실행에서 PASS했지만, 이 기존
간헐 실패는 별도 안정화 대상으로 남긴다.

전체 `SourceOnly`는 이 문서 생성 시점의 current tree에서 exit 1로 중단됐다. 첫 blocker는
`SetPosition-augmented Classes.lcb physical identity drifted`다. focused gate PASS를 full
SourceOnly PASS로 대신하지 않는다.

### 6.2 IDE build와 artifact

| 항목 | current 결과 |
|---|---|
| Target | C78 / ARM |
| Compiler | `0 errors / 79 warnings` |
| Linker | `Done` |
| 새 `CInvalidArgException` | 0 |
| `Class/Classes.lcb` | 8,610,206 bytes / SHA-256 `568FE55148D734BE4DB0BB5ED9AF4D7800DB33672A5FCE21ECCFE15EE3CAC5A7` |
| project `.lcb` | 634,865 bytes / SHA-256 `FE640A0683466FC1C68537A1CF5E9B96EEFBBBC5EE4885A78F25AF2557193A0E` |
| UDP identity | **FAIL**: verifier pin은 `33C1C2A6...B649A8`; current artifact는 `568FE551...C5A7` |

79 compiler warnings과 별도의 toolchain/version 경고는 error가 아니지만 release 전에 warning
histogram과 intentional baseline을 다시 검토해야 한다. artifact hash는 source semantics를
대신하지 않으며 source/static gate와 함께 사용한다.

### 6.3 PLC link/download/load

2026-08-20 current image에서 다음 순서를 확인했다.

| 시각 | 확인 로그 | 판정 |
|---|---|---|
| 13:06:22 | `Linking at the PLC successful` | PLC link PASS |
| 13:06:23 | `Download Ok`, `SystemInit: OK` | download/startup PASS |
| 13:06:30 | `Project successfully loaded` | project load PASS |
| 13:06:41 | IDE 종료 후 offline/disconnect | 사용자 종료 동작; PLC fault 증거 아님 |

이 세션에서는 이전 image의 `alloc for retain var failed, size=1344`가 재현되지 않았다.
기존 `salamander-log.txt`는 volatile 변경 전 실패를 담은 역사 자료이며 current PASS 자료가
아니다. startup의 `No SDIAS Client objects projected/connected`는 별도 topology warning으로
남아 있다.

### 6.4 아직 없는 증거

- download image와 함께 기록한 fresh `BootId`, `MapRevision`, Diagnostics build tuple
- 축 1~4의 실제 RT task id/core/priority 동등성
- SetPosition `0x7D12` 실행, native call, coordinate 변화 또는 packet capture
- latest image의 전체 Axis/Group motion, power, stop, fault, reconnect와 soak matrix
- live UDP callback wake + causal TCP terminal packet evidence
- fresh PLC OS/SYSMSG log를 포함한 장시간 무재시작 증거

따라서 현재 PLC가 정상 구동된 관찰은 PLC load/smoke 증거이며 production runtime 또는
SetPosition hardware PASS가 아니다.

## 7. Release blockers와 우선순위

### P0-API - 우선순위 상, 75% 미만 4개

아래 4개를 신규 기능 개발의 최우선 묶음으로 고정한다. 공통 순서와 command ID 예약,
activation 원칙은 [최우선 API 개발 설계](design/README.md)를 따른다.

| 순서 | API | 진행도 | 다음 구현 gate |
|---:|---|---:|---|
| 1 | [HomeDS402](design/HOME_DS402_DESIGN.md) | 50% | 기존 method 37 lifecycle의 5-part atomic activation과 축 1~4 실기 적격화 |
| 2 | [SetOpMode](design/SET_OPERATION_MODE_DESIGN.md) | 25% | PC/SDK contract 완료; CSP mode owner, LASAL lifecycle와 no-replay runtime recovery |
| 3 | [HomeDS402Ex](design/HOME_DS402_EX_DESIGN.md) | 0% | axis profile/scale 승인 후 `0x7D1B/1C/1D` dormant lifecycle |
| 4 | [SetPosition](design/SET_POSITION_DESIGN.md) | 25% | RAMex production NO-GO; `_FileSys` dual-file backend, RT claim/native와 terminal durability |

`HomeDS402`와 `SetOpMode` contract를 먼저 닫고, SetPosition의 durable backend/task proof는
동시에 시작한다. capability와 native mutation은 각 설계의 hardware gate 전까지 OFF다.

### P0 - current baseline 고정

1. current full SourceOnly, focused verifier, method budget와 UDP identity를 같은 tree에서 모두
   재실행한다.
2. `Classes.lcb` current delta를 source/generated ABI와 대조하고, 의미 변화가 없다는 검토 뒤에만
   UDP physical identity ratchet을 `568FE551...C5A7`로 갱신한다. hash만 보고 승인하지 않는다.
3. 목적별 source/verifier/docs/artifact 변경을 분리해 commit하고 clean checkout에서 재현한다.
4. latest image의 `BootId`, `MapRevision`, Diagnostics build와 artifact tuple을 한 evidence로 묶는다.

### P1 - 기존 기능 current image 회귀

1. 축 1~4 EtherCAT/DS402 연결과 정지 상태를 확인한다.
2. Connection/lookup, representative Axis/Group read와 안전한 non-motion command를 재검증한다.
3. callback/reconnect, fault, timeout/cancel과 stable final readback을 packet/runtime 증거로 닫는다.

### P2 - SetPosition durable backend와 executor

1. stock `RAMex UseFile=1`은 request별 completion/physical readback 부재로 production에서 제외한다.
2. `_FileSys` 기반 2 x 2,048-byte fixed A/B file, request별 completion, committed write,
   reopen/readback, CRC, marker-last와 exact tombstone을 구현·검증한다.
3. axis task/core/priority를 확인하고 claim-before-native exactly-once executor를 구현한다.
4. 3-sample terminal proof, retained terminal-before-response/release, crash/quarantine와 WPF journal
   recovery를 완성한다.
5. 위 증거 전에는 Store/ownership/max-jump/capability gate를 변경하지 않는다.

### P3 - 미완료 API와 배포

- `HomeDS402Ex`와 `SetOpMode`는 위 `P0-API`로 승격했다. 이 절에서 다시 범위를 결정하거나
  후순위로 내리지 않는다.
- `0x7E23`과 나머지 generic parameter/override API 범위를 결정한다.
- D4, dynamic node/DI, PI Write와 SDO Write의 capability와 실기 적격성을 순차 승인한다.
- 새 `API_MANUAL.md`를 입력으로 generator를 재검증하고 기존 `2.3-candidate` semantic policy를
  `2.4` release candidate로 승격할 때 별도 update/test한다.
- DOCX/PDF는 새 Markdown과 semantic/render gate가 통과하기 전까지 기존 2.3 artifact로 유지한다.
- `Build-LmcApiDistribution.ps1`의 2.4-development/2.3-candidate mismatch blocker는 위 승격이
  끝난 뒤에만 제거한다.

## 8. System of record와 문서 관리

| 사실 | 정본 |
|---|---|
| Public API signature/behavior | `LMC_API_Delivery/src/**/*.cs` + [API 설명서](API_MANUAL.md) |
| LASAL route/gate/runtime source | `TCPMotionInterface.st`, `LMCControlCommandService.st`, `LMCDiagnosticsService.st` |
| Wire offset/frame | [DINT packet map](../../LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt) |
| Current 구현률/시험/artifact/next step | 이 문서 |
| 설계 이유 | `docs/architecture/**` |
| 원시 작업 이력 | `docs/history/**` |

이 문서만 current 진척도를 가진다. dated plan/progress/backlog, generated HTML/PDF/DOCX와
history는 과거 증거 또는 배포 산출물이며 current 상태를 override하지 않는다. 시험 수치나
artifact identity가 바뀌면 이 문서를 교체하고 다른 문서에 중복 복사하지 않는다.

## 9. 직접 근거

- [API command ID](../../LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs)
- [SetPosition public API](../../LMC_Library/LMC_API_Delivery/src/LmcAxisSetPosition.cs)
- [SetPosition outcome retirement](../../LMC_Library/LMC_API_Delivery/src/LmcAxisSetPositionOutcomeRetirement.cs)
- [Control service](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st)
- [TCP interface](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)
- [Diagnostics service](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st)
- [SetPosition volatile global](../../Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/global_LMCSetPositionStore.st)
- [Current architecture](../architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)
- [SetPosition async design](../architecture/AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md)

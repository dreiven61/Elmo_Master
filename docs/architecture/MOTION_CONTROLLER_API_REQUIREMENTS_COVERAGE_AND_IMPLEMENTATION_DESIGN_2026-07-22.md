# Motion Controller API 요구사항 커버리지 및 구현 설계

- 기준일: 2026-07-22
- 요구사항 원본: `docs/Motion_Controller_API_정리.xlsx`
- 대상 시트/범위: `API 목록!A4:G68`
- 원본 SHA-256: `A4441C88A489EE2898A8A6FA34182E387ABCA03C97B07EA9F68DB73AA29E34A4`
- 판정 대상: 현재 Git working source의 C# 공개 API, TCP wire, LASAL handler/network, WPF 노출
- 갱신일: 2026-07-31
- 상태: Phase 0, Phase 1과 Phase 2 첫 슬라이스 `0x7D22 GroupMoveLinearRelative` 구현 완료. PC 자동 시험/LASAL 정적 계약과 2026-07-23 기존 live capture가 PASS했고, 이후 `0x2047` accepted-then-poll 및 Group/Bulk/Recorder qualification UI는 code/build 완료했다. 신규 runner의 PLC live, fault/stale/reconnect/RT matrix는 별도다.
- 2026-07-27 supplement: current LASAL working source의 CREVIS GL-9086 + Elmo 4개
  configured topology와 별도 topology/node-health/digital-I/O API 설계를 추가했다. 신규 C#
  command contract/parser/test는 구현했지만 PLC/LASAL handler와 capability 광고는 미구현이며,
  이 supplement는 workbook 65개 판정 수를 바꾸지 않는다.
- 2026-07-31 supplement: `0x7D13 StartAxisReference`의 typed SDK/wire와 LASAL
  fail-closed parser를 추가했다. 이 경로는 capability bit 4 OFF, native call 0인 dormant
  계약이며 `HomeDS402`의 LASAL-native 부분 대응으로만 분류한다. IDE/PLC/실축 증거는 없다.
- workbook 추출: OOXML read-only. 숨김 행과 수식은 없었고 실제 값은 A1:G68에 있었다. 현재 세션에는 workbook renderer가 없어 색/조건부 서식에 의미를 둔 판정은 하지 않았다.

## 1. 결론

워크북의 실제 요구사항은 65개다. 우선순위는 상 21개, 중 25개, 하 19개다.

현재 구현을 요구사항의 목적 기준으로 분류하면 다음과 같다.

| 구분 | 개수 | 의미 |
|---|---:|---|
| 직접 구현 | 16 | 동일 기능의 C# 공개 경로와 LASAL 실행 경로가 있다. 원래 OPUS/OPERA 시그니처나 wire와 완전 동일하다는 뜻은 아니다. |
| LASAL 적응 구현 | 24 | 별도 API를 통합했거나 LASAL-native diagnostics/workflow로 같은 목적을 달성한다. |
| 부분 구현/비활성 scaffold | 12 | 일부 범위만 실행되거나 C# 계약만 있고 PLC capability 또는 정책이 꺼져 있다. |
| 실제 미구현 | 9 | 공개 API 또는 PLC handler가 없고 다른 활성 경로도 요구 목적을 충족하지 못한다. |
| 흡수/비동등 보류 | 4 | 다른 요구사항에 흡수하거나 1:1 복제하면 잘못된 의미가 되는 항목이다. |
| 합계 | **65** | |

우선순위별 판정은 다음과 같다.

| 우선순위 | 직접 | LASAL 적응 | 부분/비활성 | 미구현 | 흡수/보류 | 합계 |
|---|---:|---:|---:|---:|---:|---:|
| 상 | 11 | 6 | 2 | 2 | 0 | 21 |
| 중 | 4 | 10 | 7 | 1 | 3 | 25 |
| 하 | 1 | 8 | 3 | 6 | 1 | 19 |
| 합계 | **16** | **24** | **12** | **9** | **4** | **65** |

핵심 판단은 아래와 같다.

1. 우선순위 상 21개 중 기능 경로가 있는 항목은 17개다. `GroupStop`의 `StopMove()` 반환은 오류가 아니라 `StopCmdNo`이므로 기존 ACK 처리는 결함이 아니었다.
2. 우선순위 상의 production-active 공백은 `HomeDS402`, `HomeDS402Ex`, `SetOpMode`,
   `SetPosition` 4개다. `HomeDS402` 요구 목적에는 LASAL-native `ReferenceAxis`의 SDK/wire와
   fail-closed PLC parser를 부분 대응으로 추가했고, `SetPosition`도 같은 dormant 단계다.
   두 기능 모두 capability와 native mutation이 꺼져 있으므로 활성 기능 경로로 계산하지 않는다.
3. SDO Read 1/2/4 byte 경로는 source-active이며 live packet PASS다. `12` capture는
   general-inline UInt32/4 성공과 동일 BootId의 의도한 TypeMismatch 실패 후 Int8/1
   복구를 확인했다. offline/abort, timeout, queued cancel, disconnect/orphan과 contention은 별도다.
4. PI Write, SDO Write, 8-byte SDO는 UI가 덜 만들어진 문제가 아니다. 현재 C# 계약만 있고 SDK/PLC policy와 capability가 의도적으로 fail-closed다.
5. Recorder는 Maestro recorder wire의 복제가 아니라 LASAL D3/D4 recorder의 기능 동등 구현이다.
6. `GroupEnable/Disable`은 실제로 LASAL Profile Lock/Unlock이며, `SetKinTransform`은 고정 X/Y/Z/U identity 검증이다. PMAS의 일반 의미와 같다고 문서화하면 안 된다.
7. PC 자동 테스트와 정적 계약 PASS는 최신 PLC download 및 실물 모션 PASS를 의미하지 않는다.
8. Phase 1에서 read-only Admin 3개 command, typed drive read, PI/Bulk facade와 PC-local error catalog를 source 구현했다. 이로써 No.6/18/34는 LASAL 적응 구현으로 이동했고 No.54는 제한적 부분 구현으로 이동했다.
9. 새 Admin `0x7D00/10/20`은 2026-07-23 live PLC happy path를 통과했다. physical axis 1..4, fixed group `0x0100`과 wire response를 확인했으며 invalid/stale/fault 경로는 아직 별도다.
10. Phase 2 첫 슬라이스 No.41 `Group MoveLinearRelativeEx`는 `0x7D22`와 LASAL `MoveRelativeCoord`로 구현됐고 live PLC에서 Aborting/Buffered 수락과 X/Y/Z/U 축별 왕복을 확인했다. PC가 현재 위치를 합산하지 않으며 PLC profile queue가 상대 이동을 원자적으로 해석한다.
11. 후속 qualification source는 `0x2047` acceptance/poll, true Buffered A/B,
    deterministic stop-first, exact 24-entry Bulk snapshot/lifecycle와 Recorder
    Single Manual/Ring forced-trigger/trigger soak를 구현하고 WPF Debug/Release build를
    통과했다. 이는 live PLC 합격이 아니며 Recorder reconnect/adopt, slave offline,
    raw negative, RT evidence와 D4 Double은 포함하지 않는다.
12. current configured source는 physical `GL-9086 SlaveIndex 0`, `Elmo SlaveIndex 1..4`다.
    기존 `0x7E10`은 4-drive subset과 legacy drive index 0..3을 그대로 유지한다. actual
    topology와 CREVIS I/O는 `0x7E11/12/13/22/23`으로 분리한다. C# SDK contract/parser/test는
    구현했지만 capability는 PLC implementation/runtime 전까지 off다. 이 기능은 workbook
    항목의 Elmo ABI 복제가 아니라 LASAL-local extension이다.

## 2. 판정 기준과 증거 경계

### 2.1 상태 코드

| 코드 | 이름 | 판정 규칙 |
|---|---|---|
| `D` | 직접 구현 | C# 공개 API와 PLC 실행 경로가 현재 source에 존재한다. |
| `E` | LASAL 적응 구현 | 이름/호출 순서는 다르지만 현재 활성 경로가 요구 목적을 달성한다. |
| `P` | 부분/비활성 | 기능 범위가 좁거나 runtime capability/policy가 꺼져 있다. |
| `G` | 미구현 | 현재 실행 경로가 없다. |
| `X` | 흡수/비동등 보류 | 별도 1:1 구현 대신 다른 기능에 흡수하거나 RT 요구를 다시 확정해야 한다. |

### 2.2 확인한 source

- C# connection/axis/group:
  - `LMC_Library/LMC_API_Delivery/src/LmcConnection.cs`
  - `LMC_Library/LMC_API_Delivery/src/LmcAxis.cs`
  - `LMC_Library/LMC_API_Delivery/src/LmcGroup.cs`
  - `LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs`
- C# diagnostics:
  - `LmcDiagnosticsD1*.cs`, `LmcDiagnosticsD2*.cs`, `LmcDiagnosticsD5*.cs`
  - `LmcDiagnosticsRecorder.cs`, `LmcRecorderDownload.cs`
  - `LmcDiagnosticsPIBulkFacade*.cs`, `LmcAxisDriveReads.cs`, `LmcDriveModels.cs`
- C# read-only admin/error catalog:
  - `LmcAdmin.cs`, `LmcAdminModels.cs`, `LmcAdminProtocol.cs`
  - `LmcErrorCatalog.cs`
- LASAL runtime:
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCAxis/_LMCAxis.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_LMCRobotBase/_LMCRobotBase.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/**/*.lcn`
- CREVIS/topology configured source와 설계:
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Eni.xml`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/GL_9086_1/GL_9086_1.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/GL_9086_1_Slot00/GL_9086_1_Slot00.st`
  - `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/GL_9086_1_Slot01/GL_9086_1_Slot01.st`
  - `docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md`
- WPF:
  - `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow*.cs`
  - `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml`

### 2.3 2026-07-23 정적 검증

| 검증 | 결과 | 한계 |
|---|---|---|
| C# PC tests Debug | 148/148 PASS | Phase 1 회귀와 `0x7D22` golden/validation/parser/fake-RPC/capability/generation/safety-owner 13개 포함 |
| C# PC tests Release | 148/148 PASS | 위와 동일 |
| LASAL contract SourceOnly | PASS | source 문자열/offset/호출 계약 정적 검사 |
| LASAL contract network 포함 | PASS | 4축 network와 generated table 정적 검사 |
| WPF Debug build/visual startup | PASS | qualification UI/partial source 포함; Group/Bulk/Recorder panel 렌더와 prerequisite 미충족 초기 실행 버튼 disabled 확인. PLC live는 별도 |
| WPF Release build | PASS | qualification UI/partial source 포함 |
| qualification source | build PASS | Group/Bulk/Recorder runner와 cleanup/logging; Recorder 자동 Release는 Ready/Uploading만 허용하고 Fault는 resource를 보존. runtime pcap/QTEST 없음 |
| SDO Read | 범위별 live PASS | `SDO_Test2`/`SDO_Test_Slave123`는 legacy 4-byte, `10_DriveRead_Axis1to4`는 general-inline 1/2-byte, `12_SDO_GeneralInline_4Byte_FailureRecovery`는 UInt32/4와 동일 BootId TypeMismatch recovery PASS |
| Admin `0x7D00/10/20` | live capture PASS | happy path만 확인; invalid axis/key/selection과 stale session은 별도 |
| Group relative `0x7D22` | 기존 live capture PASS | Aborting/Buffered 수락과 X/Y/Z/U mapping; true chaining runner code/build 완료, live는 별도 |
| Drive read axis 1..4 | live capture PASS | `0x2028`, `0x6041`, `0x6061` 순차 read; atomic snapshot 아님 |
| PI/Bulk 4 signals | live capture PASS | 기존 happy path PASS; 24-entry snapshot/lifecycle soak code/build 완료, live/fault는 별도 |
| Recorder qualification | code/build/Debug visual smoke PASS | Single/Ring/trigger soak와 hash/state-gated cleanup 구현; PLC live, reconnect/adopt, RT와 Double은 별도 |
| Group dynamic timeout | live behavior PASS | `04b`는 55.034 s 계산 한도로 20.152 s stable completion; TXT가 0 byte라 exact UI text는 미증명 |
| PowerOff verification | live function PASS | `08c`에서 Disable -> PowerOff -> final `PowerOn=False`; button label/enable visual state는 미증명 |
| Group position None/ACS | live static-alias PASS | `09b` coordinate 0/1의 exact 68-byte response가 byte-identical; true ACS transform나 MCS/PCS 증거는 아님 |

2026-07-23 raw frame, request field와 결과 값은
[SIGMATEK Phase 1/2 live capture 분석](SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md)에 고정했다.

## 3. 65개 요구사항 전체 판정

> Excel 행 번호는 `No. + 3`이다. 아래 판정은 기능 목적 기준이며, PMAS/MMCLib의 ABI나 packet parity를 의미하지 않는다.

| No. | 우선 | 요구 기능 | 상태 | 현재 대응과 남은 차이 |
|---:|:---:|---|:---:|---|
| 1 | 상 | RPC 연결/콜백 등록 | D | `RpcInitConnection[Async]`; TCP session init `0x8080`, callback endpoint 등록 `0x405C`를 통합 수행한다. PLC UDP event payload producer는 아직 없다. |
| 2 | 상 | UDP channel open | E | 별도 공개 API가 아니라 No.1 내부 callback listener/registration으로 흡수했다. typed event 송신은 미구현이다. |
| 3 | 상 | 연결 종료 | D | `CloseConnection[Async]`, PLC `0x405D`, session/epoch 정리를 수행한다. |
| 4 | 상 | 축 이름 lookup | E | `LMCSingleAxis.CreateAsync`가 `0x103C` lookup과 AxisInfo를 결합한다. |
| 5 | 상 | 그룹 이름 lookup | E | `LMCGroupAxis.CreateAsync`가 `0x1042` lookup을 수행한다. |
| 6 | 하 | Error description/resolution | E | `LMCErrorCatalog.TryDescribe`가 AdapterCommand, AdminDetail, DiagnosticsDetail, GroupProfile domain별 versioned description/resolution을 제공한다. Elmo Personality 전체 DB는 아니다. |
| 7 | 중 | PI metadata by alias | P | D1 `GetSignalCatalog`, alias lookup이 있지만 현재 24-entry 고정 catalog이며 arbitrary Maestro PI metadata와 동일하지 않다. |
| 8 | 중 | Bulk GetEntry | E | `LMCPIBulkReader.GetEntry/TryGetEntry`가 최신 Upload snapshot을 alias 또는 SignalId로 조회한다. |
| 9 | 중 | Bulk AddEntry | E | `CreatePIBulkBuilder(catalog)`의 `AddEntry`/`Configure`가 local builder 흐름을 제공하고 wire는 D2를 재사용한다. |
| 10 | 중 | PI Read | E | D1 `ReadPI`, PLC `0x7E20`; signal ID/type/map revision 경로에 catalog alias overload를 추가했다. |
| 11 | 중 | PI Write | P | `SubmitPIWrite` ticket/wire scaffold는 있으나 compile-time allowlist와 PLC capability가 OFF다. 실제 쓰기는 불가하다. |
| 12 | 중 | Bulk Upload | E | D2 `ReadBulk`/`0x7E32` snapshot이 같은 목적을 수행한다. |
| 13 | 중 | Bulk Perform | E | D2 `ReadBulk`가 typed snapshot/exception 형태로 수행한다. |
| 14 | 중 | Bulk Init | E | `ConfigureBulk`가 로컬 검증과 원격 config를 한 transaction으로 통합한다. |
| 15 | 중 | Bulk Config | E | D2 `ConfigureBulk`, PLC `0x7E30`이 활성 경로다. |
| 16 | 하 | Emergency typed callback | G | UDP listener와 raw callback log만 있다. PLC event producer와 typed schema가 없다. |
| 17 | 하 | Group member info | D | `GetGroupMembersInfo`, PLC `0x20D2`; name/ref/device/count를 제공한다. software descriptor는 1..9, 실제 EtherCAT 축은 1..4다. |
| 18 | 하 | Group parameter batch read | E | `connection.Admin.ReadGroupParameters[Async]`, local `0x7D20`; Group `0x0100`의 velocity/acceleration/jerk-time 선택 mask만 허용하며 2026-07-23 live happy path가 PASS했다. |
| 19 | 상 | HomeDS402 | P | `0x7D13 ReferenceAxis` typed SDK/wire와 fail-closed PLC parser로 LASAL-native reference 목적을 부분 대응한다. capability bit 4는 OFF이고 native `_LMCAxis.MoveReference` 호출은 0회다. Elmo Gold DS402 method `1..14`, `17..30`, `33..35`와 동등하지 않다. |
| 20 | 중 | Axis ReadStatus | D | `ReadStatus[Result][Async]`, PLC `0x2028`. |
| 21 | 중 | GroupReadStatus | D | `GroupReadStatus[Result][Async]`, PLC `0x2045`. |
| 22 | 상 | HomeDS402Ex | G | 확장 homing model/handler가 없다. LASAL-native reference adapter로 별도 설계해야 한다. |
| 23 | 하 | Axis SetParameter | G | 공개 API/handler가 없다. `_LMCAxis.SetParameter`는 이미 존재하므로 semantic allowlist adapter를 추가할 수 있다. |
| 24 | 상 | MoveAbsoluteEx | D | `MoveAbsoluteEx[Async]`, PLC `0x209F`. raw application DINT와 제한된 direction/buffer 의미를 사용한다. |
| 25 | 중 | MoveLinearAbsolute non-Ex | X | workbook도 OPERA 미사용으로 표시한다. No.26/27 Ex 경로에 흡수한다. |
| 26 | 중 | MoveLinearAbsoluteEx full | P | C# options는 넓지만 PLC는 고정 X/Y/Z/U, 제한된 coord/transition/buffer만 처리한다. transition parameter array와 superimposed는 없다. |
| 27 | 중 | MoveLinearAbsoluteEx simple | D | 단순 overload가 있다. `double[]`이 아니라 raw DINT `int[]`를 사용한다. |
| 28 | 중 | MoveLinearRelative non-Ex | X | workbook도 OPERA 미사용으로 표시한다. 실제 요구는 No.41 relative Ex 하나로 통합한다. |
| 29 | 상 | Axis Reset | D | `Reset[Async]`, PLC `0x2024`. SDK/WPF accepted-once Begin/Resume과 durable Accepted restart는 `0x2024`를 replay하지 않고 `0x2028`의 `AxisErrorId==0`을 3회 확인한다. |
| 30 | 상 | Group Reset | D | `GroupReset[Async]`, PLC `0x2049`. |
| 31 | 상 | Axis PowerOff | D | `PowerOff[Async]`, PLC `0x2023 enable=0`. |
| 32 | 상 | Group Disable | E | `GroupDisable[Async]`, PLC `0x2048`; 실제 의미는 LASAL `UnlockProfile`이다. same-cycle `LockState` read를 제거해 ACK를 native acceptance로 한정했고, Begin/Resume/cross-session status-only API와 방향성 durable journal이 후속 `0x2045` 3회로 완료를 증명한다. 새 IDE/download/live 재캡처는 대기다. |
| 33 | 상 | SetOpMode | G | dedicated API가 없다. 0x6060 PDO는 Elmo 객체에서 disabled이고 unrestricted SDO Write도 OFF다. LMC mode ownership 결정이 먼저다. |
| 34 | 하 | GetOpMode | E | `GetDriveOperationMode[Async]`가 D5 SDO `0x6061:0 Int8/1` ticket을 bounded poll하고 raw signed value와 typed mode를 반환한다. 축 1..4 live 결과는 모두 8(CSP)로 PASS했다. |
| 35 | 상 | Axis PowerOn | D | `PowerOn[Async]`, PLC `0x2023 enable=1`. |
| 36 | 상 | Group Enable | E | `GroupEnable[Async]`, PLC `0x2047`; 실제 의미는 LASAL `LockProfile`이다. same-cycle `LockState` read를 제거하고 acceptance ACK + 후속 `0x2045` poll로 source/static 수정했으며 새 IDE/download/live 재캡처는 대기다. |
| 37 | 하 | Profile Conditioning clear/enable | G | 검증된 SIGMATEK/LASAL 대응 기능이 없다. |
| 38 | 하 | Profile Conditioning parameters | G | 검증된 SIGMATEK/LASAL 대응 기능이 없다. |
| 39 | 상 | Axis actual position | E | `GetActualPosition[Result][Async]`, PLC `0x202E`; engineering double이 아니라 caller-scaled raw DINT다. |
| 40 | 상 | MoveRelativeEx | D | `MoveRelativeEx[Async]`, PLC `0x20A0`; raw DINT와 제한된 buffer 의미를 사용한다. |
| 41 | 중 | Group MoveLinearRelativeEx | D | `LMCGroupAxis.MoveLinearRelativeEx[Async]`, Admin `0x7D22`, LASAL `MoveRelativeCoord` 경로가 있다. 기존 Aborting/Buffered 수락과 축별 왕복은 live PASS했고 true Buffered A/B runner는 code/build 완료, live queue/endpoint는 대기다. |
| 42 | 상 | Axis Stop | D | `Stop[Async]`, PLC `0x2022`; decel/jerk를 지원하나 buffer mode는 미노출이다. SDK/WPF accepted-once 완료는 `0x2028` Standstill 3회이며, durable Reset takeover는 pinned old transport abort와 fresh identity 확인 뒤 Stop을 정확히 1회 보낸다. |
| 43 | 상 | Group Stop | D | `GroupStop[Async]`, PLC `0x2085`. ACK는 검증된 `StopMove(Mode:=3)` dispatch를 뜻한다. `GroupStopAndWaitForStableStandbyAsync`는 Stop 1회 뒤 `GroupReadStatus(0x2045)`만 poll해 stable standby를 확인하고 failure evidence를 보존한다. PC 계약은 구현됐고 PLC 실기 완료 poll은 별도다. |
| 44 | 상 | MoveVelocityEx | D | `MoveVelocityEx[Async]`, PLC `0x20A2`; 실제 LASAL 호출은 `MoveEndless`, deceleration=0 제약이 있다. |
| 45 | 하 | Axis override | G | 공개 API/handler가 없다. LASAL에는 1000=100%인 Override server가 있으나 Maestro의 vel/acc/jerk 3-factor와 같지 않다. |
| 46 | 중 | SDO Read Int32, explicit length | E | `ReadSdoInline[Async]`가 D5 capability preflight, submit, bounded terminal poll을 결합하고 `LMCSdoReadResult`에 request/ticket/status/exact bytes/typed value를 보존한다. 1/2/4-byte만 허용하며 사용자 backend 실기 확인 PASS, 새 facade 자체 live 재확인은 대기다. |
| 47 | 중 | SDO Read Double/8-byte | G | `Real64` type과 8-byte active result path가 없다. MaxSDO=4, extended result capability OFF다. |
| 48 | 중 | SDO Read Float/4-byte | E | `ReadSdoInline[Async]`의 `Real32/4` 결과가 canonical little-endian bytes와 `Single` typed value를 반환한다. |
| 49 | 중 | SDO Read Int32/4-byte | E | `ReadSdoInline[Async]`의 `Int32/4` 결과를 사용한다. No.46과 기능상 중복이다. |
| 50 | 중 | SDO Write Int64 | P | request model만 있고 PLC SDO Write capability/allowlist가 OFF다. 8-byte type도 없다. |
| 51 | 중 | SDO Write Double | P | request model만 있고 PLC SDO Write capability/allowlist가 OFF다. `Real64`도 없다. |
| 52 | 중 | SDO Write Int32 | P | `CreateWrite` scaffold는 있으나 SDK/PLC 이중 정책으로 항상 차단된다. |
| 53 | 하 | Group parameter batch write | G | 공개 API/handler가 없다. LASAL native parameter method를 raw passthrough하지 말고 semantic key로 제한해야 한다. |
| 54 | 하 | Axis GetParameter | P | `connection.Admin.ReadAxisParameter[Async]`, local `0x7D10`; physical axis 1..4와 6개 semantic Int32 key만 허용하는 제한 구현이며 이 범위의 live happy path가 PASS했다. |
| 55 | 하 | Group actual position | P | `GroupReadActualPosition`, PLC `0x2051`; `09b`에서 None/ACS static member-slot alias의 byte-identical 68-byte 응답을 live 확인했다. true ACS transform는 아니며 MCS/PCS 변환은 지원하지 않는다. |
| 56 | 하 | StatusRegister/MCS limit | P | `ReadDriveStatus[Async]`가 axis status, DS402 `0x6041`, `0x6061`을 source별로 분리하고 limit indication을 제공한다. 축 1..4 live 순차 read는 PASS했지만 Maestro StatusRegister/MCS limit와 동일한 atomic register는 아니다. |
| 57 | 중 | ReadBoolParameter | X | workbook상 OPERA 미사용이며 목적은 current mode 확인이다. raw API를 복제하지 않고 No.34 typed mode read로 흡수한다. |
| 58 | 상 | SetPosition | P | `0x7D12` typed SDK/wire와 fail-closed PLC parser를 구현했다. capability bit 3은 OFF이고 native `_LMCAxis.SetPosition` 호출은 0회다. 별도 max-jump 설정, task/core/priority, state/ownership과 PLC proof 전에는 활성 기능이 아니다. |
| 59 | 중 | SetKinTransform | P | `SetKinTransformCartesian4Axis`, PLC `0x20E7`; fixed X/Y/Z/U identity ready 검증이지 generic kinematics가 아니다. |
| 60 | 하 | StopRecording | E | D3/D4 `StopRecorder`, PLC `0x7E43`; qualification cleanup은 final Status를 확인해 `Ready`/`Uploading`에서만 자동 Release하고 `Fault`는 resource를 보존한다. 신규 PLC live는 대기다. |
| 61 | 하 | Recording status | E | `GetRecorderStatus`, PLC `0x7E44`; bounded qualification poll code/build 완료, live는 대기다. |
| 62 | 하 | BeginRecording | E | `ConfigureRecorder` + `StartRecorder`, PLC `0x7E40/41`; Single/Ring/trigger-soak runner code/build 완료, live는 대기다. |
| 63 | 하 | UploadDataHeader | E | `GetRecorderHeader`, PLC `0x7E45`; metadata assertion runner code/build 완료, live는 대기다. |
| 64 | 하 | UploadData | E | `ReadRecorderChunk`/`DownloadRecorderAsync`, PLC `0x7E46`; exact byte count/double-download SHA-256 assertion code/build 완료, live는 대기다. |
| 65 | 하 | WaitUntilConditionFB | X | PC polling은 여러 축/컨트롤러의 deterministic sync와 동등하지 않다. 실제 장비 동기화 요구가 확정될 때 PLC-owned arm/condition/start로 설계한다. |

## 4. 구현 전에 먼저 수정할 계약 불일치

신규 기능보다 아래 계약 항목을 먼저 바로잡아야 한다. 현재 API가 실제 PLC 지원 범위를 넓게 보이게 하거나 반환값 의미를 잘못 설명할 수 있기 때문이다.

### 4.1 Group motion option truthfulness

현재 C# `LMCGroupMotionOptions`는 다음을 공개한다.

- coordinate: None/ACS/MCS/PCS
- transition: ExactStop, ContinuousDirect, SmoothParabolic/Cubic/Quintic
- buffer: Aborting, Buffered, 네 종류 blending
- execute flag

하지만 current PLC `0x20A4`는 X/Y/Z/U 4축 identity 범위와 일부 조합만 유효하게 처리한다. v1 선택은 다음 중 하나여야 한다.

1. 권장: PC에서 현재 PLC whitelist만 fail-fast하고 unsupported enum을 송신하지 않는다.
2. 후속: PLC 구현을 확장한 뒤 capability/schema version으로 지원 범위를 광고한다.

지원하지 않는 값을 받아 두고 내부에서 다른 값으로 처리하는 방식은 금지한다.

### 4.2 Group actual position 좌표계 계약

`0x2051` request는 coordinate enum을 받지만 current PLC는 `CoordSystem:=0`으로 읽는다. v1 계약을 다음으로 고정한다.

- `None/ACS`: member slot position 반환
- `MCS/PCS`: C# public builder는 `NotSupportedException`, legacy/raw wire 요청은 PLC `ErrorId=-7`
- 반환 slot 수: group member metadata와 일치시키고 남은 slot은 0
- Cartesian position이 필요하면 generic group position과 분리된 새 typed command로 설계

`09b` live capture는 coordinate 0/1 요청과 exact 68-byte 응답 두 건을 확보했다.
`HeaderStatus=0`, `FunctionStatus=0x4000`, `ErrorId=0`이고 두 position vector는
byte-identical하므로 위 static-alias 계약은 runtime PASS다. 이것은 true ACS transform나
MCS/PCS transform/rejection의 runtime 증거가 아니다.

### 4.3 GroupStop command result

초기 설계의 “`LMCRobot.StopMove()` return을 response status/error에 반영” 판단은 폐기한다. vendor declaration의 반환은 `_LMCPROFERRORTYPES`가 아니라 `UDINT StopCmdNo`, 즉 정지가 끝날 profile-buffer command index다. 0/비0 어느 쪽도 성공/실패 코드가 아니므로 ACK에 매핑하면 안 된다.

`0x2085` success ACK의 범위는 request/parameter 검증, `LMCRobot` client 연결 확인, `StopMove(Mode:=3)` 호출 dispatch까지다. 정지 완료와 runtime profile error는 이후 `0x2045 GroupReadStatus`의 `ProfileInPosition`/`ReadProfileError`로 확인한다.

## 5. 구현 아키텍처

### 5.1 기존 wire 재사용 원칙

다음은 새 command를 만들지 않는다.

- GetOpMode: D5 SDO Read `0x6061:0 Int8/1` 위 typed facade
- SDO 1/2/4-byte convenience: 현재 D5 ticket/status 위 typed facade
- PI/Bulk compatibility facade: 현재 D1/D2 위 `AddEntry/Configure/Upload/GetEntry` adapter
- Recorder compatibility facade: 현재 D3/D4 configure/start/header/chunk 위 adapter
- Status view: `ReadStatus`, D1 PI, SDO `0x6041`, EtherCAT health를 조합한 typed composite

### 5.2 LASAL-local Admin Extension

확인되지 않은 PMAS `0x20xx` command ID나 payload를 추측하지 않는다. 새 LASAL-native 관리 기능은 충돌이 없는 local extension command family로 분리한다.

Phase 1에서 read-only 3개 ID를 구현해 `DINT_PACKET_MAP.txt`, golden/parser/fake-RPC와 LASAL 정적 계약에 등록했다. 나머지 ID는 reservation이다.

| Command | 제안 ID | v1 목적 | 상태 |
|---|:---:|---|---|
| GetAdminCapabilities | `0x7D00` | admin schema/version/feature bit/limit 광고 | source 구현 |
| ReadAxisParameter | `0x7D10` | semantic key 기반 axis parameter read | source 구현 |
| WriteAxisParameter | `0x7D11` | 이중 allowlist 기반 제한 write | 후속 |
| SetAxisPosition | `0x7D12` | 제한된 mode/state에서 position set | SDK/wire + fail-closed PLC parser, activation 후속 |
| StartAxisReference | `0x7D13` | LASAL `MoveReference` 기반 reference | SDK/wire + fail-closed PLC parser, activation 후속 |
| SetDriveOperationMode | `0x7D15` | ownership 승인 후 dedicated mode change | 보류 |
| SetAxisOverride | `0x7D16` | v1 velocity override permille | 후속 |
| ReadGroupParameters | `0x7D20` | bounded batch read | source 구현 |
| WriteGroupParameters | `0x7D21` | bounded semantic allowlist write | 후속 |
| GroupMoveLinearRelative | `0x7D22` | LASAL `MoveRelativeCoord` 원자적 실행 | source 구현 |
| ArmWaitCondition | `0x7D30` | PLC-local condition arm/start | 요구 확정 전 보류 |

공통 payload prefix 제안:

| offset | type | field | 규칙 |
|---:|---|---|---|
| `P+0` | U16 | SchemaVersion | v1=`1` |
| `P+2` | U16 | Flags | v1 reserved=`0` |
| `P+4` | U32 | RequestId | nonzero, response echo |

공통 response prefix 제안:

| offset | type | field |
|---:|---|---|
| `P+0` | U16 | SchemaVersion |
| `P+2` | U16 | ResponseFlags |
| `P+4` | U16 | CommandStatus |
| `P+6` | I16 | ErrorId |
| `P+8` | U32 | RequestId echo |
| `P+12` | U32 | DetailCode |

기존 outer 8-byte envelope와 session generation/exchange serialization을 그대로 사용한다. state-changing command는 axis/group별 single owner만 허용하고, TCP disconnect는 자동 motion stop으로 간주하지 않는다.

### 5.3 Semantic parameter key

Maestro enum 번호나 SIGMATEK private enum을 wire에 그대로 노출하지 않는다.

v1 read 후보:

- SoftwareMinPosition
- SoftwareMaxPosition
- EndPositionToleranceWindow
- MaxVelocity
- MaxAcceleration
- ReferencePosition

v1 write 후보는 실제 장비 요구가 확인된 최소 집합만 승인한다. 각 key는 다음 메타데이터를 가져야 한다.

- value type과 unit
- 허용 min/max
- 허용 axis와 state
- LASAL native method/enum mapping
- read-back verification 여부
- audit detail code

SDK와 PLC가 동일한 versioned allowlist를 각각 검사한다.

### 5.4 EtherCAT configured topology와 digital I/O extension

현재 source의 configured physical order는 `GL_9086_11(SlaveIndex 0) ->
Elmo_11..41(SlaveIndex 1..4)`다. public axis는 계속 1..4다. 기존
`0x7E10 ReadEtherCATHealth`의 `SlaveIndex=0..3`은 actual physical index가 아니라
호환용 legacy drive index로 유지한다.

local C# SDK contract는 아래와 같다.

| Command | ID | 목적 | 상태 |
|---|:---:|---|---|
| GetEtherCATTopologyInfo | `0x7E11` | configured topology revision/count/stride | SDK 구현, PLC/capability off |
| GetEtherCATTopologyChunk | `0x7E12` | opaque `NodeId`, actual `MasterSlaveIndex`와 configured identity/schema | SDK 구현, PLC/capability off |
| ReadEtherCATNodeHealth | `0x7E13` | node별 runtime state와 data-valid/defaulted quality | SDK 구현, PLC/capability off |
| ReadDigitalIO | `0x7E22` | `IOReference`별 input 또는 PLC output-shadow word | SDK 구현, PLC/capability off |
| SubmitDigitalOutputWrite | `0x7E23` | RT-owned CAS whole-word/atomic masked write ticket | SDK 구현, PLC/capability off |

configured slave order/identity, slot/PDO index-subindex, I/O width와 process-image mapping은
schema다. Online/EtherCAT state/AL status, digital value와 quality만 동적이다. runtime
discovery로 schema를 바꾸거나 raw process-image bit offset을 public API에 노출하지 않는다.

capability bit 14~17을 각각 topology, node health, digital I/O read, digital output write로
예약하되 현재 `0x7E00`에는 광고하지 않는다. output write는 bit 14~16, nonzero BootId,
exact topology revision, fresh output revision, SDK/PLC allowlist와 single PLC RT owner를 모두
요구한다. whole-word는 exact output valid mask, masked write는 nonzero subset mask를 사용하며
validation/offline/stale/owner 실패에서는 어떤 output bit도 바꾸지 않는다. accepted request는
reconnect 뒤 자동 재생하지 않는다. 현재 SDK allowlist는 empty다.

local Elmo 문서에서 확인한 대응 근거는 `MMC_GetEthercatCommStatistics`,
`MMC_GetCommDiagnosticsEx`, `MMC_ECATIOReadDigitalInput`과
`MMC_ECATIOWriteDigitalOutput`이다. EtherCAT physical topology graph API는 확인하지
못했으므로 신규 topology wire를 Elmo parity로 부르지 않는다. exact schema와 구현 순서는
[LMC EtherCAT Topology 및 Digital I/O API 설계](LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md)를
따른다.

## 6. 단계별 구현 순서

### Phase 0 - 현재 계약 정합

1. C# Group options를 PLC 실제 whitelist로 fail-fast한다.
2. `0x2051` coordinate/slot 계약을 확정하고 C#/PLC/docs/tests를 동시에 수정한다.
3. `0x2085`의 `StopCmdNo`를 오류로 오해하지 않도록 코드/문서/정적 계약을 고정하고, 완료/error 확인을 `0x2045` poll로 명시한다.
4. SDO Read user runtime PASS를 current status 문서에 반영하되 Write/8-byte capability는 계속 OFF로 유지한다.

완료 조건:

- 기존 104개 회귀 테스트와 신규 Phase 0 matrix를 합친 105개가 Debug/Release에서 모두 PASS
- group option invalid matrix golden/fake-RPC test 추가
- GroupStop 입력 오류는 PC/PLC에서 거부되고, dispatch ACK와 runtime 완료/error가 명확히 분리됨
- `0x2051` None/ACS source/fake-RPC와 `09b` live static-alias PASS, MCS/PCS explicit reject는 source/static이며 live negative는 별도

Phase 0 source 결과:

- C# request builder는 group option, position topology와 motion 수치 범위를 RPC 송신 전에 검사한다.
- LASAL `0x2051`은 None/ACS만 member-slot alias로 처리하고 MCS/PCS는 `ErrorId=-7`, 그 밖의 enum은 `-3`으로 거부한다.
- LASAL `0x2085`는 `StopMove()`의 `StopCmdNo`를 ACK error로 사용하지 않는다. 완료와 profile error는 `GroupReadStatus(0x2045)`로 확인한다.
- Debug/Release PC 시험 `105/105`, LASAL SourceOnly/full 정적 계약, WPF Debug/Release 임시 output build를 통과했다.
- 실제 PLC의 None/ACS static alias는 `09b`로 PASS했다. LASAL IDE 최신 build 증거,
  MCS/PCS 거부와 GroupStop 완료 poll은 아직 별도다.

### Phase 1 - read-only admin과 compatibility facade

1. `GetDriveOperationModeAsync`: SDO `0x6061:0 Int8/1` typed wrapper
2. `ReadAxisParameterAsync`와 `ReadGroupParametersAsync`: semantic read allowlist
3. `LMCDriveStatus`: axis state, DS402 statusword, limit/error source를 분리한 composite
4. PI/Bulk compatibility facade: mutable local builder를 제공하되 wire는 D1/D2 재사용
5. `LMCErrorCatalog.TryDescribe`: PC-local versioned data provider

이 단계는 motion을 생성하지 않으므로 가장 먼저 기능 범위를 늘릴 수 있다.

Phase 1 source 결과:

- `connection.Admin`은 `0x7D00` capability를 매 호출에서 확인한 뒤 `0x7D10`/`0x7D20`을 실행한다. physical axis는 1..4, group은 `0x0100`으로 제한한다.
- axis read key는 `SoftwareMinPosition`, `SoftwareMaxPosition`, `EndPositionToleranceWindow`, `MaxVelocity`, `MaxAcceleration`, `ReferencePosition` 6개다. `EndPositionToleranceWindow`는 profile in-position 상태가 아니라 `_LMCAxis`의 end-position tolerance parameter다.
- group read는 `PathVelocityLimit`, `PathAccelerationLimit`, `JerkTime` 선택 mask만 허용하며 각 결과 unit을 schema로 고정한다.
- `GetDriveOperationMode[Async]`와 `ReadDriveStatus[Async]`는 기존 D5 SDO ticket/status wire를 재사용한다. composite는 axis status -> `0x6041` -> `0x6061` 순차 read이므로 같은 cycle의 atomic snapshot이 아니다.
- D5 terminal poll은 광고된 `BaseCycleTimeUs`의 millisecond ceiling 간격과 `TimeoutCycles+32` 상한을 사용한다. 제출 뒤 async 취소는 PLC ticket을 cancel하지 않고 ticket을 포함한 `LMCSdoReadWaitCanceledException`으로 PC wait만 끝낸다. 진행 중인 status RPC는 응답을 drain한 뒤 취소를 관찰해 connection과 ticket 재조회 경로를 보존한다.
- `CreatePIBulkBuilder`는 catalog의 exact `MapRevision`, readable flag, 최대 32개와 중복을 검사한다. `Upload` 후 `GetEntry/TryGetEntry`를 제공하며 wire는 D1/D2 그대로다.
- `LMCErrorCatalog`는 project-local 네 domain(`AdapterCommand`, `AdminDetail`, `DiagnosticsDetail`, `GroupProfile`)을 명시적으로 구분한다. 같은 숫자를 domain 없이 해석하지 않으며 Elmo Personality database를 표방하지 않는다.
- WPF example에 별도 `Read-only API` 탭을 추가했다. `0x7D00` capability가 성공한 뒤에만 axis/group semantic read가 열리고, physical axis 1~4의 typed operation mode와 non-atomic drive status를 같은 흐름에서 확인한다. motion/write control은 포함하지 않는다.
- C# Debug/Release 자동 시험은 각각 `148/148`, LASAL SourceOnly/full 정적 계약은
  PASS다. 2026-07-23 live capture에서 Phase 1 `0x7D00/10/20`, Phase 2 `0x7D22`,
  axis 1~4 drive read와 D1 PI/D2 Bulk happy path를 확인했다. fault/race/soak와
  `0x2047` accepted-then-poll 수정본의 IDE build/download/live 재검증은 남아 있다.

### Phase 2 - LASAL-native motion/admin

#### 6.2.1 Reference/Homing

API 이름은 `ReferenceAxis` 또는 `HomeUsingLasalReference`로 한다. Elmo Gold DS402 method `1..14`, `17..30`, `33..35`를 지원하지 않으면서 `HomeDS402Ex`라고 부르지 않는다.

backend는 `_LMCAxis.MoveReference`이며 v1 조건은 다음과 같다.

- physical axis 1..4만 허용
- axis powered, StandStill, no active error
- group/profile ownership과 충돌하지 않음
- `RefSwitch`, `HWMin`, `HWMax`, `ZImpulse`, `LatchPos`의 실제 장비 연결 확인
- mode, position, VRef1, VRef2, acceleration, window, jerk의 bounded validation
- PLC가 연결 단절과 무관하게 집행하는 positive `MaxTravel`과 `TimeoutMs`
- cancel은 새로운 reference command가 아니라 기존 controlled Stop을 사용
- 완료는 original command provenance가 있는 상태에서 status/reference bit의 stable poll 또는
  typed event로 판단

현재 Motion Network에는 `_LMCAxis` 내부 client 정의는 있으나 physical reference input source가 연결됐다는 증거가 없다. 객체/IO 배치가 확정되기 전에는 capability를 광고하지 않는다.
현재 source는 `0x7D13` request/response와 one-shot SDK 계약, fail-closed PLC parser까지만
고정하고 capability bit 4와 native `_LMCAxis.MoveReference()` 호출을 끈다. exact wire와
activation gate는
[Axis Reference LASAL-native dormant 계약](AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md)을
따른다.

#### 6.2.2 SetPosition

`_LMCAxis.SetPosition`을 사용하되 v1에서는 한 개의 명확한 semantic mode만 허용한다.

- request는 target과 expected actual position을 함께 보내 stale coordinate snapshot을 CAS로 거부한다.
- public semantic mode는 actual position과 destination을 application units로 함께 맞추는 mode 1뿐이다.
- explicit one-shot confirmation과 little-endian ASCII `SETP` token을 사용한다.
- response는 applied position, semantic mode와 full native command-state를 echo한다.
- PC prepared command는 owner/session/axis/capability/target/expected actual에 고정되며 write boundary를
  한 번만 넘는다. post-boundary loss는 outcome uncertain이고 reconnect 뒤 replay하지 않는다.
- PLC activation은 physical/non-modulo/non-simulation, `IsReferenced=1` axis, powered-off standstill, actual/set velocity 0,
  no error/coupling/profile owner, valid software limits와 별도 application-approved max-jump를 요구한다.
- `LMCAXIS_PAR_RD_SWLIMWINDOW`는 software end-position standstill tolerance이므로 max-jump로 쓰지 않는다.
- `_LMCAxis.SetPosition`의 same-core/equal-or-lower-priority 조건은 IDE task map과 PLC에서 별도 확인한다.

raw enum passthrough는 금지한다.

현재 source는 request/response/parser 계약만 고정하고 capability bit 3을 광고하지 않는다.
유효한 raw `0x7D12` 요청도 fail-closed `InvalidState`로 응답하며 native mutation은 0회다. exact wire,
activation gate와 검증 기준은
[Axis Set Position 제한 좌표 보정 설계](AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)를 따른다.

#### 6.2.3 Group relative move

`0x7D22`는 기존 absolute group motion body의 position vector를 distance vector로 해석하고 PLC에서 `MoveRelativeCoord`를 직접 호출한다. PC가 현재 위치를 읽어 absolute target으로 바꾸는 방식은 원자성과 좌표계 의미가 깨지므로 사용하지 않는다.

v1은 4축 identity, supported coordinate, transition 0/2, buffer 1/2의 실제 PLC 범위만 허용한다.

구현된 v1 wire는 Admin 공통 prefix 8바이트 뒤에 16개 DINT distance slot과 velocity, acceleration, deceleration, jerk, coordinate, transition, buffer, execute를 둔 정확히 104바이트 payload다. 현재 4축 topology 때문에 slot 5..16은 반드시 0이고 coordinate는 `None(0)`만 허용한다. 성공 응답은 16바이트 Admin 공통 ACK이며 이동 완료가 아니라 profile queue 수락을 뜻한다. 완료와 profile error는 기존 `GroupReadStatus(0x2045)`로 판정한다.

`0x7D00 FeatureBits`가 `0x00000003`에서 `0x00000007`로 바뀌므로 PLC와 Phase 2 PC DLL은 paired rollout한다. 기존 strict parser는 알 수 없는 bit 2를 거부하므로 PLC만 선행 배포하면 기존 Admin read도 capability 단계에서 차단된다.

LASAL handler는 Robot/Axis1..4 client 연결, 4축 kinematic identity 준비, Robot power와 profile lock을 확인한 뒤에만 `MoveRelativeCoord`를 호출한다. Admin detail 9는 motion body 오류, 10은 준비 상태 오류, 11은 native profile command 거부이며 detail 11의 `ErrorId`는 positive `GroupProfile` 오류 번호 또는 adapter fallback `-6`만 허용한다.

WPF Group Motion 탭은 기존 X/Y/Z/U와 dynamics/options 입력을 재사용하는 `Move Linear Relative` 버튼을 제공한다. 기존 absolute move와 같은 motion-uncertain, Stop, `GroupReadStatus` 완료 모니터 경로를 사용한다.

#### 6.2.4 Override

v1은 `SetVelocityOverridePermille(0..1000)` 하나만 제공한다. LASAL `Override` server의 1000=100% 의미를 사용한다. Maestro의 acceleration/jerk factor는 native 의미가 검증될 때까지 unsupported다.

### Phase 3 - controlled write와 8-byte SDO

1. parameter/group write는 semantic key와 SDK/PLC 이중 allowlist로만 연다.
2. `SetOpMode`는 unrestricted SDO Write로 제공하지 않는다. LMC가 DS402 mode를 소유하는지 확인한 뒤 dedicated state machine으로 구현한다.
3. 8-byte SDO는 `Int64/UInt64/Real64`, executor buffer, status/result schema, `MaxSdoDataBytes`를 함께 확장한다.
4. SDO Write allowlist는 `(slave,index,subindex,type,length)` 전체 tuple을 검사한다.
5. `0x6040`, `0x607A`, `0x60FF`, `0x6071` 같은 직접 motion/control target은 general SDO Write에서 영구 차단한다.
6. capability bit와 max length는 PLC runtime path가 실제 활성화된 뒤에만 광고한다.
7. CREVIS digital output은 general PI/SDO Write에 섞지 않는다. `0x7E23`의 configured
   GT-22BA output slot-module `IOReference`, whole/masked valid mask와 RT single-writer ticket으로
   분리하고 capability bit 17은 physical output/fault/RT matrix 전까지 0으로 둔다.

### Phase 4 - 선택 기능

- Emergency callback: safety chain 대체가 아닌 telemetry-only로 구현한다. session/sequence/source/drop counter가 필요하다.
- Profile Conditioning: SIGMATEK 또는 drive native 대응 기능과 parameter meaning이 확인되기 전에는 explicit unsupported다.
- WaitUntilCondition: 구체적인 공유 IO/다중축 동시 시작 요구가 승인된 경우에만 PLC-owned queue/latch로 구현한다. WPF polling helper는 완료 구현으로 인정하지 않는다.

## 7. 검증 게이트

각 신규 command/API는 아래를 모두 통과해야 한다.

### 7.1 C#

- exact golden request bytes
- valid/malformed/truncated/trailing response parser
- fake RPC sync/async
- timeout/cancellation/stale session
- unsupported capability와 invalid state fail-fast
- public convenience facade가 raw backend와 동일한 결과를 내는지 검증

### 7.2 LASAL source/static

- exact payload offset/type/length
- enum/semantic key mapping
- axis 1..4와 invalid descriptor 분기
- native method call count와 return propagation
- queue depth/session generation/busy handling
- configured topology revision/order와 legacy `0x7E10` 4-drive byte 호환
- digital output whole/masked canonical form, RT owner와 invalid/stale/offline mutation 0
- `Verify-LasalContract.ps1` SourceOnly/full 갱신

### 7.3 LASAL IDE

- Reload/Rebuild/Link
- Object Network Server/Client `Find in Implementation` smoke
- 변경 function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header 직접 open
- smoke 기준시각 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException` 없음
- Network/object/channel 변경이 필요한 경우 사용자가 LASAL에서 배치/저장 후 외부 implementation 편집

### 7.4 PLC/실물

- success, invalid-state, busy, timeout, duplicate, disconnect, cancel
- physical axis 1..4의 제한 속도/거리 시험
- Stop/PowerOff recovery
- packet capture로 request/response/sequence 확인
- capability 광고와 실제 수행 가능 범위 일치
- GL=physical index 0, Elmo=1..4인 5 slaves + 2 slot-module entry inventory와 node별
  disconnect/recovery quality
- CREVIS DI bit/byte pattern, whole/masked output, unmasked bit 보존과 physical output readback
- output response-loss/reconnect에서 자동 replay가 없고 unresolved mutation이 차단되는지 확인

## 8. 다음 구현 슬라이스

Phase 0, Phase 1과 Phase 2 첫 슬라이스 `0x7D22`의 happy-path source/runtime 검증과
`09b` None/ACS static-alias 검증은 완료했다. 이어 아래 1, 2, 4의 public-SDK
qualification source/build와 Recorder Single/Ring/trigger soak source/build를 완료했다.
다음 순서는 이 runner를 PLC에서 실행해 pcap/QTEST로 닫고, 외부 fault·race·stale matrix를
진행한 뒤 물리 계약이 확정된 기능만 추가하는 것이다.

1. [source/static 완료, live 대기] `0x2047`을 accepted-then-poll 계약으로 수정했다. 새 LASAL IDE build/download 뒤 ACK 0과 후속 `0x2045` Locked/Standby를 재캡처한다.
2. [code/build 완료, live 대기] command B를 A 완료 전에 보내는 true Buffered queue chaining과 safety generation을 이용한 deterministic stop-first를 Test UI에 추가했다.
3. D5 offline/abort, timeout, queued cancel, disconnect/orphan, duplicate/late callback과 contention matrix를 실행한다.
4. [code/build 부분 완료] PI/Bulk exact 24-entry/100 snapshot과 lifecycle/release-reuse를 자동화했다. PLC live, stale map/config/BootId, raw double-release와 partial-entry는 별도다.
5. [code/build 부분 완료] Recorder D3 Single Manual, D4 Ring forced-trigger, 100-cycle trigger soak와 chunk data hash/cleanup을 자동화했다. PLC live, Window/Mask별 matrix, reconnect/adopt, RAM/jitter와 Double은 별도다.
6. 위 runtime gate가 닫힌 뒤 axis velocity override의 persistence/read-back/ownership 계약을 확정하고, Reference/Homing은 physical IO 연결 확인 후 별도로 진행한다.
7. [C# contract 완료, PLC/LASAL/WPF 구현 전] current 5-slave LASAL project를 먼저
   Rebuild/Link/download해 configured order를 고정한다. 이후 `0x7E11/12/13/22` PLC
   read-only source를 구현·검증하고, `0x7E23` RT CAS whole/masked output write는 별도 최종
   gate로 진행한다. 신규 capability는 각 PLC runtime 단계가 끝날 때까지 off다.

구체적인 파일 변경 순서, 공통 scenario runner, 로그 schema와 packet acceptance는
[SIGMATEK 다음 runtime qualification 및 Test UI 설계](SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md)를 따른다.

Homing/SetPosition 같은 물리 위험 기능은 IO/상태/ownership 계약을 승인하기 전에는 capability를 광고하지 않는다.

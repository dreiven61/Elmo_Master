# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

프로젝트 전체 역할과 release gate는
[현재 아키텍처 및 릴리스 상태](../../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
우선합니다.

## 개발 상태

2026-07-31 기준 C# request/typed response path와 재실행 가능한 자동 테스트가
반영됐습니다. tracked `TCPMotionInterface`에는 RPC lifecycle, 실제 LASAL
객체명 lookup, opaque descriptor, 9축 single-axis dispatcher, DINT single-axis path와
현재 공개된 group API handler를 반영했습니다. Diagnostics 개발 source에는
EtherCAT Health/Catalog/PI Read, Bulk Snapshot, Recorder v1, D4 single-bank
  Ring/Trigger, D5 Read 및 fail-closed Write, static EtherCAT topology와 capability-off
  dynamic node health/digital I/O SDK contract가 포함됩니다.

현재 완료도를 구분하면 다음과 같습니다.

2026-08-04 current source의 Home/encoder-maintenance 상태는 다음과 같다.

- Admin `0x7D13/0x7D18/0x7D19`는 각각 `LMC_Home CurrentPositionZero` 시작,
  retained terminal outcome 조회, exact terminal retirement다. `0x7D13`은 더 이상
  switch-search `StartAxisReference`가 아니며 축 이동이나 Home/limit switch를 요구하지 않는다.
  Admin feature bit 4는 source에서 ON이고 WPF Single Axis 화면에 실행/조회 control이 있다.
  Start ACK는 완료 증거가 아니며 `0x7D18` outcome과 `0x7D19` retirement를 확인해야 한다.
- WPF는 `LMC Home outcome:` 로그에 record state, success, original status/error/detail,
  axis status/error, raw drive position, application/internal actual/set/destination/master,
  native command state, evidence flags, stop state, runtime phase와 record generation을 남긴다.
  성공 raw feedback 창은 wrap-safe `-2/-1/0/+1/+2 count`이며 `+/-3 count` 이상은 실패다.
- Admin `0x7D15/0x7D16/0x7D17`의 DS402 method 37, Home offset 0,
  non-moving current-position-zero source와 query/retirement 계약은 존재하지만
  `LMC_DIAG_DS402_HOME_ENABLED=FALSE`이고 Admin feature bit 6도 OFF다.
- Diagnostics `0x7E53/0x7E54/0x7E55`는 source-on이다. TW[20]은
  `0x20FC:0x02 <- UInt16 1`, TW[19]는 `0x20FC:0x01 <- UInt16 1`로 고정되며
  start/outcome/retirement를 분리한다. terminal RPC 결과는 drive의 정확한 물리 효과를
  대신하지 않으므로 선택 축의 독립적인 실기 증거가 필요하다.
- 2026-08-04 Axis2 실행은 axis/native/LASAL zero 조건을 통과했지만 raw feedback
  `8382700 -> 8382701`을 기존 exact-equality gate가 `-7`로 오판해 quarantine했다. 이어진
  Axis1 실행도 같은 조건에서 `8027834 -> 8027836`을 실패로 판정했다. current source와
  SDK는 두 실측에 맞춰 raw 창을 `-2/-1/0/+1/+2 count`로 동기화하고 `+/-3`부터 거부한다. 수정 뒤
  C78 Rebuild/Download와 새 BootId의 한 축 단독 runtime 재검증이 남아 있으므로 이전 PLC
  실행 결과를 최신 source의 완료 증거로 사용하지 않는다.

- 기존 Wireshark 캡처 기준 command: 23개
- LASAL project-local extension command: 2개
  (`0x204A GroupPowerOn`, `0x204B GroupPowerOff`; 기존 캡처 명령이 아님)
- LASAL diagnostics command namespace는 기존 `0x7E00..0x7E51`에 source-on
  encoder-maintenance `0x7E53/0x7E54/0x7E55`가 추가됐다. 2026-07-31의 C# 33개,
  LASAL route 32개 집계는 이 세 ID를 포함하지 않은 역사적 checkpoint다. output write
  `0x7E23`은 여전히 없다
  - D0~D3: capability, Health/Catalog/PI Read, Bulk, Recorder v1 handler 활성
  - D4: PLC single-bank Ring/Trigger와 `0x7E42` 활성. 두 개의 1.28 MB 고정 bank,
    bank별 identity/metadata, full `ResourceBusy`, exact all-bank rebind와 isolated release는
    dormant LASAL source로 구현됐다. `0x7E4A` exact bank inventory, `0x7E4B`
    empty-configuration adopt와 token-qualified `0x7E4C/0x7E4D` Configure/조회도 dormant
    복구 계약으로 구현됐고, 0x7E4B는 Start를 허용하지 않는 release-only lease를 반환한다.
    PC 측에는 Configure 전 journal, exact inventory/adopt,
    bank/configuration Release intent-confirmed checkpoint와 재시작 presence/absence 복구도 있다.
    WPF qualification/retained-cleanup/reconnect/config-only manual Configure adapter도 구현됐지만
    `RecorderDoubleManualActionsReady`, `RecorderDoubleManualConfigureRouteReady`,
    `RecorderDoubleQualificationExecutionReady`, `RecorderDoubleReconnectRecoveryReady` 네
    proof/route gate는 모두 `false`다. 따라서 store gate,
    capability bit 6과 `RecorderBufferCount=2`도 계속 비활성이고 LASAL
    build/RAM/jitter/live/pcap 검증 전이다.
    UI 독립 retained/release 11개와 PLC core reference model 1개도 PC 계약으로 검증한다
  - D5: test profile에서 축 1~4, nonzero ObjectIndex, 임의 U8 SubIndex와 exact typed
    1/2/4-byte SDO Read ticket/status/queued cancel 활성. public
    `ReadSdoInline[Async]`는 capability preflight, accepted ticket와 bounded status poll을
    결합하고 같은 owner/session의 exact `Completed/Success` 결과만 반환한다. 이미 수신한
    terminal 성공/실패는 늦은 cancellation보다 우선하며, nonterminal PC-side wait
    cancel/timeout 뒤에는 accepted ticket과 exact `LastObservedStatus`를 보존한다. PLC cancel 또는 replay는 하지 않는다. SDO Write의
    parser/executor/API/WPF 경로와 축 1의 Gold UI[24] `0x2F00:24` Int32/4-byte 단일
    SDK/PLC source gate가 활성화됐다. 일반 수동 Write는 exact current
    connection/session/build/boot/map에서 same-value 4-ticket qualification을 먼저 통과해야
    열린다. 축 2~4와 다른 tuple은 계속 차단되고,
    extended result도 비활성이다. public `LMCSdoWriteVerificationContext`는 accepted Write
    ticket의 immutable submitted-request provenance를 exact readback에 묶는다. read-only
    deliberate contention -> exact `ResourceBusy` -> recovery qualification runner/core는
    구현됐고 timeout -> exact `Expired` -> bounded late-callback drain -> recovery runner/core도
    구현됐다. 두 경로의 실제 PLC packet은 아직 없다
  - EtherCAT topology/I/O: `0x7E11/12/13/22/23` C# model/parser/public API와 개발 WPF
    구현. PLC/LASAL은 static `0x7E11/12`, revision `0x15867EEC`, ordered 7-entry와
    CREVIS/Elmo read-owner `0x7E13/22`를 구현했다. bit 15~17은 의도적으로 OFF이고
    `0x7E23` handler와 SDK output allowlist는 없다
  - Phase 1 PI/Bulk compatibility facade: catalog alias PI Read와
    `AddEntry/Configure/Upload/GetEntry` local builder/reader 구현; wire는 D1/D2 재사용
- LASAL admin source-active Home command: 3개
  (`0x7D13 StartAxisHome`, `0x7D18 ReadAxisHomeOutcome`,
  `0x7D19 RetireAxisHomeOutcome`). `0x7D00`, `0x7D10`, `0x7D20`, `0x7D22`도
  기존 source-active command다.
- LASAL admin dormant/fail-closed command family:
  - `0x7D12 SetAxisPosition`: capability bit 3 OFF, valid request도
    `InvalidState/detail 10`, native `_LMCAxis.SetPosition` 호출 0회
  - `0x7D15/0x7D16/0x7D17` DS402 Home: method 37 non-moving source가 있으나
    `LMC_DIAG_DS402_HOME_ENABLED=FALSE`, Admin capability bit 6 OFF
- SDK-only Admin read-only recovery query: 1개
  - `0x7D14 ReadAxisSetPositionOutcome`: capability bit 5와 PLC retained store/route 없음
- 2026-07-31 command-count checkpoint는 active 53, dispatcher/wire 63, C# ID 65,
  요구사항 `D+E=40/65`, partial 포함 `52/65`였다. 이 역사적 집계는 이후 추가된
  `0x7D15..0x7D19`와 `0x7E53..0x7E55`를 포함하지 않으므로 current command count로
  사용하지 않는다.
- C# diagnostics 공개 API: D0~D5와 capability-off EtherCAT topology/I/O sync/async contract 구현
- LASAL diagnostics test source capability:
  - 정상 retained BootId 경로의 base 값 `0x0000613F`에 승인된 SDO Write bit 9와
    TW[20]/TW[19] bit 18/19를 더한 기대값: `CapabilityBits=0x000C633F`
  - bit 0~2: Health, SignalCatalog, PIRead
  - nonzero retained BootId일 때 bit 3~4: BulkSnapshot, RecorderSingleBank
  - nonzero retained BootId일 때 bit 5: RecorderTrigger
  - nonzero retained BootId일 때 bit 8: SDORead, bit 13: SDOReadGeneralInline,
    `MaxSdoDataBytes=4`
  - bit 14: static `EtherCATTopology`
  - bit 9: 축 1 UI[24] gate가 활성인 새 PLC build에서 1
  - bit 18: TW[20] error/warning reset, bit 19: TW[19] multi-turn position reset
  - bit 6, 7, 10~12, 15~17: 0
- CyWork axis/group control·read·motion command: 18개
  (lifecycle과 name/member metadata handler 제외)
  (`0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`, `0x209F`,
  `0x20A0`, `0x20A2`, `0x204A`, `0x204B`, `0x2047`, `0x2048`, `0x2045`, `0x2049`,
  `0x2085`, `0x20A4`, `0x2051`, `0x20E7`)
- 기존 캡처 기반 23-command 공개 범위의 deterministic unsupported: 0개
- C# 자동 테스트 runner current Release 검증: 1111/1111 PASS. 이번 callback
  consumer tranche에서는 Debug runner를 다시 실행하지 않았다. fake-RPC request/session
  atomic append와 stable snapshot 회귀, Axis/Group sync/async typed lookup의
  exact 6-byte/nonzero descriptor, structured failure와 raw 방어 복사 회귀에 더해 topology/I/O read-only raw qualifier의
  옵션/allowlist/mutation 차단/dormant capability/dry-run/fake 17-request sequence와
  bounded info/chunk/node/BootId/durable checkpoint 부정 회귀 12개,
  topology-bound health/I/O fail-closed facade, CREVIS exact status-cause matrix, topology/D5
  fixed-seed parser property와 opt-in parser-stress CLI 계약, SDO in-flight editor와
  stale-readback recovery policy, output
  uncertainty acknowledgement, public SDO Write verification provenance 4개, pinned capability
  single-wire health/DI와 pre-wire guard 3개, Catalog/Topology aggregate owner/session
  provenance, auto live monitor policy 7개, send-priority ordering/zero-wire/SDO·DO
  `NotAttempted`와 Axis Power On total-deadline/read-only result, Axis Stop Begin/Resume 및
  process-local mutation 귀속 전용 32개, Axis Reset accepted-once Begin/Resume·mutation 귀속 전용
  33개, Axis PowerOff accepted-once/mutation 귀속 전용 35개, Admin SetPosition exact
  28/36-byte wire, CAS, one-shot/capability/session/fail-closed/strict-ErrorId/native-reject/publication-race/uncertain 회귀 18개, Admin
  `GroupMoveLinearRelative`·D5 `SubmitSdo`/`CancelOperation` 지연 ACK의
  `ResultDiscarded` 회귀, RPC lifecycle deterministic race 회귀 19개, Group Power On/Off
  accepted-once Begin/Resume·typed pending/interference·publication 회귀 35개와 Group Enable total-deadline,
  accepted-ACK/stable-status/continuation/Disable 선형화 회귀 40개,
  Double-bank retained lifecycle, `0x7E4A..0x7E4D` response-loss 재접속, Recorder
  Trigger/Stop 지연 ACK의 `ResultDiscarded`와 네 Release surface의 `OutcomeUnverified`
  quarantine, public Release outcome-unverified, final configuration Release의 typed
  canonical-empty absence, durable v3
  release intent/confirmed, exact pending bank/config intent 재사용과 retained ACK-success
  zero-replay confirm/resolve 회귀, PLC core reference model 1개,
  semantic Recorder header canonicalization 5개 및 process-termination
  journal reopen 회귀, D1 one-slave Health/PI baseline-fault-recovery 회귀 6개,
  D5 contention exact Busy/recovery 및 accepted/uncertain 보존 회귀 12개와 D5 timeout
  exact Expired/drain/recovery 회귀 14개, D5 disconnect/orphan application-recovery와
  evidence 보존 회귀 28개, public bounded inline SDO Read의 typed terminal/pre-wire
  rejection/failure/timeout/cancel evidence 회귀 7개를 포함한다. 이는 PC
  build/test 증거이며 PLC live/pcap 증거가 아니다.
  현행 `0x7D13/0x7D18/0x7D19` LMC Home 계약은 CurrentPositionZero exact
  frame/validation/parser, one-shot start, no-replay outcome query와 exact terminal
  retirement 경계를 고정한다.
- `parser-stress --seed <u32> --iterations <8..1000000>`는 topology info/chunk,
  node health, digital input/output, D5 variable-inline과 recoverable Recorder
  Configure/Inventory의 여덟 parser family를 total round-robin으로 변이한다. raw frame은
  최대 1,572 bytes로 제한하고 첫 unexpected
  exception/timeout에 seed, iteration, family와 전체 hex를 출력한다. Release 고정 시드
  `0x7E4C7E4D` 100,000회는 accepted 1,511, exact `InvalidDataException` reject 98,489로
  PASS했다. 이 도구는 파일/네트워크/PLC I/O를 하지 않는다.
- LASAL `IntegratedReadOwnerDormant` SourceOnly/full static contract: PASS. `LMCDiagnosticsService`
  constructor의 전체 38-state 이름/타입, 37개 scalar exact-once,
  `BulkSignalIds[0..23]`, control-flow 금지와 final `C_OK`
  순서를 negative fixture로 고정한다. `LMCRecorderStore` constructor의
  scalar/array/recovery-token/two-bank descriptor 초기화, metadata-before-Empty publish와 final `C_OK`
  순서를 actual-source negative fixture로 검증한다. tracked `Classes.lcb`에는
  `TryStartWrite`, `ActiveIsWrite`, `WriteBuffer`, `SdoWriteData`, `GetSdoWritePolicyDetail`
  declaration이 동기화돼 있다. CREVIS용 generated client/network와 method declaration은 LASAL
  IDE에서 생성했고, `LMCEcatInputLatch`의 464-byte seqlock snapshot과
  `LMCDiagnosticsService`의 `0x7E13/0x7E22` handler를 external source implementation으로 완성했다.
  default checkpoint는 read-owner 구현과 bit 15~17 OFF, `0x7E23` 부재를 함께 검증한다.
- 개발 WPF example current Release rebuild: 경고 0, 오류 0. actual-control startup
  smoke는 VS2019 MSBuild Release에서 332/332 PASS다. 이번 tranche에서는 Debug
  build/smoke를 다시 실행하지 않았다. Admin capability/axis/group와 Drive mode/non-atomic status를
  exact fake-RPC 및 non-default axis lookup/AxisInfo payload로 검증한다. D5
  abort/contention/timeout/queued-cancel/abrupt-disconnect 버튼의 capability/idle/interlock gate, typed v2 SDO
  restart recovery의 capability-off zero-wire, 잠긴 D4 journal fail-closed와 active D4
  journal의 child-process restart/Recorder zero-replay에 더해 Double recovery Guid의 결정적
  RequestedConfigId, active journal의 독립 recovery capability contract/gate-off zero-wire와
  semantic journal conflict/runtime I/O failure 분리와 invalid PI/Bulk raw의
  `UNAVAILABLE` 표시를 확인한다. Axis Stop은 Begin 1회 뒤 status-only Resume을 수행하고 더 새
  Power Off가 monitor를 선점해도 `0x2022`를 replay하지 않음을 확인한다. Group Power On/Off는
  durable journal을 command보다 먼저 arm하고 accepted observer에서 exact continuation과 ACK를
  보존한 뒤 status-only Resume으로 3개 stable status를 요구한다. 재시작 뒤 accepted record는
  exact identity의 read-only 확인만 허용하고, 불명확한 Power On은 자동 replay 없이 명시적 Power Off
  takeover로만 복구한다. On/Off 모두 ACK 직후 child process를 강제 종료한 회귀에서 새 process가
  journal lock을 다시 획득하고 `0x204A`/`0x204B` 재전송 없이 `0x2045` 3회만으로 resolve함을 확인했다.
  Axis Power On/Off는 방향을 포함한 공용 durable v2 journal을 사용한다. fresh Power Off는
  `0x2023(false)` 전에 arm되고 accepted observer가 첫 `0x2028` 전에 ACK 상태를 기록한다.
  accepted 또는 outcome-uncertain Off의 재시작은 exact endpoint/axis/reference/BootId/MapRevision을
  확인한 뒤 `0x2028`만 사용한다. Axis Power Off ACK 직후 child process를 강제 종료한 회귀도
  journal lock 재획득, 재시작 `0x2023` 0회, `0x2028` 3회와 동일 identity의 `Resolved`를 확인한다.
  safety generation 검증을 통과한 수동 Group Status 성공 응답은 상태에 맞는 pending Enable
  continuation proof에 누적되며, Locked Standby proof가 3/3이면 기존 ACK를 재사용한 zero-wire
  Resume으로 완료할 수 있다. safety 예약은
  pending Enable의 누적 proof를 즉시 0으로 되돌리고 ACK와 continuation은 유지한다. 예약 뒤 도착한
  `GroupReadStatus` 결과는 drain 후 `ResultDiscarded`로 폐기한다. 예약 전에 SDK 완료가 확정됐지만 WPF
  적용 전에 safety가 예약된 좁은 경우만 recovery-required로 승격한다. connected unresolved 상태에서는
  group 이름 변경, group 재조회, clean connection/window close, connected reconnect와 새 Power On을
  차단한다. 외부 connection loss 뒤 reconnect 진입에서 원 exact group 이름을 보존한 recovery로 승격한다.
  명시적 `0x2048` Disable ACK는 Unlock 요청 접수만 뜻하며 pending/recovery를 해제하지 않는다.
  accepted pending과 recovery-required는 exact group identity에서 PowerOn=True +
  Disabled/Unlocked 3회 연속 또는 PowerOn=False 3회 연속 proof가 끝난 뒤에만 해제한다.
  Power On 성공만으로는 해제되지 않으며 어느 경로도 `0x2047`을 replay하지 않는다. fresh
  Enable은 endpoint IP/port, group name/reference, BootId와 MapRevision을 durable journal에
  `0x2047` 송신 전에 기록한다. 재시작의 Armed record는 RecoveryRequired로 승격하며 exact
  endpoint는 RPC 전에, BootId/MapRevision은 연결 뒤, group reference는 lookup 뒤 대조한다.
  verified Enable/Disable/PowerOff의 identity 확인 뒤 safety generation을 다시 검사하고 durable
  `Resolved`를 먼저 기록한다. mismatch와 post-identity safety race는 `0x2047` replay 없이
  recovery를 유지한다. Group Profile Lock journal은 기존 state 1~3을 유지한 채
  `AcceptedAwaitingProof=4`를 추가해 format-version 1과 호환된다. accepted ACK 뒤 첫 status에서
  child process를 Kill한 회귀는 exact endpoint/group/reference/BootId/MapRevision을 다시 확인하고
  journal lock을 재획득한 새 process가 `0x2047` 0회, `0x2045` 3회로 resolve함을 확인했다. 이
  status-only 복구는 process-local Set Identity/Home Check를 복원하지 않으므로 Move가 fail-closed하고,
  Armed는 기존처럼 safety-only recovery다. 이는 fake-RPC/PC 증거이며 PLC/hardware proof가 아니다. D5
  full-handler smoke는 old/new TCP 두 세션을 실제로
  수락해 old 세션의 `0x405D` 0회, distinct owner 채택, exact recovery ticket 두 개,
  quarantine clear와 다른 revision의 CREVIS topology 재로딩을 함께 확인한다. bits 14~16 fake RPC에서는
  `0x7E13/0x7E22` Health/selected-DI 표시, output-shadow background poll 0회, 늦은 수동 응답의
  selection/session guard, mixed-I/O output proof와 Health/DI channel별 stale/error를 검증한다.
  이는 current LASAL source의 handler가 실제 PLC에서 실행됐다는 증거가 아니다. qualification/cleanup/reconnect
  adapter는 연결됐지만 네 proof/route gate는 모두 `false`다.
  실제 D5 PLC 동작과 visual 확인은 별도다
- DiagnosticsBootCounter/D1~D4 single-bank와 gate-off D5 source LASAL IDE
  Rebuild/Link: 0 error, version mismatch warning. gate-on fixed-source runtime download는
  BootId 5 capture로 확인했지만 대응 IDE build/smoke log는 미보존
- 위 통합 source의 `Find in Implementation` smoke: InputLatch, RecorderStore,
  TCPMotionInterface.Diagnostics 3건 PASS; smoke 이후 `Lasal2.log`의 신규
  `CInvalidArgException` 0건
- SDO Write/topology/same-peer 편집 당시 fresh LASAL IDE Rebuild/Link는
  `0 error(s), 20 warning(s)`, Linker `Done`이며 변경 class smoke와 신규
  `CInvalidArgException=0`도 PASS했다. 이 값은 current callback+`0x7D12`+`0x7D13`
  편집 전 checkpoint다. current source의 fresh IDE Save/Rebuild/Link와
  `TCPMotionInterface`/`LMCControlCommandService` smoke, PLC download, 실축과 EtherCAT
  mailbox는 검증하지 않았다.
- CyWork와 motion RT thread의 CPU core/priority 조건: 미검증
- diagnostics PLC: `11_PI_Bulk_Regression`의 D0/D1/D2 happy path와
  `10_DriveRead_Axis1to4`/`12_SDO_GeneralInline_4Byte_FailureRecovery`의
  general-inline 1/2/4-byte 및 same-BootId TypeMismatch recovery packet PASS.
  D1/D2 partial 판정과 D1 Health/PI 축 일치·stale 표시·복구 판정 코드는 완료됐지만
  fault/soak live capture, D3/D4 전체와
  D5 나머지 fault matrix는 별도. read-only D5 abort -> known-valid recovery와 deliberate
  contention -> exact `ResourceBusy` -> recovery WPF runner/순수 판정 코드는 build/test
  완료했지만 PLC live와 pcap은 미검증

기존 motion/control PC API 범위는 캡처 기반 23개 command와 LASAL local motion
extension 2개 모두 request/public path까지 구현됐다. Diagnostics는
`LMCConnection.Diagnostics` 아래 D0~D5 공개 API와 common envelope, capability,
Catalog/Health/PI/Bulk/Recorder/ticket/chunk parser를 제공한다. 현재 PLC test build가
광고하는 실제 실행 범위는 D1 read-only, D2 Bulk, D3 single-bank manual Recorder,
D4 single-bank Ring/Edge/Window/Mask/forced Trigger와 D5 general-inline SDO Read다.
D5의 legacy `0x1000:0` 4축 path와 general-inline 1/2/4-byte SDO Read는 live packet으로
확인했다. 의도한 TypeMismatch terminal failure 뒤 같은 BootId의 다음 Int8/1 ticket
success도 확인했다. offline/abort, queued cancel, disconnect/orphan과 timeout/contention의
실제 PLC qualification은 남아 있다. abort -> recovery는
`0x6061:0 Int8/1` baseline과 같은 BootId/MapRevision의 복구를 판정하는 WPF runner까지
구현했지만 실제 abort code와 recovery packet은 아직 확보하지 않았다. contention은 첫
Read ticket을 완료시키기 전에 같은 request를 한 번 더 제출해 두 번째 요청의 exact
`ResourceBusy` rejection을 요구하고, 첫 ticket의 `Completed+Success` 뒤 세 번째의 서로 다른
ticket과 같은 value/type/length를 요구하는 WPF runner까지 구현했다. 실제 23f packet은 아직
확보하지 않았다.

disconnect/orphan은 UI 독립 검증 코어와 PC 회귀 28개, production WPF adapter까지 구현했다.
`Run D5 Abrupt Disconnect -> App Recovery`는 old owner의 read-only probe가 nonterminal일 때
local TCP를 zero-linger로 닫고 RPC Close `0x405D`를 보내지 않는다. 이후 서로 다른 새
`LMCConnection`을 열어 fresh owner/session-bound capability를 두 번 확인하고, exact
`0x6061:0 Int8/1` recovery ticket 두 개와 마지막 capability sample의 BootId/MapRevision,
DiagnosticsBuild/CapabilityBits, BaseCycleTimeUs, MaxSDO 및 request/response payload limit 불변성을
검증한다. old executor drain 동안 recovery Submit이 실패하면 request timeout + 5초, 최대
120초의 monotonic retry-admission budget에서 25 ms 간격으로 exact
`Rejected/ResourceBusy`만 재시도한다. accepted 또는 outcome-uncertain 응답은 자동 재시도하지
않는다. 이 budget은 이미 시작된 단일 RPC의 소요시간 상한을 뜻하지 않는다. PASS log는
quarantine clear 전에 commit하며, clear 뒤 늦은 cancel은 기존 PASS를 `ABORTED`로 뒤집지 않는다.
성공 뒤 새 connection을 GUI가 adopt하고
CREVIS topology를 자동 reload한다. old ticket이 loss 전에 terminal이면 disconnect를 수행하지
않고 `INCONCLUSIVE`다.

이 경로의 최종 판정은 old status가 Running이었더라도 항상
`ApplicationRecoveryOnly`, `orphanQualified=false`다. PC socket close와 Running 표본만으로는
PLC의 exact `MarkOrphan`, executor token, late callback drain을 증명할 수 없다. 실제 orphan
PASS에는 PLC에 남는 lifecycle witness와 live PLC/pcap이 추가로 필요하다. 취소, ambiguous
submit, identity drift, ABA, owner-state race 또는 PASS log 실패 시 quarantine 증거는 지우지 않는다.

신규 topology/I/O 공개 API는 `GetEtherCATTopology*`, `ReadEtherCATNodeHealth`,
`ReadDigitalIO`, `SubmitDigitalOutputWrite`다. current LASAL source는 static topology bit 14,
`0x7E11/0x7E12`와 dormant read-owner `0x7E13/0x7E22`를 처리한다. capability bit 15~17은
의도적으로 OFF이고 output write `0x7E23`은 없다.
`GetEtherCATTopology[Async]`가 반환한 immutable aggregate는 diagnostics owner와 connection
session generation에 bind되며 `BelongsTo`/`BelongsToCurrentSession`으로 확인한다. topology-bound
Health/Digital I/O는 unbound, foreign, reconnect-stale aggregate를 capability/read RPC 전에 거부하고
검증한 topology session generation을 실제 exchange까지 유지한다. 로컬 topology validator와 raw
observation-only overload는 호환을 위해 그대로 사용할 수 있다.
raw `ReadDigitalIO(request)`는 observation-only 호환 경로다. output write request는
`ReadDigitalIO(topology, request)`가 NodeId, IOReference, 방향과 폭을 topology에 대조해 반환한
`HasValidatedTopologyBinding=true` snapshot에서만 만들 수 있다.
개발 WPF의 auto live monitor는 bit 15 node health 또는 bit 16 selected DI가 있을 때만 tick당
owner/session-bound cached capability snapshot을 pinned topology-bound overload에 넘긴다. 따라서
eligible tick은 별도 `0x7E00` refresh 없이 `0x7E13` 또는 `0x7E22`를 정확히 1회만 보내고
configured topology와 live columns를 분리한다. 일반 non-pinned SDK overload의 capability
refresh+read 계약은 유지된다. 현재 두 bit가 off이므로 WPF 정상 경로의 wire request는 0회이며
background에서 output shadow나 write-authorizing snapshot을 갱신하지 않는다.
개발 WPF의 수동 Health/DI도 클릭 시점의 owner/current-session capability snapshot을 pinned
overload에 넘기므로 read 앞에 추가 `0x7E00`을 보내지 않는다. current-session commit gate를
통과한 Auto/Manual Health/DI read attempt만 최대 4,096개 FIFO journal에 기록하며 oldest-drop
count를 별도로 보존한다. failure record에는 이전 성공 sample을 복제하지 않고 TXT/CSV export는
UTF-8 no-BOM이다. capability bit 15/16 off는 새 wire/record가 모두 0이고, stale/late response는
원 request가 이미 송신됐을 수 있지만 record로 commit하지 않는다.
이 journal은 PC가 파싱한 PLC response와 read failure evidence이며 physical cable order, 실제 DI
접점, physical DO feedback 또는 PLC 구현 완전성 증거가 아니다. 현재 `0x7E13/0x7E22`의
LASAL source/IDE 정적 증거는 있지만 PLC runtime/actual-hardware proof는 없다.
`SubmitDigitalOutputWrite`는 nonzero topology/output revision, mask와 BootId를 요구하고
`OperationKind=4` ticket을 사용하지만 SDK write allowlist가 empty이므로 신규 command를
송신하지 않는다. 이 C# contract는 LASAL build나 CREVIS runtime 지원 증거가 아니다.

SDO Write 성공 뒤 검증은 public `CreateSdoWriteVerificationContext`가 담당한다. 이 factory는
승인된 Write request와 accepted ticket의 내부 immutable submitted request가 operation flags,
target, type, length, timeout과 4-byte 값까지 exact match인지 확인한다. 여기에 같은 owner/session에
bind된 exact ticket/SubmitCycle/BootId의 `Completed+Success` Write terminal status까지 요구한 뒤
nonzero BootId와 MapRevision에 묶인 context만 만든다. context의 `SubmitReadback[Async]`는 같은
target/type/length의 SDO Read를 기존 guarded submit 경로로 보내고, `Evaluate`는 read ticket,
fresh capabilities와 Read status도 같은 owner/session provenance인지 확인한다. SubmitCycle,
fresh capability observation sequence가 context 생성 baseline보다 크고 identity, terminal success와
exact result bytes가 모두 일치할 때만
`Verified`를 반환한다. WPF도 별도 local matcher 대신 이 SDK context를 사용한다. public
`EvaluateSdoWritePolicy`는 immutable approved-target snapshot과 connection/capability/identity/payload
blocker matrix를 cached observation만으로 평가해 wire를 보내지 않는다. WPF readiness도
`EVALUATION_WIRE=NONE`과 PLC bit 9 및 SDK `NoApprovedTarget`을 독립적으로 표시한다. 현재 SDK
목록에는 축 1의 UI[24] exact target 하나만 있고 축 2~4는 승인되지 않는다. 새 LASAL source를
Rebuild/Link하고 PLC에 download한 뒤 fresh capability가 bit 9를 광고해야 제출 가능하며, 기존
download의 bit 9가 0이면 `SdoWriteCapabilityMissing`으로 계속 차단된다.
기반 public guarded `SubmitSdo[Async](readRequest, requiredIdentityTicket)`도 SDOWrite ticket의
immutable submitted provenance와 read target/type/length exact match를 강제한다. readback 재시도의
timeout은 달라도 되지만 target/type/length를 바꿀 수는 없다.

WPF mutation journal format v2는 SDO Write의 Slave/Object/SubIndex/Type/Length/Timeout과
expected 4-byte 값을 checksum 범위 안에 typed metadata로 저장한다. legacy v1 record는 읽되
typed metadata가 없으므로 protocol recovery는 zero-wire/fail-closed다. 재시작한 v2 record가
`TerminalSuccessPendingReadback`이고 current SDK allowlist의 exact target과 일치할 때만 운영자가
명시적으로 recovery 버튼을 눌러 read-only SDO ticket을 한 번 실행할 수 있다. Read 전후의
fresh capability가 원 BootId/MapRevision과 일치하고 exact result bytes 및 같은 record/state의
atomic CAS까지 통과하면 durable `Resolved` tombstone을 먼저 기록한다. mismatch는
`ReadbackMismatch`로 보존한다. Write command는
재전송하지 않는다. 승인되지 않은 축 2~4, 다른 tuple 또는 bit 9가 없는 PLC에서는
capability/SDO recovery wire가 모두 0회다.

D5 qualification은 Submit 전에 outcome evidence를 arm해 응답 유실을 unknown-ticket로
quarantine한다. accepted `LMCOperationTicket`은 owner `LMCConnection`, `DiagnosticsBootId`,
실제 제출 `SubmissionMapRevision`과 15~120초 deadline-aware cleanup 정보를 보존한다. ledger
전이는 owner/BootId/MapRevision이 exact match일 때만 허용한다. 모든 pending-ticket cleanup은
status/cancel 전에 같은 connection의 BootId와 MapRevision을 선검증한다. 둘 중 하나의 변화,
exact `BootIdMismatch` 또는 stale local session은 old terminal로
간주하지 않고 quarantine한다. 반면 같은 Boot/session의
exact `TicketNotFound`는 one-terminal-slot 교체 계약상 이전 ticket이 terminal이었다는 사실만
증명한다. 상태는 `TERMINAL_INFERRED`, outcome은 `UNKNOWN`으로 기록하고 해당 ticket을 해제한다.
여러 evidence의 recovery proof는 current capability가 GeneralInline이면 서로 다른 두
`0x6061:0 Int8/1` ticket, legacy SDORead-only이면 서로 다른 두 `0x1000:0 UInt32/4` ticket을
사용한다. stable BootId/MapRevision 아래 두 결과의 exact type/length/bytes가 같고 proof 중
evidence 목록이 불변일 때만 quarantine을 해제한다. UI 독립
`D5SdoRecoveryScopePolicy`는 owner reference+BootId+MapRevision 조합만으로 scope를 순수
판정하고 MainWindow는 proof 시작 로그와 PASS 로그에 같은 decision을 사용한다. 모든 evidence가 current
owner+BootId+MapRevision과 같은 동질 집합이면 `same_owner_connection_recovery`, current
owner를 공유하면서 이전의 한 BootId+MapRevision으로 동질이면
`new_diagnostics_identity_session`, 모두 current owner와 다르면서 한 previous
owner+BootId+MapRevision으로 동질이면 `new_connection_session`이다. owner 또는 submission
identity가 섞이면 `mixed_evidence_sessions`로 분류하며 same/new session 증거로 세지 않는다.
mixed도 two-ticket application recovery proof와 성공 시 quarantine clear는 허용한다.
`same_owner_connection_recovery`는 old terminal/disconnect/orphan PASS가 아니다.
한 previous owner+identity로 동질인 `new_connection_session`만 decision의
`NewConnectionRecovery=true`이고 로그에 `newConnectionRecovery=true`로 기록한다. WPF는 항상
`orphanQualified=false`로 기록한다. 이는 새 RPC connection에서 application
recovery가 성립했다는 뜻일 뿐 PLC 내부 orphan cleanup이나 late callback을 증명하지 않는다.
실제 orphan PASS에는 known Running old ticket, 실제 owner loss와 별도 PLC hook/capture가
필요하다. 로그는 `evidenceBootIds`/`evidenceMapRevisions`,
`recoveryBootId`/`recoveryMapRevision`, `proofScope`, `mapChangedEvidence`,
`sameIdentityEvidence`, `mixedEvidenceSessions`, `newConnectionRecovery`,
`orphanQualified=false`를 구분한다. unresolved
동안 Group Disable을 포함한 새 mutation과 모든 다른 qualification은 차단하되 기존 Bulk/Recorder/queued-ticket
cleanup, Stop/PowerOff와 read-only는 허용한다. Resolve는 same-session/new-Boot에서도
실행할 수 있고 reconnect는 외부 connection loss 뒤 new-connection proof에만 사용한다.
`D5SdoPendingCleanup` Resolve는 기존 qualification log를 지우지 않고 이어 쓰며
`D5_LOG_CONTINUATION`을 남겨 원래 `FAIL`/`OUTCOME_UNCERTAIN`과 해결 증거를 같은 QTEST
log에 보존한다.
Deliberate contention runner는 canonical `0x6061:0 Int8/1` request와 exact
`SDORead+SDOReadGeneralInline`, `MaxSdoDataBytes=4`, nonzero BootId/MapRevision을 preflight한다.
두 번째 Submit은 `LMCDiagnosticsCommandException.Detail=ResourceBusy`,
`LMCSdoSubmissionFailureContext.Phase=Submission`, `Outcome=Rejected`, 동일 request/identity,
accepted ticket 없음이 모두 맞아야 Busy 증거로 인정한다. 두 번째 요청이 accepted 또는
outcome uncertain이면 해당 evidence를 quarantine에 보존하고 세 번째 Submit은 보내지 않는다.
exact Busy인 경우에만 첫 ticket의 terminal success를 확인하고 세 번째 distinct ticket의
terminal success와 baseline value/type/length 일치를 확인한다. 이 판정은 PC 자동 시험 계약이며
PLC의 one-terminal-slot contention 동작을 아직 증명하지 않는다.
Timeout runner는 같은 canonical target의 정상 baseline 뒤 `TimeoutCycles=1` ticket에 exact
`Expired/TimedOut`, `OperationErrorId=0`, `OperationDetail=0x05040000`, zero result를 요구한다.
늦은 callback drain 중 recovery Submit은 동일 request/BootId/MapRevision, no-ticket의 exact
`Submission/Rejected + ResourceBusy`일 때만 25 ms 간격 최대 600회 재시도한다. 다른 오류,
accepted-context 또는 outcome-uncertain evidence는 보존하고 즉시 중단한다. drain 뒤 distinct
recovery ticket의 exact same-value `Completed/Success`가 필요하다. 이 역시 PC 자동 시험 계약이며
실제 PLC timeout/drain packet을 아직 증명하지 않는다.
Phase 1 read-only facade는 diagnostics domain command 실패를 기존
`LMCDiagnosticsCommandException`의 subtype인 `LMCSdoReadCommandException`으로 문맥화한다.
`CapabilityPreflight`/`Submission`에는 accepted ticket이 없고 `StatusPolling`에는 정확한
ticket이 보존된다. WPF는 pre-ticket command rejection이면 outcome guard를 해제하고 status
command failure이면 known ticket을 보존한다. 기존 base exception catch 호환성은 유지된다.
`GetDriveOperationMode[Async]`/`ReadDriveStatus[Async]`의 모든 실패는 새 wrapper로
바꾸지 않고 던져진 예외 객체, 원래 타입과 stack을 그대로 보존한다. 호출자는
`LMCDriveReadFailureContext.TryGet(exception, out context)`으로
`FacadePreflight`/`AxisStatusRead`/`CapabilityPreflight`/`Submission`/`StatusPolling`/
`ResultMaterialization` phase와 각 SDO attempt의 `GenericSubmissionOutcome`에서 공용
`LMCSdoSubmissionOutcome` (`NotAttempted`/`Rejected`/`OutcomeUncertain`/`Accepted`)을
읽는다. 기존 `SubmissionOutcome`/`LMCSdoReadSubmissionOutcome`은 source/binary 호환용으로
같은 값을 유지한다. attempt snapshot은
capability identity가 확보된 경우 실제 Submit에 사용한 `DiagnosticsBootId`/`MapRevision`을
보존하고, 확보 전 실패는 0 sentinel을 사용한다. WPF는 no-submit, explicit
rejection 또는 terminal status가 있는 실패는 guard를 해제하고,
`OutcomeUncertain`은 quarantine하며, `Accepted` nonterminal은 exact ticket을 보존한다.
context가 없거나 내부 증거가 일관되지 않으면 기존 UNKNOWN quarantine을 유지한다.
raw manual `LMCConnection.Diagnostics.SubmitSdo[Async]`도 원래 exception 객체/타입/stack을
보존하고 `LMCSdoSubmissionFailureContext.TryGet(exception, out context)`으로
`LMCSdoSubmissionPhase` (`RequestValidation`/`SessionPreflight`/`CapabilityPreflight`/
`Submission`/`PostSubmissionValidation`)와 공통 `LMCSdoSubmissionOutcome`, request,
capability identity 확보 후의 실제 `DiagnosticsBootId`/`MapRevision`, accepted ticket을
제공한다. identity 확보 전 실패는 두 값을 `0` sentinel로 둔다. WPF manual router는
`NotAttempted`/`Rejected`를 disarm하고, `OutcomeUncertain`은 context의 실제 identity로
unknown-ticket evidence를 reconcile한 뒤 quarantine하며, `Accepted`는 exact ticket을 manual
diagnostic state와 D5 tracker 둘 다에 보존한 뒤 disarm한다. 이때 ticket의
`DiagnosticsBootId`/`SubmissionMapRevision`과 owner를 context의 BootId/MapRevision 및 현재
`LMCConnection`에 exact match시킨다. context가 없거나
일관되지 않으면 fail-closed한다. 이 경로는 code/test 계약이며 PLC live/pcap
증거가 아니다.
성공한 Write 뒤 exact Readback에는 원 Write ticket을 받는 guarded
`SubmitSdo[Async]` overload를 사용한다. 이 overload는 owner/current session을 capability
RPC 전에 확인하고, fresh capability의 `DiagnosticsBootId`와 `MapRevision`을 원 ticket과
대조한 뒤에만 `0x7E50`을 송신한다. 어느 identity라도 다르면 Read request를 보내지 않는다.
D4 Double bank의 PLC capability와 D5 PI Write 및 extended result는 capability-off다.
D4에는 exact owner/session/BootId/config/record/buffer identity, 두 frozen download의 SHA-256
불변성, third-Start Busy와 보존형 실패/cancel을 검증하는 UI 독립 orchestrator가 있다. 이 core
non-durable orchestrator는 exact unexpected-third handle이 반환된 경우 명시적 unexpected third ->
B -> A -> configuration release primitive도 제공하지만 durable WPF cleanup 계약과는 구분한다.
external-session-loss 뒤
`0x7E4A/0x7E4B` exact recovery, durable release intent/confirmed와 response-loss 재접속,
`0x7E4C/0x7E4D` token-qualified Configure-response-loss 복구도
PC 계약으로 구현했다. final Configuration Release 응답 유실은 nonzero exact identity의
`0x7E4A` canonical-empty detail 32를 typed absence로 받아 journal을 mutation 없이 resolve한다.
새 v3 journal의 `ClientTokenV1` marker가 있는 ConfigRevision=0은 4D로 실제 revision을 durable
확정한 뒤 4A를 다시 읽고 기존 release-only adoption으로 진행한다. wire binding 증거가 없는
legacy v2 ConfigRevision=0은 계속 zero-wire fail-closed다.
production WPF의 qualification adapter는 recovery Guid의 첫 4바이트 little-endian으로
결정적 nonzero RequestedConfigId를 만들고 Configure 전에 journal을 arm한다. 성공한 A/B
resource, operation scope, coordinator, connection과 diagnostics context는 같은 session cleanup을
위해 보존하며 자동 Release하지 않는다. same-session cleanup은 third Start가 exact
ResourceBusy였을 때만 허용한다. session/journal/identity/order preflight 뒤 사용자 checkbox를
소비하고 Status를 읽어 필요하면 Stop한 뒤 Ready/Uploading 상태에서 B -> A -> configuration
순서로 exact Release한다. 실패 뒤에는 checkbox를 다시 확인해야 한다. unexpected third success
또는 ambiguous outcome이면 같은 session Release는 모두 zero-wire이고 disconnect/reconnect 뒤
token-qualified exact inventory inspection만 허용한다. conflicting inventory는 external/manual
recovery로 남기며 자동 Release하지 않는다. confirmed-not-applied pending bank/configuration
intent는 동일 target의 exact intent만 재사용하고 새 intent/다른 target을 금지한다. retained
handle이 ACK-success면 Release wire replay 없이 durable confirm/resolve한다.
reconnect recovery는 일반 Catalog/global mutation
interlock과 분리된 recovery capability contract를 사용하고, ConfigRevision=0이면
`0x7E4D -> 0x7E4A`, 그 외에는 `0x7E4A`부터 시작한다. occupied bank는 exact `0x7E49`, empty
configuration은 `0x7E4B`로 채택하며 일부 성공한 Adopt handle도 즉시 보존해 나머지만 재개한다.
매 시도는 사용자 확인 시점의 journal/config/bank 집합을 immutable snapshot으로 고정한다.
4D/4A가 새 ConfigRevision 또는 snapshot에 없던 exact bank를 발견하면 local journal까지만 갱신하고
4B/49/Release는 0회로 중단한다. 갱신된 exact 계획을 표시한 뒤 checkbox를 다시 확인해야 한다.
WPF 시작 시 journal은 열지만 inventory/adopt/release를 자동 replay하지 않는다.
PLC build/RAM/jitter/live proof 전에는 Double capability와 네 WPF proof/route gate를 계속 끄고
수동 mode/모호한 Adopt·Configure를 zero-wire로 막는다. 이 adapter 구현은 PLC runtime 또는
pcap 증거가 아니다.
SDO Write는
`0x7E50`의 `OperationFlags=1`, exact 36-byte, Int32 4-byte request와
`OperationKind=SDOWrite(3)` parser/executor 및 C# API/WPF까지 구현했다. 현재 source는 PLC와
SDK의 global gate 및 UI[24] axis 1 gate만 `TRUE`이고 axis 2~4 gate는 `FALSE`다.
`AllowedSdoWrites`도 축 1 Gold UI[24] `0x2F00:24`, exact Int32 4-byte 한 건만 노출한다.
임의 SDO address, 축 2~4와 DS402 motion/control object는 계속 차단된다. 이 제한 승인은
source/test 설정이며 사용자 drive program에서 축 1 UI[24]가 실제로 미사용인지와 EtherCAT
mailbox 동작은 아직 실기 검증되지 않았다.
WPF는 PLC bit 9, SDK target allowlist, 선택 축의 `PowerOn=False`/`Standstill=True`, actual
    position 3회 안정, 명시적 확인과 operation-kind별 quarantine을 모두 통과해야만 Write를
    submit한다. 최초 same-value qualification은 baseline/pre-Write guard/Write/readback의 서로
    다른 4개 ticket을 사용하며, 그 PASS proof는 exact connection/session,
    `DiagnosticsBuild`/`BootId`/`MapRevision` 및 승인 target 전체 tuple에 묶인다. 일반 수동
    second-click은 proof-bound capability/target을 identity-pinned SDK submit에 전달한다. SDK는
    mutation gate 내부 fresh Build/BootId/MapRevision exact 비교가 성공한 경우만 `0x7E50`을
    만들며 mismatch는 `NotAttempted`/zero-wire다. mismatch/disconnect proof는 영구 폐기된다.
    Write handler는 이 proof가 없거나 stale이면 wire 전에 다시 거부한다. write outcome이
    불명확하면 read recovery로 자동 해제하지 않는다. 실제 제출 전에는
변경된 LASAL project의 IDE Rebuild/Link와 PLC download가 필요하고, reconnect 후 fresh
capability bit 9와 새 BootId/MapRevision을 확인해야 한다. 이 경로의 PLC/실축 검증은 아직 없다.
Phase 1 WPF의 PI Write는 SDK compile-time allowlist가 empty인 것에 더해
`Phase1AllowsPiWrite=false`가 입력/button을 비활성화하고 click handler도 다시 거부하는
이중 차단이다.
PI/Bulk compatibility facade는 `CreatePIBulkBuilder(catalog)`와 alias `ReadPI`로 구현했다.
builder는 catalog의 exact `MapRevision`, readable flag, 최대 32개와 중복을 검사하며,
`Upload` 뒤 `GetEntry/TryGetEntry`로 최신 snapshot을 조회한다. 별도 D6 wire를 만들지 않고
D1/D2 wire를 그대로 사용한다.
`GetSignalCatalog[Async]` 결과도 owner/session-bound aggregate다. alias PI Read, builder 생성과
기존 builder의 `Configure[Async]`, PI Write submit은 unbound, foreign, reconnect-stale Catalog를
capability/read/write RPC 전에 거부한다. `GetByAlias` 같은 로컬 Catalog 조회는 historical/static
snapshot에도 계속 사용할 수 있다.

Phase 1 read-only 확장은 `LMCConnection.Admin`과 `LMCSingleAxis` facade로 제공한다.
Admin capability `0x7D00`을 확인한 뒤 physical axis 1..4의 6개 semantic Int32 parameter를
`0x7D10`으로 읽고, group `0x0100`의 path velocity/acceleration/jerk-time을 `0x7D20`으로
읽는다. axis key 3의 정확한 이름은 `EndPositionToleranceWindow`이며 profile
in-position 상태와 다른 값이다. `GetDriveOperationMode[Async]`는 D5 SDO
`0x6061:0 Int8/1`을, `ReadDriveStatus[Async]`는 axis status -> `0x6041` -> `0x6061`을
순차 조회한다. 이 composite는 atomic same-cycle snapshot이 아니다. async cancellation은
PC의 ticket wait만 중단하며 제출된 PLC ticket을 자동 cancel하지 않는다. terminal poll
간격은 PLC가 광고한 `BaseCycleTimeUs`에서 계산하고 최대 poll 수는
`TimeoutCycles+32`다. 제출 뒤 취소는 ticket을 포함한
`LMCSdoReadWaitCanceledException`으로 보고한다. 이미 진행 중인 status RPC는 caller
token으로 transport를 끊지 않고 응답을 수신한 뒤 취소를 관찰하므로 connection과
ticket 재조회 가능 상태를 보존한다.

`LMCDriveStatus.HasDs402Fault`는 D5 SDO로 읽은 실제 `0x6041:0`의 bit 3을 표시한다.
`0x2028` 응답의 `StatusWord`는 current LASAL에서 reserved 0이므로 DS402 Fault 판정에
사용하지 않는다. 별도 `GetDriveErrorCode[Async]`는 `0x603F:0 UInt16/2`를 한 ticket으로
읽고 error code, `HasError`, ticket과 terminal status를 보존한다. 이 API는 기존
SDORead/GeneralInline capability와 BootId/MapRevision, physical slave 1..4 gate를 그대로
사용하며 새 opcode나 LASAL Network를 추가하지 않는다. Reset의 `AxisErrorId==0`, DS402
Fault bit 해제와 `0x603F==0`은 각각 확인해야 한다. 상세 경계는
[`DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md`](../../docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md)에 고정했다.

Phase 2 첫 motion facade는 `LMCGroupAxis.MoveLinearRelativeEx[Async]`다. Admin
`0x7D22`가 16-slot distance vector와 dynamics/options를 보내고 PLC는 현재 위치를 PC에서
합산하지 않은 채 `LMCRobot.MoveRelativeCoord`를 직접 호출한다. current 4-axis contract는
slot 5..16=0, Coordinate=None, transition ExactStop/ContinuousDirect, buffer
Aborting/Buffered, Execute=true만 허용한다. 성공 ACK는 profile queue 수락이며 실제 완료와
profile error는 기존 `GroupReadStatus` poll로 확인한다.
Phase 2 도입 당시 `0x7D00 FeatureBits=0x00000007`은 새 DLL과 PLC source를 함께
배포하는 계약이었다. current source는 LMC Home bit 4를 더한 `FeatureBits=0x00000017`이다.
이전 DLL은 새 feature를 unknown으로 strict reject할 수 있으므로 PLC만 먼저 내려받지 않는다.
bit 3 `AxisSetPosition`, bit 5 `AxisSetPositionOutcomeRead`, bit 6 `AxisDs402Home`은 OFF다.

Admin `0x7D12 SetAxisPosition`은 활성 API가 아니라 bounded coordinate-correction을 위한
dormant/fail-closed 계약이다. request는 56 bytes, response는 36 bytes이며 fresh
DiagnosticsBuild/BootId/MapRevision, process/session을 넘어 유일한 4 x U32 client intent,
target과 expected actual position의 CAS, prepare-time pinned RequestId와 atomic one-shot
execution을 사용한다. RequestId는 `LMCAdmin` instance마다 다시 시작하므로 단독으로
authoritative duplicate-suppression key가 아니다.
Detail 11 `NativeCommandRejected`는 common `INT16 ErrorId=-6`만 허용하고 payload `P+24`의
full `_LMCAXIS_CMDERROR` `UINT32` bitfield를 typed exception에 보존한다. positive/other
ErrorId 또는 applied/native 불변식 위반은 malformed response이므로 outcome은 uncertain이고
그 exact session을 fault시킨다. current LASAL은 valid raw request도 `InvalidState/detail 10`으로
반환하며 native SetPosition을 호출하지 않는다. SDK의 `0x7D14`는 journal에서 복원한 exact
key로 terminal outcome만 반복 조회하는 read-only API이고 SetPosition을 replay하지 않는다.
current PLC에는 two-bank retained store, query route, terminal retirement CAS가 없으므로 bit 5도
OFF다. 독립 durable journal core를 MainWindow에 arm하지 않으며, store/query/retirement,
journal/no-auto-replay와 unified axis/group mutation ownership을 같은 slice에서 연결하고
motion RT/task-core priority, application-approved `SetPositionMaxJump>0`, `IsReferenced` 정책과
PLC proof를 완료하기 전에는 capability를 켜지 않는다.
상세 계약은
[Axis SetPosition bounded coordinate correction](../../docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md)에 있다.

Admin `0x7D13`은 현재 `LMC_Home CurrentPositionZero` 시작 명령이다. 준비 시 읽은
actual position은 stale-read guard이고 target은 0으로 고정된다. 이 경로는 axis motion을
enable하거나 Home/limit switch를 찾지 않는다. Admin feature bit 4는 source에서 ON이며,
start ACK 뒤 exact recovery key로 `0x7D18 ReadAxisHomeOutcome`을 조회하고 terminal
record만 `0x7D19 RetireAxisHomeOutcome`으로 retire한다. 응답 손실이나 timeout 때
`0x7D13`을 자동 replay하지 않는다. WPF Single Axis 화면은 이 경로를 노출하고 exact
terminal outcome을 조회한 뒤 상세 `LMC Home outcome:` 로그를 남긴다.
성공 판정은 Standstill, AxisError/native/failure `0`, LASAL application/internal 좌표 6개 `0`,
fresh sample 3개, evidence `0x3F`와 raw feedback의 wrap-safe `-2/-1/0/+1/+2 count` 창을 함께
요구한다. raw before/after는 물리 feedback 증거이며 bit-identical sample을 요구하지 않는다.

별도 Admin `0x7D15/0x7D16/0x7D17`은 `LMC_HomeDS402` method 37, Home offset 0의
non-moving current-position-zero source 계약이다. 현재 `LMC_DIAG_DS402_HOME_ENABLED=FALSE`와
Admin feature bit 6 OFF가 실행을 차단한다. source 존재나 WPF control 존재를 runtime 사용
가능 증거로 해석하지 않는다. current source는 완전한 72-byte Start 형식에만 axis owner를
예약하고 `-2`를 detail 41, 그 밖의 admission 실패를 detail 42로 보존한다. malformed Start는
zero-token으로 Diagnostics parser에 위임한다. terminal success는 RT owner release 뒤 fresh
latch에서 ActualPosition 0, StatusWord fault/homing-error clear, 허용 DS402 base state와 모든
pending/uncertainty slot clear를 다시 확인한다. timeout 경계는 inclusive `>=`다. 다만 durable
DS402 owner-release/rollback-complete receipt와 bit-4 safety drain/tombstone이 남아 있으므로
gate를 열 수 없다. prepared stage `89`의 RESERVED/ACTIVE warm reconcile, generation slot `109`를
지우지 않는 split clear와 cleanup stage `90..99`의 1초 bounded quarantine는 source에 반영됐다.

Diagnostics `0x7E53/0x7E54/0x7E55` encoder-maintenance 경로는 TW[20]
`0x20FC:0x02 <- UInt16 1`과 TW[19] `0x20FC:0x01 <- UInt16 1`만 허용하고 source
capability bit 18/19를 광고한다. start ACK, terminal outcome, exact retirement는 각각
구분하며 drive error/warning 또는 multi-turn position의 실제 변화는 별도 실기 readback으로
확인한다.

위 Home/encoder-maintenance source는 최신 ownership receipt 수정 이후 C78 Rebuild/Download와
새 BootId의 한 축 단독 시험이 아직 남아 있다. 상세 구현과 시험 순서는
[LMC Home current-position-zero와 encoder maintenance IDE handoff](../../docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md)를 따른다.

`../LasalApiWpfTestApp`의 `Read-only API` 탭은 이 Phase 1 surface를 실물에서 확인하는
전용 UI다. Admin capability를 먼저 읽어야 axis/group parameter 버튼이 활성화되고,
physical axis 1..4의 operation mode와 non-atomic drive status도 같은 탭에서 확인한다.
이 화면은 read-only이며 LASAL IDE build/download와 live PLC 검증을 대신하지 않는다.
`SetAxisPosition`은 활성 조건을 충족하지 못해 WPF에 노출하지 않는다. LMC Home은
Single Axis 화면에 노출하지만 최신 PLC build의 한 축 terminal outcome 검증 전에는
production 완료로 판정하지 않는다.

`LMCErrorCatalog.TryDescribe`는 AdapterCommand, AdminDetail, DiagnosticsDetail,
GroupProfile domain을
명시한 project-local versioned description/resolution만 제공한다. Elmo Personality 전체
error database가 아니며 같은 숫자를 domain 없이 해석하면 안 된다.
새로
추가한 `0x2051`은 LASAL-DINT v1 전용 68-byte success response
(`DINT[16] + UINT16 status + INT16 error`)만 받으며 캡처의 PMAS legacy
136-byte LREAL response는 명시적으로 거부한다. 4-byte command-error
envelope는 오류 context를 보존한다. `0x20E7`은 캡처와 같은
1,320-byte payload를 만들되, 공개 호출은 캡처로 확인된 Cartesian 4축
X/Y/Z/U identity-shift와 `Buffered(2)` 조합으로 제한한다.

group source는 local extension인 `GroupPowerOn(0x204A)`/`GroupPowerOff(0x204B)`,
`GroupReset(0x2049)`, `GroupStop(0x2085)`,
`MoveLinearAbsoluteEx(0x20A4)`, `GroupReadActualPosition(0x2051)`,
`SetKinTransformCartesian4Axis(0x20E7)`과 Admin
`MoveLinearRelativeEx(0x7D22)`까지 활성화됐다. 적용 범위는 현재
4축 static identity 구성으로 제한된다. `MoveLinearAbsoluteEx`는 좌표계
`None(0)`, transition `ExactStop(0)`/`ContinuousDirect(2)`, buffer
`Aborting(1)`/`Buffered(2)`만 허용한다. `0x20E7`은 동적 kinematic model을
생성하지 않고 exact X/Y/Z/U identity 요청을 검증해 static axis-order mapping만
설정한다. profile lock/unlock은 각각 `GroupEnable(0x2047)`의 `LockProfile`과
`GroupDisable(0x2048)`의 `UnlockProfile`이 담당한다. GroupDisable은
`ProfileInPosition(_LMCPROF_ProfileFinished)`가 확인된 상태에서만 unlock한다.

current `0x2051` handler는 None/ACS만 허용하며, no-CalcModel static identity에서
두 이름 모두 member-slot read alias다. MCS/PCS는 C#에서 fail-fast하고 구 SDK로
요청하면 PLC가 `ErrorId=-7`로 거부한다. `_LMCPROF_POS`의 Pos1..Pos9는 DINT[16]
response slot 1..9에 복사되고 slot 10..16은 0이다. 이는 software group member
readback 계약이며 Move/SetKin/Lock의 4축 제한을 9축 group motion으로 확대하지
않는다. `09_Group_ReadPosition_None_ACS_2051`에는 `0x2051`이 없었지만 후속 `09b`
packet capture에서 None/ACS가 같은 static member-slot 순서와 값을 반환하는 것을
확인했다. 이는 true ACS transform 또는 MCS/PCS 지원 증거가 아니다.

정상 group 순서는 `GroupPowerOn -> GroupReadStatus.IsPowerOn -> identity axes
ReadStatus.IsReferenced -> SetKinTransform ->
GroupEnable/LockProfile -> motion -> GroupDisable/UnlockProfile -> GroupPowerOff ->
GroupReadStatus.IsPowerOn=false 확인`이다.
`GroupPowerOn`/`GroupPowerOff` ACK는 각각 비동기 `RobotOn`/`RobotOff` 요청 접수만
뜻하며 최종 완료가 아니다. `GroupReadStatusResult`의 `0x00040000=IsPowerOn`만
LASAL project-local Power Ready 확장이다. `0x00020000`은 Maestro 표준
`NC_GROUP_STANDBY_MASK`, `0x00010000`은 표준 `NC_GROUP_DISABLED_MASK`이며,
현재 어댑터는 각각 locked standby(`IsStandby/IsEnabled`)와 unlocked
disabled(`IsDisabled`) 조건에서 이 표준 mask를 설정한다.

public `BeginGroupPowerOnWaitForStableStateAsync`와
`BeginGroupPowerOffWaitForStableStateAsync`는 각각 `0x204A` 또는 `0x204B`를 정확히 한 번
dispatch한다. success ACK와 `LMCGroupPowerStateWaitContinuation`은 같은
connection/session/group-reference의 session-bound send-priority publication 안에서 원자적으로
설치되며 Begin은 `0x2045`를 보내지 않는다. accepted observer overload는 이 publication 뒤 첫
status보다 먼저 continuation을 durable recovery 계층에 넘긴다.

`ResumeGroupPowerStateWaitForStableStateAsync`는 exact pending continuation으로 `0x2045`만
poll하여 기대 `IsPowerOn`을 기본 3회 연속 확인하고 power command를 replay하지 않는다.
`GroupPowerOnAndWaitForStableStateAsync`와 `GroupPowerOffAndWaitForStableStateAsync`는 Begin과
Resume을 같은 elapsed total deadline으로 조합하는 convenience facade다. status gate 대기, wire
exchange와 poll delay가 deadline에 포함되며 각 Resume epoch는 stable count를 다시 0에서 시작한다.
accepted timeout/cancel/status failure는 continuation, ACK, 마지막 status, poll/stable count와
mutation generation을 immutable evidence로 보존한다. stale/resolved/concurrent Resume과 이미
pending인 방향의 fresh Begin은 typed zero-wire failure이고, later same-group mutation은
`LMCGroupPowerInterferenceException`으로 원 power transition 귀속을 거부한다. result/typed
exception의 submission outcome은 `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`를
분리한다. post-write deadline으로 transport가 불명확하면 connection은 `Faulted`로 격리된다.

public `WaitForPowerStateAsync(expectedPowerOn, ...)`는 continuation을 소비하지 않는 read-only
`0x2045` helper다. reconnect/restart의 exact-identity recovery에는 사용할 수 있지만, 성공 결과도
원 ACK를 재사용했다는 뜻은 아니며 outcome-uncertain Power On을 status만으로 안전하게 해제하지
않는다. 이 SDK 계약은 deterministic PC/fake-RPC 증거이고 PLC의 `RobotOn`/`RobotOff` 완료 시간이나
실제 EtherCAT/drive 상태 증거가 아니다.

public `BeginGroupStopWaitForStableStandbyAsync`는 `0x2085`를 정확히 한 번 dispatch하고 success
ACK를 connection/session/group/latest-pending에 묶인 `LMCGroupStopWaitContinuation`으로 반환한다.
Begin은 `0x2045`를 보내지 않는다. `ResumeGroupStopWaitForStableStandbyAsync`는 exact continuation으로
`0x2045`만 poll하여 `IsStandby`를 기본 3회 연속 확인한다. timeout/cancel/status failure와
send-priority preemption 뒤에도 accepted continuation과 immutable evidence를 보존하고 Stop을
replay하지 않는다. `TransportInvalidatedAtDeadline=true`이면 owner session은 faulted라 Resume할 수
없으며 reconnect 뒤에도 Stop을 자동 replay하지 않는다. stale, superseded, completed continuation과
concurrent second Resume은 zero-wire로 거부된다. 새 accepted Begin은 이전 pending을 supersede하며, 기존
`GroupStopAndWaitForStableStandbyAsync`는 Begin과 Resume을 같은 elapsed total deadline으로 조합한다.
Stop ACK는 호출 수락일 뿐 완료 증거가 아니고, result/typed exception은 `NotAttempted`, `Rejected`,
`OutcomeUncertain`, `Accepted` submission outcome, ACK, 마지막 status, poll/stable count를 분리한다.
이는 PC-side orchestration 계약이며 실제 PLC 정지 시간이나 장비 안전 성능의 증거가 아니다.

public `PowerOnAndWaitForStableStateAsync`는 Axis Power On `0x2023(enable=true)`를 정확히
한 번만 dispatch하고 success ACK를 same connection/session/axis의
`LMCAxisPowerOnWaitContinuation`으로 보존한 뒤 `0x2028`의 `PowerOn=true`를 기본 3회 연속
확인한다. total deadline은 mutation/status gate 대기, ACK/status exchange와 poll delay를 모두
포함한다. 최종 write 경계 전 취소는 `NotAttempted`/zero-wire이고 connection을 재사용할 수 있다.
write 뒤 사용자 취소는 ACK를 drain하고 continuation을 설치한 다음 accepted observer를 먼저
호출해 durable evidence를 남기며, 같은 continuation의
`ResumePowerOnWaitForStableStateAsync`는 `0x2028`만 보내고 `0x2023`을 replay하지 않는다.
ACK 또는 status 무응답이 deadline을 넘으면 connection을 `Faulted`로 전환하고
`TransportInvalidatedAtDeadline`을 남긴다. public Axis `WaitForPowerStateAsync`는 Power 명령을
보내지 않는 순수 status-only helper다. 따라서 read-only 성공 결과도
`SubmissionOutcome=NotAttempted`, ACK/continuation 없음, `ReusedAcceptedAcknowledgement=false`다.

public `BeginResetWaitForStableErrorClearanceAsync`는 Axis Reset `0x2024`를 정확히 한 번
dispatch하고 status를 읽지 않는다. success ACK와 latest pending continuation은 exact
connection session/send-priority publication 안에서 원자적으로 설치된다.
`ResumeResetWaitForStableErrorClearanceAsync`는 `0x2028`만 poll해 successful status의
`AxisErrorId == 0`을 기본 3회 연속 확인한다. Resume epoch는 stable count를 다시 0에서 시작하지만
poll count와 마지막 status는 누적한다. `ResetAndWaitForStableErrorClearanceAsync`는 두 phase를 한
total elapsed deadline으로 조합한다. rejected ACK와 invalid/stale/superseded/completed/concurrent
Resume은 status zero-wire다. timeout/cancel/status/response-loss는 immutable
submission/ACK/마지막 status/poll/mutation-generation evidence와 pending continuation을 보존하며
Reset을 replay하지 않는다. ACK/status 무응답이 deadline을 넘으면 connection을 `Faulted`로
전환하고 `TransportInvalidatedAtDeadline` evidence를 남긴다.

Reset Resume은 status 전송 전, status publication과 final resolution에서 original Reset
mutation generation을 확인한다. later same-axis `LMCSingleAxis` mutation은
`LMCAxisResetInterferenceException`으로 귀속을 거부하며, intentional post-Reset Power On 뒤에도
명시적 새 Reset 전에는 원 continuation으로 완료를 주장하지 않는다. final proof가 먼저 commit된
뒤의 cancel/deadline은 성공을 뒤집지 않고, 먼저 관찰된 cancel/deadline은 pending을 유지한다. 이
proof는 LASAL AxisErrorId-clear 관찰이며 DS402 Fault bit 또는 `0x603F` 해제 증거가 아니다. 상세
경계는
[`AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md`](../../docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md)에 기록했다.

public `BeginStopWaitForStableStandstillAsync`는 `deceleration > 0`, `jerk >= 0`인 Axis Stop
`0x2022`를 정확히 한 번만 dispatch하고 success ACK를
`LMCAxisStopWaitContinuation`으로 보존한다. Begin은 `0x2028`이나 status gate를 사용하지 않고,
mutation gate를 ACK publication과 pending 설치까지 유지한다. 새 accepted Stop은 이전 pending
continuation을 supersede한다. `ResumeStopWaitForStableStandstillAsync`는 exact
connection/session/axis/latest-pending identity를 확인하고 `0x2028`만 poll해
`IsSuccess && IsStandstill`을 기본 3회 연속 확인한다. 각 Resume은 Stop proof와 관찰 대상 pending
Power On의 PowerOff/Standstill proof를 fresh reset하며 timeout/cancel/status-fail/preemption 뒤에도
분리된 Resume epoch의 sample을 합치지 않는다. rejected ACK는 status zero-wire이고 typed evidence는
`NotAttempted/Rejected/OutcomeUncertain/Accepted`, command-may-have-been-sent, ACK, 마지막 status,
poll/stable count, expected/observed mutation generation, elapsed와
`TransportInvalidatedAtDeadline`을 보존한다. 어느 실패 경로도
`0x2022`를 자동 replay하지 않는다. 기존 `StopAndWaitForStableStandstillAsync`는 Begin+Resume을
한 total deadline으로 조합한다. WPF Stop 버튼은 Begin을 priority safety-send phase에서 실행하고
Resume을 preemptible monitor phase에서 실행하므로 확인 중에도 더 새 Stop/Power Off가 다음 safety
generation을 예약할 수 있다. Resume은 status 전송 전, status publication과 final resolution에서
original Stop mutation generation을 확인한다. later same-axis mutation이면
`LMCAxisStopInterferenceException`을 반환하고 pending을 유지하며 Stop을 replay하지 않는다.
zero-wire mutation과 다른 AxisReference는 간섭하지 않는다. pending Power On proof를 관찰해도
자동 해제하지 않는다. 상세 경계는
[`AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md`](../../docs/architecture/AXIS_STOP_STABLE_STANDSTILL_2026-07-29.md)에 기록했다.

Axis raw/accepted-wait mutation은 structurally valid NACK가 확인되면 exact latest mutation
reservation만 rollback한다. accepted ACK 또는 post-write outcome uncertainty는 generation을
유지한다. 따라서 active Reset 뒤 Stop이 NACK이면 기존 Reset continuation의 status-only proof를
계속할 수 있지만, Stop 결과가 불명확하면 Reset proof로 되돌아가지 않는다.

`LMCConnection.AbortTransportForSafetyPreemption(expectedSessionGeneration)`은 held RPC response 뒤에
안전 명령이 막히는 경우를 위한 production transport escape hatch다. expected session과 현재
published client session을 원자적으로 비교하고, 일치할 때만 zero-time linger를 best-effort로 적용해
local socket을 detach/close한다. RPC Close와 안전 명령 자체는 보내지 않는다. normal Close가
lifecycle lock을 보유한 경우에도 그 lock 뒤에서 기다리지 않는다. mismatch는
`LMCSafetyPreemptionSessionMismatchException`이며 어떤 transport도 끊지 않는다. caller는 evidence의
`TransportDetached`, `FaultStatePublished`와 session을 확인하고, fresh connection/object identity에서
안전 명령을 정확히 한 번 별도로 보내야 한다. Open은 새 session을 old transport close 전에 reserve하고
published client/lifetime/session을 한 임계구역에서 묶으므로 reconnect의 pre-publish 경계도 old-session
abort와 혼동하지 않는다.

위 PowerOn/Stop/Reset/PowerOff 귀속용 generation은 connection session + `AxisReference`에 묶인 process-local
coordinator다. `LMCSingleAxis` raw sync/async Power On/Off, Reset, Stop, Move
Absolute/Relative/Velocity와 accepted-wait write가 may-have-been-sent boundary에 도달할 때만
증가한다. validation/cancel/preemption으로 zero-wire인 호출은 증가시키지 않고 다른
AxisReference도 간섭하지 않는다. 외부 PLC logic, 다른 RPC client, direct SDO write와 group
operation은 이 귀속 범위 밖이다.

public `BeginPowerOffWaitForStableStateAsync`는 Axis Power `0x2023(enable=false)`를 정확히
한 번만 dispatch하고 success ACK를 same connection/session/axis의
`LMCAxisPowerOffWaitContinuation`으로 보존한다. 이 Begin phase는 status-observation gate를
잡거나 `0x2028`을 보내지 않으며 mutation gate는 ACK, PowerOff mutation generation과 pending
continuation의 session/send-priority atomic publication이 끝날 때까지 유지해 concurrent Begin의
wire/게시 순서를 일치시킨다. accepted observer overload는 이 원자 publication 뒤 mutation gate를
해제한 상태에서 첫 status보다 먼저 호출된다. observer가 예외를 내도 exact pending continuation은
보존되며 observer 내부의 same-axis mutation/중첩 Begin/Resume은 zero-wire로 거부된다.
`ResumePowerOffWaitForStableStateAsync`는 continuation을
검증하고 `0x2028`만 보내 `IsSuccess &&`
`PowerOn=false && Standstill=true`를 기본 3회 연속 확인한다. typed evidence는
`NotAttempted/Rejected/OutcomeUncertain/Accepted`, ACK, 마지막 status와 poll/stable count를
분리하고 실패 뒤 PowerOff를 자동 replay하지 않는다. 각 Resume은 exact pending Power On
continuation의 PowerOff/Standstill proof를 fresh reset하며 timeout/cancel/status-fail/preemption
경계에서도 다시 reset해 분리된 Resume epoch의 샘플을 합치지 않는다. evidence는
`PowerOffMutationGeneration`, `ObservedMutationGeneration`과 `InterveningMutationDetected`도
보존한다. Resume은 status wire 전, publication과 final resolution에서 원 generation을 확인한다.
later same-axis mutation은 `LMCAxisPowerOffInterferenceException`으로 끝나고 pending을 유지하며
PowerOff를 replay하지 않는다. final proof보다 먼저 관찰된 cancel/deadline/generation change는
pending을 보존하고 proof commit 뒤 late cancel/deadline은 성공을 뒤집지 않는다. 외부
PLC/client/direct SDO/group mutation은 process-local 귀속 범위 밖이다.
기존 `PowerOffAndWaitForStableStateAsync`는 Begin+Resume을 한 total deadline으로 조합한다.
Begin ACK 또는 Resume status가 write 뒤 deadline을 넘으면 connection을 `Faulted`로 전환하고
`TransportInvalidatedAtDeadline`을 보존한다. accepted Resume continuation은 evidence로 남지만
faulted session에 묶여 reconnect 뒤 재사용할 수 없으며 `0x2023`을 자동 replay하지 않는다.
WPF는 Begin을 priority send/command-gate phase에, Resume을 preemptible monitor phase에 배치한다.
일반 timeout/cancel/status failure 뒤 Power Off 재클릭은 exact continuation의 status-only Resume이고,
확인 중 재클릭은 zero-wire다. typed interference가 확인된 경우에만 `Power Off Again (Confirmed
Interference)`로 replacement `0x2023` 1회를 허용하며 reject면 기존 pending/flag를 보존한다. Stop은
명시적 newer safety로 계속 사용할 수 있다. 상세 경계는
[`AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md`](../../docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md)에 기록했다.
Axis PowerOff 전용 fake-RPC contract는 35개다.
Stop request가 actual RPC write boundary에 도달하면 같은 group coordinator의 pending Enable
proof를 reset하고 per-group mutation generation을 고정한다. 이후 다른 group mutation이 actual
write boundary에 도달하면 `LMCGroupStopInterferenceException`으로 원 Stop의 stable proof 귀속을
거부한다. 마지막 status publication은 generation, early cancel/deadline과 stable proof를 coordinator
lock 안에서 한 번에 결정한다. proof 뒤 late cancel/deadline은 성공을 뒤집지 않으며 pre-canceled
Resume도 accepted evidence/continuation을 가진 typed cancellation으로 끝난다. ACK와 각 status의
최종 publication은 원 connection session에 bind되어 Close/reconnect와 경합한 stale success도
반환하지 않는다. generic Group Reset, Axis Reset, Admin
`GroupMoveLinearRelative`와 D5 `SubmitSdo`/`CancelOperation`은 parse 뒤
exact session/generation publication을 거치므로 지연된 ACK가 새 safety 예약 뒤 도착하면 drain 후
`ResultDiscarded`로 끝난다. accepted `SubmitSdo`는 exact ticket/BootId/MapRevision과 immutable
request를 failure context로 보존하고, `CancelOperation`은 실행됐을 수 있는 ACK를 stale success로
  관찰하지 않는다. topology/CREVIS `0x7E13`, `0x7E22`, `0x7E23`도 같은 publication 계약을
  사용한다. Recorder Trigger/Stop뿐 아니라 Configure `0x7E40`, recoverable Configure `0x7E4C`,
  Start `0x7E41`, exact/active Adopt `0x7E49`, empty-configuration Adopt `0x7E4B`의 typed 결과도
  publication 전에 선점되면 원 `LMCSendPreemptedException`에
  `LMCRecorderAcceptedResultFailureContext`를 붙인다. 정확한 handle/identity/lease와 BootId,
  MapRevision, Config/Record/Buffer/owner identity를 보존하며 accepted 객체는 recovery-only라
  Status/Stop/Release cleanup 외 정상 운전에 사용할 수 없다. Start의 source configuration도 함께
  격리한다. buffer/configuration/recovered/adopted identity Release는 지연 ACK 선점 시 각 handle을
  `OutcomeUnverified`로 격리해 재사용과 destructive retry를 차단한다. wire 전 선점은 release state를
  되돌려 안전한 재시도를 허용한다. WPF 일반 Group Stop과 qualification은 safety generation을
  gate 대기 전에 예약하고 Begin만 priority scope/command gate 안에서 수행한다. accepted continuation과
  recovery evidence를 gate 반환 전에 보존한 뒤 Resume은 preemptible status-only monitor에서 수행한다.
  accepted Resume 실패의 cleanup은 exact pending continuation만 재사용하며 새 `0x2085`를 자동
  전송하지 않는다. fake-RPC는 외부 Power Off 선점에서 Stop 1회/Power Off 1회/status 4회, accepted
  status failure 뒤 cleanup에서 Stop 1회/status 4회를 확인했다. 이는 PLC packet 또는 정지 성능
  proof가 아니다. current SDK Release runner는 callback-v2 회귀를 포함해
  1111/1111 PASS했다. 이번 tranche에서는 Debug runner를 다시 실행하지 않았고,
  이 값을 PLC runtime proof로 확대하지 않는다.

public `GroupEnableAndWaitForLockedStandbyAsync`는 동일 connection session과 group
reference별 coordinator에서 `0x2047` ACK를 한 번만 허용하고 `0x2045`의 PowerOn + Locked
Standby를 기본 3회 연속 확인한다. mutation/status gate 대기, `0x2047`, 모든 `0x2045`와 poll
delay는 하나의 total deadline을 공유한다. final write commit 전 취소/deadline은
`NotAttempted`, zero wire, mutation/proof 불변이며 connection을 재사용한다. actual write commit의
`onWriteCommitted`에서만 mutation generation을 갱신하고 pending proof를 0으로 reset한다. caller cancel이 write 뒤 발생하면 response를
drain하고 accepted ACK/status를 먼저 게시한 뒤 typed cancellation을 반환하므로 connection을
재사용한다. ACK 무응답 deadline은 `OutcomeUncertain`, continuation 없음, connection `Faulted`이고,
accepted 뒤 status 무응답은 `Accepted`, exact pending continuation, connection `Faulted`다. `0x2047`
ACK 수신 전과 accepted 뒤의 두 no-response 경우 모두 `TransportInvalidatedAtDeadline=true`다. rejected ACK는
`Rejected`이며 continuation이 없고 accepted observer도 호출하지 않는다.
`BeginGroupEnableWaitForLockedStandbyAsync`와 observer overload는 accepted ACK와 exact pending
continuation을 먼저 게시한 뒤 observer를 정확히 한 번 호출하며 helper-owned 첫 `0x2045`보다 앞선다.
observer가 예외를 던져도 원 예외를 그대로 전달하고 continuation은 pending으로 보존하므로
`ResumeGroupEnableWaitForLockedStandbyAsync`에서 새 `0x2047` 없이 이어서 확인할 수 있다.
timeout/cancel/status 실패의 accepted continuation은
`ResumeGroupEnableWaitForLockedStandbyAsync`로 status-only 재개한다. 같은 group reference의
다른 `LMCGroupAxis` handle도 pending/in-progress/status proof를 공유한다. 수동
`GroupReadStatus` 한 번만으로 continuation을 완료하지는 않지만 safety generation 검증을 통과한
성공 응답은 상태에 맞는 pending continuation proof에 누적된다. Locked Standby proof가 3/3이면
기존 ACK를 재사용한 zero-wire Resume으로 완료할 수 있다. 새 safety 예약은 이 proof를 0으로 초기화하되 accepted ACK와
continuation을 보존한다. 예약 뒤 도착한 응답은 drain 후 `ResultDiscarded`되어 observe되지 않는다.
SDK completion publication이 먼저 끝난 뒤 WPF 적용 전에 safety가 예약된 좁은 경우만
recovery-required로 승격한다. connected unresolved 상태에서는 group 이름 변경, group 재조회,
clean connection/window close, connected reconnect와 새 Power On을 차단한다. 외부 connection loss
뒤 reconnect 진입에서는 원 exact group 이름을 보존한 recovery로 승격하고 새 session에서 같은
이름의 group만 다시 조회한다. 명시적 `0x2048 GroupDisable` ACK는 Unlock 요청 접수만 뜻하며
pending/recovery를 해제하지 않는다. accepted pending과 recovery-required는 exact group identity에서
PowerOn=True + Disabled/Unlocked 3회 연속 또는 PowerOn=False 3회 연속 proof가 끝난 뒤에만
해제한다. Power On 성공만으로는 해제되지 않고
어느 경로도 `0x2047`을 replay하지 않는다. legacy `GroupEnable[Async]`도
pending/in-progress/recovery-required 중에는 wire 전에 거부한다.
재시작 또는 새 SDK connection처럼 process-local continuation이 없는 경우 public
`WaitForLockedStandbyAsync`는 `0x2045`만 전송해 PowerOn + Locked Standby를 기본 3회 연속
확인한다. mismatch는 stable count를 0으로 되돌리고 timeout/cancel/status 실패는 last status,
poll count, stable count, transport invalidation을 typed evidence로 보존한다. 이 API는
`0x2047`을 전송하지 않는다. Group Enable 전용 40개 회귀는 위 deadline/evidence를
fake-RPC로 고정하며 PLC runtime proof는 아니다. Group Disable은
`BeginGroupDisableWaitForStableDisabledAsync`가 `0x2048`을 한 번만 보내 accepted observer에
exact continuation을 전달하고, same-session Resume 또는 cross-session
`WaitForStableDisabledAsync`가 `0x2045`만 보내 PowerOn + Disabled + !Standby를 기본 3회
연속 확인한다. stable PowerOff는 더 새로운 safety mutation으로 pending Disable을
`SupersededByStablePowerOff`로 retire할 수 있지만 Disable 완료로 보고하지 않는다. ACK 뒤 첫
status 전에 child process를 Kill한 WPF 회귀는 새 session에서 `0x2048` 0회, `0x2045` 3회,
journal lock 재획득과 동일 identity `Resolved`를 확인했다. 이는 PC/fake-RPC 증거이며 PLC
profile unlock 또는 hardware proof가 아니다.

live capture에서는 `LockProfile`이 수락됐는데도 `0x2047` handler가 같은 CyWork의 stale
LockState를 읽어 `ErrorId=-6`을 반환했다. PC에서 성공으로 바꾸지 않는다. PLC ACK를
accepted-then-poll로 수정하고 최종 lock은 `0x2045`로 확인해야 한다.

다만 전체 장비 API 완료가 아니다. 2026-07-23 capture에서 Admin
`0x7D00/0x7D10/0x7D20/0x7D22`, 대표 absolute/relative group, Stop/PowerOff,
`09b` None/ACS static alias, D1/D2와 D5 1/2/4-byte happy path는 PASS했다.
`0x2047` ACK timing, true Buffered/stop-first, D1/D2 soak/fault, D3/D4와 D5 나머지
fault matrix가 남아 있다.
callback library default는 캡처 기반 legacy raw `12/4`를 유지한다. 명시적
`Version2WakeHint` opt-in은 project-local `32/20` registration과 52-byte `LMC2`
datagram을 사용하고, source/BootId/session/cookie/length/policy/sequence fence를 통과한
typed non-authoritative wake만 전달한다. EventType 1은
`DiagnosticsOperationTerminalAvailable`, EventId는 exact nonzero D5 TicketId다. UDP로
ticket이나 terminal state를 만들지 않는다. 이미 Submit 응답으로 보유한 current-session
ticket의 connection/session/BootId/TicketId가 모두 일치할 때만 generation-pinned
`GetOperationStatusAsync` (`0x7E03`)를 실행하며, 오직 TCP 응답만 상태를 갱신한다. tracked
LASAL handler는 `CurrentPeerValid`, exact TCP-peer IPv4와 port `1..65535`를 모두
확인한다. production PLC `PublishEvent` caller와 live callback packet 증거는 아직 없다.
다중 PC의 읽기 공유·motion owner 정책은 LASAL session/ownership 계층에서
구현해야 한다.

기존 motion/group 25-command 전체와 D3/D4 전체 packet matrix는 아직 수행하지 않았다.
WPF example의 live command gate, 작은 기본값, 물리 E-stop과
`../LasalApiWpfTestApp/README.md`의 순서를 지켜 단계별로 검증한다.

다음 구현/시험 순서는
[SIGMATEK runtime qualification 및 Test UI 설계](../../docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md)를
따른다.

tracked `TCPMotionInterface.Response()`는 최대 1,328-byte frame을 2,048-byte
receive accumulator에서 조립하고 payload 최대 1,320 bytes를 depth-8 queue에
복사한다. non-RT `CyWork()` 하나가 `MsgPaser()`와 위 18개 legacy
control/read/motion command, `0x7D22` Admin motion, source-active Home
`0x7D13/0x7D18/0x7D19`, dormant SetPosition `0x7D12`와 gate-off DS402 Home
`0x7D15/0x7D16/0x7D17` route를
실행합니다. interface RT task, RtWork mailbox와 atomic state는 사용하지
않습니다. editable `TCPIPServer1 : TCPIPServer`와 interface의 동일 cyclic task,
axis RT thread와 같은 core 배치, PLC jitter를 확인하기 전까지 production-safe로
판정하지 않습니다. 서버의 두 번째 connection slot은 same-peer reconnect candidate
비교 전용이며 RPC owner는 하나입니다.

상세 command matrix, 우선순위와 완료 조건:

- `docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md`

## 특징

- API 입력 단위: LASAL PLC가 받을 internal DINT
- API 내부 변환: 없음
- 단위 변환: 사용자 프로그램에서 직접 수행
- `LMC_Units`는 `unit.h` 기반 상수 선언만 제공
- 호출자는 물리값에 해당 `LMC_Units`를 곱하고 DINT 범위를 검사한 뒤 API 호출
- API 내부 코드는 `LMC_Units`를 참조하지 않음
- 현재 축의 PC application unit은 `1 mm = 10000 DINT`이며
  encoder `ExUnits=8388608`을 PC UNIT으로 사용하지 않음
- 이미 변환된 raw DINT는 추가 UNIT 변환 없이 그대로 전달할 수 있음
- LASAL PLC는 수신한 DINT를 변환 없이 `_LMCAxis` 또는 `_LMCRobot`에 전달
- `RpcInitConnection`은 TCP 연결 후 캡처 기반 RPC handshake(`0x8080`, `0x405C`)를 수행
- `RpcInitConnection`은 `0x405C` 전송 전에 callback listener를 연다. library 기본
  `LegacyRaw`는 12-byte request/4-byte ACK와 raw event를 그대로 유지한다. 명시적
  `Version2WakeHint`는 exact 32-byte request/20-byte response를 사용하고 typed wake만
  전달한다. LASAL은 callback IPv4가 current valid TCP peer와 exact match하고 port가
  `1..65535`일 때만 최초 tuple을 validate-then-commit한다. exact duplicate는
  idempotent, mismatch retry는 이전 tuple을 보존한 채 실패한다.
- `LMCCallbackEventArgs`는 legacy raw provenance를 제공한다.
  `LMCCallbackWakeHintEventArgs`는 version-2 typed wake provenance와
  `MatchesD5OperationTerminalTicket`을 제공한다. 이 matcher는 retained ticket과 exact
  current connection/session/DiagnosticsBootId/TicketId만 상관시키며 ticket을 합성하지
  않는다.
- `CloseConnection`/`Dispose`는 캡처 기반 close frame(`0x405D`)을 송신
- 연결 timeout, state event, 초기화/transport/close 오류 분리, callback
  source-address 검증과 취소 가능한 async API를 제공
- timeout/전송 오류와 in-flight 취소는 오염된 transport를 폐기하고
  `Faulted`로 전환하며, queue 대기 중 취소는 active request를 건드리지 않음
- 명시적 Close/reconnect는 connection lifetime generation을 갱신한다. 이전 transport의 늦은
  fault/cancellation cleanup은 같은 generation의 client metadata와 callback listener만 정리하므로
  명시적 `Disconnected` 또는 새 session을 뒤늦게 `Faulted`로 덮거나 지우지 않는다.
- 같은 `LMCConnection`의 `ConnectionStateChanged` handler에서 Init/Close/Dispose를 직접
  재호출하거나 handler가 아직 반환하지 않은 상태에서 `Task.Run`으로 넘기는 것은 sync/async
  모두 즉시 `InvalidOperationException`으로 거부한다. handler가 반환한 뒤 별도 흐름에서
  수행해야 한다. `CallbackReceived` handler의 Close/Dispose는 기존처럼 허용한다.
- 설치된 SIGMATEK `GetBroadCastData.st`의 `OS_TCP_USER_TOIP` 호출은 IPv4 UDINT를
  LSB, SHR 8, SHR 16, SHR 24 순으로 octet에 복원한다. 이는 request IPv4 UDINT와
  `OS_TCP_USER_GETPEERIP` 비교의 정적 byte-order 근거지만 target PLC runtime proof는 아니다.
- diagnostics의 configuration/resource/ticket 상태 변경 async API는 token을
  전송 시작 전까지만 적용한다. 전송 뒤에는 handle/ticket/최종 결과를 잃지 않도록
  같은 RPC 완료를 기다리며, 이 구간의 token 취소는 PLC Stop 명령이 아님
- `LMCConnectionOptions.SendPriorityCoordinator`는 선택 항목이다. 경쟁하는 connection에
  같은 `LMCSendPriorityCoordinator`를 명시적으로 주입하고 호출 흐름을 scope로 감싼 경우에만
  Stop/Power Off 우선순위 계약이 적용되며, 기본 `null` 또는 unscoped SDK 호출은 기존 전송
  동작을 유지한다. 이 WPF의 priority 요청은 application gate 대기 전에 generation을 선예약한다.
  `BeginPriorityScope`는 호출 시점의 최신 완료 reservation만 받으며 scope는 생성한 logical
  async flow에서 LIFO 순서로 해제해야 한다. scope 생성 뒤 더 최신 reservation이 생기면 기존
  priority sender도 write 직전 stale로 거부된다.
  `ExchangeCore`는 SDK compound helper의 후속 RPC를 포함해 각 command를 실제
  `stream.Write`하기 직전에 captured generation을 다시 검사한다. 더 새 priority 예약이 있으면
  아직 쓰지 않은 ordinary RPC는 `LMCSendPreemptedException`으로 거부되어 해당 command의 wire
  byte가 0이며, 이미 이 최종 검사를 통과해 in-flight가 된 RPC는 transport를 취소하지 않고
  결과/timeout까지 완료한다. SDO 또는 Digital Output submit이 이 경계에서 선점되면 각 SDK
  failure context는 `Phase=Submission`, `SubmissionOutcome=NotAttempted`, ticket 없음으로 남는다. 이 계약은
  deterministic fake-TCP PC 증거이며 PLC 실행 순서나 장비 안전 인증 증거가 아니다.
- reconnect 후 이전 session에서 만든 axis/group object는 stale handle로 거부
- `LMCConnection.Diagnostics`에서 `0x7E00` capability를 sync/async로 조회하며,
  stateful bit가 켜졌는데 `DiagnosticsBootId=0`이면 malformed contract로 거부
- 취소 가능한 name lookup은 `LMCSingleAxis.CreateAsync`와
  `LMCGroupAxis.CreateAsync`를 사용하며 generation 검증과 request 전송을
  같은 exchange gate에서 확인
- `LMCSingleAxis.LookupResult`/`LMCGroupAxis.LookupResult`는 target kind, object name,
  nonzero reference와 exact successful response를 보존한다. 실패는 기존
  `InvalidOperationException` catch와 호환되는 `LMCLookupException`으로 parsed response,
  payload/reference 유무와 defensive-copy raw bytes를 제공한다.
- `LMCSingleAxis`/`LMCGroupAxis` object는 name lookup으로 얻은 reference를 보관하고, 이후 motion/status API 호출 시 해당 reference를 패킷에 자동 삽입
- DLL은 `_LMCAxis1` 같은 PLC object name을 하드코딩하지 않음
- LASAL이 연결된 실제 object name을 읽어 opaque descriptor를 발급하고, 이후 descriptor로 axis client를 dispatch
- read API는 명령별 typed result를 제공하며 malformed response를 정상값 `0`과 구분
- WPF example은 네트워크/polling을 비동기로 실행하고 callback version 2를 명시적으로
  선택한다. 알려진 current D5 ticket wake만 single-flight `0x7E03` refresh로 합치며,
  unknown/stale/busy wake는 버리고 기존 manual/poll fallback을 유지한다. 또한 확인창 없는
  즉시 명령, MoveVelocity stop 추적과 group API/options를 제공한다. in-flight Cancel은
  transport를 중단해 연결을 `Faulted`로 만들 수 있고
  PLC Stop을 보내지 않으므로, 안전 관련 command/rollback 중에는 Cancel을 차단
- 공개 API는 한 기능당 하나만 둡니다. `LMC_*Cmd`와 같은 중복 메소드 alias는 제공하지 않습니다.

## 폴더

- `src/bin/Release/LasalMotionControlLib.dll`: 현재 내부 개발 빌드 산출물
- `../LMC_API_Distribution`: 고객 배포 시점에만 검증된 DLL/문서/예제를 복사하는
  별도 산출물 영역. 내부 개발 build와 자동 미러링하지 않음. 현재 canonical은 Axis1
  SDO Write 이전 gate-off snapshot이며 `LMC_API/Build-LmcApiDistribution.ps1`도 이를
  in-place로 수정하지 않는다. sibling staging의 전체 build/semantic/manifest gate가 PASS한
  경우에만 별도 candidate를 publish한다. 2026-07-31 actual run은 stale DOCX/PDF를
  `MANUAL_SDO_WRITE_SCOPE`로 차단해 candidate를 만들지 않았다
- `src/`: DLL 전체 C# 소스
- `sample/BasicUsage.cs`: RPC 연결, raw callback, caller-side UNIT 변환과
  단일축 motion 호출 전 안전 확인 구조 예제
- `docs/USER_MANUAL_PREPARATION_2026-07-13.md`: 배포용 사용자 매뉴얼의
  범위, 목차, 예제와 출판 전 검증 gate
- `tests/LasalMotionControlLib.Tests/`: NuGet 없는 .NET Framework 4.8 자동 테스트 runner
- `docs/DINT_PACKET_MAP.txt`: LASAL 파서용 오프셋
- `docs/UNIT_CONVERSION_MANUAL_2026-07-10.md`: PC 호출자 UNIT 변환 배포 매뉴얼
- `docs/API_STRUCTURE_DECISION_2026-07-09.md`: 현재 API 구조 결정 기록
- `docs/RPC_CONNECTION_PACKET_DECISION_2026-07-09.md`: RPC connection 패킷 근거
- `docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md`: callback listener 소유권과 수명주기
- `docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md`: RPC/UDP callback 구현 상태와 검증 기준
- `docs/RESPONSE_MODEL_DESIGN_2026-07-09.md`: `LMC_Response` 한계와 응답 parser 재설계 방향
- `docs/LASAL_OBJECT_DISPATCHER_DESIGN_2026-07-10.md`: 실제 object name lookup과 opaque descriptor 설계
- `docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md`: 일반 TCP server와 CyWork-only queue 실행 설계
- `docs/GROUP_API_IMPLEMENTATION_2026-07-14.md`: 현재 group API의 LASAL method, 제한과 검증 상태
- `docs/NINE_AXIS_DISPATCH_IMPLEMENTATION_2026-07-15.md`: 9축 single-axis dispatcher 범위와 group 분리 원칙
- [`../../docs/architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md`](../../docs/architecture/AXIS_REFERENCE_LASAL_NATIVE_DORMANT_CONTRACT_2026-07-31.md):
  2026-07-31 당시 `0x7D13 ReferenceAxis` dormant 설계 기록. 현행 wire 의미로 사용하지 않는다
- [`../../docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md`](../../docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md):
  현행 `0x7D13/18/19` LMC Home, gate-off `0x7D15/16/17` DS402 Home과
  source-on `0x7E53/54/55` TW[20]/TW[19] 계약
- `docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`: 폐기된 RtWork 대안 검토 기록
- `docs/AUTOMATED_TESTS_2026-07-10.md`: 자동 테스트 범위와 실행법
- `docs/NEGATIVE_WIRE_TOOL_2026-07-27.md`: public SDK 보호를 유지한 internal-only
  diagnostics raw rejection 도구, 실행 확인 절차와 report/pcap 증거 경계
- `docs/TOPOLOGY_IO_QUALIFICATION_TOOL_2026-07-28.md`: 현재 bit 14에서
  `topology-inventory`의 `0x7E11` 1회 + `0x7E12` 7회, 총 8개 raw read와 향후
  `0x7E13/0x7E22` dormant qualification 실행법 및 증거 경계
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계
- [`../../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md`](../../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md):
  D1~D3, D4 single-bank Ring/Trigger와 D5 general-inline Read 내부 PLC 시험 순서와 판정 기준

자동 테스트는 `RunPcTests`(C# Phase 1, Phase 2 `0x7D22`, dormant `0x7D12`와
Home/encoder-maintenance 계약 포함), `RunLasalContract`(tracked LASAL
source static checks), `RunTests`(두 검증과 개발 WPF test app build) target으로
분리돼 있다. 개발 WPF의 실제 컨트롤/fake RPC 회귀는 별도
`LasalApiWpfTestApp.SmokeTests.csproj /t:RunWpfSmokeTests` target이며 current Release는
VS2019 MSBuild 332/332 PASS다. 이번 tranche에서는 Debug smoke를 다시 실행하지
않았다. Admin/Drive read-only 탭의 exact request/typed UI와 one-click bounded SDO Read의 typed/raw terminal 표시, accepted-timeout/cancel
ticket과 last-status 보존/수동 Refresh 복구, pre-accept cancel 및 capability-off zero-wire,
  terminal failure guard 해제, SDO Write의 current-session same-value proof 선행 gate와
  비모달 immutable arm/편집 시 re-arm/exact second-click consume,
abrupt-disconnect 버튼의
capability/idle gate와 실제 WPF child process의 SDO/DO unresolved record 및 D4 Double active journal 재시작,
single-writer lock, Close interlock, `0x7E50/0x7E23/0x7E40..0x7E4F` zero-replay와 강제 종료 뒤 byte-identical journal 재복구를
포함하고, typed v2 SDO record의 강제 recovery 호출도 capability-off 상태에서는 추가
capability read와 SDO submit이 0회임을 확인한다. 또한 bit 6 + two-buffer + 실제 4-entry Recordable Catalog로
`DoubleContractReady=True`를 만든 뒤에도 live 버튼 disabled, 수동 Double mode 미노출,
mode-ambiguous Adopt와 강제 주입 Configure의 `0x7E40..0x7E4F` zero-wire를 확인한다. 잠긴
D4 journal은 신규 mutation admission을 fail-closed한다. 추가 smoke는 같은 recovery Guid의
결정적 nonzero RequestedConfigId와 active journal 상태에서도 ordinary diagnostics-ready 조건과
분리된 reconnect recovery capability contract, semantic journal conflict 뒤 usable 상태와 runtime
I/O failure 분류를 확인하고, 네 proof/route gate가 모두 `false`여서
live wire가 0회임을 검증한다. durable motion 회귀는 Move 전/Stop 전/해제 전의 fresh
  BootId/MapRevision identity gate, restart exact recovery, status-only 해제 금지, Axis Power Off의
  방향성 durable journal과 연속 3회 `PowerOn=False && Standstill=True`, ACK 직후 실제 child-process
  Kill/restart의 `0x2023` zero-replay와 journal lock 재획득, Axis Stop Begin 1회/status-only Resume과
  Stop monitor를 더 새 Power Off가 선점해도 `0x2022`를 replay하지 않는 경로, 강제 종료 뒤 Move
  zero-replay를 포함한다.
qualification/retained-cleanup/reconnect adapter는 구현됐지만
PLC/live/pcap proof는 대기다. 고객 배포 예제 build는 기본
`RunTests` 완료 조건에서 제외한다.

장시간 parser 변이는 기본 suite와 분리해 같은 test executable에서 명시적으로 실행한다.

```powershell
.\bin\Release\LasalMotionControlLib.Tests.exe parser-stress --seed 0x7E4C7E4D --iterations 100000
```

## 주의

이 DLL은 Maestro의 LREAL 패킷과 호환되지 않습니다. LASAL 전용입니다.

`MoveCircle`은 현재 공개 C# API와 승인된 LASAL-DINT command ID/payload 계약에
없으므로 이 버전의 구현 범위가 아닙니다. 이름만 추가해 임의 wire protocol을
만들지 않습니다.

WPF example의 기본 `UNIT=10000`은 현재 저장된 `_LMCAxis1..9`의 `1 mm`
macro와 일치한다. Jerk 기본값은 `0`이지만 입력할 수 있으며
`Jerk DINT = (물리 jerk / 1000) x 축 UNIT`을 사용한다. 현재 저장된 profile은
`_JERK_PROFILE`, `JMax=75000 mm`다. 과거 `8,388,608 count/rev`는 23-bit
encoder dummy였고 DLL의 자동 변환이 아니다. 실제 배포 프로그램은 다운로드된
PLC 설정과 일치하는 UNIT/MoveType/JMax를 사용해야 한다.

Group motion의 nonzero Jerk도 robot profile 설정이 필요하다. canonical
`_LMCRobotBase1`은 `_JERK_PROFILE`, `JMax=50000 mm`로 저장돼 있다.
raw `GroupReset[Async]`는 `AxQuitError(AxisNo:=0)` 기반 ACK-only axis/hardware error reset이며
robot profile error 전체 초기화를 보장하지 않는다. 완료가 필요하면
`BeginGroupResetWaitForStableErrorClearanceAsync` /
`ResumeGroupResetWaitForStableErrorClearanceAsync` 또는 compound facade를 사용한다. 이 경로는
`0x20D2` observed snapshot 뒤 Reset을 한 번만 보내고, 각 status-only round의 `0x2045`와
pinned member별 `0x2028`에서 group/member error all-clear를 기본 3회 연속 확인한다. generic
snapshot은 expected topology/current PLC build attestation이 아니다. command-before 저장은
`LMCGroupResetPreparedEvidence`, cross-session status-only attach는
`LMCGroupResetDurableRecoveryRecord`와 `AttachGroupResetDurableRecoveryAsync`를 사용한다. attach는
fresh `0x20D2`의 count/order/name/reference/device exact-match 뒤에만 current-session continuation을
게시하며 `0x2049`를 보내지 않는다. evidence의 `RecoveredFromDurableRecord`와
`CommandDispatchedInOwnerSession`으로 fresh dispatch와 recovery를 구분한다.
Stop/PowerOff/safe Disable의 accepted/outcome-uncertain takeover 뒤 Reset을 replay하지 않고, valid
safety NACK는 continuation을 보존한다. captured-member Axis safety coordinator는
`SupersedePendingGroupResetAfterCapturedMemberSafetyMutation`으로 exact generation을 reconcile해
SDK pending을 즉시 terminalize할 수 있다. 이 proof는 DS402 Fault, power/profile lock,
reference/home 또는 motion-ready 증거가 아니다. GroupStop ACK 뒤에도
`GroupReadStatusResult`로 실제 상태를 확인한다.
`StopMove()` 반환값은 오류가 아니라 정지가 끝날 profile-buffer `StopCmdNo`이므로
ACK status/error로 해석하지 않는다. GroupStop success ACK는 입력 검증과 호출
dispatch까지만 뜻한다.

선형축 profile 예:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

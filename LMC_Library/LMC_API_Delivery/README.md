# LASAL Motion Control Lib

LASAL 전용 DINT 패킷 API입니다. 기존 Elmo/Maestro용 legacy 패키지와 별도로 사용합니다.

프로젝트 전체 역할과 release gate는
[현재 아키텍처 및 릴리스 상태](../../docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md)를
우선합니다.

## 개발 상태

2026-07-27 기준 C# request/typed response path와 재실행 가능한 자동 테스트가
반영됐습니다. tracked `TCPMotionInterface`에는 RPC lifecycle, 실제 LASAL
객체명 lookup, opaque descriptor, 9축 single-axis dispatcher, DINT single-axis path와
현재 공개된 group API handler를 반영했습니다. Diagnostics 개발 source에는
EtherCAT Health/Catalog/PI Read, Bulk Snapshot, Recorder v1, D4 single-bank
Ring/Trigger와 D5 Read 및 fail-closed Write 공개 API가 포함됩니다.

현재 완료도를 구분하면 다음과 같습니다.

- 기존 Wireshark 캡처 기준 command: 23개
- LASAL project-local extension command: 2개
  (`0x204A GroupPowerOn`, `0x204B GroupPowerOff`; 기존 캡처 명령이 아님)
- LASAL diagnostics command namespace: `0x7E00..0x7E51` 중 24개 ID 예약
  - D0~D3: capability, Health/Catalog/PI Read, Bulk, Recorder v1 handler 활성
  - D4: single-bank Ring/Trigger와 `0x7E42` 활성, Double bank는 미구현
  - D5: test profile에서 축 1~4, nonzero ObjectIndex, 임의 U8 SubIndex와 exact typed
    1/2/4-byte SDO Read ticket/status/queued cancel 활성. SDO Write의
    parser/executor/API/WPF 경로는 구현됐지만 승인 target과 capability는 비활성이고,
    extended result도 비활성
  - Phase 1 PI/Bulk compatibility facade: catalog alias PI Read와
    `AddEntry/Configure/Upload/GetEntry` local builder/reader 구현; wire는 D1/D2 재사용
- LASAL admin command: 4개
  (`0x7D00 GetAdminCapabilities`, `0x7D10 ReadAxisParameter`,
  `0x7D20 ReadGroupParameters`, `0x7D22 GroupMoveLinearRelative`)
- 성공 응답 capable PLC active command: 51개
  (기존 motion/group 25 + diagnostics D0~D3 18 + D4 Trigger 1 + D5 general-inline 3 + admin 4)
- dispatcher/wire handled contract: 53개
  (active 51 + D5 reserved `0x7E21/0x7E51` 2)
- C# diagnostics 공개 API: D0~D5 sync/async contract 구현
- LASAL diagnostics test build capability:
  - 정상 retained BootId 경로의 전체 값: `CapabilityBits=0x0000213F`
  - bit 0~2: Health, SignalCatalog, PIRead
  - nonzero retained BootId일 때 bit 3~4: BulkSnapshot, RecorderSingleBank
  - nonzero retained BootId일 때 bit 5: RecorderTrigger
  - nonzero retained BootId일 때 bit 8: SDORead, bit 13: SDOReadGeneralInline,
    `MaxSdoDataBytes=4`
  - bit 6, 7, 9~12: 0
- CyWork axis/group control·read·motion command: 18개
  (lifecycle과 name/member metadata handler 제외)
  (`0x2023`, `0x2024`, `0x2022`, `0x2028`, `0x202E`, `0x209F`,
  `0x20A0`, `0x20A2`, `0x204A`, `0x204B`, `0x2047`, `0x2048`, `0x2045`, `0x2049`,
  `0x2085`, `0x20A4`, `0x2051`, `0x20E7`)
- 기존 캡처 기반 23-command 공개 범위의 deterministic unsupported: 0개
- C# 자동 테스트 runner: Debug/Release 각 277/277 PASS
  (SDO Write target policy와 operation-kind별 quarantine 회귀 포함. 기존 269개는
  직전 260개 + UI 독립 D5 pending cleanup orchestrator 9개이고, 직전 260개는
  기존 225개 Phase 1/2
  회귀, 53-command response hard limit, AxisInfo descriptor,
  read-only qualification 분석/CSV, callback lifecycle loopback과 Recorder
  two-session exact/discovery adoption, pre-close transport-fault exact recovery,
  Fault mutation 차단, cancel/Stop-race/release retry/quarantine, Bulk cleanup/retry와
  one-slave-partial 순수 판정, Group Stop-first fallback/UI-context orchestration,
  internal negative-wire 계약 9개, D5 abort/recovery analyzer 12개와 drive-read
  command stage/ticket 및 non-domain 계약 2개 포함 + D5 external-read WPF
  routing orchestrator 7개 + drive-read all-failure facade context 4개 + raw
  `SubmitSdo` submission context 7개 + manual failure router 1개 + owner-bound
  immutable D5 quarantine ledger/atomic recovery commit 5개 + recovery scope policy 7개 +
  quarantine ledger deterministic concurrency 4개로 구성)
  concurrency 4개는 각 등록 test를 50회 반복해 candidate snapshot 뒤 clear 전 mutation 거부, atomic clear
  뒤 competing Arm 보존, callback 예외 뒤 waiter 진행과 ledger 재사용, concurrent Disarm
  exact-once를 bounded wait로 검증하며 `Thread.Sleep`을 사용하지 않는다. 성공한 Write 뒤에는
  동일 Slave/Index/SubIndex/Type/Length의 Read 결과가 exact 4-byte Write 값과 일치할 때만
  mutation/Close interlock을 해제하는 순수 계약 시험 1개도 포함한다. 이는 PC test 강화일 뿐
  production/wire/LASAL 변경이나 PLC live 증거가 아니다.
  pending cleanup 9개는 owner/current connection, ticket owner와 저장 MapRevision을 wire 전
  fail-closed하고, capability BootId를 우선 판정한 뒤 MapRevision 불일치를 status/cancel 없이
  quarantine한다. cached terminal status/cancel 무송신, cached pending refresh, Queued-only cancel과
  `InvalidState` race, Running wait, cancel accepted 뒤 exact `Cancelled/Cancelled`, 마지막 status
  보존, 최소 15초/남은 deadline+1초/최대 120초 및 `<=` poll 경계를 검증한다. production WPF는
  같은 UI 독립 orchestrator를 호출한다. 이 검증은 PC test이며 PLC live/pcap 증거는 아니다.
- LASAL SourceOnly static contract: PASS. 새 `TryStartWrite`, `ActiveIsWrite`, `WriteBuffer`,
  `SdoWriteData`, `GetSdoWritePolicyDetail` declaration은 tracked `Classes.lcb`에 아직 없으므로
  switch 없는 full static은 의도적으로 FAIL한다. `-AllowStaleLasalBinaryMetadata` PASS는
  external source 중간 검사일 뿐 LASAL IDE 동기화/빌드 증거가 아니다.
- 개발 WPF example Debug/Release build: PASS. startup smoke는 기존
  Group/Bulk/Recorder panel까지 PASS이며 D5 panel visual은 별도
- DiagnosticsBootCounter/D1~D4 single-bank와 gate-off D5 source LASAL IDE
  Rebuild/Link: 0 error, version mismatch warning. gate-on fixed-source runtime download는
  BootId 5 capture로 확인했지만 대응 IDE build/smoke log는 미보존
- 위 통합 source의 `Find in Implementation` smoke: InputLatch, RecorderStore,
  TCPMotionInterface.Diagnostics 3건 PASS; smoke 이후 `Lasal2.log`의 신규
  `CInvalidArgException` 0건
- 위 LASAL IDE build/smoke 증거는 현재 SDO Write 변경 전 snapshot이다. Write 경로는
  아직 LASAL build, PLC download, 실축 또는 EtherCAT mailbox로 검증하지 않았다.
- CyWork와 motion RT thread의 CPU core/priority 조건: 미검증
- diagnostics PLC: `11_PI_Bulk_Regression`의 D0/D1/D2 happy path와
  `10_DriveRead_Axis1to4`/`12_SDO_GeneralInline_4Byte_FailureRecovery`의
  general-inline 1/2/4-byte 및 same-BootId TypeMismatch recovery packet PASS.
  D1/D2 partial 판정 코드는 완료됐지만 fault/soak live capture, D3/D4 전체와
  D5 나머지 fault matrix는 별도. read-only D5 abort -> known-valid recovery WPF runner와
  순수 판정 코드는 build/test 완료했지만 PLC live와 pcap은 미검증

기존 motion/control PC API 범위는 캡처 기반 23개 command와 LASAL local motion
extension 2개 모두 request/public path까지 구현됐다. Diagnostics는
`LMCConnection.Diagnostics` 아래 D0~D5 공개 API와 common envelope, capability,
Catalog/Health/PI/Bulk/Recorder/ticket/chunk parser를 제공한다. 현재 PLC test build가
광고하는 실제 실행 범위는 D1 read-only, D2 Bulk, D3 single-bank manual Recorder,
D4 single-bank Ring/Edge/Window/Mask/forced Trigger와 D5 general-inline SDO Read다.
D5의 legacy `0x1000:0` 4축 path와 general-inline 1/2/4-byte SDO Read는 live packet으로
확인했다. 의도한 TypeMismatch terminal failure 뒤 같은 BootId의 다음 Int8/1 ticket
success도 확인했다. offline/abort, timeout, queued cancel, disconnect/orphan과
contention은 아직 production qualification으로 남아 있다. abort -> recovery는
`0x6061:0 Int8/1` baseline과 같은 BootId/MapRevision의 복구를 판정하는 WPF runner까지
구현했지만 실제 abort code와 recovery packet은 아직 확보하지 않았다.

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
D4 Double bank와 D5 PI Write 및 extended result는 capability-off다. SDO Write는
`0x7E50`의 `OperationFlags=1`, exact 36-byte, Int32 4-byte request와
`OperationKind=SDOWrite(3)` parser/executor 및 C# API/WPF까지 구현했지만,
현재 PLC의 global gate와 UI[24] axis 1~4 per-axis gate가 모두 `FALSE`이고 SDK의
`SdoWriteEnabled`/`SdoWriteUi24Axis1Enabled..Axis4Enabled` 및
`AllowedSdoWrites`도 closed/empty라 실행할 수 없다.
승인 후보는 Gold
UI[24] `0x2F00:24`, exact Int32 4-byte지만 사용자 drive program에서 미사용인지와 적용
축이 확정되지 않았다. 배포 설정에서는 확인한 한 축의 gate만 활성화한다. 임의 SDO address와
DS402 motion/control object는 승인할 수 없다.
WPF는 PLC bit 9, SDK target allowlist, 선택 축의 `PowerOn=False`/`Standstill=True`, actual
position 3회 안정, 명시적 확인과 operation-kind별 quarantine을 모두 통과해야만 Write를
submit한다. write outcome이 불명확하면 read recovery로 자동 해제하지 않는다. 이 경로의
LASAL build/PLC/실축 검증은 아직 없다.
Phase 1 WPF의 PI Write는 SDK compile-time allowlist가 empty인 것에 더해
`Phase1AllowsPiWrite=false`가 입력/button을 비활성화하고 click handler도 다시 거부하는
이중 차단이다.
PI/Bulk compatibility facade는 `CreatePIBulkBuilder(catalog)`와 alias `ReadPI`로 구현했다.
builder는 catalog의 exact `MapRevision`, readable flag, 최대 32개와 중복을 검사하며,
`Upload` 뒤 `GetEntry/TryGetEntry`로 최신 snapshot을 조회한다. 별도 D6 wire를 만들지 않고
D1/D2 wire를 그대로 사용한다.

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

Phase 2 첫 motion facade는 `LMCGroupAxis.MoveLinearRelativeEx[Async]`다. Admin
`0x7D22`가 16-slot distance vector와 dynamics/options를 보내고 PLC는 현재 위치를 PC에서
합산하지 않은 채 `LMCRobot.MoveRelativeCoord`를 직접 호출한다. current 4-axis contract는
slot 5..16=0, Coordinate=None, transition ExactStop/ContinuousDirect, buffer
Aborting/Buffered, Execute=true만 허용한다. 성공 ACK는 profile queue 수락이며 실제 완료와
profile error는 기존 `GroupReadStatus` poll로 확인한다.
`0x7D00 FeatureBits=0x00000007`은 새 DLL과 PLC source를 함께 배포하는 계약이다.
이전 DLL은 bit 2를 unknown feature로 strict reject하므로 PLC만 먼저 내려받지 않는다.

`../LasalApiWpfTestApp`의 `Read-only API` 탭은 이 Phase 1 surface를 실물에서 확인하는
전용 UI다. Admin capability를 먼저 읽어야 axis/group parameter 버튼이 활성화되고,
physical axis 1..4의 operation mode와 non-atomic drive status도 같은 탭에서 확인한다.
이 화면은 read-only이며 LASAL IDE build/download와 live PLC 검증을 대신하지 않는다.

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

live capture에서는 `LockProfile`이 수락됐는데도 `0x2047` handler가 같은 CyWork의 stale
LockState를 읽어 `ErrorId=-6`을 반환했다. PC에서 성공으로 바꾸지 않는다. PLC ACK를
accepted-then-poll로 수정하고 최종 lock은 `0x2045`로 확인해야 한다.

다만 전체 장비 API 완료가 아니다. 2026-07-23 capture에서 Admin
`0x7D00/0x7D10/0x7D20/0x7D22`, 대표 absolute/relative group, Stop/PowerOff,
`09b` None/ACS static alias, D1/D2와 D5 1/2/4-byte happy path는 PASS했다.
`0x2047` ACK timing, true Buffered/stop-first, D1/D2 soak/fault, D3/D4와 D5 나머지
fault matrix가 남아 있다.
callback은 payload 캡처가 없어 raw datagram event까지만
제공한다.
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
control/read/motion command 및 `0x7D22` Admin motion을
실행합니다. interface RT task, RtWork mailbox와 atomic state는 사용하지
않습니다. 일반 `_TCPIPServer1`과 interface의 동일 cyclic task, axis RT thread와
같은 core 배치, PLC jitter를 확인하기 전까지 production-safe로 판정하지 않습니다.

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
- `RpcInitConnection`은 `0x405C` 전송 전에 callback listener를 열고 raw callback payload를 이벤트로 전달
- `CloseConnection`/`Dispose`는 캡처 기반 close frame(`0x405D`)을 송신
- 연결 timeout, state event, 초기화/transport/close 오류 분리, callback
  source-address 검증과 취소 가능한 async API를 제공
- timeout/전송 오류와 in-flight 취소는 오염된 transport를 폐기하고
  `Faulted`로 전환하며, queue 대기 중 취소는 active request를 건드리지 않음
- diagnostics의 configuration/resource/ticket 상태 변경 async API는 token을
  전송 시작 전까지만 적용한다. 전송 뒤에는 handle/ticket/최종 결과를 잃지 않도록
  같은 RPC 완료를 기다리며, 이 구간의 token 취소는 PLC Stop 명령이 아님
- reconnect 후 이전 session에서 만든 axis/group object는 stale handle로 거부
- `LMCConnection.Diagnostics`에서 `0x7E00` capability를 sync/async로 조회하며,
  stateful bit가 켜졌는데 `DiagnosticsBootId=0`이면 malformed contract로 거부
- 취소 가능한 name lookup은 `LMCSingleAxis.CreateAsync`와
  `LMCGroupAxis.CreateAsync`를 사용하며 generation 검증과 request 전송을
  같은 exchange gate에서 확인
- `LMCSingleAxis`/`LMCGroupAxis` object는 name lookup으로 얻은 reference를 보관하고, 이후 motion/status API 호출 시 해당 reference를 패킷에 자동 삽입
- DLL은 `_LMCAxis1` 같은 PLC object name을 하드코딩하지 않음
- LASAL이 연결된 실제 object name을 읽어 opaque descriptor를 발급하고, 이후 descriptor로 axis client를 dispatch
- read API는 명령별 typed result를 제공하며 malformed response를 정상값 `0`과 구분
- WPF example은 네트워크/polling을 비동기로 실행하고 connection/callback 상태,
  raw callback log, 확인창 없는 즉시 명령, MoveVelocity stop 추적과 group API/options를
  제공. in-flight Cancel은 transport를 중단해 연결을 `Faulted`로 만들 수 있고
  PLC Stop을 보내지 않으므로, 안전 관련 command/rollback 중에는 Cancel을 차단
- 공개 API는 한 기능당 하나만 둡니다. `LMC_*Cmd`와 같은 중복 메소드 alias는 제공하지 않습니다.

## 폴더

- `src/bin/Release/LasalMotionControlLib.dll`: 현재 내부 개발 빌드 산출물
- `../LMC_API_Distribution`: 고객 배포 시점에만 검증된 DLL/문서/예제를 복사하는
  별도 산출물 영역. 내부 개발 build와 자동 미러링하지 않음
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
- `docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md`: 폐기된 RtWork 대안 검토 기록
- `docs/AUTOMATED_TESTS_2026-07-10.md`: 자동 테스트 범위와 실행법
- `docs/NEGATIVE_WIRE_TOOL_2026-07-27.md`: public SDK 보호를 유지한 internal-only
  diagnostics raw rejection 도구, 실행 확인 절차와 report/pcap 증거 경계
- `docs/SESSION_MANAGEMENT_DESIGN_2026-07-09.md`: 다중 PC 세션 관리 설계
- [`../../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md`](../../docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md):
  D1~D3, D4 single-bank Ring/Trigger와 D5 general-inline Read 내부 PLC 시험 순서와 판정 기준

자동 테스트는 `RunPcTests`(C# Phase 1과 Phase 2 `0x7D22` case 포함), `RunLasalContract`(tracked LASAL
source static checks), `RunTests`(두 검증과 개발 WPF test app build) target으로
분리돼 있다. 고객 배포 예제 build는 기본 `RunTests` 완료 조건에서 제외한다.

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
`GroupReset`은 `AxQuitError(AxisNo:=0)` 기반 axis/hardware error reset이며 robot
profile error 전체 초기화를 보장하지 않는다. `GroupReset`과 `GroupStop` ACK 뒤에는
`GroupReadStatusResult`로 실제 상태를 확인한다.
`StopMove()` 반환값은 오류가 아니라 정지가 끝날 profile-buffer `StopCmdNo`이므로
ACK status/error로 해석하지 않는다. GroupStop success ACK는 입력 검증과 호출
dispatch까지만 뜻한다.

선형축 profile 예:

```csharp
var position = checked((int)Math.Round(1.0 * LMC_Units.MM));
axis.MoveAbsoluteEx(position, velocity, acceleration, deceleration, jerk);
```

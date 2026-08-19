# LASAL Motion Control Example API Mapping

이 예제는 현재 PLC에서 활성화된 motion 경로와 SDK의 capability-gated diagnostics
경로를 화면에 노출한다. Diagnostics 버튼은 PLC가 해당 capability를 광고해야만
활성화된다.

| 화면 | Command ID | 실제 API |
|---|---:|---|
| Runtime Language (`English`/`한국어`) | local only, wire 없음 | `UiLocalizationService`가 정적 UI chrome과 동적 Power/Reset/Stop/Group 복구 action·안전 guidance를 전환하고 기존 WPF binding을 유지한다. startup/safety confirmation과 save dialog도 선택 언어를 사용한다. raw log/result/evidence 및 입력값은 English/protocol-stable로 유지하고 culture와 숫자 parsing은 변경하지 않는다. `UiLanguagePreferenceStore`는 `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\ui-language.txt`를 사용하며 시험은 주입된 임시 경로로 격리한다. |
| Connect | `0x8080`, `0x405C` | `LMCConnection.RpcInitConnectionAsync`; this example explicitly selects version-2 32-byte registration/20-byte response while the library default remains legacy 12/4; PLC exact-peer/port ownership validation |
| Close | `0x405D` | `LMCConnection.CloseConnectionAsync` |
| D5 terminal wake | UDP `LMC2`, then TCP `0x7E03` | `LMCCallbackWakeHintEventArgs`; EventType 1/nonzero EventId를 retained current-session `LMCOperationTicket`의 BootId/TicketId와 exact match한 뒤 single-flight `GetOperationStatusAsync`를 실행한다. UDP는 state를 바꾸지 않고 only TCP response가 기존 D5 completion/journal 경로를 갱신한다. unknown/stale/busy wake는 drop하고 manual/poll fallback을 유지한다. |
| Load Axis | `0x103C`, `0x202B` | `LMCSingleAxis.CreateAsync`; `LookupResult`와 별도 `AxisInfoResponse` |
| Power On + stable verification | `0x2023` once, then `0x2028` | `PowerOnAndWaitForStableStateAsync`; accepted continuation은 `ResumePowerOnWaitForStableStateAsync`, reconnect/restart는 read-only `WaitForPowerStateAsync(true)`로 status-only resume |
| Power Off + stable verification | Begin `0x2023` once, Resume `0x2028` | `BeginPowerOffWaitForStableStateAsync`, then `ResumePowerOffWaitForStableStateAsync`; ACK+mutation generation+continuation atomic publication, priority send/preemptible monitor, later same-axis mutation typed interference/no replay |
| Reset + stable LASAL error-clear observation | Begin `0x2024` once, Resume `0x2028` | `BeginResetWaitForStableErrorClearanceAsync`, then `ResumeResetWaitForStableErrorClearanceAsync`; accepted continuation을 gate 반환 전에 보존하고 status-only 재개, confirmed interference 뒤에만 explicit new Reset. DS402 Fault/`0x603F` clearance proof가 아님 |
| Stop + stable verification | Begin `0x2022` once, Resume `0x2028` | `BeginStopWaitForStableStandstillAsync`, then `ResumeStopWaitForStableStandstillAsync`; priority send와 preemptible monitor 분리, later same-axis mutation typed interference, no Stop replay |
| Read Status | `0x2028` | `ReadStatusResultAsync` |
| Read Position | `0x202E` | `GetActualPositionResultAsync` |
| Set Axis Position | Start `0x7D12`, outcome `0x7D14`, retire `0x7D1A` | SDK는 diagnostics identity+4 x U32 intent one-shot mutation, original BootId retained key와 fresh current BootId를 분리한 restart-safe exact read-only query, nonzero generation CAS retirement 계약을 제공한다. bit 5는 query-only이고 별도 bit 7이 retirement를 광고한다. WPF에는 독립 durable journal core만 있고 MainWindow와 연결하지 않았다. PLC route/exact parser는 source-active지만 retained store/tombstone이 없어 valid query/retirement도 detail 24이고 bit 3/5/7은 OFF, native/store mutation은 0회다. retained store/query/retirement success path, evidence-bound journal과 unified ownership 동시 연결, task/core/priority, authoritative max-jump 및 PLC proof 전에는 control을 노출하지 않는다. |
| LMC Home - Current Position Zero | Start `0x7D13`, outcome `0x7D18`, retire `0x7D19` | `LMCSingleAxis.PrepareLMC_Home`, `LMC_Home[Async]`, `ReadLMC_HomeOutcome[Async]`, `RetireLMC_HomeOutcome[Async]`. `ExpectedActualPosition`은 stale-read guard이고 target은 0이다. motion enable이나 Home/limit switch 탐색은 없다. Admin bit 4는 current source에서 ON이고 Single Axis WPF control이 있다. Start ACK는 완료가 아니며 terminal query와 exact retirement가 필요하다. WPF는 `LMC Home outcome:`에 record/native/position/evidence/failure/runtime/generation 상세를 기록한다. raw feedback 성공 창은 wrap-safe `-2/-1/0/+1/+2 count`이고 `+/-3`부터 거부한다. Axis2 `+1`과 Axis1 `+2`의 false `-7` 증거를 반영한 수정 뒤 C78 Rebuild/Download와 새 BootId의 한 축 runtime proof가 남아 있다. |
| DS402 Home method 37 | Start `0x7D15`, outcome `0x7D16`, retire `0x7D17` | `PrepareLMC_HomeDS402`, `LMC_HomeDS402[Async]`, `ReadDs402HomeOutcome[Async]`, `RetireDs402HomeOutcome[Async]`. Method 37, Home offset 0, velocity/acceleration/distance/torque 0의 non-moving current-position-zero source는 구현됐지만 `LMC_DIAG_DS402_HOME_ENABLED=FALSE`, Admin bit 6 OFF라 current runtime에서는 차단된다. |
| TEST ONLY Encoder Maintenance TW[20]/TW[19] | Start `0x7E53`, outcome `0x7E54`, retire `0x7E55` | TW[20]은 `0x20FC:0x02 <- UInt16 1`, TW[19]는 `0x20FC:0x01 <- UInt16 1`만 허용한다. `PrepareTw20EncoderErrorWarningReset`/`PrepareTw19MultiturnPositionReset`, `StartEncoderMaintenance[Async]`, `ReadEncoderMaintenanceOutcome[Async]`, `RetireEncoderMaintenanceOutcome[Async]`; source capability bit 18/19는 ON이다. terminal RPC 결과와 drive의 정확한 물리 효과는 별도 증거다. |
| Read Drive Operation Mode | `0x7E50`, `0x7E03` | `GetDriveOperationModeAsync`; fixed SDO `0x6061:0 Int8/1` |
| Read Drive Status | `0x2028`, then `0x7E50`/`0x7E03` twice | `ReadDriveStatusAsync`; LASAL status + fixed SDO `0x6041:0` + `0x6061:0`, non-atomic, `HasDs402Fault` comes only from `0x6041` bit 3 |
| Read Drive Error Code | `0x7E50`, `0x7E03` | `GetDriveErrorCodeAsync`; fixed one-attempt SDO `0x603F:0 UInt16/2` |
| Move Absolute | `0x209F` | `MoveAbsoluteExAsync` |
| Move Relative | `0x20A0` | `MoveRelativeExAsync` |
| Single Axis live qualification | `0x2023(true)` once -> `0x2028` stable -> `0x2028` ready + `0x202E` start -> `0x20A0` once -> `0x2028` motion/stable -> `0x202E` final x3 -> `0x2022` once + `0x2028` stable -> `0x2023(false)` once + `0x2028` stable | `MainWindow.Qualification.Axis.cs` + `AxisQualificationRecoveryJournal`; exact connection/session/build/BootId/MapRevision/axis identity, whole-sequence PowerOn/Move/Stop/PowerOff monotonic checkpoints와 command-level durable journals, cancellation-independent Stop/PowerOff cleanup, Move replay zero. 외부 Axis Stop/PowerOff는 status-only proof로 인수하고 restart는 자동 mutation 0회이며 명시적 Power Off만 허용한다. |
| Move Velocity | `0x20A2` | `MoveVelocityExAsync` |
| Load Group | `0x1042` | `LMCGroupAxis.CreateAsync`; `LookupResult` |
| Get Members | `0x20D2` | `GetGroupMembersInfoResultAsync` |
| Group Power On accepted-once + stable verification | Begin `0x204A` once, Resume `0x2045` only | `BeginGroupPowerOnWaitForStableStateAsync` + `ResumeGroupPowerStateWaitForStableStateAsync`; compound facade는 `GroupPowerOnAndWaitForStableStateAsync`, 기본 PowerOn=True 3회 연속 proof |
| Group Power Off accepted-once + stable verification | Begin `0x204B` once, Resume `0x2045` only | `BeginGroupPowerOffWaitForStableStateAsync` + `ResumeGroupPowerStateWaitForStableStateAsync`; compound facade는 `GroupPowerOffAndWaitForStableStateAsync`, 기본 PowerOn=False 3회 연속 proof |
| Group Enable (Lock Profile) + stable wait | `0x2047` once, then `0x2045` | `GroupEnableAndWaitForLockedStandbyAsync`; gates/command/status/delay total deadline, accepted timeout/error는 `ResumeGroupEnableWaitForLockedStandbyAsync`로 status-only 재개. WPF 일반/자격검증 경로 모두 exact identity durable `ArmedBeforeDispatch -> AcceptedAwaitingProof -> Resolved` journal을 사용하며 accepted `0x2047`을 replay하지 않음 |
| Group Disable (Unlock Profile) | `0x2048` | `GroupDisableAsync` |
| Group Read Status | `0x2045` | `GroupReadStatusResultAsync` |
| Raw Group Reset (ACK only) | `0x2049` | `GroupReset[Async]`; 성공 ACK는 `AxQuitError(AxisNo:=0)` dispatch acceptance이며 member error-clear 완료가 아님 |
| Group Reset stable member error-clearance | fresh Begin `0x20D2`, then `0x2049` exactly once; Resume rounds `0x2045` + pinned member별 `0x2028`; durable attach는 fresh `0x20D2` 1회와 `0x2049` 0회 | `LMCGroupResetPreparedEvidence`, `LMCGroupResetDurableRecoveryRecord`, `PendingGroupResetWaitContinuation`, `BeginGroupResetWaitForStableErrorClearanceAsync`, `AttachGroupResetDurableRecoveryAsync`, `ResumeGroupResetWaitForStableErrorClearanceAsync`, compound facade; all-clear 3회, exact-match reconnect/restart status-only recovery, no Reset replay, safety takeover |
| Raw Group Stop | `0x2085` | `GroupStop[Async]`; WPF button은 accepted-once split 경로 사용 |
| Group Stop accepted-once + stable verification | Begin `0x2085` once, Resume `0x2045` only | `BeginGroupStopWaitForStableStandbyAsync` + `ResumeGroupStopWaitForStableStandbyAsync`; compound facade는 elapsed deadline 공유, 기본 Standby 3회 연속 proof |
| Group Read Position | `0x2051` | `GroupReadActualPositionAsync` |
| Move Linear Absolute | `0x20A4` | `MoveLinearAbsoluteExAsync` |
| Set Identity Kinematics | `0x20E7` | `SetKinTransformCartesian4AxisAsync` |
| Diagnostics Capabilities | `0x7E00` | `LMCConnection.Diagnostics.GetCapabilitiesAsync` |
| PI Catalog Info / Chunk | `0x7E01`, `0x7E02` | `GetSignalCatalogAsync` |
| EtherCAT Health | `0x7E10` | `ReadEtherCATHealthAsync` |
| Connect auto-load / Reload CREVIS / Topology | `0x7E00`, `0x7E11`, `0x7E12` | `GetCapabilitiesAsync`, `GetEtherCATTopologyAsync` |
| Read Selected Node Health | `0x7E13` | topology-bound `ReadEtherCATNodeHealthAsync` |
| Read Digital Input / Output Shadow | `0x7E22` | topology-bound `ReadDigitalIOAsync` |
| Save Live Health/DI Evidence | wire 없음 | current-session-gated Auto/Manual journal snapshot의 TXT/CSV UTF-8 no-BOM export |
| Submit Digital Output Write | `0x7E23` | `CreateDigitalOutputWriteRequest`, `SubmitDigitalOutputWriteAsync` |
| PI Read | `0x7E20` | `ReadPIAsync` |
| Bulk Configure / Status / Snapshot / Release | `0x7E30`~`0x7E33` | `ConfigureBulkAsync`, `ReadBulkStatusAsync`, `ReadBulkAsync`, `ReleaseBulkAsync` |
| Recorder Configure / Start / Trigger / Stop | `0x7E40`~`0x7E43` | `ConfigureRecorderAsync`, `StartRecorderAsync`, `TriggerRecorderAsync`, `StopRecorderAsync`; Configure/Start accepted typed result와 Trigger/Stop delayed ACK는 priority publication을 사용한다 |
| Recorder Status / Header / Chunk | `0x7E44`~`0x7E46` | `GetRecorderStatusAsync`, `GetRecorderHeaderAsync`, `ReadRecorderChunkAsync`, `DownloadRecorderAsync` |
| Recorder Buffer / Configuration Release | `0x7E47`, `0x7E48` | `ReleaseRecorderBufferAsync`, `ReleaseRecorderAsync`; delayed ACK 선점은 resource별 `OutcomeUnverified` quarantine |
| Recorder Reconnect Adoption | `0x7E49` | `AdoptRecorderAsync`, `AdoptActiveRecorderAsync` |
| Recorder Double exact inventory / adoption / explicit cleanup | `0x7E4A`, `0x7E4B`, `0x7E49`, `0x7E44`, `0x7E43`, `0x7E47`, `0x7E48` | reconnect WPF adapter가 `ReadRecorderBankInventoryAsync` 뒤 occupied bank는 exact `AdoptRecorderAsync`, empty configuration은 `AdoptEmptyRecorderConfigurationAsync`로 채택한다. 일부 Adopt 성공 handle도 즉시 보존한다. bank는 Status, 필요 시 Stop -> Ready/Uploading 뒤 B -> A, 마지막으로 동일 Buffer 0 identity의 configuration을 명시적으로 Release한다. detail 32는 `LMCRecorderConfigurationAbsentException`으로 승격해 pending final Release journal만 zero-mutation resolve |
| Recorder Double token-qualified Configure / resolver | `0x7E4C`, `0x7E4D` | config-only/qualification WPF adapter가 recovery Guid에서 결정적 nonzero ConfigId를 만든다. `ValidateRecoverableDoubleRecorderConfiguration`으로 owner/session-bound capability snapshot과 전체 설정을 저널 arm 전에 검증하고, pinned-capability `ConfigureRecoverableDoubleRecorderAsync` overload로 같은 BootId/MapRevision을 one-shot 전송한다. v3 `ClientTokenV1` journal의 ConfigRevision=0 recovery는 `ReadRecoverableRecorderBankInventoryAsync`로 typed absence 또는 actual revision을 durable 저장한 뒤 0x7E4A/49/4B 경로를 사용한다. startup은 journal만 복구하고 wire를 자동 replay하지 않는다 |
| PI Write ticket submit | `0x7E21` | `SubmitPIWriteAsync` |
| SDO Read / Write ticket submit | `0x7E50` | `SubmitSdoAsync`; manual Write는 현재 connection/session/DiagnosticsBuild/BootId/MapRevision/exact target에 귀속된 same-value four-ticket PASS proof 전에 zero-wire. proof 후에도 첫 클릭에서 immutable request를 비모달 arm하고 exact identity와 byte-identical 요청의 두 번째 클릭만 실제 submit. 실제 Write는 internal `SubmitSdoWriteIdentityPinnedAsync`가 mutation gate 안에서 fresh capability와 proof identity를 다시 exact 비교하며 mismatch는 `NotAttempted`/`0x7E50` 0회다. 요청 편집은 기존 arm을 즉시 폐기 |
| SDO 1/2/4-byte bounded terminal Read | `0x7E00`, `0x7E50`, `0x7E03` | `ReadSdoInline`, `ReadSdoInlineAsync`; accepted ticket와 exact `Completed/Success` status/result를 `LMCSdoReadResult`로 반환 |
| D5 Contention -> ResourceBusy -> Recovery | `0x7E00`, `0x7E50`, `0x7E03` | `D5SdoContentionQualificationOrchestrator.RunAsync` + `SubmitSdoAsync`, `GetOperationStatusAsync` |
| D5 Timeout -> Drain -> Recovery | `0x7E00`, `0x7E50`, `0x7E03` | `D5SdoTimeoutQualificationOrchestrator.RunAsync` + `SubmitSdoAsync`, `GetOperationStatusAsync` |
| SDO Write exact readback | `0x7E50`, `0x7E03` | `CreateSdoWriteVerificationContext`, `SubmitReadbackAsync`, `Evaluate` |
| SDO Write policy/readiness | wire 없음 | immutable `EvaluateSdoWritePolicy`; cached blocker matrix와 PLC bit 9/SDK `NoApprovedTarget` 독립 표시 |
| Extended SDO result chunk | `0x7E51` | `ReadSdoResultChunkAsync` |
| Diagnostics ticket status / cancel | `0x7E03`, `0x7E04` | `GetOperationStatusAsync`, `CancelOperationAsync` |

`Connect`는 TCP 연결, RPC session 초기화, UDP callback listener 개방과 callback
등록을 한 번에 수행한다. 이 예제는 명시적으로 `Version2WakeHint`와 maximum 52 bytes를
선택한다. SDK library의 기본은 계속 legacy raw 12/4다. PLC는 `CurrentPeerValid`,
requested IPv4와 TCP peer의 exact match, port `1..65535`를 검증한 뒤 최초 valid tuple만
commit한다. exact duplicate는 idempotent이고 다른 re-registration은 기존 tuple을 보존한
채 실패한다.

`0x8080` exact short failure의 outer `Status=1`, command `Status=1`, `ErrorId=-1`은
`ParseAcknowledgement`가 보존한다. `Version2WakeHint`에서만 frame valid,
`HeaderReserved=0`, payload 4 bytes와 이 exact command error가 모두 맞을 때 20 ms
cancellation-aware 대기 뒤 같은 TCP socket으로 init을 한 번 더 시도한다. 두 번째도
실패하면 `Faulted`와 TCP/UDP cleanup으로 끝나며, 다른 ErrorId/nonzero
reserved/malformed response는 SDK zero-retry다.

Current WPF marker `RPC_INIT_FRESH_TCP_ONCE_V2`의 fresh-TCP budget은 첫 candidate에만 있다.
(A) 두 exact canonical `-1`로 `AttemptCount=2`, `CanonicalRetryUsed=true`가 된
persistent same-socket failure는 100 ms 뒤, (B) 실제 `0x8080` request가 시작된
`AttemptCount=1`이고 response가 없으며 exception이 직접 `EndOfStreamException`,
`SocketException`, `TimeoutException` 중 하나이거나 `IOException`의 `InnerException` chain에
그중 하나가 포함된
pre-response transport failure는 1000 ms 뒤 fresh candidate 하나를 연다. 두 번째
candidate의 모든 failure는 terminal이다. TCP connect-before-init(`AttemptCount=0`),
cancellation, `ObjectDisposedException`, `InvalidDataException`(허용형 `InnerException`이 있어도 포함),
malformed/valid non-`-1` response, response 이후
failure와 callback-stage failure는 retry하지 않는다. 사용자 Connect 한 번당 최대 TCP
2개/`0x8080` 4회이며 `0x405C`는 init 성공 뒤에만 전송된다. 정상 registration ACK까지
받아야 Connect가 성공한다. 이는 PLC의 persistent callback disarm `-8`/`-9` root fix가
아니다. Historical `14ccf58` V1은 persistent-`-1`만 허용했던 과거 policy다.

PLC source는 일반 `0x8080`/`0x405D` mismatch를 계속 fail-closed로 보존한다. 별도의
internal owner-loss retirement는 accepted owner transition 또는
`CurrentSock=dSock`인 definitive disconnect에서 ordinary helper가 정확히 `-8`을
반환한 경우만 sender에 `(0,0,0)` sentinel을 전달하고, sender 결과 `0/1` 뒤 같은
helper로 local tuple clear를 확인한다. `-9`, different-IP/unknown candidate, failed
takeover와 late retiring-old disconnect는 이 경로를 사용할 수 없다. 2026-08-12 15:58
reconnect PLC image의 LASAL build/download는 완료됐다. 같은 창 Close -> Connect live
PASS는 아직 확인되지 않았으므로 이 사실을 PLC runtime branch proof로 확대하지 않는다.

GUI는 connection cleanup 뒤에도 RPC init 시도 횟수, canonical retry 사용 여부와 마지막
ACK를 Active/Retired evidence로 보존한다. 입력 tuple은 `RequestedCallback`, 실제 UDP
endpoint는 `BoundCallback` 또는 init 전 bind 실패를 뜻하는 `not-bound`로 구분한다.
성공한 version-2 등록의 BootId, SessionEpoch,
cookie, listener generation, expected source, event mask와 PC receiver의
accepted/rejected/duplicate/out-of-order 누계, 마지막 decision/protocol error도 표시한다.
이는 PC측 관측 증거이며 pcap, PLC `RpcCallbackLastDisarmResult`, PLC producer/sender
counter를 대체하지 않는다. `0x8080` wire response에는 PLC의 `-8`/`-9` disarm 원인이
포함되지 않는다.

내부 replacement와 창 X는 공용 최대 2회 `Dispose` cleanup을 사용하고 complete local
disconnected postcondition을 요구한다. `RpcCloseResponse`/`LastCloseException`은 이전
connection에 남는다. X는 postcondition 미완료 시 취소되고, 명시적 Close 버튼은 cleanup
뒤에도 close 오류를 throw한다. startup은
`ReconnectPolicy=RPC_INIT_FRESH_TCP_ONCE_V2`, `SdkPath`, `SdkBuildUtc`를 기록하고 기존
topology marker V5를 유지한다. Candidate evidence는 `CandidateOrdinal`,
`FreshSessionRetryReason`, `FreshSessionRetryDelayMs`,
`FreshSessionRetryFromCandidate`, `FreshSessionRetryNextCandidate`,
`FreshSessionFirstFailure`를 기록한다. 100/1000 ms는 PC bounded backoff일 뿐 PLC readiness
증거가 아니며 PC cleanup 자체는 PLC disarm 성공 또는 internal owner-loss retirement를
뜻하지 않는다.

Typed wake는 listener가 소유한 connection과 positive session generation에 귀속된다. WPF는
dispatcher queue에서 active connection identity와 `BelongsToCurrentSession(connection)`,
retained ticket의 `BelongsToCurrentSession(connection)`, exact DiagnosticsBootId와 nonzero
TicketId를 다시 확인한다. EventType 1은 `DiagnosticsOperationTerminalAvailable`만 뜻한다.
조건이 맞을 때만 retained ticket으로 generation-pinned `0x7E03`을 조회하고, 성공한 TCP
response만 기존 terminal/journal UI path에 반영한다. UDP에서 ticket을 합성하거나
OperationKind/state를 추론하지 않는다. Gate D source에는 sender/broker와
production-path candidate `PublishEvent(...)` caller가 있지만, 승인된 exact downloaded
publisher와 실제 callback capture는 아직 없다.

이 WPF가 생성하거나 reconnect하는 모든 `LMCConnection`에는 하나의
`LMCSendPriorityCoordinator`를 주입한다. Axis/Group Stop과 Power Off는 app send gate를
기다리기 전에 safety generation을 선예약하고 priority scope에서 송신한다. SDK
`ExchangeCore`는 `stream.Write` 직전에 같은 generation을 확인하므로 아직 wire에 쓰지 않은
ordinary diagnostics와 compound API의 후속 RPC는 `LMCSendPreemptedException`으로 끝나며 해당
command byte는 0이다. 최종 검사를 이미 통과한 in-flight RPC는 취소하지 않고 결과/timeout을
확정하며, 그 뒤 safety send가 같은 직렬 경로를 얻는다. 앞선 RPC가 transport를 fault로
전환했다면 safety send도 성공을 보장하지 않는다. safety ACK 뒤에는 같은 exact generation의 상태 monitor 자리를 먼저
예약해 일반 operation이 그 사이에 들어오지 못하게 하고, 더 새 safety 예약이 생기면 이전
monitor의 다음 RPC도 stale로 거부한다. 이 선점으로 SDO submit이 write 직전에 거부되면 SDK
failure context는 `Submission/NotAttempted`이고 ticket이 없으므로 WPF tracker는 quarantine하지
않고 pre-submission guard를 해제한다. qualification 중 같은 예외는 `FAIL`이 아니라
`ABORTED`다. 이는 PC deterministic 전송 계약이며 PLC runtime packet 순서나 장비 안전 인증이
아니다.

일반 Group Stop과 Stop-first qualification은 gate 대기 전에 safety generation을 예약하고
`BeginGroupStopWaitForStableStandbyAsync`만 priority scope/command gate 안에서 실행한다. accepted
continuation과 recovery evidence를 gate 반환 전에 보존한 뒤
`ResumeGroupStopWaitForStableStandbyAsync`를 preemptible status-only monitor에서 실행한다. 더 새
Group Stop/Power Off는 monitor 중 다음 generation을 예약할 수 있다. 기존 Resume 또는 지연된 일반
Group ACK는 next write/result publication/UI application에서 폐기된다. 성공 proof에서만 pending을
지우며 cleanup은 exact pending continuation을 재사용하고 accepted Stop 뒤 fresh `0x2085`를 금지한다.
fake-RPC wire evidence는 외부 Power Off 선점에서 Stop 1회/Power Off 1회/status 4회, accepted status
failure cleanup에서 Stop 1회/status 4회다. PLC packet 순서나 정지 성능 proof는 아니다.

Diagnostics 탭은 먼저 `0x7E00` capability를 읽고 PLC가 광고한 bit에 해당하는
버튼만 활성화한다. Catalog/PI는 read-only이고, Bulk와 Recorder configuration은
선택 signal의 Catalog access flag를 다시 검사한다. Recorder download는 header와
chunk identity/sequence/CRC를 SDK가 검증한 뒤 immutable `LMCRecorderData`로
조립하며, WPF는 이 데이터만 plot/CSV에 사용한다.
`GetSignalCatalog[Async]` 결과는 diagnostics owner와 connection session generation에 bind된다.
alias PI Read, Bulk builder 생성/Configure와 PI Write submit은 unbound, foreign, reconnect-stale
Catalog를 capability 또는 data RPC 전에 거부한다. 로컬 alias 조회는 wire 없이 계속 가능하다.

CREVIS/topology 화면은 Connect 직후 capability와 static 7-entry inventory를 자동으로 읽고,
Reload에서도 capability부터 새로 읽는다. load 실패 또는 connection loss 시 이전 topology 행은
폐기하고 늦은 다른 session 응답은 commit 직전에 거부하며, 화면에 capability bits,
DiagnosticsBuild, BootId, MapRevision과 오류를 남긴다. 창 제목과 시작 로그의
`CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5` 실행 파일 marker로 구버전 GUI 실행 여부를 구분한다. `LMC_API_Distribution`
복제본은 이 current source/session-proof 계약과 동기화되지 않은 stale artifact이므로 현재
실행/배포 mapping 기준으로 사용하지 않는다. configured
schema 열과 `LIVE` health/DI 열은 분리돼 있다. 기본 선택된 `Auto refresh live state`는 bit 15
node health 또는 bit 16 selected DI가 있을 때만 동작한다. auto-loaded topology aggregate도
owner/session-bound이며 topology-bound read는 unbound, foreign, reconnect-stale aggregate를
capability/read RPC 전에 거부하고 해당 session을 exchange까지 고정한다. raw overload는
observation-only 호환 경로로 유지한다. auto monitor는 owner/session-bound cached capability
snapshot을 pinned overload에 전달하므로 eligible tick마다 별도 `0x7E00` 없이 `0x7E13` 또는
`0x7E22`를 정확히 1회만 wire로 보낸다. 일반 non-pinned API의 capability refresh+read 동작은
유지한다. 7개 node health를 순환하며 선택된 input이 있으면 DI read를 사이에 배치한다. foreground,
safety 또는 qualification command가 활성일 때는 nonblocking으로 건너뛰고, stale response는
session/topology/selection generation 대조에서 폐기한다. bit 15/16이 모두 off인 current capability에서는
자동 monitor의 wire request가 0회다. output shadow는 명시적 사용자의 read에만 갱신하며 background
monitor가 읽거나 write 승인 상태를 바꾸지 않는다. 응답의 topology revision, NodeId,
IOReference, DS402 의미, 방향과 bit 폭을 선택 entry에 대조한 뒤 표시한다. raw
`ReadDigitalIO(request)` 결과는 observation-only이고 output write를 승인하지 않는다.
`CreateDigitalOutputWriteRequest`는 topology-bound output shadow만 받으며 현재 bit 17과
SDK output allowlist가 꺼져 있어 `0x7E23`은 송신되지 않는다. current LASAL source에는
`0x7E13/0x7E22` handler, 464-byte snapshot과 CREVIS read-owner가 있으며 fresh IDE
Rebuild/Link/static smoke를 통과했다. 그러나 bit 15/16은 아직 OFF이고 PLC download,
runtime/actual-hardware proof는 없다. `0x7E23` handler/output owner는 구현하지 않았다.

수동 Health/DI도 화면이 capture한 owner/current-session capability snapshot을 pinned overload에
전달하므로 read 앞에 `0x7E00`을 추가하지 않는다. Auto/Manual 성공/실패 journal은 current-session
commit gate를 통과한 실제 read attempt만 최대 4,096개 FIFO로 보존하고 overflow의 oldest-drop
count를 노출한다. failure에는 이전 성공 sample을 복제하지 않는다. capability bit 15/16 off는
새 wire와 새 record 모두 0이고, stale/late response는 원 request가 이미 송신됐을 수 있지만
record로 commit하지 않는다. TXT/CSV export는 PC가 파싱한 response/read
failure 기록이며 physical cable order, 실제 DI 접점, physical DO feedback 또는 PLC 구현 완전성을
증명하지 않는다. 현재 `0x7E13/0x7E22` PLC runtime/actual-hardware proof는 없다.

Axis Power On facade는 mutation/status gate, `0x2023` ACK, `0x2028` status와 poll delay를 하나의
total deadline으로 제한한다. 최종 write 경계 전 취소는 zero-wire `NotAttempted`, write 뒤 사용자
취소는 ACK drain과 continuation/accepted-observer publication 뒤 typed cancellation이다. ACK 또는
status 무응답 deadline은 connection을 `Faulted`로 전환하고
`TransportInvalidatedAtDeadline` evidence를 남긴다. `WaitForPowerStateAsync`는 `0x2023`을 전혀
보내지 않는 read-only helper이므로 성공 결과도 ACK/continuation이 없고
`ReusedAcceptedAcknowledgement=false`다.

Axis Stop 버튼은 Begin에서 `0x2022` ACK와 exact latest-pending continuation을 보존한 뒤 Resume에서
`0x2028`만 보내 successful Standstill을 기본 3회 연속 확인한다. timeout/cancel/status failure,
send-priority preemption과 새 accepted Stop supersede 뒤에도 원 Stop을 replay하지 않는다. WPF는
Begin을 safety command gate 안에서, Resume을 preemptible monitor에서 실행하므로 monitor 중 더 새
Stop/Power Off가 다음 generation을 예약할 수 있다. SDK는 connection session + AxisReference의
process-local generation으로 later `LMCSingleAxis` mutation을 검출하고 typed interference로 원 Stop
귀속을 거부한다. zero-wire mutation과 다른 AxisReference는 간섭하지 않으며 외부 PLC/client,
direct SDO와 group operation은 귀속 범위 밖이다.

Group Power Begin은 `0x204A` 또는 `0x204B`를 한 번 보내고 success ACK와 continuation을
connection/session/group-reference의 session-bound send-priority publication 안에서 원자적으로
설치한다. accepted observer는 first status 전에 continuation을 전달한다. Resume은 exact pending으로
`0x2045`만 보내며 stale/resolved/concurrent Resume과 fresh same-direction Begin은 typed zero-wire로
거부된다. later same-group mutation은 `LMCGroupPowerInterferenceException`과 pending evidence를
남기고 원 command를 replay하지 않는다. `NotAttempted`, `Rejected`, `OutcomeUncertain`, `Accepted`
submission outcome과 ACK/status/poll/generation evidence는 result 및 typed exception에서 분리된다.

Group Stop Begin은 `0x2085`를 한 번 보내고 connection/session/group/latest-pending continuation을
반환하며 Resume은 `0x2045`만 보낸다. timeout/cancel/status failure/preemption은 continuation과
evidence를 보존하고, stale/superseded/completed continuation과 concurrent second Resume은 zero-wire로
거부된다. actual Stop write boundary에서 pending Enable proof를 reset하고 per-group mutation
generation을 고정한다. 이후 다른 group mutation의 actual write가 확인되면 stable Standby를 원
Stop에 귀속하지 않으며 final result publication도 원 connection session에 bind한다.
Axis Reset, Admin `GroupMoveLinearRelative`와 D5
`SubmitSdo`/`CancelOperation`의 지연 ACK도 drain 뒤 `ResultDiscarded`된다. accepted Submit은
exact ticket/BootId/MapRevision evidence를 보존하고 Cancel ACK는 stale success로 적용하지 않는다.
Recorder Configure, recoverable Configure, Start, exact/active Adopt, empty-configuration Adopt의
6개 경로는 PLC가 만든 typed result를 정상 반환하기 전에 publication이 선점되면 원 예외에
`LMCRecorderAcceptedResultFailureContext`를 붙인다. 호출자는 `TryGet(exception, out context)`로
exact recovery-only handle/identity/lease를 회수하며, Start에서는 source configuration도 함께
격리해 정상 재사용하지 않고 같은 session cleanup 또는 reconnect inventory reconciliation에만 쓴다.
Group Reset stable API는 먼저 성공한 generic `0x20D2` snapshot의 `1..16`개 nonzero/unique
axis reference를 고정한다. 이는 observed snapshot proof이며 expected topology, 현재 PLC build 또는
추적 LASAL source의 9-member 구성과 일치한다는 attestation이 아니다. Begin은 `0x2049`를 한 번만
보내고 accepted continuation을 첫 status 전에 게시한다. Resume의 각 full round는 `0x2045` 뒤
pinned 순서의 모든 `0x2028`을 읽으며 group/member error가 모두 0인 round를 기본 3회 연속 요구한다.
timeout/cancel/status failure는 same-session continuation을 pending으로 보존하고, 수동 Resume은 새
status-only timeout epoch로 시작해 stable count를 다시 센다. accepted/outcome-uncertain group 또는
pinned-member mutation은 terminal supersede이며, structurally valid safety NACK와 pre-wire failure는
Reset continuation을 보존한다. raw `GroupReset[Async]`는 계속 ACK-only다. prepared observer는
`0x2049` write 직전에 operation ID와 exact ordered snapshot을 제공하며 throw/reentrant mutation은
zero Reset wire다. durable attach는 저장된 prior outcome과 member identity를 current PLC의 fresh
`0x20D2`와 exact-match한 뒤에만 recovery continuation을 게시한다.

WPF Group Reset 버튼은 이 stable 경로를 사용한다. accepted Reset 즉시 cached group power-active,
kinematic identity, Home과 profile-lock readiness를 모두 무효화하고 proof 성공 뒤에도 자동 복원하지
않는다. final status가 LockedStandby이면 motion 준비로 쓰지 않고 safe Disable만 연다. exact live
pending 또는 submission-outcome-uncertain 상태에서는 새 Reset, Power On, Enable, SetKin, Move,
mutation qualification, Connect/Reconnect, connection/window Close를 차단한다. status-only Resume,
read-only inspection, Group Stop, Power Off, safe Disable은 허용한다. 일반 Read Group Status는 관찰
전용이며 Reset proof를 전진시키지 않는다. WPF journal은 command 전에 endpoint(remote/local/callback),
DiagnosticsBuild/BootId/MapRevision, group/ref, old owner session, ordered members와 stable count를
`ArmedBeforeDispatch`로 저장하고 ACK 뒤 `AcceptedAwaitingProof`로 바꾼다. spontaneous disconnect나
process restart는 `RecoveryRequired`로 승격한다. exact reconnect와 Load Group 뒤 SDK가 fresh
`0x20D2`를 1회 검증하고 Resume은 `0x2045`/`0x2028`만 보낸다. mismatch는 record를 유지한 채
fail-closed하고 Reset을 자동 replay하지 않는다.

`af4ab63` SDK Debug/Release `1117/1117`과 WPF Release `335/335`는 historical
snapshot이다. Historical executable-gate checkpoint `cbf2548`은 SDK Debug/Release direct runner
각각 `1133/1133`, WPF Debug/Release Rebuild PASS, 기존 full smoke `339/339`, reconnect
targeted `6/6`을 PASS했다. 독립 callback/reconnect review는 `9/9`, P0/P1 없음이다.
별도 actual-EXE relaunch gate도 Debug/Release 각각 `1/1` PASS했다. 실제 PID/HWND에 외부
`WM_SYSCOMMAND/SC_CLOSE`를 보낸 owner는 `0x405D` exact `-1` 뒤 종료되고, 같은 EXE
successor가 default named mutex를 재획득한다. Successor의 첫 session은 exact `-1`
`0x8080` 두 번과 `0x405C/0x405D` 0회, fresh session은 init/registration 성공이며 전체
session/request는 `3/28 (13,2,13)`이다. malformed probe는 exit `64`, owned root/write `0`,
TCP session `0`이고, live-mutex contender는 exit `2`, TCP session `0`, exact
`MUTEX_BUSY` report `1`개다. EXE/DLL/optional config identity도 시험 전후
동일하다. 이는 PC loopback process/mutex/wire 증거일 뿐 PLC cleanup/disarm/readiness나
실제 사용자 PLC 재접속 완료 증거가 아니다. Historical same-process 새-`MainWindow`
smoke는 그대로 보존한다.

추가된
`Wpf.CallbackV2.ExplicitCloseFixedPortThenReconnectSucceeds`와
`Wpf.CallbackV2.ExplicitCloseMinusOneFixedPortThenReconnectSucceeds`는 같은
`MainWindow`에서 동일한 fixed UDP callback port로 explicit Close 후 Connect하는 두
경로를 검사한다. 정상 close ACK와 `ErrorId=-1` close ACK 모두 local listener/TCP 정리,
fixed-port 재bind, 새 `LMCConnection`/TCP session과 두 번째 `0x8080 -> 0x405C`를 요구한다.
이는 loopback fake-RPC/UDP PC 증거이며 PLC의 `-8` retirement, `-9` fail-closed, socket
takeover 또는 사용자 PLC 재접속 완료를 증명하지 않는다. 기존 `339/339` historical
count에 소급 포함시키지 않는다.
Current V2는 Release build/full smoke `347/347`, isolated Debug build와
`Wpf.CallbackV2.*` `17/17`을 PASS했다. 이는 PC fake-RPC 증거다.

후속 배포 verifier compatibility commit `ad4af91`은 API/wire mapping을 바꾸지 않는다.
Targeted PS5/PS7 Publish+Reserve는 PASS했고, PS5.1 Release `RunLasalContract`와
`RunLasalNetworkContract`는 수정 지점을 통과한 뒤 기존 intentional `Classes.lcb sanctioned
Gate D identity drifted` STOP에서 각각 177.7초/174.9초, exit `1`이었다. 사용자 current
`Classes.lcb`는 수정하지 않았으며 full Distribution의 new actual-EXE gate/manifest에는
도달하지 않았다.

Axis Reset UI는 Begin에서 `0x2024` ACK와 accepted continuation을 live-command gate 반환 전에
저장한다. Resume은 gate 밖에서 `0x2028`의 successful `AxisErrorId == 0`을 기본 3회 연속
확인한다. timeout/cancel/status failure와 safety preemption 뒤에도 status-only Resume을 유지하고
Reset을 replay하지 않는다. accepted Resume은 계속 활성화하면서 새 live mutation은 interlock한다.
later same-axis mutation이 typed interference로 확인된 경우에만 사용자의 명시적 새 Reset을
허용한다. pending은 session cleanup에서 지우며 durable cross-reconnect proof를 주장하지 않는다.
화면은 expected/observed mutation generation을 포함한 typed evidence를 남긴다. current
`StatusWord`는 reserved 0이므로 DS402 Fault-clear 증거로 사용하지 않는다.

Physical drive read UI의 `DS402Fault`는 D5 SDO `0x6041:0` bit 3이고 별도 Error Code
버튼은 `0x603F:0 UInt16/2`를 한 ticket으로 읽는다. 둘 다 `0x2028`의 reserved
`StatusWord=0`과 구분한다. LASAL `AxisErrorId==0`, DS402 Fault=false와
`0x603F==0`은 각각의 관측이며 하나만으로 나머지 해제를 주장하지 않는다.

Edge/Window/Mask trigger와 Ring/Double 동작은 `0x7E40 Configure` payload와
capability로 선택한다. Window는 payload의 `TriggerValue`를 lower bound,
`TriggerMask`를 upper bound로 사용한다. `0x7E42 Trigger`는 locally configured
non-Manual D4 identity에만 사용한다.
reconnect resume은 기존 handle을 재사용하지 않고
`DiagnosticsBootId + RecordId + BufferId`로 `0x7E49 Adopt`한 새 identity를 사용한다.
Adopt한 resource는 Status 또는 Header로 configuration metadata를 복구한 뒤
buffer(`0x7E47`)와 configuration(`0x7E48`) 순서로 해제한다.

D4 Double은 `Run Double Bank`, `Cleanup Retained Double`, `Recover Double Journal`의 세
분리된 WPF route를 가진다. 새 qualification은 두 bank와 exact provenance를 retained state로
남기며 자동 Release하지 않는다. core non-durable orchestrator는 exact unexpected-third handle이
있을 때 명시적 unexpected third -> B -> A -> configuration release primitive를 제공하지만 WPF
durable cleanup은 이 경로를 사용하지 않는다. WPF same-session cleanup은 third Start exact
ResourceBusy가 확인된 경우에만 B -> A -> configuration을 해제한다. unexpected third success 또는
ambiguous outcome이면 same-session Release는 모두 zero-wire이고 disconnect/reconnect 뒤
token-qualified exact inventory inspection만 허용한다. conflicting inventory는 external/manual
recovery 대상으로 남기며 자동 Release하지 않는다.

cleanup/recovery는 exact journal identity와 Release order를 사용자가 checkbox로 확인해야 한다.
확인은 session/capability/journal/third-Start preflight 뒤 매 시도 소비되므로 실패 뒤 다시 선택해야
한다. confirmed-not-applied pending bank/configuration intent는 동일 target의 exact intent만
재사용하고 새 intent/다른 target을 금지한다. retained handle이 이미 ACK-success이면 Release wire를
replay하지 않고 durable confirm/resolve한다. reconnect inventory가 확인 snapshot에 없던 revision이나
`BufferId + RecordId`를 발견하면 journal만 갱신하고 `0x7E49/0x7E4B/Release` 전에 중단해 갱신된
exact 계획을 다시 확인하게 한다. 이 route는 general mutation interlock과 분리된 좁은
lifecycle admission을 사용하고, recovery capability 계약도 ordinary qualification의
Catalog/global-interlock 조건과 분리된다. 현재 `RecorderDoubleManualActionsReady`,
`RecorderDoubleQualificationExecutionReady`, `RecorderDoubleReconnectRecoveryReady`는 모두
`false`이므로 이 mapping은 code/PC 계약이며 `0x7E40..0x7E4F` live wire, PLC runtime 또는 pcap
증거가 아니다.

public `ReadSdoInline[Async]`는 1/2/4-byte Read만 허용하고 capability preflight ->
Submit -> bounded status polling을 수행한다. 반환되는 `LMCSdoReadResult`는 원 request,
accepted ticket, exact terminal status, immutable result bytes와 canonical typed value를 보존한다.
이미 수신한 terminal 성공/실패는 늦은 PC cancellation보다 우선한다. nonterminal wait
cancellation이나 poll timeout은 PLC ticket을 취소/재전송하지 않고 accepted ticket과 exact
`LastObservedStatus`를 exception에 남긴다. WPF의 `Read SDO Inline (wait terminal)` 버튼은 이 facade를 사용해
terminal ticket/status/typed value/raw bytes를 한 번에 표시한다. `Cancel Inline Wait (PC only)`는
PLC `CancelOperation`이나 재전송 없이 PC 쪽 bounded wait만 중단한다. pre-accept 취소는 submit하지 않고,
accepted 뒤 취소는 ticket/`LastObservedStatus`를 수동 cleanup 경로에 보존한다. terminal 실패는 exact
terminal status를 표시하고 operation guard를 해제한다. 기존 Submit/Refresh는 저수준 ticket 진단과
exact Write readback용으로 유지하며, pending Write readback 또는 Write mode에서는 inline 버튼을
비활성화한다. 이 버튼은 CREVIS topology/I/O 경로와 연결하지 않는다.
SDO의 4/8/12-byte 결과는 `0x7E03 GetOperationStatus` response에 inline으로
포함된다. 더 큰 Read는 `ExtendedSdoResultChunk` capability가 필요하고 terminal
success 뒤 `0x7E51 ReadSdoResultChunk`를 반복해 전체 결과를 조립한다. PI/SDO Write
버튼은 PLC capability와 SDK allowlist가 모두 허용하지 않으면 실행되지 않는다.

D5 contention runner는 `SDORead + SDOReadGeneralInline`, `MaxSdoDataBytes=4`, nonzero
BootId/MapRevision에서만 활성화된다. canonical target은 Slave 1..4의 `0x6061:0 Int8/1`로
고정한다. 첫 ticket status를 읽기 전에 두 번째 Submit을 보내 exact
`LMCSdoSubmissionOutcome.Rejected + ResourceBusy`를 확인하고, 첫 terminal 뒤 서로 다른 세 번째
ticket의 같은 값을 확인한다. 예상 밖 accepted ticket과 outcome-uncertain evidence는
quarantine하며 자동 재전송하거나 세 번째 Submit을 하지 않는다. 이는 PC/build 계약이며 실제
PLC contention 및 EtherCAT mailbox capture 증거는 별도다.

D5 timeout runner도 같은 canonical target과 capability/identity 조건을 사용한다. baseline 뒤
`TimeoutCycles=1` ticket이 exact `Expired/TimedOut`, `OperationErrorId=0`,
`OperationDetail=0x05040000`, result 없음으로 끝나야 한다. 늦은 callback drain 중 recovery Submit의
exact `Submission/Rejected + ResourceBusy`만 25 ms 간격, 최대 600회로 재시도한다. 다른 오류,
accepted-context 또는 outcome-uncertain evidence가 있으면 즉시 중단하고 보존한다. drain 뒤 서로
다른 recovery ticket이 baseline과 같은 Int8 값을 `Completed/Success`로 반환해야 PASS다. 이 역시
PC/build 계약이며 실제 PLC timeout/drain packet 증거는 별도다.

현재 `0x7E50` SDO Write 인프라는 `OperationFlags=1`, exact 36-byte request,
`ValueType=Int32(4)`, `DataLength=4`만 받으며 ticket의 `OperationKind`는
`SDOWrite(3)`이다. PLC와 SDK source의 global gate 및 UI[24] axis 1 gate만 `TRUE`이고
axis 2~4 gate는 `FALSE`다. SDK approved target은 Slave/Axis 1 Gold UI[24]
`0x2F00:24`, `Int32`, 4-byte, 값 범위 `-1073741823..1073741823` 한 건뿐이다.
GUI의 Write draft 필드는 로컬 편집할 수 있지만 exact approved target 외 임의 SDO address,
axis 2~4, DS402 motion/control object, PI Write `0x7E21`, extended result `0x7E51`은
허용하지 않는다.
일반 manual editor Write는 이 source gate만으로 활성화되지 않는다. same-value qualification이
baseline Read, unchanged pre-Write guard Read, byte-identical Write 1회, guarded exact Readback을
서로 다른 네 ticket으로 PASS한 뒤 process-local activation proof를 만든다. proof는 현재
`LMCConnection` reference/session generation, `DiagnosticsBuild`, `DiagnosticsBootId`,
`MapRevision`과 approved target의 tuple/range에 귀속되며 재연결이나 identity/target 변경을
건너 재사용되지 않는다. identity mismatch 또는 disconnect를 한 번 관측한 proof는 영구 폐기돼
A -> B -> A에서도 다시 활성화되지 않는다. manual second-click은 proof-bound capability와 target을
SDK mutation gate에 전달하고, SDK의 fresh Build/BootId/MapRevision exact 비교를 통과한 경우만
`0x7E50`을 만든다.

이 source 변경을 실제 전송 경로에 적용하려면 LASAL Rebuild/Link 및 PLC download 뒤 다시
연결해야 한다. 새 PLC source가 광고하는 expected capability는 base `0x613F`에 bit 9와
TW[20]/TW[19] bit 18/19를 더한 `0xC633F`이고, WPF는 fresh current-session capabilities의 bit 9와 nonzero
`DiagnosticsBootId`/`MapRevision`을 확인하기 전까지 same-value qualification Write와 manual
Submit을 모두 비활성으로 유지한다. manual Submit은 이 조건에 더해 current-session activation
proof가 필요하다. 기존 download가
계속 bit 9=0을 광고하면 target 편집만 가능하고 wire 전송은 0회다. public policy evaluation과
WPF readiness refresh도 cached immutable target/capability snapshot만 사용하므로 wire가 0회다.
Gold UI[24]가 사용자 drive program에서 실제로 미사용인지와 live EtherCAT mailbox/packet 동작은
아직 검증되지 않았다.

승인 후에도 GUI submit은 exact SDK target 선택, PLC capability 재확인, 선택 축의
`PowerOn=False`/`Standstill=True`와 actual position 3회 안정, 명시적 확인 및 D5
quarantine 등록을 요구한다. Write outcome이 불명확하면 read recovery proof로 자동
해제하지 않는다. Write가 `Completed/Success`여도 동일 Slave/Index/SubIndex/Type/Length를
원 Write의 owner/current session, `DiagnosticsBootId`, `MapRevision`에 묶인 guarded Read로
다시 읽어 exact 4-byte 값이 일치할 때까지 mutation과 Close를 차단한다. identity mismatch는
`0x7E50` Read submit 전에 거부하고 interlock을 유지한다. WPF는 public
`LMCSdoWriteVerificationContext`를 사용한다. 이 SDK context는 accepted Write ticket의 immutable
submitted request와 supplied Write request를 flags/target/type/length/timeout/value까지 대조하고,
같은 owner/session에 bind된 exact `Completed+Success` Write terminal status까지 context 생성에
요구한다. exact guarded readback submit과 terminal result 판정을 하나의
owner/session/BootId/MapRevision provenance에 고정하며, fresh capabilities와 Read status도 해당
owner/session에 bind된 객체만 인정한다. capability observation sequence도 context 생성 baseline보다
커야 한다. public context 생성은 SDK의 Axis 1 exact target 제한이나 fresh bit 9 조건을 우회하지 않는다.
기반 guarded `SubmitSdo[Async](readRequest, writeTicket)`도 immutable submitted Write provenance와
read target/type/length를 exact 대조하며 readback retry timeout 차이만 허용한다.
SDO/output Write의 dispatch 전
fingerprint와 accepted/terminal/readback 상태는 `%LOCALAPPDATA%` 아래 crash-safe journal에
영속화한다. 재시작 또는 exact readback이 불가능한 stale connection session에서는
command/ticket을 재생하지 않고 물리 확인과 명시적 recovery ACK 뒤 Resolved tombstone을
먼저 기록한 경우에만 volatile readback interlock을 해제한다. journal open/runtime fault,
corruption과 두 번째 writer는 새
live/mutation command와 tracked D5 read를 fail-closed한다. 일반 non-D5 read-only
inspection과 Stop/PowerOff/Group Stop은 허용한다. 정상 종료는 active durable evidence가
없을 때만 허용하며, active evidence가 남으면 connection/window Close도 차단한다.
현재 Write 경로는 PC 자동 테스트와 fresh LASAL Rebuild/Link까지 확인했지만 PLC download,
실축 및 EtherCAT mailbox로 검증되지 않았다.

Motion 인자는 PC 프로그램이 `engineering value × PLC UNIT`으로 변환하거나
이미 변환된 raw 값으로 제공한 LASAL DINT다. DLL 내부에서는 단위 변환을 수행하지 않는다. 예제의 UNIT
콤보는 이 caller-side 변환만 선택하며 wire protocol은 바꾸지 않는다. 기본
`mm`는 `LMC_Units.MM=10000`이고, `None / raw DINT`는 이미 변환된 정수를
그대로 전송한다. Encoder `ExUnits=8388608`은 PC UNIT 선택 대상이 아니다.

현재 PLC group motion은 static X/Y/Z/U identity 범위다. Move Linear는
`Coordinate=None`, `ExactStop`/`ContinuousDirect`, `Aborting`/`Buffered`만
노출한다. `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 없어 노출하지 않는다.

Group 준비는 `BeginGroupPowerOnWaitForStableStateAsync` ->
`ResumeGroupPowerStateWaitForStableStateAsync`의 Power Ready/ACTIVE 3회 연속 확인 ->
`SetKinTransformCartesian4AxisAsync` ->
`GroupEnableAndWaitForLockedStandbyAsync` -> Move다. helper는 `0x2047`을 한 번만 보내고
`0x2045`의 PowerOn + Enabled/Locked Standby를 기본 3회 연속 확인한다. accepted 뒤
timeout/cancel/status 실패가 발생하면 보존된 continuation으로
`ResumeGroupEnableWaitForLockedStandbyAsync`를 호출하며, 이 재개는 `0x2045`만 보내고
`0x2047`을 replay하지 않는다. 동일 connection/session/group-reference의 다른 handle도 같은
pending/status proof를 공유한다. 성공한 `GroupDisableAsync` ACK 또는 Disabled/Unlocked나
PowerOn=False의 3회 연속 safe-state proof 뒤에만 pending을 명시적으로 해제하고 새 Enable을
허용한다. Enable ACK만으로 lock 완료를 판정하지 않는다.
일반 Group Enable 버튼과 Group Enable qualifier는 둘 다 WPF Group Profile Lock durable
journal을 `0x2047` 전에 arm한다. accepted observer는 ACK와 exact continuation 게시 후
첫 `0x2045` 전 `AcceptedAwaitingProof`를 저장하고, restart는 exact endpoint/group/
`DiagnosticsBootId`/`MapRevision`에서 status-only 검증만 수행한다.
gate 대기, `0x2047`, 모든 status와 delay는 하나의 total deadline을 공유한다. pre-write
cancel/deadline은 `NotAttempted`, zero wire, reusable/no-mutation이다. write 뒤 caller cancel은
response와 accepted evidence를 drain·게시한 뒤 typed cancellation을 반환한다. ACK 무응답은
`OutcomeUncertain`/no-continuation/`Faulted`, status 무응답은
`Accepted`/exact-continuation/`Faulted`이며 둘 다 `TransportInvalidatedAtDeadline=true`다. rejected
ACK는 `Rejected`/no-continuation이다.
종료는 `GroupDisableAsync(Unlock)` -> `BeginGroupPowerOffWaitForStableStateAsync` ->
`ResumeGroupPowerStateWaitForStableStateAsync`의 PowerOn=False 3회 연속 확인 순서다. compound
facade는 각 방향의 Begin+Resume elapsed deadline을 공유한다. 일반 Read Status 한 번은 관찰용이고
pending Power On/Off 완료 proof로 쓰지 않는다.

WPF는 Group Power On/Off 공용 durable journal에 endpoint, group name/reference,
DiagnosticsBootId, MapRevision, 방향과 `ArmedBeforeDispatch`/`AcceptedAwaitingProof`/
`RecoveryRequired`/`Resolved` 상태를 저장한다. fresh command는 journal arm 뒤에만 송신하고 accepted
observer가 첫 status 전에 durable accepted 전이를 기록한다. restart의 accepted record는 exact
identity에서 status-only 확인하며 command를 replay하지 않는다. startup의 Armed record와 실제
outcome-uncertain Power On은 RecoveryRequired로 승격하고, Power On은 status만으로 resolve하지 않고
명시적 Power Off takeover가 durable record를 원자 교체한 뒤 PowerOn=False stable proof를 요구한다.
불명확한 Power Off는 먼저 status-only false proof를 시도하며 typed interference 또는 successful
PowerOn=True 관찰 전에는 replacement `0x204B`를 허용하지 않는다. replacement가 rejected/pre-wire
failure면 이전 durable record와 재시도 권한을 보존한다. active record 동안 endpoint/group identity,
새 mutation과 connected clean Close/reconnect는 fail-closed하고 safety/read-only recovery만 허용한다.

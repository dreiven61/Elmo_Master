# LMC Diagnostics 남은 구현 및 검증 계획

- 기준일: 2026-07-21
- 최종 검토일: 2026-07-30
- 대상: LASAL PLC diagnostics D0-D6, C# SDK, `LasalApiWpfTestApp`,
  `Codex_PMAS_WPF_Version2`, native packet capture 비교
- 현재 test-profile source capability: BootId 0=`0x00004007`, stable BootId=`0x0000613F`,
  `MaxSdoDataBytes=4`; 신규 topology bit 14 포함

## 2026-07-30 CREVIS integrated read-owner checkpoint

- current LASAL source는 `0x7E11/0x7E12/0x7E13/0x7E22`를
  `LMCDiagnosticsService`로 route/구현한다. `LMCEcatInputLatch`는 기존 304-byte prefix를
  유지하면서 CREVIS coupler/input/output 상태와 값을 포함한 coherent 464-byte snapshot을
  publish하고, `Coupler`/`InputSlot`/`OutputSlot` client는 Motion Network에 연결됐다.
- `Phase5TransportClean / IntegratedReadOwnerDormant` SourceOnly와 full/network static은
  PASS했다. fresh LASAL IDE Rebuild/Link는 `0 error(s), 20 warning(s)`, Linker `Done`이고,
  변경 implementation 직접 open smoke도 성공했다. 현재 LASAL IDE PID의
  `CInvalidArgException`은 0건이다.
- 이 단계는 의도적으로 dormant다. capability bit 15 `EtherCATNodeHealth`와 bit 16
  `DigitalIORead`는 OFF이고, `0x7E23` PLC route/handler는 없으며 bit 17
  `DigitalIOWrite`도 OFF다. current PLC download, `0x7E13/0x7E22` live response와 실제
  CREVIS input/output 값 검증은 아직 없다.

## 2026-07-30 SDO Write activation checkpoint

- LASAL `0x7E50`, C# API와 WPF의 exact Int32/4-byte SDO Write 경로를 구현했다.
- source 승인 target은 Axis 1 Gold `UI[24] 0x2F00:24`, `Int32/4`, local range
  `-1073741823..1073741823`의 exact singleton이다. PLC/SDK global gate와 Axis 1 gate는
  `TRUE`, Axis 2..4 gate는 `FALSE`다. 이는 전송 가능한 source/PC/IDE 계약이며, drive program의
  UI[24] ownership과 실제 장비 적용 안전성까지 입증한 것은 아니다.
- WPF는 Write `Completed/Success` 뒤 동일 target/type/length의 exact 4-byte Readback이
  원 Write owner/current session/BootId/MapRevision에서 일치할 때까지 mutation과 Close를
  차단한다. identity mismatch는 Read submit 전에 거부한다. 불명확한 Write outcome은 Read proof로
  quarantine 해제하지 않는다.
- 이 exact readback 계약은 public `LMCSdoWriteVerificationContext`로 SDK에 승격했다. accepted
  Write ticket은 내부 immutable submitted-request provenance를 보존하며 context factory는 supplied
  request와 flags/target/type/length/timeout/value를 exact 대조한다. 같은 owner/session에 bind된
  exact ticket/SubmitCycle/BootId의 `Completed+Success` Write terminal status도 context 생성에
  필수다. guarded sync/async readback submit과 owner/session-bound fresh capability/Read status
  terminal 평가를 WPF도 사용하며 capability observation sequence는 context baseline보다 커야 한다.
  public factory는 current Axis 1 exact singleton SDK allowlist를
  우회하지 않는다. public SDO Write policy 평가는 승인 target의 immutable snapshot과 cached
  blocker matrix만 사용해 wire 0회로 끝난다. WPF readiness는 `EVALUATION_WIRE=NONE`을 표시하고
  PLC bit 9와 SDK target 승인을 독립적으로 평가하며 Axis 2..4 또는 비승인 target은
  `NoApprovedTarget`으로 wire 전에 거부한다.
- SDO 요청은 클릭 시 immutable request로 먼저 고정하므로 submit RPC가 진행 중이거나 accepted
  ticket이 operation slot을 점유해도 다음 요청의 필드는 편집할 수 있다. Write의
  Slave/Index/SubIndex/Type/Length/Value도 편집할 수 있지만 실제 Submit은 선택한 SDK-approved
  target과 exact match일 때만 허용한다. exact Write readback interlock이 남아도 draft 편집은
  허용하되, 실제 Submit은 명시적으로 불러온 exact Readback과 일치할 때만 허용한다. load 전
  draft는 same-session volatile snapshot으로 보존되고 VERIFIED 뒤 untouched exact editor에서만
  복원된다. load 이후 operator edit 또는 reconnect/session drift는 자동 복원보다 우선한다.
- CREVIS topology/health/I/O read와 output ticket은 parse 뒤 exact session/safety-generation
  publication을 사용한다. Recorder `0x7E40/41/49/4B/4C`가 PLC에서 typed 자원을 만든 뒤 결과 적용이
  선점되면 SDK는 exact recovery-only handle/identity/lease를 원 예외에 보존한다. WPF 일반,
  qualification, Double-bank recovery 경로는 SDK context와 publication 뒤 UI-application race 모두에서
  자원을 cleanup scope에 먼저 저장한다. 이 PC 계약은 PLC에서 자원 생성/해제 순서를 확인한 live
  증거가 아니다.
- Recorder lifecycle의 직접 회귀는 동일 handle concurrent Start/Release를 응답 전 barrier에서
  zero-extra-wire로 거부하고, configuration/buffer/recovered/adopted-configuration 네 Release surface의
  `BeforeWire` 선점이 `OutcomeUnverified`가 아니라 usable rollback으로 복원된 뒤 unscoped retry 1회만
  송신됨을 고정한다. WPF는 delayed ordinary Configure의 accepted handle을 recovery-only/manual cleanup
  상태로 보존하고 명시 Release까지 수행한다. manual Double Configure는 ordinary API와 ordinary
  `recorderConfiguration` field 양쪽에서 구조적으로 분리됐고 durable config-only lifecycle
  adapter까지 구현됐다. adapter는 Configure 뒤 Start를 호출하지 않고 exact lease를 저널에
  보존해 명시 Release만 허용한다. `RecorderDoubleManualActionsReady`,
  `RecorderDoubleManualConfigureRouteReady`, `RecorderDoubleQualificationExecutionReady`,
  `RecorderDoubleReconnectRecoveryReady`는 모두 `false`다.
- PC current Debug/Release는 1006/1006 PASS했다. WPF actual-control smoke는 VS2019 MSBuild current
  Debug/Release 278/278 PASS했다. 별도 Debug/Release output build와 LASAL SourceOnly/full static은
  `ExpectedSdoWriteAxis=1`로 PASS했다. smoke는
  bit-14-only CREVIS 자동 표시, configured `INITIAL/UNCHANGED/CHANGED`, endpoint reset,
  failed/stale baseline 보존과 evidence export, 초기 bit 14 OFF 뒤 수동 Reload CREVIS 복구와 일반 RPC 중
  SDO Write editor 유지/Submit 직렬화, 비모달 immutable arm/편집 시 re-arm/exact second-click consume과
  D5 contention 및 abrupt-disconnect
  capability/in-flight start gate를 실제 MainWindow 컨트롤로 검사하지만 PLC나 실제 SDO
  Write를 송신하지 않는다. bits 14~16 fake RPC의 `0x7E13/0x7E22` Health/selected-DI 표시,
  output-shadow background poll 0회, 늦은 수동 응답 selection/session guard, mixed-I/O output
  proof와 Health/DI channel별 stale/error, invalid PI/Bulk raw의 `UNAVAILABLE` 표시를 추가로
  고정했지만 LASAL current source handler의 PLC live 증거는 아니다. 실제 WPF child process의
  SDO/DO unresolved startup, single-writer,
  Close 차단, `0x7E50/0x7E23` zero-replay, 강제종료 재복구와 typed v2 SDO restart recovery의
  Axis 2..4와 비승인 target의 zero-wire도 포함한다. D4 Double active journal의 WPF single-writer
  수명주기, 전역 mutation/Close interlock, 잠긴 journal fail-closed와 두 번의 child process
  강제 종료/restart에서 `0x7E40..0x7E4F` zero-replay 및 byte-identical identity/state 보존도
  포함한다. 추가 smoke는 결정적 Double recovery Guid -> RequestedConfigId와 active journal에서도
  ordinary diagnostics interlock과 분리된 reconnect recovery contract와 semantic journal
  conflict/runtime I/O failure 분리를 검증한다. durable motion 범위는 Move/Stop/Power Off와
  최종 해제 직전 fresh BootId/MapRevision, startup explicit-safety ACK, status-only 해제 금지,
  연속 safe-state proof와 강제 종료 zero-replay를 검증한다. Double
  qualification/retained-cleanup/reconnect/config-only manual Configure adapter는 구현됐지만
  `ManualActions`, `ManualConfigureRoute`, `QualificationExecution`, `ReconnectRecovery`
  proof/route gate는 모두 `false`이며 live wire/PLC/pcap 증거는 없다. generated
  `Classes.lcb`의 신규 Write/read-owner declaration 동기화도 확인됐다. current tracked source를
  다시 연 LASAL IDE에서 Rebuild/Link `0 errors / 20 warnings`, Linker Done을 확인했고,
  변경 implementation 직접 open smoke와 현재 IDE PID의 `CInvalidArgException` 0건도 확인했다.
  `Test2`에서는 static `0x7E11/0x7E12` 7-entry wire 응답을 확인했다. current source에는
  `0x7E13/0x7E22` T2 client/method/network와 464-byte coherent snapshot이 구현됐지만 bits
  15/16은 OFF다. current PLC download와 live Motion/Power/SDO Write 및 dynamic Health/I/O
  response는 미검증이다.
- same-value qualification은 initial baseline Read -> fresh capability -> 첫 safe check -> 운영자
  네 가지 확인 -> unchanged exact pre-Write guard Read -> 최종 두 번째 safe check -> durable journal ->
  동일 4 bytes Write 1회 -> exact guarded Readback 순서다. initial/guard/Write/readback은 서로 다른
  네 ticket을 사용한다. 반환된 Write ticket은 semantic validation 전에 durable journal/quarantine에
  먼저 adopt한다. 두 번째 Read는 race window를 좁히지만 compare-and-write를 원자적으로 만들지
  않으므로 명시적 단일 writer 작업창이 필수다. sentinel, 자동 restore와
  accepted/outcome-uncertain Write replay는 하지 않는다. current Axis 1 exact target만 전송 경로에
  진입할 수 있고 Axis 2..4와 비승인 target의 강제 handler 호출은 zero-wire다. 상세 절차는
  `LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md`를 따른다.
- 실제 운전 승인 전 남은 gate는 Axis 1 UI[24] ownership 확인, current PLC download,
  mailbox/pcap 실행, 명시적 단일 writer 작업창 및 물리 증거다. PLC/SDK 동일 Axis 1 gate와
  LASAL build/smoke는 현재 source에서 활성/PASS했다. durable
  journal/운영자 ACK 정책은 구현됐고 PC child process와 실제 WPF child process의 강제 종료 뒤
  reopen/interlock 및 RPC zero-replay 회귀도 통과했다. 실제 전원 손실 뒤 recovery와 장비에서의
  물리 확인/ACK 절차 검증은 남았다.
- 별도 `parser-stress --seed <u32> --iterations <N>`는 topology info/chunk, node health,
  digital input/output, D5 variable-inline과 Recorder recoverable configure/inventory를 최대
  1,572-byte raw frame으로 total round-robin 변이한다. Release `0x7E4C7E4D`, 100,000회가
  accepted 1,511, exact `InvalidDataException` reject 98,489로 PASS했지만 PC parser robustness 증거일 뿐
  PLC runtime, mailbox 또는 EtherCAT fault 증거가 아니다.

## 2026-07-28 Stop/PowerOff send-priority checkpoint

- SDK coordinator는 기본 강제가 아닌 opt-in이다. 경쟁하는 `LMCConnection`에 같은
  `LMCSendPriorityCoordinator`를 주입하고 logical async flow를 scope로 감싼 경우에만 적용된다.
  `SendPriorityCoordinator=null` 또는 unscoped SDK 호출은 기존 transport 동작을 유지한다.
- WPF의 Axis/Group Stop과 Power Off는 application send gate 대기 전에 safety generation을
  선예약한다. ordinary foreground/diagnostics/qualification은 captured generation을 유지하며,
  `LMCConnection.ExchangeCore`가 각 command의 mutation callback과 `stream.Write` 직전에 마지막
  검사를 수행한다. 새 safety 예약 뒤 아직 쓰지 않은 ordinary RPC와 compound API의 후속 RPC는
  `LMCSendPreemptedException`으로 끝나 해당 command의 wire byte가 0이다.
- 최종 검사를 이미 통과한 in-flight RPC는 transport cancellation으로 끊지 않는다. 그 RPC의
  response/timeout을 확정한 뒤 Stop/Power Off send가 connection 직렬 경로를 얻는다. 앞선 RPC가
  transport를 fault로 전환하면 safety send 성공도 보장하지 않는다. 따라서 이 coordinator는 이미
  전송된 command를 취소하거나 PLC 내부 동작을 선점하는 기능이 아니다.
- safety ACK 뒤에는 app gate를 반환하기 전에 monitor admission을 예약하고, command에서 받은
  exact generation으로 stable Standstill/InPosition 또는 PowerOn=false를 확인한다. 새 safety
  요청이 다시 예약되면 이전 monitor의 다음 RPC는 stale로 거부된다. ACK만으로 정지/전원 차단
  완료를 판정하지 않는다.
- Group Power On/Off는 `0x204A`/`0x204B`를 정확히 한 번 전송한 뒤
  `WaitForPowerStateAsync`가 `0x2045`만 polling하여 기대 `IsPowerOn` 값을 성공 응답 3회
  연속 확인해야 완료다. timeout/cancel/Stop·PowerOff 선점 뒤 pending 검증을 재개할 때도
  `0x2045`만 보내며 원 power command를 replay하지 않는다. 수동 `Read Status` 한 번만으로
  pending Power On/Off 또는 Enable continuation을 완료하거나 ACTIVE/profile lock을 승격하지
  않는다. 다만 safety generation 검증을 통과한 성공 응답은 상태에 맞는 pending Enable
  continuation proof에 누적되고 Locked Standby proof가 3/3이면 기존 ACK를 재사용한 zero-wire
  Resume으로 완료할 수 있다. safety
  예약은 pending Enable의 누적 proof를 즉시 초기화하되 accepted ACK와 continuation을 보존한다.
  예약 뒤 완료된 수동 Group Status response는 drain 후
  `LMCSendPreemptionPhase.ResultDiscarded`되어 observe되지 않는다. 예약 전에 SDK completion
  publication이 끝났지만 WPF 적용 전에 safety가 예약된 좁은 경우만 recovery-required로 승격한다.
  같은 delayed-ACK drain 계약은 Axis Reset, Admin `GroupMoveLinearRelative`와 D5
  `SubmitSdo`/`CancelOperation`에도 적용된다. accepted Submit 실패 context는 exact ticket,
  BootId, MapRevision과 immutable request를 보존하며, Cancel의 늦은 ACK는 stale success로 공개하지 않는다.
  connected unresolved 상태에서는 동일 group 이름 변경, group 재조회, clean connection/window
  close, connected reconnect와 새 Power On을 차단한다. 외부 connection loss 뒤 reconnect 진입에서는
  원 exact group 이름을 보존한 recovery로 승격하고 새 session에서 그 이름의 group만 다시 조회한다.
  accepted pending은 같은 group/session에서 성공한 명시적 `0x2048 GroupDisable` ACK,
  PowerOn=True + Disabled/Unlocked 3회 연속 또는 PowerOn=False 3회 연속 proof로 해제한다.
  recovery-required는 성공한 `0x2048 GroupDisable` ACK 또는 보존한 exact recovery group의
  PowerOn=False 3회 연속 proof로만 해제한다. Power On 성공만으로는 해제되지 않으며 어느 경로도
  `0x2047`을 replay하지 않는다.
- fresh Group Enable은 endpoint IP/port, group name/reference, DiagnosticsBootId와 MapRevision의
  exact identity를 별도 durable journal에 `0x2047` 전에 arm한다. startup Armed record는
  RecoveryRequired로 승격하며, reconnect endpoint는 RPC 전에, BootId/MapRevision은 connect 뒤,
  group reference는 read-only lookup 뒤 검증한다. verified Enable/Disable/PowerOff는 identity
  refresh 뒤 safety generation을 다시 검사하고 durable resolve를 volatile clear보다 먼저 한다.
  mismatch와 post-identity safety reservation은 recovery를 유지하고 `0x2047`을 replay하지 않는다.
- Group Enable stable wait는 mutation/status gate 대기, fresh `0x2047`, 모든 `0x2045`와 poll
  delay를 하나의 total deadline으로 제한한다. final write commit 전 cancel/deadline은
  `NotAttempted`, zero wire, connection reusable, mutation/proof 불변이다. actual write commit의
  `onWriteCommitted`에서만 mutation generation을 갱신하고 pending proof를 0으로 reset한다. write 뒤 caller cancel은 response를 drain하고
  accepted ACK/status를 먼저 게시한 뒤 typed cancellation을 반환한다. ACK no-response는
  `OutcomeUncertain`, continuation 없음, `Faulted`; status no-response는 `Accepted`, exact pending
  continuation, `Faulted`이며 둘 다 `TransportInvalidatedAtDeadline=true`다. rejected ACK는
  `Rejected`, continuation 없음이다. 전용 fake-RPC 회귀
  35개가 통과했으며 PLC runtime proof는 아니다.
- GroupStop accepted wait는 `0x2085`를 정확히 한 번 보내고 accepted continuation 이후에는
  `0x2045`만 polling한다. 마지막 stable Standby proof, cancel, deadline과 per-group mutation
  generation과 session을 coordinator lock 안의 한 결정으로 게시한다. 먼저 관찰된 cancel은 accepted
  evidence와 exact continuation을 가진 `LMCGroupStopWaitCanceledException`, 먼저 관찰된 deadline은
  pending/no-replay `LMCGroupStopWaitTimeoutException`으로 끝난다. proof commit 뒤 late
  cancel/deadline은 성공을 뒤집지 않고 continuation은 `IsCompleted`로 게시된다. 전용 회귀는 34개이며
  PLC 정지 완료 증거와 별도다.
- qualification의 coordinator 선점은 `ABORTED`로 기록한다. SDO submit이 최종 write 전에
  선점되면 tracker는 `Phase=Submission`, `SubmissionOutcome=NotAttempted`, ticket 없음으로 남아
  pre-submission guard를 해제하며 uncertain/accepted ticket으로 보존하지 않는다.
- 현재 증거는 source 대조와 deterministic fake-TCP PC test다. PLC runtime packet order,
  EtherCAT 응답, 실제 정지 시간과 장비 safety certification은 별도 검증 대상이다. packet format,
  LASAL source/declaration/network 및 7-bit ASCII custom-source 규칙은 이 변경으로 바뀌지 않았다.

## 2026-07-29 Axis accepted-once completion checkpoint

- Axis Stop은 `BeginStopWaitForStableStandstillAsync`가 mutation gate 안에서 `0x2022`를
  정확히 한 번 보내고 accepted continuation을 게시한다. Resume은 status-observation gate에서
  `0x2028`만 poll하며 기본 3회 연속 `IsSuccess && IsStandstill`을 확인한다. timeout/cancel,
  status failure, preemption과 ACK 게시 직후 deadline에서도 continuation을 보존하고 Stop을
  replay하지 않는다. stale/superseded/completed continuation과 concurrent second Resume은
  zero-wire로 거부한다. 전용 32개 회귀는 same-axis other-handle/accepted-wait mutation의 typed
  interference, status publication race, pending 보존과 zero-wire/different-axis 비간섭까지 고정한다.
- WPF Axis Stop은 priority safety-send와 preemptible status monitor로 분리했다. ACK가 게시된 뒤
  예외 경계에서도 continuation을 회수해 accepted motion journal을 먼저 남기고 status-only Resume을
  수행한다. monitor 중 새 Stop/Power Off는 허용되며 이전 Stop command는 재전송하지 않는다.
- Axis Reset은 Begin이 `0x2024`를 한 번 보내고 status zero-wire accepted continuation을
  session/send-priority publication 안에서 latest pending으로 설치한다. Resume은 `0x2028`만 보내며
  stable epoch는 다시 시작하고 poll evidence는 누적한다. compound는 gate/ACK/status/delay를 한
  elapsed total deadline으로 제한한다. invalid/concurrent/stale continuation은 zero-wire이고 final
  proof는 session/send-priority/mutation/deadline을 함께 선형화한다. proof commit 뒤의 late
  cancel/deadline은 success를 뒤집지 않고 먼저 관찰된 cancel/deadline은 pending을 보존한다.
- process-local axis mutation generation은 connection session+AxisReference에 묶이며 raw sync/async
  및 accepted-wait `LMCSingleAxis` write의 may-have-been-sent boundary에서 증가한다. PowerOn/Stop/Reset/PowerOff는
  pre-wire/status publication/final resolution에서 원 generation을 확인하고 later same-axis mutation은
  typed interference/pending/no-replay로 끝낸다. intentional post-Reset Power On 뒤에는 명시적 새
  Reset이 필요하다. 외부 PLC/client, direct SDO와 group operation은 범위 밖이다.
- Axis PowerOff Begin은 `0x2023(enable=false)` ACK, mutation generation과 accepted continuation을
  session/send-priority publication 안에서 원자적으로 설치한다. Resume은 pre-wire/status
  publication/final resolution에서 generation을 확인하고 later same-axis mutation이면
  `LMCAxisPowerOffInterferenceException`, expected/observed/intervening evidence, pending/no-replay로
  끝낸다. final cancel/deadline/generation은 proof publication과 선형화하며 전용 회귀는 35개다.
- WPF Reset은 accepted continuation을 command gate 반환 전에 저장하고 failure/preemption 뒤
  status-only Resume한다. unresolved 동안 Resume은 열고 새 live mutation은 막으며, confirmed
  interference 뒤에만 사용자의 explicit replacement Reset을 허용한다. session cleanup은 volatile
  pending을 지우며 durable cross-reconnect Reset proof는 제공하지 않는다.
- WPF Axis PowerOff는 monitor 실행 중 재클릭을 zero-wire로 막되 Stop은 계속 허용하고, transient
  timeout/cancel/status failure 뒤에는 원 continuation의 status-only Resume만 수행한다. SDK가 typed
  interference를 확인한 경우에만 `Power Off Again (Confirmed Interference)`가 replacement `0x2023`을
  허용한다. accepted replacement는 원 pending을 대체하고 flag를 지운다. ACK 거절은 원 pending,
  confirmed flag와 label, Load Axis·이름 편집 차단을 그대로 보존하며 cleanup은 pending/flag를 지운다.
- Axis Power On의 gate, ACK, status와 poll delay는 하나의 total deadline을 공유한다. submission은
  `NotAttempted/Rejected/OutcomeUncertain/Accepted`로 구분하고 ACK, 마지막 status, poll/stable
  count와 `TransportInvalidatedAtDeadline`을 typed evidence로 보존한다. post-write ACK 무응답은
  continuation 없는 `OutcomeUncertain`, accepted 뒤 status 무응답은 exact pending continuation을
  남기며 둘 다 connection을 `Faulted`로 전환한다. 최종 pre-write 취소는 zero-wire
  `NotAttempted`이고 connection을 재사용한다.
- Power On write의 may-have-been-sent generation을 ACK와 exact continuation에 묶고 Resume/status/final
  publication에서 재검사한다. ACK parse 뒤 continuation publication 전에 같은 축 mutation이 wire에
  도달한 경계도 `LMCAxisPowerOnInterferenceException`과 pending/no-replay로 끝난다. final stable
  status, cancel, deadline과 generation은 한 coordinator 결정으로 선형화한다. proof commit 뒤의
  late cancel/deadline은 이미 게시된 성공을 뒤집지 않는다.
- restart용 Axis `WaitForPowerStateAsync`는 `0x2028`만 사용하고 ACK를 재사용했다고 표시하지 않는다.
  current PC 자동 검증은 SDK Debug/Release 1006/1006, WPF VS2019 MSBuild Debug/Release smoke 278/278,
  LASAL SourceOnly/full static PASS다. 이것은 fake-RPC/정적 증거이며 실제 PLC download,
  DS402 상태와 물리 축 정지·전원 상태
  검증은 남아 있다.

## 2026-07-27 CREVIS topology 및 digital I/O checkpoint

- 현재 working source의 configured physical order는 `GL_9086_11(SlaveIndex 0) ->
  Elmo_11..41(SlaveIndex 1..4)`인 5-slave 구성이다. 이것은 source/ENI/network 확인 결과이며
  current LASAL Rebuild/Link는 PASS했지만 PLC download와 실제 I/O PASS는 아니다.
- slave 순서/identity, slot module, PDO index/sub-index, I/O 폭과 generated process-image
  mapping은 configured topology에 고정한다. Online/EtherCAT state/AL status, value와
  valid/fresh/stale quality만 runtime에 변한다. 물리 순서를 바꾸면 ENI/network를 다시
  생성해야 하며 API가 runtime discovery로 schema를 바꾸지 않는다.
- 기존 `0x7E10 ReadEtherCATHealth`의 exact 200-byte, 4-entry Elmo subset은 유지한다.
  기존 wire `SlaveIndex=0..3`은 호환용 legacy drive index다. actual physical node index
  0..4는 신규 topology API에서만 제공한다.
- C# SDK contract command는 `0x7E11/0x7E12` topology info/chunk, `0x7E13` node health,
  `0x7E22` digital I/O read, `0x7E23` output write submit이다. model/parser/golden과 capability-off
  pre-wire 검증은 구현했다. LASAL source에는 7-entry `0x7E11/0x7E12` static serializer와
  `0x7E13/0x7E22` read handler/route가 있으며, latch의 coherent 464-byte snapshot과 CREVIS
  coupler/input/output client도 연결됐다. revision은 `0x15867EEC`, advertised chunk limit은 1이다.
  bit 14만 활성이고 bit 15/16은 dormant/OFF다. `0x7E23` PLC route/handler는 없고 bit 17도 OFF다.
- output write는 GT-22BA output slot-module의 configured `IOReference`와 valid mask만 허용한다. whole-word와 atomic
  masked write를 하나의 PLC RT owner가 적용하고, non-RT diagnostics service는 owner/session,
  BootId/topology revision과 ticket을 소유한다. validation 실패, stale/offline/not-OP,
  mailbox/owner 불가와 uncertain outcome은 fail-closed하며 자동 replay하지 않는다.
- SDK는 facade에서 읽은 Output snapshot에 owner/session/source capability/BootId provenance를
  고정하고 그 snapshot factory로 만든 request만 submit한다. detached raw request, reconnect 뒤
  stale snapshot과 source/fresh BootId 불일치는 `0x7E23` 송신 전에 거부한다.
- 개발 WPF에는 Value/Mask, 직전 output revision, 명시적 확인, ticket terminal 뒤 exact
  output-shadow readback을 갖춘 guarded write 화면이 있다. 현재 bit 17과 SDK allowlist가
  닫혀 있어 submit은 비활성이다. submission 응답 유실, disconnect, terminal-success 뒤
  readback 실패나 identity/schema/full-shadow 불일치는 unresolved mutation으로 유지해 신규
  mutation과 Close를 차단한다. 자동 replay는 없고 운영자 acknowledgement는 물리 확인 뒤 GUI
  interlock만 해제하며 write 성공을 입증하지 않는다. SDK failure context가 미송신 또는
  RPC/PLC의 명시적 거절을 증명할 때만 GUI의 pre-armed interlock을 해제한다. fake TCP는
  sync/async accepted, 명시적 RPC rejection, response-loss outcome-uncertain과
  accepted-session-race를 검증한다.
- SDO Write와 digital output write는 dispatch 전에 durable journal을 `ArmedBeforeDispatch`로
  기록하고 accepted, terminal, readback 상태를 순차 저장한다. format v2 SDO record는
  Slave/Object/SubIndex/Type/Length/Timeout과 expected bytes를 checksum 범위에 보존한다. active record는
  `%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\DiagnosticsMutationJournal\v1`에서 crash/restart
  뒤 복구하며 Write를 자동 replay하지 않는다. legacy v1과 미승인 target은 protocol recovery를
  zero-wire로 거부한다. terminal-success v2 record가 current SDK allowlist의 exact target과
  일치할 때만 운영자가 1회 read-only recovery를 실행한다. Read 전후의 fresh BootId/MapRevision과
  exact bytes가 일치하고 같은 record/state의 atomic CAS가 성공하면 durable `Resolved` tombstone을
  먼저 쓰고, 그 외 record는 물리 확인 checkbox와 명시적
  ACK로만 해소한다. ACK는 write 성공 증거가 아니다.
  journal open/runtime fault와 두 번째 writer에서는 새 live/mutation command와 tracked D5
  read를 차단하고 Stop/PowerOff/Group Stop은 유지한다. 정상 종료는 active durable evidence가
  없을 때만 허용하고, active evidence가 남으면 connection/window Close도 차단한다.
- UI 독립 `DiagnosticsOperationAdmissionPolicy`는 이 조건을 immutable state와 작업 종류별
  reason으로 판정한다. 일반 tracked D5 submit, live/mutation, Connect/Reconnect,
  connection/window Close와 정상 qualification은 UI enablement와 handler에서 같은 decision을
  다시 평가한다. 필수 exact SDO Write readback은 pending, operation slot, D5/DO conflict와 원
  connection session을 만족할 때만 좁게 허용하고, 기존 fresh BootId/MapRevision 및
  durable-resolve-before-clear 검증에 restart post-read identity와 atomic CAS를 추가한다.
- WPF `Load Topology`는 capability를 먼저 다시 읽고 `0x7E11/0x7E12` configured topology를
  한 동작으로 로드한다. 따라서 별도 `Refresh Capabilities` 선행 클릭은 필요 없다. 이것은
  CREVIS 구성 정보 표시 개선이며 bit 15~17이 off인 현재 live Health/DI/DO 증거는 아니다.
  현재 실행 파일이면 제목과 startup log에 `CREVIS topology / editable SDO draft` marker가 보인다.
  이 marker가 있는데도 구성 7행/CREVIS 3행이 없으면 PLC bit 14 및 `0x7E11/0x7E12` 응답을
  먼저 확인한다. T2 read-owner 구조가 source에 있어도 bit 15/16 활성화와 current PLC live
  qualification 전까지 live 열이 `UNAVAILABLE`인 것이 정상이다.
- `Verify-LasalContract.ps1`는 `Eni.xml`의 GL-9086 -> Elmo 4대 order/identity/physical address,
  CREVIS 32-bit process image와 PDO를 EtherCAT network의 SlaveIndex/slot/device/connection,
  7-entry serializer/revision과 교차검증한다. full static은 generated
  `ONE_EtherCAT_Network_Table.st`의 vendor/product/SlaveIndex/slot PDO도 확인한다. ENI,
  network, serializer와 generated table을 변조한 9개 negative fixture는 모두 거부한다.
  이 gate는 configured source drift를 빌드 전에 잡는 검사이며 runtime discovery나 live
  Health/DI/DO 증거가 아니다.
- WPF auto live monitor는 `CFG` static topology와 `LIVE` health/DI 열을 분리한다. bit 15 또는
  bit 16이 있을 때만 owner/session-bound cached capability snapshot을 pinned SDK overload에 넘긴다.
  eligible tick의 실제 wire는 추가 `0x7E00` 없이 `0x7E13` 또는 `0x7E22` 1회다. 일반 non-pinned
  API의 capability refresh+read 계약은 유지한다. 7-node health와 selected DI를 순환하고,
  foreground/safety/qualification/in-flight 중에는 건너뛴다. 현재 bit 15/16이 모두 off이므로
  wire request는 0회다. background monitor는 output shadow를 읽거나 write-authorizing shadow를
  갱신하지 않는다.
- `LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md`의 client/method/network 구조는
  current tracked project에 생성됐고, `0x7E13/0x7E22` route/handler와 coherent read owner까지
  `IntegratedReadOwnerDormant` 정적 계약 및 IDE Rebuild/Link로 확인했다. capability bits 15/16은
  PLC download와 raw live qualification 전까지 OFF로 유지한다.
- internal qualifier의 `--scope topology-inventory`는 현재 bit 14 단계에서 capability identity
  전후 일치와 exact `0x15867EEC` 7-entry order/identity/CRC를 검증한다. raw allowlist는
  `0x7E11/0x7E12`뿐이고 총 8개 request만 전송하며 `0x7E13/0x7E22/0x7E23`은 전송하지 않는다.
  PC contract와 별개로 `Test2` pcap에서 같은 static wire sequence와 7-entry 응답은 확인했다.
  qualifier가 생성하는 durable PLC report는 아직 없다.

구현 순서는 다음으로 고정한다.

| 순서 | 구현 | capability |
|---:|---|---|
| IO-0 | current 5-slave LASAL Rebuild/Link와 실제 configured order 확인 | 사용자 build PASS 보고, `Test2` static inventory wire PASS; dynamic bits off |
| IO-1 | C# model/protocol/golden과 capability-off facade | 완료, 모두 off |
| IO-1B | PLC topology handler route와 exact envelope | `0x7E11/12` source 구현, bit 14 활성; `Test2` wire response 확인 |
| IO-2 | configured topology info/chunk와 revision | `0x15867EEC` 7-entry `Test2` live response 확인; durable qualifier report 대기 |
| IO-3 | node health와 DI/output-shadow coherent 464-byte snapshot, `0x7E13/0x7E22` route와 CREVIS read-owner wiring | source/static/IDE build 완료; bit 15/16은 각각 PLC runtime PASS 뒤 활성 |
| IO-4 | RT single-writer, whole/masked mailbox와 `0x7E23` ticket | bit 17 off |
| IO-5 | invalid mask/offline/stale/contention/response-loss/RT 및 physical output matrix | bit 17을 최종 활성 |

exact field layout, local Elmo API 근거와 수정 파일은
[LMC EtherCAT Topology 및 Digital I/O API 설계](LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md)를
기준으로 한다.

## 1. 결론

현재 Git source와 working implementation 기준으로 D0-D3, D4 single-bank Ring/Trigger와
D5 general-inline SDO Read 실행부가 구현돼 있다. test-profile source는 D5 bit 8
`SDORead`, bit 13 `SDOReadGeneralInline`과 `MaxSdoDataBytes=4`를 광고하도록
활성화했다. gate-on 첫 runtime은 same-cycle immediate
timeout으로 실패했고 request-local/class-member shadowing을 수정했다. 후속 download의
Slave 1~4 happy path는 43~54 cycles 뒤 Completed/Success와 UInt32 4-byte 결과로
PASS했다. 이것은 legacy `0x1000:0` fixed-vector runtime 증거다. nonzero Index,
Sub-index 0-255와 exact typed 1/2/4-byte general-inline source는 구현됐다. 과거 BootId 6
general-inline 캡처의 `ResourceBusy(9)` 결함은 callback ordering과 owned completion
회수 source에서 수정했다. 이후 `10_DriveRead_Axis1to4.pcapng`에서 general-inline
Int8/1-byte와 BitField16/2-byte가 전 축 Completed/Success로 확인됐다. general-inline
UInt32/4-byte는 `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서
Completed/Success를 반환했다. 같은 BootId 8에서 UInt16/2-byte TypeMismatch 후
Int8/1-byte가 성공해 executor 무재부팅 recovery도 확인됐다. SDO abort/offline,
queued cancel live, disconnect/orphan 등 fault 증거와 active contention 및 timeout/drain의 실제
PLC packet은 남았다. contention runner/보존형 PC 계약 12개, timeout -> exact Expired ->
bounded drain -> recovery와 queued Cancel -> exact Cancelled/race -> recovery PC 계약은 구현됐다.
D4 Double bank는 두 개의 1.28 MB 고정 bank, bank별 identity/metadata, full Busy,
exact all-bank rebind, isolated release와 RT/non-RT generation handshake까지 dormant source로
구현됐다. store gate와 capability bit 6/count 2는 계속 꺼져 있으며 LASAL build, RAM/jitter와
live qualification은 남았다. 기존 D6 계획에는 별도 wire 없이 D1/D2를 재사용하는 instance
기반 PI/Bulk compatibility facade가 구현됐다.
`GetSignalCatalog[Async]`와 `GetEtherCATTopology[Async]` aggregate는 diagnostics owner와
connection session generation에 bind된다. alias PI Read, Bulk builder 생성/Configure, PI Write
submit과 topology-bound Health/Digital I/O는 unbound, foreign, reconnect-stale aggregate를
capability/data RPC 전에 거부하고 원 session generation을 exchange까지 유지한다. 로컬
Catalog/topology 조회와 raw topology/I/O observation overload는 유지한다.

이번 작업에서 Health 화면 예외, Recorder Stop 완료 경쟁과 Download/CSV 용어 혼동을
수정했다. `LMCEcatInputLatch1` 중복 주기 실행 의심은 HEAD/current XML 재비교 결과
실제 회귀가 아닌 것으로 정정했다. 23개 PMAS/MMCLib native
capture도 분석해 PMAS Version2 Recorder의 ready/header/range gate와 PI 선택 변환을
보완했다. 이 capture에는 custom `0x7Exx` packet이 없으므로 LASAL diagnostics 실기
증거로 사용하지 않는다.

2026-07-22 `LMCSdoExecutor : EtherCAT_SDOBase`, 축별 executor 4개, service one-ticket
실행부와 두 network의 연결을 구현했다. Recorder terminal Stop 멱등 처리도 유지했다.
PC 자동 시험 148/148, WPF Debug/Release build와 각 3초 startup smoke 및 현재 수정 LASAL SourceOnly/
full static 계약이 통과했다. `Classes.lcb`의 `TryStartRead` declaration도 current source와
동기화됐다. 10:53 IDE
Rebuild/Link는 gate-off baseline 결과다. gate-on 첫 D5
runtime은 Ticket 11 same-cycle Expired/TimedOut으로 실패했지만 수정 후 BootId 5의
Slave 1~4 Ticket 5~8은 모두 Completed/Success로 통과했다. 이후 BootId 6 general-inline
Submit은 `ResourceBusy`로 실패했고 callback recovery source를 수정했다. 수정본의
general-inline 1/2-byte runtime은 `10_DriveRead_Axis1to4.pcapng`에서 PASS했다.
`12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서 general-inline UInt32/4-byte
성공과 같은 BootId TypeMismatch 후 Int8/1-byte 복구도 PASS했다. D5 전체 fault
matrix, D1/D2 fault/soak와 D3/D4 실장 시험은 아직 남아 있다.

이 문서에서 다음 표현은 구분한다.

- `구현됨`: source와 wire 계약이 존재하고 정적 검증 대상에 포함됐다는 뜻이다.
- `활성`: PLC가 capability bit를 광고하도록 source가 구성됐다는 뜻이다.
- `PLC 검증 완료`: 변경 source를 실제 PLC에 download하고 장비 조건에서 결과를
  확인했다는 뜻이다. D1/D2 happy path packet은 확보했지만 fault/soak까지 포함한
  단계 전체 완료 판정은 아직 D1-D4 어느 단계에도 사용하지 않는다.

## 2. D0-D6 현재 상태

| 단계 | 현재 상태 | 구현 범위 | 남은 작업 |
|---|---|---|---|
| D0 | 구현됨 | common envelope, capability, BootId, C#/PLC dispatcher | 회귀 시험 유지 |
| D1 | internal test source 활성; static topology wire PASS; integrated read owner dormant | legacy 4-drive Health, 24-entry Catalog, PI Read, RT latch/seqlock, 7-entry static EtherCAT topology; C# Catalog/Topology owner-session provenance; one-slave Bulk와 4축 Health/StatusWord PI baseline-fault-recovery 교차 판정, invalid raw `UNAVAILABLE` 표시; `Test2`에서 bit 14와 `0x7E11/12` exact 7-entry 응답 확인; current source의 464-byte coherent snapshot, `0x7E13/0x7E22` route와 CREVIS coupler/input/output wiring은 source/static/IDE build PASS | 변경된 `0x7E20` DetailCode의 실제 offline/recovery PLC 검증; topology durable qualifier report; current PLC download와 node health/I/O raw live qualification 뒤 bit 15/16 활성 |
| D2 | internal test source 활성 | 최대 24-entry Bulk configure/status/snapshot/release | same-cycle 및 부하 PLC 검증 |
| D3 | internal test source 활성 | 1,280,000-byte single bank, 최대 24채널 Manual Recorder, download/adopt/release | RAM, jitter, 장시간 upload, reconnect PLC 검증 |
| D4 | single-bank Ring/Trigger 활성; C# Double fail-closed contract | pre-trigger ring, Edge/Window/Mask, forced trigger, chronological upload; Double enum/identity/capability-off validation | trigger PLC 검증, 실제 2-bank PLC storage/ownership 및 WPF live qualification 구현 |
| D5 | general-inline Read source 구현; legacy 4-byte와 수정본 1/2/4-byte 성공 pcap PASS, TypeMismatch 후 same-Boot recovery PASS; public `ReadSdoInline[Async]` bounded terminal facade와 immutable `LMCSdoReadResult` 구현; deliberate contention/timeout/queued-cancel runner code test PASS; disconnect/orphan UI 독립 코어 회귀 28개와 MainWindow abrupt-disconnect application-recovery adapter 및 fake-RPC 2-session full-handler smoke 구현, Debug/Release code/test PASS | 4축 derived executor, 한 ticket, nonzero Index/any SubIndex, typed 1/2/4-byte inline status, queued cancel, timeout/orphan drain; public facade는 capability preflight -> submit -> bounded status poll 뒤 exact owner/session terminal을 판정하고, 이미 수신한 terminal 성공/실패를 늦은 PC cancellation보다 우선한다. nonterminal cancel/timeout은 accepted ticket과 exact `LastObservedStatus`를 보존한다. WPF는 별도 `Read SDO Inline (wait terminal)` 버튼으로 성공 ticket/status/typed/raw를 한 번에 표시하고, accepted timeout/cancel은 기존 수동 cleanup으로 넘긴다. local TCP zero-linger close/no-RPC-`0x405D`, distinct new connection과 fresh owner/session-bound capability, exact `0x6061:0` two-ticket recovery, stable Boot/Map/build/bits/cycle/payload contract, request-timeout+5초/25 ms/exact Rejected-ResourceBusy-only monotonic retry-admission budget, PASS-log-before-clear, proof commit 뒤 late-cancel 무시, GUI adoption 뒤 다른 revision의 CREVIS auto-load와 quarantine clear를 two-session PC E2E로 확인; 결과는 Running/Queued 모두 `ApplicationRecoveryOnly`/`orphanQualified=false`; exact Axis 1 `0x2F00:24 Int32/4` SDO Write 실행/API/GUI와 durable policy, four-ticket same-value qualification, returned Write ticket adoption-before-validation과 changed pre-Write zero-mutation 계약 활성; Axis 2..4와 비승인 target은 fail-closed; PI Write C# request/API와 Catalog provenance는 fail-closed | current source/PC contract와 LASAL Rebuild/Link/implementation smoke PASS. PLC `MarkOrphan`/executor token/late-callback drain durable witness와 실제 owner-loss live PLC/pcap을 포함한 Read fault matrix 및 contention/timeout/queued-cancel live packet, public facade의 실제 PLC 재확인, current PLC download, Axis 1 UI[24] ownership 확인, 명시적 single-writer 작업창 및 same-value Write/readback의 mailbox/pcap/물리 증거 후 production 승인; PI Write PLC handler/allowlist/live qualification은 off |
| D6 | Closed - current release Not Planned | Phase 1 D1/D2 기반 PI/Bulk instance facade가 owner/session provenance와 reconnect-stale pre-wire 거부를 포함해 현재 사용 목적을 충족 | 이름이 정해진 Elmo-style compatibility 소비자가 실제로 생길 때만 별도 milestone로 재개; 현재 릴리스 작업 없음 |

current C# protocol inventory는 62개 고유 ID이고 LASAL TCP route는 61개다. route 61개는
capability-advertised active 53개, dormant read-owner `0x7E13/0x7E22` 2개,
reserved/dormant 6개로 나뉜다. C# contract에는 있으나 LASAL route가 없는 ID는
`0x7E23` 하나다.

현재 정상 retained BootId 경로의 capability는 다음과 같다.

```text
CapabilityBits       = 0x0000613F
MapRevision          = 0x957F101E
CatalogEntryCount    = 24
RecorderBufferCount  = 1
MaxSdoDataBytes      = 4
```

즉 bit 0-5, bit 8 `SDORead`, bit 13 `SDOReadGeneralInline`과 bit 14
`EtherCATTopology`가 source에서 활성이다. bit 13은 bit 8과 `MaxSdoDataBytes=4`를
요구한다. bit 6 `RecorderDoubleBank`, bit 7
`PIWrite`, bit 9 `SDOWrite`, bit 12 `ExtendedSdoResultChunk`는 0이다.

## 3. 이번 작업에서 완료한 수정

### 3.1 EtherCAT Health WPF 예외

`HealthSlaveRow.Online`은 읽기 전용 속성이다. DataGrid checkbox의 기본 TwoWay
binding을 `Mode=OneWay`로 바꿨다. `Grid.IsReadOnly=True`만으로는 binding mode가
바뀌지 않으므로 이 수정이 필요하다.

### 3.2 Recorder Stop 완료 경쟁

Stop 전 authoritative status를 읽는다. 이미 terminal 상태이면 Stop command를 보내지
않는다. status 확인 직후 PLC가 자연 완료되는 TOCTOU 구간에서 Stop이
`InvalidState/DetailCode=19`를 반환하면 status를 다시 읽고 `Ready` 또는 `Uploading`인
경우에만 자연 완료로 처리한다. Fault 및 다른 오류는 숨기지 않는다.

### 3.3 Recorder Download와 Export CSV 표시

- `Download`는 PLC의 frozen sample을 WPF 프로세스의 PC 메모리로 가져온다. 이 단계는
  파일을 생성하지 않는다.
- `Export CSV`는 메모리에 내려받은 sample을 사용자가 Save dialog에서 선택한 경로에
  파일로 쓴다.
- 완료 메시지에 실제 CSV 경로를 표시한다.

### 3.4 `LMCEcatInputLatch1` network 재검토 정정

최초 dirty `.lcn` 검토에서 `LMCEcatInputLatch1`에 독립 `RealTime=1 ms` task가
추가됐다고 잘못 판정했다. HEAD와 current XML 모두 이 객체에
RealTime/Cyclic/Background scheduling 속성이 없다. 현재 실행 경로는 이미
`_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr` 하나이고
해당 topology 검사는 이전 full-network 계약에서 PASS했다. 현재 `Classes.lcb`도
`TryStartRead` declaration과 동기화돼 full static suite가 PASS한다.
`.lcn/.lcb`의 당시 layout diff는 연결·scheduling 변경이 아니라
IDE 시각 배치 metadata이므로 기능 수정으로 커밋하지 않는다.

### 3.5 native capture와 PMAS Version2 정렬

제공된 23개 capture는 모두 PMAS/MMCLib native port 4000 호출이다. Custom LASAL
diagnostics `0x7Exx` packet은 포함하지 않는다. 분석 결과를 다음처럼 반영했다.

- native PI Bulk 대응을 `MMC_ConfigureBulkReadPI (0x1102)`와
  `MMC_PerformBulkReadCmdPI (0x1103)`로 확정했다. Generic parameter Bulk
  `0x10C9/0x10CA`는 custom D2 범위 밖이다.
- PMAS Health에 port 0-3의 InvalidFrames counter를 포함했다.
- PMAS Recorder의 checked PI를 native `uiRv/uiRc`로 변환하는 local helper를 추가했다.
- `uiSr` ready mask, global header `Rl`, selected buffer와 `[From..To]` 범위를 확인하기
  전에는 Header/Download를 실행하지 않는다. 실패한 RPC 뒤 stale cache도 재사용하지 않는다.
- capture로 확인된 SDO 성공 범위는 legacy `0x1000:0` UInt32/4-byte와
  `10_DriveRead_Axis1to4.pcapng`의 general-inline `0x6061:0` Int8/1-byte,
  `0x6041:0` BitField16/2-byte, `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`의
  `0x1018:1` UInt32/4-byte다. 같은 12번 capture에서 TypeMismatch 후 Int8/1-byte
  무재부팅 recovery도 확인했다. 8/12-byte와 Write는 계속 별도 gate 뒤에 둔다.

PMAS Version2의 `uiSr=0` 차단과 `0x0104` ready flow는 source/build 수준으로만
확인했다. 실제 controller UI smoke는 남아 있다.

### 3.6 D5 capability-gated dispatcher와 실행부

`0x7E03 GetOperationStatus`, `0x7E04 CancelOperation`, `0x7E50 SubmitSDO`를 default
case에 맡기지 않고 `LMCDiagnosticsService`의 명시적 handler와 one-ticket 실행부로
구현했다.

- `0x7E03/0x7E04`는 exact 16-byte request만 구조적으로 유효하다.
- `0x7E50`은 32-byte header, OperationFlags 0/1, reserved zero와 read/write별 정확한
  payload 길이를 검증한다.
- malformed shape는 `BoundsInvalid`이고, 구조적으로 유효한 general-inline request는
  gate-on test source에서 ticket 실행 경로로 들어간다.
- stable BootId에서 current source capability는 `0x0000613F`, `MaxSdoDataBytes=4`다.
  여기에는 static EtherCAT topology bit 14가 포함된다. bit 13은 bit 8과 MaxSDO=4가
  함께 있을 때만 유효하다. BootId가 0이면 topology와 stateless D1만 남은
  `0x00004007`이고,
  Diagnostics client가 없으면 MaxSDO는 0인 fail-closed 응답을 유지한다.
- PC 회귀는 bit 13이 없을 때 legacy `0x1000:0` UInt32/4-byte만 허용하고, bit 13이
  있으면 supported ValueType과 정확히 일치하는 1/2/4-byte general request를 허용하며
  8-byte read를 송신 전에 거부하는 경계를 포함한다.
- 같은 general-inline read를 사용하는 source-only drive facade는 `0x6041:0`
  BitField16/2의 bit 3을 `HasDs402Fault`로 표시하고, 별도
  `GetDriveErrorCode[Async]`가 `0x603F:0 UInt16/2`를 한 ticket으로 읽는다. 새 opcode나
  LASAL 구조는 없다. `0x2028 StatusWord=0`, LASAL AxisErrorId, DS402 Fault와
  `0x603F`는 각각 별도 관측이며 실제 Reset 전후 PLC/drive capture가 남아 있다.

위 세 handler와 executor/service 실행부는 최신 정적 계약을 통과했다. current Axis 1 gate-on
source를 다시 연 LASAL IDE Rebuild/Link는 `0 errors / 20 warnings`, Linker Done이며
implementation smoke와 신규 `CInvalidArgException` 0건도 확인했다. 어느 결과도 PLC mailbox
동작 성공 증거가 아니다.

### 3.7 `EtherCAT_SDOBase` 파생 executor 구현

사용자가 추가한 `EtherCAT_SDOBase`와 축별 object/network를 검토했다. plain base의
수동 `Para*` channel을 운영 API로 쓰지 않고 다음 구조를 채택했다.

- `LMCSdoExecutor : EtherCAT_SDOBase` 파생 class와 축별 4개 instance
- 파생 class는 inherited `toSlave`와 actual-length callback만 재사용하는 transport adapter
- `ParaReadWrite::Write`를 override해 manual SDO 시작 경로 차단
- private 4-byte buffer와 cross-task safe callback mailbox 사용
- D5 ticket, BootId, session owner, timeout/cancel은 `LMCDiagnosticsService`가 전담
- physical Running cancel은 지원하지 않고 queued-only cancel과 orphan drain 적용
- `LMCSdoExecutor1..4.toSlave -> Elmo_11..41.ClassState`와
  `LMCDiagnosticsService1.SdoAxis1..4 -> LMCSdoExecutor1..4.ClassState` 연결
- plain `EtherCAT_SDOBase1..4` 제거, executor object의 visualization/remote surface 차단

정확한 class, state machine, wire validation과 검증 gate는
`LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`를 기준으로 한다.

### 3.8 general-inline ResourceBusy 회귀와 executor recovery 수정

`SDO_Test_Error.pcapng`에서 BootId 6 PLC는 `0x213F`, MaxSDO=4를 정상 광고했지만
캡처된 두 Submit은 ticket 전 `ResourceBusy(9)`로 실패했다. 실제 wire request는
`0x6061:0` UInt16/2와 Int8/1 두 건이다. `0x6041:0`, `0x1018:1`, accepted ticket과
status는 이 capture에 없다.

wire DetailCode 9는 service active/drain gate와 executor non-reusable gate를 구분하지
않는다. 최초 callback도 capture 밖이므로 정확한 최초 trigger는 추정으로 남긴다. 다만
실패 당시 source에서 다음 결함은 직접 확인했다.

- vendor call 뒤에야 `Running`을 publish하는 callback race window
- owned callback validation failure가 `Quarantined`로 된 뒤 `Idle`로 회수되지 않는 경로
- orphan callback의 validation failure가 adapter 회수를 막을 수 있는 경로

수정 source는 vendor call 전에 `Running`을 publish한다. request 미접수와 owned
completion cleanup에는 내부 `Releasing=6`을 사용한다. owned validation failure는
`ResultReady`로 publish해 service가 terminal Failed로 보고한 뒤 release하고, orphan
callback은 public 결과 없이 release한다. active token이 없는 unsolicited/duplicate
  callback과 token/atomic ownership 불일치만 hard quarantine한다. SourceOnly/full static
  계약은 PASS했고 `Classes.lcb` declaration도 동기화됐다. 최신 IDE Rebuild/Link와
  implementation smoke도 PASS했다. 수정 source의 PLC download와 같은 BootId TypeMismatch 후 한 번의
재사용은 12번 capture로 PASS했지만, 더 넓은 연속/fault 재사용 matrix는 남았다.

## 4. 구현 우선순위

단계 번호는 설계 분류이고 아래 `P0-P5`는 실제 작업 순서다.

| 우선순위 | 작업 | 완료 조건 |
|---|---|---|
| P0 | LASAL source 회귀와 D5 shadowing 수정 완료 | D5 실행 source, Recorder terminal Stop, request-local 수정, SourceOnly/full 정적 계약과 current Axis 1 gate-on IDE Rebuild/Link·implementation smoke 통과 |
| P1 | executor 명시 초기화 완료, 최신 LASAL source PLC 검증 대기 | IDE `LMCSdoExecutor` constructor declaration/`@STD` wiring과 private state 명시 초기화, `ExpectedSdoWriteAxis=1` 정적 검증 및 gate-on source Rebuild/implementation smoke는 PASS했다. current PLC download 뒤 5절의 D1-D4 행과 D5 재시험을 통과해 packet/trace 결과를 보존해야 한다. |
| P2 | D5 general-inline SDO Read-only | 한 ticket/4축/1·2·4-byte inline source와 bit 8+13 광고, public bounded facade 및 WPF one-click terminal Read 구현 완료. terminal-before-cancel과 nonterminal cancel/timeout `LastObservedStatus` 보존을 PC에서 검증했다. legacy 4축, general-inline 1/2/4-byte packet success와 TypeMismatch 후 same-Boot recovery는 확보했으며, 실제 facade packet과 나머지 fault/cancel/orphan matrix가 잔여 gate다. |
| P3 | D4 Double bank | 두 고정 bank의 dormant source와 정적/PC reference 계약, WPF durable journal open/lock/status/interlock/restart zero-replay 및 qualification/retained-cleanup/reconnect/config-only manual Configure adapter 구현 완료. 네 proof/route gate는 `false`. LASAL build, 2.56 MB RAM, RT jitter, A upload/B capture, exact reconnect/release 실기와 pcap 통과 후에만 bit 6/count 2 및 gate 활성화 |
| P4 | D5 SDO Write activation qualification | Axis 1 exact `0x2F00:24 Int32/4` 실행/API/GUI와 PLC/SDK 이중 allowlist, type/range/state/owner, durable journal 및 initial baseline -> fresh capability -> safe -> 4 confirmations -> unchanged pre-Write guard -> final safe -> byte-identical Write 1회 -> exact Readback 실행기가 source/PC에서 활성이다. four-ticket, returned-ticket durable adoption-before-validation, sentinel/자동 restore/replay 없음. constructor readiness, SourceOnly/full static, IDE Rebuild/implementation smoke도 PASS했다. current PLC download, Axis 1 UI[24] ownership 확인, single-writer 작업창과 mailbox/pcap/물리 증거 확보가 남았다. Axis 2..4와 비승인 target은 계속 off이며 PI Write도 별도 미구현/off 유지 |
| P5 | Closed - D6 static/handle facade Not Planned | current instance facade로 필요한 사용성과 provenance 계약을 충족했다. 명시적인 compatibility 소비자가 생기기 전에는 registry/static wrapper를 추가하지 않음 |

P2를 P3보다 먼저 둔 이유는 SDO Read-only가 한 ticket과 4-byte 결과로 범위를
고정할 수 있기 때문이다. Double bank는 PLC RAM, RT jitter, 두 bank ownership 및
reconnect 의미를 함께 검증해야 하므로 현재 single-bank 실장 기준을 먼저 확정해야 한다.

## 5. D1-D4 PLC 검증 매트릭스

| 단계 | 시험 | 합격 기준 | 증거 |
|---|---|---|---|
| D1 Health | 정상 OP 상태에서 Health 읽기 | 4축 행이 표시되고 WPF binding 예외가 없다 | WPF log와 screenshot |
| D1 fault | slave 단절, cable fault, AL/DS402 fault | `Online/EC State/AL Code/DS402/Axis Error`가 실제 상태와 일치한다 | WPF log, PLC 상태, packet capture |
| D1 stale | fault 전후 PI Read | 직전 raw 값이 새 정상값으로 표시되지 않고 stale/offline status가 붙는다 | cycle/status 비교 |
| D2 same-cycle | 최대 24개 signal Bulk Snapshot | 모든 값의 `CycleCounter`가 같고 TCP 처리 중 PLC object 개별 live read가 없다 | packet과 latch trace |
| D2 lifecycle | configure/read/release 및 reconnect | owner, BootId, revision 불일치를 거부하고 release 후 resource가 재사용된다 | 요청/응답 log |
| D3 capacity | 16채널 x 20,000, 24채널 x 13,333 | PLC가 반환한 `AcceptedCapacity` 안에서 sample 수와 stride가 일치한다 | header, chunk, CSV |
| D3 timing | divider와 sample period 변경 | cycle 간격이 설정 divider와 일치하고 허용 RT jitter를 넘지 않는다 | LASAL trace/Data Analyzer |
| D3 immutable upload | record 완료 후 장시간 chunk download | download 중 frozen bank header/hash/sample이 변하지 않는다 | 반복 header/hash |
| D3 reconnect/adopt | disconnect, 같은 BootId reconnect, exact/zero-ID adopt | active Ring과 frozen record를 규칙대로 회수하고 다른 BootId는 거부한다 | session log |
| D3 resource | full, Stop, Release, buffer 재사용 | full/terminal 상태와 StopReason이 일치하고 Release 전 덮어쓰지 않는다 | status/header log |
| D4 trigger | Edge/Window/Mask 각각 조건 발생 | `SampleCount=Pre+1+Post`, `TriggerIndex=Pre`, `StopReason=TriggerComplete` | status/header/CSV |
| D4 forced | `Trigger Now` | 입력 health와 무관하게 한 번만 trigger되고 두 번째 요청은 거부된다 | request/status log |
| D4 invalid input | trigger signal의 EtherCAT sample을 invalid로 전환 | invalid 구간을 건너뛴 가짜 edge/window 전이가 발생하지 않는다 | latch/trigger trace |
| D4 Stop race | 자연 완료와 Stop을 같은 시점에 실행 | WPF가 종료되지 않고 최종 status를 authoritative 결과로 표시한다 | WPF execution log |

장비 허용 RT jitter 수치는 PLC cycle과 현재 motion 부하를 측정한 뒤 시험 기록에
명시한다. 측정 전 임의 수치를 완료 기준으로 고정하지 않는다.

## 6. D5 첫 증분: SDO Read-only

### 6.1 포함 범위

- diagnostics service 전체에서 active ticket 한 개
- 물리 축 1-4의 SDO Read만 허용
- `SubmitSDO (0x7E50)`, `GetOperationStatus (0x7E03)`,
  `CancelOperation (0x7E04)` 실행
- nonzero ObjectIndex와 SubIndex 0..255 허용
- Bool/Int8/UInt8/BitField8은 1 byte, Int16/UInt16/BitField16은 2 bytes,
  Int32/UInt32/Real32/BitField32는 4 bytes만 허용
- 요청 type과 일치하는 1/2/4 bytes를 `GetOperationStatus` response에 inline
- `Queued -> Running -> Completed/Failed` 상태와 queued-only cancel
- drive busy는 bounded retry, start 실패와 SDO abort는 ticket failure로 반환
- disconnect 시 queued ticket 취소, running callback 결과는 폐기한 뒤 slot 회수
- first live regression vector는 `0x1000:0` UInt32/4-byte read다. 일반 Index/SubIndex는
  object dictionary에서 크기와 read-only 성격을 확인한 항목으로 시험한다.

2026-07-22 현재 위 세 command, `SdoAxis1..4` client, callback mailbox, one-ticket state
machine, RT latch cycle 기반 실행 scheduling과 network/generated table까지 구현했다.
10:53 gate-off Rebuild/Link는 baseline으로 통과했다. compile-time gate는 test 목적으로
`TRUE`며 legacy stable BootId capability `0x13F`, MaxSDO=4도 캡처에서 확인했다. 첫
`0x1000:0`은 request-local shadowing으로 same-cycle Expired 됐지만 수정 후 BootId 5의
Slave 1~4 요청은 43~54 cycles 뒤 모두 Completed/Success, UInt32 4-byte 결과를
반환했다. 이 캡처는 bit 13과 general-inline shape를 검증하지 않는다. current source의
`0x213F` capability는 BootId 6 capture에서 확인됐지만 general-inline Submit 두 건이
`ResourceBusy`로 실패했다. callback recovery 수정본의 1/2-byte 연속 성공은 BootId 8
capture로 확인했다. `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서
general-inline UInt32/4-byte 성공과 TypeMismatch 후 같은 BootId의 Int8/1-byte 재사용도
확인했다. queued-cancel one-shot/race/recovery도 PC/WPF runner 및 계약 시험을 완료했다.
abrupt disconnect application-recovery adapter도 local TCP zero-linger close/no-`0x405D`, distinct
new connection, exact two-ticket recovery와 CREVIS auto-load까지 구현했다. 그러나 결과는 항상
`orphanQualified=false`이며 PLC `MarkOrphan`/late-callback durable witness와 실제 owner-loss
capture가 필요하다. SDO abort/offline과 queued cancel live도 여전히 필요하다.
active contention과 timeout/drain recovery도 실제 PLC
실행과 packet이 필요하다. timeout runner는 canonical `0x6061:0 Int8/1` baseline 뒤
`TimeoutCycles=1`을 사용해 exact `Expired/TimedOut`, `0x05040000`, zero result를 요구한다. drain
중 recovery Submit은 동일 request/identity의 no-ticket `Rejected/ResourceBusy`일 때만 25 ms
간격 최대 600회 재시도하고, uncertain 또는 accepted-context에서는 즉시 중단한다. 실제 실행은
QTEST log와 `23g_SDO_Timeout_Drain_Recovery.pcapng`를 한 쌍으로 보존한다.

### 6.2 제외 범위

- PI Write
- SDO Write는 이 첫 Read-only 증분에서는 제외했지만 이후 별도 gate-off 증분으로 구현했다.
- 8/12-byte read, `ReadSDOResultChunk (0x7E51)`, 4 bytes 초과 결과
- 둘 이상의 동시 ticket 또는 축별 병렬 SDO
- 동적 메모리와 무제한 retry

runtime 시험을 위해 test-profile source에서 `SDORead` bit 8과
`SDOReadGeneralInline` bit 13을 열었다. D4 Double이 계속 꺼진 current source capability는
topology bit 14를 포함한 `CapabilityBits=0x0000613F`, `MaxSdoDataBytes=4`다. bit 7, 9,
12와 신규 node health/I/O bit 15~17은 계속 0이며 전체
runtime matrix evidence 전에는 이 값을 production 승인으로 보지 않는다.

기존 `ECAT_DS402Base::AddASyncEntryDS402` wrapper는 실제 반환 길이를 service에 전달하지
않는다. 새 설계는 `LMCSdoExecutor : EtherCAT_SDOBase`에서 lower-level callback의
`aPara[5]` actual length와 `aPara[6]` abort code를 직접 보존한다. current source는
request별 1/2/4-byte actual length를 검증한다. legacy `0x1000:0` regression과 별도로
확인된 1-byte, 2-byte, nondefault Index/SubIndex vector 및 success, busy, timeout,
cancel, disconnect/orphan을 PLC에서 시험한다. 8/12-byte는 general-inline profile과
분리한다.

구현 구조와 정확한 상태 전이는
`LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`를 따른다.

## 7. D4 Double bank 구현 범위

D4 Double enum, request/parser identity와 bit 6 off fail-closed C# 계약에 더해 2026-07-28
UI 독립 retained/release orchestrator와 deterministic PC 시험을 구현했다. 별도로
transport 응답을 제외한 `LMCRHDR1` semantic header canonicalization 5개와 PLC core reference
  model 1개를 추가했다. 이 코어는
exact owner/session/BootId/config/record/buffer identity, A/B capture/freeze/download, third Start의
ResourceBusy, A canonical header/data SHA-256 불변성과 보존형 failure/cancel을 검사한다. core
non-durable release primitive는 exact unexpected-third handle이 있으면 명시적 unexpected third ->
B -> A -> configuration release를 지원하지만 durable WPF 동작과는 구분한다. ambiguous
  Configure/Start/Release는 possible resource 또는 unverified outcome으로 남겨 destructive retry를
  차단한다.
  Recorder Trigger/Stop의 delayed ACK도 drain 뒤 `ResultDiscarded`된다. buffer release,
  configuration handle release, recovered configuration lease release와 adopted-identity recorder
  release의 늦은 ACK는 각 resource를 `OutcomeUnverified`로 격리해 재사용과 destructive retry를 막는다.

dormant `LMCRecorderStore` source에는 두 개의 1.28 MB bank, bank별 상태/identity/header/trigger,
첫 free bank 선택, full `ResourceBusy`, terminal publish-last, RT active-generation handshake,
Double zero-ID 거부, same-lease occupied bank 전체 exact rebind와 target-bank release 격리가
구현됐다. 여기에 exact `0x7E4A ReadRecorderBankInventory`와 Configure 응답 유실 뒤
occupied bank가 0개인 closed configuration만 새 session으로 넘기는
`0x7E4B AdoptEmptyRecorderConfiguration`을 추가했다. 0x7E4B 결과는 exact 0x7E48
release만 가능한 lease이며 Start 입력 type이 아니다. 0x7E4B success 응답 유실은 같은
session에서 임의 재시도하지 않고 transport fault/reconnect 뒤 새 inventory가 반환한
closed previous owner를 다시 exact bind한다. SourceOnly/full static 계약은 constructor의 54개
scalar/array/token 초기화, 두 bank descriptor 24개, metadata-before-Empty publish 및 final `C_OK`
순서를 actual-source negative fixture 13종으로 고정하며 PC reference model도 통과했다.
production WPF에는 `Run Double Bank`, `Cleanup Retained Double`, `Recover Double
Journal` adapter와 Start를 수행하지 않는 config-only manual Double Configure adapter가 연결됐다.
manual adapter는 source configuration의 모든 필드를 복제하고 같은 recovery Guid에서 만든
nonzero RequestedConfigId만 교체한다. Configure 전 journal arm, exact accepted-result 선점 보존,
Configure/Release 응답 유실의 no-replay를 적용하고 ordinary `recorderConfiguration`은 사용하지 않는다.
qualification은 recovery Guid에서 결정적 nonzero RequestedConfigId를
만들고 Configure 전에 journal을 arm하며, 결과와 exact operation scope/coordinator/connection/
diagnostics context를 같은 session cleanup용으로 보존한다. 자동 Release하지 않으며 same-session
cleanup은 third Start exact ResourceBusy일 때만 허용한다. exact preflight 뒤 checkbox 확인을
소비하고 Status, 필요 시 Stop, Ready/Uploading 확인 뒤 B -> A -> configuration 순서로 해제한다.
실패 뒤에는 checkbox를 다시 확인해야 한다. unexpected third success 또는 ambiguous outcome이면
same-session Release는 모두 zero-wire이고 disconnect/reconnect exact inventory inspection만
허용한다. conflicting inventory는 external/manual recovery로 보존하며 자동 Release하지 않는다.
현재 네 proof/route gate는 모두 `false`이므로 bit 6 + two
buffers + 4-entry Recordable Catalog에서도 live wire는 0회이고 수동 Double mode/강제 Configure는
계속 막힌다.
PC recovery에는 Configure 송신 전 exact journal, `0x7E4A/0x7E4B` inventory/adopt,
token-qualified `0x7E4C/0x7E4D`, bank/configuration Release 전 intent와 ACK 뒤 confirmed를
원자적으로 보존하는 durable v3
journal/coordinator를 추가했다. confirmed-not-applied pending bank/configuration intent는 동일
target의 exact intent만 재사용하며 새 intent/다른 target을 금지한다. retained handle이 이미
ACK-success면 Release wire replay 없이 durable confirm/resolve한다. 재시작 시 exact inventory에서 pending bank가 없으면 release를
confirmed로 reconcile하고, 그대로 있으면 closed-owner exact Adopt 뒤 기존 intent로 재시도한다.
사용자 확인 시점의 journal/config/exact bank 집합은 immutable snapshot으로 고정한다. 이후 4D/4A가
새 ConfigRevision 또는 snapshot에 없던 bank를 발견하면 local journal만 갱신하고 Adopt/Release는
0회로 중단한다. 갱신된 계획을 표시한 뒤 새 checkbox 확인을 받아야 하며 partial/retained 경로도
다음 mutation 전에 같은 policy를 다시 통과한다.
pending configuration이 동일한 empty/closed identity로 남아 있으면 `0x7E4B` 뒤 기존 intent로
`0x7E48`을 재시도한다. final configuration Release 응답 유실은 nonzero exact identity의
`0x7E4A` canonical-empty detail 32를 typed absence로 받아 mutation 없이 journal을 resolve한다.
occupied contradiction, identity mismatch와 wire binding marker가 없는 legacy v2
ConfigRevision=0은 unresolved fail-closed로 남긴다.
아래 source 항목은 구현됐지만 LASAL build와 PLC runtime으로 아직 증명되지 않았다.

- 두 번째 고정 bank와 bank별 `Free/Capturing/Ready/Uploading` 상태
- bank별 `RecordId`, `BufferId`, header, sample count, trigger metadata
- `BufferId=0/1`의 정확한 identity/owner/BootId 검사
- 한 bank upload 중 다른 bank에서 capture 시작
- 두 bank가 모두 점유됐을 때 RT를 block하지 않고 새 configure/start를 `Busy`로 거부
- terminal bank는 Release 전 덮어쓰지 않음
- reconnect/adopt 시 대상 bank의 모호하지 않은 선택 규칙
- 앱 외부 session loss 뒤 known exact lease를 새 session에 all-bank rebind하는 복구 규칙
- 한 bank fault/release가 다른 bank metadata를 변경하지 않음
- 두 bank RAM 배치와 worst-case recorder copy의 RT jitter 측정(미수행)

unknown Start 응답 유실은 0x7E4A inventory 뒤 occupied bank exact Adopt로, Configure만
적용되고 Start 전인 응답 유실은 exact Configure reply가 journal에 남으면 empty inventory 뒤
0x7E4B release-only lease로 복구한다. Configure reply까지 유실된 새 v3 `ClientTokenV1`
기록은 0x7E4C를 재전송하지 않고 0x7E4D로 actual ConfigRevision을 durable 확정한 뒤 0x7E4A를
다시 읽는다. wire binding marker가 없는 legacy v2 ConfigRevision=0은 계속 zero-wire
fail-closed한다. reconnect adapter는 ordinary diagnostics-ready/Catalog/global mutation
interlock과 분리된 recovery capability contract를 사용한다. ConfigRevision=0이면 0x7E4D ->
0x7E4A, 그 외에는 0x7E4A부터 시작하며 occupied bank는 exact 0x7E49, empty configuration은
0x7E4B로 채택한다. 일부 성공한 Adopt handle을 즉시 보존해 누락된 bank만 재개하고, recovered
bank는 Status 후 필요 시 Stop -> Ready/Uploading으로 만든 뒤 B -> A -> configuration 순서로
해제한다. inventory에서 확인되지 않은 revision/bank가 나타나면 journal merge 뒤 pre-mutation
reconfirmation으로 멈춘다. startup은 journal만 열고 inventory/adopt/release를 자동 replay하지 않는다.
qualification/manual/reconnect UI와 explicit cleanup UI는 구현됐지만 네 proof/route gate는 모두
`false`다. gate를 열기 전 LASAL IDE build/link, 2.56 MB free RAM, 1 ms jitter,
A upload 중 B capture, 두 bank full, session loss 뒤 두 exact Adopt와 bank별 Release도 실제
PLC에서 통과해야 한다.

zero-ID discovery는 single-bank에서만 대상이 유일하다. Double 활성 시에는 exact
`RecordId/BufferId`를 기본 adopt 경로로 사용하고, zero-ID discovery를 유지하려면 두
bank 중 하나를 고르는 wire 규칙을 먼저 추가해야 한다.

bit 6과 `RecorderBufferCount=2`는 capture/upload 동시성, 두 bank full, reconnect와 외부 session
loss 복구, RAM/jitter 시험을 모두 통과한 뒤에만 PLC에서 광고하고 WPF live gate를 연다.

## 8. D5 Write Policy와 D6

D5 SDO Write는 Read-only 증분과 분리한 별도 증분으로 이미 구현했다. exact Int32/4-byte
LASAL 실행 경로, C# API, WPF 편집/submit/readback interlock, SDK/PLC 이중 allowlist와 typed v2
durable journal이 있으며 current source는 Axis 1 exact `0x2F00:24 Int32/4` singleton만 승인한다.
global과 Axis 1 gate는 on이고 Axis 2..4 및 그 밖의 target은 off다. same-value qualification은
initial baseline Read와 fresh capability, 첫 safety check, 네 가지 operator confirmation,
unchanged exact pre-Write guard Read와 그 직후의 최종 두 번째 safety check를 통과한 뒤에만 journal arm -> byte-identical Write 1회 ->
exact Readback을 수행한다. 네 단계는 서로 다른 ticket이며 returned Write ticket을 durable
adopt/quarantine한 뒤 의미를 검증한다. 두 번째 Read는 atomic compare-and-write가 아니므로
single-writer 작업창이 필수다. sentinel, 자동 restore와 자동 replay를 하지 않는다. journal
recovery는 Write를 replay하지 않고 승인된 terminal-success
record의 exact target을 1회 Read할 뿐이며 legacy v1, Axis 2..4와 비승인 target은 zero-wire다.
현재 source/PC 계약과 LASAL Rebuild/Link·implementation smoke는 PASS했다. 따라서 남은 작업은
current PLC download, Axis 1 UI[24] ownership 확인 및 mailbox/pcap/물리 evidence 확보다.
승인 target도 type, 범위, owner, 물리 축,
축 상태를 모두 검사하고 ControlWord와 Target 계열 direct write는 영구 차단한다. PI Write는
구현하지 않았고 계속 off다.

D6은 현재 릴리스에서 `Not Planned`로 닫는다. PLC command나 packet을 추가하지 않으며,
Phase 1의 instance-owned `LMCConnection` 위에 구현한 D1/D2 PI/Bulk builder/reader facade가
owner/session provenance와 reconnect-stale pre-wire 거부까지 제공한다. 현재 repository의
WPF consumer도 instance connection만 사용한다. 따라서 이름이 정해진 Elmo-style
compatibility 소비자가 실제로 생기기 전에는 전역 handle registry와 static sync/async
wrapper를 추가하지 않는다. 그런 요구가 생기면 별도 milestone로 재개하고 slot+generation,
stale handle, dispose와 concurrent call을 독립 검증한다.

## 9. 정확한 검증 gate

| Gate | 명령 또는 절차 | 합격 기준 | 현재 판단 |
|---|---|---|---|
| C# PC contract | `MSBuild.exe LasalMotionControlLib.Tests.csproj /t:RunPcTests /p:Configuration=Debug /p:Platform=AnyCPU` | current Debug/Release `1006/1006 passed` | config-only Double Configure, pinned capability owner/session/single-observation, identity-pinned SDO Write pre-wire, Axis Power On mutation 귀속/final publication, Axis Stop/Reset/PowerOff, Group Enable/Stop/Reset, recovery identity retirement/readmission과 DS402 drive read 계약을 포함한다. PLC runtime 증거는 별도다. |
| WPF build/smoke | VS2019 MSBuild로 `LasalApiWpfTestApp.SmokeTests.csproj` Debug/Release | current Debug/Release 278/278 PASS 및 build error 0 | 기존 actual-control 회귀와 recovery identity read-only quarantine/explicit retirement/readmission, Single Axis live accepted-once/no-duplicate qualification, Axis 1 exact SDO Write activation, Group Reset accepted-once/member proof/safety reconciliation, proof identity drift/disconnect 영구 폐기 및 Axis 2..4 zero-wire를 포함한다. 네 D4 proof/route gate는 `false`; 실제 PLC `0x7E13/0x7E22`, Double live recovery, D5 live Write와 실제 축 motion recovery는 대기다. |
| LASAL SourceOnly contract | `Verify-LasalContract.ps1 -RepositoryRoot <repo> -SourceOnly -ControlServiceCheckpoint Phase5TransportClean -TopologyIoCheckpoint IntegratedReadOwnerDormant -ExpectedSdoWriteAxis 1` | PASS | 기존 diagnostics/Recorder/SDO 계약에 더해 464-byte coherent snapshot, `0x7E11/12/13/22` route/handler와 CREVIS read-owner client 계약을 확인한다. bit 15/16은 OFF로 요구한다. |
| LASAL full static contract | `Verify-LasalContract.ps1 -RepositoryRoot <repo> -ControlServiceCheckpoint Phase5TransportClean -TopologyIoCheckpoint IntegratedReadOwnerDormant -ExpectedSdoWriteAxis 1` | PASS | source, generated metadata, same-peer `TCPIPServer`, executor declaration/network, Axis 1 exact allowlist와 CREVIS coupler/input/output network wiring이 current tracked project와 일치한다. |
| executor initialization | LASAL IDE의 `LMCSdoExecutor` constructor state/buffer 명시 초기화 | declaration, generated `@STD` call, implementation 및 정적 assertion 일치 | PASS; constructor implementation SHA256 `DA7FD8454F16D24B1696579A54A9807F27B14D06535AEB4AA0000B5B9BB89254`, `ActiveToken := 0;` 8건을 build 전후 확인했다. |
| LASAL IDE compile | 대상 tracked project Rebuild 후 Link | compile/link error 0 | current integrated read-owner/Axis 1 gate-on source Rebuild/Link `0 errors / 20 warnings`, Linker Done. 설치 library/project version warning은 남아 있고 PLC download는 미실시다. |
| LASAL implementation smoke | Object Network Server/Client는 IDE `Find in Implementation`; 변경 function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header 직접 open | applicable index와 변경 implementation이 정상 로드되고 IDE 예외가 없음 | historical class-level smoke PASS; 기존 executor/service 검색과 latest 변경 implementation open은 성공했지만 exact method header 증거는 기록되지 않았다. 현재 기준으로 재사용할 때는 exact method direct-open을 별도 확인한다. |
| LASAL IDE log | 현재 IDE PID의 `%TEMP%\Lasal2.log` 검색 | `CInvalidArgException` 0건 | PASS; current IDE PID `CInvalidArgException` 0건 |
| CREVIS configured topology | current project Rebuild/Link, download 후 EtherCAT diagnostics | GL=physical index 0, Elmo=1..4인 5-slave configured order와 Vendor/Product/slot/PDO exact | ENI/network/serializer/generated-table drift static gate와 9개 negative fixture PASS. 사용자 LASAL build PASS 보고; `Test2`에서 GL + Elmo 4 + slot 2의 static 7-entry 응답 확인 |
| topology inventory raw qualifier | `topology-io-qualify --scope topology-inventory --execute-live --confirm PLC-RAW-TOPOLOGY-INVENTORY-READ ...` | bit 14와 nonzero BootId, pre/post capability identity 동일, `0x15867EEC` 7-entry order/identity/CRC exact; raw request는 `0x7E11/12` 8개뿐이고 `0x7E13/22/23` 없음; durable report 완결 | `Test2` raw pcap의 static sequence/response PASS, qualifier durable report 대기; static configured inventory만 입증 |
| topology/I/O read | `0x7E11/12/13/22` golden과 PLC read | topology revision/order exact, legacy `0x7E10` byte-identical, node state/quality만 동적, DI bit pattern 일치 | C# contract/parser와 WPF configured/live read UI 완료; LASAL `0x7E11/12` static live PASS; current `IntegratedReadOwnerDormant` source/full static과 IDE build/smoke PASS, 464-byte snapshot 및 `0x7E13/22` route/client wiring 구현. bit 15/16은 OFF이며 current PLC dynamic read는 미검증 |
| digital output write | `0x7E23` CAS ticket whole/masked/fault/RT matrix | single RT owner, stale output revision 거부, unmasked bit 보존, invalid/stale/offline에서 mutation 0, response-loss 자동 replay 0 | C# snapshot-bound request, ticket/policy/submission outcome context와 guarded WPF submit/exact full-shadow readback, durable unresolved mutation/Close interlock, restart recovery와 physical-verify/explicit-ACK UI 완료, SDK allowlist empty; PLC/LASAL 미구현, capability bit 17은 0 |
| diff hygiene | `git diff --check`와 staging 시 `git diff --cached --check` | whitespace error 0 | 최종 작업 종료 시 반복 |
| PLC capability | 변경 project download 후 `Refresh Capabilities` | stable BootId에서 `0x0000613F`, MaxSDO=4, topology bit 14 | `Test2` BootId 17에서 `0x0000613F` live 확인; bits 15~17 off |
| PLC runtime | 5절 매트릭스와 D5/D4 Double 단계별 시험 | 모든 행의 합격 기준과 증거 확보 | D0/D1/D2와 D5 legacy/general-inline 1/2/4-byte happy path pcap PASS; TypeMismatch 후 same-Boot recovery PASS; D5 나머지 fault/D1-D4 fault 재시험 대기 |

D5 PLC runtime을 위해 test source의 bit 8, bit 13과 `MaxSdoDataBytes=4`를 활성화했다.
legacy fixed-vector 4축 success는 확보했다. BootId 6 general-inline capture는 capability와
request shape를 확인했지만 Submit이 Busy로 실패한 과거 증거다. recovery 수정본의
1/2-byte success는 BootId 8 capture로 확인했고, 12번 capture는 general-inline 4-byte
success와 TypeMismatch 후 같은 executor 재사용을 증명했다. contention, timeout/drain과
queued-cancel PC/WPF runner는 구현됐지만 실제 packet은 없다. 나머지 fault/cancel/orphan,
offline/abort와 live
timeout/contention evidence를 확보하기 전에는
이 값을 production capability로 승인하지 않는다.

정적 계약 통과는 packet offset, source pattern, network 연결을 검증한다. LASAL
Rebuild/Link는 IDE 통합과 compile/link 가능성을 검증한다. 어느 것도 PLC scheduling,
EtherCAT fault 전이, RAM 여유, 실제 RT jitter, drive mailbox 응답을 대신하지 않는다.

LASAL class implementation은 tracked `.st`를 외부 편집기로 수정한다. IDE가 열린 상태에서
수정했다면 저장 전에 `Reload Class`를 실행한다. 권장 순서는 `IDE 저장/종료`, 외부 편집,
IDE 재열기 또는 `Reload Class`, Rebuild, item 종류별 smoke다. Object Network Server/Client는
`Find in Implementation`을 실행하고, 변경 function/method는 `Edit Method` 또는 `Enter`로 exact
Implementation header를 직접 연다. stale IDE model을 저장해 외부 implementation을 덮어쓰지 않는다.

## 10. 작업 종료 기준

각 우선순위는 다음 네 항목이 모두 있어야 종료한다.

1. source와 capability가 실제 구현 범위만 광고한다.
2. PC 자동 시험, LASAL 정적 계약, IDE Rebuild/Link를 통과한다.
3. 해당 단계의 PLC 시험 결과를 packet/log/trace로 저장한다.
4. 사용자 문서와 release status의 수치 및 미구현 표기가 source와 일치한다.

현재 working source에는 D1-D4와 활성화한 D5 general-inline 실행부, shadowing 수정 및
4개 Elmo에 앞선 GL-9086을 포함한 configured 5-slave network source가 있다. CREVIS
read-owner의 464-byte snapshot, `0x7E13/0x7E22` route와 network wiring은 current
SourceOnly/full static 및 LASAL Rebuild/Link를 통과했다. PLC download와 live I/O 증거는 없다.
legacy `0x1000:0` drive 1~4와 수정본 general-inline 1/2/4-byte
happy path는 성공 pcap을 확보했고 TypeMismatch 후 same-Boot executor 재사용도
확인했다. contention runner와 PC 계약 12개, disconnect/orphan UI 독립 코어와 계약 28개 및
WPF abrupt-disconnect application-recovery adapter와 fake-RPC 2-session full-handler smoke는 완료됐다.
후자는 local TCP abort 뒤 새 connection recovery, quarantine clear와 새 CREVIS topology adoption까지만
증명하며 `orphanQualified=false`다. PLC durable lifecycle witness와
live owner-loss packet은 없다. D1 one-slave Health/PI 판정과 stale raw 표시 및 integrated
read-owner는 PC/WPF/source/static/IDE build까지만 완료됐고 PLC download/live fault evidence는
없다. constructor 명시 초기화, 나머지 D5 fault/timeout/cancel/orphan live matrix와 5절 검증,
IO-3의 raw live qualification 및 IO-4~IO-5가 남았다.
따라서 D1-D5를 production 완료로 분류하지 않는다. current test source 광고값은
`0x0000613F`, `MaxSdoDataBytes=4`다. `0x0000213F`는 topology bit 14 추가 전 BootId 6
capture 값이다.

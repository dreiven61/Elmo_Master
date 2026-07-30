# Elmo_Master history 1 parts 018-034 digest

## 범위와 판독 기준

- 대상: `Elmo_Master_history_260730_1_part_018_lines_04251_04500.md`부터 `part_034_lines_08251_08500.md`까지 17개 물리 청크, 원본 source line 4,251-8,500.
- 각 청크를 처음부터 끝까지 읽었다. 청크 경계가 접힌 `previous messages`/`details` 블록 중간을 자르는 경우에는 앞뒤 청크를 이어서 해석했다.
- 아래 내용은 **히스토리에 기록된 당시 작업·판정의 요약**이다. 현재 worktree, 현재 바이너리, PLC 다운로드 상태 또는 실장 결과로 재검증한 사실이 아니다.
- `PASS`, 빌드 수치, 파일 변경 및 리뷰 결과도 모두 당시 기록이다. 후속 작업 전에는 live source와 `git status`를 다시 확인해야 한다.

## 청크별 기록

### Part 018 — source lines 4,251-4,500

- 주요 요청/문제: CREVIS topology/I/O 화면을 완성하고, SDO Write 중 다음 입력을 편집할 수 있게 하며, Digital Output Write의 불확정 결과를 안전하게 다루는 작업을 계속함.
- 수행/변경: `MainWindow.TopologyIo.cs`, `MainWindow.Diagnostics.cs`, XAML과 SDK 모델에 ticket, 전체 output shadow, unmasked-bit 보존, topology/revision/node/I/O identity, 세션/BootId provenance, `NotAttempted`/`Rejected`/`OutcomeUncertain` 구분을 추가했다고 기록됨. `Load Topology`가 capability를 먼저 갱신하도록 바꿈.
- 검증 기록: PC 계약 시험이 290에서 298개까지 증가해 Debug/Release PASS, WPF 격리 Debug/Release 및 LASAL SourceOnly/full static PASS.
- 실패·정정·철회: C# 지역 변수명 충돌은 Write 전용 이름으로 수정. 실행 중 Debug GUI가 DLL을 잠가 표준 Debug 복사만 실패해 별도 출력 폴더를 사용. stale shadow가 재접속 후 재사용될 수 있는 결함, ACK 직후 세션 변경, 확인창이 snapshot 대신 현재 TextBox를 읽던 결함을 리뷰 후 수정.
- 남은 작업: PLC `0x7E13/0x7E22/0x7E23`, `LMCEtherCATIoService` 또는 동등 RT owner, capability bits 15-17, 실제 CREVIS/출력 시험은 미구현·미검증으로 남음.

### Part 019 — source lines 4,501-4,750

- 주요 요청/문제: SDO/DO mutation을 앱 강제 종료·재시작 뒤에도 잃지 않는 durable journal을 만들고, CREVIS 동적 서비스의 LASAL 삽입 구조를 확정함.
- 수행/변경: `MainWindow.MutationJournal.cs`를 만들고 single-writer, checksum/corruption fail-closed, 전송 전 기록→ticket 기록→종결/readback 기록, 자동 재전송 금지, 재시작 interlock을 GUI에 연결했다고 기록됨. 취소 ticket 잔존, journal 손상 시 motion이 열리는 문제, guard 해제 오류가 원 오류를 가리는 문제를 보강.
- 검증 기록: PC Debug/Release `304/304`, WPF Debug/Release, LASAL SourceOnly/full static PASS.
- 실패·정정·철회: `git diff --check`의 당시 개발 diff는 정상이었으나, 이미 staged된 대형 history의 trailing whitespace 때문에 cached 검사는 실패. 별도 새 RT class 계획은 재검토되어 기존 1 ms `LMCEcatInputLatch`를 확장하는 통합-owner 설계로 바뀜; 기존 `_LMCAxis1.LMCPreRtWorkTrigger` 연결을 보존하기로 함.
- 남은 작업: 테스트 폴더 PLC source에는 당시 `0x7E11/0x7E12`조차 없었다고 기록됨. `LMCEcatInputLatch`의 CREVIS clients/methods와 `LMCDiagnosticsService` helper, Motion Network 연결은 LASAL IDE 생성 대기.

### Part 020 — source lines 4,751-5,000

- 주요 요청/문제: IDE 구조가 생기기 전에 CREVIS RT snapshot/mailbox 계약과 단계별 정적 검증을 최대한 고정함.
- 수행/변경: `EtherCATIoRtReferenceModelTests.cs`와 대규모 `Verify-LasalContract.ps1` 검증을 추가. 단계는 `StaticTopologyOnly → IntegratedReadOwner → IntegratedOutputOwnerDormant`로 나누고, 464-byte 확장 snapshot, freshness, seqlock, mailbox 원자성, output revision/CAS, source 연결, wire offsets, disabled gate를 검사하도록 했다고 기록됨. PLC global/per-node output gate는 `FALSE` 유지.
- 검증 기록: 중간 기준 PC Debug/Release `314/314`에서 `315/315`로 증가, 현재 `StaticTopologyOnly` PASS, 다음 두 단계는 필요한 route/구조가 없어 의도대로 FAIL. 최신 WPF는 별도 출력에 빌드.
- 실패·정정·철회: 테스트 named argument 오류와 실행 명령/매개변수 오류를 수정. SIGMATEK enum을 임의값으로 둔 모델을 실제 `_ClassOk=0`, `_NoHardware=5`로 정정. 병렬 빌드의 공용 `obj\Debug` 충돌 및 실행 중 GUI DLL 잠금은 순차/격리 빌드로 우회.
- 남은 작업: 실제 clients/method/network와 `0x7E13/0x7E22/0x7E23` 구현, IDE build/download, live PDO 검증은 여전히 대기.

### Part 021 — source lines 5,001-5,250

- 주요 요청/문제: 미래 CREVIS output 구현이 잘못돼도 정적 검증을 통과하지 못하도록 verifier를 공격적으로 강화하고, GUI의 불확정 output ACK/SDO 편집 정책을 고정함.
- 수행/변경: `AppliedCycle`, 32-bit revision/token, `DataDefaulted`, unconditional seqlock close, request/completion publish 순서, 단일 writer, exact response field count, pointer/length guard 등을 검증하도록 확대. Output 불확정 ACK는 물리 확인 체크+경고 확인을 요구하고, SDO editor availability를 순수 정책/시험으로 분리했다고 기록됨.
- 검증 기록: 중간 `315/315`; 후속 경계에서 WPF 양 구성과 PC `319/319`, LASAL `StaticTopologyOnly` PASS로 정리됨.
- 실패·정정·철회: `AppliedCycle` 대신 서비스 현재 cycle을 쓰는 오류 가능성, mailbox 정상 문장 뒤 덮어쓰기, snapshot publish 이후 훼손, 잘못된 PREOP/SAFEOP 검출 조건 등을 리뷰로 찾아 verifier를 수정. 병렬 Debug 빌드 산출물 충돌도 순차 재빌드.
- 남은 작업: 정적 검증은 미래 구현의 형태만 보장하며, live PLC 경로와 IDE 구조는 없음. `IntegratedReadOwnerDormant`/output 단계는 아직 통과 대상이 아님.

### Part 022 — source lines 5,251-5,500

- 주요 요청/문제: verifier의 문자열 기반 우회 가능성을 닫고, 사용자가 최신 GUI/PLC 조합을 식별할 수 있게 함.
- 수행/변경: pointer alias, helper 외 주소 취득, legacy channel RHS 위조, 주석으로 canonical line을 가장하는 우회, OOB access, 조건부 response 실행을 실제 주석 제거 코드와 command 구간별 소유권으로 검사하도록 강화했다고 기록됨. T2 IDE handoff 문서를 만들고 partial IDE structure도 fail하도록 guard 추가.
- 검증 기록: SourceOnly/full static PASS, PC/WPF는 경계상 `315/315`에서 `319/319`로 증가. 최신 `bin/CodexLatest` GUI가 생성됐다고 기록됨.
- 실패·정정·철회: verifier가 정상 `BuildCatalogEntry` 포인터 전달까지 금지하고 payload arm 깊이를 잘못 계산한 자기모순을 발견해 규칙을 되돌리고 command-local 검사로 재작성. 기존 실행 PID는 구버전이고 테스트 PLC source도 구버전이라는 원인을 분리.
- 남은 작업: 최신 GUI 재실행과 최신 LASAL source build/download 필요. configured topology만 가능하며 live health/DI/DO는 계속 미구현.

### Part 023 — source lines 5,501-5,750

- 주요 요청/문제: capability를 production에서 열지 않고 `0x7E11/0x7E12/0x7E13/0x7E22` read 경계를 검증할 별도 qualification 도구와 stale SDO readback 복구를 마련함.
- 수행/변경: read-only `TopologyIoQualificationTool` 및 문서/시험을 추가하고, topology chunk 전체 개수, NodeId/entry 결속, 전후 DiagnosticsBootId, report durability, 요청 횟수 상한을 검사. SDO stale exact-readback은 물리 확인+명시 ACK+tombstone으로만 해제하고 topology aggregate를 owner/session-bound로 강화했다고 기록됨.
- 검증 기록: 도구 보강 후 Debug/Release `331/331`; 이어 SDO/topology binding 보강으로 `340/340`, 다음 청크 경계에서 `342/342`로 정리. WPF와 `StaticTopologyOnly` PASS, `IntegratedReadOwnerDormant`는 route 부재로 예상 FAIL.
- 실패·정정·철회: 잘못된 topology count가 최대 65,535회 요청을 유도할 수 있는 문제, `.inprogress` 보고서 durability, 서로 다른 BootId 증거 혼합을 리뷰 후 수정. raw qualifier는 output write를 명시적으로 금지.
- 남은 작업: LASAL IDE handoff 구조 생성 후 live read owner 구현. qualification 도구 PASS도 PLC 실장 PASS로 보지 않음.

### Part 024 — source lines 5,751-6,000

- 주요 요청/문제: Connect 직후 CREVIS configured topology 자동 로드, 실패/재접속 시 stale UI 제거, mutation journal의 실제 프로세스 종료 검증, SDK SDO exact-readback 및 live monitor 계약을 보강함.
- 수행/변경: topology 원자 갱신/실패 상태, late-response 세대 차단, capability-off wire 0회, tick당 live RPC 1회 구조를 추가. child process 강제 종료 뒤 journal reopen/interlock 보존, parser deterministic property/fuzz, topology-only inventory dry-run, SDK SDO 검증 API를 추가했다고 기록됨.
- 검증 기록: 단계별 `347/347`을 거쳐 PC Debug/Release `361/361`, WPF 양 구성 PASS, LASAL static PASS. topology inventory dry-run은 네트워크 송신 없음.
- 실패·정정·철회: 외부 disconnect 뒤 stale row, 끊어진 세션의 늦은 응답, UI 갱신 중 `RequireConnection()` 예외를 수정. 새 test helper 누락으로 한 번 compile 실패했고 oracle 과잉 제약 2건을 수정 후 전량 재실행. process test는 전원손실/실제 RPC no-replay 증거로 과장하지 않도록 범위를 낮춤.
- 남은 작업: 실제 PLC bit 14 source 실행 여부와 live bits 15/16은 미확인. 이어지는 owner/session provenance 보강이 시작됨.

### Part 025 — source lines 6,001-6,250

- 주요 요청/문제: Topology/Signal Catalog aggregate가 다른 연결·재접속 세션에서 재사용되는 문제, Stop/PowerOff보다 diagnostics가 먼저 송신될 수 있는 문제, 분산된 admission 규칙을 해결함.
- 수행/변경: topology/catalog/bulk/PI write를 owner/session-bound로 만들고 세션을 끝까지 pin. `LmcSendPriorityCoordinator`를 만들어 실제 `stream.Write` 직전 safety generation을 검사하고, 아직 송신 전인 일반 RPC를 0-byte preempt하도록 연결. `DiagnosticsOperationAdmissionPolicy`로 GUI mutation/quarantine/journal 규칙을 통합했다고 기록됨.
- 검증 기록: provenance `363/363`, priority `371/371`, admission 통합 `378/378`; WPF Debug/Release 및 LASAL SourceOnly/full static PASS.
- 실패·정정·철회: preflight 직후 reconnect와 aggregate 준비 중 capability 재캡처 race를 수정. 처음에는 journal 장애 시 exact readback도 막아야 한다고 판단했으나, 기존 Write를 해소하는 recovery read임을 소스 추적 후 정정하여 read는 허용하고 durable resolve만 fail-closed로 유지. 이미 시작된 RPC는 강제 취소하지 않는다고 범위를 제한.
- 남은 작업: 연결 Init/Close/Dispose 경쟁 검증과 실제 safety latency/PLC 증거는 후속으로 남음.

### Part 026 — source lines 6,251-6,500

- 주요 요청/문제: 연결 수명주기 경쟁, 실제 WPF control 수준의 CREVIS/SDO 회귀, parser stress, 재시작 no-replay를 검증함.
- 수행/변경: `LmcConnection`에 connection generation과 stale cleanup 격리, 상태 callback 내 sync/async 재진입 거부, 중첩 A→B→A scope chain을 추가. WPF smoke 프로젝트를 만들고 실제 Connect/diagnostics controls를 실행. 6-family parser stress와 stale UDP callback generation 차단, 실제 WPF process mutation recovery smoke를 추가했다고 기록됨.
- 검증 기록: lifecycle 반복 포함 `395/395`; 이후 API `396`, parser 포함 `399`, callback/Reload 포함 `400/400`; WPF는 2/2→3/3→4/4로 증가. 고정 seed 10,000 parser 변이 PASS.
- 실패·정정·철회: smoke가 XAML 초기화 중 조기 `TextChanged` NRE와 disconnected window close 재호출 예외/journal lock을 실제로 찾아 수정. `ArmedBeforeDispatch`가 Connecting에서 `OutcomeUnverified`로 승격되는 의도된 rewrite를 반영해 byte-invariance 기준을 안정 상태로 변경. zero-replay smoke의 조기 EOF 가능성도 barrier로 보강.
- 남은 작업: D4 Double-bank PC orchestrator가 시작됐지만 live gate와 PLC 증거는 아직 없음.

### Part 027 — source lines 6,501-6,750

- 주요 요청/문제: D4 Double-bank qualification core를 안전하게 만들고, recovery 준비 전 GUI 우회 송신을 막은 뒤 D5 contention 시험을 추가함.
- 수행/변경: Double-bank orchestrator에 bank A/B identity, third Start `ResourceBusy`, B→A→configuration cleanup, retained scope, confirmed-not-applied와 outcome-unverified 구분을 추가. live 실행은 dormant UI/zero-wire로 유지하고 수동 Double mode/Configure 및 Double-capable ambiguous Adopt 우회를 차단. 이어 D5 `Contention → Recovery` runner를 추가했다고 기록됨.
- 검증 기록: D4 단계 API `411/411`, WPF `5/5`; D5 contention 계약 추가 뒤 `423/423`, WPF 양 구성 PASS.
- 실패·정정·철회: Release ACK 유실, Configure/Start 응답 유실, third Start 예상 밖 성공에서 잘못 자동 정리할 위험을 찾아 fail-closed로 변경. Catalog 로드보다 클릭이 빨랐던 smoke 동기화 오류는 버튼 활성화/완료 대기로 수정.
- 남은 작업: D4 외부 세션 손실 복구가 없어 gates off. D5 contention은 실제 PLC/pcap 증거가 없음.

### Part 028 — source lines 6,751-7,000

- 주요 요청/문제: D5 timeout→drain→recovery runner를 만들고, IDE 없이 가능한 D4 Double PLC dormant core를 구현함.
- 수행/변경: `TimeoutCycles=1`, 최대 600회/15초의 exact `ResourceBusy` drain, distinct recovery ticket 계약을 추가. `LMCRecorderStore.st`에 두 번째 1.28 MB bank와 per-bank identity/state/metadata, isolated release/adopt 구조를 넣고 header normalization을 추가했다고 기록됨. 동시에 CREVIS 최신 GUI와 SDO 편집 회귀를 다시 확인.
- 검증 기록: D5 timeout core 후 `437/437`; D4 dormant core 후 PC Debug/Release `443/443`, WPF `5/5`, LASAL SourceOnly/full static PASS.
- 실패·정정·철회: queued-cancel은 1-cycle race로 당시 결정론적이지 않아 후순위로 미룸. D4는 코어 구현만 했고 RAM 2.56 MB 배치, jitter, build/link, 실제 A/B 불변성 증거가 없어 capability bit 6/count 2/WPF live gate를 계속 OFF로 유지.
- 남은 작업: 사용자의 LASAL build/link 결과와 RAM/jitter/실기 proof 필요. D4 recovery inventory/journal 구현이 다음 단계로 시작됨.

### Part 029 — source lines 7,001-7,250

- 주요 요청/문제: D4 Configure/Start/Release 응답 유실과 재시작을 exact identity로 복구하고, mutation 재전송 없이 durable cleanup을 보장함.
- 수행/변경: `0x7E4A/0x7E4B`, recovery planner/orchestrator, journal v2, release intent→ACK confirmed, inventory 기반 re-adopt/release를 추가. canonical Empty일 때만 detail 32 absence proof를 반환하도록 PLC/SDK를 확장하고, 이후 128-bit token 기반 `0x7E4C/0x7E4D` 설계·구현을 시작했다고 기록됨.
- 검증 기록: durable recovery 전환 후 `500/500`, detail 32 후 `505/505`, token 통합 중 `516/516`; WPF `6/6`, LASAL static PASS.
- 실패·정정·철회: inventory에서 bank가 사라진 것을 해제 성공으로 추정하던 문제, revision 0 자동 채택, release response loss 후 handle 재사용, intent 저장 후 송신 전 crash, occupied-bank 모순을 모두 fail-closed로 수정. 전환 중 `493개 중 4개`가 깨졌으나 기존 시험을 새 intent 모델로 옮기는 예상 중간 실패였고 최종 재실행에서 해소. detail 32를 unknown으로 쓰던 3개 시험은 33으로 정정.
- 남은 작업: token 재사용 방지와 public API 우회 차단 마무리, live gates/PLC proof는 계속 대기.

### Part 030 — source lines 7,251-7,500

- 주요 요청/문제: D4 token recovery를 닫고, SDO restart recovery journal v2 및 D5 queued-cancel qualification을 구현함.
- 수행/변경: `0x7E4C/0x7E4D`, token tombstone, `4D → durable save → 4A → 4B` 순서와 direct Adopt 우회 zero-wire를 고정. Recorder parser stress를 8-family로 확장. SDO v2 journal은 `allowlist → capability pre → 1회 Read → capability post → record/state CAS`로만 해제. D5 `Submit → Cancel 1회 → terminal → recovery Read` runner를 추가했다고 기록됨.
- 검증 기록: D4 token 단계 `517/517`; parser stress Release 100,000회 PASS; SDO recovery `530/530`, WPF `7/7`; queued-cancel은 다음 경계에서 API `540/540`, WPF `7/7` PASS.
- 실패·정정·철회: token tombstone이 일반 release 후 0으로 덮여 재사용될 수 있는 문제와 recoverable inventory를 기존 Adopt API에 직접 넣는 우회를 차단. SDO recovery의 post-read identity 누락과 journal state/CAS race를 수정. D6 static registry는 실제 consumer가 없고 instance facade가 요구를 충족한다는 판단으로 `Not Planned` 처리.
- 남은 작업: current allowlist가 비어 있어 SDO recovery/write는 wire 0회. queued-cancel 실제 PLC 증거와 D4 live proof는 없음.

### Part 031 — source lines 7,501-7,750

- 주요 요청/문제: D4 Double journal을 실제 MainWindow 수명주기와 interlock에 연결하고, qualification/reconnect/cleanup SDK adapter를 붙임.
- 수행/변경: Double 전용 journal open/lock/status, global mutation/Close interlock, recovery identity UI를 추가하되 `ReconnectRecovery=false`로 강제 호출도 zero-wire 유지. 이후 실제 Double qualification adapter, retained handles, Adopt 후 exact Status 보강, 명시적 Stop→Ready와 B→A→configuration cleanup을 구현했다고 기록됨.
- 검증 기록: journal lifecycle WPF `9/9`; adapter 보강 뒤 Debug API가 541에서 545로 증가하고 WPF가 11에서 12로 증가하는 과정이 기록됨(최종 수치는 다음 청크에서 확정).
- 실패·정정·철회: Adopt 첫 응답에 configuration metadata가 없는데 즉시 요구하던 P1을 exact Status 보강으로 수정. ACK 성공 후 journal 저장 실패 crash window, unexpected third Start를 같은 세션에서 release하려던 잘못된 계약, pending intent 재시도 교착, 확인 checkbox 재사용을 수정. third Start 성공/불명확 시 어떤 Release도 보내지 않고 reconnect inventory만 허용하도록 정정.
- 남은 작업: 세 proof gate는 계속 false. 문서와 Release 최종 수치 정합성, PLC build/RAM/jitter/실기 증거가 남음.

### Part 032 — source lines 7,751-8,000

- 주요 요청/문제: D4 reconnect 계획 변경 시 재확인을 강제하고, CREVIS live GUI의 stale/error 혼선을 제거하며, D1 offline semantics와 D5 disconnect core를 보강함.
- 수행/변경: D4 confirmed-not-applied exact-target retry와 새 revision/bank 발견 시 Adopt/Release 전 중단·재확인을 추가. WPF Health/DI/DO late-response와 채널별 error/stale 상태를 분리하고 mixed I/O의 stale output shadow submit을 차단. D1 `0x7E20`은 valid=detail 0, offline=18, 기타 invalid=11로 바꾸고 invalid raw를 `UNAVAILABLE`로 표시. D5 disconnect/orphan은 우선 UI-independent core로 추가했다고 기록됨.
- 검증 기록: D4/API `552/552`, WPF `12/12`; CREVIS UI 보강 후 WPF `15/15`; D1/D5 core 합산 PC `575/575`, WPF `16/16`, LASAL static PASS.
- 실패·정정·철회: reconnect confirmation 당시 없던 bank를 같은 클릭에서 해제할 위험, 성공 뒤 실패 시 상세 패널에 오래된 값이 남는 문제, DI가 output shadow를 덮는 문제를 수정. D1에서 offline raw=0을 요구하던 잘못된 판정은 `Valid` 해제+`SlaveOffline`+detail 18로 정정. D5는 Running만 orphan 후보로 보고 Queued는 application recovery만 인정.
- 남은 작업: `LMCDiagnosticsService.st` 변경은 build/download 및 실제 D1 offline 시험 필요. CREVIS live route 미구현, D5 core는 아직 UI/PLC witness 없음.

### Part 033 — source lines 8,001-8,250

- 주요 요청/문제: 실제 사용자 증상 기준으로 CREVIS 표시 위치와 SDO draft 잠금을 다시 손보고, D5 transport-loss GUI adapter를 구현함.
- 수행/변경: legacy 4-axis 표에 `CFG slave`를 추가하고 CREVIS load/status를 탭 상단으로 이동. **이전의 “exact readback 중 입력 잠금” 정책을 다시 수정하여**, 진행/readback/capability refresh 중에도 draft를 보존하고 `Load Required Exact Readback` 버튼으로 원 요청을 명시적으로 복원하도록 변경. D5는 로컬 zero-linger abort→새 connection→복구 Read 2회→CREVIS reload adapter를 추가했다고 기록됨.
- 검증 기록: GUI 증분 WPF `17/17`, API `575/575`; D5 작업 중 API `582/582`, WPF `18/18`을 거쳐 다음 청크에서 `587/587`로 확정.
- 실패·정정·철회: capability/topology refresh가 승인 target과 draft를 첫 항목으로 덮는 결함, live bit 하강 뒤 stale summary를 남기는 결함을 수정. zero-linger가 Windows loopback에서 항상 RST 예외로 보이지 않고 EOF로 정규화될 수 있어 자동 증거 범위를 “linger 설정+`0x405D` 미전송+연결 종료”로 낮춤. 명칭도 `ExternalLoss`가 아닌 `TransportLoss`로 정정하고 `orphanQualified=false`/`ApplicationRecoveryOnly` 유지.
- 남은 작업: 실제 TCP RST packet proof와 PLC 내부 orphan witness가 없음. 별도 LASAL IDE witness 구조가 필요.

### Part 034 — source lines 8,251-8,500

- 주요 요청/문제: D5 old/new TCP 2-session GUI 경로를 끝까지 검증하고, gate-off 상태에서 실행 가능한 SDO same-value Write qualification을 준비함.
- 수행/변경: 두 세션 fake server로 old session `0x405D` 없음, 새 connection 채택, SDO recovery 2회, quarantine 0, 다른 topology revision 재로딩을 검증. `LMC_D5_ORPHAN_WITNESS_IDE_HANDOFF_2026-07-28.md`를 생성. SDO same-value runner는 `baseline Read → 사용자 확인 → 값 불변 guard Read → 최종 축 안전 재검사 → journal → Write 1회 → exact Readback` 순서로 구현했다고 기록됨.
- 검증 기록: D5 후 API Debug/Release `587/587`, WPF `21/21`; same-value runner 후 API `596/596`, WPF `22/22`, LASAL SourceOnly/full static PASS. CREVIS live T2 checkpoint는 `0x7E13/0x7E22` 부재로 예상 FAIL.
- 실패·정정·철회: 첫 2-session 통합 smoke는 실패했고 구조화 GUI log와 server request index를 추가해 scripted 응답/동기화를 바로잡은 뒤 통과. Write runner 리뷰에서 장시간 확인창 뒤 축 상태 stale, 승인 ticket의 durable 보존 순서, baseline 값 변경 후 오래된 값 재쓰기 위험을 찾아 guard Read와 최종 safety recheck, ticket-first journal로 수정. sentinel/자동 restore/retry 방식은 사용하지 않음.
- 남은 작업: SDK/PLC Write gate는 OFF라 실제 Write는 0회. 활성화 전 `UI[24] (0x2F00:24)` 미사용 확인과 시험 축 1개 지정, PLC build/download/live capture가 필요. D5 orphan witness와 CREVIS live implementation도 미완료.

## 전체 구간의 시간순 단계

1. **CREVIS/SDO GUI 기본 기능과 mutation 안전성**
   - configured topology 화면, 자동 capability/topology load, SDO immutable request/draft 편집, DO full-shadow/ticket/readback 검증을 구현했다고 기록됨.
   - SDO/DO 전송 불확정 상태를 durable journal과 전역 interlock으로 연결함.

2. **CREVIS T2 구조 설계와 정적 계약 선행**
   - 별도 RT service 대신 기존 1 ms `LMCEcatInputLatch`를 확장하는 방향으로 설계를 바꿈.
   - reference model과 단계형 LASAL verifier를 대폭 강화했지만, 필요한 IDE clients/method/network와 live routes가 없어 `StaticTopologyOnly`까지만 가능하다고 분리함.

3. **PC qualification·parser·실제 WPF smoke 확대**
   - topology/I/O raw read qualifier, parser fuzz/stress, process crash/restart journal smoke, 실제 MainWindow control test를 추가함.
   - 이 과정에서 startup NRE, close 재진입, stale callback/late response, 자동 reload 경쟁을 실제로 찾아 수정했다고 기록됨.

4. **SDK provenance·송신 우선순위·수명주기 보강**
   - topology/catalog/capability를 owner/session-bound로 만들고, Stop/PowerOff 예약이 미송신 diagnostics를 write 직전 0-byte preempt하도록 함.
   - admission 정책과 connection generation을 통합하고 stale cleanup/callback 및 reentrant lifecycle을 차단함.

5. **D4 Recorder Double-bank dormant 구현과 복구 체계**
   - 두 bank PLC core, PC qualification/retained cleanup, `0x7E4A/4B/4C/4D`, token/journal/release intent/reconnect recovery를 단계적으로 구현했다고 기록됨.
   - 그러나 capability bit 6, bank count 2, live WPF proof gates는 RAM/jitter/build/실기 증거 전까지 계속 닫아 둠.

6. **D5 qualification 확대**
   - contention, timeout/drain, queued-cancel, transport-loss/application-recovery runner를 추가함.
   - local TCP abort와 2-session application recovery까지 PC fake-RPC로 확인했으나, PLC 내부 orphan witness와 실제 RST/pcap 증거가 없어 orphan PASS로 올리지 않음.

7. **D1/CREVIS 표시 의미 정정**
   - invalid/offline PI raw를 현재값처럼 표시하지 않고 `UNAVAILABLE`로 바꾸고, LASAL `0x7E20` detail mapping을 명시함.
   - configured CREVIS(7 nodes/3 CREVIS entries)와 live Health/DI/DO를 UI와 문서에서 분리함.

8. **최종 SDO UX/활성화 준비**
   - 중간 단계의 “exact readback 동안 draft lock”은 최종적으로 철회되어, draft는 항상 보존하고 필수 readback target은 별도 버튼으로 불러오게 바뀌었다고 기록됨.
   - same-value qualification은 안전 순서를 갖췄지만 allowlist/PLC gate가 닫혀 실제 Write 증거는 없음.

## 후속 구간에 넘길 상태

### 히스토리상 마지막 소프트웨어 체크포인트

- PC API Debug/Release: `596/596 PASS`.
- WPF actual-control Debug/Release: `22/22 PASS`.
- LASAL SourceOnly/full static: PASS.
- CREVIS live T2 checkpoint: `0x7E13/0x7E22` route 부재로 예상 FAIL.
- working diff `git diff --check`: PASS.
- cached diff: 기존 staged `docs/history/Elmo_Master_history_260721.md` trailing whitespace 때문에 FAIL이라고 기록됨.
- 위 수치는 모두 히스토리 기록이며 현재 checkout에서 재실행하지 않았다.

### 우선순위 1 — CREVIS live T2

- configured topology는 최신 PLC가 bit 14와 `0x7E11/0x7E12`를 실제 실행할 때 `Nodes=7`, `Configured CREVIS entries=3`이 기대값.
- live Health/DI/DO는 당시 `LMCEcatInputLatch` clients/methods, diagnostics helper/routes, Motion Network 연결과 bits 15-17이 없었음.
- `docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md`의 IDE 구조를 생성·저장한 뒤 tracked `.st` implementation, LASAL rebuild/link/download, 실제 CREVIS capture를 진행해야 함.

### 우선순위 2 — SDO Write 활성화/실장 검증

- 최종 GUI 계약은 ordinary operation, exact-readback pending, capability refresh 중에도 draft 보존; Submit만 직렬화; 필수 target은 `Load Required Exact Readback`으로 명시 복원.
- same-value qualification 구현은 존재한다고 기록됐지만 SDK allowlist와 PLC gate가 OFF여서 actual Write는 0회.
- 활성화 전에 시험 축 1개와 `UI[24] (0x2F00:24)` 미사용을 확인하고, `docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md`에 따라 build/download/pcap/readback을 수행해야 함.

### 우선순위 3 — D4 Double-bank 실증

- dormant PLC core와 PC/WPF recovery adapter는 구현됐다고 기록됨.
- capability bit 6, advertised buffer count 2, qualification/reconnect proof gates는 계속 OFF.
- LASAL compile/link의 2.56 MB global RAM 배치, task jitter, A/B capture/upload 불변성, third `ResourceBusy`, reconnect/adopt/release 및 crash-window를 실제 PLC에서 검증해야 함.

### 우선순위 4 — D5/D1 실장 증거

- D5 contention/timeout/queued-cancel/transport recovery는 PC core·fake-RPC 수준. `docs/architecture/LMC_D5_ORPHAN_WITNESS_IDE_HANDOFF_2026-07-28.md` 구조와 PLC witness가 없으므로 orphan-qualified가 아님.
- D1 `0x7E20` detail mapping 변경은 tracked source/static 수준이므로 rebuild/download 뒤 single-axis offline→recovery 시험과 payload 확인이 필요.

### 작업 재개 시 첫 확인

1. `git status --short`와 관련 source diff를 다시 읽어 이 히스토리 이후 변경 여부를 확인한다.
2. 실행 중 GUI의 실제 경로/빌드 시각과 PLC BootId/Capabilities/MapRevision을 확인한다. 히스토리에 여러 임시 `CodexLatest`/smoke 출력이 섞여 있으므로 경로만 보고 최신이라고 단정하지 않는다.
3. LASAL IDE에서 T2/D5 witness 구조가 이미 생성됐는지 확인한다. 없다면 generated 선언/network를 외부에서 합성하지 않는다.
4. PC/static/build PASS와 PLC runtime/pcap PASS를 분리해 기록한다.
5. 히스토리 기록상 이 구간에서는 commit, 추가 staging, 테스트 폴더 복사를 하지 않았다. 현재 상태는 반드시 다시 확인한다.

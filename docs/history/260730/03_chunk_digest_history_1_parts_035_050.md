# Elmo_Master history 1 parts 035-050 digest

## 읽기 범위와 판정 원칙

- 대상: `Elmo_Master_history_260730_1_part_035_lines_08501_08750.md`부터 `part_050_lines_12251_12285.md`까지 16개 물리 청크
- 원본 논리 행 범위: `08501-12285`
- 이 문서는 위 히스토리의 요청, 수행 주장, 당시 검증 결과, 실패/정정, 미완료 항목을 정리한 인계 자료다.
- 아래의 구현 상태와 PASS/FAIL 수치는 모두 **히스토리 기록상 당시 체크포인트**다. 현재 working tree, 현재 바이너리, LASAL IDE 생성물, PLC runtime의 현재 사실로 재사용하려면 반드시 다시 확인해야 한다.
- 원본은 compaction 결과가 중첩되어 있어 한 청크 안에 최종 답변과 그보다 앞선 메시지가 역순으로 포함되기도 한다. 아래 시간순 단계는 메시지 의미와 테스트 수치의 진전을 기준으로 재구성했다.
- `ACK`는 완료로 보지 않는다. 특히 motion, power, reset, stop, group 명령은 ACK 뒤 상태 polling, 안정 표본, identity 확인 또는 readback을 별도로 요구한 기록으로 정리했다.

## 청크별 digest

### Part 035 — source lines 08501-08750

- 주요 요청/초점: CREVIS 구성 표시와 SDO 편집 동작을 유지하면서 LASAL IDE 없이 진행 가능한 API/WPF/정적 계약 공백을 계속 구현.
- 수행/변경 기록:
  - SDO same-value Write 결과(`PASS`/`RECOVERY REQUIRED`)와 다음 요청 draft를 UI 갱신과 분리해 보존.
  - 1/2/4-byte `ReadSdoInline[Async]` 공개 API를 추가하고 같은 ticket의 terminal `Completed/Success`, type, length, data를 확인하도록 구성.
  - `LMCDiagnosticsService` constructor의 D5/Boot 초기화와 뒤이은 Bulk/Recorder 상태 초기화 정적 gate 강화.
  - terminal 응답 뒤 UI cancel이 결과를 덮지 않도록 하고, WPF에 inline read 및 PC-only wait cancel 경로 추가.
  - Axis/Group lookup typed result/exception, WPF Admin/Drive 실제 control 테스트, Group Enable 안정 대기 작업을 후속으로 착수.
- 당시 검증 기록: API `603/603` 후 `605/605`, WPF `23/23` 후 `29/29`, LASAL SourceOnly/full static PASS, `git diff --check` PASS. `607/607` 및 WPF `30/30`은 이 청크 말미에는 진행/예상 상태이고 다음 청크에서 확정됨.
- 실패/정정/철회:
  - `/checked+` 테스트 바이너리를 증분 빌드가 재사용해 unchecked 전제 테스트가 실패했고 일반 설정 강제 Rebuild로 정정.
  - LF→CRLF 경고를 공백 오류로 오인했다가 내용 오류가 아닌 것으로 정정.
  - 테스트 요약 추출 패턴 불일치와 병렬 빌드 `obj` 잠금은 제품 결함으로 보지 않고 순차/원문 확인으로 재검증.
- 남은 작업: CREVIS live `0x7E13/0x7E22`, SDO Write 실제 gate/allowlist, Group Enable 동시성 및 TCP 재연결 상태 정리.

### Part 036 — source lines 08751-09000

- 주요 요청/초점: typed lookup과 constructor gate를 마감하고, static CREVIS 표시 및 Group Enable의 ACK 이후 완료 증명을 안전하게 연결.
- 수행/변경 기록:
  - `LMCLookupResult`/`LMCLookupException`으로 Axis/Group lookup 응답과 raw failure evidence를 구조화.
  - diagnostics constructor의 38개 상태 이름/정확 타입, scalar exact-once, `BulkSignalIds[0..23]`, 초기화 순서를 검사.
  - Test2 캡처에서 7-entry topology 및 CREVIS 3-entry를 확인했다는 감사 문서 작성. 기존 `0x7E10` Health는 Elmo 4축 경로이며 CREVIS live Health가 아님을 구분.
  - 새 TCP 연결 시 이전 session queue/RPC 등록을 초기화하는 reconnect reset과 Group Enable 1회 송신 후 `0x2045` Locked Standby 3회 안정 확인을 구현.
  - WPF에 자동 topology load, SDO editable draft, Group Enable Resume/Disable 경쟁 처리를 연결.
- 당시 검증 기록: typed lookup 단계 API `607/607`, WPF `30/30`; 후속 통합 API `636/636`, WPF `32/32`; LASAL SourceOnly/full, PowerShell parser, ASCII, `git diff --check` PASS.
- 실패/정정/철회:
  - 정적 gate 담당 작업 지연 후 회수해 직접 완료.
  - 변수명만 검사해 타입 변이를 놓친 허점을 찾아 정확 타입 검사로 보강.
  - 병렬 Debug/Release 빌드 출력 경합 및 오래된 문서/경고 문구를 수정.
- 남은 작업: Group Power On/Off의 accepted-once 완료 확인, Group/Profile Lock durable recovery, CREVIS live IDE 구조, SDO Write gate.

### Part 037 — source lines 09001-09250

- 주요 요청/초점: Group Power On/Off를 명령 1회와 상태 3회 안정 확인으로 바꾸고, safety 명령과 마지막 상태 응답의 경쟁을 닫음.
- 수행/변경 기록:
  - `0x204A/0x204B`는 한 번만 송신하고 이후 `0x2045` status-only Resume을 수행하도록 SDK/WPF를 확장.
  - 단일 상태 read가 pending을 완료하지 못하게 하고, safety generation, stale UI result, Disable/Stop 선점, Close/name/reload 우회를 차단.
  - `Lock state uncertain → Disable required`와 PowerOff 3회 확인에 의한 복구 조건을 분리.
  - 실시간 지연 기반 테스트를 제어된 barrier로 바꾸고 실제 socket reconnect 회귀 및 group recovery journal 작업을 시작.
- 당시 검증 기록: 중간 API `646/646`; WPF `44/44`; 이후 SDK `649/649`, WPF `46/46`까지 기록. LASAL static은 계속 PASS로 기록됨.
- 실패/정정/철회:
  - .NET Framework WPF 프로젝트를 `dotnet run`으로 실행해 XAML 생성이 빠진 실패를 VS MSBuild 경로로 정정.
  - `LegacyEnableBlockedWhileWaitActive`가 150 ms 실시간 지연에 의존해 간헐 실패한 것을 결정론적 barrier로 교체하고 50회 반복 PASS.
  - 테스트 cleanup이 `journal.lock`을 남긴 문제와 여러 safety/result publication race를 수정.
- 남은 작업: endpoint/group reference/BootId/MapRevision을 포함한 durable Group Profile Lock 복구 완성, CREVIS live IDE 구조.

### Part 038 — source lines 09251-09500

- 주요 요청/초점: Group Enable/Profile Lock을 프로세스 재시작과 PLC identity 변화에도 fail-closed로 복구하고, SDO draft/배포 추적성/ENI drift 검사를 진행.
- 수행/변경 기록:
  - `MainWindow.GroupProfileLockRecovery.cs` 기반 durable journal 추가.
  - Enable 전 endpoint, group reference, BootId, MapRevision을 기록하고 복구 직전 fresh capability identity를 재확인.
  - active journal 자체를 unresolved로 취급하고 Group Reset/Set Identity 등 우회 mutation을 차단.
  - SDO exact-readback 전 draft 보존 및 같은 세션 VERIFIED에서만 안전 복원.
  - release manifest 자동 생성/검증과 ENI↔Network↔serializer↔generated table drift gate 작업 착수.
- 당시 검증 기록: SDK `649/649`, WPF `57/57`; restart/endpoint/group-reference 회귀 및 post-identity race 테스트 PASS. 이후 SDO draft 단독 smoke와 manifest fixture가 진행됨.
- 실패/정정/철회:
  - journal arm 직후 ACK 전 단절, 안전 상태 관측 뒤 journal을 너무 일찍 삭제, identity 응답 대기 중 safety reservation, identity 확인 실패 뒤 continuation 소실 등 경계를 순차 보강.
  - recovery journal을 단순 메모리 latch로만 처리하던 방향을 영속 record로 강화.
- 남은 작업: manifest/ENI gate 전체 마감, configured topology evidence, CREVIS T2 IDE 구조.

### Part 039 — source lines 09501-09750

- 주요 요청/초점: 배포/토폴로지 증거의 재현성을 닫고, Group Stop의 ACK 이후 stable Standby 완료 API를 구현.
- 수행/변경 기록:
  - SDO draft 보존, atomic `RELEASE_MANIFEST.md`, ENI/Network/7-entry serializer/generated table 교차검증을 완료했다고 기록.
  - configured topology를 `INITIAL/UNCHANGED/CHANGED`, SHA-256, ordered diff로 비교하고 TXT evidence를 저장하도록 WPF 확장.
  - evidence에는 configured schema이며 runtime discovery/live I/O 증거가 아님을 명시.
  - Group Stop 1회 송신 후 stable state를 확인하는 facade와 취소/응답 유실/다른 mutation 간섭 방지를 구현하기 시작.
- 당시 검증 기록: SDK `649/649`, WPF `57/57`, manifest `39/39`, LASAL static PASS; topology evidence 후 WPF `59/59`; Group Stop 수정 중 SDK Debug `662/662`까지 기록.
- 실패/정정/철회:
  - WPF 실제 프로젝트 경로를 한 단계 잘못 잡았다가 정정.
  - `dotnet build`/병렬 WPF 빌드의 XAML 생성 충돌을 제품 오류로 보지 않고 VS2019 MSBuild 순차 검증으로 전환.
  - Group Stop에서 response loss, disconnect 직전 결과, mutation generation 시점의 P1 경계를 수정.
- 남은 작업: Group Stop WPF priority orchestration, CREVIS live evidence UI, PLC의 T2 live owner.

### Part 040 — source lines 09751-10000

- 주요 요청/초점: Group Stop 전체를 동일 priority generation으로 귀속하고, CREVIS live 응답을 받을 준비가 된 GUI evidence 및 남은 mutation publication 경계를 보강.
- 수행/변경 기록:
  - Auto/Manual Health·DI evidence history, 4,096-entry FIFO, drop count, TXT/CSV export를 WPF에 추가. stale session/topology 응답은 기록하지 않도록 함.
  - Group Stop 1회와 3회 Standby 판정을 같은 priority generation/command gate로 묶고 새 safety 요청이 선점 가능하도록 구현.
  - Axis/Admin/D5 SDO의 늦은 ACK는 accepted evidence를 보존하되 stale success publication을 폐기하도록 보강.
  - zero-wire SDO Write readiness matrix와 Recorder Trigger/Stop/Release의 stale-result 격리 추가.
- 당시 검증 기록: SDK `664/664`, WPF `66/66`, LASAL SourceOnly PASS; 후속 mutation/readiness/Recorder 보강 후 SDK `691/691`, WPF `66/66`으로 기록.
- 실패/정정/철회:
  - WPF Debug DLL/PDB가 실행 중 앱/VS에 잠겨 별도 출력/Release로 검증.
  - `dotnet build`가 구형 WPF XAML target을 로드하지 못한 실패를 VS2019 MSBuild로 정정.
  - readiness가 session/capability snapshot을 원자적으로 읽지 않아 잠깐 READY가 될 수 있는 경계를 수정.
- 남은 작업: Recorder Configure/Start/Adopt accepted-result 복구, nonmodal SDO 확인, 실제 CREVIS live PLC route.

### Part 041 — source lines 10001-10250

- 주요 요청/초점: Recorder 반환 자원의 accepted-result를 잃지 않게 하고, SDO Write modal 확인창을 없애 편집 가능성을 실제 UI에서 유지.
- 수행/변경 기록:
  - `0x7E23` 결과 publication과 Recorder 6개 resource-creation 경로에 recovery-only handle/identity/lease 보존 모델을 추가.
  - callback을 no-throw로 만들고 원래 preemption 예외를 보존.
  - SDO Write를 비모달 2단계 `Arm → Submit`으로 변경. 입력 변경 시 arm snapshot을 즉시 폐기하고 재확인하도록 함.
  - configured CREVIS 7행/3행과 live Health/DI/DO 미구현 경계를 다시 문서화.
- 당시 검증 기록: 시작 체크포인트 SDK `691/691`, WPF `66/66`; Recorder 보강 SDK `709/709`; 최종 WPF `67/67`, `git diff --check` PASS.
- 실패/정정/철회:
  - WPF `dotnet build` XAML 실패와 Debug 파일 잠금을 다시 분리해 Release/임시 출력으로 검증.
  - same-value qualification에 남은 modal 함수, edit 후 버튼 label이 Submit으로 남는 문제, 테스트 journal cleanup 문제를 수정.
- 남은 작업: Recorder Double config-only durable adapter, LASAL T2 client/handler/network 생성, 실기 SDO/CREVIS 검증.

### Part 042 — source lines 10251-10500

- 주요 요청/초점: Recorder 동시성/Double 경로를 fail-closed로 분리하고 durable cleanup 범위를 정립.
- 수행/변경 기록:
  - 동일 handle 동시 Start와 네 종류 Release의 BeforeWire rollback 테스트 추가.
  - 수동 Double Configure가 일반 `0x7E40` 경로로 잘못 들어가지 않도록 분리하고 durable recovery 전까지 송신 차단.
  - accepted Configure 결과를 recovery-only로 보존하고 명시 Release로 정리하는 WPF 경로 추가.
  - Configure-only `ARM→CONFIGURE→CHECKPOINT→RETAIN` adapter, journal, response-loss/release-loss 회귀 구현.
  - capability snapshot의 owner/session/BootId/MapRevision을 SDK가 고정한 뒤 journal arm 및 Configure를 수행하도록 보강.
- 당시 검증 기록: 초기 SDK `711/711`, WPF `69/69`; 후속 Double adapter SDK `714/714`, WPF `74/74`까지 기록.
- 실패/정정/철회:
  - 강제 `OutputPath`/일반 dotnet 경로의 XAML 실패를 유효 빌드로 보지 않음.
  - 테스트가 3개 journal lock을 닫지 못한 문제와 response barrier harness 문제 수정.
  - capability A로 arm하고 B로 Configure할 수 있던 identity race, 변경 가능한 `config-only` bool 권한 경계를 고정 scope로 교체.
- 남은 작업: scope 권한 수정 최종 재검증, `IdeStructureReady` gate, motion durable recovery.

### Part 043 — source lines 10501-10750

- 주요 요청/초점: Recorder scope 수정 마감 후 CREVIS IDE-ready 중간 checkpoint와 Move 불확실성 영속 복구를 구현.
- 수행/변경 기록:
  - config-only scope 권한 경계를 고정하고 SDK `715/715`, WPF `74/74` 체크포인트 기록.
  - `StaticTopologyOnly`와 live 구현 사이의 `IdeStructureReady` 정적 checkpoint 추가. client/method/network/빈 stub/capability 비트까지 검사.
  - 모든 Move에 durable journal, fresh BootId/MapRevision, exact endpoint/axis identity, 재시작 recovery를 적용.
  - Close/identity mismatch/명령 replay를 차단하고 명시적 Stop/PowerOff ACK와 안정 상태로만 journal 해제.
  - Axis PowerOn accepted-once 복구 및 CREVIS verifier 강화 작업을 후반에 착수.
- 당시 검증 기록: Motion journal 단위 `9/9`, WPF가 `83/83→89/89→96/96`, SDK `715/715`, LASAL SourceOnly/full static PASS로 기록. `IdeStructureReady`는 `Coupler` client 누락으로 예상 FAIL.
- 실패/정정/철회:
  - verifier가 빈 stub, 몰래 켠 capability, CRLF/숫자 표기, 중첩 CASE 우회를 놓친 허점을 보강.
  - motion journal 테스트 cleanup 2건과 fresh identity/final resolve/safety race 4건을 수정.
- 남은 작업: Axis PowerOn durable recovery 완성, CREVIS IDE 구조 생성, later full PLC proof.

### Part 044 — source lines 10751-11000

- 주요 요청/초점: 진행 중 Axis PowerOn 복구와 별도로, 사용자가 외부 시험에서 해결한 비정상 종료 client의 stale socket 문제를 master 개발 소스에 반영하라고 명시 요청.
- 수행/변경 기록:
  - 외부 시험 문서/WTR 기록과 master를 비교하고 takeover 관련 파일만 선별 반영.
  - editable derived `TCPIPServer`, `TCPMotionInterface`, `Comm_Network`, project registration에 `MaxConnections=2`, same IPv4 old socket shutdown, new owner takeover, stale callback/session isolation을 반영했다고 기록.
  - 예전 `_TCPIPServer_RT` source를 제거하고 `LMCRecorderStore`의 DINT↔UDINT 비교 3건을 별도 수정.
  - Axis PowerOn ACK 직후 `AcceptedAwaitingProof` journal flush, reconnect status-only 복구, unresolved 동안 diagnostics mutation 차단을 병행 구현.
- 당시 검증 기록: SDK가 중간 `722/722→724/724→725/725`, WPF `105/105→108/108`; LASAL SourceOnly PASS, ASCII 및 `git diff --check` PASS.
- 실패/정정/철회:
  - 테스트 프로젝트 전체/생성물/Recorder 변경을 그대로 복사하지 않고 takeover 핵심만 선별.
  - full static은 master `Classes.lcb`가 이전 `_TCPIPServer_RT`를 등록해 의도한 FAIL로 기록. 외부 시험 PASS를 master runtime PASS로 승격하지 않음.
  - PowerOn ACK와 WPF journal 사이 crash window, unresolved PowerOn 중 진단 mutation 허용 문제를 추가 수정.
- 남은 작업: master LASAL Save/Generate/Rebuild/Link와 metadata 동기화, master PLC same-peer 재시험, Axis Reset stable clearance, CREVIS T2.

### Part 045 — source lines 11001-11250

- 주요 요청/초점: Axis recovery cleanup을 닫고 Reset/DS402/PowerOff/Stop 안전 facade를 연속 구현.
- 수행/변경 기록:
  - disconnect cleanup에서 journal I/O 실패가 다른 cleanup을 막지 않는 회귀 추가.
  - Axis Reset `0x2024` 1회 후 `0x2028`의 `AxisErrorId=0` 3회 안정 확인 API/WPF 추가.
  - `0x6041:0` bit 3 `HasDs402Fault`와 `0x603F:0 UInt16/2-byte` `GetDriveErrorCode[Async]` read-only 진단 추가.
  - Axis PowerOff `0x2023(false)` 1회 + `PowerOn=false && Standstill` 3회 facade 추가.
  - Axis Stop 입력 `deceleration>0`, `jerk>=0`을 C# pre-wire와 LASAL handler 양쪽에 적용하고 accepted-once Stop 완료 facade를 진행.
- 당시 검증 기록: Reset 단계 SDK `733/733`, WPF `110/110`; DS402/PowerOff 단계 SDK `752/752`, WPF `110/110`; 이후 split PowerOff `762/762`, Stop facade `773/773`/`774/774`까지 기록.
- 실패/정정/철회:
  - WPF의 `0x603F` 2-byte preflight를 1로 넣은 오류를 2로 수정.
  - Stop malformed ACK 문제라는 리뷰는 parser가 이미 거부하므로 false positive로 정정했지만, response-loss/priority 회귀는 추가.
  - ACK를 DS402 Fault 해제 증거로 보지 않고 `AxisErrorId`, `0x6041`, `0x603F`를 별도 관측으로 유지.
- 남은 작업: Stop/Reset/PowerOff total deadline과 pre-wire cancel, split Begin/Resume WPF 통합, LASAL full rebuild.

### Part 046 — source lines 11251-11500

- 주요 요청/초점: Axis safety facade의 timeout/evidence를 통일하고 Group Stop/Enable 및 Axis Stop attribution을 강화.
- 수행/변경 기록:
  - Stop/Reset/PowerOff의 post-write 무응답에서 transport를 detach하고 connection을 `Faulted`로 만들어 stream 재사용을 금지.
  - Axis Stop을 `Begin(0x2022 1회)/Resume(0x2028 only)`로 분리하고 WPF 연결.
  - Axis PowerOn과 read-only wait에도 total deadline/evidence를 적용.
  - Group Stop G1 total deadline과 G2 `Begin(0x2085 1회)/Resume(0x2045 only)` continuation, WPF durable reuse 구현.
  - Group Enable ACK/status 무응답 및 Close/Reopen session publication을 보강하고 Axis Stop 같은-axis mutation attribution 추가.
- 당시 검증 기록: SDK `778/778→784/784→793/793→801/801→815/815→819/819→824/824`; WPF `111/111→112/112→114/114`; LASAL SourceOnly PASS.
- 실패/정정/철회:
  - Stop 구현 파일 교체 중 파일이 잠시 비어 있었으나 컴파일 가능한 전체 소스로 복구 후 검증.
  - `dotnet msbuild`의 WPF XAML 실패를 VS MSBuild로 정정.
  - Group Enable 마지막 sample이 Close/Reopen 뒤 게시될 수 있던 session race와 동시 Resume 테스트 race를 수정.
- 남은 작업: Axis Reset accepted-once split, PowerOff/PowerOn 동일 축 attribution 완성, WPF durable recovery 확대.

### Part 047 — source lines 11501-11750

- 주요 요청/초점: Axis Reset을 accepted-once split state machine으로 만들고 PowerOff/PowerOn/GroupStop의 간섭·최종 publication을 같은 수준으로 통일.
- 수행/변경 기록:
  - Reset `Begin(0x2024 1회)/Resume(0x2028 only)`와 timeout/cancel/status failure continuation 보존, same-axis interference, 명시적 re-Reset 정책 구현.
  - PowerOff에 mutation generation 및 typed interference를 추가하고 transient failure는 status-only, confirmed interference에서만 `Power Off Again` 허용.
  - Axis PowerOn attribution과 GroupStop cancel/deadline/final-state 선형화를 보강.
  - WPF에서 pending 중 axis reload/name change/Close 우회 차단 및 rejection 시 기존 pending 보존.
- 당시 검증 기록: Reset 단계 SDK `845/845`, WPF `117/117`; PowerOff 단계 `859/859`, WPF `120/120`; 후속 통합 SDK `876/876`, WPF `125/125`; LASAL SourceOnly와 `git diff --check` PASS.
- 실패/정정/철회:
  - 기존 PowerState 동시성 테스트가 1회 실패했으나 clean 반복 통과; 독립 리뷰로 연관성을 점검.
  - 마지막 sample 직후 cancel/deadline이 성공을 뒤집는 P1 선형화 결함을 수정.
  - 문서의 작업일을 미래 `2026-07-30`으로 쓴 4건을 당시 실제 작업일 `2026-07-29`로 정정.
- 남은 작업: Group Power durable journal, Axis PowerOff 프로세스 재시작 durable proof, master LASAL generated metadata.

### Part 048 — source lines 11751-12000

- 주요 요청/초점: Group Power On/Off와 Axis PowerOff, Group Enable을 실제 프로세스 강제 종료 뒤에도 명령 재전송 없이 복구.
- 수행/변경 기록:
  - Group Power `0x204A/0x204B` accepted-once continuation과 단일 durable journal, On→Off 원자 교체, exact identity 복구 추가.
  - SDK callback 재진입 deadlock을 수정하고 child-process Kill 후 재시작에서 명령 0회/status 3회/Resolved를 검증.
  - Axis Power journal을 direction 포함 v2로 확장. PowerOff ACK 직후 journal을 저장하고 재시작 시 `0x2023=0`, `0x2028=3`으로 복구.
  - Group Enable ACK도 `AcceptedAwaitingProof`로 영속 기록하고 재시작 시 `0x2047=0`, `0x2045=3`으로 복구.
  - active recovery 중 Axis reload의 handler-level pre-wire 차단과 ACK journal failure의 UI-thread marshaling 보강.
- 당시 검증 기록: Group Power 단계 SDK `901/901`, WPF `141/141`; Axis PowerOff 단계 SDK `906/906`, WPF `156/156`; Group Enable 단계 SDK `911/911`, WPF `160/160`; LASAL SourceOnly 및 `git diff --check` PASS.
- 실패/정정/철회:
  - callback이 lock 아래 재진입해 deadlock할 수 있던 구조를 lock release 후 callback으로 수정.
  - stale tombstone/result가 새 record를 되살리거나 replacement 권한을 지우는 race, fixture의 새 final `0x7E00` 누락, ACK 없는 복구에서 Accepted를 주장한 문구를 수정.
  - 반복 WPF timeout을 테스트 barrier 문제로만 넘기지 않고 handler 직접 호출이 RPC를 내보내던 production guard 누락까지 수정.
- 남은 작업: Group Disable durable recovery, Axis Stop/Reset child-process durability, master LASAL full build/runtime.

### Part 049 — source lines 12001-12250

- 주요 요청/초점: Group Disable을 ACK가 아닌 stable Disabled proof로 완성하고 Axis Stop/Reset의 비정상 종료·재시작 복구까지 확대.
- 수행/변경 기록:
  - Group Disable `0x2048` accepted-once continuation과 Lock/Unlock direction journal v2를 구현.
  - 완료 조건을 `PowerOn && Disabled && !Standby`인 `0x2045` 상태 3회 연속으로 정의.
  - LASAL `0x2048` handler는 `UnlockProfile()` native 접수 결과만 ACK로 반환하고 실제 완료는 polling으로 증명하도록 정적 계약과 맞춤.
  - Axis Stop/Reset 공용 durable journal, same-axis generation, raw replay guard, Reset→Stop safety takeover, held-status transport abort/new connection/fresh identity 검증 구현.
  - 외부 시험본과 master socket 관련 4개 파일의 실행 구조/해시 동일성을 재확인했다고 기록.
  - Reset 완료와 Stop NACK가 겹칠 때 최종 D0 identity 일치에서만 tombstone을 해제하도록 보강.
- 당시 검증 기록: Group Disable SDK `941/941`, WPF `175/175`, child Kill 후 `0x2048=0`, `0x2045=3`; Axis SDK Debug/Release `974/974`; WPF는 첫 전체 실행 `192/205 PASS, 13 FAIL` 후 분류/수정하여 `206/206 PASS`, warning/error `0/0`; LASAL `Phase5TransportClean/StaticTopologyOnly` PASS.
- 실패/정정/철회:
  - 처음에는 Disable NACK면 이전 locked 상태를 복원할 수 있다고 보았으나, `UnlockProfile()`이 효과를 냈어도 같은 cycle `LockState` 때문에 NACK일 수 있음을 확인해 **철회**. 오직 pre-wire `NotAttempted`만 복원 허용.
  - PowerOff가 먼저 끝난 뒤 늦은 Disable failure가 journal을 오염시키는 race, Disable continuation 영구 pending, ACK/Close publication race를 수정.
  - SDK 전체 시험이 장시간 정지한 것을 PASS로 처리하지 않고 bounded timeout 경로로 고침.
  - WPF 13건 실패를 Reset 7/Motion 5/Axis Power 1로 분류하여 fixture 누락과 실제 UI 회귀를 구분; Reset-completed/Stop NACK의 D0 미확인 P1은 production 수정.
- 남은 작업: 문서 수치/해시/diff 최종 확인, master LASAL generated metadata 갱신과 PLC runtime 검증.

### Part 050 — source lines 12251-12285

- 주요 요청/초점: Part 049의 구현과 문서 수치를 최종 정리하고 남은 경계를 인계.
- 수행/변경 기록: 별도 신규 기능보다 socket takeover 4개 파일, Axis Stop/Reset durable recovery, Reset-completed/Stop-NACK D0 identity 정책의 최종 요약.
- 당시 검증 기록: SDK Debug/Release `974/974`, WPF Release build warning/error `0/0` 및 `206/206`, LASAL SourceOnly `Phase5TransportClean/StaticTopologyOnly` PASS, socket 파일 SHA-256 일치, working-tree `git diff --check` PASS.
- 실패/정정/철회: cached check는 기존 staged `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에만 실패했다고 기록. 해당 파일/stage는 건드리지 않음.
- 남은 작업:
  - master `Classes.lcb`, `Networks.lcb`, `.lba` 등 이전 `_TCPIPServer_RT` 기준 생성물 갱신.
  - LASAL Save/Generate → Rebuild/Link → Find in Implementation smoke → PLC download.
  - master에서 same-IP abnormal-close/reconnect 및 첫 명령 응답 재검증.
  - 다른 IP 거절, peer 조회 실패, 반복 reconnect/soak는 실기 미검증.

## 전체 구간의 시간순 단계

1. **SDO/CREVIS GUI 기본 정리**
   - configured topology 7행/CREVIS 3행 표시, editable SDO draft, terminal-aware inline read, constructor static gate를 추가.
   - configured topology와 live Health/DI/DO를 명시적으로 분리.

2. **Group Enable/Power/Stop의 ACK 이후 안정 상태 모델**
   - Group Enable/Power/Stop을 command 1회와 `0x2045` status-only polling으로 분리.
   - safety generation, cancel/deadline, Disable/PowerOff 선점, 같은/다른 handle 경합을 보강.

3. **영속 저널과 identity 기반 복구 확대**
   - Group Profile Lock, motion, Axis/Group Power, Group Enable 등에 endpoint/reference/BootId/MapRevision 기반 durable journal 적용.
   - 프로세스 종료 후 명령을 재전송하지 않고 상태 조회와 명시적 cleanup만 수행하도록 강화.

4. **토폴로지/배포/Recorder 증거 강화**
   - topology diff/evidence export, ENI↔Network↔serializer drift gate, release manifest, Recorder accepted-resource recovery와 Double config-only adapter를 추가.
   - configured schema는 runtime discovery가 아니라는 경계를 유지.

5. **Axis safety facade 정교화**
   - Reset, Stop, PowerOn, PowerOff를 accepted-once Begin/Resume 또는 compound 계약으로 확장.
   - `0x2024/0x2022/0x2023` ACK 뒤 `0x2028` 안정 표본, same-axis mutation attribution, transport timeout fail-closed를 적용.
   - DS402 Fault(`0x6041` bit 3), AxisErrorId, drive error `0x603F`를 서로 다른 관측으로 유지.

6. **외부 시험 same-peer takeover의 master 선별 역반영**
   - 사용자의 명시 요청으로 외부 시험에서 통과한 동일 IPv4 stale session takeover를 editable `TCPIPServer`, `TCPMotionInterface`, Comm Network/project registration에 선별 반영.
   - 테스트 프로젝트 전체나 LASAL 생성물을 덮어쓰지 않았고, 외부 시험 PASS를 master PLC PASS로 승격하지 않음.

7. **강제 종료·재시작 내구성 검증 확대**
   - Axis PowerOff, Group Power, Group Enable, Group Disable, Axis Stop/Reset에 child-process Kill 회귀를 추가.
   - ACK 뒤 재시작에서 원 명령 0회와 status-only 3회, journal single-writer 재획득, identity 일치 resolve를 확인했다고 기록.

8. **Group Disable 및 Axis Stop/Reset 최종 복구 모델**
   - Disable 완료를 ACK가 아닌 `PowerOn && Disabled && !Standby` 3회로 규정.
   - Reset→Stop safety takeover는 in-flight old transport를 폐기하고 새 session에서 identity를 다시 확인한 뒤 Stop 1회만 허용.
   - Reset 완료/Stop NACK도 최종 D0 identity가 일치해야만 해제.

9. **구간 종료 체크포인트**
   - 기록상 PC/fake-RPC: SDK `974/974`, WPF `206/206`.
   - 기록상 LASAL source-only: `Phase5TransportClean/StaticTopologyOnly PASS`.
   - 기록상 full LASAL: 이전 generated metadata 때문에 아직 완료 아님.

## 후속 파일에 넘길 상태

### 기록상 구현 완료 또는 PC/정적 검증 완료

- Same-peer takeover 실행 소스/네트워크가 외부 시험본과 일치한다고 기록됨.
- SDO draft 편집/비모달 Arm-Submit/terminal inline read/readiness, configured CREVIS topology/evidence UI가 구현됐다고 기록됨.
- Axis Stop/Reset/PowerOn/PowerOff와 Group Enable/Disable/Power/Stop의 accepted-once, status-only Resume, durable journal, identity/generation guard가 광범위한 fake-RPC/child-process 테스트를 통과했다고 기록됨.
- 마지막 기록 수치는 SDK Debug/Release `974/974`, WPF Release `206/206`, WPF build warning/error `0/0`, SourceOnly `Phase5TransportClean/StaticTopologyOnly PASS`.

### 반드시 현재 source에서 재확인할 사항

- 위 수치와 파일 일치 여부는 히스토리일 뿐이다. 다음 분석자는 먼저 `git status`, 현재 diff, 관련 source/hash, 테스트 카운트, generated metadata를 다시 확인해야 한다.
- socket takeover 비교 대상 4개 파일의 현재 SHA-256과 external test copy의 존재/경로를 다시 확인해야 한다.
- `Classes.lcb`, `Networks.lcb`, `.lba`, export metadata가 새 `TCPIPServer`를 반영했는지 확인해야 한다. 히스토리 마지막 상태는 이전 `_TCPIPServer_RT` 등록 때문에 full static FAIL이었다.
- 기존 staged history trailing whitespace 문제도 현재 stage 상태를 다시 확인해야 한다. 이 구간에서는 해당 stage를 수정하거나 commit하지 않았다고 기록돼 있다.

### IDE/PLC/실기 미완료

- master LASAL 프로젝트 Save/Generate, Rebuild All, Link, `Find in Implementation` smoke, `%TEMP%\Lasal2.log`의 새 `CInvalidArgException` 확인.
- master PLC download 후 same-IP abnormal close→reconnect→첫 RPC 응답 확인.
- 다른 IP 거절, peer query 실패, 반복 reconnect/soak.
- CREVIS live Health/DI/DO의 `LMCEcatInputLatch` client 3개, diagnostics handler, Motion Network 연결, `0x7E13/0x7E22/0x7E23`, capability bits 15-17. 마지막 기록은 `StaticTopologyOnly`이며 live 값은 미완료.
- 실제 SDO Write는 approved target/allowlist와 PLC/SDK gate가 닫힌 상태로 기록됨. 실기 Write 및 exact readback qualification은 별도 승인/시험 필요.
- 실제 축에서는 ACK 외에 Stop/Reset/Power 상태, DS402 Fault, AxisErrorId, `0x603F`, 최종 위치/readback을 별도 계측해야 한다.

### 다음 작업 권장 시작점

1. 현재 `git status`와 socket/axis/group/CREVIS 관련 diff를 읽어 히스토리 이후 변경 유무를 확인한다.
2. SourceOnly와 full static을 각각 실행해 generated metadata가 아직 blocker인지 판정한다.
3. 사용자의 LASAL Save/Rebuild/Link 결과가 이미 생겼다면 `Classes.lcb`/Network/export와 tracked `.st/.lcn/.lcp`를 교차 확인한다.
4. IDE build가 통과한 경우에만 master PLC download와 same-peer takeover 실기 재검증으로 간다.
5. CREVIS live와 SDO Write는 별도 안전 gate로 유지하고, static topology/PC fake-RPC 결과를 runtime proof로 부르지 않는다.

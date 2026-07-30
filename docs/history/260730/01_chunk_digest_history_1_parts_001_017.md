# `Elmo_Master_history_260730_1` 청크 분석: parts 001-017

## 분석 경계

- 대상은 `Elmo_Master_history_260730_1.md`의 원본 1-4,250행을 옮긴 250행짜리 물리 청크 17개다.
- 아래 내용은 **히스토리에 기록된 요청, 변경 주장, 검증 결과와 당시 판단**을 요약한 것이다. 현재 저장소의 구현·빌드·PLC 상태로 단정하지 않는다.
- 특히 커밋 해시, 테스트 개수, LASAL Build/Link/Download, PLC 동작은 후속 히스토리나 현재 파일에서 폐기·변경됐을 수 있다. 이어서 작업할 때 live source, `git status`, 정적 계약, LASAL 로그와 실기 결과를 다시 확인해야 한다.
- 이 분석에서는 현재 dirty worktree를 변경 근거로 사용하지 않았고, 지정된 digest 이외의 파일은 수정하지 않았다.

## 청크별 digest

### Part 001 — source lines 00001-00250

- **주요 사용자 요청:** 260724 대형 히스토리를 날짜 폴더에 나누어 분석하고 이어서 작업; 이후 변경을 종류별로 커밋하고, 시험보다 구현을 먼저 진행.
- **수행·변경 기록:** 51 MB급 히스토리를 194개로 분할·인덱싱했다는 이전 작업을 요약하고, dormant `LMCControlCommandService` 및 9축 topology checkpoint를 포함한 5개 커밋을 정리했다. 이어 Group 11개+Admin 2개를 서비스로 보내는 Phase 3B 라우팅 구현을 시작했다.
- **검증 기록:** 히스토리 분할 hash/재결합, PC Debug/Release 148/148, WPF 양 구성, dormant/full 정적 계약, diff check가 통과했다고 기록한다. Phase 3B도 SourceOnly 계약까지 통과했다고 보고했다.
- **실패·정정·철회:** 처음에는 Network 객체/연결이 없어 Phase 3B 불가라고 판단했지만, 사용자 저장 작업으로 service object·11개 연결·generated table이 생겨 그 blocker를 폐기했다. Rebuild/Download는 런타임 동작 증거가 아니라는 경계를 유지했다. `TestClass`와 0-byte 로그는 커밋에서 제외했다.
- **남은 작업:** Phase 3B full-network 판정과 문서 동기화, 냉간 다운로드 후 `0x20E7` 재초기화, Phase 4/5, 실제 패킷·성능·축 시험.

### Part 002 — source lines 00251-00500

- **주요 사용자 요청:** Phase 4/5까지 구현을 계속하고, 구현과 시험을 병행하되 시험용 소스를 별도 폴더에 복사.
- **수행·변경 기록:** Phase 3B 완료를 선언한 뒤 Axis 8개, Registry 3개, 잔여 Admin 2개까지 합쳐 Control 26개를 서비스 단일 경로로 전환했다. `C:\work\Elmo\Elmo_Master_test_260724_phase4` worktree와 `codex/phase4-test-260724` 브랜치를 만들고 `a0f8ebe`로 고정했다. 원본에서는 Diagnostics 24개 및 `0x7E00`을 서비스 경로로 옮기기 시작했다.
- **검증 기록:** Phase 3B SourceOnly/full static PASS, 별도 worktree PC Debug/Release 148/148 및 WPF Debug/Release build PASS라고 기록한다. LASAL IDE·PLC·실축·패킷 시험은 하지 않았다.
- **실패·정정·철회:** 시험은 임의의 중간 소스가 아니라 일관된 checkpoint에서만 병행해야 한다고 정정했다. Phase 5 선언/Network 정리는 외부 `.st` 합성만으로 끝낼 수 없다고 판단해 LASAL UI 자동화 절차로 전환했다.
- **남은 작업:** Diagnostics service route 검증, Phase 5의 class channel/client/method 및 Network 연결 정리, LASAL 적용 확인.

### Part 003 — source lines 00501-00750

- **주요 사용자 요청:** 직전 청크의 Phase 5 LASAL IDE 구조 정리를 계속하기 위한 도구 사용 구간이다. 새 사용자 기능 요청은 없다.
- **수행·변경 기록:** Computer Use의 관찰-행동 분리, screenshot/accessibility freshness, Windows 자동화 금지사항과 확인 정책을 장문으로 읽었다.
- **검증 기록:** 프로젝트 코드·빌드 검증은 없다. 도구 운용 규칙을 확인한 것뿐이다.
- **실패·정정·철회:** 실제 LASAL 변경이 아직 시작되지 않았으므로 성공으로 볼 수 없다.
- **남은 작업:** LASAL Class 2의 실제 창을 고르고 프로젝트를 열어 Phase 5 구조 변경을 시도.

### Part 004 — source lines 00751-01000

- **주요 사용자 요청:** LASAL Class 2를 찾아 Phase 5 IDE 작업을 준비.
- **수행·변경 기록:** 실행 가능한 앱과 열린 창을 열거했다. 표준 LASAL Class 2가 설치돼 있지만 당시 `isRunning=false`인 상태를 관찰했다.
- **검증 기록:** 앱 inventory 외 프로젝트 검증은 없다.
- **실패·정정·철회:** 여러 무관한 앱/창이 함께 반환됐으며 아직 대상 LASAL 창을 열지 않았다.
- **남은 작업:** 표준 LASAL Class 2를 실행하고 단일 창을 확보.

### Part 005 — source lines 01001-01250

- **주요 사용자 요청:** 표준 LASAL Class 2 실행 및 프로젝트 열기 준비.
- **수행·변경 기록:** 앱 목록 확인을 끝내고 LASAL을 실행했다. 실행 직후 `user input was detected`가 반환돼 창을 다시 관찰했다.
- **검증 기록:** 빈 LASAL 창이 열린 사실만 확인했다. 시험 project load나 build 증거는 없다.
- **실패·정정·철회:** 사용자 입력 감지 때문에 최초 자동화 호출을 그대로 이어갈 수 없었다.
- **남은 작업:** fresh state를 확보한 뒤 Open Project dialog를 안전하게 열기.

### Part 006 — source lines 01251-01500

- **주요 사용자 요청:** 열린 LASAL 창 상태 확인.
- **수행·변경 기록:** 빈 LASAL Class 2 창의 accessibility tree를 두 차례 수집했다. 상태는 `Offline`, project 미오픈이었다.
- **검증 기록:** Open Project 및 Build 버튼의 활성 상태를 관찰했을 뿐 프로젝트 검증은 없다.
- **실패·정정·철회:** 반복 관찰만 있었고 Phase 5 변경은 수행되지 않았다.
- **남은 작업:** Open Project를 실행하고 시험 worktree의 `.lcp`를 실제로 열기.

### Part 007 — source lines 01501-01750

- **주요 사용자 요청:** 사용자가 `Elmo_Master_test_260724_phase4`가 실제 LASAL 시험에 쓸 수 있는지, 복사가 아직 진행 중인지 질문.
- **수행·변경 기록:** worktree 복사는 끝났고 Git clean, 정적/PC/WPF 검증은 통과했다고 답했다. 자동화가 띄운 원본 작업용 Open Project dialog가 사용자 조작과 겹쳤음을 인정하고 취소한 뒤 시험 project 열기를 다시 시도했다.
- **검증 기록:** `.lcp/.lcb`의 실제 LASAL load/Rebuild는 아직 검증하지 않았다고 명시했다.
- **실패·정정·철회:** “시험 폴더 준비 완료”는 정적 checkpoint 의미였고 LASAL IDE 실행 가능성까지 증명한 것은 아니었다. Open Project 클릭 결과가 `outcome unknown`으로 끝났다.
- **남은 작업:** file dialog 상태를 재관찰하고 시험 project가 열리는지 확인하거나 사용자에게 제어권 반환.

### Part 008 — source lines 01751-02000

- **주요 사용자 요청:** 시험 폴더를 쓸 수 있게 하되, 결국 사용자는 LASAL 프로그램과 시험용 WPF만 원했고 직접 수동 복사했으니 원본 개발을 계속하라고 지시.
- **수행·변경 기록:** file dialog 입력 시도 실패 후 dialog와 빈 LASAL 인스턴스를 모두 닫았다. 시험 폴더에 `.lcp/.lcb`와 관련 파일이 있다고 안내하고 직접 실행 링크를 제시했다. 이후 사용자의 수동 복사본은 건드리지 않고 원본 Phase 5 개발로 돌아가기로 했다.
- **검증 기록:** PC 148/148 및 WPF build 기록은 있으나 LASAL project load/Rebuild 증거는 없다.
- **실패·정정·철회:** GUI 자동화의 focus/outcome 불확실성이 반복됐고, LASAL 직접 제어는 사실상 중단됐다. `ProjectInternal` 미복사는 의도된 IDE cache 제외라고 설명했다.
- **남은 작업:** 사용자 수동 시험본과 분리해 원본에서 Phase 5 transport-only 정리 진행.

### Part 009 — source lines 02001-02250

- **주요 사용자 요청:** LASAL을 직접 제어하지 말고 외부 편집 후 LASAL 적용만 확인; 지금 해야 할 시험을 정리; 저장소를 커밋하고 시험 브랜치/worktree를 제거; 이후 개발→사용자 build→시험 폴더 복사→실기 순서로 고정.
- **수행·변경 기록:** `TCPMotionInterface`를 server/client/data `4/3/0`, 함수 8개로 줄이고 TCP→축/로봇 직접 연결 10개를 제거했다. test worktree가 최신 Phase 5가 아님을 확인하고 Reload/Rebuild/PLC smoke checklist를 작성했다. 이어 Phase 5 구현(`837758f`), 상태 문서(`a29cb33`), 미사용 Welcon demo 삭제(`6ba73e8`)를 분리 커밋하고 시험 브랜치/worktree metadata를 제거했다.
- **검증 기록:** 외부 준비 static, PC Debug/Release 148/148, WPF 양 구성, 최종 `Phase5TransportClean` full static 및 diff checks가 PASS했다고 기록한다. LASAL IDE/PLC 시험은 미수행이었다.
- **실패·정정·철회:** 처음에는 `Classes.lcb`/`Networks.lcb`가 stale라 기본 Phase 5가 실패한다고 했으나, 커밋 전 live 파일에서는 full 계약이 PASS해 문서를 정정했다. 기존 시험 폴더는 `20/13/0`, 직접 연결 10개, WPF 부재의 구버전이므로 Phase 5 시험에 쓰지 말라고 명확히 했다.
- **남은 작업:** 사용자 빌드/복사/실기, cold restart 후 read-only→저속 motion, 장시간 Bulk/Recorder/SDO/성능 시험. 청크 끝에서는 LASAL 변경을 잠시 멈추고 PC 응답 payload 방어를 다음 개발로 선택했다.

### Part 010 — source lines 02251-02500

- **주요 사용자 요청:** 개발→사용자 LASAL build/오류 확인→사용자 복사→시험 폴더 실기의 순서를 유지하면서 PC/WPF 개발을 계속.
- **수행·변경 기록:** 53개 명령별 응답 payload 상한, oversized 응답 전 연결 fault, 미등록 명령 송신 차단, `AxisInfo` 축 번호 검증을 추가했다. WPF에는 `0x2045` 10,000회 read-only RTT/CSV 측정기를 넣고, qualification 계산·CSV helper와 callback 예외/재진입 시험을 추가했다. Recorder exact/0/0 discovery reconnect/adopt runner와 cleanup 정책을 구현했다.
- **검증 기록:** 테스트 수가 155→157→167→169로 증가했고 최종 Debug/Release 169/169, WPF Debug/Release build가 통과했다고 기록한다. Phase 5 LASAL build 성공은 사용자 보고뿐 아니라 당시 `%TEMP%\Lasal2.log` Compiler/Linker ERROR/FATAL 0으로 확인했다고 적었다.
- **실패·정정·철회:** 실행 중 WPF가 기본 output DLL을 잠가 기본 build가 실패했지만 소스 오류가 아니며 임시 output으로 검증했다. RTT 응답 변화의 단순 byte-equality 판정을 철회하고 정지 조건과 전송 건전성을 분리했다. Adopt identity 검증 실패 시 자동 cleanup 위험과 Stop race를 수정했다. 기존 staged history의 trailing whitespace 때문에 전체 cached check/커밋은 보류됐다.
- **남은 작업:** 실제 PLC reconnect/adopt, Group/Bulk, Recorder fault/cancel, 장비 RTT/패킷 검증.

### Part 011 — source lines 02501-02750

- **주요 사용자 요청:** Group/Bulk 및 Recorder fault/cancel 자동 검증을 계속하고, LASAL 변경이 없으면 사용자 build를 요구하지 않기.
- **수행·변경 기록:** Recorder Start 직후 identity 보존·exact reconnect·상태별 cleanup 정책, Group Stop-first fallback과 3회 안정 상태 확인을 구현했다. 이어 Bulk/Recorder cleanup orchestration을 공통 helper로 분리하고, Release 실패 시 ownership/handle을 보존해 재시도 가능하게 했다. 마지막에는 한 축 offline→Partial→복구 qualification 분석을 시작했다.
- **검증 기록:** Group/Recorder 보강 후 176/176, Bulk cleanup 추가 후 179/179가 통과했다고 기록하며, 다음 묶음 시작 시점에는 191개까지 통과한 상태로 회고한다. WPF Debug/Release build PASS라고 적었다.
- **실패·정정·철회:** Recorder pre-history disconnect, Group Stop fallback 누락, UI context를 끊던 `ConfigureAwait(false)`, Close 원본 stack 손실, Release 실패 handle 유실, 검증되지 않은 adopted identity cleanup이 실제 결함으로 발견돼 수정됐다. adopted identity 불일치 상태는 자동 mutation 대신 격리/수동 Status 경로로 바뀌었다.
- **남은 작업:** Bulk partial offline/recovery UI 및 결정적 시험, Recorder cancel/Stop-race/release-failure matrix, PLC 실기.

### Part 012 — source lines 02751-03000

- **주요 사용자 요청:** 장비 없이 가능한 구현을 계속하되, 마지막에는 “1차 업데이트”로 범위를 끊고 시험 준비 상태로 만들기.
- **수행·변경 기록:** 24/24 정상→선택 축의 6개만 `SlaveOffline(18)`→24/24 복구를 판정하는 두-checkpoint Bulk runner를 구현했다. Group PowerOff/Disabled와 4축 position 3회 안정 조건을 추가했다. 이후 기존 테스트 실행 파일에 allowlist 기반 `negative-wire` dry-run/live 모드를 넣고, D5 SDO abort→정상 read recovery 및 pending-ticket/quarantine 경로를 개발했다.
- **검증 기록:** Bulk/Recorder 묶음은 202/202, negative-wire/D5 기반은 219/219와 dry-run PASS라고 기록한다. 실제 PLC에는 negative-wire를 송신하지 않았다.
- **실패·정정·철회:** Bulk Configure 전 실패 시 null reader가 원본 예외를 덮는 문제, 복합 상태 비트 오판, Recorder 격리 dead-end, 보고서 덮어쓰기/송신 후 저장 실패, AxisReference 불일치, 장기 ticket 유실, 재연결 뒤 stale ticket 조회 불가, Boot/Map 변경과 우회 경로 누락을 차례로 수정했다. 비RT에서 PDO를 직접 우회하는 식의 해결은 하지 않았다.
- **남은 작업:** D5 1차 코드 동결, 전체 build/test/runbook, 수동 SDO·Drive Read까지 quarantine 범위 확장 여부와 실제 PLC 시험.

### Part 013 — source lines 03001-03250

- **주요 사용자 요청:** 1차 코드를 정리·커밋하고 시험 폴더에 복사한 뒤 다음 개발을 계속.
- **수행·변경 기록:** D5 tracker를 qualification뿐 아니라 수동 SDO와 Drive Read까지 확대하고, `TicketNotFound`/stale owner semantics와 PI Write 차단을 정리했다. Phase 1 runbook을 만들고 API/WPF/문서를 6개 커밋(`be3a929`, `3d1d7f7`, `473bd66`, `3e57841`, `5738582`, `b774d30`)으로 나눴다. LASAL 시험 폴더 618개 파일을 SHA-256 대조했다고 기록한다. 이어 Drive Read 예외를 capability preflight/submission/status polling 단계와 ticket으로 구분했다.
- **검증 기록:** 최종 API Debug/Release 225/225, WPF Debug/Release Rebuild PASS, negative-wire dry-run PASS라고 기록한다. 시험 폴더는 414,254,483 bytes, hash mismatch 0이라고 보고했다.
- **실패·정정·철회:** 수동 SDO/Drive 로그가 이전 qualification run ID에 붙는 문제, `HandleOrGenerationStale(10)` 반복 실패, `msbuild.exe` PATH 부재, capability 오류 fixture의 잘못된 68-byte 길이를 수정했다. malformed/transport 응답은 전용 단계 예외로 오분류하지 않고 unknown/quarantine에 남겼다.
- **남은 작업:** non-domain transport/malformed/session 실패의 전체 attempt context, WPF tracker 상태기계 단위시험, PLC/LASAL/실축/pcap 검증.

### Part 014 — source lines 03251-03500

- **주요 사용자 요청:** 실패 단계/ticket 보존 계약을 계속 구현·커밋하고, 실행 중인 LASAL 시험 폴더는 건드리지 않기.
- **수행·변경 기록:** 일반 예외 타입을 유지하면서 Drive Read attempt context와 WPF `D5ExternalReadFailureOrchestrator`를 추가했다. 이어 raw `SubmitSdo[Async]`를 송신 전 실패/PLC 거절/결과 불명/ticket 발급 후 세션 경합으로 구분하고, UI 상태 초기화와 binary-compatible outcome property를 보강했다. 코드·문서 커밋 `ebbc39a`/`ac0e065` 후 D5 quarantine ledger와 계약 시험을 구현했다.
- **검증 기록:** 중간 236/236, raw SDO 완료 시 244/244, ledger 이관 후 Debug 249/249 및 WPF build PASS라고 기록한다. negative-wire는 dry-run만 수행했다.
- **실패·정정·철회:** session-race ticket 채택 시 이전 UI 결과가 남는 문제를 수정했다. 기존 공개 `SubmissionOutcome`을 공용 enum으로 교체하려던 방향은 binary compatibility 위험 때문에 철회하고 기존 property 유지+새 generic property 추가로 바꿨다. 단순 ledger version 비교는 proof의 정상 임시 guard까지 막으므로 deep invariant snapshot 방식으로 정정했다.
- **남은 작업:** ledger Release 검증·문서/커밋, 제출 `MapRevision` 고정과 mixed-session evidence 분류, 동시성/복구 scope 시험.

### Part 015 — source lines 03501-03750

- **주요 사용자 요청:** D5 recovery 정리를 계속하던 중 “SDO Write가 가능하도록 LASAL 코드, API, 시험 GUI를 수정” 요청.
- **수행·변경 기록:** ticket `MapRevision`, mixed evidence, cleanup identity를 보강하고 `6a8bf35`/`df4eb68`로 정리했다. Recovery scope 정책과 concurrency 시험을 각각 `874baca`, `4b1e8d5` checkpoint로 만들고 pending cleanup orchestrator를 269개 시험까지 확장했다. 이후 D5 `0x7E50` Write, API 정책, WPF Read/Write 선택·입력·격리 흐름을 구현했으며 승인 후보를 Gold `UI[24]=0x2F00:24`, Int32/4-byte로 좁혔다.
- **검증 기록:** 단계별 249→256→260→269→274 테스트 PASS와 WPF build 기록이 있다. SDO Write 격리 복구 기반은 274/274라고 적었다.
- **실패·정정·철회:** concurrency 시험 자체가 delay/scheduling에 의존하고 실패 시 task 회수가 안 되는 결함을 수정했다. 저장소만으로 `UI[24]` 미사용을 증명할 수 없어 실제 allowlist/capability 활성화를 보류했다. `0x6040/0x607A/0x60FF/0x6071` 같은 motion 객체는 영구 차단 대상으로 유지했다.
- **남은 작업:** 4축 모두 `UI[24]` 미사용 여부와 첫 시험 축 확인, Write exact fingerprint/readback, LASAL 선언 메타데이터 동기화, 사용자 Build/Link와 실기.

### Part 016 — source lines 03751-04000

- **주요 사용자 요청:** SDO Write 구현을 안전하게 마무리한 뒤, 새 CREVIS I/O와 EtherCAT 노드 순서 변경을 반영하고 Elmo 방식을 참고한 topology/I/O API를 구현 목록에 추가.
- **수행·변경 기록:** Write timeout/cancel/orphan, exact request fingerprint, owner/session/BootId/MapRevision guarded readback을 보강하고 코드/문서를 `3ae7b88`/`efed7fb`로 커밋했다. 이후 CREVIS가 node 1, Elmo가 뒤로 밀린 구성을 분석해 legacy `0x7E10`은 유지하고 새 topology/node-state/digital I/O 계약 `0x7E11/12/13/22/23`과 CAS output 모델을 C# SDK/문서에 추가했다. capability bit 14-17과 output allowlist는 닫아 둔 설계였다.
- **검증 기록:** SDO Write PC Debug/Release 277/277, WPF 격리 build, LASAL SourceOnly PASS라고 기록한다. full LASAL 계약은 stale `Classes.lcb`를 이유로 의도적으로 FAIL했다. topology/I/O SDK 전체 회귀는 다음 청크에서 286개로 확정된다.
- **실패·정정·철회:** 기존 full verifier가 새 Write 선언이 없는 `Classes.lcb`도 잘못 PASS시키는 허점을 찾아 수정했다. Write terminal/error를 성공/실패로 추정해 격리 해제하던 문제를 금지했다. “노드가 offline이면 PDO가 재번호화된다”는 가정은 폐기하고, 구성 mapping은 고정·상태/quality만 동적이라는 모델로 정정했다. durable Write journal은 여전히 없다고 명시했다.
- **남은 작업:** SDO Write의 승인 축/대상 및 durable journal, CREVIS용 PLC/LASAL handler·RT I/O owner·WPF, topology parser의 중복 index와 64-bit 폭 검증.

### Part 017 — source lines 04001-04250

- **주요 사용자 요청:** topology/I/O SDK를 실제 PLC/LASAL read-only topology와 GUI에 연결; CREVIS 정보가 GUI에 안 보이고 SDO Write 중 다른 입력을 편집할 수 없는 문제를 함께 반영; 이후 node health와 digital I/O까지 계속 구현.
- **수행·변경 기록:** topology parser의 slave/slot 중복과 8-byte 초과를 막고 SDK 계약을 `353ede1`, 문서를 `6ce2cb2`로 커밋했다. 이어 LASAL `LMCDiagnosticsService`와 TCP router에 `0x7E11/0x7E12`를 붙여 7개 static topology entry를 1개씩 7회 chunk 응답하도록 만들고, WPF CREVIS 목록과 SDO 입력 잠금 범위를 수정했다. 마지막 구간에서는 WPF의 `0x7E13/0x7E22/0x7E23` 화면/flow를 계속 작성했다.
- **검증 기록:** SDK checkpoint는 Debug/Release 286/286, static topology/WPF checkpoint는 288/288, WPF Release build와 LASAL static contract PASS라고 기록한다. 7-entry CRC는 `0x15867EEC`; 당시 안내한 capability는 `0x0000613F`다. 마지막 행에서는 PC 측 시험이 290개로 늘었다고 했지만 그 묶음의 최종 완료·커밋은 이 범위에 없다.
- **실패·정정·철회:** 실행 중 Debug GUI는 이전 DLL이고 PLC가 bit 14를 광고하지 않아 CREVIS가 안 보였다는 원인을 명시했다. `0x7E11/12` 7개 entry를 한 함수에서 한 번에 작성하면 LASAL 함수 크기 한계를 넘어서 chunk 방식으로 바꿨다. SDO는 ticket이 살아 있어도 다음 입력 편집을 허용하되, 중복 Submit과 Write exact-readback 중에는 계속 잠그도록 범위를 정정했다. node health/실제 I/O는 typed client와 RT owner 없이는 구현할 수 있어 비RT PDO 직접 접근을 거부했다.
- **남은 작업:** 사용자 Build/Link/Download 후 새 Release GUI에서 7행 static topology 확인, `LMCEtherCATIoService` class/object/client와 RT owner 생성, `0x7E13/22/23`의 실제 상태·input·output shadow/CAS write 구현 및 capability bit 15-17 활성화, 290개 묶음의 후속 완료 확인.

## 전체 구간 시간순 단계

1. **260724 인계와 Phase 3 전환:** 대형 히스토리 분할 결과를 재개점으로 삼고 dormant service를 Network에 등록한 뒤 Group/Admin 13개를 실제 route로 전환했다.
2. **Phase 4/5 서비스 소유권 정리:** Control 26개와 Diagnostics를 service 단일 경로로 옮기고 `TCPMotionInterface`를 transport-only로 축소했다. 시험용 worktree 시도는 LASAL IDE 자동화 비효율과 구버전 문제 때문에 폐기되고, 사용자 수동 복사 workflow로 정착했다.
3. **개발/시험 분리 규칙 고정:** 메인 저장소는 에이전트가 개발·정적 검증하고, LASAL 변경 시 사용자가 메인을 Build한 뒤 시험 폴더로 복사해 실기하는 순서가 합의됐다.
4. **PC transport와 qualification 강화:** response payload 상한, AxisInfo 검증, 10,000회 RTT/CSV, callback 안전성, Recorder reconnect/adopt, Group Stop fallback, Bulk/Recorder cleanup·partial recovery가 추가됐다.
5. **D5 실패·복구 모델 정교화:** negative-wire 도구, abort recovery, pending ticket quarantine, attempt context, raw submit outcome, ledger/recovery scope/concurrency 정책을 구현하고 테스트 수가 148에서 269 이상으로 증가했다는 기록이 이어진다.
6. **gated SDO Write:** LASAL/API/WPF Write 경로를 만들었지만 실제 allowlist는 비워 두고, exact readback·identity 보호·full metadata gate를 추가했다. `UI[24]`와 시험 축 미확정, durable journal 부재로 실제 활성화는 보류됐다.
7. **CREVIS topology/I/O 방향 전환:** 고정 PDO mapping과 동적 상태/quality를 분리하고 C# topology/I/O 계약을 먼저 커밋했다. 이어 static 7-entry topology를 LASAL/WPF에 연결했으며, 구간 끝에서는 dynamic health/input/output GUI와 계약 개발이 진행 중이었다.

## 후속 구간으로 넘길 상태

### 마지막으로 기록된 active focus

- 이 구간 끝의 active 작업은 **CREVIS/EtherCAT dynamic node health와 digital I/O**다.
- 히스토리상 SDK wire/model/parser는 존재하지만, 실제 PLC-side `LMCEtherCATIoService`, typed PDO client, 단일 RT output owner는 아직 없었다.
- `0x7E11/0x7E12` static topology와 GUI는 288개 정적/PC 시험까지 기록됐고, `0x7E13/0x7E22/0x7E23` GUI·계약은 290개 시험이 진행 중인 상태에서 이 청크 범위가 끝난다.
- dynamic capability bit 15-17과 output allowlist는 닫혀 있어야 했고, 실제 input/output/health나 hardware write 성공 증거는 없다.

### 다음에 우선 확인할 것

1. 후속 parts에서 290개 묶음이 최종 통과·커밋됐는지, 아니면 수정/철회됐는지 확인한다.
2. 현재 live source에서 `LMCDiagnosticsService`, `TCPMotionInterface`, `MainWindow.TopologyIo.cs`, topology/I/O SDK 및 capability bit를 다시 읽는다.
3. `LMCEtherCATIoService`와 typed clients/RT owner가 후속 히스토리 또는 현재 LASAL project에 실제 생성·연결됐는지 확인한다.
4. static topology 시험은 사용자 Build/Link/Download, 새 Release GUI, 실제 7행/CRC/capability 캡처가 있는지 별도로 찾는다.
5. SDO Write는 `UI[24]` 미사용 확인, 한 축 allowlist, `Classes.lcb` 동기화, durable journal 여부를 확인하기 전에는 활성화됐다고 보지 않는다.

### 지속해야 할 검증 경계

- `ACK`, C# 테스트, WPF build, LASAL SourceOnly/full static, IDE Build/Link, PLC Download, 실제 packet/hardware 동작을 서로 대체 증거로 취급하지 않는다.
- 히스토리의 테스트 개수와 커밋 해시는 당시 checkpoint 식별자일 뿐 현재 HEAD 증거가 아니다.
- 기존 staged history trailing whitespace, 사용자 `Classes.lcb`/CREVIS/LASAL 변경, 미추적 시험 자료는 여러 단계에서 의도적으로 커밋 제외됐다는 기록이 있으므로 live `git status`에서 소유권을 다시 분리한다.
- 사용자가 고정한 작업 순서인 **메인 개발 → 사용자 메인 LASAL Build/오류 확인 → 사용자 시험 폴더 복사 → 시험 폴더 실기**를 유지한다.

# Chunk Digest: Parts 001-065

## 주의

이 문서는 `Elmo_Master_history_260724_part_001_*`부터 `part_065_*`까지,
원본 source line 1-16,251을 **각 part 전체를 읽어** 만든 history-only 인덱스다.
기록 당시의 코드, IDE 상태, 테스트 결과, Git 상태와 완료 판단을 현재 저장소의
사실로 간주하지 않는다. 작업을 재개하기 전에는 현재 소스와 `git status`, LASAL
선언/Network, 관련 자동 검증, PLC 다운로드 상태와 새 패킷 캡처를 다시 확인해야
한다. computer-use 지침, accessibility tree와 실행 앱 목록처럼 기술 판단이 없는
구간도 Part index에 포함했다.

## Chronological phases

| Phase | Part / source line | 기록상 흐름 |
|---|---|---|
| P0 diagnostics 보정 | 001-016 / 1-4,000 | EtherCAT Health의 read-only binding, Recorder 자연 완료 후 Stop race, 독립 RT latch 중복 스케줄을 점검했다. WPF·LASAL 수정과 정적/빌드 검증이 기록됐지만 PLC 다운로드와 실기 재검증은 하지 않았다. |
| PMAS native capture 비교와 baseline 정리 | 017-018 / 4,001-4,500 | PMAS/MMCLib native 캡처 23개를 LASAL custom `0x7Exx`와 분리해 분석했다. Recorder 입력 검증과 terminal Stop 차이를 반영하고 6개 커밋을 기록했다. D5는 parser shape만 fail-closed로 열고 capability는 계속 닫았다. |
| D5 derived executor 설계·IDE 생성 | 018-041 / 4,251-10,250 | vendor `EtherCAT_SDOBase`를 그대로 executor로 쓰지 않고 `LMCSdoExecutor`를 파생해 ticket/session/timeout을 `LMCDiagnosticsService`가 소유하는 구조를 선택했다. IDE에서 class와 4개 object/network 연결을 만들고, IDE 저장이 외부 `.st`를 되돌린 문제를 복구했다. |
| D5 first-slice live qualification | 042-045 / 10,251-11,250 | 처음에는 고정 `0x1000:0 UInt32/4`만 capability `0x13F`로 열었다. LASAL 대소문자 비구분 self-assignment로 즉시 timeout이 발생한 뒤 이름을 고쳐 4축 성공 캡처를 얻었다. “일반 SDO 완료”라는 표현은 잘못으로 정정됐다. |
| 일반 SDO read 확대와 executor race 보정 | 045-046 / 11,001-11,500 | 1/2/4-byte 정수·bitfield·Real32 읽기로 확장하고 capability를 `0x213F`로 바꿨다. callback timing race와 quarantine 고착으로 `ResourceBusy`가 생기는 상태 머신을 수정했다. Write는 계속 비활성이다. |
| 65-row API 요구사항과 Phase 0/1/2 | 046-048 / 11,251-12,001 | Excel 요구사항을 분류하고 Group option/좌표계 fail-fast, read-only Admin/Axis/Group API, `0x7D22` group-relative motion을 순서대로 구현했다고 기록했다. PC 계약 테스트는 104→105→135→148로 증가했다. |
| 실기 capture 판정과 UI 보강 | 049-051 / 12,002-12,751 | Phase 1/2 캡처에서 Admin, group motion, PowerOff, SDO, PI/Bulk를 판정했다. `0x2047 GroupEnable`의 same-cycle LockState 판정 결함을 찾았고, dynamic timeout·ACS read-only·PowerOff verification을 추가했다. `09b`로 None/ACS wire 값을 재검증했다. |
| Runtime Qualification runner 구현 | 051-061 / 12,502-15,251 | Group/Bulk/Recorder 자동 qualification UI와 runner를 만들고, safety-generation/send gate, cleanup 직렬화, double-release 검증, chunk 사이 취소를 강화했다. 실제 WPF visual smoke와 정적/빌드 검증은 기록됐으나 새 runner의 실물 PLC 시험은 아직이었다. |
| 커밋 정리와 LASAL Find 진단 정정 | 062-065 / 15,252-16,251 | PLC `0x2047`, WPF runner, 문서를 3개 커밋으로 분리했다. 이어 `Find in Implementation` 결과가 보이지 않는 사용자 지적을 처음에는 로그 성공으로 잘못 반박했다가 이를 인정하고 실제 Find Results UI 확인을 시작했다. Part 065는 화면 읽기 도구 호출 도중 끝난다. |

## 주요 결정과 완료 기록

- EtherCAT Health 예외는 `Online` read-only 속성에 기본 TwoWay binding이 적용된
  문제로 보고 `Mode=OneWay`를 지정했다. Recorder는 sample count로 이미 terminal인
  경우 Stop을 다시 보내지 않도록 PLC 상태를 재조회하는 방향으로 보정했다
  (`part_001`-`002`, source 1-500).
- `LMCEcatInputLatch1`의 별도 RealTime 실행은 메인 diagnostics 경로와 중복될 수 있어
  RealTime을 끄는 것으로 기록됐다. IDE Rebuild/Link와 implementation smoke는 기록됐지만
  PLC download와 실제 motion 검증은 하지 않았다 (`part_004`-`016`, source 751-4,000).
- PMAS capture는 Maestro/MMCLib native PI/Recorder 흐름이고 LASAL `0x7E00` 계열의
  wire 증거가 아니라고 구분했다. Recorder native 실패의 입력 단서는 `uiSr=0`,
  `Rl=0`이었고 PMAS V2는 ready/range/cache/selected-PI 검증을 fail-closed로 강화했다
  (`part_017`, source 4,001-4,250).
- 당시 baseline 정리 커밋은 `d138f4b`, `b02ae28`, `b8e80d7`, `a320d59`,
  `6d2c717`, `d5061bf`로 기록됐다. D5 parser는 4-byte `0x1000:0` frame shape를
  검증했지만 capability는 `0x3F`, `MaxSdo=0`으로 계속 닫혀 있었다
  (`part_018`, source 4,307-4,344).
- D5 구조는 `LMCSdoExecutor : EtherCAT_SDOBase` 4개와
  `LMCDiagnosticsService` owner를 선택했다. service가 ticket/session/timeout을 소유하고,
  executor는 vendor callback을 얇게 수용하며 dedicated 4-byte buffer를 쓰고,
  cancel은 queued-only로 시작하는 설계였다. legacy manual base object와의 동시 사용은
  금지했다 (`part_018`-`019`, source 4,414-4,750).
- LASAL network의 핵심 연결은 derived class 내부 `_base.toSlave -> this.toSlave`,
  각 executor의 inherited `ClassState -> Elmo_11..41`, service `SdoAxis1..4 ->`
  executor `ClassState`로 기록됐다. legacy `SDOBase1..4`는 제거했다
  (`part_035`-`041`, source 8,501-10,250).
- 이 D5 derived slice 정리 커밋은 `975cab0`과 `2f20844`로 기록됐다. Rebuild/Link
  0 error, PC 103/103, static PASS, WPF Release PASS도 당시 기록이나, 이 시점에는
  capability를 아직 열지 않았다 (`part_041`, source 10,212-10,250).
- 첫 live slice는 capability `0x13F`, `MaxSdo=4`로 고정
  `0x1000:0 UInt32/4`만 허용했다. 축 1-4에서 각각 ticket 6/7/8/5,
  43/51/43/54 cycles, 값 `0x00020192` 성공이 기록됐다. 이는 네 축의 해당 object에
  대한 증거일 뿐 임의 index/type 또는 SDO write의 증거는 아니었다
  (`part_042`-`045`, source 10,251-11,045).
- 이후 general read 범위를 nonzero index/subindex, 1/2/4 bytes와 Bool/Int/UInt/
  BitField/Real32로 확대하고 capability를 `0x213F`로 기록했다. Write와 더 큰 payload는
  계속 범위 밖이다 (`part_045`, source 11,141-11,220).
- 65-row 요구사항 분류는 기록상 direct 14, LASAL-equivalent 21, partial 11,
  missing 15, 1:1 inappropriate 4였고, 우선 공백은 Homing, SetOpMode,
  SetPosition 계열이었다 (`part_046`, source 11,388-11,500).
- Group `StopMove()` 반환값을 error가 아닌 StopCmdNo로 정정하고 완료/error는
  `0x2045`에서 읽도록 했다. 좌표계는 motion에 None/ACS만 허용하고 MCS/PCS를
  fail-fast하며, read-only Admin/Axis/Group 명령과 `0x7D22` relative motion을
  순차 추가했다고 기록했다 (`part_047`-`048`, source 11,501-12,001).
- live capture 후 `0x2047`은 실제 lock 완료 전에 same-cycle `LockState`를 보고
  `-6`을 반환하는 결함으로 판정했다. 명령 수락 ACK 후 `0x2045` polling으로 바꾸고,
  long motion에는 속도/거리 기반 dynamic timeout, PowerOff에는 최종 status 검증을
  추가했다 (`part_049`-`051`, source 12,002-12,751).
- Runtime Qualification은 Group Enable, true buffered A→B, Stop-first, Bulk 24-entry
  snapshot/lifecycle soak, Recorder Single/Ring/trigger soak를 포함했다. 공용 send gate와
  Stop/PowerOff generation을 요청 직전에 확인하고, Recorder download도 qualification
  전용 chunk loop로 나눠 안전 명령이 chunk 사이에 들어갈 수 있게 했다고 기록했다
  (`part_051`-`061`, source 12,502-15,189).
- 마지막 정리 커밋은 `8572adb` (`0x2047` ACK), `b7d0a7a` (qualification
  workflows), `b6c3511` (문서)로 기록됐다. 당시 `main`은 origin보다 3 commits ahead,
  push하지 않은 상태였다 (`part_062`, source 15,325-15,370).

## 수정·재검토에서 잡힌 문제

- Recorder Stop의 `DetailCode=19 InvalidState`는 실패한 recording이 아니라 이미
  `SampleCountComplete`가 된 뒤 Stop을 다시 보낸 terminal race였다. Download는 PC
  메모리 복사, Export CSV는 파일 영속화라는 UI 의미도 구분했다
  (`part_002`, source 251-500).
- LASAL IDE 저장이 외부에서 복구한 D5 parser/Recorder implementation을 이전 상태로
  덮어쓴 사례가 있었다. 이후 선언/network 변경은 IDE, implementation은 IDE 종료 후
  외부 `.st`로 나누고 post-save source를 다시 확인했다
  (`part_018`-`019`, source 4,251-4,750).
- derived executor에 임시 `ClassSvr`를 추가했으나 불필요하다고 정정하고 inherited
  `ClassState`를 service connection에 사용했다. service source에는 qualified nested
  type, `IsClientConnected(...) <> 0`, 대소문자 비구분 shadow 회피가 필요했다
  (`part_035`-`041`, source 8,501-10,250).
- 첫 SDO timeout의 직접 원인은 `SdoTimeoutCycles := sdoTimeoutCycles`처럼 LASAL이
  대소문자를 구분하지 않는 환경에서 member와 local이 자기 자신을 대입한 것이다.
  slave/index/subindex에도 같은 shadow가 있어 `requestSdo*` 이름으로 바꿨다
  (`part_042`-`044`, source 10,443-10,900).
- 네 축 `0x1000:0` 성공 뒤 “D5 SDO read complete”라고 한 판단은 과장으로 정정됐다.
  그 시점의 SDK/UI/PLC는 고정 first slice만 허용했고, 임의 object/type은 이후 별도
  구현·검증 대상이었다 (`part_045`, source 11,001-11,166).
- general read 시험에서 `0x6061` 요청이 `ResourceBusy`가 된 원인은 callback validation
  실패가 executor를 `QUARANTINED`에 남기고, vendor start 뒤에 RUNNING을 publish해
  fast callback이 race하는 상태 머신이었다. RUNNING 선공개, RELEASING cleanup,
  normal-token failure 소비 후 IDLE 복귀로 고쳤다고 기록했다
  (`part_046`, source 11,251-11,400).
- `09` capture는 처음 `0x2051`이 아니라 `0x2045`를 담아 무효로 판정됐다.
  `09b`에서는 `0x2051` 두 건과 coord 0/1, 같은 16-DINT 배열을 확인했으며
  FunctionStatus를 `0x4000`으로 문서 정정할 필요도 발견했다
  (`part_050`-`051`, source 12,252-12,700).
- Bulk/Recorder qualification review에서 primary 작업과 cleanup이 모두 실패할 때
  `finally`가 원인을 덮는 문제, PLC 거부를 local double-release 차단 성공으로 오판하는
  문제, Recorder Download 뒤 `Uploading` 상태의 Release를 거부하는 문제를 고쳤다.
  `Fault`는 자동 Release 성공으로 간주하지 않았다
  (`part_060`-`061`, source 14,752-15,189).
- LASAL `Find in Implementation`에 결과가 없다는 사용자 관찰을 처음에는
  `Last command succeeded`와 로그만 보고 성공으로 잘못 단정했다. 이후 그 로그는
  명령 실행만 뜻하며 Find Results 생성 증거가 아님을 인정했다. Part 065까지는 실제
  Find Results UI를 읽기 시작한 단계라 최종 원인·우회는 이 범위에서 확정되지 않았다
  (`part_062`-`065`, source 15,373-16,251).

## 기록상 검증과 실기 증거

| 시점 | 기록상 결과 | 경계 |
|---|---|---|
| P0 diagnostics | PC 101/101, LASAL static PASS, WPF build와 별도-output startup PASS, Rebuild/Link 0 error 및 implementation smoke | PLC download와 live Health/Recorder 재시험은 미완료 (`part_015`-`017`, source 3,501-4,250). |
| PMAS/native 및 D5 parser | PMAS WPF build, PC 102/102, LASAL static PASS | PMAS controller recapture와 LASAL D5 execution은 미검증 (`part_017`-`018`, source 4,001-4,500). |
| Derived first slice | PC 103/103, static PASS, WPF build/startup; LASAL Rebuild/Link 0 error | cap enable 전의 정적/IDE 결과였음 (`part_041`-`042`, source 10,212-10,380). |
| SDO first-slice live | axis 1-4 `0x1000:0 UInt32/4` completed, 같은 value `0x00020192`; Slave 4는 54 cycles | arbitrary SDO read/write 증거 아님 (`part_044`-`045`, source 10,900-11,166). |
| General read 보정 | PC 104/104, SourceOnly/full static, WPF Debug/Release와 startup PASS | state-machine 수정 후 전체 live fault matrix는 미완료 (`part_045`-`046`, source 11,141-11,410). |
| Phase 0/1/2 | PC 105→135→148/148, LASAL static, WPF build/startup PASS | 새 PLC Rebuild/download와 일부 runtime capture가 뒤따라야 했음 (`part_047`-`048`, source 11,501-12,001). |
| Phase live captures | Admin 01-03, group motion 04b/05b, PowerOff 08b/08c, SDO 10/12, PI/Bulk 11, None/ACS 09b가 기록상 PASS | buffered chaining, stop-first, long soak, fault/race는 미완료 (`part_049`-`051`, source 12,002-12,751). |
| Qualification runner | WPF Debug/Release, 3-second startup smoke, visual smoke, PC 148/148 Debug/Release, LASAL SourceOnly/full static, diff checks PASS; 독립 리뷰에 남은 BLOCKER/HIGH 없음 | runner 자체의 실물 PLC capture는 하지 않음 (`part_061`, source 15,168-15,191). |
| 커밋 후 최종 기록 | WPF Debug/Release/startup, PC 148/148 양쪽, LASAL SourceOnly/full, `git diff --check` PASS | push하지 않았고 `Classes.lcb`와 원시 TXT는 의도적으로 제외 (`part_062`, source 15,325-15,370). |

## 기록상 미완료·재개점

1. `8572adb`의 `0x2047` acceptance ACK를 최신 LASAL 프로젝트에 동기화해
   Rebuild/Link, PLC Download 후 `13_GroupEnable_AcceptedThenLocked` capture로 확인한다.
2. 새 runner로 `14` true buffered, `15` stop-first, `16/17` Bulk soak,
   `19/20/22` Recorder lifecycle/soak를 각각 `pcapng + QTEST txt`로 수집한다.
3. 외부 조작 UI가 필요한 `18` one-slave-offline Bulk checkpoint와 `21` Recorder
   reconnect/adopt exact/zero-ID workflow를 구현·검증한다.
4. SDO는 abort/offline/timeout, queued cancel, disconnect orphan, contention/recovery
   `23a-23f`를 위한 재현 가능한 timing hook 또는 전용 runner가 필요하다.
5. LASAL Data Analyzer로 1 ms task jitter, overrun, free RAM과 Recorder bank 불변성을
   측정한다. `RecorderDoubleBank`는 PLC capability가 광고될 때까지 SKIP이다.
6. 위 gate가 닫힌 뒤 Homing, SetPosition, PI/SDO Write와 production 승격을 진행한다.
7. 이 범위의 마지막 이슈인 `Find in Implementation`은 사용자 화면에서 실제 결과가
   없다는 상태부터 다시 확인해야 한다. 과거 log의 전체 `CInvalidArgException` count를
   쓰지 말고 smoke 시작 offset/현재 PID 이후만 분리한다. Part 065는 Find Results
   화면 읽기 직전이므로 뒤 part의 최종 판정을 이어 읽어야 한다.

위 재개점은 당시 기록이다. 현재 checkout이나 PLC에 이미 반영됐는지는 이 digest에서
재검증하지 않았다.

## Part index

| Part | Source line | Topical hint |
|---:|---:|---|
| [001](Elmo_Master_history_260724_part_001_lines_00001_00250.md) | 1-250 | 이전 260721 인계, Health binding·Recorder·SDO/Write 상태 질의와 P0 보정 시작. |
| [002](Elmo_Master_history_260724_part_002_lines_00251_00500.md) | 251-500 | Recorder natural completion/Stop race 판정, Download와 Export 의미 구분, 후속 계획. |
| [003](Elmo_Master_history_260724_part_003_lines_00501_00750.md) | 501-750 | Windows 실행 앱 inventory 중심; 새 기술 결정 없음. |
| [004](Elmo_Master_history_260724_part_004_lines_00751_01000.md) | 751-1,000 | LASAL Motion Network와 task/channel tree 탐색, latch 중복 schedule 확인 준비. |
| [005](Elmo_Master_history_260724_part_005_lines_01001_01250.md) | 1,001-1,250 | Computer Use 지침과 LASAL UI state 중심. |
| [006](Elmo_Master_history_260724_part_006_lines_01251_01500.md) | 1,251-1,500 | LASAL navigation continuation, Recorder race/latch priority 재확인. |
| [007](Elmo_Master_history_260724_part_007_lines_01501_01750.md) | 1,501-1,750 | `LMCEcatInputLatch1` RealTime off, Recorder terminal-state 재조회 보정과 UI 설명 추가. |
| [008](Elmo_Master_history_260724_part_008_lines_01751_02000.md) | 1,751-2,000 | LASAL save/Rebuild/Link 진행과 class implementation smoke 준비. |
| [009](Elmo_Master_history_260724_part_009_lines_02001_02250.md) | 2,001-2,250 | implementation tab/navigation 및 rebuild 상태 확인. |
| [010](Elmo_Master_history_260724_part_010_lines_02251_02500.md) | 2,251-2,500 | P0 IDE smoke continuation; PLC download 없음. |
| [011](Elmo_Master_history_260724_part_011_lines_02501_02750.md) | 2,501-2,750 | LASAL implementation/source accessibility dump 중심. |
| [012](Elmo_Master_history_260724_part_012_lines_02751_03000.md) | 2,751-3,000 | `LMCEcatInputLatch::RtWork` 탐색·UI state continuation. |
| [013](Elmo_Master_history_260724_part_013_lines_03001_03250.md) | 3,001-3,250 | class tabs와 implementation smoke용 LASAL 화면 기록. |
| [014](Elmo_Master_history_260724_part_014_lines_03251_03500.md) | 3,251-3,500 | `LMCEcatInputLatch`, Recorder, Diagnostics class smoke continuation. |
| [015](Elmo_Master_history_260724_part_015_lines_03501_03750.md) | 3,501-3,750 | Rebuild/Link 0 error, 3/3 implementation smoke, 신규 IDE exception 없음 기록. |
| [016](Elmo_Master_history_260724_part_016_lines_03751_04000.md) | 3,751-4,000 | PC 101/101·static/WPF 검증과 D5/D4/D6 잔여 범위 정리; live smoke 중단. |
| [017](Elmo_Master_history_260724_part_017_lines_04001_04250.md) | 4,001-4,250 | PMAS native pcap 23개 분석, Recorder 입력/Stop 계약 보정, 101/101 기록. |
| [018](Elmo_Master_history_260724_part_018_lines_04251_04500.md) | 4,251-4,500 | 6개 baseline commit, D5 parser fail-closed, PMAS/LMC 경계, SDOBase executor 검토. |
| [019](Elmo_Master_history_260724_part_019_lines_04501_04750.md) | 4,501-4,750 | derived `LMCSdoExecutor` 설계 확정, 4-object topology와 first-slice 정책. |
| [020](Elmo_Master_history_260724_part_020_lines_04751_05000.md) | 4,751-5,000 | LASAL 실행을 위한 Computer Use 지침과 app 탐색. |
| [021](Elmo_Master_history_260724_part_021_lines_05001_05250.md) | 5,001-5,250 | LASAL launch/project open 진행. |
| [022](Elmo_Master_history_260724_part_022_lines_05251_05500.md) | 5,251-5,500 | LASAL project load와 class view 진입. |
| [023](Elmo_Master_history_260724_part_023_lines_05501_05750.md) | 5,501-5,750 | project/class tree load 확인과 UI navigation. |
| [024](Elmo_Master_history_260724_part_024_lines_05751_06000.md) | 5,751-6,000 | `EtherCAT_SDOBase` class 탐색 시작. |
| [025](Elmo_Master_history_260724_part_025_lines_06001_06250.md) | 6,001-6,250 | vendor SDOBase의 generated/read-only 구조 확인. |
| [026](Elmo_Master_history_260724_part_026_lines_06251_06500.md) | 6,251-6,500 | derived class 생성 메뉴와 base class 선택 경로 탐색. |
| [027](Elmo_Master_history_260724_part_027_lines_06501_06750.md) | 6,501-6,750 | derived class 이름·base selection UI continuation. |
| [028](Elmo_Master_history_260724_part_028_lines_06751_07000.md) | 6,751-7,000 | `LMCSdoExecutor` 생성 대화상자와 LASAL state. |
| [029](Elmo_Master_history_260724_part_029_lines_07001_07250.md) | 7,001-7,250 | derived class 생성 확정 직전/직후 UI continuation. |
| [030](Elmo_Master_history_260724_part_030_lines_07251_07500.md) | 7,251-7,500 | `LMCSdoExecutor : EtherCAT_SDOBase` 생성, PC first-slice 103/103 기록. |
| [031](Elmo_Master_history_260724_part_031_lines_07501_07750.md) | 7,501-7,750 | class tree reload와 derived class registration 확인 시도. |
| [032](Elmo_Master_history_260724_part_032_lines_07751_08000.md) | 7,751-8,000 | LASAL reload/UI API 탐색; 새 기술 결론 없음. |
| [033](Elmo_Master_history_260724_part_033_lines_08001_08250.md) | 8,001-8,250 | derived class property 및 BaseClass 확인 continuation. |
| [034](Elmo_Master_history_260724_part_034_lines_08251_08500.md) | 8,251-8,500 | class declaration/network 준비와 UI state. |
| [035](Elmo_Master_history_260724_part_035_lines_08501_08750.md) | 8,501-8,750 | IDE reload 후 service/executor command type 불일치 발견, 임시 server 추가. |
| [036](Elmo_Master_history_260724_part_036_lines_08751_09000.md) | 8,751-9,000 | inherited `ClassState` 사용으로 방향 정정, derived internal network 요구 확인. |
| [037](Elmo_Master_history_260724_part_037_lines_09001_09250.md) | 9,001-9,250 | `_base.toSlave -> this.toSlave` internal connection 편집·저장 진행. |
| [038](Elmo_Master_history_260724_part_038_lines_09251_09500.md) | 9,251-9,500 | EtherCAT Network에서 executor object 배치/교체 시작. |
| [039](Elmo_Master_history_260724_part_039_lines_09501_09750.md) | 9,501-9,750 | 4개 executor object와 Comm link 생성 시도. |
| [040](Elmo_Master_history_260724_part_040_lines_09751_10000.md) | 9,751-10,000 | object placement/network continuation, 사용자 수동 배치로 전환. |
| [041](Elmo_Master_history_260724_part_041_lines_10001_10250.md) | 10,001-10,250 | 4축 topology 완성·source 복구·Rebuild/검증, `975cab0`/`2f20844` 기록. |
| [042](Elmo_Master_history_260724_part_042_lines_10251_10500.md) | 10,251-10,500 | first slice capability `0x13F` 활성화 후 즉시 timeout과 shadow bug 발견. |
| [043](Elmo_Master_history_260724_part_043_lines_10501_10750.md) | 10,501-10,750 | submit pcap은 serializer/ticket까지만 증명, SDO_Test terminal timeout 분석. |
| [044](Elmo_Master_history_260724_part_044_lines_10751_11000.md) | 10,751-11,000 | self-assignment/race 수정, Slave 4 completed capture와 회복 기록. |
| [045](Elmo_Master_history_260724_part_045_lines_11001_11250.md) | 11,001-11,250 | 4축 first-slice 성공, 완료 범위 과장 정정, general 1/2/4-byte read 확대. |
| [046](Elmo_Master_history_260724_part_046_lines_11251_11500.md) | 11,251-11,500 | ResourceBusy state-machine race 수정, 104/104, 65-row API 요구사항 분석. |
| [047](Elmo_Master_history_260724_part_047_lines_11501_11750.md) | 11,501-11,750 | StopCmdNo 의미 정정, Group option/coord contract와 Phase 1 read-only 구현. |
| [048](Elmo_Master_history_260724_part_048_lines_11751_12001.md) | 11,751-12,001 | Phase 1 마감 135/135, Phase 2 relative motion·session generation fix, 148/148. |
| [049](Elmo_Master_history_260724_part_049_lines_12002_12251.md) | 12,002-12,251 | 01-08 live capture 판정, GroupEnable same-cycle 결함, UI 개선 항목 확정. |
| [050](Elmo_Master_history_260724_part_050_lines_12252_12501.md) | 12,252-12,501 | 05b/08b/10/11 및 12/04b/08c 결과, dynamic timeout·PowerOff UI 보강. |
| [051](Elmo_Master_history_260724_part_051_lines_12502_12751.md) | 12,502-12,751 | 09b None/ACS 재검증, next qualification 설계와 Group/Bulk/Recorder runner 착수. |
| [052](Elmo_Master_history_260724_part_052_lines_12752_13001.md) | 12,752-13,001 | runner 안전 review: prerequisite, cleanup Stop 조건, selected-axis return, state recovery. |
| [053](Elmo_Master_history_260724_part_053_lines_13002_13251.md) | 13,002-13,251 | WPF visual smoke를 위한 Computer Use 지침/창 탐색. |
| [054](Elmo_Master_history_260724_part_054_lines_13252_13501.md) | 13,252-13,501 | WPF app launch·화면 관찰과 09b 관련 도구 state. |
| [055](Elmo_Master_history_260724_part_055_lines_13502_13751.md) | 13,502-13,751 | Computer Use confirmation/guidance 및 WPF target window 준비. |
| [056](Elmo_Master_history_260724_part_056_lines_13752_14001.md) | 13,752-14,001 | Group Qualification panel이 실제 WPF에 생성된 것을 확인. |
| [057](Elmo_Master_history_260724_part_057_lines_14002_14251.md) | 14,002-14,251 | Group runner 버튼/summary/progress visual smoke continuation. |
| [058](Elmo_Master_history_260724_part_058_lines_14252_14501.md) | 14,252-14,501 | Bulk Qualification panel과 24-entry/lifecycle controls 확인. |
| [059](Elmo_Master_history_260724_part_059_lines_14502_14751.md) | 14,502-14,751 | disconnected 상태의 disabled controls 확인, Recorder tab 전환 재시도. |
| [060](Elmo_Master_history_260724_part_060_lines_14752_15001.md) | 14,752-15,001 | Recorder Qualification panel visual smoke와 controls/accessibility 확인. |
| [061](Elmo_Master_history_260724_part_061_lines_15002_15251.md) | 15,002-15,251 | visual smoke 종료, Recorder/Bulk cleanup·safety gate 보정, 148/148 및 캡처 순서. |
| [062](Elmo_Master_history_260724_part_062_lines_15252_15501.md) | 15,252-15,501 | 3개 커밋·최종 검증, `Find in Implementation`/로그 판정 논쟁과 정정 시작. |
| [063](Elmo_Master_history_260724_part_063_lines_15502_15751.md) | 15,502-15,751 | Computer Use guidance/confirmation 전문; 새 프로젝트 결론 없음. |
| [064](Elmo_Master_history_260724_part_064_lines_15752_16001.md) | 15,752-16,001 | confirmation 정책 continuation과 열린 LASAL/Wireshark/VS 앱 inventory. |
| [065](Elmo_Master_history_260724_part_065_lines_16002_16251.md) | 16,002-16,251 | 앱 inventory continuation; LASAL Find Results 상태 읽기 도구 호출 직전 종료. |

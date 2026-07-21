# Elmo Master history digest: parts 073-144

## 주의

이 문서는 `Elmo_Master_history_260721_part_073...144`에 남은 과거 대화와 도구 실행 기록을 탐색하기 위한 인덱스다. 아래 내용은 **history-only 사실**이며 현재 소스, Git 상태, LASAL IDE 프로젝트, PLC 다운로드 상태 또는 실기 동작을 증명하지 않는다. 후속 작업 전에는 관련 소스와 `git status`를 다시 확인하고, PC 정적 검증과 LASAL/PLC 런타임 검증을 분리해야 한다. 각 part의 `Context compaction / Status: completed`는 대화 압축 완료 표시이지 구현 완료 표시가 아니다.

## Chronological phases

| Phase | Part 범위 | 요약 |
|---|---:|---|
| 1 | 073-080 | LASAL 축 client class를 실제 `Elmo_1..4`로 정정하고 저장했으나, IDE 저장이 기존 D0 `0x7E00` 구현을 되돌린 사실을 발견했다. |
| 2 | 081-099 | D1 서비스/latch/TCP client 메타데이터를 생성하고 C# 진단 계약을 확장했다. PC 테스트 86/86이 기록됐지만 PLC body와 런타임 연결은 미완료였다. |
| 3 | 100-102 | Computer Use 세션을 복구했다. 사용자는 완료 여부를 물으며 필요하면 계속하라고 했고, PLC 잔여 작업 때문에 미완료라고 답했다. |
| 4 | 103-122 | LASAL 프로젝트를 다시 열어 RT latch 변수와 snapshot 배열 메타데이터를 만들고 저장했다. network 배치를 시도하며 `.lcn`을 편집 가능한 LASAL XML로 확인했다. |
| 5 | 123-126 | D1 Health/Catalog/PI, 4축 PDO latch, seqlock, TCP/network 정적 구현을 넣었다. 독립 1 ms task를 폐기하고 `_LMCAxis1.LMCPreRtWorkTrigger` 연결로 고쳤다. |
| 6 | 127-130 | `LMCRecorderStore`를 만들던 중 Save All이 D2 소스를 덮는 충돌을 복구했다. 4 MB class-local 배열 대신 global bank를 택하고 D2 metadata를 Reload Class로 동기화했다. |
| 7 | 131-138 | Recorder 상태 전달과 client/network 연결을 구현했다. Reload Class가 초기화를 지우는 문제를 표준 Constructor로 고치고 Store 객체와 두 연결을 `.lcn`에 추가했다. |
| 8 | 139-144 | 첫 LASAL rebuild의 21개 오류를 수정해 `0 error`와 link 완료를 기록했다. PC/계약/ASCII 검증과 문서 보정 뒤 implementation smoke를 진행하던 중 part 144가 끝난다. |

## Part별 topical hint

- **073** — Hardware Editor/HW tree와 TCPMotionInterface, Elmo_4, TCP server 구현 상태가 담긴 UI 덤프.
- **074** — `Drive2`를 선택하고 기존 class 속성을 확인.
- **075** — 축별 Elmo class 정정과 D1/D2 진행 계획, 실행 PLC의 ENI 미검증 경계를 명시.
- **076** — Hardware/Class tree를 다시 관찰하고 Drive1~4 client 구조를 확인.
- **077** — Drive2의 Class 속성 편집에 진입.
- **078** — Drive2 class 목록을 열고 `Elmo_2` 선택을 시도.
- **079** — Drive2=`Elmo_2`, Drive3=`Elmo_3`를 확정하고 Drive4 편집을 시작.
- **080** — Drive4=`Elmo_4`를 저장하고 4축 client를 확인한 뒤 D0 `0x7E00` rollback을 발견.
- **081** — DiagnosticsService에 `InputLatch` client를 생성.
- **082** — InputLatch 이름과 channel type 속성을 편집.
- **083** — InputLatch를 Object Channel로 설정하기 위한 속성 조작.
- **084** — Object Channel을 확정하고 대상 `LMCEcatInputLatch` class를 탐색.
- **085** — InputLatch class 지정과 함께 D0 복구, D1 parser/계약, C# project, TCP source 변경을 시작.
- **086** — LASAL client metadata 저장과 열린 앱/창 상태 점검.
- **087** — DiagnosticsService의 `HandleRequest` method 생성 시도.
- **088** — HandleRequest 생성을 재시도하고 4축 PDO/ENI map 확인 결과를 기록.
- **089** — GLOBAL method 그룹에서 HandleRequest를 다시 생성.
- **090** — GLOBAL HandleRequest의 이름과 입력 확정 방식을 검증.
- **091** — HandleRequest 저장/visibility 확인 후 D2/D5 및 RT/TCP 배선 진행을 선언.
- **092** — 잘못 만들어진 PRIVATE method를 확인하고 제거.
- **093** — GLOBAL handler를 정상 생성·저장하고 RT latch를 점검.
- **094** — LMCEcatInputLatch의 `RealtimeTask` 설정을 활성화.
- **095** — RT latch GLOBAL `CopySnapshot` method를 생성·명명.
- **096** — CopySnapshot을 저장하고 source를 생성한 뒤 TCP client 구조를 점검.
- **097** — TCPMotionInterface에 `Diagnostics` client를 생성.
- **098** — Diagnostics client를 Object Channel로 설정.
- **099** — 대상 class를 LMCDiagnosticsService로 연결하고 PC D3~D5 통합 및 86/86 테스트를 기록.
- **100** — Computer Use 지침을 다시 읽는 구간.
- **101** — UI 확인 정책을 검토하고 LASAL 대상 창을 다시 선정.
- **102** — Computer Use kernel 종료 뒤 사용자가 계속 진행을 요청하고 미완료 범위를 재확정.
- **103** — LASAL 프로세스와 프로젝트 창을 찾고 IDE를 재실행.
- **104** — LASAL 시작을 기다린 뒤 Open Project 대화상자에 진입.
- **105** — `.lcp`를 선택하고 project load/compile을 시작.
- **106** — load/compile 종료 상태와 TCP/Class tree를 관찰.
- **107** — LASAL Output을 선택하고 오류 위치를 찾으려 시도.
- **108** — `LMCEcatInputLatch` class를 선택.
- **109** — RT latch class tree를 확장.
- **110** — 확장된 LASAL UI 상태 덤프이며 새 확정 결과는 없음.
- **111** — RT latch context menu에 진입하고 Computer Use API를 재확인.
- **112** — RT latch menu 위치 조정과 UI 탐색을 계속.
- **113** — `PublishSequence` 변수를 생성.
- **114** — PublishSequence datatype을 `UDINT`로 지정.
- **115** — UDINT 지정 뒤 class/property UI 상태를 확인.
- **116** — 다음 RT latch 변수 추가를 시작.
- **117** — snapshot 배열 생성 중 잘못 열린 method 생성을 취소.
- **118** — `GetSnapshotSize` 보조 method를 명명하고 variable menu로 복귀.
- **119** — snapshot 배열 변수 생성을 다시 시도한 뒤 context compaction.
- **120** — 자동화 상태를 복구하고 `SnapshotBytes` 이름과 array type을 설정.
- **121** — array element를 `USINT`로 지정하기 위한 반복 UI 작업.
- **122** — USINT 적용, Save All, network 배치 시도와 IDE 종료 후 `.lcn` XML 판단을 기록.
- **123** — D1 latch/service/TCP/network 및 contract script를 크게 구현하고 fail-closed 상태를 유지.
- **124** — 외부 source 변경 반영을 위해 LASAL 프로젝트를 다시 엶.
- **125** — `SnapshotBytes[0..511]`를 확인하고 RT trigger를 `LMCPreRtWorkTrigger`로 정정.
- **126** — D1 상태를 재확인하고 `LMCRecorderStore` class/ClassSvr를 정식 생성.
- **127** — Save All의 D2 overwrite 충돌을 복구하고 global-bank/manual Recorder 구현을 시작.
- **128** — Computer Use와 LASAL 창 연결 유실을 복구.
- **129** — LASAL 프로세스를 재실행하고 창 상태를 회복.
- **130** — Reload Class로 D2 변수와 `CallerSessionEpoch` metadata 반영을 확인.
- **131** — Recorder class source를 다시 불러오고 IDE metadata 항목을 확인.
- **132** — Recorder Configure 오염, RT/non-RT 원자성, 오류 계약을 수정하고 latch client를 추가.
- **133** — Latch와 DiagnosticsService의 RecorderStore Object Channel/class를 지정.
- **134** — Recorder/TCP 최신 source를 다시 불러오고 저장.
- **135** — RT sample, `0x7E40..49`, disconnect ownership을 구현하고 Constructor 초기화 유실을 발견.
- **136** — 표준 Constructor를 IDE metadata에 등록하고 초기화 위치를 재구성.
- **137** — Constructor 보존을 확인하고 `.lcn`에 Store 객체/양쪽 연결 및 TCP epoch 보완을 반영.
- **138** — 외부 network/source 변경 반영을 위해 프로젝트를 다시 엶.
- **139** — 대형 screenshot/tool-state 구간에서 LASAL 프로젝트/TCP 창을 다시 선정.
- **140** — TCP metadata 저장 후 첫 rebuild의 21 errors와 세 오류 유형을 수정.
- **141** — rebuild `0 error`와 link 완료, C78/C81 warning을 기록하고 connection/implementation smoke를 시작.
- **142** — WPF `dotnet build` 경로 문제, PC/계약/ASCII 통과, 실제 RT hook에 맞춘 문서 보정을 기록.
- **143** — Computer Use 지침 재로딩 구간.
- **144** — LASAL 프로젝트 창을 다시 선정·관찰하며 implementation smoke를 재개하던 상태로 종료.

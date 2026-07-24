# Chunk Digest: Parts 066-130

## 주의

이 문서는 `Elmo_Master_history_260724_part_066_*`부터 `part_130_*`까지,
원본 source line 16,252-32,501을 읽고 만든 **history-only 인덱스**다.
기록 당시의 코드, IDE 상태, 테스트 결과와 완료 판단을 현재 저장소의 사실로
간주하지 않는다. 작업 재개 전에는 현재 소스와 `git status`, LASAL 선언/Network,
관련 자동 검증 및 실제 PLC 상태를 다시 확인해야 한다. 분할본에서 placeholder로
치환된 거대 computer-use payload는 원본에서 재확장하지 않았고, 해당 줄의 hash
기록과 앞뒤 텍스트·도구 제목으로 흐름을 확인했다.

## Chronological phases

| Phase | Part / source line | 기록상 흐름 |
|---|---|---|
| LASAL 검색 동작 재판정 | 066-074 / 16,252-18,501 | `Find in Implementation` 결과가 없다는 초기 판단을 뒤집고, 결과 목록이 하단 summary 위치에 스크롤돼 있었음을 확인했다. 다만 결과 행을 여는 위치 매핑은 잘못됐고, 일반 `Ctrl+F`의 `Show in Find Results`가 실제 구현 행으로 이동하는 우회로로 확인됐다. |
| 성능 우선 분리 설계 | 074-075 / 18,252-18,751 | `Static Router + No-task Domain Service`를 선택했다. transport/session/FIFO/단일 `SendData`는 `TCPMotionInterface`에 남기고, pointer/capacity/actual-length ABI로 control service를 호출하며 task·queue·동적 할당·추가 frame copy를 만들지 않는 설계다. |
| Group handler 1차 분리 | 076-093 / 18,752-23,251 | 장시간 LASAL UI 탐색 끝에 private `HandleGroupCommands`를 IDE로 선언했다. 잘못 만든 `Server` channel은 저장 전 Undo했고, 외부 편집으로 Group 11개 명령을 옮겨 `MsgPaser` 크기를 줄였다. 정적/PC 회귀는 기록상 통과했지만 마지막 IDE smoke, LASAL build와 PLC download는 수행되지 않았다. |
| 나머지 family handler 분리 | 093-104 / 23,002-26,001 | 같은 class 안에 Admin/Diagnostics/Registry/Axis private handler를 IDE 등록하고 implementation을 외부 분리했다. 기록상 static/PC 회귀 후 `HandleAxisCommands`와 `HandleAdminCommands` 검색·행 이동 smoke가 성공했다. |
| control service 선언 시도 | 104-119 / 25,752-29,751 | no-task `LMCControlCommandService` class를 만들고 axis/robot client를 선언했다. 자동화가 처음 `LMCAxis1/2`를 잘못된 DINT channel로 만들었고 복제 과정에서 임시 이름도 꼬였으나, 사용자가 IDE에서 `LMCAxis1..9`와 `LMCRobot` object command client를 바로잡아 저장했다고 기록됐다. |
| service ABI 및 verifier 준비 | 119-127 / 29,502-31,751 | `HandleRequest`, private handlers/helpers, TCP `ControlCommands` 선언을 만들려 했으나 method access/drag·삭제와 창 활성화가 반복 실패해 저장본에는 반영되지 않았다. 대신 verifier가 private ABI를 잘못 검사하고 service object/11개 연결 누락을 놓치는 문제를 보강했다. SourceOnly는 미저장 `HandleRequest` 때문에 예상대로 실패했다. |
| 다음 실행의 IDE 재개 | 127-130 / 31,502-32,501 | 2026-07-24 재확인에서 LASAL이 닫혔고 method/TCP client 선언이 없음을 다시 확인했다. 새 Computer Use runtime으로 LASAL을 실행해 빈 workspace에서 `Open project`를 누른 시점에서 이 범위가 끝난다. |

## 주요 결정과 완료 기록

- 검색 문제는 두 층으로 구분됐다. `Find Results 1`의 `Total found: 29`는 실제로
  존재했으나 목록이 summary 쪽에 스크롤돼 있었고, `Home`으로 행을 노출할 수
  있었다. 그러나 `Find in Implementation` 결과를 열 때 선언 line 1/4로 잘못
  이동하는 문제는 남았다 (`part_069`, source 17,050-17,095).
- 실사용 우회는 implementation 편집기에서 일반 `Ctrl+F`를 열고
  `Show in Find Results`를 켠 뒤 검색하는 방법이다. `0x2047` 한 건을
  double-click했을 때 `MsgPaser`의 실제 구현 위치로 이동했다고 기록됐다
  (`part_074`, source 18,252-18,399). 당시 약 67 KB짜리 단일 implementation과
  64 KiB 경계를 원인 후보로 제시했지만 공식 제한을 확인한 결론은 아니었다.
- 선택한 구조는 static domain `CASE` 뒤 정확히 한 번의 no-task service
  `HandleRequest`를 호출하고, caller buffer에 직접 응답을 작성해 facade가 길이를
  검증한 뒤 `SendData`를 한 번만 실행하는 방식이다. service가 pointer를 보관하거나
  socket/session/FIFO를 소유하지 않으며 Command object, Chain of Responsibility,
  별도 task/mailbox를 금지했다 (`part_074`-`075`, source 18,405-18,607).
- 기록된 성능/구조 acceptance에는 1 ms task overrun 없음, CyWork p99 증가
  `<= 10 us`, end-to-end p99 증가 `<= 2%`, 추가 payload copy 없음, golden packet
  byte 동일, 명령 ID 단일 소유, class `<= 48 KiB`, 각 implementation `<= 32 KiB`가
  포함됐다 (`part_075`, source 18,502-18,607). 이는 목표 기준이지 실제 측정 결과가
  아니다.
- 1차 구현은 Group 11개를 같은 `TCPMotionInterface`의 private
  `HandleGroupCommands`로 옮긴 단계다. 기록상 `MsgPaser`는
  `67,081 -> 44,784 bytes`, 새 handler는 `23,926 bytes`였고 task, queue,
  frame copy와 wire는 바꾸지 않았다 (`part_092`-`093`, source
  22,976-23,043).
- 이어 Admin/Diagnostics/Registry/Axis family도 같은 class의 private method로
  분리했다. 이는 최종 service 이관 전에 diff와 IDE implementation 크기를 줄이는
  중간 단계였고, service route나 network를 바꾸는 단계는 아니었다
  (`part_093`, source 23,053-23,057; `part_101`-`104`, source
  25,035-25,964).
- no-task `LMCControlCommandService` class는 IDE에 등록됐고, 기록상 class 자체는
  `Automatic=false`, `SharedCommandTable=true`, `ClassSvr` 구조였다. 최종적으로
  저장 확인된 client는 `LMCAxis1..9`와
  `LMCRobot : CltChCmd__LMCRobotBase`였다 (`part_109`, source
  27,149-27,162; `part_114`, 28,372-28,470; `part_119`, 29,580-29,601).
- 설계된 public ABI는 `HandleRequest(CommandId : UINT, Reference : UINT,
  pRequestFrame : ^USINT, RequestFrameSize : UDINT, pResponseFrame : ^USINT,
  ResponseCapacity : UDINT) -> ResponseSize : DINT`다. Admin/Registry/Axis/Group
  private handler도 같은 ABI를 사용하고, `MoveLinearAbsEx`와 `GroupReadStatus`는
  더 좁은 helper ABI를 쓰도록 요청됐다 (`part_119`, source 29,586-30,001).

## 수정·재검토에서 잡힌 문제

- 결과가 없다는 초기 진단은 잘못이었다. `LMCAxis3` 검색 결과 29건은 존재했고
  `Home`으로 보였으므로 cache/index 미생성 문제가 아니라 LASAL 2.03.001의 결과
  목록 scroll-position 문제로 정정됐다. `_TCPIPServer` 검색 실패는 별도 현상으로
  남겼다 (`part_068`-`069`, source 16,752-17,095).
- Group method 생성 중 좌표 오조작으로 임시 `Server` channel과 dirty mark를
  만들었지만 `Ctrl+Z` 후 title의 별표가 사라진 것을 확인하고 저장하지 않았다
  (`part_087`-`090`, source 21,502-22,501; 최종 정리 `part_093`, 23,027-23,034).
- 처음 service client를 자동 생성할 때 `LMCAxis1`과 `LMCAxis2`가 object command
  client가 아니라 일반 DINT channel이 됐다. 자동화는 이를 미완료로 명시하고
  외부 `.st` 편집을 중단했다 (`part_109`, source 27,149-27,162).
- axis client 복제 도중 `LMCAxis52`, `LMCAxis51`, `LMCAxis50` 같은 임시 이름이
  나타났고, 이후 사용자가 `LMCAxis6..8`을 포함한 1..9 목록으로 수정했다
  (`part_112`-`114`, source 27,752-28,470).
- `HandleRequest`를 global로 바꾸려는 property/drag 조작이 성공하지 않았고,
  `HandleRegistryCommands`를 잘못 Global 폴더에 만든 뒤 삭제도 확정하지 못했다.
  이 method들은 디스크 미저장으로 판단했다 (`part_120`-`125`, source
  29,752-31,423).
- verifier 검토에서 올바른 private pointer/size ABI를 오히려 실패시키는 기준과,
  full-static이 service object 및 총 11개 Object Network 연결 누락을 놓칠 수 있는
  공백을 찾았다. 기록상 ABI 검사와 network topology 검사를 보강했다
  (`part_127`, source 31,605-31,635).

## 기록상 검증 결과

- Group 1차 분리: LASAL SourceOnly PASS, full static/generated metadata PASS,
  C# Debug `148/148 PASS`, C# Release `148/148 PASS`, `git diff --check` PASS
  (`part_093`, source 23,037-23,043).
- 위 결과에도 post-edit `Find in Implementation`, LASAL build, PLC download는
  사용자의 physical Escape로 Computer Use가 중단되어 미검증이었다
  (`part_093`, source 23,045-23,047).
- family handler 분리 후 정적 계약과 PC 회귀가 통과했다고 기록했고,
  `HandleAxisCommands`와 `HandleAdminCommands`는 호출부/구현부 각 1건이 검색되며
  구현 결과 double-click이 해당 함수 행으로 이동했다
  (`part_102`-`104`, source 25,263-25,964). 이 검사는 IDE source index smoke이며
  PLC 동작 증명이 아니라고 명시됐다.
- 당시 LASAL 화면의 기존 vendor library/project C81/C78 불일치로 보이는
  `1 error / 6 warnings`는 신규 handler source-index smoke와 분리해 취급했다
  (`part_103`, source 25,653).
- service 선언 준비 단계에서는 PowerShell syntax와 `git diff --check`가 PASS,
  SourceOnly는 `HandleRequest` 미저장 때문에 예상대로 FAIL이었다
  (`part_127`, source 31,620-31,628).
- 이 모든 결과는 transcript에 기록된 당시 결과이며 이 digest 작성 과정에서
  현재 checkout으로 재실행하지 않았다.

## 기록상 미완료·재개점

1. `LMCControlCommandService`의 global `HandleRequest`, private
   Admin/Registry/Axis/Group handler와 `MoveLinearAbsEx`, `GroupReadStatus` 선언을
   LASAL IDE에서 정확한 ABI로 생성·저장해야 한다.
2. `TCPMotionInterface`에 required object client `ControlCommands`를 만들고 class를
   `LMCControlCommandService`로 지정해야 한다.
3. task 없는 service object를 network에 배치하고 TCP-to-service 1개,
   service-to-axis 9개, service-to-robot 1개인 총 11개 연결을 완성해야 한다.
   이 범위에서는 객체 배치와 network 편집을 의도적으로 하지 않았다.
4. IDE 저장·종료 뒤 fail-closed service implementation과 static route를 외부
   `.st`에서 작성하고 SourceOnly/full-static/PC regression을 다시 실행해야 한다.
5. LASAL Rebuild, 변경 method의 `Find in Implementation`, 새
   `CInvalidArgException` 확인, PLC download와 실제 packet/performance regression이
   남아 있다.

`part_127`의 다음 실행은 LASAL이 닫힌 상태와 미저장 method/TCP client를 다시
확인한 뒤 IDE를 새로 열었다(source 31,641-31,652). `part_130` 마지막은 빈 LASAL
workspace에서 `Open project`를 선택한 도구 호출 도중 끝나므로(source
32,303-32,501), 이 범위만으로 Phase 2 service skeleton이 완성됐다고 판단하면
안 된다.

## Part index

| Part | Source line | Topical hint |
|---:|---:|---|
| [066](Elmo_Master_history_260724_part_066_lines_16252_16501.md) | 16,252-16,501 | LASAL `Find Results` 창과 class/implementation workspace 상태 확인. |
| [067](Elmo_Master_history_260724_part_067_lines_16502_16751.md) | 16,502-16,751 | 검색 결과가 보이지 않는다는 초기 진단과 UI 탐색 계속. |
| [068](Elmo_Master_history_260724_part_068_lines_16752_17001.md) | 16,752-17,001 | 결과 목록 summary/scroll 위치와 `Total found: 29` 단서 확인. |
| [069](Elmo_Master_history_260724_part_069_lines_17002_17251.md) | 17,002-17,251 | `Home`으로 결과 노출, index 문제 오판 정정, 행 단위 이동 요구 확인. |
| [070](Elmo_Master_history_260724_part_070_lines_17252_17501.md) | 17,252-17,501 | result 행 Enter/double-click의 잘못된 line 이동을 재현. |
| [071](Elmo_Master_history_260724_part_071_lines_17502_17751.md) | 17,502-17,751 | Edit 메뉴의 Find/Find in Files/Goto Line 경로 비교. |
| [072](Elmo_Master_history_260724_part_072_lines_17752_18001.md) | 17,752-18,001 | 일반 Find의 `Show in Find Results` 옵션 발견과 `0x2047` 검색 준비. |
| [073](Elmo_Master_history_260724_part_073_lines_18002_18251.md) | 18,002-18,251 | 좌표/포커스 재시도 후 `0x2047` 결과 생성·열기. |
| [074](Elmo_Master_history_260724_part_074_lines_18252_18501.md) | 18,252-18,501 | 일반 Find 우회 성공, `Find in Implementation` 결함 정리, performance-first OOP 설계 시작. |
| [075](Elmo_Master_history_260724_part_075_lines_18502_18751.md) | 18,502-18,751 | no-task service ABI, 금지 패턴, migration 순서와 acceptance 기준 확정. |
| [076](Elmo_Master_history_260724_part_076_lines_18752_19001.md) | 18,752-19,001 | LASAL 작업을 위한 Computer Use 지침과 초기 실행 준비. |
| [077](Elmo_Master_history_260724_part_077_lines_19002_19251.md) | 19,002-19,251 | LASAL 실행 및 초기 window 탐색. |
| [078](Elmo_Master_history_260724_part_078_lines_19252_19501.md) | 19,252-19,501 | 프로젝트 file picker 경로 선택 재시도. |
| [079](Elmo_Master_history_260724_part_079_lines_19502_19751.md) | 19,502-19,751 | 대상 project load 완료와 class tree 진입. |
| [080](Elmo_Master_history_260724_part_080_lines_19752_20001.md) | 19,752-20,001 | TCPMotionInterface class/root navigation. |
| [081](Elmo_Master_history_260724_part_081_lines_20002_20251.md) | 20,002-20,251 | class tree와 method 생성 위치 탐색. |
| [082](Elmo_Master_history_260724_part_082_lines_20252_20501.md) | 20,252-20,501 | LASAL class context menu 접근 시도. |
| [083](Elmo_Master_history_260724_part_083_lines_20502_20751.md) | 20,502-20,751 | TCP class node 재선택과 context action 반복. |
| [084](Elmo_Master_history_260724_part_084_lines_20752_21001.md) | 20,752-21,001 | class menu/method 선언 경로 탐색 계속. |
| [085](Elmo_Master_history_260724_part_085_lines_21002_21251.md) | 21,002-21,251 | private method 생성 전 TCP class/root 포커스 복구. |
| [086](Elmo_Master_history_260724_part_086_lines_21252_21501.md) | 21,252-21,501 | context menu에서 New Method 진입 준비. |
| [087](Elmo_Master_history_260724_part_087_lines_21502_21751.md) | 21,502-21,751 | New Method 메뉴 확인 중 잘못된 Server channel 생성. |
| [088](Elmo_Master_history_260724_part_088_lines_21752_22001.md) | 21,752-22,001 | accidental Server와 dirty 상태 확인, 복구 경로 탐색. |
| [089](Elmo_Master_history_260724_part_089_lines_22002_22251.md) | 22,002-22,251 | accidental Server를 저장하지 않고 Undo하기로 명시. |
| [090](Elmo_Master_history_260724_part_090_lines_22252_22501.md) | 22,252-22,501 | Ctrl+Z 후 dirty mark 소멸 확인, method 생성 재개. |
| [091](Elmo_Master_history_260724_part_091_lines_22502_22751.md) | 22,502-22,751 | private `HandleGroupCommands` 생성·명명. |
| [092](Elmo_Master_history_260724_part_092_lines_22752_23001.md) | 22,752-23,001 | method metadata 저장·IDE 종료, Group implementation/verifier/docs 편집. |
| [093](Elmo_Master_history_260724_part_093_lines_23002_23251.md) | 23,002-23,251 | Group 분리 검증 보고, Escape로 IDE smoke 중단, 다음 family split 착수. |
| [094](Elmo_Master_history_260724_part_094_lines_23252_23501.md) | 23,252-23,501 | LASAL 재실행 및 project open 준비. |
| [095](Elmo_Master_history_260724_part_095_lines_23502_23751.md) | 23,502-23,751 | project load와 TCP class 탐색. |
| [096](Elmo_Master_history_260724_part_096_lines_23752_24001.md) | 23,752-24,001 | private family method 선언 위치 진입. |
| [097](Elmo_Master_history_260724_part_097_lines_24002_24251.md) | 24,002-24,251 | Admin/Diagnostics/Registry/Axis 네 method 등록 계획 재확인. |
| [098](Elmo_Master_history_260724_part_098_lines_24252_24501.md) | 24,252-24,501 | `HandleAdminCommands` 생성. |
| [099](Elmo_Master_history_260724_part_099_lines_24502_24751.md) | 24,502-24,751 | `HandleDiagnosticsCommands` 생성·등록 확인. |
| [100](Elmo_Master_history_260724_part_100_lines_24752_25001.md) | 24,752-25,001 | `HandleRegistryCommands` 등록, `HandleAxisCommands` 생성 시작. |
| [101](Elmo_Master_history_260724_part_101_lines_25002_25251.md) | 25,002-25,251 | 네 method 등록 확인·metadata 저장·IDE 종료, source family split 적용. |
| [102](Elmo_Master_history_260724_part_102_lines_25252_25501.md) | 25,252-25,501 | 문서/contract/PC regression 갱신 및 source-index smoke용 LASAL 재실행. |
| [103](Elmo_Master_history_260724_part_103_lines_25502_25751.md) | 25,502-25,751 | project load, 기존 C81/C78 mismatch 분리, Axis handler 검색. |
| [104](Elmo_Master_history_260724_part_104_lines_25752_26001.md) | 25,752-26,001 | Admin handler 검색·행 이동 성공, no-task service skeleton 착수. |
| [105](Elmo_Master_history_260724_part_105_lines_26002_26251.md) | 26,002-26,251 | project load 재시도 후 `LMCControlCommandService` class 생성. |
| [106](Elmo_Master_history_260724_part_106_lines_26252_26501.md) | 26,252-26,501 | service client 생성 UI 탐색과 `LMCAxis1` channel type 설정 시도. |
| [107](Elmo_Master_history_260724_part_107_lines_26502_26751.md) | 26,502-26,751 | client 속성 편집·project 저장, stale window/state 복구. |
| [108](Elmo_Master_history_260724_part_108_lines_26752_27001.md) | 26,752-27,001 | 저장된 service class/Clients/Methods tree와 workspace 대형 상태 확인. |
| [109](Elmo_Master_history_260724_part_109_lines_27002_27251.md) | 27,002-27,251 | LMCAxis2 type 설정 실패와 user input 감지; DINT client 오생성 명시. |
| [110](Elmo_Master_history_260724_part_110_lines_27252_27501.md) | 27,252-27,501 | Computer Use guidance/confirmation payload. |
| [111](Elmo_Master_history_260724_part_111_lines_27502_27751.md) | 27,502-27,751 | 사용자가 만든 선언 확인·저장, LMCAxis4를 복제해 Axis5 생성. |
| [112](Elmo_Master_history_260724_part_112_lines_27752_28001.md) | 27,752-28,001 | Axis client 반복 복제/rename과 잘못된 clone 이름 발생. |
| [113](Elmo_Master_history_260724_part_113_lines_28002_28251.md) | 28,002-28,251 | client 목록에서 Axis1..5/9/52/51/50 상태 확인. |
| [114](Elmo_Master_history_260724_part_114_lines_28252_28501.md) | 28,252-28,501 | Axis8 rename 시도 후 사용자 수정; Axis1..9 정상 목록 확인. |
| [115](Elmo_Master_history_260724_part_115_lines_28502_28751.md) | 28,502-28,751 | LMCRobot client 추가를 위한 Clients 메뉴/API 탐색. |
| [116](Elmo_Master_history_260724_part_116_lines_28752_29001.md) | 28,752-29,001 | Computer Use guidance와 class/client tree 재확인. |
| [117](Elmo_Master_history_260724_part_117_lines_29002_29251.md) | 29,002-29,251 | Axis1..9 확인 후 `LMCRobot` client 생성 시작. |
| [118](Elmo_Master_history_260724_part_118_lines_29252_29501.md) | 29,252-29,501 | LMCRobot 명명·확정과 channel type 편집 시도. |
| [119](Elmo_Master_history_260724_part_119_lines_29502_29751.md) | 29,502-29,751 | Axis/Robot 저장 확인, 필요한 method ABI/TCP client 목록 제시, Methods 진입. |
| [120](Elmo_Master_history_260724_part_120_lines_29752_30001.md) | 29,752-30,001 | `HandleRequest` 생성·rename 후 global access 설정 시도. |
| [121](Elmo_Master_history_260724_part_121_lines_30002_30251.md) | 30,002-30,251 | Global access property 토글/편집 실패와 API 조사. |
| [122](Elmo_Master_history_260724_part_122_lines_30252_30501.md) | 30,252-30,501 | HandleRequest drag/global 이동 재시도와 Global folder method 생성. |
| [123](Elmo_Master_history_260724_part_123_lines_30502_30751.md) | 30,502-30,751 | Registry를 잘못 Global에 생성, 삭제 실패, 미저장 상태로 UI 복구. |
| [124](Elmo_Master_history_260724_part_124_lines_30752_31001.md) | 30,752-31,001 | stale LASAL window 재연결·활성화 실패와 UI API 재확인. |
| [125](Elmo_Master_history_260724_part_125_lines_31002_31251.md) | 31,002-31,251 | 저장 상태 요약: Axis/Robot만 완료, methods/TCP client 없음, 사용자 저장 요청. |
| [126](Elmo_Master_history_260724_part_126_lines_31252_31501.md) | 31,252-31,501 | GUI 복구를 위한 Computer Use guidance/confirmation 재로딩. |
| [127](Elmo_Master_history_260724_part_127_lines_31502_31751.md) | 31,502-31,751 | activation 실패, verifier ABI/network 검사 보강, 예상 SourceOnly FAIL; 다음 실행 시작. |
| [128](Elmo_Master_history_260724_part_128_lines_31752_32001.md) | 31,752-32,001 | 새 Computer Use runtime의 guidance/confirmation 전문. |
| [129](Elmo_Master_history_260724_part_129_lines_32002_32251.md) | 32,002-32,251 | Windows control API 확인과 LASAL Class 2가 닫힌 상태 확인. |
| [130](Elmo_Master_history_260724_part_130_lines_32252_32501.md) | 32,252-32,501 | LASAL 재실행 성공, 빈 workspace 확인, Open project 선택 시점에서 종료. |

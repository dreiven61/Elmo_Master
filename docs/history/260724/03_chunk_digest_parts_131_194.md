# Chunk Digest: Parts 131-194

## 주의

이 문서는 `Elmo_Master_history_260724_part_131_*`부터 `part_194_*`까지,
원본 source line 32,502-48,495를 읽고 만든 **history-only 인덱스**다.
기록 당시의 코드, 테스트 결과, Git 상태 및 완료 판단을 현재 저장소의 사실로
간주하지 않는다. 작업 재개 전에는 현재 소스, `git status`, 관련 자동 검증,
LASAL Rebuild 결과와 실제 PLC/패킷 상태를 다시 확인해야 한다. 분할본에서
placeholder로 치환된 거대 computer-use payload는 원본에서 재확장하지 않았고,
앞뒤의 텍스트 기록과 도구 제목을 이용해 흐름만 색인했다.

## Chronological phases

| Phase | Part / source line | 기록상 흐름 |
|---|---|---|
| LASAL 프로젝트 복구 | 131-133 / 32,502-33,251 | LASAL 프로젝트를 다시 열고 중복 창·stale UI element 문제를 정리한 뒤 대상 프로젝트 로드를 확인했다. |
| Phase 2 서비스 ABI 생성 | 134-147 / 33,252-36,751 | `LMCControlCommandService.HandleRequest`를 만들고 `CommandId`, `Reference`, request/response pointer·size, `ResponseSize` ABI를 LASAL IDE 선언 편집기로 생성했다. |
| private handler/helper 선언 | 148-160 / 36,752-40,001 | 전역 메서드를 복제해 Registry/Axis/Group/Admin handler와 `MoveLinearAbsEx`, `GroupReadStatus`를 만들고 helper별 불필요한 인자를 제거했다. UI paste/rename 재시도가 길게 이어졌다. |
| TCP client 및 Phase 2 골격 마감 | 161-171 / 40,002-42,751 | `TCPMotionInterface.ControlCommands` required object client를 생성·저장하고 implementation lookup smoke를 수행했다. IDE 종료 후 7개 service stub을 fail-closed로 작성하고 CRLF 검증기 오판을 수정했다. 기록상 Phase 2 SourceOnly 골격이 완료됐지만 full static은 service object 부재로 의도적으로 실패했다. |
| Phase 3 선언 준비 | 172-191 / 42,752-47,751 | Computer Use 지침을 다시 읽고 LASAL을 열어 `GroupMovePos`, `GroupKinematicReady`와 `MoveLinearAbsEx` request pointer/size 선언을 추가했다. 중간에 생긴 `Type0` 흔적을 제거한 뒤 저장·Rebuild했다. |
| Rebuild blocker 판별 및 Phase 3A | 192-194 / 47,752-48,495 | `2 errors / 46 warnings`가 class compile 오류가 아니라 미연결 `ControlCommands`와 연쇄 network-table 오류임을 확인했다. IDE 종료 후 Group 11개와 Admin 2개를 dormant service에 구현하고 verifier를 의미·변이 검사 수준으로 보강했다. 실제 TCP route는 기존 구현에 남겨 뒀다. |

## 주요 결정과 완료 기록

- 생성 선언과 class/client 구조는 LASAL IDE에서만 수정하고, implementation은 IDE
  종료 후 외부 `.st`에서 편집한다는 경계를 유지했다. 네트워크 객체 배치는 이
  구간에서 하지 않는다고 반복 명시했다
  (`part_133`, source 33,168; `part_147`, 36,570;
  `part_167`, 41,692; `part_171`, 42,655-42,679).
- Phase 2 기록상 결과는 service method 7개가 `ResponseSize := -1`인 fail-closed
  skeleton, `TCPMotionInterface.ControlCommands` required client, SourceOnly verifier
  강화였다 (`part_171`, source 42,591-42,596).
- Phase 2 시점 full static 실패는 의도된 checkpoint였다. 기록된 blocker는
  `LMCControlCommandService1 must exist exactly once...`였고, service object와
  연결 저장 후 Phase 3 원자 전환을 하기로 했다
  (`part_171`, source 42,607-42,616).
- Phase 3A는 production route를 바꾸지 않는 dormant 구현으로 한정했다. 기록상
  13개 ID는 Group 11개
  (`0x20D2`, `0x2047`, `0x2048`, `0x2049`, `0x204A`, `0x204B`,
  `0x2085`, `0x20A4`, `0x2045`, `0x2051`, `0x20E7`)와 Admin
  `0x7D20`, `0x7D22`이며 helper는 `MoveLinearAbsEx`, `GroupReadStatus`다
  (`part_171`, source 42,642; `part_194`, 48,458-48,462).
- 기록상 Phase 3A 완료 시점에도 실제 TCP/PLC 동작은 legacy
  `TCPMotionInterface` 경로를 사용했다. 신규 service body는 route 전 검증
  대상으로만 존재했다 (`part_194`, source 48,315; 48,458).

## 수정·재검토에서 잡힌 문제

- service method가 존재하는데도 verifier가 0개라고 판단한 원인은 LASAL 생성
  파일의 CRLF를 처리하지 못한 정규식이었다. 소스 문제가 아니라 verifier
  false-negative로 판정하고 수정했다 (`part_170`-`171`, source 42,496-42,503).
- `Classes.lcb` 전체에서 문자열만 찾는 검사는 stale metadata를 놓칠 수 있어,
  class별 record 범위를 읽는 검사로 강화했다 (`part_171`, source 42,567-42,573).
- Phase 3 선언 편집 중 `Type0`이 보였으나 최종 선언 점검에서는 존재하지 않고
  `GroupKinematicReady : BOOL`과 `MoveLinearAbsEx` request pointer/size만 남았다고
  기록했다 (`part_187`, source 46,578-46,634; `part_191`, 47,599).
- LASAL Rebuild의 `2 errors / 46 warnings`는 두 독립 compile 오류가 아니었다.
  실제 원인은 `Comm_Network.TCPMotionInterface1.ControlCommands has to be connected`,
  두 번째는 그 결과 network table을 쓸 수 없다는 연쇄 오류로 정리됐다
  (`part_191`, source 47,719; `part_193`, 48,108).
- 독립 리뷰에서 9-byte command 6곳이 `&`의 단락 평가를 기대해 8-byte malformed
  frame에서도 offset 8을 읽을 수 있는 pointer/size ABI 위반을 찾았고, 중첩 size
  gate로 수정했다 (`part_194`, source 48,326-48,355).
- verifier가 ID 존재만 검사하고 offset·response size·status mapping을 충분히
  고정하지 못한 공백을 발견했다. ACK 6개 필드 `+2/+4/+8/+12/+14`, `0x2045`
  오류 프레임과 `-2/-3` 구분, handler 오배선, 응답 순서, Axis2/3 누락,
  `&`에서 `|`로의 변이, motion 인자 누락까지 검출하도록 보강했다고 기록했다
  (`part_194`, source 48,295-48,332; 48,421-48,432).

## 기록상 검증 결과

- Phase 2 마감: PC `148/148 PASS`, WPF Debug build PASS, LASAL SourceOnly PASS,
  `Find in Implementation`의 `Power/pos/velo` 실제 행 검색 성공,
  신규 `CInvalidArgException` 0, `git diff --check` PASS
  (`part_171`, source 42,598-42,605).
- Phase 3 선언 Rebuild: `2 errors / 46 warnings`; class 선언/컴파일 자체가 아니라
  미연결 channel checkpoint로 판정
  (`part_191`-`193`, source 47,719-48,108).
- Phase 3A 마감: PC `148/148 PASS`, WPF Debug build PASS, Dormant
  SourceOnly/MSBuild 계약 PASS, Phase 2 및 잘못된 Routed checkpoint 거부,
  ASCII/transport dependency/diff check PASS, `CInvalidArgException` 0,
  독립 변이 검토 후 추가 P1/P2 없음
  (`part_194`, source 48,465-48,473).
- 위 결과는 모두 transcript에 기록된 당시 결과다. 이 digest에서는 현재
  checkout으로 재실행하지 않았다.

## 기록상 미완료·재개점

1. LASAL `Comm_Network`에 task 없는 `LMCControlCommandService1`을 정확히 1개 배치.
2. `TCPMotionInterface1.ControlCommands -> LMCControlCommandService1.ClassSvr`,
   service `LMCAxis1..9 -> _LMCAxis1..9.Control`,
   `LMCRobot -> _LMCRobotBase1.Control`의 총 11개 Object Network 연결 생성.
3. 기존 `TCPMotionInterface1`의 axis/robot 연결은 Phase 3B 원자 route 전환 전까지 유지.
4. 저장 후 LASAL Rebuild를 성공시키고 IDE 종료. 실패한 이전 Rebuild가 삭제한
   `ONE_Comm_Network_Table.st`는 이전 Git 파일로 복원하지 말고 성공한 Rebuild로
   재생성.
5. full-static 통과 후 13개 ID를 service로 원자 전환하는 Phase 3B 진행.
6. 이후 Phase 4 Axis/Registry/Admin 이동, Phase 5 transport 정리·전체 회귀,
   LASAL build와 실제 PLC packet/performance regression 수행.

이 재개점은 `part_194`, source 48,446-48,486에 기록돼 있다. 당시 커밋과
스테이징은 하지 않았고(source 48,488), 마지막 6행은 기술 결과가 아니라 usage
limit system error다 (`part_194`, source 48,490-48,495).

## Part index

| Part | Source line | Topical hint |
|---:|---:|---|
| [131](Elmo_Master_history_260724_part_131_lines_32502_32751.md) | 32,502-32,751 | 빈 LASAL workspace에서 프로젝트 열기, stale element/단축키 실패와 창 재탐색. |
| [132](Elmo_Master_history_260724_part_132_lines_32752_33001.md) | 32,752-33,001 | 프로젝트 대화상자 캡처, 중복 LASAL 창 식별·종료. |
| [133](Elmo_Master_history_260724_part_133_lines_33002_33251.md) | 33,002-33,251 | 대상 `.lcp` 선택·로드 후 Phase 2 선언 작업 재개 브리핑. |
| [134](Elmo_Master_history_260724_part_134_lines_33252_33501.md) | 33,252-33,501 | 열린 LASAL 창 재연결과 Computer Use 제어 상태 확인. |
| [135](Elmo_Master_history_260724_part_135_lines_33502_33751.md) | 33,502-33,751 | `LMCControlCommandService` class tree 진입과 대규모 workspace 상태 확인. |
| [136](Elmo_Master_history_260724_part_136_lines_33752_34001.md) | 33,752-34,001 | service의 Servers/Clients/Methods/Dependencies와 Global/Private 폴더 전개. |
| [137](Elmo_Master_history_260724_part_137_lines_34002_34251.md) | 34,002-34,251 | 기존 private `HandleAdminCommands` 확인 후 global `HandleRequest` 생성 시작. |
| [138](Elmo_Master_history_260724_part_138_lines_34252_34501.md) | 34,252-34,501 | 새 method를 `HandleRequest`로 명명·확정. |
| [139](Elmo_Master_history_260724_part_139_lines_34502_34751.md) | 34,502-34,751 | `HandleRequest` 생성 확인과 definition/parameter 편집 메뉴 진입. |
| [140](Elmo_Master_history_260724_part_140_lines_34752_35001.md) | 34,752-35,001 | 첫 입력 `CommandId` 생성·명명. |
| [141](Elmo_Master_history_260724_part_141_lines_35002_35251.md) | 35,002-35,251 | `CommandId : UINT` 확정 후 `Reference` 입력 추가 시작. |
| [142](Elmo_Master_history_260724_part_142_lines_35252_35501.md) | 35,252-35,501 | `Reference : UINT`와 `pRequestFrame` 입력 생성. |
| [143](Elmo_Master_history_260724_part_143_lines_35502_35751.md) | 35,502-35,751 | `pRequestFrame : ^USINT`와 `RequestFrameSize` 선언 작업. |
| [144](Elmo_Master_history_260724_part_144_lines_35752_36001.md) | 35,752-36,001 | `RequestFrameSize : UDINT` 확정과 `pResponseFrame` 생성. |
| [145](Elmo_Master_history_260724_part_145_lines_36002_36251.md) | 36,002-36,251 | `pResponseFrame : ^USINT`와 `ResponseCapacity` 생성. |
| [146](Elmo_Master_history_260724_part_146_lines_36252_36501.md) | 36,252-36,501 | `ResponseCapacity : UDINT`와 `ResponseSize` 출력 생성. |
| [147](Elmo_Master_history_260724_part_147_lines_36502_36751.md) | 36,502-36,751 | `HandleRequest` ABI 완료 확인 후 private handler 복제 시작. |
| [148](Elmo_Master_history_260724_part_148_lines_36752_37001.md) | 36,752-37,001 | 복제 method를 Private로 이동하고 access 속성 변경. |
| [149](Elmo_Master_history_260724_part_149_lines_37002_37251.md) | 37,002-37,251 | private clone ABI 확인과 rename UI 탐색. |
| [150](Elmo_Master_history_260724_part_150_lines_37252_37501.md) | 37,252-37,501 | clone을 `HandleRegistryCommands`로 명명하고 다음 clone 생성. |
| [151](Elmo_Master_history_260724_part_151_lines_37502_37751.md) | 37,502-37,751 | `HandleAxisCommands` 생성과 Group handler clone 준비. |
| [152](Elmo_Master_history_260724_part_152_lines_37752_38001.md) | 37,752-38,001 | Group clone copy/paste 메뉴 반복 시도. |
| [153](Elmo_Master_history_260724_part_153_lines_38002_38251.md) | 38,002-38,251 | 포커스·메뉴 상태 복구 후 Group handler paste 재시도. |
| [154](Elmo_Master_history_260724_part_154_lines_38252_38501.md) | 38,252-38,501 | Group clone 생성 성공, Registry/Axis ABI 상태 브리핑. |
| [155](Elmo_Master_history_260724_part_155_lines_38502_38751.md) | 38,502-38,751 | Group handler rename 및 기존 Admin handler 삭제·재생성 준비. |
| [156](Elmo_Master_history_260724_part_156_lines_38752_39001.md) | 38,752-39,001 | Admin handler clone/rename과 `MoveLinearAbsEx` helper clone 시작. |
| [157](Elmo_Master_history_260724_part_157_lines_39002_39251.md) | 39,002-39,251 | `MoveLinearAbsEx` 생성·rename, `GroupReadStatus` clone 시작. |
| [158](Elmo_Master_history_260724_part_158_lines_39252_39501.md) | 39,252-39,501 | `GroupReadStatus` 생성·rename과 helper ABI 정리 시작. |
| [159](Elmo_Master_history_260724_part_159_lines_39502_39751.md) | 39,502-39,751 | `MoveLinearAbsEx`에서 불필요한 `CommandId`/request 인자 제거. |
| [160](Elmo_Master_history_260724_part_160_lines_39752_40001.md) | 39,752-40,001 | helper별 ABI 삭제 작업과 `GroupReadStatus` 인자 축소. |
| [161](Elmo_Master_history_260724_part_161_lines_40002_40251.md) | 40,002-40,251 | service 7-method 선언 확인 후 TCP required client `ControlCommands` 생성. |
| [162](Elmo_Master_history_260724_part_162_lines_40252_40501.md) | 40,252-40,501 | `ControlCommands`를 Object Channel로 설정. |
| [163](Elmo_Master_history_260724_part_163_lines_40502_40751.md) | 40,502-40,751 | channel 대상 class를 `LMCControlCommandService`로 지정. |
| [164](Elmo_Master_history_260724_part_164_lines_40752_41001.md) | 40,752-41,001 | client 선언 저장 및 TCP class tree 재탐색. |
| [165](Elmo_Master_history_260724_part_165_lines_41002_41251.md) | 41,002-41,251 | class/Edit 메뉴와 선언 접근 경로 점검. |
| [166](Elmo_Master_history_260724_part_166_lines_41252_41501.md) | 41,252-41,501 | TCP Clients 아래 `ControlCommands` 저장 여부 확인. |
| [167](Elmo_Master_history_260724_part_167_lines_41502_41751.md) | 41,502-41,751 | network view 확인과 implementation lookup smoke 준비. |
| [168](Elmo_Master_history_260724_part_168_lines_41752_42001.md) | 41,752-42,001 | LASAL context action 및 Computer Use 제어 API 탐색. |
| [169](Elmo_Master_history_260724_part_169_lines_42002_42251.md) | 42,002-42,251 | `Power` channel implementation 검색과 결과 확인. |
| [170](Elmo_Master_history_260724_part_170_lines_42252_42501.md) | 42,252-42,501 | `pos`/`velo` 검색, LASAL 종료, fail-closed source 작성과 CRLF verifier 결함 발견. |
| [171](Elmo_Master_history_260724_part_171_lines_42502_42751.md) | 42,502-42,751 | Phase 2 골격 검증·문서화 완료 보고와 다음 요청의 Phase 3 dormant 준비 착수. |
| [172](Elmo_Master_history_260724_part_172_lines_42752_43001.md) | 42,752-43,001 | Computer Use guidance/confirmation 문서와 안전 제어 절차. |
| [173](Elmo_Master_history_260724_part_173_lines_43002_43251.md) | 43,002-43,251 | 실행 앱·창 목록에서 LASAL 대상 탐색. |
| [174](Elmo_Master_history_260724_part_174_lines_43252_43501.md) | 43,252-43,501 | 창 후보/target selection 자료와 desktop 상태 확인. |
| [175](Elmo_Master_history_260724_part_175_lines_43502_43751.md) | 43,502-43,751 | LASAL 실행·대기·workspace 확인과 프로젝트 열기. |
| [176](Elmo_Master_history_260724_part_176_lines_43752_44001.md) | 43,752-44,001 | 프로젝트 파일 선택 대화상자와 대상 `.lcp` 로드. |
| [177](Elmo_Master_history_260724_part_177_lines_44002_44251.md) | 44,002-44,251 | 프로젝트 로드 대기와 Class View 진입. |
| [178](Elmo_Master_history_260724_part_178_lines_44252_44501.md) | 44,252-44,501 | class tree 전개와 대형 LASAL UI 상태 기록. |
| [179](Elmo_Master_history_260724_part_179_lines_44502_44751.md) | 44,502-44,751 | Computer Use API 확인 후 control service class 열기. |
| [180](Elmo_Master_history_260724_part_180_lines_44752_45001.md) | 44,752-45,001 | service class action/menu 점검. |
| [181](Elmo_Master_history_260724_part_181_lines_45002_45251.md) | 45,002-45,251 | service implementation editor 및 workspace 상태 dump. |
| [182](Elmo_Master_history_260724_part_182_lines_45252_45501.md) | 45,252-45,501 | New Variable로 persistent Group state 생성 시작. |
| [183](Elmo_Master_history_260724_part_183_lines_45502_45751.md) | 45,502-45,751 | service 변수를 `GroupMovePos`로 명명·확정. |
| [184](Elmo_Master_history_260724_part_184_lines_45752_46001.md) | 45,752-46,001 | `GroupMovePos` 형식을 `_LMCPROF_POS`로 지정. |
| [185](Elmo_Master_history_260724_part_185_lines_46002_46251.md) | 46,002-46,251 | `GroupKinematicReady` 추가 준비와 대형 class/workspace 상태 기록. |
| [186](Elmo_Master_history_260724_part_186_lines_46252_46501.md) | 46,252-46,501 | kinematic state 생성 시도와 accidental type creation 취소. |
| [187](Elmo_Master_history_260724_part_187_lines_46502_46751.md) | 46,502-46,751 | `GroupMovePos` 및 `Type0` 흔적 확인 후 선언 정리 재개 계획. |
| [188](Elmo_Master_history_260724_part_188_lines_46752_47001.md) | 46,752-47,001 | LASAL 창 재연결과 declaration 상태 재검사. |
| [189](Elmo_Master_history_260724_part_189_lines_47002_47251.md) | 47,002-47,251 | service class menu에서 `GroupKinematicReady` 생성·명명. |
| [190](Elmo_Master_history_260724_part_190_lines_47252_47501.md) | 47,252-47,501 | `GroupKinematicReady : BOOL` 확정과 `MoveLinearAbsEx` request pointer 추가. |
| [191](Elmo_Master_history_260724_part_191_lines_47502_47751.md) | 47,502-47,751 | request size 인자, `Type0` 제거 확인, 저장·Rebuild 및 `2 errors / 46 warnings` 확인. |
| [192](Elmo_Master_history_260724_part_192_lines_47752_48001.md) | 47,752-48,001 | LASAL build output 탐색·스크롤·오류 원문 추출 준비. |
| [193](Elmo_Master_history_260724_part_193_lines_48002_48251.md) | 48,002-48,251 | 미연결 `ControlCommands`와 연쇄 table 오류 판별, IDE 종료, Phase 3A external 구현 착수. |
| [194](Elmo_Master_history_260724_part_194_lines_48252_48495.md) | 48,252-48,495 | Phase 3A 구현·pointer fix·semantic verifier·148 tests 마감, 사용자 network/Rebuild와 Phase 3B 이후를 재개점으로 남김. |

> 				34411 메뉴 항목 BUILD
> 				34412 메뉴 항목 DEBUG
> 				34413 메뉴 항목 ANALYZE
> 				34414 메뉴 항목 TOOLS
> 				34415 메뉴 항목 EXTRAS
> 				34416 메뉴 항목 WINDOW
> 				34417 메뉴 항목 HELP
> 		67 창 Splitter ID: 381326624
> 		68 창 Splitter ID: 381325280
> 		69 Tab Output ID: 274980984
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						26546 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							26547 단추 위쪽 스크롤 화살표 ID: UpButton
> 							26548 단추
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 트리 검색 실행",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:150,y:225,mouse_button:\"left\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "트리 검색 입력 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,3200));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		35461 대화 상자 Find in Tree Secondary Actions: Raise
> 			35462 텍스트 Find:
> 			35463 콤보 상자 Find: ID: 1106
> 				35464 편집 Find: ID: 1001
> 				35465 단추 열기 ID: DropDown
> 			35466 텍스트 What:
> 			35467 콤보 상자 What: ID: 1112
> 				35468 텍스트 What:
> 				35469 단추 열기 ID: DropDown
> 			35470 텍스트 Where:
> 			35471 콤보 상자 Where: ID: 1102
> 				35472 텍스트 Where:
> 				35473 단추 열기 ID: DropDown
> 			35474 확인란 Match whole word only ID: 1108
> 			35475 그룹 Direction
> 			35476 라디오 단추 (selectable) Up ID: 1110
> 			35477 라디오 단추 (selectable) Down ID: 1111
> 			35478 단추 Find Next ID: 1107
> 			35479 단추 Close ID: 2
> 			35480 제목 표시줄
> 				35481 메뉴 모음 시스템 ID: MenuBar
> 					35482 메뉴 항목 시스템
> 				35483 단추 닫기
> 		1 창 작업 영역 ID: 59648
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21397 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21398 위치 조정 위치 ID: ScrollbarThumb
> 						21399 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21400 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21401 위치 조정 (disabled)
> 			21402 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				21403 창 ID: 59648
> 					21404 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21405 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21406 위치 조정 위치 ID: ScrollbarThumb
> 						21407 단추 페이지 아래로 ID: DownPageButton
> 						21408 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21409 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21410 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21411 위치 조정 위치 ID: ScrollbarThumb
> 						21412 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21413 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21414 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				35484 단추 (disabled) Toggle bookmark
> 				35485 단추 (disabled) Previous bookmark
> 				35486 단추 (disabled) Next bookmark
> 				35487 단추 (disabled) Delete all bookmarks
> 				35488 단추 (disabled) Previous bookmark in this file
> 				35489 단추 (disabled) Next bookmark in this file
> 				35490 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				35491 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				35492 단추 (disabled) Unindent (Shift+Tab)
> 				35493 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				35494 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				35495 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				35496 단추 DataAnalyzer
> 				35497 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				35498 단추 Select
> 				35499 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				35500 단추 Go online (Alt+F6)
> 				35501 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				35502 메뉴 항목 Target Architecture
> 				35503 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				35504 단추 New project (Strg+N)
> 				35505 단추 Open a file (Strg+Shift+O)
> 				35506 단추 Close active document (Strg+F4)
> 				35507 단추 (disabled) Save file (Strg+S)
> 				35
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCEcatInputLatch 찾기",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"LMCEcatInputLatch\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 트리 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 선택 결과 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,2200));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		35461 대화 상자 Find in Tree Secondary Actions: Raise
> 			35462 텍스트 Find:
> 			35463 콤보 상자 Find: ID: 1106
> 				35464 편집 Find: ID: 1001
> 				35465 단추 열기 ID: DropDown
> 			35466 텍스트 What:
> 			35467 콤보 상자 What: ID: 1112
> 				35468 텍스트 What:
> 				35469 단추 열기 ID: DropDown
> 			35470 텍스트 Where:
> 			35471 콤보 상자 Where: ID: 1102
> 				35472 텍스트 Where:
> 				35473 단추 열기 ID: DropDown
> 			35474 확인란 Match whole word only ID: 1108
> 			35475 그룹 Direction
> 			35476 라디오 단추 (selectable) Up ID: 1110
> 			35477 라디오 단추 (selectable) Down ID: 1111
> 			35478 단추 Find Next ID: 1107
> 			35479 단추 Close ID: 2
> 			35480 제목 표시줄
> 				35481 메뉴 모음 시스템 ID: MenuBar
> 					35482 메뉴 항목 시스템
> 				35483 단추 닫기
> 		1 창 작업 영역 ID: 59648
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21397 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21398 위치 조정 위치 ID: ScrollbarThumb
> 						21399 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21400 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21401 위치 조정 (disabled)
> 			21402 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				21403 창 ID: 59648
> 					21404 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21405 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21406 위치 조정 위치 ID: ScrollbarThumb
> 						21407 단추 페이지 아래로 ID: DownPageButton
> 						21408 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21409 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21410 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21411 위치 조정 위치 ID: ScrollbarThumb

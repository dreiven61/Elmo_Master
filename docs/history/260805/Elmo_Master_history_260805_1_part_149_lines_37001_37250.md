> 				39938 LIST What:
> 					39939 스크롤 막대 (disabled) 세로 ID: NonClientVerticalScrollBar
> 						39940 단추 (disabled) 위쪽 스크롤 화살표 ID: UpButton
> 						39941 단추 (disabled) 아래쪽 스크롤 화살표 ID: DownButton
> 					39942 목록 항목 (selectable) Class
> 					39943 목록 항목 (selectable) Server
> 					39944 목록 항목 (selectable) Client
> 					39945 목록 항목 (selectable) Variables
> 					39946 목록 항목 (selectable) SfcElement
> 					39947 목록 항목 (selectable) Network
> 					39948 목록 항목 (selectable) Method
> 					39949 목록 항목 (selectable) Object
> 					39950 목록 항목 (selectable) Type
> 					39951 목록 항목 (selectable) File
> 					39952 목록 항목 (selectable) Type-Element
> 				39853 텍스트 What:
> 				39854 단추 닫기 ID: DropDown
> 			39855 텍스트 Where:
> 			39856 콤보 상자 Where: ID: 1102
> 				39857 텍스트 Where:
> 				39858 단추 열기 ID: DropDown
> 			39859 확인란 Match whole word only ID: 1108
> 			39860 그룹 Direction
> 			39861 라디오 단추 (selectable) Up ID: 1110
> 			39862 라디오 단추 (selectable) Down ID: 1111
> 			39863 단추 Find Next ID: 1107
> 			39864 단추 Close ID: 2
> 			39865 제목 표시줄
> 				39866 메뉴 모음 시스템 ID: MenuBar
> 					39867 메뉴 항목 시스템
> 				39868 단추 닫기
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
> 						214
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
>   "title": "메서드 검색으로 전환",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:104,y:198,mouse_button:\"left\"});"
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
>   "title": "함수명 입력란 선택",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:190,y:54,mouse_button:\"left\"});"
> }
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
>   "title": "기존 함수명 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+A\"});"
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
>   "title": "함수명 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"CopyAxisOwnershipStartupSnapshot\"});"
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
>   "title": "새 함수 트리 선택",
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
>   "title": "새 함수 선택 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/CopyAxisOwnershipStartupSnapshot|Find in Tree|Find Next|Close/.test(x)).slice(0,20));"
> }
> ```
>
> ```text
> [
>   '\t\t39846 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t\t39863 단추 Find Next ID: 1107',
>   '\t\t\t39864 단추 Close ID: 2',
>   '\t\t\t\t40296 단추 Close active document (Strg+F4)',
>   '\t\t\t\t40300 단추 Close project',
>   '\t\t\t80 단추 Close',
>   '\t\t\t\t\t\t\t\t38928 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot',
>   '\t\t\t88 단추 Close',
>   '\t\t\t96 단추 Close',
>   '\t\t39846 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t\t39863 단추 Find Next ID: 1107',
>   '\t\t\t39864 단추 Close ID: 2'
> ]
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
>   "title": "함수 검색 대화상자 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:382,y:87,mouse_button:\"left\"});"
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
>   "title": "함수 노드 위치 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({dialog:(globalThis.state.accessibility?.tree||\"\").includes(\"대화 상자 Find in Tree\"),selected:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/CopyAxisOwnershipStartupSnapshot/.test(x))});"
> }
> ```
>
> ```text
> {
>   dialog: false,
>   selected: [
>     '\t\t\t\t\t\t\t\t38928 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot'
>   ]
> }
> ```
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
>   "title": "함수 컨텍스트 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:865,y:182,mouse_button:\"right\"});"
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
>   "title": "Find in Implementation 명령 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,2600));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		42374 창
> 		42375 창
> 		42376 메뉴
> 			42785 메뉴 항목 Edit Method Enter
> 			42786 메뉴 항목 Delete Method Del
> 			42787 메뉴 항목 Move Up
> 			42788 메뉴 항목 (disabled) Move Down
> 			42789 메뉴 항목 Copy
> 			42790 메뉴 항목 New Input Variable
> 			42791 메뉴 항목 (disabled) New Output Variable
> 			42792 메뉴 항목 Add to Newinst
> 		1 창 작업 영역 ID: 59648
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281

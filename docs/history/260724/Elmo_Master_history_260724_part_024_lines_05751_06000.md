> 				78 단추 Open project (Strg+O)
> 				79 단추 (disabled) Save project changes (Strg+Shift+S)
> 				80 단추 (disabled) Close project
> 				81 단추 (disabled) Print
> 				82 단추 Cut (Strg+X)
> 				83 단추 Copy (Strg+C)
> 				84 단추 Paste (Strg+V)
> 				85 메뉴 항목 (disabled) Undo (Strg+Z)
> 				86 메뉴 항목 (disabled) Redo (Strg+Y)
> 				87 단추 (disabled) Navigate Backward (Alt+Left)
> 				88 단추 (disabled) Navigate Forward (Alt +Right)
> 			89 메뉴 모음 Menu Bar
> 				90 메뉴 항목 FILE
> 				91 메뉴 항목 EDIT
> 				92 메뉴 항목 VIEW
> 				93 메뉴 항목 PROJECT
> 				94 메뉴 항목 BUILD
> 				95 메뉴 항목 DEBUG
> 				96 메뉴 항목 ANALYZE
> 				97 메뉴 항목 TOOLS
> 				98 메뉴 항목 EXTRAS
> 				99 메뉴 항목 WINDOW
> 				100 메뉴 항목 HELP
> 		101 창 Splitter ID: 254458464
> 		102 창 Splitter ID: 254457624
> 		103 Tab Output ID: 251705512
> 			104 창 ID: 1200
> 				105 창 ID: 1200
> 					106 LIST ID: 1201
> 					107 스크롤 막대 (disabled) ID: 59904
> 						108 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						109 위치 조정 위치 ID: ScrollbarThumb
> 						110 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			111 탭 항목 (selectable) Python Script
> 			112 탭 항목 (selectable) Debugger
> 			113 탭 항목 (selectable) Output
> 			114 단추 Close
> 		115 창 Splitter ID: 254458296
> 		116 Tab Class View ID: 251698672
> 			117 트리 ID: 103
> 			118 탭 항목 (selectable) Lib
> 			119 탭 항목 (selectable) File
> 			120 탭 항목 (selectable) Global
> 			121 탭 항목 (selectable) Net
> 			122 탭 항목 (selectable) Class
> 			123 단추 Close
> 		1
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state; globalThis.state=null; await sky.click({window:observation.window,element_index:78}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,5000));}",
>   "title": "LASAL 프로젝트 선택 창 열기",
>   "timeout_ms": 30000
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
>   "code": "{const observation=globalThis.state; globalThis.state=null; await sky.press_key({window:observation.window,key:\"Return\"}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,2500));}",
>   "title": "프로젝트 폴더 열기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 대화 상자 Open a Lasal Projectfile Secondary Actions: Raise
> 			2 창
> 				3 도구 모음 명령 모듈 ID: FolderBandModuleInner
> 					4 단추 도움말(&H) Description: 도움말을 봅니다. ID: HelpButton
> 					5 확인란 미리 보기 창 Description: 미리 보기 창을 표시합니다. ID: PreviewButton
> 					6 분할 단추 자세히 Description: 보기를 변경합니다. ID: ViewControl
> 						7 단추 슬라이더 보기 Description: 기타 옵션 ID: SplitMenuButton
> 					8 단추 구성 Description: 이 폴더의 내용을 구성합니다. ID: {7DDC1264-7E4D-4F74-BBC0-D191987C8D0F}
> 					9 단추 새 폴더 Description: 새 폴더를 만듭니다. ID: {E44616AD-6DF1-4B94-85A4-E465AE8A19DB}
> 				10 창 호스트 제어 ID: ProperTreeHost
> 					11 트리 탐색 창 ID: 100
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 단추 페이지 위로 ID: UpPageButton
> 							15 위치 조정 위치 ID: ScrollbarThumb
> 							16 단추 페이지 아래로 ID: DownPageButton
> 							17 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						18 콘솔 트리 (selectable) 바탕 화면
> 							19 콘솔 트리 (selectable) 홈
> 							20 콘솔 트리 (selectable) 갤러리
> 							21 콘솔 트리 (selectable) 즐겨찾기 시작 - 바탕 화면(고정됨)
> 							22 콘솔 트리 (selectable) 다운로드(고정됨)
> 							23 콘솔 트리 (selectable) 문서(고정됨)
> 							24 콘솔 트리 (selectable) 사진(고정됨)
> 							25 콘솔 트리 (selectable) 192.168.0.18(고정됨)
> 							26 콘솔 트리 (selectable) Slam(고정됨)
> 							27 콘솔 트리 (selectable) SlamLocCommunicator(고정됨)
> 							28 콘솔 트리 (selectable) SlamLocCommunicatorQt(고정됨)
> 							29 콘솔 트리 (selectable) work(고정됨)
> 							30 콘솔 트리 (selectable) SEMICS(고정됨)
> 							31 콘솔 트리 (selectable) repos(고정됨)
> 							32 콘솔 트리 (selectable) CtrlX(고정됨)
> 							33 콘솔 트리 (selectable) Elmo_Master(고정됨)
> 							34 콘솔 트리 (selectable) WorkReport(고정됨)
> 							35 콘솔 트리 (selectable) GUDEL(고정됨)
> 							36 콘솔 트리 (selectable) Samhyun(고정됨)
> 							37 콘솔 트리 (selectable) JC_Solution(고정됨)
> 							38 콘솔 트리 (selectable) AGV_Driving_Platform(고정됨)
> 							39 콘솔 트리 (selectable) AGV_Driving_Platform_Document(고정됨)
> 							40 콘솔 트리 (selectable) OHT_PLC(고정됨)
> 							41 콘솔 트리 (selectable) KTI_OHT(고정됨)
> 							42 콘솔 트리 (selectable) ASTO(고정됨)
> 							43 콘솔 트리 (selectable) SIGMATEK(고정됨)
> 							44 콘솔 트리 (selectable) project_file(고정됨)
> 							45 콘솔 트리 (selectable) SKI(고정됨)
> 							46 콘솔 트리 (selectable) SEComSimulator(고정됨)
> 							47 콘솔 트리 (selectable) 03.ACS,OCS(고정됨)
> 							48 콘솔 트리 (selectable) 셋업일보(고정됨)
> 							49 콘솔 트리 (selectable) DeviceMap(고정됨)
> 							50 콘솔 트리 (selectable) 현장백업파일(고정됨)
> 							51 콘솔 트리 (selectable) Debug(고정됨)
> 							52 콘솔 트리 (selectable) VirtualAgv(고정됨)
> 							53 콘솔 트리 (selectable) MELSEC MC Protocol Samp
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||\"\"));",
>   "title": "프로젝트 파일 입력 포커스 확인",
>   "timeout_ms": 30000
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); globalThis.targetWindow=state.window; nodeRepl.write(\"Project file dialog observed\");",
>   "title": "프로젝트 파일 선택 위치 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Project file dialog observed
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
>   "code": "{const observation=globalThis.state; const sid=observation.screenshots?.[0]?.id; globalThis.state=null; await sky.click({window:observation.window,screenshotId:sid,x:485,y:297,click_count:2}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,3500));}",
>   "title": "Elmo LASAL 프로젝트 열기",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await new Promise(r=>setTimeout(r,3000)); globalThis.apps=await sky.list_apps(); globalThis.targetApp=apps.find(a=>a.id===\"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\"); nodeRepl.write(JSON.stringify(targetApp.windows,null,2));",
>   "title": "LASAL 프로젝트 로드 대기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 201694,
>     "title": "Lasal Class 2 (02.03.001)"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.targetWindow=await sky.get_window({id:targetApp.windows[0].id,app:targetApp.windows[0].app}); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,4500));",
>   "title": "LASAL 프로젝트 로드 상태 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			3 진행률 표시줄 ID: 120733000
> 			4 텍스트
> 			5 텍스트 Compiling
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트
> 			9 텍스트
> 			10 텍스트
> 			11 텍스트 Offline
> 			12 텍스트
> 			13 텍스트 NUM
> 			14 텍스트
> 		15 창 xtpBarTop ID: 59419
> 			16 도구 모음 Edit
> 				17 단추 (disabled) Toggle bookmark
> 				18 단추 (disabled) Previous bookmark
> 				19 단추 (disabled) Next bookmark
> 				20 단추 (disabled) Delete all bookmarks
> 				21 단추 (disabled) Previous bookmark in this file
> 				22 단추 (disabled) Next bookmark in this file

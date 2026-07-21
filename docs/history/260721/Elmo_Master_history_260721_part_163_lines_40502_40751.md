> 						106 목록 항목 (selectable)
> 						107 목록 항목 (selectable)
> 						108 목록 항목 (selectable)
> 						109 목록 항목 (selectable)
> 						110 목록 항목 (selectable)
> 						111 목록 항목 (selectable)
> 						112 목록 항목 (selectable)
> 						113 목록 항목 (selectable)
> 						114 목록 항목 (selectable)
> 						115 목록 항목 (selectable)
> 						116 목록 항목 (selectable)
> 						117 목록 항목 (selectable)
> 						118 목록 항목 (selectable)
> 						119 목록 항목 (selectable)
> 						120 목록 항목 (selectable)
> 						121 목록 항목 (selectable)
> 						122 목록 항목 (selectable)
> 						123 목록 항목 (selectable)
> 					124 스크롤 막대 ID: 59904
> 						125 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						126 위치 조정 위치 ID: ScrollbarThumb
> 						127 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			128 탭 항목 (selectable) Python Script
> 			129 탭 항목 (selectable) Output
> 			130 탭 항목 (selectable) Debugger
> 			131 단추 Close
> 		132 창 Splitter ID: 411854424
> 		133 Tab Class View ID: 409868448
> 			134 트리 ID: 103
> 			135 탭 항목 (selectable) Lib
> 			136 탭 항목 (selectable) File
> 			137 탭 항목 (selectable) Global
> 			138 탭 항목 (selectable) Net
> 			139 탭 항목 (selectable) Class
> 			140 단추 Close
> 		141 Tab Properties ID: 409871640
> 			142 창 ID: 121918456
> 				143 TABLE Properties Window ID: 127184512
> 				144 도구 모음 ID: 59392
> 					145 단추
> 					146 단추
> 			147 탭 항목 (selectable) Properties
> 			148 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 프로젝트 파일 선택",
>   "code": "{\n  var observationOpen = globalThis.state;\n  globalThis.state = null;\n  await sky.click({ window: observationOpen.window, element_index: 60 });\n  await new Promise(r => setTimeout(r, 500));\n  globalThis.windowsNow = await sky.list_windows();\n  nodeRepl.write(JSON.stringify(globalThis.windowsNow.map(w=>({id:w.id,app:w.app,title:w.title})), null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "id": 3084742,
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "title": "Lasal Class 2 (02.03.001)"
>   },
>   {
>     "id": 132772,
>     "app": "process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe",
>     "title": "ChatGPT"
>   },
>   {
>     "id": 595394,
>     "app": "process:C:\\Program Files\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\Studio\\Common\\ctrlX-PLC-Engineering.exe",
>     "title": "C:\\Users\\dreiv\\source\\repos\\BOSCH\\CtrlX\\PLC Engineering\\Gudel\\Gudel.project - ctrlX PLC Engineering"
>   },
>   {
>     "id": 132936,
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "title": "카카오톡"
>   },
>   {
>     "id": 267760,
>     "app": "process:C:\\Program Files\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\LogViewer.exe",
>     "title": "LogViewer (ctrlxworks.Studio.Launcher.txt)"
>   },
>   {
>     "id": 524684,
>     "app": "process:C:\\Windows\\explorer.exe",
>     "title": "GUDEL - 파일 탐색기 [ ssh_dev ↓0 ↑3 ]"
>   },
>   {
>     "id": 8916318,
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "title": "서보스타 전직원 단톡방"
>   },
>   {
>     "id": 1837062,
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "title": "Lasal Class 2 (02.03.001)"
>   },
>   {
>     "id": 396542,
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "title": "ChatGPT - Chrome"
>   },
>   {
>     "id": 68064,
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "title": "MMCLibDotNET_Test - Microsoft Visual Studio"
>   },
>   {
>     "id": 68068,
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "title": "PmasApiWpfTestApp - Microsoft Visual Studio"
>   },
>   {
>     "id": 68062,
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "title": "LasalApiWpfTestApp - Microsoft Visual Studio"
>   },
>   {
>     "id": 67542,
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "title": "EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel"
>   },
>   {
>     "id": 67890,
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "title": "Motion_Controller_API_정리.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel"
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
>   "title": "LASAL 파일 선택창 확인",
>   "code": "globalThis.targetWindow = await sky.get_window({ id: 3084742, app: \"process:C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\" });\nglobalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
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
> 			13 도구 모음 Script
> 			14 도구 모음 Edit
> 				15 단추 (disabled) Toggle bookmark
> 				16 단추 (disabled) Previous bookmark
> 				17 단추 (disabled) Next bookmark
> 				18 단추 (disabled) Delete all bookmarks
> 				19 단추 (disabled) Previous bookmark in this file
> 				20 단추 (disabled) Next bookmark in this file
> 				21 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				22 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				23 단추 (disabled) Unindent (Shift+Tab)
> 				24 단추 (disabled) Indent (Tab)
> 			25 도구 모음 Macros Manager
> 				26 메뉴 항목 Macros
> 			27 도구 모음 Layout Manager
> 				28 메뉴 항목 Layouts
> 			29 도구 모음 Toolbox
> 				30 단추 DataAnalyzer
> 				31 메뉴 항목 Toolbar Options
> 			32 도구 모음 Net Edit
> 				33 단추 (disabled) Select
> 				34 메뉴 항목 Toolbar Options
> 			35 도구 모음 Debug
> 				36 단추 Go online (Alt+F6)
> 				37 단추 Change Online Settings
> 				38 메뉴 항목 Online Connection
> 				39 단추 (disabled) Set Online Connection For Current Project
> 				40 단추 (disabled) Download (F6)
> 				41 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				42 단추 (disabled) Download Module on the Fly
> 				43 단추 (disabled) Save Project on PLC
> 				44 단추 (disabled) Start (F7)
> 				45 단추 (disabled) Reset (F8)
> 				46 단추 (disabled) Toggle breakpoint (F4)
> 				47 단추 (disabled) Create condition breakpoint
> 				48 메뉴 항목 Toolbar Options
> 			49 도구 모음 Build
> 				50 메뉴 항목 Target Architecture
> 				51 단추 (disabled) Build changes (F9)
> 				52 단추 (disabled) Rebuild project (Strg+F9)
> 				53 단추 (disabled) Cancel building (Ctrl+Break)
> 				54 단추 (disabled) Link project
> 			55 도구 모음 Standard
> 				56 단추 New project (Strg+N)
> 				57 단추 Open a file (Strg+Shift+O)
> 				58 단추 (disabled) Close active document (Strg+F4)
> 				59 단추 (disabled) Save file (Strg+S)
> 				60 단추 Open project (Strg+O)
> 				61 단추 (disabled) Save project changes (Strg+Shift+S)
> 				62 단추 (disabled) Close project
> 				63 단추 (disabled) Print
> 				64 단추 Cut (Strg+X)
> 				65 단추 Copy (Strg+C)
> 				66 단추 Paste (Strg+V)
> 				67 메뉴 항목 (disabled) Undo (Strg+Z)
> 				68 메뉴 항목 (disabled) Redo (Strg+Y)
> 				69 단추 (disabled) Navigate Backward (Alt+Left)
> 				70 단추 (disabled) Navigate Forward (Alt +Right)
> 			71 메뉴 모음 Menu Bar
> 				72 메뉴 항목 FILE
> 				73 메뉴 항목 EDIT
> 				74 메뉴 항목 VIEW
> 				75 메뉴 항목 PROJECT
> 				76 메뉴 항목 BUILD
> 				77 메뉴 항목 DEBUG
> 				78 메뉴 항목 ANALYZE
> 				79 메뉴 항목 TOOLS
> 				80 메뉴 항목 EXTRAS
> 				81 메뉴 항목 WINDOW
> 				82 메뉴 항목 HELP
> 		83 창 Splitter ID: 411855768
> 		84 창 Splitter ID: 411851736
> 		85 Tab Output ID: 409867992
> 			86 창 ID: 1200
> 				87 창 ID: 1200
> 					88 LIST ID: 1201
> 						89 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							90 단추 위쪽 스크롤 화살표 ID: UpButton
> 							91 단추 페이지 위로 ID: UpPageButton
> 							92 위치 조정 위치 ID: ScrollbarThumb
> 							93 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						94 목록 항목 (selectable)
> 						95 목록 항목 (selectable)
> 						96 목록 항목 (selectable)
> 						97 목록 항목 (selectable)
> 						98 목록 항목 (selectable)
> 						99 목록 항목 (selectable)
> 						100 목록 항목 (selectable)
> 						101 목록 항목 (selectable)
> 						102 목록 항목 (selectable)
> 						103 목록 항목 (selectable)

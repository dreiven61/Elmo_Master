> 						244 목록 항목 (selectable)
> 						245 목록 항목 (selectable)
> 						246 목록 항목 (selectable)
> 						247 목록 항목 (selectable)
> 						248 목록 항목 (selectable)
> 						249 목록 항목 (selectable)
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 						260 목록 항목 (selectable)
> 						261 목록 항목 (selectable)
> 						262 목록 항목 (selectable)
> 						263 목록 항목 (selectable)
> 						264 목록 항목 (selectable)
> 						265 목록 항목 (selectable)
> 						266 목록 항목 (selectable)
> 						267 목록 항목 (selectable)
> 						268 목록 항목 (selectable)
> 						269 목록 항목 (selectable)
> 						270 목록 항목 (selectable)
> 						271 목록 항목 (selectable)
> 					272 스크롤 막대 ID: 59904
> 						273 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						274 위치 조정 위치 ID: ScrollbarThumb
> 						275 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			276 탭 항목 (selectable) Python Script
> 			277 탭 항목 (selectable) Output
> 			278 탭 항목 (selectable) Debugger
> 			279 단추 Close
> 		280 창 Splitter ID: 411854424
> 		281 Tab Class View ID: 409868448
> 			282 트리 ID: 103
> 				283 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					284 단추 위쪽 스크롤 화살표 ID: UpButton
> 					285 위치 조정 위치 ID: ScrollbarThumb
> 					286 단추 페이지 아래로 ID: DownPageButton
> 					287 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				288 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					289 콘솔 트리 (selectable, disabled) External
> 					290 콘솔 트리 (selectable, disabled) Sigmatek
> 					291 콘솔 트리 (selectable, disabled) _TCPIPServer_RT
> 					292 콘솔 트리 (selectable, disabled) Elmo_1
> 					293 콘솔 트리 (selectable, disabled) Elmo_2
> 					294 콘솔 트리 (selectable, disabled) Elmo_3
> 					295 콘솔 트리 (selectable, disabled) Elmo_4
> 					296 콘솔 트리 (selectable, disabled) LMCDiagnosticsService
> 						297 콘솔 트리 (selectable, disabled) Servers
> 						298 콘솔 트리 (selectable, disabled) Clients
> 							299 콘솔 트리 (selectable, disabled) InputLatch
> 							300 콘솔 트리 (selectable, disabled) RecorderStore
> 						301 콘솔 트리 (selectable, disabled) Methods
> 						302 콘솔 트리 (selectable, disabled) Variables
> 						303 콘솔 트리 (selectable, disabled) Objects
> 						304 콘솔 트리 (selectable, disabled) Dependencies
> 					305 콘솔 트리 (selectable, disabled) LMCEcatInputLatch
> 					306 콘솔 트리 (selectable, disabled) LMCRecorderStore
> 					307 콘솔 트리 (selectable, disabled) TCPMotionInterface
> 			308 탭 항목 (selectable) Lib
> 			309 탭 항목 (selectable) File
> 			310 탭 항목 (selectable) Global
> 			311 탭 항목 (selectable) Net
> 			312 탭 항목 (selectable) Class
> 			313 단추 Close
> 		314 Tab Properties ID: 409871640
> 			315 창 ID: 121918456
> 				316 TABLE Properties Window ID: 127184512
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			320 탭 항목 (selectable) Properties
> 			321 단추 Close
> 		322 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			323 단추 예(Y) ID: 6
> 			324 단추 아니요(N) ID: 7
> 			325 이미지 ID: 20
> 			326 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			327 제목 표시줄
> 				328 단추 (disabled) 닫기
>
> The focused UI element is 0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise.
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
>   "title": "LASAL 확인창 위치 재확인",
>   "code": "globalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: false });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(\"Dialog screenshot captured.\");"
> }
> ```
>
> ```text
> Dialog screenshot captured.
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
>   "title": "LASAL 라이브러리 유지 선택",
>   "code": "{\n  var observationCoord = globalThis.state;\n  var shot = observationCoord?.screenshots?.[0]?.id;\n  if (!shot) throw new Error(\"No screenshot id\");\n  globalThis.state = null;\n  await sky.click({ window: observationCoord.window, screenshotId: shot, x: 808, y: 450 });\n  globalThis.state = await sky.get_window_state({ window: observationCoord.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
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
>   "title": "LASAL 프로젝트 열기 준비",
>   "code": "globalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: false, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
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
> 						104 목록 항목 (selectable)
> 						105 목록 항목 (selectable)

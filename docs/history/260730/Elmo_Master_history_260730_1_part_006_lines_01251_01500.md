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
> 			13 도구 모음 Edit
> 				14 단추 (disabled) Toggle bookmark
> 				15 단추 (disabled) Previous bookmark
> 				16 단추 (disabled) Next bookmark
> 				17 단추 (disabled) Delete all bookmarks
> 				18 단추 (disabled) Previous bookmark in this file
> 				19 단추 (disabled) Next bookmark in this file
> 				20 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				21 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				22 단추 (disabled) Unindent (Shift+Tab)
> 				23 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				25 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				27 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				29 단추 DataAnalyzer
> 				30 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				32 단추 (disabled) Select
> 				33 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				35 단추 Go online (Alt+F6)
> 				36 단추 Change Online Settings
> 				37 메뉴 항목 Online Connection
> 				38 단추 (disabled) Set Online Connection For Current Project
> 				39 단추 (disabled) Download (F6)
> 				40 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				41 단추 (disabled) Download Module on the Fly
> 				42 단추 (disabled) Save Project on PLC
> 				43 단추 (disabled) Start (F7)
> 				44 단추 (disabled) Reset (F8)
> 				45 단추 (disabled) Toggle breakpoint (F4)
> 				46 단추 (disabled) Create condition breakpoint
> 				47 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				49 메뉴 항목 Target Architecture
> 				50 단추 (disabled) Build changes (F9)
> 				51 단추 (disabled) Rebuild project (Strg+F9)
> 				52 단추 (disabled) Cancel building (Ctrl+Break)
> 				53 단추 (disabled) Link project
> 			54 도구 모음 Standard
> 				55 단추 New project (Strg+N)
> 				56 단추 Open a file (Strg+Shift+O)
> 				57 단추 (disabled) Close active document (Strg+F4)
> 				58 단추 (disabled) Save file (Strg+S)
> 				59 단추 Open project (Strg+O)
> 				60 단추 (disabled) Save project changes (Strg+Shift+S)
> 				61 단추 (disabled) Close project
> 				62 단추 (disabled) Print
> 				63 단추 Cut (Strg+X)
> 				64 단추 Copy (Strg+C)
> 				65 단추 Paste (Strg+V)
> 				66 메뉴 항목 (disabled) Undo (Strg+Z)
> 				67 메뉴 항목 (disabled) Redo (Strg+Y)
> 				68 단추 (disabled) Navigate Backward (Alt+Left)
> 				69 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				71 메뉴 항목 FILE
> 				72 메뉴 항목 EDIT
> 				73 메뉴 항목 VIEW
> 				74 메뉴 항목 PROJECT
> 				75 메뉴 항목 BUILD
> 				76 메뉴 항목 DEBUG
> 				77 메뉴 항목 ANALYZE
> 				78 메뉴 항목 TOOLS
> 				79 메뉴 항목 EXTRAS
> 				80 메뉴 항목 WINDOW
> 				81 메뉴 항목 HELP
> 		82 창 Splitter ID: 364541216
> 		83 창 Splitter ID: 364538024
> 		84 Tab Output ID: 363470888
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 					88 스크롤 막대 (disabled) ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			92 탭 항목 (selectable) Python Script
> 			93 탭 항목 (selectable) Debugger
> 			94 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 364539704
> 		97 Tab Class View ID: 363471800
> 			98 트리 ID: 103
> 			99 탭 항목 (selectable) Lib
> 			100 탭 항목 (selectable) File
> 			101 탭 항목 (selectable) Global
> 			102 탭 항목 (selectable) Net
> 			103 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 363467240
> 			106 창 ID: 243573088
> 				107 TABLE Properties Window ID: 254244504
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			111 탭 항목 (selectable) Properties
> 			112 단추 Close
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
>   "title": "Inspect LASAL startup state",
>   "code": "globalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));"
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
> 			13 도구 모음 Edit
> 				14 단추 (disabled) Toggle bookmark
> 				15 단추 (disabled) Previous bookmark
> 				16 단추 (disabled) Next bookmark
> 				17 단추 (disabled) Delete all bookmarks
> 				18 단추 (disabled) Previous bookmark in this file
> 				19 단추 (disabled) Next bookmark in this file
> 				20 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				21 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				22 단추 (disabled) Unindent (Shift+Tab)
> 				23 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				25 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				27 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				29 단추 DataAnalyzer
> 				30 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				32 단추 (disabled) Select
> 				33 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				35 단추 Go online (Alt+F6)
> 				36 단추 Change Online Settings
> 				37 메뉴 항목 Online Connection
> 				38 단추 (disabled) Set Online Connection For Current Project
> 				39 단추 (disabled) Download (F6)
> 				40 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				41 단추 (disabled) Download Module on the Fly
> 				42 단추 (disabled) Save Project on PLC
> 				43 단추 (disabled) Start (F7)
> 				44 단추 (disabled) Reset (F8)
> 				45 단추 (disabled) Toggle breakpoint (F4)
> 				46 단추 (disabled) Create condition breakpoint
> 				47 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				49 메뉴 항목 Target Architecture
> 				50 단추 (disabled) Build changes (F9)
> 				51 단추 (disabled) Rebuild project (Strg+F9)
> 				52 단추 (disabled) Cancel building (Ctrl+Break)
> 				53 단추 (disabled) Link project
> 			54 도구 모음 Standard
> 				55 단추 New project (Strg+N)
> 				56 단추 Open a file (Strg+Shift+O)
> 				57 단추 (disabled) Close active document (Strg+F4)
> 				58 단추 (disabled) Save file (Strg+S)
> 				59 단추 Open project (Strg+O)
> 				60 단추 (disabled) Save project changes (Strg+Shift+S)
> 				61 단추 (disabled) Close project
> 				62 단추 (disabled) Print
> 				63 단추 Cut (Strg+X)
> 				64 단추 Copy (Strg+C)
> 				65 단추 Paste (Strg+V)
> 				66 메뉴 항목 (disabled) Undo (Strg+Z)
> 				67 메뉴 항목 (disabled) Redo (Strg+Y)
> 				68 단추 (disabled) Navigate Backward (Alt+Left)
> 				69 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				71 메뉴 항목 FILE
> 				72 메뉴 항목 EDIT
> 				73 메뉴 항목 VIEW
> 				74 메뉴 항목 PROJECT
> 				75 메뉴 항목 BUILD
> 				76 메뉴 항목 DEBUG
> 				77 메뉴 항목 ANALYZE
> 				78 메뉴 항목 TOOLS
> 				79 메뉴 항목 EXTRAS
> 				80 메뉴 항목 WINDOW
> 				81 메뉴 항목 HELP
> 		82 창 Splitter ID: 364541216
> 		83 창 Splitter ID: 364538024
> 		84 Tab Output ID: 363470888
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 					88 스크롤 막대 (disabled) ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			92 탭 항목 (selectable) Python Script
> 			93 탭 항목 (selectable) Debugger
> 			94 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 364539704
> 		97 Tab Class View ID: 363471800
> 			98 트리 ID: 103
> 			99 탭 항목 (selectable) Lib
> 			100 탭 항목 (selectable) File
> 			101 탭 항목 (selectable) Global
> 			102 탭 항목 (selectable) Net
> 			103 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 363467240
> 			106 창 ID: 243573088
> 				107 TABLE Properties Window ID: 254244504
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			111 탭 항목 (selectable) Properties
> 			112 단추 Close
> ```
>

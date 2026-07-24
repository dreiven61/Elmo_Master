> 				63 단추 (disabled) Step into (F5)
> 				64 단추 (disabled) Step over (Alt+F5)
> 				65 단추 (disabled) Step out (Shift+F5)
> 				66 단추 (disabled) Set instruction pointer
> 			67 도구 모음 Build
> 				68 메뉴 항목 Target Architecture
> 				69 단추 (disabled) Build changes (F9)
> 				70 단추 (disabled) Rebuild project (Strg+F9)
> 				71 단추 (disabled) Cancel building (Ctrl+Break)
> 				72 단추 (disabled) Link project
> 			73 도구 모음 Standard
> 				74 단추 New project (Strg+N)
> 				75 단추 Open a file (Strg+Shift+O)
> 				76 단추 (disabled) Close active document (Strg+F4)
> 				77 단추 (disabled) Save file (Strg+S)
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
> 		101 창 Splitter ID: 358284568
> 		102 창 Splitter ID: 358287088
> 		103 Tab Output ID: 295836696
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
> 		115 창 Splitter ID: 358285744
> 		116 Tab Class View ID: 295832136
> 			117 트리 ID: 103
> 			118 탭 항목 (selectable) Lib
> 			119 탭 항목 (selectable) File
> 			120 탭 항목 (selectable) Global
> 			121 탭 항목 (selectable) Net
> 			122 탭 항목 (selectable) Class
> 			123 단추 Close
> 		124 Tab Properties ID: 295834416
> 			125 창 ID: 286533584
> 				126 TABLE Properties Window ID: 293432256
> 				127 도구 모음 ID: 59392
> 					128 단추
> 					129 단추
> 			130 탭 항목 (selectable) Properties
> 			131 단추 Close
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
>   "title": "LASAL 프로젝트 열기",
>   "code": "{const observation=globalThis.state;if(!observation?.accessibility) throw new Error('No observation');globalThis.state=null;await sky.click({window:observation.window,element_index:78});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||''));}"
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 상태 재확인",
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			3 진행률 표시줄 ID: 201831384
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
> 				23 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				24 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				25 단추 (disabled) Unindent (Shift+Tab)
> 				26 단추 (disabled) Indent (Tab)
> 			27 도구 모음 Macros Manager
> 				28 메뉴 항목 Macros
> 			29 도구 모음 Layout Manager
> 				30 메뉴 항목 Layouts
> 			31 도구 모음 Toolbox
> 				32 단추 DataAnalyzer
> 				33 단추 Interpreter
> 				34 단추 DiasDrive
> 				35 단추 PLC Diagnosis
> 				36 단추 (disabled) Hardware Editor
> 				37 단추 (disabled) Graphical Hardware Editor
> 				38 단추 (disabled) Connection Manager
> 				39 단추 (disabled) Task Configuration
> 			40 도구 모음 Net Edit
> 				41 단추 (disabled) Select
> 				42 단추 (disabled) Move view
> 				43 단추 (disabled) Insert comment
> 				44 단추 (disabled) Zoom(+/-)
> 				45 단추 (disabled) Zoom to all
> 				46 단추 (disabled) Zoom selection
> 			47 도구 모음 Debug
> 				48 단추 (disabled) Go online (Alt+F6)
> 				49 단추 (disabled) Change Online Settings
> 				50 메뉴 항목 (disabled) Online Connection
> 				51 단추 (disabled) Set Online Connection For Current Project
> 				52 단추 (disabled) Download (F6)
> 				53 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				54 단추 (disabled) Download Module on the Fly
> 				55 단추 (disabled) Save Project on PLC
> 				56 단추 (disabled) Start (F7)
> 				57 단추 (disabled) Reset (F8)
> 				58 단추 (disabled) Toggle breakpoint (F4)
> 				59 단추 (disabled) Create condition breakpoint
> 				60 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				61 단추 AWL trigger on/off
> 				62 단추 (disabled) Fix AWL trigger to current instruction
> 				63 단추 Activate/Deactivate Live View
> 				64 단추 Activate/Deactivate Auto Watch
> 				65 단추 (disabled) Goto instruction pointer
> 				66 단추 (disabled) Step into (F5)
> 				67 단추 (disabled) Step over (Alt+F5)
> 				68 단추 (disabled) Step out (Shift+F5)
> 				69 단추 (disabled) Set instruction pointer
> 			70 도구 모음 Build
> 				71 메뉴 항목 (disabled) Target Architecture
> 				72 단추 (disabled) Build changes (F9)
> 				73 단추 (disabled) Rebuild project (Strg+F9)
> 				74 단추 Cancel building (Ctrl+Break)
> 				75 단추 (disabled) Link project
> 			76 도구 모음 Standard
> 				77 단추 (disabled) New project (Strg+N)
> 				78 단추 Open a file (Strg+Shift+O)
> 				79 단추 (disabled) Close active document (Strg+F4)
> 				80 단추 (disabled) Save file (Strg+S)
> 				81 단추 (disabled) Open project (Strg+O)
> 				82 단추 (disabled) Save project changes (Strg+Shift+S)
> 				83 단추 (disabled) Close project
> 				84 단추 (disabled) Print
> 				85 단추 Cut (Strg+X)
> 				86 단추 Copy (Strg+C)
> 				87 단추 Paste (Strg+V)
> 				88 메뉴 항목 (disabled) Undo (Strg+Z)
> 				89 메뉴 항목 (disabled) Redo (Strg+Y)
> 				90 단추 (disabled) Navigate Backward (Alt+Left)
> 				91 단추 (disabled) Navigate Forward (Alt +Right)
> 			92 메뉴 모음 Menu Bar
> 				93 메뉴 항목 FILE
> 				94 메뉴 항목 EDIT
> 				95 메뉴 항목 VIEW
> 				96 메뉴 항목 PROJECT
> 				97 메뉴 항목 BUILD
> 				98 메뉴 항목 DEBUG
> 				99 메뉴 항목 ANALYZE
> 				100 메뉴 항목 TOOLS
> 				101 메뉴 항목 EXTRAS
> 				102 메뉴 항목 WINDOW
> 				103 메뉴 항목 HELP
> 		104 창 Splitter ID: 358284568
> 		105 창 Splitter ID: 358287088
> 		106 Tab Output ID: 295836696
> 			107 창 ID: 1200
> 				108 창 ID: 1200
> 					109 LIST ID: 1201
> 						110 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							111 단추 위쪽 스크롤 화살표 ID: UpButton
> 							112 단추 페이지 위로 ID: UpPageButton
> 							113 위치 조정 위치 ID: ScrollbarThumb
> 							114 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						115 목록 항목 (selectable)
> 						116 목록 항목 (selectable)
> 						117 목록 항목 (selectable)
> 						118 목록 항목 (selectable)
> 						119 목록 항목 (selectable)
> 						120 목록 항목 (selectable)
> 						121 목록 항목 (selectable)
> 						122 목록 항목 (selectable)
> 						123 목록 항목 (selectable)
> 						124 목록 항목 (selectable)
> 						125 목록 항목 (selectable)
> 						126 목록 항목 (selectable)
> 						127 목록 항목 (selectable)
> 						128 목록 항목 (selectable)
> 						129 목록 항목 (selectable)
> 						130 목록 항목 (selectable)
> 						131 목록 항목 (selectable)
> 						132 목록 항목 (selectable)
> 						133 목록 항목 (selectable)
> 						134 목록 항목 (selectable)
> 						135 목록 항목 (selectable)
> 						136 목록 항목 (selectable)
> 						137 목록 항목 (selectable)
> 						138 목록 항목 (selectable)
> 						139 목록 항목 (selectable)
> 						140 목록 항목 (selectable)
> 						141 목록 항목 (selectable)
> 						142 목록 항목 (selectable)
> 						143 목록 항목 (selectable)
> 						144 목록 항목 (selectable)
> 						145 목록 항목 (selectable)
> 						146 목록 항목 (selectable)
> 						147 목록 항목 (selectable)
> 						148 목록 항목 (selectable)

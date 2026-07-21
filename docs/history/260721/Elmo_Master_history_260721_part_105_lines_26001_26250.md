> project file dialog screenshot ready
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var obs2=globalThis.state; var sid=obs2.screenshots?.[0]?.id; if(!sid) throw new Error('no screenshot'); globalThis.state=null; await sky.click({window:obs2.window,screenshotId:sid,x:503,y:299}); globalThis.state=await sky.get_window_state({window:obs2.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));",
>   "title": "프로젝트 파일 선택"
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
>   "code": "var obs3=globalThis.state; globalThis.state=null; await sky.press_key({window:obs3.window,key:'ENTER'}); await new Promise(r=>setTimeout(r,2500)); globalThis.state=await sky.get_window_state({window:obs3.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "LASAL 프로젝트 로드"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			3 진행률 표시줄 ID: 207928296
> 			4 텍스트
> 			5 텍스트 Load Project
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
> 			16 도구 모음 Script
> 			17 도구 모음 Edit
> 				18 단추 (disabled) Toggle bookmark
> 				19 단추 (disabled) Previous bookmark
> 				20 단추 (disabled) Next bookmark
> 				21 단추 (disabled) Delete all bookmarks
> 				22 단추 (disabled) Previous bookmark in this file
> 				23 단추 (disabled) Next bookmark in this file
> 				24 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				25 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				26 단추 (disabled) Unindent (Shift+Tab)
> 				27 단추 (disabled) Indent (Tab)
> 			28 도구 모음 Macros Manager
> 				29 메뉴 항목 Macros
> 			30 도구 모음 Layout Manager
> 				31 메뉴 항목 Layouts
> 			32 도구 모음 Toolbox
> 				33 단추 DataAnalyzer
> 				34 단추 Interpreter
> 				35 단추 DiasDrive
> 				36 단추 PLC Diagnosis
> 				37 단추 (disabled) Hardware Editor
> 				38 단추 (disabled) Graphical Hardware Editor
> 				39 단추 (disabled) Connection Manager
> 				40 단추 (disabled) Task Configuration
> 			41 도구 모음 Net Edit
> 				42 단추 (disabled) Select
> 				43 단추 (disabled) Move view
> 				44 단추 (disabled) Insert comment
> 				45 단추 (disabled) Zoom(+/-)
> 				46 단추 (disabled) Zoom to all
> 				47 단추 (disabled) Zoom selection
> 			48 도구 모음 Debug
> 				49 단추 (disabled) Go online (Alt+F6)
> 				50 단추 (disabled) Change Online Settings
> 				51 메뉴 항목 (disabled) Online Connection
> 				52 단추 (disabled) Set Online Connection For Current Project
> 				53 단추 (disabled) Download (F6)
> 				54 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				55 단추 (disabled) Download Module on the Fly
> 				56 단추 (disabled) Save Project on PLC
> 				57 단추 (disabled) Start (F7)
> 				58 단추 (disabled) Reset (F8)
> 				59 단추 (disabled) Toggle breakpoint (F4)
> 				60 단추 (disabled) Create condition breakpoint
> 				61 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				62 단추 AWL trigger on/off
> 				63 단추 (disabled) Fix AWL trigger to current instruction
> 				64 단추 Activate/Deactivate Live View
> 				65 단추 Activate/Deactivate Auto Watch
> 				66 단추 (disabled) Goto instruction pointer
> 				67 단추 (disabled) Step into (F5)
> 				68 단추 (disabled) Step over (Alt+F5)
> 				69 단추 (disabled) Step out (Shift+F5)
> 				70 단추 (disabled) Set instruction pointer
> 			71 도구 모음 Build
> 				72 메뉴 항목 (disabled) Target Architecture
> 				73 단추 (disabled) Build changes (F9)
> 				74 단추 (disabled) Rebuild project (Strg+F9)
> 				75 단추 Cancel building (Ctrl+Break)
> 				76 단추 (disabled) Link project
> 			77 도구 모음 Standard
> 				78 단추 (disabled) New project (Strg+N)
> 				79 단추 Open a file (Strg+Shift+O)
> 				80 단추 (disabled) Close active document (Strg+F4)
> 				81 단추 (disabled) Save file (Strg+S)
> 				82 단추 (disabled) Open project (Strg+O)
> 				83 단추 (disabled) Save project changes (Strg+Shift+S)
> 				84 단추 (disabled) Close project
> 				85 단추 (disabled) Print
> 				86 단추 Cut (Strg+X)
> 				87 단추 Copy (Strg+C)
> 				88 단추 Paste (Strg+V)
> 				89 메뉴 항목 (disabled) Undo (Strg+Z)
> 				90 메뉴 항목 (disabled) Redo (Strg+Y)
> 				91 단추 (disabled) Navigate Backward (Alt+Left)
> 				92 단추 (disabled) Navigate Forward (Alt +Right)
> 			93 메뉴 모음 Menu Bar
> 				94 메뉴 항목 FILE
> 				95 메뉴 항목 EDIT
> 				96 메뉴 항목 VIEW
> 				97 메뉴 항목 PROJECT
> 				98 메뉴 항목 BUILD
> 				99 메뉴 항목 DEBUG
> 				100 메뉴 항목 ANALYZE
> 				101 메뉴 항목 TOOLS
> 				102 메뉴 항목 EXTRAS
> 				103 메뉴 항목 WINDOW
> 				104 메뉴 항목 HELP
> 		105 창 Splitter ID: 302431488
> 		106 창 Splitter ID: 302437704
> 		107 Tab Output ID: 297478928
> 			108 창 ID: 1200
> 				109 창 ID: 1200
> 					110 LIST ID: 1201
> 						111 목록 항목 (selectable)
> 					112 스크롤 막대 (disabled) ID: 59904
> 						113 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						114 위치 조정 위치 ID: ScrollbarThumb
> 						115 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			116 탭 항목 (selectable) Python Script
> 			117 탭 항목 (selectable) Output
> 			118 탭 항목 (selectable) Debugger
> 			119 단추 Close
> 		120 창 Splitter ID: 302436696
> 		121 Tab Class View ID: 297479384
> 			122 트리 (disabled) ID: 103
> 			123 탭 항목 (selectable) Lib
> 			124 탭 항목 (selectable) File
> 			125 탭 항목 (selectable) Global
> 			126 탭 항목 (selectable) Net
> 			127 탭 항목 (selectable) Class
> 			128 단추 Close
> 		129 Tab Properties ID: 302395640
> 			130 창 (disabled) ID: 291858232
> 				131 TABLE Properties Window ID: 297584112
> 				132 도구 모음 ID: 59392
> 					133 단추
> 					134 단추
> 			135 탭 항목 (selectable) Properties
> 			136 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
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
>   "code": "await new Promise(r=>setTimeout(r,5000)); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "LASAL 프로젝트 로드 완료 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001)", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 		2 상태 표시줄 ID: 59393
> 			3 진행률 표시줄 ID: 207928296
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
> 			16 도구 모음 Script
> 			17 도구 모음 Edit
> 				18 단추 (disabled) Toggle bookmark
> 				19 단추 (disabled) Previous bookmark
> 				20 단추 (disabled) Next bookmark
> 				21 단추 (disabled) Delete all bookmarks
> 				22 단추 (disabled) Previous bookmark in this file
> 				23 단추 (disabled) Next bookmark in this file
> 				24 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				25 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				26 단추 (disabled) Unindent (Shift+Tab)
> 				27 단추 (disabled) Indent (Tab)
> 			28 도구 모음 Macros Manager
> 				29 메뉴 항목 Macros
> 			30 도구 모음 Layout Manager
> 				31 메뉴 항목 Layouts
> 			32 도구 모음 Toolbox
> 				33 단추 DataAnalyzer
> 				34 단추 Interpreter
> 				35 단추 DiasDrive
> 				36 단추 PLC Diagnosis
> 				37 단추 (disabled) Hardware Editor
> 				38 단추 (disabled) Graphical Hardware Editor
> 				39 단추 (disabled) Connection Manager
> 				40 단추 (disabled) Task Configuration
> 			41 도구 모음 Net Edit
> 				42 단추 (disabled) Select
> 				43 단추 (disabled) Move view
> 				44 단추 (disabled) Insert comment
> 				45 단추 (disabled) Zoom(+/-)
> 				46 단추 (disabled) Zoom to all
> 				47 단추 (disabled) Zoom selection
> 			48 도구 모음 Debug
> 				49 단추 (disabled) Go online (Alt+F6)
> 				50 단추 (disabled) Change Online Settings
> 				51 메뉴 항목 (disabled) Online Connection
> 				52 단추 (disabled) Set Online Connection For Current Project
> 				53 단추 (disabled) Download (F6)
> 				54 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)

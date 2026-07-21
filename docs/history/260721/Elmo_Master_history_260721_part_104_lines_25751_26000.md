>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
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
>   "code": "await new Promise(r=>setTimeout(r,3000)); globalThis.windows=await sky.list_windows(); nodeRepl.write(JSON.stringify(windows.filter(w=>String(w.app).includes('Lasal2')||String(w.title).includes('Lasal Class')),null,2));",
>   "title": "LASAL 창 준비 대기"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 134378,
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
>   "code": "globalThis.returnedWindow=windows.filter(w=>w.id===134378)[0]; if(!returnedWindow) throw new Error('window missing'); globalThis.targetWindow=await sky.get_window({id:returnedWindow.id,app:returnedWindow.app}); await sky.activate_window({window:targetWindow}); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "LASAL 초기 화면 확인"
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
> 				31 단추 Interpreter
> 				32 단추 DiasDrive
> 				33 단추 PLC Diagnosis
> 				34 단추 (disabled) Hardware Editor
> 				35 단추 (disabled) Graphical Hardware Editor
> 				36 단추 (disabled) Connection Manager
> 				37 단추 (disabled) Task Configuration
> 			38 도구 모음 Net Edit
> 				39 단추 (disabled) Select
> 				40 단추 (disabled) Move view
> 				41 단추 (disabled) Insert comment
> 				42 단추 (disabled) Zoom(+/-)
> 				43 단추 (disabled) Zoom to all
> 				44 단추 (disabled) Zoom selection
> 			45 도구 모음 Debug
> 				46 단추 Go online (Alt+F6)
> 				47 단추 Change Online Settings
> 				48 메뉴 항목 Online Connection
> 				49 단추 (disabled) Set Online Connection For Current Project
> 				50 단추 (disabled) Download (F6)
> 				51 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				52 단추 (disabled) Download Module on the Fly
> 				53 단추 (disabled) Save Project on PLC
> 				54 단추 (disabled) Start (F7)
> 				55 단추 (disabled) Reset (F8)
> 				56 단추 (disabled) Toggle breakpoint (F4)
> 				57 단추 (disabled) Create condition breakpoint
> 				58 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				59 단추 AWL trigger on/off
> 				60 단추 (disabled) Fix AWL trigger to current instruction
> 				61 단추 Activate/Deactivate Live View
> 				62 단추 Activate/Deactivate Auto Watch
> 				63 단추 (disabled) Goto instruction pointer
> 				64 단추 (disabled) Step into (F5)
> 				65 단추 (disabled) Step over (Alt+F5)
> 				66 단추 (disabled) Step out (Shift+F5)
> 				67 단추 (disabled) Set instruction pointer
> 			68 도구 모음 Build
> 				69 메뉴 항목 Target Architecture
> 				70 단추 (disabled) Build changes (F9)
> 				71 단추 (disabled) Rebuild project (Strg+F9)
> 				72 단추 (disabled) Cancel building (Ctrl+Break)
> 				73 단추 (disabled) Link project
> 			74 도구 모음 Standard
> 				75 단추 New project (Strg+N)
> 				76 단추 Open a file (Strg+Shift+O)
> 				77 단추 (disabled) Close active document (Strg+F4)
> 				78 단추 (disabled) Save file (Strg+S)
> 				79 단추 Open project (Strg+O)
> 				80 단추 (disabled) Save project changes (Strg+Shift+S)
> 				81 단추 (disabled) Close project
> 				82 단추 (disabled) Print
> 				83 단추 Cut (Strg+X)
> 				84 단추 Copy (Strg+C)
> 				85 단추 Paste (Strg+V)
> 				86 메뉴 항목 (disabled) Undo (Strg+Z)
> 				87 메뉴 항목 (disabled) Redo (Strg+Y)
> 				88 단추 (disabled) Navigate Backward (Alt+Left)
> 				89 단추 (disabled) Navigate Forward (Alt +Right)
> 			90 메뉴 모음 Menu Bar
> 				91 메뉴 항목 FILE
> 				92 메뉴 항목 EDIT
> 				93 메뉴 항목 VIEW
> 				94 메뉴 항목 PROJECT
> 				95 메뉴 항목 BUILD
> 				96 메뉴 항목 DEBUG
> 				97 메뉴 항목 ANALYZE
> 				98 메뉴 항목 TOOLS
> 				99 메뉴 항목 EXTRAS
> 				100 메뉴 항목 WINDOW
> 				101 메뉴 항목 HELP
> 		102 창 Splitter ID: 302431488
> 		103 창 Splitter ID: 302437704
> 		104 Tab Output ID: 297478928
> 			105 창 ID: 1200
> 				106 창 ID: 1200
> 					107 LIST ID: 1201
> 					108 스크롤 막대 (disabled) ID: 59904
> 						109 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						110 위치 조정 위치 ID: ScrollbarThumb
> 						111 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			112 탭 항목 (selectable) Python Script
> 			113 탭 항목 (selectable) Output
> 			114 탭 항목 (selectable) Debugger
> 			115 단추 Close
> 		116 창 Splitter ID: 302436696
> 		117 Tab Class View ID: 297479384
> 			118 트리 ID: 103
> 			119 탭 항목 (selectable) Lib
> 			120 탭 항목 (selectable) File
> 			121 탭 항목 (selectable) Global
> 			122 탭 항목 (selectable) Net
> 			123 탭 항목 (selectable) Class
> 			124 단추 Close
> 		125 Tab Properties ID: 302395640
> 			126 창 ID: 291858232
> 				127 TABLE Properties Window ID: 297584112
> 				128 도구 모음 ID: 59392
> 					129 단추
> 					130 단추
> 			131 탭 항목 (selectable) Properties
> 			132 단추 Close
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
>   "code": "var obs=globalThis.state; globalThis.state=null; await sky.click({window:obs.window,element_index:79}); globalThis.windows=await sky.list_windows(); nodeRepl.write(JSON.stringify(windows.filter(w=>String(w.app).includes('Lasal2')||String(w.title).includes('Open')).map(w=>({id:w.id,app:w.app,title:w.title})),null,2));",
>   "title": "LASAL 프로젝트 열기"
> }
> ```
>
> ```text
> [
>   {
>     "id": 134378,
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "프로젝트 열기 대화상자 확인"
> }
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||'')+'\\n'+String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "프로젝트 파일 선택 포커스 확인"
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); globalThis.targetWindow=state.window; nodeRepl.write('project file dialog screenshot ready');",
>   "title": "프로젝트 파일 선택"
> }
> ```
>
> ```text

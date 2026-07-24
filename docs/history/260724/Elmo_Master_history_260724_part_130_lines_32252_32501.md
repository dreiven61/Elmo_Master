>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-06-23",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-06-23",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\LARSConfigTool.exe",
>     "windows": [],
>     "displayName": "LARSConfigTool",
>     "lastUsedDate": "2026-06-11",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\Lars.exe",
>     "windows": [],
>     "displayName": "Lars",
>     "lastUsedDate": "2026-05-27",
>     "useCount": 0,
>     "isRunning": false
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
>   "code": "globalThis.cuaLasalApp26=cuaApps26.filter(a=>a.displayName==='LASAL Class 2'&&!/Lasal_0094/i.test(a.id))[0]; await sky.launch_app({app:cuaLasalApp26.id}); globalThis.cuaApps26=await sky.list_apps(); globalThis.cuaLasalApp26=cuaApps26.find(a=>a.id===cuaLasalApp26.id); nodeRepl.write(JSON.stringify(cuaLasalApp26,null,2))",
>   "title": "LASAL Class 2 실행"
> }
> ```
>
> ```text
> {
>   "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>   "windows": [],
>   "displayName": "LASAL Class 2",
>   "lastUsedDate": "2026-07-23",
>   "useCount": 15,
>   "isRunning": false
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.cuaWins26=await sky.list_windows(); nodeRepl.write(JSON.stringify(cuaWins26.filter(w=>/Lasal2\\.exe$/i.test(w.app)||/Lasal Class 2/i.test(w.title||'')),null,2))",
>   "title": "LASAL 창 확인"
> }
> ```
>
> ```text
> []
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.launch_app({app:'C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe'}); globalThis.cuaWins26=await sky.list_windows(); nodeRepl.write(JSON.stringify(cuaWins26.filter(w=>/Lasal2\\.exe$/i.test(w.app)||/Lasal Class 2/i.test(w.title||'')),null,2))",
>   "title": "LASAL 실행 재시도"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 11081092,
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
>   "code": "globalThis.cuaLasalCandidates26=cuaWins26.filter(w=>/Lasal2\\.exe$/i.test(w.app)||/Lasal Class 2/i.test(w.title||'')); if(cuaLasalCandidates26.length!==1) throw new Error('Expected one LASAL window, found '+cuaLasalCandidates26.length); globalThis.cuaLasalWindow26=await sky.get_window({id:cuaLasalCandidates26[0].id,app:cuaLasalCandidates26[0].app}); await sky.activate_window({window:cuaLasalWindow26}); globalThis.cuaLasalState26=await sky.get_window_state({window:cuaLasalWindow26,include_screenshot:true,include_text:true}); globalThis.cuaLasalWindow26=cuaLasalState26.window; nodeRepl.write(String(cuaLasalState26.accessibility?.tree||''));",
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
> 		82 창 Splitter ID: 126148032
> 		83 창 Splitter ID: 126146688
> 		84 Tab Output ID: 129283072
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
> 		96 창 Splitter ID: 126148200
> 		97 Tab Class View ID: 129287632
> 			98 트리 ID: 103
> 			99 탭 항목 (selectable) Lib
> 			100 탭 항목 (selectable) File
> 			101 탭 항목 (selectable) Global
> 			102 탭 항목 (selectable) Net
> 			103 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 129288544
> 			106 창 ID: 120748032
> 				107 TABLE Properties Window ID: 125710488
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			111 탭 항목 (selectable) Properties
> 			112 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
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
>   "code": "{ const observation=cuaLasalState26; globalThis.cuaLasalState26=null; await sky.click({window:observation.window,element_index:59}); globalThis.cuaLasalState26=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.cuaLasalWindow26=cuaLasalState26.window; nodeRepl.write(String(cuaLasalState26.accessibility?.tree||'')); }",

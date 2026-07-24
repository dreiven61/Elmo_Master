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
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Freeplane\\freeplane.exe",
>     "windows": [],
>     "displayName": "Freeplane",
>     "lastUsedDate": "2026-06-09",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\CANopen Architect Mini\\CANopenArchitectMini.exe",
>     "windows": [],
>     "displayName": "CANopen Architect Mini",
>     "lastUsedDate": "2026-06-04",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "WelconServoStudio.AppID.NoVersion",
>     "windows": [],
>     "displayName": "WELSS.exe",
>     "lastUsedDate": "2026-06-04",
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
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Microsoft Office\\Office16\\POWERPNT.EXE",
>     "windows": [],
>     "displayName": "PowerPoint 2016",
>     "lastUsedDate": "2026-05-13",
>     "useCount": 4,
>     "isRunning": false
>   },
>   {
>     "id": "AcrobatReader",
>     "windows": [],
>     "displayName": "Adobe Acrobat",
>     "lastUsedDate": "2026-05-06",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Advanced IP Scanner\\advanced_ip_scanner.exe",
>     "windows": [],
>     "displayName": "Advanced IP Scanner",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe",
>     "windows": [],
>     "displayName": "Qt Creator 18.0.1 (Community)",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "windows": [],
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "windows": [],
>     "displayName": "MDS Installation Guide",
>     "lastUsedDate": "2026-04-03",
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
>   "code": "globalThis.targetApp = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\nif (!targetApp) throw new Error(\"LASAL app not returned\");\nawait sky.launch_app({app: targetApp.id});\nfor (var lasalAttempt=0; lasalAttempt<15; lasalAttempt++) {\n  await new Promise(r=>setTimeout(r,1000));\n  globalThis.apps=await sky.list_apps();\n  globalThis.targetApp=apps.find(a=>a.id===\"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\n  if(targetApp?.windows?.length) break;\n}\nnodeRepl.write(JSON.stringify(targetApp,null,2));",
>   "title": "LASAL Class 2 실행",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>   "windows": [
>     {
>       "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>       "id": 201694,
>       "title": "Lasal Class 2 (02.03.001)"
>     }
>   ],
>   "displayName": "LASAL Class 2",
>   "lastUsedDate": "2026-07-21",
>   "useCount": 5,
>   "isRunning": true
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "if(targetApp.windows.length!==1) throw new Error(\"Expected one LASAL window\");\nglobalThis.targetWindow=await sky.get_window(targetApp.windows[0]);\nawait sky.activate_window({window:targetWindow});\nglobalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});\nglobalThis.targetWindow=state.window;\nnodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||\"\").slice(0,5000));",
>   "title": "LASAL 시작 화면 확인",
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
>   "code": "{ const observation=globalThis.state; globalThis.state=null; await sky.press_key({window:observation.window,key:\"Control_L+o\"}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||\"\").slice(0,4000)); }",
>   "title": "LASAL 프로젝트 열기",
>   "timeout_ms": 30000
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
> 				30 단추 Interpreter
> 				31 단추 DiasDrive
> 				32 단추 PLC Diagnosis
> 				33 단추 (disabled) Hardware Editor
> 				34 단추 (disabled) Graphical Hardware Editor
> 				35 단추 (disabled) Connection Manager
> 				36 단추 (disabled) Task Configuration
> 			37 도구 모음 Net Edit
> 				38 단추 (disabled) Select
> 				39 단추 (disabled) Move view
> 				40 단추 (disabled) Insert comment
> 				41 단추 (disabled) Zoom(+/-)
> 				42 단추 (disabled) Zoom to all
> 				43 단추 (disabled) Zoom selection
> 			44 도구 모음 Debug
> 				45 단추 Go online (Alt+F6)
> 				46 단추 Change Online Settings
> 				47 메뉴 항목 Online Connection
> 				48 단추 (disabled) Set Online Connection For Current Project
> 				49 단추 (disabled) Download (F6)
> 				50 단추 (disabled) Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				51 단추 (disabled) Download Module on the Fly
> 				52 단추 (disabled) Save Project on PLC
> 				53 단추 (disabled) Start (F7)
> 				54 단추 (disabled) Reset (F8)
> 				55 단추 (disabled) Toggle breakpoint (F4)
> 				56 단추 (disabled) Create condition breakpoint
> 				57 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				58 단추 AWL trigger on/off
> 				59 단추 (disabled) Fix AWL trigger to current instruction
> 				60 단추 Activate/Deactivate Live View
> 				61 단추 Activate/Deactivate Auto Watch
> 				62 단추 (disabled) Goto instruction pointer
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

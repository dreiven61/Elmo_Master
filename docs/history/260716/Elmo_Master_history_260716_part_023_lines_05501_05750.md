>       {
>         "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\GitExtensions\\GitExtensions.exe",
>         "id": 199114,
>         "title": "Elmo_Master (main) - Git Extensions"
>       }
>     ],
>     "displayName": "Git Extensions",
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>         "id": 20778988,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-10",
>     "useCount": 28,
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>         "id": 20778988,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-10",
>     "useCount": 28,
>     "isRunning": true
>   },
>   {
>     "id": "VisualStudio.89931261",
>     "windows": [
>       {
>         "app": "VisualStudio.89931261",
>         "id": 197574,
>         "title": "LasalMotionControlLibTestApp - Microsoft Visual Studio"
>       }
>     ],
>     "displayName": "Visual Studio 2019",
>     "isRunning": true
>   },
>   {
>     "id": "Microsoft.VisualStudioCode",
>     "windows": [
>       {
>         "app": "Microsoft.VisualStudioCode",
>         "id": 395020,
>         "title": "미리 보기 Retiring-dotnet67-sdk-support.md - Elmo_Master - Visual Studio Code"
>       }
>     ],
>     "displayName": "Visual Studio Code",
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe",
>     "windows": [],
>     "displayName": "LASAL Safety Designer",
>     "lastUsedDate": "2026-07-07",
>     "useCount": 2,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 1,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 1,
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
>   "code": "globalThis.targetApp = apps.find(a => /\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2\\.exe$/i.test(a.id));\nif (!targetApp || targetApp.windows.length !== 1) {\n  nodeRepl.write(JSON.stringify(targetApp || apps.filter(a => /Lasal2\\.exe/i.test(a.id)), null, 2));\n  throw new Error(\"Expected one active LASAL Class 2 window\");\n}\nglobalThis.targetWindow = await sky.get_window(targetApp.windows[0]);\nawait sky.activate_window({window: targetWindow});\nglobalThis.targetWindow = await sky.get_window({id: targetWindow.id, app: targetWindow.app});\nglobalThis.state = await sky.get_window_state({window: targetWindow});\nglobalThis.targetWindow = state.window;",
>   "title": "LASAL IDE 상태 확인",
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
>   "code": "await sky.click({window: targetWindow, x: 308, y: 40});\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: true, include_text: true});\nglobalThis.targetWindow = state.window;\n{\n  var treeText1 = state.accessibility?.tree || \"\";\n  nodeRepl.write(treeText1.split(\"\\n\").filter(line => /Build|Rebuild|Compile|Link|Project/i.test(line)).slice(0,80).join(\"\\n\"));\n}",
>   "title": "LASAL Build 메뉴 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 				80 단추 (disabled) Set Online Connection For Current Project
> 				82 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				84 단추 (disabled) Save Project on PLC
> 			90 도구 모음 Build
> 				92 단추 Build changes (F9)
> 				93 단추 Rebuild project (Strg+F9)
> 				94 단추 (disabled) Cancel building (Ctrl+Break)
> 				95 단추 Link project
> 				97 단추 New project (Strg+N)
> 				101 단추 Open project (Strg+O)
> 				102 단추 (disabled) Save project changes (Strg+Shift+S)
> 				103 단추 Close project
> 				117 메뉴 항목 PROJECT
> 				118 메뉴 항목 BUILD
> 					119 메뉴 Build
> 						120 메뉴 항목 Build F9
> 						121 메뉴 항목 Rebuild All Ctrl+F9
> 						122 메뉴 항목 Build Connection List
> 						123 메뉴 항목 (disabled) Build Drive Tables
> 						125 메뉴 항목 (disabled) Compile Module Alt+F9
> 						127 메뉴 항목 Link
> 		181 메뉴 Build
> 			182 메뉴 항목 Build F9
> 			183 메뉴 항목 Rebuild All Ctrl+F9
> 			184 메뉴 항목 Build Connection List
> 			185 메뉴 항목 (disabled) Build Drive Tables
> 			187 메뉴 항목 (disabled) Compile Module Alt+F9
> 			189 메뉴 항목 Link
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window: targetWindow, element_index: 121});\nawait new Promise(resolve => setTimeout(resolve, 30000));\nglobalThis.state = await sky.get_window_state({window: targetWindow});\nglobalThis.targetWindow = state.window;",
>   "title": "LASAL 프로젝트 Rebuild",
>   "timeout_ms": 45000
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
>   "code": "await sky.click({window: targetWindow, x: 183, y: 143});\nglobalThis.state = await sky.get_window_state({window: targetWindow});\nglobalThis.targetWindow = state.window;",
>   "title": "TCPMotionInterface 구현 열기",
>   "timeout_ms": 30000
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
>   "code": "await sky.click({window: targetWindow, x: 405, y: 255, mouse_button:\"right\"});\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true});\nglobalThis.targetWindow = state.window;\n{\n  var t2 = state.accessibility?.tree || \"\";\n  nodeRepl.write(t2.split(\"\\n\").filter(line => /Find|Implementation|Definition|Reference/i.test(line)).slice(0,80).join(\"\\n\"));\n}",
>   "title": "LASAL 구현 검색 메뉴 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> point (405, 255) is outside window bounds { originX: 0, originY: 0, width: 175, height: 25 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json

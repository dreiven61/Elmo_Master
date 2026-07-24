> {
>   "title": "Focus LASAL project path",
>   "code": "{\n  var obsDialog = globalThis.state;\n  var shotDialog = obsDialog.screenshots?.[0]?.id;\n  globalThis.state = null;\n  await sky.click({window:obsDialog.window, screenshotId:shotDialog, x:930, y:956});\n  globalThis.state = await sky.get_window_state({window:obsDialog.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
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
>   "title": "Choose LASAL project file",
>   "code": "{\n  var obsFile = globalThis.state;\n  var shotFile = obsFile.screenshots?.[0]?.id;\n  globalThis.state = null;\n  await sky.click({window:obsFile.window, screenshotId:shotFile, x:505, y:291});\n  globalThis.state = await sky.get_window_state({window:obsFile.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}"
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
>   "title": "Open LASAL project",
>   "code": "{\n  var obsEnter = globalThis.state;\n  globalThis.state = null;\n  await sky.press_key({window:obsEnter.window, key:\"ENTER\"});\n  await new Promise(resolve => setTimeout(resolve,4000));\n  globalThis.apps = await sky.list_apps();\n  var lasalApp2 = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\n  nodeRepl.write(JSON.stringify(lasalApp2?.windows, null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 6490100,
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
>   "title": "Verify LASAL project loaded",
>   "code": "globalThis.apps = await sky.list_apps();\nglobalThis.targetApp = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\nif (!targetApp || targetApp.windows.length !== 1) throw new Error(\"LASAL window selection is not unique\");\nglobalThis.targetWindow = await sky.get_window({id:targetApp.windows[0].id, app:targetApp.windows[0].app});\nawait sky.activate_window({window:targetWindow});\nglobalThis.state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));"
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
>   "title": "Enter LASAL project filename",
>   "code": "{\n  var obsName = globalThis.state;\n  var shotName = obsName.screenshots?.[0]?.id;\n  globalThis.state = null;\n  await sky.click({window:obsName.window, screenshotId:shotName, x:820, y:963});\n  globalThis.state = await sky.get_window_state({window:obsName.window, include_screenshot:false, include_text:true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || state.accessibility?.tree || \"\"));\n}"
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
>   "title": "Confirm filename focus",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element, text:state.accessibility?.document_text, tree:state.accessibility?.tree}, null, 2));"
> }
> ```
>
> ```text
> {}
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
>   "title": "Type LASAL project filename",
>   "code": "{\n  var obsTypeName = globalThis.state;\n  globalThis.state = null;\n  await sky.type_text({window:obsTypeName.window, text:\"Elmo_EtherCAT_Test_4Axis.lcp\"});\n  globalThis.state = await sky.get_window_state({window:obsTypeName.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(\"filename entered\");\n}"
> }
> ```
>
> ```text
> filename entered
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
>   "title": "Load LASAL project",
>   "code": "{\n  var obsLoad = globalThis.state;\n  globalThis.state = null;\n  await sky.press_key({window:obsLoad.window, key:\"ENTER\"});\n  await new Promise(resolve => setTimeout(resolve,8000));\n  globalThis.apps = await sky.list_apps();\n  var appLoad = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\n  nodeRepl.write(JSON.stringify(appLoad?.windows,null,2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 6490100,
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
>   "title": "Inspect loaded LASAL project",
>   "code": "globalThis.apps = await sky.list_apps();\nglobalThis.targetApp = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\nglobalThis.targetWindow = await sky.get_window({id:targetApp.windows[0].id, app:targetApp.windows[0].app});\nawait sky.activate_window({window:targetWindow});\nglobalThis.state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));"
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
> 				45 단추 (disabled) Go online (Alt+F6)
> 				46 단추 (disabled) Change Online Settings
> 				47 메뉴 항목 (disabled) Online Connection
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
> 				68 메뉴 항목 (disabled) Target Architecture
> 				69 단추 (disabled) Build changes (F9)
> 				70 단추 (disabled) Rebuild project (Strg+F9)
> 				71 단추 Cancel building (Ctrl+Break)
> 				72 단추 (disabled) Link project
> 			73 도구 모음 Standard
> 				74 단추 (disabled) New project (Strg+N)
> 				75 단추 Open a file (Strg+Shift+O)
> 				76 단추 (disabled) Close active document (Strg+F4)
> 				77 단추 (disabled) Save file (Strg+S)
> 				78 단추 (disabled) Open project (Strg+O)
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
> 		101 창 Splitter ID: 355145208

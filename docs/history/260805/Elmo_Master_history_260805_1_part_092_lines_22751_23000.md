> - **[3.3]** Submit age verification
> - **[5.1]** Accept third-party “are you sure?” warnings
> - **[6]** Upload files
> - **[12]** File management via a browser action
>   - local move/rename
>   - cloud move/rename within same cloud
> - **[14]** Transmit sensitive data
>   - pre-approval must clearly mention **specific data** + **specific destination**; otherwise confirm.
>
> #### 4) No Confirmation Needed (Always Allowed)
>
> - **[3.1, 3.2]** Cookie consent UIs + accepting ToS/Privacy Policy (during account creation)
> - **[7]** Download files from the Internet (inbound transfer)
> - Any action outside this taxonomy
> - Any non-UI action that does not alter the state of an app.
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 실행",
>   "code": "await sky.launch_app({app:'C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe'}); globalThis.apps = await sky.list_apps(); nodeRepl.write(JSON.stringify(apps.filter(a => /Lasal|Sigmatek/i.test((a.id||'')+' '+(a.name||'')+' '+JSON.stringify(a.windows||[]))), null, 2))",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>         "id": 10488792,
>         "title": "Lasal Class 2 (02.03.001)"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-31",
>     "useCount": 3,
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>         "id": 10488792,
>         "title": "Lasal Class 2 (02.03.001)"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-31",
>     "useCount": 3,
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-07-30",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-07-30",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\Lars.exe",
>     "windows": [],
>     "displayName": "Lars",
>     "lastUsedDate": "2026-07-29",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe",
>     "windows": [],
>     "displayName": "LASAL Safety Designer",
>     "lastUsedDate": "2026-07-27",
>     "useCount": 0,
>     "isRunning": false
>   },
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
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lars\\LARSConfigTool.exe",
>     "windows": [],
>     "displayName": "LARSConfigTool",
>     "lastUsedDate": "2026-06-11",
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
>   "title": "LASAL 시작 화면 확인",
>   "code": "globalThis.lasalWindows = (await sky.list_windows()).filter(w => w.id === 10488792 && /^Lasal Class 2 \\(02\\.03\\.001\\)$/i.test(w.title||'')); if(lasalWindows.length!==1){nodeRepl.write(JSON.stringify(lasalWindows,null,2)); throw new Error('Expected exactly one LASAL window')}; globalThis.targetWindow = await sky.get_window({id:lasalWindows[0].id, app:lasalWindows[0].app}); await sky.activate_window({window:targetWindow}); globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))"
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
> 				1157 단추 (disabled) Toggle bookmark
> 				1158 단추 (disabled) Previous bookmark
> 				1159 단추 (disabled) Next bookmark
> 				1160 단추 (disabled) Delete all bookmarks
> 				1161 단추 (disabled) Previous bookmark in this file
> 				1162 단추 (disabled) Next bookmark in this file
> 				1163 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				1164 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				1165 단추 (disabled) Unindent (Shift+Tab)
> 				1166 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				1167 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				1168 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				1169 단추 DataAnalyzer
> 				1170 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				1171 단추 (disabled) Select
> 				1172 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				1173 단추 Go online (Alt+F6)
> 				1174 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				1175 메뉴 항목 Target Architecture
> 				1176 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				1177 단추 New project (Strg+N)
> 				1178 단추 Open a file (Strg+Shift+O)
> 				1179 단추 (disabled) Close active document (Strg+F4)
> 				1180 단추 (disabled) Save file (Strg+S)
> 				1181 단추 Open project (Strg+O)
> 				1182 단추 (disabled) Save project changes (Strg+Shift+S)
> 				1183 단추 (disabled) Close project
> 				1184 단추 (disabled) Print
> 				1185 단추 Cut (Strg+X)
> 				1186 단추 Copy (Strg+C)
> 				1187 단추 Paste (Strg+V)
> 				1188 메뉴 항목 (disabled) Undo (Strg+Z)
> 				1189 메뉴 항목 (disabled) Redo (Strg+Y)
> 				1190 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				1191 메뉴 항목 FILE
> 				1192 메뉴 항목 EDIT
> 				1193 메뉴 항목 VIEW
> 				1194 메뉴 항목 PROJECT
> 				1195 메뉴 항목 BUILD
> 				1196 메뉴 항목 DEBUG
> 				1197 메뉴 항목 ANALYZE
> 				1198 메뉴 항목 TOOLS
> 				1199 메뉴 항목 EXTRAS
> 				1200 메뉴 항목 WINDOW
> 				1201 메뉴 항목 HELP
> 		67 창 Splitter ID: 384981624
> 		68 창 Splitter ID: 384977088
> 		69 Tab Output ID: 274935144
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 					73 스크롤 막대 (disabled) ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			1202 탭 항목 (selectable) Python Script
> 			1203 탭 항목 (selectable) Debugger
> 			1204 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 384981960
> 		82 Tab Class View ID: 274930128
> 			83 트리 ID: 103
> 			1205 탭 항목 (selectable) Lib
> 			1206 탭 항목 (selectable) File
> 			1207 탭 항목 (selectable) Class
> 			1208 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274930584
> 			90 창 ID: 266310816
> 				91 TABLE Properties Window ID: 272695160
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			1209 탭 항목 (selectable) Properties
> 			96 단추 Close
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
>   "title": "Canonical 프로젝트 열기",
>   "code": "{const observation=globalThis.state;if(!observation?.accessibility)throw new Error('No accessibility observation');globalThis.state=null;await sky.click({window:observation.window,element_index:1181});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))}"
> }
> ```
>
> ```text
> element 1181 is not available in cached app state for Lasal2.exe
> ```

>     '- **[3.1, 3.2]** Cookie consent UIs + accepting ToS/Privacy Policy (during account creation)\r\n' +
>     '- **[7]** Download files from the Internet (inbound transfer)\r\n' +
>     '- Any action outside this taxonomy\r\n' +
>     '- Any non-UI action that does not alter the state of an app.\n'
> }
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "선택된 LASAL 프로젝트 확인",
>   "code": "globalThis.explorerState2 = await sky.get_window_state({window: projectExplorerWindow, include_screenshot:true, include_text:false}); globalThis.projectExplorerWindow = explorerState2.window; nodeRepl.write(JSON.stringify({window:projectExplorerWindow,screenshots:explorerState2.screenshots.map(s=>({id:s.id,width:s.width,height:s.height}))}))"
> }
> ```
>
> ```text
> {"window":{"app":"process:C:\\Windows\\explorer.exe","id":721818,"title":"Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"},"screenshots":[{"id":"screenshot-0","width":1920,"height":1080},{"id":"screenshot-1","width":1904,"height":1023}]}
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
>   "title": "정식 LASAL 프로젝트 열기",
>   "code": "{ const observation = globalThis.explorerState2; globalThis.explorerState2 = null; await sky.press_key({window:observation.window,key:'Return'}); await new Promise(r=>setTimeout(r,5000)); globalThis.windowsAfterLasalLaunch = await sky.list_windows(); nodeRepl.write(JSON.stringify(windowsAfterLasalLaunch,null,2)); }",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 721818,
>     "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
>   },
>   {
>     "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>     "id": 132684,
>     "title": "ChatGPT"
>   },
>   {
>     "app": "Chrome",
>     "id": 133230,
>     "title": "ChatGPT Plugins | Browse and add plugins to ChatGPT - Chrome"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 2887378,
>     "title": "architecture - 파일 탐색기 [ main ↓0 ↑17 ]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "id": 133028,
>     "title": "카카오톡"
>   }
> ]
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 시작 대기",
>   "code": "await new Promise(r=>setTimeout(r,10000)); globalThis.windowsAfterLasalWait = await sky.list_windows(); globalThis.appsAfterLasalWait = await sky.list_apps(); nodeRepl.write(JSON.stringify({windows:windowsAfterLasalWait, lasalApps:appsAfterLasalWait.filter(a=>/lasal/i.test((a.displayName||'')+' '+a.id)||a.windows.some(w=>/lasal|elmo_ethercat/i.test(w.title||''))},null,2))",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [1:364-1:365]: Expected ')'
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 시작 대기",
>   "code": "await new Promise(r=>setTimeout(r,10000)); globalThis.windowsAfterLasalWait = await sky.list_windows(); globalThis.appsAfterLasalWait = await sky.list_apps(); globalThis.lasalAppsAfterWait = appsAfterLasalWait.filter(a => /lasal/i.test((a.displayName||'')+' '+a.id) || a.windows.some(w => /lasal|elmo_ethercat/i.test(w.title||''))); nodeRepl.write(JSON.stringify({windows:windowsAfterLasalWait,lasalApps:lasalAppsAfterWait},null,2));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   "windows": [
>     {
>       "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>       "id": 3739584,
>       "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"
>     },
>     {
>       "app": "process:C:\\Windows\\explorer.exe",
>       "id": 721818,
>       "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
>     },
>     {
>       "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>       "id": 132684,
>       "title": "ChatGPT"
>     },
>     {
>       "app": "Chrome",
>       "id": 133230,
>       "title": "ChatGPT Plugins | Browse and add plugins to ChatGPT - Chrome"
>     },
>     {
>       "app": "process:C:\\Windows\\explorer.exe",
>       "id": 2887378,
>       "title": "architecture - 파일 탐색기 [ main ↓0 ↑17 ]"
>     },
>     {
>       "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>       "id": 133028,
>       "title": "카카오톡"
>     }
>   ],
>   "lasalApps": [
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>       "windows": [
>         {
>           "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>           "id": 3739584,
>           "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"
>         }
>       ],
>       "displayName": "LASAL Class 2",
>       "lastUsedDate": "2026-07-31",
>       "useCount": 3,
>       "isRunning": true
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>       "windows": [
>         {
>           "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>           "id": 3739584,
>           "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"
>         }
>       ],
>       "displayName": "LASAL Class 2",
>       "lastUsedDate": "2026-07-31",
>       "useCount": 3,
>       "isRunning": true
>     },
>     {
>       "id": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>       "windows": [
>         {
>           "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>           "id": 721818,
>           "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
>         },
>         {
>           "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>           "id": 2887378,
>           "title": "architecture - 파일 탐색기 [ main ↓0 ↑17 ]"
>         }
>       ],
>       "displayName": "OLAP용 Oracle Provider for OLE DB Readme",
>       "isRunning": true
>     },
>     {
>       "id": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>       "windows": [
>         {
>           "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>           "id": 721818,
>           "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
>         },
>         {
>           "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>           "id": 2887378,
>           "title": "architecture - 파일 탐색기 [ main ↓0 ↑17 ]"
>         }
>       ],
>       "displayName": "Oracle Provider for OLE DB Readme",
>       "isRunning": true
>     },
>     {
>       "id": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>       "windows": [
>         {
>           "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>           "id": 721818,
>           "title": "Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"
>         },
>         {
>           "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>           "id": 2887378,
>           "title": "architecture - 파일 탐색기 [ main ↓0 ↑17 ]"
>         }
>       ],
>       "displayName": "Windows Software Development Kit",
>       "isRunning": true
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe",
>       "windows": [],
>       "displayName": "LASAL MachineManager",
>       "lastUsedDate": "2026-07-30",
>       "useCount": 0,
>       "isRunning": false
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe",
>       "windows": [],
>       "displayName": "LASAL MachineManager",
>       "lastUsedDate": "2026-07-30",
>       "useCount": 0,
>       "isRunning": false
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe",
>       "windows": [],
>       "displayName": "LASAL Safety Designer",
>       "lastUsedDate": "2026-07-27",
>       "useCount": 0,
>       "isRunning": false
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe",
>       "windows": [],
>       "displayName": "LASAL Screen Modeless",
>       "lastUsedDate": "2026-07-03",
>       "useCount": 0,
>       "isRunning": false
>     },
>     {
>       "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe",
>       "windows": [],
>       "displayName": "LASAL Screen Modeless",

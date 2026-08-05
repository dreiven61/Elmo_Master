>   "code": "({skyType: typeof sky, cwd: nodeRepl.cwd})"
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
>   "code": "nodeRepl.write({skyType: typeof sky, cwd: nodeRepl.cwd})",
>   "title": "LASAL 제어 세션 확인"
> }
> ```
>
> ```text
> { skyType: 'object', cwd: 'C:\\work\\Elmo\\Elmo_Master' }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(Object.keys(sky))",
>   "title": "LASAL 제어 API 확인"
> }
> ```
>
> ```text
> [
>   'target',
>   'activate_window',
>   'get_window_state',
>   'click',
>   'scroll',
>   'drag',
>   'press_key',
>   'type_text',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'get_window',
>   'perform_secondary_action',
>   'set_value',
>   'transport'
> ]
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
>   "code": "nodeRepl.write(await sky.list_windows())",
>   "title": "열린 창 확인"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 721818,
>     title: 'Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]'
>   },
>   {
>     app: 'OpenAI.Codex_2p2nqsd0c76g0!App',
>     id: 132684,
>     title: 'ChatGPT'
>   },
>   {
>     app: 'Chrome',
>     id: 133230,
>     title: 'ChatGPT Plugins | Browse and add plugins to ChatGPT - Chrome'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 2887378,
>     title: 'architecture - 파일 탐색기 [ main ↓0 ↑17 ]'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 133028,
>     title: '카카오톡'
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
>   "code": "nodeRepl.write((await sky.list_apps()).filter(x=>JSON.stringify(x).toLowerCase().includes('lasal')))",
>   "title": "LASAL 앱 확인"
> }
> ```
>
> ```text
> [
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     windows: [],
>     displayName: 'LASAL Class 2',
>     lastUsedDate: '2026-07-31',
>     useCount: 3,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe',
>     windows: [],
>     displayName: 'LASAL Class 2',
>     lastUsedDate: '2026-07-31',
>     useCount: 3,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe',
>     windows: [],
>     displayName: 'LASAL MachineManager',
>     lastUsedDate: '2026-07-30',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe',
>     windows: [],
>     displayName: 'LASAL MachineManager',
>     lastUsedDate: '2026-07-30',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Safety Designer\\bin\\SafetyDesigner.exe',
>     windows: [],
>     displayName: 'LASAL Safety Designer',
>     lastUsedDate: '2026-07-27',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe',
>     windows: [],
>     displayName: 'LASAL Screen Modeless',
>     lastUsedDate: '2026-07-03',
>     useCount: 0,
>     isRunning: false
>   },
>   {
>     id: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe',
>     windows: [],
>     displayName: 'LASAL Screen Modeless',
>     lastUsedDate: '2026-07-03',
>     useCount: 0,
>     isRunning: false
>   }
> ]
> ```
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(sky.launch_app.toString())",
>   "title": "LASAL 실행 방식 확인"
> }
> ```
>
> ```text
> (...args) => {
>         const app = getComputerUseAppReference(args[0]);
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app,
>           },
>           ...(isChromeComputerUseAppReference(app)
>             ? { [CHROME_COMPUTER_USE_META_KEY]: true }
>             : {}),
>         });
>         return Reflect.apply(value, target, args);
>       }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(Object.keys(sky.transport))",
>   "title": "LASAL 제어 인터페이스 확인"
> }
> ```
>
> ```text
> [ 'nextRequestId', 'pendingData', 'pendingRequests', 'socket' ]
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
>   "code": "var ws=await sky.get_window_state(721818); nodeRepl.write(ws)",
>   "title": "프로젝트 창 확인"
> }
> ```
>
> ```text
> window.app must be a non-empty string and window.id must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var wproj={app:'process:C:\\\\Windows\\\\explorer.exe',id:721818}; var ws2=await sky.get_window_state(wproj); nodeRepl.write(ws2)",
>   "title": "프로젝트 창 확인"
> }
> ```
>
> ```text
> window.app must be a non-empty string and window.id must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json

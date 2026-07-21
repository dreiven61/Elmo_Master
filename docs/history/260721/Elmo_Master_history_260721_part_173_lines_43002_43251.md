>
> ```text
> {
>   skyKeys: [
>     'activate_window',
>     'get_window_state',
>     'click',
>     'scroll',
>     'drag',
>     'press_key',
>     'type_text',
>     'launch_app',
>     'list_apps',
>     'list_windows',
>     'get_window',
>     'perform_secondary_action',
>     'set_value',
>     'transport'
>   ],
>   proto: [ 'constructor', 'documentation', 'close' ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 창 찾기",
>   "code": "var winsNow = await sky.list_windows(); nodeRepl.write(winsNow)"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   {
>     app: 'process:C:\\Program Files\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\Studio\\Common\\ctrlX-PLC-Engineering.exe',
>     id: 595394,
>     title: 'C:\\Users\\dreiv\\source\\repos\\BOSCH\\CtrlX\\PLC Engineering\\Gudel\\Gudel.project - ctrlX PLC Engineering'
>   },
>   {
>     app: 'process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.715.7063.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe',
>     id: 132772,
>     title: 'ChatGPT'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 132936,
>     title: '카카오톡'
>   },
>   {
>     app: 'process:C:\\Program Files\\ctrlX WORKS\\ctrlX PLC Engineering\\3.6.3\\LogViewer.exe',
>     id: 267760,
>     title: 'LogViewer (ctrlxworks.Studio.Launcher.txt)'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 524684,
>     title: 'GUDEL - 파일 탐색기 [ ssh_dev ↓0 ↑3 ]'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 8916318,
>     title: '서보스타 전직원 단톡방'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1837062,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   {
>     app: 'process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
>     id: 396542,
>     title: 'ChatGPT - Chrome'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68064,
>     title: 'MMCLibDotNET_Test - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68068,
>     title: 'PmasApiWpfTestApp - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68062,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio'
>   },
>   {
>     app: 'process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE',
>     id: 67542,
>     title: 'EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
>   },
>   {
>     app: 'process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE',
>     id: 67890,
>     title: 'Motion_Controller_API_정리.xlsx  [제한된 보기] [사용자가 마지막으로 저장함] - Excel'
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
>   "title": "LASAL IDE 조작 준비",
>   "code": "nodeRepl.write(await sky.documentation('activate_window'))"
> }
> ```
>
> ```text
> Unsupported Computer Use documentation
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 창 활성화 방식 확인",
>   "code": "nodeRepl.write(String(sky.activate_window))"
> }
> ```
>
> ```text
> (...args) => {
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app: getComputerUseAppReference(args[0]),
>           },
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
>   "title": "LASAL IDE 세션 상태 확인",
>   "code": "nodeRepl.write(Object.keys(globalThis).filter(k=>/sky|win|guid|state|api/i.test(k)).sort())"
> }
> ```
>
> ```text
> [
>   'sky',
>   'state',
>   'state2',
>   'targetWindow',
>   'targetWindow2',
>   'windowsNow'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 이전 UI 상태 확인",
>   "code": "nodeRepl.write({targetWindow,targetWindow2,stateKeys:state&&Object.keys(state),state2Keys:state2&&Object.keys(state2)})"
> }
> ```
>
> ```text
> {
>   targetWindow: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   targetWindow2: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
>   },
>   stateKeys: null,
>   state2Keys: [ 'window', 'screenshots', 'accessibility' ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 활성화",
>   "code": "var actLasal = await sky.activate_window(targetWindow); nodeRepl.write(actLasal)"
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
>   "title": "LASAL IDE 제어 매개변수 확인",
>   "code": "nodeRepl.write({transportKeys:Object.keys(sky.transport||{}),transportProto:Object.getOwnPropertyNames(Object.getPrototypeOf(sky.transport||{}))})"
> }
> ```
>
> ```text
> {
>   transportKeys: [ 'nextRequestId', 'pendingData', 'pendingRequests', 'socket' ],
>   transportProto: [
>     'constructor',
>     'close',
>     'request',
>     'sendMessage',
>     'handleData',
>     'handleMessage',
>     'handleApprovalRequest',
>     'rejectPendingRequests',
>     'writeMessage'
>   ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 활성화",
>   "code": "var actLasal2 = await sky.activate_window({window:targetWindow}); nodeRepl.write(actLasal2)"
> }
> ```

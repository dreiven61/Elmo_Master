>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 클래스 트리 읽기",
>   "code": "var lasalState3 = await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:lasalState3.window, screenshots:lasalState3.screenshots?.length, accessibilityCount:lasalState3.accessibility?.length, accessibility:lasalState3.accessibility})",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: 2,
>   accessibilityCount: undefined,
>   accessibility: null
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
>   "title": "LMCDiagnosticsService 열기",
>   "code": "var openClassResult = await sky.press_key({window:targetWindow, key:'ENTER'}); nodeRepl.write(openClassResult)"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 열린 상태 확인",
>   "code": "var lasalState4 = await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState4.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
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
>   "title": "의도치 않은 편집 되돌리기",
>   "code": "await sky.press_key({window:targetWindow, key:'CTRL+Z'}); nodeRepl.write('undo sent')"
> }
> ```
>
> ```text
> undo sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "편집 복구 확인",
>   "code": "var lasalState5=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState5.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
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
>   "title": "LMCDiagnosticsService 선택",
>   "code": "var clickClass=await sky.click({window:targetWindow, x:1058, y:325}); nodeRepl.write(clickClass)"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 열기",
>   "code": "await sky.press_key({window:targetWindow,key:'ENTER'}); nodeRepl.write('enter sent')"
> }
> ```
>
> ```text
> enter sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 편집기 전환 확인",
>   "code": "var lasalState6=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState6.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
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
>   "title": "Computer Use 클릭 사용법 확인",
>   "code": "nodeRepl.write(await sky.documentation())"
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
>   "title": "클릭 매개변수 확인",
>   "code": "try{await sky.click({})}catch(e){nodeRepl.write(String(e))}"
> }
> ```
>
> ```text
> TypeError: window.app must be a non-empty string and window.id must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 구현 열기",
>   "code": "var dblClass=await sky.click({window:targetWindow,x:1058,y:325,clickCount:2}); nodeRepl.write(dblClass)"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 구현 열기",
>   "code": "await sky.click({window:targetWindow,x:1058,y:325}); await new Promise(r=>setTimeout(r,120)); await sky.click({window:targetWindow,x:1058,y:325}); nodeRepl.write('double click sent')"
> }
> ```
>
> ```text
> double click sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 구현 확인",
>   "code": "var lasalState7=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState7.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
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

> ```json
> {
>   "title": "클래스 이름 편집 취소",
>   "code": "await sky.press_key({window:targetWindow,key:'ESC'}); nodeRepl.write('escape sent')"
> }
> ```
>
> ```text
> escape sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 메뉴 열기",
>   "code": "var ctxRes=await sky.perform_secondary_action({window:targetWindow,x:1058,y:325}); nodeRepl.write(ctxRes)"
> }
> ```
>
> ```text
> action is required
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 메뉴 열기",
>   "code": "var rightClickRes=await sky.click({window:targetWindow,x:1058,y:325,button:'right'}); nodeRepl.write(rightClickRes)"
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
>   "title": "클래스 컨텍스트 메뉴 확인",
>   "code": "var lasalState8=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState8.window)"
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
>   "title": "클래스 작업 메뉴 열기",
>   "code": "await sky.press_key({window:targetWindow,key:'ESC'}); await sky.click({window:targetWindow,x:1058,y:325}); await sky.press_key({window:targetWindow,key:'SHIFT+F10'}); nodeRepl.write('context key sent')"
> }
> ```
>
> ```text
> context key sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 작업 메뉴 확인",
>   "code": "var lasalState9=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState9.window)"
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
>   "title": "LMCDiagnosticsService 소스 열기",
>   "code": "await sky.press_key({window:targetWindow,key:'ENTER'}); nodeRepl.write('edit source selected')"
> }
> ```
>
> ```text
> edit source selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 소스 확인",
>   "code": "var lasalState10=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState10.window)"
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
>   "title": "LMCDiagnosticsService 소스 열기",
>   "code": "await sky.click({window:targetWindow,x:1044,y:348}); nodeRepl.write('clicked Edit Source')"
> }
> ```
>
> ```text
> clicked Edit Source
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCDiagnosticsService 로드 확인",
>   "code": "var lasalState11=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState11.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
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
>   "title": "LMCDiagnosticsService 저장",
>   "code": "await sky.press_key({window:targetWindow,key:'CTRL+S'}); nodeRepl.write('save sent')"
> }
> ```
>
> ```text
> save sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 메타데이터 저장 확인",
>   "code": "await new Promise(r=>setTimeout(r,1200)); var lasalState12=await sky.get_window_state({window:targetWindow}); nodeRepl.write(lasalState12.window)",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
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
>   "title": "LMCDiagnosticsService 다시 로드 준비",
>   "code": "await sky.click({window:targetWindow,x:1058,y:325}); await sky.press_key({window:targetWindow,key:'SHIFT+F10'}); nodeRepl.write('context opened')"
> }
> ```
>
> ```text
> context opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {

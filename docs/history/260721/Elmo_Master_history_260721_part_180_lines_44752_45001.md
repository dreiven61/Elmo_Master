>
> ```json
> {
>   "title": "Comm_Network 진단 객체 영역 확대",
>   "code": "await sky.click({window:targetWindow,x:933,y:467}); await sky.click({window:targetWindow,x:1429,y:140}); nodeRepl.write('network workspace expanded')"
> }
> ```
>
> ```text
> network workspace expanded
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "확대된 Comm_Network 확인",
>   "code": "var commExpandedState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(commExpandedState.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
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
>   "title": "네트워크 작업 영역 최대화",
>   "code": "await sky.click({window:targetWindow,x:932,y:590}); await sky.click({window:targetWindow,x:1429,y:141}); nodeRepl.write('panes closed')"
> }
> ```
>
> ```text
> panes closed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "최대화된 진단 네트워크 확인",
>   "code": "var fullCommState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(fullCommState.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
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
>   "title": "InputLatch 객체 구현 검색 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,x:737,y:434,mouse_button:'right'}); nodeRepl.write('network client right click sent')"
> }
> ```
>
> ```text
> network client right click sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 객체 메뉴 확인",
>   "code": "var networkClientMenu=await sky.get_window_state({window:targetWindow}); nodeRepl.write(networkClientMenu.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
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
>   "title": "InputLatch Find in Implementation 실행",
>   "code": "await sky.click({window:targetWindow,x:817,y:489}); nodeRepl.write('Find in Implementation clicked')"
> }
> ```
>
> ```text
> Find in Implementation clicked
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 구현 검색 결과 확인",
>   "code": "await new Promise(r=>setTimeout(r,900)); var inputImplState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(inputImplState.window)"
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
>   "title": "RecorderStore 구현 검색 준비",
>   "code": "await sky.click({window:targetWindow,x:278,y:141}); nodeRepl.write('Comm_Network selected again')"
> }
> ```
>
> ```text
> Comm_Network selected again
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 네트워크 위치 확인",
>   "code": "var commAfterSearch=await sky.get_window_state({window:targetWindow}); nodeRepl.write(commAfterSearch.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
> }
> ```
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
>   "title": "RecorderStore 채널 표시 영역 확대",
>   "code": "await sky.click({window:targetWindow,x:932,y:466}); nodeRepl.write('output pane closed')"
> }
> ```
>
> ```text
> output pane closed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 객체 구현 검색 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,x:737,y:456,mouse_button:'right'}); nodeRepl.write('RecorderStore network right click sent')"
> }
> ```
>
> ```text
> RecorderStore network right click sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 객체 메뉴 확인",
>   "code": "var recNetworkMenu=await sky.get_window_state({window:targetWindow}); nodeRepl.write(recNetworkMenu.window)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 3084742,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

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
>   "title": "진단 서비스 클라이언트 펼치기",
>   "code": "await sky.click({window:targetWindow,x:976,y:407}); nodeRepl.write('clients expanded')"
> }
> ```
>
> ```text
> clients expanded
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "진단 서비스 클라이언트 확인",
>   "code": "var diagClientsState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(diagClientsState.window)"
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
>   "title": "InputLatch 클라이언트 인터페이스 펼치기",
>   "code": "await sky.click({window:targetWindow,x:1008,y:423}); nodeRepl.write('InputLatch expanded')"
> }
> ```
>
> ```text
> InputLatch expanded
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 구현 심볼 확인",
>   "code": "var inputLatchExpanded=await sky.get_window_state({window:targetWindow}); nodeRepl.write(inputLatchExpanded.window)"
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
>   "title": "InputLatch 구현 검색 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,x:1053,y:423}); await sky.press_key({window:targetWindow,key:'SHIFT+F10'}); nodeRepl.write('client context opened')"
> }
> ```
>
> ```text
> client context opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 구현 검색 명령 확인",
>   "code": "var clientMenuState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(clientMenuState.window)"
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
>   "title": "RecorderStore 사용 위치로 이동",
>   "code": "await sky.press_key({window:targetWindow,key:'ESC'}); await sky.click({window:targetWindow,x:450,y:347}); await sky.press_key({window:targetWindow,key:'CTRL+G'}); await sky.type_text({window:targetWindow,text:'355'}); await sky.press_key({window:targetWindow,key:'ENTER'}); nodeRepl.write('goto line 355 sent')"
> }
> ```
>
> ```text
> goto line 355 sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 참조 확인",
>   "code": "var line355State=await sky.get_window_state({window:targetWindow}); nodeRepl.write(line355State.window)"
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
>   "title": "RecorderStore 참조 찾기",
>   "code": "await sky.press_key({window:targetWindow,key:'CTRL+F'}); await sky.type_text({window:targetWindow,text:'#RecorderStore'}); await sky.press_key({window:targetWindow,key:'ENTER'}); await sky.press_key({window:targetWindow,key:'ESC'}); nodeRepl.write('find RecorderStore sent')"
> }
> ```
>
> ```text
> find RecorderStore sent
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 참조 위치 확인",
>   "code": "var recRefState=await sky.get_window_state({window:targetWindow}); nodeRepl.write(recRefState.window)"
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
>   "title": "RecorderStore 검색창 열기",
>   "code": "await sky.click({window:targetWindow,x:500,y:285}); await sky.press_key({window:targetWindow,key:'CTRL+F'}); nodeRepl.write('find dialog requested')"
> }
> ```
>
> ```text
> find dialog requested
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RecorderStore 검색창 확인",
>   "code": "var findDialogState=await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:findDialogState.window,screens:findDialogState.screenshots?.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height,ox:s.originX,oy:s.originY}))})"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3084742,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService'
>   },
>   screens: [
>     { id: 'screenshot-0', z: 0, w: 1450, h: 763, ox: -1880, oy: 41 },
>     { id: 'screenshot-1', z: 1, w: 1440, h: 753, ox: -1875, oy: 46 },
>     { id: 'screenshot-2', z: 2, w: 504, h: 220, ox: 663, oy: 474 }
>   ]
> }
> ```

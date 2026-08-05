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
>   "title": "Control helper 위치 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ResponseSize 출력 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:850,y:180,mouse_button:'right'});",
>   "timeout_ms": 30000
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
>   "title": "ResponseSize 메뉴 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
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
>   "title": "ResponseSize 출력 추가",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:913,y:331});",
>   "timeout_ms": 30000
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
>   "title": "ResponseSize 출력 생성 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ResponseSize 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'ResponseSize'});",
>   "timeout_ms": 30000
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
>   "title": "ResponseSize 이름 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});",
>   "timeout_ms": 30000
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
>   "title": "LASAL 선언 Save All",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Control_L+Shift_L+s'});",
>   "timeout_ms": 30000
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
>   "title": "Save All 결과 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); nodeRepl.write(globalThis.state.window); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 393658,
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
>   "title": "LASAL IDE 종료",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Alt_L+F4'});",
>   "timeout_ms": 30000
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
>   "title": "LASAL 종료 확인",
>   "code": "var winsAfterClose2=await globalThis.sky.list_windows(); nodeRepl.write(winsAfterClose2.filter(w=>String(w.app).toLowerCase().includes('lasal2')||String(w.title||'').includes('Lasal Class 2')));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 393658,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
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
>   "title": "LASAL 종료 상태 확인",
>   "code": "globalThis.targetWindow=await globalThis.sky.get_window({app:winsAfterClose2.find(w=>String(w.app).toLowerCase().includes('lasal2')).app,id:winsAfterClose2.find(w=>String(w.app).toLowerCase().includes('lasal2')).id}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
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
>   "title": "사용 중인 라이브러리 유지",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:572,y:329});",
>   "timeout_ms": 30000
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

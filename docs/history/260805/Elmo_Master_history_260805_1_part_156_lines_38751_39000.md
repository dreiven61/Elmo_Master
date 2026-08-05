>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 구현 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "Reconcile 구현 smoke 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:globalThis.state.window.title,hasImpl:(globalThis.state.accessibility?.tree||\"\").includes(\"FUNCTION GLOBAL LMCControlCommandService::ReconcileAxisOwnershipStartup\")});"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService',
>   hasImpl: false
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
>   "title": "Diagnostics 클래스 검색 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+SHIFT+T\"});"
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
>   "title": "클래스 검색 대상으로 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"TAB\"});"
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
>   "title": "Class 검색 대상 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"HOME\"});"
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
>   "title": "클래스명 입력란으로 복귀",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"SHIFT+TAB\"});"
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
>   "title": "기존 검색어 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+A\"});"
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
>   "title": "Diagnostics 클래스명 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"LMCDiagnosticsService\"});"
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
>   "title": "클래스 검색 방향 Down 설정",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:236,y:194,mouse_button:\"left\"});"
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
>   "title": "Down 검색 방향 선택",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:255,y:194,mouse_button:\"left\"});"
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
>   "title": "Diagnostics 클래스 찾기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "Diagnostics 클래스 선택 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Name not found|Find in Tree/.test(x)).slice(0,15));"
> }
> ```
>
> ```text
> [
>   '\t\t59558 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t\t\t\t21378 콘솔 트리 (selectable) LMCDiagnosticsService',
>   '\t\t59558 대화 상자 Find in Tree Secondary Actions: Raise'
> ]
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
>   "title": "Diagnostics 클래스 검색 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:382,y:87,mouse_button:\"left\"});"
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
>   "title": "Diagnostics 메서드 검색 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+SHIFT+T\"});"
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
>   "title": "검색 대상 필드 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"TAB\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call

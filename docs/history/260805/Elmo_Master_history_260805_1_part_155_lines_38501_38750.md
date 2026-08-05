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
>   "title": "함수명 입력란으로 복귀",
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
>   "title": "Reconcile 함수명 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"ReconcileAxisOwnershipStartup\"});"
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
>   "title": "Reconcile 함수 찾기",
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
>   "title": "Reconcile 함수 트리 선택 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/ReconcileAxisOwnershipStartup|Name not found|Find in Tree/.test(x)).slice(0,15));"
> }
> ```
>
> ```text
> [
>   '\t\t56811 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t56811 대화 상자 Find in Tree Secondary Actions: Raise',
>   'The focused UI element is 56811 대화 상자 Find in Tree Secondary Actions: Raise.'
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
>   "title": "Reconcile 함수명 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"ReconcileAxisOwnershipStartup\"});"
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
>   "title": "Reconcile 함수 선택",
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
>   "title": "Reconcile 선택 결과 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/ReconcileAxisOwnershipStartup|Name not found|Find in Tree/.test(x)).slice(0,20));"
> }
> ```
>
> ```text
> [
>   '\t\t56811 대화 상자 (disabled) Find in Tree Secondary Actions: Raise',
>   '\t\t\t\t57117 텍스트 Name not found ID: 65535',
>   '\t\t\t57117 텍스트 Name not found ID: 65535',
>   '\t\t56811 대화 상자 (disabled) Find in Tree Secondary Actions: Raise',
>   '\t\t\t\t57117 텍스트 Name not found ID: 65535'
> ]
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
>   "title": "함수 검색 알림 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===3).id,x:89,y:134,mouse_button:\"left\"});"
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
>   "title": "검색 방향 위로 전환",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:236,y:169,mouse_button:\"left\"});"
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
>   "title": "검색 방향 Up 선택",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:236,y:169,mouse_button:\"left\"});"
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
>   "title": "Reconcile 함수 위쪽 검색",
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
>   "title": "Reconcile 함수 선택 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/ReconcileAxisOwnershipStartup|Name not found|Find in Tree/.test(x)).slice(0,20));"
> }
> ```
>
> ```text
> [
>   '\t\t56811 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t\t\t\t\t\t\t57335 콘솔 트리 (selectable) ReconcileAxisOwnershipStartup',
>   '\t\t56811 대화 상자 Find in Tree Secondary Actions: Raise'
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
>   "title": "Reconcile 검색 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:382,y:87,mouse_button:\"left\"});"
> }
> ```
>
> ```text

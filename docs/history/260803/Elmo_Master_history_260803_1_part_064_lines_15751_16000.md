> ```json
> {
>   "title": "OutputPreviousValid 속성 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"OutputPreviousValid\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
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
>   "title": "OutputPreviousValid 타입 편집",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:513,screenshotId:globalThis.state.screenshots[1].id,click_count:2});"
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
>   "title": "OutputPreviousValid을 BOOL로 지정",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"BOOL\"});"
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
>   "title": "OutputPreviousValid 타입 확정",
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
>   "title": "BOOL 타입 검증",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/\\bVariables\\b/.test(x)||x.includes(\"OutputPreviousValid\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> 						17899 콘솔 트리 (selectable) Variables
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
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
>   "title": "마지막 상태 변수 추가 메뉴",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:17899,mouse_button:\"right\"});"
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
>   "title": "마지막 변수 메뉴 위치 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"메뉴 항목 New Variable\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> 			65717 메뉴 항목 New Variable
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
>   "title": "OutputPreviousValue 변수 생성",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1308,y:288,screenshotId:globalThis.state.screenshots[1].id});"
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
>   "title": "OutputPreviousValue 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"OutputPreviousValue\"});"
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
>   "title": "OutputPreviousValue 이름 확정",
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
>   "title": "OutputPreviousValue 속성 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"OutputPreviousValue\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> 							66427 콘솔 트리 (selectable) OutputPreviousValue
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
>   "title": "OutputPreviousValue 타입 편집",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:513,screenshotId:globalThis.state.screenshots[1].id,click_count:2});"
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
>   "title": "OutputPreviousValue를 UDINT로 지정",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"UDINT\"});"
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
>   "title": "OutputPreviousValue 타입 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
> }
> ```
>
> ```text
>
> ```
>
> 내부 상태 변수는 `OutputRevision/OutputPreviousValue=UDINT`, `OutputObserved/OutputPreviousValid=BOOL`로 등록을 끝냈습니다. 검증기의 정식 계약과 대조해, 다음 단계에서는 두 전역 latch 메서드와 하나의 private 진단 핸들러를 빈 IDE 스텁으로 정확히 만든 뒤 메타데이터가 갱신되는지 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "메서드 폴더 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/\\bMethods\\b/.test(x)&&x.includes(\"트리\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> 						17898 콘솔 트리 (selectable) Methods
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

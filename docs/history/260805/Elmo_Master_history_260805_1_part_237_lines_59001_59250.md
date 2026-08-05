>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수명 전체 선택",
>   "code": "await sky.press_key({window:targetWindow, key:\"Control_L+a\"}); nodeRepl.write(\"selected Name text\");"
> }
> ```
>
> ```text
> selected Name text
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수명 선택 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element,selectedText:state.accessibility?.selected_text},null,2));"
> }
> ```
>
> ```text
> {
>   "focused": "33866 편집 ID: 693892752"
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
>   "title": "LASAL 선점 변수명 타이핑",
>   "code": "await sky.type_text({window:targetWindow, text:\"OwnershipPreemptedState\"}); nodeRepl.write(\"typed after select-all\");"
> }
> ```
>
> ```text
> typed after select-all
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수명 변경 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write(\"fresh\");"
> }
> ```
>
> ```text
> fresh
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
>   "title": "LASAL 속성 편집 종료",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); nodeRepl.write(\"closed property edit\");"
> }
> ```
>
> ```text
> closed property edit
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수 선택 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility?.focused_element||\"\");"
> }
> ```
>
> ```text
> 109 TABLE Properties Window ID: 272020368
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
>   "title": "LASAL 복제 변수 선택",
>   "code": "await sky.click({window:targetWindow,x:1123,y:382}); nodeRepl.write(\"selected duplicate\");"
> }
> ```
>
> ```text
> selected duplicate
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 선택 상태 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(\"fresh\");"
> }
> ```
>
> ```text
> fresh
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
>   "title": "LASAL 인라인 변수명 전체 선택",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); nodeRepl.write(\"selected inline name\");"
> }
> ```
>
> ```text
> selected inline name
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 인라인 선택 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility?.focused_element||\"\");"
> }
> ```
>
> ```text
> 40987 편집 ID: 1
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
>   "title": "LASAL 선점 변수명 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"OwnershipPreemptedState\"}); nodeRepl.write(\"typed inline name\");"
> }
> ```
>
> ```text
> typed inline name
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 인라인 이름 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility?.focused_element||\"\");"
> }
> ```
>
> ```text
> 40987 편집 ID: 1
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
>   "title": "LASAL 선점 변수명 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed name\");"
> }
> ```
>
> ```text
> committed name
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 선점 변수 검증",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").match(/Ownership[^\\n]*/g)?.join(\"\\n\")||\"\");"
> }
> ```
>
> ```text
> OwnershipState
> OwnershipStartupState
> OwnershipObserverState
> OwnershipLeaseState
> OwnershipPreemptedState
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
>   "title": "LASAL 변수 목록 선택",

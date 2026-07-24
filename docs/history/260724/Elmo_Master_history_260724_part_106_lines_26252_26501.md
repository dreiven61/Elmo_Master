> node_repl.js
>
> ```json
> {
>   "title": "첫 axis client 생성",
>   "code": "await sky.click({window:targetWindow,x:1350,y:612}); await new Promise(r=>setTimeout(r,450)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "첫 private handler 이름 지정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); await sky.type_text({window:targetWindow,text:\"HandleAdminCommands\"}); await sky.press_key({window:targetWindow,key:\"Return\"}); await new Promise(r=>setTimeout(r,450)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "첫 LMCAxis client 생성",
>   "code": "await sky.click({window:targetWindow,x:1285,y:278,mouse_button:\"right\"}); await new Promise(r=>setTimeout(r,250)); await sky.click({window:targetWindow,x:1350,y:580}); await new Promise(r=>setTimeout(r,500)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "service class 메뉴 재확인",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:1294,y:278,mouse_button:\"right\"}); await new Promise(r=>setTimeout(r,350)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 5 }
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
>   "title": "LMCAxis client 생성",
>   "code": "await sky.click({window:targetWindow,x:1355,y:565}); await new Promise(r=>setTimeout(r,500)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "LMCAxis1 client 이름 지정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); await sky.type_text({window:targetWindow,text:\"LMCAxis1\"}); await sky.press_key({window:targetWindow,key:\"Return\"}); await new Promise(r=>setTimeout(r,450)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "LMCAxis1 client를 object channel로 설정",
>   "code": "await sky.click({window:targetWindow,x:1371,y:513}); await new Promise(r=>setTimeout(r,350)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "client channel 종류 선택",
>   "code": "await sky.click({window:targetWindow,x:1433,y:517}); await new Promise(r=>setTimeout(r,350)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "기존 object client 설정 참고",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:1183,y:379}); await new Promise(r=>setTimeout(r,350)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "기존 object client 설정 참고",
>   "code": "await sky.click({window:targetWindow,x:1183,y:278}); await new Promise(r=>setTimeout(r,250)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "기존 object client 속성 확인",
>   "code": "await sky.click({window:targetWindow,x:1184,y:298}); await new Promise(r=>setTimeout(r,350)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "object client 항목 열기",
>   "code": "await sky.click({window:targetWindow,x:1198,y:298}); await new Promise(r=>setTimeout(r,300)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
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
>   "title": "LMCAxis1 client 편집으로 복귀",
>   "code": "await sky.click({window:targetWindow,x:1183,y:257}); await sky.click({window:targetWindow,x:1183,y:235}); await new Promise(r=>setTimeout(r,300)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({shotCount:state.screenshots?.length})"
> }
> ```
>
> ```text
> { shotCount: 2 }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg

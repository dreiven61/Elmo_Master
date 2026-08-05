> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "입력 이름 전체 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Control_L+a'});",
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
>   "title": "선택 상태 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); nodeRepl.write(globalThis.state.accessibility?.slice?globalThis.state.accessibility.slice(0,1000):globalThis.state.accessibility); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> null
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
>   "title": "pRequest 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'pRequest'});",
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
>   "title": "이름 입력 확인",
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
>   "title": "pRequest 이름 확정",
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
>   "title": "pRequest 선언 확인",
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
>   "title": "pRequest 타입 편집",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:855,y:371});",
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
>   "title": "타입 편집 상태 확인",
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "USINT 타입 선택",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'USINT'});",
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
>   "title": "USINT 타입 확정",
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
>   "title": "pRequest 타입 확인",
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
>   "title": "pRequest 포인터 설정",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:855,y:390});",
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
>   "title": "포인터 편집 상태 확인",
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
>   "title": "포인터 true 선택",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'true'});",
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

> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "RequestFrameSize 생성 확인",
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
>   "title": "RequestFrameSize 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'RequestFrameSize'});",
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
>   "title": "RequestFrameSize 이름 확정",
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
>   "title": "RequestFrameSize 타입 편집",
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
>   "title": "RequestFrameSize UDINT 선택",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'UDINT'});",
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
>   "title": "RequestFrameSize 타입 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> Control helper를 private로 생성했고 현재 `CommandId : UINT`, `Reference : UINT`, `pRequestFrame : ^USINT`, `RequestFrameSize : UDINT`까지 입력 순서대로 추가했습니다. 남은 것은 `pResponseFrame`, `ResponseCapacity`, 출력 `ResponseSize`입니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control helper 선언 상태 확인",
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
>   "title": "pResponseFrame 입력 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:850,y:159,mouse_button:'right'});",
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
>   "title": "pResponseFrame 메뉴 확인",
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
>   "title": "pResponseFrame 입력 추가",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:913,y:291});",
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
>   "title": "pResponseFrame 생성 확인",
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
>   "title": "pResponseFrame 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'pResponseFrame'});",
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
>   "title": "pResponseFrame 이름 확정",
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
>   "title": "pResponseFrame 타입 편집",
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
>   "title": "pResponseFrame USINT 선택",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'USINT'});",

>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "포인터 true 위치 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'End'});",
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
>   "title": "포인터 값 확정",
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
>   "title": "포인터 값 확인",
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
>   "title": "포인터 선택 목록 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:936,y:389});",
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
>   "title": "포인터 목록 상세 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); nodeRepl.write(globalThis.state.screenshots.map(s=>({id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height}))); for (var si3 of globalThis.state.screenshots) await nodeRepl.emitImage(si3.url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     id: 'screenshot-0',
>     originX: -1895,
>     originY: 22,
>     width: 970,
>     height: 512
>   },
>   {
>     id: 'screenshot-1',
>     originX: -1890,
>     originY: 27,
>     width: 960,
>     height: 502
>   }
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
>   "title": "포인터 불리언 토글",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Space'});",
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
>   "title": "포인터 토글 확인",
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
>   "title": "포인터 목록 활성화",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:935,y:389});",
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
>   "title": "true 빠른 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'t'});",
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
>   "title": "true 선택 확정",
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
>   "title": "포인터 선택 결과 확인",
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
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클릭 인자 형식 확인",
>   "code": "var idxclick2=apiDoc2.indexOf('type ClickInput'); nodeRepl.write(apiDoc2.slice(idxclick2,idxclick2+1000));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> type ClickInput = {
>   click_count?: number; // Number of clicks to perform.
>   element_index?: number; // Element index from the latest `get_window_state()` accessibility tree.
>   mouse_button?: MouseButton; // Mouse button to click.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to click in.
>   x?: number; // Window-relative X coordinate.
>   y?: number; // Window-relative Y coordinate.
> };
>
> type PressKeyInput = {
>   key: string; // Key or `+`-separated key chord using X Window System keysym-style names, such as `a`, `space`, `Return`, `Tab`, `Control_L+a`, `Control_L+Shift_L+period`, or `KP_0`; whitespace around `+` is ignored, and common aliases such as `Control`, `Ctrl`, `Alt`, `Shift`, `period`, `greater`, and `Numpad_0` are accepted.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to receive
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "다음 입력 변수 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:850,y:137,mouse_button:'right'});",
>   "timeout_ms": 30000
> }

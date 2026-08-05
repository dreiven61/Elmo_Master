> {
>   "title": "Network 확대 결과 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
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
>   "title": "Network 확대 도구 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:138,y:41,screenshot_id:globalThis.state.screenshots[0].id}); await globalThis.sky.click({window:globalThis.targetWindow,x:161,y:134,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "Network 클러스터 확대",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:105,y:245,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "Network 확대 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
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
>   "title": "축·Latch 영역 확대",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:500,y:330,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "축·Latch 영역 식별",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
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
>   "title": "Latch와 축 포트 확대",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:760,y:240,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "포트 이름 식별",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Network 연결 드래그 명령 확인",
>   "code": "nodeRepl.write(Object.keys(globalThis.sky).filter(k=>k.toLowerCase().includes(\"drag\")||k.toLowerCase().includes(\"move\")));"
> }
> ```
>
> ```text
> [ 'drag' ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "드래그 API 형식 확인",
>   "code": "nodeRepl.write(String(globalThis.sky.drag));"
> }
> ```
>
> ```text
> (...args) => {
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app: getComputerUseAppReference(args[0]),
>           },
>         });
>         return Reflect.apply(value, target, args);
>       }
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
>   "title": "Network Select 도구 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:138,y:41,screenshot_id:globalThis.state.screenshots[0].id}); await globalThis.sky.click({window:globalThis.targetWindow,x:158,y:65,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "Axis 3 Home 제어 채널 연결",
>   "code": "await globalThis.sky.drag({window:globalThis.targetWindow,from_x:943,from_y:225,to_x:349,to_y:219,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Axis 3 연결 검증",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
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
>   "title": "잘못 이동한 Network 객체 되돌리기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+Z\"});"
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
>   "title": "Latch 위치 복구 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
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
>   "title": "미연결 축 Client 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:943,y:232,mouse_button:\"right\",screenshotId:globalThis.state.screenshots[0].id});"
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

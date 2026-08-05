> node_repl.js
>
> ```json
> {
>   "title": "기존 LMCAxis Client 찾기",
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
>   "title": "Control 서비스 Clients 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1250,y:280,screenshot_id:globalThis.state.screenshots[0].id}); await globalThis.sky.click({window:globalThis.targetWindow,x:1250,y:280,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "Control 서비스 축 Client 확인",
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
>   "title": "기존 축 Client 속성 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1280,y:176,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "축 Client 정확한 속성 확인",
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
>   "title": "기존 축 Client 템플릿 복사",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+C\"});"
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
>   "title": "Latch 클래스 위치로 이동",
>   "code": "await globalThis.sky.scroll({window:globalThis.targetWindow,x:1400,y:370,scrollX:0,scrollY:581});"
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
>   "title": "Latch 클래스 찾기",
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
>   "title": "Latch 임시 Client 찾기",
>   "code": "await globalThis.sky.scroll({window:globalThis.targetWindow,x:1395,y:365,scrollX:0,scrollY:389});"
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
>   "title": "Latch Client 목록 확인",
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
>   "title": "Latch 임시 Client 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1275,y:298,screenshot_id:globalThis.state.screenshots[0].id});"
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
>   "title": "Latch 임시 Client 검증",
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
>   "title": "임시 Data Channel 제거",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DELETE\"});"
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
>   "title": "Latch Clients 그룹으로 이동",
>   "code": "await globalThis.sky.scroll({window:globalThis.targetWindow,x:1400,y:155,scrollX:0,scrollY:-299});"
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
>   "title": "Latch Clients 그룹 확인",
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
>   "title": "축 Client 템플릿 붙여넣기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1250,y:220,screenshot_id:globalThis.state.screenshots[0].id}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+V\"});"
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
>   "title": "붙여넣은 Client 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window: globalThis.targetWindow, include_screenshot:true, include_text:true});"
> }
> ```
>
> Image output: image/jpeg

>   "code": "var cuaState89 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState89.screenshots[0].url);"
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
>   "title": "Private 속성 적용",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"ENTER\"});"
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
>   "title": "Private 이동 확인",
>   "code": "var cuaState90 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState90.screenshots[0].url); nodeRepl.write((JSON.stringify(cuaState90.accessibility)||\"\").match(/LMCControlCommandService[\\s\\S]*?Dependencies/)?.[0]||\"\");"
> }
> ```
>
> ```text
> LMCControlCommandService\n\t\t\t\t\t\t14711 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t14712 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t14713 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t\t15861 콘솔 트리 (selectable) Global\n\t\t\t\t\t\t\t\t19783 콘솔 트리 (selectable) HandleRequest\n\t\t\t\t\t\t\t15862 콘솔 트리 (selectable) Private\n\t\t\t\t\t\t\t\t69085 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t69086 콘솔 트리 (selectable) HandleRequest1\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "복제 private 메서드 선택",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1304,y:299});"
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
>   "title": "복제 private ABI 확인",
>   "code": "var cuaState91 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState91.screenshots[0].url);"
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
>   "title": "복제 private ABI 펼치기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1234,y:299});"
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
>   "title": "복제 ABI 파라미터 확인",
>   "code": "var cuaState92 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState92.accessibility)||\"\").match(/HandleRequest1[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState92.screenshots[0].url);"
> }
> ```
>
> ```text
> HandleRequest1\n\t\t\t\t\t\t\t\t\t70887 콘솔 트리 (selectable) CommandId\n\t\t\t\t\t\t\t\t\t70888 콘솔 트리 (selectable) Reference\n\t\t\t\t\t\t\t\t\t70889 콘솔 트리 (selectable) pRequestFrame\n\t\t\t\t\t\t\t\t\t70890 콘솔 트리 (selectable) RequestFrameSize\n\t\t\t\t\t\t\t\t\t70891 콘솔 트리 (selectable) pResponseFrame\n\t\t\t\t\t\t\t\t\t70892 콘솔 트리 (selectable) ResponseCapacity\n\t\t\t\t\t\t\t\t\t70893 콘솔 트리 (selectable) ResponseSize\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "private 메서드 이름 편집",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1361,y:474});"
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
>   "title": "이름 편집 상태 확인",
>   "code": "var cuaState93 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState93.screenshots[0].url);"
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
>   "title": "메서드 이름 편집 활성화",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1356,y:478,clickCount:2});"
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
>   "title": "이름 입력란 확인",
>   "code": "var cuaState94 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState94.screenshots[0].url);"
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
>   "title": "메서드 이름 편집 시작",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"F2\"});"
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
>   "title": "이름 입력 준비 확인",
>   "code": "var cuaState95 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState95.screenshots[0].url);"
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
>   "title": "private 메서드 편집 메뉴 열기",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"SHIFT+F10\"});"
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
>   "title": "private 메서드 메뉴 확인",
>   "code": "var cuaState96 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState96.screenshots[0].url);"
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {

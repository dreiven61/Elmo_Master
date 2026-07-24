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
>   "title": "private 메서드 바로 붙여넣기",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"CTRL+V\"});"
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
>   "title": "바로 붙여넣기 결과 확인",
>   "code": "var cuaState114 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState114.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState114.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t\t\t90831 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t90832 콘솔 트리 (selectable) HandleRegistryCommands\n\t\t\t\t\t\t\t\t90833 콘솔 트리 (selectable) HandleAxisCommands\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "Group 원본 메서드 선택",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1336,y:301});"
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
>   "title": "Group 원본 복사 메뉴",
>   "code": "var cuaState115 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.press_key({window:cuaLasalWindow27,key:\"SHIFT+F10\"});"
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
>   "title": "Group 원본 메뉴 확인",
>   "code": "var cuaState116 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState116.screenshots[0].url);"
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
>   "title": "Group 원본 메서드 복사",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1292,y:417});"
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
>   "title": "Group 붙여넣기 폴더 선택",
>   "code": "var cuaState117 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.click({window:cuaLasalWindow27,x:1267,y:262});"
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
>   "title": "Group Private 선택 확인",
>   "code": "var cuaState118 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState118.screenshots[0].url);"
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
>   "title": "Group 붙여넣기 메뉴 열기",
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
>   "title": "Group 붙여넣기 항목 확인",
>   "code": "var cuaState119 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState119.screenshots[0].url);"
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
>   "title": "Paste 메뉴 선택 1",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"DOWN\"});"
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
>   "title": "Paste 메뉴 선택 확인",
>   "code": "var cuaState120 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState120.screenshots[0].url);"
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
>   "title": "Paste 메뉴 선택 2",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"DOWN\"});"
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
>   "title": "Paste 메뉴 위치 확인 2",
>   "code": "var cuaState121 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState121.screenshots[0].url);"
> }
> ```
>
> Image output: image/jpeg
>

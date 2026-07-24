>   "title": "속성 메뉴 닫기",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:\"ESC\"});"
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
>   "title": "메서드 트리 포커스",
>   "code": "var cuaState97 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.click({window:cuaLasalWindow27,x:1310,y:239});"
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
>   "title": "트리 메서드 이름 편집",
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
>   "title": "트리 이름 입력란 확인",
>   "code": "var cuaState98 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState98.screenshots[0].url);"
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
>   "title": "HandleRegistryCommands 이름 입력",
>   "code": "await sky.type_text({window:cuaLasalWindow27,text:\"HandleRegistryCommands\"});"
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
>   "code": "var cuaState99 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState99.screenshots[0].url);"
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
>   "title": "HandleRegistryCommands 이름 적용",
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
>   "title": "Registry 메서드 적용 확인",
>   "code": "var cuaState100 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState100.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState100.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t\t\t69085 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t78021 콘솔 트리 (selectable) HandleRegistryCommands\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "Registry 메서드 복사 메뉴",
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
>   "title": "Registry 메뉴 확인",
>   "code": "var cuaState101 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState101.screenshots[0].url);"
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
>   "title": "Registry 메서드 복사",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1292,y:378});"
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
>   "title": "Private 폴더 선택",
>   "code": "var cuaState102 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.click({window:cuaLasalWindow27,x:1271,y:220});"
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
>   "title": "Private 선택 확인",
>   "code": "var cuaState103 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState103.screenshots[0].url);"
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
>   "title": "Private 붙여넣기 메뉴",
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
>   "title": "Private 붙여넣기 확인",
>   "code": "var cuaState104 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState104.screenshots[0].url);"
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

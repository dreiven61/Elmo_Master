>
> ```json
> {
>   "title": "private 메서드 복제",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1277,y:316});"
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
>   "title": "private 복제 결과 확인",
>   "code": "var cuaState105 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState105.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState105.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t\t\t83279 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t83280 콘솔 트리 (selectable) HandleRegistryCommands\n\t\t\t\t\t\t\t\t83281 콘솔 트리 (selectable) HandleRegistryCommands1\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "Axis 복제 메서드 선택",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1328,y:319});"
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
>   "title": "Axis 이름 편집 준비",
>   "code": "var cuaState106 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.press_key({window:cuaLasalWindow27,key:\"F2\"});"
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
>   "title": "Axis 이름 입력란 확인",
>   "code": "var cuaState107 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState107.screenshots[0].url);"
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
>   "title": "HandleAxisCommands 이름 입력",
>   "code": "await sky.type_text({window:cuaLasalWindow27,text:\"HandleAxisCommands\"});"
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
>   "title": "Axis 이름 입력 확인",
>   "code": "var cuaState108 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.press_key({window:cuaLasalWindow27,key:\"ENTER\"});"
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
>   "title": "Axis 이름 적용 결과 확인",
>   "code": "var cuaState109 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState109.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState109.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t\t\t83279 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t83280 콘솔 트리 (selectable) HandleRegistryCommands\n\t\t\t\t\t\t\t\t86349 콘솔 트리 (selectable) HandleAxisCommands\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "Group 복제용 Private 선택",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1264,y:261});"
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
>   "title": "Group 복제 메뉴 준비",
>   "code": "var cuaState110 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await sky.press_key({window:cuaLasalWindow27,key:\"SHIFT+F10\"});"
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
>   "title": "Group 복제 메뉴 확인",
>   "code": "var cuaState111 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); await nodeRepl.emitImage(cuaState111.screenshots[0].url);"
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
>   "title": "Group private 메서드 복제",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1275,y:353});"
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
>   "title": "Group 복제 결과 확인",
>   "code": "var cuaState112 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState112.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState112.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "Private 메서드 목록 펼치기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1216,y:263});"
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
>   "title": "Group 복제 여부 확인",
>   "code": "var cuaState113 = await sky.get_window_state({window:cuaLasalWindow27, include_screenshot:true, include_text:true}); nodeRepl.write((JSON.stringify(cuaState113.accessibility)||\"\").match(/Private[\\s\\S]*?Dependencies/)?.[0]||\"\"); await nodeRepl.emitImage(cuaState113.screenshots[0].url);"
> }
> ```
>
> ```text
> Private\n\t\t\t\t\t\t\t\t90831 콘솔 트리 (selectable) HandleAdminCommands\n\t\t\t\t\t\t\t\t90832 콘솔 트리 (selectable) HandleRegistryCommands\n\t\t\t\t\t\t\t\t90833 콘솔 트리 (selectable) HandleAxisCommands\n\t\t\t\t\t\t14714 콘솔 트리 (selectable) Dependencies
> ```
>

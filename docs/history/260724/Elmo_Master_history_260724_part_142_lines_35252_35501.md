>   "title": "Reference 입력 편집 상태 확인",
>   "code": "var cuaState44 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState44.accessibility?.focused_element||''); if(cuaState44.screenshots?.[0]) await nodeRepl.emitImage(cuaState44.screenshots[0].url)"
> }
> ```
>
> ```text
> 27911 편집 ID: 1
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
>   "title": "Reference 이름 입력",
>   "code": "await sky.type_text({window:cuaLasalWindow27,text:'Reference'}); nodeRepl.write('typed Reference')"
> }
> ```
>
> ```text
> typed Reference
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reference 이름 확인",
>   "code": "var cuaState45 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState45.accessibility?.focused_element||''); if(cuaState45.screenshots?.[0]) await nodeRepl.emitImage(cuaState45.screenshots[0].url)"
> }
> ```
>
> ```text
> 27911 편집 ID: 1
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
>   "title": "Reference 생성 확정",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:'Return'}); nodeRepl.write('confirmed Reference')"
> }
> ```
>
> ```text
> confirmed Reference
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reference 기본 형식 확인",
>   "code": "var cuaState46 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState46.screenshots?.[0]) await nodeRepl.emitImage(cuaState46.screenshots[0].url); nodeRepl.write('Reference selected')"
> }
> ```
>
> ```text
> Reference selected
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
>   "title": "Reference 형식 편집",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1352,y:491,click_count:2,screenshotId:cuaState46.screenshots[0].id}); nodeRepl.write('opened type dropdown')"
> }
> ```
>
> ```text
> opened type dropdown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reference 형식 목록 확인",
>   "code": "var cuaState47 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState47.screenshots?.[0]) await nodeRepl.emitImage(cuaState47.screenshots[0].url); nodeRepl.write(cuaState47.accessibility?.focused_element||'')"
> }
> ```
>
> ```text
> 29276 편집
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
>   "title": "Reference 형식을 UINT로 지정",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1032,y:665,screenshotId:cuaState47.screenshots[0].id}); nodeRepl.write('selected UINT')"
> }
> ```
>
> ```text
> selected UINT
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reference UINT 적용 확인",
>   "code": "var cuaState48 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState48.screenshots?.[0]) await nodeRepl.emitImage(cuaState48.screenshots[0].url); nodeRepl.write('Reference UINT')"
> }
> ```
>
> ```text
> Reference UINT
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
>   "title": "pRequestFrame 입력 변수 추가 메뉴",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1306,y:341,mouse_button:'right',screenshotId:cuaState48.screenshots[0].id}); nodeRepl.write('opened HandleRequest menu')"
> }
> ```
>
> ```text
> opened HandleRequest menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame 메뉴 확인",
>   "code": "var cuaState49 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState49.screenshots?.[0]) await nodeRepl.emitImage(cuaState49.screenshots[0].url); nodeRepl.write('menu open')"
> }
> ```
>
> ```text
> menu open
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
>   "title": "pRequestFrame 입력 변수 생성",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1386,y:467,screenshotId:cuaState49.screenshots[0].id}); nodeRepl.write('new input selected')"
> }
> ```
>
> ```text
> new input selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame 이름 편집 확인",
>   "code": "var cuaState50 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState50.accessibility?.focused_element||''); if(cuaState50.screenshots?.[0]) await nodeRepl.emitImage(cuaState50.screenshots[0].url)"
> }
> ```
>
> ```text
> 31817 편집 ID: 1
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
>   "title": "pRequestFrame 이름 입력",
>   "code": "await sky.type_text({window:cuaLasalWindow27,text:'pRequestFrame'}); nodeRepl.write('typed pRequestFrame')"
> }

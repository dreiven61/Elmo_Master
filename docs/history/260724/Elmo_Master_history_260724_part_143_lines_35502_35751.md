> ```
>
> ```text
> typed pRequestFrame
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame 이름 확인",
>   "code": "var cuaState51 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState51.accessibility?.focused_element||''); if(cuaState51.screenshots?.[0]) await nodeRepl.emitImage(cuaState51.screenshots[0].url)"
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
>   "title": "pRequestFrame 생성 확정",
>   "code": "await sky.press_key({window:cuaLasalWindow27,key:'Return'}); nodeRepl.write('confirmed pRequestFrame')"
> }
> ```
>
> ```text
> confirmed pRequestFrame
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame 기본 속성 확인",
>   "code": "var cuaState52 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState52.screenshots?.[0]) await nodeRepl.emitImage(cuaState52.screenshots[0].url); nodeRepl.write('pRequestFrame selected')"
> }
> ```
>
> ```text
> pRequestFrame selected
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
>   "title": "pRequestFrame 형식 편집",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1352,y:493,click_count:2,screenshotId:cuaState52.screenshots[0].id}); nodeRepl.write('opened type dropdown')"
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
>   "title": "pRequestFrame 형식 목록 확인",
>   "code": "var cuaState53 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState53.screenshots?.[0]) await nodeRepl.emitImage(cuaState53.screenshots[0].url); nodeRepl.write(cuaState53.accessibility?.focused_element||'')"
> }
> ```
>
> ```text
> 33332 편집
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
>   "title": "pRequestFrame 형식을 USINT로 지정",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1034,y:680,screenshotId:cuaState53.screenshots[0].id}); nodeRepl.write('selected USINT')"
> }
> ```
>
> ```text
> selected USINT
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame USINT 적용 확인",
>   "code": "var cuaState54 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState54.screenshots?.[0]) await nodeRepl.emitImage(cuaState54.screenshots[0].url); nodeRepl.write('pRequestFrame USINT')"
> }
> ```
>
> ```text
> pRequestFrame USINT
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
>   "title": "pRequestFrame 포인터 속성 편집",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1350,y:513,click_count:2,screenshotId:cuaState54.screenshots[0].id}); nodeRepl.write('opened Pointer editor')"
> }
> ```
>
> ```text
> opened Pointer editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "pRequestFrame 포인터 옵션 확인",
>   "code": "var cuaState55 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState55.screenshots?.[0]) await nodeRepl.emitImage(cuaState55.screenshots[0].url); nodeRepl.write(cuaState55.accessibility?.focused_element||'')"
> }
> ```
>
> ```text
> 107 TABLE Properties Window ID: 125710488
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
>   "title": "RequestFrameSize 입력 변수 추가 메뉴",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1304,y:317,mouse_button:'right',screenshotId:cuaState55.screenshots[0].id}); nodeRepl.write('opened HandleRequest menu')"
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
>   "title": "RequestFrameSize 메뉴 확인",
>   "code": "var cuaState56 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); if(cuaState56.screenshots?.[0]) await nodeRepl.emitImage(cuaState56.screenshots[0].url); nodeRepl.write('menu open')"
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
>   "title": "RequestFrameSize 입력 변수 생성",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1385,y:444,screenshotId:cuaState56.screenshots[0].id}); nodeRepl.write('new input selected')"
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
>   "title": "RequestFrameSize 이름 편집 확인",
>   "code": "var cuaState57 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState57.accessibility?.focused_element||''); if(cuaState57.screenshots?.[0]) await nodeRepl.emitImage(cuaState57.screenshots[0].url)"
> }
> ```
>
> ```text
> 36615 편집 ID: 1
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg

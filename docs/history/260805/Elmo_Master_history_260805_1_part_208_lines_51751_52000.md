> ```json
> {
>   "title": "서비스 객체 위치로 이동",
>   "code": "await sky.scroll({window:globalThis.targetWindow77,x:884,y:241,scrollX:0,scrollY:617}); nodeRepl.write(\"scrolled object tree\");"
> }
> ```
>
> ```text
> scrolled object tree
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "서비스 객체 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "Control 서비스 클래스 확장",
>   "code": "await sky.click({window:globalThis.targetWindow77,x:706,y:138}); nodeRepl.write(\"expanded control service class\");"
> }
> ```
>
> ```text
> expanded control service class
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 구성 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "Methods 확장",
>   "code": "await sky.click({window:globalThis.targetWindow77,x:722,y:204}); nodeRepl.write(\"expanded Methods\");"
> }
> ```
>
> ```text
> expanded Methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "메서드 그룹 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "Global 메서드 확장",
>   "code": "await sky.click({window:globalThis.targetWindow77,x:739,y:223}); nodeRepl.write(\"expanded Global methods\");"
> }
> ```
>
> ```text
> expanded Global methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Global 메서드 목록 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "ValidateAxisOwnership 인수 확장",
>   "code": "await sky.click({window:globalThis.targetWindow77,x:751,y:219}); nodeRepl.write(\"expanded ValidateAxisOwnership\");"
> }
> ```
>
> ```text
> expanded ValidateAxisOwnership
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "메서드 인수 그룹 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "입력 목록 끝으로 이동",
>   "code": "await sky.scroll({window:globalThis.targetWindow77,x:879,y:237,scrollX:0,scrollY:522}); nodeRepl.write(\"scrolled method inputs\");"
> }
> ```
>
> ```text
> scrolled method inputs
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "마지막 입력 항목 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "ValidateAxisOwnership 위치로 복귀",
>   "code": "await sky.scroll({window:globalThis.targetWindow77,x:880,y:198,scrollX:0,scrollY:-303}); nodeRepl.write(\"scrolled up method tree\");"
> }
> ```
>
> ```text
> scrolled up method tree
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ValidateAxisOwnership 위치 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
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
>   "title": "OwnerGeneration 항목 메뉴 열기",
>   "code": "await sky.click({window:globalThis.targetWindow77,x:854,y:219,button:\"right\"}); nodeRepl.write(\"right-clicked OwnerGeneration\");"
> }
> ```
>
> ```text
> right-clicked OwnerGeneration
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "입력 항목 메뉴 확인",
>   "code": "globalThis.state77 = await sky.get_window_state({window:globalThis.targetWindow77}); if(globalThis.state77.screenshots[0]?.url) await nodeRepl.emitImage(globalThis.state77.screenshots[0].url);"
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

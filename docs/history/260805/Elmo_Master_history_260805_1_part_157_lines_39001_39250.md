>
> node_repl.js
>
> ```json
> {
>   "title": "Method 검색 대상 선택",
>   "code": "for (let i=0;i<6;i++) await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "Method 검색 대상 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.find(s=>s.zIndex===2).id);"
> }
> ```
>
> ```text
> screenshot-2
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
>   "title": "Diagnostics 함수명 입력란 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:190,y:54,mouse_button:\"left\"});"
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
>   "title": "기존 검색어 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+A\"});"
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
>   "title": "ProcessAxisOwnershipStartup 검색",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"ProcessAxisOwnershipStartup\"});"
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
>   "title": "PRIVATE Process 함수 찾기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "PRIVATE Process 함수 선택 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/ProcessAxisOwnershipStartup|Name not found|Find in Tree/.test(x)).slice(0,20));"
> }
> ```
>
> ```text
> [
>   '\t\t61201 대화 상자 Find in Tree Secondary Actions: Raise',
>   '\t\t\t\t\t\t\t\t61357 콘솔 트리 (selectable) ProcessAxisOwnershipStartup',
>   '\t\t61201 대화 상자 Find in Tree Secondary Actions: Raise'
> ]
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
>   "title": "PRIVATE Process 검색 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:382,y:87,mouse_button:\"left\"});"
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
>   "title": "PRIVATE Process 구현 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "PRIVATE Process 구현 smoke 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:globalThis.state.window.title,status:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/ProcessAxisOwnershipStartup/.test(x)).slice(0,4)});"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*',
>   status: [
>     '\t\t\t\t\t\t\t\t61357 콘솔 트리 (selectable) ProcessAxisOwnershipStartup'
>   ]
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> 세 번째 smoke에서 트리 포커스가 편집기로 돌아가 Enter가 빈 줄을 삽입했습니다. 즉시 Undo해서 원복한 뒤, 선택된 PRIVATE 함수는 컨텍스트 메뉴의 `Edit Method`로 열겠습니다. 저장은 하지 않겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "실수 입력 즉시 Undo",
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
>   "title": "Undo 원복 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:globalThis.state.window.title,first:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").find(x=>x.includes(\"FUNCTION GLOBAL LMCControlCommandService::ReconcileAxisOwnershipStartup\"))||\"\"});"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*',
>   first: ''
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
>   "title": "PRIVATE Process 컨텍스트 메뉴 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:865,y:241,mouse_button:\"right\"});"
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
>   "title": "Edit Method 명령 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,650));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise
> 		64083 창
> 		64084 창
> 		64085 메뉴
> 			64488 메뉴 항목 Edit Method Enter
> 			64489 메뉴 항목 Delete Method Del
> 			64490 메뉴 항목 Move Up
> 			64491 메뉴 항목 (disabled) Move Down
> 			64492 메뉴 항목 Copy

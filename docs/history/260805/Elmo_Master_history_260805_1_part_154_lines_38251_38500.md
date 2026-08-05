>   "title": "Control 메서드 펼치기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:720,y:198,mouse_button:\"left\"});"
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
>   "title": "Control 메서드 트리 펼치기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:720,y:202,mouse_button:\"left\"});"
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
>   "title": "Control GLOBAL 메서드 펼치기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:736,y:219,mouse_button:\"left\"});"
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
>   "title": "Control 메서드 검색 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+SHIFT+T\"});"
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
>   "title": "Control 메서드 검색 설정",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:307,y:87,mouse_button:\"left\"});"
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
>   "title": "Method 검색 목록 열기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:307,y:87,mouse_button:\"left\"});"
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
>   "title": "Method 검색 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:102,y:198,mouse_button:\"left\"});"
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
>   "title": "Method 검색 적용 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,750));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		56811 대화 상자 Find in Tree Secondary Actions: Raise
> 			56812 텍스트 Find:
> 			56813 콤보 상자 Find: ID: 1106
> 				56814 편집 Find: ID: 1001
> 				56815 단추 열기 ID: DropDown
> 			56816 텍스트 What:
> 			56817 콤보 상자 What: ID: 1112
> 				56818 텍스트 What:
> 				56819 단추 열기 ID: DropDown
> 			56820 텍스트 Where:
> 			56821 콤보 상자 Where: ID: 1102
> 				56822 텍스트 Where:
> 				56823 단추 열기 ID: DropDown
> 			56824 확인란 Match whole word only ID: 1108
> 			56825 그룹 Direction
> 			56826 라디오 단추 (selectable) Up ID: 1110
> 			56827 라디오 단추 (selectable) Down ID: 1111
> 			56828 단추 Find Next ID: 1107
> 			5
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
>   "title": "검색 대상 다음 항목 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "검색 대상 이동 확인",
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
>   "title": "검색 대상 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "검색 대상 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "검색 대상 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "검색 대상 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
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
>   "title": "Method 검색 대상 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"});"
> }
> ```

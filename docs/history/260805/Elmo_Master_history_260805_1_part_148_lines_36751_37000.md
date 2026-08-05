> 						21412 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21413 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21414 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				35688 단추 (disabled) Toggle bookmark
> 				35689 단추 (disabled) Previous bookma
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
>   "title": "트리 검색 닫기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:382,y:87,mouse_button:\"left\"});"
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
>   "title": "LMCEcatInputLatch 메서드 펼치기",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:720,y:201,mouse_button:\"left\"});"
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
>   "title": "대화상자 상태 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({dialog:(globalThis.state.accessibility?.tree||\"\").includes(\"대화 상자 Find in Tree\"),shots:globalThis.state.screenshots?.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))});"
> }
> ```
>
> ```text
> {
>   dialog: true,
>   shots: [
>     { id: 'screenshot-0', z: 0, w: 970, h: 512 },
>     { id: 'screenshot-1', z: 1, w: 960, h: 502 },
>     { id: 'screenshot-2', z: 2, w: 448, h: 225 }
>   ]
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
>   "title": "트리 검색 대화상자 닫기",
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
>   "title": "메서드 트리 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({dialog:(globalThis.state.accessibility?.tree||\"\").includes(\"대화 상자 Find in Tree\"),tree:(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCEcatInputLatch|Methods|Global|Private/.test(x)).slice(0,30)});"
> }
> ```
>
> ```text
> {
>   dialog: false,
>   tree: [
>     '\t\t\t\t\t21379 콘솔 트리 (selectable) LMCEcatInputLatch',
>     '\t\t\t\t\t\t35590 콘솔 트리 (selectable) Methods',
>     '\t\t\t\t\t\t\t36565 콘솔 트리 (selectable) Global',
>     '\t\t\t\t\t\t\t36566 콘솔 트리 (selectable) Private',
>     '\t\t\t37674 탭 항목 (selectable) Global'
>   ]
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
>   "title": "GLOBAL 메서드 펼치기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:736,y:219,mouse_button:\"left\"});"
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
>   "title": "GLOBAL 함수 목록 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/CopyAxisOwnership|Copy|Global|Private/.test(x)).slice(0,60));"
> }
> ```
>
> ```text
> [
>   '\t\t\t\t39185 단추 Copy (Strg+C)',
>   '\t\t\t\t\t\t\t36565 콘솔 트리 (selectable) Global',
>   '\t\t\t\t\t\t\t\t38920 콘솔 트리 (selectable) CopySnapshot',
>   '\t\t\t\t\t\t\t\t38921 콘솔 트리 (selectable) CopyTopologyIoSnapshot',
>   '\t\t\t\t\t\t\t\t38927 콘솔 트리 (selectable) CopyAxisZeroHomeResult',
>   '\t\t\t\t\t\t\t\t38928 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot',
>   '\t\t\t\t\t\t\t36566 콘솔 트리 (selectable) Private',
>   '\t\t\t39208 탭 항목 (selectable) Global'
> ]
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
>   "title": "새 GLOBAL 함수 찾기",
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
>   "title": "함수 검색 대화상자 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({dialog:(globalThis.state.accessibility?.tree||\"\").includes(\"대화 상자 Find in Tree\"),shots:globalThis.state.screenshots?.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))});"
> }
> ```
>
> ```text
> {
>   dialog: true,
>   shots: [
>     { id: 'screenshot-0', z: 0, w: 970, h: 512 },
>     { id: 'screenshot-1', z: 1, w: 960, h: 502 },
>     { id: 'screenshot-2', z: 2, w: 448, h: 225 }
>   ]
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
>   "title": "트리 검색 종류 선택",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots.find(s=>s.zIndex===2).id,x:304,y:87,mouse_button:\"left\"});"
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
>   "title": "검색 대상 목록 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,2200));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		39846 대화 상자 Find in Tree Secondary Actions: Raise
> 			39847 텍스트 Find:
> 			39848 콤보 상자 Find: ID: 1106
> 				39849 편집 Find: ID: 1001
> 				39850 단추 열기 ID: DropDown
> 			39851 텍스트 What:
> 			39852 콤보 상자 What: ID: 1112

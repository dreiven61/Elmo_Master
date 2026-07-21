>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 4461624,
>     "title": "Windows-SSD (C:) - 파일 탐색기"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 19207442,
>     "title": "Elmo_manual - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 3216470,
>     "title": "LasalApiWpfTestApp - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 266798,
>     "title": "02_Example_Program - 파일 탐색기 [ main ↓0 ↑0 ]"
>   },
>   {
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "id": 69598,
>     "title": "EtherCAT Controller(Master) 요구 사양.xlsx  [제한된 보기] - Excel"
>   },
>   {
>     "app": "process:C:\\Program Files\\GitExtensions\\GitExtensions.exe",
>     "id": 134470,
>     "title": "01.WaferTransferRobot (ssh_jc_manual_update) - Git Extensions"
>   },
>   {
>     "app": "process:C:\\Program Files\\Microsoft Office\\Office16\\EXCEL.EXE",
>     "id": 200664,
>     "title": "Motion_Controller_API_정리.xlsx  [제한된 보기] - Excel"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\IPMate\\ipmate.exe",
>     "id": 1902696,
>     "title": "아이피 메이트 v2.1.2.1  [선택 어댑터 : 이더넷]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 2299272,
>     "title": "MMCLibDotNET_Test - Microsoft Visual Studio"
>   },
>   {
>     "app": "process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
>     "id": 788248,
>     "title": "ctrlX PLC 매뉴얼 스터디 노트 - Chrome"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url); nodeRepl.write(JSON.stringify({text:state.text,controls:(state.controls||[]).slice(0,80)},null,2));"
> }
> ```
>
> ```text
> {
>   "controls": []
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
>   "title": "LASAL 입력 도구 확인",
>   "code": "nodeRepl.write(Object.keys(sky).sort().join('\\n'));"
> }
> ```
>
> ```text
> activate_window
> click
> drag
> get_window
> get_window_state
> launch_app
> list_apps
> list_windows
> perform_secondary_action
> press_key
> scroll
> set_value
> transport
> type_text
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변경 저장",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:false}); targetWindow=state.window; var saveRes=await sky.press_key({window:targetWindow,key:'CTRL+S'}); state=null; nodeRepl.write(JSON.stringify(saveRes));"
> }
> ```
>
> ```text
> get_window_state must request include_text, include_screenshot, or both
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변경 저장",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var saveRes2=await sky.press_key({window:targetWindow,key:'CTRL+S'}); state=null; nodeRepl.write(JSON.stringify(saveRes2));"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 프로젝트 저장",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var saveRes3=await sky.press_key({window:targetWindow,key:'Ctrl_L+S'}); state=null; nodeRepl.write(JSON.stringify(saveRes3));"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 저장 버튼 실행",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; var saveClick=await sky.click({window:targetWindow,x:110,y:70}); state=null; nodeRepl.write(JSON.stringify(saveClick));"
> }
> ```
>
> ```text
> undefined
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
>   "title": "LASAL client 편집 확정",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var commitClient=await sky.press_key({window:targetWindow,key:'Return'}); state=null; nodeRepl.write(JSON.stringify(commitClient));"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL client 저장",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var saveClient=await sky.press_key({window:targetWindow,key:'Ctrl_L+S'}); state=null; nodeRepl.write(JSON.stringify(saveClient));"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL client 속성 적용",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; var applyClient=await sky.click({window:targetWindow,x:1030,y:358}); state=null; nodeRepl.write(JSON.stringify(applyClient));"
> }
> ```
>
> ```text
> undefined
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
>   "title": "LASAL 선택 결과 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); targetWindow=state.window; if(state.screenshots&&state.screenshots[0]) await nodeRepl.emitImage(state.screenshots[0].url); nodeRepl.write(JSON.stringify({text:state.text,controls:(state.controls||[]).slice(-40)},null,2));"
> }
> ```
>
> ```text
> {
>   "controls": []
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
>   "title": "LASAL class metadata 저장",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); targetWindow=state.window; var saveClick2=await sky.click({window:targetWindow,x:110,y:70}); state=null; nodeRepl.write(JSON.stringify(saveClick2));"
> }
> ```
>

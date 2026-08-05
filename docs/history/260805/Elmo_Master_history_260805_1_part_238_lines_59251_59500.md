>   "code": "await sky.click({window:targetWindow,x:1075,y:226}); nodeRepl.write(\"selected Variables\");"
> }
> ```
>
> ```text
> selected Variables
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수 목록 선택 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(\"fresh\");"
> }
> ```
>
> ```text
> fresh
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
>   "title": "LASAL 배열 변수 빠른 복제",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+v\"}); nodeRepl.write(\"pasted via keyboard\");"
> }
> ```
>
> ```text
> pasted via keyboard
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 빠른 복제 결과 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").match(/OwnershipLeaseState\\d*/g)?.join(\"\\n\")||\"none\");"
> }
> ```
>
> ```text
> OwnershipLeaseState
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
>   "title": "LASAL identity 배열 복제 메뉴",
>   "code": "await sky.click({window:targetWindow,x:1063,y:226,mouse_button:\"right\"}); nodeRepl.write(\"opened menu\");"
> }
> ```
>
> ```text
> opened menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 복제 메뉴 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").slice(0,500));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise
> 		47784 창
> 		47785 창
> 		47786 메뉴
> 			48152 메뉴 항목 New Variable
> 			48153 메뉴 항목 Sort Variables
> 			48154 메뉴 항목 Paste
> 			48155 메뉴 항목 Copy
> 		1 창 작업 영역 ID: 59648
> 			1224 창 LMCControlCommandService* Secondary Actions: Raise ID: 65284
> 				1225 창 ID: 59648
> 					1226 창 #define LMC_ADMIN_AX
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
>   "title": "LASAL identity 배열 복제",
>   "code": "await sky.click({window:targetWindow,x:1108,y:289}); nodeRepl.write(\"pasted copy\");"
> }
> ```
>
> ```text
> pasted copy
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL identity 복제 결과 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").match(/Ownership[^\\n]*/g)?.join(\"\\n\")||\"\");"
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
>   "title": "LASAL 변수 목록 펼치기",
>   "code": "await sky.click({window:targetWindow,x:1010,y:222}); nodeRepl.write(\"expanded Variables\");"
> }
> ```
>
> ```text
> expanded Variables
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL identity 복제 항목 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").match(/Ownership[^\\n]*/g)?.join(\"\\n\")||\"\");"
> }
> ```
>
> ```text
> OwnershipState
> OwnershipStartupState
> OwnershipObserverState
> OwnershipLeaseState
> OwnershipPreemptedState
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
>   "title": "LASAL source 배열 다시 복사",
>   "code": "await sky.click({window:targetWindow,x:1128,y:363,mouse_button:\"right\"}); nodeRepl.write(\"opened Lease context\");"
> }
> ```
>
> ```text
> opened Lease context
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL source 배열 메뉴 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").slice(0,450));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise
> 		53325 창
> 		53326 창
> 		53327 메뉴
> 			53738 메뉴 항목 Delete Variable Del
> 			53739 메뉴 항목 Move Up
> 			53740 메뉴 항목 Move Down
> 			53741 메뉴 항목 New Dimension
> 			53742 메뉴 항목 (disabled) Go to Datatype
> 			53743 메뉴 항목 Add Server for Visu
> 			5374
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
>   "title": "LASAL 기준 배열 복사",
>   "code": "await sky.click({window:targetWindow,x:1172,y:518}); nodeRepl.write(\"copied 324-word array\");"
> }
> ```
>
> ```text
> copied 324-word array
> ```
>
> MCP tool call
>
> node_repl.js

> 					259 스크롤 막대 ID: 59904
> 						260 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			263 탭 항목 (selectable) Python Script
> 			264 탭 항목 (selectable) Output
> 			265 탭 항목 (selectable) Debugger
> 			266 단추 Close
> 		267 창 Splitter ID: 125724144
> 		268 Tab Class View ID: 125483184
> 			269 트리 ID: 103
> 				270 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					271 단추 위쪽 스크롤 화살표 ID: UpButton
> 					272 위치 조정 위치 ID: ScrollbarThumb
> 					273 단추 페이지 아래로 ID: DownPageButton
> 					274 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				275 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					276 콘솔 트리 (selectable) External
> 					277 콘솔 트리 (selectable) Sigmatek
> 					278 콘솔 트리 (selectable) _TCPIPServer_RT
> 					279 콘솔 트리 (selectable) Elmo_1
> 					280 콘솔 트리 (selectable) Elmo_2
> 					281 콘솔 트리 (selectable) Elmo_3
> 					282 콘솔 트리 (selectable) Elmo_4
> 					283 콘솔 트리 (selectable) LMCDiagnosticsService
> 					284 콘솔 트리 (selectable) LMCEcatInputLatch
> 						285 콘솔 트리 (selectable) Servers
> 						286 콘솔 트리 (selectable) Clients
> 							287 콘솔 트리 (selectable) EcatMaster
> 							288 콘솔 트리 (selectable) Drive1
> 							289 콘솔 트리 (selectable) Drive2
> 							290 콘솔 트리 (selectable) Drive3
> 							291 콘솔 트리 (selectable) Drive4
> 						292 콘솔 트리 (selectable) Methods
> 						293 콘솔 트리 (selectable) Dependencies
> 					294 콘솔 트리 (selectable) TCPMotionInterface
> 			295 탭 항목 (selectable) Lib
> 			296 탭 항목 (selectable) File
> 			297 탭 항목 (selectable) Global
> 			298 탭 항목 (selectable) Net
> 			299 탭 항목 (selectable) Class
> 			300 단추 Close
> 		301 Tab Properties ID: 125485008
> 			302 창 ID: 290002192
> 				303 TABLE Properties Window ID: 293314152
> 					304 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						305 단추 위쪽 스크롤 화살표 ID: UpButton
> 						306 위치 조정 위치 ID: ScrollbarThumb
> 						307 단추 페이지 아래로 ID: DownPageButton
> 						308 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					309 custom Name
> 					310 custom Class
> 					311 custom DataType
> 					312 custom Type
> 					313 custom Required
> 					314 custom DefaultInitValue
> 					315 custom Comment
> 				316 도구 모음 ID: 59392
> 					317 단추
> 					318 단추
> 			319 탭 항목 (selectable) Properties
> 			320 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
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
>   "title": "Drive1 클래스 항목 열기",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:310}); globalThis.state=null; nodeRepl.write('class row selected');"
> }
> ```
>
> ```text
> class row selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 클래스 콤보 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 303 TABLE Properties Window ID: 293314152
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
>   "title": "Drive1 클래스 목록 열기",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Alt_L+Down'}); globalThis.state=null; nodeRepl.write('class list opened');"
> }
> ```
>
> ```text
> class list opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 클래스 목록 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 클래스 목록 이동 1",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Down'}); globalThis.state=null; nodeRepl.write('moved to EEPROM');"
> }
> ```
>
> ```text
> moved to EEPROM
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 클래스 선택 위치 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 1 편집
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 클래스 목록 이동 2",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Down'}); globalThis.state=null; nodeRepl.write('moved to Elmo_1');"
> }
> ```
>
> ```text
> moved to Elmo_1
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 Elmo_1 선택 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 1 편집
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 Elmo_1 적용",
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:'Return'}); globalThis.state=null; nodeRepl.write('Elmo_1 applied');"
> }
> ```
>
> ```text
> Elmo_1 applied
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Drive1 적용 결과 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				3 창 ID: 59648
> 					4 창 xtpBarTop ID: 59419
> 						5 도구 모음 Hardware Editor
> 							6 단추 Hardware Editor Configuration Settings
> 							7 단추 Calculate Resources of Project

> 							317 콘솔 트리 (selectable) ParaType
> 							318 콘솔 트리 (selectable) ParaLength
> 							319 콘솔 트리 (selectable) ParaValue
> 							320 콘솔 트리 (selectable) ParaString
> 							321 콘솔 트리 (selectable) ErrorCode
> 							322 콘솔 트리 (selectable) Timeout
> 							323 콘솔 트리 (selectable) ClassSvr
> 						324 콘솔 트리 (selectable) Methods
> 						325 콘솔 트리 (selectable) Variables
> 						326 콘솔 트리 (selectable) Types
> 						327 콘솔 트리 (selectable) Network
> 						328 콘솔 트리 (selectable) Dependencies
> 					329 콘솔 트리 (selectable) TCPMotionInterface
> 			330 탭 항목 (selectable) Lib
> 			331 탭 항목 (selectable) File
> 			332 탭 항목 (selectable) Global
> 			333 탭 항목 (selectable) Net
> 			334 탭 항목 (selectable) Class
> 			335 단추 Close
> 		336 Tab Properties ID: 251700040
> 			337 창 ID: 244074672
> 				338 TABLE Properties Window ID: 249456424
> 					339 custom Name
> 					340 custom GUID
> 					341 custom Visualized
> 					342 custom World
> 					343 custom Alarm
> 					344 custom DataType
> 					345 custom Type
> 					346 custom Initialize
> 					347 custom WriteProtected
> 					348 custom Retentive
> 					349 custom Comment
> 				350 도구 모음 ID: 59392
> 					351 단추
> 					352 단추
> 			353 탭 항목 (selectable) Properties
> 			354 단추 Close
>
> The focused UI element is 280 트리 ID: 103.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 클래스 변경 저장",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+Shift_L+s\"}); await new Promise(r=>setTimeout(r,1400)); state=await sky.get_window_state({window:targetWindow,include_text:true}); var tx=state.accessibility?.tree||\"\"; nodeRepl.write(tx.includes(\"Save project changes\") ? \"save command sent\" : tx.slice(-1200));"
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
>   "title": "LASAL 프로젝트 저장",
>   "code": "await sky.press_key({window:targetWindow,key:\"Ctrl+Shift+s\"}); await new Promise(r=>setTimeout(r,1600)); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").includes(\"Comm_Network *\") ? \"network still dirty\" : \"save completed\");"
> }
> ```
>
> ```text
> save completed
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
>   "title": "LASAL 옵션 창 닫기",
>   "code": "await sky.click({window:targetWindow,x:1590,y:198}); await new Promise(r=>setTimeout(r,500)); state=await sky.get_window_state({window:targetWindow,include_text:false}); nodeRepl.write(\"closed\");"
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 창 상태 동기화",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((state.window?.title||\"\")+\"\\n\"+(state.accessibility?.focused_element||\"\"));"
> }
> ```
>
> ```text
> Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - EtherCAT_SDOBase.st [RO]
> 1 대화 상자 Application Options Secondary Actions: Raise
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 전체 재빌드",
>   "code": "await sky.press_key({window:targetWindow,key:\"Ctrl+F9\"}); await new Promise(r=>setTimeout(r,5000)); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").slice(-2500));"
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 빌드 상태 확인",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_text:true}); var a=state.accessibility?.tree||\"\"; nodeRepl.write(a.slice(-2500));"
> }
> ```
>
> ```text
> le) Sigmatek
> 						293 콘솔 트리 (selectable) OSInterface
> 						294 콘솔 트리 (selectable) Hardware
> 							295 콘솔 트리 (selectable) Communication
> 								296 콘솔 트리 (selectable) EtherCAT
> 									297 콘솔 트리 (selectable) Internal
> 									298 콘솔 트리 (selectable) EtherCAT_SDOBase
> 							299 콘솔 트리 (selectable) ControlSystems
> 							300 콘솔 트리 (selectable) Internal
> 							301 콘솔 트리 (selectable) Manager
> 						302 콘솔 트리 (selectable) System
> 						303 콘솔 트리 (selectable) Tools
> 					304 콘솔 트리 (selectable) _TCPIPServer_RT
> 					305 콘솔 트리 (selectable) Elmo_1
> 					306 콘솔 트리 (selectable) Elmo_2
> 					307 콘솔 트리 (selectable) Elmo_3
> 					308 콘솔 트리 (selectable) Elmo_4
> 					309 콘솔 트리 (selectable) LMCDiagnosticsService
> 					310 콘솔 트리 (selectable) LMCEcatInputLatch
> 					311 콘솔 트리 (selectable) LMCRecorderStore
> 					312 콘솔 트리 (selectable) LMCSdoExecutor
> 						313 콘솔 트리 (selectable) Base
> 							314 콘솔 트리 (selectable) EtherCAT_SDOBase
> 								315 콘솔 트리 (selectable) Servers
> 								316 콘솔 트리 (selectable) Clients
> 								317 콘솔 트리 (selectable) Methods
> 								318 콘솔 트리 (selectable) Variables
> 								319 콘솔 트리 (selectable) Network
> 								320 콘솔 트리 (selectable) Files
> 								321 콘솔 트리 (selectable) Objects
> 								322 콘솔 트리 (selectable) Dependencies
> 						323 콘솔 트리 (selectable) Servers
> 							324 콘솔 트리 (selectable) ClassState
> 							325 콘솔 트리 (selectable) ParaReadWrite
> 							326 콘솔 트리 (selectable) ParaIndex
> 							327 콘솔 트리 (selectable) ParaSubIndex
> 							328 콘솔 트리 (selectable) CompleteAccess
> 							329 콘솔 트리 (selectable) ParaType
> 							330 콘솔 트리 (selectable) ParaLength
> 							331 콘솔 트리 (selectable) ParaValue
> 							332 콘솔 트리 (selectable) ParaString
> 							333 콘솔 트리 (selectable) ErrorCode
> 							334 콘솔 트리 (selectable) Timeout
> 							335 콘솔 트리 (selectable) ClassSvr
> 						336 콘솔 트리 (selectable) Methods
> 						337 콘솔 트리 (selectable) Variables
> 						338 콘솔 트리 (selectable) Types
> 						339 콘솔 트리 (selectable) Network
> 							340 콘솔 트리 (selectable) LMCSdoExecutor
> 						341 콘솔 트리 (selectable) Dependencies
> 					342 콘솔 트리 (selectable) TCPMotionInterface
> 			343 탭 항목 (selectable) Lib
> 			344 탭 항목 (selectable) File
> 			345 탭 항목 (selectable) Global
> 			346 탭 항목 (selectable) Net
> 			347 탭 항목 (selectable) Class
> 			348 단추 Close
> 		349 Tab Properties ID: 251700040
> 			350 창 ID: 244074672
> 				351 TABLE Properties Window ID: 249456424
> 					352 custom Name
> 					353 custom Draw Connection
> 				354 도구 모음 ID: 59392
> 					355 단추
> 					356 단추
> 			357 탭 항목 (selectable) Properties
> 			358 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 전체 재빌드",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control+F9\"}); await new Promise(r=>setTimeout(r,6000)); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((state.accessibility?.tree||\"\").slice(-3000));"
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> derive Class 하면 내부 네트워크 원하는 형태로 수정해야돼.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 빌드 완료 대기",
>   "code": "await new Promise(r=>setTimeout(r,12000)); state=await sky.get_window_state({window:targetWindow,include_text:true,include_screenshot:false}); var tree=state.accessibility?.tree||\"\"; nodeRepl.write(tree.slice(-4500));"
> }
> ```
>
> ```text
>
> 						341 목록 항목 (selectable)
> 						342 목록 항목 (selectable)
> 						343 목록 항목 (selectable)
> 						344 목록 항목 (selectable)
> 						345 목록 항목 (selectable)
> 						346 목록 항목 (selectable)
> 						347 목록 항목 (selectable)
> 						348 목록 항목 (selectable)
> 						349 목록 항목 (selectable)

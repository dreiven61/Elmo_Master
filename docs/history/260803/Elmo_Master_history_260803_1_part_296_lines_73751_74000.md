>   "code": "await sky.click({window:targetWindow,x:139,y:42}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(state.accessibility?.tree?.slice(0,700)||state); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network *", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network * Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			44 창 Motion_Network * Secondary Actions: Raise ID: 65284
> 				45 창 ID: 59648
> 					125609 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						125610 단추 위쪽 스크롤 화살표 ID: UpButton
> 						125611 단추 페이지 위로 ID: UpPageButton
> 						125612 위치 조정 위치 ID: ScrollbarThumb
> 						125613 단추 페이지 아래로 ID: DownPageButton
> 						125614 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					125615 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						125616 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						125617 단추 페이지 왼쪽으로 ID: UpPageBut
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
>   "title": "Activate LASAL select",
>   "code": "await sky.click({window:targetWindow,x:158,y:66}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(state.accessibility?.focused_element||\"select active\"); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 45 창 ID: 59648
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
>   "title": "Open axis 1 control menu",
>   "code": "await sky.click({window:targetWindow,x:532,y:180,mouse_button:\"right\"}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(state.accessibility?.tree?.slice(0,1600)||state); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network *", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network * Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			44 창 Motion_Network * Secondary Actions: Raise ID: 65284
> 				45 창 ID: 59648
> 					125609 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						125610 단추 위쪽 스크롤 화살표 ID: UpButton
> 						125611 단추 페이지 위로 ID: UpPageButton
> 						125612 위치 조정 위치 ID: ScrollbarThumb
> 						125613 단추 페이지 아래로 ID: DownPageButton
> 						125614 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					125615 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						125616 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						125617 단추 페이지 왼쪽으로 ID: UpPageButton
> 						125618 위치 조정 위치 ID: ScrollbarThumb
> 						125619 단추 페이지 오른쪽으로 ID: DownPageButton
> 						125620 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					125621 위치 조정 (disabled)
> 			29 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65288
> 				30 창 ID: 59648
> 					31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderRes
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
>   "title": "Dismiss axis menu",
>   "code": "await sky.press_key({window:targetWindow,key:\"ESC\"}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(state.accessibility?.focused_element||\"dismissed\"); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 45 창 ID: 59648
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
>   "title": "Open axis 1 server value menu",
>   "code": "await sky.click({window:targetWindow,x:547,y:191,mouse_button:\"right\"}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(state.accessibility?.tree?.slice(0,1300)||state); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network *", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network * Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			44 창 Motion_Network * Secondary Actions: Raise ID: 65284
> 				45 창 ID: 59648
> 					125609 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						125610 단추 위쪽 스크롤 화살표 ID: UpButton
> 						125611 단추 페이지 위로 ID: UpPageButton
> 						125612 위치 조정 위치 ID: ScrollbarThumb
> 						125613 단추 페이지 아래로 ID: DownPageButton
> 						125614 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					125615 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						125616 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						125617 단추 페이지 왼쪽으로 ID: UpPageButton
> 						125618 위치 조정 위치 ID: ScrollbarThumb
> 						125619 단추 페이지 오른쪽으로 ID: DownPageButton
> 						125620 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					125621 위치 조정 (disabled)
> 			29 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65288
> 				30 창 ID: 59648
> 					31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UD
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
>   "title": "Dismiss object menu",
>   "code": "await sky.press_key({window:targetWindow,key:\"ESC\"}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(\"dismissed\"); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> dismissed
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
>   "title": "Choose tighter network zoom",
>   "code": "await sky.click({window:targetWindow,x:139,y:42}); state=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(\"menu\"); for(const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> menu
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
> Image output: image/jpeg
>
> Image output: image/jpeg

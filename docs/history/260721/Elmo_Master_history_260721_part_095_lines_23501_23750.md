> 							288 콘솔 트리 (selectable) Global
> 							290 콘솔 트리 (selectable) Private
> 					291 콘솔 트리 (selectable) LMCEcatInputLatch
> 						299 콘솔 트리 (selectable) Methods
> 			304 탭 항목 (selectable) Global
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
>   "title": "Close properties pane",
>   "code": "await sky.click({window:targetWindow,x:1427,y:562,screenshotId:state.screenshots[0].id}); state=null; nodeRepl.write('closed');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> closed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Show full class tree",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>x.includes('LMCEcatInputLatch')||x.includes('Methods')||x.includes('RtWork')||x.includes('Global')||x.includes('Private')).slice(-20).join('\\n')); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 					149 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						281 콘솔 트리 (selectable) Methods
> 							282 콘솔 트리 (selectable) Global
> 							284 콘솔 트리 (selectable) Private
> 					285 콘솔 트리 (selectable) LMCEcatInputLatch
> 						293 콘솔 트리 (selectable) Methods
> 			298 탭 항목 (selectable) Global
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
>   "title": "Expand RT latch methods",
>   "code": "await sky.click({window:targetWindow,element_index:293,click_count:2}); state=null; nodeRepl.write('expanded');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> expanded
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect RT method folders",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>x.includes('LMCEcatInputLatch')||x.includes('Methods')||x.includes('RtWork')||x.includes('Global')||x.includes('Private')).slice(-20).join('\\n')); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 					149 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						286 콘솔 트리 (selectable) Methods
> 							287 콘솔 트리 (selectable) Global
> 							289 콘솔 트리 (selectable) Private
> 					290 콘솔 트리 (selectable) LMCEcatInputLatch
> 						298 콘솔 트리 (selectable) Methods
> 							299 콘솔 트리 (selectable) Global
> 							300 콘솔 트리 (selectable) Private
> 			305 탭 항목 (selectable) Global
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
>   "title": "Select RT global methods",
>   "code": "await sky.click({window:targetWindow,element_index:299}); state=null; nodeRepl.write('selected');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> selected
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open RT method menu",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write('ready');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> ready
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open RT method menu",
>   "code": "await sky.press_key({window:targetWindow,key:'Shift_L+F10'}); state=null; nodeRepl.write('opened');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Create snapshot method",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').slice(0,12).join('\\n')); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴
> 			4 메뉴 항목 New Method Ins
> 			5 메뉴 항목 New Std. Methods
> 			6 메뉴 항목 Paste
> 			7 메뉴 항목 Copy
> 		8 창 작업 영역 ID: 59648
> 			9 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				10 창 ID: 59648
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
>   "title": "Create snapshot method",
>   "code": "await sky.click({window:targetWindow,element_index:4}); state=null; nodeRepl.write('created');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> created
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Name snapshot method",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.focused_element||'');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 269 편집 ID: 1
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Name snapshot method",
>   "code": "await sky.type_text({window:targetWindow,text:'CopySnapshot'}); state=null; nodeRepl.write('typed');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> typed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify snapshot method name",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.focused_element||''); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>

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
>   "title": "Read global LASAL method list",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var tg7=globalThis.state.accessibility.tree.lastIndexOf('Global'); nodeRepl.write(globalThis.state.accessibility.tree.slice(tg7,tg7+1200));"
> }
> ```
>
> ```text
> Global
> 								38307 콘솔 트리 (selectable) RtWork
> 								38308 콘솔 트리 (selectable) CopySnapshot
> 								38309 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 								38310 콘솔 트리 (selectable) AdvanceOutputRevision
> 								38311 콘솔 트리 (selectable) SubmitDs402HomeControl
> 								38312 콘솔 트리 (selectable) GetDs402HomeControlState
> 							37503 콘솔 트리 (selectable) Private
> 						18376 콘솔 트리 (selectable) Variables
> 							18377 콘솔 트리 (selectable) PublishSequence
> 							18378 콘솔 트리 (selectable) SnapshotBytes
> 								36041 콘솔 트리 (selectable) 0..511
> 							18379 콘솔 트리 (selectable) OutputRevision
> 							18380 콘솔 트리 (selectable) OutputObserved
> 							18381 콘솔 트리 (selectable) OutputPreviousValid
> 							18382 콘솔 트리 (selectable) OutputPreviousValue
> 							18796 콘솔 트리 (selectable) Ds402HomeRequestSequence
> 							28164 콘솔 트리 (selectable) Ds402HomeAppliedSequence
> 						18384 콘솔 트리 (selectable) Objects
> 						18385 콘솔 트리 (selectable) Dependencies
> 					9435 콘솔 트리 (selectable) LMCRecorderStore
> 					9436 콘솔 트리 (selectable) LMCSdoExecutor
> 					9437 콘솔 트리 (selectable) TCPIPServer
> 					9438 콘솔 트리 (selectable) TCPMotionInterface
> 			38532 탭 항목 (selectable) Lib
> 			38533 탭 항목 (selectable) File
> 			38534 탭 항목 (selectable) Cla
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
>   "title": "Expand DS402 control method",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1230,y:318,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Read DS402 control method declaration",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var sm8=globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl'); nodeRepl.write(globalThis.state.accessibility.tree.slice(sm8,sm8+900));"
> }
> ```
>
> ```text
> SubmitDs402HomeControl
> 								38312 콘솔 트리 (selectable) GetDs402HomeControlState
> 							37503 콘솔 트리 (selectable) Private
> 						18376 콘솔 트리 (selectable) Variables
> 							18377 콘솔 트리 (selectable) PublishSequence
> 							18378 콘솔 트리 (selectable) SnapshotBytes
> 								36041 콘솔 트리 (selectable) 0..511
> 							18379 콘솔 트리 (selectable) OutputRevision
> 							18380 콘솔 트리 (selectable) OutputObserved
> 							18381 콘솔 트리 (selectable) OutputPreviousValid
> 							18382 콘솔 트리 (selectable) OutputPreviousValue
> 							18796 콘솔 트리 (selectable) Ds402HomeRequestSequence
> 							28164 콘솔 트리 (selectable) Ds402HomeAppliedSequence
> 						18384 콘솔 트리 (selectable) Objects
> 						18385 콘솔 트리 (selectable) Dependencies
> 					9435 콘솔 트리 (selectable) LMCRecorderStore
> 					9436 콘솔 트리 (selectable) LMCSdoExecutor
> 					9437 콘솔 트리 (selectable) TCPIPServer
> 					9438 콘솔 트리 (selectable) TCPMotionInterface
> 			39658 탭 항목 (selectable) Lib
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
>   "title": "Inspect DS402 control method menu",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:38311,mouse_button:'right'});"
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
>   "title": "Read DS402 method menu",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,tail:globalThis.state.accessibility.tree.slice(-3500)});"
> }
> ```
>
> ```text
> {
>   focused: '98 트리 ID: 103',
>   tail: '\t83 창 Splitter ID: 409230608\n' +
>     '\t\t84 Tab Output ID: 303755232\n' +
>     '\t\t\t85 창 ID: 1200\n' +
>     '\t\t\t\t86 창 ID: 1200\n' +
>     '\t\t\t\t\t87 LIST ID: 1201\n' +
>     '\t\t\t\t\t\t8918 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t8919 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t8920 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t\t\t8921 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t8922 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t9603 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t9604 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23837 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23838 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23839 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23840 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23841 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t23842 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t88 스크롤 막대 ID: 59904\n' +
>     '\t\t\t\t\t\t89 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t90 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t91 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t40733 탭 항목 (selectable) Python Script\n' +
>     '\t\t\t40734 탭 항목 (selectable) Debugger\n' +
>     '\t\t\t40735 탭 항목 (selectable) Output\n' +
>     '\t\t\t95 단추 Close\n' +
>     '\t\t96 창 Splitter ID: 409228760\n' +
>     '\t\t97 Tab Class View ID: 303755688\n' +
>     '\t\t\t98 트리 ID: 103\n' +
>     '\t\t\t\t9608 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t9609 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t18372 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t9610 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t9611 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t9612 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t9422 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n' +
>     '\t\t\t\t\t9423 콘솔 트리 (selectable) External\n' +
>     '\t\t\t\t\t9424 콘솔 트리 (selectable) Sigmatek\n' +
>     '\t\t\t\t\t9425 콘솔 트리 (selectable) Elmo_1\n' +
>     '\t\t\t\t\t9426 콘솔 트리 (selectable) Elmo_2\n' +
>     '\t\t\t\t\t9427 콘솔 트리 (selectable) Elmo_3\n' +
>     '\t\t\t\t\t9428 콘솔 트리 (selectable) Elmo_4\n' +
>     '\t\t\t\t\t9429 콘솔 트리 (selectable) GL_9086_1\n' +
>     '\t\t\t\t\t9430 콘솔 트리 (selectable) GL_9086_1_Slot00\n' +
>     '\t\t\t\t\t9431 콘솔 트리 (selectable) GL_9086_1_Slot01\n' +
>     '\t\t\t\t\t9432 콘솔 트리 (selectable) LMCControlCommandService\n' +
>     '\t\t\t\t\t9433 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>     '\t\t\t\t\t9434 콘솔 트리 (selectable) LMCEcatInputLatch\n' +
>     '\t\t\t\t\t\t18373 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t18374 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t18375 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t37502 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t38307 콘솔 트리 (selectable) RtWork\n' +
>     '\t\t\t\t\t\t\t\t38308 콘솔 트리 (selectable) CopySnapshot\n' +
>     '\t\t\t\t\t\t\t\t38309 콘솔 트리 (selectable) CopyTopologyIoSnapshot\n' +
>     '\t\t\t\t\t\t\t\t38310 콘솔 트리 (selectable) AdvanceOutputRevision\n' +
>     '\t\t\t\t\t\t\t\t38311 콘솔 트리 (selectable) SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t38312 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t37503 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t18376 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t18377 콘솔 트리 (selectable) PublishSequence\n' +
>     '\t\t\t\t\t\t\t18378 콘솔 트리 (selectable) SnapshotBytes\n' +
>     '\t\t\t\t\t\t\t\t36041 콘솔 트리 (selectable) 0..511\n' +
>     '\t\t\t\t\t\t\t18379 콘솔 트리 (selectable) OutputRevision\n' +
>     '\t\t\t\t\t\t\t18380 콘솔 트리 (selectable) OutputObserved\n' +
>     '\t\t\t\t\t\t\t18381 콘솔 트리 (selectable) OutputPreviousValid\n' +
>     '\t\t\t\t\t\t\t18382 콘솔 트리 (selectable) OutputPreviousValue\n' +
>     '\t\t\t\t\t\t\t18796 콘솔 트리 (selectable) Ds402HomeRequestSequence\n' +
>     '\t\t\t\t\t\t\t28164 콘솔 트리 (selectable) Ds402HomeAppliedSequence\n' +
>     '\t\t\t\t\t\t18384 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t18385 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t9435 콘솔 트리 (selectable) LMCRecorderStore\n' +
>     '\t\t\t\t\t9436 콘솔 트리 (selectable) LMCSdoExecutor\n' +
>     '\t\t\t\t\t9437 콘솔 트리 (selectable) TCPIPServer\n' +
>     '\t\t\t\t\t9438 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t40736 탭 항목 (selectable) Lib\n' +
>     '\t\t\t40737 탭 항목 (selectable) File\n' +
>     '\t\t\t40738 탭 항목 (selectable) Class\n' +
>     '\t\t\t102 단추 Close\n' +
>     '\t\t103 Tab Properties ID: 303751584\n' +
>     '\t\t\t104 창 ID: 295129888\n' +
>     '\t\t\t\t105 TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t40739 custom Name\n' +
>     '\t\t\t\t\t40740 custom Virtual\n' +
>     '\t\t\t\t\t40741 custom Global access\n' +
>     '\t\t\t\t\t40742 custom AWL implementation\n' +
>     '\t\t\t\t\t40743 custom CDecl\n' +
>     '\t\t\t\t\t40744 custom Comment\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t40745 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 98 트리 ID: 103.'
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Add DS402 control input",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1390,y:452,screenshotId:globalThis.state.screenshots[0].id});"
> }
> ```
>

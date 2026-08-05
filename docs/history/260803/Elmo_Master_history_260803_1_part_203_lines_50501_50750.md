>     '\t\t\t\t\t27764 custom Objectsize\n' +
>     '\t\t\t\t\t27765 custom Singleton\n' +
>     '\t\t\t\t\t27766 custom Hidden\n' +
>     '\t\t\t\t\t27767 custom Deprecated\n' +
>     '\t\t\t\t\t27768 custom GCCOptions\n' +
>     '\t\t\t\t\t27769 custom Comment\n' +
>     '\t\t\t\t\t27770 custom Filename\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t27771 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\t\t27772 창 {0C15F4E1-3BD1-45B4-8655-F13049B7F4A0}\n' +
>     '\n' +
>     'The focused UI element is 27680 메뉴 항목 New Variable.'
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
>   "title": "Name LASAL mailbox variable",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'Ds402HomeAppliedSequence'});"
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
>   "title": "Verify LASAL variable name",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.slice(-2200));"
> }
> ```
>
> ```text
> 7 콘솔 트리 (selectable) Elmo_3
> 					9428 콘솔 트리 (selectable) Elmo_4
> 					9429 콘솔 트리 (selectable) GL_9086_1
> 					9430 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					9431 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					9432 콘솔 트리 (selectable) LMCControlCommandService
> 					9433 콘솔 트리 (selectable) LMCDiagnosticsService
> 					9434 콘솔 트리 (selectable) LMCEcatInputLatch
> 						18373 콘솔 트리 (selectable) Servers
> 						18374 콘솔 트리 (selectable) Clients
> 						18375 콘솔 트리 (selectable) Methods
> 						18376 콘솔 트리 (selectable) Variables
> 							18377 콘솔 트리 (selectable) PublishSequence
> 							18378 콘솔 트리 (selectable) SnapshotBytes
> 							18379 콘솔 트리 (selectable) OutputRevision
> 							18380 콘솔 트리 (selectable) OutputObserved
> 							18381 콘솔 트리 (selectable) OutputPreviousValid
> 							18382 콘솔 트리 (selectable) OutputPreviousValue
> 							18796 콘솔 트리 (selectable) Ds402HomeRequestSequence
> 							27752 콘솔 트리 (selectable) Variable
> 						18384 콘솔 트리 (selectable) Objects
> 						18385 콘솔 트리 (selectable) Dependencies
> 					9435 콘솔 트리 (selectable) LMCRecorderStore
> 					9436 콘솔 트리 (selectable) LMCSdoExecutor
> 					9437 콘솔 트리 (selectable) TCPIPServer
> 					9438 콘솔 트리 (selectable) TCPMotionInterface
> 			27918 탭 항목 (selectable) Lib
> 			27919 탭 항목 (selectable) File
> 			27920 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 303751584
> 			104 창 ID: 295129888
> 				105 TABLE Properties Window ID: 301947776
> 					26801 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						26802 단추 위쪽 스크롤 화살표 ID: UpButton
> 						26803 위치 조정 위치 ID: ScrollbarThumb
> 						26804 단추 페이지 아래로 ID: DownPageButton
> 						26805 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					27921 custom Name
> 					27922 custom Revision
> 					27923 custom GUID
> 					27924 custom Task Settings
> 					27925 custom Sigmatek
> 					27926 custom OSInterface
> 					27927 custom IconPath
> 					27928 custom SharedCommandTable
> 					27929 custom Objectsize
> 					27930 custom Singleton
> 					27931 custom Hidden
> 					27932 custom Deprecated
> 					27933 custom GCCOptions
> 					27934 custom Comment
> 					27935 custom Filename
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			27936 탭 항목 (selectable) Properties
> 			110 단추 Close
> 		27772 창 {0C15F4E1-3BD1-45B4-8655-F13049B7F4A0}
>
> The focused UI element is 27751 편집 ID: 1.
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
>   "title": "Commit LASAL variable name",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});"
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
>   "title": "Verify LASAL mailbox declaration",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,selected:globalThis.state.accessibility.selected_elements,tail:globalThis.state.accessibility.tree.slice(-2100)});"
> }
> ```
>
> ```text
> {
>   focused: '98 트리 ID: 103',
>   selected: undefined,
>   tail: '\t9609 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
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
>     '\t\t\t\t\t\t18376 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t18377 콘솔 트리 (selectable) PublishSequence\n' +
>     '\t\t\t\t\t\t\t18378 콘솔 트리 (selectable) SnapshotBytes\n' +
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
>     '\t\t\t28237 탭 항목 (selectable) Lib\n' +
>     '\t\t\t28238 탭 항목 (selectable) File\n' +
>     '\t\t\t28239 탭 항목 (selectable) Class\n' +
>     '\t\t\t102 단추 Close\n' +
>     '\t\t103 Tab Properties ID: 303751584\n' +
>     '\t\t\t104 창 ID: 295129888\n' +
>     '\t\t\t\t105 TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t28240 custom Name\n' +
>     '\t\t\t\t\t28241 custom GUID\n' +
>     '\t\t\t\t\t28242 custom Type\n' +
>     '\t\t\t\t\t28243 custom Pointer\n' +
>     '\t\t\t\t\t28244 custom Comment\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t28245 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\t\t27772 창 {0C15F4E1-3BD1-45B4-8655-F13049B7F4A0}\n' +
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Select new LASAL variable",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:28164});"
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
>   "title": "Inspect LASAL variable type",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,props:globalThis.state.accessibility.tree.match(/TABLE Properties Window[\\s\\S]{0,900}/)?.[0]});"
> }
> ```
>
> ```text
> {
>   focused: '28597 편집 ID: 1',
>   props: 'TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t28817 custom Name\n' +
>     '\t\t\t\t\t28818 custom GUID\n' +
>     '\t\t\t\t\t28819 custom Type\n' +
>     '\t\t\t\t\t28820 custom Pointer\n' +
>     '\t\t\t\t\t28821 custom Comment\n' +

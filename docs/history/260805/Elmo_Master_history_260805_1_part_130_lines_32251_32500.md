> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 메서드 트리로 이동",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:894,y:183,scrollY:-573,scrollX:0}); nodeRepl.write(\"scrolled to methods\");"
> }
> ```
>
> ```text
> scrolled to methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 메서드 루트 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-2200)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: '리 (selectable) ZeroHomeState\n' +
>     '\t\t\t\t\t\t\t106719 콘솔 트리 (selectable) OwnershipState\n' +
>     '\t\t\t\t\t\t\t107265 콘솔 트리 (selectable) OwnershipStartupState\n' +
>     '\t\t\t\t\t\t\t\t113185 콘솔 트리 (selectable) 0..15\n' +
>     '\t\t\t\t\t\t104393 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t104394 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3141 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>     '\t\t\t\t\t3142 콘솔 트리 (selectable) LMCEcatInputLatch\n' +
>     '\t\t\t\t\t\t66030 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t66031 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t66032 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t67056 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t70659 콘솔 트리 (selectable) RtWork\n' +
>     '\t\t\t\t\t\t\t\t70660 콘솔 트리 (selectable) CopySnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t75839 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t75840 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t75841 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t\t70661 콘솔 트리 (selectable) CopyTopologyIoSnapshot\n' +
>     '\t\t\t\t\t\t\t\t70662 콘솔 트리 (selectable) AdvanceOutputRevision\n' +
>     '\t\t\t\t\t\t\t\t70663 콘솔 트리 (selectable) SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t70664 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t\t70665 콘솔 트리 (selectable) SubmitDs402HomeSetpointAlignment\n' +
>     '\t\t\t\t\t\t\t\t70666 콘솔 트리 (selectable) SubmitAxisZeroHome\n' +
>     '\t\t\t\t\t\t\t\t70667 콘솔 트리 (selectable) CopyAxisZeroHomeResult\n' +
>     '\t\t\t\t\t\t\t\t70668 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t73240 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t93430 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t96123 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t67057 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t66033 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t66034 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t66035 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3143 콘솔 트리 (selectable) LMCRecorderStore\n' +
>     '\t\t\t\t\t3144 콘솔 트리 (selectable) LMCSdoExecutor\n' +
>     '\t\t\t\t\t3145 콘솔 트리 (selectable) TCPIPServer\n' +
>     '\t\t\t\t\t3146 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t118188 탭 항목 (selectable) Lib\n' +
>     '\t\t\t118189 탭 항목 (selectable) File\n' +
>     '\t\t\t118190 탭 항목 (selectable) Class\n' +
>     '\t\t\t118191 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t118192 custom Limit Low\n' +
>     '\t\t\t\t\t118193 custom Limit Low value\n' +
>     '\t\t\t\t\t118194 custom Limit High\n' +
>     '\t\t\t\t\t118195 custom Limit High value\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t118196 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 91 TABLE Properties Window ID: 272349640.'
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
>   "title": "Control 클래스 위치로 복귀",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:900,y:194,scrollY:608,scrollX:0}); nodeRepl.write(\"scrolled back to Control class\");"
> }
> ```
>
> ```text
> scrolled back to Control class
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 재확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
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
>   "title": "Control Methods 항목으로 이동",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:900,y:198,scrollY:-181,scrollX:0}); nodeRepl.write(\"scrolled one page upward\");"
> }
> ```
>
> ```text
> scrolled one page upward
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control Methods 위치 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
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
>   "title": "Control 메서드 목록 펼치기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:718,y:158,mouse_button:\"left\"}); nodeRepl.write(\"expanded Methods\");"
> }
> ```
>
> ```text
> expanded Methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 메서드 범주 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-2600)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: '086_1_Slot01\n' +
>     '\t\t\t\t\t3140 콘솔 트리 (selectable) LMCControlCommandService\n' +
>     '\t\t\t\t\t\t104389 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t104390 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t104391 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t121380 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t121381 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t104392 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t106716 콘솔 트리 (selectable) GroupMovePos\n' +
>     '\t\t\t\t\t\t\t106717 콘솔 트리 (selectable) GroupKinematicReady\n' +
>     '\t\t\t\t\t\t\t106718 콘솔 트리 (selectable) ZeroHomeState\n' +
>     '\t\t\t\t\t\t\t106719 콘솔 트리 (selectable) OwnershipState\n' +
>     '\t\t\t\t\t\t\t107265 콘솔 트리 (selectable) OwnershipStartupState\n' +
>     '\t\t\t\t\t\t\t\t113185 콘솔 트리 (selectable) 0..15\n' +
>     '\t\t\t\t\t\t104393 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t104394 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3141 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>     '\t\t\t\t\t3142 콘솔 트리 (selectable) LMCEcatInputLatch\n' +
>     '\t\t\t\t\t\t66030 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t66031 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t66032 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t67056 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t70659 콘솔 트리 (selectable) RtWork\n' +
>     '\t\t\t\t\t\t\t\t70660 콘솔 트리 (selectable) CopySnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t75839 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t75840 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t75841 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t\t70661 콘솔 트리 (selectable) CopyTopologyIoSnapshot\n' +
>     '\t\t\t\t\t\t\t\t70662 콘솔 트리 (selectable) AdvanceOutputRevision\n' +
>     '\t\t\t\t\t\t\t\t70663 콘솔 트리 (selectable) SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t70664 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t\t70665 콘솔 트리 (selectable) SubmitDs402HomeSetpointAlignment\n' +
>     '\t\t\t\t\t\t\t\t70666 콘솔 트리 (selectable) SubmitAxisZeroHome\n' +
>     '\t\t\t\t\t\t\t\t70667 콘솔 트리 (selectable) CopyAxisZeroHomeResult\n' +
>     '\t\t\t\t\t\t\t\t70668 콘솔 트리 (selectable) CopyAxisOwnershipStartupSnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t73240 콘솔 트리 (selectable) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t93430 콘솔 트리 (selectable) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t96123 콘솔 트리 (selectable) Result\n' +
>     '\t\t\t\t\t\t\t67057 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t66033 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t66034 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t66035 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t3143 콘솔 트리 (selectable) LMCRecorderStore\n' +
>     '\t\t\t\t\t3144 콘솔 트리 (selectable) LMCSdoExecutor\n' +
>     '\t\t\t\t\t3145 콘솔 트리 (selectable) TCPIPServer\n' +
>     '\t\t\t\t\t3146 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t121838 탭 항목 (selectable) Lib\n' +
>     '\t\t\t121839 탭 항목 (selectable) File\n' +
>     '\t\t\t121840 탭 항목 (selectable) Class\n' +
>     '\t\t\t121841 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t121842 custom Limit Low\n' +
>     '\t\t\t\t\t121843 custom Limit Low value\n' +
>     '\t\t\t\t\t121844 custom Limit High\n' +
>     '\t\t\t\t\t121845 custom Limit High value\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t121846 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 83 트리 ID: 103.'
> }

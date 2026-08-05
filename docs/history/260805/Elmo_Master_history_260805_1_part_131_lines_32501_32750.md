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
>   "title": "Control 글로벌 메서드 펼치기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:735,y:178,mouse_button:\"left\"}); nodeRepl.write(\"expanded Global methods\");"
> }
> ```
>
> ```text
> expanded Global methods
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 글로벌 메서드 목록 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-4000)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: 'calScrollBar\n' +
>     '\t\t\t\t\t3126 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t46351 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t3127 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t66029 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t3129 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t67806 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t67807 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t67808 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t67809 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t67810 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t67811 위치 조정 (disabled)\n' +
>     '\t\t\t\t3130 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n' +
>     '\t\t\t\t\t3131 콘솔 트리 (selectable) External\n' +
>     '\t\t\t\t\t3132 콘솔 트리 (selectable) Sigmatek\n' +
>     '\t\t\t\t\t3133 콘솔 트리 (selectable) Elmo_1\n' +
>     '\t\t\t\t\t3134 콘솔 트리 (selectable) Elmo_2\n' +
>     '\t\t\t\t\t3135 콘솔 트리 (selectable) Elmo_3\n' +
>     '\t\t\t\t\t3136 콘솔 트리 (selectable) Elmo_4\n' +
>     '\t\t\t\t\t3137 콘솔 트리 (selectable) GL_9086_1\n' +
>     '\t\t\t\t\t3138 콘솔 트리 (selectable) GL_9086_1_Slot00\n' +
>     '\t\t\t\t\t3139 콘솔 트리 (selectable) GL_9086_1_Slot01\n' +
>     '\t\t\t\t\t3140 콘솔 트리 (selectable) LMCControlCommandService\n' +
>     '\t\t\t\t\t\t104389 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t104390 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t104391 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t121380 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t122522 콘솔 트리 (selectable) HandleRequest\n' +
>     '\t\t\t\t\t\t\t\t122523 콘솔 트리 (selectable) ProcessAxisZeroHome\n' +
>     '\t\t\t\t\t\t\t\t122524 콘솔 트리 (selectable) ReserveAxisOwnership\n' +
>     '\t\t\t\t\t\t\t\t122525 콘솔 트리 (selectable) ValidateAxisOwnership\n' +
>     '\t\t\t\t\t\t\t\t122526 콘솔 트리 (selectable) CommitAxisOwnership\n' +
>     '\t\t\t\t\t\t\t\t122527 콘솔 트리 (selectable) RollbackAxisOwnership\n' +
>     '\t\t\t\t\t\t\t\t122528 콘솔 트리 (selectable) PublishAxisOwnership\n' +
>     '\t\t\t\t\t\t\t\t122529 콘솔 트리 (selectable) ReportAxisOwnershipStartup\n' +
>     '\t\t\t\t\t\t\t\t122530 콘솔 트리 (selectable) NotifyAxisOwnershipSessionClosed\n' +
>     '\t\t\t\t\t\t\t\t122531 콘솔 트리 (selectable) ProcessAxisOwnership\n' +
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
>     '\t\t\t122760 탭 항목 (selectable) Lib\n' +
>     '\t\t\t122761 탭 항목 (selectable) File\n' +
>     '\t\t\t122762 탭 항목 (selectable) Class\n' +
>     '\t\t\t122763 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t122764 custom Limit Low\n' +
>     '\t\t\t\t\t122765 custom Limit Low value\n' +
>     '\t\t\t\t\t122766 custom Limit High\n' +
>     '\t\t\t\t\t122767 custom Limit High value\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t122768 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 83 트리 ID: 103.'
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
>   "title": "Startup 소유권 메서드 찾기",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:909,y:212,scrollY:502,scrollX:0}); nodeRepl.write(\"scrolled Global methods downward\");"
> }
> ```
>
> ```text
> scrolled Global methods downward
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Startup 메서드 위치 확인",
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
>   "title": "Report 메서드 한 줄 위로 이동",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:908,y:168,scrollY:-95,scrollX:0}); nodeRepl.write(\"scrolled slightly upward\");"
> }
> ```
>
> ```text
> scrolled slightly upward
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Report 메서드 표시 확인",
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
>   "title": "Report 메서드 위치 미세 조정",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:904,y:156,scrollY:-52,scrollX:0}); nodeRepl.write(\"scrolled one line upward\");"
> }
> ```
>
> ```text
> scrolled one line upward
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Report 메서드 확인",
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
>   "title": "Notify 메서드 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:847,y:137,mouse_button:\"left\"}); nodeRepl.write(\"selected NotifyAxisOwnershipSessionClosed\");"
> }
> ```
>
> ```text
> selected NotifyAxisOwnershipSessionClosed

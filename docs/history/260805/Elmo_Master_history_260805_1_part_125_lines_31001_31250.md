>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t101917 custom Name\n' +
>     '\t\t\t\t\t101918 custom Type\n' +
>     '\t\t\t\t\t101919 custom Pointer\n' +
>     '\t\t\t\t\t101920 custom Register\n' +
>     '\t\t\t\t\t101921 custom Init Value\n' +
>     '\t\t\t\t\t101922 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t101923 탭 항목 (selectable) Properties\n' +
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 선택",
>   "code": "await sky.click({window:targetWindow,element_index:3140}); nodeRepl.write(\"clicked visible Control class node\");"
> }
> ```
>
> ```text
> clicked visible Control class node
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 노드 상태 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,selected:state.accessibility?.selected_elements,treeTail:state.accessibility?.tree?.slice(-2200)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '83 트리 ID: 103',
>   selected: undefined,
>   treeTail: 'selectable) Global\n' +
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
>     '\t\t\t103097 탭 항목 (selectable) Lib\n' +
>     '\t\t\t103098 탭 항목 (selectable) File\n' +
>     '\t\t\t103099 탭 항목 (selectable) Class\n' +
>     '\t\t\t103100 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t103101 custom Name\n' +
>     '\t\t\t\t\t103102 custom Revision\n' +
>     '\t\t\t\t\t103103 custom GUID\n' +
>     '\t\t\t\t\t103104 custom Task Settings\n' +
>     '\t\t\t\t\t103105 custom Sigmatek\n' +
>     '\t\t\t\t\t103106 custom OSInterface\n' +
>     '\t\t\t\t\t103107 custom IconPath\n' +
>     '\t\t\t\t\t103108 custom SharedCommandTable\n' +
>     '\t\t\t\t\t103109 custom Objectsize\n' +
>     '\t\t\t\t\t103110 custom Singleton\n' +
>     '\t\t\t\t\t103111 custom Hidden\n' +
>     '\t\t\t\t\t103112 custom Deprecated\n' +
>     '\t\t\t\t\t103113 custom GCCOptions\n' +
>     '\t\t\t\t\t103114 custom Comment\n' +
>     '\t\t\t\t\t103115 custom Filename\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t103116 탭 항목 (selectable) Properties\n' +
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
>   "title": "Control 클래스 노드 확장",
>   "code": "await sky.perform_secondary_action({window:targetWindow,element_index:3140,action:\"Expand\"}); nodeRepl.write(\"expanded Control class\");"
> }
> ```
>
> ```text
> element 3140 has no cached secondary actions for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 트리 펼치기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:707,y:218,mouse_button:\"left\"}); nodeRepl.write(\"clicked expand glyph\");"
> }
> ```
>
> ```text
> clicked expand glyph
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 구성 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-3000)}); for (const s of state.screenshots) if(s.zIndex<=1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: '솔 트리 (selectable) Elmo_1\n' +
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
>     '\t\t\t\t\t\t104392 콘솔 트리 (selectable) Variables\n' +
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
>     '\t\t\t104735 탭 항목 (selectable) Lib\n' +
>     '\t\t\t104736 탭 항목 (selectable) File\n' +
>     '\t\t\t104737 탭 항목 (selectable) Class\n' +
>     '\t\t\t104738 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t104739 custom Name\n' +
>     '\t\t\t\t\t104740 custom Revision\n' +
>     '\t\t\t\t\t104741 custom GUID\n' +
>     '\t\t\t\t\t104742 custom Task Settings\n' +
>     '\t\t\t\t\t104743 custom Sigmatek\n' +
>     '\t\t\t\t\t104744 custom OSInterface\n' +
>     '\t\t\t\t\t104745 custom IconPath\n' +
>     '\t\t\t\t\t104746 custom SharedCommandTable\n' +
>     '\t\t\t\t\t104747 custom Objectsize\n' +
>     '\t\t\t\t\t104748 custom Singleton\n' +
>     '\t\t\t\t\t104749 custom Hidden\n' +
>     '\t\t\t\t\t104750 custom Deprecated\n' +
>     '\t\t\t\t\t104751 custom GCCOptions\n' +
>     '\t\t\t\t\t104752 custom Comment\n' +
>     '\t\t\t\t\t104753 custom Filename\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t104754 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 83 트리 ID: 103.'
> }
> ```
>
> Image output: image/jpeg

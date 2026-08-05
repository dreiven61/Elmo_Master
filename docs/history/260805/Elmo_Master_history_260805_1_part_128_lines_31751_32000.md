>
> ```json
> {
>   "title": "배열 변수 타입 목록 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:866,y:389,mouse_button:\"left\",click_count:2}); nodeRepl.write(\"opened variable type editor\");"
> }
> ```
>
> ```text
> opened variable type editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 타입 후보 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height,originX:s.originX,originY:s.originY}))}); for (const s of state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '112215 편집',
>   screens: [
>     {
>       id: 'screenshot-0',
>       z: 0,
>       width: 970,
>       height: 512,
>       originX: -1895,
>       originY: 22
>     },
>     {
>       id: 'screenshot-1',
>       z: 1,
>       width: 960,
>       height: 502,
>       originX: -1890,
>       originY: 27
>     },
>     {
>       id: 'screenshot-2',
>       z: 2,
>       width: 410,
>       height: 452,
>       originX: -1376,
>       originY: 425
>     },
>     {
>       id: 'screenshot-3',
>       z: 3,
>       width: 98,
>       height: 16,
>       originX: -1064,
>       originY: 407
>     }
>   ]
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
>   "title": "배열 타입 선택 준비",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+a\"}); nodeRepl.write(\"selected type text\");"
> }
> ```
>
> ```text
> selected type text
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ARRAY 타입 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"ARRAY\"}); nodeRepl.write(\"typed ARRAY\");"
> }
> ```
>
> ```text
> typed ARRAY
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ARRAY 타입 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed ARRAY\");"
> }
> ```
>
> ```text
> committed ARRAY
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ARRAY 속성 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,treeTail:state.accessibility?.tree?.slice(-1800)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '91 TABLE Properties Window ID: 272349640',
>   treeTail: '\t\t\t75840 콘솔 트리 (selectable) DestSize\n' +
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
>     '\t\t\t112654 탭 항목 (selectable) Lib\n' +
>     '\t\t\t112655 탭 항목 (selectable) File\n' +
>     '\t\t\t112656 탭 항목 (selectable) Class\n' +
>     '\t\t\t112657 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t112658 custom Name\n' +
>     '\t\t\t\t\t112659 custom GUID\n' +
>     '\t\t\t\t\t112660 custom Type\n' +
>     '\t\t\t\t\t112661 custom Element Type\n' +
>     '\t\t\t\t\t112662 custom Pointer\n' +
>     '\t\t\t\t\t112663 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t112664 탭 항목 (selectable) Properties\n' +
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
>   "title": "새 배열 차원 설정 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:734,y:236,mouse_button:\"left\"}); nodeRepl.write(\"expanded new array variable\");"
> }
> ```
>
> ```text
> expanded new array variable
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 차원 항목 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-2500)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: ' GroupMovePos\n' +
>     '\t\t\t\t\t\t\t106717 콘솔 트리 (selectable) GroupKinematicReady\n' +
>     '\t\t\t\t\t\t\t106718 콘솔 트리 (selectable) ZeroHomeState\n' +
>     '\t\t\t\t\t\t\t106719 콘솔 트리 (selectable) OwnershipState\n' +
>     '\t\t\t\t\t\t\t107265 콘솔 트리 (selectable) OwnershipStartupState\n' +
>     '\t\t\t\t\t\t\t\t113185 콘솔 트리 (selectable) 0..1\n' +
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

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
>     '\t\t\t113776 탭 항목 (selectable) Lib\n' +
>     '\t\t\t113777 탭 항목 (selectable) File\n' +
>     '\t\t\t113778 탭 항목 (selectable) Class\n' +
>     '\t\t\t113779 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t113780 custom Name\n' +
>     '\t\t\t\t\t113781 custom GUID\n' +
>     '\t\t\t\t\t113782 custom Type\n' +
>     '\t\t\t\t\t113783 custom Element Type\n' +
>     '\t\t\t\t\t113784 custom Pointer\n' +
>     '\t\t\t\t\t113785 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t113786 탭 항목 (selectable) Properties\n' +
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
>   "title": "배열 범위 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:792,y:237,mouse_button:\"left\"}); nodeRepl.write(\"selected array dimension\");"
> }
> ```
>
> ```text
> selected array dimension
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 범위 속성 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,treeTail:state.accessibility?.tree?.slice(-1400)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '83 트리 ID: 103',
>   treeTail: 'hot\n' +
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
>     '\t\t\t114767 탭 항목 (selectable) Lib\n' +
>     '\t\t\t114768 탭 항목 (selectable) File\n' +
>     '\t\t\t114769 탭 항목 (selectable) Class\n' +
>     '\t\t\t114770 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t114771 custom Limit Low\n' +
>     '\t\t\t\t\t114772 custom Limit Low value\n' +
>     '\t\t\t\t\t114773 custom Limit High\n' +
>     '\t\t\t\t\t114774 custom Limit High value\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t114775 탭 항목 (selectable) Properties\n' +
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
>   "title": "배열 상한 편집",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:854,y:389,mouse_button:\"left\",click_count:2}); nodeRepl.write(\"opened high bound editor\");"
> }
> ```
>
> ```text
> opened high bound editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 상한 입력 상태 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height}))}); for (const s of state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '115398 편집 ID: 769883464',
>   screens: [
>     { id: 'screenshot-0', z: 0, width: 970, height: 512 },
>     { id: 'screenshot-1', z: 1, width: 960, height: 502 }
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 상한 15 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"15\"}); nodeRepl.write(\"typed high bound 15\");"
> }
> ```
>
> ```text
> typed high bound 15
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 상한 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed high bound\");"
> }
> ```
>
> ```text
> committed high bound
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "OwnershipStartupState 범위 검증",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,treeTail:state.accessibility?.tree?.slice(-1100)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '91 TABLE Properties Window ID: 272349640',
>   treeTail: 'yAxisZeroHomeResult\n' +
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
>     '\t\t\t117048 탭 항목 (selectable) Lib\n' +
>     '\t\t\t117049 탭 항목 (selectable) File\n' +
>     '\t\t\t117050 탭 항목 (selectable) Class\n' +
>     '\t\t\t117051 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t117052 custom Limit Low\n' +
>     '\t\t\t\t\t117053 custom Limit Low value\n' +
>     '\t\t\t\t\t117054 custom Limit High\n' +
>     '\t\t\t\t\t117055 custom Limit High value\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t117056 탭 항목 (selectable) Properties\n' +
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

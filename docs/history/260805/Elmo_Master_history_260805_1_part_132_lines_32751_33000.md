> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Notify 선택 상태 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,selected:state.accessibility?.selected_elements});"
> }
> ```
>
> ```text
> { focused: '83 트리 ID: 103', selected: undefined }
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
>   "title": "Report 메서드로 이동",
>   "code": "await sky.press_key({window:targetWindow,key:\"Up\"}); nodeRepl.write(\"moved selection one row up\");"
> }
> ```
>
> ```text
> moved selection one row up
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Report 메서드 선택 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> { focused: '83 트리 ID: 103' }
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
>   "title": "Startup 메서드 이름 편집",
>   "code": "await sky.press_key({window:targetWindow,key:\"F2\"}); nodeRepl.write(\"opened method name editor\");"
> }
> ```
>
> ```text
> opened method name editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Startup 메서드 이름 입력 상태 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height}))});"
> }
> ```
>
> ```text
> {
>   focused: '128592 편집 ID: 1',
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 메서드 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"ReconcileAxisOwnershipStartup\"}); nodeRepl.write(\"typed ReconcileAxisOwnershipStartup\");"
> }
> ```
>
> ```text
> typed ReconcileAxisOwnershipStartup
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 메서드 이름 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed Reconcile method name\");"
> }
> ```
>
> ```text
> committed Reconcile method name
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 메서드 입력 목록 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-2500)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: 'le, disabled) CopySnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t75839 콘솔 트리 (selectable, disabled) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t75840 콘솔 트리 (selectable, disabled) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t75841 콘솔 트리 (selectable, disabled) Result\n' +
>     '\t\t\t\t\t\t\t\t70661 콘솔 트리 (selectable, disabled) CopyTopologyIoSnapshot\n' +
>     '\t\t\t\t\t\t\t\t70662 콘솔 트리 (selectable, disabled) AdvanceOutputRevision\n' +
>     '\t\t\t\t\t\t\t\t70663 콘솔 트리 (selectable, disabled) SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t70664 콘솔 트리 (selectable, disabled) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t\t70665 콘솔 트리 (selectable, disabled) SubmitDs402HomeSetpointAlignment\n' +
>     '\t\t\t\t\t\t\t\t70666 콘솔 트리 (selectable, disabled) SubmitAxisZeroHome\n' +
>     '\t\t\t\t\t\t\t\t70667 콘솔 트리 (selectable, disabled) CopyAxisZeroHomeResult\n' +
>     '\t\t\t\t\t\t\t\t70668 콘솔 트리 (selectable, disabled) CopyAxisOwnershipStartupSnapshot\n' +
>     '\t\t\t\t\t\t\t\t\t73240 콘솔 트리 (selectable, disabled) pDest\n' +
>     '\t\t\t\t\t\t\t\t\t93430 콘솔 트리 (selectable, disabled) DestSize\n' +
>     '\t\t\t\t\t\t\t\t\t96123 콘솔 트리 (selectable, disabled) Result\n' +
>     '\t\t\t\t\t\t\t67057 콘솔 트리 (selectable, disabled) Private\n' +
>     '\t\t\t\t\t\t66033 콘솔 트리 (selectable, disabled) Variables\n' +
>     '\t\t\t\t\t\t66034 콘솔 트리 (selectable, disabled) Objects\n' +
>     '\t\t\t\t\t\t66035 콘솔 트리 (selectable, disabled) Dependencies\n' +
>     '\t\t\t\t\t3143 콘솔 트리 (selectable, disabled) LMCRecorderStore\n' +
>     '\t\t\t\t\t3144 콘솔 트리 (selectable, disabled) LMCSdoExecutor\n' +
>     '\t\t\t\t\t3145 콘솔 트리 (selectable, disabled) TCPIPServer\n' +
>     '\t\t\t\t\t3146 콘솔 트리 (selectable, disabled) TCPMotionInterface\n' +
>     '\t\t\t130153 탭 항목 (selectable) Lib\n' +
>     '\t\t\t130154 탭 항목 (selectable) File\n' +
>     '\t\t\t130155 탭 항목 (selectable) Class\n' +
>     '\t\t\t130156 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t126526 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t126527 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t126528 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t126529 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t126530 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t130157 custom Name\n' +
>     '\t\t\t\t\t130158 custom Virtual\n' +
>     '\t\t\t\t\t130159 custom Global access\n' +
>     '\t\t\t\t\t130160 custom AWL implementation\n' +
>     '\t\t\t\t\t130161 custom CDecl\n' +
>     '\t\t\t\t\t130162 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t130163 탭 항목 (selectable) Properties\n' +
>     '\t\t\t96 단추 Close\n' +
>     '\t\t130079 대화 상자 Automatic Source Code Update Secondary Actions: Raise\n' +
>     '\t\t\t130080 단추 Yes ID: 1366\n' +
>     '\t\t\t130081 단추 Yes to All ID: 1367\n' +
>     '\t\t\t130082 단추 No ID: 1368\n' +
>     '\t\t\t130083 단추 Skip File ID: 1697\n' +
>     '\t\t\t130084 텍스트 Should the selected name be changed? ID: 1370\n' +
>     '\t\t\t130085 단추 Cancel ID: 2\n' +
>     '\t\t\t130086 제목 표시줄\n' +
>     '\t\t\t\t130087 메뉴 모음 시스템 ID: MenuBar\n' +
>     '\t\t\t\t\t130088 메뉴 항목 시스템\n' +
>     '\t\t\t\t130089 단추 닫기\n' +
>     '\n' +
>     'The focused UI element is 130079 대화 상자 Automatic Source Code Update Secondary Actions: Raise.'
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
> 메서드를 새 이름으로 바꾸자 IDE가 기존 TCP 호출부까지 자동 갱신할지 묻고 있습니다. 기존 호출을 임시로 일관되게 유지하기 위해 `Yes to All`로 반영하고, 이후 설계대로 TCP의 BootId-only 호출 블록 자체를 제거하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 자동 참조 이름 갱신",
>   "code": "await sky.click({window:targetWindow,element_index:130081}); nodeRepl.write(\"accepted Yes to All\");"
> }
> ```
>
> ```text
> element 130081 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 자동 참조 이름 갱신",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-2\",x:127,y:86,mouse_button:\"left\"}); nodeRepl.write(\"clicked Yes to All\");"
> }
> ```
>
> ```text
> clicked Yes to All
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile 이름 변경 결과 확인",

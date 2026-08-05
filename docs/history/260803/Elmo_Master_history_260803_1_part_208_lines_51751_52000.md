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
>   "title": "Verify DS402 input placeholder",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var sm9=globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl'); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,section:globalThis.state.accessibility.tree.slice(sm9,sm9+700)});"
> }
> ```
>
> ```text
> {
>   focused: '41619 편집 ID: 1',
>   section: 'SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t\t41620 콘솔 트리 (selectable) Param\n' +
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
>     '\t\t\t\t\t\t18385 콘'
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> C# SDK와 WPF 쪽은 병렬 검증이 끝났습니다. 현재 각각 Debug/Release 전체 테스트가 SDK 1066/1066, WPF 317/317 통과했고, LASAL 쪽은 IDE에서 DS402 실시간 제어용 메서드 인자·메일박스·축 클라이언트를 실제 프로젝트 구조에 등록하고 있습니다. 현재 보이는 LASAL 1개 오류는 새 코드가 아니라 설치된 MotionLib가 참조하는 `DriveComL2.h` 누락입니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Name DS402 operation token",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'OperationToken'});"
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
>   "title": "Verify DS402 parameter name",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,section:globalThis.state.accessibility.tree.slice(globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl'),globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl')+350)});"
> }
> ```
>
> ```text
> {
>   focused: '41619 편집 ID: 1',
>   section: 'SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t\t41620 콘솔 트리 (selectable) Param\n' +
>     '\t\t\t\t\t\t\t\t38312 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t37503 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t18376 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t18377 콘솔 트리 (selectable) PublishSequence\n' +
>     '\t\t\t\t\t\t\t18378 콘솔 트리 (selectable) SnapshotBytes\n' +
>     '\t\t\t\t\t\t\t\t36041 콘솔 트리 (selectable) 0..511\n' +
>     '\t\t\t\t\t\t\t18379 콘'
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
>   "title": "Commit DS402 parameter name",
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
>   "title": "Inspect DS402 operation token type",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var sec10=globalThis.state.accessibility.tree.slice(globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl'),globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl')+500); nodeRepl.write({section:sec10,props:globalThis.state.accessibility.tree.match(/TABLE Properties Window[\\s\\S]{0,500}/)?.[0]});"
> }
> ```
>
> ```text
> {
>   section: 'SubmitDs402HomeControl\n' +
>     '\t\t\t\t\t\t\t\t\t42205 콘솔 트리 (selectable) OperationToken\n' +
>     '\t\t\t\t\t\t\t\t38312 콘솔 트리 (selectable) GetDs402HomeControlState\n' +
>     '\t\t\t\t\t\t\t37503 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t18376 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t18377 콘솔 트리 (selectable) PublishSequence\n' +
>     '\t\t\t\t\t\t\t18378 콘솔 트리 (selectable) SnapshotBytes\n' +
>     '\t\t\t\t\t\t\t\t36041 콘솔 트리 (selectable) 0..511\n' +
>     '\t\t\t\t\t\t\t18379 콘솔 트리 (selectable) OutputRevision\n' +
>     '\t\t\t\t\t\t\t18380 콘솔 트리 (selectable) OutputObserved\n' +
>     '\t\t\t\t\t\t\t18381 콘솔 트리 (selectable) OutputPreviousValid\n' +
>     '\t\t\t\t\t\t\t18',
>   props: 'TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t42647 custom Name\n' +
>     '\t\t\t\t\t42648 custom Type\n' +
>     '\t\t\t\t\t42649 custom Pointer\n' +
>     '\t\t\t\t\t42650 custom Register\n' +
>     '\t\t\t\t\t42651 custom Init Value\n' +
>     '\t\t\t\t\t42652 custom Comment\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t42653 탭 항목 (selectable) Properties\n' +
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Edit DS402 token type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1362,y:497,click_count:2,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Verify DS402 token type menu",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.map((s,i)=>({i,w:s.width,h:s.height,ox:s.originX,oy:s.originY})));"
> }
> ```
>
> ```text
> [
>   { i: 0, w: 1450, h: 763, ox: -1880, oy: 41 },
>   { i: 1, w: 1440, h: 753, ox: -1875, oy: 46 },
>   { i: 2, w: 410, h: 452, ox: -864, oy: 550 },
>   { i: 3, w: 115, h: 16, ox: -569, oy: 532 }
> ]
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
>   "title": "Choose UDINT token type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:30,y:152,screenshotId:globalThis.state.screenshots[2].id});"
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
>   "title": "Confirm UDINT token type",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({accessibility:!!globalThis.state.accessibility,focused:globalThis.state.accessibility?.focused_element});"
> }
> ```
>
> ```text
> {
>   accessibility: true,
>   focused: '105 TABLE Properties Window ID: 301947776'
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
>   "title": "Reopen DS402 token type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:494,click_count:2,screenshotId:globalThis.state.screenshots[0].id});"
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

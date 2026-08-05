>     '\t\t\t\t\t3146 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t107672 탭 항목 (selectable) Lib\n' +
>     '\t\t\t107673 탭 항목 (selectable) File\n' +
>     '\t\t\t107674 탭 항목 (selectable) Class\n' +
>     '\t\t\t107675 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t107676 custom Name\n' +
>     '\t\t\t\t\t107677 custom GUID\n' +
>     '\t\t\t\t\t107678 custom Type\n' +
>     '\t\t\t\t\t107679 custom Pointer\n' +
>     '\t\t\t\t\t107680 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t107681 탭 항목 (selectable) Properties\n' +
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
>   "title": "기존 배열 변수 속성 비교",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:817,y:198,mouse_button:\"left\"}); nodeRepl.write(\"selected ZeroHomeState\");"
> }
> ```
>
> ```text
> selected ZeroHomeState
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 변수 타입 형식 확인",
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
>   "title": "새 변수 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:833,y:238,mouse_button:\"left\"}); nodeRepl.write(\"selected OwnershipStartupState\");"
> }
> ```
>
> ```text
> selected OwnershipStartupState
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 변수 선택 상태 확인",
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
>   "title": "배열 변환 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:830,y:236,mouse_button:\"right\"}); nodeRepl.write(\"opened variable menu\");"
> }
> ```
>
> ```text
> opened variable menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "변수 배열 메뉴 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height,originX:s.originX,originY:s.originY})),tree:state.accessibility?.tree?.slice(0,1800)}); for (const s of state.screenshots) if(s.zIndex>=2) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
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
>       width: 174,
>       height: 148,
>       originX: -1060,
>       originY: 263
>     },
>     {
>       id: 'screenshot-3',
>       z: 3,
>       width: 174,
>       height: 4,
>       originX: -1056,
>       originY: 411
>     },
>     {
>       id: 'screenshot-4',
>       z: 4,
>       width: 4,
>       height: 144,
>       originX: -886,
>       originY: 267
>     }
>   ],
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise\n' +
>     '\t\t110132 창\n' +
>     '\t\t110133 창\n' +
>     '\t\t110134 메뉴\n' +
>     '\t\t\t110583 메뉴 항목 Delete Variable Del\n' +
>     '\t\t\t110584 메뉴 항목 Move Up\n' +
>     '\t\t\t110585 메뉴 항목 (disabled) Move Down\n' +
>     '\t\t\t110586 메뉴 항목 (disabled) Go to Datatype\n' +
>     '\t\t\t110587 메뉴 항목 Add Server for Visu\n' +
>     '\t\t\t110588 메뉴 항목 Copy\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t3167 창 LMCControlCommandService* Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t3168 창 ID: 59648\n' +
>     '\t\t\t\t\t3169 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_K'
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
>   "title": "변수 메뉴 닫기",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); nodeRepl.write(\"closed variable menu\");"
> }
> ```
>
> ```text
> closed variable menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "배열 타입 편집 준비",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex}))});"
> }
> ```
>
> ```text
> {
>   screens: [ { id: 'screenshot-0', z: 0 }, { id: 'screenshot-1', z: 1 } ]
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

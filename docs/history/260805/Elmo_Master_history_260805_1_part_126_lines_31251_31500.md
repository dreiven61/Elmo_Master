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
>   "title": "Control 변수 추가 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:823,y:140,mouse_button:\"right\"}); nodeRepl.write(\"opened Control class menu\");"
> }
> ```
>
> ```text
> opened Control class menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 메뉴 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height,originX:s.originX,originY:s.originY})),tree:state.accessibility?.tree?.slice(0,2200)}); for (const s of state.screenshots) if(s.zIndex>=2) await nodeRepl.emitImage(s.url);"
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
>       width: 209,
>       height: 528,
>       originX: -1067,
>       originY: 167
>     },
>     {
>       id: 'screenshot-3',
>       z: 3,
>       width: 209,
>       height: 4,
>       originX: -1063,
>       originY: 695
>     },
>     {
>       id: 'screenshot-4',
>       z: 4,
>       width: 4,
>       height: 524,
>       originX: -858,
>       originY: 171
>     }
>   ],
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService Secondary Actions: Raise\n' +
>     '\t\t105299 창\n' +
>     '\t\t105300 창\n' +
>     '\t\t105301 메뉴\n' +
>     '\t\t\t105666 메뉴 항목 Edit Source Enter\n' +
>     '\t\t\t105667 메뉴 항목 Edit Defines\n' +
>     '\t\t\t105668 메뉴 항목 Delete Del\n' +
>     '\t\t\t105669 메뉴 항목 Duplicate Class\n' +
>     '\t\t\t105670 메뉴 항목 Derive Class\n' +
>     '\t\t\t105671 메뉴 항목 Find Reference\n' +
>     '\t\t\t105672 메뉴 항목 Print Class\n' +
>     '\t\t\t105673 메뉴 항목 Export Class(es)\n' +
>     '\t\t\t105674 메뉴 항목 Reload Class\n' +
>     '\t\t\t105675 메뉴 항목 Re\\Import MatLab Model\n' +
>     '\t\t\t105676 메뉴 항목 Protection Setting\n' +
>     '\t\t\t105677 메뉴 항목 New Server\n' +
>     '\t\t\t105678 메뉴 항목 New Client\n' +
>     '\t\t\t105679 메뉴 항목 New Method\n' +
>     '\t\t\t105680 메뉴 항목 New Std. Methods\n' +
>     '\t\t\t105681 메뉴 항목 New Variable\n' +
>     '\t\t\t105682 메뉴 항목 New Type\n' +
>     '\t\t\t105683 메뉴 항목 New Table\n' +
>     '\t\t\t105684 메뉴 항목 Add Network\n' +
>     '\t\t\t105685 메뉴 항목 Add File...\n' +
>     '\t\t\t105686 메뉴 항목 Open Subfolders\n' +
>     '\t\t\t105687 메뉴 항목 Paste\n' +
>     '\t\t\t105688 메뉴 항목 Copy\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t3167 창 LMCControlCommandService Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t3168 창 ID: 59648\n' +
>     '\t\t\t\t\t3169 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LM'
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
>   "title": "Control 시작 상태 변수 생성",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-2\",x:75,y:353,mouse_button:\"left\"}); nodeRepl.write(\"selected New Variable\");"
> }
> ```
>
> ```text
> selected New Variable
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 새 변수 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element}); for (const s of state.screenshots) if(s.zIndex<=1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> { focused: '106715 편집 ID: 1' }
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
>   "title": "Control 변수 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"OwnershipStartupState\"}); nodeRepl.write(\"typed OwnershipStartupState\");"
> }
> ```
>
> ```text
> typed OwnershipStartupState
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 변수 이름 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed variable name\");"
> }
> ```
>
> ```text
> committed variable name
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 변수 속성 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,treeTail:state.accessibility?.tree?.slice(-2600)}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '83 트리 ID: 103',
>   treeTail: 'ectable) Servers\n' +
>     '\t\t\t\t\t\t104390 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t104391 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t104392 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t106716 콘솔 트리 (selectable) GroupMovePos\n' +
>     '\t\t\t\t\t\t\t106717 콘솔 트리 (selectable) GroupKinematicReady\n' +
>     '\t\t\t\t\t\t\t106718 콘솔 트리 (selectable) ZeroHomeState\n' +
>     '\t\t\t\t\t\t\t106719 콘솔 트리 (selectable) OwnershipState\n' +
>     '\t\t\t\t\t\t\t107265 콘솔 트리 (selectable) OwnershipStartupState\n' +
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

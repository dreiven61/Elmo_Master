> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService',
>   focused: '3169 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000',
>   treeTail: 'OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000.'
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 뷰로 전환",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:813,y:281,mouse_button:\"left\"}); nodeRepl.write(\"clicked Class view tab\");"
> }
> ```
>
> ```text
> clicked Class view tab
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 트리 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,treeTail:state.accessibility?.tree?.slice(-3500)}); for (const s of state.screenshots) if(s.zIndex<=1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '83 트리 ID: 103',
>   treeTail: '록 항목 (selectable)\n' +
>     '\t\t\t\t\t\t97011 목록 항목 (selectable)\n' +
>     '\t\t\t\t\t73 스크롤 막대 ID: 59904\n' +
>     '\t\t\t\t\t\t74 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t75 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t76 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t101025 탭 항목 (selectable) Python Script\n' +
>     '\t\t\t101026 탭 항목 (selectable) Debugger\n' +
>     '\t\t\t101027 탭 항목 (selectable) Output\n' +
>     '\t\t\t80 단추 Close\n' +
>     '\t\t81 창 Splitter ID: 371773352\n' +
>     '\t\t82 Tab Class View ID: 274609808\n' +
>     '\t\t\t83 트리 ID: 103\n' +
>     '\t\t\t\t3125 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
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
>     '\t\t\t101028 탭 항목 (selectable) Lib\n' +
>     '\t\t\t101029 탭 항목 (selectable) File\n' +
>     '\t\t\t101030 탭 항목 (selectable) Class\n' +
>     '\t\t\t101031 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t71366 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t101032 custom Name\n' +
>     '\t\t\t\t\t101033 custom Type\n' +
>     '\t\t\t\t\t101034 custom Pointer\n' +
>     '\t\t\t\t\t101035 custom Register\n' +
>     '\t\t\t\t\t101036 custom Init Value\n' +
>     '\t\t\t\t\t101037 custom Comment\n' +
>     '\t\t\t\t92 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t93 단추\n' +
>     '\t\t\t\t\t94 단추\n' +
>     '\t\t\t101038 탭 항목 (selectable) Properties\n' +
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
>   "title": "Control 클래스 트리 상단으로 이동",
>   "code": "await sky.scroll({window:targetWindow,screenshotId:\"screenshot-1\",x:903,y:173,scrollY:-890,scrollX:0}); nodeRepl.write(\"scrolled class tree upward\");"
> }
> ```
>
> ```text
> scrolled class tree upward
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Control 클래스 루트 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({treeTail:state.accessibility?.tree?.slice(-3200)}); for (const s of state.screenshots) if(s.zIndex<=1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   treeTail: '\t\t\t80 단추 Close\n' +
>     '\t\t81 창 Splitter ID: 371773352\n' +
>     '\t\t82 Tab Class View ID: 274609808\n' +
>     '\t\t\t83 트리 ID: 103\n' +
>     '\t\t\t\t3125 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
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
>     '\t\t\t101913 탭 항목 (selectable) Lib\n' +
>     '\t\t\t101914 탭 항목 (selectable) File\n' +
>     '\t\t\t101915 탭 항목 (selectable) Class\n' +
>     '\t\t\t101916 탭 항목 (selectable) Global\n' +
>     '\t\t\t88 단추 Close\n' +
>     '\t\t89 Tab Properties ID: 274603880\n' +
>     '\t\t\t90 창 ID: 261121536\n' +
>     '\t\t\t\t91 TABLE Properties Window ID: 272349640\n' +
>     '\t\t\t\t\t71362 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t71363 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t71364 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t71365 단추 페이지 아래로 ID: DownPageButton\n' +

>   "title": "ObservationCycle 참조 이름 갱신",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-2\",x:127,y:86,mouse_button:\"left\"}); nodeRepl.write(\"accepted parameter reference rename\");"
> }
> ```
>
> ```text
> accepted parameter reference rename
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ObservationCycle 변경 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title,focused:state.accessibility?.focused_element}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*',
>   focused: '134788 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000'
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
>   "title": "ProofFlags 입력 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:832,y:198,mouse_button:\"left\"}); nodeRepl.write(\"selected ProofFlags\");"
> }
> ```
>
> ```text
> selected ProofFlags
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ProofFlags 선택 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element});"
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ReportCycle 입력 이름 편집",
>   "code": "await sky.press_key({window:targetWindow,key:\"F2\"}); nodeRepl.write(\"opened ProofFlags name editor\");"
> }
> ```
>
> ```text
> opened ProofFlags name editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ReportCycle 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"ReportCycle\"}); nodeRepl.write(\"typed ReportCycle\");"
> }
> ```
>
> ```text
> typed ReportCycle
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ReportCycle 이름 확정",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"}); nodeRepl.write(\"committed ReportCycle\");"
> }
> ```
>
> ```text
> committed ReportCycle
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "ReportCycle 참조 갱신 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element,screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,width:s.width,height:s.height}))}); for (const s of state.screenshots) if(s.zIndex>=2) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   focused: '137873 대화 상자 Automatic Source Code Update Secondary Actions: Raise',
>   screens: [
>     { id: 'screenshot-0', z: 0, width: 970, height: 512 },
>     { id: 'screenshot-1', z: 1, width: 960, height: 502 },
>     { id: 'screenshot-2', z: 2, width: 412, height: 116 }
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
>   "title": "ReportCycle 참조 이름 갱신",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-2\",x:127,y:86,mouse_button:\"left\"}); nodeRepl.write(\"accepted ReportCycle rename\");"
> }
> ```
>
> ```text
> accepted ReportCycle rename
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Reconcile ABI 중간 결과 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title,focused:state.accessibility?.focused_element}); for (const s of state.screenshots) if(s.zIndex===1) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*',
>   focused: '134788 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000'
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
>   "title": "QuarantineReason 입력 선택",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:842,y:218,mouse_button:\"left\"}); nodeRepl.write(\"selected QuarantineReason\");"
> }
> ```
>
> ```text
> selected QuarantineReason
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "QuarantineReason 선택 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:state.accessibility?.focused_element});"
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "DiagnosticsDrainFlags 입력 이름 편집",
>   "code": "await sky.press_key({window:targetWindow,key:\"F2\"}); nodeRepl.write(\"opened QuarantineReason name editor\");"
> }
> ```
>
> ```text
> opened QuarantineReason name editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "DiagnosticsDrainFlags 이름 입력",
>   "code": "await sky.type_text({window:targetWindow,text:\"DiagnosticsDrainFlags\"}); nodeRepl.write(\"typed DiagnosticsDrainFlags\");"
> }
> ```
>
> ```text
> typed DiagnosticsDrainFlags
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json

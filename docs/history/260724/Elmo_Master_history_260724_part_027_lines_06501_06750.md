> 					312 custom AutomationML
> 					313 custom IO Connection Manager Options
> 					314 custom Comment
> 					315 custom Filename
> 				316 도구 모음 ID: 59392
> 					317 단추
> 					318 단추
> 			319 탭 항목 (selectable) Properties
> 			320 단추 Close
>
> The focused UI element is 266 트리 ID: 102.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state; globalThis.state=null; await sky.click({window:observation.window,element_index:270,click_count:2}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; {const t=state.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").filter(x=>/Source Files|EtherCAT|SDO|\\.st/.test(x)).slice(0,200).join(\"\\n\"));}}",
>   "title": "LASAL 소스 파일 목록 펼치기",
>   "timeout_ms": 30000
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; {const t=state.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").filter(x=>/Source Files|EtherCAT|SDO|\\.st|console tree|콘솔 트리/.test(x)).slice(-260).join(\"\\n\"));}",
>   "title": "LASAL 소스 파일 항목 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Elmo_3", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Elmo_3 Secondary Actions: Raise
> 			16 창 EtherCAT_Network Secondary Actions: Raise ID: 65291
> 					56 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxError if StateWord.Fault = FALSE then AxError := 0; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; else // Set AxError if Fault is set in Stateword and Error is not beeing quit. if AxErrorQuit = 0 then AxError := 1; elsif (ops.tabsolute - TimeStampResetFault) >= GetQuitErrorTimeout() then AxError := 1; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; end_if; end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRtPostScan if AxError = 0 then if UseDefaultEnableSequence then EnableSequence(); end_if; end_if; END_FUNCTION FUNCTION VIRTUAL ECAT_DS402Base::EnableSequence //************************************************************************************************** // ENABLE/DISABLE SEQUENCE //************************************************************************************************** case EnableSequenceSSW of //************************************************************************************************** e_EnableSequenceSSW::_Disabled: // do nothing special, we're enabled now e_EnableSequenceSSW::_Enabled: // do nothing special, we're disabled now //************************************************************************************************** e_EnableSequenceSSW::_StartEnable: ControlWord.SwitchOn := FALSE; ControlWord.EnableVoltage := TRUE; ControlWord.QuickStop := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _SwitchOn; //************************************************************************************************** e_EnableSequenceSSW::_SwitchOn: if StateWord.VoltageEnabled StateWord.QuickStop & StateWord.ReadyToSwitchOn then ControlWord.SwitchOn := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _EnableOperation; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_EnableOperation: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn then ControlWord.EnableOperation := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _CheckOperationEnabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_CheckOperationEnabled: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn & StateWord.OperationEnabled then EnableSequenceSSW := _Enabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_DisableOperation: if StateWord.OperationEnabled = 0 then AxEnable := 0; EnableSequenceSSW := _Disabled; end_if; //**************************************************************************** ID: 10000
> 					113 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 				273 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					274 콘솔 트리 (selectable) Additional Files
> 					275 콘솔 트리 (selectable) Header Files
> 					276 콘솔 트리 (selectable) Source Files
> 					277 콘솔 트리 (selectable) Network Files
> 					278 콘솔 트리 (selectable) Object Files
> 					279 콘솔 트리 (selectable) Library Files
> 					280 콘솔 트리 (selectable) Dias Drive Files
> 					281 콘솔 트리 (selectable) Connection Files
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state; globalThis.state=null; await sky.click({window:observation.window,element_index:276,click_count:2}); globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(\"Source Files action complete\");}",
>   "title": "Source Files 펼치기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Source Files action complete
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; {const t=state.accessibility?.tree||\"\"; nodeRepl.write(t.split(\"\\n\").filter(x=>/Source Files|EtherCAT_SDO|\\.st|콘솔 트리/.test(x)).slice(-300).join(\"\\n\"));}",
>   "title": "Source Files 내용 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 				267 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					268 콘솔 트리 (selectable) Additional Files
> 					269 콘솔 트리 (selectable) Header Files
> 					270 콘솔 트리 (selectable) Source Files
> 					271 콘솔 트리 (selectable) Network Files
> 					272 콘솔 트리 (selectable) Object Files
> 					273 콘솔 트리 (selectable) Library Files
> 					274 콘솔 트리 (selectable) Dias Drive Files
> 					275 콘솔 트리 (selectable) Connection Files
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); globalThis.targetWindow=state.window; nodeRepl.write(\"File Explorer observed\");",
>   "title": "Source Files 확장 버튼 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> File Explorer observed
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1664,y:196});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Source Files|EtherCAT_SDO|\\.st|콘솔 트리/.test(x)).slice(-300).join(\"\\n\"));}",
>   "title": "Source Files 목록 확장",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(String(globalThis.state?.accessibility?.focused_element||\"\"));",
>   "title": "소스 트리 포커스 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1735,y:197});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:false,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||\"\"));}",
>   "title": "LASAL 소스 트리에 포커스",
>   "timeout_ms": 30000
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
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.type_text({window:observation.window,text:\"EtherCAT_SDOBase.st\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/EtherCAT_SDO|selected|selectable/.test(x)).slice(-80).join(\"\\n\"));}",
>   "title": "EtherCAT SDO 소스 선택",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"e\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(JSON.stringify({selected:state.accessibility?.selected_elements,focused:state.accessibility?.focused_element},null,2));}",
>   "title": "EtherCAT 소스 위치로 이동",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   "focused": "266 트리 ID: 102"
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.drag({window:observation.window,screenshotId:sid,from_x:1907,from_y:184,to_x:1907,to_y:335});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/\\.st|EtherCAT|SDO/.test(x)).slice(-80).join(\"\\n\"));}",
>   "title": "EtherCAT 소스 위치로 스크롤",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.drag({window:observation.window,screenshotId:sid,from_x:1908,from_y:311,to_x:1908,to_y:415});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/\\.st|EtherCAT|SDO/.test(x)).slice(-100).join(\"\\n\"));}",
>   "title": "E 소스 구간으로 이동",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.scroll({window:observation.window,screenshotId:sid,x:1780,y:400,scrollX:0,scrollY:1540});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write((state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/\\.st|EtherCAT|SDO/.test(x)).slice(-100).join(\"\\n\"));}",
>   "title": "Source Files 아래로 이동",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {

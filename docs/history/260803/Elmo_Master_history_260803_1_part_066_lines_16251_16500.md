>   "title": "CopyTopologyIoSnapshot 이름 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "새 메서드 구조 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"CopyTopologyIoSnapshot\")||x.includes(\"CopySnapshot\")||x.includes(\"Input\")||x.includes(\"Output\")).slice(-20).join(\"\\n\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise
> 			2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283
> 					2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 		126 Tab Output ID: 296578152
> 			75907 탭 항목 (selectable) Output
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> 							31405 콘솔 트리 (selectable) InputSlot
> 							37416 콘솔 트리 (selectable) OutputSlot
> 								72588 콘솔 트리 (selectable) CopySnapshot
> 								75607 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 							44367 콘솔 트리 (selectable) OutputRevision
> 							61212 콘솔 트리 (selectable) OutputObserved
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
> 							66427 콘솔 트리 (selectable) OutputPreviousValue
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
>   "title": "CopyTopologyIoSnapshot 시그니처 펼치기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1230,y:317,screenshotId:globalThis.state.screenshots[1].id});"
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
>   "title": "메서드 입력 출력 폴더 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"Input\")||x.includes(\"Output\")||x.includes(\"CopyTopologyIoSnapshot\")).slice(-20).join(\"\\n\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise
> 			2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283
> 					2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 		126 Tab Output ID: 296578152
> 			76708 탭 항목 (selectable) Output
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> 							31405 콘솔 트리 (selectable) InputSlot
> 							37416 콘솔 트리 (selectable) OutputSlot
> 								75607 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 							44367 콘솔 트리 (selectable) OutputRevision
> 							61212 콘솔 트리 (selectable) OutputObserved
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
> 							66427 콘솔 트리 (selectable) OutputPreviousValue
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
>   "title": "CopyTopologyIoSnapshot 매개변수 메뉴",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:75607,mouse_button:\"right\"});"
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
>   "title": "매개변수 메뉴 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"메뉴 항목\")).slice(0,12).join(\"\\n\"));"
> }
> ```
>
> ```text
> 			78016 메뉴 항목 Edit Method Enter
> 			78017 메뉴 항목 Delete Method Del
> 			78018 메뉴 항목 Move Up
> 			78019 메뉴 항목 (disabled) Move Down
> 			78020 메뉴 항목 Copy
> 			78021 메뉴 항목 New Input Variable
> 			78022 메뉴 항목 New Output Variable
> 			78023 메뉴 항목 Add to Newinst
> 				78034 메뉴 항목 Macros
> 				78035 메뉴 항목 Layouts
> 				78037 메뉴 항목 Toolbar Options
> 				78039 메뉴 항목 Toolbar Options
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
>   "title": "pDest 입력 변수 생성",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1391,y:451,screenshotId:globalThis.state.screenshots[1].id});"
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
>   "title": "pDest 이름 입력",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"pDest\"});"
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
>   "title": "pDest 이름 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "pDest 속성 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>x.includes(\"pDest\")||x.includes(\"CopyTopologyIoSnapshot\")).slice(-10).join(\"\\n\"));"
> }
> ```
>
> ```text
> 								75607 콘솔 트리 (selectable) CopyTopologyIoSnapshot
> 									79045 콘솔 트리 (selectable) pDest
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
>   "title": "pDest 기본 타입 편집",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:494,screenshotId:globalThis.state.screenshots[1].id,click_count:2});"
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
>   "title": "pDest 타입을 Void로 지정",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"Void\"});"
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
>   "title": "pDest 타입 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
> }
> ```
>
> ```text
>
> ```
>

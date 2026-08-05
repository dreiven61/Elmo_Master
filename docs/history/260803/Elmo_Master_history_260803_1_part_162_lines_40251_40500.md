> ```
>
> ```text
> [
>   '\t\t\t3507 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65283',
>   '\t\t\t\t\t3509 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000',
>   '\t\t\t\t\t3457 콘솔 트리 (selectable) LMCEcatInputLatch'
> ]
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
>   "title": "LMCEcatInputLatch Client 목록 펼치기",
>   "code": "await sky.click({window:lasalWindow15,x:1201,y:298});"
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
>   "title": "LMCEcatInputLatch Client 검색 대상 확인",
>   "code": "var latchClients20 = await sky.get_window_state({window:lasalWindow15,include_text:true}); nodeRepl.write(latchClients20.accessibility.tree.split(\"\\n\").filter(x=>/Coupler|InputSlot|OutputSlot|EcatMaster|RecorderStore/.test(x)));"
> }
> ```
>
> ```text
> [
>   '\t\t\t\t\t3509 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000',
>   '\t\t\t\t\t\t\t17346 콘솔 트리 (selectable) EcatMaster',
>   '\t\t\t\t\t\t\t17351 콘솔 트리 (selectable) RecorderStore',
>   '\t\t\t\t\t\t\t17352 콘솔 트리 (selectable) Coupler',
>   '\t\t\t\t\t\t\t17353 콘솔 트리 (selectable) InputSlot',
>   '\t\t\t\t\t\t\t17354 콘솔 트리 (selectable) OutputSlot',
>   '\t\t\t\t\t3458 콘솔 트리 (selectable) LMCRecorderStore'
> ]
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
>   "title": "Coupler 구현 참조 검색",
>   "code": "await sky.click({window:lasalWindow15,element_index:17352,mouse_button:\"right\"});"
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
>   "title": "Coupler 구현 검색 명령 선택",
>   "code": "var couplerMenu21 = await sky.get_window_state({window:lasalWindow15,include_text:true}); nodeRepl.write(couplerMenu21.accessibility.tree.split(\"\\n\").filter(x=>/Find|Implementation|Coupler/.test(x)));"
> }
> ```
>
> ```text
> [ '\t\t\t\t\t\t\t17352 콘솔 트리 (selectable) Coupler' ]
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
>   "title": "Coupler Client 채널 펼치기",
>   "code": "await sky.press_key({window:lasalWindow15,key:\"ESC\"}); await sky.click({window:lasalWindow15,x:1217,y:342});"
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
>   "title": "Coupler 채널 확인",
>   "code": "var couplerChannels22 = await sky.get_window_state({window:lasalWindow15,include_text:true}); nodeRepl.write(couplerChannels22.accessibility.tree.split(\"\\n\").filter(x=>/Coupler|EtherCATState|ClassState|NativeOnline|Online|SlaveState|AL_Status/.test(x)));"
> }
> ```
>
> ```text
> [
>   '\t\t\t\t\t3495 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="EtherCAT_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "Elmo_11" GUID = "{47F87ED0-D0EE-486E-BC7A-A70547F6C0D9}" Class = "Elmo_1" Position = "(1500,1620)" Visualized = "true" Remotely = "true"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATOut_1"/> <Server Name="EtherCATState"/> <Server Name="Inputs_DigitalInputs"/> <Server Name="Online"/> <Server Name="Outputs_DigitalOutputs"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{7266B399-596C-4DBB-A5BF-56AC8BC68024}" Class="ECAT_DS402Base"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{80EAE1CA-1B88-4D0B-86A1-68539F1C74D5}" Class="ECAT_Slave_Base"> <Channels> <Server Name="AL_StatusCode"/> <Server Name="ClassState"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SlaveState"/> <Server Name="VendorID"/> <Client Name="NoAsyncBuffer" Value="0"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="NoSSCBuffer" Value="0"/> <Client Name="Required" Value="1"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="toStdLib"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components ID: 10000',
>   '\t\t\t\t\t3509 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000',
>   '\t\t\t\t18739 단추 Change Online Settings',
>   '\t\t\t\t18740 메뉴 항목 Online Connection',
>   '\t\t\t\t18741 단추 (disabled) Set Online Connection For Current Project',
>   '\t\t\t\t18743 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)',
>   '\t\t\t\t\t\t\t17352 콘솔 트리 (selectable) Coupler'
> ]
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
>   "title": "LASAL Class View 복귀",
>   "code": "await sky.click({window:lasalWindow15,x:1354,y:407});"
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
>   "title": "LASAL Class View 상태 확인",
>   "code": "var classState23 = await sky.get_window_state({window:lasalWindow15,include_text:true}); nodeRepl.write(classState23.accessibility.focused_element);"
> }
> ```
>
> ```text
> 98 트리 ID: 103
> ```
>
> Image output: image/jpeg
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
>   "title": "LASAL Class View 탭 확인",
>   "code": "nodeRepl.write(classState23.accessibility.tree.split(\"\\n\").filter(x=>/탭 항목/.test(x)).slice(-20));"
> }
> ```
>
> ```text
> [
>   '\t\t\t19794 탭 항목 (selectable) Python Script',
>   '\t\t\t19795 탭 항목 (selectable) Debugger',
>   '\t\t\t19796 탭 항목 (selectable) Output',
>   '\t\t\t19797 탭 항목 (selectable) Lib',
>   '\t\t\t19798 탭 항목 (selectable) File',
>   '\t\t\t19799 탭 항목 (selectable) Global',
>   '\t\t\t19800 탭 항목 (selectable) Class',
>   '\t\t\t19808 탭 항목 (selectable) Properties'
> ]
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
>   "title": "LASAL Class View 복귀",
>   "code": "await sky.click({window:lasalWindow15,element_index:19800});"
> }
> ```
>
> ```text
> element 19800 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Class View 복귀",
>   "code": "await sky.click({window:lasalWindow15,x:1380,y:408});"
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
>   "title": "LASAL Class View 복귀 확인",
>   "code": "var classBack24=await sky.get_window_state({window:lasalWindow15,include_text:false}); nodeRepl.write(classBack24.window);"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 4790364,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network'
> }
> ```
>
> Image output: image/jpeg

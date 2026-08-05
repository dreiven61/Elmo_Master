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
>   "title": "잘못 입력된 타입 선택",
>   "timeout_ms": 30000,
>   "code": "\nvar observation14 = globalThis.state;\nglobalThis.state = null;\nawait sky.press_key({window:observation14.window,key:\"Control_L+a\"});\nglobalThis.state = await sky.get_window_state({window:observation14.window,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element,selectedText:state.accessibility?.selected_text}, null, 2));\n"
> }
> ```
>
> ```text
> {
>   "focused": "19382 편집"
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
>   "title": "변수 타입 입력",
>   "timeout_ms": 30000,
>   "code": "\nvar observation15 = globalThis.state;\nif (!observation15?.accessibility?.focused_element) throw new Error(\"No focused type editor\");\nglobalThis.state = null;\nawait sky.type_text({window:observation15.window,text:\"UDINT\"});\nglobalThis.state = await sky.get_window_state({window:observation15.window,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element}, null, 2));\n"
> }
> ```
>
> ```text
> {}
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
>   "title": "UDINT 타입 확정",
>   "timeout_ms": 30000,
>   "code": "\nvar observation16 = globalThis.state;\nglobalThis.state = null;\nawait sky.press_key({window:observation16.window,key:\"Enter\"});\nglobalThis.state = await sky.get_window_state({window:observation16.window,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch* Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			9443 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283
> 				9444 창 ID: 59648
> 					9445 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000
> 						9446 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9447 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9448 위치 조정 위치 ID: ScrollbarThumb
> 							9449 단추 페이지 아래로 ID: DownPageButton
> 							9450 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9451 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9452 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9453 위치 조정 위치 ID: ScrollbarThumb
> 							9454 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9455 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9456 위치 조정 (disabled)
> 			9457 창 EtherCAT_Network Secondary Actions: Raise ID: 65286
> 				9458 창 ID: 59648
> 					9459 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						9460 단추 위쪽 스크롤 화살표 ID: UpButton
> 						9461 위치 조정 위치 ID: ScrollbarThumb
> 						9462 단추 페이지 아래로 ID: DownPageButton
> 						9463 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9464 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						9465 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						9466 위치 조정 위치 ID: ScrollbarThumb
> 						9467 단추 페이지 오른쪽으로 ID: DownPageButton
> 						9468 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					9469 위치 조정 (disabled)
> 			9470 창 EtherCAT_Network.lcn Secondary Actions: Raise ID: 65285
> 				9471 창 ID: 59648
> 					9472 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="EtherCAT_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "Elmo_11" GUID = "{47F87ED0-D0EE-486E-BC7A-A70547F6C0D9}" Class = "Elmo_1" Position = "(1500,1620)" Visualized = "true" Remotely = "true"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATOut_1"/> <Server Name="EtherCATState"/> <Server Name="Inputs_DigitalInputs"/> <Server Name="Online"/> <Server Name="Outputs_DigitalOutputs"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{7266B399-596C-4DBB-A5BF-56AC8BC68024}" Class="ECAT_DS402Base"> <Channels> <Server Name="ActPos"/> <Server Name="ActSpeed"/> <Server Name="ActTorque"/> <Server Name="ActXw"/> <Server Name="AL_StatusCode"/> <Server Name="AxEnable"/> <Server Name="AxError"/> <Server Name="AxErrorQuit"/> <Server Name="ClassState"/> <Server Name="ControlWord"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="DriveIF"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SetPos"/> <Server Name="SetSpeed"/> <Server Name="SetTorque"/> <Server Name="SlaveState"/> <Server Name="StateWord"/> <Server Name="VendorID"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="UseDefaultEnableSequence" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{80EAE1CA-1B88-4D0B-86A1-68539F1C74D5}" Class="ECAT_Slave_Base"> <Channels> <Server Name="AL_StatusCode"/> <Server Name="ClassState"/> <Server Name="DeviceAdress"/> <Server Name="DeviceName"/> <Server Name="EtherCATState"/> <Server Name="Online"/> <Server Name="ProductCode"/> <Server Name="ProductRevision"/> <Server Name="RevisionNo"/> <Server Name="SerialNo"/> <Server Name="SlaveState"/> <Server Name="VendorID"/> <Client Name="NoAsyncBuffer" Value="0"/> <Client Name="NoSDOBuffer" Value="0"/> <Client Name="NoSSCBuffer" Value="0"/> <Client Name="Required" Value="1"/> <Client Name="SlaveIndex" Value="1"/> <Client Name="toMaster"/> <Client Name="toStdLib"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components ID: 10000
> 						9473 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9474 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9475 위치 조정 위치 ID: ScrollbarThumb
> 							9476 단추 페이지 아래로 ID: DownPageButton
> 							9477 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9478 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9479 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9480 위치 조정 위치 ID: ScrollbarThumb
> 							9481 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9482 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9483 위치 조정 (disabled)
> 			9484 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				9485 창 ID: 59648
> 					9486 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						9487 단추 위쪽 스크롤 화살표 ID: UpButton
> 						9488 위치 조정 위치 ID: ScrollbarThumb
> 						9489 단추 페이지 아래로 ID: DownPageButton
> 						9490 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9491 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						9492 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						9493 위치 조정 위치 ID: ScrollbarThumb
> 						9494 단추 페이지 오른쪽으로 ID: DownPageButton
> 						9495 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					9496 위치 조정 (disabled)
> 			9497 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282
> 				9498 창 ID: 59648
> 					9499 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // TW[20] is an encoder error/warning reset, not a generic multi-turn // position reset. Keep it dormant until encoder compatibility and continuous // motor-off ownership are implemented and proven on the target drive. #define LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 #define LMC_DIAG_ADMIN_ERROR_ID -31000 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex ID: 10000
> 						9500 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9501 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9502 위치 조정 위치 ID: ScrollbarThumb
> 							9503 단추 페이지 아래로 ID: DownPageButton
> 							9504 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9505 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9506 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9507 위치 조정 위치 ID: ScrollbarThumb
> 							9508 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9509 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9510 위치 조정 (disabled)
> 			9511 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				9512 창 ID: 59648
> 					9513 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						9514 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9515 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9516 위치 조정 위치 ID: ScrollbarThumb
> 							9517 단추 페이지 아래로 ID: DownPageButton
> 							9518 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9519 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9520 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9521 위치 조정 위치 ID: ScrollbarThumb
> 							9522 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9523 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9524 위치 조정 (disabled)
> 			9525 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				9526 창 ID: 59648
> 					9527 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						9528 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9529 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9530 위치 조정 위치 ID: ScrollbarThumb
> 							9531 단추 페이지 아래로 ID: DownPageButton
> 							9532 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9533 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9534 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9535 위치 조정 위치 ID: ScrollbarThumb
> 							9536 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9537 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9538 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				19673 단추 Toggle bookmark
> 				19674 단추 (disabled) Previous bookmark
> 				19675 단추 (disabled) Next bookmark
> 				19676 단추 (disabled) Delete all bookmarks
> 				19677 단추 (disabled) Previous bookmark in this file
> 				19678 단추 (disabled) Next bookmark in this file
> 				19679 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				19680 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				19681 단추 (disabled) Unindent (Shift+Tab)
> 				19682 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				19683 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				19684 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				19685 단추 DataAnalyzer
> 				19686 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				19687 단추 (disabled) Select
> 				19688 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				19689 단추 Go online (Alt+F6)
> 				19690 단추 Change Online Settings
> 				19691 메뉴 항목 Online Connection
> 				19692 단추 (disabled) Set Online Connection For Current Project
> 				19693 단추 (disabled) Download (F6)
> 				19694 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				19695 단추 (disabled) Download Module on the Fly
> 				19696 단추 (disabled) Save Project on PLC
> 				19697 단추 (disabled) Start (F7)
> 				19698 단추 (disabled) Reset (F8)
> 				19699 단추 Toggle breakpoint (F4)
> 				19700 단추 Create condition breakpoint
> 				19701 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				19702 메뉴 항목 Target Architecture
> 				19703 단추 Build changes (F9)
> 				19704 단추 Rebuild project (Strg+F9)
> 				19705 단추 (disabled) Cancel building (Ctrl+Break)
> 				19706 단추 Link project
> 			54 도구 모음 Standard
> 				19707 단추 New project (Strg+N)
> 				19708 단추 Open a file (Strg+Shift+O)
> 				19709 단추 Close active document (Strg+F4)
> 				19710 단추 Save file (Strg+S)
> 				19711 단추 Open project (Strg+O)
> 				19712 단추 Save project changes (Strg+Shift+S)
> 				19713 단추 Close project
> 				19714 단추 Print
> 				19715 단추 Cut (Strg+X)
> 				19716 단추 Copy (Strg+C)
> 				19717 단추 Paste (Strg+V)
> 				19718 메뉴 항목 Undo (Strg+Z)
> 				19719 메뉴 항목 (disabled) Redo (Strg+Y)
> 				19720 단추 Navigate Backward (Alt+Left)
> 				19721 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				19722 메뉴 항목 FILE
> 				19723 메뉴 항목 EDIT
> 				19724 메뉴 항목 VIEW
> 				19725 메뉴 항목 PROJECT
> 				19726 메뉴 항목 BUILD
> 				19727 메뉴 항목 DEBUG
> 				19728 메뉴 항목 ANALYZE
> 				19729 메뉴 항목 TOOLS
> 				19730 메뉴 항목 EXTRAS
> 				19731 메뉴 항목 WINDOW
> 				19732 메뉴 항목 HELP
> 		82 창 Splitter ID: 409230944
> 		83 창 Splitter ID: 409230608
> 		84 Tab Output ID: 303755232
> 			85 창 ID: 1200

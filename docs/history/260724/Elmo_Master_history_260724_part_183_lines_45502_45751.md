> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			10294 탭 항목 (selectable) Python Script
> 			10295 탭 항목 (selectable) Debugger
> 			10296 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 364851552
> 		97 Tab Class View ID: 121361080
> 			7279 트리 ID: 103
> 				7280 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					7281 단추 위쪽 스크롤 화살표 ID: UpButton
> 					7282 위치 조정 위치 ID: ScrollbarThumb
> 					7283 단추 페이지 아래로 ID: DownPageButton
> 					7284 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				7285 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					7286 콘솔 트리 (selectable) External
> 					7287 콘솔 트리 (selectable) Sigmatek
> 					7288 콘솔 트리 (selectable) _TCPIPServer_RT
> 					7289 콘솔 트리 (selectable) Elmo_1
> 					7290 콘솔 트리 (selectable) Elmo_2
> 					7291 콘솔 트리 (selectable) Elmo_3
> 					7292 콘솔 트리 (selectable) Elmo_4
> 					7293 콘솔 트리 (selectable) LMCControlCommandService
> 					7294 콘솔 트리 (selectable) LMCDiagnosticsService
> 					7295 콘솔 트리 (selectable) LMCEcatInputLatch
> 					7296 콘솔 트리 (selectable) LMCRecorderStore
> 					7297 콘솔 트리 (selectable) LMCSdoExecutor
> 					7298 콘솔 트리 (selectable) TCPMotionInterface
> 			10297 탭 항목 (selectable) Lib
> 			10298 탭 항목 (selectable) File
> 			10299 탭 항목 (selectable) Global
> 			10300 탭 항목 (selectable) Net
> 			10301 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 121363360
> 			106 창 ID: 288430568
> 				107 TABLE Properties Window ID: 118941016
> 					7184 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						7185 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7186 위치 조정 위치 ID: ScrollbarThumb
> 						7187 단추 페이지 아래로 ID: DownPageButton
> 						7188 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					10302 custom Name
> 					10303 custom Revision
> 					10304 custom GUID
> 					10305 custom Task Settings
> 					10306 custom Sigmatek
> 					10307 custom OSInterface
> 					10308 custom IconPath
> 					10309 custom SharedCommandTable
> 					10310 custom Objectsize
> 					10311 custom Singleton
> 					10312 custom Hidden
> 					10313 custom Deprecated
> 					10314 custom GCCOptions
> 					10315 custom Comment
> 					10316 custom Filename
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			10317 탭 항목 (selectable) Properties
> 			112 단추 Close
>
> The focused UI element is 7279 트리 ID: 103.
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
>   "code": "{\n  globalThis.state = await sky.get_window_state({ window: targetWindow, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}",
>   "timeout_ms": 30000,
>   "title": "Check variable name focus"
> }
> ```
>
> ```text
> 10381 편집 ID: 1
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
>   "code": "{\n  var observationVarName = globalThis.state;\n  globalThis.state = null;\n  await sky.type_text({ window: observationVarName.window, text: \"GroupMovePos\" });\n  globalThis.state = await sky.get_window_state({ window: observationVarName.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.focused_element || \"\"));\n}",
>   "timeout_ms": 30000,
>   "title": "Name service variable"
> }
> ```
>
> ```text
> 10381 편집 ID: 1
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
>   "code": "{\n  var observationVarCommit = globalThis.state;\n  globalThis.state = null;\n  await sky.press_key({ window: observationVarCommit.window, key: \"Return\" });\n  globalThis.state = await sky.get_window_state({ window: observationVarCommit.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "timeout_ms": 30000,
>   "title": "Commit variable name"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			8460 창 LMCControlCommandService Secondary Actions: Raise ID: 65287
> 				8461 창 ID: 59648
> 					8462 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleAxisCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleGroupCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleAdminCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::MoveLinearAbsEx VAR_INPUT Reference : UINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::GroupReadStatus VAR_INPUT pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION ID: 10000
> 						8463 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							8464 단추 위쪽 스크롤 화살표 ID: UpButton
> 							8465 위치 조정 위치 ID: ScrollbarThumb
> 							8466 단추 페이지 아래로 ID: DownPageButton
> 							8467 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						8468 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							8469 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							8470 위치 조정 위치 ID: ScrollbarThumb
> 							8471 단추 페이지 오른쪽으로 ID: DownPageButton
> 							8472 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						8473 위치 조정 (disabled)
> 			4950 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				4951 창 ID: 59648
> 					4952 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						4953 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4954 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4955 단추 페이지 위로 ID: UpPageButton
> 							4956 위치 조정 위치 ID: ScrollbarThumb
> 							4957 단추 페이지 아래로 ID: DownPageButton
> 							4958 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						4959 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							4960 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							4961 위치 조정 위치 ID: ScrollbarThumb
> 							4962 단추 페이지 오른쪽으로 ID: DownPageButton
> 							4963 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						4964 위치 조정 (disabled)
> 			4965 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				4966 창 ID: 59648
> 					4967 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						4968 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4969 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4970 위치 조정 위치 ID: ScrollbarThumb
> 							4971 단추 페이지 아래로 ID: DownPageButton
> 							4972 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						4973 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							4974 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							4975 위치 조정 위치 ID: ScrollbarThumb
> 							4976 단추 페이지 오른쪽으로 ID: DownPageButton
> 							4977 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						4978 위치 조정 (disabled)
> 			4979 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				4980 창 ID: 59648
> 					4981 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						4982 단추 위쪽 스크롤 화살표 ID: UpButton
> 						4983 위치 조정 위치 ID: ScrollbarThumb
> 						4984 단추 페이지 아래로 ID: DownPageButton
> 						4985 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					4986 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						4987 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						4988 위치 조정 위치 ID: ScrollbarThumb
> 						4989 단추 페이지 오른쪽으로 ID: DownPageButton
> 						4990 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					4991 위치 조정 (disabled)
> 			4992 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				4993 창 ID: 59648
> 					4994 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						4995 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4996 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4997 단추 페이지 위로 ID: UpPageButton
> 							4998 위치 조정 위치 ID: ScrollbarThumb
> 							4999 단추 페이지 아래로 ID: DownPageButton
> 							5000 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						5001 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							5002 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							5003 위치 조정 위치 ID: ScrollbarThumb
> 							5004 단추 페이지 오른쪽으로 ID: DownPageButton
> 							5005 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						5006 위치 조정 (disabled)
> 			5007 창 HW_Network Secondary Actions: Raise ID: 65282
> 				5008 창 ID: 59648
> 					5009 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5010 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5011 위치 조정 위치 ID: ScrollbarThumb
> 						5012 단추 페이지 아래로 ID: DownPageButton
> 						5013 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			5014 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				5015 창 ID: 59648
> 					5016 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5017 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5018 위치 조정 위치 ID: ScrollbarThumb
> 						5019 단추 페이지 아래로 ID: DownPageButton
> 						5020 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					5021 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						5022 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						5023 위치 조정 위치 ID: ScrollbarThumb
> 						5024 단추 페이지 오른쪽으로 ID: DownPageButton
> 						5025 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					5026 위치 조정 (disabled)
> 			5027 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				5028 창 ID: 59648
> 					5029 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5030 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5031 위치 조정 위치 ID: ScrollbarThumb
> 						5032 단추 페이지 아래로 ID: DownPageButton
> 						5033 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					5034 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						5035 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						5036 위치 조정 위치 ID: ScrollbarThumb
> 						5037 단추 페이지 오른쪽으로 ID: DownPageButton
> 						5038 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					5039 위치 조정 (disabled)
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
> 				10998 단추 Toggle bookmark
> 				10999 단추 (disabled) Previous bookmark
> 				11000 단추 (disabled) Next bookmark
> 				11001 단추 (disabled) Delete all bookmarks
> 				11002 단추 (disabled) Previous bookmark in this file
> 				11003 단추 (disabled) Next bookmark in this file
> 				11004 단추 (disabled) Comment selected text (Ctrl+Shift+C)

>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Motion Network 상태 확인",
>   "code": "{\n  var lasalAppIdNow = '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe';\n  globalThis.lasalApp = globalThis.apps.find(a => a.id === lasalAppIdNow);\n  if (!globalThis.lasalApp) throw new Error('LASAL app not returned');\n  var candidatesNow = globalThis.lasalApp.windows.filter(w => /Elmo_EtherCAT_Test_4Axis/.test(w.title || ''));\n  if (candidatesNow.length !== 1) throw new Error('Expected one LASAL project window, found '+candidatesNow.length);\n  globalThis.targetWindow = await sky.get_window({id:candidatesNow[0].id, app:candidatesNow[0].app});\n  await sky.activate_window({window:globalThis.targetWindow});\n  globalThis.state = await sky.get_window_state({window:globalThis.targetWindow, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || ''));\n}"
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
>   "title": "LASAL IDE 오프라인 전환",
>   "code": "{\n  var obsDisconnect = globalThis.state;\n  var shotDisconnect = obsDisconnect?.screenshots?.[0]?.id;\n  if (shotDisconnect == null) throw new Error('No fresh LASAL screenshot id');\n  globalThis.state = null;\n  await sky.click({window:obsDisconnect.window, screenshotId:shotDisconnect, x:632, y:70});\n  globalThis.state = await sky.get_window_state({window:obsDisconnect.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || ''));\n}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO] Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Motion_Network [RO] Secondary Actions: Raise ID: 65280
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 단추 페이지 위로 ID: UpPageButton
> 						7 위치 조정 위치 ID: ScrollbarThumb
> 						8 단추 페이지 아래로 ID: DownPageButton
> 						9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						12 단추 페이지 왼쪽으로 ID: UpPageButton
> 						13 위치 조정 위치 ID: ScrollbarThumb
> 						14 단추 페이지 오른쪽으로 ID: DownPageButton
> 						15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					16 위치 조정 (disabled)
> 			17 창 HW_Network [RO] Secondary Actions: Raise ID: 65288
> 				18 창 ID: 59648
> 					19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21 단추 페이지 위로 ID: UpPageButton
> 						22 위치 조정 위치 ID: ScrollbarThumb
> 						23 단추 페이지 아래로 ID: DownPageButton
> 						24 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					25 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						26 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						27 위치 조정 위치 ID: ScrollbarThumb
> 						28 단추 페이지 오른쪽으로 ID: DownPageButton
> 						29 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					30 위치 조정 (disabled)
> 			31 창 LMCEcatInputLatch [RO] Secondary Actions: Raise ID: 65281
> 				32 창 ID: 59648
> 					33 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						34 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							35 단추 위쪽 스크롤 화살표 ID: UpButton
> 							36 위치 조정 위치 ID: ScrollbarThumb
> 							37 단추 페이지 아래로 ID: DownPageButton
> 							38 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						39 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							40 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							41 위치 조정 위치 ID: ScrollbarThumb
> 							42 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						44 위치 조정 (disabled)
> 					45 창 xtpBarTop ID: 59419
> 						46 도구 모음 Object Editor
> 							47 단추
> 							48 단추 Debugging Context:
> 							49 메뉴 항목 Context
> 			50 창 LMCRecorderStore [RO] Secondary Actions: Raise ID: 65282
> 				51 창 ID: 59648
> 					52 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_RECORDER_SCHEMA_VERSION 1 #define LMC_RECORDER_MAP_REVISION 0x957F101E #define LMC_RECORDER_ERROR_ID -32000 #define LMC_RECORDER_STORAGE_BYTES 1280000 #define LMC_RECORDER_MAX_CHANNELS 24 #define LMC_RECORDER_MAX_CHUNK_BYTES 1280 #define LMC_RECORDER_EMPTY 0 #define LMC_RECORDER_CONFIGURED 1 #define LMC_RECORDER_ARMED 2 #define LMC_RECORDER_RECORDING 3 #define LMC_RECORDER_READY 4 #define LMC_RECORDER_UPLOADING 5 #define LMC_RECORDER_FAULT 6 #define LMC_RECORDER_STOP_NONE 0 #define LMC_RECORDER_STOP_COUNT_COMPLETE 1 #define LMC_RECORDER_STOP_USER 2 #define LMC_RECORDER_STOP_TRIGGER_COMPLETE 3 // The data bank is global so the generated class object stays below the // 16-bit object-size field used by the LASAL class table. Exactly one // LMCRecorderStore object is allowed in the project. VAR_GLOBAL g_LMCRecorderData : ARRAY [0..1279999] OF USINT; END_VAR FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed VAR_INPUT SessionEpoch : UDINT; END_VAR if (SessionEpoch <> 0) (SessionEpoch = OwnerSessionEpoch) then ClosedSessionEpoch := SessionEpoch; end_if; END_FUNCTION FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot VAR_INPUT pSnapshot : ^USINT; SnapshotSize : UDINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR state : UDINT; startRequest : UDINT; triggerRequest : UDINT; stopRequest : UDINT; sequence : UDINT; cycleCounter : UDINT; channelIndex : UINT; dataOffset : UDINT; snapshotOffset : UDINT; triggerRaw : UDINT; triggerHealthOffset : UDINT; triggerEvent : BOOL; triggerInputValid : BOOL; previousCondition : BOOL; currentCondition : BOOL; prehistoryReady : BOOL; previousSigned : DINT; currentSigned : DINT; thresholdSigned : DINT; lowerSigned : DINT; upperSigned : DINT; timestampStep : UDINT; timestampLowBefore : UDINT; END_VAR Result := -1; if (pSnapshot = NIL) | (SnapshotSize < 304) then RETURN; end_if; state := sigclib_atomic_getU32(pValue:=#StateValue); startRequest := sigclib_atomic_getU32(pValue:=#StartRequestSequence); triggerRequest := sigclib_atomic_getU32(pValue:=#TriggerRequestSequence); stopRequest := sigclib_atomic_getU32(pValue:=#StopRequestSequence); sequence := sigclib_atomic_getU32(pValue:=#StatusSequence) + 1; if (sequence and 1) = 0 then sequence += 1; end_if; sigclib_atomic_setU32(pValue:=#StatusSequence, value:=sequence); cycleCounter := pSnapshot^$UDINT; if (state = LMC_RECORDER_ARMED) & (startRequest <> StartAppliedSequence) then StartAppliedSequence := startRequest; SampleCount := 0; DividerCounter := 0; StopReason := LMC_RECORDER_STOP_NONE; TriggerIndex := 0xFFFFFFFF; TriggerCycle := 0; TriggerTimestampLow := 0; TriggerTimestampHigh := 0; StartCycle := 0; EndCycle := 0; StartTimestampLow := 0; StartTimestampHigh := 0; EndTimestampLow := 0; EndTimestampHigh := 0; DroppedCycles := 0; OverflowCount := 0; WriteSampleIndex := 0; FrozenFirstSampleIndex := 0; PostSamplesRemaining := 0; PreviousTriggerValue := 0; PreviousTriggerValid := FALSE; state := LMC_RECORDER_RECORDING; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_RECORDING); end_if; if ((state = LMC_RECORDER_ARMED) | (state = LMC_RECORDER_RECORDING)) & (stopRequest <> StopAppliedSequence) then StopAppliedSequence := stopRequest; if (TriggerType <> 0) & (TriggerIndex = 0xFFFFFFFF) & (SampleCapacity > 0) then FrozenFirstSampleIndex := (WriteSampleIndex + SampleCapacity - SampleCount) MOD SampleCapacity; end_if; StopReason := LMC_RECORDER_STOP_USER; if SampleCount = 0 then EndCycle := cycleCounter; EndTimestampLow := (pSnapshot + 4)^$UDINT; EndTimestampHigh := (pSnapshot + 8)^$UDINT; end_if; state := LMC_RECORDER_READY; sigclib_atomic_setU32( pValue:=#StateValue, value:=LMC_RECORDER_READY); end_if; if state = LMC_RECORDER_RECORDING then if DividerCounter = 0 then if TriggerType = 0 then if SampleCount < SampleCapacity then if SampleCount = 0 then Start ID: 10000
> 						53 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							54 단추 위쪽 스크롤 화살표 ID: UpButton
> 							55 위치 조정 위치 ID: ScrollbarThumb
> 							56 단추 페이지 아래로 ID: DownPageButton
> 							57 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						58 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							59 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							60 위치 조정 위치 ID: ScrollbarThumb
> 							61 단추 페이지 오른쪽으로 ID: DownPageButton
> 							62 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						63 위치 조정 (disabled)
> 					64 창 xtpBarTop ID: 59419
> 						65 도구 모음 Object Editor
> 							66 단추
> 							67 단추 Debugging Context:
> 							68 메뉴 항목 Context
> 			69 창 Comm_Network [RO] Secondary Actions: Raise ID: 65286
> 				70 창 ID: 59648
> 					71 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						72 단추 위쪽 스크롤 화살표 ID: UpButton
> 						73 위치 조정 위치 ID: ScrollbarThumb
> 						74 단추 페이지 아래로 ID: DownPageButton
> 						75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					76 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						77 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						78 위치 조정 위치 ID: ScrollbarThumb
> 						79 단추 페이지 오른쪽으로 ID: DownPageButton
> 						80 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					81 위치 조정 (disabled)
> 			82 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65287
> 				83 창 ID: 59648
> 					84 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						85 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							86 단추 위쪽 스크롤 화살표 ID: UpButton
> 							87 위치 조정 위치 ID: ScrollbarThumb
> 							88 단추 페이지 아래로 ID: DownPageButton
> 							89 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						90 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							91 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							92 위치 조정 위치 ID: ScrollbarThumb
> 							93 단추 페이지 오른쪽으로 ID: DownPageButton
> 							94 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						95 위치 조정 (disabled)
> 					96 창 xtpBarTop ID: 59419
> 						97 도구 모음 Object Editor
> 							98 단추
> 							99 단추 Debugging Context:
> 							100 메뉴 항목 Context
> 			101 창 LMCDiagnosticsService [RO] Secondary Actions: Raise ID: 65283
> 				102 창 ID: 59648
> 					103 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						104 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							105 단추 위쪽 스크롤 화살표 ID: UpButton
> 							106 위치 조정 위치 ID: ScrollbarThumb
> 							107 단추 페이지 아래로 ID: DownPageButton
> 							108 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							110 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							111 위치 조정 위치 ID: ScrollbarThumb
> 							112 단추 페이지 오른쪽으로 ID: DownPageButton
> 							113 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						114 위치 조정 (disabled)
> 					115 창 xtpBarTop ID: 59419
> 						116 도구 모음 Object Editor
> 							117 단추
> 							118 단추 Debugging Context:
> 							119 메뉴 항목 Context
> 			120 창 _TCPIPServer_RT [RO] Secondary Actions: Raise ID: 65289
> 				121 창 ID: 59648
> 					122 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						123 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							124 단추 위쪽 스크롤 화살표 ID: UpButton
> 							125 위치 조정 위치 ID: ScrollbarThumb
> 							126 단추 페이지 아래로 ID: DownPageButton
> 							127 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						128 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							129 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							130 위치 조정 위치 ID: ScrollbarThumb
> 							131 단추 페이지 오른쪽으로 ID: DownPageButton
> 							132 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						133 위치 조정 (disabled)
> 					134 창 xtpBarTop ID: 59419
> 						135 도구 모음 Object Editor
> 							136 단추 (disabled)
> 							137 단추 (disabled) Debugging Context:
> 							138 메뉴 항목 (disabled) Context
> 			139 창 ECAT_DS402Base [RO] Secondary Actions: Raise ID: 65285
> 				140 창 ID: 59648
> 					141 창 FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::Init ECAT_Slave_Base::Init(); if _FirstScan then UseDefaultEnableSequence := UseDefaultEnableSequence.Read() <> 0; sigclib_atomic_setU32(pValue:=#sDriveIFSDO.udIsSDOActive, value:=ECAT_SB_SDO_FREE); end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRt // Update AxError if StateWord.Fault = FALSE then AxError := 0; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; else // Set AxError if Fault is set in Stateword and Error is not beeing quit. if AxErrorQuit = 0 then AxError := 1; elsif (ops.tabsolute - TimeStampResetFault) >= GetQuitErrorTimeout() then AxError := 1; AxErrorQuit := 0; ControlWord.ResetFault := FALSE; end_if; end_if; END_FUNCTION FUNCTION VIRTUAL GLOBAL ECAT_DS402Base::UpdateRtPostScan if AxError = 0 then if UseDefaultEnableSequence then EnableSequence(); end_if; end_if; END_FUNCTION FUNCTION VIRTUAL ECAT_DS402Base::EnableSequence //************************************************************************************************** // ENABLE/DISABLE SEQUENCE //************************************************************************************************** case EnableSequenceSSW of //************************************************************************************************** e_EnableSequenceSSW::_Disabled: // do nothing special, we're enabled now e_EnableSequenceSSW::_Enabled: // do nothing special, we're disabled now //************************************************************************************************** e_EnableSequenceSSW::_StartEnable: ControlWord.SwitchOn := FALSE; ControlWord.EnableVoltage := TRUE; ControlWord.QuickStop := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _SwitchOn; //************************************************************************************************** e_EnableSequenceSSW::_SwitchOn: if StateWord.VoltageEnabled StateWord.QuickStop & StateWord.ReadyToSwitchOn then ControlWord.SwitchOn := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _EnableOperation; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_EnableOperation: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn then ControlWord.EnableOperation := TRUE; EnableStartTime := ops.tAbsolute; EnableSequenceSSW := _CheckOperationEnabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_CheckOperationEnabled: if StateWord.VoltageEnabled & StateWord.QuickStop & StateWord.ReadyToSwitchOn & StateWord.SwitchedOn & StateWord.OperationEnabled then EnableSequenceSSW := _Enabled; elsif (ops.tAbsolute - EnableStartTime) > GetEnableTimeout() then // after 1 sec we stop trying => disable AxEnable.Write(0); end_if; //************************************************************************************************** e_EnableSequenceSSW::_DisableOperation: if StateWord.OperationEnabled = 0 then AxEnable := 0; EnableSequenceSSW := _Disabled; end_if; //**************************************************************************** ID: 10000
> 						142 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							143 단추 위쪽 스크롤 화살표 ID: UpButton
> 							144 단추 페이지 위로 ID: UpPageButton
> 							145 위치 조정 위치 ID: ScrollbarThumb
> 							146 단추 페이지 아래로 ID: DownPageButton
> 							147 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						148 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							149 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							150 위치 조정 위치 ID: ScrollbarThumb
> 							151 단추 페이지 오른쪽으로 ID: DownPageButton
> 							152 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						153 위치 조정 (disabled)
> 					154 창 xtpBarTop ID: 59419
> 						155 도구 모음 Object Editor
> 							156 단추
> 							157 단추 Debugging Context:
> 							158 메뉴 항목 Context
> 			159 창 Elmo_2 [RO] Secondary Actions: Raise ID: 65284
> 				160 창 ID: 59648
> 					161 창 FUNCTION VIRTUAL GLOBAL Elmo_2::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_2_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_2_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_2_SETPOS_INDEX, ELMO_2_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_2_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_2_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_2_CONTROLWORD_INDEX, ELMO_2_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_2_ACTPOS_INDEX, ELMO_2_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_2_INPUTS_DIGITALINPUTS_INDEX, ELMO_2_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_2_STATEWORD_INDEX, ELMO_2_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						162 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							163 단추 위쪽 스크롤 화살표 ID: UpButton
> 							164 위치 조정 위치 ID: ScrollbarThumb
> 							165 단추 페이지 아래로 ID: DownPageButton
> 							166 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						167 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							168 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							169 위치 조정 위치 ID: ScrollbarThumb
> 							170 단추 페이지 오른쪽으로 ID: DownPageButton
> 							171 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						172 위치 조정 (disabled)
> 					173 창 xtpBarTop ID: 59419
> 						174 도구 모음 Object Editor
> 							175 단추
> 							176 단추 Debugging Context:
> 							177 메뉴 항목 Context
> 		178 상태 표시줄 ID: 59393
> 			179 텍스트
> 			180 텍스트
> 			181 텍스트
> 			182 텍스트
> 			183 텍스트
> 			184 텍스트
> 			185 텍스트
> 			186 텍스트
> 			187 텍스트
> 			188 텍스트 NUM
> 			189 텍스트
> 		190 창 xtpBarTop ID: 59419
> 			191 도구 모음 Script
> 			192 도구 모음 Edit
> 				193 단추 (disabled) Toggle bookmark
> 				194 단추 (disabled) Previous bookmark
> 				195 단추 (disabled) Next bookmark
> 				196 단추 (disabled) Delete all bookmarks
> 				197 단추 (disabled) Previous bookmark in this file
> 				198 단추 (disabled) Next bookmark in this file
> 				199 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				200 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				201 단추 (disabled) Unindent (Shift+Tab)
> 				202 단추 (disabled) Indent (Tab)
> 			203 도구 모음 Macros Manager
> 				204 메뉴 항목 Macros
> 			205 도구 모음 Layout Manager
> 				206 메뉴 항목 Layouts
> 			207 도구 모음 Toolbox
> 				208 단추 DataAnalyzer
> 				209 메뉴 항목 Toolbar Options
> 			210 도구 모음 Net Edit
> 				211 단추 Select
> 				212 메뉴 항목 Toolbar Options
> 			213 도구 모음 Debug
> 				214 단추 Go online (Alt+F6)
> 				215 단추 (disabled) Change Online Settings
> 				216 메뉴 항목 Online Connection
> 				217 단추 (disabled) Set Online Connection For Current Project
> 				218 단추 Download (F6)
> 				219 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				220 단추 Download Module on the Fly

> 		269 Tab Global View ID: 409868448
> 			270 트리 ID: 105
> 				271 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					272 콘솔 트리 (selectable) Types
> 					273 콘솔 트리 (selectable) Variables
> 			274 탭 항목 (selectable) Lib
> 			275 탭 항목 (selectable) File
> 			276 탭 항목 (selectable) Global
> 			277 단추 Close
> 		278 Tab Properties ID: 409871640
> 			279 창 ID: 121918456
> 				280 TABLE Properties Window ID: 127184512
> 				281 도구 모음 ID: 59392
> 					282 단추
> 					283 단추
> 			284 탭 항목 (selectable) Properties
> 			285 단추 Close
> 		286 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			287 단추 예(Y) ID: 6
> 			288 단추 아니요(N) ID: 7
> 			289 이미지 ID: 20
> 			290 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			291 제목 표시줄
> 				292 단추 (disabled) 닫기
>
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
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
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"Need dialog observation\");\n  globalThis.state = null;\n  await sky.click({window: observation.window, element_index: 288});\n  globalThis.state = await sky.get_window_state({window: observation.window, include_screenshot: true, include_text: true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "title": "미사용 라이브러리 유지",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 288 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.allWindows = await sky.list_windows();\nnodeRepl.write(JSON.stringify(allWindows.filter(w => (w.app || \"\").toLowerCase().includes(\"lasal\") || (w.title || \"\").toLowerCase().includes(\"lasal\")), null, 2));",
>   "title": "LASAL 대화상자 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 3084742,
>     "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 1837062,
>     "title": "Lasal Class 2 (02.03.001)"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe",
>     "id": 68062,
>     "title": "LasalApiWpfTestApp - Microsoft Visual Studio"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.targetWindow = await sky.get_window({id: 3084742, app: \"process:C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\"});\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: false, include_text: true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));",
>   "title": "LASAL 대화상자 새로 읽기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			2 단추 예(Y) ID: 6
> 			3 단추 아니요(N) ID: 7
> 			4 이미지 ID: 20
> 			5 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			6 제목 표시줄
> 				7 단추 (disabled) 닫기
> 		8 창 작업 영역 ID: 59648
> 			9 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				10 창 ID: 59648
> 					11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 단추 페이지 위로 ID: UpPageButton
> 							15 위치 조정 위치 ID: ScrollbarThumb
> 							16 단추 페이지 아래로 ID: DownPageButton
> 							17 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						18 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							19 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							20 위치 조정 위치 ID: ScrollbarThumb
> 							21 단추 페이지 오른쪽으로 ID: DownPageButton
> 							22 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						23 위치 조정 (disabled)
> 			24 창 Comm_Network Secondary Actions: Raise ID: 65288
> 				25 창 ID: 59648
> 					26 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						27 단추 위쪽 스크롤 화살표 ID: UpButton
> 						28 위치 조정 위치 ID: ScrollbarThumb
> 						29 단추 페이지 아래로 ID: DownPageButton
> 						30 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					31 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						32 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						33 위치 조정 위치 ID: ScrollbarThumb
> 						34 단추 페이지 오른쪽으로 ID: DownPageButton
> 						35 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					36 위치 조정 (disabled)
> 			37 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287
> 				38 창 ID: 59648
> 					39 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						40 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							41 단추 위쪽 스크롤 화살표 ID: UpButton
> 							42 단추 페이지 위로 ID: UpPageButton
> 							43 위치 조정 위치 ID: ScrollbarThumb
> 							44 단추 페이지 아래로 ID: DownPageButton
> 							45 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						46 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							47 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							48 위치 조정 위치 ID: ScrollbarThumb
> 							49 단추 페이지 오른쪽으로 ID: DownPageButton
> 							50 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						51 위치 조정 (disabled)
> 			52 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65286
> 				53 창 ID: 59648
> 					54 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						55 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							56 단추 위쪽 스크롤 화살표 ID: UpButton
> 							57 단추 페이지 위로 ID: UpPageButton
> 							58 위치 조정 위치 ID: ScrollbarThumb
> 							59 단추 페이지 아래로 ID: DownPageButton
> 							60 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						61 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							62 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							63 위치 조정 위치 ID: ScrollbarThumb
> 							64 단추 페이지 오른쪽으로 ID: DownPageButton
> 							65 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						66 위치 조정 (disabled)
> 			67 창 Hardware Editor Secondary Actions: Raise ID: 65285
> 				68 창 ID: 59648
> 					69 창 xtpBarTop ID: 59419
> 						70 도구 모음 Hardware Editor
> 							71 단추 Hardware Editor Configuration Settings
> 							72 단추 Calculate Resources of Project
> 							73 단추 (disabled) Upload Hardware Tree from PLC
> 							74 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							75 단추 Show Detail Mode\Show Configuration Mode
> 							76 단추 Generates the ENI File of the current project
> 					77 창 ID: 59648
> 						78 트리 ID: 1
> 							79 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								80 단추 위쪽 스크롤 화살표 ID: UpButton
> 								81 단추 페이지 위로 ID: UpPageButton
> 								82 위치 조정 위치 ID: ScrollbarThumb
> 								83 단추 페이지 아래로 ID: DownPageButton
> 								84 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							85 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								86 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								87 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]->
> 								88 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								89 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]->
> 								90 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								91 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								92 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								93 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								94 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								95 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								96 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]->
> 								97 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								98 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								99 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]->
> 								100 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								101 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								102 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								103 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								104 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								105 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								106 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								107 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								108 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								109 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							110 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			111 창 Elmo_4 Secondary Actions: Raise ID: 65284
> 				112 창 ID: 59648
> 					113 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						114 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							115 단추 위쪽 스크롤 화살표 ID: UpButton
> 							116 위치 조정 위치 ID: ScrollbarThumb
> 							117 단추 페이지 아래로 ID: DownPageButton
> 							118 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						119 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							120 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 오른쪽으로 ID: DownPageButton
> 							123 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						124 위치 조정 (disabled)
> 			125 창 Motion_Network Secondary Actions: Raise ID: 65283
> 				126 창 ID: 59648
> 					127 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						128 단추 위쪽 스크롤 화살표 ID: UpButton
> 						129 위치 조정 위치 ID: ScrollbarThumb
> 						130 단추 페이지 아래로 ID: DownPageButton
> 						131 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					132 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						133 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						134 위치 조정 위치 ID: ScrollbarThumb
> 						135 단추 페이지 오른쪽으로 ID: DownPageButton
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					137 위치 조정 (disabled)
> 			138 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				139 창 ID: 59648
> 					140 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						141 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							142 단추 위쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 아래로 ID: DownPageButton
> 							145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							148 위치 조정 위치 ID: ScrollbarThumb
> 							149 단추 페이지 오른쪽으로 ID: DownPageButton
> 							150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						151 위치 조정 (disabled)
> 			152 창 HW_Network Secondary Actions: Raise ID: 65281
> 				153 창 ID: 59648

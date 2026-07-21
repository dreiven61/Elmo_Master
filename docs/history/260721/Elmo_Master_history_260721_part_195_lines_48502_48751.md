>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:globalThis.targetWindow,x:86,y:42,screenshotId:\"screenshot-1\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeEdit2=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeEdit2.split(\"\\n\").filter(x=>/Find|Search|Implementation|Reference|찾기|검색|Replace/i.test(x)).slice(-120).join(\"\\n\")); for (const s of globalThis.state.screenshots.filter(s=>s.zIndex===Math.max(...globalThis.state.screenshots.map(x=>x.zIndex)))) await nodeRepl.emitImage(s.url);",
>   "title": "Open LASAL edit menu",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 						236 메뉴 항목 Find... Ctrl+F
> 						237 메뉴 항목 Find in Files... Ctrl+Shift+F
> 						238 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 						239 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 						240 메뉴 항목 Replace... Ctrl+R
> 						241 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			316 메뉴 항목 Find... Ctrl+F
> 			317 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			318 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			319 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			320 메뉴 항목 Replace... Ctrl+R
> 			321 메뉴 항목 Replace in Files... Ctrl+Shift+R
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:\"Escape\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeTabs=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeTabs.split(\"\\n\").filter(x=>/탭 항목|tab item|Tab Global|Lib$|File$|Global$/.test(x)).slice(-80).join(\"\\n\"));",
>   "title": "Locate LASAL project tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 			311 탭 항목 (selectable) Python Script
> 			312 탭 항목 (selectable) Debugger
> 			313 탭 항목 (selectable) Output
> 		316 Tab Global View ID: 409868448
> 			321 탭 항목 (selectable) Lib
> 			322 탭 항목 (selectable) File
> 			323 탭 항목 (selectable) Global
> 			331 탭 항목 (selectable) Properties
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
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:322}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeFile=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeFile.split(\"\\n\").filter(x=>/LMCRecorder|TCPMotion|Class|Include|Network|File|Global View|console tree|콘솔 트리/i.test(x)).slice(-160).join(\"\\n\"));",
>   "title": "Inspect LASAL file tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 322 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))));",
>   "title": "Refresh LASAL tree view",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753}]
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
>   "code": "await sky.click({window:globalThis.targetWindow,x:1025,y:533,screenshotId:\"screenshot-1\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeFile2=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeFile2.split(\"\\n\").filter(x=>/LMCRecorder|TCPMotion|Class|Include|Network|console tree|콘솔 트리|Project/i.test(x)).slice(-180).join(\"\\n\"));",
>   "title": "Open LASAL file tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 					19 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 					34 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 							52 단추 Calculate Resources of Project
> 							54 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							56 단추 Generates the ENI File of the current project
> 							65 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								66 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								67 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								68 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								69 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								70 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								71 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								72 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								73 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								74 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								75 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								76 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								77 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								78 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								79 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								80 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								81 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								82 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								83 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								84 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								85 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								86 콘솔 트리 (selectable) ALARM:00, Empty
> 								87 콘솔 트리 (selectable) SDIAS:00, Empty
> 								88 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								89 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							90 콘솔 트리 (selectable) Unplaced Module(s)
> 			105 창 Motion_Network Secondary Actions: Raise ID: 65284
> 			118 창 Comm_Network Secondary Actions: Raise ID: 65283
> 			145 창 HW_Network Secondary Actions: Raise ID: 65281
> 			160 텍스트 TCPMotionInterface::MsgPaser
> 				195 단추 (disabled) Set Online Connection For Current Project
> 				199 단추 (disabled) Save Project on PLC
> 				208 단추 Rebuild project (Strg+F9)
> 				210 단추 Link project
> 				212 단추 New project (Strg+N)
> 				216 단추 Open project (Strg+O)
> 				217 단추 (disabled) Save project changes (Strg+Shift+S)
> 				218 단추 Close project
> 				231 메뉴 항목 PROJECT
> 				274 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					275 콘솔 트리 (selectable) Types
> 					276 콘솔 트리 (selectable) Variables
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000.
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
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); var linesFile=(globalThis.state.accessibility?.tree||\"\").split(\"\\n\"); nodeRepl.write(linesFile.slice(-100).join(\"\\n\"));",
>   "title": "Read LASAL file nodes",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 				228 메뉴 항목 FILE
> 				229 메뉴 항목 EDIT
> 				230 메뉴 항목 VIEW
> 				231 메뉴 항목 PROJECT
> 				232 메뉴 항목 BUILD
> 				233 메뉴 항목 DEBUG
> 				234 메뉴 항목 ANALYZE
> 				235 메뉴 항목 TOOLS
> 				236 메뉴 항목 EXTRAS
> 				237 메뉴 항목 WINDOW
> 				238 메뉴 항목 HELP
> 		239 창 Splitter ID: 481389416
> 		240 창 Splitter ID: 481390928
> 		241 Tab Output ID: 409867992
> 			242 창 ID: 1200
> 				243 창 ID: 1200
> 					244 LIST ID: 1201
> 						245 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							246 단추 위쪽 스크롤 화살표 ID: UpButton
> 							247 단추 페이지 위로 ID: UpPageButton
> 							248 위치 조정 위치 ID: ScrollbarThumb
> 							249 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 						260 목록 항목 (selectable)
> 						261 목록 항목 (selectable)
> 						262 목록 항목 (selectable)
> 					263 스크롤 막대 ID: 59904
> 						264 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						265 위치 조정 위치 ID: ScrollbarThumb
> 						266 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			267 탭 항목 (selectable) Python Script
> 			268 탭 항목 (selectable) Debugger
> 			269 탭 항목 (selectable) Output
> 			270 단추 Close
> 		271 창 Splitter ID: 608798568
> 		272 Tab File Explorer ID: 409868448
> 			273 트리 ID: 102
> 				274 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					275 콘솔 트리 (selectable) Additional Files
> 					276 콘솔 트리 (selectable) Header Files
> 					277 콘솔 트리 (selectable) Source Files
> 					278 콘솔 트리 (selectable) Network Files
> 					279 콘솔 트리 (selectable) Object Files
> 					280 콘솔 트리 (selectable) Library Files
> 					281 콘솔 트리 (selectable) Dias Drive Files
> 					282 콘솔 트리 (selectable) Connection Files
> 			283 탭 항목 (selectable) Lib
> 			284 탭 항목 (selectable) File
> 			285 탭 항목 (selectable) Global
> 			286 단추 Close

> 			278 창 ID: 121918456
> 				279 TABLE Properties Window ID: 127184512
> 				280 도구 모음 ID: 59392
> 					281 단추
> 					282 단추
> 			283 탭 항목 (selectable) Properties
> 			284 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000.
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
>   "title": "LASAL 진단 클래스 트리 펼치기",
>   "code": "{\n  var observationPlus = globalThis.state;\n  var shotPlus = observationPlus.screenshots?.[0]?.id;\n  globalThis.state = null;\n  await sky.click({ window: observationPlus.window, screenshotId: shotPlus, x: 965, y: 322 });\n  globalThis.state = await sky.get_window_state({ window: observationPlus.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> call get_window_state before issuing coordinate input
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 캡처 식별자 확인",
>   "code": "nodeRepl.write(JSON.stringify(globalThis.state?.screenshots?.map(s=>({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY,z:s.zIndex}))));"
> }
> ```
>
> ```text
> undefined
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 위치 재관찰",
>   "code": "globalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: false });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY,z:s.zIndex}))));"
> }
> ```
>
> ```text
> [{"id":"screenshot-0","width":1450,"height":763,"originX":-1880,"originY":41,"z":0},{"id":"screenshot-1","width":1440,"height":753,"originX":-1875,"originY":46,"z":1}]
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
>   "title": "LASAL 진단 클래스 선택 상태 확보",
>   "code": "await sky.activate_window({ window: globalThis.targetWindow });\nglobalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: false, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nvar classLines = String(globalThis.state.accessibility?.tree || \"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Tab Class View|콘솔 트리/.test(x));\nnodeRepl.write(classLines.join(\"\\n\"));"
> }
> ```
>
> ```text
> 							35 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								36 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								37 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								38 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								39 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								40 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								41 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								42 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								43 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								44 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								45 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								46 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								47 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								48 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								49 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								50 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								51 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								52 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								53 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								54 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								55 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								56 콘솔 트리 (selectable) ALARM:00, Empty
> 								57 콘솔 트리 (selectable) SDIAS:00, Empty
> 								58 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								59 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							60 콘솔 트리 (selectable) Unplaced Module(s)
> 		257 Tab Class View ID: 409868448
> 				259 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					260 콘솔 트리 (selectable) External
> 					261 콘솔 트리 (selectable) Sigmatek
> 					262 콘솔 트리 (selectable) _TCPIPServer_RT
> 					263 콘솔 트리 (selectable) Elmo_1
> 					264 콘솔 트리 (selectable) Elmo_2
> 					265 콘솔 트리 (selectable) Elmo_3
> 					266 콘솔 트리 (selectable) Elmo_4
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 					269 콘솔 트리 (selectable) LMCRecorderStore
> 					270 콘솔 트리 (selectable) TCPMotionInterface
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 펼치기",
>   "code": "{\n  var observationClassClick = globalThis.state;\n  globalThis.state = null;\n  await sky.click({ window: observationClassClick.window, element_index: 267 });\n  globalThis.state = await sky.get_window_state({ window: observationClassClick.window, include_screenshot: false, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  var linesAfterClick = String(globalThis.state.accessibility?.tree || \"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Servers|Clients|Methods|Variables/.test(x));\n  nodeRepl.write(linesAfterClick.join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 					130 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 열기",
>   "code": "{\n  var observationClassClick2 = globalThis.state;\n  globalThis.state = null;\n  await sky.click({ window: observationClassClick2.window, element_index: 267 });\n  globalThis.state = await sky.get_window_state({ window: observationClassClick2.window, include_screenshot: false, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  var linesAfterClick2 = String(globalThis.state.accessibility?.tree || \"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Servers|Clients|Methods|Variables|DiagnosticsBoot/.test(x));\n  nodeRepl.write(linesAfterClick2.join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 					130 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 진단 클래스 트리 확장",
>   "code": "{\n  var observationRight = globalThis.state;\n  globalThis.state = null;\n  await sky.press_key({ window: observationRight.window, key: \"Right\" });\n  globalThis.state = await sky.get_window_state({ window: observationRight.window, include_screenshot: false, include_text: true });\n  globalThis.targetWindow = globalThis.state.window;\n  var linesAfterRight = String(globalThis.state.accessibility?.tree || \"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Servers|Clients|Methods|Variables|DiagnosticsBoot/.test(x));\n  nodeRepl.write(linesAfterRight.join(\"\\n\"));\n}"
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
>   "title": "LASAL 트리 확장 결과 확인",
>   "code": "nodeRepl.write(String(globalThis.state?.accessibility?.tree || globalThis.state?.accessibility?.document_text || \"\"));"
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
>   "title": "LASAL 진단 소스 파일 열기",
>   "code": "await sky.activate_window({ window: globalThis.targetWindow });\nawait sky.press_key({ window: globalThis.targetWindow, key: \"Control_L+Shift_L+o\" });\nawait new Promise(r => setTimeout(r, 500));\nglobalThis.state = await sky.get_window_state({ window: globalThis.targetWindow, include_screenshot: true, include_text: true });\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65287
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton
> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 xtpBarTop ID: 59419
> 						20 도구 모음 Hardware Editor
> 							21 단추 Hardware Editor Configuration Settings
> 							22 단추 Calculate Resources of Project
> 							23 단추 (disabled) Upload Hardware Tree from PLC
> 							24 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							25 단추 Show Detail Mode\Show Configuration Mode
> 							26 단추 Generates the ENI File of the current project
> 					27 창 ID: 59648
> 						28 트리 ID: 1
> 							29 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								30 단추 위쪽 스크롤 화살표 ID: UpButton
> 								31 단추 페이지 위로 ID: UpPageButton
> 								32 위치 조정 위치 ID: ScrollbarThumb
> 								33 단추 페이지 아래로 ID: DownPageButton
> 								34 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							35 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								36 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								37 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								38 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								39 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								40 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								41 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								42 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								43 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								44 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								45 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								46 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								47 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								48 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								49 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								50 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								51 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								52 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								53 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								54 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								55 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								56 콘솔 트리 (selectable) ALARM:00, Empty
> 								57 콘솔 트리 (selectable) SDIAS:00, Empty

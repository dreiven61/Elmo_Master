> 					288 custom Hidden
> 					289 custom Deprecated
> 					290 custom GCCOptions
> 					291 custom Comment
> 					292 custom Filename
> 				293 도구 모음 ID: 59392
> 					294 단추
> 					295 단추
> 			296 탭 항목 (selectable) Properties
> 			297 단추 Close
>
> The focused UI element is 251 트리 ID: 103.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Observe LASAL selected class",
>   "code": "globalThis.state2 = await sky.get_window_state({window:targetWindow2, include_screenshot:true, include_text:true});\nglobalThis.targetWindow2 = state2.window;\nnodeRepl.write(JSON.stringify({focus:state2.accessibility?.focused_element, selected:state2.accessibility?.selected_elements, menu:(state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/메뉴 항목|Find in Implementation|LMCDiagnosticsService/.test(x)).slice(0,80)}, null, 2));"
> }
> ```
>
> ```text
> {
>   "focus": "225 트리 ID: 103",
>   "menu": [
>     "\t\t\t\t152 메뉴 항목 Macros",
>     "\t\t\t\t154 메뉴 항목 Layouts",
>     "\t\t\t\t157 메뉴 항목 Toolbar Options",
>     "\t\t\t\t160 메뉴 항목 Toolbar Options",
>     "\t\t\t\t164 메뉴 항목 Online Connection",
>     "\t\t\t\t174 메뉴 항목 Toolbar Options",
>     "\t\t\t\t176 메뉴 항목 Target Architecture",
>     "\t\t\t\t193 메뉴 항목 Undo (Strg+Z)",
>     "\t\t\t\t194 메뉴 항목 (disabled) Redo (Strg+Y)",
>     "\t\t\t\t198 메뉴 항목 FILE",
>     "\t\t\t\t199 메뉴 항목 EDIT",
>     "\t\t\t\t200 메뉴 항목 VIEW",
>     "\t\t\t\t201 메뉴 항목 PROJECT",
>     "\t\t\t\t202 메뉴 항목 BUILD",
>     "\t\t\t\t203 메뉴 항목 DEBUG",
>     "\t\t\t\t204 메뉴 항목 ANALYZE",
>     "\t\t\t\t205 메뉴 항목 TOOLS",
>     "\t\t\t\t206 메뉴 항목 EXTRAS",
>     "\t\t\t\t207 메뉴 항목 WINDOW",
>     "\t\t\t\t208 메뉴 항목 HELP",
>     "\t\t\t\t\t234 콘솔 트리 (selectable) LMCDiagnosticsService"
>   ]
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
>   "title": "Expand LASAL diagnostics class",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.press_key({window:observation.window, key:\"Right\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|InputLatch|RecorderStore|ClassSvr|HandleRequest/.test(x)).slice(0,120).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 								62 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 					234 콘솔 트리 (selectable) LMCDiagnosticsService
> 					241 콘솔 트리 (selectable) LMCEcatInputLatch
> 					242 콘솔 트리 (selectable) LMCRecorderStore
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
>   "title": "Inspect LASAL diagnostics class members",
>   "code": "globalThis.state2 = await sky.get_window_state({window:targetWindow2, include_screenshot:false, include_text:true});\nglobalThis.targetWindow2 = state2.window;\nnodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/LMCDiagnosticsService|Servers|Clients|Methods|Variables|Objects|Dependencies/.test(x)).slice(0,80).join(\"\\n\"));"
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 					234 콘솔 트리 (selectable) LMCDiagnosticsService
> 						235 콘솔 트리 (selectable) Servers
> 						236 콘솔 트리 (selectable) Clients
> 						237 콘솔 트리 (selectable) Methods
> 						238 콘솔 트리 (selectable) Variables
> 						239 콘솔 트리 (selectable) Objects
> 						240 콘솔 트리 (selectable) Dependencies
> 					266 custom Objectsize
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Select LASAL diagnostics clients",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, element_index:236});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:false, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(JSON.stringify({focus:state2.accessibility?.focused_element, selected:state2.accessibility?.selected_elements}, null, 2));\n}"
> }
> ```
>
> ```text
> {
>   "focus": "225 트리 ID: 103"
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Expand LASAL diagnostics clients",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.press_key({window:observation.window, key:\"Right\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Clients|InputLatch|RecorderStore/.test(x)).slice(0,80).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 								62 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 						236 콘솔 트리 (selectable) Clients
> 					241 콘솔 트리 (selectable) LMCEcatInputLatch
> 					242 콘솔 트리 (selectable) LMCRecorderStore
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
>   "title": "Inspect LASAL diagnostics client nodes",
>   "code": "globalThis.state2 = await sky.get_window_state({window:targetWindow2, include_screenshot:false, include_text:true});\nglobalThis.targetWindow2 = state2.window;\nnodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Clients|InputLatch|RecorderStore/.test(x)).slice(0,80).join(\"\\n\"));"
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 								62 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 						241 콘솔 트리 (selectable) Clients
> 							242 콘솔 트리 (selectable) InputLatch
> 							243 콘솔 트리 (selectable) RecorderStore
> 					248 콘솔 트리 (selectable) LMCEcatInputLatch
> 					249 콘솔 트리 (selectable) LMCRecorderStore
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open LASAL Find in Implementation menu",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, element_index:242, mouse_button:\"right\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Find|Implementation|InputLatch|menu|메뉴/.test(x)).slice(-100).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 								62 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 				152 메뉴 항목 Macros
> 				154 메뉴 항목 Layouts
> 				157 메뉴 항목 Toolbar Options
> 				160 메뉴 항목 Toolbar Options
> 				164 메뉴 항목 Online Connection
> 				174 메뉴 항목 Toolbar Options
> 				176 메뉴 항목 Target Architecture
> 				193 메뉴 항목 Undo (Strg+Z)
> 				194 메뉴 항목 (disabled) Redo (Strg+Y)
> 			197 메뉴 모음 Menu Bar
> 				198 메뉴 항목 FILE
> 				199 메뉴 항목 EDIT
> 				200 메뉴 항목 VIEW
> 				201 메뉴 항목 PROJECT
> 				202 메뉴 항목 BUILD
> 				203 메뉴 항목 DEBUG
> 				204 메뉴 항목 ANALYZE
> 				205 메뉴 항목 TOOLS
> 				206 메뉴 항목 EXTRAS
> 				207 메뉴 항목 WINDOW
> 				208 메뉴 항목 HELP
> 							242 콘솔 트리 (selectable) InputLatch
> 					248 콘솔 트리 (selectable) LMCEcatInputLatch
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
>   "title": "Dismiss LASAL client menu",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.press_key({window:observation.window, key:\"Escape\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:false});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(\"Client menu dismissed; Motion_Network tab is visible.\");\n}"
> }
> ```
>
> ```text
> Client menu dismissed; Motion_Network tab is visible.
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
>   "title": "Open LASAL Motion Network",
>   "code": "{\n  const observation = globalThis.state2;\n  const screenshotId = observation.screenshots?.[0]?.id;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId, x:505, y:145});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(JSON.stringify({title:state2.window.title, focus:state2.accessibility?.focused_element}, null, 2));\n}"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface",
>   "focus": "225 트리 ID: 103"
> }
> ```
>
> Image output: image/jpeg

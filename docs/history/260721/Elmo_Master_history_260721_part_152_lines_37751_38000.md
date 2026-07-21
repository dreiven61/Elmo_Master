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
>   "title": "Open EcatMaster implementation search",
>   "code": "{\n  const observation = globalThis.state2;\n  const screenshotId = observation.screenshots?.[0]?.id;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId, x:82, y:230, mouse_button:\"right\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write((state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Find|Implementation|Reference|menu|메뉴/.test(x)).slice(-100).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 					18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ID: 10000
> 				153 메뉴 항목 Macros
> 				155 메뉴 항목 Layouts
> 				158 메뉴 항목 Toolbar Options
> 				161 메뉴 항목 Toolbar Options
> 				165 메뉴 항목 Online Connection
> 				175 메뉴 항목 Toolbar Options
> 				177 메뉴 항목 Target Architecture
> 				194 메뉴 항목 Undo (Strg+Z)
> 				195 메뉴 항목 (disabled) Redo (Strg+Y)
> 			198 메뉴 모음 Menu Bar
> 				199 메뉴 항목 FILE
> 				200 메뉴 항목 EDIT
> 				201 메뉴 항목 NETEDIT
> 				202 메뉴 항목 VIEW
> 				203 메뉴 항목 PROJECT
> 				204 메뉴 항목 BUILD
> 				205 메뉴 항목 DEBUG
> 				206 메뉴 항목 ANALYZE
> 				207 메뉴 항목 TOOLS
> 				208 메뉴 항목 EXTRAS
> 				209 메뉴 항목 WINDOW
> 				210 메뉴 항목 HELP
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
>   "title": "Inspect EcatMaster search menu",
>   "code": "nodeRepl.write(JSON.stringify(state2.screenshots.map((s,i)=>({i,id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height})),null,2));"
> }
> ```
>
> ```text
> [
>   {
>     "i": 0,
>     "id": "screenshot-0",
>     "originX": -1880,
>     "originY": 41,
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "i": 1,
>     "id": "screenshot-1",
>     "originX": -1875,
>     "originY": 46,
>     "width": 1440,
>     "height": 753
>   },
>   {
>     "i": 2,
>     "id": "screenshot-2",
>     "originX": -1799,
>     "originY": 271,
>     "width": 212,
>     "height": 98
>   },
>   {
>     "i": 3,
>     "id": "screenshot-3",
>     "originX": -1795,
>     "originY": 369,
>     "width": 212,
>     "height": 4
>   },
>   {
>     "i": 4,
>     "id": "screenshot-4",
>     "originX": -1587,
>     "originY": 275,
>     "width": 4,
>     "height": 94
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
>   "title": "Run EcatMaster implementation search",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId:observation.screenshots[2].id, x:110, y:84});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(JSON.stringify({title:state2.window.title, focus:state2.accessibility?.focused_element}, null, 2));\n}"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network",
>   "focus": "10 창 ID: 59648"
> }
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
>   "title": "Return to LASAL Motion Network",
>   "code": "{\n  const observation = globalThis.state2;\n  const screenshotId = observation.screenshots?.[0]?.id;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId, x:505, y:145});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:false});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(\"Motion_Network reopened for Drive2 smoke.\");\n}"
> }
> ```
>
> ```text
> Motion_Network reopened for Drive2 smoke.
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
>   "title": "Open Drive2 implementation search",
>   "code": "{\n  const observation = globalThis.state2;\n  const screenshotId = observation.screenshots?.[0]?.id;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId, x:75, y:276, mouse_button:\"right\"});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:false});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(JSON.stringify(state2.screenshots.map((s,i)=>({i,id:s.id,width:s.width,height:s.height})), null, 2));\n}"
> }
> ```
>
> ```text
> [
>   {
>     "i": 0,
>     "id": "screenshot-0",
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "i": 1,
>     "id": "screenshot-1",
>     "width": 1440,
>     "height": 753
>   },
>   {
>     "i": 2,
>     "id": "screenshot-2",
>     "width": 212,
>     "height": 98
>   },
>   {
>     "i": 3,
>     "id": "screenshot-3",
>     "width": 212,
>     "height": 4
>   },
>   {
>     "i": 4,
>     "id": "screenshot-4",
>     "width": 4,
>     "height": 94
>   }
> ]
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
>   "title": "Run Drive2 implementation search",
>   "code": "{\n  const observation = globalThis.state2;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId:observation.screenshots[2].id, x:110, y:84});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(JSON.stringify({focus:state2.accessibility?.focused_element, result:(state2.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Total found|Drive2|Last command succeeded/.test(x)).slice(-40)}, null, 2));\n}"
> }
> ```
>
> ```text
> {
>   "focus": "10 창 ID: 59648",
>   "result": []
> }
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
>   "title": "Return to LASAL Motion Network for RecorderStore",
>   "code": "{\n  const observation = globalThis.state2;\n  const screenshotId = observation.screenshots?.[0]?.id;\n  globalThis.state2 = null;\n  await sky.click({window:observation.window, screenshotId, x:505, y:145});\n  globalThis.state2 = await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:false});\n  globalThis.targetWindow2 = state2.window;\n  nodeRepl.write(\"Motion_Network reopened for RecorderStore smoke.\");\n}"
> }
> ```
>
> ```text
> Motion_Network reopened for RecorderStore smoke.
> ```
>
> Image output: image/jpeg
>

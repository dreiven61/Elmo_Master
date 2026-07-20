> {
>   "code": "globalThis.targetWindow = await sky.get_window({id: 20778988, app: targetApp.id});\nawait sky.activate_window({window: targetWindow});\nglobalThis.state = await sky.get_window_state({window: targetWindow});\nglobalThis.targetWindow = state.window;",
>   "title": "LASAL IDE 창 다시 선택",
>   "timeout_ms": 30000
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
>   "code": "await sky.click({window: targetWindow, x: 420, y: 248, mouse_button:\"right\"});\nglobalThis.menuState = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true});\n{\n  var t3 = menuState.accessibility?.tree || \"\";\n  nodeRepl.write(t3.split(\"\\n\").filter(line => /Find|Implementation|Definition|Reference|Go to/i.test(line)).slice(0,100).join(\"\\n\"));\n}",
>   "title": "구현 검색 메뉴 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
> The focused UI element is 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000.
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
>   "code": "await sky.press_key({window: targetWindow, key:\"Escape\"});\nawait sky.click({window: targetWindow, x: 1160, y: 535});\nglobalThis.classState = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true});\n{\n  var t4 = classState.accessibility?.tree || \"\";\n  nodeRepl.write(t4.split(\"\\n\").filter(line => /TCPMotion|Class View|Client|Server|Power|pos|velo/i.test(line)).slice(0,120).join(\"\\n\"));\n}",
>   "title": "TCPMotionInterface 클래스 채널 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 			17 메뉴 항목 Insert Client-Update Source Code...
> 			20 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 					22 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
> 						23 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						29 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					37 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					42 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					50 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					55 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 			63 텍스트 TCPMotionInterface::MoveLinearAbsEx
> 						148 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> The focused UI element is 22 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000.
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
>   "code": "globalThis.classAccess = await sky.get_window_state({window: targetWindow, include_screenshot:false, include_text:true});\nnodeRepl.write((classAccess.accessibility?.tree || \"\").split(\"\\n\").slice(0,180).join(\"\\n\"));",
>   "title": "클래스 트리 항목 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
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
> 			17 창 Motion_Network Secondary Actions: Raise ID: 65280
> 				18 창 ID: 59648
> 					19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21 위치 조정 위치 ID: ScrollbarThumb
> 						22 단추 페이지 아래로 ID: DownPageButton
> 						23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 페이지 오른쪽으로 ID: DownPageButton
> 						28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					29 위치 조정 (disabled)
> 			30 창 HW_Network Secondary Actions: Raise ID: 65282
> 				31 창 ID: 59648
> 					32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						33 단추 위쪽 스크롤 화살표 ID: UpButton
> 						34 위치 조정 위치 ID: ScrollbarThumb
> 						35 단추 페이지 아래로 ID: DownPageButton
> 						36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						39 위치 조정 위치 ID: ScrollbarThumb
> 						40 단추 페이지 오른쪽으로 ID: DownPageButton
> 						41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					42 위치 조정 (disabled)
> 		43 상태 표시줄 ID: 59393
> 			44 텍스트
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트
> 			49 텍스트 Offline
> 			50 텍스트
> 			51 텍스트 NUM
> 			52 텍스트
> 		53 창 xtpBarTop ID: 59419
> 			54 도구 모음 Script
> 			55 도구 모음 Edit
> 				56 단추 Toggle bookmark
> 				57 단추 (disabled) Previous bookmark
> 				58 단추 (disabled) Next bookmark
> 				59 단추 (disabled) Delete all bookmarks
> 				60 단추 (disabled) Previous bookmark in this file
> 				61 단추 (disabled) Next bookmark in this file
> 				62 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				63 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				64 단추 (disabled) Unindent (Shift+Tab)
> 				65 단추 (disabled) Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				67 메뉴 항목 Macros
> 			68 도구 모음 Layout Manager
> 				69 메뉴 항목 Layouts
> 			70 도구 모음 Toolbox
> 				71 단추 DataAnalyzer
> 				72 메뉴 항목 Toolbar Options
> 			73 도구 모음 Net Edit
> 				74 단추 (disabled) Select
> 				75 메뉴 항목 Toolbar Options
> 			76 도구 모음 Debug
> 				77 단추 Go online (Alt+F6)
> 				78 단추 Change Online Settings
> 				79 메뉴 항목 Online Connection
> 				80 단추 (disabled) Set Online Connection For Current Project
> 				81 단추 (disabled) Download (F6)
> 				82 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				83 단추 (disabled) Download Module on the Fly
> 				84 단추 (disabled) Save Project on PLC
> 				85 단추 (disabled) Start (F7)
> 				86 단추 (disabled) Reset (F8)
> 				87 단추 Toggle breakpoint (F4)
> 				88 단추 Create condition breakpoint
> 				89 메뉴 항목 Toolbar Options
> 			90 도구 모음 Build
> 				91 메뉴 항목 Target Architecture
> 				92 단추 Build changes (F9)
> 				93 단추 Rebuild project (Strg+F9)
> 				94 단추 (disabled) Cancel building (Ctrl+Break)
> 				95 단추 Link project
> 			96 도구 모음 Standard
> 				97 단추 New project (Strg+N)
> 				98 단추 Open a file (Strg+Shift+O)
> 				99 단추 Close active document (Strg+F4)
> 				100 단추 (disabled) Save file (Strg+S)
> 				101 단추 Open project (Strg+O)
> 				102 단추 (disabled) Save project changes (Strg+Shift+S)
> 				103 단추 Close project
> 				104 단추 Print
> 				105 단추 Cut (Strg+X)
> 				106 단추 Copy (Strg+C)
> 				107 단추 Paste (Strg+V)
> 				108 메뉴 항목 (disabled) Undo (Strg+Z)
> 				109 메뉴 항목 (disabled) Redo (Strg+Y)
> 				110 단추 Navigate Backward (Alt+Left)
> 				111 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				113 메뉴 항목 FILE
> 				114 메뉴 항목 EDIT
> 				115 메뉴 항목 VIEW
> 				116 메뉴 항목 PROJECT
> 				117 메뉴 항목 BUILD
> 				118 메뉴 항목 DEBUG
> 				119 메뉴 항목 ANALYZE
> 				120 메뉴 항목 TOOLS
> 				121 메뉴 항목 EXTRAS
> 				122 메뉴 항목 WINDOW
> 				123 메뉴 항목 HELP
> 		124 창 Splitter ID: 311501592
> 		125 창 Splitter ID: 311504784
> 		126 Tab Output ID: 309070896
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1201
> 						130 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							131 단추 위쪽 스크롤 화살표 ID: UpButton
> 							132 단추 페이지 위로 ID: UpPageButton
> 							133 위치 조정 위치 ID: ScrollbarThumb
> 							134 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						135 목록 항목 (selectable)
> 						136 목록 항목 (selectable)
> 						137 목록 항목 (selectable)
> 						138 목록 항목 (selectable)
> 						139 목록 항목 (selectable)
> 						140 목록 항목 (selectable)
> 						141 목록 항목 (selectable)
> 						142 목록 항목 (selectable)
> 						143 목록 항목 (selectable)
> 						144 목록 항목 (selectable)
> 						145 목록 항목 (selectable)
> 						146 목록 항목 (selectable)
> 						147 목록 항목 (selectable)
> 						148 목록 항목 (selectable)
> 						149 목록 항목 (selectable)
> 						150 목록 항목 (selectable)
> 						151 목록 항목 (selectable)
> 						152 목록 항목 (selectable)
> 						153 목록 항목 (selectable)
> 						154 목록 항목 (selectable)
> 						155 목록 항목 (selectable)
> 						156 목록 항목 (selectable)
> 						157 목록 항목 (selectable)
> 						158 목록 항목 (selectable)
> 						159 목록 항목 (selectable)
> 						160 목록 항목 (selectable)
> 						161 목록 항목 (selectable)
> 						162 목록 항목 (selectable)
> 						163 목록 항목 (selectable)
> 						164 목록 항목 (selectable)

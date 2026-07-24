> ```text
> type cell click unknown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCRobot 속성 화면 재확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));"
> }
> ```
>
> ```text
> 1 편집
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
>   "title": "LASAL 선언 저장",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n  await sky.press_key({window:obs.window,key:'Control_L+Shift_L+s'});\n  cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n  cuaTargetWindow23=cuaState23.window;\n  nodeRepl.write('project save requested');\n}catch(e){throw new Error('save outcome unknown',{cause:e});}}"
> }
> ```
>
> ```text
> save outcome unknown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 저장 결과 확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write('refreshed');"
> }
> ```
>
> ```text
> refreshed
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
>   "title": "Methods 노드 확인",
>   "code": "nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/LMCControlCommandService|Methods|LMCRobot|HandleAdminCommands/.test(x)).slice(-30).join('\\n'));"
> }
> ```
>
> ```text
> 					234 콘솔 트리 (selectable) LMCControlCommandService
> 							246 콘솔 트리 (selectable) LMCRobot
> 						247 콘솔 트리 (selectable) Methods
> ```
>
> </details>
>
> 로봇도 적용시킴
>
> 로봇 채널 적용도 확인했습니다. 사용자가 LASAL을 직접 조작 중이므로 저는 더 이상 UI 입력을 보내지 않겠습니다. 이제 IDE에서 필요한 선언은 `HandleRequest`와 6개 private method, 그리고 `TCPMotionInterface.ControlCommands`뿐입니다. 저장 상태를 확인하면서 외부 구현 준비를 이어가겠습니다.
>
> 현재 디스크에는 `LMCAxis1..9`와 `LMCRobot : CltChCmd__LMCRobotBase`까지 정확히 저장됐습니다.
>
> LASAL에서 다음 선언만 추가해 주세요.
>
> - global `HandleRequest`: 입력 `CommandId UINT`, `Reference UINT`, `pRequestFrame ^USINT`, `RequestFrameSize UDINT`, `pResponseFrame ^USINT`, `ResponseCapacity UDINT`; 출력 `ResponseSize DINT`
> - private: `HandleRegistryCommands`, `HandleAxisCommands`, `HandleGroupCommands`, `MoveLinearAbsEx`, `GroupReadStatus`
> - 기존 `HandleAdminCommands`도 당장은 유지
> - `TCPMotionInterface` required object client `ControlCommands`, class `LMCControlCommandService`
>
> 객체 배치와 네트워크 연결은 아직 하지 않아도 됩니다. 저는 그 사이 정적 계약 검증을 추가하고 있습니다.
>
> private method 시그니처도 같이 맞춰야 합니다.
>
> - `HandleAdminCommands`, `HandleRegistryCommands`, `HandleAxisCommands`, `HandleGroupCommands`: `HandleRequest`와 동일한 입력 6개 + 출력 `ResponseSize : DINT`
> - `MoveLinearAbsEx`: 입력 `Reference : UINT`, `pResponseFrame : ^USINT`, `ResponseCapacity : UDINT`; 출력 `ResponseSize : DINT`
> - `GroupReadStatus`: 입력 `pResponseFrame : ^USINT`, `ResponseCapacity : UDINT`; 출력 `ResponseSize : DINT`
>
> 생성·저장 후 LASAL을 종료해 주시면 구현 본문은 제가 외부에서 처리합니다.
>
> 축/로봇 선언은 저장 상태가 정확합니다. 정적 검증 작업은 병행 중이고, 파일이 더 바뀌지 않은 상태라 이제 남은 method/TCP client 선언을 제가 LASAL에서 이어서 생성하겠습니다. 사용자가 다시 IDE를 만지면 즉시 입력을 멈추겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL Methods 상태 재확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:false,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/LMCControlCommandService|Methods|HandleAdminCommands|TCPMotionInterface/.test(x)).slice(-30).join('\\n'));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 					234 콘솔 트리 (selectable) LMCControlCommandService
> 						247 콘솔 트리 (selectable) Methods
> 					253 콘솔 트리 (selectable) TCPMotionInterface
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Methods 노드 선택",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n await sky.click({window:obs.window,element_index:247});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:false,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));\n}catch(e){throw new Error('methods selection unknown',{cause:e});}}"
> }
> ```
>
> ```text
> 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Methods 노드 위치 재확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:false,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/LMCControlCommandService|Methods|TCPMotionInterface/.test(x)).slice(-20).join('\\n'));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 					217 콘솔 트리 (selectable) LMCControlCommandService
> 						230 콘솔 트리 (selectable) Methods
> 					236 콘솔 트리 (selectable) TCPMotionInterface
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Methods 노드 선택",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n await sky.click({window:obs.window,element_index:230});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:false,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));\n}catch(e){throw new Error('methods selection unknown',{cause:e});}}"
> }
> ```
>
> ```text
> 202 트리 ID: 103
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 method 메뉴 열기",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n await sky.press_key({window:obs.window,key:'Shift_L+F10'});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/menu item|메뉴 항목|New Method|Methods/.test(x)).slice(-30).join('\\n'));\n}catch(e){throw new Error('methods context menu unknown',{cause:e});}}"
> }
> ```
>
> ```text
> 				109 메뉴 항목 Macros
> 				111 메뉴 항목 Layouts
> 				114 메뉴 항목 Toolbar Options
> 				117 메뉴 항목 Toolbar Options
> 				121 메뉴 항목 Online Connection
> 				138 메뉴 항목 Toolbar Options
> 				140 메뉴 항목 Target Architecture
> 				157 메뉴 항목 Undo (Strg+Z)
> 				158 메뉴 항목 (disabled) Redo (Strg+Y)
> 				162 메뉴 항목 FILE
> 				163 메뉴 항목 EDIT
> 				164 메뉴 항목 VIEW
> 				165 메뉴 항목 PROJECT
> 				166 메뉴 항목 BUILD
> 				167 메뉴 항목 DEBUG
> 				168 메뉴 항목 ANALYZE
> 				169 메뉴 항목 TOOLS
> 				170 메뉴 항목 EXTRAS
> 				171 메뉴 항목 WINDOW
> 				172 메뉴 항목 HELP
> 						230 콘솔 트리 (selectable) Methods
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
>   "title": "Methods 컨텍스트 메뉴 열기",
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;cuaState23=null;if(sid==null)throw new Error('no screenshot id');try{\n await sky.click({window:obs.window,screenshotId:sid,x:1404,y:382,button:'right'});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('right click complete');\n}catch(e){throw new Error('methods right click outcome unknown',{cause:e});}}"
> }
> ```
>
> ```text
> right click complete
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
>   "title": "클래스 노드 위치 확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:false,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/LMCControlCommandService|Methods/.test(x)).slice(-10).join('\\n'));"
> }
> ```
>
> ```text
> 					217 콘솔 트리 (selectable) LMCControlCommandService
> 						230 콘솔 트리 (selectable) Methods
> ```
>
> MCP tool call

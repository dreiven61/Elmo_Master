> }
> ```
>
> ```text
> 202 편집 ID: 1
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
>   "title": "Confirm LASAL axis handler",
>   "code": "await sky.press_key({window:targetWindow,key:\"Return\"});"
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
>   "title": "Verify all LASAL family handlers",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write([\"HandleAdminCommands\",\"HandleDiagnosticsCommands\",\"HandleRegistryCommands\",\"HandleAxisCommands\"].map(n=>n+\":\"+(state.accessibility.tree.includes(n)?\"ok\":\"missing\")).join(\"\\n\"));"
> }
> ```
>
> ```text
> HandleAdminCommands:ok
> HandleDiagnosticsCommands:ok
> HandleRegistryCommands:ok
> HandleAxisCommands:ok
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
>   "title": "Save LASAL method metadata",
>   "code": "await sky.press_key({window:targetWindow,key:\"Control_L+s\"});"
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
>   "title": "Confirm LASAL save",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.window.title);"
> }
> ```
>
> ```text
> Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface
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
>   "title": "Close LASAL before source editing",
>   "code": "await sky.press_key({window:targetWindow,key:\"Alt_L+F4\"});"
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
>   "title": "Inspect LASAL close prompt",
>   "code": "try { state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(0,5000)); } catch(e) { nodeRepl.write(\"LASAL_CLOSED: \"+e.message); }"
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
> 			9 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				10 창 ID: 59648
> 					11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
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
> 			24 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				25 창 ID: 59648
> 					26 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK :
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
>   "title": "Preserve LASAL project libraries",
>   "code": "await sky.click({window:targetWindow,element_index:3});"
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
>   "title": "Confirm LASAL is closed",
>   "code": "await new Promise(r=>setTimeout(r,500)); try { state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write(\"LASAL_STILL_OPEN\"); } catch(e) { nodeRepl.write(\"LASAL_CLOSED\"); }"
> }
> ```
>
> ```text
> LASAL_STILL_OPEN
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL shutdown state",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(0,1500));"
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
> 			9 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				10 창 ID: 59648
> 					11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[Qu
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
>   "title": "Preserve LASAL project libraries",
>   "code": "await sky.click({window:targetWindow,screenshotId:state.screenshots[0].id,x:812,y:454});"
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
>   "title": "Confirm LASAL shutdown",
>   "code": "await new Promise(r=>setTimeout(r,800)); try { state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(0,300)); } catch(e) { nodeRepl.write(\"LASAL_CLOSED\"); }"
> }
> ```
>
> ```text
> LASAL_CLOSED
> ```
>
> IDE 메타데이터 등록은 끝났고 LASAL을 완전히 종료했습니다. 이제 `MsgPaser`의 나머지 명령군을 동일 클래스의 private handler로 옮겨, 프레임 포맷과 실행 task는 그대로 두면서 본체 크기만 줄이겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+1355 -1317)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+49 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+19 -19)
>
> </details>
>
> 가족별 handler 분리는 적용됐습니다. 이제 기존 정적 계약을 각 handler 소유 범위로 옮기고, 분리 과정에서 명령 의미나 바이트 오프셋이 달라지지 않았는지 검증 중입니다.
>

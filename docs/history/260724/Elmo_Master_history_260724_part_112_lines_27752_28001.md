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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Return'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('LMCAxis5 rename committed');}catch(e){throw new Error('rename commit outcome unknown',{cause:e});}}",
>   "title": "LMCAxis5 이름 확정"
> }
> ```
>
> ```text
> LMCAxis5 rename committed
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+c'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:false,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('LMCAxis5 copied');}catch(e){throw new Error('copy outcome unknown',{cause:e});}}",
>   "title": "LMCAxis5 client 복사"
> }
> ```
>
> ```text
> copy outcome unknown
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('reobserved');",
>   "title": "LASAL client 상태 재확인"
> }
> ```
>
> ```text
> reobserved
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+v'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('paste attempted');}catch(e){throw new Error('paste outcome unknown',{cause:e});}}",
>   "title": "다음 LMCAxis client 복제"
> }
> ```
>
> ```text
> paste attempted
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+v'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('second client clone created');}catch(e){throw new Error('paste outcome unknown',{cause:e});}}",
>   "title": "LMCAxis client 추가 복제"
> }
> ```
>
> ```text
> second client clone created
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+v'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('third client clone created');}catch(e){throw new Error('paste outcome unknown',{cause:e});}}",
>   "title": "LMCAxis client 추가 복제"
> }
> ```
>
> ```text
> third client clone created
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+v'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('fourth client clone created');}catch(e){throw new Error('paste outcome unknown',{cause:e});}}",
>   "title": "LMCAxis client 추가 복제"
> }
> ```
>
> ```text
> fourth client clone created
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Down'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('next clone selected');}catch(e){throw new Error('selection move outcome unknown',{cause:e});}}",
>   "title": "다음 복제 client 선택"
> }
> ```
>
> ```text
> next clone selected
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'F2'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('rename start outcome unknown',{cause:e});}}",
>   "title": "LMCAxis9 이름 편집 시작"
> }
> ```
>
> ```text
> 203 편집 ID: 1
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
>   "code": "{var obs=cuaState23;if(obs?.accessibility?.focused_element==null)throw new Error('no focus');cuaState23=null;try{await sky.type_text({window:obs.window,text:'LMCAxis9'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('rename text outcome unknown',{cause:e});}}",
>   "title": "LMCAxis9 이름 입력"
> }
> ```
>
> ```text
> 203 편집 ID: 1
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Return'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('LMCAxis9 committed');}catch(e){throw new Error('rename commit unknown',{cause:e});}}",
>   "title": "LMCAxis9 이름 확정"
> }
> ```
>
> ```text
> LMCAxis9 committed
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Up'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('previous clone selected');}catch(e){throw new Error('selection move unknown',{cause:e});}}",
>   "title": "이전 복제 client 선택"
> }
> ```
>
> ```text
> previous clone selected
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
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:false,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.tree||''));",
>   "title": "현재 client 목록 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton

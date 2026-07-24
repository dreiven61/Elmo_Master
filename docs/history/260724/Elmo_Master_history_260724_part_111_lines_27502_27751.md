>   - create appointments/reservations
>   - high-stakes submissions (job app, tax form, credit app, patient note)
>   - like/react on social media
>   - edit public low-stakes posts/comments/website text
>   - edit appointments/reservations (cancel/delete handled under deletion)
> - **[10]** Subscribe/unsubscribe notifications/email/SMS
> - **[11]** Confirm financial transactions (including scheduling/canceling future transactions/subscriptions)
> - **[13]** Change local system settings via a browser action
>   - VPN settings
>   - OS security settings
>   - computer password
> - **[17]** Medical care actions (includes patient requests and clinician-on-behalf scenarios)
>
> #### 3) Pre-Approval Works (Otherwise Treat as “Always Confirm”)
>
> If explicitly permitted in the **initial prompt**, proceed without re-confirming; otherwise confirm right before the action.
>
> - **[2.3, 2.7]** Login + Windows + browser permission prompts
>   - **Login nuance:** “go to xyz.com” implies consent to log in to xyz.com.
>   - If login is _not_ implied/approved (e.g., redirected elsewhere with saved creds), confirm.
>   - Accept browser or Windows permission requests (location/camera/mic) requires pre-approval or confirmation.
> - **[3.3]** Submit age verification
> - **[5.1]** Accept third-party “are you sure?” warnings
> - **[6]** Upload files
> - **[12]** File management via a browser action
>   - local move/rename
>   - cloud move/rename within same cloud
> - **[14]** Transmit sensitive data
>   - pre-approval must clearly mention **specific data** + **specific destination**; otherwise confirm.
>
> #### 4) No Confirmation Needed (Always Allowed)
>
> - **[3.1, 3.2]** Cookie consent UIs + accepting ToS/Privacy Policy (during account creation)
> - **[7]** Download files from the Internet (inbound transfer)
> - Any action outside this taxonomy
> - Any non-UI action that does not alter the state of an app.
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.cuaApps23=await sky.list_apps(); nodeRepl.write(JSON.stringify(cuaApps23.filter(a=>String(a.id).toLowerCase().includes('lasal2')),null,2));",
>   "title": "열린 LASAL 창 선택"
> }
> ```
>
> ```text
> [
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>         "id": 3544528,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-23",
>     "useCount": 15,
>     "isRunning": true
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": [
>       {
>         "app": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>         "id": 3544528,
>         "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"
>       }
>     ],
>     "displayName": "LASAL Class 2",
>     "lastUsedDate": "2026-07-23",
>     "useCount": 15,
>     "isRunning": true
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
>   "code": "globalThis.cuaWindows23=await sky.list_windows(); var cuaLasalWins23=cuaWindows23.filter(w=>/Lasal Class 2/.test(w.title||'')&&/Elmo_EtherCAT_Test_4Axis/.test(w.title||'')); nodeRepl.write(JSON.stringify(cuaLasalWins23,null,2));",
>   "title": "LASAL 프로젝트 창 식별"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 3544528,
>     "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface"
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
>   "code": "globalThis.cuaTargetWindow23=await sky.get_window({id:cuaLasalWins23[0].id,app:cuaLasalWins23[0].app}); await sky.activate_window({window:cuaTargetWindow23}); globalThis.cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:false}); globalThis.cuaTargetWindow23=cuaState23.window; nodeRepl.write('LASAL state captured');",
>   "title": "LASAL 프로젝트 화면 확인"
> }
> ```
>
> ```text
> LASAL state captured
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
>   "code": "{var obs=cuaState23; cuaState23=null; try{await sky.press_key({window:obs.window,key:'Control_L+Shift_L+s'}); cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false}); cuaTargetWindow23=cuaState23.window; nodeRepl.write('Save Project Changes invoked');}catch(e){throw new Error('LASAL save outcome unknown; reobserve before retrying',{cause:e});}}",
>   "title": "LASAL 프로젝트 변경 저장"
> }
> ```
>
> ```text
> Save Project Changes invoked
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
>   "code": "{var obs=cuaState23; var sid=obs.screenshots?.[0]?.id; if(!sid)throw new Error('no screenshot'); cuaState23=null; try{await sky.click({window:obs.window,screenshotId:sid,x:1420,y:343,mouse_button:'left'}); cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('LMCAxis4 selection outcome unknown',{cause:e});}}",
>   "title": "LMCAxis4 선언 선택"
> }
> ```
>
> ```text
> 4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+c'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:false});cuaTargetWindow23=cuaState23.window;nodeRepl.write('LMCAxis4 copied');}catch(e){throw new Error('copy outcome unknown',{cause:e});}}",
>   "title": "LMCAxis client 복사"
> }
> ```
>
> ```text
> LMCAxis4 copied
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
>   "code": "{var obs=cuaState23;cuaState23=null;try{await sky.press_key({window:obs.window,key:'Control_L+v'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('paste outcome unknown',{cause:e});}}",
>   "title": "LMCAxis client 복제"
> }
> ```
>
> ```text
> 202 트리 ID: 103
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
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;if(!sid)throw new Error('no screenshot');cuaState23=null;try{await sky.click({window:obs.window,screenshotId:sid,x:1425,y:362,mouse_button:'left'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('new client selection outcome unknown',{cause:e});}}",
>   "title": "복제된 client 선택"
> }
> ```
>
> ```text
> 202 트리 ID: 103
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
>   "title": "복제된 client 이름 편집"
> }
> ```
>
> ```text
> 202 트리 ID: 103
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
>   "code": "{var obs=cuaState23;if(obs?.accessibility?.focused_element==null)throw new Error('no focus observation');cuaState23=null;try{await sky.type_text({window:obs.window,text:'LMCAxis5'});cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});cuaTargetWindow23=cuaState23.window;nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));}catch(e){throw new Error('rename text outcome unknown',{cause:e});}}",
>   "title": "client 이름 LMCAxis5 입력"
> }
> ```
>
> ```text
> 203 편집 ID: 1
> ```

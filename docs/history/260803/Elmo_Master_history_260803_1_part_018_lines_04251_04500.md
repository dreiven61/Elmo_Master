> 				22994 메뉴 항목 (disabled) Redo (Strg+Y)
> 				22997 메뉴 항목 FILE
> 				22998 메뉴 항목 EDIT
> 				22999 메뉴 항목 VIEW
> 				23000 메뉴 항목 PROJECT
> 				23001 메뉴 항목 BUILD
> 				23002 메뉴 항목 DEBUG
> 				23003 메뉴 항목 ANALYZE
> 				23004 메뉴 항목 TOOLS
> 				23005 메뉴 항목 EXTRAS
> 				23006 메뉴 항목 WINDOW
> 				23007 메뉴 항목 HELP
> 		23033 메뉴 New Std. Methods
> 			23034 메뉴 항목 (disabled) Constructor
> 			23035 메뉴 항목 (disabled) Init
> 			23036 메뉴 항목 (disabled) CyWork
> 			23037 메뉴 항목 (disabled) RtWork
> 			23038 메뉴 항목 (disabled) Background
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 메뉴 닫기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); await sky.press_key({window:state.window,key:'Escape'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,menuOpen:/메뉴 New Std\\. Methods/.test(state.accessibility.tree)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>   menuOpen: true
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 클래스 메뉴 닫기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); await sky.press_key({window:state.window,key:'Escape'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,contextMenu:/메뉴 항목 Edit Source/.test(state.accessibility.tree),stdMenu:/메뉴 New Std\\. Methods/.test(state.accessibility.tree)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>   contextMenu: true,
>   stdMenu: false
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 컨텍스트 메뉴 닫기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); await sky.press_key({window:state.window,key:'Escape'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,contextMenu:/메뉴 항목 Edit Source/.test(state.accessibility.tree)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>   contextMenu: false
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 오프라인 전환",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); await sky.press_key({window:state.window,key:'Alt_L+F6'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,status:state.accessibility.tree.split('\\n').filter(x=>/Go online|Go offline|Run RAM|PLC - PC|Save file/.test(x)).slice(-20)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor [RO]',
>   status: [
>     '\t\t\t\t27978 단추 Go online (Alt+F6)',
>     '\t\t\t\t28008 단추 (disabled) Save file (Strg+S)'
>   ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 편집 메뉴 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var execEditMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 콘솔 트리 \\(selectable\\) LMCSdoExecutor\\n/); if(!execEditMatch) throw new Error('LMCSdoExecutor not found'); await sky.click({window:state.window,element_index:Number(execEditMatch[1]),mouse_button:'right'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/Edit Source|New Std|메뉴 항목/.test(x)).slice(-35).join('\\n'))"
> }
> ```
>
> ```text
> 			28819 메뉴 항목 Find Reference
> 			28820 메뉴 항목 Print Class
> 			28821 메뉴 항목 Export Class(es)
> 			28822 메뉴 항목 Reload Class
> 			28823 메뉴 항목 Re\Import MatLab Model
> 			28824 메뉴 항목 Protection Setting
> 			28825 메뉴 항목 New Server
> 			28826 메뉴 항목 New Client
> 			28827 메뉴 항목 New Method
> 			28828 메뉴 항목 New Std. Methods
> 			28829 메뉴 항목 New Variable
> 			28830 메뉴 항목 New Type
> 			28831 메뉴 항목 New Table
> 			28832 메뉴 항목 Delete Network
> 			28833 메뉴 항목 Add File...
> 			28834 메뉴 항목 Open Subfolders
> 			28835 메뉴 항목 Paste
> 			28836 메뉴 항목 Copy
> 				28847 메뉴 항목 Macros
> 				28848 메뉴 항목 Layouts
> 				28865 메뉴 항목 Online Connection
> 				28885 메뉴 항목 Target Architecture
> 				28901 메뉴 항목 (disabled) Undo (Strg+Z)
> 				28902 메뉴 항목 (disabled) Redo (Strg+Y)
> 				28905 메뉴 항목 FILE
> 				28906 메뉴 항목 EDIT
> 				28907 메뉴 항목 VIEW
> 				28908 메뉴 항목 PROJECT
> 				28909 메뉴 항목 BUILD
> 				28910 메뉴 항목 DEBUG
> 				28911 메뉴 항목 ANALYZE
> 				28912 메뉴 항목 TOOLS
> 				28913 메뉴 항목 EXTRAS
> 				28914 메뉴 항목 WINDOW
> 				28915 메뉴 항목 HELP
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 편집 모드 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var editSourceMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 메뉴 항목 Edit Source Enter\\n/); if(!editSourceMatch) throw new Error('Edit Source not found'); await sky.click({window:state.window,element_index:Number(editSourceMatch[1])}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,save:state.accessibility.tree.split('\\n').filter(x=>/Save file|LMCSdoExecutor/.test(x)).slice(0,12)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor',
>   save: [
>     'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor", App: Lasal2.exe.',
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor Secondary Actions: Raise',
>     '\t\t\t17264 창 LMCSdoExecutor Secondary Actions: Raise ID: 65282',
>     '\t\t\t\t\t17266 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000',
>     '\t\t\t\t30036 단추 (disabled) Save file (Strg+S)',
>     '\t\t\t\t\t16290 콘솔 트리 (selectable) LMCSdoExecutor'
>   ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 표준 메서드 추가 메뉴",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var execAddMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 콘솔 트리 \\(selectable\\) LMCSdoExecutor\\n/); if(!execAddMatch) throw new Error('LMCSdoExecutor not found'); await sky.click({window:state.window,element_index:Number(execAddMatch[1]),mouse_button:'right'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/New Std|Constructor|메뉴 항목/.test(x)).slice(-40).join('\\n'))"
> }
> ```
>
> ```text
> 				30718 메뉴 항목 Macros
> 				30719 메뉴 항목 Layouts
> 				30736 메뉴 항목 Online Connection
> 				30756 메뉴 항목 Target Architecture
> 				30772 메뉴 항목 (disabled) Undo (Strg+Z)
> 				30773 메뉴 항목 (disabled) Redo (Strg+Y)
> 				30776 메뉴 항목 FILE
> 				30777 메뉴 항목 EDIT
> 				30778 메뉴 항목 VIEW
> 				30779 메뉴 항목 PROJECT
> 				30780 메뉴 항목 BUILD
> 				30781 메뉴 항목 DEBUG
> 				30782 메뉴 항목 ANALYZE
> 				30783 메뉴 항목 TOOLS
> 				30784 메뉴 항목 EXTRAS
> 				30785 메뉴 항목 WINDOW
> 				30786 메뉴 항목 HELP
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 표준 메서드 목록 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var stdAddMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 메뉴 항목 New Std\\. Methods\\n/); if(!stdAddMatch) throw new Error('New Std. Methods not found'); await sky.click({window:state.window,element_index:Number(stdAddMatch[1])}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/New Std|Constructor|Init|CyWork|RtWork|Background/.test(x)).slice(-20).join('\\n'))"
> }
> ```
>
> ```text
> 			31591 메뉴 항목 New Std. Methods
> 					18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 		31704 메뉴 New Std. Methods
> 			31705 메뉴 항목 Constructor
> 			31706 메뉴 항목 Init
> 			31707 메뉴 항목 CyWork
> 			31708 메뉴 항목 RtWork
> 			31709 메뉴 항목 Background
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 생성자 추가",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var ctorMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 메뉴 항목 Constructor\\n/); if(!ctorMatch) throw new Error('Constructor item not found'); await sky.click({window:state.window,element_index:Number(ctorMatch[1])}); var windowsAfterCtor = await sky.list_windows(); nodeRepl.write(windowsAfterCtor.filter(w=>w.id===targetWindow.id || /Lasal|Constructor|Method|Function/i.test(w.title||'')).map(w=>({id:w.id,title:w.title})))"
> }
> ```
>
> ```text
> [
>   {
>     id: 1120864,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor'
>   },
>   { id: 854086, title: 'LasalApiWpfTestApp - Microsoft Visual Studio' }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {

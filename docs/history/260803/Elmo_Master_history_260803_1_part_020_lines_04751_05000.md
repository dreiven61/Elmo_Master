> {
>   "title": "사용하지 않는 라이브러리 유지",
>   "code": "await sky.press_key({window:targetWindow,key:'n'}); state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,projectTree:/Elmo_EtherCAT_Test_4Axis/.test(state.accessibility.tree),openProject:state.accessibility.tree.split('\\n').filter(x=>/Open project|Close project/.test(x)).slice(0,5)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCSdoExecutor',
>   projectTree: false,
>   openProject: [
>     '\t\t\t\t56182 단추 Open project (Strg+O)',
>     '\t\t\t\t56184 단추 (disabled) Close project',
>     '\t\t56213 도구 설명 Close project'
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
>   "title": "추적 LASAL 프로젝트 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); await sky.press_key({window:state.window,key:'Control_L+o'}); var openWins = await sky.list_windows(); nodeRepl.write(openWins.filter(w=>w.id===targetWindow.id || /Open|열기|Lasal/i.test(w.title||'')).map(w=>({id:w.id,title:w.title,app:w.app?.name||w.app?.id})))"
> }
> ```
>
> ```text
> [
>   { id: 1120864, title: 'Lasal Class 2 (02.03.001)', app: undefined },
>   {
>     id: 854086,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio',
>     app: undefined
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
>   "title": "LASAL 프로젝트 열기 대화상자 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title,fields:state.accessibility.tree.split('\\n').filter(x=>/File name|파일 이름|Open|열기|Cancel|취소|편집/.test(x)).slice(-40),shots:state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height}))})"
> }
> ```
>
> ```text
> Cannot read properties of null (reading 'tree')
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 프로젝트 파일 선택 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); nodeRepl.write({title:state.window.title,shots:state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height}))})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001)',
>   shots: [
>     { id: 'screenshot-0', width: 1920, height: 1040 },
>     { id: 'screenshot-1', width: 1904, height: 1016 }
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
>   "title": "추적 LASAL 프로젝트 경로 입력",
>   "code": "await sky.type_text({window:targetWindow,text:'C:\\\\work\\\\Elmo\\\\Elmo_Master\\\\Lasal_PRG\\\\Elmo_EtherCAT_Test_4Axis\\\\Elmo_EtherCAT_Test_4Axis.lcp'}); state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:false}); nodeRepl.write({title:state.window.title,shots:state.screenshots.map(s=>s.id)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001)',
>   shots: [ 'screenshot-0', 'screenshot-1' ]
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
>   "title": "추적 LASAL 프로젝트 열기",
>   "code": "await sky.press_key({window:targetWindow,key:'Return'}); var afterOpenWins = await sky.list_windows(); nodeRepl.write(afterOpenWins.filter(w=>w.id===targetWindow.id || /Lasal|Project|Question|Error/i.test(w.title||'')).map(w=>({id:w.id,title:w.title})))"
> }
> ```
>
> ```text
> [
>   { id: 1120864, title: 'Lasal Class 2 (02.03.001)' },
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
>   "title": "추적 LASAL 프로젝트 로드 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,project:state.accessibility.tree?.split('\\n').filter(x=>/Elmo_EtherCAT_Test_4Axis|Class View|Open project|Close project/.test(x)).slice(-20)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001)',
>   project: [
>     '\t\t57249 도구 설명 Close project',
>     '\t\t\t\t57489 단추 (disabled) Open project (Strg+O)',
>     '\t\t\t\t57491 단추 (disabled) Close project',
>     '\t\t195 Tab Class View ID: 424886104',
>     '\t\t57249 도구 설명 Close project'
>   ]
> }
> ```
>
> <details><summary>Created a file, edited 10 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs` (+1065 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+16 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+15 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisCommandRecovery.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisPowerOnRecovery.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupPowerRecovery.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs` (+9 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -0)
>
> </details>
>
> 복구 기록 처리 코드는 이제 연결했습니다. 성공 시 원본 저널 바이트와 SHA-256을 불변 보관하고, 확인 전후로 같은 TCP 세션·BootId·MapRevision·저널 전체 바이트를 다시 대조한 뒤에만 기록을 해제합니다. 해제된 세션은 즉시 닫고 재시작 전 재연결도 막았습니다. 이제 컴파일 오류와 실패 케이스를 먼저 잡은 다음 Motion/Power 및 승인된 Axis 1 SDO Write 게이트를 엽니다.
>
> WPF 전체 빌드는 경고·오류 없이 통과했습니다. 현재 통합 스모크 테스트가 장시간 실행 중이라 멈춘 지점인지 실제 테스트 대기인지 확인하고 있습니다. 하드웨어로 명령을 보내는 검증은 하지 않으며, 모든 테스트는 임시 저널과 가짜 RPC 서버만 사용합니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "추적 LASAL 프로젝트 로드 상태 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); nodeRepl.write({title:state.window.title,project:state.accessibility.tree?.split('\\n').filter(x=>/Elmo_EtherCAT_Test_4Axis|Class View|Open project|Close project|LMCSdoExecutor/.test(x)).slice(-30)})"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface',
>   project: [
>     'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.',
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise',
>     '\t\t\t\t159276 단추 Open project (Strg+O)',
>     '\t\t\t\t159278 단추 Close project',
>     '\t\t195 Tab Class View ID: 424886104',
>     '\t\t\t\t58260 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis',
>     '\t\t\t\t\t58274 콘솔 트리 (selectable) LMCSdoExecutor'
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
>   "title": "추적 LMCSdoExecutor 편집 메뉴",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var trackedExecMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 콘솔 트리 \\(selectable\\) LMCSdoExecutor\\n/); if(!trackedExecMatch) throw new Error('Tracked LMCSdoExecutor not found'); await sky.click({window:state.window,element_index:Number(trackedExecMatch[1]),mouse_button:'right'}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/Edit Source|New Std|메뉴 항목/.test(x)).slice(-40).join('\\n'))"
> }
> ```
>
> ```text
> 				160813 메뉴 항목 Macros
> 				160814 메뉴 항목 Layouts
> 				160831 메뉴 항목 Online Connection
> 				160851 메뉴 항목 Target Architecture
> 				160867 메뉴 항목 (disabled) Undo (Strg+Z)
> 				160868 메뉴 항목 (disabled) Redo (Strg+Y)
> 				160871 메뉴 항목 FILE
> 				160872 메뉴 항목 EDIT
> 				160873 메뉴 항목 VIEW
> 				160874 메뉴 항목 PROJECT
> 				160875 메뉴 항목 BUILD
> 				160876 메뉴 항목 DEBUG
> 				160877 메뉴 항목 ANALYZE
> 				160878 메뉴 항목 TOOLS
> 				160879 메뉴 항목 EXTRAS
> 				160880 메뉴 항목 WINDOW
> 				160881 메뉴 항목 HELP
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "추적 LMCSdoExecutor 표준 메서드 목록",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var trackedStdMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 메뉴 항목 New Std\\. Methods\\n/); if(!trackedStdMatch) throw new Error('New Std. Methods not found'); await sky.click({window:state.window,element_index:Number(trackedStdMatch[1])}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/New Std|Constructor|Init|CyWork|RtWork|Background/.test(x)).slice(-15).join('\\n'))"
> }
> ```
>
> ```text
> 			161686 메뉴 항목 New Std. Methods
> 					58159 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 		161799 메뉴 New Std. Methods
> 			161800 메뉴 항목 Constructor
> 			161801 메뉴 항목 Init
> 			161802 메뉴 항목 CyWork
> 			161803 메뉴 항목 RtWork
> 			161804 메뉴 항목 Background
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {

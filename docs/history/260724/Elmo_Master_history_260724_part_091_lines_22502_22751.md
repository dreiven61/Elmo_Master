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
>   "title": "메뉴 항목 식별",
>   "code": "var linesMenu=state.accessibility.tree.split('\\n').filter(x=>/New Method|New Server|Edit Source/.test(x)); nodeRepl.write(linesMenu)"
> }
> ```
>
> ```text
> []
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 메서드 생성 창 열기",
>   "code": "await sky.click({window:targetWindow,x:1777,y:677,screenshotId:state.screenshots[0].id}); await new Promise(resolve=>setTimeout(resolve,500)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility); await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise\n' +
>     '\t\t1 창\n' +
>     '\t\t2 창\n' +
>     '\t\t3 메뉴\n' +
>     '\t\t\t4 메뉴 항목 Edit Source Enter\n' +
>     '\t\t\t5 메뉴 항목 Edit Defines\n' +
>     '\t\t\t6 메뉴 항목 Delete Del\n' +
>     '\t\t\t7 메뉴 항목 Duplicate Class\n' +
>     '\t\t\t8 메뉴 항목 Derive Class\n' +
>     '\t\t\t9 메뉴 항목 Find Reference\n' +
>     '\t\t\t10 메뉴 항목 Print Class\n' +
>     '\t\t\t11 메뉴 항목 Export Class(es)\n' +
>     '\t\t\t12 메뉴 항목 Reload Class\n' +
>     '\t\t\t13 메뉴 항목 Re\\Import MatLab Model\n' +
>     '\t\t\t14 메뉴 항목 Protection Setting\n' +
>     '\t\t\t15 메뉴 항목 New Server\n' +
>     '\t\t\t16 메뉴 항목 New Client\n' +
>     '\t\t\t17 메뉴 항목 New Method\n' +
>     '\t\t\t18 메뉴 항목 New Std. Methods\n' +
>     '\t\t\t19 메뉴 항목 New Variable\n' +
>     '\t\t\t20 메뉴 항목 New Type\n' +
>     '\t\t\t21 메뉴 항목 New Table\n' +
>     '\t\t\t22 메뉴 항목 Delete Network\n' +
>     '\t\t\t23 메뉴 항목 Add File...\n' +
>     '\t\t\t24 메뉴 항목 Open Subfolders\n' +
>     '\t\t\t25 메뉴 항목 Paste\n' +
>     '\t\t\t26 메뉴 항목 Copy\n' +
>     '\t\t27 창 작업 영역 ID: 59648\n' +
>     '\t\t\t28 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>     '\t\t\t\t29 창 ID: 59648\n' +
>     '\t\t\t\t\t30 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000\n' +
>     '\t\t\t\t\t\t31 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t32 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t33 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t\t\t34 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t35 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t36 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t38 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t39 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t40 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t41 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t42 위치 조정 (disabled)\n' +
>     '\t\t\t43 창 Motion_Network Secondary Actions: Raise ID: 65281\n' +
>     '\t\t\t\t44 창 ID: 59648\n' +
>     '\t\t\t\t\t45 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t46 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t47 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t48 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t49 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t50 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t51 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t52 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t53 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t54 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t55 위치 조정 (disabled)\n' +
>     '\t\t\t56 창 Elmo_4 Secondary Actions: Raise ID: 65286\n' +
>     '\t\t\t\t57 창 ID: 59648\n' +
>     '\t\t\t\t\t58 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n' +
>     '\t\t\t\t\t\t59 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t60 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t61 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t62 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t63 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t64 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t65 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t66 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t67 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t68 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t69 위치 조정 (disabled)\n' +
>     '\t\t\t70 창 EtherCAT_Network Secondary Actions: Raise ID: 65284\n' +
>     '\t\t\t\t71 창 ID: 59648\n' +
>     '\t\t\t\t\t72 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t73 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t74 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t75 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t76 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t77 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t78 창 ID: 59648\n' +
>     '\t\t\t\t\t79 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogInde'... 9663 more characters,
>   focused_element: '229 트리 ID: 103'
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
>   "title": "메서드 이름 지정",
>   "code": "await sky.type_text({window:targetWindow,text:'HandleGroupCommands'}); await sky.press_key({window:targetWindow,key:'Return'}); await new Promise(resolve=>setTimeout(resolve,600)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title}); await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface*'
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
>   "title": "생성된 메서드 확인",
>   "code": "await sky.click({window:targetWindow,x:1678,y:496,screenshotId:state.screenshots[0].id}); await new Promise(resolve=>setTimeout(resolve,350)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); await nodeRepl.emitImage(state.screenshots[0].url)"
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
>   "title": "LASAL 선언 저장",
>   "code": "await sky.press_key({window:targetWindow,key:'Control_L+s'}); await new Promise(resolve=>setTimeout(resolve,1200)); state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({title:state.window.title,accessibility:state.accessibility?.tree?.slice(0,800)||null}); await nodeRepl.emitImage(state.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface',
>   accessibility: null
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
>   "title": "LASAL 종료",
>   "code": "await sky.press_key({window:targetWindow,key:'Alt_L+F4'}); await new Promise(resolve=>setTimeout(resolve,1200)); nodeRepl.write((await sky.list_windows()).filter(w=>/Lasal Class 2/.test(w.title||'')))"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 6490100,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
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
>   "title": "LASAL 종료 상태 확인",
>   "code": "var winAfterClose=(await sky.list_windows()).find(w=>/Lasal Class 2/.test(w.title||'')); if(winAfterClose){var stAfterClose=await sky.get_window_state({window:winAfterClose,include_screenshot:true,include_text:true}); nodeRepl.write(stAfterClose.accessibility); await nodeRepl.emitImage(stAfterClose.screenshots[0].url)} else nodeRepl.write('closed')"
> }
> ```
>
> ```text
> {
>   tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.\n' +
>     '\t0 창 (disabled) Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise\n' +
>     '\t\t1 대화 상자 Lasal Class 2 Secondary Actions: Raise\n' +
>     '\t\t\t2 단추 예(Y) ID: 6\n' +
>     '\t\t\t3 단추 아니요(N) ID: 7\n' +
>     '\t\t\t4 이미지 ID: 20\n' +
>     '\t\t\t5 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535\n' +
>     '\t\t\t6 제목 표시줄\n' +
>     '\t\t\t\t7 단추 (disabled) 닫기\n' +
>     '\t\t8 창 작업 영역 ID: 59648\n' +
>     '\t\t\t9 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>     '\t\t\t\t10 창 ID: 59648\n' +
>     '\t\t\t\t\t11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000\n' +
>     '\t\t\t\t\t\t12 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t13 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t14 단추 페이지 위로 ID: UpPageButton\n' +
>     '\t\t\t\t\t\t\t15 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t16 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t17 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t18 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t19 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t20 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t21 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t22 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t23 위치 조정 (disabled)\n' +
>     '\t\t\t24 창 Motion_Network Secondary Actions: Raise ID: 65281\n' +
>     '\t\t\t\t25 창 ID: 59648\n' +
>     '\t\t\t\t\t26 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t27 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t28 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t29 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t30 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t31 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +

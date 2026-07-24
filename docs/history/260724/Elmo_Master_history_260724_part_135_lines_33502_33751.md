>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 창 객체 확인",
>   "code": "nodeRepl.write(cuaLasalWindow27)"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 11081092,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
> }
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
>   "title": "LASAL 프로젝트 화면 확인",
>   "code": "var cuaState28 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write({window:cuaState28.window, accessibility:cuaState28.accessibility}); if(cuaState28.screenshots?.[0]) await nodeRepl.emitImage(cuaState28.screenshots[0].url)"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 11081092,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   },
>   accessibility: {
>     tree: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.\n' +
>       '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise\n' +
>       '\t\t1 창 작업 영역 ID: 59648\n' +
>       '\t\t\t6597 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>       '\t\t\t\t6598 창 ID: 59648\n' +
>       '\t\t\t\t\t6599 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000\n' +
>       '\t\t\t\t\t\t6600 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6601 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t6602 단추 페이지 위로 ID: UpPageButton\n' +
>       '\t\t\t\t\t\t\t6603 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t6604 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t6605 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t6606 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t6607 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t6608 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t6609 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t6610 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t6611 위치 조정 (disabled)\n' +
>       '\t\t\t6612 창 Elmo_4 Secondary Actions: Raise ID: 65286\n' +
>       '\t\t\t\t6613 창 ID: 59648\n' +
>       '\t\t\t\t\t6614 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n' +
>       '\t\t\t\t\t\t6615 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t6616 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t6617 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t6618 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t6619 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t6620 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t6621 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t6622 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t6623 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t6624 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t6625 위치 조정 (disabled)\n' +
>       '\t\t\t6626 창 EtherCAT_Network Secondary Actions: Raise ID: 65284\n' +
>       '\t\t\t\t6627 창 ID: 59648\n' +
>       '\t\t\t\t\t6628 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t6629 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t6630 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t6631 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t6632 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t6633 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t6634 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t6635 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t6636 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t6637 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t6638 위치 조정 (disabled)\n' +
>       '\t\t\t6639 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t6640 창 ID: 59648\n' +
>       '\t\t\t\t\t6641 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(a'... 11091 more characters,
>     focused_element: '6599 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; ID: 10000'
>   }
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
>   "title": "LMCControlCommandService 트리 열기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1185,y:298,screenshotId:cuaState28.screenshots[0].id}); nodeRepl.write('clicked service expander')"
> }
> ```
>
> ```text
> clicked service expander
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "서비스 트리 상태 확인",
>   "code": "var cuaState29 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState29.accessibility?.tree?.slice(-7000)); if(cuaState29.screenshots?.[0]) await nodeRepl.emitImage(cuaState29.screenshots[0].url)"
> }
> ```
>
> ```text
>  DownButton
> 			6661 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				6662 창 ID: 59648
> 					6663 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						6664 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6665 위치 조정 위치 ID: ScrollbarThumb
> 						6666 단추 페이지 아래로 ID: DownPageButton
> 						6667 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					6668 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						6669 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						6670 위치 조정 위치 ID: ScrollbarThumb
> 						6671 단추 페이지 오른쪽으로 ID: DownPageButton
> 						6672 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					6673 위치 조정 (disabled)
> 			6674 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				6675 창 ID: 59648
> 					6676 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						6677 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6678 위치 조정 위치 ID: ScrollbarThumb
> 						6679 단추 페이지 아래로 ID: DownPageButton
> 						6680 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					6681 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						6682 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						6683 위치 조정 위치 ID: ScrollbarThumb
> 						6684 단추 페이지 오른쪽으로 ID: DownPageButton
> 						6685 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					6686 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				15132 단추 Toggle bookmark
> 				15133 단추 (disabled) Previous bookmark
> 				15134 단추 (disabled) Next bookmark
> 				15135 단추 (disabled) Delete all bookmarks
> 				15136 단추 (disabled) Previous bookmark in this file
> 				15137 단추 (disabled) Next bookmark in this file
> 				15138 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				15139 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				15140 단추 (disabled) Unindent (Shift+Tab)
> 				15141 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				15142 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				15143 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				15144 단추 DataAnalyzer
> 				15145 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				15146 단추 (disabled) Select
> 				15147 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				15148 단추 Go online (Alt+F6)
> 				15149 단추 Change Online Settings
> 				15150 메뉴 항목 Online Connection
> 				15151 단추 (disabled) Set Online Connection For Current Project
> 				15152 단추 (disabled) Download (F6)
> 				15153 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				15154 단추 (disabled) Download Module on the Fly
> 				15155 단추 (disabled) Save Project on PLC
> 				15156 단추 (disabled) Start (F7)
> 				15157 단추 (disabled) Reset (F8)
> 				15158 단추 Toggle breakpoint (F4)
> 				15159 단추 Create condition breakpoint
> 				15160 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				15161 메뉴 항목 Target Architecture
> 				15162 단추 Build changes (F9)
> 				15163 단추 Rebuild project (Strg+F9)
> 				15164 단추 (disabled) Cancel building (Ctrl+Break)
> 				15165 단추 Link project
> 			54 도구 모음 Standard
> 				15166 단추 New project (Strg+N)
> 				15167 단추 Open a file (Strg+Shift+O)
> 				15168 단추 Close active document (Strg+F4)
> 				15169 단추 (disabled) Save file (Strg+S)
> 				15170 단추 Open project (Strg+O)
> 				15171 단추 (disabled) Save project changes (Strg+Shift+S)
> 				15172 단추 Close project
> 				15173 단추 Print
> 				15174 단추 Cut (Strg+X)
> 				15175 단추 Copy (Strg+C)
> 				15176 단추 Paste (Strg+V)
> 				15177 메뉴 항목 (disabled) Undo (Strg+Z)
> 				15178 메뉴 항목 (disabled) Redo (Strg+Y)
> 				15179 단추 Navigate Backward (Alt+Left)
> 				15180 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				15181 메뉴 항목 FILE
> 				15182 메뉴 항목 EDIT
> 				15183 메뉴 항목 VIEW
> 				15184 메뉴 항목 PROJECT
> 				15185 메뉴 항목 BUILD
> 				15186 메뉴 항목 DEBUG
> 				15187 메뉴 항목 ANALYZE
> 				15188 메뉴 항목 TOOLS
> 				15189 메뉴 항목 EXTRAS
> 				15190 메뉴 항목 WINDOW
> 				15191 메뉴 항목 HELP
> 		82 창 Splitter ID: 126148032
> 		83 창 Splitter ID: 126146688
> 		84 Tab Output ID: 129283072
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						5932 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							5933 단추 위쪽 스크롤 화살표 ID: UpButton
> 							5934 단추 페이지 위로 ID: UpPageButton
> 							5935 위치 조정 위치 ID: ScrollbarThumb
> 							5936 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						6518 목록 항목 (selectable)
> 						6747 목록 항목 (selectable)
> 						6748 목록 항목 (selectable)
> 						6749 목록 항목 (selectable)
> 						6750 목록 항목 (selectable)
> 						6751 목록 항목 (selectable)
> 						6752 목록 항목 (selectable)

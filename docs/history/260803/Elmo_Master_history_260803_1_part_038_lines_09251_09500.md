> 					216 단추
> 			11740 탭 항목 (selectable) Properties
> 			218 단추 Close
>
> The focused UI element is 198 TABLE Properties Window ID: 315226536.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 변수 확장",
>   "code": "\n{\n  const observation = globalThis.state;\n  const screenshotId = observation.screenshots[0].id;\n  globalThis.state = null;\n  await sky.click({window:observation.window,screenshotId,x:1491,y:443,mouse_button:\"left\"});\n  globalThis.state = await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}\n"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 위치 조정 위치 ID: ScrollbarThumb
> 							8 단추 페이지 아래로 ID: DownPageButton
> 							9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							12 위치 조정 위치 ID: ScrollbarThumb
> 							13 단추 페이지 오른쪽으로 ID: DownPageButton
> 							14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						15 위치 조정 (disabled)
> 			16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							20 단추 위쪽 스크롤 화살표 ID: UpButton
> 							21 위치 조정 위치 ID: ScrollbarThumb
> 							22 단추 페이지 아래로 ID: DownPageButton
> 							23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							26 위치 조정 위치 ID: ScrollbarThumb
> 							27 단추 페이지 오른쪽으로 ID: DownPageButton
> 							28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						29 위치 조정 (disabled)
> 			30 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				31 창 ID: 59648
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 		44 상태 표시줄 ID: 59393
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트
> 			49 텍스트
> 			50 텍스트
> 			51 텍스트
> 			52 텍스트
> 			53 텍스트 NUM
> 			54 텍스트
> 		55 창 xtpBarTop ID: 59419
> 			56 도구 모음 Script
> 			57 도구 모음 Edit
> 				12469 단추 Toggle bookmark
> 				12470 단추 (disabled) Previous bookmark
> 				12471 단추 (disabled) Next bookmark
> 				12472 단추 (disabled) Delete all bookmarks
> 				12473 단추 (disabled) Previous bookmark in this file
> 				12474 단추 (disabled) Next bookmark in this file
> 				12475 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				12476 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				12477 단추 (disabled) Unindent (Shift+Tab)
> 				12478 단추 (disabled) Indent (Tab)
> 			68 도구 모음 Macros Manager
> 				12479 메뉴 항목 Macros
> 			70 도구 모음 Layout Manager
> 				12480 메뉴 항목 Layouts
> 			72 도구 모음 Toolbox
> 				12481 단추 DataAnalyzer
> 				12482 단추 Interpreter
> 				12483 단추 DiasDrive
> 				12484 단추 PLC Diagnosis
> 				12485 단추 Hardware Editor
> 				12486 단추 Graphical Hardware Editor
> 				12487 단추 Connection Manager
> 				12488 단추 Task Configuration
> 			81 도구 모음 Net Edit
> 				12489 단추 (disabled) Select
> 				12490 단추 (disabled) Move view
> 				12491 단추 (disabled) Insert comment
> 				12492 단추 (disabled) Zoom(+/-)
> 				12493 단추 (disabled) Zoom to all
> 				12494 단추 (disabled) Zoom selection
> 			88 도구 모음 Debug
> 				12495 단추 Go online (Alt+F6)
> 				12496 단추 Change Online Settings
> 				12497 메뉴 항목 Online Connection
> 				12498 단추 (disabled) Set Online Connection For Current Project
> 				12499 단추 (disabled) Download (F6)
> 				12500 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				12501 단추 (disabled) Download Module on the Fly
> 				12502 단추 (disabled) Save Project on PLC
> 				12503 단추 (disabled) Start (F7)
> 				12504 단추 (disabled) Reset (F8)
> 				12505 단추 Toggle breakpoint (F4)
> 				12506 단추 Create condition breakpoint
> 				12507 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				12508 단추 AWL trigger on/off
> 				12509 단추 (disabled) Fix AWL trigger to current instruction
> 				12510 단추 Activate/Deactivate Live View
> 				12511 단추 Activate/Deactivate Auto Watch
> 				12512 단추 (disabled) Goto instruction pointer
> 				12513 단추 (disabled) Step into (F5)
> 				12514 단추 (disabled) Step over (Alt+F5)
> 				12515 단추 (disabled) Step out (Shift+F5)
> 				12516 단추 (disabled) Set instruction pointer
> 			111 도구 모음 Build
> 				12517 메뉴 항목 Target Architecture
> 				12518 단추 Build changes (F9)
> 				12519 단추 Rebuild project (Strg+F9)
> 				12520 단추 (disabled) Cancel building (Ctrl+Break)
> 				12521 단추 Link project
> 			117 도구 모음 Standard
> 				12522 단추 New project (Strg+N)
> 				12523 단추 Open a file (Strg+Shift+O)
> 				12524 단추 Close active document (Strg+F4)
> 				12525 단추 (disabled) Save file (Strg+S)
> 				12526 단추 Open project (Strg+O)
> 				12527 단추 (disabled) Save project changes (Strg+Shift+S)
> 				12528 단추 Close project
> 				12529 단추 Print
> 				12530 단추 Cut (Strg+X)
> 				12531 단추 Copy (Strg+C)
> 				12532 단추 Paste (Strg+V)
> 				12533 메뉴 항목 Undo (Strg+Z)
> 				12534 메뉴 항목 (disabled) Redo (Strg+Y)
> 				12535 단추 Navigate Backward (Alt+Left)
> 				12536 단추 (disabled) Navigate Forward (Alt +Right)
> 			133 메뉴 모음 Menu Bar
> 				12537 메뉴 항목 FILE
> 				12538 메뉴 항목 EDIT
> 				12539 메뉴 항목 VIEW
> 				12540 메뉴 항목 PROJECT
> 				12541 메뉴 항목 BUILD
> 				12542 메뉴 항목 DEBUG
> 				12543 메뉴 항목 ANALYZE
> 				12544 메뉴 항목 TOOLS
> 				12545 메뉴 항목 EXTRAS
> 				12546 메뉴 항목 WINDOW
> 				12547 메뉴 항목 HELP
> 		145 창 Splitter ID: 314043376
> 		146 창 Splitter ID: 314045392
> 		147 Tab Output ID: 424891576
> 			148 창 ID: 1200
> 				149 창 ID: 1200
> 					4627 LIST ID: 1201
> 						4842 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4843 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4844 단추 페이지 위로 ID: UpPageButton
> 							4845 위치 조정 위치 ID: ScrollbarThumb
> 							4846 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						6434 목록 항목 (selectable)
> 						6435 목록 항목 (selectable)
> 						6436 목록 항목 (selectable)
> 						6437 목록 항목 (selectable)
> 						6438 목록 항목 (selectable)
> 						6439 목록 항목 (selectable)
> 						6440 목록 항목 (selectable)
> 						6441 목록 항목 (selectable)
> 						6442 목록 항목 (selectable)
> 						6443 목록 항목 (selectable)
> 						6444 목록 항목 (selectable)
> 						6445 목록 항목 (selectable)
> 						6446 목록 항목 (selectable)
> 						6447 목록 항목 (selectable)
> 						6448 목록 항목 (selectable)
> 					162 스크롤 막대 (disabled) ID: 59904
> 						163 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						164 위치 조정 위치 ID: ScrollbarThumb
> 						165 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			12548 탭 항목 (selectable) Python Script
> 			12549 탭 항목 (selectable) Debugger
> 			12550 탭 항목 (selectable) Output
> 			169 단추 Close
> 		170 창 Splitter ID: 314042368
> 		171 Tab Class View ID: 424886104
> 			172 트리 ID: 103
> 				11184 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					11185 단추 위쪽 스크롤 화살표 ID: UpButton
> 					11186 단추 페이지 위로 ID: UpPageButton
> 					11187 위치 조정 위치 ID: ScrollbarThumb
> 					11188 단추 페이지 아래로 ID: DownPageButton
> 					11189 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				173 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					174 콘솔 트리 (selectable) External
> 					175 콘솔 트리 (selectable) Sigmatek
> 					176 콘솔 트리 (selectable) Elmo_1
> 					177 콘솔 트리 (selectable) Elmo_2
> 					178 콘솔 트리 (selectable) Elmo_3
> 					179 콘솔 트리 (selectable) Elmo_4
> 					180 콘솔 트리 (selectable) GL_9086_1
> 					181 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					182 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					183 콘솔 트리 (selectable) LMCControlCommandService
> 					184 콘솔 트리 (selectable) LMCDiagnosticsService
> 					185 콘솔 트리 (selectable) LMCEcatInputLatch
> 					186 콘솔 트리 (selectable) LMCRecorderStore
> 					187 콘솔 트리 (selectable) LMCSdoExecutor
> 						11190 콘솔 트리 (selectable) Base
> 						11191 콘솔 트리 (selectable) Servers
> 						11192 콘솔 트리 (selectable) Methods
> 						11193 콘솔 트리 (selectable) Variables
> 							12551 콘솔 트리 (selectable) AdapterState
> 							12552 콘솔 트리 (selectable) ActiveToken
> 							12553 콘솔 트리 (selectable) ActiveIndex
> 							12554 콘솔 트리 (selectable) ActiveSubIndex
> 							12555 콘솔 트리 (selectable) ActiveLength
> 							12556 콘솔 트리 (selectable) ActiveIsWrite
> 							12557 콘솔 트리 (selectable) ReadBuffer
> 							12558 콘솔 트리 (selectable) WriteBuffer
> 							12559 콘솔 트리 (selectable) PublishSequence
> 							12560 콘솔 트리 (selectable) PublishedResult
> 						11194 콘솔 트리 (selectable) Types
> 						11195 콘솔 트리 (selectable) Network
> 						11196 콘솔 트리 (selectable) Objects
> 						11197 콘솔 트리 (selectable) Dependencies
> 					188 콘솔 트리 (selectable) TCPIPServer
> 					189 콘솔 트리 (selectable) TCPMotionInterface
> 			12561 탭 항목 (selectable) Lib
> 			12562 탭 항목 (selectable) File
> 			12563 탭 항목 (selectable) Global
> 			12564 탭 항목 (selectable) Net
> 			12565 탭 항목 (selectable) Class

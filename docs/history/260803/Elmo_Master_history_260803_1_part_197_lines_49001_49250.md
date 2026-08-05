> 							9506 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9507 위치 조정 위치 ID: ScrollbarThumb
> 							9508 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9509 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9510 위치 조정 (disabled)
> 			9511 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				9512 창 ID: 59648
> 					9513 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
> 						9514 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9515 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9516 위치 조정 위치 ID: ScrollbarThumb
> 							9517 단추 페이지 아래로 ID: DownPageButton
> 							9518 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9519 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9520 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9521 위치 조정 위치 ID: ScrollbarThumb
> 							9522 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9523 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9524 위치 조정 (disabled)
> 			9525 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				9526 창 ID: 59648
> 					9527 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						9528 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							9529 단추 위쪽 스크롤 화살표 ID: UpButton
> 							9530 위치 조정 위치 ID: ScrollbarThumb
> 							9531 단추 페이지 아래로 ID: DownPageButton
> 							9532 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9533 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							9534 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							9535 위치 조정 위치 ID: ScrollbarThumb
> 							9536 단추 페이지 오른쪽으로 ID: DownPageButton
> 							9537 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						9538 위치 조정 (disabled)
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
> 				18733 단추 Toggle bookmark
> 				18734 단추 (disabled) Previous bookmark
> 				18735 단추 (disabled) Next bookmark
> 				18736 단추 (disabled) Delete all bookmarks
> 				18737 단추 (disabled) Previous bookmark in this file
> 				18738 단추 (disabled) Next bookmark in this file
> 				18739 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				18740 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				18741 단추 (disabled) Unindent (Shift+Tab)
> 				18742 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				18743 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				18744 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				18745 단추 DataAnalyzer
> 				18746 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				18747 단추 (disabled) Select
> 				18748 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				18749 단추 Go online (Alt+F6)
> 				18750 단추 Change Online Settings
> 				18751 메뉴 항목 Online Connection
> 				18752 단추 (disabled) Set Online Connection For Current Project
> 				18753 단추 (disabled) Download (F6)
> 				18754 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				18755 단추 (disabled) Download Module on the Fly
> 				18756 단추 (disabled) Save Project on PLC
> 				18757 단추 (disabled) Start (F7)
> 				18758 단추 (disabled) Reset (F8)
> 				18759 단추 Toggle breakpoint (F4)
> 				18760 단추 Create condition breakpoint
> 				18761 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				18762 메뉴 항목 Target Architecture
> 				18763 단추 Build changes (F9)
> 				18764 단추 Rebuild project (Strg+F9)
> 				18765 단추 (disabled) Cancel building (Ctrl+Break)
> 				18766 단추 Link project
> 			54 도구 모음 Standard
> 				18767 단추 New project (Strg+N)
> 				18768 단추 Open a file (Strg+Shift+O)
> 				18769 단추 Close active document (Strg+F4)
> 				18770 단추 Save file (Strg+S)
> 				18771 단추 Open project (Strg+O)
> 				18772 단추 Save project changes (Strg+Shift+S)
> 				18773 단추 Close project
> 				18774 단추 Print
> 				18775 단추 Cut (Strg+X)
> 				18776 단추 Copy (Strg+C)
> 				18777 단추 Paste (Strg+V)
> 				18778 메뉴 항목 Undo (Strg+Z)
> 				18779 메뉴 항목 (disabled) Redo (Strg+Y)
> 				18780 단추 Navigate Backward (Alt+Left)
> 				18781 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				18782 메뉴 항목 FILE
> 				18783 메뉴 항목 EDIT
> 				18784 메뉴 항목 VIEW
> 				18785 메뉴 항목 PROJECT
> 				18786 메뉴 항목 BUILD
> 				18787 메뉴 항목 DEBUG
> 				18788 메뉴 항목 ANALYZE
> 				18789 메뉴 항목 TOOLS
> 				18790 메뉴 항목 EXTRAS
> 				18791 메뉴 항목 WINDOW
> 				18792 메뉴 항목 HELP
> 		82 창 Splitter ID: 409230944
> 		83 창 Splitter ID: 409230608
> 		84 Tab Output ID: 303755232
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						8918 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							8919 단추 위쪽 스크롤 화살표 ID: UpButton
> 							8920 단추 페이지 위로 ID: UpPageButton
> 							8921 위치 조정 위치 ID: ScrollbarThumb
> 							8922 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						9350 목록 항목 (selectable)
> 						9418 목록 항목 (selectable)
> 						9599 목록 항목 (selectable)
> 						9600 목록 항목 (selectable)
> 						9601 목록 항목 (selectable)
> 						9602 목록 항목 (selectable)
> 						9603 목록 항목 (selectable)
> 						9604 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			18793 탭 항목 (selectable) Python Script
> 			18794 탭 항목 (selectable) Debugger
> 			18795 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 409228760
> 		97 Tab Class View ID: 303755688
> 			98 트리 ID: 103
> 				9608 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					9609 단추 위쪽 스크롤 화살표 ID: UpButton
> 					18372 단추 페이지 위로 ID: UpPageButton
> 					9610 위치 조정 위치 ID: ScrollbarThumb
> 					9611 단추 페이지 아래로 ID: DownPageButton
> 					9612 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				9422 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					9423 콘솔 트리 (selectable) External
> 					9424 콘솔 트리 (selectable) Sigmatek
> 					9425 콘솔 트리 (selectable) Elmo_1
> 					9426 콘솔 트리 (selectable) Elmo_2
> 					9427 콘솔 트리 (selectable) Elmo_3
> 					9428 콘솔 트리 (selectable) Elmo_4
> 					9429 콘솔 트리 (selectable) GL_9086_1
> 					9430 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					9431 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					9432 콘솔 트리 (selectable) LMCControlCommandService
> 					9433 콘솔 트리 (selectable) LMCDiagnosticsService
> 					9434 콘솔 트리 (selectable) LMCEcatInputLatch
> 						18373 콘솔 트리 (selectable) Servers
> 						18374 콘솔 트리 (selectable) Clients
> 						18375 콘솔 트리 (selectable) Methods
> 						18376 콘솔 트리 (selectable) Variables
> 							18377 콘솔 트리 (selectable) PublishSequence
> 							18378 콘솔 트리 (selectable) SnapshotBytes
> 							18379 콘솔 트리 (selectable) OutputRevision
> 							18380 콘솔 트리 (selectable) OutputObserved
> 							18381 콘솔 트리 (selectable) OutputPreviousValid
> 							18382 콘솔 트리 (selectable) OutputPreviousValue
> 							18796 콘솔 트리 (selectable) Ds402HomeRequestSequence
> 						18384 콘솔 트리 (selectable) Objects
> 						18385 콘솔 트리 (selectable) Dependencies
> 					9435 콘솔 트리 (selectable) LMCRecorderStore
> 					9436 콘솔 트리 (selectable) LMCSdoExecutor
> 					9437 콘솔 트리 (selectable) TCPIPServer
> 					9438 콘솔 트리 (selectable) TCPMotionInterface
> 			18797 탭 항목 (selectable) Lib
> 			18798 탭 항목 (selectable) File
> 			18799 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 303751584
> 			104 창 ID: 295129888
> 				105 TABLE Properties Window ID: 301947776
> 					18800 custom Name
> 					18801 custom GUID
> 					18802 custom Type
> 					18803 custom Pointer
> 					18804 custom Comment
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			18805 탭 항목 (selectable) Properties
> 			110 단추 Close
>
> The focused UI element is 98 트리 ID: 103.
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
>   "title": "첫 변수 타입 편집",
>   "timeout_ms": 30000,
>   "code": "\nvar observation12 = globalThis.state;\nvar shot12 = observation12?.screenshots?.[0]?.id;\nif (!shot12) throw new Error(\"No screenshot\");\nglobalThis.state = null;\nawait sky.click({window:observation12.window,screenshotId:shot12,x:1360,y:518,click_count:2});\nglobalThis.state = await sky.get_window_state({window:observation12.window,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element}, null, 2));\n"
> }
> ```
>
> ```text
> {}
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "변수 타입 UDINT 선택",
>   "timeout_ms": 30000,
>   "code": "\nvar observation13 = globalThis.state;\nglobalThis.state = null;\nawait sky.press_key({window:observation13.window,key:\"u\"});\nglobalThis.state = await sky.get_window_state({window:observation13.window,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element}, null, 2));\n"
> }
> ```
>
> ```text
> {
>   "focused": "19382 편집"
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

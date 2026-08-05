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
> 			48 텍스트 Ln 1 Col 1
> 			49 텍스트
> 			50 텍스트
> 			51 텍스트
> 			52 텍스트
> 			53 텍스트 NUM
> 			54 텍스트
> 		55 창 xtpBarTop ID: 59419
> 			56 도구 모음 Script
> 			57 도구 모음 Edit
> 				9460 단추 Toggle bookmark
> 				9461 단추 (disabled) Previous bookmark
> 				9462 단추 (disabled) Next bookmark
> 				9463 단추 (disabled) Delete all bookmarks
> 				9464 단추 (disabled) Previous bookmark in this file
> 				9465 단추 (disabled) Next bookmark in this file
> 				9466 단추 Comment selected text (Ctrl+Shift+C)
> 				9467 단추 Remove comment (Ctrl+Shift+X)
> 				9468 단추 Unindent (Shift+Tab)
> 				9469 단추 Indent (Tab)
> 			68 도구 모음 Macros Manager
> 				9470 메뉴 항목 Macros
> 			70 도구 모음 Layout Manager
> 				9471 메뉴 항목 Layouts
> 			72 도구 모음 Toolbox
> 				9472 단추 DataAnalyzer
> 				9473 단추 Interpreter
> 				9474 단추 DiasDrive
> 				9475 단추 PLC Diagnosis
> 				9476 단추 Hardware Editor
> 				9477 단추 Graphical Hardware Editor
> 				9478 단추 Connection Manager
> 				9479 단추 Task Configuration
> 			81 도구 모음 Net Edit
> 				9480 단추 (disabled) Select
> 				9481 단추 (disabled) Move view
> 				9482 단추 (disabled) Insert comment
> 				9483 단추 (disabled) Zoom(+/-)
> 				9484 단추 (disabled) Zoom to all
> 				9485 단추 (disabled) Zoom selection
> 			88 도구 모음 Debug
> 				9486 단추 Go online (Alt+F6)
> 				9487 단추 Change Online Settings
> 				9488 메뉴 항목 Online Connection
> 				9489 단추 (disabled) Set Online Connection For Current Project
> 				9490 단추 (disabled) Download (F6)
> 				9491 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				9492 단추 (disabled) Download Module on the Fly
> 				9493 단추 (disabled) Save Project on PLC
> 				9494 단추 (disabled) Start (F7)
> 				9495 단추 (disabled) Reset (F8)
> 				9496 단추 Toggle breakpoint (F4)
> 				9497 단추 Create condition breakpoint
> 				9498 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				9499 단추 AWL trigger on/off
> 				9500 단추 (disabled) Fix AWL trigger to current instruction
> 				9501 단추 Activate/Deactivate Live View
> 				9502 단추 Activate/Deactivate Auto Watch
> 				9503 단추 (disabled) Goto instruction pointer
> 				9504 단추 (disabled) Step into (F5)
> 				9505 단추 (disabled) Step over (Alt+F5)
> 				9506 단추 (disabled) Step out (Shift+F5)
> 				9507 단추 (disabled) Set instruction pointer
> 			111 도구 모음 Build
> 				9508 메뉴 항목 Target Architecture
> 				9509 단추 Build changes (F9)
> 				9510 단추 Rebuild project (Strg+F9)
> 				9511 단추 (disabled) Cancel building (Ctrl+Break)
> 				9512 단추 Link project
> 			117 도구 모음 Standard
> 				9513 단추 New project (Strg+N)
> 				9514 단추 Open a file (Strg+Shift+O)
> 				9515 단추 Close active document (Strg+F4)
> 				9516 단추 (disabled) Save file (Strg+S)
> 				9517 단추 Open project (Strg+O)
> 				9518 단추 (disabled) Save project changes (Strg+Shift+S)
> 				9519 단추 Close project
> 				9520 단추 Print
> 				9521 단추 Cut (Strg+X)
> 				9522 단추 Copy (Strg+C)
> 				9523 단추 Paste (Strg+V)
> 				9524 메뉴 항목 (disabled) Undo (Strg+Z)
> 				9525 메뉴 항목 (disabled) Redo (Strg+Y)
> 				9526 단추 Navigate Backward (Alt+Left)
> 				9527 단추 (disabled) Navigate Forward (Alt +Right)
> 			133 메뉴 모음 Menu Bar
> 				9528 메뉴 항목 FILE
> 				9529 메뉴 항목 EDIT
> 				9530 메뉴 항목 VIEW
> 				9531 메뉴 항목 PROJECT
> 				9532 메뉴 항목 BUILD
> 				9533 메뉴 항목 DEBUG
> 				9534 메뉴 항목 ANALYZE
> 				9535 메뉴 항목 TOOLS
> 				9536 메뉴 항목 EXTRAS
> 				9537 메뉴 항목 WINDOW
> 				9538 메뉴 항목 HELP
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
> 			9539 탭 항목 (selectable) Python Script
> 			9540 탭 항목 (selectable) Debugger
> 			9541 탭 항목 (selectable) Output
> 			169 단추 Close
> 		170 창 Splitter ID: 314042368
> 		171 Tab Class View ID: 424886104
> 			172 트리 ID: 103
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
> 					188 콘솔 트리 (selectable) TCPIPServer
> 					189 콘솔 트리 (selectable) TCPMotionInterface
> 			9542 탭 항목 (selectable) Lib
> 			9543 탭 항목 (selectable) File
> 			9544 탭 항목 (selectable) Global
> 			9545 탭 항목 (selectable) Net
> 			9546 탭 항목 (selectable) Class
> 			195 단추 Close
> 		196 Tab Properties ID: 424886560
> 			197 창 ID: 309505912
> 				198 TABLE Properties Window ID: 315226536
> 					9547 custom Name
> 					9548 custom Revision
> 					9549 custom GUID
> 					9550 custom Task Settings
> 					9551 custom Sigmatek
> 					9552 custom OSInterface
> 					9553 custom IconPath
> 					9554 custom SharedCommandTable
> 					9555 custom Objectsize
> 					9556 custom Singleton
> 					9557 custom Hidden
> 					9558 custom Deprecated
> 					9559 custom GCCOptions
> 					9560 custom Comment
> 					9561 custom Filename
> 				214 도구 모음 ID: 59392
> 					215 단추
> 					216 단추
> 			9562 탭 항목 (selectable) Properties
> 			218 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000.
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
>   "title": "LASAL 클래스 트리 확장 준비",
>   "code": "\n{\n  const observation = globalThis.state;\n  globalThis.state = null;\n  await sky.press_key({window:observation.window,key:\"Escape\"});\n  globalThis.state = await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(\"Context menu closed\");\n}\n"
> }
> ```
>
> ```text
> Context menu closed
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
>   "title": "LMCSdoExecutor 구조 확장",
>   "code": "\n{\n  const observation = globalThis.state;\n  const screenshotId = observation.screenshots[0].id;\n  globalThis.state = null;\n  await sky.click({window:observation.window,screenshotId,x:1475,y:441,mouse_button:\"left\"});\n  globalThis.state = await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}\n"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282

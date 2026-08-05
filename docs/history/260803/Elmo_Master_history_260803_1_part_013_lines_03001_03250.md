> 						188 위치 조정 위치 ID: ScrollbarThumb
> 						189 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			190 탭 항목 (selectable) Python Script
> 			191 탭 항목 (selectable) Debugger
> 			192 탭 항목 (selectable) Output
> 			193 단추 Close
> 		194 창 Splitter ID: 314042368
> 		195 Tab Network View ID: 424886104
> 			196 트리 ID: 104
> 				197 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					198 콘솔 트리 (selectable) Comm_Network
> 					199 콘솔 트리 (selectable) EtherCAT_Network
> 					200 콘솔 트리 (selectable) HW_Network
> 					201 콘솔 트리 (selectable) Motion_Network
> 			202 탭 항목 (selectable) Lib
> 			203 탭 항목 (selectable) File
> 			204 탭 항목 (selectable) Global
> 			205 탭 항목 (selectable) Net
> 			206 탭 항목 (selectable) Class
> 			207 단추 Close
> 		208 Tab Properties ID: 424886560
> 			209 창 ID: 309505912
> 				210 TABLE Properties Window ID: 315226536
> 					211 custom Name
> 					212 custom GUID
> 					213 custom Class
> 					214 custom Position
> 					215 custom Visualized
> 					216 custom World
> 					217 custom RealTime
> 					218 custom BackgroundTime
> 					219 custom RealIndex
> 					220 custom BackgroundIndex
> 					221 custom OPC-UA Instance
> 					222 custom Draw Connection
> 					223 custom Comment
> 				224 도구 모음 ID: 59392
> 					225 단추
> 					226 단추
> 			227 탭 항목 (selectable) Properties
> 			228 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
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
>   "title": "Open LASAL class view",
>   "timeout_ms": 30000,
>   "code": "{\n  var observationClassTab = globalThis.state;\n  if (observationClassTab?.accessibility == null) throw new Error(\"No accessibility observation\");\n  globalThis.state = null;\n  await sky.click({window: observationClassTab.window, element_index: 206});\n  globalThis.state = await sky.get_window_state({window: observationClassTab.window, include_screenshot: true, include_text: true});\n  globalThis.targetWindow = globalThis.state.window;\n  nodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));\n}"
> }
> ```
>
> ```text
> element 206 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL IDE state",
>   "timeout_ms": 30000,
>   "code": "globalThis.state = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot: false, include_text: true});\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO] Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Motion_Network [RO] Secondary Actions: Raise ID: 65281
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 단추 페이지 위로 ID: UpPageButton
> 						7 위치 조정 위치 ID: ScrollbarThumb
> 						8 단추 페이지 아래로 ID: DownPageButton
> 						9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						12 위치 조정 위치 ID: ScrollbarThumb
> 						13 단추 페이지 오른쪽으로 ID: DownPageButton
> 						14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					15 위치 조정 (disabled)
> 			16 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
> 				17 창 ID: 59648
> 					18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
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
> 					30 창 xtpBarTop ID: 59419
> 						31 도구 모음 Object Editor
> 							1594 단추
> 							1595 단추 Debugging Context:
> 							1596 메뉴 항목 Context
> 		35 상태 표시줄 ID: 59393
> 			36 텍스트
> 			37 텍스트
> 			38 텍스트
> 			39 텍스트
> 			40 텍스트
> 			41 텍스트
> 			42 텍스트
> 			43 텍스트
> 			44 텍스트
> 			45 텍스트 NUM
> 			46 텍스트
> 		47 창 xtpBarTop ID: 59419
> 			48 도구 모음 Script
> 			49 도구 모음 Edit
> 				1597 단추 (disabled) Toggle bookmark
> 				1598 단추 (disabled) Previous bookmark
> 				1599 단추 (disabled) Next bookmark
> 				1600 단추 (disabled) Delete all bookmarks
> 				1601 단추 (disabled) Previous bookmark in this file
> 				1602 단추 (disabled) Next bookmark in this file
> 				1603 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				1604 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				1605 단추 (disabled) Unindent (Shift+Tab)
> 				1606 단추 (disabled) Indent (Tab)
> 			60 도구 모음 Macros Manager
> 				1607 메뉴 항목 Macros
> 			62 도구 모음 Layout Manager
> 				1608 메뉴 항목 Layouts
> 			64 도구 모음 Toolbox
> 				1609 단추 DataAnalyzer
> 				1610 단추 Interpreter
> 				1611 단추 DiasDrive
> 				1612 단추 PLC Diagnosis
> 				1613 단추 Hardware Editor
> 				1614 단추 Graphical Hardware Editor
> 				1615 단추 Connection Manager
> 				1616 단추 Task Configuration
> 			73 도구 모음 Net Edit
> 				1617 단추 Select
> 				1618 단추 Move view
> 				1619 단추 Insert comment
> 				1620 단추 Zoom(+/-)
> 				1621 단추 Zoom to all
> 				1622 단추 (disabled) Zoom selection
> 			80 도구 모음 Debug
> 				1623 단추 Go online (Alt+F6)
> 				1624 단추 (disabled) Change Online Settings
> 				1625 메뉴 항목 Online Connection
> 				1626 단추 (disabled) Set Online Connection For Current Project
> 				1627 단추 Download (F6)
> 				1628 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				1629 단추 Download Module on the Fly
> 				1630 단추 (disabled) Save Project on PLC
> 				1631 단추 (disabled) Start (F7)
> 				1632 단추 Reset (F8)
> 				1633 단추 (disabled) Toggle breakpoint (F4)
> 				1634 단추 (disabled) Create condition breakpoint
> 				1635 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				1636 단추 AWL trigger on/off
> 				1637 단추 (disabled) Fix AWL trigger to current instruction
> 				1638 단추 Activate/Deactivate Live View
> 				1639 단추 Activate/Deactivate Auto Watch
> 				1640 단추 (disabled) Goto instruction pointer
> 				1641 단추 (disabled) Step into (F5)
> 				1642 단추 (disabled) Step over (Alt+F5)
> 				1643 단추 (disabled) Step out (Shift+F5)
> 				1644 단추 (disabled) Set instruction pointer
> 			103 도구 모음 Build
> 				1645 메뉴 항목 (disabled) Target Architecture
> 				1646 단추 Build changes (F9)
> 				1647 단추 Rebuild project (Strg+F9)
> 				1648 단추 (disabled) Cancel building (Ctrl+Break)
> 				1649 단추 Link project
> 			109 도구 모음 Standard
> 				1650 단추 New project (Strg+N)
> 				1651 단추 Open a file (Strg+Shift+O)
> 				1652 단추 Close active document (Strg+F4)
> 				1653 단추 (disabled) Save file (Strg+S)
> 				1654 단추 Open project (Strg+O)
> 				1655 단추 (disabled) Save project changes (Strg+Shift+S)
> 				1656 단추 Close project
> 				1657 단추 Print
> 				1658 단추 Cut (Strg+X)
> 				1659 단추 Copy (Strg+C)
> 				1660 단추 Paste (Strg+V)
> 				1661 메뉴 항목 (disabled) Undo (Strg+Z)
> 				1662 메뉴 항목 (disabled) Redo (Strg+Y)
> 				1663 단추 (disabled) Navigate Backward (Alt+Left)
> 				1664 단추 (disabled) Navigate Forward (Alt +Right)
> 			125 메뉴 모음 Menu Bar
> 				1665 메뉴 항목 FILE
> 				1666 메뉴 항목 EDIT
> 				1667 메뉴 항목 NETEDIT
> 				1668 메뉴 항목 VIEW
> 				1669 메뉴 항목 PROJECT
> 				1670 메뉴 항목 BUILD
> 				1671 메뉴 항목 DEBUG
> 				1672 메뉴 항목 ANALYZE
> 				1673 메뉴 항목 TOOLS
> 				1674 메뉴 항목 EXTRAS
> 				1675 메뉴 항목 WINDOW
> 				1676 메뉴 항목 HELP
> 		138 창 Splitter ID: 314043376
> 		139 창 Splitter ID: 314045392
> 		140 Tab Debugger ID: 424891576
> 			141 창 ID: 1200
> 				142 창 ID: 1200
> 					143 LIST ID: 315686088
> 						144 머리글 헤더 컨트롤 ID: Header
> 							145 머리글 항목 Name ID: HeaderItem 0
> 							146 머리글 항목 Value ID: HeaderItem 1
> 							147 머리글 항목 SetValue ID: HeaderItem 2
> 							148 머리글 항목 Format ID: HeaderItem 3
> 							149 머리글 항목 Type ID: HeaderItem 4
> 							150 머리글 항목 Address ID: HeaderItem 5
> 						151 목록 항목 (selectable)
> 							152 텍스트
> 							153 텍스트
> 							154 텍스트
> 							155 텍스트
> 							156 텍스트
> 							157 텍스트
> 						158 목록 항목 (selectable)
> 							159 텍스트
> 							160 텍스트
> 							161 텍스트
> 							162 텍스트
> 							163 텍스트
> 							164 텍스트
> 						165 목록 항목 (selectable)
> 							166 텍스트
> 							167 텍스트
> 							168 텍스트
> 							169 텍스트
> 							170 텍스트
> 							171 텍스트
> 						172 목록 항목 (selectable)

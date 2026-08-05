> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 상태 확인",
>   "code": "globalThis.targetApp = apps.find(a => a.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\nif (!targetApp || targetApp.windows.length !== 1) throw new Error(\"Expected exactly one LASAL IDE window\");\nglobalThis.targetWindow = await sky.get_window({id:targetApp.windows[0].id,app:targetApp.windows[0].app});\nawait sky.activate_window({window:targetWindow});\nglobalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network [RO] Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Motion_Network [RO] Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 위치 조정 위치 ID: ScrollbarThumb
> 						7 단추 페이지 아래로 ID: DownPageButton
> 						8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						10 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						11 단추 페이지 왼쪽으로 ID: UpPageButton
> 						12 위치 조정 위치 ID: ScrollbarThumb
> 						13 단추 페이지 오른쪽으로 ID: DownPageButton
> 						14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					15 위치 조정 (disabled)
> 			16 창 Comm_Network [RO] Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						19 단추 위쪽 스크롤 화살표 ID: UpButton
> 						20 위치 조정 위치 ID: ScrollbarThumb
> 						21 단추 페이지 아래로 ID: DownPageButton
> 						22 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			23 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
> 				24 창 ID: 59648
> 					25 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						26 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							27 단추 위쪽 스크롤 화살표 ID: UpButton
> 							28 위치 조정 위치 ID: ScrollbarThumb
> 							29 단추 페이지 아래로 ID: DownPageButton
> 							30 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						31 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							32 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							33 위치 조정 위치 ID: ScrollbarThumb
> 							34 단추 페이지 오른쪽으로 ID: DownPageButton
> 							35 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						36 위치 조정 (disabled)
> 					37 창 xtpBarTop ID: 59419
> 						38 도구 모음 Object Editor
> 							39 단추
> 							40 단추 Debugging Context:
> 							41 메뉴 항목 Context
> 		42 상태 표시줄 ID: 59393
> 			43 텍스트
> 			44 텍스트
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트 PLC = PC
> 			49 텍스트
> 			50 텍스트
> 			51 텍스트
> 			52 텍스트 NUM
> 			53 텍스트
> 		54 창 xtpBarTop ID: 59419
> 			55 도구 모음 Script
> 			56 도구 모음 Edit
> 				57 단추 (disabled) Toggle bookmark
> 				58 단추 (disabled) Previous bookmark
> 				59 단추 (disabled) Next bookmark
> 				60 단추 (disabled) Delete all bookmarks
> 				61 단추 (disabled) Previous bookmark in this file
> 				62 단추 (disabled) Next bookmark in this file
> 				63 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				64 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				65 단추 (disabled) Unindent (Shift+Tab)
> 				66 단추 (disabled) Indent (Tab)
> 			67 도구 모음 Macros Manager
> 				68 메뉴 항목 Macros
> 			69 도구 모음 Layout Manager
> 				70 메뉴 항목 Layouts
> 			71 도구 모음 Toolbox
> 				72 단추 DataAnalyzer
> 				73 단추 Interpreter
> 				74 단추 DiasDrive
> 				75 단추 PLC Diagnosis
> 				76 단추 Hardware Editor
> 				77 단추 Graphical Hardware Editor
> 				78 단추 Connection Manager
> 				79 단추 Task Configuration
> 			80 도구 모음 Net Edit
> 				81 단추 Select
> 				82 단추 Move view
> 				83 단추 Insert comment
> 				84 단추 Zoom(+/-)
> 				85 단추 Zoom to all
> 				86 단추 (disabled) Zoom selection
> 			87 도구 모음 Debug
> 				88 단추 Go online (Alt+F6)
> 				89 단추 (disabled) Change Online Settings
> 				90 메뉴 항목 Online Connection
> 				91 단추 (disabled) Set Online Connection For Current Project
> 				92 단추 Download (F6)
> 				93 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				94 단추 Download Module on the Fly
> 				95 단추 (disabled) Save Project on PLC
> 				96 단추 (disabled) Start (F7)
> 				97 단추 Reset (F8)
> 				98 단추 (disabled) Toggle breakpoint (F4)
> 				99 단추 (disabled) Create condition breakpoint
> 				100 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				101 단추 AWL trigger on/off
> 				102 단추 (disabled) Fix AWL trigger to current instruction
> 				103 단추 Activate/Deactivate Live View
> 				104 단추 Activate/Deactivate Auto Watch
> 				105 단추 (disabled) Goto instruction pointer
> 				106 단추 (disabled) Step into (F5)
> 				107 단추 (disabled) Step over (Alt+F5)
> 				108 단추 (disabled) Step out (Shift+F5)
> 				109 단추 (disabled) Set instruction pointer
> 			110 도구 모음 Build
> 				111 메뉴 항목 (disabled) Target Architecture
> 				112 단추 Build changes (F9)
> 				113 단추 Rebuild project (Strg+F9)
> 				114 단추 (disabled) Cancel building (Ctrl+Break)
> 				115 단추 Link project
> 			116 도구 모음 Standard
> 				117 단추 New project (Strg+N)
> 				118 단추 Open a file (Strg+Shift+O)
> 				119 단추 Close active document (Strg+F4)
> 				120 단추 (disabled) Save file (Strg+S)
> 				121 단추 Open project (Strg+O)
> 				122 단추 (disabled) Save project changes (Strg+Shift+S)
> 				123 단추 Close project
> 				124 단추 Print
> 				125 단추 Cut (Strg+X)
> 				126 단추 Copy (Strg+C)
> 				127 단추 Paste (Strg+V)
> 				128 메뉴 항목 (disabled) Undo (Strg+Z)
> 				129 메뉴 항목 (disabled) Redo (Strg+Y)
> 				130 단추 (disabled) Navigate Backward (Alt+Left)
> 				131 단추 (disabled) Navigate Forward (Alt +Right)
> 			132 메뉴 모음 Menu Bar
> 				133 메뉴 항목 FILE
> 				134 메뉴 항목 EDIT
> 				135 메뉴 항목 NETEDIT
> 				136 메뉴 항목 VIEW
> 				137 메뉴 항목 PROJECT
> 				138 메뉴 항목 BUILD
> 				139 메뉴 항목 DEBUG
> 				140 메뉴 항목 ANALYZE
> 				141 메뉴 항목 TOOLS
> 				142 메뉴 항목 EXTRAS
> 				143 메뉴 항목 WINDOW
> 				144 메뉴 항목 HELP
> 		145 창 Splitter ID: 310700120
> 		146 창 Splitter ID: 310700624
> 		147 Tab Debugger ID: 307179656
> 			148 창 ID: 1200
> 				149 창 ID: 1200
> 					150 LIST ID: 307636328
> 						151 머리글 헤더 컨트롤 ID: Header
> 							152 머리글 항목 Name ID: HeaderItem 0
> 							153 머리글 항목 Value ID: HeaderItem 1
> 							154 머리글 항목 SetValue ID: HeaderItem 2
> 							155 머리글 항목 Format ID: HeaderItem 3
> 							156 머리글 항목 Type ID: HeaderItem 4
> 							157 머리글 항목 Address ID: HeaderItem 5
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
> 							173 텍스트
> 							174 텍스트
> 							175 텍스트
> 							176 텍스트
> 							177 텍스트
> 							178 텍스트
> 						179 목록 항목 (selectable)
> 							180 텍스트
> 							181 텍스트
> 							182 텍스트
> 							183 텍스트
> 							184 텍스트
> 							185 텍스트
> 						186 목록 항목 (selectable)
> 							187 텍스트
> 							188 텍스트
> 							189 텍스트
> 							190 텍스트
> 							191 텍스트
> 							192 텍스트
> 					193 스크롤 막대 (disabled) ID: 59904
> 						194 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						195 위치 조정 위치 ID: ScrollbarThumb
> 						196 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			197 탭 항목 (selectable) Python Script
> 			198 탭 항목 (selectable) Debugger
> 			199 탭 항목 (selectable) Output
> 			200 단추 Close
> 		201 창 Splitter ID: 310692896
> 		202 Tab Network View ID: 307176464
> 			203 트리 ID: 104
> 				204 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					205 콘솔 트리 (selectable) Comm_Network
> 					206 콘솔 트리 (selectable) EtherCAT_Network
> 					207 콘솔 트리 (selectable) HW_Network
> 					208 콘솔 트리 (selectable) Motion_Network
> 			209 탭 항목 (selectable) Lib
> 			210 탭 항목 (selectable) File
> 			211 탭 항목 (selectable) Global
> 			212 탭 항목 (selectable) Net
> 			213 탭 항목 (selectable) Class
> 			214 단추 Close
> 		215 Tab Properties ID: 307176920
> 			216 창 ID: 302362672
> 				217 TABLE Properties Window ID: 307233816
> 					218 custom Name
> 					219 custom GUID
> 					220 custom Class
> 					221 custom Position
> 					222 custom Visualized
> 					223 custom World
> 					224 custom RealTime
> 					225 custom BackgroundTime
> 					226 custom RealIndex
> 					227 custom BackgroundIndex
> 					228 custom OPC-UA Instance
> 					229 custom Draw Connection
> 					230 custom Comment
> 				231 도구 모음 ID: 59392
> 					232 단추
> 					233 단추
> 			234 탭 항목 (selectable) Properties
> 			235 단추 Close
>
> The focused UI element is 150 LIST ID: 307636328.
> ```

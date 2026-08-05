>     "displayName": "Advanced IP Scanner",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe",
>     "windows": [],
>     "displayName": "Qt Creator 18.0.1 (Community)",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "windows": [],
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "windows": [],
>     "displayName": "MDS Installation Guide",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe",
>     "windows": [],
>     "displayName": "WSL Settings",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\ctrlX WORKS\\ctrlX Cam Designer\\3.6.0\\ctrlX-CamDesigner.exe",
>     "windows": [],
>     "displayName": "ctrlX Cam Designer 3.6.1",
>     "lastUsedDate": "2026-03-13",
>     "useCount": 0,
>     "isRunning": false
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
>   "title": "Inspect LASAL IDE",
>   "timeout_ms": 30000,
>   "code": "globalThis.targetApp = globalThis.apps.find((app) => app.id === \"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe\");\nif (!globalThis.targetApp) throw new Error(\"LASAL app was not returned\");\nif (globalThis.targetApp.windows.length !== 1) throw new Error(\"Expected exactly one LASAL window; found \" + globalThis.targetApp.windows.length);\nglobalThis.returnedLasalWindow = globalThis.targetApp.windows[0];\nglobalThis.targetWindow = await sky.get_window({id: globalThis.returnedLasalWindow.id, app: globalThis.returnedLasalWindow.app});\nawait sky.activate_window({window: globalThis.targetWindow});\nglobalThis.state = await sky.get_window_state({window: globalThis.targetWindow, include_screenshot: true, include_text: true});\nglobalThis.targetWindow = globalThis.state.window;\nnodeRepl.write(String(globalThis.state.accessibility?.tree || globalThis.state.accessibility?.document_text || \"\"));"
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
> 							32 단추
> 							33 단추 Debugging Context:
> 							34 메뉴 항목 Context
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
> 				50 단추 (disabled) Toggle bookmark
> 				51 단추 (disabled) Previous bookmark
> 				52 단추 (disabled) Next bookmark
> 				53 단추 (disabled) Delete all bookmarks
> 				54 단추 (disabled) Previous bookmark in this file
> 				55 단추 (disabled) Next bookmark in this file
> 				56 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				57 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				58 단추 (disabled) Unindent (Shift+Tab)
> 				59 단추 (disabled) Indent (Tab)
> 			60 도구 모음 Macros Manager
> 				61 메뉴 항목 Macros
> 			62 도구 모음 Layout Manager
> 				63 메뉴 항목 Layouts
> 			64 도구 모음 Toolbox
> 				65 단추 DataAnalyzer
> 				66 단추 Interpreter
> 				67 단추 DiasDrive
> 				68 단추 PLC Diagnosis
> 				69 단추 Hardware Editor
> 				70 단추 Graphical Hardware Editor
> 				71 단추 Connection Manager
> 				72 단추 Task Configuration
> 			73 도구 모음 Net Edit
> 				74 단추 Select
> 				75 단추 Move view
> 				76 단추 Insert comment
> 				77 단추 Zoom(+/-)
> 				78 단추 Zoom to all
> 				79 단추 (disabled) Zoom selection
> 			80 도구 모음 Debug
> 				81 단추 Go online (Alt+F6)
> 				82 단추 (disabled) Change Online Settings
> 				83 메뉴 항목 Online Connection
> 				84 단추 (disabled) Set Online Connection For Current Project
> 				85 단추 Download (F6)
> 				86 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				87 단추 Download Module on the Fly
> 				88 단추 (disabled) Save Project on PLC
> 				89 단추 (disabled) Start (F7)
> 				90 단추 Reset (F8)
> 				91 단추 (disabled) Toggle breakpoint (F4)
> 				92 단추 (disabled) Create condition breakpoint
> 				93 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				94 단추 AWL trigger on/off
> 				95 단추 (disabled) Fix AWL trigger to current instruction
> 				96 단추 Activate/Deactivate Live View
> 				97 단추 Activate/Deactivate Auto Watch
> 				98 단추 (disabled) Goto instruction pointer
> 				99 단추 (disabled) Step into (F5)
> 				100 단추 (disabled) Step over (Alt+F5)
> 				101 단추 (disabled) Step out (Shift+F5)
> 				102 단추 (disabled) Set instruction pointer
> 			103 도구 모음 Build
> 				104 메뉴 항목 (disabled) Target Architecture
> 				105 단추 Build changes (F9)
> 				106 단추 Rebuild project (Strg+F9)
> 				107 단추 (disabled) Cancel building (Ctrl+Break)
> 				108 단추 Link project
> 			109 도구 모음 Standard
> 				110 단추 New project (Strg+N)
> 				111 단추 Open a file (Strg+Shift+O)
> 				112 단추 Close active document (Strg+F4)
> 				113 단추 (disabled) Save file (Strg+S)
> 				114 단추 Open project (Strg+O)
> 				115 단추 (disabled) Save project changes (Strg+Shift+S)
> 				116 단추 Close project
> 				117 단추 Print
> 				118 단추 Cut (Strg+X)
> 				119 단추 Copy (Strg+C)
> 				120 단추 Paste (Strg+V)
> 				121 메뉴 항목 (disabled) Undo (Strg+Z)
> 				122 메뉴 항목 (disabled) Redo (Strg+Y)
> 				123 단추 (disabled) Navigate Backward (Alt+Left)
> 				124 단추 (disabled) Navigate Forward (Alt +Right)
> 			125 메뉴 모음 Menu Bar
> 				126 메뉴 항목 FILE
> 				127 메뉴 항목 EDIT
> 				128 메뉴 항목 NETEDIT
> 				129 메뉴 항목 VIEW
> 				130 메뉴 항목 PROJECT
> 				131 메뉴 항목 BUILD
> 				132 메뉴 항목 DEBUG
> 				133 메뉴 항목 ANALYZE
> 				134 메뉴 항목 TOOLS
> 				135 메뉴 항목 EXTRAS
> 				136 메뉴 항목 WINDOW
> 				137 메뉴 항목 HELP
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
> 					186 스크롤 막대 (disabled) ID: 59904
> 						187 단추 왼쪽 스크롤 화살표 ID: UpButton

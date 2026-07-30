>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe",
>     "windows": [],
>     "displayName": "WSL Settings",
>     "lastUsedDate": "2026-04-03",
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
>   "code": "globalThis.targetApp = apps.find(a => a.id === '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe'); if (!targetApp || targetApp.windows.length !== 1) throw new Error('Expected one LASAL window'); globalThis.returnedWindow = targetApp.windows[0]; globalThis.targetWindow = await sky.get_window({id: returnedWindow.id, app: returnedWindow.app}); await sky.activate_window({window: targetWindow}); globalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: true, include_text: true}); globalThis.targetWindow = state.window; nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ''));",
>   "title": "LASAL 런타임 화면 읽기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network [RO]", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network [RO] Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Comm_Network [RO] Secondary Actions: Raise ID: 65281
> 				3 창 ID: 59648
> 					4 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5 단추 위쪽 스크롤 화살표 ID: UpButton
> 						6 위치 조정 위치 ID: ScrollbarThumb
> 						7 단추 페이지 아래로 ID: DownPageButton
> 						8 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			9 창 TCPMotionInterface [RO] Secondary Actions: Raise ID: 65280
> 				10 창 ID: 59648
> 					11 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						12 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							13 단추 위쪽 스크롤 화살표 ID: UpButton
> 							14 위치 조정 위치 ID: ScrollbarThumb
> 							15 단추 페이지 아래로 ID: DownPageButton
> 							16 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						17 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							18 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							19 위치 조정 위치 ID: ScrollbarThumb
> 							20 단추 페이지 오른쪽으로 ID: DownPageButton
> 							21 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						22 위치 조정 (disabled)
> 					23 창 xtpBarTop ID: 59419
> 						24 도구 모음 Object Editor
> 							25 단추
> 							26 단추 Debugging Context:
> 							27 메뉴 항목 Context
> 		28 상태 표시줄 ID: 59393
> 			29 텍스트 
> 			30 텍스트
> 			31 텍스트
> 			32 텍스트
> 			33 텍스트
> 			34 텍스트
> 			35 텍스트
> 			36 텍스트
> 			37 텍스트
> 			38 텍스트 NUM
> 			39 텍스트
> 		40 창 xtpBarTop ID: 59419
> 			41 도구 모음 Script
> 			42 도구 모음 Edit
> 				43 단추 (disabled) Toggle bookmark
> 				44 단추 (disabled) Previous bookmark
> 				45 단추 (disabled) Next bookmark
> 				46 단추 (disabled) Delete all bookmarks
> 				47 단추 (disabled) Previous bookmark in this file
> 				48 단추 (disabled) Next bookmark in this file
> 				49 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				50 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				51 단추 (disabled) Unindent (Shift+Tab)
> 				52 단추 (disabled) Indent (Tab)
> 			53 도구 모음 Macros Manager
> 				54 메뉴 항목 Macros
> 			55 도구 모음 Layout Manager
> 				56 메뉴 항목 Layouts
> 			57 도구 모음 Toolbox
> 				58 단추 DataAnalyzer
> 				59 단추 Interpreter
> 				60 단추 DiasDrive
> 				61 단추 PLC Diagnosis
> 				62 단추 Hardware Editor
> 				63 단추 Graphical Hardware Editor
> 				64 단추 Connection Manager
> 				65 단추 Task Configuration
> 			66 도구 모음 Net Edit
> 				67 단추 Select
> 				68 단추 Move view
> 				69 단추 Insert comment
> 				70 단추 Zoom(+/-)
> 				71 단추 Zoom to all
> 				72 단추 (disabled) Zoom selection
> 			73 도구 모음 Debug
> 				74 단추 Go online (Alt+F6)
> 				75 단추 (disabled) Change Online Settings
> 				76 메뉴 항목 Online Connection
> 				77 단추 (disabled) Set Online Connection For Current Project
> 				78 단추 Download (F6)
> 				79 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				80 단추 Download Module on the Fly
> 				81 단추 (disabled) Save Project on PLC
> 				82 단추 (disabled) Start (F7)
> 				83 단추 Reset (F8)
> 				84 단추 (disabled) Toggle breakpoint (F4)
> 				85 단추 (disabled) Create condition breakpoint
> 				86 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				87 단추 AWL trigger on/off
> 				88 단추 (disabled) Fix AWL trigger to current instruction
> 				89 단추 Activate/Deactivate Live View
> 				90 단추 Activate/Deactivate Auto Watch
> 				91 단추 (disabled) Goto instruction pointer
> 				92 단추 (disabled) Step into (F5)
> 				93 단추 (disabled) Step over (Alt+F5)
> 				94 단추 (disabled) Step out (Shift+F5)
> 				95 단추 (disabled) Set instruction pointer
> 			96 도구 모음 Build
> 				97 메뉴 항목 (disabled) Target Architecture
> 				98 단추 Build changes (F9)
> 				99 단추 Rebuild project (Strg+F9)
> 				100 단추 (disabled) Cancel building (Ctrl+Break)
> 				101 단추 Link project
> 			102 도구 모음 Standard
> 				103 단추 New project (Strg+N)
> 				104 단추 Open a file (Strg+Shift+O)
> 				105 단추 Close active document (Strg+F4)
> 				106 단추 (disabled) Save file (Strg+S)
> 				107 단추 Open project (Strg+O)
> 				108 단추 (disabled) Save project changes (Strg+Shift+S)
> 				109 단추 Close project
> 				110 단추 Print
> 				111 단추 Cut (Strg+X)
> 				112 단추 Copy (Strg+C)
> 				113 단추 Paste (Strg+V)
> 				114 메뉴 항목 (disabled) Undo (Strg+Z)
> 				115 메뉴 항목 (disabled) Redo (Strg+Y)
> 				116 단추 (disabled) Navigate Backward (Alt+Left)
> 				117 단추 (disabled) Navigate Forward (Alt +Right)
> 			118 메뉴 모음 Menu Bar
> 				119 메뉴 항목 FILE
> 				120 메뉴 항목 EDIT
> 				121 메뉴 항목 NETEDIT
> 				122 메뉴 항목 VIEW
> 				123 메뉴 항목 PROJECT
> 				124 메뉴 항목 BUILD
> 				125 메뉴 항목 DEBUG
> 				126 메뉴 항목 ANALYZE
> 				127 메뉴 항목 TOOLS
> 				128 메뉴 항목 EXTRAS
> 				129 메뉴 항목 WINDOW
> 				130 메뉴 항목 HELP
> 		131 창 Splitter ID: 412901000
> 		132 창 Splitter ID: 412902848
> 		133 Tab Debugger ID: 302504992
> 			134 창 ID: 1200
> 				135 창 ID: 1200
> 					136 LIST ID: 303024280
> 						137 머리글 헤더 컨트롤 ID: Header
> 							138 머리글 항목 Name ID: HeaderItem 0
> 							139 머리글 항목 Value ID: HeaderItem 1
> 							140 머리글 항목 SetValue ID: HeaderItem 2
> 							141 머리글 항목 Format ID: HeaderItem 3
> 							142 머리글 항목 Type ID: HeaderItem 4
> 							143 머리글 항목 Address ID: HeaderItem 5
> 						144 목록 항목 (selectable)
> 							145 텍스트
> 							146 텍스트
> 							147 텍스트
> 							148 텍스트
> 							149 텍스트
> 							150 텍스트
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
> 					179 스크롤 막대 (disabled) ID: 59904
> 						180 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						181 위치 조정 위치 ID: ScrollbarThumb
> 						182 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			183 탭 항목 (selectable) Python Script
> 			184 탭 항목 (selectable) Debugger
> 			185 탭 항목 (selectable) Output
> 			186 단추 Close
> 		187 창 Splitter ID: 412902008
> 		188 Tab Network View ID: 302499064
> 			189 트리 ID: 104
> 				190 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					191 콘솔 트리 (selectable) Comm_Network
> 					192 콘솔 트리 (selectable) EtherCAT_Network
> 					193 콘솔 트리 (selectable) HW_Network
> 					194 콘솔 트리 (selectable) Motion_Network
> 			195 탭 항목 (selectable) Lib
> 			196 탭 항목 (selectable) File
> 			197 탭 항목 (selectable) Global
> 			198 탭 항목 (selectable) Net
> 			199 탭 항목 (selectable) Class
> 			200 단추 Close
> 		201 Tab Properties ID: 302499520
> 			202 창 ID: 298037752
> 				203 TABLE Properties Window ID: 302619936
> 					204 custom Name
> 					205 custom Revision
> 					206 custom Type
> 					207 custom Draw Connection
> 					208 custom Comment
> 					209 custom Filename
> 				210 도구 모음 ID: 59392
> 					211 단추
> 					212 단추
> 			213 탭 항목 (selectable) Properties
> 			214 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> Image output: image/jpeg
>
> LASAL 화면에서 현재 런타임은 명확합니다: `Port=4000`, 서버 제어 상태 `_STATE_ACCEPT`, `ErrorCode=0`, `ConnectedClients=0`, 인터페이스 상태 `_STATE_RUNNING`입니다. 즉 PLC 자체와 LASAL 프로젝트는 살아 있고, 현재는 실제 클라이언트가 하나도 붙어 있지 않습니다. 이제 동일 PC에서 테스트 앱의 실제 Connect 단계만 재현해 어느 예외가 나는지 확인하겠습니다. 축 명령은 보내지 않습니다.
>
> MCP tool call
>

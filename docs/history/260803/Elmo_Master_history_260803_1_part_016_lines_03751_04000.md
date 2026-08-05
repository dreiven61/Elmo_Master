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
> 						188 위치 조정 위치 ID: ScrollbarThumb
> 						189 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			16272 탭 항목 (selectable) Python Script
> 			16273 탭 항목 (selectable) Debugger
> 			16274 탭 항목 (selectable) Output
> 			193 단추 Close
> 		194 창 Splitter ID: 314042368
> 		195 Tab Class View ID: 424886104
> 			16275 트리 ID: 103
> 				16276 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					16277 콘솔 트리 (selectable) External
> 					16278 콘솔 트리 (selectable) Sigmatek
> 					16279 콘솔 트리 (selectable) Elmo_1
> 					16280 콘솔 트리 (selectable) Elmo_2
> 					16281 콘솔 트리 (selectable) Elmo_3
> 					16282 콘솔 트리 (selectable) Elmo_4
> 					16283 콘솔 트리 (selectable) GL_9086_1
> 					16284 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					16285 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					16286 콘솔 트리 (selectable) LMCControlCommandService
> 					16287 콘솔 트리 (selectable) LMCDiagnosticsService
> 					16288 콘솔 트리 (selectable) LMCEcatInputLatch
> 					16289 콘솔 트리 (selectable) LMCRecorderStore
> 					16290 콘솔 트리 (selectable) LMCSdoExecutor
> 					16291 콘솔 트리 (selectable) TCPIPServer
> 					16292 콘솔 트리 (selectable) TCPMotionInterface
> 			16293 탭 항목 (selectable) Lib
> 			16294 탭 항목 (selectable) File
> 			16295 탭 항목 (selectable) Global
> 			16296 탭 항목 (selectable) Net
> 			16297 탭 항목 (selectable) Class
> 			207 단추 Close
> 		208 Tab Properties ID: 424886560
> 			209 창 ID: 309505912
> 				210 TABLE Properties Window ID: 315226536
> 					16298 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						16299 단추 위쪽 스크롤 화살표 ID: UpButton
> 						16300 위치 조정 위치 ID: ScrollbarThumb
> 						16301 단추 페이지 아래로 ID: DownPageButton
> 						16302 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					16303 custom Project Version
> 					16304 custom Name
> 					16305 custom Revision
> 					16306 custom Online Connection
> 					16307 custom CodeGenerator
> 					16308 custom Networks
> 					16309 custom Server list
> 					16310 custom Target
> 					16311 custom Compiler
> 					16312 custom Directory
> 					16313 custom OPC-UA
> 					16314 custom Load all libraries
> 					16315 custom Use Unit System
> 					16316 custom Include Paths
> 					16317 custom Library Paths
> 					16318 custom Backup Includes and Loader
> 					16319 custom Ignore at Cleanup
> 					16320 custom Reencrypt Project On Close
> 					16321 custom Enable OPC UA
> 					16322 custom Enable initvalues for output parameters
> 					16323 custom Use multiple CPU core
> 					16324 custom Use Advanced-IO
> 					16325 custom AutomationML
> 					16326 custom IO Connection Manager Options
> 					16327 custom Comment
> 					16328 custom Filename
> 				224 도구 모음 ID: 59392
> 					225 단추
> 					226 단추
> 			16329 탭 항목 (selectable) Properties
> 			228 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCSdoExecutor 클래스 열기",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:false,include_text:true}); var execMatch = state.accessibility.tree.match(/\\n\\s*(\\d+) 콘솔 트리 \\(selectable\\) LMCSdoExecutor\\n/); if(!execMatch) throw new Error('LMCSdoExecutor not found'); await sky.click({window:state.window,element_index:Number(execMatch[1]),click_count:2}); state = await sky.get_window_state({window:state.window,include_screenshot:false,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(0,9000))"
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
> 							17156 단추
> 							17157 단추 Debugging Context:
> 							17158 메뉴 항목 Context
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
> 				17159 단추 (disabled) Toggle bookmark
> 				17160 단추 (disabled) Previous bookmark
> 				17161 단추 (disabled) Next bookmark
> 				17162 단추 (disabled) Delete all bookmarks
> 				17163 단추 (disabled) Previous bookmark in this file
> 				17164 단추 (disabled) Next bookmark in this file
> 				17165 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				17166 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				17167 단추 (disabled) Unindent (Shift+Tab)
> 				17168 단추 (disabled) Indent (Tab)
> 			60 도구 모음 Macros Manager
> 				17169 메뉴 항목 Macros
> 			62 도구 모음 Layout Manager
> 				17170 메뉴 항목 Layouts
> 			64 도구 모음 Toolbox
> 				17171 단추 DataAnalyzer
> 				17172 단추 Interpreter
> 				17173 단추 DiasDrive
> 				17174 단추 PLC Diagnosis
> 				17175 단추 Hardware Editor
> 				17176 단추 Graphical Hardware Editor
> 				17177 단추 Connection Manager
> 				17178 단추 Task Configuration
> 			73 도구 모음 Net Edit
> 				17179 단추 Select
> 				17180 단추 Move view
> 				17181 단추 Insert comment
> 				17182 단추 Zoom(+/-)
> 				17183 단추 Zoom to all
> 				17184 단추 (disabled) Zoom selection
> 			80 도구 모음 Debug
> 				17185 단추 Go online (Alt+F6)
> 				17186 단추 (disabled) Change Online Settings
> 				17187 메뉴 항목 Online Connection
> 				17188 단추 (disabled) Set Online Connection For Current Project
> 				17189 단추 Download (F6)
> 				17190 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				17191 단추 Download Module on the Fly
> 				17192 단추 (disabled) Save Project on PLC
> 				17193 단추 (disabled) Start (F7)
> 				17194 단추 Reset (F8)
> 				17195 단추 (disabled) Toggle breakpoint (F4)
> 				17196 단추 (disabled) Create condition breakpoint
> 				17197 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				17198 단추 AWL trigger on/off
> 				17199 단추 (disabled) Fix AWL trigger to current instruction
> 				17200 단추 Activate/Deactivate Live View
> 				17201 단추 Activate/Deactivate Auto Watch
> 				17202 단추 (disabled) Goto instruction pointer
> 				17203 단추 (disabled) Step into (F5)
> 				17204 단추 (disabled) Step over (Alt+F5)
> 				17205 단추 (disabled) Step out (Shift+F5)
> 				17206 단추 (disabled) Set instruction pointer
> 			103 도구 모음 Build
> 				17207 메뉴 항목 (disabled) Target Architecture
> 				17208 단추 Build changes (F9)
> 				17209 단추 Rebuild project (Strg+F9)
> 				17210 단추 (disabled) Cancel building (Ctrl+Break)
> 				17211 단추 Link project
> 			109 도구 모음 Standard
> 				17212 단추 New project (Strg+N)
> 				17213 단추 Open a file (Strg+Shift+O)
> 				17214 단추 Close active document (Strg+F4)
> 				17215 단추 (disabled) Save file (Strg+S)
> 				17216 단추 Open project (Strg+O)
> 				17217 단추 (disabled) Save project changes (Strg+Shift+S)
> 				17218 단추 Close project
> 				17219 단추 Print
> 				17220 단추 Cut (Strg+X)
> 				17221 단추 Copy (Strg+C)
> 				17222 단추 Paste (Strg+V)
> 				17223 메뉴 항목 (disabled) Undo (Strg+Z)
> 				17224 메뉴 항목 (disabled) Redo (Strg+Y)

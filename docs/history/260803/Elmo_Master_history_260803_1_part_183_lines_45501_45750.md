> 			22376 탭 항목 (selectable) Net
> 			22377 탭 항목 (selectable) Class
> 			214 단추 Close
> 		215 Tab Properties ID: 307176920
> 			216 창 ID: 302362672
> 				217 TABLE Properties Window ID: 307233816
> 					22378 custom Name
> 					22379 custom GUID
> 					22380 custom Class
> 					22381 custom Position
> 					22382 custom Visualized
> 					22383 custom World
> 					22384 custom RealTime
> 					22385 custom BackgroundTime
> 					22386 custom RealIndex
> 					22387 custom BackgroundIndex
> 					22388 custom OPC-UA Instance
> 					22389 custom Draw Connection
> 					22390 custom Comment
> 				231 도구 모음 ID: 59392
> 					232 단추
> 					233 단추
> 			22391 탭 항목 (selectable) Properties
> 			235 단추 Close
>
> The focused UI element is 3 창 ID: 59648.
> ```
>
> Image output: image/jpeg
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Home 기능은 세 경로로 분리해 진행 중입니다. `MMC_Home = MoveReference`, `DS402 Home = PLC 순차 제어`, `멀티턴 리셋 = TEST ONLY 고정 SDO(TW[20])`로 구현하며, 지금은 병렬 작업 결과를 합치고 LASAL IDE 구조 변경을 이어가겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true}); console.log((state.accessibility&&state.accessibility.tree)||''); if (state.screenshots) { for (const s of state.screenshots) await nodeRepl.emitImage(s); }",
>   "title": "LASAL IDE 상태 확인"
> }
> ```
>
> ```text
> nodeRepl.emitImage received an unsupported value
> ```
>
> <details><summary>Called 3 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "console.log(JSON.stringify({keys:Object.keys(state||{}), shotType:typeof state?.screenshot, shotsType:typeof state?.screenshots, shotKeys:state?.screenshot?Object.keys(state.screenshot):null, shotsLen:Array.isArray(state?.screenshots)?state.screenshots.length:null, title:state?.title},null,2)); console.log((state?.accessibility?.tree||'').slice(0,12000));",
>   "title": "LASAL IDE 구조 확인"
> }
> ```
>
> ```text
> {
>   "keys": [
>     "window",
>     "screenshots",
>     "accessibility"
>   ],
>   "shotType": "undefined",
>   "shotsType": "object",
>   "shotKeys": null,
>   "shotsLen": 1
> }
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
> 							38457 단추
> 							38458 단추 Debugging Context:
> 							38459 메뉴 항목 Context
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
> 				38460 단추 (disabled) Toggle bookmark
> 				38461 단추 (disabled) Previous bookmark
> 				38462 단추 (disabled) Next bookmark
> 				38463 단추 (disabled) Delete all bookmarks
> 				38464 단추 (disabled) Previous bookmark in this file
> 				38465 단추 (disabled) Next bookmark in this file
> 				38466 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				38467 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				38468 단추 (disabled) Unindent (Shift+Tab)
> 				38469 단추 (disabled) Indent (Tab)
> 			67 도구 모음 Macros Manager
> 				38470 메뉴 항목 Macros
> 			69 도구 모음 Layout Manager
> 				38471 메뉴 항목 Layouts
> 			71 도구 모음 Toolbox
> 				38472 단추 DataAnalyzer
> 				38473 단추 Interpreter
> 				38474 단추 DiasDrive
> 				38475 단추 PLC Diagnosis
> 				38476 단추 Hardware Editor
> 				38477 단추 Graphical Hardware Editor
> 				38478 단추 Connection Manager
> 				38479 단추 Task Configuration
> 			80 도구 모음 Net Edit
> 				38480 단추 Select
> 				38481 단추 Move view
> 				38482 단추 Insert comment
> 				38483 단추 Zoom(+/-)
> 				38484 단추 Zoom to all
> 				38485 단추 (disabled) Zoom selection
> 			87 도구 모음 Debug
> 				38486 단추 Go online (Alt+F6)
> 				38487 단추 (disabled) Change Online Settings
> 				38488 메뉴 항목 Online Connection
> 				38489 단추 (disabled) Set Online Connection For Current Project
> 				38490 단추 Download (F6)
> 				38491 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				38492 단추 Download Module on the Fly
> 				38493 단추 (disabled) Save Project on PLC
> 				38494 단추 (disabled) Start (F7)
> 				38495 단추 Reset (F8)
> 				38496 단추 (disabled) Toggle breakpoint (F4)
> 				38497 단추 (disabled) Create condition breakpoint
> 				38498 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				38499 단추 AWL trigger on/off
> 				38500 단추 (disabled) Fix AWL trigger to current instruction
> 				38501 단추 Activate/Deactivate Live View
> 				38502 단추 Activate/Deactivate Auto Watch
> 				38503 단추 (disabled) Goto instruction pointer
> 				38504 단추 (disabled) Step into (F5)
> 				38505 단추 (disabled) Step over (Alt+F5)
> 				38506 단추 (disabled) Step out (Shift+F5)
> 				38507 단추 (disabled) Set instruction pointer
> 			110 도구 모음 Build
> 				38508 메뉴 항목 (disabled) Target Architecture
> 				38509 단추 Build changes (F9)
> 				38510 단추 Rebuild project (Strg+F9)
> 				38511 단추 (disabled) Cancel building (Ctrl+Break)
> 				38512 단추 Link project
> 			116 도구 모음 Standard
> 				38513 단추 New project (Strg+N)
> 				38514 단추 Open a file (Strg+Shift+O)
> 				38515 단추 Close active document (Strg+F4)
> 				38516 단추 (disabled) Save file (Strg+S)
> 				38517 단추 Open project (Strg+O)
> 				38518 단추 (disabled) Save project changes (Strg+Shift+S)
> 				38519 단추 Close project
> 				38520 단추 Print
> 				38521 단추 Cut (Strg+X)
> 				38522 단추 Copy (Strg+C)
> 				38523 단추 Paste (Strg+V)
> 				38524 메뉴 항목 (disabled) Undo (Strg+Z)
> 				38525 메뉴 항목 (disabled) Redo (Strg+Y)
> 				38526 단추 (disabled) Navigate Backward (Alt+Left)
> 				38527 단추 (disabled) Navigate Forward (Alt +Right)
> 			132 메뉴 모음 Menu Bar
> 				38528 메뉴 항목 FILE
> 				38529 메뉴 항목 EDIT
> 				38530 메뉴 항목 NETEDIT
> 				38531 메뉴 항목 VIEW
> 				38532 메뉴 항목 PROJECT
> 				38533 메뉴 항목 BUILD
> 				38534 메뉴 항목 DEBUG
> 				38535 메뉴 항목 ANALYZE
> 				38536 메뉴 항목 TOOLS
> 				38537 메뉴 항목 EXTRAS
> 				38538 메뉴 항목 WINDOW
> 				38539 메뉴 항목 HELP
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

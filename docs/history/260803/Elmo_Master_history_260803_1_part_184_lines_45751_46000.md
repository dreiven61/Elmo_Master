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
> 			38540 탭 항목 (selectable) Python Script
> 			38541 탭 항목 (selectable) Debugger
> 			38542 탭 항목 (selectable) Output
> 			200 단추 Close
> 		201 창 Splitter ID: 310692896
> 		202 Tab Network View ID: 307176464
> 			203 트리 ID: 104
> 				204 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					205 콘솔 트리 (selectable) Comm_Network
> 					206 콘솔 트리 (selectable) EtherCAT_Network
> 					207 콘솔 트리 (selectable) HW_Network
> 					208 콘솔 트리 (selectable) Motion_Network
> 			38543 탭 항목 (selectable) Lib
> 			38544 탭 항목 (selectable) File
> 			38545 탭 항목 (selectable) Global
> 			38546 탭 항목 (selectable) Net
> 			38547 탭 항목 (selectable) Class
> 			214 단추 Close
> 		215 Tab Properties ID: 307176920
> 			216 창 ID: 302362672
> 				217 TABLE Properties Window ID: 307233816
> 					38548 custom Name
> 					38549 custom GUID
> 					38550 custom Class
> 					38551 custom Position
> 					38552 custom Visualized
> 					38553 custom World
> 					38554 custom RealTime
> 					38555 custom BackgroundTime
> 					38556 custom RealIndex
> 					38557 custom BackgroundIndex
> 					38558 custom OPC-UA Instance
> 					38559 custom Draw Connection
> 					38560 custom Comment
> 				231 도구 모음 ID: 59392
> 					232 단추
> 					233 단추
> 			38561 탭 항목 (selectable) Properties
> 			235 단추 Close
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
>   "code": "console.log(JSON.stringify(state.screenshots?.map(x=>({type:typeof x, keys:x&&typeof x==='object'?Object.keys(x):null, preview:typeof x==='string'?x.slice(0,80):null})),null,2));",
>   "title": "LASAL 화면 데이터 확인"
> }
> ```
>
> ```text
> [
>   {
>     "type": "object",
>     "keys": [
>       "id",
>       "zIndex",
>       "url",
>       "originX",
>       "originY",
>       "width",
>       "height"
>     ],
>     "preview": null
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
>   "code": "await nodeRepl.emitImage(state.screenshots[0].url);",
>   "title": "LASAL IDE 화면 보기"
> }
> ```
>
> Image output: image/jpeg
>
> </details>
>
> LASAL 프로젝트가 현재 PLC 온라인 읽기 전용 상태라 구조를 수정할 수 없습니다. Computer Use 절차에 따라 먼저 오프라인으로 전환한 뒤, 생성 선언과 Network 연결만 IDE에서 변경하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:'ALT+F6'});",
>   "title": "LASAL 오프라인 전환"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true}); console.log((state.accessibility&&state.accessibility.tree||'').slice(0,9000)); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "title": "LASAL 오프라인 상태 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Motion_Network Secondary Actions: Raise ID: 65282
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
> 			16 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						19 단추 위쪽 스크롤 화살표 ID: UpButton
> 						20 위치 조정 위치 ID: ScrollbarThumb
> 						21 단추 페이지 아래로 ID: DownPageButton
> 						22 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			23 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
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
> 		42 상태 표시줄 ID: 59393
> 			43 텍스트
> 			44 텍스트
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트
> 			49 텍스트
> 			50 텍스트
> 			51 텍스트 NUM
> 			52 텍스트
> 		54 창 xtpBarTop ID: 59419
> 			55 도구 모음 Script
> 			56 도구 모음 Edit
> 				40362 단추 (disabled) Toggle bookmark
> 				40363 단추 (disabled) Previous bookmark
> 				40364 단추 (disabled) Next bookmark
> 				40365 단추 (disabled) Delete all bookmarks
> 				40366 단추 (disabled) Previous bookmark in this file
> 				40367 단추 (disabled) Next bookmark in this file
> 				40368 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				40369 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				40370 단추 (disabled) Unindent (Shift+Tab)
> 				40371 단추 (disabled) Indent (Tab)
> 			67 도구 모음 Macros Manager
> 				40372 메뉴 항목 Macros
> 			69 도구 모음 Layout Manager
> 				40373 메뉴 항목 Layouts
> 			71 도구 모음 Toolbox
> 				40374 단추 DataAnalyzer
> 				40375 단추 Interpreter
> 				40376 단추 DiasDrive
> 				40377 단추 PLC Diagnosis
> 				40378 단추 Hardware Editor
> 				40379 단추 Graphical Hardware Editor
> 				40380 단추 Connection Manager
> 				40381 단추 Task Configuration
> 			80 도구 모음 Net Edit
> 				40382 단추 Select
> 				40383 단추 Move view
> 				40384 단추 Insert comment
> 				40385 단추 Zoom(+/-)
> 				40386 단추 Zoom to all
> 				40387 단추 (disabled) Zoom selection
> 			87 도구 모음 Debug
> 				40388 단추 Go online (Alt+F6)
> 				40389 단추 Change Online Settings
> 				40390 메뉴 항목 Online Connection
> 				40391 단추 (disabled) Set Online Connection For Current Project
> 				40392 단추 (disabled) Download (F6)
> 				40393 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				40394 단추 (disabled) Download Module on the Fly
> 				40395 단추 (disabled) Save Project on PLC
> 				40396 단추 (disabled) Start (F7)
> 				40397 단추 (disabled) Reset (F8)
> 				40398 단추 (disabled) Toggle breakpoint (F4)
> 				40399 단추 (disabled) Create condition breakpoint
> 				40400 단추 (disabled) Remove all breakpoint(s) (Shift+F4)
> 				40401 단추 AWL trigger on/off
> 				40402 단추 (disabled) Fix AWL trigger to current instruction
> 				40403 단추 Activate/Deactivate Live View
> 				40404 단추 Activate/Deactivate Auto Watch
> 				40405 단추 (disabled) Goto instruction pointer
> 				40406 단추 (disabled) Step into (F5)
> 				40407 단추 (disabled) Step over (Alt+F5)
> 				40408 단추 (disabled) Step out (Shift+F5)
> 				40409 단추 (disabled) Set instruction pointer
> 			110 도구 모음 Build
> 				40410 메뉴 항목 Target Architecture
> 				40411 단추 Build changes (F9)
> 				40412 단추 Rebuild project (Strg+F9)

>   "title": "Inspect diagnostics client",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>x.includes('Diagnostics')||x.includes('Properties')).slice(-10).join('\\n')); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 					283 콘솔 트리 (selectable) LMCDiagnosticsService
> 							320 콘솔 트리 (selectable) Diagnostics
> ```
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
>   "title": "Open LASAL view menu",
>   "code": "await sky.press_key({window:targetWindow,key:'Alt_L+v'}); state=null; nodeRepl.write('view menu');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> view menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Find properties command",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').slice(0,40).join('\\n')); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface* Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴 View
> 			4 메뉴 항목 Output Pane
> 			5 메뉴 항목 Properties Window
> 			6 메뉴 항목 Debugger Pane
> 			7 메뉴 항목 Python Pane
> 			8 메뉴 항목 DataAnalyzer Pane
> 			9 메뉴 항목 Drive Pane
> 			10 메뉴 항목 Multimaster Pane
> 			11 메뉴 항목 Code Analysis
> 			12 메뉴 항목 Trees
> 			13 메뉴 항목 Toolbars
> 			14 메뉴 항목 Status Bar
> 			15 메뉴 항목 Layout
> 			16 메뉴 항목 Graphical User Interface
> 		17 창 작업 영역 ID: 59648
> 			18 창 TCPMotionInterface* Secondary Actions: Raise ID: 65280
> 				19 창 ID: 59648
> 					20 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000
> 						21 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							22 단추 위쪽 스크롤 화살표 ID: UpButton
> 							23 단추 페이지 위로 ID: UpPageButton
> 							24 위치 조정 위치 ID: ScrollbarThumb
> 							25 단추 페이지 아래로 ID: DownPageButton
> 							26 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						27 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							28 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							29 위치 조정 위치 ID: ScrollbarThumb
> 							30 단추 페이지 오른쪽으로 ID: DownPageButton
> 							31 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						32 위치 조정 (disabled)
> 			33 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				34 창 ID: 59648
> 					35 창 xtpBarTop ID: 59419
> 						36 도구 모음 Hardware Editor
> 							37 단추 Hardware Editor Configuration Settings
> 							38 단추 Calculate Resources of Project
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
>   "title": "Open client properties",
>   "code": "await sky.click({window:targetWindow,element_index:5}); state=null; nodeRepl.write('properties opened');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> properties opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect client properties",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').filter(x=>/Properties|custom (Name|DataType|Class|Required|Object|Channel|Internal)/.test(x)).slice(-30).join('\\n')); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 		335 Tab Properties ID: 125485008
> 				337 TABLE Properties Window ID: 293314152
> 					343 custom Name
> 					344 custom DataType
> 					346 custom Required
> 			352 탭 항목 (selectable) Properties
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Set diagnostics client type",
>   "code": "await sky.click({window:targetWindow,x:1226,y:644,click_count:2,screenshotId:state.screenshots[0].id}); state=null; nodeRepl.write('type editor');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> type editor
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Choose object channel",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({focused:state.accessibility.focused_element,tail:state.accessibility.tree.split('\\n').slice(-20)})); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"focused":"336 TABLE Properties Window ID: 293314152","tail":["\t\t\t335 창 ID: 290002192","\t\t\t\t336 TABLE Properties Window ID: 293314152","\t\t\t\t\t337 스크롤 막대 세로 ID: NonClientVerticalScrollBar","\t\t\t\t\t\t338 단추 위쪽 스크롤 화살표 ID: UpButton","\t\t\t\t\t\t339 위치 조정 위치 ID: ScrollbarThumb","\t\t\t\t\t\t340 단추 페이지 아래로 ID: DownPageButton","\t\t\t\t\t\t341 단추 아래쪽 스크롤 화살표 ID: DownButton","\t\t\t\t\t342 custom Name","\t\t\t\t\t343 custom DataType","\t\t\t\t\t344 custom Type","\t\t\t\t\t345 custom Required","\t\t\t\t\t346 custom DefaultInitValue","\t\t\t\t\t347 custom Comment","\t\t\t\t348 도구 모음 ID: 59392","\t\t\t\t\t349 단추","\t\t\t\t\t350 단추","\t\t\t351 탭 항목 (selectable) Properties","\t\t\t352 단추 Close","","The focused UI element is 336 TABLE Properties Window ID: 293314152."]}
> ```
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
>   "title": "Open client type choices",
>   "code": "await sky.click({window:targetWindow,x:1411,y:641,screenshotId:state.screenshots[0].id}); state=null; nodeRepl.write('opened');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Choose object channel",
>   "code": "state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.split('\\n').slice(-20).join('\\n')); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 				336 TABLE Properties Window ID: 293314152
> 					337 편집 ID: 881865800
> 					338 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						339 단추 위쪽 스크롤 화살표 ID: UpButton
> 						340 위치 조정 위치 ID: ScrollbarThumb
> 						341 단추 페이지 아래로 ID: DownPageButton
> 						342 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					343 custom Name
> 					344 custom DataType
> 					345 custom Type
> 					346 custom Required
> 					347 custom DefaultInitValue
> 					348 custom Comment
> 				349 도구 모음 ID: 59392
> 					350 단추
> 					351 단추
> 			352 탭 항목 (selectable) Properties
> 			353 단추 Close
>
> The focused UI element is 345 custom Type.
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

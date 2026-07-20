>   action: string; // Secondary action label from `get_window_state()`, such as `Raise`, `Scroll Up`, `Scroll Down`, `Scroll Left`, `Scroll Right`, `Expand`, or `Collapse`; matching is case-insensitive.
>   element_index: number; // Element index from the latest `get_window_state()` accessibility tree.
>   window: Window; // Window object from `list_apps()` or `list_windows()` containing the element.
> };
>
> type ActivateWindowInput = {
>   window: Window; // Window object from `list_apps()` or `list_windows()` to bring to the foreground.
> };
>
> type AppIdentifier = string;
>
> type AccessibilityState = {
>   document_text?: string; // Document text for the focused or most relevant document element when available.
>   focused_element?: string; // Formatted line for the focused element when available.
>   selected_elements?: Array<string>; // Formatted lines for selected elements when available.
>   selected_text?: string; // Text selected in the window when available.
>   tree: string; // Existing formatted accessibility tree text, including element indexes and tab hierarchy.
> };
>
> type Screenshot = {
>   height?: number; // Screenshot height in logical pixels, when available.
>   id: string; // Stable identifier for this screenshot within the latest window state.
>   originX?: number; // Screen X origin for this bounded screenshot region, when available.
>   originY?: number; // Screen Y origin for this bounded screenshot region, when available.
>   url: string; // Screenshot image as a data URL.
>   width?: number; // Screenshot width in logical pixels, when available.
>   zIndex: number; // Relative z-order for this screenshot; larger values are visually above smaller values.
> };
>
> type MouseButton = "left" | "right" | "middle" | "l" | "r" | "m";
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Navigate to TCP motion interface",
>   "code": "\nawait sky.drag({window:targetWindow, from_x:160, from_y:521, to_x:850, to_y:521, screenshotId:\"screenshot-0\"});\nawait new Promise(r=>setTimeout(r,700));\nvar wsRight = await sky.get_window_state({window:targetWindow, include_text:true});\nnodeRepl.write(JSON.stringify({focused:wsRight.accessibility?.focused_element, shots:wsRight.screenshots?.map(s=>({id:s.id,originX:s.originX,originY:s.originY,width:s.width,height:s.height}))},null,2));\n"
> }
> ```
>
> ```text
> {
>   "focused": "4 창 ID: 59648",
>   "shots": [
>     {
>       "id": "screenshot-0",
>       "originX": -1880,
>       "originY": 41,
>       "width": 1450,
>       "height": 763
>     },
>     {
>       "id": "screenshot-1",
>       "originX": -1875,
>       "originY": 46,
>       "width": 1440,
>       "height": 753
>     }
>   ]
> }
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
>   "title": "View TCP interface network area",
>   "code": "\nawait nodeRepl.emitImage(wsRight.screenshots[0].url);\n"
> }
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
>   "title": "Open Power implementation lookup",
>   "code": "\nawait sky.click({window:targetWindow, x:765, y:465, mouse_button:\"right\", screenshotId:\"screenshot-0\"});\nawait new Promise(r=>setTimeout(r,400));\nvar wsMenuPower = await sky.get_window_state({window:targetWindow, include_text:true});\nnodeRepl.write((wsMenuPower.accessibility?.tree||\"\").slice(-12000));\n"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 Motion_Network Secondary Actions: Raise ID: 65280
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
> 						13 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					14 위치 조정 (disabled)
> 			15 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 				16 창 ID: 59648
> 					17 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
> 						18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							19 단추 위쪽 스크롤 화살표 ID: UpButton
> 							20 단추 페이지 위로 ID: UpPageButton
> 							21 위치 조정 위치 ID: ScrollbarThumb
> 							22 단추 페이지 아래로 ID: DownPageButton
> 							23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							26 위치 조정 위치 ID: ScrollbarThumb
> 							27 단추 페이지 오른쪽으로 ID: DownPageButton
> 							28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						29 위치 조정 (disabled)
> 			30 창 HW_Network Secondary Actions: Raise ID: 65282
> 				31 창 ID: 59648
> 					32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						33 단추 위쪽 스크롤 화살표 ID: UpButton
> 						34 위치 조정 위치 ID: ScrollbarThumb
> 						35 단추 페이지 아래로 ID: DownPageButton
> 						36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						39 위치 조정 위치 ID: ScrollbarThumb
> 						40 단추 페이지 오른쪽으로 ID: DownPageButton
> 						41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					42 위치 조정 (disabled)
> 		43 상태 표시줄 ID: 59393
> 			44 텍스트
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트
> 			49 텍스트 Offline
> 			50 텍스트
> 			51 텍스트 NUM
> 			52 텍스트
> 		53 창 xtpBarTop ID: 59419
> 			54 도구 모음 Script
> 			55 도구 모음 Edit
> 				56 단추 (disabled) Toggle bookmark
> 				57 단추 (disabled) Previous bookmark
> 				58 단추 (disabled) Next bookmark
> 				59 단추 (disabled) Delete all bookmarks
> 				60 단추 (disabled) Previous bookmark in this file
> 				61 단추 (disabled) Next bookmark in this file
> 				62 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				63 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				64 단추 (disabled) Unindent (Shift+Tab)
> 				65 단추 (disabled) Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				67 메뉴 항목 Macros
> 			68 도구 모음 Layout Manager
> 				69 메뉴 항목 Layouts
> 			70 도구 모음 Toolbox
> 				71 단추 DataAnalyzer
> 				72 메뉴 항목 Toolbar Options
> 			73 도구 모음 Net Edit
> 				74 단추 Select
> 				75 메뉴 항목 Toolbar Options
> 			76 도구 모음 Debug
> 				77 단추 Go online (Alt+F6)
> 				78 단추 Change Online Settings
> 				79 메뉴 항목 Online Connection
> 				80 단추 (disabled) Set Online Connection For Current Project
> 				81 단추 (disabled) Download (F6)
> 				82 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				83 단추 (disabled) Download Module on the Fly
> 				84 단추 (disabled) Save Project on PLC
> 				85 단추 (disabled) Start (F7)
> 				86 단추 (disabled) Reset (F8)
> 				87 단추 (disabled) Toggle breakpoint (F4)
> 				88 단추 (disabled) Create condition breakpoint
> 				89 메뉴 항목 Toolbar Options
> 			90 도구 모음 Build
> 				91 메뉴 항목 Target Architecture
> 				92 단추 Build changes (F9)
> 				93 단추 Rebuild project (Strg+F9)
> 				94 단추 (disabled) Cancel building (Ctrl+Break)
> 				95 단추 Link project
> 			96 도구 모음 Standard
> 				97 단추 New project (Strg+N)
> 				98 단추 Open a file (Strg+Shift+O)
> 				99 단추 Close active document (Strg+F4)
> 				100 단추 (disabled) Save file (Strg+S)
> 				101 단추 Open project (Strg+O)
> 				102 단추 (disabled) Save project changes (Strg+Shift+S)
> 				103 단추 Close project
> 				104 단추 Print
> 				105 단추 Cut (Strg+X)
> 				106 단추 Copy (Strg+C)
> 				107 단추 Paste (Strg+V)
> 				108 메뉴 항목 (disabled) Undo (Strg+Z)
> 				109 메뉴 항목 (disabled) Redo (Strg+Y)
> 				110 단추 Navigate Backward (Alt+Left)
> 				111 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				113 메뉴 항목 FILE
> 				114 메뉴 항목 EDIT
> 				115 메뉴 항목 NETEDIT
> 				116 메뉴 항목 VIEW
> 				117 메뉴 항목 PROJECT
> 				118 메뉴 항목 BUILD
> 				119 메뉴 항목 DEBUG
> 				120 메뉴 항목 ANALYZE
> 				121 메뉴 항목 TOOLS
> 				122 메뉴 항목 EXTRAS
> 				123 메뉴 항목 WINDOW
> 				124 메뉴 항목 HELP
> 		125 창 Splitter ID: 311501592
> 		126 창 Splitter ID: 311504784
> 		127 Tab Output ID: 309070896
> 			128 창 ID: 1200
> 				129 창 ID: 1200
> 					130 LIST ID: 1204
> 						131 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							132 단추 위쪽 스크롤 화살표 ID: UpButton
> 							133 단추 페이지 위로 ID: UpPageButton
> 							134 위치 조정 위치 ID: ScrollbarThumb
> 							135 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						136 목록 항목 (selectable)
> 						137 목록 항목 (selectable)
> 						138 목록 항목 (selectable)
> 						139 목록 항목 (selectable)
> 						140 목록 항목 (selectable)
> 						141 목록 항목 (selectable)
> 						142 목록 항목 (selectable)
> 						143 목록 항목 (selectable)
> 						144 목록 항목 (selectable)
> 						145 목록 항목 (selectable)
> 						146 목록 항목 (selectable)
> 						147 목록 항목 (selectable)
> 						148 목록 항목 (selectable)
> 						149 목록 항목 (selectable)
> 						150 목록 항목 (selectable)
> 						151 목록 항목 (selectable)
> 						152 목록 항목 (selectable)
> 						153 목록 항목 (selectable)

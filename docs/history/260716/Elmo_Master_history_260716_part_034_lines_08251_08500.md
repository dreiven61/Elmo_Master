> 			16 메뉴 항목 Graphical User Interface
> 		17 창 작업 영역 ID: 59648
> 			18 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 				19 창 ID: 59648
> 					20 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // Object discovery belongs to CyWork because it performs string operations. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis1) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis1.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := ActiveRequest.CommandId$DINT; AxisRef := ActiveRequest.Reference$DINT; Payload := ActiveRequest.PayloadLength$DINT; CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT ID: 10000
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
> 			33 창 Motion_Network Secondary Actions: Raise ID: 65280
> 				34 창 ID: 59648
> 					35 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						36 단추 위쪽 스크롤 화살표 ID: UpButton
> 						37 위치 조정 위치 ID: ScrollbarThumb
> 						38 단추 페이지 아래로 ID: DownPageButton
> 						39 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					40 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						41 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						42 위치 조정 위치 ID: ScrollbarThumb
> 						43 단추 페이지 오른쪽으로 ID: DownPageButton
> 						44 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					45 위치 조정 (disabled)
> 			46 창 HW_Network Secondary Actions: Raise ID: 65282
> 				47 창 ID: 59648
> 					48 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						49 단추 위쪽 스크롤 화살표 ID: UpButton
> 						50 위치 조정 위치 ID: ScrollbarThumb
> 						51 단추 페이지 아래로 ID: DownPageButton
> 						52 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					53 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						54 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						55 위치 조정 위치 ID: ScrollbarThumb
> 						56 단추 페이지 오른쪽으로 ID: DownPageButton
> 						57 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					58 위치 조정 (disabled)
> 		59 상태 표시줄 ID: 59393
> 			60 텍스트
> 			61 텍스트
> 			62 텍스트
> 			63 텍스트
> 			64 텍스트
> 			65 텍스트 Offline
> 			66 텍스트
> 			67 텍스트 NUM
> 			68 텍스트
> 		69 창 xtpBarTop ID: 59419
> 			70 도구 모음 Script
> 			71 도구 모음 Edit
> 				72 단추 Toggle bookmark
> 				73 단추 (disabled) Previous bookmark
> 				74 단추 (disabled) Next bookmark
> 				75 단추 (disabled) Delete all bookmarks
> 				76 단추 (disabled) Previous bookmark in this file
> 				77 단추 (disabled) Next bookmark in this file
> 				78 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				79 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				80 단추 (disabled) Unindent (Shift+Tab)
> 				81 단추 (disabled) Indent (Tab)
> 			82 도구 모음 Macros Manager
> 				83 메뉴 항목 Macros
> 			84 도구 모음 Layout Manager
> 				85 메뉴 항목 Layouts
> 			86 도구 모음 Toolbox
> 				87 단추 DataAnalyzer
> 				88 메뉴 항목 Toolbar Options
> 			89 도구 모음 Net Edit
> 				90 단추 (disabled) Select
> 				91 메뉴 항목 Toolbar Options
> 			92 도구 모음 Debug
> 				93 단추 Go online (Alt+F6)
> 				94 단추 Change Online Settings
> 				95 메뉴 항목 Online Connection
> 				96 단추 (disabled) Set Online Connection For Current Project
> 				97 단추 (disabled) Download (F6)
> 				98 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				99 단추 (disabled) Download Module on the Fly
> 				100 단추 (disabled) Save Project on PLC
> 				101 단추 (disabled) Start (F7)
> 				102 단추 (disabled) Reset (F8)
> 				103 단추 Toggle breakpoint (F4)
> 				104 단추 Create condition breakpoint
> 				105 메뉴 항목 Toolbar Options
> 			106 도구 모음 Build
> 				107 메뉴 항목 Target Architecture
> 				108 단추 Build changes (F9)
> 				109 단추 Rebuild project (Strg+F9)
> 				110 단추 (disabled) Cancel building (Ctrl+Break)
> 				111 단추 Link project
> 			112 도구 모음 Standard
> 				113 단추 New project (Strg+N)
> 				114 단추 Open a file (Strg+Shift+O)
> 				115 단추 Close active document (Strg+F4)
> 				116 단추 (disabled) Save file (Strg+S)
> 				117 단추 Open project (Strg+O)
> 				118 단추 (disabled) Save project changes (Strg+Shift+S)
> 				119 단추 Close project
> 				120 단추 Print
> 				121 단추 Cut (Strg+X)
> 				122 단추 Copy (Strg+C)
> 				123 단추 Paste (Strg+V)
> 				124 메뉴 항목 (disabled) Undo (Strg+Z)
> 				125 메뉴 항목 (disabled) Redo (Strg+Y)
> 				126 단추 Navigate Backward (Alt+Left)
> 				127 단추 (disabled) Navigate Forward (Alt +Right)
> 			128 메뉴 모음 Menu Bar
> 				129 메뉴 항목 FILE
> 				130 메뉴 항목 EDIT
> 				131 메뉴 항목 VIEW
> 					132 메뉴 View
> 						133 메뉴 항목 Output Pane
> 						134 메뉴 항목 Properties Window
> 						135 메뉴 항목 Debugger Pane
> 						136 메뉴 항목 Python Pane
> 						137 메뉴 항목 DataAnalyzer Pane
> 						138 메뉴 항목 Drive Pane
> 						139 메뉴 항목 Multimaster Pane
> 						140 메뉴 항목 Code Analysis
> 						141 메뉴 항목 Trees
> 						142 메뉴 항목 Toolbars
> 						143 메뉴 항목 Status Bar
> 						144 메뉴 항목 Layout
> 						145 메뉴 항목 Graphical User Interface
> 				146 메뉴 항목 PROJECT
> 				147 메뉴 항목 BUILD
> 				148 메뉴 항목 DEBUG
> 				149 메뉴 항목 ANALYZE
> 				150 메뉴 항목 TOOLS
> 				151 메뉴 항목 EXTRAS
> 				152 메뉴 항목 WINDOW
> 				153 메뉴 항목 HELP
> 		154 창 Splitter ID: 311501592
> 		155 창 Splitter ID: 311504784
> 		156 Tab Output ID: 309070896
> 			157 창 ID: 1200
> 				158 창 ID: 1200
> 					159 LIST ID: 1204
> 						160 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							161 단추 위쪽 스크롤 화살표 ID: UpButton
> 							162 단추 페이지 위로 ID: UpPageButton
> 							163 위치 조정 위치 ID: ScrollbarThumb
> 							164 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						165 목록 항목 (selectable)
> 						166 목록 항목 (selectable)
> 						167 목록 항목 (selectable)
> 						168 목록 항목 (selectable)
> 						169 목록 항목 (selectable)
> 						170 목록 항목 (selectable)
> 						171 목록 항목 (selectable)
> 						172 목록 항목 (selectable)
> 						173 목록 항목 (selectable)
> 						174 목록 항목 (selectable)
> 						175 목록 항목 (selectable)
> 						176 목록 항목 (selectable)
> 						177 목록 항목 (selectable)
> 						178 목록 항목 (selectable)
> 						179 목록 항목 (selectable)
> 						180 목록 항목 (selectable)
> 						181 목록 항목 (selectable)
> 						182 목록 항목 (selectable)
> 						183 목록 항목 (selectable)
> 						184 목록 항목 (selectable)
> 					185 스크롤 막대 ID: 59904
> 						186 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						187 위치 조정 위치 ID: ScrollbarThumb
> 						188 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			189 탭 항목 (selectable) Python Script
> 			190 탭 항목 (selectable) Output
> 			191 탭 항목 (selectable) Debugger
> 			192 단추 Close
> 		193 창 Splitter ID: 311498736
> 		194 Tab Network View ID: 305714776
> 			195 트리 ID: 104
> 				196 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					197 콘솔 트리 (selectable) HW_Network
> 					198 콘솔 트리 (selectable) Motion_Network
> 			199 탭 항목 (selectable) Lib
> 			200 탭 항목 (selectable) File
> 			201 탭 항목 (selectable) Global
> 			202 탭 항목 (selectable) Net
> 			203 탭 항목 (selectable) Class
> 			204 단추 Close
> 		205 Tab Properties ID: 305711584
> 			206 창 ID: 302137496
> 				207 TABLE Properties Window ID: 307037824
> 					208 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						209 단추 위쪽 스크롤 화살표 ID: UpButton
> 						210 위치 조정 위치 ID: ScrollbarThumb
> 						211 단추 페이지 아래로 ID: DownPageButton
> 						212 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					213 custom Name
> 					214 custom Revision
> 					215 custom Type
> 					216 custom Draw Connection
> 					217 custom Comment
> 					218 custom Filename
> 				219 도구 모음 ID: 59392
> 					220 단추
> 					221 단추
> 			222 탭 항목 (selectable) Properties
> 			223 단추 Close
>
> The focused UI element is 131 메뉴 항목 VIEW.
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
>   "title": "Inspect LASAL screenshot regions",
>   "code": "\nnodeRepl.write(JSON.stringify(wsMotion.screenshots, (k,v)=> (typeof v===\"string\" && v.length>200 ? v.slice(0,80)+\"...\" : v), 2).slice(0,12000));\n"
> }
> ```
>
> ```text
> [
>   {
>     "id": "screenshot-0",
>     "zIndex": 0,
>     "url": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKC...",
>     "originX": -1880,
>     "originY": 41,
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "id": "screenshot-1",
>     "zIndex": 1,
>     "url": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKC...",
>     "originX": -1875,
>     "originY": 46,
>     "width": 1440,
>     "height": 753
>   }
> ]
> ```
>

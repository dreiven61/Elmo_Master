>   x?: number; // Window-relative X coordinate.
>   y?: number; // Window-relative Y coordinate.
> };
>
> type PressKeyInput = {
>   key: string; // Key or `+`-separated key chord using X Window System keysym-style names, such as `a`, `space`, `Return`, `Tab`, `Control_L+a`, `Control_L+Shift_L+period`, or `KP_0`; whitespace around `+` is ignored, and common aliases such as `Control`, `Ctrl`, `Alt`, `Shift`, `period`, `greater`, and `Numpad_0` are accepted.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to receive the key press.
> };
>
> type TypeTextInput = {
>   text: string; // Text to type into the current focus.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to type into.
> };
>
> type ScrollInput = {
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   scrollX: number; // Horizontal scroll delta; negative means left, positive means right.
>   scrollY: number; // Vertical scroll delta; negative means up, positive means down.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to scroll.
>   x: number; // Window-relative X coordinate to scroll from.
>   y: number; // Window-relative Y coordinate to scroll from.
> };
>
> type SetValueInput = {
>   element_index: number; // Element index from the latest `get_window_state()` accessibility tree.
>   value: string; // Replacement value for the editable element.
>   window: Window; // Window object from `list_apps()` or `list_windows()` containing the editable element.
> };
>
> type DragInput = {
>   from_x: number; // Starting window-relative X coordinate.
>   from_y: number; // Starting window-relative Y coordinate.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   to_x: number; // Ending window-relative X coordinate.
>   to_y: number; // Ending window-relative Y coordinate.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to drag in.
> };
>
> type PerformSecondaryActionInput = {
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
>   "code": "await sky.click({window:editWindow, element_index:166});\nglobalThis.editState2 = await sky.get_window_state({window:editWindow, include_screenshot:false, include_text:true});\nglobalThis.editWindow=editState2.window;\n{\n const lines=(editState2.accessibility?.tree??\"\").split(\"\\n\");\n nodeRepl.write(JSON.stringify({focus:editState2.accessibility?.focused_element,selected:editState2.accessibility?.selected_elements,candidates:lines.filter(x=>/Class View|Tab Class|탭 항목.*Class|TCPMotion|GroupMove|Find|Implementation/.test(x)).slice(0,180)},null,2));\n}",
>   "title": "Edit 프로젝트 클래스 보기 열기"
> }
> ```
>
> ```text
> call get_window_state before issuing coordinate input
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.activate_window({window:editWindow});\nglobalThis.editWindow=await sky.get_window({id:editWindow.id,app:editWindow.app});\nglobalThis.editState3=await sky.get_window_state({window:editWindow,include_screenshot:true,include_text:true});\nglobalThis.editWindow=editState3.window;\nnodeRepl.write(JSON.stringify({shot:editState3.screenshots.map(s=>({id:s.id,width:s.width,height:s.height})),classLine:(editState3.accessibility?.tree??\"\").split(\"\\n\").find(x=>/탭 항목 \\(selectable\\) Class/.test(x))},null,2));",
>   "title": "Edit 프로젝트 클래스 탭 위치 확인"
> }
> ```
>
> ```text
> {
>   "shot": [
>     {
>       "id": "screenshot-0",
>       "width": 1450,
>       "height": 763
>     },
>     {
>       "id": "screenshot-1",
>       "width": 1440,
>       "height": 753
>     }
>   ],
>   "classLine": "\t\t\t167 탭 항목 (selectable) Class"
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
>   "code": "await sky.click({window:editWindow,element_index:167});\nglobalThis.editState4=await sky.get_window_state({window:editWindow,include_screenshot:false,include_text:true});\nglobalThis.editWindow=editState4.window;\n{\n const lines=(editState4.accessibility?.tree??\"\").split(\"\\n\");\n nodeRepl.write(JSON.stringify({candidates:lines.filter(x=>/Class View|Network View|TCPMotion|GroupMove|LMCPROF|Find|Implementation/.test(x)).slice(0,180),selected:editState4.accessibility?.selected_elements},null,2));\n}",
>   "title": "Edit 프로젝트 클래스 인덱스 확인"
> }
> ```
>
> ```text
> {
>   "candidates": [
>     "\t\t\t16 창 TCPMotionInterface Secondary Actions: Raise ID: 65281",
>     "\t\t\t\t\t18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; CurrentSock := dSock; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if ConnectedClients = 0 then CurrentSock := 0; end_if; end_case; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response VAR_INPUT pData : ^void; udSize : UDINT; dSock : DINT; END_VAR VAR usPayloadLength : UINT; END_VAR if udSize >= 8 then if udSize <= sizeof(ReceiveBuf) then _memset( dest:=#ReceiveBuf, usByte:=0, cntr:=sizeof(ReceiveBuf) ); _StdLib.MemCpy( dest:=#ReceiveBuf[0], source:=pData, size:=udSize ); _StdLib.MemCpy( dest:=#CommandID, source:=#ReceiveBuf[0], size:=2 ); _StdLib.MemCpy( dest:=#usPayloadLength, source:=#ReceiveBuf[4], size:=2 ); _StdLib.MemCpy( dest:=#AxisRef, source:=#ReceiveBuf[6], size:=2 ); if udSize >= usPayloadLength + 8 then MsgPaser(); end_if; end_if; end_if; END_FUNCTION FUNCTION TCPMotionInterface::MsgPaser VAR RawPos : LREAL; RawVelo : LREAL; RawAcc : LREAL; RawDec : LREAL; RawJer : LREAL; END_VAR case CommandID of // GetAxisByName 0x103C: case ReceiveBuf[10]$INT of 0x31: AxisRef := 0; // a01 0x32: AxisRef := 1; // a02 0x33: AxisRef := 2; // a03 0x34: AxisRef := 3; // a04 else AxisRef := 0; end_case; _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 6; // Response payload Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := AxisRef; SendData( pData:=#Sendbuf[0], udSize:=14, dSocket:=CurrentSock, bDirect:=TRUE ); // GetAxisInfo 0x202B: _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 8; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := 0; // Status Sendbuf[14]$UINT := 0; // ErrorId SendData( pData:=#Sendbuf[0], udSize:=16, dSocket:=CurrentSock, bDirect:=TRUE ); // Power On / Off 0x2023: _StdLib.MemCpy( dest:=#bufMode, source:=#ReceiveBuf[8], size:=4 ); Power := ReceiveBuf[12]; Exec := ReceiveBuf[13]; if Power <> 0 then PowerOn(); else PowerOff(); end_if; ID: 10000",
>     "\t\t158 Tab Network View ID: 419923568"
>   ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "{const ls=(editState4.accessibility?.tree??\"\").split(\"\\n\");nodeRepl.write(ls.slice(145,180).join(\"\\n\"));}",
>   "title": "Edit 프로젝트 탭 상태 상세 확인"
> }
> ```
>
> ```text
> 						144 목록 항목 (selectable)
> 						145 목록 항목 (selectable)
> 						146 목록 항목 (selectable)
> 						147 목록 항목 (selectable)
> 						148 목록 항목 (selectable)
> 					149 스크롤 막대 ID: 59904
> 						150 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						151 위치 조정 위치 ID: ScrollbarThumb
> 						152 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			153 탭 항목 (selectable) Python Script
> 			154 탭 항목 (selectable) Output
> 			155 탭 항목 (selectable) Debugger
> 			156 단추 Close
> 		157 창 Splitter ID: 418456656
> 		158 Tab Network View ID: 419923568
> 			159 트리 ID: 104
> 				160 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis_Edit
> 					161 콘솔 트리 (selectable) HW_Network
> 					162 콘솔 트리 (selectable) Motion_Network
> 			163 탭 항목 (selectable) Lib
> 			164 탭 항목 (selectable) File
> 			165 탭 항목 (selectable) Global
> 			166 탭 항목 (selectable) Net
> 			167 탭 항목 (selectable) Class
> 			168 단추 Close
> 		169 Tab Properties ID: 419917640
> 			170 창 ID: 131449744
> 				171 TABLE Properties Window ID: 137620416
> 					172 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						173 단추 위쪽 스크롤 화살표 ID: UpButton
> 						174 위치 조정 위치 ID: ScrollbarThumb
> 						175 단추 페이지 아래로 ID: DownPageButton
> 						176 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					177 custom Name
> 					178 custom GUID
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.editState5=await sky.get_window_state({window:editWindow});\nglobalThis.editWindow=editState5.window;\nnodeRepl.write(JSON.stringify(editState5.screenshots.map(s=>({id:s.id,width:s.width,height:s.height})),null,2));",
>   "title": "Edit 프로젝트 클래스 탭 좌표 확인"
> }
> ```
>
> ```text
> [
>   {
>     "id": "screenshot-0",
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "id": "screenshot-1",
>     "width": 1440,
>     "height": 753
>   }
> ]
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
>   "code": "globalThis.editState6=await sky.get_window_state({window:editWindow,include_screenshot:false,include_text:true});globalThis.editWindow=editState6.window;{const ls=(editState6.accessibility?.tree??\"\").split(\"\\n\");nodeRepl.write(ls.filter(x=>/Class View|Elmo_Ether|External|Sigmatek|TCPMotion|Elmo_[1-4]|탭 항목.*Class/.test(x)).slice(0,120).join(\"\\n\"));}",
>   "title": "Edit 프로젝트 클래스 트리 확인"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis_Edit} - Motion_Network", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis_Edit} - Motion_Network Secondary Actions: Raise
> 			16 창 TCPMotionInterface Secondary Actions: Raise ID: 65281
> 					18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; CurrentSock := dSock; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if ConnectedClients = 0 then CurrentSock := 0; end_if; end_case; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response VAR_INPUT pData : ^void; udSize : UDINT; dSock : DINT; END_VAR VAR usPayloadLength : UINT; END_VAR if udSize >= 8 then if udSize <= sizeof(ReceiveBuf) then _memset( dest:=#ReceiveBuf, usByte:=0, cntr:=sizeof(ReceiveBuf) ); _StdLib.MemCpy( dest:=#ReceiveBuf[0], source:=pData, size:=udSize ); _StdLib.MemCpy( dest:=#CommandID, source:=#ReceiveBuf[0], size:=2 ); _StdLib.MemCpy( dest:=#usPayloadLength, source:=#ReceiveBuf[4], size:=2 ); _StdLib.MemCpy( dest:=#AxisRef, source:=#ReceiveBuf[6], size:=2 ); if udSize >= usPayloadLength + 8 then MsgPaser(); end_if; end_if; end_if; END_FUNCTION FUNCTION TCPMotionInterface::MsgPaser VAR RawPos : LREAL; RawVelo : LREAL; RawAcc : LREAL; RawDec : LREAL; RawJer : LREAL; END_VAR case CommandID of // GetAxisByName 0x103C: case ReceiveBuf[10]$INT of 0x31: AxisRef := 0; // a01 0x32: AxisRef := 1; // a02 0x33: AxisRef := 2; // a03 0x34: AxisRef := 3; // a04 else AxisRef := 0; end_case; _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 6; // Response payload Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := AxisRef; SendData( pData:=#Sendbuf[0], udSize:=14, dSocket:=CurrentSock, bDirect:=TRUE ); // GetAxisInfo 0x202B: _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 8; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := 0; // Status Sendbuf[14]$UINT := 0; // ErrorId SendData( pData:=#Sendbuf[0], udSize:=16, dSocket:=CurrentSock, bDirect:=TRUE ); // Power On / Off 0x2023: _StdLib.MemCpy( dest:=#bufMode, source:=#ReceiveBuf[8], size:=4 ); Power := ReceiveBuf[12]; Exec := ReceiveBuf[13]; if Power <> 0 then PowerOn(); else PowerOff(); end_if; ID: 10000
> 		158 Tab Class View ID: 419923568
> 				160 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis_Edit
> 					161 콘솔 트리 (selectable) External
> 					162 콘솔 트리 (selectable) Sigmatek
> 					164 콘솔 트리 (selectable) Elmo_1
> 					165 콘솔 트리 (selectable) Elmo_2
> 					166 콘솔 트리 (selectable) Elmo_3
> 					167 콘솔 트리 (selectable) Elmo_4
> 					168 콘솔 트리 (selectable) TCPMotionInterface
> 			173 탭 항목 (selectable) Class
> 					188 custom Sigmatek
> ```
>
> MCP tool call

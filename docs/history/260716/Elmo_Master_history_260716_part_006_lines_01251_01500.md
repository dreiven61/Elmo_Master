>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 1,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Screen\\LSE_Modeless.exe",
>     "windows": [],
>     "displayName": "LASAL Screen Modeless",
>     "lastUsedDate": "2026-07-03",
>     "useCount": 1,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-06-23",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\MachineManager\\Bin\\MachineManager.exe",
>     "windows": [],
>     "displayName": "LASAL MachineManager",
>     "lastUsedDate": "2026-06-23",
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
>   "code": "globalThis.targetApp = apps.find(a => /\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2\\.exe$/i.test(a.id));\nglobalThis.baseWindow = await sky.get_window(targetApp.windows.find(w => /\\{Elmo_EtherCAT_Test_4Axis\\}/.test(w.title)));\nglobalThis.editWindow = await sky.get_window(targetApp.windows.find(w => /\\{Elmo_EtherCAT_Test_4Axis_Edit\\}/.test(w.title)));\nglobalThis.baseState = await sky.get_window_state({window: baseWindow, include_screenshot:false, include_text:true});\nglobalThis.baseWindow = baseState.window;\nglobalThis.editState = await sky.get_window_state({window: editWindow, include_screenshot:false, include_text:true});\nglobalThis.editWindow = editState.window;\n{\n const pat=/Find|Implementation|Search|Menu|Motion_Network|TCPMotion|Class|disabled|enabled/i;\n const pick=s=>({title:s.window.title,focus:s.accessibility?.focused_element,selected:s.accessibility?.selected_elements,lines:(s.accessibility?.tree??\"\").split(\"\\n\").filter(x=>pat.test(x)).slice(0,100)});\n nodeRepl.write(JSON.stringify({base:pick(baseState),edit:pick(editState)},null,2));\n}",
>   "title": "두 LASAL 프로젝트 메뉴 상태 비교"
> }
> ```
>
> ```text
> window is minimized; call activate_window, refresh with get_window, then retry get_window_state
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.activate_window({window:baseWindow});\nglobalThis.baseWindow = await sky.get_window({id:baseWindow.id, app:baseWindow.app});\nglobalThis.baseState = await sky.get_window_state({window:baseWindow, include_screenshot:false, include_text:true});\nglobalThis.baseWindow = baseState.window;\n{\n const pat=/Find|Implementation|Search|Menu|Motion_Network|TCPMotion|Class|disabled|enabled/i;\n const s=baseState;\n nodeRepl.write(JSON.stringify({title:s.window.title,focus:s.accessibility?.focused_element,selected:s.accessibility?.selected_elements,lines:(s.accessibility?.tree??\"\").split(\"\\n\").filter(x=>pat.test(x)).slice(0,120)},null,2));\n}",
>   "title": "기준 LASAL 프로젝트 메뉴 확인"
> }
> ```
>
> ```text
> {
>   "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network",
>   "lines": [
>     "Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network\", App: Lasal2.exe.",
>     "\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise",
>     "\t\t\t2 창 Motion_Network Secondary Actions: Raise ID: 65280",
>     "\t\t\t\t\t15 위치 조정 (disabled)",
>     "\t\t\t16 창 TCPMotionInterface* Secondary Actions: Raise ID: 65283",
>     "\t\t\t\t\t18 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // Build the immutable public-name -> opaque reference table from the // connected LASAL objects. The PC API never knows or stores PLC pointers. if ObjectRegistryReady = FALSE then if (IsClientConnected(#LMCAxis) = 1) (IsClientConnected(#LMCAxis2) = 1) & (IsClientConnected(#LMCAxis3) = 1) & (IsClientConnected(#LMCAxis4) = 1) & (IsClientConnected(#LMCRobot) = 1) then _memset(dest:=#AxisObjectName1[0], usByte:=0, cntr:=sizeof(AxisObjectName1)); _memset(dest:=#AxisObjectName2[0], usByte:=0, cntr:=sizeof(AxisObjectName2)); _memset(dest:=#AxisObjectName3[0], usByte:=0, cntr:=sizeof(AxisObjectName3)); _memset(dest:=#AxisObjectName4[0], usByte:=0, cntr:=sizeof(AxisObjectName4)); _memset(dest:=#GroupObjectName[0], usByte:=0, cntr:=sizeof(GroupObjectName)); _GetObjName(pThis:=LMCAxis.pCmd, pName:=#AxisObjectName1[0]); _GetObjName(pThis:=LMCAxis2.pCmd, pName:=#AxisObjectName2[0]); _GetObjName(pThis:=LMCAxis3.pCmd, pName:=#AxisObjectName3[0]); _GetObjName(pThis:=LMCAxis4.pCmd, pName:=#AxisObjectName4[0]); _GetObjName(pThis:=LMCRobot.pCmd, pName:=#GroupObjectName[0]); ObjectRegistryReady := (_strlen(#AxisObjectName1[0]) > 0) & (_strlen(#AxisObjectName1[0]) <= 79) & (_strlen(#AxisObjectName2[0]) > 0) & (_strlen(#AxisObjectName2[0]) <= 79) & (_strlen(#AxisObjectName3[0]) > 0) & (_strlen(#AxisObjectName3[0]) <= 79) & (_strlen(#AxisObjectName4[0]) > 0) & (_strlen(#AxisObjectName4[0]) <= 79) & (_strlen(#GroupObjectName[0]) > 0) & (_strlen(#GroupObjectName[0]) <= 79); end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; CurrentSock := dSock; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; end_if; if RpcSocket = dSock then RpcSocket := 0; RpcInitialized := FALSE; RpcCallbackRegistered := FALSE; RpcCallbackEventMask := 0; RpcCallbackPort := 0; RpcCallbackIPv4[0] := 0; RpcCallbackIPv4[1] := 0; RpcCallbackIPv4[2] := 0; RpcCallbackIPv4[3] := 0; end_if; if ReceiveSocket = dSock then ReceiveFill := 0; ReceiveSocket := 0; _memset(dest:=#ReceiveBuf, usByte:=0, cntr:=sizeof(ReceiveBuf)); end_if; end_case; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response VAR_INPUT pData : ^void; udSize : UDINT; dSock : DINT; END_VAR VAR usPayloadLength : UINT; usReference : UINT; udFrameSize : UDINT; udRemainingSize : UDINT; END_VAR if udSize = 0 then RETURN; end_if; // Once RPC is initialized, only the owner socket may enter the accumulator. if (RpcInitialized = TRUE) & (RpcSocket <> dSock) then _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := -1; SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=dSock, bDirect:=TRUE ); RETURN; end_if; if ReceiveFill = 0 then ReceiveSocket := dSock; _memset(dest:=#ReceiveBuf, usByte:=0, cntr:=sizeof(ReceiveBuf)); elsif ReceiveSocket <> dSock then // Before a session exists, let the latest socket restart the phase-1 // accumulator so an abandoned partial frame cannot block initialization. ReceiveFi ID: 10000",
>     "\t\t\t\t\t\t30 위치 조정 (disabled)",
>     "\t\t\t\t\t\t52 위치 조정 (disabled)",
>     "\t\t\t\t66 단추 (disabled) Toggle bookmark",
>     "\t\t\t\t67 단추 (disabled) Previous bookmark",
>     "\t\t\t\t68 단추 (disabled) Next bookmark",
>     "\t\t\t\t69 단추 (disabled) Delete all bookmarks",
>     "\t\t\t\t70 단추 (disabled) Previous bookmark in this file",
>     "\t\t\t\t71 단추 (disabled) Next bookmark in this file",
>     "\t\t\t\t72 단추 (disabled) Comment selected text (Ctrl+Shift+C)",
>     "\t\t\t\t73 단추 (disabled) Remove comment (Ctrl+Shift+X)",
>     "\t\t\t\t74 단추 (disabled) Unindent (Shift+Tab)",
>     "\t\t\t\t75 단추 (disabled) Indent (Tab)",
>     "\t\t\t\t95 단추 (disabled) Zoom selection",
>     "\t\t\t\t100 단추 (disabled) Set Online Connection For Current Project",
>     "\t\t\t\t101 단추 (disabled) Download (F6)",
>     "\t\t\t\t103 단추 (disabled) Download Module on the Fly",
>     "\t\t\t\t104 단추 (disabled) Save Project on PLC",
>     "\t\t\t\t105 단추 (disabled) Start (F7)",
>     "\t\t\t\t106 단추 (disabled) Reset (F8)",
>     "\t\t\t\t107 단추 (disabled) Toggle breakpoint (F4)",
>     "\t\t\t\t108 단추 (disabled) Create condition breakpoint",
>     "\t\t\t\t111 단추 (disabled) Fix AWL trigger to current instruction",
>     "\t\t\t\t114 단추 (disabled) Goto instruction pointer",
>     "\t\t\t\t115 단추 (disabled) Step into (F5)",
>     "\t\t\t\t116 단추 (disabled) Step over (Alt+F5)",
>     "\t\t\t\t117 단추 (disabled) Step out (Shift+F5)",
>     "\t\t\t\t118 단추 (disabled) Set instruction pointer",
>     "\t\t\t\t123 단추 (disabled) Cancel building (Ctrl+Break)",
>     "\t\t\t\t129 단추 (disabled) Save file (Strg+S)",
>     "\t\t\t\t137 메뉴 항목 (disabled) Undo (Strg+Z)",
>     "\t\t\t\t138 메뉴 항목 (disabled) Redo (Strg+Y)",
>     "\t\t\t\t140 단추 (disabled) Navigate Forward (Alt +Right)",
>     "\t\t\t141 메뉴 모음 Menu Bar",
>     "\t\t\t\t\t183 스크롤 막대 (disabled) ID: 59904",
>     "\t\t192 Tab Class View ID: 138701912",
>     "\t\t\t\t\t208 콘솔 트리 (selectable) TCPMotionInterface",
>     "\t\t\t340 탭 항목 (selectable) Class"
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
>   "code": "await sky.activate_window({window:editWindow});\nglobalThis.editWindow = await sky.get_window({id:editWindow.id, app:editWindow.app});\nglobalThis.editState = await sky.get_window_state({window:editWindow});\nglobalThis.editWindow = editState.window;",
>   "title": "Edit 프로젝트 화면 확인"
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
>   "code": "await sky.activate_window({window:baseWindow});\nglobalThis.baseWindow = await sky.get_window({id:baseWindow.id, app:baseWindow.app});\nglobalThis.baseState = await sky.get_window_state({window:baseWindow});\nglobalThis.baseWindow = baseState.window;",
>   "title": "기준 프로젝트 화면 확인"
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
>   "code": "await sky.activate_window({window:editWindow});\nglobalThis.editWindow = await sky.get_window({id:editWindow.id, app:editWindow.app});\nglobalThis.editState = await sky.get_window_state({window:editWindow, include_screenshot:false, include_text:true});\nglobalThis.editWindow=editState.window;\n{\n const lines=(editState.accessibility?.tree??\"\").split(\"\\n\");\n nodeRepl.write(JSON.stringify({focus:editState.accessibility?.focused_element, selected:editState.accessibility?.selected_elements, candidates:lines.filter(x=>/Tab Class|탭 항목.*Class|Class View|Network View|Find|Implementation|TCPMotionInterface/.test(x)).slice(0,160)},null,2));\n}",
>   "title": "Edit 프로젝트 클래스 보기 상태 확인"
> }
> ```
>
> ```text
> {
>   "focus": "3 창 ID: 59648",
>   "candidates": [
>     "\t\t\t15 창 TCPMotionInterface Secondary Actions: Raise ID: 65281",
>     "\t\t\t\t\t17 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR ReadPos := LMCAxis.ReadPosition(Mode:=LMCAXIS_ACTPOS_APPUNIT); state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; CurrentSock := dSock; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if ConnectedClients = 0 then CurrentSock := 0; end_if; end_case; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response VAR_INPUT pData : ^void; udSize : UDINT; dSock : DINT; END_VAR VAR usPayloadLength : UINT; END_VAR if udSize >= 8 then if udSize <= sizeof(ReceiveBuf) then _memset( dest:=#ReceiveBuf, usByte:=0, cntr:=sizeof(ReceiveBuf) ); _StdLib.MemCpy( dest:=#ReceiveBuf[0], source:=pData, size:=udSize ); _StdLib.MemCpy( dest:=#CommandID, source:=#ReceiveBuf[0], size:=2 ); _StdLib.MemCpy( dest:=#usPayloadLength, source:=#ReceiveBuf[4], size:=2 ); _StdLib.MemCpy( dest:=#AxisRef, source:=#ReceiveBuf[6], size:=2 ); if udSize >= usPayloadLength + 8 then MsgPaser(); end_if; end_if; end_if; END_FUNCTION FUNCTION TCPMotionInterface::MsgPaser VAR RawPos : LREAL; RawVelo : LREAL; RawAcc : LREAL; RawDec : LREAL; RawJer : LREAL; END_VAR case CommandID of // GetAxisByName 0x103C: case ReceiveBuf[10]$INT of 0x31: AxisRef := 0; // a01 0x32: AxisRef := 1; // a02 0x33: AxisRef := 2; // a03 0x34: AxisRef := 3; // a04 else AxisRef := 0; end_case; _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 6; // Response payload Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := AxisRef; SendData( pData:=#Sendbuf[0], udSize:=14, dSocket:=CurrentSock, bDirect:=TRUE ); // GetAxisInfo 0x202B: _memset( dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf) ); Sendbuf[0]$UINT := 0; Sendbuf[2]$UINT := 8; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UDINT := 0; Sendbuf[12]$UINT := 0; // Status Sendbuf[14]$UINT := 0; // ErrorId SendData( pData:=#Sendbuf[0], udSize:=16, dSocket:=CurrentSock, bDirect:=TRUE ); // Power On / Off 0x2023: _StdLib.MemCpy( dest:=#bufMode, source:=#ReceiveBuf[8], size:=4 ); Power := ReceiveBuf[12]; Exec := ReceiveBuf[13]; if Power <> 0 then PowerOn(); else PowerOff(); end_if; ID: 10000",
>     "\t\t157 Tab Network View ID: 419923568",
>     "\t\t\t166 탭 항목 (selectable) Class"
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
>   "code": "var apiDoc = await sky.documentation(\"api\"); nodeRepl.write(apiDoc);",
>   "title": "LASAL UI 조회 방법 확인"
> }
> ```
>
> ````text
> ## API Reference
>
> # Sky Window2 API
>
> ## API Reference
>
> Use this as the supported `sky` window2 API surface.
>
> ```ts
> import { sky } from "@oai/sky";
>
> const apps = await sky.list_apps();
> const candidate_windows = apps.flatMap((app) => app.windows);
> // Choose the task-specific app and window before acting.
> // Each input action takes the specific Window for that action.
>
> interface Window2ComputerUseClient {
>   list_windows(): Promise<Array<Window>>; // List open windows that can be targeted by the window2 API.
>   get_window(input: GetWindowInput): Promise<Window>; // Rehydrate a currently open window by id; useful after losing a window binding.
>   list_apps(): Promise<Array<ListAppsApp>>; // List installed apps, including their currently open targetable windows when present.
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can later be selected from `list_apps()`.
>   get_window_state(input: GetWindowStateInput): Promise<WindowState>; // Capture selected state for an open window.
>   click(input: ClickInput): Promise<void>; // Click either an indexed element from the latest window state or a coordinate in the window.
>   press_key(input: PressKeyInput): Promise<void>; // Press a `+`-separated keyboard chord in a window.
>   type_text(input: TypeTextInput): Promise<void>; // Type text into the current focus in a window.
>   scroll(input: ScrollInput): Promise<void>; // Scroll by a delta from a specific coordinate in the window.
>   set_value(input: SetValueInput): Promise<void>; // Replace the value of an indexed editable element.
>   drag(input: DragInput): Promise<void>; // Drag from one window coordinate to another.
>   perform_secondary_action(input: PerformSecondaryActionInput): Promise<void>; // Invoke a secondary accessibility action on an indexed element.
>   activate_window(input: ActivateWindowInput): Promise<void>; // Optional escape hatch to bring an open window to the foreground; input methods activate their target window automatically.
> }
>
> type Window = {
>   app: AppIdentifier; // App identifier for the app that owns this window; process-backed identifiers may include the full process path.
>   id: number; // Opaque identifier for the open window.
>   title?: string; // User-visible window title when available; may contain PII.
> };
>
> type GetWindowInput = {
>   app?: AppIdentifier; // Optional app identifier to carry forward from a previously returned `Window`.
>   id: number; // Opaque window identifier from a previously returned `Window`.
> };
>
> type ListAppsApp = {
>   displayName?: string; // User-visible app name when available.
>   id: AppIdentifier; // Canonical app id for the app that owns the windows.
>   isRunning?: boolean; // Whether the app currently appears to be running.
>   lastUsedDate?: string; // ISO 8601 timestamp for recent app usage when available.
>   useCount?: number; // Usage count signal when available.
>   windows: Array<Window>; // Open windows owned by this app.
> };
>
> type LaunchAppInput = {
>   app: AppIdentifier; // App id returned by `list_apps()`, or an explicit `.exe` process path/identifier for apps that are not yet discoverable in `list_apps()`.
> };
>
> type GetWindowStateInput = {
>   include_screenshot?: boolean; // Whether to capture and display a screenshot of the window; defaults to true.
>   include_text?: boolean; // Whether to capture accessibility text describing visible elements and indexes; defaults to false.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to capture.
> };
>
> type WindowState = {
>   accessibility: AccessibilityState | null; // Structured accessibility state when requested.
>   screenshots: Array<Screenshot>; // Bounded screenshots captured for the window and related transient UI.
>   window: Window; // Window captured by the state request.
> };
>
> type ClickInput = {
>   click_count?: number; // Number of clicks to perform.
>   element_index?: number; // Element index from the latest `get_window_state()` accessibility tree.
>   mouse_button?: MouseButton; // Mouse button to click.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to click in.

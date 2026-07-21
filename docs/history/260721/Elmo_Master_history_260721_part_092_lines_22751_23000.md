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
>   target: "windows";
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
>   "title": "Inspect LASAL target",
>   "code": "nodeRepl.write(JSON.stringify(targetWindow));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"app":"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","id":395936,"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor"}
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL class tree",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify({a:state.accessibility,s:state.screenshots.map(x=>({id:x.id,width:x.width,height:x.height}))})); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"a":{"tree":"Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor\", App: Lasal2.exe.\n\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise\n\t\t1 창 작업 영역 ID: 59648\n\t\t\t2 창 Hardware Editor Secondary Actions: Raise ID: 65286\n\t\t\t\t3 창 ID: 59648\n\t\t\t\t\t4 창 xtpBarTop ID: 59419\n\t\t\t\t\t\t5 도구 모음 Hardware Editor\n\t\t\t\t\t\t\t6 단추 Hardware Editor Configuration Settings\n\t\t\t\t\t\t\t7 단추 Calculate Resources of Project\n\t\t\t\t\t\t\t8 단추 (disabled) Upload Hardware Tree from PLC\n\t\t\t\t\t\t\t9 단추 (disabled) Compare Hardware Tree of project and PLC\\Reset Online Compare\n\t\t\t\t\t\t\t10 단추 Show Detail Mode\\Show Configuration Mode\n\t\t\t\t\t\t\t11 단추 Generates the ENI File of the current project\n\t\t\t\t\t12 창 ID: 59648\n\t\t\t\t\t\t13 트리 ID: 1\n\t\t\t\t\t\t\t14 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t\t15 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t\t16 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t\t17 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t\t18 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t\t19 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t\t20 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)\n\t\t\t\t\t\t\t\t21 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->\n\t\t\t\t\t\t\t\t22 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->\n\t\t\t\t\t\t\t\t23 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->\n\t\t\t\t\t\t\t\t24 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->\n\t\t\t\t\t\t\t\t25 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->\n\t\t\t\t\t\t\t\t26 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->\n\t\t\t\t\t\t\t\t27 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->\n\t\t\t\t\t\t\t\t28 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->\n\t\t\t\t\t\t\t\t29 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->\n\t\t\t\t\t\t\t\t30 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->\n\t\t\t\t\t\t\t\t31 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->\n\t\t\t\t\t\t\t\t32 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->\n\t\t\t\t\t\t\t\t33 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->\n\t\t\t\t\t\t\t\t34 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->\n\t\t\t\t\t\t\t\t35 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->\n\t\t\t\t\t\t\t\t36 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->\n\t\t\t\t\t\t\t\t37 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->\n\t\t\t\t\t\t\t\t38 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->\n\t\t\t\t\t\t\t\t39 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->\n\t\t\t\t\t\t\t\t40 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)\n\t\t\t\t\t\t\t\t\t41 콘솔 트리 (selectable) Class State (ClassState) <-[]->\n\t\t\t\t\t\t\t\t\t42 콘솔 트리 (selectable) USB:00, Empty\n\t\t\t\t\t\t\t\t\t43 콘솔 트리 (selectable) CAN:00, Empty\n\t\t\t\t\t\t\t\t\t44 콘솔 트리 (selectable) Ethernet:00:0, Empty\n\t\t\t\t\t\t\t\t45 콘솔 트리 (selectable) ALARM:00, Empty\n\t\t\t\t\t\t\t\t46 콘솔 트리 (selectable) SDIAS:00, Empty\n\t\t\t\t\t\t\t\t47 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)\n\t\t\t\t\t\t\t\t\t48 콘솔 트리 (selectable) ---------------------- General -----------------------\n\t\t\t\t\t\t\t\t\t49 콘솔 트리 (selectable) Class State (ClassState) <-[]->\n\t\t\t\t\t\t\t\t\t50 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->\n\t\t\t\t\t\t\t\t\t51 콘솔 트리 (selectable) Slave State (SlaveState) <-[]->\n\t\t\t\t\t\t\t\t\t52 콘솔 트리 (selectable) Device Address (DeviceAdress) <-[]->\n\t\t\t\t\t\t\t\t\t53 콘솔 트리 (selectable) Vendor ID (VendorID) <-[]->\n\t\t\t\t\t\t\t\t\t54 콘솔 트리 (selectable) Product Code (ProductCode) <-[]->\n\t\t\t\t\t\t\t\t\t55 콘솔 트리 (selectable) Revision No (RevisionNo) <-[]->\n\t\t\t\t\t\t\t\t\t56 콘솔 트리 (selectable) Serial No (SerialNo) <-[]->\n\t\t\t\t\t\t\t\t\t57 콘솔 트리 (selectable) Device Name (DeviceName) <-[]->\n\t\t\t\t\t\t\t\t\t58 콘솔 트리 (selectable) Product Revision (ProductRevision) <-[]->\n\t\t\t\t\t\t\t\t\t59 콘솔 트리 (selectable) AL StatusCode (AL_StatusCode) <-[]->\n\t\t\t\t\t\t\t\t\t60 콘솔 트리 (selectable) Online (Online) <-[]-> _LMCAxis1.HWReady\n\t\t\t\t\t\t\t\t\t61 콘솔 트리 (selectable) ---------------------- Axis -----------------------\n\t\t\t\t\t\t\t\t\t62 콘솔 트리 (selectable) Act Position (ActPos) <-[]-> _LMCAxis1.ActPosition\n\t\t\t\t\t\t\t\t\t63 콘솔 트리 (selectable) Set Position (SetPos) <-[]-> PosController1.oOutputSignal\n\t\t\t\t\t\t\t\t\t64 콘솔 트리 (selectable) Act Speed (ActSpeed) <-[]->\n\t\t\t\t\t\t\t\t\t65 콘솔 트리 (selectable) Set Speed (SetSpeed) <-[]->\n\t\t\t\t\t\t\t\t\t66 콘솔 트리 (selectable) Act Torque (ActTorque) <-[]->\n\t\t\t\t\t\t\t\t\t67 콘솔 트리 (selectable) Set Torque (SetTorque) <-[]->\n\t\t\t\t\t\t\t\t\t68 콘솔 트리 (selectable) Act Xw (ActXw) <-[]->\n\t\t\t\t\t\t\t\t\t69 콘솔 트리 (selectable) State Word (StateWord) <-[]->\n\t\t\t\t\t\t\t\t\t70 콘솔 트리 (selectable) Control Word (ControlWord) <-[]->\n\t\t\t\t\t\t\t\t\t71 콘솔 트리 (selectable) Axis Enable (AxEnable) <-[]-> PosController1.DriveEnabled\n\t\t\t\t\t\t\t\t\t72 콘솔 트리 (selectable) Axis Error (AxError) <-[]-> _LMCAxis1.HWError\n\t\t\t\t\t\t\t\t\t73 콘솔 트리 (selectable) Axis Error Quit (AxErrorQuit) <-[]-> PosController1.HWErrorQuit\n\t\t\t\t\t\t\t\t\t74 콘솔 트리 (selectable) Outputs_DigitalOutputs (Outputs_DigitalOutputs) <-[]->\n\t\t\t\t\t\t\t\t\t75 콘솔 트리 (selectable) Inputs_DigitalInputs (Inputs_DigitalInputs) <-[]->\n\t\t\t\t\t\t\t\t\t76 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_2 (Elmo_21)\n\t\t\t\t\t\t\t\t77 콘솔 트리 (selectable) MULTIVARAN:00, Empty\n\t\t\t\t\t\t\t78 콘솔 트리 (selectable) Unplaced Module(s)\n\t\t\t79 창 Motion_Network Secondary Actions: Raise ID: 65284\n\t\t\t\t80 창 ID: 59648\n\t\t\t\t\t81 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t82 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t83 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t84 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t85 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t86 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t87 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t88 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t89 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t90 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t91 위치 조정 (disabled)\n\t\t\t92 창 HW_Network Secondary Actions: Raise ID: 65281\n\t\t\t\t93 창 ID: 59648\n\t\t\t\t\t94 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t95 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t96 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t97 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t98 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t99 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t100 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t101 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t102 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t103 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t104 위치 조정 (disabled)\n\t\t\t105 창 TCPMotionInterface Secondary Actions: Raise ID: 65280\n\t\t\t\t106 창 ID: 59648\n\t\t\t\t\t107 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. currentEpoch := SessionEpoch; // MsgPaser resolves object names on demand in this CyWork context. CyWork // performs no periodic string discovery or retry loop. if (ActiveRequestValid = TRUE) (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : ConnectedClients +=1; if CurrentSock = 0 then CurrentSock := dSock; SessionEpoch += 1; if SessionEpoch = 0 then SessionEpoch := 1; end_if; IngressBlocked := FALSE; IngressFaultPending := FALSE; IngressFaultCloseRequired := FALSE; IngressDiscardRemaining := 0; IngressDiscardSocket := 0; end_if; TCP_SVR_SOCK_INFO_DISCONNECT : ConnectedClients -=1; if ConnectedClients < 0 then ConnectedClients :=0; end_if; if CurrentSock = dSock then CurrentSock := 0; SessionEpoch += 1; if SessionEpoch = 0 ID: 10000\n\t\t\t\t\t\t108 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t109 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t110 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t111 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t112 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t113 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t114 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t115 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t116 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t117 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t118 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t119 위치 조정 (disabled)\n\t\t\t120 창 Comm_Network Secondary Actions: Raise ID: 65283\n\t\t\t\t121 창 ID: 59648\n\t\t\t\t\t122 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t123 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t124 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t125 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t126 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t127 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t128 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t129 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t130 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t131 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t132 위치 조정 (disabled)\n\t\t\t133 창 Elmo_4 Secondary Actions: Raise ID: 65285\n\t\t\t\t134 창 ID: 59648\n\t\t\t\t\t135 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n\t\t\t\t\t\t136 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t137 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t138 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t139 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t140 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t141 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t142 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t143 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t144 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t145 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t146 위치 조정 (disabled)\n\t\t\t147 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282\n\t\t\t\t148 창 ID: 59648\n\t\t\t\t\t149 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000\n\t\t\t\t\t\t150 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t151 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t152 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t153 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t154 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t155 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t156 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t157 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t158 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t159 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t160 위치 조정 (disabled)\n\t\t161 상태 표시줄 ID: 59393\n\t\t\t162 텍스트 \n\t\t\t163 텍스트 \n\t\t\t164 텍스트\n\t\t\t165 텍스트 \n\t\t\t166 텍스트\n\t\t\t167 텍스트 \n\t\t\t168 텍스트\n\t\t\t169 텍스트\n\t\t\t170 텍스트 NUM\n\t\t\t171 텍스트\n\t\t172 창 xtpBarTop ID: 59419\n\t\t\t173 도구 모음 Script\n\t\t\t174 도구 모음 Edit\n\t\t\t\t175 단추 (disabled) Toggle bookmark\n\t\t\t\t176 단추 (disabled) Previous bookmark\n\t\t\t\t177 단추 (disabled) Next bookmark\n\t\t\t\t178 단추 (disabled) Delete all bookmarks\n\t\t\t\t179 단추 (disabled) Previous bookmark in this file\n\t\t\t\t180 단추 (disabled) Next bookmark in this file\n\t\t\t\t181 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n\t\t\t\t182 단추 (disabled) Remove comment (Ctrl+Shift+X)\n\t\t\t\t183 단추 (disabled) Unindent (Shift+Tab)\n\t\t\t\t184 단추 (disabled) Indent (Tab)\n\t\t\t185 도구 모음 Macros Manager\n\t\t\t\t186 메뉴 항목 Macros\n\t\t\t187 도구 모음 Layout Manager\n\t\t\t\t188 메뉴 항목 Layouts\n\t\t\t189 도구 모음 Toolbox\n\t\t\t\t190 단추 DataAnalyzer\n\t\t\t\t191 메뉴 항목 Toolbar Options\n\t\t\t192 도구 모음 Net Edit\n\t\t\t\t193 단추 (disabled) Select\n\t\t\t\t194 메뉴 항목 Toolbar Options\n\t\t\t195 도구 모음 Debug\n\t\t\t\t196 단추 Go online (Alt+F6)\n\t\t\t\t197 단추 Change Online Settings\n\t\t\t\t198 메뉴 항목 Online Connection\n\t\t\t\t199 단추 (disabled) Set Online Connection For Current Project\n\t\t\t\t200 단추 (disabled) Download (F6)\n\t\t\t\t201 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n\t\t\t\t202 단추 (disabled) Download Module on the Fly\n\t\t\t\t203 단추 (disabled) Save Project on PLC\n\t\t\t\t204 단추 (disabled) Start (F7)\n\t\t\t\t205 단추 (disabled) Reset (F8)\n\t\t\t\t206 단추 (disabled) Toggle breakpoint (F4)\n\t\t\t\t207 단추 (disabled) Create condition breakpoint\n\t\t\t\t208 메뉴 항목 Toolbar Options\n\t\t\t209 도구 모음 Build\n\t\t\t\t210 메뉴 항목 Target Architecture\n\t\t\t\t211 단추 Build changes (F9)\n\t\t\t\t212 단추 Rebuild project (Strg+F9)\n\t\t\t\t213 단추 (disabled) Cancel building (Ctrl+Break)\n\t\t\t\t214 단추 Link project\n\t\t\t215 도구 모음 Standard\n\t\t\t\t216 단추 New project (Strg+N)\n\t\t\t\t217 단추 Open a file (Strg+Shift+O)\n\t\t\t\t218 단추 Close active document (Strg+F4)\n\t\t\t\t219 단추 (disabled) Save file (Strg+S)\n\t\t\t\t220 단추 Open project (Strg+O)\n\t\t\t\t221 단추 (disabled) Save project changes (Strg+Shift+S)\n\t\t\t\t222 단추 Close project\n\t\t\t\t223 단추 Print\n\t\t\t\t224 단추 Cut (Strg+X)\n\t\t\t\t225 단추 Copy (Strg+C)\n\t\t\t\t226 단추 Paste (Strg+V)\n\t\t\t\t227 메뉴 항목 Undo (Strg+Z)\n\t\t\t\t228 메뉴 항목 (disabled) Redo (Strg+Y)\n\t\t\t\t229 단추 Navigate Backward (Alt+Left)\n\t\t\t\t230 단추 (disabled) Navigate Forward (Alt +Right)\n\t\t\t231 메뉴 모음 Menu Bar\n\t\t\t\t232 메뉴 항목 FILE\n\t\t\t\t233 메뉴 항목 EDIT\n\t\t\t\t234 메뉴 항목 VIEW\n\t\t\t\t235 메뉴 항목 PROJECT\n\t\t\t\t236 메뉴 항목 BUILD\n\t\t\t\t237 메뉴 항목 DEBUG\n\t\t\t\t238 메뉴 항목 ANALYZE\n\t\t\t\t239 메뉴 항목 TOOLS\n\t\t\t\t240 메뉴 항목 EXTRAS\n\t\t\t\t241 메뉴 항목 WINDOW\n\t\t\t\t242 메뉴 항목 HELP\n\t\t243 창 Splitter ID: 125724648\n\t\t244 창 Splitter ID: 125724480\n\t\t245 Tab Output ID: 295437008\n\t\t\t246 창 ID: 1200\n\t\t\t\t247 창 ID: 1200\n\t\t\t\t\t248 LIST ID: 1201\n\t\t\t\t\t\t249 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t250 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t251 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t252 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t253 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t254 목록 항목 (selectable)\n\t\t\t\t\t\t255 목록 항목 (selectable)\n\t\t\t\t\t\t256 목록 항목 (selectable)\n\t\t\t\t\t\t257 목록 항목 (selectable)\n\t\t\t\t\t\t258 목록 항목 (selectable)\n\t\t\t\t\t259 스크롤 막대 ID: 59904\n\t\t\t\t\t\t260 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t261 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t262 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t263 탭 항목 (selectable) Python Script\n\t\t\t264 탭 항목 (selectable) Output\n\t\t\t265 탭 항목 (selectable) Debugger\n\t\t\t266 단추 Close\n\t\t267 창 Splitter ID: 125724144\n\t\t268 Tab Class View ID: 125483184\n\t\t\t269 트리 ID: 103\n\t\t\t\t270 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t271 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t272 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t273 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t274 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t275 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n\t\t\t\t\t276 콘솔 트리 (selectable) External\n\t\t\t\t\t277 콘솔 트리 (selectable) Sigmatek\n\t\t\t\t\t278 콘솔 트리 (selectable) _TCPIPServer_RT\n\t\t\t\t\t279 콘솔 트리 (selectable) Elmo_1\n\t\t\t\t\t280 콘솔 트리 (selectable) Elmo_2\n\t\t\t\t\t281 콘솔 트리 (selectable) Elmo_3\n\t\t\t\t\t282 콘솔 트리 (selectable) Elmo_4\n\t\t\t\t\t283 콘솔 트리 (selectable) LMCDiagnosticsService\n\t\t\t\t\t\t284 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t285 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t\t286 콘솔 트리 (selectable) InputLatch\n\t\t\t\t\t\t287 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t\t288 콘솔 트리 (selectable) Global\n\t\t\t\t\t\t\t289 콘솔 트리 (selectable) Private\n\t\t\t\t\t290 콘솔 트리 (selectable) LMCEcatInputLatch\n\t\t\t\t\t\t291 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t292 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t\t293 콘솔 트리 (selectable) EcatMaster\n\t\t\t\t\t\t\t294 콘솔 트리 (selectable) Drive1\n\t\t\t\t\t\t\t295 콘솔 트리 (selectable) Drive2\n\t\t\t\t\t\t\t296 콘솔 트리 (selectable) Drive3\n\t\t\t\t\t\t\t297 콘솔 트리 (selectable) Drive4\n\t\t\t\t\t\t298 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t299 콘솔 트리 (selectable) Dependencies\n\t\t\t\t\t300 콘솔 트리 (selectable) TCPMotionInterface\n\t\t\t301 탭 항목 (selectable) Lib\n\t\t\t302 탭 항목 (selectable) File\n\t\t\t303 탭 항목 (selectable) Global\n\t\t\t304 탭 항목 (selectable) Net\n\t\t\t305 탭 항목 (selectable) Class\n\t\t\t306 단추 Close\n\t\t307 Tab Properties ID: 125485008\n\t\t\t308 창 ID: 290002192\n\t\t\t\t309 TABLE Properties Window ID: 293314152\n\t\t\t\t310 도구 모음 ID: 59392\n\t\t\t\t\t311 단추\n\t\t\t\t\t312 단추\n\t\t\t313 탭 항목 (selectable) Properties\n\t\t\t314 단추 Close\n\nThe focused UI element is 269 트리 ID: 103.","focused_element":"269 트리 ID: 103"},"s":[{"id":"screenshot-0","width":1450,"height":763},{"id":"screenshot-1","width":1440,"height":753}]}
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
>   "title": "Expand private methods",
>   "code": "await sky.click({window:targetWindow,element_index:289,click_count:2}); state=null; nodeRepl.write('ok');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> ok
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify private method",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(state.accessibility.tree.slice(state.accessibility.tree.indexOf('LMCDiagnosticsService'),state.accessibility.tree.indexOf('LMCEcatInputLatch'))); await nodeRepl.emitImage(state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> LMCDiagnosticsService
> 						284 콘솔 트리 (selectable) Servers
> 						285 콘솔 트리 (selectable) Clients
> 							286 콘솔 트리 (selectable) InputLatch
> 						287 콘솔 트리 (selectable) Methods
> 							288 콘솔 트리 (selectable) Global
> 							289 콘솔 트리 (selectable) Private
> 								290 콘솔 트리 (selectable) HandleRequest
> 					291 콘솔 트리 (selectable)
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
>   "title": "Remove misplaced method",
>   "code": "await sky.click({window:targetWindow,element_index:290}); state=null; await sky.press_key({window:targetWindow,key:'Delete'}); nodeRepl.write('delete requested');",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> delete requested
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Confirm method removal",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(JSON.stringify(state.accessibility)); await nodeRepl.emitImage(state.screenshots[state.screenshots.length-1].url);",
>   "timeout_ms": 30000
> }
> ```
>
> ```text

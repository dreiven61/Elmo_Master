>
> node_repl.js
>
> ```json
> {
>   "title": "상한 끝 위치 이동",
>   "code": "await sky.press_key({window:targetWindow,key:\"END\"}); state = await sky.get_window_state({window:targetWindow,include_text:true}); for (const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
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
>   "title": "잘못된 끝자리 제거",
>   "code": "await sky.press_key({window:targetWindow,key:\"BACKSPACE\"}); state = await sky.get_window_state({window:targetWindow,include_text:true}); for (const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
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
>   "title": "상한 편집 모드 활성화",
>   "code": "await sky.press_key({window:targetWindow,key:\"F2\"}); state = await sky.get_window_state({window:targetWindow,include_text:true}); for (const s of state.screenshots||[]) await nodeRepl.emitImage(s.url);"
> }
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
>   "title": "입력 동작 확인",
>   "code": "var apidoc2 = await sky.documentation(\"api\"); nodeRepl.write(String(apidoc2).slice(0,20000));"
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
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can be selected from `list_apps()`.
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
>   "title": "속성 입력 요소 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write(JSON.stringify(state.accessibility));"
> }
> ```
>
> ```text
> {"tree":"Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network *\", App: Lasal2.exe.\n\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network * Secondary Actions: Raise\n\t\t1 창 작업 영역 ID: 59648\n\t\t\t44 창 Motion_Network * Secondary Actions: Raise ID: 65284\n\t\t\t\t45 창 ID: 59648\n\t\t\t\t\t166573 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t166574 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t166575 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t166576 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t166577 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t166578 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t166579 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t166580 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t166581 단추 페이지 왼쪽으로 ID: UpPageButton\n\t\t\t\t\t\t166582 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t166583 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t166584 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t166585 위치 조정 (disabled)\n\t\t\t29 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65288\n\t\t\t\t30 창 ID: 59648\n\t\t\t\t\t31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000\n\t\t\t\t\t\t32 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t33 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t34 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t35 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t36 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t37 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t39 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t40 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t41 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t42 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t43 위치 조정 (disabled)\n\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n\t\t\t\t3 창 ID: 59648\n\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000\n\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t15 위치 조정 (disabled)\n\t\t\t16 창 Comm_Network Secondary Actions: Raise ID: 65283\n\t\t\t\t17 창 ID: 59648\n\t\t\t\t\t18 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t19 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t20 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t21 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t22 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t24 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t26 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t52418 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t27 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t28 위치 조정 (disabled)\n\t\t\t58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287\n\t\t\t\t59 창 ID: 59648\n\t\t\t\t\t60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // TW[20] is an encoder error/warning reset, not a generic multi-turn // position reset. Keep it dormant until encoder compatibility and continuous // motor-off ownership are implemented and proven on the target drive. #define LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 #define LMC_DIAG_ADMIN_ERROR_ID -31000 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex ID: 10000\n\t\t\t\t\t\t61 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t62 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t63 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t64 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t65 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t66 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t67 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t68 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t69 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t70 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t71 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t72 위치 조정 (disabled)\n\t\t\t73 창 LMCControlCommandService Secondary Actions: Raise ID: 65286\n\t\t\t\t74 창 ID: 59648\n\t\t\t\t\t75 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_case; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR VAR objectNameLength : UDINT; objectName : ARRAY [0..255] OF CHAR; resolvedReference : UINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C: if ResponseCapacity < 14 then RETURN; end_if; resolvedReference := 0; if RequestFrameSize = 88 then (pRequestFrame + 87)^ := 0; if IsClientConnected(#LMCAxis1) = 1 then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis1.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 1; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis2) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis2.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 2; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis3) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis3.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 3; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis4) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis4.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricm ID: 10000\n\t\t\t\t\t\t76 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t77 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t78 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t79 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t80 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t81 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t82 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t83 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t84 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t85 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t86 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t87 위치 조정 (disabled)\n\t\t\t88 창 Comm_Network.lcn Secondary Actions: Raise ID: 65282\n\t\t\t\t89 창 ID: 59648\n\t\t\t\t\t90 창 <?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?> <Network Name=\"Comm_Network\" Revision=\"0.0\"> <!-- List of Components in this network --> <Components> <Object Name = \"LMCControlCommandService1\" GUID = \"{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}\" Class = \"LMCControlCommandService\" Position = \"(930,1380)\" Visualized = \"false\"> <Channels> <Server Name=\"ClassSvr\"/> <Client Name=\"LMCAxis1\"/> <Client Name=\"LMCAxis2\"/> <Client Name=\"LMCAxis3\"/> <Client Name=\"LMCAxis4\"/> <Client Name=\"LMCAxis5\"/> <Client Name=\"LMCAxis6\"/> <Client Name=\"LMCAxis7\"/> <Client Name=\"LMCAxis8\"/> <Client Name=\"LMCAxis9\"/> <Client Name=\"LMCRobot\"/> </Channels> </Object> <Object Name = \"LMCDiagnosticsService1\" GUID = \"{F42F0DD4-D9CC-4E5B-B073-F88FACAD14A8}\" Class = \"LMCDiagnosticsService\" Position = \"(870,900)\" Visualized = \"false\"> <Channels> <Server Name=\"ClassSvr\"/> <Server Name=\"DiagnosticsBootCounter\" Value=\"0\"/> <Client Name=\"InputLatch\"/> <Client Name=\"RecorderStore\"/> <Client Name=\"SdoAxis1\"/> <Client Name=\"SdoAxis2\"/> <Client Name=\"SdoAxis3\"/> <Client Name=\"SdoAxis4\"/> </Channels> </Object> <Object Name = \"TCPIPServer1\" GUID = \"{42E82217-EDCD-47A0-BF97-FCBD9C009436}\" Class = \"TCPIPServer\" Position = \"(870,180)\" Visualized = \"true\" Remotely = \"true\" CyclicTime = \"1 ms\" BackgroundTime = \"always\"> <Channels> <Server Name=\"Control\"/> <Server Name=\"ErrorCode\"/> <Server Name=\"SemaName01\"/> <Server Name=\"SemaName02\"/> <Server Name=\"TaskName\"/> <Client Name=\"_MultiTask\"/> <Client Name=\"_StdLib\"/> <Client Name=\"_TaskObjectControl\"/> <Client Name=\"Config\" Value=\"0\"/> <Client Name=\"ConnectionsPerRun\" Value=\"1\"/> <Client Name=\"MaxConnections\" Value=\"2\"/> <Client Name=\"Port\" Value=\"4000\"/> <Client Name=\"SigCLib\"/> <Client Name=\"SizeOfTXBuffer\"/> <Client Name=\"Timeout\"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name=\"_base\" GUID=\"{BEB0D8C1-05A6-452D-879B-F50A84747DCB}\" Class=\"_TCPIPServer\"> <Channels> <Server Name=\"Control\"/> <Server Name=\"ErrorCode\"/> <Server Name=\"SemaName01\"/> <Server Name=\"SemaName02\"/> <Server Name=\"TaskName\"/> <Client Name=\"_MultiTask\"/> <Client Name=\"_StdLib\"/> <Client Name=\"_TaskObjectControl\"/> <Client Name=\"Config\"/> <Client Name=\"ConnectionsPerRun\" Value=\"1\"/> <Client Name=\"MaxConnections\" Value=\"2\"/> <Client Name=\"Port\"/> <Client Name=\"SigCLib\"/> <Client Name=\"SizeOfTXBuffer\"/> <Client Name=\"Timeout\"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name=\"CheckSum\" GUID=\"{924983ED-FE4B-4B5A-BC71-6E6963A07A78}\" Class=\"_CheckSum\"> <Channels> <Server Name=\"ClassSvr\"/> </Channels> </Object> <Object Name=\"StrSemaName01\" GUID=\"{299AFE23-53C0-4268-B520-661EA498CF23}\" Class=\"String\"> <Channels> <Server Name=\"Data\"/> <Client Name=\"SingleRealloc\" Value=\"1\"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name=\"_base\" GUID=\"{B9D2D328-1416-469A-BE13-9F6CBBB1958D}\" Class=\"StringInternal\"> <Channels> <Server Name=\"Data\"/> <Client Name=\"DataBuffer\"/> <Client Name=\"SingleRealloc\"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> ID: 10000\n\t\t\t\t\t\t91 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t92 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t93 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t94 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t95 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t96 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t97 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t98 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t99 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t100 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t101 위치 조정 (disabled)\n\t\t\t102 창 EtherCAT_Network Secondary Actions: Raise ID: 65281\n\t\t\t\t103 창 ID: 59648\n\t\t\t\t\t104 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t105 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t106 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t107 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t108 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t110 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t111 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t112 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t113 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t114 위치 조정 (disabled)\n\t\t\t115 창 LMCSdoExecutor Secondary Actions: Raise ID: 65280\n\t\t\t\t116 창 ID: 59648\n\t\t\t\t\t117 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000\n\t\t\t\t\t\t118 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t119 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t120 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t121 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t122 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t123 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t124 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t125 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t126 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t127 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t128 위치 조정 (disabled)\n\t\t129 상태 표시줄 ID: 59393\n\t\t\t130 텍스트 \n\t\t\t131 텍스트\n\t\t\t132 텍스트\n\t\t\t133 텍스트\n\t\t\t134 텍스트 \n\t\t\t135 텍스트 Offline\n\t\t\t136 텍스트\n\t\t\t137 텍스트 NUM\n\t\t\t138 텍스트\n\t\t139 창 xtpBarTop ID: 59419\n\t\t\t140 도구 모음 Edit\n\t\t\t\t185944 단추 (disabled) Toggle bookmark\n\t\t\t\t185945 단추 (disabled) Previous bookmark\n\t\t\t\t185946 단추 (disabled) Next bookmark\n\t\t\t\t185947 단추 (disabled) Delete all bookmarks\n\t\t\t\t185948 단추 (disabled) Previous bookmark in this file\n\t\t\t\t185949 단추 (disabled) Next bookmark in this file\n\t\t\t\t185950 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n\t\t\t\t185951 단추 (disabled) Remove comment (Ctrl+Shift+X)\n\t\t\t\t185952 단추 (disabled) Unindent (Shift+Tab)\n\t\t\t\t185953 단추 (disabled) Indent (Tab)\n\t\t\t151 도구 모음 Macros Manager\n\t\t\t\t185954 메뉴 항목 Macros\n\t\t\t153 도구 모음 Layout Manager\n\t\t\t\t185955 메뉴 항목 Layouts\n\t\t\t155 도구 모음 Toolbox\n\t\t\t\t185956 단추 DataAnalyzer\n\t\t\t\t185957 메뉴 항목 Toolbar Options\n\t\t\t158 도구 모음 Net Edit\n\t\t\t\t185958 단추 Select\n\t\t\t\t185959 메뉴 항목 Toolbar Options\n\t\t\t161 도구 모음 Debug\n\t\t\t\t185960 단추 Go online (Alt+F6)\n\t\t\t\t185961 단추 Change Online Settings\n\t\t\t\t185962 메뉴 항목 Online Connection\n\t\t\t\t185963 단추 (disabled) Set Online Connection For Current Project\n\t\t\t\t185964 단추 (disabled) Download (F6)\n\t\t\t\t185965 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n\t\t\t\t185966 단추 (disabled) Download Module on the Fly\n\t\t\t\t185967 단추 (disabled) Save Project on PLC\n\t\t\t\t185968 단추 (disabled) Start (F7)\n\t\t\t\t185969 단추 (disabled) Reset (F8)\n\t\t\t\t185970 단추 (disabled) Toggle breakpoint (F4)\n\t\t\t\t185971 단추 (disabled) Create condition breakpoint\n\t\t\t\t185972 메뉴 항목 Toolbar Options\n\t\t\t175 도구 모음 Build\n\t\t\t\t185973 메뉴 항목 Target Architecture\n\t\t\t\t185974 단추 Build changes (F9)\n\t\t\t\t185975 단추 Rebuild project (Strg+F9)\n\t\t\t\t185976 단추 (disabled) Cancel building (Ctrl+Break)\n\t\t\t\t185977 단추 Link project\n\t\t\t181 도구 모음 Standard\n\t\t\t\t185978 단추 New project (Strg+N)\n\t\t\t\t185979 단추 Open a file (Strg+Shift+O)\n\t\t\t\t185980 단추 Close active document (Strg+F4)\n\t\t\t\t185981 단추 Save file (Strg+S)\n\t\t\t\t185982 단추 Open project (Strg+O)\n\t\t\t\t185983 단추 Save project changes (Strg+Shift+S)\n\t\t\t\t185984 단추 Close project\n\t\t\t\t185985 단추 Print\n\t\t\t\t185986 단추 Cut (Strg+X)\n\t\t\t\t185987 단추 Copy (Strg+C)\n\t\t\t\t185988 단추 Paste (Strg+V)\n\t\t\t\t185989 메뉴 항목 Undo (Strg+Z)\n\t\t\t\t185990 메뉴 항목 (disabled) Redo (Strg+Y)\n\t\t\t\t185991 단추 Navigate Backward (Alt+Left)\n\t\t\t\t185992 단추 (disabled) Navigate Forward (Alt +Right)\n\t\t\t197 메뉴 모음 Menu Bar\n\t\t\t\t185993 메뉴 항목 FILE\n\t\t\t\t185994 메뉴 항목 EDIT\n\t\t\t\t185995 메뉴 항목 NETEDIT\n\t\t\t\t185996 메뉴 항목 VIEW\n\t\t\t\t185997 메뉴 항목 PROJECT\n\t\t\t\t185998 메뉴 항목 BUILD\n\t\t\t\t185999 메뉴 항목 DEBUG\n\t\t\t\t186000 메뉴 항목 ANALYZE\n\t\t\t\t186001 메뉴 항목 TOOLS\n\t\t\t\t186002 메뉴 항목 EXTRAS\n\t\t\t\t186003 메뉴 항목 WINDOW\n\t\t\t\t186004 메뉴 항목 HELP\n\t\t69246 창 Splitter ID: 133722488\n\t\t233 창 Splitter ID: 617298272\n\t\t234 Tab Class View ID: 279804736\n\t\t\t235 트리 ID: 103\n\t\t\t\t236 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t237 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t238 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t239 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t240 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t241 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t242 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n\t\t\t\t\t243 콘솔 트리 (selectable) External\n\t\t\t\t\t244 콘솔 트리 (selectable) Sigmatek\n\t\t\t\t\t245 콘솔 트리 (selectable) Elmo_1\n\t\t\t\t\t246 콘솔 트리 (selectable) Elmo_2\n\t\t\t\t\t247 콘솔 트리 (selectable) Elmo_3\n\t\t\t\t\t248 콘솔 트리 (selectable) Elmo_4\n\t\t\t\t\t249 콘솔 트리 (selectable) GL_9086_1\n\t\t\t\t\t250 콘솔 트리 (selectable) GL_9086_1_Slot00\n\t\t\t\t\t251 콘솔 트리 (selectable) GL_9086_1_Slot01\n\t\t\t\t\t252 콘솔 트리 (selectable) LMCControlCommandService\n\t\t\t\t\t\t253 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t254 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t\t40663 콘솔 트리 (selectable) LMCAxis1\n\t\t\t\t\t\t\t40664 콘솔 트리 (selectable) LMCAxis2\n\t\t\t\t\t\t\t40665 콘솔 트리 (selectable) LMCAxis3\n\t\t\t\t\t\t\t40666 콘솔 트리 (selectable) LMCAxis4\n\t\t\t\t\t\t\t40667 콘솔 트리 (selectable) LMCAxis5\n\t\t\t\t\t\t\t40668 콘솔 트리 (selectable) LMCAxis6\n\t\t\t\t\t\t\t40669 콘솔 트리 (selectable) LMCAxis7\n\t\t\t\t\t\t\t40670 콘솔 트리 (selectable) LMCAxis8\n\t\t\t\t\t\t\t40671 콘솔 트리 (selectable) LMCAxis9\n\t\t\t\t\t\t\t40672 콘솔 트리 (selectable) LMCRobot\n\t\t\t\t\t\t255 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t\t256 콘솔 트리 (selectable) Global\n\t\t\t\t\t\t\t\t257 콘솔 트리 (selectable) HandleRequest\n\t\t\t\t\t\t\t\t258 콘솔 트리 (selectable) ProcessAxisReference\n\t\t\t\t\t\t\t259 콘솔 트리 (selectable) Private\n\t\t\t\t\t\t260 콘솔 트리 (selectable) Variables\n\t\t\t\t\t\t\t261 콘솔 트리 (selectable) GroupMovePos\n\t\t\t\t\t\t\t262 콘솔 트리 (selectable) GroupKinematicReady\n\t\t\t\t\t\t\t263 콘솔 트리 (selectable) ReferenceState\n\t\t\t\t\t\t\t\t264 콘솔 트리 (selectable) 0..18\n\t\t\t\t\t\t265 콘솔 트리 (selectable) Objects\n\t\t\t\t\t\t266 콘솔 트리 (selectable) Dependencies\n\t\t\t\t\t267 콘솔 트리 (selectable) LMCDiagnosticsService\n\t\t\t\t\t268 콘솔 트리 (selectable) LMCEcatInputLatch\n\t\t\t\t\t\t269 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t270 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t\t271 콘솔 트리 (selectable) EcatMaster\n\t\t\t\t\t\t\t272 콘솔 트리 (selectable) Drive1\n\t\t\t\t\t\t\t273 콘솔 트리 (selectable) Drive2\n\t\t\t\t\t\t\t274 콘솔 트리 (selectable) Drive3\n\t\t\t\t\t\t\t275 콘솔 트리 (selectable) Drive4\n\t\t\t\t\t\t\t276 콘솔 트리 (selectable) RecorderStore\n\t\t\t\t\t\t\t277 콘솔 트리 (selectable) Coupler\n\t\t\t\t\t\t\t278 콘솔 트리 (selectable) InputSlot\n\t\t\t\t\t\t\t279 콘솔 트리 (selectable) OutputSlot\n\t\t\t\t\t\t\t31799 콘솔 트리 (selectable) LMCAxis1\n\t\t\t\t\t\t\t52482 콘솔 트리 (selectable) LMCAxis2\n\t\t\t\t\t\t\t54785 콘솔 트리 (selectable) LMCAxis3\n\t\t\t\t\t\t\t57247 콘솔 트리 (selectable) LMCAxis4\n\t\t\t\t\t\t280 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t281 콘솔 트리 (selectable) Variables\n\t\t\t\t\t\t\t176377 콘솔 트리 (selectable) PublishSequence\n\t\t\t\t\t\t\t176378 콘솔 트리 (selectable) SnapshotBytes\n\t\t\t\t\t\t\t176379 콘솔 트리 (selectable) OutputRevision\n\t\t\t\t\t\t\t176380 콘솔 트리 (selectable) OutputObserved\n\t\t\t\t\t\t\t176381 콘솔 트리 (selectable) OutputPreviousValid\n\t\t\t\t\t\t\t176382 콘솔 트리 (selectable) OutputPreviousValue\n\t\t\t\t\t\t\t176383 콘솔 트리 (selectable) Ds402HomeRequestSequence\n\t\t\t\t\t\t\t176384 콘솔 트리 (selectable) Ds402HomeAppliedSequence\n\t\t\t\t\t\t\t176385 콘솔 트리 (selectable) Ds402HomeMailbox\n\t\t\t\t\t\t\t176655 콘솔 트리 (selectable) Ds402HomeAlignmentState\n\t\t\t\t\t\t\t\t178468 콘솔 트리 (selectable) 0..71\n\t\t\t\t\t\t282 콘솔 트리 (selectable) Objects\n\t\t\t\t\t\t283 콘솔 트리 (selectable) Dependencies\n\t\t\t\t\t284 콘솔 트리 (selectable) LMCRecorderStore\n\t\t\t\t\t285 콘솔 트리 (selectable) LMCSdoExecutor\n\t\t\t\t\t286 콘솔 트리 (selectable) TCPIPServer\n\t\t\t\t\t287 콘솔 트리 (selectable) TCPMotionInterface\n\t\t\t\t\t\t288 콘솔 트리 (selectable) Base\n\t\t\t\t\t\t289 콘솔 트리 (selectable) Servers\n\t\t\t\t\t\t290 콘솔 트리 (selectable) Clients\n\t\t\t\t\t\t\t291 콘솔 트리 (selectable) _StdLib\n\t\t\t\t\t\t\t292 콘솔 트리 (selectable) Diagnostics\n\t\t\t\t\t\t\t293 콘솔 트리 (selectable) ControlCommands\n\t\t\t\t\t\t294 콘솔 트리 (selectable) Methods\n\t\t\t\t\t\t295 콘솔 트리 (selectable) Variables\n\t\t\t\t\t\t296 콘솔 트리 (selectable) Types\n\t\t\t\t\t\t297 콘솔 트리 (selectable) Network\n\t\t\t\t\t\t298 콘솔 트리 (selectable) Objects\n\t\t\t\t\t\t299 콘솔 트리 (selectable) Dependencies\n\t\t\t186005 탭 항목 (selectable) Lib\n\t\t\t186006 탭 항목 (selectable) File\n\t\t\t186007 탭 항목 (selectable) Class\n\t\t\t186008 탭 항목 (selectable) Global\n\t\t\t186009 탭 항목 (selectable) Net\n\t\t\t305 단추 Close\n\t\t306 Tab Properties ID: 279798808\n\t\t\t307 창 ID: 270081768\n\t\t\t\t308 TABLE Properties Window ID: 277338752\n\t\t\t\t\t183240 편집 ID: 1100837040\n\t\t\t\t\t186010 custom Limit Low\n\t\t\t\t\t186011 custom Limit Low value\n\t\t\t\t\t186012 custom Limit High\n\t\t\t\t\t186013 custom Limit High value\n\t\t\t\t317 도구 모음 ID: 59392\n\t\t\t\t\t318 단추\n\t\t\t\t\t319 단추\n\t\t\t186014 탭 항목 (selectable) Properties\n\t\t\t321 단추 Close\n\nThe focused UI element is 183240 편집 ID: 1100837040.","focused_element":"183240 편집 ID: 1100837040"}
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
>   "title": "배열 상한 정확히 설정",
>   "code": "await sky.set_value({window:targetWindow,element_index:183240,value:\"7\"}); state = await sky.get_window_state({window:targetWindow,include_text:true}); for (const s of state.screenshots||[]) await nodeRepl.emitImage(s.url); nodeRepl.write(state.accessibility?.tree?.includes(\"0..7\") ? \"range=0..7\" : \"range-not-yet-visible\");"
> }
> ```
>
> ```text
> read UIA value read-only state: 요청한 속성이 CacheRequest에 없습니다. (0x80070057)
> ```
>
> MCP tool call

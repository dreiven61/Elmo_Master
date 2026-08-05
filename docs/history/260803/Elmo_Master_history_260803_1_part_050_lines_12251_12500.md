> 			178 탭 항목 (selectable) Properties
> 			179 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000.
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
>   "code": "var cuaApi = await sky.documentation(\"api\");\nnodeRepl.write(cuaApi);",
>   "title": "Read Windows automation API"
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
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"Reobserve before class open\");\n  globalThis.state = null;\n  await sky.click({ window: observation.window, element_index: 161, click_count: 2 });\n  globalThis.state = await sky.get_window_state({ window: observation.window, include_screenshot: true, include_text: true });\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "title": "Open input latch class"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65282
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex >= 0x8000 then physicalAxis := (CatalogIndex shr 8) and 0x007F; topologyCount := CatalogIndex and 0x00FF; if (topologyCount = 0) | (physicalAxis >= 7) | ((physicalAxis + topologyCount) > 7) then RETURN; end_if; (pEntry + 16)^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 20)^$UINT := phy ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 위치 조정 위치 ID: ScrollbarThumb
> 							8 단추 페이지 아래로 ID: DownPageButton
> 							9 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							12 위치 조정 위치 ID: ScrollbarThumb
> 							13 단추 페이지 오른쪽으로 ID: DownPageButton
> 							14 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						15 위치 조정 (disabled)
> 			16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281
> 				17 창 ID: 59648
> 					18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The production executor cannot be started through the manual channel. result := ParaReadWrite; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaType::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR result := ParaType; END_FUNCTION FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaString::Write VAR_INPUT input (EAX) : UDINT; END_VAR VAR_OUTPUT result (EAX) : UDINT; END_VAR result := ParaString; END_FUNCTION FUNCTION GLOBAL LMCSdoExecutor::TryStartRead VAR_INPUT OperationToken : UDINT; ObjectIndex : UINT; SubIndex : USINT; ReadLength : UINT; TimeoutMs : UDINT; END_VAR VAR_OUTPUT ret_code : iprStates; END_VAR VAR previousState : UDINT; startResult : iprStates; END_VAR ret_code := ERROR; if (sizeof(LMCSdoExecutorResult) <> 32) | (OperationToken = 0) | ((ReadLength <> 1) (ReadLength <> 2) & (ReadLength <> 4)) | (TimeoutMs = 0) | (TimeoutMs > 60000) then RETURN; end_if; previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_IDLE, newVal:=LMC_SDO_EXEC_ARMING); if previousState <> LMC_SDO_EXEC_IDLE then ret_code := BUSY; RETURN; end_if; ActiveToken := OperationToken; ActiveIndex := ObjectIndex; ActiveSubIndex := SubIndex; ActiveLength := ReadLength; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); if IsClientConnected(#toSlave) = FALSE then previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RELEASING); if previousState = LMC_SDO_EXEC_ARMING then ActiveToken := 0; ActiveIndex := 0; ActiveSubIndex := 0; ActiveLength := 0; ActiveIsWrite := FALSE; _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer)); _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer)); _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult)); previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_RELEASING, newVal:=LMC_SDO_EXEC_IDLE); if previousState <> LMC_SDO_EXEC_RELEASING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; else sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); end_if; RETURN; end_if; // Publish RUNNING before the vendor request becomes visible. A short SDO // can otherwise callback while the adapter is still ARMING. previousState := sigclib_atomic_cmpxchgU32( pValue:=#AdapterState, cmpVal:=LMC_SDO_EXEC_ARMING, newVal:=LMC_SDO_EXEC_RUNNING); if previousState <> LMC_SDO_EXEC_ARMING then sigclib_atomic_setU32( pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED); RETURN; end_if; startResult := toSlave.StartReadSDO( ObjectIndex$HINT, SubIndex$HSINT, 0, (#ReadBuffer[0])$^USINT, TO_UDINT(ReadLength), TimeoutMs, THIS); ret_code := startResult; if startResult <> READY then // BUSY and ERROR do not own the vendor buffer. Reserve a private // releasing state while clearing the request before exposing Idle. previousState := sigclib_atomic_cmpxchgU32( pValue: ID: 10000
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
> 			30 창 TCPMotionInterface Secondary Actions: Raise ID: 65280
> 				31 창 ID: 59648
> 					32 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 		44 상태 표시줄 ID: 59393
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트 Ln 13 Col 50
> 			49 텍스트
> 			50 텍스트 Offline
> 			51 텍스트
> 			52 텍스트 NUM
> 			53 텍스트
> 		54 창 xtpBarTop ID: 59419
> 			55 도구 모음 Edit
> 				2664 단추 Toggle bookmark
> 				2665 단추 (disabled) Previous bookmark
> 				2666 단추 (disabled) Next bookmark
> 				2667 단추 (disabled) Delete all bookmarks
> 				2668 단추 (disabled) Previous bookmark in this file
> 				2669 단추 (disabled) Next bookmark in this file
> 				2670 단추 Comment selected text (Ctrl+Shift+C)
> 				2671 단추 Remove comment (Ctrl+Shift+X)
> 				2672 단추 Unindent (Shift+Tab)
> 				2673 단추 Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				2674 메뉴 항목 Macros

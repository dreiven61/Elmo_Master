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
> 감사 결과, 실제 전송 기능 자체는 살아 있습니다. Motion/Power와 승인된 SDO Write는 활성 상태이고, 지금 막힌 것은 새 기능 두 군데입니다: DS402 Home의 LASAL 좌표 정렬이 의도적으로 실패 처리되어 있고, TW[20]은 SDO가 끝날 때까지 Motor Off를 보장하지 못합니다. 우선 DS402 Home 정렬용 축 클라이언트와 연결을 IDE에서 추가하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCEcatInputLatch Clients 메뉴 열기",
>   "code": "{ const observation = globalThis.state; if (!observation?.accessibility) throw new Error('Reobserve required'); globalThis.state=null; await sky.click({window:observation.window, element_index:270, mouse_button:'right'}); globalThis.state=await sky.get_window_state({window:observation.window, include_screenshot:true, include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||'')); }"
> }
> ```
>
> ```text
> element 270 has no cached bounds
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMCEcatInputLatch 트리 다시 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow, include_screenshot:true, include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285
> 				3 창 ID: 59648
> 					4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
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
> 			16 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				17 창 ID: 59648
> 					18 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						19 단추 위쪽 스크롤 화살표 ID: UpButton
> 						20 위치 조정 위치 ID: ScrollbarThumb
> 						21 단추 페이지 아래로 ID: DownPageButton
> 						22 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						24 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						25 단추 페이지 왼쪽으로 ID: UpPageButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					28 위치 조정 (disabled)
> 			29 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65288
> 				30 창 ID: 59648
> 					31 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; END_VAR // The legacy recorder prefix remains 304 bytes. The coherent topology and // I/O reader uses the extended 464-byte scalar snapshot. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStat ID: 10000
> 						32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							33 단추 위쪽 스크롤 화살표 ID: UpButton
> 							34 단추 페이지 위로 ID: UpPageButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 			44 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				45 창 ID: 59648
> 					46 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						47 단추 위쪽 스크롤 화살표 ID: UpButton
> 						48 단추 페이지 위로 ID: UpPageButton
> 						49 위치 조정 위치 ID: ScrollbarThumb
> 						50 단추 페이지 아래로 ID: DownPageButton
> 						51 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					52 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						53 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						54 단추 페이지 왼쪽으로 ID: UpPageButton
> 						55 위치 조정 위치 ID: ScrollbarThumb
> 						56 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					57 위치 조정 (disabled)
> 			58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287
> 				59 창 ID: 59648
> 					60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // TW[20] is an encoder error/warning reset, not a generic multi-turn // position reset. Keep it dormant until encoder compatibility and continuous // motor-off ownership are implemented and proven on the target drive. #define LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED FALSE #define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED FALSE #define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE 0x00010002 #define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK 0xFFFFFFFF #define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES 1000 #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 #define LMC_DIAG_ADMIN_ERROR_ID -31000 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; topologyCount : UINT; pTopologyEntry : ^USINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if pEntry = NIL then RETURN; end_if; if CatalogIndex = 0x0200 then _memset(dest:=pEntry, usByte:=0, cntr:=28); pEntry^$UDINT := LMC_DIAG_TOPOLOGY_REVISION; (pEntry + 4)^$UINT := 7; (pEntry + 6)^$UINT := 96; (pEntry + 8)^$UINT := 1; (pEntry + 10)^$UINT := 5; (pEntry + 12)^$UINT := 2; (pEntry + 14)^$UINT := 4; (pEntry + 16)^$UDINT := 0x0000000F; (pEntry + 20)^$UDINT := 1; Result := 0; RETURN; end_if; // Reuse this private serializer for the seven fixed topology records. The // 0x0100 discriminator keeps the existing D1 catalog index ABI unchanged. if ((CatalogIndex >= 0x0100) (CatalogIndex <= 0x0106)) | (CatalogIndex >= 0x8000) then if CatalogIndex ID: 10000
> 						61 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							62 단추 위쪽 스크롤 화살표 ID: UpButton
> 							63 단추 페이지 위로 ID: UpPageButton
> 							64 위치 조정 위치 ID: ScrollbarThumb
> 							65 단추 페이지 아래로 ID: DownPageButton
> 							66 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						67 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							68 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							69 위치 조정 위치 ID: ScrollbarThumb
> 							70 단추 페이지 오른쪽으로 ID: DownPageButton
> 							71 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						72 위치 조정 (disabled)
> 			73 창 LMCControlCommandService Secondary Actions: Raise ID: 65286
> 				74 창 ID: 59648
> 					75 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_case; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR VAR objectNameLength : UDINT; objectName : ARRAY [0..255] OF CHAR; resolvedReference : UINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; case CommandId of 0x103C: if ResponseCapacity < 14 then RETURN; end_if; resolvedReference := 0; if RequestFrameSize = 88 then (pRequestFrame + 87)^ := 0; if IsClientConnected(#LMCAxis1) = 1 then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis1.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 1; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis2) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis2.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 2; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis3) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis3.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricmp(str1:=(pRequestFrame + 8)$^CHAR, str2:=#objectName[0]) = 0 then resolvedReference := 3; end_if; end_if; end_if; if (resolvedReference = 0) & (IsClientConnected(#LMCAxis4) = 1) then _memset(dest:=#objectName[0], usByte:=0, cntr:=sizeof(objectName)); objectNameLength := _GetObjName( pThis:=LMCAxis4.pCmd, pName:=#objectName[0]); if (objectNameLength > 0) & (objectNameLength <= 79) then if _stricm ID: 10000
> 						76 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							77 단추 위쪽 스크롤 화살표 ID: UpButton
> 							78 단추 페이지 위로 ID: UpPageButton
> 							79 위치 조정 위치 ID: ScrollbarThumb
> 							80 단추 페이지 아래로 ID: DownPageButton
> 							81 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						82 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							83 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							84 위치 조정 위치 ID: ScrollbarThumb
> 							85 단추 페이지 오른쪽으로 ID: DownPageButton
> 							86 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						87 위치 조정 (disabled)
> 			88 창 Comm_Network.lcn Secondary Actions: Raise ID: 65282
> 				89 창 ID: 59648
> 					90 창 <?xml version="1.0" encoding="ISO-8859-1" ?> <Network Name="Comm_Network" Revision="0.0"> <!-- List of Components in this network --> <Components> <Object Name = "LMCControlCommandService1" GUID = "{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}" Class = "LMCControlCommandService" Position = "(930,1380)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Client Name="LMCAxis1"/> <Client Name="LMCAxis2"/> <Client Name="LMCAxis3"/> <Client Name="LMCAxis4"/> <Client Name="LMCAxis5"/> <Client Name="LMCAxis6"/> <Client Name="LMCAxis7"/> <Client Name="LMCAxis8"/> <Client Name="LMCAxis9"/> <Client Name="LMCRobot"/> </Channels> </Object> <Object Name = "LMCDiagnosticsService1" GUID = "{F42F0DD4-D9CC-4E5B-B073-F88FACAD14A8}" Class = "LMCDiagnosticsService" Position = "(870,900)" Visualized = "false"> <Channels> <Server Name="ClassSvr"/> <Server Name="DiagnosticsBootCounter" Value="0"/> <Client Name="InputLatch"/> <Client Name="RecorderStore"/> <Client Name="SdoAxis1"/> <Client Name="SdoAxis2"/> <Client Name="SdoAxis3"/> <Client Name="SdoAxis4"/> </Channels> </Object> <Object Name = "TCPIPServer1" GUID = "{42E82217-EDCD-47A0-BF97-FCBD9C009436}" Class = "TCPIPServer" Position = "(870,180)" Visualized = "true" Remotely = "true" CyclicTime = "1 ms" BackgroundTime = "always"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config" Value="0"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port" Value="4000"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{BEB0D8C1-05A6-452D-879B-F50A84747DCB}" Class="_TCPIPServer"> <Channels> <Server Name="Control"/> <Server Name="ErrorCode"/> <Server Name="SemaName01"/> <Server Name="SemaName02"/> <Server Name="TaskName"/> <Client Name="_MultiTask"/> <Client Name="_StdLib"/> <Client Name="_TaskObjectControl"/> <Client Name="Config"/> <Client Name="ConnectionsPerRun" Value="1"/> <Client Name="MaxConnections" Value="2"/> <Client Name="Port"/> <Client Name="SigCLib"/> <Client Name="SizeOfTXBuffer"/> <Client Name="Timeout"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="CheckSum" GUID="{924983ED-FE4B-4B5A-BC71-6E6963A07A78}" Class="_CheckSum"> <Channels> <Server Name="ClassSvr"/> </Channels> </Object> <Object Name="StrSemaName01" GUID="{299AFE23-53C0-4268-B520-661EA498CF23}" Class="String"> <Channels> <Server Name="Data"/> <Client Name="SingleRealloc" Value="1"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> <!-- List of Components in this network --> <Components> <Object Name="_base" GUID="{B9D2D328-1416-469A-BE13-9F6CBBB1958D}" Class="StringInternal"> <Channels> <Server Name="Data"/> <Client Name="DataBuffer"/> <Client Name="SingleRealloc"/> </Channels> <!-- List of Networks in this component --> <Networks> <Network> ID: 10000
> 						91 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							92 단추 위쪽 스크롤 화살표 ID: UpButton
> 							93 위치 조정 위치 ID: ScrollbarThumb
> 							94 단추 페이지 아래로 ID: DownPageButton
> 							95 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						96 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							97 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							98 위치 조정 위치 ID: ScrollbarThumb
> 							99 단추 페이지 오른쪽으로 ID: DownPageButton
> 							100 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						101 위치 조정 (disabled)
> 			102 창 EtherCAT_Network Secondary Actions: Raise ID: 65281
> 				103 창 ID: 59648
> 					104 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						105 단추 위쪽 스크롤 화살표 ID: UpButton
> 						106 위치 조정 위치 ID: ScrollbarThumb
> 						107 단추 페이지 아래로 ID: DownPageButton
> 						108 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					109 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						110 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						111 위치 조정 위치 ID: ScrollbarThumb
> 						112 단추 페이지 오른쪽으로 ID: DownPageButton
> 						113 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					114 위치 조정 (disabled)
> 			115 창 LMCSdoExecutor Secondary Actions: Raise ID: 65280

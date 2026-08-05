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
>   "title": "LASAL 상태 새로 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write(\"fresh\");"
> }
> ```
>
> ```text
> fresh
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
>   "title": "LASAL 변수 메뉴 열기",
>   "code": "await sky.click({window:targetWindow, x:1120, y:364, mouse_button:\"right\"}); nodeRepl.write(\"context opened\");"
> }
> ```
>
> ```text
> context opened
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 변수 메뉴 확인",
>   "code": "state = await sky.get_window_state({window:targetWindow, include_screenshot:true, include_text:true}); nodeRepl.write(state.accessibility?.tree||\"\");"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise
> 		26404 창
> 		26405 창
> 		26406 메뉴
> 			26817 메뉴 항목 Delete Variable Del
> 			26818 메뉴 항목 Move Up
> 			26819 메뉴 항목 (disabled) Move Down
> 			26820 메뉴 항목 New Dimension
> 			26821 메뉴 항목 (disabled) Go to Datatype
> 			26822 메뉴 항목 Add Server for Visu
> 			26823 메뉴 항목 Copy
> 		1 창 작업 영역 ID: 59648
> 			1224 창 LMCControlCommandService* Secondary Actions: Raise ID: 65284
> 				1225 창 ID: 59648
> 					1226 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE #define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9 #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_PHASE_RESERVED 1 #define LMC_OWNER_PHASE_ACTIVE 2 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F #define LMC_OWNER_STARTUP_STATE_MAGIC 0x4F575350 #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_REQUIRED 0x0000001F #define LMC_OWNER_STARTUP_DIAG_REQUIRED 0x0000001F #define LMC_OWNER_STARTUP_STABLE_SAMPLES 3 #define LMC_OWNER_STARTUP_STABLE_MS 100 #define LMC_OWNER_STARTUP_AXIS_CLEAR_MASK 0x05028890 #define LMC_OWNER_STARTUP_AXIS_LOCK_MASK 0x01000800 #define LMC_OWNER_PROFILE_AXIS_MASK 0x0000000F #define LMC_OWNER_ROBOT_AXIS_MASK 0x000001FF #define LMC_OWNER_OBSERVER_STRIDE 12 #define LMC_OWNER_OBSERVER_MAGIC 0x4F425300 #define LMC_OWNER_OBSERVER_ACTIVITY_SEEN 0x00000001 #define LMC_OWNER_OBSERVER_BASELINE_VALID 0x00000002 #define LMC_OWNER_OBSERVER_RESTORE_GROUP_LEASE 0x00000004 #define LMC_OWNER_OBSERVER_PREEMPTED 0x00000008 #define LMC_OWNER_OBSERVER_PREEMPTED_SPECIAL 0x00000010 #define LMC_OWNER_OBSERVER_FORCE_QUARANTINE 0x00000020 #define LMC_OWNER_OBSERVER_RETURN_GROUP_LEASE 0x00000040 #define LMC_OWNER_OBSERVER_GROUP_ACTIVE_PREEMPTED 0x00000080 #define LMC_OWNER_OBSERVER_EVIDENCE_CLEAR_MASK 0xFFFFFFFC #define LMC_OWNER_ORDINARY_STABLE_SAMPLES 3 #define LMC_OWNER_ORDINARY_STABLE_MS 100 #define LMC_OWNER_ORDINARY_TIMEOUT_MS 120000 #define LMC_OWNER_AXIS_POWER_ON_MASK 0x00000001 #define LMC_OWNER_AXIS_IN_POSITION_MASK 0x00000004 #define LMC_OWNER_AXIS_EMERGENCY_MASK 0x00000200 #define LMC_OWNER_UNSUPPORTED_AXIS_MASK 0x000001F0 #define LMC_OWNER_LATCH_PHYSICAL_FLAG 0x00000001 #define LMC_OWNER_DS402_OPERATION_ENABLED_MASK 0x00000004 #define LMC_OWNER_DS402_FAULT_MASK 0x00000008 #define LMC_OWNER_DS402_STATE_MASK 0x0000006F #define LMC_OWNER_DS402_OPERATION_ENABLED_STATE 0x00000027 #define LMC_OWNER_DS402_TARGET_REACHED_MASK 0x00000400 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : U ID: 10000
> 						1227 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							1228 단추 위쪽 스크롤 화살표 ID: UpButton
> 							1229 단추 페이지 위로 ID: UpPageButton
> 							1230 위치 조정 위치 ID: ScrollbarThumb
> 							1231 단추 페이지 아래로 ID: DownPageButton
> 							1232 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						1233 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							1234 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							1235 위치 조정 위치 ID: ScrollbarThumb
> 							1236 단추 페이지 오른쪽으로 ID: DownPageButton
> 							1237 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						1238 위치 조정 (disabled)
> 			1239 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				1240 창 ID: 59648
> 					1241 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						1242 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							1243 단추 위쪽 스크롤 화살표 ID: UpButton
> 							1244 단추 페이지 위로 ID: UpPageButton
> 							1245 위치 조정 위치 ID: ScrollbarThumb
> 							1246 단추 페이지 아래로 ID: DownPageButton
> 							1247 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						1248 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							1249 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							1250 위치 조정 위치 ID: ScrollbarThumb
> 							1251 단추 페이지 오른쪽으로 ID: DownPageButton
> 							1252 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						1253 위치 조정 (disabled)
> 			1254 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65282
> 				1255 창 ID: 59648
> 					1256 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_ZERO_HOME_STATE_RUNNING 1 #define LMC_ZERO_HOME_STATE_SUCCEEDED 2 #define LMC_ZERO_HOME_STATE_FAILED 3 #define LMC_ZERO_HOME_PHASE_VERIFY 1 #define LMC_ZERO_HOME_REQUIRED_STABLE 3 #define LMC_ZERO_HOME_FAILURE_INVALID -1 #define LMC_ZERO_HOME_FAILURE_BUSY -2 #define LMC_ZERO_HOME_FAILURE_CLIENT -3 #define LMC_ZERO_HOME_FAILURE_STATE -4 #define LMC_ZERO_HOME_FAILURE_STALE -5 #define LMC_ZERO_HOME_FAILURE_NATIVE -6 #define LMC_ZERO_HOME_FAILURE_VERIFY -7 #define LMC_ZERO_HOME_FAILURE_CORRUPT -8 #define LMC_ZERO_HOME_FAILURE_DS402 -9 #define LMC_ZERO_HOME_STANDSTILL 0x02000000 #define LMC_ZERO_HOME_EVIDENCE_EXPECTED 0x00000001 #define LMC_ZERO_HOME_EVIDENCE_STATE 0x00000002 #define LMC_ZERO_HOME_EVIDENCE_RAW 0x00000004 #define LMC_ZERO_HOME_EVIDENCE_APP 0x00000008 #define LMC_ZERO_HOME_EVIDENCE_INTERNAL 0x00000010 #define LMC_ZERO_HOME_EVIDENCE_STABLE 0x00000020 #define LMC_ZERO_HOME_EVIDENCE_DISPATCH 0x00000003 #define LMC_ZERO_HOME_EVIDENCE_VERIFIED 0x0000001F #define LMC_ZERO_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_PHYSICAL 0x00000001 #define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002 #define LMC_OWNER_STARTUP_LATCH_DS402 0x00000004 #define LMC_OWNER_STARTUP_LATCH_OWNER 0x00000008 #define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010 FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; zeroHomeRequestSequence : UDINT; zeroHomeAppliedSequence : UDINT; zeroHomeRequestToken : UDINT; zeroHomeRequestAxis : DINT; zeroHomeExpectedActualPosition : DINT; zeroHomePhase : DINT; zeroHomeStableSampleCount : DINT; zeroHomeFailure : DINT; zeroHomeCycle : UDINT; zeroHomeEvidence : UDINT; zeroHomeHasRequest : BOOL; zeroHomeTerminal : BOOL; zeroHomeSucceeded : BOOL; zeroHomeDriveConnected : BOOL; zeroHomeAxisConnected : BOOL; zeroHomeAxisStatus : _LMCAXIS_STATUS; zeroHomeAxisError : _LMCAXIS_ERROR; zeroHomeRawDrivePosition : DINT; zeroHomeActualAppPosition : DINT; zeroHomeSetAppPosition : DINT; zeroHomeActualIntPositio ID: 10000
> 						1257 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							1258 단추 위쪽 스크롤 화살표 ID: UpButton
> 							1259 단추 페이지 위로 ID: UpPageButton
> 							1260 위치 조정 위치 ID: ScrollbarThumb
> 							1261 단추 페이지 아래로 ID: DownPageButton
> 							1262 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						1263 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							1264 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							1265 위치 조정 위치 ID: ScrollbarThumb
> 							1266 단추 페이지 오른쪽으로 ID: DownPageButton
> 							1267 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						1268 위치 조정 (disabled)
> 			1269 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				1270 창 ID: 59648
> 					1271 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						1272 단추 위쪽 스크롤 화살표 ID: UpButton
> 						1273 위치 조정 위치 ID: ScrollbarThumb
> 						1274 단추 페이지 아래로 ID: DownPageButton
> 						1275 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					1276 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						1277 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						1278 위치 조정 위치 ID: ScrollbarThumb
> 						1279 단추 페이지 오른쪽으로 ID: DownPageButton
> 						1280 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					1281 위치 조정 (disabled)
> 			1282 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				1283 창 ID: 59648
> 					1284 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						1285 단추 위쪽 스크롤 화살표 ID: UpButton
> 						1286 위치 조정 위치 ID: ScrollbarThumb
> 						1287 단추 페이지 아래로 ID: DownPageButton
> 						1288 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					1289 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						1290 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						1291 위치 조정 위치 ID: ScrollbarThumb
> 						1292 단추 페이지 오른쪽으로 ID: DownPageButton
> 						1293 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					1294 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Script
> 			14 도구 모음 Edit
> 				26824 단추 Toggle bookmark
> 				26825 단추 (disabled) Previous bookmark
> 				26826 단추 (disabled) Next bookmark
> 				26827 단추 (disabled) Delete all bookmarks
> 				26828 단추 (disabled) Previous bookmark in this file
> 				26829 단추 (disabled) Next bookmark in this file
> 				26830 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				26831 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				26832 단추 (disabled) Unindent (Shift+Tab)
> 				26833 단추 (disabled) Indent (Tab)
> 			25 도구 모음 Macros Manager
> 				26834 메뉴 항목 Macros
> 			27 도구 모음 Layout Manager
> 				26835 메뉴 항목 Layouts
> 			29 도구 모음 Toolbox
> 				26836 단추 DataAnalyzer
> 				26837 메뉴 항목 Toolbar Options
> 			32 도구 모음 Net Edit
> 				26838 단추 (disabled) Select
> 				26839 메뉴 항목 Toolbar Options
> 			35 도구 모음 Debug
> 				26840 단추 Go online (Alt+F6)
> 				26841 단추 Change Online Settings
> 				26842 메뉴 항목 Online Connection
> 				26843 단추 (disabled) Set Online Connection For Current Project
> 				26844 단추 (disabled) Download (F6)
> 				26845 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				26846 단추 (disabled) Download Module on the Fly
> 				26847 단추 (disabled) Save Project on PLC
> 				26848 단추 (disabled) Start (F7)
> 				26849 단추 (disabled) Reset (F8)
> 				26850 단추 Toggle breakpoint (F4)
> 				26851 단추 Create condition breakpoint
> 				26852 메뉴 항목 Toolbar Options
> 			49 도구 모음 Build
> 				26853 메뉴 항목 Target Architecture
> 				26854 단추 Build changes (F9)
> 				26855 단추 Rebuild project (Strg+F9)
> 				26856 단추 (disabled) Cancel building (Ctrl+Break)
> 				26857 단추 Link project
> 			55 도구 모음 Standard
> 				26858 단추 New project (Strg+N)
> 				26859 단추 Open a file (Strg+Shift+O)
> 				26860 단추 Close active document (Strg+F4)
> 				26861 단추 Save file (Strg+S)
> 				26862 단추 Open project (Strg+O)
> 				26863 단추 Save project changes (Strg+Shift+S)
> 				26864 단추 Close project
> 				26865 단추 Print

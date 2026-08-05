> 					1202 콘솔 트리 (selectable) External
> 					1203 콘솔 트리 (selectable) Sigmatek
> 					1204 콘솔 트리 (selectable) Elmo_1
> 					1205 콘솔 트리 (selectable) Elmo_2
> 					1206 콘솔 트리 (selectable) Elmo_3
> 					1207 콘솔 트리 (selectable) Elmo_4
> 					1208 콘솔 트리 (selectable) GL_9086_1
> 					1209 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					1210 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					1211 콘솔 트리 (selectable) LMCControlCommandService
> 						8366 콘솔 트리 (selectable) Servers
> 						8367 콘솔 트리 (selectable) Clients
> 						8368 콘솔 트리 (selectable) Methods
> 						8369 콘솔 트리 (selectable) Variables
> 							9044 콘솔 트리 (selectable) GroupMovePos
> 							9045 콘솔 트리 (selectable) GroupKinematicReady
> 							9046 콘솔 트리 (selectable) ZeroHomeState
> 							9047 콘솔 트리 (selectable) OwnershipState
> 							9048 콘솔 트리 (selectable) OwnershipStartupState
> 							9049 콘솔 트리 (selectable) OwnershipObserverState
> 						8370 콘솔 트리 (selectable) Objects
> 						8371 콘솔 트리 (selectable) Dependencies
> 					1212 콘솔 트리 (selectable) LMCDiagnosticsService
> 					1213 콘솔 트리 (selectable) LMCEcatInputLatch
> 					1214 콘솔 트리 (selectable) LMCRecorderStore
> 					1215 콘솔 트리 (selectable) LMCSdoExecutor
> 					1216 콘솔 트리 (selectable) TCPIPServer
> 					1217 콘솔 트리 (selectable) TCPMotionInterface
> 			10189 탭 항목 (selectable) Lib
> 			10190 탭 항목 (selectable) File
> 			10191 탭 항목 (selectable) Global
> 			10192 탭 항목 (selectable) Net
> 			10193 탭 항목 (selectable) Class
> 			106 단추 Close
> 		107 Tab Properties ID: 385795760
> 			108 창 ID: 262717224
> 				109 TABLE Properties Window ID: 272020368
> 				110 도구 모음 ID: 59392
> 					111 단추
> 					112 단추
> 			10194 탭 항목 (selectable) Properties
> 			114 단추 Close
>
> The focused UI element is 100 트리 ID: 103.
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
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||''))",
>   "title": "새 변수 이름 입력 위치 확인"
> }
> ```
>
> ```text
> 10258 편집 ID: 1
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
>   "code": "{const observation=globalThis.state;if(observation?.accessibility?.focused_element==null)throw new Error('no focus');globalThis.state=null;await sky.type_text({window:observation.window,text:'OwnershipLeaseState'});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||''))}",
>   "title": "ownership lease 변수 이름 입력"
> }
> ```
>
> ```text
> 10258 편집 ID: 1
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
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:'Return'});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||''))}",
>   "title": "ownership lease 변수 이름 확정"
> }
> ```
>
> ```text
> 10258 편집 ID: 1
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
>   "code": "{const observation=globalThis.state;const sid=observation?.screenshots?.[0]?.id;if(!sid)throw new Error('no screenshot');globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1260,y:527,click_count:2});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.focused_element||''))}",
>   "title": "ownership lease 변수 타입 편집"
> }
> ```
>
> ```text
> 100 트리 ID: 103
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
>   "code": "{const observation=globalThis.state;const sid=observation?.screenshots?.[0]?.id;if(!sid)throw new Error('no screenshot');globalThis.state=null;await sky.click({window:observation.window,screenshotId:sid,x:1031,y:271});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))}",
>   "title": "ownership lease 배열 타입 선택"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService* Secondary Actions: Raise
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
> 				11819 단추 Toggle bookmark
> 				11820 단추 (disabled) Previous bookmark
> 				11821 단추 (disabled) Next bookmark
> 				11822 단추 (disabled) Delete all bookmarks
> 				11823 단추 (disabled) Previous bookmark in this file
> 				11824 단추 (disabled) Next bookmark in this file
> 				11825 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				11826 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				11827 단추 (disabled) Unindent (Shift+Tab)
> 				11828 단추 (disabled) Indent (Tab)
> 			25 도구 모음 Macros Manager
> 				11829 메뉴 항목 Macros
> 			27 도구 모음 Layout Manager
> 				11830 메뉴 항목 Layouts
> 			29 도구 모음 Toolbox
> 				11831 단추 DataAnalyzer
> 				11832 메뉴 항목 Toolbar Options
> 			32 도구 모음 Net Edit
> 				11833 단추 (disabled) Select
> 				11834 메뉴 항목 Toolbar Options
> 			35 도구 모음 Debug

> 			20263 탭 항목 (selectable) Python Script
> 			20264 탭 항목 (selectable) Debugger
> 			20265 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 271013656
> 		82 Tab Class View ID: 274742248
> 			83 트리 ID: 103
> 				11396 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					11397 단추 위쪽 스크롤 화살표 ID: UpButton
> 					11398 위치 조정 위치 ID: ScrollbarThumb
> 					11399 단추 페이지 아래로 ID: DownPageButton
> 					11400 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				11401 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					11402 콘솔 트리 (selectable) External
> 					11403 콘솔 트리 (selectable) Sigmatek
> 					11404 콘솔 트리 (selectable) Elmo_1
> 					11405 콘솔 트리 (selectable) Elmo_2
> 					11406 콘솔 트리 (selectable) Elmo_3
> 					11407 콘솔 트리 (selectable) Elmo_4
> 					11408 콘솔 트리 (selectable) GL_9086_1
> 					11409 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					11410 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					11411 콘솔 트리 (selectable) LMCControlCommandService
> 					11412 콘솔 트리 (selectable) LMCDiagnosticsService
> 					11413 콘솔 트리 (selectable) LMCEcatInputLatch
> 					11414 콘솔 트리 (selectable) LMCRecorderStore
> 					11415 콘솔 트리 (selectable) LMCSdoExecutor
> 					11416 콘솔 트리 (selectable) TCPIPServer
> 					11417 콘솔 트리 (selectable) TCPMotionInterface
> 			20266 탭 항목 (selectable) Lib
> 			20267 탭 항목 (selectable) File
> 			20268 탭 항목 (selectable) Class
> 			20269 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274740424
> 			90 창 ID: 260670512
> 				91 TABLE Properties Window ID: 272204320
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			20270 탭 항목 (selectable) Properties
> 			96 단추 Close
> 		20271 메뉴 Edit
> 			20272 메뉴 항목 (disabled) Undo Ctrl+Z
> 			20273 메뉴 항목 (disabled) Redo Ctrl+Y
> 			20274 메뉴 항목 Cut Ctrl+X
> 			20275 메뉴 항목 Copy Ctrl+C
> 			20276 메뉴 항목 Paste Ctrl+V
> 			20277 메뉴 항목 Find... Ctrl+F
> 			20278 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			20279 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			20280 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			20281 메뉴 항목 Replace... Ctrl+R
> 			20282 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			20283 메뉴 항목 Goto Line... Ctrl+G
> 			20284 메뉴 항목 Reset Editor Colors
> 			20285 메뉴 항목 Mark
> 			20286 메뉴 항목 Bookmark
> 			20287 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			20288 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			20289 메뉴 항목 Select All Ctrl+A
> 			20290 메뉴 항목 Comment
> 			20291 메뉴 항목 Dent
>
> The focused UI element is 72 LIST ID: 1201.
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\n{\n  var observationDismissMenu = globalThis.state;\n  globalThis.state = null;\n  try {\n    await sky.press_key({window: observationDismissMenu.window, key: \"Escape\"});\n    globalThis.state = await sky.get_window_state({window: observationDismissMenu.window, include_screenshot: true, include_text: false});\n  } catch (error) {\n    nodeRepl.write(\"MENU_DISMISS_ERROR: \"+String(error));\n  }\n  if (globalThis.state) globalThis.targetWindow = state.window;\n}\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 검색 메뉴 닫기"
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
>   "code": "\n{\n  var observationFindFiles = globalThis.state;\n  globalThis.state = null;\n  try {\n    await sky.press_key({window: observationFindFiles.window, key: \"Control_L+Shift_L+f\"});\n    globalThis.state = await sky.get_window_state({window: observationFindFiles.window, include_screenshot: true, include_text: true});\n  } catch (error) {\n    nodeRepl.write(\"FIND_FILES_OPEN_ERROR: \"+String(error));\n  }\n  if (globalThis.state) {\n    globalThis.targetWindow = state.window;\n    nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n  }\n}\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 구현 검색 열기"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			11423 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				11424 창 ID: 59648
> 					11425 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						11426 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							11427 단추 위쪽 스크롤 화살표 ID: UpButton
> 							11428 단추 페이지 위로 ID: UpPageButton
> 							11429 위치 조정 위치 ID: ScrollbarThumb
> 							11430 단추 페이지 아래로 ID: DownPageButton
> 							11431 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11432 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11433 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							11434 위치 조정 위치 ID: ScrollbarThumb
> 							11435 단추 페이지 오른쪽으로 ID: DownPageButton
> 							11436 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						11437 위치 조정 (disabled)
> 			11438 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65282
> 				11439 창 ID: 59648
> 					11440 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_ZERO_HOME_STATE_RUNNING 1 #define LMC_ZERO_HOME_STATE_SUCCEEDED 2 #define LMC_ZERO_HOME_STATE_FAILED 3 #define LMC_ZERO_HOME_PHASE_VERIFY 1 #define LMC_ZERO_HOME_REQUIRED_STABLE 3 #define LMC_ZERO_HOME_FAILURE_INVALID -1 #define LMC_ZERO_HOME_FAILURE_BUSY -2 #define LMC_ZERO_HOME_FAILURE_CLIENT -3 #define LMC_ZERO_HOME_FAILURE_STATE -4 #define LMC_ZERO_HOME_FAILURE_STALE -5 #define LMC_ZERO_HOME_FAILURE_NATIVE -6 #define LMC_ZERO_HOME_FAILURE_VERIFY -7 #define LMC_ZERO_HOME_FAILURE_CORRUPT -8 #define LMC_ZERO_HOME_FAILURE_DS402 -9 #define LMC_ZERO_HOME_STANDSTILL 0x02000000 #define LMC_ZERO_HOME_EVIDENCE_EXPECTED 0x00000001 #define LMC_ZERO_HOME_EVIDENCE_STATE 0x00000002 #define LMC_ZERO_HOME_EVIDENCE_RAW 0x00000004 #define LMC_ZERO_HOME_EVIDENCE_APP 0x00000008 #define LMC_ZERO_HOME_EVIDENCE_INTERNAL 0x00000010 #define LMC_ZERO_HOME_EVIDENCE_STABLE 0x00000020 #define LMC_ZERO_HOME_EVIDENCE_DISPATCH 0x00000003 #define LMC_ZERO_HOME_EVIDENCE_VERIFIED 0x0000001F #define LMC_ZERO_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_PHYSICAL 0x00000001 #define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002 #define LMC_OWNER_STARTUP_LATCH_DS402 0x00000004 #define LMC_OWNER_STARTUP_LATCH_OWNER 0x00000008 #define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010 FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; zeroHomeRequestSequence : UDINT; zeroHomeAppliedSequence : UDINT; zeroHomeRequestToken : UDINT; zeroHomeRequestAxis : DINT; zeroHomeExpectedActualPosition : DINT; zeroHomePhase : DINT; zeroHomeStableSampleCount : DINT; zeroHomeFailure : DINT; zeroHomeCycle : UDINT; zeroHomeEvidence : UDINT; zeroHomeHasRequest : BOOL; zeroHomeTerminal : BOOL; zeroHomeSucceeded : BOOL; zeroHomeDriveConnected : BOOL; zeroHomeAxisConnected : BOOL; zeroHomeAxisStatus : _LMCAXIS_STATUS; zeroHomeAxisError : _LMCAXIS_ERROR; zeroHomeRawDrivePosition : DINT; zeroHomeActualAppPosition : DINT; zeroHomeSetAppPosition : DINT; zeroHomeActualIntPositio ID: 10000
> 						11441 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							11442 단추 위쪽 스크롤 화살표 ID: UpButton
> 							11443 단추 페이지 위로 ID: UpPageButton
> 							11444 위치 조정 위치 ID: ScrollbarThumb
> 							11445 단추 페이지 아래로 ID: DownPageButton
> 							11446 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11447 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							11448 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							11449 위치 조정 위치 ID: ScrollbarThumb
> 							11450 단추 페이지 오른쪽으로 ID: DownPageButton
> 							11451 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						11452 위치 조정 (disabled)
> 			11453 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				11454 창 ID: 59648
> 					11455 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						11456 단추 위쪽 스크롤 화살표 ID: UpButton
> 						11457 위치 조정 위치 ID: ScrollbarThumb
> 						11458 단추 페이지 아래로 ID: DownPageButton
> 						11459 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					11460 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						11461 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						11462 위치 조정 위치 ID: ScrollbarThumb
> 						11463 단추 페이지 오른쪽으로 ID: DownPageButton
> 						11464 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					11465 위치 조정 (disabled)
> 			11466 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				11467 창 ID: 59648
> 					11468 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						11469 단추 위쪽 스크롤 화살표 ID: UpButton
> 						11470 위치 조정 위치 ID: ScrollbarThumb
> 						11471 단추 페이지 아래로 ID: DownPageButton
> 						11472 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					11473 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						11474 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						11475 위치 조정 위치 ID: ScrollbarThumb
> 						11476 단추 페이지 오른쪽으로 ID: DownPageButton
> 						11477 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					11478 위치 조정 (disabled)
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
> 			13 도구 모음 Edit
> 				25729 단추 Toggle bookmark
> 				25730 단추 (disabled) Previous bookmark
> 				25731 단추 (disabled) Next bookmark
> 				25732 단추 (disabled) Delete all bookmarks
> 				25733 단추 (disabled) Previous bookmark in this file
> 				25734 단추 (disabled) Next bookmark in this file
> 				25735 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				25736 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				25737 단추 (disabled) Unindent (Shift+Tab)
> 				25738 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				25739 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				25740 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				25741 단추 DataAnalyzer
> 				25742 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				25743 단추 (disabled) Select
> 				25744 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				25745 단추 Go online (Alt+F6)
> 				25746 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				25747 메뉴 항목 Target Architecture
> 				25748 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				25749 단추 New project (Strg+N)
> 				25750 단추 Open a file (Strg+Shift+O)
> 				25751 단추 Close active document (Strg+F4)
> 				25752 단추 (disabled) Save file (Strg+S)
> 				25753 단추 Open project (Strg+O)
> 				25754 단추 (disabled) Save project changes (Strg+Shift+S)
> 				25755 단추 Close project
> 				25756 단추 Print
> 				25757 단추 Cut (Strg+X)
> 				25758 단추 Copy (Strg+C)
> 				25759 단추 Paste (Strg+V)
> 				25760 메뉴 항목 (disabled) Undo (Strg+Z)
> 				25761 메뉴 항목 (disabled) Redo (Strg+Y)
> 				25762 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				25763 메뉴 항목 FILE
> 				25764 메뉴 항목 EDIT
> 				25765 메뉴 항목 VIEW
> 				25766 메뉴 항목 PROJECT
> 				25767 메뉴 항목 BUILD
> 				25768 메뉴 항목 DEBUG
> 				25769 메뉴 항목 ANALYZE
> 				25770 메뉴 항목 TOOLS
> 				25771 메뉴 항목 EXTRAS
> 				25772 메뉴 항목 WINDOW
> 				25773 메뉴 항목 HELP
> 		67 창 Splitter ID: 271011808
> 		68 창 Splitter ID: 275657544
> 		69 Tab Output ID: 274746808
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						16585 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							16586 단추 위쪽 스크롤 화살표 ID: UpButton
> 							16587 단추 페이지 위로 ID: UpPageButton
> 							16588 위치 조정 위치 ID: ScrollbarThumb
> 							16589 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						16469 목록 항목 (selectable)
> 						16470 목록 항목 (selectable)
> 						16471 목록 항목 (selectable)
> 						16472 목록 항목 (selectable)
> 						16473 목록 항목 (selectable)
> 						16530 목록 항목 (selectable)
> 						16531 목록 항목 (selectable)
> 						16590 목록 항목 (selectable)
> 						16591 목록 항목 (selectable)

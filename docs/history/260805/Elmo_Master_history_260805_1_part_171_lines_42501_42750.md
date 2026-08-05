> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: true, include_text: false});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(\"LASAL_READY_FOR_SEARCH_MENU\");\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 검색 준비"
> }
> ```
>
> ```text
> LASAL_READY_FOR_SEARCH_MENU
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
>   "code": "\n{\n  var observationEditShortcut = globalThis.state;\n  globalThis.state = null;\n  try {\n    await sky.press_key({window: observationEditShortcut.window, key: \"Alt_L+e\"});\n    globalThis.state = await sky.get_window_state({window: observationEditShortcut.window, include_screenshot: true, include_text: true});\n  } catch (error) {\n    nodeRepl.write(\"EDIT_SHORTCUT_ERROR: \"+String(error));\n  }\n  if (globalThis.state) {\n    globalThis.targetWindow = state.window;\n    nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n  }\n}\n",
>   "timeout_ms": 30000,
>   "title": "LASAL 검색 메뉴 열기"
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
> 				20218 단추 Toggle bookmark
> 				20219 단추 (disabled) Previous bookmark
> 				20220 단추 (disabled) Next bookmark
> 				20221 단추 (disabled) Delete all bookmarks
> 				20222 단추 (disabled) Previous bookmark in this file
> 				20223 단추 (disabled) Next bookmark in this file
> 				20224 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				20225 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				20226 단추 (disabled) Unindent (Shift+Tab)
> 				20227 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				20228 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				20229 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				20230 단추 DataAnalyzer
> 				20231 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				20232 단추 (disabled) Select
> 				20233 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				20234 단추 Go online (Alt+F6)
> 				20235 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				20236 메뉴 항목 Target Architecture
> 				20237 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				20238 단추 New project (Strg+N)
> 				20239 단추 Open a file (Strg+Shift+O)
> 				20240 단추 Close active document (Strg+F4)
> 				20241 단추 (disabled) Save file (Strg+S)
> 				20242 단추 Open project (Strg+O)
> 				20243 단추 (disabled) Save project changes (Strg+Shift+S)
> 				20244 단추 Close project
> 				20245 단추 Print
> 				20246 단추 Cut (Strg+X)
> 				20247 단추 Copy (Strg+C)
> 				20248 단추 Paste (Strg+V)
> 				20249 메뉴 항목 (disabled) Undo (Strg+Z)
> 				20250 메뉴 항목 (disabled) Redo (Strg+Y)
> 				20251 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				20252 메뉴 항목 FILE
> 				20253 메뉴 항목 EDIT
> 				20254 메뉴 항목 VIEW
> 				20255 메뉴 항목 PROJECT
> 				20256 메뉴 항목 BUILD
> 				20257 메뉴 항목 DEBUG
> 				20258 메뉴 항목 ANALYZE
> 				20259 메뉴 항목 TOOLS
> 				20260 메뉴 항목 EXTRAS
> 				20261 메뉴 항목 WINDOW
> 				20262 메뉴 항목 HELP
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
> 						16592 목록 항목 (selectable)
> 						16593 목록 항목 (selectable)
> 						16594 목록 항목 (selectable)
> 						16595 목록 항목 (selectable)
> 						16596 목록 항목 (selectable)
> 						16597 목록 항목 (selectable)
> 						16598 목록 항목 (selectable)
> 						16599 목록 항목 (selectable)
> 						16600 목록 항목 (selectable)
> 						16601 목록 항목 (selectable)
> 						16602 목록 항목 (selectable)
> 						16603 목록 항목 (selectable)
> 						16604 목록 항목 (selectable)
> 						16605 목록 항목 (selectable)
> 						16606 목록 항목 (selectable)
> 						16607 목록 항목 (selectable)
> 						16608 목록 항목 (selectable)
> 						16609 목록 항목 (selectable)
> 						16610 목록 항목 (selectable)
> 						16611 목록 항목 (selectable)
> 						16612 목록 항목 (selectable)
> 						16613 목록 항목 (selectable)
> 						16614 목록 항목 (selectable)
> 						16615 목록 항목 (selectable)
> 						16616 목록 항목 (selectable)
> 						16617 목록 항목 (selectable)
> 						16618 목록 항목 (selectable)
> 						16619 목록 항목 (selectable)
> 						16620 목록 항목 (selectable)
> 						16621 목록 항목 (selectable)
> 						16622 목록 항목 (selectable)
> 						16623 목록 항목 (selectable)
> 						16624 목록 항목 (selectable)
> 						16625 목록 항목 (selectable)
> 						16626 목록 항목 (selectable)
> 						16627 목록 항목 (selectable)
> 						16628 목록 항목 (selectable)
> 						16629 목록 항목 (selectable)
> 						16630 목록 항목 (selectable)
> 						16631 목록 항목 (selectable)
> 						16632 목록 항목 (selectable)
> 						16633 목록 항목 (selectable)
> 						16634 목록 항목 (selectable)
> 						16635 목록 항목 (selectable)
> 						16636 목록 항목 (selectable)
> 						16637 목록 항목 (selectable)
> 						16638 목록 항목 (selectable)
> 						16639 목록 항목 (selectable)
> 						16640 목록 항목 (selectable)
> 						16641 목록 항목 (selectable)
> 						16642 목록 항목 (selectable)
> 						16643 목록 항목 (selectable)
> 						16644 목록 항목 (selectable)
> 						16698 목록 항목 (selectable)
> 						16699 목록 항목 (selectable)
> 						16700 목록 항목 (selectable)
> 						16701 목록 항목 (selectable)
> 						16702 목록 항목 (selectable)
> 						16703 목록 항목 (selectable)
> 						16704 목록 항목 (selectable)
> 						16705 목록 항목 (selectable)
> 						16706 목록 항목 (selectable)
> 						16707 목록 항목 (selectable)
> 						16708 목록 항목 (selectable)
> 						16709 목록 항목 (selectable)
> 						16710 목록 항목 (selectable)
> 						16711 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton

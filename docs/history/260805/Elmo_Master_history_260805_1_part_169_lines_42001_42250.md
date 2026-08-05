>   "code": "\n{\n  var observationBuildStart = globalThis.state;\n  globalThis.state = null;\n  try {\n    await sky.press_key({window: observationBuildStart.window, key: \"F9\"});\n    globalThis.state = await sky.get_window_state({window: observationBuildStart.window, include_screenshot: true, include_text: true});\n  } catch (error) {\n    nodeRepl.write(\"BUILD_START_ERROR: \"+String(error));\n  }\n  if (globalThis.state) {\n    globalThis.targetWindow = state.window;\n    nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n  }\n}\n",
>   "timeout_ms": 30000,
>   "title": "LASAL C78 Rebuild 시작"
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
> 				16371 단추 Toggle bookmark
> 				16372 단추 (disabled) Previous bookmark
> 				16373 단추 (disabled) Next bookmark
> 				16374 단추 (disabled) Delete all bookmarks
> 				16375 단추 (disabled) Previous bookmark in this file
> 				16376 단추 (disabled) Next bookmark in this file
> 				16377 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				16378 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				16379 단추 (disabled) Unindent (Shift+Tab)
> 				16380 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				16381 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				16382 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				16383 단추 DataAnalyzer
> 				16384 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				16385 단추 (disabled) Select
> 				16386 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				16387 단추 Go online (Alt+F6)
> 				16388 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				16389 메뉴 항목 Target Architecture
> 				16390 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				16391 단추 New project (Strg+N)
> 				16392 단추 Open a file (Strg+Shift+O)
> 				16393 단추 Close active document (Strg+F4)
> 				16394 단추 (disabled) Save file (Strg+S)
> 				16395 단추 Open project (Strg+O)
> 				16396 단추 (disabled) Save project changes (Strg+Shift+S)
> 				16397 단추 Close project
> 				16398 단추 Print
> 				16399 단추 Cut (Strg+X)
> 				16400 단추 Copy (Strg+C)
> 				16401 단추 Paste (Strg+V)
> 				16402 메뉴 항목 (disabled) Undo (Strg+Z)
> 				16403 메뉴 항목 (disabled) Redo (Strg+Y)
> 				16404 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				16405 메뉴 항목 FILE
> 				16406 메뉴 항목 EDIT
> 				16407 메뉴 항목 VIEW
> 				16408 메뉴 항목 PROJECT
> 				16409 메뉴 항목 BUILD
> 				16410 메뉴 항목 DEBUG
> 				16411 메뉴 항목 ANALYZE
> 				16412 메뉴 항목 TOOLS
> 				16413 메뉴 항목 EXTRAS
> 				16414 메뉴 항목 WINDOW
> 				16415 메뉴 항목 HELP
> 		67 창 Splitter ID: 271011808
> 		68 창 Splitter ID: 275657544
> 		69 Tab Output ID: 274746808
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						10647 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							10648 단추 위쪽 스크롤 화살표 ID: UpButton
> 							10649 단추 페이지 위로 ID: UpPageButton
> 							10650 위치 조정 위치 ID: ScrollbarThumb
> 							15187 단추 페이지 아래로 ID: DownPageButton
> 							10651 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						15188 목록 항목 (selectable)
> 						15189 목록 항목 (selectable)
> 						15190 목록 항목 (selectable)
> 						15191 목록 항목 (selectable)
> 						15192 목록 항목 (selectable)
> 						15193 목록 항목 (selectable)
> 						15194 목록 항목 (selectable)
> 						15195 목록 항목 (selectable)
> 						15196 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			16416 탭 항목 (selectable) Python Script
> 			16417 탭 항목 (selectable) Debugger
> 			16418 탭 항목 (selectable) Output
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
> 			16419 탭 항목 (selectable) Lib
> 			16420 탭 항목 (selectable) File
> 			16421 탭 항목 (selectable) Class
> 			16422 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274740424
> 			90 창 (disabled) ID: 260670512
> 				91 TABLE Properties Window ID: 272204320
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			16423 탭 항목 (selectable) Properties
> 			96 단추 Close
>
> The focused UI element is 72 LIST ID: 1201.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> C78/ARM Rebuild를 시작했습니다. IDE가 현재 프로젝트를 다시 저장하고 컴파일 중입니다. 결과가 나오면 오류 줄을 바로 추적해 수정하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait new Promise(resolve => setTimeout(resolve, 10000));\nglobalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: true, include_text: true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n",
>   "timeout_ms": 30000,
>   "title": "LASAL Rebuild 결과 확인"
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

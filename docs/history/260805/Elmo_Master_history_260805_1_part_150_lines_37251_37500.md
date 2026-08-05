> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21397 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21398 위치 조정 위치 ID: ScrollbarThumb
> 						21399 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21400 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21401 위치 조정 (disabled)
> 			21402 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				21403 창 ID: 59648
> 					21404 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21405 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21406 위치 조정 위치 ID: ScrollbarThumb
> 						21407 단추 페이지 아래로 ID: DownPageButton
> 						21408 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21409 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						21410 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						21411 위치 조정 위치 ID: ScrollbarThumb
> 						21412 단추 페이지 오른쪽으로 ID: DownPageButton
> 						21413 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					21414 위치 조정 (disabled)
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
> 				42793 단추 (disabled) Toggle bookmark
> 				42794 단추 (disabled) Previous bookmark
> 				42795 단추 (disabled) Next bookmark
> 				42796 단추 (disabled) Delete all bookmarks
> 				42797 단추 (disabled) Previous bookmark in this file
> 				42798 단추 (disabled) Next bookmark in this file
> 				42799 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				42800 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				42801 단추 (disabled) Unindent (Shift+Tab)
> 				42802 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				42803 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				42804 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				42805 단추 DataAnalyzer
> 				42806 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				42807 단추 Select
> 				42808 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				42809 단추 Go online (Alt+F6)
> 				42810 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				42811 메뉴 항목 Targ
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
>   "title": "함수 선언 편집기 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:922,y:190,mouse_button:\"left\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "함수 선언 편집기 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,5000));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		43873 창
> 		1 창 작업 영역 ID: 59648
> 			43874 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65282
> 				43875 창 ID: 59648
> 					43876 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_ZERO_HOME_STATE_RUNNING 1 #define LMC_ZERO_HOME_STATE_SUCCEEDED 2 #define LMC_ZERO_HOME_STATE_FAILED 3 #define LMC_ZERO_HOME_PHASE_VERIFY 1 #define LMC_ZERO_HOME_REQUIRED_STABLE 3 #define LMC_ZERO_HOME_FAILURE_INVALID -1 #define LMC_ZERO_HOME_FAILURE_BUSY -2 #define LMC_ZERO_HOME_FAILURE_CLIENT -3 #define LMC_ZERO_HOME_FAILURE_STATE -4 #define LMC_ZERO_HOME_FAILURE_STALE -5 #define LMC_ZERO_HOME_FAILURE_NATIVE -6 #define LMC_ZERO_HOME_FAILURE_VERIFY -7 #define LMC_ZERO_HOME_FAILURE_CORRUPT -8 #define LMC_ZERO_HOME_FAILURE_DS402 -9 #define LMC_ZERO_HOME_STANDSTILL 0x02000000 #define LMC_ZERO_HOME_EVIDENCE_EXPECTED 0x00000001 #define LMC_ZERO_HOME_EVIDENCE_STATE 0x00000002 #define LMC_ZERO_HOME_EVIDENCE_RAW 0x00000004 #define LMC_ZERO_HOME_EVIDENCE_APP 0x00000008 #define LMC_ZERO_HOME_EVIDENCE_INTERNAL 0x00000010 #define LMC_ZERO_HOME_EVIDENCE_STABLE 0x00000020 #define LMC_ZERO_HOME_EVIDENCE_DISPATCH 0x00000003 #define LMC_ZERO_HOME_EVIDENCE_VERIFIED 0x0000001F #define LMC_ZERO_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_PHYSICAL 0x00000001 #define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002 #define LMC_OWNER_STARTUP_LATCH_DS402 0x00000004 #define LMC_OWNER_STARTUP_LATCH_OWNER 0x00000008 #define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010 FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; zeroHomeRequestSequence : UDINT; zeroHomeAppliedSequence : UDINT; zeroHomeRequestToken : UDINT; zeroHomeRequestAxis : DINT; zeroHomeExpectedActualPosition : DINT; zeroHomePhase : DINT; zeroHomeStableSampleCount : DINT; zeroHomeFailure : DINT; zeroHomeCycle : UDINT; zeroHomeEvidence : UDINT; zeroHomeHasRequest : BOOL; zeroHomeTerminal : BOOL; zeroHomeSucceeded : BOOL; zeroHomeDriveConnected : BOOL; zeroHomeAxisConnected : BOOL; zeroHomeAxisStatus : _LMCAXIS_STATUS; zeroHomeAxisError : _LMCAXIS_ERROR; zeroHomeRawDrivePosition : DINT; zeroHomeActualAppPosition : DINT; zeroHomeSetAppPosition : DINT; zeroHomeActualIntPositio ID: 10000
> 						43877 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							43878 단추 위쪽 스크롤 화살표 ID: UpButton
> 							43879 단추 페이지 위로 ID: UpPageButton
> 							43880 위치 조정 위치 ID: ScrollbarThumb
> 							43881 단추 페이지 아래로 ID: DownPageButton
> 							43882 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						43883 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							43884 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							43885 위치 조정 위치 ID: ScrollbarThumb
> 							43886 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43887 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43888 위치 조정 (disabled)
> 			21389 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				21390 창 ID: 59648
> 					21391 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						21392 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21393 위치 조정 위치 ID: ScrollbarThumb
> 						21394 단추 페이지 아래로 ID: DownPageButton
> 						21395 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					21396 스크롤 막대 가로 ID: NonClientHorizontalScr
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
>   "title": "구현 편집기 검색 메뉴 확인",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,screenshotId:globalThis.state.screenshots[0].id,x:370,y:137,mouse_button:\"right\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "구현 검색 메뉴 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(0,2600));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch Secondary Actions: Raise
> 		45246 창
> 		45247 창
> 		45248 메뉴
> 			45774 메뉴 항목 (disabled) Undo Ctrl+Z
> 			45775 메뉴 항목 (disabled) Redo Ctrl+Y
> 			45776 메뉴 항목 (disabled) Cut Ctrl+X
> 			45777 메뉴 항목 (disabled) Copy Ctrl+C
> 			45778 메뉴 항목 (disabled) Paste Ctrl+V
> 			45779 메뉴 항목 (disabled) Delete
> 			45780 메뉴 항목 Select All Ctrl+A
> 			45781 메뉴 항목 Go To Definition F11
> 			45782 메뉴 항목 Toggle Breakpoint F4
> 			45783 메뉴 항목 Toggle Bookmark Ctrl+F2
> 			45784 메뉴 항목 Mark Text
> 			45785 메뉴 항목 Clear Marks
> 			45786 메뉴 항목 Toggle Function Folding
> 			45787 메뉴 항목 Update Method
> 			45788 메뉴 항목 Insert Client-Update Source Code...
> 			45789 메뉴 항목 Insert NewInst command...
> 		1 창 작업 영역 ID: 59648
> 			43874 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65282
> 				43875 창 ID: 59648
> 					43876 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_ZERO_HOME_STATE_RUNNING 1 #define LMC_ZERO_HOME_STATE_SUCCEEDED 2 #define LMC_ZERO_HOME_STATE_FAILED 3 #define LMC_ZERO_HOME_PHASE_VERIFY 1 #define LMC_ZERO_HOME_REQUIRED_STABLE 3 #define LMC_ZERO_HOME_FAILURE_INVALID -1 #define LMC_ZERO_HOME_FAILURE_BUSY -2 #define LMC_ZERO_HOME_FAILURE_CLIENT -3 #define LMC_ZERO_HOME_FAILURE_STATE -4 #define LMC_ZERO_HOME_FAILURE_STALE -5 #define LMC_ZERO_HOME_FAILURE_NATIVE -6 #define LMC_ZERO_HOME_FAILURE_VERIFY -7 #define LMC_ZERO_HOME_FAILURE_CORRUPT -8 #define LMC_ZERO_HOME_FAILURE_DS402 -9 #define LMC_ZERO_HOME_STANDSTILL 0x02000000 #define LMC_ZERO_HOME_EVIDENCE_EXPECTED 0x00000001 #define LMC_ZERO_HOME_EVIDENCE_STATE 0x00000002 #define LMC_ZERO_HOME_EVIDENCE_RAW 0x00000004 #define LMC_ZERO_HOME_EVIDENCE_APP 0x00000008 #define LMC_ZERO_HOME_EVIDENCE_INTERNAL 0x00000010 #define LMC_ZERO_HOME_EVIDENCE_STABLE 0x00000020 #define LMC_ZERO_HOME_EVIDENCE_DISPATCH 0x00000003 #define LMC_ZERO_HOME_EVIDENCE_VERIFIED 0x0000001F #define LMC_ZERO_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_PHYSICAL 0x00000001 #define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002 #define LMC_OWNER_STARTUP_LATCH_DS402 0x00000004 #define LMC_OWNER_STARTUP_LATCH_OWNER 0x00000008 #define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010 FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDIN
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
>   "title": "구현 편집기 메뉴 닫기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ESC\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 루트 다시 찾기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"CTRL+SHIFT+T\"});"
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "클래스 검색 대화상자 확인",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({shots:globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height})),dialog:(globalThis.state.accessibility?.tree||\"\").includes(\"Find in Tree\")});"
> }
> ```
>
> ```text
> {
>   shots: [
>     { id: 'screenshot-0', z: 0, w: 970, h: 512 },
>     { id: 'screenshot-1', z: 1, w: 960, h: 502 },
>     { id: 'screenshot-2', z: 2, w: 448, h: 225 }
>   ],

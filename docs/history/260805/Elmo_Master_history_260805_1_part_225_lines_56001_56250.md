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
> 				8302 단추 Toggle bookmark
> 				8303 단추 (disabled) Previous bookmark
> 				8304 단추 (disabled) Next bookmark
> 				8305 단추 (disabled) Delete all bookmarks
> 				8306 단추 (disabled) Previous bookmark in this file
> 				8307 단추 (disabled) Next bookmark in this file
> 				8308 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				8309 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				8310 단추 (disabled) Unindent (Shift+Tab)
> 				8311 단추 (disabled) Indent (Tab)
> 			25 도구 모음 Macros Manager
> 				8312 메뉴 항목 Macros
> 			27 도구 모음 Layout Manager
> 				8313 메뉴 항목 Layouts
> 			29 도구 모음 Toolbox
> 				8314 단추 DataAnalyzer
> 				8315 메뉴 항목 Toolbar Options
> 			32 도구 모음 Net Edit
> 				8316 단추 (disabled) Select
> 				8317 메뉴 항목 Toolbar Options
> 			35 도구 모음 Debug
> 				8318 단추 Go online (Alt+F6)
> 				8319 단추 Change Online Settings
> 				8320 메뉴 항목 Online Connection
> 				8321 단추 (disabled) Set Online Connection For Current Project
> 				8322 단추 (disabled) Download (F6)
> 				8323 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				8324 단추 (disabled) Download Module on the Fly
> 				8325 단추 (disabled) Save Project on PLC
> 				8326 단추 (disabled) Start (F7)
> 				8327 단추 (disabled) Reset (F8)
> 				8328 단추 Toggle breakpoint (F4)
> 				8329 단추 Create condition breakpoint
> 				8330 메뉴 항목 Toolbar Options
> 			49 도구 모음 Build
> 				8331 메뉴 항목 Target Architecture
> 				8332 단추 Build changes (F9)
> 				8333 단추 Rebuild project (Strg+F9)
> 				8334 단추 (disabled) Cancel building (Ctrl+Break)
> 				8335 단추 Link project
> 			55 도구 모음 Standard
> 				8336 단추 New project (Strg+N)
> 				8337 단추 Open a file (Strg+Shift+O)
> 				8338 단추 Close active document (Strg+F4)
> 				8339 단추 (disabled) Save file (Strg+S)
> 				8340 단추 Open project (Strg+O)
> 				8341 단추 (disabled) Save project changes (Strg+Shift+S)
> 				8342 단추 Close project
> 				8343 단추 Print
> 				8344 단추 Cut (Strg+X)
> 				8345 단추 Copy (Strg+C)
> 				8346 단추 Paste (Strg+V)
> 				8347 메뉴 항목 (disabled) Undo (Strg+Z)
> 				8348 메뉴 항목 (disabled) Redo (Strg+Y)
> 				8349 단추 Navigate Backward (Alt+Left)
> 				8350 단추 (disabled) Navigate Forward (Alt +Right)
> 			71 메뉴 모음 Menu Bar
> 				8351 메뉴 항목 FILE
> 				8352 메뉴 항목 EDIT
> 				8353 메뉴 항목 VIEW
> 				8354 메뉴 항목 PROJECT
> 				8355 메뉴 항목 BUILD
> 				8356 메뉴 항목 DEBUG
> 				8357 메뉴 항목 ANALYZE
> 				8358 메뉴 항목 TOOLS
> 				8359 메뉴 항목 EXTRAS
> 				8360 메뉴 항목 WINDOW
> 				8361 메뉴 항목 HELP
> 		83 창 Splitter ID: 384042808
> 		84 창 Splitter ID: 384041968
> 		85 Tab Output ID: 274291232
> 			86 창 ID: 1200
> 				87 창 ID: 1200
> 					88 LIST ID: 1201
> 						670 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							671 단추 위쪽 스크롤 화살표 ID: UpButton
> 							672 단추 페이지 위로 ID: UpPageButton
> 							673 위치 조정 위치 ID: ScrollbarThumb
> 							674 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						1046 목록 항목 (selectable)
> 						1047 목록 항목 (selectable)
> 						1048 목록 항목 (selectable)
> 						1049 목록 항목 (selectable)
> 						1050 목록 항목 (selectable)
> 						1051 목록 항목 (selectable)
> 						1121 목록 항목 (selectable)
> 						1122 목록 항목 (selectable)
> 						1192 목록 항목 (selectable)
> 						1355 목록 항목 (selectable)
> 						1356 목록 항목 (selectable)
> 						1357 목록 항목 (selectable)
> 						1358 목록 항목 (selectable)
> 						1359 목록 항목 (selectable)
> 						1360 목록 항목 (selectable)
> 					90 스크롤 막대 ID: 59904
> 						91 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						92 위치 조정 위치 ID: ScrollbarThumb
> 						93 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			8362 탭 항목 (selectable) Python Script
> 			8363 탭 항목 (selectable) Debugger
> 			8364 탭 항목 (selectable) Output
> 			97 단추 Close
> 		98 창 Splitter ID: 384043816
> 		99 Tab Class View ID: 274289864
> 			100 트리 ID: 103
> 				1196 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					1197 단추 위쪽 스크롤 화살표 ID: UpButton
> 					8365 단추 페이지 위로 ID: UpPageButton
> 					1198 위치 조정 위치 ID: ScrollbarThumb
> 					1199 단추 페이지 아래로 ID: DownPageButton
> 					1200 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				1201 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
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
> 						8370 콘솔 트리 (selectable) Objects
> 						8371 콘솔 트리 (selectable) Dependencies
> 					1212 콘솔 트리 (selectable) LMCDiagnosticsService
> 					1213 콘솔 트리 (selectable) LMCEcatInputLatch
> 					1214 콘솔 트리 (selectable) LMCRecorderStore
> 					1215 콘솔 트리 (selectable) LMCSdoExecutor
> 					1216 콘솔 트리 (selectable) TCPIPServer
> 					1217 콘솔 트리 (selectable) TCPMotionInterface
> 			8372 탭 항목 (selectable) Lib
> 			8373 탭 항목 (selectable) File
> 			8374 탭 항목 (selectable) Global
> 			8375 탭 항목 (selectable) Net
> 			8376 탭 항목 (selectable) Class
> 			106 단추 Close
> 		107 Tab Properties ID: 385795760
> 			108 창 ID: 262717224
> 				109 TABLE Properties Window ID: 272020368
> 					7441 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						7442 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7443 위치 조정 위치 ID: ScrollbarThumb
> 						7444 단추 페이지 아래로 ID: DownPageButton
> 						7445 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					8377 custom Name
> 					8378 custom Revision
> 					8379 custom GUID
> 					8380 custom Task Settings
> 					8381 custom Sigmatek
> 					8382 custom OSInterface
> 					8383 custom IconPath
> 					8384 custom SharedCommandTable
> 					8385 custom Objectsize
> 					8386 custom Singleton
> 					8387 custom Hidden
> 					8388 custom Deprecated
> 					8389 custom GCCOptions
> 					8390 custom Comment
> 					8391 custom Filename
> 				110 도구 모음 ID: 59392
> 					111 단추
> 					112 단추
> 			8392 탭 항목 (selectable) Properties
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
>   "code": "{const observation=globalThis.state;if(!observation?.accessibility)throw new Error('reobserve');globalThis.state=null;await sky.click({window:observation.window,element_index:8369,click_count:2});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''))}",
>   "title": "LMCControlCommandService 변수 목록 열기"
> }
> ```

> 				8502 단추 (disabled) Navigate Backward (Alt+Left)
> 				8503 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				8504 메뉴 항목 FILE
> 				8505 메뉴 항목 EDIT
> 				8506 메뉴 항목 VIEW
> 				8507 메뉴 항목 PROJECT
> 				8508 메뉴 항목 BUILD
> 				8509 메뉴 항목 DEBUG
> 				8510 메뉴 항목 ANALYZE
> 				8511 메뉴 항목 TOOLS
> 				8512 메뉴 항목 EXTRAS
> 				8513 메뉴 항목 WINDOW
> 				8514 메뉴 항목 HELP
> 		82 창 Splitter ID: 306256112
> 		83 창 Splitter ID: 306255440
> 		84 Tab Output ID: 306064200
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						5235 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							5236 단추 위쪽 스크롤 화살표 ID: UpButton
> 							5237 단추 페이지 위로 ID: UpPageButton
> 							5238 위치 조정 위치 ID: ScrollbarThumb
> 							5239 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						5871 목록 항목 (selectable)
> 						5939 목록 항목 (selectable)
> 						6030 목록 항목 (selectable)
> 						6031 목록 항목 (selectable)
> 						6032 목록 항목 (selectable)
> 						6033 목록 항목 (selectable)
> 						6034 목록 항목 (selectable)
> 						6035 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			8515 탭 항목 (selectable) Python Script
> 			8516 탭 항목 (selectable) Debugger
> 			8517 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 306256448
> 		97 Tab Class View ID: 306057360
> 			98 트리 ID: 103
> 				5943 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					5944 단추 위쪽 스크롤 화살표 ID: UpButton
> 					5945 위치 조정 위치 ID: ScrollbarThumb
> 					5946 단추 페이지 아래로 ID: DownPageButton
> 					5947 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				5948 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					5949 콘솔 트리 (selectable) External
> 					5950 콘솔 트리 (selectable) Sigmatek
> 					5951 콘솔 트리 (selectable) Elmo_1
> 					5952 콘솔 트리 (selectable) Elmo_2
> 					5953 콘솔 트리 (selectable) Elmo_3
> 					5954 콘솔 트리 (selectable) Elmo_4
> 					5955 콘솔 트리 (selectable) GL_9086_1
> 					5956 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					5957 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					5958 콘솔 트리 (selectable) LMCControlCommandService
> 					5959 콘솔 트리 (selectable) LMCDiagnosticsService
> 					5960 콘솔 트리 (selectable) LMCEcatInputLatch
> 					5961 콘솔 트리 (selectable) LMCRecorderStore
> 					5962 콘솔 트리 (selectable) LMCSdoExecutor
> 					5963 콘솔 트리 (selectable) TCPIPServer
> 					5964 콘솔 트리 (selectable) TCPMotionInterface
> 			8518 탭 항목 (selectable) Lib
> 			8519 탭 항목 (selectable) File
> 			8520 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 306057816
> 			104 창 ID: 297102840
> 				105 TABLE Properties Window ID: 301242408
> 					8521 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						8522 단추 위쪽 스크롤 화살표 ID: UpButton
> 						8523 위치 조정 위치 ID: ScrollbarThumb
> 						8524 단추 페이지 아래로 ID: DownPageButton
> 						8525 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					8526 custom Name
> 					8527 custom Revision
> 					8528 custom GUID
> 					8529 custom Task Settings
> 					8530 custom Sigmatek
> 					8531 custom OSInterface
> 					8532 custom IconPath
> 					8533 custom SharedCommandTable
> 					8534 custom Objectsize
> 					8535 custom Singleton
> 					8536 custom Hidden
> 					8537 custom Deprecated
> 					8538 custom GCCOptions
> 					8539 custom Comment
> 					8540 custom Filename
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			8541 탭 항목 (selectable) Properties
> 			110 단추 Close
>
> The focused UI element is 98 트리 ID: 103.
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
>   "code": "var obsDiag = globalThis.state; globalThis.state=null; await sky.click({window:obsDiag.window,element_index:5959}); globalThis.state=await sky.get_window_state({window:obsDiag.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree || ''));",
>   "title": "Select diagnostics class",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis}", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
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
> 				9362 단추 (disabled) Toggle bookmark
> 				9363 단추 (disabled) Previous bookmark
> 				9364 단추 (disabled) Next bookmark
> 				9365 단추 (disabled) Delete all bookmarks
> 				9366 단추 (disabled) Previous bookmark in this file
> 				9367 단추 (disabled) Next bookmark in this file
> 				9368 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				9369 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				9370 단추 (disabled) Unindent (Shift+Tab)
> 				9371 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				9372 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				9373 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				9374 단추 DataAnalyzer
> 				9375 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				9376 단추 (disabled) Select
> 				9377 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				9378 단추 Go online (Alt+F6)
> 				9379 단추 Change Online Settings
> 				9380 메뉴 항목 Online Connection
> 				9381 단추 (disabled) Set Online Connection For Current Project
> 				9382 단추 (disabled) Download (F6)
> 				9383 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				9384 단추 (disabled) Download Module on the Fly
> 				9385 단추 (disabled) Save Project on PLC
> 				9386 단추 (disabled) Start (F7)
> 				9387 단추 (disabled) Reset (F8)
> 				9388 단추 (disabled) Toggle breakpoint (F4)
> 				9389 단추 (disabled) Create condition breakpoint
> 				9390 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				9391 메뉴 항목 Target Architecture
> 				9392 단추 Build changes (F9)
> 				9393 단추 Rebuild project (Strg+F9)
> 				9394 단추 (disabled) Cancel building (Ctrl+Break)
> 				9395 단추 Link project
> 			54 도구 모음 Standard
> 				9396 단추 New project (Strg+N)
> 				9397 단추 Open a file (Strg+Shift+O)
> 				9398 단추 (disabled) Close active document (Strg+F4)
> 				9399 단추 (disabled) Save file (Strg+S)
> 				9400 단추 Open project (Strg+O)
> 				9401 단추 (disabled) Save project changes (Strg+Shift+S)
> 				9402 단추 Close project
> 				9403 단추 (disabled) Print
> 				9404 단추 Cut (Strg+X)
> 				9405 단추 Copy (Strg+C)
> 				9406 단추 Paste (Strg+V)
> 				9407 메뉴 항목 (disabled) Undo (Strg+Z)
> 				9408 메뉴 항목 (disabled) Redo (Strg+Y)
> 				9409 단추 (disabled) Navigate Backward (Alt+Left)
> 				9410 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				9411 메뉴 항목 FILE
> 				9412 메뉴 항목 EDIT
> 				9413 메뉴 항목 VIEW
> 				9414 메뉴 항목 PROJECT
> 				9415 메뉴 항목 BUILD
> 				9416 메뉴 항목 DEBUG
> 				9417 메뉴 항목 ANALYZE
> 				9418 메뉴 항목 TOOLS
> 				9419 메뉴 항목 EXTRAS
> 				9420 메뉴 항목 WINDOW
> 				9421 메뉴 항목 HELP
> 		82 창 Splitter ID: 306256112
> 		83 창 Splitter ID: 306255440
> 		84 Tab Output ID: 306064200
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						5235 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							5236 단추 위쪽 스크롤 화살표 ID: UpButton
> 							5237 단추 페이지 위로 ID: UpPageButton
> 							5238 위치 조정 위치 ID: ScrollbarThumb
> 							5239 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						5871 목록 항목 (selectable)
> 						5939 목록 항목 (selectable)
> 						6030 목록 항목 (selectable)
> 						6031 목록 항목 (selectable)
> 						6032 목록 항목 (selectable)
> 						6033 목록 항목 (selectable)
> 						6034 목록 항목 (selectable)
> 						6035 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			9422 탭 항목 (selectable) Python Script
> 			9423 탭 항목 (selectable) Debugger
> 			9424 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 306256448
> 		97 Tab Class View ID: 306057360
> 			98 트리 ID: 103
> 				5943 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					5944 단추 위쪽 스크롤 화살표 ID: UpButton
> 					5945 위치 조정 위치 ID: ScrollbarThumb
> 					5946 단추 페이지 아래로 ID: DownPageButton
> 					5947 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				5948 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					5949 콘솔 트리 (selectable) External
> 					5950 콘솔 트리 (selectable) Sigmatek
> 					5951 콘솔 트리 (selectable) Elmo_1
> 					5952 콘솔 트리 (selectable) Elmo_2
> 					5953 콘솔 트리 (selectable) Elmo_3
> 					5954 콘솔 트리 (selectable) Elmo_4
> 					5955 콘솔 트리 (selectable) GL_9086_1
> 					5956 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					5957 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					5958 콘솔 트리 (selectable) LMCControlCommandService
> 					5959 콘솔 트리 (selectable) LMCDiagnosticsService
> 					5960 콘솔 트리 (selectable) LMCEcatInputLatch

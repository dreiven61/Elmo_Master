> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ''));",
>   "title": "Wait for LASAL project",
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
> 				7919 단추 (disabled) Toggle bookmark
> 				7920 단추 (disabled) Previous bookmark
> 				7921 단추 (disabled) Next bookmark
> 				7922 단추 (disabled) Delete all bookmarks
> 				7923 단추 (disabled) Previous bookmark in this file
> 				7924 단추 (disabled) Next bookmark in this file
> 				7925 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				7926 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				7927 단추 (disabled) Unindent (Shift+Tab)
> 				7928 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				7929 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				7930 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				7931 단추 DataAnalyzer
> 				7932 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				7933 단추 (disabled) Select
> 				7934 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				7935 단추 Go online (Alt+F6)
> 				7936 단추 Change Online Settings
> 				7937 메뉴 항목 Online Connection
> 				7938 단추 (disabled) Set Online Connection For Current Project
> 				7939 단추 (disabled) Download (F6)
> 				7940 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				7941 단추 (disabled) Download Module on the Fly
> 				7942 단추 (disabled) Save Project on PLC
> 				7943 단추 (disabled) Start (F7)
> 				7944 단추 (disabled) Reset (F8)
> 				7945 단추 (disabled) Toggle breakpoint (F4)
> 				7946 단추 (disabled) Create condition breakpoint
> 				7947 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				7948 메뉴 항목 Target Architecture
> 				7949 단추 Build changes (F9)
> 				7950 단추 Rebuild project (Strg+F9)
> 				7951 단추 (disabled) Cancel building (Ctrl+Break)
> 				7952 단추 Link project
> 			54 도구 모음 Standard
> 				7953 단추 New project (Strg+N)
> 				7954 단추 Open a file (Strg+Shift+O)
> 				7955 단추 (disabled) Close active document (Strg+F4)
> 				7956 단추 (disabled) Save file (Strg+S)
> 				7957 단추 Open project (Strg+O)
> 				7958 단추 (disabled) Save project changes (Strg+Shift+S)
> 				7959 단추 Close project
> 				7960 단추 (disabled) Print
> 				7961 단추 Cut (Strg+X)
> 				7962 단추 Copy (Strg+C)
> 				7963 단추 Paste (Strg+V)
> 				7964 메뉴 항목 (disabled) Undo (Strg+Z)
> 				7965 메뉴 항목 (disabled) Redo (Strg+Y)
> 				7966 단추 (disabled) Navigate Backward (Alt+Left)
> 				7967 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				7968 메뉴 항목 FILE
> 				7969 메뉴 항목 EDIT
> 				7970 메뉴 항목 VIEW
> 				7971 메뉴 항목 PROJECT
> 				7972 메뉴 항목 BUILD
> 				7973 메뉴 항목 DEBUG
> 				7974 메뉴 항목 ANALYZE
> 				7975 메뉴 항목 TOOLS
> 				7976 메뉴 항목 EXTRAS
> 				7977 메뉴 항목 WINDOW
> 				7978 메뉴 항목 HELP
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
> 			7979 탭 항목 (selectable) Python Script
> 			7980 탭 항목 (selectable) Debugger
> 			7981 탭 항목 (selectable) Output
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
> 			7982 탭 항목 (selectable) Lib
> 			7983 탭 항목 (selectable) File
> 			7984 탭 항목 (selectable) Class
> 			102 단추 Close
> 		103 Tab Properties ID: 306057816
> 			104 창 ID: 297102840
> 				105 TABLE Properties Window ID: 301242408
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			7985 탭 항목 (selectable) Properties
> 			110 단추 Close
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
>   "code": "var obsClass = globalThis.state; globalThis.state=null; await sky.click({window:obsClass.window,element_index:5960}); globalThis.state=await sky.get_window_state({window:obsClass.window,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree || ''));",
>   "title": "Open diagnostics class",
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
> 				8455 단추 (disabled) Toggle bookmark
> 				8456 단추 (disabled) Previous bookmark
> 				8457 단추 (disabled) Next bookmark
> 				8458 단추 (disabled) Delete all bookmarks
> 				8459 단추 (disabled) Previous bookmark in this file
> 				8460 단추 (disabled) Next bookmark in this file
> 				8461 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				8462 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				8463 단추 (disabled) Unindent (Shift+Tab)
> 				8464 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				8465 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				8466 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				8467 단추 DataAnalyzer
> 				8468 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				8469 단추 (disabled) Select
> 				8470 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				8471 단추 Go online (Alt+F6)
> 				8472 단추 Change Online Settings
> 				8473 메뉴 항목 Online Connection
> 				8474 단추 (disabled) Set Online Connection For Current Project
> 				8475 단추 (disabled) Download (F6)
> 				8476 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				8477 단추 (disabled) Download Module on the Fly
> 				8478 단추 (disabled) Save Project on PLC
> 				8479 단추 (disabled) Start (F7)
> 				8480 단추 (disabled) Reset (F8)
> 				8481 단추 (disabled) Toggle breakpoint (F4)
> 				8482 단추 (disabled) Create condition breakpoint
> 				8483 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				8484 메뉴 항목 Target Architecture
> 				8485 단추 Build changes (F9)
> 				8486 단추 Rebuild project (Strg+F9)
> 				8487 단추 (disabled) Cancel building (Ctrl+Break)
> 				8488 단추 Link project
> 			54 도구 모음 Standard
> 				8489 단추 New project (Strg+N)
> 				8490 단추 Open a file (Strg+Shift+O)
> 				8491 단추 (disabled) Close active document (Strg+F4)
> 				8492 단추 (disabled) Save file (Strg+S)
> 				8493 단추 Open project (Strg+O)
> 				8494 단추 (disabled) Save project changes (Strg+Shift+S)
> 				8495 단추 Close project
> 				8496 단추 (disabled) Print
> 				8497 단추 Cut (Strg+X)
> 				8498 단추 Copy (Strg+C)
> 				8499 단추 Paste (Strg+V)
> 				8500 메뉴 항목 (disabled) Undo (Strg+Z)
> 				8501 메뉴 항목 (disabled) Redo (Strg+Y)

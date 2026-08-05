>     "windows": [],
>     "displayName": "Advanced IP Scanner",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Qt\\Tools\\QtCreator\\bin\\qtcreator.exe",
>     "windows": [],
>     "displayName": "Qt Creator 18.0.1 (Community)",
>     "lastUsedDate": "2026-04-07",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "windows": [],
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "windows": [],
>     "displayName": "MDS Installation Guide",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\WSL\\wslsettings\\wslsettings.exe",
>     "windows": [],
>     "displayName": "WSL Settings",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 상태 확인",
>   "code": "globalThis.lasalApp = apps.find((app) => app.id === '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe'); if (!lasalApp) throw new Error('LASAL app not returned'); if (lasalApp.windows.length !== 1) throw new Error('Expected exactly one LASAL window; found '+lasalApp.windows.length); globalThis.lasalReturnedWindow = lasalApp.windows[0]; globalThis.targetWindow = await sky.get_window({id: lasalReturnedWindow.id, app: lasalReturnedWindow.app}); await sky.activate_window({window: targetWindow}); globalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot: true, include_text: true}); globalThis.targetWindow = state.window; nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280
> 				3 창 ID: 59648
> 					4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						5 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							6 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7 단추 페이지 위로 ID: UpPageButton
> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				18 창 ID: 59648
> 					19 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						20 단추 위쪽 스크롤 화살표 ID: UpButton
> 						21 위치 조정 위치 ID: ScrollbarThumb
> 						22 단추 페이지 아래로 ID: DownPageButton
> 						23 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					24 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						25 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						26 위치 조정 위치 ID: ScrollbarThumb
> 						27 단추 페이지 오른쪽으로 ID: DownPageButton
> 						28 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					29 위치 조정 (disabled)
> 			30 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				31 창 ID: 59648
> 					32 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						33 단추 위쪽 스크롤 화살표 ID: UpButton
> 						34 위치 조정 위치 ID: ScrollbarThumb
> 						35 단추 페이지 아래로 ID: DownPageButton
> 						36 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					37 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						38 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						39 위치 조정 위치 ID: ScrollbarThumb
> 						40 단추 페이지 오른쪽으로 ID: DownPageButton
> 						41 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					42 위치 조정 (disabled)
> 		43 상태 표시줄 ID: 59393
> 			44 텍스트
> 			45 텍스트 LMCDiagnosticsService::HandleEncoderMaintenanceStart
> 			46 텍스트
> 			47 텍스트 Ln 2634 Col 19
> 			48 텍스트
> 			49 텍스트 Offline
> 			50 텍스트
> 			51 텍스트 NUM
> 			52 텍스트
> 		53 창 xtpBarTop ID: 59419
> 			54 도구 모음 Script
> 			55 도구 모음 Edit
> 				56 단추 Toggle bookmark
> 				57 단추 (disabled) Previous bookmark
> 				58 단추 (disabled) Next bookmark
> 				59 단추 (disabled) Delete all bookmarks
> 				60 단추 (disabled) Previous bookmark in this file
> 				61 단추 (disabled) Next bookmark in this file
> 				62 단추 Comment selected text (Ctrl+Shift+C)
> 				63 단추 Remove comment (Ctrl+Shift+X)
> 				64 단추 Unindent (Shift+Tab)
> 				65 단추 Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				67 메뉴 항목 Macros
> 			68 도구 모음 Layout Manager
> 				69 메뉴 항목 Layouts
> 			70 도구 모음 Toolbox
> 				71 단추 DataAnalyzer
> 				72 메뉴 항목 Toolbar Options
> 			73 도구 모음 Net Edit
> 				74 단추 (disabled) Select
> 				75 메뉴 항목 Toolbar Options
> 			76 도구 모음 Debug
> 				77 단추 Go online (Alt+F6)
> 				78 단추 Change Online Settings
> 				79 메뉴 항목 Online Connection
> 				80 단추 (disabled) Set Online Connection For Current Project
> 				81 단추 (disabled) Download (F6)
> 				82 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				83 단추 (disabled) Download Module on the Fly
> 				84 단추 (disabled) Save Project on PLC
> 				85 단추 (disabled) Start (F7)
> 				86 단추 (disabled) Reset (F8)
> 				87 단추 Toggle breakpoint (F4)
> 				88 단추 Create condition breakpoint
> 				89 메뉴 항목 Toolbar Options
> 			90 도구 모음 Build
> 				91 메뉴 항목 Target Architecture
> 				92 단추 Build changes (F9)
> 				93 단추 Rebuild project (Strg+F9)
> 				94 단추 (disabled) Cancel building (Ctrl+Break)
> 				95 단추 Link project
> 			96 도구 모음 Standard
> 				97 단추 New project (Strg+N)
> 				98 단추 Open a file (Strg+Shift+O)
> 				99 단추 Close active document (Strg+F4)
> 				100 단추 (disabled) Save file (Strg+S)
> 				101 단추 Open project (Strg+O)
> 				102 단추 (disabled) Save project changes (Strg+Shift+S)
> 				103 단추 Close project
> 				104 단추 Print
> 				105 단추 Cut (Strg+X)
> 				106 단추 Copy (Strg+C)
> 				107 단추 (disabled) Paste (Strg+V)
> 				108 메뉴 항목 (disabled) Undo (Strg+Z)
> 				109 메뉴 항목 (disabled) Redo (Strg+Y)
> 				110 단추 Navigate Backward (Alt+Left)
> 				111 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				113 메뉴 항목 FILE
> 				114 메뉴 항목 EDIT
> 				115 메뉴 항목 VIEW
> 				116 메뉴 항목 PROJECT
> 				117 메뉴 항목 BUILD
> 				118 메뉴 항목 DEBUG
> 				119 메뉴 항목 ANALYZE
> 				120 메뉴 항목 TOOLS
> 				121 메뉴 항목 EXTRAS
> 				122 메뉴 항목 WINDOW
> 				123 메뉴 항목 HELP
> 		124 창 Splitter ID: 296801200
> 		125 창 Splitter ID: 296800528
> 		126 Tab Output ID: 295631256
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1201
> 						130 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							131 단추 위쪽 스크롤 화살표 ID: UpButton
> 							132 단추 페이지 위로 ID: UpPageButton
> 							133 위치 조정 위치 ID: ScrollbarThumb
> 							134 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						135 목록 항목 (selectable)
> 						136 목록 항목 (selectable)
> 						137 목록 항목 (selectable)
> 						138 목록 항목 (selectable)
> 						139 목록 항목 (selectable)
> 						140 목록 항목 (selectable)
> 						141 목록 항목 (selectable)
> 						142 목록 항목 (selectable)
> 						143 목록 항목 (selectable)
> 						144 목록 항목 (selectable)
> 						145 목록 항목 (selectable)
> 						146 목록 항목 (selectable)
> 						147 목록 항목 (selectable)
> 						148 목록 항목 (selectable)
> 						149 목록 항목 (selectable)
> 					150 스크롤 막대 ID: 59904
> 						151 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						152 위치 조정 위치 ID: ScrollbarThumb
> 						153 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			154 탭 항목 (selectable) Python Script
> 			155 탭 항목 (selectable) Debugger
> 			156 탭 항목 (selectable) Output
> 			157 단추 Close
> 		158 창 Splitter ID: 296801032
> 		159 Tab Network View ID: 295632624
> 			160 트리 ID: 104
> 				161 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					162 콘솔 트리 (selectable) Comm_Network
> 					163 콘솔 트리 (selectable) EtherCAT_Network
> 					164 콘솔 트리 (selectable) HW_Network
> 					165 콘솔 트리 (selectable) Motion_Network
> 			166 탭 항목 (selectable) Lib
> 			167 탭 항목 (selectable) File
> 			168 탭 항목 (selectable) Global
> 			169 탭 항목 (selectable) Net
> 			170 탭 항목 (selectable) Class
> 			171 단추 Close
> 		172 Tab Properties ID: 295635360
> 			173 창 ID: 287704144
> 				174 TABLE Properties Window ID: 293361080
> 				175 도구 모음 ID: 59392
> 					176 단추
> 					177 단추
> 			178 탭 항목 (selectable) Properties
> 			179 단추 Close
>
> The focused UI element is 4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> 현재 LASAL IDE가 canonical 프로젝트가 아니라 `C:\work\Elmo\Elmo_Master_test\...` 복제본을 열고 있는 것을 확인했습니다. 저장되지 않은 변경은 없으므로, 지금 canonical `C:\work\Elmo\Elmo_Master\Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcp`로 전환하겠습니다. 이 확인 없이 선언을 추가하면 잘못된 복제본을 수정하게 됩니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json

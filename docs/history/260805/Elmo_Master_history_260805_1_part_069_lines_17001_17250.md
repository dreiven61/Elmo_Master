> 					92 단추
> 					93 단추
> 			22552 탭 항목 (selectable) Properties
> 			95 단추 Close
>
> The focused UI element is 83 트리 ID: 103.
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
>   "title": "Diagnostics CommandId 타입 편집",
>   "code": "var obsTypeEdit=globalThis.state; var sidTypeEdit=obsTypeEdit.screenshots[0].id; globalThis.state=null; try{await sky.click({window:obsTypeEdit.window,screenshotId:sidTypeEdit,x:855,y:371,click_count:2}); globalThis.state=await sky.get_window_state({window:obsTypeEdit.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Type edit outcome unknown: '+String(error));} globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||state.accessibility?.tree||''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 83 트리 ID: 103
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
>   "title": "Diagnostics CommandId UINT 선택",
>   "code": "var obsTypeDropdown=globalThis.state; globalThis.state=null; try{await sky.type_text({window:obsTypeDropdown.window,text:'UINT'}); globalThis.state=await sky.get_window_state({window:obsTypeDropdown.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Type selection text outcome unknown: '+String(error));} globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.focused_element||''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 23191 편집
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
>   "title": "Diagnostics CommandId 타입 확정",
>   "code": "var obsTypeCommit=globalThis.state; globalThis.state=null; try{await sky.press_key({window:obsTypeCommit.window,key:'Return'}); globalThis.state=await sky.get_window_state({window:obsTypeCommit.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Type commit outcome unknown: '+String(error));} globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService* Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			7927 창 LMCDiagnosticsService* Secondary Actions: Raise ID: 65280
> 				7928 창 ID: 59648
> 					7929 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						7930 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							7931 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7932 단추 페이지 위로 ID: UpPageButton
> 							7933 위치 조정 위치 ID: ScrollbarThumb
> 							7934 단추 페이지 아래로 ID: DownPageButton
> 							7935 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						7936 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							7937 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							7938 위치 조정 위치 ID: ScrollbarThumb
> 							7939 단추 페이지 오른쪽으로 ID: DownPageButton
> 							7940 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						7941 위치 조정 (disabled)
> 			7942 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				7943 창 ID: 59648
> 					7944 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						7945 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7946 위치 조정 위치 ID: ScrollbarThumb
> 						7947 단추 페이지 아래로 ID: DownPageButton
> 						7948 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					7949 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						7950 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						7951 위치 조정 위치 ID: ScrollbarThumb
> 						7952 단추 페이지 오른쪽으로 ID: DownPageButton
> 						7953 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					7954 위치 조정 (disabled)
> 			7955 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				7956 창 ID: 59648
> 					7957 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						7958 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7959 위치 조정 위치 ID: ScrollbarThumb
> 						7960 단추 페이지 아래로 ID: DownPageButton
> 						7961 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					7962 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						7963 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						7964 위치 조정 위치 ID: ScrollbarThumb
> 						7965 단추 페이지 오른쪽으로 ID: DownPageButton
> 						7966 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					7967 위치 조정 (disabled)
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
> 				23310 단추 Toggle bookmark
> 				23311 단추 (disabled) Previous bookmark
> 				23312 단추 (disabled) Next bookmark
> 				23313 단추 (disabled) Delete all bookmarks
> 				23314 단추 (disabled) Previous bookmark in this file
> 				23315 단추 (disabled) Next bookmark in this file
> 				23316 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				23317 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				23318 단추 (disabled) Unindent (Shift+Tab)
> 				23319 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				23320 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				23321 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				23322 단추 DataAnalyzer
> 				23323 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				23324 단추 (disabled) Select
> 				23325 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				23326 단추 Go online (Alt+F6)
> 				23327 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				23328 메뉴 항목 Target Architecture
> 				23329 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				23330 단추 New project (Strg+N)
> 				23331 단추 Open a file (Strg+Shift+O)
> 				23332 단추 Close active document (Strg+F4)
> 				23333 단추 Save file (Strg+S)
> 				23334 단추 Open project (Strg+O)
> 				23335 단추 Save project changes (Strg+Shift+S)
> 				23336 단추 Close project
> 				23337 단추 Print
> 				23338 단추 Cut (Strg+X)
> 				23339 단추 Copy (Strg+C)
> 				23340 단추 Paste (Strg+V)
> 				23341 메뉴 항목 Undo (Strg+Z)
> 				23342 메뉴 항목 (disabled) Redo (Strg+Y)
> 				23343 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				23344 메뉴 항목 FILE
> 				23345 메뉴 항목 EDIT
> 				23346 메뉴 항목 VIEW
> 				23347 메뉴 항목 PROJECT
> 				23348 메뉴 항목 BUILD
> 				23349 메뉴 항목 DEBUG
> 				23350 메뉴 항목 ANALYZE
> 				23351 메뉴 항목 TOOLS
> 				23352 메뉴 항목 EXTRAS
> 				23353 메뉴 항목 WINDOW
> 				23354 메뉴 항목 HELP
> 		67 창 Splitter ID: 370324112
> 		68 창 Splitter ID: 370324280
> 		69 Tab Output ID: 369862944
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						7382 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							7383 단추 위쪽 스크롤 화살표 ID: UpButton
> 							7384 단추 페이지 위로 ID: UpPageButton
> 							7385 위치 조정 위치 ID: ScrollbarThumb
> 							7386 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						7844 목록 항목 (selectable)
> 						7897 목록 항목 (selectable)
> 						8013 목록 항목 (selectable)
> 						8014 목록 항목 (selectable)
> 						8015 목록 항목 (selectable)
> 						8016 목록 항목 (selectable)
> 						8017 목록 항목 (selectable)
> 						8018 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			23355 탭 항목 (selectable) Python Script
> 			23356 탭 항목 (selectable) Debugger
> 			23357 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 370325456
> 		82 Tab Class View ID: 369860664
> 			83 트리 ID: 103
> 				7901 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					7902 단추 위쪽 스크롤 화살표 ID: UpButton
> 					14531 단추 페이지 위로 ID: UpPageButton
> 					7903 위치 조정 위치 ID: ScrollbarThumb
> 					18831 단추 페이지 아래로 ID: DownPageButton
> 					7905 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				19310 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					19311 단추 왼쪽 스크롤 화살표 ID: UpButton
> 					19312 위치 조정 위치 ID: ScrollbarThumb
> 					19313 단추 페이지 오른쪽으로 ID: DownPageButton
> 					19314 단추 오른쪽 스크롤 화살표 ID: DownButton
> 				19315 위치 조정 (disabled)
> 				7906 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					7907 콘솔 트리 (selectable) External
> 					7908 콘솔 트리 (selectable) Sigmatek
> 					7909 콘솔 트리 (selectable) Elmo_1
> 					7910 콘솔 트리 (selectable) Elmo_2
> 					7911 콘솔 트리 (selectable) Elmo_3
> 					7912 콘솔 트리 (selectable) Elmo_4
> 					7913 콘솔 트리 (selectable) GL_9086_1
> 					7914 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					7915 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					7916 콘솔 트리 (selectable) LMCControlCommandService
> 					7917 콘솔 트리 (selectable) LMCDiagnosticsService
> 						18832 콘솔 트리 (selectable) Servers
> 						18833 콘솔 트리 (selectable) Clients
> 						18834 콘솔 트리 (selectable) Methods
> 							18835 콘솔 트리 (selectable) Global
> 							18836 콘솔 트리 (selectable) Private
> 								19316 콘솔 트리 (selectable) LMCDiagnosticsService
> 								19317 콘솔 트리 (selectable) IsSdoReadReady
> 								19318 콘솔 트리 (selectable) GetSdoWritePolicyDetail
> 								19319 콘솔 트리 (selectable) BuildCatalogEntry
> 								19320 콘솔 트리 (selectable) HandleEtherCATTopologyIoRequest
> 								19321 콘솔 트리 (selectable) HandleAxisDs402HomeStart
> 								19322 콘솔 트리 (selectable) HandleAxisDs402HomeOutcome
> 								19323 콘솔 트리 (selectable) HandleAxisDs402HomeRetire
> 								19324 콘솔 트리 (selectable) ProcessAxisDs402Home

> 					7922 콘솔 트리 (selectable) TCPMotionInterface
> 			24286 탭 항목 (selectable) Lib
> 			24287 탭 항목 (selectable) File
> 			24288 탭 항목 (selectable) Class
> 			87 단추 Close
> 		88 Tab Properties ID: 369857928
> 			89 창 ID: 261805944
> 				90 TABLE Properties Window ID: 272248624
> 					15315 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						15316 단추 위쪽 스크롤 화살표 ID: UpButton
> 						15317 위치 조정 위치 ID: ScrollbarThumb
> 						15318 단추 페이지 아래로 ID: DownPageButton
> 						15319 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					24289 custom Name
> 					24290 custom Virtual
> 					24291 custom Global access
> 					24292 custom AWL implementation
> 					24293 custom CDecl
> 					24294 custom Comment
> 				91 도구 모음 ID: 59392
> 					92 단추
> 					93 단추
> 			24295 탭 항목 (selectable) Properties
> 			95 단추 Close
>
> The focused UI element is 19330 콘솔 트리 (selectable) HandleDiagnosticsBulkRequest.
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
>   "title": "Diagnostics 메서드 상세 편집",
>   "code": "var obsEditMethod=globalThis.state; var sidEditMethod=obsEditMethod.screenshots[0].id; globalThis.state=null; try{await sky.click({window:obsEditMethod.window,screenshotId:sidEditMethod,x:920,y:150}); globalThis.state=await sky.get_window_state({window:obsEditMethod.window,include_screenshot:true,include_text:true});}catch(error){throw new Error('Edit Method action outcome unknown: '+String(error));} globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService*", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService* Secondary Actions: Raise
> 		24296 창
> 		24297 창
> 		24298 메뉴
> 			24827 메뉴 항목 Edit Method Enter
> 			24828 메뉴 항목 Delete Method Del
> 			24829 메뉴 항목 Move Up
> 			24830 메뉴 항목 (disabled) Move Down
> 			24831 메뉴 항목 Copy
> 			24832 메뉴 항목 New Input Variable
> 			24833 메뉴 항목 New Output Variable
> 			24834 메뉴 항목 Add to Newinst
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
> 				24835 단추 Toggle bookmark
> 				24836 단추 (disabled) Previous bookmark
> 				24837 단추 (disabled) Next bookmark
> 				24838 단추 (disabled) Delete all bookmarks
> 				24839 단추 (disabled) Previous bookmark in this file
> 				24840 단추 (disabled) Next bookmark in this file
> 				24841 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				24842 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				24843 단추 (disabled) Unindent (Shift+Tab)
> 				24844 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				24845 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				24846 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				24847 단추 DataAnalyzer
> 				24848 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				24849 단추 (disabled) Select
> 				24850 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				24851 단추 Go online (Alt+F6)
> 				24852 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				24853 메뉴 항목 Target Architecture
> 				24854 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				24855 단추 New project (Strg+N)
> 				24856 단추 Open a file (Strg+Shift+O)
> 				24857 단추 Close active document (Strg+F4)
> 				24858 단추 Save file (Strg+S)
> 				24859 단추 Open project (Strg+O)
> 				24860 단추 Save project changes (Strg+Shift+S)
> 				24861 단추 Close project
> 				24862 단추 Print
> 				24863 단추 Cut (Strg+X)
> 				24864 단추 Copy (Strg+C)
> 				24865 단추 Paste (Strg+V)
> 				24866 메뉴 항목 Undo (Strg+Z)
> 				24867 메뉴 항목 (disabled) Redo (Strg+Y)
> 				24868 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				24869 메뉴 항목 FILE
> 				24870 메뉴 항목 EDIT
> 				24871 메뉴 항목 VIEW
> 				24872 메뉴 항목 PROJECT
> 				24873 메뉴 항목 BUILD
> 				24874 메뉴 항목 DEBUG
> 				24875 메뉴 항목 ANALYZE
> 				24876 메뉴 항목 TOOLS
> 				24877 메뉴 항목 EXTRAS
> 				24878 메뉴 항목 WINDOW
> 				24879 메뉴 항목 HELP
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
> 			24880 탭 항목 (selectable) Python Script
> 			24881 탭 항목 (selectable) Debugger
> 			24882 탭 항목 (selectable) Output
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
> 								19325 콘솔 트리 (selectable) HandleDiagnosticsCapabilities
> 								19326 콘솔 트리 (selectable) HandleEncoderMaintenanceStart
> 								19327 콘솔 트리 (selectable) HandleEncoderMaintenanceOutcome
> 								19328 콘솔 트리 (selectable) HandleEncoderMaintenanceRetire
> 								19329 콘솔 트리 (selectable) ProcessEncoderMaintenance
> 								19330 콘솔 트리 (selectable) HandleDiagnosticsBulkRequest
> 									22542 콘솔 트리 (selectable) CommandId
> 						18838 콘솔 트리 (selectable) Variables
> 						18839 콘솔 트리 (selectable) Objects
> 						18840 콘솔 트리 (selectable) Dependencies
> 					7918 콘솔 트리 (selectable) LMCEcatInputLatch

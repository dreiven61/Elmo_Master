> 							7384 단추 페이지 위로 ID: UpPageButton
> 							7385 위치 조정 위치 ID: ScrollbarThumb
> 							7386 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						7733 목록 항목 (selectable)
> 						7734 목록 항목 (selectable)
> 						7735 목록 항목 (selectable)
> 						7736 목록 항목 (selectable)
> 						7737 목록 항목 (selectable)
> 						7738 목록 항목 (selectable)
> 						7791 목록 항목 (selectable)
> 						7844 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			7845 탭 항목 (selectable) Python Script
> 			7846 탭 항목 (selectable) Debugger
> 			7847 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 370325456
> 		82 Tab Class View ID: 369860664
> 			83 트리 (disabled) ID: 103
> 			7848 탭 항목 (selectable) Lib
> 			7849 탭 항목 (selectable) File
> 			7850 탭 항목 (selectable) Class
> 			87 단추 Close
> 		88 Tab Properties ID: 369857928
> 			89 창 (disabled) ID: 261805944
> 				90 TABLE Properties Window ID: 272248624
> 				91 도구 모음 ID: 59392
> 					92 단추
> 					93 단추
> 			7851 탭 항목 (selectable) Properties
> 			95 단추 Close
>
> The focused UI element is 1 창 작업 영역 ID: 59648.
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
>   "title": "LASAL 검색 메뉴 확인",
>   "code": "var obsEditMenu = globalThis.state; var sidEditMenu = obsEditMenu.screenshots[0].id; globalThis.state=null; try { await sky.click({window:obsEditMenu.window,screenshotId:sidEditMenu,x:88,y:42}); globalThis.state=await sky.get_window_state({window:obsEditMenu.window,include_screenshot:true,include_text:true}); } catch(error){ throw new Error('Edit menu action outcome unknown: '+String(error)); } globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			7927 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280
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
> 			4 텍스트 LMCDiagnosticsService::HandleEncoderMaintenanceStart
> 			5 텍스트
> 			6 텍스트 Ln 2657 Col 1
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				10730 단추 Toggle bookmark
> 				10731 단추 (disabled) Previous bookmark
> 				10732 단추 (disabled) Next bookmark
> 				10733 단추 (disabled) Delete all bookmarks
> 				10734 단추 (disabled) Previous bookmark in this file
> 				10735 단추 (disabled) Next bookmark in this file
> 				10736 단추 Comment selected text (Ctrl+Shift+C)
> 				10737 단추 Remove comment (Ctrl+Shift+X)
> 				10738 단추 Unindent (Shift+Tab)
> 				10739 단추 Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				10740 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				10741 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				10742 단추 DataAnalyzer
> 				10743 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				10744 단추 (disabled) Select
> 				10745 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				10746 단추 Go online (Alt+F6)
> 				10747 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				10748 메뉴 항목 Target Architecture
> 				10749 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				10750 단추 New project (Strg+N)
> 				10751 단추 Open a file (Strg+Shift+O)
> 				10752 단추 Close active document (Strg+F4)
> 				10753 단추 (disabled) Save file (Strg+S)
> 				10754 단추 Open project (Strg+O)
> 				10755 단추 (disabled) Save project changes (Strg+Shift+S)
> 				10756 단추 Close project
> 				10757 단추 Print
> 				10758 단추 Cut (Strg+X)
> 				10759 단추 Copy (Strg+C)
> 				10760 단추 (disabled) Paste (Strg+V)
> 				10761 메뉴 항목 (disabled) Undo (Strg+Z)
> 				10762 메뉴 항목 (disabled) Redo (Strg+Y)
> 				10763 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				10764 메뉴 항목 FILE
> 				10765 메뉴 항목 EDIT
> 				10766 메뉴 항목 VIEW
> 				10767 메뉴 항목 PROJECT
> 				10768 메뉴 항목 BUILD
> 				10769 메뉴 항목 DEBUG
> 				10770 메뉴 항목 ANALYZE
> 				10771 메뉴 항목 TOOLS
> 				10772 메뉴 항목 EXTRAS
> 				10773 메뉴 항목 WINDOW
> 				10774 메뉴 항목 HELP
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
> 			10775 탭 항목 (selectable) Python Script
> 			10776 탭 항목 (selectable) Debugger
> 			10777 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 370325456
> 		82 Tab Class View ID: 369860664
> 			83 트리 ID: 103
> 				7901 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					7902 단추 위쪽 스크롤 화살표 ID: UpButton
> 					7903 위치 조정 위치 ID: ScrollbarThumb
> 					7904 단추 페이지 아래로 ID: DownPageButton
> 					7905 단추 아래쪽 스크롤 화살표 ID: DownButton
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
> 					7918 콘솔 트리 (selectable) LMCEcatInputLatch
> 					7919 콘솔 트리 (selectable) LMCRecorderStore
> 					7920 콘솔 트리 (selectable) LMCSdoExecutor
> 					7921 콘솔 트리 (selectable) TCPIPServer
> 					7922 콘솔 트리 (selectable) TCPMotionInterface
> 			10778 탭 항목 (selectable) Lib
> 			10779 탭 항목 (selectable) File
> 			10780 탭 항목 (selectable) Class
> 			87 단추 Close
> 		88 Tab Properties ID: 369857928
> 			89 창 ID: 261805944
> 				90 TABLE Properties Window ID: 272248624
> 				91 도구 모음 ID: 59392
> 					92 단추
> 					93 단추
> 			10781 탭 항목 (selectable) Properties
> 			95 단추 Close
> 		10782 메뉴 Edit
> 			10783 메뉴 항목 (disabled) Undo Ctrl+Z
> 			10784 메뉴 항목 (disabled) Redo Ctrl+Y
> 			10785 메뉴 항목 Cut Ctrl+X
> 			10786 메뉴 항목 Copy Ctrl+C
> 			10787 메뉴 항목 (disabled) Paste Ctrl+V
> 			10788 메뉴 항목 Find... Ctrl+F
> 			10789 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			10790 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			10791 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			10792 메뉴 항목 Replace... Ctrl+R
> 			10793 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			10794 메뉴 항목 Goto Line... Ctrl+G
> 			10795 메뉴 항목 Reset Editor Colors
> 			10796 메뉴 항목 Mark
> 			10797 메뉴 항목 Bookmark
> 			10798 메뉴 항목 (disabled) Navigate Backward Alt+Left Arrow
> 			10799 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			10800 메뉴 항목 Select All Ctrl+A
> 			10801 메뉴 항목 Comment
> 			10802 메뉴 항목 Dent
>

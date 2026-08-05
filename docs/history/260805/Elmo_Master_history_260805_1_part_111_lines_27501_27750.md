> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			3068 탭 항목 (selectable) Python Script
> 			3069 탭 항목 (selectable) Debugger
> 			3070 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 371773352
> 		82 Tab Class View ID: 274609808
> 			83 트리 (disabled) ID: 103
> 			3071 탭 항목 (selectable) Lib
> 			3072 탭 항목 (selectable) File
> 			3073 탭 항목 (selectable) Class
> 			3074 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274603880
> 			90 창 (disabled) ID: 261121536
> 				91 TABLE Properties Window ID: 272349640
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			3075 탭 항목 (selectable) Properties
> 			96 단추 Close
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
>   "title": "Finish LASAL project load",
>   "code": "await new Promise(r=>setTimeout(r,3000)); globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||\"\"));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			3152 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65280
> 				3153 창 ID: 59648
> 					3154 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						3155 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							3156 단추 위쪽 스크롤 화살표 ID: UpButton
> 							3157 단추 페이지 위로 ID: UpPageButton
> 							3158 위치 조정 위치 ID: ScrollbarThumb
> 							3159 단추 페이지 아래로 ID: DownPageButton
> 							3160 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						3161 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							3162 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							3163 위치 조정 위치 ID: ScrollbarThumb
> 							3164 단추 페이지 오른쪽으로 ID: DownPageButton
> 							3165 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						3166 위치 조정 (disabled)
> 			3167 창 LMCControlCommandService Secondary Actions: Raise ID: 65283
> 				3168 창 ID: 59648
> 					3169 창 #define LMC_ADMIN_AXIS_HOME_ENABLED FALSE #define LMC_AXIS_STATUS_STANDSTILL 0x02000000 #define LMC_HOME_RECORD_EMPTY 0 #define LMC_HOME_RECORD_RUNNING 1 #define LMC_HOME_RECORD_SUCCEEDED 2 #define LMC_HOME_RECORD_FAILED 3 #define LMC_HOME_RECORD_ABORTED 4 #define LMC_HOME_RECORD_QUARANTINED 5 #define LMC_HOME_ENGINE_IDLE 0 #define LMC_HOME_ENGINE_WAIT_RT 1 #define LMC_HOME_ENGINE_TERMINAL 2 #define LMC_HOME_RECORD_MAGIC 0x4C4D4348 #define LMC_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_GLOBAL_SLOTS 28 #define LMC_OWNER_AXIS_STRIDE 36 #define LMC_OWNER_AXIS_COUNT 9 #define LMC_OWNER_TABLE_MAGIC 0x4C4D434F #define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00 #define LMC_OWNER_STATE_IDLE 0 #define LMC_OWNER_STATE_RESERVED 1 #define LMC_OWNER_STATE_DIRECT_ACTIVE 2 #define LMC_OWNER_STATE_GROUP_LEASE 3 #define LMC_OWNER_STATE_GROUP_ACTIVE 4 #define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5 #define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6 #define LMC_OWNER_STATE_TW20_QUEUED 7 #define LMC_OWNER_STATE_TW20_RUNNING 8 #define LMC_OWNER_STATE_TW20_DRAINING 9 #define LMC_OWNER_STATE_SAFETY_PREEMPTING 10 #define LMC_OWNER_STATE_QUARANTINED 11 #define LMC_OWNER_KIND_DIRECT 1 #define LMC_OWNER_KIND_GROUP 2 #define LMC_OWNER_KIND_LMC_HOME 3 #define LMC_OWNER_KIND_DS402_HOME 4 #define LMC_OWNER_KIND_ENCODER 5 #define LMC_OWNER_RESOURCE_AXIS 1 #define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2 #define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3 #define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4 #define LMC_OWNER_ADMISSION_ORDINARY 1 #define LMC_OWNER_ADMISSION_SAFETY 2 #define LMC_OWNER_ADMISSION_READ 3 #define LMC_OWNER_ADMISSION_LIFECYCLE 4 #define LMC_OWNER_REPORT_DISPATCH 1 #define LMC_OWNER_REPORT_TERMINAL_SUCCESS 2 #define LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE 3 #define LMC_OWNER_REPORT_QUARANTINE 4 #define LMC_OWNER_REPORT_SAFETY_PREEMPT 5 #define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001 #define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; CallerSessionEpoch : UDINT; RequestSequence : UDINT; AdmissionToken : UDINT; OwnerGeneration : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; if (pRequestFrame = NIL) | (pResponseFrame = NIL) | (RequestFrameSize < 8) then RETURN; end_if; // Slots 16..21 are a synchronous call context, not retained ownership. OwnershipState[16]$UDINT := CallerSessionEpoch; OwnershipState[17]$UDINT := RequestSequence; OwnershipState[18]$UDINT := AdmissionToken; OwnershipState[19]$UDINT := OwnerGeneration; OwnershipState[20] := TO_DINT(CommandId); OwnershipState[21] := TO_DINT(Reference); case CommandId of 0x103C, 0x1042, 0x202B: ResponseSize := HandleRegistryCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x2023, 0x2024, 0x2022, 0x2028, 0x202E, 0x209F, 0x20A0, 0x20A2: ResponseSize := HandleAxisCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x20D2, 0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085, 0x20A4, 0x2045, 0x2051, 0x20E7: ResponseSize := HandleGroupCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); 0x7D00, 0x7D10, 0x7D12, 0x7D13, 0x7D18, 0x7D19, 0x7D20, 0x7D22: ResponseSize := HandleAdminCommands( CommandId:=CommandId, Reference:=Reference, pRequestFrame:=pRequestFrame, RequestFrameSize:=RequestFrameSize, pResponseFrame:=pResponseFrame, ResponseCapacity:=ResponseCapacity); else ResponseSize := -1; end_ ID: 10000
> 						3170 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							3171 단추 위쪽 스크롤 화살표 ID: UpButton
> 							3172 단추 페이지 위로 ID: UpPageButton
> 							3173 위치 조정 위치 ID: ScrollbarThumb
> 							3174 단추 페이지 아래로 ID: DownPageButton
> 							3175 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						3176 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							3177 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							3178 위치 조정 위치 ID: ScrollbarThumb
> 							3179 단추 페이지 오른쪽으로 ID: DownPageButton
> 							3180 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						3181 위치 조정 (disabled)
> 			3182 창 Motion_Network Secondary Actions: Raise ID: 65282
> 				3183 창 ID: 59648
> 					3184 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						3185 단추 위쪽 스크롤 화살표 ID: UpButton
> 						3186 위치 조정 위치 ID: ScrollbarThumb
> 						3187 단추 페이지 아래로 ID: DownPageButton
> 						3188 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					3189 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						3190 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						3191 위치 조정 위치 ID: ScrollbarThumb
> 						3192 단추 페이지 오른쪽으로 ID: DownPageButton
> 						3193 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					3194 위치 조정 (disabled)
> 			3195 창 Comm_Network Secondary Actions: Raise ID: 65281
> 				3196 창 ID: 59648
> 					3197 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						3198 단추 위쪽 스크롤 화살표 ID: UpButton
> 						3199 위치 조정 위치 ID: ScrollbarThumb
> 						3200 단추 페이지 아래로 ID: DownPageButton
> 						3201 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					3202 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						3203 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						3204 위치 조정 위치 ID: ScrollbarThumb
> 						3205 단추 페이지 오른쪽으로 ID: DownPageButton
> 						3206 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					3207 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트 LMCDiagnosticsService::HandleDiagnosticsBulkRequest
> 			5 텍스트
> 			6 텍스트 Ln 5241 Col 1
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				3744 단추 Toggle bookmark
> 				3745 단추 (disabled) Previous bookmark
> 				3746 단추 (disabled) Next bookmark
> 				3747 단추 (disabled) Delete all bookmarks
> 				3748 단추 (disabled) Previous bookmark in this file
> 				3749 단추 (disabled) Next bookmark in this file
> 				3750 단추 Comment selected text (Ctrl+Shift+C)
> 				3751 단추 Remove comment (Ctrl+Shift+X)
> 				3752 단추 Unindent (Shift+Tab)
> 				3753 단추 Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				3754 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				3755 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				3756 단추 DataAnalyzer
> 				3757 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				3758 단추 (disabled) Select
> 				3759 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				3760 단추 Go online (Alt+F6)
> 				3761 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				3762 메뉴 항목 Target Architecture
> 				3763 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				3764 단추 New project (Strg+N)
> 				3765 단추 Open a file (Strg+Shift+O)
> 				3766 단추 Close active document (Strg+F4)
> 				3767 단추 (disabled) Save file (Strg+S)
> 				3768 단추 Open project (Strg+O)
> 				3769 단추 (disabled) Save project changes (Strg+Shift+S)
> 				3770 단추 Close project
> 				3771 단추 Print
> 				3772 단추 Cut (Strg+X)
> 				3773 단추 Copy (Strg+C)
> 				3774 단추 (disabled) Paste (Strg+V)
> 				3775 메뉴 항목 (disabled) Undo (Strg+Z)
> 				3776 메뉴 항목 (disabled) Redo (Strg+Y)
> 				3777 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				3778 메뉴 항목 FILE
> 				3779 메뉴 항목 EDIT
> 				3780 메뉴 항목 VIEW
> 				3781 메뉴 항목 PROJECT
> 				3782 메뉴 항목 BUILD
> 				3783 메뉴 항목 DEBUG
> 				3784 메뉴 항목 ANALYZE
> 				3785 메뉴 항목 TOOLS
> 				3786 메뉴 항목 EXTRAS
> 				3787 메뉴 항목 WINDOW
> 				3788 메뉴 항목 HELP
> 		67 창 Splitter ID: 371772512
> 		68 창 Splitter ID: 371770328
> 		69 Tab Output ID: 274603424
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						2594 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							2595 단추 위쪽 스크롤 화살표 ID: UpButton
> 							2596 단추 페이지 위로 ID: UpPageButton
> 							2597 위치 조정 위치 ID: ScrollbarThumb
> 							2598 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						3014 목록 항목 (selectable)
> 						3121 목록 항목 (selectable)
> 						3253 목록 항목 (selectable)
> 						3254 목록 항목 (selectable)
> 						3255 목록 항목 (selectable)
> 						3256 목록 항목 (selectable)
> 						3257 목록 항목 (selectable)
> 						3258 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			3789 탭 항목 (selectable) Python Script
> 			3790 탭 항목 (selectable) Debugger
> 			3791 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 371773352
> 		82 Tab Class View ID: 274609808
> 			83 트리 ID: 103
> 				3125 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					3126 단추 위쪽 스크롤 화살표 ID: UpButton
> 					3127 위치 조정 위치 ID: ScrollbarThumb
> 					3128 단추 페이지 아래로 ID: DownPageButton
> 					3129 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				3130 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					3131 콘솔 트리 (selectable) External
> 					3132 콘솔 트리 (selectable) Sigmatek
> 					3133 콘솔 트리 (selectable) Elmo_1
> 					3134 콘솔 트리 (selectable) Elmo_2
> 					3135 콘솔 트리 (selectable) Elmo_3
> 					3136 콘솔 트리 (selectable) Elmo_4
> 					3137 콘솔 트리 (selectable) GL_9086_1
> 					3138 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					3139 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					3140 콘솔 트리 (selectable) LMCControlCommandService
> 					3141 콘솔 트리 (selectable) LMCDiagnosticsService
> 					3142 콘솔 트리 (selectable) LMCEcatInputLatch
> 					3143 콘솔 트리 (selectable) LMCRecorderStore
> 					3144 콘솔 트리 (selectable) LMCSdoExecutor
> 					3145 콘솔 트리 (selectable) TCPIPServer
> 					3146 콘솔 트리 (selectable) TCPMotionInterface
> 			3792 탭 항목 (selectable) Lib
> 			3793 탭 항목 (selectable) File
> 			3794 탭 항목 (selectable) Class
> 			3795 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274603880
> 			90 창 ID: 261121536
> 				91 TABLE Properties Window ID: 272349640
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			3796 탭 항목 (selectable) Properties
> 			96 단추 Close
>
> The focused UI element is 3154 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 반영했습니다. 평일은 17:30~다음 날 08:30, 토·일·공휴일은 전 시간 LASAL IDE를 제가 직접 제어하겠습니다. 지금은 허용 시간대이므로, 시작 소유권 `0xF` 완전 증명 경로부터 계속 구현하겠습니다.
>
> 설계 검토 결과, 단순 플래그 읽기 방식은 RT 메일박스가 찢어진 상태로 보일 수 있어 폐기합니다. `InputLatch`의 기존 seqlock 안에 48바이트 시작 증거 스냅샷을 넣고, `Control` 한 곳에서 서로 다른 RT 주기 3개를 판정하는 구조로 확정하겠습니다. 기존 TCP의 BootId-only 보고 함수도 제거 대상입니다.
>
> <details><summary>Edited a file, called 4 tools</summary>

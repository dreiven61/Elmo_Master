> 					2930 콘솔 트리 (selectable) Elmo_3
> 					2931 콘솔 트리 (selectable) Elmo_4
> 					2932 콘솔 트리 (selectable) GL_9086_1
> 					2933 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					2934 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					2935 콘솔 트리 (selectable) LMCControlCommandService
> 					2936 콘솔 트리 (selectable) LMCDiagnosticsService
> 					2937 콘솔 트리 (selectable) LMCEcatInputLatch
> 					2938 콘솔 트리 (selectable) LMCRecorderStore
> 					2939 콘솔 트리 (selectable) LMCSdoExecutor
> 					2940 콘솔 트리 (selectable) TCPIPServer
> 					2941 콘솔 트리 (selectable) TCPMotionInterface
> 			13010 탭 항목 (selectable) Lib
> 			13011 탭 항목 (selectable) File
> 			13012 탭 항목 (selectable) Class
> 			13013 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 379754464
> 			90 창 ID: 269687312
> 				91 TABLE Properties Window ID: 271397096
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			13014 탭 항목 (selectable) Properties
> 			96 단추 Close
>
> The focused UI element is 2811 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> 최신 observer/marker 소스의 C78 ARM Rebuild가 통과했습니다: `0 errors, 42 warnings`. 경고는 구 컴파일러 C78 및 C81 라이브러리 버전 차이 계열입니다. 이제 구현 검색 2건과 새 `CInvalidArgException` 부재를 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open LASAL implementation search",
>   "code": "var obsFind1=state; state=null; await sky.press_key({window:obsFind1.window,key:'Control_L+Shift_L+f'}); state=await sky.get_window_state({window:obsFind1.window,include_screenshot:true,include_text:true}); targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||''));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2809 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				2810 창 ID: 59648
> 					2811 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED FALSE #define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED FALSE // Implementation remains dormant until RT mailbox, setpoint alignment and // no-jump CSP return have been proven on every enabled physical axis. #define LMC_DIAG_DS402_HOME_ENABLED FALSE #define LMC_DIAG_ENCODER_TW20_ENABLED FALSE #define LMC_DIAG_ENCODER_TW19_ENABLED FALSE // Axis-specific compatibility manifests remain unprovisioned until current // live drive, encoder family, feedback socket and evidence have been captured. // Enabling a global feature gate without provisioning the exact matching // manifest still fails closed before any SDO executor call. #define LMC_DIAG_ENCODER_TW20_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW20_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS1_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS2_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS3_EVIDENCE3 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_PROFILE 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_SOCKET 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE0 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE1 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE2 0 #define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 0 #define LMC_DIAG_ENCODER_RECORD_STRIDE 38 #define LMC_DIAG_ENCODER_RUNTIME_BASE 152 #define LMC_DIAG_ENCODER_STAGE_IDLE 0 #define LMC_DIAG_ENCODER_STAGE_PRE_STABLE 1 #define LMC_DIAG_ENCODER_STAGE_DISPATCH 2 #define LMC_DIAG_ENCODER_STAGE_WAIT_SDO 3 #define LMC_DIAG_ENCODER_STAGE_POST_STABLE 4 #define LMC_DIAG_ENCODER_STAGE_RELEASE_OWNER 5 #define LMC_DIAG_ENCODER_STAGE_DRAIN 90 #define LMC_DIAG_ENCODER_STAGE_QUARANTINED 101 #define LMC_DIAG_ENCODER_RECORD_RUNNING 1 #define LMC_DIAG_ENCODER_RECORD_SUCCEEDED 2 #define LMC_DIAG_ENCODER_RECORD_FAILED 3 #define LMC_DIAG_ENCODER_RECORD_ABORTED 4 #define LMC_D ID: 10000
> 						2812 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							2813 단추 위쪽 스크롤 화살표 ID: UpButton
> 							2814 단추 페이지 위로 ID: UpPageButton
> 							2815 위치 조정 위치 ID: ScrollbarThumb
> 							2816 단추 페이지 아래로 ID: DownPageButton
> 							2817 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						2818 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							2819 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							2820 위치 조정 위치 ID: ScrollbarThumb
> 							2821 단추 페이지 오른쪽으로 ID: DownPageButton
> 							2822 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						2823 위치 조정 (disabled)
> 			2824 창 LMCEcatInputLatch Secondary Actions: Raise ID: 65282
> 				2825 창 ID: 59648
> 					2826 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib #define LMC_ZERO_HOME_STATE_RUNNING 1 #define LMC_ZERO_HOME_STATE_SUCCEEDED 2 #define LMC_ZERO_HOME_STATE_FAILED 3 #define LMC_ZERO_HOME_PHASE_VERIFY 1 #define LMC_ZERO_HOME_REQUIRED_STABLE 3 #define LMC_ZERO_HOME_FAILURE_INVALID -1 #define LMC_ZERO_HOME_FAILURE_BUSY -2 #define LMC_ZERO_HOME_FAILURE_CLIENT -3 #define LMC_ZERO_HOME_FAILURE_STATE -4 #define LMC_ZERO_HOME_FAILURE_STALE -5 #define LMC_ZERO_HOME_FAILURE_NATIVE -6 #define LMC_ZERO_HOME_FAILURE_VERIFY -7 #define LMC_ZERO_HOME_FAILURE_CORRUPT -8 #define LMC_ZERO_HOME_FAILURE_DS402 -9 #define LMC_ZERO_HOME_STANDSTILL 0x02000000 #define LMC_ZERO_HOME_EVIDENCE_EXPECTED 0x00000001 #define LMC_ZERO_HOME_EVIDENCE_STATE 0x00000002 #define LMC_ZERO_HOME_EVIDENCE_RAW 0x00000004 #define LMC_ZERO_HOME_EVIDENCE_APP 0x00000008 #define LMC_ZERO_HOME_EVIDENCE_INTERNAL 0x00000010 #define LMC_ZERO_HOME_EVIDENCE_STABLE 0x00000020 #define LMC_ZERO_HOME_EVIDENCE_DISPATCH 0x00000003 #define LMC_ZERO_HOME_EVIDENCE_VERIFIED 0x0000001F #define LMC_ZERO_HOME_EVIDENCE_COMPLETE 0x0000003F #define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353 #define LMC_OWNER_STARTUP_LATCH_PHYSICAL 0x00000001 #define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002 #define LMC_OWNER_STARTUP_LATCH_DS402 0x00000004 #define LMC_OWNER_STARTUP_LATCH_OWNER 0x00000008 #define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010 FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; couplerConnected : BOOL; inputSlotConnected : BOOL; outputSlotConnected : BOOL; couplerDetected : BOOL; couplerIdentityMatched : BOOL; couplerDataValid : BOOL; inputDetected : BOOL; inputIdentityMatched : BOOL; inputDataValid : BOOL; outputDetected : BOOL; outputIdentityMatched : BOOL; outputDataValid : BOOL; inputValid : BOOL; outputValid : BOOL; couplerStateChanged : BOOL; inputStateChanged : BOOL; outputStateChanged : BOOL; couplerNativeOnline : DINT; inputStatus : UINT; outputStatus : UINT; couplerEtherCATState : UDINT; couplerSlaveState : UDINT; couplerALStatus : UDINT; couplerClassState : UDINT; inputSlotClassState : UDINT; outputSlotClassState : UDINT; inputByte0Value : UDINT; inputByte1Value : UDINT; inputByte2Value : UDINT; inputByte3Value : UDINT; outputByte0Value : UDINT; outputByte1Value : UDINT; outputByte2Value : UDINT; outputByte3Value : UDINT; inputValue : UDINT; outputValue : UDINT; inputValidMask : UDINT; outputValidMask : UDINT; couplerLastValidCycle : UDINT; couplerLastStateChangeCycle : UDINT; inputLastValidCycle : UDINT; inputLastStateChangeCycle : UDINT; outputLastValidCycle : UDINT; outputLastStateChangeCycle : UDINT; zeroHomeRequestSequence : UDINT; zeroHomeAppliedSequence : UDINT; zeroHomeRequestToken : UDINT; zeroHomeRequestAxis : DINT; zeroHomeExpectedActualPosition : DINT; zeroHomePhase : DINT; zeroHomeStableSampleCount : DINT; zeroHomeFailure : DINT; zeroHomeCycle : UDINT; zeroHomeEvidence : UDINT; zeroHomeHasRequest : BOOL; zeroHomeTerminal : BOOL; zeroHomeSucceeded : BOOL; zeroHomeDriveConnected : BOOL; zeroHomeAxisConnected : BOOL; zeroHomeAxisStatus : _LMCAXIS_STATUS; zeroHomeAxisError : _LMCAXIS_ERROR; zeroHomeRawDrivePosition : DINT; zeroHomeActualAppPosition : DINT; zeroHomeSetAppPosition : DINT; zeroHomeActualIntPositio ID: 10000
> 						2827 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							2828 단추 위쪽 스크롤 화살표 ID: UpButton
> 							2829 단추 페이지 위로 ID: UpPageButton
> 							2830 위치 조정 위치 ID: ScrollbarThumb
> 							2831 단추 페이지 아래로 ID: DownPageButton
> 							2832 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						2833 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							2834 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							2835 위치 조정 위치 ID: ScrollbarThumb
> 							2836 단추 페이지 오른쪽으로 ID: DownPageButton
> 							2837 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						2838 위치 조정 (disabled)
> 			2839 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				2840 창 ID: 59648
> 					2841 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						2842 단추 위쪽 스크롤 화살표 ID: UpButton
> 						2843 위치 조정 위치 ID: ScrollbarThumb
> 						2844 단추 페이지 아래로 ID: DownPageButton
> 						2845 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					2846 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						2847 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						2848 위치 조정 위치 ID: ScrollbarThumb
> 						2849 단추 페이지 오른쪽으로 ID: DownPageButton
> 						2850 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					2851 위치 조정 (disabled)
> 			2852 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				2853 창 ID: 59648
> 					2854 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						2855 단추 위쪽 스크롤 화살표 ID: UpButton
> 						2856 위치 조정 위치 ID: ScrollbarThumb
> 						2857 단추 페이지 아래로 ID: DownPageButton
> 						2858 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					2859 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						2860 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						2861 위치 조정 위치 ID: ScrollbarThumb
> 						2862 단추 페이지 오른쪽으로 ID: DownPageButton
> 						2863 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					2864 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트 LMCDiagnosticsService::ProcessAxisOwnershipStartup
> 			5 텍스트
> 			6 텍스트 Ln 5592 Col 1
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				14022 단추 Toggle bookmark
> 				14023 단추 (disabled) Previous bookmark
> 				14024 단추 (disabled) Next bookmark
> 				14025 단추 (disabled) Delete all bookmarks
> 				14026 단추 (disabled) Previous bookmark in this file
> 				14027 단추 (disabled) Next bookmark in this file
> 				14028 단추 Comment selected text (Ctrl+Shift+C)
> 				14029 단추 Remove comment (Ctrl+Shift+X)
> 				14030 단추 Unindent (Shift+Tab)
> 				14031 단추 Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				14032 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				14033 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				14034 단추 DataAnalyzer
> 				14035 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				14036 단추 (disabled) Select
> 				14037 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				14038 단추 Go online (Alt+F6)
> 				14039 메뉴 항목 Toolbar Options
> 			37 도구 모음 Build
> 				14040 메뉴 항목 Target Architecture
> 				14041 메뉴 항목 Toolbar Options
> 			40 도구 모음 Standard
> 				14042 단추 New project (Strg+N)
> 				14043 단추 Open a file (Strg+Shift+O)
> 				14044 단추 Close active document (Strg+F4)
> 				14045 단추 (disabled) Save file (Strg+S)
> 				14046 단추 Open project (Strg+O)
> 				14047 단추 (disabled) Save project changes (Strg+Shift+S)
> 				14048 단추 Close project
> 				14049 단추 Print
> 				14050 단추 Cut (Strg+X)
> 				14051 단추 Copy (Strg+C)
> 				14052 단추 (disabled) Paste (Strg+V)
> 				14053 메뉴 항목 (disabled) Undo (Strg+Z)
> 				14054 메뉴 항목 (disabled) Redo (Strg+Y)
> 				14055 메뉴 항목 Toolbar Options
> 			55 메뉴 모음 Menu Bar
> 				14056 메뉴 항목 FILE
> 				14057 메뉴 항목 EDIT
> 				14058 메뉴 항목 VIEW
> 				14059 메뉴 항목 PROJECT
> 				14060 메뉴 항목 BUILD
> 				14061 메뉴 항목 DEBUG
> 				14062 메뉴 항목 ANALYZE
> 				14063 메뉴 항목 TOOLS
> 				14064 메뉴 항목 EXTRAS
> 				14065 메뉴 항목 WINDOW
> 				14066 메뉴 항목 HELP
> 		67 창 Splitter ID: 379912712
> 		68 창 Splitter ID: 379917248
> 		69 Tab Output ID: 379750816
> 			70 창 ID: 1200
> 				71 창 ID: 1200
> 					72 LIST ID: 1201
> 						10797 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							10798 단추 위쪽 스크롤 화살표 ID: UpButton
> 							10799 단추 페이지 위로 ID: UpPageButton
> 							10800 위치 조정 위치 ID: ScrollbarThumb
> 							10801 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11992 목록 항목 (selectable)
> 						11993 목록 항목 (selectable)
> 						11994 목록 항목 (selectable)
> 						11995 목록 항목 (selectable)
> 						11996 목록 항목 (selectable)
> 						11997 목록 항목 (selectable)
> 						11998 목록 항목 (selectable)
> 						11999 목록 항목 (selectable)
> 					73 스크롤 막대 ID: 59904
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			14067 탭 항목 (selectable) Python Script
> 			14068 탭 항목 (selectable) Debugger
> 			14069 탭 항목 (selectable) Output
> 			80 단추 Close
> 		81 창 Splitter ID: 379914392
> 		82 Tab Class View ID: 379754008
> 			83 트리 ID: 103
> 				2920 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					2921 단추 위쪽 스크롤 화살표 ID: UpButton
> 					2922 위치 조정 위치 ID: ScrollbarThumb
> 					2923 단추 페이지 아래로 ID: DownPageButton
> 					2924 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				2925 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					2926 콘솔 트리 (selectable, disabled) External
> 					2927 콘솔 트리 (selectable, disabled) Sigmatek
> 					2928 콘솔 트리 (selectable, disabled) Elmo_1
> 					2929 콘솔 트리 (selectable, disabled) Elmo_2
> 					2930 콘솔 트리 (selectable, disabled) Elmo_3
> 					2931 콘솔 트리 (selectable, disabled) Elmo_4
> 					2932 콘솔 트리 (selectable, disabled) GL_9086_1
> 					2933 콘솔 트리 (selectable, disabled) GL_9086_1_Slot00
> 					2934 콘솔 트리 (selectable, disabled) GL_9086_1_Slot01
> 					2935 콘솔 트리 (selectable, disabled) LMCControlCommandService
> 					2936 콘솔 트리 (selectable, disabled) LMCDiagnosticsService
> 					2937 콘솔 트리 (selectable, disabled) LMCEcatInputLatch
> 					2938 콘솔 트리 (selectable, disabled) LMCRecorderStore
> 					2939 콘솔 트리 (selectable, disabled) LMCSdoExecutor
> 					2940 콘솔 트리 (selectable, disabled) TCPIPServer
> 					2941 콘솔 트리 (selectable, disabled) TCPMotionInterface
> 			14070 탭 항목 (selectable) Lib
> 			14071 탭 항목 (selectable) File
> 			14072 탭 항목 (selectable) Class
> 			14073 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 379754464
> 			90 창 ID: 269687312
> 				91 TABLE Properties Window ID: 271397096
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			14074 탭 항목 (selectable) Properties
> 			96 단추 Close
> 		14075 대화 상자 Find in Files Secondary Actions: Raise
> 			14076 텍스트 Find what:
> 			14077 콤보 상자 Find what: ID: 1010
> 				14078 편집 Find what: ID: 1001
> 				14079 단추 열기 ID: DropDown
> 			14080 확인란 Match case ID: 1014
> 			14081 확인란 Use regular expression ID: 1016
> 			14082 확인란 Match whole word ID: 1015
> 			14083 확인란 Use wildcards ID: 1165
> 			14084 확인란 Ignore Whitespaces ID: 1425
> 			14085 확인란 Ignore Return ID: 1426
> 			14086 확인란 Ignore Comments (ST&&C) ID: 1798
> 			14087 확인란 Search all project files ID: 1104
> 			14088 확인란 Only implementation of copied classes ID: 1128

>       '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>       '\t\t\t2753 창 LMCEcatInputLatch* Secondary Actions: Raise ID: 65283\n' +
>       '\t\t\t\t2754 창 ID: 59648\n' +
>       '\t\t\t\t\t2755 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000\n' +
>       '\t\t\t\t\t\t2756 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>       '\t\t\t\t\t\t\t2757 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2758 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2759 단추 페이지 아래로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2760 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2761 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>       '\t\t\t\t\t\t\t2762 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>       '\t\t\t\t\t\t\t2763 위치 조정 위치 ID: ScrollbarThumb\n' +
>       '\t\t\t\t\t\t\t2764 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>       '\t\t\t\t\t\t\t2765 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>       '\t\t\t\t\t\t2766 위치 조정 (disabled)\n' +
>       '\t\t\t16 창 LMCSdoExecutor Secondary Actions: Raise ID: 65281\n' +
>       '\t\t\t\t17 창 ID: 59648\n' +
>       '\t\t\t\t\t18 창 #pragma usingLtd SigCLib #define LMC_SDO_EXEC_IDLE 0 #define LMC_SDO_EXEC_ARMING 1 #define LMC_SDO_EXEC_RUNNING 2 #define LMC_SDO_EXEC_RESULT_READY 3 #define LMC_SDO_EXEC_ORPHANED 4 #define LMC_SDO_EXEC_QUARANTINED 5 #define LMC_SDO_EXEC_RELEASING 6 #define LMC_SDO_EXEC_VALID 0$UDINT #define LMC_SDO_EXEC_INVALID_VERSION 1 #define LMC_SDO_EXEC_INVALID_STATE 2 #define LMC_SDO_EXEC_INVALID_DIRECTION 3 #define LMC_SDO_EXEC_INVALID_INDEX 4 #define LMC_SDO_EXEC_INVALID_SUBINDEX 5 #define LMC_SDO_EXEC_INVALID_TOKEN 6 #define LMC_SDO_EXEC_INVALID_LENGTH 7 FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ParaReadWrite::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR // The producti'... 13975 more characters,
>     focused_element: '143 트리 ID: 103'
>   }
> }
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
>   "title": "Create diagnostics helper",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:117268});"
> }
> ```
>
> ```text
> element 117268 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Create diagnostics helper",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:60,y:12,screenshotId:\"screenshot-2\"});"
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
>   "title": "Name diagnostics helper",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"HandleEtherCATTopologyIoRequest\"}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "Verify diagnostics helper",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.slice(-8000));"
> }
> ```
>
> ```text
> eRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000
> 						33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							34 단추 위쪽 스크롤 화살표 ID: UpButton
> 							35 위치 조정 위치 ID: ScrollbarThumb
> 							36 단추 페이지 아래로 ID: DownPageButton
> 							37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							40 위치 조정 위치 ID: ScrollbarThumb
> 							41 단추 페이지 오른쪽으로 ID: DownPageButton
> 							42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						43 위치 조정 (disabled)
> 		44 상태 표시줄 ID: 59393
> 			45 텍스트
> 			46 텍스트
> 			47 텍스트
> 			48 텍스트
> 			49 텍스트
> 			50 텍스트 Offline
> 			51 텍스트
> 			52 텍스트 NUM
> 			53 텍스트
> 		54 창 xtpBarTop ID: 59419
> 			55 도구 모음 Edit
> 				118541 단추 Toggle bookmark
> 				118542 단추 (disabled) Previous bookmark
> 				118543 단추 (disabled) Next bookmark
> 				118544 단추 (disabled) Delete all bookmarks
> 				118545 단추 (disabled) Previous bookmark in this file
> 				118546 단추 (disabled) Next bookmark in this file
> 				118547 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				118548 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				118549 단추 (disabled) Unindent (Shift+Tab)
> 				118550 단추 (disabled) Indent (Tab)
> 			66 도구 모음 Macros Manager
> 				118551 메뉴 항목 Macros
> 			68 도구 모음 Layout Manager
> 				118552 메뉴 항목 Layouts
> 			70 도구 모음 Toolbox
> 				118553 단추 DataAnalyzer
> 				118554 메뉴 항목 Toolbar Options
> 			73 도구 모음 Net Edit
> 				118555 단추 (disabled) Select
> 				118556 메뉴 항목 Toolbar Options
> 			76 도구 모음 Debug
> 				118557 단추 Go online (Alt+F6)
> 				118558 단추 Change Online Settings
> 				118559 메뉴 항목 Online Connection
> 				118560 단추 (disabled) Set Online Connection For Current Project
> 				118561 단추 (disabled) Download (F6)
> 				118562 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				118563 단추 (disabled) Download Module on the Fly
> 				118564 단추 (disabled) Save Project on PLC
> 				118565 단추 (disabled) Start (F7)
> 				118566 단추 (disabled) Reset (F8)
> 				118567 단추 Toggle breakpoint (F4)
> 				118568 단추 Create condition breakpoint
> 				118569 메뉴 항목 Toolbar Options
> 			90 도구 모음 Build
> 				118570 메뉴 항목 Target Architecture
> 				118571 단추 Build changes (F9)
> 				118572 단추 Rebuild project (Strg+F9)
> 				118573 단추 (disabled) Cancel building (Ctrl+Break)
> 				118574 단추 Link project
> 			96 도구 모음 Standard
> 				118575 단추 New project (Strg+N)
> 				118576 단추 Open a file (Strg+Shift+O)
> 				118577 단추 Close active document (Strg+F4)
> 				118578 단추 Save file (Strg+S)
> 				118579 단추 Open project (Strg+O)
> 				118580 단추 Save project changes (Strg+Shift+S)
> 				118581 단추 Close project
> 				118582 단추 Print
> 				118583 단추 Cut (Strg+X)
> 				118584 단추 Copy (Strg+C)
> 				118585 단추 Paste (Strg+V)
> 				118586 메뉴 항목 Undo (Strg+Z)
> 				118587 메뉴 항목 (disabled) Redo (Strg+Y)
> 				118588 단추 Navigate Backward (Alt+Left)
> 				118589 단추 (disabled) Navigate Forward (Alt +Right)
> 			112 메뉴 모음 Menu Bar
> 				118590 메뉴 항목 FILE
> 				118591 메뉴 항목 EDIT
> 				118592 메뉴 항목 VIEW
> 				118593 메뉴 항목 PROJECT
> 				118594 메뉴 항목 BUILD
> 				118595 메뉴 항목 DEBUG
> 				118596 메뉴 항목 ANALYZE
> 				118597 메뉴 항목 TOOLS
> 				118598 메뉴 항목 EXTRAS
> 				118599 메뉴 항목 WINDOW
> 				118600 메뉴 항목 HELP
> 		124 창 Splitter ID: 404445040
> 		125 창 Splitter ID: 404445712
> 		126 Tab Output ID: 296578152
> 			127 창 ID: 1200
> 				128 창 ID: 1200
> 					129 LIST ID: 1204
> 						130 목록 항목 (selectable)
> 						131 목록 항목 (selectable)
> 						132 목록 항목 (selectable)
> 					133 스크롤 막대 (disabled) ID: 59904
> 						134 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						135 위치 조정 위치 ID: ScrollbarThumb
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			118601 탭 항목 (selectable) Python Script
> 			118602 탭 항목 (selectable) Debugger
> 			118603 탭 항목 (selectable) Output
> 			140 단추 Close
> 		141 창 Splitter ID: 404446216
> 		142 Tab Class View ID: 296578608
> 			143 트리 ID: 103
> 				144 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					145 단추 위쪽 스크롤 화살표 ID: UpButton
> 					17888 단추 페이지 위로 ID: UpPageButton
> 					146 위치 조정 위치 ID: ScrollbarThumb
> 					147 단추 페이지 아래로 ID: DownPageButton
> 					148 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				118523 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 					118524 단추 왼쪽 스크롤 화살표 ID: UpButton
> 					118525 위치 조정 위치 ID: ScrollbarThumb
> 					118526 단추 페이지 오른쪽으로 ID: DownPageButton
> 					118527 단추 오른쪽 스크롤 화살표 ID: DownButton
> 				118528 위치 조정 (disabled)
> 				149 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					150 콘솔 트리 (selectable) External
> 					151 콘솔 트리 (selectable) Sigmatek
> 					152 콘솔 트리 (selectable) Elmo_1
> 					153 콘솔 트리 (selectable) Elmo_2
> 					154 콘솔 트리 (selectable) Elmo_3
> 					155 콘솔 트리 (selectable) Elmo_4
> 					156 콘솔 트리 (selectable) GL_9086_1
> 					157 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					158 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					159 콘솔 트리 (selectable) LMCControlCommandService
> 					160 콘솔 트리 (selectable) LMCDiagnosticsService
> 						115922 콘솔 트리 (selectable) Servers
> 						115923 콘솔 트리 (selectable) Clients
> 						115924 콘솔 트리 (selectable) Methods
> 							116516 콘솔 트리 (selectable) Global
> 							116517 콘솔 트리 (selectable) Private
> 								118373 콘솔 트리 (selectable) LMCDiagnosticsService
> 								118374 콘솔 트리 (selectable) IsSdoReadReady
> 								118375 콘솔 트리 (selectable) GetSdoWritePolicyDetail
> 								118376 콘솔 트리 (selectable) BuildCatalogEntry
> 								118447 콘솔 트리 (selectable) HandleEtherCATTopologyIoRequest
> 						115925 콘솔 트리 (selectable) Variables
> 						115926 콘솔 트리 (selectable) Objects
> 						115927 콘솔 트리 (selectable) Dependencies
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> 						17889 콘솔 트리 (selectable) Servers
> 						17890 콘솔 트리 (selectable) Clients
> 							17891 콘솔 트리 (selectable) EcatMaster

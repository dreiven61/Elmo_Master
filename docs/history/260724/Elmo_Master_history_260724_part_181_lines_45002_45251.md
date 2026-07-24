> 							4963 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						4964 위치 조정 (disabled)
> 			4965 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				4966 창 ID: 59648
> 					4967 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						4968 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4969 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4970 위치 조정 위치 ID: ScrollbarThumb
> 							4971 단추 페이지 아래로 ID: DownPageButton
> 							4972 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						4973 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							4974 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							4975 위치 조정 위치 ID: ScrollbarThumb
> 							4976 단추 페이지 오른쪽으로 ID: DownPageButton
> 							4977 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						4978 위치 조정 (disabled)
> 			4979 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				4980 창 ID: 59648
> 					4981 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						4982 단추 위쪽 스크롤 화살표 ID: UpButton
> 						4983 위치 조정 위치 ID: ScrollbarThumb
> 						4984 단추 페이지 아래로 ID: DownPageButton
> 						4985 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					4986 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						4987 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						4988 위치 조정 위치 ID: ScrollbarThumb
> 						4989 단추 페이지 오른쪽으로 ID: DownPageButton
> 						4990 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					4991 위치 조정 (disabled)
> 			4992 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				4993 창 ID: 59648
> 					4994 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						4995 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4996 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4997 단추 페이지 위로 ID: UpPageButton
> 							4998 위치 조정 위치 ID: ScrollbarThumb
> 							4999 단추 페이지 아래로 ID: DownPageButton
> 							5000 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						5001 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							5002 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							5003 위치 조정 위치 ID: ScrollbarThumb
> 							5004 단추 페이지 오른쪽으로 ID: DownPageButton
> 							5005 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						5006 위치 조정 (disabled)
> 			5007 창 HW_Network Secondary Actions: Raise ID: 65282
> 				5008 창 ID: 59648
> 					5009 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5010 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5011 위치 조정 위치 ID: ScrollbarThumb
> 						5012 단추 페이지 아래로 ID: DownPageButton
> 						5013 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			5014 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				5015 창 ID: 59648
> 					5016 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5017 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5018 위치 조정 위치 ID: ScrollbarThumb
> 						5019 단추 페이지 아래로 ID: DownPageButton
> 						5020 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					5021 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						5022 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						5023 위치 조정 위치 ID: ScrollbarThumb
> 						5024 단추 페이지 오른쪽으로 ID: DownPageButton
> 						5025 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					5026 위치 조정 (disabled)
> 			5027 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				5028 창 ID: 59648
> 					5029 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						5030 단추 위쪽 스크롤 화살표 ID: UpButton
> 						5031 위치 조정 위치 ID: ScrollbarThumb
> 						5032 단추 페이지 아래로 ID: DownPageButton
> 						5033 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					5034 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						5035 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						5036 위치 조정 위치 ID: ScrollbarThumb
> 						5037 단추 페이지 오른쪽으로 ID: DownPageButton
> 						5038 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					5039 위치 조정 (disabled)
> 		2 상태 표시줄 ID: 59393
> 			3 텍스트
> 			4 텍스트
> 			5 텍스트
> 			6 텍스트 Ln 1 Col 1
> 			7 텍스트
> 			8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				9482 단추 Toggle bookmark
> 				9483 단추 (disabled) Previous bookmark
> 				9484 단추 (disabled) Next bookmark
> 				9485 단추 (disabled) Delete all bookmarks
> 				9486 단추 (disabled) Previous bookmark in this file
> 				9487 단추 (disabled) Next bookmark in this file
> 				9488 단추 Comment selected text (Ctrl+Shift+C)
> 				9489 단추 Remove comment (Ctrl+Shift+X)
> 				9490 단추 Unindent (Shift+Tab)
> 				9491 단추 Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				9492 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				9493 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				9494 단추 DataAnalyzer
> 				9495 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				9496 단추 (disabled) Select
> 				9497 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				9498 단추 Go online (Alt+F6)
> 				9499 단추 Change Online Settings
> 				9500 메뉴 항목 Online Connection
> 				9501 단추 (disabled) Set Online Connection For Current Project
> 				9502 단추 (disabled) Download (F6)
> 				9503 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				9504 단추 (disabled) Download Module on the Fly
> 				9505 단추 (disabled) Save Project on PLC
> 				9506 단추 (disabled) Start (F7)
> 				9507 단추 (disabled) Reset (F8)
> 				9508 단추 Toggle breakpoint (F4)
> 				9509 단추 Create condition breakpoint
> 				9510 메뉴 항목 Toolbar Options
> 			48 도구 모음 Build
> 				9511 메뉴 항목 Target Architecture
> 				9512 단추 Build changes (F9)
> 				9513 단추 Rebuild project (Strg+F9)
> 				9514 단추 (disabled) Cancel building (Ctrl+Break)
> 				9515 단추 Link project
> 			54 도구 모음 Standard
> 				9516 단추 New project (Strg+N)
> 				9517 단추 Open a file (Strg+Shift+O)
> 				9518 단추 Close active document (Strg+F4)
> 				9519 단추 (disabled) Save file (Strg+S)
> 				9520 단추 Open project (Strg+O)
> 				9521 단추 (disabled) Save project changes (Strg+Shift+S)
> 				9522 단추 Close project
> 				9523 단추 Print
> 				9524 단추 Cut (Strg+X)
> 				9525 단추 Copy (Strg+C)
> 				9526 단추 Paste (Strg+V)
> 				9527 메뉴 항목 (disabled) Undo (Strg+Z)
> 				9528 메뉴 항목 (disabled) Redo (Strg+Y)
> 				9529 단추 Navigate Backward (Alt+Left)
> 				9530 단추 (disabled) Navigate Forward (Alt +Right)
> 			70 메뉴 모음 Menu Bar
> 				9531 메뉴 항목 FILE
> 				9532 메뉴 항목 EDIT
> 				9533 메뉴 항목 VIEW
> 				9534 메뉴 항목 PROJECT
> 				9535 메뉴 항목 BUILD
> 				9536 메뉴 항목 DEBUG
> 				9537 메뉴 항목 ANALYZE
> 				9538 메뉴 항목 TOOLS
> 				9539 메뉴 항목 EXTRAS
> 				9540 메뉴 항목 WINDOW
> 				9541 메뉴 항목 HELP
> 		82 창 Splitter ID: 364851720
> 		83 창 Splitter ID: 364850208
> 		84 Tab Output ID: 121361536
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						4204 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							4205 단추 위쪽 스크롤 화살표 ID: UpButton
> 							4206 단추 페이지 위로 ID: UpPageButton
> 							4207 위치 조정 위치 ID: ScrollbarThumb
> 							4208 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						4865 목록 항목 (selectable)
> 						4935 목록 항목 (selectable)
> 						5100 목록 항목 (selectable)
> 						5101 목록 항목 (selectable)
> 						5102 목록 항목 (selectable)
> 						5103 목록 항목 (selectable)
> 						5104 목록 항목 (selectable)
> 						5105 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			9542 탭 항목 (selectable) Python Script
> 			9543 탭 항목 (selectable) Debugger
> 			9544 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 364851552
> 		97 Tab Class View ID: 121361080
> 			7279 트리 ID: 103
> 				7280 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					7281 단추 위쪽 스크롤 화살표 ID: UpButton
> 					7282 위치 조정 위치 ID: ScrollbarThumb
> 					7283 단추 페이지 아래로 ID: DownPageButton
> 					7284 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				7285 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					7286 콘솔 트리 (selectable) External
> 					7287 콘솔 트리 (selectable) Sigmatek
> 					7288 콘솔 트리 (selectable) _TCPIPServer_RT
> 					7289 콘솔 트리 (selectable) Elmo_1
> 					7290 콘솔 트리 (selectable) Elmo_2
> 					7291 콘솔 트리 (selectable) Elmo_3
> 					7292 콘솔 트리 (selectable) Elmo_4
> 					7293 콘솔 트리 (selectable) LMCControlCommandService
> 					7294 콘솔 트리 (selectable) LMCDiagnosticsService
> 					7295 콘솔 트리 (selectable) LMCEcatInputLatch
> 					7296 콘솔 트리 (selectable) LMCRecorderStore
> 					7297 콘솔 트리 (selectable) LMCSdoExecutor
> 					7298 콘솔 트리 (selectable) TCPMotionInterface
> 			9545 탭 항목 (selectable) Lib
> 			9546 탭 항목 (selectable) File
> 			9547 탭 항목 (selectable) Global
> 			9548 탭 항목 (selectable) Net
> 			9549 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 121363360
> 			106 창 ID: 288430568
> 				107 TABLE Properties Window ID: 118941016
> 					7184 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						7185 단추 위쪽 스크롤 화살표 ID: UpButton
> 						7186 위치 조정 위치 ID: ScrollbarThumb
> 						7187 단추 페이지 아래로 ID: DownPageButton
> 						7188 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					9550 custom Name
> 					9551 custom Revision
> 					9552 custom GUID
> 					9553 custom Task Settings
> 					9554 custom Sigmatek
> 					9555 custom OSInterface
> 					9556 custom IconPath
> 					9557 custom SharedCommandTable
> 					9558 custom Objectsize
> 					9559 custom Singleton
> 					9560 custom Hidden
> 					9561 custom Deprecated
> 					9562 custom GCCOptions
> 					9563 custom Comment
> 					9564 custom Filename
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			9565 탭 항목 (selectable) Properties
> 			112 단추 Close
>
> The focused UI element is 8462 창 FUNCTION GLOBAL LMCControlCommandService::HandleRequest VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleRegistryCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleAxisCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleGroupCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::HandleAdminCommands VAR_INPUT CommandId : UINT; Reference : UINT; pRequestFrame : ^USINT; RequestFrameSize : UDINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::MoveLinearAbsEx VAR_INPUT Reference : UINT; pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION FUNCTION LMCControlCommandService::GroupReadStatus VAR_INPUT pResponseFrame : ^USINT; ResponseCapacity : UDINT; END_VAR VAR_OUTPUT ResponseSize : DINT; END_VAR ResponseSize := -1; END_FUNCTION ID: 10000.
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

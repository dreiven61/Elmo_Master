> 							8 위치 조정 위치 ID: ScrollbarThumb
> 							9 단추 페이지 아래로 ID: DownPageButton
> 							10 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						11 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							12 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							13 위치 조정 위치 ID: ScrollbarThumb
> 							14 단추 페이지 오른쪽으로 ID: DownPageButton
> 							15 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						16 위치 조정 (disabled)
> 			17 창 Elmo_4 Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						20 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							21 단추 위쪽 스크롤 화살표 ID: UpButton
> 							22 위치 조정 위치 ID: ScrollbarThumb
> 							23 단추 페이지 아래로 ID: DownPageButton
> 							24 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						25 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							26 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							27 위치 조정 위치 ID: ScrollbarThumb
> 							28 단추 페이지 오른쪽으로 ID: DownPageButton
> 							29 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						30 위치 조정 (disabled)
> 			31 창 EtherCAT_Network Secondary Actions: Raise ID: 65284
> 				32 창 ID: 59648
> 					33 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						34 단추 위쪽 스크롤 화살표 ID: UpButton
> 						35 위치 조정 위치 ID: ScrollbarThumb
> 						36 단추 페이지 아래로 ID: DownPageButton
> 						37 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					38 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						39 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						40 위치 조정 위치 ID: ScrollbarThumb
> 						41 단추 페이지 오른쪽으로 ID: DownPageButton
> 						42 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					43 위치 조정 (disabled)
> 			44 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65283
> 				45 창 ID: 59648
> 					46 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 #define LMC_DIAG_SDO_LOCAL_ERROR_ID -32001 #define LMC_DIAG_SDO_ABORT_TIMEOUT 0x05040000 #define LMC_DIAG_SDO_STATE_QUEUED 1 #define LMC_DIAG_SDO_STATE_RUNNING 2 #define LMC_DIAG_SDO_STATE_COMPLETED 3 #define LMC_DIAG_SDO_STATE_FAILED 4 #define LMC_DIAG_SDO_STATE_CANCELLED 5 #define LMC_DIAG_SDO_STATE_EXPIRED 6 #define LMC_DIAG_SDO_OUTCOME_SUCCESS 1 #define LMC_DIAG_SDO_OUTCOME_FAILED 2 #define LMC_DIAG_SDO_OUTCOME_CANCELLED 3 #define LMC_DIAG_SDO_OUTCOME_TIMED_OUT 4 #define LMC_DIAG_SDO_DRAIN_EXPIRED 1 #define LMC_DIAG_SDO_DRAIN_ORPHAN 2 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := ID: 10000
> 						47 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							48 단추 위쪽 스크롤 화살표 ID: UpButton
> 							49 단추 페이지 위로 ID: UpPageButton
> 							50 위치 조정 위치 ID: ScrollbarThumb
> 							51 단추 페이지 아래로 ID: DownPageButton
> 							52 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						53 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							54 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							55 위치 조정 위치 ID: ScrollbarThumb
> 							56 단추 페이지 오른쪽으로 ID: DownPageButton
> 							57 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						58 위치 조정 (disabled)
> 			59 창 HW_Network Secondary Actions: Raise ID: 65282
> 				60 창 ID: 59648
> 					61 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						62 단추 위쪽 스크롤 화살표 ID: UpButton
> 						63 위치 조정 위치 ID: ScrollbarThumb
> 						64 단추 페이지 아래로 ID: DownPageButton
> 						65 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			66 창 Motion_Network Secondary Actions: Raise ID: 65281
> 				67 창 ID: 59648
> 					68 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						69 단추 위쪽 스크롤 화살표 ID: UpButton
> 						70 위치 조정 위치 ID: ScrollbarThumb
> 						71 단추 페이지 아래로 ID: DownPageButton
> 						72 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					73 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						74 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						75 위치 조정 위치 ID: ScrollbarThumb
> 						76 단추 페이지 오른쪽으로 ID: DownPageButton
> 						77 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					78 위치 조정 (disabled)
> 			79 창 Comm_Network Secondary Actions: Raise ID: 65280
> 				80 창 ID: 59648
> 					81 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						82 단추 위쪽 스크롤 화살표 ID: UpButton
> 						83 위치 조정 위치 ID: ScrollbarThumb
> 						84 단추 페이지 아래로 ID: DownPageButton
> 						85 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		86 상태 표시줄 ID: 59393
> 			87 텍스트
> 			88 텍스트
> 			89 텍스트
> 			90 텍스트
> 			91 텍스트
> 			92 텍스트 Offline
> 			93 텍스트
> 			94 텍스트 NUM
> 			95 텍스트
> 		96 창 xtpBarTop ID: 59419
> 			97 도구 모음 Edit
> 				98 단추 Toggle bookmark
> 				99 단추 (disabled) Previous bookmark
> 				100 단추 (disabled) Next bookmark
> 				101 단추 (disabled) Delete all bookmarks
> 				102 단추 (disabled) Previous bookmark in this file
> 				103 단추 (disabled) Next bookmark in this file
> 				104 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				105 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				106 단추 (disabled) Unindent (Shift+Tab)
> 				107 단추 (disabled) Indent (Tab)
> 			108 도구 모음 Macros Manager
> 				109 메뉴 항목 Macros
> 			110 도구 모음 Layout Manager
> 				111 메뉴 항목 Layouts
> 			112 도구 모음 Toolbox
> 				113 단추 DataAnalyzer
> 				114 메뉴 항목 Toolbar Options
> 			115 도구 모음 Net Edit
> 				116 단추 (disabled) Select
> 				117 메뉴 항목 Toolbar Options
> 			118 도구 모음 Debug
> 				119 단추 Go online (Alt+F6)
> 				120 단추 Change Online Settings
> 				121 메뉴 항목 Online Connection
> 				122 단추 (disabled) Set Online Connection For Current Project
> 				123 단추 (disabled) Download (F6)
> 				124 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				125 단추 (disabled) Download Module on the Fly
> 				126 단추 (disabled) Save Project on PLC
> 				127 단추 (disabled) Start (F7)
> 				128 단추 (disabled) Reset (F8)
> 				129 단추 Toggle breakpoint (F4)
> 				130 단추 Create condition breakpoint
> 				131 단추 Remove all breakpoint(s) (Shift+F4)
> 				132 단추 AWL trigger on/off
> 				133 단추 (disabled) Fix AWL trigger to current instruction
> 				134 단추 Activate/Deactivate Live View
> 				135 단추 Activate/Deactivate Auto Watch
> 				136 단추 (disabled) Goto instruction pointer
> 				137 단추 (disabled) Step into (F5)
> 				138 메뉴 항목 Toolbar Options
> 			139 도구 모음 Build
> 				140 메뉴 항목 Target Architecture
> 				141 단추 Build changes (F9)
> 				142 단추 Rebuild project (Strg+F9)
> 				143 단추 (disabled) Cancel building (Ctrl+Break)
> 				144 단추 Link project
> 			145 도구 모음 Standard
> 				146 단추 New project (Strg+N)
> 				147 단추 Open a file (Strg+Shift+O)
> 				148 단추 Close active document (Strg+F4)
> 				149 단추 (disabled) Save file (Strg+S)
> 				150 단추 Open project (Strg+O)
> 				151 단추 Save project changes (Strg+Shift+S)
> 				152 단추 Close project
> 				153 단추 Print
> 				154 단추 Cut (Strg+X)
> 				155 단추 Copy (Strg+C)
> 				156 단추 Paste (Strg+V)
> 				157 메뉴 항목 Undo (Strg+Z)
> 				158 메뉴 항목 (disabled) Redo (Strg+Y)
> 				159 단추 Navigate Backward (Alt+Left)
> 				160 단추 (disabled) Navigate Forward (Alt +Right)
> 			161 메뉴 모음 Menu Bar
> 				162 메뉴 항목 FILE
> 				163 메뉴 항목 EDIT
> 				164 메뉴 항목 VIEW
> 				165 메뉴 항목 PROJECT
> 				166 메뉴 항목 BUILD
> 				167 메뉴 항목 DEBUG
> 				168 메뉴 항목 ANALYZE
> 				169 메뉴 항목 TOOLS
> 				170 메뉴 항목 EXTRAS
> 				171 메뉴 항목 WINDOW
> 				172 메뉴 항목 HELP
> 		173 창 Splitter ID: 360603976
> 		174 창 Splitter ID: 359974384
> 		175 Tab Output ID: 358094976
> 			176 창 ID: 1200
> 				177 창 ID: 1200
> 					178 LIST ID: 1201
> 						179 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							180 단추 위쪽 스크롤 화살표 ID: UpButton
> 							181 단추 페이지 위로 ID: UpPageButton
> 							182 위치 조정 위치 ID: ScrollbarThumb
> 							183 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						184 목록 항목 (selectable)
> 						185 목록 항목 (selectable)
> 						186 목록 항목 (selectable)
> 						187 목록 항목 (selectable)
> 						188 목록 항목 (selectable)
> 						189 목록 항목 (selectable)
> 						190 목록 항목 (selectable)
> 						191 목록 항목 (selectable)
> 					192 스크롤 막대 ID: 59904
> 						193 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						194 위치 조정 위치 ID: ScrollbarThumb
> 						195 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			196 탭 항목 (selectable) Python Script
> 			197 탭 항목 (selectable) Debugger
> 			198 탭 항목 (selectable) Output
> 			199 단추 Close
> 		200 창 Splitter ID: 360599944
> 		201 Tab Class View ID: 358100904
> 			202 트리 ID: 103
> 				203 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					204 단추 위쪽 스크롤 화살표 ID: UpButton
> 					205 단추 페이지 위로 ID: UpPageButton
> 					206 위치 조정 위치 ID: ScrollbarThumb
> 					207 단추 페이지 아래로 ID: DownPageButton
> 					208 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				209 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					210 콘솔 트리 (selectable) External
> 					211 콘솔 트리 (selectable) Sigmatek
> 					212 콘솔 트리 (selectable) _TCPIPServer_RT
> 					213 콘솔 트리 (selectable) Elmo_1
> 					214 콘솔 트리 (selectable) Elmo_2
> 					215 콘솔 트리 (selectable) Elmo_3
> 					216 콘솔 트리 (selectable) Elmo_4
> 					217 콘솔 트리 (selectable) LMCControlCommandService
> 						218 콘솔 트리 (selectable) Servers
> 						219 콘솔 트리 (selectable) Clients
> 							220 콘솔 트리 (selectable) LMCAxis1
> 							221 콘솔 트리 (selectable) LMCAxis2
> 							222 콘솔 트리 (selectable) LMCAxis3
> 							223 콘솔 트리 (selectable) LMCAxis4
> 							224 콘솔 트리 (selectable) LMCAxis5
> 							225 콘솔 트리 (selectable) LMCAxis9
> 							226 콘솔 트리 (selectable) LMCAxis52
> 							227 콘솔 트리 (selectable) LMCAxis51
> 							228 콘솔 트리 (selectable) LMCAxis50
> 						229 콘솔 트리 (selectable) Methods
> 						230 콘솔 트리 (selectable) Dependencies
> 					231 콘솔 트리 (selectable) LMCDiagnosticsService
> 					232 콘솔 트리 (selectable) LMCEcatInputLatch
> 					233 콘솔 트리 (selectable) LMCRecorderStore
> 					234 콘솔 트리 (selectable) LMCSdoExecutor
> 					235 콘솔 트리 (selectable) TCPMotionInterface
> 			236 탭 항목 (selectable) Lib
> 			237 탭 항목 (selectable) File
> 			238 탭 항목 (selectable) Global
> 			239 탭 항목 (selectable) Net
> 			240 탭 항목 (selectable) Class
> 			241 단추 Close
> 		242 Tab Properties ID: 358094064
> 			243 창 ID: 290198624
> 				244 TABLE Properties Window ID: 293571936
> 					245 custom Name
> 					246 custom Class
> 					247 custom DataType
> 					248 custom Type
> 					249 custom Required
> 					250 custom DefaultInitValue
> 					251 custom Comment
> 				252 도구 모음 ID: 59392
> 					253 단추
> 					254 단추
> 			255 탭 항목 (selectable) Properties
> 			256 단추 Close
>

> 							54 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							55 위치 조정 위치 ID: ScrollbarThumb
> 							56 단추 페이지 오른쪽으로 ID: DownPageButton
> 							57 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						58 위치 조정 (disabled)
> 			59 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65288
> 				60 창 ID: 59648
> 					61 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 						62 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							63 단추 위쪽 스크롤 화살표 ID: UpButton
> 							64 단추 페이지 위로 ID: UpPageButton
> 							65 위치 조정 위치 ID: ScrollbarThumb
> 							66 단추 페이지 아래로 ID: DownPageButton
> 							67 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						68 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							69 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							70 위치 조정 위치 ID: ScrollbarThumb
> 							71 단추 페이지 오른쪽으로 ID: DownPageButton
> 							72 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						73 위치 조정 (disabled)
> 			74 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				75 창 ID: 59648
> 					76 창 xtpBarTop ID: 59419
> 						77 도구 모음 Hardware Editor
> 							78 단추 Hardware Editor Configuration Settings
> 							79 단추 Calculate Resources of Project
> 							80 단추 (disabled) Upload Hardware Tree from PLC
> 							81 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							82 단추 Show Detail Mode\Show Configuration Mode
> 							83 단추 Generates the ENI File of the current project
> 					84 창 ID: 59648
> 						85 트리 ID: 1
> 							86 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								87 단추 위쪽 스크롤 화살표 ID: UpButton
> 								88 단추 페이지 위로 ID: UpPageButton
> 								89 위치 조정 위치 ID: ScrollbarThumb
> 								90 단추 페이지 아래로 ID: DownPageButton
> 								91 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							92 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								93 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								94 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								95 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								96 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								97 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								98 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								99 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								100 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								101 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								102 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								103 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								104 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								105 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								106 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								107 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								108 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								109 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								110 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								111 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								112 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								113 콘솔 트리 (selectable) ALARM:00, Empty
> 								114 콘솔 트리 (selectable) SDIAS:00, Empty
> 								115 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								116 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							117 콘솔 트리 (selectable) Unplaced Module(s)
> 			118 창 Elmo_4 Secondary Actions: Raise ID: 65285
> 				119 창 ID: 59648
> 					120 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						121 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							122 단추 위쪽 스크롤 화살표 ID: UpButton
> 							123 위치 조정 위치 ID: ScrollbarThumb
> 							124 단추 페이지 아래로 ID: DownPageButton
> 							125 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						126 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							127 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							128 위치 조정 위치 ID: ScrollbarThumb
> 							129 단추 페이지 오른쪽으로 ID: DownPageButton
> 							130 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						131 위치 조정 (disabled)
> 			132 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				133 창 ID: 59648
> 					134 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						135 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							136 단추 위쪽 스크롤 화살표 ID: UpButton
> 							137 위치 조정 위치 ID: ScrollbarThumb
> 							138 단추 페이지 아래로 ID: DownPageButton
> 							139 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						140 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							141 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							142 위치 조정 위치 ID: ScrollbarThumb
> 							143 단추 페이지 오른쪽으로 ID: DownPageButton
> 							144 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						145 위치 조정 (disabled)
> 			146 창 HW_Network Secondary Actions: Raise ID: 65281
> 				147 창 ID: 59648
> 					148 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						149 단추 위쪽 스크롤 화살표 ID: UpButton
> 						150 위치 조정 위치 ID: ScrollbarThumb
> 						151 단추 페이지 아래로 ID: DownPageButton
> 						152 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					153 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						154 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						155 위치 조정 위치 ID: ScrollbarThumb
> 						156 단추 페이지 오른쪽으로 ID: DownPageButton
> 						157 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					158 위치 조정 (disabled)
> 		159 상태 표시줄 ID: 59393
> 			160 텍스트
> 			161 텍스트
> 			162 텍스트
> 			163 텍스트
> 			164 텍스트
> 			165 텍스트 Offline
> 			166 텍스트
> 			167 텍스트 NUM
> 			168 텍스트
> 		169 창 xtpBarTop ID: 59419
> 			170 도구 모음 Script
> 			171 도구 모음 Edit
> 				172 단추 (disabled) Toggle bookmark
> 				173 단추 (disabled) Previous bookmark
> 				174 단추 (disabled) Next bookmark
> 				175 단추 (disabled) Delete all bookmarks
> 				176 단추 (disabled) Previous bookmark in this file
> 				177 단추 (disabled) Next bookmark in this file
> 				178 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				179 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				180 단추 (disabled) Unindent (Shift+Tab)
> 				181 단추 (disabled) Indent (Tab)
> 			182 도구 모음 Macros Manager
> 				183 메뉴 항목 Macros
> 			184 도구 모음 Layout Manager
> 				185 메뉴 항목 Layouts
> 			186 도구 모음 Toolbox
> 				187 단추 DataAnalyzer
> 				188 메뉴 항목 Toolbar Options
> 			189 도구 모음 Net Edit
> 				190 단추 Select
> 				191 메뉴 항목 Toolbar Options
> 			192 도구 모음 Debug
> 				193 단추 Go online (Alt+F6)
> 				194 단추 Change Online Settings
> 				195 메뉴 항목 Online Connection
> 				196 단추 (disabled) Set Online Connection For Current Project
> 				197 단추 (disabled) Download (F6)
> 				198 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				199 단추 (disabled) Download Module on the Fly
> 				200 단추 (disabled) Save Project on PLC
> 				201 단추 (disabled) Start (F7)
> 				202 단추 (disabled) Reset (F8)
> 				203 단추 (disabled) Toggle breakpoint (F4)
> 				204 단추 (disabled) Create condition breakpoint
> 				205 메뉴 항목 Toolbar Options
> 			206 도구 모음 Build
> 				207 메뉴 항목 Target Architecture
> 				208 단추 Build changes (F9)
> 				209 단추 Rebuild project (Strg+F9)
> 				210 단추 (disabled) Cancel building (Ctrl+Break)
> 				211 단추 Link project
> 			212 도구 모음 Standard
> 				213 단추 New project (Strg+N)
> 				214 단추 Open a file (Strg+Shift+O)
> 				215 단추 Close active document (Strg+F4)
> 				216 단추 (disabled) Save file (Strg+S)
> 				217 단추 Open project (Strg+O)
> 				218 단추 (disabled) Save project changes (Strg+Shift+S)
> 				219 단추 Close project
> 				220 단추 Print
> 				221 단추 Cut (Strg+X)
> 				222 단추 Copy (Strg+C)
> 				223 단추 Paste (Strg+V)
> 				224 메뉴 항목 Undo (Strg+Z)
> 				225 메뉴 항목 (disabled) Redo (Strg+Y)
> 				226 단추 Navigate Backward (Alt+Left)
> 				227 단추 (disabled) Navigate Forward (Alt +Right)
> 			228 메뉴 모음 Menu Bar
> 				229 메뉴 항목 FILE
> 				230 메뉴 항목 EDIT
> 				231 메뉴 항목 NETEDIT
> 				232 메뉴 항목 VIEW
> 				233 메뉴 항목 PROJECT
> 				234 메뉴 항목 BUILD
> 				235 메뉴 항목 DEBUG
> 				236 메뉴 항목 ANALYZE
> 				237 메뉴 항목 TOOLS
> 				238 메뉴 항목 EXTRAS
> 				239 메뉴 항목 WINDOW
> 				240 메뉴 항목 HELP
> 		241 창 Splitter ID: 481389416
> 		242 창 Splitter ID: 481390928
> 		243 Tab Output ID: 409867992
> 			244 창 ID: 1200
> 				245 창 ID: 1200
> 					246 LIST ID: 1204
> 						247 목록 항목 (selectable)
> 						248 목록 항목 (selectable)
> 					249 스크롤 막대 (disabled) ID: 59904
> 						250 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						251 위치 조정 위치 ID: ScrollbarThumb
> 						252 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			253 탭 항목 (selectable) Python Script
> 			254 탭 항목 (selectable) Debugger
> 			255 탭 항목 (selectable) Output
> 			256 단추 Close
> 		257 창 Splitter ID: 608798568
> 		258 Tab Global View ID: 409868448
> 			259 트리 ID: 105
> 				260 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					261 단추 위쪽 스크롤 화살표 ID: UpButton
> 					262 위치 조정 위치 ID: ScrollbarThumb
> 					263 단추 페이지 아래로 ID: DownPageButton
> 					264 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				265 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					266 콘솔 트리 (selectable) Types
> 						267 콘솔 트리 (selectable) External
> 						268 콘솔 트리 (selectable) _LMC
> 							269 콘솔 트리 (selectable) _LMCAxis
> 							270 콘솔 트리 (selectable) _LMCProfile
> 							271 콘솔 트리 (selectable) old
> 							272 콘솔 트리 (selectable) _LMC_DYN_SWLIMITS
> 							273 콘솔 트리 (selectable) _LMCCONTROLLERTYPE
> 							274 콘솔 트리 (selectable) _POSFLAGS
> 							275 콘솔 트리 (selectable) CNCInternalStruct
> 							276 콘솔 트리 (selectable) CurveTable
> 							277 콘솔 트리 (selectable) MasterStruct
> 						278 콘솔 트리 (selectable) DS402_Control
> 						279 콘솔 트리 (selectable) DS402_State
> 						280 콘솔 트리 (selectable) FeSetup
> 						281 콘솔 트리 (selectable) IO_State
> 						282 콘솔 트리 (selectable) pHwBase
> 						283 콘솔 트리 (selectable) pHwBaseCDIAS
> 						284 콘솔 트리 (selectable) SafetyConfigStateType
> 						285 콘솔 트리 (selectable) SafetyDiagState
> 						286 콘솔 트리 (selectable) t_e_SafetyMemState
> 						287 콘솔 트리 (selectable) t_e_VaranErrors
> 						288 콘솔 트리 (selectable) t_s_ModulInfo
> 						289 콘솔 트리 (selectable) IO_FLAG
> 						290 콘솔 트리 (selectable) SafetyDiagInfo
> 					291 콘솔 트리 (selectable) Variables
> 			292 탭 항목 (selectable) Lib
> 			293 탭 항목 (selectable) File
> 			294 탭 항목 (selectable) Global
> 			295 단추 Close
> 		296 Tab Properties ID: 409871640
> 			297 창 ID: 121918456
> 				298 TABLE Properties Window ID: 127184512
> 					299 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						300 단추 위쪽 스크롤 화살표 ID: UpButton
> 						301 위치 조정 위치 ID: ScrollbarThumb
> 						302 단추 페이지 아래로 ID: DownPageButton
> 						303 단추 아래쪽 스크롤 화살표 ID: DownButton

> 							40 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							41 위치 조정 위치 ID: ScrollbarThumb
> 							42 단추 페이지 오른쪽으로 ID: DownPageButton
> 							43 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						44 위치 조정 (disabled)
> 			45 창 Comm_Network Secondary Actions: Raise ID: 65286
> 				46 창 ID: 59648
> 					47 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						48 단추 위쪽 스크롤 화살표 ID: UpButton
> 						49 위치 조정 위치 ID: ScrollbarThumb
> 						50 단추 페이지 아래로 ID: DownPageButton
> 						51 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					52 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						53 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						54 위치 조정 위치 ID: ScrollbarThumb
> 						55 단추 페이지 오른쪽으로 ID: DownPageButton
> 						56 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					57 위치 조정 (disabled)
> 			58 창 Hardware Editor Secondary Actions: Raise ID: 65284
> 				59 창 ID: 59648
> 					60 창 xtpBarTop ID: 59419
> 						61 도구 모음 Hardware Editor
> 							62 단추 Hardware Editor Configuration Settings
> 							63 단추 Calculate Resources of Project
> 							64 단추 (disabled) Upload Hardware Tree from PLC
> 							65 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							66 단추 Show Detail Mode\Show Configuration Mode
> 							67 단추 Generates the ENI File of the current project
> 					68 창 ID: 59648
> 						69 트리 ID: 1
> 							70 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								71 단추 위쪽 스크롤 화살표 ID: UpButton
> 								72 단추 페이지 위로 ID: UpPageButton
> 								73 위치 조정 위치 ID: ScrollbarThumb
> 								74 단추 페이지 아래로 ID: DownPageButton
> 								75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							76 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								77 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								78 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								79 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								80 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								81 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								82 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								83 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								84 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								85 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								86 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								87 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 								88 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								89 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								90 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->
> 								91 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								92 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								93 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								94 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								95 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								96 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								97 콘솔 트리 (selectable) ALARM:00, Empty
> 								98 콘솔 트리 (selectable) SDIAS:00, Empty
> 								99 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								100 콘솔 트리 (selectable) MULTIVARAN:00, Empty
> 							101 콘솔 트리 (selectable) Unplaced Module(s)
> 			102 창 Elmo_4 Secondary Actions: Raise ID: 65283
> 				103 창 ID: 59648
> 					104 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						105 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							106 단추 위쪽 스크롤 화살표 ID: UpButton
> 							107 위치 조정 위치 ID: ScrollbarThumb
> 							108 단추 페이지 아래로 ID: DownPageButton
> 							109 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						110 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							111 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							112 위치 조정 위치 ID: ScrollbarThumb
> 							113 단추 페이지 오른쪽으로 ID: DownPageButton
> 							114 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						115 위치 조정 (disabled)
> 			116 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				117 창 ID: 59648
> 					118 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						119 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							120 단추 위쪽 스크롤 화살표 ID: UpButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 아래로 ID: DownPageButton
> 							123 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						124 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							125 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							126 위치 조정 위치 ID: ScrollbarThumb
> 							127 단추 페이지 오른쪽으로 ID: DownPageButton
> 							128 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						129 위치 조정 (disabled)
> 			130 창 HW_Network Secondary Actions: Raise ID: 65281
> 				131 창 ID: 59648
> 					132 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						133 단추 위쪽 스크롤 화살표 ID: UpButton
> 						134 위치 조정 위치 ID: ScrollbarThumb
> 						135 단추 페이지 아래로 ID: DownPageButton
> 						136 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					137 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						138 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						139 위치 조정 위치 ID: ScrollbarThumb
> 						140 단추 페이지 오른쪽으로 ID: DownPageButton
> 						141 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					142 위치 조정 (disabled)
> 		143 상태 표시줄 ID: 59393
> 			144 텍스트
> 			145 텍스트 LMCEcatInputLatch::RtWork
> 			146 텍스트
> 			147 텍스트 Ln 286 Col 39
> 			148 텍스트
> 			149 텍스트 Offline
> 			150 텍스트
> 			151 텍스트 NUM
> 			152 텍스트
> 		153 창 xtpBarTop ID: 59419
> 			154 도구 모음 Script
> 			155 도구 모음 Edit
> 				156 단추 Toggle bookmark
> 				157 단추 (disabled) Previous bookmark
> 				158 단추 (disabled) Next bookmark
> 				159 단추 (disabled) Delete all bookmarks
> 				160 단추 (disabled) Previous bookmark in this file
> 				161 단추 (disabled) Next bookmark in this file
> 				162 단추 Comment selected text (Ctrl+Shift+C)
> 				163 단추 Remove comment (Ctrl+Shift+X)
> 				164 단추 Unindent (Shift+Tab)
> 				165 단추 Indent (Tab)
> 			166 도구 모음 Macros Manager
> 				167 메뉴 항목 Macros
> 			168 도구 모음 Layout Manager
> 				169 메뉴 항목 Layouts
> 			170 도구 모음 Toolbox
> 				171 단추 DataAnalyzer
> 				172 메뉴 항목 Toolbar Options
> 			173 도구 모음 Net Edit
> 				174 단추 (disabled) Select
> 				175 메뉴 항목 Toolbar Options
> 			176 도구 모음 Debug
> 				177 단추 Go online (Alt+F6)
> 				178 단추 Change Online Settings
> 				179 메뉴 항목 Online Connection
> 				180 단추 (disabled) Set Online Connection For Current Project
> 				181 단추 (disabled) Download (F6)
> 				182 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				183 단추 (disabled) Download Module on the Fly
> 				184 단추 (disabled) Save Project on PLC
> 				185 단추 (disabled) Start (F7)
> 				186 단추 (disabled) Reset (F8)
> 				187 단추 Toggle breakpoint (F4)
> 				188 단추 Create condition breakpoint
> 				189 메뉴 항목 Toolbar Options
> 			190 도구 모음 Build
> 				191 메뉴 항목 Target Architecture
> 				192 단추 Build changes (F9)
> 				193 단추 Rebuild project (Strg+F9)
> 				194 단추 (disabled) Cancel building (Ctrl+Break)
> 				195 단추 Link project
> 			196 도구 모음 Standard
> 				197 단추 New project (Strg+N)
> 				198 단추 Open a file (Strg+Shift+O)
> 				199 단추 Close active document (Strg+F4)
> 				200 단추 (disabled) Save file (Strg+S)
> 				201 단추 Open project (Strg+O)
> 				202 단추 (disabled) Save project changes (Strg+Shift+S)
> 				203 단추 Close project
> 				204 단추 Print
> 				205 단추 Cut (Strg+X)
> 				206 단추 Copy (Strg+C)
> 				207 단추 (disabled) Paste (Strg+V)
> 				208 메뉴 항목 (disabled) Undo (Strg+Z)
> 				209 메뉴 항목 (disabled) Redo (Strg+Y)
> 				210 단추 Navigate Backward (Alt+Left)
> 				211 단추 (disabled) Navigate Forward (Alt +Right)
> 			212 메뉴 모음 Menu Bar
> 				213 메뉴 항목 FILE
> 				214 메뉴 항목 EDIT
> 				215 메뉴 항목 VIEW
> 				216 메뉴 항목 PROJECT
> 				217 메뉴 항목 BUILD
> 				218 메뉴 항목 DEBUG
> 				219 메뉴 항목 ANALYZE
> 				220 메뉴 항목 TOOLS
> 				221 메뉴 항목 EXTRAS
> 				222 메뉴 항목 WINDOW
> 				223 메뉴 항목 HELP
> 		224 창 Splitter ID: 411855768
> 		225 창 Splitter ID: 411851736
> 		226 Tab Output ID: 409867992
> 			227 창 ID: 1200
> 				228 창 ID: 1200
> 					229 LIST ID: 1201
> 						230 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							231 단추 위쪽 스크롤 화살표 ID: UpButton
> 							232 단추 페이지 위로 ID: UpPageButton
> 							233 위치 조정 위치 ID: ScrollbarThumb
> 							234 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						235 목록 항목 (selectable)
> 						236 목록 항목 (selectable)
> 						237 목록 항목 (selectable)
> 						238 목록 항목 (selectable)
> 						239 목록 항목 (selectable)
> 						240 목록 항목 (selectable)
> 						241 목록 항목 (selectable)
> 						242 목록 항목 (selectable)
> 						243 목록 항목 (selectable)
> 						244 목록 항목 (selectable)
> 						245 목록 항목 (selectable)
> 						246 목록 항목 (selectable)
> 						247 목록 항목 (selectable)
> 						248 목록 항목 (selectable)
> 						249 목록 항목 (selectable)
> 						250 목록 항목 (selectable)
> 						251 목록 항목 (selectable)
> 						252 목록 항목 (selectable)
> 						253 목록 항목 (selectable)
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 						260 목록 항목 (selectable)
> 						261 목록 항목 (selectable)
> 						262 목록 항목 (selectable)
> 						263 목록 항목 (selectable)
> 						264 목록 항목 (selectable)
> 					265 스크롤 막대 ID: 59904
> 						266 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						267 위치 조정 위치 ID: ScrollbarThumb
> 						268 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			269 탭 항목 (selectable) Python Script
> 			270 탭 항목 (selectable) Output
> 			271 탭 항목 (selectable) Debugger
> 			272 단추 Close
> 		273 창 Splitter ID: 411854424
> 		274 Tab Class View ID: 409868448
> 			275 트리 ID: 103
> 				276 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					277 단추 위쪽 스크롤 화살표 ID: UpButton
> 					278 위치 조정 위치 ID: ScrollbarThumb
> 					279 단추 페이지 아래로 ID: DownPageButton
> 					280 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				281 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					282 콘솔 트리 (selectable) External
> 					283 콘솔 트리 (selectable) Sigmatek
> 					284 콘솔 트리 (selectable) _TCPIPServer_RT
> 					285 콘솔 트리 (selectable) Elmo_1
> 					286 콘솔 트리 (selectable) Elmo_2
> 					287 콘솔 트리 (selectable) Elmo_3
> 					288 콘솔 트리 (selectable) Elmo_4
> 					289 콘솔 트리 (selectable) LMCDiagnosticsService

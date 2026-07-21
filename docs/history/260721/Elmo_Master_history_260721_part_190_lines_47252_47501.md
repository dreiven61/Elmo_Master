> 						70 도구 모음 Hardware Editor
> 							71 단추 Hardware Editor Configuration Settings
> 							72 단추 Calculate Resources of Project
> 							73 단추 (disabled) Upload Hardware Tree from PLC
> 							74 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							75 단추 Show Detail Mode\Show Configuration Mode
> 							76 단추 Generates the ENI File of the current project
> 					77 창 ID: 59648
> 						78 트리 ID: 1
> 							79 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								80 단추 위쪽 스크롤 화살표 ID: UpButton
> 								81 단추 페이지 위로 ID: UpPageButton
> 								82 위치 조정 위치 ID: ScrollbarThumb
> 								83 단추 페이지 아래로 ID: DownPageButton
> 								84 단추 아래쪽 스크롤 화살표 ID: DownButton
> 							85 콘솔 트리 (selectable, disabled) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)
> 								86 콘솔 트리 (selectable, disabled) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								87 콘솔 트리 (selectable, disabled) EtherCAT State (EtherCATState) <-[]->
> 								88 콘솔 트리 (selectable, disabled) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								89 콘솔 트리 (selectable, disabled) EtherCAT Synchron (Synchron) <-[]->
> 								90 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								91 콘솔 트리 (selectable, disabled) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								92 콘솔 트리 (selectable, disabled) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								93 콘솔 트리 (selectable, disabled) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								94 콘솔 트리 (selectable, disabled) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								95 콘솔 트리 (selectable, disabled) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->
> 								96 콘솔 트리 (selectable, disabled) Sdias Class State (ClassState) <-[]->
> 								97 콘솔 트리 (selectable, disabled) Sdias Firmware Version (FirmwareVersion) <-[]->
> 								98 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								99 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]->
> 								100 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								101 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								102 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								103 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								104 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								105 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								106 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								107 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								108 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								109 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							110 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			111 창 Elmo_4 Secondary Actions: Raise ID: 65284
> 				112 창 ID: 59648
> 					113 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						114 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							115 단추 위쪽 스크롤 화살표 ID: UpButton
> 							116 위치 조정 위치 ID: ScrollbarThumb
> 							117 단추 페이지 아래로 ID: DownPageButton
> 							118 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						119 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							120 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							121 위치 조정 위치 ID: ScrollbarThumb
> 							122 단추 페이지 오른쪽으로 ID: DownPageButton
> 							123 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						124 위치 조정 (disabled)
> 			125 창 Motion_Network Secondary Actions: Raise ID: 65283
> 				126 창 ID: 59648
> 					127 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						128 단추 위쪽 스크롤 화살표 ID: UpButton
> 						129 위치 조정 위치 ID: ScrollbarThumb
> 						130 단추 페이지 아래로 ID: DownPageButton
> 						131 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					132 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						133 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						134 위치 조정 위치 ID: ScrollbarThumb
> 						135 단추 페이지 오른쪽으로 ID: DownPageButton
> 						136 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					137 위치 조정 (disabled)
> 			138 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				139 창 ID: 59648
> 					140 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						141 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							142 단추 위쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 아래로 ID: DownPageButton
> 							145 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						146 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							147 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							148 위치 조정 위치 ID: ScrollbarThumb
> 							149 단추 페이지 오른쪽으로 ID: DownPageButton
> 							150 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						151 위치 조정 (disabled)
> 			152 창 HW_Network Secondary Actions: Raise ID: 65281
> 				153 창 ID: 59648
> 					154 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						155 단추 위쪽 스크롤 화살표 ID: UpButton
> 						156 위치 조정 위치 ID: ScrollbarThumb
> 						157 단추 페이지 아래로 ID: DownPageButton
> 						158 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					159 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						160 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						161 위치 조정 위치 ID: ScrollbarThumb
> 						162 단추 페이지 오른쪽으로 ID: DownPageButton
> 						163 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					164 위치 조정 (disabled)
> 		165 상태 표시줄 ID: 59393
> 			166 텍스트
> 			167 텍스트
> 			168 텍스트
> 			169 텍스트
> 			170 텍스트
> 			171 텍스트 Offline
> 			172 텍스트
> 			173 텍스트 NUM
> 			174 텍스트
> 		175 창 xtpBarTop ID: 59419
> 			176 도구 모음 Script
> 			177 도구 모음 Edit
> 				178 단추 Toggle bookmark
> 				179 단추 (disabled) Previous bookmark
> 				180 단추 (disabled) Next bookmark
> 				181 단추 (disabled) Delete all bookmarks
> 				182 단추 (disabled) Previous bookmark in this file
> 				183 단추 (disabled) Next bookmark in this file
> 				184 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				185 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				186 단추 (disabled) Unindent (Shift+Tab)
> 				187 단추 (disabled) Indent (Tab)
> 			188 도구 모음 Macros Manager
> 				189 메뉴 항목 Macros
> 			190 도구 모음 Layout Manager
> 				191 메뉴 항목 Layouts
> 			192 도구 모음 Toolbox
> 				193 단추 DataAnalyzer
> 				194 메뉴 항목 Toolbar Options
> 			195 도구 모음 Net Edit
> 				196 단추 (disabled) Select
> 				197 메뉴 항목 Toolbar Options
> 			198 도구 모음 Debug
> 				199 단추 Go online (Alt+F6)
> 				200 단추 Change Online Settings
> 				201 메뉴 항목 Online Connection
> 				202 단추 (disabled) Set Online Connection For Current Project
> 				203 단추 (disabled) Download (F6)
> 				204 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				205 단추 (disabled) Download Module on the Fly
> 				206 단추 (disabled) Save Project on PLC
> 				207 단추 (disabled) Start (F7)
> 				208 단추 (disabled) Reset (F8)
> 				209 단추 Toggle breakpoint (F4)
> 				210 단추 Create condition breakpoint
> 				211 메뉴 항목 Toolbar Options
> 			212 도구 모음 Build
> 				213 메뉴 항목 Target Architecture
> 				214 단추 Build changes (F9)
> 				215 단추 Rebuild project (Strg+F9)
> 				216 단추 (disabled) Cancel building (Ctrl+Break)
> 				217 단추 Link project
> 			218 도구 모음 Standard
> 				219 단추 New project (Strg+N)
> 				220 단추 Open a file (Strg+Shift+O)
> 				221 단추 Close active document (Strg+F4)
> 				222 단추 (disabled) Save file (Strg+S)
> 				223 단추 Open project (Strg+O)
> 				224 단추 (disabled) Save project changes (Strg+Shift+S)
> 				225 단추 Close project
> 				226 단추 Print
> 				227 단추 Cut (Strg+X)
> 				228 단추 Copy (Strg+C)
> 				229 단추 Paste (Strg+V)
> 				230 메뉴 항목 Undo (Strg+Z)
> 				231 메뉴 항목 (disabled) Redo (Strg+Y)
> 				232 단추 Navigate Backward (Alt+Left)
> 				233 단추 (disabled) Navigate Forward (Alt +Right)
> 			234 메뉴 모음 Menu Bar
> 				235 메뉴 항목 FILE
> 				236 메뉴 항목 EDIT
> 				237 메뉴 항목 VIEW
> 				238 메뉴 항목 PROJECT
> 				239 메뉴 항목 BUILD
> 				240 메뉴 항목 DEBUG
> 				241 메뉴 항목 ANALYZE
> 				242 메뉴 항목 TOOLS
> 				243 메뉴 항목 EXTRAS
> 				244 메뉴 항목 WINDOW
> 				245 메뉴 항목 HELP
> 		246 창 Splitter ID: 481389416
> 		247 창 Splitter ID: 481390928
> 		248 Tab Output ID: 409867992
> 			249 창 ID: 1200
> 				250 창 ID: 1200
> 					251 LIST ID: 1204
> 						252 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							253 단추 위쪽 스크롤 화살표 ID: UpButton
> 							254 단추 페이지 위로 ID: UpPageButton
> 							255 위치 조정 위치 ID: ScrollbarThumb
> 							256 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 						259 목록 항목 (selectable)
> 						260 목록 항목 (selectable)
> 						261 목록 항목 (selectable)
> 						262 목록 항목 (selectable)
> 						263 목록 항목 (selectable)
> 						264 목록 항목 (selectable)
> 						265 목록 항목 (selectable)
> 						266 목록 항목 (selectable)
> 					267 스크롤 막대 ID: 59904
> 						268 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						269 위치 조정 위치 ID: ScrollbarThumb
> 						270 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			271 탭 항목 (selectable) Python Script
> 			272 탭 항목 (selectable) Debugger
> 			273 탭 항목 (selectable) Output
> 			274 단추 Close
> 		275 창 Splitter ID: 608798568
> 		276 Tab Global View ID: 409868448
> 			277 트리 ID: 105
> 				278 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					279 콘솔 트리 (selectable, disabled) Types
> 					280 콘솔 트리 (selectable, disabled) Variables
> 			281 탭 항목 (selectable) Lib
> 			282 탭 항목 (selectable) File
> 			283 탭 항목 (selectable) Global
> 			284 단추 Close
> 		285 Tab Properties ID: 409871640
> 			286 창 ID: 121918456
> 				287 TABLE Properties Window ID: 127184512
> 				288 도구 모음 ID: 59392
> 					289 단추
> 					290 단추
> 			291 탭 항목 (selectable) Properties
> 			292 단추 Close
> 		293 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			294 단추 예(Y) ID: 6
> 			295 단추 아니요(N) ID: 7
> 			296 이미지 ID: 20
> 			297 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			298 제목 표시줄
> 				299 단추 (disabled) 닫기
>
> The focused UI element is 1 대화 상자 Lasal Class 2 Secondary Actions: Raise.
> ```
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
>   "code": "{\n  const observation = globalThis.state;\n  if (!observation?.accessibility) throw new Error(\"Need fresh dialog state\");\n  globalThis.state = null;\n  await sky.click({window: observation.window, element_index: 3});\n  globalThis.state = await sky.get_window_state({window: observation.window, include_screenshot: true, include_text: true});\n  globalThis.targetWindow = state.window;\n  nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || \"\"));\n}",
>   "title": "미사용 라이브러리 유지 선택",
>   "timeout_ms": 30000
> }

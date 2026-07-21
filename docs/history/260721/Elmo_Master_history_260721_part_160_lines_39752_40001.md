> 								96 콘솔 트리 (selectable, disabled) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->
> 								97 콘솔 트리 (selectable, disabled) Sdias Retry Counter (RetryCounter) <-[]->
> 								98 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->
> 								99 콘솔 트리 (selectable, disabled) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->
> 								100 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->
> 								101 콘솔 트리 (selectable, disabled) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->
> 								102 콘솔 트리 (selectable, disabled) Sdias Manager Option Bits (ManagerOptionBits) <-[]->
> 								103 콘솔 트리 (selectable, disabled) OnBoardIO:00, CP313 IOs (CP313_IOs1)
> 								104 콘솔 트리 (selectable, disabled) ALARM:00, Empty
> 								105 콘솔 트리 (selectable, disabled) SDIAS:00, Empty
> 								106 콘솔 트리 (selectable, disabled) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)
> 								107 콘솔 트리 (selectable, disabled) MULTIVARAN:00, Empty
> 							108 콘솔 트리 (selectable, disabled) Unplaced Module(s)
> 			109 창 Elmo_4 Secondary Actions: Raise ID: 65283
> 				110 창 ID: 59648
> 					111 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						112 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							113 단추 위쪽 스크롤 화살표 ID: UpButton
> 							114 위치 조정 위치 ID: ScrollbarThumb
> 							115 단추 페이지 아래로 ID: DownPageButton
> 							116 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						117 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							118 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							119 위치 조정 위치 ID: ScrollbarThumb
> 							120 단추 페이지 오른쪽으로 ID: DownPageButton
> 							121 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						122 위치 조정 (disabled)
> 			123 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				124 창 ID: 59648
> 					125 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						126 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							127 단추 위쪽 스크롤 화살표 ID: UpButton
> 							128 위치 조정 위치 ID: ScrollbarThumb
> 							129 단추 페이지 아래로 ID: DownPageButton
> 							130 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						131 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							132 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							133 위치 조정 위치 ID: ScrollbarThumb
> 							134 단추 페이지 오른쪽으로 ID: DownPageButton
> 							135 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						136 위치 조정 (disabled)
> 			137 창 HW_Network Secondary Actions: Raise ID: 65281
> 				138 창 ID: 59648
> 					139 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						140 단추 위쪽 스크롤 화살표 ID: UpButton
> 						141 위치 조정 위치 ID: ScrollbarThumb
> 						142 단추 페이지 아래로 ID: DownPageButton
> 						143 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					144 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						145 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						146 위치 조정 위치 ID: ScrollbarThumb
> 						147 단추 페이지 오른쪽으로 ID: DownPageButton
> 						148 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					149 위치 조정 (disabled)
> 		150 상태 표시줄 ID: 59393
> 			151 텍스트
> 			152 텍스트
> 			153 텍스트
> 			154 텍스트
> 			155 텍스트
> 			156 텍스트 Offline
> 			157 텍스트
> 			158 텍스트 NUM
> 			159 텍스트
> 		160 창 xtpBarTop ID: 59419
> 			161 도구 모음 Script
> 			162 도구 모음 Edit
> 				163 단추 Toggle bookmark
> 				164 단추 (disabled) Previous bookmark
> 				165 단추 (disabled) Next bookmark
> 				166 단추 (disabled) Delete all bookmarks
> 				167 단추 (disabled) Previous bookmark in this file
> 				168 단추 (disabled) Next bookmark in this file
> 				169 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				170 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				171 단추 (disabled) Unindent (Shift+Tab)
> 				172 단추 (disabled) Indent (Tab)
> 			173 도구 모음 Macros Manager
> 				174 메뉴 항목 Macros
> 			175 도구 모음 Layout Manager
> 				176 메뉴 항목 Layouts
> 			177 도구 모음 Toolbox
> 				178 단추 DataAnalyzer
> 				179 메뉴 항목 Toolbar Options
> 			180 도구 모음 Net Edit
> 				181 단추 (disabled) Select
> 				182 메뉴 항목 Toolbar Options
> 			183 도구 모음 Debug
> 				184 단추 Go online (Alt+F6)
> 				185 단추 Change Online Settings
> 				186 메뉴 항목 Online Connection
> 				187 단추 (disabled) Set Online Connection For Current Project
> 				188 단추 (disabled) Download (F6)
> 				189 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				190 단추 (disabled) Download Module on the Fly
> 				191 단추 (disabled) Save Project on PLC
> 				192 단추 (disabled) Start (F7)
> 				193 단추 (disabled) Reset (F8)
> 				194 단추 Toggle breakpoint (F4)
> 				195 단추 Create condition breakpoint
> 				196 메뉴 항목 Toolbar Options
> 			197 도구 모음 Build
> 				198 메뉴 항목 Target Architecture
> 				199 단추 Build changes (F9)
> 				200 단추 Rebuild project (Strg+F9)
> 				201 단추 (disabled) Cancel building (Ctrl+Break)
> 				202 단추 Link project
> 			203 도구 모음 Standard
> 				204 단추 New project (Strg+N)
> 				205 단추 Open a file (Strg+Shift+O)
> 				206 단추 Close active document (Strg+F4)
> 				207 단추 (disabled) Save file (Strg+S)
> 				208 단추 Open project (Strg+O)
> 				209 단추 (disabled) Save project changes (Strg+Shift+S)
> 				210 단추 Close project
> 				211 단추 Print
> 				212 단추 Cut (Strg+X)
> 				213 단추 Copy (Strg+C)
> 				214 단추 Paste (Strg+V)
> 				215 메뉴 항목 Undo (Strg+Z)
> 				216 메뉴 항목 (disabled) Redo (Strg+Y)
> 				217 단추 Navigate Backward (Alt+Left)
> 				218 단추 (disabled) Navigate Forward (Alt +Right)
> 			219 메뉴 모음 Menu Bar
> 				220 메뉴 항목 FILE
> 				221 메뉴 항목 EDIT
> 				222 메뉴 항목 VIEW
> 				223 메뉴 항목 PROJECT
> 				224 메뉴 항목 BUILD
> 				225 메뉴 항목 DEBUG
> 				226 메뉴 항목 ANALYZE
> 				227 메뉴 항목 TOOLS
> 				228 메뉴 항목 EXTRAS
> 				229 메뉴 항목 WINDOW
> 				230 메뉴 항목 HELP
> 		231 창 Splitter ID: 411855768
> 		232 창 Splitter ID: 411851736
> 		233 Tab Output ID: 409867992
> 			234 창 ID: 1200
> 				235 창 ID: 1200
> 					236 LIST ID: 1201
> 						237 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							238 단추 위쪽 스크롤 화살표 ID: UpButton
> 							239 단추 페이지 위로 ID: UpPageButton
> 							240 위치 조정 위치 ID: ScrollbarThumb
> 							241 단추 아래쪽 스크롤 화살표 ID: DownButton
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
> 						265 목록 항목 (selectable)
> 						266 목록 항목 (selectable)
> 						267 목록 항목 (selectable)
> 						268 목록 항목 (selectable)
> 						269 목록 항목 (selectable)
> 						270 목록 항목 (selectable)
> 						271 목록 항목 (selectable)
> 					272 스크롤 막대 ID: 59904
> 						273 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						274 위치 조정 위치 ID: ScrollbarThumb
> 						275 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			276 탭 항목 (selectable) Python Script
> 			277 탭 항목 (selectable) Output
> 			278 탭 항목 (selectable) Debugger
> 			279 단추 Close
> 		280 창 Splitter ID: 411854424
> 		281 Tab Class View ID: 409868448
> 			282 트리 ID: 103
> 				283 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					284 단추 위쪽 스크롤 화살표 ID: UpButton
> 					285 위치 조정 위치 ID: ScrollbarThumb
> 					286 단추 페이지 아래로 ID: DownPageButton
> 					287 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				288 콘솔 트리 (selectable, disabled) Elmo_EtherCAT_Test_4Axis
> 					289 콘솔 트리 (selectable, disabled) External
> 					290 콘솔 트리 (selectable, disabled) Sigmatek
> 					291 콘솔 트리 (selectable, disabled) _TCPIPServer_RT
> 					292 콘솔 트리 (selectable, disabled) Elmo_1
> 					293 콘솔 트리 (selectable, disabled) Elmo_2
> 					294 콘솔 트리 (selectable, disabled) Elmo_3
> 					295 콘솔 트리 (selectable, disabled) Elmo_4
> 					296 콘솔 트리 (selectable, disabled) LMCDiagnosticsService
> 						297 콘솔 트리 (selectable, disabled) Servers
> 						298 콘솔 트리 (selectable, disabled) Clients
> 							299 콘솔 트리 (selectable, disabled) InputLatch
> 							300 콘솔 트리 (selectable, disabled) RecorderStore
> 						301 콘솔 트리 (selectable, disabled) Methods
> 						302 콘솔 트리 (selectable, disabled) Variables
> 						303 콘솔 트리 (selectable, disabled) Objects
> 						304 콘솔 트리 (selectable, disabled) Dependencies
> 					305 콘솔 트리 (selectable, disabled) LMCEcatInputLatch
> 					306 콘솔 트리 (selectable, disabled) LMCRecorderStore
> 					307 콘솔 트리 (selectable, disabled) TCPMotionInterface
> 			308 탭 항목 (selectable) Lib
> 			309 탭 항목 (selectable) File
> 			310 탭 항목 (selectable) Global
> 			311 탭 항목 (selectable) Net
> 			312 탭 항목 (selectable) Class
> 			313 단추 Close
> 		314 Tab Properties ID: 409871640
> 			315 창 ID: 121918456
> 				316 TABLE Properties Window ID: 127184512
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			320 탭 항목 (selectable) Properties
> 			321 단추 Close
> 		322 대화 상자 Lasal Class 2 Secondary Actions: Raise
> 			323 단추 예(Y) ID: 6
> 			324 단추 아니요(N) ID: 7
> 			325 이미지 ID: 20
> 			326 텍스트 The following Libraries are not used: 1) Hardware 2) MotionLib 3) System 4) Tools Should they be removed? ID: 65535
> 			327 제목 표시줄
> 				328 단추 (disabled) 닫기
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
>   "title": "LASAL 라이브러리 제거 거부",

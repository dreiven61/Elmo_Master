> 			298 탭 항목 (selectable) Net
> 			299 탭 항목 (selectable) Class
> 			300 단추 Close
> 		301 Tab Properties ID: 125485008
> 			302 창 ID: 290002192
> 				303 TABLE Properties Window ID: 293314152
> 					304 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						305 단추 위쪽 스크롤 화살표 ID: UpButton
> 						306 위치 조정 위치 ID: ScrollbarThumb
> 						307 단추 페이지 아래로 ID: DownPageButton
> 						308 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					309 custom Name
> 					310 custom Class
> 					311 custom DataType
> 					312 custom Type
> 					313 custom Required
> 					314 custom DefaultInitValue
> 					315 custom Comment
> 				316 도구 모음 ID: 59392
> 					317 단추
> 					318 단추
> 			319 탭 항목 (selectable) Properties
> 			320 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
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
>   "title": "Drive2 클래스 선택",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:310}); globalThis.state=null; nodeRepl.write('clicked Drive2 Class row');"
> }
> ```
>
> ```text
> clicked Drive2 Class row
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 선택 상태 갱신",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||'').slice(-8000));"
> }
> ```
>
> ```text
> oductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						136 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							137 단추 위쪽 스크롤 화살표 ID: UpButton
> 							138 위치 조정 위치 ID: ScrollbarThumb
> 							139 단추 페이지 아래로 ID: DownPageButton
> 							140 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						141 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							142 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							143 위치 조정 위치 ID: ScrollbarThumb
> 							144 단추 페이지 오른쪽으로 ID: DownPageButton
> 							145 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						146 위치 조정 (disabled)
> 			147 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				148 창 ID: 59648
> 					149 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						150 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							151 단추 위쪽 스크롤 화살표 ID: UpButton
> 							152 위치 조정 위치 ID: ScrollbarThumb
> 							153 단추 페이지 아래로 ID: DownPageButton
> 							154 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						155 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							156 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							157 위치 조정 위치 ID: ScrollbarThumb
> 							158 단추 페이지 오른쪽으로 ID: DownPageButton
> 							159 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						160 위치 조정 (disabled)
> 		161 상태 표시줄 ID: 59393
> 			162 텍스트
> 			163 텍스트
> 			164 텍스트
> 			165 텍스트
> 			166 텍스트
> 			167 텍스트
> 			168 텍스트
> 			169 텍스트
> 			170 텍스트 NUM
> 			171 텍스트
> 		172 창 xtpBarTop ID: 59419
> 			173 도구 모음 Script
> 			174 도구 모음 Edit
> 				175 단추 (disabled) Toggle bookmark
> 				176 단추 (disabled) Previous bookmark
> 				177 단추 (disabled) Next bookmark
> 				178 단추 (disabled) Delete all bookmarks
> 				179 단추 (disabled) Previous bookmark in this file
> 				180 단추 (disabled) Next bookmark in this file
> 				181 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				182 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				183 단추 (disabled) Unindent (Shift+Tab)
> 				184 단추 (disabled) Indent (Tab)
> 			185 도구 모음 Macros Manager
> 				186 메뉴 항목 Macros
> 			187 도구 모음 Layout Manager
> 				188 메뉴 항목 Layouts
> 			189 도구 모음 Toolbox
> 				190 단추 DataAnalyzer
> 				191 메뉴 항목 Toolbar Options
> 			192 도구 모음 Net Edit
> 				193 단추 (disabled) Select
> 				194 메뉴 항목 Toolbar Options
> 			195 도구 모음 Debug
> 				196 단추 Go online (Alt+F6)
> 				197 단추 Change Online Settings
> 				198 메뉴 항목 Online Connection
> 				199 단추 (disabled) Set Online Connection For Current Project
> 				200 단추 (disabled) Download (F6)
> 				201 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				202 단추 (disabled) Download Module on the Fly
> 				203 단추 (disabled) Save Project on PLC
> 				204 단추 (disabled) Start (F7)
> 				205 단추 (disabled) Reset (F8)
> 				206 단추 (disabled) Toggle breakpoint (F4)
> 				207 단추 (disabled) Create condition breakpoint
> 				208 메뉴 항목 Toolbar Options
> 			209 도구 모음 Build
> 				210 메뉴 항목 Target Architecture
> 				211 단추 Build changes (F9)
> 				212 단추 Rebuild project (Strg+F9)
> 				213 단추 (disabled) Cancel building (Ctrl+Break)
> 				214 단추 Link project
> 			215 도구 모음 Standard
> 				216 단추 New project (Strg+N)
> 				217 단추 Open a file (Strg+Shift+O)
> 				218 단추 Close active document (Strg+F4)
> 				219 단추 (disabled) Save file (Strg+S)
> 				220 단추 Open project (Strg+O)
> 				221 단추 Save project changes (Strg+Shift+S)
> 				222 단추 Close project
> 				223 단추 Print
> 				224 단추 Cut (Strg+X)
> 				225 단추 Copy (Strg+C)
> 				226 단추 Paste (Strg+V)
> 				227 메뉴 항목 Undo (Strg+Z)
> 				228 메뉴 항목 (disabled) Redo (Strg+Y)
> 				229 단추 Navigate Backward (Alt+Left)
> 				230 단추 (disabled) Navigate Forward (Alt +Right)
> 			231 메뉴 모음 Menu Bar
> 				232 메뉴 항목 FILE
> 				233 메뉴 항목 EDIT
> 				234 메뉴 항목 VIEW
> 				235 메뉴 항목 PROJECT
> 				236 메뉴 항목 BUILD
> 				237 메뉴 항목 DEBUG
> 				238 메뉴 항목 ANALYZE
> 				239 메뉴 항목 TOOLS
> 				240 메뉴 항목 EXTRAS
> 				241 메뉴 항목 WINDOW
> 				242 메뉴 항목 HELP
> 		243 창 Splitter ID: 125724648
> 		244 창 Splitter ID: 125724480
> 		245 Tab Output ID: 295437008
> 			246 창 ID: 1200
> 				247 창 ID: 1200
> 					248 LIST ID: 1201
> 						249 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							250 단추 위쪽 스크롤 화살표 ID: UpButton
> 							251 단추 페이지 위로 ID: UpPageButton
> 							252 위치 조정 위치 ID: ScrollbarThumb
> 							253 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						254 목록 항목 (selectable)
> 						255 목록 항목 (selectable)
> 						256 목록 항목 (selectable)
> 						257 목록 항목 (selectable)
> 						258 목록 항목 (selectable)
> 					259 스크롤 막대 ID: 59904
> 						260 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			263 탭 항목 (selectable) Python Script
> 			264 탭 항목 (selectable) Output
> 			265 탭 항목 (selectable) Debugger
> 			266 단추 Close
> 		267 창 Splitter ID: 125724144
> 		268 Tab Class View ID: 125483184
> 			269 트리 ID: 103
> 				270 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					271 단추 위쪽 스크롤 화살표 ID: UpButton
> 					272 위치 조정 위치 ID: ScrollbarThumb
> 					273 단추 페이지 아래로 ID: DownPageButton
> 					274 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				275 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					276 콘솔 트리 (selectable) External
> 					277 콘솔 트리 (selectable) Sigmatek
> 					278 콘솔 트리 (selectable) _TCPIPServer_RT
> 					279 콘솔 트리 (selectable) Elmo_1
> 					280 콘솔 트리 (selectable) Elmo_2
> 					281 콘솔 트리 (selectable) Elmo_3
> 					282 콘솔 트리 (selectable) Elmo_4
> 					283 콘솔 트리 (selectable) LMCDiagnosticsService
> 					284 콘솔 트리 (selectable) LMCEcatInputLatch
> 						285 콘솔 트리 (selectable) Servers
> 						286 콘솔 트리 (selectable) Clients
> 							287 콘솔 트리 (selectable) EcatMaster
> 							288 콘솔 트리 (selectable) Drive1
> 							289 콘솔 트리 (selectable) Drive2
> 							290 콘솔 트리 (selectable) Drive3
> 							291 콘솔 트리 (selectable) Drive4
> 						292 콘솔 트리 (selectable) Methods
> 						293 콘솔 트리 (selectable) Dependencies
> 					294 콘솔 트리 (selectable) TCPMotionInterface
> 			295 탭 항목 (selectable) Lib
> 			296 탭 항목 (selectable) File
> 			297 탭 항목 (selectable) Global
> 			298 탭 항목 (selectable) Net
> 			299 탭 항목 (selectable) Class
> 			300 단추 Close
> 		301 Tab Properties ID: 125485008
> 			302 창 ID: 290002192
> 				303 TABLE Properties Window ID: 293314152
> 					304 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						305 단추 위쪽 스크롤 화살표 ID: UpButton
> 						306 위치 조정 위치 ID: ScrollbarThumb
> 						307 단추 페이지 아래로 ID: DownPageButton
> 						308 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					309 custom Name
> 					310 custom Class
> 					311 custom DataType
> 					312 custom Type
> 					313 custom Required
> 					314 custom DefaultInitValue
> 					315 custom Comment
> 				316 도구 모음 ID: 59392
> 					317 단추
> 					318 단추
> 			319 탭 항목 (selectable) Properties
> 			320 단추 Close
>
> The focused UI element is 303 TABLE Properties Window ID: 293314152.
> ```
>
> MCP tool call
>

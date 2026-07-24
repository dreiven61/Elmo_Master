> 				153 창 ID: 59648
> 					154 창 FUNCTION VIRTUAL GLOBAL Elmo_2::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_2_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_2_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_2::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_2_SETPOS_INDEX, ELMO_2_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_2_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_2_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_2_CONTROLWORD_INDEX, ELMO_2_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_2_ACTPOS_INDEX, ELMO_2_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_2_INPUTS_DIGITALINPUTS_INDEX, ELMO_2_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_2_STATEWORD_INDEX, ELMO_2_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						155 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							156 단추 위쪽 스크롤 화살표 ID: UpButton
> 							157 위치 조정 위치 ID: ScrollbarThumb
> 							158 단추 페이지 아래로 ID: DownPageButton
> 							159 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						160 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							161 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							162 위치 조정 위치 ID: ScrollbarThumb
> 							163 단추 페이지 오른쪽으로 ID: DownPageButton
> 							164 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						165 위치 조정 (disabled)
> 		166 상태 표시줄 ID: 59393
> 			167 텍스트
> 			168 텍스트
> 			169 텍스트
> 			170 텍스트
> 			171 텍스트
> 			172 텍스트
> 			173 텍스트
> 			174 텍스트
> 			175 텍스트 NUM
> 			176 텍스트
> 		177 창 xtpBarTop ID: 59419
> 			178 도구 모음 Script
> 			179 도구 모음 Edit
> 				180 단추 (disabled) Toggle bookmark
> 				181 단추 (disabled) Previous bookmark
> 				182 단추 (disabled) Next bookmark
> 				183 단추 (disabled) Delete all bookmarks
> 				184 단추 (disabled) Previous bookmark in this file
> 				185 단추 (disabled) Next bookmark in this file
> 				186 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				187 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				188 단추 (disabled) Unindent (Shift+Tab)
> 				189 단추 (disabled) Indent (Tab)
> 			190 도구 모음 Macros Manager
> 				191 메뉴 항목 Macros
> 			192 도구 모음 Layout Manager
> 				193 메뉴 항목 Layouts
> 			194 도구 모음 Toolbox
> 				195 단추 DataAnalyzer
> 				196 메뉴 항목 Toolbar Options
> 			197 도구 모음 Net Edit
> 				198 단추 Select
> 				199 메뉴 항목 Toolbar Options
> 			200 도구 모음 Debug
> 				201 단추 Go online (Alt+F6)
> 				202 단추 Change Online Settings
> 				203 메뉴 항목 Online Connection
> 				204 단추 (disabled) Set Online Connection For Current Project
> 				205 단추 (disabled) Download (F6)
> 				206 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				207 단추 (disabled) Download Module on the Fly
> 				208 단추 (disabled) Save Project on PLC
> 				209 단추 (disabled) Start (F7)
> 				210 단추 (disabled) Reset (F8)
> 				211 단추 (disabled) Toggle breakpoint (F4)
> 				212 단추 (disabled) Create condition breakpoint
> 				213 메뉴 항목 Toolbar Options
> 			214 도구 모음 Build
> 				215 메뉴 항목 Target Architecture
> 				216 단추 Build changes (F9)
> 				217 단추 Rebuild project (Strg+F9)
> 				218 단추 (disabled) Cancel building (Ctrl+Break)
> 				219 단추 Link project
> 			220 도구 모음 Standard
> 				221 단추 New project (Strg+N)
> 				222 단추 Open a file (Strg+Shift+O)
> 				223 단추 Close active document (Strg+F4)
> 				224 단추 (disabled) Save file (Strg+S)
> 				225 단추 Open project (Strg+O)
> 				226 단추 (disabled) Save project changes (Strg+Shift+S)
> 				227 단추 Close project
> 				228 단추 Print
> 				229 단추 Cut (Strg+X)
> 				230 단추 Copy (Strg+C)
> 				231 단추 Paste (Strg+V)
> 				232 메뉴 항목 Undo (Strg+Z)
> 				233 메뉴 항목 (disabled) Redo (Strg+Y)
> 				234 단추 Navigate Backward (Alt+Left)
> 				235 단추 (disabled) Navigate Forward (Alt +Right)
> 			236 메뉴 모음 Menu Bar
> 				237 메뉴 항목 FILE
> 				238 메뉴 항목 EDIT
> 					239 메뉴 Edit
> 						240 메뉴 항목 Undo Ctrl+Z
> 						241 메뉴 항목 (disabled) Redo Ctrl+Y
> 						242 메뉴 항목 Cut Ctrl+X
> 						243 메뉴 항목 Copy Ctrl+C
> 						244 메뉴 항목 Paste Ctrl+V
> 						245 메뉴 항목 Find... Ctrl+F
> 						246 메뉴 항목 Find in Files... Ctrl+Shift+F
> 						247 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 						248 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 						249 메뉴 항목 Replace... Ctrl+R
> 						250 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 						251 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 						252 메뉴 항목 Reset Editor Colors
> 						253 메뉴 항목 Mark
> 						254 메뉴 항목 Bookmark
> 						255 메뉴 항목 Navigate Backward Alt+Left Arrow
> 						256 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 						257 메뉴 항목 Select All Ctrl+A
> 						258 메뉴 항목 Comment
> 						259 메뉴 항목 Dent
> 				260 메뉴 항목 NETEDIT
> 				261 메뉴 항목 VIEW
> 				262 메뉴 항목 PROJECT
> 				263 메뉴 항목 BUILD
> 				264 메뉴 항목 DEBUG
> 				265 메뉴 항목 ANALYZE
> 				266 메뉴 항목 TOOLS
> 				267 메뉴 항목 EXTRAS
> 				268 메뉴 항목 WINDOW
> 				269 메뉴 항목 HELP
> 		270 창 Splitter ID: 297810792
> 		271 창 Splitter ID: 297810456
> 		272 Tab Output ID: 295820960
> 			273 창 ID: 1200
> 				274 창 ID: 1200
> 					275 LIST ID: 1201
> 						276 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							277 단추 위쪽 스크롤 화살표 ID: UpButton
> 							278 단추 페이지 위로 ID: UpPageButton
> 							279 위치 조정 위치 ID: ScrollbarThumb
> 							280 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						281 목록 항목 (selectable)
> 						282 목록 항목 (selectable)
> 						283 목록 항목 (selectable)
> 						284 목록 항목 (selectable)
> 						285 목록 항목 (selectable)
> 					286 스크롤 막대 ID: 59904
> 						287 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						288 위치 조정 위치 ID: ScrollbarThumb
> 						289 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			290 탭 항목 (selectable) Python Script
> 			291 탭 항목 (selectable) Output
> 			292 탭 항목 (selectable) Debugger
> 			293 단추 Close
> 		294 창 Splitter ID: 297812808
> 		295 Tab Class View ID: 298069024
> 			296 트리 ID: 103
> 				297 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					298 단추 위쪽 스크롤 화살표 ID: UpButton
> 					299 위치 조정 위치 ID: ScrollbarThumb
> 					300 단추 페이지 아래로 ID: DownPageButton
> 					301 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				302 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					303 콘솔 트리 (selectable) External
> 					304 콘솔 트리 (selectable) Sigmatek
> 					305 콘솔 트리 (selectable) _TCPIPServer_RT
> 					306 콘솔 트리 (selectable) Elmo_1
> 					307 콘솔 트리 (selectable) Elmo_2
> 					308 콘솔 트리 (selectable) Elmo_3
> 					309 콘솔 트리 (selectable) Elmo_4
> 					310 콘솔 트리 (selectable) LMCDiagnosticsService
> 					311 콘솔 트리 (selectable) LMCEcatInputLatch
> 						312 콘솔 트리 (selectable) Servers
> 						313 콘솔 트리 (selectable) Clients
> 							314 콘솔 트리 (selectable) EcatMaster
> 							315 콘솔 트리 (selectable) Drive1
> 							316 콘솔 트리 (selectable) Drive2
> 							317 콘솔 트리 (selectable) Drive3
> 							318 콘솔 트리 (selectable) Drive4
> 							319 콘솔 트리 (selectable) RecorderStore
> 						320 콘솔 트리 (selectable) Methods
> 							321 콘솔 트리 (selectable) Global
> 								322 콘솔 트리 (selectable) RtWork
> 								323 콘솔 트리 (selectable) CopySnapshot
> 							324 콘솔 트리 (selectable) Private
> 						325 콘솔 트리 (selectable) Variables
> 						326 콘솔 트리 (selectable) Objects
> 							327 콘솔 트리 (selectable) LMCEcatInputLatch1
> 						328 콘솔 트리 (selectable) Dependencies
> 					329 콘솔 트리 (selectable) LMCRecorderStore
> 					330 콘솔 트리 (selectable) TCPMotionInterface
> 			331 탭 항목 (selectable) Lib
> 			332 탭 항목 (selectable) File
> 			333 탭 항목 (selectable) Global
> 			334 탭 항목 (selectable) Net
> 			335 탭 항목 (selectable) Class
> 			336 단추 Close
> 		337 Tab Properties ID: 298070392
> 			338 창 ID: 289142312
> 				339 TABLE Properties Window ID: 293586600
> 					340 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						341 단추 위쪽 스크롤 화살표 ID: UpButton
> 						342 위치 조정 위치 ID: ScrollbarThumb
> 						343 단추 페이지 아래로 ID: DownPageButton
> 						344 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					345 custom Name
> 					346 custom Class
> 					347 custom DataType
> 					348 custom Type
> 					349 custom Required
> 					350 custom DefaultInitValue
> 					351 custom Comment
> 				352 도구 모음 ID: 59392
> 					353 단추
> 					354 단추
> 			355 탭 항목 (selectable) Properties
> 			356 단추 Close
> 		357 메뉴 Edit
> 			358 메뉴 항목 Undo Ctrl+Z
> 			359 메뉴 항목 (disabled) Redo Ctrl+Y
> 			360 메뉴 항목 Cut Ctrl+X
> 			361 메뉴 항목 Copy Ctrl+C
> 			362 메뉴 항목 Paste Ctrl+V
> 			363 메뉴 항목 Find... Ctrl+F
> 			364 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			365 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			366 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			367 메뉴 항목 Replace... Ctrl+R
> 			368 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			369 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 			370 메뉴 항목 Reset Editor Colors
> 			371 메뉴 항목 Mark
> 			372 메뉴 항목 Bookmark
> 			373 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			374 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			375 메뉴 항목 Select All Ctrl+A
> 			376 메뉴 항목 Comment
> 			377 메뉴 항목 Dent
>
> The focused UI element is 238 메뉴 항목 EDIT.
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
>   "code": "await sky.press_key({window:lasalWindow,key:\"Escape\"}); nodeRepl.write(\"closed\");",
>   "title": "LASAL 메뉴 닫기"
> }
> ```
>

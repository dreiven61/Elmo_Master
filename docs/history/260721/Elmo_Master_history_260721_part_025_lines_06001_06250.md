> 				134 창 ID: 59648
> 					135 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
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
> 				270 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					271 콘솔 트리 (selectable) External
> 					272 콘솔 트리 (selectable) Sigmatek
> 					273 콘솔 트리 (selectable) _TCPIPServer_RT
> 					274 콘솔 트리 (selectable) Elmo_1
> 					275 콘솔 트리 (selectable) Elmo_2
> 					276 콘솔 트리 (selectable) Elmo_3
> 					277 콘솔 트리 (selectable) Elmo_4
> 					278 콘솔 트리 (selectable) LMCEcatInputLatch
> 					279 콘솔 트리 (selectable) TCPMotionInterface
> 			280 탭 항목 (selectable) Lib
> 			281 탭 항목 (selectable) File
> 			282 탭 항목 (selectable) Global
> 			283 탭 항목 (selectable) Net
> 			284 탭 항목 (selectable) Class
> 			285 단추 Close
> 		286 Tab Properties ID: 125485008
> 			287 창 ID: 290002192
> 				288 TABLE Properties Window ID: 293314152
> 					289 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						290 단추 위쪽 스크롤 화살표 ID: UpButton
> 						291 위치 조정 위치 ID: ScrollbarThumb
> 						292 단추 페이지 아래로 ID: DownPageButton
> 						293 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					294 custom Project Version
> 					295 custom Name
> 					296 custom Revision
> 					297 custom Online Connection
> 					298 custom CodeGenerator
> 					299 custom Networks
> 					300 custom Server list
> 					301 custom Target
> 					302 custom Compiler
> 					303 custom Directory
> 					304 custom OPC-UA
> 					305 custom Load all libraries
> 					306 custom Use Unit System
> 					307 custom Include Paths
> 					308 custom Library Paths
> 					309 custom Backup Includes and Loader
> 					310 custom Ignore at Cleanup
> 					311 custom Reencrypt Project On Close
> 					312 custom Enable OPC UA
> 					313 custom Enable initvalues for output parameters
> 					314 custom Use multiple CPU core
> 					315 custom Use Advanced-IO
> 					316 custom AutomationML
> 					317 custom IO Connection Manager Options
> 					318 custom Comment
> 					319 custom Filename
> 				320 도구 모음 ID: 59392
> 					321 단추
> 					322 단추
> 			323 탭 항목 (selectable) Properties
> 			324 단추 Close
>
> The focused UI element is 269 트리 ID: 103.
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
>   "title": "LASAL 진단 클래스 생성",
>   "code": "{const observation=globalThis.state;if(!observation)throw new Error(\"No fresh observation\");globalThis.state=null;try{await sky.click({window:observation.window,x:984,y:184});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});}catch(e){globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});throw e;}globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||\"\"));}"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴
> 			4 메뉴 항목 Create Class
> 			5 메뉴 항목 Create Sfc Class
> 			6 메뉴 항목 Create Ladder Diagram Class
> 			7 메뉴 항목 Create SDCI Class
> 			8 메뉴 항목 Import Class/Network
> 			9 메뉴 항목 Update Referenced Classes
> 			10 메뉴 항목 Add Folder
> 			11 메뉴 항목 Hide User Folder
> 			12 메뉴 항목 Paste from Clipboard
> 			13 메뉴 항목 Print Project Info
> 			14 메뉴 항목 Print All Classes
> 			15 메뉴 항목 Protection Setting
> 		16 창 작업 영역 ID: 59648
> 			17 창 Hardware Editor Secondary Actions: Raise ID: 65286
> 				18 창 ID: 59648
> 					19 창 xtpBarTop ID: 59419
> 						20 도구 모음 Hardware Editor
> 							21 단추 Hardware Editor Configuration Settings
> 							22 단추 Calculate Resources of Project
> 							23 단추 (disabled) Upload Hardware Tree from PLC
> 							24 단추 (disabled) Compare Hardware Tree of project and PLC\Reset Online Compare
> 							25 단추 Show Detail Mode\Show Configuration Mode
> 							26 단추 Generates the ENI File of the current project
> 					27 창 ID: 59648
> 						28 트리 ID: 1
> 							29 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 								30 단추 위쪽 스크롤 화살표 ID: UpButton
> 								31 단추 페이지 위로 ID: UpPageButton

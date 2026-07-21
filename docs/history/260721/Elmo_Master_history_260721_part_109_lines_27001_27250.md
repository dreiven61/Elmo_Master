> 				56 창 ID: 59648
> 					57 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000
> 						58 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							59 단추 위쪽 스크롤 화살표 ID: UpButton
> 							60 위치 조정 위치 ID: ScrollbarThumb
> 							61 단추 페이지 아래로 ID: DownPageButton
> 							62 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						63 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							64 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							65 위치 조정 위치 ID: ScrollbarThumb
> 							66 단추 페이지 오른쪽으로 ID: DownPageButton
> 							67 단추 오른쪽 스크롤 화살표 ID: DownButton
> 						68 위치 조정 (disabled)
> 			69 창 Motion_Network Secondary Actions: Raise ID: 65284
> 				70 창 ID: 59648
> 					71 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						72 단추 위쪽 스크롤 화살표 ID: UpButton
> 						73 위치 조정 위치 ID: ScrollbarThumb
> 						74 단추 페이지 아래로 ID: DownPageButton
> 						75 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					76 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 						77 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						78 위치 조정 위치 ID: ScrollbarThumb
> 						79 단추 페이지 오른쪽으로 ID: DownPageButton
> 						80 단추 오른쪽 스크롤 화살표 ID: DownButton
> 					81 위치 조정 (disabled)
> 			82 창 Comm_Network Secondary Actions: Raise ID: 65283
> 				83 창 ID: 59648
> 					84 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						85 단추 위쪽 스크롤 화살표 ID: UpButton
> 						86 위치 조정 위치 ID: ScrollbarThumb
> 						87 단추 페이지 아래로 ID: DownPageButton
> 						88 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			89 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282
> 				90 창 ID: 59648
> 					91 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000
> 						92 스크롤 막대 가로 ID: NonClientHorizontalScrollBar
> 							93 단추 왼쪽 스크롤 화살표 ID: UpButton
> 							94 위치 조정 위치 ID: ScrollbarThumb
> 							95 단추 페이지 오른쪽으로 ID: DownPageButton
> 							96 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			97 창 HW_Network Secondary Actions: Raise ID: 65281
> 				98 창 ID: 59648
> 					99 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						100 단추 위쪽 스크롤 화살표 ID: UpButton
> 						101 위치 조정 위치 ID: ScrollbarThumb
> 						102 단추 페이지 아래로 ID: DownPageButton
> 						103 단추 아래쪽 스크롤 화살표 ID: DownButton
> 		104 상태 표시줄 ID: 59393
> 			105 텍스트
> 			106 텍스트
> 			107 텍스트
> 			108 텍스트
> 			109 텍스트
> 			110 텍스트 Offline
> 			111 텍스트
> 			112 텍스트 NUM
> 			113 텍스트
> 		114 창 xtpBarTop ID: 59419
> 			115 도구 모음 Script
> 			116 도구 모음 Edit
> 				117 단추 Toggle bookmark
> 				118 단추 (disabled) Previous bookmark
> 				119 단추 (disabled) Next bookmark
> 				120 단추 (disabled) Delete all bookmarks
> 				121 단추 (disabled) Previous bookmark in this file
> 				122 단추 (disabled) Next bookmark in this file
> 				123 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				124 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				125 단추 (disabled) Unindent (Shift+Tab)
> 				126 단추 (disabled) Indent (Tab)
> 			127 도구 모음 Macros Manager
> 				128 메뉴 항목 Macros
> 			129 도구 모음 Layout Manager
> 				130 메뉴 항목 Layouts
> 			131 도구 모음 Toolbox
> 				132 단추 DataAnalyzer
> 				133 단추 Interpreter
> 				134 단추 DiasDrive
> 				135 단추 PLC Diagnosis
> 				136 단추 Hardware Editor
> 				137 단추 Graphical Hardware Editor
> 				138 단추 Connection Manager
> 				139 단추 Task Configuration
> 			140 도구 모음 Net Edit
> 				141 단추 (disabled) Select
> 				142 단추 (disabled) Move view
> 				143 단추 (disabled) Insert comment
> 				144 단추 (disabled) Zoom(+/-)
> 				145 단추 (disabled) Zoom to all
> 				146 단추 (disabled) Zoom selection
> 			147 도구 모음 Debug
> 				148 단추 Go online (Alt+F6)
> 				149 단추 Change Online Settings
> 				150 메뉴 항목 Online Connection
> 				151 단추 (disabled) Set Online Connection For Current Project
> 				152 단추 (disabled) Download (F6)
> 				153 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				154 단추 (disabled) Download Module on the Fly
> 				155 단추 (disabled) Save Project on PLC
> 				156 단추 (disabled) Start (F7)
> 				157 단추 (disabled) Reset (F8)
> 				158 단추 Toggle breakpoint (F4)
> 				159 단추 Create condition breakpoint
> 				160 단추 Remove all breakpoint(s) (Shift+F4)
> 				161 단추 AWL trigger on/off
> 				162 단추 (disabled) Fix AWL trigger to current instruction
> 				163 단추 Activate/Deactivate Live View
> 				164 단추 Activate/Deactivate Auto Watch
> 				165 단추 (disabled) Goto instruction pointer
> 				166 단추 (disabled) Step into (F5)
> 				167 단추 (disabled) Step over (Alt+F5)
> 				168 단추 (disabled) Step out (Shift+F5)
> 				169 단추 (disabled) Set instruction pointer
> 			170 도구 모음 Build
> 				171 메뉴 항목 Target Architecture
> 				172 단추 Build changes (F9)
> 				173 단추 Rebuild project (Strg+F9)
> 				174 단추 (disabled) Cancel building (Ctrl+Break)
> 				175 단추 Link project
> 			176 도구 모음 Standard
> 				177 단추 New project (Strg+N)
> 				178 단추 Open a file (Strg+Shift+O)
> 				179 단추 Close active document (Strg+F4)
> 				180 단추 (disabled) Save file (Strg+S)
> 				181 단추 Open project (Strg+O)
> 				182 단추 (disabled) Save project changes (Strg+Shift+S)
> 				183 단추 Close project
> 				184 단추 Print
> 				185 단추 Cut (Strg+X)
> 				186 단추 Copy (Strg+C)
> 				187 단추 Paste (Strg+V)
> 				188 메뉴 항목 (disabled) Undo (Strg+Z)
> 				189 메뉴 항목 (disabled) Redo (Strg+Y)
> 				190 단추 Navigate Backward (Alt+Left)
> 				191 단추 (disabled) Navigate Forward (Alt +Right)
> 			192 메뉴 모음 Menu Bar
> 				193 메뉴 항목 FILE
> 				194 메뉴 항목 EDIT
> 				195 메뉴 항목 VIEW
> 				196 메뉴 항목 PROJECT
> 				197 메뉴 항목 BUILD
> 				198 메뉴 항목 DEBUG
> 				199 메뉴 항목 ANALYZE
> 				200 메뉴 항목 TOOLS
> 				201 메뉴 항목 EXTRAS
> 				202 메뉴 항목 WINDOW
> 				203 메뉴 항목 HELP
> 		204 창 Splitter ID: 302431488
> 		205 창 Splitter ID: 302437704
> 		206 Tab Output ID: 297478928
> 			207 창 ID: 1200
> 				208 창 ID: 1200
> 					209 LIST ID: 1201
> 						210 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							211 단추 위쪽 스크롤 화살표 ID: UpButton
> 							212 위치 조정 위치 ID: ScrollbarThumb
> 							213 단추 페이지 아래로 ID: DownPageButton
> 							214 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						215 목록 항목 (selectable)
> 						216 목록 항목 (selectable)
> 						217 목록 항목 (selectable)
> 						218 목록 항목 (selectable)
> 						219 목록 항목 (selectable)
> 						220 목록 항목 (selectable)
> 						221 목록 항목 (selectable)
> 						222 목록 항목 (selectable)
> 						223 목록 항목 (selectable)
> 						224 목록 항목 (selectable)
> 						225 목록 항목 (selectable)
> 						226 목록 항목 (selectable)
> 						227 목록 항목 (selectable)
> 					228 스크롤 막대 ID: 59904
> 						229 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						230 위치 조정 위치 ID: ScrollbarThumb
> 						231 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			232 탭 항목 (selectable) Python Script
> 			233 탭 항목 (selectable) Output
> 			234 탭 항목 (selectable) Debugger
> 			235 단추 Close
> 		236 창 Splitter ID: 302436696
> 		237 Tab Class View ID: 297479384
> 			238 트리 ID: 103
> 				239 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					240 콘솔 트리 (selectable) External
> 					241 콘솔 트리 (selectable) Sigmatek
> 					242 콘솔 트리 (selectable) _TCPIPServer_RT
> 					243 콘솔 트리 (selectable) Elmo_1
> 					244 콘솔 트리 (selectable) Elmo_2
> 					245 콘솔 트리 (selectable) Elmo_3
> 					246 콘솔 트리 (selectable) Elmo_4
> 					247 콘솔 트리 (selectable) LMCDiagnosticsService
> 					248 콘솔 트리 (selectable) LMCEcatInputLatch
> 					249 콘솔 트리 (selectable) TCPMotionInterface
> 			250 탭 항목 (selectable) Lib
> 			251 탭 항목 (selectable) File
> 			252 탭 항목 (selectable) Global
> 			253 탭 항목 (selectable) Net
> 			254 탭 항목 (selectable) Class
> 			255 단추 Close
> 		256 Tab Properties ID: 302395640
> 			257 창 ID: 291858232
> 				258 TABLE Properties Window ID: 297584112
> 					259 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						260 단추 위쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 페이지 아래로 ID: DownPageButton
> 						263 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					264 custom Name
> 					265 custom Revision
> 					266 custom GUID
> 					267 custom Task Settings
> 					268 custom Sigmatek
> 					269 custom OSInterface
> 					270 custom IconPath
> 					271 custom SharedCommandTable
> 					272 custom Objectsize
> 					273 custom Singleton
> 					274 custom Hidden
> 					275 custom Deprecated
> 					276 custom GCCOptions
> 					277 custom Comment
> 					278 custom Filename
> 				279 도구 모음 ID: 59392
> 					280 단추
> 					281 단추
> 			282 탭 항목 (selectable) Properties
> 			283 단추 Close
>
> The focused UI element is 209 LIST ID: 1201.
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var o7=globalThis.state;var sid7=o7.screenshots?.[0]?.id;if(!sid7)throw new Error('no screenshot');globalThis.state=null;await sky.click({window:o7.window,screenshotId:sid7,x:1442,y:341});globalThis.state=await sky.get_window_state({window:o7.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "RT latch 클래스 확장"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창 작업 영역 ID: 59648
> 			2 창 TCPMotionInterface Secondary Actions: Raise ID: 65280

> 		287 Tab Properties ID: 409871640
> 			288 창 ID: 121918456
> 				289 TABLE Properties Window ID: 127184512
> 					290 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						291 단추 위쪽 스크롤 화살표 ID: UpButton
> 						292 위치 조정 위치 ID: ScrollbarThumb
> 						293 단추 페이지 아래로 ID: DownPageButton
> 						294 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					295 custom Project Version
> 					296 custom Name
> 					297 custom Revision
> 					298 custom Online Connection
> 					299 custom CodeGenerator
> 					300 custom Networks
> 					301 custom Server list
> 					302 custom Target
> 					303 custom Compiler
> 					304 custom Directory
> 					305 custom OPC-UA
> 					306 custom Load all libraries
> 					307 custom Use Unit System
> 					308 custom Include Paths
> 					309 custom Library Paths
> 					310 custom Backup Includes and Loader
> 					311 custom Ignore at Cleanup
> 					312 custom Reencrypt Project On Close
> 					313 custom Enable OPC UA
> 					314 custom Enable initvalues for output parameters
> 					315 custom Use multiple CPU core
> 					316 custom Use Advanced-IO
> 					317 custom AutomationML
> 					318 custom IO Connection Manager Options
> 					319 custom Comment
> 					320 custom Filename
> 				321 도구 모음 ID: 59392
> 					322 단추
> 					323 단추
> 			324 탭 항목 (selectable) Properties
> 			325 단추 Close
>
> The focused UI element is 273 트리 ID: 102.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.perform_secondary_action({window:globalThis.targetWindow,element_index:277,action:\"Expand\"}); nodeRepl.write(\"expanded source files\");",
>   "title": "Expand LASAL source files",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> element 277 has no cached secondary actions for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); nodeRepl.write(JSON.stringify(globalThis.state.screenshots.map(s=>({id:s.id,z:s.zIndex,w:s.width,h:s.height}))));",
>   "title": "Refresh LASAL source tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [{"id":"screenshot-0","z":0,"w":1450,"h":763},{"id":"screenshot-1","z":1,"w":1440,"h":753}]
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
>   "code": "await sky.click({window:globalThis.targetWindow,x:1018,y:225,click_count:2,screenshotId:\"screenshot-1\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); var treeSrc=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeSrc.split(\"\\n\").slice(-180).join(\"\\n\"));",
>   "title": "Expand LASAL source files",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=globalThis.state.window; nodeRepl.write(JSON.stringify({title:globalThis.state.window?.title,tree:(globalThis.state.accessibility?.tree||\"\").slice(-12000)}));",
>   "title": "Check expanded LASAL source tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface","tree":" Editor\n\t\t\t\t\t\t\t51 단추 Hardware Editor Configuration Settings\n\t\t\t\t\t\t\t52 단추 Calculate Resources of Project\n\t\t\t\t\t\t\t53 단추 (disabled) Upload Hardware Tree from PLC\n\t\t\t\t\t\t\t54 단추 (disabled) Compare Hardware Tree of project and PLC\\Reset Online Compare\n\t\t\t\t\t\t\t55 단추 Show Detail Mode\\Show Configuration Mode\n\t\t\t\t\t\t\t56 단추 Generates the ENI File of the current project\n\t\t\t\t\t57 창 ID: 59648\n\t\t\t\t\t\t58 트리 ID: 1\n\t\t\t\t\t\t\t59 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t\t60 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t\t61 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t\t62 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t\t63 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t\t64 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t\t65 콘솔 트리 (selectable) CP313 (EtherCAT_PLC1, HwControl1, SdiasPLC1)\n\t\t\t\t\t\t\t\t66 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster\n\t\t\t\t\t\t\t\t67 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->\n\t\t\t\t\t\t\t\t68 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->\n\t\t\t\t\t\t\t\t69 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->\n\t\t\t\t\t\t\t\t70 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->\n\t\t\t\t\t\t\t\t71 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->\n\t\t\t\t\t\t\t\t72 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->\n\t\t\t\t\t\t\t\t73 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->\n\t\t\t\t\t\t\t\t74 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->\n\t\t\t\t\t\t\t\t75 콘솔 트리 (selectable) EtherCAT Missed Frame Counter (MissedFrameCounter) <-[]->\n\t\t\t\t\t\t\t\t76 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->\n\t\t\t\t\t\t\t\t77 콘솔 트리 (selectable) Sdias Firmware Version (FirmwareVersion) <-[]->\n\t\t\t\t\t\t\t\t78 콘솔 트리 (selectable) Sdias Manager Protocol Version (ManagerProtocolVersion) <-[]->\n\t\t\t\t\t\t\t\t79 콘솔 트리 (selectable) Sdias Retry Counter (RetryCounter) <-[]->\n\t\t\t\t\t\t\t\t80 콘솔 트리 (selectable) Sdias Iso Wr Task Duration (IsoWrTaskDuration) <-[]->\n\t\t\t\t\t\t\t\t81 콘솔 트리 (selectable) Sdias Iso Wr Task Duration Max (IsoWrTaskMaxDuration) <-[]->\n\t\t\t\t\t\t\t\t82 콘솔 트리 (selectable) Sdias Iso Rd Task Duration (IsoRdTaskDuration) <-[]->\n\t\t\t\t\t\t\t\t83 콘솔 트리 (selectable) Sdias Iso Rd Task Duration Max (IsoRdTaskMaxDuration) <-[]->\n\t\t\t\t\t\t\t\t84 콘솔 트리 (selectable) Sdias Manager Option Bits (ManagerOptionBits) <-[]->\n\t\t\t\t\t\t\t\t85 콘솔 트리 (selectable) OnBoardIO:00, CP313 IOs (CP313_IOs1)\n\t\t\t\t\t\t\t\t86 콘솔 트리 (selectable) ALARM:00, Empty\n\t\t\t\t\t\t\t\t87 콘솔 트리 (selectable) SDIAS:00, Empty\n\t\t\t\t\t\t\t\t88 콘솔 트리 (selectable) EtherCAT_ETHERNET:01, Elmo_1 (Elmo_11)\n\t\t\t\t\t\t\t\t89 콘솔 트리 (selectable) MULTIVARAN:00, Empty\n\t\t\t\t\t\t\t90 콘솔 트리 (selectable) Unplaced Module(s)\n\t\t\t91 창 Elmo_4 Secondary Actions: Raise ID: 65285\n\t\t\t\t92 창 ID: 59648\n\t\t\t\t\t93 창 FUNCTION VIRTUAL GLOBAL Elmo_4::Outputs_DigitalOutputs::Write VAR_INPUT input (EAX) : DINT; END_VAR VAR_OUTPUT result (EAX) : DINT; END_VAR Outputs_DigitalOutputs := input; result := Outputs_DigitalOutputs; END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::CheckProductCode VAR_INPUT udProductCodeToCheck : UDINT; udVendorIDToCheck : UDINT; END_VAR VAR_OUTPUT bIsOK : BOOL; END_VAR bIsOK := (udProductCodeToCheck = ELMO_4_ETHERCATSLAVE_PRODUCT_CODE); bIsOK := bIsOK (udVendorIDToCheck = ELMO_4_ETHERCATSLAVE_VENDOR_ID); END_FUNCTION FUNCTION VIRTUAL GLOBAL Elmo_4::SetPDOSettings VAR_OUTPUT retcode : DINT; END_VAR retcode := ECAT_DS402Base::SetPDOSettings(); if retcode <> 0 then return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #SetPos.dData, ELMO_4_SETPOS_INDEX, ELMO_4_SETPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #Outputs_DigitalOutputs.dData, ELMO_4_OUTPUTS_DIGITALOUTPUTS_INDEX, ELMO_4_OUTPUTS_DIGITALOUTPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_OUTPUT, #ControlWord.dData, ELMO_4_CONTROLWORD_INDEX, ELMO_4_CONTROLWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #ActPos.dData, ELMO_4_ACTPOS_INDEX, ELMO_4_ACTPOS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #Inputs_DigitalInputs.dData, ELMO_4_INPUTS_DIGITALINPUTS_INDEX, ELMO_4_INPUTS_DIGITALINPUTS_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; retcode := AddPDOEntry(ECAT_PDO_TYPE_INPUT, #StateWord.dData, ELMO_4_STATEWORD_INDEX, ELMO_4_STATEWORD_SUBINDEX); if retcode <> 0 then AddPDOEntryFailed(retcode); return; end_if; END_FUNCTION ID: 10000\n\t\t\t\t\t\t94 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t95 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t96 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t97 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t98 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t99 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t100 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t101 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t102 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t103 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t104 위치 조정 (disabled)\n\t\t\t105 창 Motion_Network Secondary Actions: Raise ID: 65284\n\t\t\t\t106 창 ID: 59648\n\t\t\t\t\t107 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t108 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t109 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t110 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t111 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t112 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t113 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t114 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t115 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t116 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t117 위치 조정 (disabled)\n\t\t\t118 창 Comm_Network Secondary Actions: Raise ID: 65283\n\t\t\t\t119 창 ID: 59648\n\t\t\t\t\t120 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t121 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t122 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t123 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t124 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t125 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t126 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t127 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t128 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t129 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t130 위치 조정 (disabled)\n\t\t\t131 창 _TCPIPServer_RT Secondary Actions: Raise ID: 65282\n\t\t\t\t132 창 ID: 59648\n\t\t\t\t\t133 창 FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR // TCP transport is owned by CyWork. RtWork must not execute socket handling. state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR if (bdStatus.CyclicTask = true) then CyclicCall(); end_if; state := READY; END_FUNCTION ID: 10000\n\t\t\t\t\t\t134 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t135 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t136 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t137 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t\t138 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t139 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t\t140 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t141 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t142 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t\t143 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t144 위치 조정 (disabled)\n\t\t\t145 창 HW_Network Secondary Actions: Raise ID: 65281\n\t\t\t\t146 창 ID: 59648\n\t\t\t\t\t147 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t148 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t149 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t150 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t151 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t152 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t153 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t154 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t155 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t156 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t157 위치 조정 (disabled)\n\t\t158 상태 표시줄 ID: 59393\n\t\t\t159 텍스트 \n\t\t\t160 텍스트\n\t\t\t161 텍스트\n\t\t\t162 텍스트\n\t\t\t163 텍스트 \n\t\t\t164 텍스트 Offline\n\t\t\t165 텍스트\n\t\t\t166 텍스트 NUM\n\t\t\t167 텍스트\n\t\t168 창 xtpBarTop ID: 59419\n\t\t\t169 도구 모음 Script\n\t\t\t170 도구 모음 Edit\n\t\t\t\t171 단추 Toggle bookmark\n\t\t\t\t172 단추 (disabled) Previous bookmark\n\t\t\t\t173 단추 (disabled) Next bookmark\n\t\t\t\t174 단추 (disabled) Delete all bookmarks\n\t\t\t\t175 단추 (disabled) Previous bookmark in this file\n\t\t\t\t176 단추 (disabled) Next bookmark in this file\n\t\t\t\t177 단추 (disabled) Comment selected text (Ctrl+Shift+C)\n\t\t\t\t178 단추 (disabled) Remove comment (Ctrl+Shift+X)\n\t\t\t\t179 단추 (disabled) Unindent (Shift+Tab)\n\t\t\t\t180 단추 (disabled) Indent (Tab)\n\t\t\t181 도구 모음 Macros Manager\n\t\t\t\t182 메뉴 항목 Macros\n\t\t\t183 도구 모음 Layout Manager\n\t\t\t\t184 메뉴 항목 Layouts\n\t\t\t185 도구 모음 Toolbox\n\t\t\t\t186 단추 DataAnalyzer\n\t\t\t\t187 메뉴 항목 Toolbar Options\n\t\t\t188 도구 모음 Net Edit\n\t\t\t\t189 단추 (disabled) Select\n\t\t\t\t190 메뉴 항목 Toolbar Options\n\t\t\t191 도구 모음 Debug\n\t\t\t\t192 단추 Go online (Alt+F6)\n\t\t\t\t193 단추 Change Online Settings\n\t\t\t\t194 메뉴 항목 Online Connection\n\t\t\t\t195 단추 (disabled) Set Online Connection For Current Project\n\t\t\t\t196 단추 (disabled) Download (F6)\n\t\t\t\t197 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)\n\t\t\t\t198 단추 (disabled) Download Module on the Fly\n\t\t\t\t199 단추 (disabled) Save Project on PLC\n\t\t\t\t200 단추 (disabled) Start (F7)\n\t\t\t\t201 단추 (disabled) Reset (F8)\n\t\t\t\t202 단추 Toggle breakpoint (F4)\n\t\t\t\t203 단추 Create condition breakpoint\n\t\t\t\t204 메뉴 항목 Toolbar Options\n\t\t\t205 도구 모음 Build\n\t\t\t\t206 메뉴 항목 Target Architecture\n\t\t\t\t207 단추 Build changes (F9)\n\t\t\t\t208 단추 Rebuild project (Strg+F9)\n\t\t\t\t209 단추 (disabled) Cancel building (Ctrl+Break)\n\t\t\t\t210 단추 Link project\n\t\t\t211 도구 모음 Standard\n\t\t\t\t212 단추 New project (Strg+N)\n\t\t\t\t213 단추 Open a file (Strg+Shift+O)\n\t\t\t\t214 단추 Close active document (Strg+F4)\n\t\t\t\t215 단추 (disabled) Save file (Strg+S)\n\t\t\t\t216 단추 Open project (Strg+O)\n\t\t\t\t217 단추 (disabled) Save project changes (Strg+Shift+S)\n\t\t\t\t218 단추 Close project\n\t\t\t\t219 단추 Print\n\t\t\t\t220 단추 Cut (Strg+X)\n\t\t\t\t221 단추 Copy (Strg+C)\n\t\t\t\t222 단추 Paste (Strg+V)\n\t\t\t\t223 메뉴 항목 (disabled) Undo (Strg+Z)\n\t\t\t\t224 메뉴 항목 (disabled) Redo (Strg+Y)\n\t\t\t\t225 단추 Navigate Backward (Alt+Left)\n\t\t\t\t226 단추 (disabled) Navigate Forward (Alt +Right)\n\t\t\t227 메뉴 모음 Menu Bar\n\t\t\t\t228 메뉴 항목 FILE\n\t\t\t\t229 메뉴 항목 EDIT\n\t\t\t\t230 메뉴 항목 VIEW\n\t\t\t\t231 메뉴 항목 PROJECT\n\t\t\t\t232 메뉴 항목 BUILD\n\t\t\t\t233 메뉴 항목 DEBUG\n\t\t\t\t234 메뉴 항목 ANALYZE\n\t\t\t\t235 메뉴 항목 TOOLS\n\t\t\t\t236 메뉴 항목 EXTRAS\n\t\t\t\t237 메뉴 항목 WINDOW\n\t\t\t\t238 메뉴 항목 HELP\n\t\t239 창 Splitter ID: 481389416\n\t\t240 창 Splitter ID: 481390928\n\t\t241 Tab Output ID: 409867992\n\t\t\t242 창 ID: 1200\n\t\t\t\t243 창 ID: 1200\n\t\t\t\t\t244 LIST ID: 1201\n\t\t\t\t\t\t245 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t\t246 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t\t247 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t\t248 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t\t249 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t\t250 목록 항목 (selectable)\n\t\t\t\t\t\t251 목록 항목 (selectable)\n\t\t\t\t\t\t252 목록 항목 (selectable)\n\t\t\t\t\t\t253 목록 항목 (selectable)\n\t\t\t\t\t\t254 목록 항목 (selectable)\n\t\t\t\t\t\t255 목록 항목 (selectable)\n\t\t\t\t\t\t256 목록 항목 (selectable)\n\t\t\t\t\t\t257 목록 항목 (selectable)\n\t\t\t\t\t\t258 목록 항목 (selectable)\n\t\t\t\t\t\t259 목록 항목 (selectable)\n\t\t\t\t\t\t260 목록 항목 (selectable)\n\t\t\t\t\t\t261 목록 항목 (selectable)\n\t\t\t\t\t\t262 목록 항목 (selectable)\n\t\t\t\t\t263 스크롤 막대 ID: 59904\n\t\t\t\t\t\t264 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t265 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t266 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t267 탭 항목 (selectable) Python Script\n\t\t\t268 탭 항목 (selectable) Debugger\n\t\t\t269 탭 항목 (selectable) Output\n\t\t\t270 단추 Close\n\t\t271 창 Splitter ID: 608798568\n\t\t272 Tab File Explorer ID: 409868448\n\t\t\t273 트리 ID: 102\n\t\t\t\t274 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t275 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t276 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t277 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t278 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t279 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t280 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis\n\t\t\t\t\t281 콘솔 트리 (selectable) Additional Files\n\t\t\t\t\t282 콘솔 트리 (selectable) Header Files\n\t\t\t\t\t283 콘솔 트리 (selectable) Source Files\n\t\t\t\t\t284 콘솔 트리 (selectable) Network Files\n\t\t\t\t\t285 콘솔 트리 (selectable) Object Files\n\t\t\t\t\t286 콘솔 트리 (selectable) Library Files\n\t\t\t\t\t287 콘솔 트리 (selectable) Dias Drive Files\n\t\t\t\t\t288 콘솔 트리 (selectable) Connection Files\n\t\t\t289 탭 항목 (selectable) Lib\n\t\t\t290 탭 항목 (selectable) File\n\t\t\t291 탭 항목 (selectable) Global\n\t\t\t292 단추 Close\n\t\t293 Tab Properties ID: 409871640\n\t\t\t294 창 ID: 121918456\n\t\t\t\t295 TABLE Properties Window ID: 127184512\n\t\t\t\t296 도구 모음 ID: 59392\n\t\t\t\t\t297 단추\n\t\t\t\t\t298 단추\n\t\t\t299 탭 항목 (selectable) Properties\n\t\t\t300 단추 Close\n\nThe focused UI element is 273 트리 ID: 102."}
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
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); var topShot=globalThis.state.screenshots.sort((a,b)=>b.zIndex-a.zIndex)[0]; nodeRepl.write(JSON.stringify({id:topShot.id,w:topShot.width,h:topShot.height}));",
>   "title": "Prepare LASAL source context menu",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {"id":"screenshot-1","w":1440,"h":753}
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
>   "code": "await sky.click({window:globalThis.targetWindow,x:1054,y:202,mouse_button:\"right\",screenshotId:\"screenshot-1\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeCtx=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeCtx.split(\"\\n\").filter(x=>/Find|Implementation|Open|Delete|Rename|Remove|Class|menu/i.test(x)).slice(-120).join(\"\\n\")); for (const s of globalThis.state.screenshots.filter(s=>s.zIndex===Math.max(...globalThis.state.screenshots.map(x=>x.zIndex)))) await nodeRepl.emitImage(s.url);",
>   "title": "Inspect LASAL source context menu",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 					19 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_MAX_BULK_SIGNALS 24 FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId VAR_OUTPUT BootId : UDINT; END_VAR VAR nextBootId : UDINT; END_VAR // Retentive servers are restored after class construction. Initialize the // runtime generation lazily on the first diagnostics request so the restored // counter is incremented exactly once for this service instance. if BootIdInitialized = FALSE then nextBootId := DiagnosticsBootCounter.Read(); if nextBootId = 0xFFFFFFFF then BootIdFault := TRUE; DiagnosticsBootId := 0; else nextBootId += 1; if nextBootId = 0 then BootIdFault := TRUE; DiagnosticsBootId := 0; else DiagnosticsBootCounter.Write(input:=nextBootId); if DiagnosticsBootCounter.Read() = nextBootId then DiagnosticsBootId := nextBootId; else BootIdFault := TRUE; DiagnosticsBootId := 0; end_if; end_if; end_if; BootIdInitialized := TRUE; end_if; if BootIdFault then BootId := 0; else BootId := DiagnosticsBootId; end_if; END_FUNCTION FUNCTION LMCDiagnosticsService::BuildCatalogEntry VAR_INPUT pEntry : ^USINT; CatalogIndex : UINT; END_VAR VAR_OUTPUT Result : DINT; END_VAR VAR physicalAxis : UINT; signalCode : UINT; signalId : UDINT; sourceKind : USINT; valueType : USINT; byteWidth : USINT; unitCode : UINT; pdoIndex : UINT; pdoSubIndex : USINT; pdoDirection : USINT; minimumRaw : UDINT; maximumRaw : UDINT; aliasText : ARRAY [0..39] OF CHAR; END_VAR Result := -1; if (pEntry = NIL) | (CatalogIndex >= 24) then RETURN; end_if; physicalAxis := (CatalogIndex / 6) + 1; signalCode := (CatalogIndex mod 6) + 1; signalId := 0x00100000 or (TO_UDINT(physicalAxis) shl 8) or TO_UDINT(signalCode); sourceKind := 5; valueType := 8; byteWidth := 4; unitCode := 0; pdoIndex := 0; pdoSubIndex := 0; pdoDirection := 1; minimumRaw := 0; maximumRaw := 0xFFFFFFFF; _memset(dest:=#aliasText[0], usByte:=0, cntr:=sizeof(aliasText)); _strcpy(dest:=#aliasText[0], src:="axis"); case physicalAxis of 1: aliasText[4] := 49; 2: aliasText[4] := 50; 3: aliasText[4] := 51; 4: aliasText[4] := 52; end_case; aliasText[5] := 46; aliasText[6] := 0; case signalCode of 1: valueType := 4; unitCode := 1; pdoIndex := 0x607A; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="target_position_last_tx"); 2: pdoIndex := 0x60FE; pdoSubIndex := 1; _strcat(dest:=#aliasText[0], src:="digital_outputs_last_tx"); 3: valueType := 7; byteWidth := 2; pdoIndex := 0x6040; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="control_word_last_tx"); 4: sourceKind := 4; valueType := 4; unitCode := 1; pdoIndex := 0x6064; pdoDirection := 2; minimumRaw := 0x80000000; maximumRaw := 0x7FFFFFFF; _strcat(dest:=#aliasText[0], src:="actual_position"); 5: sourceKind := 4; pdoIndex := 0x60FD; pdoDirection := 2; _strcat(dest:=#aliasText[0], src:="digital_inputs"); 6: sourceKind := 4; valueType := 7; byteWidth := 2; pdoIndex := 0x6041; pdoDirection := 2; maximumRaw := 65535; _strcat(dest:=#aliasText[0], src:="status_word"); end_case; _memset(dest:=pEntry, usByte:=0, cntr:=80); pEntry^$UDINT := signalId; (pEntry + 4)^$UINT := CatalogIndex; (pEntry + 6)^$USINT := sourceKind; (pEntry + 7)^$USINT := TO_USINT(physicalAxis); (pEntry + 8)^$USINT := valueType; (pEntry + 9)^$USINT := byteWidth; (pEntry + 10)^$UINT := unitCode; (pEntry + 12)^$UINT := 0x000D; (pEntry + 14)^$UINT := 0x000B; (pEntry + 16)^$UINT := pdoIndex; (pEntry + 18)^$USINT := pdoSubIndex; (pEntry + 19)^$USINT := pdoDirection; (pEntry + 20)^$DINT := 1; (pEntry + 24)^$DINT := 1; (pEntry + 28)^$UDINT := minimum ID: 10000
> 					34 창 #pragma usingLtd SigCLib #pragma usingLtd _StdLib FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR cycleCounter : UDINT; timestampLow : UDINT; timestampHigh : UDINT; previousTimestampLow : UDINT; masterState : UDINT; masterClassState : UDINT; consecutiveInvalidCycles : UDINT; invalidCycleTotal : UDINT; masterFlags : UDINT; writeSequence : UDINT; finalSequence : UDINT; onlineValue : DINT; etherCATStateValue : UDINT; slaveStateValue : UDINT; alStatusValue : UDINT; classStateValue : UDINT; statusWordValue : UDINT; axisErrorValue : DINT; stateChanged : BOOL; recorderResult : DINT; END_VAR // Snapshot layout is fixed and contains only scalar values. The published // byte count is 304; the larger static array leaves room for later fields. writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1; if (writeSequence and 1) = 0 then writeSequence += 1; end_if; sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence); cycleCounter := SnapshotBytes[0]$UDINT + 1; timestampLow := OS_READMICROSEC(); previousTimestampLow := SnapshotBytes[4]$UDINT; timestampHigh := SnapshotBytes[8]$UDINT; if timestampLow < previousTimestampLow then timestampHigh += 1; end_if; masterState := 0; masterClassState := 0xFFFFFFFF; consecutiveInvalidCycles := 0; masterFlags := 2; if IsClientConnected(#EcatMaster) then masterState := TO_UDINT(EcatMaster.EtherCATState.Read()); masterClassState := TO_UDINT(EcatMaster.ClassState.Read()); consecutiveInvalidCycles := EcatMaster.MissedFrameCounter.Read(); SnapshotBytes[24]$UDINT := EcatMaster.FrameTimeTask0.Read(); SnapshotBytes[28]$UDINT := EcatMaster.FrameTimeMaxTask0.Read(); SnapshotBytes[32]$UDINT := EcatMaster.Act_RtTime.Read(); SnapshotBytes[36]$UDINT := EcatMaster.Max_RtTime.Read(); masterFlags := 0; if masterState = 8 then masterFlags := masterFlags or 1; end_if; if consecutiveInvalidCycles <> 0 then masterFlags := masterFlags or 2; end_if; else SnapshotBytes[24]$UDINT := 0; SnapshotBytes[28]$UDINT := 0; SnapshotBytes[32]$UDINT := 0; SnapshotBytes[36]$UDINT := 0; end_if; invalidCycleTotal := SnapshotBytes[20]$UDINT; if ((masterFlags and 2) <> 0) | (consecutiveInvalidCycles <> 0) then invalidCycleTotal += 1; end_if; SnapshotBytes[0]$UDINT := cycleCounter; SnapshotBytes[4]$UDINT := timestampLow; SnapshotBytes[8]$UDINT := timestampHigh; SnapshotBytes[12]$UDINT := masterState; SnapshotBytes[16]$UDINT := consecutiveInvalidCycles; SnapshotBytes[20]$UDINT := invalidCycleTotal; SnapshotBytes[40]$UDINT := masterFlags; SnapshotBytes[48]$UDINT := masterClassState; // Drive 1 health and six active PDO values. onlineValue := 0; etherCATStateValue := 0; slaveStateValue := 0; alStatusValue := 0; classStateValue := 0xFFFFFFFF; statusWordValue := 0; axisErrorValue := 0; if IsClientConnected(#Drive1) then onlineValue := Drive1.Online.Read(); etherCATStateValue := TO_UDINT(Drive1.EtherCATState.Read()); slaveStateValue := Drive1.SlaveState.Read(); alStatusValue := TO_UDINT(Drive1.AL_StatusCode.Read()); classStateValue := TO_UDINT(Drive1.ClassState.Read()); statusWordValue := TO_UDINT(Drive1.StateWord.Read()) and 0x0000FFFF; axisErrorValue := Drive1.AxError.Read(); SnapshotBytes[208]$DINT := Drive1.SetPos.Read(); SnapshotBytes[212]$UDINT := TO_UDINT(Drive1.Outputs_DigitalOutputs.Read()); SnapshotBytes[216]$UDINT := TO_UDINT(Drive1.ControlWord.Read()) and 0x0000FFFF; SnapshotBytes[220]$DINT := Drive1.ActPos.Read(); SnapshotBytes[224]$UDINT := TO_UDINT(Drive1.Inputs_DigitalInputs.Read()); SnapshotBytes[228]$UDINT := statusWordValue; else _memset(dest:=#SnapshotBytes[208], usByte:=0, cntr:=24); end_if; stateChanged := (SnapshotBytes[64]$DINT <> onlineValue) | (SnapshotBytes[68]$UDINT <> etherCATStateValue) | (SnapshotBytes[72]$UDINT <> slaveStateValue) | (SnapshotBytes[76]$UDINT <> alStatusValue) | (SnapshotBytes[80]$UDINT <> classStateValue) | (Snapsho ID: 10000
> 								66 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]-> LMCEcatInputLatch1.EcatMaster
> 								76 콘솔 트리 (selectable) Sdias Class State (ClassState) <-[]->
> 				174 단추 (disabled) Delete all bookmarks
> 				178 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				213 단추 Open a file (Strg+Shift+O)
> 				216 단추 Open project (Strg+O)
> 			227 메뉴 모음 Menu Bar
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:globalThis.targetWindow,key:\"Escape\"}); globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); var shotG=globalThis.state.screenshots.sort((a,b)=>b.zIndex-a.zIndex)[0]; await sky.click({window:globalThis.targetWindow,x:1082,y:533,screenshotId:shotG.id}); nodeRepl.write(\"opened Global view\");",
>   "title": "Open LASAL global tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> opened Global view
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
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); var tabLines=(globalThis.state.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/탭 항목 \\(selectable\\) (Lib|File|Global)$/.test(x)); nodeRepl.write(tabLines.join(\"\\n\"));",
>   "title": "Read LASAL explorer tabs",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 			277 탭 항목 (selectable) Lib
> 			278 탭 항목 (selectable) File
> 			279 탭 항목 (selectable) Global
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:279}); nodeRepl.write(\"selected Global tab\");",
>   "title": "Switch to LASAL global tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> selected Global tab
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var treeGlob=(globalThis.state.accessibility?.tree||\"\"); nodeRepl.write(treeGlob.split(\"\\n\").slice(-100).join(\"\\n\"));",
>   "title": "Inspect LASAL global tree",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> 				222 단추 Paste (Strg+V)
> 				223 메뉴 항목 (disabled) Undo (Strg+Z)
> 				224 메뉴 항목 (disabled) Redo (Strg+Y)
> 				225 단추 Navigate Backward (Alt+Left)
> 				226 단추 (disabled) Navigate Forward (Alt +Right)
> 			227 메뉴 모음 Menu Bar
> 				228 메뉴 항목 FILE
> 				229 메뉴 항목 EDIT
> 				230 메뉴 항목 VIEW
> 				231 메뉴 항목 PROJECT

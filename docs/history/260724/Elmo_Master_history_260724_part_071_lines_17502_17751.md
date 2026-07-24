> 				146 메뉴 항목 FILE
> 				147 메뉴 항목 EDIT
> 						149 메뉴 항목 (disabled) Undo Ctrl+Z
> 						150 메뉴 항목 (disabled) Redo Ctrl+Y
> 						151 메뉴 항목 Cut Ctrl+X
> 						152 메뉴 항목 Copy Ctrl+C
> 						153 메뉴 항목 Paste Ctrl+V
> 						154 메뉴 항목 Find... Ctrl+F
> 						155 메뉴 항목 Find in Files... Ctrl+Shift+F
> 						156 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 						157 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 						158 메뉴 항목 Replace... Ctrl+R
> 						159 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 						160 메뉴 항목 Goto Line... Ctrl+G
> 						161 메뉴 항목 Reset Editor Colors
> 						162 메뉴 항목 Mark
> 						163 메뉴 항목 Bookmark
> 						164 메뉴 항목 Navigate Backward Alt+Left Arrow
> 						165 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 						166 메뉴 항목 Select All Ctrl+A
> 						167 메뉴 항목 Comment
> 						168 메뉴 항목 Dent
> 				169 메뉴 항목 VIEW
> 				170 메뉴 항목 PROJECT
> 				171 메뉴 항목 BUILD
> 				172 메뉴 항목 DEBUG
> 				173 메뉴 항목 ANALYZE
> 				174 메뉴 항목 TOOLS
> 				175 메뉴 항목 EXTRAS
> 				176 메뉴 항목 WINDOW
> 				177 메뉴 항목 HELP
> 			234 메뉴 항목 (disabled) Undo Ctrl+Z
> 			235 메뉴 항목 (disabled) Redo Ctrl+Y
> 			236 메뉴 항목 Cut Ctrl+X
> 			237 메뉴 항목 Copy Ctrl+C
> 			238 메뉴 항목 Paste Ctrl+V
> 			239 메뉴 항목 Find... Ctrl+F
> 			240 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			241 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			242 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			243 메뉴 항목 Replace... Ctrl+R
> 			244 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			245 메뉴 항목 Goto Line... Ctrl+G
> 			246 메뉴 항목 Reset Editor Colors
> 			247 메뉴 항목 Mark
> 			248 메뉴 항목 Bookmark
> 			249 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			250 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			251 메뉴 항목 Select All Ctrl+A
> 			252 메뉴 항목 Comment
> 			253 메뉴 항목 Dent
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
>   "title": "행 단위 Find in Files 열기",
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"});\nawait sky.press_key({window:targetWindow,key:\"Control_L+Shift_L+f\"});\nawait new Promise(r=>setTimeout(r,400));\nglobalThis.findFilesState = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});\n{\n const lines=(findFilesState.accessibility?.tree||\"\").split(\"\\n\");\n nodeRepl.write(lines.slice(-100).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 						179 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 						180 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 						181 메뉴 항목 Replace... Ctrl+R
> 						182 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 						183 메뉴 항목 Goto Line... Ctrl+G
> 						184 메뉴 항목 Reset Editor Colors
> 						185 메뉴 항목 Mark
> 						186 메뉴 항목 Bookmark
> 						187 메뉴 항목 Navigate Backward Alt+Left Arrow
> 						188 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 						189 메뉴 항목 Select All Ctrl+A
> 						190 메뉴 항목 Comment
> 						191 메뉴 항목 Dent
> 				192 메뉴 항목 VIEW
> 				193 메뉴 항목 PROJECT
> 				194 메뉴 항목 BUILD
> 				195 메뉴 항목 DEBUG
> 				196 메뉴 항목 ANALYZE
> 				197 메뉴 항목 TOOLS
> 				198 메뉴 항목 EXTRAS
> 				199 메뉴 항목 WINDOW
> 				200 메뉴 항목 HELP
> 		201 창 Splitter ID: 369458024
> 		202 창 Splitter ID: 369460208
> 		203 Tab Output ID: 895548072
> 			204 창 ID: 1200
> 				205 창 ID: 1200
> 					206 LIST ID: 1204
> 						207 목록 항목 (selectable)
> 						208 목록 항목 (selectable)
> 						209 목록 항목 (selectable)
> 					210 스크롤 막대 (disabled) ID: 59904
> 						211 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						212 위치 조정 위치 ID: ScrollbarThumb
> 						213 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			214 탭 항목 (selectable) Python Script
> 			215 탭 항목 (selectable) Debugger
> 			216 탭 항목 (selectable) Output
> 			217 단추 Close
> 		218 창 Splitter ID: 369455504
> 		219 Tab Class View ID: 895543056
> 			220 트리 ID: 103
> 				221 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					222 콘솔 트리 (selectable) External
> 					223 콘솔 트리 (selectable) Sigmatek
> 					224 콘솔 트리 (selectable) _TCPIPServer_RT
> 					225 콘솔 트리 (selectable) Elmo_1
> 					226 콘솔 트리 (selectable) Elmo_2
> 					227 콘솔 트리 (selectable) Elmo_3
> 					228 콘솔 트리 (selectable) Elmo_4
> 					229 콘솔 트리 (selectable) LMCDiagnosticsService
> 					230 콘솔 트리 (selectable) LMCEcatInputLatch
> 					231 콘솔 트리 (selectable) LMCRecorderStore
> 					232 콘솔 트리 (selectable) LMCSdoExecutor
> 					233 콘솔 트리 (selectable) TCPMotionInterface
> 			234 탭 항목 (selectable) Lib
> 			235 탭 항목 (selectable) File
> 			236 탭 항목 (selectable) Global
> 			237 탭 항목 (selectable) Net
> 			238 탭 항목 (selectable) Class
> 			239 단추 Close
> 		240 Tab Properties ID: 895540320
> 			241 창 ID: 288524952
> 				242 TABLE Properties Window ID: 292730376
> 					243 custom Name
> 					244 custom Class
> 					245 custom DataType
> 					246 custom Type
> 					247 custom Required
> 					248 custom DefaultInitValue
> 					249 custom Disable RedefinitionVariableWarning
> 					250 custom Comment
> 				251 도구 모음 ID: 59392
> 					252 단추
> 					253 단추
> 			254 탭 항목 (selectable) Properties
> 			255 단추 Close
> 		256 메뉴 Edit
> 			257 메뉴 항목 (disabled) Undo Ctrl+Z
> 			258 메뉴 항목 (disabled) Redo Ctrl+Y
> 			259 메뉴 항목 Cut Ctrl+X
> 			260 메뉴 항목 Copy Ctrl+C
> 			261 메뉴 항목 Paste Ctrl+V
> 			262 메뉴 항목 Find... Ctrl+F
> 			263 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			264 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			265 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			266 메뉴 항목 Replace... Ctrl+R
> 			267 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			268 메뉴 항목 Goto Line... Ctrl+G
> 			269 메뉴 항목 Reset Editor Colors
> 			270 메뉴 항목 Mark
> 			271 메뉴 항목 Bookmark
> 			272 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			273 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			274 메뉴 항목 Select All Ctrl+A
> 			275 메뉴 항목 Comment
> 			276 메뉴 항목 Dent
>
> The focused UI element is 27 창 //This file was generated by the LASAL2 CodeGenerator -- //Please, do not edit this file (it might be overwritten by the next generator run) //{{LSL_DECLARATION (*! <Class Name = "TCPMotionInterface" Revision = "0.0" GUID = "{C9B663E2-7D2C-462A-B738-8FDD7B099E2F}" RealtimeTask = "false" CyclicTask = "true" DefCyclictime = "1 ms" BackgroundTask = "false" Sigmatek = "false" OSInterface = "false" HighPriority = "false" Automatic = "false" UpdateMode = "Prescan" SharedCommandTable = "true" Objectsize = "(536,120)"> <Channels> <Server Name="acc" GUID="{EF724BEA-AF9C-43E5-BDC6-0FAD76A9AD08}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="AxisRef" GUID="{99145B0B-4F1C-4C52-9705-9F801FE1A3A1}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="bufMode" GUID="{8B602708-A478-435C-A43F-473F29186A2C}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="CommandID" GUID="{F8B2658C-8914-4808-B041-775825E501E8}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="CurrentSock" GUID="{1F419127-A1E0-44AA-AC32-A2CDCE9841DF}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="dec" GUID="{1A1BE933-EAD9-4A81-81E3-0CCB0EA2985F}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="dir" GUID="{12F08241-03C5-4833-8272-1C429686E36C}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="Exec" GUID="{F862445A-42BB-4FB4-AF61-5CA82DB86CF7}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="jer" GUID="{3AEE5633-5554-427D-92B5-249F061C3F20}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="Payload" GUID="{722817E8-BD3B-4762-B3E4-AD743AEBA249}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="pos" GUID="{F431AAC4-369D-4228-812D-07B6ADE82C30}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="Power" GUID="{130E4A27-675A-41AC-AB89-829ABE2D8CBA}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="ReadPos" GUID="{44E93FA6-636B-4EA8-A3A0-BDCB89CF8B79}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="Reserved" GUID="{F20FEA26-1B51-4AB0-81CB-3B30D508F31F}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="RetCode" GUID="{C427A968-CED5-49C0-BFDE-AC2143C688EC}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Server Name="RobotLock" GUID="{C73AB6B8-68DE-4192-ABA4-2D32F5D8D566}" Visualized="false" Initialize="false" WriteProtected="false" Retentive="false"/> <Server Name="RobotPowerOff" GUID="{45163924-14EF-4B60-9C3B-364662E53787}" Visualized="false" Initialize="false" WriteProtected="false" Retentive="false"/> <Server Name="RobotPowerOn" GUID="{DE939C5A-E281-44F8-9661-45FB4988066B}" Visualized="false" Initialize="false" WriteProtected="false" Retentive="false"/> <Server Name="RobotUnLock" GUID="{ADED8B18-F205-4D23-9D6E-A966DCA5605E}" Visualized="false" Initialize="false" WriteProtected="false" Retentive="false"/> <Server Name="velo" GUID="{62BB909C-5656-4232-B799-BE918E89FDFD}" Visualized="false" Initialize="false" WriteProtected="true" Retentive="false"/> <Client Name="_StdLib" Required="false" Internal="false"/> <Client Name="Diagnostics" Required="true" Internal="false"/> <Client Name="LMCAxis1" Required="true" Internal="false"/> <Client Name="LMCAxis2" Required="true" Internal="false"/> <Client Name="LMCAxis3" Required="true" Internal="false"/> <Client Name="LMCAxis4" Required="true" Internal="false"/> <Client Name="LMCAxis5" Required="true" Internal=" ID: 10000.
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
>   "title": "Find in Files 대화상자 확인",
>   "code": "await sky.click({window:targetWindow,x:145,y:204});\nawait new Promise(r=>setTimeout(r,400));\nglobalThis.findFilesDialog = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});\n{\n const lines=(findFilesDialog.accessibility?.tree||\"\").split(\"\\n\");\n nodeRepl.write(lines.slice(-120).join(\"\\n\"));\n}"
> }
> ```
>
> ```text
> 				149 단추 Rebuild project (Strg+F9)
> 				150 단추 (disabled) Cancel building (Ctrl+Break)
> 				151 단추 Link project
> 			152 도구 모음 Standard
> 				153 단추 New project (Strg+N)
> 				154 단추 Open a file (Strg+Shift+O)
> 				155 단추 Close active document (Strg+F4)
> 				156 단추 (disabled) Save file (Strg+S)
> 				157 단추 Open project (Strg+O)
> 				158 단추 (disabled) Save project changes (Strg+Shift+S)
> 				159 단추 Close project
> 				160 단추 Print
> 				161 단추 Cut (Strg+X)
> 				162 단추 Copy (Strg+C)
> 				163 단추 Paste (Strg+V)
> 				164 메뉴 항목 (disabled) Undo (Strg+Z)
> 				165 메뉴 항목 (disabled) Redo (Strg+Y)
> 				166 단추 Navigate Backward (Alt+Left)
> 				167 단추 (disabled) Navigate Forward (Alt +Right)
> 			168 메뉴 모음 Menu Bar
> 				169 메뉴 항목 FILE
> 				170 메뉴 항목 EDIT
> 				171 메뉴 항목 VIEW
> 				172 메뉴 항목 PROJECT
> 				173 메뉴 항목 BUILD
> 				174 메뉴 항목 DEBUG
> 				175 메뉴 항목 ANALYZE
> 				176 메뉴 항목 TOOLS
> 				177 메뉴 항목 EXTRAS
> 				178 메뉴 항목 WINDOW
> 				179 메뉴 항목 HELP
> 		180 창 Splitter ID: 369458024
> 		181 창 Splitter ID: 369460208
> 		182 Tab Output ID: 895548072
> 			183 창 ID: 1200
> 				184 창 ID: 1200
> 					185 LIST ID: 1204
> 						186 목록 항목 (selectable)
> 						187 목록 항목 (selectable)
> 						188 목록 항목 (selectable)
> 					189 스크롤 막대 (disabled) ID: 59904
> 						190 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						191 위치 조정 위치 ID: ScrollbarThumb
> 						192 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			193 탭 항목 (selectable) Python Script
> 			194 탭 항목 (selectable) Debugger
> 			195 탭 항목 (selectable) Output
> 			196 단추 Close
> 		197 창 Splitter ID: 369455504
> 		198 Tab Class View ID: 895543056
> 			199 트리 ID: 103

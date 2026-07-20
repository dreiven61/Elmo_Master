> 					270 custom Type
> 					271 custom Initialize
> 					272 custom WriteProtected
> 					273 custom Retentive
> 					274 custom Comment
> 				275 도구 모음 ID: 59392
> 					276 단추
> 					277 단추
> 			278 탭 항목 (selectable) Properties
> 			279 단추 Close
>
> The focused UI element is 178 트리 ID: 103.
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
>   "code": "await sky.click({window:targetWindow,x:1107,y:461}); globalThis.standardMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((standardMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-30).join(\"\\n\"))",
>   "title": "표준 메서드 메뉴 펼치기"
> }
> ```
>
> ```text
> 			265 단추 Close
> 		266 Tab Properties ID: 305711584
> 			267 창 ID: 302137496
> 				268 TABLE Properties Window ID: 307037824
> 					269 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						270 단추 위쪽 스크롤 화살표 ID: UpButton
> 						271 위치 조정 위치 ID: ScrollbarThumb
> 						272 단추 페이지 아래로 ID: DownPageButton
> 						273 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					274 custom Name
> 					275 custom GUID
> 					276 custom Visualized
> 					277 custom World
> 					278 custom Alarm
> 					279 custom DataType
> 					280 custom Type
> 					281 custom Initialize
> 					282 custom WriteProtected
> 					283 custom Retentive
> 					284 custom Comment
> 				285 도구 모음 ID: 59392
> 					286 단추
> 					287 단추
> 			288 탭 항목 (selectable) Properties
> 			289 단추 Close
> 		290 메뉴 Standard Methods
> 			291 메뉴 항목 Read
> 			292 메뉴 항목 Write
>
> The focused UI element is 188 트리 ID: 103.
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
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,element_index:207,click_count:2}); globalThis.powerOpened=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({focused:powerOpened.accessibility?.focused_element,selected:powerOpened.accessibility?.selected_text,tree:(powerOpened.accessibility?.tree||\"\").split(\"\\n\").slice(0,35).join(\"\\n\")})",
>   "title": "Power 선언 열기"
> }
> ```
>
> ```text
> element 207 no longer matches the cached target in Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.postMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((postMenu.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Power|pos|velo/.test(x)).join(\"\\n\"))",
>   "title": "Power 선언 항목 재확인"
> }
> ```
>
> ```text
> 							207 콘솔 트리 (selectable) Power
> 							208 콘솔 트리 (selectable) pos
> 							209 콘솔 트리 (selectable) velo
> 							218 콘솔 트리 (selectable) RobotPowerOn
> 							219 콘솔 트리 (selectable) RobotPowerOff
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
>   "code": "await sky.click({window:targetWindow,element_index:207,click_count:2}); globalThis.powerOpened2=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({focused:powerOpened2.accessibility?.focused_element, selected:powerOpened2.accessibility?.selected_text})",
>   "title": "Power 선언 열기"
> }
> ```
>
> ```text
> { focused: '178 트리 ID: 103', selected: undefined }
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
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:352,y:40}); globalThis.analyzeMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((analyzeMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-70).join(\"\\n\"))",
>   "title": "LASAL Analyze 메뉴 확인"
> }
> ```
>
> ```text
> 							214 콘솔 트리 (selectable) bufMode
> 							215 콘솔 트리 (selectable) Exec
> 							216 콘솔 트리 (selectable) Reserved
> 							217 콘솔 트리 (selectable) ReadPos
> 							218 콘솔 트리 (selectable) RobotPowerOn
> 							219 콘솔 트리 (selectable) RobotPowerOff
> 							220 콘솔 트리 (selectable) RobotLock
> 							221 콘솔 트리 (selectable) RobotUnLock
> 							222 콘솔 트리 (selectable) RetCode
> 						223 콘솔 트리 (selectable) Clients
> 							224 콘솔 트리 (selectable) LMCAxis1
> 								225 콘솔 트리 (selectable) _LMCAXIS_OPMODE
> 									226 콘솔 트리 (selectable) PASSIVE_LMCAXIS
> 									227 콘솔 트리 (selectable) WAITIDLE_LMCAXIS
> 									228 콘솔 트리 (selectable) IDLE_LMCAXIS
> 									229 콘솔 트리 (selectable) MOVE_LMCAXIS
> 									230 콘솔 트리 (selectable) FOLLOW_LMCAXIS
> 									231 콘솔 트리 (selectable) STOPED_LMCAXIS
> 									232 콘솔 트리 (selectable) LOCKED_LMCAXIS
> 									233 콘솔 트리 (selectable) REFRUN_LMCAXIS
> 									234 콘솔 트리 (selectable) TUNEPOS_LMCAXIS
> 									235 콘솔 트리 (selectable) TUNENEG_LMCAXIS
> 									236 콘솔 트리 (selectable) ERROR_LMCAXIS
> 									237 콘솔 트리 (selectable) WAITPASSIVE_LMCAXIS
> 									238 콘솔 트리 (selectable) SBT_LMCAXIS
> 							239 콘솔 트리 (selectable) LMCAxis2
> 							240 콘솔 트리 (selectable) LMCAxis3
> 							241 콘솔 트리 (selectable) LMCAxis4
> 							242 콘솔 트리 (selectable) LMCRobot
> 							243 콘솔 트리 (selectable) _StdLib
> 						244 콘솔 트리 (selectable) Methods
> 						245 콘솔 트리 (selectable) Variables
> 						246 콘솔 트리 (selectable) Types
> 						247 콘솔 트리 (selectable) Network
> 						248 콘솔 트리 (selectable) Objects
> 						249 콘솔 트리 (selectable) Dependencies
> 			250 탭 항목 (selectable) Lib
> 			251 탭 항목 (selectable) File
> 			252 탭 항목 (selectable) Global
> 			253 탭 항목 (selectable) Net
> 			254 탭 항목 (selectable) Class
> 			255 단추 Close
> 		256 Tab Properties ID: 305711584
> 			257 창 ID: 302137496
> 				258 TABLE Properties Window ID: 307037824
> 					259 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						260 단추 위쪽 스크롤 화살표 ID: UpButton
> 						261 위치 조정 위치 ID: ScrollbarThumb
> 						262 단추 페이지 아래로 ID: DownPageButton
> 						263 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					264 custom Name
> 					265 custom GUID
> 					266 custom Visualized
> 					267 custom World
> 					268 custom Alarm
> 					269 custom DataType
> 					270 custom Type
> 					271 custom Initialize
> 					272 custom WriteProtected
> 					273 custom Retentive
> 					274 custom Comment
> 				275 도구 모음 ID: 59392
> 					276 단추
> 					277 단추
> 			278 탭 항목 (selectable) Properties
> 			279 단추 Close
> 		280 메뉴 Analyze
> 			281 메뉴 항목 Static Code Analysis
>
> The focused UI element is 178 트리 ID: 103.
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
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.click({window:targetWindow,x:86,y:40}); globalThis.editMenu=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((editMenu.accessibility?.tree||\"\").split(\"\\n\").slice(-80).join(\"\\n\"))",
>   "title": "LASAL Edit 메뉴 확인"
> }
> ```
>
> ```text
> 						229 콘솔 트리 (selectable) Clients
> 							230 콘솔 트리 (selectable) LMCAxis1
> 								231 콘솔 트리 (selectable) _LMCAXIS_OPMODE

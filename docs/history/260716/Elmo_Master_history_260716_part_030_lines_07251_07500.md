> 									232 콘솔 트리 (selectable) PASSIVE_LMCAXIS
> 									233 콘솔 트리 (selectable) WAITIDLE_LMCAXIS
> 									234 콘솔 트리 (selectable) IDLE_LMCAXIS
> 									235 콘솔 트리 (selectable) MOVE_LMCAXIS
> 									236 콘솔 트리 (selectable) FOLLOW_LMCAXIS
> 									237 콘솔 트리 (selectable) STOPED_LMCAXIS
> 									238 콘솔 트리 (selectable) LOCKED_LMCAXIS
> 									239 콘솔 트리 (selectable) REFRUN_LMCAXIS
> 									240 콘솔 트리 (selectable) TUNEPOS_LMCAXIS
> 									241 콘솔 트리 (selectable) TUNENEG_LMCAXIS
> 									242 콘솔 트리 (selectable) ERROR_LMCAXIS
> 									243 콘솔 트리 (selectable) WAITPASSIVE_LMCAXIS
> 									244 콘솔 트리 (selectable) SBT_LMCAXIS
> 							245 콘솔 트리 (selectable) LMCAxis2
> 							246 콘솔 트리 (selectable) LMCAxis3
> 							247 콘솔 트리 (selectable) LMCAxis4
> 							248 콘솔 트리 (selectable) LMCRobot
> 							249 콘솔 트리 (selectable) _StdLib
> 						250 콘솔 트리 (selectable) Methods
> 						251 콘솔 트리 (selectable) Variables
> 						252 콘솔 트리 (selectable) Types
> 						253 콘솔 트리 (selectable) Network
> 						254 콘솔 트리 (selectable) Objects
> 						255 콘솔 트리 (selectable) Dependencies
> 			256 탭 항목 (selectable) Lib
> 			257 탭 항목 (selectable) File
> 			258 탭 항목 (selectable) Global
> 			259 탭 항목 (selectable) Net
> 			260 탭 항목 (selectable) Class
> 			261 단추 Close
> 		262 Tab Properties ID: 305711584
> 			263 창 ID: 302137496
> 				264 TABLE Properties Window ID: 307037824
> 					265 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						266 단추 위쪽 스크롤 화살표 ID: UpButton
> 						267 위치 조정 위치 ID: ScrollbarThumb
> 						268 단추 페이지 아래로 ID: DownPageButton
> 						269 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					270 custom Name
> 					271 custom GUID
> 					272 custom Visualized
> 					273 custom World
> 					274 custom Alarm
> 					275 custom DataType
> 					276 custom Type
> 					277 custom Initialize
> 					278 custom WriteProtected
> 					279 custom Retentive
> 					280 custom Comment
> 				281 도구 모음 ID: 59392
> 					282 단추
> 					283 단추
> 			284 탭 항목 (selectable) Properties
> 			285 단추 Close
> 		286 메뉴 Edit
> 			287 메뉴 항목 (disabled) Undo Ctrl+Z
> 			288 메뉴 항목 (disabled) Redo Ctrl+Y
> 			289 메뉴 항목 Cut Ctrl+X
> 			290 메뉴 항목 Copy Ctrl+C
> 			291 메뉴 항목 Paste Ctrl+V
> 			292 메뉴 항목 Find... Ctrl+F
> 			293 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			294 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			295 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			296 메뉴 항목 Replace... Ctrl+R
> 			297 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			298 메뉴 항목 Goto Line... Ctrl+G
> 			299 메뉴 항목 Reset Editor Colors
> 			300 메뉴 항목 Mark
> 			301 메뉴 항목 Bookmark
> 			302 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			303 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			304 메뉴 항목 Select All Ctrl+A
> 			305 메뉴 항목 Comment
> 			306 메뉴 항목 Dent
>
> The focused UI element is 123 메뉴 항목 ANALYZE.
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
>   "code": "await sky.press_key({window:targetWindow,key:\"Escape\"}); await sky.press_key({window:targetWindow,key:\"Control_L+f\"}); await sky.type_text({window:targetWindow,text:\"Power\"}); await sky.press_key({window:targetWindow,key:\"Return\"}); await sky.press_key({window:targetWindow,key:\"Escape\"}); globalThis.powerFind=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write({focused:powerFind.accessibility?.focused_element,selected:powerFind.accessibility?.selected_text})",
>   "title": "Power 구현 위치 찾기"
> }
> ```
>
> ```text
> { focused: '137 메뉴 항목 EDIT', selected: undefined }
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
>   "code": "await sky.click({window:targetWindow,x:400,y:300}); await sky.press_key({window:targetWindow,key:\"Control_L+f\"}); globalThis.findBox=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((findBox.accessibility?.tree||\"\").split(\"\\n\").slice(-40).join(\"\\n\"))",
>   "title": "Power 편집기 검색 열기"
> }
> ```
>
> ```text
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
> 		280 대화 상자 Find Secondary Actions: Raise
> 			281 텍스트 Find What:
> 			282 콤보 상자 Find What: ID: 1115
> 				283 편집 Find What: ID: 1001
> 				284 단추 열기 ID: DropDown
> 			285 확인란 Match Case ID: 1116
> 			286 확인란 Whole Word ID: 1117
> 			287 확인란 (disabled) Include Properties ID: 1776
> 			288 확인란 Show in Find Results ID: 1430
> 			289 확인란 Show in Find Results 2 ID: 1431
> 			290 확인란 Ignore Whitespaces ID: 1423
> 			291 확인란 Ignore Return ID: 1424
> 			292 확인란 Ignore Comments ID: 1799
> 			293 확인란 Use regular expression ID: 1774
> 			294 그룹 Direction:
> 			295 라디오 단추 (selectable) Up ID: 1118
> 			296 라디오 단추 (selectable) Down ID: 1119
> 			297 단추 Find ID: 1120
> 			298 단추 Mark All ID: 1729
> 			299 단추 Clear all marks ID: 1730
> 			300 단추 Close ID: 1
> 			301 단추 Cancel ID: 2
> 			302 제목 표시줄
> 				303 메뉴 모음 시스템 ID: MenuBar
> 					304 메뉴 항목 시스템
> 				305 단추 닫기
>
> The focused UI element is 114 메뉴 항목 EDIT.
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
>   "code": "await sky.click({window:targetWindow,element_index:297}); await new Promise(r=>setTimeout(r,900)); globalThis.powerSearchResult=await sky.get_window_state({window:targetWindow,include_text:true}); nodeRepl.write((powerSearchResult.accessibility?.tree||\"\").split(\"\\n\").filter(x=>/Find Results|TCPMotionInterface\\.st|Power/.test(x)).slice(-60).join(\"\\n\"))",
>   "title": "Power 구현 검색 실행"
> }
> ```
>
> ```text
> element 297 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(findBox.window)",
>   "title": "검색 대화상자 대상 확인"
> }
> ```
>
> ```text
> {
>   app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 20778988,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(findBox.screenshots.map(({id,zIndex,originX,originY,width,height})=>({id,zIndex,originX,originY,width,height})))",
>   "title": "검색 대화상자 위치 확인"
> }
> ```
>
> ```text
> [
>   {
>     id: 'screenshot-0',
>     zIndex: 0,
>     originX: -1880,
>     originY: 41,
>     width: 1450,
>     height: 763
>   },
>   {
>     id: 'screenshot-1',
>     zIndex: 1,
>     originX: -1875,
>     originY: 46,
>     width: 1440,
>     height: 753
>   },
>   {
>     id: 'screenshot-2',
>     zIndex: 2,
>     originX: 663,
>     originY: 474,
>     width: 504,
>     height: 220
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.findWindows=await sky.list_windows(); nodeRepl.write(findWindows.filter(w=>/Find|Lasal/i.test(w.title||\"\")))",
>   "title": "검색 대화상자 연결"

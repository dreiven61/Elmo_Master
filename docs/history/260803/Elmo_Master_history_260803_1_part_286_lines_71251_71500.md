> 						280 콘솔 트리 (selectable) Methods
> 						281 콘솔 트리 (selectable) Variables
> 						282 콘솔 트리 (selectable) Objects
> 						283 콘솔 트리 (selectable) Dependencies
> 					284 콘솔 트리 (selectable) LMCRecorderStore
> 					285 콘솔 트리 (selectable) LMCSdoExecutor
> 					286 콘솔 트리 (selectable) TCPIPServer
> 					287 콘솔 트리 (selectable) TCPMotionInterface
> 						288 콘솔 트리 (selectable) Base
> 						289 콘솔 트리 (selectable) Servers
> 						290 콘솔 트리 (selectable) Clients
> 							291 콘솔 트리 (selectable) _StdLib
> 							292 콘솔 트리 (selectable) Diagnostics
> 							293 콘솔 트리 (selectable) ControlCommands
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Variables
> 						296 콘솔 트리 (selectable) Types
> 						297 콘솔 트리 (selectable) Network
> 						298 콘솔 트리 (selectable) Objects
> 						299 콘솔 트리 (selectable) Dependencies
> 			33676 탭 항목 (selectable) Lib
> 			33677 탭 항목 (selectable) File
> 			33678 탭 항목 (selectable) Class
> 			33679 탭 항목 (selectable) Global
> 			33680 탭 항목 (selectable) Net
> 			305 단추 Close
> 		306 Tab Properties ID: 279798808
> 			307 창 ID: 270081768
> 				308 TABLE Properties Window ID: 277338752
> 					32630 편집 ID: 1092566080
> 					33681 custom Name
> 					33682 custom DataType
> 					33683 custom Type
> 					33684 custom Required
> 					33685 custom DefaultInitValue
> 					33686 custom Comment
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			33687 탭 항목 (selectable) Properties
> 			321 단추 Close
>
> The focused UI element is 32630 편집 ID: 1092566080.
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Object Channel 유형 선택 준비",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1428,y:518}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(-1800)); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> 71 콘솔 트리 (selectable) EcatMaster
> 							272 콘솔 트리 (selectable) Drive1
> 							273 콘솔 트리 (selectable) Drive2
> 							274 콘솔 트리 (selectable) Drive3
> 							275 콘솔 트리 (selectable) Drive4
> 							276 콘솔 트리 (selectable) RecorderStore
> 							277 콘솔 트리 (selectable) Coupler
> 							278 콘솔 트리 (selectable) InputSlot
> 							279 콘솔 트리 (selectable) OutputSlot
> 							31799 콘솔 트리 (selectable) LMCAxis1
> 						280 콘솔 트리 (selectable) Methods
> 						281 콘솔 트리 (selectable) Variables
> 						282 콘솔 트리 (selectable) Objects
> 						283 콘솔 트리 (selectable) Dependencies
> 					284 콘솔 트리 (selectable) LMCRecorderStore
> 					285 콘솔 트리 (selectable) LMCSdoExecutor
> 					286 콘솔 트리 (selectable) TCPIPServer
> 					287 콘솔 트리 (selectable) TCPMotionInterface
> 						288 콘솔 트리 (selectable) Base
> 						289 콘솔 트리 (selectable) Servers
> 						290 콘솔 트리 (selectable) Clients
> 							291 콘솔 트리 (selectable) _StdLib
> 							292 콘솔 트리 (selectable) Diagnostics
> 							293 콘솔 트리 (selectable) ControlCommands
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Variables
> 						296 콘솔 트리 (selectable) Types
> 						297 콘솔 트리 (selectable) Network
> 						298 콘솔 트리 (selectable) Objects
> 						299 콘솔 트리 (selectable) Dependencies
> 			34126 탭 항목 (selectable) Lib
> 			34127 탭 항목 (selectable) File
> 			34128 탭 항목 (selectable) Class
> 			34129 탭 항목 (selectable) Global
> 			34130 탭 항목 (selectable) Net
> 			305 단추 Close
> 		306 Tab Properties ID: 279798808
> 			307 창 ID: 270081768
> 				308 TABLE Properties Window ID: 277338752
> 					32630 편집 ID: 1092566080
> 					34131 custom Name
> 					34132 custom DataType
> 					34133 custom Type
> 					34134 custom Required
> 					34135 custom DefaultInitValue
> 					34136 custom Comment
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			34137 탭 항목 (selectable) Properties
> 			321 단추 Close
>
> The focused UI element is 32630 편집 ID: 1092566080.
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Object Channel 유형 적용",
>   "code": "await globalThis.sky.set_value({window:globalThis.targetWindow,element_index:32630,value:\"Object Channel\"}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility?.focused_element,tree:(globalThis.state.accessibility?.tree||\"\").slice(-1800)}); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> read UIA value read-only state: 요청한 속성이 CacheRequest에 없습니다. (0x80070057)
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Object Channel 유형으로 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Down\"}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write(globalThis.state.accessibility?.focused_element||\"\"); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> 32630 편집 ID: 1092566080
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Object Channel 유형으로 이동",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Down\"}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Object Channel 유형 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Return\"}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(-2200)); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> roupKinematicReady
> 							263 콘솔 트리 (selectable) ReferenceState
> 								264 콘솔 트리 (selectable) 0..18
> 						265 콘솔 트리 (selectable) Objects
> 						266 콘솔 트리 (selectable) Dependencies
> 					267 콘솔 트리 (selectable) LMCDiagnosticsService
> 					268 콘솔 트리 (selectable) LMCEcatInputLatch
> 						269 콘솔 트리 (selectable) Servers
> 						270 콘솔 트리 (selectable) Clients
> 							271 콘솔 트리 (selectable) EcatMaster
> 							272 콘솔 트리 (selectable) Drive1
> 							273 콘솔 트리 (selectable) Drive2
> 							274 콘솔 트리 (selectable) Drive3
> 							275 콘솔 트리 (selectable) Drive4
> 							276 콘솔 트리 (selectable) RecorderStore
> 							277 콘솔 트리 (selectable) Coupler
> 							278 콘솔 트리 (selectable) InputSlot
> 							279 콘솔 트리 (selectable) OutputSlot
> 							31799 콘솔 트리 (selectable) LMCAxis1
> 						280 콘솔 트리 (selectable) Methods
> 						281 콘솔 트리 (selectable) Variables
> 						282 콘솔 트리 (selectable) Objects
> 						283 콘솔 트리 (selectable) Dependencies
> 					284 콘솔 트리 (selectable) LMCRecorderStore
> 					285 콘솔 트리 (selectable) LMCSdoExecutor
> 					286 콘솔 트리 (selectable) TCPIPServer
> 					287 콘솔 트리 (selectable) TCPMotionInterface
> 						288 콘솔 트리 (selectable) Base
> 						289 콘솔 트리 (selectable) Servers
> 						290 콘솔 트리 (selectable) Clients
> 							291 콘솔 트리 (selectable) _StdLib
> 							292 콘솔 트리 (selectable) Diagnostics
> 							293 콘솔 트리 (selectable) ControlCommands
> 						294 콘솔 트리 (selectable) Methods
> 						295 콘솔 트리 (selectable) Variables
> 						296 콘솔 트리 (selectable) Types
> 						297 콘솔 트리 (selectable) Network
> 						298 콘솔 트리 (selectable) Objects
> 						299 콘솔 트리 (selectable) Dependencies
> 			36976 탭 항목 (selectable) Lib
> 			36977 탭 항목 (selectable) File
> 			36978 탭 항목 (selectable) Class
> 			36979 탭 항목 (selectable) Global
> 			36980 탭 항목 (selectable) Net
> 			305 단추 Close
> 		306 Tab Properties ID: 279798808
> 			307 창 ID: 270081768
> 				308 TABLE Properties Window ID: 277338752
> 					36981 편집 ID: 1092565656
> 					36982 custom Name
> 					36983 custom Class
> 					36984 custom DataType
> 					36985 custom Type

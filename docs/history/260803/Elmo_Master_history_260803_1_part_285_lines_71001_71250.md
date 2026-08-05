>     '\t\t\t\t\t31810 custom Comment\n' +
>     '\t\t\t\t317 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t318 단추\n' +
>     '\t\t\t\t\t319 단추\n' +
>     '\t\t\t31811 탭 항목 (selectable) Properties\n' +
>     '\t\t\t321 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 235 트리 ID: 103.'
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
>   "title": "클라이언트 유형 편집",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1363,y:517}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility?.focused_element,tree:(globalThis.state.accessibility?.tree||\"\").slice(-2500)}); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> {
>   focused: '32630 편집 ID: 1092566080',
>   tree: 'selectable) Clients\n' +
>     '\t\t\t\t\t\t255 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t\t256 콘솔 트리 (selectable) Global\n' +
>     '\t\t\t\t\t\t\t\t257 콘솔 트리 (selectable) HandleRequest\n' +
>     '\t\t\t\t\t\t\t\t258 콘솔 트리 (selectable) ProcessAxisReference\n' +
>     '\t\t\t\t\t\t\t259 콘솔 트리 (selectable) Private\n' +
>     '\t\t\t\t\t\t260 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t\t261 콘솔 트리 (selectable) GroupMovePos\n' +
>     '\t\t\t\t\t\t\t262 콘솔 트리 (selectable) GroupKinematicReady\n' +
>     '\t\t\t\t\t\t\t263 콘솔 트리 (selectable) ReferenceState\n' +
>     '\t\t\t\t\t\t\t\t264 콘솔 트리 (selectable) 0..18\n' +
>     '\t\t\t\t\t\t265 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t266 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t267 콘솔 트리 (selectable) LMCDiagnosticsService\n' +
>     '\t\t\t\t\t268 콘솔 트리 (selectable) LMCEcatInputLatch\n' +
>     '\t\t\t\t\t\t269 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t270 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t\t271 콘솔 트리 (selectable) EcatMaster\n' +
>     '\t\t\t\t\t\t\t272 콘솔 트리 (selectable) Drive1\n' +
>     '\t\t\t\t\t\t\t273 콘솔 트리 (selectable) Drive2\n' +
>     '\t\t\t\t\t\t\t274 콘솔 트리 (selectable) Drive3\n' +
>     '\t\t\t\t\t\t\t275 콘솔 트리 (selectable) Drive4\n' +
>     '\t\t\t\t\t\t\t276 콘솔 트리 (selectable) RecorderStore\n' +
>     '\t\t\t\t\t\t\t277 콘솔 트리 (selectable) Coupler\n' +
>     '\t\t\t\t\t\t\t278 콘솔 트리 (selectable) InputSlot\n' +
>     '\t\t\t\t\t\t\t279 콘솔 트리 (selectable) OutputSlot\n' +
>     '\t\t\t\t\t\t\t31799 콘솔 트리 (selectable) LMCAxis1\n' +
>     '\t\t\t\t\t\t280 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t281 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t282 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t283 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t\t\t284 콘솔 트리 (selectable) LMCRecorderStore\n' +
>     '\t\t\t\t\t285 콘솔 트리 (selectable) LMCSdoExecutor\n' +
>     '\t\t\t\t\t286 콘솔 트리 (selectable) TCPIPServer\n' +
>     '\t\t\t\t\t287 콘솔 트리 (selectable) TCPMotionInterface\n' +
>     '\t\t\t\t\t\t288 콘솔 트리 (selectable) Base\n' +
>     '\t\t\t\t\t\t289 콘솔 트리 (selectable) Servers\n' +
>     '\t\t\t\t\t\t290 콘솔 트리 (selectable) Clients\n' +
>     '\t\t\t\t\t\t\t291 콘솔 트리 (selectable) _StdLib\n' +
>     '\t\t\t\t\t\t\t292 콘솔 트리 (selectable) Diagnostics\n' +
>     '\t\t\t\t\t\t\t293 콘솔 트리 (selectable) ControlCommands\n' +
>     '\t\t\t\t\t\t294 콘솔 트리 (selectable) Methods\n' +
>     '\t\t\t\t\t\t295 콘솔 트리 (selectable) Variables\n' +
>     '\t\t\t\t\t\t296 콘솔 트리 (selectable) Types\n' +
>     '\t\t\t\t\t\t297 콘솔 트리 (selectable) Network\n' +
>     '\t\t\t\t\t\t298 콘솔 트리 (selectable) Objects\n' +
>     '\t\t\t\t\t\t299 콘솔 트리 (selectable) Dependencies\n' +
>     '\t\t\t32625 탭 항목 (selectable) Lib\n' +
>     '\t\t\t32626 탭 항목 (selectable) File\n' +
>     '\t\t\t32627 탭 항목 (selectable) Class\n' +
>     '\t\t\t32628 탭 항목 (selectable) Global\n' +
>     '\t\t\t32629 탭 항목 (selectable) Net\n' +
>     '\t\t\t305 단추 Close\n' +
>     '\t\t306 Tab Properties ID: 279798808\n' +
>     '\t\t\t307 창 ID: 270081768\n' +
>     '\t\t\t\t308 TABLE Properties Window ID: 277338752\n' +
>     '\t\t\t\t\t32630 편집 ID: 1092566080\n' +
>     '\t\t\t\t\t32631 custom Name\n' +
>     '\t\t\t\t\t32632 custom DataType\n' +
>     '\t\t\t\t\t32633 custom Type\n' +
>     '\t\t\t\t\t32634 custom Required\n' +
>     '\t\t\t\t\t32635 custom DefaultInitValue\n' +
>     '\t\t\t\t\t32636 custom Comment\n' +
>     '\t\t\t\t317 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t318 단추\n' +
>     '\t\t\t\t\t319 단추\n' +
>     '\t\t\t32637 탭 항목 (selectable) Properties\n' +
>     '\t\t\t321 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 32630 편집 ID: 1092566080.'
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
>   "title": "클라이언트 유형 목록 열기",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Alt_L+Down\"}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(-3000)); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> 			242 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					243 콘솔 트리 (selectable) External
> 					244 콘솔 트리 (selectable) Sigmatek
> 					245 콘솔 트리 (selectable) Elmo_1
> 					246 콘솔 트리 (selectable) Elmo_2
> 					247 콘솔 트리 (selectable) Elmo_3
> 					248 콘솔 트리 (selectable) Elmo_4
> 					249 콘솔 트리 (selectable) GL_9086_1
> 					250 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					251 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					252 콘솔 트리 (selectable) LMCControlCommandService
> 						253 콘솔 트리 (selectable) Servers
> 						254 콘솔 트리 (selectable) Clients
> 						255 콘솔 트리 (selectable) Methods
> 							256 콘솔 트리 (selectable) Global
> 								257 콘솔 트리 (selectable) HandleRequest
> 								258 콘솔 트리 (selectable) ProcessAxisReference
> 							259 콘솔 트리 (selectable) Private
> 						260 콘솔 트리 (selectable) Variables
> 							261 콘솔 트리 (selectable) GroupMovePos
> 							262 콘솔 트리 (selectable) GroupKinematicReady
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
> 			33076 탭 항목 (selectable) Lib
> 			33077 탭 항목 (selectable) File
> 			33078 탭 항목 (selectable) Class
> 			33079 탭 항목 (selectable) Global
> 			33080 탭 항목 (selectable) Net
> 			305 단추 Close
> 		306 Tab Properties ID: 279798808
> 			307 창 ID: 270081768
> 				308 TABLE Properties Window ID: 277338752
> 					32630 편집 ID: 1092566080
> 					33081 custom Name
> 					33082 custom DataType
> 					33083 custom Type
> 					33084 custom Required
> 					33085 custom DefaultInitValue
> 					33086 custom Comment
> 				317 도구 모음 ID: 59392
> 					318 단추
> 					319 단추
> 			33087 탭 항목 (selectable) Properties
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
>   "title": "클라이언트 유형 드롭다운 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1431,y:518}); globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true}); nodeRepl.write((globalThis.state.accessibility?.tree||\"\").slice(-2200)); for(const s of globalThis.state.screenshots||[]) await nodeRepl.emitImage(s.url)"
> }
> ```
>
> ```text
> oupMovePos
> 							262 콘솔 트리 (selectable) GroupKinematicReady
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

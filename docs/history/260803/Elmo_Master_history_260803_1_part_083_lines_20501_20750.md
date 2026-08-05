> 							44367 콘솔 트리 (selectable) OutputRevision
> 							61212 콘솔 트리 (selectable) OutputObserved
> 							63856 콘솔 트리 (selectable) OutputPreviousValid
> 							66427 콘솔 트리 (selectable) OutputPreviousValue
> 						17900 콘솔 트리 (selectable) Objects
> 						17901 콘솔 트리 (selectable) Dependencies
> 					162 콘솔 트리 (selectable) LMCRecorderStore
> 					163 콘솔 트리 (selectable) LMCSdoExecutor
> 					164 콘솔 트리 (selectable) TCPIPServer
> 					165 콘솔 트리 (selectable) TCPMotionInterface
> 			120046 탭 항목 (selectable) Lib
> 			120047 탭 항목 (selectable) File
> 			120048 탭 항목 (selectable) Global
> 			120049 탭 항목 (selectable) Net
> 			120050 탭 항목 (selectable) Class
> 			171 단추 Close
> 		172 Tab Properties ID: 296581344
> 			173 창 ID: 287643296
> 				174 TABLE Properties Window ID: 294249272
> 					120051 custom Name
> 					120052 custom Type
> 					120053 custom Pointer
> 					120054 custom Register
> 					120055 custom Init Value
> 					120056 custom Comment
> 				175 도구 모음 ID: 59392
> 					176 단추
> 					177 단추
> 			120057 탭 항목 (selectable) Properties
> 			179 단추 Close
>
> The focused UI element is 143 트리 ID: 103.
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
>   "title": "Set command id type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1340,y:499,screenshotId:\"screenshot-0\"}); await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"UINT\"}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "Add request pointer input",
>   "code": "globalThis.addLasalInput = async function(memberName) { await globalThis.sky.click({window:globalThis.targetWindow,element_index:118447,mouse_button:\"right\"}); var popupState = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var popupShot = popupState.screenshots.find(function(s){ return s.width >= 170 && s.width <= 200 && s.height >= 180 && s.height <= 205; }); if (!popupShot) throw new Error(\"input menu screenshot not found\"); await globalThis.sky.click({window:globalThis.targetWindow,x:75,y:129,screenshotId:popupShot.id}); await globalThis.sky.type_text({window:globalThis.targetWindow,text:memberName}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"}); }; await globalThis.addLasalInput(\"pRequest\");"
> }
> ```
>
> ```text
> element 118447 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL class tree",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.match(/\\d+ 콘솔 트리 \\(selectable\\) HandleEtherCATTopologyIoRequest/)?.[0] || \"helper not visible\");"
> }
> ```
>
> ```text
> 118447 콘솔 트리 (selectable) HandleEtherCATTopologyIoRequest
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
>   "title": "Select UINT command id type",
>   "code": "for (var i=0;i<10;i++) await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"DOWN\"}); await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ENTER\"});"
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
>   "title": "Add request pointer input",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); await globalThis.addLasalInput(\"pRequest\");"
> }
> ```
>
> ```text
> input menu screenshot not found
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect input menu bounds",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({shots:globalThis.state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height,originX:s.originX,originY:s.originY})),head:globalThis.state.accessibility.tree.slice(0,700)});"
> }
> ```
>
> ```text
> {
>   shots: [
>     {
>       id: 'screenshot-0',
>       width: 1450,
>       height: 763,
>       originX: -1880,
>       originY: 41
>     },
>     {
>       id: 'screenshot-1',
>       width: 1440,
>       height: 753,
>       originX: -1875,
>       originY: 46
>     },
>     {
>       id: 'screenshot-2',
>       width: 253,
>       height: 231,
>       originX: -547,
>       originY: 382
>     }
>   ],
>   head: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService*", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService* Secondary Actions: Raise\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t2 창 LMCDiagnosticsService* Secondary Actions: Raise ID: 65282\n' +
>     '\t\t\t\t3 창 ID: 59648\n' +
>     '\t\t\t\t\t4 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #define LMC_DIAG_MAP_REVISION 0x957F101E #define LMC_DIAG_TOPOLOGY_REVISION 0x15867EEC #define LMC_DIAG_ERROR_ID -32000 #define LMC_DIAG_D1_ENABLED TRUE #define LMC_DIAG_D2_ENABLED TRUE #define LMC_DIAG_D3_ENABLED TRUE #define LMC_DIAG_D5_SDO_READ_ENABLED TRUE #define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED TRU'
> }
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
>   "title": "Dismiss text context menu",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"ESC\"}); globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(\"ready\");"
> }
> ```
>
> ```text
> ready
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
>   "title": "Open helper member menu",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1300,y:338,screenshotId:\"screenshot-0\",mouse_button:\"right\"});"
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
>   "title": "Verify helper menu",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({shots:globalThis.state.screenshots.map(s=>({id:s.id,width:s.width,height:s.height})),head:globalThis.state.accessibility.tree.slice(0,600)});"
> }
> ```
>
> ```text
> {
>   shots: [
>     { id: 'screenshot-0', width: 1450, height: 763 },
>     { id: 'screenshot-1', width: 1440, height: 753 },
>     { id: 'screenshot-2', width: 182, height: 192 },
>     { id: 'screenshot-3', width: 182, height: 4 },
>     { id: 'screenshot-4', width: 4, height: 188 }
>   ],
>   head: 'Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService*", App: Lasal2.exe.\n' +
>     '\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService* Secondary Actions: Raise\n' +
>     '\t\t121260 창\n' +
>     '\t\t121261 창\n' +
>     '\t\t121262 메뉴\n' +
>     '\t\t\t121346 메뉴 항목 Edit Method Enter\n' +
>     '\t\t\t121347 메뉴 항목 Delete Method Del\n' +
>     '\t\t\t121348 메뉴 항목 Move Up\n' +
>     '\t\t\t121349 메뉴 항목 (disabled) Move Down\n' +
>     '\t\t\t121350 메뉴 항목 Copy\n' +
>     '\t\t\t121351 메뉴 항목 New Input Variable\n' +
>     '\t\t\t121352 메뉴 항목 New Output Variable\n' +
>     '\t\t\t121353 메뉴 항목 Add to Newinst\n' +
>     '\t\t1 창 작업 영역 ID: 59648\n' +
>     '\t\t\t2 창 LMCDiagnosticsService* Secondary Actions: Raise ID: 65282\n' +
>     '\t\t\t\t3 창 I'
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg

>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify Coupler object channel",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/Coupler|custom Class|custom Type|custom DataType|custom Required/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							18322 콘솔 트리 (selectable) Coupler
> 					28275 custom Class
> 					28276 custom DataType
> 					28277 custom Type
> 					28278 custom Required
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
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Locate current LASAL class item",
>   "code": "nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/콘솔 트리.*LMCEcatInputLatch$/.test(x)).join(\"\\n\"));"
> }
> ```
>
> ```text
> 					161 콘솔 트리 (selectable) LMCEcatInputLatch
> ```
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open class declaration menu",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:161,mouse_button:\"right\"});"
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
>   "title": "Refresh class declaration menu",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/메뉴 항목.*New Client/.test(x)).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 			30244 메뉴 항목 New Client
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
>   "title": "Add InputSlot client",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1325,y:485,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Name InputSlot client",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"InputSlot\"});"
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
>   "title": "Confirm InputSlot client name",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Return\"});"
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
>   "title": "Inspect InputSlot defaults",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/InputSlot|custom Type|custom Class/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							31405 콘솔 트리 (selectable) InputSlot
> 					31788 custom Type
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
>   "title": "Set InputSlot command channel intermediate",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:517,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Refresh InputSlot type",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:false}); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
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
>   "title": "Set InputSlot object channel",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:517,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Refresh InputSlot object channel",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/InputSlot|custom Class|custom Type/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							31405 콘솔 트리 (selectable) InputSlot
> 					33968 custom Class
> 					33970 custom Type
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
>   "title": "Open InputSlot target class list",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:499,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
> }
> ```

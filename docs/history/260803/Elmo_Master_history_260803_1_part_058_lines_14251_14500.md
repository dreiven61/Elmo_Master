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
>   "title": "Filter InputSlot target class",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"GL_9086_1_Slot00\"});"
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
>   "title": "Confirm InputSlot target class",
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
>   "title": "Verify InputSlot declaration",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/InputSlot|custom Class|custom Type|custom Required/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							31405 콘솔 트리 (selectable) InputSlot
> 					35184 custom Class
> 					35186 custom Type
> 					35187 custom Required
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
>   "title": "Open class menu for OutputSlot",
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
>   "title": "Refresh OutputSlot declaration menu",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/메뉴 항목.*New Client/.test(x)).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 			36255 메뉴 항목 New Client
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
>   "title": "Add OutputSlot client",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1325,y:464,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Name OutputSlot client",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:\"OutputSlot\"});"
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
>   "title": "Confirm OutputSlot client name",
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
>   "title": "Inspect OutputSlot defaults",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/OutputSlot|custom Type|custom Class/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							37416 콘솔 트리 (selectable) OutputSlot
> 					37799 custom Type
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
>   "title": "Set OutputSlot command channel intermediate",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:516,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Refresh OutputSlot type",
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
>   "title": "Set OutputSlot object channel",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1360,y:516,screenshotId:globalThis.state.screenshots[0].id,click_count:2});"
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
>   "title": "Refresh OutputSlot object channel",
>   "code": "globalThis.state = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.split(\"\\n\").filter(x=>/OutputSlot|custom Class|custom Type/.test(x)).slice(-20).join(\"\\n\")); for (const s of globalThis.state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> 							37416 콘솔 트리 (selectable) OutputSlot
> 					39980 custom Class
> 					39982 custom Type
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

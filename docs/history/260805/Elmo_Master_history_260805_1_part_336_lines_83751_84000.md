>   "title": "Inspect control class",
>   "code": "var controlClassState = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); nodeRepl.write({title:controlClassState.window.title}); await nodeRepl.emitImage(controlClassState.screenshots[controlClassState.screenshots.length-1].url);"
> }
> ```
>
> ```text
> {
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCControlCommandService'
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
>   "title": "Search control class front section",
>   "code": "var controlFindOpen1 = await globalThis.sky.press_key({window:controlClassState.window,key:\"Control_L+F\"}); nodeRepl.write(\"find-open\");"
> }
> ```
>
> ```text
> find-open
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect control search field",
>   "code": "var controlFindState1 = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); await nodeRepl.emitImage(controlFindState1.screenshots[controlFindState1.screenshots.length-1].url);"
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Enter front-section symbol",
>   "code": "var controlFindType1 = await globalThis.sky.type_text({window:controlFindState1.window,text:\"liveNonGroupMask\"}); nodeRepl.write(\"typed\");"
> }
> ```
>
> ```text
> typed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify front-section search term",
>   "code": "var controlFindTyped1 = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); await nodeRepl.emitImage(controlFindTyped1.screenshots[controlFindTyped1.screenshots.length-1].url);"
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Set front-section search direction",
>   "code": "var controlFindUp1 = await globalThis.sky.click({window:controlFindTyped1.window,screenshotId:controlFindTyped1.screenshots[controlFindTyped1.screenshots.length-1].id,x:500,y:347}); nodeRepl.write(\"up\");"
> }
> ```
>
> ```text
> point (500, 347) is outside window bounds { originX: 0, originY: 0, width: 823, height: 35 }
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Locate control search dialog",
>   "code": "nodeRepl.write(controlFindTyped1.screenshots.map(s=>({id:s.id,z:s.zIndex,ox:s.originX,oy:s.originY,w:s.width,h:s.height})));"
> }
> ```
>
> ```text
> [
>   { id: 'screenshot-0', z: 0, ox: 182, oy: 243, w: 1449, h: 763 },
>   { id: 'screenshot-1', z: 1, ox: 187, oy: 248, w: 1439, h: 753 },
>   { id: 'screenshot-2', z: 2, ox: 363, oy: 478, w: 504, h: 220 },
>   { id: 'screenshot-3', z: 3, ox: 787, oy: 364, w: 823, h: 35 }
> ]
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
>   "title": "Set front-section search direction",
>   "code": "var controlFindUp2 = await globalThis.sky.click({window:controlFindTyped1.window,screenshotId:\"screenshot-1\",x:500,y:347}); nodeRepl.write(\"up\");"
> }
> ```
>
> ```text
> up
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Confirm front-section search direction",
>   "code": "var controlFindStateUp1 = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); await nodeRepl.emitImage(controlFindStateUp1.screenshots[2].url);"
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
>   "title": "Find front-section symbol",
>   "code": "var controlFindExec1 = await globalThis.sky.click({window:controlFindStateUp1.window,screenshotId:\"screenshot-1\",x:629,y:296}); nodeRepl.write(\"find\");"
> }
> ```
>
> ```text
> find
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify front-section search hit",
>   "code": "await new Promise(r=>setTimeout(r,1200)); var controlFrontHit = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); await nodeRepl.emitImage(controlFrontHit.screenshots[controlFrontHit.screenshots.length-1].url);"
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
>   "title": "Select front-section search result",
>   "code": "var controlFrontResultSelect = await globalThis.sky.click({window:controlFrontHit.window,screenshotId:\"screenshot-1\",x:401,y:462}); nodeRepl.write(\"selected-result\");"
> }
> ```
>
> ```text
> selected-result
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect selected front result",
>   "code": "var controlFrontSelected = await globalThis.sky.get_window_state({window:globalThis.freshLasalWindow}); await nodeRepl.emitImage(controlFrontSelected.screenshots[controlFrontSelected.screenshots.length-1].url);"
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
>   "title": "Open front-section search result",
>   "code": "var controlFrontOpen = await globalThis.sky.press_key({window:controlFrontSelected.window,key:\"Enter\"}); nodeRepl.write(\"open-result\");"
> }
> ```
>
> ```text
> open-result
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Verify front-section navigation",

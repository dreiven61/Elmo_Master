>   x?: number; // Window-relative X coordinate.
>   y?: number; // Window-relative Y coordinate.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   x: number; // Window-relative X coordinate to scroll from.
>   y: number; // Window-relative Y coordinate to scroll from.
>   from_x: number; // Starting window-relative X coordinate.
>   from_y: number; // Starting window-relative Y coordinate.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   to_x: number; // Ending window-relative X coordinate.
>   to_y: number; // Ending window-relative Y coordinate.
>   originX?: number; // Screen X origin for this bounded screenshot region, when available.
>   originY?: number; // Screen Y origin for this bounded screenshot region, when available.
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
>   "title": "Choose UDINT for LASAL variable",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1038,y:682,screenshotId:globalThis.state.screenshots[2].id});"
> }
> ```
>
> ```text
> point (1038, 682) is outside window bounds { originX: 0, originY: 0, width: 410, height: 452 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL type menu",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.map((s,i)=>({i,id:s.id,w:s.width,h:s.height,ox:s.originX,oy:s.originY,z:s.zIndex})));"
> }
> ```
>
> ```text
> [
>   {
>     i: 0,
>     id: 'screenshot-0',
>     w: 1450,
>     h: 763,
>     ox: -1880,
>     oy: 41,
>     z: 0
>   },
>   {
>     i: 1,
>     id: 'screenshot-1',
>     w: 1440,
>     h: 753,
>     ox: -1875,
>     oy: 46,
>     z: 1
>   },
>   { i: 2, id: 'screenshot-2', w: 410, h: 452, ox: -864, oy: 569, z: 2 },
>   { i: 3, id: 'screenshot-3', w: 115, h: 16, ox: -569, oy: 551, z: 3 }
> ]
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
>   "title": "Choose UDINT for LASAL variable",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:36,y:264,screenshotId:globalThis.state.screenshots[2].id});"
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
>   "title": "Verify LASAL UDINT type",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({accessibility:!!globalThis.state.accessibility,focused:globalThis.state.accessibility?.focused_element,props:globalThis.state.accessibility?.tree.match(/TABLE Properties Window[\\s\\S]{0,500}/)?.[0]});"
> }
> ```
>
> ```text
> {
>   accessibility: true,
>   focused: '105 TABLE Properties Window ID: 301947776',
>   props: 'TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t30619 custom Name\n' +
>     '\t\t\t\t\t30620 custom GUID\n' +
>     '\t\t\t\t\t30621 custom Type\n' +
>     '\t\t\t\t\t30622 custom Pointer\n' +
>     '\t\t\t\t\t30623 custom Comment\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t30624 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 105 TABLE Properties Window ID: 301947776.'
> }
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
>   "title": "Reopen LASAL type selector",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1362,y:518,click_count:2,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Verify LASAL type selector",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.map((s,i)=>({i,w:s.width,h:s.height,ox:s.originX,oy:s.originY})));"
> }
> ```
>
> ```text
> [
>   { i: 0, w: 1450, h: 763, ox: -1880, oy: 41 },
>   { i: 1, w: 1440, h: 753, ox: -1875, oy: 46 },
>   { i: 2, w: 410, h: 452, ox: -864, oy: 569 },
>   { i: 3, w: 115, h: 16, ox: -569, oy: 551 }
> ]
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
>   "title": "Select previous LASAL type",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Up'});"
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
>   "title": "Verify LASAL type selection",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.map((s,i)=>({i,w:s.width,h:s.height})));"
> }
> ```
>
> ```text
> [
>   { i: 0, w: 1450, h: 763 },
>   { i: 1, w: 1440, h: 753 },
>   { i: 2, w: 410, h: 452 },
>   { i: 3, w: 115, h: 16 }
> ]
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
>   "title": "Select UDINT LASAL type",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Up'});"
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
>   "title": "Verify UDINT selection",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.screenshots.map((s,i)=>({i,w:s.width,h:s.height})));"
> }
> ```
>
> ```text
> [
>   { i: 0, w: 1450, h: 763 },
>   { i: 1, w: 1440, h: 753 },
>   { i: 2, w: 410, h: 452 },
>   { i: 3, w: 115, h: 16 }
> ]
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>

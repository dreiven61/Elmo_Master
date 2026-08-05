>   "title": "Verify DS402 token dropdown",
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
>   "title": "Select UDINT token type",
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
>   "title": "Verify UDINT token selection",
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
>   "title": "Commit UDINT token selection",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});"
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
>   "title": "Confirm DS402 token declaration",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility?.focused_element,props:globalThis.state.accessibility?.tree.match(/TABLE Properties Window[\\s\\S]{0,300}/)?.[0]});"
> }
> ```
>
> ```text
> {
>   focused: '105 TABLE Properties Window ID: 301947776',
>   props: 'TABLE Properties Window ID: 301947776\n' +
>     '\t\t\t\t\t44768 custom Name\n' +
>     '\t\t\t\t\t44769 custom Type\n' +
>     '\t\t\t\t\t44770 custom Pointer\n' +
>     '\t\t\t\t\t44771 custom Register\n' +
>     '\t\t\t\t\t44772 custom Init Value\n' +
>     '\t\t\t\t\t44773 custom Comment\n' +
>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t44774 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 10'
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
>   "title": "Open DS402 control method menu",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,element_index:38311,mouse_button:'right'});"
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
>   "title": "Inspect DS402 method menu",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.focused_element);"
> }
> ```
>
> ```text
> 98 트리 ID: 103
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
>   "title": "Add DS402 axis input",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1392,y:449,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Verify DS402 axis placeholder",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.focused_element);"
> }
> ```
>
> ```text
> 46535 편집 ID: 1
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
>   "title": "Name DS402 axis input",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'AxisReference'});"
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
>   "title": "Verify DS402 axis input name",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.focused_element);"
> }
> ```
>
> ```text
> 46535 편집 ID: 1
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
>   "title": "Commit DS402 axis input",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});"
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
>   "title": "Verify DS402 axis declaration",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); var sm11=globalThis.state.accessibility.tree.lastIndexOf('SubmitDs402HomeControl'); nodeRepl.write(globalThis.state.accessibility.tree.slice(sm11,sm11+400));"
> }
> ```
>
> ```text

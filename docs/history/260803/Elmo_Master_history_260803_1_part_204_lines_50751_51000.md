>     '\t\t\t\t106 도구 모음 ID: 59392\n' +
>     '\t\t\t\t\t107 단추\n' +
>     '\t\t\t\t\t108 단추\n' +
>     '\t\t\t28822 탭 항목 (selectable) Properties\n' +
>     '\t\t\t110 단추 Close\n' +
>     '\n' +
>     'The focused UI element is 28597 편집 ID: 1.'
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
>   "title": "Set LASAL variable type",
>   "code": "await globalThis.sky.set_value({window:globalThis.targetWindow,element_index:28819,value:'UDINT'});"
> }
> ```
>
> ```text
> element 28819 is not available in cached app state for Lasal2.exe
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL after type edit attempt",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state.accessibility.tree.match(/TABLE Properties Window[\\s\\S]{0,500}/)?.[0]);"
> }
> ```
>
> ```text
> TABLE Properties Window ID: 301947776
> 					29393 custom Name
> 					29394 custom GUID
> 					29395 custom Type
> 					29396 custom Pointer
> 					29397 custom Comment
> 				106 도구 모음 ID: 59392
> 					107 단추
> 					108 단추
> 			29398 탭 항목 (selectable) Properties
> 			110 단추 Close
>
> The focused UI element is 28597 편집 ID: 1.
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
>   "title": "Edit LASAL variable type",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:1364,y:512,click_count:2,screenshotId:globalThis.state.screenshots[0].id});"
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
>   "title": "Verify LASAL type editor",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({focused:globalThis.state.accessibility.focused_element,tail:globalThis.state.accessibility.tree.slice(-900)});"
> }
> ```
>
> ```text
> Cannot read properties of null (reading 'focused_element')
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Refresh LASAL type editor",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write(globalThis.state);"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 3674522,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=50863 chars=9977 sha256=349b71e7cabc92b8b04e9cb1d6ef2b2220331b187e5e138951852dd6ffc5f498]'... 259675 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=50872 chars=9977 sha256=0bb7e4757d2a2bf01ea096ac4c40115de28f68ff80f0ed2dd6470391555c9d51]'... 249015 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[OMITTED_BASE64 source=Emlo_Master_history_260803_1.md line=50881 chars=9977 sha256=dba6f4f85e928056b3270ee95d5e78cf47107dbcd844aadbb4990d49a838002c]'... 42435 more characters,
>       originX: -864,
>       originY: 569,
>       width: 410,
>       height: 452
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQAHMDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3aa+8VX+v65Bot3okVtp0kcaW93ZSu85MSSEGZZgEBLYz5bY64bpVG78a6jd6RJc6LbWsdxJZ2E0Ed0GYJLPcNEyuVYZC7eoxzk8itS68O66Nb1W50vXrSxstSdHlX+zjJcRlY1jJjlMuwHC5G6NgD2NSS+D4BhLS4MNukNjCiFC5AtpzLyxOSWzjPbrz0pJaDb/r5r9LlZfEN/rP9mw6K1rYz3lpcSubyBpzbzQyxI8bKsiZwXkU4bqAc4GDyvhm+1rU/GegX2u3WnTGbSdTESWdo8Hl7bm0VtxaV92cAjG3HPXPHcReGlt/GUuuwXOyKWB45LXZwZWMeZQ2eMrEgIx2z65wLHQp9J8V+HoV8y4t7fTNSR7gRFUDSXFo6qeoBIDYGedpPY1rF6L+upm+xv3Z61iXbcmty7XrWLdJ1rtpHHUOT8catfaJ4P1fVdKhgnvbOIThJ1LKUDDfwCOduT+FebeE/jxo+oukGv6ZdafcsQBJZ/v42P8Aun5h+Ga9jkt4ZxJBdoHtpkaKVSMgowII/I1598IPh4vgmyvJr+3t5NYmuXEc3yyNHApwu084LfeOOemaKiqe0XI9wpuHI+dbHpsOVcDOeAQcY4IzWvZnpWRbqS2TyT3rZtF6VVTbUmnua1selXo+lUrYVdjHFcEztiTCigUVmaBRRRQB/9k=',
>       originX: -569,
>       originY: 551,
>       width: 115,
>       height: 16
>     }
>   ],
>   accessibility: null
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
>   "title": "Select LASAL type text",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'CTRL+A'});"
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
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({shots:globalThis.state.screenshots.length,accessibility:globalThis.state.accessibility});"
> }
> ```
>
> ```text
> { shots: 4, accessibility: null }
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
>   "title": "Enter LASAL type",
>   "code": "await globalThis.sky.type_text({window:globalThis.targetWindow,text:'UDINT'});"
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
>   "title": "Verify entered LASAL type",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({shots:globalThis.state.screenshots.length,accessibility:globalThis.state.accessibility});"
> }
> ```
>
> ```text
> { shots: 4, accessibility: null }
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
>   "title": "Check transient screenshot coordinates",
>   "code": "var gText3=String(globalThis.apiDoc2); nodeRepl.write(gText3.split('\\n').filter(x=>/screenshotId|originX|window-relative|bounded/i.test(x)).slice(0,50).join('\\n'));"
> }
> ```
>
> ```text
>   screenshots: Array<Screenshot>; // Bounded screenshots captured for the window and related transient UI.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.

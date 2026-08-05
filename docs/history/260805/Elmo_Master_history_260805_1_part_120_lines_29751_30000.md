>   keys: [ 'window', 'screenshots', 'accessibility' ],
>   stateText: '{\n' +
>     '  "window": {\n' +
>     '    "app": "process:C:\\\\Program Files (x86)\\\\Sigmatek\\\\Lasal\\\\Class2\\\\Bin\\\\Lasal2.exe",\n' +
>     '    "id": 1838028,\n' +
>     '    "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"\n' +
>     '  },\n' +
>     '  "screenshots": [\n' +
>     '    {\n' +
>     '      "id": "screenshot-0",\n' +
>     '      "zIndex": 0,\n' +
>     '      "url": "data:image/jpeg;base64,[BASE64_OMITTED source_line=29762 chars=9669 sha256=7a0e2f68b4564867a33372f8460061e80c9dae77619aa1113660f4b9e814975e]'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 계층 확인",
>   "code": "nodeRepl.write(state.screenshots.map(s=>({id:s.id,zIndex:s.zIndex,bounds:s.bounds,shape:[s.width,s.height]})))"
> }
> ```
>
> ```text
> [
>   {
>     id: 'screenshot-0',
>     zIndex: 0,
>     bounds: undefined,
>     shape: [ 970, 512 ]
>   },
>   {
>     id: 'screenshot-1',
>     zIndex: 1,
>     bounds: undefined,
>     shape: [ 960, 502 ]
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
>   "title": "Computer Use 입력 API 확인",
>   "code": "nodeRepl.write({keys:Object.keys(sky),proto:Object.getOwnPropertyNames(Object.getPrototypeOf(sky))})"
> }
> ```
>
> ```text
> {
>   keys: [
>     'target',
>     'activate_window',
>     'get_window_state',
>     'click',
>     'scroll',
>     'drag',
>     'press_key',
>     'type_text',
>     'launch_app',
>     'list_apps',
>     'list_windows',
>     'get_window',
>     'perform_secondary_action',
>     'set_value',
>     'transport'
>   ],
>   proto: [ 'constructor', 'documentation', 'close' ]
> }
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
>   "title": "Computer Use 좌표 입력 형식 확인",
>   "code": "nodeRepl.write(String(await sky.documentation()).slice(0,12000))"
> }
> ```
>
> ```text
> Unsupported Computer Use documentation
> ```
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Computer Use 보조 클릭 형식 확인",
>   "code": "nodeRepl.write({secondary:String(sky.perform_secondary_action),click:String(sky.click),press:String(sky.press_key)})"
> }
> ```
>
> ```text
> {
>   secondary: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',
>   click: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }',
>   press: '(...args) => {\r\n' +
>     '        const app = getComputerUseAppReference(args[0]);\r\n' +
>     '        globals.nodeRepl?.setResponseMeta({\r\n' +
>     '          [TOOL_SURFACE_META_KEY]: {\r\n' +
>     '            kind: "computerUse",\r\n' +
>     '            app,\r\n' +
>     '          },\r\n' +
>     '          ...(isChromeComputerUseAppReference(app)\r\n' +
>     '            ? { [CHROME_COMPUTER_USE_META_KEY]: true }\r\n' +
>     '            : {}),\r\n' +
>     '        });\r\n' +
>     '        return Reflect.apply(value, target, args);\r\n' +
>     '      }'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 원점 확인",
>   "code": "nodeRepl.write(state.screenshots.map(s=>({id:s.id,z:s.zIndex,originX:s.originX,originY:s.originY,width:s.width,height:s.height})))"
> }
> ```
>
> ```text
> [
>   {
>     id: 'screenshot-0',
>     z: 0,
>     originX: -1895,
>     originY: 22,
>     width: 970,
>     height: 512
>   },
>   {
>     id: 'screenshot-1',
>     z: 1,
>     originX: -1890,
>     originY: 27,
>     width: 960,
>     height: 502
>   }
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
>   "title": "LASAL 메서드 입력 변수 메뉴 열기",
>   "code": "await sky.click({window:targetWindow,screenshotId:\"screenshot-1\",x:826,y:217,mouse_button:\"right\"}); nodeRepl.write(\"right-clicked method\");"
> }
> ```
>
> ```text
> right-clicked method
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 메서드 메뉴 확인",
>   "code": "globalThis.state = await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true}); nodeRepl.write({screens:state.screenshots.map(s=>({id:s.id,z:s.zIndex,originX:s.originX,originY:s.originY,width:s.width,height:s.height})),tree:state.accessibility?.tree?.slice(0,8000)}); for (const s of state.screenshots) await nodeRepl.emitImage(s.url);"
> }
> ```
>
> ```text
> {
>   screens: [
>     {
>       id: 'screenshot-0',
>       z: 0,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       z: 1,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
>     },
>     {
>       id: 'screenshot-2',
>       z: 2,
>       originX: -1064,
>       originY: 244,
>       width: 182,
>       height: 192
>     },
>     {
>       id: 'screenshot-3',
>       z: 3,
>       originX: -1060,
>       originY: 436,
>       width: 182,
>       height: 4
>     },
>     {
>       id: 'screenshot-4',
>       z: 4,
>       originX: -882,

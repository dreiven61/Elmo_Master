> {
>   "sky": [
>     "target",
>     "activate_window",
>     "get_window_state",
>     "click",
>     "scroll",
>     "drag",
>     "press_key",
>     "type_text",
>     "launch_app",
>     "list_apps",
>     "list_windows",
>     "get_window",
>     "perform_secondary_action",
>     "set_value",
>     "transport"
>   ],
>   "win": [
>     "app",
>     "id",
>     "title"
>   ]
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
>   "code": "var diagFindShot48 = await sky.get_window_state({window: lasalWindow8}); nodeRepl.write(JSON.stringify(diagFindShot48.screenshots?.map((s,i)=>({i,originX:s.originX,originY:s.originY,width:s.width,height:s.height})),null,2))",
>   "title": "검색 대화상자 상태 갱신"
> }
> ```
>
> ```text
> [
>   {
>     "i": 0,
>     "originX": -1880,
>     "originY": 41,
>     "width": 1450,
>     "height": 763
>   },
>   {
>     "i": 1,
>     "originX": -1875,
>     "originY": 46,
>     "width": 1440,
>     "height": 753
>   },
>   {
>     "i": 2,
>     "originX": 663,
>     "originY": 474,
>     "width": 504,
>     "height": 220
>   }
> ]
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called 2 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(JSON.stringify(Object.keys(diagFindShot48))); nodeRepl.write(\"\\n\"+JSON.stringify(diagFindShot48, (k,v)=>k===\"url\"?\"<url>\":v, 2).slice(0,20000))",
>   "title": "검색 대화상자 요소 확인"
> }
> ```
>
> ```text
> ["window","screenshots","accessibility"]
> {
>   "window": {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 10750076,
>     "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"
>   },
>   "screenshots": [
>     {
>       "id": "screenshot-0",
>       "zIndex": 0,
>       "url": "<url>",
>       "originX": -1880,
>       "originY": 41,
>       "width": 1450,
>       "height": 763
>     },
>     {
>       "id": "screenshot-1",
>       "zIndex": 1,
>       "url": "<url>",
>       "originX": -1875,
>       "originY": 46,
>       "width": 1440,
>       "height": 753
>     },
>     {
>       "id": "screenshot-2",
>       "zIndex": 2,
>       "url": "<url>",
>       "originX": 663,
>       "originY": 474,
>       "width": 504,
>       "height": 220
>     }
>   ],
>   "accessibility": null
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(sky.click.toString())",
>   "title": "클릭 좌표 규약 확인"
> }
> ```
>
> ```text
> (...args) => {
>         const app = getComputerUseAppReference(args[0]);
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app,
>           },
>           ...(isChromeComputerUseAppReference(app)
>             ? { [CHROME_COMPUTER_USE_META_KEY]: true }
>             : {}),
>         });
>         return Reflect.apply(value, target, args);
>       }
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
>   "code": "await sky.press_key({window: lasalWindow8, key: \"ALT+S\"}); nodeRepl.write(\"pressed\")",
>   "title": "검색 결과창 옵션 전환"
> }
> ```
>
> ```text
> pressed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var diagFindShot49 = await sky.get_window_state({window: lasalWindow8}); await nodeRepl.emitImage(diagFindShot49.screenshots[diagFindShot49.screenshots.length-1].url)",
>   "title": "검색 옵션 확인"
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
>   "code": "var lasalWins50 = await sky.list_windows({app: lasalApp}); nodeRepl.write(JSON.stringify(lasalWins50,null,2))",
>   "title": "LASAL 대화상자 식별"
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "id": 10750076,
>     "title": "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCDiagnosticsService"
>   },
>   {
>     "app": "OpenAI.Codex_2p2nqsd0c76g0!App",
>     "id": 3149508,
>     "title": "ChatGPT"
>   },
>   {
>     "app": "Chrome",
>     "id": 1247078,
>     "title": "Daou Office - Chrome"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 3742080,
>     "title": "history - 파일 탐색기 [ main ↓0 ↑17 ]"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "id": 594816,
>     "title": "혜진, 강동권, 민정이, 어머님"
>   },
>   {
>     "app": "process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
>     "id": 69144,
>     "title": "카카오톡"
>   },
>   {
>     "app": "process:C:\\Windows\\explorer.exe",
>     "id": 2823732,
>     "title": "26_07 - 파일 탐색기"
>   },
>   {
>     "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Wireshark\\Wireshark.exe",
>     "id": 3085686,
>     "title": "Home Check_Read Legacy Elmo Health.pcapng"
>   },
>   {
>     "app": "Microsoft.WindowsNotepad_8wekyb3d8bbwe!App",
>     "id": 1116302,
>     "title": "*우리가 정리한 설계문서와 기능 구현 문서 리스트를 토대로 개발을 - 메모장"
>   }
> ]
> ```
>
> MCP tool call

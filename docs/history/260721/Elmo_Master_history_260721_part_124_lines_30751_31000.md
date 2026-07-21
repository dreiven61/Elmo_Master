>
> ```text
> [
>   'activate_window',
>   'click',
>   'drag',
>   'get_window',
>   'get_window_state',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'perform_secondary_action',
>   'press_key',
>   'scroll',
>   'set_value',
>   'transport',
>   'type_text'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write({targetWindow, stateTitle: state?.title, stateApp: state?.app});",
>   "title": "LASAL 창 상태 확인"
> }
> ```
>
> ```text
> {
>   targetWindow: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 134378,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface'
>   },
>   stateTitle: undefined,
>   stateApp: undefined
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var currentLasalInfo = appsLasal[0].windows[0]; var currentLasalWin = await sky.get_window(currentLasalInfo); nodeRepl.write(currentLasalWin);",
>   "title": "LASAL 창 연결"
> }
> ```
>
> ```text
> {
>   app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 10883450,
>   title: 'Lasal Class 2 (02.03.001)'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var currentLasalState = await sky.get_window_state(currentLasalWin); nodeRepl.write(currentLasalState); if(currentLasalState?.screenshot) await nodeRepl.emitImage(currentLasalState.screenshot);",
>   "title": "LASAL 초기 화면 확인"
> }
> ```
>
> ```text
> window.app must be a non-empty string and window.id must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var currentLasalState2 = await sky.get_window_state({window: currentLasalWin}); nodeRepl.write(currentLasalState2); if(currentLasalState2?.screenshot) await nodeRepl.emitImage(currentLasalState2.screenshot);",
>   "title": "LASAL 초기 화면 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 10883450,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,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'... 213275 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,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23ZcHA4YnoK3KKAK1pY29pJPJbx7HnZWkO4ncQoUdenCgfhVSDQdPt71rqCKWNy5kMa3Egh3Hq3lbtme+dvXnrWpRQHkYn/CLaR/o+LZ1WBI0RVnkCkRnKbgGw2DyC2TUx0CxL3Dk3vmT43yfbp94Gc4Vt+UGey4FatFAGbNoljNpcOntHItrEVZBHM6MCpyDvUhs55znJNSw6bbxafJZZnkgkDKwnneZiCMEbnJP61dooAxofD9qy2j3+bm5t1VPNDMglCHKb0DYcjqN2cHJGM0R+GtNjS4jVbswzrIkkLXs7RkPnd8hfaM5PQd+K2aKNwWhmf2Fp324Xf2c+cO3mNszt27tmdu7bxuxuxxmmReHtNjjkQxTS+Y0bM09xJK/wAjbkG5mJADc4zjrxya1qKAMtdA0wTXEptQxnV1dHdmQBzlwqE7V3Hk7QMnk5p0GiWEOn3VksTtBdArP5kzyPICu05diWPHHXgDitKigPMox2BGrtfSShgsIghjC48sE5Yk55JIX0xt+tXqKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigClq2k6drFstvq9haX8CuJFjuoVlUMARuAYEZwSM+5qlp3hTw7pl5Hd6boGk2l3HnZNBZxxuuQQcMACMgkfjW1RQAUUUUAUtb02HWNFv9MuWkSC9t5LaRoyAwV1KkgkEZwfQ1k/8ACPan/wBDhr3/AH5sf/keujooApaJpsOj6LYaZbNI8Flbx20bSEFiqKFBJAAzgegq7RRQAVz+j+D9E0ldVFvZrIdUleW8e4Jlabdn5WLZJUZIA/xNdBRQBk+F/D9h4Z0hNM0lZUs43d0SSRn2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'... 208743 more characters,
>       originX: -1875,
>       originY: 46,
>       width: 1440,
>       height: 753
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.activate_window({window: currentLasalWin}); await sky.click({window: currentLasalWin, x:45, y:38}); await new Promise(r=>setTimeout(r,300)); var lasalMenuState = await sky.get_window_state({window: currentLasalWin}); nodeRepl.write({title:lasalMenuState.window.title, shots:lasalMenuState.screenshots.length}); for (var im of lasalMenuState.screenshots) await nodeRepl.emitImage(im.url);",
>   "title": "LASAL 프로젝트 열기 메뉴"
> }
> ```
>
> ```text
> { title: 'Lasal Class 2 (02.03.001)', shots: 5 }
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
>   "code": "await sky.click({window: currentLasalWin, x:95, y:88}); await new Promise(r=>setTimeout(r,400)); var openDialogState = await sky.get_window_state({window: currentLasalWin}); nodeRepl.write({title:openDialogState.window.title, count:openDialogState.screenshots.length, acc:openDialogState.accessibility}); for (var im2 of openDialogState.screenshots) await nodeRepl.emitImage(im2.url);",
>   "title": "LASAL 프로젝트 파일 선택"
> }
> ```
>
> ```text
> { title: 'Lasal Class 2 (02.03.001)', count: 3, acc: null }
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.click({window: currentLasalWin, x:520, y:292}); await sky.click({window: currentLasalWin, x:1740, y:985}); await new Promise(r=>setTimeout(r,2500)); var loadedState = await sky.get_window_state({window: currentLasalWin}); nodeRepl.write({title:loadedState.window.title, shots:loadedState.screenshots.length}); for (var im3 of loadedState.screenshots) await nodeRepl.emitImage(im3.url);",
>   "title": "LASAL 프로젝트 로드"
> }
> ```
>
> ```text
> point (1740, 985) is outside window bounds { originX: 0, originY: 0, width: 1450, height: 763 }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window: currentLasalWin, key:'ENTER'}); await new Promise(r=>setTimeout(r,2500)); var loadedState2 = await sky.get_window_state({window: currentLasalWin}); nodeRepl.write({title:loadedState2.window.title, shots:loadedState2.screenshots.length}); for (var im4 of loadedState2.screenshots) await nodeRepl.emitImage(im4.url);",
>   "title": "LASAL 프로젝트 열기"
> }
> ```
>
> ```text
> { title: 'Lasal Class 2 (02.03.001)', shots: 3 }
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var allWinNow = await sky.list_windows(); nodeRepl.write(allWinNow.filter(w=>/Lasal|Projectfile|Open/i.test(w.title||'')));",
>   "title": "LASAL 파일 대화상자 연결"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 10883450,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\Common7\\IDE\\devenv.exe',
>     id: 68062,
>     title: 'LasalApiWpfTestApp - Microsoft Visual Studio'
>   }

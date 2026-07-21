>
> node_repl.js
>
> ```json
> {
>   "code": "\nnodeRepl.write({\n  hasSky: typeof sky !== \"undefined\",\n  hasWindow: typeof lasalWinCU !== \"undefined\",\n  cwd: nodeRepl.cwd\n})\n",
>   "title": "LASAL IDE 상태 확인"
> }
> ```
>
> ```text
> { hasSky: true, hasWindow: true, cwd: 'C:\\work\\Elmo\\Elmo_Master' }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar shotCU2 = await sky.getScreenshot({window: lasalWinCU});\nawait nodeRepl.emitImage(shotCU2.url)\n",
>   "title": "LASAL Class View 확인"
> }
> ```
>
> ```text
> sky.getScreenshot is not a function
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nnodeRepl.write(Object.keys(sky))\n",
>   "title": "Computer Use API 확인"
> }
> ```
>
> ```text
> [
>   'activate_window',
>   'get_window_state',
>   'click',
>   'scroll',
>   'drag',
>   'press_key',
>   'type_text',
>   'launch_app',
>   'list_apps',
>   'list_windows',
>   'get_window',
>   'perform_secondary_action',
>   'set_value',
>   'transport'
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar winStateCU2 = await sky.get_window({app: lasalWinCU.app});\nnodeRepl.write(winStateCU2)\n",
>   "title": "LASAL 창 상태 가져오기"
> }
> ```
>
> ```text
> id must be an integer >= 0
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar winStateCU3 = await sky.get_window({id: lasalWinCU.id});\nnodeRepl.write(winStateCU3)\n",
>   "title": "LASAL 창 상태 가져오기"
> }
> ```
>
> ```text
> {
>   app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 1837062,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nvar wsCU4 = await sky.get_window_state({window: lasalWinCU});\nnodeRepl.write(wsCU4)\n",
>   "title": "LASAL 화면 상태 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1837062,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Comm_Network'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,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'... 327955 more characters,
>       originX: -1880,
>       originY: 41,
>       width: 1450,
>       height: 763
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,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8r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/78r/hV2ii7Apf2Tp3/Phaf9+V/wAKP7J07/nwtP8Avyv+FXaKLsCl/ZOnf8+Fp/35X/Cj+ydO/wCfC0/7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I8N6aIthS5ZvNE4le7maUOAVB8wtuxgkYzjk+prYooAz7CLVUnY6he2M8O3hYLR4mB9dxlbjrxinWulWVpcCeCHbKFdQ25jgO+9up7tz+XpV6igDJk8O6XJLNI9sxeUlmIlcYJZWJXn5TuVTlcHIFEnh/TpAqtHOIxK0xjW5lVHdm3sWUNhvm5wwIrWoo2Ao6dYm0uL+Z5RI93P5xwu0KAioB1OeEGT3OeBV6iigDLg0HT7e9N1BFLG5cyGNZ5BDuPVvK3bM987evPWoofDWkwx3EaWz+XPE8DI00jKkbfeRAWxGp9Fx0HoK2aKAKE+kWM/2nzYN32i3FpL87DdEN3y9ePvNyOeaR9GsHSdGgys8kc0g3t8zx7dh68Y2L+XPetCigDLutPuo55ZtHntLWW4ffcNcW7z+YQqqpAEi7cBQPf8AnJFpdube7S7jimkvQPtZCkJKdgQ/KSdowAMZ/wAa0KKPIDGHhnS1W0EMM8H2WIQRNBdSxN5Y5CsVYFgPRs9/WpH8P6ZJHPHJbs8U84uXjaVyvmBt24DOFyeSBgHvmtWigDOl0TTpY75HtgVvWDz/ADMCzDGCDn5SMAjGMHnrzSW+h6fbrCEhdmin+0K8kzu5k2ldzMxJY7TjknjHoK0qKAM+30awtrpbmCExzK0j7lkYbi7Fm3DPzDJJwcgHpitCiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigClq2k6drFstvq9haX8CuJFjuoVlUMARuAYEZwSM+5qlp3hTw7pl5Hd6boGk2l3HnZNBZxxuuQQcMACMgkfjW1RQAUUUUAUtb02HWNFv8ATLlpEgvbeS2kaMgMFdSpIJBGcH0NZP8Awj2p/wDQ4a9/35sf/keujooApaJpsOj6LYaZbNI8Flbx20bSEFiqKFBJAAzgegq7RRQAVz+j+D9E0ldVFvZrIdUleW8e4Jlabdn5WLZJUZIA/wATXQUUAZPhfw/YeGdITTNJWVLON3dEkkZ9u5icAnoBngf1yas63/yBb/8A695P/QTV2qWt/wDIFv8A/r3k/wDQTTW4Bon/ACBbD/r3j/8AQRTrqPc7fukbzF2YIyJeH+R/lOF56+px7Fuif8gWw/694/8A0EU66j3O37pG8xdmCMiXh/kf5TheevqcexHuBbooopAcEni25Z9dYa1oDzWC3ZTSliP2lREWCs587ODtBPyDg9utdPo+pTXuo6vbyrGEs5Y0jKg5IaJHOefVj+FMbQ8+H9T0z7R/x+m5Pm7Pueczt0zzjf6jOO1VoNG1W0127u7LUrFbG7kjk'... 325503 more characters,
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
>   "code": "\nawait sky.click({window: lasalWinCU, x: 1056, y: 484, mouse_button: \"right\"});\nawait new Promise(res => setTimeout(res, 500));\nvar menuStateCU5 = await sky.get_window_state({window: lasalWinCU});\nawait nodeRepl.emitImage(menuStateCU5.screenshots.at(-1).url)\n",
>   "title": "Recorder 클래스 메뉴 열기"
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "\nawait sky.click({window: lasalWinCU, x: 1132, y: 173});\nawait new Promise(res => setTimeout(res, 2500));\nvar afterReloadRecCU6 = await sky.get_window_state({window: lasalWinCU});\nawait nodeRepl.emitImage(afterReloadRecCU6.screenshots.at(-1).url)\n",
>   "title": "Recorder 클래스 메타데이터 재생성"
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
>   "code": "\nawait sky.press_key({window: lasalWinCU, key: \"ESC\"});\nawait new Promise(res => setTimeout(res, 500));\n",
>   "title": "잘못 열린 가져오기 창 닫기"
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
>   "code": "\nvar recMenuBaseCU7 = await sky.get_window_state({window: lasalWinCU});\nawait nodeRepl.emitImage(recMenuBaseCU7.screenshots.at(-1).url)\n",
>   "title": "LASAL 창으로 복귀"
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
>   "code": "\nawait sky.click({window: lasalWinCU, x: 1055, y: 486, mouse_button: \"right\"});\nawait new Promise(res => setTimeout(res, 300));\nawait sky.click({window: lasalWinCU, x: 1126, y: 140});\nawait new Promise(res => setTimeout(res, 2500));\nvar recReloadDoneCU8 = await sky.get_window_state({window: lasalWinCU});\nawait nodeRepl.emitImage(recReloadDoneCU8.screenshots.at(-1).url)\n",
>   "title": "Recorder 클래스 다시 불러오기"
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
>   "code": "\nawait sky.click({window: lasalWinCU, x: 976, y: 485});\nawait new Promise(res => setTimeout(res, 500));\nvar recTreeCU9 = await sky.get_window_state({window: lasalWinCU});\nawait nodeRepl.emitImage(recTreeCU9.screenshots.at(-1).url)\n",
>   "title": "Recorder 메타데이터 항목 확인"
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

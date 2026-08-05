>
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:state.window});",
>   "title": "순서 조정 메뉴 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   }
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
>   "code": "await sky.click({window:targetWindow,x:903,y:257});",
>   "title": "CancelAxisZeroHome 한 단계 위로 이동"
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:state.window});",
>   "title": "CancelAxisZeroHome 순서 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   }
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
>   "code": "await sky.click({window:targetWindow,x:841,y:178,mouse_button:'right'});",
>   "title": "CancelAxisZeroHome 순서 조정 메뉴 재열기"
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
>   "code": "await sky.click({window:targetWindow,x:903,y:235});",
>   "title": "CancelAxisZeroHome를 Submit 다음으로 이동"
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:state.window});",
>   "title": "CancelAxisZeroHome 최종 순서 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch*'
>   }
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
>   "code": "await sky.press_key({window:targetWindow,key:'Control_L+Shift_L+s'});",
>   "title": "LASAL 선언 순서 Save All"
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
>   "code": "await sky.press_key({window:targetWindow,key:'Alt_L+F4'});",
>   "title": "LASAL IDE 종료"
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
>   "code": "var winListAfterClose = await sky.list_windows(); nodeRepl.write(winListAfterClose);",
>   "title": "LASAL 종료 대화상자 확인"
> }
> ```
>
> ```text
> [
>   {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 721818,
>     title: 'Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]'
>   },
>   {
>     app: 'OpenAI.Codex_2p2nqsd0c76g0!App',
>     id: 132684,
>     title: 'ChatGPT'
>   },
>   {
>     app: 'Chrome',
>     id: 133230,
>     title: 'ChatGPT Plugins | Browse and add plugins to ChatGPT - Chrome'
>   },
>   {
>     app: 'process:C:\\Windows\\explorer.exe',
>     id: 2887378,
>     title: 'architecture - 파일 탐색기 [ main ↓0 ↑17 ]'
>   },
>   {
>     app: 'process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe',
>     id: 133028,
>     title: '카카오톡'
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write({window:state.window});",
>   "title": "LASAL 종료 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   }
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
>   "code": "await sky.click({window:targetWindow,x:570,y:329});",
>   "title": "미사용 라이브러리 유지 후 LASAL 종료"
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

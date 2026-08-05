> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78762 chars=9977 sha256=ac32efd8e4a17f0b2df276fa0d531d0c49317ca4ea435eb1c50e006a8a6d13b0]'... 166023 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78771 chars=9977 sha256=0b52bcdfe6607843fbdd05b60ff514bfc12e8f5fa69c152b735cdfeda786314d]'... 159823 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
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
>   "code": "await sky.press_key({window:targetWindow,key:'Control_L+f'});",
>   "title": "LASAL 클래스 검색 열기"
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "검색 창 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78823 chars=9977 sha256=b99bc9b0c37d815858f308b6336373c96990f482be9fd93e23e5df1844508b73]'... 164667 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78832 chars=9977 sha256=3c67fa68017f5b739104b1aef5d9c3cf0034556d2b1f76e9debffd00f6dfa19b]'... 156583 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78841 chars=9977 sha256=de8c27cd4755f4530e5af50aed4bbe933cc0d6a03adf9c7640a7a842b9bf56ac]'... 20019 more characters,
>       originX: 663,
>       originY: 474,
>       width: 504,
>       height: 220
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.type_text({window:targetWindow,text:'CancelAxisZeroHome'});",
>   "title": "CancelAxisZeroHome 검색어 입력"
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
>   "code": "await sky.press_key({window:targetWindow,key:'Return'});",
>   "title": "CancelAxisZeroHome 검색 실행"
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "검색 결과 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78910 chars=9977 sha256=618d36320587471f14238bfa9dcca6bc4c47a92ca80bcfdb91233824b0f9dacc]'... 126259 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78919 chars=9977 sha256=0b52bcdfe6607843fbdd05b60ff514bfc12e8f5fa69c152b735cdfeda786314d]'... 121671 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
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
>   "code": "await sky.click({window:targetWindow,x:420,y:342});",
>   "title": "CancelAxisZeroHome 검색 결과 열기"
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
>   "code": "state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "CancelAxisZeroHome 위치 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - LMCEcatInputLatch'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78971 chars=9977 sha256=2d65661327a9f8e931176841996686e3c0b564dc8010d691f4e0ff25564d54da]'... 126719 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78980 chars=9977 sha256=0b52bcdfe6607843fbdd05b60ff514bfc12e8f5fa69c152b735cdfeda786314d]'... 119715 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
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

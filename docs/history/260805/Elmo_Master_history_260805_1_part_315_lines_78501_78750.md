>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78508 chars=9977 sha256=e9e0bc0fe6abc55e557355dca875bfcc7c91cfaa987561b0b58c05b27289e690]'... 57495 more characters,
>       originX: -1890,
>       originY: 27,
>       width: 960,
>       height: 502
>     },
>     {
>       id: 'screenshot-2',
>       zIndex: 2,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78517 chars=9977 sha256=5fe76a317c96552f89f1dc58665de69fd983ce8f8867ed54ae2cf5326c02d117]'... 134359 more characters,
>       originX: -1912,
>       originY: -3,
>       width: 1904,
>       height: 1016
>     },
>     {
>       id: 'screenshot-3',
>       zIndex: 3,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78526 chars=5896 sha256=f3420c766a0a1a9d8ad0dfcf1d16301bf84523414d5829691ce2aa9b2ca735cc]',
>       originX: -1542,
>       originY: 299,
>       width: 211,
>       height: 55
>     },
>     {
>       id: 'screenshot-4',
>       zIndex: 4,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78535 chars=5568 sha256=2251c9195f4ef840e0a40e3146912a784a927b10bdf70a58a5aedd4bf3444523]',
>       originX: -1542,
>       originY: 299,
>       width: 206,
>       height: 50
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
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.press_key({window:targetWindow,key:'Return'});",
>   "title": "LASAL 프로젝트 열기"
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
>   "code": "await new Promise(resolve => setTimeout(resolve,3000)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "LASAL 프로젝트 로딩 확인"
> }
> ```
>
> ```text
> {
>   window: {
>     app: 'process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>     id: 1839040,
>     title: 'Lasal Class 2 (02.03.001)'
>   },
>   screenshots: [
>     {
>       id: 'screenshot-0',
>       zIndex: 0,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78593 chars=9977 sha256=778c3ca01a23e482c12dd5c668b5109d7c49d52789da302d9a05559425004af4]'... 177851 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78602 chars=9977 sha256=3413df86d24af8fcaaa50a83cf5b8990769e114329b8411a14a59d2bfbd28a1c]'... 172139 more characters,
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
>   "code": "await new Promise(resolve => setTimeout(resolve,10000)); state = await sky.get_window_state({window:targetWindow}); nodeRepl.write(state);",
>   "title": "LASAL 프로젝트 준비 확인",
>   "timeout_ms": 30000
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78640 chars=9977 sha256=633c5bf3e0a274b2afa7bb00511561abac4b4af5f781dfe05e9e10346be80ee5]'... 159035 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78649 chars=9977 sha256=0b52bcdfe6607843fbdd05b60ff514bfc12e8f5fa69c152b735cdfeda786314d]'... 153091 more characters,
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
>   "code": "await sky.click({window:targetWindow,x:770,y:214});",
>   "title": "LASAL 클래스 트리 선택"
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
>   "title": "클래스 트리 선택 확인"
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
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78701 chars=9977 sha256=b9b79b1a3d7f9f9d779d786e14d10b10e9349e1ba49a60c924959d22368f153a]'... 165975 more characters,
>       originX: -1895,
>       originY: 22,
>       width: 970,
>       height: 512
>     },
>     {
>       id: 'screenshot-1',
>       zIndex: 1,
>       url: 'data:image/jpeg;base64,[BASE64_OMITTED source_line=78710 chars=9977 sha256=0b52bcdfe6607843fbdd05b60ff514bfc12e8f5fa69c152b735cdfeda786314d]'... 159811 more characters,
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
>   "code": "await sky.type_text({window:targetWindow,text:'LMCEcatInputLatch'});",
>   "title": "LMCEcatInputLatch 클래스 찾기"
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
>   "title": "클래스 검색 확인"
> }
> ```
>

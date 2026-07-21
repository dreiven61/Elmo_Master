> 								21 콘솔 트리 (selectable) EtherCAT Class State (ClassState) <-[]->
> 								22 콘솔 트리 (selectable) EtherCAT State (EtherCATState) <-[]->
> 								23 콘솔 트리 (selectable) EtherCAT Software Version (SoftwareVersion) <-[]->
> 								24 콘솔 트리 (selectable) EtherCAT Synchron (Synchron) <-[]->
> 								25 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 (FrameTimeTask0) <-[]->
> 								26 콘솔 트리 (selectable) EtherCAT Frame Time Task 0 Max (FrameTimeMaxTask0) <-[]->
> 								27 콘솔 트리 (selectable) EtherCAT Act Realtime Time (Act_RtTime) <-[]->
> 								28 콘솔 트리 (selectable) EtherCAT Min Realtime Time (Min_RtTime) <-[]->
> 								29 콘솔 트리 (selectable) EtherCAT Max Realtime Time (Max_RtTime) <-[]->
> 								30 콘솔 트리 (selectable) EtherCAT Missed Fr
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 타입 포커스 확인",
>   "code": "var ft=String(globalThis.state.accessibility?.tree||globalThis.state.accessibility?.document_text||''); var fm=ft.match(/The focused UI element is[^\\n]*/); nodeRepl.write(fm?fm[0]:'none');"
> }
> ```
>
> ```text
> The focused UI element is 316 custom Type.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 메타데이터 확인",
>   "code": "nodeRepl.write(Object.keys(globalThis.state));"
> }
> ```
>
> ```text
> [ 'window', 'screenshots', 'accessibility' ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 캡처 확인",
>   "code": "nodeRepl.write(globalThis.state.screenshots.map((x,i)=>({i,keys:Object.keys(x),w:x.width,h:x.height,mime:x.mimeType})));"
> }
> ```
>
> ```text
> []
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 창 영역 확인",
>   "code": "nodeRepl.write(globalThis.state.window);"
> }
> ```
>
> ```text
> {
>   app: '{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe',
>   id: 395936,
>   title: 'Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor'
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 속성 화면 캡처",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true}); globalThis.targetWindow=state.window; nodeRepl.write(globalThis.state.screenshots.map((x,i)=>({i,keys:Object.keys(x),w:x.width,h:x.height,mime:x.mimeType,dataType:typeof x.data})));"
> }
> ```
>
> ```text
> [
>   {
>     i: 0,
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     w: 1450,
>     h: 763,
>     mime: undefined,
>     dataType: 'undefined'
>   },
>   {
>     i: 1,
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     w: 1440,
>     h: 753,
>     mime: undefined,
>     dataType: 'undefined'
>   },
>   {
>     i: 2,
>     keys: [
>       'id',      'zIndex',
>       'url',     'originX',
>       'originY', 'width',
>       'height'
>     ],
>     w: 245,
>     h: 56,
>     mime: undefined,
>     dataType: 'undefined'
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 타입 필드 확대",
>   "code": "await nodeRepl.emitImage(globalThis.state.screenshots[2].url);"
> }
> ```
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch를 Object Channel로 설정",
>   "code": "for (var q=0;q<2;q++){globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; await sky.press_key({window:globalThis.targetWindow,key:'Down'}); globalThis.state=null;} globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; await sky.press_key({window:globalThis.targetWindow,key:'Return'}); globalThis.state=null; nodeRepl.write('Object Channel confirmed');"
> }
> ```
>
> ```text
> Object Channel confirmed
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 객체 채널 확인",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; var ap=String(state.accessibility?.tree||state.accessibility?.document_text||''); var aip=ap.lastIndexOf('custom Name'); nodeRepl.write(ap.slice(aip,aip+1000));"
> }
> ```
>
> ```text
> custom Name
> 					315 custom Class
> 					316 custom DataType
> 					317 custom Type
> 					318 custom Required
> 					319 custom DefaultInitValue
> 					320 custom Comment
> 				321 도구 모음 ID: 59392
> 					322 단추
> 					323 단추
> 			324 탭 항목 (selectable) Properties
> 			325 단추 Close
>
> The focused UI element is 307 TABLE Properties Window ID: 293314152.
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 클래스 필드 선택",
>   "code": "await sky.click({window:globalThis.targetWindow,element_index:315}); globalThis.state=null; nodeRepl.write('clicked Class');"
> }
> ```
>
> ```text
> clicked Class
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 클래스 목록 열기",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; await sky.press_key({window:globalThis.targetWindow,key:'Alt_L+Down'}); globalThis.state=null; nodeRepl.write('opened Class');"
> }
> ```
>
> ```text
> opened Class
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "InputLatch 클래스 항목 찾기",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:false,include_text:true}); globalThis.targetWindow=state.window; var cl=String(state.accessibility?.tree||state.accessibility?.document_text||''); var ci=cl.indexOf('LMCEcatInputLatch'); nodeRepl.write(cl.slice(Math.max(0,ci-1000),ci+1200));"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Hardware Editor Secondary Actions: Raise
> 		1 편집
> 		2 LIST
> 			3 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 				4 단추 위쪽 스크롤 화살표 ID: UpButton
> 				5 단추 페이지 위로 ID: UpPageButton
> 				6 위치 조정 위치 ID: ScrollbarThumb
> 				7 단추 아래쪽 스크롤 화살표 ID: DownButton
> 			8 목록 항목 (selectable) HWRtPostScan
> 			9 목록 항목 (selectable) I2CCDIAS
> 			10 목록 항목 (selectable) LMCDiagnosticsService
> 			11 목록 항목 (selectable) LMCEcatInputLatch
> 			12 목록 항목 (selectable) Merker
> 			13 목록 항목 (selectable) MerkerEx

> {
>   "title": "글로벌 메서드 목록 열기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1215,y:359,screenshotId:cuaState30.screenshots[0].id}); nodeRepl.write('expanded Global')"
> }
> ```
>
> ```text
> expanded Global
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "글로벌 메서드 상태 확인",
>   "code": "var cuaState31 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write((cuaState31.accessibility?.tree||'').match(/Methods[\\s\\S]*?Dependencies/)?.[0]||''); if(cuaState31.screenshots?.[0]) await nodeRepl.emitImage(cuaState31.screenshots[0].url)"
> }
> ```
>
> ```text
> Methods
> 							15861 콘솔 트리 (selectable) Global
> 							15862 콘솔 트리 (selectable) Private
> 						14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "프라이빗 메서드 목록 열기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1215,y:382,screenshotId:cuaState31.screenshots[0].id}); nodeRepl.write('expanded Private')"
> }
> ```
>
> ```text
> expanded Private
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "기존 프라이빗 메서드 확인",
>   "code": "var cuaState32 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write((cuaState32.accessibility?.tree||'').match(/Methods[\\s\\S]*?Dependencies/)?.[0]||''); if(cuaState32.screenshots?.[0]) await nodeRepl.emitImage(cuaState32.screenshots[0].url)"
> }
> ```
>
> ```text
> Methods
> 							15861 콘솔 트리 (selectable) Global
> 							15862 콘솔 트리 (selectable) Private
> 								17667 콘솔 트리 (selectable) HandleAdminCommands
> 						14714 콘솔 트리 (selectable) Dependencies
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
>   "title": "글로벌 메서드 생성 메뉴 열기",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1264,y:337,mouse_button:'right',screenshotId:cuaState32.screenshots[0].id}); nodeRepl.write('opened Global context menu')"
> }
> ```
>
> ```text
> opened Global context menu
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "글로벌 메서드 생성 옵션 확인",
>   "code": "var cuaState33 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState33.accessibility?.tree?.slice(-2500)); if(cuaState33.screenshots?.[0]) await nodeRepl.emitImage(cuaState33.screenshots[0].url)"
> }
> ```
>
> ```text
> tter ID: 126148032
> 		83 창 Splitter ID: 126146688
> 		84 Tab Output ID: 129283072
> 			85 창 ID: 1200
> 				86 창 ID: 1200
> 					87 LIST ID: 1201
> 						5932 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 							5933 단추 위쪽 스크롤 화살표 ID: UpButton
> 							5934 단추 페이지 위로 ID: UpPageButton
> 							5935 위치 조정 위치 ID: ScrollbarThumb
> 							5936 단추 아래쪽 스크롤 화살표 ID: DownButton
> 						6518 목록 항목 (selectable)
> 						6747 목록 항목 (selectable)
> 						6748 목록 항목 (selectable)
> 						6749 목록 항목 (selectable)
> 						6750 목록 항목 (selectable)
> 						6751 목록 항목 (selectable)
> 						6752 목록 항목 (selectable)
> 						6753 목록 항목 (selectable)
> 					88 스크롤 막대 ID: 59904
> 						89 단추 왼쪽 스크롤 화살표 ID: UpButton
> 						90 위치 조정 위치 ID: ScrollbarThumb
> 						91 단추 오른쪽 스크롤 화살표 ID: DownButton
> 			18926 탭 항목 (selectable) Python Script
> 			18927 탭 항목 (selectable) Debugger
> 			18928 탭 항목 (selectable) Output
> 			95 단추 Close
> 		96 창 Splitter ID: 126148200
> 		97 Tab Class View ID: 129287632
> 			98 트리 ID: 103
> 				6757 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 					6758 단추 위쪽 스크롤 화살표 ID: UpButton
> 					15860 단추 페이지 위로 ID: UpPageButton
> 					6759 위치 조정 위치 ID: ScrollbarThumb
> 					6760 단추 페이지 아래로 ID: DownPageButton
> 					6761 단추 아래쪽 스크롤 화살표 ID: DownButton
> 				6762 콘솔 트리 (selectable) Elmo_EtherCAT_Test_4Axis
> 					6763 콘솔 트리 (selectable) External
> 					6764 콘솔 트리 (selectable) Sigmatek
> 					6765 콘솔 트리 (selectable) _TCPIPServer_RT
> 					6766 콘솔 트리 (selectable) Elmo_1
> 					6767 콘솔 트리 (selectable) Elmo_2
> 					6768 콘솔 트리 (selectable) Elmo_3
> 					6769 콘솔 트리 (selectable) Elmo_4
> 					6770 콘솔 트리 (selectable) LMCControlCommandService
> 						14711 콘솔 트리 (selectable) Servers
> 						14712 콘솔 트리 (selectable) Clients
> 						14713 콘솔 트리 (selectable) Methods
> 							15861 콘솔 트리 (selectable) Global
> 							15862 콘솔 트리 (selectable) Private
> 								17667 콘솔 트리 (selectable) HandleAdminCommands
> 						14714 콘솔 트리 (selectable) Dependencies
> 					6771 콘솔 트리 (selectable) LMCDiagnosticsService
> 					6772 콘솔 트리 (selectable) LMCEcatInputLatch
> 					6773 콘솔 트리 (selectable) LMCRecorderStore
> 					6774 콘솔 트리 (selectable) LMCSdoExecutor
> 					6775 콘솔 트리 (selectable) TCPMotionInterface
> 			18929 탭 항목 (selectable) Lib
> 			18930 탭 항목 (selectable) File
> 			18931 탭 항목 (selectable) Global
> 			18932 탭 항목 (selectable) Net
> 			18933 탭 항목 (selectable) Class
> 			104 단추 Close
> 		105 Tab Properties ID: 129288544
> 			106 창 ID: 120748032
> 				107 TABLE Properties Window ID: 125710488
> 				108 도구 모음 ID: 59392
> 					109 단추
> 					110 단추
> 			18934 탭 항목 (selectable) Properties
> 			112 단추 Close
>
> The focused UI element is 98 트리 ID: 103.
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
>   "title": "HandleRequest 메서드 생성 시작",
>   "code": "await sky.click({window:cuaLasalWindow27,x:1338,y:345,screenshotId:cuaState33.screenshots[0].id}); nodeRepl.write('selected New Method')"
> }
> ```
>
> ```text
> selected New Method
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "새 메서드 대화상자 확인",
>   "code": "var cuaState34 = await sky.get_window_state({window:cuaLasalWindow27,include_screenshot:true,include_text:true}); nodeRepl.write(cuaState34.accessibility?.tree?.slice(-5000)); if(cuaState34.screenshots?.[0]) await nodeRepl.emitImage(cuaState34.screenshots[0].url)"
> }
> ```
>
> ```text
> 	8 텍스트 Offline
> 			9 텍스트
> 			10 텍스트 NUM
> 			11 텍스트
> 		12 창 xtpBarTop ID: 59419
> 			13 도구 모음 Edit
> 				19304 단추 Toggle bookmark
> 				19305 단추 (disabled) Previous bookmark
> 				19306 단추 (disabled) Next bookmark
> 				19307 단추 (disabled) Delete all bookmarks
> 				19308 단추 (disabled) Previous bookmark in this file
> 				19309 단추 (disabled) Next bookmark in this file
> 				19310 단추 (disabled) Comment selected text (Ctrl+Shift+C)
> 				19311 단추 (disabled) Remove comment (Ctrl+Shift+X)
> 				19312 단추 (disabled) Unindent (Shift+Tab)
> 				19313 단추 (disabled) Indent (Tab)
> 			24 도구 모음 Macros Manager
> 				19314 메뉴 항목 Macros
> 			26 도구 모음 Layout Manager
> 				19315 메뉴 항목 Layouts
> 			28 도구 모음 Toolbox
> 				19316 단추 DataAnalyzer
> 				19317 메뉴 항목 Toolbar Options
> 			31 도구 모음 Net Edit
> 				19318 단추 (disabled) Select
> 				19319 메뉴 항목 Toolbar Options
> 			34 도구 모음 Debug
> 				19320 단추 Go online (Alt+F6)
> 				19321 단추 Change Online Settings
> 				19322 메뉴 항목 Online Connection
> 				19323 단추 (disabled) Set Online Connection For Current Project
> 				19324 단추 (disabled) Download (F6)
> 				19325 단추 Build Changes, Online, Download and Run (CTRL+ALT+F6)
> 				19326 단추 (disabled) Download Module on the Fly
> 				19327 단추 (disabled) Save Project on PLC

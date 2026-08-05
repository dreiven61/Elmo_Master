>   mouse_button?: MouseButton; // Mouse button to click.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to click in.
>   x?: number; // Window-relative X coordinate.
>   y?: number; // Window-relative Y coordinate.
> };
>
> type PressKeyInput = {
>   key: string; // Key or `+`-separated key chord using X Window System keysym-style names, such as `a`, `space`, `Return`, `Tab`, `Control_L+a`, `Control_L+Shift_L+period`, or `KP_0`; whitespace around `+` is ignored, and common aliases such as `Control`, `Ctrl`, `Alt`, `Shift`, `period`, `greater`, and `Numpad_0` are accepted.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to receive the key press.
> };
>
> type TypeTextInput = {
>   text: string; // Text to type into the current focus.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to type into.
> };
>
> type ScrollInput = {
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   scrollX: number; // Horizontal scroll delta; negative means left, positive means right.
>   scrollY: number; // Vertical scroll delta; negative means up, positive means down.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to scroll.
>   x: number; // Window-relative X coordinate to scroll from.
>   y: number; // Window-relative Y coordinate to scroll from.
> };
>
> type SetValueInput = {
>   element_index: number; // Element index from the latest `get_window_state()` accessibility tree.
>   value: string; // Replacement value for the editable element.
>   window: Window; // Window object from `list_apps()` or `list_windows()` containing the editable element.
> };
>
> type DragInput = {
>   from_x: number; // Starting window-relative X coordinate.
>   from_y: number; // Starting window-relative Y coordinate.
>   screenshotId?: string; // Optional screenshot id from `get_window_state()`; when supplied, it must be cached for the target window.
>   to_x: number; // Ending window-relative X coordinate.
>   to_y: number; // Ending window-relative Y coordinate.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to drag in.
> };
>
> type PerformSecondaryActionInput = {
>   action: string; // Secondary action label from `get_window_state()`, such as `Raise`, `Scroll Up`, `Scroll Down`, `Scroll Left`, `Scroll Right`, `Expand`, or `Collapse`; matching is case-insensitive.
>   element_index: number; // Element index from the latest `get_window_state()` accessibility tree.
>   window: Window; // Window object from `list_apps()` or `list_windows()` containing the element.
> };
>
> type ActivateWindowInput = {
>   window: Window; // Window object from `list_apps()` or `list_windows()` to bring to the foreground.
> };
>
> type AppIdentifier = string;
>
> type AccessibilityState = {
>   document_text?: string; // Document text for the focused or most relevant document element when available.
>   focused_element?: string; // Formatted line for the focused element when available.
>   selected_elements?: Array<string>; // Formatted lines for selected elements when available.
>   selected_text?: string; // Text selected in the window when available.
>   tree: string; // Existing formatted accessibility tree text, including element indexes and tab hierarchy.
> };
>
> type Screenshot = {
>   height?: number; // Screenshot height in logical pixels, when available.
>   id: string; // Stable identifier for this screenshot within the latest window state.
>   originX?: number; // Screen X origin for this bounded screenshot region, when available.
>   originY?: number; // Screen Y origin for this bounded screenshot region, when available.
>   url: string; // Screenshot image as a data URL.
>   width?: number; // Screenshot width in logical pixels, when available.
>   zIndex: number; // Relative z-order for this screenshot; larger values are visually above smaller values.
> };
>
> type MouseButton = "left" | "right" | "middle" | "l" | "r" | "m";
> ```
> ````
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "List LASAL windows",
>   "code": "const wins=await sky.list_windows();nodeRepl.write(JSON.stringify(wins));"
> }
> ```
>
> ```text
> [{"app":"process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","id":3151338,"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network"},{"app":"process:C:\\Program Files\\WindowsApps\\OpenAI.Codex_26.721.11231.0_x64__2p2nqsd0c76g0\\app\\ChatGPT.exe","id":394022,"title":"ChatGPT"},{"app":"process:C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe","id":1247078,"title":"Daou Office - 메일 - Chrome"},{"app":"process:C:\\Windows\\explorer.exe","id":3742080,"title":"Elmo_EtherCAT_Test_4Axis - 파일 탐색기 [ main ↓0 ↑17 ]"},{"app":"process:C:\\Windows\\explorer.exe","id":2823732,"title":"26_07 - 파일 탐색기"},{"app":"process:C:\\Program Files\\Wireshark\\Wireshark.exe","id":3085686,"title":"Home Check_Read Legacy Elmo Health.pcapng"},{"app":"process:C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe","id":69144,"title":"카카오톡"}]
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Rebind LASAL main window",
>   "code": "const wins=await sky.list_windows();globalThis.targetWindow=wins.find(w=>String(w.title||\"\").includes(\"Lasal Class 2\"));globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true});nodeRepl.write(JSON.stringify({window:globalThis.state.window,focused:globalThis.state.accessibility?.focused_element,tree:(globalThis.state.accessibility?.tree||\"\").slice(0,4000)}));"
> }
> ```
>
> ```text
> Identifier 'wins' has already been declared
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
>   "title": "Rebind LASAL main window",
>   "code": "globalThis.windowList2=await sky.list_windows();globalThis.targetWindow=globalThis.windowList2.find(w=>String(w.title||\"\").includes(\"Lasal Class 2\"));globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true});nodeRepl.write(JSON.stringify({window:globalThis.state.window,focused:globalThis.state.accessibility?.focused_element,tree:(globalThis.state.accessibility?.tree||\"\").slice(0,4000)}));"
> }
> ```
>
> ```text
> {"window":{"app":"process:C:\\Program Files (x86)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe","id":3151338,"title":"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network"},"focused":"3685 창 ID: 59648","tree":"Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network\", App: Lasal2.exe.\n\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise\n\t\t163711 메뉴\n\t\t\t164624 메뉴\n\t\t163537 대화 상자 Find in Network Secondary Actions: Raise\n\t\t\t163538 텍스트 Find What:\n\t\t\t163539 콤보 상자 Find What: ID: 1115\n\t\t\t\t163540 편집 Find What: ID: 1001\n\t\t\t\t163541 단추 열기 ID: DropDown\n\t\t\t163542 확인란 Match Case ID: 1116\n\t\t\t163543 확인란 Whole Word ID: 1117\n\t\t\t163544 확인란 (disabled) Include Properties ID: 1776\n\t\t\t163545 확인란 (disabled) Show in Find Results ID: 1430\n\t\t\t163546 확인란 (disabled) Show in Find Results 2 ID: 1431\n\t\t\t163547 확인란 (disabled) Ignore Whitespaces ID: 1423\n\t\t\t163548 확인란 (disabled) Ignore Return ID: 1424\n\t\t\t163549 확인란 (disabled) Ignore Comments ID: 1799\n\t\t\t163550 확인란 (disabled) Use regular expression ID: 1774\n\t\t\t163551 그룹 Direction:\n\t\t\t163552 라디오 단추 (selectable) Previous ID: 1118\n\t\t\t163553 라디오 단추 (selectable) Next ID: 1119\n\t\t\t163554 단추 Find Next ID: 1120\n\t\t\t163555 단추 (disabled) Mark All ID: 1729\n\t\t\t163556 단추 (disabled) Clear all marks ID: 1730\n\t\t\t163557 단추 Close ID: 1\n\t\t\t163558 단추 Cancel ID: 2\n\t\t\t163559 제목 표시줄\n\t\t\t\t163560 메뉴 모음 시스템 ID: MenuBar\n\t\t\t\t\t163561 메뉴 항목 시스템\n\t\t\t\t163562 단추 닫기\n\t\t1 창 작업 영역 ID: 59648\n\t\t\t3684 창 Motion_Network Secondary Actions: Raise ID: 65282\n\t\t\t\t3685 창 ID: 59648\n\t\t\t\t\t3686 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t3687 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t3688 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t3689 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t3690 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t3691 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t3692 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t3693 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t3694 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t3695 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t3696 위치 조정 (disabled)\n\t\t\t136003 창 Comm_Network Secondary Actions: Raise ID: 65287\n\t\t\t\t136004 창 ID: 59648\n\t\t\t\t\t136005 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n\t\t\t\t\t\t136006 단추 위쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t137384 단추 페이지 위로 ID: UpPageButton\n\t\t\t\t\t\t136007 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t136008 단추 페이지 아래로 ID: DownPageButton\n\t\t\t\t\t\t136009 단추 아래쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t136010 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n\t\t\t\t\t\t136011 단추 왼쪽 스크롤 화살표 ID: UpButton\n\t\t\t\t\t\t136012 위치 조정 위치 ID: ScrollbarThumb\n\t\t\t\t\t\t136013 단추 페이지 오른쪽으로 ID: DownPageButton\n\t\t\t\t\t\t136014 단추 오른쪽 스크롤 화살표 ID: DownButton\n\t\t\t\t\t136015 위치 조정 (disabled)\n\t\t\t132746 창 Comm_Network.lcn Secondary Actions: Raise ID: 65286\n\t\t\t\t132747 창 ID: 59648\n\t\t\t\t\t132748 창 <?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?> <Network Name=\"Comm_Network\" Revision=\"0.0\"> <!-- List of Components in this network --> <Components> <Object Name = \"LMCControlCommandService1\" GUID = \"{5E164D6C-7E45-4BA4-B0F7-F9DBCCE8C71B}\" Class = \"LMCControlCommandService\" Position = \"(930,1380)\" Visualized = \"false\"> <Channels> <Server Name=\"ClassSvr\"/> <Client Name=\"LMCAxis1\"/> <Client Name=\"LMCAxis2\"/> <Client Name=\"LMCAxis3\"/> <Client Name=\"LMCAxis4\"/> <Client Name=\"LMCAxis5\"/> <Client Name=\"LMCAxis6\"/> <Client Name=\"LMCAxis7\"/> <Client Name=\"LMCAxis8\"/> <Client Name=\"LMCAxis9\"/> <Client Name=\"LMCRobot\"/> </Channels> </Object> <Object Name = \"LMCDiagnosticsService1\" GUID = \"{F42F0DD4-D9CC-4E5B-B073-F88FACAD14A8}\" Class = \"LMCDiagnosticsService\" Position = \"(870,900)\" Visualized = \"false\"> <Channels> <Server Name=\"ClassSvr\"/> <Server Name=\"DiagnosticsBootCounter\" Value=\"0\"/> <Client Name=\"InputLatch\"/> <Client Name=\"RecorderStore\"/> <Client Name=\"SdoAxis1\"/> <Client Name=\"SdoAxis2\"/> <Client Name=\"SdoAxis3\"/> <Client Name=\"SdoAxis4\"/> </Channels> </Object> <Object Name = \"TCPIPServer1\" GUID = \"{42E82217-EDCD-47A0-BF97-FCBD9C009436}\" Class = \"TCPIPServer\" Position = \"(870,180)\" Visualized = \"true\" Remotely = \"true\" CyclicTime = \"1 ms\" BackgroundTime = \"always\"> <Channels> <Server Name=\"Control\"/> <Server Name=\"ErrorCode\"/> <Server Name=\"SemaName01\"/> <Server Name=\"SemaName02\"/> <Server Name=\"TaskName\"/> <Client Name=\"_MultiTask\"/> <Client Name=\"_StdLib\"/> <Client Name=\"_TaskObjectControl\"/> <Cli"}
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
>   "title": "Find latch object in network",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.click({window:observation.window,element_index:163554});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({focused:globalThis.state.accessibility?.focused_element,selected:globalThis.state.accessibility?.selected_elements,tree:(globalThis.state.accessibility?.tree||\"\").slice(0,1400)}));}"
> }
> ```
>
> ```text
> point (3006, 495) is outside window bounds { originX: 0, originY: 0, width: 1450, height: 763 }
> ```
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect LASAL screenshot regions",
>   "code": "nodeRepl.write(JSON.stringify((globalThis.state?.screenshots||[]).map(s=>({id:s.id,w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))));"
> }
> ```
>
> ```text
> []
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
>   "title": "Observe LASAL dialog regions",
>   "code": "globalThis.state=await sky.get_window_state({window:globalThis.targetWindow,include_screenshot:true,include_text:true});nodeRepl.write(JSON.stringify({shots:globalThis.state.screenshots.map(s=>({id:s.id,w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex})),tree:(globalThis.state.accessibility?.tree||\"\").slice(0,900)}));"
> }
> ```
>
> ```text
> {"shots":[{"id":"screenshot-0","w":1450,"h":763,"x":-1880,"y":41,"z":0},{"id":"screenshot-1","w":1440,"h":753,"x":-1875,"y":46,"z":1},{"id":"screenshot-2","w":504,"h":220,"x":663,"y":474,"z":2},{"id":"screenshot-3","w":70,"h":34,"x":-1446,"y":123,"z":3}],"tree":"Window: \"Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network\", App: Lasal2.exe.\n\t0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - Motion_Network Secondary Actions: Raise\n\t\t163711 메뉴\n\t\t\t165308 메뉴\n\t\t163537 대화 상자 Find in Network Secondary Actions: Raise\n\t\t\t163538 텍스트 Find What:\n\t\t\t163539 콤보 상자 Find What: ID: 1115\n\t\t\t\t163540 편집 Find What: ID: 1001\n\t\t\t\t163541 단추 열기 ID: DropDown\n\t\t\t163542 확인란 Match Case ID: 1116\n\t\t\t163543 확인란 Whole Word ID: 1117\n\t\t\t163544 확인란 (disabled) Include Properties ID: 1776\n\t\t\t163545 확인란 (disabled) Show in Find Results ID: 1430\n\t\t\t163546 확인란 (disabled) Show in Find Results 2 ID: 1431\n\t\t\t163547 확인란 (disabled) Ignore Whitespaces ID: 1423\n\t\t\t163548 확인란 (disabled) Ignore Return ID: 1424\n\t\t\t163549 확인란 (disabled) Ignore Comments ID: 1799\n\t\t\t163550 확인란 (disabled) Use regular expression ID: 1774\n\t\t\t163551 그룹 Direction:\n\t\t\t163552 라디오 단추 (selectabl"}
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
>   "title": "Run network search",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Return\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({focused:globalThis.state.accessibility?.focused_element,shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex}))}));}"
> }
> ```
>
> ```text
> {"focused":"3685 창 ID: 59648","shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1},{"w":504,"h":220,"x":663,"y":474,"z":2}]}
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
>   "title": "Close LASAL network search dialog",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:\"Escape\"});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(JSON.stringify({focused:globalThis.state.accessibility?.focused_element,shots:globalThis.state.screenshots.map(s=>({w:s.width,h:s.height,x:s.originX,y:s.originY,z:s.zIndex})),selected:globalThis.state.accessibility?.selected_elements}));}"
> }
> ```
>
> ```text
> {"focused":"3685 창 ID: 59648","shots":[{"w":1450,"h":763,"x":-1880,"y":41,"z":0},{"w":1440,"h":753,"x":-1875,"y":46,"z":1}]}
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
>   "title": "Pan Motion network toward latch",
>   "code": "{const observation=globalThis.state;const sid=observation.screenshots?.[0]?.id;globalThis.state=null;await sky.scroll({window:observation.window,screenshotId:sid,x:500,y:300,scrollX:1200,scrollY:0});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:true});globalThis.targetWindow=globalThis.state.window;nodeRepl.write(String(globalThis.state.accessibility?.focused_element||\"\"));}"
> }
> ```
>
> ```text
> 3685 창 ID: 59648
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> MCP tool call
>

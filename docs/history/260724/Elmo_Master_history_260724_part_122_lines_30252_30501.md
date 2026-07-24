>   windows: Array<Window>; // Open windows owned by this app.
> };
>
> type LaunchAppInput = {
>   app: AppIdentifier; // App id returned by `list_apps()`, or an explicit `.exe` process path/identifier for apps that are not yet discoverable in `list_apps()`.
> };
>
> type GetWindowStateInput = {
>   include_screenshot?: boolean; // Whether to capture and display a screenshot of the window; defaults to true.
>   include_text?: boolean; // Whether to capture accessibility text describing visible elements and indexes; defaults to false.
>   window: Window; // Window object from `list_apps()` or `list_windows()` to capture.
> };
>
> type WindowState = {
>   accessibility: AccessibilityState | null; // Structured accessibility state when requested.
>   screenshots: Array<Screenshot>; // Bounded screenshots captured for the window and related transient UI.
>   window: Window; // Window captured by the state request.
> };
>
> type ClickInput = {
>   click_count?: number; // Number of clicks to perform.
>   element_index?: number; // Element index from the latest `get_window_state()` accessibility tree.
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
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "HandleRequest를 global method로 이동",
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;cuaState23=null;if(sid==null)throw new Error('no screenshot id');try{\n await sky.drag({window:obs.window,screenshotId:sid,from_x:1449,from_y:378,to_x:1413,to_y:318});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('HandleRequest drag requested');\n}catch(e){throw new Error('HandleRequest drag outcome unknown',{cause:e});}}"
> }
> ```
>
> ```text
> no screenshot id
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
>   "title": "HandleRequest 이동 위치 재확인",
>   "code": "cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:true}); cuaTargetWindow23=cuaState23.window; nodeRepl.write('drag positions refreshed');"
> }
> ```
>
> ```text
> drag positions refreshed
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
>   "title": "HandleRequest global folder 이동",
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;cuaState23=null;if(sid==null)throw new Error('no screenshot id');try{\n await sky.drag({window:obs.window,screenshotId:sid,from_x:1449,from_y:378,to_x:1410,to_y:318});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('HandleRequest drag complete');\n}catch(e){throw new Error('HandleRequest drag outcome unknown',{cause:e});}}"
> }
> ```
>
> ```text
> HandleRequest drag complete
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
>   "title": "Global access 값 전환",
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;cuaState23=null;if(sid==null)throw new Error('no screenshot id');try{\n await sky.click({window:obs.window,screenshotId:sid,x:1474,y:519,click_count:2});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('global access double click complete');\n}catch(e){throw new Error('global access double click unknown',{cause:e});}}"
> }
> ```
>
> ```text
> global access double click complete
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "HandleRequest global 이동 확인",
>   "code": "nodeRepl.write(cuaState23.accessibility.tree.split('\\n').filter(x=>/Global|Private|HandleRequest|HandleAdminCommands/.test(x)).slice(-20).join('\\n'));"
> }
> ```
>
> ```text
> 							231 콘솔 트리 (selectable) Global
> 							232 콘솔 트리 (selectable) Private
> 								233 콘솔 트리 (selectable) HandleAdminCommands
> 								234 콘솔 트리 (selectable) HandleRequest
> 			243 탭 항목 (selectable) Global
> 					252 custom Global access
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
>   "title": "Global method 폴더 선택",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n await sky.click({window:obs.window,element_index:231});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:false,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write(String(cuaState23.accessibility?.focused_element||''));\n}catch(e){throw new Error('global folder selection unknown',{cause:e});}}"
> }
> ```
>
> ```text
> 231 콘솔 트리 (selectable) Global
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Global method 메뉴 확인",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n await sky.press_key({window:obs.window,key:'Shift_L+F10'});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('global folder context requested');\n}catch(e){throw new Error('global folder context unknown',{cause:e});}}"
> }
> ```
>
> ```text
> global folder context requested
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
>   "title": "Global HandleRequest 생성",
>   "code": "{var obs=cuaState23;var sid=obs.screenshots?.[0]?.id;cuaState23=null;if(sid==null)throw new Error('no screenshot id');try{\n await sky.click({window:obs.window,screenshotId:sid,x:1418,y:342});\n cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n cuaTargetWindow23=cuaState23.window; nodeRepl.write('global new method selected');\n}catch(e){throw new Error('global new method outcome unknown',{cause:e});}}"
> }
> ```
>
> ```text
> global new method selected

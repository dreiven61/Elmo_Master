> 					253 콘솔 트리 (selectable) TCPMotionInterface
> 			254 탭 항목 (selectable) Lib
> 			255 탭 항목 (selectable) File
> 			256 탭 항목 (selectable) Global
> 			257 탭 항목 (selectable) Net
> 			258 탭 항목 (selectable) Class
> 			259 단추 Close
> 		260 Tab Properties ID: 302395640
> 			261 창 ID: 291858232
> 				262 TABLE Properties Window ID: 297584112
> 					263 스크롤 막대 세로 ID: NonClientVerticalScrollBar
> 						264 단추 위쪽 스크롤 화살표 ID: UpButton
> 						265 위치 조정 위치 ID: ScrollbarThumb
> 						266 단추 페이지 아래로 ID: DownPageButton
> 						267 단추 아래쪽 스크롤 화살표 ID: DownButton
> 					268 custom Name
> 					269 custom Revision
> 					270 custom GUID
> 					271 custom Task Settings
> 					272 custom Sigmatek
> 					273 custom OSInterface
> 					274 custom IconPath
> 					275 custom SharedCommandTable
> 					276 custom Objectsize
> 					277 custom Singleton
> 					278 custom Hidden
> 					279 custom Deprecated
> 					280 custom GCCOptions
> 					281 custom Comment
> 					282 custom Filename
> 				283 도구 모음 ID: 59392
> 					284 단추
> 					285 단추
> 			286 탭 항목 (selectable) Properties
> 			287 단추 Close
>
> The focused UI element is 238 트리 ID: 103.
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
>   "code": "var o8=globalThis.state;globalThis.state=null;await sky.perform_secondary_action({window:o8.window,element_index:248});globalThis.state=await sky.get_window_state({window:o8.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "RT latch 클래스 메뉴 열기"
> }
> ```
>
> ```text
> action is required
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "nodeRepl.write(await sky.documentation('api'))",
>   "title": "컴퓨터 제어 API 확인"
> }
> ```
>
> ````text
> ## API Reference
>
> # Sky Window2 API
>
> ## API Reference
>
> Use this as the supported `sky` window2 API surface.
>
> ```ts
> import { sky } from "@oai/sky";
>
> const apps = await sky.list_apps();
> const candidate_windows = apps.flatMap((app) => app.windows);
> // Choose the task-specific app and window before acting.
> // Each input action takes the specific Window for that action.
>
> interface Window2ComputerUseClient {
>   list_windows(): Promise<Array<Window>>; // List open windows that can be targeted by the window2 API.
>   get_window(input: GetWindowInput): Promise<Window>; // Rehydrate a currently open window by id; useful after losing a window binding.
>   list_apps(): Promise<Array<ListAppsApp>>; // List installed apps, including their currently open targetable windows when present.
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can later be selected from `list_apps()`.
>   get_window_state(input: GetWindowStateInput): Promise<WindowState>; // Capture selected state for an open window.
>   click(input: ClickInput): Promise<void>; // Click either an indexed element from the latest window state or a coordinate in the window.
>   press_key(input: PressKeyInput): Promise<void>; // Press a `+`-separated keyboard chord in a window.
>   type_text(input: TypeTextInput): Promise<void>; // Type text into the current focus in a window.
>   scroll(input: ScrollInput): Promise<void>; // Scroll by a delta from a specific coordinate in the window.
>   set_value(input: SetValueInput): Promise<void>; // Replace the value of an indexed editable element.
>   drag(input: DragInput): Promise<void>; // Drag from one window coordinate to another.
>   perform_secondary_action(input: PerformSecondaryActionInput): Promise<void>; // Invoke a secondary accessibility action on an indexed element.
>   activate_window(input: ActivateWindowInput): Promise<void>; // Optional escape hatch to bring an open window to the foreground; input methods activate their target window automatically.
>   target: "windows";
> }
>
> type Window = {
>   app: AppIdentifier; // App identifier for the app that owns this window; process-backed identifiers may include the full process path.
>   id: number; // Opaque identifier for the open window.
>   title?: string; // User-visible window title when available; may contain PII.
> };
>
> type GetWindowInput = {
>   app?: AppIdentifier; // Optional app identifier to carry forward from a previously returned `Window`.
>   id: number; // Opaque window identifier from a previously returned `Window`.
> };
>
> type ListAppsApp = {
>   displayName?: string; // User-visible app name when available.
>   id: AppIdentifier; // Canonical app id for the app that owns the windows.
>   isRunning?: boolean; // Whether the app currently appears to be running.
>   lastUsedDate?: string; // ISO 8601 timestamp for recent app usage when available.
>   useCount?: number; // Usage count signal when available.
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
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.state=await sky.get_window_state({window:targetWindow,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write('fresh');",
>   "title": "RT latch 메뉴 위치 갱신"
> }
> ```
>
> ```text
> fresh
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
>   "code": "var o9=globalThis.state;var sid9=o9.screenshots?.[0]?.id;if(!sid9)throw new Error('no screenshot');globalThis.state=null;await sky.click({window:o9.window,screenshotId:sid9,x:1531,y:342,mouse_button:'right'});globalThis.state=await sky.get_window_state({window:o9.window,include_screenshot:true,include_text:true});globalThis.targetWindow=state.window;nodeRepl.write(String(state.accessibility?.tree||state.accessibility?.document_text||''));",
>   "title": "RT latch 클래스 메뉴 열기"
> }
> ```
>
> ```text
> Window: "Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface", App: Lasal2.exe.
> 	0 창 Lasal Class 2 (02.03.001) - {Elmo_EtherCAT_Test_4Axis} - TCPMotionInterface Secondary Actions: Raise
> 		1 창
> 		2 창
> 		3 메뉴
> 			4 메뉴 항목 Edit Source Enter

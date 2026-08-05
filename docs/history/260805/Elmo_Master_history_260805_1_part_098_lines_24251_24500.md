> 					9228 콘솔 트리 (selectable) Elmo_1
> 					9229 콘솔 트리 (selectable) Elmo_2
> 					9230 콘솔 트리 (selectable) Elmo_3
> 					9231 콘솔 트리 (selectable) Elmo_4
> 					9232 콘솔 트리 (selectable) GL_9086_1
> 					9233 콘솔 트리 (selectable) GL_9086_1_Slot00
> 					9234 콘솔 트리 (selectable) GL_9086_1_Slot01
> 					9235 콘솔 트리 (selectable) LMCControlCommandService
> 					9236 콘솔 트리 (selectable) LMCDiagnosticsService
> 					9237 콘솔 트리 (selectable) LMCEcatInputLatch
> 					9238 콘솔 트리 (selectable) LMCRecorderStore
> 					9239 콘솔 트리 (selectable) LMCSdoExecutor
> 					9240 콘솔 트리 (selectable) TCPIPServer
> 					9241 콘솔 트리 (selectable) TCPMotionInterface
> 			17915 탭 항목 (selectable) Lib
> 			17916 탭 항목 (selectable) File
> 			17917 탭 항목 (selectable) Class
> 			17918 탭 항목 (selectable) Global
> 			88 단추 Close
> 		89 Tab Properties ID: 274930584
> 			90 창 ID: 266310816
> 				91 TABLE Properties Window ID: 272695160
> 				92 도구 모음 ID: 59392
> 					93 단추
> 					94 단추
> 			17919 탭 항목 (selectable) Properties
> 			96 단추 Close
> 		17920 메뉴 Edit
> 			17921 메뉴 항목 (disabled) Undo Ctrl+Z
> 			17922 메뉴 항목 (disabled) Redo Ctrl+Y
> 			17923 메뉴 항목 Cut Ctrl+X
> 			17924 메뉴 항목 Copy Ctrl+C
> 			17925 메뉴 항목 Paste Ctrl+V
> 			17926 메뉴 항목 Find... Ctrl+F
> 			17927 메뉴 항목 Find in Files... Ctrl+Shift+F
> 			17928 메뉴 항목 Find in Tree... Ctrl+Shift+T
> 			17929 메뉴 항목 Find Retentive Server... Ctrl+Shift+E
> 			17930 메뉴 항목 Replace... Ctrl+R
> 			17931 메뉴 항목 Replace in Files... Ctrl+Shift+R
> 			17932 메뉴 항목 (disabled) Goto Line... Ctrl+G
> 			17933 메뉴 항목 Reset Editor Colors
> 			17934 메뉴 항목 Mark
> 			17935 메뉴 항목 Bookmark
> 			17936 메뉴 항목 Navigate Backward Alt+Left Arrow
> 			17937 메뉴 항목 (disabled) Navigate Forward Alt+Right Arrow
> 			17938 메뉴 항목 Select All Ctrl+A
> 			17939 메뉴 항목 Comment
> 			17940 메뉴 항목 Dent
>
> The focused UI element is 9248 창 ID: 59648.
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
>   "title": "Class View로 복귀",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:'Escape'});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Edit menu closed')}"
> }
> ```
>
> ```text
> Edit menu closed
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
>   "title": "Class View에서 대상 클래스 찾기",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.scroll({window:observation.window,x:835,y:242,scrollX:0,scrollY:520});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Class View scrolled')}"
> }
> ```
>
> ```text
> Class View scrolled
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
>   "title": "Class View 포커스",
>   "code": "{const observation=globalThis.state;const screenshotId=observation?.screenshots?.[0]?.id;if(screenshotId==null)throw new Error('No screenshot id');globalThis.state=null;await sky.click({window:observation.window,screenshotId,x:760,y:261});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Class View focused')}"
> }
> ```
>
> ```text
> Class View focused
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
>   "title": "Class View 아래 항목으로 이동",
>   "code": "{const observation=globalThis.state;globalThis.state=null;await sky.press_key({window:observation.window,key:'End'});globalThis.state=await sky.get_window_state({window:observation.window,include_screenshot:true,include_text:false});globalThis.targetWindow=state.window;nodeRepl.write('Moved to last class')}"
> }
> ```
>
> ```text
> Moved to last class
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
>   "title": "Windows 입력 API 확인",
>   "code": "nodeRepl.write(await sky.documentation('api'))"
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
>   launch_app(input: LaunchAppInput): Promise<void>; // Launch an app by id so its window can be selected from `list_apps()`.
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

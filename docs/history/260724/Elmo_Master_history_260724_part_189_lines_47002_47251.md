>
> ```json
> {
>   "title": "Open LASAL class menu",
>   "code": "\nvar actionCtx = await globalThis.sky.perform_secondary_action({window:globalThis.targetWindow, x:1288, y:237});\nnodeRepl.write(JSON.stringify(actionCtx,null,2));\n"
> }
> ```
>
> ```text
> action is required
> ```
>
> <details><summary>Ran a command, called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Inspect computer controls",
>   "code": "nodeRepl.write({transportKeys:Object.keys(globalThis.sky.transport||{}), secondarySource:String(globalThis.sky.transport?.perform_secondary_action||\"\")});"
> }
> ```
>
> ```text
> {
>   transportKeys: [ 'nextRequestId', 'pendingData', 'pendingRequests', 'socket' ],
>   secondarySource: ''
> }
> ```
>
> - Ran `Get-Content -LiteralPath 'C:\Users\dreiv\.codex\plugins\cache\openai-bundled\computer-use\26.721.30844\skills\computer-use\SKILL.md'`
>
> </details>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Read computer-use API",
>   "code": "var apiDocNow = await globalThis.sky.documentation(\"api\"); nodeRepl.write(apiDocNow);"
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
> <details><summary>Called 3 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Open LASAL class menu",
>   "code": "\nawait globalThis.sky.click({window:globalThis.targetWindow,x:1288,y:237,mouse_button:\"right\",screenshotId:stateNow3.screenshots[0].id});\nvar menuState = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true});\nnodeRepl.write(menuState.accessibility?menuState.accessibility.tree:\"no accessibility\");\nfor (var s of menuState.screenshots||[]) await nodeRepl.emitImage(s.url);\n"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 47,211; original character count 1,046,058; UTF-8 byte count 1,048,606; UTF-8 SHA-256 f036f720d3bc949a0b4637be2cba3de94fff1962ad2afeb9c2bec4869408b6e0; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Add LASAL state variable",
>   "code": "\nawait globalThis.sky.click({window:globalThis.targetWindow,x:1350,y:587});\nawait new Promise(r=>setTimeout(r,250));\nvar varState1 = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true});\nnodeRepl.write(varState1.accessibility?.focused_element||varState1.accessibility?.tree||\"\");\nfor (var s of varState1.screenshots||[]) await nodeRepl.emitImage(s.url);\n"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 47,226; original character count 1,048,597; UTF-8 byte count 1,048,605; UTF-8 SHA-256 d66137c538a7ed16869763a4cf5ca9e2ef6b917440c35a7d07b72fdd01f897da; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Name LASAL state variable",
>   "code": "\nawait globalThis.sky.type_text({window:globalThis.targetWindow,text:\"GroupKinematicReady\"});\nawait globalThis.sky.press_key({window:globalThis.targetWindow,key:\"Return\"});\nawait new Promise(r=>setTimeout(r,250));\nvar varState2 = await globalThis.sky.get_window_state({window:globalThis.targetWindow,include_text:true});\nnodeRepl.write(varState2.accessibility?.selected_elements||varState2.accessibility?.tree||\"\");\nfor (var s of varState2.screenshots||[]) await nodeRepl.emitImage(s.url);\n"
> }
> ```
>
> ```text
> [OMITTED FROM SPLIT COPY: embedded computer-use image/tool-state payload at source line 47,241; original character count 1,046,140; UTF-8 byte count 1,048,604; UTF-8 SHA-256 8edb3a4cfc0df24496c34e5756eee177522bc4d2ebb890465b8aa7f7d76d7850; retained unchanged in docs/history/Elmo_Master_history_260724.md.]
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

>   "title": "포인터 설정 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});",
>   "timeout_ms": 30000
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
>   "title": "pRequest 포인터 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "timeout_ms": 30000
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
>   "title": "포인터 값 목록 열기",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:935,y:389});",
>   "timeout_ms": 30000
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
>   "title": "포인터 목록 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "timeout_ms": 30000
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
>   "title": "포인터 true 항목 선택",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Down'});",
>   "timeout_ms": 30000
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
>   "title": "포인터 true 확정",
>   "code": "await globalThis.sky.press_key({window:globalThis.targetWindow,key:'Return'});",
>   "timeout_ms": 30000
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
>   "title": "pRequest 포인터 최종 확인",
>   "code": "globalThis.state=await globalThis.sky.get_window_state({window:globalThis.targetWindow}); await nodeRepl.emitImage(globalThis.state.screenshots[0].url);",
>   "timeout_ms": 30000
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> <details><summary>Called 4 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "포인터 제어 API 확인",
>   "code": "nodeRepl.write(Object.keys(globalThis).filter(k=>/doc|api|guide|sky|state|target/i.test(k)));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [ 'sky', 'targetWindow', 'state' ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "컴퓨터 제어 API 확인",
>   "code": "var cuapi2=await import('file:///C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.727.51351/scripts/computer-use-client.mjs'); nodeRepl.write(Object.keys(cuapi2));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [ 'readDocumentation', 'setupComputerUseRuntime' ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "set_value 사용법 확인",
>   "code": "var apiDoc2=await cuapi2.readDocumentation('api'); nodeRepl.write(apiDoc2.includes('set_value')?apiDoc2.slice(Math.max(0,apiDoc2.indexOf('set_value')-800),apiDoc2.indexOf('set_value')+1800):apiDoc2.slice(0,2000));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> istAppsApp>>; // List installed apps, including their currently open targetable windows when present.
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
>   include_screenshot?: boolean; // Whether to capture
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "set_value 인자 확인",
>   "code": "var idxsv2=apiDoc2.indexOf('type SetValueInput'); nodeRepl.write(apiDoc2.slice(idxsv2,idxsv2+1200));",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
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
>   window: Window; // Window object from `list_apps()` or `list_windows()
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
>   "title": "포인터 값 선택 재시도",
>   "code": "await globalThis.sky.click({window:globalThis.targetWindow,x:855,y:390});",
>   "timeout_ms": 30000
> }
> ```
>
> ```text

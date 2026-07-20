>   },
>   {
>     "id": "C:\\Users\\dreiv\\eclipse\\java-2023-09\\eclipse\\eclipse.exe",
>     "windows": [],
>     "displayName": "Eclipse IDE for Java Developers - 2023-09",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   },
>   {
>     "id": "C:\\ProgramData\\MDS6\\MDS Installation Guide.pdf",
>     "windows": [],
>     "displayName": "MDS Installation Guide",
>     "lastUsedDate": "2026-04-03",
>     "useCount": 0,
>     "isRunning": false
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.launch_app({ app: String.raw`C:\\Users\\dreiv\\AppData\\Local\\Temp\\Codex_Elmo_WPF_Layout_Verify\\LasalMotionControlApiExample.exe` });\nfor (let attempt = 0; attempt < 10; attempt += 1) {\n  await new Promise((resolve) => setTimeout(resolve, 700));\n  globalThis.apps = await sky.list_apps();\n  globalThis.layoutTestMatches = apps.filter((candidate) =>\n    /Codex_Elmo_WPF_Layout_Verify|LasalMotionControlApiExample/i.test(`${candidate.id} ${candidate.displayName ?? \"\"}`)\n  );\n  if (layoutTestMatches.some((candidate) => candidate.windows?.length)) break;\n}\nnodeRepl.write(JSON.stringify(layoutTestMatches, null, 2));",
>   "title": "수정 화면 실행",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "id": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>     "windows": [
>       {
>         "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>         "id": 4003572,
>         "title": "LASAL Motion Control API Example"
>       }
>     ],
>     "displayName": "LasalMotionControlApiExample",
>     "lastUsedDate": "2026-07-13",
>     "useCount": 2,
>     "isRunning": true
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.windowsNow = await sky.list_windows();\nnodeRepl.write(JSON.stringify(windowsNow.filter((w) => /LASAL Motion Control API Example/i.test(w.title ?? \"\")), null, 2));",
>   "title": "테스트 창 찾기",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "app": "process:C:\\Users\\dreiv\\AppData\\Local\\Temp\\Codex_Elmo_WPF_Layout_Verify\\LasalMotionControlApiExample.exe",
>     "id": 5183710,
>     "title": "LASAL Motion Control API Example"
>   },
>   {
>     "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\Codex_LASAL_WPF\\PmasApiWpfTestApp\\bin\\Release\\LasalMotionControlApiExample.exe",
>     "id": 4003572,
>     "title": "LASAL Motion Control API Example"
>   }
> ]
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.targetWindow = await sky.get_window(windowsNow.find((w) => /Codex_Elmo_WPF_Layout_Verify/i.test(w.app)));\nawait sky.activate_window({ window: targetWindow });\nglobalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });\nglobalThis.state = await sky.get_window_state({ window: targetWindow });\nglobalThis.targetWindow = state.window;",
>   "title": "기본 화면 시각 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "globalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });\nglobalThis.state = await sky.get_window_state({ window: targetWindow });\nglobalThis.targetWindow = state.window;",
>   "title": "기본 화면 다시 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> window is minimized; call activate_window, refresh with get_window, then retry get_window_state
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await sky.activate_window({ window: targetWindow });\nglobalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });\nglobalThis.state = await sky.get_window_state({ window: targetWindow });\nglobalThis.targetWindow = state.window;",
>   "title": "테스트 창 복원",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> </details>
>
> 코드 변경과 빌드는 완료됐습니다. 새 빌드 창을 띄워 시각 확인하는 과정에서 사용자의 창 조작이 감지되어 포커스를 계속 빼앗지 않도록 잠시 멈췄습니다. 한 번만 다시 확인하고, 방해가 계속되면 정적 레이아웃·빌드 검증 결과로 마무리하겠습니다.
>
> <details><summary>Called 4 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await new Promise((resolve) => setTimeout(resolve, 2500));\nglobalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });\nawait sky.activate_window({ window: targetWindow });\nglobalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });\nglobalThis.state = await sky.get_window_state({ window: targetWindow });\nglobalThis.targetWindow = state.window;",
>   "title": "수정 화면 최종 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> user input was detected in this window; call get_window_state before continuing
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var cuApiDocs = await sky.documentation(\"api\");\nnodeRepl.write(cuApiDocs);",
>   "title": "테스트 창 정리 확인",
>   "timeout_ms": 30000
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

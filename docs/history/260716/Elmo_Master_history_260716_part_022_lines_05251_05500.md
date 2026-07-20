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
>   "code": "globalThis.apps = await sky.list_apps();\nnodeRepl.write(JSON.stringify(apps.filter(a => /lasal|class|sigmatek/i.test((a.id||\"\")+\" \"+(a.displayName||\"\")) || (a.windows||[]).some(w => /lasal|elmo|tcpmotion/i.test(w.title||\"\"))), null, 2));",
>   "title": "열린 LASAL IDE 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> [
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Elmo Motion Control\\Elmo Application Studio II\\ElmoMotionControl.View.Main.exe",
>     "windows": [
>       {
>         "app": "{6D809377-6AF0-444B-8957-A3773F02200E}\\Elmo Motion Control\\Elmo Application Studio II\\ElmoMotionControl.View.Main.exe",
>         "id": 1114286,
>         "title": "Elmo Application Studio II"
>       }
>     ],
>     "displayName": "Elmo Application Studio II 64bit",
>     "lastUsedDate": "2026-06-17",
>     "useCount": 0,
>     "isRunning": true
>   },
>   {
>     "id": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>     "windows": [
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 2429574,
>         "title": "네트워크 연결"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 8916380,
>         "title": "네트워크 및 공유 센터"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 4526070,
>         "title": "01.WaferTransferRobot - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 12652174,
>         "title": "Include - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 657502,
>         "title": "Lasal_PRG - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{0EA7AB48-AEFF-4173-EDE3-8E0ED90A6110}",
>         "id": 4394220,
>         "title": "Include - 파일 탐색기"
>       }
>     ],
>     "displayName": "OLAP용 Oracle Provider for OLE DB Readme",
>     "isRunning": true
>   },
>   {
>     "id": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>     "windows": [
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 2429574,
>         "title": "네트워크 연결"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 8916380,
>         "title": "네트워크 및 공유 센터"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 4526070,
>         "title": "01.WaferTransferRobot - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 12652174,
>         "title": "Include - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 657502,
>         "title": "Lasal_PRG - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3FCCECF0-812D-886E-B932-1CC0C8DA44CB}",
>         "id": 4394220,
>         "title": "Include - 파일 탐색기"
>       }
>     ],
>     "displayName": "Oracle Provider for OLE DB Readme",
>     "isRunning": true
>   },
>   {
>     "id": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>     "windows": [
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 2429574,
>         "title": "네트워크 연결"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 8916380,
>         "title": "네트워크 및 공유 센터"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 4526070,
>         "title": "01.WaferTransferRobot - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 12652174,
>         "title": "Include - 파일 탐색기"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 657502,
>         "title": "Lasal_PRG - 파일 탐색기 [ main ↓0 ↑0 ]"
>       },
>       {
>         "app": "Microsoft.AutoGenerated.{3B6E7DF3-19A3-70DB-96F6-86E65A17E61C}",
>         "id": 4394220,
>         "title": "Include - 파일 탐색기"
>       }
>     ],
>     "displayName": "Windows Software Development Kit",
>     "isRunning": true
>   },
>   {
>     "id": "{6D809377-6AF0-444B-8957-A3773F02200E}\\GitExtensions\\GitExtensions.exe",
>     "windows": [

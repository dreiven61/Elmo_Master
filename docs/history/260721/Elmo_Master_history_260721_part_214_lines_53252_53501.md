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
> </details>
>
> LASAL IDE 내부 클래스에는 새 멤버 `TriggerHealthOffset`가 아직 등록되지 않았습니다. 생성 영역을 외부 편집기로만 늘리는 방식은 Rebuild 시 다시 유실될 수 있으므로, 추가 클래스 멤버 없이 기존 `TriggerSignalId`에서 축 health offset을 RT 함수의 지역 변수로 계산하도록 바꾸겠습니다. 그러면 사용자 구현 영역만 IDE에 반영하면 되고 생성 선언과 내부 DB가 어긋나지 않습니다.
>
> <details><summary>Edited 2 files, called 5 tools</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+16 -21)
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LMC Recorder 편집기 포커스 확인",
>   "code": "globalThis.state = await sky.get_window_state({window: targetWindow, include_screenshot:true, include_text:true});\nglobalThis.targetWindow = state.window;\nnodeRepl.write(JSON.stringify({focused:state.accessibility?.focused_element, selectedLength:(state.accessibility?.selected_text||'').length, title:state.window.title},null,2));"
> }
> ```
>
> ```text
> foreground window did not report a process id
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 창 다시 선택",
>   "code": "globalThis.openWindows = await sky.list_windows();\nvar freshCandidates = openWindows.filter(w => w.id === 3084742 && /^Lasal Class 2/.test(w.title||''));\nnodeRepl.write(JSON.stringify(freshCandidates,null,2));\nif(freshCandidates.length!==1) throw new Error('Expected one LASAL window after refresh');\nglobalThis.targetWindow = await sky.get_window({id:freshCandidates[0].id, app:freshCandidates[0].app});\nawait sky.activate_window({window:targetWindow});"
> }
> ```
>
> ```text
> Expected one LASAL window after refresh
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "열린 LASAL 창 재확인",
>   "code": "globalThis.openWindows = await sky.list_windows(); nodeRepl.write(JSON.stringify(openWindows.filter(w => /Lasal Class 2/.test(w.title||'')),null,2));"
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
>   "title": "LASAL 앱 상태 재확인",
>   "code": "globalThis.apps = await sky.list_apps(); nodeRepl.write(JSON.stringify(apps.filter(a=>/Lasal2\\.exe/i.test(a.id)).map(a=>({id:a.id,windows:a.windows})),null,2));"
> }
> ```
>
> ```text
> [
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
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
>   "title": "LASAL 창 재탐색",
>   "code": "await new Promise(r=>setTimeout(r,2000)); globalThis.apps = await sky.list_apps(); nodeRepl.write(JSON.stringify(apps.filter(a=>/Lasal2\\.exe/i.test(a.id)).map(a=>({id:a.id,windows:a.windows})),null,2));"
> }
> ```
>
> ```text
> [
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
>   },
>   {
>     "id": "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\\Sigmatek\\Lasal_0094\\Class2\\Bin\\Lasal2.exe",
>     "windows": []
>   }
> ]
> ```
>
> </details>
>
> lasal을 편집할때 Class 생성 및 Network 편집만 IDE에서 진행하고 나머지는 외부 편집기에서 편집하고 수정해. 편집하는데 시간이 너무 많이 소요되는거같다. 불필요한 작업이 너무 많아지는거 같아.
>
> 맞습니다. 방금 과정은 과했습니다. 앞으로 LASAL IDE는 **Class 생성·선언 구조 변경과 Network 편집**에만 쓰고, 구현 로직 수정·검토·테스트는 외부 편집기에서 진행하겠습니다.
>
> 이번 변경도 새 Class/Network 작업은 없으므로 IDE 재동기화·검색 스모크를 더 진행하지 않겠습니다. 추가 클래스 멤버가 필요 없도록 구현을 정리했고, 현재 외부 `.st` 소스와 정적 계약을 기준으로 전체 테스트·diff 확인 후 커밋하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+25 -20)
>
> </details>
>
> IDE 작업은 여기서 중단했습니다. 현재 기준은 외부 `.st` 소스이며, 추가 멤버 없이 구현 영역만으로 정리했습니다. 이제 PC API 101개 테스트, LASAL source/network 정적 계약, WPF Debug·Release 빌드를 최종 상태에서 다시 확인합니다.
>

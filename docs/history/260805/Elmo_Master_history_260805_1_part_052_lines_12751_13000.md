>
> 반영하겠습니다. 오늘(8월 3일) 17:30 이후부터 내일(8월 4일) 08:30 이전까지는 LASAL IDE를 제가 직접 제어해도 되는 것으로 적용하겠습니다. 그 시간 밖에는 다시 사용자에게 부탁하겠습니다. Windows 앱 제어가 필요한 단계부터 `computer-use` 스킬을 사용하되, 현재 정적 분석과 소스 편집은 계속 병행합니다.
>
> 평일 전체에 반영해도 된다.
>
> 평일 반복 규칙으로 적용하겠습니다. 월–금 17:30부터 다음 날 08:30 직전까지는 제가 LASAL IDE를 직접 제어하고, 그 밖의 시간에는 IDE 입력을 사용자에게 부탁하겠습니다. 금요일 17:30에 시작한 작업은 토요일 08:30 전까지 같은 야간 작업 창으로 보겠습니다. 현재는 17:30까지 정적 설계를 마무리한 뒤 직접 IDE 작업으로 전환하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Windows 제어 준비",
>   "code": "if (!globalThis.sky) { var { setupComputerUseRuntime } = await import('C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.727.51351/scripts/computer-use-client.mjs'); await setupComputerUseRuntime({ globals: globalThis }); } var guidanceDoc = await sky.documentation('guidance'); var confirmationsDoc = await sky.documentation('confirmations'); nodeRepl.write(guidanceDoc + '\\n---CONFIRMATIONS---\\n' + confirmationsDoc);",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> ## node_repl + @oai/sky
>
> - Use `node_repl` JavaScript for all Computer Use actions.
> - The `node_repl` state persists across calls. Store cross-cell values on `globalThis`; top-level `const` and `let` names cannot be redeclared by later retries.
> - For text output, call `nodeRepl.write(...)` with a string. Use `JSON.stringify(...)` for objects.
>
> ## Workflow
>
> ### Initialize target selection
>
> Run the Initialize setup cell from `SKILL.md` first. Then list apps and choose the target from returned app and window objects:
>
> ```js
> globalThis.apps = await sky.list_apps();
> nodeRepl.write(JSON.stringify(apps, null, 2));
> ```
>
> Never reconstruct an app or window from guessed fields. Do not call `get_window`, `activate_window`, or any input method until selection has produced exactly one returned window.
>
> ```js
> {
>   function escapeRegExp(value) {
>     return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
>   }
>
>   function returnedWindowSummary(window) {
>     return {
>       id: window.id,
>       app: window.app,
>       title: window.title,
>     };
>   }
>
>   function requireUniqueWindow(windows, label) {
>     if (windows.length !== 1) {
>       nodeRepl.write(
>         `Returned candidate windows:\n${JSON.stringify(windows.map(returnedWindowSummary), null, 2)}`,
>       );
>       throw new Error(`Expected exactly one target window for ${label}; found ${windows.length}`);
>     }
>     return windows[0];
>   }
>
>   globalThis.apps = await sky.list_apps();
>   globalThis.targetApp = apps.find((app) => app.id === "<app id>");
>   if (!targetApp) throw new Error("Target app was not returned by list_apps");
>   const targetAppId = targetApp.id;
>
>   if (targetApp.windows.length === 0) {
>     await sky.launch_app({ app: targetApp.id });
>     globalThis.apps = await sky.list_apps();
>     globalThis.targetApp = apps.find((app) => app.id === targetAppId);
>   }
>   if (!targetApp?.windows.length) {
>     throw new Error("Target app did not expose a window after launch");
>   }
>
>   const windowTitleHint = "<optional exact window title>";
>   const candidateWindows =
>     windowTitleHint === "<optional exact window title>"
>       ? targetApp.windows
>       : targetApp.windows.filter((window) =>
>           new RegExp(`^${escapeRegExp(windowTitleHint)}$`, "i").test(window.title ?? ""),
>         );
>   const returnedWindow = requireUniqueWindow(candidateWindows, targetApp.id);
>
>   globalThis.targetWindow = await sky.get_window({
>     id: returnedWindow.id,
>     app: returnedWindow.app,
>   });
>   await sky.activate_window({ window: targetWindow });
>   globalThis.state = await sky.get_window_state({ window: targetWindow });
>   globalThis.targetWindow = state.window;
> }
> ```
>
> Use `list_windows()` when inspecting currently open windows or recovering a known running app. If the intended app is absent from `list_apps`, launch it with an explicit `.exe` path or `.exe` process identifier, refresh `list_apps()` or `list_windows()`, filter to the intended returned windows, and stop unless the filtered list has exactly one window. Escape Windows path backslashes in JavaScript strings, for example `await sky.launch_app({ app: "C:\\Users\\me\\build\\MyApp.exe" });`.
>
> ### Act and refresh
>
> Use a two-cell loop for state-derived inputs: observe and stop, inspect the result, then perform exactly one action and refresh immediately. Element indexes, screenshot IDs, and coordinates are valid only for the observation that produced them. Interleaving or retry requires re-observation.
>
> Accessibility path, cell 1: observe and inspect.
>
> ```js
> globalThis.state = await sky.get_window_state({
>   window: targetWindow,
>   include_screenshot: false,
>   include_text: true,
> });
> globalThis.targetWindow = state.window;
> nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));
> ```
>
> Stop here and inspect the emitted tree before choosing an index.
>
> Accessibility path, cell 2: one action and refresh.
>
> ```js
> {
>   const observation = globalThis.state;
>   if (observation?.accessibility == null) {
>     throw new Error("No accessibility observation; reobserve before acting");
>   }
>   const elementIndex = 12; // Replace with one index from the printed accessibility tree.
>   globalThis.state = null;
>   try {
>     await sky.click({ window: observation.window, element_index: elementIndex });
>     globalThis.state = await sky.get_window_state({
>       window: observation.window,
>       include_screenshot: true,
>       include_text: true,
>     });
>   } catch (error) {
>     throw new Error("Input or refresh outcome is unknown; reobserve before retrying", {
>       cause: error,
>     });
>   }
>   globalThis.targetWindow = state.window;
>   nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));
> }
> ```
>
> Coordinate path, cell 1: observe and inspect.
>
> ```js
> globalThis.state = await sky.get_window_state({
>   window: targetWindow,
>   include_screenshot: true,
>   include_text: false,
> });
> globalThis.targetWindow = state.window;
> nodeRepl.write("Inspect the displayed screenshot, then run the coordinate action cell.");
> ```
>
> Coordinate path, cell 2: one action and refresh.
>
> ```js
> {
>   const observation = globalThis.state;
>   if (observation == null) {
>     throw new Error("No screenshot observation; reobserve before acting");
>   }
>   const screenshotId = observation.screenshots?.[0]?.id;
>   if (screenshotId == null) {
>     throw new Error("No screenshotId was returned by the latest screenshot observation");
>   }
>   globalThis.state = null;
>   try {
>     await sky.click({ window: observation.window, screenshotId, x: 420, y: 260 });
>     globalThis.state = await sky.get_window_state({
>       window: observation.window,
>       include_screenshot: true,
>       include_text: true,
>     });
>   } catch (error) {
>     throw new Error("Input or refresh outcome is unknown; reobserve before retrying", {
>       cause: error,
>     });
>   }
>   globalThis.targetWindow = state.window;
>   nodeRepl.write(String(state.accessibility?.tree || state.accessibility?.document_text || ""));
> }
> ```
>
> For typing, observe focus first and stop. After confirming focus is correct, type in a separate cell and refresh. If typing or refresh fails, the outcome is unknown; reobserve before retrying.
>
> Focus observation cell:
>
> ```js
> {
>   globalThis.state = await sky.get_window_state({
>     window: targetWindow,
>     include_screenshot: true,
>     include_text: true,
>   });
>   globalThis.targetWindow = state.window;
>   nodeRepl.write(String(state.accessibility?.focused_element || ""));
> }
> ```
>
> Typing action cell:
>
> ```js
> {
>   const observation = globalThis.state;
>   if (observation?.accessibility?.focused_element == null) {
>     throw new Error("No focused element observation; reobserve before typing");
>   }
>   globalThis.state = null;
>   try {
>     await sky.type_text({ window: observation.window, text: "<text>" });
>     globalThis.state = await sky.get_window_state({
>       window: observation.window,
>       include_screenshot: true,
>       include_text: true,
>     });
>   } catch (error) {
>     throw new Error("Text input or refresh outcome is unknown; reobserve before retrying", {
>       cause: error,
>     });
>   }
>   globalThis.targetWindow = state.window;
> }
> ```
>
> ## Reading screenshots
>
> Screenshots returned by `get_window_state` are displayed automatically. Inspect them directly and use the returned screenshot ID for coordinate actions. Do not decode, save, print, emit, or inspect screenshot payloads again solely for inspection.
>
> ## Guidelines
>
> - Treat `get_window_state` as an expensive point-in-time snapshot. Capture a new state when you need to verify progress or when focus, layout, modality, or element indexes may have changed.
> - Element indexes are valid only for the accessibility state that produced them. Refresh accessibility state after any action that may change the visible element tree.
> - By default, `get_window_state({ window })` captures and automatically displays a screenshot, and returns `accessibility: null`. This is the best default for desktop apps with weak accessibility trees.
> - If you need accessibility text or element indexes, call `get_window_state({ window, include_screenshot: false, include_text: true })`. Request both only when you truly need both the screenshot and accessibility text for the next decision.
> - Important accessibility context is also extracted as structured fields: `focused_element`, `selected_text`, `selected_elements`, and `document_text`.
> - If an input call reports that the point is over a non-target window, call `sky.activate_window({ window: state.window })`, refresh screenshot-backed state, and retry the intended input once with the refreshed `state.window`.
> - If you expect a modal in the target app but `get_window_state` does not show it, call `sky.list_windows()` to find the modal or owned secondary window, then capture that returned window with `sky.get_window_state(...)`.
> - `type_text` sends literal text. Re-check focus immediately before `type_text`; use `press_key` for controls such as `Enter`, `Tab`, arrows, Escape, and keyboard chords instead of embedding control characters in a typed string.
> - Prefer X Window System keysym-style names for key input, especially `KP_0` through `KP_9` for apps that distinguish numpad keys from the number row. Common aliases such as `period`, `greater`, `less`, `comma`, `slash`, `question`, `Numpad_0`, `Numpad_Add`, `Numpad_Subtract`, `Numpad_Multiply`, `Numpad_Divide`, `Numpad_Decimal`, and `Numpad_Enter` are also supported. For shifted punctuation shortcuts, include `Shift`, for example `Control_L+Shift_L+period` for Ctrl+Shift+`.` / `>`.
> - `scroll` scrolls with input injection from a specific window-relative coordinate. Use `sky.scroll({ window, x, y, scrollX: 0, scrollY: 600 })` to scroll down from `(x, y)`. Negative `scrollY` scrolls up; negative `scrollX` scrolls left. Do not pass `element_index` to `scroll`; if a specific pane needs focus, click it first with coordinates, then scroll from inside that pane.
> - Use keyboard navigation when it is faster than hunting UI pixels.
> - For text entry into a document, slide, sheet, editor, or canvas, foreground process metadata and window title are not enough. Click a stable point or element inside the observed editable work surface, refresh to verify focus, then type. If the requested text is not visible after a refresh, refocus the editable surface and retry.
> - For drawing or handwriting or canvas or 3D viewport manipulation tasks, use `drag` strokes directly on the canvas.
> - Prefer Browser Use plugin for browser automation.
>
> ## Non-negotiable Windows Automation Safety
>
> These denies are mandatory. Confirmation policy applies only to allowed-but-confirmed actions and cannot replace these denies.

>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Click API 확인",
>   "code": "nodeRepl.write(String(sky.click));"
> }
> ```
>
> ```text
> (...args) => {
>         globals.nodeRepl?.setResponseMeta({
>           [TOOL_SURFACE_META_KEY]: {
>             kind: "computerUse",
>             app: getComputerUseAppReference(args[0]),
>           },
>         });
>         return Reflect.apply(value, target, args);
>       }
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
>   "title": "새 Object client 메뉴 열기",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n  await sky.click({window:obs.window,x:1408,y:178,button:'right'});\n  cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n  cuaTargetWindow23=cuaState23.window;\n  nodeRepl.write('context menu opened');\n}catch(e){cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:true}); throw e;}}"
> }
> ```
>
> ```text
> context menu opened
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
>   "title": "Clients 메뉴 열기",
>   "code": "{var obs=cuaState23;cuaState23=null;try{\n  await sky.perform_secondary_action({window:obs.window,target:219});\n  cuaState23=await sky.get_window_state({window:obs.window,include_screenshot:true,include_text:true});\n  cuaTargetWindow23=cuaState23.window;\n  nodeRepl.write('secondary action requested');\n}catch(e){cuaState23=await sky.get_window_state({window:cuaTargetWindow23,include_screenshot:true,include_text:true}); nodeRepl.write({error:String(e)});}}"
> }
> ```
>
> ```text
> { error: 'TypeError: action is required' }
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
>   "title": "Secondary action 문서 확인",
>   "code": "nodeRepl.write({docType:typeof sky.documentation});"
> }
> ```
>
> ```text
> { docType: 'function' }
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
>   "title": "Computer-use 동작 규칙 확인",
>   "code": "nodeRepl.write(await sky.documentation('guidance'));"
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
>     for (let attempt = 0; attempt < 10; attempt++) {
>       await new Promise((resolve) => setTimeout(resolve, 1000));
>       globalThis.apps = await sky.list_apps();
>       globalThis.targetApp = apps.find((app) => app.id === targetAppId);
>       if (targetApp?.windows.length) break;
>     }
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
> Use `list_windows()` when inspecting currently open windows or recovering a known running app. If the intended app is absent from `list_apps`, launch it with an explicit `.exe` path or `.exe` process identifier, poll `list_apps()` or `list_windows()`, filter to the intended returned windows, and stop unless the filtered list has exactly one window. Escape Windows path backslashes in JavaScript strings, for example `await sky.launch_app({ app: "C:\\Users\\me\\build\\MyApp.exe" });`.
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

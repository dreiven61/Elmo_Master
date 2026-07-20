- `28abbda` 최종 DLL/EXE 배포물 갱신

남은 환경 문제는 프로젝트 compiler C78과 설치 라이브러리 C81 불일치, 설치 폴더의 `DriveComL2.h` 누락입니다. 프로젝트 Rebuild는 성공하지만 SIGMATEK 설치 세트는 동일 compiler 세대로 복구하는 것이 맞습니다.

> 이거 예제프로그램이 쓰는게 너무 어려운거 같다. 이거말고 예전에 만들어둔 Codex_Lasal_WPF를 개선해서 예제프로그램을 만들도록 하자.
>
> 일단 기존에 예제프로그램을 테스트용으로 사용할 수도 있으니 기존에 소스코드는 냅두고 내가 LMC_Library에 프로젝트를 복사해놨어. C:\work\Elmo\Elmo_Master\LMC_Library\Codex_LASAL_WPF 여기로 옮겨놨고, 여기 구현된 더미 코드를 우리가 구현한 API를 이용해서 실제로 동작할 수 있는 예제 프로그램을 만들자.
>
> 다만 여기서 구현안된 탭들은 오해의 소지를 불러 일으킬수 있으니 구현 안된 API에 대한 내용들은 전체 다 없애도록해. 그럼 설계 및 구현을 진행해봐

<details><summary>17 previous messages</summary>

> 복사본 `LMC_Library/Codex_LASAL_WPF`만 수정하고, 기존 `Codex_LASAL_WPF`와 방금 만든 `LasalMotionControlLibTestApp`은 보존하겠습니다. 먼저 복사본의 탭·더미 통신·프로젝트 참조 구조를 현재 18개 활성 API와 대조한 뒤, 미구현 기능은 UI와 코드에서 제거하고 실제 API 연결·안전 동작까지 구현하겠습니다.
>
> 분석 결과, 복사본은 실제 API를 전혀 참조하지 않고 오래된 64-byte/INT64 더미 프레임과 잘못된 command ID까지 포함하고 있었습니다. 새 예제는 구조를 과감히 줄여 `Connection + Single Axis + Group State + Log`만 남기고, 실제 활성 API 18개 외의 탭·버튼·더미 서비스·coverage 코드를 복사본에서 제거하는 설계로 구현합니다.
>
> <details><summary>Created 7 files, edited 5 files, deleted 11 files</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+450 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+1447 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp.sln` (+28 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj` (+86 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp.sln` (+0 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/PmasApiWpfTestApp.csproj` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/Properties/AssemblyInfo.cs` (+5 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md` (+29 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+75 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+41 -0)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.AxisOperations.cs` (+0 -321)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.Coverage.cs` (+0 -59)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestGroupOperations.cs` (+0 -1026)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.CycleTestOperations.cs` (+0 -1869)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.GroupOperations.cs` (+0 -252)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.PiBulkOperations.cs` (+0 -114)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.RecorderOperations.cs` (+0 -85)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/Models/ApiCoverageItem.cs` (+0 -11)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/PmasControllerContext.cs` (+0 -415)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SigmatekTcpIpDummyMMCLib.cs` (+0 -1940)
> - Deleted `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/Services/SimpleXlsxExporter.cs` (+0 -276)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+7 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+2 -2)
>
> </details>
>
> 새 UI가 실제 창에서 잘리는 부분이나 잘못 활성화되는 버튼이 없는지 확인하기 위해 Windows 앱 제어 스킬로 WPF를 직접 실행해 시각 검증하겠습니다. 이 검증은 코드 동작 범위를 늘리지 않고, 방금 만든 복사본의 레이아웃과 초기 안전 상태만 확인합니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Prepare Windows UI inspection",
>   "code": "if (!globalThis.sky) {\n  var computerUseModule = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.707.61608/scripts/computer-use-client.mjs\");\n  await computerUseModule.setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidanceDoc = await sky.documentation(\"guidance\");\nvar confirmationDoc = await sky.documentation(\"confirmations\");\nnodeRepl.write(guidanceDoc);\nnodeRepl.write(confirmationDoc);"
> }
> ```
>
> ````text
> ## Troubleshooting
>
> IMPORTANT: do NOT attempt to dig through source code or control Windows apps through unrelated mechanisms before attempting this workflow. If you run into issues, follow the steps below FIRST.
>
> - Do not fall back to PowerShell, shell scripts, SendKeys, or other foreground keyboard/mouse automation just because those tools are visible. Read and attempt this workflow first.
> - If `js_reset` is visible but `js` is not, do not conclude that `node_repl` is unusable. Use tool discovery for `node_repl js`, then `mcp__node_repl__js`, then `js`, then `node_repl js JavaScript execution`; run the bootstrap cell with the Node REPL `js` tool once it is exposed.
> - If the Node REPL `js` execution tool is still unavailable after those searches, say that explicitly before choosing any fallback Windows-control path.
> - If `node_repl` is not available, say that explicitly before choosing any fallback Windows-control path.
>
> On the first Computer Use task in a session, try a lightweight call after bootstrap:
>
> ```js
> globalThis.apps = await sky.list_apps();
> nodeRepl.write(JSON.stringify(apps, null, 2));
> ```
>
> Any non-error response means the Windows helper is reachable. If `list_apps`, `list_windows`, or another lightweight request times out, wait 2 seconds and retry the same lightweight call once. If the retry succeeds, continue from the returned apps.
>
> If bootstrap reports `Module not found: @oai/sky`, report the exact setup error and plugin path. Do not add `<plugin root>/node_modules` or try to import `@oai/sky` directly from the JavaScript session.
>
> If Computer Use reports that the turn ended, that the user stopped Computer Use, or that it is unavailable for the current turn, stop the task and report that Computer Use was stopped or became unavailable. Do not fall back to foreground keyboard/mouse automation such as PowerShell `SendKeys`.
>
> If the same lightweight call times out again, do not keep issuing app input. Reset the JavaScript session if available, rerun the bootstrap cell, and retry `list_apps` once. If it still times out or reports helper communication failure, stop and report that the Windows Computer Use helper may have crashed.
>
> If the intended app is present but has no suitable open window, call `await sky.launch_app({ app: targetApp.id })`, then poll `list_apps()` until that app exposes a targetable window. If the intended app is not yet discoverable in `list_apps()` call `await sky.launch_app({ app: "C:\\path\\to\\YourApp.exe" })` or use an equivalent `.exe` process identifier, then poll `list_apps()` or `list_windows()` for the new targetable window. Do not open or navigate the Windows Start menu/Search UI to launch apps. Do not continue while a launcher, splash screen, modal, or permission prompt is blocking the app's workspace.
>
> ## Runtime Behavior
>
> - Computer Use commands run through the Node REPL `js` tool. Do not look for a separate computer-use-specific JavaScript tool.
> - Reuse the existing `sky`, `apps`, `targetApp`, `targetWindow`, and `state` bindings across cells. If `targetWindow` already exists, keep using it until a stale handle, activation failure, or missing window error requires recovery.
> - Store cross-cell values on `globalThis`. The JavaScript session is persistent: top-level `const` and `let` names cannot be redeclared by later retries. Do not declare retry-prone scratch names such as `tree`, `lines`, `state`, or `accessibility` at top level. Use `globalThis` for state you need later, and wrap temporary parsing code in a short `{ ... }` block or use fresh names for one-off retries.
> - On the first cell, initialize `sky`, list installed apps, and print the returned app objects. Each app includes its currently open targetable windows.
> - Choose one app from the latest `apps` array. If it has exactly one suitable open window, call `get_window` on that returned window before the first snapshot. This is the Computer Use equivalent of resolving the chosen target into the current canonical object.
> - For app-control tasks, call `activate_window({ window: targetWindow })` once after selecting the target and before the first snapshot. Activation is idempotent, and restores minimized windows. Skip this only when the task is explicitly passive inspection of multiple windows without stealing focus.
> - Use `list_windows` as a shortcut only when the task is explicitly about currently open windows or when recovering after you already know the app is running.
> - After `get_window_state`, replace `targetWindow` with `state.window`; it is the canonical window object that was actually captured.
> - If bindings still exist after a stale handle error, recover with `sky.get_window({ id: targetWindow.id, app: targetWindow.app })`. If bindings are gone after a reset, call `list_apps` again and choose from the fresh returned objects. Do not reconstruct a window from guessed ids.
>
> ### First Computer Use Cell
>
> ```js
> if (!globalThis.sky) {
>   const { setupComputerUseRuntime } = await import("<plugin root>/scripts/computer-use-client.mjs");
>   await setupComputerUseRuntime({ globals: globalThis });
> }
> globalThis.apps = await sky.list_apps();
> nodeRepl.write(JSON.stringify(apps, null, 2));
> ```
>
> After that, keep using the existing `targetApp` and `targetWindow` bindings. Do not alternate between `targetWindow`, `window`, `taskWindow`, `targetWindowId`, and `targetWindowApp` across retries.
>
> GOOD: choose one returned app, then choose one of its returned windows:
>
> ```js
> globalThis.targetAppMatches = apps.filter((candidate) =>
>   /replace-with-app-name-or-id/i.test(`${candidate.id} ${candidate.displayName ?? ""}`),
> );
> if (targetAppMatches.length !== 1) {
>   nodeRepl.write(JSON.stringify(targetAppMatches.length ? targetAppMatches : apps, null, 2));
>   throw new Error("Expected exactly one matching app; refresh apps or refine the pattern");
> }
>
> globalThis.targetApp = targetAppMatches[0];
> if (targetApp.windows.length !== 1) {
>   nodeRepl.write(JSON.stringify(targetApp.windows, null, 2));
>   throw new Error(
>     "Expected exactly one target window; call launch_app or refine the window choice",
>   );
> }
>
> globalThis.targetWindow = await sky.get_window(targetApp.windows[0]);
> await sky.activate_window({ window: targetWindow });
> globalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });
> globalThis.state = await sky.get_window_state({ window: targetWindow });
> globalThis.targetWindow = state.window;
> ```
>
> GOOD: if the chosen app is installed but has no returned window yet, launch it by id and poll `list_apps()` for its window:
>
> ```js
> await sky.launch_app({ app: targetApp.id });
> for (let attempt = 0; attempt < 10; attempt += 1) {
>   await new Promise((resolve) => setTimeout(resolve, 1000));
>   globalThis.apps = await sky.list_apps();
>   globalThis.targetApp = apps.find((candidate) => candidate.id === targetApp.id);
>   if (targetApp?.windows?.length) break;
> }
> if (!targetApp?.windows?.length) {
>   const label = targetAppMatches[0].displayName ?? targetAppMatches[0].id;
>   throw new Error(`Launched ${label}, but no targetable window appeared`);
> }
> globalThis.targetWindow = await sky.get_window(targetApp.windows[0]);
> ```
>
> GOOD: if the app is a local `.exe` build and is not returned by `list_apps()` yet, launch it by `.exe` path and poll for the resulting window:
>
> ```js
> await sky.launch_app({ app: String.raw`C:\work\MyApp\bin\Debug\MyApp.exe` });
> for (let attempt = 0; attempt < 10; attempt += 1) {
>   await new Promise((resolve) => setTimeout(resolve, 1000));
>   globalThis.apps = await sky.list_apps();
>   globalThis.targetAppMatches = apps.filter((candidate) =>
>     /MyApp(?:\.exe)?/i.test(`${candidate.id} ${candidate.displayName ?? ""}`),
>   );
>   if (targetAppMatches.some((candidate) => candidate.windows?.length)) break;
> }
> globalThis.targetApp = targetAppMatches.find((candidate) => candidate.windows?.length);
> if (!targetApp?.windows?.length) {
>   globalThis.windows = await sky.list_windows();
>   nodeRepl.write(JSON.stringify({ apps: targetAppMatches, windows }, null, 2));
>   throw new Error("Launched MyApp.exe, but no targetable window appeared");
> }
> globalThis.targetWindow = await sky.get_window(targetApp.windows[0]);
> ```
>
> GOOD: if the app has multiple windows, choose from that app's returned windows:
>
> ```js
> globalThis.targetWindowMatches = targetApp.windows.filter((candidate) =>
>   /replace-with-window-title/i.test(candidate.title ?? ""),
> );
> if (targetWindowMatches.length !== 1) {
>   nodeRepl.write(
>     JSON.stringify(targetWindowMatches.length ? targetWindowMatches : targetApp.windows, null, 2),
>   );
>   throw new Error("Expected exactly one matching window; refine the title pattern");
> }
>
> globalThis.targetWindow = await sky.get_window(targetWindowMatches[0]);
> await sky.activate_window({ window: targetWindow });
> globalThis.targetWindow = await sky.get_window({ id: targetWindow.id, app: targetWindow.app });
> ```
>
> GOOD: request accessibility text only when it will drive the next action, then narrow it in JS before printing:
>
> ```js
> {
>   const snapshotState = await sky.get_window_state({
>     window: targetWindow,
>     include_screenshot: false,
>     include_text: true,
>   });
>   globalThis.state = snapshotState;
>   globalThis.targetWindow = snapshotState.window;
> }
> ```
>
> GOOD: when `include_text: true` returns a large tree, print the structured critical fields first, then filter the indexed element tree:
>
> ```js
> {
>   const snapshotAccessibility = state.accessibility;
>   if (!snapshotAccessibility) {
>     throw new Error("No accessibility state returned");
>   }
>   const pattern = /replace-with-relevant-labels-or-words/i;
>   const treeLines = snapshotAccessibility.tree.split("\n");
>   const candidates = treeLines.filter((text) => pattern.test(text)).slice(0, 80);
>   const criticalContext = {
>     focused_element: snapshotAccessibility.focused_element,
>     selected_text: snapshotAccessibility.selected_text,
>     selected_elements: snapshotAccessibility.selected_elements,
>     document_text: snapshotAccessibility.document_text,
>   };
>
>   nodeRepl.write(
>     [
>       JSON.stringify(criticalContext, null, 2),
>       "Candidate elements:",
>       ...(candidates.length ? candidates : treeLines.slice(0, 80)),
>     ].join("\n"),
>   );
> }
> ```
>
> BAD: guessing or reconstructing a window instead of using one returned by `list_apps`, `list_windows`, `get_window`, or `get_window_state`:
>
> ```js
> await sky.click({ window: { id: 123456, app: "example.exe" }, x: 400, y: 300 });
> ```
>
> GOOD: batch related actions against the selected window, then verify once:
>
> ```js
> await sky.click({ window: targetWindow, x: 400, y: 300 }); // replace with stable window-relative coordinates
> await sky.type_text({ window: targetWindow, text: "hello" });
> await sky.press_key({ window: targetWindow, key: "Return" });
>
> globalThis.state = await sky.get_window_state({ window: targetWindow });
> globalThis.targetWindow = state.window;
> ```
>

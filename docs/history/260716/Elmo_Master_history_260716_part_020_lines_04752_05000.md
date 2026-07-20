   - 현재 생성 테이블에는 `SimulateMode=0`이며 `SWMinPos=0`, `SWMaxPos=0`, `AEmergency=0`이 보입니다. [ONE_Motion_Network_Table.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st:1254)
   - 이 `0` 값의 실제 의미를 확인하고, UNIT·소프트웨어 리미트·비상감속·하드웨어 리미트·E-Stop을 확정한 다음 움직여야 합니다.

실제 시험 순서는 이렇게 진행하는 것이 맞습니다.

1. RPC Init → Callback 등록 ACK → Close
2. 축 1~4와 그룹 object lookup
3. `AxisInfo`, `GetGroupMembersInfo`
4. `ReadStatus`, `ReadPosition`
5. Reset → PowerOn → Stop
6. 무부하·저속·짧은 거리의 `MoveRelative`
7. `MoveAbsolute`
8. `MoveVelocity` 후 즉시 Stop 검증
9. GroupEnable → GroupReadStatus → GroupDisable
10. 패킷 재캡처, disconnect/reconnect, queue 및 cycle jitter 확인

현재 23개 PC API 경로 중 18개는 실제 PLC 시험 대상이고, 아래 5개는 LASAL에서 의도적으로 `-5`를 반환합니다.

- GroupReset
- GroupStop
- MoveLinearAbsoluteEx
- GroupReadActualPosition
- SetKinTransformCartesian4Axis

현재 범위를 “5개 미지원이 명시된 부분 릴리스”로 잡는다면 위 5개 구현은 시험 시작의 차단 항목이 아닙니다. 전체 23개 완성을 목표로 하면 이 5개 LASAL 구현과 실제 UDP callback event sender, multi-PC ownership까지 추가로 남습니다.

시험 완료 후에는 패킷/PLC 결과 문서화, 사용자 매뉴얼 본문 작성, 확정 커밋 기준 Release DLL 재빌드, 버전·SHA-256 기록과 배포 패키지 재생성이 남습니다.

> LasalMotionControlLibTestApp 요 테스트 프로그램을 써서 테스트 할껀데 api 반영이 제대로 됐는지 체크해보자. 그리고 lasal 최신 lib로 업데이트 했는데 git에 반영이 좀 이상한데 gitignore가 제대로 반영됐는지 이거 확인해서 gitignore 반영하고 정리해서 커밋도 완료해
>
> User context
>
> Attachments:
> - .gitignore: `C:/Users/dreiv/source/repos/SIGMATEK/project_repos/WTR/JC_Solution/01.WaferTransferRobot/.gitignore`

<details><summary>29 previous messages</summary>

> `LasalMotionControlLibTestApp`가 현재 공개 API와 실제로 맞는지 빌드·호출 경로까지 점검하고, 최신 LASAL 라이브러리 동기화로 생긴 변경을 사람이 수정한 코드와 생성물로 분리하겠습니다. 제공한 WTR `.gitignore`는 참고 기준으로 비교하되 이 저장소 구조에 맞게 적용하고, 검증 후 목적별 커밋으로 정리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 루트 `.gitignore`는 WTR 예시보다 보수적으로 잘 구성돼 있습니다. WTR 규칙의 `*.lcb`, `Network/*/*.st`, `ConfigObjects.st`까지 그대로 가져오면 이 프로젝트의 필수 등록·네트워크 파일을 숨길 수 있으므로, 실제 추적 파일과 최신 라이브러리 동기화 결과를 기준으로 필요한 항목만 선별하겠습니다.
>
> 테스트 앱은 라이브러리 소스를 직접 참조하고 23개 호출 경로를 모두 갖고 있어 API 시그니처 반영 자체는 정상입니다. 다만 실장비 시험 기준으로는 두 가지를 고쳐야 합니다. 현재 `8388608 counts/rev` 고정 변환은 PLC의 engineering `UNIT` 계약과 다르고, LASAL에서 `-5`로 막아 둔 5개 명령이 정상 기능처럼 보입니다. 이 두 부분을 시험 전에 안전하게 수정하겠습니다.
>
> 최신 라이브러리 동기화는 단순 생성물 노이즈가 아닙니다. 58개 vendor source가 설치본과 정확히 일치하고 `SafetyRoutingTables`가 실제 프로젝트 의존성으로 추가됐습니다. 반면 현재 설치는 MotionLib이 요구하는 `DriveComL2.h`가 Hardware에 없어 시작 시 `E0015`가 계속 나며, 동기화 이후 Rebuild 기록도 없습니다. 커밋 전에 열린 LASAL IDE에서 최종 Rebuild와 검색 smoke를 직접 확인하기 위해 Windows 앱 제어 절차를 사용하겠습니다.
>
> <details><summary>Called 13 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "if (!globalThis.sky) {\n  var computerUseModule = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.707.61608/scripts/computer-use-client.mjs\");\n  await computerUseModule.setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidanceDoc = await sky.documentation(\"guidance\");\nnodeRepl.write(guidanceDoc);",
>   "title": "LASAL IDE 제어 준비",
>   "timeout_ms": 30000
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

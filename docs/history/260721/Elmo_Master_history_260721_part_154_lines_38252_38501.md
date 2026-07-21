- 내부 테스트 완료 전 고객용 DLL 갱신: 하지 않음
- 내부 개발 소스: Git 커밋으로 관리해야 함
- 고객 산출물 별도 버전 저장소: 선택 사항

복잡한 배포 버전 시스템은 없어도 됩니다. 다만 DLL과 PLC 프로그램의 조합을 나중에 식별할 수 있도록 배포 시 다음 정도는 남기는 것을 권장합니다.

- DLL 파일 버전
- PLC 프로젝트 빌드 기준
- Protocol schema/build
- Git commit hash
- 배포 날짜

현재 진단 구현 파일들은 아직 수정·신규 파일 상태로 커밋되지 않았습니다. 고객 배포와 별개로, 내부 구현은 기능 단위로 커밋해야 안전합니다. 또한 [README.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/README.md:21)의 D0-only/53개 테스트 설명과 [API_MAPPING.md](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/API_MAPPING.md:37)의 Recorder 명령 매핑도 현재 구현과 맞지 않아 정리가 필요합니다.

> API 구현 완료 시켜주고, 테스트 할 수 있게 예제 프로그램도 완성시켜줘. 그 이후에 테스트 진행해볼께

<details><summary>30 previous messages</summary>

> 현재 기준에서 “완료”는 고객 배포가 아니라 개발 소스 기준으로 잡겠습니다. PC 공개 API, LASAL 백엔드, WPF 테스트 앱을 함께 마무리하고 자동 검증까지 통과시키되, 실제 PLC 다운로드·실기 동작만 사용자 테스트 항목으로 남기겠습니다. 먼저 설계 문서와 LASAL 규칙을 다시 대조해 구현 범위를 확정하겠습니다.
>
> 설계 문서를 다시 읽은 결과, 테스트 전 반드시 활성화해야 하는 1차 범위는 D1 Health/Catalog/PI, D2 Bulk, D3 manual Recorder입니다. D4 Trigger/Double Buffer와 D5 Write/SDO는 설계상 D1~D3 실기 검증 뒤 단계지만, 공개 API와 예제 흐름은 이번에 끝까지 준비하고 위험한 Write는 기본 차단 상태를 유지하겠습니다. 지금부터 기존 89개 계약 테스트를 기준선으로 다시 실행하고 구현 변경을 병합하겠습니다.
>
> C# 확장 작업 중 작업자가 종료되어 현재 저장된 변경을 제가 인계받았습니다. 코드가 절반만 반영된 상태일 수 있으므로, 먼저 빌드와 계약 테스트로 끊어진 지점을 찾고 보완한 뒤 WPF와 PLC 계약을 맞추겠습니다.
>
> PC API와 WPF는 이제 계약상 완료된 상태입니다. 문서에서 아직 D0-only·53개 테스트로 적힌 부분이 남아 있어, 실제 구현과 맞도록 현재 상태·패킷 맵·사용자 PLC 시험 절차를 함께 갱신하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+31 -21)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+103 -18)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+27 -24)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> PC API와 개발용 WPF 예제는 구현이 끝났고, 지금은 LASAL 쪽 메타데이터 재생성·빌드 가능 여부와 문서/검증 기준을 맞추는 마무리 단계입니다. 고객 배포 폴더는 건드리지 않고, 내부 시험 가능한 D1~D3와 아직 차단된 D4/D5 경계를 명확히 남기겠습니다.
>
> LASAL IDE 저장·재빌드와 `Find in Implementation` 스모크 검증은 소스 파일만으로 확인할 수 없어서, Windows 앱 제어 절차를 사용하겠습니다. 이 절차가 IDE 내부 클래스 메타데이터와 실제 프로젝트 상태를 맞추는 데 필요합니다.
>
> <details><summary>Called 20 tools</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL IDE 제어 준비",
>   "code": "if (!globalThis.sky) {\n    const { setupComputerUseRuntime } = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.715.52143/scripts/computer-use-client.mjs\");\n    await setupComputerUseRuntime({ globals: globalThis });\n  }\n  var cuaGuidance = await sky.documentation(\"guidance\");\n  var cuaConfirmations = await sky.documentation(\"confirmations\");\n  nodeRepl.write(cuaGuidance + \"\\n\\n=== CONFIRMATIONS ===\\n\" + cuaConfirmations);"
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

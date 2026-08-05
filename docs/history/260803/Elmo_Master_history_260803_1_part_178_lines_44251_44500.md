>
> 병렬로 SDK 계약과 WPF journal core를 분리해 구현 중입니다. 저는 LASAL 변경 규칙 3개 문서를 먼저 전부 확인한 뒤, generated 선언은 건드리지 않고 기존 `0x7D12` dormant parser의 길이·offset만 새 계약에 맞출 수 있는지 검토하겠습니다. 신규 retentive store/`0x7D14` route는 IDE 구조 변경 없이는 추가하지 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md` (+108 -22)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+71 -11)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+20 -14)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+13 -6)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -2)
>
> </details>
>
> LASAL 쪽은 새 구조를 억지로 만들지 않고 기존 dormant parser만 56-byte 계약으로 갱신됐습니다. bit 3/5는 그대로 OFF이고 valid 요청도 `InvalidState/detail 10`, native call 0회입니다. 정적 verifier negative fixture는 12개에서 28개로 늘었고 SourceOnly/full 모두 현재 Axis1 SDO 설정에 맞춰 통과했습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+10 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+8 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+17 -7)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -3)
> - Wrote `./docs/architecture/AXIS_SET_POSITION_BOUNDED_COORDINATE_CORRECTION_2026-07-31.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+9 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -1)
>
> </details>
>
> 현재 SDK 쪽은 recovery key/모델과 parser가 생성됐고 facade·회귀시험을 마무리하는 중입니다. WPF journal은 재시작 시 남은 `ArmedBeforeDispatch`를 `RecoveryRequired`로 durable 승격하도록 보완했고, 독립 컴파일 기준 신규 9/9가 통과했습니다. 전체 빌드는 SDK 파일 생성이 끝나는 즉시 이어서 돌리겠습니다.
>
> 이거 테스트할때마다 Home안돼어있어서 MoveReference를 해야되는데 Home하는 기능이 필요할 꺼 같다. Elmo에서 Home 할수 있는 API를 구현을 진행하고 반영해야 될꺼같다. MMC_Home은 MoveReference를 수행하고 DS402 Home은 DS402를 이용한 Home기능을 구현해야 될꺼같아. 그리고 드라이브가 꺼졌다 켜지면 현재 테스트용으로 멀티턴리셋을 날려야 되는데 이건 Test용 항목으로 따로 빼서 테스트용 기능으로 멀티턴 리셋을 SDO로 날려서 쓸수있게 만들어야될꺼같아. 이 내용들 구현하면서 반영해
>
> 새 요구를 기존 작업에 추가하겠습니다. 다만 세 기능은 반드시 분리해서 구현해야 합니다.
>
> - `MMC_Home`: LASAL `_LMCAxis.MoveReference()` 기반의 LASAL-native reference
> - `DS402 Home`: DS402 mode 6, controlword/statusword와 homing parameter를 쓰는 별도 state machine
> - `Test Multiturn Reset`: 장비 시험 전용 SDO 기능으로 격리
>
> 특히 멀티턴 리셋 SDO index/value는 현재 확인되지 않은 값을 추정해서 보내면 안 됩니다. 로컬 Elmo 매뉴얼·기존 캡처·소스에서 정확한 object/subindex/type/value를 먼저 확정하고, 못 찾으면 UI/전송 gate는 OFF로 둔 채 계약만 구현하겠습니다. 진행 중인 SetPosition retained-outcome slice를 닫으면서 Home/멀티턴 근거 감사를 병렬로 시작합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 기존 `SetPosition` 변경을 먼저 충돌 없이 마무리하면서, 동시에 Home 경로를 소스와 Elmo 문서 기준으로 분리 검증하고 있습니다. `MMC_Home`, `DS402 Home`, 시험용 멀티턴 리셋은 서로 다른 기능·안전 조건으로 구현하겠습니다. 멀티턴 리셋 SDO 주소/값은 근거가 확인되기 전에는 절대 추정해서 활성화하지 않겠습니다.
>
> 기존 히스토리에서 멀티턴 관련 근거가 확인됐습니다. 다만 서로 다른 명령이 섞여 있습니다: `0x20FC:02`는 EnDat 오류 리셋 후보이고, `0x20FC:01` 및 일부 `TW[...]` 명령은 멀티턴 위치 자체를 초기화할 수 있어 위험합니다. 현재 장비에서 실제로 쓰는 시험 명령을 소스·ESI·기존 캡처와 대조한 뒤 “시험 전용”으로만 노출하겠습니다.
>
> DS402 Home은 현재 코드만 한 줄 추가해서 끝나는 기능이 아닙니다. 현재 Drive PDO에는 `Controlword/Statusword`만 활성화돼 있고 `0x6060/0x6061` 모드 채널은 비활성입니다. 그래서 LASAL 쪽에 Home 상태기계와 드라이브 소유권 연결을 추가해야 합니다. 이 구조 변경은 저장소 규칙상 LASAL IDE에서 해야 하므로, 열린 LASAL IDE를 안전하게 조작하기 위해 Windows 앱 제어 스킬을 사용하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Windows 앱 제어 초기화",
>   "code": "if (!globalThis.sky) {\n  const { setupComputerUseRuntime } = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.721.81911/scripts/computer-use-client.mjs\");\n  await setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidance = await sky.documentation(\"guidance\");\nvar confirmations = await sky.documentation(\"confirmations\");\nnodeRepl.write(guidance + \"\\n---CONFIRMATIONS---\\n\" + confirmations);"
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

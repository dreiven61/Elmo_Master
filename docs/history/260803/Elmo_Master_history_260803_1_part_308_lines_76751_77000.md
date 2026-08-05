>     '\t\t\t2 창 TCPMotionInterface Secondary Actions: Raise ID: 65285\n' +
>     '\t\t\t\t3 창 ID: 59648\n' +
>     '\t\t\t\t\t4 창 FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork VAR_INPUT EAX : UDINT; END_VAR VAR_OUTPUT state (EAX) : UDINT; END_VAR VAR currentEpoch : UDINT; sendRet : DINT; END_VAR // The ordinary TCP server and this interface run in one cyclic task. // Response publishes the queue before this CyWork consumes it. if (PendingClosedSessionEpoch <> 0) IsClientConnected(#Diagnostics) then Diagnostics.NotifySessionClosed( SessionEpoch:=PendingClosedSessionEpoch); PendingClosedSessionEpoch := 0; end_if; currentEpoch := SessionEpoch; // ControlCommands resolves object names on demand through MsgPaser in this // CyWork context. CyWork performs no periodic discovery or retry loop. if (ActiveRequestValid = TRUE) & (ActiveRequest.SessionEpoch <> currentEpoch) then ActiveRequestValid := FALSE; end_if; // The queue has one cyclic owner, so enum states use direct transitions. if ActiveRequestValid = FALSE then if RequestQueue[QueueReadIndex$DINT].State = TCPMI_QUEUE_READY then RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_ACTIVE; _StdLib.MemCpy( dest:=#ActiveRequest, source:=#RequestQueue[QueueReadIndex$DINT], size:=sizeof(ActiveRequest) ); RequestQueue[QueueReadIndex$DINT].State := TCPMI_QUEUE_FREE; QueueReadIndex := (QueueReadIndex + 1) mod 8; ActiveRequestValid := TRUE; end_if; end_if; if ActiveRequestValid = TRUE then if ActiveRequest.SessionEpoch = currentEpoch then _memset(dest:=#RequestBuf, usByte:=0, cntr:=sizeof(RequestBuf)); RequestBuf[0]$UINT := ActiveRequest.CommandId; RequestBuf[4]$UINT := ActiveRequest.PayloadLength; RequestBuf[6]$UINT := ActiveRequest.Reference; if ActiveRequest.PayloadLength > 0 then _StdLib.MemCpy( dest:=#RequestBuf[8], source:=#ActiveRequest.PayloadData[0], size:=ActiveRequest.PayloadLength ); end_if; CommandID := TO_DINT(ActiveRequest.CommandId); AxisRef := TO_DINT(ActiveRequest.Reference); Payload := TO_DINT(ActiveRequest.PayloadLength); CurrentSock := ActiveRequest.Socket; MsgPaser(); end_if; ActiveRequestValid := FALSE; end_if; if IsClientConnected(#Diagnostics) then Diagnostics.ProcessOperations(); end_if; // Fault responses are ordered after every request accepted before the fault. if (ActiveRequestValid = FALSE) & (IngressFaultPending = TRUE) & (RequestQueue[QueueReadIndex$DINT].State <> TCPMI_QUEUE_READY) then if IngressFaultEpoch <> currentEpoch then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else _memset(dest:=#Sendbuf, usByte:=0, cntr:=sizeof(Sendbuf)); Sendbuf[0]$UINT := 1; Sendbuf[2]$UINT := 4; Sendbuf[4]$UDINT := 0; Sendbuf[8]$UINT := 1; Sendbuf[10]$INT := IngressFaultError; sendRet := SendData( pData:=#Sendbuf[0], udSize:=12, dSocket:=IngressFaultSocket, bDirect:=TRUE ); if sendRet = 12 then IngressFaultPending := FALSE; if (IngressFaultCloseRequired = FALSE) & (IngressDiscardRemaining = 0) then IngressBlocked := FALSE; end_if; else IngressFaultPending := FALSE; IngressBlocked := TRUE; IngressFaultCloseRequired := TRUE; end_if; end_if; end_if; state := READY; END_FUNCTION FUNCTION VIRTUAL GLOBAL TCPMotionInterface::ConnSocketInfo VAR_INPUT dSock : DINT; InfoPara1 : DINT; InfoPara2 : DINT; END_VAR VAR takeCandidate : BOOL; takeover : BOOL; candidatePeerValid : BOOL; candidatePeerIPv4 : UDINT; activePeerIPv4 : UDINT; END_VAR case InfoPara1 of TCP_SVR_SOCK_INFO_CONNECT : // A duplicate callback for the current descriptor is not a new client. if CurrentSock = dSock then LastTakeoverResult := -7; RETURN; end_if; C ID: 10000\n' +
>     '\t\t\t\t\t\t5 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t\t6 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t7 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t8 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t9 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t10 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t\t11 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t\t12 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t\t13 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t\t14 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t\t15 위치 조정 (disabled)\n' +
>     '\t\t\t16 창 Comm_Network Secondary Actions: Raise ID: 65283\n' +
>     '\t\t\t\t17 창 ID: 59648\n' +
>     '\t\t\t\t\t18 스크롤 막대 세로 ID: NonClientVerticalScrollBar\n' +
>     '\t\t\t\t\t\t19 단추 위쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t20 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t21 단추 페이지 아래로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t22 단추 아래쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t23 스크롤 막대 가로 ID: NonClientHorizontalScrollBar\n' +
>     '\t\t\t\t\t\t24 단추 왼쪽 스크롤 화살표 ID: UpButton\n' +
>     '\t\t\t\t\t\t26 위치 조정 위치 ID: ScrollbarThumb\n' +
>     '\t\t\t\t\t\t52418 단추 페이지 오른쪽으로 ID: DownPageButton\n' +
>     '\t\t\t\t\t\t27 단추 오른쪽 스크롤 화살표 ID: DownButton\n' +
>     '\t\t\t\t\t28 위치 조정 (disabled)\n' +
>     '\t\t\t58 창 LMCDiagnosticsService Secondary Actions: Raise ID: 65287\n' +
>     '\t\t\t\t59 창 ID: 59648\n' +
>     '\t\t\t\t\t60 창 #pragma usingLtd _StdLib #define LMC_DIAG_SCHEMA_VERSION 1 #d'... 2000 more characters
> }
> ```
>
> Image output: image/jpeg
>
> Image output: image/jpeg
>
> LASAL 증분 빌드는 `0 errors / 25 warnings`로 통과했고, 생성된 네트워크 테이블에도 latch 축 연결 4개가 반영됐습니다. 소스 계약 검증도 전체 통과했으며, 현재 WPF 전체 smoke와 IDE 검색 smoke를 동시에 확인 중입니다.
>
> 독립 리뷰에서 두 가지 실제 결함이 추가로 확인됐습니다. 하나는 확정된 MMC/TW20 거절을 WPF가 “미확정 전송”으로 잘못 남겨 기능을 잠글 수 있는 문제이고, 다른 하나는 DS402 terminal record 보존 문제입니다. 게이트는 여전히 OFF로 두되, 먼저 WPF의 잘못된 격리 처리를 수정하고 회귀 테스트를 추가하겠습니다.
>
> IDE 검색 smoke 4건도 모두 통과했고, smoke 시작 이후 LASAL 로그에 새 `CInvalidArgException`은 0건입니다. 추가 리뷰에서 발견된 WPF 확정-거절 격리 버그를 수정·테스트 중이며, PLC 게이트는 안전상 계속 OFF로 유지합니다.
>
> DS402 Home의 terminal 결과가 다음 동일 축 요청에 덮어써질 수 있는 별도 결함도 확인했습니다. 새 채널/Network는 필요 없지만, 안전하게 해결하려면 exact-key 기반 `0x7D17` retire 계약과 LASAL 내부 handler 1개가 필요합니다. 이번 변경에서는 이를 억지로 활성화하지 않고 설계·차단 조건으로 명시한 채 bit 6을 OFF로 유지합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/AXIS_HOME_AND_TEST_ENDAT22_ID30_ERROR_WARNING_RESET_IMPLEMENTATION_2026-07-31.md` (+24 -1)
> - Wrote `./docs/architecture/AXIS_HOME_AND_TEST_ENDAT22_ID30_ERROR_WARNING_RESET_IMPLEMENTATION_2026-07-31.md` (+4 -0)
>
> </details>
>
> 확정 거절 격리 수정까지 반영했고 Debug/Release WPF 유지보수 회귀 테스트가 각각 `12/12` 통과했습니다. 이제 게이트·capability·생성 테이블·whitespace·잔류 프로세스를 한 번에 최종 점검합니다.
</details>

구현과 정적 검증까지 완료했습니다.

- LASAL 축 1~4 클라이언트와 Motion Network 연결 반영
- DS402 Home command 5를 2-scan SetPosition/readback 검증 방식으로 구현
- MMC/TW20 확정 거절 시 예제 전체가 격리되는 WPF 문제 수정
- LASAL 계약 검증기와 설계 문서 갱신

주요 파일:

- [LMCEcatInputLatch.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st:51)
- [MainWindow.MaintenanceActions.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs:474)
- [Verify-LasalContract.ps1](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1)
- [Home/TW20 설계 문서](./docs/architecture/AXIS_HOME_AND_TEST_ENDAT22_ID30_ERROR_WARNING_RESET_IMPLEMENTATION_2026-07-31.md:394)

검증 결과:

- LASAL F9: `0 errors / 25 warnings`
- LASAL full static contract: PASS
- SDK Debug/Release: 각각 `1066/1066 PASS`
- WPF Debug/Release build: PASS
- WPF Maintenance: 각각 `12/12 PASS`
- Find in Implementation 4건: PASS
- 신규 `CInvalidArgException`: 0
- `git diff --check`, cached check: PASS
- staged/commit/push: 없음

현재 Home/TW20은 구현되어 있지만 실제 전송은 아직 비활성화 상태입니다.

- MMC Home bit 4: OFF
- DS402 Home bit 6: OFF
- TW20 bit 18: OFF
- TW19 멀티턴 위치 리셋: 미구현

활성화 전 필요한 추가 구조는 두 가지입니다.

- Home/TW20 실행 중 동일 축 Motion/Power를 차단하는 공통 소유권 메서드
- DS402 결과 보존용 `0x7D17 RetireAxisDs402HomeOutcome` 내부 handler와 C# API

새 client/server/channel 또는 Network 연결은 필요 없습니다. PLC 다운로드와 실제 축 동작은 수행하지 않았습니다. 전체 WPF 무필터 smoke는 120초 제한을 넘겨 완료 증거에서 제외했습니다.

<details><summary>68 previous messages</summary>

> 계속 진행하겠습니다. 현재 문서와 소스를 다시 대조한 뒤, 가장 먼저 DS402 Home 결과 덮어쓰기 문제를 닫는 `0x7D17 outcome retire` 계약을 SDK·WPF·LASAL까지 연결하고 검증하겠습니다. 이어서 축별 Motion/Power 공통 소유권 interlock을 다음 구현 대상으로 진행합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 0x7D17 계약은 확정했습니다. 성공 응답은 기존 0x7D16의 92바이트 terminal 결과를 그대로 반환하고, 같은 키·세대의 재시도는 tombstone에서 동일하게 성공하도록 구현 중입니다. retire되지 않은 결과가 있으면 새 Home은 detail 32로 차단됩니다.
>
> LASAL 쪽은 새 내부 handler 선언을 IDE에서 추가해야 하므로 `computer-use` 스킬을 사용하겠습니다. 선언 구조만 IDE로 편집하고, 구현 본문과 라우팅은 추적된 `.st` 소스에서 수정한 뒤 F9와 implementation 검색으로 검증합니다. PLC 다운로드나 실제 축 명령은 실행하지 않습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "if (!globalThis.sky) {\n  var { setupComputerUseRuntime } = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.721.81911/scripts/computer-use-client.mjs\");\n  await setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidanceDoc = await sky.documentation(\"guidance\");\nvar confirmationsDoc = await sky.documentation(\"confirmations\");\nnodeRepl.write(guidanceDoc);\nnodeRepl.write(\"\\n---CONFIRMATIONS---\\n\");\nnodeRepl.write(confirmationsDoc);",
>   "title": "LASAL IDE 제어 준비",
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

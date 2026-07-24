 ├─ HandleAdminCommands
 ├─ HandleRegistryCommands
 ├─ HandleAxisCommands
 └─ HandleGroupCommands
```

`LMCAxis1..9`, `LMCRobot` Client는 이 서비스가 단독 소유합니다. Axis/Group/Admin을 처음부터 별도 객체 세 개로 나누면 같은 축 Client 연결이 중복되고 LASAL 네트워크 배선 및 검증 부담만 커집니다.

### 3. 기존 `LMCDiagnosticsService`

`0x7E00~0x7E51` 진단 계열 24개 계약을 모두 담당합니다.

현재 TCP에 남아 있는 `0x7E00` capability도 여기로 이동합니다. 기존 [HandleRequest](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:908)의 `pointer + capacity + actual length` 방식이 이번 구조의 기준입니다.

## 호출 계약

```pascal
FUNCTION GLOBAL LMCControlCommandService::HandleRequest : DINT
VAR_INPUT
    CommandId       : UINT;
    Reference       : UINT;
    pRequest        : ^USINT;
    RequestSize     : UDINT;
    pResponseFrame  : ^USINT;
    ResponseCapacity: UDINT;
END_VAR
```

규칙은 다음과 같습니다.

- 요청과 응답 배열을 값으로 전달하지 않습니다.
- 서비스는 호출자가 가진 버퍼에 직접 기록합니다.
- 서비스는 raw pointer를 함수 종료 후 보관하면 안 됩니다.
- 서비스는 완성된 TCP response frame 크기만 반환합니다.
- `SendData()`는 호출하지 않습니다.
- Facade가 frame 길이를 검증한 뒤 정확히 한 번 전송합니다.
- 동적 할당, service 간 queue, 별도 task 전환은 없습니다.

실행 경로는 다음으로 고정합니다.

```text
CyWork
  → Static domain CASE
  → 정확히 하나의 HandleRequest
  → response frame 검증
  → SendData 1회
  → Diagnostics.ProcessOperations
```

따라서 새로 추가되는 런타임 비용은 요청당 object method 호출 한 번과 domain 내부 `CASE` 한 번입니다. 요청이 없을 때는 호출되지 않습니다. 설치된 LASAL 도움말 기준 object client 하나는 12바이트이므로 TCP 측 추가 메모리는 사실상 client 한 개 수준입니다.

## 사용하지 않을 패턴

- Command object per ID: 객체와 channel 수가 과도해집니다.
- Chain of Responsibility: 명령 하나가 여러 handler를 순차 통과합니다.
- 깊은 derive/virtual 계층: method-table 간접 호출과 base 상태 중복이 생깁니다.
- 서비스별 CyWork 또는 mailbox: task scheduling, 복사, 동기화 비용이 발생합니다.
- Axis/Group/Admin 별도 객체 분할: 현재는 동일한 축/로봇 Client 연결을 중복시킵니다.
- 큰 STRUCT/ARRAY의 value 전달: 숨은 복사 비용이 생깁니다.

`EtherCAT_SDOBase`처럼 vendor callback ABI를 override해야 하는 경우에는 상속이 맞지만, 이번 기능 분할은 “is-a” 관계가 아니므로 composition이 맞습니다.

SIGMATEK도 LASAL CLASS를 OOP와 client-server 기반 구조로 설명하고 있습니다. 실제 성능 판정은 추측이 아니라 Data Analyzer와 PLC Trace View로 해야 합니다. [LASAL CLASS](https://www.sigmatek-automation.com/en/products/engineering-tool-lasal/lasal-class/), [LASAL SERVICE](https://www.sigmatek-automation.com/en/products/engineering-tool-lasal/lasal-service/)

## 안전한 구현 순서

1. 현재 PLC task time, max jitter, 명령별 latency를 먼저 기록합니다.
2. `MsgPaser()`에서 Group 11개 명령을 같은 클래스의 `HandleGroupCommands()`로 먼저 이동합니다.
   - 예상 `MsgPaser`: 약 43.5KB
   - Group handler: 약 23.7KB
   - 이 단계는 네트워크 및 buffer ownership을 전혀 바꾸지 않습니다.
3. LASAL IDE에서 `LMCControlCommandService` class/object/client 구조를 생성합니다.
4. `0x7E00`을 기존 Diagnostics service로 이동합니다.
5. read-only 명령부터 Control service로 이동합니다.
6. Axis 제어 명령을 이동합니다.
7. Group lifecycle/move 명령과 `0x7D22`를 마지막에 이동합니다.
8. TCP에서 `LMCAxis1..9`, `LMCRobot` Client와 motion scratch state를 제거합니다.
9. 안정화 후 Queue를 직접 pointer로 처리해 `ActiveRequest`와 `RequestBuf` 재복사를 제거합니다.

특히 다음 동작 순서는 그대로 유지해야 합니다.

- FIFO `FREE → WRITING → READY → ACTIVE → FREE`
- 한 CyWork당 요청 최대 한 건
- stale `SessionEpoch` 요청 폐기
- `0x405D` ACK 후 epoch 증가
- D5 submit 후 같은 scan의 `Diagnostics.ProcessOperations`
- `0x2047`은 요청 수락 즉시 ACK, 실제 완료는 `0x2045` polling
- `0x7D22`, `0x204A/B`, `0x2085`, Recorder도 ACK와 최종 상태 구분 유지

## 성능 합격 기준

- 1ms task overrun 0회
- 같은 packet workload에서 CyWork p99 증가 10µs 이하
- end-to-end p99 latency 증가 2% 이하
- 요청/응답 payload 추가 복사 0회
- PC golden packet 결과 byte-for-byte 동일
- 53개 command ID가 정확히 하나의 owner에만 존재
- `SendData`, socket, session, FIFO 접근은 TCP class에만 존재
- class memory 목표 48KiB 이하
- 개별 implementation 함수 소스는 관리 기준 32KiB 이하

객체 호출 자체가 측정 가능한 성능 저하를 보이면 hot Axis/Group handler만 같은 클래스 private 함수로 유지하는 fallback도 가능합니다. 하지만 기존 Diagnostics 구조를 보면 **no-task object composition과 pointer 전달이 현재 프로젝트에서 가장 균형이 좋은 최종안**입니다.

이번에는 설계 분석만 했으며 소스와 현재 사용자/IDE 변경 파일은 수정하지 않았습니다.

> ㅇㅋ 이대로 설계문서 완료하고 구현 진행해보자

<details><summary>58 previous messages</summary>

> 설계문서를 저장소에 확정한 뒤, 우선 `MsgPaser`의 Group 분리로 IDE 편집 문제를 해소하고 성능 기준선을 유지하겠습니다. 동시에 현재 미커밋 LASAL 프로젝트 파일과 IDE 실행 상태를 먼저 확인해 사용자 변경을 덮어쓰지 않겠습니다.
>
> 구현에는 LASAL class method 선언이 필요하므로, 규칙대로 IDE에서는 선언 구조만 만들고 즉시 종료한 뒤 구현 본문은 외부 편집으로 진행해야 합니다. 이 부분에 한해 Windows 앱 제어 절차를 확인하겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "Initialize Windows app control",
>   "code": "if (!globalThis.sky) {\n  var { setupComputerUseRuntime } = await import(\"C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.715.72359/scripts/computer-use-client.mjs\");\n  await setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidance = await sky.documentation(\"guidance\");\nvar confirmations = await sky.documentation(\"confirmations\");\nnodeRepl.write(guidance);\nnodeRepl.write(confirmations);"
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

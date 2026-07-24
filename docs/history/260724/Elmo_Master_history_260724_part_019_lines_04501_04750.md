LMC 0x7E50 요청
  → LMCDiagnosticsService ticket/owner 관리
  → 전용 LMCSdoExecutor
  → ECAT_Slave_Base.StartReadSDO(..., pCallback := THIS)
  ← actual length + abort code callback
```

`ECAT_Slave_Base.StartReadSDO`를 직접 사용하면 즉시 반환되는 `READY/BUSY/ERROR`를 구분하면서 실제 길이와 abort code도 받을 수 있습니다. 기존 `ECAT_DS402Base::AddASyncEntryDS402` 역시 actual `usLength`를 최종 callback에 전달하지 않아 그대로 사용하기에는 부족합니다.

첫 구현 범위는 기존 계획 그대로 제한하는 것이 맞습니다.

- Slave 1~4
- `0x1000:0`
- UInt32
- 4-byte Read-only
- Complete Access 꺼짐
- ticket 한 개
- Write 및 extended chunk 비활성
- 성공/busy/abort/timeout/cancel/disconnect 시험 후에만 capability `0x13F`, `MaxSdoDataBytes=4` 활성화

그리고 현재 작업본에는 먼저 복구해야 할 회귀가 있습니다.

- LASAL IDE 저장 과정에서 `LMCDiagnosticsService`의 `0x7E03/0x7E04/0x7E50` fail-closed parser가 삭제됐습니다.
- Recorder의 terminal 상태 멱등 Stop 패치도 삭제됐습니다.
- 현재 `Verify-LasalContract.ps1`는 이 때문에 실패합니다.
- 새 클래스 추가 후 Rebuild/Link는 확인되지 않았고 Save만 확인됐습니다.

따라서 지금은 `EtherCAT_SDOBase`를 수동 시험용으로 남겨두고, 정식 LMC SDO는 전용 executor로 구현하는 것이 가장 안전합니다. 이번 검토에서는 코드를 수정하지 않았습니다.

> 그럼 SDO 기능 구현에 대한 구조설계를 해봐. 나는 EtherCAT_SDOBase를 Derive Class( Lasal에서 이용하는 상속 개념 ) 을 이용해서 구현했으면 하는데, 니가 생각하기에 더 좋은 구조가 있으면 그렇게해봐.

<details><summary>6 previous messages</summary>

> 요청하신 방향대로 `EtherCAT_SDOBase` 파생 클래스를 우선안으로 검토하되, 상속으로 해결되는 부분과 별도 ticket/service 계층이 반드시 필요한 부분을 분리하겠습니다. 구현은 아직 하지 않고, LASAL 객체·네트워크·상태머신·D5 wire·검증 순서까지 저장소 설계 문서로 남기겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 파생 클래스 방향은 유지하되 역할을 분리하겠습니다. `EtherCAT_SDOBase` 파생 객체는 축별 EtherCAT mailbox만 담당하고, 기존 `LMCDiagnosticsService`가 D5 티켓·세션·타임아웃을 한 곳에서 관리하는 구조로 설계를 확정하는 중입니다. 현재 저장소에서 D5 wire 형식과 LASAL callback 계약까지 맞춰 문서화하겠습니다.
>
> 검토 결과, 단순히 base 채널을 재사용하는 상속은 위험하지만 “얇은 파생 adapter + 기존 diagnostics service의 전역 ticket 관리”는 안전하게 만들 수 있습니다. 특히 파생 클래스에서는 base의 수동 실행 경로를 봉쇄하고, 전용 4-byte 버퍼와 actual-length callback만 사용하도록 설계에 고정하겠습니다.
>
> <details><summary>Created a file, edited 4 files</summary>
>
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+636 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+37 -14)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+13 -10)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+4 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+15 -10)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -4)
>
> </details>
>
> 설계 문서는 작성했고, 기존 통합 설계·남은 구현 계획·release status의 D5 설명도 같은 구조로 맞췄습니다. 검증 중 확인된 사실은 그대로입니다. 현재 LASAL 소스는 IDE 저장 과정에서 reserved D5 handler가 빠져 정적 계약이 실패하므로, 구현 Phase 0은 이 회귀 복구입니다.
</details>

구조설계를 완료했습니다. 채택안은 `EtherCAT_SDOBase` 파생 구조이지만, base의 수동 실행 경로는 사용하지 않는 방식입니다.

핵심 구조:

- `LMCSdoExecutor : EtherCAT_SDOBase`를 축별 4개 배치
- 파생 클래스는 EtherCAT mailbox와 callback만 담당
- `LMCDiagnosticsService`가 전역 one-ticket, BootId, session, timeout, cancel 관리
- `ParaReadWrite::Write`를 override해 수동 실행 차단
- `ParaValue` 대신 전용 4-byte buffer 사용
- callback의 actual length와 abort code 보존
- Running SDO는 강제 취소하지 않고 queued-only cancel 적용
- 첫 구현은 축 1~4의 `0x1000:0`, UInt32, 4-byte Read-only로 제한

문서:

- [SDO 파생 Executor 상세 설계](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md:8)
- [남은 구현 계획 갱신](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md:129)
- [통합 Diagnostics 설계 갱신](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md:1889)
- [Release 상태 갱신](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:40)

검증 결과:

- 문서 whitespace와 `git diff --check`: 통과
- LASAL 정적 계약: 예상대로 실패
- 원인: 현재 IDE 저장본에서 `0x7E03/0x7E04/0x7E50` reserved handler와 Recorder Stop 멱등 처리가 빠짐

따라서 구현 시작 순서는 다음과 같습니다.

1. 현재 IDE 저장 회귀 복구
2. LASAL IDE에서 `LMCSdoExecutor` 파생 클래스와 네트워크 생성
3. callback mailbox와 one-ticket 실행부 구현
4. capability를 끈 상태로 정적·IDE·PLC 시험
5. 전부 통과한 뒤에만 `0x0000013F`, `MaxSdoDataBytes=4` 활성화

이번에는 설계와 문서만 수정했고 코드는 아직 구현하지 않았습니다.

> 그럼 이 설계안으로 소스코드 구현을 진행하자. 구현 진행하기 전에 커밋 안된부분 정리해서 커밋하고 진행해

<details><summary>161 previous messages</summary>

> 먼저 현재 dirty working tree를 사용자 LASAL IDE 추가분, 우발적으로 사라진 기존 구현, 제가 작성한 설계 문서로 분류하겠습니다. 누락된 D5 parser와 Recorder Stop 처리를 복구해 정적 계약을 다시 통과시킨 뒤, 기존 미커밋분을 목적별 커밋으로 고정하고 새 SDO 구현을 시작하겠습니다.
>
> 현재 미커밋 핵심은 의도된 LASAL 재구성입니다. EtherCAT master와 4축 drive를 새 `EtherCAT_Network`로 이동하고 `EtherCAT_SDOBase1..4`를 추가한 변경입니다. 원본 42MB 히스토리는 이미 215개 chunk로 커밋돼 있으므로 원본 파일은 보존하되 Git 대상에서는 제외하겠습니다. 먼저 IDE 저장으로 사라진 기존 로직 두 군데만 복구합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./.gitignore` (+3 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+45 -12)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+9 -4)
>
> </details>
>
> 기존 미커밋분의 LASAL source-only/full-network 계약은 모두 다시 PASS했습니다. 이제 이 baseline을 커밋하기 전에 C# 회귀 102개와 WPF Debug/Release 빌드를 재확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 복구한 진단/Recorder 계약과 테스트 통과 상태를 기준으로 문서의 “회귀 중” 표현부터 바로잡고, LASAL IDE로 추가된 SDO 기반 구조와 설계 문서를 목적별 커밋으로 정리하겠습니다. 커밋 후에는 새 파생 클래스 생성이 필요한 부분은 저장소 규칙대로 LASAL IDE 경로를 사용하고, C#·정적 계약은 병렬로 구현하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -10)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+6 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+11 -13)
>
> </details>
>
> 감사 결과, 새 EtherCAT 네트워크는 객체 GUID와 기존 연결을 보존한 의도된 구조 변경입니다. 반면 진단 클래스 3개의 `Objectsize`와 `Comm_Network` 좌표 변경은 기능 없는 IDE 노이즈로 확인됐습니다. 이 네 항목만 기존 상태로 되돌리고, 원본 히스토리 보존 규칙·LASAL 토폴로지·설계 문서를 각각 분리 커밋하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st` (+1 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
>
> </details>
>
> 사전 정리는 세 커밋으로 완료했고 작업 트리는 깨끗합니다. 이제 파생 클래스 선언과 네트워크 객체 교체는 저장소 규칙상 LASAL IDE에서 해야 하므로, Windows 앱 제어 절차를 사용해 IDE 스켈레톤을 먼저 만들겠습니다. 그 뒤 추적되는 `.st` 구현과 PC 정책·테스트를 코드 수준에서 채우겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "if (!globalThis.sky) {\n  const { setupComputerUseRuntime } = await import(\"C:\\\\Users\\\\dreiv\\\\.codex\\\\plugins\\\\cache\\\\openai-bundled\\\\computer-use\\\\26.715.61943\\\\scripts\\\\computer-use-client.mjs\");\n  await setupComputerUseRuntime({ globals: globalThis });\n}\nvar guidanceText = await sky.documentation(\"guidance\");\nvar confirmationsText = await sky.documentation(\"confirmations\");\nvar apiText = await sky.documentation(\"api\");\nnodeRepl.write(\"GUIDANCE\\n\" + guidanceText + \"\\nCONFIRMATIONS\\n\" + confirmationsText + \"\\nAPI\\n\" + apiText);",
>   "title": "LASAL IDE 제어 절차 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ````text
> GUIDANCE
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

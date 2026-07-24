  - Recorder Start 후 identity 보존
  - disconnect/reconnect
  - exact RecordId/BufferId Adopt
  - 별도 실행에서 zero-ID discovery Adopt
  - Status→Stop/Header/Download/Release
  - 자동 Qualification 시나리오를 추가해야 합니다.

## 3차: SDO Fault 캡처

1차·2차 결과가 정상인 뒤 진행합니다.

- `23a_SDO_Abort_Recovery_7E50_03`
- `23b_SDO_Offline_Recovery_7E50_03`
- `23c_SDO_Timeout_Recovery_7E50_03`
- `23d_SDO_QueuedCancel_7E04`
- `23e_SDO_DisconnectOrphan_Recovery`
- `23f_SDO_Contention_ResourceBusy_Recovery`

특히 Queued Cancel, orphan, contention은 수동 클릭만으로 재현성이 부족하므로 PLC timing hook 또는 전용 Qualification 시나리오를 먼저 구현해야 합니다.

## 다음 개발 순서

1. 현재 구현 코드와 설계 문서를 목적별로 커밋
2. 13~17, 19, 20, 22 캡처 분석 및 PASS/FAIL 판정
3. Qualification fake-RPC 자동시험 추가
   - success
   - cancel-before-wire
   - cleanup failure
   - primary+cleanup 오류 보존
   - local double-release 무송신
4. Bulk slave-offline guided checkpoint 구현
5. Recorder exact/zero-ID reconnect-adopt Qualification 구현
6. SDO 23a~23f fault/timing 시나리오 구현
7. 내부 전용 stale BootId/MapRevision/raw negative-wire 도구 설계
8. LASAL Data Analyzer로 1 ms task jitter, overrun, free RAM, Recorder bank 불변성 검증
9. `RecorderDoubleBank`는 PLC capability가 광고될 때까지 SKIP
10. 위 gate가 닫힌 뒤 Homing, SetPosition, PI/SDO Write와 production 승격 진행

현재는 코드·빌드·정적 검증까지만 완료된 상태이며, 실물 PLC 완료 판정은 위 `pcapng + QTEST`가 확보된 뒤 가능합니다.

> 그럼 정리해서 커밋하는거 먼저 진행하자

<details><summary>14 previous messages</summary>

> 현재 작업 트리를 먼저 목적별로 분류하겠습니다. 소스/정적 계약, WPF Qualification, 분석·설계 문서, 실험 로그를 각각 확인하고 생성물이나 캡처 원본은 제외한 뒤, 서로 독립적인 커밋으로 정리하겠습니다. 기존 사용자 변경이 섞여 있으면 임의로 포함하지 않겠습니다.
>
> 커밋 전 감사에서 문서 한 곳이 현재 소스보다 뒤처진 것을 찾았습니다. `0x2047` 수정은 이미 반영됐는데도 “수정 대기”라고 적혀 있어, “소스·정적 계약 완료 / 새 PLC 빌드·재캡처 대기”로 바로잡은 뒤 커밋하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 감사 결과상 커밋은 3개로 분리하겠습니다: PLC 0x2047 계약 수정, WPF 런타임 Qualification UI, 분석·설계 문서. 8MB `Classes.lcb`는 이번 외부 `.st` 수정과 동기화된 근거가 없어서 건드리지 않고 보존하며, 원시 캡처 TXT도 소스 커밋에는 넣지 않겠습니다. 이제 각 묶음을 검증해 순서대로 커밋하겠습니다.
>
> 문서 교차검토에서 `09b` 재캡처 결과가 일부 오래된 “pending” 문구에 남아 있는 것이 확인됐습니다. 실제 확인 범위는 “None/ACS가 동일한 static-alias 축 순서로 PASS”이므로, 과장 없이 그 경계로 4곳을 맞춘 뒤 문서 커밋에 포함하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+9 -7)
> - Wrote `./README.md` (+4 -2)
>
> </details>
>
> 첫 번째 커밋을 완료했습니다: `8572adb` — Group Enable을 비동기 상태 완료가 아니라 native 요청 수락 시점에 ACK하도록 수정하고, 같은-cycle LockState 판정을 금지하는 정적 계약을 추가했습니다. `Classes.lcb`는 최신 `.st`보다 오래된 IDE 상태라 계속 제외한 채 WPF 변경을 묶겠습니다.
>
> 두 번째 커밋도 완료했습니다: `b7d0a7a` — Group/Bulk/Recorder Qualification runner와 capture 기반 UI 안전 보강을 함께 묶었습니다. 문서에는 `09b` PASS, 아직 남은 `0x2047` live 재검증, fault/race/soak 경계를 일관되게 반영한 뒤 마지막 문서 커밋을 만들겠습니다.
>
> 세 번째 커밋을 완료했습니다: `84d503b` — live capture 분석, 현재 상태, 다음 Qualification 설계를 20개 진입/설계 문서에 동기화했습니다. 이제 최종 HEAD에서 Debug/Release 빌드·148개 PC 계약 테스트·LASAL 정적 계약을 다시 실행하고, 커밋되지 않은 항목이 의도한 로컬 증거와 오래된 IDE 바이너리뿐인지 확인하겠습니다.
>
> 최종 재검증은 모두 통과했습니다: WPF Debug/Release 빌드와 3초 startup smoke, PC 계약 테스트 Debug/Release 각각 148/148, LASAL SourceOnly/full 정적 계약입니다. 현재 브랜치는 3커밋 앞서 있으며, 남은 변경은 의도적으로 보류한 `Classes.lcb`와 원시 TXT 로그뿐입니다. 마지막 문서 상태 감사를 마치는 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
>
> </details>
>
> 최종 감사에서 문서의 실제 상태와 어긋나는 표현이 추가로 발견됐습니다. 코드 문제는 아니며, `09b`, 148/148, GroupStop/D2 검증 범위를 정확히 맞춘 뒤 방금 만든 문서 커밋에 amend하겠습니다. 아직 push하지 않았으므로 새 불필요한 보정 커밋은 만들지 않겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+3 -1)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+4 -1)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+2 -2)
>
> </details>
</details>

정리 후 3개 커밋으로 분리했습니다. `main`은 `origin/main`보다 3커밋 앞서 있으며 push는 하지 않았습니다.

- `8572adb` `fix(lasal): acknowledge group enable before status polling`
- `b7d0a7a` `feat(test-app): add runtime qualification workflows`
- `b6c3511` `docs(api): record live results and qualification plan`

검증 결과:

- WPF Debug/Release 빌드 및 startup smoke: PASS
- PC 계약 테스트 Debug/Release: 각각 148/148 PASS
- LASAL SourceOnly/full 정적 계약: PASS
- `git diff --check`: PASS

의도적으로 커밋하지 않은 항목:

- `Classes.lcb`: 최신 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st) 수정 전 IDE 바이너리라 현재 소스와 동기화되지 않음
- 원시 TXT 로그 10개: 로컬 보존. `04b`는 0바이트, `API_TEST.txt`는 `05b`와 중복
- `*.pcapng`: 기존 `.gitignore` 정책대로 제외

다음 단계는 LASAL IDE에서 프로젝트 동기화·빌드·다운로드 후 `0x2047` ACK와 새 Qualification 시나리오를 실기 캡처하는 것입니다.

> TCPMotionInterface의 0x2047 구현을 Find in Implementation으로 확인합니다.
> %TEMP%\Lasal2.log에 신규 CInvalidArgException이 없는지 확인합니다.
>
> 이거 안되는데? 익셉션 생긴다. 왜그런건지 알아봐바

<details><summary>1 previous message</summary>

> 우선 추정하지 않고 신규 `CInvalidArgException`의 발생 시각·스택·대상 클래스를 `%TEMP%\Lasal2.log`에서 분리하겠습니다. 동시에 `Classes.lcb`와 외부 수정된 `TCPMotionInterface.st`의 동기화 상태, 기존 LASAL 오류 예방 기록을 교차 확인해서 “검색 기능 자체 문제인지, 프로젝트 DB와 소스 불일치인지”를 구분하겠습니다. 지금은 진단만 하고 파일은 수정하지 않겠습니다.
</details>

결론부터 말하면 `0x2047` 구현 문제도 아니고, 최신 세션에서 `CInvalidArgException`이 발생한 것도 아닙니다. 제가 앞서 검사 방법을 부정확하게 안내했습니다.

최신 LASAL 세션(PID 34128) 확인 결과:

- `TCPMotionInterface.st` 컴파일 성공: 14:58:20
- `Classes.lcb` 갱신: 14:58:23
- PLC 다운로드 성공: 14:58:41
- `LMCAxis9` 구현 검색 2회 성공
- `CommandID` 구현 검색 성공
- 신규 `CInvalidArgException`: 0건
- 실패한 것은 `_TCPIPServer` 구현 검색 1건뿐

`_TCPIPServer`는 `TCPMotionInterface`의 직접 Client가 아니라 상속된 `_TCPIPServerInterface` 내부 Client입니다. LASAL이 이 상속 경로의 참조 검색을 제대로 처리하지 못해 명령 실패를 반환한 것으로 판단됩니다. 전체 검색 인덱스가 깨졌다면 `LMCAxis9`와 `CommandID`도 실패해야 하지만 정상 동작했습니다.

로그에 보이는 `CInvalidArgException` 17건은 이전 PID 27828 세션에서 `ECAT_Slave_Base`를 열 때 발생한 과거 기록입니다. `%TEMP%\Lasal2.log`가 세션별로 초기화되지 않아 전체 파일을 검색하면 신규 예외로 오인하게 됩니다.

정확한 검사 방법은 다음입니다.

1. Implementation/File Editor에서 일반 찾기 `Ctrl+F`로 `0x2047`을 검색합니다.
   - [0x2047 handler](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2390)
   - [수락 즉시 ACK 0 처리](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2408)

2. `Find in Implementation`은 Object Network의 직접 Client인 `LMCRobot`으로 실행합니다.
   - `CommandID`, `LMCAxis9`도 유효
   - `_TCPIPServer`는 검사 대상에서 제외

3. 로그는 해당 smoke 시작 이후 추가된 부분이나 현재 LASAL PID만 검사합니다.

따라서 캐시 삭제나 프로젝트 재생성은 필요 없습니다. `LMCRobot` 검색도 실패하면서 현재 PID에 신규 `CInvalidArgException`이 기록될 때만 IDE 캐시 문제를 추가 조사하면 됩니다.

참고로 로그에는 라이브러리 자동 재링크 전 `E0015`가 한 번 있지만, 이후 최종 Rebuild/Link와 PLC Download가 성공했으므로 이번 구현 검색 실패와는 별개입니다.

> Find in Implementation 해도 Find Results에 결과가 안나온다고 사람말을 못믿냐

<details><summary>12 previous messages</summary>

> 맞습니다. 제가 `Last command succeeded`를 “Find Results에 결과가 표시됐다”로 잘못 해석했습니다. 그 로그는 검색 명령이 실행됐다는 뜻일 뿐, 검색 결과가 생성됐다는 증거가 아닙니다. 사용자 확인이 맞습니다.
>
> 이번에는 LASAL 화면의 실제 `Find Results` 상태와 선택한 Client/Server를 직접 확인하겠습니다. Windows 앱 상태를 확인해야 하므로 `computer-use` 지침을 사용하되, 프로젝트나 소스는 수정하지 않겠습니다.
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "title": "LASAL 화면 확인 준비",
>   "code": "if (!globalThis.sky) { const { setupComputerUseRuntime } = await import('C:/Users/dreiv/.codex/plugins/cache/openai-bundled/computer-use/26.715.72359/scripts/computer-use-client.mjs'); await setupComputerUseRuntime({ globals: globalThis }); }\nvar guidanceDoc = await sky.documentation('guidance');\nvar confirmationsDoc = await sky.documentation('confirmations');\nnodeRepl.write(guidanceDoc);\nnodeRepl.write(confirmationsDoc);"
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

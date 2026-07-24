> 							112 텍스트 (disabled) Run Single Manual
> 						113 단추 (disabled) Run Ring Forced Trigger ID: ButtonRunRecorderRingQualification
> 							114 텍스트 (disabled) Run Ring Forced Trigger
> 						115 단추 (disabled) Run Trigger Lifecycle Soak ID: ButtonRunRecorderSoakQualification
> 							116 텍스트 (disabled) Run Trigger Lifecycle Soak
> 						117 단추 (disabled) Cancel Test ID: ButtonCancelRecorderQualification
> 							118 텍스트 (disabled) Cancel Test
> 						119 단추 (disabled) Save QTEST Log ID: ButtonSaveRecorderQualificationLog
> 							120 텍스트 (disabled) Save QTEST Log
> 						121 진행률 표시줄 ID: ProgressRecorderQualification
> 						122 텍스트 No qualification has run yet. ID: TextRecorderQualificationProgress
> 						123 편집 ID: TextRecorderQualificationSummary
> 					124 스크롤 막대 ID: VerticalScrollBar
> 						125 단추 (disabled) ID: PART_LineUpButton
> 						126 단추 (disabled) ID: PageUp
> 						127 단추 (disabled) ID: PageDown
> 						128 엄지 (disabled)
> 						129 단추 (disabled) ID: PART_LineDownButton
> 			130 탭 항목 (selectable) SDO / Write Policy
> 				131 텍스트 SDO / Write Policy
> 			132 탭 항목 (selectable) Read-only API
> 				133 텍스트 Read-only API
> 		134 그룹 Execution log / raw callback diagnostics
> 			135 단추 Execution log / raw callback diagnostics ID: HeaderSite
> 				136 텍스트 Execution log / raw callback diagnostics
> 		137 상태 표시줄
> 			138 텍스트 Ready ID: TextOperationState
>
> Document text: ```
> Structured QTEST results will appear here.
> ```
> ````
>
> Image output: image/jpeg
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var closeObservation = globalThis.state;\nglobalThis.state = null;\nawait sky.click({ window: closeObservation.window, element_index: 6 });\nawait new Promise((resolve) => setTimeout(resolve, 800));\nglobalThis.openWindowsAfterClose = await sky.list_windows();\nvar remainingQualificationWindows = openWindowsAfterClose.filter((window) => window.title === \"LASAL Motion Control API Example\" && window.app.includes(\"LasalMotionControlApiExample.exe\"));\nnodeRepl.write(JSON.stringify({ remainingQualificationWindows }, null, 2));",
>   "title": "WPF 테스트 앱 종료",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   "remainingQualificationWindows": [
>     {
>       "app": "process:C:\\work\\Elmo\\Elmo_Master\\LMC_Library\\LasalApiWpfTestApp\\LasalApiWpfTestApp\\bin\\Debug\\LasalMotionControlApiExample.exe",
>       "id": 36311334,
>       "title": "LASAL Motion Control API Example"
>     }
>   ]
> }
> ```
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "await new Promise((resolve) => setTimeout(resolve, 2500));\nglobalThis.openWindowsAfterClose2 = await sky.list_windows();\nvar remainingQualificationWindows2 = openWindowsAfterClose2.filter((window) => window.title === \"LASAL Motion Control API Example\" && window.app.includes(\"LasalMotionControlApiExample.exe\"));\nnodeRepl.write(JSON.stringify({ remainingQualificationWindows2 }, null, 2));",
>   "title": "WPF 종료 확인",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> {
>   "remainingQualificationWindows2": []
> }
> ```
>
> 시각 smoke에서 Group/Bulk/Recorder 탭과 새 Qualification 컨트롤이 실제 WPF 창에 정상 생성되는 것을 확인했습니다. 동시에 코드 리뷰에서 Recorder 정상 Download 후 PLC 상태가 `Uploading`으로 바뀌는데 cleanup이 이를 거부하는 실제 차단 결함을 찾았습니다. 이 상태는 PLC가 Release를 허용하므로 `Ready/Uploading`만 해제 가능하게 하고, `Fault`는 해제 성공으로 오인하지 않도록 실패로 분리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+16 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+1 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+4 -4)
>
> </details>
>
> Bulk 쪽에도 예외 보존 결함이 있었습니다. Snapshot/Lifecycle의 본 작업과 Release가 동시에 실패하면 기존 `finally`가 원인을 덮어썼습니다. 각 resource scope에서 원 작업 예외와 cleanup 예외를 `AggregateException`으로 함께 보존하도록 수정하고 있습니다. Recorder Download 취소도 내부 chunk RPC 사이에서만 적용되도록 scenario token을 composite Download API에 전달했습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+109 -17)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 설계 구현은 이미 들어가 있고, 지금은 실사용 전에 막아야 할 두 가지 안전 결함을 우선 고치고 있습니다. 진단 RPC가 Stop/PowerOff보다 먼저 재진입할 수 있는 경로와, 두 번째 Release가 실제로 전송돼 거부돼도 로컬 차단 성공으로 오판하는 검증 로직입니다. 이 두 부분을 수정한 뒤 전체 빌드·계약 테스트·UI 실행 검증까지 다시 돌리겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+104 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+60 -24)
>
> </details>
>
> 안전 우선순위는 단순히 취소 토큰만 추가해서 해결되지 않습니다. 각 요청 직전에 공용 전송 게이트와 Stop/PowerOff 세대값을 다시 확인하도록 바꾸고 있으며, Recorder 다운로드는 여러 청크를 내부에서 연속 송신하므로 qualification 전용 청크 루프로 분리해 청크 사이에도 안전 명령이 끼어들 수 있게 수정 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+87 -64)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+11 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+139 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+7 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+116 -27)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+6 -0)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+9 -5)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+6 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+31 -27)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+37 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+8 -12)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+33 -25)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+10 -0)
>
> </details>
>
> 현재 핵심 수정은 컴파일이 통과했습니다. 이제 PC 계약 시험 148개를 Debug/Release 양쪽에서 다시 실행하고, LASAL `0x2047` 정적 계약(SourceOnly/Network 포함)도 병렬로 재검증하겠습니다. 이 검증은 실기 PLC 결과와는 구분해서 기록합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+4 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+4 -2)
>
> </details>
</details>

설계된 1차 Qualification 구현을 완료했습니다.

- PLC `0x2047 GroupEnable`을 acceptance ACK 후 `0x2045` polling 구조로 수정했습니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2390)
- 공통 실행기와 safety-generation/send-gate/cleanup 직렬화를 구현했습니다. [MainWindow.Qualification.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs:115)
- Group Enable, True Buffered A→B, Stop-first 시험을 구현했습니다.
- Bulk 24-entry snapshot/lifecycle soak와 엄격한 double-release 검증을 구현했습니다. [MainWindow.Qualification.Bulk.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs:37)
- Recorder Single/Ring/100회 soak를 구현했습니다. 다운로드는 청크별로 safety gate를 다시 확인합니다. [MainWindow.Qualification.Recorder.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs:904)
- Recorder cleanup 실패는 원 오류와 함께 보존하며, `Fault`에서는 자동 Release하지 않습니다.
- PLC 거부 예외를 로컬 double-release 차단 성공으로 오판하던 경로도 수정했습니다.
- 설계 및 사용 문서를 현재 구현과 검증 경계에 맞췄습니다. [Qualification 설계](./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md:244)

검증 결과:

- WPF Debug/Release 빌드: PASS
- PC 계약 시험: Debug 148/148, Release 148/148 PASS
- LASAL SourceOnly/full 정적 계약: PASS
- Debug 실제 화면 visual smoke: PASS
- Debug/Release 3초 startup smoke: PASS
- 독립 정적 리뷰: 남은 BLOCKER/HIGH 없음
- `git diff --check`, `git diff --cached --check`: PASS

아직 실물 PLC 시험은 하지 않았습니다. 다음 캡처 순서는 `13 GroupEnable`, `14 True Buffered`, `15 Stop-first`, `16/17 Bulk`, `19/20/22 Recorder`입니다. `18 slave-offline`, `21 reconnect/adopt`는 외부 조작이 필요한 별도 단계입니다.

커밋은 하지 않았습니다. 새 Qualification 소스와 설계 문서는 아직 untracked 상태이며 기존 작업 내역도 그대로 보존했습니다.

> 캡쳐 순서에 대해 리스트업을 해주고 다음에 진행해야 될 내용에 대해서도 정리해줘

<details><summary>1 previous message</summary>

> 현재 설계 문서와 실제 구현 상태를 다시 맞춰서, 바로 실행 가능한 캡처와 외부 조작이 필요한 캡처를 분리해 순서대로 정리하겠습니다. 이어서 각 캡처가 끝난 뒤 개발해야 할 항목도 우선순위로 묶겠습니다.
</details>

현재 권장 순서는 “자동화된 저위험 시험 → 외부 조작 시험 → Fault 시험”입니다. 전체 기준은 [Runtime Qualification 설계](./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md:114)에 정리돼 있습니다.

## 캡처 전 준비

1. 현재 변경 내용을 먼저 커밋해 캡처와 소스 revision을 고정합니다.
2. LASAL에서 프로젝트 Reload/Reopen 후 Rebuild/Link 및 PLC Download를 수행합니다.
3. `TCPMotionInterface`의 `0x2047` 구현을 `Find in Implementation`으로 확인합니다.
4. `%TEMP%\Lasal2.log`에 신규 `CInvalidArgException`이 없는지 확인합니다.
5. WPF에서 Connect 후 필요한 Group/Diagnostics 초기화를 완료합니다.
6. Wireshark 권장 조건:
   - Capture filter: `host 10.10.150.1`
   - Display filter: `tcp.port == 4000 || udp.port == 5000`
7. 한 파일에 여러 시험을 합치지 말고 각 시험마다 `pcapng + QTEST txt` 한 쌍으로 저장합니다.

## 1차: 지금 바로 실행할 자동 Qualification

| 순서 | 파일명 | 실행/합격 핵심 |
|---:|---|---|
| 1 | `13_GroupEnable_AcceptedThenLocked_2047_2045` | `0x2047` 1건, ACK ErrorId=0, 이후 `0x2045`에서 PowerOn+Standby 3회 |
| 2 | `14_Group_TrueBuffered_Chaining_7D22` | A/B는 Buffer=2, A 완료 전 B 송신, 최종 `Start+A+B`, Buffer=1 복귀, 성공 경로 Stop 없음 |
| 3 | `15_Group_StopFirst_Preemption_2085_NoMove` | Move packet 0건, `0x2085` Stop 1건, 최종 stable InPosition |
| 4 | `16_Bulk_24Entry_100Snapshot_Soak_7E30_33` | 정확히 24개 entry, snapshot 100/100, Partial/Invalid/Error 0, Release 성공 |
| 5 | `17_Bulk_100Lifecycle_ReleaseReuse_7E30_33` | Configure→Active→Snapshot→Release 100회, 이후 새 Configure 성공, 두 번째 Release packet 없음 |
| 6 | `19_Recorder_SingleManual_Lifecycle_7E40_48` | 4채널/1000 samples, 자연 완료, Download A/B SHA-256 동일, buffer/config Release |
| 7 | `20_Recorder_Ring_ForcedTrigger_7E42` | pre=100/post=899, 강제 Trigger, TriggerIndex=100, 1000 samples, chunk gap/중복 0 |
| 8 | `22_Recorder_100Cycle_TriggerSoak` | lifecycle 100/100, ResourceBusy/Dropped/Overflow 0, 매 회 cleanup 성공 |

각 시험은 다음 순서로 진행합니다.

1. 시험 시작 조건을 준비합니다.
2. Wireshark 캡처를 시작합니다.
3. Qualification 버튼을 한 번만 실행합니다.
4. PASS/FAIL 및 cleanup 완료까지 기다립니다.
5. 필요한 PowerOff/Release와 최종 상태까지 포함합니다.
6. 캡처를 중지하고 위 이름으로 `.pcapng`를 저장합니다.
7. `Save QTEST Log`로 같은 이름의 `.txt`를 저장합니다.
8. 실패해도 파일을 삭제하지 말고 `_FAIL_시간`을 붙여 보존합니다.

## 2차: 추가 구현 후 실행할 시험

다음 두 시험은 외부 조작 또는 guided workflow가 아직 필요하므로 1차 시험 뒤 진행합니다.

- `18_Bulk_Partial_OneSlaveOffline_7E32`
  - Group PowerOff/no-motion 확인
  - 정상 24 Valid baseline
  - 한 slave offline
  - 해당 축 6개만 Invalid/SlaveOffline, 나머지 18개 Valid
  - slave 복구 후 다시 24 Valid
  - 앱에 `Pause → 외부 조작 → Resume` checkpoint UI를 먼저 구현해야 합니다.

- `21_Recorder_Reconnect_ExactAndDiscovery_7E49`

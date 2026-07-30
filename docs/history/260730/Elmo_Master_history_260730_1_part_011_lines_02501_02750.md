- 미stage 변경 `git diff --check` 통과

이번 구현에는 LASAL `.st`/Network 소스 변경이 없습니다. 따라서 지금은 LASAL 빌드나 테스트 폴더 복사가 필요하지 않습니다. 작업 트리의 `Classes.lcb` 변경도 건드리지 않았습니다.

실제 PLC 재접속/adopt 동작은 아직 미검증입니다. 사용자의 현재 구동 테스트가 끝난 뒤 WPF의 두 신규 버튼으로 확인하면 됩니다. 저는 그동안 Group/Bulk 및 Recorder fault/cancel 자동 검증 쪽을 계속 개발하겠습니다.

참고로 `git diff --cached --check`는 기존에 stage된 `docs/history/Elmo_Master_history_260721.md`의 168개 후행 공백 때문에 계속 실패합니다. 해당 파일은 수정하거나 stage 해제하지 않았습니다.

<details><summary>20 previous messages</summary>

> 계속 진행하겠습니다. 다음 우선순위는 설계 문서에 남은 `Group/Bulk` 및 `Recorder fault/cancel orchestration`의 PC 자동 검증입니다. 먼저 현재 테스트가 실제로 덮는 범위를 다시 대조한 뒤, 누락된 상태 전이와 정리 순서를 공용 정책 또는 fake-RPC 회귀 테스트로 구현하겠습니다. 이번 단계도 LASAL 소스를 건드리지 않는 한 빌드는 요청하지 않습니다.
>
> 현재 감사에서 단순 테스트 공백보다 먼저 고쳐야 할 실제 결함 두 개가 확인됐습니다.
>
> - Recorder: Start 성공 후 pre-history Status에서 연결이 끊기면, 보존 가능한 identity가 있어도 재접속 복구로 가지 못할 수 있음
> - Group Stop-First: Stop 전송/검증 실패 시 fallback Stop과 3회 안정 상태 확인이 누락됨
>
> 두 결함을 먼저 수정하고, 같은 로직을 PC 테스트가 직접 컴파일하도록 분리해 회귀 시험을 붙이겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderReconnectQualificationPolicyTests.cs` (+39 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+57 -18)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs` (+33 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+147 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderReconnectQualificationPolicyTests.cs` (+29 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs` (+23 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+53 -20)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+12 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+75 -0)
>
> </details>
>
> Recorder 쪽은 수정이 들어갔습니다. Start ACK 직후 recovery identity를 보존하고, cleanup 경로를 임의 플래그가 아니라 실제 연결 상태로 선택하게 바꿨습니다. 또한 `Armed/Recording → Stop`, `Ready/Uploading → Release`, `Fault/기타 → 보존` 판단을 한 정책으로 통합했으며, pre-close 전송 단절 후 exact adopt 복구와 Fault 상태에서 mutation 0건을 fake-RPC로 추가 검증 중입니다. Group Stop-First fallback 구현은 병행 진행 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderReconnectQualificationPolicyTests.cs` (+56 -36)
>
> </details>
>
> <details><summary>Created a file, edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/GroupStopQualificationOrchestratorTests.cs` (+272 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
>
> </details>
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+13 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/GroupStopQualificationOrchestrator.cs` (+3 -3)
>
> </details>
>
> 현재 자동 시험은 `175/175`까지 올라갔고 새 Group 3개와 Recorder 3개가 모두 통과했습니다. Group 쪽은 정상 Stop 1건/Status 3건/Move 0건, 최초 Stop 실패 후 fallback, fallback까지 실패했을 때 primary+cleanup aggregate를 각각 검증합니다. 이제 독립 리뷰와 최종 Debug/Release 빌드를 맞추고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/GroupStopQualificationOrchestrator.cs` (+5 -7)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 구현분의 UI 스레드 안전성 수정부터 재확인하고, PC 테스트 175개와 WPF Debug/Release 빌드를 다시 돌리겠습니다. 이번 변경은 LASAL 소스가 아니므로 `Elmo_EtherCAT_Test_4Axis` 빌드·복사 요청 없이 메인 저장소에서만 마무리합니다.
>
> UI 컨텍스트를 끊던 `ConfigureAwait(false)`가 전부 제거된 것을 확인했습니다. 이제 서로 다른 임시 출력 경로를 써서 Debug/Release 테스트를 병렬 실행하므로, 사용자가 실행 중인 WPF나 저장소의 `bin/obj`에는 손대지 않습니다.
>
> PC 계약·가짜 RPC 테스트는 Debug/Release 모두 `175/175` 통과했습니다. 이어서 동일한 임시 출력 방식으로 WPF 자체의 Debug/Release 컴파일을 확인합니다.
>
> WPF도 Debug/Release 모두 컴파일됐습니다. 지금은 문서의 테스트 수치, 공백 오류, 변경 범위를 교차 확인하고 있으며, 별도 리뷰 에이전트가 Recorder/Group 안전 복구 경로를 한 번 더 독립 검토 중입니다.
>
> 독립 리뷰에서 Group Stop의 UI 스레드 문제는 해소됐고, Recorder에 원래 Close 예외의 스택을 잃는 P2 한 건이 남았습니다. 정리 `finally` 구조는 유지하면서 예외를 원래 발생 지점 그대로 재전파하도록 수정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+2 -1)
>
> </details>
>
> 최종 리뷰에서 실행 코드 P0~P2는 더 없었습니다. 다만 이번 UI 컨텍스트 회귀를 현재 콘솔 테스트가 잡지 못하는 테스트 공백이 확인됐으므로, 호출 스레드 컨텍스트에서 fallback이 실행되는 회귀 테스트를 추가한 뒤 테스트 수치와 문서를 다시 맞추겠습니다. Recorder 전체 WPF orchestration 추출은 범위가 더 커서 다음 구현 단위로 넘깁니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/GroupStopQualificationOrchestratorTests.cs` (+138 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
>
> </details>
>
> UI 컨텍스트 회귀 테스트가 추가되어 Debug는 `176/176`으로 통과했습니다. Release도 다시 실행하고, 리뷰에서는 이 테스트가 과거의 `ConfigureAwait(false)`를 실제로 잡는 구조인지까지 확인합니다.
>
> 최종 상태는 Group fallback의 실제 UI 컨텍스트 회귀까지 자동 검증되는 `176/176`입니다. 마지막으로 현재 변경분의 공백 검사와 LASAL 소스 비변경 여부, 문서 수치 일치를 확인한 뒤 이번 구현 단위를 정리하겠습니다.
</details>

이번 구현 단위를 완료했습니다.

- Recorder: Start 직후 identity 보존, 전송 장애 시 exact reconnect, Fault/identity 불일치 시 Stop·Release 금지, Close 원본 예외 스택 보존.
- Group Stop-first: 실패 시 gate 반환 후 `GroupStop → Standby 3회` fallback 수행. UI 컨텍스트 회귀도 자동 검증합니다.
- PC 테스트: Debug/Release 각각 `176/176 PASS`
- WPF: Debug/Release 빌드 PASS
- 변경분 `git diff --check` PASS
- LASAL `.st`/Network 변경 없음. 따라서 지금은 LASAL 빌드나 테스트 폴더 복사가 필요 없습니다.
- 기존 staged history 파일에는 미정리 trailing whitespace 168건이 남아 있어 `git diff --cached --check`만 실패합니다. 이번 작업에서는 건드리지 않았습니다.

핵심 구현은 [Recorder qualification](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs:483), [Recorder cleanup policy](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs:33), [Group Stop fallback](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/GroupStopQualificationOrchestrator.cs:121)에 있습니다.

다음 구현 단위는 Bulk cancel/partial cleanup과 Recorder Stop-race/cancel/release-failure production orchestration입니다. LASAL 소스를 변경하는 시점에만 합의한 순서대로 메인 프로젝트 빌드를 요청하겠습니다.

<details><summary>18 previous messages</summary>

> 다음 구현 단위로 바로 넘어갑니다. 먼저 현재 WPF의 Bulk/Recorder cleanup 경로와 설계 문서의 미완료 조건을 다시 대조한 뒤, 공통 orchestration을 테스트 가능한 형태로 추출하고 취소·부분 성공·Stop race·Release 실패를 가짜 RPC로 검증하겠습니다. 이번에도 LASAL 소스나 테스트 폴더는 건드리지 않습니다.
>
> 감사 결과 실제 결함이 확인됐습니다. Bulk/Recorder 모두 Release 실패 시 qualification의 지역 handle이 사라져 같은 세션에서 수동 재시도할 수 없고, 기존 테스트는 WPF cleanup 상태기를 호출하지 않았습니다. 우선 ownership을 보존하는 공통 orchestration으로 세 중복 경로를 통합하고, 그 동일 소스를 fake-RPC가 실행하도록 수정하겠습니다.
>
> <details><summary>Created 2 files, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/BulkQualificationCleanupOrchestrator.cs` (+67 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderQualificationCleanupOrchestrator.cs` (+317 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+65 -57)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+15 -98)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+166 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+13 -119)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+10 -110)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+0 -51)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderQualificationCleanupOrchestrator.cs` (+21 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+40 -31)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+37 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+57 -38)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+0 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+0 -6)
>
> </details>
>
> 공통 orchestration을 production 코드에 연결했습니다. 이제 WPF를 먼저 컴파일해 signature·async 흐름 오류를 제거한 뒤, 같은 helper를 사용하는 fake-RPC fault matrix를 추가하겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/BulkQualificationCleanupOrchestratorTests.cs` (+364 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/BulkQualificationCleanupOrchestratorTests.cs` (+0 -1)
>
> </details>
>
> Bulk 공통 cleanup 경로의 실제 fake-RPC 3종은 `179/179`로 통과했습니다. 이제 Recorder의 pre-start/active/Uploading 취소, Stop `InvalidState` race, unsafe Fault, buffer/config Release 실패와 재시도를 동일 production helper로 검증합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+337 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+188 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+1 -0)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderQualificationCleanupOrchestrator.cs` (+54 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+19 -32)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+87 -0)
>
> </details>
>
> 독립 리뷰에서 공통 helper 자체 오류는 없었지만 Bulk 복구 UX의 경계가 남았습니다. Release 실패로 보존된 configuration은 일반 읽기용이 아니라 cleanup 재시도 전용으로 표시하고, 첫 Release 실패 후 같은 handle로 두 번째 Release가 성공하는 wire 시험까지 추가하겠습니다. 취소 후 cleanup 실패 로그도 최종 FAIL로 일관되게 정리합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+12 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 말한 순서로 고정하겠습니다. 주 저장소만 계속 수정하고, LASAL 소스가 바뀌는 시점에만 빌드를 요청하겠습니다. 지금 진행 중인 C#/WPF 정리부터 마무리하고 검증한 뒤, LASAL 변경 여부와 다음 테스트 항목을 명확히 나누어 보고하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+42 -29)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/BulkQualificationCleanupOrchestratorTests.cs` (+16 -1)
>
> </details>
>
> Bulk 쪽은 취소/오류 본문 결과와 cleanup 최종 결과가 서로 모순되지 않도록 로그를 분리했고, Release 실패 후 같은 핸들로 재시도할 수 있게 복구 상태도 명시했습니다. 병렬 리뷰에서 Recorder Adopt 검증 실패 시 새 세션의 소유권 핸들이 유실될 수 있는 P1 경로가 추가로 확인되어, 지금 그 경로까지 막고 있습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs` (+24 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+26 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+56 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderQualificationCleanupOrchestrator.cs` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+14 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Bulk.cs` (+17 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderQualificationCleanupOrchestrator.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+171 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+41 -0)
>
> </details>
>
> Recorder 쪽은 단순히 핸들을 남기는 데서 끝내지 않고, 검증 실패 핸들을 격리했습니다. 이 상태에서는 자동 Stop/Release를 보내지 않고, 수동 Status 확인 전에는 Release도 비활성화되며, 확인 후에도 Release만 허용됩니다. 마지막 Recorder RPC 중 취소가 들어오는 경계와 adopted identity 기반 Release 재시도 회귀 테스트도 추가했습니다. 이제 실제 빌드로 컴파일·계약 오류를 잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+53 -26)
>
> </details>
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/BulkPartialQualificationAnalysis.cs` (+266 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/BulkPartialQualificationAnalysis.cs` (+18 -13)
>
> </details>
</details>

<details><summary>83 previous messages</summary>

> 이어서 진행합니다. 중단 직전까지 cleanup 안전성 수정과 191개 자동 테스트는 통과했고, 다음 구현인 “한 축 EtherCAT slave offline → Partial 확인 → 복구 확인” 수동 checkpoint 흐름을 작성하던 상태였습니다. 현재 워크트리를 다시 확인한 뒤 UI·분석 helper·자동 테스트·문서를 한 묶음으로 완성하겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/BulkPartialQualificationAnalysisTests.cs` (+275 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)

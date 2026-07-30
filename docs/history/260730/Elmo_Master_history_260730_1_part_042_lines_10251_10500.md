> 계속 진행하겠습니다. CREVIS 실시간 경로는 LASAL IDE 구조가 생기기 전까지 capability를 켜지 않고, 그동안 외부 편집만으로 완료할 수 있는 Recorder 동시성 회귀검증과 WPF recovery 경로의 남은 결함을 우선 정리하겠습니다. 먼저 현재 소스·남은 구현 계획·작업 트리를 다시 대조해 다음 변경 범위를 확정합니다.
>
> 소스 재점검 결과, 다음 변경은 두 갈래로 확정했습니다. 먼저 Recorder의 동일 handle 동시 Start와 네 종류 Release의 `BeforeWire` 롤백을 직접 검증합니다. 이어서 현재 gate가 닫혀 있어 숨겨진 수동 Double Configure 오배선(`ConfigureRecorderAsync` 호출)을 구조적으로 분리해, 향후 gate 변경만으로 잘못 열린 UI가 생기지 않도록 막겠습니다. 기존 `ResultDiscarded` 8개 검사는 이미 충분하므로 중복 추가하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+420 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Recorder 동시 실행과 Release BeforeWire 롤백 회귀 테스트를 추가했고, 지금 SDK Release 전체 테스트 결과를 확인 중입니다. 통과시키고 나면 Double Recorder 수동 경로가 일반 Configure로 잘못 들어가지 않도록 구조를 분리하겠습니다.
>
> SDK 신규 회귀 2개를 포함해 Release 전체 `711/711`이 통과했습니다. 이제 수동 Double Configure는 일반 `0x7E40` 경로와 컴파일 구조상 분리하고, durable recovery 경로가 완성되기 전에는 어떤 Configure도 송신하지 않는 fail-closed 계약으로 고정합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+58 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+75 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> Double 경로를 더 확인해 보니 중요한 구조 위험이 하나 더 있었습니다. Double 결과를 일반 `recorderConfiguration` 필드에 공통 대입하면 향후 durable journal의 same-session cleanup admission과 충돌할 수 있으므로, 표준 경로만 일반 필드에 넣고 Double 경로는 별도 retained scope만 쓰도록 호출 구조까지 분리해서 고치겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+6 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+20 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+219 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+5 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+6 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+20 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -1)
>
> </details>
>
> 실제 MainWindow에서 delayed Configure ACK 뒤 safety reservation이 들어오는 경우도 추가했습니다. accepted configuration이 일반 성공처럼 소실되지 않고 recovery-only 상태로 보존된 뒤, 명시적 Release로 정리되는 전체 흐름까지 Release `69/69`으로 통과했습니다. 이제 Debug 재검증과 문서 수치/남은 CREVIS 경계를 맞춥니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+6 -2)
> - Wrote `./LMC_Library/README.md` (+5 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+7 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+6 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 소스를 다시 기준으로 잡고 두 항목을 분리해 처리하겠습니다. SDO Write 편집 잠금은 GUI 동작으로 끝까지 검증하고, CREVIS는 “설정상 노드 표시”와 “PLC에서 올라오는 실제 I/O 값”을 구분해 누락 경로를 구현 가능한 데까지 이어가겠습니다. LASAL IDE는 건드리지 않습니다.
>
> 현재 코드 기준으로 SDO 편집 잠금 문제는 이미 재현 테스트까지 들어가 있습니다. 일반 RPC가 진행 중이어도 Slave/Index/SubIndex/Type/Length/Timeout/Write Value를 바꿀 수 있고, 변경하면 기존 Arm만 무효화되며 전송 중인 immutable request에는 영향이 없도록 되어 있습니다. 이제 CREVIS가 안 보이는 원인을 PLC capability/route와 GUI 표시 중 어느 쪽인지 소스 기준으로 좁히고 있습니다.
>
> 마지막 정적 검토에서 현재 기능 오류는 없었지만, 동시성 시험의 sync↔async 교차 조합과 WPF README의 오래된 문구가 비어 있는 것을 찾았습니다. 이 두 가지는 지금 바로 보완한 뒤 같은 테스트를 다시 돌리겠습니다. CREVIS live 값 자체는 GUI 패치로 만들 수 있는 문제가 아니며, LASAL IDE 구조 3개 client/1개 handler가 선행되어야 한다는 점도 확정됐습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+19 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
>
> </details>
>
> 보완은 끝냈습니다. SDK 전체 PC 테스트를 Debug/Release로 다시 실행해 교차 조합이 실제로 통과하는지 확인 중입니다. WPF 소스는 이번 보완에서 바뀌지 않았고, 앞서 Debug/Release 69/69가 통과한 상태입니다.
</details>

반영 완료했습니다.

- SDO Write는 Arm/Confirm 방식으로 변경했습니다. 전송 중에도 모든 입력값을 편집할 수 있으며, 편집하면 기존 Arm만 해제됩니다.
- CREVIS configured topology는 연결 후 자동 로드되어 7행/CREVIS 3행이 표시됩니다.
- GUI 제목에 `[CREVIS topology / editable SDO draft]`가 없으면 이전 빌드를 실행 중인 것입니다.
- CREVIS 행도 완전히 비어 있다면 PLC/WPF 실행본 불일치 또는 topology load 실패입니다.
- 실제 CREVIS Health/DI/DO 값은 아직 올라올 수 없습니다. LASAL에 `0x7E13/0x7E22/0x7E23`과 capability bits 15~17, CREVIS RT owner 구조가 없습니다.

다음 작업을 위해 사용자가 LASAL IDE에서 [T2 IDE 구조 인계 문서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)대로 client 3개, handler 선언 1개, Network 연결 3개만 생성해 주세요. 생성 후 알려주면 외부 편집으로 live snapshot과 `0x7E13/0x7E22`를 이어 구현하겠습니다.

검증 결과:

- SDK Debug/Release: 각각 `711/711 PASS`
- WPF Debug/Release: 각각 `69/69 PASS`
- `git diff --check`: PASS
- staged check는 기존 `docs/history/Elmo_Master_history_260721.md` 공백 때문에 실패하며 건드리지 않았습니다.
- 커밋·스테이징·테스트 폴더 복사는 하지 않았습니다.

<details><summary>29 previous messages</summary>

> CREVIS live 경로는 IDE 구조가 생길 때까지 대기하되, 개발을 멈추지는 않겠습니다. 다음으로 gate를 계속 닫아 둔 상태에서 Recorder Double의 “Configure만 수행한 뒤에도 재시작 복구 가능한 durable adapter”를 구현하고, accepted-result/응답 유실/명시 Release까지 자동 테스트로 고정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이어서 Recorder Double의 “Configure만 실행하고 복구 가능한 상태로 보존” 경로를 구현하겠습니다. UI/배선 게이트는 계속 닫아 둔 채 내부 계약과 자동 테스트만 완성하고, CREVIS는 사용자 쪽 LASAL IDE 구조 작업이 끝난 뒤 연결하겠습니다.
>
> 구조 감사 결과, 기존 Double 전체 시퀀스를 재사용하면 Configure 직후 Start까지 나가므로 위험합니다. 공통 오케스트레이터에 `ARM → CONFIGURE → CHECKPOINT → RETAIN` 전용 경로를 추가하고, Start 관련 delegate는 호출되면 즉시 실패하도록 분리하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleBankQualificationOrchestrator.cs` (+67 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleBankQualificationOrchestratorTests.cs` (+110 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+3 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+443 -0)
>
> </details>
>
> 첫 WPF 확인은 임시 `OutputPath`를 강제로 바꾸자 XAML 생성 경로가 깨져 `InitializeComponent` 계열 오류가 연쇄 발생했습니다. 코드 오류로 판단하지 않고, 사용자 Debug 실행물과 겹치지 않는 Release 구성으로 다시 컴파일하겠습니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleBankQualificationOrchestratorTests.cs` (+9 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+798 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+9 -3)
>
> </details>
>
> 어댑터와 네 가지 실통신 스모크 케이스(정상 보존·명시적 정리, accepted-result 선점, Configure 응답 손실, Release 응답 손실/재전송 금지)가 컴파일됐습니다. 이제 전체 WPF 스모크를 돌려 기존 기능 회귀와 새 수명주기 상태를 같이 확인합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+35 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+18 -14)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 응답 유실 테스트에서 남는 것은 구현 오류보다 테스트 종료 시 3개 저널 락을 모두 닫지 못한 문제로 좁혀졌습니다. 테스트 정리 루틴을 먼저 바로잡은 뒤, 새 Double 경로 4건과 전체 SDK/WPF 회귀를 다시 확인하겠습니다. CREVIS 실경로 게이트는 계속 닫아둡니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+16 -3)
>
> </details>
>
> 저널 락 문제는 해결됐고, 응답 유실·재전송 차단을 포함한 3개 경로가 통과했습니다. 남은 1건은 accepted-result 선점 테스트가 응답 배리어에 도달하기 전에 대기 구조에서 막히는 테스트 하네스 문제라서, 실제 송신 순서와 Dispatcher 펌프 방식을 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+4 -3)
>
> </details>
>
> 새 Manual Double 5건은 모두 통과했습니다. 이제 전체 WPF 회귀와 SDK 빌드를 병렬로 돌리고, 이어서 SDK 전체 테스트 및 Release 구성을 확인하겠습니다. 이 단계도 PLC 실기 검증이 아니라 PC 정적·가짜 RPC 검증입니다.
>
> 전체 결과는 WPF Release 73/73, SDK Debug·Release 각각 714/714입니다. WPF Debug 실행 파일은 사용자가 현재 실행 중이어서 덮어쓰지 않았습니다. 지금은 문서 숫자·구현 상태를 맞추기 전에 변경분 계약 리뷰와 작업 트리 경계를 다시 확인하고 있습니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+13 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -2)
> - Wrote `./LMC_Library/README.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -1)
>
> </details>
>
> 계약 리뷰에서 실제 경계 문제가 하나 확인됐습니다. WPF가 capability A로 저널을 arm한 뒤 SDK가 Configure 직전에 capability B를 다시 읽기 때문에, 그 사이 BootId/MapRevision이 바뀌면 정확한 handle은 생겨도 저널 A와 분리될 수 있습니다. 동일 응답만 쓰던 테스트를 보강하고, wire 전에 같은 capability identity를 강제하도록 수정하겠습니다. UI cleanup 게이트 의존성도 함께 분리해서 확인 중입니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+80 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+25 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+12 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+40 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleBankQualificationOrchestrator.cs` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+46 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+18 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+77 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+15 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+103 -0)
>
> </details>
>
> 리뷰 지적을 반영해 경계를 강화했습니다. 이제 capability snapshot을 SDK에서 소유자·세션까지 검증한 뒤 저널을 arm하고, 같은 snapshot의 BootId/MapRevision으로만 Configure를 보냅니다. 설정 크기·trigger·bank byte 제한 같은 결정적 오류는 저널 생성 전에 차단합니다. config-only cleanup은 full qualification 게이트와 분리된 manual route로 정리할 수 있게 정책도 나눴습니다. 지금 빌드로 계약을 확인합니다.
>
> 보강 후 전체 WPF Release 74/74가 통과했습니다. invalid capacity는 capability 조회까지만 하고 `0x7E4C`와 저널을 모두 0건으로 유지했고, Window trigger의 비기본 필드도 그대로 복제됐습니다. 이제 SDK Release 전체와 문서 숫자를 최종 정합화합니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이번 단계에서는 Double 구성 전용 복구 경로의 검증을 마무리하고, 현재 소스 기준으로 CREVIS 표시·제어 경로에서 IDE 구조 없이도 진행 가능한 부분을 바로 이어서 구현하겠습니다. LASAL IDE는 건드리지 않고 외부 소스만 수정하며, 현재 실행 중인 WPF Debug는 그대로 두겠습니다.
>
> 현재 작업 트리의 일반 diff는 공백 오류 없이 통과했습니다. staged 검사는 제가 건드리지 않은 기존 히스토리 파일의 trailing whitespace 때문에만 실패하며, 테스트 수 표기는 모두 최신 값으로 정리돼 있습니다. 이제 CREVIS 데이터가 GUI에 실제로 도달하는 체인과 남은 LASAL/API 경계를 병렬로 다시 추적하겠습니다.
>
> 재검토에서 실제 권한 경계 문제 1건이 확인됐습니다. config-only 표시가 변경 가능한 bool이라 잘못 분류되면 더 약한 cleanup gate를 선택할 수 있었습니다. 이를 생성 시 고정되는 scope 종류로 바꾸고, bank/start 흔적이 하나라도 있으면 config-only cleanup을 wire 전 차단하도록 바로 닫겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+19 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+10 -1)

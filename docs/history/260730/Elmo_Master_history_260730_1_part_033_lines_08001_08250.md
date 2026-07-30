현재 CREVIS 경계는 명확합니다.

- 최신 LASAL 소스를 빌드·다운로드하면 정적 CREVIS 구성 3행은 표시돼야 합니다.
- 실제 CREVIS Health/DI는 아직 표시되지 않습니다. `0x7E13/0x7E22` route와 capability bit 15/16이 없어서 `IntegratedReadOwnerDormant` 검사가 예상대로 실패했습니다.
- 다음 작업은 [T2 LASAL IDE 구조 인계 문서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)를 따라 사용자가 IDE 구조를 생성한 뒤 제가 외부에서 implementation을 작성하는 단계입니다.
- 실제 SDO Write도 현재 SDK allowlist와 PLC gate가 닫혀 있어 편집 UI만 검증됐고 wire Write는 아직 차단 상태입니다.

이번 LASAL 빌드 대상 변경은 [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1568)입니다. 빌드·다운로드 후 먼저 `Reload CREVIS / Topology` 결과와 D1 오프라인 시험을 확인하면 됩니다.

<details><summary>19 previous messages</summary>

> 다음 증분은 현재 코드상 가장 명확한 미완료인 D5 disconnect/orphan WPF 연결로 진행하겠습니다. 코어는 이미 검증됐으므로, WPF가 연결을 직접 끊지 않고 사용자 외부 단절을 관측하게 만들고 capability·quarantine·복구 로그를 기존 qualification 흐름에 안전하게 연결한 뒤 smoke와 전체 계약을 갱신하겠습니다. CREVIS T2 구조는 LASAL IDE 작업 전까지 건드리지 않습니다.
>
> 실제 WPF 연결에서 중요한 제약을 확인했습니다. 앱이 소켓을 닫아서는 고아 증거가 되지 않으므로, runner는 사전 준비 확인 → old ticket 제출 → 외부 단절을 status polling의 transport fault로 관측 → 사용자가 transport를 복구한 뒤 새 `LMCConnection`으로 두 번 읽는 순서가 되어야 합니다. terminal-before-loss와 Queued-only는 전체 PASS로 올리지 않고 INCONCLUSIVE로 분리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 확인했습니다. 지금 보이는 두 문제를 우선 반영하겠습니다.
>
> - CREVIS 정보가 GUI에 실제로 표시되는 경로를 점검해, 아직 PLC 측 미구현 데이터라면 “없는 값을 표시”하지 않고 현재 확보 가능한 토폴로지/PDO 정보와 미지원 상태를 명확히 구분하겠습니다.
> - SDO Write 실행 중에도 다음 요청의 Index/SubIndex/Type/Value 등을 편집할 수 있도록, 실행 중 잠금 범위를 해당 요청 스냅샷과 실행/취소 버튼으로만 좁히겠습니다.
>
> 진행 중이던 D5 작업은 이 변경과 충돌하지 않는 범위에서 병행하되, 우선 두 GUI 증상을 재현하고 수정하겠습니다.
>
> 소스 기준으로는 원인이 두 갈래로 확인됐습니다.
>
> - SDO 입력은 일반적인 송신 대기 중에는 이미 편집 가능하지만, Write 완료 직후의 “정확한 readback 확인 대기” 상태에서 전체 입력을 강제로 고정하고 매 화면 갱신마다 값을 다시 덮어쓰고 있습니다. 이 잠금을 없애고, 필요할 때만 원래 readback 값을 다시 불러오는 별도 버튼으로 바꾸겠습니다. 실행 중 요청 자체는 이미 로컬 객체로 복사되므로 편집해도 진행 중 요청은 변하지 않습니다.
> - CREVIS는 현재 소스에 정적 구성 토폴로지 3개 행(커플러+슬롯 2개)을 올리는 경로가 있지만, 실제 health/DI 값은 아직 LASAL IDE 구조 작업 전이라 제공되지 않습니다. 우선 “구성 정보도 안 보이는 문제”와 “실시간 값이 미지원인 상태”가 GUI에서 확실히 구분되도록 경로와 오류 표시를 보강하고 있습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+28 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+82 -21)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/SdoEditorAvailabilityPolicyTests.cs` (+6 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoEditorAvailabilityPolicy.cs` (+5 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+2 -2)
>
> </details>
>
> CREVIS 쪽도 원인이 확정됐습니다. 기존 `0x7E10` 표는 프로토콜 자체가 4개 Elmo의 고정 슬롯 `0..3`만 내보내므로, 노드 순서를 바꿔도 그 표는 그대로 보이는 것이 현재 계약상 정상입니다. 그래서 해당 표에 실제 구성 토폴로지에서 찾은 `CFG slave` 열을 추가했고, CREVIS 로드 버튼과 상태를 탭 맨 위로 올렸습니다.
>
> 실시간 CREVIS Online/DI 값은 아직 PLC의 `0x7E13/0x7E22` 경로가 없어 표시할 수 없습니다. 이 값은 LASAL IDE에서 필요한 채널/네트워크 구조를 먼저 생성한 뒤 구현해야 하며, 현재 변경에서는 정적 구성 정보와 실시간 미지원 상태를 숨기지 않고 구분해 표시합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+30 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+4 -0)
>
> </details>
>
> GUI 소스 수정은 끝났고 현재 컴파일 검증에 들어갑니다. 창 제목에도 `CREVIS topology / editable SDO draft` 표식을 넣었으므로, 이후 사용자가 실행 중인 GUI가 최신 빌드인지 바로 구분할 수 있게 했습니다. SDO의 안전 규칙은 그대로입니다: 편집은 계속 가능하지만 Submit은 현재 요청 종료 및 정확한 readback 일치 조건을 계속 통과해야 합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> GUI 수정은 반영됐고, 지금은 CREVIS 표시 계약과 SDO 편집 동작을 자동 테스트로 고정한 뒤 Debug/Release 전체 검증 및 문서 정합성까지 마무리하겠습니다. 실제 CREVIS live I/O는 PLC 구조가 아직 없는 부분과 분리해서 결과를 보고하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+34 -0)
>
> </details>
>
> 자동 검증은 기존 WPF 실행 파일이 아니라 방금 수정한 소스를 VS2019 MSBuild로 Debug/Release 재빌드한 뒤 각각 실행합니다. SDO 입력 잠금 해제뿐 아니라 변조된 readback 요청이 wire 전송 전에 차단되는지도 함께 확인합니다.
>
> WPF smoke는 현재 실행 중입니다. 앞선 병렬 테스트 프로세스가 Debug 산출물을 잠깐 잡아 재시도 경고가 있었지만 빌드 자체는 성공했고 프로세스는 이미 종료됐습니다. 실행 결과 확인 후 Debug를 한 번 더 깨끗하게 재빌드해 잠금 영향까지 제거하겠습니다.
>
> PC 측 결과는 WPF Debug/Release 17/17, API Debug/Release 575/575까지 통과했습니다. 이제 LASAL 소스는 IDE 빌드로 오인하지 않도록 정적 계약 검사만 수행하고, CREVIS live I/O 구조 미등록은 별도 미완료 항목으로 남기겠습니다.
>
> 리뷰에서 실제 남은 덮어쓰기 경로를 하나 찾았습니다. 일반 편집 잠금은 풀렸지만, 허용 대상이 있는 배포 빌드에서는 capability/topology 재조회 응답이 도착할 때 선택 대상과 SDO draft를 첫 항목으로 되돌릴 수 있습니다. 이 경로도 제거하고 회귀 테스트를 추가하겠습니다. CREVIS capability가 같은 연결 안에서 바뀔 때 상단 요약이 과거 live 상태를 남길 수 있는 문제도 같이 정리합니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+68 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+149 -37)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+33 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+30 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+24 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+8 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+8 -6)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -6)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+17 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+6 -0)
>
> </details>
>
> 두 잔여 경로까지 반영했습니다. 승인 target이 있는 빌드에서도 capability 갱신은 dropdown만 재바인딩하고 작성 중인 SDO 값은 건드리지 않으며, 사용자가 target을 직접 바꿀 때만 target 값이 적용됩니다. CREVIS live capability가 내려가면 과거 표본은 `UNAVAILABLE`로 바뀌고 상단 요약도 즉시 갱신됩니다. 이제 최종 Release 재검증을 진행합니다.
>
> 이번 GUI 증분은 완료했습니다. 최종 결과는 “정적 CREVIS 구성은 GUI에서 즉시 보임, live CREVIS 값은 PLC bit 15/16 및 IDE 구조가 생기기 전까지 미표시”로 명확히 분리됐고, SDO 편집은 동작 중·readback 대기 중·capability 갱신 중 모두 draft가 보존됩니다. 마지막으로 변경 위치와 실행 파일을 정리해 넘기겠습니다.
</details>

반영 완료했습니다.

- CREVIS 정보를 상단에 노출하고, legacy Elmo 표에 현재 `CFG slave`를 추가했습니다. [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:784)
- capability bit 15/16이 내려가면 이전 LIVE 값은 `UNAVAILABLE`로 폐기됩니다. [MainWindow.TopologyIo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs:1749)
- SDO 동작·readback 대기·capability 갱신 중에도 입력 draft가 유지됩니다.
- `Load Required Exact Readback` 버튼으로 필요한 Read 요청만 명시적으로 복원합니다. 다른 요청은 wire 전 차단됩니다. [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1095)

실행은 [Debug GUI](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)로 확인하면 됩니다. 창 제목에 `[CREVIS topology / editable SDO draft]`가 보여야 최신 빌드입니다.

검증 결과:

- WPF Debug/Release: `17/17 PASS`
- API Debug/Release: `575/575 PASS`
- LASAL `StaticTopologyOnly`: PASS
- `git diff --check`: PASS
- 커밋·스테이징은 하지 않았습니다.
- 기존에 staged된 대형 history 파일의 trailing whitespace 때문에 `git diff --cached --check`만 실패합니다.

주의할 점은 명확합니다. `Configured CREVIS entries=3`은 PLC가 bit 14를 광고하면 표시됩니다. 실제 CREVIS Health/DI는 아직 PLC bit 15/16과 IDE 구조가 없어서 올라오지 않는 것이 현재 정상입니다. 필요한 LASAL IDE 작업은 [T2 IDE handoff](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md)에 정리돼 있습니다.

<details><summary>37 previous messages</summary>

> 다음 구현은 현재 계획대로 D5 SDO의 외부 연결 단절/orphan 검증 경로입니다. 먼저 현재 orchestrator가 어디까지 실제 증거를 소유하는지와 PLC에서 외부 편집만으로 추가 가능한 증거 지점을 병렬 점검한 뒤, “앱 추정”을 PASS로 만들지 않는 형태로 adapter와 테스트를 이어 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금은 CREVIS/SDO GUI 수정에 이어서 D5 외부 연결 단절 증명을 보강하는 중입니다. 우선 병렬 감사 결과를 회수하고, PLC가 실제 orphan 전이를 증명하지 못하면 GUI가 성공으로 판정하지 않도록 fail-closed 계약부터 적용하겠습니다.
>
> 감사 결과가 일치합니다. 현재 코어는 호출자가 준 `bool`과 직전 `Running`만으로 orphan 성공을 만들 수 있어 잘못된 PASS가 가능합니다. 먼저 이 경계를 닫고, GUI에는 `0x405D`를 보내지 않는 로컬 TCP 강제 단절 후 새 연결 2회 읽기 복구를 넣되 결과는 PLC 증거가 생길 때까지 반드시 `ApplicationRecoveryOnly`로 표시하겠습니다.
>
> <details><summary>Created a file, edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+6 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+11 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+11 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/Properties/AssemblyInfo.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+39 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+35 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+41 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+29 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+572 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+12 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+6 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+9 -0)
>
> </details>
>
> 코어의 false PASS는 닫았고, GUI 버튼과 qualification 전용 TCP 강제 단절 경로까지 연결했습니다. 지금부터는 먼저 C# 컴파일/계약 테스트로 소유권 전환·UI 스레드·프로젝트 포함 누락을 잡고, 통과한 뒤 문서와 남은 PLC IDE 작업 경계를 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+1 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이번 증분은 GUI에서 실제로 실행 가능한 “강제 TCP 단절 → 새 연결 복구”까지 들어간 상태입니다. 다만 PLC 내부 orphan 판정 증거가 없으므로 결과는 의도적으로 `orphanQualified=false`로 고정했습니다. 지금 코드 리뷰 결과를 회수한 뒤 문서와 Release 검증까지 맞추겠습니다.
>
> 리뷰에서 바로 고쳐야 할 결함이 확인됐습니다. 현재 초안은 성공 증거를 지우기 전에 PASS 로그를 남기지 않았고, 새 연결 직후 CREVIS 토폴로지 자동 로드도 빠져 있습니다. 이 두 부분과 capability-off 강제 실행의 zero-wire 보장을 먼저 보강하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+85 -25)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+15 -0)
>
> </details>
>
> 핵심 보강 후 Debug 빌드는 통과했습니다. 이제 강제 handler 호출에서도 capability가 없으면 요청 수가 한 건도 늘지 않는 smoke와, 성공 후 CREVIS 자동 재로딩 경로를 검증하고 있습니다.
>
> 추가 리뷰에서 안전 경합도 확인됐습니다. 상태 응답 직후 Stop/PowerOff가 예약되면 기존 초안은 안전 명령보다 먼저 TCP를 닫을 수 있었습니다. 강제 단절 직전에도 cancellation과 safety generation을 다시 확인하고, 같은 송신 직렬화 gate 안에서 abort하도록 수정하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+47 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+45 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+31 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+35 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+41 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+80 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+101 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+87 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+36 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+22 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+38 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+8 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+26 -1)
>
> </details>
>
> 실제 소켓 검증에서 중요한 차이가 나왔습니다. Windows loopback 수신측은 zero-linger를 설정해도 항상 `ConnectionReset` 예외로 관측하지 않고 EOF로 정규화할 수 있었습니다. 따라서 코드는 zero-linger 요청을 유지하되, 자동 테스트는 “zero-linger 설정 확인 + 0x405D 미전송 + 연결 종료”까지만 증명하도록 표기를 정확히 낮춥니다. 실제 RST wire 판정은 pcap 영역입니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+2 -36)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+10 -13)
>
> </details>
>
> 코드 쪽은 Debug 기준 API `582/582`, WPF `18/18`까지 통과했습니다. 동시에 문서를 최신 수치와 정확한 증명 경계로 갱신하고, PLC 내부 orphan 증거를 추가할 때 LASAL IDE에서 만들어야 할 선언 항목을 별도 인계 문서로 정리하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> GUI 요청 사항은 이미 반영된 상태로 유지하면서, 현재는 D5 연결 단절 후 복구 경로의 남은 문서·증거 계약과 최종 빌드 검증을 정리하고 있습니다. LASAL IDE는 제어하지 않고, 외부 소스와 정적 계약까지만 진행하겠습니다.
>
> 현재 구현의 의미를 코드 이름까지 맞추겠습니다. 지금 경로는 “외부 케이블 단선 증명”이 아니라 “로컬 zero-linger 전송 종료 후 새 세션 복구”이므로, 내부 명칭도 `ExternalLoss` 대신 `TransportLoss`로 정리하고 PLC 측 orphan 증거는 별도 LASAL IDE 인계 문서로 분리하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+15 -15)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+17 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+6 -6)
>
> </details>
>
> 리뷰에서 실제 모순 가능성 두 건이 잡혔습니다. 복구 증거를 이미 커밋한 뒤 취소 요청이 들어오면 `PASS_APPLICATION_RECOVERY` 다음에 `ABORTED`가 찍힐 수 있었고, 동일 Boot/Map에서도 PLC cycle/payload 계약이 바뀌는 경우를 놓쳤습니다. 두 경로를 모두 fail-closed로 고치고 회귀 테스트를 추가합니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+2 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+37 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoDisconnectOrphanQualificationOrchestrator.cs` (+23 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoDisconnectOrphanQualificationOrchestratorTests.cs` (+78 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.D5Disconnect.cs` (+15 -1)

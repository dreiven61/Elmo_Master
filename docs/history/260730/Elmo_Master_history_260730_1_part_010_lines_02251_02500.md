>
> 내가 빌드 및 오류 확인
>
> 내가 테스트 폴더로 코드 복사
>
> 테스트 폴더에서 테스트 진행.
>
> 그 순서로 고정하겠습니다.
>
> 1. 제가 메인 저장소에서 개발·정적 검증
> 2. LASAL 변경분이 준비되면 제가 빌드를 요청
> 3. 사용자가 메인 프로젝트를 빌드하고 오류 결과 전달
> 4. 빌드 통과본을 `C:\work\Elmo\Elmo_Master_test\Elmo_EtherCAT_Test_4Axis`로 복사
> 5. 테스트 폴더에서 실제 장비 테스트
>
> 지금 진행할 PC 수신부 보강은 LASAL 빌드가 필요 없으므로 바로 구현합니다. 이후 LASAL 소스를 다시 변경하는 시점에는 변경 파일과 확인할 동작을 묶어서 명확히 빌드 요청하겠습니다. 메인의 `Classes.lcb`는 방금 빌드 과정에서 생긴 것으로 추정되지만, 사용자 산출물로 보존하고 이번 변경에는 포함하지 않겠습니다.
>
> 확인 결과 Phase 5 LASAL 빌드는 단순 사용자 보고만이 아니라 `%TEMP%\Lasal2.log`에서도 Compiler/Linker 성공, ERROR/FATAL 0, 신규 `CInvalidArgException` 0으로 확인됐습니다. 따라서 LASAL은 현재 구동시험 결과를 기다리는 상태로 두고, PC 쪽은 비정상 서버가 응답 길이만 크게 선언해 최대 65,535바이트를 기다리거나 할당하던 경로를 명령별 상한으로 차단하겠습니다. 정상 wire 형식은 바뀌지 않습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+139 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+11 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+68 -0)
>
> </details>
>
> <details><summary>Created 2 files, edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+4 -125)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResponsePayloadLimits.cs` (+145 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponsePayloadLimitTests.cs` (+112 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+29 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResponsePayloadLimits.cs` (+5 -1)
>
> </details>
>
> PC 수신 보강 1차 구현은 완료됐고 Debug 자동 테스트가 기존 148개에서 신규 계약·통합 테스트를 포함한 153/153으로 통과했습니다. 53개 명령 모두 정상 최대 응답 크기를 명시했고, 초과 길이는 body를 읽기 전에 연결을 Faulted 처리하며, 미등록 명령은 송신 전에 거부합니다. 지금 Release와 WPF 양 구성 회귀 및 별도 코드 리뷰를 이어서 확인합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+65 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+21 -13)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+11 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+9 -2)
>
> </details>
>
> 첫 번째 병행 개발 항목은 리뷰까지 끝났고 P0/P1 문제 없이 155/155 Debug·Release를 통과했습니다. 문서에는 새 시험 순서와 LASAL Compiler/Linker 통과 상태도 반영했습니다. 다음은 기존 WPF qualification 구조를 재사용해 Phase 5 read-only 반복 측정/CSV 기능을 붙일 수 있는지 확인하겠습니다. 이 작업도 LASAL 소스와 테스트 폴더를 건드리지 않습니다.
>
> <details><summary>Created a file, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+35 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+425 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+150 -77)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+79 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+47 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+11 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> WPF에는 기존 qualification 실행기와 송신 gate를 그대로 사용한 `0x2045` read-only 반복 측정기를 추가했습니다. 기본 100회 warm-up/10,000회 측정, 1% 단위 UI 갱신, PC RTT의 min/P50/P95/P99/max·throughput, 응답 상태·SHA-256을 CSV로 보존하며 PLC 내부 dispatch 측정이 아니라는 경계도 화면과 로그에 표시했습니다. Debug 빌드는 통과했고, 지금 별도 리뷰와 Release 확인을 진행합니다. 이어 캡처 70건으로 확정된 `AxisInfo` descriptor 검증도 PC 쪽에서 보강하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 그 순서로 고정하겠습니다. 저는 메인 저장소만 개발하고, LASAL 변경분이 빌드 가능한 단위가 될 때만 빌드를 요청하겠습니다. 테스트 폴더에는 손대지 않겠습니다. 지금은 LASAL과 충돌하지 않는 PC/WPF 쪽 안전성·측정 기능을 마무리한 뒤, 메인 저장소 기준 검증 결과와 복사할 범위를 분명히 알려드리겠습니다.
>
> PC/WPF 쪽 1차 구현은 컴파일까지 통과했습니다. 현재 추가한 것은 응답 크기 상한 방어와 0x2045 기반 10,000회 읽기 전용 RTT 측정 기능입니다. 이는 PLC 구동을 바꾸지 않으며, 수치는 PC 왕복시간으로만 표시해 PLC task jitter/overrun과 혼동되지 않게 했습니다. 동시에 캡처에서 확인된 AxisInfo 축 번호 불일치 검증도 회귀 테스트와 함께 마무리 중입니다.
>
> 리뷰에서 실제 시험 오판 가능성이 있는 네 가지를 잡았습니다. 외부 동작 중 실행 방지, 측정 범위 표기, 상태값 변화의 판정 기준, 실패 실행의 CSV 보존 문제입니다. 그대로 두지 않고 수정한 뒤 다시 빌드·검토하겠습니다. 특히 상태 응답은 동작 중에도 정상적으로 바뀔 수 있으므로 “바이트가 다르면 무조건 실패”로 단순 처리하지 않고, 정지 조건과 전송 건전성을 분리해 판정하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+88 -14)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> 현재 메인 WPF 실행 파일과 DLL이 Visual Studio/실행 중인 테스트 앱에 잠겨 있어서 기본 출력 폴더 빌드는 중단됐습니다. 실행 중인 프로그램은 종료시키지 않겠습니다. 사용자 테스트를 방해하지 않도록 임시 출력 폴더로 빌드 검증을 계속합니다. 소스 컴파일 오류가 난 것은 아닙니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+14 -10)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+15 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
</details>

정한 작업 순서로 진행하겠습니다. 테스트 폴더는 수정하지 않습니다.

이번 개발분:

- 53개 명령별 응답 크기 상한 및 비정상 연결 차단 구현: [LmcConnection.cs](./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs:420)
- `AxisInfo` 응답 축 번호 불일치 검증 구현: [LmcAxis.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxis.cs:111)
- 최소 10,000회 `0x2045` 읽기 전용 API RPC 측정·CSV 저장 기능 구현: [MainWindow.Qualification.Transport.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs:70)
- 실행 전/중 InPosition 확인, 응답 변경 감지, 부분 실패 CSV 보존까지 반영했습니다.

검증 결과:

- PC 자동 테스트 Debug/Release: 각각 `157/157 PASS`
- WPF Debug/Release: 임시 출력 폴더 빌드 PASS
- 최종 코드 리뷰: P0/P1 없음
- `git diff --check`: PASS

현재 WPF가 실행 중이라 기본 `bin` DLL은 잠겨 있습니다. 실행 프로그램은 종료하지 않았으며, 새 측정 화면은 나중에 WPF를 종료하고 다시 빌드해야 반영됩니다.

이번에는 LASAL 소스를 변경하지 않았으므로 지금 LASAL 빌드·테스트 폴더 복사는 필요 없습니다. 현재 진행 중인 간단 구동 테스트를 계속하면 됩니다.

커밋은 기존에 staged된 53,548줄짜리 `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에 `git diff --cached --check`가 실패하여 보류했습니다. 해당 파일은 임의로 수정하거나 stage 해제하지 않았습니다.

<details><summary>19 previous messages</summary>

> 현재 소스와 설계 문서를 다시 대조해 다음 구현 항목을 고르겠습니다. 사용자 장비 시험과 충돌하지 않도록 우선 PC/WPF 또는 외부 편집만으로 닫을 수 있는 항목을 진행하고, LASAL 소스 변경이 생기는 시점에만 빌드를 요청하겠습니다. 테스트 폴더와 실행 중인 LASAL/WPF는 건드리지 않습니다.
>
> 현재 기준 문서상 장비 없이 닫을 수 있는 가장 명확한 미완료 항목은 qualification runner의 자동 회귀시험입니다. 반면 `LMCSdoExecutor` 명시적 초기화는 LASAL IDE에서 constructor 선언을 먼저 만들어야 하므로 외부 소스만 임의로 편집하면 안 됩니다. 우선 WPF 측정·판정 로직을 순수 helper로 분리해 실제 사용 코드와 동일한 계산/CSV를 자동 시험하는 방향을 검증 중입니다.
>
> 다음 구현 범위는 확정했습니다. 첫째, `0x2045` qualification의 요청 수·percentile·throughput·SHA-256·부분 실패 CSV 판정을 실제 WPF 코드와 동일한 순수 helper로 분리해 자동 시험합니다. 둘째, UDP callback handler 예외와 callback 내부 재진입 종료가 listener를 죽이거나 deadlock을 만들지 않는지 loopback 회귀시험을 추가합니다. 두 작업 모두 LASAL wire와 테스트 폴더에는 영향이 없습니다.
>
> 두 구현이 소스에 반영됐습니다. 현재 자동 시험 수는 `157 → 167`로 늘었고, callback 재진입 종료·handler 예외 격리와 qualification 판정/CSV 계약을 실제 코드 경로로 검증합니다. 이제 제가 별도 임시 출력 폴더에서 전체 Debug/Release를 다시 실행하고, 문서의 검증 수치와 남은 gate를 현재 상태로 맞추겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+17 -8)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+9 -4)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+7 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 그 순서로 고정하겠습니다. 저는 메인 저장소만 수정하고, LASAL 소스가 바뀐 시점에만 변경 파일과 빌드 확인 항목을 정확히 요청하겠습니다. 테스트 폴더는 건드리지 않겠습니다. 지금은 다음 구현 항목인 Recorder 재연결·Adopt 검증 흐름을 이어서 개발하겠습니다.
>
> 현재 구조를 확인했습니다. 수동 Adopt API는 이미 있지만 자동 검증은 없었습니다. 새 러너는 “Ring 시작 → ID 보존 → 실제 RPC 연결 종료/재연결 → Capability/BootId 재검증 → exact 또는 0/0 discovery Adopt → 필요 시 Stop → Header/Download → 상태 확인 후 Release”를 각각 별도 실행으로 추가하는 방향입니다. 실패·취소 시에도 가능한 경우 exact ID로 재접속 정리를 시도하고, Fault 상태는 자동 Release하지 않도록 유지합니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+15 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+9 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+342 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+551 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+41 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+10 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+30 -23)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+143 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+6 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+8 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+6 -0)
>
> </details>
>
> 구현부는 Debug에서 컴파일됐고, 새 두-세션 회귀 시험도 exact/discovery 모두 포함해 현재 `168/168 PASS`입니다. 이제 Release와 WPF 양 구성 빌드를 별도 출력 폴더에서 확인하고, 리뷰 결과를 반영한 뒤 문서를 현재 상태로 맞추겠습니다. LASAL 소스는 이번 작업에서 변경하지 않았습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+15 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+4 -4)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+32 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+15 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+11 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+2 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+80 -22)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsRecorderContractTests.cs` (+19 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+15 -0)
>
> </details>
>
> 리뷰에서 실제 안전 결함 1건이 확인됐습니다. Adopt 응답이 반환됐지만 보존 ID/새 owner 검증에 실패한 경우, 기존 cleanup이 그 응답 객체를 다시 신뢰해 Stop/Release할 수 있었습니다. 이 경로는 자동 정리를 금지하도록 바꾸고 있습니다. 또한 Status와 Stop 사이에 trigger가 완료되는 경쟁은 `InvalidState` 후 Status 재확인으로 정상 처리하도록 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+65 -18)
>
> </details>
>
> <details><summary>Created 2 files, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderReconnectQualificationPolicyTests.cs` (+63 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs` (+28 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+8 -6)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+3 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 정한 작업 흐름 그대로 유지합니다. 지금은 Recorder 재접속/adopt 구현의 최종 Release 빌드·169개 회귀 테스트·문서 수치·Git 무결성만 다시 확인하겠습니다. 이번 변경에는 LASAL 소스가 없으므로, 확인이 끝나도 LASAL 빌드나 테스트 폴더 복사는 요청하지 않을 예정입니다.
>
> 코드 리뷰도 끝났습니다. 앞서 발견했던 두 문제—검증되지 않은 adopted identity를 정리 대상으로 쓰는 위험과 Stop 직전 상태가 바뀌는 경쟁 조건—은 공용 정책 코드와 결정적 테스트로 막혔고, 재검토에서 새 P0/P1/P2는 나오지 않았습니다. 이제 최종 Debug/Release 결과를 같은 169개 기준으로 고정하겠습니다.
>
> 최종 컴파일 결과는 PC 자동 테스트 Debug/Release 모두 169/169, WPF Debug/Release 모두 성공입니다. 다음으로 변경 소스의 후행 공백과 Git diff를 검사하고, 기존에 이미 stage돼 있던 대용량 히스토리 파일만 별도 문제로 남는지 확인하겠습니다.
</details>

작업 순서를 확정했고, 이번 개발분도 그 기준으로 처리했습니다.

- Recorder Exact/0/0 Discovery 재접속 qualification 구현: [MainWindow.Qualification.Recorder.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs:412)
- WPF 실행 버튼 추가: [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1207)
- 잘못된 identity 자동 정리와 Stop 경쟁 조건 방지: [RecorderReconnectQualificationPolicy.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderReconnectQualificationPolicy.cs:8)
- PC 자동 테스트: Debug/Release 각각 `169/169 PASS`
- WPF: Debug/Release 빌드 성공
- 리뷰 결과: 신규 P0/P1/P2 없음

>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+199 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> 추가한 교체 연결·Init 취소 회귀까지 포함해 Debug는 `391/391 PASS`입니다. 경쟁 테스트가 우연히 한 번만 통과한 것이 아닌지 확인하려고 Debug 전체 suite를 반복 실행하고 있으며, 동시에 독립 리뷰에서 잠금 순서와 stale cleanup 범위를 다시 확인 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
>
> </details>
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+24 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+193 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -0)
>
> </details>
>
> 연결 수명 테스트를 17개까지 확장했습니다. 상태 이벤트에서 `Task.Run`으로 우회한 재진입도 handler가 끝나기 전이면 거부하고, 동시 Close/Dispose는 close frame 1회, callback 종료 대기는 설정된 timeout 안에 끝나는 것으로 고정했습니다. Debug 전체 `395/395`와 3회 반복이 모두 통과했습니다.
>
> 독립 리뷰에서 다중 연결의 중첩 상태 이벤트 한 경우가 더 잡혔습니다. A의 상태 handler가 B를 닫고, B의 handler가 다시 A를 건드리면 현재 표시 중인 B scope가 A scope를 가려 재진입을 허용할 수 있습니다. 상태 scope를 부모 체인으로 바꾸고 A→B→A 회귀를 추가하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+28 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+86 -0)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 자동 표시와 SDO 편집 잠금 문제를 실제 WPF 컨트롤 수준에서 재현하는 테스트를 추가하겠습니다. 우선 기존 가짜 PLC 응답 형식과 UI 실행 경로를 그대로 재사용할 수 있는 최소 테스트 연결부를 확인한 뒤, LASAL IDE는 건드리지 않고 구현·빌드까지 진행하겠습니다.
>
> 테스트 경계가 정리됐습니다. 실제 Connect 버튼과 실제 진단 버튼을 누르는 STA 테스트로 만들고, 테스트 때문에 SDO Write 허용 목록을 열지는 않겠습니다. 대신 일반 진단 요청을 서버에서 대기시킨 동안 Write 입력값을 계속 바꿀 수 있고 Submit만 직렬화되는지를 실제 컨트롤로 확인합니다.
>
> <details><summary>Created 3 files, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/Properties/AssemblyInfo.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+66 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+54 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+636 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.sln` (+6 -0)
>
> </details>
>
> 테스트 프로젝트와 최소한의 journal 격리 생성자를 추가했습니다. 이제 별도 출력 폴더로 빌드해 사용자가 실행 중인 GUI 바이너리와 충돌하지 않게 한 뒤, 두 회귀 시나리오를 실제로 실행하겠습니다.
>
> 첫 실행에서 테스트 자체가 아니라 실제 MainWindow 초기화 결함이 잡혔습니다. XAML이 로딩되는 도중 TextChanged 이벤트가 너무 일찍 전체 UI 갱신을 호출해 아직 생성되지 않은 Recorder 컨트롤을 참조했습니다. 초기화 완료 플래그로 생성 중 이벤트를 차단하는 방식으로 바로 수정합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+5 -0)
>
> </details>
>
> 두 번째 실행에서는 실제 종료 경로 결함도 확인됐습니다. 연결이 이미 끊긴 상태에서 창을 닫으면 `OnClosing` 안에서 즉시 `Close()`를 재호출해 WPF가 예외를 내고 journal lock이 남았습니다. 종료 재호출을 Dispatcher 다음 턴으로 넘겨 정상적으로 `OnClosed`까지 도달하도록 고칩니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
>
> </details>
>
> 두 실제 회귀 테스트가 Debug에서 2/2 통과했습니다. 이 과정에서 CREVIS/SDO 변경 외에 앱 시작 시 조기 이벤트 NRE와 연결 해제 후 창 종료 예외까지 함께 수정됐습니다. 이제 독립 검토와 Release·전체 PC·LASAL 정적 계약을 병행해 최종 상태를 맞추겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+8 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+20 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -3)
>
> </details>
>
> 독립 검토에서도 P1/P2가 없었습니다. 현재 확정된 결과는 PC Debug/Release 396/396, WPF actual-control Debug/Release 2/2, LASAL SourceOnly/full static PASS입니다. CREVIS는 실제 Connect부터 7행·3개 표시까지, SDO는 실행 중 필드 변경·값 유지까지 확인했습니다. 다음 IDE 비의존 구현 항목을 바로 선별 중입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -1)
>
> </details>
>
> 다음 비차단 항목은 parser stress runner로 정했습니다. 기존 고정 변형 테스트는 유지하고, opt-in으로 seed와 반복 수를 지정해 topology/D5 응답을 장시간 변형하며 첫 실패 payload를 그대로 재현할 수 있게 하겠습니다. LASAL/WPF/wire 계약은 바꾸지 않습니다.
>
> Parser stress runner는 전체 반복 수를 6개 parser family에 round-robin 배분하는 방식으로 구현 중입니다. 실행은 stdout/stderr만 사용하고 파일·네트워크는 전혀 건드리지 않으며, seed·iteration·family·payload hex를 첫 실패에 남기는 구조로 제한했습니다.
>
> 기존 parser의 정상/거부 불변식이 family마다 달라 공통화를 먼저 확인하느라 시간이 걸렸습니다. 범위는 6-family, 총 6~1,000,000회, raw 1,572바이트 이하로 고정했고 이제 코드와 빌드 단계로 넘어갑니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 자동 표시와 SDO 편집 잠금 문제는 실제 WPF 컨트롤 기준으로 반영했고, Debug/Release 스모크 테스트까지 통과시켰습니다. 이어서 막혀 있지 않은 다음 작업인 진단 응답 파서 스트레스 도구를 구현하고, 전체 회귀와 고정 시드 1만 회 검증까지 진행하겠습니다. LASAL IDE는 건드리지 않습니다.
>
> 현재 스트레스 도구는 topology info/chunk, node health, DI, DO, D5 variable-inline의 6개 응답만 대상으로 잡았습니다. 입력 크기는 제한하고, `InvalidDataException`으로 명시적으로 거부되는 경우만 정상 처리하며 그 외 예외는 시드·반복 번호·payload hex를 남기고 실패하도록 구성 중입니다. 네트워크나 PLC에는 요청을 보내지 않습니다.
>
> 고정 시드 `0xC0FFEE01`로 변이 10,000회를 돌렸고 현재까지 통과했습니다. 기본 회귀는 새 CLI 검증 3개가 추가되어 396개에서 399개로 늘었고 Debug 399/399도 통과했습니다. Release와 독립 리뷰를 마무리한 뒤 문서의 현행 숫자와 실행법을 맞추겠습니다.
>
> 독립 리뷰에서도 P1/P2 문제는 없었습니다. 이제 문서의 현행 테스트 수를 399/399로 맞추고, `parser-stress`가 “PC 메모리 내 파서 검증”일 뿐 PLC·EtherCAT 실기 증거가 아니라는 경계를 함께 기록하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+15 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -8)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+6 -3)
>
> </details>
>
> 요청한 두 GUI 문제와 그 회귀 테스트는 현재 완료 상태입니다. 다음 구현은 LASAL IDE 구조나 실장비 승인이 필요한 항목을 피하고, 현재 소스만으로 끝낼 수 있는 후속 범위를 다시 소팅하겠습니다. 우선 WPF 실제 컨트롤의 남은 D5/출력 경로와 SDK의 남은 동시성·수명주기 공백을 병렬로 점검합니다.
>
> 다음 범위는 두 개를 병행하겠습니다. 실제 SDK 결함 가능성이 확인된 “이전 UDP callback 세대가 재연결된 새 세션에 늦게 오류를 올리는 경로”를 production 코드에서 차단하고, 사용자 증상과 직접 연결되는 “bit 14가 처음에는 꺼져 자동 로드 실패 후 수동 Reload로 CREVIS 7행/3행이 복구되는 경로”를 실제 WPF 컨트롤로 고정하겠습니다. 둘 다 PLC나 LASAL IDE 없이 검증할 수 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 수동 재조회 복구 테스트는 추가되어 Debug에서 3/3 통과했습니다. 지금은 동시에 진행 중인 UDP 콜백 세대 격리 수정까지 회수한 뒤, 전체 Debug/Release 회귀 테스트와 문서의 테스트 수치를 한 번에 맞추겠습니다.
>
> 추가된 CREVIS 복구 검증은 “연결 직후 capability OFF에서는 자동 조회를 보내지 않고, 이후 수동 Reload에서 capability를 다시 읽어 3개 CREVIS 행을 포함한 7개 노드를 표시”하는 실제 WPF 컨트롤 경로입니다. Release 빌드에서도 같은 경로를 확인 중입니다.
>
> 콜백 수정은 이전 세션의 늦은 handler 예외와 source-reject 카운트가 재연결 세션으로 새어 들어오지 않도록 listener 객체와 connection generation을 함께 검사합니다. 새 race 테스트까지 포함해 전체 PC 회귀를 Debug/Release로 독립 재실행합니다.
>
> PC 전체 400/400과 WPF 3/3가 양쪽 구성에서 통과했습니다. 이제 문서에는 기존 399/399·2/2 숫자를 현재 결과로 올리고, CREVIS capability OFF 후 수동 Reload 복구와 콜백 stale-generation 차단 경계를 명시하겠습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/CALLBACK_LISTENER_DESIGN_2026-07-09.md` (+5 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/RPC_INITIALIZATION_CALLBACK_IMPLEMENTATION_2026-07-10.md` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -8)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+7 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+6 -5)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -9)
>
> </details>
>
> 독립 검토에서도 P1/P2 문제는 없었습니다. 콜백 lock을 사용자 handler 호출 중 잡지 않는 점, 이전 세대 오류 차단, CREVIS Reload의 정확한 요청 순서까지 확인됐고, 현재 두 핵심 회귀를 3회 반복 실행해 흔들림 여부를 보고 있습니다.
>
> 현재 지적사항은 완료됐습니다. 이어서 LASAL이나 PLC를 건드리지 않고 진행할 수 있는 다음 항목인 “실제 WPF 프로세스를 강제 종료한 뒤 mutation journal이 그대로 복구되고 SDO/DO가 자동 재전송되지 않는가”를 구현하겠습니다. 이 테스트는 지금 추가한 SDO Write 안전 경계를 실제 프로세스 수명으로 확인합니다.
>
> 다음 smoke는 단순히 화면 문구만 보는 테스트가 아닙니다. 자식 WPF 프로세스가 journal lock을 실제로 잡은 상태에서 다른 writer가 거부되는지, 프로세스를 강제 종료해도 journal 바이트가 변하지 않는지, 재시작 후 같은 기록이 다시 복구되는지, 연결 중 `0x7E50`/`0x7E23` 자동 replay가 0건인지까지 묶어 검증하도록 구성하고 있습니다.
>
> 기존 단위 수준 journal 강제종료 테스트와 겹치지 않도록, 이번 것은 실제 `MainWindow`가 복구 UI와 연결 admission을 적용한 상태를 검증하는 별도 4번째 WPF smoke로 한정했습니다. 두 mutation 종류(SDO/DO)를 한 테스트 안에서 각각 독립 프로세스로 실행해 테스트 수만 불필요하게 부풀리지 않겠습니다.
>
> <details><summary>Created a file, edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+14 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+948 -0)
>
> </details>
>
> 첫 실행에서 실제 동작 하나가 확인됐습니다. `ArmedBeforeDispatch` 기록은 연결 상태가 `Connecting`으로 바뀔 때 안전하게 `OutcomeUnverified`로 승격되며 journal을 다시 씁니다. 이건 자동 replay가 아니라 의도된 보수적 상태 전이입니다. 따라서 프로세스 강제종료 시 바이트 불변성을 검증할 기준 기록은 이미 안정 상태인 `OutcomeUnverified`로 고정하고 다시 실행합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+10 -3)
>
> </details>
>
> 실제 프로세스 smoke는 Debug/Release 각각 4/4로 통과했습니다. SDO와 DO 두 종류 모두 자식 WPF를 두 번씩 재시작했고, 각 실행에서 연결 wire는 정확히 `0x8080 → 0x405C → 0x7E00`뿐이었으며 강제종료 전후 journal 바이트와 identity/state가 유지됐습니다. 이제 이 새 검증 경계를 문서에 반영합니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+11 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+8 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+8 -8)
>
> </details>
>
> 복구 smoke까지 닫혔으므로 다음 구현은 D4 Recorder Double-bank의 PC/WPF qualification harness로 넘어갑니다. PLC의 bit 6이나 Double storage는 활성화하지 않고, 현재 SDK 계약 위에서 두 bank lifecycle·세 번째 Start Busy·bank 간 데이터 불변성·실패 시 자원 보존을 먼저 고정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+48 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+83 -21)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+60 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+15 -2)
>
> </details>
>
> 독립 검토에서 “READY 직후 TCP가 이미 끊겨도 zero-replay로 오판할 수 있는 구멍”이 발견됐습니다. 부모가 자식 WPF에 `ApplicationIdle` 관찰 barrier를 요청하고, 서버가 그 시점에도 live read 상태인지 확인한 뒤에만 강제종료를 허가하도록 강화했습니다. 조기 EOF/IOException은 이제 실패입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+8 -6)
>
> </details>
>
> 복구 smoke의 검토 지적 3건을 모두 수정했고 최종 독립 재검토도 P1/P2 없음으로 끝났습니다. D4 Double-bank 쪽은 현재 UI와 분리된 orchestrator 계약을 먼저 작성 중이며, 기존 bit 6 OFF 상태와 LASAL 소스는 그대로 유지합니다.
>
> D4 작업은 active capability를 여는 일이 아니라 qualification 계약을 만드는 단계입니다. 핵심 경계는 실패·취소 때 두 bank의 exact identity를 자동 release하지 않고 recovery scope로 보존하는 것입니다. 단일-bank cleanup 코드를 재사용하면 한 bank를 잃을 수 있으므로 별도 orchestrator로 분리하고 있습니다.
>
> <details><summary>Created 2 files, edited 3 files</summary>

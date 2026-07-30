> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsTopologyIoContractTests.cs` (+6 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 소유권/세션 경계 보강 코드에 대한 병렬 리뷰를 회수한 뒤, 문서와 전체 Debug·Release·정적 검증까지 마무리하겠습니다. 현재 단계에서는 LASAL 소스나 실행 중인 테스트 GUI를 건드리지 않습니다.
>
> 리뷰에서 실제 세션 경합 두 건을 확인했습니다. 검증 직후 재접속되면 topology-bound 호출이 새 세션을 다시 잡을 수 있었고, aggregate/Bulk 준비 중 capability 조회도 세션을 재캡처했습니다. 두 경로 모두 원래 aggregate가 가진 세션 번호로 끝까지 고정한 뒤 테스트를 추가하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD1.cs` (+12 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD2.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+11 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+106 -29)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+9 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD45CompletionContractTests.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/TopologyBindingContractTests.cs` (+113 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+123 -0)
>
> </details>
>
> 세션 경합 수정과 PI Write Catalog 경계까지 반영했고, Debug 전체 363/363이 다시 통과했습니다. 이제 문서에는 “로컬 모델 열람은 가능하지만 실제 RPC에 쓰는 aggregate는 owner/session-bound”라는 기준과 재접속 시 재조회 조건을 정확히 맞추겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+12 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+9 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+13 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+13 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+10 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -2)
>
> </details>
>
> PC 쪽은 Debug/Release 모두 363/363, WPF도 실행 중인 기존 GUI와 분리한 출력 폴더에서 Debug/Release 빌드가 통과했습니다. 마지막으로 LASAL은 IDE를 열지 않고 source/generated 정적 계약만 교차 확인합니다.
>
> 현재 provenance 작업은 검증까지 끝났고, 기존에 staged 상태였던 대형 history 파일의 trailing-whitespace 336건만 `git diff --cached --check`에 남아 있습니다. 제 변경의 unstaged diff는 깨끗합니다. 독립 최종 리뷰를 기다리는 동안 다음 우선순위인 Stop/PowerOff 송신 우선권 구조를 읽기 전용으로 분해하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 확인 결과, topology/catalog provenance 구현 자체에는 차단 결함이 없지만 “preflight 직후 reconnect” 경쟁 조건과 async/pinned 무송신 조합의 테스트가 부족합니다. 이 부분을 먼저 보강한 뒤, 수동 diagnostics가 현재 안전 송신 게이트를 우회하는 문제를 RPC 단위로 고치겠습니다. 단순히 긴 diagnostics 작업 전체를 잠그면 Stop/PowerOff가 더 늦어지므로 그렇게 처리하지 않겠습니다.
>
> provenance 테스트 보강도 끝났습니다. unbound/foreign/stale topology의 sync·async·pinned 경로가 실제 송신 0회인지 확인했고, Catalog/Topology/Bulk/PI Write가 세션을 다시 캡처하지 않는 구조도 IL 계약으로 고정했습니다. Debug 전체 결과는 그대로 `363/363 PASS`입니다. 이제 안전 우선순위 구현으로 넘어갑니다.
>
> 안전 우선순위는 SDK의 실제 `stream.Write` 직전에 세대를 검사하는 방식으로 확정했습니다. 이 경계보다 먼저 Stop/PowerOff가 예약되면 후속 diagnostics RPC는 바이트를 전혀 쓰지 않고 종료되고, 이미 이 경계를 통과한 1건은 소켓을 끊지 않고 응답까지 완료합니다. SDO/DO Write의 `onWriteStarting`보다도 앞에서 검사해 preemption을 `NotAttempted`로 보존합니다.
>
> <details><summary>Created a file, edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+7 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnectionModels.cs` (+7 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcSendPriorityCoordinator.cs` (+211 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+111 -50)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Transport.cs` (+9 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+25 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+28 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
>
> </details>
>
> SDK와 WPF 배선 후 Debug 빌드는 통과했습니다. 일반 GUI 작업은 시작 시점의 세대를 전체 compound API 호출에 유지하고, qualification은 각 송신 단위로 유지하며 cleanup만 현재 세대로 새로 열도록 분리했습니다. 따라서 Stop 이후 오래된 diagnostics가 다시 이어서 송신하는 경로와 cleanup까지 같이 막아버리는 경로를 동시에 피했습니다.
>
> 기존 363개 회귀는 새 송신 경계를 넣은 상태에서도 전부 통과했습니다. 지금은 지연된 diagnostics A, 대기 중인 diagnostics B, 뒤늦게 들어온 Stop의 실제 wire 순서를 fake TCP로 고정하는 테스트와, preempt된 SDO/DO 제출이 `OutcomeUncertain`이 아니라 `NotAttempted`로 남는 테스트를 추가하고 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+18 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -9)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+44 -15)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcSendPriorityCoordinator.cs` (+25 -18)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+60 -25)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+36 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+20 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 안전 우선 전송 경로는 368개 PC 테스트까지 통과했습니다. 지금 공개 scope의 잘못된 Dispose 순서에서 컨텍스트가 오염될 수 있는 저위험 결함을 먼저 고친 뒤, 문서와 Release 정적 검증까지 마무리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcSendPriorityCoordinator.cs` (+11 -1)
>
> </details>
>
> 검토 중 한 가지 경계조건도 확인했습니다. 같은 coordinator에서 더 최신 안전 예약이 생긴 뒤 예전 priority scope가 전송을 시도하면 현재 구현은 통과시킬 수 있으므로, “최신 예약만 priority로 유효”하도록 write 직전 검증을 강화하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcSendPriorityCoordinator.cs` (+13 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcSendPriorityCoordinator.cs` (+4 -3)
>
> </details>
>
> 문서에는 구현 의미와 한계를 반영했습니다. 이미 송신을 시작한 RPC는 강제 취소하지 않고, 아직 쓰기 직전 검사를 통과하지 못한 일반·진단 후속 RPC만 0바이트로 선점된다는 범위를 명확히 적었습니다. PLC 실시간 안전 인증으로는 기록하지 않았습니다.
>
> 최종 리뷰에서 P1/P2 결함은 더 발견되지 않았습니다. 추가된 3개 회귀까지 포함해 Debug는 371/371이며, 이제 Release·WPF 양 구성과 LASAL SourceOnly/full 정적 계약을 서로 충돌하지 않는 구성으로 나눠 검증합니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> Debug/Release PC 테스트는 각각 371/371, WPF Debug/Release 빌드와 LASAL SourceOnly/full 정적 계약도 모두 통과했습니다. 문서의 전체 테스트 수를 371로 맞췄고, 마지막으로 whitespace·작업트리 범위를 점검합니다.
>
> 이번 전송 우선순위 묶음은 정적 검증까지 끝났습니다. staged 상태의 대형 히스토리 파일에 기존 trailing whitespace 336줄이 있어 cached 검사만 실패하며, 이번 변경과 무관해 건드리지 않았습니다. 이제 LASAL IDE 구조를 요구하지 않는 다음 구현 항목을 현재 설계와 소스에서 다시 소팅해 이어가겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -2)
>
> </details>
>
> 현재까지 확인된 사용자 요구 범위는 이렇게 동작합니다. 연결 직후 capability를 다시 읽어 bit 14의 구성 topology를 자동 로드하고 CREVIS 3개 항목을 `CFG` 열에 표시하며, bit 15/16이 꺼져 있으면 `LIVE` 정보는 “미광고·wire 0회”로 구분합니다. SDO는 클릭 시 immutable request를 먼저 만들기 때문에 일반 전송 중 다음 입력을 편집할 수 있고, 성공 Write의 exact readback 대기 중에만 고정됩니다. 별도 리뷰가 이 두 경계를 다시 대조 중입니다.
>
> 다음 외부 편집 가능 항목은 diagnostics 허용/차단 판정 통합으로 잡겠습니다. 현재 SDO·DO·journal·quarantine 규칙이 여러 `MainWindow` partial에 반복되어 새 경로가 한 조건을 빠뜨릴 위험이 있습니다. wire·capability·LASAL은 건드리지 않고, 기존 동작을 먼저 truth table로 고정한 뒤 순수 정책과 PC 테스트로 옮기는 범위로 진행합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 표시는 CFG/LIVE 분리 방식으로 들어가 있고, SDO Write 편집 잠금도 해제한 상태입니다. 지금은 다음 안전한 구현 묶음인 “진단 작업 허용/차단 정책 통합”을 진행해, 같은 조건이 화면·명령 경로마다 다르게 적용되는 위험을 줄이겠습니다. LASAL IDE는 건드리지 않습니다.
>
> 정책 추출 중 확인된 중요한 불일치가 하나 있습니다. 문서 계약은 mutation journal 장애 시 tracked D5 Read를 차단하도록 되어 있는데, 현재 “Write 후 필수 exact readback” 예외 경로는 그 조건을 우회할 수 있습니다. 정책 통합 시 이 경로도 fail-closed로 맞추고 회귀 테스트로 고정하겠습니다.
>
> <details><summary>Created 2 files, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsOperationAdmissionPolicy.cs` (+234 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsOperationAdmissionPolicyTests.cs` (+279 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+43 -4)
>
> </details>
>
> 추가 추적 결과를 반영해 방금 판단을 바로잡습니다. exact readback은 일반 tracked D5 Read와 달리 기존 미해결 Write를 해소하는 복구 작업이고, 검증 성공 뒤에도 durable journal 해소가 먼저 성공해야 volatile interlock을 지웁니다. 따라서 journal 장애 시 읽기 자체는 허용하되 interlock 해제는 계속 fail-closed인 기존 계약을 유지하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsOperationAdmissionPolicyTests.cs` (+0 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsOperationAdmissionPolicy.cs` (+5 -12)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+17 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+15 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+75 -27)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+12 -34)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+16 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+9 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+10 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+12 -1)
>
> </details>
>
> 1차 정책 추출은 코드에 연결했고, 현재 PC 회귀는 378/378 PASS, WPF Debug 빌드는 PASS입니다. 이제 두 개의 독립 리뷰로 기존 CREVIS 자동 로드·SDO 편집 동작과 exact readback/Connect/Close 의미가 변하지 않았는지 교차 확인 중입니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsOperationAdmissionPolicyTests.cs` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/DiagnosticsOperationAdmissionPolicy.cs` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+24 -8)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+10 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+8 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> 독립 리뷰 두 건 모두 P1/P2 없음으로 끝났습니다. 특히 CREVIS는 Connect 후 auto-load가 그대로 유지되고, SDO는 일반 in-flight 중 편집 가능·필수 exact readback 때만 고정되는 동작이 유지됐습니다. 이제 Debug/Release와 LASAL 정적 계약을 최종 재검증합니다.
>
> admission 정책 묶음은 완료했습니다: Debug/Release 378/378, WPF 양 구성, LASAL SourceOnly/full static 모두 PASS입니다. 기존 staged history 파일의 336개 trailing-whitespace만 별도 문제로 남겨 그대로 보존했습니다. 다음으로 연결 Init/Close/Dispose 경쟁 조건을 결정적으로 재현하는 테스트 묶음을 진행합니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+388 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+4 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금까지 확인된 CREVIS 표시·SDO 편집 잠금 개선과 진단 정책은 반영된 상태입니다. 이어서 연결 종료와 통신 장애가 겹칠 때 최종 상태가 다시 `Faulted`로 덮이는 경쟁 조건을 재현 테스트와 함께 수정한 뒤, 전체 정적·C# 검증까지 다시 돌리겠습니다. LASAL IDE는 건드리지 않습니다.
>
> 추가 감사에서 실제 P2 두 건이 확인됐습니다. 이전 세션의 늦은 정리 작업이 새 연결 메타데이터·콜백까지 지울 수 있는 문제와, `ConnectionStateChanged` 안에서 Init/Close/Dispose를 재호출하면 연결이 분리 상태가 되는 문제입니다. 둘 다 같은 연결 수명 세대로 묶어 수정하고, 상태 이벤트 안의 동기·비동기 재진입은 즉시 거부하는 계약으로 고정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+165 -33)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcLifecycleConcurrencyTests.cs` (+344 -0)
>
> </details>
>
> 회귀 테스트까지 포함한 PC Debug 결과는 `388/388 PASS`입니다. 이제 Release와 테스트 GUI Debug/Release를 서로 다른 출력 폴더로 빌드하고, 그 뒤 LASAL 소스 계약 검증과 문서의 테스트 수를 맞추겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
>
> </details>
>
> 현재 수정은 명시적 Close뿐 아니라 새 연결까지 보호해야 완전합니다. 그래서 이전 callback 정리가 멈춘 동안 실제 replacement Init을 시작해 새 listener/handshake 정보가 유지되는 회귀와, Init handshake 두 단계(`0x8080`, `0x405C`) 취소 회귀도 추가하겠습니다.
>
> <details><summary>Edited 8 files</summary>

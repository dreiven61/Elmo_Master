
주요 파일:

- [D5SdoContentionQualificationOrchestrator.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoContentionQualificationOrchestrator.cs:161)
- [MainWindow.Qualification.Sdo.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs:137)
- [MainWindow.xaml](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1629)
- [D5SdoContentionQualificationOrchestratorTests.cs](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoContentionQualificationOrchestratorTests.cs:10)

검증 결과:

- API Debug/Release: 각각 `423/423 PASS`
- WPF Debug/Release 빌드: PASS
- WPF startup smoke: 각각 `5/5 PASS`
- `git diff --check`: PASS
- 기존에 staged된 대형 히스토리 파일에는 원래 있던 trailing whitespace 168건이 남아 있습니다. 이번 변경과 무관하며 수정하지 않았습니다.

실장 테스트 시 축을 Power Off/Standstill로 두고 새 버튼을 실행하면 됩니다. 정상 순서는 `baseline → first accepted → second ResourceBusy → first success → third distinct ticket success`입니다. 캡처는 `23f_SDO_Contention_ResourceBusy_Recovery.pcapng`로 남기면 됩니다.

이번 증분에는 LASAL 코드 변경이 없으므로 LASAL 재빌드는 필요하지 않습니다. 실제 PLC/pcap 증거는 아직 확보되지 않았습니다. 커밋이나 테스트 폴더 복사도 수행하지 않았습니다.

<details><summary>29 previous messages</summary>

> 다음 증분을 바로 이어가겠습니다. 현재 작업 트리와 남은 구현 계획을 다시 대조해서, LASAL IDE 구조 생성 없이 외부 편집만으로 완결할 수 있는 가장 높은 우선순위 항목을 선정한 뒤 코드·자동 시험·문서를 함께 반영하겠습니다.
>
> 현재 계획상 다음 독립 항목은 D5 timeout 또는 queued-cancel 검증입니다. 둘 다 단순 버튼 추가가 아니라 `terminal identity → slot/drain 회수 → 같은 BootId 복구`까지 증명해야 하므로, 실제 PLC 상태 전이와 맞지 않는 시험을 만들지 않도록 현재 LASAL 상태 머신과 SDK 예외 계약을 대조하고 있습니다.
>
> 다음 구현은 D5 timeout→drain→recovery로 확정했습니다. 정상적으로 43~54 cycle이 걸렸던 읽기에 `TimeoutCycles=1`을 적용해 `Expired/TimedOut`을 유도하고, 늦은 callback drain 동안에는 정확한 `ResourceBusy`만 최대 15초 재시도한 뒤 같은 값의 새 ticket 복구를 확인합니다. queued-cancel은 1-cycle 경쟁 때문에 PLC test hook 없이 결정론적이지 않아 그 다음 단계로 미룹니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+41 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+44 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> timeout runner의 fail-closed 계약을 확정했습니다. 복구 제출은 총 600회/15초 범위에서만 허용하고, 매 재시도는 같은 request·BootId·MapRevision의 `Rejected/ResourceBusy`이며 ticket이 없다는 증거가 모두 맞을 때만 진행합니다. 응답 유실이나 예상 밖 승인 흔적이 있으면 즉시 중단하고 기존 격리/정리 경로로 넘깁니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+342 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+47 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+8 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+15 -6)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -2)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -1)
>
> </details>
>
> WPF 쪽 실행 흐름과 문서 반영은 끝났습니다. 현재 timeout core의 결정론적 시험을 작성 중이며, 성공 경로뿐 아니라 600회 Busy 소진, 잘못된 Busy 증거, timeout terminal 불일치, accepted-context/응답 불명확 보존, 취소 시 복구 scope 발행까지 포함해 자동 재전송 경계를 검증하고 있습니다.
>
> timeout 증분 검증이 진행되는 동안 다음 PLC 작업도 준비하겠습니다. D4 Double-bank는 새 LASAL class나 network 없이 기존 `LMCRecorderStore.st` implementation만으로 구현 가능하다는 근거를 확보했습니다. 다만 실제 편집 전 저장소 규칙상 LASAL 코딩·프로그래밍·오류 예방 문서 3개를 먼저 전부 다시 확인합니다.
>
> timeout core와 14개 결정론적 시험이 등록됐고 API Debug는 `437/437 PASS`입니다. 600회 Busy 소진 시험도 실제 sleep 없이 전송 횟수와 599회 delay callback을 정확히 검증했습니다. 이제 WPF Debug/Release 빌드·smoke와 Release API 시험을 실행하고, 문서의 423 수치를 437로 맞추겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> D5 타임아웃/복구 구현 검증을 마무리하면서, 말씀하신 두 가지를 같은 변경 묶음에 반영하겠습니다. 먼저 현재 GUI의 Cravis 정보 경로와 SDO Write 중 입력 잠금 범위를 실제 코드에서 확인해 서로 간섭 없이 수정하겠습니다.
>
> 현재 소스만 보면 SDO 편집 허용 정책은 이미 “실행 중에도 다음 요청 편집 가능”으로 작성돼 있습니다. 그런데 실제 화면에서 잠긴다면 정책 자체보다 상위 UI 상태 갱신이나 실행 파일 반영 여부 문제일 가능성이 큽니다. Cravis 쪽은 GUI 틀은 있으나 PLC가 topology capability(bit 14)를 광고하지 않으면 행을 의도적으로 비우는 구조라, 연결 시 받은 capability와 실패 사유가 화면에서 바로 드러나도록 경로까지 확인 중입니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -3)
>
> </details>
>
> 소스 기준으로 두 현상 모두 대응 코드와 실제 WPF 컨트롤 smoke가 이미 들어와 있음을 확인했습니다. 특히 SDO는 “입력 편집”과 “Submit 직렬화”를 분리해 ordinary in-flight 중 입력만 열고, 안전상 exact readback이 남은 경우만 고정합니다. 이제 최신 Release 실행 파일을 다시 만들고, Cravis 7행/3행 자동 표시와 SDO 편집 동작을 Debug/Release에서 실제 컨트롤 수준으로 검증하겠습니다.
>
> 검증에서 두 현상을 재현 조건까지 분리했습니다.
>
> - Cravis 정적 구성은 최신 GUI+PLC라면 7행 중 3행으로 표시돼야 합니다. Live Health/DI는 아직 PLC bit 15/16과 RT snapshot owner가 없어 비어 있는 것이 현재 설계상 정상입니다.
> - 지금 PC에는 구형 실행 파일과 최신 실행 파일이 같이 존재해, 사용자가 구형 Release를 실행했을 가능성이 확인됐습니다. 방금 표준 Debug/Release 실행 파일을 최신 소스로 다시 빌드했습니다.
> - SDO는 최신 smoke에서 실제 컨트롤이 실행 중에도 편집 가능하고 Submit만 잠기는 것을 확인했습니다.
>
> 이제 LASAL 정적 계약과 변경 파일 무결성까지 닫겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/D5SdoTimeoutQualificationOrchestrator.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/D5SdoTimeoutQualificationOrchestratorTests.cs` (+3 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+7 -2)
>
> </details>
>
> 현장 피드백 반영분은 닫았습니다. 최신 GUI는 legacy 4축 표를 `CREVIS excluded`로 명확히 표시하고, 별도 CREVIS topology 영역에서 정적 7행/3행을 자동 로드합니다. SDO 편집/Submit 분리도 실제 컨트롤 smoke까지 통과했습니다.
>
> Cravis LIVE 값은 LASAL IDE에서 latch client 3개와 network/method 구조를 먼저 만들어야 하므로 그 구조 작업 전에는 외부 `.st`만으로 안전하게 진행할 수 없습니다. 그 작업을 기다리는 동안, IDE 구조 변경 없이 진행 가능한 다음 항목인 Recorder D4 Double-bank PLC 코어 구현을 이어가겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 표시와 SDO 편집 문제는 최신 Debug/Release 실행 파일까지 반영하고 자동 검증을 끝냈습니다. 지금은 이어서 D4 Recorder 이중 버퍼 구현 패치를 회수해, PLC 계약과 기존 API/WPF 흐름을 교차 검토한 뒤 정적 검증까지 마무리하겠습니다.
>
> 현재 확인 결과, CREVIS는 최신 GUI에서 정적 구성 7개 행 중 3개 장치로 자동 표시되며, 실시간 DI/DO가 비어 있는 것은 PLC 쪽 IDE 구조와 네트워크 연결이 아직 없는 상태라서입니다. SDO는 전송 중에도 다음 요청 값을 편집할 수 있고, 중복 전송만 막도록 분리됐습니다. D4 이중 버퍼는 저장소 한 파일에 소유권·재접속·개별 해제 계약이 몰려 있어 패치를 보수적으로 검토 중입니다.
>
> 중간 검토에서 중요한 제약을 확인했습니다. D4 이중 버퍼 PLC 코드는 구현할 수 있지만, capability를 바로 켜면 안 됩니다. 외부 세션이 끊긴 뒤 두 버퍼를 각각 식별·인계·정리할 실기 경로와 RAM/주기 영향이 아직 증명되지 않았기 때문에, 이번 단계에서는 코어를 넣되 capability bit와 GUI 실기 버튼은 계속 차단 상태로 유지합니다.
>
> D4 패치는 현재 두 번째 버퍼의 저장 영역, 버퍼별 상태·RecordId·소유 세션·헤더 메타데이터까지 들어갔습니다. 남은 검토 포인트는 첫 버퍼를 업로드하는 동안 두 번째 캡처가 첫 데이터를 건드리지 않는지, 세션 재연결 시 한 버퍼를 Adopt해도 다른 버퍼의 인계 가능성이 사라지지 않는지, 그리고 개별 Release가 반대편 상태를 바꾸지 않는지입니다.
>
> 병행 작업으로 Double-bank 실기 검증에 필요한 Recorder 헤더 정규화도 추가하고 있습니다. 매 요청마다 바뀌는 RequestId를 해시에 넣지 않고, Boot/Record/Buffer/Config/Map/샘플·타임스탬프·신호 목록 같은 의미 필드만 little-endian으로 고정해 A 버퍼 재다운로드 불변성을 정확히 비교하도록 만드는 작업입니다. GUI 실기 gate는 여전히 열지 않습니다.
>
> 현재 D4는 중간본 기준으로 캡처 완료 상태를 버퍼 descriptor에 마지막으로 publish하도록 조정해, 헤더보다 상태가 먼저 보이는 경쟁 조건을 줄였습니다. 다만 Release/Adopt가 아직 단일 버퍼 코드인 상태라 그 부분까지 끝나기 전에는 D4 완료로 취급하지 않고 있습니다. CREVIS/SDO 반영분은 이 작업과 독립적으로 이미 최신 실행 파일에 들어가 있습니다.
>
> D4 코어와 독립 리뷰까지 끝났습니다. 정적 계약과 PC 회귀는 Debug/Release 각각 443/443으로 통과했고, capability bit 6·buffer count 1·WPF live 차단은 그대로입니다. 이제 문서를 “미구현”에서 “dormant 소스 구현 완료, LASAL 빌드/RAM/jitter/실기 미검증”으로 정확히 맞춘 뒤 전체 검증을 한 번 더 돌리겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+8 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+12 -9)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+24 -13)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+8 -6)
>
> </details>
</details>

반영 완료했습니다.

- CREVIS: 현재 실행 중인 Debug GUI에도 자동 표시 수정이 들어가 있습니다. Connect 후 `EtherCAT / PI` 탭에서 7개 토폴로지 행과 CREVIS 3개 행이 보여야 합니다. 안 보이면 `Reload CREVIS / Topology`을 누르세요.
- SDO Write: 전송 중에도 다음 요청 값을 편집할 수 있습니다. 중복 Submit만 차단되며, Write 후 exact readback이 남은 경우에만 안전상 입력이 고정됩니다.
- 최신 Release GUI: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)
- CREVIS 실시간 Health/DI/DO는 아직 표시되지 않습니다. PLC의 `0x7E13/0x7E22/0x7E23`과 LASAL IDE 연결 구조가 없어서 capability bits 15~17은 계속 OFF입니다. 현재 표시되는 것은 구성 토폴로지입니다.

추가로 D4 Double-bank dormant 코어도 구현했습니다.

- [LMCRecorderStore.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st:187)
- 1.28 MB bank 2개, bank별 identity/state/metadata
- 한 bank 업로드 중 다른 bank 캡처 구조
- 두 bank 점유 시 `ResourceBusy`
- exact all-bank Adopt/rebind와 bank별 Release 격리
- RT/non-RT generation 경합 차단
- capability bit 6/count 2/WPF live gate는 계속 OFF

검증은 PC Debug/Release 각각 443/443, WPF smoke 각각 5/5, LASAL SourceOnly/full static 모두 PASS입니다.

이제 사용자가 `C:\work\Elmo\Elmo_Master\Lasal_PRG\Elmo_EtherCAT_Test_4Axis`를 LASAL에서 빌드해 주세요. 특히 2.56 MB global RAM 배치 때문에 compile/link 결과가 필요합니다. 아직 커밋하거나 테스트 폴더로 복사하지는 않았습니다.

<details><summary>30 previous messages</summary>

> 다음 단계로 D4 실기 활성화의 가장 큰 빈틈인 “Start 응답 유실 후 RecordId를 모르는 상태”를 먼저 해결하겠습니다. 현재 wire/API와 저널 구조를 다시 확인한 뒤, 복구용 bank 조회 계약·durable journal·WPF gate 분리를 서로 충돌하지 않게 병행 구현하겠습니다. capability와 live 버튼은 검증 전까지 계속 닫아 둡니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 병행 작업은 세 갈래로 분리했습니다: PLC/API의 복구용 bank inventory 계약, crash-safe recovery journal, WPF의 수동·qualification·reconnect 독립 gate입니다. 저는 동시에 기존 Double qualification이 복구 키를 실제 송신 전에 확정하도록 `RequestedConfigId` 계약을 강화하고, 응답 유실 시 자동 정리하지 못하게 하는 테스트를 보강하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleBankQualificationOrchestratorTests.cs` (+32 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleBankQualificationOrchestrator.cs` (+9 -0)
>
> </details>
>
> 기존 복구 흐름을 더 확인해 보니 중요한 빈틈이 하나 있었습니다. `Configure`는 PLC에 적용됐지만 `Start` 전 응답이 유실된 경우 bank가 0개라 기존 `AdoptRecorder`를 쓸 RecordId가 없고, 새 세션은 기존 `ReleaseRecorder`도 소유권 불일치로 거절됩니다. 따라서 inventory만 추가해서는 복구가 완성되지 않습니다. 이 경우만 처리하는 exact configuration-adopt 계약까지 추가하고 있으며, 자동 reset이나 추정 ID 정리는 넣지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+20 -1)
>
> </details>
>
> <details><summary>Created 2 files, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryPlanner.cs` (+231 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryPlannerTests.cs` (+323 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
>
> </details>
>
> `0x7E4A`는 정적·PC 검증까지 통과했고 capability는 계속 OFF입니다. 복구 저널도 Configure 송신 전 identity를 원자적으로 보존하도록 완료됐습니다. 지금은 두 결과를 연결해, inventory를 먼저 durable하게 기록한 뒤에만 exact adopt를 허용하고 첫 소유권 변경 이후에는 취소로 중간 상태를 만들지 않는 복구 orchestrator를 구현하고 있습니다.
>
> <details><summary>Created 2 files, edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+311 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+8 -16)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+398 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryPlannerTests.cs` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleBankQualificationOrchestrator.cs` (+21 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleBankQualificationOrchestratorTests.cs` (+68 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleBankQualificationOrchestratorTests.cs` (+1 -0)
>
> </details>
>
> <details><summary>Created 2 files, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+111 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+277 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleQualificationJournalBridge.cs` (+39 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+74 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+2 -2)
>
> </details>
>
> Context compaction

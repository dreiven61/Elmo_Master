> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+10 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/GroupProfileLockRecoveryJournalTests.cs` (+23 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+2 -0)
>
> </details>
>
> 영속 저널 본체와 WPF 통합을 붙였습니다. 새 Enable은 전송 전에 endpoint·group reference·BootId·MapRevision을 기록하고, 복구 명령 직전에도 fresh capability로 동일성을 재검증합니다. 우선 컴파일을 통과시킨 뒤 기존 fake-RPC 시퀀스를 새 read-only identity 요청에 맞추겠습니다.
>
> 컴파일은 통과했습니다. 기존 WPF 회귀는 새 Enable 직전 capability identity read가 한 번 추가되어 fake-RPC 예상 순서를 조정 중입니다. 이 read는 복구 저널에 기록할 BootId/MapRevision을 캐시가 아닌 현재 PLC에서 다시 확인하기 위한 읽기 전용 요청입니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+18 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+6 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+18 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+31 -26)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+9 -9)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 진행 중인 WPF 회귀 테스트 결과부터 회수하겠습니다. 그다음 재시작 복구와 엔드포인트·그룹 참조·PLC 세대 불일치 차단 테스트를 보강하고, 전체 빌드/정적 검증까지 이어가겠습니다.
>
> 현재 WPF Debug 기준은 `52/52 PASS`로 회복됐습니다. 다만 코드 검토에서 journal arm 직후 ACK 확정 전 연결이 끊기면, 같은 프로세스의 메모리 latch가 아직 올라오기 전인 좁은 구간이 확인됐습니다. 활성 journal 자체를 unresolved 상태로 취급하도록 이 구간을 먼저 막겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+15 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+9 -3)
>
> </details>
>
> 추가 검토에서 안전 상태 3회 관측으로 pending을 해제하는 경로가 durable journal을 먼저 지워, Power Off 완료 후의 BootId/MapRevision 재확인을 건너뛸 수 있는 문제가 확인됐습니다. 이 경로는 기존 정책대로 해제를 허용하되, journal을 지우기 전에 fresh capability와 exact identity를 반드시 재검증하도록 순서를 수정합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+21 -7)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -1)
>
> </details>
>
> 재검토에서 한 단계 더 좁은 race가 잡혔습니다. Locked 상태를 확인한 뒤 journal identity를 재확인하는 `0x7E00` 응답 대기 중 Stop/PowerOff가 예약되면, 이전 generation 결과를 적용할 수 있었습니다. identity 확인 뒤 journal resolve 직전에 safety generation을 다시 검사하도록 보강합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+29 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+4 -3)
>
> </details>
>
> 완료 결과를 적용하는 도중 identity 확인이 실패하면 SDK continuation은 이미 소비됐지만 durable record만 남을 수 있었습니다. 이 경우를 즉시 `RecoveryRequired`로 승격하고, 복구 중 허용하면 안 되는 Group Reset/Set Identity도 handler와 UI 양쪽에서 차단하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+67 -10)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+36 -33)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+9 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+112 -0)
>
> </details>
>
> 재시작·endpoint·group reference 회귀 4종은 모두 단독 PASS했습니다. 지금은 마지막으로, identity 확인 응답이 지연된 정확한 순간에 Group Stop을 예약해 stale 완료가 durable journal을 지우지 않는지 새 회귀를 실행 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+26 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+0 -4)
>
> </details>
>
> 새 post-identity race 회귀까지 통과했고, WPF Debug 전체는 `57/57 PASS`입니다. 이제 WPF Release와 SDK Debug/Release를 병렬로 돌리는 동안 문서의 테스트 수치와 durable recovery 계약을 현재 소스에 맞추겠습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+7 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -0)
>
> </details>
>
> PC 회귀는 최종적으로 SDK Debug/Release `649/649`, WPF Debug/Release `57/57` 모두 통과했습니다. 문서 수치도 57로 맞췄습니다. LASAL IDE는 건드리지 않고, 현재 추적 source에 대해서만 SourceOnly/full 정적 계약을 마지막으로 다시 확인하겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+0 -23)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청한 두 항목을 병행 반영했고, 마지막으로 현재 소스 위치와 검증 결과만 다시 확인하겠습니다. LASAL IDE나 테스트용 복사본은 건드리지 않았습니다.
</details>

요청한 두 항목을 반영했습니다.

- CREVIS 구성 정보는 연결 시 자동 로드되며, `bit 14 + 0x7E11/0x7E12`가 정상 응답하면 7행 중 CREVIS 3행이 표시됩니다. 현재 GUI인지 창 제목의 `[CREVIS topology / editable SDO draft]`로 확인할 수 있습니다. 표시되지 않으면 `Load CREVIS / Topology`를 눌러 재조회하면 됩니다. 관련 기준은 [README.md](./LMC_Library/LasalApiWpfTestApp/README.md:483)에 정리했습니다.
- SDO Write 실행 중에도 Index/SubIndex/값 등 입력 필드를 계속 편집할 수 있도록 변경했습니다. 단, 중복 Submit만 직렬화됩니다.
- CREVIS의 실시간 노드 상태·DI·DO는 아직 PLC의 `0x7E13/0x7E22/0x7E23` 구조와 capability bit 15~17이 필요합니다. 현재 구현은 우선 구성 정보 표시까지입니다.
- SDO Write 실제 송신은 안전상 계속 gate-off 상태입니다. 대상 축과 SDO 항목 승인, LASAL IDE에서 `LMCSdoExecutor` 생성자 구조 구성 후 PLC/SDK gate를 함께 켜야 합니다.
- 병행 작업으로 Group Enable의 재시작·통신 단절 복구를 영속 저널 방식으로 보강했습니다. 핵심 경로는 [MainWindow.GroupProfileLockRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs:120)입니다.

검증 결과:

- SDK Debug/Release: 각각 `649/649 PASS`
- WPF Debug/Release: 각각 `57/57 PASS`
- LASAL 외부 정적 계약 검사: PASS
- `git diff --check`: PASS
- PLC 다운로드 및 실제 하드웨어 시험: 아직 미실시
- LASAL IDE: 실행하지 않음
- 커밋·테스트 폴더 복사: 이번에는 수행하지 않음

참고로 staged 검사는 기존에 stage된 [Elmo_Master_history_260721.md](./docs/history/Elmo_Master_history_260721.md:17314)의 trailing whitespace 때문에 실패합니다. 이번 구현 파일 문제는 아닙니다.

<details><summary>19 previous messages</summary>

> 다음 구현 우선순위를 현재 소스와 백로그에서 다시 확인하겠습니다. 기존 변경을 건드리지 않도록 작업 영역을 분리해 점검하고, PC에서 구현·자동검증 가능한 항목부터 계속 진행하겠습니다.
>
> CREVIS의 실제 동적 정보는 PC 쪽이 아니라 LASAL `LMCEcatInputLatch`/`LMCDiagnosticsService` 확장이 핵심입니다. 구조 생성 영역을 외부에서 임의 수정하지 않도록, 저장소의 LASAL 작성 규칙을 먼저 전부 확인한 뒤 구현 가능한 경계를 결정하겠습니다.
>
> 현재 확인된 핵심 경계는 명확합니다. CREVIS 동적 Health/DI/DO는 기존 class에 필요한 client와 method 선언이 아직 없어 외부 `.st` 구현을 바로 넣으면 LASAL 구조를 깨뜨립니다. 그 부분은 IDE 구조 생성 전에는 손대지 않고, 동시에 남아 있는 PC/API 측 미완료 항목을 좁혀 실제 구현 가능한 다음 증분을 고르는 중입니다.
>
> 다음 증분은 두 가지로 진행 중입니다.
>
> - SDO exact readback 화면으로 전환하더라도 사용자가 준비한 다음 요청 draft를 잃지 않게 하고, 검증 성공 시에만 안전하게 복원합니다. 검증 중 사용자가 다시 편집했다면 그 최신 값은 덮어쓰지 않습니다.
> - PLC와 무관하게 닫을 수 있는 배포 추적성 작업으로, 배포 파일의 버전·commit·SHA-256을 패키지 내부 manifest로 자동 생성하고 재검증하도록 만들고 있습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+50 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+137 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+47 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+7 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -2)
>
> </details>
>
> SDO 편집 draft 보존 증분은 구현했고 해당 WPF 실제 컨트롤 smoke `1/1 PASS`까지 확인했습니다. 현재는 전체 회귀 전에 두 가지를 추가 점검 중입니다: VERIFIED 직전 사용자 편집을 절대 덮지 않는지 코드 리뷰, 그리고 새 release manifest가 첫 생성 후 자기 자신 때문에 다음 clean build를 dirty로 오판하지 않는지 반복 빌드 계약입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+12 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+35 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+95 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+34 -7)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 동적 데이터는 현재 LASAL IDE 구조가 없어서 여기서 억지로 연결하지 않겠습니다. 우선 지금 반영한 SDO 편집값 보존과 배포 매니페스트를 실제 빌드·테스트로 확인하고, CREVIS는 사용자가 IDE에서 추가해야 할 정확한 구조까지 정리한 뒤 다음 외부 구현 구간을 확정하겠습니다.
>
> SDO 편집값 보존 변경은 컴파일됐고, 실제 `VERIFIED` 처리 경로까지 포함한 회귀 테스트가 통과했습니다. 오래된 세션에서는 버튼 자체가 비활성화되고, 같은 세션에서는 원래 편집값을 복원하며, 로드 후 사용자가 다시 수정한 값은 덮어쓰지 않는 동작입니다. 배포 매니페스트 반복성 테스트도 현재 39/39 통과했습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+1 -1)
>
> </details>
>
> WPF 전체 회귀도 Debug/Release 각각 57/57 통과했습니다. 지금은 SDK 전체 회귀와 LASAL 정적 계약을 다시 돌려서, 이번 GUI 수정이 기존 진단·SDO·토폴로지 계약을 건드리지 않았는지 확인 중입니다. CREVIS GUI의 정적 7행 표시와 실제 온라인 상태/DI/DO는 구분해서 보고하겠습니다.
>
> 최종 감사에서 코드 P0는 없었지만, 배포 매니페스트 정책을 아직 “금지”로 적은 현재 문서 두 곳이 발견됐습니다. 이건 그대로 두면 다음 작업자가 잘못 판단하므로 지금 새 정책과 “실제 패키지는 아직 재조립하지 않음”을 구분해 바로잡겠습니다. 실제 Distribution 빌드는 건드리지 않습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_SOURCE_REVIEW_2026-07-15.md` (+7 -6)
> - Wrote `./LMC_Library/LMC_API/LMC_API/RELEASE_MANIFEST.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+9 -6)
>
> </details>
>
> CREVIS 동적 값은 IDE 구조가 필요하지만, 그 전에 외부 편집으로 바로 막을 수 있는 실제 위험을 하나 찾았습니다. 현재 정적 검사기는 LASAL serializer와 network를 보지만 `Eni.xml`의 실제 슬레이브/PDO 정의와 서로 대조하지 않습니다. 케이블·노드 순서를 바꾼 뒤 API가 예전 배열을 계속 내보내는 문제를 빌드 전에 잡도록 ENI↔network↔7-entry serializer 정적 gate를 추가하겠습니다.
>
> ENI를 직접 확인한 결과 현재 설정은 실제로 `GL-9086(Slave 1) → Elmo 4대(Slave 2~5)`이고, CREVIS 입력/출력은 각각 32-bit PDO입니다. 즉 “온라인 상태에 따라 노드 목록 자체가 자동 재발견되는 API”가 아니라 “다운로드된 ENI의 고정 스키마 + 동적 상태/값” 구조가 맞습니다. 지금 추가하는 gate는 ENI가 바뀌었는데 고정 스키마와 revision을 안 바꾼 경우를 실패 처리합니다.
>
> 정적 drift gate는 구현됐고 현재 ENI·network·serializer·생성 table을 함께 검사합니다. ENI 순서/identity/PDO, network SlaveIndex, serializer revision/count, generated table identity를 일부러 깨뜨린 7개 negative fixture도 모두 거부됩니다. SourceOnly/full 정적 계약은 현재 PASS이며 독립 리뷰 결과를 확인한 뒤 문서에 완료 경계를 고정하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -0)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+8 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -1)

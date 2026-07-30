> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleBankQualificationOrchestrator.cs` (+43 -6)
>
> </details>
>
> 권한 경계 수정 후 WPF Release 74/74, SDK Debug·Release 715/715가 다시 통과했습니다. 실행 중인 WPF Debug는 건드리지 않았습니다. 이제 CREVIS는 “설정된 토폴로지 표시”와 “PLC가 실시간으로 보고하는 노드/PDO”를 분리해, 어느 단계에서 값이 끊기는지 소스 기준으로 확인 중입니다.
>
> 정적 계약 검사를 돌려 보니 예상대로 `0x7E13`과 `0x7E22`가 TCP route 목록에 없어서 중단됐습니다. 현재 GUI/SDK가 못 그리는 문제가 아니라 PLC 쪽 live owner가 아직 없는 상태입니다. 정적 7행도 안 보였다면 실행 중인 Debug가 현재 소스보다 오래됐거나, 해당 세션에서 bit 14/`0x7E11·12`가 실패한 것입니다. live 구현에 필요한 IDE 생성 구조가 현재 프로젝트에 생겼는지 마지막으로 교차 확인하고 있습니다.
</details>

확인 결과, 두 문제가 겹쳐 있습니다.

- 현재 실행 중인 WPF Debug는 09:36 빌드본입니다. 최신 Release는 13:49 빌드됐습니다. [최신 Release 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)을 사용해야 이번 변경이 보입니다.
- 최신 GUI도 configured CREVIS 7행/3행만 표시할 수 있습니다. 실시간 Health·DI·DO는 LASAL에 `0x7E13/0x7E22/0x7E23` route와 CREVIS client 연결이 없어서 아직 올라올 수 없습니다.
- SDO Write 중 편집, 비모달 확인, pending readback 중 draft 보존은 반영됐고 관련 WPF 테스트가 통과했습니다.

다음 개발을 위해 LASAL IDE에서 아래 구조만 생성해 주세요.

- `LMCEcatInputLatch`
  - Client: `Coupler`, `InputSlot`, `OutputSlot`
  - 변수·메서드: 인계 문서 그대로 생성
- `LMCDiagnosticsService`
  - private `HandleEtherCATTopologyIoRequest`
- Motion Network
  - 위 세 client를 `GL_9086_11`, `Slot001`, `Slot011`에 연결

정확한 이름과 타입은 [T2 IDE 구조 인계 문서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:24)에 있습니다. 구조 생성 후 Rebuild/Link 결과만 알려주세요. 그러면 제가 외부 편집으로 464-byte snapshot, `0x7E13/0x7E22`, TCP route를 이어서 구현하겠습니다. bit 15~17과 DO allowlist는 실제 read 경로 검증 전까지 켜지 않습니다.

현재 검증 결과는 SDK Debug·Release `715/715`, WPF Release `74/74`, `git diff --check` PASS입니다. 커밋·스테이징·테스트 폴더 복사는 하지 않았습니다.

<details><summary>33 previous messages</summary>

> IDE 구조가 아직 생성되지 않았으므로 live CREVIS 경로를 억지로 활성화하지 않겠습니다. 대신 현재 소스에서 선언 변경 없이 구현 가능한 안전한 범위를 다시 확인하고, 가능하면 다음 외부 구현 단계를 코드·검증까지 진행하겠습니다.
>
> 현재 verifier에는 “정적 topology”와 “live 구현 완료” 사이 단계가 없습니다. 사용자가 IDE에서 구조만 생성하면 두 기존 checkpoint가 모두 실패해 구조 오류와 미구현 로직을 구분할 수 없습니다. 이 중간 상태를 정확히 검증하는 `IdeStructureReady` checkpoint를 추가해 다음 LASAL 인계 단계를 검증 가능하게 만들겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+97 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 쪽은 다시 확인했고, 구현부 우회는 안전하지 않다는 결론입니다. IDE에서 3개 client와 Network 연결이 생성되기 전까지 live 비트/API를 켜지 않겠습니다. 그 사이 외부 편집만으로 완결 가능한 안전 기능을 계속 구현하되, 먼저 새 `IdeStructureReady` 검증 단계를 문서와 맞추겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+19 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+11 -5)
>
> </details>
>
> 구조 검증 문서까지 동기화했고 기존 `StaticTopologyOnly`는 다시 PASS했습니다. 새 단계는 현재 소스에서 정확히 “client 6개, 기대 9개”로 실패하므로 IDE 작업 전·후 경계가 분리됐습니다. 이제 CREVIS와 독립적인 PC 측 고위험 잔여 항목인 “Move 송신 뒤 앱이 종료되면 불확실 상태가 사라지는 문제”를 구현하겠습니다.
>
> 정적 검증기 재감사에서 두 가지 허점이 확인됐습니다. 구조-only 단계가 실제로 빈 stub만 허용하도록 고정하고, capability 응답 비트가 몰래 추가되지 못하도록 `0x7E00`의 정확한 3회 쓰기만 허용하는 검사를 먼저 보강하겠습니다. 이 경계를 닫은 뒤 motion journal 구현으로 들어갑니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+37 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+88 -15)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -0)
>
> </details>
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+757 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+33 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+12 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+27 -35)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+17 -18)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+24 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+39 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+33 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+11 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+1 -4)
>
> </details>
>
> motion journal 코어와 9개 단위 테스트가 추가됐고, 독립 C# 7.3 컴파일에서 9/9 PASS했습니다. 이제 앱의 모든 Move 경로를 이 journal로 연결한 상태를 실제 프로젝트 빌드로 맞추고, 재시작 시 정확한 endpoint/target만 Connect·Lookup·Stop 할 수 있는지 통합 테스트를 붙이겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+2 -0)
>
> </details>
>
> 첫 통합 실행은 새 journal 9개를 포함해 83개 중 81개가 통과했습니다. 실패 2개는 동작 실패가 아니라 기존 테스트 정리 코드가 새 `journal.lock`을 닫지 않고 임시 폴더를 삭제한 문제입니다. 테스트 cleanup을 새 journal까지 닫도록 보정한 뒤 다시 실행하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfRecorderManualDoubleAdapterIntegrationTests.cs` (+3 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS는 IDE 구조가 생기기 전에는 안전하게 활성화할 수 없는 상태로 고정해 두었고, 지금은 그와 병행 가능한 WPF 모션 불확실성 복구 경로를 구현 중입니다. 방금 실패했던 2건은 기능 실패가 아니라 새 저널 잠금 파일을 테스트가 정리하지 못한 문제였고, 정리 코드를 고쳤으니 Release 전체 테스트부터 다시 확인하겠습니다.
>
> Release 빌드는 경고·오류 없이 통과했습니다. 전체 스모크 테스트는 WPF 프로세스 기반 항목까지 포함되어 시간이 걸리고 있어 계속 실행 중이며, 동시에 재시작 후 정확한 대상만 복구하고 Move를 자동 재전송하지 않는 통합 테스트를 추가하고 있습니다.
>
> 기존 전체 스모크 테스트는 `83/83 PASS`로 복구됐습니다. 이제 단위 저널뿐 아니라 실제 WPF 복구 흐름과 강제 종료 후 “Move 재전송 0회”를 자동으로 증명하는 테스트를 채우고, 그 과정에서 드러나는 구현 결함까지 바로 수정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+15 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -3)
>
> </details>
>
> SDK Release 회귀도 `715/715 PASS`입니다. 모션 쪽 문서에서 “불확실 상태인데도 창을 닫는다”는 기존 설명이 새 구현과 정면으로 충돌해, 현재 동작인 “Close 차단·자동 Stop 없음·정확 일치 복구·Move replay 없음”으로 함께 바로잡았습니다. CREVIS 구조 체크포인트도 IDE 저장 산출물까지 증명하도록 한 단계 더 조이겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+41 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+17 -0)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+7 -4)
>
> </details>
>
> 현재 자동 증거는 두 층으로 늘었습니다. WPF 재시작 복구 통합 6건이 추가되어 `89/89 PASS`이고, endpoint·BootId·MapRevision·축 reference 중 하나라도 다르면 mutation은 0회입니다. 지금은 더 강한 강제종료 시험—Move가 서버에 실제 도착한 뒤 응답을 보류한 상태에서 프로세스를 종료하고, 재시작 시 Move 0회·Stop 1회·상태 3회 후 해제—을 실행 가능하게 마무리하고 있습니다.
>
> 코드 리뷰에서 안전상 실제 결함 4건이 잡혔습니다. 시작 시 저널 상태 승격 저장이 실패하면 복구 잠금이 사라지는 문제, Move 직전 진단 identity가 캐시일 수 있는 문제, Stop/PowerOff 직전에 BootId/MapRevision을 다시 확인하지 않는 문제, 재시작 후 상태 조회만으로 저널을 해제할 수 있는 문제입니다. 모두 수용했고, 지금부터 mutation wire 직전 fresh identity 확인과 “명시적 안전 명령 ACK + 안정 상태” 조건으로 수정합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+249 -10)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+122 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+26 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+26 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+43 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+13 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+12 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMotionRecoveryIntegrationTests.cs` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+2 -0)
>
> </details>
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+77 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+12 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMotionRecoveryIntegrationTests.cs` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+1 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 모션 불확실성 복구 경로에서 발견한 실제 결함은 수정했습니다. 지금은 Release 빌드와 회귀 테스트를 다시 돌려, 식별자 변경·재시작·PowerOff 상태 흔들림이 있어도 잘못 해제되지 않는지 확인하겠습니다. CREVIS 실데이터는 LASAL IDE 구조가 생성되기 전까지 활성화하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+2 -0)
>
> </details>
>
> 현재 실패 2건은 제품 로직 결함이 아니라 새 “최종 해제 직전 BootId/MapRevision 재확인” RPC를 테스트 서버 시나리오가 빠뜨린 것이 원인으로 확인됐습니다. 테스트 프레임의 PowerOff bool 검사 오프셋 오류도 함께 바로잡고 재실행 중입니다.
>
> 현재 모션 복구 전용 회귀는 13/13 통과했고, SDK 전체도 715/715 통과했습니다. 추가한 핵심 케이스는 Move가 A/A 식별자로 승인된 뒤 안전상태가 보여도 최종 확인에서 B/B로 바뀌면 저널과 잠금을 유지하는지 검증합니다. WPF 전체 스모크를 계속 돌리고 있습니다.
>
> 최종 소스 감사에서도 새 P0~P2 결함은 없었습니다. 현재 남은 자동 검증 1건은 제품 실패가 아니라 기존 GroupEnable 테스트가 변경된 안전 명령 delegate 형식을 따라오지 못한 문제라, 그 테스트만 갱신한 뒤 전체 96개를 다시 확인하고 있습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+12 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -3)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
>
> 최종 Release 빌드가 경고/오류 0으로 통과했고 WPF 전체 스모크도 96/96 통과했습니다. 이제 LASAL은 수정하지 않은 채 source/static 계약만 재검증하고, 현재 IDE 구조가 CREVIS live owner 조건을 충족하지 않는 실패도 예상대로 유지되는지 확인하겠습니다.
</details>

이번 구현 체크포인트는 완료했습니다.

- SDO Write 실행 중에도 다음 Write 데이터를 편집할 수 있습니다. Submit만 직렬화됩니다.
- 모든 Move에 durable journal과 fresh BootId/MapRevision 검사를 적용했습니다.
- 재시작 복구는 정확한 Stop/Power Off ACK와 연속 안전상태 확인 없이는 해제되지 않습니다.
- Release 빌드: 경고 0, 오류 0
- SDK: 715/715 PASS
- WPF 전체: 96/96 PASS
- LASAL SourceOnly/full static: PASS
- 실제 PLC/축 런타임 검증은 아직 별도입니다.

주요 코드는 [MainWindow.MotionUncertaintyRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs:63), [MainWindow.xaml.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:694)에 반영했고, [README.md](./LMC_Library/LasalApiWpfTestApp/README.md:27)도 96/96 기준으로 갱신했습니다.

CREVIS는 현재 다음 상태입니다.

- configured coupler/slot GUI 렌더링은 PC fake-RPC 테스트에서 통과했습니다.
- 실제 `0x7E13/0x7E22` live 값은 아직 활성화하지 않았습니다.
- `IdeStructureReady` 검사는 현재 `LMCEcatInputLatch`의 `Coupler` client 누락으로 예상 실패합니다.

다음 작업을 위해 LASAL IDE에서 [구조 생성 지침](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:18)의 `Coupler`, `InputSlot`, `OutputSlot` client와 Network 연결을 생성하고 Save/Rebuild해 주세요. 완료되면 나는 LASAL을 직접 제어하지 않고 외부 소스 편집으로 `0x7E13/0x7E22` 구현을 이어가겠습니다.

커밋·스테이징·테스트 폴더 복사는 하지 않았습니다. `git diff --check`는 통과했고, `git diff --cached --check`는 기존에 staged된 대형 히스토리 파일의 trailing whitespace 때문에만 실패합니다.

<details><summary>47 previous messages</summary>

> LASAL IDE 구조 생성 전에도 진행할 수 있는 작업을 다시 분리하겠습니다. 현재 CREVIS 클래스/네트워크 상태와 남은 구현 목록을 소스 기준으로 재점검한 뒤, IDE 생성물 없이 안전하게 완성할 수 있는 부분부터 계속 구현하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 쪽은 현재 `0x7E11/0x7E12`의 정적 구성 정보까지만 있고, 실제 입력·출력 스냅샷 경로(`0x7E13/0x7E22`)는 LASAL 클래스 채널 구조가 아직 없어 막혀 있습니다. 지금은 생성 구조를 임의 편집하지 않는 범위에서, 이미 준비된 구현·검증 계약에 빠진 부분이 있는지와 병행 가능한 다음 코드 작업을 교차 점검하고 있습니다.
>
> 확인 결과, CREVIS 실시간 경로는 외부 편집만으로 바로 넣을 수 있는 상태가 아닙니다. `LMCEcatInputLatch`에 IDE가 생성해야 하는 typed client 3개와 method 구조가 없기 때문입니다. 대신 지금은 IDE 작업 후 잘못된 연결을 놓치지 않도록 정적 검증기를 보강하고 있습니다. 특히 선언된 외부 연결 수 불일치, CREVIS 채널의 중복 owner, 주석으로 위장된 잘못된 변수 형식을 새로 차단합니다.
>
> 병행 작업도 하나 더 시작했습니다. 현재 Axis `Power On`은 ACK 이후 상태 확인이 끊기면 재시도 시 `0x2023`을 다시 보낼 수 있어, CREVIS 작업과 무관하게 남아 있던 안전성 결함입니다. 이를 별도 durable journal로 분리해 재시작 후에는 PowerOn을 재전송하지 않고 `0x2028` 상태 조회만 이어가도록 구현 중입니다. 기존 Move 불확실성 journal과는 섞지 않습니다.
>
> CREVIS 구조 검증기 보강은 적용됐습니다. 현재 정적 구성 상태는 계속 통과하고, IDE 구조 준비 단계는 예상대로 `Coupler` 미생성 때문에 실패합니다. 이제 동일 검증을 제가 다시 독립 실행하고, 병행 중인 Axis PowerOn 복구 구현이 합쳐진 뒤 전체 Release 회귀를 돌리겠습니다.
>
> 검증기 자체도 별도 리뷰에서 네 가지 결함을 잡았습니다. CRLF에서 음성 fixture가 무효화되는 문제, method 지역변수로 선언을 위장할 수 있는 문제, 연결 network 위치와 generated TargetIndex drift를 놓치는 문제입니다. 이 네 가지를 다시 막는 중입니다. 한편 Axis SDK는 post-write 취소까지 보강되어 Release `721/721 PASS`, PowerOn wire 1회와 같은 연결 재사용을 확인했고 이제 WPF 재시작 복구를 붙이고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md` (+12 -0)
>
> </details>

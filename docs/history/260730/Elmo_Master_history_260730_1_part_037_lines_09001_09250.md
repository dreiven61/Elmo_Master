- LASAL SourceOnly/full 정적 계약: PASS
- PowerShell parser 및 LASAL 수정 소스 ASCII 검사: PASS
- working-tree `git diff --check`: PASS

현재 CREVIS의 구성 목록은 표시되지만, 실시간 Node Health/DI/DO는 아직 PLC capability bits 15~17이 꺼져 있고 `0x7E13/0x7E22/0x7E23`이 미구현이므로 나오지 않습니다. [Test2 캡처 분석](./docs/architecture/LMC_ETHERCAT_TEST2_CAPTURE_AUDIT_2026-07-28.md:7)

또한 실제 SDO Write는 편집 문제와 별개로 PLC bit 9 및 승인 allowlist가 꺼져 있어 아직 실행 차단 상태입니다. 커밋·스테이징은 하지 않았습니다. 기존 staged history 파일에는 trailing whitespace 168건이 그대로 남아 있습니다.

<details><summary>74 previous messages</summary>

> 현재 소스와 구현계획을 다시 대조해 다음 미완료 항목을 잡겠습니다. 우선순위는 사용자가 직접 지적한 CREVIS 동적 상태/I/O 경로이며, LASAL IDE 구조가 아직 없으면 외부 편집만으로 진행 가능한 SDK·WPF·정적 계약 부분부터 구현하고 IDE 작업이 필요한 경계는 정확히 분리하겠습니다.
>
> 확인 결과 CREVIS T2 checkpoint는 현재 정확히 실패합니다. `0x7E13/0x7E22` route뿐 아니라 `LMCEcatInputLatch`의 Coupler/InputSlot/OutputSlot client와 helper 선언이 아직 IDE에서 생성되지 않았기 때문입니다. 이 생성영역을 외부에서 위조하지 않고, 그 사이 진행 가능한 다음 상태대기 API를 선정하고 있습니다.
>
> CREVIS 동적 경로와 SDO Write 활성화는 둘 다 현재 IDE-owned 선언이 선행되어야 하므로 임의 편집하지 않습니다. 그 공백 동안 바로 구현 가능한 다음 실제 결함을 잡았습니다. Group Power On/Off ACK도 최종 상태가 아닌데 Power On은 수동 1회 상태확인에 의존하고, 실패 뒤 재시도 시 명령 재송신 위험이 있습니다. 이를 `0x2045` 전용 안정상태 대기 API와 GUI의 “검증만 재개” 흐름으로 바꾸고 있습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 동적 정보는 코드 누락이 아니라 LASAL IDE가 생성해야 하는 client/method/network 선언이 아직 없는 상태로 확정됐습니다. 그 구조를 손으로 합성하지 않고, 지금은 IDE 작업 없이 진행 가능한 Group Power On/Off의 “명령 1회 + 상태 안정 확인/재개” 경로를 구현하고 있습니다. 다음으로 SDK 결과를 받아 WPF까지 연결하겠습니다.
>
> WPF 쪽은 재시도 시 `0x204A/0x204B`를 다시 보내지 않는 분기와, 단일 Read Status 샘플로 Power 상태를 확정하지 않는 규칙까지 같이 적용할 예정입니다. 현재 SDK의 공용 상태 대기 API와 드레인/재연결 테스트가 끝나는 즉시 그 API를 GUI에 묶겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+28 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+99 -64)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+30 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+337 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+8 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+34 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+8 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+7 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -5)
>
> </details>
>
> WPF 연결은 적용했습니다. Power On/Off는 각각 명령을 한 번만 보낸 뒤 3회 연속 상태를 확인하고, 선점·timeout 뒤에는 같은 버튼이 status-only Resume으로 바뀝니다. 단일 Read Status로 pending을 완료하지 않는 smoke도 추가했으며, 지금 SDK의 deadline/cancel/reconnect 계약 테스트와 함께 빌드 오류를 정리하는 단계입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
>
> </details>
>
> 독립 검토에서 두 가지 안전성 결함을 추가로 잡았습니다. 첫째, pending이 없는 단일 `PowerOn=True` 조회가 ACTIVE로 승격되던 경로를 막겠습니다. 둘째, Read Status 응답 대기 중 Power Off/Stop이 예약되면 그 오래된 응답이 UI 상태를 바꾸지 못하도록 응답 직후 generation 검사를 넣습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+19 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+120 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+84 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+208 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+9 -0)
>
> </details>
>
> SDK Debug는 새 테스트 10개를 포함해 `646/646 PASS`입니다. 이제 WPF 보강 테스트의 최종 수치와 API/WPF Release, LASAL 정적 계약을 순서대로 확인하겠습니다. 이 단계는 PC/정적 검증이며 PLC runtime 증거로 취급하지 않습니다.
>
> 마지막 검토에서 Group Enable의 “마지막 상태 응답 도착과 Stop 예약이 겹치는” 경계를 찾았습니다. 결과 적용 전 generation 검사를 추가하고, 이미 SDK에서는 완료됐지만 UI가 stale 결과를 폐기한 경우 새 Enable을 재전송하지 못하게 `Lock state uncertain → Disable required` 복구 상태를 별도로 두겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+48 -10)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+24 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+31 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+95 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+17 -0)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> WPF의 마지막 상태경합 수정에서 남은 우회 경로를 먼저 막고 있습니다. 그룹 재조회로 복구 상태가 지워지는 문제는 확실히 수정하고, 안정된 Power Off를 복구 조건으로 인정할지는 기존 SDK 계약과 맞춰 확인한 뒤 테스트·문서 수치를 함께 갱신하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+105 -20)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+119 -0)
>
> </details>
>
> 기존 SDK 계약은 “동일 세션에서 PowerOn=False가 3회 연속 확인되면 안전 상태로 복구 가능”을 이미 명시하고 있어, Power Off를 단순 우회로 보지 않고 별도 회귀 테스트로 고정했습니다. 반면 그룹 재조회·이름 변경·연결 종료로 복구 플래그가 사라지는 경로는 차단했고, 이제 WPF 전체 테스트를 다시 돌립니다.
>
> 방금 첫 실행은 SDK-style가 아닌 .NET Framework WPF 프로젝트를 `dotnet run`으로 잘못 호출해 XAML 생성 단계가 빠지면서 실패한 것입니다. 소스 오류가 아니라 실행 방식 오류였고, 저장소에 적힌 Visual Studio MSBuild 경로로 다시 검증합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+12 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+25 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+9 -25)
>
> </details>
>
> Debug actual-control smoke는 이제 `44/44 PASS`입니다. 추가된 테스트가 마지막 Enable 응답 경합 뒤에도 `0x2047` 재전송 없이, 정확히 `0x204B` 1회와 `PowerOn=False` 3회 확인으로만 복구되는 경로까지 고정했습니다. 같은 코드를 Release 구성으로도 확인 중입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+5 -0)
>
> </details>
>
> 최종 독립 리뷰에서 실제 우회 두 건이 잡혔습니다. 창 X/Alt+F4가 복구 상태를 메모리에서 없애는 경로와, pending Enable 2/3 상태에서 수동 상태 읽기의 마지막 응답이 안전 명령과 경합하면 SDK 내부 3/3 proof가 남는 경로입니다. 둘 다 그대로 두면 `0x2047` 재전송 방지 계약이 깨지므로 지금 수정하고 해당 경합 테스트를 추가합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+126 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+26 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+39 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+1 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 남은 안전 경합 테스트 1건의 문자열 기대값을 맞춘 뒤, WPF/API를 Debug·Release로 다시 전수 검증하겠습니다. 이어서 현재 문서의 테스트 수와 “PC 검증 대 PLC 실기 검증” 경계도 실제 결과에 맞춰 갱신하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+1 -1)
>
> </details>
>
> WPF 쪽은 Debug/Release 모두 44/44 통과했습니다. 특히 CREVIS 설정 행 표시, SDO 쓰기 중 편집 유지, 수동 상태 응답 폐기, 미해결 Group Enable 상태에서 연결·이름·창 종료 차단까지 함께 통과했습니다. 이제 SDK 647개 전수 회귀와 문서 정합성만 마무리합니다.
>
> SDK 전수 실행에서 알려진 동일 테스트가 다시 1회 실패했습니다. 기능 실패가 아니라 `LegacyEnableBlockedWhileWaitActive`가 대기 상태를 확정하기 전에 두 번째 호출을 시작하는 테스트 타이밍 결함으로 보이지만, 반복됐으므로 단순 재실행으로 덮지 않고 테스트 자체를 결정론적으로 고치겠습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+9 -6)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+7 -5)
>
> </details>
>
> 반복 실패 원인은 제품 로직이 아니라 150ms 실시간 지연에 의존하던 테스트였습니다. 첫 상태 폴 직후의 제어된 대기점으로 바꿔 부하와 무관하게 “wait 활성” 상태를 고정했고, 해당 경합 테스트 50회 연속 통과를 확인했습니다. 전체 SDK는 제가 순차 실행으로 한 번 더 확인하겠습니다.
>
> 독립 리뷰에서 추가 경합창을 찾았습니다. 수동 Group Status가 SDK proof에 반영된 직후, WPF가 화면에 적용하기 전에 Stop/Power Off가 예약되면 화면 결과는 버려도 SDK의 3회 누적값이 남을 수 있습니다. safety 예약 시점에 accepted Enable proof를 원자적으로 무효화하는 경로와 그 정확한 재현 테스트를 추가하겠습니다.
>
> 현재 보강 방향은 “응답을 폐기했다”는 UI 로그만 맞추는 수준이 아닙니다. safety generation 예약과 SDK proof 갱신의 모든 순서를 닫고, 실제 소켓 단절 후 동일 CREVIS/Group 세션 복구 흐름도 fake TCP로 끝까지 재현하는 중입니다. 이 두 검사가 통과해야 최종 수치를 확정하겠습니다.
>
> 두 보강이 모두 들어갔습니다. 새 최종 수치는 SDK 649개, WPF 46개이며, 이제 병렬 빌드 간섭 없이 Debug→Release 순서로 전수 실행해 확정합니다.
>
> 새 안전 보강으로 기능 검사는 통과했지만 기존 두 recovery 테스트의 종료 정리가 더 엄격해진 상태와 맞지 않아 `journal.lock`이 남았습니다. 실제 기능 assertion이 아니라 테스트 창을 닫는 cleanup 결함입니다. recovery를 정상 해제한 뒤 창 수명주기가 끝났는지 확인하도록 정리를 고치고 다시 실행하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+19 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+35 -19)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+9 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+12 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfGroupEnableWaitIntegrationTests.cs` (+7 -7)
>
> </details>
>
> 최종 안전 모델에 맞춰 기존 경합 테스트 의미도 바로잡았습니다. Stop/Power Off가 늦은 3번째 샘플보다 먼저 예약되면 이제 recovery로 오판하지 않고, 기존 ACK는 보존하되 누적 3회 증거만 0으로 되돌린 pending 상태가 됩니다. 이후 Disable 또는 새 PowerOff 3회로만 해제되며 Debug WPF 46/46이 통과했습니다.
>
> <details><summary>Edited 9 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+4 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 구현은 핵심 경합 조건까지 반영된 상태입니다. 이제 문서 표현을 실제 동작과 맞춘 뒤, SDK 649개와 WPF 46개를 Debug/Release에서 순차 재검증하겠습니다. LASAL IDE는 건드리지 않습니다.
>
> 문서에 남아 있던 표현 하나가 실제 구현보다 과도했습니다. 안전 예약과 늦은 상태 응답이 경합한다고 항상 복구 상태가 되는 것은 아니며, 보통은 누적 증명만 초기화하고 ACK를 유지한 채 상태 확인을 재개합니다. SDK 완료가 이미 확정된 뒤 WPF 적용 직전에 안전 예약이 들어온 경우만 복구 상태로 올리도록 문서를 바로잡고 있습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+7 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+6 -5)
>
> </details>
>
> WPF Release 검증은 46/46 전부 통과했습니다. CREVIS 7행/3행 표시, SDO 편집 유지, Group Enable 안전 경합과 실제 소켓 단절 복구 회귀까지 포함된 결과입니다. 이어 SDK 전체를 Debug와 Release 순서로 돌립니다.
>
> SDK도 Debug 649/649, Release 649/649로 모두 통과했습니다. 이제 독립 검토 결과를 반영하고, 변경분의 공백/정합성 검사만 마무리하겠습니다.
>
> 독립 검토에서 기존 경합 수정 자체는 이상이 없었지만, 복구 상태에 두 가지 우회가 남았습니다. 재접속 시 그룹 이름만 같으면 다른 reference/PLC도 허용될 수 있고, 강제 종료 후에는 메모리 복구 잠금이 사라집니다. 이 둘은 안전상 P1이므로 현재 단계에서 identity 고정과 영속 복구 기록까지 이어서 막겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/FakeRpcServer.cs` (+7 -0)
>
> </details>
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+525 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+25 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.GroupProfileLockRecovery.cs` (+30 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+26 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+20 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+41 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+14 -0)

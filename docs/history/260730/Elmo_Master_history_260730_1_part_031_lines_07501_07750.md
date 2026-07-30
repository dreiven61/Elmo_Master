- API Debug/Release: 각각 `540/540 PASS`
- WPF actual-control Debug/Release: 각각 `7/7 PASS`
- `git diff --check`: PASS
- 기존 staged 히스토리 파일의 trailing whitespace 때문에 `git diff --cached --check`만 기존과 동일하게 실패
- 커밋·staging·테스트 폴더 복사는 하지 않았습니다.

새 기능이 포함된 검증용 실행 파일은 [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/bin/codex_queued_cancel_final/Release/LasalMotionControlApiExample.exe)입니다. 현재 실행 중이던 기존 GUI에는 자동 반영되지 않습니다.

CREVIS는 현재 PC 쪽에서 configured topology 7행/CREVIS 3행 표시까지 통과했습니다. 동적 Node 상태와 DI 정보는 LASAL T2 구조 생성, Rebuild/Link 및 PLC 다운로드 전에는 올라오지 않습니다. 실제 queued-cancel 및 CREVIS live PASS도 아직 PLC 증거가 아닙니다.

다음 개발 증분은 D4 Double WPF의 durable recovery 연결을 gate-off 상태로 진행하는 것이 우선입니다.

<details><summary>10 previous messages</summary>

> 현재 소스를 다시 기준으로 D4 Double의 WPF 연결부터 이어가겠습니다. 이번 증분은 live gate를 열지 않은 채 journal 수명주기, 재시작 복구 진입점, mutation interlock을 실제 MainWindow에 연결하고 zero-wire 시험으로 고정하는 작업입니다. LASAL IDE나 실행 중인 사용자 GUI는 건드리지 않겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> D4 코어에는 이미 단일 writer journal과 정확한 복구 identity/checkpoint 모델이 있습니다. 지금 확인되는 공백은 그 journal을 `MainWindow` 수명주기에 열고, 재시작 시 발견된 미해결 기록을 앱 전체 mutation interlock과 화면 상태에 연결하는 부분입니다. 자동 재전송은 추가하지 않고, 복구 실행 gate도 계속 닫아 둡니다.
>
> 구현 범위를 확정했습니다. 이번 증분은 D4 저널을 실제 WPF 프로세스가 독점 오픈하고, 활성 기록·오픈 실패를 기존 전역 interlock에 합치며, 화면에 정확한 복구 identity를 표시하는 단계입니다. 별도 `Recover Double Journal` 진입점은 추가하지만 `ReconnectRecovery=false`를 유지해 어떤 강제 호출도 첫 줄에서 차단합니다. 자동 inventory/adopt/release는 실행하지 않습니다.
>
> <details><summary>Created a file, edited 9 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+19 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+13 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+15 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+13 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+285 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+24 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+152 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+91 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+10 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+9 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMutationRecoveryProcessTests.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+80 -1)
>
> </details>
>
> 현재 추가된 실제 프로세스 smoke는 D4 활성 저널을 두 번 재시작해 확인합니다. 각 실행에서 WPF가 single-writer lock을 유지하고, 강제 종료 전후 저널 바이트가 동일하며, Connect 기본 3개 요청 외 `0x7E40..0x7E4F`가 0건인지 검사합니다. 별도로 저널이 이미 잠긴 경우 신규 mutation admission이 `MutationJournalUnavailable`로 닫히는 시험도 추가했습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+16 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+11 -7)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+7 -4)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+9 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+2 -2)
>
> </details>
>
> 리뷰에서도 P1/P2 문제는 없었습니다. 남은 정리는 현재 문서에 남아 있는 이전 `7/7` 표기를 이번 `9/9` 결과와 맞추고, 변경 범위의 diff/스테이징 상태를 마지막으로 확인하는 것입니다. 기존에 스테이징된 대형 히스토리 파일은 건드리지 않습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+7 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+7 -5)
>
> </details>
</details>

D4 Double WPF 저널 수명주기 증분을 완료했습니다.

- Double 전용 journal open/lock/status와 전역 mutation·Close interlock 연결
- 잠김·손상·오픈 실패 시 신규 mutation fail-closed
- `Recover Double Journal` UI 추가. `ReconnectRecovery` gate는 계속 닫혀 있어 강제 클릭도 zero-wire
- 재시작 시 자동 inventory/adopt/release는 수행하지 않음
- 실제 WPF 프로세스 강제 종료·2회 재시작 후 journal byte/identity 보존 및 `0x7E40..0x7E4F` 송신 0건 검증
- Debug/Release WPF smoke 각각 `9/9 PASS`
- 리뷰 결과 P1/P2 문제 없음
- `git diff --check` 통과

새 빌드는 [Release 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/bin/codex_d4_lifecycle/Release/LasalMotionControlApiExample.exe)에서 확인할 수 있습니다. 핵심 구현은 [MainWindow.RecorderDoubleRecovery.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs:47)입니다.

이번에는 LASAL 소스·IDE, 테스트 폴더, Git staging/commit은 건드리지 않았습니다. 기존 staged 히스토리 파일의 trailing whitespace 문제도 그대로 보존했습니다.

다음 구현 대상은 Double qualification 전용 retained state와 명시적 cleanup, reconnect recovery wire adapter입니다. PLC build/RAM/jitter 및 실제 A/B bank 증거 전까지 live gate는 열지 않습니다.

<details><summary>31 previous messages</summary>

> 다음 증분은 D4 Double의 실제 WPF 어댑터입니다. 세 proof gate는 계속 닫아 둔 상태에서, qualification 실행 객체·retained handle·명시적 same-session cleanup·재시작 recovery를 core orchestrator와 실제 SDK 호출에 연결하겠습니다. 자동 Release나 자동 재전송은 넣지 않고, 테스트는 fake RPC와 zero-wire 경계를 먼저 고정합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 어댑터 경계를 확인했습니다. Qualification은 현재 연결 객체와 Diagnostics 인스턴스를 동일 세션 토큰으로 고정하고, 하나의 recovery GUID에서 nonzero `RequestedConfigId`를 결정적으로 파생하겠습니다. 성공하더라도 두 bank와 configuration은 자동 해제하지 않고 전용 retained state에 남기며, 해제는 별도 명시 동작으로만 진행합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+34 -34)
>
> </details>
>
> 이제 실제 코드를 붙이고 있습니다. 새 경로는 기존 Single/Ring용 한 개의 Recorder 필드를 재사용하지 않고, Double qualification scope와 reconnect-adopt 결과를 별도로 보존합니다. 특히 Adopt 직후 Status로 configuration metadata를 채우고, `Armed/Recording`이면 Stop→Ready 확인 뒤에만 B→A→configuration 순서로 Release하도록 묶습니다.
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+1354 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+55 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+13 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+31 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+14 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+38 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+98 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+43 -35)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+16 -16)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+18 -8)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> D4 실제 어댑터는 붙은 상태입니다. 지금은 리뷰 결과를 반영하고 Debug/Release 전체 검증을 다시 돌린 뒤, “구현됨”과 “PLC 실증 전이라 게이트는 닫힘”을 문서에 정확히 분리해 갱신하겠습니다. LASAL IDE·테스트 폴더·Git staging은 건드리지 않습니다.
>
> Debug 재검증은 경고·오류 없이 통과했고 WPF 회귀도 11/11입니다. 특히 사용자가 지적한 두 항목인 Cravis 자동 표시와 SDO Write 중 편집 가능 회귀가 함께 통과했습니다. 이제 Release와 API 전체 회귀를 확인합니다.
>
> 리뷰에서 실제 P1을 잡았습니다. reconnect의 첫 Adopt 응답은 설정 메타데이터가 비어 있는데 현재 검증이 그 메타데이터를 즉시 요구해, 게이트를 열면 첫 bank 소유권을 가져온 뒤 실패하는 문제입니다. 각 Adopt 직후 exact Status로 handle을 보강한 다음 retained state에 넣도록 수정하겠습니다. 현재 게이트가 닫혀 있어 실제 wire에는 아직 노출되지 않았습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+14 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+56 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleRecoveryOrchestrator.cs` (+4 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+39 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+2 -2)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+92 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+12 -0)
>
> </details>
>
> 문서는 D4 어댑터 상태와 11/11 WPF 회귀 기준으로 동기화됐습니다. 방금 raw-Adopt 회귀를 1개 추가해 API 총계가 540에서 541로 늘었으므로, 최종 검증 후 현재 상태 표의 숫자도 541/541로 맞추겠습니다.
>
> 추가로 ACK 성공 뒤 journal 저장만 실패하는 crash-window를 점검했습니다. 현재는 같은 세션에서 이미 해제된 handle을 건너뛰어 다음 단계가 막히고 재접속을 강제합니다. wire 재전송 없이 exact retained handle로 pending 확인만 저장하거나 최종 resolve만 수행하는 경로를 추가하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleDurableReleaseCoordinator.cs` (+58 -30)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleDurableReleaseCoordinator.cs` (+61 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+6 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleRecoveryOrchestratorTests.cs` (+141 -0)
>
> </details>
>
> 리뷰에서 third Start 비정상 성공 경로의 계약 불일치를 확인했습니다. 현재 journal은 A/B 두 bank만 기록하므로, 충돌 가능한 third handle에 durable intent를 만들 수 없습니다. 따라서 `UnexpectedThird→...`를 같은 세션에서 해제한다고 표시한 것은 잘못입니다. 이 경우는 어떤 Release도 보내지 않고 연결을 끊은 뒤 exact inventory 재조정만 허용하도록 수정합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+25 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+26 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+10 -7)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecorderDoubleRecovery.cs` (+1 -1)
>
> </details>
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
>
> </details>
>
> 리뷰에서 두 가지 P2를 더 확인했습니다. 첫째, “명확히 미적용”으로 판정된 Release는 journal intent가 남아 같은 세션 재시도가 막혔습니다. exact pending target일 때만 기존 intent를 재사용하도록 맞춥니다. 둘째, 실패 뒤 확인 체크가 남아 새 남은-resource 계획을 재확인하지 않고 재시도할 수 있었습니다. 확인은 매 실행 시도 시작 때 즉시 소비하도록 바꿉니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+8 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecorderDoubleDurableReleaseCoordinator.cs` (+24 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 표시와 SDO 편집 문제 수정은 유지한 채, 지금은 D4 이중 버퍼의 실패·재접속 경로를 끝까지 닫고 있습니다. 특히 “세 번째 Start가 예상 밖으로 성공한 경우”가 저널 장애로 잘못 굳어 재접속 점검까지 막는 문제를 먼저 바로잡고, 재시도 회귀 테스트와 문서를 맞춘 뒤 Debug/Release를 다시 검증하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RecorderDoubleQualificationJournalBridgeTests.cs` (+150 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+17 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+20 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+10 -0)
>
> </details>
>
> 저널 오류를 두 종류로 분리했습니다. 디스크/권한/손상 같은 실제 런타임 장애만 저널을 “사용 불가”로 만들고, 예상 밖의 세 번째 bank처럼 의미상 충돌인 `InvalidOperationException`은 건강한 저널을 유지합니다. 또한 Configure/Start 결과 handle이 없는 경우에는 같은 세션 Release를 활성화하지 않고 재접속 복구로 보내도록 admission을 보강했습니다.
>
> 추가한 회귀 검증까지 Debug에서 통과했습니다: API `545/545`, WPF `12/12`. 여기에는 CREVIS 자동/수동 표시 복구, 일반 RPC 중 SDO Write 편집 가능, D4 pending Release intent의 동일 target 재시도, 의미상 저널 충돌이 recovery를 영구 차단하지 않는 검증이 포함됩니다. 이제 Release 빌드와 문서 정합성을 병행 확인 중입니다.
>
> Release 검증도 완료됐습니다: API `545/545`, WPF `12/12`, 빌드 오류·경고 0입니다. 별도 코드 리뷰에서도 현재 변경분의 추가 P1/P2 결함은 없었고, 남은 작업은 코드와 어긋난 과거 문서 표현을 정리하는 것입니다. LASAL IDE·PLC·실기 통신은 이번 검증에서 실행하지 않았습니다.
>
> 문서는 현재 “core 비내구 orchestrator의 기능”과 “WPF 내구 저널 adapter에서 실제 허용하는 기능”을 분리해 수정 중입니다. 핵심은 정상적인 세 번째 `ResourceBusy` 확인 시에만 `B → A → Configuration` 정리를 허용하고, 세 번째 Start 성공·불명확 결과에는 같은 세션 Release를 0건으로 유지하는 것입니다.
>
> 코드·테스트는 끝났고 문서 8개 중 6개 정합성 수정이 완료됐습니다. 현재 아키텍처 상태 문서와 잔여 구현 계획 두 파일의 테스트 수치와 D4 안전 경계를 마지막으로 맞추고 있습니다.

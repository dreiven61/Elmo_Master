>
> UI 골격이 들어왔습니다. 별도 `LIVE Single Axis qualification` 영역에서 세 가지 물리 안전 확인과 raw DINT 시험값을 명시하도록 했고, 도착 후 보내는 Stop은 “주행 중 정지 성능 증명”으로 과장하지 않도록 화면에도 경계를 표시했습니다. 이제 실제 durable 전송 코어와 무재전송 회귀를 연결 중입니다.
>
> 현재 구현은 단순히 버튼을 다시 켜는 수준이 아니라, 기존 Power/Move/Stop/PowerOff의 명령 전 저널과 accepted-once 경계를 그대로 공용화하는 작업입니다. 그래서 연결이 다시 흔들려도 Move나 Power 명령이 자동 재전송되지 않도록 유지하면서 runner를 붙이고 있습니다.
>
> 구현 담당 작업이 실제 core 파일을 저장하지 못한 상태여서 충돌을 막고 제가 이어받았습니다. UI 초안은 보존하고, 이제 작은 패치 단위로 durable primitive 기반 runner를 직접 완성하겠습니다.
>
> <details><summary>Created a file, edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+48 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+0 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+488 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 이전 작업을 이어서 Single Axis 실기 검증 러너가 실제로 저장됐는지부터 확인하겠습니다. 먼저 누락된 전송/정리 헬퍼를 완성하고, 그 다음 가짜 PLC 회귀시험으로 명령 순서와 “재전송 없음”을 검증하겠습니다.
>
> UI 골격은 저장돼 있지만 핵심 전송 헬퍼 10개가 빠져 있어 현재 상태로는 빌드되지 않습니다. 전송 경로 구현과 회귀시험 작성을 병렬로 진행하되, 기존 복구 저널의 정확한 계약을 그대로 재사용하겠습니다.
>
> 명령 경로는 다음 안전 계약으로 고정했습니다: `Power On 1회 → fresh ready status → 시작 위치 → Relative Move(0x20A0) 1회 → 실제 이동 관측 + standstill 3회 → 최종 위치 3회 → Stop 1회 → Power Off 1회`. 취소나 실패가 Move 이후 발생해도 Move는 재전송하지 않고, 정확한 PLC/축 식별자가 유지될 때만 Stop과 Power Off 정리를 수행합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+516 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+286 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+43 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -0)
>
> </details>
>
> 앱과 새 시험 코드는 컴파일됐습니다. 지금 첫 실행에서 가짜 PLC 응답 순서를 실제 구현과 맞추고 있으며, 실패가 나오면 명령 카운트와 request ID를 기준으로 즉시 조정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisQualificationIntegrationTests.cs` (+20 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+14 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+54 -30)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+5 -5)
>
> </details>
>
> <details><summary>Created a file, edited 11 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/DistributionSemanticPolicy.ps1` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/Test-LmcDistributionSemanticPolicy.ps1` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_SDO_WRITE_SAME_VALUE_QUALIFICATION_2026-07-28.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+31 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+16 -0)
> - Wrote `./docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md` (+72 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+5 -1)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+4 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 안전 리뷰에서 실제 전송 경계와 관련된 결함 3건을 확인했습니다. 지금 바로 수정한 뒤 Debug/Release 전체 테스트와 배포 정책 검증까지 다시 돌리겠습니다. 특히 Motion 복구 기록은 Stop 검증 전에는 지우지 않고, Power identity 검증은 durable 기록 해제 전에 수행하도록 바꿉니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisPowerOnRecovery.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+69 -29)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+134 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+8 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+13 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+24 -13)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+22 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+6 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+16 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+26 -21)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MotionUncertaintyRecovery.cs` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+20 -14)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+19 -14)
>
> </details>
>
> 현재 수정으로 외부 Axis Stop/Power Off가 끼어들 때 동일 명령을 다시 보내지 않는 분기까지 넣었습니다. 다만 리뷰 중 “Power On 완료 후 프로그램이 비정상 종료되면 runner 자체의 진행 상태는 영구 기록되지 않는다”는 별도 한계를 확인했습니다. 이 범위에서는 PLC identity가 바뀐 경우 자동 Stop/Power Off를 금지하고 명시적으로 실패 처리하며, 문서에도 이 경계를 실제 동작대로 정확히 적겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+55 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+14 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+27 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+1 -0)
>
> </details>
>
> WPF 소스는 현재 Debug 재빌드가 통과했습니다. 다음으로 SDK 자체 회귀를 Debug/Release로 병렬 확인하는 동안, 별도 테스트 작업에서는 Build drift·pre-wire 취소·외부 Stop/Power Off 간섭 시나리오를 추가하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+76 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+7 -1)
>
> </details>
>
> 중점 안전 회귀 8개가 현재 Debug에서 모두 통과했습니다. 포함 범위는 실제 Power/Move/Stop/Power Off 정상 순서뿐 아니라, 전송 전 취소 0건, DiagnosticsBuild drift 시 Move/Stop/Power Off 0건, 외부 Axis Stop/Power Off 간섭 시 중복 전송 0건입니다. 이제 Release까지 포함한 전체 278개 회귀를 고정하겠습니다.
>
> 전체 WPF 회귀는 Debug/Release 두 구성을 동시에 실행 중입니다. 중점 8개는 이미 반복 통과했고, 현재는 기존 270개와의 전체 상호작용을 확인하는 단계입니다.
>
> 전체 278개 중 1개가 실패했습니다. 기능 실패가 아니라 private recovery helper에 final-identity 콜백 인자를 추가하면서 기존 reflection 기반 회귀가 예전 인자 수로 호출한 문제입니다. 해당 회귀 호출을 새 계약에 맞춘 뒤 Debug/Release 전체를 다시 실행하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+2 -1)
>
> </details>
>
> 최종 소스 재리뷰에서도 새 P0/P1 코드 결함은 발견되지 않았고, 중점 8개가 다시 통과했습니다. 남은 것은 문서가 command-level durability와 whole-sequence crash continuity를 구분하도록 고치는 작업이며, Debug 전체 회귀는 계속 실행 중입니다.
>
> Debug 전체는 278/278 통과했습니다. Release 전체도 같은 278개를 순차 실행 중이며, 이 결과가 끝나면 문서 수치·Release 바이너리 해시·최종 diff 검사를 한 번에 맞춥니다.
>
> <details><summary>Edited 22 files</summary>
>
> - Wrote `./docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md` (+19 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+7 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+6 -5)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+5 -4)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+2 -2)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/GROUP_RESET_STABLE_MEMBER_ERROR_CLEARANCE_2026-07-31.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md` (+3 -0)
>
> </details>
>
> Release 전체도 278/278 통과했습니다. 문서에는 실제 구현과 동일하게 “명령 단위 durable recovery는 제공하지만 전체 runner의 crash 연속성 journal은 아직 없다”는 한계를 명시했고, 현재 격리 상태에서 새 명령을 열려면 stale record 보관·폐기 후 재연결해야 한다는 절차도 반영했습니다. 이제 최종 whitespace/status 검사를 합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 구현은 끝났고, 지금은 전체 작업 트리의 공백 오류와 이번 변경 파일 범위를 마지막으로 확인하겠습니다. 실제 PLC/드라이브 구동은 이 환경에서 대신 실행하지 않고, 빌드·자동 테스트·정적 계약 결과와 현장 실행 절차를 분리해 보고하겠습니다.
</details>

수정 완료했습니다. 새 Release 실행 파일은 실제 PLC로 Power/Motion/Stop/PowerOff 및 제한된 SDO Write를 전송할 수 있습니다.

- `Run LIVE Axis Qualification` 실행 순서:
  `Power On → 상태 확인 → Move Relative → 위치 검증 → Stop → Power Off`
- 각 명령은 한 번만 전송하며, 취소/실패 시 Stop·PowerOff 안전 정리를 수행합니다.
- SDO Write는 Axis1 `0x2F00:24`, Int32/4-byte만 허용됩니다. 동일값 4단계 검증을 통과해야 실제 Write 버튼이 열립니다.
- 화면의 `RECOVERY IDENTITY READ-ONLY QUARANTINE` 상태는 우회하지 않았습니다. 새 버전에서 물리 상태를 확인한 뒤 `Archive and Retire Stale Recovery` 체크 및 실행 → 연결 종료 → 재연결 → `Load Axis` 순으로 해제해야 합니다.

검증 결과:

- WPF Debug/Release: 각각 `278/278 PASS`
- 신규 실제 Axis 송신 테스트: `8/8 PASS`
- SDK Debug/Release: 각각 `1006/1006 PASS`
- LASAL 정적 계약, 배포 정책 `28/28`, Manifest `56/56 PASS`
- `git diff --check`, `git diff --cached --check` 통과
- 스테이징 및 커밋은 하지 않았습니다.

실행 파일: [LasalMotionControlApiExample.exe](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Release/LasalMotionControlApiExample.exe)
버전: `0.9.1.0`
SHA-256: `890EAC643EE6A70566DDB50CD3B96989DE9725158B55A4DE9A89F5348B67E0B6`

현장 실행 절차와 판정 기준: [SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md](./docs/architecture/SINGLE_AXIS_LIVE_QUALIFICATION_2026-07-31.md)

단, 실제 PLC/실축 송신은 이번 작업에서 수행하지 않았습니다. 현재 결과는 빌드·가상 RPC·정적 계약 검증까지입니다. 실축 시험 시 이동 방향/거리, STO·비상정지, 기구 간섭 및 단독 제어권을 먼저 확인해야 합니다.

<details><summary>44 previous messages</summary>

> 현재 소스와 개발 계획 문서를 다시 대조해서 다음 미완료 항목을 선정하겠습니다. 우선 작업 트리의 기존 변경을 보존한 채 계획·진척 문서의 미완료 게이트와 실제 구현/테스트 간 차이를 확인하고, 가장 우선순위가 높은 기능부터 코드와 검증까지 이어가겠습니다.
>
> 문서상 최상위 미완료는 PLC 다운로드·실축 검증이지만, 그 부분은 장비 없이 완료 처리할 수 없습니다. 병렬로 현재 계획의 소프트웨어 측 다음 항목, 배포 후보 갱신 필요성, 상위 API 공백을 각각 대조해 장비 없이도 실제로 진전시킬 수 있는 최우선 작업을 확정하겠습니다.
>
> 다음 작업은 P0-D의 오래된 배포 후보를 현재 Group Reset·Single Axis 구현과 일치시키는 것으로 확정되는 중입니다. 이 과정에서 외부 DOCX/PDF 매뉴얼을 다시 만들어야 하므로 `doc`과 `pdf` 스킬의 렌더링·시각 검증 절차를 적용하겠습니다. 원본 Markdown과 canonical Distribution은 그대로 두고 새 후보만 생성합니다.
>
> 우선순위를 바꿨습니다. 배포 후보 갱신보다 먼저, 현재 문서가 명시한 실제 안전 공백인 Single Axis 전체 시퀀스의 재시작 복구 journal을 구현하겠습니다. 명령별 journal은 이미 있지만 Power On 완료 후 Move journal이 생기기 전 프로세스가 죽으면 Power Off 의무가 사라지는 구간이 있으므로, 이 구간부터 닫은 뒤 새 매뉴얼·배포 후보를 만들겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/status/WORKTREE_BASELINE_BEFORE_AXIS_QUALIFICATION_SEQUENCE_JOURNAL_2026-07-31.md` (+41 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 축 단위 명령 저널 사이에 남아 있던 “Power On 안정 확인 후 Move 저널 생성 전” 복구 공백을 막는 작업을 이어가고 있습니다. 전 시퀀스 저널 구현 결과를 받은 뒤 MainWindow 복구 경로와 테스트까지 연결하겠습니다.
>
> 전 시퀀스 저널은 명령을 재전송하지 않는 상위 복구 장부로 구현 중입니다. 기존 Power/Stop/Motion 저널을 대체하지 않고, 각 하위 저널이 해제되기 전에 시퀀스 단계가 먼저 디스크에 확정되도록 연결할 예정입니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/RecoveryRecordRetirementLedger.cs` (+13 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+24 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+37 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+14 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+10 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+0 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.TopologyIo.cs` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/LasalApiWpfTestApp.SmokeTests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Axis.cs` (+2 -1)
>
> </details>
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs` (+66 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.RecoveryRecordRetirement.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MutationJournal.cs` (+17 -10)

> 최종 WPF 재검증 명령이 도구의 10초 제한에 걸려 중간 종료됐습니다. 빌드 결함으로 판정하지 않고, 방금 만든 임시 출력과 제가 띄운 프로세스만 식별해 정리한 뒤 충분한 제한시간으로 다시 실행하겠습니다. 기존 사용자가 실행 중인 WPF 프로세스는 건드리지 않습니다.
>
> 취소 blocker는 수정됐습니다. 이제 status 응답 도중 취소돼도 TCP 연결을 끊지 않고 응답을 안전하게 비운 다음 `LMCSdoReadWaitCanceledException`을 반환하며, 같은 ticket을 재조회하는 회귀 시험까지 추가해 Debug/Release 135/135가 통과했습니다. WPF의 reserved 상태 필드 표기도 제거했습니다.
</details>

Phase 1 구현과 테스트 화면까지 완료했습니다.

- Admin capability/축·그룹 semantic read 구현: [LmcAdmin.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcAdmin.cs:11)
- Drive operation mode/non-atomic status 구현: [LmcAxisDriveReads.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcAxisDriveReads.cs:11)
- PI/Bulk facade와 4-domain error catalog 구현
- WPF `Read-only API` 탭 추가: [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1080)
- WPF 처리 코드: [MainWindow.ReadOnlyApi.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs:11)
- SDO status RPC 도중 취소 시 연결이 Faulted 되던 결함도 수정했습니다: [LmcDiagnosticsD5.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs:400)

검증 결과:

- C# Debug: 135/135 PASS
- C# Release: 135/135 PASS
- LASAL SourceOnly/full 정적 계약: PASS
- WPF Debug/Release 빌드: PASS
- WPF Debug/Release 3초 기동 smoke: PASS
- `git diff --check`, 신규 파일 whitespace: PASS

다음은 실물 검증 단계입니다.

1. 현재 실행 중인 PID 47068 WPF는 변경 전 바이너리이므로 종료합니다.
2. LASAL에서 최신 source를 Rebuild/Link하고 PLC에 다운로드합니다.
3. WPF를 다시 빌드·실행합니다.
4. `Read-only API` 탭에서 다음 순서로 테스트합니다.
   - `Refresh Admin Capabilities`
   - 축 1~4의 6개 Axis Parameter
   - Group `0x0100`의 3개 Parameter와 `All`
   - `Get Drive Operation Mode`
   - `Read Drive Status`
5. 이 과정의 패킷을 캡처하면 Phase 1 runtime gate를 닫을 수 있습니다.

상세 시험 순서는 [WPF README](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/README.md:142), 전체 설계와 다음 단계는 [Phase 설계 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:313)에 갱신했습니다.

커밋은 아직 하지 않았습니다. 기존 변경과 캡처 파일을 그대로 보존했습니다.

> 구현된 내용 정리해서 커밋하고 계속 이어서 구현 진행해

<details><summary>24 previous messages</summary>

> 먼저 현재 68개 변경을 기능별로 분류해 Phase 0/1 구현·문서·시험만 한 커밋으로 정리하겠습니다. 패킷 캡처와 목적이 다른 변경은 제외하고, 커밋 직전 전체 검증을 다시 확인한 뒤 다음 단계에서는 실물 IO가 필요한 Homing보다 정적 계약으로 안전하게 진행 가능한 Phase 2 항목부터 착수하겠습니다.
>
> 현재 변경은 세 묶음으로 정리할 수 있습니다: SDO 캡처 분석 기록, 실제 소스·자동시험, 설계·사용자 문서입니다. 이 순서로 커밋을 분리하면 이후 Phase 2 변경이 기존 작업과 섞이지 않고, 문제 발생 시 원인별로 되돌릴 수 있습니다. 지금 커밋 전 리뷰와 범위 감사를 병행 중입니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+12 -9)
> - Wrote `./LMC_Library/README.md` (+7 -4)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+6 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+7 -4)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -4)
>
> </details>
>
> 첫 번째 실험 분석 커밋의 staged whitespace 검사에서 문서 2개의 EOF 빈 줄이 검출됐습니다. 내용 문제는 아니지만 커밋 기준에 맞게 정리한 뒤 다시 검사하겠습니다. raw `.pcapng`는 계속 제외됩니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Error_analysis_2026-07-22.md` (+0 -1)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Slave123_analysis_2026-07-22.md` (+0 -1)
>
> </details>
>
> 정리 커밋 3개를 완료했고 worktree는 깨끗합니다. 이어서 Phase 2의 첫 항목은 `0x7D22 GroupMoveLinearRelative`로 확정했습니다. 새 IO나 Network 배치가 필요 없고, 기존 4축 group safety gate와 완료 감시를 재사용하면서 LASAL의 `MoveRelativeCoord`를 직접 호출하는 구조로 구현합니다. Homing·SetPosition은 현재 IO/상태 계약이 없어 계속 차단합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 기존 변경분은 목적별 3개 커밋으로 정리됐고 현재 작업 트리는 깨끗합니다. 이제 다음 단계인 `0x7D22 GroupMoveLinearRelative`를 C# API·LASAL 처리·WPF 테스트 UI로 나눠 병렬 구현한 뒤, 통합 검증과 문서 갱신까지 진행하겠습니다. 먼저 LASAL IDE 실행 여부와 현재 커밋 상태를 다시 확인하겠습니다.
>
> 병렬 작업 범위는 겹치지 않게 분리했습니다. 저는 그동안 현재 요구사항 추적표와 설계 문서의 Phase 2 구간을 대조해, 구현 완료 뒤 바로 반영할 문서·검증 항목을 확정하겠습니다.
>
> 설계 문서를 대조해 보니 기존 문서가 Phase 2를 “read-only 실물 검증 후”로만 남겨 둔 상태입니다. 이번 변경은 객체/Network 추가 없이 기존 `LMCRobot.MoveRelativeCoord`를 쓰는 독립 슬라이스라 진행 가능하지만, 성공 판정은 소스·정적 시험까지로 제한하고 LASAL IDE 빌드/다운로드와 실제 이동 시험은 별도 게이트로 남기겠습니다. 오류 카탈로그도 신규 상세 코드 9/10/11에 맞춰 함께 확장합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcErrorCatalog.cs` (+9 -0)
>
> </details>
>
> 현재 C# 쪽은 wire builder/parser와 capability/detail 확장까지 들어가고 있고, LASAL 쪽은 기존 absolute move의 안전 조건을 그대로 적용해 상대이동 분기를 작성 중입니다. 이번 명령은 “PLC가 요청을 수락했다”는 ACK만 반환하며, 실제 이동 완료·오류는 기존 `GroupReadStatus(0x2045)` 모니터 경로로 판정하도록 유지합니다.
>
> WPF 상대이동 화면은 먼저 완료됐고 Debug/Release 빌드와 3초 기동 smoke까지 통과했습니다. C# 신규 시험은 총 143개로 늘었으며, 제가 추가한 오류 카탈로그 매핑을 포함한 최종 Debug/Release 재실행 중입니다. 이제 통합 diff에서 안전 추적·응답 오류 처리·wire offset을 직접 재검토하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ErrorCatalogTests.cs` (+1 -1)
>
> </details>
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+23 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+27 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+23 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+10 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -2)
>
> </details>
>
> 통합 리뷰에서 실제로 수정해야 할 안전 경합 1건을 찾았습니다. 상대이동은 내부에서 capability 조회 후 motion을 보내는데, 그 두 RPC 사이에 Stop/PowerOff 요청이 들어오면 기존 UI gate만으로는 뒤의 `0x7D22` 송신을 막지 못합니다. 이 상태로 커밋하지 않고, capability 사전확인과 실제 motion 송신 사이에도 safety generation을 재검사하도록 API/UI 경계를 보강하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/README.md` (+6 -5)
> - Wrote `./LMC_Library/README.md` (+4 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+19 -15)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+23 -10)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+6 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+12 -12)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+64 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+19 -1)
>
> </details>
>
> 안전 경합과 parser 범위 문제를 모두 수정했습니다. 최종 C# 시험은 신규 안전·소유권·parser 케이스 5개가 더해져 Debug/Release 각각 `148/148`입니다. prepared capability는 같은 connection/session에만 유효하고, WPF gate 내부에서는 이제 capability 재조회 없이 `0x7D22` 한 번만 전송됩니다. 문서의 임시 `143/143` 표기를 최종 수치로 맞추고 있습니다.
>
> <details><summary>Edited 18 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+15 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcErrorCatalog.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs` (+1 -1)
>
> </details>
>
> 최종 독립 리뷰에서 High/Medium 잔여 결함은 없었습니다. 마지막 Low 항목인 “Phase 1 read” 전용 오류 문구도 Admin 전체 명령에 맞게 고쳤습니다. 이제 최종 빌드·정적 계약을 다시 통과시킨 뒤 기능 변경과 문서 변경을 분리해 두 개 커밋으로 정리하겠습니다. 실제 LASAL IDE build/download와 PLC 상대이동 시험은 커밋 완료 범위에 포함하지 않습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+2 -0)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+4 -0)
>
> </details>
</details>

구현·문서 정리와 커밋까지 완료했습니다. 작업 트리는 깨끗합니다.

새 커밋:

- `db6575d feat(api): add phase 2 group relative motion`
- `4fd7db2 docs(api): document phase 2 relative motion`

주요 구현:

- `0x7D22 GroupMoveLinearRelative`
- `MoveLinearRelativeEx[Async]` 공개 API
- LASAL `LMCRobot.MoveRelativeCoord` 실행 경로
- WPF `Move Linear Relative` 버튼과 Stop/PowerOff 안전 추적
- capability 조회와 motion 송신 사이의 안전 경합 수정
- connection/session-bound capability 검증
- Admin detail 9/10/11 및 native 오류 보존
- PLC capability `0x3 → 0x7`

검증 결과:

- C# Debug/Release: 각각 `148/148 PASS`
- LASAL SourceOnly/full static: PASS
- WPF Debug/Release build: PASS
- WPF 3초 startup smoke: PASS
- `git diff --check`: PASS
- LASAL 변경 소스 7-bit ASCII: PASS
- 기존 실행 중인 WPF PID 47068은 건드리지 않음
- LASAL Network와 `Classes.lcb` 변경 없음

최신 설계는 [요구사항 커버리지 및 구현 설계](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md), wire 계약은 [DINT_PACKET_MAP.txt](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt)에 반영했습니다.

남은 것은 LASAL IDE Rebuild/Link, PLC 다운로드와 작은 상대거리 실동작·Stop 복구·패킷 캡처입니다. 새 PLC의 `FeatureBits=0x7`은 이전 DLL이 거부하므로 새 DLL과 PLC를 같이 배포해야 합니다. 이후 코드 단계는 axis velocity override 계약 확정이며, Homing/SetPosition은 물리 IO와 상태 계약이 확정되기 전까지 보류하는 것이 맞습니다.

> 내가 와이어샤크로 캡쳐해야되는 테스트 리스트좀 알려줘

<details><summary>2 previous messages</summary>

> 현재 구현 기준으로 “반드시 캡처할 항목”과 “오류 분기용 추가 캡처”를 나눠 정리하겠습니다. WPF에서 바로 가능한 시험과 별도 테스트 호출이 필요한 시험도 구분해서, 파일명과 예상 command ID까지 맞춰 드리겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
</details>

현재 구현을 실물 PLC 기준으로 닫으려면 우선 아래 8개를 캡처하면 됩니다.

캡처 전 최신 LASAL 프로젝트를 PLC에 다운로드하고, 최신 DLL로 빌드된 테스트 프로그램을 사용하세요. `Refresh Admin Capabilities` 결과가 `FeatureBits=0x00000007`이 아니면 나머지 시험을 중단해야 합니다.

## 필수 캡처

| No. | 저장 파일명 | 실행할 시험 | 정상 패킷 순서 |
|---:|---|---|---|
| 1 | `01_Admin_Capabilities_7D00.pcapng` | `Read-only API` → `Refresh Admin Capabilities` 1회 | `0x7D00` |
| 2 | `02_Admin_AxisParameters_1to4_7D10.pcapng` | Axis 1~4에서 6개 parameter를 모두 읽기, 총 24회 | 매번 `0x7D00 → 0x7D10` |
| 3 | `03_Admin_GroupParameters_7D20.pcapng` | `PathVelocityLimit`, `PathAccelerationLimit`, `JerkTime`, `All` 각각 실행 | 매번 `0x7D00 → 0x7D20` |
| 4 | `04_Group_Absolute_Regression_20A4.pcapng` | 준비 완료 후 작은 거리의 기존 `Move Linear Absolute` | `0x2051 → 0x20A4 → 0x2045 반복` |
| 5 | `05_Group_Relative_Aborting_XYZU_7D22.pcapng` | `Aborting`, X/Y/Z/U 한 축씩 작은 `+delta`와 원위치 `-delta` | 매 동작 `0x2051 → 0x7D00 → 0x7D22 → 0x2045 반복` |
| 6 | `06_Group_Relative_Buffered_7D22.pcapng` | `Buffered`로 작은 상대이동과 원위치 복귀 | 위와 동일, `0x7D22` Buffer 값 `2` |
| 7 | `07_Group_Relative_StopRace_7D22_2085.pcapng` | 낮은 속도로 상대이동 버튼 직후 `Group Stop`, 3회 정도 반복 | 아래 Stop 경합 조건 확인 |
| 8 | `08_Group_Recovery_2085_2048_204B.pcapng` | Stop → InPosition 확인 → Disable → Power Off → Read Status | `0x2085 → 0x2045 반복 → 0x2048 → 0x204B → 0x2045` |

`05`에서는 한 번 클릭할 때 `0x7D22`가 정확히 하나만 나와야 합니다. X/Y/Z/U 외 나머지 distance slot은 모두 0이어야 합니다.

## Stop 경합 판정

`07`에서 정상 순서는 다음 둘 중 하나입니다.

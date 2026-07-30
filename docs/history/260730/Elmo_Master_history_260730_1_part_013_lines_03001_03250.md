>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+40 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+4 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+5 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+2 -2)
>
> </details>
>
> 코드 수정은 이제 동결 후보입니다. 현재 WPF Debug 빌드는 통과했습니다. 이어서 PC 계약 테스트 219개, negative-wire dry-run, WPF/API Release까지 같은 소스로 검증하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+43 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+3 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+1 -1)
>
> </details>
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+186 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+7 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+16 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+6 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+2 -3)
>
> </details>
>
> 최종 WPF Debug/Release 빌드는 둘 다 통과했습니다. 테스트 인계 문서도 추가했고, 이제 일반 `bin\Debug` 실행 파일을 생성한 뒤 변경 범위와 로그 문서만 마지막으로 대조하겠습니다. LASAL 소스는 이번 1차 범위에서 변경하지 않았습니다.
>
> 최종 검토에서 qualification 밖의 D5 읽기 경로도 같은 위험이 확인됐습니다. Drive Read와 수동 SDO가 응답 유실·poll timeout으로 끝나면 PLC 티켓이 남을 수 있으므로, 이 두 경로도 동일 quarantine에 편입하겠습니다. 이 수정까지 포함해야 “테스트 준비 완료”라고 말할 수 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+52 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+260 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+16 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 1차 범위를 닫는 단계입니다. 방금 공통 D5 추적에 수동 SDO와 Drive Read 경로까지 포함했으므로, 우선 컴파일과 상태 전이 누락을 확인한 뒤 문서·실행 파일까지 최종 동기화하겠습니다. LASAL 원본은 건드리지 않습니다.
>
> Debug 재빌드는 통과했습니다. 다만 공통 tracker에 연결한 뒤 수동 SDO 티켓이 Resolve 경로에서 끝나거나 세션 격리될 때, 수동 진단 UI 상태도 함께 갱신해야 하는 누락을 확인했습니다. 이 상태 동기화까지 보완한 후 Release와 실제 테스트용 Debug 출력을 다시 만들겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+66 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+8 -1)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+11 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+53 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+2 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+71 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+40 -23)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+54 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+21 -0)
>
> </details>
>
> 남은 안전 검토 결과를 반영했습니다. `TicketNotFound`는 PLC의 terminal-slot 교체 계약에 따라 “이전 티켓은 이미 terminal이었다”로만 해제하고 결과 성공은 추정하지 않으며, 로컬 stale-session은 quarantine으로 전환합니다. PI Write는 1차에서 UI와 handler 모두 명시적으로 차단했습니다. 이제 전체 빌드·223개 회귀 테스트로 닫겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+19 -0)
>
> </details>
>
> 코드정리되는대로 정리해서 커밋하고 테스트 할 수 있게 폴더 복사해놓고 다시 계소 개발 진행해.
>
> 요청대로 순서를 바꿉니다. 1차 변경을 코드/API·WPF·문서로 분리 커밋하고, 기존 사용자 작업물은 제외하겠습니다. 그다음 테스트 폴더를 안전하게 대조한 뒤 필요한 파일만 복사하고, 바로 다음 개발 항목으로 이어가겠습니다. 현재 검토에서 발견된 legacy recovery 로그 한 줄도 먼저 바로잡겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+1 -1)
>
> </details>
>
> 최종 코드 빌드는 다시 통과했습니다. 이제 사용자 쪽 LASAL 파일과 이미 staged 된 대형 history 파일은 건드리지 않고, 먼저 API/자동검증 계약을 독립 커밋하겠습니다. 이어서 WPF 구현과 문서를 별도 커밋합니다.
>
> 독립 리뷰에서 동작 안전성 문제는 없었지만, 수동 SDO/Drive 로그가 직전 qualification의 run ID에 잘못 붙는 감사 로그 문제를 찾았습니다. 커밋 직후 발견된 것이므로 WPF 커밋을 그대로 두지 않고, 외부 D5 전용 run/scenario 문맥을 추가한 뒤 마지막 WPF 커밋을 amend하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+53 -27)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+99 -0)
>
> </details>
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -0)
>
> </details>
>
> 마지막 복구 경계 하나가 더 확인됐습니다. PLC가 `HandleOrGenerationStale(10)`을 반환하면 현재 코드는 Resolve가 반복 실패할 수 있습니다. 이를 `TicketNotFound`처럼 성공/terminal로 오판하지 않고 owner-session quarantine으로 전환하고, 오해를 막기 위해 scope 이름도 `same_owner_connection_recovery`로 바로잡은 뒤 커밋을 다시 amend하겠습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+11 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+3 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 최종 정적 리뷰에서 P0/P1/P2 잔여 문제 없음으로 확인됐습니다. 지금 stale-owner 복구 수정까지 별도 커밋하고, 그 커밋을 기준점으로 LASAL 테스트 폴더를 복사·대조한 뒤 다음 구현 항목으로 바로 넘어가겠습니다.
>
> 커밋 기준점은 `3e57841`까지 확정했습니다. 이제 지정한 `C:\work\Elmo\Elmo_Master_test\Elmo_EtherCAT_Test_4Axis`가 비어 있는지 다시 확인한 후, 현재 LASAL 작업 트리를 그대로 복사하고 파일 목록·크기를 대조하겠습니다. 사용자 쪽 `Classes.lcb`/`TestClass` 변경은 커밋에서는 제외했지만 테스트 복사본에는 현재 상태 그대로 포함됩니다.
>
> 테스트 복사본은 618개 파일, 414,254,483바이트를 전 파일 SHA-256까지 대조해 원본과 일치합니다. 이 복사본은 그대로 고정해 두고, 본 저장소에서는 다음 우선순위인 “Drive facade의 submit 전 실패와 ticket/status 단계 실패를 구분하는 예외 계약”을 문서·코드 기준으로 재확인한 뒤 구현을 이어갑니다. 이 항목은 현재 안전성 결함은 아니지만 불필요한 quarantine을 줄이는 다음 UX/진단 개선입니다.
>
> 다음 변경은 LASAL 소스에는 손대지 않습니다. SDK에 `Submission`/`StatusPolling` 단계와 선택적 ticket을 가진 전용 SDO Read 예외를 추가하고, 기존 `LMCDiagnosticsCommandException` catch 호환성은 상속으로 유지하겠습니다. WPF는 이 정보로 제출 단계 거절은 guard 해제, status 단계 실패는 known ticket 보존으로 처리합니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs` (+10 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDriveModels.cs` (+91 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+23 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+30 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+117 -0)
>
> </details>
>
> 첫 빌드 시도는 코드 오류가 아니라 `msbuild.exe`가 PATH에 없어서 실행되지 않았습니다. 설치된 Build Tools 경로를 확인해 같은 검증을 다시 돌리겠습니다. 동시에 검토 결과상 capability 조회 실패와 submit 거절도 구분하는 편이 더 정확하므로, 현재 2단계 설계를 3단계로 다듬겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+51 -10)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDriveModels.cs` (+12 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+10 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+35 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+2 -2)
>
> </details>
>
> 첫 실행에서 224개 중 기존 223개는 그대로 통과했고, 새 capability-rejection 케이스만 테스트 프레임 길이가 실제 parser 계약(68바이트)과 달라 실패했습니다. 구현 실패가 아니라 새 fixture 오류이며, capability 오류 응답을 68바이트로 맞춘 뒤 다시 검증합니다. 파생 예외 형식은 기존 base catch 호환성과 fail-closed 범위를 가장 잘 보존한다는 교차 검토도 끝났습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDriveModels.cs` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+79 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Sdo.cs` (+6 -4)
>
> </details>
>
> 새 예외 계약은 capability preflight, submit, status polling을 sync/async 전부 검증하고, `ReadDriveStatusAsync`의 두 번째 내부 SDO 실패까지 포함해 Debug 224/224를 통과했습니다. 이제 Release 224/224와 WPF Debug/Release를 확인한 뒤 문서의 223 기준 및 “후속 부채” 표현을 현재 범위에 맞게 갱신하겠습니다. 비진단 transport/malformed/session 실패는 여전히 보수적으로 quarantine한다는 제한은 남겨둡니다.
>
> <details><summary>Edited 11 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+10 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+10 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/NEGATIVE_WIRE_TOOL_2026-07-27.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+8 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+76 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/NEGATIVE_WIRE_TOOL_2026-07-27.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+9 -6)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+8 -6)
> - Wrote `./docs/architecture/SIGMATEK_NEXT_RUNTIME_QUALIFICATION_AND_TEST_UI_DESIGN_2026-07-23.md` (+9 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_TCP_MOTION_INTERFACE_PERFORMANCE_FIRST_OOP_REFACTOR_DESIGN_2026-07-23.md` (+1 -1)
>
> </details>
>
> 추가 negative 계약도 반영했습니다. Submit 응답 유실·malformed 응답은 전용 예외로 오분류되지 않고 기존 unknown 경로에 남으며, capability 거절 뒤 `0x7E50`이 전송되지 않는 요청 순서도 fake RPC로 검증됩니다. 최종 자동시험은 Debug/Release 각각 225/225입니다. 이제 diff/호환성 검토 후 이 후속 개발분도 별도 커밋으로 정리합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -3)
>
> </details>
>
> 최종 diff 리뷰도 P0/P1/P2 없음입니다. 기능 변경 5개 파일과 문서 10개 파일을 목적별 두 커밋으로 분리하고, 기존에 사용자가 staging한 대형 history 및 LASAL/packet 작업물은 그대로 제외하겠습니다.
</details>

완료했습니다.

- LASAL 테스트 폴더: [Elmo_EtherCAT_Test_4Axis](C:/work/Elmo/Elmo_Master_test/Elmo_EtherCAT_Test_4Axis)
  - 618개 파일, 414,254,483바이트
  - 전체 SHA-256 재대조 결과 불일치 0건
  - 현재 사용자 변경 `Classes.lcb`, `TestClass`도 포함
- [WPF Debug 실행 파일](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/bin/Debug/LasalMotionControlApiExample.exe)
- [Phase 1 테스트 런북](./LMC_Library/LasalApiWpfTestApp/PHASE1_TEST_RUNBOOK_2026-07-27.md)

정리된 커밋은 다음과 같습니다.

- `be3a929` API transport/qualification 강화
- `3d1d7f7` WPF Phase 1 qualification
- `473bd66` Phase 1 테스트 런북
- `3e57841` D5 stale-owner 복구
- `5738582` SDO Read 단계·ticket 예외 계약
- `b774d30` 관련 문서 및 225/225 기준 갱신

검증 결과:

- API Debug/Release: 각각 `225/225 PASS`
- WPF Debug/Release Rebuild: PASS
- Negative-wire: dry-run PASS, 실제 PLC 송신 없음
- 최종 리뷰: P0/P1/P2 없음
- LASAL IDE 빌드·PLC 다운로드·실축·pcap은 아직 사용자 실기 검증 범위

사용자 작업인 history staging, `Classes.lcb`, `TestClass`, packet 자료는 커밋에서 제외했습니다. 전체 cached check는 기존 staging된 history 파일의 trailing whitespace 때문에 실패하지만, 이번 커밋 대상별 check는 모두 통과했습니다.

다음 개발 순서는 non-domain transport/session 실패의 전체 attempt context와 WPF tracker 상태 전용 단위시험입니다.

<details><summary>122 previous messages</summary>

> 다음 우선순위로 이어갑니다. 먼저 현재 HEAD·사용자 변경을 다시 분리 확인하고, 남아 있는 `transport/malformed/session` 실패의 실행 단계·ticket 보존 계약과 WPF tracker 단위시험 범위를 현재 소스/설계 문서에서 확정한 뒤 구현하겠습니다. LASAL 프로젝트는 수정하지 않습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 남은 핵심은 “실패가 어느 단계에서 났는지”를 모든 예외 경로에서 잃지 않도록 만드는 일입니다. 기존 예외 형식은 깨지 않으면서, WPF가 안전하게 티켓 보존·가드 해제를 판단할 수 있는 구조로 바로 구현하겠습니다.

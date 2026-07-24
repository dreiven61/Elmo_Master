
검증은 C# Debug/Release 각각 104/104, LASAL SourceOnly/full 정적 계약, WPF Debug/Release 빌드가 모두 PASS했습니다. 이번에는 기존 미커밋 파일을 건드리지 않고 설계 문서 1개만 추가했습니다.

워크북은 원본을 수정하지 않고 OOXML로 읽었습니다. 숨김 행과 수식은 없었지만, 현재 workbook renderer를 사용할 수 없어 색상·조건부 서식은 판정 근거로 사용하지 않았습니다.

> 단계별 구현순서에 따라서 구현을 진행해보자.

<details><summary>18 previous messages</summary>

> 설계 문서의 Phase 0부터 구현하겠습니다. 범위는 ① Group motion 옵션의 PC-side fail-fast, ② `0x2051`을 None/ACS 전용으로 고정하고 MCS/PCS를 명시적으로 거부, ③ `0x2085` GroupStop에서 LASAL 반환값을 응답에 보존하는 세 항목입니다. 기존 SDO 관련 미커밋 변경과 겹치는 파일이 있으므로 먼저 해당 구간의 diff를 확인한 뒤 최소 범위로 수정하고, C# 104개 회귀·LASAL 정적 계약·WPF 빌드까지 다시 돌리겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Phase 0의 실제 PLC 계약을 먼저 확정하고 있습니다. 특히 `GroupStop` 반환값이 오류 코드인지 명령 번호인지, `0x2051` 좌표계 값이 실제로 무엇을 허용하는지를 소스 근거로 확인한 뒤 바로 수정하겠습니다.
>
> 중요한 설계 오류를 하나 확인했습니다. LASAL `StopMove()` 반환값은 오류 코드가 아니라 `StopCmdNo`(정지가 끝나는 명령/버퍼 인덱스)입니다. 따라서 0/비0을 성공·실패로 매핑하면 오히려 정상 동작을 오류로 만들 수 있어, 이 항목은 설계 문구부터 바로잡고 실제 오류 확인 경로(`GroupReadStatus/ProfileError`)와 분리하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+121 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+158 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+13 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcIntegrationTests.cs` (+30 -6)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+5 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+20 -18)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+5 -2)
>
> </details>
>
> 현재 코드 수정은 다음처럼 반영됐습니다. `0x20A4`는 PC에서 실제 4축/옵션/동역학 제약을 선검증하고, `0x2051`은 PC와 PLC 모두 None/ACS만 허용하며 MCS/PCS는 PLC에서 `-7`로 거부합니다. 이제 테스트와 문서 계약을 같은 기준으로 맞추고 있습니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+19 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+12 -6)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+16 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+15 -10)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+21 -11)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+17 -14)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -7)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -13)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -8)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+7 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+9 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+10 -9)
>
> </details>
>
> 단위·fake-RPC 검증은 Debug에서 새 matrix 포함 `105/105 PASS`, LASAL SourceOnly 정적 계약도 PASS했습니다. 이제 Release, full network 계약, WPF Debug/Release를 병렬로 확인하고 문서의 검증 수치를 확정하겠습니다.
>
> WPF Debug 검증은 코드 오류가 아니라 현재 실행 중인 `LASAL Motion Control API Example`과 Visual Studio가 기존 `bin\Debug` DLL/PDB를 잠가 실패했습니다. 실행 중인 프로세스는 건드리지 않고 별도 임시 출력 폴더로 다시 빌드하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Phase 0 구현은 코드 기준으로 거의 끝났습니다. 남은 문서의 오래된 `104/104` 표기와 현재 상태를 정리한 뒤, 최종 Debug/Release·정적 계약·diff 검증까지 다시 묶어서 확인하겠습니다.
>
> 문서 정합성에서 두 가지를 바로잡겠습니다. 테스트 총수는 이제 `105/105`이고, `0x2051`의 None/ACS는 소스·가짜 RPC 기준 PASS이지 실 PLC 좌표 동등성이 확인된 것은 아닙니다. 이 경계를 문서 전체에 동일하게 반영하겠습니다.
>
> <details><summary>Edited 18 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+15 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+12 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+26 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponseParserTests.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+8 -8)
> - Wrote `./LMC_Library/LMC_API/README.md` (+2 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+9 -9)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+19 -15)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+18 -18)
>
> </details>
>
> 재검토에서 소스 결함은 없었지만 정적 계약의 방어력이 약한 부분이 확인됐습니다. `StopCmdNo`가 나중에 다시 오류 코드로 쓰이지 못하게 하고, `0x2051`의 기본 `-3`·zero tail·GroupStop `(0,0)` 허용 경계를 테스트로 고정하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RequestGoldenTests.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -4)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+5 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+7 -7)
>
> </details>
>
> 이제 최종 소스 기준 검증을 다시 돌립니다. C# Debug/Release와 LASAL SourceOnly/full 정적 계약을 병렬로 확인하고, 실패가 있으면 해당 계약부터 바로 수정하겠습니다.
>
> 핵심 검증은 모두 통과했습니다: C# Debug/Release 각각 105/105, LASAL SourceOnly/full 계약 PASS입니다. 이제 Markdown과 함께 배포되는 standalone HTML을 재생성하고, 최종 WPF build와 diff 검사를 마무리하겠습니다.
>
> WPF 재검증 첫 시도는 코드 문제가 아니라 PowerShell 환경변수를 단일 인용부호로 넘겨 경로가 문자 그대로 해석된 오류였습니다. 실제 절대 임시 경로로 바로 다시 실행합니다.
</details>

Phase 0 소스 구현을 완료했습니다.

- C#: Group 옵션·수치·4축 topology를 RPC 전 fail-fast 처리했습니다. `0x2051`은 None/ACS만 허용합니다. [LmcProtocol.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:419)
- LASAL: None/ACS는 member-slot alias, MCS/PCS는 `-7`, 미정 enum은 `-3`, slot 10~16은 0으로 고정했습니다. [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2280)
- GroupStop의 `StopCmdNo`를 오류로 해석하지 않으며, 완료·오류는 `GroupReadStatus(0x2045)`로 확인하도록 고정했습니다. [TCPMotionInterface.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2150)
- 테스트와 정적 계약을 보강하고 관련 Markdown/HTML 문서를 동기화했습니다. [Phase 0 결과](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:289)

검증 결과:

- C# Debug/Release: 각각 `105/105 PASS`
- LASAL SourceOnly/full network 계약: PASS
- WPF Debug/Release 임시 출력 경로 build: 경고·오류 0
- `git diff --check`, `git diff --cached --check`: PASS
- LASAL IDE build/download 및 실 PLC 동작은 아직 검증하지 않았습니다.

다음 순서는 실제 PLC에서 None/ACS read, MCS/PCS `-7`, GroupStop 후 `0x2045` 상태 poll을 확인한 뒤 Phase 1 read-only admin/compatibility facade 구현입니다. 기존 미커밋 SDO 작업은 그대로 보존했으며 이번 변경도 아직 커밋하지 않았습니다.

> 계속 구현 이어서 진행해보자

<details><summary>24 previous messages</summary>

> Phase 1로 이어가겠습니다. 우선 read-only 기능부터 진행하되, 기존 D1/D2/D5 wire를 재사용하고 새 PLC command를 불필요하게 만들지 않는 범위로 구현하겠습니다. 현재 미커밋 SDO·Phase 0 변경은 건드리지 않고 확장 지점을 먼저 확정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Phase 1 구현을 계속 진행 중입니다. 지금은 에이전트 감사 결과를 반영해 공개 API와 PLC 명령 계약을 먼저 고정하고 있으며, 특히 오류 번호 충돌을 피하도록 오류 카탈로그에 도메인을 포함하고 그룹 파라미터는 기존 명령 재사용이 아닌 `0x7D20` 읽기 전용 명령으로 분리하겠습니다.
>
> Admin 확장 계약은 `0x7D00/0x7D10/0x7D20`으로 확정해 구현하겠습니다. 축 파라미터 키는 native enum 번호를 노출하지 않고, `InPositionWindow`라는 모호한 이름은 실제 LASAL 의미에 맞게 `EndPositionToleranceWindow`로 명확히 하겠습니다. 그룹 읽기는 검증된 3개 값만 selection mask로 허용합니다.
>
> <details><summary>Created 4 files, edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminModels.cs` (+286 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs` (+422 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdmin.cs` (+333 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcConnection.cs` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+12 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+4 -0)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+304 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminContractTests.cs` (+439 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+47 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdmin.cs` (+21 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+53 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 1단계 읽기 전용 API 구현을 이어서 마무리하겠습니다. 먼저 병렬로 진행된 D5 상태/운전모드 구현과 관리자 명령 검토 결과를 회수한 뒤, 전체 diff·테스트·설계 문서를 한 번에 맞추겠습니다. LASAL IDE/PLC 실기 검증은 코드 완료와 분리해서 명시하겠습니다.
>
> 현재까지 합쳐진 구현은 기존 wire를 재사용하는 부분과 새 LASAL-local 관리자 명령을 분리한 상태입니다. 특히 운전모드/상태 읽기는 D5 티켓을 순차 사용하며, PC 취소가 이미 제출된 PLC 티켓을 자동 취소하지 않도록 경계를 유지한 것을 확인했습니다. 이제 전체 회귀를 돌려 실제 합산 상태를 확인하겠습니다.
>
> 독립 검토에서 실제 수정이 필요한 세션 경계 결함 1건이 확인됐습니다. 오래된 axis/group 객체를 검사한 직후 재연결되면 숫자 overload가 새 세션을 다시 잡을 수 있는 경쟁 조건입니다. handle이 생성될 때의 session generation을 capability 조회부터 실제 read까지 고정하도록 바로 수정하고 회귀 테스트를 추가하겠습니다.
>
> 세션 경쟁 조건과 Admin 오류 카탈로그까지 정리됐습니다. 최종 합산은 Debug/Release 각각 134/134입니다. 이제 문서의 `AdminDetail` 네 번째 도메인과 최종 시험 수를 맞춘 뒤, 전체 정적 계약·WPF 빌드·HTML 재생성까지 최종 검증하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+5 -5)
>
> </details>
>
> Phase 1의 라이브 검증을 바로 할 수 있게 할지도 점검했습니다. 현재 WPF 예제에는 새 Admin/typed drive API를 호출하는 화면이 아직 없습니다. 라이브 PLC 게이트를 막지 않도록 기존 화면 구조를 유지한 별도 read-only 테스트 탭까지 추가하겠습니다. PI/Bulk는 기존 탭이 이미 동등한 조작 경로를 제공하므로 중복 UI는 만들지 않겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Phase 1의 마지막 작업인 WPF 읽기 전용 실기 테스트 화면부터 마무리하겠습니다. 서브 작업 결과를 검토한 뒤 Debug/Release 빌드·기동 스모크 테스트와 전체 정적 검증을 다시 돌리고, 문서까지 현재 상태에 맞추겠습니다.
>
> 화면 구조는 이미 반영되고 있습니다. 기존 EtherCAT/PI·Recorder 기능을 중복하지 않고, 새 API만 별도 읽기 전용 탭으로 분리한 상태이며 현재 이벤트 연결과 빌드 검증을 끝내는 중입니다.
>
> WPF 탭은 빌드만 되는 수준이 아니라, Admin capability를 먼저 확인해야 후속 읽기가 열리는 fail-closed 흐름으로 연결됐습니다. 이제 독립 리뷰를 병행하면서 사용자 가이드와 설계 문서를 실제 화면 기준으로 맞추고, 제가 전체 검증을 다시 수행하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+26 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+14 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+5 -0)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+3 -2)
>
> </details>
>
> PC 라이브러리 Debug 자동 시험은 134/134 전부 통과했습니다. Admin reconnect 경쟁, typed SDO polling/cancel, PI/Bulk session·revision 검증까지 포함된 수치이며, 이제 Release와 LASAL 정적 계약을 같은 기준으로 재확인합니다.
>
> Release 134/134와 LASAL source-only/full 정적 계약도 모두 통과했습니다. 남은 검증은 새 탭이 포함된 WPF를 기존 실행 파일과 충돌하지 않는 별도 임시 출력 폴더에 빌드하고, Debug/Release 창이 실제로 초기화되는지 확인하는 것입니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -2)
>
> </details>
>
> 독립 리뷰에서 실제 blocker 1건을 찾았습니다. 제출 이후 SDO status 조회 중 취소 토큰이 transport까지 전달되어 연결 자체를 Faulted로 만들 수 있었고, 이는 “PC 대기만 취소하고 PLC ticket은 계속 조회 가능”이라는 계약과 충돌합니다. 이 경로를 수정하고, status RPC 도중 취소 회귀 시험을 추가한 뒤 전체 검증을 다시 돌리겠습니다.
>
> <details><summary>Edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DriveReadFacadeTests.cs` (+74 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+0 -2)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.html` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+1 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+3 -3)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+3 -1)
> - Wrote `./LMC_Library/LMC_API/API_USER_MANUAL_KO.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+3 -1)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
>
> </details>
>

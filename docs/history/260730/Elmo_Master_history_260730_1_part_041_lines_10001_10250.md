- SDO Write 중에도 Slave/Index/SubIndex/Type/Length/Data를 계속 편집할 수 있습니다. 전송 요청은 클릭 시점 값으로 복사되므로 이후 편집이 진행 중 요청을 변경하지 않습니다. 추가 Submit만 직렬화됩니다. [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:2248)
- 최신 WPF는 접속 후 커플러·DI·DO를 포함한 CREVIS 3개 행을 표시합니다. 기존 GUI 바이너리는 자동 갱신되지 않으므로 WPF를 Rebuild/재시작해야 확인됩니다.
- CREVIS 실시간 Health/DI/DO는 아직 올라올 수 없습니다. 하드웨어 객체는 있지만 `LMCEcatInputLatch`의 `Coupler/InputSlot/OutputSlot` Client와 Motion Network 연결이 없습니다. 현재 값은 고정 토폴로지 정보뿐입니다.
- 다음에는 [LASAL IDE 구조 인계 문서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:18)의 구조만 IDE에서 생성·저장해 주세요. 이후 제가 외부 편집으로 snapshot과 `0x7E13/0x7E22`를 구현하겠습니다. `0x7E23` 출력 제어는 실제 Read 검증 전까지 닫아둡니다.

검증 결과:

- SDK Debug/Release: `691/691 PASS`
- WPF Debug/Release smoke: `66/66 PASS`
- LASAL 정적 계약: PASS
- PLC 실시간 CREVIS 검증: 미수행
- `git diff --check`: PASS
- cached 검사는 기존 staged [Elmo_Master_history_260721.md](./docs/history/Elmo_Master_history_260721.md:2508)의 후행 공백 때문에 실패하며 이번에는 건드리지 않았습니다.
- 커밋이나 stage는 하지 않았습니다.

<details><summary>37 previous messages</summary>

> CREVIS 실시간 경로는 IDE 구조 대기 상태로 유지하고, 그동안 멈추지 않고 나머지 구현을 진행하겠습니다. 우선 현재 소스와 남은 설계 항목을 다시 대조해 다음 안전한 기능 묶음을 확정한 뒤 바로 코드와 테스트까지 반영하겠습니다. LASAL IDE는 제어하지 않습니다.
>
> 감사 결과, CREVIS 출력 API인 `0x7E23`에도 같은 지연 ACK 공백이 하나 확인됐습니다. 이 경로는 기존 accepted-ticket 실패 컨텍스트가 이미 있으므로 먼저 작게 닫고, 이어서 Recorder Configure/Start/Adopt의 반환 리소스 보존까지 진행하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+6 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+183 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+39 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+34 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+19 -0)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTopologyIo.cs` (+30 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+12 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+179 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+40 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+28 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> CREVIS 조회/출력 접수 결과 폐기 처리는 우선 반영됐고 현재 SDK 정적 테스트는 697/697 통과 상태입니다. 이제 Recorder의 “서버가 자원을 만든 뒤 우선 명령 때문에 응답 적용이 취소되는 경우”에도 핸들·Identity를 잃지 않도록 SDK와 GUI 복구 경로를 이어서 구현하겠습니다. LASAL IDE는 건드리지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+166 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+60 -0)
>
> </details>
>
> Recorder 쪽은 단순히 예외를 던지는 것으로 끝내면 PLC에 생성된 자원이 고아가 됩니다. 그래서 응답에서 생성된 정확한 handle/identity/lease를 예외에 붙이고, 해당 객체는 정상 운전에는 못 쓰고 Status·Stop·Release 같은 복구 동작에만 쓰이도록 모델 계층부터 제한하고 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+51 -14)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+42 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+65 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+24 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+12 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorderModels.cs` (+54 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+30 -17)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+72 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+371 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+303 -0)
>
> </details>
>
> SDK 모델과 6개 Recorder 자원 생성 경로를 연결했고, 이제 6경로 × 동기/비동기 12개 경쟁 테스트를 추가 중입니다. 각 테스트는 “응답은 PLC에서 정상 접수됐지만 GroupStop이 먼저 예약됨”을 재현하고, 복구 전용 객체가 남는지와 실제 Release까지 같은 세션에서 가능한지를 확인합니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/RpcSendPriorityCoordinatorTests.cs` (+197 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsRecorder.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+59 -4)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+16 -8)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+87 -26)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.Recorder.cs` (+18 -5)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+50 -20)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+36 -5)
>
> </details>
>
> WPF 솔루션을 `dotnet build`로 직접 빌드하면 이 구형 .NET Framework WPF 프로젝트의 XAML 생성 단계가 빠져 기존과 동일하게 수백 개의 컨트롤 미정의 오류가 납니다. 이건 방금 코드 오류 판정에 쓸 수 없는 빌드 경로라서, 저장소의 WPF smoke-test 경로와 Visual Studio MSBuild 경로로 다시 검증하겠습니다.
>
> Visual Studio MSBuild의 C# 컴파일 자체는 오류 없이 끝났습니다. 최종 실패 원인은 사용자가 실행 중인 `LASAL Motion Control API Example`과 Visual Studio가 Debug DLL/PDB를 잠근 상태라 복사 단계가 막힌 것입니다. 실행 중인 테스트를 건드리지 않고 Release 출력으로 별도 검증하겠습니다.
>
> <details><summary>Edited 6 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+3 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+8 -6)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+8 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 현재 체크포인트부터 다시 확인하겠습니다. 우선 이번에 반영한 Recorder/CREVIS/SDO 관련 변경만 좁혀서 diff와 문서 정합성을 점검하고, 사용 중인 VS 디버그 세션은 건드리지 않은 채 Release 빌드·정적 테스트 결과를 기준으로 마무리하겠습니다.
>
> 작업 트리가 매우 큰 상태라 이번 변경 범위를 분리해 보겠습니다. 한쪽은 CREVIS 표시·SDO 편집 가능 상태를 현재 소스에서 재검증하고, 다른 쪽은 Recorder 문서/코드 누락만 읽기 전용으로 점검하게 하겠습니다.
>
> 문서 숫자는 전부 `709/709`, WPF `66/66`으로 맞아 있고, 작업 트리 자체의 `git diff --check`도 통과했습니다. staged 검사 실패는 이번 코드가 아니라 이미 stage된 대형 히스토리 파일의 기존 trailing whitespace입니다. 이제 현재 Release 산출물로 SDK와 WPF smoke를 한 번 더 재현하겠습니다.
>
> 문서 감사에서 기능 누락은 없고 설명 경계만 네 군데 발견됐습니다. 특히 Start만 source configuration을 함께 격리한다는 점과, WPF 보존 callback 자체는 아직 전용 smoke가 없다는 점을 과장 없이 문서에 명시하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+7 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+5 -4)
>
> </details>
>
> 재검증에서 한 가지 실제 UX 구멍을 찾았습니다. RPC가 진행 중일 때의 입력 잠금은 이미 풀렸지만, SDO Write 직전의 modal 확인창이 WPF 전체를 막고 있었습니다. 이 확인창을 제거하고, 첫 클릭은 immutable 요청을 화면에 고정해 확인 대기 상태로 만들고 두 번째 클릭만 송신하도록 바꾸겠습니다. 확인 대기 중에도 다른 입력은 계속 편집할 수 있게 하겠습니다.
>
> <details><summary>Created a file, edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/LasalApiWpfTestApp.csproj` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoWriteConfirmationState.cs` (+153 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+23 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+4 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+9 -17)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+87 -0)
>
> </details>
>
> Recorder 리뷰에서도 보존 callback 내부 예외가 원래 선점 예외를 덮을 수 있는 경로가 확인됐습니다. callback을 실제로 no-throw로 만들고, 혹시 남는 callback 오류도 원래 예외에 진단 정보만 붙이도록 정리하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+9 -11)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.cs` (+15 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.RecorderDouble.cs` (+12 -3)
>
> </details>
>
> 빌드에서 같은 modal 확인 함수가 SDO same-value qualification에도 남아 있던 것을 잡았습니다. 이 경로는 이미 실행 전 4개 운영자 확인 체크를 강제하므로, 중간 modal은 제거하고 immutable Write snapshot을 로그/진행 상태에 남기는 방식으로 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Qualification.SdoWrite.cs` (+12 -2)
>
> </details>
>
> 새 smoke가 추가되어 WPF 검증 수는 `67/67`이 됐습니다. 관련 문서의 기존 `66/66` 표기와 “경고 확인창” 설명을 실제 동작인 비모달 2단계 확인으로 동기화하겠습니다.
>
> 현재 VS Debug 산출물은 사용자 디버그 세션이 잡고 있으므로 그 프로세스는 건드리지 않겠습니다. 같은 Debug 구성을 임시 출력 폴더로 빌드·실행해 `67/67`을 확인하겠습니다.
>
> <details><summary>Edited 8 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+6 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+7 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+4 -2)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+8 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> SDO Write 쪽은 모달 확인창을 없애고 2단계 Arm/Submit 방식으로 바꿨습니다. 지금 마지막 리뷰에서 “값을 편집한 뒤 버튼 문구가 여전히 Submit으로 보이는” UI 불일치가 하나 확인돼, 편집 즉시 재확인 상태로 돌아가도록 고친 뒤 전체 빌드·스모크 테스트까지 다시 돌리겠습니다. CREVIS는 정적 구성 표시와 실제 PLC 실시간 데이터 경로를 분리해서 최종 상태를 확인하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+64 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs` (+32 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+28 -6)
>
> </details>
>
> 편집 후 버튼 문구 문제까지 반영했습니다. Slave/Index/Sub-index/자료형/길이/Timeout/Write 값 중 하나라도 바뀌면 기존 Arm 스냅샷을 즉시 폐기하고 버튼이 `Arm SDO Write`로 돌아갑니다. 첫 클릭이 송신하지 않는지와 모든 입력 필드의 재-Arm 동작을 같은 WPF 스모크 테스트에 포함했습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfMainWindowIntegrationTests.cs` (+12 -1)
>
> </details>
>
> Release WPF 빌드와 스모크 테스트는 `67/67 PASS`입니다. 테스트 중 격리용 journal 창을 표시하지 않아 lock 정리가 늦어진 테스트 코드 문제 1건이 먼저 드러났고, 실제 기능 결함은 아니었으며 테스트 창 수명주기를 고친 뒤 재실행해 통과했습니다. 현재 실행 중인 Visual Studio Debug 출력은 건드리지 않고 별도 임시 출력 폴더로 Debug 검증을 한 번 더 하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+3 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
>
> </details>
</details>

반영 완료했습니다.

- SDO Write 모달 확인창 제거.
- 첫 클릭은 송신 없이 immutable 요청만 Arm.
- Slave/Index/Sub-index/자료형/길이/Timeout/Write 값 중 하나라도 편집하면 기존 Arm을 즉시 폐기하고 버튼이 `Arm SDO Write`로 복귀.
- 동일 요청의 두 번째 클릭만 안전조건을 다시 검사한 후 송신.
- 일반 RPC 또는 exact-readback 대기 중에도 편집은 가능하며, 추가 송신만 직렬화됩니다. 구현은 [MainWindow.Diagnostics.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.Diagnostics.cs:1216), [SdoWriteConfirmationState.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/SdoWriteConfirmationState.cs:12)에 있습니다.

CREVIS는 현재 상태이므로 구분해야 합니다.

- 구성 정보: LASAL `0x7E11/0x7E12`에 커플러·입력 슬롯·출력 슬롯을 포함한 7개 노드가 반영돼 있습니다. [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:388)
- WPF: 연결 직후 자동으로 구성 정보를 읽으며 `Configured CREVIS entries=3`을 표시합니다.
- 실시간 상태·DI·DO: 아직 미완성입니다. PLC의 bit 15~17과 `0x7E13/0x7E22/0x7E23` 실제 데이터 소스는 LASAL IDE 구조 생성 전이라 켜지 않았습니다.

사용자 측 확인 순서는 다음입니다.

1. 현재 LASAL 프로젝트를 빌드·다운로드합니다.
2. WPF를 다시 빌드하고 완전히 재시작합니다.
3. 창 제목에 `[CREVIS topology / editable SDO draft]`가 있는지 확인합니다.
4. 연결 후 `Configured CREVIS entries=3`을 확인합니다.
5. 안 나오면 `0x7E00`, `0x7E11`, `0x7E12` 관련 Execution Log를 전달해 주세요.

실시간 CREVIS 구현을 이어가려면 [LASAL IDE 구조 작업 인계서](./docs/architecture/LMC_ETHERCAT_T2_IDE_STRUCTURE_HANDOFF_2026-07-28.md:18)의 class client/method와 Network 연결만 LASAL IDE에서 생성해야 합니다. 그 이후 구현부는 외부 편집기로 계속 작성하겠습니다.

검증 결과:

- WPF Debug/Release: `67/67 PASS`
- SDK Debug/Release: `709/709 PASS`
- `git diff --check`: PASS
- 실제 PLC CREVIS/SDO Write 동작 시험: 아직 미수행
- 커밋·테스트 폴더 복사: 이번에는 하지 않음
- staged check는 기존 `docs/history/Elmo_Master_history_260721.md`의 trailing whitespace 때문에 실패하며 이번 변경과 무관합니다.

<details><summary>17 previous messages</summary>


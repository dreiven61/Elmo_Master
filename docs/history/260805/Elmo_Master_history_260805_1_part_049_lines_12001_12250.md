> </details>
>
> C# SDK 쪽은 현재 정리가 끝났습니다. `LMC_Home`, DS402 method 37, TW19/TW20 전용 경로를 포함한 전체 1,069개 테스트가 모두 통과했습니다. 특히 TW19/TW20이 구형 일반 SDO `0x7E50`이나 `0x20FC`로 빠지는 경로는 제거했고, 해당 주소들은 일반 SDO에서 송신 전 차단되는 테스트까지 추가했습니다.
>
> 다음 핵심은 방금 요청드린 LASAL 함수 타입 수정과 나머지 클래스 선언/연결 저장입니다. 저장 확인 즉시 PLC 구현부를 이어 넣겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+0 -7)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+2 -17)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+0 -10)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+6 -17)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+1 -4)
>
> </details>
>
> 추가로 한 항목이 필요합니다. `SubmitAxisZeroHome`에 아래 입력을 하나 더 추가해 주세요.
>
> - `ExpectedActualPosition : DINT`
>
> 최종 시그니처는 다음입니다.
>
> ```text
> SubmitAxisZeroHome(
>     OperationToken : UDINT,
>     AxisReference : DINT,
>     ExpectedActualPosition : DINT
> ) -> Result : DINT
> ```
>
> 이 값이 없으면 TCP에서 확인한 위치와 RT에서 실제 `SetPosition()`을 실행하는 순간 사이의 위치 변화를 차단할 수 없습니다. 앞서 요청한 `CopyAxisZeroHomeResult`의 `UDINT / ^void / UDINT` 수정도 함께 적용해 Save All만 해 주세요.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+7 -31)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+4 -4)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+34 -7)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 프로젝트 외부 명칭과 공개 API는 전부 `MMC`가 아니라 `LMC`로 통일하겠습니다. 지금 진행 중인 코드·문서·WPF 변경에서도 `MMC`, `Mmc` 잔존을 검색해 제거하고, LASAL IDE에서 사용자께 부탁드릴 이름도 `LMC` 기준으로만 정리하겠습니다.
>
> 검색 결과의 `MMC` 대부분은 원본 Elmo/MMCLib API명, 캡처 파일명, 레거시 PMAS 샘플이라 일괄 변경 대상이 아닙니다. 이번에 추가하는 공개 SDK·WPF·LASAL 경로만 별도로 좁혀서 검사하고 있으며, 새 명령 이름에는 `LMC`만 허용하겠습니다.
>
> 지금 LASAL IDE에서는 명령/클래스 이름을 바꿀 필요는 없고, 아래 선언 타입만 고쳐서 `Save All` 해주세요. 이름은 모두 `LMC` 기준입니다.
>
> ```text
> SubmitAxisZeroHome(
>     OperationToken : UDINT,
>     AxisReference : DINT,
>     ExpectedActualPosition : DINT
> ) -> Result : DINT
>
> CopyAxisZeroHomeResult(
>     OperationToken : UDINT,
>     pDest : ^void,
>     DestSize : UDINT
> ) -> Result : DINT
> ```
>
> 아직 F9 빌드는 하지 마세요. 저장되면 제가 구현부를 붙인 뒤 빌드를 부탁드리겠습니다.
>
> SDK 공개 surface는 이미 `LMC_Home`, `ReadLMC_HomeOutcome`, `RetireLMC_HomeOutcome`으로 되어 있습니다. 이 정정이 다시 퇴행하지 않도록 `MMC_Home`/`MmcHome` 공개 메서드가 생기면 테스트가 실패하도록 계약 검사를 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminLmcHomeContractTests.cs` (+4 -0)
>
> </details>
>
> `TW19/TW20`은 잘못된 엔코더에 쓰면 안 되므로 축별 설정값은 별도로 필요합니다. LASAL 선언 작업과 함께 아래 4줄도 알려주세요.
>
> ```text
> Axis1: encoder family / feedback socket(1..4)
> Axis2: encoder family / feedback socket(1..4)
> Axis3: encoder family / feedback socket(1..4)
> Axis4: encoder family / feedback socket(1..4)
> ```
>
> 모르면 Elmo Application Studio의 Feedback 화면 캡처를 주셔도 됩니다. 확인 전에는 구현은 넣되 TW19/TW20 실행 gate만 `FALSE`로 유지합니다.
>
> 명칭 회귀 방지 테스트를 추가한 뒤 SDK 전체 테스트를 다시 돌렸고 `1069/1069 PASS`입니다. WPF도 현재 `LMC_Home`과 전용 TW19/TW20 경로로 빌드·집중 테스트를 통과했으며, 에이전트가 마지막 `MMC/Mmc` 잔존 검사를 진행 중입니다. 이는 PC 정적 증명이고 PLC 실행 증명은 아닙니다.
>
> DS402 Home 쪽에서 입력 latch cycle이 멈추면 기존 timeout도 같이 멈추는 결함을 확인했습니다. 이 부분은 IDE 선언과 무관하게, 서비스의 절대시간을 기존 상태 배열에 보존하도록 수정해 timeout과 cleanup이 계속 진행되게 하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+13 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+2 -2)
>
> </details>
>
> WPF 변경은 완료됐습니다. `MMC/Mmc` 잔존 0건이고, 화면 표기는 `LMC Home`으로 통일됐습니다. DS402는 method 37·offset 0·모든 이동 파라미터 0, TW19/TW20은 전용 `0x7E53→0x7E54→0x7E55` 수명주기만 사용합니다. Release 빌드와 집중 smoke test도 모두 통과했습니다.
>
> 정적 검토에서 실제 계약 불일치를 하나 더 찾았습니다. PLC가 LMC Home 시작 전 `InvalidState`, 축 오류, 좌표 precondition, Build/Boot/Map mismatch를 정상적으로 거부해도 SDK가 이를 “깨진 패킷”으로 오판하는 상태였습니다. 이 값들은 실행 전 확정 거부로 허용하되, `NativeCommandRejected`는 계속 금지하도록 수정합니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs` (+34 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminDs402HomeOutcomeRetirementContractTests.cs` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminLmcHomeContractTests.cs` (+9 -0)
>
> </details>
>
> 제가 WPF를 `dotnet msbuild`로 재검증했는데 이 프로젝트는 그 경로에서 WPF/XAML target이 로드되지 않아 `InitializeComponent` 대량 오류가 났습니다. 코드 오류로 단정하지 않고, 에이전트가 통과시킨 Visual Studio Build Tools의 실제 MSBuild 명령으로 다시 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 제가 `MMC Home`이라고 부른 건 잘못입니다. 이 프로젝트의 신규 명령·API·UI·문서는 전부 `LMC Home`으로 통일하겠습니다. 공식 Elmo 라이브러리명이나 과거 캡처 원문처럼 바꿀 수 없는 근거 자료에만 `MMC/MMCLib` 표기가 남을 수 있습니다. 현재 변경분에서 잘못 남은 이름을 다시 전수 확인하겠습니다.
>
> 지금 전수 검색 결과, 이번 Home 생산 코드·WPF·인계 문서에는 `MMC/Mmc`가 0건입니다. 테스트에만 “그 이름의 공개 API가 존재하지 않아야 한다”는 음성(negative) 검증 4건이 남아 있으며, 이것은 재유입 방지용입니다.
>
> 정식 VS MSBuild로 WPF 솔루션은 경고·오류 없이 빌드됐습니다. 전체 SmokeTests 실행은 현재 출력 없이 오래 도는 항목이 있어, 완료 여부를 조금 더 확인한 뒤 필요하면 이번 기능 관련 테스트만 분리해 실행하겠습니다.
>
> 이름 수정 외에 WPF 복구 저널에서 경계 조건 2건도 발견했습니다. 과거 형식의 완료 레코드를 v4로 잘못 재저장하는 문제와 correlation ID 0 허용 문제이며, 지금 회귀 테스트와 함께 막고 있습니다. Home 의미 자체는 계속 “축을 움직이지 않고 현재 좌표만 0으로 설정”입니다.
>
> 현재 남은 PLC 쪽 핵심 차단점은 이름 문제가 아니라 LASAL 선언입니다. `LMCEcatInputLatch`의 두 함수가 아직 `DINT`로 저장되어 있고 `ExpectedActualPosition`도 빠져 있어, 이 상태에서는 구현 본문을 연결하면 타입 계약이 깨집니다. IDE에서 선언 저장이 끝나는 즉시 외부 편집으로 본문을 이어 붙이겠습니다.
>
> 사용자께 부탁드릴 IDE 작업은 이것뿐입니다. `LMCEcatInputLatch`에서 아래처럼 고치고 `Save All`만 해주세요. 아직 F9/다운로드는 하지 마세요.
>
> ```text
> SubmitAxisZeroHome
>   OperationToken : UDINT
>   AxisReference : DINT
>   ExpectedActualPosition : DINT
>   Result : DINT
>
> CopyAxisZeroHomeResult
>   OperationToken : UDINT
>   pDest : ^void
>   DestSize : UDINT
>   Result : DINT
> ```
>
> 저장했다고 알려주시면 파일 갱신을 확인하고 구현을 계속하겠습니다.
>
> 현재까지 확인된 명명 결과는 확실합니다. 신규 공개 경로는 `LMC_Home`이며, `MMC_Home/MmcHome`은 공개 API에 존재하지 않는다는 회귀 테스트까지 포함했습니다. 이제 마지막 정적 검사만 하고, 사용자 IDE 저장이 필요한 지점을 분리해서 정리하겠습니다.
</details>

맞습니다. `MMC_Home`은 사용하지 않고 전부 `LMC_Home`으로 통일했습니다.

확인 결과:

- 신규 생산 코드·WPF·현재 계약 문서의 `MMC/Mmc`: 0건
- SDK 전체 테스트: 1069/1069 PASS
- WPF Release 빌드: 오류 0
- WPF Maintenance 테스트: 23/23 PASS
- 벤더 공식 명칭 `MMCLib`과 구 이력 문서만 원문 보존

현재 작업을 계속하려면 [LMCEcatInputLatch.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st:136)의 IDE 선언을 아래처럼 수정하고 `Save All` 해주세요. F9/다운로드는 아직 하지 마세요.

```text
SubmitAxisZeroHome
  OperationToken : UDINT
  AxisReference : DINT
  ExpectedActualPosition : DINT
  Result : DINT

CopyAxisZeroHomeResult
  OperationToken : UDINT
  pDest : ^void
  DestSize : UDINT
  Result : DINT
```

저장 후 알려주시면 LMC Home 본문 구현을 바로 이어가겠습니다. TW19/TW20 활성화를 위해 Axis1~4의 encoder family와 feedback socket 번호도 필요합니다.

<details><summary>15 previous messages</summary>

> LASAL 선언 저장을 기다리는 동안 멈추지 않고 진행하겠습니다. 먼저 정리된 설계·구현 문서의 요구사항을 현재 소스와 다시 대조해, IDE 선언과 무관하게 끝낼 수 있는 항목부터 처리하겠습니다.
>
> 정적 검증기는 현재 소스를 정확히 차단하고 있습니다. 첫 실패 원인은 기존 `ProcessAxisReference/ReferenceState/MoveReference`가 아직 IDE 선언에 남아 있다는 것입니다. 즉, 테스트를 억지로 통과시키지 않고 구 switch-search 경로가 제거될 때까지 Home capability가 열리지 않도록 되어 있습니다.
>
> 현재 소스 대조 결과, 8시대 히스토리 요약의 `0x7D17 미구현` 판정은 이미 낡았습니다. 이후 작업에서 PLC route/retire, WPF retirement, SDK 계약이 구현됐기 때문에, 다음 재개 때 잘못된 지점에서 시작하지 않도록 요약 문서에 최신 체크포인트를 덧붙이겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+66 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+1 -1)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.html` (+7 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PLAN_2026-07-30.md` (+7 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.html` (+7 -0)
> - Wrote `./docs/status/API_DEVELOPMENT_PROGRESS_2026-07-30.md` (+7 -0)
>
> </details>
>
> 추가로 실제 구현 전에 고쳐야 할 단위 불일치를 찾았습니다. TW19/TW20 요청은 현재 `TimeoutCycles`로 이름 붙어 있지만 SDO executor는 `TimeoutMs`를 받습니다. task 주기가 정확히 1 ms라는 보장이 없으므로 같은 숫자를 그대로 넘기면 안 됩니다. 현재 프로토콜·소스 근거를 대조해 단위를 하나로 확정하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+8 -2)
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+5 -4)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 제가 `MMC Home`이라고 표현한 건 잘못입니다. 우리 프로젝트의 명령·API·UI·문서는 모두 `LMC Home`으로 통일하고, `MMCLib`은 Elmo 원본 라이브러리 이름을 인용할 때만 남겨야 합니다. 지금 실제 개발 소스와 최신 설계/인계 문서에서 잘못 남은 `MMC` 표기를 찾아 바로 정리하겠습니다. LASAL IDE는 직접 조작하지 않겠습니다.
>
> 현재 소스 기준으로는 이미 `LmcAdminHome*`, `LmcAxisHome*`, `LMC_Home`만 공개되어 있고, 테스트도 `MMC_Home` 같은 공개 API가 생기면 실패하도록 막고 있습니다. 잘못 남은 곳은 최신 히스토리 요약의 과거 설계 표기 2곳입니다. 이 두 곳은 현재 계약과 혼동되지 않게 `LMC_Home` 기준으로 바로 고치겠습니다. 공식 Elmo 레퍼런스의 `MMCLib`/`MMC_*` 원문과 폐기 문서의 역사 기록은 프로젝트 명령으로 사용하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+2 -2)
>
> </details>
>
> 명칭 수정은 반영했습니다. 이어서 이 변경과 현재 PC 계약 상태를 함께 확인하겠습니다. SDK 전체, WPF 빌드·Home/엔코더 유지보수 smoke, 그리고 `MMC` 잔존 검색을 다시 돌려 현재 코드가 실제로 `LMC` 계약만 노출하는지 확인합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/history/260803/99_analysis_summary.md` (+1 -1)
>
> </details>
>
> 검증 결과는 명확합니다. 현재 공개 경로의 `MMC Home` 잔존은 0건이고, SDK는 `1074/1074`, WPF Home/엔코더 유지보수 smoke는 `23/23`, UI 언어 smoke는 `8/8` 모두 통과했습니다. 히스토리 요약의 시험 개수도 실제 결과인 `1074/1074`로 맞췄습니다. 이제 남은 LASAL 단계는 제가 IDE를 만지지 않고, 필요한 선언 수정만 정확히 사용자께 요청하는 방식으로 진행하겠습니다.
</details>

맞습니다. 우리 프로젝트 명령은 `MMC`가 아니라 `LMC`입니다. 제가 잘못 표현했습니다.

- 공개 명칭: `LMC_Home`, `LMC_HomeDS402`
- 최신 소스·WPF·LASAL에서 `MMC Home` 잔존 0건
- 과거 표현 2곳을 [99_analysis_summary.md](./docs/history/260803/99_analysis_summary.md:69)에서 수정
- `MMCLib`과 `MMC_*`는 Elmo 공식 원문 및 금지 API 검사에서만 유지

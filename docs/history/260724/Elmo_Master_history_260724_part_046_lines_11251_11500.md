  SubIndex       : USINT
  ReadLength     : UINT
  TimeoutMs      : UDINT
  ret_code       : iprStates
```

그 후 PLC에 다운로드하고 `Refresh Capabilities` 결과가 다음인지 확인하면 됩니다.

```text
Bits=0x0000213F
MaxSDO=4 bytes
```

권장 실기 테스트는 `0x6061:0 Int8/1`, `0x6041:0 BitField16/2`, `0x1018:1 UInt32/4`, 기존 `0x1000:0 UInt32/4`입니다. 아직 커밋하지 않았습니다.

> 이거 니가 해준거 0x6061:0 Int8/1, 0x6041:0 BitField16/2, 0x1018:1 UInt32/4 이거 다 테스트 해봤는데 익셉션만 나고 안돌아가는데?
>
> User context
>
> Attachments:
> - SDO_Test_Error.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Error.pcapng`

<details><summary>3 previous messages</summary>

> 세 항목이 모두 예외라면 “드라이브에 객체가 없다”기보다 공통 경로에 문제가 있을 가능성이 큽니다. 캡처에서 각 요청의 D5 응답, 티켓 상태, PLC `ErrorId/DetailCode`를 먼저 분리하고 현재 LASAL 실행 경로와 대조하겠습니다.
>
> 현재 LASAL IDE가 실행 중인 것도 확인했습니다. 우선 읽기 전용 분석만 하고, 원인이 확인되더라도 IDE가 열린 동안 소스는 수정하지 않겠습니다.
>
> 캡처에서 첫 번째 확실한 원인이 나왔습니다. 세 객체의 CoE/SDO 응답까지 도달한 것이 아니라, PLC가 `Submit SDO` 단계에서 즉시 `DetailCode=9`를 반환하고 있습니다. 즉 현재 예외는 “해당 인덱스 읽기 실패”가 아니라 기존 티켓/실행기가 Busy 상태여서 새 요청이 거부된 것입니다. Busy가 왜 해제되지 않는지 상태 머신을 더 추적 중입니다.
</details>

맞습니다. 현재 구현에 결함이 있습니다. 이번 예외는 객체 인덱스 오류가 아니라 PLC의 SDO executor가 `ResourceBusy`에 고정된 것입니다.

캡처 결과:

| 프레임 | 실제 전송 요청 | PLC 응답 |
|---|---|---|
| 201→202 | `0x6061:0`, `UInt16/2` | `ErrorId=-32000`, `DetailCode=9 ResourceBusy` |
| 468→469 | `0x6061:0`, `Int8/1` | 동일한 `ResourceBusy` |

- Capability는 `0x0000213F`, `MaxSDO=4`, `BootId=6`으로 정상입니다.
- 요청은 EtherCAT SDO 실행까지 가지 못하고 PLC Submit gate에서 거부됐습니다.
- 이 캡처에는 `0x6041`과 `0x1018` 요청이 없습니다.
- C# 예외는 PLC 오류 응답을 `LMCDiagnosticsCommandException`으로 변환한 것입니다.

원인은 상태 머신입니다.

- callback 검증 실패 시 executor가 `QUARANTINED`로 들어갑니다: [LMCSdoExecutor.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:641)
- 그런데 `CopyCompletion`은 `RESULT_READY`만 `IDLE`로 되돌리고 `QUARANTINED`는 복구하지 않습니다: [LMCSdoExecutor.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:469)
- 이후 모든 Submit은 계속 `ResourceBusy`가 됩니다: [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1689)
- `ARMING` 상태에서 vendor 요청을 시작한 후 `RUNNING`으로 바꾸는 순서에도 빠른 callback race가 있습니다.

수정 방향은 callback 노출 전에 `RUNNING`으로 전환하고, 정상 토큰의 실패 callback도 Service가 소비한 뒤 executor를 반드시 `IDLE`로 회수하도록 바꾸는 것입니다. 현재 잠긴 PLC 상태는 수정본 다운로드 전까지 PLC 재시작/재다운로드해야 풀립니다.

지금 LASAL IDE가 실행 중이라 소스는 건드리지 않았습니다. LASAL 변경 사항을 저장하고 종료해주면 상태 머신과 회귀 테스트까지 수정하겠습니다.

> IDE 종료했으니 수정작업 진행해

<details><summary>15 previous messages</summary>

> IDE 종료 여부를 확인한 뒤, 이번에는 단순 재시작 회피가 아니라 executor가 다시 `IDLE`로 회수되지 않는 상태 머신 결함과 빠른 callback 경쟁을 함께 수정하겠습니다. 기존에 정상 동작한 4바이트 경로는 유지하고, 정적 계약·테스트·실패 캡처 문서까지 같이 맞추겠습니다.
>
> 원인은 단순 Busy 처리 부족이 아니라 두 가지가 겹친 구조였습니다. `StartReadSDO`에 요청을 노출하기 전에 `RUNNING`을 확정하고, 정상 토큰의 callback은 검증 실패여도 Service가 결과를 소비한 뒤 executor를 다시 사용할 수 있게 회수하는 방식으로 고치겠습니다. 선언/네트워크는 건드리지 않아 LASAL 생성 메타데이터 재작업도 최소화합니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st` (+142 -78)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -5)
>
> </details>
>
> 핵심 상태 머신 수정 후 source-only와 생성 메타데이터 포함 전체 LASAL 정적 계약이 모두 통과했습니다. 이번 변경은 implementation 영역과 계약 검사만 바꿨기 때문에 class 선언/네트워크를 다시 배치할 필요는 없습니다. 이제 경쟁 조건과 실패 callback 회수 경로를 한 번 더 코드 감사한 뒤 문서와 회귀 검증을 마무리하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st` (+35 -20)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st` (+39 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+8 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+3 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs` (+2 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 소스 수정은 적용된 상태입니다. 지금은 병렬 리뷰 결과를 회수하면서 정적 계약·C# 빌드/테스트를 다시 돌리고, 캡처 분석 및 설계 문서의 상태를 실제 수정 내용에 맞춰 마무리하겠습니다. LASAL IDE는 실행하지 않습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+4 -4)
>
> </details>
>
> 강화한 정적 검사까지 포함해 LASAL SourceOnly와 네트워크/생성 메타데이터를 포함한 전체 계약이 모두 통과했습니다. 이제 PC 쪽 104개 계약 시험과 WPF Debug/Release 빌드로 회귀 여부를 확인 중입니다.
>
> PC 계약 시험은 Debug/Release 모두 104/104 통과했고, WPF도 두 구성에서 빌드 및 3초 시작 smoke test를 통과했습니다. 현재 남은 일은 캡처 근거와 새 상태 전이(`RUNNING` 선공개, `RELEASING`, hard quarantine)를 문서 전반에 동일하게 반영하는 작업입니다.
>
> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+12 -10)
> - Wrote `./LMC_Library/LMC_API/README.md` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -7)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+16 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+16 -8)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+39 -17)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+10 -6)
> - Wrote `./LMC_Library/README.md` (+7 -5)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+4 -3)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -2)
>
> </details>
>
> 문서도 실패 캡처의 사실과 소스 추정을 분리해 반영했습니다. 특히 캡처에는 `0x6041`과 `0x1018` 요청 자체가 없었다는 점, 두 Submit 모두 ticket 생성 전에 거부됐다는 점을 명확히 기록했습니다. 마지막으로 whitespace·ASCII·충돌·IDE 비실행 상태를 확인하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+32 -1)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+9 -5)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -2)
>
> </details>
</details>

외부 소스 수정은 완료했습니다. 원인은 SDO 미구현이 아니라 executor 복구 상태 머신 결함입니다.

- vendor 호출 전에 `Running`을 공개해 빠른 callback race를 제거했습니다.
- completion/error/orphan 처리 시 `Releasing` 상태에서 정리한 후 `Idle`로 복귀합니다.
- validation 실패도 terminal Failed로 소비되므로 다음 SDO를 재부팅 없이 실행할 수 있습니다.
- public ticket 없이 executor만 non-Idle이면 `ResourceBusy(9)` 대신 `InternalError(24)`로 구분합니다.
- 구현: [LMCSdoExecutor.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st:339), [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1730)

캡처 분석 결과도 저장했습니다: [SDO_Test_Error 분석](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Error_analysis_2026-07-22.md:10). 해당 캡처에는 `0x6061` 요청 두 건만 있으며 `0x6041`, `0x1018` 요청은 없습니다.

검증 결과:

- C# Debug/Release: 각각 104/104 PASS
- LASAL SourceOnly/full static: PASS
- WPF Debug/Release build 및 3초 startup: PASS
- `git diff --check`, LASAL ASCII 검사: PASS

다만 릴리스 전 P1이 하나 남았습니다. `LMCSdoExecutor`에 명시적 constructor가 없고 LASAL이 private 변수를 항상 0으로 초기화한다는 공식 보장을 확인하지 못했습니다. 현재 Busy의 직접 원인은 아니지만 결정적인 초기 상태를 위해 IDE에서 constructor declaration을 추가해야 합니다. 정확한 작업 조건은 [설계 가이드](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md:263)에 기록했습니다.

constructor를 IDE에서 추가하고 저장·종료하면, 제가 외부에서 초기화 implementation과 정적 assertion을 마무리하면 됩니다. 기존 PLC의 고착 상태는 남아 있으므로 이후 Rebuild/Download 또는 재시작 후 재시험해야 합니다. 커밋은 아직 하지 않았습니다.

> SDO 기능은 정상 구동되는거 확인했다. 이제 이 요구사항 문서 기준으로 우리가 API 어디까지 구현되어 있는지 점검 진행하고 그에 따른 구현 설계를 진행해보자.
>
> User context
>
> Attachments:
> - Motion_Controller_API_정리.xlsx: `./docs/Motion_Controller_API_정리.xlsx`

<details><summary>12 previous messages</summary>

> 요구사항 원본이 Excel이므로 `Spreadsheets` 스킬로 시트 구조·수식·표 범위를 먼저 정확히 추출하겠습니다. 그다음 현재 C#/LASAL/WPF 구현을 요구사항별로 대조하고, 확인된 공백만 우선순위·프로토콜·검증 기준까지 포함한 구현 설계 문서로 정리하겠습니다. SDO 실기 PASS는 이번 기준 상태에 반영합니다.
>
> <details><summary>Called a tool</summary>
>
> MCP tool call
>
> node_repl.js
>
> ```json
> {
>   "code": "var artifactResolution = await import.meta.resolve('@oai/artifact-tool'); nodeRepl.write(artifactResolution);",
>   "title": "Check spreadsheet runtime",
>   "timeout_ms": 30000
> }
> ```
>
> ```text
> Module not found: @oai/artifact-tool
> ```
>
> </details>
>
> 스프레드시트용 `@oai/artifact-tool` 런타임이 이 세션에는 노출되지 않아 workbook 렌더링/편집 경로는 사용할 수 없습니다. 이번 요청은 원본 수정이 아니라 요구사항 판독이므로 파일은 건드리지 않고 OOXML의 시트·셀·병합·수식 정보를 읽기 전용으로 추출해 분석을 계속하겠습니다. 시각 서식에 의미가 있는 경우는 별도 한계로 표시하겠습니다.
>
> 요구사항 원본은 실제 데이터가 `API 목록!A1:G68`에 있고, 기능 요구 65건으로 확인됐습니다. 수식·숨김 행·추가 요구사항 시트는 없으며 우선순위는 상 19건, 중 20건, 하 26건입니다. 이제 각 행을 단순 함수명 일치가 아니라 “동일 목적을 현재 구조에서 수행 가능한가” 기준으로 대조하겠습니다.
>
> 방금 우선순위 집계를 정정합니다. 65건 중 `상 21건`, `중 25건`, `하 19건`입니다. 앞선 수치는 수기 집계 오류였고, 이후 문서에는 이 확정 수치만 사용하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요구사항 65개를 현재 C# API, LASAL 명령 처리, WPF 노출 여부의 세 층으로 대조하고 있습니다. 다음으로 특히 우선순위 ‘상’인데 빠진 Home/OpMode/SetPosition 계열이 실제 LASAL 클래스에서 어떤 네이티브 호출로 연결 가능한지 확인한 뒤, 65개 전체 판정표와 단계별 구현 설계를 문서로 남기겠습니다. SDO Read는 이번에는 “사용자 실기 확인 완료”로 반영하되, 별도 캡처가 없는 항목은 그 증거 수준을 구분하겠습니다.
>
> 현재 소스에서 중요한 구조 차이도 확인됐습니다. `_LMCAxis` 자체에는 `MoveReference`, `SetPosition`, `SetParameter`, `ReadParameter`가 존재하지만 TCP/C# API로는 아직 노출되지 않았습니다. 반면 DS402 Operation Mode PDO(0x6060/0x6061)는 현재 Elmo 객체에서 비활성이라, `SetOpMode`를 단순 필드 쓰기로 추가하면 안 되고 SDO Write 안전 정책 또는 PDO 재구성이 선행되어야 합니다. 이 차이를 설계에 명시하고, 지금부터 현재 자동 테스트와 LASAL 정적 계약도 다시 실행해 문서의 기준 시점을 고정하겠습니다.
>
> 자동 검증은 현재 working source 기준으로 다시 통과했습니다: C# Debug/Release 각 104/104, LASAL SourceOnly와 네트워크 포함 정적 계약, WPF Debug/Release 빌드가 모두 PASS입니다. 다만 이 결과는 패킷/PLC 실기 완료율이 아닙니다. 요구사항 판정은 “직접 구현 14, LASAL식 목적 동등 21, 부분·비활성 11, 실제 미구현 15, 별도 1:1 구현 부적절 4”로 정리했고, 이제 이 기준과 우선 구현 순서를 전용 설계 문서에 기록하겠습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+398 -0)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
>
> </details>
>
> 전용 설계 문서는 작성했고 65개 행 번호·우선순위·판정 집계를 다시 기계적으로 확인했습니다. 현재 마지막으로 독립 리뷰를 돌려 `GroupStop`, 0x2051 좌표계, Homing 안전 조건처럼 잘못 설계하면 위험한 부분의 표현을 재검토 중입니다. 기존 34개 수정 파일에는 손대지 않았고, 이번 작업에서 새로 추가한 것은 설계 문서 1개뿐입니다.
</details>

점검 및 구현 설계를 완료했습니다. 요구사항 원본은 65개이며 우선순위는 상 21, 중 25, 하 19입니다. :codex-file-citation{path="C:/work/Elmo/Elmo_Master/docs/Motion_Controller_API_정리.xlsx" sheet="API 목록" range="A3:G68"}

결과는 다음과 같습니다.

- 직접 구현: 14개
- LASAL 방식의 목적 동등 구현: 21개
- 부분 구현·비활성 scaffold: 11개
- 실제 미구현: 15개
- 중복 또는 1:1 구현이 부적절한 항목: 4개

우선순위 ‘상’의 실제 공백은 `HomeDS402`, `HomeDS402Ex`, `SetOpMode`, `SetPosition` 4개입니다. SDO Read는 사용자 실기 확인 PASS로 반영했으며, SDO/PI Write와 8-byte SDO는 현재 의도적으로 비활성 상태입니다.

작성한 문서:

- [전체 판정 및 구현 설계](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:11)
- [65개 요구사항별 판정표](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:89)
- [단계별 구현 순서](/C:/work/Elmo/Elmo_Master/docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md:271)

첫 구현 슬라이스는 신규 기능보다 아래 계약 결함을 먼저 고치는 것으로 정했습니다.

1. Group motion 옵션을 실제 PLC 지원 범위로 fail-fast
2. `0x2051` Group Position 좌표계 계약 확정
3. `0x2085` GroupStop의 LASAL 반환값을 ACK에 반영
4. 관련 C#/LASAL 테스트와 문서 동기화

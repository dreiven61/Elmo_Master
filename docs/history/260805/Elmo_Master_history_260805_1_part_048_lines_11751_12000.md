> TW19 0x3204:0x13	실제 멀티턴 위치 초기화. 완전 미구현·명시적 차단
>
> 이거 다 사용할 수 있게 구현해줘
>
> 요청 범위를 네 기능 모두의 production 경로로 확장하겠습니다.
>
> - `0x7D13` MMC Home: 실제 실행 함수와 완료 판정까지 구현
> - `0x7D15/0x7D16` DS402 Home: gate 활성화에 필요한 ownership·복구·오류 계약까지 완성
> - `TW20 (0x3204:0x14)`: 전용 실행 경로 활성화 및 결과 판정
> - `TW19 (0x3204:0x13)`: 멀티턴 위치 초기화 경로를 새로 구현하되 오축·오세션 실행 방지와 명시적 확인을 포함
>
> LASAL IDE 작업은 직접 제어하지 않고 필요한 선언/빌드/다운로드 단계만 사용자에게 요청하겠습니다. 먼저 방금 복구한 `LMCEcatInputLatch`를 정상 CRLF로 고정하면서 네 경로의 현재 구현과 로컬 Elmo 근거를 병렬로 감사하겠습니다.
>
> `LMCEcatInputLatch` 복구가 완료됐습니다. 현재 1,457줄, CRLF, SHA-256 `05B3…CB9`, Git 정규화 해시가 복구 blob `39e25c…`와 정확히 일치합니다. 이제 SourceOnly 전체 계약으로 기능 경로가 실제로 되살아났는지 확인합니다.
>
> 복구된 mailbox 검사는 통과 지점을 넘겼습니다. 현재 SourceOnly의 다음 실패는 별도 drift입니다. `StartAxisReference` native gate가 이미 `TRUE`로 바뀌어 있는데 ownership/physical reference 입력 증거가 없어서 검증기가 막고 있습니다. MMC Home과 직접 연결되는 부분이므로 임의로 되돌리지 않고 현재 구현을 먼저 추적하겠습니다.
>
> `LMCControlCommandService`도 같은 10:02 저장 영향 여부를 확인 중입니다. 현재 `ProcessAxisReference()`는 실제로 빈 함수이고, 정적 검증기는 그보다 앞선 완전 구현 형태를 요구하고 있어 Git의 미도달 blob에서 정확한 저장 전 버전을 찾고 있습니다. 찾히면 수작업 재작성보다 먼저 byte-level 교차검증하겠습니다.
>
> 정확한 저장 전 후보를 찾았습니다. `LMCControlCommandService` blob `e6f3bdb…`는 09:33:45 생성, `ProcessAxisReference` 본문 8,184자, gate `FALSE`, 축별 `MoveReference()` 호출을 포함합니다. 현재 파일과 “누락된 블록만 추가된 버전”인지 diff로 교차검증합니다.
>
> 교차검증 결과 저장 전 후보는 현재 파일에 reference 구현을 복원하는 정확한 상위 버전입니다. 차이는 `399 insertions / 3 deletions`이며, 삭제 3줄은 dormant 주석/분기 교체이고 나머지는 gate 정의, 상태 변수, 4축 `MoveReference`, watchdog/terminal 처리입니다. 먼저 이 09:33 checkpoint를 production 소스에 복구한 뒤 그 위에서 gate와 ownership을 정식 활성화하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st` (+441 -45)
>
> </details>
>
> 이번 실패는 production 코드가 아니라 새 activation guard의 CRLF 처리 버그였습니다. fixture는 LF라 통과했지만 실제 `.st`의 `#define ... FALSE\r\n`을 정규식이 놓쳤습니다. gate 판정을 `\r?\n` 모두 처리하도록 verifier를 고치고, CRLF 양성 fixture도 추가하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+7 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -3)
>
> </details>
>
> 두 손상 파일의 정확 복구와 회귀 검증이 끝났습니다.
>
> - `LMCEcatInputLatch`: blob `39e25c…`와 exact
> - `LMCControlCommandService`: blob `e6f3bdb…`와 exact
> - SourceOnly 전체: PASS
> - ownership guard: CRLF 회귀를 포함해 `7/7` 음성 fixture PASS
>
> 이제부터는 복구가 아니라 실제 활성화 구현 단계입니다. 현재 세 기능은 모두 gate `FALSE`이고 TW19는 아직 경로가 없으므로, 공통 ownership을 먼저 production 코드로 넣어 동시에 실행되지 않도록 만들겠습니다.
>
> LASAL production 코드를 바꾸기 전에 저장소 규칙대로 코딩 규칙·프로그래밍 방법·오류 예방 가이드와 ownership 계약을 다시 읽고 있습니다. 이 규칙 때문에 새 class/network 선언은 나중에 사용자 IDE 단계로 분리하고, 기존 `.st` implementation은 외부에서 수정·정적 검증합니다.
>
> TW19의 write 값과 완료 판정은 현재 코드·문서만으로 확정할 수 없습니다. 로컬 Elmo drive PDF를 근거로 확인해야 하므로 PDF skill의 추출·검증 절차를 사용하겠습니다. 이 근거가 확정되기 전에는 `0x3204:0x13` 쓰기 값을 추정해 구현하지 않습니다.
>
> <details><summary>Searched the web</summary>
>
> Searched the web for `site:elmosoftware.com "TW[19]" ...`
>
> Searched the web for `"TW[19]" Elmo Motion Control ...`
>
> Searched the web for `"TW[19]" "socket" Elmo ...`
>
> Searched the web for `'Single-turn & Multi-turn position resets'`
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 복구 상태는 먼저 바로잡았습니다. `0x7D13` 실행 본체와 InputLatch 구현은 손실 전 소스로 정확히 복원했고, 현재 정적 계약은 통과합니다. 이제 게이트만 켜는 방식은 사용하지 않겠습니다. 공통 축 소유권, 중단·재시작·타임아웃, 그리고 TW19의 실제 쓰기 의미까지 확정한 뒤 구현하겠습니다. LASAL IDE 조작이 필요해지는 시점에는 제가 화면을 만지지 않고, 입력할 클래스·변수·연결을 정확한 순서로 요청드리겠습니다.
>
> 로컬 Elmo EtherCAT 구성도 확인했습니다. 네 축 모두 Vendor `0x009A`, Product `0x00030924`, Revision `0x00010420`으로 동일합니다. 다만 이 값만으로 각 축의 실제 feedback 센서 종류와 socket을 증명할 수는 없습니다. 따라서 TW19/TW20은 “명령을 보낼 수 있음”과 “현재 축에서 실행 허용됨”을 분리하고, 호환성 확인값 없이는 PLC가 write를 거부하도록 만들겠습니다.
>
> 정확한 명령 의미는 닫혔습니다. 공식 Elmo 로컬 문서 기준으로 `TW19/TW20` 모두 값은 feedback socket `1..4`, motor-off가 필수입니다. `0x20FC:1/2`도 공식 동등 object지만 타입이 2바이트라 `0x3204` 4바이트 경로와 섞거나 자동 fallback하면 안 됩니다. 구현은 요청하신 `0x3204:0x13/0x14`만 사용하고, mailbox 완료를 최종 성공으로 오인하지 않도록 사후 진단과 quarantine을 포함하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+8 -7)
> - Wrote `./docs/architecture/AXIS_COMMON_OWNERSHIP_INTERLOCK_DORMANT_CONTRACT_2026-08-03.md` (+5 -4)
> - Wrote `./docs/architecture/AXIS_HOME_AND_TEST_ENDAT22_ID30_ERROR_WARNING_RESET_IMPLEMENTATION_2026-07-31.md` (+4 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTestEndat22Id30ErrorWarningReset.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/HomeAndTestEndat22Id30ErrorWarningResetContractTests.cs` (+2 -2)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금은 LASAL IDE를 제가 직접 조작하지 않고, 외부에서 가능한 소스·프로토콜·테스트 구현을 먼저 확정하고 있습니다. 병렬 검토 결과를 합쳐서, IDE에서 필요한 선언·채널·Network 작업만 정확한 입력 목록으로 정리해 부탁드리겠습니다. 다만 실제 사용 가능 상태로 만들려면 축별 원점센서 배선과 엔코더 종류/소켓 정보는 결국 확인이 필요합니다.
>
> 계속 구현하는 동안 두 가지 실기 정보만 확인해 주세요.
>
> 1. 축 1~4의 MMC Home용 `RefSwitch` 실제 입력 채널, ON 극성(0/1), 그리고 `HWMin/HWMax` 연결 채널은 무엇입니까?
> 2. 축 1~4 각각의 피드백은 Panasonic / Tamagawa / EnDat 2.2 중 무엇이며, Elmo feedback socket 번호는 1~4 중 몇 번입니까?
>
> 현재 프로젝트에는 `_LMCAxis1..4.RefSwitch/HWMin/HWMax` 채널 자체는 있지만 물리 I/O 연결이 비어 있습니다. 이 정보 없이 Home/TW19/TW20 gate를 켜면 축 오동작 가능성이 있어 임의로 추정하지 않겠습니다.
>
> 공식 Elmo 자료 기준으로 TW19와 TW20의 대상은 확정했습니다. 둘 다 `0x3204`이고, TW19는 subindex `0x13`, TW20은 `0x14`, 값은 축 번호가 아니라 해당 드라이브의 feedback socket `1..4`입니다. 따라서 앞서 요청한 축별 소켓 정보가 프로토콜 입력값과 안전 검증에 직접 필요합니다.
>
> 현재로 Home을 하기위해 현재위치 0으로 만드는 home을 수행할꺼야.
>
> 리미트나 home 스위치 없이 동작하는거지
>
> 네. 요구사항을 이렇게 바꾸겠습니다: **축을 움직이지 않고, 리미트 스위치나 Home 스위치 없이 현재 기계 위치를 논리적 0으로 확정하는 Home**입니다.
>
> 따라서 기존 `0x7D13`의 “스위치를 찾고 backoff” 방식은 기본 Home으로 쓰지 않겠습니다. MMC 쪽은 현재 위치 기준 zero-set 방식, DS402 쪽은 Elmo가 지원하는 current-position homing 방식이 정확히 무엇인지 로컬 소스와 공식 문서로 대조한 뒤 같은 결과가 되게 맞추겠습니다. 이 경우 앞서 질문한 `RefSwitch/HWMin/HWMax` 정보는 필요 없습니다. 축 이동 명령도 발생시키지 않도록 계약과 테스트를 바꾸겠습니다.
>
> 확인했습니다. 공식 문서 기준으로도 정확히 가능합니다.
>
> - `MMC_Home`: `MC_DIRECT` 의미로 현재 실제 위치를 `0`으로 강제하며 물리 이동이 없습니다.
> - `DS402 Home`: 최신 Gold DS402의 **method 37** “Homing on the Current Position”을 쓰고 `0x607C=0`으로 설정합니다. 이 방식도 리미트/Home 스위치가 필요 없고, 드라이브가 Operation Enabled일 필요도 없습니다.
> - 기존 코드가 허용한 method 35도 같은 계열이지만 공식 문서에서 obsolete이므로 쓰지 않겠습니다.
>
> 즉 두 Home 버튼 모두 “현재 자리에서 좌표만 0으로 확정”하고, 검색 이동·backoff·리미트/Home 입력은 사용하지 않도록 구현하겠습니다.
>
> 우리는 MMC라는 명령어 안쓰기로 했자나 MMC -> LMC라고
>
> 맞습니다. 제가 명칭을 다시 잘못 썼습니다.
>
> 프로젝트 공개 명칭은 전부 `LMC`로 통일합니다.
>
> - `0x7D13`: `LMC_Home` — 현재 위치를 0으로 설정, 무이동
> - `0x7D15/16/17`: `LMC_HomeDS402` — DS402 method 37, 현재 위치 0, 무이동
> - `MMC_*` 이름은 SDK·WPF·문서·주석에서 제거
> - Elmo 공식 문서의 원래 함수명을 근거로 인용할 때만 `MMC_Home`이라고 표시
>
> 지금 진행 중인 코드도 `MmcHome`이 아니라 `LmcHome`/`AxisZeroHome` 계열로 수정시키겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 맞습니다. 프로젝트 공개 명령·클래스·UI·문서에서는 `MMC`를 쓰지 않고 전부 `LMC`로 통일하겠습니다. 공식 Elmo API 원문을 설명할 때만 출처명으로 `MMC_Home`을 표기하고, 구현 명칭은 `LMC_Home(0x7D13/0x7D18/0x7D19)`으로 진행합니다. 먼저 병렬 작업 결과와 현재 변경분에서 `MMC`가 다시 들어간 곳이 없는지 확인하겠습니다.
>
> 기존 작업분에는 실제로 `LmcMmcHome.cs`, `PrepareMMCHome`, `ButtonMmcHome` 같은 이름이 남아 있습니다. 이것들은 호환 alias로 남겨둘 대상이 아니라 제거·개명 대상입니다. 동시에 화면의 “축이 움직일 수 있음 / MoveReference” 설명도 현재 요구사항과 반대이므로, `현재 위치를 0으로 좌표 재설정하며 물리 이동 없음`으로 바꾸겠습니다. LASAL IDE에서 필요한 선언 작업은 제가 소스 설계를 확정한 뒤 항목별로 부탁드리겠습니다.
>
> `LMC_Home`의 `SetPosition()`은 일반 TCP task에서 직접 호출하지 않고, 이미 축의 `LMCPreRtWorkTrigger`에 연결된 `LMCEcatInputLatch`의 RT mailbox에서 실행하는 구조로 잡겠습니다. 그래야 “같은 core이며 motion RT보다 같거나 낮은 priority에서만 호출”이라는 `_LMCAxis.SetPosition()` 계약을 지킬 수 있고, 원시 drive 위치가 바뀌지 않았다는 전후값도 함께 남길 수 있습니다. 이 구조를 기준으로 IDE 입력 목록을 문서화하고 있습니다.
>
> <details><summary>Created a file</summary>
>
> - Wrote `./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md` (+375 -0)
>
> </details>
>
> LASAL IDE 작업을 부탁드립니다. [IDE handoff 문서](./docs/architecture/LMC_HOME_CURRENT_POSITION_ZERO_AND_ENCODER_MAINTENANCE_IDE_HANDOFF_2026-08-03.md)에 정확한 변수·함수 signature·Network 연결을 적었습니다.
>
> 지금은 문서의 **3~5절만** 수행해 주세요.
>
> - `LMCEcatInputLatch`, `LMCControlCommandService`, `LMCDiagnosticsService` declaration 입력
> - Comm Network 연결 2개 추가
> - `Save All`
> - **F9는 아직 하지 말기**
> - 저장 완료 시각을 알려주기
>
> 그리고 Axis1~4 각각의 `encoder family`와 `feedback socket(1~4)`도 알려주세요. 이 정보가 없으면 TW19/TW20 코드는 완성해도 실축 capability는 안전상 켤 수 없습니다. 저는 기다리는 동안 SDK/WPF와 `MMC → LMC` 개명을 계속 진행하겠습니다.
>
> <details><summary>Edited 5 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminModels.cs` (+12 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs` (+52 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs` (+6 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResponsePayloadLimits.cs` (+16 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminProtocol.cs` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ResponsePayloadLimitTests.cs` (+6 -1)
>
> </details>
>
> <details><summary>Edited 7 files, deleted a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs` (+14 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsProtocol.cs` (+22 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+8 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/LasalMotionControlLib.Tests.csproj` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Program.cs` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminDs402HomeOutcomeRetirementContractTests.cs` (+3 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAxisDs402Home.cs` (+11 -11)
> - Deleted `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/HomeAndTestEndat22Id30ErrorWarningResetContractTests.cs` (+0 -932)
>
> </details>
>
> PC 쪽 중앙 통합을 반영했습니다. `0x7D13/18/19`, `0x7E53/54/55`, capability bit 18/19, detail code, payload limit, 프로젝트 등록을 연결했고, DS402 공개 alias도 `LMC_HomeDS402`로 바꿨습니다. 이제 먼저 SDK 전체 빌드에서 누락된 중앙 계약과 구명칭 참조를 잡겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+0 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminLmcHomeContractTests.cs` (+1 -1)
>
> </details>
>
> SDK와 테스트 프로젝트는 Release 기준 경고·오류 0으로 빌드됐습니다. 전체 계약 테스트를 돌리고 있으며, 여기서 새 Home/DS402/TW19/TW20뿐 아니라 기존 RPC 회귀까지 함께 확인합니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 지금은 두 갈래로 진행하겠습니다. 전체 테스트의 진단 상세코드 경계(기존 32 → 현재 42) 때문에 생긴 퍼즈 실패를 먼저 고치고, 동시에 WPF 쪽 `LMC_Home` 전환 결과와 남아 있는 구형 TW20 경로를 점검하겠습니다. LASAL IDE 선언 저장 결과를 받기 전까지는 생성부를 건드리지 않고 외부 편집 가능한 구현 준비만 진행합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsParserDeterministicFuzzTests.cs` (+2 -1)
>
> </details>
>
> LASAL 쪽은 저장 흔적이 보이기 시작했는데, 선언 타입 3개가 전달 문서와 다르게 생성된 상태를 확인했습니다. 테스트가 끝나는 동안 현재 소스와 저장 시각을 다시 확인한 뒤, IDE에서 고칠 항목만 짧고 정확하게 요청드리겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcErrorCatalog.cs` (+70 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminDs402HomeOutcomeRetirementContractTests.cs` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsContractTests.cs` (+2 -1)
>
> </details>
>
> 지금 LASAL IDE에서 `LMCEcatInputLatch`의 함수 파라미터 타입 4개만 바로 수정해 주세요. 현재 `.st`에 잘못 생성되어 있습니다.
>
> - `SubmitAxisZeroHome.OperationToken`: `DINT` → `UDINT`
> - `CopyAxisZeroHomeResult.OperationToken`: `DINT` → `UDINT`
> - `CopyAxisZeroHomeResult.pDest`: `DINT` → `^void`
> - `CopyAxisZeroHomeResult.DestSize`: `DINT` → `UDINT`
>
> `AxisReference`와 두 함수의 `Result`는 `DINT` 그대로입니다. IDE에서 수정 후 **Save All만** 하고 F9는 아직 누르지 마세요. 저장하셨다고 알려주시면 제가 생성 결과를 확인하고 구현부를 넣겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/ErrorCatalogTests.cs` (+2 -2)
>
> </details>
>
> <details><summary>Edited 5 files, deleted 2 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LasalMotionControlLib.csproj` (+0 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5.cs` (+6 -25)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsEncoderMaintenanceModels.cs` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsModels.cs` (+0 -1)
> - Deleted `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTestEndat22Id30ErrorWarningReset.cs` (+0 -284)
> - Deleted `./LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsTestEndat22Id30ErrorWarningResetModels.cs` (+0 -439)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWritePolicyEvaluationTests.cs` (+61 -0)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeModels.cs` (+18 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcAdminDs402HomeProtocol.cs` (+6 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminDs402HomeCurrentPositionZeroContractTests.cs` (+25 -11)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/AdminDs402HomeOutcomeRetirementContractTests.cs` (+2 -2)
>

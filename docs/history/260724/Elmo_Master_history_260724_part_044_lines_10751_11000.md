- 실제 wire의 `Refresh Ticket(0x7E03)`: **5회**
- 5회 모두 동일:

```text
State=Expired
Outcome=TimedOut
SubmitCycle=1443742
CompletionCycle=1443742
ErrorId=0
DetailCode=0x05040000
ResultLength=0
```

UI에서 6회 클릭했더라도 pcap에는 5회만 전송됐습니다. TCP 누락이나 재전송은 없으므로 한 번은 캡처 범위 밖이거나 wire 요청으로 나가지 않은 것입니다. 판정에는 영향 없습니다.

핵심은 `SubmitCycle == CompletionCycle`입니다. PC는 Timeout `1000`을 정확히 전송했지만 PLC는 1000 cycle을 기다리지 않고 같은 cycle에 즉시 Timeout 처리했습니다.

원인은 LASAL 코드의 대소문자 무시 shadowing입니다.

```st
SdoTimeoutCycles := sdoTimeoutCycles;
```

- [로컬 변수 선언](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:947)
- [잘못된 자기 대입](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:1734)
- [즉시 Timeout 판정](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:733)

멤버의 `SdoTimeoutCycles`가 0으로 남아 `0 >= 0`이 성립하며, 실제 `TryStartRead4` 호출 전에 Expired 됩니다. SlaveReference, ObjectIndex, SubIndex도 같은 문제가 있으므로 네 로컬 변수를 모두 `requestSdo*` 형태로 변경해야 합니다.

화면 로그의 `Refresh Diagnostics Operation PASS`는 **조회 RPC가 성공했다는 뜻**이지 SDO가 성공했다는 뜻은 아닙니다. 실제 SDO 결과는 `Expired/TimedOut`입니다.

다음 단계는 이 네 변수 shadowing을 수정하고 LASAL 재빌드·다운로드한 뒤, 우선 Slave 1만 다시 시험하는 것입니다. 정상 기준은 `Completed + Success + ResultLength=4`입니다.

> 수정안을 설계하고 구현 진행하자

<details><summary>3 previous messages</summary>

> 같은 문제가 다시 생기지 않도록 단순 변수명 수정에 그치지 않고, ① 요청 로컬 변수 명명 규칙, ② case-insensitive shadowing 정적 회귀검사, ③ SDO 정상·타임아웃 상태 기준까지 묶어서 수정하겠습니다. 먼저 LASAL 편집 안전 상태와 현재 미커밋 변경을 확인하고 필수 코딩 규칙을 다시 읽겠습니다.
>
> 현재 `Lasal2`가 프로젝트를 연 상태로 실행 중입니다. 사용자께서 지정한 덮어쓰기 방지 규칙에 따라 `.st` 편집은 아직 시작하지 않겠습니다. 그동안 읽기 전용으로 수정 설계와 회귀검사 범위를 확정하고, 구현 직전에는 LASAL 종료가 필요합니다.
>
> 수정 설계는 확정됐습니다. 요청 파싱 로컬은 모두 `requestSdo*`, ticket에 보존되는 멤버는 `Sdo*`로 명확히 분리하고, 정적 계약 검사가 두 영역의 식별자를 대소문자 무시로 비교해 충돌 시 실패하도록 만들겠습니다. 현재 `Lasal2` PID 37108이 열려 있으니 프로젝트를 저장한 뒤 LASAL IDE만 종료해 주세요. 종료되는 동안 테스트·문서 패치 위치를 계속 준비하겠습니다.
</details>

수정 설계는 확정됐지만, 현재 `Lasal2` PID `37108`이 프로젝트를 열고 있어 편집을 중단했습니다. 외부 수정본이 IDE 저장으로 덮어써질 수 있습니다.

적용할 수정은 다음입니다.

- 충돌하는 로컬 변수 4개를 `requestSdo*`로 변경
  - `requestSdoSlaveReference`
  - `requestSdoObjectIndex`
  - `requestSdoSubIndex`
  - `requestSdoTimeoutCycles`
- ticket 멤버에 명시적으로 복사
- `Verify-LasalContract.ps1`에 모든 함수 로컬과 class member를 대소문자 무시 비교하는 일반 shadowing 검사 추가
- `SDO_Test.pcapng` 실패 결과와 재시험 기준을 설계·시험 문서에 반영
- Network, class 선언, TCP wire 규격은 변경하지 않음
- 기존 미커밋 변경은 그대로 보존

LASAL IDE를 저장 후 종료하고 알려주세요. 종료가 확인되면 바로 소스·테스트·문서를 수정하고 정적 계약과 C# 테스트까지 실행하겠습니다.

> IDE 종료했다. 계속 진행해

<details><summary>14 previous messages</summary>

> LASAL 종료 여부와 작업트리 기준점을 다시 확인한 뒤 패치를 적용하겠습니다. 구현부만 수정하고 생성 선언·Network·wire 계약은 건드리지 않습니다.
>
> <details><summary>Created a file, edited 4 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st` (+16 -16)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+76 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+3 -2)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+47 -9)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+16 -4)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+93 -0)
>
> </details>
>
> 핵심 소스와 재발 방지 검사를 반영했습니다. 이제 정적 계약이 실제로 새 이름과 retained 복사를 강제하는지, 기존 Network 계약까지 깨지지 않았는지 확인하겠습니다.
>
> 정적 계약 source/full과 PC 자동시험 `103/103`이 모두 통과했고, 새 shadowing 검사는 메모리에서 결함 이름을 다시 주입했을 때 의도대로 실패했습니다. 이제 diff를 독립 검토해 정규식 오탐·문서 과장·기존 미커밋 변경 침범 여부를 확인하겠습니다.
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+61 -20)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+18 -13)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+10 -8)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+17 -12)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+2 -2)
>
> </details>
>
> 검증용 합성 PowerShell 문자열 작성에서만 here-string 문법 오류가 났습니다. 제품 소스나 테스트에는 영향이 없으며, 해당 검증 명령을 바로잡아 전체 결과를 다시 수집합니다.
>
> 현재 release-status와 remaining-plan 문서에는 아직 “D5 runtime 미실시”라고 남아 있습니다. 실제로는 첫 시험이 실패했고 수정 source가 정적 검증을 통과한 상태이므로, 이 두 System-of-Record 문서도 같은 경계로 맞추겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+16 -16)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+23 -17)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> LASAL 종료 상태를 전제로 외부 소스 편집을 계속하겠습니다. 남은 문서 표현을 “캡처 당시 수정 전 소스”로 정확히 바로잡고, 정적 계약·빌드·diff 검사를 마지막으로 다시 확인하겠습니다.
>
> 패킷과 코드의 인과관계는 맞지만 문서 일부가 수정 전 결함을 “현재 source”라고 표현해 오해 소지가 있습니다. 그 부분과 기존 “진단 시험 미실시” 문구를 실제 상태인 “D5 첫 시험 실패, D0~D4 미실시”로 정정하겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+4 -3)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+4 -3)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+3 -3)
>
> </details>
>
> 구현부 재검토에서도 남은 코드 결함은 발견되지 않았습니다. 이제 동일 소스에 대해 source-only/full-network 계약과 PC 자동시험 103개를 다시 돌려 회귀 여부를 확정하겠습니다.
>
> 정적 계약 두 경로와 PC 자동시험이 모두 통과했습니다(103/103). 마지막으로 whitespace, LASAL 추가 코드의 ASCII 규칙, 잔존한 구 결함 이름, IDE 프로세스 및 작업트리 범위를 점검하겠습니다.
</details>

SDO immediate-timeout 수정과 문서 정리를 완료했습니다.

- 원인: LASAL의 대소문자 비구분 때문에 request-local 변수가 class member를 가려 timeout이 `0`으로 남았습니다.
- 수정: local 이름을 `requestSdo*`로 변경하고 ticket-state 복사를 바로잡았습니다. [LMCDiagnosticsService.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:947)
- 회귀 방지: local/class member 대소문자 충돌을 자동 검출하고, SDO 요청값 6개가 retained ticket으로 복사되는지 검사합니다. [Verify-LasalContract.ps1](./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1:22)
- 패킷 분석과 설계·시험·릴리스 문서를 갱신했습니다.
  - [SDO 캡처 분석](./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md:1)
  - [SDO 설계와 수정 내역](./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md:345)
  - [PLC 재시험 절차](./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md:179)
  - [현재 릴리스 상태](./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:49)

검증 결과:

- LASAL source-only 계약: PASS
- LASAL full-network 계약: PASS
- PC 자동시험: `103/103 PASS`
- `git diff --check`, `git diff --cached --check`: PASS
- 추가 LASAL 코드 7-bit ASCII: PASS
- LASAL IDE 프로세스 종료 상태 확인

이제 사용자가 할 작업은 다음입니다.

1. LASAL을 열고 변경 class를 `Reload Class`.
2. Rebuild/Link 후 `Find in Implementation` smoke test.
3. PLC에 다운로드.
4. Slave 1에서 `0x1000:0`, UInt32, 4 bytes, timeout 1000으로 Submit 후 Refresh.
5. `Completed/Success`, `ResultLength=4`, 실제 data가 나오면 정상입니다. 이후 Slave 2~4를 시험하십시오.
6. 다시 `SubmitCycle=CompletionCycle`인 `Expired/TimedOut`이 나오면 PLC trace로 `TryStartRead4` 진입 여부를 확인해야 합니다.

현재는 정적 구현까지 완료됐으며, 수정본의 PLC 실기 성공은 아직 확인되지 않았습니다. 기존 작업트리는 보존했고 이번 변경은 아직 stage/commit하지 않았습니다.

> 테스트 완료했다
>
> User context
>
> Attachments:
> - SDO_Test2.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test2.pcapng`

<details><summary>5 previous messages</summary>

> 새 캡처를 수정 전 `SDO_Test.pcapng` 및 정상 판정 기준과 직접 비교하겠습니다. 우선 요청·Ticket 상태·cycle delta·결과 4바이트를 추출하고, 실제 SDO 완료 증거인지 TCP 계층만 성공한 것인지 분리해서 판정하겠습니다.
>
> 새 캡처의 핵심 상태는 이미 달라졌습니다. Ticket 5가 `Completed/Success`로 끝났고 Submit–Completion 간 54 PLC cycle이 발생했으며 4바이트 결과도 포함되어 있습니다. 필드 오프셋과 값(`0x00020192` 등)을 소스 파서와 교차 확인 중입니다.
>
> 판정은 명확합니다. 수정본은 Slave 4에서 `Completed/Success`, 4바이트 `0x00020192`, 54-cycle 완료로 정상 동작했습니다. 이 결과를 새 캡처 분석 문서로 남기고, 설계·시험·릴리스 문서의 “재시험 대기”를 “Slave 4 실기 PASS, 나머지 축·오류 경로 대기”로 갱신하겠습니다.
>
> <details><summary>Created a file, edited 5 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+10 -9)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+33 -10)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+22 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+20 -18)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test2_analysis_2026-07-22.md` (+105 -0)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_analysis_2026-07-22.md` (+2 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
>
> </details>
>
> 문서에는 성공 범위도 제한해서 기록했습니다. 이번 캡처가 직접 증명하는 것은 Slave 4 happy path이며, Slave 1~3과 abort/offline/timeout/cancel/orphan은 아직 완료로 올리지 않았습니다. 최종 문서 대조 검토와 diff 검사를 마무리하겠습니다.
</details>

테스트 성공입니다. 수정한 SDO Read가 Slave 4에서 정상 동작했습니다.

| 항목 | 결과 |
|---|---|
| 요청 | Slave 4, `0x1000:0`, UInt32, 4 bytes |
| Ticket | 5, Queued |
| 완료 | `Completed / Success` |
| Cycle | 92042 → 92096, 총 54 cycles ≈ 54 ms |
| 오류 | ErrorId 0, Detail 0 |
| 결과 | `92 01 02 00` = `0x00020192` = 131474 |
| 반복 조회 | 3회 모두 동일한 성공 결과 |

수정 전의 `SubmitCycle=CompletionCycle` 즉시 timeout은 제거됐습니다. Slave 4 기준으로 PLC derived executor → ticket → inline result → PC 응답까지 end-to-end 정상입니다. 4바이트 결과는 `Refresh Ticket`에 이미 포함되므로 `Download Result`는 필요 없습니다.

같이 실행된 EtherCAT Health도 축 1~4 모두 Online/OP, AL Code 0, AxisError 0이었습니다. TCP 재전송이나 순서 오류도 없습니다.

다만 이번 캡처가 직접 증명하는 범위는 Slave 4뿐입니다. Slave 1~3과 abort/offline/timeout/cancel/orphan은 추가 시험이 필요합니다. 또한 캡처에는 EtherCAT `0x88A4` frame이 없으므로 mailbox frame 자체를 독립 관측한 자료는 아닙니다.

분석 및 상태 문서도 갱신했습니다.

- [SDO_Test2 상세 분석](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/SDO_Test2_analysis_2026-07-22.md:11)
- [SDO 설계 재시험 결과](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md:383)
- [PLC 시험 기록](/C:/work/Elmo/Elmo_Master/docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md:208)
- [현재 릴리스 상태](/C:/work/Elmo/Elmo_Master/docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md:50)

다음에는 Slave 1, 2, 3을 같은 조건으로 연속 시험해 하나의 pcap으로 저장하면 됩니다. 아직 stage/commit은 하지 않았습니다.

> 캡쳐했다. 다시 확인하고 문제없으면 SDO 기능 다 완료시켜
>
> User context
>
> Attachments:
> - SDO_Test_Slave123.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Slave123.pcapng`

<details><summary>8 previous messages</summary>

> 이번 캡처에서 Slave 1·2·3 각각의 요청, Ticket, terminal 상태, cycle delta, 4바이트 결과와 TCP 이상 여부를 확인하겠습니다. 모두 통과하면 현재 승인 범위를 “4축 first-slice SDO Read 완료”로 정리하되, SDO Write와 고장 유도 시험은 별도 범위로 명확히 남기겠습니다.
>
> 초기 해석상 Slave 1·2·3 모두 `Completed/Success`이며 같은 UInt32 결과를 반환했습니다. 완료 시간은 각각 43, 51, 43 cycles로 timeout 1000 cycles 안입니다. TCP 오류와 캡처 범위를 더 확인한 뒤 4축 Read 완료 상태로 문서를 정리하겠습니다.
>
> 분석 결과 추가 소스 수정은 필요 없습니다. 현재 설계 범위인 물리축 1~4 `0x1000:0` UInt32 4-byte SDO Read happy path는 완료로 판정할 수 있습니다. 다만 SDO Write와 fault/cancel/orphan 시험은 별도 항목이므로 “전체 SDO production 완료”로 과장하지 않고 문서 상태를 일괄 정리하겠습니다.
>
> <details><summary>Created a file, edited 15 files</summary>
>
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test2_analysis_2026-07-22.md` (+10 -9)
> - Wrote `./test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Slave123_analysis_2026-07-22.md` (+72 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+13 -13)
> - Wrote `./docs/architecture/ELMO_NATIVE_API_PACKET_CAPTURE_ANALYSIS_2026-07-21.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+31 -12)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+18 -4)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+14 -13)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+9 -4)
> - Wrote `./docs/architecture/LMC_NATIVE_CAPTURE_ALIGNMENT_IMPLEMENTATION_DESIGN_2026-07-21.md` (+7 -4)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+11 -11)
> - Wrote `./LMC_Library/LMC_API/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+14 -13)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+9 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -3)

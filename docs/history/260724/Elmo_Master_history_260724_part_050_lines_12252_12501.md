> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+3 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+7 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+17 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+20 -6)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+18 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+6 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+43 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+8 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+14 -3)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+16 -7)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+10 -4)
>
> </details>
>
> UI 코드와 사용 안내를 반영했습니다. 이제 Debug/Release 빌드와 기존 148개 API 회귀시험, XAML 이벤트 연결, 짧은 startup smoke를 확인하겠습니다. 사용자 LASAL 저장 변경(`Classes.lcb`)과 새 캡처 로그는 건드리지 않고 있습니다.
>
> 현재 사용자가 실행 중인 TEST UI 프로세스가 기존 `bin\Debug` DLL을 잠그고 있어 일반 Rebuild는 실패했습니다. 실행 중인 시험은 종료시키지 않겠습니다. 새 빌드는 별도 임시 출력 폴더로 분리해서 검증하겠습니다. 코드 경고 하나도 함께 정리합니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+1 -3)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 실행 중인 기존 TEST UI는 그대로 두고, 별도 출력 폴더에서 API 회귀 테스트와 새 UI 시작 검증을 진행하겠습니다. 새 화면은 현재 실행 중인 인스턴스를 닫고 다시 빌드·실행한 뒤부터 적용됩니다.
>
> Debug API 회귀 테스트는 148/148 통과했습니다. 이제 Release 회귀와 별도 Debug 실행 파일의 시작 smoke test를 확인하고, 마지막으로 diff에서 안전 상태 전이와 버튼 활성화 조건을 재검토하겠습니다.
>
> 실행 중이던 TEST UI와 디버거 프로세스가 현재는 종료된 것을 확인했습니다. 잠금이 풀린 지금 실제 기본 `bin\Debug`/`bin\Release` 경로도 다시 빌드해, 임시 출력 검증에만 의존하지 않도록 마무리하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+15 -3)
>
> </details>
>
> 나는 테스트 완료했으니 이거 가지고 분석도 이어서 진행해
>
> User context
>
> Attachments:
> - 05b_Group_Relative_Aborting_PerAxis_7D22.txt: `./test/packet_capture/SIGMATEK_API_Analyze/05b_Group_Relative_Aborting_PerAxis_7D22.txt`
> - 08b_PowerOff_FinalStatus_2045.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/08b_PowerOff_FinalStatus_2045.pcapng`
> - 08b_PowerOff_FinalStatus_2045.txt: `./test/packet_capture/SIGMATEK_API_Analyze/08b_PowerOff_FinalStatus_2045.txt`
> - 10_DriveRead_Axis1to4.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/10_DriveRead_Axis1to4.pcapng`
> - 10_DriveRead_Axis1to4.txt: `./test/packet_capture/SIGMATEK_API_Analyze/10_DriveRead_Axis1to4.txt`
> - 11_PI_Bulk_Regression.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/11_PI_Bulk_Regression.pcapng`
> - 11_PI_Bulk_Regression.txt: `./test/packet_capture/SIGMATEK_API_Analyze/11_PI_Bulk_Regression.txt`
>
> TEST UI 개선은 기본 Debug/Release 빌드까지 통과한 상태로 최종 리뷰 중입니다. 방금 받은 캡처는 병행 분석해서, `05b` 축 매핑·`08b` PowerOff 최종 상태·`10` Drive Read·`11` PI/Bulk를 각각 실제 요청/응답과 로그가 일치하는지 판정하겠습니다.
>
> 최종 리뷰에서 PowerOff 확인 대기 중에도 `Get Members`와 `Home Check`가 눌리는 작은 흐름 불일치가 확인됐습니다. 이때는 `Read Status`와 안전용 `Group Stop`만 남기는 방향으로 바로 보완한 뒤, 새 캡처 분석 결과와 함께 다시 검증하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+7 -2)
>
> </details>
>
> 캡처 판정은 명확합니다. `10` Drive Read와 `11` PI/Bulk는 wire 계약까지 전부 PASS했고, `08b`는 PowerOn 비트 해제를 확정했습니다. `05b`는 축 순서는 PASS지만 파일명과 달리 실제 `Buffer=2(Buffered)`였습니다. 기존 `05`가 `Buffer=1(Aborting)` 성공을 이미 담고 있어 두 캡처를 합치면 기능 범위는 닫히며, 이 차이는 분석 문서에 명시하겠습니다.
>
> <details><summary>Created a file, edited 7 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md` (+192 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+10 -10)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+19 -15)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+22 -17)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -0)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+3 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+4 -2)
>
> </details>
>
> 분석 문서와 현재 상태 문서까지 실제 캡처 기준으로 갱신했습니다. 이제 코드 최종 변경(PowerOff 대기 중 다른 준비 버튼 차단, monitor 제한시간 표시)을 포함해 기본 Debug/Release 빌드와 148개 회귀를 다시 실행하고, 문서 링크·diff 오류까지 확인하겠습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md` (+3 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
>
> </details>
>
> 문서 교차 리뷰에서 두 가지 오래된 표현을 찾았습니다. 전체 25개 명령을 아직 모두 재시험하지 않았다는 뜻을 `0/25`로 잘못 남긴 부분과, 실패 캡처인 `SDO_Test_Error`를 성공 증거처럼 묶은 부분입니다. 이를 실제 증거대로 `legacy 4-byte 성공 / 신규 1·2-byte 성공 / general-inline 4-byte 사용자 확인 / 실패 캡처`로 분리해 바로잡겠습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+11 -9)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+3 -2)
> - Wrote `./docs/architecture/MOTION_CONTROLLER_API_REQUIREMENTS_COVERAGE_AND_IMPLEMENTATION_DESIGN_2026-07-22.md` (+1 -1)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 테스트 캡처 분석과 TEST UI 개선은 모두 반영해 둔 상태입니다. 지금은 문서에 남은 과거 표현이 새 실측 결과와 충돌하지 않는지 마지막으로 교차 점검하고, 전체 빌드·회귀 결과와 함께 최종 판정을 정리하겠습니다. 사용자 LASAL 프로젝트 변경과 캡처 원본은 건드리지 않습니다.
>
> 캡처 결론 자체는 일관되지만, 7월 10~16일에 작성된 배포·시험 문서 일부가 아직 `PLC E2E 0/25`, “신규 pcap 없음”을 현재 상태처럼 적고 있습니다. 히스토리 파일은 당시 기록이므로 보존하고, 현재도 참조되는 문서에는 2026-07-23 실측 결과가 우선한다는 갱신 표기를 넣어 오판 가능성을 없애겠습니다.
>
> <details><summary>Edited 7 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+13 -12)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+6 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/BUILD_METADATA_2026-07-16.md` (+5 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+8 -4)
> - Wrote `./docs/architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md` (+18 -14)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+24 -18)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+11 -9)
>
> </details>
>
> 실측 근거를 문서 전반에 반영했습니다. 특히 SDO는 “1/2/4-byte 모두 성공 캡처”로 뭉뚱그리지 않고, `1/2-byte는 이번 pcap으로 확인`, `4-byte는 legacy 캡처 및 사용자 확인 범위`로 분리했습니다. 05b 역시 파일명과 달리 실제 요청은 전부 `Buffered(2)`였다는 점을 유지하고 있습니다.
>
> <details><summary>Edited 3 files</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -2)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -1)
>
> </details>
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+1 -1)
> - Wrote `./docs/architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md` (+6 -4)
>
> </details>
>
> 최종 문서 감사에서도 충돌 0건으로 확인됐습니다. 이제 변경된 UI 소스만 다시 빌드·회귀 검사하고, 사용자 캡처와 LASAL `Classes.lcb`를 분리한 상태로 결과를 넘기겠습니다. 이번 요청에는 커밋 지시가 없어서 스테이징·커밋은 하지 않습니다.
>
> 첫 startup 확인에서 앱은 3초 후에도 정상 생존했지만, PowerShell의 `Wait-Process -Timeout`이 “아직 실행 중”을 명령 실패 코드로 반환해 검사 스크립트만 1로 끝났습니다. 앱 종료가 아니라 검사 방식 문제이므로 프로세스 API로 다시 확인하겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_INTERNAL_PLC_TEST_GUIDE_2026-07-21.md` (+6 -4)
>
> </details>
</details>

TEST UI 개선과 신규 캡처 분석을 모두 완료했습니다.

캡처 판정:

- `05b`: X/Y/Z/U 축별 매핑 정상. 단, 파일명과 달리 실제 요청은 모두 `Buffered(2)`입니다. 진짜 연속 Buffered queue 동작은 아직 미검증입니다.
- `08b`: `0x40050000 → 0x40010000`으로 Power Ready 비트만 해제되어 PowerOff 완료 PASS입니다.
- `10`: 축 1~4의 SDO 12건 모두 `Completed/Success`. Operation Mode는 8(CSP), Statusword는 `0x02B3`입니다.
- `11`: Capability, 24개 Catalog, 축 4개 PI Read, Bulk Pending→Active→동일 주기 Snapshot→Release 전부 PASS입니다.

상세 프레임과 수치는 [신규 캡처 분석 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md:10)에 정리했습니다.

TEST UI에는 다음을 반영했습니다.

- Read Position은 None/ACS 지원, Motion은 안전하게 None만 허용
- 고정 15초 대신 이동 거리와 동역학 기반 15~600초 timeout 적용
- PowerOff 후 반드시 Read Status로 `PowerOn=False` 확인
- PI/Bulk 및 Drive Read 캡처 순서 번호와 안내 추가

관련 코드는 [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:420), [MainWindow.xaml.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:2706)에 있습니다.

검증 결과:

- WPF Debug/Release Rebuild PASS
- 자동시험 Debug/Release 각각 `148/148 PASS`
- Debug 실행 파일 startup smoke PASS
- 문서 로컬 링크 및 `git diff --check` PASS

남은 핵심은 `0x2047 GroupEnable`이 `ErrorId=-6`을 반환한 뒤 약 2초 후 실제로 Locked 상태가 되는 PLC ACK/state timing 불일치입니다. UI에서 성공으로 숨기면 안 되고 PLC 계약을 수정해야 합니다. ACS 재캡처, 진짜 Buffered chaining, fault/soak 시험도 남았습니다.

이번 요청에서는 커밋하지 않았습니다. 사용자 변경인 `Classes.lcb`, 캡처 TXT/API_TEST 파일은 수정하지 않았습니다.

> 이제 어떤거 테스트 진행하면 되냐?

<details><summary>2 previous messages</summary>

> 현재 확보된 캡처에서 이미 통과한 항목은 제외하고, 실제로 판정이 남아 있는 항목만 우선순위와 조작 순서까지 다시 추려보겠습니다. 특히 `0x2047 -6`, ACS, 진짜 Buffered queue, fault/soak를 서로 섞지 않고 별도 캡처로 나누겠습니다.
>
> 지금 바로 할 수 있는 테스트는 네 가지로 압축됩니다: `GroupEnable -6 타이밍`, `None/ACS 위치 비교`, `긴 이동 timeout 회귀`, `SDO general-inline 4-byte 및 실패 후 재사용`입니다. 반면 진짜 Buffered chaining과 100회 Bulk soak는 현재 UI가 명령을 직렬화하므로 수동 테스트보다 전용 자동 시퀀스를 먼저 추가하는 편이 정확합니다.
</details>

지금은 아래 4개만 테스트하면 됩니다. 기존 `01~03`, `05b`, `08b`, `10`, `11`은 반복할 필요 없습니다.

| 순서 | 테스트 방법 | 합격 기준 | 파일명 |
|---:|---|---|---|
| 1 | Group Motion → Load Group → Coordinate `None` → Read Position → `ACS` → Read Position | `0x2051` 요청 좌표값이 0/1. 정지 상태에서 X/Y/Z/U 슬롯 순서가 같고 나머지 슬롯은 0. ACS 선택 시 Move 버튼 비활성 | `09_Group_ReadPosition_None_ACS_2051.pcapng/.txt` |
| 2 | EtherCAT/PI에서 Refresh Capabilities → SDO 탭에서 `0x1018:1 UInt32/4` 성공 → `0x6061:0 UInt16/2` 의도적 실패 → 재부팅 없이 `0x6061:0 Int8/1` 실행 | 첫 요청 `Completed/Success`, 두 번째는 terminal Failed/abort, 마지막 요청은 `ResourceBusy` 없이 성공 | `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng/.txt` |
| 3 | 기존 04번에서 검증한 안전 범위로 장거리 Move Linear Absolute 실행 | 로그의 `group monitor started with timeout ... ms`가 15000보다 크고, 15초 false timeout 없이 stable InPosition 완료 | `04b_Group_Absolute_DynamicTimeout_20A4.pcapng/.txt` |
| 4 | 3번 완료 후 Disable → Power Off → Read Status를 `PowerOn=False`까지 반복 | 대기 중 버튼이 `7 Verify Power Off`로 바뀌고 Get Members/Read Position/Home Check가 비활성. 최종 `PowerOn=False` | `08c_PowerOff_UI_Verification_204B_2045.pcapng/.txt` |

4번은 패킷만으로 UI 상태를 증명할 수 없으므로 PowerOff 직후와 최종 확인 후 화면도 각각 캡처해 주세요.

`0x2047 GroupEnable ErrorId=-6`은 이미 충분히 재현됐습니다. 지금 같은 상태로 다시 캡처할 필요는 없고, PLC ACK 계약을 수정한 다음 재시험해야 합니다. Read Status에서 Locked/Standby가 확인되지 않으면 3번 이동 테스트는 중단하세요.

현재 UI로 정확히 시험할 수 없는 항목도 있습니다.

- 진짜 Buffered chaining: A 완료 전에 B를 보내는 전용 연속 송신 UI 필요
- stop-first race: `0x2085`와 `0x7D22` 경합용 자동 시퀀스 필요
- Bulk 24개/100회 soak, stale BootId/MapRevision, double-release: 자동화 harness 필요

위 네 가지 캡처가 끝나면 Buffered/race/Bulk 자동 테스트 UI를 구현하는 순서가 맞습니다. 남은 전체 항목은 [캡처 분석 문서](/C:/work/Elmo/Elmo_Master/docs/architecture/SIGMATEK_PHASE1_PHASE2_LIVE_CAPTURE_ANALYSIS_2026-07-23.md:175)에 정리돼 있습니다.

> 니가 요청한 테스트 완료했다. 데이터 검증해봐
>
> User context
>
> Attachments:
> - 09_Group_ReadPosition_None_ACS_2051.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/09_Group_ReadPosition_None_ACS_2051.pcapng`
> - 09_Group_ReadPosition_None_ACS_2051.txt: `./test/packet_capture/SIGMATEK_API_Analyze/09_Group_ReadPosition_None_ACS_2051.txt`
> - 12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`
> - 12_SDO_GeneralInline_4Byte_FailureRecovery.txt: `./test/packet_capture/SIGMATEK_API_Analyze/12_SDO_GeneralInline_4Byte_FailureRecovery.txt`
> - 04b_Group_Absolute_DynamicTimeout_20A4.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/04b_Group_Absolute_DynamicTimeout_20A4.pcapng`
> - 08c_PowerOff_UI_Verification_204B_2045.pcapng: `./test/packet_capture/SIGMATEK_API_Analyze/08c_PowerOff_UI_Verification_204B_2045.pcapng`
> - 08c_PowerOff_UI_Verification_204B_2045.txt: `./test/packet_capture/SIGMATEK_API_Analyze/08c_PowerOff_UI_Verification_204B_2045.txt`

<details><summary>1 previous message</summary>

> 네 개 테스트를 서로 분리해서 검증하겠습니다. 패킷의 command ID·request/response 필드·시간 순서와 앱 로그를 교차 대조하고, UI 상태처럼 pcap만으로 증명할 수 없는 부분은 별도로 표시하겠습니다. 먼저 `09/04b`, `12 SDO`, `08c`를 병렬 분석하겠습니다.
</details>

검증 결과는 `3건 PASS, 1건 재시험 필요`입니다.

| 테스트 | 판정 | 핵심 결과 |
|---|---|---|
| 09 None/ACS | 재시험 필요 | `0x2051`이 없고 `0x2045 Read Status`만 2회 실행됨 |
| 12 SDO recovery | PASS | 4-byte 성공 → 의도적 실패 → 재부팅 없이 1-byte 성공 |
| 04b Dynamic timeout | PASS | 기존 15초를 넘겨 약 20.15초 후 정상 완료 |
| 08c PowerOff | PASS | PowerOff 후 `PowerOn=False` 최종 확인 성공 |

### 09 — 잘못된 버튼이 실행됨

[09 캡처](/C:/work/Elmo/Elmo_Master/test/packet_capture/SIGMATEK_API_Analyze/09_Group_ReadPosition_None_ACS_2051.pcapng)에는 `0x2051 Read Position`이 한 건도 없습니다.

- frame 13/14: `0x2045 Read Group Status`
- frame 22/23: `0x2045 Read Group Status`
- 두 상태 모두 `0x40060000`, Error 0
- TXT도 `Read Group Status` 두 번으로 기록됨

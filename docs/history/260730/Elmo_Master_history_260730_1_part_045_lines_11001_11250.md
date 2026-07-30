> <details><summary>Edited 10 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+2 -2)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+4 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
>
> </details>
>
> 문서의 현재 WPF 기준도 `109/109`로 갱신했고 `git diff --check`는 통과했습니다. Axis Reset SDK는 지금 typed 결과/timeout/cancel/response-loss 증거까지 포함해 구현 중이며, 완료되면 제가 API 계약과 WPF 적용을 다시 검토한 뒤 전체 Debug/Release 회귀를 돌립니다.
>
> SDK 구현을 기다리는 동안 겹치지 않는 WPF 적용을 먼저 진행합니다. Reset 버튼은 ACK만 표시하지 않고 새 compound API를 호출해 `AxisErrorId=0` 연속 3회를 확인하며, 성공/실패 모두 “명령 전송 가능성·ACK·poll 수”를 화면에 남기도록 바꿉니다. DS402 Fault 해제 증명으로 표기하지는 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisPowerOnRecoveryIntegrationTests.cs` (+67 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs` (+95 -6)
>
> </details>
>
> Axis Reset SDK의 8개 신규 계약을 포함한 Debug 전체가 `733/733 PASS`했습니다. 지금 Release 전체 확인이 끝나는 대로 WPF 새 Reset 회귀를 빌드·실행하고, 최종 수치와 하드웨어 검증 경계를 문서에 고정하겠습니다.
>
> Release SDK도 `733/733 PASS`, 새 WPF Reset 단독 회귀도 PASS했습니다. 이제 WPF 전체 110건을 마지막으로 돌립니다. 여기서 통과하면 구현 근거는 PC/fake-RPC까지 완료이고, 실제 PLC에서는 `0x2024` 1회와 뒤따르는 `0x2028`의 `AxisErrorId=0` 연속 관측만 별도로 확인하면 됩니다.
>
> <details><summary>Created a file, edited 12 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+4 -4)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+3 -3)
> - Wrote `./LMC_Library/README.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+2 -2)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+6 -6)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+4 -4)
> - Wrote `./docs/architecture/LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+6 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+5 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+4 -0)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+74 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+5 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+5 -0)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> Axis Reset의 “1회 전송 + 연속 3회 AxisErrorId=0 확인” 구현과 WPF 연동은 완료됐고, 전체 WPF 회귀도 110/110 통과했습니다. 지금 변경분의 최종 정적 검증을 닫은 뒤, LASAL IDE 구조 작업과 독립적으로 진행할 수 있는 다음 API 항목을 바로 이어서 정리하겠습니다.
>
> 다음 단계는 두 갈래로 병렬 점검하겠습니다. 하나는 현재 WPF에 남아 있는 Axis PowerOff/Stop 완료 판정을 SDK로 옮길 최소 범위, 다른 하나는 DS402 Fault와 드라이브 오류코드(0x603F)를 “관측만” 하는 진단 API의 실제 LASAL/프로토콜 변경 범위입니다. CREVIS는 IDE 구조가 먼저라 이 점검과 충돌하지 않습니다.
>
> 현재 소스 기준으로는 `0x2028`의 `StatusWord`가 실제 DS402 상태워드가 아니라 항상 0인 예약 필드입니다. 따라서 다음 구현에서 이를 Fault 해제 증거로 재사용하면 틀립니다. Axis Stop/PowerOff 쪽은 각각 `Standstill` 또는 `PowerOn=false + Standstill`의 연속 3회 확인만 완료 조건으로 쓸 수 있습니다.
>
> DS402 진단 쪽은 예상보다 범위가 작습니다. 기존 D5 general-inline SDO Read가 이미 `0x6041:0`을 읽고 있으므로 Fault bit는 C# 파생 속성으로 추가할 수 있고, `0x603F:0`도 같은 read-only 경로를 재사용할 수 있습니다. 새 opcode, capability, LASAL Network/IDE 구조 변경 없이 SDK와 GUI부터 구현 가능합니다. 이 항목을 다음 구현으로 선택합니다.
>
> <details><summary>Created a file, edited 10 files</summary>
>
> - Wrote `./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md` (+75 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+8 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+4 -2)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+4 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+5 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+8 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+5 -1)
> - Wrote `./LMC_Library/README.md` (+4 -0)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+5 -0)
>
> </details>
>
> DS402 진단 구현과 동시에, 다음 safety API도 독립적으로 진행합니다. Axis PowerOff는 LASAL 변경 없이 `0x2023(enable=0)` 1회와 `PowerOn=false + Standstill + 정상 Axis status` 3회 연속 확인을 SDK compound로 묶을 수 있습니다. 다만 WPF 버튼은 현재 “송신 후 검증 중에도 Stop/PowerOff 허용” 구조를 보존해야 하므로 이번에는 SDK만 추가하고 GUI 교체는 하지 않습니다.
>
> <details><summary>Created a file, edited 4 files</summary>
>
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+73 -0)
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+9 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+10 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+0 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+7 -0)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -0)
>
> </details>
>
> 현재 병렬 구현 상태입니다.
>
> - DS402: `0x6041` Fault bit와 별도 `0x603F` one-ticket API/GUI를 작성 중입니다.
> - Axis PowerOff: 정확히 1회 송신하고 정상 `PowerOn=false + Standstill`을 3회 연속 확인하는 SDK facade를 작성 중입니다.
> - 둘 다 새 LASAL 명령이나 Network 변경은 없습니다.
> - WPF PowerOff 버튼은 기존 안전 선점 구조를 유지하며 이번에 compound로 바꾸지 않습니다.
>
> 코드가 합쳐지는 즉시 SDK Debug/Release 전체 회귀, WPF Release 전체 smoke, LASAL SourceOnly 정적 계약까지 한 번에 다시 닫겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs` (+1 -1)
>
> </details>
>
> <details><summary>Edited 4 files</summary>
>
> - Wrote `./LMC_Library/LMC_API/API_DEVELOPMENT_GUIDE.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+2 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+1 -1)
>
> </details>
>
> 두 구현이 합쳐졌고 SDK 최종 전체 회귀는 Debug/Release 각각 752/752까지 올라와 통과했습니다. WPF 쪽에서 `0x603F`가 2바이트인데 외부 D5 추적 preflight 인자가 1로 들어간 한 곳을 검토 중 발견해 2로 바로잡았습니다. 이 보정까지 포함해 제가 전체 빌드와 smoke를 다시 실행합니다.
>
> <details><summary>Edited 13 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/AUTOMATED_TESTS_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/DESIGN.md` (+1 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/README.md` (+1 -1)
> - Wrote `./LMC_Library/README.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_POWER_ON_ACCEPTED_ONCE_RECOVERY_2026-07-29.md` (+1 -1)
> - Wrote `./docs/architecture/AXIS_RESET_STABLE_ERROR_CLEARANCE_2026-07-29.md` (+2 -3)
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+3 -3)
> - Wrote `./docs/architecture/LMC_DIAGNOSTICS_REMAINING_IMPLEMENTATION_PLAN_2026-07-21.md` (+2 -2)
> - Wrote `./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md` (+9 -0)
> - Wrote `./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md` (+7 -1)
> - Wrote `./LMC_Library/LasalApiWpfTestApp/API_MAPPING.md` (+1 -1)
>
> </details>
>
> PC 쪽 최종 검증은 모두 닫혔습니다: SDK 752/752, WPF 110/110, LASAL SourceOnly PASS입니다. 마지막으로 마스터 LASAL 프로젝트가 왜 아직 사용자 Save/Rebuild를 요구하는지 full static gate를 다시 확인해, 빌드 요청 항목을 정확히 넘기겠습니다.
</details>

반영 완료했습니다.

- Same-peer takeover 소스를 마스터의 [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st)와 [TCPIPServer.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPIPServer/TCPIPServer.st)에 반영했습니다.
- 실제 SDO `0x6041:0` bit 3 기반 `HasDs402Fault`를 추가했습니다.
- `GetDriveErrorCode[Async]`가 `0x603F:0 UInt16/2-byte`를 정확히 한 번 읽도록 [LmcAxisDriveReads.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxisDriveReads.cs)에 구현했습니다.
- WPF에 `3 Get Drive Error Code` 버튼과 결과 표시를 추가했습니다: [MainWindow.ReadOnlyApi.cs](./LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs)
- SDK에 `PowerOffAndWaitForStableStateAsync`를 추가했습니다. `0x2023(enable=false)` 1회 후 `IsSuccess && PowerOn=false && Standstill=true`를 3회 연속 확인합니다: [LmcAxisPowerStateWait.cs](./LMC_Library/LMC_API_Delivery/src/LmcAxisPowerStateWait.cs)
- WPF PowerOff 버튼은 검증 중에도 새 Stop/PowerOff를 허용하는 기존 구조를 유지하기 위해 아직 compound API로 교체하지 않았습니다.

검증 결과:

- SDK Debug/Release: 각각 `752/752 PASS`
- WPF Release 전체 smoke: `110/110 PASS`
- LASAL SourceOnly static: PASS
- `git diff --check`: PASS
- 실제 PLC·드라이브 시험은 아직 수행하지 않았습니다.

지금 사용자 작업이 필요합니다.

1. 마스터 LASAL 프로젝트를 열어 `Save → Rebuild → Link`를 수행해 주세요.
2. `Classes.lcb`가 새 `Class\TCPIPServer\TCPIPServer.st`를 등록하는지 확인해야 합니다. 현재 full static은 이전 `_TCPIPServer_RT` 등록이 남아 있어 의도대로 FAIL합니다.
3. `TCPIPServer::SetSocketParameter`, `TCPMotionInterface::CyWork`, `Response`, `MsgPaser`의 `Find in Implementation`을 확인하고 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`이 없는지 봐야 합니다.
4. 빌드 성공 후 테스트 폴더로 복사하면 됩니다.

실축 Reset 시험 순서는 다음이 적절합니다.

1. `Read Drive Status`로 `AxisErrorId`, 실제 `0x6041`의 `DS402Fault` 기록
2. `Get Drive Error Code`로 `0x603F` 기록
3. Reset 실행
4. Reset 뒤 두 항목을 다시 읽어 비교
5. 패킷에서 `0x2024` 1회와 후속 `0x2028`, 별도 `0x7E50/0x7E03`을 확인

`AxisErrorId==0`, DS402 Fault=false, `0x603F==0`은 서로 다른 관측입니다. 하나만으로 전체 Fault 해제를 판정하면 안 됩니다.

관련 설계는 [DS402 진단 문서](./docs/architecture/DRIVE_DS402_FAULT_ERROR_DIAGNOSTICS_2026-07-29.md)와 [PowerOff facade 문서](./docs/architecture/AXIS_POWER_OFF_STABLE_STATE_2026-07-29.md)에 정리했습니다.

커밋·스테이징·테스트 폴더 복사는 하지 않았습니다. 기존 staged history 파일의 trailing whitespace 때문에 `git diff --cached --check`는 여전히 실패합니다.

<details><summary>33 previous messages</summary>

> 다음 구현은 현재 상태를 다시 기준으로 잡겠습니다. CREVIS T2는 LASAL IDE 구조 생성 전에는 실제 handler를 넣을 수 없으므로, 그 대기와 충돌하지 않는 축 안전 경로를 우선 진행합니다. 우선 PowerOff SDK facade를 WPF의 기존 “송신/선점 가능한 검증” 구조에 안전하게 연결할 split-phase 방식과 Axis Stop의 입력·완료 계약을 병렬로 점검하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 다음 개발 단위는 두 갈래를 동시에 점검 중입니다. WPF의 PowerOff 선점 구조를 유지하면서 새 SDK 안정상태 확인을 붙이는 방법과, Axis Stop의 입력·ACK·완료 계약을 현재 소스 기준으로 대조한 뒤 구현 가능한 쪽부터 바로 반영하겠습니다. LASAL IDE 구조가 필요한 CREVIS T2는 별도 대기 항목으로 유지합니다.
>
> 현재 확인된 차이는 명확합니다. WPF는 이미 Stop 입력에서 감속도 양수·jerk 0 이상을 강제하지만, public SDK의 `Stop/StopAsync`와 LASAL `0x2022` 처리에는 같은 의미 검증이 없습니다. 반면 PowerOff는 SDK에 exact-once 완료 계약이 생겼지만 WPF는 아직 구형 raw send + 자체 polling이라 증거 모델이 이원화되어 있습니다. 두 분석 결과를 받은 뒤 계약 근거가 더 확실한 쪽을 먼저 구현하겠습니다.
>
> 구현 범위를 확정했습니다.
>
> - Axis Stop: WPF/PMAS 근거에 맞춰 `deceleration > 0`, `jerk >= 0`을 이 프로젝트의 명시적 로컬 계약으로 만들고, C# 송신 전과 LASAL 수신 양쪽에서 잘못된 값을 차단합니다. ACK는 정지 완료가 아니므로 완료 facade는 다음 단계로 분리합니다.
> - Axis PowerOff: `Begin`은 0x2023 1회와 ACK까지만, `Resume`은 0x2028 상태 확인만 수행하도록 나눕니다. 이렇게 해야 WPF 송신 잠금을 polling 동안 잡지 않아 새 Stop/PowerOff가 계속 선점할 수 있습니다.
>
> 두 변경은 서로 다른 파일군에서 병행 구현 중입니다.
>
> Axis Stop의 1차 코드는 이미 들어왔고 현재 교차검증 중입니다. 잘못된 `deceleration/jerk`는 SDK frame 생성 전에 예외가 나고, LASAL에서는 형식은 맞지만 의미가 잘못된 요청을 `-7`로 거부하도록 맞췄습니다. 정적 검증 스크립트와 public API zero-wire 회귀까지 보강하고 있습니다. PowerOff 분할 API는 기존 status gate를 정확히 분리하는 작업이라 조금 더 시간이 필요합니다.
>
> Axis Stop 쪽은 public `Stop`과 `StopAsync`가 잘못된 입력에서 실제 소켓 요청을 0건으로 유지하는 fake-server 회귀까지 추가했습니다. 현재 전체 빌드는 PowerOff 파일을 병행 편집하는 중간 상태라 일시적으로 깨져 있으며, 구현 완료 후 한 번에 전체 테스트를 다시 돌릴 예정입니다. 이는 확정된 실패가 아니라 동시 편집 중인 상태입니다.
>
> PowerOff SDK 골격은 이제 저장됐습니다. 새 continuation은 connection/session/axis와 결합되고, 새 PowerOff가 수락되면 이전 continuation을 `superseded`로 만들어 재사용을 막습니다. timeout·cancel·status 실패 뒤에는 수락된 ACK를 유지해 `Resume`이 0x2023을 재전송하지 않도록 했습니다. 지금 helper 연결과 컴파일을 닫는 중이며, 이후 WPF를 이 분할 경로로 교체합니다.
>
> 독립 Release SDK 검증도 끝났고 현재 `758/758 PASS`입니다. 여기에는 기존 752개에 Axis Stop 검증 2개와 PowerOff 분할 계약 4개가 추가된 수치가 포함됩니다. WPF도 이제 실제로 `Begin`을 송신 구간에서, `Resume`을 선점 가능한 monitor 구간에서 호출하도록 교체됐습니다. 남은 것은 WPF smoke와 독립 코드리뷰에서 나온 경계 보완입니다.
>
> WPF 변경의 핵심 순서도 확인했습니다. PowerOff ACK가 수락된 뒤 exact continuation을 먼저 보존하고, 기존 motion/PowerOn recovery 기록을 유지한 다음 송신 잠금을 해제합니다. 상태 확인이 끝나도 exact continuation·3회 안정 표본·최종 identity가 모두 맞아야만 recovery journal을 해제합니다. 테스트가 끝나는 동안에는 이 순서에 race가 없는지 별도 리뷰도 진행 중입니다.
>
> 독립 리뷰에서 실제 race 1건을 잡았습니다. 같은 axis 객체에서 PowerOff `Begin` 두 개가 동시에 들어오면 ACK 송신 순서와 continuation 등록 순서가 뒤집혀, 더 오래된 명령이 최신 pending으로 남을 수 있었습니다. 아직 배포·커밋 전이라 지금 바로 수정 중입니다. Mutation gate를 ACK뿐 아니라 continuation 등록까지 유지하고, 이 역전 상황을 강제로 재현하는 회귀 테스트를 추가하겠습니다.
>
> 두 번째 안전 경계도 발견했습니다. PowerOff 상태 확인이 timeout/cancel로 끊긴 뒤 pending PowerOn의 “안전한 PowerOff 표본” 카운터만 남아, 서로 다른 두 번의 monitor 구간 표본이 합산될 수 있었습니다. 이 경우 수락된 PowerOn guard를 너무 일찍 해제할 수 있으므로 P1으로 처리합니다. 각 Resume 시작과 실패 경계에서 pending PowerOn proof도 함께 초기화하고, 0x2023이 재전송되지 않는 회귀 테스트를 추가하도록 수정 중입니다.
>
> 독립 리뷰 결과는 방금 확인한 두 P1 외 추가 P0~P2가 없었습니다. 두 경계 수정과 소유권·동시 Resume 회귀를 마무리하는 동안, 다음 개발 항목인 Axis Stop exact-once 완료 facade도 별도 파일군에서 병행 착수했습니다. Stop은 `0x2022` 1회 수락 뒤 `0x2028`의 성공한 Standstill을 3회 연속 확인하는 구조로 만들며, LASAL IDE 파일은 건드리지 않습니다.
>
> 두 P1 수정 후 Debug SDK 전체가 `762/762 PASS`했습니다. 추가된 4개 회귀는 동시 Begin wire/publication 순서, 실패 epoch proof 초기화, 같은 continuation 동시 Resume의 두 번째 zero-wire 거부, reconnect 후 stale token zero-wire 거부를 고정합니다. 현재 Release·WPF 전체 회귀와 수정본 재리뷰를 진행 중이고, Stop stable-standstill facade는 별도 작업으로 계속 구현 중입니다.
>
> PowerOff 분할 통합은 이제 완료됐고 독립 재리뷰도 승인됐습니다. 최종 현재 검증은 SDK Debug/Release `762/762`, WPF Release smoke `111/111 PASS`입니다. 스테이징·커밋·테스트 폴더 복사·LASAL IDE 작업은 하지 않았습니다. 이제 병행 중인 Axis Stop exact-once 완료 facade를 주 작업으로 전환하고, 끝나면 전체 숫자와 문서를 한 번에 다시 맞추겠습니다.
>
> Axis Stop 완료 facade의 코드와 11개 계약 테스트까지 들어왔습니다. 현재 고정한 범위는 0x2022 정확히 1회, 성공 ACK 뒤 0x2028만 polling, 3회 Standstill, 중간 비정지·AxisError 시 연속 카운터 초기화, reject/pre-wire cancel/response loss/send-priority 선점의 typed evidence입니다. 이 facade는 아직 WPF 버튼에 붙이지 않았고, 현재 WPF의 Stop 선점 구조는 그대로 유지합니다.
>
> Axis Stop facade의 SDK Debug/Release 전체가 각각 `773/773 PASS`했습니다. 11개 신규 테스트가 모두 통과했고 scoped diff 검사도 현재 정상입니다. 전용 설계 문서와 API 안내를 마무리하는 동안 이 신규 facade를 독립 안전 리뷰에 넘겼습니다. WPF는 아직 기존 raw Stop + 선점 가능한 monitor를 유지하므로, 이번 결과를 곧바로 GUI 동작 변경이나 실제 축 정지 증거로 보지는 않습니다.
>
> Stop facade 독립 리뷰에서 invalid ACK 처리 문제로 보인 항목은 소스를 대조해 false positive로 확인했습니다. `IsSuccess`가 이미 frame validity를 포함하고, malformed ACK는 parser에서 예외가 되어 `OutcomeUncertain` submission evidence로 남습니다. 대신 보강 가치가 있는 “Stop ACK 수락 후 status 응답 유실” 회귀를 추가해, 이 경우에도 Accepted 증거를 보존하고 Stop을 재전송하지 않는지 고정하고 있습니다.
>
> Stop facade 자체는 `774/774 PASS`로 완료됐지만, 리뷰가 남긴 P2급 테스트 빈틈 하나도 닫기로 했습니다. Stop ACK 수락 뒤 in-flight 0x2028 응답이 더 최신 safety generation 때문에 폐기될 때 `Accepted` evidence를 유지하고 0x2022를 재전송하지 않는 경우입니다. 구현 문제는 발견되지 않았고, 이 회귀 1개를 추가한 뒤 최종 전체 숫자를 확정하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 외부 수정분의 same-peer takeover 반영은 끝났고, 이어 만든 Stop 안정정지 API를 독립 검토한 결과 deadline과 전송 직전 취소 경계에서 실제 결함 2건이 확인됐습니다. 이 두 경로를 먼저 고치고 무응답 회귀 테스트까지 통과시킨 뒤 전체 검증하겠습니다.
>
> 외부 시험본과 마스터의 두 LASAL 파일은 바이트 단위로는 같지 않습니다. 마스터 쪽에 이후 개발분이 섞였을 가능성이 있어, 그대로 덮어쓰지 않고 takeover 핵심 변경이 빠짐없이 포함됐는지 구조 diff로 대조하고 있습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./docs/architecture/ELMO_MASTER_CURRENT_ARCHITECTURE_AND_RELEASE_STATUS_2026-07-16.md` (+2 -1)
>
> </details>
>
> same-peer 핵심 소스와 네트워크 설정은 외부 PLC 시험본과 의미상 일치합니다. 현재 확인된 유일한 반영 차이는 마스터의 `Classes.lcb`가 예전 `_TCPIPServer_RT` 등록을 가진 IDE 생성 메타데이터라는 점이며, SourceOnly 계약은 PASS했고 full static은 이 항목 하나에서 의도대로 멈춥니다.
>
> Stop wait 결함 수정은 transport를 끊지 않고 단순 타임아웃 반환하면 다음 RPC가 이전 응답과 섞이므로, deadline 도달 시 해당 연결을 Faulted로 전환하는 방식으로 잡고 있습니다. 이렇게 해야 정확히 1회 전송 증거와 스트림 정합성을 동시에 지킬 수 있습니다.
>
> 독립 대조 결과, 외부 시험 로직은 마스터의 `.st`, `.lcp`, `Comm_Network.lcn`, 생성 테이블 소스까지 정확히 들어왔습니다. 다만 `Classes.lcb`뿐 아니라 `Networks.lcb`, `.lba`, export, 루트 `.lcb`도 7/28 생성본이라서 시험 폴더의 생성물을 수동 복사하면 현재 마스터 개발분을 잃을 수 있습니다. 마스터 LASAL에서 Save/Generate 후 Rebuild/Link로 재생성해야 합니다.

>
> Source: automatic
> Status: completed
>
> 오류 코드 의미는 확인됐습니다. 지금은 실제 프로젝트의 `Group Enable → LockProfile` 순서와 MotionLib의 호출 전제조건을 대조해서, 재시험 순서만으로 해결되는지 아니면 PLC 상태 처리를 고쳐야 하는지 확정하고 있습니다.
>
> 공식 MotionLib 도움말을 추가 대조해 보니, 이 프로젝트의 LASAL 쪽은 일반 PMAS 캡처와 초기화 순서가 다를 가능성이 있습니다. 그래서 지금은 “SetKin 먼저”라고 단정하지 않고, `RobotOn` 완료 상태와 `LockProfile`의 공식 전제조건을 기준으로 원인을 좁히고 있습니다.
>
> 원인은 패킷이 아니라 **Robot 활성화 완료 및 Profile Lock 순서 문제**입니다.
>
> 확인된 동작은 다음과 같습니다.
>
> - `ErrorId=17`은 `_LMCPROF_INSERT_ERROR`, 즉 profile buffer에 명령을 넣지 못했다는 뜻입니다. [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:750)
> - `ErrorId=1003`은 `_LMCROBOT_POWERON_ERROR`입니다. 공식 MotionLib 설명은 “LockProfile 시 로봇이 켜져 있지 않거나 축들이 제한 시간 안에 Power On되지 않음”입니다. [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:796)
> - `Group Enable PASS`는 `RobotOn()` 호출 접수 성공일 뿐, `_ROBOT_ACTIVE` 전환 완료가 아닙니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1548)
> - `Read Group Position PASS`도 위치를 읽었다는 의미일 뿐, 로봇 활성화나 Profile Lock 성공을 뜻하지 않습니다.
>
> 현재 실행 순서는 다음과 같았습니다.
>
> ```text
> Group Enable
> → Read Position
> → MoveLinear
> → Set Identity Kinematics
> ```
>
> `MoveLinear` 실행 당시 `LockProfile`이 안 된 상태여서 17이 발생했고, 이후 `Set Identity Kinematics`가 호출한 `LockProfile()`은 로봇이 ACTIVE가 아니어서 1003을 반환한 것입니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1881)
>
> 재시험 순서는 다음이 맞습니다.
>
> 1. Group Disable 후 필요하면 Group Reset
> 2. Group Enable
> 3. LASAL Watch에서 아래 상태가 될 때까지 확인
>    - `RobotMode = _ROBOT_ACTIVE`
>    - `RobotStatus.RobotIsOn = 1`
>    - `RobotStatus.PowerIsOn = 1`
>    - `RobotStatus.IsReferenced = 1`
> 4. Set Identity Kinematics
> 5. `ProfileStatus.bProfileLocked = 1` 확인
> 6. 현재 표시 위치와 가까운 절대 목표로 Move Linear
>
> 3번에서 `_ROBOT_ACTIVE`가 되지 않는다면 `_LMCAxis1..4` 각각의 `PowerOn`, `IsReferenced`, `Error` 상태를 확인해야 합니다. 특히 이전에 2~4축이 Reference되지 않았던 문제가 있었으므로 그쪽이 가장 의심되지만, 현재 로그만으로 어느 축인지까지는 확정할 수 없습니다.
>
> 추가로 현재 `_LMCRobotBase1`은 `_JERK_PROFILE`이므로 화면의 `Jerk=0`은 다음 시험에서 `ErrorId=14`를 발생시킬 수 있습니다. 설정된 `JMax` 이내의 양수 Jerk를 사용해야 합니다. [Motion_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:1655)
>
> 그리고 구현상 결함도 있습니다.
>
> - 예제 프로그램이 `Group Enable PASS`를 최종 ACTIVE 완료처럼 표시합니다.
> - SetKin/Enable/Move 버튼이 준비 순서와 관계없이 모두 활성화됩니다. [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:2467)
> - PLC의 `GroupKinematicReady`도 MoveLinear 전제조건으로 검사되지 않습니다.
> - 현재 README의 `SetKin → Group Enable` 안내도 SIGMATEK `LockProfile` 의미와 맞지 않아 수정이 필요합니다.
>
> 따라서 재시험 전에 WPF와 PLC 쪽에 ACTIVE/Lock 준비 상태 검사를 추가하는 것이 맞습니다.
>
> 요청하신 의미대로 역할을 분리하겠습니다. `Group PowerOn/PowerOff`는 로봇 축의 서보 전원 전환, `Group Enable/Disable`은 `LockProfile/UnlockProfile`로 바꾸고, 기존 Set Identity Kinematics는 축 매핑 검증·등록만 하도록 API·PLC·예제·문서를 함께 맞추겠습니다.
>
> 현재 역할 분리는 코드에 반영 중이고, 같은 실수를 반복하지 않도록 상태 의미도 함께 고칩니다. `PowerOn` 응답은 시작 접수, `Enable` 성공은 실제 Profile Lock 성공으로 구분하며, Group 상태의 `Disabled/Standby` 비트도 Unlock/Lock 상태와 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+56 -6)
>
> </details>
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 요청한 역할 분리를 그대로 적용 중입니다. `Group Power`는 실제 서보 전원, `Enable/Disable`은 프로파일 Lock/Unlock으로 고정하고, 전원 완료는 ACK가 아니라 PLC 상태 비트로 확인하도록 PC·LASAL·예제·문서를 함께 맞추겠습니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+23 -9)
>
> </details>
>
> 핵심 코드 반영은 끝났고, 정적 계약 검사도 새 의미로 갱신해 통과했습니다. 지금은 WPF 호출 순서와 문서에 옛 의미(`Enable=PowerOn`)가 남지 않았는지 교차 확인하고 전체 PC 테스트를 다시 돌리는 단계입니다.
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
>
> </details>
>
> <details><summary>Edited 15 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResults.cs` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+15 -9)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+57 -25)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/API_MAPPING.md` (+2 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+7 -5)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+4 -3)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/USER_MANUAL_PREPARATION_2026-07-13.md` (+11 -8)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+2 -2)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_method_study.md` (+2 -1)
>
> </details>
>
> <details><summary>Edited 17 files</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/sample/BasicUsage.cs` (+3 -5)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_COMMAND_QUEUE_RTWORK_DESIGN_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResults.cs` (+14 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcResults.cs` (+4 -1)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+14 -9)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml` (+1 -1)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs` (+20 -6)
> - Wrote `./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs` (+2 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+2 -0)
> - Wrote `./LMC_Library/LMC_API_Delivery/README.md` (+2 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+1 -1)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt` (+5 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md` (+3 -2)
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/LASAL_CYWORK_ONLY_TCP_EXECUTION_DESIGN_2026-07-13.md` (+1 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_coding_rules.md` (+2 -1)
> - Wrote `./docs/architecture/SIGMATEK_LASAL_programming_error_prevention_guide.md` (+2 -0)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/DESIGN.md` (+2 -2)
> - Wrote `./LMC_Library/Codex_LASAL_WPF/README.md` (+2 -1)
>
> </details>
>
> 독립 리뷰에서 두 가지 상태 전이 구멍을 찾아 추가로 막았습니다. PLC는 이동 중 `GroupDisable/UnlockProfile`을 `-6`으로 거부하고, WPF는 PowerOff ACK 뒤 `PowerOn=False` 확인 전 재로드·재명령으로 상태를 우회할 수 없게 했습니다. Group Reset도 더 이상 실제로 유지되는 power/lock 상태를 UI에서 임의로 지우지 않습니다.
>
> <details><summary>Edited 2 files</summary>
>
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+14 -17)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+0 -3)
> - Wrote `./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st` (+30 -21)
> - Wrote `./LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1` (+34 -6)
>
> </details>
>
> <details><summary>Edited a file</summary>
>
> - Wrote `./LMC_Library/LMC_API_Delivery/docs/API_DEVELOPMENT_BACKLOG_2026-07-10.md` (+2 -2)
>
> </details>
</details>

소스 구현은 완료했습니다. 실제 장비 완료 판정만 남았습니다.

- `0x204A/0x204B`: `GroupPowerOn/Off` → `RobotOn/RobotOff`
- `0x2047/0x2048`: `GroupEnable/Disable` → `LockProfile/UnlockProfile`
- `0x00040000`: Group Power Ready
- 이동 중 `GroupDisable`은 PLC에서 `-6`으로 거부
- PowerOff ACK 뒤 `IsPowerOn=False` 확인 전 WPF 재명령 차단
- RT Task 재도입 없음
- `Classes.lcb`와 `GroupKinematicReady` metadata 정합성 확인

주요 파일:

- [LmcGroup.cs](./LMC_Library/LMC_API_Delivery/src/LmcGroup.cs:98)
- [LmcProtocol.cs](./LMC_Library/LMC_API_Delivery/src/LmcProtocol.cs:117)
- [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1543)
- [MainWindow.xaml.cs](./LMC_Library/Codex_LASAL_WPF/PmasApiWpfTestApp/MainWindow.xaml.cs:846)
- [Group API 구현 문서](./LMC_Library/LMC_API_Delivery/docs/GROUP_API_IMPLEMENTATION_2026-07-14.md:1)

검증 결과:

- PC 테스트: `46/46 PASS`
- LASAL source/network/metadata 정적 계약: PASS
- WPF Debug 및 Release 빌드: PASS
- LASAL 7-bit ASCII 검사: PASS
- `git diff --check`: PASS

실기 시험 순서는 다음입니다.

`PowerOn → IsPowerOn=True → SetKin → Enable/Lock → IsStandby=True → Move → Stop/InPosition → Disable/Unlock → PowerOff → IsPowerOn=False`

남은 것은 LASAL IDE Rebuild/Link, Find in Implementation 검사, PLC 다운로드와 위 순서의 실제 동작 확인입니다. 이번 요청에서는 커밋하지 않았습니다.

> 요렇게 에러나는데 무슨일이지?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-8c779b73-1f1a-493e-920b-6e20ebe209c7.png`

컴파일 오류는 아닙니다. 화면상 실제 실패 지점은 `OutputPersistence: Writing ...\Class\Classes.lcb failed`라서, 우선 `Classes.lcb`의 읽기 전용 속성·권한·파일 잠금과 같은 시각의 `Lasal2.log`를 확인하겠습니다.

> 이거 Lock Profile이 활성화가 안돼는데? 그리고 ReadStatus 하니까 이렇게 에러 스테이트로 빠지는데?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-9abf682d-99d5-4424-8d8b-84f3f078512e.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-23247e1e-13ac-4c23-885e-6d58ba557b8f.png`
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-f476d998-2913-4226-ac50-53908a2e752e.png`

<details><summary>3 previous messages</summary>

> 화면 근거만 보면 `Read Status`가 오류를 만든 것이 아니라, 앞서 실행한 `Group Power On(RobotOn)`이 4축 reference 조건을 만족하지 못해 `ROB REF ERROR`로 끝난 상태입니다. 그래서 `PowerOn=False`이고 Lock Profile 버튼이 비활성인 것이 맞습니다. 다만 WPF가 `ErrorId=0`으로 표시한 것은 오류 전달 누락 가능성이 있어 LASAL 상태 판정 코드까지 확인하겠습니다.
>
> Context compaction
>
> Source: automatic
> Status: completed
>
> 원인은 거의 확정됐습니다. 지금은 축 reference 실패가 본체 원인이고, WPF에 `GroupErrorId=0`으로 보이는 것은 별도의 상태 응답 누락입니다. 마지막으로 오류 해제 순서와 현재 API의 reset 한계를 확인하고 있습니다.
</details>

원인은 확정적입니다. `Read Status`가 오류를 만든 게 아니라, 앞서 실행한 `Group Power On → RobotOn(_ACTIVE)`이 축 Reference 조건을 만족하지 못해 실패한 것입니다.

- LASAL 화면의 `ROB REF ERROR`는 `_LMCROBOT_REF_ERROR = 1000`입니다.
- 정의도 “축 하나 이상이 Reference되지 않았거나 PowerOn 중 Reference가 실패함”입니다. [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:793)
- 그래서 `RobotIsOn=False`이고 `State=0x40010000`은 현재 `Disabled/Unlocked` 상태입니다.
- Power Ready가 아니므로 `Set Identity`가 막히고, Identity 설정도 안 됐으므로 `Enable (Lock Profile)`도 비활성인 것이 정상입니다.

`Profile Status=24`는 오류 원인이 아닙니다. `NoPreRtWork + NoPostRtWork`를 의미하며 별도 RT Trigger를 연결하지 않았다는 표시입니다. RT Task를 사용하지 않는 현재 구성에서 예상되는 상태입니다. [types.h](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h:825)

현재 네 축 모두 `AbsEncoder=1`로 설정되어 있습니다. 따라서 아래 항목을 축별로 확인해야 합니다.

1. `_LMCAxis1..4.AxisStatus.IsReferenced`
2. 각 축의 `_LMCABSEncoder1.PosOffsetOk`
3. `PosOffset`, `PosCRC32` 유효 여부

[Motion_Network.lcn](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn:118)

이전에 1축만 Reference되고 2~4축은 안 됐던 현상까지 고려하면, 2~4축의 Absolute Encoder 위치 Offset/CRC가 저장되지 않았거나 유효하지 않은 가능성이 큽니다. 이 부분은 현재 화면만으로 확정할 수 없으니 위 값을 확인해야 합니다.

복구 순서는 다음과 같습니다.

1. 네 축 모두 승인된 Reference/Homing 또는 SetPosition 절차로 `IsReferenced=1`을 만든다.
2. 축 오류가 남았다면 Axis Reset/QuitError를 실행한다.
3. `Group Power On`을 다시 실행한다.
4. `Read Status`로 `PowerOn=True`가 될 때까지 확인한다.
5. `Set Identity`
6. `Enable (Lock Profile)`
7. `Read Status`로 Locked/Standby 확인

주의할 점은 현재 `Group Reset`은 `AxQuitError(AxisNo:=0)`만 호출하므로 Reference 상태를 만들어 주지 않습니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:1658)

그리고 별도의 진단 결함도 있습니다. LASAL에는 `ROB REF ERROR`가 있는데 WPF가 `GroupErrorId=0`으로 표시하고 있습니다. 현재 `GroupReadStatus()`는 `robotState == _ROBOT_ERROR`일 때만 `ReadProfileError()`를 호출합니다. [TCPMotionInterface.st](./Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st:2497)

실기에서 확인할 것은 `GroupReadStatus()` 내부의 `robotState` 값입니다.

- `robotState=3(_ROBOT_ERROR)`인데 WPF가 계속 0이면 이전 `Classes.lcb` 저장 실패 때문에 최신 바이너리가 다운로드되지 않았을 가능성이 큽니다.
- `robotState`가 3이 아니라면 현재 조건식이 실제 상태를 놓치는 것이므로, `ReadProfileError()`를 매 상태 조회마다 읽어 nonzero `ErrorNo`를 우선 전달하도록 수정해야 합니다.

즉, 현재 동작 실패의 본체 원인은 축 Reference이고, `GroupErrorId=0` 표시는 PLC 상태 응답 구현의 별도 수정 대상입니다.

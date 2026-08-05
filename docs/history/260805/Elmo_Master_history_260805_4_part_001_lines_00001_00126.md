# Verify Axis1 status recovery

> [10:07:22.155] Recovered a durable accepted Axis Power Off ACK for _LMCAxis1. Reconnect to the exact identity and run status-only verification; 0x2023 will not be replayed.
> [10:07:22.180] Executable identity: Path=C:\work\Elmo\Elmo_Master\LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\bin\Debug\LasalMotionControlApiExample.exe, Version=0.9.1.0, BuildUtc=2026-08-03 01:03:06 UTC, Feature=CREVIS_TOPOLOGY_AXIS1_UI24_SDO_WRITE_LIVE_AXIS_QUAL_V5.
> [10:07:22.180] Example ready. Connect, load _LMCAxis1, and start with Read Status. Connect automatically attempts only the read-only CREVIS topology load; no motion or mutation command is sent automatically.
> [10:07:41.970] Connect started.
> [10:07:41.996] Connection state Disconnected -> Connecting
> [10:07:41.998] Connection state changed to Connecting invalidated session-bound Axis Power continuations. Exact-identity recovery remains status-only; no 0x2023 replay is automatic.
> [10:07:42.013] Connection state Connecting -> Connected
> [10:07:42.014] RPC initialized. Callback endpoint=10.10.150.13:5000, EventMask=0xFFFFFFFF
> [10:07:42.091] CREVIS / EtherCAT topology auto-load failed; the RPC connection remains available and Reload can be retried: EtherCAT Topology is not advertised by the connected PLC.
> [10:07:42.103] Connect PASS.
> [10:07:47.732] Refresh Home/EnDat ID30 Capabilities started.
> [10:07:47.744] Refresh Home/EnDat ID30 Capabilities PASS.
> [10:08:07.397] Refresh Diagnostics Capabilities started.
> [10:08:07.401] Refresh Diagnostics Capabilities PASS.
> [10:08:10.332] Refresh Diagnostics Capabilities started.
> [10:08:10.335] Refresh Diagnostics Capabilities PASS.
> [10:08:10.936] Refresh Diagnostics Capabilities started.
> [10:08:10.960] Refresh Diagnostics Capabilities PASS.
> [10:08:11.668] Read EtherCAT Health started.
> [10:08:11.678] Read EtherCAT Health PASS.
> [10:08:12.532] Load EtherCAT Topology started.
> [10:08:12.611] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:08:27.111] Refresh Home/EnDat ID30 Capabilities started.
> [10:08:27.122] Refresh Home/EnDat ID30 Capabilities PASS.
> [10:09:22.722] Load Group started.
> [10:09:22.741] Group loaded. Name=_LMCRobotBase1, Ref=256
> [10:09:22.741] Load Group PASS.
> [10:09:25.792] Load Axis started.
> [10:09:25.809] Axis loaded. Name=_LMCAxis1, Ref=1
> [10:09:25.810] Load Axis PASS.
> [10:09:28.758] Refresh Home/EnDat ID30 Capabilities started.
> [10:09:28.776] Refresh Home/EnDat ID30 Capabilities PASS.
> [10:09:29.833] Read Home Status started.
> [10:09:29.883] Read Home Status FAILED: Read Home Status failed. ErrorId=0, AxisErrorId=1.
> [10:09:33.891] Refresh Diagnostics Capabilities started.
> [10:09:33.899] Refresh Diagnostics Capabilities PASS.
> [10:09:34.359] Read EtherCAT Health started.
> [10:09:34.367] Read EtherCAT Health PASS.
> [10:09:34.812] Load EtherCAT Topology started.
> [10:09:34.910] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:09:40.186] Refresh Home/EnDat ID30 Capabilities started.
> [10:09:40.199] Refresh Home/EnDat ID30 Capabilities PASS.
> [10:09:44.290] Refresh Home/EnDat ID30 Capabilities started.
> [10:09:44.300] Refresh Home/EnDat ID30 Capabilities PASS.
> [10:10:06.514] Refresh Diagnostics Capabilities started.
> [10:10:06.531] Refresh Diagnostics Capabilities PASS.
> [10:10:07.993] Read EtherCAT Health started.
> [10:10:08.017] Read EtherCAT Health PASS.
> [10:10:08.464] Load EtherCAT Topology started.
> [10:10:08.549] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:10:09.502] Load EtherCAT Topology started.
> [10:10:09.575] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:10:10.163] Load EtherCAT Topology started.
> [10:10:10.243] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:13:50.145] Refresh Diagnostics Capabilities started.
> [10:13:50.156] Refresh Diagnostics Capabilities PASS.
> [10:13:51.838] Read EtherCAT Health started.
> [10:13:51.848] Read EtherCAT Health PASS.
> [10:13:53.263] Load EtherCAT Topology started.
> [10:13:53.354] Load EtherCAT Topology FAILED: EtherCAT Topology is not advertised by the connected PLC.
> [10:14:28.048] Refresh Home/EnDat ID30 Capabilities started.
> [10:14:28.073] Refresh Home/EnDat ID30 Capabilities PASS.
>
> 멀티턴 리셋이 안되는데?
>
> User context
>
> Images:
> - `C:/Users/dreiv/AppData/Local/Temp/codex-clipboard-29d3b716-1103-4217-bfa6-694ed97891b9.png`

<details><summary>3 previous messages</summary>

> 화면상 원인은 이미 꽤 명확합니다. 현재 `TW(20)`은 “멀티턴 위치 초기화”가 아니라 EnDat ID30 오류/경고 리셋이고, 별도 `TW(19)` 멀티턴 위치 리셋은 이 빌드에서 미구현·차단된 상태입니다. 버튼이 비활성화된 정확한 조건과 PLC가 광고한 capability까지 현재 소스로 교차확인하겠습니다.
>
> 현재 로그의 `PASS`는 기능 실행 성공이 아니라 capability 조회 RPC가 정상 응답했다는 뜻으로 보입니다. 실제 버튼은 별도 capability 비트가 켜져야 활성화되는데, 화면 상태는 그 비트가 꺼져 있음을 나타냅니다. 이제 비트 값과 PLC gate를 확인 중입니다.
>
> 추가로 분리해야 할 차단이 하나 더 있습니다. 시작 시 복구된 Axis Power-Off ACK가 아직 `Read Status`로 종료되지 않았습니다. 다만 그것을 먼저 해소해도, 현재 `TW20` capability 자체가 OFF라 리셋 버튼은 그대로 활성화되지 않습니다. 두 문제는 별개입니다.
</details>

안 됩니다. 현재는 “리셋 실패”가 아니라 **리셋 명령이 한 번도 전송되지 않은 상태**입니다.

- 화면의 `TW[20] / 0x3204:0x14`는 EnDat ID30 오류·경고 리셋입니다. 멀티턴 위치 초기화인 `TW[19] / 0x3204:0x13`은 미구현·금지입니다. [MainWindow.xaml](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml:1851)
- 현재 LASAL 소스는 `LMC_DIAG_D5_TW20_TEST_RESET_ENABLED FALSE`입니다. 따라서 capability bit 18이 없고 Arm 버튼도 비활성화됩니다. [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:309) [MainWindow.MaintenanceActions.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs:238)
- `Refresh ... Capabilities PASS`는 조회 성공일 뿐 `TW20 지원=True`가 아닙니다.
- 현재 디스크 소스는 Topology bit 14를 항상 광고하지만 연결 PLC는 미광고입니다. 새 EXE는 PC 빌드만 증명하며, PLC에는 현재 소스와 다른 이미지가 실행 중입니다. 단순 재다운로드해도 TW20 gate가 `FALSE`라 리셋은 계속 닫힙니다. [capability 생성 코드](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:4091)

또한 `Read Home Status FAILED: ErrorId=0, AxisErrorId=1`은 통신 실패가 아닙니다. 읽기는 성공했지만 축에 `HwError`가 있다는 뜻이며, 이 구현은 DS402 Fault가 남으면 `AxError=1`로 설정합니다. 이것만으로 엔코더 멀티턴 오류라고 특정할 수는 없습니다. [LmcResults.cs](/C:/work/Elmo/Elmo_Master/LMC_Library/LMC_API_Delivery/src/LmcResults.cs:83) [ECAT_DS402Base.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/ECAT_DS402Base/ECAT_DS402Base.st:490)

추가 구현 결함도 있습니다. TW20을 활성화하더라도 WPF의 Power-Off 사전검사가 `AxisErrorId=0`까지 요구합니다. 반면 PLC는 오류를 지우는 명령이므로 기존 Fault를 허용하고 motor-off만 검사합니다. 현재 `AxisErrorId=1`이면 Arm 단계에서 다시 막힙니다. [WPF 사전검사](/C:/work/Elmo/Elmo_Master/LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.xaml.cs:5747) [PLC TW20 검사](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:744)

지금은 다음 순서가 맞습니다.

1. `0x2023`을 재전송하지 말고 전용 `Resume Power Off Verification (No 0x2023 Replay)` 경로만 사용합니다.
2. Diagnostics의 `Bits`, `Build`, `BootId`, `MapRevision`을 기록합니다.
3. 쓰기 없이 `0x6061`, 실제 DS402 `0x6041`, `0x603F`와 Elmo/EAS 오류 코드, 엔코더 모델·socket을 확인합니다.
4. 목표를 구분해야 합니다.
   - 오류/경고 해제: TW20 경로의 WPF 사전검사와 지속 motor-off interlock을 수정한 뒤 gate 활성화가 필요합니다. 현재 조합 기준 기대 capability는 `0x0004633F`입니다.
   - 절대 멀티턴 위치 초기화: TW19 전용 기능을 별도로 구현해야 합니다. TW20으로는 안 됩니다.

원하는 것이 **TW20 오류 해제**인지 **TW19 위치 초기화**인지 정해 주시면 그 경로만 수정하겠습니다.

> 아직 home이랑 멀티턴 리셋은 구현 안된거야?

<details><summary>1 previous message</summary>

> 네. 사용자 관점에서는 둘 다 아직 사용할 수 없습니다. 다만 “코드 골격은 있지만 비활성”인 것과 “아예 미구현”이 섞여 있으니, 현재 소스 기준으로 세 경로를 정확히 나눠 확인하겠습니다.
</details>

네. 정확히는 다음 상태입니다.

| 기능 | 현재 상태 |
|---|---|
| MMC Home `0x7D13` | parser만 있고 실행 함수가 비어 있음. 미구현·비활성 |
| DS402 Home `0x7D15/0x7D16` | 상태 머신 코드는 있으나 gate가 `FALSE`. 사용·실기검증 불가 |
| TW20 `0x3204:0x14` | ID30 오류/경고 리셋 코드는 있으나 gate가 `FALSE` |
| TW19 `0x3204:0x13` | 실제 멀티턴 위치 초기화. 완전 미구현·명시적 차단 |

MMC Home은 `ProcessAxisReference()`가 빈 함수입니다. [LMCControlCommandService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st:2984)

DS402 Home과 TW20은 코드 골격이 있어도 둘 다 현재 `FALSE`입니다. [LMCDiagnosticsService.st](/C:/work/Elmo/Elmo_Master/Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st:309)

따라서 사용자 관점에서는 **Home도 아직 못 쓰고, 멀티턴 위치 리셋도 아직 구현되지 않은 상태**가 맞습니다. 화면의 버튼은 완성된 기능이 아니라 dormant 시험 UI입니다.

게다가 현재 PLC는 디스크의 최신 소스조차 반영되지 않은 상태지만, 최신 소스를 그대로 다운로드해도 Home/TW20 gate가 꺼져 있으므로 활성화되지는 않습니다.

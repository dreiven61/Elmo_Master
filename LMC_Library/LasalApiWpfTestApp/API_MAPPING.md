# LASAL Motion Control Example API Mapping

이 예제는 현재 PLC에서 활성화된 protocol 경로만 화면에 노출한다.

| 화면 | Command ID | 실제 API |
|---|---:|---|
| Connect | `0x8080`, `0x405C` | `LMCConnection.RpcInitConnectionAsync` |
| Close | `0x405D` | `LMCConnection.CloseConnectionAsync` |
| Load Axis | `0x103C`, `0x202B` | `LMCSingleAxis.CreateAsync` |
| Power On / Off | `0x2023` | `PowerOnAsync`, `PowerOffAsync` |
| Reset | `0x2024` | `ResetAsync` |
| Stop | `0x2022` | `StopAsync` |
| Read Status | `0x2028` | `ReadStatusResultAsync` |
| Read Position | `0x202E` | `GetActualPositionResultAsync` |
| Move Absolute | `0x209F` | `MoveAbsoluteExAsync` |
| Move Relative | `0x20A0` | `MoveRelativeExAsync` |
| Move Velocity | `0x20A2` | `MoveVelocityExAsync` |
| Load Group | `0x1042` | `LMCGroupAxis.CreateAsync` |
| Get Members | `0x20D2` | `GetGroupMembersInfoResultAsync` |
| Group Power On | `0x204A` | `GroupPowerOnAsync` |
| Group Power Off | `0x204B` | `GroupPowerOffAsync` |
| Group Enable (Lock Profile) | `0x2047` | `GroupEnableAsync` |
| Group Disable (Unlock Profile) | `0x2048` | `GroupDisableAsync` |
| Group Read Status | `0x2045` | `GroupReadStatusResultAsync` |
| Group Reset | `0x2049` | `GroupResetAsync` |
| Group Stop | `0x2085` | `GroupStopAsync` |
| Group Read Position | `0x2051` | `GroupReadActualPositionAsync` |
| Move Linear Absolute | `0x20A4` | `MoveLinearAbsoluteExAsync` |
| Set Identity Kinematics | `0x20E7` | `SetKinTransformCartesian4AxisAsync` |

`Connect`는 TCP 연결, RPC session 초기화, UDP callback listener 개방과 callback
등록을 한 번에 수행한다. callback은 typed motion event가 아니라 raw diagnostic
payload로만 표시한다.

Motion 인자는 PC 프로그램이 `engineering value × PLC UNIT`으로 변환하거나
이미 변환된 raw 값으로 제공한 LASAL DINT다. DLL 내부에서는 단위 변환을 수행하지 않는다. 예제의 UNIT
콤보는 이 caller-side 변환만 선택하며 wire protocol은 바꾸지 않는다. 기본
`mm`는 `LMC_Units.MM=10000`이고, `None / raw DINT`는 이미 변환된 정수를
그대로 전송한다. Encoder `ExUnits=8388608`은 PC UNIT 선택 대상이 아니다.

현재 PLC group motion은 static X/Y/Z/U identity 범위다. Move Linear는
`Coordinate=None`, `ExactStop`/`ContinuousDirect`, `Aborting`/`Buffered`만
노출한다. `MoveCircle`은 공개 API와 승인된 DINT wire 계약이 없어 노출하지 않는다.

Group 준비는 `GroupPowerOnAsync -> GroupReadStatusResultAsync로 Power Ready/ACTIVE
확인 -> SetKinTransformCartesian4AxisAsync -> GroupEnableAsync(Lock) ->
GroupReadStatusResultAsync로 Enabled/Locked Standby 확인 -> Move`다. Enable ACK만으로
lock 완료를 판정하지 않는다.
종료는 `GroupDisableAsync(Unlock) -> GroupPowerOffAsync ->
GroupReadStatusResultAsync로 PowerOn=False 확인` 순서다.

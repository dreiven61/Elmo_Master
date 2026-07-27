# LASAL Motion Control Example API Mapping

이 예제는 현재 PLC에서 활성화된 motion 경로와 SDK의 capability-gated diagnostics
경로를 화면에 노출한다. Diagnostics 버튼은 PLC가 해당 capability를 광고해야만
활성화된다.

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
| Diagnostics Capabilities | `0x7E00` | `LMCConnection.Diagnostics.GetCapabilitiesAsync` |
| PI Catalog Info / Chunk | `0x7E01`, `0x7E02` | `GetSignalCatalogAsync` |
| EtherCAT Health | `0x7E10` | `ReadEtherCATHealthAsync` |
| PI Read | `0x7E20` | `ReadPIAsync` |
| Bulk Configure / Status / Snapshot / Release | `0x7E30`~`0x7E33` | `ConfigureBulkAsync`, `ReadBulkStatusAsync`, `ReadBulkAsync`, `ReleaseBulkAsync` |
| Recorder Configure / Start / Trigger / Stop | `0x7E40`~`0x7E43` | `ConfigureRecorderAsync`, `StartRecorderAsync`, `TriggerRecorderAsync`, `StopRecorderAsync` |
| Recorder Status / Header / Chunk | `0x7E44`~`0x7E46` | `GetRecorderStatusAsync`, `GetRecorderHeaderAsync`, `ReadRecorderChunkAsync`, `DownloadRecorderAsync` |
| Recorder Buffer / Configuration Release | `0x7E47`, `0x7E48` | `ReleaseRecorderBufferAsync`, `ReleaseRecorderAsync` |
| Recorder Reconnect Adoption | `0x7E49` | `AdoptRecorderAsync` |
| PI Write ticket submit | `0x7E21` | `SubmitPIWriteAsync` |
| SDO Read / Write ticket submit | `0x7E50` | `SubmitSdoAsync` |
| Extended SDO result chunk | `0x7E51` | `ReadSdoResultChunkAsync` |
| Diagnostics ticket status / cancel | `0x7E03`, `0x7E04` | `GetOperationStatusAsync`, `CancelOperationAsync` |

`Connect`는 TCP 연결, RPC session 초기화, UDP callback listener 개방과 callback
등록을 한 번에 수행한다. callback은 typed motion event가 아니라 raw diagnostic
payload로만 표시한다.

Diagnostics 탭은 먼저 `0x7E00` capability를 읽고 PLC가 광고한 bit에 해당하는
버튼만 활성화한다. Catalog/PI는 read-only이고, Bulk와 Recorder configuration은
선택 signal의 Catalog access flag를 다시 검사한다. Recorder download는 header와
chunk identity/sequence/CRC를 SDK가 검증한 뒤 immutable `LMCRecorderData`로
조립하며, WPF는 이 데이터만 plot/CSV에 사용한다.

Edge/Window/Mask trigger와 Ring/Double 동작은 `0x7E40 Configure` payload와
capability로 선택한다. Window는 payload의 `TriggerValue`를 lower bound,
`TriggerMask`를 upper bound로 사용한다. `0x7E42 Trigger`는 locally configured
non-Manual D4 identity에만 사용한다.
reconnect resume은 기존 handle을 재사용하지 않고
`DiagnosticsBootId + RecordId + BufferId`로 `0x7E49 Adopt`한 새 identity를 사용한다.
Adopt한 resource는 Status 또는 Header로 configuration metadata를 복구한 뒤
buffer(`0x7E47`)와 configuration(`0x7E48`) 순서로 해제한다.

SDO의 4/8/12-byte 결과는 `0x7E03 GetOperationStatus` response에 inline으로
포함된다. 더 큰 Read는 `ExtendedSdoResultChunk` capability가 필요하고 terminal
success 뒤 `0x7E51 ReadSdoResultChunk`를 반복해 전체 결과를 조립한다. PI/SDO Write
버튼은 PLC capability와 SDK allowlist가 모두 허용하지 않으면 실행되지 않는다.

현재 `0x7E50` SDO Write 인프라는 `OperationFlags=1`, exact 36-byte request,
`ValueType=Int32(4)`, `DataLength=4`만 받으며 ticket의 `OperationKind`는
`SDOWrite(3)`이다. 승인 후보는 Gold UI[24] `0x2F00:24`지만 사용자 drive program에서
미사용인지와 적용 축이 확정되지 않았다. 따라서 PLC와 SDK의 global gate 및 UI[24]
axis 1~4 per-axis gate는 모두 `FALSE`, SDK approved target 목록은 empty, capability
bit 9는 0이고 GUI Write도 비활성이다. 배포 설정에서는 확인한 한 축의 gate만 활성화한다. 임의
SDO address, DS402 motion/control object, PI Write `0x7E21`, extended result
`0x7E51`은 허용하지 않는다.

승인 후에도 GUI submit은 exact SDK target 선택, PLC capability 재확인, 선택 축의
`PowerOn=False`/`Standstill=True`와 actual position 3회 안정, 명시적 확인 및 D5
quarantine 등록을 요구한다. Write outcome이 불명확하면 read recovery proof로 자동
해제하지 않는다. Write가 `Completed/Success`여도 동일 Slave/Index/SubIndex/Type/Length를
원 Write의 owner/current session, `DiagnosticsBootId`, `MapRevision`에 묶인 guarded Read로
다시 읽어 exact 4-byte 값이 일치할 때까지 mutation과 Close를 차단한다. identity mismatch는
`0x7E50` Read submit 전에 거부하고 interlock을 유지한다. 이 pending readback은 현재
프로세스 메모리에만 있으므로 강제 종료/전원 손실 뒤 복구 journal은 아직
없다. 현재 Write 경로는 PC 자동 테스트 대상일 뿐 LASAL build, PLC download, 실축 및
EtherCAT mailbox로 검증되지 않았다.

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

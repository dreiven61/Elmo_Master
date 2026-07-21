# Elmo PMAS Packet Capture Version2 API Mapping

`PacketCaptureWindow`는 `LasalApiWpfTestApp`의 6개 탭과 실행 흐름을 기준으로 만든
Elmo MMCLibDotNET 직접 호출 화면이다. 화면의 기능 이름은 비교 가능하게 유지하지만
LASAL DINT wire protocol을 흉내 내지 않는다.

## Motion

| 화면 | PMAS/MMCLib 호출 | 패킷/계약 차이 |
|---|---|---|
| Connect | `MMCConnection.ConnectRPC` | MMCLib RPC 연결과 callback 등록 |
| Close | `MMCConnection.CloseConnection` | Close는 Stop이 아님 |
| Load Axis | `new MMCSingleAxis(name, handle)` | wrapper 내부에서 axis name lookup과 DriveID read가 발생 |
| Read Status | `MMCSingleAxis.ReadStatus` | PMAS status bit이며 LASAL mask로 해석하지 않음 |
| Read Position | `MMCSingleAxis.GetActualPosition` | `double` controller/user unit 반환 |
| Power On / Off | `MMCSingleAxis.PowerOn`, `PowerOff` | direct axis command |
| Reset / Stop | `MMCSingleAxis.Reset`, `Stop` | Stop은 deceleration/jerk 사용 |
| Move Absolute / Relative / Velocity | `MoveAbsoluteEx`, `MoveRelativeEx`, `MoveVelocityEx` | 입력 `double`을 그대로 전달; `x10000` 변환 없음 |
| Load Group | `new MMCGroupAxis(name, handle)` | 컨트롤러 group name lookup |
| Get Members | `GetGroupMembersInfo` + X/Y/Z/U `MMCSingleAxis` 생성 | controller가 반환한 정확히 4개 member name/ref를 검증하고 이후 명령용 wrapper를 cache; group query 1회와 member별 name/DriveID query 발생 |
| Group Power On / Off | X/Y/Z/U 각각 `PowerOn/PowerOff` 후 `ReadStatus` | 1:1 group power API가 없어 클릭 1회에 4개 power command와 4개 member status read 발생 |
| Group Status / Enable / Disable / Reset / Stop | `GroupReadStatus`, `GroupEnable`, `GroupDisable`, `GroupReset`, `GroupStop` | PMAS group status/command |
| Group Position | `GroupReadActualPosition` | `double[16]` 반환 |
| Home Check | X/Y/Z/U 각각 `ReadStatus` | raw PMAS state만 표시; LASAL `IsReferenced`와 같다고 판정하지 않음 |
| Set Identity | X/Y/Z/U `ReadStatus` 후 `SetKinTransformCartesian` | Get Members로 검증한 wrapper만 사용; raw status는 표시하되 PMAS Home 판정으로 사용하지 않음 |
| Move Linear | `MoveLinearAbsoluteEx` | PMAS enum과 `double[16]` 사용 |

## Diagnostics, PI, Bulk, Recorder, SDO

| 화면 | PMAS/MMCLib 호출 | 직접 대응 여부 |
|---|---|---|
| Refresh Capabilities | local MMCLib mapping 표시 | LASAL runtime capability bitmask 없음; controller packet 없음 |
| Read EtherCAT Health | `MMCNetwork.GetCommDiagnosticsEx` | master/path count와 nonzero port error만 표시; per-axis identity/Online/AL/DS402로 오해하지 않음 |
| Load PI Catalog | axis별 `MMCSingleAxis` 생성 후 axis/index별 `GetPIVarInfo` | 축 1개/index N개면 name/DriveID 2회 + PI info N회; controller metadata type을 사용하며 `VAR_TYPE` 미지원 type은 direct read/write/bulk 차단 |
| Read Selected PI | 선택 entry별 `ReadPIVar` | 순차 synchronous read이며 same-cycle snapshot이 아님 |
| Configure Bulk | `MMCPIBulkRead.AddEntry` | 각 entry의 PI metadata를 `GetPIVarInfo`로 조회하므로 controller packet 발생; LASAL Bulk ID/lease 없음 |
| Bulk Status | local object 상태 | controller packet 없음 |
| Read Snapshot | `MMCPIBulkRead.Upload` + `GetEntry` | 첫 Upload는 `ConfigurePIBulkRead` 후 `PerformPIBulkRead`; 이후 Upload는 perform, `GetEntry`는 local decode |
| Bulk Release | local object 제거 | controller packet 없음 |
| Recorder Configure | `BeginRecordingEx` 인자를 PC에 저장 | controller packet 없음 |
| Use Selected PI | checked PI row를 native `uiRv`와 signal bit mask로 변환 | local helper; controller packet 없음; raw uiRv/uiRp 편집 유지 |
| Recorder Start | `MMCConnection.BeginRecordingEx` | internal/PI variable용 Ex command; 시작 직전 화면의 native uiRv/uiRp를 다시 읽음 |
| Recorder Status / Stop | `GetRecordingStatus`, `StopRecording` | `uiSr` phase/ready buffer를 decode하고 Stop 뒤 status/header cache를 무효화 |
| Recorder Header / Download | `GetRecordingDataHeader`, `GetRecordingData` | ready buffer, global header `Rl`, selected buffer와 `[From..To]` 범위를 검증; BootId/RecordId/BufferId/CRC 계약은 없음 |
| Trigger Now / Adopt | local 비지원 설명 | 검증된 1:1 wrapper 없음; controller packet 없음 |
| Recorder Release | PC 메모리/설정 제거 | LASAL release command 없음; controller packet 없음 |
| Submit SDO | loaded axis의 `UploadSDO` 또는 `DownloadSDO` | synchronous typed call; data length는 type에서 1/2/4 byte로 파생 |
| Refresh/Download/Cancel Ticket | 마지막 local result 표시 | PMAS operation ticket 없음; 표시/저장 byte는 typed value의 host-endian 재인코딩이며 raw wire payload가 아님 |
| PI Write | 선택 entry의 `WritePIVar` | direct write; LASAL SDK/PLC allowlist 계약은 적용되지 않음 |

## 로그 판독

실제 controller call은 다음 형식으로 기록한다.

```text
CAPTURE #0001 START API=MMC_ReadStatusCmd
CAPTURE #0001 PASS API=MMC_ReadStatusCmd ElapsedMs=1.234
```

로그 timestamp는 millisecond까지 기록한다. `CAPTURE START`는 호출 시도를 뜻하며 실제 packet
송신 증명은 아니다. local validation에서 실패하면 packet이 없을 수 있으므로 Wireshark와 함께 판정한다.

controller packet을 보내지 않는 화면 동작은 다음 prefix를 사용한다.

```text
LOCAL (no controller packet): ...
```

이 로그는 pcap을 만들거나 packet byte를 기록하지 않는다. Wireshark를 별도로 실행해
로그 시각과 `CAPTURE #` 순서를 기준으로 비교한다.

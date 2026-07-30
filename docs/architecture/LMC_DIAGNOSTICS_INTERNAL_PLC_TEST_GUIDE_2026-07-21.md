# LMC EtherCAT Diagnostics 내부 PLC 시험 가이드

- 작성일: 2026-07-21
- 대상: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- PC 시험 앱: `LMC_Library/LasalApiWpfTestApp`
- 범위: D1 Health/Catalog/PI Read, D2 Bulk, D3 Recorder v1,
  D4 single-bank Ring/Trigger, D5 general-inline SDO Read와 gate-off exact SDO Write checkpoint
- 제외: 고객 배포 패키지 갱신, D4 Double bank, PI Write, arbitrary SDO Write, 8/12-byte 및
  extended SDO result, 기존 D6 static/handle facade
- preflight 상태: PC 자동 테스트 Debug/Release 각 277/277와 WPF Debug/Release 별도 output
  build PASS. LASAL SourceOnly 정적 계약은 PASS했다. 새 `TryStartWrite`, `ActiveIsWrite`,
  `WriteBuffer`, `SdoWriteData`, `GetSdoWritePolicyDetail` declaration은 tracked `.st`에만 있고
  `Classes.lcb`에는 아직 없으므로 switch 없는 full static은 현재 의도적으로 FAIL한다.
  사용자가 LASAL IDE에서 declaration 동기화·저장·build한 뒤 full을 다시 통과시켜야 한다.
  legacy fixed-vector D5 시험의 same-cycle timeout을 수정했고 후속 download의 Slave 1~4
  `0x1000:0` UInt32 4-byte 경로는 모두 Completed/Success, 43~54 cycles로 PASS했다.
  과거 BootId 6 general-inline 시험은 Submit 두 건이 ticket 전 `ResourceBusy`로
  실패했지만 callback ordering/release source 수정 뒤
  `10_DriveRead_Axis1to4`에서 수정본 general-inline 1/2-byte 성공 ticket을
  보존했다. `12_SDO_GeneralInline_4Byte_FailureRecovery`는 같은 BootId 8에서
  UInt32/4-byte 성공, TypeMismatch 실패와 이어진 Int8/1-byte 성공을 보존했고
  `ResourceBusy` 재발이 없었다. 전체 fault/timeout/cancel/orphan matrix는 아직 남아 있다.
  `LMCSdoExecutor` constructor를 통한 private state 명시 초기화도
  PLC 재시험 전 P1 조건이다. C78/C81 version mismatch warning은 남아 있다.

현재 source 상태와 실기 판정은 구분한다.

| 단계 | 현재 source 상태 | 이 문서의 PLC 실기 상태 |
|---|---|---|
| D0 | common envelope, capability와 `0x7E00` 구현 | 2026-07-23 `0x213F`, MapRevision `0x957F101E`, BootId 8 live PASS |
| D1 | Health/Catalog/PI Read 활성 | 24-entry Catalog와 축 1..4 actual-position PI happy path PASS; fault matrix 별도 |
| D2 | 최대 24-entry Bulk 활성 | 4-entry Pending->Active, same-cycle snapshot, Release PASS; 24-entry/soak/fault 별도 |
| D3 | single-bank finite/manual Recorder 활성 | 미실시 |
| D4 | single-bank Ring/Trigger 활성, Double source/API/WPF adapter dormant/gate-off | 미실시. Double PLC 실기 미완료 |
| D5 | general-inline SDO Read, callback ordering/release 수정 source 활성 | legacy 4-byte와 수정본 general-inline 1/2/4-byte capture PASS; TypeMismatch 후 같은 BootId recovery PASS; 나머지 fault/timeout/cancel/orphan matrix 대기 |
| D6 | 기존 static/handle facade 후속 설계 | 미구현; Phase 1 D1/D2 기반 PI/Bulk instance facade는 구현 |

따라서 정적 계약과 IDE Build/Link 통과를 실제 PLC 완료로 해석하지 않는다.

## 1. 시험 전 완료 조건

다음 조건이 모두 충족되기 전에는 PLC에 다운로드하지 않는다.

1. LASAL IDE에서 `LMCDiagnosticsService`, `LMCSdoExecutor`, `LMCEcatInputLatch`,
   `LMCRecorderStore`, `TCPMotionInterface`를 Reload Class 후 저장한다.
2. `LMCSdoExecutor`에 class constructor가 있고 `AdapterState/Active*`, `ReadBuffer`, `WriteBuffer`,
   `PublishSequence`, `PublishedResult`를 명시적으로 초기화한다. declaration/`@STD` wiring은
   LASAL IDE가 생성해야 한다.
3. Rebuild와 Link가 0 error다.
4. `LMCDiagnosticsService` tree에 hidden retentive `DiagnosticsBootCounter` server와
   `GetDiagnosticsBootId` method가 보인다.
5. `Find in Implementation` smoke가 정상이고 smoke 시작 이후 `Lasal2.log`에 새
   `CInvalidArgException`이 없다.
6. PC test와 LASAL SourceOnly verifier가 통과한다. full verifier는 LASAL IDE에서
   declaration을 저장하고 Rebuild한 뒤 반드시 다시 통과시킨다.

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunPcTests /p:Configuration=Release /nologo

& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe' `
  'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj' `
  /t:RunLasalNetworkContract /p:Configuration=Release /nologo
```

## 2. 안전 조건

- 첫 시험은 축 power off와 정지 상태에서 수행한다.
- D1~D4는 read/record 경로이며 motion command를 발생시키지 않아야 한다.
- PI Write는 계속 off다. SDO Write는 사용자 drive program에서 예약한 정확한 축의
  `UI[24] 0x2F00:24`만 SDK global+per-axis allowlist와 PLC global+per-axis gate를 함께
  활성화한 뒤 8.4절 순서로만 시험한다. 예약 확인 전에는 bit 9와 GUI submit이 off인 것이 정상이다.
- `ControlWord`, `TargetPosition`, `TargetVelocity`, `TargetTorque` direct write는 금지한다.
- PLC reboot, diagnostics service 재초기화 또는 BootId 변경 뒤 기존 Bulk/Recorder
  handle을 재사용하지 않는다.

## 3. 연결 및 capability

1. PLC에 test build를 다운로드하고 Run 상태로 전환한다.
2. 개발 WPF를 실행한다.
3. PLC TCP endpoint에 연결하고 RPC Init을 완료한다.
4. Diagnostics capability를 읽는다.

정상 retained 경로의 기대값:

```text
DiagnosticsBuild       1
CapabilityBits         0x0000213F
MapRevision            0x957F101E
CatalogEntryCount      24
MaxBulkEntryCount      24
MaxRecorderChannels    24
RecorderBufferCount    1
MaxRecorderSamples     320000
RecorderBytesPerBank   1280000
MaxSdoDataBytes        4
DiagnosticsBootId      nonzero
```

`MaxRecorderSamples=320000`은 1채널일 때의 capability 상한이다. Configure 응답의
`AcceptedCapacity=min(requested, floor(1280000 / (channelCount * 4)))`가 실제 상한이며,
16채널은 20,000 samples, 24채널은 13,333 samples까지다.

BootId가 0이면 D2/D3, D4 Trigger와 D5 SDORead/general-inline/Write bit가 꺼지고 MaxSDO가 0인 것이 정상
fail-closed다. 이 경우 Recorder나 SDO를 강행하지 말고 `DiagnosticsBootCounter`
retentive restore/write/read-back부터 확인한다.

## 4. D1 Health, Catalog, PI Read

1. Health를 연속 조회한다.
2. master state, invalid/missed frame counter, frame time, RT time을 기록한다.
3. Catalog info/chunk를 읽고 24개 entry, map revision, Catalog CRC를 확인한다.
4. 축 1~4마다 아래 6개 PDO signal만 존재하는지 확인한다.
   - SetPosition
   - DigitalOutputs
   - ControlWord
   - ActualPosition
   - DigitalInputs
   - StatusWord
5. 24개 PI를 읽고 같은 snapshot의 cycle counter/timestamp/status를 확인한다.
6. EtherCAT cable 또는 slave fault 시험에서는 값을 정상값으로 오인하지 않고
   stale/offline 상태와 AL/status 정보가 바뀌는지 확인한다.

software axis 5~9는 EtherCAT PDO Catalog에 없어야 한다.

## 5. D2 Bulk Snapshot

1. 먼저 4개 read-only signal로 Bulk를 configure한다.
2. status와 snapshot을 100회 이상 읽는다.
3. 모든 entry가 하나의 cycle counter, timestamp, map revision을 공유하는지 확인한다.
4. 24개 signal로 확대해 같은 검사를 반복한다.
5. 같은 session에서 release하고 다시 configure했을 때 stale config가 재사용되지 않는지
   확인한다.
6. 다른 session, 다른 BootId, 잘못된 map revision으로 접근하면 거부되는지 확인한다.

기록할 값:

- 요청/응답 시간
- snapshot cycle 간격
- entry status 분포
- invalid frame 전후 cycle/timestamp 변화
- configure/release 반복 후 resource leak 여부

## 6. D3 Recorder v1

첫 시험은 작은 capture로 시작한다.

```text
BufferMode          Single
TriggerType         Manual/None
Channels            2~4
SamplePeriodCycles  1
SampleCapacity      1000
```

1. Configure 후 반환된 ConfigId/Revision/BootId/OwnerSessionEpoch를 확인한다.
2. Start 후 status가 Configured -> Armed/Recording -> Completed로 전이하는지 확인한다.
3. Header를 읽고 channel metadata, sample stride, cycle/timestamp 범위를 확인한다.
4. chunk를 sequence 0부터 끝까지 내려받는다.
5. 각 chunk CRC, offset, returned count, `LastChunk`를 검증한다.
6. WPF plot과 CSV를 생성하고 sample 수와 cycle 간격을 대조한다.
7. buffer release 후 같은 identity로 chunk read가 거부되는지 확인한다.
8. 24채널과 divider/capacity 조합으로 확대한다.
9. 현재 bank 상한 시험은 16채널/20,000 samples와 24채널/13,333 samples로 수행한다.
   Configure 응답의 AcceptedCapacity, 1,280,000-byte bank 범위, PLC free RAM과 RT
   jitter를 함께 측정한다. 32채널/31,250 samples는 4,000,000-byte bank로 확장한
   후속 profile이며 현재 test build의 완료 기준이 아니다.

upload 중에는 완료 bank의 header/data hash가 변하면 안 되고 RT task가 TCP 전송 때문에
block되면 안 된다.

## 7. D4 single-bank Ring/Trigger

현재 D4 시험 대상은 하나의 물리 bank를 쓰는 Ring/Trigger뿐이다.
`RecorderBufferCount=1`, BufferId=0, capability bit 5=1과 bit 6=0을 먼저 확인한다.

1. `Ring + Edge`를 Int32 또는 BitField signal에 설정하고 pre-trigger 100,
   post-trigger 899, capacity 1000으로 시작한다.
2. pre-trigger history가 채워진 뒤 threshold를 통과시켜 자동 trigger가 한 번만
   발생하고 `TriggerIndex=100`인지 확인한다.
3. Int32 signal의 `Window`에서 `TriggerValue=lower`, `TriggerMask=upper` 경계를
   각각 통과시켜 조건과 signed 비교가 일치하는지 확인한다.
4. BitField16/32 signal의 `Mask`에서 nonzero mask로 all-set/any-set/all-clear 조건을
   각각 확인한다.
5. non-Manual Ring을 다시 시작하고 `Trigger Now (0x7E42)`로 forced trigger를 발생시킨다.
6. EtherCAT master/slave가 유효하지 않은 cycle에서는 자동 trigger가 발생하지 않고,
   정상 상태 복귀 뒤 새 history로 조건을 다시 평가하는지 확인한다.
7. 각 경우 Header/CSV에서 pre + trigger + post sample 순서, sample count와 cycle 간격을
   확인하고 완료 bank가 upload 중 변하지 않는지 확인한다.

이 시험의 통과는 Double bank를 검증하지 않는다. 두 번째 bank, BufferId=1과 capture/upload
동시 진행은 현재 source에 없다.

## 8. D5 general-inline SDO Read

1. 연결 시 자동 capability summary 또는 `EtherCAT / PI` 탭의 `Refresh Capabilities`에서
   bit 8 `SDORead`, bit 13 `SDOReadGeneralInline`, `MaxSdoDataBytes=4`, nonzero BootId를
   확인한다. general-inline은 bit 8과 bit 13이 모두 있어야 활성이다. SDO 탭에는 이
   capability 갱신 버튼이 없다.
2. legacy 회귀 시험으로 축 1~4 각각에 대해 `0x1000:0`, UInt32, 4-byte,
   timeout 1000 cycles를 Submit한다.
3. 장비 제조사 object dictionary에서 읽기 안전성이 확인된 object를 사용해 1-byte,
   2-byte, 4-byte를 각각 시험한다. ObjectIndex는 `0x0001..0xFFFF`, SubIndex는
   `0..255`가 계약 범위이며 선택한 ValueType과 DataLength가 정확히 일치해야 한다.
   - 1 byte: Bool/Int8/UInt8/BitField8
   - 2 bytes: Int16/UInt16/BitField16
   - 4 bytes: Int32/UInt32/Real32/BitField32
4. `Refresh Ticket`을 terminal까지 반복한다. status는 `Queued/Running`을 거치거나 빠른
   callback이면 첫 조회에서 바로 `Completed`일 수 있다. `Completed/Success`의
   ResultType/ResultLength가 요청한 1/2/4-byte shape와 같은지 확인하고 `Save Result`로
   raw bytes를 보존한다.
   - `TimeoutCycles=1000`인데 첫 terminal이 `SubmitCycle=CompletionCycle`,
     `Expired/TimedOut`, `DetailCode=0x05040000`이면 즉시 FAIL이다.
   - 이 경우 축을 바꾸거나 `Refresh Ticket`을 반복해 통과 처리하지 않는다.
   - 정상 기준은 각 축 `Completed/Success`, ErrorId/DetailCode 0, UInt32,
     ResultLength 4와 실제 inline 4-byte data다.
5. 같은 slave SDO channel이 BUSY일 때 bounded retry 후 Running 또는 Expired가 되는지
   기록한다.
6. offline/start error, 실제 SDO abort, timeout과 actual-length mismatch가 Failed 또는
   Expired로 분류되고 DetailCode가 보존되는지 확인한다.
   별도 유도 timeout은 unsigned `CompletionCycle - SubmitCycle`이 요청 timeout 이상인지
   확인한다.
7. Queued ticket은 Cancelled가 되고 Running ticket cancel은 InvalidState인지 확인한다.
8. Queued/Running 중 TCP session 종료 후 새 session에서 old ticket이 stale 처리되고,
   late callback 결과가 새 ticket에 노출되지 않는지 확인한다.
9. ObjectIndex 0, ValueType/DataLength 불일치와 raw 8/12-byte Read가 계속 거부되는지
   확인한다. 현재 gate-off checkpoint에서는 SDO Write도 거부되어야 한다. SubIndex 0
   이외의 값은 그 자체로 Read 거부 조건이 아니다.
10. callback failure recovery는 같은 BootId와 같은 slave에서 수행한다. drive object의
    실제 길이와 다른 `0x6061:0` UInt16/2처럼 SDK shape 자체는 유효한 요청을 실행해
    terminal Failed 또는 SDO abort를 확인한 뒤, PLC를 재시작하지 않고 올바른 Int8/1을
    즉시 Submit한다. 두 번째 요청이 영구 `ResourceBusy`가 되면 FAIL이다.

callback recovery 수정본의 general-inline 1/2-byte happy path는 성공 캡처를 확보했다.
`12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서 general-inline 4-byte 성공과
의도한 TypeMismatch 뒤 같은 BootId의 Int8/1-byte recovery도 확인했다. 그러나
SDO abort/offline, timeout, queued cancel, disconnect/orphan과 active contention을 포함한
전체 matrix까지 확인하기 전에는 `0x213F`, MaxSDO=4를 production 승인으로
기록하지 않는다.
PC-PLC TCP 캡처에는 EtherCAT mailbox frame이 없을 수 있으므로 terminal packet과 함께
executor callback/PLC trace 또는 별도 EtherCAT 관측 자료를 보존한다.

### 8.1 2026-07-22 legacy fixed-vector Slave 4 통과 기록

`SDO_Test2.pcapng`에서 다음 결과를 확인했다.

| 항목 | 실제 값 | 판정 |
|---|---:|---|
| Capability / BootId | `0x13F`, MaxSDO=4, BootId=5 | PASS |
| 요청 | Slave 4, `0x1000:0`, UInt32, 4 bytes, timeout 1000 | PASS |
| Ticket | 5, Queued, SubmitCycle 92042 | PASS |
| terminal | Completed/Success, Error/Detail 0 | PASS |
| 완료 cycle | 92096, delta 54 cycles | PASS |
| 결과 | `92 01 02 00`, UInt32 `0x00020192` | PASS |
| 반복 조회 | 3회 모두 동일 terminal 결과 | PASS |

이 개별 기록은 Slave 4 happy path만 통과시킨다. Slave 1~3은 8.2절 후속 capture에서
통과했다. abort/offline, timeout, cancel/orphan과 allowlist 거부 시험은 완료되지
않았다. 캡처에 EtherCAT `0x88A4` frame이 없으므로 mailbox wire 자체의 독립 관측
자료도 아니다.

### 8.2 legacy fixed-vector Slave 1~3 통과와 4축 합산 완료 기록

`SDO_Test_Slave123.pcapng`에서 Slave 1~3도 같은 요청을 통과했다.

| Slave | Ticket | SubmitCycle | CompletionCycle | Delta | 결과 |
|---:|---:|---:|---:|---:|---|
| 1 | 6 | 987464 | 987507 | 43 | Completed/Success |
| 2 | 7 | 990944 | 990995 | 51 | Completed/Success |
| 3 | 8 | 993897 | 993940 | 43 | Completed/Success |

모두 ErrorId/Detail 0, UInt32, ResultLength 4와 `92 01 02 00`을 반환했다. 8.1절의
Slave 4까지 합쳐 물리축 1~4 legacy `0x1000:0` UInt32 4-byte SDO Read happy path는
완료다. 이 캡처는 당시 `0x13F` runtime의 fixed-vector 증거이며 현재 `0x213F`
general-inline 1/2/4-byte 범위를 증명하지 않는다. general-inline, 아래 fault 항목과
EtherCAT mailbox frame의 독립 관측은 계속 production qualification으로 남는다.

### 8.3 general-inline ResourceBusy 실패 기록과 수정본 재시험

`SDO_Test_Error.pcapng`의 BootId 6 capability는 세 번 모두 `0x213F`, MaxSDO=4로
정상이다. 그러나 실제 Diagnostics wire의 Submit은 아래 두 건만 있으며 모두 ticket 전
`ErrorId=-32000`, `DetailCode=9 ResourceBusy`로 거부됐다.

| RequestId | 실제 요청 | 판정 |
|---:|---|---|
| 14 | Slave 1, `0x6061:0`, UInt16/2 | FAIL, ResourceBusy |
| 16 | Slave 1, `0x6061:0`, Int8/1 | FAIL, ResourceBusy |

`0x6041:0`과 `0x1018:1`은 이 capture에 없고 ticket/status도 없다. 따라서 사용자가 세
vector를 시도했다는 기록과 별개로 이 파일은 2-byte/4-byte general-inline 실행 결과를
증명하지 않는다. wire DetailCode 9만으로 active/drain slot과 executor non-reusable 중
어느 gate인지 구분할 수도 없다.

실패 당시 source에서 vendor call 뒤 `Running` publish와 owned validation failure의
미회수 결함을 확인했다. 현재 source는 vendor call 전 `Running`, private cleanup 중
`Releasing`, owned completion 소비 후 release, orphan callback release와 unsolicited
hard quarantine을 적용했고 full static 계약을 통과했다. 이후
`10_DriveRead_Axis1to4.pcapng`에서 수정본 general-inline 1/2-byte happy path는
성공했다.

`12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`의 BootId 8에서는 다음 순서가
재부팅 없이 실행됐다.

1. Ticket 13: `0x1018:1` UInt32/4, 17 cycles, Completed/Success,
   result `9A 00 00 00`
2. Ticket 14: `0x6061:0` UInt16/2, 30 cycles, Failed,
   `ErrorId=-32001`, `DetailCode=5 TypeMismatch`
3. Ticket 15: `0x6061:0` Int8/1, 36 cycles, Completed/Success, result `08`

같은 BootId에서 TypeMismatch terminal result 후 executor가 재사용됐고 `ResourceBusy(9)`는
발생하지 않았다. 이로써 general-inline UInt32/4-byte 성공과 한 failure-recovery
vector는 PASS다. SDO abort/offline, timeout, queued cancel, disconnect/orphan, active
contention은 여전히 별도로 시험한다.

### 8.4 SDO Write 활성화 및 첫 시험

이 절은 사용자 drive program에서 시험 축의 `UI[24]`가 미사용이라고 확인한 뒤에만 수행한다.
임의 축 전체를 한 번에 열지 않는다.

1. 첫 시험 축 하나를 정하고 SDK의 global gate와 해당 `UI24 Axis N` gate, PLC의 global gate와
   같은 축의 per-axis gate만 TRUE로 변경한다. 다른 세 축 gate는 FALSE로 유지한다.
2. LASAL IDE에서 `LMCSdoExecutor` constructor declaration과 generated `@STD` call,
   `TryStartWrite` declaration을 동기화한다. constructor implementation은 Active state와
   Read/Write buffer, PublishedResult, PublishSequence를 먼저 초기화하고 마지막에 atomic
   `AdapterState=Idle`, `ret_code=C_OK` 순서로 공개한다. 저장·Rebuild/Link 뒤 변경 class의
   앞/중간/뒤 `Find in Implementation`과 smoke 시작 이후 `%TEMP%\Lasal2.log`의 신규
   `CInvalidArgException=0`을 확인한다.
3. PC API/WPF Debug/Release와 source-only/full static 계약을 다시 실행한다. capability는 bit 8,
   bit 9, bit 13, MaxSDO=4와 nonzero BootId를 광고해야 하며 GUI target 목록에는 승인한 축
   하나의 `0x2F00:24 Int32/4`만 보여야 한다. 이때 verifier에는 선택 축 번호를
   `-ExpectedSdoWriteAxis N`으로 전달한다. 이 옵션은 constructor source와 full-mode
   `Classes.lcb` metadata가 하나라도 없으면 활성화를 거부한다. gate-off baseline은 기본값 `0`이다.
4. drive program/EAS에서 원래 `UI[24]` 값을 기록한다. 축은 PowerOff, DS402 Switch On Disabled,
   WPF `PowerOn=False`, `Standstill=True`, position 3회 동일 상태여야 한다.
   PLC의 DS402 재검사는 async EtherCAT mailbox가 실제 실행되는 순간까지 상태를 고정하는
   hard interlock이 아니라 submit-time precondition이므로 시험 중 다른 조작을 하지 않는다.
5. 먼저 원래 값과 동일한 값을 Write한다. `Arm SDO Write`를 눌러 화면에 고정된
   Slave/Object/Type/value/wire bytes를 대조한다. modal 대화상자는 없으며 이 시점에는 아직
   전송되지 않는다. 요청을 바꾸지 않고 `Confirm & Submit SDO Write`를 다시 눌러 제출한 뒤
   `Queued/Running -> Completed/Success`, Error/Detail 0,
   ResultLength/ResultData 0을 확인한다. GUI는 이 terminal을 전송 성공으로만 표시하고 exact
   readback이 끝날 때까지 mutation과 Close를 계속 차단해야 한다.
6. GUI가 자동 설정한 같은 Slave/Object/SubIndex/Type/Length로 SDO Read한다. 이 Read는 원
   Write의 owner/current session, BootId, MapRevision에 묶여야 하며 identity mismatch면 submit
   없이 interlock을 유지해야 한다. terminal type/length와 4-byte 값이 Write 값과 정확히
   일치할 때만 interlock이 해제되어야 한다.
   mismatch/failure 또는 불명확한 submit outcome이면 새 mutation을 중단한다. Write uncertainty는
   Read recovery proof로 quarantine 해제할 수 없다. GUI의 Persisted Mutation Recovery에는
   dispatch 전 target/value와 accepted/terminal/readback 상태가 crash-safe journal로 남아야 한다.
   강제 종료/전원 손실 시험에서는 재시작 후 자동 replay가 0회이고, target 물리 확인 체크와 명시적
   acknowledgement 뒤에만 interlock이 해제되는지 별도로 확인한다.
7. 별도 sentinel 시험이 필요하면 승인한 conservative range 안의 값 하나를 Write하고 즉시
   Readback한 뒤 원래 값을 복원하고 다시 Readback한다. PLC/TCP terminal packet, EtherCAT
   mailbox 관측 또는 executor callback trace를 함께 보존한다.
8. offline/timeout/cancel/session-loss 시험은 same-value happy path와 restore가 모두 PASS한 뒤
   별도 수행한다. direct `0x6040/0x607A/0x60FF/0x6071` write는 항상 금지한다.

각 정상 요청은 Submit Queued와 terminal Completed/Success를 반환해야 한다. 의도한
실패 뒤에도 다음 Submit이 진행돼야 한다. 실제 active/draining 구간의 일시적 Busy는
정상이지만 terminal 처리 뒤 지속되는 Busy는 FAIL이다. 상세 packet 사실과 source 추정
경계는 `../../test/packet_capture/SIGMATEK_API_Analyze/SDO_Test_Error_analysis_2026-07-22.md`를
따른다.

## 9. 재접속과 Adopt

1. 완료된 record를 release하지 않은 상태에서 PC TCP session만 종료한다.
2. 새 session으로 연결하고 capability를 다시 읽는다.
3. BootId가 같을 때만 `AdoptRecorder(recordId, bufferId, bootId)`를 호출한다.
4. 새 OwnerSessionEpoch를 받은 뒤 임의 chunk offset부터 다운로드를 재개한다.
5. PLC reboot로 BootId가 바뀐 뒤 이전 identity의 adopt가 거부되는지 확인한다.

## 10. 현재 미구현 또는 Unsupported가 정상인 기능

아래는 버튼/API contract가 보여도 PLC 성공을 기대하지 않는다.

- D4 Double Buffer
- D5 `SubmitPIWrite (0x7E21)`
- D5 PI Write, arbitrary/non-allowlisted SDO Write와 8/12-byte Read
- extended SDO `ReadSDOResultChunk (0x7E51)`
- D6 static/handle compatibility facade. Phase 1 D1/D2 instance facade는 시험 범위에 포함한다.

D4 Double과 D5 Write/extended capability가 0인 동안 WPF는 해당 control을
비활성화하거나 API가 호출 전 차단해야 한다. 승인 축을 활성화해도 arbitrary Write는
SDK와 PLC 양쪽에서 차단해야 한다. `0x7E21/0x7E51` exact request는
`UnsupportedFeature`를 반환해야 한다. D6에는 호출할 PLC command 자체가 없고 Phase 1
PI/Bulk facade도 기존 D1/D2 command만 사용한다.

## 11. 시험 결과 기록

각 시험 결과에는 다음을 남긴다.

- Git commit 또는 dirty diff 식별자
- LASAL project/compiler version
- PLC model, firmware, cycle time, free RAM
- capability 전체 값과 BootId
- WPF/API build configuration
- 성공/실패 step과 실제 error/status
- D5 requested timeout, SubmitCycle, CompletionCycle, unsigned cycle delta,
  terminal State/Outcome/DetailCode/ResultLength
- TCP packet capture
- System Trace의 task/core/priority/jitter
- Recorder 설정, header, CSV, bank hash

이 결과가 통과한 뒤에만 검증된 DLL, 예제 프로그램, 문서를 고객 배포 폴더로 옮긴다.

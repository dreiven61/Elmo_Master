# LMC Diagnostics 남은 구현 및 검증 계획

- 기준일: 2026-07-21
- 최종 검토일: 2026-07-22
- 대상: LASAL PLC diagnostics D0-D6, C# SDK, `LasalApiWpfTestApp`,
  `Codex_PMAS_WPF_Version2`, native packet capture 비교
- 현재 PLC capability baseline: `0x0000003F`

## 1. 결론

현재 Git source와 working implementation 기준으로 D0-D3, D4 single-bank Ring/Trigger와
D5 first-slice SDO Read 실행부가 구현돼 있다. D5는 capability gate가 꺼져 있어 PLC가
광고하거나 실행하지 않으며, D4 Double bank와 D6 static compatibility facade는 구현되지
않았다.

이번 작업에서 Health 화면 예외, Recorder Stop 완료 경쟁과 Download/CSV 용어 혼동을
수정했다. `LMCEcatInputLatch1` 중복 주기 실행 의심은 HEAD/current XML 재비교 결과
실제 회귀가 아닌 것으로 정정했다. 23개 PMAS/MMCLib native
capture도 분석해 PMAS Version2 Recorder의 ready/header/range gate와 PI 선택 변환을
보완했다. 이 capture에는 custom `0x7Exx` packet이 없으므로 LASAL diagnostics 실기
증거로 사용하지 않는다.

2026-07-22 `LMCSdoExecutor : EtherCAT_SDOBase`, 축별 executor 4개, service one-ticket
실행부와 두 network의 연결을 구현했다. Recorder terminal Stop 멱등 처리도 유지했다.
PC 자동 시험 103/103, WPF Debug/Release build, LASAL source-only/full-network 정적 계약과
10:53 IDE Rebuild/Link가 통과했다. 최신 `Find in Implementation` smoke, PLC download,
D1-D4 fault/capture 실장 시험과 실제 SDO runtime 시험은 아직 수행하지 않았다.

이 문서에서 다음 표현은 구분한다.

- `구현됨`: source와 wire 계약이 존재하고 정적 검증 대상에 포함됐다는 뜻이다.
- `활성`: PLC가 capability bit를 광고하도록 source가 구성됐다는 뜻이다.
- `PLC 검증 완료`: 변경 source를 실제 PLC에 download하고 장비 조건에서 결과를
  확인했다는 뜻이다. 현재 D1-D4에는 이 판정을 사용하지 않는다.

## 2. D0-D6 현재 상태

| 단계 | 현재 상태 | 구현 범위 | 남은 작업 |
|---|---|---|---|
| D0 | 구현됨 | common envelope, capability, BootId, C#/PLC dispatcher | 회귀 시험 유지 |
| D1 | internal test source 활성 | 4축 Health, 24-entry Catalog, PI Read, RT latch/seqlock | fault 조건 PLC 검증 |
| D2 | internal test source 활성 | 최대 24-entry Bulk configure/status/snapshot/release | same-cycle 및 부하 PLC 검증 |
| D3 | internal test source 활성 | 1,280,000-byte single bank, 최대 24채널 Manual Recorder, download/adopt/release | RAM, jitter, 장시간 upload, reconnect PLC 검증 |
| D4 | single-bank Ring/Trigger 활성 | pre-trigger ring, Edge/Window/Mask, forced trigger, chronological upload | trigger PLC 검증 및 Double bank 구현 |
| D5 | first-slice Read source/network 구현 및 IDE build 완료; capability off | 4축 derived executor, 한 ticket, status/queued cancel, timeout/orphan drain, SDK/WPF read-only policy | PLC runtime 검증 후 bit 8 승인; 이후 write policy |
| D6 | 미구현 | instance 기반 `LMCConnection`은 유지 | C# static/handle compatibility facade |

현재 정상 retained BootId 경로의 capability는 다음과 같다.

```text
CapabilityBits       = 0x0000003F
MapRevision          = 0x957F101E
CatalogEntryCount    = 24
RecorderBufferCount  = 1
MaxSdoDataBytes      = 0
```

즉 bit 0-5만 활성이다. bit 6 `RecorderDoubleBank`, bit 7 `PIWrite`, bit 8
`SDORead`, bit 9 `SDOWrite`, bit 12 `ExtendedSdoResultChunk`는 0이다.

## 3. 이번 작업에서 완료한 수정

### 3.1 EtherCAT Health WPF 예외

`HealthSlaveRow.Online`은 읽기 전용 속성이다. DataGrid checkbox의 기본 TwoWay
binding을 `Mode=OneWay`로 바꿨다. `Grid.IsReadOnly=True`만으로는 binding mode가
바뀌지 않으므로 이 수정이 필요하다.

### 3.2 Recorder Stop 완료 경쟁

Stop 전 authoritative status를 읽는다. 이미 terminal 상태이면 Stop command를 보내지
않는다. status 확인 직후 PLC가 자연 완료되는 TOCTOU 구간에서 Stop이
`InvalidState/DetailCode=19`를 반환하면 status를 다시 읽고 `Ready` 또는 `Uploading`인
경우에만 자연 완료로 처리한다. Fault 및 다른 오류는 숨기지 않는다.

### 3.3 Recorder Download와 Export CSV 표시

- `Download`는 PLC의 frozen sample을 WPF 프로세스의 PC 메모리로 가져온다. 이 단계는
  파일을 생성하지 않는다.
- `Export CSV`는 메모리에 내려받은 sample을 사용자가 Save dialog에서 선택한 경로에
  파일로 쓴다.
- 완료 메시지에 실제 CSV 경로를 표시한다.

### 3.4 `LMCEcatInputLatch1` network 재검토 정정

최초 dirty `.lcn` 검토에서 `LMCEcatInputLatch1`에 독립 `RealTime=1 ms` task가
추가됐다고 잘못 판정했다. HEAD와 current XML 모두 이 객체에
RealTime/Cyclic/Background scheduling 속성이 없다. 현재 실행 경로는 이미
`_LMCAxis1.LMCPreRtWorkTrigger -> LMCEcatInputLatch1.ClassSvr` 하나이고
full-network 정적 계약도 PASS다. `.lcn/.lcb` diff는 연결·scheduling 변경이 아니라
IDE 시각 배치 metadata이므로 기능 수정으로 커밋하지 않는다.

### 3.5 native capture와 PMAS Version2 정렬

제공된 23개 capture는 모두 PMAS/MMCLib native port 4000 호출이다. Custom LASAL
diagnostics `0x7Exx` packet은 포함하지 않는다. 분석 결과를 다음처럼 반영했다.

- native PI Bulk 대응을 `MMC_ConfigureBulkReadPI (0x1102)`와
  `MMC_PerformBulkReadCmdPI (0x1103)`로 확정했다. Generic parameter Bulk
  `0x10C9/0x10CA`는 custom D2 범위 밖이다.
- PMAS Health에 port 0-3의 InvalidFrames counter를 포함했다.
- PMAS Recorder의 checked PI를 native `uiRv/uiRc`로 변환하는 local helper를 추가했다.
- `uiSr` ready mask, global header `Rl`, selected buffer와 `[From..To]` 범위를 확인하기
  전에는 Header/Download를 실행하지 않는다. 실패한 RPC 뒤 stale cache도 재사용하지 않는다.
- capture로 확인된 SDO 성공 범위는 `0x1000:0` UInt32 4 bytes뿐이다. D5 첫 증분은
  4-byte Read-only로 제한하고 8/12-byte와 Write는 별도 gate 뒤에 연다.

PMAS Version2의 `uiSr=0` 차단과 `0x0104` ready flow는 source/build 수준으로만
확인했다. 실제 controller UI smoke는 남아 있다.

### 3.6 D5 capability-gated dispatcher와 실행부

`0x7E03 GetOperationStatus`, `0x7E04 CancelOperation`, `0x7E50 SubmitSDO`를 default
case에 맡기지 않고 `LMCDiagnosticsService`의 명시적 handler와 one-ticket 실행부로
구현했다.

- `0x7E03/0x7E04`는 exact 16-byte request만 구조적으로 유효하다.
- `0x7E50`은 32-byte header, OperationFlags 0/1, reserved zero와 read/write별 정확한
  payload 길이를 검증한다.
- malformed shape는 `BoundsInvalid`, 구조적으로 유효한 request는
  gate-off 상태에서 `UnsupportedFeature`다.
- capability는 계속 `0x0000003F`, `MaxSdoDataBytes=0`이다. ticket, drive callback과
  SDO 실행 source는 존재하지만 compile-time gate 앞에서 비활성이다.
- PC 회귀는 first-slice capability `MaxSdoDataBytes=4`에서 4-byte read를 허용하고
  8-byte read를 송신 전에 거부하는 경계를 포함한다.

위 세 handler와 executor/service 실행부는 최신 정적 계약 및 10:53 LASAL IDE
Rebuild/Link를 통과했다. 이것은 PLC mailbox 동작 성공 증거가 아니다.

### 3.7 `EtherCAT_SDOBase` 파생 executor 구현

사용자가 추가한 `EtherCAT_SDOBase`와 축별 object/network를 검토했다. plain base의
수동 `Para*` channel을 운영 API로 쓰지 않고 다음 구조를 채택했다.

- `LMCSdoExecutor : EtherCAT_SDOBase` 파생 class와 축별 4개 instance
- 파생 class는 inherited `toSlave`와 actual-length callback만 재사용하는 transport adapter
- `ParaReadWrite::Write`를 override해 manual SDO 시작 경로 차단
- private 4-byte buffer와 cross-task safe callback mailbox 사용
- D5 ticket, BootId, session owner, timeout/cancel은 `LMCDiagnosticsService`가 전담
- physical Running cancel은 지원하지 않고 queued-only cancel과 orphan drain 적용
- `LMCSdoExecutor1..4.toSlave -> Elmo_11..41.ClassState`와
  `LMCDiagnosticsService1.SdoAxis1..4 -> LMCSdoExecutor1..4.ClassState` 연결
- plain `EtherCAT_SDOBase1..4` 제거, executor object의 visualization/remote surface 차단

정확한 class, state machine, wire validation과 검증 gate는
`LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`를 기준으로 한다.

## 4. 구현 우선순위

단계 번호는 설계 분류이고 아래 `P0-P5`는 실제 작업 순서다.

| 우선순위 | 작업 | 완료 조건 |
|---|---|---|
| P0 | LASAL IDE save 회귀 복구 완료 | D5 실행 source와 Recorder terminal Stop 병합, 정적 계약, 10:53 Rebuild/Link 통과 |
| P1 | 최신 LASAL source PLC 검증 | 최신 implementation smoke 후 5절의 D1-D4 행을 모두 통과하고 packet/trace 결과를 보존 |
| P2 | D5 첫 증분: SDO Read-only runtime 승인 | 구현된 한 ticket/4축/4-byte inline source를 PLC에서 검증한 뒤 bit 8만 추가 광고 |
| P3 | D4 Double bank | 두 고정 bank의 capture/upload 소유권과 full 정책을 구현하고 RAM/jitter 기준 통과 후 bit 6 광고 |
| P4 | D5 Write Policy | PI/SDO Write를 별도 증분으로 구현하고 기본 off, 이중 allowlist, type/range/state/owner 검증 |
| P5 | D6 C# compatibility facade | PLC/wire 변경 없이 handle registry와 static sync/async wrapper 및 concurrency 시험 추가 |

P2를 P3보다 먼저 둔 이유는 SDO Read-only가 한 ticket과 4-byte 결과로 범위를
고정할 수 있기 때문이다. Double bank는 PLC RAM, RT jitter, 두 bank ownership 및
reconnect 의미를 함께 검증해야 하므로 현재 single-bank 실장 기준을 먼저 확정해야 한다.

## 5. D1-D4 PLC 검증 매트릭스

| 단계 | 시험 | 합격 기준 | 증거 |
|---|---|---|---|
| D1 Health | 정상 OP 상태에서 Health 읽기 | 4축 행이 표시되고 WPF binding 예외가 없다 | WPF log와 screenshot |
| D1 fault | slave 단절, cable fault, AL/DS402 fault | `Online/EC State/AL Code/DS402/Axis Error`가 실제 상태와 일치한다 | WPF log, PLC 상태, packet capture |
| D1 stale | fault 전후 PI Read | 직전 raw 값이 새 정상값으로 표시되지 않고 stale/offline status가 붙는다 | cycle/status 비교 |
| D2 same-cycle | 최대 24개 signal Bulk Snapshot | 모든 값의 `CycleCounter`가 같고 TCP 처리 중 PLC object 개별 live read가 없다 | packet과 latch trace |
| D2 lifecycle | configure/read/release 및 reconnect | owner, BootId, revision 불일치를 거부하고 release 후 resource가 재사용된다 | 요청/응답 log |
| D3 capacity | 16채널 x 20,000, 24채널 x 13,333 | PLC가 반환한 `AcceptedCapacity` 안에서 sample 수와 stride가 일치한다 | header, chunk, CSV |
| D3 timing | divider와 sample period 변경 | cycle 간격이 설정 divider와 일치하고 허용 RT jitter를 넘지 않는다 | LASAL trace/Data Analyzer |
| D3 immutable upload | record 완료 후 장시간 chunk download | download 중 frozen bank header/hash/sample이 변하지 않는다 | 반복 header/hash |
| D3 reconnect/adopt | disconnect, 같은 BootId reconnect, exact/zero-ID adopt | active Ring과 frozen record를 규칙대로 회수하고 다른 BootId는 거부한다 | session log |
| D3 resource | full, Stop, Release, buffer 재사용 | full/terminal 상태와 StopReason이 일치하고 Release 전 덮어쓰지 않는다 | status/header log |
| D4 trigger | Edge/Window/Mask 각각 조건 발생 | `SampleCount=Pre+1+Post`, `TriggerIndex=Pre`, `StopReason=TriggerComplete` | status/header/CSV |
| D4 forced | `Trigger Now` | 입력 health와 무관하게 한 번만 trigger되고 두 번째 요청은 거부된다 | request/status log |
| D4 invalid input | trigger signal의 EtherCAT sample을 invalid로 전환 | invalid 구간을 건너뛴 가짜 edge/window 전이가 발생하지 않는다 | latch/trigger trace |
| D4 Stop race | 자연 완료와 Stop을 같은 시점에 실행 | WPF가 종료되지 않고 최종 status를 authoritative 결과로 표시한다 | WPF execution log |

장비 허용 RT jitter 수치는 PLC cycle과 현재 motion 부하를 측정한 뒤 시험 기록에
명시한다. 측정 전 임의 수치를 완료 기준으로 고정하지 않는다.

## 6. D5 첫 증분: SDO Read-only

### 6.1 포함 범위

- diagnostics service 전체에서 active ticket 한 개
- 물리 축 1-4의 SDO Read만 허용
- `SubmitSDO (0x7E50)`, `GetOperationStatus (0x7E03)`,
  `CancelOperation (0x7E04)` 실행
- 결과 길이 4 bytes만 허용
- 4 bytes를 `GetOperationStatus` response에 inline
- `Queued -> Running -> Completed/Failed` 상태와 queued-only cancel
- drive busy는 bounded retry, start 실패와 SDO abort는 ticket failure로 반환
- disconnect 시 queued ticket 취소, running callback 결과는 폐기한 뒤 slot 회수
- 실제 object 크기가 확인된 read allowlist만 사용. 첫 실기 벡터는 `0x1000:0`
  UInt32/4-byte read다.

2026-07-22 현재 위 세 command, `SdoAxis1..4` client, callback mailbox, one-ticket state
machine, RT latch cycle 기반 실행 scheduling과 network/generated table까지 구현했다.
10:53 Rebuild/Link도 통과했다. 그러나 compile-time gate는 `FALSE`이고 PLC download 및
실제 axis 1-4 `0x1000:0` 성공/실패/timeout/orphan 동작은 미검증이다.

### 6.2 제외 범위

- PI Write와 SDO Write
- 8/12-byte read, `ReadSDOResultChunk (0x7E51)`, 4 bytes 초과 결과
- 둘 이상의 동시 ticket 또는 축별 병렬 SDO
- 동적 메모리와 무제한 retry

첫 증분이 모든 gate를 통과하기 전에는 bit 8을 0으로 유지한다. 통과 후에도 새로 켜는
bit는 `SDORead` bit 8 하나뿐이다. D4 Double이 계속 꺼진 현재 capability에 bit 8만
더하면 `CapabilityBits=0x0000013F`, `MaxSdoDataBytes=4`다. bit 7, 9, 12는 계속 0이다.

기존 `ECAT_DS402Base::AddASyncEntryDS402` wrapper는 실제 반환 길이를 service에 전달하지
않는다. 새 설계는 `LMCSdoExecutor : EtherCAT_SDOBase`에서 lower-level callback의
`aPara[5]` actual length와 `aPara[6]` abort code를 직접 보존한다. 그래도 최초 allowlist는
실측된 4-byte `0x1000:0`으로 제한하고 success, busy, timeout, cancel,
disconnect/orphan을 PLC에서 시험한다. 8/12-byte는 first slice와 분리한다.

구현 구조와 정확한 상태 전이는
`LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`를 따른다.

## 7. D4 Double bank 구현 범위

D4 Double은 기존 single-bank 배열의 mode flag만 바꾸는 작업이 아니다. 아래를 모두
구현한다.

- 두 번째 고정 bank와 bank별 `Free/Capturing/Ready/Uploading` 상태
- bank별 `RecordId`, `BufferId`, header, sample count, trigger metadata
- `BufferId=0/1`의 정확한 identity/owner/BootId 검사
- 한 bank upload 중 다른 bank에서 capture 시작
- 두 bank가 모두 점유됐을 때 RT를 block하지 않고 새 configure/start를 `Busy`로 거부
- terminal bank는 Release 전 덮어쓰지 않음
- reconnect/adopt 시 대상 bank의 모호하지 않은 선택 규칙
- 한 bank fault/release가 다른 bank metadata를 변경하지 않음
- 두 bank RAM 배치와 worst-case recorder copy의 RT jitter 측정

zero-ID discovery는 single-bank에서만 대상이 유일하다. Double 활성 시에는 exact
`RecordId/BufferId`를 기본 adopt 경로로 사용하고, zero-ID discovery를 유지하려면 두
bank 중 하나를 고르는 wire 규칙을 먼저 추가해야 한다.

bit 6과 `RecorderBufferCount=2`는 capture/upload 동시성, 두 bank full, reconnect,
RAM/jitter 시험을 모두 통과한 뒤에만 광고한다.

## 8. D5 Write Policy와 D6

D5 Write는 Read-only 증분과 섞지 않는다. 기본값은 off이며 SDK allowlist와 PLC
allowlist가 모두 허용한 항목만 후보가 된다. 후보라도 type, 범위, owner, 물리 축,
축 상태를 모두 검사한다. ControlWord와 Target 계열 direct write는 영구 차단한다.

D6은 PLC command나 packet을 추가하지 않는다. 현재 instance API 위에 C# handle
registry와 static sync/async wrapper를 추가하고 stale handle, dispose, concurrent call을
시험한다. D1-D5 wire가 안정되기 전에 시작하지 않는다.

## 9. 정확한 검증 gate

| Gate | 명령 또는 절차 | 합격 기준 | 현재 판단 |
|---|---|---|---|
| C# PC contract | `MSBuild.exe LasalMotionControlLib.Tests.csproj /t:RunTests /p:Configuration=Debug /p:Platform=AnyCPU` | `103/103 passed` | 통과 확인 |
| WPF build | VS2019 MSBuild로 `LasalApiWpfTestApp.csproj`, Debug/Release AnyCPU build | error 0 | 둘 다 통과 확인 |
| LASAL source/network contract | `Verify-LasalContract.ps1 -RepositoryRoot <repo>` | `PASS LASAL.StaticContract` | 2026-07-22 source-only/full-network 통과 |
| LASAL IDE compile | 대상 tracked project Rebuild 후 Link | compile/link error 0 | 2026-07-22 10:53 최신 D5 executor/network 포함 통과 |
| LASAL implementation smoke | 변경 class마다 IDE `Find in Implementation` 또는 implementation tab 직접 open | InputLatch, RecorderStore, DiagnosticsService와 새 executor implementation이 정상 로드되고 IDE 예외가 없음 | 최신 Rebuild는 통과; 별도 최신 smoke는 재실행 대기 |
| LASAL IDE log | smoke 시작 시각 이후 `%TEMP%\Lasal2.log` 검색 | 신규 `CInvalidArgException` 0건 | 10:53 Rebuild error 0; 최신 implementation smoke 기준 검사는 대기 |
| diff hygiene | `git diff --check`와 staging 시 `git diff --cached --check` | whitespace error 0 | 최종 작업 종료 시 반복 |
| PLC capability | 변경 project download 후 `Refresh Capabilities` | 현재 baseline `0x0000003F`; D5 Read 승인 뒤 `0x0000013F` | 미검증 |
| PLC runtime | 5절 매트릭스와 D5/D4 Double 단계별 시험 | 모든 행의 합격 기준과 증거 확보 | 미검증 |

D5 PLC runtime은 static/IDE gate를 통과한 test build에서 bit 8과 `MaxSdoDataBytes=4`를
임시 활성화해 수행한다. 성공/실패/timeout/orphan evidence를 확보하기 전에는 그 값을
production capability로 승인하지 않고 기본 source는 `0x3F`, MaxSdo=0을 유지한다.

정적 계약 통과는 packet offset, source pattern, network 연결을 검증한다. LASAL
Rebuild/Link는 IDE 통합과 compile/link 가능성을 검증한다. 어느 것도 PLC scheduling,
EtherCAT fault 전이, RAM 여유, 실제 RT jitter, drive mailbox 응답을 대신하지 않는다.

LASAL class implementation은 tracked `.st`를 외부 편집기로 수정한다. IDE가 열린 상태에서
수정했다면 저장 전에 `Reload Class`를 실행한다. 권장 순서는 `IDE 저장/종료`, 외부 편집,
IDE 재열기 또는 `Reload Class`, Rebuild, `Find in Implementation` smoke다. stale IDE
model을 저장해 외부 implementation을 덮어쓰지 않는다.

## 10. 작업 종료 기준

각 우선순위는 다음 네 항목이 모두 있어야 종료한다.

1. source와 capability가 실제 구현 범위만 광고한다.
2. PC 자동 시험, LASAL 정적 계약, IDE Rebuild/Link를 통과한다.
3. 해당 단계의 PLC 시험 결과를 packet/log/trace로 저장한다.
4. 사용자 문서와 release status의 수치 및 미구현 표기가 source와 일치한다.

현재 working source에는 D1-D4와 gated D5 first-slice 실행부 및 4축 network가 있다.
실제 PLC download와 5절 및 D5 runtime 검증을 하지 않았으므로 D1-D5를 production
완료로 분류하지 않는다. D5 광고는 계속 `0x3F`, `MaxSdoDataBytes=0`이다.

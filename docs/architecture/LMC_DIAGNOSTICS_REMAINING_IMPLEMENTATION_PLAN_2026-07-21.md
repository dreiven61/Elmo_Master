# LMC Diagnostics 남은 구현 및 검증 계획

- 기준일: 2026-07-21
- 최종 검토일: 2026-07-27
- 대상: LASAL PLC diagnostics D0-D6, C# SDK, `LasalApiWpfTestApp`,
  `Codex_PMAS_WPF_Version2`, native packet capture 비교
- 현재 test-profile source capability: `0x0000213F`, `MaxSdoDataBytes=4`

## 2026-07-27 SDO Write checkpoint

- LASAL `0x7E50`, C# API와 WPF의 exact Int32/4-byte SDO Write 경로를 구현했다.
- 후보는 Gold `UI[24] 0x2F00:24`, local range `-1073741823..1073741823`이지만 drive
  program 미사용 여부와 첫 적용 축이 확정되지 않아 PLC/SDK global 및 axis 1..4 gate는 모두
  FALSE다. 현재 실제 승인 target은 0개다.
- WPF는 Write `Completed/Success` 뒤 동일 target/type/length의 exact 4-byte Readback이
  원 Write owner/current session/BootId/MapRevision에서 일치할 때까지 mutation과 Close를
  차단한다. identity mismatch는 Read submit 전에 거부한다. 불명확한 Write outcome은 Read proof로
  quarantine 해제하지 않는다.
- PC Debug/Release는 현재 각 286/286, WPF Debug/Release 별도 output build와 LASAL SourceOnly는
  PASS했다. tracked `Classes.lcb`가 신규 Write declaration과 아직 동기화되지 않아 switch 없는
  full static은 의도적으로 FAIL하며 LASAL IDE Reload Class/저장/Rebuild가 필요하다.
- 실제 활성화 전 남은 gate는 UI[24] 미사용/시험 축 확정, PLC/SDK 동일 gate 활성화, LASAL
  build/smoke/download, same-value Write/readback/restore와 mailbox/pcap 증거, 그리고 강제
  종료/전원 손실 뒤 pending Write/readback을 복구할 durable journal/운영자 ACK 정책이다.

## 2026-07-27 CREVIS topology 및 digital I/O checkpoint

- 현재 working source의 configured physical order는 `GL_9086_11(SlaveIndex 0) ->
  Elmo_11..41(SlaveIndex 1..4)`인 5-slave 구성이다. 이것은 source/ENI/network 확인 결과이며
  현재 변경의 LASAL Rebuild/Link, PLC download와 실제 I/O PASS는 아니다.
- slave 순서/identity, slot module, PDO index/sub-index, I/O 폭과 generated process-image
  mapping은 configured topology에 고정한다. Online/EtherCAT state/AL status, value와
  valid/fresh/stale quality만 runtime에 변한다. 물리 순서를 바꾸면 ENI/network를 다시
  생성해야 하며 API가 runtime discovery로 schema를 바꾸지 않는다.
- 기존 `0x7E10 ReadEtherCATHealth`의 exact 200-byte, 4-entry Elmo subset은 유지한다.
  기존 wire `SlaveIndex=0..3`은 호환용 legacy drive index다. actual physical node index
  0..4는 신규 topology API에서만 제공한다.
- C# SDK contract command는 `0x7E11/0x7E12` topology info/chunk, `0x7E13` node health,
  `0x7E22` digital I/O read, `0x7E23` output write submit이다. model/parser/golden과 capability-off
  pre-wire 검증은 구현했다. capability bit 14~17은 PLC handler와 data source가 구현·검증될
  때까지 모두 0으로 두며 현재 `CapabilityBits=0x0000213F`와 active command 수는 변하지 않는다.
- output write는 GT-22BA output slot-module의 configured `IOReference`와 valid mask만 허용한다. whole-word와 atomic
  masked write를 하나의 PLC RT owner가 적용하고, non-RT diagnostics service는 owner/session,
  BootId/topology revision과 ticket을 소유한다. validation 실패, stale/offline/not-OP,
  mailbox/owner 불가와 uncertain outcome은 fail-closed하며 자동 replay하지 않는다.

구현 순서는 다음으로 고정한다.

| 순서 | 구현 | capability |
|---:|---|---|
| IO-0 | current 5-slave LASAL Rebuild/Link와 실제 configured order 확인 | 모두 off |
| IO-1 | C# model/protocol/golden과 capability-off facade | 완료, 모두 off |
| IO-1B | PLC reserved handler와 exact `UnsupportedFeature` 응답 | 미구현, 모두 off |
| IO-2 | configured topology info/chunk와 revision | bit 14를 runtime PASS 뒤 활성 |
| IO-3 | node health와 DI/output-shadow coherent snapshot | bit 15/16을 각각 runtime PASS 뒤 활성 |
| IO-4 | RT single-writer, whole/masked mailbox와 `0x7E23` ticket | bit 17 off |
| IO-5 | invalid mask/offline/stale/contention/response-loss/RT 및 physical output matrix | bit 17을 최종 활성 |

exact field layout, local Elmo API 근거와 수정 파일은
[LMC EtherCAT Topology 및 Digital I/O API 설계](LMC_ETHERCAT_TOPOLOGY_AND_IO_API_DESIGN_2026-07-27.md)를
기준으로 한다.

## 1. 결론

현재 Git source와 working implementation 기준으로 D0-D3, D4 single-bank Ring/Trigger와
D5 general-inline SDO Read 실행부가 구현돼 있다. test-profile source는 D5 bit 8
`SDORead`, bit 13 `SDOReadGeneralInline`과 `MaxSdoDataBytes=4`를 광고하도록
활성화했다. gate-on 첫 runtime은 same-cycle immediate
timeout으로 실패했고 request-local/class-member shadowing을 수정했다. 후속 download의
Slave 1~4 happy path는 43~54 cycles 뒤 Completed/Success와 UInt32 4-byte 결과로
PASS했다. 이것은 legacy `0x1000:0` fixed-vector runtime 증거다. nonzero Index,
Sub-index 0-255와 exact typed 1/2/4-byte general-inline source는 구현됐다. 과거 BootId 6
general-inline 캡처의 `ResourceBusy(9)` 결함은 callback ordering과 owned completion
회수 source에서 수정했다. 이후 `10_DriveRead_Axis1to4.pcapng`에서 general-inline
Int8/1-byte와 BitField16/2-byte가 전 축 Completed/Success로 확인됐다. general-inline
UInt32/4-byte는 `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서
Completed/Success를 반환했다. 같은 BootId 8에서 UInt16/2-byte TypeMismatch 후
Int8/1-byte가 성공해 executor 무재부팅 recovery도 확인됐다. SDO abort/offline,
timeout, queued cancel, disconnect/orphan, active contention 등 전체 fault matrix는 남았다.
D4 Double bank와 기존 D6 계획의 static/handle facade는 구현되지 않았다. Phase 1에는
별도 D6 wire 없이 D1/D2를 재사용하는 instance 기반 PI/Bulk compatibility facade가
구현됐다.

이번 작업에서 Health 화면 예외, Recorder Stop 완료 경쟁과 Download/CSV 용어 혼동을
수정했다. `LMCEcatInputLatch1` 중복 주기 실행 의심은 HEAD/current XML 재비교 결과
실제 회귀가 아닌 것으로 정정했다. 23개 PMAS/MMCLib native
capture도 분석해 PMAS Version2 Recorder의 ready/header/range gate와 PI 선택 변환을
보완했다. 이 capture에는 custom `0x7Exx` packet이 없으므로 LASAL diagnostics 실기
증거로 사용하지 않는다.

2026-07-22 `LMCSdoExecutor : EtherCAT_SDOBase`, 축별 executor 4개, service one-ticket
실행부와 두 network의 연결을 구현했다. Recorder terminal Stop 멱등 처리도 유지했다.
PC 자동 시험 148/148, WPF Debug/Release build와 각 3초 startup smoke 및 현재 수정 LASAL SourceOnly/
full static 계약이 통과했다. `Classes.lcb`의 `TryStartRead` declaration도 current source와
동기화됐다. 10:53 IDE
Rebuild/Link는 gate-off baseline 결과다. gate-on 첫 D5
runtime은 Ticket 11 same-cycle Expired/TimedOut으로 실패했지만 수정 후 BootId 5의
Slave 1~4 Ticket 5~8은 모두 Completed/Success로 통과했다. 이후 BootId 6 general-inline
Submit은 `ResourceBusy`로 실패했고 callback recovery source를 수정했다. 수정본의
general-inline 1/2-byte runtime은 `10_DriveRead_Axis1to4.pcapng`에서 PASS했다.
`12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서 general-inline UInt32/4-byte
성공과 같은 BootId TypeMismatch 후 Int8/1-byte 복구도 PASS했다. D5 전체 fault
matrix, D1/D2 fault/soak와 D3/D4 실장 시험은 아직 남아 있다.

이 문서에서 다음 표현은 구분한다.

- `구현됨`: source와 wire 계약이 존재하고 정적 검증 대상에 포함됐다는 뜻이다.
- `활성`: PLC가 capability bit를 광고하도록 source가 구성됐다는 뜻이다.
- `PLC 검증 완료`: 변경 source를 실제 PLC에 download하고 장비 조건에서 결과를
  확인했다는 뜻이다. D1/D2 happy path packet은 확보했지만 fault/soak까지 포함한
  단계 전체 완료 판정은 아직 D1-D4 어느 단계에도 사용하지 않는다.

## 2. D0-D6 현재 상태

| 단계 | 현재 상태 | 구현 범위 | 남은 작업 |
|---|---|---|---|
| D0 | 구현됨 | common envelope, capability, BootId, C#/PLC dispatcher | 회귀 시험 유지 |
| D1 | internal test source 활성 | legacy 4-drive Health, 24-entry Catalog, PI Read, RT latch/seqlock | fault 조건 PLC 검증; 5 slaves + 2 slot-module entry topology/I/O extension은 C# contract만 구현, PLC capability off |
| D2 | internal test source 활성 | 최대 24-entry Bulk configure/status/snapshot/release | same-cycle 및 부하 PLC 검증 |
| D3 | internal test source 활성 | 1,280,000-byte single bank, 최대 24채널 Manual Recorder, download/adopt/release | RAM, jitter, 장시간 upload, reconnect PLC 검증 |
| D4 | single-bank Ring/Trigger 활성 | pre-trigger ring, Edge/Window/Mask, forced trigger, chronological upload | trigger PLC 검증 및 Double bank 구현 |
| D5 | general-inline Read source 구현; legacy 4-byte와 수정본 1/2/4-byte 성공 pcap PASS, TypeMismatch 후 same-Boot recovery PASS | 4축 derived executor, 한 ticket, nonzero Index/any SubIndex, typed 1/2/4-byte inline status, queued cancel, timeout/orphan drain | abort/offline, timeout, cancel/orphan, contention 포함 fault matrix 후 production 승인; 이후 write policy |
| D6 | 기존 static/handle 계획 미구현 | Phase 1 D1/D2 기반 PI/Bulk instance facade는 구현 | 별도 wire 없이 static registry가 실제로 필요한지 재평가 |

현재 정상 retained BootId 경로의 capability는 다음과 같다.

```text
CapabilityBits       = 0x0000213F
MapRevision          = 0x957F101E
CatalogEntryCount    = 24
RecorderBufferCount  = 1
MaxSdoDataBytes      = 4
```

즉 bit 0-5, bit 8 `SDORead`와 bit 13 `SDOReadGeneralInline`이 활성이다. bit 13은
bit 8과 `MaxSdoDataBytes=4`를 요구한다. bit 6 `RecorderDoubleBank`, bit 7
`PIWrite`, bit 9 `SDOWrite`, bit 12 `ExtendedSdoResultChunk`는 0이다.

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
해당 topology 검사는 이전 full-network 계약에서 PASS했다. 현재 `Classes.lcb`도
`TryStartRead` declaration과 동기화돼 full static suite가 PASS한다.
`.lcn/.lcb`의 당시 layout diff는 연결·scheduling 변경이 아니라
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
- capture로 확인된 SDO 성공 범위는 legacy `0x1000:0` UInt32/4-byte와
  `10_DriveRead_Axis1to4.pcapng`의 general-inline `0x6061:0` Int8/1-byte,
  `0x6041:0` BitField16/2-byte, `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`의
  `0x1018:1` UInt32/4-byte다. 같은 12번 capture에서 TypeMismatch 후 Int8/1-byte
  무재부팅 recovery도 확인했다. 8/12-byte와 Write는 계속 별도 gate 뒤에 둔다.

PMAS Version2의 `uiSr=0` 차단과 `0x0104` ready flow는 source/build 수준으로만
확인했다. 실제 controller UI smoke는 남아 있다.

### 3.6 D5 capability-gated dispatcher와 실행부

`0x7E03 GetOperationStatus`, `0x7E04 CancelOperation`, `0x7E50 SubmitSDO`를 default
case에 맡기지 않고 `LMCDiagnosticsService`의 명시적 handler와 one-ticket 실행부로
구현했다.

- `0x7E03/0x7E04`는 exact 16-byte request만 구조적으로 유효하다.
- `0x7E50`은 32-byte header, OperationFlags 0/1, reserved zero와 read/write별 정확한
  payload 길이를 검증한다.
- malformed shape는 `BoundsInvalid`이고, 구조적으로 유효한 general-inline request는
  gate-on test source에서 ticket 실행 경로로 들어간다.
- stable BootId에서 capability는 `0x0000213F`, `MaxSdoDataBytes=4`다. bit 13은 bit 8과
  MaxSDO=4가 함께 있을 때만 유효하다. BootId가 0이거나
  Diagnostics client가 없으면 MaxSDO는 0인 fail-closed 응답을 유지한다.
- PC 회귀는 bit 13이 없을 때 legacy `0x1000:0` UInt32/4-byte만 허용하고, bit 13이
  있으면 supported ValueType과 정확히 일치하는 1/2/4-byte general request를 허용하며
  8-byte read를 송신 전에 거부하는 경계를 포함한다.

위 세 handler와 executor/service 실행부는 최신 정적 계약을 통과했다. 10:53 LASAL IDE
Rebuild/Link는 gate-off source 기준이므로 gate-on source는 다시 Rebuild해야 한다. 어느
결과도 PLC mailbox 동작 성공 증거가 아니다.

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

### 3.8 general-inline ResourceBusy 회귀와 executor recovery 수정

`SDO_Test_Error.pcapng`에서 BootId 6 PLC는 `0x213F`, MaxSDO=4를 정상 광고했지만
캡처된 두 Submit은 ticket 전 `ResourceBusy(9)`로 실패했다. 실제 wire request는
`0x6061:0` UInt16/2와 Int8/1 두 건이다. `0x6041:0`, `0x1018:1`, accepted ticket과
status는 이 capture에 없다.

wire DetailCode 9는 service active/drain gate와 executor non-reusable gate를 구분하지
않는다. 최초 callback도 capture 밖이므로 정확한 최초 trigger는 추정으로 남긴다. 다만
실패 당시 source에서 다음 결함은 직접 확인했다.

- vendor call 뒤에야 `Running`을 publish하는 callback race window
- owned callback validation failure가 `Quarantined`로 된 뒤 `Idle`로 회수되지 않는 경로
- orphan callback의 validation failure가 adapter 회수를 막을 수 있는 경로

수정 source는 vendor call 전에 `Running`을 publish한다. request 미접수와 owned
completion cleanup에는 내부 `Releasing=6`을 사용한다. owned validation failure는
`ResultReady`로 publish해 service가 terminal Failed로 보고한 뒤 release하고, orphan
callback은 public 결과 없이 release한다. active token이 없는 unsolicited/duplicate
callback과 token/atomic ownership 불일치만 hard quarantine한다. SourceOnly/full static
계약은 PASS했고 `Classes.lcb` declaration도 동기화됐다. 최신 IDE Rebuild/Link는
대기 중이다. 수정 source의 PLC download와 같은 BootId TypeMismatch 후 한 번의
재사용은 12번 capture로 PASS했지만, 더 넓은 연속/fault 재사용 matrix는 남았다.

## 4. 구현 우선순위

단계 번호는 설계 분류이고 아래 `P0-P5`는 실제 작업 순서다.

| 우선순위 | 작업 | 완료 조건 |
|---|---|---|
| P0 | LASAL source 회귀와 D5 shadowing 수정 완료 | D5 실행 source, Recorder terminal Stop, request-local 수정과 정적 계약 통과; 10:53 build는 gate-off baseline |
| P1 | executor 명시 초기화와 최신 LASAL source PLC 검증 | IDE에서 `LMCSdoExecutor` constructor declaration/`@STD` wiring을 생성하고 private state를 초기화한 뒤 gate-on source Rebuild/implementation smoke, 5절의 D1-D4 행과 D5 재시험을 통과해 packet/trace 결과 보존 |
| P2 | D5 general-inline SDO Read-only | 한 ticket/4축/1·2·4-byte inline source와 bit 8+13 광고 구현; legacy 4축, general-inline 1/2/4-byte packet success와 TypeMismatch 후 same-Boot recovery 확보; 나머지 fault/timeout/cancel/orphan matrix가 잔여 gate |
| P3 | D4 Double bank | 두 고정 bank의 capture/upload 소유권과 full 정책을 구현하고 RAM/jitter 기준 통과 후 bit 6 광고 |
| P4 | D5 Write Policy | PI/SDO Write를 별도 증분으로 구현하고 기본 off, 이중 allowlist, type/range/state/owner 검증 |
| P5 | D6 static/handle facade 재평가 | Phase 1 D1/D2 instance facade 사용성 검증 뒤 registry/static wrapper가 실제로 필요한 경우에만 추가 |

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
- nonzero ObjectIndex와 SubIndex 0..255 허용
- Bool/Int8/UInt8/BitField8은 1 byte, Int16/UInt16/BitField16은 2 bytes,
  Int32/UInt32/Real32/BitField32는 4 bytes만 허용
- 요청 type과 일치하는 1/2/4 bytes를 `GetOperationStatus` response에 inline
- `Queued -> Running -> Completed/Failed` 상태와 queued-only cancel
- drive busy는 bounded retry, start 실패와 SDO abort는 ticket failure로 반환
- disconnect 시 queued ticket 취소, running callback 결과는 폐기한 뒤 slot 회수
- first live regression vector는 `0x1000:0` UInt32/4-byte read다. 일반 Index/SubIndex는
  object dictionary에서 크기와 read-only 성격을 확인한 항목으로 시험한다.

2026-07-22 현재 위 세 command, `SdoAxis1..4` client, callback mailbox, one-ticket state
machine, RT latch cycle 기반 실행 scheduling과 network/generated table까지 구현했다.
10:53 gate-off Rebuild/Link는 baseline으로 통과했다. compile-time gate는 test 목적으로
`TRUE`며 legacy stable BootId capability `0x13F`, MaxSDO=4도 캡처에서 확인했다. 첫
`0x1000:0`은 request-local shadowing으로 same-cycle Expired 됐지만 수정 후 BootId 5의
Slave 1~4 요청은 43~54 cycles 뒤 모두 Completed/Success, UInt32 4-byte 결과를
반환했다. 이 캡처는 bit 13과 general-inline shape를 검증하지 않는다. current source의
`0x213F` capability는 BootId 6 capture에서 확인됐지만 general-inline Submit 두 건이
`ResourceBusy`로 실패했다. callback recovery 수정본의 1/2-byte 연속 성공은 BootId 8
capture로 확인했다. `12_SDO_GeneralInline_4Byte_FailureRecovery.pcapng`에서
general-inline UInt32/4-byte 성공과 TypeMismatch 후 같은 BootId의 Int8/1-byte 재사용도
확인했다. SDO abort/offline, timeout, queued cancel, disconnect/orphan, active contention
qualification은 여전히 필요하다.

### 6.2 제외 범위

- PI Write와 SDO Write
- 8/12-byte read, `ReadSDOResultChunk (0x7E51)`, 4 bytes 초과 결과
- 둘 이상의 동시 ticket 또는 축별 병렬 SDO
- 동적 메모리와 무제한 retry

runtime 시험을 위해 test-profile source에서 `SDORead` bit 8과
`SDOReadGeneralInline` bit 13을 열었다. D4 Double이 계속 꺼진 current source capability는
`CapabilityBits=0x0000213F`, `MaxSdoDataBytes=4`다. bit 7, 9, 12는 계속 0이며 전체
runtime matrix evidence 전에는 이 값을 production 승인으로 보지 않는다.

기존 `ECAT_DS402Base::AddASyncEntryDS402` wrapper는 실제 반환 길이를 service에 전달하지
않는다. 새 설계는 `LMCSdoExecutor : EtherCAT_SDOBase`에서 lower-level callback의
`aPara[5]` actual length와 `aPara[6]` abort code를 직접 보존한다. current source는
request별 1/2/4-byte actual length를 검증한다. legacy `0x1000:0` regression과 별도로
확인된 1-byte, 2-byte, nondefault Index/SubIndex vector 및 success, busy, timeout,
cancel, disconnect/orphan을 PLC에서 시험한다. 8/12-byte는 general-inline profile과
분리한다.

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

D6은 PLC command나 packet을 추가하지 않는다. Phase 1에서 현재 instance API 위에
D1/D2 기반 PI/Bulk builder/reader facade를 먼저 구현했다. 별도 handle registry와 static
sync/async wrapper는 이 facade의 실제 사용성 검증 뒤 필요성이 확인된 경우에만 추가하고,
추가한다면 stale handle, dispose와 concurrent call을 별도 시험한다.

## 9. 정확한 검증 gate

| Gate | 명령 또는 절차 | 합격 기준 | 현재 판단 |
|---|---|---|---|
| C# PC contract | `MSBuild.exe LasalMotionControlLib.Tests.csproj /t:RunPcTests /p:Configuration=Debug /p:Platform=AnyCPU` | `286/286 passed` | Debug/Release 통과 확인; topology/I/O contract 9개 포함 |
| WPF build/smoke | VS2019 MSBuild로 `LasalApiWpfTestApp.csproj` Debug/Release build | error 0 | 별도 output build 통과; current D5 visual/startup smoke는 대기 |
| LASAL SourceOnly contract | `Verify-LasalContract.ps1 -RepositoryRoot <repo> -SourceOnly` | PASS | 통과 확인 |
| LASAL full static contract | `Verify-LasalContract.ps1 -RepositoryRoot <repo>` | PASS | 현재 의도적 FAIL; `Classes.lcb` 신규 Write declaration 동기화/IDE Rebuild 뒤 재실행 |
| executor initialization | LASAL IDE에서 `LMCSdoExecutor` constructor 생성 후 state/buffer 명시 초기화 | declaration, generated `@STD` call, implementation 및 정적 assertion 일치 | 미완료; 자동 zero-init 보장을 확인하지 못했으며 current Busy의 직접 원인으로 확정된 것은 아님 |
| LASAL IDE compile | 대상 tracked project Rebuild 후 Link | compile/link error 0 | 10:53 gate-off baseline 통과; fixed-source runtime download 확인, build log 미보존 |
| LASAL implementation smoke | 변경 class마다 IDE `Find in Implementation` 또는 implementation tab 직접 open | InputLatch, RecorderStore, DiagnosticsService와 새 executor implementation이 정상 로드되고 IDE 예외가 없음 | fixed-source smoke 기록은 미보존 |
| LASAL IDE log | smoke 시작 시각 이후 `%TEMP%\Lasal2.log` 검색 | 신규 `CInvalidArgException` 0건 | 10:53 Rebuild error 0; 최신 implementation smoke 기준 검사는 대기 |
| CREVIS configured topology | current project Rebuild/Link, download 후 EtherCAT diagnostics | GL=physical index 0, Elmo=1..4인 5-slave configured order와 Vendor/Product/slot/PDO exact | source/ENI/network만 확인; build/download/live 대기 |
| topology/I/O read | `0x7E11/12/13/22` golden과 PLC read | topology revision/order exact, legacy `0x7E10` byte-identical, node state/quality만 동적, DI bit pattern 일치 | C# contract/parser/golden 완료; PLC/LASAL/WPF 미구현, capability bit 14~16은 0 |
| digital output write | `0x7E23` CAS ticket whole/masked/fault/RT matrix | single RT owner, stale output revision 거부, unmasked bit 보존, invalid/stale/offline에서 mutation 0, response-loss 자동 replay 0 | C# request/ticket/policy test 완료, SDK allowlist empty; PLC/LASAL/WPF 미구현, capability bit 17은 0 |
| diff hygiene | `git diff --check`와 staging 시 `git diff --cached --check` | whitespace error 0 | 최종 작업 종료 시 반복 |
| PLC capability | 변경 project download 후 `Refresh Capabilities` | stable BootId에서 `0x0000213F`, MaxSDO=4 | BootId 6 capture에서 확인; 최종 runtime 증거/fault matrix 미비로 production 미승인 |
| PLC runtime | 5절 매트릭스와 D5/D4 Double 단계별 시험 | 모든 행의 합격 기준과 증거 확보 | D0/D1/D2와 D5 legacy/general-inline 1/2/4-byte happy path pcap PASS; TypeMismatch 후 same-Boot recovery PASS; D5 나머지 fault/D1-D4 fault 재시험 대기 |

D5 PLC runtime을 위해 test source의 bit 8, bit 13과 `MaxSdoDataBytes=4`를 활성화했다.
legacy fixed-vector 4축 success는 확보했다. BootId 6 general-inline capture는 capability와
request shape를 확인했지만 Submit이 Busy로 실패한 과거 증거다. recovery 수정본의
1/2-byte success는 BootId 8 capture로 확인했고, 12번 capture는 general-inline 4-byte
success와 TypeMismatch 후 같은 executor 재사용을 증명했다. 나머지
fault/timeout/cancel/orphan, offline/abort와 contention evidence를 확보하기 전에는
이 값을 production capability로 승인하지 않는다.

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

현재 working source에는 D1-D4와 활성화한 D5 general-inline 실행부, shadowing 수정 및
4개 Elmo에 앞선 GL-9086을 포함한 configured 5-slave network source가 있다. 후자는 아직
LASAL build/download/live I/O 증거가 없다. legacy `0x1000:0` drive 1~4와 수정본 general-inline 1/2/4-byte
happy path는 성공 pcap을 확보했고 TypeMismatch 후 same-Boot executor 재사용도
확인했다. constructor 명시 초기화, 나머지 D5 fault/timeout/cancel/orphan matrix와
5절 검증 및 위 IO-0~IO-5가 남았다.
따라서 D1-D5를 production 완료로 분류하지 않는다. current test source 광고값은
`0x213F`, `MaxSdoDataBytes=4`다.

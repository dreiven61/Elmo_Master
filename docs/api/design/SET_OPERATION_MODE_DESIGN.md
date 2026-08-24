# SetOpMode 최우선 개발 설계

- 대상: No.33 `MMC_ChngOpMode`
- 현재 진행도: 25%
- current 상태: `Dormant route + owner ABI frozen`; PC/SDK contract와 LASAL deterministic failure route 구현, Control owner/runtime executor는 `Missing`
- 구현된 SDK/LASAL command: `0x7D23 Start`, `0x7D24 ReadOutcome`, `0x7D25 Retire`
- 1차 activation 범위: physical axis 1..4, Immediate, CSP mode 8만
- 진행 판정: dormant source checkpoint와 owner/resource numeric ABI freeze 완료; CSP runtime/recovery와 실축 적격화는 미완료
- 구현 동기화 기준: 2026-08-24

## 1. 정확한 API 의미

No.33은 단순 `MMC_ChngOpMode`다. 입력은 DS402 operation mode 하나이며
`MMC_ChangeOpModeEx`의 initial value, execution mode와 queue 의미는 이번 범위가 아니다.

Maestro reference는 mode 1(Profile Position), 3(Profile Velocity partial), 6(Homing),
7(Interpolated Position)을 설명한다. current SIGMATEK/LMC image는 정상 motion과 DS402 Home
cleanup에서 CSP mode 8을 전제로 한다. 따라서 `0x6060` unrestricted SDO Write나 모든 positive
mode 허용으로 구현하면 안 된다.

현재 C#에는 기존 `0x6061:0 Int8/1` read API와 별도로 SetOperationMode의 immutable
prepare/start/query/retire SDK contract가 있다. LASAL에는 `0x7D23/0x7D24/0x7D25` TCP route와
`LMCDiagnosticsService`의 Start/Outcome/Retire deterministic failure handler도 들어가 있다.
`AxisOperationMode`의 numeric owner/resource ABI는 이 문서에서 동결했지만, 그 상수와 admission
validation을 `LMCControlCommandService`/`LMCDiagnosticsService` source에 반영하는 작업,
`0x6061 -> 0x6060 -> 0x6061` executor와 durable outcome store는 아직 없다. capability bits
8/9/10도 current PLC에서 광고하지 않는다. 따라서 public Start는 capability preflight에서 wire
송신 전에 차단되며, raw well-shaped request를 직접 보내도 dormant handler가 fail-closed
response만 반환한다. `0x6060/0x6061` PDO도 current Elmo object에서 disabled다.

## 2. 1차 지원 범위

첫 구현은 lifecycle과 recovery를 완성하되 public activation은 CSP 8만 허용한다.

- physical axis 1..4
- Immediate-only
- requested mode `8`만
- 이미 `0x6061=8`이면 terminal `SucceededNoWrite`
- 다른 mode에서 8로 복구할 때만 exact one-byte `0x6060:0=8` Write
- 움직임, Fault, pending motion, 다른 mutation owner 또는 SDO owner가 있으면 거부
- Homing mode 6은 `HomeDS402/HomeDS402Ex` 내부 owner만 사용하고 public SetOpMode에서 거부

Mode 1/3/7은 해당 mode의 setpoint PDO/controller, `_LMCAxis` output 정지·인계와 physical
proof가 마련될 때 각각 별도 activation한다. 초기 구현에서 이를 광고하지 않는다.

## 3. wire 설계

### 3.1 Start `0x7D23`

Admin payload는 56 bytes로 고정한다. 8-byte TCP/RPC header를 포함한 complete frame은
64 bytes다. 아래 `P` offset은 Admin payload 시작 기준이다.

| Offset | Type | Field |
|---:|---|---|
| P8 | U16 | SchemaVersion = 1 |
| P10 | U16 | Reserved = 0 |
| P12 | U32 | ExpectedDiagnosticsBuild, nonzero |
| P16 | U32 | ExpectedDiagnosticsBootId, nonzero |
| P20 | U32 | ExpectedMapRevision, nonzero |
| P24 | U32 | DuplicatedRequestId, common P4와 동일하고 nonzero |
| P28..P40 | 4 x U32 | ClientIntentId, 전체가 0이면 거부 |
| P44 | U16 | AxisReference, outer Reference와 동일 |
| P46 | I8 | RequestedMode |
| P47 | U8 | Reserved = 0 |
| P48 | U32 | TimeoutMilliseconds, nonzero |
| P52 | U32 | Flags = 0, Immediate-only |

성공 ACK는 24 bytes이며 P16에 requested mode를 DINT로 echo하고,
P20 NativeCommandState는 U32 0으로 고정한다. SetOperationMode는 SDO executor를 쓰며 native
axis-command bitfield를 이 필드에 싣지 않는다. well-shaped Start의 domain success/failure는 24 bytes이고 malformed common-frame
failure는 16 bytes다. ACK는 terminal이 아니다.

### 3.2 query와 retire

복구 key는 다음 전체 tuple이다.

`Schema + Build + BootId + MapRevision + OriginalRequestId + ClientIntentId[4] + Axis +
RequestedMode + Timeout + Flags`

- `0x7D24` request: payload 56 bytes, complete frame 64 bytes
- `0x7D25` request: 같은 key + P56 nonzero `ExpectedRecordGeneration`, payload 60 bytes,
  complete frame 68 bytes
- query와 retire는 `0x6060`을 쓰지 않고 original Start를 replay하지 않는다.

| Query offset | Type | Field |
|---:|---|---|
| P8/P10 | U16/U16 | SchemaVersion=1 / Reserved=0 |
| P12/P16/P20 | U32 | build/BootId/MapRevision |
| P24 | U32 | Original Start RequestId |
| P28..P40 | 4 x U32 | ClientIntentId |
| P44 | U16 | AxisReference |
| P46/P47 | I8/U8 | RequestedMode / Reserved=0 |
| P48/P52 | U32/U32 | TimeoutMilliseconds / Flags=0 |
| P56 | U32 | Retire에만 존재하는 ExpectedRecordGeneration |

Outcome success response는 112 bytes로 고정한다.

| Offset | Type | Field |
|---:|---|---|
| P16/P18 | U16/U16 | RecordState / Reserved=0 |
| P20..P32 | 4 x U32 | build/BootId/map/original RequestId |
| P36..P48 | 4 x U32 | ClientIntentId |
| P52 | U16 | AxisReference |
| P54/P55 | I8/I8 | RequestedMode / ObservedMode |
| P56/P60 | U32/U32 | TimeoutMilliseconds / Flags=0 |
| P64/P66 | U16/I16 | OriginalCommandStatus / OriginalErrorId |
| P68 | U32 | OriginalDetailCode |
| P72/P76 | U32/U32 | SdoExecutorToken / EvidenceFlags |
| P80/P84 | U32/U32 | StartCycle / CompletionCycle |
| P88/P92 | U32/U32 | NativeCommandState = 0 / RecordGeneration |
| P96 | I8 | PreviousMode |
| P97..P99 | 3 bytes | Reserved=0 |
| P100 | U32 | QuarantineReason |
| P104/P106 | U16/U16 | DS402StatusWord / Reserved=0 |
| P108 | U32 | ContextCheck |

RecordState는 1 Running, 2 Succeeded, 3 Failed, 4 Aborted다. Indeterminate/quarantine은
Succeeded response로 노출하지 않고 query detail 46으로 unresolved 상태를 보존한다.

`EvidenceFlags`는 6060 write requested/dispatched, 6061 verify dispatched/completed, owner
released와 executor reusable을 포함한다. Running은 CompletionCycle=0이고 terminal은
CompletionCycle>=StartCycle이어야 한다. Query/Retire domain failure는 16-byte common
envelope만 반환한다.

ErrorCatalogVersion 6 후보 detail은 `43 unsupported mode`, `44 unsafe mode state`,
`45 outcome not found`, `46 outcome indeterminate`, `47 store corrupt`, `48 exact-key mismatch`,
`49 storage unavailable`, `50 runtime execution failed`, `51 outcome slot occupied`로 예약한다.
owner conflict/quarantine는 기존 41/42를 재사용한다.

## 4. 상태 머신

```text
Accepted
  -> OwnerReserve
  -> Preflight6061Read
     -> AlreadyTarget -> SucceededNoWrite
     -> ValidateStandstill/PowerOff/NoPendingMutation
  -> Write6060Requested
  -> Write6060Dispatched
  -> Verify6061Read
     -> ExactMatch -> TerminalCandidate
     -> DefinitivePreWriteFailure -> FailedSafe
     -> Timeout/LostCallback/AmbiguousWrite -> ReadOnlyRecovery
          -> ExactMatch -> TerminalCandidate
          -> Otherwise -> IndeterminateQuarantined
  -> TerminalCommit/Readback
  -> OwnerRelease
  -> Response
```

핵심 규칙:

- 6060 Write가 dispatch 된 뒤에는 같은 intent라도 다시 쓰지 않는다.
- reconnect/recovery는 `0x6061` read만 사용한다.
- observed mode가 requested mode와 같아야 success를 commit한다.
- terminal commit/readback 전 owner release와 response를 금지한다.
- indeterminate record는 operator recovery 전까지 해당 축의 mutation을 막는다.
- terminal exact-generation retire만 record와 owner를 정리할 수 있다.

## 5. mode ownership

전용 `AxisOperationMode` owner kind를 추가하고 기존 Diagnostics SDO resource를 공유해 다음과
충돌시킨다.

- Axis/Group motion, Power, Stop과 Reset의 active mutation
- HomeDS402와 HomeDS402Ex
- SetPosition
- Encoder maintenance
- generic D5 SDO operation

### 5.1 AxisOperationMode owner/resource numeric ABI freeze

MODE-06 source 구현 전에 다음 numeric ABI를 고정한다.

| 항목 | 고정값 | source 이름/의미 |
|---|---:|---|
| OwnerKind | `6` | `LMC_OWNER_KIND_AXIS_OPERATION_MODE`, Diagnostics mirror도 `6` |
| ResourceKind | `4` | 기존 `LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE` / `LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO` 공유 |
| AdmissionMode | `4` | 기존 `LMC_OWNER_ADMISSION_LIFECYCLE` / `LMC_DIAG_ADMISSION_LIFECYCLE` |
| Start Command | `0x7D23` | `AxisSetOperationModeStart` |
| Active owner state | `12` | 신규 `LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE` |
| Reference | `1..4` | physical axis only |
| Axis mask | `1 << (Reference-1)` | exact single-axis mask만 허용 |

resource 4를 공유하는 이유는 current Control의 resource 4가 이미
`DIAGNOSTICS_SDO_ENGINE`으로 정의돼 있고 singleton token/generation/axis-mask를 소유하며,
`LMCDiagnosticsService`가 같은 축 1..4 `LMCSdoExecutor` client를 통해 Encoder maintenance와
SetOperationMode를 실행하기 때문이다. 같은 실행 자원을 새 resource 5로 이중 표현하면 두
lifecycle owner가 서로 다른 singleton lock을 획득해 같은 Diagnostics SDO execution domain을
동시에 사용할 수 있는 여지를 만든다. 1차 구현은 resource 4를 공유해 Encoder maintenance와
SetOperationMode를 전역 직렬화한다. 축별 병렬화가 필요하면 runtime evidence와 executor
reentrancy를 별도 증명한 뒤 resource 모델 자체를 다시 설계한다.

OwnerKind는 Encoder maintenance와 lifecycle/outcome 의미가 다르므로 `5`를 재사용하지 않고
`6`으로 분리한다. Control의 current owner-kind upper bound와 command/resource tuple validator,
commit/validate state mapping, safety-preemption special-owner 목록은 MODE-06 source에서 이 값을
명시적으로 추가한다. `AxisOperationMode`는 좌표 원점을 바꾸지 않으므로 Encoder maintenance와
달리 AxisRebaseRequired barrier를 set/clear하지 않는다.

resource 4 ownership만으로 generic D5 operation까지 자동 배제된다고 간주하지 않는다.
`LMCDiagnosticsService`의 generic D5 operation state가 idle인지와 해당 `SdoAxisN` executor가
reusable인지 preflight에서 별도로 확인하고, generic D5 Write policy의 permanent unsafe object에
`0x6060`을 추가한다. capability bits 8/9/10은 이 ABI가 source에 반영돼도 계속 OFF다.

축의 current mode가 8이 아니거나 mode outcome이 unresolved이면 ordinary LMC motion을
승인하지 않는다. PLC startup에서는 6061을 read-only로 확인하고 자동으로 6060을 쓰지 않는다.

SetOpMode는 `LMCDiagnosticsService`의 Home SDO 단계 처리 패턴을 재사용하되 별도 state와
outcome record를 가진다. generic D5 Write의 permanent unsafe object 목록에도 `0x6060`을
추가한다.

## 6. capability

신규 Admin capability는 다음 triad로 예약한다.

- bit 8 `AxisSetOperationModeStart`
- bit 9 `AxisSetOperationModeOutcomeRead`
- bit 10 `AxisSetOperationModeOutcomeRetire`

세 bit는 indivisible하다. SDK는 세 bit가 모두 없으면 Start를 송신하지 않는다. current strict
capability parser와 PLC를 paired 배포하며, C78/PLC/hardware gate 전까지 compile-time flag와
세 bit를 모두 OFF로 둔다.

## 7. 변경 대상

### C#

- 구현 완료: `LmcAxisSetOperationMode.cs`, `LmcAdminSetOperationMode.cs`
- 구현 완료: SetOperationMode request/ack/outcome/retirement protocol·model 파일
- 등록 완료: `LmcProtocol.cs`, `LmcAdminModels.cs`, `LmcAdminProtocol.cs`,
  `LmcResponsePayloadLimits.cs`, `LmcErrorCatalog.cs`
- 계약 시험: `AdminSetOperationModeContractTests.cs`
- `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`

### LASAL

- dormant route 구현 완료: `Class/TCPMotionInterface/TCPMotionInterface.st`
- dormant handler 구현 완료: `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`
- owner ABI 설계 동결, source 반영 미완료: `Class/LMCControlCommandService/LMCControlCommandService.st`
- 필요 declaration은 IDE에서 생성하고 implementation은 추적 `.st`에서 작성

### 7.1 MODE-05 PLC route freeze

SetOperationMode는 `LMCControlCommandService`의 full-frame Admin handler에 임시로 넣지 않는다.
`0x6060/0x6061` SDO executor와 axis ownership client를 이미 가진
`LMCDiagnosticsService`가 payload-only handler를 소유한다. TCP는 `RequestBuf[8]`부터의
payload와 `Sendbuf[8]`부터의 response를 Diagnostics에 전달한다. MODE-05에서는 owner reserve,
SDO submit, motion/native call과 outcome-store 접근이 모두 0회이며, capability bits 8/9/10도
계속 OFF다.

LASAL IDE에서 생성할 private ABI는 다음과 같다. `GLOBAL`과 `VIRTUAL`은 붙이지 않는다.

- `HandleAxisSetOperationModeStart`
  - input 순서: `Reference : UINT`, `pRequest : ^USINT`, `pResponse : ^USINT`,
    `ResponseCapacity : UDINT`, `CallerSessionEpoch : UDINT`,
    `RequestSequence : UDINT`, `AdmissionToken : UDINT`,
    `OwnerGeneration : UDINT`, `RequestSize : UDINT`
  - output: `ResponseSize : DINT`
- `HandleAxisSetOperationModeOutcome`
  - input 순서: `Reference : UINT`, `pRequest : ^USINT`, `pResponse : ^USINT`,
    `ResponseCapacity : UDINT`, `CallerSessionEpoch : UDINT`,
    `RequestSize : UDINT`
  - output: `ResponseSize : DINT`
- `HandleAxisSetOperationModeRetire`
  - input/output ABI는 Outcome과 동일
- `ProcessAxisSetOperationMode`
  - input/output 없음
  - MODE-05에서는 호출하지 않는 empty scaffold이며, MODE-06의 별도 cyclic processor로 사용

MODE-05 dormant response는 malformed detail 1..5를 payload 16 bytes로 반환한다.
well-shaped Start는 requested-mode DINT echo와 `NativeCommandState=0`을 포함한 payload
24 bytes로 detail 43(unsupported mode) 또는 detail 49(storage unavailable)를 반환한다.
well-shaped Outcome/Retire는 payload 16 bytes와 detail 49를 반환한다. 이 단계에서는
성공 ACK, 112-byte outcome, `0x6060` write와 record retirement가 존재하면 안 된다.

MODE-06 source는 5.1의 frozen ABI를 그대로 사용한다. `OwnerKind=6`,
`ResourceKind=4`, `AdmissionMode=4`, active state `12`를 다른 값으로 임의 재배정하거나
resource 5를 추가하지 않는다. 현재 MODE-05 route는 계속 `AdmissionToken=0`,
`OwnerGeneration=0`으로만 동작하며 `ReserveAxisOwnership`을 호출하지 않는다.

### WPF recovery

- 신규 `AxisSetOperationModeRecoveryJournal.cs`
- `MainWindow.xaml`/`MainWindow.xaml.cs`의 explicit confirmation과 unresolved interlock
- wire write 전에 exact endpoint/build/BootId/map/intent/mode를 journal에 저장
- startup/reconnect recovery는 `0x6061` read와 `0x7D24` query만 사용하고 6060 Write를 replay하지 않음
- exact terminal/generation 저장 후 `0x7D25` retire 성공 뒤에만 resolve
- journal unit test와 MainWindow smoke test

## 8. 작업 체크리스트

- [x] `MODE-01` No.33을 Immediate-only `MMC_ChngOpMode`로 고정하고 Ex를 분리
- [ ] `MODE-02` command ID/capability 중앙 등록과 owner/resource numeric ABI freeze 완료; Control/Diagnostics source 상수·validator 반영 미완료
- [x] `MODE-03` immutable request/ack/outcome/recovery model과 sync/async API 구현
- [x] `MODE-04` exact golden bytes, malformed parser와 capability-off zero-wire test 구현
- [x] `MODE-05` Start/ReadOutcome/Retire LASAL parser와 dormant deterministic failure 구현
- [ ] `MODE-06` 6061 preflight -> 6060 write -> 6061 verify state machine 구현
- [ ] `MODE-07` write-dispatch 이후 no-replay와 read-only recovery 구현
- [ ] `MODE-08` Home/SetPosition/motion/generic SDO ownership conflict 구현
- [ ] `MODE-09` generic D5 permanent unsafe object에 `0x6060` 추가
- [ ] `MODE-10` C78, SourceOnly, method-size와 fault mutation matrix 통과
- [ ] `MODE-11` CSP same-mode no-write와 exact one-write/readback packet 검증
- [ ] `MODE-12` 축 1~4 timeout/disconnect/mismatch/quarantine/retire 검증
- [ ] `MODE-13` WPF pre-dispatch journal/startup no-replay recovery와 smoke test PASS
- [ ] `MODE-14` capability bits 8/9/10 paired activation

### 8.1 2026-08-20 PC/SDK historical checkpoint

- VS2019 MSBuild Debug/Release 전체 PC suite: 각각 `1164/1164 PASS`, warning 0, error 0
- SetOperationMode 신규 계약 시험: 11개 PASS
- 독립 Debug 전체 실행에서 기존 GroupDisableWait compound test가 한 번 실패했으나 같은
  tree의 즉시 재실행은 PASS했다. SetOperationMode 11개에는 실패가 없었고, 해당 기존
  간헐 실패는 별도 안정화 대상으로 남긴다.
- 이 checkpoint에서 PC가 증명한 것은 immutable contract, exact frame/parser,
  capability-off zero-wire와 one-shot/no-replay API 경계다.
- 아래 MODE-05 source checkpoint가 이후 추가되었으므로 이 항목의 "PC-only" 범위를 current
  LASAL route 상태로 확대 해석하지 않는다.

### 8.2 MODE-05 current source/build checkpoint

- `TCPMotionInterface` Diagnostics route와 `LMCDiagnosticsService` private handler 3개가
  `0x7D23/0x7D24/0x7D25`를 수신한다.
- 정형 Start는 detail 43 또는 49의 24-byte failure ACK와 `NativeCommandState=0`만 반환한다.
  정형 Outcome/Retire는 detail 49의 16-byte failure만 반환한다.
- owner reservation, SDO submit/read/write, motion/native call, outcome record와 capability bits
  8/9/10은 모두 OFF다. 따라서 raw route가 존재해도 SetOperationMode runtime mutation은 0회다.
- owner/resource numeric ABI는 문서상 `OwnerKind=6`, shared Diagnostics SDO `ResourceKind=4`,
  `AdmissionMode=4`, active state `12`로 동결했지만 current Control/Diagnostics source에는 아직
  이 tuple이 추가되지 않았다. 이를 source 구현 완료 증거로 해석하지 않는다.
- method-size checkpoint는 112 methods, 109 under-limit, baseline debt 3으로 통과했다.
- latest C78/ARM Rebuild/Link는 `0 errors / 79 warnings`, `Linker Done`이고 PLC
  link/download/SystemInit/project load까지 성공했다. 이 image에 dormant route가 포함되어도
  `6061 -> 6060 -> 6061` runtime executor와 hardware proof가 생기는 것은 아니다.
- current `Classes.lcb`는 `8,610,206` bytes / SHA-256
  `568FE55148D734BE4DB0BB5ED9AF4D7800DB33672A5FCE21ECCFE15EE3CAC5A7`이며, 기존 artifact
  ratchet과 identity가 달라 UDP VerifyCurrent와 이를 포함한 full SourceOnly는 현재 STOP이다.
  따라서 `MODE-10`은 아직 완료로 표시하지 않는다.
- SetOperationMode packet/hardware 정상·fault·reconnect proof는 아직 없다.

## 9. 비-CSP 후속 gate

Mode 1/3/7을 열려면 mode별 PDO, setpoint producer, controlword owner, `_LMCAxis` output 인계,
Stop/Power/fault/restart와 mode 8 복귀 계약을 별도로 구현한다. 이 결정 전에는 진행도가 올라가도
`CSP=8 only` 제한을 특이사항에서 제거하지 않는다. CSP recovery-only tranche 완료를
`MMC_ChngOpMode` 전체 구현 또는 75% 완료로 기록하지 않는다.

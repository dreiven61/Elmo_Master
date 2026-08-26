# SetOpMode 최우선 개발 설계

- 대상: No.33 `MMC_ChngOpMode`
- 현재 진행도: 60%
- current baseline: `dev@52bd4cc120812c2510f8ac99d2d6a42576133d67`
- current 상태: `Dormant runtime source`; PC/SDK contract, owner/runtime, no-replay recovery, safety preemption, generic D5 0x6060 차단과 MODE-13 WPF durable recovery 구현
- 구현된 SDK command: `0x7D23 Start`, `0x7D24 ReadOutcome`, `0x7D25 Retire`
- 구현된 PLC route/runtime: Diagnostics route, owner kind/resource, `6061 -> 6060 -> 6061`, outcome lifecycle, write-dispatch 이후 read-only recovery, safety-preemption cleanup
- 1차 activation 범위: physical axis 1..4, Immediate, CSP mode 8만
- activation 상태: `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`, capability bits 8/9/10 OFF
- 진행 판정: MODE-02/06/07/08/09 source 완료, MODE-10 source/static PASS, MODE-13 PC/WPF PASS; current exact-image C78/PLC/hardware는 미완료
- open qualification branch: PR #18 `codex/setopmode-mode11-bench-activation` — `DO NOT MERGE`, physical bench evidence 전용
- WPF follow-up: PR #37 dynamic recovery-panel localization/CI coverage는 미병합이며 activation 근거가 아님

## 1. 정확한 API 의미

No.33은 단순 `MMC_ChngOpMode`다. 입력은 DS402 operation mode 하나이며
`MMC_ChangeOpModeEx`의 initial value, execution mode와 queue 의미는 이번 범위가 아니다.

Maestro reference는 mode 1(Profile Position), 3(Profile Velocity partial), 6(Homing),
7(Interpolated Position)을 설명한다. current SIGMATEK/LMC image는 정상 motion과 DS402 Home
cleanup에서 CSP mode 8을 전제로 한다. 따라서 `0x6060` unrestricted SDO Write나 모든 positive
mode 허용으로 구현하면 안 된다.

현재 C#에는 기존 `0x6061:0 Int8/1` read API와 별도로 SetOperationMode의 immutable
prepare/start/query/retire SDK contract가 있다. LASAL에는 `0x7D23/0x7D24/0x7D25` route와
handler, `AxisOperationMode` owner, 전용 outcome state와 `0x6060/0x6061` SDO executor runtime이
구현돼 있다. public activation은 아직 하지 않는다. capability bits 8/9/10과
`LMC_DIAG_SET_OPERATION_MODE_ENABLED`는 C78/PLC/hardware 검증 완료 전까지 OFF로 유지한다.
`0x6060/0x6061` PDO도 current Elmo object에서 disabled다.

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
axis-command bitfield를 이 필드에 싣지 않는다. well-shaped Start의 domain success/failure는
24 bytes이고 malformed common-frame failure는 16 bytes다. ACK는 terminal이 아니다.

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

`EvidenceFlags`는 현재 SDK/PLC contract에서 다음 bit를 사용한다.

- bit0 `WriteRequested`
- bit1 `WriteDispatched`
- bit2 `VerifyReadDispatched`
- bit3 `VerifyReadCompleted`
- bit4 `OwnerReleased`
- bit5 `ExecutorReusable`

Running은 CompletionCycle=0이고 terminal은 CompletionCycle>=StartCycle이어야 한다.
terminal outcome은 owner released + executor reusable 증거를 요구한다. Success는 추가로
ObservedMode=RequestedMode와 verify read 증거를 요구한다. Query/Retire domain failure는
16-byte common envelope만 반환한다.

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

current PLC source는 위 상태 머신과 no-replay recovery를 구현한다.

- 6060 Write가 dispatch 된 뒤에는 같은 intent라도 다시 쓰지 않는다.
- warm-start/reconnect recovery는 dispatch evidence가 있는 exact Running record만 복구 후보로 삼는다.
- dispatch 이후 복구는 `0x6061` read-only 경로만 사용한다.
- observed mode가 requested mode와 같아야 success를 commit한다.
- terminal payload를 stage/readback한 뒤에만 owner release를 수행하고 RecordState를 terminal로 바꾼다.
- indeterminate record는 quarantine으로 보존한다.
- terminal exact-generation retire만 record를 제거한다.

### 4.1 MODE-08 safety preemption

`Stop/Power` 계열 safety owner가 `AxisOperationMode` owner를 preempt할 수 있도록 Control의
preempted-owner validation과 Diagnostics processor의 cleanup path를 연결한다.

current source 원칙은 다음과 같다.

- `AxisOperationMode`는 owner kind 6, active state 12, command `0x7D23`, identity 56 bytes다.
- safety preemption snapshot은 exact old owner identity와 safety replacement identity를 검증한다.
- preemption pending-freeze 동안 SetOperationMode local stage를 진행하지 않는다.
- `0x6060` irreversible-dispatch evidence가 있으면 safety cleanup 중 새 `0x6060` write를 시작하지 않는다.
- outstanding executor token은 drain/reuse 여부를 확인하고, uncertainty는 quarantine으로 보낸다.
- cleanup publication은 common ownership preemption receipt를 사용한다.

### 4.2 MODE-10 method split

`ProcessAxisSetOperationMode`의 LASAL 32 KiB 제한을 피하기 위해 current source는 세 method로
분리한다.

- `ProcessAxisSetOperationMode`: warm-start/identity, MODE-08 preemption, activation-OFF,
  timeout/no-replay normalization과 helper dispatch
- `ProcessAxisSetOperationModeMutationStages`: PREFLIGHT/WRITE/VERIFY
- `ProcessAxisSetOperationModeRecoveryStages`: RECOVERY/TERMINAL/QUARANTINE

`0x6060` write site는 mutation helper에만 존재하고 recovery helper에는 없다. 상세 분할 규칙과
qualification evidence는
[MODE-10 method split 설계](SET_OPERATION_MODE_MODE10_METHOD_SPLIT_DESIGN.md)를 따른다.

## 5. mode ownership

전용 owner는 기존 diagnostics SDO singleton resource를 공유한다.

- owner kind: `LMC_OWNER_KIND_AXIS_OPERATION_MODE = 6`
- resource: `LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE = 4`
- lifecycle admission: 4
- active state: `LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE = 12`
- Start command: `0x7D23`, identity size 56

다음 mutation과 충돌한다.

- Axis/Group motion, Power, Stop과 Reset의 active mutation
- HomeDS402와 HomeDS402Ex
- SetPosition
- Encoder maintenance
- generic D5 SDO operation

동일 축의 ordinary motion/Home/SetPosition은 common axis ownership으로 상호 배제한다.
Encoder maintenance는 동일 diagnostics SDO resource 4를 공유해 singleton에서 상호 배제한다.
Safety Stop/Power는 kind6 owner를 preempt하고 정확한 cleanup/quarantine receipt를 남긴다.

MODE-09에서 generic D5 Write policy의 permanent unsafe object 목록에 `0x6060`을 추가했다.
따라서 generic D5가 SetOperationMode owner/outcome/no-replay lifecycle을 우회해 operation mode를
직접 쓰는 경로는 source에서 차단한다.

축의 current mode가 8이 아니거나 mode outcome이 unresolved이면 ordinary LMC motion을
승인하지 않는 interlock은 activation 전에 실기 검증한다. PLC startup에서는 6061을
read-only로 확인하고 자동으로 6060을 쓰지 않는다.

SetOpMode는 `LMCDiagnosticsService`의 SDO executor를 사용하되 별도
`AxisOperationModeState : ARRAY [0..191] OF DINT` outcome/runtime state를 가진다.

## 6. capability

신규 Admin capability는 다음 triad로 예약한다.

- bit 8 `AxisSetOperationModeStart`
- bit 9 `AxisSetOperationModeOutcomeRead`
- bit 10 `AxisSetOperationModeOutcomeRetire`

세 bit는 indivisible하다. SDK는 세 bit가 모두 없으면 Start를 송신하지 않는다. current strict
capability parser와 PLC를 paired 배포하며, C78/PLC/hardware gate 전까지 compile-time flag와
세 bit를 모두 OFF로 둔다.

현재 PLC source에는 `LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE`가 존재한다. OFF build에서도
이미 dispatch된 Running record의 read-only recovery/cleanup은 허용하되 신규 mutation은 시작하지
않는 것이 activation gate의 의미다.

## 7. 변경 대상

### C#

- 구현 완료: `LmcAxisSetOperationMode.cs`, `LmcAdminSetOperationMode.cs`
- 구현 완료: SetOperationMode request/ack/outcome/retirement protocol·model 파일
- 등록 완료: `LmcProtocol.cs`, `LmcAdminModels.cs`, `LmcAdminProtocol.cs`,
  `LmcResponsePayloadLimits.cs`, `LmcErrorCatalog.cs`
- 계약 시험: `AdminSetOperationModeContractTests.cs`
- recovery identity fence: Build/BootId/MapRevision exact match, mismatch zero-wire
- `LMC_Library/LMC_API_Delivery/docs/DINT_PACKET_MAP.txt`

### LASAL

- `Class/TCPMotionInterface/TCPMotionInterface.st`
- `Class/LMCControlCommandService/LMCControlCommandService.st`
- `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`
- IDE 생성 declaration: `AxisOperationModeState : ARRAY [0..191] OF DINT`
- IDE 생성 private helper declaration:
  `ProcessAxisSetOperationModeMutationStages`, `ProcessAxisSetOperationModeRecoveryStages`
- implementation은 추적 `.st`의 `//{{LSL_IMPLEMENTATION` 이후에서 유지

### 검증

- `tools/Verify-SetOperationModeStatic.ps1`
- `LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1`
- `.github/workflows/set-operation-mode-static-qualification.yml`
- `.github/workflows/set-operation-mode-wpf-recovery.yml`
- [MODE-13 WPF recovery evidence](evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md)

### 7.1 MODE-05 PLC route freeze — historical checkpoint

SetOperationMode는 `LMCControlCommandService`의 full-frame Admin handler에 임시로 넣지 않는다.
`0x6060/0x6061` SDO executor와 axis ownership client를 가진 `LMCDiagnosticsService`가
payload-only handler를 소유한다. TCP는 `RequestBuf[8]`부터의 payload와 `Sendbuf[8]`부터의
response를 Diagnostics에 전달한다.

MODE-05 당시에는 owner reserve, SDO submit, motion/native call과 outcome-store 접근이 0회이고
capability bits 8/9/10도 OFF였다. 아래 private ABI는 이후 runtime implementation에서도 그대로
유지한다.

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
  - MODE-06부터 cyclic processor로 사용

### 7.2 MODE-06/07 PLC runtime checkpoint

- `AxisOperationMode` numeric owner ABI는 kind 6/resource 4로 freeze됐다.
- `LMCControlCommandService`가 `0x7D23` exact identity 56과 lifecycle admission을 validate/commit한다.
- `LMCDiagnosticsService`는 4축 x 32-DINT record와 32-DINT runtime 영역을 사용한다.
- Start는 `6061` preflight 후 same-mode면 no-write success, 아니면 standstill/power-off/owner/SDO
  안전 조건을 확인한 뒤 exact one-byte `6060=8` write를 시도한다.
- `READY`만 irreversible dispatch evidence로 기록한다.
- write dispatch 이후 timeout/callback uncertainty는 `6061` read-only recovery로만 이동한다.
- warm-start는 write-dispatched Running record만 reconstruct한다.
- terminal candidate는 payload/readback 후 common owner release를 수행한다.
- terminal outcome은 exact 112 bytes이며 terminal retire는 같은 112-byte outcome을 반환한 뒤 record를 비운다.
- compile-time activation flag와 capability triad는 계속 OFF다.

### WPF recovery — MODE-13 PASS

- `AxisSetOperationModeRecoveryJournal.cs` durable exact-key journal
- dynamic WPF controller의 explicit one-shot confirmation과 unresolved interlock
- wire write 전에 exact endpoint/build/BootId/map/intent/mode를 journal에 저장
- startup/reconnect recovery는 `0x7D24` query와 read-only observation만 사용하고 original `0x7D23`/`0x6060`을 replay하지 않음
- exact terminal/generation 저장 후 `0x7D25` retire 성공 뒤에만 resolve
- deterministic Start rejection은 PLC가 retained outcome을 publish하기 전 반환한다는 source 의미를 적용
- definitive rejection은 exact identity/response와 당시 active journal bytes/hash를 checksum-protected evidence에 write-through 저장한 뒤 interlock 해제
- rejection identity mismatch 또는 evidence write 실패는 fail-closed
- journal unit test, SDK recovery identity test와 MainWindow smoke test PASS

MODE-13 PASS는 PC/WPF 증거 등급이며 C78/PLC/hardware activation 근거가 아니다.

## 8. 작업 체크리스트

- [x] `MODE-01` No.33을 Immediate-only `MMC_ChngOpMode`로 고정하고 Ex를 분리
- [x] `MODE-02` command ID/capability bit와 PLC owner kind 6/resource 4 ABI freeze/source 반영
- [x] `MODE-03` immutable request/ack/outcome/recovery model과 sync/async API 구현
- [x] `MODE-04` exact golden bytes, malformed parser와 capability-off zero-wire test 구현
- [x] `MODE-05` Start/ReadOutcome/Retire LASAL parser와 dormant deterministic failure 구현
- [x] `MODE-06` 6061 preflight -> 6060 write -> 6061 verify state machine 구현
- [x] `MODE-07` write-dispatch 이후 no-replay와 read-only recovery 구현
- [x] `MODE-08` Home/SetPosition/motion/SDO ownership conflict와 safety preemption source 구현
- [x] `MODE-09` generic D5 permanent unsafe object에 `0x6060` 추가
- [ ] `MODE-10` source/static PASS; PR #17 fresh C78 artifact checkpoint는 존재하지만 current `dev` exact-source C78/PLC/fault matrix는 재검증 필요
- [ ] `MODE-11` CSP same-mode no-write와 exact one-write/readback packet 검증 — PR #18 software bench tooling 준비, physical evidence 미완료
- [ ] `MODE-12` 축 1~4 timeout/disconnect/mismatch/quarantine/retire 검증
- [x] `MODE-13` WPF pre-dispatch journal/startup no-replay recovery와 smoke test PASS
- [ ] `MODE-14` capability bits 8/9/10 paired activation

### 8.1 2026-08-20 PC/SDK checkpoint

- VS2019 MSBuild Debug/Release 전체 PC suite: 각각 `1164/1164 PASS`, warning 0, error 0
- SetOperationMode 신규 계약 시험: 11개 PASS
- 독립 Debug 전체 실행에서 기존 GroupDisableWait compound test가 한 번 실패했으나 같은
  tree의 즉시 재실행은 PASS했다. SetOperationMode 11개에는 실패가 없었고, 해당 기존
  간헐 실패는 별도 안정화 대상으로 남긴다.
- PC가 구현한 것은 immutable contract, exact frame/parser, capability-off zero-wire와
  one-shot/no-replay API 경계다.
- 이 checkpoint 자체는 runtime activation 근거가 아니다.

### 8.2 MODE-05 source checkpoint — historical

- `TCPMotionInterface` Diagnostics route와 `LMCDiagnosticsService` private handler 3개가
  `0x7D23/0x7D24/0x7D25`를 수신한다.
- MODE-05 당시 정형 Start는 detail 43 또는 49의 24-byte failure ACK와
  `NativeCommandState=0`만 반환했고, Outcome/Retire는 detail 49였다.
- 이후 MODE-06/07에서 이 dormant handler를 실제 owner/SDO/outcome runtime으로 확장했다.
- MODE-05 method-size checkpoint는 112 methods, 109 under-limit, baseline debt 3이었다.

### 8.3 MODE-06/07 historical C78/PLC checkpoint

- LASAL declaration `AxisOperationModeState : ARRAY [0..191] OF DINT`를 IDE에서 생성했다.
- `ReserveAxisOwnership`의 diagnostics SDO tuple 판정은 LASAL parser 호환을 위해
  `diagnosticsSdoTupleValid` BOOL 분기로 단순화했다.
- 이전 MODE-06 source는 C78 build, PLC download와 기본 정상동작을 통과한 기록이 있다.
- MODE-07 source는 `WriteDispatched` 이후 `WRITE_START`로 복귀하지 않는 guard와
  `RECOVERY_START/RECOVERY_WAIT` read-only path를 추가했다.
- 이 historical C78/PLC 결과를 current MODE-08/09/10 source의 fresh build 증거로 사용하지 않는다.

### 8.4 MODE-08/09 current source checkpoint

- Control의 preempted-owner surface가 kind6/command `0x7D23`/resource4/identity56/active-state12를
  인식한다.
- Diagnostics processor는 preemption snapshot을 읽고 exact old/safety tuple을 검증하며
  pending-freeze/cleanup/quarantine을 처리한다.
- write-dispatched state에서는 safety preemption 처리 중 새 `0x6060` mutation을 시작하지 않는다.
- generic D5 `GetSdoWritePolicyDetail`은 `0x6060`을 permanent deny(detail 8)한다.

### 8.5 MODE-10 source/static qualification checkpoint

qualification branch에서 current source semantics를 다음까지 검증했고 해당 canonical source와
verifier를 `dev`에 복구했다.

- generated declaration 변경 없음
- main -> mutation/recovery helper dispatch 각각 1개
- `0x6060` write site: main 0 / mutation 4 / recovery 0
- method size: main 19,895 bytes, mutation 19,731 bytes, recovery 14,251 bytes(LF 기준)
- `Verify-SetOperationModeStatic.ps1`: 57 checks PASS
- common ownership/source ratchets PASS
- LASAL source 7-bit ASCII PASS
- `git diff --check` PASS
- full SourceOnly는 source/static gate를 통과하고 기존 `Classes.lcb` physical identity ratchet에서만 STOP

따라서 현재 판정은 `MODE-10 source/static PASS`, `current exact-image IDE/artifact 이후 미완료`다.
PR #17에서 fresh C78 artifact checkpoint를 확보했지만 이후 HomeDS402Ex retained-store/ownership source가
`dev`에 추가됐으므로 그 artifact를 current activation image로 간주하지 않는다. production activation
전에는 current `dev` source tree 기준 C78/ARM Rebuild/Link, PLC download/runtime/hardware proof를 다시
묶어야 한다. artifact ratchet도 자동 갱신하지 않는다.

### 8.6 MODE-13 PC/WPF qualification checkpoint

PR #15(`fix(mode): close MODE-13 WPF reject recovery gap`)를 Windows runner에서 검증한 뒤
`dev`에 squash merge했다. qualification run은 `32789073664`다.

- Debug WPF smoke build: PASS, 0 warnings / 0 errors
- Debug `--filter SetOperationMode`: `12/12 PASS`
- Release WPF smoke build: PASS, 0 warnings / 0 errors
- Release `--filter SetOperationMode`: `12/12 PASS`
- `git diff --check origin/dev...HEAD`: PASS
- SDK recovery BootId mismatch: 0x7D24/0x7D25 zero-wire PASS
- WPF definitive rejection archive 후 interlock clear PASS
- reject recovery-key mismatch 시 active interlock 유지 PASS

상세 evidence는
[evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md](evidence/SET_OPERATION_MODE_MODE13_WPF_RECOVERY_20260825.md)에 고정한다.

### 8.7 MODE-11 software bench preparation / current WPF follow-up

PR #18 `codex/setopmode-mode11-bench-activation`은 production source를 merge하는 branch가 아니라
physical bench qualification 전용 `DO NOT MERGE` branch다.

software-side 준비 checkpoint:

- BASELINE_OFF/BENCH_ACTIVE activation transform self-test PASS
- 33-check candidate verifier PASS
- durable WPF journal evidence exporter self-test PASS
- MODE-11A/B hardware evidence verifier self-test PASS
- same-mode source branch에서 `WriteRequested=0`, `WriteDispatched=0` contract proof

이 evidence는 실제 drive packet/hardware PASS를 대신하지 않는다.

또한 PR #37은 recent SetOperationMode/HomeDS402Ex dynamic recovery panel의 Korean localization과 CI path
coverage 후속 작업이다. 현재 미병합이며 localization regression이 아직 green이 아니다. protocol/SDK/
LASAL/no-replay semantics와 MODE-13 완료 상태에는 영향을 주지 않는다.

## 9. 비-CSP 후속 gate

Mode 1/3/7을 열려면 mode별 PDO, setpoint producer, controlword owner, `_LMCAxis` output 인계,
Stop/Power/fault/restart와 mode 8 복귀 계약을 별도로 구현한다. 이 결정 전에는 진행도가 올라가도
`CSP=8 only` 제한을 특이사항에서 제거하지 않는다. CSP recovery-only tranche 완료를
`MMC_ChngOpMode` 전체 구현 또는 75% 완료로 기록하지 않는다.

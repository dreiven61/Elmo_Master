# LMC Home current-position zero and encoder maintenance IDE handoff

작성일: 2026-08-03

> 2026-08-04 TW19/TW20 정정·활성화: 이 문서의 과거 checkpoint에 기록된
> `CommandValue=FeedbackSocket`, 축별 manifest 및 gate OFF 판정은
> [TW19/TW20 fixed-one activation](./LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md)으로
> 대체한다.

## 1. 확정 범위

이 문서는 다음 네 기능을 실제 구현하기 위한 LASAL IDE declaration 및 Network 변경의
authoritative handoff다.

| 기능 | Wire | 확정 동작 |
|---|---|---|
| `LMC_Home` | `0x7D13/0x7D18/0x7D19` | 현재 축 좌표를 `0`으로 재설정한다. 물리 이동, Home switch, limit switch를 사용하지 않는다. |
| `LMC_HomeDS402` | `0x7D15/0x7D16/0x7D17` | DS402 Homing method `37`, Home offset `0`만 허용한다. 물리 이동과 switch를 사용하지 않는다. |
| `TW20` | `0x7E53/0x7E54/0x7E55` | exact `0x20FC:0x02`에 UInt16 `1`을 2 byte로 쓴다. |
| `TW19` | `0x7E53/0x7E54/0x7E55` | exact `0x20FC:0x01`에 UInt16 `1`을 2 byte로 쓴다. |

프로젝트 공개 API, UI, LASAL symbol, test 및 설계 문서에서는 `LMC` 이름만 사용한다.
이전 문서의 switch-search Home, `MoveReference()`, DS402 method 35 및 임의 Home offset 설계는
이 문서로 폐기한다.

`TW19`와 `TW20`의 write value는 둘 다 고정 UInt16 `1`이다. `DriveReference`가 실제 drive 축을
선택한다. 범용 `TW[]` LONG alias `0x3204:0x13/0x14`로 대체, fallback 또는 이중 전송하지 않는다.

전용 source gate는 `LMC_ADMIN_AXIS_HOME_ENABLED`, `LMC_DIAG_DS402_HOME_ENABLED`,
`LMC_DIAG_ENCODER_TW20_ENABLED`, `LMC_DIAG_ENCODER_TW19_ENABLED`다. 앞의 두 gate는 각 complete
triad와 Admin capability bit 4/6을 원자적으로 맞춘다. TW20/TW19 gate와 Diagnostics bit
18/19는 2026-08-04 fixed-one 계약에서 `TRUE`/ON으로 활성화했다. Home과 DS402 Home gate는
별도 상태이며 이 변경으로 활성화하지 않는다.

## 2. 실행 경계

`_LMCAxis.SetPosition()`은 해당 axis realtime thread와 같은 CPU core에서, realtime thread와
같거나 더 낮은 priority의 thread에서만 호출할 수 있다. 따라서 `LMC_Home`의 좌표 재설정은
TCP task에서 직접 실행하지 않는다.

현재 `LMCEcatInputLatch1`은 `_LMCAxis1.LMCPreRtWorkTrigger`에 연결돼 있고
`_LMCAxis1..4.Control` client를 이미 가진다. 여기에 전용 one-shot RT mailbox를 추가해 다음을
연속된 RT sample로 수행한다.

1. 대상 axis가 Standstill인지 확인한다.
2. raw drive actual position과 LASAL application/internal position을 보존한다.
3. `SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=0)`을 정확히 한 번 호출한다.
4. raw drive actual position 전후 sample의 차이는 stationary feedback 허용 창
   `-2/-1/0/+1/+2 count` 안이고 LASAL actual/set/destination/master position은 `0`으로 정렬되는지
   3개의 fresh sample로 확인한다. `+/-3 count` 이상은 계속 fail-closed한다.
5. native command state, axis status/error, 전후 위치 및 cycle을 terminal result로 게시한다.

이 경로는 모터를 움직이는 reference search가 아니다.

PC response parser와 PLC detail mapping은 다음 exact failure allow-list를 공유한다.

- `LMC_Home` Start: common envelope 외 `10/13/15/16/17/18/40/41/42`. Detail `11
  NativeCommandRejected`는 금지한다.
- `LMC_Home` Outcome/Retire: identity `16/17/18`과 outcome `33..37`.
- `LMC_HomeDS402` Outcome/Retire: identity `16/17/18`과 outcome `25..29`.

Outcome/Retire의 identity failure는 정확히 16-byte common failure envelope여야 하며 retained
record payload로 해석하지 않는다.

## 3. LASAL IDE declaration 작업

아래 identifier, comment 및 symbol은 모두 7-bit ASCII로 입력한다. declaration과 Network만
IDE에서 변경하고 implementation body는 Codex가 추적된 `.st` source에서 수정한다.

### 3.1 `LMCEcatInputLatch`

Variables에 추가한다.

```text
AxisZeroHomeRequestSequence : UDINT
AxisZeroHomeAppliedSequence : UDINT
AxisZeroHomeCancelSequence : UDINT
AxisZeroHomeMailbox : ARRAY [0..7] OF DINT
AxisZeroHomeResult : ARRAY [0..31] OF DINT
```

다음 세 Home function을 `GLOBAL`로 추가한다.

```text
FUNCTION GLOBAL SubmitAxisZeroHome
VAR_INPUT
    OperationToken : UDINT
    AxisReference : DINT
    ExpectedActualPosition : DINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL CancelAxisZeroHome
VAR_INPUT
    OperationToken : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL CopyAxisZeroHomeResult
VAR_INPUT
    OperationToken : UDINT
    pDest : ^void
    DestSize : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION
```

startup reconciler 후속 반영에는 다음 `GLOBAL` snapshot copy function도 포함된다.

```text
FUNCTION GLOBAL CopyAxisOwnershipStartupSnapshot
VAR_INPUT
    pDest : ^void
    DestSize : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION
```

이 function은 `PublishSequence` seqlock 아래 `SnapshotBytes[464..511]`의 exact 48 byte만
복사한다. scalar flag getter로 대체하지 않는다.

`ExpectedActualPosition`은 PC가 준비 시 고정한 application-unit 실제 위치다. RT mailbox가
`SetPosition`을 호출하기 직전에 읽은 fresh actual position과 exact 일치하지 않으면 native
호출 0회로 거부한다. 이 stale-position guard를 TCP task의 과거 snapshot으로 대체하지 않는다.

`CopyAxisZeroHomeResult.pDest`는 정확히 `^void`다. `pVoid`에 포인터를 한 단계 더 붙인
`^pVoid`는 이 프로젝트의 pointer-out ABI이며 이 buffer-copy ABI에 사용하지 않는다.

#### 3.1.1 RT mailbox/result slot 계약

`AxisZeroHomeMailbox[0..3]`은 request sequence 게시 뒤 immutable이다. Slot 4는 RT만 쓰는
irrevocable dispatch claim이고 새 request producer만 `0`으로 초기화한다. Slot 5는
`CancelAxisZeroHome` producer가 exact request sequence를 써서 취소 요청을 echo하는 곳이며,
slot 6..7은 예약 영역이다.

| Slot | 형식 | 의미 |
|---:|---|---|
| 0 | `UDINT` | `OperationToken` |
| 1 | `DINT` | `AxisReference` (`1..4`) |
| 2 | `DINT` | `ExpectedActualPosition` (application unit) |
| 3 | `UDINT` | `RequestSequenceEcho` |
| 4 | `UDINT` | `NativeDispatchSequenceClaim`; native 호출 직전에 RT가 request sequence를 1회 기록 |
| 5 | `UDINT` | `CancelRequestSequenceEcho`; 취소 없음은 `0`, 취소 게시 중/완료는 exact request sequence |
| 6..7 | `DINT` | Reserved, 반드시 `0` |

`AxisZeroHomeResult[0..31]`은 다음 exact layout을 사용한다.

| Slot | 형식 | 의미 |
|---:|---|---|
| 0 | `UDINT` | `OperationToken` |
| 1 | `DINT` | `State`: `1 Running`, `2 Succeeded`, `3 Failed` |
| 2 | `DINT` | 내부 RT `FailureCode`; 성공은 `0` |
| 3 | `DINT` | `AxisReference` |
| 4 | `DINT` | `ExpectedActualPosition` |
| 5 | `DINT` | `RuntimePhase`: `0 Dispatch`, `1 Verify` |
| 6 | `DINT` | `StableSampleCount`; 성공은 `3` |
| 7 | `UDINT` | `EvidenceFlags`; 성공은 정확히 `0x0000003F` |
| 8 | `DINT` | `NativeCallCount`; 성공은 정확히 `1` |
| 9 | `UDINT` | `StartRtCycle` |
| 10 | `UDINT` | `CompletionRtCycle`; terminal에서만 nonzero |
| 11 | `UDINT` | `_LMCAXIS_STATUS` |
| 12 | `UDINT` | `_LMCAXIS_ERROR` |
| 13 | `DINT` | `RawDrivePositionBefore` |
| 14 | `DINT` | `RawDrivePositionAfter` |
| 15 | `DINT` | `ActualApplicationPositionAfter` |
| 16 | `DINT` | `SetApplicationPositionAfter` |
| 17 | `DINT` | `ActualInternalPositionAfter` |
| 18 | `DINT` | `SetInternalPositionAfter` |
| 19 | `DINT` | `DestinationInternalPositionAfter` |
| 20 | `DINT` | `MasterInternalPositionAfter` |
| 21 | `UDINT` | `NativeCommandState` |
| 22 | `UDINT` | `RequestSequenceEcho` |
| 23 | `UDINT` | `LastObservationRtCycle` |
| 24..31 | `DINT` | Reserved, 반드시 `0` |

Evidence bit는 bit 0 expected-position CAS, bit 1 Standstill/AxisError state, bit 2 raw
drive position 불변, bit 3 application actual/set zero, bit 4 internal
actual/set/destination/master zero, bit 5 서로 다른 RT 호출의 3개 연속 sample을 뜻한다.
Verify phase 진입 시 허용되는 `(StableSampleCount, EvidenceFlags)`는 `(0, 0x03)`,
`(1, 0x1F)`, `(2, 0x1F)`뿐이며 다른 조합은 재호출 없이 corrupt terminal이다.

내부 failure는 `-1 invalid identity`, `-3 client disconnected`, `-4 invalid axis state`,
`-5 stale expected position`, `-6 native state`, `-7 verification mismatch`, `-8 corrupt
runtime`, `-9 DS402 owner active`다. `SubmitAxisZeroHome`의 `-2`는 다른 request가 진행 중임을
뜻한다. `CopyAxisZeroHomeResult`는 `0 copied`, `1 pending`, `-1 invalid token`, `-2 invalid
destination`, `-4 missing/key mismatch`를 반환한다.

Producer는 mailbox payload와 slot 3을 먼저 쓰고 slot 4를 `0`으로 초기화한 뒤
`AxisZeroHomeRequestSequence`를 마지막에 원자적으로 게시한다. RT는 native 호출 직전에 slot
4에 current request sequence를 기록한다. Phase 0에서 slot 4 또는 `NativeCallCount`가 이미
nonzero이면 재호출하지 않고 corrupt terminal로 끝낸다. RT는 terminal result 전체를 먼저 쓴 뒤
`AxisZeroHomeAppliedSequence`를 마지막에 원자적으로 게시한다. `CopyAxisZeroHomeResult`는 두
sequence가 같은 terminal에서만 128 byte를 복사하고 복사 전후 sequence가 모두 같은지 다시
확인한다. Exact retry는 terminal result를 재사용하며 `SetPosition`을 다시 호출하지 않는다.

`CancelAxisZeroHome`는 slot 5에 exact request sequence echo를 먼저 쓰고
`AxisZeroHomeCancelSequence`를 마지막에 원자적으로 게시한다. RT는 두 값이 모두
current request sequence와 exact 일치할 때만 취소로 판정한다. Echo만 먼저 보이는
게시 중간 구간에서는 native dispatch나 terminal success를 commit하지 않는다.

`SubmitAxisZeroHome`의 유일한 producer는 `LMCControlCommandService`다. 다른 class/task에서 이
method를 직접 호출하거나 둘 이상의 producer가 동시에 호출하는 것은 금지한다. 이 invariant는
Comm Network와 static verifier에서 caller count 1로 고정한다.

Slot 9, 10, 23은 RT cycle이고 wire의 millisecond가 아니다. `0x7D18/0x7D19`의
`StartMilliseconds`와 `CompletionMilliseconds`는 `LMCControlCommandService`가 자기
service-clock `ops.tAbsolute`로 기록한다. RT cycle에 상수를 곱해 wire millisecond로 변환하지
않는다.

기존 DS402 Home declaration은 삭제하거나 이름을 바꾸지 않는다.

### 3.2 `LMCControlCommandService`

Client를 추가한다.

```text
InputLatch : CltChCmd_LMCEcatInputLatch
Required : true
Internal : false
```

Variables를 다음과 같이 변경한다.

```text
delete ReferenceState : ARRAY [0..18] OF DINT
add ZeroHomeState : ARRAY [0..63] OF DINT
add OwnershipState : ARRAY [0..351] OF DINT
add OwnershipStartupState : ARRAY [0..15] OF DINT
add OwnershipObserverState : ARRAY [0..107] OF DINT
```

`HandleRequest`의 기존 six inputs 뒤에 다음 inputs를 순서대로 추가한다.

```text
CallerSessionEpoch : UDINT
RequestSequence : UDINT
AdmissionToken : UDINT
OwnerGeneration : UDINT
```

기존 global function `ProcessAxisReference`를 삭제하고 다음 global function을 추가한다.

```text
FUNCTION GLOBAL ProcessAxisZeroHome
END_FUNCTION
```

공통 축 소유권을 위해 다음 global functions를 추가한다.

```text
FUNCTION GLOBAL ReserveAxisOwnership
VAR_INPUT
    CommandId : UINT
    Reference : UINT
    RequestedAxisMask : UDINT
    OwnerKind : UINT
    ResourceKind : UINT
    AdmissionMode : UINT
    CallerSessionEpoch : UDINT
    RequestSequence : UDINT
    pIdentity : ^UDINT
    IdentityCount : UINT
    pEffectiveAxisMask : ^UDINT
    pAdmissionToken : ^UDINT
    pOwnerGeneration : ^UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL ValidateAxisOwnership
VAR_INPUT
    CommandId : UINT
    Reference : UINT
    ExpectedAxisMask : UDINT
    OwnerKind : UINT
    ResourceKind : UINT
    AdmissionMode : UINT
    CallerSessionEpoch : UDINT
    RequestSequence : UDINT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL CommitAxisOwnership
VAR_INPUT
    CommandId : UINT
    Reference : UINT
    ExpectedAxisMask : UDINT
    CallerSessionEpoch : UDINT
    RequestSequence : UDINT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL RollbackAxisOwnership
VAR_INPUT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
    CallerSessionEpoch : UDINT
    RequestSequence : UDINT
    Reason : DINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL PublishAxisOwnership
VAR_INPUT
    AxisMask : UDINT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
    ReportKind : UINT
    ReportValue0 : UDINT
    ReportValue1 : UDINT
    ObservationCycle : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL ReconcileAxisOwnershipStartup
VAR_INPUT
    DiagnosticsBootId : UDINT
    ObservationCycle : UDINT
    ReportCycle : UDINT
    DiagnosticsDrainFlags : UDINT
END_VAR
VAR_OUTPUT
    Result : DINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL NotifyAxisOwnershipSessionClosed
VAR_INPUT
    SessionEpoch : UDINT
END_VAR
END_FUNCTION

FUNCTION GLOBAL ProcessAxisOwnership
END_FUNCTION
```

#### 3.2.1 `ZeroHomeState[0..63]` exact retained layout

| Slot | 형식 | 의미 |
|---:|---|---|
| 0 | `DINT` | record state: `0 Empty`, `1 Running`, `2 Succeeded`, `3 Failed`, `4 Aborted`, `5 Quarantined` |
| 1..4 | `UDINT` | DiagnosticsBuild, original BootId, MapRevision, original RequestId |
| 5..8 | `UDINT` | ClientIntentId 128-bit |
| 9..10 | `DINT` | AxisReference, SemanticMode (`1 CurrentPositionZero`) |
| 11..12 | `DINT` | ExpectedActualPosition, TargetPosition(항상 `0`) |
| 13 | `UDINT` | TimeoutMilliseconds |
| 14..16 | `DINT` | wire Status, ErrorId, DetailCode |
| 17 | `UDINT` | terminal `_LMCAXIS_STATUS` |
| 18 | `DINT` | terminal `_LMCAXIS_ERROR` |
| 19..20 | `DINT` | RawDrivePositionBefore/After |
| 21..26 | `DINT` | application actual/set, internal actual/set/destination/master after |
| 27..28 | `UDINT` | NativeCommandState, EvidenceFlags |
| 29..30 | `UDINT` | service-clock StartMilliseconds, CompletionMilliseconds |
| 31..32 | `DINT` | FailureCode, RuntimePhase |
| 33..38 | `UDINT` | RecordGeneration, AdmissionToken, OwnerGeneration, SessionEpoch, RequestSequence, AxisMask |
| 39..43 | `DINT` | engine state, RT state, RT failure, NativeCallCount, StableSampleCount |
| 44..46 | `UDINT` | StartRtCycle, CompletionRtCycle, LastObservationRtCycle |
| 47 | `DINT` | owner transition/publish result |
| 48 | `DINT` | Retired flag |
| 49 | `UDINT` | RetirementGeneration |
| 50 | `UDINT` | monotonic record generation counter |
| 51 | `UDINT` | record magic |
| 52 | `DINT` | RT mailbox submit result |
| 53 | `DINT` | post-mailbox owner-commit uncertainty flag |
| 54 | `UDINT` | cancel/drain start service milliseconds |
| 55 | `UDINT` | RT `RequestSequenceEcho` |
| 56 | `UDINT` | Home lifecycle flags |
| 57 | `DINT` | pending terminal record state |
| 58 | `DINT` | pending owner report kind |
| 59 | `DINT` | pending preemption cleanup kind; ordinary Home owner publish는 `0` |
| 60 | `UDINT` | ordinary Home owner-release receipt magic `0x484F4D50` |
| 61 | `DINT` | receipt phase: `1 PREPARED`, `2 IDENTITY`, `3 OBSERVER`, `4 SINGLETON`, `5 RECORD`, `6 COMPLETE` |
| 62 | `DINT` | initial owner publish result; v1은 정확히 `0` |
| 63 | `UDINT` | receipt가 구속된 exact RecordGeneration |

Receipt는 ordinary Home success/safe-failure의 owner release만 다룬다. 첫 번째 exact
`PublishAxisOwnership`는 owner를 지우지 않고 slot 60을 마지막에 commit한
`PREPARED`를 남긴다. Caller가 finalize acceptance를 retained flag에 기록한 뒤 같은 exact
publish를 replay하면 identity, observer, singleton, axis record를 phase별로 정리하고
`COMPLETE`를 게시한다. 이 receipt는 같은 `LMCControlCommandService` instance의 재호출과
warm continuation에서 partial cleanup을 판별하기 위한 retained ledger이다. Cold restart 후에도
보존되는 persistent journal이 아니며 cold-restart recovery proof로 사용하지 않는다. 인증된
`COMPLETE` receipt는 terminal outcome 증거로 남지만, tuple이 다른 후속 ownership publish를
가로채지 않는다. 반면 incomplete 또는 손상된 receipt는 계속 fail-closed로 차단한다.

`StartMilliseconds`와 `CompletionMilliseconds`는 `ops.tAbsolute`의 unsigned 차이로 timeout을
판정한다. RT cycle slot을 millisecond로 변환하지 않는다. `NativeCallCount=1`, raw drive 전후
sample의 wrap-safe 차이 `-2/-1/0/+1/+2 count`, application/internal 좌표 `0`, fresh sample 3개와
evidence `0x3F`가 모두 맞아야 성공이다. raw 전후 값은 물리 feedback 증거이며 반드시
bit-identical해야 한다는 계약은 아니다.

#### 3.2.2 `OwnershipState[0..351]` exact layout

global prefix `0..27`은 다음과 같다.

| Slot | 의미 |
|---:|---|
| 0 | table magic `0x4C4D434F` |
| 1..2 | next AdmissionToken, next OwnerGeneration |
| 3..6 | reconciled DiagnosticsBootId, report cycle, proof flags, quarantine reason |
| 7..9 | LMC Home engine token/generation/exact mask |
| 10..12 | DS402 Home engine token/generation/exact mask |
| 13..15 | Diagnostics SDO engine token/generation/exact mask |
| 16..21 | synchronous HandleRequest session/sequence/token/generation/command/reference context |
| 22..24 | last closed SessionEpoch, last process time, global quarantine flag |
| 25..27 | Reserved |

Axis `n`의 record base는 `28 + (n - 1) * 36`이다.

| Offset | 의미 |
|---:|---|
| 0..3 | AxisReference, State, OwnerKind, CommandId |
| 4..7 | AdmissionToken, OwnerGeneration, SessionEpoch, RequestSequence |
| 8..10 | AcquireCycle, LastObservationCycle, StableSinceCycle |
| 11..14 | exact AxisMask, Reference, ResourceKind, AdmissionMode |
| 15 | identity word count (`0..16`) |
| 16..31 | exact operation identity raw UDINT words |
| 32..34 | ReportKind, ReportValue0, ReportValue1 |
| 35 | record magic `0x4F574E00 + AxisReference` |

State 값은 `0 Idle`, `1 Reserved`, `2 DirectActive`, `3 GroupLease`, `4 GroupActive`,
`5 LmcHomeActive`, `6 Ds402HomeActive`, `7 Tw20Queued`, `8 Tw20Running`, `9 Tw20Draining`,
`10 SafetyPreempting`, `11 Quarantined`다. owner kind는 `1 Direct`, `2 Group`, `3 LMC Home`,
`4 DS402 Home`, `5 Encoder`다. resource kind는 `1 Axis`, `2 LMC Home engine`, `3 DS402 Home
engine`, `4 Diagnostics SDO engine`다.

Home Start의 identity는 frame byte `8..63` 전체, 즉 `14`개의 raw UDINT다. reserve/validate/
commit/rollback/publish는 같은 token, generation, session, request sequence, exact mask와 singleton
engine tuple을 mutation 전에 모두 확인한다. 반환값은 `0 success`, `-1 invalid input`, `-2
busy/stale/mismatch`, `-3 startup 또는 quarantine`이다. session close는 아직 dispatch되지 않은
`Reserved`만 quarantine하고 이미 active인 Home을 임의 release하지 않는다.

#### 3.2.3 startup proof와 현재 명시적 blocker

startup proof는 exact `0x0000000F`가 필요하다.

| Bit | 필요한 증거 |
|---:|---|
| 0 | nonzero current Diagnostics BootId |
| 1 | physical axes가 fresh/stable idle, standstill 및 power-transition 없음 |
| 2 | Group/profile lease와 active motion 없음 |
| 3 | Home/DS402/SDO executor 및 mailbox가 drained/stable idle |

과거 `TCPMotionInterface`의 BootId-only `ReportAxisOwnershipStartup` caller와 해당 public ABI는
삭제됐다. 현재 private `LMCDiagnosticsService.ProcessAxisOwnershipStartup`가
`LMCEcatInputLatch.CopyAxisOwnershipStartupSnapshot`으로 seqlock 48-byte snapshot을 읽고
Diagnostics drain flags를 만든 뒤 `ReconcileAxisOwnershipStartup`를 호출한다.

48-byte snapshot은 magic, observation cycle, Axis1..4 `_LMCAXIS_STATUS`, Axis1..4
`Drive.StateWord` low 16-bit, exact latch drain `0x0000001F`, reserved zero를 가진다. 여기의
`Drive.StateWord`를 DS402 `0x6041` read라고 단정하지 않는다. latch drain은 physical EtherCAT
health/current cycle, Zero Home request/applied terminal, DS402 request/applied, RT owner/alignment
idle 및 네 drive ControlWord bit 4 low를 각각 증명한다. Diagnostics drain도 snapshot validity,
DS402 lifecycle idle, encoder lifecycle idle, generic SDO idle 및 네 executor `IsReusable()`의
exact `0x0000001F`가 필요하다.

reconciler는 exact proof `0x0000000F`, 동일 axis/drive/group signature의 서로 다른 fresh latch
cycle 3개와 `ops.tAbsolute` 100 ms를 모두 만족한 경우에만 zero table 또는 exact prior-BootId
idle table을 초기화한다. 같은 cycle replay는 sample count를 올리지 않는다. 같은 BootId로 이미
완료된 valid table은 정상 operation의 transient non-idle을 startup failure로 재해석하지 않는다.
corrupt, active 또는 quarantined prior table은 `-3`이며 임의로 지우지 않는다.

따라서 BootId-only permanent quarantine blocker는 source에서 제거됐다. 그러나 이 경로의
runtime fresh-cycle/100 ms 동작은 PLC에서 검증되지 않았고 아래 ordinary/safety blocker도 남아
있다. `LMC_ADMIN_AXIS_HOME_ENABLED`를 포함한 gate를 단독으로 바꾸는 것은 활성화 절차가 아니다.

### 3.3 `LMCDiagnosticsService`

Client를 추가한다.

```text
AxisOwnership : CltChCmd_LMCControlCommandService
Required : true
Internal : false
```

Variables에 추가한다.

```text
EncoderMaintenanceState : ARRAY [0..191] OF DINT
EncoderMaintenanceServiceMilliseconds : UDINT
EncoderMaintenanceObservedLatchCycle : UDINT
EncoderMaintenanceLatchAdvanceServiceMilliseconds : UDINT
EncoderMaintenanceLatchFreshSampleCount : UINT
```

전용 encoder-maintenance wire field는 cycle 수가 아니다. `0x7E53` request의 P48,
`0x7E54/0x7E55` identity의 해당 field 및 outcome P64는 모두
`TimeoutMilliseconds : UDINT`이며 허용 범위는 `1..60000`이다. 이 값은 request 수락부터
terminal/cleanup까지의 전체 PLC service-clock timeout이다. 일반 D5 SDO의
`TimeoutCycles` 계약과 섞지 않는다.

`ProcessEncoderMaintenance`는 시작 시점의 `ops.tAbsolute`를 runtime state에 보존하고 매
호출마다 unsigned `elapsedMs := serviceNow - serviceStartMs`를 계산한다. RT latch cycle이
정지해도 overall timeout과 cleanup timeout은 계속 진행해야 한다. executor를 시작하기 전
`elapsedMs >= TimeoutMilliseconds`이면 timeout terminal로 전환한다. 그렇지 않으면
`remainingMs := TimeoutMilliseconds - elapsedMs`를 계산하고 `1..60000`으로 제한한 뒤
`LMCSdoExecutor.TryStartWrite(..., TimeoutMs:=remainingMs)`에 전달한다. 전체 timeout 값을
그대로 재사용하거나 `EncoderMaintenanceServiceMilliseconds`/RT latch cycle을 같은
단위로
간주해서는 안 된다. `EncoderMaintenanceObservedLatchCycle`,
`EncoderMaintenanceLatchAdvanceServiceMilliseconds` 및 fresh sample count는 완료 evidence에만
사용한다.

DS402 service-clock용 새 IDE variable은 추가하지 않는다. 기존 `Ds402HomeState` runtime
record의 slot 118은 `ops.tAbsolute` service start, slot 119는 cleanup start를 보존한다. 전체
timeout과 cleanup timeout은 unsigned `serviceNow - serviceStart`로 판정하며 RT latch cycle이
멈춰도 진행해야 한다. `newCycle AND timeout` 형태로 timeout을 fresh-latch 조건 안에 넣지
않는다. fresh latch는 완료 안정성/evidence 판정에만 사용한다.

`HandleRequest`의 기존 `CallerSessionEpoch` 뒤에 다음 inputs를 순서대로 추가한다.

```text
RequestSequence : UDINT
AdmissionToken : UDINT
OwnerGeneration : UDINT
```

기존 `HandleAxisDs402HomeStart`의 `CallerSessionEpoch` 뒤, `RequestSize` 앞에 다음 inputs를
추가한다.

```text
RequestSequence : UDINT
AdmissionToken : UDINT
OwnerGeneration : UDINT
```

다음 private functions를 추가한다. `GLOBAL`로 만들지 않는다.

```text
FUNCTION HandleEncoderMaintenanceStart
VAR_INPUT
    Reference : UINT
    pRequest : ^USINT
    pResponse : ^USINT
    ResponseCapacity : UDINT
    CallerSessionEpoch : UDINT
    RequestSequence : UDINT
    AdmissionToken : UDINT
    OwnerGeneration : UDINT
    RequestSize : UDINT
END_VAR
VAR_OUTPUT
    ResponseSize : DINT
END_VAR
END_FUNCTION

FUNCTION HandleEncoderMaintenanceOutcome
VAR_INPUT
    Reference : UINT
    pRequest : ^USINT
    pResponse : ^USINT
    ResponseCapacity : UDINT
    CallerSessionEpoch : UDINT
    RequestSize : UDINT
END_VAR
VAR_OUTPUT
    ResponseSize : DINT
END_VAR
END_FUNCTION

FUNCTION HandleEncoderMaintenanceRetire
VAR_INPUT
    Reference : UINT
    pRequest : ^USINT
    pResponse : ^USINT
    ResponseCapacity : UDINT
    CallerSessionEpoch : UDINT
    RequestSize : UDINT
END_VAR
VAR_OUTPUT
    ResponseSize : DINT
END_VAR
END_FUNCTION

FUNCTION ProcessEncoderMaintenance
END_FUNCTION

FUNCTION ProcessAxisOwnershipStartup
END_FUNCTION
```

`ProcessAxisOwnershipStartup`도 `PRIVATE`이며 `GLOBAL`로 만들지 않는다. `ProcessOperations`에서
encoder maintenance와 DS402 Home cleanup을 먼저 pump한 뒤 generic-SDO early return보다 앞에서
호출한다.

### 3.4 `TCPMotionInterface`

새 declaration은 없다. 기존 `ActiveRequest.Sequence`와 `ActiveRequest.SessionEpoch`를 사용한다.
Codex가 implementation에서 owner reserve/validate/commit/rollback, 새 command route와 cyclic
processing을 추가한다.

## 4. Comm Network 작업

`Comm_Network`에 아래 두 연결을 추가한다.

```text
LMCControlCommandService1.InputLatch -> LMCEcatInputLatch1.ClassSvr
LMCDiagnosticsService1.AxisOwnership -> LMCControlCommandService1.ClassSvr
```

기존 연결은 삭제하지 않는다.

## 5. IDE 저장 절차

1. Git 추적 production project `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`인지 확인한다.
2. 위 declaration과 두 Network 연결만 입력한다.
3. Save All 한다.
4. 이 시점에는 F9 build를 실행하지 않는다. implementation signature가 아직 이전 declaration과
   다르므로 Codex가 `.st` body를 맞춘 뒤 build해야 한다.
5. 저장 완료 시각을 Codex에 알려준다. Codex가 generated declaration, implementation 보존 및
   Network diff를 먼저 검사한다.

## 6. Codex 구현 뒤 사용자 검증

Codex가 source, SDK, WPF, protocol map 및 static tests를 맞춘 뒤 사용자에게 다음을 별도로
요청한다.

1. F9 incremental build.
2. Object Network Server/Client는 `Find in Implementation`으로 확인하고, 변경
   function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header를 직접 연다.
3. smoke 시작 시점 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException` 0건 확인.
4. PLC download.
5. 축별 no-motion Home 및 maintenance live test.

Object Network Server/Client의 `Find in Implementation`이 큰 implementation block 전체를
출력하는 것은 class 크기 때문에 결과가 길어진 것이며, hit count와 `Last command succeeded`
및 신규 exception 유무로 성공을 판정한다. 이 판정은 function/method direct-open 증거를
대신하지 않는다.

## 7. 아직 필요한 실기 정보

`LMC_Home`과 `LMC_HomeDS402`는 switch 정보가 필요 없다. `TW19/TW20` activation에는 각 축의
아래 정보가 필요하다.

```text
Axis1: encoder family, feedback socket 1..4
Axis2: encoder family, feedback socket 1..4
Axis3: encoder family, feedback socket 1..4
Axis4: encoder family, feedback socket 1..4
```

현재 repo/local EAS 자료에서 Axis4에 대해서만 과거 근거가 발견됐다. 2026-06-11 export의
serial `25493155`와 `P01.a04` 화면은 `Serial Absolute - Panasonic, Port A`, `CA[41]=6`,
`CA[45]=1`, `CA[46]=1`, `CA[47]=1`, `CA[59]=23`, `CA[62]=0`을 보인다. 이는 과거 시점의
Panasonic/socket 1 구성은 뒷받침하지만 현재 live readback이 아니며, multi-turn resolution이
0이므로 TW19 적용 근거가 아니다. 현재 좁은 TW20 계약인 EnDat 2.2 ID30 근거도 아니다.
Axis1..3은 family/socket 근거가 없다. 따라서 활성화 가능한 축은 현재 `0`개다.

다음 IDE/실기 단계에서는 각 `P01.a01..a04`의 대상명과 drive serial이 보이는 상태로 아래 값을
읽고, 가능하면 축별 `.gprm`을 새로 export한다.

```text
CA[41]
CA[42]
CA[43]
CA[44]
CA[45]
CA[46]
CA[47]
CA[59]
CA[62]
```

`Port A/B`, software feedback socket `1..4`, axis 번호를 서로 같은 값으로 추정하지 않는다.
TW19는 지원 family가 배치된 socket과 `CA[62] > 0`을 확인한 뒤, TW20은 exact EnDat 2.2 ID30을
확인한 뒤에만 활성화한다.

이 정보와 motor-off live proof가 없으면 source/parser/test를 완성해도 해당 capability bit는
계속 OFF로 둔다.

## 8. 2026-08-03 source checkpoint

현재 추적 `.st` implementation에는 아래 dormant 경로가 prewire돼 있다.

- `0x7D13/0x7D18/0x7D19` current-position-zero Start, retained Outcome, exact-generation Retire
- TCP full-payload identity, session/request/token/generation fencing과 exact response correlation
- RT mailbox의 stale-position guard, native call 1회, raw drive `-2/-1/0/+1/+2 count` 창 및 3 fresh sample 검증
- exact-mask common owner reserve/validate/commit/rollback/publish와 singleton engine lease
- `0x7E53/0x7E54/0x7E55` Start, retained Outcome, exact-generation Retire
- exact `0x20FC:0x02` UInt16 TW20과 exact `0x20FC:0x01` UInt16 TW19 one-shot SDO dispatch
- request 수락부터 owner cleanup까지 `ops.tAbsolute` 기반 `1..60000 ms` overall timeout
- executor drain과 common-owner release 뒤 terminal record를 마지막에 게시하는 순서
- generic D5, DS402 Home, encoder-maintenance SDO engine 상호 배제
- `0x3204:0x13/0x14` LONG alias fallback 및 generic `0x7E50` bypass 없음

이 checkpoint는 활성화 증거가 아니다. 이 절 작성 당시 두 global gate는 `FALSE`였고,
TW20/TW19의 축 1..4
`PROFILE`, `SOCKET`, `EVIDENCE0..3` manifest 48개 값은 전부 `0`이다. 따라서 global gate 하나를
실수로 바꿔도 축별 exact manifest가 provision되지 않으면 SDO write 전 fail-closed한다.
Diagnostics capability bit 18/19도 해당 gate가 `TRUE`일 때만 게시되므로 현재 OFF다.

`LMC_ADMIN_AXIS_HOME_ENABLED`도 `FALSE`이고 Admin capability bit 4는 OFF다. 이 절 작성
시점에는 startup proof가 BootId-only라 common owner가 quarantine되고 reserve가 `-3`으로
거부됐다. 그 specific blocker는 이후 3.2.3의 reconciler로 교체됐지만, Home gate만 바꾸는 것이
활성화 절차가 아니라는 판정은 유지한다.

이 절 작성 당시 generated declaration은 의도적으로 직접 수정하지 않았다. 사용자가 LASAL IDE에서 3.2와
3.3의 declaration을 저장하기 전에는 implementation과 ABI가 일치하지 않으므로 F9 build를
실행하지 않는다. 특히 `LMCDiagnosticsService`에는 아래 항목이 모두 필요하다.

```text
AxisOwnership client
EncoderMaintenanceState and four service/evidence variables
HandleRequest RequestSequence/AdmissionToken/OwnerGeneration inputs
HandleAxisDs402HomeStart RequestSequence/AdmissionToken/OwnerGeneration inputs
HandleEncoderMaintenanceStart/Outcome/Retire private functions
ProcessEncoderMaintenance private function
```

Comm Network의 두 연결도 4절 그대로 사용자 IDE 작업으로 추가됐다. 이후 generated declaration,
implementation 보존 및 Network diff를 확인한 뒤 F9와 Find smoke를 진행했다.

이 당시 static/source checkpoint가 보장하지 않은 항목 중 LASAL build는 9절에서 완료됐다. PLC
download, task/core 배치,
실축 motor-off 상태, exact encoder family/socket, SDO callback, 실제 TW19/TW20 효과다. 이 항목이
확인되기 전에는 기능을 `사용 가능` 또는 `실기 검증 완료`로 판정하지 않는다.

## 9. 2026-08-03 post-IDE/build checkpoint

이 절은 8절의 source checkpoint와 당시의 안전 차단 근거를 삭제하거나 변경하지 않는다. 다만
8절 작성 뒤 진행된 IDE 반영, source 분할, build 및 IDE smoke 결과를 추가로 기록한다.

사용자가 Git 추적 production project의 LASAL IDE에서 다음 declaration과 Network 변경을
저장했고, 생성된 source와 연결 metadata에서 반영을 확인했다.

- `LMCControlCommandService`: `InputLatch`, `ZeroHomeState[0..63]`,
  `OwnershipState[0..351]`, 확장된 `HandleRequest`, `ProcessAxisZeroHome` 및 ownership global
  functions
- `LMCDiagnosticsService`: `AxisOwnership`, encoder-maintenance 변수 5개, 확장된
  `HandleRequest`와 `HandleAxisDs402HomeStart`, encoder-maintenance private functions 4개
- `Comm_Network`:
  `LMCControlCommandService1.InputLatch -> LMCEcatInputLatch1.ClassSvr`
- `Comm_Network`:
  `LMCDiagnosticsService1.AxisOwnership -> LMCControlCommandService1.ClassSvr`

큰 parent implementation의 command 처리는 각각
`LMCControlCommandService.HandleAxisZeroHomeCommands`와
`LMCDiagnosticsService.HandleDiagnosticsBulkRequest` helper로 분리했다. parent는 대상 command
범위를 helper에 위임하고 helper가 fail-closed parsing과 처리를 담당한다. 이 분할로 parent
function 크기와 IDE 검색 범위를 줄여, 큰 class에서 method 위치를 비정상적으로 찾던 부담을
완화했다.

이 최초 post-IDE checkpoint에서 Git 추적 production project를 LASAL compiler C78, ARM
target으로 Rebuild한 결과는
`0 error(s), 38 warning(s)`이다. 이어서 아래 두 method의 method-to-implementation smoke가
각 implementation으로 정상 이동했다.

- `LMCControlCommandService.HandleAxisZeroHomeCommands`
- `LMCDiagnosticsService.HandleDiagnosticsBulkRequest`

smoke 시작 기준점 이후 `%TEMP%\Lasal2.log`에 새 `CInvalidArgException`은 `0`건이다.
post-IDE/build 상태의 최종 재검증 결과는 다음과 같다.

- PowerShell parser `2/2` PASS
- negative fixture: AxisZeroHome IDE ABI `10/10`, AxisZeroHome RT mailbox `26/26`,
  Encoder Maintenance `52/52`, Axis Ownership `24/24`, DS402 Home Retirement `19/19` PASS
- SourceOnly 및 generated Network/metadata 포함 full static contract PASS
- C# Release build warning/error `0/0`, 전체 test `1075/1075` PASS

이 build와 smoke는 기능 활성화 또는 실축 검증 증거가 아니다. 이 checkpoint의 아래 gate는
모두 `FALSE`였다.

```text
LMC_ADMIN_AXIS_HOME_ENABLED
LMC_DIAG_DS402_HOME_ENABLED
LMC_DIAG_ENCODER_TW20_ENABLED
LMC_DIAG_ENCODER_TW19_ENABLED
```

TW20/TW19의 축 1..4 `PROFILE`, `SOCKET`, `EVIDENCE0..3` manifest도 계속 전부 `0`이다.
축별 current drive/encoder family/socket 증거가 없으므로 encoder-maintenance는 SDO dispatch 전
fail-closed한다. 이 checkpoint 당시에는 common owner startup proof가 BootId-only라 startup
quarantine blocker가 남아 있었다. 이후 3.2.3의 startup reconciler가 해당 BootId-only 경로를
교체했으며 최신 build/search 상태는 10절에 기록한다.

current-position-zero Home은 home switch나 limit switch를 찾는 DS402 homing 동작이 아니다.
대상 축의 현재 actual position을 확인한 뒤
`SetPosition(Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST, Position:=0)`으로 현재 위치 좌표만 `0`으로
설정하는 no-motion 경로이므로 switch/limit 입력을 사용하지 않는다. 그러나 현재 gate와 startup
runtime proof가 없고 PLC download, motor-off 실축 실행, raw drive 허용 창, fresh sample 및 재기동
보존 proof도 없다. 따라서 이 checkpoint에서 current-position-zero, DS402 Home, TW20, TW19 중
어느 것도 `실기 사용 가능` 또는 `실기 검증 완료`로 판정하지 않는다.

## 10. 2026-08-04 startup/ordinary ownership follow-up

BootId-only `ReportAxisOwnershipStartup` ABI와 TCP caller는 삭제됐고, 다음 source chain이 canonical
project에 반영됐다.

```text
LMCEcatInputLatch.CopyAxisOwnershipStartupSnapshot
  -> LMCDiagnosticsService.ProcessAxisOwnershipStartup (PRIVATE)
  -> LMCControlCommandService.ReconcileAxisOwnershipStartup
```

latest LASAL Class2 02.03.001 C78 ARM build 결과는 `0 error(s), 40 warning(s)`다. 최신
implementation 검색 smoke는 다음과 같다.

- `LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN`: `TCPMotionInterface`에서 1 match
- `OwnershipObserverState`: `LMCControlCommandService`에서 54 matches
- smoke start: `2026-08-03T23:50:12.8194686+09:00`
- 그 시점 이후 `%TEMP%\Lasal2.log` 신규 `CInvalidArgException`: 0건

이 결과는 source/static/build/IDE search proof다. PLC download나 PLC runtime proof가 아니다.
현재 source의 아래 gate는 모두 `FALSE`다.

```text
LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED   // TCP and control service
LMC_ADMIN_AXIS_HOME_ENABLED
LMC_DIAG_DS402_HOME_ENABLED
LMC_DIAG_ENCODER_TW20_ENABLED
LMC_DIAG_ENCODER_TW19_ENABLED
```

따라서 current-position-zero Home, DS402 Home, TW20, TW19 및 ordinary Axis/Group ownership은
어느 것도 production 또는 실기 사용 가능으로 판정하지 않는다. activation 전에 다음 blocker를
모두 닫아야 한다.

1. safety preemption은 기존 axis record를 덮어쓰고 observer에 이전 token/generation/state만
   남긴다. 이전 special owner의 kind/session/raw identity 전체를 보존하는 overlay가 필요하다.
2. ordinary observer는 service task에서 `_LMCAxis`/`LMCRobot`를 직접 읽는다. coherent
   InputLatch cycle이 아니며 DS402 `0x6041`을 관찰하지 않는다.
3. current handler-entry fence에는 실제 native-call marker가 없다. handler boundary를 넘은 뒤
   accepted가 아닌 모든 응답을 보수적으로 quarantine하므로 definite pre-wire rollback과
   post-wire uncertainty를 정확히 구분하지 못한다.
4. owner record의 identity는 최대 16 DINT다. `0x20E7` 및 large move payload의 전체 byte
   identity를 보존하지 못한다.
5. startup/ordinary stability `100 ms`와 ordinary timeout `120000 ms`는 PLC task/EtherCAT에서
   측정되지 않았다.
6. SDK error catalog는 v1이고 symbolic `-9 AxisOwnershipConflict`가 없다.
7. safety preemption이 LMC Home, DS402 Home, TW20/TW19를 exact cancel/drain하고 Group lease를
   파기하는 전체 경로가 검증되지 않았다.

TW20/TW19 축별 manifest 48개 값도 계속 0이며 encoder family/socket과 motor-off 실기 증거가
없다. 이 blocker와 PLC activation matrix가 끝나기 전에는 gate 또는 capability bit를 켜지 않는다.

## 11. 2026-08-04 LMC Home safety-preemption cancel/drain source checkpoint

10절의 blocker 목록 중 LMC Home에 한정한 safety-preemption consumer가 canonical source에
추가됐다. 안전 admission이 exact reservation 검증을 통과하면
`LMCControlCommandService.HandleRequest`가 native Stop/PowerOff handler를 호출하기 전에
`ProcessAxisZeroHome()`을 동기 호출한다. 이 호출은 200-byte preemption snapshot의
144-byte header와 56-byte `0x7D13` identity를 검증하고, retained operation token으로
`LMCEcatInputLatch.CancelAxisZeroHome`을 먼저 게시한 뒤 normal Home 진행을 동결한다.

안전 명령 commit 뒤의 cyclic processor는 다음 순서를 사용한다.

1. 같은 old token/generation으로 `CopyAxisOwnershipPreemption`을 다시 읽는다.
2. `CLEANUP_REQUIRED`이면 cancel을 idempotent하게 다시 게시하고 exact RT terminal을 drain한다.
3. exact terminal이면 safe cleanup 또는 complete quarantine을, terminal을 증명하지 못하면
   incomplete quarantine을 `PublishAxisOwnershipPreemptionCleanup`으로 게시한다.
4. preemption 경로에서는 old token으로 일반 `PublishAxisOwnership`을 호출하지 않는다.
5. normal Home timeout도 즉시 terminal 처리하지 않고 cancel 뒤 exact RT terminal을 기다린다.

`CLEANUP_COMPLETE` replay는 retained Home을 `ABORTED/detail 39`로 복구하고,
`CLEANUP_QUARANTINED` replay는 `QUARANTINED/detail 42`로 복구한다. invalid snapshot, cancel 실패,
drain timeout 또는 cleanup publish 실패는 global ownership quarantine를 유지한다.

변경 뒤 `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1`은 PASS했다. 이 결과는
source ordering과 fail-closed 정적 계약만 증명한다. 이 변경 이후 C78 rebuild와
Find-in-Implementation smoke, PLC download, 실제 Stop/PowerOff 동시 요청, mailbox drain timing 및
실축 좌표 불변 검증은 아직 수행하지 않았다. DS402 Home, TW20/TW19 및 Group lease의
cancel/drain consumer도 이 절의 완료 범위가 아니다. 따라서 모든 관련 gate는 계속 `FALSE`로 둔다.

## 12. 2026-08-04 TW19/TW20 fixed-one 활성화 정정

8~11절의 TW19/TW20 gate OFF와 48개 manifest 판정은 각 작성 시점의 dormant checkpoint다.
현재 source는 아래처럼 정정됐다.

- `LMC_DIAG_ENCODER_TW20_ENABLED TRUE`
- `LMC_DIAG_ENCODER_TW19_ENABLED TRUE`
- `LMC_DIAG_ENCODER_RESET_VALUE 1`
- 축별 profile/socket/evidence manifest 제거
- TW20: selected drive의 `0x20FC:0x02`에 UInt16 `1`
- TW19: selected drive의 `0x20FC:0x01`에 UInt16 `1`

profile/socket/evidence wire field는 기존 72/76/156-byte recovery ABI의 exact identity로만
보존한다. SDO target과 value를 선택하거나 gate를 열지 않는다. WPF는 profile/socket 입력을
숨기고 drive/operation/timeout만 받으며 capability refresh 로그에 bit mask, Build, BootId,
MapRevision과 TW19/TW20 boolean을 남긴다.

현재 source의 nonzero-Boot 예상 Diagnostics CapabilityBits는 `0x000C633F`다. 09:33 C78
rebuild/download/restart 뒤 09:35 runtime은 `0x00000001`을 반환했다. 원인은 capability bit를
추가하는 세 UDINT 식이 정수 `OR`가 아니라 BOOL 결합 `|`를 사용해 전체 nonzero mask를 `1`로
축약한 것이었다. 후속 전수 감사에서 같은 오류가 verification flag 누적 7개 식과 retired
tombstone 1개 식에도 확인됐다. capability 3개 식과 maintenance 8개 식의 `|` 16개를 모두
정수 `OR`로 수정하고 verifier에 `|` negative fixture를 추가했다. 수정 binary의 clean C78
rebuild/download/restart 및 새 runtime mask 확인은 아직 필요하다.

Terminal success는 exact SDO completion, executor drain, motor-off post observation 및 owner cleanup
증거다. 실제 encoder reset 효과는 별도 확인해야 한다. TW19 뒤에는 motion 전에 LMC Home
current-position-zero가 필요하다.

별도 read-only review에서 11절 LMC Home cancel/drain은 cancel 실패 후 safety handler 진행,
terminal-before-cleanup restart window 및 unbounded normal drain 문제가 발견됐다. 이 Home 문제는
TW19/TW20 fixed-one correction과 분리하며, Home gate를 계속 `FALSE`로 유지하고 후속 수정한다.

## 13. 2026-08-04 first-axis Home evidence and owner-release receipt follow-up

2026-08-04 실기 시험에서 확인한 범위는 다음과 같다.

- 서로 다른 fresh PLC boot/session의 첫 Home에서 Axis1과 Axis2의
  current-position-zero/reference 효과는 관찰됐다.
- 11:12 로그의 fresh `BootId=0x00000013` 시도에서 Axis2 Start ACK와 후속
  `Read Home Status` RPC는 PASS했다. 그러나 당시 WPF는 exact 144-byte terminal
  outcome field를 로그에 남기지 않았다.
- 같은 boot에서 바로 다음 Axis3 Start는 `ErrorId=-31000`, `DetailCode=41`로
  수락되지 않았다.

따라서 첫 축의 native/reference 효과는 관찰 사실이지만 exact terminal success
증거는 아니다. Detail 41은 다음 admission 시점에 Home singleton/common owner가 계속
busy였음을 뜻한다. 첫 작업이 strict RT post-verification 후 Quarantined로 남아
owner가 release되지 않았다는 것이 현재 가장 강한 가설이다. 다만 이는 기존
로그로 확정할 수 없다. `Read Home Status PASS`나 Start ACK를 terminal success로
대체하지 않는다.

후속 source는 ordinary Home owner release에 slot 60..63 receipt를 추가했다. First publish는
`PREPARED`를 남기고 owner를 유지하며, caller acceptance 후 exact replay가 phase 2..6을
거쳐 cleanup을 완료한다. 중간에 재진입하면 같은 token/generation/mask/record
identity로만 이어간다. 이 수정은 현재 source/static 계약일 뿐이다. C78 Rebuild,
PLC download/restart, same-instance/warm continuation 실행 및 cold-restart persistent recovery는
아직 증명되지 않았다. Receipt 자체도 cold-persistent store가 아니다. 인증된 `COMPLETE`
receipt와 tuple이 다른 후속 publish는 일반 ownership 경로로 진행하며, incomplete/corrupt
receipt와 tuple이 다른 요청은 전역 quarantine으로 차단한다.

새 WPF는 exact outcome을 받으면 다음 prefix의 한 줄을 남긴다.

```text
LMC Home outcome: RecordState=...; HomeSucceeded=...; OriginalStatus=...; OriginalErrorId=...; OriginalDetail=... (...); AxisStatus=0x...; AxisError=...; RawDriveBefore=...; RawDriveAfter=...; ActualApplicationAfter=...; SetApplicationAfter=...; ActualInternalAfter=...; SetInternalAfter=...; DestinationInternalAfter=...; MasterInternalAfter=...; NativeCommandState=...; EvidenceFlags=0x...; StopState=0x... (...); RuntimePhase=...; RecordGeneration=....
```

`StopState`는 이전 wire field 이름을 유지하지만 실제로는 retained RT/service failure code다.
이 no-motion Home에는 별도 Stop 명령이 없다. raw 32-bit hex와 signed DINT 해석을 같이
출력하며, 예를 들어 `0xFFFFFFF9 (-7)`은 RT verification mismatch failure를 뜻한다.

2026-08-04 Axis2 실기에서는 `AxisStatus=0x22D0000A`, `AxisError=0`, native state `0`, LASAL
application/internal 좌표 6개가 모두 `0`이었지만 raw feedback이 `8382700 -> 8382701`로 한
count 변했다. 기존 exact-equality gate 때문에 `EvidenceFlags=0x1F`, `StopState=-7`,
`RecordState=Quarantined`, detail `38`이 게시됐고 다음 Start는 held owner 때문에 detail `41`로
거부됐다. 이 첫 로그를 근거로 source는 우선 raw 비교를 wrap-safe `-1/0/+1 count` 창으로
바꿨지만, 아직 C78 Rebuild/Download하지 않은 상태였다.

같은 날 Axis1 재시험에서도 `AxisStatus=0x2290000A`, `AxisError=0`, native state `0`, LASAL
application/internal 좌표 6개가 모두 `0`이었지만 raw feedback이 `8027834 -> 8027836`으로 두
count 변했다. 역시 `EvidenceFlags=0x1F`, `StopState=-7`, `RecordState=Quarantined`, detail `38`이
게시됐고 후속 Axis2/3 Start는 held owner 때문에 detail `41`로 거부됐다. 이 추가 실측에 맞춰
current source와 SDK parser의 raw 창은 wrap-safe `-2/-1/0/+1/+2 count`로 동기화했고,
`+/-3 count`와 wrap 경계의 `+/-3`을 거부하는 자동 시험을 둔다. 이 수정도 아직 C78
Rebuild/Download되지 않았으므로 새 PLC runtime 성공 증거가 아니다.

다음 실기 확인은 여러 축을 연속 시험하지 말고 다음 one-axis 절차로 제한한다.

1. 수정 source를 C78 Rebuild하고 PLC에 download한 뒤 PLC를 restart한다.
2. 새 WPF executable로 connect하고 fresh Build/BootId/MapRevision을 로그로 고정한다.
3. 한 축만 load하고 Home 전 `Read Home Status`를 1회 수행한다.
4. 그 축에 `LMC Home (Current Position Zero)`를 정확히 1회 수행한다.
5. 다음 축 Home을 누르지 말고, `LMC Home outcome:` 전체 한 줄과 Home 전/후
   축 status를 보존한다.
6. `RecordState`, `OriginalDetail`, `StopState`, raw before/after, application/internal 좌표,
   `EvidenceFlags`, `RuntimePhase`, generation으로 terminal과 owner release를 판정한 뒤에만
   다음 축 시험을 한다.

## 14. 2026-08-04 DS402 Home admission/final-evidence hardening

앱과 LASAL IDE를 닫은 뒤 tracked external source만 수정했다. IDE declaration, generated
channel, Network, C78 build와 PLC download는 수행하지 않았다. `LMC_DIAG_DS402_HOME_ENABLED`는
계속 `FALSE`다.

`TCPMotionInterface.MsgPaser`는 exact 72-byte method-37 Start와 valid axis에만 common axis
ownership reservation을 요청한다. malformed `0x7D15`는 ownership token을 만들지 않고
Diagnostics parser로 위임한다. valid Start의 reservation `-2`는 detail `41`, disconnected 또는
그 밖의 nonzero admission 결과는 detail `42`의 exact 24-byte 응답으로 보존한다. 이 mapper는
encoder-maintenance `0x7E53`에 적용하지 않는다.

`LMCDiagnosticsService`는 Outcome/Retire identity 실패를 shape/key `28`, Build `16`, Boot `17`,
Map `18`, storage/session `29`로 분리한다. stage `101`의 active record는 Running으로 숨기지 않고
indeterminate detail `26`을 반환한다. 성공 record와 stage `34`는 ActualPosition `0`, StatusWord
bit 3/13 clear, DS402 base state `0x0040/0x0021/0x0023/0x0027`를 모두 요구한다. stage `32/33`의
RT owner release cycle을 slot `112`에 저장하고, 그 이후 fresh latch cycle에서 pending SDO,
drain, mode restore, RT owner와 uncertainty slot `94/113/116/117/127`이 모두 `0`일 때만 terminal
success를 publish한다. overall/cleanup timeout 비교는 inclusive `>=`로 맞췄다.

Start는 common owner Commit 전에 exact record/runtime을 구성하고 slot `125=1`, record state `1`,
stage `89` 순서로 prepared state를 publish한다. Commit 성공 뒤에만 slot `125=2`, stage `1`과
ACK를 publish한다. warm continuation의 stage `89`는 preemption 조회보다 먼저 exact ACTIVE를
검증하고, 아니면 exact RESERVED에서만 Commit을 재시도한 뒤 ACTIVE를 다시 확인한다. 복구할 수
없으면 exact Rollback 성공 시 generation slot `109`를 보존해 state를 clear하고, 실패하면 stage
`101`에 남긴다. runtime clear는 slot `92..108`과 `110..127`로 분리해 slot `109`를 한 번도
지우지 않는다. normal state machine은 committed phase `2`만 허용한다.

cleanup stage `90..99`는 service clock 기준 1초가 지나면 exact SDO token의 `MarkOrphan`을 한 번
시도하고 pending/uncertainty flag를 지우지 않은 채 quarantine를 best-effort publish한 후 local
stage `101`로 종료한다. safety owner가 아직 RESERVED인 `PENDING_FREEZE`에서는 owner나 receipt를
변경하지 않고 1초 만료 시 local stage `101`로만 격리한다.

정적 검증 결과는 다음과 같다.

- DS402 Home Retirement/admission/runtime/warm-reconcile/bounded-cleanup negative fixture `61/61` PASS
- current source adversarial mutation `7/7` reject PASS
- current DS402 targeted source contract PASS
- `TCPMotionInterface.MsgPaser` UTF-8 크기 `31378` bytes, 상한 `32768` bytes 이내
- 변경 LASAL source 4개 7-bit ASCII PASS
- 관련 파일 `git diff --check` PASS
- full `-SourceOnly`는 새 DS402/size/transport 검사를 통과한 뒤, 이 작업과 무관한 기존
  `D5 SDO Write global production gate must remain FALSE` 정책 gate에서 중단

따라서 이번 수정은 gate를 열 근거가 아니다. 다음 blocker가 남아 있다.

1. DS402 owner-release durable receipt와 rollback-complete durable receipt
2. safety dispatch 전에 DS402 control-word bit 4를 low/readback/tombstone 처리하는 InputLatch drain
3. 위 ABI를 위한 LASAL IDE declaration/client regeneration 뒤 C78 build/download
4. motor-off 한 축 단독 method-37 실행, exact outcome/retire와 pcap proof

## 15. pending LASAL IDE declaration handoff

아래 ABI는 아직 IDE에 입력하지 않았다. external source 구현과 verifier를 먼저 준비한 뒤 사용자가
LASAL IDE에서 declaration/client wrapper만 생성하는 단계다. `GLOBAL`/private 구분과 입력 순서를
바꾸면 안 된다.

### 15.1 `LMCEcatInputLatch`

class variable 4개를 추가한다.

```text
Ds402HomeDrainRequestSequence : UDINT
Ds402HomeDrainAppliedSequence : UDINT
Ds402HomeDispatchSequence : UDINT
Ds402HomeDrainMailbox : ARRAY [0..7] OF DINT
```

아래 function은 `GLOBAL`로 추가한다.

```text
RequestDs402HomeSafetyDrain
  OperationToken : UDINT
  AxisReference : DINT
  Result : DINT
```

### 15.2 `LMCControlCommandService`

아래 function은 `GLOBAL`로 추가한다. `pDs402State`는 `^void`다.

```text
PublishAxisOwnershipDs402Receipt
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  pDs402State : ^void
  Ds402StateSize : UDINT
  Result : DINT
```

`LMCControlCommandService.InputLatch : CltChCmd_LMCEcatInputLatch`와
`LMCDiagnosticsService.AxisOwnership : CltChCmd_LMCControlCommandService`는 이미 존재한다.
IDE가 두 신규 호출 경로의 client wrapper를 새 ABI로 regenerate해야 하지만 `Comm_Network` 연결은 추가하거나
삭제하지 않는다. declaration 생성 확인 뒤에는 `Save All -> IDE 종료`까지만 하고, external
implementation과 verifier를 맞춘 뒤 별도 요청에서 C78 Rebuild한다.

## 16. 2026-08-04 IDE declaration completion and durable cleanup source checkpoint

사용자가 Section 15의 LASAL IDE declaration을 입력하고 Save All 후 IDE를 종료했다. 이 사실은
사용자 작업 완료 보고이며, 아직 C78 compiler와 downloaded PLC에서 ABI를 검증한 결과는 아니다.
이후 작업은 tracked external source와 정적 계약에 한정했다. 사용자 매뉴얼/API 매뉴얼은 구현 중
반복 갱신하지 않고 계약과 runtime evidence가 고정된 뒤 한 번에 정리한다.

### 16.1 measured LMC Home raw window

마지막 downloaded PLC의 Axis1 시험은 raw feedback `8027834 -> 8027836`, LASAL application/internal
좌표 6개 `0`, `AxisError=0`, `EvidenceFlags=0x1F`였지만 기존 raw gate에서 `StopState=-7`, detail
`38`, `RecordState=Quarantined`로 끝났다. 후속 Axis2/3은 남은 owner 때문에 detail `41`로
거부됐다. 따라서 그 PLC는 수정 완료본이 아니다.

당시 source와 SDK parser는 raw feedback의 wrap-safe `-2/-1/0/+1/+2 count`만 허용하고
`+/-3 count`부터 거부했다. 이 계약은 Section 19의 임시 SetPosition-only mode로 대체됐으며,
raw-qualified legacy branch와 signed 경계 test 자체는 보존한다.

### 16.2 DS402 rollback-only journal and receipt

zero-token preflight는 common ownership Reserve 전에 exact 72-byte request, session/sequence, 기존 local
record state, generation snapshot과 service cycle을 split WAL에 기록하고 stage `87`을 마지막 publication
word로 쓴다. 따라서 Reserve 직후 전원 차단되어 final Start 호출이 오지 않아도 cyclic processor가 exact
RESERVED tuple을 찾아 rollback-only stage `88`로 승격하거나, common owner 부재/검증된 later owner를
확인해 local intent만 폐기할 수 있다.

final Start와 tokenless receipt provider는 동일한 `ADOPT_MAGIC=0x44344144`를 사용한다. target tuple을
먼저 기록하고 adoption marker를 게시한 뒤 selected record state DINT를 `0`으로 명시적으로 게시한다.
그 다음 state word를 제외한 88-byte body만 지우고 재구성한 뒤 record state `1`, runtime mirror,
rollback-only stage `88` 순서로 commit한다. 이 순서는 기존 retired tombstone을 92-byte `_memset`으로
지우는 중 전원이 끊겨 중간 state가 남는 자동복구 창을 정적 DINT commit-word 모델에서 제거한다.
stage `88`은 Commit을 호출하지 않고 rollback receipt만 first/replay 처리한다.

snapshot freshness와 executor reusable gate가 모두 통과한 경우에만 stage `89`를 commit-ready word로
publish한다. stage `89` warm recovery는 exact ACTIVE, exact RESERVED -> Commit, exact ACTIVE recheck
순서만 허용한다. 실패한 Commit은 generic clear가 아니라 durable rollback receipt로 common owner
identity, observer, singleton과 record를 phase별로 제거한다. 성공한 cleanup은 원래 Build/Boot/Map/Gate
detail을 보존하고 detail `42`로 덮지 않는다. AxisOwnership client disconnect, validator `-1`, receipt
result `0/-1`은 stage `87/88/89` 또는 partial receipt를 시간 제한 없이 유지한다. 1초 bound는 owner
receipt 재시도가 아니라 별도 pending-freeze/cleanup 단계에만 적용한다. definitive identity mismatch,
torn local proof 또는 지원하지 않는 common surface만 stage `101`로 격리한다.

common cleanup 완료 또는 exact absence가 확인된 뒤 selected record를 지워야 하면 stage `86`을 먼저
게시한다. stage `86`은 retained slot `93`의 축 index를 검증하고 그 축의 92-byte record만 idempotent하게
지운 뒤 stage `0`을 마지막에 쓴다. slot `93`과 WAL은 먼저 지우지 않으므로 Axis2~4 재시작이 Axis1
record를 잘못 지우는 창이 없다. untouched raw tombstone을 보존해야 하는 result `2`는 record/WAL을
건드리지 않고 stage `0`만 게시하며, 남은 WAL은 다음 preflight가 새 intent를 쓰기 전에 덮어쓴다.

receipt provider는 first publication과 replay 모두 72-byte admission identity를 local record에서
재구성하고 exact owner phase를 검증한다. rollback은 stage `88/89/102`, terminal success는
`34/102`, terminal safe failure는 `100/102`에서만 허용한다. magic, phase, kind, generation과
각 ownership surface가 일치하지 않으면 fail closed한다. finalizer는 retained record를 먼저
commit한 뒤 lifecycle stage `0`을 마지막에 publish한다.

### 16.3 DS402 bit-4 safety drain

`LMCEcatInputLatch`는 DS402 Home control-word bit 4 HIGH를 실제 connected drive에 쓰기 직전에
monotonic `Ds402HomeDispatchSequence`를 증가시킨다. `0xFFFFFFFF`에서는 wrap하지 않고 sticky
fail-closed `-7`을 반환한다. safety drain은 exact operation token/axis, request/applied sequence,
dispatch barrier, RT owner/alignment ledger를 검증한 뒤 bit 4 LOW를 쓰고 readback한다.

LOW/readback이 확인되기 전에는 같은 scan의 normal Home mailbox write와 result publication을 모두
막는다. 확인 후 exact pending normal command를 `-6`으로 retire하고 RT owner/alignment를 clear한 뒤
drain receipt payload와 applied sequence를 순서대로 publish한다. completed receipt는 허용 result
domain `0/-1/-2`, request/applied tuple, current dispatch barrier와 stable atomic reread가 모두 맞을
때만 terminal로 사용한다. trusted internal producer는 restart 또는 normal command 이전의 idle
ledger에도 no-work LOW tombstone을 만들 수 있다.

common ownership의 safety admission은 handler/native dispatch보다 먼저 overlapping preempted DS402
record를 검사한다. dispatch evidence가 있으면 위 drain의 exact terminal `0`이 확인된 경우에만
safety handler를 호출한다. drain result `1`은 같은 admission token/generation/session/sequence의
`RESERVED` tuple을 TCP active request tail에 이중 checksum으로 보존하고, wire response와 두 번째
Reserve, native handler, Commit/Rollback을 모두 금지한 채 다음 cyclic scan에서 같은 tuple로
`HandleRequest`를 다시 호출한다. terminal result만 service가 ownership을 finalize한다. corrupt context,
1000 ms timeout 또는 disconnect는 exact tuple rollback과 session epoch fence 뒤 handler를 호출하지
않는다. retained LMC current-position-zero Home의 기존 safety pump도 그 뒤에 별도로 검사한다.

### 16.4 current static evidence and remaining boundary

현재 pre-IDE source staging은 아래 **다섯** waiver surface를 갖는다. diagnostics method-size split
verifier와 전체 정적 실행은 current split source에서 다시 완료했다. 이전 네-waiver checkpoint의
수치를 현재 결과로 재사용하지 않았다.

- pre-IDE source staging command:
  `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1
  -AllowPendingTcpSafetyHelperDeclaration -AllowPendingControlSafetyRepeatHelperDeclaration
  -AllowPendingTcpRpcLifecycleHelperDeclaration
  -AllowPendingAxisRebaseRequiredStateDeclaration
  -AllowPendingDiagnosticsMethodSplitDeclarations`
- 다섯 pre-IDE waiver는 각각
  `TCPMotionInterface.HandleControlSafetyDrainPending`,
  `LMCControlCommandService.HandleAxisOwnershipSafetyRepeat`,
  `TCPMotionInterface.HandleRpcLifecycleCommands`,
  `LMCControlCommandService.AxisRebaseRequiredState` 및 두 private codec function의
  declaration/metadata, `LMCDiagnosticsService`의 신규 private method-size helper 3개 declaration/metadata
  생성을 기다리는 동안에만 사용한다.
- 위 다섯-waiver `-SourceOnly` 전체 실행 PASS
- `-AllowPendingDiagnosticsMethodSplitDeclarations`만 제거한 실행은
  `LMCDiagnosticsService.HandleEncoderMaintenancePreemption`의 exact private IDE declaration 누락에서 FAIL
- diagnostics method-size split negative mutation `23/23` reject
- TW19 retained Home barrier negative mutation `37/37` reject
- encoder-maintenance negative mutation `56/56` reject
- ownership activation/repeated-safety/preemption-cleanup negative mutation `271/271` reject
- `PublishAxisOwnershipPreemptionCleanup` focused semantic mutation `24/24` reject; persistent mutation
  exact inventory, replacement 9-axis absence loop, quarantine observer loop, replay/result domain과
  clock/client 재표본 금지를 포함
- `PublishAxisOwnershipDs402Receipt` focused semantic mutation `55/55` reject; exact closed public ABI와
  receipt/stage constant ABI,
  persistent mutation `77`개/order SHA-256
  `95A9EAF512D0F4DCB5B406F2FB8B1B433A420A8C729C722AF3BC7C41B93388BA`, token/generation pair,
  direct/offset/alias pointer write, pState reassignment, input-guard/call allowlist, validator-result 우회,
  Result/RETURN control flow, first receipt publication, replay no-mutation, ordered destructive clear와
  COMPLETE-last 및 full semantic-token fallback ratchet을 포함
- DS402 Home retirement negative mutation `50/50` reject
- split-aware direct contract 독립 audit에서 남은 monolithic-method 위치 가정 없음
- custom method-size debt ratchet self-test `5/5` PASS; 2026-08-05 pre-split 시점 6개 service class의
  qualified implementation `93`개를 전수 계산해 under-limit `86`, 증가 금지 baseline debt `7`을 확인
- integrated five-waiver full `-SourceOnly`는 위 size ratchet까지 포함해 다시 PASS
- 이 inventory 작업은 LASAL source, generated declaration, Network와 Section 17의 1 channel +
  8 private function handoff를 변경하지 않음
- DS402 receipt의 post-C78 별도 split은 Stage-87 always-return branch 전체를 private
  `HandleAxisOwnershipDs402ReceiptStage87Recovery`로 옮기는 계획만 확정했다. 예상 all-CRLF는
  adapter `22784`, helper `26175` bytes이고 reverse-inline은 current Control SHA-256을 byte-exact
  복원한다. 이 helper는 현재 Section 17 선언 목록에 추가하지 않음
- `RollbackAxisOwnership` focused semantic mutation `38/38` reject, comment-only positive fixture accept;
  exact closed five-input/one-output ABI, used constant define `47`개의 single canonical closure,
  include/conditional/undef/redefinition/executable-identifier macro 우회 금지,
  pointer/client/clock/custom-call 금지를 포함
- rollback persistent mutation `79`개/order SHA-256
  `FFA826951AFAD84F64A21788ED0590330D5FA6A92C22B89A0363E03F9CF3BB08`, Result/RETURN token `29`개
  SHA-256 `E03138AF05891034DAF1DFE79BAD9B3FB68B33E6D6730950068F622476E32A51`, whole semantic-token
  SHA-256 `B997DB4BE547EF3EE07B4A2D2C8CAFC0588A1BACE65FF3A59D78C1F5E9AE2142`를 고정
- focused entrypoint
  `Verify-LasalAxisOwnershipRollback.Fixture.ps1 -RunSelfTest -RepositoryRoot <repo>` PASS
- rollback fence 통합 뒤 ownership aggregate `271/271`, custom method-size `5/5`, integrated five-waiver
  full `-SourceOnly -ExpectedSdoWriteAxis 1` 재실행 PASS; source hash와 baseline debt `7` 불변
- rollback의 post-C78 별도 split은 read-only preemption-bank validator만 private
  `ValidateAxisOwnershipRollbackPreemptBank(ExpectedAxisMask, pRestoreContext, RestoreContextSize)`로 옮기는
  미적용 계획이다. 예상 all-CRLF는 adapter `30820`, helper `21655` bytes이고 reverse-inline은 current
  Control SHA-256을 byte-exact 복원한다. 이 helper도 현재 Section 17 선언 목록에 추가하지 않음
- current rollback은 durable receipt journal이 없어 mutation 도중 전원 차단 후 재개를 증명하지 않는다.
  semantic fence와 post-C78 size split은 이 runtime 경계를 해결한 것으로 간주하지 않음
- `PublishAxisOwnership` focused semantic mutation `69/69` reject, comment-only positive fixture accept;
  exact seven-input/one-output ABI와 same-name header 총수, used define `68`개의 canonical closure,
  whole-control define inventory `167`개 SHA-256
  `455C87BB8B4BEA396585B8EFD6A5D233FD7F5BB9A3B0870FAD8D3F7C62814B7F`, exact three-`usingLtd`
  pragma 외 모든 non-define directive와 line splice, pointer/client/clock 및 unsupported call을 금지
- one-pass lexical masking, exact class block 안의 declaration, persistent member array 8개의 exact
  type/bound/location, pragma/generated table/macro/custom implementation 상대 순서를 포함해 raw string,
  comment/string delimiter, macro alias와 선언/구현 relocation 우회를 차단
- publish local inventory `93`개 SHA-256
  `FF2AD6EE2FADB9C3C42C74D1BA671D477BAF7409907E99BBF1F7A021D24A19E5`, direct/bulk persistent
  mutation `83`개/order SHA-256
  `86AC17C8D876F87826F98F9EE160F4711E02A6BC82327E07CE1D75C064F53B99`; retained rebase helper
  call을 포함한 semantic mutation event는 `84`개
- publish Result/RETURN `47`개 SHA-256
  `92C5659611A0A4F3B7086490BD2623B2370230D477DEBC76F267D5F9428F6CBD`, whole semantic-token
  SHA-256 `FB6B7BE724A2AA4091004890B40B2A11E9AF35471C23CCE9738CDABA9CDDDE16`, lexical boundary token
  `9672`개 SHA-256 `59B54CCFBA25322103DA85FDF0C92C9BC8EEAA28026A2935B35C989CABEF785A`를 고정
- focused entrypoint
  `Verify-LasalAxisOwnershipPublish.Fixture.ps1 -RunSelfTest -RepositoryRoot <repo>` PASS
- 2026-08-05 publish fence 통합 당시 ownership aggregate `271/271`, custom method-size `5/5`, integrated
  five-waiver full `-SourceOnly -ExpectedSdoWriteAxis 1` 재실행 PASS; source hash와 baseline debt `7` 불변
- publish Home receipt는 same-service-instance warm continuation일 뿐 cold-restart journal이 아니다.
  일반 multi-axis clear/restore/bank mutation도 단계 journal이 없어 crash-atomic 또는 replay-idempotent로
  간주하지 않음
- publish는 table magic/BootId/startup proof/global latch와 command-phase authorization을 자체 완결형으로
  재검증하지 않는다. production caller ordering/whitelist가 precondition이다. 2026-08-07 current source는
  production call `19`, assigned Result `19`, consumed Result `19`, OPEN `0`이며 과거 `21/10/11`은
  synthetic regression fixture에만 남음
- publish의 post-C78 split은 private
  `HandleAxisOwnershipPublishHomeReceipt(...)`와 `PrepareAxisOwnershipPublishDecision(...)` 두 helper로
  적용됐다. 실제 post-IDE/pre-split capture는 CRLF `609947` bytes /
  `C636265238F44D73FDC483309BFB1FF48384EFCD7AF44EE487071CB467281AE5`, canonical LF `593113` bytes /
  `F923D5F5A2649B33911072537BFF4B9CB597FAB1C3C8E1D956C8AB5F3C80B2DC`다. declaration은
  `//Tables:` 직전, qualified empty stub은 EOF에 모두 Home -> Decision 순서로 존재했다. external body
  적용과 final C78 Rebuild 뒤 canonical LF는 `594938` bytes /
  `8715896406D3B99185C40FBE9C2F0E29170C2D57E1E58792515172EBDDC81E65`로 유지됐다. expected CRLF
  projection `611837` bytes / `B6A3D9368AA5A81ADD58B002A8504607443ACDAA6AD176E8193FFEBEC9552636`은 actual
  post-build source가 아니다. terminal EOL 제외 adapter/Home/Decision canonical-LF는
  각각 `26265/15035/24708` bytes이고 SHA-256은 `355A0EA7...` / `EF688642...` / `75804F7C...`다.
  inventory는 `98/95/3`이다. post-build `Classes.lcb`는 `8434505` bytes / `CA5CE9AB...`이고 Split
  exact private ABI record `1/1`, ordered input `7/10`, output 각 `Result : DINT`를 보존한다. project
  `.lcb`는 `634514` bytes / `438DE310...`, Network는 `23/23`, drift `0`, `B80867C9...`다. body rollback은
  declaration/stub을 유지한 채 F923/C636 post-IDE PRE를 복원하며, generated declaration/stub까지
  별도로 제거해야만 7EAB/A51E를 복원한다. 과거 Home `15027`, Decision `24697`, A293/C4B whole-source
  값은 실제 IDE capture 전 superseded planning simulation이다. Publish focused static contract와 TW19
  negative `37/37`, waiver 없는 full SourceOnly은 PASS했다. final C78/ARM Rebuild는
  16:31:49~16:32:12에 `0 errors / 61 warnings`, Compiler Done 2회, Linker Done, command succeeded
  `23.5 s`로 완료됐다. class-level `InputLatch`/`LMCAxis1` implementation search와
  `CInvalidArgException=0`도 PASS했고, post-build full contract는 `236.9`초에 generated Split ABI
  `1/1`로 PASS했다. PLC/runtime proof는 남아 있음
- `ReserveAxisOwnership` 안의 미선언 `preemptRecordBase` 5개 참조를 이미 선언된 동일 record-base local
  `probeRecordBase`로 교정했다. public/class ABI, local 수와 call/write/result 순서는 불변이다.
  교정 뒤 method raw/LF/all-CRLF는 `79880/77732/79881`, raw block SHA-256은
  `4ABD82FF0BC73FA343F6D1ACFA0FA951FA09B12BD2E2DAD1D9D76621DA0B7BFC`
- reserve focused semantic/structural mutation `62/62` reject, comment-only positive fixture accept;
  exact thirteen-input/one-output ABI, qualified implementation 26개와 모든 lexical `END_FUNCTION`의
  alternating order, macro-to-custom/final boundary와 모든 method gap, function별
  `IF/CASE/FOR/WHILE/REPEAT` stack closure를 포함한다. macro 앞/가짜 이웃/다른 function 내부/orphan end,
  top-level·three-method·cross-function wrapper와 balanced-count bad-order 변형을 모두 fixture로 고정
- reserve fence 최종 통합 뒤 ownership aggregate `271/271` reject. 다른 function의 body-only semantics는
  이 fence의 의도적 비범위이며, 특히 균형 잡힌 `HandleRequest` body 변경까지 동결하려면 별도
  HandleRequest semantic/lexical fence가 필요함
- reserve fence 최종 통합 뒤 five-waiver full
  `-SourceOnly -ExpectedSdoWriteAxis 1`과 method-size self-test `5/5` 재실행 PASS; 6 classes / `93`
  methods / under-limit `86` / baseline debt `7`, current Control SHA-256 불변
- reserve local inventory `81`개 SHA-256
  `55AC47497D5CA174A5837D094607699207F2AE8E6DA761C9F67ED86EF48BFF1`, persistent/output mutation
  `110`개/order SHA-256
  `BBBDA1315DD5D184A3DB2F9CB55BE264022726743E8B4060B6FC9629D7609361`, Result/RETURN `127`개
  SHA-256 `5F438C3D4FEE2F1F024D9AA84025C21AB3C6CB69488B2D12556855B861686154`를 고정
- reserve whole semantic-token SHA-256
  `9E0A14511F49B47D174CECC978749BAE5C8B4D42D5E934A020BEC2158322C85E`, lexical token `11839`개
  SHA-256 `F13EDA75E7EFF379D407E88EC5CE2C37BA3445A3FED0C7D59B3DB9C53517246F`;
  `ops.tAbsolute` read exact 2개, corruption latch write 9개, magic-last와 final output order를 고정
- production caller는 TCP의 exact 3곳이다. DS402와 LMC Home은 `Result=0`만 service 진입을 허용하고,
  ordinary Axis/Group은 음수만 거부해 `0/+1/+2`를 repeated-safety helper에 전달한다. encoder `0x7E53`은
  Reserve 실패 뒤 diagnostics handler에 도달할 수 있으나 downstream token/identity gate가 fail-close하며
  ownership `-2/-3` detail은 encoder detail `9`로 축약됨
- reserve의 post-C78 별도 split minimum은 private
  `ValidateAxisOwnershipReserveSurface(...)`와 `PrepareAxisOwnershipReserveDecision(...)` 두 helper다.
  예상 all-CRLF는 adapter `31061`, surface helper `28152`, decision helper `29255` bytes, planned whole
  Control SHA-256은 `88EFF5F607AB415834F9C9A86741D77CF2DCBBE69A73CFEB9B09B6EEF40A94C6`이며
  reverse-inline은 교정 후 current Control SHA-256을 byte-exact 복원한다. 두 helper 모두 현재
  Section 17 선언 목록에 추가하지 않음
- reserve backup/main magic-last ordering은 crash-atomic proof가 아니다. durable receipt/replay journal,
  authenticated caller identity와 pointer capacity/alias 검증이 없고 token/generation wrap 경계도 남으므로
  C78/download/power-loss runtime 증거 없이 완료로 간주하지 않음
- current `LMCDiagnosticsService.st`: all-CRLF `266206` bytes, SHA-256
  `348E45AD486B4072D0105E7C0800B31BAF30A0B908F8AD2A5D2C26D3E46496E8`
- `LMCDiagnosticsService` function inventory `25`개는 모두 all-CRLF `32768` bytes 미만이며 최대
  `ProcessAxisDs402Home`은 `30376` bytes
- 세 helper를 원 위치에 다시 inline한 reverse transform은 split 전 all-CRLF `260860` bytes,
  SHA-256 `1F9CC2DB681BB16A1D347A1D0A1FB45A016DA8C92B53CE5DBB04C74F40BA74AC`를 exact 복원
- 아래 항목은 diagnostics method-size split 전 checkpoint에서 이미 확정된 회귀 증거다.
- DS402 local durable-intent/receipt negative mutation `29/29` reject
- DS402 common receipt-provider negative mutation `17/17` reject
- DS402 same-RESERVED continuation negative mutation `25/25` reject
- ownership activation/repeated-safety negative mutation `247/247` reject; repeated-safety 신규 `27`
- 당시 TW19 retained Home barrier negative mutation `26/26` reject; current verifier는 위 `37/37`
- callback negative mutation 기존 `8`개와 RPC helper/route/ABI/size 신규 `7`개 reject
- Visual Studio 2019 MSBuild로 `LasalMotionControlLib.Tests` Debug build PASS
- `LasalMotionControlLib.Tests.exe`: `TOTAL 1082, PASSED 1082, FAILED 0`
- `TCPMotionInterface.MsgPaser`: raw/LF `28439`, all-CRLF `29209` bytes
- `TCPMotionInterface.HandleControlSafetyDrainPending`: raw/LF `16468`, all-CRLF `16916` bytes
- `TCPMotionInterface.HandleRpcLifecycleCommands`: raw/LF `4249`, all-CRLF `4403` bytes
- `LMCControlCommandService.ReadAxisRebaseRequiredMask`: raw/all-CRLF `744`, LF `718` bytes
- `LMCControlCommandService.UpdateAxisRebaseRequiredState`: raw/all-CRLF `1034`, LF `997` bytes
- `LMCControlCommandService.HandleAxisOwnershipSafetyRepeat`: raw/LF/all-CRLF `30179/29315/30180` bytes
- `LMCControlCommandService.HandleRequest`: raw/LF/all-CRLF `32574/31728/32575` bytes
- `LMCControlCommandService.CommitAxisOwnership`: raw/all-CRLF `5102`, LF `4947` bytes
- `LMCControlCommandService.HandleGroupCommands`: raw/all-CRLF `23395`, LF `22691` bytes
- 위 method는 raw/LF/all-CRLF 모두 `32768`-byte ceiling 이하
- `LMCControlCommandService.st` SHA-256
  `C976CD364010EEFDFDDA8D7BC6D7655293DAD221FBEC908D50E5805CE4AFF072`
- RPC 3-arm in-memory reverse transform이 추출 전 TCP source SHA-256을 byte-exact 복원
- 변경 LASAL source 7-bit ASCII PASS
- 관련 source/verifier `git diff --check` PASS, line-ending warning만 존재
- 이번 checkpoint에서는 사용자/API/배포 매뉴얼을 갱신하지 않았고, C78/runtime 안정화 전까지
  매뉴얼/README/HTML의 반복 갱신을 계속 보류함. architecture/IDE handoff 설계 근거만 구현과 함께 갱신

아직 증명하지 않은 경계는 다음과 같다.

1. Section 17의 hidden retained server channel 1개, private helper 8개 IDE declaration과
   generated metadata 확인
2. `TCPMotionInterface`, `LMCControlCommandService`, `LMCDiagnosticsService` 세 class의 generated
   declaration/`Classes.lcb` external inspection과 다섯 pre-IDE waiver 제거
3. LASAL C78 Rebuild와 generated declaration/client ABI compile
4. 변경 function의 `Edit Method` 직접-open smoke, Object Network channel의
   `Find in Implementation` class-index smoke와 그 시작 이후 `%TEMP%\Lasal2.log`의 새
   `CInvalidArgException` 부재
5. PLC cold download/restart 후 새 `BootId`와 capability/map identity
6. 한 축 LMC Home의 exact terminal success 및 owner release
7. 다른 축의 다음 Start가 detail `41` 없이 admission되는 연속성
8. DS402 method-37, safety preemption drain, TW19/TW20의 새 PLC 실기 증거
9. `AxisRebaseRequiredState` encoded word와 실제 restart/power-loss retention의 target 증거
10. restart 시 no-owner bit-4 HIGH 자동 drain과 관련 gate들의 atomic activation
11. `PublishAxisOwnership`의 `Result`를 소비하지 않는 production caller `11`곳의 fail-closed 결과 처리와
    일반 multi-axis publication의 cold-restart recovery 설계

`LMC_DIAG_DS402_HOME_ENABLED`와 `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED`는 계속 `FALSE`다. 정적 PASS는
gate 활성화나 PLC runtime 완료를 뜻하지 않는다. 다음 단계는 Section 17의 IDE declarations를 먼저
생성하고 external inspection을 통과한 뒤 사용자에게 C78 Rebuild 결과를 요청한다. compiler error와
IDE smoke 이상이 없을 때만 download와 one-axis test handoff로 진행한다.

## 17. 2026-08-04 one-visit retained-barrier and eight-private-helper IDE handoff

same-RESERVED retry, repeated safety escalation, LASAL Save All CRLF normalization과 retained word
codec, diagnostics oversized cyclic method의 preemption/receipt/cleanup을 각각 독립된 implementation
method로 분리했다. 여덟 implementation은 tracked source에 있지만 LASAL generated declaration과
`Classes.lcb` metadata는 아직 없다. TW19 뒤 다음
PowerOn/motion을 exact LMC Home 성공까지 차단할 hidden retained server channel도 같은 IDE 방문에서
선언해야 한다. 현재 상태에서 Rebuild하지 않는다.

한 번의 LASAL IDE 방문에서 아래 channel 1개와 function 8개를 추가한다. 여덟 function은 모두
**private**이며 `GLOBAL` 또는 `VIRTUAL GLOBAL`로 만들지 않는다. input/output 이름, type, 순서를
바꾸지 않는다. `Comm_Network`를 포함한 Network 변경은 없다. Save All 뒤 세 class를 external
inspection하기 전과 다섯 pre-IDE waiver를 제거하기 전에는 Rebuild하지 않는다.

### 17.1 `TCPMotionInterface`

```text
HandleControlSafetyDrainPending
  Phase : UINT
  EffectiveAxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  Result : DINT
```

```text
HandleRpcLifecycleCommands
  no inputs
  no outputs
```

`HandleRpcLifecycleCommands`는 function 자체만 만들고 input/output 행을 추가하지 않는다.

### 17.2 `LMCControlCommandService`

먼저 class variable에 아래 hidden server channel을 추가한다.

```text
AxisRebaseRequiredState : SvrCh_UDINT
  Initialize     = true
  DefValue       = 0x5242530F
  WriteProtected = false
  Retentive      = File
  Visualized     = false
```

`AxisRebaseRequiredState`는 Comm Network에 연결하지 않는다. client channel, global function 또는
visualized UI channel로 만들지 않는다. LASAL property editor가 hexadecimal prefix를 `16#`로
표시하면 `16#5242530F`를 사용하되 generated UDINT value가 exact `0x5242530F`인지 external
inspection에서 확인한다.

```text
HandleAxisOwnershipSafetyRepeat
  CommandId : UINT
  Reference : UINT
  pRequestFrame : ^USINT
  RequestFrameSize : UDINT
  pResponseFrame : ^USINT
  ResponseCapacity : UDINT
  CallerSessionEpoch : UDINT
  RequestSequence : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  Result : DINT
```

```text
ReadAxisRebaseRequiredMask
  no inputs
  Result : DINT
```

```text
UpdateAxisRebaseRequiredState
  SetAxisMask : UDINT
  ClearAxisMask : UDINT
  Result : DINT
```

`ReadAxisRebaseRequiredMask`는 input 행을 만들지 않는다. 두 codec function도 private이며
`GLOBAL`/`VIRTUAL GLOBAL` 또는 class channel로 만들지 않는다. persistent word decode/update를
already-large reservation/publication method에 다시 inline하지 않는다.

### 17.3 `LMCDiagnosticsService`

```text
HandleEncoderMaintenancePreemption
  Stage : DINT
  PreemptionResult : DINT
  pPreemptionSnapshot : ^USINT
  ServiceNow : UDINT
  Result : DINT
```

```text
HandleAxisDs402HomeReceiptStages
  Stage : DINT
  Result : DINT
```

```text
HandleAxisDs402HomeCleanupStages
  Stage : DINT
  AxisReference : UINT
  AxisMask : UDINT
  AdmissionToken : UDINT
  OwnerGeneration : UDINT
  Ds402ProcessOwnerSessionEpoch : UDINT
  OwnerRequestSequence : UDINT
  SafetyAdmissionToken : UDINT
  SafetyOwnerGeneration : UDINT
  InitialCurrentCycle : UDINT
  ServiceNow : UDINT
  ActualPosition : DINT
  PreemptionCleanup : BOOL
  no outputs
```

세 function은 모두 private이다. 첫 두 function은 exact `Result : DINT` output 하나만 만들고,
cleanup function은 output 행을 만들지 않는다. `InitialCurrentCycle`을 다른 이름으로 바꾸지 않는다.

작업 순서는 다음과 같다.

1. `LMCControlCommandService.AxisRebaseRequiredState` channel과 위 property 5개를 exact 추가한다.
2. 위 private function declaration 8개를 세 class에 추가한다.
3. Save All 한다.
4. **Rebuild하지 않고** LASAL IDE를 종료한다.
5. `TCPMotionInterface`, `LMCControlCommandService`, `LMCDiagnosticsService` 세 class의 generated
   `.st` declaration, hidden channel property와 `Classes.lcb` metadata를 external inspection한다.
6. `AxisRebaseRequiredState`에 Comm Network connection이 없고 Visualized가 `false`인지 확인한다.
   다른 Network object에도 connection 추가/삭제가 없는지 확인한다.
7. Save 뒤 TCP/Control/Diagnostics source hash와 각 method의 actual/LF/all-CRLF size를 다시 측정한다.
8. default `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1`이 다섯 pre-IDE waiver switch 없이
   PASS한 뒤에만
   별도 요청으로 C78 Rebuild한다.
9. Rebuild 뒤 Object Network Server/Client는 `Find in Implementation`으로 확인하고, 변경
   function/method는 `Edit Method` 또는 `Enter`로 exact Implementation header를 직접 연다.
   smoke 시작 이후 `%TEMP%\Lasal2.log`의 새 `CInvalidArgException=0`도 확인한다.

retained word는 상위 magic `0x524253`, 하위 mask와 inverse nibble을 사용한다. 초기
`0x5242530F`는 4축 모두 Home 필요, empty `0x524253F0`는 4축 모두 완료를 뜻한다. 형식이 틀린
값은 effective `0xF`로 fail closed한다. TW19는 exact owner commit에서 SDO 전에 selected bit를
set하고, exact LMC Home terminal-success receipt COMPLETE만 Result `1` 직전에 해당 bit를 clear한다.
이 계약의 상세 admission matrix와 power-loss proof는
[axis ownership overlay Section 10](./LMC_AXIS_OWNERSHIP_OVERLAY_IDENTITY_RESTORE_IDE_HANDOFF_2026-08-04.md#10-tw19-retained-current-position-zero-barrier)를 따른다.

같은 checkpoint에서 SDK adapter error catalog는 v2와 symbolic `-9 AxisOwnershipConflict`까지 source,
MSBuild와 C# test `1082/1082`를 통과했다. PLC admin catalog version `5`와 SDK adapter catalog version
`2`는 서로 다른 catalog이므로 숫자를 같게 맞추지 않는다. ordinary ownership, DS402 Home, startup
bit-4 sweep과 Admin bit 6은 아직 all-dormant 상태이며 C78/download/runtime evidence 전에는 켜지 않는다.

## 18. 2026-08-05 post-IDE C78 type-fix checkpoint

Section 17의 hidden server channel 1개와 private function declaration 8개는 canonical LASAL
project에 저장됐다. generated class declaration과 `Classes.lcb` metadata가 존재하며 다섯 pre-IDE
waiver를 제거한 다음 계약은 PASS했다.

```powershell
& 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalContract.ps1' `
  -RepositoryRoot '.' -SourceOnly -ExpectedSdoWriteAxis 1
```

첫 C78 Rebuild는 `LMCDiagnosticsService::HandleAxisDs402HomeStart`의 `_memcmp` 네 곳에서
`UDINT` 반환값을 `DINT` local에 대입한 `E0166` 네 건으로 실패했다. 기존 `copyResult`와
`ownerResult`는 함수 후반의 signed API result와 `-1` sentinel을 받아야 하므로 타입을 바꾸지
않았다. 대신 다음 비교 전용 local을 추가하고 네 `_memcmp` 결과와 zero 비교만 분리했다.

```st
intentBodyCompareResult, intentTailCompareResult : UDINT;
```

이 수정은 function ABI, channel, retained state, wire layout과 Network를 바꾸지 않는다. verifier는
DS402 Start의 `_memcmp` assignment가 정확히 네 개이며 모든 receiver가 `UDINT`인지 확인한다.
receiver type을 `DINT`로 되돌리거나 기존 signed local을 다시 사용하는 두 negative fixture도
추가했다.

Rebuild가 생성한 다음 항목은 `Initialize=true`의 instance initialization representation이다.

- `Comm_Network.lcn`: `<Server Name="AxisRebaseRequiredState" Value="16#5242530F"/>`
- `ONE_Comm_Network_Table.st`: initialization value `16#5242530F`
- `Networks.lcb`: generated channel metadata

실제 `.lcn` Source/Destination endpoint와 generated Internal/External connection entry는 모두 0개다.
따라서 verifier는 initialization representation을 허용하고 actual connection만 거부한다.
AxisRebase barrier self-test는 `37/37` negative fixture를 거부했다.

첫 실패 로그의 `55 warnings`는 다음처럼 분류한다.

- `W0069` 35건: compile-time feature gate 30건과 고정 `sizeof` invariant 5건
- `W0072` 17건: helper extraction 뒤 남은 unused local
- `W0073` 3건: write-protected channel setter가 의도적으로 무시하는 ABI parameter

즉시 논리 결함으로 분류된 warning은 0건이다. 현재 최소 수정으로 다시 C78 Rebuild할 때 기준은
`0 errors / 55 warnings`다. 이 기준선을 먼저 닫고, `W0072` local 17개 정리는 별도 warning-cleanup
변경으로 수행한다. `W0069` 조건을 warning 회피 목적으로 재작성하거나 `W0073` parameter를 ABI에서
제거하지 않는다.

현재 남은 exact 순서는 다음과 같다.

1. canonical project에서 C78 Rebuild만 실행해 `0 errors / 55 warnings`인지 확인한다.
2. Object Network Server/Client는 `Find in Implementation`으로 확인하고, 변경 function/method는
   `Edit Method` 또는 `Enter`로 exact Implementation header를 직접 연다. smoke 이후 새
   `CInvalidArgException=0`도 확인한다.
3. 그 전에는 추가 feature, method split, Link/download와 PLC write/motion을 진행하지 않는다.
4. 기준선 성공 뒤 warning cleanup과 post-C78 method split을 각각 독립 tranche로 진행한다.

## 19. 2026-08-05 temporary SetPosition-only Home mode

downloaded BootId `0x18`의 Axis1 LMC Home은 native `SetPosition` 반환과 application/internal
좌표 6개가 모두 정상인데도 raw feedback `8028436 -> 8028440`의 `+4 count` 때문에
`StopState=-7`, detail `38`, `RecordState=Quarantined`로 끝났다. 후속 축은 같은 owner quarantine
때문에 detail `41`로 admission 거부됐다.

축별 native 호출은 이미 `LMCEcatInputLatch::RtWork`에서 다음 형태로 정확히 한 번 수행된다.

```st
LMCAxis1..4.SetPosition(
    Mode:=LMCAXIS_SET_ACTPOS_APPUNIT_DEST,
    Position:=0)
```

임시 source 변경은 이 호출 위치, RT mailbox, cancellation fence, Standstill, AxisError, stale
expected-position guard, native-call count, ownership lifecycle과 retained outcome을 유지한다. 실제
raw before/after도 그대로 기록하지만 raw delta 계산과 성공 gate는 제거한다. 검증하지 않은 RAW
bit는 세우지 않으므로 성공 evidence는 `0x3B`다.

- expected request: `0x01`
- Standstill/AxisError state: `0x02`
- application coordinates zero: `0x08`
- internal/destination/master coordinates zero: `0x10`
- three stable post samples: `0x20`
- raw delta qualified: 제외

SDK parser는 `0x3B`일 때 raw delta와 무관하게 나머지 성공 조건을 적용한다. 기존 `0x3F`와 raw
wrap-safe `+/-2 count` gate는 현재 source에서 제거됐으며 임시 변경을 revert해 원래 계약으로
되돌릴 수 있다.
이 source 변경은 정적/C# 검증 대상이며 아직 C78 Rebuild, Download 또는 새 BootId runtime 성공
증거가 아니다. 현재 quarantined BootId는 source 변경만으로 복구되지 않으므로 새 build/download 뒤
새 BootId에서 축 하나씩 다시 확인해야 한다.

## 20. 2026-08-05 C78, download and BootId `0x1B` runtime checkpoint

Section 19의 미검증 상태는 같은 날 후속 C78 build/download와 새 BootId runtime으로 대체됐다.
세부 표와 supplied-log hash는
[260805 runtime evidence](../history/260805/04_runtime_evidence_boot_1b_home_group.md)에 기록했다.

- C78/ARM Rebuild: `0 errors / 55 warnings`, exact histogram
  `W0069=35`, `W0072=17`, `W0073=3`
- canonical download/link: 14:26 PASS; 직전 두 download는 `Timeout waiting CPU state`로 실패
- runtime identity: `BootId=0x1B`, `MapRevision=0x957F101E`, `DiagnosticsBuild=1`,
  `DiagnosticsBits=0x000C633F`, `AdminFeatures=0x17`
- Axis1..4 Home: 모두 terminal `Succeeded`, `HomeSucceeded=True`, `AxisError=0`, 좌표 6개 `0`,
  `EvidenceFlags=0x3B`, exact retirement PASS
- raw before/after delta: Axis1 `0`, Axis2 `0`, Axis3 `+1`, Axis4 `+1`; raw delta는 성공 gate가 아님
- record generation `1 -> 4`, 다음 축 admission과 Group Identity Home Check `4/4` PASS
- Group Power/Set Identity/Enable과 실제 non-Standstill basic motion PASS

따라서 temporary SetPosition-only Home mode는 이 downloaded checkpoint에서 4축 연속 runtime PASS다.
다만 이 결론은 original raw-window `0x3F` 계약을 복구할 근거가 아니며, actual in-motion Stop,
restart/power-loss rebase retention, TW19/TW20 physical effect와 DS402 Home을 증명하지 않는다.

이 LASAL session에는 새 `CInvalidArgException`이 없지만 required three-class
`Find in Implementation` 검색 기록도 없다. implementation smoke는 계속 열린 IDE gate다. 다음 source
작업은 `PublishAxisOwnership` Result 미소비 production caller 11곳의 fail-closed semantic tranche이며,
`LMC_DIAG_DS402_HOME_ENABLED`와 `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED`는 계속 `FALSE`로 둔다.

## 21. 2026-08-06 DS402 receipt Stage-87 method-size split checkpoint

Section 8.3의 `PublishAxisOwnershipDs402Receipt` oversized debt를 줄이기 위해 canonical
`LMCControlCommandService`에 다음 private function declaration을 추가했다. Section 17의 기존 8개
helper와 별도인 아홉 번째 helper다.

```text
HandleAxisOwnershipDs402ReceiptStage87Recovery
  pState : ^USINT
  activeIndex : DINT
  AxisMask : UDINT
  ReportKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT
```

final generated declaration과 implementation header에는 `GLOBAL`/`VIRTUAL GLOBAL`이 없다.
`Classes.lcb` record flags는 `0x00000000`, input count는 `7`이며 output은 exact `Result : DINT`다.
`HandleRegistryCommands`는 원래 6-input ABI로 보존됐다. IDE Save All 뒤 implementation split 전
Control snapshot은 `606170` bytes, SHA-256
`BAB60FF1891F424B132C52EF3FBF5D099AB010BFF1D7E812648DFA7BF619BE7A`다. 같은 시점
`Classes.lcb` SHA-256은
`DC71B0F8B8A493B84D2BE0A294408E462FEF87D758F28F9AA8C50C1F32124B7B`다.

Stage-87 tokenless always-return branch의 outer wrapper를 제외한 588줄을 helper로 옮겼다. adapter는
tokenless 조건에서 helper를 한 번 호출하고 즉시 return한다. reverse-inline은 위 pre-split Control
snapshot을 byte-exact 복원한다. current source와 method-size 결과는 다음과 같다.

- `LMCControlCommandService.st`: `606348` bytes, SHA-256
  `DA93EB01DBF7E842C36EE22E1ACBF6277D60C0E12C58B93A24BA870976321FCF`
- public adapter raw/LF/all-CRLF `21836/21279/21837`
- private helper raw/LF/all-CRLF `26182/25531/26183`
- local inventory adapter/helper `35/42`
- persistent mutation adapter/helper/transitive `28/49/77`
- custom methods total/under-limit/debt `95/90/5`

focused split-aware negative fixture는 `67/67`, method-size self-test는 `6/6`, waiver 없는
`Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1`은 exit `0`으로 PASS했다. 독립 재실행과
diff review에서도 actionable finding이 없었다. `Comm_Network.lcn`, `ONE_Comm_Network_Table.st`,
`Networks.lcb` SHA-256은 이전 checkpoint와 같아 Network 연결 변경은 없다.

2026-08-07 LASAL Class 2 `02.03.001`의 C78/ARM Rebuild는 `26318.1 ms`에 성공했고 IDE 결과는
`0 errors / 55 warnings`다. WARN line `61`개 중 `55`개는 기존 source warning(`W 0069=35`,
`W 0072=17`, `W 0073=3`), `6`개는 C78/C81 version warning이며 ERROR/FATAL은 0개다. 실제
`Comm_Network.LMCControlCommandService1.LMCAxis1`에서 `Find in Implementation`을 실행해 `29` hits,
`1` matched file / `3` searched files로 성공했고 smoke 이후 `CInvalidArgException=0`이다. Save All 뒤
`Classes.lcb`는 SHA-256
`9147D2185860FE2082777013FC944248196B686402FE88F7EF52FAB9875301E0`로 재기록됐으며 post-save
SourceOnly 재실행도 exit `0`이다. IDE는 종료했고 Download는 수행하지 않았다. 따라서 Section 8.4
rollback split의 current-source 재기준화는 진행할 수 있지만 PLC download/runtime 증거는 여전히
남아 있다. 두 dormant gate와 Axis1 SDO Write 시험 gate 상태는 바꾸지 않는다.

## 22. 2026-08-07 RollbackAxisOwnership post-IDE implementation checkpoint

Section 8.4의 rollback helper declaration을 LASAL IDE로 저장했다. post-IDE/pre-implementation Control은
`606820` bytes, SHA-256
`DAA8E134CE6E67BA47D6B30530F0FB9DBEF041A1B355466472872975897C3DF0`이고 `Classes.lcb`는
`8429648` bytes, SHA-256
`2AEFD0B004B9F0CE1688077FC5B842AB46B893C811A8951DF2E7F8CDF23406A5`다. 이 DAA8 snapshot에는
exact private declaration과 empty implementation stub가 있으며 Network 연결은 바뀌지 않았다.

실제 생성된 ABI는 다음과 같다.

```text
ValidateAxisOwnershipRollbackPreemptBank
  ExpectedAxisMask : UDINT
  pRestoreContext : ^void
  RestoreContextSize : UDINT
  Result : DINT
```

declaration과 implementation header에는 `GLOBAL`/`VIRTUAL GLOBAL`이 없다. generated declaration은
canonical LF/all-CRLF `207/216`, canonical LF SHA-256
`4BC23CE3F6FAC1F2E18CBC5D2AF7E2C27111834B8064E322AB5C6E66D0FD44E4`다.

DAA8 monolithic method는 line `5032..6337`, byte0 `[180762,230865)`, raw/LF/all-CRLF
`50103/48798/50104`, SHA-256
`2A88838417913B76449739447AAA8175157EAF8A370CC53F7FF916A3F25FF745`다. 안전한 extraction은 두 번째
`preemptBankValid := TRUE;`인 line `5375..5879`, byte0 `[192424,212796)`, raw/LF/all-CRLF
`20372/19867/20372`, SHA-256
`9A6EFE09CBE17D062802245E06974BF80AA7268D95489DEB8C137A0E1F68A62C`다. 바깥
`if restorePreempt then`/`end_if` line `5374`/`5880`은 adapter에 남겼다.

DAA8에서 rebase한 candidate를 canonical source에 적용했다. current Control의 IDE CRLF checkpoint는
`608436` bytes, SHA-256
`A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753`이고 Git canonical LF는
`591670` bytes, SHA-256
`7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE`다. planner의 whole-source
결과는 두 projection 모두 exact다.

- adapter canonical LF/all-CRLF `29124/29922`, canonical LF SHA-256
  `8855AEEAE9B617CEAC1D10C7CC4ADB7F4D0536D108592560CE0D39ACF344AFAC`
- helper canonical LF/all-CRLF `21451/22046`, canonical LF SHA-256
  `AE6AD76007725544FBC57D8D60DF5C483CD3381149A1D14C424C96BCBEE0AF09`
- call map canonical LF/all-CRLF `758/776`, canonical LF SHA-256
  `66E328773321E978F63BF13F3080E77193D27D69E704081A7205D366EC76FF55`
- reverse-inline은 DAA8 post-IDE source를 byte-exact 복원한다.

helper는 NIL, exact 40-byte size와 mask 범위를 검사하고 full validation 성공 뒤에만 Group active,
mask, token, generation, session, sequence, identity size, command/reference/admission bit pattern의 10개
UDINT slot을 게시한다. persistent write, `_memset`, `_memcpy`, client/clock call은 없다. helper nonzero는
public output으로 직접 흘리지 않고 adapter의 기존 위치에서 `Result := -3; RETURN;`으로 변환한다.

현재 정적 검증 결과는 다음과 같다.

1. current adapter/helper split verifier `20/20` expected semantic rejection PASS
2. ownership aggregate `287/287` PASS
3. method-size methods/under-limit/debt `96/92/4` PASS
4. waiver 없는 `Verify-LasalContract.ps1 -SourceOnly -ExpectedSdoWriteAxis 1` exit `0` PASS
5. pre-Rebuild `Classes.lcb`와 Network 세 파일 hash 불변

DAA8 one-shot planner의 `18/18`은 candidate construction/reverse proof이고, current A51E의 CRLF와
fresh-checkout LF 입력에서 모두 통과한다. A51E current acceptance는 main verifier의 adapter/helper
composite fence가 담당한다. 두 gate를 같은 증거로 취급하지 않는다.

A51E canonical project의 Save All과 C78/ARM Rebuild는 2026-08-07 11:39 KST 단일 LASAL 세션에서
수행했다. baseline 입력 8개는 build 뒤 byte-exact이고 rebuild command window는 compiler error `0`, coded
warning `55`개(`W0069=35`, `W0072=17`, `W0073=3`), result 뒤 C78/C81 compatibility warning `6`개,
필수 custom ST 6개 각 1회 compile, Linker `Done`, command success다. append 전체에서
`CInvalidArgException`과 download/online command는 `0`이다.

post-C78 `Classes.lcb`는 `8430171` bytes, SHA-256
`3B5D814F566F20D49D8033CC6E6F735A1503D91B7A3D5F87D3E6339FECC3421B`다. helper 이름의 두 번째
출현은 compiler compact symbol entry이고 detailed private ABI record는 정확히 한 개다. 565-byte record
SHA-256은 기존 값
`094573D70AC34005F1072D5FE88D705CD2D63BD8F4B3A16068228D97EFB4F337`와 같다. whole-binary 이름
유일성으로 이 정상 symbol entry를 거부하던 verifier를 exact method-header 후보 유일성으로 교정했고,
그 뒤 waiver 없는 full `Verify-LasalContract.ps1 -ExpectedSdoWriteAxis 1`은 exit `0`으로 PASS했다.

actual C78 build는 raw log로 확인됐지만 별도 GUI Build Output transcript가 없어 strict dual-evidence gate는
미완료다. function row의 우클릭 메뉴에는 `Find in Implementation`이 없으므로 두 exact function의 올바른
smoke는 각각 `Edit Method` 또는 `Enter`로 implementation header를 직접 여는 것이다. 별도로 Object
Network channel의 `Find in Implementation`은 class index smoke로 유지한다. 현재
`RollbackAxisOwnership`과 `ValidateAxisOwnershipRollbackPreemptBank`의 exact direct-open은 증명되지
않았다. download/restart와 PLC/실축 runtime도 아직 수행하지 않았다. 이 split은 method-size debt만
줄였고 durable power-loss rollback recovery journal을 추가하지 않았다.

## 23. 2026-08-07 current C78 rebuild, download and corrected IDE smoke checkpoint

A51E Control source를 바꾸지 않은 상태에서 사용자가 canonical 프로젝트를 다시 C78/ARM으로
Rebuild했다. latest command window는 13:27:53~13:28:15 KST이며 compiler error/fatal `0`, coded warning
`55`개(`W0069=35`, `W0072=17`, `W0073=3`), C78 project와 C82 library compatibility warning `6`개,
필수 custom ST 6개 각 1회 compile, Linker `Done`, command success다. 같은 세션의 바로 전
13:26:45~13:27:09 Rebuild도 성공했다. 두 build window에는 새 `CInvalidArgException`이 없다.

build-only gate 뒤 13:28:59~13:29:04에 PLC download가 추가로 실행되어 `279` files와 `Download Ok`가
기록됐고 13:29:10 project load 뒤 13:29:56 Offline으로 전환됐다. 이 download는 static/build 승인과
별도 external-state 변경이며 새 BootId, capability/map identity와 축 결과를 수집하지 않았으므로
runtime 기능 성공 증거로 사용하지 않는다. 추가 Download/Online은 금지하고 다음 runtime tranche에서
identity부터 다시 수집한다.

post-build observed baseline은 다음과 같다.

- `LMCControlCommandService.st`: `608436` bytes, SHA-256
  `A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753`
- `Classes.lcb`: `8430171` bytes, SHA-256
  `54617F162BF631ECD7779CC7356414E7BC95C1C680FA0A74587E5C9C5B3EE553`
- project `.lcb`: `634514` bytes, SHA-256
  `4F384C743CFF388967594F1F37F2CA51952134D3D53A9AEED201FDA2423F5F7A`
- `IOConnectionManager.xml`: IDE가 schema Version `2 -> 3`, formatting과 `Advanced="0"`를
  기록한 `7682` bytes, SHA-256
  `DD6922DA3D499B3F3FED809C8685A9F8EB395E171910A7AFF81610FB442E0208`. connection tuple의 의미
  변경으로 간주하지 않지만 이후 Network 무변경 비교는 HEAD가 아니라 이 observed state를 기준으로 함

13:45:32~13:48:12에는 TCPMotionInterface의 `ClassSvr`, `ConnectedClients`, `ControlCommands`,
LMCDiagnosticsService의 `AxisOwnership` 및 GL slot symbol을 대상으로 `Find in Implementation` 검색이
성공했다. 13:46:10~13:47:33에는 LMCControlCommandService, LMCDiagnosticsService,
LMCEcatInputLatch, LMCSdoExecutor, TCPMotionInterface implementation editor direct-open이 성공했고 이
구간의 신규 `CInvalidArgException`은 `0`이다. 이후 PID `12836`으로 canonical 프로젝트를 다시 열었고
source/generated/Network hash는 위 baseline에서 변하지 않았다.

function 행에는 `Find in Implementation` 메뉴가 없다는 실제 UI 결과에 따라 smoke 계약을 바로잡았다.
function은 `Edit Method` 또는 `Enter`로 exact implementation header를 직접 열고, Object Network
Client/Server는 별도로 `Find in Implementation`을 실행한다. 로그는 class 이름만 남기므로
`RollbackAxisOwnership`과 `ValidateAxisOwnershipRollbackPreemptBank` 두 exact header direct-open은
화면 확인 전까지 미완료다.

## 24. 2026-08-07 PublishAxisOwnership split transition guard

Section 8.5의 두 private function IDE declaration 작업과 post-IDE capture, external body split 적용을
완료했다. declaration은 `//Tables:` 직전, qualified implementation은 source EOF에 모두
`HandleAxisOwnershipPublishHomeReceipt` -> `PrepareAxisOwnershipPublishDecision` 순서다.
`GLOBAL`/`VIRTUAL GLOBAL`과 Network 연결은 없다.

실제 post-IDE/pre-split source는 CRLF `609947` bytes / SHA-256
`C636265238F44D73FDC483309BFB1FF48384EFCD7AF44EE487071CB467281AE5`다. canonical LF는 `593113`
bytes / SHA-256
`F923D5F5A2649B33911072537BFF4B9CB597FAB1C3C8E1D956C8AB5F3C80B2DC`이며 두 helper가 qualified
empty stub인 중간상태였다. capture는
`test/Reports_Lasal/C78_20260807_publish_split_rebaseline/post_ide_pre_split_manifest.json`과 원본 CRLF
snapshot에 보존했다.

external split 적용과 final C78 Rebuild 뒤 current canonical source는 LF `594938` bytes / SHA-256
`8715896406D3B99185C40FBE9C2F0E29170C2D57E1E58792515172EBDDC81E65`로 byte-exact 유지됐다. expected
all-CRLF projection `611837` bytes / SHA-256
`B6A3D9368AA5A81ADD58B002A8504607443ACDAA6AD176E8193FFEBEC9552636`은 actual post-build source가
아닌 projection 진단값이다. terminal EOL 제외 current method는 다음과 같다.

- adapter: `26265` bytes /
  `355A0EA77E13D0CA612BDBD9FA0A55FCA5233B33D3C4DEAC91F5BAEED2B108BE`
- Home: `15035` bytes /
  `EF68864255B888F8E579AE066BB65C1313349B8BE44E0FCEB402FE2DF4DCC849`
- Decision: `24708` bytes /
  `75804F7C0681D51416E75C55D54038162E71768EAFF00C4057F8200D138FC377`

current inventory는 `98/95/3`이다. post-build generated/project/Network evidence는 다음과 같다.

- `Classes.lcb`: `8434505` bytes /
  `CA5CE9AB4B6AFB498D55CF6E5D3460A2C35D54FF8E4FE9C9D3B59636C3603F78`; Split helper record
  `1/1`, exact private ABI, ordered input `7/10`, 각 output `Result : DINT`
- project `.lcb`: `634514` bytes /
  `438DE310CA23C672B52F57483159520887890C17A76B2AE288B7707F4549A919`
- Network available/union `23/23`, pre-build 대비 drift `0`, inventory
  `B80867C9A0E1EF8CBB380F118B92E4E0B54B9705AA676E955A6C1CCB7A74C759`

final C78/ARM Rebuild는 16:31:49~16:32:12에 `0 errors / 61 warnings`, `Compiler Done` 2회,
`Linker Done`, command succeeded로 끝났고 경과시간은 `23.5 s`다. 그보다 앞선 project-load `E0015`와
첫 persistence write 실패는 final build window와 분리한 이전 시도 이력이다. final 성공은 이 과거
실패를 삭제하지 않으며, 과거 실패도 최종 성공 rebuild의 compiler error로 합산하지 않는다. 관련
`Lasal2.log` 전체의 `CInvalidArgException`은 `0`건이다.

class-level `InputLatch`와 `LMCAxis1`의 `Find in Implementation`은 성공했다. 첨부 출력은 `29` hits,
`1` matched file / `3` searched files이고 result presentation이 큰 것은 검색 실패가 아니다. 이를 changed-
class implementation smoke로 승인하지만 새 Home/Decision helper를 직접 검색했다고 주장하지 않는다.

Publish focused static contract와 TW19 negative `37/37`, waiver 없는 pre-build full SourceOnly은 PASS했다.
post-build full `Verify-LasalContract.ps1 -ExpectedSdoWriteAxis 1`도 `236.9`초에 generated Split exact private
ABI `1/1`로 PASS했다. 따라서 source/static/C78/link/generated/changed-class smoke gate는 닫혔다. body
split만 reverse하면 generated declaration/empty stub을 유지한 F923/C636 post-IDE PRE를 복원하고,
generated declaration/stub까지 별도로 제거해야만 7EAB/A51E를 복원한다. A51E를 대상으로 계산했던
Home `15027`, Decision `24697` bytes와 A293/C4B whole-source 값은 actual capture 전 superseded planning
simulation이다. PLC download, reconnect와 실축 runtime proof는 아직 남아 있다.

# Generic SDO Write 상세 구현 설계 — 2026-09-01

- 기준 branch: `dev`
- 설계 기준: `dev@08bd07e43f274e6c150c7348c51dc084070733a0`
- tracking: issue #46
- 우선순위: **P0 / first implementation tranche**
- SetOperationMode: **IMPLEMENTATION COMPLETE / Active**, raw `0x6060` owner는 계속 SetOperationMode 전용
- Generic SDO Write source/runtime 기반: 이미 존재
- 이번 설계 목표: **UI[24] 단일 qualification 구조를 generic axis1..4 / 1·2·4-byte Write 완료 구조로 전환하고, 실기 qualification과 no-replay recovery를 닫는다.**
- production posture: **NO-GO until this document's completion gate closes**

관련 문서:

- `REMAINING_IMPLEMENTATION_DESIGN_20260901.md`
- `SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md` — historical redesign evidence
- `../../architecture/LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`
- `../../architecture/LMC_ETHERCAT_PI_BULK_RECORDER_IMPLEMENTATION_DESIGN_2026-07-20.md`
- `../API_MANUAL.md`

---

# 1. 설계 결론

Generic SDO Write는 새 SDO executor를 만드는 작업이 아니다. current `dev`에는 이미 다음이 구현되어 있다.

1. `LMCSdoExecutor`의 Manual Server / programmatic dual-entry single-owner state machine
2. programmatic `TryStartWrite`의 1/2/4-byte executor-owned buffer
3. `0x7E50 SubmitSDO`의 variable payload Write wire
4. physical axis 1..4 generic scalar request policy
5. PLC execution 직전 DS402 safe-state 재검사
6. operation ticket / status / timeout / orphan drain
7. WPF exact immutable two-click confirmation
8. pre-dispatch durable mutation journal
9. accepted/uncertain submission evidence preservation
10. successful Write 뒤 exact guarded Readback와 byte-for-byte verification

남은 핵심 문제는 **authorization/qualification 계층이 아직 과거 Axis1 UI[24] 단일 preset에 결박돼 있다는 점**이다.

따라서 이번 구현의 핵심 결정은 다음과 같다.

> **current-image transport qualification과 individual generic Write admission을 분리한다.**

- UI[24] same-value test는 current PLC image / executor / ticket / readback path를 검증하는 **canary**로만 사용한다.
- canary가 PASS했다고 임의 object를 자동 승인하지 않는다.
- 실제 generic Write는 매 요청마다 semantic blocklist, scalar shape, fresh identity, axis state, exact confirmation, durable journal과 exact readback을 독립적으로 통과해야 한다.
- `GetApprovedSdoWriteTargets()`의 항목은 앞으로 **preset metadata**이지 generic address allowlist가 아니다.

이 분리가 끝나야 source에 이미 존재하는 generic 1/2/4-byte policy를 WPF/API 사용 계약과 일치시킬 수 있다.

---

# 2. 변경하지 않는 frozen safety contract

## 2.1 semantic/dedicated-owner permanent blocklist

다음 object는 Generic SDO Write에서 영구 차단한다.

```text
0x6040  Controlword
0x6060  Modes of operation
0x607A  Target position
0x60FF  Target velocity
0x6071  Target torque
0x3204  dedicated semantic owner
0x20FC  dedicated semantic owner
```

특히 `0x6060`은 완료된 SetOperationMode의 one-shot/outcome/no-replay lifecycle을 우회할 수 없으며 Generic SDO blocklist에서 제거하지 않는다.

## 2.2 generic scalar shape

v1 Generic Write는 physical axis 1..4와 canonical scalar만 지원한다.

| ValueType family | DataLength | wire bytes |
|---|---:|---:|
| Bool / Int8 / UInt8 / BitField8 | 1 | 1 |
| Int16 / UInt16 / BitField16 | 2 | 2 |
| Int32 / UInt32 / Real32 / BitField32 | 4 | 4 |

추가 규칙:

- `SlaveReference = 1..4`
- `ObjectIndex != 0`
- Bool raw value는 `0` 또는 `1`
- request data length와 actual byte array length exact
- timeout `1..60000`
- string/domain/64-bit/array/complete-access Write는 v1 범위 밖

## 2.3 ordinary Write safe state

Write 직전 PC와 PLC가 모두 다음을 확인한다.

```text
Standstill = TRUE
DS402 Fault = FALSE
DS402 OperationEnabled = FALSE
```

PLC에서 허용하는 DS402 base state는 current contract 그대로다.

```text
0x40
0x21
0x23
```

`0x27` Operation Enabled 및 fault/unknown/incoherent snapshot은 mutation 0회로 거부한다.

PowerOff 자체는 ordinary generic Write의 필수조건이 아니다. qualification canary가 더 보수적으로 PowerOff를 요구할 수 있으나 그것을 generic runtime contract로 확대하지 않는다.

## 2.4 no automatic replay

다음 boundary 이후 original Write automatic replay는 항상 0회다.

```text
Durable ArmBeforeDispatch
-> SubmitSdo wire attempt begins
```

응답을 못 받았거나 ticket을 잃었거나 timeout/disconnect가 발생했다는 이유로 동일 Write를 자동 재전송하지 않는다.

## 2.5 automatic restore 금지

qualification에서 값을 변경하더라도 original value를 자동 restore하지 않는다.

복원이 필요하면 **별도의 새 SDO Write**로 취급한다.

```text
새 baseline
-> 새 confirmation
-> 새 journal
-> 새 one-shot Write
-> 새 exact readback
```

---

# 3. current source truth

## 3.1 `LMCSdoExecutor`

current state machine:

```text
IDLE
 -> ARMING
 -> RUNNING
 -> RESULT_READY
 -> RELEASING
 -> IDLE

exceptional:
RUNNING -> ORPHANED
invariant fault -> QUARANTINED
```

request source:

```text
NONE
MANUAL_SERVER
PROGRAMMATIC
```

`ParaReadWrite::Write`와 `TryStartWrite`는 같은 `AdapterState`를 atomic CAS로 소유한다. programmatic Write는 caller buffer를 직접 vendor request에 넘기지 않고 executor 내부 `WriteBuffer[0..3]`에 복사한 후 callback/drain이 끝날 때까지 보존한다.

이번 작업에서 별도 executor를 추가하거나 per-axis executor를 중복 생성하지 않는다.

## 3.2 `LMCDiagnosticsService`

current `GetSdoWritePolicyDetail`은 이미 generic scalar policy다.

- permanent blocklist
- axis 1..4
- nonzero object
- canonical 1/2/4-byte type/length
- Bool canonical value
- UI[24]에만 별도 conservative range
- execution 직전 current 304-byte snapshot과 DS402 safe state 확인

`ProcessOperations`는 `OperationKind=SDOWrite(3)`일 때 policy를 다시 검사한 뒤 selected `SdoAxis1..4.TryStartWrite`를 한 번 dispatch한다.

## 3.3 WPF / SDK

WPF ordinary `ButtonSubmitSdo` path도 이미 다음 경계를 가진다.

```text
editor request validation
-> fresh D5 capability
-> WPF safe-axis check
-> exact immutable first-click confirmation (zero Write)
-> second click
-> safe-axis recheck
-> durable external submission guard/journal
-> SubmitSdoAsync once
-> accepted ticket preservation
-> status polling
-> exact guarded Readback
```

SDK `LMCSdoWriteVerificationContext`는 generic canonical 1/2/4-byte Write를 이미 지원하며 Write ticket/session/BootId/MapRevision과 exact target tuple을 보존해 Readback result bytes를 비교한다.

## 3.4 current mismatch

다음 요소만 아직 old UI[24]-only activation 시대의 의미가 남아 있다.

- `LMCSdoWriteTarget`가 32-bit preset 형태 중심
- `GetApprovedSdoWriteTargets()`가 Axis1 UI[24] 한 건을 반환
- `EvaluateSdoWritePolicy()`의 `NoApprovedTarget` blocker
- WPF same-value activation runner가 `approvedTargets.Count == 1`을 요구
- `SdoWriteActivationQualificationProof`가 specific UI[24] target tuple에 bind
- UI/문서 일부가 "same-value qualification PASS 후 approved target manual Write"를 generic authorization처럼 표현
- PLC/SDK의 old `...UI24_AXISn_ENABLED` flags가 capability/preset 의미와 generic runtime 의미를 혼합할 여지가 있음

이번 상세설계는 이 legacy coupling을 제거한다.

---

# 4. frozen `0x7E50` wire contract

신규 command ID를 만들지 않는다.

`SubmitSDO = 0x7E50`

common header를 포함한 diagnostics payload contract에서 SDO request header는 32 bytes이고 Write일 때 data bytes가 뒤에 붙는다.

| Offset | Type | Field |
|---:|---|---|
| P8 | U32 | ExpectedMapRevision |
| P12 | U16 | SlaveReference |
| P14 | U16 | OperationFlags, Write=`1` |
| P16 | U16 | ObjectIndex |
| P18 | U8 | SubIndex |
| P19 | U8 | ValueType |
| P20 | U32 | TimeoutCycles |
| P24 | U16 | DataLength |
| P26 | U16 | reserved/canonical zero |
| P28 | U32 | DiagnosticsBootId |
| P32.. | bytes | exact WriteData |

따라서 v1 Write request payload length는 정확히 다음이다.

```text
1-byte Write = 33 bytes
2-byte Write = 34 bytes
4-byte Write = 36 bytes
```

Submit success:

- exact 32-byte operation-ticket payload
- `OperationKind = SDOWrite(3)`
- initial state `Queued`
- nonzero TicketId / BootId

terminal status:

- exact status contract 유지
- successful Write는 `ResultLength=0`
- Write terminal status 자체는 target value verification이 아니다.

즉 `Completed + Success` 뒤 exact Readback까지 끝나야 application-level Write VERIFIED다.

---

# 5. SWR-01 — preset authorization과 generic policy 분리

## 5.1 `LMCSdoWriteTarget`의 새 의미

`LMCSdoWriteTarget` / `GetApprovedSdoWriteTargets()`는 제거하지 않는다. 기존 API와 WPF preset 사용성을 보존한다.

하지만 문서/코드 의미를 다음으로 고정한다.

> **KnownWritePreset / engineering metadata. Generic address authorization 아님.**

UI[24]는 다음 목적으로만 남긴다.

- known qualification canary
- display name
- conservative range
- 편리한 WPF preset loading

preset list가 비어 있어도 global generic Write policy가 활성이고 개별 request가 generic policy를 통과하면 SDK policy evaluation이 실패해서는 안 된다.

## 5.2 `EvaluateSdoWritePolicy` 정리

현재 `NoApprovedTarget`은 generic admission blocker로 사용하지 않도록 변경한다.

권장 compatibility 처리:

- public enum member `NoApprovedTarget`은 binary/source compatibility를 위해 남긴다.
- generic evaluation에서는 더 이상 이 bit를 set하지 않는다.
- 필요하면 새 blocker `WritePolicyDisabled`를 추가해 global policy OFF를 명시한다.
- `ApprovedTargets` property는 preset 목록을 그대로 반환한다.

즉 다음은 허용한다.

```text
CanAttemptSubmission = true
ApprovedTargets.Count = 0
```

이 조합은 "generic Write 가능, known UI preset 없음"을 의미한다.

## 5.3 UI24 per-axis flags 정리

다음 old flags는 generic object authorization으로 사용하지 않는다.

```text
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED
LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED
```

전환 단계에서는 이름을 즉시 삭제하지 않아도 되지만 의미를 **UI24 preset/canary exposure only**로 좁힌다.

`SDOWrite` capability bit9는 generic runtime support를 뜻해야 하므로 "UI24 preset이 최소 한 개 켜짐"을 generic feature support의 필수조건으로 삼지 않는다.

최종 source verifier는 다음 불변식을 강제한다.

```text
bit9 Generic SDOWrite support
    != UI24 preset count
    != UI24 axis flag count
```

bit9가 ON이면 generic PLC policy/SDK policy/1..4 executor route가 존재해야 한다. 특정 physical axis가 offline이면 request가 runtime failure가 될 수 있지만 capability 의미 자체를 UI24 주소에 묶지 않는다.

---

# 6. SWR-02 — image/session transport qualification proof

## 6.1 기존 target-bound proof를 transport proof로 변경

현재 `SdoWriteActivationQualificationProof`는 UI24 target tuple에 bind돼 있다. 이를 다음 둘 중 하나로 리팩터한다.

권장안:

```text
SdoWriteTransportQualificationProof
```

또는 기존 class 이름을 유지하되 내부 의미를 image/session proof로 변경한다.

proof가 보존할 identity:

- exact `LMCConnection` owner
- connection SessionGeneration
- DiagnosticsBuild
- DiagnosticsBootId
- MapRevision
- `SDORead` capability
- `SDOWrite` capability
- `SDOReadGeneralInline` capability
- BaseCycleTimeUs
- MaxSdoDataBytes
- canary run evidence identity
  - canary Slave/Object/SubIndex/Type/Length
  - baseline Read ticket id
  - pre-write guard Read ticket id
  - Write ticket id
  - readback ticket id

canary target tuple은 **증거 metadata**이며 이후 generic request target authorization에 사용하지 않는다.

## 6.2 proof revoke 조건

다음 중 하나면 즉시 폐기한다.

- disconnect
- reconnect / SessionGeneration 변경
- DiagnosticsBuild 변경
- BootId 변경
- MapRevision 변경
- SDORead/SDOWrite/GeneralInline capability 상실
- MaxSdoDataBytes < 4
- current connection owner 불일치
- mutation uncertainty/quarantine가 새로 발생

proof는 프로세스 restart를 넘어 durable authorization으로 저장하지 않는다.

## 6.3 canary workflow

현재 UI24 same-value runner는 첫 transport canary로 유지한다.

```text
canary baseline Read
-> safe-axis proof
-> operator qualification confirmations
-> exact pre-write guard Read
-> baseline == pre-write guard
-> journal arm
-> byte-identical Write once
-> Completed+Success
-> exact Readback
-> readback == written bytes
-> current image/session transport proof capture
```

sentinel value를 자동으로 쓰지 않는다. canary가 same-value이므로 "실제 값 변화" 증거가 아니라 **Write transport + executor + ticket + readback lifecycle** 증거다.

추후 프로젝트에서 더 적합한 safe canary object가 승인되면 UI24를 교체할 수 있지만, 본 설계에서 새로운 object address를 추측하지 않는다.

---

# 7. SWR-03 — ordinary Generic Write 최종 workflow

ordinary generic Write도 same-value qualification과 동일한 수준의 stale/race 방어를 갖도록 baseline/pre-write guard를 추가한다.

## 7.1 1단계 — request draft / local validation

WPF editor에서 다음 immutable request를 만든다.

```text
SlaveReference
ObjectIndex
SubIndex
ValueType
DataLength
WriteData exact bytes
TimeoutCycles
```

local zero-wire 검증:

- generic scalar shape
- semantic blocklist
- Bool canonical
- known preset special range when applicable
- current connected session
- current transport qualification proof

실패 시 RPC 0회, journal arm 0회다.

## 7.2 2단계 — exact baseline Read

**Write confirmation을 arm하기 전에** exact same target을 Read한다.

```text
same slave
same object
same subindex
same ValueType
same DataLength
```

baseline은 target의 현재 raw bytes를 기록하는 evidence다.

baseline Read 실패/abort/timeout이면 Write를 시작하지 않는다.

## 7.3 3단계 — first-click confirmation

화면에 최소 다음을 표시한다.

```text
Axis/Slave
Object:SubIndex
ValueType / DataLength
Baseline raw bytes
Requested raw bytes
Timeout
Build/BootId/MapRevision
```

첫 click은 exact immutable confirmation snapshot만 arm하고 **Write 0회**다.

editor field, connection, capability identity, selected axis 또는 requested bytes가 바뀌면 confirmation을 즉시 무효화한다.

## 7.4 4단계 — second-click final preflight

두 번째 click에서 다음을 다시 수행한다.

1. current connection/session 확인
2. fresh capabilities
3. transport proof가 동일 image/session인지 확인
4. final WPF safe-axis proof
   - Standstill
   - Fault false
   - OperationEnabled false
   - 3 stable position samples
5. exact target **pre-write guard Read**
6. `preWriteGuardBytes == baselineBytes` 확인

baseline과 pre-write guard가 다르면 다른 actor가 target을 변경했을 가능성이 있으므로 confirmation을 폐기하고 **Write 0회**로 종료한다.

이 비교가 외부 actor와의 완전한 atomic CAS를 보장한다고 주장하지 않는다. 실제 single writer는 `LMCSdoExecutor`의 Manual/programmatic owner와 운영 절차로 보장하며 baseline guard는 stale operator intent 방어다.

## 7.5 5단계 — durable arm

모든 final preflight 후, wire 직전에 journal을 durable arm한다.

권장 metadata:

```text
Endpoint IP/port
DiagnosticsBuild
DiagnosticsBootId
MapRevision
Slave/Object/SubIndex/ValueType/DataLength/Timeout
BaselineData
PreWriteGuardData
ExpectedWriteData
TransportProof image/session identity summary
```

journal arm 실패 시 Write 0회다.

## 7.6 6단계 — one-shot submit

journal arm 성공 후 다음 호출을 정확히 한 번 수행한다.

```text
Diagnostics.SubmitSdoAsync(writeRequest)
```

submission attempt tracker가 wire boundary 이후 `OutcomeUncertain`을 표현할 수 있어야 한다.

accepted ticket이 반환되면 validation보다 먼저 durable/external guard와 WPF retained-ticket state에 채택한다.

## 7.7 7단계 — ticket polling

Write가 accepted되면 original request를 다시 제출하지 않는다.

허용되는 동작:

```text
0x7E03 GetOperationStatus
queued cancel when still definitively queued
Stop / PowerOff safety action
recovery inspection
```

RUNNING 이후 cancel 요청을 physical undo로 해석하지 않는다.

## 7.8 8단계 — successful terminal -> exact readback

`Completed + Success`를 받으면 journal state를 `TerminalSuccessPendingReadback`으로 전환한다.

그 다음 SDK `LMCSdoWriteVerificationContext`로 exact Readback을 수행한다.

```text
Write ticket/session/BootId/MapRevision
+ same target tuple
+ fresh post-write capability observation
+ SDORead ticket
+ successful terminal
+ result byte-for-byte == ExpectedWriteData
```

모두 일치한 경우에만:

```text
SDO WRITE VERIFIED
journal -> Resolved
mutation interlock clear
```

---

# 8. failure / recovery classification

## 8.1 NOT_SUBMITTED

다음은 mutation 0회가 증명되는 failure다.

- local request validation failure
- semantic blocklist
- transport proof 없음/stale
- baseline Read failure
- first-click confirmation only
- final safety preflight failure
- pre-write guard mismatch
- journal arm failure

이 경우 original Write replay 개념이 없다.

## 8.2 explicit Submit rejection

SDK failure context가 **wire attempt는 있었지만 PLC가 ticket을 발급하지 않고 명시적으로 reject했다**는 것을 증명하면 rejection evidence를 남기고 같은 자동 attempt를 하지 않는다.

PLC rejection이 actual SDO dispatch 이전임이 producer contract로 증명되는 경우에만 "target mutation 없음"으로 분류한다.

## 8.3 submission outcome uncertain

TCP loss/parse failure/response loss 등으로 ticket 여부가 불명확하면:

```text
OutcomeUnverified
```

- original Write replay 0회
- new mutation blocked
- Close blocked according to current journal policy
- same identity에서 read-only recovery만 시도

## 8.4 accepted ticket / nonterminal

```text
AcceptedPendingTerminal
```

status query만 사용한다. reconnect 때문에 Start를 재구성하지 않는다.

## 8.5 failed / timed out / orphaned Write

보수적 규칙:

> executor Write dispatch 가능성이 존재한 이후 Failed/TimedOut/Orphan은 "값이 안 바뀌었다"는 증거가 아니다.

가능하면 exact Readback으로 actual target value를 확인한다. 단, readback 값이 expected와 다르다고 original Write를 다시 실행하거나 자동 restore하지 않는다.

same BootId/MapRevision에서 exact recovery readback이 불가능하거나 executor/quarantine evidence가 불완전하면 durable unresolved 상태를 유지한다.

## 8.6 terminal success + readback mismatch

```text
ReadbackMismatch
```

- success로 축소 금지
- automatic rewrite 0회
- automatic restore 0회
- operator/physical investigation 필요

## 8.7 disconnect/restart

### same process / same session이 유지되는 일시적 readback failure

exact guarded readback retry만 허용한다.

### reconnect, same BootId/MapRevision

persisted exact metadata로 **Write가 아니라 Readback request만** 재구성한다.

### BootId 변경

이전 target state를 current PLC runtime이 자동 증명할 수 없으므로 automatic resolve하지 않는다. independent physical/operator recovery evidence가 필요하다.

---

# 9. durable journal schema 보강

현재 journal은 exact requested Write bytes와 endpoint/build/boot/map를 저장한다. ordinary generic baseline guard를 정식 contract로 만들 때 다음 metadata를 추가한다.

```text
SchemaVersion += 1
BaselineDataLength
BaselineData[1..4]
PreWriteGuardDataLength
PreWriteGuardData[1..4]
ExpectedWriteDataLength
ExpectedWriteData[1..4]
```

rules:

- three lengths는 request DataLength와 exact match
- baseline/prewrite는 equality가 증명된 상태에서만 ArmBeforeDispatch 가능
- old journal schema는 migration으로 새로운 Write authorization을 얻지 않는다.
- old unresolved record는 **recovery-only**로 계속 열 수 있어야 한다.
- journal parser corruption은 fail-closed한다.

binary file format을 이미 versioned text/structured record로 관리한다면 같은 의미를 해당 storage format에 적용한다. 구현 전에 current serializer compatibility test를 먼저 작성한다.

---

# 10. Manual Server / programmatic contention 상세 설계

Generic SDO의 중요한 완료조건은 LASAL Class View manual path와 API programmatic path가 같은 executor를 안전하게 공유하는 것이다.

## 10.1 required invariant

한 physical axis의 `LMCSdoExecutor`에는 어느 순간에도 다음 중 하나만 존재한다.

```text
NONE
MANUAL_SERVER
PROGRAMMATIC
```

둘 다 vendor SDO를 동시에 소유할 수 없다.

## 10.2 test matrix

| Case | 먼저 owner | 두 번째 요청 | 기대 결과 |
|---|---|---|---|
| C01 | manual Read | programmatic Read | BUSY / hidden request 0 |
| C02 | manual Write | programmatic Write | BUSY / second Write 0 |
| C03 | manual Write | programmatic Read | BUSY / race 0 |
| C04 | programmatic Read | manual Write | ClassState BUSY / manual Write 0 |
| C05 | programmatic Write | manual Read | ClassState BUSY / hidden Read 0 |
| C06 | programmatic Write | manual Write | ClassState BUSY / second Write 0 |
| C07 | completion publish | next request before consume/release | BUSY until reusable |
| C08 | orphan/quarantine | new request | blocked until exact drain/recovery |

Manual Class View physical qualification은 `ParaReadWrite=0` Read와 `ParaReadWrite=1` Write를 모두 캡처한다.

manual Write target도 semantic blocklist와 hardware safety 절차를 우회하는 production API로 간주하지 않는다. Class View는 engineering qualification surface이며 production software path는 programmatic policy를 기준으로 한다.

---

# 11. timeout / cancel / orphan 상세 contract

## 11.1 queued timeout

아직 vendor executor가 request를 claim하지 못한 채 overall ticket timeout이면 terminal timed-out으로 종료할 수 있다. 실제 `TryStartWrite`가 호출되지 않았음을 static/runtime evidence로 구분한다.

## 11.2 RUNNING timeout

RUNNING에서 timeout이면 `MarkOrphan(ExpectedToken)` 후 adapter를 즉시 IDLE로 재사용하지 않는다.

late callback drain이 확인될 때까지:

```text
ORPHANED / drain state
new request blocked
```

## 11.3 queued cancel

queued 상태에서만 cancel을 허용한다. cancel과 executor claim race에서 executor가 먼저 RUNNING을 소유했다면 cancel success로 물리 Write 미실행을 주장하지 않는다.

## 11.4 disconnect

RUNNING disconnect는 orphan/drain으로 전환하고 Write payload/ticket identity를 durable PC evidence에 남긴다. reconnect 후 original SubmitSDO를 replay하지 않는다.

---

# 12. target qualification policy

본 설계는 1/2/4-byte physical test object address를 임의 지정하지 않는다.

각 physical test 전 다음 evidence를 먼저 작성한다.

```text
Axis
ObjectIndex/SubIndex
ValueType
Width
vendor/project meaning
why non-semantic
why safe in Standstill/FaultFalse/OperationEnabledFalse
initial value
permitted test value/range
persistence behavior
readback semantics
```

다음 종류는 safe test target 후보에서 제외한다.

- controlword/opmode/target position/velocity/torque
- homing parameter with active semantic lifecycle
- encoder maintenance semantic object
- safety/limit configuration
- persistent drive tuning parameter unless explicitly approved
- unknown object whose side effect is not documented

안전한 1-byte 또는 2-byte target이 실제 drive/project evidence로 확인되지 않으면 width matrix를 억지로 닫지 않는다. 그 경우 해당 width는 `BLOCKED_BY_TARGET_EVIDENCE`로 기록한다.

---

# 13. physical qualification 순서

## Phase A — current-image transport canary

Axis1 UI24 same-value canary 또는 별도 승인된 canary를 사용한다.

목표:

- SubmitSDO Write 실제 1회
- ticket accepted
- terminal success
- exact readback
- transport proof capture

값 변경 여부를 증명하는 단계가 아니다.

## Phase B — Axis1 width matrix

승인된 safe target별로:

```text
baseline
-> pre-write guard
-> one Write
-> exact readback
```

순서:

1. 1-byte same-value
2. 2-byte same-value
3. 4-byte same-value
4. 승인된 경우 changed-value 1/2/4-byte
5. 필요 시 별도 explicit restore transaction

각 changed-value와 restore는 각각 별도 mutation evidence다.

## Phase C — contention/failure

- Class View manual vs programmatic BUSY
- programmatic vs manual BUSY
- SDO abort
- timeout before claim
- timeout after claim / orphan drain
- queued cancel race
- TCP response loss
- disconnect
- exact readback mismatch fixture
- reconnect same identity readback-only recovery
- BootId changed unresolved recovery

## Phase D — Axis2..4

Axis1 PASS를 axis2..4로 자동 승격하지 않는다. 같은 width/failure matrix를 각 axis에서 수행한다.

---

# 14. source 변경 대상

## SDK

- `LMC_Library/LMC_API_Delivery/src/LmcDiagnosticsD5Models.cs`
  - preset vs generic policy 의미 분리
  - `NoApprovedTarget` generic blocker 제거/deprecate
  - generic policy regression
- `LmcDiagnosticsD5.cs`
  - current generic submission contract 유지/회귀 추가
- `LmcDiagnosticsSdoWriteVerification.cs`
  - 1/2/4 generic verification regression 확대
- `LmcDiagnosticsD5Protocol.cs`
  - 33/34/36-byte golden tests 고정

## WPF

- `MainWindow.Qualification.SdoWrite.cs`
  - UI24-only authorization을 transport canary로 재분류
- `SdoWriteActivationQualificationProof.cs`
  - target-bound proof -> image/session transport proof
- `MainWindow.Diagnostics.cs`
  - ordinary generic baseline/pre-write guard workflow
- `MainWindow.MutationJournal.cs`
  - baseline/prewrite/request evidence 저장
- `DiagnosticsMutationJournal.cs`
  - schema/version compatibility
- WPF smoke tests
- distribution example mirror

## LASAL

- `LMCDiagnosticsService.st`
  - generic policy는 유지
  - capability bit9와 UI24 preset flag 의미 분리
  - all-axis generic route/static verifier
- `LMCSdoExecutor.st`
  - state machine 자체는 유지
  - contention/orphan regression에 의해 결함이 발견된 경우에만 최소 수정

## docs / verifier

- `DINT_PACKET_MAP.txt`
- `API_MANUAL.md`
- `API_DEVELOPMENT_PROGRESS.md`
- `DistributionSemanticPolicy.ps1`
- SDO source/static verifier

---

# 15. automated regression requirements

## 15.1 SDK tests

필수:

- axis 1..4 request local policy
- each canonical ValueType family
- exact 1/2/4 data length
- Bool 0/1 positive / >1 negative
- permanent blocklist seven objects
- zero object negative
- malformed length negative
- 33/34/36-byte wire golden
- Write ticket `OperationKind=3`
- terminal Write result length 0
- generic readback verify 1/2/4
- readback wrong byte -> Pending/not Verified
- stale session/BootId/MapRevision negative
- preset list empty + generic policy ON => generic policy still attemptable

## 15.2 WPF tests

필수 ordering assertions:

```text
baseline Read
< confirmation arm
< final capability/safety
< pre-write guard Read
< journal arm
< SubmitSdo Write
< terminal success
< exact readback
< journal resolve
```

negative:

- first click -> Write count 0
- editor change -> old confirmation unusable
- capability identity change -> Write count 0
- transport proof stale -> Write count 0
- safety fail -> Write count 0
- baseline/prewrite mismatch -> Write count 0
- journal arm fail -> Write count 0
- submit uncertainty -> retry count 0
- terminal success -> journal still unresolved until readback
- readback mismatch -> unresolved
- reconnect -> Write replay count 0
- semantic blocklist -> journal arm 0, Write 0

## 15.3 LASAL/source tests

- `GetSdoWritePolicyDetail` generic width/type matrix
- blocklist exact seven-object inventory
- safe base states `40/21/23` positive
- `27`, fault, stale snapshot negative
- each axis 1..4 dispatches only matching `SdoAxisN.TryStartWrite`
- one ticket cannot call `TryStartWrite` twice after READY/RUNNING
- `SdoWriteData` clearing does not invalidate executor-owned WriteBuffer
- completion object/subindex/length/IsWrite exact
- timeout/orphan path cannot expose IDLE before late callback drain
- Manual/Programmatic source mutual exclusion
- capability bit9 independent from UI24 preset count

---

# 16. implementation tranche

## SWR-01 — policy decoupling

- preset vs generic semantics 정리
- `NoApprovedTarget` legacy blocker 제거/deprecation
- bit9/UI24 preset coupling 제거
- SDK/LASAL/static tests
- physical Write 없음

완료조건: UI24 preset 없이도 generic policy contract가 source/static에서 일관되며 semantic denylist는 그대로다.

## SWR-02 — transport qualification proof refactor

- target-bound proof를 image/session proof로 변경
- same-value runner는 canary 역할만 수행
- disconnect/Boot/Map/capability revoke test

완료조건: canary target과 실제 generic target이 달라도 proof가 generic policy를 대신하지 않으며 image/session freshness만 증명한다.

## SWR-03 — ordinary generic baseline/pre-write guard

- baseline Read
- immutable confirmation snapshot
- final safety
- pre-write guard Read
- equality gate
- journal schema 보강
- one-shot submit
- exact readback

완료조건: automated ordering/zero-wire negative tests PASS.

## SWR-04 — executor contention / recovery regression

- Manual vs programmatic matrix
- timeout/orphan/cancel/disconnect
- static exact-once verifier

완료조건: hidden second Write / early IDLE / replay 0.

## SWR-05 — Axis1 physical 1/2/4 matrix

- target evidence 승인
- canary
- safe width matrix
- exact readback

완료조건: user/bench evidence로 PASS. source test만으로 physical PASS 선언 금지.

## SWR-06 — Axis1 failure/recovery matrix

- abort/timeout/orphan/disconnect/response-loss/readback mismatch

완료조건: uncertain Write auto replay 0 + durable recovery proof.

## SWR-07 — Axis2..4 expansion

각 축에 SWR-05/06 적용.

완료조건: axis1 PASS를 복사하지 않고 각 axis physical evidence 존재.

## SWR-08 — release/documentation sync

- issue #46 completion evidence
- API manual
- development progress
- DINT map
- WPF distribution mirror
- semantic distribution policy
- Debug/Release/current qualification

완료조건: Generic SDO P0 closed.

---

# 17. completion gate

Generic SDO Write를 `IMPLEMENTATION COMPLETE`로 올리려면 다음이 모두 필요하다.

- [x] preset authorization과 generic policy 분리
- [x] transport proof image/session scoped
- [x] ordinary generic baseline/pre-write guard implementation
- [x] durable journal baseline/prewrite/request evidence
- [x] exact one-shot submit ordering static proof
- [ ] generic 1/2/4 SDK golden + verification tests
- [ ] permanent semantic blocklist regression
- [ ] PLC safe-state gate regression
- [ ] Manual/programmatic contention matrix
- [ ] timeout/orphan/cancel/disconnect no-replay matrix
- [ ] Axis1 approved safe 1/2/4 physical Write/readback
- [ ] Axis2..4 physical matrix
- [ ] same source/artifact/image WPF/SDK/Class View evidence
- [ ] distribution/manual sync

위 항목 중 hardware target evidence가 없는 width/axis는 PASS로 추정하지 않는다.
2026-09-01 current working tree에서 SWR-01~04 software tranche와 C# build/static verifier는 완료했다.
test executable, C78/PLC 및 physical matrix는 사용자가 수행하므로 위 나머지 gate는 미완료로 유지한다.

---

# 18. 구현 시작점

첫 코드 변경은 **SWR-01**로 고정한다.

이 단계에서 physical behavior를 바꾸지 않고 먼저 다음 모순을 제거한다.

```text
current PLC/SDK request policy = generic axis1..4, 1/2/4-byte
current WPF activation proof    = one Axis1 UI24 4-byte preset
```

SWR-01에서 이 둘을 정합시킨 뒤 SWR-02/03으로 WPF transport proof와 ordinary generic execution workflow를 열고, 그 다음 실제 Axis1 hardware qualification으로 이동한다.

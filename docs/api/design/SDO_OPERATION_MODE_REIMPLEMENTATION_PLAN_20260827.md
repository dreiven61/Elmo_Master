# Generic SDO / SetOperationMode 재구현 계획

- 기준일: 2026-08-27
- 기준 branch: `dev@cd89d189a3dd574c1fc1147eba07dff88effc54a`
- 상위 설계: `docs/architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- 목적: 2026-08-27 실기 피드백 3건을 실제 구현 순서와 release gate로 변환한다.
- 중요: 이 문서는 구현 계획이며 현재 기능 완료/activation을 의미하지 않는다.

---

## 0. 해결해야 할 세 문제

### P0-1. SetOperationMode 실제 동작 실패

실기 WPF에서 PP/PV/IP/CSP selector는 표시됐지만 Start가 PLC에서 definitive reject 됐다.
현재 log는 `StartAxisSetOperationMode was rejected`까지 보여주지만 exact `DetailCode`와 loaded PLC support mode를 한눈에 보여주지 않는다.

### P0-2. Generic SDO Write가 `0x2F00:24`로 제한

현재 SDK는 `LMCDiagnosticsWritePolicy.AllowedSdoWrites`와 `RequireSdoWriteAllowed()` 때문에 exact compile-time target만 허용한다.
현재 활성 target은 Axis1 `0x2F00:24 Int32/4` 하나다.

### P0-3. `LMCSdoExecutor` Server Write가 작동하지 않음

현재 `LMCSdoExecutor::ParaReadWrite::Write`, `ParaType::Write`, `ParaString::Write`가 base manual path를 의도적으로 무력화한다.
LASAL Class View에서 `EtherCAT_SDOBase` 방식으로 Server 값을 써도 SDO가 시작되지 않는다.

---

# 구현 전략 요약

구현을 8개 tranche로 나눈다.

| ID | 내용 | 우선도 | production activation |
|---|---|---:|---|
| SDO-R01 | 현재 실패 evidence / regression fixture 고정 | P0 | 없음 |
| SDO-R02 | `LMCSdoExecutor` dual-entry manual Server 복구 | P0 | 별도 C78 gate |
| SDO-R03 | generic SDO Write compile-time target 제한 제거 | P0 | capability gate 유지 |
| SDO-R04 | WPF SDO Editor를 arbitrary target 입력형으로 변경 | P0 | PLC capability 필요 |
| SDO-R05 | generic Write durable no-replay recovery 일반화 | P0 | evidence 후 |
| MODE-R01 | PLC-advertised supported mode mask + rejection diagnostics | P0 | OFF 유지 |
| MODE-R02 | PP/PV/IP/CSP bench runtime 검증 | P0 | bench only |
| REL-R01 | C78/PLC/hardware matrix + 문서/distribution sync | P0 | PASS 후 판단 |

---

## 1. SDO-R01 — 현재 실패 상태를 regression fixture로 고정

### 목적

실기에서 발견한 세 문제를 수정 과정에서 다시 놓치지 않도록 source/static 테스트로 먼저 고정한다.

### 변경 파일 후보

- `LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsSdoWritePolicyEvaluationTests.cs`
- `LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/DiagnosticsD5ContractTests.cs`
- `LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/...`
- 신규 LASAL source verifier script

### 추가할 실패 재현 테스트

1. `LMCSdoRequest.CreateWrite(Slave=1, Object=0x6061 or another test object)`가 request model 단계에서 표현 가능해야 한다.
2. generic Write policy가 `0x2F00:24` exact match만 요구하는 현재 테스트를 제거한다.
3. `LMCSdoExecutor` source verifier가 `ParaReadWrite::Write`에 no-op comment/구현이 남아 있으면 실패하도록 한다.
4. SetOperationMode WPF definitive reject formatter가 `DetailCode`, `ErrorId`, mode, PLC identity를 누락하면 실패하도록 한다.

### 완료 조건

- 현재 source에서는 새 regression test가 의도대로 RED.
- 이후 tranche에서 하나씩 GREEN으로 전환.

---

## 2. SDO-R02 — `LMCSdoExecutor` dual-entry 복구

### 2.1 LASAL IDE declaration 변경

대상:

`Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st`

직접 generated declaration을 임의 편집하지 않는다. LASAL IDE에서 class declaration/network를 갱신하고 generated 결과를 commit한다.

추가 private state 예:

```text
RequestSource      : UDINT
ManualGeneration   : UDINT
ManualLastResult   : DINT
ManualLastAbort    : UDINT
```

필요하면 operator 관찰용 read-only Server도 추가한다.

```text
ActiveSource       // 0 none, 1 manual, 2 programmatic
LastAbortCode
```

정확한 Server type/GUID는 LASAL IDE가 생성한다.

### 2.2 `ParaReadWrite::Write` 구현

현재 no-op 제거.

필수 동작:

```text
A. programmatic owner 없음 확인
B. manual source reserve
C. inherited Para* snapshot
D. StartReadSDO / StartWriteSDO 호출
E. READY -> ClassState BUSY
F. BUSY/ERROR -> reservation release + visible failure
G. callback까지 buffer/source lifetime 유지
```

숫자 Write는 기존 `EtherCAT_SDOBase`와 동일하게:

```text
ObjectIndex = ParaIndex
SubIndex = ParaSubIndex
CompleteAccess = CompleteAccess
pWriteData = &ParaValue
WriteLength = ParaLength
Timeout = Timeout
```

단, `ParaLength`가 numeric buffer capacity를 넘으면 SDO를 시작하지 않고 ERROR 처리한다.

### 2.3 `ParaType` / `ParaString`

현재 no-op override를 폐기한다.

선택안 A — LASAL에서 안전하게 base Server method inheritance가 유지되면 override 제거.

선택안 B — codegenerator/VMT 때문에 override가 필요하면 `EtherCAT_SDOBase` 동작을 동일하게 재현.

결정은 LASAL IDE Rebuild/Link로 검증한다. GitHub source만 보고 base method 호출 syntax를 추정하지 않는다.

### 2.4 callback

`ClassState::NewInst`에서:

```text
ECAT_M_SDO_CALLBACK
  RequestSource=MANUAL
      -> base-compatible manual completion
  RequestSource=PROGRAMMATIC
      -> tokenized strict completion
  RequestSource=NONE
      -> quarantine
```

manual success:

- Read: `ParaLength` 갱신, value/string buffer 유지, `ClassState=READY`, `ErrorCode=0`
- Write: `ClassState=READY`, `ErrorCode=0`

manual failure:

- `ErrorCode=aPara[6]`
- `ClassState=ERROR`

### 2.5 arbitration 시험

반드시 다음 race test를 한다.

```text
Manual active -> TryStartRead = BUSY
Manual active -> TryStartWrite = BUSY
Programmatic active -> ParaReadWrite trigger = no wire + visible BUSY
Manual completion -> programmatic reusable
Programmatic completion -> manual reusable
late callback -> source mismatch quarantine
```

### SDO-R02 완료 gate

- [x] LASAL IDE Rebuild/Link error 0 (2026-08-27 C78/ARM, 101 warnings)
- [x] `Classes.lcb` regenerated
- 4개 executor network direct-open
- manual `0x6061:0` read on Axis1..4 PASS
- safe test object manual write/readback PASS
- programmatic D5 regression PASS

2026-08-27 사용자가 PLC 다운로드와 기본 정상 구동을 확인했다. 이 결과는 PLC smoke PASS이며
위 manual SDO/회귀 항목의 PASS를 의미하지 않는다. post-download `Classes.lcb` physical hash가
strict verifier 기준과 달라 artifact identity review는 계속 필요하다. 상세 evidence는
`evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md`에 기록한다.

---

## 3. SDO-R03 — generic Write policy 일반화

### 변경 핵심

현재 제거 대상:

```text
LMCDiagnosticsWritePolicy.AllowedSdoWrites
SdoWriteUi24Axis1Enabled ...
CreateAllowedSdoWriteTargets()
RequireSdoWriteAllowed() exact-match loop
```

`LMCSdoWriteTarget`은 "preset description"으로는 유지할 수 있지만, request 허용 여부의 유일한 source로 사용하지 않는다.

### 신규 policy 개념

```csharp
public enum LMCSdoWriteAccessClass
{
    GeneralRaw,
    SemanticReserved,
    BlockedByActiveOwner
}
```

실제 이름은 기존 naming과 맞춰 구현 시 결정한다.

정책 함수 개념:

```csharp
EvaluateSdoWriteAccess(
    LMCSdoRequest request,
    current capabilities,
    current ownership,
    bool expertRawRequested)
```

### 일반 허용 조건

- connected current session
- current capability observation
- `SDOWrite` advertised
- `SDORead` + general inline read는 readback 가능한 일반 target recovery에 필요
- valid physical slave 1..4
- object index nonzero
- valid subindex
- 1/2/4-byte canonical data
- current MaxSdoDataBytes 범위 내
- unresolved mutation 없음
- executor/resource owner available

**주소가 0x2F00이 아니라는 이유만으로 거부하지 않는다.**

### reserved semantic objects

초기 분류:

```text
0x6040 Controlword
0x6060 Modes of operation
0x607A Target position
0x60FF Target velocity
0x6071 Target torque
```

일반 사용자는 semantic API를 우선 사용한다.
Expert Raw Write는 별도 explicit option으로 분리하고 active owner가 있으면 무조건 거부한다.

### SDK 테스트

최소:

- arbitrary 1-byte write request allowed
- arbitrary 2-byte write request allowed
- arbitrary 4-byte write request allowed
- invalid zero index reject
- invalid length reject
- current capability missing reject before wire
- active owner reject before wire
- 0x2F00 remains just one preset, not unique gate

---

## 4. SDO-R04 — WPF 실제 SDO Editor 구현

### 현재 문제

현재 `SDK 승인 SDO Write target` combo와 same-value qualification 중심 UI는 transport qualification 용도에는 적합하지만 사용자가 실제 object dictionary를 편집하기에는 부적합하다.

### 신규 화면

`SDO / Write 정책` 탭을 다음처럼 정리한다.

```text
[Operation] Read / Write
[Slave] 1..4
[Object Index] 0x0001..0xFFFF
[SubIndex] 0..255
[Value Type]
[Data Length]
[Timeout]
[Value / Raw Hex]
[Known Preset] optional

[Read]
[Write Once]
[Refresh Ticket]
[Readback]
```

기존 D5 fault/recovery qualification section은 하단 별도 영역으로 유지한다.

### Write confirmation

modal을 매번 띄우는 구조보다 화면 내 explicit arm을 사용한다.

표시 예:

```text
WRITE ONCE
Slave 1 | 0x2005:01 | UInt16 | 2 bytes | 0x0010
```

semantic reserved object를 선택하면 별도 경고가 표시되고 Expert Raw가 아니면 Write 버튼이 비활성화된다.

### 결과 표시

반드시:

```text
TicketId
SubmitCycle
CompletionCycle
Outcome
AbortCode
Exact request bytes
Readback bytes/value
Recovery state
```

---

## 5. SDO-R05 — generic Write durable recovery 일반화

### 현재 문제

현재 WPF durable mutation/recovery와 same-value qualification은 `approvedSdoWriteTargets.Count == 1` 같은 전제를 여러 곳에서 사용한다.

### 변경

journal identity를 target preset reference가 아니라 request 자체에 바인딩한다.

필수 저장:

```text
Endpoint IP/port
DiagnosticsBuild
BootId
MapRevision
SlaveReference
ObjectIndex
SubIndex
ValueType
DataLength
WriteData bytes
Timeout
RequestId/TicketId if acquired
submission phase
terminal proof
readback proof
```

### 상태

```text
ArmedBeforeDispatch
SubmittedOutcomeUnknown
AcceptedTicket
TerminalSuccessReadbackPending
TerminalFailure
ReadbackVerified
Resolved
Quarantined
```

### replay rule

- `ArmedBeforeDispatch`인데 실제 send 경계를 넘지 않았다는 확실한 evidence가 있을 때만 새 operation 가능.
- write dispatch 가능성이 생긴 뒤 original Write 자동 replay 금지.
- reconnect recovery는 exact target Read 또는 ticket status만 수행.

---

## 6. MODE-R01 — SetOperationMode runtime capability / 진단 재구현

### 6.1 capability model

Admin bits 8/9/10은 lifecycle API 존재 여부만 나타낸다.
추가로 PLC가 supported mode mask를 반환해야 한다.

예상 model:

```csharp
public sealed class LMCAxisSetOperationModeCapabilities
{
    public bool ApiAvailable { get; }
    public uint SupportedModeMask { get; }
    public bool Supports(LMCDriveOperationMode mode);
}
```

wire는 기존 capability payload의 reserved field를 안전하게 사용할 수 있는지 검토한 뒤 확정한다. 호환 공간이 없으면 새 read-only command를 설계한다.

**새 command ID는 packet map 검토 전 임의 확정하지 않는다.**

### 6.2 PLC mode policy

bench 1차 mask:

```text
PP(1)  = ON
PV(3)  = ON
IP(7)  = ON
CSP(8) = ON
Homing(6) = OFF
```

production mask는 physical qualification 결과에 따라 별도로 결정한다.

### 6.3 WPF

selector items:

```text
SDK-known modes ∩ PLC SupportedModeMask
```

PLC가 mask를 광고하지 않으면 selector Start는 fail-closed.

### 6.4 rejection detail

`LMCAxisSetOperationModeRejectedException` catch에서 최소 다음을 status/log에 출력한다.

```text
RequestedMode
Axis
CommandStatus
ErrorId
DetailCode number
DetailCode name
RequestId
DiagnosticsBuild
BootId
MapRevision
Admin mask
SupportedModeMask
```

가능하면 rejection 직후 read-only `0x6061` current mode도 읽는다. 이 read가 실패해도 original rejection을 덮어쓰지 않는다.

### 6.5 loaded image mismatch 진단

WPF 상단에 다음 identity를 눈에 띄게 표시한다.

```text
SDK build/version
PLC DiagnosticsBuild
BootId
MapRevision
Admin features
SetOpMode supported modes
```

벤치 문서에 기록한 expected identity와 실제 연결 identity가 다르면 Start를 막을 수 있는 qualification mode를 추가한다.

---

## 7. MODE-R02 — SetOperationMode 실기 검증

### 전제

- exact branch source
- LASAL IDE Rebuild/Link error 0
- generated artifact current
- C78 generated from same source
- PLC loaded with that C78
- WPF connects after reboot/reload and reads fresh BootId

### matrix

축 1부터 시작하고 PASS 후 2..4 확장.

```text
A. CSP -> PP
B. PP -> CSP
C. CSP -> PV
D. PV -> CSP
E. CSP -> IP
F. IP -> CSP
G. CSP -> CSP same-mode
```

각 case evidence:

- current `0x6061`
- requested mode
- Start request exactly once
- `0x6060` write count
- verify `0x6061`
- outcome query
- exact-generation retire

same-mode case:

```text
0x6060 Write count = 0
verify read >= 1
Succeeded
```

### negative matrix

- operation enabled state -> reject before write
- moving/not standstill -> reject before write
- fault -> reject before write
- generic/manual SDO owner active -> BUSY/owner conflict, no write
- response loss after dispatch -> no replay, read-only recovery
- unsupported Homing(6) -> explicit UnsupportedMode

---

## 8. REL-R01 — release / deployment gate

### PC/source gate

- API Debug/Release build
- WPF Debug/Release build
- SDK contract tests
- WPF smoke
- source verifier
- `git diff --check`

### LASAL generated gate

- declaration generated by IDE
- `Classes.lcb` matches source
- Rebuild/Link error 0
- direct-open methods
- network connectivity

### C78 gate

- fresh C78 artifact timestamp/hash recorded
- exact source commit recorded
- no stale generated file

### PLC runtime gate

- fresh BootId
- expected DiagnosticsBuild/MapRevision
- Admin capabilities
- supported mode mask
- manual Server SDO test
- generic D5 arbitrary target test
- SetOperationMode matrix

### production activation

production capability/gate는 위 physical evidence가 모두 PASS한 뒤 별도 PR에서 변경한다.
문서/PC CI PASS만으로 activation flag를 올리지 않는다.

---

## 9. 구현 시 파일별 예상 변경 목록

### LASAL

- `Class/LMCSdoExecutor/LMCSdoExecutor.st`
- `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`
- `Class/LMCControlCommandService/LMCControlCommandService.st` — capability 확장 필요 시
- `Class/TCPMotionInterface/TCPMotionInterface.st` — 신규 capability route 필요 시
- generated `Classes.lcb`

### SDK

- `src/LmcDiagnosticsD5Models.cs`
- `src/LmcDiagnosticsD5.cs`
- `src/LmcDiagnosticsD5Protocol.cs` — wire 변경 필요 시
- SetOperationMode capability model/protocol
- 관련 error/catalog files

### WPF

- `MainWindow.Diagnostics.cs`
- `MainWindow.Qualification.SdoWrite.cs`
- `MainWindow.AxisSetOperationModeRecovery.cs`
- `MainWindow.ReadOnlyApi.cs`
- `UiLocalization.cs`
- mutation/recovery journal files

### tests

- Diagnostics D5 policy/contract tests
- SDO Write recovery tests
- WPF SDO Editor smoke
- SetOperationMode capability/rejection tests
- LASAL source/static verifier

---

## 10. 구현 순서

실제 개발은 다음 순서를 지킨다.

```text
1. SDO-R01 regression fixture
2. SDO-R02 LMCSdoExecutor manual Server restore
3. C78 + manual Server 최소 bench 확인
4. SDO-R03 generic SDK policy
5. SDO-R04 WPF arbitrary target editor
6. SDO-R05 durable generic recovery
7. MODE-R01 supported-mode capability + detailed rejection
8. MODE-R02 PP/PV/IP/CSP bench matrix
9. REL-R01 distribution/docs sync
10. production activation 별도 review
```

이 순서를 택하는 이유는 3번 문제인 `LMCSdoExecutor` transport entry가 해결되지 않은 상태에서 PC UI/API만 계속 확장하면 또다시 "화면에는 기능이 있는데 PLC에서 실제 실행되지 않는" 상태가 생기기 때문이다.

---

## 11. 최종 acceptance checklist

- [ ] `LMCSdoExecutor`의 `ParaReadWrite=0`으로 실제 Read 발동
- [ ] `LMCSdoExecutor`의 `ParaReadWrite=1`으로 실제 Write 발동
- [ ] manual/programmatic arbitration PASS
- [ ] WPF에서 object index/subindex 직접 입력 가능
- [ ] `0x2F00:24` 이외 target Write PASS
- [ ] 1/2/4-byte generic Write PASS
- [ ] exact readback/recovery PASS
- [ ] uncertain Write 자동 replay 0회
- [ ] PLC supported mode mask가 WPF selector를 결정
- [ ] PP/PV/IP/CSP 실제 mode change PASS
- [ ] Homing(6) 일반 selector에서 거부
- [ ] SetOperationMode reject에 exact detail/identity 표시
- [ ] fresh C78/PLC evidence 수집
- [ ] production activation은 별도 승인

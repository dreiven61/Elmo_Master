# Generic SDO / SetOperationMode 재구현 계획

- 기준일: 2026-08-27
- production baseline: `dev@cd89d189a3dd574c1fc1147eba07dff88effc54a`
- audit branch/head: `codex/sdo-mode-redesign-docs-20260827@4909200ba45e9e5d4f87334e92f6190599f471e2`
- 상위 설계: `docs/architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md`
- tracking: issue #46 / PR #47
- production posture: **NO-GO**

이 문서는 2026-08-27 사용자 요구 3건의 구현 상태와 앞으로의 작업 순서를 관리한다.

---

## 0. 사용자 요청 3건 구현 audit

| ID | 사용자 요청 | 현재 상태 | 완료 판정 |
|---|---|---|---|
| P0-1 | SetOperationMode를 CSP 외 PP/PV/IP 등으로 실제 전환 | qualification branch에 PP/PV/IP/CSP request/UI는 존재하지만 이전 bench Start가 definitive reject. redesign branch에 PLC SupportedModeMask 없음 | **부분 구현 / 미완료** |
| P0-2 | Generic SDO Write를 `0x2F00` 외 arbitrary object에도 허용 | arbitrary request construction test만 추가. 실제 SDK `AllowedSdoWrites` exact allowlist는 유지됨 | **미구현** |
| P0-3 | `LMCSdoExecutor`에서 기존 `EtherCAT_SDOBase` Server Read/Write 사용 | dual-entry source와 generated artifact 구현, C78/PLC 기본 smoke까지 진행 | **소스 구현 완료 / bench 미완료** |

**결론: 세 기능 모두 완료된 상태는 아니다.**

---

## 1. tranche status

| ID | 내용 | 상태 | 현재 evidence | 다음 gate |
|---|---|---|---|---|
| SDO-R01 | 실패 regression fixture | **부분 완료** | arbitrary request model test, dual-entry source verifier, SetOpMode rejection evidence field smoke 추가 | policy-level RED/GREEN fixture와 SupportedModeMask fixture 보강 |
| SDO-R02 | `LMCSdoExecutor` dual-entry | **source/C78 부분 완료** | source 구현, verifier PASS, C78 Rebuild/Link 0 errors, generated lcb, PLC download/basic smoke | manual Read/Write + arbitration + D5 regression + artifact identity |
| SDO-R03 | Generic Write policy 일반화 | **미구현** | 없음 | exact allowlist 제거/재분류 + SDK tests |
| SDO-R04 | WPF arbitrary-target SDO Editor | **미구현** | 기존 qualification UI만 존재 | object/subindex/type/length/value 직접 입력 UI |
| SDO-R05 | Generic Write durable recovery | **미구현** | 기존 single approved-target journal 전제 존재 | exact request-bound journal/recovery |
| MODE-R01 | PLC-supported mode capability + rejection diagnostics | **미구현** | qualification branch에 1/3/7/8 request allowlist, rejection archive field 일부 강화 | SupportedModeMask wire/model/PLC/WPF |
| MODE-R02 | PP/PV/IP/CSP physical matrix | **미실행/기존 실패** | 기존 bench definitive reject | 새 same-image C78/PLC에서 Axis1 matrix |
| REL-R01 | release/distribution | **미준비** | branch-local source/C78 evidence 일부 | 모든 기능/physical gate 후 진행 |

---

## 2. SDO-R01 — regression fixture

### 현재 완료

- [x] `LMCSdoRequest`가 arbitrary object request를 표현할 수 있는 fixture 추가
- [x] `LMCSdoExecutor` manual path가 다시 no-op이 되면 실패하는 source verifier 추가
- [x] SetOperationMode definitive rejection evidence에 ErrorId / DetailCode / RequestedMode / Build / BootId / MapRevision field 존재 여부 보강

### 아직 필요

- [ ] `RequireSdoWriteAllowed()`가 `0x2F00:24` exact match만 요구하면 실패하는 policy regression
- [ ] arbitrary 1/2/4-byte policy acceptance fixture
- [ ] WPF가 arbitrary target 입력 surface를 잃으면 실패하는 smoke
- [ ] PLC SupportedModeMask가 없는데 multi-mode Start를 허용하면 실패하는 SetOperationMode fixture
- [ ] supported-mode mismatch / stale capability zero-wire fixture

SDO-R01은 위 fixture가 이후 tranche의 완료조건을 직접 감시할 때 완료로 본다.

---

## 3. SDO-R02 — `LMCSdoExecutor` dual-entry

### 구현 완료 source

현재 `LMCSdoExecutor.st`에 다음이 구현돼 있다.

- [x] `RequestSource : UDINT`
- [x] `NONE / MANUAL_SERVER / PROGRAMMATIC`
- [x] manual `ParaReadWrite::Write` 실제 Read/Write dispatch
- [x] numeric buffer length guard
- [x] `ParaType::Write` base-compatible behavior
- [x] `ParaString::Write` inherited String client forwarding
- [x] programmatic TryStartRead/TryStartWrite source arbitration
- [x] callback manual/programmatic 분기
- [x] manual completion `ClassState` / `ErrorCode` / `ParaLength` publish
- [x] `CopyCompletion`/`MarkOrphan` programmatic identity fence
- [x] `IsReusable` source+adapter double condition
- [x] focused dual-entry verifier

### build / PLC evidence

- [x] LASAL C78/ARM Rebuild/Link: 0 errors, 101 warnings
- [x] `Classes.lcb` regenerated
- [x] project/network generated artifacts observed
- [x] PLC download: 사용자 확인
- [x] PLC 기본 run smoke: 사용자 확인
- [ ] strict same-image generated artifact identity closure

상세: `evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md`

### 남은 SDO-R02 bench qualification

- [ ] Axis1 executor network direct-open
- [ ] Axis2 executor network direct-open
- [ ] Axis3 executor network direct-open
- [ ] Axis4 executor network direct-open
- [ ] Axis1..4 Class View `0x6061:0` manual Read PASS
- [ ] 승인된 safe writable object manual Write PASS
- [ ] exact manual readback PASS
- [ ] Manual active -> TryStartRead/Write BUSY + no-wire
- [ ] Programmatic active -> manual trigger BUSY + no-wire
- [ ] manual completion -> programmatic reuse
- [ ] programmatic completion -> manual reuse
- [ ] late/source mismatch -> quarantine
- [ ] D5 programmatic Read regression
- [ ] D5 programmatic Write regression

위 항목이 닫혀야 사용자 요청 3번을 **완료**로 표시한다.

---

## 4. SDO-R03 — generic Write policy 일반화

### 현재 확인된 blocker

현재 branch에서도 다음 source가 그대로 존재한다.

```text
LMCDiagnosticsWritePolicy.AllowedSdoWrites
RequireSdoWriteAllowed()
CreateAllowedSdoWriteTargets()
AddUi24TargetIfEnabled()
```

현재 effective approved target:

```text
Slave1 / 0x2F00:24 / Int32 / 4 bytes
```

따라서 사용자 요청 2번은 아직 구현되지 않았다.

### 구현 작업

- [ ] `0x2F00:24`를 qualification preset으로 강등
- [ ] generic admission에서 exact target allowlist loop 제거
- [ ] request validity/capability/owner 기반 policy로 교체
- [ ] physical slave 1..4
- [ ] index `0x0001..0xFFFF`
- [ ] subindex 0..255
- [ ] 1/2/4-byte canonical scalar
- [ ] `MaxSdoDataBytes` check
- [ ] current capability observation check
- [ ] unresolved mutation / SDO owner check

### semantic-reserved policy

초기 reserved object:

```text
0x6040 Controlword
0x6060 Modes of operation
0x607A Target position
0x60FF Target velocity
0x6071 Target torque
```

- normal mode: semantic API 우선
- Expert Raw: explicit opt-in 필요
- active semantic owner 존재: raw write zero-wire reject

### tests

- [ ] arbitrary Int8/UInt8 1-byte Write
- [ ] arbitrary Int16/UInt16 2-byte Write
- [ ] arbitrary Int32/UInt32 4-byte Write
- [ ] zero index reject
- [ ] invalid length reject
- [ ] noncanonical data reject
- [ ] capability stale/missing reject before wire
- [ ] active owner reject before wire
- [ ] `0x2F00:24` 외 target policy PASS

---

## 5. SDO-R04 — WPF arbitrary-target editor

현재 WPF에는 여전히 `SDK 승인 SDO Write target` qualification 중심 surface가 남아 있다.

새 UI:

```text
Operation: Read / Write
Slave: 1..4
Object Index: 0x0001..0xFFFF
SubIndex: 0..255
Value Type
Data Length
Timeout
Value / Raw Hex
Known Preset: optional
Expert Raw: optional

Read
Write Once
Refresh Ticket
Exact Readback
```

### 완료 gate

- [x] `0x2F00` combo 선택 없이 arbitrary target 입력 가능
- [x] type/length/value canonical validation
- [x] exact request preview
- [x] semantic reserved warning
- [x] Write Once explicit arm
- [x] ticket/status/abort 표시
- [x] exact readback 표시
- [x] Korean/English localization round-trip
- [x] Debug/Release WPF smoke

2026-08-28 current-dev update: arbitrary 1/2/4-byte scalar draft input, optional preset, exact two-click Write Once, durable exact readback/no-replay 경계에 더해 wire 직전 canonical request preview와 semantic/dedicated-owner zero-wire warning surface를 추가했다. Preview는 capability refresh와 독립된 draft validation이며 실제 Submit admission/policy를 완화하지 않는다.

---

## 6. SDO-R05 — generic Write durable no-replay

현재 recovery code의 single approved-target 전제를 제거한다.

### durable identity

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
RequestId/TicketId
submission phase
terminal proof
readback proof
```

### states

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

### invariants

- [x] wire dispatch 가능성 이후 original Write automatic replay 0회
- [x] reconnect는 ticket status 또는 exact target Read만 수행
- [x] exact identity mismatch -> zero-wire
- [x] terminal success라도 readback 전 mutation block 유지
- [x] unresolved record startup recovery
- [x] tamper/corrupt journal fail-closed

2026-08-28 current-dev R05-A: durable SDO metadata를 `Int8/UInt8/Int16/UInt16/Int32/UInt32` canonical 1/2/4-byte scalar로 일반화했다. restart recovery의 legacy `approved target` 용어/전제는 generic exact-request recoverability policy로 교체했고, `0x6060`은 durable metadata 단계에서도 semantic-reserved zero-wire deny로 고정했다. 기존 BootId/MapRevision exact-read no-replay 경계는 유지한다. R05-B에서는 journal format을 v3로 올려 SDO metadata에 Endpoint IP/port + DiagnosticsBuild를 함께 영속화했다. 새 SDO Write durable arm은 fresh current-session DiagnosticsBuild/BootId/MapRevision과 connected endpoint를 캡처하며, restart exact-read는 Endpoint + DiagnosticsBuild + BootId + MapRevision이 모두 일치할 때만 wire를 허용한다. v1/v2 record는 계속 읽을 수 있지만 full durable identity가 없으므로 automatic exact-read recovery는 zero-wire `NotEligible`이다.

---

## 7. MODE-R01 — SetOperationMode capability/diagnostics

### 현재 상태

별도 qualification branch에서는 SDK request가 PP/PV/IP/CSP를 허용한다. 그러나 이것은 current redesign/dev completion evidence가 아니다.

이전 bench는 실제 Start에서 definitive rejection됐다.

### 신규 capability contract

Admin bits 8/9/10은 lifecycle API availability로 유지한다.

추가로 PLC runtime은 실제 지원 mode를 `SupportedModeMask`로 광고해야 한다.

개념 model:

```csharp
LMCAxisSetOperationModeCapabilities
  ApiAvailable
  SupportedModeMask
  Supports(mode)
```

새 wire field/command는 packet map 검토 후 확정하며 임의 command ID를 사용하지 않는다.

### bench candidate modes

```text
PP(1)
PV(3)
IP(7)
CSP(8)
```

Homing(6)은 일반 selector에서 제외한다.

### WPF selector

```text
SDK-known modes ∩ PLC SupportedModeMask
```

mask가 없거나 stale이면 Start disabled.

### rejection diagnostics

반드시 기록:

```text
RequestedMode
CurrentMode if readable
Axis
CommandStatus
ErrorId
DetailCode numeric/name
RequestId
DiagnosticsBuild
BootId
MapRevision
Admin features
SupportedModeMask
```

현재 branch의 rejection archive smoke는 ErrorId/DetailCode/requested mode/identity 일부를 검증하지만 SupportedModeMask는 아직 없다.

---

## 8. MODE-R02 — physical qualification

전제:

- [ ] exact source commit
- [ ] fresh C78/ARM build
- [ ] generated artifacts same-image identity
- [ ] exact image PLC load
- [ ] fresh BootId / expected Build / MapRevision
- [ ] lifecycle triad + SupportedModeMask read

Axis1 matrix:

- [ ] CSP -> PP
- [ ] PP -> CSP
- [ ] CSP -> PV
- [ ] PV -> CSP
- [ ] CSP -> IP
- [ ] IP -> CSP
- [ ] CSP -> CSP same-mode zero-write

각 case evidence:

- pre `0x6061`
- Start exactly once
- `0x6060` write count/byte
- post `0x6061`
- terminal `0x7D24`
- exact-generation `0x7D25`
- automatic Start replay 0회

Negative matrix:

- [ ] Operation Enabled reject-before-write
- [ ] moving/not-standstill reject-before-write
- [ ] Fault reject-before-write
- [ ] manual/programmatic SDO owner conflict
- [ ] response loss after dispatch -> read-only recovery
- [ ] Homing(6) unsupported

Axis1 PASS 후 Axis2..4로 확대한다.

---

## 9. REL-R01 — release gate

### PC/source

- [ ] API Debug/Release
- [ ] WPF Debug/Release
- [ ] focused SDK/WPF tests
- [ ] dual-entry verifier
- [ ] SetOperationMode verifier
- [ ] `git diff --check`

### generated/C78

- [ ] IDE-generated declaration reviewed
- [ ] `Classes.lcb` exact source match
- [ ] project/network artifact identity
- [ ] Rebuild/Link error 0
- [ ] direct-open/network smoke

### PLC/hardware

- [ ] Generic manual SDO matrix
- [ ] programmatic D5 regression
- [ ] arbitrary Generic Write matrix
- [ ] SetOperationMode matrix
- [ ] same-image identity evidence

### release

- [ ] distribution copy sync
- [ ] API manual/packet map sync
- [ ] progress docs sync
- [ ] issue #46 completion review
- [ ] production capability/activation 별도 PR

---

## 10. current PR #47 qualification 상태

Latest audited head `4909200...` 기준:

- SetOperationMode source invariant: **57 checks PASS**
- SetOperationMode define order: **PASS**
- SetOperationMode WPF Debug smoke: **PASS**
- SetOperationMode WPF Release smoke: **PASS**
- C78 evidence collector self-test: **PASS**
- PR 전체 status: **FAIL / merge-ready 아님**

현재 확인된 focused failure는 기능 regression이 아니라 diff hygiene다.

```text
docs/api/design/evidence/SDO_R02_C78_DOWNLOAD_SMOKE_20260827.md: extra blank line at EOF
docs/architecture/LMC_GENERIC_SDO_AND_OPERATION_MODE_REDESIGN_2026-08-27.md: trailing whitespace
```

본 문서 update에서 해당 두 hygiene 항목을 제거한다. 그 뒤 최신 head에서 CI를 다시 확인한다.

---

## 11. 다음 작업 순서

```text
1. 문서/diff hygiene 수정 + PR #47 focused CI green
2. SDO-R02 manual Server physical qualification 완료
3. SDO-R03 generic SDK Write policy 일반화
4. SDO-R04 WPF arbitrary-target editor
5. SDO-R05 durable generic Write recovery
6. MODE-R01 PLC SupportedModeMask + detailed rejection
7. MODE-R02 Axis1 PP/PV/IP/CSP physical matrix
8. Axis2..4 확대
9. REL-R01 distribution/docs/artifact closure
10. production activation 별도 review
```

---

## 12. 최종 acceptance checklist

### 요청 1 — SetOperationMode

- [ ] PLC SupportedModeMask 구현
- [ ] WPF selector PLC 연동
- [ ] PP/PV/IP/CSP Axis1 actual PASS
- [ ] Axis2..4 PASS
- [ ] same-mode zero-write PASS
- [ ] fault/timeout/disconnect/no-replay PASS

### 요청 2 — Generic SDO Write

- [ ] `0x2F00:24` unique allowlist 제거
- [ ] arbitrary object/subindex Write
- [ ] 1/2/4-byte support
- [ ] WPF arbitrary editor
- [ ] exact readback
- [ ] durable no-replay recovery

### 요청 3 — LMCSdoExecutor Server

- [x] dual-entry source
- [x] C78 Rebuild/Link
- [x] generated artifact regeneration
- [x] PLC download/basic run smoke
- [ ] manual Read physical PASS
- [ ] manual Write/readback physical PASS
- [ ] manual/programmatic arbitration PASS
- [ ] D5 regression PASS
- [ ] same-image artifact closure

**세 요청 모두 체크가 닫히기 전에는 issue #46을 완료 처리하거나 production activation을 열지 않는다.**

# LMC Generic SDO / Operation Mode 재설계

- 기준일: 2026-08-27
- production 기준: `dev@cd89d189a3dd574c1fc1147eba07dff88effc54a`
- 구현 audit branch: `codex/sdo-mode-redesign-docs-20260827@4909200ba45e9e5d4f87334e92f6190599f471e2`
- tracking: issue #46 / PR #47
- 목적: 2026-08-27 실기에서 확인된 세 문제를 구조적으로 수정하고, source 구현과 실제 PLC 기능 qualification을 분리해서 관리한다.
- production 상태: **NO-GO / activation 변경 없음**

이 문서는 다음 과거 결정을 supersede한다.

- `LMC_D5_ETHERCAT_SDO_DERIVED_EXECUTOR_DESIGN_2026-07-22.md`의 manual Server entry 영구 차단 결정
- generic SDO Write를 Axis1 `0x2F00:24` 단일 compile-time allowlist로 운영하는 결정
- PC/WPF가 PLC runtime support와 독립적으로 PP/PV/IP/CSP를 열어 두는 qualification 방식

다음 원칙은 유지한다.

- mutation이 wire dispatch 가능 경계를 넘은 뒤에는 original mutation 자동 replay 금지
- endpoint / DiagnosticsBuild / BootId / MapRevision exact identity fence
- unresolved mutation이 있으면 새 mutation 차단, read-only recovery 우선
- semantic motion owner와 generic/manual SDO가 같은 physical SDO resource를 동시에 소유하지 않음
- source/static, generated artifact, C78, PLC load, hardware/packet, production activation을 별도 evidence grade로 관리

---

## 1. 2026-08-27 구현 audit 결론

사용자가 요청한 세 기능을 현재 repository source와 PR #47 diff를 기준으로 다시 판정한다.

| 요청 | 현재 판정 | 구현된 부분 | 아직 부족한 부분 |
|---|---|---|---|
| 1. PP/PV/IP/CSP SetOperationMode 실제 전환 | **부분 구현 / 완료 아님** | 별도 MODE-11 qualification branch에서 SDK/WPF가 PP(1), PV(3), IP(7), CSP(8)를 표현할 수 있음. 기존 `0x7D23/24/25` lifecycle/no-replay 구조 존재 | redesign branch/dev에 PLC-advertised `SupportedModeMask` 없음. 기존 bench는 definitive reject. 실제 PP/PV/IP/CSP mode-change PASS 없음. MODE-R01/R02 미완료 |
| 2. `0x2F00` 외 arbitrary Generic SDO Write | **미구현** | `LMCSdoRequest`가 arbitrary object request를 표현할 수 있다는 test 추가 | `LMCDiagnosticsWritePolicy.AllowedSdoWrites`와 `RequireSdoWriteAllowed()` exact allowlist가 그대로 존재. 현재 operational Write는 계속 Axis1 `0x2F00:24`에 묶임. WPF arbitrary-target editor도 없음 |
| 3. `LMCSdoExecutor` 기존 `EtherCAT_SDOBase` Server Read/Write | **소스 구현 완료 / 실기 qualification 미완료** | manual/programmatic dual-entry, `RequestSource` arbitration, `ParaReadWrite` Read/Write dispatch, `ParaType`/`ParaString`, callback 분기 구현. C78 Rebuild/Link 0 errors 및 PLC download/basic smoke 기록 | Class View manual Read/Write 실제 PASS, manual/programmatic contention, D5 regression, same-image artifact identity가 아직 미완료 |

따라서 **세 기능 모두 완료된 상태로 처리하면 안 된다.**

현재 가장 앞선 항목은 3번이며, 2번은 구현 착수 전 정책 단계, 1번은 기존 qualification branch 코드가 있으나 새로운 runtime-capability 설계 기준으로는 아직 미완료다.

---

## 2. 확인된 현재 source 사실

### 2.1 `LMCSdoExecutor` dual-entry는 branch source에 구현됨

현재 audit branch의 `LMCSdoExecutor.st`는 다음 source identity를 가진다.

```text
RequestSource
  NONE           = 0
  MANUAL_SERVER  = 1
  PROGRAMMATIC   = 2
```

Manual Server entry는 더 이상 no-op이 아니다.

```text
ParaReadWrite::Write
  -> shared AdapterState reserve
  -> RequestSource=MANUAL_SERVER
  -> ParaIndex / ParaSubIndex / CompleteAccess / ParaType / ParaLength snapshot
  -> StartReadSDO 또는 StartWriteSDO
  -> callback에서 ClassState / ErrorCode / ParaLength / String 결과 publish
  -> RequestSource release
```

Programmatic entry는 기존 tokenized path를 유지하면서 `RequestSource=PROGRAMMATIC`을 사용한다.

```text
TryStartRead
TryStartWrite
CopyCompletion
MarkOrphan
IsReusable
```

두 entry는 같은 executor/slave mailbox를 공유하므로 동시 실행하지 않는다.

### 2.2 Generic SDO Write policy는 아직 단일 target allowlist

현재 audit branch의 `LmcDiagnosticsD5Models.cs`에는 여전히 다음 정책이 존재한다.

```text
AllowedSdoWrites
RequireSdoWriteAllowed()
CreateAllowedSdoWriteTargets()
AddUi24TargetIfEnabled()
```

현재 source-approved target은 다음 하나다.

```text
Slave 1
0x2F00:24
Int32 / 4 bytes
```

`DiagnosticsD5ContractTests`에 arbitrary `0x6061:0` request construction test가 추가됐지만 이는 **request model 표현 가능성**만 검증한다.

다음 두 명제는 다르다.

```text
A. LMCSdoRequest가 arbitrary address를 표현할 수 있다.     -> 현재 true
B. LMCDiagnostics.SubmitSdo가 그 Write를 policy상 허용한다. -> 현재 false
```

SDO-R03에서 B를 열어야 2번 요청이 구현된 것으로 본다.

### 2.3 SetOperationMode multi-mode는 current redesign에 통합되지 않음

별도 `codex/setopmode-mode11-bench-activation`에는 PP/PV/IP/CSP request allowlist가 존재한다.

```text
ProfilePosition(1)
ProfileVelocity(3)
InterpolatedPosition(7)
CyclicSynchronousPosition(8)
```

그러나 이 branch는 Draft / DO NOT MERGE qualification branch이고 이전 bench에서 definitive rejection이 확인됐다.

현재 redesign branch PR #47 변경 목록에는 SetOperationMode SDK/protocol/runtime의 `SupportedModeMask` 구현이 없다. 따라서 qualification branch의 selector 존재를 1번 요청의 완료 근거로 사용하지 않는다.

---

## 3. 목표 architecture

```text
PC / WPF
   |
   +-- Generic SDO Editor -------------------------------+
   |                                                     |
   +-- Semantic SetOperationMode ------------------+      |
                                                  |      |
                                                  v      v
                                      LMCDiagnosticsService
                                                  |
                                    shared SDO ownership/arbitration
                                                  |
                                      +-----------+-----------+
                                      |     LMCSdoExecutor     |
                                      |                       |
LASAL Class View -------------------> | Manual Server Entry   |
ParaIndex/SubIndex/Value/Length       |                       |
                                      | Programmatic Entry    | <--- tokenized service
                                      +-----------+-----------+
                                                  |
                                             toSlave SDO
                                                  |
                                          ECAT_Slave_Base
```

핵심은 **transport를 하나로 유지하되 entry source와 semantic ownership을 분리**하는 것이다.

---

## 4. `LMCSdoExecutor` dual-entry 최종 계약

### 4.1 arbitration

- `RequestSource=NONE`에서만 새 request를 시작한다.
- Manual active -> `TryStartRead/TryStartWrite = BUSY`, no wire.
- Programmatic active -> manual `ParaReadWrite`는 새 SDO를 시작하지 않는다.
- callback 완료 전 source를 NONE으로 되돌리지 않는다.
- late/duplicate callback의 source/metadata가 현재 request와 맞지 않으면 quarantine한다.

### 4.2 Manual Server semantics

숫자 Read/Write:

```text
ParaIndex
ParaSubIndex
CompleteAccess
ParaType=0
ParaValue
ParaLength
Timeout
ParaReadWrite=0/1 trigger
```

String Read/Write:

```text
ParaType=1
ParaString
inherited string buffer
```

Numeric Write는 `ParaLength <= sizeof(ParaValue)`를 반드시 지킨다.

Operator-facing completion:

- success -> `ClassState=READY`, `ErrorCode=0`
- failure -> `ClassState=ERROR`, `ErrorCode=AbortCode`
- Read -> `ParaLength`와 result value/string 갱신

### 4.3 SDO-R02 remaining qualification

Source 구현과 C78 build는 완료됐지만 다음 evidence가 있어야 기능 3을 **완료**로 승격한다.

- [ ] Axis1..4 executor network direct-open
- [ ] Axis1..4 Class View manual `0x6061:0` Read
- [ ] 승인된 safe object manual Write + exact readback
- [ ] Manual active -> programmatic BUSY/no-wire
- [ ] Programmatic active -> manual BUSY/no-wire
- [ ] completion 이후 반대 entry 재사용 가능
- [ ] late callback/source mismatch quarantine
- [ ] programmatic D5 Read/Write regression
- [ ] 동일 시험 image의 source / Classes.lcb / project lcb / Networks.lcb identity 고정

---

## 5. Generic SDO Write 재설계

### 5.1 목표

`0x2F00:24`는 qualification preset으로만 유지한다. 주소 allowlist 자체가 generic Write admission의 유일한 조건이 되면 안 된다.

1차 지원 범위:

- physical slave 1..4
- object index `0x0001..0xFFFF`
- subindex `0..255`
- canonical scalar 1/2/4 byte
- current `MaxSdoDataBytes` 이내
- current capability observation 필수
- exact readback/recovery 가능한 session identity 필수

### 5.2 semantic-reserved target

Generic Raw Write가 semantic API lifecycle을 우회하면 안 되는 대상은 분류한다.

초기 reserved set:

```text
0x6040 Controlword
0x6060 Modes of operation
0x607A Target position
0x60FF Target velocity
0x6071 Target torque
```

기본 UI에서는 semantic API를 우선한다. Expert Raw를 제공하더라도 active semantic owner가 있으면 wire 전 거부한다.

### 5.3 SDO-R03 완료 조건

- [ ] compile-time exact target loop를 generic admission에서 제거
- [ ] arbitrary 1/2/4-byte Write policy PASS
- [ ] invalid index/length/canonical data fail-closed
- [ ] capability stale/missing -> zero-wire reject
- [ ] active owner -> zero-wire reject
- [ ] `0x2F00:24`는 preset일 뿐 unique gate가 아님

### 5.4 WPF SDO Editor

SDO-R04에서는 qualification combo 대신 실제 editor를 제공한다.

```text
Operation: Read / Write
Slave: 1..4
Object Index
SubIndex
Value Type
Data Length
Timeout
Value / Raw Hex
Known Preset(optional)
Expert Raw(optional)

Read
Write Once
Refresh Ticket
Exact Readback
```

결과에는 최소 다음을 표시한다.

```text
TicketId
SubmitCycle / CompletionCycle
Outcome
AbortCode
Exact request bytes
Readback bytes/value
Recovery state
```

---

## 6. Generic Write durable no-replay

SDO-R05는 preset이 아니라 exact request를 journal identity로 사용한다.

필수 durable identity:

```text
Endpoint
DiagnosticsBuild
BootId
MapRevision
SlaveReference
ObjectIndex
SubIndex
ValueType
DataLength
WriteData
Timeout
Request/Ticket identity
submission phase
terminal proof
readback proof
```

write dispatch 가능성이 생긴 이후에는 original Write 자동 replay를 절대 하지 않는다.

Recovery는 다음만 허용한다.

- accepted ticket status query
- exact target readback
- explicit quarantine/retirement workflow

---

## 7. SetOperationMode multi-mode 재설계

### 7.1 lifecycle capability와 mode support를 분리

Admin bits 8/9/10은 API lifecycle 존재 여부다.

```text
AxisSetOperationModeStart
AxisSetOperationModeOutcomeRead
AxisSetOperationModeOutcomeRetire
```

여기에 실제 drive/runtime mode support를 나타내는 `SupportedModeMask`가 별도로 필요하다.

PC selector는 다음 교집합이어야 한다.

```text
SDK-known mode ∩ PLC SupportedModeMask
```

PLC가 mask를 광고하지 않으면 Start는 fail-closed다.

### 7.2 bench 후보

```text
PP(1)
PV(3)
IP(7)
CSP(8)
```

Homing(6)은 HomeDS402/HomeDS402Ex owner가 담당하며 일반 SetOperationMode selector에서 제외한다.

### 7.3 rejection diagnostics

Definitive rejection은 최소 다음을 operator-visible evidence에 남긴다.

```text
RequestedMode
CurrentMode if read succeeded
Axis
CommandStatus
ErrorId
DetailCode number/name
RequestId
DiagnosticsBuild
BootId
MapRevision
Admin feature mask
SupportedModeMask
```

기존 branch에서 ErrorId/DetailCode/request identity archive coverage는 강화됐지만 `SupportedModeMask`와 actual mode policy는 아직 미구현이다.

### 7.4 MODE-R02 physical matrix

Axis1부터 시작한다.

```text
CSP -> PP
PP  -> CSP
CSP -> PV
PV  -> CSP
CSP -> IP
IP  -> CSP
CSP -> CSP same-mode
```

각 case는 다음 evidence를 요구한다.

- pre-read `0x6061`
- exact Start one-shot
- `0x6060` write count/bytes
- post-read `0x6061`
- terminal Outcome
- exact-generation Retire
- no automatic Start replay

이 matrix가 PASS하기 전에는 기능 1을 완료로 표시하지 않는다.

---

## 8. evidence grade와 release boundary

### 현재 완료된 branch-local evidence

- dual-entry source verifier PASS
- C78/ARM Rebuild/Link 0 errors, 101 warnings
- generated artifact 재생성
- PLC download/basic runtime smoke 사용자 확인
- SetOperationMode source invariant 57 checks PASS
- SetOperationMode WPF Debug/Release recovery smoke PASS

### 현재 미완료

- PR #47 full CI green: 최신 head는 docs diff hygiene 때문에 일부 workflow failure
- strict generated artifact identity closure
- manual Server physical Read/Write qualification
- arbitrary Generic Write policy/UI/recovery
- PLC SupportedModeMask
- PP/PV/IP/CSP physical mode-change matrix
- production activation

Production `dev`의 feature gates/capabilities는 이 문서/branch 결과만으로 변경하지 않는다.

---

## 9. 구현 우선순위

```text
P0-A  PR #47 diff hygiene 정리 + focused CI green
P0-B  SDO-R02 manual Server bench/packet qualification 완료
P0-C  SDO-R03 generic Write policy 일반화
P0-D  SDO-R04 arbitrary-target WPF editor
P0-E  SDO-R05 exact-request durable recovery
P0-F  MODE-R01 SupportedModeMask + rejection diagnostics
P0-G  MODE-R02 Axis1 PP/PV/IP/CSP matrix -> Axis2..4
P0-H  REL-R01 distribution/docs/generated artifact sync
P0-I  production activation 별도 review
```

이 순서를 바꾸어 WPF UI부터 먼저 열지 않는다. PLC/runtime/transport capability보다 UI가 앞서면 2026-08-27 bench에서 확인된 "화면에는 기능이 있지만 실제 실행은 reject" 상태가 재발한다.

---

## 10. 최종 Definition of Done

세 사용자 요구를 완료로 판정하려면 다음이 모두 PASS해야 한다.

### 기능 1 — SetOperationMode

- [ ] PLC SupportedModeMask 계약 구현
- [ ] WPF selector가 PLC mask와 연동
- [ ] PP/PV/IP/CSP Axis1 actual mode change PASS
- [ ] same-mode zero-write PASS
- [ ] Axis2..4 확대
- [ ] fault/timeout/disconnect/no-replay negative matrix PASS

### 기능 2 — Generic SDO Write

- [ ] compile-time single-target gate 제거
- [ ] arbitrary object/subindex 1/2/4-byte Write 가능
- [ ] WPF arbitrary-target editor
- [ ] exact readback
- [ ] durable no-replay recovery
- [ ] owner/capability fail-closed negative tests

### 기능 3 — LMCSdoExecutor Server interface

- [x] dual-entry source 구현
- [x] LASAL C78 Rebuild/Link
- [x] PLC download/basic smoke
- [ ] manual Read physical PASS
- [ ] manual Write/readback physical PASS
- [ ] manual/programmatic arbitration PASS
- [ ] D5 programmatic regression PASS
- [ ] same-image generated artifact identity closure

세 기능의 source 및 physical evidence가 닫힌 뒤에만 production activation을 별도 changeset에서 검토한다.

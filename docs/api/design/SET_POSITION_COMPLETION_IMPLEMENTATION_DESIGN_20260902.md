# SetPosition 완료 구현 설계 — 2026-09-02

- 대상: No.58 `MMC_SetPositionCmd`
- 기준 branch: `dev`
- source baseline: `dev@90a86a795773d5f8eca211368aac3f0d64944a32` (`dev : SDO Write Func Complete`)
- current 상태: `Dormant`, fail-closed
- command: `0x7D12 Start`, `0x7D14 ReadOutcome`, `0x7D1A Retire`
- tracker/prerequisite: issue #44
- production posture: **NO-GO**

이 문서는 기존 `SET_POSITION_DESIGN.md`와
`AXIS_SET_POSITION_ASYNC_RT_EXECUTOR_AND_RECOVERY_DESIGN_2026-08-19.md`의 frozen ABI를 유지하면서,
**현재 dev에서 실제로 무엇을 어떤 순서로 구현해야 하는지**를 개발 handoff 형태로 다시 정리한다.

핵심 결론:

> SetPosition의 wire/API lifecycle은 이미 존재한다. 남은 핵심 개발은
> **durable A/B backend -> RT claim-before-native exactly-once executor -> terminal durable commit ->
> owner release -> WPF recovery -> hardware -> paired activation** 순서다.

---

# 1. current source truth

## 1.1 이미 존재하는 것

- SDK Start/Query/Retire surface
- `0x7D12/0x7D14/0x7D1A` wire와 parser
- `LMCSetPositionStore` Begin/Commit/Read/Retire scaffold
- 336 UDINT / 1,344-byte volatile ledger ABI
- `LMCEcatInputLatch` observation-only RT preflight
- `LMCControlCommandService` P1 async lifecycle scaffold
- `TCPMotionInterface` pending / quarantine status separation
- `AxisSetPositionRecoveryJournal.cs` durable recovery record model
- SetPosition outcome retirement/no-replay contract tests
- host-side deployment receipt/readback tooling 일부
  - `tools/LmcSetPositionStoreDeploymentReceipt.ps1`
  - `tools/Start-LmcSetPositionStoreDeployment.ps1`
  - `tools/Verify-LmcSetPositionStoreDeployment.ps1`

## 1.2 current fail-closed gates

다음은 구현 완료 전까지 유지한다.

```text
LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = FALSE
LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED = FALSE
SetPositionMaxJump Axis1..4 = 0
Admin capability bits 3/5/7 = OFF
native SetPosition production path = OFF / no authorized execution
```

이 값을 먼저 켜서 기능을 개발하지 않는다.

## 1.3 외부 prerequisite

issue #44가 다음 두 항목을 요구한다.

1. vendor `CheckSum.CRC32` golden fixture
2. LASAL IDE-generated `_FileSys` class/client/channel ABI

두 항목은 **추측 구현 금지 영역**이다.

- `CRC32` 이름만 보고 IEEE CRC32라고 가정하지 않는다.
- generated `_FileSys` declaration/client를 GitHub에서 손으로 작성하지 않는다.
- exact ABI가 나오기 전 handwritten backend는 interface seam까지만 준비한다.

---

# 2. frozen semantic contract

SetPosition은 단순 변수 대입이 아니다.

application-level 성공 순서:

```text
request admission
-> durable Armed record
-> RT exact request claim
-> native SetPosition logical call exactly once
-> fresh stable observer
-> durable terminal commit
-> durable full readback
-> owner release
-> TCP terminal response
-> exact-generation query/retire lifecycle
```

다음 불변식을 유지한다.

- mutation boundary 이후 original Start replay 0
- same exact request replay는 stored outcome만 반환
- response loss 뒤 `0x7D14` Query / `0x7D1A` Retire만 허용
- terminal durable readback 전 owner release 금지
- terminal durable readback 전 success response 금지
- post-claim uncertainty는 success로 축소 금지
- BootId/session/identity drift에서 automatic replay 금지

---

# 3. implementation architecture

최종 구조는 다음 책임 분리를 따른다.

```text
C# SDK / WPF
   |
   | 0x7D12 Start
   v
TCPMotionInterface
   |
   v
LMCControlCommandService
   |-- admission / ownership
   |-- durable Store Begin
   |-- execution mailbox publish
   v
LMCEcatInputLatch (RT)
   |-- exact tuple claim
   |-- native SetPosition once
   |-- stable observer
   v
LMCControlCommandService
   |-- terminal candidate
   v
LMCSetPositionStore
   |-- durable terminal commit
   |-- reopen/readback proof
   v
owner release -> response
```

Storage와 RT executor는 서로의 역할을 대신하지 않는다.

---

# 4. SP-C0 — current-dev source inventory / regression freeze

실제 코드를 쓰기 전에 current `dev`에서 이미 구현된 callsite와 gap을 확정한다.

필수 inventory:

- `LMCSetPositionStore.st`
  - Begin / Commit / Read / Retire entry
  - current volatile backing field
  - exact 1,344-byte serialization source
- `LMCControlCommandService.st`
  - `0x7D12` admission
  - Store Begin 위치
  - owner reserve/release 위치
  - pending `-13`, uncertainty `-14`, storage failure `-12` 전이
- `LMCEcatInputLatch.st`
  - existing preflight mailbox/result
  - actual native SetPosition call count가 production path에서 0인지 확인
- `TCPMotionInterface.st`
  - Start/Outcome/Retire routing
- WPF
  - `AxisSetPositionRecoveryJournal.cs`
  - 실제 MainWindow dispatch/interlock 연결 여부
  - startup unresolved record 차단 여부
- host tooling
  - receipt chain tools의 실제 implemented 범위
  - image generator가 아직 없는지 확인

### SP-C0 output

새 코드 전에 `SET_POSITION_CURRENT_SOURCE_INVENTORY_20260902.md` 또는 equivalent test evidence에
**existing / missing / blocked**를 기록한다.

목적은 이미 존재하는 recovery/tooling을 중복 구현하지 않는 것이다.

---

# 5. SP-C1 — prerequisite capture

## 5.1 vendor CRC golden fixture

최소 fixture:

- zero-filled deterministic block
- nonzero deterministic byte sequence
- exact 64-byte store header candidate
- exact 1,344-byte ledger body candidate

각 fixture에:

- input bytes/length
- returned UDINT
- controller/toolchain/library identity
- exact LASAL call path

를 기록한다.

Host-side image generator는 fixture가 review되기 전까지 구현 완료로 처리하지 않는다.

## 5.2 IDE-generated `_FileSys` ABI

LASAL IDE/CodeGenerator에서 SetPosition file backend class와 필요한 `_FileSys` client/channel을 만든다.

capture:

- generated declaration
- Client list/order
- method signatures
- project/network delta
- generated `.lcb/.lcn` identity

이 결과를 code review한 뒤 handwritten backend implementation을 붙인다.

### SP-C1 완료조건

- issue #44의 CRC fixture review 완료
- generated `_FileSys` ABI review 완료
- same-tree C78/generated artifact checkpoint 가능 상태

---

# 6. SP-C2 — durable `_FileSys` A/B backend

기존 설계의 fixed A/B format을 사용한다.

```text
C:\LMCSP_A.BIN
C:\LMCSP_B.BIN
file size = 2048 bytes each
header = 64 bytes
ledger body = 1344 bytes
padding = 640 bytes
```

## 6.1 header contract

| Offset | Field | Contract |
|---:|---|---|
| 0 | Magic | `0x50534D4C` (`LMSP`) |
| 4 | Schema | `1` |
| 8 | HeaderBytes | `64` |
| 12 | FileBytes | `2048` |
| 16 | Generation | nonzero U32 |
| 20 | GenerationInverse | `~Generation` |
| 24 | BodyBytes | `1344` |
| 28 | BodyCRC32 | vendor golden semantics |
| 32 | HeaderCRC32 | frozen segmented calculation |
| 36 | CommitMarker | 0 or `0x54494D43` |
| 40 | CommitMarkerInverse | 0 or `0xABB6B2BC` |
| 44..63 | Reserved | zero |

모든 integer는 little-endian file byte order다.

## 6.2 backend class state machine

backend instance는 one-in-flight만 허용한다.

```text
Idle
-> OpenInactive
-> WriteFullImageMarker0
-> WaitWrite
-> Close
-> ReopenRead
-> VerifyFullImage
-> ReopenReadWrite
-> SeekMarkerOffset36
-> WriteMarker8
-> Close
-> ReopenRead
-> VerifyCommittedImage
-> PublishGeneration
-> Idle
```

각 async request ID, file handle, path, 2,048-byte buffers는 class field에 고정한다.
request terminal 전 재사용/변경 금지.

## 6.3 completion 판정

`GetAsyncState(RequestId, OperationResult)`를 request별로 추적한다.

- not-started/in-progress -> pending
- completed -> operation별 exact result 검증
- invalid/deleted/unknown -> uncertainty

최종 성공은 close/reopen/full-read/CRC/marker/generation 검증 뒤에만 반환한다.

## 6.4 startup selection

A/B 모두 읽고 valid state를 판정한다.

- valid/valid -> serial generation newer 선택
- same generation + same image -> valid
- same generation + different image -> corrupt
- one valid + one invalid/missing -> fail-closed
- both missing -> Unprovisioned
- I/O error -> StorageUnavailable

runtime이 임의로 factory empty store를 생성하지 않는다.

## 6.5 factory image generator

CRC fixture가 확정된 뒤에만 `tools/Generate-LmcSetPositionStoreImages.ps1`를 구현한다.

기존 receipt tools와 연결:

```text
FactoryNew
-> FactoryInstallStarted
-> upload A/B while PLC stopped/unloaded
-> exact readback SHA-256
-> VerifiedFactoryEmpty
```

receipt chain 구현을 교체하지 말고 현재 tool을 재사용/확장한다.

### SP-C2 tests

- valid A/B boot select
- same-generation same/different image
- torn full write
- marker write failure
- bad BodyCRC/HeaderCRC
- generation inverse mismatch
- truncated file
- async request failure/invalid id
- full readback mismatch
- one valid/one invalid restart fail-closed
- tombstone commit/readback

---

# 7. SP-C3 — `LMCSetPositionStore` durable adapter

`LMCSetPositionStore.st`의 public lifecycle ABI는 유지한다.

현재 volatile ledger serializer를 canonical 1,344-byte body serializer로 재사용하고 backend를
storage transport로 연결한다.

필수 동작:

### Begin

```text
validate exact key
-> ensure durable backend healthy
-> produce Armed ledger image
-> durable A/B commit + readback
-> only then return Begin success
```

### Terminal Commit

```text
exact active generation/key
-> update terminal outcome
-> durable A/B commit + readback
-> return terminal commit success
```

### Retire

```text
exact terminal generation/key
-> write tombstone / retired outcome
-> durable commit + readback
-> only then reusable
```

RAM mirror update만으로 success를 반환하면 안 된다.

---

# 8. SP-C4 — versioned RT execution mailbox

existing observation-only preflight mailbox와 mutation mailbox를 분리한다.

필요하면 새 class/client declaration은 LASAL IDE에서 생성한다.

mailbox identity 최소값:

- request/intent identity
- source Build/BootId/MapRevision
- axis
- Store record generation
- TargetPosition
- approved MaxJump
- timeout/deadline
- request sequence

## 8.1 claim-before-native

RT는 다음 순서만 허용한다.

```text
Published
-> validate fresh axis/owner/limits/tuple
-> Claimed publish
-> NativeCount 0 -> 1
-> native SetPosition call
-> Observe
-> TerminalCandidate
```

`Claimed` 전에 native call 금지.

## 8.2 exactly-once native call

native callsite는 논리적으로 정확히 한 곳이다.

```text
SetPosition(
  Mode := LMCAXIS_SET_ACTPOS_APPUNIT_DEST,
  Position := TargetPosition)
```

동일 request가 다시 보이면 stored executor state만 반환하고 call 반복 금지.

Static verifier에서 native callsite count와 `NativeCount 0 -> 1` invariant를 고정한다.

## 8.3 stable terminal observer

native accept만으로 성공하지 않는다.

서로 다른 fresh RT cycle에서 최소 3개의 안정 sample을 요구한다.

- owner identity same
- axis state valid
- actual/application position contract satisfied
- no fault/interlock violation
- no newer request sequence

post-claim timeout/owner drift/torn result는 indeterminate/quarantine다.

---

# 9. SP-C5 — terminal-before-release integration

`LMCControlCommandService.st`의 final ordering을 다음으로 고정한다.

```text
RT terminal proof
< durable terminal Store commit
< durable full readback
< owner release
< TCP success response
```

반대 순서는 금지한다.

failure classification:

| 상태 | 의미 |
|---|---|
| `-13` | exact request in progress / poll only |
| `-14` | claim/native/context uncertainty / replay 금지 |
| `-12` | durable storage/commit/readback 불확실 |

`-12/-14`를 받았다고 owner를 자동 release하거나 Start를 재전송하지 않는다.

---

# 10. SP-C6 — WPF durable recovery completion

현재 journal/recovery code를 먼저 inventory한 뒤 missing wiring만 구현한다.

required UI lifecycle:

```text
before 0x7D12 wire
-> journal ArmedBeforeDispatch
-> submit once
-> accepted/running evidence
-> terminal query
-> terminal snapshot + store generation persist
-> exact retire
-> journal Resolved
```

startup unresolved:

```text
RecoveryRequired
-> block new SetPosition mutation
-> reconnect exact endpoint/build/boot/map if possible
-> Query only
-> exact terminal proof
-> Retire only
-> Resolve
```

original `0x7D12` automatic replay는 항상 0.

WPF tests:

- journal arm failure -> wire 0
- process restart unresolved -> new mutation blocked
- response loss -> query only
- terminal observed but retire not done -> unresolved
- exact retire retry idempotent
- BootId changed -> automatic resolve/replay 없음

---

# 11. SP-C7 — source/static/C78 qualification

필수 source tests:

- Store A/B fault matrix
- one-in-flight `_FileSys` request ownership
- exact body/header serialization
- RT mailbox torn/stale/duplicate matrix
- native logical callsite exactly one
- duplicate request native mutation 0
- terminal-before-release ordering
- WPF no-replay recovery
- method-size budget

그 다음 exact current tree에서:

- C78/ARM Rebuild + Link
- 0 errors
- no new `CInvalidArgException`
- generated artifact review
- direct-open
- Network smoke
- full SourceOnly

---

# 12. SP-C8 — hardware/storage qualification

## Storage

- factory A/B install/readback
- cold power cycle
- valid A/B reopen selection
- interrupted full-write
- interrupted marker-write
- disk unavailable/full/read-only conditions
- terminal commit cold-cycle persistence
- retire tombstone cold-cycle persistence

## Axis execution

각 axis 1..4에서 승인된 small correction 범위를 별도로 정한다.

- positive small correction
- negative small correction
- zero/no-op semantic case
- max-jump reject
- fault/interlock reject
- timeout after claim
- response loss
- disconnect
- restart unresolved

packet/evidence에서 native call exactly once를 증명한다.

---

# 13. SP-C9 — paired activation

모든 앞 단계가 PASS한 뒤에만 activation한다.

한 changeset에 포함:

- `LMC_ADMIN_SET_POSITION_STORE_CONFIGURED = TRUE`
- required ordinary ownership gate ON
- axis 1..4 approved `SetPositionMaxJump` values
- Admin capability bits 3/5/7 ON
- WPF availability gating update

partial activation은 금지한다.

activation 이후에도 physical matrix가 없는 axis를 추정 승인하지 않는다.

---

# 14. 구현 순서 요약

```text
SP-C0 current source inventory
-> SP-C1 CRC + IDE _FileSys prerequisites
-> SP-C2 durable A/B backend
-> SP-C3 Store adapter
-> SP-C4 RT exactly-once executor
-> SP-C5 terminal-before-release
-> SP-C6 WPF recovery completion
-> SP-C7 source/C78 qualification
-> SP-C8 storage + axis hardware matrix
-> SP-C9 paired activation
```

병렬 가능:

- SP-C0과 host receipt tooling regression
- CRC fixture 준비와 IDE-generated ABI capture
- WPF recovery test inventory

병렬 금지:

- prerequisite 없이 `_FileSys` ABI hand-authoring
- durable commit 전에 RT activation
- hardware PASS 전에 capability ON

---

# 15. 완료 gate

- [ ] SP-C0 current source inventory frozen
- [ ] issue #44 CRC fixture complete
- [ ] issue #44 IDE-generated `_FileSys` ABI complete
- [ ] A/B durable backend complete
- [ ] Store Begin/Terminal/Retire durable readback complete
- [ ] RT claim-before-native + exactly-once executor complete
- [ ] stable-3 terminal observer complete
- [ ] terminal durable readback-before-release complete
- [ ] WPF startup/query/retire no-replay complete
- [ ] Source/static/method-size/C78/generated artifact PASS
- [ ] cold-cycle storage fault matrix PASS
- [ ] Axis1 approved correction matrix PASS
- [ ] Axis2..4 matrix PASS
- [ ] Store/ownership/max-jump/capability paired activation

위 항목이 닫히기 전 SetPosition은 `Dormant`를 유지한다.

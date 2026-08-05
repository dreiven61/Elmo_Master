# LMC axis ownership overlay, full identity, exact restore IDE handoff

Date: 2026-08-04

> TW-only supersession later on 2026-08-04: items 13-18 and verification step 7 below describe
> this overlay checkpoint before encoder-maintenance activation. TW19/TW20 now use the exact
> fixed-one contract and their two gates are `TRUE`; see
> [TW19/TW20 fixed-one activation](./LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md).
> Ordinary ownership, LMC Home and DS402 Home gates remain separate and are not activated by that change.

## 1. Scope and safety boundary

This handoff closes three dormant source blockers without enabling any feature gate:

1. retain and compare the complete request payload identity, including the 1320-byte `0x20E7` payload;
2. preserve and restore the exact prior Group lease record and identity;
3. preserve one immutable preempted-owner snapshot for safety cleanup.

The following gates remain `FALSE` after this work:

- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED`
- `LMC_ADMIN_AXIS_HOME_ENABLED`
- DS402 Home activation gate
- TW20/TW19 activation gates

Static source, C78 build, and IDE search proof do not authorize PLC download or physical-axis operation.

## 2. LMCControlCommandService declaration changes

Add these five class variables after `OwnershipObserverState`. Names, types, and bounds are exact.

```text
OwnershipLeaseState : ARRAY [0..323] OF DINT
OwnershipPreemptedState : ARRAY [0..323] OF DINT
OwnershipIdentityState : ARRAY [0..431] OF DINT
OwnershipLeaseIdentityState : ARRAY [0..323] OF DINT
OwnershipPreemptedIdentityState : ARRAY [0..431] OF DINT
```

The two record banks each contain nine exact 36-DINT axis records. Identity storage uses a compact
prefix-and-tail layout: the first 64 bytes remain in each ownership record at offsets `16..31`; only
the bytes after that prefix are kept in the identity arrays.

Change the last two identity inputs of `ReserveAxisOwnership` as follows. Keep every other input and
their order unchanged.

```text
pIdentity : ^void
IdentitySize : UDINT
```

Delete the old `pIdentity : ^UDINT` and `IdentityCount : UINT` inputs.

Add this global function. Input names, types, and order are exact.

```text
ValidateAxisOwnershipIdentity
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
  RequiredPhase : UINT
  pIdentity : ^void
  IdentitySize : UDINT
  Result : DINT
```

Add these two global functions for exact safety-cleanup coordination.

```text
CopyAxisOwnershipPreemption
  PreemptedAdmissionToken : UDINT
  PreemptedOwnerGeneration : UDINT
  pDest : ^void
  DestSize : UDINT
  Result : DINT
```

```text
PublishAxisOwnershipPreemptionCleanup
  ExpectedAxisMask : UDINT
  PreemptedAdmissionToken : UDINT
  PreemptedOwnerGeneration : UDINT
  SafetyAdmissionToken : UDINT
  SafetyOwnerGeneration : UDINT
  CleanupKind : UINT
  ReportValue0 : UDINT
  ReportValue1 : UDINT
  ObservationCycle : UDINT
  Result : DINT
```

No Network connection changes are required for these functions.

## 3. LMCEcatInputLatch declaration changes

Add this class variable after `AxisZeroHomeAppliedSequence`:

```text
AxisZeroHomeCancelSequence : UDINT
```

Add this global function. Input/output names, types, and order are exact. Keep the method order
`SubmitAxisZeroHome`, `CancelAxisZeroHome`, `CopyAxisZeroHomeResult`.

```text
CancelAxisZeroHome
  OperationToken : UDINT
  Result : DINT
```

No Network connection change is required. `LMCControlCommandService.InputLatch` already points to
`LMCEcatInputLatch.ClassSvr`.

## 4. Exact identity layout

`OwnershipState` record offset `15` becomes `IdentitySizeBytes`. Offsets `16..31` keep the first
`min(IdentitySizeBytes, 64)` request payload bytes byte-exactly.

`OwnershipIdentityState[0..431]` uses this compact layout:

- `0..313`: 1256-byte suffix store for request bytes after the 64-byte record prefix;
- non-Group 72-byte identities use `(AxisReference - 1) * 8` as the fixed suffix byte offset;
- Group identities use suffix byte offset zero and may occupy all 1256 bytes;
- Axis 1..9 headers use base `314 + (axis - 1) * 12`, through `410..421`;
- the Group header uses `422..431`.

The suffix store is physically shared. For example, the 104-byte `0x7D22` identity uses suffix
bytes `0..39`, which overlap the non-Group Axis 5 slot at bytes `32..39`. Every ordinary Group
command/reservation/transition therefore rejects any live non-Group owner on any axis, including a
disjoint Direct safety owner, before any snapshot, token, or identity mutation. Conversely, ordinary
Direct mutation and Home/TW admission reject a live Group anywhere. The only one-way exception is a
new Direct safety request arriving after an established Group: it may coexist only when fully
disjoint and its non-Group identity has `TailSizeBytes=0`; that path must not clear, copy, or otherwise
write its nominal eight-byte suffix slot. An overlap widens/preempts the full Group mask. Cleanup
validates only the bytes actually owned by the identity; it must not require an unused safety suffix
slot to contain zero.

A fresh Group safety command with no live Group rejects every live non-Group owner outside its exact
requested mask before mutation; owners selected by the mask remain eligible for safety preemption.
An existing Group safety transition may preserve a fully disjoint Direct safety owner only under the
same zero-tail and preemption-root invariants. This exception does not permit a fresh ordinary or
lifecycle Group command over any non-Group owner.

Each 12-DINT Axis header is exact:

```text
+0  Magic = 0x49444100 + AxisIndex, published last
+1  Version = 1
+2  IdentitySizeBytes
+3  AdmissionToken
+4  OwnerGeneration
+5  ExactAxisMask
+6  CallerSessionEpoch
+7  RequestSequence
+8  (Reference << 16) | CommandId
+9  OwnerKind | (ResourceKind << 8) | (AdmissionMode << 16)
+10 TailOffsetBytes
+11 TailSizeBytes
```

The 10-DINT Group header at `422..431` uses magic `0x49444752`, version 1, identity size,
admission token, generation, exact mask, session epoch, request sequence, packed command/reference,
and packed owner/resource/admission in that order. Its suffix offset is zero.

`OwnershipLeaseIdentityState[0..313]` stores the exact Group suffix and `314..323` stores the same
10-DINT Group header with magic `0x49444C53`. `OwnershipPreemptedIdentityState[0..313]` stores the
complete current suffix, `314..421` stores the exact 108-DINT observer snapshot, and `422..431`
stores the preemption root header:

```text
422 Magic = 0x49445052, published last
423 VersionAndFlags, low 16 bits = 1, bit 16 = ForceQuarantine
424 SafetyAdmissionToken
425 SafetyOwnerGeneration
426 SafetyAxisMask
427 SafetyCallerSessionEpoch
428 SafetyRequestSequence
429 (SafetyReference << 16) | SafetyCommandId
430 CleanupRequiredAxisMask
431 CleanupCompleteAxisMask
```

Before storing any identity or snapshot, clear its magic first, finish all exact byte copies and
header fields, then publish magic last. Validation compares exactly `IdentitySizeBytes` bytes with
`_memcmp`; it must not compare rounded DINT counts or read a one-byte identity as `UDINT`.

## 5. Required caller identity sizes

All pointers start at the request payload, not at the 8-byte LMC frame header.

| caller | pointer | exact size |
|---|---|---:|
| Diagnostics DS402/encoder admission | `#RequestBuf[8]` | 72 bytes |
| LMC current-position-zero Home admission | `#RequestBuf[8]` | 56 bytes |
| ordinary Axis/Group admission | `#RequestBuf[8]` | `Payload` bytes |
| `LMCControlCommandService.HandleRequest` final pre-wire fence | `pRequestFrame + 8` | `RequestFrameSize - 8` |
| Home start final pre-wire fence | `pRequest` | `RequestSize` = 56 |
| DS402/encoder start final pre-wire fence | `pRequest` | `RequestSize` = 72 |

The existing Diagnostics admission at `RequestBuf[16]` is incorrect because it omits the first eight
payload bytes. The fixed `16 DINT` ordinary/group identity is also insufficient.

## 6. Exact snapshot and restore invariants

- Copy the current record and complete identity before overwriting any selected axis.
- Validate the complete selected mask and every snapshot first; mutate only in a second loop.
- The lease bank is global recovery evidence for any coherent live or snapshotted `GROUP_ACTIVE`
  tuple. A disjoint safety reservation and pre-wire rollback must preserve it even when that Group
  tuple is outside the safety mask. Clear it only after an exact Group lease is restored live and the
  bank is redundant, or after an explicitly successful lease-destroying command.
- Every consumer validates the lease bank as an exact successful `0x2047` Group lease before any
  reservation, token, snapshot, or dispatch mutation: identity size 1, reference `0x0100`, nonzero
  session and request sequence, exact mask/token/generation, exact record command/reference/resource/
  admission fields, payload prefix byte zero exactly `Execute=1`, zero remaining 63 prefix bytes,
  zero suffix, and a complete magic-last header. A range-valid or partially coherent bank is
  corruption.
- `0x20A4`, `0x7D22`, and an eligible `0x2085` restore the exact prior Group lease on terminal success
  or a proven pre-wire rollback.
- Restoration copies token, generation, session, request sequence, acquire time, command, reference,
  the complete record, and every payload identity byte without synthesizing operational data. The
  compact preemption bank does not retain the redundant old identity header, so rollback may
  deterministically republish that header only from fully validated record metadata, with magic
  cleared first and published last. The lease bank retains its exact header and must copy it.
- Delete the synthetic `0x2047` rollback record and every state-only restoration of a prior lease.
  A successfully accepted `0x2047` still transitions its own exact current reservation tuple and
  complete identity into the new Group lease; that terminal transition is not a prior-lease restore.
- Successful `0x2048` and `0x204B` destroy the lease; they do not restore it.
- Direct safety preemption of a Group member never restores a normal Group lease after dispatch.
- A second safety request never overwrites the first root snapshot. Repeated Stop coalescing and
  monotonic Stop-to-PowerOff escalation are implemented in source/static form; the private IDE ABI,
  C78 build and PLC runtime proof remain activation blockers.

The three singleton tuples in `OwnershipState[7..15]` stay bound to the original special owner until
exact owner-specific cleanup completes. A safety token must not be reused as a Home or SDO executor
operation token.

### 6.1 Preemption copy result and destination ABI

`CopyAxisOwnershipPreemption` returns `0` for no preemption, `1` for `PENDING_FREEZE`, `2` for
`CLEANUP_REQUIRED`, `3` for `CLEANUP_COMPLETE`, and `4` for `CLEANUP_QUARANTINED`. Errors are
`-1` invalid argument, `-2` missing old token/generation, `-3` corrupt bank, and `-4` destination too
small.

The preemption bank snapshots all nine axis records, including owners that are disjoint from the
safety mask. A fully disjoint old tuple returns `0` only after the current owner is still in an
owner-specific ACTIVE state and its immutable tuple, current identity header, exact 64-byte prefix,
and exact suffix all match the snapshot. A partial mask overlap or any failed proof is corruption and
returns `-3`; it must not be converted into a local cleanup request.

The destination starts with a 144-byte, 36-word native header. Word offsets `0..7` are magic
`0x4C4D4350` published last, version 1, header size 144, total size, status, flags, cleanup-required
mask, and cleanup-complete mask. Words `8..25` contain the complete preempted tuple, state,
identity size, acquisition/observation cycles, and last report. Words `26..35` contain the safety
tuple/state and reserved zero. The exact original identity starts at byte 144. Required destination
sizes are 200 bytes for 56-byte Home, 216 bytes for 72-byte DS402/encoder maintenance, and 1464
bytes for the 1320-byte Group configuration request.

### 6.2 Cleanup publication

`PublishAxisOwnershipPreemptionCleanup` accepts only `SAFETY_PREEMPTING` or uncertain
`QUARANTINED`; it rejects `RESERVED`. `CleanupKind=1` means complete-safe, `2` means
complete-quarantine, and `3` means incomplete-quarantine. Complete kinds clear only the exact
singleton old-owner tuple and set its completion bit. Quarantine kinds also set `ForceQuarantine`;
kind 3 retains the singleton tuple and leaves its completion bit clear.

Executor `IsReusable` alone is not exact-token retirement proof. Encoder kind 2 and DS402 kind 1
require either an exact `CopyCompletion`, or an exact-token `MarkOrphan` result followed by the
executor reaching reusable/idle. Token mismatch, impossible state, missing client, or timeout without
that evidence uses kind 3 and retains the singleton tuple.

Result values are `0` applied, `1` exact idempotent replay, `-1` invalid argument, `-2` tuple or
evidence mismatch, and `-3` invalid phase/corrupt snapshot.

After a completed cleanup, the retired exact old token/generation/mask must be absent. Its singleton
slot may be all zero or may contain a fully coherent replacement bound to the exact current live
owner record and identity. A partial or mixed old tuple is corruption. An idempotent replay returns
`1` without clearing or modifying a coherent replacement owner.

## 7. LMC current-position-zero Home cancellation

Mailbox slots `0..3` remain immutable after request publication. Slot 4 is the RT-owned irrevocable
native-dispatch claim. Slot 5 is `CancelRequestSequenceEcho`; slots 6 and 7 stay zero.

`SubmitAxisZeroHome` clears slots `4..7`, atomically clears `AxisZeroHomeCancelSequence`, and publishes
the request sequence last. `CancelAxisZeroHome` writes the current request sequence to slot 5 first,
then atomically publishes the same value to `AxisZeroHomeCancelSequence`. It returns `1` for an exact
active cancel already/newly published, `0` for an exact terminal token, `-1` for token zero, and `-4`
for mismatch or corruption.

The RT path performs four cancel reads: in phase 0, immediately before writing slot 4, on verify-phase
entry, and after all raw/application/internal evidence checks immediately before committing a stable
sample or success. The fourth read is the success/cancel linearization point. Slot 5 visible while the
cancel atomic is zero is a publication gap and must wait without dispatch or stable/evidence commit;
the retained evidence is restored to `DISPATCH` for sample count zero or `VERIFIED` otherwise. A
current cancel atomic with a different echo is corruption. Pre-claim cancel publishes `FAILED/-10`
with native-call count zero; post-claim cancel publishes the same failure with native-call count one
and therefore forces ownership quarantine. The 32-DINT result ABI remains unchanged.

## 8. IDE and static verification

After declaration generation and implementation edits:

1. Save All.
2. Rebuild project with compiler C78 for ARM.
3. Search implementations for every new variable/function name.
4. Search `RequiredPhase` and verify all final-prewire and ACTIVE fences remain present.
5. Verify no new `%TEMP%\Lasal2.log` `CInvalidArgException` after the smoke start time.
6. Run ownership mutation fixtures, encoder fixtures, full SourceOnly verification with
   `-ExpectedSdoWriteAxis 1`, C# tests, and `git diff --check`.
7. At this overlay checkpoint, confirm every activation gate remains `FALSE`. For the later TW-only
   activation, confirm only the TW19/TW20 gates are `TRUE` and all unrelated gates retain their own state.

## 9. Repeated safety and monotonic PowerOff escalation contract

The second safety request must not replace the first safety tuple. The retained command, admission
token, owner generation, caller session, request sequence, request identity, preempted-owner bank,
root header and cleanup masks remain bound to the first accepted lineage. This preserves every Home,
DS402 and encoder cleanup publication that already references the first safety token.

`ReserveAxisOwnership` uses the following internal result contract before any token counter or bank
mutation:

| result | meaning | mutation/native rule |
|---:|---|---|
| `0` | fresh reservation | existing RESERVED path |
| `1` | exact repeat or a weaker request already covered by accepted PowerOff | return retained mask/token/generation; zero mutation and zero native calls |
| `2` | Stop/Disable-to-PowerOff escalation | return retained mask/token/generation; the repeat helper may dispatch the matching PowerOff exactly once |
| `-2` | coherent but unsupported overlap, scope or identity conflict | zero wire/native calls |
| `-3` | corrupt tuple, identity, observer or preemption bank | fail closed |

Only a coherent `SAFETY_PREEMPTING` tuple can return `1` or `2`. Before escalation, a repeated Stop
payload must be byte-exact; a different payload is a conflict. After PowerOff escalation is accepted,
a retained Axis Stop coalesces a valid Stop or PowerOff for the same reference regardless of Stop
payload, and a retained Group Stop/Disable coalesces a valid Group Stop, Disable or PowerOff. These
contained repeats return `1` with zero native calls. A fresh exact `RESERVED` tuple reaches the normal
first-dispatch path through the helper-only `-11` sentinel. An external repeat against a structurally
valid `RESERVED` tuple returns `-2`; a damaged RESERVED or any QUARANTINED tuple returns `-3`.

A root may be absent only when both complete preemption banks are zero and no selected observer says
`PREEMPTED`. If root magic is present, `CopyAxisOwnershipPreemption` with `DestSize=144` must validate
the complete old-owner snapshot, returning the expected destination-too-small proof `-4`, before even
a disjoint/no-match request may return conflict `-2`. Root corruption is never hidden as a conflict.

The accepted escalation is recorded only with
`LMC_OWNER_OBSERVER_POWER_OFF_ESCALATED = 0x00000100`. It does not rewrite the retained command,
identity or root flags. The semantic known mask is
`LMC_OWNER_OBSERVER_KNOWN_MASK = 0x000001FF`; all five observer unknown-bit checks use
`(0xFFFFFFFF xor LMC_OWNER_OBSERVER_KNOWN_MASK)`. Literal `0xFFFFFF00` remains only at the one-byte
identity guard and `0xFFFE0000` remains the root known-flag mask. The escalation bit is legal only on
retained Axis Stop, Group Disable or Group Stop. After the bit is published, observer sample slots
`+6/+7/+8` and record evidence slots `+9/+10` are reset so an earlier Standstill sample cannot satisfy
the stronger PowerOff predicate.

Axis Stop `0x2022` to Axis PowerOff `0x2023` completes only when the original reference axis satisfies
`referencePowerOff`. Group Disable/Stop `0x2048/0x2085` to Group PowerOff `0x204B` completes only when
`(groupPowerState = 0) & allPowerOff`. A profile-mask `0x000F` Group Disable/Stop escalated to robot
PowerOff `0x01FF` publishes `FORCE_QUARANTINE` only in the live observer; the unchanged root force may
remain zero under this exact exception. It cannot restore or release the prior Group lease. A direct
safety lineage widened by Group preemption also remains force-quarantined.

The repeat implementation is isolated from the near-limit `HandleRequest` body in one private method.
The implementation exists, but the LASAL private declaration and `Classes.lcb` metadata do not. Its
exact IDE ABI is:

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

`-11` is the Control repeat helper's internal not-applicable sentinel; `-10` is the separate TCP
same-RESERVED drain sentinel. Neither reaches the wire. TCP rejects only negative admission results,
so `0`, `+1` and `+2` all invoke the Control service. `controlReserved` remains exclusive to the
`0x7D13` Home reservation because ordinary terminal Commit/Rollback is owned by `HandleRequest`.
Reserve `+1/+2` values are not wire responses; a positive repeat-helper result is a completed
canonical response byte count and returns from `HandleRequest` immediately.

A coalesced request calls no native handler, `CommitAxisOwnership`, `RollbackAxisOwnership` or
`PublishAxisOwnership`. Escalation calls only the matching PowerOff handler once. A proven
pre-dispatch failure leaves the retained lineage byte-exact; marker-after or malformed/uncertain
results quarantine that lineage and must not roll it back.

## 10. TW19 retained current-position-zero barrier

TW19 changes the drive's multi-turn position and therefore invalidates the PLC application's current
position basis. A successful TW19 SDO write is not sufficient evidence that application position is
safe for the next motion. The selected physical axis must remain behind a retained, fail-closed
barrier until an exact LMC current-position-zero Home receipt completes successfully.

This barrier is independent of the common owner record. DS402 Home, another maintenance request,
owner cleanup, session cleanup, a safety preemption, and a PLC restart must not erase it.

### 10.1 Hidden retained server channel and encoding

Add the following hidden server channel to `LMCControlCommandService` in LASAL IDE:

```text
AxisRebaseRequiredState : SvrCh_UDINT
  Initialize     = true
  DefValue       = 0x5242530F
  WriteProtected = false
  Retentive      = File
  Visualized     = false
```

It is class-local state. Do not add a Comm Network connection, client channel, public/global helper,
or user-facing write route for it.

The word format is exact:

```text
bits 31..8 = 0x524253
bits 7..4  = mask xor 0xF
bits 3..0  = physical-axis mask
```

Equivalently, an encoded value is `0x52425300 + ((mask xor 0xF) << 4) + mask`, where `mask` is
limited to `0x0..0xF`. `0x524253F0` is the valid empty state. The initialized value
`0x5242530F` is the valid all-four-axes-required state. Any wrong magic, complement mismatch, or
out-of-format value decodes to effective mask `0xF`; corruption never opens motion.

Each update is a read-decode-modify-encode-write-readback transaction on the single UDINT. Arming
ORs the selected axis bit. Clearing removes only the exact successfully Homed axis bit and preserves
all other bits. A write/readback mismatch is treated as not applied: an unconfirmed arm cannot
proceed to SDO, and an unconfirmed clear cannot publish Home `Result=1`. The exact owner or Home
receipt evidence retains the retry. This condition must not set the common global-corruption word or
quarantine an otherwise coherent owner merely because retained-channel persistence has not yet been
confirmed.

The codec is isolated from the already-large reservation/publication methods in two private
`LMCControlCommandService` functions. `ReadAxisRebaseRequiredMask` owns the exact magic/complement
decode and fail-closed effective-mask rule. `UpdateAxisRebaseRequiredState` owns the bounded set/
clear, encode, server-channel write and exact readback transaction. Callers must not duplicate or
partially inline this persistent-word codec in `ReserveAxisOwnership`, `PublishAxisOwnership`,
`CommitAxisOwnership` or the Home receipt path.

The private IDE ABIs are exact:

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

The read result is the effective `0..15` mask, including fail-closed `15` for an invalid encoded
word. The update result is `0` only after exact write/readback, `-1` for an invalid set/clear request,
and `-4` for a persistence retry. A set and clear mask may not overlap.

### 10.2 TW19 arm linearization point

The barrier is armed in `CommitAxisOwnership`, not after SDO completion. The arming case must match
the complete reserved tuple and stored request identity:

- command `0x7E53`;
- owner kind `LMC_OWNER_KIND_ENCODER`;
- resource `LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE`;
- admission `LMC_OWNER_ADMISSION_LIFECYCLE`;
- maintenance kind `2` (TW19);
- one physical reference axis `1..4` with the exact matching single-axis mask.

After the existing full RESERVED validation succeeds, publish and read back the rebase bit before
changing any selected owner record to ACTIVE/QUEUED and before any SDO executor can receive a write.
If the retained word cannot be confirmed, return without owner mutation and without SDO dispatch so
the exact reservation can retry the commit. TW20 maintenance kind `1` does not arm the barrier.

Arming before the SDO is intentionally conservative. A post-arm TW19 dispatch failure leaves the bit
set and requires LMC Home. Repeated TW19 and TW20 maintenance remain admissible while the bit is set;
they never clear it.

### 10.3 Admission matrix while a rebase bit is effective

For a direct request, apply the barrier to its exact axis. For a Group request, apply it when the
resolved Group mask intersects the effective physical mask. An invalid retained word is treated as
all four physical bits set. This policy check is unconditional; it is not bypassed when ordinary
ownership, DS402 Home or startup activation gates remain dormant.

| request class | decision while affected | clear effect |
|---|---|---|
| read/status/outcome queries | allow | none |
| Axis/Group Reset | allow | none |
| Axis Stop or PowerOff | allow | none |
| Group Disable, Stop or PowerOff | allow | none |
| TW19/TW20 Start, Outcome or Retire | allow | none |
| exact LMC Home Start, Outcome or Retire | allow | only the exact success receipt path may clear |
| Axis PowerOn, MoveAbsolute, MoveRelative or MoveVelocity | block before native call | none |
| `0x7D12` SetPosition | currently dormant/unavailable; barrier is mandatory before future activation | none |
| `0x7D15` DS402 Home Start | block before native call | none |
| Group Enable, PowerOn, motion or `0x7D22` mutation | block before native call | none |
| `0x20E7` SetKin | block after full `kinValid` parsing and before `GroupKinematicReady`/native marker | none |

DS402 Home Outcome/Retire and other non-motion cleanup must remain available so a pre-existing ledger
can drain, but DS402 Home success is not a substitute for LMC current-position-zero Home. Safety
Stop/PowerOff/Disable always outrank this barrier. A Reset may remove a drive fault but cannot clear
the retained position-basis requirement.

Shape and identity validation still precede this policy decision. A malformed request must reach its
normal parser error rather than being reported as a rebase conflict. A blocked, otherwise valid
request uses its existing command-specific ownership-conflict/fail-closed response and performs zero
owner/native/SDO mutation. Where the adapter ABI applies, that response is symbolic
`-9 AxisOwnershipConflict`; an Admin envelope retains its existing Admin error/detail shape. The
currently dormant `0x7D12` route keeps its present dormant response and native-call-zero behavior;
its future activation is forbidden until the same parse-first, barrier-before-native rule is added.
For `0x20E7`, the exact barrier boundary is inside the existing handler after the complete payload
has produced `kinValid=TRUE` and before `GroupKinematicReady` or any native-call marker. A malformed
SetKin payload therefore keeps the existing `-7` parser result; only a fully valid affected request
is converted to the rebase conflict, with no native marker or configuration mutation.

### 10.4 The only clear linearization point

Only the exact `0x7D13` LMC current-position-zero Home lineage may clear a bit. The Home operation
must already have a terminal-success report, the durable receipt must be in COMPLETE phase, and all
existing token, generation, session, sequence, reference, single-axis mask and evidence validation
must still match. Clear and read back the selected bit immediately before returning terminal
`Result=1` from the COMPLETE receipt replay path.

ACK, running, terminal safe failure, cancel, quarantine, DS402 Home success, TW19/TW20 completion,
Retire, owner release, and session cleanup never clear a bit. If clear persistence is not confirmed,
do not return `Result=1`; retain the completed Home receipt and retry the clear on the next exact
outcome read. This persistence retry does not globally quarantine the Home or ownership subsystem.

### 10.5 Activation and proof boundary

The retained channel declaration, generated metadata and external implementation must agree before
C78 Rebuild. C78 success alone does not prove File retention. Qualification requires at least:

1. initialized `0x5242530F` blocks PowerOn/motion on axes 1..4 but allows safety and LMC Home;
2. exact successful Home clears only its axis and publishes `0x524253F0` after all four clear;
3. successful TW19 re-arms only its selected axis before the SDO path can run;
4. failed/quarantined TW19 and failed/quarantined Home keep the bit asserted;
5. unrelated TW20, Reset, cleanup and owner retirement do not change the word;
6. a controlled invalid word fails closed as effective `0xF`;
7. PLC restart and target power loss preserve the last confirmed encoded value;
8. a blocked request produces the existing conflict response with zero native/SDO calls.

Until those checks pass on the downloaded target, TW19 remains unqualified for subsequent motion
even if its SDO terminal record reports success.

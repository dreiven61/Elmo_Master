# SetOperationMode Detail 49 observability implementation result - 2026-08-31

> Status: **OBSERVABILITY PHYSICALLY CONFIRMED / ADMISSION BOUNDARY DEFECT OPEN**
>
> implementation baseline: `dev@1ab539c4b82918d1e2095e73c03799415d9d06d0`
>
> latest runtime: `Build=1 / BootId=0x0000006A / MapRevision=0x957F101E`
>
> Production release posture: **NO-GO**

## 1. Implemented protocol split

The previous broad Detail 49 producer was split without weakening admission or mutation safety.

| Detail | Symbol | Producer condition |
|---|---|---|
| 49 | `SetOperationModeOutcomeStorageUnavailable` | retained outcome/storage availability |
| 52 | `SetOperationModeOwnershipChannelUnavailable` | `AxisOwnership` client disconnected |
| 63 | `SetOperationModeAdmissionIdentityUnavailable` | caller session, request sequence, admission token, or owner generation is zero |
| 64 | `SetOperationModeFeatureDisabled` | loaded runtime feature gate is disabled |
| 42 | ownership conflict/quarantine | ownership identity validation or commit failed |

## 2. Physical result of the split

Latest Axis1 CSP(8) -> PP(1) qualification returned:

```text
WPF BuildUtc=2026-08-31 04:17:26 UTC
SDK BuildUtc=2026-08-31 04:17:24 UTC
Build=1
BootId=0x0000006A
MapRevision=0x957F101E
RequestId=4
StatusWord=0x02D0
Detail=SetOperationModeAdmissionIdentityUnavailable(63)
```

The host path reached:

```text
canonical UI handler
-> cross-mode preflight PASS
-> final Diagnostics refresh PASS
-> Prepare PASS
-> durable journal arm PASS
-> Start dispatch
-> definitive PLC Detail 63 reject
```

No successful Start acknowledgement and no `0x6060` mutation evidence were observed.

Therefore the Detail 49 split itself is now **physically confirmed to discriminate the failure**. The next blocker is no longer generic OutcomeStorageUnavailable; it is the admission identity transfer boundary.

## 3. What Detail 63 proves

`HandleAxisSetOperationModeStart()` emits 63 when any of these are zero:

```text
CallerSessionEpoch
RequestSequence
AdmissionToken
OwnerGeneration
```

At least one is therefore zero at Diagnostics entry.

The current protocol response does not say which one. The previous documentation statement that the implementation already provides all per-field zero/nonzero evidence was too strong. Current implementation provides a grouped Detail 63 only.

The next implementation must add per-boundary non-sensitive bitmap evidence before another root-cause patch is selected.

## 4. Source invariant analysis

`TCPMotionInterface` invokes `ControlCommands.ReserveAxisOwnership()` with the active request session/sequence and output pointers for admission token/generation.

It dispatches SetOperationMode to `Diagnostics.HandleRequest()` only when reservation returns `0`, forwarding the same session/sequence plus the returned token/generation.

`ReserveAxisOwnership()` rejects zero session/sequence. Its normal success path forces token/generation nonzero, writes them to the output pointers, then returns `0`. Repeat reservation success also copies a previously validated nonzero tuple to the outputs.

Thus under a correct generated ABI/marshalling contract:

```text
ReserveResult == 0
AND Diagnostics dispatch
AND Detail63 zero tuple
```

is an impossible state.

## 5. Current root-cause priority

### Proven defect class

Admission identity state is lost/corrupted or observed inconsistently across the ownership reservation -> TCP caller -> Diagnostics service boundary.

### Highest-priority hypotheses

1. generated `CltChCmd_LMCControlCommandService` client/server ABI mismatch;
2. output pointer marshalling failure for `pAdmissionToken` / `pOwnerGeneration`;
3. server reservation complete but TCP caller outputs remain zero;
4. TCP -> Diagnostics argument marshalling/cached generated interface mismatch;
5. tuple overwrite between successful Reserve and Diagnostics call.

Do not select one of these as final root cause until boundary evidence distinguishes them.

## 6. Next implementation tranche

### P0-A: three-boundary bitmap instrumentation

A — `ReserveAxisOwnership()` successful server exit:

```text
SessionNonZero
SequenceNonZero
TokenNonZero
GenerationNonZero
EffectiveAxisMaskNonZero
```

Expected = `0x1F`.

B — `TCPMotionInterface` immediately after Reserve returns:

same expected bitmap `0x1F`.

C — `LMCDiagnosticsService.HandleAxisSetOperationModeStart()` entry:

```text
SessionNonZero
SequenceNonZero
TokenNonZero
GenerationNonZero
```

Expected = `0x0F`.

Do not log raw token/generation values in normal operator output.

### P0-B: generated ABI regeneration and fingerprint

Regenerate the related LASAL class interfaces as one generation and capture fingerprints for:

- `LMCControlCommandService` GLOBAL method interface;
- `LMCDiagnosticsService` GLOBAL method interface;
- `TCPMotionInterface` `ControlCommands` / `Diagnostics` clients;
- communication network generated table;
- `Classes.lcb` and project artifact.

Generated headers/declarations should not be hand-reordered as a substitute for code generation.

### P0-C: correction selected from evidence

```text
A invalid
 -> Reserve server logic

A valid, B invalid
 -> ControlCommands client/output marshalling or generated ABI

A/B valid, C invalid
 -> TCP -> Diagnostics HandleRequest ABI/marshalling

A/B/C valid, Detail42
 -> ownership identity validation/commit

A/B/C valid and Start accepted
 -> proceed to 0x6060 one-write qualification
```

## 7. Safety / cleanup requirement

If Reserve reports success but the TCP caller receives an incomplete tuple, do not synthesize the missing token and do not replay Start.

Because server-side ownership may already have been reserved, an impossible-state guard also needs a safe reconciliation/cleanup design keyed by exact reservation/session evidence. A local early return without cleanup is not sufficient if it leaks ownership state.

## 8. Acceptance criteria for the next physical run

Before testing mode mutation, establish:

```text
Boundary A = 0x1F
Boundary B = 0x1F
Boundary C = 0x0F
```

Only after those invariants hold may the run proceed to:

```text
one 0x6060=1 dispatch
-> 0x6061=1 verify
-> terminal retained outcome
-> exact-generation retire
```

The current run does **not** qualify SetOperationMode physical functionality.

## 9. Safety invariants unchanged

- no ownership reservation/validation bypass;
- no synthesized admission token/generation;
- no relaxation of Standstill/Fault/OperationEnabled;
- no removal of current Diagnostics observation freshness;
- no replay after accepted/uncertain Start;
- no raw Generic SDO write to `0x6060`;
- production remains **NO-GO**.

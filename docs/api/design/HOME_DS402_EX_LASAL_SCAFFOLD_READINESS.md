# HomeDS402Ex LASAL Scaffold Readiness

Date: 2026-08-25

This document prepares `HOMEEX-06` without modifying LASAL function bodies or enabling HomeDS402Ex.

## Verified current baseline

Current `dev` before the readiness work:

- HomeDS402 lifecycle `0x7D15/0x7D16/0x7D17` is routed in `TCPMotionInterface.st`.
- SetOperationMode lifecycle `0x7D23/0x7D24/0x7D25` is routed in `TCPMotionInterface.st`.
- HomeDS402 Start uses owner kind 4 and shared Home resource kind 3.
- SetOperationMode Start uses owner kind 6 and diagnostics SDO resource kind 4.
- `LMCDiagnosticsService.st` contains `Ds402HomeState[0..127]` and `AxisOperationModeState[0..191]`.
- HomeDS402Ex `0x7D1B/0x7D1C/0x7D1D` is not present in the LASAL TCP/diagnostics sources.
- no `Ds402HomeExState`, `HandleAxisDs402HomeEx*` or `ProcessAxisDs402HomeEx` LASAL symbols exist yet.
- Admin feature mask is exactly `0x00000017`; bit 11 is OFF.

`tools/Verify-HomeDs402ExLasalBaseline.ps1` locks this baseline and must PASS before any scaffold implementation begins.

## Frozen items already supplied by the SDK contract

The following are frozen and should not be changed by the LASAL scaffold:

- Start command: `0x7D1B`
- ReadOutcome command: `0x7D1C`
- Retire command: `0x7D1D`
- Start request payload: 116 bytes
- Query request payload: 116 bytes
- Retire request payload: 120 bytes
- Start response: strict 24-byte domain response
- Outcome/Retire success response: strict 176 bytes
- ExecuteToken: `0x58453448` (`H4EX`)
- Admin feature bit: 11 (`0x00000800`)
- ErrorCatalogVersion requirement: 7
- physical axis range: 1..4
- full recovery identity: Build + BootId + MapRevision + original RequestId + 128-bit ClientIntentId + axis + all frozen plan fields
- recovery query/retire must never execute Home or replay parameter writes
- successful terminal proof requires all six cleanup flags before retirement
- capability bit 11 remains OFF until final paired activation.

## Frozen ownership ABI

The ownership ABI is frozen in `HOME_DS402_EX_OWNER_ABI.md`:

- HomeDS402Ex OwnerKind = **7**
- ResourceKind = **3**, reusing the existing `DS402_HOME_ENGINE`
- AdmissionMode = **4** lifecycle
- Start `0x7D1B` is the only command that may create the HomeDS402Ex owner reservation
- ReadOutcome `0x7D1C` and Retire `0x7D1D` never create or replay a Start reservation
- physical axis mask is exact `1 << (Reference-1)` for Reference 1..4
- active-state value **13** is reserved for the later actual runtime tranche; HOMEEX-06 gate-OFF scaffold must not transition into it.

ResourceKind 3 is intentionally shared with legacy HomeDS402 so both APIs serialize on the same DS402 Home engine. OwnerKind 7 remains distinct so the two APIs retain separate lifecycle/recovery identity.

## HOMEEX-06 scaffold boundary

The first LASAL tranche should be a **gate-OFF parser/state/outcome scaffold only**. It must not perform homing motion, SDO parameter programming, controlword bit 4 changes, mode switching, setpoint alignment or capability activation.

Expected structural work:

1. `TCPMotionInterface.st`
   - recognize `0x7D1B/1C/1D` in the diagnostics lifecycle route;
   - validate exact request sizes before admission;
   - Start only: physical axis mask must be exact `1 << (Reference-1)` for Reference 1..4;
   - reserve/validate/commit ownership using OwnerKind 7 / ResourceKind 3 / AdmissionMode 4;
   - keep legacy HomeDS402 OwnerKind 4 / ResourceKind 3 behavior unchanged;
   - Query/Retire are recovery operations and must not reserve a new motion Start intent;
   - preserve exact accepted/rejected response-shape checking.

2. `LMCDiagnosticsService.st`
   - add a dedicated HomeDS402Ex state/outcome record, not an alias of `Ds402HomeState`;
   - add Start/Outcome/Retire handlers and a processor entry point;
   - parser must validate every reserved/spare field and full exact key;
   - Start may only create/inspect a dormant outcome record while the runtime gate is OFF;
   - Query returns only an existing exact-key record;
   - Retire requires exact terminal record generation;
   - no Start replay from Query/Retire;
   - no SDO write or RT mailbox behavior in HOMEEX-06;
   - no transition into reserved active state 13 while the runtime gate is OFF.

3. `LMCControlCommandService.st`
   - define OwnerKind 7 and extend the current owner-kind upper bound from 6 to 7;
   - ResourceKind 3 must accept exactly legacy HomeDS402 `(OwnerKind 4, Start 0x7D15)` or HomeDS402Ex `(OwnerKind 7, Start 0x7D1B)`, both lifecycle mode 4;
   - no other OwnerKind/command pairing may consume ResourceKind 3;
   - preserve per-axis exclusivity, safety-preemption, quarantine and session-close semantics;
   - do not advertise bit 11;
   - existing HomeDS402/SetOperationMode ownership behavior must remain unchanged.

4. verification
   - source verifier must distinguish `BASELINE_OFF`, `SCAFFOLD_OFF`, and later `ACTIVE` states;
   - `SCAFFOLD_OFF` requires routes/handlers to exist while Admin bit11 remains OFF and runtime mutation sites remain absent;
   - OwnerKind 7 must be accepted only for the exact HomeDS402Ex lifecycle tuple;
   - ResourceKind 3 must remain shared with legacy HomeDS402 and must not accept unrelated owners;
   - generic D5 writes must remain unable to manufacture HomeDS402Ex state;
   - SourceOnly and 7-bit ASCII checks remain mandatory for modified `.st` files.

## Activation prohibition

HOMEEX-06 must leave all of the following true:

- Admin feature mask `0x00000017`
- HomeDS402Ex bit11 OFF
- no public engineering-unit Prepare
- no WPF HomeDS402Ex Start UI
- no HomeDS402Ex motion/SDO/controlword runtime execution
- no production or hardware qualification claim.

`HOMEEX-06` is not complete merely because the baseline verifier or ABI documentation passes. It becomes complete only after the actual gate-OFF LASAL parser/state/outcome scaffold exists and passes its dedicated source/static qualification.
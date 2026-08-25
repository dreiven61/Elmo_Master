# HomeDS402Ex LASAL Scaffold Readiness

Date: 2026-08-25
Status: **HOMEEX-06 source/static scaffold completed; HOMEEX-07 ownership/runtime remains closed**

This document records the final HOMEEX-06 boundary after the gate-OFF LASAL parser/state/outcome scaffold was implemented and statically qualified.

## Verified pre-scaffold baseline

Before HOMEEX-06 implementation:

- HomeDS402 lifecycle `0x7D15/0x7D16/0x7D17` was routed in `TCPMotionInterface.st`.
- SetOperationMode lifecycle `0x7D23/0x7D24/0x7D25` was routed in `TCPMotionInterface.st`.
- HomeDS402 Start used owner kind 4 and shared Home resource kind 3.
- SetOperationMode Start used owner kind 6 and diagnostics SDO resource kind 4.
- `LMCDiagnosticsService.st` contained `Ds402HomeState[0..127]` and `AxisOperationModeState[0..191]`.
- HomeDS402Ex `0x7D1B/0x7D1C/0x7D1D` did not exist in LASAL TCP/diagnostics source.
- no `Ds402HomeExState`, `HandleAxisDs402HomeEx*` or `ProcessAxisDs402HomeEx` LASAL symbols existed.
- Admin feature mask was `0x00000017`; bit 11 was OFF.

That baseline was locked before the source transform and the exact application workflow passed before creating the tracked source commit.

## Frozen SDK/wire contract retained by HOMEEX-06

HOMEEX-06 does not change the frozen C# contract:

- Start command: `0x7D1B`
- ReadOutcome command: `0x7D1C`
- Retire command: `0x7D1D`
- Start request payload: 116 bytes
- Query request payload: 116 bytes
- Retire request payload: 120 bytes
- Start domain response: strict 24 bytes
- Outcome/Retire success response: strict 176 bytes
- ExecuteToken: `0x58453448` (`H4EX`)
- Admin feature bit: 11 (`0x00000800`)
- ErrorCatalogVersion requirement: 7
- physical axis range: 1..4
- full recovery identity: Build + BootId + MapRevision + original RequestId + 128-bit ClientIntentId + axis + all frozen plan fields
- Query/Retire never execute Home or replay parameter writes
- successful terminal proof later requires all six cleanup flags before retirement
- capability bit 11 remains OFF until final paired activation.

## Frozen ownership ABI and resolved tranche boundary

The ownership ABI remains frozen in `HOME_DS402_EX_OWNER_ABI.md`:

- OwnerKind = **7**
- ResourceKind = **3**, reusing `DS402_HOME_ENGINE`
- AdmissionMode = **4** lifecycle
- Start `0x7D1B` is the only command that may later create the HomeDS402Ex owner reservation
- ReadOutcome `0x7D1C` and Retire `0x7D1D` never create or replay a Start reservation
- physical axis mask is exact `1 << (Reference-1)` for Reference 1..4
- active-state value **13** is reserved for the actual runtime tranche.

However, ownership integration is **not part of the completed HOMEEX-06 source tranche**.

The current non-group ownership identity bank retains only a 64-byte prefix plus at most an 8-byte tail. That is insufficient to preserve the full 116-byte HomeDS402Ex Start identity. Adding OwnerKind 7 before extending the identity bank would weaken the exact lifecycle/recovery contract.

Therefore the paired ownership changes are moved to HOMEEX-07:

- extend the owner identity store to retain the complete HomeDS402Ex Start identity;
- define/accept OwnerKind 7 only for ResourceKind 3 + AdmissionMode 4 + Start `0x7D1B`;
- preserve legacy HomeDS402 OwnerKind 4 + ResourceKind 3 + Start `0x7D15`;
- retain per-axis exclusivity, safety-preemption, quarantine and session-close semantics;
- do not advertise bit 11.

HOMEEX-06 intentionally rejects unexpected admission token/generation values and never creates an owner reservation.

## Completed HOMEEX-06 scaffold

### `TCPMotionInterface.st`

- routes `0x7D1B/0x7D1C/0x7D1D` through the diagnostics lifecycle path;
- does **not** add HomeDS402Ex to ownership admission;
- legacy HomeDS402 and SetOperationMode routes remain unchanged.

### `LMCDiagnosticsService.st`

- declares dedicated `Ds402HomeExState[0..255]` storage;
- declares and implements dedicated Start/Outcome/Retire handlers;
- declares and pumps `ProcessAxisDs402HomeEx`;
- `ProcessAxisDs402HomeEx` is a no-op;
- `LMC_DIAG_DS402_HOME_EX_ENABLED` remains `FALSE`;
- frozen HomeDS402Ex detail values 53..62 are declared;
- Start validates the strict 116-byte shape, schema/flags/request identity, physical axis, Build/BootId/MapRevision, full 128-bit intent, standard-method candidate range, representable final position, Aborting mode, reserved/spare bytes, timeouts and `H4EX` execute token;
- a well-shaped Start remains deterministically rejected while the runtime gate is OFF;
- Query validates the strict 116-byte recovery-key shape and returns only the scaffold store state;
- Retire validates the strict 120-byte shape and requires a nonzero expected generation;
- empty scaffold state returns outcome-not-found;
- unexpected nonzero scaffold state fails as store-corrupt;
- no HomeDS402Ex state/outcome record writes occur;
- no SDO, RT latch, controlword, mode, setpoint or motion mutation is introduced.

### `LMCControlCommandService.st`

No HOMEEX-06 source change. OwnerKind 7 integration is explicitly deferred to HOMEEX-07 for the full-identity reason above.

## HOMEEX-06 qualification

The final source/static qualification is recorded in:

`docs/api/design/evidence/HOME_DS402_EX_HOMEEX06_LASAL_SCAFFOLD_20260825.md`

Key result:

- HomeDS402Ex verifier: **67 checks PASS**
- state: **SCAFFOLD_OFF**
- SetOperationMode define-order regression: PASS
- diff hygiene: PASS
- 7-bit ASCII: PASS for modified LASAL sources
- Admin feature mask: `0x00000017`
- capability bit 11: OFF
- OwnerKind 7 source support: absent by design in HOMEEX-06
- HomeDS402Ex ownership/SDO/RT/motion mutation sites: absent

The same source head also passed SetOperationMode 57-check static qualification and C78 evidence-tool self-test. Full repository SourceOnly still stops only at the pre-existing `SetPosition-augmented Classes.lcb physical identity drifted` artifact ratchet; that STOP is not reclassified as a HomeDS402Ex failure or as a PASS.

## Activation prohibition

After HOMEEX-06 all of the following remain required:

- Admin feature mask `0x00000017`
- HomeDS402Ex bit11 OFF
- `LMC_DIAG_DS402_HOME_EX_ENABLED FALSE`
- no public engineering-unit Prepare
- no WPF HomeDS402Ex Start UI
- no HomeDS402Ex owner reservation yet
- no HomeDS402Ex SDO/controlword/mode/setpoint/motion runtime execution
- no C78/PLC/hardware qualification claim
- production **NO-GO**.

HOMEEX-06 is complete only at the source/static scaffold level. HOMEEX-07 through HOMEEX-13 remain independent gates and cannot inherit this evidence.

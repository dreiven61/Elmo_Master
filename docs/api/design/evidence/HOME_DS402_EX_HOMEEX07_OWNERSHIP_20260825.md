# HomeDS402Ex HOMEEX-07 Ownership Qualification Evidence

Date: 2026-08-25
Source head: `a576586688721a6935743dc5420f62533bc13fa4`
PR: #26 `feat(homeex): add full-identity ownership admission`
Stage: **HOMEEX-07 ownership complete; runtime/capability remain OFF**

## Scope qualified

HOMEEX-07 adds only the ownership tranche required before any HomeDS402Ex runtime work:

- `OwnerKind 7` for HomeDS402Ex and active-state value `13` are reserved in the common owner ABI.
- `ResourceKind 3` remains the shared DS402 Home engine for legacy `0x7D15` and HomeDS402Ex `0x7D1B`.
- Start `0x7D1B` uses lifecycle admission and exact physical-axis ownership.
- the non-group owner identity bank preserves the complete 116-byte Start identity as a 64-byte prefix plus a 52-byte per-axis tail slot.
- TCP validates the frozen HomeDS402Ex Start shape before reserving ownership.
- Diagnostics validates the exact reserved identity before making a runtime decision.
- because runtime remains disabled, every HOMEEX-07 Start reservation is deterministically rolled back before response completion.
- legacy HomeDS402, Encoder maintenance and SetOperationMode owner tuples remain present.

## Safety boundary retained

The following remain deliberately unchanged:

- `LMC_DIAG_DS402_HOME_EX_ENABLED FALSE`
- Admin feature mask `0x00000017`
- HomeDS402Ex capability bit 11 OFF
- no HomeDS402Ex `CommitAxisOwnership`
- no HomeDS402Ex SDO execution
- no HomeDS402Ex RT latch consumption
- no controlword, mode, setpoint or motion mutation
- no HomeDS402Ex runtime/outcome record writes
- no ResourceKind 5
- production activation remains **NO-GO**

## Workflow evidence

On source head `a576586688721a6935743dc5420f62533bc13fa4`:

- HomeDS402Ex HOMEEX-07 ownership qualification: run `32816128617` — **SUCCESS**
- HomeDS402Ex LASAL stage qualification: run `32816128657` — **SUCCESS**
- SetOperationMode C78 evidence tool: run `32816128598` — **SUCCESS**
- SetOperationMode static qualification: run `32816128603` — **FAIL**, only in the repository SourceOnly contract/artifact-ratchet step

The SetOperationMode static failure is not classified as a HOMEEX-07 ownership regression. The failure is the intentional persisted-read inventory fence in `Verify-LasalContract.ps1`: the HOMEEX-07 `ValidateAxisOwnershipPreemptionReplacement` branch adds exactly three `OwnershipState[...]` reads for the new HomeDS402Ex states (`RESERVED`, `DS402_HOME_EX_ACTIVE`, `QUARANTINED`), moving the fenced inventory from 44 to 47 while pointer inventory remains unchanged.

The design assigns repository SourceOnly/method-size/C78/generated-artifact closure to **HOMEEX-09**. Therefore HOMEEX-07 does not weaken or silently re-baseline that fence; the ratchet remains visible until the HOMEEX-09 paired qualification tranche updates it with full generated-artifact evidence.

## HOMEEX-07 conclusion

HOMEEX-07 ownership admission and full-identity preservation are qualified at source/static level. This evidence does not claim HOMEEX-08 parameter execution, HOMEEX-09 SourceOnly/C78 closure, PLC load/runtime, EtherCAT packet qualification, hardware homing, or production activation.

The next runtime work must remain fail-closed until HOMEEX-01/02 axis wiring, method allowlist, scale/rounding/range and MapRevision profile approvals are available.

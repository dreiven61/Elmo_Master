# HomeDS402Ex Ownership ABI

Date: 2026-08-25
Status: frozen design contract for HOMEEX-07 ownership integration

## Frozen values

HomeDS402Ex uses:

- `OwnerKind = 7` — dedicated `AxisDs402HomeEx` owner identity
- `ResourceKind = 3` — existing `DS402_HOME_ENGINE`
- `AdmissionMode = 4` — lifecycle admission
- Start command = `0x7D1B`
- ReadOutcome command = `0x7D1C`
- Retire command = `0x7D1D`
- physical axis Reference = 1..4 only
- exact axis mask = `1 << (Reference - 1)`
- proposed active owner state = 13, immediately after current SetOperationMode active state 12

The numeric active state 13 is reserved for the later actual runtime tranche. HOMEEX-06 gate-OFF scaffold does not define or transition an active HomeDS402Ex owner state.

## Why OwnerKind 7

Current ownership kinds are contiguous:

1. Direct
2. Group
3. LMC Home
4. DS402 Home
5. Encoder Maintenance
6. Axis Operation Mode

HomeDS402Ex needs a separate semantic identity from legacy HomeDS402 because its request identity, outcome store, recovery key and execution lifecycle are independently versioned. Reusing OwnerKind 4 would make owner records unable to distinguish legacy `0x7D15` from extended `0x7D1B` intent without command-specific exceptions throughout ownership validation and recovery.

OwnerKind 7 is therefore the next dedicated value. `ReserveAxisOwnership`, identity validation and any owner-kind range guards must be paired to accept 7 only for the exact HomeDS402Ex lifecycle tuple.

## Why ResourceKind 3 is reused

HomeDS402Ex and legacy HomeDS402 operate the same physical DS402 homing engine. They must never run concurrently.

Current ResourceKind 3 already represents `DS402_HOME_ENGINE`. Creating a separate resource for HomeDS402Ex would not by itself serialize the two Home implementations and would require an additional cross-resource conflict layer. Reusing ResourceKind 3 gives the intended engine-level exclusion directly.

The shared resource does **not** mean the two APIs share an outcome record or recovery identity. They retain separate OwnerKind, command ids, parser/state/outcome storage and exact recovery keys.

The current ResourceKind 3 admission code only accepts OwnerKind 4 + command `0x7D15`. HOMEEX-07 must broaden that exact tuple check to accept either:

- legacy HomeDS402: OwnerKind 4 + Start `0x7D15`; or
- HomeDS402Ex: OwnerKind 7 + Start `0x7D1B`.

No other OwnerKind/command pairing may use ResourceKind 3.

## Axis-level conflict semantics

The ownership service maintains per-axis ownership records independently of shared resource tokens. A HomeDS402Ex Start must reserve the exact physical-axis bit in addition to ResourceKind 3.

Consequences:

- HomeDS402 on the same axis conflicts through both the per-axis record and ResourceKind 3;
- HomeDS402 on another axis still conflicts through ResourceKind 3 because the DS402 Home engine is globally shared in the current ownership model;
- SetOperationMode, SetPosition, ordinary motion, Power/Stop/Reset, encoder maintenance and other non-group axis mutations conflict through the overlapping per-axis ownership record;
- group/robot ownership conflicts through the existing overlapping-axis/group ownership rules;
- generic diagnostics SDO mutation remains independently serialized by ResourceKind 4 and must also remain prohibited from manufacturing HomeDS402Ex runtime state.

This contract intentionally does not add ResourceKind 5.

## Lifecycle rules

`AdmissionMode = 4` is retained because HomeDS402Ex is a Start / ReadOutcome / Retire durable lifecycle.

Only Start `0x7D1B` may create a new owner reservation. ReadOutcome `0x7D1C` and Retire `0x7D1D` are exact-key recovery/store operations and must not create or replay a Start owner intent.

The Start owner identity must contain the full frozen HomeDS402Ex request identity used by the diagnostics service. Query/Retire must use the retained outcome store identity and exact record generation; they must not reconstruct a new owner reservation from a recovery key.

## Why ownership starts in HOMEEX-07, not HOMEEX-06

HOMEEX-06 completed only the gate-OFF parser/state/outcome scaffold. It intentionally did not add OwnerKind 7 to source.

The current non-group ownership identity bank preserves a 64-byte prefix plus at most an 8-byte tail. The frozen HomeDS402Ex Start request is 116 bytes and its exact lifecycle identity includes fields beyond that storage window. Accepting OwnerKind 7 without first extending the identity bank would permit admission using an identity representation that cannot preserve the complete frozen Start tuple.

Therefore HOMEEX-07 must pair the owner-kind admission change with a full identity-storage extension. Until then HOMEEX-06 rejects unexpected admission token/generation values and remains non-executable.

## Required source changes in HOMEEX-07

Ownership-related source changes must be paired and fail closed:

1. extend the non-group owner identity bank so the full frozen 116-byte HomeDS402Ex Start identity is retained and exactly validated;
2. define `LMC_OWNER_KIND_DS402_HOME_EX 7`;
3. extend the OwnerKind range guard from maximum 6 to maximum 7;
4. accept OwnerKind 7 only with ResourceKind 3, AdmissionMode 4 and Start `0x7D1B` on physical axes 1..4;
5. retain legacy OwnerKind 4 + ResourceKind 3 + Start `0x7D15` unchanged;
6. update exact tuple validation, commit/rollback/session-close/recovery paths that enumerate lifecycle owner kinds;
7. reserve active state 13 only for actual runtime activation logic; ownership admission alone must not imply successful homing runtime;
8. preserve all existing safety-preemption and quarantine semantics;
9. keep Admin capability bit 11 OFF and feature mask `0x00000017` during HOMEEX-07 source work.

Any path that accepts OwnerKind 7 with another resource, command or admission mode, or accepts a truncated HomeDS402Ex owner identity, is an ABI violation.

## Non-goals

This ABI freeze does not:

- enable capability bit 11;
- approve wiring, homing methods or engineering-unit scale profiles;
- prove parameter SDO programming/restore;
- prove CSP restoration, setpoint alignment or final position;
- provide C78, PLC load or hardware qualification.

HOMEEX-06 parser/state/outcome source qualification does not satisfy this ownership gate. Production remains fail-closed until the remaining HOMEEX gates are completed.

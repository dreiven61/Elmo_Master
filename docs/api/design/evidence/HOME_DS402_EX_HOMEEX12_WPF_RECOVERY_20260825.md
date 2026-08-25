# HomeDS402Ex HOMEEX-12 WPF Recovery Qualification

Date: 2026-08-25

Branch: `codex/home-ds402ex-wpf-recovery`
Base: `dev@4283cdc6e05db70900a5a80e76a24d51ef8e9ccf`
Qualified head before this evidence-only commit: `0012f67f7b38e633421cf7f9cdf989cc3f6537f5`

## Scope

This checkpoint qualifies the WPF durable no-replay recovery tranche only. It does not qualify or activate the HomeDS402Ex LASAL runtime, Admin capability bit 11 advertisement, engineering-unit scale/profile Prepare, or hardware execution.

Implemented recovery contract:

- durable `AxisDs402HomeExRecoveryJournal` arm occurs before any future Start write boundary;
- reopening an unresolved `ArmedBeforeDispatch` record promotes it to `RecoveryRequired`;
- the durable identity includes endpoint, schema, original RequestId, DiagnosticsBuild, DiagnosticsBootId, MapRevision, 128-bit ClientIntentId, physical axis and every frozen execution-plan field;
- the journal has no Start sender and cannot manufacture a new prepared Start command;
- startup unresolved recovery interlocks ordinary mutation UI;
- recovery UI exposes capability refresh and `Query / Retire HomeEx Recovery (No Start Replay)` only;
- exact recovery requires endpoint and current Build/BootId/MapRevision identity match;
- `RecoveryRequired` may send only exact-key `0x7D1C` outcome query;
- a running outcome remains unresolved and does not retire or replay Start;
- terminal outcome proof is persisted durably before retirement;
- terminal proof requires safe cleanup proof flags, nonzero record generation and SDO executor token, zero native command state, and strict terminal tuple validation;
- `TerminalOutcomeObserved` may skip a new query after restart and reconstruct only the typed terminal retirement input from durable proof;
- retirement uses only exact key + exact nonzero record generation through `0x7D1D`;
- journal resolves only after successful exact-generation retirement proof;
- BootId or other recovery identity drift remains fail-closed and does not auto-resolve.

Persistence contract:

- canonical LF serialization;
- UTF-8 without BOM;
- SHA-256 integrity;
- bounded record size;
- temp file + durable `Flush(true)` + atomic replace/move;
- stale-copy/revision protection.

## CI evidence

GitHub Actions workflow: `HomeDS402Ex WPF recovery qualification`

Run: `32802902270`
Job: `97667184835`
Head: `0012f67f7b38e633421cf7f9cdf989cc3f6537f5`

Results:

- Debug WPF smoke build: PASS, 0 warnings / 0 errors;
- Debug HomeDS402Ex recovery smoke: 11 / 11 PASS;
- Release WPF smoke build: PASS, 0 warnings / 0 errors;
- Release HomeDS402Ex recovery smoke: 11 / 11 PASS;
- `git diff --check`: PASS.

The 11 tests cover:

- journal surface contains no replay path;
- startup `ArmedBeforeDispatch -> RecoveryRequired` promotion;
- durable terminal proof + exact retirement resolution;
- exact-key mismatch rejection;
- stale-copy rejection;
- terminal proof validation;
- journal integrity/corruption handling;
- startup unresolved-record global interlock;
- durable pre-dispatch arm with zero connection/wire activity;
- restart from durable terminal proof to retirement input without a new query;
- WPF recovery surface contains no HomeDS402Ex Start control.

Cross-workflow regression check on the same head:

- `SetOperationMode WPF recovery qualification` run `32802902209`: PASS.

## Decision

`HOMEEX-12`: **PASS / complete for software WPF recovery qualification**.

This is not permission to activate HomeDS402Ex. Production posture remains fail-closed because HOMEEX-01/02 and HOMEEX-06..11 are incomplete and Admin capability bit 11 remains unadvertised by the PLC. HOMEEX-13 remains blocked.
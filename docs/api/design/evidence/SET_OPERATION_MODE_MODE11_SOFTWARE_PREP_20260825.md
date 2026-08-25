# SetOperationMode MODE-11 Software/Bench Preparation Checkpoint

- Date: 2026-08-25
- Production branch: `dev`
- Production checkpoint: `7f00ce559e648a667a69fd68f9a1ea3290e72379`
- Qualification branch: `codex/setopmode-mode11-bench-activation`
- Qualification PR: #18 `DO NOT MERGE: MODE-11 bench activation tooling`
- Verified qualification head: `1caec1b32404afd5eb92cd77c588c80d06d25c69`
- Verified workflow: run `32794974820`, job `97644262203`
- Evidence grade: **software-side bench preparation only**
- MODE-11 hardware state: **NOT_RUN**
- Production activation: **KEEP_OFF**

## 1. Purpose

This checkpoint records the software-side preparation completed before SetOperationMode MODE-11 physical
qualification. It intentionally does not claim a fresh BENCH_ACTIVE C78 build, PLC load, physical drive effect,
EtherCAT/SDO packet proof, MODE-11 PASS, MODE-12 PASS or MODE-14 activation approval.

The production `dev` branch remains dormant:

- `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`
- Admin SetOperationMode capability bits 8/9/10 remain OFF
- generic D5 `0x6060` Write remains permanently denied

The qualification branch keeps those tracked production values OFF and activates them only in a clean local
working tree for the bench image.

## 2. Qualification-only activation transform

`Prepare-SetOperationModeMode11Bench.ps1` is restricted to the exact qualification branch and refuses a dirty
working tree before activation. The transform changes exactly two runtime-advertisement values:

1. `LMCDiagnosticsService.st`
   - `LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE -> TRUE`
2. `LMCControlCommandService.st`
   - Admin feature mask `0x00000017 -> 0x00000717`

`0x00000717` adds exactly SetOperationMode capability bits 8/9/10 as one Start / OutcomeRead / OutcomeRetire
triad. Physical-axis count remains 4 and ErrorCatalogVersion remains 6.

The same tool provides exact `-Revert` back to the tracked production-OFF values.

## 3. Candidate source safety contract

`Verify-SetOperationModeMode11Candidate.ps1` passed **33 checks** in both tracked `BASELINE_OFF` state and the
actual locally transformed `BENCH_ACTIVE` state.

The verified source contract includes:

- OwnerKind 6 and shared Diagnostics SDO ResourceKind 4;
- `0x7D23 / 0x7D24 / 0x7D25` Start / ReadOutcome / Retire routing;
- CSP=8-only validation at Start and recovery-key boundaries;
- exactly four physical-axis `0x6060` write fanout call sites;
- every dedicated `0x6060` mutation has `WriteLength=1`;
- main processor owns no `0x6060` write site;
- recovery owns no `0x6060` write and retains the explicit no-replay invariant;
- non-CSP mutation requires standstill, DS402 Fault clear and Operation Enabled clear;
- generic D5 still permanently denies `0x6060`.

MODE-11A same-mode source proof is pinned directly. In the exact `observedMode = 8` branch:

- WriteRequested evidence is set 0 times;
- WriteDispatched evidence is set 0 times;
- `0x6060` write dispatch is present 0 times;
- VerifyReadDispatched evidence is present;
- VerifyReadCompleted evidence is present;
- terminal success occurs before the non-CSP write-safety branch.

This is source evidence for the intended zero-write path. Actual MODE-11A still requires physical/packet proof.

## 4. Candidate identity capture

`Capture-SetOperationModeMode11BenchCheckpoint.ps1` is BENCH_ACTIVE-only and fail-closed. It reruns the candidate
source and define-order gates and records:

- qualification branch and Git HEAD;
- active source diff SHA-256;
- `LMCDiagnosticsService.st` and `LMCControlCommandService.st` SHA-256/bytes;
- `Classes.lcb` and project `.lcb` SHA-256/bytes/timestamps;
- optional fresh C78 build evidence;
- optional same-image nonzero DiagnosticsBuild / DiagnosticsBootId / MapRevision;
- endpoint and PLC load timestamp when supplied.

The generated checkpoint is explicitly stamped `PRE-HARDWARE / MODE-11 NOT YET PASSED`.

## 5. Durable WPF journal evidence extraction

The existing WPF SetOperationMode durable journal already preserves the exact recovery key, terminal outcome
proof and retirement request id. No WPF runtime change is required to extract those fields.

`Export-SetOperationModeMode11JournalEvidence.ps1` reads the journal without mutating it and validates:

- maximum 16 KiB journal size;
- strict UTF-8/canonical LF framing;
- final payload SHA-256 checksum;
- exact serialized field order;
- nonzero Build / BootId / MapRevision and durable request/intent identity;
- CSP requested mode and zero flags;
- terminal outcome state/generation/cycles;
- known EvidenceFlags only;
- OwnerReleased + ExecutorReusable terminal evidence;
- successful outcome exact observed mode, zero original status/error/detail and completed verify-read evidence;
- `Resolved` requires a nonzero retirement request id;
- `TerminalOutcomeObserved` must not claim retirement.

Self-tests accept valid resolved and terminal-before-retirement fixtures and reject checksum tamper, missing
owner/executor terminal evidence and an invalid resolved-without-retirement record.

## 6. Machine-checkable hardware evidence contract

`Verify-SetOperationModeMode11Evidence.ps1` validates the structured evidence sidecar after the real bench run.
Its self-test covers both positive and fail-closed cases.

Positive fixtures accepted:

- MODE-11A same-mode zero-write;
- MODE-11B exact one-write/readback.

Negative fixtures rejected:

- any MODE-11A `0x6060` write;
- Start replay;
- retirement generation mismatch;
- missing same-image BootId.

The final verifier also requires exact candidate hashes, nonzero same-image Build/BootId/Map, AxisReference 1,
C78/load evidence flags, one Start, no Start replay, terminal success, required EvidenceFlags, packet references,
exact-generation retirement and resolved WPF journal state. MODE-11B additionally requires an independently
approved non-CSP setup and all safe-state preconditions.

A verifier PASS only proves that supplied evidence is internally consistent. It does not authenticate a packet
capture or replace physical observation.

## 7. Windows qualification result

Workflow run `32794974820`, job `97644262203` completed **SUCCESS** at qualification head
`1caec1b32404afd5eb92cd77c588c80d06d25c69`.

Passed steps:

1. bench activation transform self-test;
2. durable WPF journal evidence exporter self-test;
3. machine-checkable hardware evidence verifier self-test;
4. baseline MODE-11 candidate safety contract;
5. actual local `BASELINE_OFF -> BENCH_ACTIVE` transform;
6. BENCH_ACTIVE candidate contract and candidate identity capture;
7. exact revert to `BASELINE_OFF` with tracked LASAL source diff 0;
8. SetOperationMode define-order gate;
9. diff hygiene.

This closes the software-side bench/evidence tooling gate only.

## 8. Remaining MODE-11 evidence

MODE-11 is not complete until the following external evidence is produced from the isolated BENCH_ACTIVE image:

1. fresh C78/ARM Rebuild/Link of the BENCH_ACTIVE working tree;
2. exact PLC load and same-image nonzero DiagnosticsBuild / DiagnosticsBootId / MapRevision;
3. axis-1 MODE-11A:
   - pre-read `0x6061 = 8`;
   - one `0x7D23` Start only;
   - terminal success through `0x7D24`;
   - zero `0x6060` writes in the available causal SDO evidence;
   - exact-generation `0x7D25` retire;
4. axis-1 MODE-11B using a separately approved non-CSP setup:
   - safe standstill / Fault clear / Operation Enabled clear context;
   - exactly one one-byte `0x6060:0 = 8` write;
   - `0x6061` verify readback = 8;
   - no replay after dispatch uncertainty;
   - exact-generation terminal retirement;
5. durable WPF journal export, TCP/SDO capture references and machine-checkable evidence verifier PASS.

Any uncertainty after write dispatch moves into MODE-12 recovery qualification; the original Start/write must not
be replayed.

## 9. Gate decision

- MODE-11 software/bench preparation: **PASS**
- Fresh BENCH_ACTIVE C78/ARM build/load: **NOT_RUN**
- MODE-11A physical/packet evidence: **NOT_RUN**
- MODE-11B physical/packet evidence: **NOT_RUN**
- MODE-11 overall: **NOT_PASSED**
- MODE-12: **NOT_RUN**
- MODE-14 activation: **KEEP_CLOSED**
- Production: **NO-GO for SetOperationMode activation**

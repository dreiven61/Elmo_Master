# SetOperationMode MODE-11 Bench Qualification Runbook

- Scope: axis 1 first
- Branch: `codex/setopmode-mode11-bench-activation`
- Production branch: `dev` remains activation OFF
- Requested mode: CSP `8` only
- Goal: prove same-mode no-write first, then exact one-write/readback from an independently approved non-CSP safe state
- Software-side bench-prep checkpoint: Windows CI validates BASELINE_OFF -> BENCH_ACTIVE -> candidate capture -> exact revert
- Hardware evidence tooling: durable WPF journal exporter + machine-checkable MODE-11A/B evidence verifier

## 1. Safety boundary

This branch is qualification-only. Do not merge activated LASAL source into `dev` before MODE-11 and MODE-12
hardware/packet evidence is reviewed and MODE-14 is approved.

The bench candidate changes exactly two runtime-advertisement values in the working tree:

1. `LMCDiagnosticsService.st`
   - `LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE -> TRUE`
2. `LMCControlCommandService.st`
   - Admin feature mask `0x00000017 -> 0x00000717`
   - added bits are exactly 8/9/10: Start / ReadOutcome / Retire

ErrorCatalogVersion remains 6 and physical-axis count remains 4.

Generic D5 Write remains permanently unable to write `0x6060`; MODE-11 must not bypass the dedicated
SetOperationMode owner/outcome lifecycle.

## 2. Prepare the local bench candidate

Start from a clean checkout of the qualification branch.

```powershell
git checkout codex/setopmode-mode11-bench-activation
git status --short
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-SetOperationModeMode11Bench.ps1 -Enable
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-SetOperationModeMode11Bench.ps1 -Verify
powershell -ExecutionPolicy Bypass -File .\tools\Verify-SetOperationModeMode11Candidate.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Verify-SetOperationModeDefineOrder.ps1
git diff --check
```

`-Enable` refuses a dirty working tree and refuses to run on any other branch. Expected source state after
activation is `BENCH_ACTIVE`: Diagnostics TRUE and Admin feature mask `0x00000717`.

Before IDE build, inspect `git diff`. The intended hand-authored activation delta is only the two values above.
Do not accept unrelated `.st` changes.

The branch workflow also exercises the real transform and exact revert on a Windows runner. A green workflow
means the preparation tooling is internally consistent; it is not C78/PLC/hardware evidence.

## 3. Fresh LASAL IDE build, load and candidate identity capture

1. Record the UTC build start time.
2. Fresh C78/ARM Rebuild/Link the qualification branch working tree.
3. If LASAL CodeGenerator moves or rewrites tracked source, stop and review before download.
4. Preserve the build-specific log if available.
5. Run `Capture-SetOperationModeC78Evidence.ps1` against the fresh build artifacts/log when the log is available.
6. Download/load the exact newly built candidate to the PLC.
7. Record same-image DiagnosticsBuild, DiagnosticsBootId and MapRevision from the WPF/API after load.
8. Capture the exact active-source/artifact tuple with `Capture-SetOperationModeMode11BenchCheckpoint.ps1`.
9. Commit the activated source plus newly generated `.lcb` artifacts to this qualification branch so the tested
   image can be tied to an exact Git commit. Do not merge that activation commit into `dev`.

Example after build/load when a build log is available:

```powershell
$buildStart = [datetime]::Parse('<UTC build start>')
powershell -ExecutionPolicy Bypass -File .\tools\Capture-SetOperationModeMode11BenchCheckpoint.ps1 `
  -OutputPath .\mode11-bench-candidate.md `
  -BuildLogPath '<LASAL build log path>' `
  -BuildStartedUtc $buildStart `
  -DiagnosticsBuild <nonzero build> `
  -DiagnosticsBootId <nonzero boot id> `
  -MapRevision <nonzero map revision> `
  -Endpoint '<ip:port>' `
  -PlcLoadTimestamp '<timestamp>'
```

If the build log is not exportable, omit `BuildLogPath` and `BuildStartedUtc`; the checkpoint will still record
active-source diff SHA-256, source SHA-256, generated artifact size/SHA-256 and any supplied same-image PLC
identity. It will remain explicitly `PRE-HARDWARE / MODE-11 NOT YET PASSED` until packet/hardware evidence is
completed.

## 4. MODE-11A — same-mode no-write test

Run this first. It is the lowest-risk mutation qualification because the runtime preflight reads `0x6061` and,
when the observed mode is already `8`, commits terminal success before entering the write-safety branch.

Preconditions:

- use physical axis 1 only;
- WPF connection and diagnostics/admin capabilities are freshly refreshed after PLC load;
- Admin capability triad bits 8/9/10 are all visible together;
- no unresolved SetOperationMode WPF recovery journal exists;
- no conflicting axis/SDO/maintenance mutation is active;
- read Drive Operation Mode and confirm `0x6061 = 8` before Start;
- use the explicit one-shot WPF Set CSP confirmation.

Expected evidence:

- one `0x7D23` Start request accepted once;
- terminal result obtained through `0x7D24`, not by replaying Start;
- `RecordState = Succeeded`;
- `ObservedModeRaw = 8`;
- `WriteRequested = 0` and `WriteDispatched = 0`;
- verify/read evidence is present;
- owner released + executor reusable evidence is present;
- final read of `0x6061` remains `8`;
- no `0x6060` write appears in the available EtherCAT/SDO packet evidence;
- exact terminal `RecordGeneration` is used for `0x7D25` Retire;
- retire succeeds and WPF durable journal resolves only afterwards.

A Start ACK alone is not a PASS.

## 5. MODE-11B — exact one-write/readback test

Do **not** manufacture the initial non-CSP state through generic D5 `0x6060`; that path is permanently denied by
design. Do not change the starting mode through an unreviewed PLC/drive write path merely to make this test pass.

Proceed only after an independently approved method exists to place axis 1 in a known non-CSP DS402 mode while
the drive is safe. Immediately before SetOperationMode Start, the dedicated runtime requires the non-CSP write
branch to have:

- valid physical startup context;
- no DS402 Home / Encoder Maintenance / competing SDO mutation;
- axis standstill evidence;
- DS402 Fault clear;
- DS402 Operation Enabled clear.

Expected mutation evidence after Start:

- preflight `0x6061` observes a non-8 mode;
- `WriteRequested = 1`;
- **exactly one** one-byte SDO write `0x6060:0 = 8` is dispatched;
- original `0x6060` write is never replayed after dispatch uncertainty;
- verify read `0x6061:0` returns `8`;
- terminal outcome is persisted/read through `0x7D24`;
- exact terminal generation is retired through `0x7D25`;
- final drive readback remains `8`.

If write dispatch becomes uncertain, stop the normal test and treat the case as MODE-12 recovery evidence. Never
send a second Start to repair an uncertain first Start.

## 6. Export the durable WPF outcome/retirement proof

The WPF durable journal already stores the exact recovery key, terminal proof and retirement request id. After a
run, export it read-only instead of manually copying terminal fields from the UI.

Default journal path:

`%LOCALAPPDATA%\Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-recovery.journal`

Example:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Export-SetOperationModeMode11JournalEvidence.ps1 `
  -OutputPath .\mode11a-wpf-journal.json
```

If WPF was launched with a custom journal root, pass the exact file with `-JournalPath`.

The exporter revalidates the journal framing, SHA-256 checksum, exact field order, Build/BootId/MapRevision,
terminal generation/cycles, owner-release/executor-reusable evidence and successful verify-read invariants. It
supports both `TerminalOutcomeObserved` and `Resolved`; only `Resolved` is emitted with retirement confirmed.
A checksum mismatch, weak terminal proof or inconsistent resolved state is rejected.

The exported `RunFragment` supplies the fields that can be proven from the durable WPF record: Start/Query/Retire
request IDs, ClientIntentId, terminal state/error/detail, EvidenceFlags, cycles, RecordGeneration and exact
retirement state. Packet counts, pre/post `0x6061`, actual `0x6060` packet count/payload and capture references
must still come from the live test evidence.

## 7. Build the machine-checkable MODE-11 evidence sidecar

Use `SET_OPERATION_MODE_MODE11_EVIDENCE_JSON.md` as the field contract. Combine:

1. candidate source/artifact/same-image identity from the bench checkpoint;
2. durable WPF outcome/retirement fields from the journal exporter;
3. actual TCP and EtherCAT/SDO packet observations;
4. pre/post `0x6061` reads;
5. for MODE-11B, the separately approved non-CSP setup and all safe-state preconditions.

Then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Verify-SetOperationModeMode11Evidence.ps1 `
  -EvidencePath .\mode11a.json
```

The verifier is fail-closed. Among other checks it rejects:

- any MODE-11A `0x6060` write;
- a MODE-11B write count other than exactly one or payload other than one-byte `08`;
- any Start replay;
- missing same-image Build/BootId/MapRevision;
- unknown or missing required EvidenceFlags;
- nonterminal/failed outcome;
- retire generation mismatch;
- unresolved retirement/journal state;
- missing packet references or unsafe MODE-11B preconditions.

A verifier PASS confirms that the supplied evidence is internally consistent with the frozen contract. It does
not authenticate a packet capture or substitute for the physical observation itself.

## 8. Evidence to preserve

For each run preserve:

- exact qualification branch commit SHA;
- BENCH_ACTIVE source diff SHA-256;
- `LMCDiagnosticsService.st` and `LMCControlCommandService.st` SHA-256;
- `Classes.lcb` and project `.lcb` SHA-256/bytes;
- build log / C78 target evidence if available;
- PLC load timestamp;
- DiagnosticsBuild / DiagnosticsBootId / MapRevision;
- endpoint IP/port and physical axis reference;
- original Start RequestId and ClientIntentId tuple;
- pre/post `0x6061` readback;
- Start ACK;
- full `0x7D24` terminal result including EvidenceFlags and RecordGeneration;
- `0x7D25` retirement result;
- exported WPF durable journal evidence JSON;
- TCP packet capture for `0x7D23/24/25` when available;
- EtherCAT/SDO evidence sufficient to prove zero writes for MODE-11A or exactly one `0x6060` write for MODE-11B;
- machine-checkable MODE-11 evidence JSON and verifier output;
- any fault, timeout, disconnect or quarantine observation without reinterpretation as success.

Use `SET_OPERATION_MODE_MODE11_EVIDENCE_TEMPLATE.md` for the human-readable final checkpoint. The generated
bench candidate identity checkpoint and machine-checkable JSON are supporting evidence; neither replaces the
actual terminal/packet evidence.

## 9. Stop conditions

Stop qualification and keep activation branch isolated if any of the following occurs:

- only one or two of Admin bits 8/9/10 are advertised;
- WPF recovery key Build/BootId/MapRevision does not match the loaded image;
- Start is replayed after an uncertain response;
- more than one `0x6060` write is observed for one intent;
- same-mode test causes any `0x6060` write;
- non-CSP write occurs while Operation Enabled, Faulted or not standstill;
- observed mode after verify is not exactly `8`;
- terminal owner-release/executor-reusable evidence is absent;
- retire generation does not exactly match the terminal generation;
- source/generated artifact identity cannot be tied to the tested PLC image.

## 10. Revert local activation

After bench work, or before returning to ordinary development:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-SetOperationModeMode11Bench.ps1 -Revert
powershell -ExecutionPolicy Bypass -File .\tools\Prepare-SetOperationModeMode11Bench.ps1 -Verify
```

Expected state is `BASELINE_OFF`: Diagnostics FALSE and Admin feature mask `0x00000017`.

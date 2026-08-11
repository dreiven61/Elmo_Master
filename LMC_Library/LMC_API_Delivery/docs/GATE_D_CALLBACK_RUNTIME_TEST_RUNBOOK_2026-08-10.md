# Gate D D5 callback runtime test runbook

Date: 2026-08-10

## Evidence boundary

Gate D adds a non-authoritative UDP wake for a terminal D5 operation. UDP never
completes an operation. Only the generation-pinned TCP `0x7E03` response may
update the retained ticket, UI, or journal.

On 2026-08-10 the LASAL IDE ran an incremental C78/ARM `Build project`. It
compiled the three changed classes and finished the internal link with zero
compiler errors. The source-warning histogram was `W0069=28`, `W0070=21`,
`W0072=11`. The first Download and PLC link reported `Download Ok`. A second
Download was aborted after a CPU-state timeout; a later connection succeeded
and the PLC reported `Project successfully loaded`. This is useful incremental
Build/Download evidence, but it is not a strict clean C78 Rebuild checkpoint and
it is not callback runtime proof.

The later LASAL PID 4832 session issued two `Rebuild project` commands. The first
is invalid because two `Classes.lcb` persistence error records report an
`ios_base::failure` and a write failure. The bounded second Rebuild window is a
clean C78/ARM source build with 76 coded warnings (`W0069=35`, `W0070=21`,
`W0072=17`, `W0073=3`), zero source errors, `Compiler Done`, `Linker Done`, and
zero `CInvalidArgException`. Its generated `Classes.lcb` is 8,549,773 bytes and
has SHA-256
`3AC3D938DC1520FAEA6C3693161ABDB280CC873A97C60CF79B3F716C7F064C22`.
The focused `VerifyCurrent` check exits zero and reports the actual tree as a
`CAPTURE TerminalWakeBrokerCandidate` static state. The bootstrap `ValidateOnly`
result at that historical stage was `UNTRUSTED` with `outputCreated=false`; that
run did not create a sequence-4 physical checkpoint or grant production approval.

PID 4832 is not the required isolated strict-build evidence session. It contains
the failed first Rebuild, the clean second Rebuild, and later Connect, Reset, and
Restart actions. There was no post-build `Find in Implementation` action and no
Download in this session. That Find action applies only to Object Network
Server/Client rows and is not applicable to ordinary class method rows, so its
absence is not an incomplete-method gate for the three Gate D methods.
Reset/Restart only ran the old PLC image. A live Gate D callback from that
artifact therefore remained untested.

The retained pre-commit strict checkpoint is the `GateDVisualLayout` PID 480 / Rebuild TID
3396 session. It records one canonical project load, exactly one C78/ARM
`Rebuild project`, no Connect or Download, and a normal project close/IDE exit.
The accepted command window has 76 coded warnings (`W0069=35`, `W0070=21`,
`W0072=17`, `W0073=3`), zero errors, `Compiler Done` twice, `Linker Done` once,
six post-result C82 compatibility warnings, and zero `CInvalidArgException`.
At that checkpoint identity, `VerifyBuild` reported
`C78/ARM errors=0 warnings=76 compilerDone=2 linkerDone=1
postResultCompatibilityWarnings=6/C82 profile=GateDVisualLayout
inputsEquivalent=true; rawInputsUnchanged=10/10 replayEquivalentSt=0
regeneratedOutputsBound=2 evidenceSource=bounded-repository`.

The retained baseline is 6,887 bytes with SHA-256
`247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE`.
Its exact raw range `[6532176,7298848)` is 766,672 bytes with SHA-256
`B918E51279360E27780D212650361AF361FFFC391C5F24854447BE0F3F9ABD17`;
the 1,574-byte sidecar manifest has SHA-256
`7928BC0D641FEA79444EDE8AD49FC10C15C28D453DB75DAF82C21B9D303D1DFC`.
The derived transcript is 30,111 bytes with SHA-256
`F32122D318DBFD8F53BC9E5AD0FF693F9B6F05368D40FC64138A010A1BC810AF`.
The Rebuild/checkpoint-bound `Classes.lcb` is 8,549,773 bytes with SHA-256
`24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`;
the second regenerated output, `Network/Networks.lcb`, is 242,363 bytes with
SHA-256
`C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F`.

PID 7288 and its D71E... `Classes.lcb` remain historical superseded evidence.
They must not be used as the current Gate D identity.

The checkpoint-focused verifier is 545,566 canonical-LF bytes with SHA-256
`FBF1A8582E85039377AC39F26D8BBA64C0EB62665424DE150083CFC412CC7CA3`.
The capture self-test passes positive `46` and negative `94`. The earlier
bootstrap `ValidateOnly` passed that tree as `UNTRUSTED` with
`outputCreated=false`; it planned
`gate_d_terminal_wake_broker_candidate_checkpoint.json` at 3,225,878 bytes and
SHA-256
`E0490DC348B861FBE47AB4C2E9C558BE679E865787A014860EBA45B3E0E508E4`.
That bootstrap run created no physical manifest.

The preceding verifier identity and the source-verifier `288/288` result are
historical sequence-4 checkpoint facts. The current portability ratchet pins the
focused verifier at 564,360 canonical-LF bytes and SHA-256
`20BDC1E49B3ED329143F0C36576F118F369383B3DA922069FDD2DD8B1909CC90`;
its Windows PowerShell 5.1 self-test rejects all `290/290` negative fixtures. A
clean detached worktree at `5543579`, populated with exactly the eight generated,
ignored Network artifacts, reproduced the focused `CAPTURE` state with exit `0`.
The general SourceOnly contract also passed there in 249.3 seconds. Generated
source/include and the derived Comm table remain limited to their exact pinned
LF or CRLF physical forms. The six protected Network text artifacts are the
only exception: bare CR is rejected, while LF, CRLF, or a mixture of the two is
compared through byte-level canonical LF. The verifier removes only `0x0D` from
`0x0D 0x0A`, preserves every high byte, and still requires the exact canonical
byte count and SHA-256. Other Network binary identities, topology, path
inventory, and counts stay strict. Gate D full and tracked raw Network
aggregates still accept only the pinned IDE-layout or clean-checkout count/SHA
tuple.

The checkpoint capture tool retains the historical sequence-4 tuple as
`HistoricalGateD` and freezes the distinct current pin. Its current self-test
passes positive `50` and negative `99`, and an actual sequence-4 manifest
revalidation passes. These support-tool and retained-evidence changes are kept
as a separate tooling/evidence changeset; they do not change production source
or approval. At that post-commit checkpoint the main worktree failed the formal
current gate because `Classes.lcb` was `6E115876...` instead of checkpoint
`24402BFA...`.

Trust-anchor commit `bb5fd93` was followed by commit `5543579`, which atomically
committed the sequence-4 physical manifest plus the exact seven production paths
listed below. The manifest binds `Classes.lcb` to `24402BFA...` and records
`ProductionApproved=false`, `NeedsRebaseline=true`.

After `5543579`, LASAL PID 34656 ran C78/ARM `Rebuild project`, compiled
`LMCDiagnosticsService`, `LMCUdpCallbackSender`, and `TCPMotionInterface`, and
reported `Compiler Done`, `Linker Done`, and command success. Download then
reported `Download Ok` and `Project successfully loaded`. A later Reset/Restart
also succeeded and loaded the project again. These are IDE/online-operation facts,
not callback causal proof.

That Rebuild regenerated the then-current `Classes.lcb` as 8,549,773 bytes with SHA-256
`6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712`,
which differs from the sequence-4 checkpoint `24402BFA...`. Current focused
`VerifyCurrent` and C78 input-equivalence checks therefore fail. The exact diff
is 99 bytes in 58 contiguous runs across 36 opaque vendor class records. The
four Gate D class records and protected dependency records are byte-exact, but
the changed vendor fields and generator checksum semantics remain undefined.
Do not normalize, allowlist, or pin `6E115876...` as semantically equivalent
from this evidence. All callback observations against this artifact are
exploratory until the checkpoint identity is reproduced or a separate reviewed
strict-evidence transition accepts the regenerated artifact; the decision
remains `ProductionApproved=false` and `NeedsRebaseline=true`.

Commit `7038445` preserves the `6E115876...`-start build baseline and the exact
reversible `24402BFA...` to `6E115876...` binary patch without changing
production source. Commit `79f03d36f89c34b26325666a4a3eddb9306c4674` adds the
fail-closed comparator at
`test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Compare-LasalClassesArtifact.ps1`.
The script is 79,592 physical bytes with SHA-256
`B91BFB5AFE131F0ECB3F23DC00373BEC7FC91B2C37CF626D128E912F633EBBA4`;
Windows PowerShell 5.1 and PowerShell 7 AST checks pass, and its self-test passes
positive `6` / negative `14`. Real Windows PowerShell 5.1 and PowerShell 7 runs
against checkpoint commit `5543579` and the then-current `6E115876...` candidate produced identical
51,102-byte stdout with SHA-256
`9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639`.
Commit `2e8ca8a84a141390424ce859ac8c315a90ec3430` preserves that exact CreateNew
comparison JSON as
`classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json`.

The comparator exits `2` with
`REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT`: 99 changed bytes, 58 contiguous runs,
and 36 changed opaque vendor owners. The 120-record inventory, all four Gate D
target records, and both protected dependency records are byte-exact, with zero
unmapped runs. These bounded equalities do not approve the whole artifact. The
recorded decision remains `ProductionApproved=false` and
`SemanticEquivalenceProven=false`.

Local refs
`refs/codex/evidence/gate-d-classes-2fae-20260810`,
`refs/codex/evidence/gate-d-classes-d71e-20260810`, and
`refs/codex/evidence/gate-d-classes-6e115876-20260810` only protect recovered
Git objects from local garbage collection. They are neither repository evidence
nor a formal checkpoint, transition, or approval.

PC reconnect correction commit `66b5cf2` preserves the exact short-failure
`ErrorId=-1` and only for the canonical v2 failure envelope waits 20 ms and
retries `0x8080` once
on the same socket. Legacy and other failures do not retry. Commit `af4ab63`
also fixes the non-canonical short-ACK `ErrorId=0` case at one `0x8080`, full
listener/TCP/WPF cleanup, and a fresh socket for the next manual Connect. Current
Release PC evidence is SDK `1133/1133` and WPF `335/335`. The GUI retains the RPC-init
attempt count, canonical-retry decision, and final ACK evidence after cleanup,
labels the configured tuple `RequestedCallback`, records the actual UDP endpoint
as `BoundCallback` or `not-bound`, and displays the accepted version-2 BootId,
SessionEpoch, cookie, listener
generation, expected source, event mask, PC receiver counters, and last receiver
decision. The WPF total includes a deterministic old-session statistics action
queued across connection replacement; it cannot alter the replacement owner,
counters, last decision, or listener summary. These values are PC-side evidence
only; they do not replace the pcap,
PLC `RpcCallbackLastDisarmResult`, or PLC producer/sender counters. Negative PLC
disarm preservation remains intentional and fail-closed; do not force-clear the
callback tuple.

Before formal Gate D runtime qualification, preserve this sequence/evidence split:

1. PID 480 / TID 3396 has supplied the `GateDVisualLayout` one-Rebuild raw log,
   and the exact
   bounded delta retained in the repository has passed `VerifyBuild`. The
   mutable local log is no longer required to replay this build evidence. Do not
   rerun or alter this historical PID 480 checkpoint. This restriction does not
   prohibit the separate, new isolated artifact-classification Rebuild in item
   5.
2. PID 480 contains no method-specific UI proof; that remains a fact about the
   isolated Rebuild session. `Find in Implementation` applies
   only to Object Network Server/Client rows and is not applicable to these class
   method rows. The user separately attested that the row-level Find action works
   normally; that does not prove a method row was opened. For a method row, use
   `Edit Method`, Enter, or a direct open and confirm the exact Implementation
   tab/header. The user has contemporaneously
   attested that `LMCDiagnosticsService::TryTakeD5TerminalWake`,
   `LMCUdpCallbackSender::PublishEvent`, and
   `TCPMotionInterface::PublishD5TerminalWake` each opened with the correct
   Implementation display and that LASAL was then closed. Record this UI check as
   `exactMethodOpen=manual-attested`; do not ask the user to repeat it merely
   because the Rebuild session has no Find action. `Lasal2.log` records only a
   class-level Open Implementation token, which can also result from automatic
   session restore, and cannot prove the selected method. A separate automated
   method-smoke JSON/log result remains pending and nonblocking; it must be
   labeled as automated evidence, not as a prerequisite that invalidates the
   manual attestation.
3. Completed: `bb5fd93` froze the reviewed trust-anchor tools and `5543579`
   committed the trusted sequence-4 physical manifest atomically with these
   exact seven production transition paths:
   `Class/Classes.lcb`,
   `Class/LMCDiagnosticsService/LMCDiagnosticsService.st`,
   `Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st`,
   `Class/TCPMotionInterface/TCPMotionInterface.st`,
   `Class/_UDPTransceiver/_UDPTransceiver.st`,
   `Network/Comm_Network/Comm_Network.lcn`, and `Network/Networks.lcb`.
4. Completed but not identity-equivalent: PID 34656 performed the post-commit
   Rebuild/Download and later Reset/Restart, while that Rebuild changed
   `Classes.lcb` from manifest identity `24402BFA...` to `6E115876...`. Do not
   rerun Download merely to repeat this step, and do not rebaseline the opaque
   99-byte vendor-record drift by hash alone. First reproduce the checkpoint
   identity or complete a separate reviewed strict-evidence transition; then
   collect fresh BootId, counter deltas, WPF log, and packet trace for formal
   qualification.
5. The required new isolated artifact-classification session has completed from
   a new LASAL process and the canonical `.lcp`. Its bounded log contains one
   successful `Rebuild project`, normal close/exit, and no Connect or Download.
   The frozen inputs are `Lasal2.log` 9,554,717 bytes / SHA-256
   `25F6A3FA913FD2BF57117C19D0C4489399F5A4FD296CF86C1508AEA07BA02A8C`,
   `Classes.lcb` 8,549,773 bytes / SHA-256
   `99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD`, and
   `Networks.lcb` 242,363 bytes / SHA-256
   `C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F`;
   LASAL process count was zero before finalization. This is a third Classes hash.
   Do not repeat this Rebuild or the already completed manual exact-method smoke.
   The run classifies generator output only; it does not replace PID 480 and is
   not PLC runtime proof.
6. The finalizer originated at `111a773`; the revision that produced the
   now-committed bundle was `fa2a456` of
   `Finalize-LasalClassesRebuildCandidate.ps1`, physical 187,443
   bytes / SHA-256
   `1551A121D49C3C3169B0DADA45B4EEAAFDD8F8636425E470D1A6840159CBC0D5`
   (Git blob `5495e5636462d8aa67e13abb70c310a1ee8f9e67`). Its historical
   PowerShell 7 self-test was positive `26` / negative `76`; Windows PowerShell
   5.1 AST/self-test was positive `24` / negative `74`. The published manifest
   intentionally retains that producer tuple.

   Commit `29811c4` is the current future-run exit-code fix: physical 188,693
   bytes / SHA-256
   `817E1A416C1484E1AE897140B2C56D8A7DDDF1F4158AC7DED2B59F28C5050116`,
   with PowerShell 7 self-test positive `27` / negative `77`. It sends the
   status line directly to `Console.Out`, requires exactly one `System.Int32`
   result in `{0,2,3}`, and therefore preserves exit `3` at process level. It
   is for a future candidate only: do not rerun it against the `b2019db`
   bundle. Production `-FinalizeCandidate` remains PowerShell 7-only because
   the publication contract includes directory ADS evidence. The exact future-run
   command from the canonical repository root is:

   ```powershell
   & pwsh.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Finalize-LasalClassesRebuildCandidate.ps1 -FinalizeCandidate -RepositoryRoot (Get-Location).Path
   $LASTEXITCODE
   ```

   Exit `0` means exact checkpoint `24402BFA...` and permits static checkpoint
   replay only. Exit `2` means the known `6E115876...` result is reproducible and
   requires review only. Exit `3` means a third hash and unstable generator output:
   stop. Exit `4` means blocked/no accepted publication. Every outcome remains
   `ProductionApproved=false` and
   `onlineRuntimeQualificationPermitted=false`; perform no Download from this
   classification.
   The finalizer may accept at most one exact load-only `E0015` record for
   `DriveComL2.h`. Any other error, or an additional error record, stops
   classification; do not reinterpret it as warning debt.
   The first real third-hash production invocation used the pre-`fa2a456`
   finalizer and reached the atomic-publish named-identity recheck, then exited
   `4` with `The property 'Value' cannot be found on this object.` The cause was
   the finalizer reading an `OrderedDictionary` key through
   `PSObject.Properties[...].Value`. It published no bundle and its exact-owned
   staging directory was cleaned. Commit `fa2a456` adds exact-case
   `IDictionary`/`PSCustomObject` access and production-shape regression tests.
   This failed attempt is not an accepted exit `3` result. Because the frozen log
   and generated outputs were unchanged, one finalizer-only rerun was permitted
   without repeating the isolated Rebuild. That `fa2a456` rerun published the
   bundle with manifest disposition `UNSTABLE_THIRD_CLASSES_HASH_STOP` and
   manifest exit `3`, but the old `Write-Output` status line joined the returned
   integer on the success pipeline and the host process incorrectly returned
   `0`. Commit `29811c4` fixes that future process-exit bug; it does not change or
   republish the historical bundle. No further finalizer or Rebuild run is
   permitted for this evidence.
7. Commit `b2019db` atomically preserves the exact published directory
   `candidate_finalization_gate_d_rebaseline_6e115876`. Freeze that directory.
   Do not delete or overwrite any member and do not rerun the finalizer. The exact
   eight-file inventory is:

   - `.finalizer-owner.json`
   - `Classes.post-rebuild.snapshot.lcb`
   - `Networks.post-rebuild.snapshot.lcb`
   - `derived_build_transcript_gate_d_rebaseline_6e115876.txt`
   - `bounded_lasal2_delta_gate_d_rebaseline_6e115876.raw.txt`
   - `bounded_lasal2_delta_gate_d_rebaseline_6e115876.manifest.json`
   - `classes_lcb_gate_d_rebuild_candidate.comparison.json`
   - `classes_lcb_gate_d_rebuild_candidate.finalization.json`

   The complete manifest records `Classes.lcb=99014DD9...`, unchanged
   `Networks.lcb=C307547E...`, finalizer exit `3`,
   `ProductionApproved=false`, `staticReplayPermitted=false`, and
   `onlineRuntimeQualificationPermitted=false`. Its checkpoint comparison records
   `96` changed bytes, `52` contiguous runs, `34` changed opaque owners, zero
   unmapped runs, and exact equality for all four Gate D target records and both
   protected dependency records. That bounded equality is not semantic
   equivalence and does not permit Download or hash-only rebaseline.

8. The fail-closed bundle validator originated at `531abdd`; its current commit is
   `c48e403`
   (`Verify-LasalClassesRebuildFinalizationBundle.ps1`), physical 189,867 bytes /
   SHA-256
   `DB8B046DF00900140E1AB97B83EF1E7AD13EFB44AC2768EE54B219160D8CE6B0`.
   PowerShell 7 self-test passes positive `5` / negative `32`.
   Windows PowerShell 5.1 AST passes, but production verification exits `4`
   before reading any bundle evidence. Production verification is PowerShell
   7-only. From the canonical repository root, run exactly:

   ```powershell
   & pwsh.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Verify-LasalClassesRebuildFinalizationBundle.ps1 -VerifyBundle -RepositoryRoot (Get-Location).Path
   $LASTEXITCODE
   ```

   The earlier validator first rejected the two legitimate repeated
   `Open Network Editor for 'Comm_Network'` restoration records by command text;
   `29811c4` binds them by distinct raw `commandLineIndex` values instead. It then
   rejected the historical converter and mixed-EOL C78 verifier physical tuples
   against their canonical Git blobs; `c48e403` adds exact path/physical
   bytes/SHA/blob-OID/canonical-LF dual-tuple bridges for only those two files,
   without broad EOL relaxation. The bundle stayed byte-unchanged through both
   validator fixes. The current validator returned exit `0` on the committed
   `b2019db` bundle.

   Validator exit `0` proves only the current eight-file bundle integrity and
   cross-file contract. It does not approve production, prove a past atomic move
   or written-last ordering, or prove PLC/runtime behavior. On validator failure,
   preserve the bundle unchanged and stop. Only after validator exit `0` may the
   exact whole bundle be staged and committed atomically in one Git commit.

   | Finalizer result | Required decision after bundle-integrity PASS |
   | --- | --- |
   | `0` | Exact static replay only; complete a separate review before any future approval. |
   | `2` | Preserve and review vendor semantics; hash-only rebaseline is forbidden. |
   | `3` | Stop: unstable third hash. |
   | `4` | Stop: blocked/no accepted publication, so there is no new bundle to commit. |

   Every row remains no-Download, `ProductionApproved=false`, and
   `onlineRuntimeQualificationPermitted=false`. Validator exit `0` never changes
   the finalizer classification or these decision flags.

9. Commit `998e7132c0892788db79a0868c5b129fb20edd96` adds the pinned historical
   triad analyzer
   `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Compare-LasalClassesVolatilityTriad.ps1`,
   physical 139,073 bytes / SHA-256
   `E3E2C586C62379339EECFD8038189D9959C655CD206A4E894B846A2D79783663` /
   Git blob `a7dd4dba67e30c4adc80549a1d9b6a4d1acb6bce`. PowerShell 7 self-test
   passes positive `7` / negative `16`; Windows PowerShell 5.1 passes positive
   `3` / negative `2` by delegating the analysis core to PowerShell 7. For an
   explicit read-only stdout replay, run only:

   ```powershell
   pwsh -NoProfile -File '.\test\Reports_Lasal\C78_20260810_udp_callback_gate_d_rebaseline_6e115876\Compare-LasalClassesVolatilityTriad.ps1' -AnalyzePinnedTriad
   ```

   The report is already committed; the command above intentionally produces
   stdout only and is not permission to recreate or overwrite it. Commit
   `e7c812ad7cfc6ef2162ed1197dc615e2aebe45db` preserves exact report
   `classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_99014dd9.volatility.json`,
   schema `LasalClassesVolatilityTriadEvidence/v1`, physical 29,412 bytes /
   SHA-256
   `09C76BB3BC313642C3012A915C14C022EDF75965A8A431B87F26B463005489DC` /
   Git blob `3c4411e26493043b80828a5355bdc8b621457e09`.

   The analyzer's diagnostic exit `2` compares only pinned A/B/C identities
   `24402BFA...` / `6E115876...` / `99014DD9...`. Pairwise changed
   byte/run/owner counts are respectively `99/58/36`, `96/52/34`, and
   `105/61/36`. Across `157` structural candidates, `66` observed volatile
   16-bit slots and `91` stable candidates were found. All changed offsets map
   to two fixed 16-bit slot families: `35` marker-followers and `31` owner-end
   minus 48 slots. The candidate table SHA-256 is
   `AD8A7FC5D6CB2277819FF28A7B7994C0FD6EAFBE6940419159662B8EFE83924D`;
   the volatile-slot table SHA-256 is
   `9D12A54145C409AC257F011C88F782108BCB3D73E9EDCCD8D2653A387F0F193C`.
   This proves a fixed slot structure only. Field meaning remains
   `UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT`, and repeatability of
   `99014DD9...` is not proven.

   The six implicit inputs LASAL executable, LASAL compiler, vendor library set,
   generator cache state, filesystem timestamps, and process session state are
   all `UNPROVEN`; `allGeneratorInputsEquivalent=false`. Report publication is
   explicitly bounded to `NON_ADVERSARIAL_WORKSPACE` with
   `handleRelativeCreationUsed=false` and
   `concurrentParentReplacementResistance=false`. Therefore the triad keeps
   `ProductionApproved=false`, `SemanticEquivalenceProven=false`,
   `requiresReviewedTransition=true`, `rebaselinePermitted=false`, and no Download,
   runtime qualification, future-artifact acceptance, normalization decision, or
   hash-only rebaseline. It does not change focused/C78 failure or the finalizer
   `UNSTABLE_THIRD_CLASSES_HASH_STOP` exit `3` STOP.

10. Commit `731a01e428bdc9282edbf727f1d76a7a63cd24a3` adds the pinned historical
    slot-corpus analyzer
    `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Analyze-LasalClassesHistoricalSlotCorpus.ps1`,
    physical 156,472 bytes / SHA-256
    `90BDD86EFC9C5032788C2603755A3560CC2871672E638935C4CD955B705EA080` /
    Git blob `30379ef2a50e093bbb28d768b9df77d091199de6`. PowerShell 7 self-test
    passes positive `12` / negative `18`; Windows PowerShell 5.1 passes positive
    `5` / negative `1` with delegated analysis. Evidence commit
    `43a85319905fbb5a42418b4b1ef9cd364c0bf44d` preserves exact report
    `classes_lcb_historical_slot_corpus_bd9dcb0c_55435791_99014dd9_6e115876.analysis.json`,
    schema `LasalClassesHistoricalSlotCorpusEvidence/v1`, physical 157,999 bytes /
    SHA-256
    `F306022CECD6C71BB7EA2B3DF309556A2621821B6C2CD287BC3FFFF4FA5A1B6A` /
    Git blob `edad859d03ac0c33f21ca42996a377dda3ee7b79`.

    The canonical selector is bounded to the first-parent, oldest-to-newest
    history of
    `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb` through anchor
    `55435791f6e91c9dcb4e06dcd25a11d77b382da7`. It contains `22` occurrences,
    `20` unique artifacts, and `9` ordered owner/source-path topologies. The
    canonical-history, history-plus-C, and history-plus-C-plus-B layers contain
    `20`, `21`, and `22` unique artifacts. C is read only from the committed
    `b2019db3af5a9990d2e0fe0afd0f02cbfbfaff53` bundle snapshot; B is reconstructed
    from the committed oracle. The analyzer does not read mutable current
    `Classes.lcb` or require the local `bd47dd96...` object.

    The full history-plus-C-plus-B layer contains `2,501` records and `814`
    marker samples across the same `9` topologies. With exactly the current
    target 16-bit word zeroed for a diagnostic context key, while the other
    target family remains unmasked, tail contexts have `87` varying groups /
    `227` samples / `31` owners / `202` unequal pairs; marker contexts have `95`
    varying groups / `282` samples / `34` owners / `369` unequal pairs. This
    target-word masking is diagnostic only and is not an acceptance rule.

    Across the `21` occurrence-preserving adjacent mainline transitions, `2,378`
    common owner records partition exactly into `1,155` raw-identical records,
    `538` candidate-only changes, and `685` outside-target changes; `18` owners
    were added and `2` removed. The `538` candidate-only changes partition into
    tail-only `55`, marker-only `97`, and both-family `386` records.

    Exact-other-bytes counterexamples refute `20` bounded hypotheses only in the
    declared scope: fixed stateless functions of the target-zeroed record-local
    input. They do not refute an artifact-hash seed, timestamp/session/filesystem
    input, or LASAL internal state. `fieldMeaning` remains
    `UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT`.

    Publication remains bounded to `NON_ADVERSARIAL_WORKSPACE` with
    `handleRelativeCreationUsed=false` and
    `concurrentParentReplacementResistance=false`. The historical diagnostic
    therefore keeps `ProductionApproved=false`,
    `SemanticEquivalenceProven=false`, `rebaselinePermitted=false`,
    `downloadPermitted=false`, `runtimeQualificationPermitted=false`, and
    `requiresReviewedTransition=true`. It does not authorize normalization,
    future-artifact acceptance, another Rebuild, or Download. Focused/C78 remains
    failed and finalizer `UNSTABLE_THIRD_CLASSES_HASH_STOP` exit `3` remains the
    controlling STOP.

The retained pre-drift C78 evidence was replayed from the canonical repository
root with:

```powershell
& 'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalC78RebuildEvidence.ps1' `
  -VerifyBuild `
  -EvidenceProfile GateDVisualLayout `
  -RepositoryRoot (Get-Location).Path `
  -EvidencePath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/build_baseline_gate_d_visual_layout.json' `
  -BuildTranscriptPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/derived_build_transcript_gate_d_visual_layout.txt' `
  -BoundedLogDeltaPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/bounded_lasal2_delta_gate_d_visual_layout.raw.txt' `
  -BoundedLogDeltaManifestPath 'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/bounded_lasal2_delta_gate_d_visual_layout.manifest.json'
```

At the checkpoint identity, the exact command above was rerun with
`-RunFullStatic` and exited `0` in 247.8 seconds. A current rerun is expected to
fail input equivalence until `Classes.lcb` is rebaselined. The historical passing
run emitted both
`PASS LASAL.StaticContract (Phase5TransportClean; ... diagnostics D1-D5 ...)`
and
`PASS LASAL.C78RebuildEvidence.Verify ... profile=GateDVisualLayout
inputsEquivalent=true; rawInputsUnchanged=10/10 replayEquivalentSt=0
regeneratedOutputsBound=2 evidenceSource=bounded-repository`.

The first full-static attempt exposed a local-scope defect in the verification
tool, not a production-source failure: `$stage87AdapterCallPattern` was defined
in the wrong function-local scope in `Verify-LasalContract.ps1`. The definition
was moved into `Assert-LasalDs402OwnerReceiptProviderMutationFences`, after which
Windows PowerShell 5.1 and PowerShell 7 AST checks and strict self-test `67/67`
passed before the successful rerun.

Any callback run while current `Classes.lcb` remains outside the sequence-4
identity is exploratory evidence and must be labelled as such. The current
decision remains `ProductionApproved=false` and `NeedsRebaseline=true`.

Those decision flags are release/evidence metadata, not a runtime feature gate.
The broker is executable behind its session/BootId fences after Download, and
the WPF application activates version 2 only through explicit opt-in.

Keep these evidence classes separate:

- `Actual PLC`: packet and counter evidence from the downloaded PLC;
- `Hybrid`: an actual PLC packet deliberately dropped, delayed, duplicated, or
  reordered by an approved network proxy;
- `PC fake`: codec, fence, WPF, or fake-RPC/fake-UDP evidence with no PLC
  producer;
- `Static/IDE`: source, verifier, C78 Build/Rebuild, or generated-file evidence.

The repository has PC fake-peer tests. It does not provide a production-network
packet injector. Do not describe fake-peer results as PLC runtime proof.

### PC-only callback ownership wire harness

Commit `bff3bc7` adds the exact test-runner mode `callback-ownership-wire`. It is
a PC raw-wire harness, defaults to dry-run, retries zero times, and has no input
surface for arbitrary commands or payloads, downgrade, write, motion, reset, or
Download. Its request allowlist is only exact `0x8080`, fixed version-2 `0x405C`
(mask `1`, maximum `52`, nonzero cookie, flags/reserved zero), and `0x405D` from
the current authoritative owner. The 16 new harness tests bring the current
Release SDK result to `1133/1133`; an independent reviewer repeated the Release
`RunPcTests` target with the same result and repeated Release `RunWpfSmokeTests`
at `335/335`.

Before reviewed rebaseline, the following are the only authorized harness
commands. They are exact **DRY-RUN** examples and open no network connection:

```powershell
& '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe' callback-ownership-wire
& '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe' callback-ownership-wire --dry-run --scenario gd-n10a
& '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe' callback-ownership-wire --dry-run --scenario gd-n13-candidate
& '.\LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\bin\Release\LasalMotionControlLib.Tests.exe' callback-ownership-wire --dry-run --scenario gd-n14-candidate
```

No actual live command is provided or authorized here. A future reviewed live
invocation must abstractly satisfy all of these fail-closed guards before any
network access: exact `--execute-live`; exact case-sensitive confirmation
`--confirm PLC-CALLBACK-OWNERSHIP`; one concrete `--scenario` (never `all`); explicit PLC,
owner-local, and candidate-local IPv4 values through `--host`, `--owner-local`,
and `--candidate-local`; a required declared `--source-fingerprint`
`HEAD/TRACKED/UNTRACKED` whose three Git object hashes are each 40 or 64
hexadecimal characters; and a new `--output` path that does not
already exist. Unspecified/broadcast IPv4 is prohibited. `gd-n13-candidate`
requires identical owner/candidate source IPv4; `gd-n10a` and
`gd-n14-candidate` require different source IPv4, and GD-N10A requires candidate
`--candidate-callback-port 0` because its mismatch reuses the actual owner UDP endpoint.
The optional `--port` defaults to `4000`, `--owner-callback-port` and
`--candidate-callback-port` to `0`, and `--timeout-ms` to `3000` with an allowed
range of `250..10000` ms. The tool syntax-validates
and records the declared source fingerprint; it does not independently prove
the worktree identity, downloaded PLC image, or peer identity.

The report starts with `FORMAT=LMC_CALLBACK_OWNERSHIP_WIRE_V1`, mode/scenario,
`EVIDENCE_CLASS=PC_RAW_WIRE_HARNESS`, `PEER_IDENTITY=UNVERIFIED`, executable
path/SHA-256, Git HEAD/checkpoint identity, declared source fingerprint,
endpoints, timeout, and `RETRY_COUNT=0`. Each request/response records byte
length, SHA-256, and hex. It explicitly records
`PCAP_EVIDENCE=NOT_CAPTURED_BY_TOOL`,
`PLC_WATCH_EVIDENCE=NOT_CAPTURED_BY_TOOL`,
`QUALIFICATION_COMPLETE=FALSE`, and
`QUALIFICATION_RESULT=INCOMPLETE_WITHOUT_PCAP_AND_PLC_WATCH`, followed by the
scenario result and any exception. A new `.inprogress-<GUID>.tmp` report is
reserved before network access and moved to the requested new file at the end;
an existing target is never overwritten, and FAIL/INCONCLUSIVE evidence is
preserved when finalization succeeds.

Tool PASS proves only that this PC client observed the expected raw exchange. It
never equals PLC qualification. Reviewed rebaseline and an exact downloaded
checkpoint, a site-approved maintenance window, correlated pcapng, PLC Online
Watch counters, and the remaining case evidence in this runbook are still
required before any PLC-runtime conclusion.

## Safe test setup

Before any Download or takeover case, obtain a site-approved maintenance window.
All axes, coordinated groups, and the robot must be idle and powered off, with no
active or queued motion/diagnostics command, recorder transfer, recovery action,
or safety-drain operation. The single-axis Standstill check below is additional
read-only test setup; it does not replace this system-wide prerequisite.

1. Use the WPF test application built from the source under qualification. Its
   connection explicitly requests callback version 2 with event mask bit `1`
   and maximum datagram size `52`.
2. Select an axis whose drive is powered off and in stable Standstill. Record
   the read-only status evidence used for this check. Do not use SDO Write for
   any test in this runbook.
3. In the low-level SDO panel select `Read`, Slave `1..4`, index `0x6061`,
   sub-index `0`, `Int8`, length `1`, timeout `1000`, and use only
   `Submit SDO Read` for the causal callback cases. Do not use
   `Read SDO Inline (wait terminal)` in those cases: that helper polls by itself
   and cannot prove the UDP-to-TCP causal query.
4. Start a packet capture for the control TCP connection and the advertised UDP
   callback port. Preserve the raw pcapng and the WPF execution log. If a host
   firewall drops UDP, note that a host-side capture may still show the incoming
   datagram; prove the drop by the receiver counters, lack of WPF dispatch, and
   lack of an automatic `0x7E03`.
5. After Connect, confirm `Connected` and
   `Listening <endpoint>, rejected=<count>`. Record the GUI RPC-init evidence
   (`0x8080Attempts`, `Retry`, and retained `LastACK`) and version-2 registration
   evidence (BootId, SessionEpoch, cookie, listener generation, expected source,
   and event mask). In the capture, still verify the exact 32-byte version-2
   `0x405C` request and successful 20-byte response, including nonzero accepted
   Diagnostics BootId and SessionEpoch. The GUI values are parsed PC evidence
   and do not prove the accepted PLC wire exchange by themselves.
6. Record all counters as before/after deltas. Do not reset or force private PLC
   variables. When LASAL Online watch is available, include:
   - `TCPMotionInterface1.SessionEpoch`
   - `TCPMotionInterface1.PendingClosedSessionEpoch`
   - `TCPMotionInterface1.RpcCallbackRegistered`
   - `TCPMotionInterface1.RpcCallbackProtocolVersion`
   - `TCPMotionInterface1.RpcCallbackSessionEpoch`
   - `TCPMotionInterface1.RpcCallbackBootId`
   - `TCPMotionInterface1.RpcCallbackLastDisarmResult`
   - `TCPMotionInterface1.D5TerminalWakeAttemptCount`
   - `TCPMotionInterface1.D5TerminalWakeEnqueuedCount`
   - `TCPMotionInterface1.D5TerminalWakeRejectedCount`
   - `TCPMotionInterface1.TakeoverCount`
   - `TCPMotionInterface1.TakeoverRejectCount`
   - `TCPMotionInterface1.LastTakeoverResult`
   - `LMCUdpCallbackSender1.QueueDepth`
   - `LMCUdpCallbackSender1.QueuedCount`
   - `LMCUdpCallbackSender1.RingAcceptedCount`
   - `LMCUdpCallbackSender1.AdmissionRetryCount`
   - `LMCUdpCallbackSender1.QueueFullDropCount`
   - `LMCUdpCallbackSender1.AdmissionErrorDropCount`
   - `LMCUdpCallbackSender1.DisarmClearedCount`
   - `LMCUdpCallbackSender1.TransportErrorCount`
   - `LMCUdpCallbackSender1.LastAdmissionResult`
7. The WPF callback diagnostics expose `AcceptedCallbackWakeHintCount`,
   `RejectedCallbackCount`, `DuplicateCallbackWakeHintCount`,
   `OutOfOrderCallbackWakeHintCount`, and the last receiver decision/protocol
   error. Record their before/after values. They are PC receiver evidence and do
   not replace the pcap or PLC producer/sender counters. A WPF semantic drop
   after a valid envelope is not added to `RejectedCallbackCount`.

For a bounded interval in which none of the three producer counters saturates,
the deltas must satisfy:

```text
delta(D5TerminalWakeAttemptCount)
  = delta(D5TerminalWakeEnqueuedCount)
  + delta(D5TerminalWakeRejectedCount)
```

`RingAcceptedCount` proves vendor-ring admission, not an emitted network packet.
Only the packet capture can supply that wire evidence.

## P0: actual PLC cases

### GD-01 normal Completed wake

1. Connect the WPF application and complete the registration checks above.
2. Enter the read-only `0x6061:0 Int8/1`, timeout `1000` request and press
   `Submit SDO Read` once.
3. Do not press `Refresh Ticket` or `Read SDO Inline (wait terminal)` while
   waiting.

PASS requires all of the following:

- the submit returns one nonzero D5 TicketId;
- the PLC-origin capture contains exactly one valid 52-byte UDP datagram for
  that TicketId;
- the WPF log contains the exact prefixes
  `D5 terminal wake matched retained ticket; authoritative TCP status query started. TicketId=0x`
  and
  `Callback D5 authoritative TCP status processed. TicketId=0x`
  for the same TicketId;
- exactly one callback-triggered TCP `0x7E03` query follows the UDP hint;
- receiving UDP alone does not change the operation state; the UI changes only
  after the TCP response;
- `TextOperationState` becomes `Callback D5 status refresh completed`;
- the operation summary contains `State=Completed, Outcome=Success`,
  `ResultType=Int8`, `ResultLength=1`, and the one-byte result;
- Attempt and Enqueued each increase by one; Rejected does not increase;
- sender Queued and RingAccepted each increase by one, QueueDepth returns to
  zero, and no drop/error counter increases. AdmissionRetry normally remains
  unchanged; a bounded `-4` admission retry must be recorded separately and may
  not produce an extra datagram. It makes the transport run non-clean, but does
  not by itself invalidate a single causal UDP-to-TCP result.

If the valid wake arrives while the WPF is still busy completing Submit, the
expected log is
`D5 terminal wake skipped while busy; manual/poll refresh remains available. TicketId=0x`.
That run proves a valid wake and the busy fence, but it is INCONCLUSIVE for the
automatic causal `0x7E03` requirement. Use GD-05A to recover the ticket and
repeat GD-01 with a distinct ticket; do not resubmit the same operation
automatically.

The UDP hint can also arrive before the awaited Submit continuation has retained
the returned ticket. In that race the SDK envelope is accepted, but WPF logs
`D5 terminal wake ignored: no exact current retained ticket, EventId=0x`, sends
no automatic `0x7E03`, and must not construct a ticket from UDP. Classify this
run as INCONCLUSIVE, wait for Submit to return and retain its exact ticket, use
GD-05A once to recover it, then repeat GD-01 with a distinct ticket.

### GD-02 two distinct tickets

Run the same low-level `Submit SDO Read` twice, waiting until the first ticket's
authoritative TCP response has made it terminal before submitting the second.

PASS requires two distinct nonzero TicketIds, one PLC-origin UDP wake per
TicketId, one authoritative `0x7E03` per TicketId, and no replay of the first
ticket. Sequence may advance by more than one if an earlier sender admission was
dropped; a forward gap is not itself a receiver rejection.

### GD-03 conditional Failed wake and recovery

Do not run this case until the drive manufacturer or the approved site test plan
identifies the exact nonexistent read-only object/sub-index and its expected SDO
abort. This repository deliberately does not invent that target. Without that
approval, record GD-03 as `NOT RUN`, not FAIL or PASS.

When an approved target exists:

1. Capture a valid `0x6061:0 Int8/1` baseline.
2. Enter the approved nonexistent read-only object/sub-index and press low-level
   `Submit SDO Read` once. Do not use the automatic abort runner for the causal
   measurement because that runner also polls.
3. After the failed ticket has been collected, submit the valid
   `0x6061:0 Int8/1` request as a distinct recovery ticket.

PASS requires the approved abort ticket to reach `State=Failed,
Outcome=Failed`, preserve the expected raw EtherCAT abort, and produce one valid
wake plus one authoritative status query. The recovery ticket must reach
`Completed/Success` and produce its own single wake. Value equality with the
baseline is a separate same-value qualification and is required only if the
approved site plan freezes the drive mode during this case.

### GD-04 reconnect/session fence

1. Complete GD-01.
2. Close and reconnect the WPF application.
3. Verify a new local `LMCConnection`, a new accepted callback SessionEpoch and
   cookie, and a fresh successful `0x405C` registration. Do not infer these from
   the WPF connection label alone.
4. Submit a new low-level `0x6061:0 Int8/1` read.

PASS requires the new ticket to complete normally through the new callback
tuple. Replaying a previous-session packet is a separate PC fake or approved
proxy case: it must be rejected by the listener/session fence and must not
create a TCP `0x7E03` or change the current UI.

An old D5 ticket is not queryable from the new WPF connection. Session close may
clear or orphan that ticket. Use the existing disconnect/orphan recovery flow
and a new read; never treat reconnect as authority to query or replay the old
ticket.

### GD-05A lost wake and manual fallback

Use a reversible firewall rule scoped only to the advertised UDP callback port,
or an approved proxy, to drop one valid wake while leaving the control TCP
connection untouched. Record the rule/proxy configuration and remove it after
the case.

PASS requires:

- the PLC producer records one Attempt and one Enqueued result;
- PLC-side or pre-drop capture shows the valid LMC2 packet;
- the PC records no accepted wake and sends no automatic `0x7E03`;
- UDP alone causes no terminal UI update;
- clicking `Refresh Ticket` once sends one exact `0x7E03`, sends no new
  `0x7E50`, and applies the terminal TCP response;
- restoring UDP does not replay the already attempted PLC wake.

Manual `Refresh Ticket` is the fallback for this retained ticket. The Inline
helper submits and polls its own ticket; it does not recover this one.

### GD-05B independent polling fallback

With UDP still deliberately unavailable, start a separate
`Read SDO Inline (wait terminal)` for `0x6061:0 Int8/1`. PASS requires its own
nonzero ticket and terminal result through bounded TCP `0x7E03` polling, with no
PLC CancelOperation and no automatic `0x7E50` replay. This proves the polling
fallback, not UDP-to-TCP causality.

If a terminal slot was replaced before status retrieval, `TicketNotFound` is a
stale-hint result. It is not a connection fault and it does not establish a new
operation outcome.

### GD-06 clean Close and callback disarm

1. Connect and verify an armed version-2 callback tuple.
2. With no intentionally pending D5 ticket, press WPF `Close` once.
3. Capture the TCP `0x405D` exchange and the subsequent connection teardown.

PASS requires `RpcCallbackLastDisarmResult` to be `0` (matched clear) or `1`
(already disarmed/empty), the RPC callback tuple to be cleared, sender
QueueDepth to be zero, and the WPF to show a stopped/disconnected listener.
`DisarmClearedCount` increases only by the queue depth actually cleared.

Do not require Attempt or Rejected to increase on a clean close. CyWork may
notify Diagnostics and clear/orphan the old ticket before the broker can claim
it. In a separately captured race, if a terminal tuple was already claimed
after the local publish fence closed, the allowed producer delta is Attempt
`+1`, Rejected `+1`, Enqueued `+0`. If it was already enqueued before disarm,
the sender may instead clear that queued frame. Classify the branch from the
packet and counter evidence; do not force it.

## P1: negative and lifecycle matrix

Ordinary WPF buttons cannot safely generate malformed envelopes, EventId zero,
or internal sender mismatches. Use the mode stated below. Never force private
PLC words in the production project.

| ID | Mode | Condition | Required result |
|---|---|---|---|
| GD-N01 | PC fake or approved proxy | Exact duplicate UDP packet, same sequence | First copy may dispatch. Second copy adds `RejectedCallbackCount +1` and `DuplicateCallbackWakeHintCount +1`; no WPF handler call, second TCP query, or UI transition. PLC producer counters do not change because of replay. |
| GD-N01B | PC fake | Same current TicketId in two valid packets with distinct forward sequences while the first `0x7E03` is held | Both envelopes count as SDK accepted. WPF logs `D5 terminal wake skipped while busy; manual/poll refresh remains available. TicketId=0x` for the second and sends only one `0x7E03`. |
| GD-N02 | PC fake or approved proxy | Sequence `N+1` accepted before older `N` | The older packet adds `RejectedCallbackCount +1` and `OutOfOrderCallbackWakeHintCount +1`; no handler call or additional TCP query. The first valid packet is a baseline, so merely starting above sequence 1 is not reorder. |
| GD-N03 | Hybrid or PC fake | Drop sequence `N`, then deliver valid higher sequence | The higher sequence is forward and may be accepted. No duplicate, out-of-order, or dedicated loss counter increases. In Hybrid mode recover the actual retained ticket through GD-05A. In PC-fake mode verify only the forward-gap fence/counters; do not claim PLC runtime or ticket recovery. |
| GD-N04 | PC fake | Valid current envelope with `EventId=0` | Parser rejects `EventIdentifierNotApproved`; aggregate Rejected increases, application dispatch and TCP query remain zero. This proves the PC fence only. |
| GD-N04P | Static/IDE, or separately approved PLC test harness | Direct sender policy check with `EventId=0`, an armed endpoint, matching `ProducerSessionEpoch`, and a valid zero-payload tuple | Production Diagnostics cannot emit this tuple because zero TicketId is not claimable. Under these preconditions the current source requires `PublishEvent` result `-6` before queue/sequence mutation. An unarmed endpoint, stale epoch, or invalid payload returns the earlier `-4`, `-8`, or `-7` and is a different case. Runtime proof requires a dedicated harness and must not be claimed from the production broker path. |
| GD-N05 | PC fake | Valid current envelope but wrong nonzero TicketId | `AcceptedCallbackWakeHintCount +1`; `RejectedCallbackCount` does not increase. WPF logs the exact template `D5 terminal wake ignored: no exact current retained ticket, EventId=0x{EVENT_ID_8HEX}, BootId=0x{BOOT_ID_8HEX}`, creates no ticket, sends no `0x7E03`, and does not mutate UI state. |
| GD-N06 | PC fake or approved proxy | Wrong BootId | `RejectedCallbackCount +1` with `StaleBootId`; no application dispatch, TCP query, or retained-ticket change. |
| GD-N07 | PC fake or approved proxy | Old SessionEpoch or cookie | Aggregate Rejected increases before application dispatch; no TCP query or UI mutation. |
| GD-N08 | PC fake or approved proxy | Foreign source IPv4 | Aggregate Rejected increases with unexpected-source decision even if the envelope is otherwise valid; no application dispatch. |
| GD-N09 | PC fake | Wrong event type, mask, delivery class, payload length, or flags | Parser/policy rejection; no authoritative query or UI mutation. |
| GD-N10A | PC fake, or commit `bff3bc7` mode `callback-ownership-wire` scenario `gd-n10a` against the actual PLC only after reviewed rebaseline and separate live approval | On one owner TCP session, registration A advertises owner IPv4 A; mismatch B reuses the same actual owner callback port/cookie/frame and changes only advertised callback IPv4 to different B; then A is duplicated byte-for-byte | First use the DRY-RUN command above. In an approved live capture, A succeeds, B returns failure without changing the accepted fence, and the duplicate A preserves BootId, SessionEpoch, and accepted maximum before the authoritative owner sends `0x405D`. First prove both addresses and the exact only-IPv4 byte difference in pcap. The current WPF cannot create this mismatch. Tool PASS alone does not prove retained PLC tuple/FIFO state; correlate PLC Watch. |
| GD-N10B | Separately approved PLC test harness | Claimed terminal tuple while CallbackSender is unavailable or the local RPC/session/owner/BootId tuple mismatches | Broker Attempt `+1`, Rejected `+1`, Enqueued `+0`; sender queue and wire unchanged; no retry of that tuple after recovery. This condition is not safely operator-generated by the current WPF. |
| GD-N11 | Actual PLC race, exploratory | Terminal tuple during pending close | No enqueue to the retiring session. If close notification clears/orphans before claim, all three producer deltas may be zero. If the broker already claims behind a closed local fence, Attempt `+1`, Rejected `+1`, Enqueued `+0`. Ordinary WPF controls may not create this timing deterministically; never force private words or require the latter branch unconditionally. |
| GD-N12 | Actual PLC | Clean WPF Close/disarm | Use GD-06. Old-session callback state is cleared and no late packet may update the new/current UI. Do not claim that an old ticket remains queryable after reconnect. |
| GD-N13 | Actual PLC with commit `bff3bc7` mode `callback-ownership-wire` scenario `gd-n13-candidate`, only after reviewed rebaseline and separate live approval | Two concurrent sessions bind the exact same source IPv4: owner initializes/registers, candidate initializes/registers as replacement, the old-owner peer-retirement barrier is observed, then candidate repeats its registration | First use the DRY-RUN command above. Never assign a duplicate static IP to two hosts. Approved live evidence requires same BootId, advanced nonzero SessionEpoch/max `52`, old owner disconnect without non-owner `0x405D`, byte/fence-stable candidate duplicate, and candidate-owner `0x405D`. Missing clean old-owner retirement is INCONCLUSIVE. Pcap must prove equal source IPv4 and UDP routability; PLC Watch must show `TakeoverCount +1`, `LastTakeoverResult=2`, and that a late old-socket disconnect does not clear the new owner. |
| GD-N14 | Actual PLC with commit `bff3bc7` mode `callback-ownership-wire` scenario `gd-n14-candidate`, only after reviewed rebaseline and separate live approval | Owner is initialized/registered; a concurrent candidate with a genuinely different source IPv4 attempts `0x8080`; after candidate rejection the owner duplicates its registration | First use the DRY-RUN command above. Approved live PASS requires candidate clean EOF/connection reset, no candidate `0x405C` or `0x405D`, unchanged owner BootId/SessionEpoch/max on duplicate, and only the authoritative owner sending `0x405D`. Timeout, ConnectionAborted, or Shutdown is INCONCLUSIVE, not rejection proof. Pcap must prove both source IPv4 values; PLC Watch must show `TakeoverRejectCount +1`, `LastTakeoverResult=-4`, and unchanged active owner/callback tuple. |

The PLC sender's `QueuedCount` and `RingAcceptedCount` are unrelated to PC
duplicate/reorder counters. Likewise, a wrong-ticket WPF semantic drop is not a
PLC producer rejection.

## Exact UDP packet checks

All fields are little-endian. For the initial Gate D event, the datagram must be
exactly 52 bytes:

| Offset | Field | Required value |
|---:|---|---|
| 0 | Magic | ASCII `LMC2` |
| 4 | ProtocolVersion | `2` |
| 6 | HeaderBytes | `52` |
| 8 | DatagramBytes | `52` |
| 10 | EventType | `1` |
| 12 | EventMaskBit | `1` |
| 16 | BootId | exact accepted Diagnostics BootId |
| 20 | SessionEpoch | exact accepted callback session |
| 24/28 | CookieLo/CookieHi | exact registration-request cookie |
| 32/36 | SequenceLo/SequenceHi | sender sequence; first new-arm enqueue is `1` |
| 40 | EventId | exact nonzero D5 TicketId |
| 44 | PlcTimeMs | enqueue-time `ops.tAbsolute` snapshot |
| 48 | PayloadBytes | `0` |
| 50 | DeliveryClass | `0` |
| 51 | Flags | `0` |

An exact duplicate registration preserves the next sequence. A disarm followed
by a new arm resets it to `1`. Queue-full rejection consumes no sequence;
admission drop may leave a forward gap.

## Evidence to return

For each executed case, save:

- case ID, evidence class (`Actual PLC`, `Hybrid`, `PC fake`, or `Static/IDE`),
  and start/end time;
- exact source/build/checkpoint identity and downloaded PLC BootId;
- WPF execution log;
- pcapng and the capture location relative to any firewall/proxy;
- submitted TicketId, BootId, callback SessionEpoch, and cookie;
- before/after PLC producer/sender and PC receiver counters;
- final `0x7E03` response state, outcome, error, detail, type, length, and data;
- the exact reversible fault-injection rule, proxy action, or fake-peer test used
  and confirmation that it was removed/stopped;
- `PASS`, `FAIL`, `INCONCLUSIVE`, or `NOT RUN`, with the first mismatching or
  unavailable field.

Static, PC fake-peer, C78 Build/Rebuild, PLC Download, and live callback packet
results must remain separate in the final qualification report.

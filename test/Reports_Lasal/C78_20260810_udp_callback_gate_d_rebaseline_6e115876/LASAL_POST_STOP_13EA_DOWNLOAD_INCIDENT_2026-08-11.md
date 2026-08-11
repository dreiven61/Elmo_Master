# LASAL post-STOP `13EA5823` / Download incident (2026-08-11)

## 1. Purpose and decision boundary

This note preserves the observed state after the previously frozen Gate D
classification had already stopped with
`UNSTABLE_THIRD_CLASSES_HASH_STOP`.  It is an incident record, not an accepted
extension of the isolated Rebuild evidence series.

The current decision remains fail-closed:

- `ProductionApproved=false`
- `SemanticEquivalenceProven=false`
- `rebaselinePermitted=false`
- `requiresReviewedTransition=true`
- `onlineRuntimeQualificationPermitted=false`
- no additional finalizer run is permitted against the appended log
- no additional Rebuild or Download is authorized by this record

The immutable eight-file bundle committed at `b2019db` is unchanged.  The
post-STOP sessions described below happened later and are outside that bounded
bundle.

## 2. Confirmed frozen observations

The following values were measured repeatedly after LASAL had exited.

| Item | Bytes | SHA-256 | Last write time (KST) | State |
| --- | ---: | --- | --- | --- |
| `Class/Classes.lcb` | 8,549,773 | `13EA5823DF0887D6042408E2A884E9F8DF50304443227353B9BDCA9AD2ECBFD9` | `2026-08-11 10:28:33.0778381 +09:00` | Git worktree modified; not committed as an accepted artifact |
| `Network/Networks.lcb` | 242,363 | `C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F` | `2026-08-10 13:10:48.9244985 +09:00` | byte-exact with the preserved post-Rebuild Networks snapshot |
| `%TEMP%\Lasal2.log` | 11,045,306 | `CEC2256AA0B7B02E2938C8E294C94CFD4A8EAE96C436BF318B16E28369367051` | `2026-08-11 10:36:40.0591253 +09:00` | contains two post-bundle LASAL sessions |

`Lasal2.exe` process count was zero during the repeated measurements.

The bounded post-bundle log region begins at byte offset `9,554,717` and ends
at exclusive offset `11,045,306`.  Its length is `1,490,589` bytes and its
SHA-256 is
`CAA408D3997182495023DBC1FA9719462447D2F822C459990CE4BECB6EA4E69C`.
The exact physical CRLF bytes are preserved as
`bounded_lasal2_delta_post_stop_13ea_download.raw.txt`.  The machine-readable
capture and nonapproval contract are recorded in
`bounded_lasal2_delta_post_stop_13ea_download.manifest.json`.

## 3. Confirmed command ledger

### 3.1 PID 26200: Rebuild, Connect, and Download

The relevant source-log range is lines `109928` through `119001`.

1. Line `109943`: the canonical project was loaded.
2. Line `117301`: one known load-only `E 0015` record reported failure to read
   `_DriveMngBase/DriveComL2.h` through `MotionLib/Include/global.h`.
3. Line `117797`: the Load command reached `Last command succeeded`.
4. Line `117799`: `Executing command 'Rebuild project'`.
5. Line `117801`: the target/compiler context was `C78`, `ARM`.
6. Line `118662` at `10:28:33`: the Rebuild command reached
   `Last command succeeded`.
7. Lines `118663` through `118686`: connection to `TCP_TEST` at
   `10.10.150.1:1954` succeeded.
8. Lines `118694` through `118699`: the Download composite command requested
   Reset, Download, Save Project on PLC, Delete file from PLC, and Restart.
9. The log records 282 successful `.lba` downloads.  This includes
   `LMCUdpCallbackSender.lba` at line `118927`.
10. Line `118983`: `Download 282 files need 4634 ms`.
11. Lines `118984` and `118985`: PLC linking succeeded.
12. Lines `118986` and `118987`: the command succeeded and `Download Ok` was
    recorded.
13. Lines `118993` through `119001`: the project went offline, closed, and
    LASAL exited normally.

### 3.2 PID 21016: Connect, Reset, and Restart

The relevant source-log range is lines `119003` through `126920`.

1. Line `119018`: the canonical project was loaded.
2. Line `126376`: the same known load-only `E 0015` record occurred once.
3. Line `126872`: the Load command reached `Last command succeeded`.
4. Lines `126874` through `126897`: connection succeeded.
5. Lines `126898` and `126899`: a standalone Reset succeeded.
6. Lines `126905` and `126906`: a standalone Restart succeeded.
7. Lines `126912` through `126920`: the project went offline, closed, and
   LASAL exited normally.

There was no Rebuild or Download in PID 21016.

After the earlier PID 31664 session ended, the inspected log region contained
no `CInvalidArgException`, no `FATAL`, and no `Last command failed`.  Both new
sessions nevertheless contained the trace
`No SDIAS Client objects ... Unable to continue with Initialisation!` before a
later `Project successfully loaded`.  Command completion is therefore proven;
clean equipment-runtime qualification is not.

The log cannot prove whether any restoration, Connect, Download, Reset, or
Restart action was automatic or operator-originated.

## 4. Exact Classes comparison evidence

The committed checkpoint artifact is:

- revision `55435791f6e91c9dcb4e06dcd25a11d77b382da7`
- blob `7b0faebb1450ff67b7dad44f081ad5c4ac141ee2`
- 8,549,773 bytes
- SHA-256
  `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861`

The generated comparison evidence is:

- file `classes_lcb_post_stop_13ea5823.comparison.json`
- 50,060 bytes
- SHA-256
  `DBC54235BDB505D9E7A198B3DCFA2CBD63F8AAC19728D1349FDC46DD5FA6CEC5`
- producer `Compare-LasalClassesArtifact.ps1`, 79,592 bytes, SHA-256
  `B91BFB5AFE131F0ECB3F23DC00373BEC7FC91B2C37CF626D128E912F633EBBA4`,
  blob `b90cf244bb7479b6a0d5da85750389785ccfe90e`, scoped HEAD-clean at capture
- this is a non-adversarial workspace physical snapshot; it does not
  authenticate the already executing PowerShell bytes
- decision exit `3`
- disposition `REJECTED_BOUNDARY_OR_CONTRACT_DRIFT`

Exact result:

- equal total length
- 90 changed bytes
- 57 contiguous changed runs
- 35 changed owners
- 0 unmapped runs
- ordered 120-owner inventory exact
- Gate D target records: 4 of 4 byte-exact
- protected dependency records `_StdLib` and `CriticalSection`: both byte-exact
- first-special record `_AxisBase`: not exact
- `changedOwnersAreFrozenOpaqueSubset=false`

`changedOwnersAreFrozenOpaqueSubset=false` is caused by `_AxisBase`, which is
outside the previously frozen opaque-owner set.  The other 34 changed owners
remain inside that set.

All 57 diff previews are complete and non-overlapping.  Applying those previews
to the committed checkpoint blob independently reconstructs an 8,549,773-byte
artifact with SHA-256 `13EA5823...` that is byte-exact with the measured current
`Classes.lcb`.  The reconstructed Git blob identity is
`4db7cf0d32c0cbd8ee53aacce28ea56048ca0674`.

## 5. Fixed-slot structural observation

All 90 changed bytes map to exactly one of the 157 previously identified
16-bit structural candidates; there are no changed bytes outside them.

- marker-follower slots changed: 30
- owner-record-end-minus-48 slots changed: 27
- total changed slots: 57

The two `_AxisBase` changes are:

| Offset | Structural role | Checkpoint word | Current word | Boundary evidence |
| ---: | --- | --- | --- | --- |
| 164307 | marker follower | `9FE9` | `FAE9` | preceding 12 bytes are the exact marker `8F681A166DB06E3785CA7341` |
| 170415 | record end minus 48 | `9CEC` | `3CEC` | `_AxisBase` record end is 170463 |

Relative to the earlier A/B/C triad, 52 changed slots were already volatile and
five had previously appeared stable:

- `_AxisBase` marker at offset `164307`
- `_AxisBase` tail at offset `170415`
- `_LMCAxisVisLogViewer` tail at offset `1448548`
- `_LMCAxisVOVMonitoring` marker at offset `1463922`
- `_LMCPublisher` tail at offset `3630127`

Adding this unaccepted observation changes the diagnostic union from 66 to 71
volatile candidates.  That statement is structural only.  It does not make the
current artifact a reviewed corpus input and does not identify the field
semantics.

## 6. Artifact-to-Download binding decision

The following chronology is confirmed:

- Rebuild command completion: `10:28:33`
- current `Classes.lcb` last-write time: `10:28:33.0778381`
- Connect: `10:28:36`
- Download and linking: approximately `10:28:44` through `10:28:50`

Inference only: the evidence supports strong time correlation between the
`13EA...` file and the successful Rebuild, followed by a successful Download in
the same LASAL session.

It is not exact artifact-to-Download binding.  The following evidence was not
captured:

- a SHA-256 measurement of `Classes.lcb` before Download
- any `Classes.lcb` or `Networks.lcb` path/hash in the Download log
- a byte-hash manifest for the 282 downloaded `.lba` files
- a vendor-defined mapping from the opaque `Classes.lcb` fields to the deployed
  `.lba` bytes

The exact decision is therefore:

- `exactClassesHashCapturedBeforeDownload=false`
- `classesLcbListedAsDownloaded=false`
- `downloadedLbaHashManifestCaptured=false`
- `exactArtifactToDownloadBinding=false`
- `association=TIME_CORRELATION_ONLY`

No statement in this note may be used to claim that `Classes.lcb` itself, the
`13EA...` hash, or the 90 opaque changed bytes were transferred to or consumed
by the PLC.

## 7. Consequence

The post-STOP observation strengthens the structural finding that the volatile
bytes occupy two fixed 16-bit slot families, while weakening any assumption
that the A/B/C triad had exhausted the volatile locations.  It does not explain
the field meanings, demonstrate deterministic regeneration, reproduce the
accepted checkpoint, or satisfy a reviewed strict transition.

Therefore:

- preserve this comparison and note as an incident record
- keep the `b2019db` eight-file finalization bundle immutable
- do not append this session to that bundle
- do not rerun the old finalizer against the appended log
- do not repeat Rebuild or Download to improve this evidence retroactively
- keep artifact transition, production approval, and runtime qualification
  stopped pending vendor semantics or a separately reviewed protocol

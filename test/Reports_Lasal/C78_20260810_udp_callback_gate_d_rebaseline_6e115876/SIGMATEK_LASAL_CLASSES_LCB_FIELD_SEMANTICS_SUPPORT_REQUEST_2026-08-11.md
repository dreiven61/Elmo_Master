# Draft support request: LASAL CLASS `Classes.lcb` field semantics

> Internal boundary / 내부 경계: This is a draft only. Do not send it, normalize
> `Classes.lcb`, rebaseline any hash, approve production, or download to a PLC
> without a separate reviewed decision. 현재 상태는 STOP/nonapproval이다.

**To:** SIGMATEK LASAL Support
**Subject:** Request for authoritative `Classes.lcb` format `0x5E` field semantics and release-handling guidance

Hello SIGMATEK Support Team,

We are investigating reproducibility differences in the generated LASAL CLASS
project file `Class/Classes.lcb`. We need authoritative field semantics and
release-handling guidance before deciding whether any two generated files may be
treated as equivalent.

This inquiry does **not** assume that a file described as temporary is safe to
ignore. We are not requesting permission to normalize bytes, rebaseline a hash,
approve a production release, or download anything to a controller. Our current
project gate remains stopped and nonapproved pending a reviewed answer.

## Installed environment

The observed Rebuild output was produced in this installed environment:

- Executable: `C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\Lasal2.exe`
- File version: `02.03.002`
- Product version: `02.03.002`
- Special build: `Build: 21956`
- Executable size: `55,137,280` bytes
- Executable SHA-256: `F14BE5678DA5BAE1B1D2D38D770E4A0C05F87EEEA7981832F621F59FAE122F3A`
- English help: `C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\LASAL_CLASS_2_EN.chm`
- Help size: `50,348,885` bytes
- Help SHA-256: `6BAD01890C1BD81ADB4942A69F2E38F462C4CE29CF41B40E0AB3DE5DAA7D9A0B`

The installed English help says that an `.lcb` file stores binary information
so LASAL does not repeatedly process the corresponding ASCII information and
can operate faster. It also lists `.lcb` under temporary files. We could not
find a field layout, field-consumption contract, or comparison procedure in
that help. The public [LASAL CLASS product page](https://www.sigmatek-automation.com/en/products/engineering-tool-lasal/lasal-class/)
also does not provide the field-level information needed here. We are therefore
using the official [SIGMATEK support channel](https://www.sigmatek-automation.com/en/service/support/)
for clarification.

## Observations requiring clarification

All three pinned files below are `8,549,773` bytes and begin with the ASCII
signature `SigmatekLasal2Binary\0`. Immediately after that signature, at file
offset decimal `21` (`0x15`), we observe byte `0x5E`. Please confirm whether
`0x5E` is the file-format or persistence-format version, and provide its exact
version mapping and compatibility rules.

| ID | Role | SHA-256 |
|---|---|---|
| A | Pinned canonical project checkpoint | `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861` |
| B | Later committed-evidence Rebuild observation, reconstructed in memory from its full committed oracle | `6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712` |
| C | Later committed Rebuild snapshot | `99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD` |

The A/B/C comparison found that every changed offset maps to one of two fixed
16-bit candidate locations:

1. The two bytes immediately following marker
   `8F681A166DB06E3785CA7341`: `35` varying marker-follower slots in the triad.
2. The two bytes beginning at `recordEndExclusive - 48`: `31` varying
   owner-tail slots in the triad.

This is a structural observation only. It is not a field-name or semantic
identification.

We also analyzed the pinned first-parent history of the canonical
`Class/Classes.lcb` path. The history contains `22` occurrences and `20` unique
artifacts. Adding C and B gives `22` unique artifacts, `2,501` parsed owner
records, and `814` marker samples.

For diagnostic grouping, we zeroed only the two-byte candidate word and required
all remaining bytes in the same record to match exactly. In the full H+C+B
layer, the results were:

| Candidate family | Exact-other-bytes groups with unequal target values | Unequal artifact pairs | Owners |
|---|---:|---:|---:|
| `recordEndExclusive - 48` | 87 | 202 | 31 |
| Marker follower | 95 | 369 | 34 |

Across `21` adjacent mainline transitions, `2,378` owner records were common:

- `1,155` were raw byte-identical.
- `538` changed only at the two candidate locations.
- `685` changed outside those locations.
- The `538` candidate-only changes partition exactly into `55` tail-only,
  `97` marker-only, and `386` both-family changes.

These counterexamples show that the candidate values are not determined solely
by a fixed stateless function of the other bytes in the same record among the
tested common checksum/hash/length hypotheses. They do **not** exclude an
external seed, timestamp, process/session state, filesystem state, cache state,
ordering state, or another LASAL-internal input.

We can prove equality of the explicitly scoped project inputs recorded in the
build evidence, but we did not capture every effective generator input for all
three observations. In particular, compiler identity, vendor-library set,
generator cache state, filesystem timestamps, and process-session state are not
all proven equivalent. We therefore are not claiming that A, B, and C came from
fully identical generator state.

## Questions requiring an authoritative answer

1. Does the byte `0x5E` immediately after `SigmatekLasal2Binary\0` identify the
   `Classes.lcb` format version? If so, what is the official name and version of
   this format, and which LASAL CLASS releases can read or write it?

2. For the 16-bit word immediately after marker
   `8F681A166DB06E3785CA7341`, please provide:

   - the official field name and owning structure;
   - its exact type, width, signedness, and byte order;
   - how its value is generated and validated; and
   - whether and when it is consumed during project open, class parsing,
     Build/Rebuild All, download, or target runtime.

3. Please provide the same information for the 16-bit word beginning at
   `recordEndExclusive - 48` in an owner record.

4. Are either of these fields a GUID fragment, archive/reference identifier,
   timestamp, pointer or offset, checksum/CRC, cache key, process/session value,
   serialization-order value, or another persistence field? If none of these,
   what is their exact semantic role?

5. If every effective input to **Rebuild All** is identical, is variation in
   either field intended? If variation is intended, which inputs or sources of
   nondeterminism control it, and what provenance must be captured to reproduce
   or validate the result?

6. Does LASAL validate either field? What happens when a value is stale,
   different, zero, or invalid: rejection, silent regeneration, cache miss,
   changed generated code, changed download content, or a runtime-semantic
   change?

7. Is there an official comparator or canonicalization procedure for
   `Classes.lcb`? Specifically, can two files that differ only at these fields
   ever be declared build- or release-equivalent? If yes, please provide the
   supported tool/command and the exact release-safety acceptance criteria. If
   no, please state which identity or validation must remain exact.

8. Should `Class/Classes.lcb` be source-controlled, or should it be regenerated?
   If it is generated/temporary, which editable ASCII/project sources are
   canonical, and what is the supported clean regeneration procedure? Please
   include any required close/delete/reopen/Build/Rebuild sequence and any files
   that must or must not be deleted.

9. Can you provide one of the following for format `0x5E`:

   - a versioned binary-format specification;
   - the relevant Persistence/Binary structure definition and field map;
   - an official inspection/export/comparison tool; or
   - a support statement identifying the two fields and their release impact?

10. What exact provenance should a support case include for this issue—for
    example LASAL executable, compiler, installed library set, project options,
    cache state, timestamps, session identity, source inventory, and build log?

## Proposed attachment manifest

No `.lcb`, executable, help file, or reconstructed binary is copied into this
draft or proposed as a default attachment. The following committed source and
JSON evidence files can be supplied first; binary artifacts can be supplied
only if SIGMATEK requests them through an approved secure channel.

### Analysis tools and reports

| Purpose | Repository path | Bytes | SHA-256 | Commit | Git blob |
|---|---|---:|---|---|---|
| Pinned A/B/C triad analyzer | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Compare-LasalClassesVolatilityTriad.ps1` | 139,073 | `E3E2C586C62379339EECFD8038189D9959C655CD206A4E894B846A2D79783663` | `998e7132c0892788db79a0868c5b129fb20edd96` | `a7dd4dba67e30c4adc80549a1d9b6a4d1acb6bce` |
| Pinned triad report | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_99014dd9.volatility.json` | 29,412 | `09C76BB3BC313642C3012A915C14C022EDF75965A8A431B87F26B463005489DC` | `e7c812ad7cfc6ef2162ed1197dc615e2aebe45db` | `3c4411e26493043b80828a5355bdc8b621457e09` |
| Pinned historical-corpus analyzer | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/Analyze-LasalClassesHistoricalSlotCorpus.ps1` | 156,472 | `90BDD86EFC9C5032788C2603755A3560CC2871672E638935C4CD955B705EA080` | `731a01e428bdc9282edbf727f1d76a7a63cd24a3` | `30379ef2a50e093bbb28d768b9df77d091199de6` |
| Pinned historical-corpus report | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/classes_lcb_historical_slot_corpus_bd9dcb0c_55435791_99014dd9_6e115876.analysis.json` | 157,999 | `F306022CECD6C71BB7EA2B3DF309556A2621821B6C2CD287BC3FFFF4FA5A1B6A` | `43a85319905fbb5a42418b4b1ef9cd364c0bf44d` | `edad859d03ac0c33f21ca42996a377dda3ee7b79` |

### Binary identities and reconstruction inputs (manifest only)

| Role | Repository path and provenance | Bytes | SHA-256 |
|---|---|---:|---|
| A | `Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb` at commit `55435791f6e91c9dcb4e06dcd25a11d77b382da7`, blob `7b0faebb1450ff67b7dad44f081ad5c4ac141ee2` | 8,549,773 | `24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861` |
| B reconstruction oracle | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json` at commit `2e8ca8a84a141390424ce859ac8c315a90ec3430`, blob `2a73c039391a487082bc0958233ef1930a298f91` | 51,102 | `9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639` |
| B patch manifest | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.manifest.json` at commit `703844576c658460a018373894db85e43cda3096`, blob `e181b57a15bd10465ba6de100aa239d4dfe8709b` | 2,427 | `B919A2EC25ABE99C7C8D5D37E19F0EDDB3D7998C1DF7C1F7C74FB3B9B5D8956C` |
| B binary-patch preservation evidence | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.binary.patch` at commit `703844576c658460a018373894db85e43cda3096`, blob `fc36eb76c3293e04a7aa0acf4674d408865ffa70` | 2,553 | `AF9A4D32B6F568036E4200BD3F47C9CD63ABB4027D37A1F60BEDB7287731A160` |
| B reconstructed result | No mutable worktree file or local Git object is required; reconstructed in memory from the full committed oracle | 8,549,773 | `6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712` |
| C | `test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/candidate_finalization_gate_d_rebaseline_6e115876/Classes.post-rebuild.snapshot.lcb` at commit `b2019db3af5a9990d2e0fe0afd0f02cbfbfaff53`, blob `726f5ed4498592dba13e358c0d7320d2e5d02a1a` | 8,549,773 | `99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD` |

We would appreciate an answer tied specifically to LASAL CLASS file/product
version `02.03.002`, special build `Build: 21956`, and format byte `0x5E`. If
the behavior or field layout changed in another release, please identify the
applicable release boundary.

Thank you.

Regards,
`[Name / company / support contract number]`

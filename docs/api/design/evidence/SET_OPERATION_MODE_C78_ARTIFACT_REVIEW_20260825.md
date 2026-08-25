# SetOperationMode Fresh C78 Artifact Review Checkpoint

- Date: 2026-08-25
- Source branch: `dev`
- Fresh build commit: `a3bcdea3c5464e31b47a49a4380d853ad93ac1c8`
- Parent before fresh build: `de1d3220ddfb9c2cf585ac335356a5550420e4ec`
- Feature state: `Dormant`; `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE`, Admin capability bits 8/9/10 remain OFF
- Evidence grade: `IDE/artifact checkpoint`, not hardware/packet qualification

## 1. Operator-observed IDE result

The fresh LASAL build initially failed because the SetOperationMode `LMC_DIAG_MODE_*` preprocessor
definitions were below the valid declaration region. The definitions were moved into the common define
block before the first user `LMCDiagnosticsService` implementation function and the build then completed.

After the build, PLC download comparison reported that the target code was the same. This is consistent
with the tracked source delta: the SetOperationMode define block changed location, but its values and
runtime statements did not change. This observation does **not** prove MODE-11/12 behavior by itself.

A build-specific compiler/linker log was not committed with `a3bcdea3`; therefore this checkpoint does not
invent an exact warning count or linker text. The successful build/download comparison is operator-observed
evidence; repository evidence below is limited to tracked source, fresh generated artifacts and CI checks.

## 2. Tracked source delta

`a3bcdea3` changes three files relative to its parent:

1. `LMCDiagnosticsService.st`: SetOperationMode `#define` block moved from the later implementation
   section into the common pre-function define block; values are unchanged.
2. `Class/Classes.lcb`: regenerated binary artifact.
3. `Elmo_EtherCAT_Test_4Axis.lcb`: regenerated project artifact.

No SetOperationMode activation flag or capability bit was enabled by this build-fix commit.

## 3. Generated artifact identity

| Artifact | Parent Git identity | Fresh Git identity | Parent bytes | Fresh bytes | Fresh SHA-256 |
|---|---|---|---:|---:|---|
| `Class/Classes.lcb` | `0890d99d0ae5bb81d2227a3ce24892713cfd0e2e` | `1719bb5b73972db01968effafe7652d7199d43ea` | 8,613,996 | 8,635,373 | `E71914F152C829AD033BB8F4B7D70326A5E5C5A70BF8559AEF8F9207DA054E1C` |
| `Elmo_EtherCAT_Test_4Axis.lcb` | `b4fa8f68080386185400a1f95957a8179b07a28a` | `b88f57da4bd08c2838e8d1260b3d4929116b34ae` | 634,865 | 634,865 | `9887CD1BE02A4143FF67E8AC0D394123441C99C801F23C1DEDA8D93834732CF6` |

The existing UDP/SetPosition physical ratchet expects `Class/Classes.lcb` 8,610,206 bytes with SHA-256
`33C1C2A68B97E852AD6646317CAE032A110D1F50C9615FA5B7EEF00410B649A8`. The fresh artifact is therefore
not an exact replacement that can be approved by changing a hash alone. It contains later generated state
and must receive a new semantic/generated-ABI review before that older ratchet can be advanced.

## 4. Current CI qualification

PR #17 reordered the SetOperationMode static workflow so source semantics are proven before the known
artifact boundary.

Current fresh-tree evidence:

- `Verify-SetOperationModeStatic.ps1`: **57/57 PASS**
- method budgets: Start 12,285; Outcome 9,660; Retire 2,562; main 19,895; mutation 19,731; recovery 14,251 bytes
- `0x6060` write site ownership: main 0, mutation exactly four physical-axis fanout sites, recovery 0
- `Verify-SetOperationModeDefineOrder.ps1`: **PASS**, 71 `LMC_DIAG_MODE_*` defines, all before first user implementation function
- frozen owner/resource/stage/detail ABI constants: PASS
- C78 evidence collector self-test: PASS
- `git diff --check`: PASS
- full repository SourceOnly: **STOP only at the existing `SetPosition-augmented Classes.lcb physical identity drifted` ratchet**

The SourceOnly STOP is an artifact-identity boundary, not a SetOperationMode source-semantic failure.

## 5. Artifact/source review result

Current review result: **ACCEPT AS FRESH GENERATED CANDIDATE, KEEP ACTIVATION OFF**.

Rationale:

- the source change is a compiler-order correction rather than a semantic SetOperationMode change;
- both generated `.lcb` artifacts changed after the fresh build, so the repository is not carrying the prior
  binary artifact unchanged;
- the project `.lcb` retained its byte length while changing content, which is compatible with regenerated
  build metadata/content and does not by itself imply a runtime semantic change;
- PLC download reporting `code same` is compatible with unchanged executable semantics after a
  preprocessor declaration-order correction;
- the fresh artifact physical identity is now explicitly pinned as evidence, but the older UDP/SetPosition
  physical ratchet remains intentionally unmodified;
- source/static, WPF recovery, packet, PLC runtime and physical-drive evidence remain separate gates.

## 6. New regression gate

The C78 failure exposed a missing source/static invariant: SetOperationMode preprocessor definitions must
remain in the valid user-implementation pre-function define region. `Verify-SetOperationModeDefineOrder.ps1`
enforces:

- all `LMC_DIAG_MODE_*` definitions after `//{{LSL_IMPLEMENTATION` precede the first user
  `LMCDiagnosticsService` implementation function;
- the SetOperationMode define block remains contiguous;
- frozen owner/resource/stage/detail constants keep their expected values.

This prevents a future source-only refactor from recreating the same C78 compile failure.

## 7. Remaining gates

This checkpoint does **not** complete MODE-11, MODE-12 or MODE-14.

1. Preserve the new generated artifacts and the successful source-order layout.
2. Run MODE-11 on axis 1:
   - already-CSP (`0x6061 = 8`) => terminal success with **no `0x6060` write**;
   - non-CSP safe state => **exactly one one-byte `0x6060:0 = 8` write**, followed by `0x6061` readback = 8;
   - correlate `0x7D23` Start, `0x7D24` terminal outcome and exact-generation `0x7D25` retire.
3. Run MODE-12 timeout/disconnect/mismatch/quarantine/recovery/retire matrix, axis 1 first then axes 2..4.
4. Only after hardware/packet evidence passes may MODE-14 pair-enable `LMC_DIAG_SET_OPERATION_MODE_ENABLED`
   and Admin capability bits 8/9/10 on the production branch.

## 8. Activation decision

- ArtifactRatchetDecision: **REVIEW_REQUIRED**
- QualificationCandidate: **READY FOR A SEPARATE MODE-11 BENCH-ONLY ACTIVATION BRANCH**
- CapabilityActivation on `dev`: **KEEP_OFF**
- Production: **NO-GO**

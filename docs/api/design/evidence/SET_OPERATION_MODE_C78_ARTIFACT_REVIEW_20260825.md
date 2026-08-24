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
block before the first `LMCDiagnosticsService` implementation function and the build then completed.

After the build, PLC download comparison reported that the target code was the same. This is consistent
with the tracked source delta: the 73 SetOperationMode define lines changed location, but their values and
runtime statements did not change. This observation does **not** prove MODE-11/12 behavior by itself.

A build-specific compiler/linker log was not committed with `a3bcdea3`; therefore this checkpoint does not
invent an exact warning count or linker text. The repository evidence below is limited to tracked source and
fresh generated artifacts.

## 2. Tracked source delta

`a3bcdea3` changes three files relative to its parent:

1. `LMCDiagnosticsService.st`: 73 SetOperationMode `#define` lines moved from the later implementation
   section into the common pre-function define block; values are unchanged.
2. `Class/Classes.lcb`: regenerated binary artifact.
3. `Elmo_EtherCAT_Test_4Axis.lcb`: regenerated project artifact.

No SetOperationMode activation flag or capability bit was enabled by this build-fix commit.

## 3. Generated artifact identity

| Artifact | Parent identity | Fresh identity | Parent bytes | Fresh bytes | Review |
|---|---|---|---:|---:|---|
| `Class/Classes.lcb` | Git blob `0890d99d0ae5bb81d2227a3ce24892713cfd0e2e` | Git blob `1719bb5b73972db01968effafe7652d7199d43ea` | 8,613,996 | 8,635,373 | changed, +21,377 bytes |
| `Elmo_EtherCAT_Test_4Axis.lcb` | Git blob `b4fa8f68080386185400a1f95957a8179b07a28a` | Git blob `b88f57da4bd08c2838e8d1260b3d4929116b34ae` | 634,865 | 634,865 | content changed, size unchanged |

The artifact identities above are Git blob identities, not substitutes for the repository's SHA-256 physical
artifact ratchet. The physical ratchet must not be updated from a hash change alone.

## 4. Artifact/source review result

Current review result: **ACCEPT AS FRESH GENERATED CANDIDATE, KEEP ACTIVATION OFF**.

Rationale:

- the source change is a compiler-order correction rather than a semantic SetOperationMode change;
- both generated `.lcb` artifacts changed after the fresh build, so the repository is not carrying the prior
  binary artifact unchanged;
- the project `.lcb` retained its byte length while changing content, which is compatible with regenerated
  build metadata/content and does not by itself imply a runtime semantic change;
- PLC download reporting `code same` is compatible with unchanged executable semantics after a
  preprocessor declaration-order correction;
- source/static, WPF recovery, packet, PLC runtime and physical-drive evidence remain separate gates.

## 5. New regression gate

The C78 failure exposed a missing source/static invariant: SetOperationMode preprocessor definitions must
remain in the valid pre-function define region. `Verify-SetOperationModeDefineOrder.ps1` is added to enforce:

- all `LMC_DIAG_MODE_*` definitions precede the first `LMCDiagnosticsService` implementation function;
- the SetOperationMode define block remains contiguous;
- frozen owner/resource/stage/detail constants keep their expected values.

This prevents a future source-only refactor from recreating the same C78 compile failure.

## 6. Remaining gates

This checkpoint does **not** complete MODE-11, MODE-12 or MODE-14.

1. Preserve the new generated artifacts and the successful source-order layout.
2. Run MODE-11 on axis 1:
   - already-CSP (`0x6061 = 8`) => terminal success with **no `0x6060` write**;
   - non-CSP safe state => **exactly one one-byte `0x6060:0 = 8` write**, followed by `0x6061` readback = 8;
   - correlate `0x7D23` Start, `0x7D24` terminal outcome and exact-generation `0x7D25` retire.
3. Run MODE-12 timeout/disconnect/mismatch/quarantine/recovery/retire matrix, axis 1 first then axes 2..4.
4. Only after hardware/packet evidence passes may MODE-14 pair-enable `LMC_DIAG_SET_OPERATION_MODE_ENABLED`
   and Admin capability bits 8/9/10.

## 7. Activation decision

- ArtifactRatchetDecision: **REVIEW_REQUIRED**
- CapabilityActivation: **KEEP_OFF**
- Production: **NO-GO**

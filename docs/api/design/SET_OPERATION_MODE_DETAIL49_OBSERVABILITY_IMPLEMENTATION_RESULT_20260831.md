# SetOperationMode Detail 49 observability implementation result - 2026-08-31

> Status: **SOFTWARE IMPLEMENTED / PLC QUALIFICATION OPEN**
>
> Design source: `SET_OPERATION_MODE_PHYSICAL_FINDING_OUTCOME_STORAGE_20260831.md`
>
> Production release posture: **NO-GO**

## 1. Implemented protocol split

The Start rejection contract now reserves Detail 49 for actual retained outcome storage availability. Two previous Detail 49 producers have separate details.

| Detail | Symbol | Producer condition |
|---|---|---|
| 49 | `SetOperationModeOutcomeStorageUnavailable` | actual retained outcome storage availability only |
| 52 | `SetOperationModeOwnershipChannelUnavailable` | `AxisOwnership` client disconnected |
| 63 | `SetOperationModeAdmissionIdentityUnavailable` | caller session, request sequence, admission token, or owner generation is zero |
| 64 | `SetOperationModeFeatureDisabled` | loaded runtime feature gate is disabled |
| 42 | `AxisOwnershipQuarantined` | ownership identity validation or commit failed |

No admission value is exposed in the response or operator error catalog. The host receives only the symbolic detail and the existing request identity.

## 2. Changed components

- `LMCDiagnosticsService.st`: Feature-disabled and zero admission tuple branches now use Detail 64 and 63 respectively.
- `LmcAdminModels.cs`: exposes the same numeric details to the SDK.
- `LmcAdminSetOperationModeProtocol.cs`: accepts both as definitive Start rejections, so no retry/replay is introduced.
- `LmcErrorCatalog.cs`: supplies operator-safe resolution text.
- `Verify-SetOperationModeStatic.ps1` and SDK protocol tests: prevent producer/parser/catalog drift.

## 3. Safety invariants unchanged

- no ownership reservation, validation, or commit bypass;
- no synthesized admission identity;
- no change to Standstill, Fault, or OperationEnabled fences;
- no change to durable pre-dispatch journal;
- no automatic replay after accepted or uncertain Start;
- Generic SDO `0x6060` remains denied.

## 4. Required next physical run

Build and download the exact source/artifact, then repeat Axis1 CSP(8) to PP(1). Record source SHA, generated LASAL artifact, PLC BootId, MapRevision, WPF EXE/DLL build times, and returned detail.

- Detail 64: inspect the generated/loaded feature activation path.
- Detail 63: inspect TCP reservation to Diagnostics admission-tuple forwarding.
- Detail 52: inspect the `AxisOwnership` LASAL channel/network binding.
- Detail 42: inspect ownership identity validation or commit evidence.
- Start accepted: begin the one-write `0x6060` / exact `0x6061` qualification matrix.

This software implementation does not establish a successful PLC Start or any physical mode change.

## 5. Current software verification

The following PC/source checks passed against the implementation.

```text
MODE-10 qualification static verifier    69 checks PASS
API Debug build and full suite            1200 / 1200 PASS
API Release build and full suite          1200 / 1200 PASS
LASAL added-diff ASCII check              PASS
git diff --check                          PASS
```

WPF Debug/Release rebuild and the repository-wide LASAL contract snapshot were not rerun to completion because the current project was open in `Lasal2.exe` and the Debug WPF executable/DLL was locked by the running example application and Visual Studio Remote Debugger. These are environment gates, not a source PASS.

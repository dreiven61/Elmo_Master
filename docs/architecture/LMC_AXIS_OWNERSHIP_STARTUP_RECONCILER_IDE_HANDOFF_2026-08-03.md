# LMC Axis Ownership Startup Reconciler IDE Handoff

> 2026-08-04 TW-only supersession: the TW20/TW19 gate-OFF statement below is the
> startup-reconciler checkpoint, not current encoder-maintenance status. Both TW gates are now
> enabled for the exact fixed UInt16 value-one contract documented in
> [TW19/TW20 fixed-one activation](./LMC_ENCODER_MAINTENANCE_TW19_TW20_FIXED_ONE_ACTIVATION_2026-08-04.md).
> Ordinary ownership, LMC Home and DS402 Home remain separate activation decisions.

- Date: 2026-08-03
- Canonical project: `Lasal_PRG/Elmo_EtherCAT_Test_4Axis`
- Scope: ownership startup proof `0x0000000F` only
- Activation boundary: ordinary ownership, Home, DS402 Home, TW20 and TW19 gates remain `FALSE`

## 1. Original blocker and implementation status

The original blocker was a `TCPMotionInterface.CyWork` caller that reported only BootId proof bit
`0x00000001` with `QuarantineReason=-31`. The removed `ReportAxisOwnershipStartup` required exact
`0x0000000F`, so that first call turned a fresh zero ownership table into a permanent quarantine;
later full proof could not recover the same table.

The fix is to remove the partial caller and the bypassable public report ABI. A new
`LMCDiagnosticsService.ProcessAxisOwnershipStartup` private function submits reconciliation only
from a seqlock-protected RT snapshot. `LMCControlCommandService` is the only place that combines
the physical, Group and diagnostics proof and counts the stable samples.

This source and IDE-structure work is now implemented in the canonical project. The obsolete
`ReportAxisOwnershipStartup` ABI and BootId-only TCP caller are absent. The replacement snapshot,
reconciler and private diagnostics call chain are present. This closes the BootId-only permanent
quarantine source blocker only; ordinary Axis/Group activation and PLC runtime qualification remain
open.

## 2. Exact IDE declarations

No Network connection is added or removed. The existing connections are sufficient:

- `LMCDiagnosticsService1.AxisOwnership -> LMCControlCommandService1.ClassSvr`
- `LMCControlCommandService1.InputLatch -> LMCEcatInputLatch1.ClassSvr`
- `LMCDiagnosticsService1.InputLatch -> LMCEcatInputLatch1.ClassSvr`

### 2.1 `LMCEcatInputLatch`

Add one GLOBAL function.

```text
CopyAxisOwnershipStartupSnapshot
  pDest : ^void
  DestSize : UDINT
  Result : DINT
```

The function copies exactly 48 bytes from the currently unused `SnapshotBytes[464..511]` range
under the existing `PublishSequence` seqlock. The RT producer writes this range before it closes
the same publish transaction used by `CopySnapshot` and `CopyTopologyIoSnapshot`.

| Byte | Type | Value |
|---:|---|---|
| 0 | `UDINT` | magic `0x4C4D4353` |
| 4 | `UDINT` | RT observation cycle, equal to `SnapshotBytes[0]` |
| 8..20 | `UDINT[4]` | Axis1..4 `_LMCAXIS_STATUS` |
| 24..36 | `UDINT[4]` | Axis1..4 `Drive.StateWord` low 16-bit values; this document does not claim these are DS402 `0x6041` reads |
| 40 | `UDINT` | latch drain flags |
| 44 | `UDINT` | reserved, exact zero |

Latch drain flags require exact `0x0000001F`:

- bit 0: master, Drive1..4 and LMCAxis1..4 are connected; master and drives are OP,
  no invalid cycle or AL status is present, and every drive last-valid cycle is current;
- bit 1: Axis Zero Home request/applied sequence is equal and the result is zero or exact terminal;
- bit 2: DS402 Home request/applied sequence is equal;
- bit 3: DS402 RT owner token/axis and desired bit are zero and alignment is idle;
- bit 4: Drive1..4 ControlWord bit 4 is low.

A scalar `Get...Flags()` method is prohibited. It has neither an RT observation cycle nor a
seqlock and can therefore combine fields from different mailbox states.

### 2.2 `LMCControlCommandService`

Add one variable:

```text
OwnershipStartupState : ARRAY [0..15] OF DINT
```

Delete the existing GLOBAL `ReportAxisOwnershipStartup`. Add one GLOBAL function with this exact
input order:

```text
ReconcileAxisOwnershipStartup
  DiagnosticsBootId : UDINT
  ObservationCycle : UDINT
  ReportCycle : UDINT
  DiagnosticsDrainFlags : UDINT
  Result : DINT
```

`OwnershipStartupState` layout is internal and fixed for verification:

| Slot | Value |
|---:|---|
| 0 | reconciler magic `0x4F575350` |
| 1 | current BootId |
| 2 | last accepted RT observation cycle |
| 3 | consecutive fresh stable sample count |
| 4 | last combined proof flags |
| 5 | packed Group/profile signature |
| 6..9 | previous Axis1..4 `_LMCAXIS_STATUS` |
| 10..13 | previous Axis1..4 `Drive.StateWord` low 16-bit values |
| 14 | first stable sample service milliseconds |
| 15 | pending detail, reserved from the owner table |

### 2.3 `LMCDiagnosticsService`

Add one PRIVATE function. It must not be GLOBAL.

```text
ProcessAxisOwnershipStartup
```

No variable, client or Network change is required. `ProcessOperations` calls this private function
after `ProcessEncoderMaintenance` and `ProcessAxisDs402Home`, and before any generic-SDO early
return.

Delete the BootId-only startup block and its local variables from `TCPMotionInterface.CyWork`.

## 3. Proof contract

The reconciler initializes or reinitializes the owner table only after it has assembled exact proof
`0x0000000F`. It has no public partial-proof input and never writes a fresh owner table while proof
is incomplete.

| Bit | Source-backed predicate |
|---:|---|
| 0 | `DiagnosticsBootId` is nonzero. |
| 1 | The 48-byte latch snapshot is coherent/current and has health bit 0. Each physical Axis1..4 has `Standstill=1` and has `Decell`, `EnLesFlg`, `MasterLock`, `HandFlg`, `NCMotion`, `DelayedMasterLock` and `BrakeForPowerOff` clear. Axis error is not required to be zero because fault reset must remain recoverable. |
| 2 | `LMCRobot` and Axis1..4 are connected, profile lock is zero, all master-lock bits above are clear, and robot state is `_ROBOT_PASSIVE`, or `_ROBOT_DIRECT` with profile finished. |
| 3 | Latch drain flags are exact `0x0000001F`, the local Zero Home engine is not running or quarantined, and Diagnostics drain flags are exact `0x0000001F`. |

The exact four physical-axis status words, four `Drive.StateWord` low 16-bit values and Group signature must remain
unchanged for at least three fresh input-latch cycles and at least 100 ms of `ops.tAbsolute`
service time. Power may be on or off, but a power-state change resets stability. Repeated calls in
the same latch cycle do not increase the stable sample count.

`LMCDiagnosticsService.ProcessAxisOwnershipStartup` publishes exact drain flags:

- bit 0: the 48-byte snapshot copy, magic, cycle and reserved word are valid;
- bit 1: `Ds402HomeState[92] = 0`;
- bit 2: `EncoderMaintenanceState[152] = 0`;
- bit 3: generic SDO is neither queued nor running and `SdoInternalDrainState = 0`;
- bit 4: all four `LMCSdoExecutor` clients are connected and `IsReusable()`.

An already initialized same-BootId table is accepted only when table magic, exact startup proof,
and the global non-quarantine state remain valid. This path does not re-evaluate transient normal
operations as startup failures.

A new BootId may replace only an all-zero table or an exact prior-BootId owner table whose global
and all nine axis records are completely idle and structurally valid. Corrupt, active or
quarantined prior state returns `-3`; the reconciler never clears it as a recovery shortcut.

Return values are `0 reconciled/already reconciled`, `1 pending`, `-1 invalid input or snapshot
ABI`, and `-3 existing owner table corrupt/quarantined`. A missing, changing, moving, locked or
busy observation resets the stable sample state and remains pending without partially initializing
the owner table.

## 4. Fail-closed behavior

- Missing, stale, changing, moving, locked, active or undrained evidence leaves reconciliation
  pending and `ReserveAxisOwnership` continues to return `-3`.
- A corrupt or quarantined existing owner table is never cleared by the reconciler.
- Session close does not release an active owner.
- Feature gates and capability advertisement remain disabled until PLC download, timing
  measurement and the activation matrix are complete.

## 5. Verification

1. Static verifier rejects any remaining BootId-only `ReportAxisOwnershipStartup` caller.
2. Static verifier rejects the removed `ReportAxisOwnershipStartup` ABI and requires the exact
   replacement declarations and private call chain.
3. Negative fixtures cover partial proof, same-cycle replay, unstable axis/group state, mailbox
   busy, diagnostics busy and executor non-reusable cases.
4. LASAL C78 ARM Rebuild must finish with zero errors.
5. IDE smoke:
   - Object Network Server/Client items: run `Find in Implementation`.
   - Function/method rows: use `Edit Method` or `Enter` and confirm the exact Implementation
     header for `CopyAxisOwnershipStartupSnapshot`, `ReconcileAxisOwnershipStartup` and
     `ProcessAxisOwnershipStartup`.
6. No new `%TEMP%\Lasal2.log` `CInvalidArgException` after smoke start.
7. PLC runtime proof remains separate and is not claimed by a source/build PASS.

### 5.1 Initial startup-reconciler checkpoint completed on 2026-08-03

- LASAL Class2 02.03.001, C78 ARM `Rebuild All`: `0 error(s), 38 warning(s)`.
- The warnings are the existing C78 project versus C81 library-version family; no new compiler
  error was accepted.
- `Find in Implementation` opened all three replacement methods listed above.
- The preceding sentence preserves the original checkpoint wording; it is not a current UI test
  rule. `Find in Implementation` applies only to Object Network Server/Client items and does not
  prove that a function/method row was direct-opened.
- Smoke start: `2026-08-03T22:24:15.6494204+09:00`.
- New `%TEMP%\Lasal2.log` `CInvalidArgException` after the smoke start: `0`.
- `Verify-LasalContract.ps1 -SourceOnly -ControlServiceCheckpoint Phase5TransportClean
  -ExpectedSdoWriteAxis 1`: PASS.
- `LasalMotionControlLib.Tests` full PC suite: `1075/1075` PASS.
- Example WPF application Build: PASS.
- The four changed LASAL `.st` files remain 7-bit ASCII with CRLF line endings.
- `git diff --check` and `git diff --cached --check`: PASS.

The D5 SDO Write expectation is intentionally Axis1-only qualification state. It is not a
production or PLC runtime PASS, and the verifier must therefore receive
`-ExpectedSdoWriteAxis 1` while that qualification configuration is retained.

### 5.2 Latest ownership follow-up checkpoint

- LASAL Class2 02.03.001, C78 ARM build: `0 error(s), 40 warning(s)`.
- The warnings remain compiler/library-version warnings; this is build proof, not runtime proof.
- `Find in Implementation` smoke:
  - `LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN`: 1 match in `TCPMotionInterface`;
  - `OwnershipObserverState`: 54 matches in `LMCControlCommandService`.
- Smoke start: `2026-08-03T23:50:12.8194686+09:00`.
- New `%TEMP%\Lasal2.log` `CInvalidArgException` after that start: `0`.
- `LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED` remains `FALSE` in both
  `TCPMotionInterface` and `LMCControlCommandService`.
- `LMC_ADMIN_AXIS_HOME_ENABLED`, `LMC_DIAG_DS402_HOME_ENABLED`,
  `LMC_DIAG_ENCODER_TW20_ENABLED` and `LMC_DIAG_ENCODER_TW19_ENABLED` also remain `FALSE`.

The startup reconciler is therefore source/static/build/IDE-search proven only. There is no PLC
download, fresh-cycle timing measurement or physical-axis runtime proof in this checkpoint.

## 6. Remaining activation blockers outside the startup ABI

The BootId-only permanent quarantine defect is removed, but the following blockers keep every
feature and ordinary-ownership gate disabled:

1. A later 2026-08-04 source checkpoint added a separate safety-preemption overlay that retains the
   prior special owner's complete kind/session/raw identity. The LMC Home consumer now uses it for
   exact cancel/drain cleanup, but the edit is not yet C78- or PLC-runtime proven.
2. The ordinary terminal observer reads `_LMCAxis`/`LMCRobot` directly in the service task. It is
   not tied to a coherent InputLatch cycle and it does not observe DS402 `0x6041`.
3. Crossing the handler-entry boundary forces every non-accepted result to quarantine because no
   exact native-call marker exists. Definite pre-wire rollback cannot yet be distinguished safely.
4. Axis records retain at most 16 DINT identity words, which is not full-payload identity for large
   commands such as `0x20E7` and large moves.
5. Startup/ordinary stability `100 ms` and ordinary timeout `120000 ms` are source constants, not
   measured PLC/EtherCAT values.
6. The SDK error catalog remains version 1 and has no symbolic `-9 AxisOwnershipConflict` entry.
7. Exact safety cancellation and drain remain unresolved for DS402 Home, TW20/TW19 and Group lease
   destruction. The LMC Home source path is wired and SourceOnly-proven, but C78 and PLC concurrent
   safety/runtime proof remain unresolved.

These blockers are not defects in the replacement startup function signature itself. They are the
remaining paired activation work and must not be hidden by changing a gate alone.

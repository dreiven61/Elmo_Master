# SetOperationMode physical finding — OutcomeStorageUnavailable(49)

- Date: 2026-08-31
- Source of evidence: Axis1 live WPF qualification logs supplied after the Start execution refactor
- Status: **HOST START PATH PASS / PLC START REJECT / 0x6060 NOT REACHED**
- Production release: **NO-GO**

## 1. Reproduced physical behavior

Axis1 CSP(8) -> ProfilePosition(1) reaches the corrected host path:

```text
SetOperationMode Start UI handler entered
SetOperationMode cross-mode preflight passed
  axis=1
  currentMode=8
  requestedMode=1
  StatusWord=0x02D0
SetOperationMode final Diagnostics refreshed
SetOperationMode prepared
SetOperationMode journal armed before dispatch
```

The PLC then rejects Start definitively before mode mutation:

```text
Status=1
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

The first run reproduced this with RequestId 3, 5, 7 and 9 at BootId `0x00000066`.

After a new PLC download/restart, the same CSP -> PP attempt reproduced at:

```text
Build=1
BootId=0x00000067
MapRevision=0x957F101E
RequestId=3
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

The BootId transition `0x66 -> 0x67` confirms a new PLC boot occurred. The repeated Detail 49 makes a simple stale-running-boot explanation insufficient and shifts the investigation to the SetOperationMode ownership/outcome admission path.

No successful Start acknowledgement was observed in either boot, so there is still no evidence of any `0x6060` write.

## 2. Admission-path narrowing

The TCP path calls `LMCControlCommandService.ReserveAxisOwnership()` before invoking `LMCDiagnosticsService.HandleRequest()`.

A failed reservation is rejected by TCP before Diagnostics. A successful reservation validates nonzero caller session/sequence and returns nonzero admission token/owner generation before Diagnostics is called.

Therefore, once the request reaches `HandleAxisSetOperationModeStart()`, the previous combined Detail 49 condition hid two materially different failures:

1. invalid/zero admission identity;
2. `IsClientConnected(#AxisOwnership) = FALSE` inside `LMCDiagnosticsService`.

The repository source also has the SetOperationMode gate enabled, so the repeated fresh-boot result makes the Diagnostics-side `AxisOwnership` client binding the highest-value runtime suspect.

## 3. Concrete source inconsistency found

Before corrective commit `c670bd6fbc816116eacbe19b94199479d1a8cacf`, the embedded LASAL class metadata declared clients in this order:

```text
AxisOwnership
InputLatch
RecorderStore
...
```

but the generated ST declaration and generated class table used:

```text
InputLatch
AxisOwnership
RecorderStore
...
```

The communication network itself contains:

```text
LMCDiagnosticsService1.AxisOwnership
  -> LMCControlCommandService1.ClassSvr
```

and `ONE_Comm_Network_Table.st` contains the corresponding generated connection.

The metadata/declaration order mismatch is now corrected so the embedded LASAL declaration metadata, ST declaration and generated class-table order agree.

## 4. Detail 49 split

Corrective commit `c670bd6fbc816116eacbe19b94199479d1a8cacf` also stops hiding a disconnected owner channel behind storage Detail 49.

The SetOperationMode Start contract is now:

```text
49 = SetOperationModeOutcomeStorageUnavailable
     gate/storage/invalid ownership-admission identity

52 = SetOperationModeOwnershipChannelUnavailable
     LMCDiagnosticsService AxisOwnership client is not connected at runtime
```

The SDK enum and error catalog expose Detail 52 explicitly. No safety admission was removed or relaxed.

Permanent static verification now checks:

- Detail 52 remains defined;
- LASAL metadata client order remains aligned with generated declaration order;
- invalid admission identity and disconnected AxisOwnership remain distinct failure paths.

The corrective workflow validation passed SetOperationMode static qualification and SDK Debug/Release builds before committing the change to `dev`.

## 5. Next physical discriminator

Use exact current `dev`, regenerate/rebuild/link the LASAL C78/ARM image and download that image to the PLC. Rebuild the WPF/SDK as well so Detail 52 has its symbolic name.

Run Axis1 CSP -> PP once.

Interpret the result exactly:

- **Start accepted / lifecycle continues:** the client metadata correction changed the binding path; continue through exact `0x6060`/`0x6061` physical qualification.
- **Detail 52 `SetOperationModeOwnershipChannelUnavailable`:** runtime `LMCDiagnosticsService1.AxisOwnership` connectivity is confirmed as the blocker. Do not weaken ownership; the next correction must be the LASAL channel/server wiring or generated artifact.
- **Detail 49 remains:** the failure is not the `AxisOwnership` connectivity branch anymore; inspect gate/admission identity/storage evidence rather than changing SDO or safety rules.

## 6. Safety boundaries retained

This finding and correction do not authorize any of the following:

- disabling current-observation freshness;
- allowing cross-mode change while DS402 OperationEnabled;
- removing Standstill/Fault checks;
- bypassing retained outcome storage;
- bypassing axis ownership validation;
- replaying `0x7D23` after uncertain/accepted Start;
- raw Generic SDO write to `0x6060`.

The next physical PASS requires exact one-write/no-replay evidence, not merely a changed UI value.

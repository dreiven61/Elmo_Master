# SetOperationMode physical finding — OutcomeStorageUnavailable(49)

- Date: 2026-08-31
- Source of evidence: Axis1 live WPF qualification log supplied after the Start execution refactor
- Software Start-path baseline: `d4ce1b2f9c2a41f5117e0bd769533d0483c1ff91`
- Status: **HOST START PATH PASS / PLC START REJECT / 0x6060 NOT REACHED**
- Production release: **NO-GO**

## 1. Observed runtime sequence

The live Axis1 CSP(8) -> ProfilePosition(1) attempt reached the corrected host path:

```text
SetOperationMode Start UI handler entered
SetOperationMode cross-mode preflight passed
  axis=1
  currentMode=8
  requestedMode=1
  StatusWord=0x02D0
SetOperationMode final Diagnostics refreshed
  Build=1
  BootId=0x00000066
  MapRevision=0x957F101E
SetOperationMode prepared
SetOperationMode journal armed before dispatch
```

The PLC then rejected Start definitively:

```text
Status=1
ErrorId=-31000
Detail=SetOperationModeOutcomeStorageUnavailable(49)
```

The same rejection reproduced with RequestId 3, 5, 7 and 9.

No successful Start acknowledgement was observed, and the host correctly archived each definitive rejection without automatic Start replay.

## 2. What this proves

This run closes the previous host-side stale Diagnostics observation blocker. The final Diagnostics observation is now accepted by `PrepareSetOperationMode()` and the durable pre-dispatch journal arms correctly.

This run does **not** prove any actual DS402 mode mutation. The PLC rejects `0x7D23` before the SetOperationMode lifecycle reaches its retained-outcome/mutation path, so there is no evidence that `0x6060` was written. The subsequent successful manual `0x6061` reads only confirm that the read path is healthy and the drive remains observable.

## 3. Current Detail 49 source conditions

Current `LMCDiagnosticsService.st` uses detail 49 for SetOperationMode storage/admission infrastructure failures. Relevant conditions include:

1. `LMC_DIAG_SET_OPERATION_MODE_ENABLED = FALSE` in the loaded PLC image.
2. missing/zero ownership admission identity (`CallerSessionEpoch`, `RequestSequence`, `AdmissionToken`, `OwnerGeneration`).
3. `IsClientConnected(#AxisOwnership) = FALSE` at LMCDiagnosticsService runtime.

The TCP path calls `LMCControlCommandService.ReserveAxisOwnership()` before invoking Diagnostics. A failed reservation is returned by the TCP layer before Diagnostics. A successful reservation validates the caller/session sequence and issues nonzero admission token / owner generation. Therefore, for a loaded image whose SetOperationMode gate is actually TRUE, the remaining high-value runtime check is the `LMCDiagnosticsService1.AxisOwnership` client connection.

Do not bypass this ownership/outcome requirement. It is part of the exactly-once/no-replay safety contract.

## 4. Current repository source truth

Current `dev` source contains:

```text
LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE
LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE
Admin feature mask = 0x00000717
SetOperationModeSupportedMask = 0x018A
```

Current network source contains:

```text
LMCDiagnosticsService1.AxisOwnership
  -> LMCControlCommandService1.ClassSvr
```

The generated `ONE_Comm_Network_Table.st` also contains the corresponding generated AxisOwnership connection.

Therefore the live Detail 49 cannot be resolved safely by changing the host Start path or by weakening the SetOperationMode safety fence. The exact PLC-loaded C78/ARM image must first be tied to the current source/generation state.

## 5. Corrective qualification sequence

Before another mode-change attempt:

1. use exact current `dev` source;
2. perform a fresh LASAL IDE C78/ARM Rebuild + Link;
3. run `tools/Capture-SetOperationModeC78Evidence.ps1` with the fresh build log and build start time;
4. confirm the collector reports qualification gate ON and AxisOwnership present in both source and generated network table;
5. download/load that exact fresh artifact to the PLC;
6. reconnect WPF and record new Diagnostics Build / BootId / MapRevision;
7. retry Axis1 CSP -> PP once.

Expected discriminator:

- if Start proceeds beyond admission, next evidence must show accepted Start/outcome processing and eventually `0x6060=1` / `0x6061=1` or a later explicit lifecycle failure;
- if the exact fresh image still returns detail 49, treat `LMCDiagnosticsService1.AxisOwnership` runtime connectivity/ownership admission as the next PLC-side defect to instrument/fix.

## 6. Safety boundaries retained

This finding does not authorize any of the following:

- disabling `requireCurrentObservation=true`;
- allowing cross-mode change while DS402 OperationEnabled;
- removing Standstill/Fault checks;
- bypassing retained outcome storage;
- replaying `0x7D23` after uncertain/accepted Start;
- raw Generic SDO write to `0x6060`.

The next physical PASS requires exact one-write/no-replay evidence, not merely a changed UI value.

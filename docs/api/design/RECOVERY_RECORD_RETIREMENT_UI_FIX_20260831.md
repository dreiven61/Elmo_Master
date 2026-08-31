# Stale Recovery Retirement UI Fix (2026-08-31)

## Observed defect

The stale-recovery panel listed an `AxisSetOperationMode` record after the PLC
BootId changed. Readiness was READY and the acknowledgement was checked, but the
archive button was disabled. The deferred `DispatcherPriority.ContextIdle`
global interlock traversed every button and disabled the retirement button,
which was missing from `IsAllowedDuringAxisSetOperationModeRecovery`.

Earlier diagnoses blaming other journals or WPF Checked/Unchecked events were
not supported by evidence. Those speculative changes are withdrawn.

## Correction

1. Allow the retirement button through the SetOperationMode interlock only in
   the current recovery-identity quarantine. The same predicate covers deferred
   disabling and mouse/keyboard interception; acknowledgement and admission
   checks remain in force.
2. Include SetOperationMode original bytes in
   `CaptureStaleRecoveryRetirementEvidence`, not just the displayed metadata.
3. Save operator archival as `OperatorRetired=5`, an inactive journal state,
   instead of `Resolved=3`. Resolved still requires actual terminal and PLC
   retirement proof. Operator retirement requires an immutable committed ledger
   decision matching the exact source bytes and does not fabricate PLC proof.
4. Initialize the retirement ledger before the read-only API opens the
   SetOperationMode journal. This allows a committed decision to finish safely
   at startup after interruption between ledger commit and journal update.
5. Keep UI preparation and execution infrastructure checks consistent via
   `RecoveryRetirementSourceJournalsUnavailable`, including SetOperationMode.

The panel retains the `Retirement readiness` summary. Original Checked/Unchecked
events are restored and verified by the regression test without synthetic clicks.

## Safety boundary

Archive/retire still sends no Motion, Power, SDO, Write, replay, or cleanup
command. Read-only capabilities queries and RPC close remain permitted. It
archives exact source bytes, commits the immutable decision, retires only the
listed stale record, closes the quarantined connection, and requires an
application restart. Previous command outcomes remain unknown; source bytes are
not deleted. Exact-current and other-endpoint records are not retired.

Existing journal state values 1..4 and the binary field layout remain unchanged.
Older programs that do not recognize OperatorRetired=5 reject that journal on
load; use the updated executable after retirement.

## Verification

- Reproduced the defect before correction: the integration test fails after
  draining ContextIdle because the archive button becomes disabled.
- Corrected test passes through acknowledgement, deferred interlock, archival,
  disconnect, journal reopen and a newly constructed application window. Original
  bytes in the ledger match exactly; there is no fabricated terminal proof or
  PLC retirement request ID. Recorded RPC commands exclude mutation and replay.
- `Wpf.RecoveryRetirement.`: 22/22 PASS, including interrupted commit startup.
- `Wpf.AxisSetOperationModeJournal.`: 8/8 PASS, including rejection of missing
  ledger decisions and source changes after commitment.
- `Wpf.SetOperationModeRecovery.`: 7/7 PASS.
- Broader SetOperationMode filtering also finds one existing Korean label test
  failure (`Wpf.UiLocalization.SetOperationMode.HomeDS402ExRecentRecoveryLabelsRoundTrip`).
  It was reproduced with the pre-fix binary in `_buildcheck/retirement-click`.
  This is not a full regression PASS.
- Visual Studio 2019 MSBuild and git diff whitespace checks pass. Tests use an
  isolated local fake PLC and temporary journals, not the user's real records.
- The normal Debug executable is still running and has not been overwritten.
  A ready-to-run build is provided in the same project at `bin/RecoveryFix`.
  Real operator archival remains to be performed by the user; no LASAL source,
  PLC download, or machine state was changed for this fix.

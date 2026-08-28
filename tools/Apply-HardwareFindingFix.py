from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WPF_OPMODE = ROOT / 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs'

old = '''            var triad = HasAxisSetOperationModeCapabilityTriad();\n            var diagnostics = HasStableAxisSetOperationModeDiagnosticsIdentity();\n            textAxisSetOperationModeRecoveryStatus.Text =\n                "Journal ready; no unresolved record. AdminTriad="\n                + triad\n                + ", SupportedModeMask=0x"\n                + (adminCapabilities == null\n                    ? 0\n                    : adminCapabilities.SetOperationModeSupportedMask)\n                    .ToString("X4")\n                + ", DiagnosticsIdentity="\n                + diagnostics\n                + ". Current PLC activation is expected to keep Start disabled until bits 8/9/10 are explicitly enabled after MODE-13 evidence passes.";\n'''

new = '''            var triad = HasAxisSetOperationModeCapabilityTriad();\n            var diagnostics = HasStableAxisSetOperationModeDiagnosticsIdentity();\n            var confirmed = checkAxisSetOperationModeOneShotConfirmed != null\n                && checkAxisSetOperationModeOneShotConfirmed.IsChecked == true;\n            var selectedModeAdvertised =\n                comboAxisSetOperationModeRequestedMode != null\n                && comboAxisSetOperationModeRequestedMode.SelectedItem\n                    is LMCDriveOperationMode\n                && adminCapabilities != null\n                && adminCapabilities.SupportsSetOperationMode(\n                    (LMCDriveOperationMode)\n                        comboAxisSetOperationModeRequestedMode.SelectedItem);\n            var admissionAllowed = EvaluateDiagnosticsAdmission(\n                DiagnosticsAdmissionOperation.NewLiveOrMutation).IsAllowed;\n            textAxisSetOperationModeRecoveryStatus.Text =\n                "Journal ready; no unresolved record. AdminTriad="\n                + triad\n                + ", SupportedModeMask=0x"\n                + (adminCapabilities == null\n                    ? 0\n                    : adminCapabilities.SetOperationModeSupportedMask)\n                    .ToString("X4")\n                + ", DiagnosticsIdentity="\n                + diagnostics\n                + ", Confirmed="\n                + confirmed\n                + ", SelectedModeAdvertised="\n                + selectedModeAdvertised\n                + ", AdmissionAllowed="\n                + admissionAllowed\n                + ", JournalReady="\n                + AxisSetOperationModeRecoveryJournalCanArm\n                + ". Start is enabled only when every displayed gate and the axis/timeout/idle gates are true.";\n'''

text = WPF_OPMODE.read_text(encoding='utf-8')
if new in text:
    print('Operation Mode live gate status already applied.')
elif old in text:
    WPF_OPMODE.write_text(text.replace(old, new), encoding='utf-8')
    print('Applied Operation Mode live gate status diagnostics.')
else:
    raise RuntimeError('Operation Mode stale status block not found.')

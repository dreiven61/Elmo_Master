from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
AXIS = ROOT / "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs"
READONLY = ROOT / "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs"
TESTS = ROOT / "LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisSetOperationModeRecoveryIntegrationTests.cs"
WORKFLOW = ROOT / ".github/workflows/set-operation-mode-wpf-recovery.yml"
VERIFIER = ROOT / "tools/Verify-SetOperationModeStartExecution.ps1"


def read(path):
    return path.read_text(encoding="utf-8")


def write(path, text):
    path.write_text(text, encoding="utf-8", newline="\n")


def replace_once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one match, found {count}")
    return text.replace(old, new, 1)


# 1) Remove runtime detach/rebind and obsolete UI handler from ReadOnlyApi.
readonly = read(READONLY)
readonly = replace_once(
    readonly,
    """            InitializeAxisSetOperationModeRecoveryUi(\n                physicalAxisReferences);\n            buttonStartAxisSetOperationMode.Click -=\n                ButtonStartAxisSetOperationMode_Click;\n            buttonStartAxisSetOperationMode.Click +=\n                ButtonStartAxisSetOperationModeWithRejectResolution_Click;\n""",
    """            InitializeAxisSetOperationModeRecoveryUi(\n                physicalAxisReferences);\n""",
    "remove SetOperationMode Start detach/rebind",
)
obsolete_start = readonly.index(
    "        private async void\n            ButtonStartAxisSetOperationModeWithRejectResolution_Click("
)
obsolete_end = readonly.index(
    "        internal string ResolveAxisSetOperationModeDefinitiveRejectionForTests(",
    obsolete_start,
)
readonly = readonly[:obsolete_start] + readonly[obsolete_end:]
write(READONLY, readonly)

# 2) Make ButtonStartAxisSetOperationMode_Click the single canonical handler.
axis = read(AXIS)
axis = replace_once(
    axis,
    """        private bool axisSetOperationModeUiInterlockHooked;\n        private bool axisSetOperationModeInterlockReapplyQueued;\n""",
    """        private bool axisSetOperationModeUiInterlockHooked;\n        private bool axisSetOperationModeInterlockReapplyQueued;\n        private int axisSetOperationModeStartUiHandlerEntryCount;\n""",
    "add canonical handler test counter",
)
axis = replace_once(
    axis,
    """        internal Button AxisSetOperationModeStartButtonForTests\n        {\n            get { return buttonStartAxisSetOperationMode; }\n        }\n\n""",
    """        internal Button AxisSetOperationModeStartButtonForTests\n        {\n            get { return buttonStartAxisSetOperationMode; }\n        }\n\n        internal int AxisSetOperationModeStartUiHandlerEntryCountForTests\n        {\n            get { return axisSetOperationModeStartUiHandlerEntryCount; }\n        }\n\n        internal void RaiseAxisSetOperationModeStartClickForTests()\n        {\n            buttonStartAxisSetOperationMode.RaiseEvent(\n                new RoutedEventArgs(Button.ClickEvent));\n        }\n\n""",
    "expose canonical handler smoke hook",
)
old_handler = """        private async void ButtonStartAxisSetOperationMode_Click(\n            object sender,\n            RoutedEventArgs e)\n        {\n            await RunOperationAsync(\n                \"Set Operation Mode Selected Mode Once\",\n                StartAxisSetOperationModeOnceAsync);\n        }\n"""
new_handler = """        private async void ButtonStartAxisSetOperationMode_Click(\n            object sender,\n            RoutedEventArgs e)\n        {\n            axisSetOperationModeStartUiHandlerEntryCount++;\n            WriteLog(\"SetOperationMode Start UI handler entered.\");\n            await RunOperationAsync(\n                \"Set Operation Mode Selected Mode Once\",\n                async () =>\n                {\n                    try\n                    {\n                        await StartAxisSetOperationModeOnceAsync();\n                    }\n                    catch (LMCAxisSetOperationModeRejectedException error)\n                    {\n                        var record =\n                            RequireActiveAxisSetOperationModeRecoveryRecord(\n                                \"definitive SetOperationMode Start rejection\");\n                        var evidencePath =\n                            ResolveDefinitiveAxisSetOperationModeStartRejection(\n                                record,\n                                error.Acknowledgement.PreparedCommand.RecoveryKey,\n                                error.Response.SchemaVersion,\n                                error.Response.CommandStatus,\n                                error.Response.ErrorId,\n                                error.Response.RequestId,\n                                error.Response.DetailCodeValue,\n                                error.Response.IsSuccess,\n                                DateTime.UtcNow);\n                        RefreshAxisSetOperationModeRecoveryUi(\n                            \"START REJECTED DEFINITIVELY: \"\n                            + error.Response.DetailCode\n                            + \". PLC rejected the request before creating a retained SetOperationMode outcome. \"\n                            + \"The rejection and original pre-dispatch journal were archived durably at \"\n                            + evidencePath\n                            + \"; the recovery interlock is cleared. A future Start requires a new explicit confirmation and new identity.\");\n                        UpdateUiState();\n                        throw;\n                    }\n                });\n        }\n"""
axis = replace_once(axis, old_handler, new_handler, "replace canonical Start handler")

# 3) Move the final Diagnostics observation after preflight and pass that exact local to Prepare.
old_order = """            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);\n            EnsureAxisSetOperationModeCapabilitiesReady(\n                \"SetOperationMode Start\");\n\n            var currentAxis = await GetPhysicalAxisAsync(axisReference);\n            await VerifyAxisSetOperationModeTransitionPreflightAsync(\n                currentAxis,\n                requestedMode);\n            var prepared = currentAxis.PrepareSetOperationMode(\n                requestedMode,\n                timeoutMilliseconds,\n                adminCapabilities,\n                diagnosticCapabilities,\n                LMCAxisSetOperationModeExecuteToken.Create());\n"""
new_order = """            var currentAxis = await GetPhysicalAxisAsync(axisReference);\n            await VerifyAxisSetOperationModeTransitionPreflightAsync(\n                currentAxis,\n                requestedMode);\n\n            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);\n            EnsureAxisSetOperationModeCapabilitiesReady(\n                \"SetOperationMode Start\");\n            var finalDiagnosticCapabilities = diagnosticCapabilities;\n            if (finalDiagnosticCapabilities == null)\n            {\n                throw new InvalidOperationException(\n                    \"SetOperationMode final Diagnostics observation is unavailable. No Start was sent.\");\n            }\n            WriteLog(\n                \"SetOperationMode final Diagnostics refreshed: Build=\"\n                + finalDiagnosticCapabilities.DiagnosticsBuild.ToString(\n                    CultureInfo.InvariantCulture)\n                + \", BootId=0x\"\n                + finalDiagnosticCapabilities.DiagnosticsBootId.ToString(\n                    \"X8\",\n                    CultureInfo.InvariantCulture)\n                + \", MapRevision=0x\"\n                + finalDiagnosticCapabilities.MapRevision.ToString(\n                    \"X8\",\n                    CultureInfo.InvariantCulture)\n                + \".\");\n\n            var prepared = currentAxis.PrepareSetOperationMode(\n                requestedMode,\n                timeoutMilliseconds,\n                adminCapabilities,\n                finalDiagnosticCapabilities,\n                LMCAxisSetOperationModeExecuteToken.Create());\n            WriteLog(\n                \"SetOperationMode prepared: RequestId=\"\n                + prepared.RequestId.ToString(CultureInfo.InvariantCulture)\n                + \", ClientIntentId=\"\n                + prepared.RecoveryKey.ClientIntentId0.ToString(\"X8\", CultureInfo.InvariantCulture)\n                + \"-\"\n                + prepared.RecoveryKey.ClientIntentId1.ToString(\"X8\", CultureInfo.InvariantCulture)\n                + \"-\"\n                + prepared.RecoveryKey.ClientIntentId2.ToString(\"X8\", CultureInfo.InvariantCulture)\n                + \"-\"\n                + prepared.RecoveryKey.ClientIntentId3.ToString(\"X8\", CultureInfo.InvariantCulture)\n                + \".\");\n"""
axis = replace_once(axis, old_order, new_order, "move final Diagnostics refresh after preflight")
axis = replace_once(
    axis,
    """            var record = axisSetOperationModeRecoveryJournal\n                .ArmBeforeDispatch(\n                    Guid.NewGuid(),\n                    RequiredConnectedRemoteIp(),\n                    RequiredConnectedRemotePort(),\n                    currentAxis.AxisName,\n                    prepared.RecoveryKey,\n                    DateTime.UtcNow);\n            checkAxisSetOperationModeOneShotConfirmed.IsChecked = false;\n""",
    """            var record = axisSetOperationModeRecoveryJournal\n                .ArmBeforeDispatch(\n                    Guid.NewGuid(),\n                    RequiredConnectedRemoteIp(),\n                    RequiredConnectedRemotePort(),\n                    currentAxis.AxisName,\n                    prepared.RecoveryKey,\n                    DateTime.UtcNow);\n            WriteLog(\n                \"SetOperationMode journal armed before dispatch: Identity=\"\n                + record.Identity.ToString(\"N\")\n                + \", RequestId=\"\n                + record.OriginalRequestId.ToString(CultureInfo.InvariantCulture)\n                + \".\");\n            checkAxisSetOperationModeOneShotConfirmed.IsChecked = false;\n""",
    "log journal arm phase",
)
axis = replace_once(
    axis,
    """                var acknowledgement = await currentAxis\n                    .SetOperationModeAsync(\n                        prepared,\n                        CancellationToken.None);\n                if (acknowledgement == null\n""",
    """                var acknowledgement = await currentAxis\n                    .SetOperationModeAsync(\n                        prepared,\n                        CancellationToken.None);\n                WriteLog(\n                    \"SetOperationMode 0x7D23 dispatch boundary crossed once: RequestId=\"\n                    + prepared.RequestId.ToString(CultureInfo.InvariantCulture)\n                    + \".\");\n                if (acknowledgement == null\n""",
    "log accepted dispatch boundary",
)
write(AXIS, axis)

# 4) Add a WPF event-wiring regression.
tests = read(TESTS)
tests = replace_once(
    tests,
    """            tests.Add(\n                \"Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation\",\n                SetOperationModeDynamicUiRequiresExplicitConfirmation);\n""",
    """            tests.Add(\n                \"Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation\",\n                SetOperationModeDynamicUiRequiresExplicitConfirmation);\n            tests.Add(\n                \"Wpf.SetOperationModeRecovery.CanonicalStartClickUsesSingleHandler\",\n                SetOperationModeCanonicalStartClickUsesSingleHandler);\n""",
    "register canonical Start click smoke",
)
anchor = """        private static void SetOperationModeSelectorRemainsUsableWithoutPlcMask()\n"""
new_test = """        private static void SetOperationModeCanonicalStartClickUsesSingleHandler()\n        {\n            var root = CreateSetOperationModeTemporaryDirectory();\n            MainWindow window = null;\n            try\n            {\n                window = new MainWindow(root);\n                AssertEx.Equal(\n                    0,\n                    window.AxisSetOperationModeStartUiHandlerEntryCountForTests);\n\n                window.RaiseAxisSetOperationModeStartClickForTests();\n\n                AssertEx.Equal(\n                    1,\n                    window.AxisSetOperationModeStartUiHandlerEntryCountForTests);\n            }\n            finally\n            {\n                if (window != null)\n                {\n                    window.Close();\n                }\n                DeleteSetOperationModeTemporaryDirectory(root);\n            }\n        }\n\n"""
if anchor not in tests:
    raise RuntimeError("canonical Start click smoke insertion anchor not found")
tests = tests.replace(anchor, new_test + anchor, 1)
write(TESTS, tests)

# 5) Add a permanent source-order verifier.
verifier = r'''param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$axisPath = Join-Path $repoRoot 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.AxisSetOperationModeRecovery.cs'
$readOnlyPath = Join-Path $repoRoot 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.ReadOnlyApi.cs'
$testPath = Join-Path $repoRoot 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/WpfAxisSetOperationModeRecoveryIntegrationTests.cs'

$axis = [IO.File]::ReadAllText($axisPath)
$readOnly = [IO.File]::ReadAllText($readOnlyPath)
$tests = [IO.File]::ReadAllText($testPath)

function Require-Contains([string]$text, [string]$needle, [string]$message) {
    if (-not $text.Contains($needle)) { throw $message }
}
function Require-NotContains([string]$text, [string]$needle, [string]$message) {
    if ($text.Contains($needle)) { throw $message }
}

$subscription = 'buttonStartAxisSetOperationMode.Click +=`r`n            ButtonStartAxisSetOperationMode_Click;'
$count = ([regex]::Matches($axis, 'buttonStartAxisSetOperationMode\.Click \+=\s*ButtonStartAxisSetOperationMode_Click;')).Count
if ($count -ne 1) { throw "Expected exactly one canonical Start Click subscription, found $count." }
Require-NotContains $readOnly 'ButtonStartAxisSetOperationModeWithRejectResolution_Click' 'Obsolete SetOperationMode reject-resolution UI handler still exists.'
Require-NotContains $readOnly 'buttonStartAxisSetOperationMode.Click -=' 'ReadOnlyApi still detaches the SetOperationMode Start handler.'
Require-Contains $axis 'ResolveDefinitiveAxisSetOperationModeStartRejection(' 'Canonical handler did not preserve definitive rejection resolution.'
Require-Contains $tests 'Wpf.SetOperationModeRecovery.CanonicalStartClickUsesSingleHandler' 'Canonical Start Click smoke test is missing.'

$startMethod = $axis.IndexOf('        private async Task StartAxisSetOperationModeOnceAsync()')
$recoverMethod = $axis.IndexOf('        private async void ButtonRecoverAxisSetOperationMode_Click(', $startMethod)
if ($startMethod -lt 0 -or $recoverMethod -lt 0) { throw 'Could not isolate StartAxisSetOperationModeOnceAsync().' }
$start = $axis.Substring($startMethod, $recoverMethod - $startMethod)

$preflight = $start.IndexOf('await VerifyAxisSetOperationModeTransitionPreflightAsync(')
$finalRefresh = $start.IndexOf('await RefreshDiagnosticsCapabilitiesAsync(currentConnection);', $preflight)
$finalLocal = $start.IndexOf('var finalDiagnosticCapabilities = diagnosticCapabilities;', $finalRefresh)
$prepare = $start.IndexOf('var prepared = currentAxis.PrepareSetOperationMode(', $finalLocal)
$arm = $start.IndexOf('.ArmBeforeDispatch(', $prepare)
$dispatch = $start.IndexOf('.SetOperationModeAsync(', $arm)
if ($preflight -lt 0 -or $finalRefresh -lt 0 -or $finalLocal -lt 0 -or $prepare -lt 0 -or $arm -lt 0 -or $dispatch -lt 0) {
    throw 'SetOperationMode Start phase markers are incomplete.'
}
if (-not ($preflight -lt $finalRefresh -and $finalRefresh -lt $finalLocal -and $finalLocal -lt $prepare -and $prepare -lt $arm -and $arm -lt $dispatch)) {
    throw 'SetOperationMode Start phase ordering is not preflight -> final Diagnostics -> Prepare -> arm -> dispatch.'
}
$betweenFinalAndPrepare = $start.Substring($finalRefresh, $prepare - $finalRefresh)
if ([regex]::Matches($betweenFinalAndPrepare, 'RefreshDiagnosticsCapabilitiesAsync').Count -ne 1) {
    throw 'Final Diagnostics -> Prepare slice contains an unexpected Diagnostics refresh.'
}
if ($betweenFinalAndPrepare.Contains('ReadDriveStatusAsync') -or $betweenFinalAndPrepare.Contains('GetCapabilitiesAsync')) {
    throw 'A capability-producing/read preflight call exists between final Diagnostics refresh and Prepare.'
}
$prepareSlice = $start.Substring($prepare, [Math]::Min(500, $start.Length - $prepare))
Require-Contains $prepareSlice 'finalDiagnosticCapabilities' 'PrepareSetOperationMode does not use the final Diagnostics observation local.'
if ([regex]::Matches($start, '\.SetOperationModeAsync\(').Count -ne 1) {
    throw 'StartAxisSetOperationModeOnceAsync must contain exactly one SetOperationModeAsync dispatch.'
}
Require-Contains $start 'SetOperationMode final Diagnostics refreshed:' 'Final Diagnostics phase log is missing.'
Require-Contains $start 'SetOperationMode journal armed before dispatch:' 'Journal arm phase log is missing.'
Require-Contains $start 'SetOperationMode 0x7D23 dispatch boundary crossed once:' 'Dispatch boundary phase log is missing.'

Write-Host 'PASS SetOperationMode Start execution: single UI handler, preflight -> final Diagnostics -> Prepare adjacency, durable arm, exactly-once dispatch.'
'''
write(VERIFIER, verifier)

# 6) Wire permanent verifier into the existing SetOperationMode WPF workflow.
workflow = read(WORKFLOW)
workflow = replace_once(
    workflow,
    """      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs'\n      - '.github/workflows/set-operation-mode-wpf-recovery.yml'\n""",
    """      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs'\n      - 'tools/Verify-SetOperationModeStartExecution.ps1'\n      - '.github/workflows/set-operation-mode-wpf-recovery.yml'\n""",
    "add verifier PR path",
)
workflow = replace_once(
    workflow,
    """      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs'\n      - '.github/workflows/set-operation-mode-wpf-recovery.yml'\n""",
    """      - 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/Program.cs'\n      - 'tools/Verify-SetOperationModeStartExecution.ps1'\n      - '.github/workflows/set-operation-mode-wpf-recovery.yml'\n""",
    "add verifier push path",
)
workflow = replace_once(
    workflow,
    """      - name: Locate MSBuild\n""",
    """      - name: Verify SetOperationMode Start execution contract\n        shell: powershell\n        run: |\n          & '.\\tools\\Verify-SetOperationModeStartExecution.ps1'\n          if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }\n\n      - name: Locate MSBuild\n""",
    "run permanent Start execution verifier",
)
write(WORKFLOW, workflow)

print("Applied SetOperationMode Start execution refactor and regression guards.")

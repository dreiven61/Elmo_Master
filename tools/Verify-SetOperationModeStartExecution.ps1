param()

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

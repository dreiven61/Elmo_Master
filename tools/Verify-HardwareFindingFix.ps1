param(
    [string]$RepositoryRoot = ""
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
}

$wpfDiagnosticsPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Diagnostics.cs'
$wpfSdoPath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Qualification.Sdo.cs'
$wpfModePath = Join-Path $RepositoryRoot 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.AxisSetOperationModeRecovery.cs'
$plcDiagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$plcControlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'

$wpfDiagnostics = Get-Content -LiteralPath $wpfDiagnosticsPath -Raw
$wpfSdo = Get-Content -LiteralPath $wpfSdoPath -Raw
$wpfMode = Get-Content -LiteralPath $wpfModePath -Raw
$plcDiagnostics = Get-Content -LiteralPath $plcDiagnosticsPath -Raw
$plcControl = Get-Content -LiteralPath $plcControlPath -Raw

function Require-Match([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -notmatch $Pattern) { throw $Message }
}
function Require-Count([string]$Text, [string]$Pattern, [int]$Expected, [string]$Message) {
    $count = ([regex]::Matches($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)).Count
    if ($count -ne $Expected) { throw "$Message Expected=$Expected Actual=$count" }
}

# Generic SDO ordinary-write runtime safety is distinct from the stricter
# qualification path. Qualification stays PowerOff by default.
Require-Match $wpfSdo 'bool requirePowerOff = true' `
    'D5 qualification helper no longer preserves strict PowerOff as the default.'
Require-Match $wpfSdo 'requirePowerOff && latestStatus\.IsPowerOn' `
    'D5 qualification helper does not preserve the PowerOff gate for qualification.'
Require-Match $wpfSdo 'ds402Fault' `
    'D5 safety helper does not reject DS402 Fault.'
Require-Match $wpfSdo 'ds402OperationEnabled' `
    'D5 safety helper does not reject DS402 Operation Enabled.'
Require-Count $wpfDiagnostics 'CancellationToken\.None,\s*false\);' 2 `
    'Ordinary SDO Write did not opt into the relaxed runtime-safe preflight exactly twice.'

# PLC must allow only non-enabled safe DS402 base states for generic non-semantic
# objects. The existing R03 verifier separately proves semantic blocklist coverage.
Require-Match $plcDiagnostics '(?s)\(statusWord and 0x0000006F\) <> 0x00000040.*?\(statusWord and 0x0000006F\) <> 0x00000021.*?\(statusWord and 0x0000006F\) <> 0x00000023' `
    'PLC generic SDO Write is not limited to SwitchOnDisabled/ReadyToSwitchOn/SwitchedOn.'
Require-Match $plcDiagnostics '(?s)\(ObjectIndex = 0x6040\) \| \(ObjectIndex = 0x6060\).*?\(ObjectIndex = 0x607A\).*?\(ObjectIndex = 0x60FF\).*?\(ObjectIndex = 0x6071\).*?\(ObjectIndex = 0x3204\).*?\(ObjectIndex = 0x20FC\)' `
    'PLC semantic/dedicated-owner raw-write blocklist was weakened.'

# SetOperationMode: current target may be a no-write completion; real transition
# must be explicitly preflighted before the durable one-shot Start is prepared.
Require-Match $wpfMode 'VerifyAxisSetOperationModeTransitionPreflightAsync' `
    'SetOperationMode WPF transition preflight is missing.'
Require-Match $wpfMode 'driveStatus\.OperationMode == requestedMode' `
    'SetOperationMode same-target no-write distinction is missing.'
Require-Match $wpfMode 'SucceededNoWrite; this does not prove a 0x6060 cross-mode Write' `
    'SetOperationMode no-write evidence warning is missing.'
Require-Match $wpfMode 'DS402 Fault=False, and OperationEnabled=False' `
    'SetOperationMode cross-mode preflight does not expose the required safe state.'
Require-Match $wpfMode '(?s)GetPhysicalAxisAsync\(axisReference\).*?VerifyAxisSetOperationModeTransitionPreflightAsync\(\s*currentAxis,\s*requestedMode\).*?PrepareSetOperationMode' `
    'SetOperationMode preflight is not ordered before Prepare/Start.'

# Keep multi-mode software target support and the PLC-side safety fence. Do not
# turn this corrective tranche into an unsafe OperationEnabled 0x6060 write.
Require-Match $plcDiagnostics '#define LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE' `
    'SetOperationMode PP/PV/IP software target gate is not enabled.'
Require-Match $plcControl '0x0000018A' `
    'SetOperationMode advertised qualification mask is not PP/PV/IP/CSP (0x018A).'
Require-Match $plcDiagnostics '(?s)requestedMode <> 8.*?requestedMode <> 1.*?requestedMode <> 3.*?requestedMode <> 7' `
    'SetOperationMode PLC admission does not retain PP/PV/IP/CSP targets.'
Require-Match $plcDiagnostics '\(selectedStatusWord and LMC_DIAG_MODE_DS402_OPERATION_ENABLED\) = 0' `
    'SetOperationMode PLC cross-mode OperationEnabled safety fence was weakened.'

Write-Host 'PASS hardware-finding corrective source contract'

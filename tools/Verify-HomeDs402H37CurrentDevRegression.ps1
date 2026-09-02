param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$script:CheckCount = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "FAIL H37 current-dev regression: $Message"
    }
    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True ([regex]::IsMatch($Text, $Pattern)) $Message
}

function Assert-NoMatch {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True (-not [regex]::IsMatch($Text, $Pattern)) $Message
}

function Read-SourceText {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing source file: $RelativePath"
    }
    return Get-Content -LiteralPath $path -Raw
}

function Invoke-Verifier {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    & $path -RepositoryRoot $RepositoryRoot
    # The nested verifier scripts throw on failure and intentionally do not call
    # exit on success, so LASTEXITCODE is not a valid success signal here.
    if (-not $?) {
        throw "Nested verifier failed: $RelativePath"
    }
    $script:CheckCount++
    Write-Host "PASS nested verifier $RelativePath"
}

# Re-run the frozen H37 software/source contracts on the current dev tree.
Invoke-Verifier 'tools/Verify-HomeDs402H37Activation.ps1'
Invoke-Verifier 'tools/Verify-HomeDs402H37Ownership.ps1'
Invoke-Verifier 'tools/Verify-HomeDs402H37MethodSize.ps1'
Invoke-Verifier 'tools/Verify-HomeDs402H37WpfRecovery.ps1'

$diagnosticsPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$wpfPath = 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/MainWindow.MaintenanceActions.cs'
$diagnostics = Read-SourceText $diagnosticsPath
$wpf = Read-SourceText $wpfPath

$homeBlockMatch = [regex]::Match(
    $diagnostics,
    '(?s)FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?END_FUNCTION')
Assert-True $homeBlockMatch.Success 'ProcessAxisDs402Home source block exists on current dev'
$home = $homeBlockMatch.Value

# Preserve the dedicated Method-37 SDO sequence. These checks intentionally do not
# authorize the feature; they only prove the SDO Write refactor did not rewrite H37.
Assert-Match $home 'sdoIndex\s*:=\s*0x6061\s*;' 'H37 still reads Modes of operation display (0x6061)'
Assert-Match $home '(?s)sdoIsWrite\s*:=\s*TRUE\s*;\s*sdoIndex\s*:=\s*0x607C\s*;' 'H37 still writes Home offset (0x607C) through its dedicated lifecycle'
Assert-Match $home '(?s)sdoIsWrite\s*:=\s*TRUE\s*;\s*sdoIndex\s*:=\s*0x6098\s*;' 'H37 still writes Homing method (0x6098) through its dedicated lifecycle'
Assert-Match $home '(?s)sdoIsWrite\s*:=\s*TRUE\s*;\s*sdoIndex\s*:=\s*0x6060\s*;.*?sdoData\s*:=\s*6\s*;' 'H37 still enters DS402 Homing mode 6'
Assert-Match $home '(?s)sdoIsWrite\s*:=\s*TRUE\s*;\s*sdoIndex\s*:=\s*0x6060\s*;.*?sdoData\s*:=\s*8\s*;' 'H37 still restores CSP mode 8'
Assert-NoMatch $home 'GetSdoWritePolicyDetail|LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED' 'H37 dedicated lifecycle is not routed through Generic D5 SDO Write admission'

# H37 remains dormant until the artifact/hardware gates are closed.
Assert-Match $diagnostics '(?m)^\s*#define\s+LMC_DIAG_DS402_HOME_ENABLED\s+FALSE\s*$' 'HomeDS402 runtime activation remains OFF during H37-C0'

# WPF availability must remain tied to Admin HomeDS402 capability bit 6 rather than
# Generic SDO Write availability or Same-Value qualification state.
Assert-Match $wpf '(?s)var\s+ds402Capability\s*=\s*adminCapabilities\s*!=\s*null\s*&&\s*adminCapabilities\.Supports\(\s*LMCAdminFeature\.AxisDs402Home\s*\)' 'WPF derives HomeDS402 availability from AxisDs402Home capability'
Assert-Match $wpf '(?s)ButtonDs402Home\.IsEnabled\s*=.*?&&\s*ds402Capability\s*&&' 'WPF HomeDS402 button remains capability-gated'
Assert-NoMatch $wpf '(?s)ButtonDs402Home\.IsEnabled\s*=.*?HasCurrentSdoWriteActivationQualificationProof' 'WPF HomeDS402 availability is independent of Generic SDO Write qualification proof'

Write-Host ("H37 current-dev regression PASS: {0} top-level checks (nested verifier totals are reported separately)." -f $script:CheckCount)

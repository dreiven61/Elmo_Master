[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Enable')]
    [switch]$Enable,

    [Parameter(Mandatory = $true, ParameterSetName = 'Revert')]
    [switch]$Revert,

    [Parameter(ParameterSetName = 'Verify')]
    [switch]$Verify,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ExpectedBranch = 'codex/setopmode-mode11-bench-activation'
$DiagnosticsRelative = 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$ControlRelative = 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$DiagnosticsOff = '#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE'
$DiagnosticsOn = '#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE'
$BenchModesOff = '#define LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES FALSE'
$BenchModesOn = '#define LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES TRUE'
$AdminOff = '(pResponseFrame + 24)^$UDINT := 0x00000017;'
$AdminOn = '(pResponseFrame + 24)^$UDINT := 0x00000717;'

function Get-OrdinalCount {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle
    )

    $count = 0
    $offset = 0
    while ($true) {
        $index = $Text.IndexOf($Needle, $offset, [System.StringComparison]::Ordinal)
        if ($index -lt 0) { break }
        $count++
        $offset = $index + $Needle.Length
    }
    return $count
}

function Assert-ExactCount {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][int]$Expected,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $count = Get-OrdinalCount -Text $Text -Needle $Needle
    if ($count -ne $Expected) {
        throw "$Label count=$count expected=$Expected"
    }
}

function Assert-MinimumCount {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][int]$Minimum,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $count = Get-OrdinalCount -Text $Text -Needle $Needle
    if ($count -lt $Minimum) {
        throw "$Label count=$count minimum=$Minimum"
    }
}

function Assert-BaselineText {
    param(
        [Parameter(Mandatory = $true)][string]$Diagnostics,
        [Parameter(Mandatory = $true)][string]$Control
    )

    Assert-ExactCount $Diagnostics $DiagnosticsOff 1 'Diagnostics OFF gate'
    Assert-ExactCount $Diagnostics $DiagnosticsOn 0 'Diagnostics ON gate'
    Assert-ExactCount $Diagnostics $BenchModesOff 1 'bench-mode OFF gate'
    Assert-ExactCount $Diagnostics $BenchModesOn 0 'bench-mode ON gate'
    Assert-ExactCount $Control $AdminOff 1 'Admin baseline feature mask'
    Assert-ExactCount $Control $AdminOn 0 'Admin MODE-11 feature mask'
}

function Assert-ActiveText {
    param(
        [Parameter(Mandatory = $true)][string]$Diagnostics,
        [Parameter(Mandatory = $true)][string]$Control
    )

    Assert-ExactCount $Diagnostics $DiagnosticsOff 0 'Diagnostics OFF gate'
    Assert-ExactCount $Diagnostics $DiagnosticsOn 1 'Diagnostics ON gate'
    Assert-ExactCount $Diagnostics $BenchModesOff 0 'bench-mode OFF gate'
    Assert-ExactCount $Diagnostics $BenchModesOn 1 'bench-mode ON gate'
    Assert-ExactCount $Control $AdminOff 0 'Admin baseline feature mask'
    Assert-ExactCount $Control $AdminOn 1 'Admin MODE-11 feature mask'

    # Keep this transform verifier intentionally narrow. The dedicated
    # Verify-SetOperationModeMode11Candidate.ps1 owns the detailed safety
    # contract (0x6060 write length/fanout, no replay, D5 deny, state guards).
    Assert-MinimumCount $Diagnostics 'LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE' 2 'bench precondition allow-list guards'
    Assert-MinimumCount $Diagnostics 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 1 'write-dispatch evidence ABI'
}

function Convert-BenchText {
    param(
        [Parameter(Mandatory = $true)][string]$Diagnostics,
        [Parameter(Mandatory = $true)][string]$Control,
        [Parameter(Mandatory = $true)][bool]$ToActive
    )

    if ($ToActive) {
        Assert-BaselineText $Diagnostics $Control
        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOff, $DiagnosticsOn)
        $newDiagnostics = $newDiagnostics.Replace($BenchModesOff, $BenchModesOn)
        $newControl = $Control.Replace($AdminOff, $AdminOn)
        Assert-ActiveText $newDiagnostics $newControl
    }
    else {
        Assert-ActiveText $Diagnostics $Control
        $newDiagnostics = $Diagnostics.Replace($DiagnosticsOn, $DiagnosticsOff)
        $newDiagnostics = $newDiagnostics.Replace($BenchModesOn, $BenchModesOff)
        $newControl = $Control.Replace($AdminOn, $AdminOff)
        Assert-BaselineText $newDiagnostics $newControl
    }

    return @($newDiagnostics, $newControl)
}

function Write-AsciiPreservingText {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )

    foreach ($character in $Text.ToCharArray()) {
        if ([int]$character -gt 127) {
            throw "Non-ASCII character found while preparing LASAL source: $Path"
        }
    }
    [System.IO.File]::WriteAllText(
        $Path,
        $Text,
        (New-Object System.Text.ASCIIEncoding))
}

function Invoke-SelfTest {
    $diag = "prefix`r`n$DiagnosticsOff`r`n$BenchModesOff`r`nelsif (requestedMode <> 8) & (LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) then`r`nelsif (requestedMode <> 8) & (LMC_DIAG_SET_OPERATION_MODE_BENCH_PRECONDITION_MODES = FALSE) then`r`nLMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED`r`n"
    $control = "prefix`r`n$AdminOff`r`n"
    $active = Convert-BenchText -Diagnostics $diag -Control $control -ToActive $true
    $reverted = Convert-BenchText -Diagnostics $active[0] -Control $active[1] -ToActive $false
    if ($reverted[0] -cne $diag -or $reverted[1] -cne $control) {
        throw 'MODE-11 bench activation transform did not round-trip exactly.'
    }
    Write-Host 'PASS MODE-11 bench activation transform self-test'
}

if ($SelfTest) {
    Invoke-SelfTest
    exit 0
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$diagnosticsPath = Join-Path $repoRoot $DiagnosticsRelative
$controlPath = Join-Path $repoRoot $ControlRelative
foreach ($path in @($diagnosticsPath, $controlPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required source file missing: $path"
    }
}

$branch = (& git -C $repoRoot rev-parse --abbrev-ref HEAD 2>$null).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $ExpectedBranch) {
    throw "MODE-11 bench activation is allowed only on branch '$ExpectedBranch'. Current='$branch'."
}

$diagnosticsText = [System.IO.File]::ReadAllText($diagnosticsPath)
$controlText = [System.IO.File]::ReadAllText($controlPath)

if ($Enable) {
    $status = (& git -C $repoRoot status --porcelain=v1 2>$null) -join "`n"
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to read git working-tree status.'
    }
    if (-not [string]::IsNullOrWhiteSpace($status)) {
        throw "Refusing MODE-11 activation on a dirty working tree. Commit/stash first:`n$status"
    }

    $converted = Convert-BenchText -Diagnostics $diagnosticsText -Control $controlText -ToActive $true
    Write-AsciiPreservingText -Path $diagnosticsPath -Text $converted[0]
    Write-AsciiPreservingText -Path $controlPath -Text $converted[1]
    Write-Host 'BENCH ONLY: SetOperationMode Diagnostics mutation gate is TRUE.'
    Write-Host 'BENCH ONLY: Admin feature mask is 0x00000717 (bits 8/9/10 paired ON).'
    Write-Host 'DO NOT MERGE these activation source changes into dev before MODE-11/12 PASS and MODE-14 review.'
    exit 0
}

if ($Revert) {
    $converted = Convert-BenchText -Diagnostics $diagnosticsText -Control $controlText -ToActive $false
    Write-AsciiPreservingText -Path $diagnosticsPath -Text $converted[0]
    Write-AsciiPreservingText -Path $controlPath -Text $converted[1]
    Write-Host 'PASS MODE-11 bench activation source reverted to production-OFF values.'
    exit 0
}

$baseline = $false
$active = $false
try {
    Assert-BaselineText $diagnosticsText $controlText
    $baseline = $true
}
catch {
}
try {
    Assert-ActiveText $diagnosticsText $controlText
    $active = $true
}
catch {
}
if ($baseline -eq $active) {
    throw 'MODE-11 source is neither one exact baseline pair nor one exact active pair.'
}
if ($baseline) {
    Write-Host 'PASS MODE-11 source state: BASELINE_OFF (Diagnostics FALSE, Admin mask 0x00000017).'
}
else {
    Write-Host 'PASS MODE-11 source state: BENCH_ACTIVE (Diagnostics TRUE, Admin mask 0x00000717, bench targets PP/PV/IP/CSP).'
}

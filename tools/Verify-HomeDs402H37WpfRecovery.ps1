[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Failures = New-Object System.Collections.Generic.List[string]
$script:PassCount = 0

function Add-Pass {
    param([Parameter(Mandatory = $true)][string]$Message)
    $script:PassCount++
    Write-Host "PASS $Message"
}

function Add-Failure {
    param([Parameter(Mandatory = $true)][string]$Message)
    $script:Failures.Add($Message)
    Write-Host "FAIL $Message"
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if ($Condition) { Add-Pass $Message } else { Add-Failure $Message }
}

function Assert-Regex {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Message,
        [int]$ExpectedCount = -1,
        [int]$MinimumCount = -1
    )

    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    $matches = [regex]::Matches($Text, $Pattern, $options)
    if ($ExpectedCount -ge 0) {
        Assert-True ($matches.Count -eq $ExpectedCount) "$Message (count=$($matches.Count), expected=$ExpectedCount)"
        return
    }
    if ($MinimumCount -ge 0) {
        Assert-True ($matches.Count -ge $MinimumCount) "$Message (count=$($matches.Count), minimum=$MinimumCount)"
        return
    }
    Assert-True ($matches.Count -gt 0) $Message
}

function Get-CSharpMethodBlock {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$MethodName
    )

    $nameIndex = $Text.IndexOf($MethodName, [StringComparison]::Ordinal)
    if ($nameIndex -lt 0) {
        Add-Failure "C# method exists: $MethodName"
        return $null
    }

    $braceStart = $Text.IndexOf('{', $nameIndex)
    if ($braceStart -lt 0) {
        Add-Failure "C# method opening brace exists: $MethodName"
        return $null
    }

    $depth = 0
    $inString = $false
    $inChar = $false
    $verbatim = $false
    $escape = $false
    for ($i = $braceStart; $i -lt $Text.Length; $i++) {
        $c = $Text[$i]
        $next = if ($i + 1 -lt $Text.Length) { $Text[$i + 1] } else { [char]0 }

        if ($inString) {
            if ($verbatim) {
                if ($c -eq '"') {
                    if ($next -eq '"') { $i++; continue }
                    $inString = $false
                    $verbatim = $false
                }
                continue
            }
            if ($escape) { $escape = $false; continue }
            if ($c -eq '\') { $escape = $true; continue }
            if ($c -eq '"') { $inString = $false }
            continue
        }
        if ($inChar) {
            if ($escape) { $escape = $false; continue }
            if ($c -eq '\') { $escape = $true; continue }
            if ($c -eq "'") { $inChar = $false }
            continue
        }
        if ($c -eq '@' -and $next -eq '"') { $inString = $true; $verbatim = $true; $i++; continue }
        if ($c -eq '"') { $inString = $true; continue }
        if ($c -eq "'") { $inChar = $true; continue }

        if ($c -eq '{') { $depth++ }
        elseif ($c -eq '}') {
            $depth--
            if ($depth -eq 0) {
                Add-Pass "C# method block: $MethodName"
                return $Text.Substring($nameIndex, $i - $nameIndex + 1)
            }
        }
    }

    Add-Failure "C# method closing brace exists: $MethodName"
    return $null
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = (Get-Location).Path
}
$root = [System.IO.Path]::GetFullPath($RepositoryRoot)
$mainPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.MaintenanceActions.cs'
$journalPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MaintenanceActionRecoveryJournal.cs'

foreach ($path in @($mainPath, $journalPath)) {
    Assert-True (Test-Path -LiteralPath $path -PathType Leaf) "required WPF recovery source exists: $([System.IO.Path]::GetFileName($path))"
}
if ($script:Failures.Count -gt 0) { throw 'Required H37 WPF recovery source is missing.' }

$main = [System.IO.File]::ReadAllText($mainPath)
$journal = [System.IO.File]::ReadAllText($journalPath)

Write-Host 'HomeDS402 H37 WPF durable-recovery qualification'
Write-Host "Repository: $root"

# Stable durable action/state identity.
Assert-Regex $journal '(?m)^\s*Ds402Home\s*=\s*2\s*,?\s*$' 'DS402 Home durable action kind remains 2' -ExpectedCount 1
Assert-Regex $journal '(?m)^\s*ArmedBeforeDispatch\s*=\s*1\s*,?\s*$' 'journal ArmedBeforeDispatch state remains 1' -ExpectedCount 1
Assert-Regex $journal '(?m)^\s*RecoveryRequired\s*=\s*2\s*,?\s*$' 'journal RecoveryRequired state remains 2' -ExpectedCount 1

# Journal admission requires exact method-37 semantic and persists before dispatch.
$arm = Get-CSharpMethodBlock -Text $journal -MethodName 'ArmBeforeDispatch('
if ($null -ne $arm) {
    Assert-Regex $arm 'action\s*==\s*MaintenanceActionKind\.Ds402Home[\s\S]{0,180}!HasExactDs402HomeSemantic\(actionParameters\)' 'journal rejects non-exact DS402 Home semantics before arming' -ExpectedCount 1
    Assert-Regex $arm 'MaintenanceActionRecoveryState\.ArmedBeforeDispatch' 'journal creates ArmedBeforeDispatch record' -ExpectedCount 1
    $persistIndex = $arm.IndexOf('PersistRecord(armed)', [StringComparison]::Ordinal)
    $publishIndex = $arm.IndexOf('currentRecord = armed', [StringComparison]::Ordinal)
    Assert-True ($persistIndex -ge 0 -and $publishIndex -gt $persistIndex) 'journal persistence precedes in-memory armed publication'
}

# Relaunch converts ambiguous pre-dispatch state into durable recovery-required state.
$promoteAtOpen = Get-CSharpMethodBlock -Text $journal -MethodName 'PromoteArmedRecordAtOpen('
if ($null -ne $promoteAtOpen) {
    Assert-Regex $promoteAtOpen 'currentRecord\.State[\s\S]{0,100}MaintenanceActionRecoveryState\.ArmedBeforeDispatch' 'startup promotion recognizes ArmedBeforeDispatch' -ExpectedCount 1
    Assert-Regex $promoteAtOpen 'TransitionTo\(\s*MaintenanceActionRecoveryState\.RecoveryRequired' 'startup promotion transitions to RecoveryRequired' -ExpectedCount 1
    $persistPromoted = $promoteAtOpen.IndexOf('PersistRecord(promoted)', [StringComparison]::Ordinal)
    $publishPromoted = $promoteAtOpen.IndexOf('currentRecord = promoted', [StringComparison]::Ordinal)
    Assert-True ($persistPromoted -ge 0 -and $publishPromoted -gt $persistPromoted) 'startup RecoveryRequired transition is persisted before publication'
}

# WPF startup reconstructs the exact DS402 recovery key immediately after opening the journal.
Assert-Regex $main 'else if\s*\(active\.Action\s*==\s*MaintenanceActionKind\.Ds402Home\)\s*\{\s*latestDs402HomeRecoveryKey\s*=\s*RecreateDs402RecoveryKey\(active\);\s*\}' 'WPF startup reconstructs DS402 Home recovery key' -ExpectedCount 1

# The operator Start path must arm durable recovery before any native Start RPC.
$start = Get-CSharpMethodBlock -Text $main -MethodName 'ButtonDs402Home_Click('
if ($null -ne $start) {
    $armIndex = $start.IndexOf('.ArmBeforeDispatch(', [StringComparison]::Ordinal)
    $startRpcIndex = $start.IndexOf('.Ds402HomeAsync(', [StringComparison]::Ordinal)
    Assert-True ($armIndex -ge 0 -and $startRpcIndex -gt $armIndex) 'durable ArmBeforeDispatch precedes DS402 Home Start RPC'
    Assert-Regex $start 'MaintenanceActionKind\.Ds402Home' 'DS402 Home Start arms the correct durable action kind' -MinimumCount 1
    Assert-Regex $start 'catch\s*\(LMCAxisDs402HomeRejectedException[\s\S]{0,260}ResolveMaintenanceConfirmedRejection' 'deterministic pre-execution rejection resolves exact durable record' -ExpectedCount 1
    Assert-Regex $start 'catch\s*\{[\s\S]{0,180}PromoteMaintenanceRecovery\(' 'ambiguous Start failure promotes recovery-required state' -ExpectedCount 1
}

# Exact outcome recovery is read/retire only: no Start replay, no early resolve.
$recover = Get-CSharpMethodBlock -Text $main -MethodName 'ReadExactDs402HomeOutcomeAsync('
if ($null -ne $recover) {
    Assert-Regex $recover 'ReadDs402HomeOutcomeAsync\(' 'recovery performs exact DS402 Home outcome query' -ExpectedCount 1
    Assert-Regex $recover 'Ds402HomeAsync\(' 'recovery never replays DS402 Home Start' -ExpectedCount 0
    Assert-Regex $recover 'if\s*\(!outcome\.IsTerminal\)[\s\S]{0,360}return;' 'Running/nonterminal outcome returns with durable record active' -ExpectedCount 1
    $terminalGuard = $recover.IndexOf('if (!outcome.IsTerminal)', [StringComparison]::Ordinal)
    $retireIndex = $recover.IndexOf('.RetireDs402HomeOutcomeAsync(', [StringComparison]::Ordinal)
    $matchIndex = $recover.IndexOf('Ds402HomeTerminalSnapshotsMatch(outcome, retirement)', [StringComparison]::Ordinal)
    $resolveIndex = $recover.IndexOf('maintenanceActionRecoveryJournal.Resolve(', [StringComparison]::Ordinal)
    Assert-True ($terminalGuard -ge 0 -and $retireIndex -gt $terminalGuard) 'retirement is reachable only after terminal outcome gate'
    Assert-True ($matchIndex -gt $retireIndex -and $resolveIndex -gt $matchIndex) 'terminal retirement snapshot proof precedes durable Resolve'
}

# Recovery-key reconstruction must remain the exact non-moving method-37 semantic.
$recreate = Get-CSharpMethodBlock -Text $main -MethodName 'RecreateDs402RecoveryKey('
if ($null -ne $recreate) {
    Assert-Regex $recreate 'record\.Action\s*!=\s*MaintenanceActionKind\.Ds402Home' 'recovery-key reconstruction rejects other action kinds' -ExpectedCount 1
    Assert-Regex $recreate 'CurrentPositionZeroHomingMethod' 'recovery-key reconstruction pins method 37/current-position-zero' -ExpectedCount 1
    foreach ($field in @('HomeOffset', 'Velocity', 'Acceleration', 'DistanceLimit', 'TorqueLimit')) {
        Assert-Regex $recreate ("ReadParameterInt\(values, \"" + $field + "\"\)\s*!=\s*0") "recovery-key reconstruction requires $field zero" -ExpectedCount 1
    }
    Assert-Regex $recreate 'LMCDs402HomeBufferMode\.Aborting\.ToString\(\)' 'recovery-key reconstruction requires Aborting buffer mode' -ExpectedCount 1
}

# Home records cannot be manually waved through by the operator UI.
Assert-Regex $main 'manualRecoveryResolutionAllowed[\s\S]{0,260}activeRecoveryRecord\.Action\s*!=\s*MaintenanceActionKind\.Ds402Home[\s\S]{0,180}activeRecoveryRecord\.Action\s*!=\s*MaintenanceActionKind\.LmcHome' 'manual recovery resolution excludes DS402 Home and LMC Home' -ExpectedCount 1
Assert-Regex $main 'DS402 Home requires Read Home Status[\s\S]{0,180}manual record resolution is disabled' 'WPF explicitly communicates DS402 Home manual-resolution prohibition' -ExpectedCount 1

if ($script:Failures.Count -gt 0) {
    Write-Host ''
    Write-Host "H37 WPF recovery qualification FAILED: $($script:Failures.Count) failure(s), $script:PassCount pass(es)."
    foreach ($failure in $script:Failures) { Write-Host " - $failure" }
    exit 1
}

Write-Host ''
Write-Host "H37 WPF recovery qualification PASSED: $script:PassCount checks."
exit 0

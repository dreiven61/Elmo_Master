[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Failures = New-Object System.Collections.Generic.List[string]
$script:PassCount = 0

function Pass([string]$Message) {
    $script:PassCount++
    Write-Host "PASS $Message"
}

function Fail([string]$Message) {
    $script:Failures.Add($Message)
    Write-Host "FAIL $Message"
}

function Assert-Regex {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Message,
        [int]$ExpectedCount = -1,
        [int]$MinimumCount = -1
    )

    $options = [Text.RegularExpressions.RegexOptions]::Multiline -bor [Text.RegularExpressions.RegexOptions]::Singleline
    $matches = [regex]::Matches($Text, $Pattern, $options)
    if ($ExpectedCount -ge 0) {
        if ($matches.Count -eq $ExpectedCount) { Pass "$Message (count=$($matches.Count))" }
        else { Fail "$Message (count=$($matches.Count), expected=$ExpectedCount)" }
        return
    }
    if ($MinimumCount -ge 0) {
        if ($matches.Count -ge $MinimumCount) { Pass "$Message (count=$($matches.Count))" }
        else { Fail "$Message (count=$($matches.Count), minimum=$MinimumCount)" }
        return
    }
    if ($matches.Count -gt 0) { Pass $Message } else { Fail $Message }
}

function Get-LasalFunctionBody {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$QualifiedName
    )

    $pattern = '(?ms)^[\t ]*FUNCTION(?:[\t ]+(?:GLOBAL|VIRTUAL))*[\t ]+' +
        [regex]::Escape($QualifiedName) + '\b.*?^[\t ]*END_FUNCTION[\t ]*$'
    $matches = [regex]::Matches($Text, $pattern)
    if ($matches.Count -ne 1) {
        Fail "exact LASAL function body $QualifiedName (count=$($matches.Count), expected=1)"
        return $null
    }
    Pass "exact LASAL function body $QualifiedName"
    return $matches[0].Value
}

function To-Lf([string]$Text) {
    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$diagnosticsPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$controlPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$tcpPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'

foreach ($path in @($diagnosticsPath, $controlPath, $tcpPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Fail "required source exists: $path"
    }
    else {
        Pass "required source exists: $([IO.Path]::GetFileName($path))"
    }
}
if ($script:Failures.Count -gt 0) { exit 1 }

$diagnostics = To-Lf ([IO.File]::ReadAllText($diagnosticsPath))
$control = To-Lf ([IO.File]::ReadAllText($controlPath))
$tcp = To-Lf ([IO.File]::ReadAllText($tcpPath))

$gateOff = [regex]::Matches($diagnostics, '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]+FALSE[\t ]*$').Count
$gateOn = [regex]::Matches($diagnostics, '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]+TRUE[\t ]*$').Count
$maskOff = [regex]::Matches($control, '(?m)^\s*\(pResponseFrame \+ 24\)\^\$UDINT := 0x00000017;\s*$').Count
$maskOn = [regex]::Matches($control, '(?m)^\s*\(pResponseFrame \+ 24\)\^\$UDINT := 0x00000717;\s*$').Count

$state = 'Invalid'
if (($gateOff -eq 1) -and ($gateOn -eq 0) -and ($maskOff -eq 1) -and ($maskOn -eq 0)) {
    $state = 'BASELINE_OFF'
    Pass 'activation pair is BASELINE_OFF: Diagnostics FALSE + Admin feature mask 0x00000017'
}
elseif (($gateOff -eq 0) -and ($gateOn -eq 1) -and ($maskOff -eq 0) -and ($maskOn -eq 1)) {
    $state = 'BENCH_ACTIVE'
    Pass 'activation pair is BENCH_ACTIVE: Diagnostics TRUE + Admin feature mask 0x00000717'
}
else {
    Fail "activation pair is inconsistent: gateOff=$gateOff gateOn=$gateOn maskOff=$maskOff maskOn=$maskOn"
}

Assert-Regex $control '(?m)^\s*\(pResponseFrame \+ 36\)\^\$UINT := 4;\s*$' 'Admin capability physical-axis count remains 4' -ExpectedCount 1
Assert-Regex $control '(?m)^\s*\(pResponseFrame \+ 44\)\^\$UINT := 6;\s*$' 'Admin error catalog remains version 6' -ExpectedCount 1
Assert-Regex $control '(?m)^#define[\t ]+LMC_OWNER_KIND_AXIS_OPERATION_MODE[\t ]+6[\t ]*$' 'owner kind remains 6' -ExpectedCount 1
Assert-Regex $control '(?m)^#define[\t ]+LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE[\t ]+4[\t ]*$' 'shared diagnostics SDO resource remains 4' -ExpectedCount 1
Assert-Regex $tcp '0x7D23,[\t ]*0x7D24,[\t ]*0x7D25' 'Start/Outcome/Retire routes remain paired' -MinimumCount 1
Assert-Regex $diagnostics 'elsif[\t ]+requestedMode[\t ]*<>[\t ]*8[\t ]+then' 'CSP=8-only validation exists at both Start and recovery-key boundaries' -ExpectedCount 2
Assert-Regex $diagnostics 'LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]*=[\t ]*FALSE' 'OFF-transition recovery guards remain compiled into source' -MinimumCount 3

$main = Get-LasalFunctionBody $diagnostics 'LMCDiagnosticsService::ProcessAxisSetOperationMode'
$mutation = Get-LasalFunctionBody $diagnostics 'LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages'
$recovery = Get-LasalFunctionBody $diagnostics 'LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages'
$policy = Get-LasalFunctionBody $diagnostics 'LMCDiagnosticsService::GetSdoWritePolicyDetail'

if ($null -ne $main) {
    Assert-Regex $main 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'main processor owns no 0x6060 write site' -ExpectedCount 0
    Assert-Regex $main 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'main processor preserves irreversible-dispatch no-replay normalization' -MinimumCount 2
}

if ($null -ne $mutation) {
    Assert-Regex $mutation 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'mutation helper retains exactly four physical-axis 0x6060 write fanout sites' -ExpectedCount 4
    Assert-Regex $mutation 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060[^;\r\n]*WriteLength:=1' 'every 0x6060 mutation is exactly one byte' -ExpectedCount 4
    Assert-Regex $mutation 'if[\t ]+observedMode[\t ]*=[\t ]*8[\t ]+then[\s\S]{0,900}LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS;[\s\S]{0,120}RETURN;[\s\S]{0,900}_memset\(dest:=#startupSnapshot' 'same-mode CSP=8 path terminates before write-safety branch' -ExpectedCount 1

    $sameModePattern = '(?ms)if[\t ]+observedMode[\t ]*=[\t ]*8[\t ]+then(?<body>.*?)^[\t ]*RETURN;'
    $sameModeMatches = [regex]::Matches($mutation, $sameModePattern)
    if ($sameModeMatches.Count -ne 1) {
        Fail "exact same-mode CSP=8 branch (count=$($sameModeMatches.Count), expected=1)"
    }
    else {
        Pass 'exact same-mode CSP=8 branch'
        $sameModeBody = $sameModeMatches[0].Groups['body'].Value
        Assert-Regex $sameModeBody 'LMC_DIAG_MODE_EVIDENCE_WRITE_REQUESTED' 'same-mode branch never sets WriteRequested evidence' -ExpectedCount 0
        Assert-Regex $sameModeBody 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'same-mode branch never sets WriteDispatched evidence' -ExpectedCount 0
        Assert-Regex $sameModeBody 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'same-mode branch contains no 0x6060 write dispatch' -ExpectedCount 0
        Assert-Regex $sameModeBody 'LMC_DIAG_MODE_EVIDENCE_VERIFY_DISPATCHED' 'same-mode branch records verify-read dispatched evidence' -MinimumCount 1
        Assert-Regex $sameModeBody 'LMC_DIAG_MODE_EVIDENCE_VERIFY_COMPLETED' 'same-mode branch records verify-read completed evidence' -MinimumCount 1
    }

    Assert-Regex $mutation 'LMC_DIAG_MODE_AXIS_STANDSTILL' 'non-CSP mutation requires standstill evidence' -MinimumCount 1
    Assert-Regex $mutation 'LMC_DIAG_MODE_DS402_FAULT\)[\t ]*=[\t ]*0' 'non-CSP mutation requires DS402 fault clear' -MinimumCount 1
    Assert-Regex $mutation 'LMC_DIAG_MODE_DS402_OPERATION_ENABLED\)[\t ]*=[\t ]*0' 'non-CSP mutation requires operation-disabled state' -MinimumCount 1
    Assert-Regex $mutation 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'mutation helper persists write-dispatch evidence' -MinimumCount 1
}

if ($null -ne $recovery) {
    Assert-Regex $recovery 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'recovery path never replays 0x6060' -ExpectedCount 0
    Assert-Regex $recovery 'never fall back to WRITE_START' 'recovery source retains explicit no-replay invariant' -ExpectedCount 1
}

if ($null -ne $policy) {
    Assert-Regex $policy 'ObjectIndex[\t ]*=[\t ]*0x6060[\s\S]{0,220}DetailCode[\t ]*:=[\t ]*8;[\s\S]{0,80}RETURN;' 'generic D5 still permanently denies 0x6060' -ExpectedCount 1
}

if ($state -eq 'BENCH_ACTIVE') {
    Write-Host 'BENCH_ACTIVE candidate verified. This state is qualification-only and must not be merged into dev before MODE-11/12 PASS.'
}

if ($script:Failures.Count -gt 0) {
    Write-Host "MODE-11 candidate FAILED: $($script:Failures.Count) failure(s), $script:PassCount pass(es)."
    foreach ($failure in $script:Failures) { Write-Host " - $failure" }
    exit 1
}

Write-Host "MODE-11 candidate PASSED: $script:PassCount checks; state=$state."
exit 0

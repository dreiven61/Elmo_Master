[CmdletBinding()]
param(
    [ValidateRange(1, 4)]
    [int]$ExpectedSdoWriteAxis = 1,
    [switch]$QualificationActivation
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
    if ($Condition) {
        Add-Pass $Message
    }
    else {
        Add-Failure $Message
    }
}

function Assert-Regex {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Message,
        [int]$ExpectedCount = -1,
        [int]$MinimumCount = -1
    )

    $matches = [regex]::Matches(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

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

function Get-LasalFunctionBody {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$QualifiedName
    )

    $escapedName = [regex]::Escape($QualifiedName)
    $pattern = "(?ms)^[\t ]*FUNCTION(?:[\t ]+(?:GLOBAL|VIRTUAL))*[\t ]+$escapedName\b.*?^[\t ]*END_FUNCTION[\t ]*$"
    $matches = [regex]::Matches($Text, $pattern)
    if ($matches.Count -ne 1) {
        Add-Failure "exact LASAL function body $QualifiedName (count=$($matches.Count), expected=1)"
        return $null
    }

    Add-Pass "exact LASAL function body $QualifiedName"
    return $matches[0].Value
}

function Assert-LasalMethodBudget {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$QualifiedName,
        [int]$LimitBytes = 32768
    )

    $body = Get-LasalFunctionBody -Text $Text -QualifiedName $QualifiedName
    if ($null -eq $body) {
        return
    }

    $bytes = [System.Text.Encoding]::UTF8.GetByteCount($body)
    Assert-True ($bytes -lt $LimitBytes) "$QualifiedName method budget $bytes < $LimitBytes bytes"
}

function ConvertTo-LfText {
    param([Parameter(Mandatory = $true)][string]$Text)

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$controlPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$diagnosticsPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$tcpPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'

foreach ($path in @($controlPath, $diagnosticsPath, $tcpPath)) {
    Assert-True (Test-Path -LiteralPath $path) "required source exists: $([System.IO.Path]::GetFileName($path))"
}

if ($script:Failures.Count -gt 0) {
    throw 'Required source files are missing.'
}

$control = ConvertTo-LfText ([System.IO.File]::ReadAllText($controlPath))
$diagnostics = ConvertTo-LfText ([System.IO.File]::ReadAllText($diagnosticsPath))
$tcp = ConvertTo-LfText ([System.IO.File]::ReadAllText($tcpPath))

Write-Host 'MODE-10 SetOperationMode source/static qualification'
Write-Host "Repository: $repoRoot"
Write-Host "ExpectedSdoWriteAxis compatibility parameter: $ExpectedSdoWriteAxis"
Write-Host "QualificationActivation: $QualificationActivation"

# Production keeps the feature closed. A dedicated qualification integration
# may open the paired runtime/Admin triad without weakening the source contract.
if ($QualificationActivation) {
    Assert-Regex $diagnostics '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]+TRUE[\t ]*$' 'SetOperationMode qualification activation gate is TRUE' -ExpectedCount 1
    Assert-Regex $control '\(pResponseFrame \+ 24\)\^\$UDINT := 0x00000717;' 'Admin capability mask advertises the SetOperationMode triad' -ExpectedCount 1
    Assert-Regex $control '\(pResponseFrame \+ 46\)\^\$UINT := 0x018A;' 'Admin capability payload advertises PP/PV/IP/CSP mask' -ExpectedCount 1
}
else {
    Assert-Regex $diagnostics '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]+FALSE[\t ]*$' 'SetOperationMode compile-time activation gate is FALSE' -ExpectedCount 1
    Assert-Regex $control '\(pResponseFrame \+ 24\)\^\$UDINT := 0x00000017;' 'Production Admin capability mask keeps SetOperationMode closed' -ExpectedCount 1
}
Assert-Regex $diagnostics 'LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]*=[\t ]*FALSE' 'runtime paths explicitly honor the OFF gate' -MinimumCount 3

# Frozen MODE-06 owner ABI and TCP routing.
Assert-Regex $control '(?m)^#define[\t ]+LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE[\t ]+12[\t ]*$' 'operation-mode active owner state is 12' -ExpectedCount 1
Assert-Regex $control '(?m)^#define[\t ]+LMC_OWNER_KIND_AXIS_OPERATION_MODE[\t ]+6[\t ]*$' 'operation-mode owner kind is 6' -ExpectedCount 1
Assert-Regex $control '(?m)^#define[\t ]+LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE[\t ]+4[\t ]*$' 'diagnostics SDO owner resource is 4' -ExpectedCount 1
Assert-Regex $tcp '0x7D23,[\t ]*0x7D24,[\t ]*0x7D25' 'TCP routes Start/ReadOutcome/Retire together' -MinimumCount 1
Assert-Regex $tcp 'diagnosticsOwnerKind[\t ]*:=[\t ]*6;[\s\S]{0,160}diagnosticsResourceKind[\t ]*:=[\t ]*4;' 'TCP Start admission uses owner kind 6/resource 4' -MinimumCount 1
Assert-Regex $diagnostics '(?m)^#define[\t ]+LMC_DIAG_MODE_DETAIL_OWNERSHIP_CHANNEL[\t ]+52[\t ]*$' 'SetOperationMode has a dedicated AxisOwnership-channel unavailable detail' -ExpectedCount 1
Assert-Regex $diagnostics '<Client Name="InputLatch" Required="true" Internal="false"/>[\s\S]{0,120}<Client Name="AxisOwnership" Required="true" Internal="false"/>' 'LASAL metadata client order matches generated declaration order' -ExpectedCount 1
Assert-Regex $diagnostics 'CallerSessionEpoch = 0\) \| \(RequestSequence = 0\)[\s\S]{0,180}OwnerGeneration = 0\) then[\s\S]{0,140}LMC_DIAG_MODE_DETAIL_STORAGE;[\s\S]{0,140}elsif IsClientConnected\(#AxisOwnership\) = FALSE then[\s\S]{0,140}LMC_DIAG_MODE_DETAIL_OWNERSHIP_CHANNEL;' 'SetOperationMode distinguishes invalid admission identity from disconnected AxisOwnership channel' -ExpectedCount 1

# Relevant custom methods must stay below the LASAL 32 KiB method limit.
foreach ($functionName in @(
    'LMCDiagnosticsService::HandleAxisSetOperationModeStart',
    'LMCDiagnosticsService::HandleAxisSetOperationModeOutcome',
    'LMCDiagnosticsService::HandleAxisSetOperationModeRetire',
    'LMCDiagnosticsService::ProcessAxisSetOperationMode',
    'LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages',
    'LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages',
    'LMCDiagnosticsService::GetSdoWritePolicyDetail'
)) {
    Assert-LasalMethodBudget -Text $diagnostics -QualifiedName $functionName
}

$processMode = Get-LasalFunctionBody -Text $diagnostics -QualifiedName 'LMCDiagnosticsService::ProcessAxisSetOperationMode'
$mutationMode = Get-LasalFunctionBody -Text $diagnostics -QualifiedName 'LMCDiagnosticsService::ProcessAxisSetOperationModeMutationStages'
$recoveryMode = Get-LasalFunctionBody -Text $diagnostics -QualifiedName 'LMCDiagnosticsService::ProcessAxisSetOperationModeRecoveryStages'

# MODE-07/MODE-08 orchestration remains in the main processor. It performs
# safety preemption and no-replay normalization before delegating a stage.
if ($null -ne $processMode) {
    Assert-Regex $processMode 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'main processor retains irreversible-dispatch no-replay normalization' -MinimumCount 2
    Assert-Regex $processMode 'LMC_DIAG_MODE_STAGE_RECOVERY_START' 'main processor can normalize uncertainty to read-only recovery' -MinimumCount 2
    Assert-Regex $processMode 'CopyAxisOwnershipPreemption' 'MODE-08 processor observes ownership preemption' -MinimumCount 1
    Assert-Regex $processMode 'PublishAxisOwnershipPreemptionCleanup' 'MODE-08 processor publishes cleanup evidence' -MinimumCount 1
    Assert-Regex $processMode 'ProcessAxisSetOperationModeMutationStages\(\);' 'main processor delegates mutation stages' -ExpectedCount 1
    Assert-Regex $processMode 'ProcessAxisSetOperationModeRecoveryStages\(\);' 'main processor delegates recovery stages' -ExpectedCount 1
    Assert-Regex $processMode 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'main processor owns no 0x6060 write site after split' -ExpectedCount 0
    Assert-Regex $processMode 'recoveryScanBase \+ 10\]\$SINT = 8[\s\S]{0,260}recoveryScanBase \+ 10\]\$SINT = 1[\s\S]{0,180}recoveryScanBase \+ 10\]\$SINT = 3[\s\S]{0,180}recoveryScanBase \+ 10\]\$SINT = 7' 'MODE-11E warm-start accepts exact PP/PV/IP/CSP candidate set' -ExpectedCount 1
    Assert-Regex $processMode 'LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES[\t ]*<>[\t ]*FALSE' 'MODE-11E non-CSP recovery follows loaded-image software-mode gate' -ExpectedCount 1
    Assert-Regex $processMode 'recoveryScanBase \+ 22\]\$UDINT <> 0[\s\S]{0,220}recoveryScanBase \+ 27\]\$UDINT <> 0[\s\S]{0,160}recoveryScanBase \+ 28\]\$UDINT <> 0[\s\S]{0,160}recoveryScanBase \+ 29\]\$UDINT <> 0[\s\S]{0,160}recoveryScanBase \+ 30\]\$UDINT <> 0' 'MODE-11E warm-start requires record generation and complete owner/session identity' -ExpectedCount 1
    Assert-Regex $processMode 'if recoveryCandidateFound then[\s\S]{0,360}_memset\(dest:=#AxisOperationModeState\[LMC_DIAG_MODE_RUNTIME_BASE\][\s\S]{0,120}RETURN;' 'MODE-11E multiple retained candidates clear staged runtime and fail closed' -ExpectedCount 1
}

# MODE-06 mutation stages contain the sole logical 0x6060 mutation site,
# fanned out over physical axes 1..4, followed by 0x6061 verification.
if ($null -ne $mutationMode) {
    foreach ($stageName in @(
        'LMC_DIAG_MODE_STAGE_PREFLIGHT_START',
        'LMC_DIAG_MODE_STAGE_PREFLIGHT_WAIT',
        'LMC_DIAG_MODE_STAGE_WRITE_START',
        'LMC_DIAG_MODE_STAGE_WRITE_WAIT',
        'LMC_DIAG_MODE_STAGE_VERIFY_START',
        'LMC_DIAG_MODE_STAGE_VERIFY_WAIT'
    )) {
        Assert-Regex $mutationMode $stageName "mutation helper contains $stageName" -MinimumCount 1
    }
    Assert-Regex $mutationMode 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'one logical 0x6060 write site fans out to exactly four physical axes' -ExpectedCount 4
    Assert-Regex $mutationMode 'LMC_DIAG_MODE_EVIDENCE_WRITE_DISPATCHED' 'mutation helper persists irreversible 0x6060 dispatch evidence' -MinimumCount 1
}

# MODE-07 recovery/terminal/quarantine stages are permanently read-only with
# respect to 0x6060. They may only drain/read/verify the already-dispatched
# intent and publish terminal/quarantine evidence.
if ($null -ne $recoveryMode) {
    foreach ($stageName in @(
        'LMC_DIAG_MODE_STAGE_RECOVERY_START',
        'LMC_DIAG_MODE_STAGE_RECOVERY_WAIT',
        'LMC_DIAG_MODE_STAGE_TERMINAL_SUCCESS',
        'LMC_DIAG_MODE_STAGE_TERMINAL_FAILURE',
        'LMC_DIAG_MODE_STAGE_QUARANTINE',
        'LMC_DIAG_MODE_STAGE_QUARANTINE_HOLD'
    )) {
        Assert-Regex $recoveryMode $stageName "recovery helper contains $stageName" -MinimumCount 1
    }
    Assert-Regex $recoveryMode 'TryStartWrite\([^;\r\n]*ObjectIndex:=0x6060' 'recovery helper never replays a 0x6060 write' -ExpectedCount 0
    Assert-Regex $recoveryMode 'never fall back to WRITE_START' 'recovery helper retains explicit read-only no-replay invariant' -ExpectedCount 1
}

# MODE-08 preemption constants and exact SetOperationMode identity recognition.
Assert-Regex $diagnostics '(?m)^#define[\t ]+LMC_DIAG_MODE_PREEMPT_DRAIN_TIMEOUT_MS[\t ]+1000[\t ]*$' 'preemption drain timeout is frozen at 1000 ms' -ExpectedCount 1
Assert-Regex $diagnostics '(?m)^#define[\t ]+LMC_DIAG_MODE_QUARANTINE_SAFETY_PREEMPT[\t ]+7[\t ]*$' 'safety-preemption quarantine reason is frozen at 7' -ExpectedCount 1
Assert-Regex $control '0x7D23:[\s\S]{0,420}oldOwnerKind[\t ]*=[\t ]*LMC_OWNER_KIND_AXIS_OPERATION_MODE[\s\S]{0,260}oldResourceKind[\t ]*=[\t ]*LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE' 'preemption snapshot validates SetOperationMode identity/resource' -MinimumCount 1
Assert-Regex $control 'LMC_OWNER_KIND_AXIS_OPERATION_MODE:[\s\S]{0,300}LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE' 'preemption state handling recognizes active SetOperationMode ownership' -MinimumCount 2

# MODE-09: generic D5 can never bypass SetOperationMode by writing 0x6060.
$getSdoPolicy = Get-LasalFunctionBody -Text $diagnostics -QualifiedName 'LMCDiagnosticsService::GetSdoWritePolicyDetail'
if ($null -ne $getSdoPolicy) {
    Assert-Regex $getSdoPolicy 'ObjectIndex[\t ]*=[\t ]*0x6060' 'generic D5 permanent-deny policy contains 0x6060' -ExpectedCount 1
    Assert-Regex $getSdoPolicy 'ObjectIndex[\t ]*=[\t ]*0x6060[\s\S]{0,220}DetailCode[\t ]*:=[\t ]*8;[\s\S]{0,80}RETURN;' 'generic D5 0x6060 denial returns before write admission' -ExpectedCount 1
}

if ($script:Failures.Count -gt 0) {
    Write-Host ''
    Write-Host "MODE-10 qualification FAILED: $($script:Failures.Count) failure(s), $script:PassCount pass(es)."
    foreach ($failure in $script:Failures) {
        Write-Host " - $failure"
    }
    exit 1
}

Write-Host ''
Write-Host "MODE-10 qualification PASSED: $script:PassCount checks."
exit 0

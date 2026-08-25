[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}

$script:PassCount = 0

function Pass([string]$Message) {
    Write-Host "PASS $Message"
    $script:PassCount++
}

function Require-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw "HomeDS402Ex HOMEEX-09 static verification failed: $Message"
    }
    Pass $Message
}

function Require-Regex([string]$Text, [string]$Pattern, [string]$Message) {
    Require-True ([regex]::IsMatch($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) $Message
}

function Require-NoRegex([string]$Text, [string]$Pattern, [string]$Message) {
    Require-True (-not [regex]::IsMatch($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) $Message
}

function Read-Required([string]$RelativePath) {
    $path = Join-Path $RepositoryRoot $RelativePath
    Require-True (Test-Path -LiteralPath $path) "$RelativePath exists"
    $text = Get-Content -LiteralPath $path -Raw
    Require-True (-not [string]::IsNullOrWhiteSpace($text)) "$RelativePath is non-empty"
    return $text
}

function Extract-Function([string]$Text, [string]$QualifiedName) {
    $pattern = '(?ms)^FUNCTION\s+' + [regex]::Escape($QualifiedName) + '\b.*?^END_FUNCTION\s*$'
    $match = [regex]::Match($Text, $pattern)
    Require-True $match.Success "$QualifiedName function body is present"
    return $match.Value
}

$controlRel = 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$diagRel = 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$tcpRel = 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$legacyContractRel = 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\Verify-LasalContract.ps1'

$control = Read-Required $controlRel
$diag = Read-Required $diagRel
$tcp = Read-Required $tcpRel
$legacyContract = Read-Required $legacyContractRel

# HOMEEX-07 ABI and the expanded non-group identity bank must remain paired.
Require-Regex $control '(?m)^#define\s+LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE\s+13\s*$' 'Owner state 13 remains reserved for HomeDS402Ex'
Require-Regex $control '(?m)^#define\s+LMC_OWNER_KIND_DS402_HOME_EX\s+7\s*$' 'OwnerKind 7 remains reserved for HomeDS402Ex'
Require-Regex $control '(?m)^#define\s+LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES\s+52\s*$' 'non-group identity tail slot remains 52 bytes'
Require-Regex $control '(?s)CommandId\s*=\s*0x7D1B.*?identityShapeValid\s*:=\s*IdentitySize\s*=\s*116' '0x7D1B ownership identity remains exactly 116 bytes'

$preemption = Extract-Function $control 'LMCControlCommandService::ValidateAxisOwnershipPreemptionReplacement'
Require-Regex $preemption '(?s)LMC_OWNER_KIND_DS402_HOME_EX\s*:.*?LMC_OWNER_STATE_RESERVED.*?LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE.*?LMC_OWNER_STATE_QUARANTINED' 'HomeDS402Ex preemption replacement accepts only reserved/active/quarantined states'
Require-Regex $preemption '(?s)replacementTailOffset\s*:=\s*TO_UDINT\(probeAxisIndex\s*-\s*1\)\s*\*\s*LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES\s*;' 'preemption replacement uses the expanded 52-byte per-axis tail stride'
Require-NoRegex $preemption '(?s)replacementTailOffset\s*:=\s*TO_UDINT\(probeAxisIndex\s*-\s*1\)\s*\*\s*8\s*;' 'preemption replacement contains no stale 8-byte tail stride'

$publishDecision = Extract-Function $control 'LMCControlCommandService::PrepareAxisOwnershipPublishDecision'
Require-Regex $publishDecision '(?s)LMC_OWNER_KIND_DS402_HOME_EX\s*:.*?preemptCommand\s*=\s*0x7D1B.*?preemptIdentitySize\s*=\s*116' 'publish decision recognizes the full HomeDS402Ex preempted identity'
Require-Regex $publishDecision '(?s)replacementTailOffset\s*:=\s*TO_UDINT\(probeAxisIndex\s*-\s*1\)\s*\*\s*LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES\s*;' 'publish decision uses the expanded 52-byte replacement tail stride'
Require-NoRegex $publishDecision '(?s)replacementTailOffset\s*:=\s*TO_UDINT\(probeAxisIndex\s*-\s*1\)\s*\*\s*8\s*;' 'publish decision contains no stale 8-byte replacement tail stride'

# HOMEEX-08 remains blocked: HOMEEX-09 must not activate motion while the axis profile is pending.
Require-Regex $diag '(?m)^#define\s+LMC_DIAG_DS402_HOME_EX_ENABLED\s+FALSE\s*$' 'HomeDS402Ex runtime gate remains FALSE'
Require-Regex $diag '(?m)^#define\s+LMC_DIAG_OWNER_KIND_DS402_HOME_EX\s+7\s*$' 'Diagnostics owner kind remains paired to OwnerKind 7'
$homeExStart = Extract-Function $diag 'LMCDiagnosticsService::HandleAxisDs402HomeExStart'
Require-Regex $homeExStart '(?s)ValidateAxisOwnershipIdentity\s*\(.*?CommandId\s*:=\s*0x7D1B.*?OwnerKind\s*:=\s*LMC_DIAG_OWNER_KIND_DS402_HOME_EX.*?IdentitySize\s*:=\s*RequestSize\s*\)' 'Start validates the exact reserved 0x7D1B ownership identity'
Require-Regex $homeExStart '(?s)LMC_DIAG_DS402_HOME_EX_ENABLED\s*=\s*FALSE.*?LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE' 'gate-OFF Start remains deterministic fail-closed'
Require-NoRegex $homeExStart '(?i)CommitAxisOwnership\s*\(' 'gate-OFF Start does not commit HomeDS402Ex ownership'

$homeExProcessor = Extract-Function $diag 'LMCDiagnosticsService::ProcessAxisDs402HomeEx'
$processorWithoutComments = [regex]::Replace($homeExProcessor, '(?m)//.*$', '')
Require-Regex $processorWithoutComments '(?ms)^FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402HomeEx\s+RETURN;\s+END_FUNCTION\s*$' 'HomeDS402Ex processor remains an executable no-op before HOMEEX-08'

Require-Regex $tcp '(?s)CommandID\s*=\s*0x7D1B.*?Payload\s*=\s*116.*?diagnosticsOwnerKind\s*:=\s*7.*?diagnosticsResourceKind\s*:=\s*3' 'TCP reserves HomeDS402Ex on shared DS402 Home resource 3'
Require-Regex $tcp '(?s)diagnosticsHomeExStartValid\s*:=.*?RequestBuf\[120\]\$UDINT\s*=\s*0x58453448' 'TCP keeps the frozen H4EX execute-token validation'
Require-Regex $tcp '(?s)CommandID\s*=\s*0x7D1B.*?diagnosticsExactAccepted\s*:=\s*FALSE' 'TCP refuses to classify gate-OFF HomeDS402Ex as accepted'
Require-Regex $tcp '(?s)CommandID\s*=\s*0x7D1B.*?RollbackAxisOwnership\s*\(' 'TCP deterministically rolls back every gate-OFF HomeDS402Ex reservation'

# HOMEEX-09 re-baselines exactly the known +3 OwnershipState reads introduced
# by the HomeDS402Ex replacement-state branch. OwnershipIdentityState stays 12.
Require-Regex $legacyContract '(?m)\$replacementBankHistogram\[''ownershipstate''\]\s*-ne\s*47' 'legacy SourceOnly OwnershipState persisted-read fence is exactly 47'
Require-NoRegex $legacyContract '(?m)\$replacementBankHistogram\[''ownershipstate''\]\s*-ne\s*44' 'legacy SourceOnly OwnershipState fence no longer expects 44'
Require-Regex $legacyContract '(?m)\$replacementBankHistogram\[''ownershipidentitystate''\]\s*-ne\s*12' 'legacy SourceOnly OwnershipIdentityState fence remains exactly 12'
Require-Regex $legacyContract '(?m)expected\s+47/12\.' 'legacy SourceOnly persisted-read diagnostic reports the paired 47/12 fence'
Require-NoRegex $legacyContract '(?m)expected\s+44/12\.' 'legacy SourceOnly diagnostic no longer reports obsolete 44/12 fence'

Write-Host ("HomeDS402Ex HOMEEX-09 static verification PASS: checks={0}" -f $script:PassCount)

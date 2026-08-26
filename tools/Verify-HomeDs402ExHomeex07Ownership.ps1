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
$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$tcpPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$diagnosticsPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'

function Pass([string]$Message) {
    Write-Host "PASS $Message"
    $script:PassCount++
}

function Require-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw "HOMEEX-07 ownership verification failed: $Message"
    }
    Pass $Message
}

function Count-Regex([string]$Text, [string]$Pattern) {
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    return ([regex]::Matches($Text, $Pattern, $options)).Count
}

function Require-Regex([string]$Text, [string]$Pattern, [string]$Message) {
    Require-True ((Count-Regex $Text $Pattern) -ge 1) $Message
}

function Require-RegexCount([string]$Text, [string]$Pattern, [int]$Expected, [string]$Message) {
    $count = Count-Regex $Text $Pattern
    Require-True ($count -eq $Expected) ("{0} (count={1}, expected={2})" -f $Message, $count, $Expected)
}

function Require-AbsentRegex([string]$Text, [string]$Pattern, [string]$Message) {
    Require-True ((Count-Regex $Text $Pattern) -eq 0) $Message
}

function Require-AsciiFile([string]$Path, [string]$Label) {
    $ascii = $true
    foreach ($value in [System.IO.File]::ReadAllBytes($Path)) {
        if ($value -gt 0x7F) { $ascii = $false; break }
    }
    Require-True $ascii ($Label + ' remains 7-bit ASCII')
}

function Get-DiagnosticsFunction([string]$Text, [string]$Name) {
    $pattern = 'FUNCTION\s+(?:GLOBAL\s+)?LMCDiagnosticsService::' + [regex]::Escape($Name) + '(?s).*?END_FUNCTION'
    $matches = [regex]::Matches($Text, $pattern)
    Require-True ($matches.Count -eq 1) ("exact diagnostics function: " + $Name)
    return $matches[0].Value
}

foreach ($path in @($controlPath, $tcpPath, $diagnosticsPath)) {
    Require-True (Test-Path -LiteralPath $path) ("required source exists: " + $path)
}

$control = Get-Content -LiteralPath $controlPath -Raw
$tcp = Get-Content -LiteralPath $tcpPath -Raw
$diagnostics = Get-Content -LiteralPath $diagnosticsPath -Raw

# Frozen ownership ABI and identity-bank geometry.
Require-RegexCount $control '^#define LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE 13$' 1 'HomeDS402Ex active owner state is reserved at 13'
Require-RegexCount $control '^#define LMC_OWNER_KIND_DS402_HOME_EX 7$' 1 'HomeDS402Ex owner kind is exactly 7'
Require-RegexCount $control '^#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040$' 1 'owner identity prefix remains 64 bytes'
Require-RegexCount $control '^#define LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES 52$' 1 'non-group axis tail slot is exactly 52 bytes'
Require-RegexCount $control '^#define LMC_OWNER_IDENTITY_SUFFIX_BYTES 1256$' 1 'shared identity suffix capacity remains 1256 bytes'
Require-True ((9 * 52) -le 1256) 'nine 52-byte non-group tail slots fit inside the existing suffix bank'
Require-AbsentRegex $control 'identityTailSize\s*>\s*8' 'legacy 8-byte non-group tail limit is fully removed'
Require-AbsentRegex $control 'identityTailOffset\s*:=\s*TO_UDINT\([^;]+?\)\s*\*\s*8\s*;' 'legacy 8-byte non-group tail offsets are fully removed'
Require-RegexCount $control 'identityTailSize\s*>\s*LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES' 6 'all six non-group tail limit checks use the 52-byte slot constant'
Require-RegexCount $control 'identityTailOffset\s*:=\s*TO_UDINT\([^;]+?\)\s*\*\s*LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES\s*;' 12 'all twelve non-group tail offsets use the slot constant'

# Reserve/validate/commit/preemption/startup owner-kind coverage.
Require-RegexCount $control 'OwnerKind\s*>\s*LMC_OWNER_KIND_DS402_HOME_EX' 1 'owner-kind range guard ends at HomeDS402Ex kind 7'
Require-RegexCount $control 'LMC_OWNER_KIND_DS402_HOME_EX\s*:' 10 'all lifecycle owner-kind switch sites include HomeDS402Ex'
Require-RegexCount $control '^\s*0x7D1B\s*:' 3 'all three exact command-tuple switch sites include HomeDS402Ex Start'
Require-Regex $control 'CommandId\s*=\s*0x7D1B[\s\S]{0,120}?IdentitySize\s*=\s*116' 'ReserveAxisOwnership accepts only the 116-byte HomeDS402Ex identity shape'
Require-Regex $control 'ResourceKind\s*=\s*LMC_OWNER_RESOURCE_DS402_HOME_ENGINE[\s\S]{0,700}?LMC_OWNER_KIND_DS402_HOME[\s\S]{0,200}?0x7D15[\s\S]{0,300}?LMC_OWNER_KIND_DS402_HOME_EX[\s\S]{0,200}?0x7D1B' 'ResourceKind 3 admits only paired legacy Home and HomeDS402Ex lifecycle tuples'
Require-Regex $control '0x7D1B:[\s\S]{0,500}?OwnerKind\s*=\s*LMC_OWNER_KIND_DS402_HOME_EX[\s\S]{0,220}?ResourceKind\s*=\s*LMC_OWNER_RESOURCE_DS402_HOME_ENGINE[\s\S]{0,220}?AdmissionMode\s*=\s*LMC_OWNER_ADMISSION_LIFECYCLE' 'exact HomeDS402Ex command tuple validation uses kind 7/resource 3/lifecycle'
Require-Regex $control '0x7D1B:[\s\S]{0,120}?identityExpectedSize\s*:=\s*116[\s\S]{0,450}?oldOwnerKind\s*=\s*LMC_OWNER_KIND_DS402_HOME_EX' 'preemption metadata validates the exact 116-byte HomeDS402Ex identity'
Require-Regex $control 'LMC_OWNER_KIND_DS402_HOME_EX:[\s\S]{0,160}?LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE' 'HomeDS402Ex kind maps to active state 13'
Require-AbsentRegex $control '#define\s+LMC_OWNER_RESOURCE_[A-Z0-9_]+\s+5' 'HOMEEX-07 does not introduce ResourceKind 5'
Require-Regex $control 'LMC_OWNER_KIND_DS402_HOME[\s\S]{0,180}?0x7D15' 'legacy HomeDS402 OwnerKind 4/Start 0x7D15 contract remains present'
Require-Regex $control 'LMC_OWNER_KIND_AXIS_OPERATION_MODE[\s\S]{0,180}?0x7D23' 'SetOperationMode OwnerKind 6/Start 0x7D23 contract remains present'

$copyPreemptionMatches = [regex]::Matches(
    $control,
    'FUNCTION GLOBAL LMCControlCommandService::CopyAxisOwnershipPreemption(?s).*?END_FUNCTION')
Require-True ($copyPreemptionMatches.Count -eq 1) 'CopyAxisOwnershipPreemption function scope is unique'
$copyPreemptionBody = $copyPreemptionMatches[0].Value
Require-RegexCount $copyPreemptionBody '^\s*0x7E53:\s*\n\s*identityExpectedSize\s*:=\s*72;' 1 'Encoder preemption identity remains exactly one 72-byte command case'
Require-AbsentRegex $copyPreemptionBody '^\s*0x7E53:\s*\n\s*identityExpectedSize\s*:=\s*116;' 'HomeDS402Ex command cloning never rewrites Encoder identity to 116 bytes'
Require-RegexCount $copyPreemptionBody '^\s*0x7D1B:\s*\n\s*identityExpectedSize\s*:=\s*116;' 1 'HomeDS402Ex preemption identity remains exactly one 116-byte command case'

# TCP must reserve the full Start identity before the diagnostics handler sees a nonzero owner token.
Require-Regex $tcp 'diagnosticsHomeExStartValid\s*:\s*BOOL' 'TCP declares a dedicated HomeDS402Ex Start classifier'
Require-Regex $tcp 'diagnosticsHomeExSpareIndex\s*:\s*DINT' 'TCP declares the HomeDS402Ex spare-byte scan index'
Require-Regex $tcp '\(CommandID\s*=\s*0x7D1B\)\s*&\s*\(Payload\s*=\s*116\)[\s\S]{0,180}?diagnosticsOwnerKind\s*:=\s*7[\s\S]{0,100}?diagnosticsResourceKind\s*:=\s*3' 'TCP maps HomeDS402Ex Start to OwnerKind 7 / ResourceKind 3'
Require-Regex $tcp 'diagnosticsHomeExSpareIndex\s*:=\s*88[\s\S]{0,180}?diagnosticsHomeExSpareIndex\s*<=\s*119' 'TCP validates all 32 HomeDS402Ex spare bytes before admission'
Require-Regex $tcp 'diagnosticsHomeExMethod\s*:=\s*RequestBuf\[44\]\$DINT' 'TCP reads the frozen HomeDS402Ex homing method offset'
Require-Regex $tcp 'RequestBuf\[48\]\$UDINT\s*<>\s*0x80000000' 'TCP rejects unrepresentable HomeDS402Ex final-position negation before admission'
Require-Regex $tcp 'RequestBuf\[76\]\$UINT\s*=\s*1[\s\S]{0,200}?RequestBuf\[80\]\$UDINT\s*<>\s*0[\s\S]{0,100}?RequestBuf\[84\]\$UDINT\s*<>\s*0' 'TCP enforces Aborting mode and nonzero HomeDS402Ex timeouts'
Require-Regex $tcp 'RequestBuf\[120\]\$UDINT\s*=\s*0x58453448' 'TCP validates the H4EX execute token before ownership admission'
Require-Regex $tcp 'diagnosticsHomeExStartValid[\s\S]{0,350}?ReserveAxisOwnership' 'HomeDS402Ex classifier participates in ownership reservation admission'
Require-Regex $tcp '\(diagnosticsHomeExStartValid\s*=\s*FALSE\)[\s\S]{0,400}?diagnosticsHomeExStartValid\s*&[\s\S]{0,120}?diagnosticsAdmissionResult\s*=\s*0' 'diagnostics dispatch cannot bypass HomeDS402Ex ownership admission'
Require-Regex $tcp 'elsif CommandID\s*=\s*0x7D1B then[\s\S]{0,520}?diagnosticsExactFailure[\s\S]{0,300}?Sendbuf\[24\]\$DINT\s*=\s*RequestBuf\[44\]\$DINT[\s\S]{0,120}?Sendbuf\[28\]\$UDINT\s*=\s*0' 'TCP recognizes exact HomeDS402Ex deterministic failure with method echo and zero native state'

# Diagnostics validates the exact reservation and retained lifecycle while physical execution remains disabled.
Require-RegexCount $diagnostics '^#define LMC_DIAG_DS402_HOME_EX_ENABLED FALSE$' 1 'HomeDS402Ex runtime gate remains exactly OFF'
Require-RegexCount $diagnostics '^#define LMC_DIAG_OWNER_KIND_DS402_HOME_EX 7$' 1 'Diagnostics uses frozen HomeDS402Ex OwnerKind 7'
$startBody = Get-DiagnosticsFunction $diagnostics 'HandleAxisDs402HomeExStart'
$outcomeBody = Get-DiagnosticsFunction $diagnostics 'HandleAxisDs402HomeExOutcome'
$retireBody = Get-DiagnosticsFunction $diagnostics 'HandleAxisDs402HomeExRetire'
$processBody = Get-DiagnosticsFunction $diagnostics 'ProcessAxisDs402HomeEx'
$homeExBodies = $startBody + "`n" + $outcomeBody + "`n" + $retireBody + "`n" + $processBody
Require-Regex $startBody 'RequestSize\s*<>\s*116' 'HomeDS402Ex Start remains exact 116 bytes'
Require-Regex $startBody 'AxisOwnership\.ValidateAxisOwnershipIdentity\([\s\S]{0,700}?CommandId:=0x7D1B[\s\S]{0,300}?OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME_EX[\s\S]{0,180}?ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME[\s\S]{0,180}?AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE[\s\S]{0,350}?RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED[\s\S]{0,180}?IdentitySize:=RequestSize' 'Start validates exact kind7/resource3/reserved full identity before any runtime decision'
Require-AbsentRegex $startBody 'RollbackAxisOwnership' 'Diagnostics HomeDS402Ex Start does not own reservation release'
Require-Regex $tcp 'if \(CommandID\s*=\s*0x7D1B\)\s*\|[\s\S]{0,180}?diagnosticsExactFailure\s*=\s*FALSE[\s\S]{0,180}?RollbackAxisOwnership' 'TCP releases every reserved HomeDS402Ex gate-OFF Start before response completion'
Require-Regex $startBody 'LMC_DIAG_DS402_HOME_EX_ENABLED\s*=\s*FALSE[\s\S]{0,180}?LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE' 'validated Start still fails closed while runtime gate is OFF'
Require-AbsentRegex $startBody 'CommitAxisOwnership' 'HOMEEX-07 Start never commits HomeDS402Ex ownership active'
Require-AbsentRegex $homeExBodies 'SdoAxis[1-4]\.' 'HomeDS402Ex handlers perform no SDO execution before HOMEEX-08'
Require-AbsentRegex $homeExBodies 'InputLatch\.' 'HomeDS402Ex handlers consume no RT latch state before HOMEEX-08'
Require-AbsentRegex $homeExBodies '0x6060|0x6040|0x607A|0x60FF|0x6071' 'HomeDS402Ex handlers contain no physical motion/mode mutation object before HOMEEX-08'
Require-AbsentRegex $startBody 'Ds402HomeExState\s*\[[^\]]+\]\s*:=' 'Start remains read-only with respect to retained outcome state'
Require-AbsentRegex $outcomeBody 'Ds402HomeExState\s*\[[^\]]+\]\s*:=' 'Outcome remains read-only with respect to retained outcome state'
Require-AbsentRegex $processBody 'Ds402HomeExState\s*\[[^\]]+\]\s*:=' 'Cyclic processor still writes no HomeDS402Ex retained state'
$retainedWrites = [regex]::Matches($retireBody, 'Ds402HomeExState\s*\[([^\]]+)\]\s*:=' )
Require-True ($retainedWrites.Count -gt 0) 'Retire contains retained lifecycle writes introduced by HOMEEX-05'
$retainedWriteScopeSafe = $true
foreach ($write in $retainedWrites) {
    $indexExpression = $write.Groups[1].Value.Trim()
    if (($indexExpression -notmatch '^tombstoneBase(?:\s*\+|$)') -and
        ($indexExpression -notmatch '^recordBase(?:\s*\+|$)')) {
        $retainedWriteScopeSafe = $false
        break
    }
}
Require-True $retainedWriteScopeSafe 'HOMEEX-05 retained writes stay inside the per-axis active record or retired tombstone'
Require-Regex $retireBody 'Ds402HomeExState\[tombstoneBase\]\s*:=\s*TO_DINT\(LMC_DIAG_HOMEEX_RETIRED_MAGIC\)' 'Retire publishes the retired tombstone identity before clearing the active record'
Require-Regex $retireBody 'Ds402HomeExState\[recordBase\s*\+\s*recordIndex\]\s*:=\s*0' 'Retire clears only the active per-axis record after tombstone publication'
Require-Regex $processBody 'RETURN;' 'HomeDS402Ex cyclic processor remains a no-op'
Require-Regex $outcomeBody 'RequestSize\s*<>\s*116' 'Outcome requires exact 116-byte key payload'
Require-Regex $outcomeBody 'LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND' 'Outcome retains exact not-found behavior for an empty active record'
Require-Regex $retireBody 'RequestSize\s*<>\s*120' 'Retire requires exact 120-byte payload'
Require-Regex $retireBody 'expectedGeneration\s*=\s*0' 'Retire remains generation-checked'

# Capability remains private through HOMEEX-07/HOMEEX-05 retained-store integration.
$featureMatches = [regex]::Matches($control, '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*0x([0-9A-Fa-f]{8})\s*;')
Require-True ($featureMatches.Count -eq 1) 'Admin feature mask has one canonical assignment'
$featureMask = [Convert]::ToUInt32($featureMatches[0].Groups[1].Value, 16)
Require-True ($featureMask -eq 0x00000017) 'Admin feature mask remains 0x00000017'
Require-True (($featureMask -band 0x00000800) -eq 0) 'HomeDS402Ex capability bit 11 remains OFF'

Require-AsciiFile $controlPath 'LMCControlCommandService.st'
Require-AsciiFile $tcpPath 'TCPMotionInterface.st'
Require-AsciiFile $diagnosticsPath 'LMCDiagnosticsService.st'

Write-Host ("HOMEEX-07 ownership qualification PASS: {0} checks; runtime=OFF; capability=OFF" -f $script:PassCount)

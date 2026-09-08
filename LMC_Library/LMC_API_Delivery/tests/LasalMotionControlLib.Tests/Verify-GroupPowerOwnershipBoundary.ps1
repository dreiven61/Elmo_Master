[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $RepositoryRoot) {
    $RepositoryRoot = Join-Path $PSScriptRoot '../../../..'
}

$tcpPath = Join-Path $RepositoryRoot 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$tcp = Get-Content -Raw -LiteralPath $tcpPath
$control = Get-Content -Raw -LiteralPath $controlPath

$checks = 0
$failures = [Collections.Generic.List[string]]::new()

function Check([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) {
        $script:failures.Add($Name)
    }
}

function Method([string]$Source, [string]$Name) {
    $match = [regex]::Match(
        $Source,
        "(?ms)^FUNCTION (?:GLOBAL )?LMCControlCommandService::$Name\b.*?^END_FUNCTION")
    if (-not $match.Success) {
        throw "Missing LMCControlCommandService method $Name"
    }
    return $match.Value
}

function CaseBlock([string]$Source, [string]$StartPattern, [string]$EndPattern) {
    $match = [regex]::Match(
        $Source,
        "(?is)$StartPattern(?<Body>.*?)$EndPattern")
    if (-not $match.Success) {
        throw "Missing case block $StartPattern -> $EndPattern"
    }
    return $match.Groups['Body'].Value
}

function CheckProfileMaskContract(
    [string]$Source,
    [string]$LabelPattern,
    [string]$VariablePattern,
    [string]$Name
) {
    Check ([regex]::IsMatch(
        $Source,
        "(?is)$LabelPattern.*?$VariablePattern\s*(?:=|<>)\s*LMC_OWNER_PROFILE_AXIS_MASK")) \
        "$Name uses the four-axis profile ownership mask"
    Check (-not [regex]::IsMatch(
        $Source,
        "(?is)$LabelPattern.*?$VariablePattern\s*(?:=|<>)\s*LMC_OWNER_ROBOT_AXIS_MASK")) \
        "$Name does not use the nine-axis power mask as ownership"
}

# The two masks are intentionally different. The observer only owns/profile-tracks
# X/Y/Z/U, while native Group Power dispatch/readback covers all software axes.
Check ($control -match '(?m)^#define\s+LMC_OWNER_PROFILE_AXIS_MASK\s+0x0000000F\s*$') \
    'Profile ownership mask remains Axis1..4'
Check ($control -match '(?m)^#define\s+LMC_OWNER_ROBOT_AXIS_MASK\s+0x000001FF\s*$') \
    'Robot power mask remains Axis1..9'
Check ($control -match '(?m)^#define\s+LMC_OWNER_UNSUPPORTED_AXIS_MASK\s+0x000001F0\s*$') \
    'Ownership observer still treats Axis5..9 as unobserved rather than fabricating evidence'

# TCP admission must reserve only the four-axis profile ownership domain for
# Group Reset/Power and Set Identity. This prevents a successful nine-axis
# PowerOn from creating an unobservable Axis5..9 retained/quarantined owner.
$classifierMatch = [regex]::Match(
    $tcp,
    '(?is)//\s*LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN(?<Body>.*?)//\s*LMC_OWNER_ORDINARY_CLASSIFIER_END')
if (-not $classifierMatch.Success) {
    throw 'TCP ordinary ownership classifier block is missing.'
}
$classifier = $classifierMatch.Groups['Body'].Value

$tcpPowerOn = CaseBlock $classifier '0x2049\s*,\s*0x204A\s*:' '0x204B\s*:'
$tcpPowerOff = CaseBlock $classifier '0x204B\s*:' '0x2085\s*:'
$tcpSetKin = [regex]::Match(
    $classifier,
    '(?is)0x20E7\s*:\s*controlClassifierValid\s*:=.*?(?=\s*else\s*\r?\n\s*end_case;)').Value
Check (-not [string]::IsNullOrWhiteSpace($tcpSetKin)) 'TCP Set Identity classifier arm exists'
foreach ($entry in @(
    @{ Text = $tcpPowerOn; Name = 'TCP Group Reset/PowerOn ownership' },
    @{ Text = $tcpPowerOff; Name = 'TCP Group PowerOff ownership' },
    @{ Text = $tcpSetKin; Name = 'TCP Set Identity ownership' }
)) {
    Check ($entry.Text -match 'controlAxisMask\s*:=\s*LMC_OWNER_PROFILE_AXIS_MASK') \
        ($entry.Name + ' selects Axis1..4')
    Check ($entry.Text -notmatch 'controlAxisMask\s*:=\s*LMC_OWNER_ROBOT_AXIS_MASK') \
        ($entry.Name + ' does not reserve Axis5..9')
}

$reserve = Method $control 'ReserveAxisOwnership'
$validate = Method $control 'ValidateAxisOwnership'
$validateIdentity = Method $control 'ValidateAxisOwnershipIdentity'
$repeat = Method $control 'HandleAxisOwnershipSafetyRepeat'
$process = Method $control 'ProcessAxisOwnership'
$handle = Method $control 'HandleRequest'
$groupHandler = Method $control 'HandleGroupCommands'
$groupStatus = Method $control 'GroupReadStatus'

foreach ($methodEntry in @(
    @{ Text = $reserve; Variable = 'RequestedAxisMask'; Name = 'ReserveAxisOwnership' },
    @{ Text = $validate; Variable = 'ExpectedAxisMask'; Name = 'ValidateAxisOwnership' },
    @{ Text = $validateIdentity; Variable = 'ExpectedAxisMask'; Name = 'ValidateAxisOwnershipIdentity' }
)) {
    $text = $methodEntry.Text
    $variable = $methodEntry.Variable
    $name = $methodEntry.Name
    CheckProfileMaskContract $text '0x2049\s*,\s*0x204A\s*:' $variable "$name Group Reset/PowerOn"
    CheckProfileMaskContract $text '0x204B\s*:' $variable "$name Group PowerOff"
    CheckProfileMaskContract $text '0x20E7\s*:' $variable "$name Set Identity"
}

# Identity/snapshot/preemption metadata must preserve the same ownership mask.
# These are bounded by the exact command labels/command constants so the native
# 0x204A/0x204B power-dispatch mask below remains free to be 0x01FF.
$metadataContracts = @(
    @{ Pattern = '(?is)0x2049\s*,\s*0x204A\s*:\s*identityExpectedSize.*?oldAxisMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Group Reset/PowerOn retained identity uses profile mask' },
    @{ Pattern = '(?is)0x204B\s*:\s*identityExpectedSize.*?oldAxisMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Group PowerOff retained identity uses profile mask' },
    @{ Pattern = '(?is)0x20E7\s*:\s*identityExpectedSize.*?oldAxisMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Set Identity retained identity uses profile mask' },
    @{ Pattern = '(?is)\(snapshotCommand\s*=\s*0x2049\)\s*\|\s*\(snapshotCommand\s*=\s*0x204A\).*?snapshotMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Group Reset/PowerOn preemption snapshot uses profile mask' },
    @{ Pattern = '(?is)snapshotCommand\s*=\s*0x204B.*?snapshotMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Group PowerOff preemption snapshot uses profile mask' },
    @{ Pattern = '(?is)snapshotCommand\s*=\s*0x20E7.*?snapshotMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Set Identity preemption snapshot uses profile mask' },
    @{ Pattern = '(?is)LMC_OWNER_COMMAND_GROUP_RESET\s*,\s*LMC_OWNER_COMMAND_GROUP_POWER_ON\s*:.*?OwnershipState\[liveGroupRecordBase\s*\+\s*11\]\$UDINT\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Live Group Reset/PowerOn identity uses profile mask' },
    @{ Pattern = '(?is)LMC_OWNER_COMMAND_GROUP_POWER_OFF\s*:.*?OwnershipState\[liveGroupRecordBase\s*\+\s*11\]\$UDINT\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Live Group PowerOff identity uses profile mask' },
    @{ Pattern = '(?is)LMC_OWNER_COMMAND_GROUP_SET_KIN\s*:.*?OwnershipState\[liveGroupRecordBase\s*\+\s*11\]\$UDINT\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK'; Name = 'Live Set Identity identity uses profile mask' }
)
foreach ($contract in $metadataContracts) {
    Check ([regex]::IsMatch($control, $contract.Pattern)) $contract.Name
}

Check ($repeat -match '(?is)\(oldCommand\s*=\s*LMC_OWNER_COMMAND_GROUP_POWER_OFF\)\s*&\s*\(oldAxisMask\s*<>\s*LMC_OWNER_PROFILE_AXIS_MASK\)') \
    'Safety repeat validates retained Group PowerOff with profile mask'
Check ($repeat -match '(?is)\(oldOwnerKind\s*=\s*LMC_OWNER_KIND_GROUP\)\s*&\s*\(oldAxisMask\s*<>\s*LMC_OWNER_PROFILE_AXIS_MASK\)') \
    'Safety escalation observer validates Group PowerOff with profile mask'
Check ($control -match '(?is)\(repeatCommand\s*=\s*LMC_OWNER_COMMAND_GROUP_POWER_OFF\)\s*&\s*\(repeatAxisMask\s*<>\s*LMC_OWNER_PROFILE_AXIS_MASK\)') \
    'Reservation safety repeat validates Group PowerOff with profile mask'

$safetyPowerOffRoots = [regex]::Matches(
    $control,
    '(?is)0x204B\s*:.*?safetyAxisMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK')
Check ($safetyPowerOffRoots.Count -ge 2) \
    'Group PowerOff preemption roots recognize profile ownership mask'

# Actual power semantics stay nine-axis. Ownership is not used to narrow native
# PowerOn/PowerOff or status proof.
$nativePowerOn = CaseBlock $groupHandler '0x204A\s*:' '0x204B\s*:'
$nativePowerOff = CaseBlock $groupHandler '0x204B\s*:' '0x2085\s*:'
foreach ($entry in @(
    @{ Text = $nativePowerOn; Name = 'Native Group PowerOn' },
    @{ Text = $nativePowerOff; Name = 'Native Group PowerOff' }
)) {
    Check ($entry.Text -match 'groupAxisMask\s*:=\s*LMC_OWNER_ROBOT_AXIS_MASK') \
        ($entry.Name + ' still targets Axis1..9')
    Check ($entry.Text -match 'SetResolvedGroupAxesPower\(GroupAxisMask:=groupAxisMask') \
        ($entry.Name + ' still reaches the all-axis power helper')
}
Check ($groupStatus -match 'AreResolvedGroupAxesPowered\(\s*GroupAxisMask:=LMC_OWNER_ROBOT_AXIS_MASK\)') \
    'Group status PowerOn proof still reads Axis1..9'
Check ($process -match 'AreResolvedGroupAxesPowered\(\s*GroupAxisMask:=LMC_OWNER_ROBOT_AXIS_MASK\)') \
    'Ownership observer Group power proof still requires Axis1..9'
Check ($process -match 'unsupportedAxisSelected\s*:=\s*\(axisMask\s+and\s+LMC_OWNER_UNSUPPORTED_AXIS_MASK\)\s*<>\s*0') \
    'Observer retains explicit fail-closed handling for unobserved Axis5..9 owners'

# Set Identity is synchronous. Once its four-axis lifecycle reservation is
# accepted, it must commit and immediately publish terminal success so Profile
# Lock can acquire the next Group lease without waiting for the cyclic observer.
Check ($handle -match '(?is)if\s+CommandId\s*=\s*0x20E7\s+then\s*ownershipPublishResult\s*:=\s*PublishAxisOwnership\(.*?ReportKind:=LMC_OWNER_REPORT_TERMINAL_SUCCESS') \
    'Set Identity retires its lifecycle owner immediately after successful commit'

if ($failures.Count -gt 0) {
    Write-Host "FAIL GroupPowerOwnershipBoundary: $($failures.Count)/$checks checks failed."
    foreach ($failure in $failures) {
        Write-Host "  - $failure"
    }
    exit 1
}

Write-Host "PASS GroupPowerOwnershipBoundary: $checks/$checks source-contract checks."

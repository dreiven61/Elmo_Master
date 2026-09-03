[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$Revision,
    [switch]$IncludeRebaseContractSelfTest,
    [switch]$IncludeSafetyRepeatContractSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $RepositoryRoot) { $RepositoryRoot = Join-Path $PSScriptRoot '../../../..' }
$relative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$source = if ($Revision) {
    $lines = & git -C $RepositoryRoot show "${Revision}:$relative"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot load requested source revision.' }
    $lines -join "`n"
} else { Get-Content -Raw -LiteralPath (Join-Path $RepositoryRoot $relative) }
$constants = @{}
foreach ($match in [regex]::Matches($source, '(?m)^#define\s+(\w+)\s+(0x[0-9A-Fa-f]+|\d+)\s*$')) {
    $constants[$match.Groups[1].Value] = $match.Groups[2].Value
}
$checks = 0
$failures = [Collections.Generic.List[string]]::new()
function Check([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $script:failures.Add($Name) }
}
function Method([string]$Name) {
    $match = [regex]::Match($source,
        "(?ms)^FUNCTION (?:GLOBAL )?LMCControlCommandService::$Name\b.*?^END_FUNCTION")
    if (-not $match.Success) { throw "Missing method $Name" }
    [regex]::Replace($match.Value, '//[^\r\n]*', '')
}
function Capture([string]$Text, [string]$Pattern) {
    $match = [regex]::Match($Text, '(?is)' + $Pattern)
    if (-not $match.Success) { throw "Missing expression: $Pattern" }
    $match.Groups['Expression'].Value
}
# Evaluate the actual ST predicates, not a second handwritten allow-list.
# Only numeric/boolean literals, known names, comparisons and boolean operators
# are accepted. No arbitrary source text reaches ScriptBlock.Create.
function Predicate([string]$Expression, [hashtable]$Values) {
    $remaining = $Expression.Trim()
    $parts = [Collections.Generic.List[string]]::new()
    $operators = @{ '('='('; ')'=')'; '&'='-and'; '|'='-or'; '='='-eq'; '<>'='-ne' }
    while ($remaining.Length -gt 0) {
        $token = [regex]::Match($remaining, '^(0x[0-9A-Fa-f]+|\d+|[A-Za-z_]\w*|<>|[()&|=])')
        if (-not $token.Success) { throw "Unsupported predicate token: $remaining" }
        $name = $token.Value
        if ($operators.ContainsKey($name)) { $parts.Add($operators[$name]) }
        elseif ($name -eq 'TRUE') { $parts.Add('$true') }
        elseif ($name -eq 'FALSE') { $parts.Add('$false') }
        elseif ($constants.ContainsKey($name)) { $parts.Add($constants[$name]) }
        elseif ($Values.ContainsKey($name)) {
            $value = $Values[$name]
            if ($value -is [bool]) { $parts.Add('$' + $value.ToString().ToLowerInvariant()) }
            else { $parts.Add(([long]$value).ToString([Globalization.CultureInfo]::InvariantCulture)) }
        }
        elseif ($name -match '^(0x[0-9A-Fa-f]+|\d+)$') { $parts.Add($name) }
        else { throw "Unknown predicate identifier: $name" }
        $remaining = $remaining.Substring($token.Length).TrimStart()
    }
    [bool](& ([scriptblock]::Create('(' + ($parts -join ' ') + ')')))
}

$reserve = Method 'ReserveAxisOwnership'
$helper = Method 'HandleAxisOwnershipSafetyRepeat'
$process = Method 'ProcessAxisOwnership'
$publish = Method 'PublishAxisOwnership'
$allow = Capture $reserve 'rebaseAdmissionAllowed\s*:=\s*(?<Expression>.*?);'
$helperGuard = ($helper -split 'case\s+CommandId\s+of\s+LMC_OWNER_COMMAND_AXIS_STOP\s*:')[0]
Check ($helperGuard -notmatch '\b(?:0x2023|LMC_OWNER_COMMAND_AXIS_POWER)\b') 'Power must pass the second rebase guard too'
foreach ($command in @(0x2023,0x2024,0x209F,0x20A0,0x20A2,0x2047,0x204A,0x7D12)) {
    $values = @{ CommandId=$command; OwnerKind=1; ResourceKind=1; AdmissionMode=1 }
    $expected = $command -in @(0x2023,0x2024)
    Check ((Predicate $allow $values) -eq $expected) ("Rebase ordinary command {0:X4}" -f $command)
}
foreach ($owner in @(1,2,3)) {
    foreach ($resource in @(1,2,3,4)) {
        foreach ($mode in @(1,2,3,4)) {
            $values = @{ CommandId=0x2023; OwnerKind=$owner; ResourceKind=$resource; AdmissionMode=$mode }
            $expected = ($mode -eq 2) -or (($owner -eq 1) -and ($resource -eq 1) -and ($mode -eq 1))
            Check ((Predicate $allow $values) -eq $expected) "Power exception tuple $owner/$resource/$mode"
        }
    }
}
Check ($reserve -match '(?s)OwnershipState\[24\]\s*<>\s*0\)\s*then\s*Result\s*:=\s*-3;\s*RETURN;') 'Global quarantine gate preserved'
Check ($reserve -notmatch 'OwnershipState\[24\]\s*:=\s*0') 'No force-clear on reservation'
Check ($reserve -notmatch 'AxisRebaseRequiredState\.Write\(') 'Power admission never clears coordinate barrier'
Check ($reserve -match '(?s)rebaseAdmissionAllowed\s*=\s*FALSE.*?Result\s*:=\s*LMC_OWNER_REBASE_REQUIRED;\s*RETURN;') 'Motion rebase rejection preserved'

$off = Capture $process '0x2023:\s*if\s+admissionMode\s*=\s*LMC_OWNER_ADMISSION_SAFETY\s+then\s*terminalCandidate\s*:=\s*(?<Expression>.*?);'
$on = Capture $process '0x2023:.*?else\s*terminalCandidate\s*:=\s*(?<Expression>.*?);'
$failure = Capture $process 'if\s+(?<Expression>terminalCandidate\s*&.*?)\s+then\s*safeFailure\s*:=\s*TRUE;'
foreach ($disabled in @($false,$true)) {
    foreach ($stopped in @($false,$true)) {
        foreach ($errorClear in @($false,$true)) {
            $values = @{ allPowerOff=$disabled; allStandstill=$stopped; allPowerOn=(-not $disabled);
                allErrorClear=$errorClear; groupError=0; ownerKind=1; commandId=0x2023; admissionMode=2 }
            $candidate = Predicate $off $values
            Check ($candidate -eq ($disabled -and $stopped)) "PowerOff physical completion $disabled/$stopped/$errorClear"
            $values.terminalCandidate = $candidate
            Check (-not (Predicate $failure $values)) "Completed PowerOff does not retain an alarm-only owner $disabled/$stopped/$errorClear"
            $values.admissionMode = 1
            $candidateOn = Predicate $on $values
            Check ($candidateOn -eq ((-not $disabled) -and $errorClear)) 'PowerOn still requires enabled and error-clear evidence'
        }
    }
}
foreach ($command in @(0x2022,0x209F,0x20A0,0x204B)) {
    $values = @{ terminalCandidate=$true; allPowerOff=$true; allStandstill=$true;
        allErrorClear=$false; groupError=0; ownerKind=1; commandId=$command; admissionMode=2 }
    Check (Predicate $failure $values) ("Unrelated failed command {0:X4} stays fail-closed" -f $command)
}
Check ($process -match '(?s)terminalCandidate\s*&.*?LMC_OWNER_ORDINARY_STABLE_SAMPLES.*?stableElapsed\s*>=\s*LMC_OWNER_ORDINARY_STABLE_MS') 'Stable samples and duration retained'
Check ($process -match '(?s)if forceQuarantine then\s*reportKind := LMC_OWNER_REPORT_QUARANTINE;\s*elsif safeFailure then') 'Uncertain/preempted outcome overrides successful power-off'
Check ($process -match '(?s)ownerElapsed > LMC_OWNER_ORDINARY_TIMEOUT_MS.*?reportKind := LMC_OWNER_REPORT_QUARANTINE;') 'Timeout still quarantines, no replay'
Check ($publish -match '(?s)LMC_OWNER_REPORT_TERMINAL_SUCCESS:.*?elsif clearOwner then.*?#OwnershipState\[recordBase\].*?#OwnershipObserverState\[observerBase\]') 'Proven terminal success retires owner and observer together'
Check ($process -notmatch 'OwnershipState\[24\]\s*:=\s*0') 'Observer never clears an existing integrity latch'

# Reproduce the BootId 137 RESERVED PowerOn capture at the helper boundary.
# These are source-derived routing predicates with an explicit identity-validator
# stub, not execution of the full LASAL method or a PLC integration test.
function RoutingExpression([string]$Expression) {
    $value = $Expression
    $value = $value.Replace('(pRequestFrame + 8)^$UDINT', 'wireVersion')
    foreach ($offset in @(12,13,14,15)) {
        $value = $value.Replace("(pRequestFrame + $offset)^", "wire$offset")
    }
    $value = $value.Replace('OwnershipState[0]$UDINT', 'tableMagic')
    $value = $value.Replace('OwnershipState[3]$UDINT', 'bootId')
    $value = $value.Replace('OwnershipState[24]', 'quarantine')
    $value = [regex]::Replace($value, 'TO_DINT\((\w+)\)', '$1')
    $value
}
$powerBranch = Capture $helper 'LMC_OWNER_COMMAND_AXIS_POWER:\s*(?<Expression>.*?)LMC_OWNER_COMMAND_GROUP_DISABLE:'
$offShape = RoutingExpression (Capture $powerBranch 'repeatShapeValid\s*:=\s*(?<Expression>.*?);')
$firstShapeMatch = [regex]::Match($powerBranch, '(?s)firstDispatchShapeValid\s*:=\s*(?<Expression>.*?);')
$firstShape = if ($firstShapeMatch.Success) { RoutingExpression $firstShapeMatch.Groups['Expression'].Value } else { 'FALSE' }
$initialValidity = RoutingExpression (Capture $helper 'repeatValid\s*:=\s*(?<Expression>.*?);')
$freshGate = RoutingExpression (Capture $helper 'if\s+(?<Expression>repeatValid\s*&.*?)\s+then\s*validationResult\s*:=\s*ValidateAxisOwnershipIdentity')
$earlyGate = Capture $helper 'if\s+(?<Expression>\(repeatEligible\s*=\s*FALSE\).*?)\s+then\s*RETURN;'
$freshCall = Capture $helper 'validationResult\s*:=\s*ValidateAxisOwnershipIdentity\((?<Expression>.*?)\);'
$modeArgument = Capture $freshCall 'AdmissionMode\s*:=\s*(?<Expression>\w+)\s*,'
$defaults = @{ RequestFrameSize=16; referenceAxisMask=1; wireVersion=1; wire12=1; wire13=1; wire14=0; wire15=1;
    tableMagic=0x4C4D434F; bootId=137; quarantine=0; repeatEligible=$true;
    AdmissionToken=7; OwnerGeneration=7; oldState=1; oldCommand=0x2023; CommandId=0x2023;
    oldReference=1; Reference=1; oldSession=4; CallerSessionEpoch=4; oldSequence=58; RequestSequence=58 }
function FirstDispatch([hashtable]$Overrides, [bool]$IdentityValid=$true) {
    $values = $defaults.Clone()
    foreach ($key in $Overrides.Keys) { $values[$key] = $Overrides[$key] }
    if (Predicate $earlyGate $values) { return $false }
    # This guard is outside the assignment and prevents any short frame read.
    $shapeScope = ($values.RequestFrameSize -eq 16) -and ($values.referenceAxisMask -ne 0)
    $values.repeatShapeValid = $shapeScope -and (Predicate $offShape $values)
    $powerOnShape = $shapeScope -and (Predicate $firstShape $values)
    $values.firstDispatchShapeValid = $powerOnShape -or $values.repeatShapeValid
    $mode = if ($modeArgument -eq 'firstDispatchAdmissionMode') {
        if ($powerOnShape) { 1 } else { 2 }
    } elseif ($constants.ContainsKey($modeArgument)) { [int]$constants[$modeArgument] }
    else { throw "Unsupported first-dispatch admission argument: $modeArgument" }
    $values.repeatValid = Predicate $initialValidity $values
    $expectedMode = if ($values.wire12 -eq 1) { 1 } else { 2 }
    (Predicate $freshGate $values) -and $IdentityValid -and ($mode -eq $expectedMode)
}
Check (FirstDispatch @{}) 'Boot137 token7/sequence58 PowerOn reaches normal first dispatch, not -9'
Check (FirstDispatch @{ wire12=0 }) 'Fresh PowerOff still reaches normal safety dispatch'
Check (-not (FirstDispatch @{} $false)) 'Failed exact identity validation never passes first dispatch'
foreach ($entry in @(
    @{ oldState=0 }, @{ oldState=2 }, @{ oldState=10 }, @{ oldState=11 },
    @{ oldCommand=0x2022 }, @{ oldReference=2 }, @{ oldSession=3 }, @{ oldSequence=57 },
    @{ AdmissionToken=0 }, @{ OwnerGeneration=0 }, @{ tableMagic=0 }, @{ bootId=0 }, @{ quarantine=1 },
    @{ RequestFrameSize=15 }, @{ RequestFrameSize=17 }, @{ referenceAxisMask=0 },
    @{ wireVersion=2 }, @{ wire12=2 }, @{ wire13=0 }, @{ wire14=1 }, @{ wire15=0 })) {
    Check (-not (FirstDispatch $entry)) ('Reject non-fresh/malformed PowerOn: ' + ($entry.Keys -join ','))
}
Check (-not (Predicate $offShape $defaults)) 'PowerOn is never a valid safety-repeat payload'
$offValues = $defaults.Clone(); $offValues.wire12 = 0
Check (Predicate $offShape $offValues) 'PowerOff remains a valid safety-repeat payload'
Check ($helper -match '(?s)firstDispatchAdmissionMode\s*:=\s*LMC_OWNER_ADMISSION_SAFETY;.*?if firstDispatchShapeValid then\s*firstDispatchAdmissionMode\s*:=\s*LMC_OWNER_ADMISSION_ORDINARY;') 'Admission mode switches only for the exact fresh PowerOn shape'
Check ($helper -match 'firstDispatchShapeValid\s*:=\s*firstDispatchShapeValid\s*\|\s*repeatShapeValid;') 'Fresh routing preserves existing safety shapes'
Check ($helper -match '(?s)if repeatFresh then\s*Result := LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE;\s*RETURN;\s*end_if;\s*repeatValid := repeatValid & repeatShapeValid;') 'PowerOn cannot enter coalescing or escalation after failed fresh validation'
foreach ($argument in @('CommandId:=CommandId','Reference:=Reference','AdmissionToken:=AdmissionToken',
    'OwnerGeneration:=OwnerGeneration','CallerSessionEpoch:=CallerSessionEpoch','RequestSequence:=RequestSequence',
    'RequiredPhase:=LMC_OWNER_PHASE_RESERVED','pIdentity:=(pRequestFrame + 8)$^void','IdentitySize:=RequestFrameSize - 8')) {
    Check ($freshCall.Replace(' ','').Replace("`t",'').Contains($argument.Replace(' ',''))) "Fresh identity argument preserved: $argument"
}
Check ($helper -notmatch '\b(?:Reserve|Commit|Rollback)AxisOwnership\s*\(') 'Repeat helper does not mutate ownership lifecycle'

foreach ($name in $failures) { Write-Host "FAIL $name" }
if ($failures.Count) { throw "ServoPowerLifecycle: $($failures.Count)/$checks failed" }
Write-Host "PASS ServoPowerLifecycle: $checks/$checks source-predicate checks (no PLC execution)"

if ($IncludeRebaseContractSelfTest -or $IncludeSafetyRepeatContractSelfTest) {
    if ($Revision) { throw 'The contract self-tests operate on the working tree only.' }
    # Load only definitions: unrelated legacy top-level gates must not prevent
    # this focused regression from exercising the existing negative fixtures.
    $verifier = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile(
        $verifier, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count) { throw $parseErrors[0] }
    $definitions = ($ast.EndBlock.Statements | Where-Object {
        $_ -is [Management.Automation.Language.FunctionDefinitionAst]
    } | ForEach-Object { $_.Extent.Text }) -join "`n"
    $definitions = $definitions.Replace('$PSScriptRoot',
        ("'" + $PSScriptRoot.Replace("'", "''") + "'"))
    . ([scriptblock]::Create($definitions))
    if ($IncludeRebaseContractSelfTest) {
        $negativeCount = Invoke-LasalAxisRebaseBarrierVerifierSelfTest
        Write-Host "PASS RebaseContract: $negativeCount/$negativeCount negative fixtures rejected"
    }
    if ($IncludeSafetyRepeatContractSelfTest) {
        $negativeCount = Invoke-LasalOrdinarySafetyRepeatVerifierSelfTest
        Write-Host "PASS SafetyRepeatContract: $negativeCount/$negativeCount negative fixtures rejected"
    }
}

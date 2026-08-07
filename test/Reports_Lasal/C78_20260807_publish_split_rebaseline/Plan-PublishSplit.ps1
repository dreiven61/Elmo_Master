param(
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..'),
    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# This planner is intentionally read-only. It produces and validates a complete
# split candidate in memory, but never writes the LASAL source or generated data.
$ExpectedSourceCanonicalLfSha256 =
    '7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE'
$ExpectedSourceIdeCrlfSha256 =
    'A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753'
$ExpectedMethodCanonicalLfSha256 =
    '688241F3FD3DE43DC9B95B7A4AB0E7160C2F31D7FCFD4529AC18E8946E034F18'
$ExpectedHomeExtractionCanonicalLfSha256 =
    '84A8FE035018CC10F595EEF8024357E0EDD75035BAE9C51F11B85D49547FBFF1'
$ExpectedHomeExtractionIdeCrlfSha256 =
    '3B9C74787829FDF51B1F7E3EF2F7DB4FE1519AC17A7C146C60B56E0785507E2D'
$ExpectedDecisionExtractionCanonicalLfSha256 =
    '90B7E61AA6F4F85896835C7F0EE05855930FE73A891546E8832A46196213DB8E'
$ExpectedDecisionExtractionIdeCrlfSha256 =
    'E2E06E5ADBF2F526C765E893512D365C008A6AE9BA1C1494500BB1133D5D58A3'
$MethodSizeLimitBytes = 32768
$Utf8 = [Text.UTF8Encoding]::new($false, $true)

function ConvertTo-CanonicalLf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function ConvertTo-IdeCrlf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return (ConvertTo-CanonicalLf -Text $Text).Replace("`n", "`r`n")
}

function Get-TextSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($Text)))
}

function Get-ByteDimensions {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $lfText = ConvertTo-CanonicalLf -Text $Text
    $crlfText = ConvertTo-IdeCrlf -Text $lfText
    return [ordered]@{
        raw = $Utf8.GetByteCount($Text)
        lf = $Utf8.GetByteCount($lfText)
        crlf = $Utf8.GetByteCount($crlfText)
    }
}

function Get-MethodContent {
    param([Parameter(Mandatory = $true)][string]$Method)

    $lfMethod = ConvertTo-CanonicalLf -Text $Method
    if (-not $lfMethod.EndsWith("`n", [StringComparison]::Ordinal)) {
        throw 'Method block does not end with one canonical LF.'
    }
    return $lfMethod.Substring(0, $lfMethod.Length - 1)
}

function Replace-ExactOne {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Old,
        [AllowEmptyString()][string]$New,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    $last = $Text.LastIndexOf($Old, [StringComparison]::Ordinal)
    if (($first -lt 0) -or ($first -ne $last)) {
        throw "$Owner exact replacement count is not one."
    }
    return $Text.Substring(0, $first) + $New +
        $Text.Substring($first + $Old.Length)
}

function Assert-SourceRatchet {
    param(
        [Parameter(Mandatory = $true)][string]$InputText,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $canonicalLf = ConvertTo-CanonicalLf -Text $InputText
    $ideCrlf = ConvertTo-IdeCrlf -Text $canonicalLf
    $lineEnding = if ($InputText -ceq $canonicalLf) {
        'LF'
    }
    elseif ($InputText -ceq $ideCrlf) {
        'CRLF'
    }
    else {
        throw "$Owner uses mixed or unsupported line endings."
    }
    $canonicalSha = Get-TextSha256 -Text $canonicalLf
    $ideSha = Get-TextSha256 -Text $ideCrlf
    if (($canonicalSha -cne $ExpectedSourceCanonicalLfSha256) -or
        ($ideSha -cne $ExpectedSourceIdeCrlfSha256)) {
        throw (
            "$Owner A51E canonical LF/IDE CRLF ratchet drifted " +
            "($canonicalSha/$ideSha).")
    }
    return [pscustomobject]@{
        CanonicalLf = $canonicalLf
        IdeCrlf = $ideCrlf
        LineEnding = $lineEnding
        PhysicalSha256 = Get-TextSha256 -Text $InputText
        PhysicalBytes = $Utf8.GetByteCount($InputText)
    }
}

function Get-NormalizedInventory {
    param([Parameter(Mandatory = $true)][object[]]$Matches)

    return [string]::Join('|', @(
            foreach ($match in $Matches) {
                [regex]::Replace(
                    $match.Value, '\s+', '').ToLowerInvariant()
            }))
}

function Get-PersistentMutationMatches {
    param([Parameter(Mandatory = $true)][string]$Text)

    $pattern = (
        '(?is)(?:' +
        '\b(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[[^;]+?' +
        '\]\s*(?:\$[A-Za-z_][A-Za-z0-9_]*\s*)?' +
        '(?::|\+|-|\*|/|and|or|xor)\s*=\s*[^;]+;' +
        '|' +
        '\b_memset\s*\(\s*dest\s*:=\s*#' +
        '(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[.*?\)\s*;' +
        '|' +
        '\b_memcpy\s*\(\s*ptr1\s*:=\s*#' +
        '(?:Ownership[A-Za-z0-9_]*State|ZeroHomeState)\s*\[.*?\)\s*;' +
        '|' +
        '\bUpdateAxisRebaseRequiredState\s*\(.*?\)\s*;' +
        ')')
    return @([regex]::Matches($Text, $pattern))
}

function Get-CallHistogram {
    param([Parameter(Mandatory = $true)][string]$Text)

    $histogram = [ordered]@{}
    $controlWords = @(
        'if', 'elsif', 'while', 'case', 'for', 'and', 'or', 'not')
    foreach ($match in [regex]::Matches(
            $Text,
            '(?i)(?<![A-Za-z0-9_])(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*\(')) {
        $name = $match.Groups['Name'].Value
        if ($controlWords -contains $name.ToLowerInvariant()) {
            continue
        }
        $key = $name.ToLowerInvariant()
        if (-not $histogram.Contains($key)) {
            $histogram[$key] = 0
        }
        $histogram[$key]++
    }
    return $histogram
}

function Get-HistogramInventory {
    param([Parameter(Mandatory = $true)][Collections.IDictionary]$Histogram)

    return [string]::Join('|', @(
            foreach ($key in @($Histogram.Keys | Sort-Object)) {
                "$key=$($Histogram[$key])"
            }))
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$sourcePath = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$sourceInputPhysical = [IO.File]::ReadAllText($sourcePath, $Utf8)
$sourceCheckpoint = Assert-SourceRatchet `
    -InputText $sourceInputPhysical `
    -Owner 'Publish split source input'
$source = $sourceCheckpoint.CanonicalLf

$methodPattern = (
    '(?ms)^FUNCTION GLOBAL ' +
    'LMCControlCommandService::PublishAxisOwnership\n' +
    '.*?^END_FUNCTION\n')
$methodMatches = [regex]::Matches($source, $methodPattern)
if ($methodMatches.Count -ne 1) {
    throw "PublishAxisOwnership method count is $($methodMatches.Count), expected one."
}
$method = $methodMatches[0].Value
$methodSha256 = Get-TextSha256 -Text $method
if ($methodSha256 -cne $ExpectedMethodCanonicalLfSha256) {
    throw "PublishAxisOwnership canonical LF method drifted ($methodSha256)."
}

$publicInterface = [string]::Join("`n", @(
        'FUNCTION GLOBAL LMCControlCommandService::PublishAxisOwnership',
        "`tVAR_INPUT",
        "`t`tAxisMask : UDINT;",
        "`t`tAdmissionToken : UDINT;",
        "`t`tOwnerGeneration : UDINT;",
        "`t`tReportKind : UINT;",
        "`t`tReportValue0 : UDINT;",
        "`t`tReportValue1 : UDINT;",
        "`t`tObservationCycle : UDINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult : DINT;",
        "`tEND_VAR",
        "`tVAR"
    )) + "`n"
if (-not $method.StartsWith($publicInterface, [StringComparison]::Ordinal)) {
    throw 'PublishAxisOwnership public implementation ABI/order drifted.'
}

$localVarMatches = [regex]::Matches(
    $method, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
if ($localVarMatches.Count -ne 1) {
    throw 'PublishAxisOwnership local VAR block count is not one.'
}
$originalLocalVarBlock = $localVarMatches[0].Value
$localDeclarationMatches = [regex]::Matches(
    $originalLocalVarBlock,
    '(?m)^\t\t(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:(?!=)[^;\n]+;\n')
$localDeclarationByName = @{}
foreach ($match in $localDeclarationMatches) {
    $name = $match.Groups['Name'].Value
    if ($localDeclarationByName.ContainsKey($name)) {
        throw "Publish local '$name' is duplicated."
    }
    $localDeclarationByName[$name] = $match.Value
}
if ($localDeclarationByName.Count -ne 93) {
    throw "Publish local declaration count is $($localDeclarationByName.Count), expected 93."
}

$homeExtractionPattern = (
    '(?ms)(^\tif ZeroHomeState\[60\]\$UDINT = ' +
    'LMC_HOME_OWNER_RECEIPT_MAGIC then\n.*?^\tend_if;\n)' +
    '(?=\texpectedResourceKind := 0;)')
$homeExtractionMatches = [regex]::Matches($method, $homeExtractionPattern)
if ($homeExtractionMatches.Count -ne 1) {
    throw 'Publish Home extraction count is not one.'
}
$homeExtraction = $homeExtractionMatches[0].Groups[1].Value
$homeExtractionContent = Get-MethodContent -Method $homeExtraction
$homeExtractionLfSha256 = Get-TextSha256 -Text $homeExtractionContent
$homeExtractionCrlfSha256 = Get-TextSha256 -Text (
    ConvertTo-IdeCrlf -Text $homeExtractionContent)
if (($homeExtractionLfSha256 -cne
        $ExpectedHomeExtractionCanonicalLfSha256) -or
    ($homeExtractionCrlfSha256 -cne
        $ExpectedHomeExtractionIdeCrlfSha256)) {
    throw 'Publish Home extraction LF/CRLF ratchet drifted.'
}

$decisionExtractionPattern = (
    '(?ms)(^\tpreemptRootValid :=\n.*?^\tend_if;\n)' +
    '(?=\tdestroyBanks :=)')
$decisionExtractionMatches = [regex]::Matches(
    $method, $decisionExtractionPattern)
if ($decisionExtractionMatches.Count -ne 1) {
    throw 'Publish decision extraction count is not one.'
}
$decisionExtraction = $decisionExtractionMatches[0].Groups[1].Value
$decisionExtractionContent = Get-MethodContent -Method $decisionExtraction
$decisionExtractionLfSha256 = Get-TextSha256 -Text $decisionExtractionContent
$decisionExtractionCrlfSha256 = Get-TextSha256 -Text (
    ConvertTo-IdeCrlf -Text $decisionExtractionContent)
if (($decisionExtractionLfSha256 -cne
        $ExpectedDecisionExtractionCanonicalLfSha256) -or
    ($decisionExtractionCrlfSha256 -cne
        $ExpectedDecisionExtractionIdeCrlfSha256)) {
    throw 'Publish decision extraction LF/CRLF ratchet drifted.'
}

$homeExtractionOnlyLocals = @(
    'homeReceiptExact',
    'homeReceiptCallMatches',
    'homeReceiptTerminalComplete',
    'homeReceiptIdentityPre',
    'homeReceiptIdentityPost',
    'homeReceiptObserverPre',
    'homeReceiptObserverPost',
    'homeReceiptRecordPre',
    'homeReceiptRecordBodyPost',
    'homeReceiptRecordPost',
    'homeReceiptSingletonPre',
    'homeReceiptSingletonPost',
    'homeReceiptSurfaceValid',
    'homeReceiptPhase',
    'homeReceiptIndex',
    'rebaseUpdateResult'
)
$decisionExtractionOnlyLocals = @(
    'leaseRecordBase',
    'leaseFirstRecordBase',
    'preemptRecordBase',
    'probeRecordBase',
    'probeAxisIndex',
    'probeAxisBit',
    'preemptFlags',
    'cleanupRequiredMask',
    'cleanupCompleteMask',
    'preemptToken',
    'preemptGeneration',
    'preemptSession',
    'preemptSequence',
    'preemptMask',
    'preemptIdentitySize',
    'preemptState',
    'preemptOwnerKind',
    'preemptResourceKind',
    'preemptCommand',
    'preemptReference',
    'preemptAdmission',
    'replacementRecordBase',
    'replacementHeaderBase',
    'singletonToken',
    'singletonGeneration',
    'singletonMask',
    'replacementIdentitySize',
    'replacementTailSize',
    'replacementTailOffset',
    'replacementPackedCommand',
    'replacementPackedOwner',
    'cleanupReady',
    'leaseValid',
    'preemptBankValid',
    'preemptSpecialValid',
    'replacementFound',
    'replacementValid',
    'replacementStateValid',
    'preemptLmcFound',
    'preemptDs402Found',
    'preemptEncoderFound'
)
$homeHelperLocals = @(
    'axisIndex',
    'recordBase',
    'observerBase',
    'identityHeaderBase',
    'identitySize',
    'identityTailSize',
    'identityTailOffset',
    'identityPackedCommand',
    'identityPackedOwner',
    'homeReceiptAxisMaskValid'
) + $homeExtractionOnlyLocals
$decisionHelperLocals = @(
    'axisIndex',
    'axisBit',
    'observerBase',
    'identityIndex',
    'identityCompareResult',
    'forceQuarantine',
    'preemptRootValid',
    'restoreLease'
) + $decisionExtractionOnlyLocals

$methodBodyWithoutLocals = Replace-ExactOne `
    -Text $method `
    -Old $originalLocalVarBlock `
    -New '' `
    -Owner 'Publish local-only analysis'
$outsideExtractions = Replace-ExactOne `
    -Text $methodBodyWithoutLocals `
    -Old $homeExtraction `
    -New '' `
    -Owner 'Publish Home local-only analysis'
$outsideExtractions = Replace-ExactOne `
    -Text $outsideExtractions `
    -Old $decisionExtraction `
    -New '' `
    -Owner 'Publish decision local-only analysis'
foreach ($name in ($homeExtractionOnlyLocals + $decisionExtractionOnlyLocals)) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Extraction-only local '$name' is not declared."
    }
    $tokenPattern = '(?i)(?<![A-Za-z0-9_])' +
        [regex]::Escape($name) + '(?![A-Za-z0-9_])'
    if ([regex]::IsMatch($outsideExtractions, $tokenPattern)) {
        throw "Extraction-only local '$name' leaks outside both extraction blocks."
    }
}
foreach ($name in $homeHelperLocals) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Home helper local '$name' is not declared in the monolith."
    }
}
foreach ($name in $decisionHelperLocals) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Decision helper local '$name' is not declared in the monolith."
    }
}

$adapter = $method
foreach ($name in ($homeExtractionOnlyLocals + $decisionExtractionOnlyLocals)) {
    $adapter = Replace-ExactOne `
        -Text $adapter `
        -Old $localDeclarationByName[$name] `
        -New '' `
        -Owner "Publish adapter moved local $name"
}
$adapterLocalAnchor = "`t`tgroupHeaderPublished : BOOL;`n"
$adapterAddedLocals = [string]::Join("`n", @(
        "`t`thomeReceiptResult : DINT;",
        "`t`tpublishDecisionResult : DINT;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $adapterLocalAnchor `
    -New ($adapterLocalAnchor + $adapterAddedLocals) `
    -Owner 'Publish adapter result locals'

$homeCallMap = [string]::Join("`n", @(
        "`thomeReceiptResult := HandleAxisOwnershipPublishHomeReceipt(",
        "`t`tAxisMask:=AxisMask, AdmissionToken:=AdmissionToken,",
        "`t`tOwnerGeneration:=OwnerGeneration, ReportKind:=ReportKind,",
        "`t`tReportValue0:=ReportValue0, ReportValue1:=ReportValue1,",
        "`t`tObservationCycle:=ObservationCycle);",
        "`tif homeReceiptResult <> 2 then",
        "`t`tResult := homeReceiptResult;",
        "`t`tRETURN;",
        "`tend_if;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $homeExtraction `
    -New $homeCallMap `
    -Owner 'Publish adapter Home call/fence'

$decisionCallMap = [string]::Join("`n", @(
        "`tpublishDecisionResult := PrepareAxisOwnershipPublishDecision(",
        "`t`tAxisMask:=AxisMask, AdmissionToken:=AdmissionToken,",
        "`t`tOwnerGeneration:=OwnerGeneration, ReportKind:=ReportKind,",
        "`t`tExpectedSession:=expectedSession,",
        "`t`tExpectedSequence:=expectedSequence,",
        "`t`tExpectedCommandId:=expectedCommandId,",
        "`t`tExpectedReference:=expectedReference,",
        "`t`tExpectedAdmissionMode:=expectedAdmissionMode,",
        "`t`tExpectedOwnerKind:=expectedOwnerKind);",
        "`tif publishDecisionResult < 0 then",
        "`t`tResult := publishDecisionResult;",
        "`t`tRETURN;",
        "`telsif publishDecisionResult > 7 then",
        "`t`tResult := -3;",
        "`t`tRETURN;",
        "`tend_if;",
        "`tpreemptRootValid := (publishDecisionResult and 1) <> 0;",
        "`tforceQuarantine := (publishDecisionResult and 2) <> 0;",
        "`trestoreLease := (publishDecisionResult and 4) <> 0;"
    )) + "`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $decisionExtraction `
    -New $decisionCallMap `
    -Owner 'Publish adapter decision call/map'
$adapterLocalVarMatches = [regex]::Matches(
    $adapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
if ($adapterLocalVarMatches.Count -ne 1) {
    throw 'Publish planned adapter local VAR block count is not one.'
}
$adapterLocalVarBlock = $adapterLocalVarMatches[0].Value

$homeHelperPrefix = [string]::Join("`n", @(
        'FUNCTION LMCControlCommandService::HandleAxisOwnershipPublishHomeReceipt',
        "`tVAR_INPUT",
        "`t`tAxisMask : UDINT;",
        "`t`tAdmissionToken : UDINT;",
        "`t`tOwnerGeneration : UDINT;",
        "`t`tReportKind : UINT;",
        "`t`tReportValue0 : UDINT;",
        "`t`tReportValue1 : UDINT;",
        "`t`tObservationCycle : UDINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult : DINT;",
        "`tEND_VAR",
        "`tVAR"
    )) + "`n"
foreach ($name in $homeHelperLocals) {
    $homeHelperPrefix += $localDeclarationByName[$name]
}
$homeHelperPrefix += [string]::Join("`n", @(
        "`tEND_VAR",
        '',
        "`tResult := 2;"
    )) + "`n"
$homeHelperSuffix = "`nEND_FUNCTION`n"
$homeHelper = $homeHelperPrefix + $homeExtraction + $homeHelperSuffix

$decisionHelperPrefix = [string]::Join("`n", @(
        'FUNCTION LMCControlCommandService::PrepareAxisOwnershipPublishDecision',
        "`tVAR_INPUT",
        "`t`tAxisMask : UDINT;",
        "`t`tAdmissionToken : UDINT;",
        "`t`tOwnerGeneration : UDINT;",
        "`t`tReportKind : UINT;",
        "`t`tExpectedSession : UDINT;",
        "`t`tExpectedSequence : UDINT;",
        "`t`tExpectedCommandId : DINT;",
        "`t`tExpectedReference : DINT;",
        "`t`tExpectedAdmissionMode : DINT;",
        "`t`tExpectedOwnerKind : DINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult : DINT;",
        "`tEND_VAR",
        "`tVAR"
    )) + "`n"
foreach ($name in $decisionHelperLocals) {
    $decisionHelperPrefix += $localDeclarationByName[$name]
}
$decisionHelperPrefix += "`tEND_VAR`n`n"
$decisionHelperSuffix = [string]::Join("`n", @(
        "`tResult := 0;",
        "`tif preemptRootValid then",
        "`t`tResult += 1;",
        "`tend_if;",
        "`tif forceQuarantine then",
        "`t`tResult += 2;",
        "`tend_if;",
        "`tif restoreLease then",
        "`t`tResult += 4;",
        "`tend_if;",
        '',
        'END_FUNCTION'
    )) + "`n"
$decisionHelper = $decisionHelperPrefix + $decisionExtraction +
    $decisionHelperSuffix

$homeClassDeclaration = [string]::Join("`n", @(
        "`tFUNCTION HandleAxisOwnershipPublishHomeReceipt",
        "`t`tVAR_INPUT",
        "`t`t`tAxisMask : UDINT;",
        "`t`t`tAdmissionToken : UDINT;",
        "`t`t`tOwnerGeneration : UDINT;",
        "`t`t`tReportKind : UINT;",
        "`t`t`tReportValue0 : UDINT;",
        "`t`t`tReportValue1 : UDINT;",
        "`t`t`tObservationCycle : UDINT;",
        "`t`tEND_VAR",
        "`t`tVAR_OUTPUT",
        "`t`t`tResult : DINT;",
        "`t`tEND_VAR;"
    )) + "`n`t`n"
$decisionClassDeclaration = [string]::Join("`n", @(
        "`tFUNCTION PrepareAxisOwnershipPublishDecision",
        "`t`tVAR_INPUT",
        "`t`t`tAxisMask : UDINT;",
        "`t`t`tAdmissionToken : UDINT;",
        "`t`t`tOwnerGeneration : UDINT;",
        "`t`t`tReportKind : UINT;",
        "`t`t`tExpectedSession : UDINT;",
        "`t`t`tExpectedSequence : UDINT;",
        "`t`t`tExpectedCommandId : DINT;",
        "`t`t`tExpectedReference : DINT;",
        "`t`t`tExpectedAdmissionMode : DINT;",
        "`t`t`tExpectedOwnerKind : DINT;",
        "`t`tEND_VAR",
        "`t`tVAR_OUTPUT",
        "`t`t`tResult : DINT;",
        "`t`tEND_VAR;"
    )) + "`n`t`n"
$privateClassDeclarations = $homeClassDeclaration + $decisionClassDeclaration
if ($privateClassDeclarations -match '(?i)\b(?:GLOBAL|VIRTUAL)\b') {
    throw 'Publish helper class declarations are not private.'
}

$publicClassDeclarationMatches = [regex]::Matches(
    $source,
    ('(?ms)^\tFUNCTION GLOBAL PublishAxisOwnership\n' +
     '\t\tVAR_INPUT\n.*?^\t\tEND_VAR;\n'))
if ($publicClassDeclarationMatches.Count -ne 1) {
    throw 'Publish public class declaration count is not one.'
}
$publicClassDeclaration = $publicClassDeclarationMatches[0].Value

$implementationBundle = $adapter + "`n`n" + $homeHelper +
    "`n`n" + $decisionHelper
$plannedSource = Replace-ExactOne `
    -Text $source `
    -Old $publicClassDeclaration `
    -New ($privateClassDeclarations + $publicClassDeclaration) `
    -Owner 'Publish planned private declarations'
$plannedSource = Replace-ExactOne `
    -Text $plannedSource `
    -Old $method `
    -New $implementationBundle `
    -Owner 'Publish planned implementation bundle'

function New-PlannedSourceFromFragments {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHomeHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDecisionHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclarations
    )

    $candidateSource = Replace-ExactOne `
        -Text $source `
        -Old $publicClassDeclaration `
        -New ($CandidateDeclarations + $publicClassDeclaration) `
        -Owner 'Publish candidate private declarations'
    $candidateBundle = $CandidateAdapter + "`n`n" +
        $CandidateHomeHelper + "`n`n" + $CandidateDecisionHelper
    return Replace-ExactOne `
        -Text $candidateSource `
        -Old $method `
        -New $candidateBundle `
        -Owner 'Publish candidate implementation bundle'
}

function Get-ReversedAdapter {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $candidateLocalMatches = [regex]::Matches(
        $CandidateAdapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
    if ($candidateLocalMatches.Count -ne 1) {
        throw "$Owner adapter local VAR block count is not one."
    }
    $reversed = Replace-ExactOne `
        -Text $CandidateAdapter `
        -Old $candidateLocalMatches[0].Value `
        -New $originalLocalVarBlock `
        -Owner "$Owner reverse local movement"
    $reversed = Replace-ExactOne `
        -Text $reversed `
        -Old $homeCallMap `
        -New $homeExtraction `
        -Owner "$Owner reverse Home inline"
    return Replace-ExactOne `
        -Text $reversed `
        -Old $decisionCallMap `
        -New $decisionExtraction `
        -Owner "$Owner reverse decision inline"
}

function Assert-PublishSplitCandidate {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHomeHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDecisionHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclarations,
        [Parameter(Mandatory = $true)][string]$CandidateSource,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ($CandidateDeclarations -cne $privateClassDeclarations) {
        throw "$Owner private ABI/declaration order changed."
    }
    if (($CandidateDeclarations -match '(?i)\b(?:GLOBAL|VIRTUAL)\b') -or
        ($CandidateHomeHelper -match
            '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+') -or
        ($CandidateDecisionHelper -match
            '(?i)^FUNCTION\s+(?:GLOBAL|VIRTUAL\s+GLOBAL)\s+')) {
        throw "$Owner helper private visibility changed."
    }
    if (-not $CandidateAdapter.StartsWith(
            $publicInterface, [StringComparison]::Ordinal)) {
        throw "$Owner public ABI changed."
    }
    $candidateAdapterLocalMatches = [regex]::Matches(
        $CandidateAdapter, '(?ms)^\tVAR\n.*?^\tEND_VAR\n')
    if (($candidateAdapterLocalMatches.Count -ne 1) -or
        ($candidateAdapterLocalMatches[0].Value -cne $adapterLocalVarBlock)) {
        throw "$Owner extraction-only local movement changed."
    }
    if (-not $CandidateHomeHelper.StartsWith(
            $homeHelperPrefix, [StringComparison]::Ordinal)) {
        throw "$Owner Home helper ABI/local contract changed."
    }
    if (-not $CandidateDecisionHelper.StartsWith(
            $decisionHelperPrefix, [StringComparison]::Ordinal)) {
        throw "$Owner decision helper ABI/local contract changed."
    }

    $adapterDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateAdapter)
    $homeDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateHomeHelper)
    $decisionDimensions = Get-ByteDimensions -Text (
        Get-MethodContent -Method $CandidateDecisionHelper)
    foreach ($entry in @(
            [pscustomobject]@{ Name = 'adapter'; Value = $adapterDimensions },
            [pscustomobject]@{ Name = 'Home helper'; Value = $homeDimensions },
            [pscustomobject]@{ Name = 'decision helper'; Value = $decisionDimensions })) {
        if (($entry.Value.lf -ge $MethodSizeLimitBytes) -or
            ($entry.Value.crlf -ge $MethodSizeLimitBytes)) {
            throw "$Owner $($entry.Name) violates the method-size hard limit."
        }
    }

    if ([regex]::Matches(
            $CandidateAdapter,
            '(?i)(?<![A-Za-z0-9_])HandleAxisOwnershipPublishHomeReceipt\s*\(').Count -ne 1) {
        throw "$Owner Home helper call count/dominance changed."
    }
    if ([regex]::Matches(
            $CandidateAdapter,
            '(?i)(?<![A-Za-z0-9_])PrepareAxisOwnershipPublishDecision\s*\(').Count -ne 1) {
        throw "$Owner decision helper call count/dominance changed."
    }
    if (($CandidateAdapter.IndexOf(
                $homeCallMap, [StringComparison]::Ordinal) -lt 0) -or
        ($CandidateAdapter.IndexOf(
                $decisionCallMap, [StringComparison]::Ordinal) -lt 0)) {
        throw "$Owner exact adapter Result fence/bit map changed."
    }
    $homeBlockFirst = $CandidateHomeHelper.IndexOf(
        $homeExtraction, [StringComparison]::Ordinal)
    $homeBlockLast = $CandidateHomeHelper.LastIndexOf(
        $homeExtraction, [StringComparison]::Ordinal)
    if (($homeBlockFirst -lt 0) -or ($homeBlockFirst -ne $homeBlockLast) -or
        ((Get-TextSha256 -Text $homeExtractionContent) -cne
            $ExpectedHomeExtractionCanonicalLfSha256)) {
        throw "$Owner exact Home extraction changed."
    }
    $decisionBlockFirst = $CandidateDecisionHelper.IndexOf(
        $decisionExtraction, [StringComparison]::Ordinal)
    $decisionBlockLast = $CandidateDecisionHelper.LastIndexOf(
        $decisionExtraction, [StringComparison]::Ordinal)
    if (($decisionBlockFirst -lt 0) -or
        ($decisionBlockFirst -ne $decisionBlockLast) -or
        ((Get-TextSha256 -Text $decisionExtractionContent) -cne
            $ExpectedDecisionExtractionCanonicalLfSha256)) {
        throw "$Owner exact decision extraction changed."
    }
    if (($CandidateHomeHelper -cne $homeHelper) -or
        ($CandidateDecisionHelper -cne $decisionHelper)) {
        throw "$Owner extraction-only helper body changed."
    }

    $candidateSemanticText = $CandidateAdapter +
        $CandidateHomeHelper + $CandidateDecisionHelper
    $originalMutations = Get-PersistentMutationMatches -Text $method
    $candidateMutations = Get-PersistentMutationMatches -Text $candidateSemanticText
    $normalizedOriginalMutations = @(
        foreach ($match in $originalMutations) {
            [regex]::Replace(
                $match.Value, '\s+', '').ToLowerInvariant()
        })
    $normalizedCandidateMutations = @(
        foreach ($match in $candidateMutations) {
            [regex]::Replace(
                $match.Value, '\s+', '').ToLowerInvariant()
        })
    $originalMutationInventory = [string]::Join(
        '|', @($normalizedOriginalMutations | Sort-Object))
    $candidateMutationInventory = [string]::Join(
        '|', @($normalizedCandidateMutations | Sort-Object))
    if (($candidateMutations.Count -ne $originalMutations.Count) -or
        ($candidateMutationInventory -cne $originalMutationInventory)) {
        throw "$Owner persistent-write inventory changed."
    }

    $originalCalls = Get-CallHistogram -Text $method
    $candidateCalls = Get-CallHistogram -Text $candidateSemanticText
    foreach ($key in $originalCalls.Keys) {
        if ((-not $candidateCalls.Contains($key)) -or
            ($candidateCalls[$key] -ne $originalCalls[$key])) {
            throw "$Owner original call inventory changed at '$key'."
        }
    }
    $allowedExtraCalls = @(
        'handleaxisownershippublishhomereceipt',
        'prepareaxisownershippublishdecision'
    )
    foreach ($key in $candidateCalls.Keys) {
        if ($originalCalls.Contains($key)) {
            continue
        }
        if (($allowedExtraCalls -notcontains $key) -or
            ($candidateCalls[$key] -ne 1)) {
            throw "$Owner added unexpected call '$key'."
        }
    }
    foreach ($key in $allowedExtraCalls) {
        if ((-not $candidateCalls.Contains($key)) -or
            ($candidateCalls[$key] -ne 1)) {
            throw "$Owner helper call inventory changed at '$key'."
        }
    }

    $decisionCallIndex = $CandidateAdapter.IndexOf(
        $decisionCallMap, [StringComparison]::Ordinal)
    $postDecisionText = $CandidateAdapter.Substring(
        $decisionCallIndex + $decisionCallMap.Length)
    $firstPostDecisionMutation = Get-PersistentMutationMatches -Text $postDecisionText |
        Select-Object -First 1
    if ($null -eq $firstPostDecisionMutation) {
        throw "$Owner lost all post-decision persistent commits."
    }
    if (($CandidateHomeHelper -notmatch '(?m)^\tResult := 2;$') -or
        ([regex]::Matches(
            $CandidateHomeHelper, '(?m)^\tResult := 2;$').Count -ne 1)) {
        throw "$Owner Home Result=2 containment changed."
    }
    if (($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 1;$') -or
        ($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 2;$') -or
        ($CandidateDecisionHelper -notmatch '(?m)^\t\tResult \+= 4;$')) {
        throw "$Owner decision result bit domain changed."
    }

    $reversedAdapter = Get-ReversedAdapter `
        -CandidateAdapter $CandidateAdapter `
        -Owner $Owner
    if ($reversedAdapter -cne $method) {
        throw "$Owner adapter extraction-only reverse-inline changed the monolith."
    }

    $expectedCandidateSource = New-PlannedSourceFromFragments `
        -CandidateAdapter $CandidateAdapter `
        -CandidateHomeHelper $CandidateHomeHelper `
        -CandidateDecisionHelper $CandidateDecisionHelper `
        -CandidateDeclarations $CandidateDeclarations
    if ($CandidateSource -cne $expectedCandidateSource) {
        throw "$Owner candidate source/fragments diverged."
    }
    $candidateBundle = $CandidateAdapter + "`n`n" +
        $CandidateHomeHelper + "`n`n" + $CandidateDecisionHelper
    $reverseSource = Replace-ExactOne `
        -Text $CandidateSource `
        -Old $candidateBundle `
        -New $reversedAdapter `
        -Owner "$Owner reverse implementation bundle"
    $reverseSource = Replace-ExactOne `
        -Text $reverseSource `
        -Old $CandidateDeclarations `
        -New '' `
        -Owner "$Owner reverse private declarations"
    if (($reverseSource -cne $source) -or
        ((Get-TextSha256 -Text $reverseSource) -cne
            $ExpectedSourceCanonicalLfSha256) -or
        ((Get-TextSha256 -Text (
            ConvertTo-IdeCrlf -Text $reverseSource)) -cne
            $ExpectedSourceIdeCrlfSha256)) {
        throw "$Owner whole-source reverse-inline did not restore A51E."
    }
}

Assert-PublishSplitCandidate `
    -CandidateAdapter $adapter `
    -CandidateHomeHelper $homeHelper `
    -CandidateDecisionHelper $decisionHelper `
    -CandidateDeclarations $privateClassDeclarations `
    -CandidateSource $plannedSource `
    -Owner 'Publish planned positive candidate'

function Invoke-PublishSplitPlannerSelfTest {
    $lfCheckpoint = Assert-SourceRatchet `
        -InputText $sourceCheckpoint.CanonicalLf `
        -Owner 'Publish LF positive fixture'
    $crlfCheckpoint = Assert-SourceRatchet `
        -InputText $sourceCheckpoint.IdeCrlf `
        -Owner 'Publish CRLF positive fixture'
    if (($lfCheckpoint.LineEnding -cne 'LF') -or
        ($crlfCheckpoint.LineEnding -cne 'CRLF') -or
        ($lfCheckpoint.CanonicalLf -cne $crlfCheckpoint.CanonicalLf)) {
        throw 'Publish LF/CRLF positive fixtures did not converge.'
    }

    $fixtures = @()
    $fixtures += [pscustomobject]@{
        Name = 'PrivateDeclarationGlobal'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations.Replace(
            "`tFUNCTION HandleAxisOwnershipPublishHomeReceipt",
            "`tFUNCTION GLOBAL HandleAxisOwnershipPublishHomeReceipt")
        Expected = 'private ABI/declaration'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PrivateInputOrderSwapped'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = (Replace-ExactOne `
            -Text $homeClassDeclaration `
            -Old ("`t`t`tAxisMask : UDINT;`n" +
                "`t`t`tAdmissionToken : UDINT;`n") `
            -New ("`t`t`tAdmissionToken : UDINT;`n" +
                "`t`t`tAxisMask : UDINT;`n") `
            -Owner 'PrivateInputOrderSwapped mutation') +
            $decisionClassDeclaration
        Expected = 'private ABI/declaration'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PublicAbiChanged'
        Adapter = $adapter.Replace(
            "`t`tReportKind : UINT;", "`t`tReportKind : UDINT;")
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'public ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeHelperGlobal'
        Adapter = $adapter
        Home = $homeHelper.Replace(
            'FUNCTION LMCControlCommandService::HandleAxisOwnershipPublishHomeReceipt',
            'FUNCTION GLOBAL LMCControlCommandService::HandleAxisOwnershipPublishHomeReceipt')
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'helper private visibility|Home helper ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeResultSeedChanged'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old "`tResult := 2;`n" `
            -New "`tResult := 0;`n" `
            -Owner 'HomeResultSeedChanged mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'Home helper ABI/local|extraction-only helper|Home Result=2'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeReceiptMagicChanged'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old 'LMC_HOME_OWNER_RECEIPT_MAGIC then' `
            -New 'LMC_HOME_RECORD_MAGIC then' `
            -Owner 'HomeReceiptMagicChanged mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact Home extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeUpdateCallDropped'
        Adapter = $adapter
        Home = Replace-ExactOne `
            -Text $homeHelper `
            -Old ("`t`t`trebaseUpdateResult := UpdateAxisRebaseRequiredState(`n" +
                "`t`t`t`tSetAxisMask:=0, ClearAxisMask:=AxisMask);`n") `
            -New "`t`t`trebaseUpdateResult := 0;`n" `
            -Owner 'HomeUpdateCallDropped mutation'
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact Home extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'HomeAdapterFenceInverted'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old 'if homeReceiptResult <> 2 then' `
            -New 'if homeReceiptResult = 2 then' `
            -Owner 'HomeAdapterFenceInverted mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionHelperGlobal'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper.Replace(
            'FUNCTION LMCControlCommandService::PrepareAxisOwnershipPublishDecision',
            'FUNCTION GLOBAL LMCControlCommandService::PrepareAxisOwnershipPublishDecision')
        Declarations = $privateClassDeclarations
        Expected = 'helper private visibility|decision helper ABI'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionPreemptFlagMaskChanged'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old '0xFFFE0000) = 0);' `
            -New '0xFFFF0000) = 0);' `
            -Owner 'DecisionPreemptFlagMaskChanged mutation'
        Declarations = $privateClassDeclarations
        Expected = 'exact decision extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionResultBitDropped'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old "`t`tResult += 4;`n" `
            -New "`t`tResult += 0;`n" `
            -Owner 'DecisionResultBitDropped mutation'
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only helper|decision result bit'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionAdapterBitSwap'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old ("`tforceQuarantine := (publishDecisionResult and 2) <> 0;`n" +
                "`trestoreLease := (publishDecisionResult and 4) <> 0;`n") `
            -New ("`tforceQuarantine := (publishDecisionResult and 4) <> 0;`n" +
                "`trestoreLease := (publishDecisionResult and 2) <> 0;`n") `
            -Owner 'DecisionAdapterBitSwap mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'DecisionNegativeGateRemoved'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old 'if publishDecisionResult < 0 then' `
            -New 'if publishDecisionResult = -999 then' `
            -Owner 'DecisionNegativeGateRemoved mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'ExpectedSessionResampled'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old "`t`tExpectedSession:=expectedSession,`n" `
            -New ("`t`tExpectedSession:=" +
                "OwnershipState[firstRecordBase + 6]`$UDINT,`n") `
            -Owner 'ExpectedSessionResampled mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'exact adapter Result fence'
    }
    $fixtures += [pscustomobject]@{
        Name = 'PersistentWriteAdded'
        Adapter = $adapter
        Home = $homeHelper
        Decision = Replace-ExactOne `
            -Text $decisionHelper `
            -Old "`tResult := 0;`n" `
            -New ("`tOwnershipState[0] := 1;`n" + "`tResult := 0;`n") `
            -Owner 'PersistentWriteAdded mutation'
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only helper|persistent-write'
    }
    $fixtures += [pscustomobject]@{
        Name = 'OriginalCallChanged'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper.Replace('_memcmp(', '_memcpy(')
        Declarations = $privateClassDeclarations
        Expected = 'exact decision extraction'
    }
    $fixtures += [pscustomobject]@{
        Name = 'ExtractionOnlyLocalRetained'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old $adapterAddedLocals `
            -New ($localDeclarationByName['preemptToken'] + $adapterAddedLocals) `
            -Owner 'ExtractionOnlyLocalRetained mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'extraction-only local movement'
    }
    $fixtures += [pscustomobject]@{
        Name = 'AdapterOversized'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old "`nEND_FUNCTION`n" `
            -New ("`n// " + ('X' * 8000) + "`nEND_FUNCTION`n") `
            -Owner 'AdapterOversized mutation'
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'method-size hard limit'
    }
    $fixtures += [pscustomobject]@{
        Name = 'CandidateSourceDrift'
        Adapter = $adapter
        Home = $homeHelper
        Decision = $decisionHelper
        Declarations = $privateClassDeclarations
        Expected = 'candidate source/fragments diverged'
        SourceOverride = $plannedSource + "// drift`n"
    }

    $expectedFixtureCount = 19
    if ($fixtures.Count -ne $expectedFixtureCount) {
        throw (
            "Publish negative fixture count is $($fixtures.Count), " +
            "expected $expectedFixtureCount.")
    }
    $rejected = 0
    foreach ($fixture in $fixtures) {
        $candidateSource = if (
            $fixture.PSObject.Properties.Name -contains 'SourceOverride') {
            $fixture.SourceOverride
        }
        else {
            New-PlannedSourceFromFragments `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHomeHelper $fixture.Home `
                -CandidateDecisionHelper $fixture.Decision `
                -CandidateDeclarations $fixture.Declarations
        }
        $didReject = $false
        try {
            Assert-PublishSplitCandidate `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHomeHelper $fixture.Home `
                -CandidateDecisionHelper $fixture.Decision `
                -CandidateDeclarations $fixture.Declarations `
                -CandidateSource $candidateSource `
                -Owner "Publish fixture $($fixture.Name)"
        }
        catch {
            $didReject = $true
            if ($_.Exception.Message -notmatch $fixture.Expected) {
                throw (
                    "Publish fixture '$($fixture.Name)' rejected by the " +
                    "wrong contract: $($_.Exception.Message)")
            }
        }
        if (-not $didReject) {
            throw "Publish negative fixture '$($fixture.Name)' was accepted."
        }
        $rejected++
    }
    return $rejected
}

if ($RunSelfTest) {
    $negativeCount = Invoke-PublishSplitPlannerSelfTest
    Write-Host (
        'PASS LASAL.AxisOwnershipPublishSplitPlan.SelfTest (' +
        "$negativeCount/$negativeCount negative fixtures rejected; " +
        'LF/CRLF positive fixtures and exact candidate accepted)')
    return
}

$adapterContent = Get-MethodContent -Method $adapter
$homeHelperContent = Get-MethodContent -Method $homeHelper
$decisionHelperContent = Get-MethodContent -Method $decisionHelper
$originalMutations = Get-PersistentMutationMatches -Text $method
$originalMutationInventory = Get-NormalizedInventory -Matches $originalMutations
$originalCalls = Get-CallHistogram -Text $method
$plannedCalls = Get-CallHistogram -Text (
    $adapter + $homeHelper + $decisionHelper)
$reverseAdapter = Get-ReversedAdapter `
    -CandidateAdapter $adapter `
    -Owner 'Publish result reverse'

$result = [ordered]@{
    status = 'PASS'
    mode = 'standalone in-memory planning only; no LASAL source/generated write'
    source = [ordered]@{
        path = $sourcePath
        inputLineEnding = $sourceCheckpoint.LineEnding
        inputPhysicalBytes = $sourceCheckpoint.PhysicalBytes
        inputPhysicalSha256 = $sourceCheckpoint.PhysicalSha256
        canonicalLfBytes = $Utf8.GetByteCount($source)
        canonicalLfSha256 = Get-TextSha256 -Text $source
        ideCrlfBytes = $Utf8.GetByteCount($sourceCheckpoint.IdeCrlf)
        ideCrlfSha256 = Get-TextSha256 -Text $sourceCheckpoint.IdeCrlf
    }
    originalMethod = [ordered]@{
        dimensionsWithTerminalEol = Get-ByteDimensions -Text $method
        canonicalLfSha256 = $methodSha256
        localDeclarations = $localDeclarationByName.Count
        persistentMutationEvents = $originalMutations.Count
        persistentMutationInventorySha256 = Get-TextSha256 -Text (
            $originalMutationInventory)
        callHistogram = Get-HistogramInventory -Histogram $originalCalls
        resultAssignments = [regex]::Matches(
            $method, '(?i)\bResult\s*:=').Count
        returns = [regex]::Matches($method, '(?i)\bRETURN\s*;').Count
    }
    extraction = [ordered]@{
        home = [ordered]@{
            dimensions = Get-ByteDimensions -Text $homeExtractionContent
            canonicalLfSha256 = $homeExtractionLfSha256
            ideCrlfSha256 = $homeExtractionCrlfSha256
            extractionOnlyLocals = $homeExtractionOnlyLocals.Count
        }
        decision = [ordered]@{
            dimensions = Get-ByteDimensions -Text $decisionExtractionContent
            canonicalLfSha256 = $decisionExtractionLfSha256
            ideCrlfSha256 = $decisionExtractionCrlfSha256
            extractionOnlyLocals = $decisionExtractionOnlyLocals.Count
        }
    }
    candidate = [ordered]@{
        adapter = [ordered]@{
            dimensions = Get-ByteDimensions -Text $adapterContent
            canonicalLfSha256 = Get-TextSha256 -Text $adapterContent
        }
        homeHelper = [ordered]@{
            private = $true
            dimensions = Get-ByteDimensions -Text $homeHelperContent
            canonicalLfSha256 = Get-TextSha256 -Text $homeHelperContent
        }
        decisionHelper = [ordered]@{
            private = $true
            dimensions = Get-ByteDimensions -Text $decisionHelperContent
            canonicalLfSha256 = Get-TextSha256 -Text $decisionHelperContent
            successDomain = '0..7; bits 0/1/2 map preemptRootValid/forceQuarantine/restoreLease'
        }
        wholeSource = [ordered]@{
            canonicalLfBytes = $Utf8.GetByteCount($plannedSource)
            canonicalLfSha256 = Get-TextSha256 -Text $plannedSource
            ideCrlfBytes = $Utf8.GetByteCount(
                (ConvertTo-IdeCrlf -Text $plannedSource))
            ideCrlfSha256 = Get-TextSha256 -Text (
                ConvertTo-IdeCrlf -Text $plannedSource)
        }
        callHistogram = Get-HistogramInventory -Histogram $plannedCalls
    }
    preservation = [ordered]@{
        publicAbiExact = $adapter.StartsWith(
            $publicInterface, [StringComparison]::Ordinal)
        privateHelpersHaveNoGlobal =
            ($privateClassDeclarations -notmatch '(?i)\b(?:GLOBAL|VIRTUAL)\b')
        homeResult2Contained = $true
        decisionBitDomainChecked = $true
        extractionOnlyLocalMovement = '16 Home + 41 decision locals'
        persistentMutationInventoryExact = $true
        originalCallInventoryExact = $true
        reverseInlineRestoredCanonicalLfExact = ($reverseAdapter -ceq $method)
        reverseInlineRestoredSourceCanonicalLfSha256 =
            $ExpectedSourceCanonicalLfSha256
        reverseInlineRestoredSourceIdeCrlfSha256 =
            $ExpectedSourceIdeCrlfSha256
    }
    expectedPostSplitMethodInventory = [ordered]@{
        classes = 6
        methods = 98
        underLimit = 95
        baselineDebt = 3
    }
}

$result | ConvertTo-Json -Depth 9

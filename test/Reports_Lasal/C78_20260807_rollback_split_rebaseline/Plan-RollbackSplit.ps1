param(
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..'),
    [switch]$RunSelfTest,
    [ValidateSet('AdapterLocals', 'AdapterExtraction', 'Helper')]
    [string]$EmitApplyPatchPart
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ExpectedSourceCanonicalLfSha256 =
    '634D8C0526C572C5A84707B9290B56C5E01B8EF73827EB502040FAF36A113558'
$ExpectedSourceIdeCrlfSha256 =
    'DAA8E134CE6E67BA47D6B30530F0FB9DBEF041A1B355466472872975897C3DF0'
$ExpectedPostSplitSourceCanonicalLfSha256 =
    '7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE'
$ExpectedPostSplitSourceIdeCrlfSha256 =
    'A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753'
$ExpectedMethodCanonicalLfSha256 =
    '9CCEBDC715D0171A37E581517E4C1FEF7EAFCADCADE5613BD2C446143954A9D0'
$ExpectedMethodIdeCrlfSliceSha256 =
    '2A88838417913B76449739447AAA8175157EAF8A370CC53F7FF916A3F25FF745'
$ExpectedExtractionCanonicalLfSha256 =
    'E17AA7406C75404BACF4C92BA3CE4ECB997B1856851B155DB5F47663DB2B4417'
$ExpectedExtractionIdeCrlfSha256 =
    '9A6EFE09CBE17D062802245E06974BF80AA7268D95489DEB8C137A0E1F68A62C'
$MethodSizeLimitBytes = 32768
$Utf8 = [System.Text.UTF8Encoding]::new($false, $true)

function Get-TextSha256 {
    param([Parameter(Mandatory = $true)][string]$Text)

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($Text)))
}

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

function Get-ByteDimensions {
    param([Parameter(Mandatory = $true)][string]$Text)

    $lfText = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    $crlfText = $lfText.Replace("`n", "`r`n")
    return [ordered]@{
        raw = $Utf8.GetByteCount($Text)
        lf = $Utf8.GetByteCount($lfText)
        crlf = $Utf8.GetByteCount($crlfText)
    }
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

function Write-ApplyPatchBody {
    param(
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)][string]$New
    )

    function Get-PatchLines {
        param([Parameter(Mandatory = $true)][string]$Text)

        $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
        if ($normalized.EndsWith("`n", [StringComparison]::Ordinal)) {
            $normalized = $normalized.Substring(0, $normalized.Length - 1)
        }
        return $normalized.Split(
            @("`n"),
            [StringSplitOptions]::None)
    }

    Write-Output 'PATCH_BODY_BEGIN'
    Write-Output '@@'
    foreach ($line in (Get-PatchLines -Text $Old)) {
        Write-Output ('-' + $line)
    }
    foreach ($line in (Get-PatchLines -Text $New)) {
        Write-Output ('+' + $line)
    }
    Write-Output 'PATCH_BODY_END'
}

function Get-CanonicalMethodSlice {
    param([Parameter(Mandatory = $true)][string]$PhysicalMethod)

    if (-not $PhysicalMethod.EndsWith("`r`n", [StringComparison]::Ordinal)) {
        throw 'Physical method does not end with CRLF.'
    }
    # Match Verify-LasalCustomMethodSizeBudget.ps1: keep the final CR and
    # exclude the LF after END_FUNCTION.
    return $PhysicalMethod.Substring(0, $PhysicalMethod.Length - 1)
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$sourcePath = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$sourceInputPhysical = [IO.File]::ReadAllText($sourcePath, $Utf8)
$sourceInputCanonicalLf = ConvertTo-CanonicalLf -Text $sourceInputPhysical
$sourceInputIdeCrlf = ConvertTo-IdeCrlf -Text $sourceInputCanonicalLf
$sourceInputLineEnding = if ($sourceInputPhysical -ceq $sourceInputCanonicalLf) {
    'LF'
}
elseif ($sourceInputPhysical -ceq $sourceInputIdeCrlf) {
    'CRLF'
}
else {
    throw 'Control source uses mixed or unsupported line endings.'
}
$sourceInputPhysicalSha256 = Get-TextSha256 -Text $sourceInputPhysical
$sourceInputCanonicalLfSha256 = Get-TextSha256 -Text $sourceInputCanonicalLf
$sourceInputIdeCrlfSha256 = Get-TextSha256 -Text $sourceInputIdeCrlf
$sourceInputMode = 'pre-split planning baseline'
$postSplitSourceCanonicalLf = $null

if ($sourceInputCanonicalLfSha256 -ceq $ExpectedSourceCanonicalLfSha256) {
    if ($sourceInputIdeCrlfSha256 -cne $ExpectedSourceIdeCrlfSha256) {
        throw 'Control source canonical LF/IDE CRLF checkpoint pair drifted.'
    }
    $sourceCanonicalLf = $sourceInputCanonicalLf
}
elseif ($RunSelfTest -and
        ($sourceInputCanonicalLfSha256 -ceq
            $ExpectedPostSplitSourceCanonicalLfSha256)) {
    if ($sourceInputIdeCrlfSha256 -cne
            $ExpectedPostSplitSourceIdeCrlfSha256) {
        throw 'Post-split source canonical LF/IDE CRLF checkpoint pair drifted.'
    }
    $sourceInputMode = 'post-split exact candidate self-test'
    $postSplitSourceCanonicalLf = $sourceInputCanonicalLf

    $postAdapterPattern = (
        '(?ms)^FUNCTION GLOBAL ' +
        'LMCControlCommandService::RollbackAxisOwnership\n' +
        '.*?^END_FUNCTION\n')
    $postHelperPattern = (
        '(?ms)^FUNCTION ' +
        'LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank\n' +
        '.*?^END_FUNCTION\n')
    $postAdapterMatches = [regex]::Matches(
        $sourceInputCanonicalLf, $postAdapterPattern)
    $postHelperMatches = [regex]::Matches(
        $sourceInputCanonicalLf, $postHelperPattern)
    if (($postAdapterMatches.Count -ne 1) -or
        ($postHelperMatches.Count -ne 1)) {
        throw 'Post-split source does not contain one exact adapter/helper pair.'
    }

    $monolithEvidencePath = Join-Path $root (
        'test\Reports_Lasal\C78_20260806_preemption_cleanup_split\' +
        'pre_ide_LMCControlCommandService.st')
    if (-not (Test-Path -LiteralPath $monolithEvidencePath -PathType Leaf)) {
        throw "Rollback monolith evidence is missing: $monolithEvidencePath"
    }
    $monolithEvidencePhysical =
        [IO.File]::ReadAllText($monolithEvidencePath, $Utf8)
    $monolithEvidenceCanonicalLf =
        ConvertTo-CanonicalLf -Text $monolithEvidencePhysical
    $monolithMatches = [regex]::Matches(
        $monolithEvidenceCanonicalLf,
        ('(?ms)^FUNCTION GLOBAL ' +
         'LMCControlCommandService::RollbackAxisOwnership\n' +
         '.*?^END_FUNCTION\n'))
    if (($monolithMatches.Count -ne 1) -or
        ((Get-TextSha256 -Text $monolithMatches[0].Value) -cne
            $ExpectedMethodCanonicalLfSha256)) {
        throw 'Rollback monolith evidence method canonical LF checkpoint drifted.'
    }
    $helperStubCanonicalLf = [string]::Join("`n", @(
            'FUNCTION LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank',
            "`tVAR_INPUT",
            "`t`tExpectedAxisMask `t: UDINT;",
            "`t`tpRestoreContext `t: ^void;",
            "`t`tRestoreContextSize `t: UDINT;",
            "`tEND_VAR",
            "`tVAR_OUTPUT",
            "`t`tResult `t: DINT;",
            "`tEND_VAR",
            '',
            'END_FUNCTION'
        )) + "`n"
    $sourceCanonicalLf = Replace-ExactOne `
        -Text $sourceInputCanonicalLf `
        -Old $postAdapterMatches[0].Value `
        -New $monolithMatches[0].Value `
        -Owner 'Rollback post-split reverse adapter'
    $sourceCanonicalLf = Replace-ExactOne `
        -Text $sourceCanonicalLf `
        -Old $postHelperMatches[0].Value `
        -New $helperStubCanonicalLf `
        -Owner 'Rollback post-split reverse helper'
    if (((Get-TextSha256 -Text $sourceCanonicalLf) -cne
            $ExpectedSourceCanonicalLfSha256) -or
        ((Get-TextSha256 -Text (ConvertTo-IdeCrlf -Text $sourceCanonicalLf)) -cne
            $ExpectedSourceIdeCrlfSha256)) {
        throw 'Post-split reverse-inline did not restore the DAA8 LF/CRLF pair.'
    }
}
else {
    throw (
        "Control source canonical LF SHA-256 is $sourceInputCanonicalLfSha256, " +
        "expected $ExpectedSourceCanonicalLfSha256" +
        $(if ($RunSelfTest) {
                " or post-split $ExpectedPostSplitSourceCanonicalLfSha256."
            }
            else {
                '.'
            }))
}

# Planning is performed against a deterministic LASAL IDE CRLF projection.
# Repository identity and fresh-checkout acceptance are ratcheted above in LF.
$source = ConvertTo-IdeCrlf -Text $sourceCanonicalLf
$sourceSha256 = Get-TextSha256 -Text $source
if ($sourceSha256 -cne $ExpectedSourceIdeCrlfSha256) {
    throw (
        "Control IDE CRLF SHA-256 is $sourceSha256, expected " +
        "$ExpectedSourceIdeCrlfSha256.")
}

$methodPattern = (
    '(?ms)^FUNCTION GLOBAL ' +
    'LMCControlCommandService::RollbackAxisOwnership\r\n' +
    '.*?^END_FUNCTION\r\n')
$methodMatches = [regex]::Matches($source, $methodPattern)
if ($methodMatches.Count -ne 1) {
    throw "Rollback physical method count is $($methodMatches.Count), expected one."
}
$methodPhysical = $methodMatches[0].Value
$methodSlice = Get-CanonicalMethodSlice -PhysicalMethod $methodPhysical
$methodSha256 = Get-TextSha256 -Text $methodSlice
$methodCanonicalLf = ConvertTo-CanonicalLf -Text $methodPhysical
$methodCanonicalLfSha256 = Get-TextSha256 -Text $methodCanonicalLf
if (($methodCanonicalLfSha256 -cne $ExpectedMethodCanonicalLfSha256) -or
    ($methodSha256 -cne $ExpectedMethodIdeCrlfSliceSha256)) {
    throw (
        'Rollback method canonical LF or IDE CRLF slice checkpoint drifted ' +
        "($methodCanonicalLfSha256/$methodSha256).")
}
$localVarPattern = (
    '(?ms)^\tVAR\r\n\t\taxisIndex\s*:\s*DINT;\r\n' +
    '.*?^\tEND_VAR\r\n')
$originalLocalVarMatches = [regex]::Matches($methodPhysical, $localVarPattern)
if ($originalLocalVarMatches.Count -ne 1) {
    throw 'Rollback original local VAR block count is not one.'
}
$originalLocalVarBlock = $originalLocalVarMatches[0].Value

$extractionPattern = (
    '(?ms)(^\t\t\tpreemptBankValid := TRUE;\r\n.*?' +
    '^\t\t\tif preemptBankValid = FALSE then\r\n' +
    '\t\t\t\tResult := -3;\r\n' +
    '\t\t\t\tRETURN;\r\n' +
    '\t\t\tend_if;\r\n)(?=\t\tend_if;)')
$extractionMatches = [regex]::Matches($methodPhysical, $extractionPattern)
if ($extractionMatches.Count -ne 2) {
    throw (
        "Rollback preemptBankValid extraction candidate count is " +
        "$($extractionMatches.Count), expected two including the earlier empty-bank check.")
}
$extraction = $extractionMatches[1].Groups[1].Value
$extractionSha256 = Get-TextSha256 -Text $extraction
$extractionCanonicalLf = ConvertTo-CanonicalLf -Text $extraction
$extractionCanonicalLfSha256 = Get-TextSha256 -Text $extractionCanonicalLf
if (($extractionCanonicalLfSha256 -cne
        $ExpectedExtractionCanonicalLfSha256) -or
    ($extractionSha256 -cne $ExpectedExtractionIdeCrlfSha256)) {
    throw (
        'Rollback validation extraction canonical LF or IDE CRLF ' +
        "checkpoint drifted ($extractionCanonicalLfSha256/$extractionSha256).")
}

$extractionOnlyLocals = @(
    'probeAxisIndex',
    'probeAxisBit',
    'probeRecordBase',
    'observerIndex',
    'snapshotToken',
    'snapshotGeneration',
    'snapshotSession',
    'snapshotSequence',
    'snapshotMask',
    'snapshotCommand',
    'snapshotReference',
    'snapshotOwnerKind',
    'snapshotResourceKind',
    'snapshotAdmissionMode',
    'snapshotState',
    'snapshotIdentitySize',
    'restoredGroupState',
    'identityExpectedSize',
    'identityPrefixSize',
    'identityShapeValid',
    'snapshotTupleValid',
    'observerSnapshotValid',
    'observersIdle'
)

$helperBlockLocals = @(
    'axisIndex',
    'axisBit',
    'probeAxisIndex',
    'probeAxisBit',
    'recordBase',
    'probeRecordBase',
    'observerBase',
    'observerIndex',
    'snapshotToken',
    'snapshotGeneration',
    'snapshotSession',
    'snapshotSequence',
    'snapshotMask',
    'snapshotCommand',
    'snapshotReference',
    'snapshotOwnerKind',
    'snapshotResourceKind',
    'snapshotAdmissionMode',
    'snapshotState',
    'snapshotIdentitySize',
    'restoredGroupMask',
    'restoredGroupToken',
    'restoredGroupGeneration',
    'restoredGroupSession',
    'restoredGroupSequence',
    'restoredGroupIdentitySize',
    'restoredGroupCommand',
    'restoredGroupReference',
    'restoredGroupState',
    'restoredGroupAdmissionMode',
    'identityHeaderBase',
    'identityCompareResult',
    'identityExpectedSize',
    'identityPrefixSize',
    'identityTailSize',
    'identityTailOffset',
    'preemptBankValid',
    'identityShapeValid',
    'snapshotTupleValid',
    'observerSnapshotValid',
    'observersIdle',
    'restoredGroupActive',
    'restoredGroupFound',
    'restoredNonSafetyFound'
)

$localDeclarationMatches = [regex]::Matches(
    $methodPhysical,
    '(?m)^\t\t(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:(?!=)[^\r\n]+\r$')
$localDeclarationByName = @{}
foreach ($localMatch in $localDeclarationMatches) {
    $name = $localMatch.Groups['Name'].Value
    if ($localDeclarationByName.ContainsKey($name)) {
        throw "Rollback local declaration '$name' is duplicated."
    }
    $localDeclarationByName[$name] = $localMatch.Value + "`n"
}
foreach ($name in $helperBlockLocals) {
    if (-not $localDeclarationByName.ContainsKey($name)) {
        throw "Rollback helper local source declaration '$name' is missing."
    }
}

$adapter = $methodPhysical
foreach ($name in $extractionOnlyLocals) {
    $adapter = Replace-ExactOne `
        -Text $adapter `
        -Old $localDeclarationByName[$name] `
        -New '' `
        -Owner "Rollback adapter local $name"
}
$adapterLocalAnchor = "`t`tgroupHeaderPublished : BOOL;`r`n"
$adapterLocalReplacement = $adapterLocalAnchor +
    "`t`trollbackPreemptResult : DINT;`r`n" +
    "`t`trestoreContext : ARRAY [0..9] OF UDINT;`r`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $adapterLocalAnchor `
    -New $adapterLocalReplacement `
    -Owner 'Rollback adapter split locals'

$callMapLines = @(
    "`t`t`trollbackPreemptResult := ValidateAxisOwnershipRollbackPreemptBank(",
    "`t`t`t`tExpectedAxisMask:=expectedAxisMask,",
    "`t`t`t`tpRestoreContext:=(#restoreContext[0])`$^void,",
    "`t`t`t`tRestoreContextSize:=40);",
    "`t`t`tif rollbackPreemptResult <> 0 then",
    "`t`t`t`tResult := -3;",
    "`t`t`t`tRETURN;",
    "`t`t`tend_if;",
    "`t`t`trestoredGroupActive := restoreContext[0] <> 0;",
    "`t`t`trestoredGroupMask := restoreContext[1];",
    "`t`t`trestoredGroupToken := restoreContext[2];",
    "`t`t`trestoredGroupGeneration := restoreContext[3];",
    "`t`t`trestoredGroupSession := restoreContext[4];",
    "`t`t`trestoredGroupSequence := restoreContext[5];",
    "`t`t`trestoredGroupIdentitySize := restoreContext[6];",
    "`t`t`trestoredGroupCommand := restoreContext[7]`$DINT;",
    "`t`t`trestoredGroupReference := restoreContext[8]`$DINT;",
    "`t`t`trestoredGroupAdmissionMode := restoreContext[9]`$DINT;"
)
$callMap = [string]::Join("`r`n", $callMapLines) + "`r`n"
$adapter = Replace-ExactOne `
    -Text $adapter `
    -Old $extraction `
    -New $callMap `
    -Owner 'Rollback adapter validation call/map'
$adapterLocalVarMatches = [regex]::Matches($adapter, $localVarPattern)
if ($adapterLocalVarMatches.Count -ne 1) {
    throw 'Rollback planned adapter local VAR block count is not one.'
}
$adapterLocalVarBlock = $adapterLocalVarMatches[0].Value

$deindentedExtraction = [regex]::Replace($extraction, '(?m)^\t\t', '')
$helperLocalText = ''
foreach ($name in $helperBlockLocals) {
    $helperLocalText += $localDeclarationByName[$name]
}
$helperPrefix = [string]::Join("`r`n", @(
        'FUNCTION LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank',
        "`tVAR_INPUT",
        "`t`tExpectedAxisMask `t: UDINT;",
        "`t`tpRestoreContext `t: ^void;",
        "`t`tRestoreContextSize `t: UDINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult `t: DINT;",
        "`tEND_VAR",
        "`tVAR",
        "`t`tpContext : ^USINT;",
        "`t`tcontextActive : UDINT;"
    )) + "`r`n" + $helperLocalText + [string]::Join("`r`n", @(
        "`tEND_VAR",
        '',
        "`tResult := -1;",
        "`tif (pRestoreContext = NIL) | (RestoreContextSize <> 40) |",
        "`t   (ExpectedAxisMask = 0) |",
        "`t   (ExpectedAxisMask > LMC_OWNER_ROBOT_AXIS_MASK) then",
        "`t`tRETURN;",
        "`tend_if;",
        "`trestoredGroupToken := 0;",
        "`trestoredGroupGeneration := 0;",
        "`trestoredGroupSession := 0;",
        "`trestoredGroupSequence := 0;",
        "`trestoredGroupIdentitySize := 0;",
        "`trestoredGroupCommand := 0;",
        "`trestoredGroupReference := 0;",
        "`trestoredGroupAdmissionMode := 0;"
    )) + "`r`n"

$contextPublication = [string]::Join("`r`n", @(
        "`tcontextActive := 0;",
        "`tif restoredGroupActive then",
        "`t`tcontextActive := 1;",
        "`tend_if;",
        "`tpContext := pRestoreContext`$^USINT;",
        "`t(pContext + 0)^`$UDINT := contextActive;",
        "`t(pContext + 4)^`$UDINT := restoredGroupMask;",
        "`t(pContext + 8)^`$UDINT := restoredGroupToken;",
        "`t(pContext + 12)^`$UDINT := restoredGroupGeneration;",
        "`t(pContext + 16)^`$UDINT := restoredGroupSession;",
        "`t(pContext + 20)^`$UDINT := restoredGroupSequence;",
        "`t(pContext + 24)^`$UDINT := restoredGroupIdentitySize;",
        "`t(pContext + 28)^`$DINT := restoredGroupCommand;",
        "`t(pContext + 32)^`$DINT := restoredGroupReference;",
        "`t(pContext + 36)^`$DINT := restoredGroupAdmissionMode;",
        "`tResult := 0;"
    )) + "`r`n"
$helperSuffix = $contextPublication + "`r`nEND_FUNCTION`r`n"
$helperPhysical = $helperPrefix + $deindentedExtraction + $helperSuffix
$expectedGuard = [string]::Join("`r`n", @(
        "`tResult := -1;",
        "`tif (pRestoreContext = NIL) | (RestoreContextSize <> 40) |",
        "`t   (ExpectedAxisMask = 0) |",
        "`t   (ExpectedAxisMask > LMC_OWNER_ROBOT_AXIS_MASK) then",
        "`t`tRETURN;",
        "`tend_if;"
    )) + "`r`n"

$classDeclaration = [string]::Join("`r`n", @(
        "`tFUNCTION ValidateAxisOwnershipRollbackPreemptBank",
        "`t`tVAR_INPUT",
        "`t`t`tExpectedAxisMask `t: UDINT;",
        "`t`t`tpRestoreContext `t: ^void;",
        "`t`t`tRestoreContextSize `t: UDINT;",
        "`t`tEND_VAR",
        "`t`tVAR_OUTPUT",
        "`t`t`tResult `t: DINT;",
        "`t`tEND_VAR;"
    )) + "`r`n"
$helperStub = [string]::Join("`r`n", @(
        'FUNCTION LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank',
        "`tVAR_INPUT",
        "`t`tExpectedAxisMask `t: UDINT;",
        "`t`tpRestoreContext `t: ^void;",
        "`t`tRestoreContextSize `t: UDINT;",
        "`tEND_VAR",
        "`tVAR_OUTPUT",
        "`t`tResult `t: DINT;",
        "`tEND_VAR",
        '',
        'END_FUNCTION'
    )) + "`r`n"
if (($source.IndexOf($classDeclaration, [StringComparison]::Ordinal) -lt 0) -or
    ($source.LastIndexOf($classDeclaration, [StringComparison]::Ordinal) -ne
        $source.IndexOf($classDeclaration, [StringComparison]::Ordinal)) -or
    ($source.IndexOf($helperStub, [StringComparison]::Ordinal) -lt 0) -or
    ($source.LastIndexOf($helperStub, [StringComparison]::Ordinal) -ne
        $source.IndexOf($helperStub, [StringComparison]::Ordinal))) {
    throw 'Rollback actual IDE declaration or empty implementation stub drifted.'
}

$plannedSource = Replace-ExactOne `
    -Text $source `
    -Old $methodPhysical `
    -New $adapter `
    -Owner 'Rollback public adapter implementation'
$plannedSource = Replace-ExactOne `
    -Text $plannedSource `
    -Old $helperStub `
    -New $helperPhysical `
    -Owner 'Rollback private helper implementation'
$plannedSourceCanonicalLf = ConvertTo-CanonicalLf -Text $plannedSource
$plannedSourceCanonicalLfSha256 =
    Get-TextSha256 -Text $plannedSourceCanonicalLf
$plannedSourceIdeCrlfSha256 = Get-TextSha256 -Text $plannedSource
if (($null -ne $postSplitSourceCanonicalLf) -and
    (($plannedSourceCanonicalLf -cne $postSplitSourceCanonicalLf) -or
     ($plannedSourceCanonicalLfSha256 -cne
        $ExpectedPostSplitSourceCanonicalLfSha256) -or
     ($plannedSourceIdeCrlfSha256 -cne
        $ExpectedPostSplitSourceIdeCrlfSha256))) {
    throw 'Current post-split source does not match the exact planned LF/CRLF pair.'
}

$reverse = Replace-ExactOne `
    -Text $plannedSource `
    -Old $adapter `
    -New $methodPhysical `
    -Owner 'Rollback reverse adapter implementation'
$reverse = Replace-ExactOne `
    -Text $reverse `
    -Old $helperPhysical `
    -New $helperStub `
    -Owner 'Rollback reverse helper implementation'
$reverseSha256 = Get-TextSha256 -Text $reverse
if (($reverse -cne $source) -or
    ($reverseSha256 -cne $ExpectedSourceIdeCrlfSha256)) {
    throw "Rollback reverse-inline did not restore current source ($reverseSha256)."
}

$originalResultSequence = [string]::Join('|', @(
        [regex]::Matches($methodPhysical, '(?is)\bResult\s*:=\s*(?<Value>[^;]+)\s*;') |
            ForEach-Object {
                [regex]::Replace(
                    $_.Groups['Value'].Value, '\s+', '').ToLowerInvariant()
            }))
$adapterResultSequence = [string]::Join('|', @(
        [regex]::Matches($adapter, '(?is)\bResult\s*:=\s*(?<Value>[^;]+)\s*;') |
            ForEach-Object {
                [regex]::Replace(
                    $_.Groups['Value'].Value, '\s+', '').ToLowerInvariant()
            }))
if ($adapterResultSequence -cne $originalResultSequence) {
    throw 'Rollback public Result sequence changed in the planned adapter.'
}
$originalReturnCount = [regex]::Matches(
    $methodPhysical, '(?i)\bRETURN\s*;').Count
$adapterReturnCount = [regex]::Matches($adapter, '(?i)\bRETURN\s*;').Count
if ($adapterReturnCount -ne $originalReturnCount) {
    throw 'Rollback public RETURN count changed in the planned adapter.'
}

$persistentMutationPattern = (
    '(?is)(?:' +
    '\bOwnership[A-Za-z0-9_]*State\s*\[[^\]]+\]\s*' +
    '(?:\$[A-Za-z_][A-Za-z0-9_]*\s*)?' +
    '(?::|\+|-|\*|/|and|or|xor)\s*=\s*[^;]+;' +
    '|' +
    '\b_memset\s*\(\s*dest\s*:=\s*' +
    '#Ownership[A-Za-z0-9_]*State\s*\[.*?\)\s*;' +
    '|' +
    '\b_memcpy\s*\(\s*ptr1\s*:=\s*' +
    '#Ownership[A-Za-z0-9_]*State\s*\[.*?\)\s*;' +
    ')')
$adapterMutations = [regex]::Matches($adapter, $persistentMutationPattern)
$helperMutations = [regex]::Matches($helperPhysical, $persistentMutationPattern)
$normalizedAdapterMutations = @(
    foreach ($mutation in $adapterMutations) {
        [regex]::Replace($mutation.Value, '\s+', '').ToLowerInvariant()
    })
$adapterMutationInventory = [string]::Join('|', $normalizedAdapterMutations)
$adapterMutationSha256 = Get-TextSha256 -Text $adapterMutationInventory
if (($adapterMutations.Count -ne 79) -or
    ($adapterMutationInventory.Length -ne 6251) -or
    ($adapterMutationSha256 -cne
        'FFA826951AFAD84F64A21788ED0590330D5FA6A92C22B89A0363E03F9CF3BB08') -or
    ($helperMutations.Count -ne 0)) {
    throw 'Rollback planned persistent mutation inventory changed.'
}

$helperContextWrites = [regex]::Matches(
    $helperPhysical,
    '(?m)^\t\(pContext \+ (?<Offset>\d+)\)\^\$(?:U?DINT)\s*:=' )
if ($helperContextWrites.Count -ne 10) {
    throw "Rollback helper context write count is $($helperContextWrites.Count), expected 10."
}
$expectedOffsets = @(0, 4, 8, 12, 16, 20, 24, 28, 32, 36)
for ($index = 0; $index -lt $expectedOffsets.Count; $index++) {
    if ([int]$helperContextWrites[$index].Groups['Offset'].Value -ne
        $expectedOffsets[$index]) {
        throw "Rollback helper context offset $index drifted."
    }
}
$firstContextWrite = $helperPhysical.IndexOf(
    "`t(pContext + 0)^`$UDINT :=", [StringComparison]::Ordinal)
$validationEnd = $helperPhysical.IndexOf(
    $deindentedExtraction, [StringComparison]::Ordinal) +
    $deindentedExtraction.Length
if (($firstContextWrite -lt 0) -or ($firstContextWrite -lt $validationEnd)) {
    throw 'Rollback helper publishes context before validation completes.'
}
if (($helperPhysical -match '(?i)\b_memset\s*\(') -or
    ($helperPhysical -match '(?i)\b_memcpy\s*\(') -or
    ([regex]::Matches($helperPhysical, '(?i)\b_memcmp\s*\(').Count -ne 3)) {
    throw 'Rollback helper memory-call contract changed.'
}
if ([regex]::Matches(
        $adapter,
        '(?i)(?<![A-Za-z0-9_])ValidateAxisOwnershipRollbackPreemptBank\s*\(').Count -ne 1) {
    throw 'Rollback adapter helper call count is not one.'
}

function New-PlannedSourceFromFragments {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclaration
    )

    $candidateSource = Replace-ExactOne `
        -Text $source `
        -Old $classDeclaration `
        -New $CandidateDeclaration `
        -Owner 'Rollback candidate class declaration'
    $candidateSource = Replace-ExactOne `
        -Text $candidateSource `
        -Old $methodPhysical `
        -New $CandidateAdapter `
        -Owner 'Rollback candidate adapter implementation'
    return Replace-ExactOne `
        -Text $candidateSource `
        -Old $helperStub `
        -New $CandidateHelper `
        -Owner 'Rollback candidate helper implementation'
}

function Assert-RollbackSplitCandidate {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateAdapter,
        [Parameter(Mandatory = $true)][string]$CandidateHelper,
        [Parameter(Mandatory = $true)][string]$CandidateDeclaration,
        [Parameter(Mandatory = $true)][string]$CandidatePlannedSource,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $helperAbiPattern = (
        '(?s)\AFUNCTION ' +
        'LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank\r\n' +
        '\tVAR_INPUT\r\n' +
        '\t\tExpectedAxisMask \t: UDINT;\r\n' +
        '\t\tpRestoreContext \t: \^void;\r\n' +
        '\t\tRestoreContextSize \t: UDINT;\r\n' +
        '\tEND_VAR\r\n' +
        '\tVAR_OUTPUT\r\n' +
        '\t\tResult \t: DINT;\r\n' +
        '\tEND_VAR\r\n' +
        '\tVAR\r\n')
    if (($CandidateHelper -notmatch $helperAbiPattern) -or
        ([regex]::Matches(
            $CandidateHelper, '(?m)^\tVAR_INPUT\r?$').Count -ne 1) -or
        ([regex]::Matches(
            $CandidateHelper, '(?m)^\tVAR_OUTPUT\r?$').Count -ne 1) -or
        ([regex]::Matches(
            $CandidateHelper, '(?m)^END_FUNCTION\r?$').Count -ne 1) -or
        ($CandidateHelper -notmatch '(?s)\r\nEND_FUNCTION\r\n\z')) {
        throw "$Owner helper private ABI/header drifted."
    }
    if ($CandidateDeclaration -cne $classDeclaration) {
        throw "$Owner planned private class declaration drifted."
    }

    $guardIndex = $CandidateHelper.IndexOf(
        $expectedGuard, [StringComparison]::Ordinal)
    $extractionIndex = $CandidateHelper.IndexOf(
        $deindentedExtraction, [StringComparison]::Ordinal)
    $contextIndex = $CandidateHelper.IndexOf(
        $contextPublication, [StringComparison]::Ordinal)
    if (($guardIndex -lt 0) -or ($extractionIndex -lt 0) -or
        ($contextIndex -lt 0) -or ($guardIndex -ge $extractionIndex) -or
        ($extractionIndex -ge $contextIndex) -or
        ($CandidateHelper.LastIndexOf(
            $deindentedExtraction, [StringComparison]::Ordinal) -ne
            $extractionIndex) -or
        ($CandidateHelper.LastIndexOf(
            $contextPublication, [StringComparison]::Ordinal) -ne
            $contextIndex)) {
        throw "$Owner guard, exact second extraction, or success-only publication order drifted."
    }

    $allContextWrites = [regex]::Matches(
        $CandidateHelper,
        '(?m)^\s*\(pContext\s*\+\s*(?<Offset>\d+)\)\^\$(?:U?DINT)\s*:=' )
    if ($allContextWrites.Count -ne 10) {
        throw "$Owner context write count drifted."
    }
    for ($index = 0; $index -lt $expectedOffsets.Count; $index++) {
        if ([int]$allContextWrites[$index].Groups['Offset'].Value -ne
            $expectedOffsets[$index]) {
            throw "$Owner context offset/order drifted."
        }
    }
    if ([regex]::Matches(
            $CandidateHelper,
            '(?m)^\s*pContext\s*:=\s*pRestoreContext\$\^USINT\s*;').Count -ne 1) {
        throw "$Owner context alias cast drifted."
    }

    if ($CandidateAdapter.IndexOf($callMap, [StringComparison]::Ordinal) -lt 0 -or
        $CandidateAdapter.LastIndexOf($callMap, [StringComparison]::Ordinal) -ne
        $CandidateAdapter.IndexOf($callMap, [StringComparison]::Ordinal)) {
        throw "$Owner exact adapter call, immediate -3 fence, or slot map drifted."
    }
    if ([regex]::Matches($CandidateAdapter, '\^').Count -ne 1) {
        throw "$Owner adapter pointer surface drifted."
    }
    $candidateResultSequence = [string]::Join('|', @(
            [regex]::Matches(
                $CandidateAdapter,
                '(?is)\bResult\s*:=\s*(?<Value>[^;]+)\s*;') |
                ForEach-Object {
                    [regex]::Replace(
                        $_.Groups['Value'].Value,
                        '\s+', '').ToLowerInvariant()
                }))
    if (($candidateResultSequence -cne $originalResultSequence) -or
        ([regex]::Matches(
            $CandidateAdapter, '(?i)\bRETURN\s*;').Count -ne
            $originalReturnCount)) {
        throw "$Owner public Result/RETURN contract drifted."
    }

    $candidateAdapterMutations = [regex]::Matches(
        $CandidateAdapter, $persistentMutationPattern)
    $candidateHelperMutations = [regex]::Matches(
        $CandidateHelper, $persistentMutationPattern)
    $candidateMutationInventory = [string]::Join(
        '|',
        @($candidateAdapterMutations | ForEach-Object {
                [regex]::Replace(
                    $_.Value, '\s+', '').ToLowerInvariant()
            }))
    if (($candidateAdapterMutations.Count -ne 79) -or
        ($candidateMutationInventory.Length -ne 6251) -or
        ((Get-TextSha256 -Text $candidateMutationInventory) -cne
            'FFA826951AFAD84F64A21788ED0590330D5FA6A92C22B89A0363E03F9CF3BB08') -or
        ($candidateHelperMutations.Count -ne 0)) {
        throw "$Owner persistent mutation inventory drifted."
    }
    $helperScan = [regex]::Replace(
        $CandidateHelper,
        '(?s)\(\*.*?\*\)|//[^\r\n]*|"(?:[^"]|"")*"',
        { param($match) [regex]::Replace($match.Value, '[^\r\n]', ' ') })
    if (($helperScan -match '(?i)\b_memset\s*\(') -or
        ($helperScan -match '(?i)\b_memcpy\s*\(') -or
        ([regex]::Matches(
            $helperScan, '(?i)\b_memcmp\s*\(').Count -ne 3) -or
        ($helperScan -match (
            '(?i)\b[A-Za-z_][A-Za-z0-9_]*\s*\.\s*' +
            '[A-Za-z_][A-Za-z0-9_]*\s*\(')) -or
        ($helperScan -match '(?i)\b_gettime\s*\(')) {
        throw "$Owner helper read-only memory/client/clock contract drifted."
    }
    $allowedHelperTargets = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($target in @(
            'Result', 'pContext', 'contextActive',
            'ptr1', 'ptr2', 'cntr') + $helperBlockLocals) {
        [void]$allowedHelperTargets.Add($target)
    }
    foreach ($assignment in [regex]::Matches(
            $helperScan,
            ('(?m)^\s*(?<Target>[A-Za-z_][A-Za-z0-9_]*)\s*' +
             '(?:\$[A-Za-z_][A-Za-z0-9_]*\s*)?' +
             '(?::|\+|-|\*|/|and|or|xor)\s*='))) {
        $target = $assignment.Groups['Target'].Value
        if (-not $allowedHelperTargets.Contains($target)) {
            throw "$Owner helper assignment target '$target' is not local."
        }
    }
    $indexedOrMemberAssignments = [regex]::Matches(
        $helperScan,
        ('(?m)^\s*[A-Za-z_][A-Za-z0-9_]*\s*' +
         '(?:\[[^\r\n;]*\]|\.[A-Za-z_][A-Za-z0-9_]*)\s*' +
         '(?:\$[A-Za-z_][A-Za-z0-9_]*\s*)?' +
         '(?::|\+|-|\*|/|and|or|xor)\s*='))
    if ($indexedOrMemberAssignments.Count -ne 0) {
        throw "$Owner helper indexed/member assignment is forbidden."
    }
    $pointerAssignments = [regex]::Matches(
        $helperScan,
        ('(?m)^\s*\([^\r\n]+\)\^' +
         '\$[A-Za-z_][A-Za-z0-9_]*\s*' +
         '(?::|\+|-|\*|/|and|or|xor)\s*='))
    if ($pointerAssignments.Count -ne 10) {
        throw "$Owner helper pointer-write surface drifted."
    }
    $helperCallCounts = @{}
    foreach ($call in [regex]::Matches(
            $helperScan,
            ('(?i)(?<![A-Za-z0-9_])' +
             '(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*\('))) {
        $callName = $call.Groups['Name'].Value.ToLowerInvariant()
        if (@('if', 'elsif', 'while', 'case', 'not', 'and', 'or', 'then') -contains
            $callName) {
            continue
        }
        if (-not $helperCallCounts.ContainsKey($callName)) {
            $helperCallCounts[$callName] = 0
        }
        $helperCallCounts[$callName]++
    }
    if (($helperCallCounts.Count -ne 2) -or
        (-not $helperCallCounts.ContainsKey('_memcmp')) -or
        (-not $helperCallCounts.ContainsKey('to_udint')) -or
        ($helperCallCounts['_memcmp'] -ne 3) -or
        ($helperCallCounts['to_udint'] -ne 9)) {
        throw "$Owner helper call allowlist/histogram drifted."
    }

    $candidateAdapterSlice = Get-CanonicalMethodSlice `
        -PhysicalMethod $CandidateAdapter
    $candidateHelperSlice = Get-CanonicalMethodSlice `
        -PhysicalMethod $CandidateHelper
    $candidateAdapterDimensions = Get-ByteDimensions -Text $candidateAdapterSlice
    $candidateHelperDimensions = Get-ByteDimensions -Text $candidateHelperSlice
    foreach ($dimensionName in @('raw', 'lf', 'crlf')) {
        if (($candidateAdapterDimensions[$dimensionName] -ge
                $MethodSizeLimitBytes) -or
            ($candidateHelperDimensions[$dimensionName] -ge
                $MethodSizeLimitBytes)) {
            throw "$Owner method-size hard limit failed."
        }
    }

    if ($CandidateHelper -cne $helperPhysical) {
        throw "$Owner exact helper implementation bytes drifted."
    }

    $expectedCandidateSource = New-PlannedSourceFromFragments `
        -CandidateAdapter $CandidateAdapter `
        -CandidateHelper $CandidateHelper `
        -CandidateDeclaration $CandidateDeclaration
    if ($CandidatePlannedSource -cne $expectedCandidateSource) {
        throw "$Owner candidate source/fragments diverged."
    }

    $reconstructedMethod = Replace-ExactOne `
        -Text $CandidateAdapter `
        -Old $callMap `
        -New $extraction `
        -Owner "$Owner reverse call/map"
    $reconstructedMethod = Replace-ExactOne `
        -Text $reconstructedMethod `
        -Old $adapterLocalVarBlock `
        -New $originalLocalVarBlock `
        -Owner "$Owner reverse local VAR"
    if ($reconstructedMethod -cne $methodPhysical) {
        throw "$Owner adapter inverse transform did not restore the original method."
    }

    $candidateReverse = Replace-ExactOne `
        -Text $CandidatePlannedSource `
        -Old $CandidateDeclaration `
        -New $classDeclaration `
        -Owner "$Owner reverse declaration"
    $candidateReverse = Replace-ExactOne `
        -Text $candidateReverse `
        -Old $CandidateAdapter `
        -New $methodPhysical `
        -Owner "$Owner reverse adapter implementation"
    $candidateReverse = Replace-ExactOne `
        -Text $candidateReverse `
        -Old $CandidateHelper `
        -New $helperStub `
        -Owner "$Owner reverse helper implementation"
    if (($candidateReverse -cne $source) -or
        ((Get-TextSha256 -Text $candidateReverse) -cne
            $ExpectedSourceIdeCrlfSha256)) {
        throw "$Owner reverse-inline did not restore DAA8 byte-exact."
    }
}

Assert-RollbackSplitCandidate `
    -CandidateAdapter $adapter `
    -CandidateHelper $helperPhysical `
    -CandidateDeclaration $classDeclaration `
    -CandidatePlannedSource $plannedSource `
    -Owner 'Rollback planned positive candidate'

if (-not [string]::IsNullOrWhiteSpace($EmitApplyPatchPart)) {
    switch ($EmitApplyPatchPart) {
        'AdapterLocals' {
            Write-ApplyPatchBody `
                -Old $originalLocalVarBlock `
                -New $adapterLocalVarBlock
        }
        'AdapterExtraction' {
            Write-ApplyPatchBody -Old $extraction -New $callMap
        }
        'Helper' {
            Write-ApplyPatchBody -Old $helperStub -New $helperPhysical
        }
    }
    return
}

function Invoke-RollbackSplitPlannerSelfTest {
    $fixtures = @()

    $fixtures += [pscustomobject]@{
        Name = 'GuardNilInverted'
        Adapter = $adapter
        Helper = $helperPhysical.Replace(
            'pRestoreContext = NIL', 'pRestoreContext <> NIL')
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'GuardSizeChanged'
        Adapter = $adapter
        Helper = $helperPhysical.Replace(
            'RestoreContextSize <> 40', 'RestoreContextSize <> 44')
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'GuardMaskWeakened'
        Adapter = $adapter
        Helper = $helperPhysical.Replace(
            'ExpectedAxisMask > LMC_OWNER_ROBOT_AXIS_MASK',
            'ExpectedAxisMask > 0xFFFFFFFF')
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'WrongExtractionAnchor'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old "`tpreemptBankValid := TRUE;`r`n" `
            -New "`tpreemptBankValid := FALSE;`r`n" `
            -Owner 'WrongExtractionAnchor mutation'
        Declaration = $classDeclaration
    }

    $earlyHelper = Replace-ExactOne `
        -Text $helperPhysical `
        -Old $contextPublication `
        -New '' `
        -Owner 'ContextPublishedEarly removal'
    $earlyHelper = Replace-ExactOne `
        -Text $earlyHelper `
        -Old $deindentedExtraction `
        -New ($contextPublication + $deindentedExtraction) `
        -Owner 'ContextPublishedEarly insertion'
    $fixtures += [pscustomobject]@{
        Name = 'ContextPublishedEarly'
        Adapter = $adapter
        Helper = $earlyHelper
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'ContextWriteDropped'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old "`t(pContext + 36)^`$DINT := restoredGroupAdmissionMode;`r`n" `
            -New '' `
            -Owner 'ContextWriteDropped mutation'
        Declaration = $classDeclaration
    }
    $context28 = "`t(pContext + 28)^`$DINT := restoredGroupCommand;`r`n"
    $context32 = "`t(pContext + 32)^`$DINT := restoredGroupReference;`r`n"
    $reorderedHelper = Replace-ExactOne `
        -Text $helperPhysical `
        -Old ($context28 + $context32) `
        -New ($context32 + $context28) `
        -Owner 'ContextWriteReordered mutation'
    $fixtures += [pscustomobject]@{
        Name = 'ContextWriteReordered'
        Adapter = $adapter
        Helper = $reorderedHelper
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperMadeGlobal'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old 'FUNCTION LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank' `
            -New 'FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnershipRollbackPreemptBank' `
            -Owner 'HelperMadeGlobal implementation'
        Declaration = Replace-ExactOne `
            -Text $classDeclaration `
            -Old "`tFUNCTION ValidateAxisOwnershipRollbackPreemptBank" `
            -New "`tFUNCTION GLOBAL ValidateAxisOwnershipRollbackPreemptBank" `
            -Owner 'HelperMadeGlobal declaration'
    }
    $bypassedCallMap = Replace-ExactOne `
        -Text $callMap `
        -Old "`t`t`t`tResult := -3;`r`n" `
        -New "`t`t`t`tResult := rollbackPreemptResult;`r`n" `
        -Owner 'AdapterResultFenceBypassed call-map mutation'
    $fixtures += [pscustomobject]@{
        Name = 'AdapterResultFenceBypassed'
        Adapter = Replace-ExactOne `
            -Text $adapter `
            -Old $callMap `
            -New $bypassedCallMap `
            -Owner 'AdapterResultFenceBypassed mutation'
        Helper = $helperPhysical
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperPersistentWriteAdded'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $contextPublication `
            -New ("`tOwnershipState[0] := 1;`r`n" + $contextPublication) `
            -Owner 'HelperPersistentWriteAdded mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperExtractionDisabled'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $deindentedExtraction `
            -New (
                "`tif FALSE then`r`n" + $deindentedExtraction +
                "`tend_if;`r`n") `
            -Owner 'HelperExtractionDisabled mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperEarlyReturnAdded'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $expectedGuard `
            -New ($expectedGuard + "`tRETURN;`r`n") `
            -Owner 'HelperEarlyReturnAdded mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperValidatedScalarRewritten'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $contextPublication `
            -New ("`trestoredGroupMask := 0;`r`n" + $contextPublication) `
            -Owner 'HelperValidatedScalarRewritten mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperIndexedPersistentWriteAdded'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $contextPublication `
            -New ("`tZeroHomeState[0] := 1;`r`n" + $contextPublication) `
            -Owner 'HelperIndexedPersistentWriteAdded mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'HelperForeignPointerWriteAdded'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $contextPublication `
            -New ("`t(pContext + 1)^`$USINT := 0;`r`n" + $contextPublication) `
            -Owner 'HelperForeignPointerWriteAdded mutation'
        Declaration = $classDeclaration
    }
    $trailingInterfaceAnchor = [string]::Join("`r`n", @(
            "`t`tResult `t: DINT;",
            "`tEND_VAR",
            "`tVAR"
        )) + "`r`n"
    $fixtures += [pscustomobject]@{
        Name = 'HelperTrailingVarInputAdded'
        Adapter = $adapter
        Helper = Replace-ExactOne `
            -Text $helperPhysical `
            -Old $trailingInterfaceAnchor `
            -New ([string]::Join("`r`n", @(
                    "`t`tResult `t: DINT;",
                    "`tEND_VAR",
                    "`tVAR_INPUT",
                    "`t`tUnexpectedInput : UDINT;",
                    "`tEND_VAR",
                    "`tVAR"
                )) + "`r`n") `
            -Owner 'HelperTrailingVarInputAdded mutation'
        Declaration = $classDeclaration
    }
    $fixtures += [pscustomobject]@{
        Name = 'ReverseSourceMismatch'
        Adapter = $adapter
        Helper = $helperPhysical
        Declaration = $classDeclaration
        PlannedOverride = $plannedSource + "// reverse drift`r`n"
    }
    $oversizedAdapter = Replace-ExactOne `
        -Text $adapter `
        -Old "`r`nEND_FUNCTION`r`n" `
        -New ("`r`n// " + ('X' * 4000) + "`r`nEND_FUNCTION`r`n") `
        -Owner 'AdapterOversized mutation'
    $fixtures += [pscustomobject]@{
        Name = 'AdapterOversized'
        Adapter = $oversizedAdapter
        Helper = $helperPhysical
        Declaration = $classDeclaration
    }

    $expectedErrorPatterns = @{
        GuardNilInverted = 'guard, exact second extraction'
        GuardSizeChanged = 'guard, exact second extraction'
        GuardMaskWeakened = 'guard, exact second extraction'
        WrongExtractionAnchor = 'guard, exact second extraction'
        ContextPublishedEarly = 'guard, exact second extraction'
        ContextWriteDropped = 'guard, exact second extraction'
        ContextWriteReordered = 'guard, exact second extraction'
        HelperMadeGlobal = 'helper private ABI/header'
        AdapterResultFenceBypassed = 'exact adapter call'
        HelperPersistentWriteAdded = 'persistent mutation inventory'
        HelperExtractionDisabled = 'exact helper implementation bytes'
        HelperEarlyReturnAdded = 'exact helper implementation bytes'
        HelperValidatedScalarRewritten = 'exact helper implementation bytes'
        HelperIndexedPersistentWriteAdded = 'helper indexed/member assignment'
        HelperForeignPointerWriteAdded = 'helper pointer-write surface'
        HelperTrailingVarInputAdded = 'helper private ABI/header'
        ReverseSourceMismatch = 'candidate source/fragments diverged'
        AdapterOversized = 'method-size hard limit'
    }

    $expectedFixtureCount = 18
    if (($fixtures.Count -ne $expectedFixtureCount) -or
        ($expectedErrorPatterns.Count -ne $expectedFixtureCount)) {
        throw (
            'Rollback planner self-test fixture/error-map count drifted ' +
            "($($fixtures.Count)/$($expectedErrorPatterns.Count), expected " +
            "$expectedFixtureCount/$expectedFixtureCount).")
    }
    $actualFixtureNames = [string]::Join(
        '|', @($fixtures.Name | Sort-Object))
    $expectedFixtureNames = [string]::Join(
        '|', @($expectedErrorPatterns.Keys | Sort-Object))
    if ($actualFixtureNames -cne $expectedFixtureNames) {
        throw 'Rollback planner self-test fixture/error-map key set drifted.'
    }

    $rejected = 0
    foreach ($fixture in $fixtures) {
        $fixturePlanned = if (
            $fixture.PSObject.Properties.Name -contains 'PlannedOverride') {
            $fixture.PlannedOverride
        }
        else {
            New-PlannedSourceFromFragments `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHelper $fixture.Helper `
                -CandidateDeclaration $fixture.Declaration
        }
        $didReject = $false
        try {
            Assert-RollbackSplitCandidate `
                -CandidateAdapter $fixture.Adapter `
                -CandidateHelper $fixture.Helper `
                -CandidateDeclaration $fixture.Declaration `
                -CandidatePlannedSource $fixturePlanned `
                -Owner "Rollback planner fixture $($fixture.Name)"
        }
        catch {
            $didReject = $true
            $expectedErrorPattern = $expectedErrorPatterns[$fixture.Name]
            if ([string]::IsNullOrWhiteSpace($expectedErrorPattern)) {
                throw "Rollback planner fixture '$($fixture.Name)' has no expected error contract."
            }
            if ($_.Exception.Message -notmatch
                    [regex]::Escape($expectedErrorPattern)) {
                throw (
                    "Rollback planner fixture '$($fixture.Name)' rejected by " +
                    "the wrong contract: $($_.Exception.Message)")
            }
        }
        if (-not $didReject) {
            throw "Rollback planner negative fixture '$($fixture.Name)' was accepted."
        }
        $rejected++
    }
    return $rejected
}

if ($RunSelfTest) {
    $negativeCount = Invoke-RollbackSplitPlannerSelfTest
    Write-Host (
        'PASS LASAL.AxisOwnershipRollbackSplitPlan.SelfTest (' +
        "$negativeCount/$negativeCount negative fixtures rejected; " +
        'positive candidate accepted)')
    return
}

$adapterSlice = Get-CanonicalMethodSlice -PhysicalMethod $adapter
$helperSlice = Get-CanonicalMethodSlice -PhysicalMethod $helperPhysical
$result = [ordered]@{
    status = 'PASS'
    mode = 'in-memory planning only; canonical source is not written'
    source = [ordered]@{
        inputMode = $sourceInputMode
        inputLineEnding = $sourceInputLineEnding
        inputPhysicalBytes = $Utf8.GetByteCount($sourceInputPhysical)
        inputPhysicalSha256 = $sourceInputPhysicalSha256
        inputCanonicalLfBytes = $Utf8.GetByteCount($sourceInputCanonicalLf)
        inputCanonicalLfSha256 = $sourceInputCanonicalLfSha256
        inputIdeCrlfBytes = $Utf8.GetByteCount($sourceInputIdeCrlf)
        inputIdeCrlfSha256 = $sourceInputIdeCrlfSha256
        bytes = $Utf8.GetByteCount($source)
        sha256 = $sourceSha256
        canonicalLfBytes = $Utf8.GetByteCount($sourceCanonicalLf)
        canonicalLfSha256 = Get-TextSha256 -Text $sourceCanonicalLf
        ideCrlfBytes = $Utf8.GetByteCount($source)
        ideCrlfSha256 = $sourceSha256
    }
    originalMethod = [ordered]@{
        dimensions = Get-ByteDimensions -Text $methodSlice
        sha256 = $methodSha256
        canonicalLfBytes = $Utf8.GetByteCount($methodCanonicalLf)
        canonicalLfSha256 = $methodCanonicalLfSha256
        ideCrlfBytes = $Utf8.GetByteCount($methodPhysical)
        ideCrlfSha256 = Get-TextSha256 -Text $methodPhysical
    }
    extraction = [ordered]@{
        dimensions = Get-ByteDimensions -Text $extraction
        sha256 = $extractionSha256
        canonicalLfBytes = $Utf8.GetByteCount($extractionCanonicalLf)
        canonicalLfSha256 = $extractionCanonicalLfSha256
        ideCrlfBytes = $Utf8.GetByteCount($extraction)
        ideCrlfSha256 = $extractionSha256
        selectedOccurrence = 2
    }
    candidate = [ordered]@{
        adapter = [ordered]@{
            dimensions = Get-ByteDimensions -Text $adapterSlice
            sha256 = Get-TextSha256 -Text $adapterSlice
            retainedPersistentWrites = $adapterMutations.Count
            resultAssignments = [regex]::Matches(
                $adapter, '(?i)\bResult\s*:=').Count
            returns = $adapterReturnCount
        }
        helper = [ordered]@{
            dimensions = Get-ByteDimensions -Text $helperSlice
            sha256 = Get-TextSha256 -Text $helperSlice
            persistentWrites = $helperMutations.Count
            contextWrites = $helperContextWrites.Count
            memcmp = [regex]::Matches(
                $helperPhysical, '(?i)\b_memcmp\s*\(').Count
        }
        callMap = [ordered]@{
            dimensions = Get-ByteDimensions -Text $callMap
            sha256 = Get-TextSha256 -Text $callMap
        }
        classDeclaration = [ordered]@{
            dimensions = Get-ByteDimensions -Text $classDeclaration
            sha256 = Get-TextSha256 -Text $classDeclaration
        }
        wholeSource = [ordered]@{
            bytes = $Utf8.GetByteCount($plannedSource)
            sha256 = $plannedSourceIdeCrlfSha256
            canonicalLfBytes = $Utf8.GetByteCount($plannedSourceCanonicalLf)
            canonicalLfSha256 = $plannedSourceCanonicalLfSha256
            ideCrlfBytes = $Utf8.GetByteCount($plannedSource)
            ideCrlfSha256 = $plannedSourceIdeCrlfSha256
        }
    }
    reverseInline = [ordered]@{
        restoredByteExact = ($reverse -ceq $source)
        sha256 = $reverseSha256
        restoredCanonicalLfExact = (
            (ConvertTo-CanonicalLf -Text $reverse) -ceq $sourceCanonicalLf)
        canonicalLfSha256 = Get-TextSha256 -Text (
            ConvertTo-CanonicalLf -Text $reverse)
    }
    expectedPostSplitMethodInventory = [ordered]@{
        classes = 6
        methods = 96
        underLimit = 92
        baselineDebt = 4
    }
}

$result | ConvertTo-Json -Depth 8

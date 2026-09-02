param(
    [switch]$RunSelfTest,
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    throw 'RepositoryRoot is required.'
}
if ($RunSelfTest) {
    $verifierPath = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
    & $verifierPath `
        -RepositoryRoot $RepositoryRoot `
        -AxisRebaseBarrierVerifierSelfTestOnly
    return
}

$controlPath = Join-Path $RepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$control = Get-Content -Raw -LiteralPath $controlPath
$networkRoot = (Resolve-Path -LiteralPath (Join-Path $RepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network'))).Path
$networkRootPrefix = $networkRoot + [IO.Path]::DirectorySeparatorChar
$networkArtifacts = @(
    Get-ChildItem -LiteralPath $networkRoot -Recurse -Force -File |
        Sort-Object FullName |
        ForEach-Object {
            if (-not $_.FullName.StartsWith(
                    $networkRootPrefix,
                    [StringComparison]::OrdinalIgnoreCase)) {
                throw (
                    'TW19 Home barrier fixture Network artifact resolved ' +
                    'outside its root: ' + $_.FullName)
            }
            [pscustomobject]@{
                Name = $_.FullName.Substring($networkRootPrefix.Length).
                    Replace('/', '\')
                Text = [Text.Encoding]::ASCII.GetString(
                    [IO.File]::ReadAllBytes($_.FullName))
            }
        })
if ($networkArtifacts.Count -eq 0) {
    throw 'TW19 Home barrier fixture Network artifact inventory is empty.'
}
$negativeFixtures = [System.Collections.Generic.List[object]]::new()

function Add-NegativeFixture {
    param(
        [string]$Name,
        [string]$Pattern,
        [string]$Replacement
    )

    $regex = [regex]::new(
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $matches = $regex.Matches($control)
    if ($matches.Count -ne 1) {
        throw (
            "TW19 Home barrier fixture '$Name' found $($matches.Count) " +
            'targets, expected exactly one.')
    }
    $mutated = $regex.Replace($control, $Replacement, 1)
    if ($mutated -ceq $control) {
        throw "TW19 Home barrier fixture '$Name' did not mutate the source."
    }
    $negativeFixtures.Add([pscustomobject]@{
            Name = $Name
            Control = $mutated
        })
}

function Add-NetworkNegativeFixture {
    param(
        [string]$Name,
        [string]$ArtifactName,
        [string]$Pattern,
        [string]$Replacement
    )

    $regex = [regex]::new(
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
        [System.Text.RegularExpressions.RegexOptions]::Singleline -bor
        [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $targetCount = 0
    $mutatedArtifacts = @(
        foreach ($artifact in $networkArtifacts) {
            $artifactText = [string]$artifact.Text
            if ([string]$artifact.Name -ceq $ArtifactName) {
                $targetCount++
                $matches = $regex.Matches($artifactText)
                if ($matches.Count -ne 1) {
                    throw (
                        "TW19 Home barrier fixture '$Name' found " +
                        "$($matches.Count) mutation targets in '$ArtifactName', " +
                        'expected exactly one.')
                }
                $artifactText = $regex.Replace(
                    $artifactText,
                    $Replacement,
                    1)
            }
            [pscustomobject]@{
                Name = [string]$artifact.Name
                Text = $artifactText
            }
        })
    if ($targetCount -ne 1) {
        throw (
            "TW19 Home barrier fixture '$Name' found $targetCount " +
            "Network artifact(s) named '$ArtifactName', expected one.")
    }
    $negativeFixtures.Add([pscustomobject]@{
            Name = $Name
            Control = $control
            NetworkArtifacts = $mutatedArtifacts
        })
}

Add-NetworkNegativeFixture 'AuthoritativeNetworkConnectionAdded' `
    'Comm_Network\Comm_Network.lcn' `
    '(?m)^(\s*</Connections>\s*\r?\n\s*<!-- Headerfiles -->)' `
    ('        <Connection Source="TCPMotionInterface1.ControlCommands" ' +
     'Destination="LMCControlCommandService1.AxisRebaseRequiredState"/>' +
     [Environment]::NewLine + '${1}')
Add-NetworkNegativeFixture 'GeneratedNetworkConnectionAdded' `
    'Comm_Network\ONE_Comm_Network_Table.st' `
    ('(?m)^([ \t]*TO_UDINT\(\d+\),[ \t]*"ControlCommands",[ \t]*' +
     'TO_UDINT\(\d+\),[ \t]*)"ClassSvr"(,[^\r\n]*\r?)$') `
    '${1}"AxisRebaseRequiredState"${2}'

Add-NegativeFixture 'BarrierGateDisabled' `
    '(#define\s+LMC_AXIS_REBASE_BARRIER_ENABLED\s+)TRUE' '${1}FALSE'
Add-NegativeFixture 'MagicDrift' `
    '(#define\s+LMC_AXIS_REBASE_STATE_MAGIC\s+)0x52425300' '${1}0x52425400'
Add-NegativeFixture 'InverseMaskDrift' `
    '(#define\s+LMC_AXIS_REBASE_STATE_INVERSE_MASK\s+)0x000000F0' '${1}0x000000E0'
Add-NegativeFixture 'PersistenceRetryDrift' `
    '(#define\s+LMC_OWNER_REBASE_PERSIST_RETRY\s+)-4' '${1}-3'
Add-NegativeFixture 'HelperReadRemoved' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     'rebaseReadResult\s*:=\s*)ReadAxisRebaseRequiredMask\(\)') '${1}0'
Add-NegativeFixture 'ReaderInvalidStateOpens' `
    ('(FUNCTION\s+LMCControlCommandService::ReadAxisRebaseRequiredMask\b.*?' +
     'if\s+stateValid\s*=\s*FALSE\s+then\s*axisMask\s*:=\s*)' +
     'LMC_AXIS_REBASE_STATE_AXIS_MASK') '${1}0'
Add-NegativeFixture 'HelperPowerOffBlockedInstead' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     '0x2023\s*:.*?\(pRequestFrame\s*\+\s*12\)\^\s*=\s*)1') '${1}0'
Add-NegativeFixture 'HelperMoveShapeDrift' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     '0x209F\s*,\s*0x20A0\s*:\s*if\s*\(RequestFrameSize\s*=\s*)40') '${1}39'
Add-NegativeFixture 'HelperAxisResetBlocked' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     'case\s+CommandId\s+of\s*0x2023)(\s*:)') '${1}, 0x2024${2}'
Add-NegativeFixture 'HelperGroupEnableMissing' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     ')0x2047\s*,\s*0x204A\s*:') '${1}0x204A:'
Add-NegativeFixture 'HelperAdminDetailDrift' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     'case\s+CommandId\s+of\s*0x7D22\s*:.*?' +
     '\(pResponseFrame\s*\+\s*20\)\^\$UDINT\s*:=\s*)41') '${1}42'
Add-NegativeFixture 'HelperLegacyConflictCleared' `
    ('(FUNCTION\s+LMCControlCommandService::HandleAxisOwnershipSafetyRepeat\b.*?' +
     'case\s+CommandId\s+of\s*0x7D22\s*:.*?else\s+if\s+ResponseCapacity\s*<\s*16.*?' +
     '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*)' +
     'LMC_OWNER_ADAPTER_ERROR_CONFLICT') '${1}0'
Add-NegativeFixture 'SetKinMalformedPathBlocked' `
    ('(FUNCTION\s+LMCControlCommandService::HandleGroupCommands\b.*?' +
     '0x20E7\s*:.*?groupReadErrorId\s*:=\s*-7\s*;\s*' +
     'if\s+)kinValid\s*=\s*TRUE') '${1}TRUE'
Add-NegativeFixture 'SetKinBarrierReadRemoved' `
    ('(FUNCTION\s+LMCControlCommandService::HandleGroupCommands\b.*?' +
     '0x20E7\s*:.*?kinRebaseMask\s*:=\s*)ReadAxisRebaseRequiredMask\(\)') '${1}0'
Add-NegativeFixture 'SetKinConflictDrift' `
    ('(FUNCTION\s+LMCControlCommandService::HandleGroupCommands\b.*?' +
     '0x20E7\s*:.*?groupReadErrorId\s*:=\s*)' +
     'LMC_OWNER_ADAPTER_ERROR_CONFLICT') '${1}-2'
Add-NegativeFixture 'ReserveInvalidStateOpens' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::ReserveAxisOwnership\b.*?' +
     'rebaseReadResult\s*:=\s*)ReadAxisRebaseRequiredMask\(\)') '${1}0'
Add-NegativeFixture 'ReserveGroupResetNotAllowed' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::ReserveAxisOwnership\b.*?' +
     '\(CommandId\s*=\s*0x2024\)\s*\|\s*' +
     '\(CommandId\s*=\s*)0x2049') '${1}0x204A'
Add-NegativeFixture 'ReserveConflictDrift' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::ReserveAxisOwnership\b.*?' +
     'rebaseAdmissionAllowed\s*=\s*FALSE\)\s+then\s*Result\s*:=\s*)LMC_OWNER_REBASE_REQUIRED') '${1}-3'
Add-NegativeFixture 'CommitTw20Arms' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     'OwnershipState\[recordBase\s*\+\s*25\]\$UINT\s*=\s*)2') '${1}1'
Add-NegativeFixture 'CommitOwnerMutationBeforeArm' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     'rebaseUpdateResult\s*:=\s*UpdateAxisRebaseRequiredState\()') `
    '${1}OwnershipState[recordBase + 1] := activeState; rebaseUpdateResult := UpdateAxisRebaseRequiredState('
Add-NegativeFixture 'CommitIdentityMemsetBeforeArm' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     ')(rebaseUpdateResult\s*:=\s*UpdateAxisRebaseRequiredState\()') `
    '${1}_memset(dest:=#OwnershipIdentityState[0], usByte:=0, cntr:=4); ${2}'
Add-NegativeFixture 'CommitNativeCallBeforeValidation' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     ')(Result\s*:=\s*ValidateAxisOwnership\()') `
    '${1}LMCAxis1.ReadAxisStatus(); ${2}'
Add-NegativeFixture 'CommitNativeCallBeforeArm' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     ')(rebaseUpdateResult\s*:=\s*UpdateAxisRebaseRequiredState\()') `
    '${1}LMCAxis1.ReadAxisStatus(); ${2}'
Add-NegativeFixture 'CommitPublishCallBeforeArm' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     ')(rebaseUpdateResult\s*:=\s*UpdateAxisRebaseRequiredState\()') `
    '${1}PublishAxisOwnership(); ${2}'
Add-NegativeFixture 'CommitEarlyUpdaterCall' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::CommitAxisOwnership\b.*?' +
     ')(rebaseUpdateResult\s*:=\s*UpdateAxisRebaseRequiredState\()') `
    ('${1}UpdateAxisRebaseRequiredState(' +
     'SetAxisMask:=0, ClearAxisMask:=0); ${2}')
Add-NegativeFixture 'UpdaterPromotedGlobal' `
    ('(?m)^(FUNCTION[ \t]+)' +
     '(LMCControlCommandService::UpdateAxisRebaseRequiredState)[ \t]*\r?$') `
    '${1}GLOBAL ${2}'
Add-NegativeFixture 'UpdaterReadRangeUpperBoundDrift' `
    ('(FUNCTION\s+LMCControlCommandService::UpdateAxisRebaseRequiredState\b.*?' +
     'readResult\s*>\s*)15') '${1}16'
Add-NegativeFixture 'UpdaterInitialFailureOpens' `
    ('(FUNCTION\s+LMCControlCommandService::UpdateAxisRebaseRequiredState\b.*?' +
     'Result\s*:=\s*)-1') '${1}0'
Add-NegativeFixture 'UpdaterEarlySuccessBeforeRead' `
    ('(FUNCTION\s+LMCControlCommandService::UpdateAxisRebaseRequiredState\b.*?' +
     'Result\s*:=\s*-1\s*;)') '${1} Result := 0;'
Add-NegativeFixture 'UpdaterReadbackInverted' `
    ('(FUNCTION\s+LMCControlCommandService::UpdateAxisRebaseRequiredState\b.*?' +
     'if\s+AxisRebaseRequiredState\.Read\(\)\s*)<>') '${1}='
Add-NegativeFixture 'PublishSuccessProofDrift' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::PublishAxisOwnership\b.*?' +
     'if\s+LMC_AXIS_REBASE_BARRIER_ENABLED\s*&\s*' +
     '\(ReportKind\s*=\s*)LMC_OWNER_REPORT_TERMINAL_SUCCESS') `
    '${1}LMC_OWNER_REPORT_TERMINAL_SAFE_FAILURE'
Add-NegativeFixture 'PublishClearsWrongMask' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::PublishAxisOwnership\b.*?' +
     'ClearAxisMask\s*:=\s*)AxisMask') '${1}0'
Add-NegativeFixture 'UpdaterOverlapGuardRemoved' `
    ('(FUNCTION\s+LMCControlCommandService::UpdateAxisRebaseRequiredState\b.*?' +
     '\(SetAxisMask\s+and\s+ClearAxisMask\)\s*)<>') '${1}='
Add-NegativeFixture 'HomeRetryQuarantinesOwner' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::ProcessAxisZeroHome\b.*?' +
     'if\s+homePublishResult\s*=\s*LMC_OWNER_REBASE_PERSIST_RETRY\s+then\s*)') `
    '${1}OwnershipState[24] := 1; '
Add-NegativeFixture 'ThirdRetainedWriteAdded' `
    ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::ProcessAxisZeroHome\b)') `
    'AxisRebaseRequiredState.Write(input:=0); ${1}'

[pscustomobject]@{
    Control = $control
    NetworkArtifacts = $networkArtifacts
    NegativeFixtures = $negativeFixtures.ToArray()
}

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
        -DiagnosticsMethodSplitVerifierSelfTestOnly
    return
}

$diagnosticsPath = Join-Path $RepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCDiagnosticsService\LMCDiagnosticsService.st')
$diagnostics = Get-Content -Raw -LiteralPath $diagnosticsPath
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
    $matches = $regex.Matches($diagnostics)
    if ($matches.Count -ne 1) {
        throw (
            "Diagnostics method split fixture '$Name' found " +
            "$($matches.Count) targets, expected exactly one.")
    }
    $mutated = $regex.Replace($diagnostics, $Replacement, 1)
    if ($mutated -ceq $diagnostics) {
        throw "Diagnostics method split fixture '$Name' did not mutate the source."
    }
    $negativeFixtures.Add([pscustomobject]@{
            Name = $Name
            Diagnostics = $mutated
        })
}

foreach ($helperName in @(
        'HandleEncoderMaintenancePreemption',
        'HandleAxisDs402HomeReceiptStages',
        'HandleAxisDs402HomeCleanupStages')) {
    Add-NegativeFixture ($helperName + 'PromotedGlobal') `
        ('(?m)^(FUNCTION[ \t]+)(LMCDiagnosticsService::' +
         [regex]::Escape($helperName) + ')[ \t]*\r?$') `
        '${1}GLOBAL ${2}'
}

Add-NegativeFixture 'EncoderAbiPointerTypeDrift' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleEncoderMaintenancePreemption\b.*?' +
     'pPreemptionSnapshot\s*:\s*)\^USINT') '${1}^void'
Add-NegativeFixture 'ReceiptAbiStageTypeDrift' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeReceiptStages\b.*?' +
     'Stage\s*:\s*)DINT') '${1}UINT'
$cleanupGroupedAbiPattern =
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?' +
     ')InitialCurrentCycle\s*,\s*ServiceNow(\s*:\s*UDINT)')
$cleanupSplitAbiPattern =
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?)' +
     '(?<InitialIndent>[ \t]*)InitialCurrentCycle[ \t]*:[ \t]*' +
     'UDINT[ \t]*;(?<LineBreak>\r?\n)' +
     '(?<ServiceIndent>[ \t]*)ServiceNow[ \t]*:[ \t]*UDINT[ \t]*;')
$cleanupFixtureOptions =
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
    [System.Text.RegularExpressions.RegexOptions]::Singleline
$cleanupGroupedAbiMatches = [regex]::Matches(
    $diagnostics,
    $cleanupGroupedAbiPattern,
    $cleanupFixtureOptions)
$cleanupSplitAbiMatches = [regex]::Matches(
    $diagnostics,
    $cleanupSplitAbiPattern,
    $cleanupFixtureOptions)
if (($cleanupGroupedAbiMatches.Count + $cleanupSplitAbiMatches.Count) -ne 1) {
    throw (
        'Diagnostics method split fixture CleanupAbiCycleOrderDrift found ' +
        "$($cleanupGroupedAbiMatches.Count) grouped and " +
        "$($cleanupSplitAbiMatches.Count) split targets, expected one total.")
}
if ($cleanupGroupedAbiMatches.Count -eq 1) {
    Add-NegativeFixture 'CleanupAbiCycleOrderDrift' `
        $cleanupGroupedAbiPattern `
        '${1}ServiceNow, InitialCurrentCycle${2}'
}
else {
    Add-NegativeFixture 'CleanupAbiCycleOrderDrift' `
        $cleanupSplitAbiPattern `
        ('${1}${InitialIndent}ServiceNow : UDINT;' +
         '${LineBreak}${ServiceIndent}InitialCurrentCycle : UDINT;')
}

Add-NegativeFixture 'EncoderCallStageDrift' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessEncoderMaintenance\b.*?' +
     'HandleEncoderMaintenancePreemption\(\s*Stage\s*:=\s*)stage') `
    '${1}recordBase'
Add-NegativeFixture 'EncoderImmediateGuardOpens' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessEncoderMaintenance\b.*?' +
     'HandleEncoderMaintenancePreemption\(.*?\)\s*;\s*' +
     'if\s+preemptionResult\s*)<>') '${1}='
Add-NegativeFixture 'EncoderHelperResamplesClock' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleEncoderMaintenancePreemption\b.*?' +
     'Result\s*:=\s*1\s*;)') '${1} Result := ops.tAbsolute$DINT;'
Add-NegativeFixture 'EncoderHelperCopiesPreemptionAgain' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleEncoderMaintenancePreemption\b.*?' +
     'Result\s*:=\s*1\s*;)') `
    '${1} AxisOwnership.CopyAxisOwnershipPreemption();'

Add-NegativeFixture 'ReceiptDefaultResultOpens' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeReceiptStages\b.*?' +
     'Result\s*:=\s*)1') '${1}0'
Add-NegativeFixture 'ReceiptImmediateGuardOpens' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'HandleAxisDs402HomeReceiptStages\(.*?\)\s*;\s*' +
     'if\s+receiptResult\s*)<>') '${1}='
Add-NegativeFixture 'ReceiptHelperResamplesLatch' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeReceiptStages\b.*?' +
     'Result\s*:=\s*1\s*;)') '${1} InputLatch.CopySnapshot();'

Add-NegativeFixture 'CleanupStageGuardDrift' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'stage\s*:=\s*Ds402HomeState\[92\]\s*;\s*' +
     'if\s+stage\s*<\s*)90') '${1}89'
Add-NegativeFixture 'CleanupTrueSafetyTokenCleared' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'if\s+preemptionCleanup\s+then\s*' +
     'HandleAxisDs402HomeCleanupStages\(.*?' +
     'SafetyAdmissionToken\s*:=\s*)safetyAdmissionToken') '${1}0'
Add-NegativeFixture 'CleanupFalseSafetyTokenLeaked' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'else\s*HandleAxisDs402HomeCleanupStages\(.*?' +
     'SafetyAdmissionToken\s*:=\s*)0') '${1}safetyAdmissionToken'
Add-NegativeFixture 'CleanupFalseOwnerTokenLeaked' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'else\s*HandleAxisDs402HomeCleanupStages\(.*?' +
     'SafetyOwnerGeneration\s*:=\s*)0') '${1}safetyOwnerGeneration'
Add-NegativeFixture 'CleanupFalseFlagEscalated' `
    ('(FUNCTION\s+LMCDiagnosticsService::ProcessAxisDs402Home\b.*?' +
     'else\s*HandleAxisDs402HomeCleanupStages\(.*?' +
     'PreemptionCleanup\s*:=\s*)FALSE') '${1}TRUE'

Add-NegativeFixture 'CleanupInitialCycleResampled' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?' +
     'currentCycle\s*:=\s*)InitialCurrentCycle') '${1}ServiceNow'
Add-NegativeFixture 'CleanupHelperResamplesClock' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?' +
     'currentCycle\s*:=\s*InitialCurrentCycle\s*;)') `
    '${1} currentCycle := ops.tAbsolute;'
Add-NegativeFixture 'CleanupHelperResamplesLatch' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?' +
     'currentCycle\s*:=\s*InitialCurrentCycle\s*;)') `
    '${1} InputLatch.CopySnapshot();'
Add-NegativeFixture 'CleanupActivePhaseRelocated' `
    ('(FUNCTION\s+LMCDiagnosticsService::' +
     'HandleAxisDs402HomeCleanupStages\b.*?' +
     'RequiredPhase\s*:=\s*)LMC_DIAG_OWNER_PHASE_ACTIVE') `
    '${1}LMC_DIAG_OWNER_PHASE_RESERVED'

$oversizeInjection = "`r`n// " + ('X' * 33000)
Add-NegativeFixture 'ProcessDs402HomeExceedsSizeCeiling' `
    ('(?m)^(FUNCTION[ \t]+LMCDiagnosticsService::' +
     'ProcessAxisDs402Home[ \t]*\r?)$') ('$1' + $oversizeInjection)

[pscustomobject]@{
    Diagnostics = $diagnostics
    NegativeFixtures = $negativeFixtures.ToArray()
}

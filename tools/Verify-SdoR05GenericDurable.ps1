param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$journalPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\DiagnosticsMutationJournal.cs'
$testPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\DiagnosticsMutationJournalTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$journal = Get-Content -LiteralPath $journalPath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing ${Label}: $Needle"
    }
}
Require-Text $journal 'case LMCSignalValueType.Int8:' 'Int8 durable metadata'
Require-Text $journal 'case LMCSignalValueType.UInt16:' 'UInt16 durable metadata'
Require-Text $journal 'case LMCSignalValueType.UInt32:' 'UInt32 durable metadata'
Require-Text $journal 'objectIndex == 0x6060' '0x6060 durable deny'
Require-Text $journal 'exactRequestRecoverable' 'generic restart predicate'
Require-Text $journal 'RequestNotRecoverable' 'generic restart disposition'
if ($journal.IndexOf('TargetNotApproved', [StringComparison]::Ordinal) -ge 0) {
    throw 'Legacy approved-target restart disposition remains.'
}
Require-Text $tests 'GenericScalarMetadataSupportsOneTwoFourBytes' '1/2/4-byte durable test'
Require-Text $tests 'SemanticModeObjectIsRejectedForDurableRecovery' '0x6060 durable test'
Require-Text $design 'current-dev R05-A' 'R05-A design sync'
Write-Host 'PASS SDO-R05-A generic durable metadata source contract.'

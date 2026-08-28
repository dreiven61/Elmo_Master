param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$journalPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\DiagnosticsMutationJournal.cs'
$mainPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.MutationJournal.cs'
$testPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\DiagnosticsMutationJournalTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$journal = Get-Content -LiteralPath $journalPath -Raw
$main = Get-Content -LiteralPath $mainPath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing ${Label}: $Needle"
    }
}
Require-Text $journal 'private const int TypedSdoFormatVersion = 2;' 'v2 compatibility'
Require-Text $journal 'private const int FormatVersion = 3;' 'v3 writer'
Require-Text $journal 'HasFullDurableIdentity' 'full identity marker'
Require-Text $journal 'EndpointIp' 'endpoint identity'
Require-Text $journal 'DiagnosticsBuild' 'build identity'
Require-Text $journal 'version != TypedSdoFormatVersion' 'v2 reader support'
Require-Text $main 'durableEndpointIp = RequiredConnectedRemoteIp()' 'arm endpoint capture'
Require-Text $main 'capabilities.DiagnosticsBuild' 'arm build capture'
Require-Text $main 'RequiredConnectedRemotePort()' 'restart endpoint capture'
Require-Text $tests 'LegacyV2TypedRecoveryIsZeroWire' 'legacy v2 zero-wire test'
Require-Text $tests 'RestartRecoveryEndpointMismatchDoesNotRead' 'endpoint mismatch test'
Require-Text $tests 'RestartRecoveryBuildMismatchDoesNotRead' 'build mismatch test'
Require-Text $design 'R05-B에서는 journal format을 v3' 'R05-B design sync'
Write-Host 'PASS SDO-R05-B full durable identity source contract.'

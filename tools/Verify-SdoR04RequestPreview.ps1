param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$xamlPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.xaml'
$sourcePath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp\MainWindow.Diagnostics.cs'
$testPath = Join-Path $root 'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\WpfMainWindowIntegrationTests.cs'
$designPath = Join-Path $root 'docs\api\design\SDO_OPERATION_MODE_REIMPLEMENTATION_PLAN_20260827.md'
$xaml = Get-Content -LiteralPath $xamlPath -Raw
$source = Get-Content -LiteralPath $sourcePath -Raw
$tests = Get-Content -LiteralPath $testPath -Raw
$design = Get-Content -LiteralPath $designPath -Raw
function Require-Text([string]$Text, [string]$Needle, [string]$Label) {
    if ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        throw "Missing ${Label}: $Needle"
    }
}
Require-Text $xaml 'x:Name="TextSdoRequestPreview"' 'preview surface'
Require-Text $xaml 'x:Name="TextSdoSemanticWarning"' 'semantic warning surface'
Require-Text $xaml 'Text="Exact request preview"' 'preview label'
Require-Text $source 'private void UpdateSdoRequestPreview()' 'preview updater'
Require-Text $source 'private static string FormatSdoExactRequestPreview(' 'exact formatter'
Require-Text $source 'BLOCKED RESERVED SDO WRITE | NOT SUBMITTED' 'zero-wire warning text'
if ([regex]::Matches($source, 'LMCDiagnosticsWritePolicy\.RequireSdoWriteAllowed\(request\);').Count -lt 2) {
    throw 'Preview and submission paths must both enforce the SDK write policy.'
}
Require-Text $tests 'WriteData=34-12' 'little-endian exact preview smoke'
Require-Text $tests 'BLOCKED RESERVED SDO WRITE' 'reserved warning smoke'
Require-Text $design '- [x] exact request preview' 'R04 preview checklist'
Require-Text $design '- [x] semantic reserved warning' 'R04 warning checklist'
Write-Host 'PASS SDO-R04 exact request preview and semantic-reserved warning source contract.'

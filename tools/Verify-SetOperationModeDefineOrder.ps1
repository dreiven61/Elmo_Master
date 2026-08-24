[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$diagnosticsPath = Join-Path $repoRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'

if (-not (Test-Path -LiteralPath $diagnosticsPath -PathType Leaf)) {
    throw "Missing diagnostics source: $diagnosticsPath"
}

$text = [System.IO.File]::ReadAllText($diagnosticsPath).Replace("`r`n", "`n").Replace("`r", "`n")
$implementationMarker = '//{{LSL_IMPLEMENTATION'
$implementationIndex = $text.IndexOf($implementationMarker, [System.StringComparison]::Ordinal)
if ($implementationIndex -lt 0) {
    throw 'Unable to locate //{{LSL_IMPLEMENTATION marker.'
}

$implementation = $text.Substring($implementationIndex)
$firstFunction = [regex]::Match($implementation, '(?m)^FUNCTION(?:[\t ]+(?:GLOBAL|VIRTUAL))*[\t ]+LMCDiagnosticsService::')
if (-not $firstFunction.Success) {
    throw 'Unable to locate the first LMCDiagnosticsService user implementation function.'
}

$modeDefines = [regex]::Matches($implementation, '(?m)^#define[\t ]+LMC_DIAG_MODE_[A-Z0-9_]+[\t ]+[^\r\n]+$')
if ($modeDefines.Count -lt 50) {
    throw "Unexpected SetOperationMode define set: found $($modeDefines.Count), expected at least 50."
}

$lateDefines = @($modeDefines | Where-Object { $_.Index -gt $firstFunction.Index })
if ($lateDefines.Count -ne 0) {
    $names = $lateDefines | ForEach-Object { $_.Value.Trim() }
    throw ("SetOperationMode #define declarations must precede the first user implementation function. Late declarations:`n - " + ($names -join "`n - "))
}

$required = [ordered]@{
    'LMC_DIAG_MODE_RECORD_STRIDE' = '32'
    'LMC_DIAG_MODE_RECORD_MAGIC' = '0x4D4F4445'
    'LMC_DIAG_MODE_RUNTIME_BASE' = '128'
    'LMC_DIAG_MODE_META_RECORD_GENERATION' = '160'
    'LMC_DIAG_MODE_STAGE_PREFLIGHT_START' = '1'
    'LMC_DIAG_MODE_STAGE_WRITE_START' = '3'
    'LMC_DIAG_MODE_STAGE_VERIFY_START' = '5'
    'LMC_DIAG_MODE_STAGE_RECOVERY_START' = '11'
    'LMC_DIAG_MODE_OWNER_KIND' = '6'
    'LMC_DIAG_MODE_RESOURCE_KIND' = '4'
    'LMC_DIAG_MODE_ADMISSION_MODE' = '4'
    'LMC_DIAG_MODE_DETAIL_UNSUPPORTED' = '43'
    'LMC_DIAG_MODE_DETAIL_SLOT_OCCUPIED' = '51'
    'LMC_DIAG_MODE_QUARANTINE_SAFETY_PREEMPT' = '7'
}

foreach ($entry in $required.GetEnumerator()) {
    $pattern = '(?m)^#define[\t ]+' + [regex]::Escape($entry.Key) + '[\t ]+' + [regex]::Escape($entry.Value) + '[\t ]*$'
    $matches = [regex]::Matches($implementation, $pattern)
    if ($matches.Count -ne 1) {
        throw "Expected exactly one '$($entry.Key) $($entry.Value)' declaration; found $($matches.Count)."
    }
}

$modeBlockStart = $modeDefines[0].Index
$modeBlockEnd = $modeDefines[$modeDefines.Count - 1].Index + $modeDefines[$modeDefines.Count - 1].Length
$between = $implementation.Substring($modeBlockStart, $modeBlockEnd - $modeBlockStart)
$nonModeDefine = [regex]::Match($between, '(?m)^#define[\t ]+(?!LMC_DIAG_MODE_)[A-Z0-9_]+')
if ($nonModeDefine.Success) {
    throw "SetOperationMode define block is no longer contiguous near: $($nonModeDefine.Value.Trim())"
}

Write-Host "PASS SetOperationMode define count: $($modeDefines.Count)"
Write-Host 'PASS all SetOperationMode defines precede the first user implementation function'
Write-Host 'PASS required SetOperationMode ABI constants retain frozen values'
Write-Host 'PASS SetOperationMode define block remains contiguous'
exit 0

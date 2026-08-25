[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $RepositoryRoot = Split-Path -Parent $scriptDirectory
}

$sourcePath = Join-Path $RepositoryRoot 'tools\Apply-HomeDs402ExHomeex07Ownership.ps1'
$tempPath = Join-Path $env:RUNNER_TEMP 'Apply-HomeDs402ExHomeex07Ownership.robust.ps1'
$text = [System.IO.File]::ReadAllText($sourcePath).Replace("`r`n", "`n").Replace("`r", "`n")

function Replace-ScriptBlockOnce {
    param([string]$InputText, [string]$Pattern, [string]$Replacement, [string]$Label)
    $options = [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline
    $regex = [regex]::new($Pattern, $options)
    $count = $regex.Matches($InputText).Count
    if ($count -ne 1) {
        throw "HOMEEX-07 bootstrap refused: '$Label' expected one transform-script block, found $count"
    }
    Write-Host "PASS bootstrap anchor: $Label"
    return $regex.Replace($InputText, [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $Replacement })
}

function Replace-LiteralOnce {
    param([string]$InputText, [string]$Old, [string]$New, [string]$Label)
    $count = ([regex]::Matches($InputText, [regex]::Escape($Old))).Count
    if ($count -ne 1) {
        throw "HOMEEX-07 bootstrap refused: '$Label' expected one literal block, found $count"
    }
    Write-Host "PASS bootstrap anchor: $Label"
    return $InputText.Replace($Old, $New)
}

function Scope-DiagnosticsReplaceOnce {
    param([string]$InputText, [string]$Label)
    $labelToken = "'" + $Label + "'"
    $labelIndex = $InputText.IndexOf($labelToken)
    if ($labelIndex -lt 0) {
        throw "HOMEEX-07 bootstrap refused: diagnostics label not found: $Label"
    }
    $oldPrefix = '$diagnostics = Replace-Once $diagnostics'
    $prefixIndex = $InputText.Substring(0, $labelIndex).LastIndexOf($oldPrefix)
    if ($prefixIndex -lt 0) {
        throw "HOMEEX-07 bootstrap refused: diagnostics Replace-Once not found before: $Label"
    }
    $between = $InputText.Substring($prefixIndex, $labelIndex - $prefixIndex)
    if (([regex]::Matches($between, [regex]::Escape($oldPrefix))).Count -ne 1) {
        throw "HOMEEX-07 bootstrap refused: ambiguous diagnostics Replace-Once before: $Label"
    }
    Write-Host "PASS bootstrap scope: $Label"
    return $InputText.Remove($prefixIndex, $oldPrefix.Length).Insert(
        $prefixIndex, '$homeExStart = Replace-Once $homeExStart')
}

$text = Replace-ScriptBlockOnce $text `
    '\$control = Replace-Once \$control @''\n#define LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE 12\n#define LMC_OWNER_KIND_DIRECT 1\n''@ @''\n#define LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE 12\n#define LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE 13\n#define LMC_OWNER_KIND_DIRECT 1\n''@ ''reserve active state 13''' `
    @'
$control = Replace-RegexCount $control '^#define LMC_OWNER_STATE_AXIS_OPERATION_MODE_ACTIVE 12$' 1 {
    param($m)
    $m.Value + "`n#define LMC_OWNER_STATE_DS402_HOME_EX_ACTIVE 13"
} 'reserve active state 13'
'@ `
    'state 13 insertion'

$text = Replace-ScriptBlockOnce $text `
    '\$control = Replace-Once \$control @''\n#define LMC_OWNER_KIND_AXIS_OPERATION_MODE        6\n#define LMC_OWNER_RESOURCE_AXIS 1\n''@ @''\n#define LMC_OWNER_KIND_AXIS_OPERATION_MODE        6\n#define LMC_OWNER_KIND_DS402_HOME_EX 7\n#define LMC_OWNER_RESOURCE_AXIS 1\n''@ ''define OwnerKind 7''' `
    @'
$control = Replace-RegexCount $control '^#define LMC_OWNER_KIND_AXIS_OPERATION_MODE[ \t]+6$' 1 {
    param($m)
    $m.Value + "`n#define LMC_OWNER_KIND_DS402_HOME_EX 7"
} 'define OwnerKind 7'
'@ `
    'OwnerKind 7 insertion'

$text = Replace-ScriptBlockOnce $text `
    '\$control = Replace-Once \$control @''\n#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040\n#define LMC_OWNER_IDENTITY_SUFFIX_DINTS 314\n''@ @''\n#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040\n#define LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES 52\n#define LMC_OWNER_IDENTITY_SUFFIX_DINTS 314\n''@ ''define 52-byte per-axis tail slot''' `
    @'
$control = Replace-RegexCount $control '^#define LMC_OWNER_IDENTITY_PREFIX_BYTES 0x00000040$' 1 {
    param($m)
    $m.Value + "`n#define LMC_OWNER_IDENTITY_AXIS_TAIL_BYTES 52"
} 'define 52-byte per-axis tail slot'
'@ `
    'axis tail slot insertion'

$oldOwnerBranch = @'
$control = Replace-RegexCount $control '^(?<indent>[ \t]*)LMC_OWNER_KIND_AXIS_OPERATION_MODE:[ \t]*\n(?<body>.*?)(?=^\k<indent>else\b)' 10 {
'@
$newOwnerBranch = @'
$control = Replace-RegexCount $control '^(?<indent>[ \t]*)LMC_OWNER_KIND_AXIS_OPERATION_MODE:[ \t]*\n(?<body>.*?)(?=^[ \t]*else\b)' 10 {
'@
$text = Replace-LiteralOnce $text $oldOwnerBranch $newOwnerBranch `
    'match parent else for all owner-kind branches'

# Scope the three HomeDS402Ex Start edits to the Start function. Outcome and
# Retire intentionally share local/response text and must remain untouched.
$firstLabelToken = "'add HomeDS402Ex ownership validation locals'"
$firstLabelIndex = $text.IndexOf($firstLabelToken)
if ($firstLabelIndex -lt 0) {
    throw 'HOMEEX-07 bootstrap refused: Start-local transform label missing'
}
$globalPrefix = '$diagnostics = Replace-Once $diagnostics'
$firstPrefixIndex = $text.Substring(0, $firstLabelIndex).LastIndexOf($globalPrefix)
if ($firstPrefixIndex -lt 0) {
    throw 'HOMEEX-07 bootstrap refused: Start-local transform prefix missing'
}
$extractStart = @'
$homeExStartRegex = [regex]::new('FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeExStart(?s).*?END_FUNCTION')
$homeExStartMatches = $homeExStartRegex.Matches($diagnostics)
if ($homeExStartMatches.Count -ne 1) {
    throw "HOMEEX-07 transform refused: expected exactly one HomeDS402Ex Start function, found $($homeExStartMatches.Count)"
}
$homeExStartOriginal = $homeExStartMatches[0].Value
$homeExStart = $homeExStartOriginal

'@
$text = $text.Insert($firstPrefixIndex, $extractStart)
Write-Host 'PASS bootstrap scope: extracted exact HomeDS402Ex Start function'

foreach ($label in @(
    'add HomeDS402Ex ownership validation locals',
    'validate exact HomeDS402Ex reservation before deterministic gate-OFF failure',
    'rollback HomeDS402Ex reservation on deterministic gate-OFF rejection')) {
    $text = Scope-DiagnosticsReplaceOnce $text $label
}

$rollbackLabelToken = "'rollback HomeDS402Ex reservation on deterministic gate-OFF rejection'"
$rollbackLabelIndex = $text.IndexOf($rollbackLabelToken)
if ($rollbackLabelIndex -lt 0) {
    throw 'HOMEEX-07 bootstrap refused: rollback transform label missing after scoping'
}
$rollbackLineEnd = $text.IndexOf("`n", $rollbackLabelIndex)
if ($rollbackLineEnd -lt 0) {
    throw 'HOMEEX-07 bootstrap refused: rollback transform line terminator missing'
}
$replaceStart = @'

$diagnostics = Replace-Once $diagnostics $homeExStartOriginal $homeExStart `
    'replace HomeDS402Ex Start function after ownership edits'
'@
$text = $text.Insert($rollbackLineEnd + 1, $replaceStart)
Write-Host 'PASS bootstrap scope: scheduled exact Start-function reinsertion'

[System.IO.File]::WriteAllText($tempPath, $text, [System.Text.UTF8Encoding]::new($false))
& $tempPath -RepositoryRoot $RepositoryRoot
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

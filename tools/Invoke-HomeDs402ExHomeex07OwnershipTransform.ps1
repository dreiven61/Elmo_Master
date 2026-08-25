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
$control = Replace-RegexCount $control '^(?<indent>[ \t]*)LMC_OWNER_KIND_AXIS_OPERATION_MODE:[ \t]*\n(?<body>.*?)(?=^\k<indent>(?:else\b|end_case;))' 10 {
'@
$text = Replace-LiteralOnce $text $oldOwnerBranch $newOwnerBranch `
    'allow owner-kind branch to terminate at else or end_case'

# Diagnostic only: print bounded source context for all ten current labels before
# the fail-closed transform runs. This does not write tracked LASAL source.
$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$controlSource = [System.IO.File]::ReadAllText($controlPath).Replace("`r`n", "`n").Replace("`r", "`n")
$labels = [regex]::Matches($controlSource, '(?m)^(?<indent>[ \t]*)LMC_OWNER_KIND_AXIS_OPERATION_MODE:[ \t]*$')
Write-Host ("OWNER_KIND_CONTEXT_COUNT={0}" -f $labels.Count)
$contextIndex = 0
foreach ($labelMatch in $labels) {
    $contextIndex++
    $lineNumber = 1 + ([regex]::Matches($controlSource.Substring(0, $labelMatch.Index), "`n")).Count
    $remaining = $controlSource.Substring($labelMatch.Index)
    $take = [Math]::Min(900, $remaining.Length)
    $snippet = $remaining.Substring(0, $take)
    $snippet = $snippet.Replace("`n", ' <NL> ')
    Write-Host ("OWNER_KIND_CONTEXT_{0}_LINE={1}" -f $contextIndex, $lineNumber)
    Write-Host ("OWNER_KIND_CONTEXT_{0}={1}" -f $contextIndex, $snippet)
}

[System.IO.File]::WriteAllText($tempPath, $text, [System.Text.UTF8Encoding]::new($false))
& $tempPath -RepositoryRoot $RepositoryRoot
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$sourcePath = Join-Path $RepositoryRoot 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
if (-not (Test-Path -LiteralPath $sourcePath)) { throw "Missing LMCDiagnosticsService source: $sourcePath" }
$source = [IO.File]::ReadAllText($sourcePath).Replace("`r`n", "`n")
$checks = 0

function Assert-Match([string]$Text, [string]$Pattern, [string]$Message) {
    if (-not [regex]::IsMatch($Text, $Pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
        throw $Message
    }
    $script:checks++
}

function Assert-NotMatch([string]$Text, [string]$Pattern, [string]$Message) {
    if ([regex]::IsMatch($Text, $Pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
        throw $Message
    }
    $script:checks++
}

function Get-FunctionBlock([string]$Name) {
    $pattern = '(?ms)^FUNCTION\s+LMCDiagnosticsService::' + [regex]::Escape($Name) + '\b.*?^END_FUNCTION\s*$'
    $match = [regex]::Match($source, $pattern)
    if (-not $match.Success) { throw "Missing function $Name" }
    return $match.Value
}

Assert-Match $source '#define\s+LMC_DIAG_DS402_HOME_EX_ENABLED\s+FALSE' 'HOMEEX runtime gate must remain FALSE.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RECORD_STRIDE\s+40' 'HOMEEX active record stride must remain 40 DINTs.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_BASE\s+160' 'HOMEEX retired tombstones must start after four 40-DINT active records.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_STRIDE\s+24' 'HOMEEX retired tombstone stride must remain 24 DINTs.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_MAGIC\s+0x48585254' 'HOMEEX retired tombstone magic drifted.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_CLEANUP_PROOF_REQUIRED\s+0x0000003F' 'HOMEEX six-bit cleanup proof mask drifted.'

$start = Get-FunctionBlock 'HandleAxisDs402HomeExStart'
Assert-Match $start 'recordState\s*:=\s*Ds402HomeExState\[recordBase\]\s*;' 'Start must inspect the retained per-axis active record state.'
Assert-Match $start 'recordValid\s*:=.*?recordState\s*>=\s*1.*?recordState\s*<=\s*4.*?Ds402HomeExState\[recordBase\s*\+\s*37\].*?<>\s*0' 'Start must distinguish a structurally valid retained active record from corruption.'
Assert-Match $start 'if\s+recordValid\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_SLOT_OCCUPIED' 'A valid retained active record must block a new Start as slot occupied.'
Assert-Match $start 'tombstoneBase\s*:=\s*LMC_DIAG_HOMEEX_RETIRED_BASE\s*\+.*?LMC_DIAG_HOMEEX_RETIRED_STRIDE' 'Start must address the per-axis retired tombstone.'
Assert-Match $start 'retiredDuplicate\s*:=.*?LMC_DIAG_HOMEEX_RETIRED_MAGIC.*?requestId.*?intent0.*?homingMethod' 'Start must compare an old retired identity before allowing reuse.'
Assert-Match $start 'if\s+retiredDuplicate\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_SLOT_OCCUPIED' 'Exact old Start replay must remain blocked after retire.'
Assert-Match $start 'elsif\s+LMC_DIAG_DS402_HOME_EX_ENABLED\s*=\s*FALSE\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE' 'HOMEEX Start must remain fail-closed while runtime is disabled.'

$outcome = Get-FunctionBlock 'HandleAxisDs402HomeExOutcome'
Assert-Match $outcome 'if\s+recordState\s*=\s*0\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND' 'Outcome must report not-found only for an empty active slot.'
Assert-Match $outcome 'keyMatches\s*:=.*?diagnosticsBuild.*?originalRequestId.*?intent3.*?detectionTimeout' 'Outcome must compare the full exact recovery key.'
Assert-Match $outcome 'elsif\s+keyMatches\s*=\s*FALSE\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_KEY_MISMATCH' 'Outcome must fail closed on an exact-key mismatch.'
Assert-Match $outcome 'ResponseCapacity\s*<\s*176' 'Outcome must guard the fixed 176-byte response capacity.'
Assert-Match $outcome '\(pResponse\s*\+\s*16\)\^\$UINT\s*:=\s*TO_UINT\(recordState\)' 'Outcome must serialize RecordState from the retained record.'
Assert-Match $outcome '\(pResponse\s*\+\s*164\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*37\]\$UDINT' 'Outcome must serialize the exact retained record generation.'
Assert-Match $outcome '\(pResponse\s*\+\s*168\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*38\]\$UDINT' 'Outcome must serialize cleanup proof flags.'
Assert-Match $outcome '\(pResponse\s*\+\s*172\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*39\]\$UDINT' 'Outcome must serialize the SDO executor token.'
Assert-Match $outcome 'ResponseSize\s*:=\s*176\s*;' 'Outcome success must remain exactly 176 bytes.'

$retire = Get-FunctionBlock 'HandleAxisDs402HomeExRetire'
Assert-Match $retire 'if\s+recordState\s*=\s*1\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_INDETERMINATE' 'Retire must never clear a Running record.'
Assert-Match $retire 'expectedGeneration\s*<>\s*Ds402HomeExState\[recordBase\s*\+\s*37\]\$UDINT' 'Retire must require the exact nonzero record generation.'
Assert-Match $retire 'Ds402HomeExState\[tombstoneBase\]\s*:=\s*TO_DINT\(LMC_DIAG_HOMEEX_RETIRED_MAGIC\)' 'Retire must publish a tombstone before clearing the active record.'
Assert-Match $retire 'while\s+recordIndex\s*<=\s*20\s+do.*?Ds402HomeExState\[tombstoneBase\s*\+\s*recordIndex\]\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*recordIndex\]' 'Retire must persist the full exact key into the tombstone.'
Assert-Match $retire 'Ds402HomeExState\[tombstoneBase\s*\+\s*21\]\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*37\]' 'Retire must persist the exact record generation.'
Assert-Match $retire 'while\s+recordIndex\s*<\s*LMC_DIAG_HOMEEX_RECORD_STRIDE\s+do\s*\n\s*Ds402HomeExState\[recordBase\s*\+\s*recordIndex\]\s*:=\s*0' 'Retire must clear the active record only after tombstone publication.'
Assert-Match $retire 'tombstoneMatches\s*:=.*?LMC_DIAG_HOMEEX_RETIRED_MAGIC.*?expectedGeneration' 'An empty active slot must compare the retired tombstone for idempotent retry.'
Assert-Match $retire 'if\s+tombstoneMatches\s+then\s*\n\s*\(pResponse\s*\+\s*4\)\^\$UINT\s*:=\s*0' 'Exact retired-generation retry must be idempotent success.'

$process = Get-FunctionBlock 'ProcessAxisDs402HomeEx'
Assert-Match $process 'SDO, RT mailbox, controlword,\s*\n\s*// mode, setpoint and motion execution remain forbidden' 'HOMEEX physical runtime boundary comment drifted.'
Assert-NotMatch $process '6060|6061|6098|607C|6099|609A|controlword|Write' 'HOMEEX retained-store work must not introduce physical runtime operations.'

# Pure state-model checks for the intended duplicate/retire contract. These do not
# emulate LASAL execution; they protect the no-replay state transitions encoded above.
function Classify-Start([bool]$ActiveValid, [bool]$ActiveDirty, [bool]$RetiredExact, [bool]$RuntimeEnabled) {
    if ($ActiveDirty) { return 55 }
    if ($ActiveValid) { return 60 }
    if ($RetiredExact) { return 60 }
    if (-not $RuntimeEnabled) { return 61 }
    return 57
}
if ((Classify-Start $true $false $false $false) -ne 60) { throw 'Active retained record must block duplicate Start.' }
$checks++
if ((Classify-Start $false $false $true $false) -ne 60) { throw 'Retired exact identity must block old Start replay.' }
$checks++
if ((Classify-Start $false $false $false $false) -ne 61) { throw 'Runtime-disabled empty slot must remain fail-closed.' }
$checks++
if ((Classify-Start $false $true $false $false) -ne 55) { throw 'Corrupt active slot must remain fail-closed.' }
$checks++

Write-Host "HomeDS402Ex retained-store verifier: $checks checks PASS"

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

Assert-Match $source 'Ds402HomeExState\s*:\s*ARRAY\s*\[0\.\.319\]\s*OF\s*DINT' 'HOMEEX retained state bank must fit four active and four full retired records.'
Assert-Match $source '#define\s+LMC_DIAG_DS402_HOME_EX_ENABLED\s+FALSE' 'HOMEEX runtime gate must remain FALSE.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RECORD_STRIDE\s+40' 'HOMEEX active record stride must remain 40 DINTs.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_BASE\s+160' 'HOMEEX retired records must start after four 40-DINT active records.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_STRIDE\s+40' 'HOMEEX retired record must retain the complete 40-DINT outcome.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_RETIRED_STATE_MASK\s+0x00008000' 'HOMEEX retired record state marker drifted.'
Assert-Match $source '#define\s+LMC_DIAG_HOMEEX_CLEANUP_PROOF_REQUIRED\s+0x0000003F' 'HOMEEX six-bit cleanup proof mask drifted.'
Assert-NotMatch $source 'LMC_DIAG_HOMEEX_RETIRED_MAGIC' 'Obsolete lossy 24-DINT tombstone magic must be removed.'

$start = Get-FunctionBlock 'HandleAxisDs402HomeExStart'
Assert-Match $start 'recordState\s*:=\s*Ds402HomeExState\[recordBase\]\s*;' 'Start must inspect the retained per-axis active record state.'
Assert-Match $start 'recordValid\s*:=.*?recordState\s*>=\s*1.*?recordState\s*<=\s*4.*?Ds402HomeExState\[recordBase\s*\+\s*37\].*?<>\s*0' 'Start must distinguish a structurally valid active record from corruption.'
Assert-Match $start 'if\s+recordState\s*=\s*1\s+then.*?recordBase\s*\+\s*35.*?=\s*0.*?else.*?recordBase\s*\+\s*35.*?>=.*?recordBase\s*\+\s*34' 'Start must validate running versus terminal cycle ordering.'
Assert-Match $start 'if\s+recordDirty\s+then.*?recordValid\s*=\s*FALSE.*?LMC_DIAG_HOMEEX_DETAIL_STORE_CORRUPT.*?LMC_DIAG_HOMEEX_DETAIL_SLOT_OCCUPIED' 'A valid active record must block a new Start while corrupt state fails closed.'
Assert-Match $start 'tombstoneBase\s*:=\s*LMC_DIAG_HOMEEX_RETIRED_BASE\s*\+.*?LMC_DIAG_HOMEEX_RETIRED_STRIDE' 'Start must address the per-axis retired record.'
Assert-Match $start 'while\s+recordIndex\s*<\s*LMC_DIAG_HOMEEX_RETIRED_STRIDE.*?tombstoneDirty' 'Start must distinguish a truly empty retired record from a dirty partial record.'
Assert-Match $start 'rawRetiredState.*?0x00008002.*?0x00008003.*?0x00008004' 'Start must accept only retired terminal state markers.'
Assert-Match $start 'retiredDuplicate\s*:=.*?requestId.*?intent0.*?homingMethod.*?detectionTimeout' 'Start must compare the complete v1 Start identity before allowing reuse.'
Assert-Match $start 'if\s+retiredDuplicate\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_SLOT_OCCUPIED' 'Exact retired Start replay must remain blocked.'
Assert-Match $start 'elsif\s+LMC_DIAG_DS402_HOME_EX_ENABLED\s*=\s*FALSE\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_INVALID_PROFILE' 'HOMEEX Start must remain fail-closed while runtime is disabled.'

$outcome = Get-FunctionBlock 'HandleAxisDs402HomeExOutcome'
Assert-Match $outcome 'if\s+recordState\s*=\s*0\s+then\s*\n\s*detailCode\s*:=\s*LMC_DIAG_HOMEEX_DETAIL_NOT_FOUND' 'Outcome must report not-found for an empty active state.'
Assert-Match $outcome 'keyMatches\s*:=.*?diagnosticsBuild.*?originalRequestId.*?intent3.*?detectionTimeout' 'Outcome must compare the full exact recovery key.'
Assert-Match $outcome 'keyMatches\s*=\s*FALSE.*?LMC_DIAG_HOMEEX_DETAIL_KEY_MISMATCH' 'Outcome must fail closed on an exact-key mismatch.'
Assert-Match $outcome 'recordState\s*=\s*1.*?recordBase\s*\+\s*35.*?=\s*0.*?recordBase\s*\+\s*38.*?=\s*0' 'Running Outcome must have zero completion and zero cleanup proof.'
Assert-Match $outcome 'recordBase\s*\+\s*35.*?>=.*?recordBase\s*\+\s*34.*?recordBase\s*\+\s*38.*?LMC_DIAG_HOMEEX_CLEANUP_PROOF_REQUIRED.*?recordBase\s*\+\s*39.*?<>\s*0' 'Terminal Outcome must have ordered cycles, exact cleanup proof and an SDO executor token.'
Assert-Match $outcome 'ResponseCapacity\s*<\s*176' 'Outcome must guard the fixed 176-byte response capacity.'
Assert-Match $outcome '\(pResponse\s*\+\s*16\)\^\$UINT\s*:=\s*TO_UINT\(recordState\)' 'Outcome must serialize RecordState from the active record.'
Assert-Match $outcome '\(pResponse\s*\+\s*164\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*37\]\$UDINT' 'Outcome must serialize the exact record generation.'
Assert-Match $outcome '\(pResponse\s*\+\s*168\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*38\]\$UDINT' 'Outcome must serialize cleanup proof flags.'
Assert-Match $outcome '\(pResponse\s*\+\s*172\)\^\$UDINT\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*39\]\$UDINT' 'Outcome must serialize the SDO executor token.'
Assert-Match $outcome 'ResponseSize\s*:=\s*176\s*;' 'Outcome success must remain exactly 176 bytes.'

$retire = Get-FunctionBlock 'HandleAxisDs402HomeExRetire'
Assert-Match $retire 'recordState\s*=\s*1.*?LMC_DIAG_HOMEEX_DETAIL_INDETERMINATE' 'Retire must never clear a Running record.'
Assert-Match $retire 'expectedGeneration\s*<>\s*Ds402HomeExState\[recordBase\s*\+\s*37\]\$UDINT' 'Retire must require the exact nonzero active generation.'
Assert-Match $retire 'ResponseCapacity\s*<\s*176' 'Retire must not mutate state unless a full 176-byte success can be returned.'
Assert-Match $retire 'recordIndex\s*:=\s*1.*?recordIndex\s*<\s*LMC_DIAG_HOMEEX_RETIRED_STRIDE.*?Ds402HomeExState\[tombstoneBase\s*\+\s*recordIndex\]\s*:=\s*Ds402HomeExState\[recordBase\s*\+\s*recordIndex\]' 'Retire must preserve the full outcome body in the retired record.'
Assert-Match $retire 'Ds402HomeExState\[tombstoneBase\]\s*:=\s*TO_DINT\(TO_UDINT\(recordState\)\s+or\s+LMC_DIAG_HOMEEX_RETIRED_STATE_MASK\)' 'Retire must publish the retired terminal state marker after copying the full outcome.'
Assert-Match $retire 'while\s+recordIndex\s*<\s*LMC_DIAG_HOMEEX_RECORD_STRIDE.*?Ds402HomeExState\[recordBase\s*\+\s*recordIndex\]\s*:=\s*0' 'Retire must clear the active record only after retired-record publication.'
Assert-Match $retire 'rawRetiredState.*?retiredState.*?expectedGeneration' 'An empty active slot must validate the full retired record for exact-generation retry.'
Assert-Match $retire 'responseBase\s*:=\s*tombstoneBase' 'Retire retry and first retirement must serialize from the retained full retired outcome.'
Assert-Match $retire '\(pResponse\s*\+\s*16\)\^\$UINT\s*:=\s*TO_UINT\(retiredState\)' 'Retire success must return the normalized terminal record state.'
Assert-Match $retire '\(pResponse\s*\+\s*164\)\^\$UDINT\s*:=\s*Ds402HomeExState\[responseBase\s*\+\s*37\]\$UDINT' 'Retire success must echo the exact retired generation.'
Assert-Match $retire '\(pResponse\s*\+\s*172\)\^\$UDINT\s*:=\s*Ds402HomeExState\[responseBase\s*\+\s*39\]\$UDINT' 'Retire success must echo the retained SDO executor token.'
Assert-Match $retire 'ResponseSize\s*:=\s*176\s*;' 'Retire success must be exactly 176 bytes to match the SDK parser contract.'

$process = Get-FunctionBlock 'ProcessAxisDs402HomeEx'
Assert-Match $process 'SDO, RT mailbox, controlword,\s*\n\s*// mode, setpoint and motion execution remain forbidden' 'HOMEEX physical runtime boundary comment drifted.'
Assert-NotMatch $process '6060|6061|6098|607C|6099|609A|SdoAxis[1-4]\.|RequestSdo|DispatchSdo' 'HOMEEX retained-store work must not introduce physical SDO or mode operations.'

function Classify-Start([bool]$ActiveValid, [bool]$ActiveCorrupt, [bool]$RetiredExact, [bool]$RuntimeEnabled) {
    if ($ActiveCorrupt) { return 55 }
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

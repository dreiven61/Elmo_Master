[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $RepositoryRoot) {
    $RepositoryRoot = Join-Path $PSScriptRoot '../../../..'
}

$controlPath = Join-Path $RepositoryRoot 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$typesPath = Join-Path $RepositoryRoot 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/types.h'
$control = Get-Content -Raw -LiteralPath $controlPath
$types = Get-Content -Raw -LiteralPath $typesPath

$checks = 0
$failures = [Collections.Generic.List[string]]::new()
function Check([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $script:failures.Add($Name) }
}
function Method([string]$Name) {
    $match = [regex]::Match(
        $control,
        "(?ms)^FUNCTION (?:GLOBAL )?LMCControlCommandService::$Name\b.*?^END_FUNCTION")
    if (-not $match.Success) { throw "Missing method $Name" }
    return $match.Value
}

$process = Method 'ProcessAxisOwnership'
$status = Method 'GroupReadStatus'
$handler = Method 'HandleGroupCommands'

$enableObserver = [regex]::Match(
    $process,
    '(?is)0x2047\s*:(?<Body>.*?)(?=\s*0x2048\s*:)').Groups['Body'].Value
if ([string]::IsNullOrWhiteSpace($enableObserver)) {
    throw 'Missing 0x2047 ownership observer block.'
}
$motionObserver = [regex]::Match(
    $process,
    '(?is)0x20A4\s*,\s*0x7D22\s*:(?<Body>.*?)(?=\s*else\s*\r?\n\s*end_case;)').Groups['Body'].Value
if ([string]::IsNullOrWhiteSpace($motionObserver)) {
    throw 'Missing 0x20A4/0x7D22 motion observer block.'
}
$enableHandler = [regex]::Match(
    $handler,
    '(?is)0x2047\s*:(?<Body>.*?)(?=\s*0x2048\s*:)').Groups['Body'].Value
if ([string]::IsNullOrWhiteSpace($enableHandler)) {
    throw 'Missing 0x2047 Group Enable handler block.'
}

# Native LASAL semantics: LockState means all axes are locked; ProfileFinished
# means all motion sequences are completed and no buffered move remains.
Check ($types -match '(?is)_LMCPROF_LockState.*?All axis are locked') \
    'Generated LASAL type documents LockState as the profile-lock authority'
Check ($types -match '(?is)_LMCPROF_ProfileFinished.*?all motion sequences have been completed') \
    'Generated LASAL type documents ProfileFinished as motion completion'

Check ($enableHandler -match 'LMCRobot\.LockProfile\(\s*Profile:=0') \
    'Group Enable still reaches native LockProfile'
Check ($status -match 'ReadProfileParameter\(\s*ParNo:=_LMCPROF_LockState\)') \
    'GroupReadStatus reads the authoritative LockState'
Check ($status -match 'ProfileInPosition\(\s*Mode:=_LMCPROF_ProfileFinished\)') \
    'GroupReadStatus may still observe ProfileFinished for motion diagnostics'
Check ($status -match '(?is)if\s*\(powerIsOn\s*<>\s*0\)\s*&\s*\(profileLocked\s*=\s*TRUE\)\s*then\s*groupReadState\s*:=\s*groupReadState\s+or\s+0x00020000') \
    'Standby is asserted from PowerOn plus LockState'
Check (-not ($status -match '(?is)if\s*\(powerIsOn\s*<>\s*0\)\s*&\s*\(profileLocked\s*=\s*TRUE\)\s*&\s*\(groupReadInPosition\s*<>\s*0\)')) \
    'Standby no longer depends on ProfileFinished before the first Move'

Check ($enableObserver -match 'groupLockState\s*<>\s*0') \
    '0x2047 ownership retirement requires LockState'
Check ($enableObserver -match 'allStandstill') \
    '0x2047 ownership retirement requires standstill'
Check ($enableObserver -match 'allPowerOn') \
    '0x2047 ownership retirement requires power-on evidence'
Check ($enableObserver -match 'allErrorClear') \
    '0x2047 ownership retirement requires error-clear evidence'
Check ($enableObserver -match 'groupError\s*=\s*0') \
    '0x2047 ownership retirement requires no group error'
Check (-not ($enableObserver -match 'groupFinished')) \
    '0x2047 ownership retirement does not wait for ProfileFinished'

# Motion completion must remain stricter than lock completion.
Check ($motionObserver -match 'activitySeen') \
    'Motion ownership still requires observed activity before completion'
Check ($motionObserver -match 'groupFinished\s*<>\s*0') \
    'Motion ownership still requires ProfileFinished'
Check ($motionObserver -match 'allInPosition') \
    'Motion ownership still requires in-position evidence'

if ($failures.Count -gt 0) {
    Write-Host "FAIL GroupProfileLockCompletion: $($failures.Count)/$checks checks failed."
    foreach ($failure in $failures) { Write-Host "  - $failure" }
    exit 1
}

Write-Host "PASS GroupProfileLockCompletion: $checks/$checks source-contract checks."

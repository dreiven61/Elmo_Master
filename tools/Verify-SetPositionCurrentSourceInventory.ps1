param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$script:CheckCount = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "FAIL SetPosition SP-C0 inventory: $Message"
    }
    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True ([regex]::IsMatch($Text, $Pattern)) $Message
}

function Assert-NoMatch {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True (-not [regex]::IsMatch($Text, $Pattern)) $Message
}

function Read-SourceText {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    Assert-True (Test-Path -LiteralPath $path) "source exists: $RelativePath"
    return Get-Content -LiteralPath $path -Raw
}

$storePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSetPositionStore/LMCSetPositionStore.st'
$controlPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$tcpPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
$latchPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st'
$diagnosticsPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$journalPath = 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp/AxisSetPositionRecoveryJournal.cs'
$journalTestsPath = 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp.SmokeTests/AxisSetPositionRecoveryJournalTests.cs'

$store = Read-SourceText $storePath
$control = Read-SourceText $controlPath
$tcp = Read-SourceText $tcpPath
$latch = Read-SourceText $latchPath
$diagnostics = Read-SourceText $diagnosticsPath
$journal = Read-SourceText $journalPath
$journalTests = Read-SourceText $journalTestsPath
$combinedLasal = [string]::Join("`n", @($store, $control, $tcp, $latch, $diagnostics))

# Frozen Store ABI / volatile backing that already exists.
Assert-Match $store 'g_LMCSetPositionStoreWords\s*:\s*ARRAY\s*\[0\.\.335\]\s*OF\s*UDINT' 'Store keeps the 336-UDINT / 1344-byte canonical ledger backing'
Assert-Match $store 'FUNCTION\s+GLOBAL\s+BeginSetPosition\b' 'Store BeginSetPosition exists'
Assert-Match $store 'FUNCTION\s+GLOBAL\s+CommitSetPositionTerminal\b' 'Store CommitSetPositionTerminal exists'
Assert-Match $store 'FUNCTION\s+GLOBAL\s+ReadSetPositionOutcome\b' 'Store ReadSetPositionOutcome exists'
Assert-Match $store 'FUNCTION\s+GLOBAL\s+RetireSetPositionOutcome\b' 'Store RetireSetPositionOutcome exists'
Assert-Match $store '<Client Name="CheckSum"' 'Store currently owns the CheckSum client required by the frozen ledger CRC contract'
Assert-NoMatch $store 'FileWrite_AV1|FileRead_AV1|GetAsyncState|CltChCmd__FileSys|<Client Name="FileSys"' 'Durable _FileSys backend is not silently hand-authored before SP-C1 IDE ABI evidence'

# P1 Control/TCP scaffold already exists and must be extended rather than replaced.
Assert-Match $control 'AxisSetPositionAsyncState\s*:\s*ARRAY' 'Control keeps cross-cycle AxisSetPositionAsyncState'
Assert-Match $control '<Client Name="SetPositionStore"' 'Control is already wired to LMCSetPositionStore'
Assert-Match $control 'HandleAdminSetPosition\b' 'Control HandleAdminSetPosition exists'
Assert-Match $control 'ProcessAdminSetPositionAsync\b' 'Control ProcessAdminSetPositionAsync exists'
Assert-Match $tcp 'HandleAdminSetPositionPending\b' 'TCP pending SetPosition handler exists'
Assert-Match ($control + "`n" + $tcp) '0x7D12' 'SetPosition Start command 0x7D12 is routed'
Assert-Match ($control + "`n" + $tcp) '0x7D14' 'SetPosition ReadOutcome command 0x7D14 is routed'
Assert-Match ($control + "`n" + $tcp) '0x7D1A' 'SetPosition Retire command 0x7D1A is routed'

# Freeze the production fail-closed boundary before the durable backend / RT executor exist.
Assert-Match $combinedLasal '(?m)^\s*#define\s+LMC_ADMIN_SET_POSITION_STORE_CONFIGURED\s+FALSE\s*$' 'SetPosition durable Store configured gate remains OFF'
$ordinaryOffCount = [regex]::Matches(
    $control + "`n" + $tcp,
    '(?m)^\s*#define\s+LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED\s+FALSE\s*$').Count
Assert-True ($ordinaryOffCount -ge 2) 'Control and TCP ordinary ownership activation gates remain OFF'
for ($axis = 1; $axis -le 4; $axis++) {
    # Backslash is not PowerShell's escape character. Keep regex metacharacters
    # single-escaped in this dynamically formatted pattern.
    $maxJumpPattern = (
        "(?im)^\s*#define\s+LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS_?{0}\s+0(?:\$[A-Z][A-Z0-9_]*)?\s*$" -f $axis)
    Assert-Match $combinedLasal $maxJumpPattern ("Axis{0} SetPositionMaxJump remains zero" -f $axis)
}

$processorMatch = [regex]::Match(
    $control,
    '(?s)FUNCTION\s+LMCControlCommandService::ProcessAdminSetPositionAsync\b.*?END_FUNCTION')
Assert-True $processorMatch.Success 'ProcessAdminSetPositionAsync source block can be isolated'
Assert-NoMatch $processorMatch.Value '\.SetPosition\s*\(' 'Current SetPosition async processor contains no authorized native SetPosition call before SP-C4'

# WPF journal model/tests exist, but MainWindow dispatch wiring is intentionally still missing at SP-C0.
Assert-Match $journal 'class\s+AxisSetPositionRecoveryJournal' 'WPF SetPosition recovery journal core exists'
Assert-Match $journalTests 'AxisSetPositionRecoveryJournal' 'WPF SetPosition recovery journal smoke tests exist'
$wpfDir = Join-Path $RepositoryRoot 'LMC_Library/LasalApiWpfTestApp/LasalApiWpfTestApp'
$mainWindowText = [string]::Join(
    "`n",
    @(Get-ChildItem -LiteralPath $wpfDir -Filter 'MainWindow*.cs' -File |
        ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }))
Assert-NoMatch $mainWindowText '\bAxisSetPositionRecoveryJournal\b' 'MainWindow has not prematurely wired SetPosition recovery before SP-C6'

# Preserve existing host deployment receipt tooling; image generation remains blocked on vendor CRC evidence.
foreach ($tool in @(
    'tools/LmcSetPositionStoreDeploymentReceipt.ps1',
    'tools/Start-LmcSetPositionStoreDeployment.ps1',
    'tools/Verify-LmcSetPositionStoreDeployment.ps1')) {
    Assert-True (Test-Path -LiteralPath (Join-Path $RepositoryRoot $tool)) "existing deployment tool is preserved: $tool"
}
Assert-True (-not (Test-Path -LiteralPath (Join-Path $RepositoryRoot 'tools/Generate-LmcSetPositionStoreImages.ps1'))) 'Factory A/B image generator remains blocked until vendor CRC golden fixtures are reviewed'

Write-Host ("SetPosition SP-C0 current source inventory PASS: {0} checks." -f $script:CheckCount)

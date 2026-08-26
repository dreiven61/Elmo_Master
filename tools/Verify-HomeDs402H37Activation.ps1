param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
$script:CheckCount = 0

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw "FAIL H37 activation verifier: $Message"
    }

    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Read-SourceText {
    param([string]$RelativePath)

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing source file: $RelativePath"
    }

    return Get-Content -LiteralPath $path -Raw
}

function Get-BooleanDefine {
    param(
        [string]$Text,
        [string]$Name,
        [string]$Scope
    )

    $pattern = '(?m)^\s*#define\s+' + [regex]::Escape($Name) + '\s+(TRUE|FALSE)\s*$'
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "$Scope defines $Name exactly once"
    return $matches[0].Groups[1].Value -eq 'TRUE'
}

function Get-OperationalCapabilityMask {
    param([string]$DiagnosticsText)

    $functionMatch = [regex]::Match(
        $DiagnosticsText,
        '(?s)FUNCTION\s+LMCDiagnosticsService::HandleDiagnosticsCapabilities\b.*?END_FUNCTION')
    Assert-True $functionMatch.Success 'HandleDiagnosticsCapabilities source block exists'

    $maskMatch = [regex]::Match(
        $functionMatch.Value,
        '(?s)if\s+CurrentDiagnosticsBootId\s+<>\s+0\s+then\s*\(pResponse\s*\+\s*20\)\^\$UDINT\s*:=\s*(0x[0-9A-Fa-f]+)\s*;')
    Assert-True $maskMatch.Success 'operational diagnostics capability mask assignment exists'

    return [Convert]::ToUInt32($maskMatch.Groups[1].Value.Substring(2), 16)
}

function Test-AtomicVector {
    param([bool[]]$Values)

    $trueCount = @($Values | Where-Object { $_ }).Count
    return ($trueCount -eq 0) -or ($trueCount -eq $Values.Count)
}

$tcpPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
$controlPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$diagnosticsPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$latchPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st'

$tcp = Read-SourceText $tcpPath
$control = Read-SourceText $controlPath
$diagnostics = Read-SourceText $diagnosticsPath
$latch = Read-SourceText $latchPath

$tcpOrdinary = Get-BooleanDefine $tcp 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED' 'TCPMotionInterface'
$controlOrdinary = Get-BooleanDefine $control 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED' 'LMCControlCommandService'
$homeRuntime = Get-BooleanDefine $diagnostics 'LMC_DIAG_DS402_HOME_ENABLED' 'LMCDiagnosticsService'
$startupSweep = Get-BooleanDefine $latch 'LMC_DS402_HOME_STARTUP_SWEEP_ENABLED' 'LMCEcatInputLatch'
$capabilityMask = Get-OperationalCapabilityMask $diagnostics
$homeCapability = ($capabilityMask -band 0x00000040) -ne 0

$nonHomeMask = $capabilityMask -band (-bnot [uint32]0x00000040)
Assert-True ($nonHomeMask -eq [uint32]0x0000613F) 'non-Home capability baseline is 0x0000613F'
Assert-True (($capabilityMask -eq [uint32]0x0000613F) -or ($capabilityMask -eq [uint32]0x0000617F)) 'operational capability mask is exact OFF/ON candidate'

$current = @(
    $tcpOrdinary,
    $controlOrdinary,
    $homeRuntime,
    $startupSweep,
    $homeCapability)
Assert-True (Test-AtomicVector $current) 'five tracked HomeDS402 activation values are all-OFF or all-ON'

$accepted = 0
$rejected = 0
for ($vector = 0; $vector -lt 32; $vector++) {
    $values = @()
    for ($bit = 0; $bit -lt 5; $bit++) {
        $values += (($vector -band (1 -shl $bit)) -ne 0)
    }

    $actual = Test-AtomicVector $values
    $expected = ($vector -eq 0) -or ($vector -eq 31)
    Assert-True ($actual -eq $expected) ("mixed-state truth table vector {0:D2} is {1}" -f $vector, $(if ($expected) { 'accepted' } else { 'rejected' }))
    if ($actual) {
        $accepted++
    }
    else {
        $rejected++
    }
}

Assert-True ($accepted -eq 2) 'truth table accepts exactly all-OFF and all-ON'
Assert-True ($rejected -eq 30) 'truth table rejects all 30 mixed activation states'

Write-Host ("H37 activation verifier PASS: {0} checks; capability mask 0x{1:X8}" -f $script:CheckCount, $capabilityMask)

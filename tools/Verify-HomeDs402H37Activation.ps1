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

function Get-AdminCapabilityMask {
    param([string]$ControlText)

    $maskMatch = [regex]::Match(
        $ControlText,
        '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*(0x[0-9A-Fa-f]+)\s*;')
    Assert-True $maskMatch.Success 'Admin capability mask assignment exists'
    return [Convert]::ToUInt32($maskMatch.Groups[1].Value.Substring(2), 16)
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
$lmcHomeRuntime = Get-BooleanDefine $control 'LMC_ADMIN_AXIS_HOME_ENABLED' 'LMCControlCommandService'
$homeRuntime = Get-BooleanDefine $diagnostics 'LMC_DIAG_DS402_HOME_ENABLED' 'LMCDiagnosticsService'
$startupSweep = Get-BooleanDefine $latch 'LMC_DS402_HOME_STARTUP_SWEEP_ENABLED' 'LMCEcatInputLatch'
$diagnosticCapabilityMask = Get-OperationalCapabilityMask $diagnostics
$adminCapabilityMask = Get-AdminCapabilityMask $control
$lmcHomeCapability = ($adminCapabilityMask -band 0x00000010) -ne 0
$ds402HomeCapability = ($adminCapabilityMask -band 0x00000040) -ne 0

Assert-True ($adminCapabilityMask -eq [uint32]0x00000757) 'Admin capability mask is the current feature-specific 0x00000757 set'
Assert-True $lmcHomeCapability 'Admin AxisHome capability bit 4 is ON'
Assert-True $ds402HomeCapability 'Admin AxisDs402Home capability bit 6 is ON'
Assert-True ($diagnosticCapabilityMask -eq [uint32]0x0000613F) 'Diagnostics capability mask remains 0x0000613F; its bit 6 is RecorderDoubleBank, not HomeDS402'
Assert-True ([regex]::IsMatch($control, '\(pResponseFrame\s*\+\s*36\)\^\$UINT\s*:=\s*1\s*;')) 'Admin PhysicalAxisCount matches the one-drive topology'
Assert-True (-not $tcpOrdinary) 'TCP ordinary ownership gate remains FALSE and is not a Home activation input'
Assert-True (-not $controlOrdinary) 'Control ordinary ownership gate remains FALSE and is not a Home activation input'
Assert-True $lmcHomeRuntime 'LMC Home runtime gate is ON'
Assert-True $homeRuntime 'DS402 Home runtime gate is ON'
Assert-True $startupSweep 'DS402 Home startup sweep is ON'

Write-Host ("Home feature-specific activation verifier PASS: {0} checks; Admin capability mask 0x{1:X8}" -f $script:CheckCount, $adminCapabilityMask)

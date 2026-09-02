param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$script:CheckCount = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "FAIL TOPO-C0 static contract: $Message"
    }
    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True ([regex]::IsMatch($Text, $Pattern)) $Message
}

function Assert-MatchCount {
    param([string]$Text, [string]$Pattern, [int]$Expected, [string]$Message)
    $actual = [regex]::Matches($Text, $Pattern).Count
    Assert-True ($actual -eq $Expected) "$Message (expected $Expected, found $actual)"
}

function Read-SourceText {
    param([string]$RelativePath)
    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing source file: $RelativePath"
    }
    return Get-Content -LiteralPath $path -Raw
}

function Get-HexDefine {
    param([string]$Text, [string]$Name)
    $pattern = '(?m)^\s*#define\s+' + [regex]::Escape($Name) + '\s+(0x[0-9A-Fa-f]+)\s*$'
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "$Name has one file-local hexadecimal definition"
    return [Convert]::ToUInt32($matches[0].Groups[1].Value.Substring(2), 16)
}

function Get-BoolDefine {
    param([string]$Text, [string]$Name)
    $pattern = '(?m)^\s*#define\s+' + [regex]::Escape($Name) + '\s+(TRUE|FALSE)\s*$'
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "$Name has one file-local Boolean definition"
    return $matches[0].Groups[1].Value -eq 'TRUE'
}

$simulationPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/SimulationSetup/SimulationSetup.st'
$networkPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn'
$networkTablePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st'
$latchPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st'
$controlPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$diagnosticsPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$tcpPath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'

$simulation = Read-SourceText $simulationPath
$network = Read-SourceText $networkPath
$networkTable = Read-SourceText $networkTablePath
$latch = Read-SourceText $latchPath
$control = Read-SourceText $controlPath
$diagnostics = Read-SourceText $diagnosticsPath
$tcp = Read-SourceText $tcpPath

# The three runtime layers must use the same two-physical-drive contract.
$latchMask = Get-HexDefine $latch 'LMC_CONFIGURED_PHYSICAL_DRIVE_MASK'
$controlMask = Get-HexDefine $control 'LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK'
$diagnosticsMask = Get-HexDefine $diagnostics 'LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK'
Assert-True ($latchMask -eq 0x00000003) 'InputLatch physical drive mask is 0x00000003'
Assert-True ($controlMask -eq 0x00000003) 'ownership physical axis mask is 0x00000003'
Assert-True ($diagnosticsMask -eq 0x00000003) 'Diagnostics physical drive mask is 0x00000003'
Assert-True (($latchMask -eq $controlMask) -and ($controlMask -eq $diagnosticsMask)) 'all runtime physical masks agree'
Assert-True ((($latchMask -band 0x03) -eq 0x03) -and (($latchMask -band 0x0C) -eq 0)) 'mask includes Axis1/2 and excludes Axis3/4'
Assert-Match $latch '(?m)^\s*#define\s+LMC_OWNER_STARTUP_LATCH_PHYSICAL\s+0x00000001\s*$' 'InputLatch owns a file-local physical startup-latch definition'
Assert-Match $control '(?m)^\s*#define\s+LMC_OWNER_STARTUP_LATCH_PHYSICAL\s+0x00000001\s*$' 'ownership service owns a file-local physical startup-latch definition'

for ($axis = 1; $axis -le 4; $axis++) {
    $bit = '0x{0:X8}' -f (1 -shl ($axis - 1))
    $snapshotIndex = 1 + $axis
    Assert-Match $latch ("(?s)LMC_CONFIGURED_PHYSICAL_DRIVE_MASK\s+and\s+{0}.*?IsClientConnected\(#Drive{1}\).*?IsClientConnected\(#LMCAxis{1}\)" -f $bit, $axis) "InputLatch startup proof mask-gates Drive$axis and LMCAxis$axis"
    Assert-Match $control ("(?s)LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK\s+and\s+{0}.*?startupSnapshot\[{1}\]" -f $bit, $snapshotIndex) "ownership startup idle proof mask-gates Axis$axis"
    Assert-Match $diagnostics ("(?s)LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK\s+and\s+{0}.*?IsClientConnected\(#SdoAxis{1}\)" -f $bit, $axis) "Diagnostics startup proof mask-gates SdoAxis$axis"
}

Assert-Match $diagnostics '(?s)TO_UDINT\(1\)\s+shl\s+TO_UDINT\(driveReference\s*-\s*1\).*?LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK\)\s*=\s*0\s+then\s+detailCode\s*:=\s*LMC_DIAG_ENCODER_DETAIL_PHYSICAL_DRIVE_UNAVAILABLE\s*;' 'Encoder Maintenance rejects nonphysical targets through the configured mask'
Assert-Match $diagnostics '(?m)^\s*#define\s+LMC_DIAG_ENCODER_DETAIL_PHYSICAL_DRIVE_UNAVAILABLE\s+44\s*$' 'Encoder Maintenance nonphysical detail code remains 44'

# SimulationSetup owns nine retained settings and forwards them one-to-one.
for ($axis = 1; $axis -le 9; $axis++) {
    Assert-MatchCount $simulation ('<Server\s+Name="Axis_{0}"[^>]*Initialize="true"[^>]*Retentive="File"[^>]*/>' -f $axis) 1 "SimulationSetup Axis_$axis is one initialized File-retentive server"
    Assert-MatchCount $simulation ('<Client\s+Name="Simul_Axis_{0}"[^>]*/>' -f $axis) 1 "SimulationSetup has one Simul_Axis_$axis client"
}

$initBlock = [regex]::Match($simulation, '(?s)FUNCTION\s+VIRTUAL\s+GLOBAL\s+SimulationSetup::Init\b.*?END_FUNCTION')
Assert-True $initBlock.Success 'SimulationSetup Init implementation exists'
Assert-Match $initBlock.Value '(?s)if\s+_FirstScan\s*=\s*1\s+then' 'SimulationSetup applies retained settings on first scan'

$writeSimulBlock = [regex]::Match($simulation, '(?s)FUNCTION\s+SimulationSetup::Write_Simul\b.*?END_FUNCTION')
Assert-True $writeSimulBlock.Success 'SimulationSetup Write_Simul implementation exists'
for ($axis = 1; $axis -le 9; $axis++) {
    Assert-MatchCount $initBlock.Value ("Write_Simul\(Index\s*:=\s*{0},\s*Value\s*:=\s*Axis_{0}\)\s*;" -f $axis) 1 "first scan forwards Axis_$axis exactly once"
    $axisWriteBlock = [regex]::Match($simulation, ("(?s)FUNCTION\s+VIRTUAL\s+GLOBAL\s+SimulationSetup::Axis_{0}::Write\b.*?END_FUNCTION" -f $axis))
    Assert-True $axisWriteBlock.Success "Axis_$axis Write implementation exists"
    Assert-Match $axisWriteBlock.Value ("(?s)Axis_{0}\s*:=\s*input\s*;.*?Write_Simul\(Index\s*:=\s*{0},\s*Value\s*:=\s*Axis_{0}\)\s*;.*?result\s*:=\s*Axis_{0}\s*;" -f $axis) "Axis_$axis Write stores and forwards the matching value"
    Assert-MatchCount $writeSimulBlock.Value ("(?m)^\s*{0}:\s*\r?\n\s*Simul_Axis_{0}\.Write\(input\s*:=\s*Value\)\s*;" -f $axis) 1 "Write_Simul case $axis targets Simul_Axis_$axis exactly once"
}

# Parse the editable Motion Network source, not only the generated table.
try {
    [xml]$networkXml = $network
} catch {
    throw "FAIL TOPO-C0 static contract: Motion_Network.lcn is not valid XML: $($_.Exception.Message)"
}
$simulationObjects = @($networkXml.SelectNodes("//*[local-name()='Object' and @Name='SimulationSetup1']"))
Assert-True ($simulationObjects.Count -eq 1) 'Motion Network has exactly one SimulationSetup1 object'
$simulationObject = $simulationObjects[0]
Assert-True ($simulationObject.Class -eq 'SimulationSetup') 'SimulationSetup1 uses the SimulationSetup class'

for ($axis = 1; $axis -le 9; $axis++) {
    $serverNodes = @($simulationObject.SelectNodes("./*[local-name()='Channels']/*[local-name()='Server' and @Name='Axis_$axis']"))
    Assert-True ($serverNodes.Count -eq 1) "Motion Network has exactly one Axis_$axis setting"
    $valueAttribute = $serverNodes[0].Attributes['Value']
    if ($axis -le 2) {
        Assert-True (($null -eq $valueAttribute) -or ($valueAttribute.Value -eq '0')) "Axis$axis network default is physical (0 or implicit 0)"
    } else {
        Assert-True (($null -ne $valueAttribute) -and ($valueAttribute.Value -eq '1')) "Axis$axis network default is simulation (1)"
    }

    $source = "SimulationSetup1.Simul_Axis_$axis"
    $destination = "_LMCAxis$axis.SimulateMode"
    $connections = @($networkXml.SelectNodes("//*[local-name()='Connection' and @Source='$source' and @Destination='$destination']"))
    Assert-True ($connections.Count -eq 1) "$source maps one-to-one to $destination"
}
$simulationConnections = @($networkXml.SelectNodes("//*[local-name()='Connection' and starts-with(@Source,'SimulationSetup1.Simul_Axis_')]"))
Assert-True ($simulationConnections.Count -eq 9) 'Motion Network has no extra or missing SimulationSetup client connections'

# Check the generated Motion Network table independently of the editable XML.
Assert-MatchCount $networkTable 'TO_UDINT\(3734862543\),\s*"SimulationSetup"' 1 'generated table registers SimulationSetup exactly once'
for ($axis = 1; $axis -le 9; $axis++) {
    $destinationId = 8 + $axis
    Assert-MatchCount $networkTable ('TO_UDINT\(183\),\s*"Simul_Axis_{0}",\s*TO_UDINT\({1}\),\s*"SimulateMode"' -f $axis, $destinationId) 1 "generated table wires Simul_Axis_$axis to _LMCAxis$axis SimulateMode"
    $expectedDefault = if ($axis -le 2) { 0 } else { 1 }
    Assert-MatchCount $networkTable ('TO_UDINT\({0}\),\s*"SimulateMode",\s*TO_UDINT\({1}\),//\|Motion_Network\._LMCAxis{2}\.SimulateMode;' -f $destinationId, $expectedDefault, $axis) 1 "generated _LMCAxis$axis SimulateMode default is $expectedDefault"
    if ($axis -le 2) {
        Assert-MatchCount $networkTable ('TO_UDINT\(183\),\s*"Axis_{0}".*?Motion_Network\.SimulationSetup1\.Axis_{0};' -f $axis) 0 "generated SimulationSetup Axis_$axis has no explicit nonzero initializer"
    } else {
        Assert-MatchCount $networkTable ('TO_UDINT\(183\),\s*"Axis_{0}",\s*TO_UDINT\(1\),//\|Motion_Network\.SimulationSetup1\.Axis_{0};' -f $axis) 1 "generated SimulationSetup Axis_$axis initializer is 1"
    }
}

# HomeDS402 may be all-OFF or all-ON, but mixed activation is never valid.
$tcpOrdinary = Get-BoolDefine $tcp 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED'
$controlOrdinary = Get-BoolDefine $control 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED'
$homeRuntime = Get-BoolDefine $diagnostics 'LMC_DIAG_DS402_HOME_ENABLED'
$startupSweep = Get-BoolDefine $latch 'LMC_DS402_HOME_STARTUP_SWEEP_ENABLED'
Assert-Match $control '(?m)^\s*#define\s+LMC_ADMIN_SET_POSITION_STORE_CONFIGURED\s+FALSE\s*$' 'SetPosition durable store remains unconfigured'
for ($axis = 1; $axis -le 4; $axis++) {
    Assert-Match $control ("(?m)^\s*#define\s+LMC_ADMIN_SET_POSITION_MAX_JUMP_AXIS{0}\s+0\s*$" -f $axis) "SetPosition Axis$axis maximum jump remains zero"
}

$adminCapabilityMatch = [regex]::Match($control, '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*(0x[0-9A-Fa-f]+)\s*;')
Assert-True $adminCapabilityMatch.Success 'Admin capability mask assignment exists'
$adminCapabilities = [Convert]::ToUInt32($adminCapabilityMatch.Groups[1].Value.Substring(2), 16)
$homeAdminCapability = ($adminCapabilities -band 0x00000040) -ne 0
Assert-True (($adminCapabilities -band 0x000000A8) -eq 0) 'Admin SetPosition capability bits 3/5/7 remain OFF'
Assert-True ([regex]::IsMatch($control, '\(pResponseFrame\s*\+\s*36\)\^\$UINT\s*:=\s*2\s*;')) 'Admin PhysicalAxisCount is 2'
$homeActivation = @(
    $tcpOrdinary,
    $controlOrdinary,
    $homeRuntime,
    $startupSweep,
    $homeAdminCapability)
$homeActivationCount = @($homeActivation | Where-Object { $_ }).Count
Assert-True (($homeActivationCount -eq 0) -or ($homeActivationCount -eq 5)) 'HomeDS402 five-value activation is atomic all-OFF or all-ON'

$diagnosticCapabilityMatch = [regex]::Match(
    $diagnostics,
    '(?s)if\s+CurrentDiagnosticsBootId\s+<>\s+0\s+then\s*\(pResponse\s*\+\s*20\)\^\$UDINT\s*:=\s*(0x[0-9A-Fa-f]+)\s*;')
Assert-True $diagnosticCapabilityMatch.Success 'Diagnostics capability mask assignment exists'
$diagnosticCapabilities = [Convert]::ToUInt32($diagnosticCapabilityMatch.Groups[1].Value.Substring(2), 16)
Assert-True ($diagnosticCapabilities -eq 0x0000613F) 'Diagnostics capability mask remains 0x0000613F; bit 6 is not a HomeDS402 capability'

$homeActivationState = if ($homeActivationCount -eq 5) { 'ON' } else { 'OFF' }
Write-Host ("TOPO-C0 static verifier PASS: {0} checks; HomeDS402 activation={1}. LASAL compile, direct-open, cold/restart boot, PLC runtime, and hardware evidence remain pending." -f $script:CheckCount, $homeActivationState)

param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$script:CheckCount = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw "FAIL Home one-axis static contract: $Message"
    }
    $script:CheckCount++
    Write-Host "PASS $Message"
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    Assert-True ([regex]::IsMatch($Text, $Pattern)) $Message
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
    param([string]$Text, [string]$Name, [string]$Scope)
    $pattern = '(?m)^\s*#define\s+' + [regex]::Escape($Name) + '\s+(0x[0-9A-Fa-f]+)\s*$'
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "$Scope defines $Name exactly once"
    return [Convert]::ToUInt32($matches[0].Groups[1].Value.Substring(2), 16)
}

function Get-BoolDefine {
    param([string]$Text, [string]$Name, [string]$Scope)
    $pattern = '(?m)^\s*#define\s+' + [regex]::Escape($Name) + '\s+(TRUE|FALSE)\s*$'
    $matches = [regex]::Matches($Text, $pattern)
    Assert-True ($matches.Count -eq 1) "$Scope defines $Name exactly once"
    return $matches[0].Groups[1].Value -eq 'TRUE'
}

$tcp = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st'
$control = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$diagnostics = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$latch = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st'
$motionNetwork = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/Motion_Network.lcn'
$motionTable = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Motion_Network/ONE_Motion_Network_Table.st'
$etherCatEni = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Eni.xml'
$etherCatNetwork = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/EtherCAT_Network/EtherCAT_Network.lcn'
$etherCatTable = Read-SourceText 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/EtherCAT_Network/ONE_EtherCAT_Network_Table.st'

$tcpMask = Get-HexDefine $tcp 'LMC_HOME_CONFIGURED_PHYSICAL_AXIS_MASK' 'TCPMotionInterface'
$controlMask = Get-HexDefine $control 'LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK' 'LMCControlCommandService'
$diagnosticsMask = Get-HexDefine $diagnostics 'LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK' 'LMCDiagnosticsService'
$latchMask = Get-HexDefine $latch 'LMC_CONFIGURED_PHYSICAL_DRIVE_MASK' 'LMCEcatInputLatch'
foreach ($entry in @(
        @{ Value = $tcpMask; Name = 'TCP Home physical mask' },
        @{ Value = $controlMask; Name = 'ownership physical mask' },
        @{ Value = $diagnosticsMask; Name = 'Diagnostics physical mask' },
        @{ Value = $latchMask; Name = 'InputLatch physical mask' })) {
    Assert-True ($entry.Value -eq [uint32]1) "$($entry.Name) is Axis1-only 0x00000001"
}
Assert-True (($tcpMask -eq $controlMask) -and ($controlMask -eq $diagnosticsMask) -and ($diagnosticsMask -eq $latchMask)) 'all Home physical masks agree'

Assert-True (-not (Get-BoolDefine $tcp 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED' 'TCPMotionInterface')) 'TCP ordinary ownership gate remains FALSE'
Assert-True (-not (Get-BoolDefine $control 'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED' 'LMCControlCommandService')) 'Control ordinary ownership gate remains FALSE'
Assert-True (Get-BoolDefine $control 'LMC_ADMIN_AXIS_HOME_ENABLED' 'LMCControlCommandService') 'LMC Home runtime gate is TRUE'
Assert-True (Get-BoolDefine $diagnostics 'LMC_DIAG_DS402_HOME_ENABLED' 'LMCDiagnosticsService') 'DS402 Home runtime gate is TRUE'
Assert-True (Get-BoolDefine $latch 'LMC_DS402_HOME_STARTUP_SWEEP_ENABLED' 'LMCEcatInputLatch') 'DS402 Home startup sweep is TRUE'

$adminMaskMatch = [regex]::Match($control, '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*(0x[0-9A-Fa-f]+)\s*;')
Assert-True $adminMaskMatch.Success 'Admin capability mask assignment exists'
$adminMask = [Convert]::ToUInt32($adminMaskMatch.Groups[1].Value.Substring(2), 16)
Assert-True (($adminMask -band [uint32]0x00000010) -ne 0) 'Admin AxisHome capability bit 4 is ON'
Assert-True (($adminMask -band [uint32]0x00000040) -ne 0) 'Admin AxisDs402Home capability bit 6 is ON'
Assert-Match $control '\(pResponseFrame\s*\+\s*36\)\^\$UINT\s*:=\s*1\s*;' 'Admin PhysicalAxisCount is 1'

$tcpHomeMatch = [regex]::Match($tcp, '(?s)if\s+\(CommandID\s*=\s*0x7D13\)\s*&\s*\(Payload\s*=\s*56\)\s+then.*?LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN')
Assert-True $tcpHomeMatch.Success 'TCP LMC Home Start block exists'
$tcpHome = $tcpHomeMatch.Value
$tcpPhysicalIndex = $tcpHome.IndexOf('LMC_HOME_CONFIGURED_PHYSICAL_AXIS_MASK')
$tcpReserveIndex = $tcpHome.IndexOf('ReserveAxisOwnership')
Assert-True (($tcpPhysicalIndex -ge 0) -and ($tcpReserveIndex -gt $tcpPhysicalIndex)) 'TCP rejects a nonphysical LMC Home target before owner reservation'
Assert-Match $tcpHome '(?s)LMC_HOME_CONFIGURED_PHYSICAL_AXIS_MASK\)\s*=\s*0\).*?Sendbuf\[20\]\$UDINT\s*:=\s*4\s*;.*?controlInvokeService\s*:=\s*FALSE\s*;' 'TCP nonphysical LMC Home response is deterministic detail 4 without service dispatch'

$reserveMatch = [regex]::Match($control, '(?s)FUNCTION\s+GLOBAL\s+LMCControlCommandService::ReserveAxisOwnership\b.*?END_FUNCTION')
Assert-True $reserveMatch.Success 'ReserveAxisOwnership source block exists'
$reserveBlock = $reserveMatch.Value
$reservePhysicalIndex = $reserveBlock.IndexOf('LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK')
$reserveTableIndex = $reserveBlock.IndexOf('OwnershipState[0]')
Assert-True (($reservePhysicalIndex -ge 0) -and ($reserveTableIndex -gt $reservePhysicalIndex)) 'specialized Home reservation rejects nonphysical axes before ownership-table mutation'
Assert-Match $reserveBlock '(?s)ResourceKind\s*=\s*LMC_OWNER_RESOURCE_LMC_HOME_ENGINE.*?ResourceKind\s*=\s*LMC_OWNER_RESOURCE_DS402_HOME_ENGINE.*?RequestedAxisMask\s+and\s+LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK\)\s*<>\s*RequestedAxisMask.*?Result\s*:=\s*-3\s*;\s*RETURN\s*;' 'both specialized Home resources enforce the physical subset predicate'

$lmcHomeMatch = [regex]::Match($control, '(?s)FUNCTION\s+LMCControlCommandService::HandleAxisZeroHomeCommands\b.*?END_FUNCTION')
Assert-True $lmcHomeMatch.Success 'LMC Home command handler exists'
$lmcHome = $lmcHomeMatch.Value
$lmcStartPhysicalIndex = $lmcHome.IndexOf('LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK')
$lmcSubmitIndex = $lmcHome.IndexOf('SubmitAxisZeroHome')
Assert-True (($lmcStartPhysicalIndex -ge 0) -and ($lmcSubmitIndex -gt $lmcStartPhysicalIndex)) 'LMC Home handler rejects Axis2..4 before RT mailbox submission'
Assert-Match $lmcHome '(?s)LMC_OWNER_CONFIGURED_PHYSICAL_AXIS_MASK\)\s*=\s*0\s+then\s+.*?adminDetailCode\s*:=\s*4\s*;' 'LMC Home handler returns detail 4 for nonphysical targets'

$ds402StartMatch = [regex]::Match($diagnostics, '(?s)FUNCTION\s+LMCDiagnosticsService::HandleAxisDs402HomeStart\b.*?END_FUNCTION')
Assert-True $ds402StartMatch.Success 'DS402 Home Start handler exists'
$ds402Start = $ds402StartMatch.Value
$ds402PhysicalIndex = $ds402Start.IndexOf('LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK')
$ds402IntentIndex = $ds402Start.IndexOf('Ds402HomeState[92] :=')
Assert-True (($ds402PhysicalIndex -ge 0) -and ($ds402IntentIndex -gt $ds402PhysicalIndex)) 'DS402 Home preflight rejects Axis2..4 before retained intent publication'
Assert-Match $ds402Start '(?s)axisMask\s+and\s+LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK\)\s*=\s*0\)\s+then\s+detailCode\s*:=\s*4\s*;' 'DS402 Home Start returns detail 4 for nonphysical targets'
Assert-Match $ds402Start '(?s)journalEligible\s*:=.*?axisMask\s+and\s+LMC_DIAG_CONFIGURED_PHYSICAL_DRIVE_MASK\)\s*<>\s*0' 'DS402 Home durable journal is physical-axis-only'

$powerMatch = [regex]::Match($control, '(?s)FUNCTION\s+LMCControlCommandService::SetResolvedGroupAxesPower\b.*?END_FUNCTION')
Assert-True $powerMatch.Success 'Servo Power dispatch block exists'
Assert-True (-not [regex]::IsMatch($powerMatch.Value, 'ZeroHome|Referenced|HomeState')) 'Servo Power dispatch has no Home/Referenced predicate'

try {
    [xml]$eniXml = $etherCatEni
    [xml]$etherCatXml = $etherCatNetwork
    [xml]$motionXml = $motionNetwork
} catch {
    throw "FAIL Home one-axis static contract: generated network XML is invalid: $($_.Exception.Message)"
}
$slaves = @($eniXml.EtherCATConfig.Config.Slave)
Assert-True ($slaves.Count -eq 2) 'ENI contains current CREVIS plus one Elmo drive'
Assert-True (([string]$slaves[0].Info.Name -ceq 'Slave 01 (GL-9086,Crevis)') -and ([string]$slaves[0].Info.AutoIncAddr -ceq '0')) 'ENI CREVIS is first at index 0'
Assert-True (([string]$slaves[1].Info.Name -ceq 'Slave 02 (Elmo Drive )') -and ([string]$slaves[1].Info.AutoIncAddr -ceq '-1')) 'ENI Elmo Axis1 drive is second at index 1'

foreach ($expected in @(
        @{ Name = 'GL_9086_11'; Index = '0' },
        @{ Name = 'Elmo_11'; Index = '1' })) {
    $nodes = @($etherCatXml.SelectNodes("/Network/Components/Object[@Name='$($expected.Name)']/Channels/Client[@Name='SlaveIndex' and @Value='$($expected.Index)']"))
    Assert-True ($nodes.Count -eq 1) "$($expected.Name) owns active SlaveIndex $($expected.Index)"
}
foreach ($inactiveName in @('Elmo_21', 'Elmo_31', 'Elmo_41')) {
    $nodes = @($etherCatXml.SelectNodes("/Network/Components/Object[@Name='$inactiveName']/Channels/Client[@Name='SlaveIndex' and @Value='DEACTIVATED_LSL']"))
    Assert-True ($nodes.Count -eq 1) "$inactiveName remains deactivated"
}
Assert-Match $etherCatTable '(?m)^TO_UDINT\(24\), "SlaveIndex", TO_UDINT\(0\),//\|EtherCAT_Network\.GL_9086_11\.SlaveIndex;' 'generated EtherCAT table maps CREVIS to index 0'
Assert-Match $etherCatTable '(?m)^TO_UDINT\(1\), "SlaveIndex", TO_UDINT\(1\),//\|EtherCAT_Network\.Elmo_11\.SlaveIndex;' 'generated EtherCAT table maps Elmo Axis1 to index 1'
Assert-Match $etherCatTable '(?m)^TO_UDINT\(2\), "SlaveIndex", TO_UDINT\(DEACTIVATED_LSL\),//\|EtherCAT_Network\.Elmo_21\.SlaveIndex;' 'generated EtherCAT table keeps Elmo Axis2 deactivated'

$simulationNodes = @($motionXml.SelectNodes("/Network/Components/Object[@Name='SimulationSetup1']"))
Assert-True ($simulationNodes.Count -eq 1) 'Motion Network contains one SimulationSetup1 object'
$simulation = $simulationNodes[0]
for ($axis = 1; $axis -le 9; $axis++) {
    $servers = @($simulation.SelectNodes("Channels/Server[@Name='Axis_$axis']"))
    Assert-True ($servers.Count -eq 1) "SimulationSetup1 has one Axis_$axis setting"
    $value = $servers[0].Attributes['Value']
    if ($axis -eq 1) {
        Assert-True (($null -eq $value) -or ($value.Value -eq '0')) 'Axis1 simulation setting is implicit or explicit 0'
    } else {
        Assert-True (($null -ne $value) -and ($value.Value -eq '1')) "Axis$axis simulation setting is 1"
    }
}
Assert-Match $motionTable '(?m)^TO_UDINT\(187\), "Axis_2", TO_UDINT\(1\),//\|Motion_Network\.SimulationSetup1\.Axis_2;' 'generated Motion table retains Axis2 simulation setting 1'
Assert-Match $motionNetwork '<Connection Source="SimulationSetup1\.Simul_Axis_1" Destination="_LMCAxis1\.SimulateMode"' 'SimulationSetup Axis1 is connected to LMCAxis1'
Assert-Match $motionNetwork '<Connection Source="SimulationSetup1\.Simul_Axis_2" Destination="_LMCAxis2\.SimulateMode"' 'SimulationSetup Axis2 is connected to LMCAxis2'

Write-Host ("Home one-axis static verifier PASS: {0} checks. This is source/static evidence only; LASAL compile, download, PLC runtime, and physical behavior remain pending." -f $script:CheckCount)

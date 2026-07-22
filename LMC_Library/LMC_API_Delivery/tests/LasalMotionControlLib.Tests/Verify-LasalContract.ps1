param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [switch]$SourceOnly
)

$ErrorActionPreference = 'Stop'

function Assert-Match {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Message
    )

    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$stPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$commNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn'
$commNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st'
$etherCatNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\EtherCAT_Network\EtherCAT_Network.lcn'
$motionNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$motionNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\ONE_Motion_Network_Table.st'
$tcpServerRtPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\_TCPIPServer_RT\_TCPIPServer_RT.st'
$classDbPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'
$diagnosticsProtocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsProtocol.cs'
$diagnosticsLatchPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st'
$diagnosticsServicePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$recorderStorePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCRecorderStore\LMCRecorderStore.st'
$sdoExecutorPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCSdoExecutor\LMCSdoExecutor.st'

$st = Get-Content -Raw -LiteralPath $stPath
$commNetwork = Get-Content -Raw -LiteralPath $commNetworkPath
$etherCatNetwork = Get-Content -Raw -LiteralPath $etherCatNetworkPath
$motionNetwork = Get-Content -Raw -LiteralPath $motionNetworkPath
$commNetworkTable = ''
$motionNetworkTable = ''
if (-not $SourceOnly) {
    $commNetworkTable = Get-Content -Raw -LiteralPath $commNetworkTablePath
    $motionNetworkTable = Get-Content -Raw -LiteralPath $motionNetworkTablePath
}
$tcpServerRt = Get-Content -Raw -LiteralPath $tcpServerRtPath
$classDbText = [Text.Encoding]::ASCII.GetString(
    [IO.File]::ReadAllBytes($classDbPath))
$protocol = Get-Content -Raw -LiteralPath $protocolPath
$diagnosticsProtocol = Get-Content -Raw -LiteralPath $diagnosticsProtocolPath
$diagnosticsLatch = Get-Content -Raw -LiteralPath $diagnosticsLatchPath
$diagnosticsService = Get-Content -Raw -LiteralPath $diagnosticsServicePath
$recorderStore = Get-Content -Raw -LiteralPath $recorderStorePath
$sdoExecutor = Get-Content -Raw -LiteralPath $sdoExecutorPath

[xml]$commNetworkXml = $commNetwork
[xml]$etherCatNetworkXml = $etherCatNetwork
[xml]$motionNetworkXml = $motionNetwork

$commRecorderStoreObjects = @(
    $commNetworkXml.SelectNodes("//Object[@Name='LMCRecorderStore1']"))
$motionRecorderStoreObjects = @(
    $motionNetworkXml.SelectNodes("//Object[@Name='LMCRecorderStore1']"))
if ($commRecorderStoreObjects.Count -ne 0 -or
    $motionRecorderStoreObjects.Count -ne 1 -or
    $motionRecorderStoreObjects[0].Class -ne 'LMCRecorderStore') {
    throw ('LMCRecorderStore1 must exist exactly once as LMCRecorderStore in ' +
        "Motion_Network: motion=$($motionRecorderStoreObjects.Count), " +
        "comm=$($commRecorderStoreObjects.Count).")
}

$recorderStoreConnections = @(
    $commNetworkXml.SelectNodes("//Connection[@Destination='LMCRecorderStore1.ClassSvr']")
    $motionNetworkXml.SelectNodes("//Connection[@Destination='LMCRecorderStore1.ClassSvr']"))
if ($recorderStoreConnections.Count -ne 2) {
    throw "LMCRecorderStore1 client connection count is $($recorderStoreConnections.Count), expected exactly two."
}
$recorderConnectionSources = @(
    $recorderStoreConnections | ForEach-Object { $_.Source })
foreach ($expectedRecorderSource in @(
    'LMCEcatInputLatch1.RecorderStore',
    'LMCDiagnosticsService1.RecorderStore')) {
    if (@($recorderConnectionSources | Where-Object {
                $_ -eq $expectedRecorderSource }).Count -ne 1) {
        throw "Missing or duplicate $expectedRecorderSource -> LMCRecorderStore1.ClassSvr connection."
    }
}
if (@($motionNetworkXml.SelectNodes(
            "//Connection[@Source='LMCEcatInputLatch1.RecorderStore' and " +
            "@Destination='LMCRecorderStore1.ClassSvr']")).Count -ne 1 -or
    @($commNetworkXml.SelectNodes(
            "//Connection[@Source='LMCDiagnosticsService1.RecorderStore' and " +
            "@Destination='LMCRecorderStore1.ClassSvr']")).Count -ne 1) {
    throw 'RecorderStore client connections are not in their required Motion/Comm networks.'
}

Assert-Match $st '20\$UINT,\s*12\$UINT,\s*0\$UINT' 'TCPMotionInterface generated client count is not 12.'

$clientEntries = [regex]::Matches(
    $st,
    '\(::TCPMotionInterface\.(LMCAxis[1-9]|LMCRobot|_StdLib|Diagnostics)\.pCh\)\$UINT').Count
if ($clientEntries -ne 12) {
    throw "TCPMotionInterface generated client entry count is $clientEntries, expected 12."
}

Assert-Match $st '\(::TCPMotionInterface\.Diagnostics\.pCh\)\$UINT.*"Diagnostics".*"LMCDiagnosticsService"' 'TCPMotionInterface Diagnostics client metadata is missing.'

foreach ($axisNumber in 1..9) {
    $clientName = "LMCAxis$axisNumber"
    $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
        '.*' +
        [regex]::Escape("_LMCAxis$axisNumber.Control")
    Assert-Match $commNetwork $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link in Comm_Network."
}

if (-not $SourceOnly) {
    $interfaceObject = $commNetworkXml.SelectSingleNode("//Object[@Name='TCPMotionInterface1']")
    $serverObject = $commNetworkXml.SelectSingleNode("//Object[@Name='_TCPIPServer1']")
    if ($null -eq $interfaceObject -or $null -eq $serverObject) {
        throw 'TCPMotionInterface1 or _TCPIPServer1 network object is missing.'
    }
    if ($interfaceObject.HasAttribute('RealTime')) {
        throw 'TCPMotionInterface1 must not have a RealTime task assignment.'
    }
    if ($interfaceObject.CyclicTime -ne '1 ms') {
        throw 'TCPMotionInterface1.CyclicTime must be 1 ms.'
    }
    $configClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='Config']")
    $maxConnectionsClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='MaxConnections']")
    if ($null -eq $configClient -or $configClient.Value -ne '0') {
        throw '_TCPIPServer1.Config must be explicitly set to 0.'
    }
    if ($null -eq $maxConnectionsClient -or $maxConnectionsClient.Value -ne '1') {
        throw '_TCPIPServer1.MaxConnections must be explicitly set to 1.'
    }
    Assert-Match $commNetwork 'TCPMotionInterface1\._TCPIPServer.*_TCPIPServer1\.Control' 'TCPMotionInterface1 is not connected to the ordinary TCP server in Comm_Network.'
    $diagnosticsServiceObject = $commNetworkXml.SelectSingleNode("//Object[@Name='LMCDiagnosticsService1']")
    $diagnosticsLatchObject = $motionNetworkXml.SelectSingleNode("/Network/Components/Object[@Name='LMCEcatInputLatch1']")
    if ($null -eq $diagnosticsServiceObject -or $diagnosticsServiceObject.Class -ne 'LMCDiagnosticsService') {
        throw 'LMCDiagnosticsService1 network object is missing from Comm_Network.'
    }
    Assert-Match $classDbText 'DiagnosticsBootCounter' 'Classes.lcb metadata is missing DiagnosticsBootCounter. Reload and save LMCDiagnosticsService through LASAL IDE.'
    Assert-Match $classDbText 'GetDiagnosticsBootId' 'Classes.lcb metadata is missing GetDiagnosticsBootId. Reload and save LMCDiagnosticsService through LASAL IDE.'
    $diagnosticsBootCounterServer = $diagnosticsServiceObject.SelectSingleNode(
        "./Channels/Server[@Name='DiagnosticsBootCounter']")
    if ($null -eq $diagnosticsBootCounterServer -or
        $diagnosticsBootCounterServer.Value -ne '0') {
        throw 'LMCDiagnosticsService1.DiagnosticsBootCounter network initialization is missing.'
    }
    Assert-Match $commNetworkTable '"DiagnosticsBootCounter",\s*TO_UDINT\(0\),//\|Comm_Network\.LMCDiagnosticsService1\.DiagnosticsBootCounter;' 'LMCDiagnosticsService1 generated DiagnosticsBootCounter initialization is stale in Comm_Network.'
    if ($null -eq $diagnosticsLatchObject -or $diagnosticsLatchObject.Class -ne 'LMCEcatInputLatch') {
        throw 'LMCEcatInputLatch1 network object is missing from Motion_Network.'
    }
    if ($diagnosticsLatchObject.HasAttribute('RealTime') -or
        $diagnosticsLatchObject.HasAttribute('CyclicTime') -or
        $diagnosticsLatchObject.HasAttribute('BackgroundTime')) {
        throw 'LMCEcatInputLatch1 must not own an independent scheduled task.'
    }
    $diagnosticsLatchTriggerConnections = @(
        $motionNetworkXml.SelectNodes(
            "//Connection[@Source='_LMCAxis1.LMCPreRtWorkTrigger' and " +
            "@Destination='LMCEcatInputLatch1.ClassSvr']"))
    if ($diagnosticsLatchTriggerConnections.Count -ne 1) {
        throw ('LMCEcatInputLatch1 must have exactly one ' +
            '_LMCAxis1.LMCPreRtWorkTrigger connection for same-cycle ordering.')
    }
    $diagnosticsNetworkText = $commNetwork + "`n" + $motionNetwork
    foreach ($link in @(
        'TCPMotionInterface1.Diagnostics.*LMCDiagnosticsService1.ClassSvr',
        'LMCDiagnosticsService1.InputLatch.*LMCEcatInputLatch1.ClassSvr',
        'LMCEcatInputLatch1.EcatMaster.*EtherCAT_PLC1.ClassState',
        'LMCEcatInputLatch1.Drive1.*Elmo_11.ClassState',
        'LMCEcatInputLatch1.Drive2.*Elmo_21.ClassState',
        'LMCEcatInputLatch1.Drive3.*Elmo_31.ClassState',
        'LMCEcatInputLatch1.Drive4.*Elmo_41.ClassState')) {
        Assert-Match $diagnosticsNetworkText $link "Missing diagnostics network link matching $link."
    }

    $sdoExecutorObjects = @(
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Class='LMCSdoExecutor']"))
    if ($sdoExecutorObjects.Count -ne 4) {
        throw "EtherCAT_Network LMCSdoExecutor object count is $($sdoExecutorObjects.Count), expected exactly four."
    }
    $rawSdoBaseObjects = @(
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Class='EtherCAT_SDOBase']"))
    if ($rawSdoBaseObjects.Count -ne 0) {
        throw ('EtherCAT_Network still contains production EtherCAT_SDOBase ' +
            "objects=$($rawSdoBaseObjects.Count); replace them with LMCSdoExecutor instances.")
    }
    foreach ($sdoAxis in 1..4) {
        $executorName = "LMCSdoExecutor$sdoAxis"
        $driveName = "Elmo_$($sdoAxis)1"
        $executorObjectsForAxis = @(
            $etherCatNetworkXml.SelectNodes(
                "/Network/Components/Object[@Name='$executorName' and " +
                "@Class='LMCSdoExecutor']"))
        if ($executorObjectsForAxis.Count -ne 1) {
            throw "$executorName must exist exactly once as LMCSdoExecutor in EtherCAT_Network."
        }
        $executorObject = $executorObjectsForAxis[0]
        $executorRemotely = $executorObject.GetAttribute('Remotely')
        if ($executorObject.GetAttribute('Visualized') -ne 'false' -or
            ($executorRemotely -ne '' -and $executorRemotely -ne 'false')) {
            throw "$executorName must set Visualized=false and Remotely=false."
        }

        $slaveConnections = @(
            $etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='$executorName.toSlave' and " +
                "@Destination='$driveName.ClassState']"))
        if ($slaveConnections.Count -ne 1) {
            throw "Missing or duplicate $executorName.toSlave -> $driveName.ClassState connection in EtherCAT_Network."
        }

        $sdoClientName = "SdoAxis$sdoAxis"
        $sdoClient = $diagnosticsServiceObject.SelectSingleNode(
            "./Channels/Client[@Name='$sdoClientName']")
        if ($null -eq $sdoClient) {
            throw "LMCDiagnosticsService1.$sdoClientName client is missing from Comm_Network."
        }
        $serviceConnections = @(
            $commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='LMCDiagnosticsService1.$sdoClientName' and " +
                "@Destination='$executorName.ClassState']"))
        if ($serviceConnections.Count -ne 1) {
            throw ("Missing or duplicate LMCDiagnosticsService1.$sdoClientName " +
                "-> $executorName.ClassState cross-network connection in Comm_Network.")
        }
    }
    if (@($etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'LMCSdoExecutor') " +
                "and substring-after(@Source,'.')='toSlave']")).Count -ne 4) {
        throw 'EtherCAT_Network must contain exactly four LMCSdoExecutor.toSlave connections.'
    }
    if (@($etherCatNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'EtherCAT_SDOBase')]")).Count -ne 0) {
        throw 'EtherCAT_Network still contains legacy EtherCAT_SDOBase connections.'
    }
    if (@($commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[starts-with(@Source,'LMCDiagnosticsService1.SdoAxis')]")).Count -ne 4) {
        throw 'Comm_Network must contain exactly four LMCDiagnosticsService1.SdoAxis cross-network connections.'
    }

    foreach ($classDbEntry in @(
        'LMCSdoExecutor',
        'TryStartRead4',
        'CopyCompletion',
        'MarkOrphan',
        'IsReusable',
        'LMCSdoExecutorResult',
        'SdoAxis1',
        'SdoAxis2',
        'SdoAxis3',
        'SdoAxis4',
        'ProcessOperations')) {
        Assert-Match $classDbText ([regex]::Escape($classDbEntry)) (
            "Classes.lcb metadata is missing $classDbEntry. Reload and save the SDO classes through LASAL IDE.")
    }
    Assert-Match $commNetworkTable '"MaxConnections",\s*TO_UDINT\(1\),//\|Comm_Network\._TCPIPServer1\.MaxConnections;' '_TCPIPServer1 generated MaxConnections value is stale in Comm_Network.'
    foreach ($axisNumber in 1..9) {
        $axisObject = $motionNetworkXml.SelectSingleNode(
            "/Network/Components/Object[@Name='_LMCAxis$axisNumber']")
        if ($null -eq $axisObject) {
            throw "_LMCAxis$axisNumber network object is missing."
        }

        $moveTypeServer = $axisObject.SelectSingleNode(
            "./Channels/Server[@Name='MoveType']")
        if ($null -eq $moveTypeServer -or
            $moveTypeServer.Value -ne '_JERK_PROFILE') {
            throw "_LMCAxis$axisNumber.MoveType must be _JERK_PROFILE for nonzero Jerk commands."
        }

        $jMaxServer = $axisObject.SelectSingleNode(
            "./Channels/Server[@Name='JMax']")
        if ($null -eq $jMaxServer -or
            [string]::IsNullOrWhiteSpace($jMaxServer.Value) -or
            $jMaxServer.Value -match '^\s*0(?:\s|$)') {
            throw "_LMCAxis$axisNumber.JMax must be configured to a nonzero value."
        }

        $generatedMoveTypePattern =
            '"MoveType",\s*TO_UDINT\(_JERK_PROFILE\),//\|Motion_Network\._LMCAxis' +
            $axisNumber +
            '\.MoveType;'
        Assert-Match $motionNetworkTable $generatedMoveTypePattern "_LMCAxis$axisNumber generated MoveType value is stale."
    }

    $robotObject = $motionNetworkXml.SelectSingleNode(
        "/Network/Components/Object[@Name='_LMCRobotBase1']")
    if ($null -eq $robotObject) {
        throw '_LMCRobotBase1 network object is missing.'
    }

    $robotMoveTypeServer = $robotObject.SelectSingleNode(
        "./Channels/Server[@Name='MoveType']")
    if ($null -eq $robotMoveTypeServer -or
        $robotMoveTypeServer.Value -ne '_JERK_PROFILE') {
        throw '_LMCRobotBase1.MoveType must be _JERK_PROFILE for nonzero group Jerk commands.'
    }

    $robotJMaxServer = $robotObject.SelectSingleNode(
        "./Channels/Server[@Name='JMax']")
    if ($null -eq $robotJMaxServer -or
        [string]::IsNullOrWhiteSpace($robotJMaxServer.Value) -or
        $robotJMaxServer.Value -match '^\s*0(?:\s|$)') {
        throw '_LMCRobotBase1.JMax must be configured to a nonzero value.'
    }

    Assert-Match $motionNetworkTable '"MoveType",\s*TO_UDINT\(_JERK_PROFILE\),//\|Motion_Network\._LMCRobotBase1\.MoveType;' '_LMCRobotBase1 generated MoveType value is stale.'
    Assert-Match $motionNetworkTable '"JMax",\s*TO_UDINT\((?!0(?:\s|\)))[^)]+\),//\|Motion_Network\._LMCRobotBase1\.JMax;' '_LMCRobotBase1 generated JMax value is zero or stale.'
    $generatedTaskRefs = [regex]::Matches($commNetworkTable, '//TCPMOTIONINTERFACE1').Count
    if ($generatedTaskRefs -ne 2) {
        throw "TCPMotionInterface1 generated task references=$generatedTaskRefs, expected two cyclic-only entries. Regenerate the LASAL network table."
    }
}

if ($st -match '(?<![A-Za-z0-9_])LMCAxis(?![A-Za-z0-9_])') {
    throw 'Legacy standalone LMCAxis name is still present in TCPMotionInterface.'
}

Assert-Match $st 'RequestQueue\s*:\s*ARRAY \[0\.\.7\] OF _TCPMI_REQUEST_ENTRY' 'Depth-8 LASAL request queue is missing.'
Assert-Match $st 'TO_UDINT\(1663666918\),\s*"LMCAxis1".*TO_UDINT\(1422175863\),\s*"_LMCAxis"' 'LMCAxis1 client-name/type hashes are incorrect.'
Assert-Match $st 'RealtimeTask\s*=\s*"false"' 'TCPMotionInterface still enables a RealTime task.'
Assert-Match $st 'CyclicTask\s*=\s*"true"' 'TCPMotionInterface Cyclic task is disabled.'
Assert-Match $st 'DefCyclictime\s*=\s*"1 ms"' 'TCPMotionInterface default cyclic time is not 1 ms.'
Assert-Match $st 'PayloadData\s*:\s*ARRAY \[0\.\.1319\] OF BYTE' 'LASAL queue does not hold the 1320-byte kinematic payload.'
Assert-Match $st 'ReceiveBuf\s*:\s*ARRAY \[0\.\.2047\] OF BYTE' 'LASAL receive accumulator does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'RequestBuf\s*:\s*ARRAY \[0\.\.1327\] OF BYTE' 'LASAL active request buffer does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'if usPayloadLength > 1320 then' 'LASAL queue payload bound is not 1320 bytes.'
Assert-Match $st 'IngressDiscardRemaining\s*:=\s*udFrameSize - ReceiveFill' 'Oversize frame bounded discard is missing.'
Assert-Match $st 'GroupMoveRetCode\s*:=\s*_LMCPROF_MOVECMD_ERROR' 'Group move false-success guard is missing.'
$classDeclarationBlock = [regex]::Match(
    $st,
    '(?s)TCPMotionInterface\s*:\s*CLASS.*?END_CLASS;').Value
foreach ($persistentName in @(
    'GroupCommandConfig',
    'GroupCommandInputValid',
    'GroupStopCommandNo',
    'GroupReadPos',
    'GroupReadRetCode',
    'GroupKinematicReady',
    'GroupReadInPosition',
    'GroupReadState',
    'GroupReadErrorId')) {
    Assert-Match $classDeclarationBlock ([regex]::Escape($persistentName)) "LASAL class declaration is missing $persistentName."
    Assert-Match $classDbText ([regex]::Escape($persistentName)) "Classes.lcb metadata is missing $persistentName. Save the variable through LASAL IDE."
}
foreach ($localOnlyName in @(
    'GroupKinematicConfigured',
    'GroupPowerIsOn',
    'GroupProfileLocked',
    'GroupProfileLockState')) {
    if ($classDeclarationBlock -match [regex]::Escape($localOnlyName)) {
        throw "$localOnlyName was added to the generated class declaration without matching LASAL class metadata."
    }
}
if ($st -match '(?:_TCPMI_RT_|RtRequest|RtResult|ActiveAwaitingRt|TCPMotionInterface::RtWork|CmdTable\.RtWork)') {
    throw 'TCPMotionInterface still contains an RT mailbox or RtWork dependency.'
}
if ($st -match 'sigclib_atomic_') {
    throw 'TCPMotionInterface still contains cross-task atomic operations.'
}
if ($st -match 'bDirect\s*:=\s*FALSE') {
    throw 'TCPMotionInterface mixes buffered and direct TX ordering.'
}

$msgParserBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION TCPMotionInterface::MsgPaser.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($msgParserBlock)) {
    throw 'TCPMotionInterface.MsgPaser implementation was not found.'
}
$caseIndex = $msgParserBlock.IndexOf('case CommandID of')
if ($caseIndex -lt 0) {
    throw 'TCPMotionInterface.MsgPaser command case was not found.'
}
$preCaseBlock = $msgParserBlock.Substring(0, $caseIndex)
foreach ($commandId in @('2023', '2024', '2022', '2028', '202E', '209F', '20A0', '20A2', '2047', '2048', '2049', '204A', '204B', '2045', '2051', '2085', '20A4', '20E7')) {
    if ($preCaseBlock -match "CommandID = 0x$commandId") {
        throw "Active command 0x$commandId is blocked before its CyWork handler."
    }
}

$diagnosticsCapabilitiesCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x7E00:.*?0x103C:').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsCapabilitiesCaseBlock)) {
    throw '0x7E00 diagnostics capability case was not found.'
}
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if Payload >= 8 then.*?RequestBuf\[8\]\$UINT.*?RequestBuf\[10\]\$UINT.*?RequestBuf\[12\]\$UDINT' '0x7E00 common request fields are not decoded for exact and overlength envelopes at the specified offsets.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)\(Payload <> 8\) \| \(AxisRef <> 0\).*?diagnosticsSchemaVersion <> 1.*?diagnosticsRequestFlags <> 0' '0x7E00 payload/reference/schema/flags validation is missing.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)elsif diagnosticsRequestId = 0 then.*?Sendbuf\[20\]\$UDINT\s*:=\s*12' '0x7E00 does not reject the reserved RequestId zero value with BoundsInvalid.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[2\]\$UINT\s*:=\s*68' '0x7E00 response payload length is not 68 bytes.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[24\]\$UDINT\s*:=\s*1' '0x7E00 DiagnosticsBuild is not 1.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[28\]\$UDINT\s*:=\s*0' '0x7E00 disconnected CapabilityBits default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[32\]\$UDINT\s*:=\s*0' '0x7E00 disconnected MapRevision default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[36\]\$UINT\s*:=\s*0' '0x7E00 disconnected CatalogEntryCount default is not fail-closed.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)diagnosticsBootId := 0;.*?IsClientConnected\(#Diagnostics\).*?diagnosticsBootId := Diagnostics\.GetDiagnosticsBootId\(\)' '0x7E00 does not obtain the runtime retained DiagnosticsBootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if IsClientConnected\(#Diagnostics\) then\s*Sendbuf\[28\]\$UDINT\s*:=\s*0x00000007;\s*Sendbuf\[32\]\$UDINT\s*:=\s*0x957F101E;\s*Sendbuf\[36\]\$UINT\s*:=\s*24' '0x7E00 does not advertise active D1 Health/Catalog/PI with the canonical map.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if diagnosticsBootId <> 0 then\s*Sendbuf\[28\]\$UDINT\s*:=\s*0x0000003F;\s*Sendbuf\[38\]\$UINT\s*:=\s*24;\s*Sendbuf\[40\]\$UINT\s*:=\s*24;\s*Sendbuf\[42\]\$UINT\s*:=\s*1;\s*Sendbuf\[44\]\$UDINT\s*:=\s*320000;\s*Sendbuf\[64\]\$UDINT\s*:=\s*1280000' '0x7E00 does not advertise the bounded D2 Bulk, D3 single-bank Recorder, and D4 single-bank trigger/ring envelope only for a stable BootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[52\]\$UINT\s*:=\s*1320' '0x7E00 MaxRequestPayloadBytes is not 1320.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[54\]\$UINT\s*:=\s*2040' '0x7E00 MaxResponsePayloadBytes is not 2040.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[56\]\$UINT\s*:=\s*1280' '0x7E00 MaxChunkDataBytes is not 1280.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[58\]\$UINT\s*:=\s*80' '0x7E00 CatalogEntryStride is not 80.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[60\]\$UINT\s*:=\s*16' '0x7E00 SignalValueEntryStride is not 16.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[68\]\$UINT\s*:=\s*0' '0x7E00 MaxSdoDataBytes must remain zero while D5 is disabled.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[72\]\$UDINT\s*:=\s*diagnosticsBootId' '0x7E00 does not return the runtime DiagnosticsBootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)SendData\(.*?udSize:=76' '0x7E00 does not send the complete 76-byte frame.'

$diagnosticsDispatchBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x7E01,\s*0x7E02.*?0x7E50,\s*0x7E51:.*?0x8080:').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsDispatchBlock)) {
    throw 'The reserved diagnostics command family is not delegated to LMCDiagnosticsService.'
}
Assert-Match $diagnosticsDispatchBlock '(?s)IsClientConnected\(#Diagnostics\).*?Diagnostics\.HandleRequest\(.*?ResponseCapacity:=2040.*?diagnosticsResponseSize <= 2040.*?SendData' 'Diagnostics service delegation or response bound is incomplete.'

Assert-Match $diagnosticsLatch 'RealtimeTask\s*=\s*"true"' 'LMCEcatInputLatch is not declared as an RT class.'
Assert-Match $diagnosticsLatch 'SnapshotBytes\s*:\s*ARRAY \[0\.\.511\] OF USINT' 'LMCEcatInputLatch fixed snapshot storage is not 512 bytes.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?OS_READMICROSEC\(\).*?Drive1\.ActPos\.Read\(\).*?Drive4\.StateWord\.Read\(\).*?state := READY' 'LMCEcatInputLatch does not latch all four PDO images and timestamp in RtWork.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_setU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not stored atomically.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_getU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not loaded atomically.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CopySnapshot.*?DestSize < 304.*?retryCount < 3.*?_memcpy.*?sequenceBefore = sequenceAfter' 'LMCEcatInputLatch bounded seqlock copy is incomplete.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?sigclib_atomic_setU32\(pValue:=#PublishSequence,\s*value:=finalSequence\).*?IsClientConnected\(#RecorderStore\).*?RecorderStore\.AppendSnapshot\(\s*pSnapshot:=#SnapshotBytes\[0\],\s*SnapshotSize:=304\).*?state := READY' 'LMCEcatInputLatch does not append the final immutable 304-byte RT snapshot to RecorderStore.'

Assert-Match $sdoExecutor '(?s)LMCSdoExecutor\s*:\s*CLASS\s*:\s*EtherCAT_SDOBase' 'LMCSdoExecutor no longer derives from EtherCAT_SDOBase.'
if ([regex]::Matches(
        $sdoExecutor,
        '<Connection\s+Source="_base\.toSlave"\s+Destination="this\.toSlave"').Count -ne 1) {
    throw 'LMCSdoExecutor internal network must forward exactly one _base.toSlave client to this.toSlave.'
}
$sdoResultTypeBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)LMCSdoExecutorResult\s*:\s*STRUCT.*?END_STRUCT').Value
if ([string]::IsNullOrWhiteSpace($sdoResultTypeBlock)) {
    throw 'LMCSdoExecutorResult declaration was not found.'
}
Assert-Match $sdoResultTypeBlock 'Type Public="true" Name="LMCSdoExecutorResult"' 'LMCSdoExecutorResult is not a public LASAL type.'
Assert-Match $sdoResultTypeBlock '(?s)Token\s*:\s*UDINT;.*?OsResult\s*:\s*DINT;.*?AbortCode\s*:\s*UDINT;.*?ActualLength\s*:\s*UDINT;.*?ObjectIndex\s*:\s*UINT;.*?SubIndex\s*:\s*USINT;.*?IsWrite\s*:\s*USINT;.*?ValidationCode\s*:\s*UDINT;.*?Data\s*:\s*UDINT;.*?Reserved\s*:\s*UDINT;' 'LMCSdoExecutorResult 32-byte public field layout is incomplete or reordered.'
Assert-Match $sdoExecutor 'sizeof\(LMCSdoExecutorResult\)\s*<>\s*32' 'LMCSdoExecutor does not fail closed if its public result ABI is not 32 bytes.'
Assert-Match $sdoExecutor '(?s)#define LMC_SDO_EXEC_IDLE\s+0.*?#define LMC_SDO_EXEC_ARMING\s+1.*?#define LMC_SDO_EXEC_RUNNING\s+2.*?#define LMC_SDO_EXEC_RESULT_READY\s+3.*?#define LMC_SDO_EXEC_ORPHANED\s+4.*?#define LMC_SDO_EXEC_QUARANTINED\s+5' 'LMCSdoExecutor atomic state constants are incomplete.'
Assert-Match $sdoExecutor 'Function Name="ClassState\.NewInst" UseBaseCmd="true"' 'LMCSdoExecutor callback override does not preserve the EtherCAT_SDOBase command table.'
Assert-Match $sdoExecutor '(?s)ParaReadWrite\.pMeth\s*:=\s*StoreMethod\(\s*#M_RD_DIRECT\(\),\s*#ParaReadWrite::Write\(\)\s*\).*?ParaType\.pMeth\s*:=\s*StoreMethod\(\s*#M_RD_DIRECT\(\),\s*#ParaType::Write\(\)\s*\).*?_memcpy\(\(#vmt\.CmdTable\)\$\^USINT,\s*ParaString\.pMeth.*?vmt\.CmdTable\.Write\s*:=\s*#ParaString::Write\(\).*?ParaString\.pMeth\s*:=\s*StoreCmd' 'LMCSdoExecutor manual-channel write overrides are not registered in the IDE-generated unqualified VMT entries.'

foreach ($manualWrite in @(
    @{ Name = 'ParaReadWrite'; Expected = 'ParaReadWrite' },
    @{ Name = 'ParaType'; Expected = 'ParaType' },
    @{ Name = 'ParaString'; Expected = 'ParaString' })) {
    $manualWriteBlock = [regex]::Match(
        $sdoExecutor,
        ('(?s)FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::' +
            $manualWrite.Name + '::Write.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($manualWriteBlock)) {
        throw "LMCSdoExecutor.$($manualWrite.Name).Write implementation was not found."
    }
    Assert-Match $manualWriteBlock (
        'result\s*:=\s*' + $manualWrite.Expected + '\s*;') (
        "LMCSdoExecutor.$($manualWrite.Name).Write does not ignore manual writes fail-closed.")
    if ($manualWriteBlock -match 'result\s*:=\s*input') {
        throw "LMCSdoExecutor.$($manualWrite.Name).Write accepts the manual input."
    }
}

$sdoTryStartBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::TryStartRead4.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoTryStartBlock)) {
    throw 'LMCSdoExecutor.TryStartRead4 implementation was not found.'
}
Assert-Match $sdoTryStartBlock '(?s)sigclib_atomic_cmpxchgU32\(\s*pValue:=#AdapterState,\s*cmpVal:=LMC_SDO_EXEC_IDLE,\s*newVal:=LMC_SDO_EXEC_ARMING\).*?toSlave\.StartReadSDO\(\s*ObjectIndex,\s*SubIndex,\s*0,\s*\(#ReadBuffer\[0\]\)\$\^USINT,\s*4,\s*TimeoutMs,\s*THIS\).*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RUNNING' 'LMCSdoExecutor does not atomically reserve and start the fixed 4-byte read with CompleteAccess=0 and its own callback.'

$sdoCopyCompletionBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::CopyCompletion.*?END_FUNCTION').Value
Assert-Match $sdoCopyCompletionBlock '(?s)sigclib_atomic_getU32\(pValue:=#AdapterState\).*?stateValue <> LMC_SDO_EXEC_RESULT_READY.*?stateValue <> LMC_SDO_EXEC_QUARANTINED.*?retryCount < 3.*?sequenceBefore := sigclib_atomic_getU32.*?_memcpy.*?sequenceAfter := sigclib_atomic_getU32.*?sequenceBefore = sequenceAfter.*?localResult\.Token <> ExpectedToken.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor completion copy lacks bounded seqlock or token validation.'
Assert-Match $sdoCopyCompletionBlock '(?s)cmpVal:=LMC_SDO_EXEC_RESULT_READY,\s*newVal:=LMC_SDO_EXEC_IDLE.*?_memcpy\(ptr1:=pDest, ptr2:=#localResult' 'LMCSdoExecutor does not atomically release a consumed normal completion.'

$sdoMarkOrphanBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::MarkOrphan.*?END_FUNCTION').Value
Assert-Match $sdoMarkOrphanBlock '(?s)ExpectedToken = 0.*?ActiveToken <> ExpectedToken.*?sigclib_atomic_cmpxchgU32\(\s*pValue:=#AdapterState,\s*cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_ORPHANED\)' 'LMCSdoExecutor does not atomically orphan only the expected running token.'
Assert-Match $sdoExecutor '(?s)FUNCTION GLOBAL LMCSdoExecutor::IsReusable.*?sigclib_atomic_getU32\(\s*pValue:=#AdapterState\)\s*=\s*LMC_SDO_EXEC_IDLE.*?END_FUNCTION' 'LMCSdoExecutor reusable state is not an atomic Idle-only check.'

$sdoCallbackBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION VIRTUAL GLOBAL LMCSdoExecutor::ClassState::NewInst.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoCallbackBlock)) {
    throw 'LMCSdoExecutor.ClassState.NewInst callback implementation was not found.'
}
Assert-Match $sdoCallbackBlock '(?s)pPara\^\.uiCmd <> ECAT_M_SDO_CALLBACK.*?ret_code := EtherCAT_SDOBase::NewInst\(pPara, pResult\).*?RETURN' 'LMCSdoExecutor does not forward unknown commands to EtherCAT_SDOBase.'
Assert-Match $sdoCallbackBlock '(?s)callbackIsWrite := pPara\^\.aPara\[2\]\$USINT.*?callbackIndex := pPara\^\.aPara\[3\]\$UINT.*?callbackSubIndex := pPara\^\.aPara\[4\]\$USINT.*?osResult := pPara\^\.aPara\[1\]\$DINT.*?actualLength := pPara\^\.aPara\[5\]\$UDINT.*?abortCode := pPara\^\.aPara\[6\]\$UDINT' 'LMCSdoExecutor callback metadata extraction is incomplete.'
Assert-Match $sdoCallbackBlock '(?s)aPara\[0\]\$DINT <> 1.*?stateValue <> LMC_SDO_EXEC_RUNNING.*?stateValue <> LMC_SDO_EXEC_ORPHANED.*?callbackIsWrite <> 0.*?callbackIndex <> ActiveIndex.*?callbackSubIndex <> ActiveSubIndex.*?ActiveToken = 0.*?actualLength <> TO_UDINT\(ActiveLength\)' 'LMCSdoExecutor callback version/state/direction/index/subindex/token/length validation is incomplete.'
Assert-Match $sdoCallbackBlock '(?s)stateValue = LMC_SDO_EXEC_ORPHANED.*?validationCode = LMC_SDO_EXEC_VALID.*?ActiveToken := 0.*?value:=LMC_SDO_EXEC_IDLE.*?RETURN' 'LMCSdoExecutor does not drain a valid late orphan callback back to Idle.'
Assert-Match $sdoCallbackBlock '(?s)writeSequence := sigclib_atomic_getU32.*?writeSequence and 1.*?sigclib_atomic_setU32\(\s*pValue:=#PublishSequence, value:=writeSequence\).*?PublishedResult\.Token := ActiveToken.*?PublishedResult\.ValidationCode := validationCode.*?PublishedResult\.Data := ReadBuffer\[0\]\$UDINT.*?finalSequence := writeSequence \+ 1.*?value:=finalSequence.*?value:=LMC_SDO_EXEC_RESULT_READY.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor callback publication is not a validated atomic seqlock result.'

Assert-Match $diagnosticsService '#define LMC_DIAG_D1_ENABLED\s+TRUE' 'D1 Health/Catalog/PI Read is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D2_ENABLED\s+TRUE' 'D2 Bulk Snapshot is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D3_ENABLED\s+TRUE' 'D3 single-bank Recorder is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D5_SDO_READ_ENABLED\s+FALSE' 'D5 SDO read compile gate must remain FALSE until LASAL/PLC runtime proof is complete.'
if ([regex]::Matches($diagnosticsService, '<Client Name="SdoAxis[1-4]" Required="true" Internal="false"/>').Count -ne 4 -or
    [regex]::Matches($diagnosticsService, 'SdoAxis[1-4]\s*:\s*CltChCmd_LMCSdoExecutor;').Count -ne 4) {
    throw 'LMCDiagnosticsService does not declare exactly four required LMCSdoExecutor clients.'
}
Assert-Match $diagnosticsService '#define LMC_DIAG_MAP_REVISION\s+0x957F101E' 'LMCDiagnosticsService MapRevision is not the canonical D1 catalog CRC.'
Assert-Match $diagnosticsService 'Server Name="DiagnosticsBootCounter".*Initialize="true".*DefValue="0".*Retentive="File"' 'LMCDiagnosticsService retained DiagnosticsBootCounter metadata is missing.'
Assert-Match $diagnosticsService '(?s)FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId.*?DiagnosticsBootCounter\.Read\(\).*?nextBootId = 0xFFFFFFFF.*?DiagnosticsBootCounter\.Write\(input:=nextBootId\).*?DiagnosticsBootCounter\.Read\(\) = nextBootId.*?BootIdFault := TRUE.*?END_FUNCTION' 'LMCDiagnosticsService retained BootId generation or write verification is incomplete.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::BuildCatalogEntry.*?CatalogIndex >= 24.*?pEntry \+ 76.*?:= 0' 'LMCDiagnosticsService fixed 80-byte catalog entry builder is incomplete.'
Assert-Match $diagnosticsService '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?0x7E01:.*?0x7E02:.*?0x7E10:.*?0x7E20:' 'LMCDiagnosticsService D1 command handlers are missing.'
Assert-Match $diagnosticsService '(?s)InputLatch\.CopySnapshot\(.*?DestSize:=sizeof\(snapshot\).*?ResponseSize := 200' 'EtherCAT Health does not use the immutable latch snapshot.'
Assert-Match $diagnosticsService '(?s)entryStatus := 0.*?entryStatus := entryStatus or 4.*?entryStatus := entryStatus or 2.*?entryStatus := 1' 'PI Read entry validity/staleness status construction is incomplete.'

$diagnosticsServiceHandleBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceHandleBlock)) {
    throw 'LMCDiagnosticsService.HandleRequest implementation was not found.'
}
Assert-Match $diagnosticsServiceHandleBlock '(?s)currentBootId := GetDiagnosticsBootId\(\).*?\(CommandId >= 0x7E30\).*?\(CommandId <= 0x7E33\).*?\(CommandId >= 0x7E40\).*?\(CommandId <= 0x7E49\).*?currentBootId = 0.*?detailCode := 11' 'LMCDiagnosticsService does not fail closed for raw stateful D2/D3 calls when BootId is unavailable.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)if \(CommandId >= 0x7E40\)\s*&\s*\(CommandId <= 0x7E49\).*?IsClientConnected\(#RecorderStore\) = FALSE.*?\(pResponse \+ 4\)\^\$UINT := 1.*?\(pResponse \+ 12\)\^\$UDINT := 11.*?ResponseSize := 16.*?RETURN' 'LMCDiagnosticsService RecorderStore disconnected path is not fail-closed.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)RecorderStore\.HandleRequest\(.*?CommandId:=CommandId.*?CallerSessionEpoch:=CallerSessionEpoch.*?CurrentDiagnosticsBootId:=currentBootId.*?ResponseCapacity:=ResponseCapacity\)' 'LMCDiagnosticsService does not delegate D3 requests with the retained runtime BootId.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)0x7E21:\s*.*?if RequestSize <> 28 then\s*detailCode := 12;\s*else\s*detailCode := 2;\s*end_if' 'SubmitPIWrite 0x7E21 must validate its 28-byte reserved wire and remain UnsupportedFeature.'

$sdoStatusBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E03:.*?0x7E04:').Value
$sdoCancelBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E04:.*?0x7E21:').Value
$sdoSubmitBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)0x7E50:.*?0x7E51:').Value
foreach ($sdoHandler in @(
    @{ Name = 'GetOperationStatus 0x7E03'; Block = $sdoStatusBlock },
    @{ Name = 'CancelOperation 0x7E04'; Block = $sdoCancelBlock },
    @{ Name = 'SubmitSDO 0x7E50'; Block = $sdoSubmitBlock })) {
    if ([string]::IsNullOrWhiteSpace($sdoHandler.Block)) {
        throw "$($sdoHandler.Name) implementation was not found."
    }
}

Assert-Match $sdoStatusBlock '(?s)RequestSize <> 16.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?ResponseCapacity < 64.*?sdoTicketId := \(pRequest \+ 8\)\^\$UDINT.*?sdoBootId := \(pRequest \+ 12\)\^\$UDINT.*?sdoTicketId <> TicketId.*?sdoBootId <> TicketBootId.*?CallerSessionEpoch <> OwnerSessionEpoch.*?\(pResponse \+ 16\)\^\$UDINT := TicketId.*?\(pResponse \+ 22\)\^\$UINT := OperationState.*?\(pResponse \+ 32\)\^\$UINT := OperationOutcome.*?\(pResponse \+ 60\)\^\$UDINT := TicketBootId.*?ResponseSize := 64' 'GetOperationStatus 0x7E03 does not validate ticket/boot/session ownership and return the fixed D5 status envelope.'
Assert-Match $sdoStatusBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_COMPLETED.*?OperationOutcome = LMC_DIAG_SDO_OUTCOME_SUCCESS.*?\(pResponse \+ 40\)\^\$UDINT := SdoResultLength.*?\(pResponse \+ 48\)\^\$UDINT := SdoResultData' 'GetOperationStatus 0x7E03 does not expose data only for a successful completed operation.'

Assert-Match $sdoCancelBlock '(?s)RequestSize <> 16.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?ResponseCapacity < 28.*?sdoTicketId <> TicketId.*?sdoBootId <> TicketBootId.*?CallerSessionEpoch <> OwnerSessionEpoch.*?OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?detailCode := 19.*?OperationState := LMC_DIAG_SDO_STATE_CANCELLED.*?OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED.*?ResponseSize := 28' 'CancelOperation 0x7E04 is not restricted to the owning queued ticket.'

if ([regex]::Matches($diagnosticsService, '(?m)^\s*TicketId\s*:\s*UDINT;').Count -ne 1 -or
    $diagnosticsService -match '(?m)^\s*TicketId\s*:\s*ARRAY') {
    throw 'LMCDiagnosticsService must own one global D5 ticket, not a ticket array.'
}
Assert-Match $sdoSubmitBlock '(?s)RequestSize < 32.*?expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT.*?sdoSlaveReference := \(pRequest \+ 12\)\^\$UINT.*?sdoOperationFlags := \(pRequest \+ 14\)\^\$UINT.*?sdoObjectIndex := \(pRequest \+ 16\)\^\$UINT.*?sdoSubIndex := \(pRequest \+ 18\)\^\$USINT.*?requestSdoValueType := \(pRequest \+ 19\)\^\$USINT.*?sdoTimeoutCycles := \(pRequest \+ 20\)\^\$UDINT.*?sdoDataLength := \(pRequest \+ 24\)\^\$UINT.*?sdoReserved := \(pRequest \+ 26\)\^\$UINT.*?sdoBootId := \(pRequest \+ 28\)\^\$UDINT.*?expectedRequestSize := 32.*?sdoOperationFlags = 1.*?expectedRequestSize \+= TO_UDINT\(sdoDataLength\).*?RequestSize <> expectedRequestSize' 'SubmitSDO 0x7E50 generic request envelope validation is incomplete.'
Assert-Match $sdoSubmitBlock '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?sdoSlaveReference < 1.*?sdoSlaveReference > 4.*?sdoTimeoutCycles < 1.*?sdoTimeoutCycles > 60000.*?expectedMapRevision <> LMC_DIAG_MAP_REVISION.*?sdoBootId <> currentBootId.*?sdoOperationFlags = 1.*?sdoDataLength <> 4.*?sdoObjectIndex <> 0x1000.*?sdoSubIndex <> 0.*?requestSdoValueType <> 5' 'SubmitSDO 0x7E50 does not enforce the gated first-slice read-only axes 1..4, 0x1000:0, UInt32, 4-byte policy.'
if ($diagnosticsServiceHandleBlock -match '(?m)^\s*sdoValueType\s*:\s*USINT;') {
    throw 'HandleRequest local sdoValueType shadows the retained SdoValueType ticket field.'
}
Assert-Match $sdoSubmitBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_QUEUED.*?OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?SdoInternalDrainState <> 0.*?detailCode := 9.*?case sdoSlaveReference of.*?SdoAxis1\.IsReusable\(\).*?SdoAxis4\.IsReusable\(\).*?NextTicketId = 0xFFFFFFFF.*?NextOperationToken = 0xFFFFFFFF.*?NextTicketId \+= 1.*?NextOperationToken \+= 1.*?TicketId := NextTicketId.*?OperationToken := NextOperationToken.*?OperationState := LMC_DIAG_SDO_STATE_QUEUED.*?SdoInternalDrainState := 0.*?ResponseSize := 32' 'SubmitSDO 0x7E50 does not allocate exactly one reusable queued ticket with wrap and drain guards.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)0x7E51:\s*.*?if RequestSize <> 28 then\s*detailCode := 12;\s*else\s*detailCode := 2;\s*end_if' 'ReadSDOResultChunk 0x7E51 must validate its 28-byte reserved wire and remain UnsupportedFeature.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)if detailCode <> 0 then.*?\(pResponse \+ 4\)\^\$UINT := 1.*?ResponseSize := 16' 'LMCDiagnosticsService reserved and error commands do not return the common 16-byte error envelope.'

$sdoProcessBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::ProcessOperations.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoProcessBlock)) {
    throw 'LMCDiagnosticsService.ProcessOperations implementation was not found.'
}
Assert-Match $sdoProcessBlock 'completion\s*:\s*LMCSdoExecutor::LMCSdoExecutorResult;' 'ProcessOperations does not use the derived executor public result type with its class qualifier.'
if ($sdoProcessBlock -match 'completion\s*:\s*LMCSdoExecutorResult;') {
    throw 'ProcessOperations uses an unqualified LMCSdoExecutorResult type that LASAL C78 cannot resolve.'
}
$typedSdoConnectionChecks = [regex]::Matches(
    $diagnosticsService,
    'executorConnected\s*:=\s*IsClientConnected\(#SdoAxis[1-4]\)\s*<>\s*0;')
if ($typedSdoConnectionChecks.Count -ne 12 -or
    $diagnosticsService -match 'executorConnected\s*:=\s*IsClientConnected\(#SdoAxis[1-4]\)\s*;') {
    throw 'LMCDiagnosticsService must convert all twelve SdoAxis connection checks from DINT to BOOL explicitly.'
}
Assert-Match $sdoProcessBlock '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?RETURN.*?TicketId = 0.*?SdoInternalDrainState = 0' 'ProcessOperations does not remain inert behind the D5 compile gate and empty-ticket guard.'
Assert-Match $sdoProcessBlock '(?s)SdoInternalDrainState <> 0.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) = FALSE.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) then.*?SdoInternalDrainState := 0.*?RETURN' 'ProcessOperations does not drain late timeout/disconnect callbacks before releasing the executor.'
Assert-Match $sdoProcessBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?elapsedCycles := currentCycle - SdoSubmitCycle.*?if \(completionResult = 0\)\s*&\s*\(elapsedCycles > SdoTimeoutCycles\) then.*?elsif completionResult = 0 then.*?OperationState := LMC_DIAG_SDO_STATE_COMPLETED.*?RETURN.*?elapsedCycles >= SdoTimeoutCycles.*?MarkOrphan\(\s*ExpectedToken:=OperationToken\).*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED.*?SdoInternalDrainState := LMC_DIAG_SDO_DRAIN_EXPIRED' 'ProcessOperations must consume a completion at the deadline before timeout and quarantine an incomplete timed-out adapter for late-callback drain.'
Assert-Match $sdoProcessBlock '(?s)completion\.ValidationCode = 7.*?SdoOperationDetail := 5.*?else\s*SdoOperationDetail := 24.*?completion\.OsResult <> 0.*?completion\.AbortCode = 0x08000000.*?SdoOperationDetail := completion\.OsResult\$UDINT.*?elsif completion\.AbortCode <> 0.*?elsif completion\.ActualLength <> 4 then.*?SdoOperationDetail := 5.*?completion\.ObjectIndex <> SdoObjectIndex.*?SdoOperationDetail := 24' 'ProcessOperations does not preserve the first-slice validation, OS/abort priority, length, and metadata error mapping.'
Assert-Match $sdoProcessBlock '(?s)OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?OperationState <> LMC_DIAG_SDO_STATE_RUNNING.*?currentCycle = SdoLastProcessedCycle.*?remainingCycles := SdoTimeoutCycles - elapsedCycles.*?case SdoSlaveReference of.*?SdoAxis1\.TryStartRead4\(.*?SdoAxis4\.TryStartRead4\(.*?startResult = READY.*?OperationState := LMC_DIAG_SDO_STATE_RUNNING' 'ProcessOperations does not start one queued read per published RT cycle through the selected executor.'

$diagnosticsServiceNotifyBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::NotifySessionClosed.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceNotifyBlock)) {
    throw 'LMCDiagnosticsService.NotifySessionClosed implementation was not found.'
}
Assert-Match $diagnosticsServiceNotifyBlock '(?s)SessionEpoch = BulkOwnerSessionEpoch.*?BulkState := 0.*?RecorderStore\.NotifySessionClosed\(SessionEpoch:=SessionEpoch\)' 'LMCDiagnosticsService does not release the matching Bulk owner and notify RecorderStore on session close.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::@STD.*?ret_code\s*:=\s*LMCDiagnosticsService\(\).*?END_FUNCTION' 'LMCDiagnosticsService @STD does not invoke its constructor.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::LMCDiagnosticsService.*?NextBulkId := 0.*?BulkState := 0.*?_memset\(dest:=#BulkSignalIds\[0\].*?ret_code := C_OK.*?END_FUNCTION' 'LMCDiagnosticsService constructor does not initialize its complete Bulk state.'

Assert-Match $recorderStore '(?s)VAR_GLOBAL\s+g_LMCRecorderData\s*:\s*ARRAY \[0\.\.1279999\] OF USINT;\s*END_VAR' 'LMCRecorderStore fixed 1,280,000-byte global recorder bank is missing.'
Assert-Match $recorderStore '#define LMC_RECORDER_STORAGE_BYTES\s+1280000' 'LMCRecorderStore storage-size constant does not match the global recorder bank.'
Assert-Match $recorderStore '(?s)stride := TO_UDINT\(requestedChannelCount\) \* 4;.*?acceptedCapacity := LMC_RECORDER_STORAGE_BYTES / stride;.*?if acceptedCapacity > requestedCapacity then\s*acceptedCapacity := requestedCapacity;\s*end_if' 'LMCRecorderStore ConfigureRecorder does not clamp AcceptedCapacity to the fixed bank size and requested sample count.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::@STD.*?ret_code\s*:=\s*LMCRecorderStore\(\).*?END_FUNCTION' 'LMCRecorderStore @STD does not invoke its constructor.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::LMCRecorderStore.*?StateValue := LMC_RECORDER_EMPTY.*?SamplePeriodCycles := 1.*?NextConfigId := 1.*?NextRecordId := 1.*?BufferReleased := TRUE.*?ret_code := C_OK.*?END_FUNCTION' 'LMCRecorderStore constructor does not initialize recorder identity, timing, and ownership state.'
Assert-Match $recorderStore '(?s)elsif \(CurrentDiagnosticsBootId = 0\) then\s*detailCode := 11' 'LMCRecorderStore does not reject the BootId-zero sentinel before stateful D3 processing.'

$recorderHandleRequestBlock = [regex]::Match(
    $recorderStore,
    '(?s)FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($recorderHandleRequestBlock)) {
    throw 'LMCRecorderStore.HandleRequest implementation was not found.'
}
foreach ($recorderCommandId in @(
    '0x7E40', '0x7E41', '0x7E42', '0x7E43', '0x7E44',
    '0x7E45', '0x7E46', '0x7E47', '0x7E48', '0x7E49')) {
    $recorderCommandCount = [regex]::Matches(
        $recorderHandleRequestBlock,
        "(?m)^\s*$recorderCommandId\s*:").Count
    if ($recorderCommandCount -ne 1) {
        throw "LMCRecorderStore command $recorderCommandId handler count is $recorderCommandCount, expected one."
    }
}
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:\s*.*?RequestSize <> 28.*?requestRecordId := \(pRequest \+ 8\)\^\$UDINT.*?requestBufferId := \(pRequest \+ 12\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 16\)\^\$UDINT.*?requestOwnerEpoch := \(pRequest \+ 20\)\^\$UDINT.*?requestBootId := \(pRequest \+ 24\)\^\$UDINT.*?TriggerType = 0.*?TriggerRequestSequence.*?ResponseSize := 16.*?0x7E43:' 'TriggerRecorder 0x7E42 does not validate identity/ownership and queue an RT trigger request.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E43:.*?requestRecordId <> RecordId.*?expectedMapRevision <> MapRevision.*?requestBootId <> DiagnosticsBootId.*?requestOwnerEpoch <> OwnerSessionEpoch.*?state = LMC_RECORDER_READY.*?state = LMC_RECORDER_UPLOADING.*?ResponseSize := 16.*?state <> LMC_RECORDER_ARMED.*?state <> LMC_RECORDER_RECORDING.*?detailCode := 19.*?StopRequestSequence.*?ResponseSize := 16.*?0x7E44:' 'StopRecorder must preserve identity/ownership checks, acknowledge Ready/Uploading idempotently, and queue only active-state stops.'
Assert-Match $recorderHandleRequestBlock '(?s)if detailCode <> 0 then.*?\(pResponse \+ 4\)\^\$UINT := 1.*?ResponseSize := 16' 'LMCRecorderStore reserved and error commands do not return the common 16-byte error envelope.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?StateValue.*?g_LMCRecorderData.*?SampleCount \+= 1.*?END_FUNCTION' 'LMCRecorderStore RT AppendSnapshot capture path is incomplete.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of.*?LMC_RECORDER_STOP_TRIGGER_COMPLETE.*?END_FUNCTION' 'LMCRecorderStore D4 edge/window/mask RT trigger path is incomplete.'
Assert-Match $recorderStore '(?s)triggerInputValid :=\s*\(\(pSnapshot \+ 12\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ 16\)\^\$UDINT = 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset\)\^\$DINT <> 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 4\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 12\)\^\$UDINT = 0\)' 'LMCRecorderStore trigger validity must require master OP/no missed frame and axis Online/OP/AL=0.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64.*?TriggerSignalId.*?triggerInputValid :=.*?triggerHealthOffset.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?if prehistoryReady then.*?triggerRequest <> TriggerAppliedSequence.*?triggerEvent := TRUE.*?elsif triggerInputValid then.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of' 'LMCRecorderStore automatic edge/window/mask trigger evaluation is not gated by a valid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)if triggerInputValid then\s*PreviousTriggerValue := triggerRaw;\s*PreviousTriggerValid := TRUE;\s*else\s*.*?PreviousTriggerValid := FALSE;\s*end_if' 'LMCRecorderStore does not reset edge/window history across an invalid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)FrozenFirstSampleIndex :=.*?WriteSampleIndex \+ SampleCapacity - SampleCount.*?FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?physicalSampleIndex :=.*?FrozenFirstSampleIndex \+ offsetSample.*?_memcpy' 'LMCRecorderStore does not preserve and upload pre-trigger ring data in chronological order.'
Assert-Match $recorderStore '(?s)stopRequest <> StopAppliedSequence.*?TriggerIndex = 0xFFFFFFFF.*?FrozenFirstSampleIndex :=\s*\(WriteSampleIndex \+ SampleCapacity - SampleCount\).*?StopReason := LMC_RECORDER_STOP_USER' 'LMCRecorderStore does not freeze chronological pre-trigger ring order when the user stops before a trigger.'
Assert-Match $recorderStore '(?s)StopReason := LMC_RECORDER_STOP_USER;\s*if SampleCount = 0 then.*?EndCycle := cycleCounter' 'LMCRecorderStore user stop must preserve the End metadata of the last copied sample.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:.*?TriggerType = 0.*?TriggerIndex <> 0xFFFFFFFF then.*?detailCode := 19.*?TriggerRequestSequence' 'TriggerRecorder must reject a second force-trigger after the current record has already triggered.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E40:.*?bufferMode = 2 then\s*detailCode := 2.*?triggerType <> 0.*?bufferMode <> 1.*?expectedTriggerValueType.*?preTriggerSamples >= requestedCapacity.*?triggerOperator < 5.*?triggerValue <> 0.*?TriggerSignalOffset := triggerSignalOffset' 'ConfigureRecorder does not fail closed for double bank or fully validate and publish a D4 ring trigger configuration.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64 \+\s*\(\(\(TriggerSignalId shr 8\) and 0xFF\) - 1\) \* 36' 'AppendSnapshot does not bind trigger validity to the configured physical axis health image.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed.*?SessionEpoch = OwnerSessionEpoch.*?ClosedSessionEpoch := SessionEpoch.*?END_FUNCTION' 'LMCRecorderStore does not retain the closed owner epoch for Recorder adoption.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E49:.*?requestRecordId := \(pRequest \+ 8\)\^\$UDINT.*?requestBufferId := \(pRequest \+ 12\)\^\$UDINT.*?requestBootId := \(pRequest \+ 16\)\^\$UDINT.*?if requestRecordId = 0 then.*?requestBufferId <> 0.*?detailCode := 22.*?requestBootId <> DiagnosticsBootId.*?requestBootId <> CurrentDiagnosticsBootId.*?detailCode := 25.*?RecordId = 0.*?BufferId <> 0.*?detailCode := 22.*?state < LMC_RECORDER_ARMED.*?state > LMC_RECORDER_UPLOADING.*?ClosedSessionEpoch = 0.*?ClosedSessionEpoch <> OwnerSessionEpoch' 'AdoptRecorder 0x7E49 does not implement the fail-closed 0/0 active single-bank discovery sentinel.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E49:.*?if requestRecordId = 0 then.*?else\s*.*?requestRecordId <> RecordId.*?requestBufferId <> BufferId.*?detailCode := 22.*?requestBootId <> DiagnosticsBootId.*?requestBootId <> CurrentDiagnosticsBootId.*?state < LMC_RECORDER_ARMED.*?state > LMC_RECORDER_UPLOADING.*?ClosedSessionEpoch = 0.*?ClosedSessionEpoch <> OwnerSessionEpoch.*?end_if;\s*if detailCode = 0 then.*?OwnerSessionEpoch := CallerSessionEpoch.*?ClosedSessionEpoch := 0.*?\(pResponse \+ 20\)\^\$UDINT := RecordId.*?\(pResponse \+ 24\)\^\$UDINT := BufferId.*?\(pResponse \+ 28\)\^\$UDINT := OwnerSessionEpoch.*?\(pResponse \+ 32\)\^\$UINT := TO_UINT\(state\)' 'AdoptRecorder 0x7E49 no longer preserves exact-ID adoption or does not return the adopted active identity and new owner.'

$recorderProtocolCommands = [ordered]@{
    ConfigureRecorder = '0x7E40'
    StartRecorder = '0x7E41'
    TriggerRecorder = '0x7E42'
    StopRecorder = '0x7E43'
    ReadRecorderStatus = '0x7E44'
    ReadRecorderHeader = '0x7E45'
    ReadRecorderChunk = '0x7E46'
    ReleaseRecorderBuffer = '0x7E47'
    ReleaseRecorder = '0x7E48'
    AdoptRecorder = '0x7E49'
}
foreach ($recorderProtocolCommand in $recorderProtocolCommands.GetEnumerator()) {
    Assert-Match $protocol (
        'internal const ushort ' +
        [regex]::Escape($recorderProtocolCommand.Key) +
        ' = ' +
        [regex]::Escape($recorderProtocolCommand.Value) +
        ';') "C# recorder command $($recorderProtocolCommand.Key) has the wrong ID."
}

Assert-Match $protocol 'internal const ushort GetDiagnosticsCapabilities = 0x7E00;' 'C# diagnostics capability command ID is missing.'
Assert-Match $diagnosticsProtocol '(?s)GetDiagnosticsCapabilities\(uint requestId\).*?CreateRequest\(\s*LMC_CommandId\.GetDiagnosticsCapabilities,\s*0,\s*CommonRequestPayloadLength\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize, SchemaVersion\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize \+ 2, 0\).*?WriteUInt32\(buffer, LMC_Frame\.HeaderSize \+ 4, requestId\)' 'C# diagnostics capability common request builder is incomplete.'

$axisLookupBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x103C:.*?0x1042:').Value
if ([string]::IsNullOrWhiteSpace($axisLookupBlock)) {
    throw '0x103C axis lookup case was not found.'
}
if ($axisLookupBlock -match 'ObjectRegistryReady') {
    throw '0x103C axis lookup still depends on the aggregate object registry.'
}
if ([regex]::Matches($axisLookupBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9) {
    throw '0x103C axis lookup does not validate each axis client independently.'
}
if ([regex]::Matches($axisLookupBlock, '_GetObjName\(\s*pThis:=LMCAxis[1-9]\.pCmd').Count -ne 9) {
    throw '0x103C axis lookup does not refresh all nine connected object names on demand.'
}
if ([regex]::Matches($axisLookupBlock, '_memset\(dest:=#AxisObjectName[1-9]\[0\]').Count -ne 9) {
    throw '0x103C axis lookup does not clear every name buffer before discovery.'
}
Assert-Match $axisLookupBlock '(?s)objectNameLength := _GetObjName.*?objectNameLength > 0.*?objectNameLength <= 79.*?_stricmp' '0x103C axis lookup does not validate the discovered name length before a case-insensitive comparison.'
if ($axisLookupBlock -match '_strcmp') {
    throw '0x103C axis lookup still performs a case-sensitive object-name comparison.'
}

$groupLookupBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x1042:.*?0x202B:').Value
if ([string]::IsNullOrWhiteSpace($groupLookupBlock)) {
    throw '0x1042 group lookup case was not found.'
}
if ($groupLookupBlock -match 'ObjectRegistryReady') {
    throw '0x1042 group lookup still depends on the aggregate object registry.'
}
Assert-Match $groupLookupBlock 'IsClientConnected\(#LMCRobot\)' '0x1042 group lookup does not validate the robot client independently.'
Assert-Match $groupLookupBlock '(?s)_memset\(dest:=#GroupObjectName\[0\].*?_GetObjName\(\s*pThis:=LMCRobot\.pCmd.*?objectNameLength > 0.*?objectNameLength <= 79.*?_stricmp' '0x1042 group lookup does not safely refresh and compare the group name case-insensitively.'
if ($groupLookupBlock -match '_strcmp') {
    throw '0x1042 group lookup still performs a case-sensitive object-name comparison.'
}

$powerCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2023:.*?0x2024:').Value
Assert-Match $powerCaseBlock '(?s)\(Payload = 8\).*?\(RequestBuf\[8\]\$UDINT = 1\).*?\(RequestBuf\[12\] = 0\).*?\(RequestBuf\[12\] = 1\).*?\(RequestBuf\[13\] = 1\).*?\(RequestBuf\[14\] = 0\).*?\(RequestBuf\[15\] = 1\)' '0x2023 exact DINT payload validation is missing.'
Assert-Match $powerCaseBlock '(?s)if RequestBuf\[12\] = 1 then.*?PowerOn\(\);.*?else.*?PowerOff\(\);' '0x2023 PowerOn/PowerOff dispatch is missing.'

$powerOnBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOn.*?END_FUNCTION').Value
$powerOffBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOff.*?END_FUNCTION').Value
if ([regex]::Matches($powerOnBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($powerOnBlock, '\bLMCAxis[1-9]\.PowerOn\s*\(').Count -ne 9) {
    throw 'PowerOn does not validate and dispatch all nine LASAL axis clients.'
}
if ([regex]::Matches($powerOffBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($powerOffBlock, '\bLMCAxis[1-9]\.PowerOff\s*\(').Count -ne 9) {
    throw 'PowerOff does not validate and dispatch all nine LASAL axis clients.'
}

$resetCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2024:.*?0x2022:').Value
Assert-Match $resetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\).*?AxisReset\(\);' '0x2024 exact reset validation/dispatch is missing.'
$axisResetBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::AxisReset.*?END_FUNCTION').Value
if ([regex]::Matches($axisResetBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($axisResetBlock, '\bLMCAxis[1-9]\.QuitError\s*\(').Count -ne 9) {
    throw 'AxisReset does not validate and dispatch all nine LASAL axis clients.'
}

$stopCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2022:.*?0x2028:').Value
Assert-Match $stopCaseBlock '(?s)if Payload = 16 then.*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveStop\(\);' '0x2022 exact payload and semantic validation is missing.'
Assert-Match $stopCaseBlock '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[12\],\s*size:=4\);' '0x2022 does not read Jerk from request offset 12.'
$moveStopBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveStop.*?END_FUNCTION').Value
if ([regex]::Matches($moveStopBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($moveStopBlock, '\bLMCAxis[1-9]\.StopMove\s*\(').Count -ne 9) {
    throw 'MoveStop does not validate and dispatch all nine LASAL axis clients.'
}
if ([regex]::Matches($moveStopBlock, 'Jerk:=jer').Count -ne 9) {
    throw 'MoveStop does not forward the received Jerk to all nine LASAL axis clients.'
}

$readStatusCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x2028:.*?0x202E:').Value
if ([string]::IsNullOrWhiteSpace($readStatusCaseBlock)) {
    throw '0x2028 MsgPaser case was not found.'
}
Assert-Match $readStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\)' '0x2028 payload/reference/execute validation is missing.'
$readAxisStatusCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-9]\.ReadAxisStatus\s*\(').Count
$readAxisErrorCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-9]\.ReadAxisError\s*\(').Count
if ($readAxisStatusCalls -ne 9 -or $readAxisErrorCalls -ne 9) {
    throw "0x2028 CyWork client calls are incomplete: status=$readAxisStatusCalls error=$readAxisErrorCalls."
}
Assert-Match $readStatusCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*12;.*?Sendbuf\[8\]\$UDINT\s*:=\s*AxisStatusValue\$UDINT;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?Sendbuf\[16\]\$UINT\s*:=\s*AxisErrorValue\$UINT;.*?Sendbuf\[18\]\$UINT\s*:=\s*0;.*?udSize:=20' '0x2028 20-byte typed response framing is missing.'

$readPositionCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x202E:.*?0x209F:').Value
if ([string]::IsNullOrWhiteSpace($readPositionCaseBlock)) {
    throw '0x202E MsgPaser case was not found.'
}
Assert-Match $readPositionCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 0\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\)' '0x202E payload/reference validation is missing.'
$readPositionCalls = [regex]::Matches($readPositionCaseBlock, '\bLMCAxis[1-9]\.ReadPosition\s*\(').Count
if ($readPositionCalls -ne 9) {
    throw "0x202E CyWork client calls=$readPositionCalls, expected 9."
}
Assert-Match $readPositionCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*8;.*?Sendbuf\[8\]\$DINT\s*:=\s*ReadPos;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?udSize:=16' '0x202E 16-byte typed response framing is missing.'

$moveShortestCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x209F:.*?0x20A0:').Value
$moveRelativeCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20A0:.*?0x20A2:').Value
$moveVelocityCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20A2:.*?0x20D2:').Value
foreach ($entry in @(
    @{ Name = '0x209F'; Block = $moveShortestCaseBlock },
    @{ Name = '0x20A0'; Block = $moveRelativeCaseBlock })) {
    Assert-Match $entry.Block '(?s)if Payload = 32 then.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' "$($entry.Name) exact payload and shortest-only validation is missing."
    Assert-Match $entry.Block '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[24\],\s*size:=4\);' "$($entry.Name) does not read Jerk from request offset 24."
}
Assert-Match $moveVelocityCaseBlock '(?s)if Payload = 24 then.*?\(dec = 0\).*?\(Exec = 1\).*?\(dir = 1\).*?\(velo >= 0\).*?\(dir = 3\).*?\(velo <= 0\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' '0x20A2 exact payload, direction, and execute validation is missing.'
Assert-Match $moveVelocityCaseBlock '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[20\],\s*size:=4\);' '0x20A2 does not read Jerk from request offset 20.'

$groupMembersCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x20D2:.*?0x2047:').Value
if ([string]::IsNullOrWhiteSpace($groupMembersCaseBlock)) {
    throw '0x20D2 group-members case was not found.'
}
Assert-Match $groupMembersCaseBlock 'ObjectRegistryReady\s*:=\s*FALSE' '0x20D2 does not invalidate the object registry before refreshing it.'
if ([regex]::Matches($groupMembersCaseBlock, 'IsClientConnected\(#(?:LMCAxis[1-9]|LMCRobot)\)').Count -ne 10) {
    throw '0x20D2 does not validate all ten current LASAL client connections.'
}
if ([regex]::Matches($groupMembersCaseBlock, '_GetObjName\(\s*pThis:=(?:LMCAxis[1-9]|LMCRobot)\.pCmd').Count -ne 10) {
    throw '0x20D2 does not refresh all ten object names on demand.'
}
if ([regex]::Matches($groupMembersCaseBlock, '_memset\(dest:=#(?:AxisObjectName[1-9]|GroupObjectName)\[0\]').Count -ne 10) {
    throw '0x20D2 does not clear all ten object-name buffers before discovery.'
}
Assert-Match $groupMembersCaseBlock '(?s)objectNameLength = 0.*?objectNameLength > 79.*?ObjectRegistryReady := FALSE' '0x20D2 does not reject empty or overlength discovered names.'
foreach ($entry in @(
    @{ Offset = 16; Value = 5 },
    @{ Offset = 18; Value = 6 },
    @{ Offset = 20; Value = 7 },
    @{ Offset = 22; Value = 8 },
    @{ Offset = 24; Value = 9 })) {
    Assert-Match $groupMembersCaseBlock (
        'Sendbuf\[' + $entry.Offset + '\]\$UINT\s*:=\s*' +
        $entry.Value + ';') (
        "0x20D2 axis $($entry.Value) reference slot is missing.")
}
foreach ($entry in @(
    @{ Offset = 48; Value = 4 },
    @{ Offset = 50; Value = 5 },
    @{ Offset = 52; Value = 6 },
    @{ Offset = 54; Value = 7 },
    @{ Offset = 56; Value = 8 })) {
    Assert-Match $groupMembersCaseBlock (
        'Sendbuf\[' + $entry.Offset + '\]\$UINT\s*:=\s*' +
        $entry.Value + ';') (
        "0x20D2 axis $($entry.Value + 1) device-ID slot is missing.")
}
foreach ($entry in @(
    @{ Offset = 396; Axis = 5 },
    @{ Offset = 476; Axis = 6 },
    @{ Offset = 556; Axis = 7 },
    @{ Offset = 636; Axis = 8 },
    @{ Offset = 716; Axis = 9 })) {
    Assert-Match $groupMembersCaseBlock (
        '(?s)pThis:=LMCAxis' + $entry.Axis + '\.pCmd,.*?' +
        'MemCpy\(dest:=#Sendbuf\[' + $entry.Offset +
        '\],\s*source:=#AxisObjectName1' +
        '\[0\],\s*size:=80\)') (
        "0x20D2 axis $($entry.Axis) shared-buffer name slot is missing.")
}
Assert-Match $groupMembersCaseBlock 'Sendbuf\[1356\]\s*:=\s*9;' '0x20D2 AxisCount is not 9.'

$moveAbsBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveAbs.*?END_FUNCTION').Value
if ([regex]::Matches($moveAbsBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveShortestWay\s*\(').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveRelative\s*\(').Count -ne 9 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-9]\.MoveEndless\s*\(').Count -ne 9) {
    throw 'MoveAbs does not dispatch all three approved motion commands to all nine LASAL axis clients.'
}
if ([regex]::Matches($moveAbsBlock, 'Jerk:=jer').Count -ne 27) {
    throw 'MoveAbs does not forward the received Jerk through all 27 axis motion dispatch paths.'
}

$groupEnableCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2047:.*?0x2048:').Value
$groupDisableCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2048:.*?0x2049:').Value
Assert-Match $groupEnableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?LMCRobot\.LockProfile\(.*?Axis1:=1.*?Axis4:=1.*?Axis5:=0.*?Axis9:=0.*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?udSize:=16' '0x2047 four-axis Cartesian profile-lock validation/dispatch/ACK is missing.'
Assert-Match $groupDisableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?GroupReadInPosition <> 0.*?LMCRobot\.UnlockProfile\(\).*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?udSize:=16' '0x2048 group profile-unlock standstill validation/dispatch/ACK is missing.'

$groupPowerOnCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x204A:.*?0x204B:').Value
$groupPowerOffCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x204B:.*?0x2085:').Value
Assert-Match $groupPowerOnCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOn\(Mode:=_ACTIVE\).*?udSize:=16' '0x204A group-power-on validation/RobotOn/ACK is missing.'
if ($groupPowerOnCaseBlock -match 'GroupKinematicReady\s*=\s*TRUE') {
    throw '0x204A group-power-on is incorrectly gated by kinematic configuration.'
}
Assert-Match $groupPowerOffCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOff\(\).*?udSize:=16' '0x204B group-power-off validation/RobotOff/ACK is missing.'
foreach ($entry in @(
    @{ Name = '0x2047'; Block = $groupEnableCaseBlock },
    @{ Name = '0x2048'; Block = $groupDisableCaseBlock },
    @{ Name = '0x204A'; Block = $groupPowerOnCaseBlock },
    @{ Name = '0x204B'; Block = $groupPowerOffCaseBlock })) {
    Assert-Match $entry.Block '(?s)\(GroupReadErrorId >= -32768\).*?\(GroupReadErrorId <= 32767\).*?Sendbuf\[14\]\$INT\s*:=\s*GroupReadErrorId\$INT;.*?else.*?Sendbuf\[14\]\$INT\s*:=\s*-6' "$($entry.Name) does not preserve signed 16-bit LASAL/disconnected errors before overflow mapping."
    if ($entry.Block -match 'GroupReadErrorId\$UDINT\s+and\s+0xFFFF0000') {
        throw "$($entry.Name) still sign-extends negative DINT errors into overflow error -6."
    }
}

$groupStatusCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2045:.*?0x2051:').Value
Assert-Match $groupStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\).*?IsClientConnected\(#LMCRobot\).*?GroupReadStatus\(\);' '0x2045 group-status validation/dispatch is missing.'
$groupReadStatusBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::GroupReadStatus.*?END_FUNCTION').Value
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.RobotIsOn\(\).*?powerIsOn <> 0.*?GroupReadState := GroupReadState or 0x00040000' 'GroupReadStatus project-local power-ready mapping is missing.'
Assert-Match $groupReadStatusBlock '(?s)ReadProfileParameter\(.*?_LMCPROF_LockState.*?powerIsOn <> 0.*?profileLocked = TRUE.*?GroupReadInPosition <> 0.*?GroupReadState := GroupReadState or 0x00020000.*?profileLocked = FALSE.*?GroupReadState := GroupReadState or 0x00010000' 'GroupReadStatus locked-standby/unlocked-disabled mapping is missing.'
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.ReadRobotParameter\(ParNo:=_ROBOT_STATE, Mode:=0\).*?robotState = _ROBOT_ERROR\$DINT.*?LMCRobot\.ReadProfileError\(\).*?GroupReadErrorId := profileErrorInfo\.ErrorNo\$DINT' 'GroupReadStatus robot/profile error propagation or enum-to-DINT typing is missing.'
Assert-Match $groupReadStatusBlock '(?s)if GroupReadErrorId = 0 then.*?GroupReadErrorId := -6;.*?robotState < _ROBOT_PASSIVE\$DINT.*?robotState > _ROBOT_MODE_CHANGE\$DINT.*?GroupReadErrorId := -6' 'GroupReadStatus false-success guards are missing.'
Assert-Match $groupReadStatusBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*0x000C;.*?Sendbuf\[8\]\$UDINT\s*:=\s*GroupReadState;.*?Sendbuf\[16\]\$UINT\s*:=\s*GroupReadErrorId\$UINT;.*?SendData' '0x2045 20-byte typed response framing is missing.'
if ($groupReadStatusBlock -match 'GroupMoveRetCode') {
    throw 'GroupReadStatus still reports stale GroupMoveRetCode state.'
}

$groupResetCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2049:.*?0x204A:').Value
Assert-Match $groupResetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.AxQuitError\(AxisNo:=0\).*?AxisCommandStatus := 0;.*?AxisCommandErrorId := 0;.*?udSize:=16' '0x2049 axis-error reset validation/AxQuitError dispatch/ACK is missing.'

$groupStopCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2085:.*?0x20A4:').Value
Assert-Match $groupStopCaseBlock '(?s)if Payload = 16 then.*?RequestBuf\[8\].*?RequestBuf\[12\].*?\(bufMode = 1\).*?\(GroupExecute = 1\).*?\(GroupDecel >= 0\).*?\(GroupJerk >= 0\).*?LMCRobot\.StopMove\(\s*Mode:=3, Decel:=GroupDecel, Jerk:=GroupJerk\).*?udSize:=16' '0x2085 group stop validation/StopMove dispatch/ACK is missing.'

$groupMoveCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20A4:.*?0x2045:').Value
Assert-Match $groupMoveCaseBlock '(?s)\(Payload = 96\).*?\(AxisRef = 0x0100\).*?source:=#RequestBuf\[72\].*?source:=#RequestBuf\[76\].*?source:=#RequestBuf\[80\].*?source:=#RequestBuf\[84\].*?source:=#RequestBuf\[88\].*?source:=#RequestBuf\[92\].*?source:=#RequestBuf\[96\].*?source:=#RequestBuf\[100\]' '0x20A4 DINT field offsets are incomplete.'
Assert-Match $groupMoveCaseBlock '(?s)for kinIndex := 4 to 15 do.*?GroupCommandInputValid := FALSE.*?end_for' '0x20A4 does not reject nonzero positions outside the four-axis topology.'
Assert-Match $groupMoveCaseBlock '(?s)\(GroupCoordSystem = 0\).*?\(GroupTransitionModeInput = 0\).*?\(GroupTransitionModeInput = 2\).*?\(bufMode = 1\).*?\(bufMode = 2\).*?MoveLinearAbsEx\(\);' '0x20A4 approved coordinate/transition/buffer validation is missing.'
$groupMoveBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveLinearAbsEx.*?END_FUNCTION').Value
Assert-Match $groupMoveBlock '(?s)GroupCommandInputValid = TRUE.*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotIsOn\(\).*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?profileLocked = TRUE.*?LMCRobot\.MoveLinearCoord\(.*?CmdConfig:=GroupCommandConfig.*?CoordSystem:=0.*?Jerk:=GroupJerk.*?udSize:=16' 'MoveLinearAbsEx does not gate and dispatch the validated configured/powered/locked command.'
Assert-Match $groupMoveBlock '(?s)GroupMoveRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[12\]\$UINT := 0;.*?else.*?Sendbuf\[12\]\$UINT := 1;' 'MoveLinearAbsEx does not gate success on the MotionLib return code.'

$groupPositionCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2051:.*?0x20E7:').Value
Assert-Match $groupPositionCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(GroupCoordSystem >= 0\).*?\(GroupCoordSystem <= 3\).*?\(GroupExecute = 1\).*?LMCRobot\.GetRobotPosition\(.*?Mode:=_ACTPOS_APPUNITS.*?CoordSystem:=0.*?pPositions:=#GroupReadPos' '0x2051 static identity position mapping is missing.'
Assert-Match $groupPositionCaseBlock '(?s)GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[2\]\$UINT\s*:=\s*68;.*?else.*?Sendbuf\[2\]\$UINT\s*:=\s*4;' '0x2051 does not gate the typed success payload on the MotionLib return code.'
Assert-Match $groupPositionCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*68;.*?MemCpy\(dest:=#Sendbuf\[8\], source:=#GroupReadPos, size:=36\).*?Sendbuf\[72\]\$UINT\s*:=\s*0x4000;.*?udSize:=76' '0x2051 68-byte DINT position response is missing.'

$kinCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20E7:.*?else\s*_memset').Value
Assert-Match $kinCaseBlock '(?s)kinValid := \(Payload = 1320\).*?for kinIndex := 0 to 3 do.*?0x3FF00000.*?RequestBuf\[648\]\$DINT <> 4.*?RequestBuf\[1316\]\$DINT <> 2.*?RequestBuf\[1320\]\$DINT <> 1' '0x20E7 identity-shift Cartesian4 payload validation is missing.'
Assert-Match $kinCaseBlock '(?s)IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?GroupKinematicReady := TRUE;.*?GroupReadErrorId := 0;' '0x20E7 static four-axis mapping registration is missing.'
if ($kinCaseBlock -match 'LockProfile|UnlockProfile|RobotOn|RobotOff') {
    throw '0x20E7 mapping validation still changes profile-lock or group-power state.'
}
Assert-Match $kinCaseBlock '(?s)if GroupReadErrorId = 0 then.*?Sendbuf\[8\]\$UINT := 0;.*?else.*?Sendbuf\[8\]\$UINT := 1;' '0x20E7 does not gate acknowledgement success on mapping validation.'
Assert-Match $kinCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*4;.*?Sendbuf\[8\]\$UINT.*?Sendbuf\[10\]\$INT.*?udSize:=12' '0x20E7 short acknowledgement framing is missing.'

$responseBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::Response.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($responseBlock)) {
    throw 'TCPMotionInterface.Response implementation was not found.'
}
if ($responseBlock -match '\bMsgPaser\s*\(') {
    throw 'Response still calls MsgPaser directly.'
}
if ($responseBlock -match '\bSendData\s*\(') {
    throw 'Response still performs TCP send work.'
}
if ($responseBlock -match '\b(?:LMCAxis[1-9]|LMCRobot)\s*\.') {
    throw 'Response still performs a LASAL motion client call.'
}

$motionCyWorkBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($motionCyWorkBlock)) {
    throw 'TCPMotionInterface.CyWork implementation was not found.'
}
if ($motionCyWorkBlock -match '_GetObjName|_strlen|_stricmp|_strcmp') {
    throw 'CyWork still performs periodic object-name discovery or string comparison.'
}
Assert-Match $st 'PendingClosedSessionEpoch\s*:\s*UDINT' 'TCPMotionInterface pending closed-session epoch storage is missing.'
Assert-Match $motionCyWorkBlock '(?s)PendingClosedSessionEpoch <> 0.*?IsClientConnected\(#Diagnostics\).*?Diagnostics\.NotifySessionClosed\(\s*SessionEpoch:=PendingClosedSessionEpoch\).*?PendingClosedSessionEpoch := 0.*?currentEpoch := SessionEpoch' 'TCPMotionInterface.CyWork does not flush the pending closed epoch to LMCDiagnosticsService before processing requests.'
$closedEpochCaptureCount = [regex]::Matches(
    $st,
    '(?s)if \(SessionEpoch <> 0\)\s*&\s*\(PendingClosedSessionEpoch = 0\) then\s*PendingClosedSessionEpoch := SessionEpoch;\s*end_if;\s*SessionEpoch \+= 1').Count
if ($closedEpochCaptureCount -ne 3) {
    throw "TCPMotionInterface first-wins closed-session capture count is $closedEpochCaptureCount, expected three disconnect/send/close paths."
}
Assert-Match $motionCyWorkBlock '(?s)RequestQueue\[QueueReadIndex\$DINT\]\.State\s*=\s*TCPMI_QUEUE_READY.*?State\s*:=\s*TCPMI_QUEUE_ACTIVE.*?MemCpy.*?State\s*:=\s*TCPMI_QUEUE_FREE' 'CyWork queue READY/ACTIVE/FREE transition is missing.'
Assert-Match $motionCyWorkBlock '(?s)CommandID\s*:=\s*TO_DINT\(ActiveRequest\.CommandId\);.*?AxisRef\s*:=\s*TO_DINT\(ActiveRequest\.Reference\);.*?Payload\s*:=\s*TO_DINT\(ActiveRequest\.PayloadLength\);.*?MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE' 'CyWork does not numerically widen, execute, and release one active request.'
Assert-Match $motionCyWorkBlock '(?s)MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE.*?if IsClientConnected\(#Diagnostics\) then\s*Diagnostics\.ProcessOperations\(\);\s*end_if' 'TCPMotionInterface.CyWork does not safely advance D5 operations after request processing.'
if ($motionCyWorkBlock -match 'ActiveRequest\.(?:CommandId|Reference|PayloadLength)\$DINT') {
    throw 'CyWork reinterprets a 16-bit request field as a 32-bit DINT instead of using numeric conversion.'
}

$msgParserCallCount = [regex]::Matches($st, '(?m)^\s*MsgPaser\(\);\s*$').Count
if ($msgParserCallCount -ne 1) {
    throw "MsgPaser call count is $msgParserCallCount, expected one CyWork caller."
}

Assert-Match $responseBlock '(?s)State\s*=\s*TCPMI_QUEUE_FREE.*?State\s*:=\s*TCPMI_QUEUE_WRITING.*?State\s*:=\s*TCPMI_QUEUE_READY' 'Response queue FREE/WRITING/READY transition is missing.'

$sendDataBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::SendData.*?END_FUNCTION').Value
Assert-Match $sendDataBlock '_TCPIPServerInterface::SendData' 'TCPMotionInterface.SendData base call is missing.'
Assert-Match $sendDataBlock 'if dRetcode <> udSize\$DINT then' 'Partial/failed send check is missing.'
Assert-Match $sendDataBlock 'IngressFaultCloseRequired\s*:=\s*TRUE' 'Partial send quarantine is missing.'
Assert-Match $sendDataBlock 'SessionEpoch\s*\+=\s*1' 'Partial send does not invalidate the session epoch.'
Assert-Match $st 'vmt\.UserFcts\[2\]\s*:=\s*#SendData\(\)' 'TCPMotionInterface.SendData override is not registered.'

$tcpRtWorkBlock = [regex]::Match(
    $tcpServerRt,
    '(?s)FUNCTION VIRTUAL GLOBAL _TCPIPServer_RT::RtWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($tcpRtWorkBlock)) {
    throw '_TCPIPServer_RT.RtWork implementation was not found.'
}
if ($tcpRtWorkBlock -match '\bCyclicCall\s*\(') {
    throw '_TCPIPServer_RT.RtWork still owns TCP transport work.'
}

if ($st -match '(?m)^\s*0x208[1-4]\s*:') {
    throw 'Legacy 0x2081..0x2084 handler is active.'
}

$upperBitMappings = [regex]::Matches($st, 'AxisCommandState\$UDINT\s+and\s+0xFFFF0000').Count
if ($upperBitMappings -ne 4) {
    throw "32-bit axis error truncation guards=$upperBitMappings, expected 4."
}

Assert-Match $st 'AxisObjectName1\s*:\s*ARRAY \[0\.\.255\] OF CHAR' 'LASAL object-name buffer is not 256 bytes.'
if ($st -match 'AxisObjectName[5-9]\s*:\s*ARRAY') {
    throw 'Axes 5..9 must reuse an IDE-registered object-name buffer instead of adding CodeGenerator-only class variables.'
}
Assert-Match $st '(?s)AxisCommandInputValid\s*:=.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\)' 'Shortest-only axis direction validation is missing.'
Assert-Match $st '(?s)\(dec = 0\).*?\(Exec = 1\)' 'MoveVelocity deceleration/execute validation is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(\s*buffer,\s*HeaderSize \+ 92,\s*options\.Execute \? 1 : 0\s*\);' 'C# group execute option is not serialized at payload offset 92.'

if ($SourceOnly) {
    Write-Host 'PASS LASAL.StaticContract.SourceOnly (CyWork queue, diagnostics D1-D4 active contract, gated derived D5 SDO executor/ticket contract, recorder bank, and session-close wiring)'
}
else {
    Write-Host 'PASS LASAL.StaticContract (CyWork queue, diagnostics D1-D4 active contract, gated derived D5 SDO executor/ticket and four-axis network contract, recorder wiring, and generated metadata/tables)'
}

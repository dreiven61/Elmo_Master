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
$motionNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$motionNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\ONE_Motion_Network_Table.st'
$tcpServerRtPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\_TCPIPServer_RT\_TCPIPServer_RT.st'
$classDbPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'

$st = Get-Content -Raw -LiteralPath $stPath
$commNetwork = Get-Content -Raw -LiteralPath $commNetworkPath
$commNetworkTable = Get-Content -Raw -LiteralPath $commNetworkTablePath
$motionNetwork = Get-Content -Raw -LiteralPath $motionNetworkPath
$motionNetworkTable = Get-Content -Raw -LiteralPath $motionNetworkTablePath
$tcpServerRt = Get-Content -Raw -LiteralPath $tcpServerRtPath
$classDbText = [Text.Encoding]::ASCII.GetString(
    [IO.File]::ReadAllBytes($classDbPath))
$protocol = Get-Content -Raw -LiteralPath $protocolPath

[xml]$commNetworkXml = $commNetwork
[xml]$motionNetworkXml = $motionNetwork

Assert-Match $st '20\$UINT,\s*11\$UINT,\s*0\$UINT' 'TCPMotionInterface generated client count is not 11.'

$clientEntries = [regex]::Matches(
    $st,
    '\(::TCPMotionInterface\.(LMCAxis[1-9]|LMCRobot|_StdLib)\.pCh\)\$UINT').Count
if ($clientEntries -ne 11) {
    throw "TCPMotionInterface generated client entry count is $clientEntries, expected 11."
}

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
Assert-Match $motionCyWorkBlock '(?s)RequestQueue\[QueueReadIndex\$DINT\]\.State\s*=\s*TCPMI_QUEUE_READY.*?State\s*:=\s*TCPMI_QUEUE_ACTIVE.*?MemCpy.*?State\s*:=\s*TCPMI_QUEUE_FREE' 'CyWork queue READY/ACTIVE/FREE transition is missing.'
Assert-Match $motionCyWorkBlock '(?s)CommandID\s*:=\s*TO_DINT\(ActiveRequest\.CommandId\);.*?AxisRef\s*:=\s*TO_DINT\(ActiveRequest\.Reference\);.*?Payload\s*:=\s*TO_DINT\(ActiveRequest\.PayloadLength\);.*?MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE' 'CyWork does not numerically widen, execute, and release one active request.'
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
    Write-Host 'PASS LASAL.StaticContract.SourceOnly (CyWork-only queue, no RT task/mailbox, axis and complete published group API)'
}
else {
    Write-Host 'PASS LASAL.StaticContract (CyWork-only active command contract, 1320-byte staging, and ordinary TCP server network)'
}

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
$networkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$networkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\ONE_Motion_Network_Table.st'
$tcpServerRtPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\_TCPIPServer_RT\_TCPIPServer_RT.st'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'

$st = Get-Content -Raw -LiteralPath $stPath
$network = Get-Content -Raw -LiteralPath $networkPath
$networkTable = Get-Content -Raw -LiteralPath $networkTablePath
$tcpServerRt = Get-Content -Raw -LiteralPath $tcpServerRtPath
$protocol = Get-Content -Raw -LiteralPath $protocolPath

[xml]$networkXml = $network

Assert-Match $st '20\$UINT,\s*6\$UINT,\s*0\$UINT' 'TCPMotionInterface generated client count is not 6.'

$clientEntries = [regex]::Matches(
    $st,
    '\(::TCPMotionInterface\.(LMCAxis1|LMCAxis2|LMCAxis3|LMCAxis4|LMCRobot|_StdLib)\.pCh\)\$UINT').Count
if ($clientEntries -ne 6) {
    throw "TCPMotionInterface generated client entry count is $clientEntries, expected 6."
}

foreach ($axisNumber in 1..4) {
    $clientName = "LMCAxis$axisNumber"
    $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
        '.*' +
        [regex]::Escape("_LMCAxis$axisNumber.Control")
    Assert-Match $network $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link."
}

if (-not $SourceOnly) {
    $interfaceObject = $networkXml.SelectSingleNode("//Object[@Name='TCPMotionInterface1']")
    $serverObject = $networkXml.SelectSingleNode("//Object[@Name='_TCPIPServer1']")
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
    Assert-Match $network 'TCPMotionInterface1\._TCPIPServer.*_TCPIPServer1\.Control' 'TCPMotionInterface1 is not connected to the ordinary TCP server.'
    Assert-Match $networkTable '"MaxConnections",\s*TO_UDINT\(1\),//\|Motion_Network\._TCPIPServer1\.MaxConnections;' '_TCPIPServer1 generated MaxConnections value is stale.'
    $generatedTaskRefs = [regex]::Matches($networkTable, '//TCPMOTIONINTERFACE1').Count
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
Assert-Match $st 'if usPayloadLength > 96 then' 'LASAL queue payload bound is missing.'
Assert-Match $st 'IngressDiscardRemaining\s*:=\s*udFrameSize - ReceiveFill' 'Oversize frame bounded discard is missing.'
Assert-Match $st 'GroupMoveRetCode\s*:=\s*_LMCPROF_MOVECMD_ERROR' 'Group move false-success guard is missing.'
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
$blockedGuard = [regex]::Match(
    $preCaseBlock,
    '(?s)if \(CommandID = 0x2049\).*?Sendbuf\[14\]\$INT\s*:=\s*-5;.*?RETURN;.*?end_if;').Value
if ([string]::IsNullOrWhiteSpace($blockedGuard)) {
    throw 'The pre-case client-call -5 guard with RETURN is missing.'
}
foreach ($commandId in @('2049', '2085', '20A4')) {
    Assert-Match $blockedGuard "CommandID = 0x$commandId" "Blocked command 0x$commandId is missing from the pre-case guard."
}
foreach ($commandId in @('2023', '2024', '2022', '2028', '202E', '209F', '20A0', '20A2', '2047', '2048', '2045')) {
    if ($blockedGuard -match "CommandID = 0x$commandId") {
        throw "Active command 0x$commandId is still blocked before its CyWork handler."
    }
}

$powerCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2023:.*?0x2024:').Value
Assert-Match $powerCaseBlock '(?s)\(Payload = 8\).*?\(RequestBuf\[8\]\$UDINT = 1\).*?\(RequestBuf\[12\] = 0\).*?\(RequestBuf\[12\] = 1\).*?\(RequestBuf\[13\] = 1\).*?\(RequestBuf\[14\] = 0\).*?\(RequestBuf\[15\] = 1\)' '0x2023 exact DINT payload validation is missing.'
Assert-Match $powerCaseBlock '(?s)if RequestBuf\[12\] = 1 then.*?PowerOn\(\);.*?else.*?PowerOff\(\);' '0x2023 PowerOn/PowerOff dispatch is missing.'

$powerOnBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOn.*?END_FUNCTION').Value
$powerOffBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::PowerOff.*?END_FUNCTION').Value
if ([regex]::Matches($powerOnBlock, 'IsClientConnected\(#LMCAxis[1-4]\)').Count -ne 4 -or
    [regex]::Matches($powerOnBlock, '\bLMCAxis[1-4]\.PowerOn\s*\(').Count -ne 4) {
    throw 'PowerOn does not validate and dispatch all four LASAL axis clients.'
}
if ([regex]::Matches($powerOffBlock, 'IsClientConnected\(#LMCAxis[1-4]\)').Count -ne 4 -or
    [regex]::Matches($powerOffBlock, '\bLMCAxis[1-4]\.PowerOff\s*\(').Count -ne 4) {
    throw 'PowerOff does not validate and dispatch all four LASAL axis clients.'
}

$resetCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2024:.*?0x2022:').Value
Assert-Match $resetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef >= 1\).*?\(AxisRef <= 4\).*?AxisReset\(\);' '0x2024 exact reset validation/dispatch is missing.'
$axisResetBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::AxisReset.*?END_FUNCTION').Value
if ([regex]::Matches($axisResetBlock, 'IsClientConnected\(#LMCAxis[1-4]\)').Count -ne 4 -or
    [regex]::Matches($axisResetBlock, '\bLMCAxis[1-4]\.QuitError\s*\(').Count -ne 4) {
    throw 'AxisReset does not validate and dispatch all four LASAL axis clients.'
}

$stopCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2022:.*?0x2028:').Value
Assert-Match $stopCaseBlock '(?s)if Payload = 16 then.*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveStop\(\);' '0x2022 exact payload and semantic validation is missing.'
$moveStopBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveStop.*?END_FUNCTION').Value
if ([regex]::Matches($moveStopBlock, 'IsClientConnected\(#LMCAxis[1-4]\)').Count -ne 4 -or
    [regex]::Matches($moveStopBlock, '\bLMCAxis[1-4]\.StopMove\s*\(').Count -ne 4) {
    throw 'MoveStop does not validate and dispatch all four LASAL axis clients.'
}

$readStatusCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x2028:.*?0x202E:').Value
if ([string]::IsNullOrWhiteSpace($readStatusCaseBlock)) {
    throw '0x2028 MsgPaser case was not found.'
}
Assert-Match $readStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef >= 1\).*?\(AxisRef <= 4\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\)' '0x2028 payload/reference/execute validation is missing.'
$readAxisStatusCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-4]\.ReadAxisStatus\s*\(').Count
$readAxisErrorCalls = [regex]::Matches($readStatusCaseBlock, '\bLMCAxis[1-4]\.ReadAxisError\s*\(').Count
if ($readAxisStatusCalls -ne 4 -or $readAxisErrorCalls -ne 4) {
    throw "0x2028 CyWork client calls are incomplete: status=$readAxisStatusCalls error=$readAxisErrorCalls."
}
Assert-Match $readStatusCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*12;.*?Sendbuf\[8\]\$UDINT\s*:=\s*AxisStatusValue\$UDINT;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?Sendbuf\[16\]\$UINT\s*:=\s*AxisErrorValue\$UINT;.*?Sendbuf\[18\]\$UINT\s*:=\s*0;.*?udSize:=20' '0x2028 20-byte typed response framing is missing.'

$readPositionCaseBlock = [regex]::Match(
    $msgParserBlock,
    '(?s)0x202E:.*?0x209F:').Value
if ([string]::IsNullOrWhiteSpace($readPositionCaseBlock)) {
    throw '0x202E MsgPaser case was not found.'
}
Assert-Match $readPositionCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 0\).*?\(AxisRef >= 1\).*?\(AxisRef <= 4\)' '0x202E payload/reference validation is missing.'
$readPositionCalls = [regex]::Matches($readPositionCaseBlock, '\bLMCAxis[1-4]\.ReadPosition\s*\(').Count
if ($readPositionCalls -ne 4) {
    throw "0x202E CyWork client calls=$readPositionCalls, expected 4."
}
Assert-Match $readPositionCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*8;.*?Sendbuf\[8\]\$DINT\s*:=\s*ReadPos;.*?Sendbuf\[12\]\$UINT\s*:=\s*AxisCommandStatus;.*?Sendbuf\[14\]\$INT\s*:=\s*AxisCommandErrorId;.*?udSize:=16' '0x202E 16-byte typed response framing is missing.'

$moveShortestCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x209F:.*?0x20A0:').Value
$moveRelativeCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20A0:.*?0x20A2:').Value
$moveVelocityCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x20A2:.*?0x20D2:').Value
foreach ($entry in @(
    @{ Name = '0x209F'; Block = $moveShortestCaseBlock },
    @{ Name = '0x20A0'; Block = $moveRelativeCaseBlock })) {
    Assert-Match $entry.Block '(?s)if Payload = 32 then.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' "$($entry.Name) exact payload and shortest-only validation is missing."
}
Assert-Match $moveVelocityCaseBlock '(?s)if Payload = 24 then.*?\(dec = 0\).*?\(Exec = 1\).*?\(dir = 1\).*?\(velo >= 0\).*?\(dir = 3\).*?\(velo <= 0\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' '0x20A2 exact payload, direction, and execute validation is missing.'

$moveAbsBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveAbs.*?END_FUNCTION').Value
if ([regex]::Matches($moveAbsBlock, 'IsClientConnected\(#LMCAxis[1-4]\)').Count -ne 4 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-4]\.MoveShortestWay\s*\(').Count -ne 4 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-4]\.MoveRelative\s*\(').Count -ne 4 -or
    [regex]::Matches($moveAbsBlock, '\bLMCAxis[1-4]\.MoveEndless\s*\(').Count -ne 4) {
    throw 'MoveAbs does not dispatch all three approved motion commands to all four LASAL axis clients.'
}

$groupEnableCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2047:.*?0x2048:').Value
$groupDisableCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2048:.*?0x2049, 0x2085:').Value
Assert-Match $groupEnableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOn\(Mode:=_ACTIVE\).*?udSize:=16' '0x2047 group-enable validation/dispatch/ACK is missing.'
Assert-Match $groupDisableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotOff\(\).*?udSize:=16' '0x2048 group-disable validation/dispatch/ACK is missing.'
foreach ($entry in @(
    @{ Name = '0x2047'; Block = $groupEnableCaseBlock },
    @{ Name = '0x2048'; Block = $groupDisableCaseBlock })) {
    Assert-Match $entry.Block '(?s)\(GroupReadErrorId >= -32768\).*?\(GroupReadErrorId <= 32767\).*?Sendbuf\[14\]\$INT\s*:=\s*GroupReadErrorId\$INT;.*?else.*?Sendbuf\[14\]\$INT\s*:=\s*-6' "$($entry.Name) does not preserve signed 16-bit LASAL/disconnected errors before overflow mapping."
    if ($entry.Block -match 'GroupReadErrorId\$UDINT\s+and\s+0xFFFF0000') {
        throw "$($entry.Name) still sign-extends negative DINT errors into overflow error -6."
    }
}

$groupStatusCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2045:.*?0x2051:').Value
Assert-Match $groupStatusCaseBlock '(?s)\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(PayloadReference = AxisRef\).*?\(Exec = 1\).*?IsClientConnected\(#LMCRobot\).*?GroupReadStatus\(\);' '0x2045 group-status validation/dispatch is missing.'
$groupReadStatusBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::GroupReadStatus.*?END_FUNCTION').Value
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.ProfileInPosition\(Mode:=_LMCPROF_ProfileFinished\).*?GroupReadState := GroupReadState or 0x00020000' 'GroupReadStatus in-position mapping is missing.'
Assert-Match $groupReadStatusBlock '(?s)LMCRobot\.ReadRobotParameter\(ParNo:=_ROBOT_STATE, Mode:=0\).*?robotState = _ROBOT_ERROR\$DINT.*?LMCRobot\.ReadProfileError\(\).*?GroupReadErrorId := profileErrorInfo\.ErrorNo\$DINT' 'GroupReadStatus robot/profile error propagation or enum-to-DINT typing is missing.'
Assert-Match $groupReadStatusBlock '(?s)if GroupReadErrorId = 0 then.*?GroupReadErrorId := -6;.*?robotState < _ROBOT_PASSIVE\$DINT.*?robotState > _ROBOT_MODE_CHANGE\$DINT.*?GroupReadErrorId := -6' 'GroupReadStatus false-success guards are missing.'
Assert-Match $groupReadStatusBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*0x000C;.*?Sendbuf\[8\]\$UDINT\s*:=\s*GroupReadState;.*?Sendbuf\[16\]\$UINT\s*:=\s*GroupReadErrorId\$UINT;.*?SendData' '0x2045 20-byte typed response framing is missing.'
if ($groupReadStatusBlock -match 'GroupMoveRetCode') {
    throw 'GroupReadStatus still reports stale GroupMoveRetCode state.'
}

$setCoordinatesCaseBlock = [regex]::Match($msgParserBlock, '(?s)0x2051:.*?else\s*_memset').Value
Assert-Match $setCoordinatesCaseBlock '(?s)Sendbuf\[0\]\$UINT\s*:=\s*1;.*?Sendbuf\[2\]\$UINT\s*:=\s*4;.*?Sendbuf\[10\]\$INT\s*:=\s*-5;.*?udSize:=12' '0x2051 does not return the deterministic unsupported short ACK.'

$oversizeBlock = [regex]::Match($st, '(?s)if usPayloadLength > 96 then.*?end_if;').Value
Assert-Match $oversizeBlock 'IngressFaultError\s*:=\s*-5' 'Oversize 0x20E7-class requests do not return deterministic unsupported error -5.'

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
if ($responseBlock -match '\b(?:LMCAxis[1-4]|LMCRobot)\s*\.') {
    throw 'Response still performs a LASAL motion client call.'
}

$motionCyWorkBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($motionCyWorkBlock)) {
    throw 'TCPMotionInterface.CyWork implementation was not found.'
}
Assert-Match $motionCyWorkBlock '(?s)RequestQueue\[QueueReadIndex\$DINT\]\.State\s*=\s*TCPMI_QUEUE_READY.*?State\s*:=\s*TCPMI_QUEUE_ACTIVE.*?MemCpy.*?State\s*:=\s*TCPMI_QUEUE_FREE' 'CyWork queue READY/ACTIVE/FREE transition is missing.'
Assert-Match $motionCyWorkBlock '(?s)CommandID\s*:=\s*ActiveRequest\.CommandId\$DINT;.*?MsgPaser\(\);.*?ActiveRequestValid\s*:=\s*FALSE' 'CyWork does not execute and release one active request.'

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
Assert-Match $st '(?s)AxisCommandInputValid\s*:=.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\)' 'Shortest-only axis direction validation is missing.'
Assert-Match $st '(?s)\(dec = 0\).*?\(Exec = 1\)' 'MoveVelocity deceleration/execute validation is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(\s*buffer,\s*HeaderSize \+ 92,\s*options\.Execute \? 1 : 0\s*\);' 'C# group execute option is not serialized at payload offset 92.'

if ($SourceOnly) {
    Write-Host 'PASS LASAL.StaticContract.SourceOnly (CyWork-only queue, no RT task/mailbox, axis control/motion reads, group enable/disable/status)'
}
else {
    Write-Host 'PASS LASAL.StaticContract (CyWork-only active command contract and ordinary TCP server network)'
}

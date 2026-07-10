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
$tcpServerRtPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\_TCPIPServer_RT\_TCPIPServer_RT.st'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'

$st = Get-Content -Raw -LiteralPath $stPath
$network = Get-Content -Raw -LiteralPath $networkPath
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

if ($SourceOnly) {
    $networkAxes = 2..4
}
else {
    $networkAxes = 1..4
}
foreach ($axisNumber in $networkAxes) {
    $clientName = "LMCAxis$axisNumber"
    $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
        '.*' +
        [regex]::Escape("_LMCAxis$axisNumber.Control")
    Assert-Match $network $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link."
}

if (-not $SourceOnly) {
    $interfaceObject = $networkXml.SelectSingleNode("//Object[@Name='TCPMotionInterface1']")
    $serverObject = $networkXml.SelectSingleNode("//Object[@Name='_TCPIPServer_RT1']")
    if ($null -eq $interfaceObject -or $null -eq $serverObject) {
        throw 'TCPMotionInterface1 or _TCPIPServer_RT1 network object is missing.'
    }
    if ($interfaceObject.RealTime -ne '1 ms' -or $interfaceObject.CyclicTime -ne '1 ms') {
        throw 'TCPMotionInterface1 RealTime/CyclicTime must both be 1 ms.'
    }
    $configClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='Config']")
    $maxConnectionsClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='MaxConnections']")
    if ($null -eq $configClient -or $configClient.Value -ne '0') {
        throw '_TCPIPServer_RT1.Config must be explicitly set to 0.'
    }
    if ($null -eq $maxConnectionsClient -or $maxConnectionsClient.Value -ne '1') {
        throw '_TCPIPServer_RT1.MaxConnections must be explicitly set to 1.'
    }
}

if ($st -match '(?<![A-Za-z0-9_])LMCAxis(?![A-Za-z0-9_])') {
    throw 'Legacy standalone LMCAxis name is still present in TCPMotionInterface.'
}

Assert-Match $st 'RequestQueue\s*:\s*ARRAY \[0\.\.7\] OF _TCPMI_REQUEST_ENTRY' 'Depth-8 LASAL request queue is missing.'
Assert-Match $st 'TO_UDINT\(1663666918\),\s*"LMCAxis1".*TO_UDINT\(1422175863\),\s*"_LMCAxis"' 'LMCAxis1 client-name/type hashes are incorrect.'
Assert-Match $st 'if usPayloadLength > 96 then' 'LASAL queue payload bound is missing.'
Assert-Match $st 'IngressDiscardRemaining\s*:=\s*udFrameSize - ReceiveFill' 'Oversize frame bounded discard is missing.'
Assert-Match $st 'RtRequest\.CommandId\s*=\s*0x202E' '0x202E RT executor is missing.'
Assert-Match $st 'ActiveAwaitingRt\s*:=\s*TRUE' 'CyWork-to-RtWork handoff is missing.'
Assert-Match $st 'GroupMoveRetCode\s*:=\s*_LMCPROF_MOVECMD_ERROR' 'Group move false-success guard is missing.'
if ($st -match 'SessionEpoch\s*:=\s*sigclib_atomic_incU32') {
    throw 'SessionEpoch is non-atomically reassigned after atomic increment.'
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
    '(?s)if \(CommandID = 0x2023\).*?Sendbuf\[14\]\$INT\s*:=\s*-5;.*?RETURN;.*?end_if;').Value
if ([string]::IsNullOrWhiteSpace($blockedGuard)) {
    throw 'The pre-case client-call -5 guard with RETURN is missing.'
}
foreach ($commandId in @('2023', '2024', '2022', '2028', '209F', '20A0', '20A2', '2047', '2048', '2049', '2085', '20A4', '2045')) {
    Assert-Match $blockedGuard "CommandID = 0x$commandId" "Blocked command 0x$commandId is missing from the pre-case guard."
}

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

$motionRtWorkBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::RtWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($motionRtWorkBlock)) {
    throw 'TCPMotionInterface.RtWork implementation was not found.'
}
if ($motionRtWorkBlock -match '\b(?:SendData|_GetObjName|_strlen|CyclicCall)\s*\(') {
    throw 'TCPMotionInterface.RtWork contains TCP or string-registry work.'
}
Assert-Match $motionRtWorkBlock '(?s)pValue:=#RtRequest\.State,\s*swpVal:=TCPMI_RT_FREE.*pValue:=#RtResult\.State,\s*swpVal:=TCPMI_RT_DONE' 'RtWork does not release the request before publishing RT_DONE.'

$motionCyWorkBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork.*?END_FUNCTION').Value
if ($motionCyWorkBlock -match 'swpVal:=TCPMI_RT_DONE') {
    throw 'CyWork blindly republishes RT_DONE after a failed direct send.'
}
Assert-Match $motionCyWorkBlock '(?s)if sendRet = 16 then.*?else.*?swpVal:=TCPMI_RT_FREE.*?state := READY;\s*RETURN;' 'CyWork does not stop after an RT response send failure.'

$sendDataBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION VIRTUAL GLOBAL TCPMotionInterface::SendData.*?END_FUNCTION').Value
Assert-Match $sendDataBlock '_TCPIPServerInterface::SendData' 'TCPMotionInterface.SendData base call is missing.'
Assert-Match $sendDataBlock 'if dRetcode <> udSize\$DINT then' 'Partial/failed send check is missing.'
Assert-Match $sendDataBlock 'IngressFaultCloseRequired\s*:=\s*TRUE' 'Partial send quarantine is missing.'
Assert-Match $sendDataBlock 'sigclib_atomic_incU32\(pValue:=#SessionEpoch\)' 'Partial send does not invalidate the session epoch.'
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
if ($upperBitMappings -ne 5) {
    throw "32-bit axis error truncation guards=$upperBitMappings, expected 5."
}

Assert-Match $st 'AxisObjectName1\s*:\s*ARRAY \[0\.\.255\] OF CHAR' 'LASAL object-name buffer is not 256 bytes.'
Assert-Match $st 'AxisCommandInputValid\s*:=\s*\(dir = 2\)' 'Shortest-only axis direction validation is missing.'
Assert-Match $st '\(dec = 0\).*\(Exec <> 0\)' 'MoveVelocity deceleration/execute validation is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(\s*buffer,\s*HeaderSize \+ 92,\s*options\.Execute \? 1 : 0\s*\);' 'C# group execute option is not serialized at payload offset 92.'

if ($SourceOnly) {
    Write-Host 'PASS LASAL.StaticContract.SourceOnly (LMCAxis1, queue, RT mailbox, callback isolation, axis2-4 links)'
}
else {
    Write-Host 'PASS LASAL.StaticContract (source contract and 4 IDE network links)'
}

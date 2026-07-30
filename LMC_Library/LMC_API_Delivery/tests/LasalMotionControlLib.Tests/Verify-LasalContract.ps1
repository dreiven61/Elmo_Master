param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [switch]$SourceOnly,

    [ValidateSet(
        'Phase2Skeleton',
        'Phase3GroupDormant',
        'Phase3GroupRouted',
        'Phase4AllControlRouted',
        'Phase4DiagnosticsRouted',
        'Phase5TransportClean')]
    [string]$ControlServiceCheckpoint = 'Phase5TransportClean',

    [ValidateSet(
        'StaticTopologyOnly',
        'IdeStructureReady',
        'IntegratedReadOwnerDormant',
        'IntegratedReadOwner',
        'IntegratedOutputOwnerDormant')]
    [string]$TopologyIoCheckpoint = 'StaticTopologyOnly',

    [ValidateRange(0, 4)]
    [int]$ExpectedSdoWriteAxis = 0,

    [switch]$AllowStaleLasalBinaryMetadata
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

function Get-LasalScanText {
    param(
        [string]$Text
    )

    $scanText = [regex]::Replace(
        $Text,
        '(?s)\(\*.*?\*\)',
        {
            param($match)
            [regex]::Replace($match.Value, '[^\r\n]', ' ')
        })
    $scanText = [regex]::Replace(
        $scanText,
        '(?m)//[^\r\n]*',
        { param($match) ' ' * $match.Length })
    $scanText = [regex]::Replace(
        $scanText,
        '"(?:[^"]|"")*"',
        { param($match) ' ' * $match.Length })

    return $scanText
}

function Assert-DiagnosticsCapabilityWriteInventory {
    param(
        [string]$CapabilitiesBlock,
        [string]$ExpectedBootZeroCapabilities,
        [string]$ExpectedStableCapabilities,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $CapabilitiesBlock
    $target = '\(pResponse\s*\+\s*20\)\^\$UDINT'
    $writes = [regex]::Matches(
        $scanText,
        ($target + '\s*:=\s*(?<Rhs>[^;]+);'))
    if ($writes.Count -ne 3) {
        throw (
            "$Owner capability-mask write count is $($writes.Count), " +
            'expected exactly three.')
    }

    $normalizedRightSides = @($writes | ForEach-Object {
            [regex]::Replace($_.Groups['Rhs'].Value, '\s+', '')
        })
    $expectedRightSides = @(
        [regex]::Replace($ExpectedBootZeroCapabilities, '\s+', ''),
        [regex]::Replace($ExpectedStableCapabilities, '\s+', ''),
        '(pResponse+20)^$UDINT|0x00000200')
    foreach ($expectedRightSide in $expectedRightSides) {
        if (@($normalizedRightSides | Where-Object {
                    $_ -ceq $expectedRightSide
                }).Count -ne 1) {
            throw (
                "$Owner capability-mask RHS '$expectedRightSide' must occur " +
                'exactly once.')
        }
    }
}

function Assert-TCPMotionInterfaceFreshOwnerReset {
    param(
        [string]$TcpText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $TcpText
    $connSocketInfoMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
         'TCPMotionInterface::ConnSocketInfo[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($connSocketInfoMatches.Count -ne 1) {
        throw "$Owner ConnSocketInfo implementation was not found exactly once."
    }

    $connectBranchMatches = [regex]::Matches(
        $connSocketInfoMatches[0].Value,
        ('(?is)TCP_SVR_SOCK_INFO_CONNECT\s*:' +
         '(?<Body>.*?)TCP_SVR_SOCK_INFO_DISCONNECT\s*:'))
    if ($connectBranchMatches.Count -ne 1) {
        throw "$Owner CONNECT branch was not found exactly once."
    }

    if ($scanText -notmatch
        '(?i)(?<![A-Za-z0-9_])TCPMI_QUEUE_FREE\s*:=\s*0') {
        throw "$Owner queue zeroing is unsafe because TCPMI_QUEUE_FREE is not zero."
    }

    $freshOwnerMatch = [regex]::Match(
        $connectBranchMatches[0].Groups['Body'].Value,
        ('(?is)(?<Body>' +
         '(?<![A-Za-z0-9_])ActiveRequestValid\s*:=\s*FALSE\s*;' +
         '.*?' +
         '(?<![A-Za-z0-9_])CurrentSock\s*:=\s*dSock\s*;)'))
    if (-not $freshOwnerMatch.Success) {
        throw "$Owner fresh-owner reset-and-publish block was not found."
    }
    $connectCurrentPublishCount = [regex]::Matches(
        $connectBranchMatches[0].Groups['Body'].Value,
        '(?i)(?<![A-Za-z0-9_])CurrentSock\s*:=\s*dSock\s*;').Count
    if ($connectCurrentPublishCount -ne 1) {
        throw (
            "$Owner CONNECT CurrentSock publish count is " +
            "$connectCurrentPublishCount, expected one post-reset publish.")
    }
    $freshOwnerBody = $freshOwnerMatch.Groups['Body'].Value

    $orderedStatements = [ordered]@{
        ActiveRequestInvalid =
            '(?i)(?<![A-Za-z0-9_])ActiveRequestValid\s*:=\s*FALSE\s*;'
        RequestQueueZero =
            ('(?i)_memset\s*\(\s*dest\s*:=\s*#\s*RequestQueue\s*' +
             '\[\s*0\s*\]\s*,\s*usByte\s*:=\s*0\s*,\s*' +
             'cntr\s*:=\s*sizeof\s*\(\s*RequestQueue\s*\)\s*\)\s*;')
        QueueWriteIndexZero =
            '(?i)(?<![A-Za-z0-9_])QueueWriteIndex\s*:=\s*0\s*;'
        QueueReadIndexZero =
            '(?i)(?<![A-Za-z0-9_])QueueReadIndex\s*:=\s*0\s*;'
        ActiveRequestZero =
            ('(?i)_memset\s*\(\s*dest\s*:=\s*#\s*ActiveRequest\s*,' +
             '\s*usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*sizeof\s*\(' +
             '\s*ActiveRequest\s*\)\s*\)\s*;')
        RequestBufferZero =
            ('(?i)_memset\s*\(\s*dest\s*:=\s*#\s*RequestBuf\s*,' +
             '\s*usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*sizeof\s*\(' +
             '\s*RequestBuf\s*\)\s*\)\s*;')
        CommandIdZero =
            '(?i)(?<![A-Za-z0-9_])CommandID\s*:=\s*0\s*;'
        AxisReferenceZero =
            '(?i)(?<![A-Za-z0-9_])AxisRef\s*:=\s*0\s*;'
        PayloadZero =
            '(?i)(?<![A-Za-z0-9_])Payload\s*:=\s*0\s*;'
        ReceiveFillZero =
            '(?i)(?<![A-Za-z0-9_])ReceiveFill\s*:=\s*0\s*;'
        ReceiveSocketZero =
            '(?i)(?<![A-Za-z0-9_])ReceiveSocket\s*:=\s*0\s*;'
        ReceiveBufferZero =
            ('(?i)_memset\s*\(\s*dest\s*:=\s*#\s*ReceiveBuf\s*,' +
             '\s*usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*sizeof\s*\(' +
             '\s*ReceiveBuf\s*\)\s*\)\s*;')
        RpcSocketZero =
            '(?i)(?<![A-Za-z0-9_])RpcSocket\s*:=\s*0\s*;'
        RpcInitializedFalse =
            '(?i)(?<![A-Za-z0-9_])RpcInitialized\s*:=\s*FALSE\s*;'
        RpcCallbackRegisteredFalse =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackRegistered\s*:=\s*FALSE\s*;'
        RpcCallbackEventMaskZero =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackEventMask\s*:=\s*0\s*;'
        RpcCallbackPortZero =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackPort\s*:=\s*0\s*;'
        RpcCallbackIPv4_0 =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackIPv4\s*\[\s*0\s*\]\s*:=\s*0\s*;'
        RpcCallbackIPv4_1 =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackIPv4\s*\[\s*1\s*\]\s*:=\s*0\s*;'
        RpcCallbackIPv4_2 =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackIPv4\s*\[\s*2\s*\]\s*:=\s*0\s*;'
        RpcCallbackIPv4_3 =
            '(?i)(?<![A-Za-z0-9_])RpcCallbackIPv4\s*\[\s*3\s*\]\s*:=\s*0\s*;'
        IngressFaultPendingFalse =
            '(?i)(?<![A-Za-z0-9_])IngressFaultPending\s*:=\s*FALSE\s*;'
        IngressBlockedFalse =
            '(?i)(?<![A-Za-z0-9_])IngressBlocked\s*:=\s*FALSE\s*;'
        IngressFaultCloseRequiredFalse =
            '(?i)(?<![A-Za-z0-9_])IngressFaultCloseRequired\s*:=\s*FALSE\s*;'
        IngressFaultSocketZero =
            '(?i)(?<![A-Za-z0-9_])IngressFaultSocket\s*:=\s*0\s*;'
        IngressFaultEpochZero =
            '(?i)(?<![A-Za-z0-9_])IngressFaultEpoch\s*:=\s*0\s*;'
        IngressFaultErrorZero =
            '(?i)(?<![A-Za-z0-9_])IngressFaultError\s*:=\s*0\s*;'
        IngressDiscardRemainingZero =
            '(?i)(?<![A-Za-z0-9_])IngressDiscardRemaining\s*:=\s*0\s*;'
        IngressDiscardSocketZero =
            '(?i)(?<![A-Za-z0-9_])IngressDiscardSocket\s*:=\s*0\s*;'
        SessionEpochAdvanceAndWrap =
            ('(?is)(?<![A-Za-z0-9_])SessionEpoch\s*\+=\s*1\s*;' +
             '\s*(?<![A-Za-z0-9_])if\s+SessionEpoch\s*=\s*0\s+then' +
             '\s*(?<![A-Za-z0-9_])SessionEpoch\s*:=\s*1\s*;' +
             '\s*(?<![A-Za-z0-9_])end_if\s*;')
        CurrentSocketPublish =
            '(?i)(?<![A-Za-z0-9_])CurrentSock\s*:=\s*dSock\s*;'
    }

    $lastStatementIndex = -1
    foreach ($statement in $orderedStatements.GetEnumerator()) {
        $matches = [regex]::Matches($freshOwnerBody, $statement.Value)
        if ($matches.Count -ne 1) {
            throw (
                "$Owner fresh-owner reset '$($statement.Key)' must occur " +
                "exactly once; found $($matches.Count).")
        }
        if ($matches[0].Index -le $lastStatementIndex) {
            throw (
                "$Owner fresh-owner reset '$($statement.Key)' is out of " +
                'canonical order.')
        }
        $lastStatementIndex = $matches[0].Index
    }

    $canonicalEpochWrapPattern =
        $orderedStatements['SessionEpochAdvanceAndWrap']
    $bodyWithoutCanonicalEpochWrap =
        ([regex]::new($canonicalEpochWrapPattern)).Replace(
            $freshOwnerBody,
            '',
            1)
    $unexpectedControlFlowPattern = (
        '(?i)(?<![A-Za-z0-9_])(?:IF|THEN|ELSIF|ELSE|END_IF|' +
        'CASE|OF|END_CASE|FOR|TO|BY|DO|END_FOR|WHILE|END_WHILE|' +
        'REPEAT|UNTIL|END_REPEAT|RETURN|EXIT|CONTINUE|GOTO|JMP)' +
        '(?![A-Za-z0-9_])')
    $unexpectedControlFlow = [regex]::Match(
        $bodyWithoutCanonicalEpochWrap,
        $unexpectedControlFlowPattern)
    if ($unexpectedControlFlow.Success) {
        throw (
            "$Owner fresh-owner reset contains non-canonical control flow " +
            "'$($unexpectedControlFlow.Value)'.")
    }

    $unexpectedExecutableBody = $freshOwnerBody
    foreach ($statement in $orderedStatements.GetEnumerator()) {
        $unexpectedExecutableBody =
            ([regex]::new($statement.Value)).Replace(
                $unexpectedExecutableBody,
                '',
                1)
    }
    if (-not [string]::IsNullOrWhiteSpace($unexpectedExecutableBody)) {
        throw (
            "$Owner fresh-owner reset contains an unexpected executable " +
            'statement outside the exact allowed inventory.')
    }

    if ($freshOwnerBody -notmatch
        '(?is)CurrentSock\s*:=\s*dSock\s*;\s*$') {
        throw "$Owner must publish CurrentSock only after the complete reset."
    }
    if ($freshOwnerBody -match
        ('(?i)(?<![A-Za-z0-9_])PendingClosedSessionEpoch\s*' +
         '(?::=|\+=|-=|\*=|/=|&=|\|=|\^=|\+\+|--)')) {
        throw (
            "$Owner must preserve PendingClosedSessionEpoch until CyWork " +
            'notifies Diagnostics.')
    }
}

function Assert-TCPIPServerControlledShutdownContract {
    param(
        [string]$ServerText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $ServerText
    $classMatches = [regex]::Matches(
        $scanText,
        ('(?is)(?<![A-Za-z0-9_])TCPIPServer\s*:\s*CLASS\s*' +
         ':\s*_TCPIPServer\b.*?\bEND_CLASS\s*;'))
    if ($classMatches.Count -ne 1) {
        throw "$Owner must derive exactly once from _TCPIPServer."
    }

    Assert-Match $ServerText 'Name\s*=\s*"TCPIPServer"' (
        "$Owner generated class metadata name is not TCPIPServer.")
    Assert-Match $ServerText 'RealtimeTask\s*=\s*"false"' (
        "$Owner must not own a real-time task.")
    Assert-Match $ServerText 'CyclicTask\s*=\s*"true"' (
        "$Owner must preserve inherited cyclic scheduling.")
    if ($scanText -match
        ('(?im)^[ \t]*FUNCTION(?:[ \t]+VIRTUAL)?(?:[ \t]+GLOBAL)?[ \t]+' +
         '(?:TCPIPServer::)?(?:RtWork|CyWork)\b') -or
        $scanText -match 'vmt\.UserFcts\[[^\]]+\]\s*:=\s*#(?:RtWork|CyWork)\s*\(') {
        throw "$Owner must not override or register RtWork/CyWork."
    }
    $controlledShutdownRegistrations = [regex]::Matches(
        $scanText,
        ('(?i)vmt\.UserFcts\[\s*4\s*\]\s*:=\s*' +
         '#SetSocketParameter\s*\(\s*\)\s*;'))
    if ($controlledShutdownRegistrations.Count -ne 1) {
        throw (
            "$Owner must register SetSocketParameter exactly once at the " +
            'inherited command-table slot 4.')
    }

    $methodMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
         'TCPIPServer::SetSocketParameter[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($methodMatches.Count -ne 1) {
        throw "$Owner SetSocketParameter implementation was not found exactly once."
    }
    $methodBlock = $methodMatches[0].Value
    $endVarMatches = [regex]::Matches(
        $methodBlock,
        '(?im)^[ \t]*END_VAR[ \t]*\r?$')
    if ($endVarMatches.Count -ne 3) {
        throw "$Owner SetSocketParameter ABI/local declaration shape drifted."
    }
    $methodBody = $methodBlock.Substring(
        $endVarMatches[2].Index + $endVarMatches[2].Length)
    $methodBody = [regex]::Replace(
        $methodBody,
        '(?im)^[ \t]*END_FUNCTION[ \t]*\r?$',
        '')

    $canonicalBody = (
        '(?is)\A\s*if\s+Cmd\s*=\s*100\s+then\s*' +
        'if\s+SubCmd\s*<>\s*0\s+then\s*' +
        'Retcode\s*:=\s*TCP_SVR_ERR_SET_PARA_INVALID_SUB_CMD\s*;\s*' +
        'RETURN\s*;\s*end_if\s*;\s*' +
        'Retcode\s*:=\s*TCP_SVR_ERR_INVALID_SOCKET\s*;\s*' +
        'for\s+i\s*:=\s*0\s+to\s*\(\s*MaxConn\s*-\s*1\s*\)\s+do\s*' +
        'if\s+SocketArray\s*\[\s*i\s*\]\.dSocket\s*=\s*dSock\s+then\s*' +
        'SocketArray\s*\[\s*i\s*\]\.FSM_TCP\s*:=\s*_STATE_SHUTDOWN\s*;\s*' +
        'Retcode\s*:=\s*TCP_SVR_NO_ERROR\s*;\s*' +
        'exit\s*;\s*end_if\s*;\s*end_for\s*;\s*' +
        'else\s*Retcode\s*:=\s*_TCPIPServer::SetSocketParameter\s*\(\s*' +
        'dSock\s*:=\s*dSock\s*,\s*Cmd\s*:=\s*Cmd\s*,\s*' +
        'SubCmd\s*:=\s*SubCmd\s*,\s*ParaValue\s*:=\s*ParaValue\s*' +
        '\)\s*;\s*end_if\s*;\s*\z')
    if ($methodBody -notmatch $canonicalBody) {
        throw (
            "$Owner SetSocketParameter must implement only Cmd=100/SubCmd=0 " +
            'slot shutdown and delegate every other command to _TCPIPServer.')
    }

    if ($scanText -match
        ('(?i)(?<![A-Za-z0-9_])(?:CLOSESOCKET|DELETECONNECTION|' +
         'REMOVECONNECTION|REMOVELIST|DELETEFROM|pActConn|ActConn)' +
         '(?![A-Za-z0-9_])')) {
        throw (
            "$Owner must leave CLOSESOCKET and connection-list ownership " +
            'to the inherited _TCPIPServer FSM.')
    }
}

function Assert-TCPMotionInterfaceSamePeerTakeover {
    param(
        [string]$TcpText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $TcpText
    $requiredFields = [ordered]@{
        lsl_tcp_user = '\^\s*LSL_TCP_USER'
        CurrentPeerIPv4 = 'UDINT'
        CurrentPeerValid = 'BOOL'
        RetiringSock = 'DINT'
        TakeoverCount = 'UDINT'
        TakeoverRejectCount = 'UDINT'
        LastTakeoverResult = 'DINT'
        LastCandidateSock = 'DINT'
        LastCandidatePeerIPv4 = 'UDINT'
        LastCilTcpUserRet = 'SYS_ERROR'
        LastCandidatePeerLookupRet = 'DINT'
        LastActivePeerLookupRet = 'DINT'
        LastOwnerDisconnectRequestRet = 'DINT'
        LastCandidateDisconnectRequestRet = 'DINT'
    }
    foreach ($field in $requiredFields.GetEnumerator()) {
        $fieldPattern = (
            '(?im)^[ \t]*' + [regex]::Escape([string]$field.Key) +
            '[ \t]*:[ \t]*' + [string]$field.Value + '[ \t]*;[ \t]*\r?$')
        $fieldCount = [regex]::Matches($scanText, $fieldPattern).Count
        if ($fieldCount -ne 1) {
            throw (
                "$Owner field '$($field.Key)' exact declaration count is " +
                "$fieldCount, expected one.")
        }
    }

    $connMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
         'TCPMotionInterface::ConnSocketInfo[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($connMatches.Count -ne 1) {
        throw "$Owner ConnSocketInfo implementation was not found exactly once."
    }
    $connBlock = $connMatches[0].Value
    $connectMatches = [regex]::Matches(
        $connBlock,
        ('(?is)TCP_SVR_SOCK_INFO_CONNECT\s*:' +
         '(?<Body>.*?)TCP_SVR_SOCK_INFO_DISCONNECT\s*:'))
    $disconnectMatches = [regex]::Matches(
        $connBlock,
        ('(?is)TCP_SVR_SOCK_INFO_DISCONNECT\s*:' +
         '(?<Body>.*?)\bend_case\s*;'))
    if ($connectMatches.Count -ne 1 -or $disconnectMatches.Count -ne 1) {
        throw "$Owner CONNECT/DISCONNECT branches are not uniquely isolated."
    }
    $connectBody = $connectMatches[0].Groups['Body'].Value
    $disconnectBody = $disconnectMatches[0].Groups['Body'].Value

    Assert-Match $connectBody (
        '(?s)if\s+CurrentSock\s*=\s*dSock\s+then\s*' +
        'LastTakeoverResult\s*:=\s*-7\s*;\s*RETURN\s*;\s*end_if\s*;') (
        "$Owner duplicate owner CONNECT guard is missing.")
    Assert-Match $connectBody (
        '(?s)LastCandidatePeerLookupRet\s*:=\s*' +
        'OS_TCP_USER_GETPEERIP\s*\(\s*dSock\s*,\s*#candidatePeerIPv4\s*\)' +
        '\s*;\s*if\s+LastCandidatePeerLookupRet\s*>=\s*0\s+then\s*' +
        'candidatePeerValid\s*:=\s*TRUE\s*;\s*' +
        'LastCandidatePeerIPv4\s*:=\s*candidatePeerIPv4\s*;\s*end_if\s*;') (
        "$Owner candidate peer IPv4 lookup/validity contract is missing.")
    Assert-Match $connectBody (
        '(?s)if\s+CurrentPeerValid\s*=\s*FALSE\s+then.*?' +
        'OS_TCP_USER_GETPEERIP\s*\(\s*CurrentSock\s*,\s*#activePeerIPv4\s*\)' +
        '.*?CurrentPeerIPv4\s*:=\s*activePeerIPv4\s*;\s*' +
        'CurrentPeerValid\s*:=\s*TRUE\s*;') (
        "$Owner active owner peer IPv4 fallback lookup is missing.")
    Assert-Match $connectBody (
        '(?s)if\s+candidatePeerValid\s*=\s*FALSE\s+then\s*' +
        'LastTakeoverResult\s*:=\s*-2\s*;\s*' +
        'elsif\s+CurrentPeerValid\s*=\s*FALSE\s+then\s*' +
        'LastTakeoverResult\s*:=\s*-3\s*;\s*' +
        'elsif\s+candidatePeerIPv4\s*<>\s*CurrentPeerIPv4\s+then\s*' +
        'LastTakeoverResult\s*:=\s*-4\s*;\s*else') (
        "$Owner fail-closed candidate/current/same-IPv4 decision cascade is missing.")
    if ($connectBody -match
        '(?i)(?:GETPEERPORT|PeerPort|SourcePort|RemotePort)') {
        throw "$Owner takeover identity must compare peer IPv4 only, not a port."
    }

    $ownerShutdownPattern = (
        '(?is)LastOwnerDisconnectRequestRet\s*:=\s*' +
        '_TCPIPServerInterface::SetSocketParameter\s*\(\s*' +
        'dSock\s*:=\s*CurrentSock\s*,\s*Cmd\s*:=\s*100\s*,\s*' +
        'SubCmd\s*:=\s*0\s*,\s*ParaValue\s*:=\s*0\s*\)\s*;')
    $candidateShutdownPattern = (
        '(?is)LastCandidateDisconnectRequestRet\s*:=\s*' +
        '_TCPIPServerInterface::SetSocketParameter\s*\(\s*' +
        'dSock\s*:=\s*dSock\s*,\s*Cmd\s*:=\s*100\s*,\s*' +
        'SubCmd\s*:=\s*0\s*,\s*ParaValue\s*:=\s*0\s*\)\s*;')
    $ownerShutdownMatches = [regex]::Matches(
        $connectBody,
        $ownerShutdownPattern)
    $candidateShutdownMatches = [regex]::Matches(
        $connectBody,
        $candidateShutdownPattern)
    if ($ownerShutdownMatches.Count -ne 1 -or
        $candidateShutdownMatches.Count -ne 1) {
        throw (
            "$Owner must request exactly one old-owner and one rejected-candidate " +
            'Cmd=100 controlled shutdown.')
    }
    Assert-Match $connectBody (
        '(?s)if\s+LastOwnerDisconnectRequestRet\s*=\s*TCP_SVR_NO_ERROR\s+then\s*' +
        'RetiringSock\s*:=\s*CurrentSock\s*;\s*' +
        'takeCandidate\s*:=\s*TRUE\s*;\s*takeover\s*:=\s*TRUE\s*;\s*' +
        'TakeoverCount\s*\+=\s*1\s*;\s*' +
        'LastTakeoverResult\s*:=\s*2\s*;\s*else\s*' +
        'LastTakeoverResult\s*:=\s*-5\s*;\s*end_if\s*;') (
        "$Owner old-owner shutdown result must gate owner replacement.")
    Assert-Match $connectBody (
        '(?s)if\s+takeCandidate\s*=\s*FALSE\s+then\s*' +
        'TakeoverRejectCount\s*\+=\s*1\s*;.*?' +
        [regex]::Escape('LastCandidateDisconnectRequestRet') +
        '.*?else') (
        "$Owner candidate rejection branch does not preserve the current owner.")

    $resetIndex = $connectBody.IndexOf('ActiveRequestValid')
    $publishIndex = $connectBody.IndexOf('CurrentSock := dSock')
    if ($resetIndex -lt 0 -or $publishIndex -lt 0 -or
        $ownerShutdownMatches[0].Index -ge $resetIndex -or
        $resetIndex -ge $publishIndex) {
        throw (
            "$Owner must request old-owner shutdown before resetting transport " +
            'state and publish the new owner last.')
    }
    Assert-Match $connectBody (
        '(?s)CurrentSock\s*:=\s*dSock\s*;\s*' +
        'CurrentPeerIPv4\s*:=\s*candidatePeerIPv4\s*;\s*' +
        'CurrentPeerValid\s*:=\s*candidatePeerValid\s*;') (
        "$Owner new owner/peer publication is incomplete.")

    $dataHandlingMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
         'TCPMotionInterface::DataHandling[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    $responseMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
         'TCPMotionInterface::Response[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($dataHandlingMatches.Count -ne 1 -or $responseMatches.Count -ne 1) {
        throw "$Owner DataHandling/Response implementations are not unique."
    }
    $dataHandlingEndVars = [regex]::Matches(
        $dataHandlingMatches[0].Value,
        '(?im)^[ \t]*END_VAR[ \t]*\r?$')
    $responseEndVars = [regex]::Matches(
        $responseMatches[0].Value,
        '(?im)^[ \t]*END_VAR[ \t]*\r?$')
    if ($dataHandlingEndVars.Count -ne 3 -or $responseEndVars.Count -ne 2) {
        throw "$Owner DataHandling/Response ABI declaration shape drifted."
    }
    $dataHandlingBody = $dataHandlingMatches[0].Value.Substring(
        $dataHandlingEndVars[2].Index + $dataHandlingEndVars[2].Length)
    $responseBody = $responseMatches[0].Value.Substring(
        $responseEndVars[1].Index + $responseEndVars[1].Length)
    Assert-Match $dataHandlingBody (
        '(?s)\A\s*udReadAvailableDataBytes\s*:=\s*0\s*;\s*' +
        'if\s+dSocket\s*<>\s*CurrentSock\s+then\s*' +
        'if\s+udAvailableData\s*<\s*sizeof\s*\(\s*ReceiveBuf\s*\)\s+then\s*' +
        'udReadAvailableDataBytes\s*:=\s*udAvailableData\s*;\s*else\s*' +
        'udReadAvailableDataBytes\s*:=\s*sizeof\s*\(\s*ReceiveBuf\s*\)\s*;\s*' +
        'end_if\s*;\s*RETURN\s*;') (
        "$Owner DataHandling does not drain non-owner sockets before owner state.")
    Assert-Match $responseBody (
        '(?s)\A\s*if\s+udSize\s*=\s*0\s+then\s*RETURN\s*;\s*end_if\s*;\s*' +
        'if\s+dSock\s*<>\s*CurrentSock\s+then\s*RETURN\s*;\s*end_if\s*;') (
        "$Owner Response does not isolate retiring/rejected non-owner data.")

    Assert-Match $disconnectBody (
        '(?s)if\s+RetiringSock\s*=\s*dSock\s+then\s*' +
        'RetiringSock\s*:=\s*0\s*;\s*end_if\s*;\s*' +
        'if\s+CurrentSock\s*=\s*dSock\s+then\s*' +
        'CurrentSock\s*:=\s*0\s*;\s*CurrentPeerIPv4\s*:=\s*0\s*;\s*' +
        'CurrentPeerValid\s*:=\s*FALSE\s*;') (
        "$Owner late retiring-socket DISCONNECT can clear the new owner.")
    $currentSocketClearCount = [regex]::Matches(
        $disconnectBody,
        '(?i)(?<![A-Za-z0-9_])CurrentSock\s*:=\s*0\s*;').Count
    $currentPeerClearCount = [regex]::Matches(
        $disconnectBody,
        '(?i)(?<![A-Za-z0-9_])CurrentPeerIPv4\s*:=\s*0\s*;').Count
    $currentPeerInvalidCount = [regex]::Matches(
        $disconnectBody,
        '(?i)(?<![A-Za-z0-9_])CurrentPeerValid\s*:=\s*FALSE\s*;').Count
    if ($currentSocketClearCount -ne 1 -or
        $currentPeerClearCount -ne 1 -or
        $currentPeerInvalidCount -ne 1) {
        throw (
            "$Owner DISCONNECT guarded current-owner clear counts are " +
            "socket=$currentSocketClearCount, peer=$currentPeerClearCount, " +
            "peer-valid=$currentPeerInvalidCount; expected one each.")
    }
    Assert-Match $disconnectBody (
        '(?s)if\s+RpcSocket\s*=\s*dSock\s+then\s*' +
        'RpcSocket\s*:=\s*0\s*;\s*RpcInitialized\s*:=\s*FALSE\s*;\s*' +
        'RpcCallbackRegistered\s*:=\s*FALSE\s*;') (
        "$Owner late non-owner DISCONNECT can clear the new RPC owner.")

    if ($scanText -match
        '(?i)(?<![A-Za-z0-9_])(?:CLOSESOCKET|REMOVECONNECTION|REMOVELIST)(?![A-Za-z0-9_])') {
        throw "$Owner must not directly close sockets or mutate the server list."
    }
}

function Assert-TCPSamePeerGeneratedNetworkContract {
    param(
        [System.Xml.XmlDocument]$CommNetworkXml,
        [string]$GeneratedNetworkTable,
        [string]$ConfigObjectsText,
        [string]$Owner
    )

    $topLevelTcpServers = @(
        $CommNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='TCPIPServer1']"))
    $legacyTopLevelTcpServers = @(
        $CommNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='_TCPIPServer1' or " +
            "@Class='_TCPIPServer_RT']"))
    if ($topLevelTcpServers.Count -ne 1 -or
        $topLevelTcpServers[0].Class -ne 'TCPIPServer' -or
        $legacyTopLevelTcpServers.Count -ne 0) {
        throw (
            "$Owner must contain exactly one TCPIPServer1 object of class " +
            'TCPIPServer and no legacy _TCPIPServer1/_TCPIPServer_RT object.')
    }
    $takeoverServerObject = $topLevelTcpServers[0]
    if ($takeoverServerObject.HasAttribute('RealTime') -or
        $takeoverServerObject.CyclicTime -ne '1 ms') {
        throw "$Owner TCPIPServer1 must be cyclic 1 ms and non-real-time."
    }
    $takeoverServerValues = [ordered]@{
        Config = '0'
        ConnectionsPerRun = '1'
        MaxConnections = '2'
        Port = '4000'
    }
    foreach ($serverValue in $takeoverServerValues.GetEnumerator()) {
        $clientNodes = @($takeoverServerObject.SelectNodes(
                "./Channels/Client[@Name='$($serverValue.Key)']"))
        if ($clientNodes.Count -ne 1 -or
            $clientNodes[0].Value -ne [string]$serverValue.Value) {
            throw (
                "$Owner TCPIPServer1.$($serverValue.Key) must be exactly " +
                "$($serverValue.Value).")
        }
    }
    $takeoverServerConnections = @(
        $CommNetworkXml.SelectNodes(
            "/Network/Connections/Connection[" +
            "@Source='TCPMotionInterface1._TCPIPServer' and " +
            "@Destination='TCPIPServer1.Control']"))
    if ($takeoverServerConnections.Count -ne 1) {
        throw (
            "$Owner TCPMotionInterface1._TCPIPServer must connect exactly " +
            'once to TCPIPServer1.Control.')
    }

    foreach ($generatedTcpServerValue in ([ordered]@{
            Port = '4000'
            MaxConnections = '2'
            ConnectionsPerRun = '1'
            Config = '0'
        }).GetEnumerator()) {
        $generatedValuePattern = (
            '"' + [regex]::Escape([string]$generatedTcpServerValue.Key) +
            '",\s*TO_UDINT\(' +
            [regex]::Escape([string]$generatedTcpServerValue.Value) +
            '\),//\|' + [regex]::Escape($Owner) + '\.TCPIPServer1\.' +
            [regex]::Escape([string]$generatedTcpServerValue.Key) + ';')
        $generatedValueCount = [regex]::Matches(
            $GeneratedNetworkTable,
            $generatedValuePattern).Count
        if ($generatedValueCount -ne 1) {
            throw (
                "$Owner generated TCPIPServer1.$($generatedTcpServerValue.Key) " +
                "exact value $($generatedTcpServerValue.Value) count is " +
                "$generatedValueCount, expected one.")
        }
    }
    if ($GeneratedNetworkTable -match
        ([regex]::Escape($Owner) +
         '\._TCPIPServer1\.(?:Port|MaxConnections|ConnectionsPerRun|Config)')) {
        throw "$Owner generated table still targets legacy _TCPIPServer1."
    }

    $configTablesBlock = [regex]::Match(
        $ConfigObjectsText,
        '(?s)FUNCTION\s+GLOBAL\s+TAB\s+CONFIG_TABLES.*?END_FUNCTION').Value
    if ([string]::IsNullOrWhiteSpace($configTablesBlock)) {
        throw "$Owner ConfigObjects.st CONFIG_TABLES block was not found."
    }
    $tcpServerConfigClassCount = [regex]::Matches(
        $configTablesBlock,
        '(?m)^\s*0\$UINT,\s*0,\s*0,\s*"TCPIPSERVER",\s*$').Count
    if ($tcpServerConfigClassCount -ne 1 -or
        $configTablesBlock -match '"_TCPIPSERVER_RT"') {
        throw (
            "$Owner ConfigObjects.st must register TCPIPSERVER exactly once " +
            'and must not register _TCPIPSERVER_RT.')
    }
}

function Assert-LasalExactInitializers {
    param(
        [string]$ConstructorBlock,
        [System.Collections.IDictionary]$ExpectedInitializers,
        [string]$Owner
    )

    if ([string]::IsNullOrWhiteSpace($ConstructorBlock)) {
        throw "$Owner constructor block was not found."
    }

    $scanText = Get-LasalScanText $ConstructorBlock
    foreach ($initializer in $ExpectedInitializers.GetEnumerator()) {
        $escapedName = [regex]::Escape([string]$initializer.Key)
        $escapedValue = [regex]::Escape([string]$initializer.Value)
        $mutationPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*(?:(?::=|\+=|-=|\*=|/=|&=|\|=|\^=)\s*[^;]+|' +
            '(?:\+\+|--)\s*);')
        $exactPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*:=\s*' + $escapedValue + '\s*;')
        $mutationCount = [regex]::Matches(
            $scanText,
            $mutationPattern).Count
        $exactCount = [regex]::Matches(
            $scanText,
            $exactPattern).Count

        if ($mutationCount -ne 1 -or $exactCount -ne 1) {
            throw (
                "$Owner initializer '$($initializer.Key)' must be assigned " +
                "exactly once as '$($initializer.Key) := $($initializer.Value);'; " +
                "found $mutationCount mutation(s) and $exactCount exact assignment(s).")
        }
    }
}

function Assert-LMCSdoExecutorConstructorReady {
    param(
        [string]$SdoExecutorText,
        [string]$ClassDatabaseText,
        [switch]$RequireClassDatabaseMetadata,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $SdoExecutorText
    $classBlock = [regex]::Match(
        $scanText,
        ('(?is)(?<![A-Za-z0-9_])LMCSdoExecutor\s*:\s*CLASS\b.*?' +
         '\bEND_CLASS\s*;')).Value
    if ([string]::IsNullOrWhiteSpace($classBlock)) {
        throw "$Owner class declaration block was not found."
    }

    $constructorDeclarationMatches = [regex]::Matches(
        $classBlock,
        ('(?ims)^[ \t]*FUNCTION[ \t]+LMCSdoExecutor[ \t]*\r?$' +
         '.*?(?=^[ \t]*FUNCTION(?:[ \t]|$)|' +
         '^[ \t]*END_CLASS\b)'))
    $constructorDeclarationAbi = (
        '(?ims)\A[ \t]*FUNCTION[ \t]+LMCSdoExecutor[ \t]*\r?\n' +
        '[ \t]*VAR_OUTPUT[ \t]*\r?\n' +
        '[ \t]*ret_code[ \t]*:[ \t]*ConfStates[ \t]*;[ \t]*\r?\n' +
        '[ \t]*END_VAR[ \t]*;[ \t\r\n]*\z')
    if ($constructorDeclarationMatches.Count -ne 1 -or
        $constructorDeclarationMatches[0].Value -notmatch
            $constructorDeclarationAbi) {
        throw (
            "$Owner explicit constructor declaration is missing or does not " +
            'expose only ret_code : ConfStates.')
    }

    $stdMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+LMCSdoExecutor::@STD[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($stdMatches.Count -ne 1) {
        throw "$Owner @STD implementation was not found exactly once."
    }
    $stdBlock = $stdMatches[0].Value
    $baseStdCallPattern =
        '(?i)ret_code\s*:=\s*EtherCAT_SDOBase::@STD\s*\(\s*\)\s*;'
    $constructorStdCallPattern =
        '(?i)ret_code\s*:=\s*LMCSdoExecutor\s*\(\s*\)\s*;'
    $baseStdCalls = [regex]::Matches($stdBlock, $baseStdCallPattern)
    $constructorStdCalls = [regex]::Matches(
        $stdBlock,
        $constructorStdCallPattern)
    if ($baseStdCalls.Count -ne 1) {
        throw "$Owner @STD must invoke EtherCAT_SDOBase::@STD exactly once."
    }
    if ($constructorStdCalls.Count -ne 1) {
        throw (
            "$Owner @STD must invoke ret_code := LMCSdoExecutor(); " +
            'exactly once.')
    }
    if ($constructorStdCalls[0].Index -le $baseStdCalls[0].Index) {
        throw "$Owner @STD invokes its constructor before the base @STD call."
    }
    if ($stdBlock -notmatch
        ('(?is)' + $constructorStdCallPattern +
         '\s*END_FUNCTION[ \t]*(?:\r\n|\n|\r)?$')) {
        throw "$Owner @STD constructor call must be its final statement."
    }

    $constructorMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+' +
         'LMCSdoExecutor::LMCSdoExecutor[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($constructorMatches.Count -ne 1) {
        throw "$Owner explicit constructor implementation was not found exactly once."
    }
    $constructorBlock = $constructorMatches[0].Value
    $constructorShapePattern = (
        '(?ims)\A[ \t]*FUNCTION[ \t]+' +
        'LMCSdoExecutor::LMCSdoExecutor[ \t]*\r?\n' +
        '[ \t]*VAR_OUTPUT[ \t]*\r?\n' +
        '[ \t]*ret_code[ \t]*:[ \t]*ConfStates[ \t]*;[ \t]*\r?\n' +
        '[ \t]*END_VAR[ \t]*\r?\n' +
        '(?<Body>.*?)^[ \t]*END_FUNCTION[ \t]*\r?\z')
    $constructorShape = [regex]::Match(
        $constructorBlock,
        $constructorShapePattern)
    if (-not $constructorShape.Success) {
        throw "$Owner explicit constructor implementation ABI is not exact."
    }
    $constructorExecutableBody = $constructorShape.Groups['Body'].Value
    $controlFlowPattern = (
        '(?i)(?<![A-Za-z0-9_])(?:RETURN|IF|ELSIF|ELSE|END_IF|' +
        'CASE|END_CASE|FOR|END_FOR|WHILE|END_WHILE|REPEAT|UNTIL|' +
        'END_REPEAT|EXIT|CONTINUE|GOTO|JMP)(?![A-Za-z0-9_])')
    $controlFlowMatch = [regex]::Match(
        $constructorExecutableBody,
        $controlFlowPattern)
    if ($controlFlowMatch.Success) {
        throw (
            "$Owner constructor may not contain control flow '$($controlFlowMatch.Value)'; " +
            'all readiness initialization must execute as one straight-line block.')
    }

    $directInitializers = [ordered]@{
        ActiveToken = '0'
        ActiveIndex = '0'
        ActiveSubIndex = '0'
        ActiveLength = '0'
        ActiveIsWrite = 'FALSE'
        ret_code = 'C_OK'
    }
    $allowedConstructorStatementPatterns =
        [System.Collections.Generic.List[string]]::new()
    Assert-LasalExactInitializers `
        -ConstructorBlock $constructorBlock `
        -ExpectedInitializers $directInitializers `
        -Owner "$Owner constructor"

    $protectedOrderIndexes = [ordered]@{}
    foreach ($initializer in $directInitializers.GetEnumerator()) {
        $escapedName = [regex]::Escape([string]$initializer.Key)
        $escapedValue = [regex]::Escape([string]$initializer.Value)
        $initializerMatch = [regex]::Match(
            $constructorExecutableBody,
            ('(?i)(?<![A-Za-z0-9_])' + $escapedName +
             '\s*:=\s*' + $escapedValue + '\s*;'))
        $protectedOrderIndexes[[string]$initializer.Key] =
            $initializerMatch.Index
        $allowedConstructorStatementPatterns.Add(
            ('(?i)(?<![A-Za-z0-9_])' + $escapedName +
             '\s*:=\s*' + $escapedValue + '\s*;'))

        if ($initializer.Key -ne 'ret_code' -and
            [regex]::Matches(
                $constructorExecutableBody,
                ('(?i)#\s*' + $escapedName + '(?![A-Za-z0-9_])')).Count -ne 0) {
            throw (
                "$Owner constructor may not take the address of " +
                "'$($initializer.Key)'.")
        }
    }

    $zeroingRequirements = @(
        @{
            Name = 'ReadBuffer'
            Address = ('(?i)#\s*ReadBuffer(?![A-Za-z0-9_])' +
                '(?:\s*\[[^\]]+\])?')
            Exact = ('(?i)_memset\s*\(\s*dest\s*:=\s*' +
                '#\s*ReadBuffer\s*\[\s*0\s*\]\s*,\s*' +
                'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
                'sizeof\s*\(\s*ReadBuffer\s*\)\s*\)\s*;')
        },
        @{
            Name = 'WriteBuffer'
            Address = ('(?i)#\s*WriteBuffer(?![A-Za-z0-9_])' +
                '(?:\s*\[[^\]]+\])?')
            Exact = ('(?i)_memset\s*\(\s*dest\s*:=\s*' +
                '#\s*WriteBuffer\s*\[\s*0\s*\]\s*,\s*' +
                'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
                'sizeof\s*\(\s*WriteBuffer\s*\)\s*\)\s*;')
        },
        @{
            Name = 'PublishedResult'
            Address = '(?i)#\s*PublishedResult(?![A-Za-z0-9_])'
            Exact = ('(?i)_memset\s*\(\s*dest\s*:=\s*' +
                '#\s*PublishedResult\s*,\s*usByte\s*:=\s*0\s*,\s*' +
                'cntr\s*:=\s*sizeof\s*\(\s*PublishedResult\s*\)' +
                '\s*\)\s*;')
        })
    foreach ($requirement in $zeroingRequirements) {
        $addressCount = [regex]::Matches(
            $constructorExecutableBody,
            $requirement.Address).Count
        $exactMatches = [regex]::Matches(
            $constructorExecutableBody,
            $requirement.Exact)
        if ($addressCount -ne 1 -or $exactMatches.Count -ne 1) {
            throw (
                "$Owner constructor must zero '$($requirement.Name)' " +
                'exactly once with the complete _memset call.')
        }
        $protectedOrderIndexes[$requirement.Name] = $exactMatches[0].Index
        $allowedConstructorStatementPatterns.Add($requirement.Exact)
    }

    $atomicRequirements = @(
        @{
            Name = 'PublishSequence'
            Value = '0'
        },
        @{
            Name = 'AdapterState'
            Value = 'LMC_SDO_EXEC_IDLE'
        })
    foreach ($requirement in $atomicRequirements) {
        $escapedName = [regex]::Escape($requirement.Name)
        $escapedValue = [regex]::Escape($requirement.Value)
        $addressPattern =
            ('(?i)#\s*' + $escapedName + '(?![A-Za-z0-9_])')
        $exactPattern = (
            '(?i)sigclib_atomic_setU32\s*\(\s*pValue\s*:=\s*#\s*' +
            $escapedName + '\s*,\s*value\s*:=\s*' + $escapedValue +
            '\s*\)\s*;')
        $directMutationPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*(?:(?::=|\+=|-=|\*=|/=|&=|\|=|\^=)\s*[^;]+|' +
            '(?:\+\+|--)\s*);')
        $exactMatches = [regex]::Matches(
            $constructorExecutableBody,
            $exactPattern)
        if ([regex]::Matches(
                $constructorExecutableBody,
                $addressPattern).Count -ne 1 -or
            $exactMatches.Count -ne 1 -or
            [regex]::Matches(
                $constructorExecutableBody,
                $directMutationPattern).Count -ne 0) {
            throw (
                "$Owner constructor must initialize '$($requirement.Name)' " +
                "exactly once through sigclib_atomic_setU32 to " +
                "'$($requirement.Value)'.")
        }
        $protectedOrderIndexes[$requirement.Name] = $exactMatches[0].Index
        $allowedConstructorStatementPatterns.Add($exactPattern)
    }

    $idlePublishIndex = $protectedOrderIndexes['AdapterState']
    foreach ($stateName in @(
            'ActiveToken',
            'ActiveIndex',
            'ActiveSubIndex',
            'ActiveLength',
            'ActiveIsWrite',
            'ReadBuffer',
            'WriteBuffer',
            'PublishedResult',
            'PublishSequence')) {
        if ($protectedOrderIndexes[$stateName] -ge $idlePublishIndex) {
            throw (
                "$Owner constructor publishes AdapterState=Idle before " +
                "initializing '$stateName'.")
        }
    }
    if ($protectedOrderIndexes['ret_code'] -le $idlePublishIndex) {
        throw "$Owner constructor must assign ret_code := C_OK after publishing Idle."
    }

    $constructorBodyRemainder = $constructorExecutableBody
    foreach ($allowedPattern in $allowedConstructorStatementPatterns) {
        $constructorBodyRemainder = ([regex]::new($allowedPattern)).Replace(
            $constructorBodyRemainder,
            '',
            1)
    }
    if (-not [string]::IsNullOrWhiteSpace($constructorBodyRemainder)) {
        throw (
            "$Owner constructor contains executable content outside the " +
            'exact readiness initializer allowlist.')
    }

    if ($RequireClassDatabaseMetadata) {
        $classDatabaseRecord = Get-LasalClassDatabaseRecord `
            -DatabaseText $ClassDatabaseText `
            -SourcePath '.\Class\LMCSdoExecutor\LMCSdoExecutor.st' `
            -ClassName 'LMCSdoExecutor'
        $constructorMetadataPattern = (
            '(?s)ClassSvr.*?AdapterState.*?ActiveToken.*?ActiveIndex.*?' +
            'ActiveSubIndex.*?ActiveLength.*?ActiveIsWrite.*?ReadBuffer.*?' +
            'WriteBuffer.*?PublishSequence.*?PublishedResult.*?' +
            '(?<![A-Za-z0-9_])LMCSdoExecutor(?![A-Za-z0-9_]).*?' +
            'ret_code.*?TryStartRead')
        if ($classDatabaseRecord -notmatch $constructorMetadataPattern) {
            throw (
                "$Owner Classes.lcb metadata lacks the explicit " +
                'LMCSdoExecutor constructor member. Reload and save the class ' +
                'through LASAL IDE, then rebuild.')
        }
    }
}

function Assert-LMCDiagnosticsServiceConstructorReady {
    param(
        [string]$DiagnosticsServiceText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $DiagnosticsServiceText
    $classBlock = [regex]::Match(
        $scanText,
        ('(?is)(?<![A-Za-z0-9_])LMCDiagnosticsService\s*:\s*CLASS\b.*?' +
         '\bEND_CLASS\s*;')).Value
    if ([string]::IsNullOrWhiteSpace($classBlock)) {
        throw "$Owner class declaration block was not found."
    }

    $scalarInitializers = [ordered]@{
        NextBulkId = '0'
        NextBulkConfigRevision = '0'
        BulkId = '0'
        BulkConfigRevision = '0'
        BulkOwnerSessionEpoch = '0'
        BulkConfiguredCycle = '0'
        BulkActivationCycle = '0'
        BulkState = '0'
        BulkSignalCount = '0'
        NextTicketId = '0'
        NextOperationToken = '0'
        TicketId = '0'
        OwnerSessionEpoch = '0'
        TicketBootId = '0'
        TicketMapRevision = '0'
        OperationToken = '0'
        OperationKind = '0'
        OperationState = '0'
        OperationOutcome = '0'
        SdoSlaveReference = '0'
        SdoObjectIndex = '0'
        SdoSubIndex = '0'
        SdoValueType = '0'
        SdoRequestedLength = '0'
        SdoTimeoutCycles = '0'
        SdoSubmitCycle = '0'
        SdoCompletionCycle = '0'
        SdoLastProcessedCycle = '0'
        SdoOperationErrorId = '0'
        SdoOperationDetail = '0'
        SdoResultLength = '0'
        SdoResultData = '0'
        SdoWriteData = '0'
        SdoInternalDrainState = '0'
        DiagnosticsBootId = '0'
        BootIdInitialized = 'FALSE'
        BootIdFault = 'FALSE'
    }
    $expectedClassStateTypes = [ordered]@{
        NextBulkId = 'UDINT'
        NextBulkConfigRevision = 'UDINT'
        BulkId = 'UDINT'
        BulkConfigRevision = 'UDINT'
        BulkOwnerSessionEpoch = 'UDINT'
        BulkConfiguredCycle = 'UDINT'
        BulkActivationCycle = 'UDINT'
        BulkState = 'UINT'
        BulkSignalCount = 'UINT'
        BulkSignalIds = 'ARRAY [0..23] OF UDINT'
        NextTicketId = 'UDINT'
        NextOperationToken = 'UDINT'
        TicketId = 'UDINT'
        OwnerSessionEpoch = 'UDINT'
        TicketBootId = 'UDINT'
        TicketMapRevision = 'UDINT'
        OperationToken = 'UDINT'
        OperationKind = 'UINT'
        OperationState = 'UINT'
        OperationOutcome = 'UINT'
        SdoSlaveReference = 'UINT'
        SdoObjectIndex = 'UINT'
        SdoSubIndex = 'USINT'
        SdoValueType = 'USINT'
        SdoRequestedLength = 'UINT'
        SdoTimeoutCycles = 'UDINT'
        SdoSubmitCycle = 'UDINT'
        SdoCompletionCycle = 'UDINT'
        SdoLastProcessedCycle = 'UDINT'
        SdoOperationErrorId = 'INT'
        SdoOperationDetail = 'UDINT'
        SdoResultLength = 'UDINT'
        SdoResultData = 'UDINT'
        SdoWriteData = 'UDINT'
        SdoInternalDrainState = 'UINT'
        DiagnosticsBootId = 'UDINT'
        BootIdInitialized = 'BOOL'
        BootIdFault = 'BOOL'
    }
    $expectedClassStateNames = @($expectedClassStateTypes.Keys)
    $firstClassFunction = [regex]::Match(
        $classBlock,
        '(?im)^[ \t]*FUNCTION(?:[ \t]|$)')
    if (-not $firstClassFunction.Success) {
        throw "$Owner class function declarations were not found."
    }
    $classMemberPrefix = $classBlock.Substring(
        0,
        $firstClassFunction.Index)
    $declaredClassStateNames = @()
    $declaredClassStateTypes = @{}
    foreach ($member in [regex]::Matches(
            $classMemberPrefix,
            ('(?im)^[ \t]*(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:' +
             '[ \t]*(?<Type>[^;\r\n]+)[ \t]*;'))) {
        if ($member.Groups['Type'].Value -notmatch
            '(?i)^(?:Svr|Clt)Ch') {
            $declaredName = $member.Groups['Name'].Value
            $declaredType = [regex]::Replace(
                $member.Groups['Type'].Value.Trim(),
                '\s+',
                ' ')
            $declaredClassStateNames += $declaredName
            $declaredClassStateTypes[$declaredName] = $declaredType
        }
    }
    $missingClassState = @($expectedClassStateNames | Where-Object {
            $declaredClassStateNames -notcontains $_
        })
    $unexpectedClassState = @($declaredClassStateNames | Where-Object {
            $expectedClassStateNames -notcontains $_
        })
    if ($missingClassState.Count -ne 0 -or
        $unexpectedClassState.Count -ne 0 -or
        $declaredClassStateNames.Count -ne $expectedClassStateNames.Count) {
        throw (
            "$Owner constructor state inventory drifted; missing=" +
            "[$($missingClassState -join ', ')], unexpected=" +
            "[$($unexpectedClassState -join ', ')].")
    }
    foreach ($expectedStateType in
        $expectedClassStateTypes.GetEnumerator()) {
        $declaredType = [string]$declaredClassStateTypes[
            [string]$expectedStateType.Key]
        if (-not [string]::Equals(
                $declaredType,
                [string]$expectedStateType.Value,
                [StringComparison]::OrdinalIgnoreCase)) {
            throw (
                "$Owner state '$($expectedStateType.Key)' type drifted; " +
                "expected '$($expectedStateType.Value)', found " +
                "'$declaredType'.")
        }
    }
    $bulkSignalIdsDeclarationPattern = (
        '(?im)^[ \t]*BulkSignalIds[ \t]*:[ \t]*ARRAY[ \t]*' +
        '\[[ \t]*0[ \t]*\.\.[ \t]*23[ \t]*\][ \t]*OF[ \t]*' +
        'UDINT[ \t]*;[ \t]*$')
    if ([regex]::Matches(
            $classMemberPrefix,
            $bulkSignalIdsDeclarationPattern).Count -ne 1) {
        throw (
            "$Owner BulkSignalIds declaration must remain exactly " +
            'ARRAY [0..23] OF UDINT.')
    }

    $constructorMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+' +
         'LMCDiagnosticsService::LMCDiagnosticsService[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($constructorMatches.Count -ne 1) {
        throw "$Owner constructor implementation was not found exactly once."
    }
    $constructorBlock = $constructorMatches[0].Value
    $constructorShapePattern = (
        '(?ims)\A[ \t]*FUNCTION[ \t]+' +
        'LMCDiagnosticsService::LMCDiagnosticsService[ \t]*\r?\n' +
        '[ \t]*VAR_OUTPUT[ \t]*\r?\n' +
        '[ \t]*ret_code[ \t]*:[ \t]*ConfStates[ \t]*;[ \t]*\r?\n' +
        '[ \t]*END_VAR[ \t]*\r?\n' +
        '(?<Body>.*?)^[ \t]*END_FUNCTION[ \t]*\r?\z')
    $constructorShape = [regex]::Match(
        $constructorBlock,
        $constructorShapePattern)
    if (-not $constructorShape.Success) {
        throw "$Owner constructor implementation ABI is not exact."
    }
    $constructorBody = $constructorShape.Groups['Body'].Value

    $forbiddenControlFlowPattern = (
        '(?i)(?<![A-Za-z0-9_])(?:RETURN|IF|ELSIF|ELSE|END_IF|' +
        'CASE|END_CASE|FOR|END_FOR|WHILE|END_WHILE|REPEAT|UNTIL|' +
        'END_REPEAT|EXIT|CONTINUE|GOTO|JMP)(?![A-Za-z0-9_])')
    $forbiddenControlFlow = [regex]::Match(
        $constructorBody,
        $forbiddenControlFlowPattern)
    if ($forbiddenControlFlow.Success) {
        throw (
            "$Owner constructor may not contain control flow " +
            "'$($forbiddenControlFlow.Value)'.")
    }

    Assert-LasalExactInitializers `
        -ConstructorBlock $constructorBlock `
        -ExpectedInitializers $scalarInitializers `
        -Owner "$Owner scalar constructor"

    $allowedPatterns = [System.Collections.Generic.List[string]]::new()
    $initializerIndexes = @()
    foreach ($initializer in $scalarInitializers.GetEnumerator()) {
        $escapedName = [regex]::Escape([string]$initializer.Key)
        $escapedValue = [regex]::Escape([string]$initializer.Value)
        $exactPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*:=\s*' + $escapedValue + '\s*;')
        $exactMatches = [regex]::Matches(
            $constructorBody,
            $exactPattern)
        if ($exactMatches.Count -ne 1) {
            throw (
                "$Owner scalar '$($initializer.Key)' must be initialized " +
                'exactly once in the constructor body.')
        }
        if ([regex]::Matches(
                $constructorBody,
                ('(?i)#\s*' + $escapedName +
                 '(?![A-Za-z0-9_])')).Count -ne 0) {
            throw "$Owner constructor may not take the address of '$($initializer.Key)'."
        }
        $initializerIndexes += $exactMatches[0].Index
        $allowedPatterns.Add($exactPattern)
    }

    $bulkSignalIdsAddressPattern = (
        '(?i)#\s*BulkSignalIds(?![A-Za-z0-9_])' +
        '(?:\s*\[[^\]]+\])?')
    $bulkSignalIdsZeroPattern = (
        '(?i)_memset\s*\(\s*dest\s*:=\s*' +
        '#\s*BulkSignalIds\s*\[\s*0\s*\]\s*,\s*' +
        'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
        'sizeof\s*\(\s*BulkSignalIds\s*\)\s*\)\s*;')
    $bulkSignalIdsZeroMatches = [regex]::Matches(
        $constructorBody,
        $bulkSignalIdsZeroPattern)
    if ([regex]::Matches(
            $constructorBody,
            $bulkSignalIdsAddressPattern).Count -ne 1 -or
        $bulkSignalIdsZeroMatches.Count -ne 1) {
        throw (
            "$Owner constructor must zero the complete BulkSignalIds array " +
            'exactly once with sizeof(BulkSignalIds).')
    }
    $initializerIndexes += $bulkSignalIdsZeroMatches[0].Index
    $allowedPatterns.Add($bulkSignalIdsZeroPattern)

    $retCodePattern =
        '(?i)(?<![A-Za-z0-9_])ret_code\s*:=\s*C_OK\s*;'
    $retCodeMatches = [regex]::Matches(
        $constructorBody,
        $retCodePattern)
    if ($retCodeMatches.Count -ne 1) {
        throw "$Owner constructor must assign ret_code := C_OK exactly once."
    }
    $lastInitializerIndex =
        ($initializerIndexes | Measure-Object -Maximum).Maximum
    if ($lastInitializerIndex -ge $retCodeMatches[0].Index) {
        throw (
            "$Owner constructor publishes C_OK before all state and array " +
            'initialization is complete.')
    }
    $allowedPatterns.Add($retCodePattern)

    $constructorBodyRemainder = $constructorBody
    foreach ($allowedPattern in $allowedPatterns) {
        $constructorBodyRemainder = ([regex]::new($allowedPattern)).Replace(
            $constructorBodyRemainder,
            '',
            1)
    }
    if (-not [string]::IsNullOrWhiteSpace($constructorBodyRemainder)) {
        throw (
            "$Owner constructor contains executable content outside its " +
            'exact state/array/final-return initializer contract.')
    }
}

function Assert-LMCRecorderStoreConstructorReady {
    param(
        [string]$RecorderStoreText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $RecorderStoreText
    Assert-Match $scanText '#define\s+LMC_RECORDER_BANK_COUNT\s+2' (
        "$Owner must retain exactly two physical recorder banks.")

    $classBlock = [regex]::Match(
        $scanText,
        ('(?is)(?<![A-Za-z0-9_])LMCRecorderStore\s*:\s*CLASS\b.*?' +
         '\bEND_CLASS\s*;')).Value
    if ([string]::IsNullOrWhiteSpace($classBlock)) {
        throw "$Owner class declaration block was not found."
    }

    $scalarInitializers = [ordered]@{
        StateValue = 'LMC_RECORDER_EMPTY'
        ConfigId = '0'
        ConfigRevision = '0'
        MapRevision = '0'
        OwnerSessionEpoch = '0'
        ClosedSessionEpoch = '0'
        DiagnosticsBootId = '0'
        RecordId = '0'
        BufferId = '0'
        SampleCapacity = '0'
        SampleCount = '0'
        SamplePeriodCycles = '1'
        ChannelCount = '0'
        SampleStrideBytes = '0'
        BufferMode = '0'
        TriggerType = '0'
        TriggerValueType = '0'
        TriggerOperator = '0'
        PreTriggerSamples = '0'
        PostTriggerSamples = '0'
        TriggerSignalId = '0'
        TriggerSignalOffset = '0'
        TriggerValue = '0'
        TriggerMask = '0'
        CapturePhase = '1'
        StopReason = 'LMC_RECORDER_STOP_NONE'
        TriggerIndex = '0xFFFFFFFF'
        TriggerCycle = '0'
        TriggerTimestampLow = '0'
        TriggerTimestampHigh = '0'
        StartCycle = '0'
        EndCycle = '0'
        StartTimestampLow = '0'
        StartTimestampHigh = '0'
        EndTimestampLow = '0'
        EndTimestampHigh = '0'
        DroppedCycles = '0'
        OverflowCount = '0'
        DividerCounter = '0'
        NextConfigId = '1'
        NextRecordId = '1'
        StartRequestSequence = '0'
        StartAppliedSequence = '0'
        TriggerRequestSequence = '0'
        TriggerAppliedSequence = '0'
        StopRequestSequence = '0'
        StopAppliedSequence = '0'
        StatusSequence = '0'
        WriteSampleIndex = '0'
        FrozenFirstSampleIndex = '0'
        PostSamplesRemaining = '0'
        PreviousTriggerValue = '0'
        PreviousTriggerValid = 'FALSE'
        BufferReleased = 'TRUE'
    }
    $expectedClassStateNames = @($scalarInitializers.Keys) + @(
        'SignalIds',
        'SignalOffsets')
    $firstClassFunction = [regex]::Match(
        $classBlock,
        '(?im)^[ \t]*FUNCTION(?:[ \t]|$)')
    if (-not $firstClassFunction.Success) {
        throw "$Owner class function declarations were not found."
    }
    $classMemberPrefix = $classBlock.Substring(
        0,
        $firstClassFunction.Index)
    $declaredClassStateNames = @()
    foreach ($member in [regex]::Matches(
            $classMemberPrefix,
            ('(?im)^[ \t]*(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:' +
             '[ \t]*(?<Type>[^;\r\n]+)[ \t]*;'))) {
        if ($member.Groups['Type'].Value -notmatch
            '(?i)^(?:Svr|Clt)Ch') {
            $declaredClassStateNames += $member.Groups['Name'].Value
        }
    }
    $missingClassState = @($expectedClassStateNames | Where-Object {
            $declaredClassStateNames -notcontains $_
        })
    $unexpectedClassState = @($declaredClassStateNames | Where-Object {
            $expectedClassStateNames -notcontains $_
        })
    if ($missingClassState.Count -ne 0 -or
        $unexpectedClassState.Count -ne 0 -or
        $declaredClassStateNames.Count -ne $expectedClassStateNames.Count) {
        throw (
            "$Owner constructor state inventory drifted; missing=" +
            "[$($missingClassState -join ', ')], unexpected=" +
            "[$($unexpectedClassState -join ', ')].")
    }

    $constructorMatches = [regex]::Matches(
        $scanText,
        ('(?ims)^[ \t]*FUNCTION[ \t]+' +
         'LMCRecorderStore::LMCRecorderStore[ \t]*\r?$' +
         '.*?^[ \t]*END_FUNCTION[ \t]*\r?$'))
    if ($constructorMatches.Count -ne 1) {
        throw "$Owner constructor implementation was not found exactly once."
    }
    $constructorBlock = $constructorMatches[0].Value
    $constructorShapePattern = (
        '(?ims)\A[ \t]*FUNCTION[ \t]+' +
        'LMCRecorderStore::LMCRecorderStore[ \t]*\r?\n' +
        '[ \t]*VAR_OUTPUT[ \t]*\r?\n' +
        '[ \t]*ret_code[ \t]*:[ \t]*ConfStates[ \t]*;[ \t]*\r?\n' +
        '[ \t]*END_VAR[ \t]*\r?\n' +
        '[ \t]*VAR[ \t]*\r?\n' +
        '[ \t]*bankIndex[ \t]*:[ \t]*UINT[ \t]*;[ \t]*\r?\n' +
        '[ \t]*END_VAR[ \t]*\r?\n' +
        '(?<Body>.*?)^[ \t]*END_FUNCTION[ \t]*\r?\z')
    $constructorShape = [regex]::Match(
        $constructorBlock,
        $constructorShapePattern)
    if (-not $constructorShape.Success) {
        throw "$Owner constructor implementation ABI is not exact."
    }
    $constructorBody = $constructorShape.Groups['Body'].Value
    if ($constructorBody -match
        '(?i)(?<![A-Za-z0-9_])g_LMCRecorderData(?![A-Za-z0-9_])') {
        throw (
            "$Owner constructor must not clear the 2.56 MB recorder data " +
            'array; state and metadata gate its visibility.')
    }

    $forTokenCount = [regex]::Matches(
        $constructorBody,
        '(?i)(?<![A-Za-z0-9_])FOR(?![A-Za-z0-9_])').Count
    $endForTokenCount = [regex]::Matches(
        $constructorBody,
        '(?i)(?<![A-Za-z0-9_])END_FOR(?![A-Za-z0-9_])').Count
    $loopMatches = [regex]::Matches(
        $constructorBody,
        ('(?ims)^[ \t]*for[ \t]+bankIndex[ \t]*:=[ \t]*0[ \t]+' +
         'to[ \t]+LMC_RECORDER_BANK_COUNT[ \t]*-[ \t]*1[ \t]+do[ \t]*\r?$' +
         '(?<Body>.*?)^[ \t]*end_for[ \t]*;[ \t]*\r?$'))
    if ($forTokenCount -ne 1 -or $endForTokenCount -ne 1 -or
        $loopMatches.Count -ne 1) {
        throw (
            "$Owner constructor must contain exactly one bounded " +
            'bankIndex=0..LMC_RECORDER_BANK_COUNT-1 initialization loop.')
    }
    $loopMatch = $loopMatches[0]
    $prefixBody = $constructorBody.Substring(0, $loopMatch.Index)
    $loopBody = $loopMatch.Groups['Body'].Value
    $suffixBody = $constructorBody.Substring(
        $loopMatch.Index + $loopMatch.Length)

    $forbiddenControlFlowPattern = (
        '(?i)(?<![A-Za-z0-9_])(?:RETURN|IF|ELSIF|ELSE|END_IF|' +
        'CASE|END_CASE|WHILE|END_WHILE|REPEAT|UNTIL|END_REPEAT|' +
        'EXIT|CONTINUE|GOTO|JMP)(?![A-Za-z0-9_])')
    $forbiddenControlFlow = [regex]::Match(
        $constructorBody,
        $forbiddenControlFlowPattern)
    if ($forbiddenControlFlow.Success) {
        throw (
            "$Owner constructor may not contain control flow " +
            "'$($forbiddenControlFlow.Value)' outside its exact bank loop.")
    }

    Assert-LasalExactInitializers `
        -ConstructorBlock $constructorBlock `
        -ExpectedInitializers $scalarInitializers `
        -Owner "$Owner scalar constructor"

    $allowedPrefixPatterns =
        [System.Collections.Generic.List[string]]::new()
    $scalarIndexes = @()
    foreach ($initializer in $scalarInitializers.GetEnumerator()) {
        $escapedName = [regex]::Escape([string]$initializer.Key)
        $escapedValue = [regex]::Escape([string]$initializer.Value)
        $exactPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*:=\s*' + $escapedValue + '\s*;')
        $prefixMatches = [regex]::Matches($prefixBody, $exactPattern)
        if ($prefixMatches.Count -ne 1) {
            throw (
                "$Owner scalar '$($initializer.Key)' must be initialized " +
                'exactly once before array/token initialization.')
        }
        if ([regex]::Matches(
                $constructorBody,
                ('(?i)#\s*' + $escapedName +
                 '(?![A-Za-z0-9_])')).Count -ne 0) {
            throw "$Owner constructor may not take the address of '$($initializer.Key)'."
        }
        $scalarIndexes += $prefixMatches[0].Index
        $allowedPrefixPatterns.Add($exactPattern)
    }

    $arrayRequirements = @(
        @{
            Name = 'SignalIds'
            Address = ('(?i)#\s*SignalIds(?![A-Za-z0-9_])' +
                '(?:\s*\[[^\]]+\])?')
            Exact = ('(?i)_memset\s*\(\s*dest\s*:=\s*' +
                '#\s*SignalIds\s*\[\s*0\s*\]\s*,\s*' +
                'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
                'sizeof\s*\(\s*SignalIds\s*\)\s*\)\s*;')
        },
        @{
            Name = 'SignalOffsets'
            Address = ('(?i)#\s*SignalOffsets(?![A-Za-z0-9_])' +
                '(?:\s*\[[^\]]+\])?')
            Exact = ('(?i)_memset\s*\(\s*dest\s*:=\s*' +
                '#\s*SignalOffsets\s*\[\s*0\s*\]\s*,\s*' +
                'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
                'sizeof\s*\(\s*SignalOffsets\s*\)\s*\)\s*;')
        })
    $arrayIndexes = @()
    foreach ($requirement in $arrayRequirements) {
        $exactMatches = [regex]::Matches($prefixBody, $requirement.Exact)
        if ([regex]::Matches(
                $constructorBody,
                $requirement.Address).Count -ne 1 -or
            [regex]::Matches(
                $constructorBody,
                $requirement.Exact).Count -ne 1 -or
            $exactMatches.Count -ne 1) {
            throw (
                "$Owner constructor must zero '$($requirement.Name)' " +
                'exactly once in the scalar-array prefix.')
        }
        $arrayIndexes += $exactMatches[0].Index
        $allowedPrefixPatterns.Add($requirement.Exact)
    }

    $tokenIndexes = @()
    foreach ($tokenName in @(
            'g_LMCRecorderRecoveryToken',
            'g_LMCRecorderLastReleasedRecoveryToken')) {
        foreach ($tokenIndex in 0..3) {
            $exactPattern = (
                '(?i)(?<![A-Za-z0-9_])' +
                [regex]::Escape($tokenName) + '\s*\[\s*' +
                $tokenIndex + '\s*\]\s*:=\s*0\s*;')
            $prefixMatches = [regex]::Matches($prefixBody, $exactPattern)
            if ([regex]::Matches(
                    $constructorBody,
                    $exactPattern).Count -ne 1 -or
                $prefixMatches.Count -ne 1) {
                throw (
                    "$Owner constructor must initialize $tokenName" +
                    "[$tokenIndex] exactly once before bank publication.")
            }
            if ([regex]::Matches(
                    $constructorBody,
                    ('(?i)#\s*' + [regex]::Escape($tokenName) +
                     '(?![A-Za-z0-9_])')).Count -ne 0) {
                throw "$Owner constructor may not take the address of $tokenName."
            }
            $tokenIndexes += $prefixMatches[0].Index
            $allowedPrefixPatterns.Add($exactPattern)
        }
    }

    $activeGenerationPattern = (
        '(?i)sigclib_atomic_setU32\s*\(\s*pValue\s*:=\s*' +
        '#\s*g_LMCRecorderActiveGeneration\s*,\s*value\s*:=\s*0\s*' +
        '\)\s*;')
    $activeGenerationMatches = [regex]::Matches(
        $prefixBody,
        $activeGenerationPattern)
    if ([regex]::Matches(
            $constructorBody,
            '(?i)#\s*g_LMCRecorderActiveGeneration(?![A-Za-z0-9_])').Count -ne 1 -or
        [regex]::Matches(
            $constructorBody,
            $activeGenerationPattern).Count -ne 1 -or
        $activeGenerationMatches.Count -ne 1) {
        throw (
            "$Owner constructor must atomically initialize active generation " +
            'exactly once before its bank loop.')
    }
    $allowedPrefixPatterns.Add($activeGenerationPattern)

    $lastScalarIndex = ($scalarIndexes | Measure-Object -Maximum).Maximum
    $firstArrayIndex = ($arrayIndexes | Measure-Object -Minimum).Minimum
    $lastArrayIndex = ($arrayIndexes | Measure-Object -Maximum).Maximum
    $firstTokenIndex = ($tokenIndexes | Measure-Object -Minimum).Minimum
    $lastTokenIndex = ($tokenIndexes | Measure-Object -Maximum).Maximum
    if ($lastScalarIndex -ge $firstArrayIndex -or
        $lastArrayIndex -ge $firstTokenIndex -or
        $lastTokenIndex -ge $activeGenerationMatches[0].Index) {
        throw (
            "$Owner constructor initialization order must be scalar, arrays, " +
            'tokens, active generation, then the two-bank loop.')
    }

    $bankDescriptorInitializers = [ordered]@{
        g_LMCRecorderBankRecordId = '0'
        g_LMCRecorderBankConfigId = '0'
        g_LMCRecorderBankConfigRevision = '0'
        g_LMCRecorderBankMapRevision = '0'
        g_LMCRecorderBankOwnerSessionEpoch = '0'
        g_LMCRecorderBankClosedSessionEpoch = '0'
        g_LMCRecorderBankDiagnosticsBootId = '0'
        g_LMCRecorderBankSampleCapacity = '0'
        g_LMCRecorderBankSampleCount = '0'
        g_LMCRecorderBankCapturePhase = '0'
        g_LMCRecorderBankStopReason = '0'
        g_LMCRecorderBankTriggerIndex = '0xFFFFFFFF'
        g_LMCRecorderBankTriggerCycle = '0'
        g_LMCRecorderBankTriggerTimestampLow = '0'
        g_LMCRecorderBankTriggerTimestampHigh = '0'
        g_LMCRecorderBankStartCycle = '0'
        g_LMCRecorderBankEndCycle = '0'
        g_LMCRecorderBankStartTimestampLow = '0'
        g_LMCRecorderBankStartTimestampHigh = '0'
        g_LMCRecorderBankEndTimestampLow = '0'
        g_LMCRecorderBankEndTimestampHigh = '0'
        g_LMCRecorderBankDroppedCycles = '0'
        g_LMCRecorderBankOverflowCount = '0'
        g_LMCRecorderBankFrozenFirstSampleIndex = '0'
    }
    $allowedLoopPatterns =
        [System.Collections.Generic.List[string]]::new()
    $bankDescriptorIndexes = @()
    foreach ($initializer in $bankDescriptorInitializers.GetEnumerator()) {
        $escapedName = [regex]::Escape([string]$initializer.Key)
        $escapedValue = [regex]::Escape([string]$initializer.Value)
        $exactPattern = (
            '(?i)(?<![A-Za-z0-9_])' + $escapedName +
            '\s*\[\s*bankIndex\s*\]\s*:=\s*' +
            $escapedValue + '\s*;')
        $loopMatchesForField = [regex]::Matches($loopBody, $exactPattern)
        if ([regex]::Matches(
                $constructorBody,
                $exactPattern).Count -ne 1 -or
            $loopMatchesForField.Count -ne 1) {
            throw (
                "$Owner bank descriptor '$($initializer.Key)' must be " +
                'initialized exactly once inside the bounded bank loop.')
        }
        if ([regex]::Matches(
                $constructorBody,
                ('(?i)#\s*' + $escapedName +
                 '(?![A-Za-z0-9_])')).Count -ne 0) {
            throw "$Owner constructor may not take the address of '$($initializer.Key)'."
        }
        $bankDescriptorIndexes += $loopMatchesForField[0].Index
        $allowedLoopPatterns.Add($exactPattern)
    }

    $bankStatePattern = (
        '(?i)sigclib_atomic_setU32\s*\(\s*pValue\s*:=\s*' +
        '#\s*g_LMCRecorderBankState\s*\[\s*bankIndex\s*\]\s*,\s*' +
        'value\s*:=\s*LMC_RECORDER_EMPTY\s*\)\s*;')
    $bankStateMatches = [regex]::Matches($loopBody, $bankStatePattern)
    if ([regex]::Matches(
            $constructorBody,
            ('(?i)#\s*g_LMCRecorderBankState(?![A-Za-z0-9_])' +
             '(?:\s*\[[^\]]+\])?')).Count -ne 1 -or
        [regex]::Matches(
            $constructorBody,
            $bankStatePattern).Count -ne 1 -or
        $bankStateMatches.Count -ne 1) {
        throw (
            "$Owner constructor must atomically publish each bank Empty " +
            'exactly once inside the bounded bank loop.')
    }
    $lastBankDescriptorIndex =
        ($bankDescriptorIndexes | Measure-Object -Maximum).Maximum
    if ($lastBankDescriptorIndex -ge $bankStateMatches[0].Index) {
        throw (
            "$Owner constructor publishes a bank Empty before all of its " +
            'identity and terminal metadata are initialized.')
    }
    $allowedLoopPatterns.Add($bankStatePattern)

    $retCodePattern =
        '(?i)(?<![A-Za-z0-9_])ret_code\s*:=\s*C_OK\s*;'
    if ([regex]::Matches(
            $constructorBody,
            $retCodePattern).Count -ne 1 -or
        [regex]::Matches(
            $suffixBody,
            $retCodePattern).Count -ne 1) {
        throw "$Owner constructor must assign ret_code := C_OK once after the bank loop."
    }

    $prefixRemainder = $prefixBody
    foreach ($allowedPattern in $allowedPrefixPatterns) {
        $prefixRemainder = ([regex]::new($allowedPattern)).Replace(
            $prefixRemainder,
            '',
            1)
    }
    $loopRemainder = $loopBody
    foreach ($allowedPattern in $allowedLoopPatterns) {
        $loopRemainder = ([regex]::new($allowedPattern)).Replace(
            $loopRemainder,
            '',
            1)
    }
    $suffixRemainder = ([regex]::new($retCodePattern)).Replace(
        $suffixBody,
        '',
        1)
    if (-not [string]::IsNullOrWhiteSpace($prefixRemainder) -or
        -not [string]::IsNullOrWhiteSpace($loopRemainder) -or
        -not [string]::IsNullOrWhiteSpace($suffixRemainder)) {
        throw (
            "$Owner constructor contains executable content outside its " +
            'exact scalar/array/token/bank/return initializer contract.')
    }
}

function Assert-LasalAddressNamesAllowed {
    param(
        [string]$Text,
        [string[]]$AllowedNames,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $Text
    foreach ($addressMatch in [regex]::Matches(
            $scanText,
            '(?i)#\s*(?<Name>[A-Za-z_][A-Za-z0-9_]*)\b')) {
        $addressName = $addressMatch.Groups['Name'].Value
        if ($AllowedNames -notcontains $addressName) {
            throw ($Owner + " may not take the address of '$addressName'; " +
                'protected state must not escape direct mutation accounting.')
        }
    }
}

Assert-LasalAddressNamesAllowed `
    -Text 'copy(pDest:=#snapshot[0]);' `
    -AllowedNames @('snapshot') `
    -Owner 'Assert-LasalAddressNamesAllowed positive regression'
$lasalAddressAllowlistRejectedAlias = $false
try {
    Assert-LasalAddressNamesAllowed `
        -Text 'pAlias := #detailCode;' `
        -AllowedNames @('snapshot') `
        -Owner 'Assert-LasalAddressNamesAllowed negative regression'
}
catch {
    $lasalAddressAllowlistRejectedAlias = $true
}
if (-not $lasalAddressAllowlistRejectedAlias) {
    throw ('Assert-LasalAddressNamesAllowed failed its protected-scalar ' +
        'address-escape regression.')
}

function Get-LasalStructuredIfBlocks {
    param(
        [string]$Text,
        [string]$ConditionPattern
    )

    # Preserve character offsets while removing comments and strings so IF
    # tokens in prose cannot change the structured nesting depth.
    $scanText = Get-LasalScanText $Text

    $blocks = @()
    $startPattern = '(?i)\bif\s+' + $ConditionPattern
    foreach ($startMatch in [regex]::Matches($scanText, $startPattern)) {
        $depth = 0
        $endIndex = -1
        foreach ($tokenMatch in [regex]::Matches(
                $scanText.Substring($startMatch.Index),
                '(?i)\bif\b|\bend_if\s*;')) {
            if ($tokenMatch.Value -match '(?i)^if$') {
                $depth += 1
            }
            else {
                $depth -= 1
                if ($depth -eq 0) {
                    $endIndex = $startMatch.Index +
                        $tokenMatch.Index + $tokenMatch.Length
                    break
                }
            }
        }

        if ($endIndex -lt 0) {
            throw ('Unbalanced LASAL IF block for condition pattern: ' +
                $ConditionPattern)
        }
        $blocks += $Text.Substring(
            $startMatch.Index,
            $endIndex - $startMatch.Index)
    }

    return @($blocks)
}

function Get-LasalFirstThenArm {
    param(
        [string]$IfBlock
    )

    $scanText = Get-LasalScanText $IfBlock
    $ifDepth = 0
    $caseDepth = 0
    foreach ($tokenMatch in [regex]::Matches(
            $scanText,
            ('(?i)\belsif\b|\belse\b|\bend_if\s*;|\bend_case\s*;|' +
             '\bif\b|\bcase\b'))) {
        if ($tokenMatch.Value -match '(?i)^if$') {
            $ifDepth += 1
            continue
        }
        if ($tokenMatch.Value -match '(?i)^end_if') {
            if ($ifDepth -eq 1 -and $caseDepth -eq 0) {
                return $IfBlock.Substring(0, $tokenMatch.Index)
            }
            $ifDepth -= 1
            continue
        }
        if ($tokenMatch.Value -match '(?i)^case$') {
            $caseDepth += 1
            continue
        }
        if ($tokenMatch.Value -match '(?i)^end_case') {
            $caseDepth -= 1
            continue
        }
        if ($ifDepth -eq 1 -and $caseDepth -eq 0) {
            return $IfBlock.Substring(0, $tokenMatch.Index)
        }
    }

    return $IfBlock
}

$lasalThenArmNoElseProbe = Get-LasalFirstThenArm @'
if outerCondition then
    if innerCondition then
        innerValue := 1;
    end_if;
    outerValue := 2;
end_if;
'@
if ($lasalThenArmNoElseProbe -notmatch 'outerValue\s*:=\s*2;' -or
    $lasalThenArmNoElseProbe -match '(?i)end_if\s*;\s*$') {
    throw ('Get-LasalFirstThenArm failed its no-ELSE nested-IF regression; ' +
        'the returned true arm must exclude only the matching outer END_IF.')
}
function Assert-LasalExactDeclaredType {
    param(
        [string]$Text,
        [string]$Name,
        [string]$ExpectedType,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $Text
    $matchingDeclarations = @()
    foreach ($declarationMatch in [regex]::Matches(
            $scanText,
            ('(?ms)^[ \t]*(?<Names>[A-Za-z_][A-Za-z0-9_]*' +
             '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
             '[A-Za-z_][A-Za-z0-9_]*)*)[ \t]*:[ \t]*' +
             '(?![=])(?<Type>[^;\r\n]+)[ \t]*;'))) {
        $declaredNames = @($declarationMatch.Groups['Names'].Value -split ',' |
            ForEach-Object { $_.Trim() })
        if ($declaredNames -contains $Name) {
            $matchingDeclarations += $declarationMatch
        }
    }

    if ($matchingDeclarations.Count -ne 1) {
        throw ($Owner + ' declaration count is ' +
            $matchingDeclarations.Count + ', expected exactly one.')
    }
    $actualType = $matchingDeclarations[0].Groups['Type'].Value.Trim()
    if (-not $actualType.Equals(
            $ExpectedType,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw ($Owner + " type is '$actualType', expected '$ExpectedType'.")
    }
}

function Get-UniqueLasalIfBlockContaining {
    param(
        [string]$Text,
        [string]$ConditionPattern,
        [string]$RequiredPattern,
        [string]$Message
    )

    $candidates = @(Get-LasalStructuredIfBlocks `
        -Text $Text `
        -ConditionPattern $ConditionPattern | Where-Object {
            $_ -match $RequiredPattern
        })
    if ($candidates.Count -ne 1) {
        throw ($Message + ' Matching structured IF block count is ' +
            $candidates.Count + ', expected exactly one.')
    }

    return $candidates[0]
}

function Assert-LasalPatternDominatedByIf {
    param(
        [string]$Text,
        [string]$ConditionPattern,
        [string]$RequiredPattern,
        [string]$Message
    )

    $occurrences = [regex]::Matches($Text, $RequiredPattern).Count
    $dominatingBlocks = @(Get-LasalStructuredIfBlocks `
        -Text $Text `
        -ConditionPattern $ConditionPattern | Where-Object {
            (Get-LasalFirstThenArm $_) -match $RequiredPattern
        })
    if ($occurrences -ne 1 -or $dominatingBlocks.Count -lt 1) {
        throw ($Message + ' Required call count is ' + $occurrences +
            ' and dominating structured IF block count is ' +
            $dominatingBlocks.Count + '.')
    }
}

function Assert-LasalExactIfGuard {
    param(
        [string]$Text,
        [string]$ConditionPattern,
        [string]$AssignmentPattern,
        [string]$Owner
    )

    $conditionWithThen = '(?:' + $ConditionPattern + ')\s+then'
    $guardBlocks = @(Get-LasalStructuredIfBlocks `
        -Text $Text `
        -ConditionPattern $conditionWithThen)
    if ($guardBlocks.Count -ne 1) {
        throw ($Owner + ' structured IF guard count is ' +
            $guardBlocks.Count + ', expected exactly one.')
    }

    $guardScanText = Get-LasalScanText $guardBlocks[0]
    $exactGuardPattern =
        '(?is)\A\s*if\s+' + $ConditionPattern + '\s+then\s*' +
        $AssignmentPattern + '\s*end_if\s*;\s*\z'
    if ($guardScanText -notmatch $exactGuardPattern) {
        throw ($Owner + ' must contain only its canonical assignment in the ' +
            'true arm and must not have an ELSE or extra executable statement.')
    }
}

$lasalExactGuardProbe = @'
if (detailCode = 0) & (RequestSize <> 40) then
    detailCode := 12;
end_if;
'@
Assert-LasalExactIfGuard `
    -Text $lasalExactGuardProbe `
    -ConditionPattern (
        '\(detailCode\s*=\s*0\)\s*&\s*\(RequestSize\s*<>\s*40\)') `
    -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
    -Owner 'Assert-LasalExactIfGuard positive regression'
$lasalExactGuardRejectedExtraStatement = $false
try {
    Assert-LasalExactIfGuard `
        -Text ($lasalExactGuardProbe -replace
            'detailCode := 12;',
            "detailCode := 12;`r`ndetailCode := 0;") `
        -ConditionPattern (
            '\(detailCode\s*=\s*0\)\s*&\s*\(RequestSize\s*<>\s*40\)') `
        -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
        -Owner 'Assert-LasalExactIfGuard negative regression'
}
catch {
    $lasalExactGuardRejectedExtraStatement = $true
}
if (-not $lasalExactGuardRejectedExtraStatement) {
    throw ('Assert-LasalExactIfGuard failed its extra-statement regression; ' +
        'a failure guard must not accept a reset or second executable statement.')
}

function Get-LasalControlDepthAtIndex {
    param(
        [string]$Text,
        [int]$Index
    )

    if ($Index -lt 0 -or $Index -gt $Text.Length) {
        throw "LASAL control-depth index $Index is outside the source block."
    }

    # Preserve offsets while ignoring tokens in comments and strings.
    $scanText = Get-LasalScanText $Text

    $depth = 0
    foreach ($tokenMatch in [regex]::Matches(
            $scanText.Substring(0, $Index),
            ('(?i)\bend_if\s*;|\bend_case\s*;|\bend_while\s*;|' +
             '\bend_for\s*;|\bend_repeat\s*;|' +
             '\bif\b|\bcase\b|\bwhile\b|\bfor\b|\brepeat\b'))) {
        if ($tokenMatch.Value -match '(?i)^end_') {
            $depth -= 1
            if ($depth -lt 0) {
                throw 'LASAL control structure closes before it opens.'
            }
        }
        else {
            $depth += 1
        }
    }

    return $depth
}

$lasalThenArmDirectIndex = $lasalThenArmNoElseProbe.IndexOf(
    'outerValue := 2;',
    [StringComparison]::Ordinal)
$lasalThenArmNestedIndex = $lasalThenArmNoElseProbe.IndexOf(
    'innerValue := 1;',
    [StringComparison]::Ordinal)
if ((Get-LasalControlDepthAtIndex `
        -Text $lasalThenArmNoElseProbe `
        -Index $lasalThenArmDirectIndex) -ne 1 -or
    (Get-LasalControlDepthAtIndex `
        -Text $lasalThenArmNoElseProbe `
        -Index $lasalThenArmNestedIndex) -ne 2) {
    throw ('Get-LasalFirstThenArm control-depth regression failed; direct ' +
        'statements must be depth 1 and nested IF statements depth 2 in the ' +
        'returned arm representation.')
}

function Get-LasalCommandCaseIds {
    param(
        [string]$FunctionBlock
    )

    $commandIds = @()
    $caseLabelPattern = (
        '(?m)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:')
    foreach ($caseLabel in [regex]::Matches(
            $FunctionBlock,
            $caseLabelPattern)) {
        foreach ($commandId in [regex]::Matches(
                $caseLabel.Groups['Labels'].Value,
                '0x(?<Id>[0-9A-Fa-f]{4})')) {
            $commandIds += $commandId.Groups['Id'].Value.ToUpperInvariant()
        }
    }

    return @($commandIds)
}

function Assert-ExactLasalCommandCaseIds {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string[]]$ExpectedCommandIds
    )

    Assert-Match $FunctionBlock '(?i)\bcase\s+CommandId\s+of\b' (
        "$Owner CommandId case was not found.")

    $actualCommandIds = @(Get-LasalCommandCaseIds $FunctionBlock)
    $duplicateCommandIds = @(
        $actualCommandIds |
            Group-Object |
            Where-Object { $_.Count -ne 1 } |
            ForEach-Object { $_.Name })
    if ($duplicateCommandIds.Count -ne 0) {
        throw (
            "$Owner contains duplicate command IDs: " +
            ($duplicateCommandIds -join ', ') + '.')
    }

    $expected = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualCommandIds)
    if ($difference.Count -ne 0 -or
        $actualCommandIds.Count -ne $expected.Count) {
        throw (
            "$Owner command IDs are [$($actualCommandIds -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Get-LasalTopLevelCommandCaseIds {
    param(
        [string]$FunctionBlock
    )

    $commandIds = @()
    $caseDepth = 0
    $inCommandCase = $false
    $pendingCommandIds = @()
    $pendingPotentialLabelExpressions = @()
    $integerTypePrefixPattern = (
        '(?:(?:BYTE|WORD|DWORD|LWORD|SINT|INT|DINT|LINT|' +
        'USINT|UINT|UDINT|ULINT)#)?')
    $numericValuePattern = (
        '(?:0[xX][0-9A-Fa-f](?:_?[0-9A-Fa-f])*|' +
        '2#[01](?:_?[01])*|' +
        '8#[0-7](?:_?[0-7])*|' +
        '16#[0-9A-Fa-f](?:_?[0-9A-Fa-f])*|' +
        '[0-9](?:_?[0-9])*)')
    $labelLiteralPattern =
        '(?i:' + $integerTypePrefixPattern + $numericValuePattern + ')'
    $caseTokenPattern =
        '(?i)(?<End>\bend_case\b\s*;)|(?<Begin>\bcase\b.*?\bof\b)'
    $scanText = Get-LasalScanText $FunctionBlock
    foreach ($line in ($scanText -split "`r?`n")) {
        if (-not $inCommandCase) {
            $commandCaseMatch = [regex]::Match(
                $line,
                '(?i)\bcase\s+CommandId\s+of\b')
            if (-not $commandCaseMatch.Success) {
                continue
            }
            $inCommandCase = $true
            $caseDepth = 1
            $line = $line.Substring(
                $commandCaseMatch.Index + $commandCaseMatch.Length)
        }

        $lineEvents = @()
        foreach ($caseToken in [regex]::Matches(
                $line,
                $caseTokenPattern)) {
            $lineEvents += [pscustomobject]@{
                Index = $caseToken.Index
                Order = 1
                Kind = if ($caseToken.Groups['Begin'].Success) {
                    'Begin'
                }
                else {
                    'End'
                }
                Match = $caseToken
            }
        }
        $labelCandidatePattern = (
            '(?i)(?:^|;)\s*(?<Labels>' + $labelLiteralPattern +
            '(?:\s*,\s*' + $labelLiteralPattern +
            ')*)\s*(?<Terminator>[:,])')
        foreach ($labelCandidate in [regex]::Matches(
                $line,
                $labelCandidatePattern)) {
            $lineEvents += [pscustomobject]@{
                Index = $labelCandidate.Groups['Labels'].Index
                Order = 2
                Kind = 'Label'
                Match = $labelCandidate
            }
        }
        $potentialLabelPattern =
            '(?i)(?:^|;)\s*(?<Expression>[^;:\r\n]+?)\s*:(?!=)'
        foreach ($potentialLabel in [regex]::Matches(
                $line,
                $potentialLabelPattern)) {
            $lineEvents += [pscustomobject]@{
                Index = $potentialLabel.Groups['Expression'].Index
                Order = 0
                Kind = 'PotentialLabel'
                Match = $potentialLabel
            }
        }
        $potentialLabelContinuationPattern =
            '(?i)(?:^|;)\s*(?<Expression>[^;\r\n]+?)\s*,\s*$'
        foreach ($potentialContinuation in [regex]::Matches(
                $line,
                $potentialLabelContinuationPattern)) {
            $lineEvents += [pscustomobject]@{
                Index = $potentialContinuation.Groups['Expression'].Index
                Order = 0
                Kind = 'PotentialLabelContinuation'
                Match = $potentialContinuation
            }
        }

        $outerCaseEnded = $false
        $sawTopLevelLabelSyntax = $false
        foreach ($lineEvent in @($lineEvents | Sort-Object Index, Order)) {
            if ($lineEvent.Kind -ceq 'Begin') {
                $caseDepth += 1
                continue
            }
            if ($lineEvent.Kind -ceq 'End') {
                $caseDepth -= 1
                if ($caseDepth -eq 0) {
                    if ($pendingCommandIds.Count -ne 0 -or
                        $pendingPotentialLabelExpressions.Count -ne 0) {
                        throw (
                            'LASAL top-level CommandId CASE ended with an ' +
                            'incomplete label expression.')
                    }
                    $outerCaseEnded = $true
                    break
                }
                continue
            }
            if ($lineEvent.Kind -ceq 'PotentialLabel') {
                if ($caseDepth -eq 1) {
                    $sawTopLevelLabelSyntax = $true
                    $labelExpressionParts = @(
                        $pendingPotentialLabelExpressions)
                    $labelExpressionParts +=
                        $lineEvent.Match.Groups['Expression'].Value.Trim()
                    $labelExpression = $labelExpressionParts -join ', '
                    if (-not [regex]::IsMatch(
                            $labelExpression,
                            ('(?i)\A' + $labelLiteralPattern +
                             '(?:\s*,\s*' + $labelLiteralPattern +
                             ')*\z'))) {
                        throw (
                            'LASAL top-level CommandId CASE label expression ' +
                            "is not a recognized integer literal list: " +
                            "'$labelExpression'.")
                    }
                    $pendingPotentialLabelExpressions = @()
                }
                continue
            }
            if ($lineEvent.Kind -ceq 'PotentialLabelContinuation') {
                if ($caseDepth -eq 1) {
                    $sawTopLevelLabelSyntax = $true
                    $pendingPotentialLabelExpressions +=
                        $lineEvent.Match.Groups['Expression'].Value.Trim()
                }
                continue
            }
            if ($caseDepth -eq 1) {
                $sawTopLevelLabelSyntax = $true
                $labelLineMatch = $lineEvent.Match
                foreach ($labelMatch in [regex]::Matches(
                        $labelLineMatch.Groups['Labels'].Value,
                        $labelLiteralPattern)) {
                    $labelText = $labelMatch.Value
                    $normalizedLabelText =
                        $labelText.Replace('_', '')
                    $normalizedLabelText = [regex]::Replace(
                        $normalizedLabelText,
                        ('(?i)^(?:BYTE|WORD|DWORD|LWORD|SINT|INT|DINT|' +
                         'LINT|USINT|UINT|UDINT|ULINT)#'),
                        '')
                    $labelValue = 0L
                    if ($normalizedLabelText -match '^(?:0[xX]|16#)') {
                        $digits = [regex]::Replace(
                            $normalizedLabelText,
                            '^(?:0[xX]|16#)',
                            '')
                        $labelValue = [Convert]::ToInt64($digits, 16)
                    }
                    elseif ($normalizedLabelText -match '^2#') {
                        $digits = $normalizedLabelText.Substring(2)
                        $labelValue = [Convert]::ToInt64($digits, 2)
                    }
                    elseif ($normalizedLabelText -match '^8#') {
                        $digits = $normalizedLabelText.Substring(2)
                        $labelValue = [Convert]::ToInt64($digits, 8)
                    }
                    else {
                        $labelValue = [Convert]::ToInt64(
                            $normalizedLabelText,
                            10)
                    }
                    if ($labelValue -lt 0 -or $labelValue -gt 0xFFFF) {
                        throw (
                            'LASAL top-level CommandId label is outside UINT: ' +
                            $labelText + '.')
                    }
                    $pendingCommandIds += $labelValue.ToString(
                        'X4',
                        [Globalization.CultureInfo]::InvariantCulture)
                }
                if ($labelLineMatch.Groups['Terminator'].Value -ceq ':') {
                    $commandIds += $pendingCommandIds
                    $pendingCommandIds = @()
                }
            }
        }
        if ($outerCaseEnded) {
            break
        }
        if (-not $sawTopLevelLabelSyntax -and
            $caseDepth -eq 1 -and
            -not [string]::IsNullOrWhiteSpace($line)) {
            $pendingCommandIds = @()
            $pendingPotentialLabelExpressions = @()
        }
    }

    return @($commandIds)
}

function Assert-TopologyIoTopLevelRouteSet {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string[]]$ExpectedCommandIds
    )

    $actualCommandIds = @(Get-LasalTopLevelCommandCaseIds $FunctionBlock)
    $expectedCommandIds = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    foreach ($topologyIoCommandId in @('7E13', '7E22', '7E23')) {
        $expectedCount = if (
            $expectedCommandIds -ccontains $topologyIoCommandId) {
            1
        }
        else {
            0
        }
        $actualCount = @($actualCommandIds | Where-Object {
                $_ -ceq $topologyIoCommandId
            }).Count
        if ($actualCount -ne $expectedCount) {
            throw (
                "$Owner top-level 0x$topologyIoCommandId route count is " +
                "$actualCount, expected exactly $expectedCount.")
        }
    }
}

function Assert-ExactLasalTopLevelCommandCaseIds {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string[]]$ExpectedCommandIds
    )

    $actualCommandIds = @(Get-LasalTopLevelCommandCaseIds $FunctionBlock)
    $duplicateCommandIds = @(
        $actualCommandIds |
            Group-Object |
            Where-Object { $_.Count -ne 1 } |
            ForEach-Object { $_.Name })
    $expected = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualCommandIds)
    if ($duplicateCommandIds.Count -ne 0 -or
        $difference.Count -ne 0 -or
        $actualCommandIds.Count -ne $expected.Count) {
        throw (
            "$Owner top-level command IDs are " +
            "[$($actualCommandIds -join ', ')], expected exactly " +
            "[$($expected -join ', ')].")
    }
}

function Assert-ExactLasalCommandRouteIds {
    param(
        [string]$RouterBlock,
        [string]$Owner,
        [string]$CallPattern,
        [string[]]$ExpectedCommandIds
    )

    $routePattern = (
        '(?ms)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:' +
        '(?<Body>.*?)(?=^[ \t]*(?:0x[0-9A-Fa-f]{4}|else\b|end_case\b))')
    $matchingRoutes = @(
        [regex]::Matches($RouterBlock, $routePattern) |
            Where-Object { $_.Groups['Body'].Value -match $CallPattern })
    if ($matchingRoutes.Count -ne 1) {
        throw (
            "$Owner matching route count is $($matchingRoutes.Count), " +
            'expected one.')
    }

    $actualCommandIds = @(
        [regex]::Matches(
            $matchingRoutes[0].Groups['Labels'].Value,
            '0x(?<Id>[0-9A-Fa-f]{4})') |
            ForEach-Object { $_.Groups['Id'].Value.ToUpperInvariant() })
    $expected = @($ExpectedCommandIds | ForEach-Object {
            $_.ToUpperInvariant()
        })
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualCommandIds)
    if ($difference.Count -ne 0 -or
        $actualCommandIds.Count -ne $expected.Count) {
        throw (
            "$Owner command IDs are [$($actualCommandIds -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Assert-ExactRegexValueSet {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Owner,
        [string[]]$ExpectedValues
    )

    $actualValues = @(
        [regex]::Matches($Text, $Pattern) |
            ForEach-Object { $_.Groups['Value'].Value } |
            Sort-Object -Unique)
    $expected = @($ExpectedValues | Sort-Object -Unique)
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualValues)
    if ($difference.Count -ne 0 -or
        $actualValues.Count -ne $expected.Count) {
        throw (
            "$Owner values are [$($actualValues -join ', ')], " +
            "expected exactly [$($expected -join ', ')].")
    }
}

function Assert-ExactLasalConnectedClientSet {
    param(
        [string]$Text,
        [string]$Owner,
        [string[]]$ExpectedClients
    )

    $actualClients = @(
        [regex]::Matches(
            $Text,
            'IsClientConnected\(#(?<Name>[A-Za-z_][A-Za-z0-9_]*)\)') |
            ForEach-Object { $_.Groups['Name'].Value })
    $duplicateClients = @(
        $actualClients |
            Group-Object |
            Where-Object { $_.Count -ne 1 } |
            ForEach-Object { $_.Name })
    $expected = @($ExpectedClients | Sort-Object)
    $actualDistinct = @($actualClients | Sort-Object -Unique)
    $difference = @(Compare-Object `
        -ReferenceObject $expected `
        -DifferenceObject $actualDistinct)
    if ($duplicateClients.Count -ne 0 -or
        $difference.Count -ne 0 -or
        $actualClients.Count -ne $expected.Count) {
        throw (
            "$Owner connected clients are [$($actualClients -join ', ')], " +
            "expected each exactly once: [$($ExpectedClients -join ', ')].")
    }
}

function Test-LasalFailClosedBody {
    param(
        [string]$FunctionBlock
    )

    return [regex]::IsMatch(
        $FunctionBlock,
        ('(?s)VAR_OUTPUT\s*ResponseSize\s*:\s*DINT\s*;\s*END_VAR\s*' +
         'ResponseSize\s*:=\s*-1\s*;\s*END_FUNCTION\s*\z'))
}

function Assert-LasalFailClosedBody {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string]$Checkpoint
    )

    if (-not (Test-LasalFailClosedBody $FunctionBlock)) {
        throw "$Checkpoint $Owner must contain only ResponseSize := -1."
    }
}

function Assert-LasalImplementedBody {
    param(
        [string]$FunctionBlock,
        [string]$Owner,
        [string]$Checkpoint
    )

    if (Test-LasalFailClosedBody $FunctionBlock) {
        throw "$Checkpoint $Owner must be implemented, not fail-closed."
    }
}

function Get-LasalClassDatabaseRecord {
    param(
        [string]$DatabaseText,
        [string]$SourcePath,
        [string]$ClassName
    )

    $recordStart = $DatabaseText.IndexOf(
        $SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordStart -lt 0) {
        throw "LASAL Classes.lcb record for $ClassName was not found."
    }

    $recordEnd = $DatabaseText.IndexOf(
        '.\Class\',
        $recordStart + $SourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordEnd -lt 0) {
        $recordEnd = $DatabaseText.Length
    }

    return $DatabaseText.Substring($recordStart, $recordEnd - $recordStart)
}

function Get-LasalNetworkDatabaseRecord {
    param(
        [string]$DatabaseText,
        [string]$SourcePath,
        [string]$NetworkName
    )

    $recordStart = $DatabaseText.IndexOf(
        $SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordStart -lt 0) {
        throw "LASAL Networks.lcb record for $NetworkName was not found."
    }

    $recordEnd = $DatabaseText.IndexOf(
        '.\Network\',
        $recordStart + $SourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($recordEnd -lt 0) {
        $recordEnd = $DatabaseText.Length
    }

    return $DatabaseText.Substring($recordStart, $recordEnd - $recordStart)
}

function Assert-ExactLasalFunctionAbi {
    param(
        [string]$ClassBlock,
        [string]$FunctionName,
        [bool]$IsGlobal,
        [object[]]$Inputs,
        [object[]]$Outputs
    )

    $scopeToken = if ($IsGlobal) { ' GLOBAL' } else { '' }
    $escapedHeader = 'FUNCTION' + $scopeToken + ' ' +
        [regex]::Escape($FunctionName)
    $headerPattern = '(?m)^[ \t]*' + $escapedHeader + '[ \t]*\r?$'
    $declarationCount = [regex]::Matches(
        $ClassBlock,
        $headerPattern).Count
    if ($declarationCount -ne 1) {
        $scopeDescription = if ($IsGlobal) { 'global' } else { 'private' }
        throw ("LMCControlCommandService.$FunctionName $scopeDescription " +
            "declaration count is $declarationCount, expected one.")
    }

    $declaration = [regex]::Match(
        $ClassBlock,
        ('(?ms)^[ \t]*' + $escapedHeader + '[ \t]*\r?\n' +
         '.*?(?=^[ \t]*FUNCTION\b|^[ \t]*//Tables:)')).Value
    if ([string]::IsNullOrWhiteSpace($declaration)) {
        throw "LMCControlCommandService.$FunctionName declaration was not found."
    }

    $canonicalPattern = '\A\s*' + $escapedHeader + '\s*'
    if ($Inputs.Count -gt 0) {
        $canonicalPattern += 'VAR_INPUT\s*'
        foreach ($inputVariable in $Inputs) {
            $canonicalPattern += (
                [regex]::Escape($inputVariable.Name) + '\s*:\s*' +
                [regex]::Escape($inputVariable.Type) + '\s*;\s*')
        }
        $canonicalPattern += 'END_VAR\s*'
    }
    if ($Outputs.Count -gt 0) {
        $canonicalPattern += 'VAR_OUTPUT\s*'
        foreach ($outputVariable in $Outputs) {
            $canonicalPattern += (
                [regex]::Escape($outputVariable.Name) + '\s*:\s*' +
                [regex]::Escape($outputVariable.Type) + '\s*;\s*')
        }
        $canonicalPattern += 'END_VAR;\s*'
    }
    else {
        $canonicalPattern += ';\s*'
    }
    $canonicalPattern += '\z'

    if (-not [regex]::IsMatch($declaration, $canonicalPattern)) {
        throw ("LMCControlCommandService.$FunctionName declaration does not " +
            'match the exact ordered input/output ABI.')
    }
}

function Assert-NoCaseInsensitiveMemberShadowing {
    param(
        [string]$ClassSource,
        [string]$ClassName
    )

    $classDeclaration = [regex]::Match(
        $ClassSource,
        ('(?s)' + [regex]::Escape($ClassName) +
            '\s*:\s*CLASS(?<Members>.*?)//Functions:'))
    if (-not $classDeclaration.Success) {
        throw "$ClassName generated class member declaration was not found."
    }

    $implementationMarker = '//{{LSL_IMPLEMENTATION'
    $implementationIndex = $ClassSource.IndexOf(
        $implementationMarker,
        [StringComparison]::Ordinal)
    if ($implementationIndex -lt 0) {
        throw "$ClassName implementation marker was not found."
    }

    $declarationPattern = (
        '(?m)^[ \t]*' +
        '(?<Names>[A-Za-z_][A-Za-z0-9_]*' +
        '(?:[ \t]*,[ \t]*[A-Za-z_][A-Za-z0-9_]*)*)[ \t]*:')
    $memberNames = @{}
    foreach ($member in [regex]::Matches(
            $classDeclaration.Groups['Members'].Value,
            $declarationPattern)) {
        foreach ($memberNameValue in ($member.Groups['Names'].Value -split ',')) {
            $memberName = $memberNameValue.Trim()
            $memberNames[$memberName.ToLowerInvariant()] = $memberName
        }
    }

    $implementation = $ClassSource.Substring($implementationIndex)
    $functionHeaderPattern = (
        '(?m)^[ \t]*FUNCTION[^\r\n]*\b' +
        [regex]::Escape($ClassName) +
        '(?:::|\b)')
    $functionPattern = (
        '(?ms)^[ \t]*FUNCTION[^\r\n]*\b' +
        [regex]::Escape($ClassName) +
        '(?:::|\b)[^\r\n]*\r?\n.*?^[ \t]*END_FUNCTION[ \t]*;?[ \t]*$')
    $variableBlockHeaderPattern =
        '(?m)^[ \t]*VAR(?:_[A-Z_]+)?[ \t]*$'
    $variableBlockPattern =
        '(?ms)^[ \t]*VAR(?:_[A-Z_]+)?[ \t]*\r?$' +
        '\s*(?<Variables>.*?)^[ \t]*END_VAR[ \t]*;?[ \t]*$'
    $collisions = @()
    $functionHeaders = [regex]::Matches(
        $implementation,
        $functionHeaderPattern)
    $functions = [regex]::Matches(
        $implementation,
        $functionPattern)

    if ($functions.Count -eq 0 -or
        $functions.Count -ne $functionHeaders.Count) {
        throw (
            "$ClassName implementation function parsing is incomplete: " +
            "headers=$($functionHeaders.Count), blocks=$($functions.Count).")
    }

    $variableBlockCount = 0

    foreach ($function in $functions) {
        $functionName = [regex]::Match(
            $function.Value,
            '^[^\r\n]+').Value.Trim()
        $variableBlockHeaders = [regex]::Matches(
            $function.Value,
            $variableBlockHeaderPattern)
        $variableBlocks = [regex]::Matches(
            $function.Value,
            $variableBlockPattern)
        if ($variableBlocks.Count -ne $variableBlockHeaders.Count) {
            throw (
                "$functionName variable block parsing is incomplete: " +
                "headers=$($variableBlockHeaders.Count), " +
                "blocks=$($variableBlocks.Count).")
        }
        $variableBlockCount += $variableBlocks.Count

        foreach ($variableBlock in $variableBlocks) {
            foreach ($local in [regex]::Matches(
                    $variableBlock.Groups['Variables'].Value,
                    $declarationPattern)) {
                foreach ($localNameValue in ($local.Groups['Names'].Value -split ',')) {
                    $localName = $localNameValue.Trim()
                    $lookupName = $localName.ToLowerInvariant()
                    if ($memberNames.ContainsKey($lookupName)) {
                        $collisions += (
                            "$functionName local '$localName' shadows member " +
                            "'$($memberNames[$lookupName])'")
                    }
                }
            }
        }
    }

    if ($variableBlockCount -eq 0) {
        throw "$ClassName implementation variable blocks were not found."
    }

    if ($collisions.Count -ne 0) {
        throw (
            "$ClassName contains LASAL case-insensitive member shadowing: " +
            ($collisions -join '; '))
    }
}

function Assert-LMCEcatInputLatchGeneratedChannelContract {
    param(
        [string]$LatchText,
        [switch]$IncludeCrevis,
        [string]$Owner
    )

    $expectedClients = [ordered]@{
        'EcatMaster' = 'ECAT_Master_Base'
        'Drive1' = 'Elmo_1'
        'Drive2' = 'Elmo_2'
        'Drive3' = 'Elmo_3'
        'Drive4' = 'Elmo_4'
        'RecorderStore' = 'LMCRecorderStore'
    }
    if ($IncludeCrevis) {
        $expectedClients['Coupler'] = 'GL_9086_1'
        $expectedClients['InputSlot'] = 'GL_9086_1_Slot00'
        $expectedClients['OutputSlot'] = 'GL_9086_1_Slot01'
    }

    $metadataBlock = [regex]::Match(
        $LatchText,
        '(?s)<Class\b[^>]*\bName\s*=\s*"LMCEcatInputLatch".*?</Class>').Value
    if ([string]::IsNullOrWhiteSpace($metadataBlock)) {
        throw "$Owner generated class metadata was not found."
    }
    $metadataClients = [regex]::Matches(
        $metadataBlock,
        '<Client\s+Name="(?<Name>[A-Za-z_][A-Za-z0-9_]*)"[^>]*/>')
    if ($metadataClients.Count -ne $expectedClients.Count) {
        throw (
            "$Owner metadata client count is $($metadataClients.Count), " +
            "expected exactly $($expectedClients.Count).")
    }
    foreach ($expectedClient in $expectedClients.GetEnumerator()) {
        $exactMetadataMatches = [regex]::Matches(
            $metadataBlock,
            ('<Client\s+Name="' + [regex]::Escape($expectedClient.Key) +
             '"\s+Required="true"\s+Internal="false"\s*/>'))
        if ($exactMetadataMatches.Count -ne 1) {
            throw (
                "$Owner.$($expectedClient.Key) required external client metadata " +
                'must occur exactly once.')
        }
    }

    $scanText = Get-LasalScanText $LatchText
    $classBlocks = [regex]::Matches(
        $scanText,
        '(?s)LMCEcatInputLatch\s*:\s*CLASS.*?END_CLASS;')
    if ($classBlocks.Count -ne 1) {
        throw (
            "$Owner generated class declaration block count is " +
            "$($classBlocks.Count), expected exactly one.")
    }
    $classBlock = $classBlocks[0].Value
    $typedClientDeclarations = [regex]::Matches(
        $classBlock,
        ('(?m)^\s*(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*' +
         'CltChCmd_(?<Type>[A-Za-z_][A-Za-z0-9_]*)\s*;\s*$'))
    if ($typedClientDeclarations.Count -ne $expectedClients.Count) {
        throw (
            "$Owner typed client count is $($typedClientDeclarations.Count), " +
            "expected exactly $($expectedClients.Count).")
    }
    foreach ($expectedClient in $expectedClients.GetEnumerator()) {
        $exactDeclarationMatches = @($typedClientDeclarations | Where-Object {
                $_.Groups['Name'].Value -ceq $expectedClient.Key -and
                $_.Groups['Type'].Value -ceq $expectedClient.Value
            })
        if ($exactDeclarationMatches.Count -ne 1) {
            throw (
                "$Owner.$($expectedClient.Key) must have exactly one " +
                "CltChCmd_$($expectedClient.Value) declaration.")
        }
    }

    $channelTableBlock = [regex]::Match(
        $LatchText,
        '(?s)FUNCTION GLOBAL TAB LMCEcatInputLatch::@CT_.*?END_FUNCTION').Value
    if ([string]::IsNullOrWhiteSpace($channelTableBlock)) {
        throw "$Owner generated @CT_ channel table was not found."
    }
    $expectedCountPattern = (
        '(?m)^\s*1\$UINT,\s*' + $expectedClients.Count +
        '\$UINT,\s*0\$UINT,\s*$')
    if ([regex]::Matches(
            $channelTableBlock,
            $expectedCountPattern).Count -ne 1) {
        throw (
            "$Owner generated @CT_ server/client/data counts must be exactly " +
            "1/$($expectedClients.Count)/0.")
    }
    $generatedClientEntries = [regex]::Matches(
        $channelTableBlock,
        ('(?m)^\s*\(::LMCEcatInputLatch\.' +
         '(?<Name>[A-Za-z_][A-Za-z0-9_]*)\.pCh\)\$UINT,\s*' +
         '_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT,\s*' +
         '[^\r\n]*?"(?<QuotedName>[^"]+)",\s*' +
         '[^\r\n]*?"(?<Type>[^"]+)",[^\r\n]*\r*$'))
    if ($generatedClientEntries.Count -ne $expectedClients.Count) {
        throw (
            "$Owner generated @CT_ pCh entry count is " +
            "$($generatedClientEntries.Count), expected exactly " +
            "$($expectedClients.Count).")
    }
    foreach ($expectedClient in $expectedClients.GetEnumerator()) {
        $exactEntryMatches = @($generatedClientEntries | Where-Object {
                $_.Groups['Name'].Value -ceq $expectedClient.Key -and
                $_.Groups['QuotedName'].Value -ceq $expectedClient.Key -and
                $_.Groups['Type'].Value -ceq $expectedClient.Value
            })
        if ($exactEntryMatches.Count -ne 1) {
            throw (
                "$Owner.$($expectedClient.Key) generated @CT_ entry must bind " +
                "its pCh and quoted name to $($expectedClient.Value) exactly once.")
        }
    }

    $declarationRegion = [regex]::Match(
        $LatchText,
        '(?s)\A.*?//}}LSL_DECLARATION').Value
    if ([string]::IsNullOrWhiteSpace($declarationRegion)) {
        throw "$Owner generated declaration region was not found."
    }
    $generatedPragmas = [regex]::Matches(
        $declarationRegion,
        '(?m)^\s*#pragma usingLtd\s+(?<Type>[A-Za-z_][A-Za-z0-9_]*)\s*$')
    if ($generatedPragmas.Count -ne $expectedClients.Count) {
        throw (
            "$Owner generated limited-using pragma count is " +
            "$($generatedPragmas.Count), expected exactly " +
            "$($expectedClients.Count).")
    }
    foreach ($expectedType in $expectedClients.Values) {
        if (@($generatedPragmas | Where-Object {
                    $_.Groups['Type'].Value -ceq $expectedType
                }).Count -ne 1) {
            throw (
                "$Owner generated #pragma usingLtd $expectedType must occur " +
                'exactly once.')
        }
    }
}

function Assert-LMCEcatInputLatchIdeVariableContract {
    param(
        [string]$LatchText,
        [string]$Owner
    )

    $scanText = Get-LasalScanText $LatchText
    $classBlocks = [regex]::Matches(
        $scanText,
        '(?s)LMCEcatInputLatch\s*:\s*CLASS.*?END_CLASS;')
    if ($classBlocks.Count -ne 1) {
        throw (
            "$Owner generated class declaration block count is " +
            "$($classBlocks.Count), expected exactly one.")
    }
    $classBlockMatch = $classBlocks[0]
    $classBlock = $LatchText.Substring(
        $classBlockMatch.Index,
        $classBlockMatch.Length)
    $variableMarkers = [regex]::Matches(
        $classBlock,
        '(?m)^[ \t]*//Variables:[ \t]*\r?$')
    $functionMarkers = [regex]::Matches(
        $classBlock,
        '(?m)^[ \t]*//Functions:[ \t]*\r?$')
    if ($variableMarkers.Count -ne 1 -or
        $functionMarkers.Count -ne 1 -or
        $variableMarkers[0].Index -ge $functionMarkers[0].Index) {
        throw (
            "$Owner generated direct variable region markers are invalid; " +
            'expected one //Variables: before one //Functions:.')
    }
    $variableRegionStart =
        $variableMarkers[0].Index + $variableMarkers[0].Length
    $variableRegion = $classBlock.Substring(
        $variableRegionStart,
        $functionMarkers[0].Index - $variableRegionStart)

    foreach ($expectedVariable in @(
            @{ Name = 'OutputRevision'; Type = 'UDINT' },
            @{ Name = 'OutputObserved'; Type = 'BOOL' },
            @{ Name = 'OutputPreviousValid'; Type = 'BOOL' },
            @{ Name = 'OutputPreviousValue'; Type = 'UDINT' })) {
        Assert-LasalExactDeclaredType `
            -Text $variableRegion `
            -Name $expectedVariable.Name `
            -ExpectedType $expectedVariable.Type `
            -Owner ($Owner + '.' + $expectedVariable.Name)
    }
}

function Assert-LMCEcatInputLatchGeneratedExternalConnections {
    param(
        [string]$NetworkTableText,
        [switch]$IncludeCrevis,
        [string]$Owner
    )

    $expectedConnections = [ordered]@{
        'EcatMaster' = @{
            Object = 'EtherCAT_PLC1'; TargetIndex = '2283968053'
        }
        'Drive1' = @{
            Object = 'Elmo_11'; TargetIndex = '1622340897'
        }
        'Drive2' = @{
            Object = 'Elmo_21'; TargetIndex = '1268754146'
        }
        'Drive3' = @{
            Object = 'Elmo_31'; TargetIndex = '1384421283'
        }
        'Drive4' = @{
            Object = 'Elmo_41'; TargetIndex = '499450212'
        }
    }
    if ($IncludeCrevis) {
        $expectedConnections['Coupler'] = @{
            Object = 'GL_9086_11'; TargetIndex = '2797582533'
        }
        $expectedConnections['InputSlot'] = @{
            Object = 'GL_9086_1_Slot001'; TargetIndex = '2495490262'
        }
        $expectedConnections['OutputSlot'] = @{
            Object = 'GL_9086_1_Slot011'; TargetIndex = '2376407447'
        }
    }

    $externalBlock = [regex]::Match(
        $NetworkTableText,
        '(?s)//External connections\s*0\$UDINT,\s*' +
            '(?<DeclaredCount>\d+)\$UDINT,\s*' +
            '(?<Body>.*?)//Magic internal connections')
    if (-not $externalBlock.Success) {
        throw "$Owner generated external connection block was not found."
    }
    $externalEntries = [regex]::Matches(
        $externalBlock.Groups['Body'].Value,
        ('(?m)^\s*TO_UDINT\((?<OwnerIndex>\d+)\),\s*' +
         '"(?<Client>[^"]+)",\s*(?<Mode>[A-Z_]+),\s*' +
         'TO_UDINT\((?<TargetIndex>\d+)\),\s*' +
         '"(?<Object>[^"]+)",\s*"(?<Server>[^"]+)",[^\r\n]*\r*$'))
    $declaredExternalCount = [int]$externalBlock.Groups['DeclaredCount'].Value
    if ($externalEntries.Count -ne $declaredExternalCount) {
        throw (
            "$Owner generated external connection count header is " +
            "$declaredExternalCount, but exactly $($externalEntries.Count) " +
            'entries were parsed.')
    }
    $anchorConnections = @($externalEntries | Where-Object {
            $_.Groups['Client'].Value -ceq 'EcatMaster' -and
            $_.Groups['Mode'].Value -ceq 'C_DIR' -and
            $_.Groups['TargetIndex'].Value -ceq '2283968053' -and
            $_.Groups['Object'].Value -ceq 'EtherCAT_PLC1' -and
            $_.Groups['Server'].Value -ceq 'ClassState'
        })
    if ($anchorConnections.Count -ne 1) {
        throw (
            "$Owner must contain exactly one generated " +
            'LMCEcatInputLatch.EcatMaster external connection anchor.')
    }
    $latchOwnerIndex = $anchorConnections[0].Groups['OwnerIndex'].Value
    $latchEntries = @($externalEntries | Where-Object {
            $_.Groups['OwnerIndex'].Value -ceq $latchOwnerIndex
        })
    if ($latchEntries.Count -ne $expectedConnections.Count) {
        throw (
            "$Owner LMCEcatInputLatch external connection count is " +
            "$($latchEntries.Count), expected exactly " +
            "$($expectedConnections.Count).")
    }
    foreach ($expectedConnection in $expectedConnections.GetEnumerator()) {
        $exactConnections = @($latchEntries | Where-Object {
                $_.Groups['Client'].Value -ceq $expectedConnection.Key -and
                $_.Groups['Mode'].Value -ceq 'C_DIR' -and
                $_.Groups['TargetIndex'].Value -ceq
                    $expectedConnection.Value.TargetIndex -and
                $_.Groups['Object'].Value -ceq
                    $expectedConnection.Value.Object -and
                $_.Groups['Server'].Value -ceq 'ClassState'
            })
        if ($exactConnections.Count -ne 1) {
            throw (
                "$Owner LMCEcatInputLatch.$($expectedConnection.Key) must bind " +
                "by one generated C_DIR entry to " +
                "$($expectedConnection.Value.Object).ClassState with " +
                "TargetIndex $($expectedConnection.Value.TargetIndex).")
        }
    }

    if ($IncludeCrevis) {
        foreach ($crevisConnection in @(
                @{ Client = 'Coupler'; Object = 'GL_9086_11' },
                @{ Client = 'InputSlot'; Object = 'GL_9086_1_Slot001' },
                @{ Client = 'OutputSlot'; Object = 'GL_9086_1_Slot011' })) {
            $destinationOwners = @($externalEntries | Where-Object {
                    $_.Groups['Object'].Value -ceq $crevisConnection.Object -and
                    $_.Groups['Server'].Value -ceq 'ClassState'
                })
            if ($destinationOwners.Count -ne 1 -or
                $destinationOwners[0].Groups['OwnerIndex'].Value -cne
                    $latchOwnerIndex -or
                $destinationOwners[0].Groups['Client'].Value -cne
                    $crevisConnection.Client -or
                $destinationOwners[0].Groups['Mode'].Value -cne 'C_DIR') {
                throw (
                    "$Owner $($crevisConnection.Object).ClassState must have " +
                    'exactly one generated C_DIR owner, LMCEcatInputLatch.' +
                    $crevisConnection.Client + '.')
            }
        }
    }
}

function Set-LasalGeneratedExternalConnectionCountFixture {
    param(
        [string]$Text,
        [int]$DeclaredCount,
        [string]$Owner
    )

    $countMatches = [regex]::Matches(
        $Text,
        ('(?s)//External connections\s*0\$UDINT,\s*' +
         '(?<Count>\d+)(?=\$UDINT,)'))
    if ($countMatches.Count -ne 1) {
        throw (
            "$Owner generated external connection count header occurrence " +
            "is $($countMatches.Count), expected exactly one.")
    }
    $countGroup = $countMatches[0].Groups['Count']
    return (
        $Text.Substring(0, $countGroup.Index) +
        $DeclaredCount.ToString(
            [Globalization.CultureInfo]::InvariantCulture) +
        $Text.Substring($countGroup.Index + $countGroup.Length))
}

function Assert-LMCEcatInputLatchNetworkSourceContract {
    param(
        [xml]$CommNetworkXml,
        [xml]$MotionNetworkXml,
        [xml]$EtherCatNetworkXml,
        [string]$Owner
    )

    $networkXmlDocuments = @(
        $CommNetworkXml,
        $MotionNetworkXml,
        $EtherCatNetworkXml)
    $motionLatchObjects = @($MotionNetworkXml.SelectNodes(
        "/Network/Components/Object[@Name='LMCEcatInputLatch1' and " +
        "@Class='LMCEcatInputLatch']"))
    $allLatchObjects = @()
    foreach ($networkXml in $networkXmlDocuments) {
        $allLatchObjects += @($networkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCEcatInputLatch1' or " +
            "@Class='LMCEcatInputLatch']"))
    }
    if ($motionLatchObjects.Count -ne 1 -or
        $allLatchObjects.Count -ne 1) {
        throw (
            "$Owner LMCEcatInputLatch1 must exist exactly once as " +
            'LMCEcatInputLatch in Motion_Network and nowhere else.')
    }
    $latchObject = $motionLatchObjects[0]
    foreach ($taskAttribute in @(
            'RealTime',
            'CyclicTime',
            'BackgroundTime')) {
        if ($latchObject.HasAttribute($taskAttribute)) {
            throw (
                "$Owner LMCEcatInputLatch1 must not own a scheduled task; " +
                "$taskAttribute is present.")
        }
    }

    $commDiagnosticsObjects = @($CommNetworkXml.SelectNodes(
        "/Network/Components/Object[@Name='LMCDiagnosticsService1' and " +
        "@Class='LMCDiagnosticsService']"))
    $allDiagnosticsObjects = @()
    foreach ($networkXml in $networkXmlDocuments) {
        $allDiagnosticsObjects += @($networkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCDiagnosticsService1' or " +
            "@Class='LMCDiagnosticsService']"))
    }
    if ($commDiagnosticsObjects.Count -ne 1 -or
        $allDiagnosticsObjects.Count -ne 1) {
        throw (
            "$Owner LMCDiagnosticsService1 must exist exactly once as " +
            'LMCDiagnosticsService in Comm_Network and nowhere else.')
    }
    $inputLatchClients = @($commDiagnosticsObjects[0].SelectNodes(
        "./Channels/Client[@Name='InputLatch']"))
    if ($inputLatchClients.Count -ne 1) {
        throw (
            "$Owner LMCDiagnosticsService1.InputLatch client count is " +
            "$($inputLatchClients.Count), expected exactly one.")
    }

    $allLatchServerConnections = @()
    foreach ($networkXml in $networkXmlDocuments) {
        $allLatchServerConnections += @($networkXml.SelectNodes(
            "//Connection[@Destination='LMCEcatInputLatch1.ClassSvr']"))
    }
    $expectedLatchServerSources = @(
        '_LMCAxis1.LMCPreRtWorkTrigger',
        'LMCDiagnosticsService1.InputLatch')
    if ($allLatchServerConnections.Count -ne
        $expectedLatchServerSources.Count) {
        throw (
            "$Owner LMCEcatInputLatch1.ClassSvr owner count is " +
            "$($allLatchServerConnections.Count), expected exactly two.")
    }
    foreach ($expectedSource in $expectedLatchServerSources) {
        if (@($allLatchServerConnections | Where-Object {
                    $_.Source -ceq $expectedSource
                }).Count -ne 1) {
            throw (
                "$Owner $expectedSource -> LMCEcatInputLatch1.ClassSvr " +
                'must occur exactly once across all Object Networks.')
        }
    }

    $commInputLatchConnections = @($CommNetworkXml.SelectNodes(
        "//Connection[@Source='LMCDiagnosticsService1.InputLatch' and " +
        "@Destination='LMCEcatInputLatch1.ClassSvr']"))
    if ($commInputLatchConnections.Count -ne 1) {
        throw (
            "$Owner LMCDiagnosticsService1.InputLatch must connect exactly " +
            'once to LMCEcatInputLatch1.ClassSvr in Comm_Network.')
    }

    $rtTriggerConnections = @($MotionNetworkXml.SelectNodes(
        "//Connection[@Source='_LMCAxis1.LMCPreRtWorkTrigger' and " +
        "@Destination='LMCEcatInputLatch1.ClassSvr']"))
    if ($rtTriggerConnections.Count -ne 1) {
        throw (
            "$Owner LMCEcatInputLatch1 must have exactly one Motion_Network " +
            '_LMCAxis1.LMCPreRtWorkTrigger connection.')
    }
}

function Assert-ConfiguredEtherCATTopologyContract {
    param(
        [xml]$EniXml,
        [xml]$NetworkXml,
        [string]$DiagnosticsServiceText,
        [string]$GeneratedTableText = '',
        [string]$Owner
    )

    $expectedSlaves = @(
        @{ Name = 'GL_9086_11'; Class = 'GL_9086_1'; EniName = 'Slave 01 (GL-9086,Crevis)'; PhysAddr = 1001; AutoIncAddr = 0; VendorId = 669; ProductCode = 1196200070; RevisionNo = 65536 },
        @{ Name = 'Elmo_11'; Class = 'Elmo_1'; EniName = 'Slave 02 (Elmo Drive )'; PhysAddr = 1002; AutoIncAddr = -1; VendorId = 154; ProductCode = 198948; RevisionNo = 66592 },
        @{ Name = 'Elmo_21'; Class = 'Elmo_2'; EniName = 'Slave 03 (Elmo Drive )'; PhysAddr = 1003; AutoIncAddr = -2; VendorId = 154; ProductCode = 198948; RevisionNo = 66592 },
        @{ Name = 'Elmo_31'; Class = 'Elmo_3'; EniName = 'Slave 04 (Elmo Drive )'; PhysAddr = 1004; AutoIncAddr = -3; VendorId = 154; ProductCode = 198948; RevisionNo = 66592 },
        @{ Name = 'Elmo_41'; Class = 'Elmo_4'; EniName = 'Slave 05 (Elmo Drive )'; PhysAddr = 1005; AutoIncAddr = -4; VendorId = 154; ProductCode = 198948; RevisionNo = 66592 }
    )
    $eniSlaves = @($EniXml.EtherCATConfig.Config.Slave)
    if ($eniSlaves.Count -ne $expectedSlaves.Count) {
        throw "$Owner ENI configured slave count is $($eniSlaves.Count), expected exactly 5."
    }
    for ($index = 0; $index -lt $expectedSlaves.Count; $index++) {
        $actual = $eniSlaves[$index]
        $expected = $expectedSlaves[$index]
        foreach ($field in @('Name', 'PhysAddr', 'AutoIncAddr', 'VendorId', 'ProductCode', 'RevisionNo')) {
            $actualValue = if ($field -eq 'Name') {
                [string]$actual.Info.Name
            }
            else {
                [string]$actual.Info.$field
            }
            $expectedValue = if ($field -eq 'Name') {
                [string]$expected.EniName
            }
            else {
                [string]$expected[$field]
            }
            if ($actualValue -cne $expectedValue) {
                throw ("$Owner ENI slave order/identity mismatch at index $index " +
                    "for ${field}: '$actualValue', expected '$expectedValue'.")
            }
        }
    }

    $coupler = $eniSlaves[0]
    foreach ($direction in @('Send', 'Recv')) {
        if ([string]$coupler.ProcessData.$direction.BitStart -cne '696' -or
            [string]$coupler.ProcessData.$direction.BitLength -cne '32') {
            throw "$Owner ENI CREVIS $direction process image must be bit 696/32."
        }
    }
    foreach ($pdoContract in @(
            @{ Direction = 'TxPdo'; Sm = '3'; PdoIndex = '#x1A00'; EntryIndex = '#x6000' },
            @{ Direction = 'RxPdo'; Sm = '2'; PdoIndex = '#x1601'; EntryIndex = '#x7010' })) {
        $selectedPdos = @($coupler.ProcessData.($pdoContract.Direction) |
            Where-Object { [string]$_.Sm -ceq $pdoContract.Sm })
        if ($selectedPdos.Count -ne 1 -or
            [string]$selectedPdos[0].Index -cne $pdoContract.PdoIndex) {
            throw ("$Owner ENI CREVIS $($pdoContract.Direction) selected PDO " +
                "must be $($pdoContract.PdoIndex) on SM$($pdoContract.Sm).")
        }
        $entries = @($selectedPdos[0].Entry)
        if ($entries.Count -ne 4) {
            throw "$Owner ENI CREVIS $($pdoContract.Direction) must contain exactly four entries."
        }
        for ($entryIndex = 0; $entryIndex -lt 4; $entryIndex++) {
            $entry = $entries[$entryIndex]
            $expectedSubIndex = '#x' + ($entryIndex + 1)
            if ([string]$entry.Index -cne $pdoContract.EntryIndex -or
                [string]$entry.SubIndex -cne $expectedSubIndex -or
                [string]$entry.BitLen -cne '8' -or
                [string]$entry.DataType -cne 'USINT') {
                throw ("$Owner ENI CREVIS $($pdoContract.Direction) entry " +
                    "$entryIndex must be $($pdoContract.EntryIndex):$expectedSubIndex USINT/8-bit.")
            }
        }
    }

    $networkSlaveObjects = @($NetworkXml.SelectNodes(
        '/Network/Components/Object[Channels/Client[@Name="SlaveIndex"]]'))
    if ($networkSlaveObjects.Count -ne 5) {
        throw "$Owner EtherCAT_Network must expose exactly five configured SlaveIndex objects."
    }
    for ($index = 0; $index -lt $expectedSlaves.Count; $index++) {
        $expected = $expectedSlaves[$index]
        $matches = @($networkSlaveObjects | Where-Object {
                $_.Name -ceq $expected.Name -and
                $_.Class -ceq $expected.Class -and
                [string]$_.Channels.Client.Where({ $_.Name -ceq 'SlaveIndex' }).Value -ceq [string]$index
            })
        if ($matches.Count -ne 1) {
            throw "$Owner EtherCAT_Network SlaveIndex $index must map exactly to $($expected.Name)/$($expected.Class)."
        }
    }
    foreach ($slot in @(
            @{ Name = 'GL_9086_1_Slot001'; DeviceType = 'GL_9086_1_Slot00_GT-12FA'; Slot = '0' },
            @{ Name = 'GL_9086_1_Slot011'; DeviceType = 'GL_9086_1_Slot01_GT-22BA'; Slot = '1' })) {
        $slotObjects = @($NetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='$($slot.Name)' and AdditionalData/Entry[@Name='DeviceType' and @Value='$($slot.DeviceType)']]"))
        if ($slotObjects.Count -ne 1) {
            throw "$Owner EtherCAT_Network $($slot.Name) DeviceType must be $($slot.DeviceType)."
        }
        $slotChannels = @($slotObjects[0].SelectNodes(
            "./Channels/Client[@Name='Slot' and @Value='$($slot.Slot)']"))
        if ($slotChannels.Count -ne 1) {
            throw "$Owner EtherCAT_Network $($slot.Name).Slot must be $($slot.Slot)."
        }
        $slotConnections = @($NetworkXml.SelectNodes(
            "/Network/Connections/Connection[@Source='$($slot.Name).ECATSlotIn']"))
        if ($slotConnections.Count -ne 1 -or
            [string]$slotConnections[0].Destination -cne
                'GL_9086_11.EthercatSlotOut_1') {
            throw "$Owner EtherCAT_Network $($slot.Name) must connect exactly once to GL_9086_11.EthercatSlotOut_1."
        }
    }

    Assert-Match $DiagnosticsServiceText '#define LMC_DIAG_TOPOLOGY_REVISION\s+0x15867EEC' "$Owner serializer revision is not the canonical seven-entry CRC."
    Assert-Match $DiagnosticsServiceText '(?s)CatalogIndex = 0x0200.*?pEntry \+ 4\)\^\$UINT := 7.*?pEntry \+ 10\)\^\$UINT := 5.*?pEntry \+ 12\)\^\$UINT := 2.*?pEntry \+ 14\)\^\$UINT := 4' "$Owner serializer topology counts do not match ENI/network 5+2/4-axis contract."
    Assert-Match $DiagnosticsServiceText '(?s)pdoIndex = 0 then.*?\+ 10\)\^\$UINT := 0.*?\+ 24\)\^\$UDINT := 669.*?\+ 28\)\^\$UDINT := 1196200070.*?\+ 32\)\^\$UDINT := 65536.*?GL_9086_11' "$Owner serializer CREVIS slave entry does not match ENI/network index 0."
    Assert-Match $DiagnosticsServiceText '(?s)pdoIndex <= 4 then.*?\+ 10\)\^\$UINT := pdoIndex.*?\+ 24\)\^\$UDINT := 154.*?\+ 28\)\^\$UDINT := 198948.*?\+ 32\)\^\$UDINT := 66592.*?Elmo_11.*?48 \+ pdoIndex' "$Owner serializer Elmo entries do not match ENI/network indices 1..4."
    Assert-Match $DiagnosticsServiceText '(?s)pdoIndex = 5 then.*?1196692218.*?GL_9086_1_Slot001.*?else.*?1196696250.*?GL_9086_1_Slot011' "$Owner serializer slot entries do not match network GT-12FA/GT-22BA order."

    if (-not [string]::IsNullOrWhiteSpace($GeneratedTableText)) {
        foreach ($classIndex in 1..4) {
            Assert-Match $GeneratedTableText ("#define ELMO_{0}_ETHERCATSLAVE_PRODUCT_CODE\s+198948" -f $classIndex) "$Owner generated table Elmo_$classIndex product code is stale."
            Assert-Match $GeneratedTableText ("#define ELMO_{0}_ETHERCATSLAVE_VENDOR_ID\s+154" -f $classIndex) "$Owner generated table Elmo_$classIndex vendor ID is stale."
            Assert-Match $GeneratedTableText ('(?m)^TO_UDINT\({0}\), "SlaveIndex", TO_UDINT\({0}\),//\|EtherCAT_Network\.Elmo_{0}1\.SlaveIndex;' -f $classIndex) "$Owner generated table Elmo_$classIndex SlaveIndex is stale."
        }
        Assert-Match $GeneratedTableText '#define GL_9086_1_ETHERCATSLAVE_PRODUCT_CODE\s+1196200070' "$Owner generated table CREVIS product code is stale."
        Assert-Match $GeneratedTableText '#define GL_9086_1_ETHERCATSLAVE_VENDOR_ID\s+669' "$Owner generated table CREVIS vendor ID is stale."
        Assert-Match $GeneratedTableText '(?m)^TO_UDINT\(22\), "SlaveIndex", TO_UDINT\(0\),//\|EtherCAT_Network\.GL_9086_11\.SlaveIndex;' "$Owner generated table CREVIS SlaveIndex is stale."
        foreach ($slotContract in @(
                @{ Prefix = 'GL_9086_1_SLOT00'; Product = '1196692218'; Direction = 'INPUTS'; PdoIndex = '6000' },
                @{ Prefix = 'GL_9086_1_SLOT01'; Product = '1196696250'; Direction = 'OUTPUTS'; PdoIndex = '7010' })) {
            Assert-Match $GeneratedTableText ("#define $($slotContract.Prefix)_ETHERCATSLAVE_PRODUCT_CODE\s+$($slotContract.Product)") "$Owner generated table $($slotContract.Prefix) product code is stale."
            Assert-Match $GeneratedTableText ("#define $($slotContract.Prefix)_ETHERCATSLAVE_VENDOR_ID\s+669") "$Owner generated table $($slotContract.Prefix) vendor ID is stale."
            foreach ($byteIndex in 0..3) {
                Assert-Match $GeneratedTableText ("#define $($slotContract.Prefix)_$($slotContract.Direction)_BYTE${byteIndex}_INDEX\s+16#$($slotContract.PdoIndex)") "$Owner generated table $($slotContract.Prefix) byte $byteIndex PDO index is stale."
                Assert-Match $GeneratedTableText ("#define $($slotContract.Prefix)_$($slotContract.Direction)_BYTE${byteIndex}_SUBINDEX\s+" + ($byteIndex + 1)) "$Owner generated table $($slotContract.Prefix) byte $byteIndex PDO subindex is stale."
            }
        }
    }

}

function Assert-ConfiguredEtherCATTopologyNegativeFixture {
    param(
        [xml]$Fixture,
        [xml]$NetworkXml,
        [string]$DiagnosticsServiceText,
        [string]$FixtureName,
        [string]$GeneratedTableText = ''
    )

    $rejected = $false
    try {
        Assert-ConfiguredEtherCATTopologyContract `
            -EniXml $Fixture `
            -NetworkXml $NetworkXml `
            -DiagnosticsServiceText $DiagnosticsServiceText `
            -GeneratedTableText $GeneratedTableText `
            -Owner $FixtureName
    }
    catch {
        $rejected = $true
    }
    if (-not $rejected) {
        throw "Configured EtherCAT topology verifier accepted negative fixture '$FixtureName'."
    }
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$topologyIoIdeStructureReady =
    $TopologyIoCheckpoint -eq 'IdeStructureReady'
if ($topologyIoIdeStructureReady -and
    $AllowStaleLasalBinaryMetadata) {
    throw ('IdeStructureReady requires current LASAL Classes.lcb and ' +
        'Networks.lcb evidence; AllowStaleLasalBinaryMetadata is not allowed.')
}
$topologyIoStructureGenerated =
    $TopologyIoCheckpoint -ne 'StaticTopologyOnly'
$topologyIoReadIntegrated = @(
    'IntegratedReadOwnerDormant',
    'IntegratedReadOwner',
    'IntegratedOutputOwnerDormant') -contains $TopologyIoCheckpoint
$topologyIoReadCapabilitiesEnabled = @(
    'IntegratedReadOwner',
    'IntegratedOutputOwnerDormant') -contains $TopologyIoCheckpoint
$topologyIoOutputIntegrated =
    $TopologyIoCheckpoint -eq 'IntegratedOutputOwnerDormant'
$expectedTopologyIoBootZeroCapabilities = if (
    $topologyIoReadCapabilitiesEnabled) {
    '0x0001C007'
}
else {
    '0x00004007'
}
$expectedTopologyIoStableCapabilities = if (
    $topologyIoReadCapabilitiesEnabled) {
    '0x0001E13F'
}
else {
    '0x0000613F'
}
$stPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'
$commNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn'
$commNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st'
$etherCatNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\EtherCAT_Network\EtherCAT_Network.lcn'
$etherCatNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\EtherCAT_Network\ONE_EtherCAT_Network_Table.st'
$eniPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Eni.xml'
$motionNetworkPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\Motion_Network.lcn'
$motionNetworkTablePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Motion_Network\ONE_Motion_Network_Table.st'
$tcpServerPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPIPServer\TCPIPServer.st'
$configObjectsPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\ConfigObjects.st'
$classDbPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
$networkDbPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Networks.lcb'
$protocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcProtocol.cs'
$adminProtocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcAdminProtocol.cs'
$diagnosticsProtocolPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsProtocol.cs'
$diagnosticsModelsPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsD5Models.cs'
$diagnosticsTopologyIoModelsPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsTopologyIoModels.cs'
$diagnosticsTopologyIoPath = Join-Path $root 'LMC_Library\LMC_API_Delivery\src\LmcDiagnosticsTopologyIo.cs'
$diagnosticsLatchPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st'
$diagnosticsServicePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
$recorderStorePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCRecorderStore\LMCRecorderStore.st'
$sdoExecutorPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCSdoExecutor\LMCSdoExecutor.st'
$controlCommandServicePath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
$projectPath = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcp'

$st = Get-Content -Raw -LiteralPath $stPath
$commNetwork = Get-Content -Raw -LiteralPath $commNetworkPath
$etherCatNetwork = Get-Content -Raw -LiteralPath $etherCatNetworkPath
$eni = Get-Content -Raw -LiteralPath $eniPath
$motionNetwork = Get-Content -Raw -LiteralPath $motionNetworkPath
$commNetworkTable = ''
$motionNetworkTable = ''
$etherCatNetworkTable = ''
if (-not $SourceOnly) {
    foreach ($generatedNetworkTable in @(
            @{ Path = $commNetworkTablePath; Name = 'Comm_Network' },
            @{ Path = $motionNetworkTablePath; Name = 'Motion_Network' },
            @{ Path = $etherCatNetworkTablePath; Name = 'EtherCAT_Network' })) {
        if (-not (Test-Path -LiteralPath $generatedNetworkTable.Path -PathType Leaf)) {
            throw (
                "LASAL generated table for $($generatedNetworkTable.Name) is missing: " +
                "$($generatedNetworkTable.Path). Save the Object Network and complete a " +
                'successful LASAL Rebuild before running the full static contract; do not ' +
                'restore a stale table from Git.')
        }
    }
    $commNetworkTable = Get-Content -Raw -LiteralPath $commNetworkTablePath
    $motionNetworkTable = Get-Content -Raw -LiteralPath $motionNetworkTablePath
    $etherCatNetworkTable = Get-Content -Raw -LiteralPath $etherCatNetworkTablePath
}
$tcpServer = Get-Content -Raw -LiteralPath $tcpServerPath
$takeoverCommNetworkTable = Get-Content -Raw -LiteralPath $commNetworkTablePath
$configObjects = Get-Content -Raw -LiteralPath $configObjectsPath
$classDbText = [Text.Encoding]::ASCII.GetString(
    [IO.File]::ReadAllBytes($classDbPath))
$networkDbText = [Text.Encoding]::ASCII.GetString(
    [IO.File]::ReadAllBytes($networkDbPath))
$protocol = Get-Content -Raw -LiteralPath $protocolPath
$adminProtocol = Get-Content -Raw -LiteralPath $adminProtocolPath
$diagnosticsProtocol = Get-Content -Raw -LiteralPath $diagnosticsProtocolPath
$diagnosticsModels = Get-Content -Raw -LiteralPath $diagnosticsModelsPath
$diagnosticsTopologyIoModels =
    Get-Content -Raw -LiteralPath $diagnosticsTopologyIoModelsPath
$diagnosticsTopologyIo = Get-Content -Raw -LiteralPath $diagnosticsTopologyIoPath
$diagnosticsLatch = Get-Content -Raw -LiteralPath $diagnosticsLatchPath
$diagnosticsService = Get-Content -Raw -LiteralPath $diagnosticsServicePath
$recorderStore = Get-Content -Raw -LiteralPath $recorderStorePath
$sdoExecutor = Get-Content -Raw -LiteralPath $sdoExecutorPath
$controlCommandService = Get-Content -Raw -LiteralPath $controlCommandServicePath
$project = Get-Content -Raw -LiteralPath $projectPath

[xml]$commNetworkXml = $commNetwork
[xml]$etherCatNetworkXml = $etherCatNetwork
[xml]$eniXml = $eni
[xml]$motionNetworkXml = $motionNetwork

Assert-TCPIPServerControlledShutdownContract `
    -ServerText $tcpServer `
    -Owner 'TCPIPServer'
Assert-Match $project (
    '<File\s+Path="\.\\Class\\TCPIPServer\\TCPIPServer\.st"\s*/>') (
    'LASAL project does not register Class\TCPIPServer\TCPIPServer.st.')
if ($project -match
    '<File\s+Path="\.\\Class\\_TCPIPServer_RT\\_TCPIPServer_RT\.st"\s*/>') {
    throw 'LASAL project still registers the obsolete _TCPIPServer_RT class.'
}

Assert-TCPSamePeerGeneratedNetworkContract `
    -CommNetworkXml $commNetworkXml `
    -GeneratedNetworkTable $takeoverCommNetworkTable `
    -ConfigObjectsText $configObjects `
    -Owner 'Comm_Network'

[xml]$tcpTakeoverMaxConnectionsFixture = $commNetworkXml.OuterXml
$tcpTakeoverMaxConnectionsFixture.SelectSingleNode(
    "/Network/Components/Object[@Name='TCPIPServer1']/Channels/Client[@Name='MaxConnections']").Value = '1'
[xml]$tcpTakeoverClassFixture = $commNetworkXml.OuterXml
$tcpTakeoverClassFixture.SelectSingleNode(
    "/Network/Components/Object[@Name='TCPIPServer1']").Class = '_TCPIPServer'
[xml]$tcpTakeoverConnectionFixture = $commNetworkXml.OuterXml
$tcpTakeoverConnectionFixture.SelectSingleNode(
    "/Network/Connections/Connection[@Source='TCPMotionInterface1._TCPIPServer']").Destination = '_TCPIPServer1.Control'
$tcpTakeoverGeneratedValueFixture = ([regex]::new(
    ('("MaxConnections",\s*TO_UDINT\()2' +
     '(\),//\|Comm_Network\.TCPIPServer1\.MaxConnections;)'))).Replace(
        $takeoverCommNetworkTable,
        '${1}1${2}',
        1)
$tcpTakeoverConfigObjectsFixture = ([regex]::new(
    '(?m)^(\s*0\$UINT,\s*0,\s*0,\s*)"TCPIPSERVER"(,\s*)$')).Replace(
        $configObjects,
        '${1}"_TCPIPSERVER_RT"${2}',
        1)
$tcpTakeoverNetworkNegativeFixtures = [ordered]@{
    MaxConnectionsOne = @{
        Xml = $tcpTakeoverMaxConnectionsFixture
        Table = $takeoverCommNetworkTable
        Config = $configObjects
    }
    WrongServerClass = @{
        Xml = $tcpTakeoverClassFixture
        Table = $takeoverCommNetworkTable
        Config = $configObjects
    }
    LegacyConnectionTarget = @{
        Xml = $tcpTakeoverConnectionFixture
        Table = $takeoverCommNetworkTable
        Config = $configObjects
    }
    GeneratedMaxConnectionsOne = @{
        Xml = $commNetworkXml
        Table = $tcpTakeoverGeneratedValueFixture
        Config = $configObjects
    }
    LegacyConfigClass = @{
        Xml = $commNetworkXml
        Table = $takeoverCommNetworkTable
        Config = $tcpTakeoverConfigObjectsFixture
    }
}
$tcpTakeoverNetworkNegativeFixtureCount = 0
foreach ($negativeFixture in
        $tcpTakeoverNetworkNegativeFixtures.GetEnumerator()) {
    $fixture = $negativeFixture.Value
    $fixtureMutated =
        $fixture.Xml.OuterXml -cne $commNetworkXml.OuterXml -or
        $fixture.Table -cne $takeoverCommNetworkTable -or
        $fixture.Config -cne $configObjects
    if (-not $fixtureMutated) {
        throw (
            'Same-peer generated-network negative fixture did not mutate ' +
            "the contract for '$($negativeFixture.Key)'.")
    }
    $negativeRejected = $false
    try {
        Assert-TCPSamePeerGeneratedNetworkContract `
            -CommNetworkXml $fixture.Xml `
            -GeneratedNetworkTable $fixture.Table `
            -ConfigObjectsText $fixture.Config `
            -Owner 'Comm_Network'
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'Same-peer generated-network verifier accepted negative fixture ' +
            "'$($negativeFixture.Key)'.")
    }
    $tcpTakeoverNetworkNegativeFixtureCount++
}
if ($tcpTakeoverNetworkNegativeFixtureCount -ne 5) {
    throw (
        'Same-peer generated-network negative fixture count is ' +
        "$tcpTakeoverNetworkNegativeFixtureCount, expected five.")
}

Assert-LMCEcatInputLatchNetworkSourceContract `
    -CommNetworkXml $commNetworkXml `
    -MotionNetworkXml $motionNetworkXml `
    -EtherCatNetworkXml $etherCatNetworkXml `
    -Owner 'Diagnostics latch source network'

Assert-ConfiguredEtherCATTopologyContract `
    -EniXml $eniXml `
    -NetworkXml $etherCatNetworkXml `
    -DiagnosticsServiceText $diagnosticsService `
    -GeneratedTableText $etherCatNetworkTable `
    -Owner 'Configured EtherCAT topology'

[xml]$orderFixture = $eniXml.OuterXml
$orderFixtureSlaves = @($orderFixture.EtherCATConfig.Config.Slave)
$firstSlaveXml = $orderFixtureSlaves[0].InnerXml
$orderFixtureSlaves[0].InnerXml = $orderFixtureSlaves[1].InnerXml
$orderFixtureSlaves[1].InnerXml = $firstSlaveXml
Assert-ConfiguredEtherCATTopologyNegativeFixture $orderFixture $etherCatNetworkXml $diagnosticsService 'swapped slave order'

[xml]$identityFixture = $eniXml.OuterXml
$identityFixture.SelectSingleNode(
    '/EtherCATConfig/Config/Slave[1]/Info/ProductCode').InnerText = '198948'
Assert-ConfiguredEtherCATTopologyNegativeFixture $identityFixture $etherCatNetworkXml $diagnosticsService 'changed slave identity'

[xml]$pdoFixture = $eniXml.OuterXml
$pdoFixture.SelectSingleNode(
    '/EtherCATConfig/Config/Slave[1]/ProcessData/TxPdo[1]/Entry[1]/SubIndex').InnerText = '#x2'
Assert-ConfiguredEtherCATTopologyNegativeFixture $pdoFixture $etherCatNetworkXml $diagnosticsService 'changed CREVIS PDO mapping'

[xml]$networkSlaveIndexFixture = $etherCatNetworkXml.OuterXml
$null = $networkSlaveIndexFixture.SelectSingleNode(
    "/Network/Components/Object[@Name='GL_9086_11']/Channels/Client[@Name='SlaveIndex']").SetAttribute(
        'Value',
        '4')
Assert-ConfiguredEtherCATTopologyNegativeFixture $eniXml $networkSlaveIndexFixture $diagnosticsService 'changed network SlaveIndex'

[xml]$networkSlotFixture = $etherCatNetworkXml.OuterXml
$null = $networkSlotFixture.SelectSingleNode(
    "/Network/Components/Object[@Name='GL_9086_1_Slot001']/Channels/Client[@Name='Slot']").SetAttribute(
        'Value',
        '1')
Assert-ConfiguredEtherCATTopologyNegativeFixture $eniXml $networkSlotFixture $diagnosticsService 'changed network slot index'

[xml]$networkSlotConnectionFixture = $etherCatNetworkXml.OuterXml
$networkSlotConnections = $networkSlotConnectionFixture.SelectSingleNode(
    '/Network/Connections')
$extraSlotConnection = $networkSlotConnectionFixture.CreateElement('Connection')
$extraSlotConnection.SetAttribute(
    'Source',
    'GL_9086_1_Slot001.ECATSlotIn')
$extraSlotConnection.SetAttribute('Destination', 'Elmo_11.ClassState')
$null = $networkSlotConnections.AppendChild($extraSlotConnection)
Assert-ConfiguredEtherCATTopologyNegativeFixture $eniXml $networkSlotConnectionFixture $diagnosticsService 'changed network slot connection'

$serializerRevisionFixture = $diagnosticsService.Replace(
    '#define LMC_DIAG_TOPOLOGY_REVISION    0x15867EEC',
    '#define LMC_DIAG_TOPOLOGY_REVISION    0x15867EED')
Assert-ConfiguredEtherCATTopologyNegativeFixture $eniXml $etherCatNetworkXml $serializerRevisionFixture 'changed serializer revision'

$serializerCountFixture = $diagnosticsService.Replace(
    '(pEntry + 4)^$UINT := 7;',
    '(pEntry + 4)^$UINT := 8;')
Assert-ConfiguredEtherCATTopologyNegativeFixture $eniXml $etherCatNetworkXml $serializerCountFixture 'changed serializer topology count'

if (-not $SourceOnly) {
    $generatedTableFixture = $etherCatNetworkTable.Replace(
        '#define GL_9086_1_ETHERCATSLAVE_PRODUCT_CODE 1196200070',
        '#define GL_9086_1_ETHERCATSLAVE_PRODUCT_CODE 1196200071')
    Assert-ConfiguredEtherCATTopologyNegativeFixture `
        $eniXml `
        $etherCatNetworkXml `
        $diagnosticsService `
        'changed generated EtherCAT table identity' `
        $generatedTableFixture
}

$controlServiceClassBlock = [regex]::Match(
    $controlCommandService,
    '(?s)LMCControlCommandService\s*:\s*CLASS.*?END_CLASS;').Value
if ([string]::IsNullOrWhiteSpace($controlServiceClassBlock)) {
    throw 'LMCControlCommandService generated class declaration was not found.'
}
$controlServiceMetadataBlock = [regex]::Match(
    $controlCommandService,
    '(?s)<Class\s+.*?Name\s*=\s*"LMCControlCommandService".*?</Class>').Value
if ([string]::IsNullOrWhiteSpace($controlServiceMetadataBlock)) {
    throw 'LMCControlCommandService generated class metadata was not found.'
}

foreach ($classProperty in @(
    @{ Name = 'RealtimeTask'; Value = 'false' },
    @{ Name = 'CyclicTask'; Value = 'false' },
    @{ Name = 'BackgroundTask'; Value = 'false' },
    @{ Name = 'Automatic'; Value = 'false' },
    @{ Name = 'SharedCommandTable'; Value = 'true' })) {
    Assert-Match $controlCommandService (
        [regex]::Escape($classProperty.Name) +
        '\s*=\s*"' + [regex]::Escape($classProperty.Value) + '"') (
        "LMCControlCommandService.$($classProperty.Name) must be $($classProperty.Value).")
}

Assert-Match $controlServiceClassBlock '(?m)^\s*ClassSvr\s*:\s*SvrChCmd_DINT\s*;\s*$' 'LMCControlCommandService.ClassSvr command server declaration is missing.'
foreach ($axisNumber in 1..9) {
    $axisClientName = "LMCAxis$axisNumber"
    Assert-Match $controlServiceClassBlock (
        '(?m)^\s*' + [regex]::Escape($axisClientName) +
        '\s*:\s*CltChCmd__LMCAxis\s*;\s*$') (
        "LMCControlCommandService.$axisClientName must be an _LMCAxis object command client.")
    Assert-Match $controlServiceMetadataBlock (
        '<Client\s+Name="' + [regex]::Escape($axisClientName) +
        '"\s+Required="true"\s+Internal="false"\s*/>') (
        "LMCControlCommandService.$axisClientName must be generated as a required external client.")
}
Assert-Match $controlServiceClassBlock '(?m)^\s*LMCRobot\s*:\s*CltChCmd__LMCRobotBase\s*;\s*$' 'LMCControlCommandService.LMCRobot must be an _LMCRobotBase object command client.'
Assert-Match $controlServiceMetadataBlock '<Client\s+Name="LMCRobot"\s+Required="true"\s+Internal="false"\s*/>' 'LMCControlCommandService.LMCRobot must be generated as a required external client.'
$controlServiceMetadataClients = [regex]::Matches(
    $controlServiceMetadataBlock,
    '<Client\s+Name="[^"]+"[^>]*/>')
if ($controlServiceMetadataClients.Count -ne 10) {
    throw "LMCControlCommandService metadata client count is $($controlServiceMetadataClients.Count), expected ten."
}

$controlServiceTableBlock = [regex]::Match(
    $controlCommandService,
    '(?s)FUNCTION GLOBAL TAB LMCControlCommandService::@CT_.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($controlServiceTableBlock)) {
    throw 'LMCControlCommandService generated command table was not found.'
}
Assert-Match $controlServiceTableBlock '(?m)^\s*1\$UINT,\s*10\$UINT,\s*0\$UINT,\s*$' 'LMCControlCommandService generated server/client/data counts are not exactly 1/10/0.'

$controlServiceServerEntries = [regex]::Matches(
    $controlServiceTableBlock,
    '\(::LMCControlCommandService\.[A-Za-z_][A-Za-z0-9_]*\.pMeth\)\$UINT')
if ($controlServiceServerEntries.Count -ne 1) {
    throw "LMCControlCommandService generated server entry count is $($controlServiceServerEntries.Count), expected one."
}
Assert-Match $controlServiceTableBlock '(?m)^\s*\(::LMCControlCommandService\.ClassSvr\.pMeth\)\$UINT,\s*_CH_CMD\$UINT,.*"ClassSvr"' 'LMCControlCommandService.ClassSvr generated metadata is missing.'

$controlServiceClientLines = [regex]::Matches(
    $controlServiceTableBlock,
    '(?m)^\s*\(::LMCControlCommandService\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)\.pCh\)\$UINT.*$')
if ($controlServiceClientLines.Count -ne 10) {
    throw "LMCControlCommandService generated client entry count is $($controlServiceClientLines.Count), expected ten."
}
foreach ($clientLine in $controlServiceClientLines) {
    if ($clientLine.Value -notmatch
        '_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT') {
        throw ("LMCControlCommandService.$($clientLine.Groups['Name'].Value) " +
            'is not generated as a required object client.')
    }
}
if ($controlServiceTableBlock -match '_CH_CLT\$UINT') {
    throw 'LMCControlCommandService contains a generated scalar client entry.'
}
foreach ($axisNumber in 1..9) {
    $axisClientName = "LMCAxis$axisNumber"
    Assert-Match $controlServiceTableBlock (
        '(?m)^\s*\(::LMCControlCommandService\.' +
        [regex]::Escape($axisClientName) +
        '\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*' +
        '2#0000000000000010\$UINT,.*"' +
        [regex]::Escape($axisClientName) + '".*"_LMCAxis"') (
        "LMCControlCommandService.$axisClientName generated object-client metadata is missing.")
}
Assert-Match $controlServiceTableBlock '(?m)^\s*\(::LMCControlCommandService\.LMCRobot\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT,.*"LMCRobot".*"_LMCRobotBase"' 'LMCControlCommandService.LMCRobot required object-client metadata is missing.'

$controlServicePragmas = [regex]::Matches(
    $controlCommandService,
    '(?m)^\s*#pragma usingLtd\s+(?<Class>[A-Za-z_][A-Za-z0-9_]*)\s*$')
if ($controlServicePragmas.Count -ne 2 -or
    @($controlServicePragmas | Where-Object {
            $_.Groups['Class'].Value -eq '_LMCAxis' }).Count -ne 1 -or
    @($controlServicePragmas | Where-Object {
            $_.Groups['Class'].Value -eq '_LMCRobotBase' }).Count -ne 1) {
    throw 'LMCControlCommandService must have exactly the _LMCAxis and _LMCRobotBase limited-using pragmas.'
}
if ($controlCommandService -match '(?:#pragma usingLtd\s+_StdLib|\b_StdLib\b)') {
    throw 'LMCControlCommandService must not depend on an _StdLib client.'
}

$controlServiceRequestInputs = @(
    @{ Name = 'CommandId'; Type = 'UINT' },
    @{ Name = 'Reference'; Type = 'UINT' },
    @{ Name = 'pRequestFrame'; Type = '^USINT' },
    @{ Name = 'RequestFrameSize'; Type = 'UDINT' },
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' })
$controlServiceResponseOutput = @(
    @{ Name = 'ResponseSize'; Type = 'DINT' })

Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'HandleRequest' `
    -IsGlobal $true `
    -Inputs $controlServiceRequestInputs `
    -Outputs $controlServiceResponseOutput

$controlServicePrivateMethods = @(
    'HandleAdminCommands',
    'HandleRegistryCommands',
    'HandleAxisCommands',
    'HandleGroupCommands',
    'MoveLinearAbsEx',
    'GroupReadStatus')
foreach ($methodName in $controlServicePrivateMethods[0..3]) {
    Assert-ExactLasalFunctionAbi `
        -ClassBlock $controlServiceClassBlock `
        -FunctionName $methodName `
        -IsGlobal $false `
        -Inputs $controlServiceRequestInputs `
        -Outputs $controlServiceResponseOutput
}

$moveLinearAbsExInputs = @(
    @{ Name = 'Reference'; Type = 'UINT' },
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' },
    @{ Name = 'pRequestFrame'; Type = '^USINT' },
    @{ Name = 'RequestFrameSize'; Type = 'UDINT' })
Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'MoveLinearAbsEx' `
    -IsGlobal $false `
    -Inputs $moveLinearAbsExInputs `
    -Outputs $controlServiceResponseOutput

$groupReadStatusInputs = @(
    @{ Name = 'pResponseFrame'; Type = '^USINT' },
    @{ Name = 'ResponseCapacity'; Type = 'UDINT' })
Assert-ExactLasalFunctionAbi `
    -ClassBlock $controlServiceClassBlock `
    -FunctionName 'GroupReadStatus' `
    -IsGlobal $false `
    -Inputs $groupReadStatusInputs `
    -Outputs $controlServiceResponseOutput

$controlServiceClassDbRecord = Get-LasalClassDatabaseRecord `
    -DatabaseText $classDbText `
    -SourcePath '.\Class\LMCControlCommandService\LMCControlCommandService.st' `
    -ClassName 'LMCControlCommandService'
foreach ($generatedMemberName in @(
        'HandleRequest',
        'HandleAdminCommands',
        'HandleRegistryCommands',
        'HandleAxisCommands',
        'HandleGroupCommands',
        'MoveLinearAbsEx',
        'GroupReadStatus')) {
    Assert-Match $controlServiceClassDbRecord (
        '(?<![A-Za-z0-9_])' + [regex]::Escape($generatedMemberName) +
        '(?![A-Za-z0-9_])') (
        "LASAL Classes.lcb LMCControlCommandService record is missing $generatedMemberName.")
}
$tcpClassDbRecord = Get-LasalClassDatabaseRecord `
    -DatabaseText $classDbText `
    -SourcePath '.\Class\TCPMotionInterface\TCPMotionInterface.st' `
    -ClassName 'TCPMotionInterface'
if (-not $SourceOnly) {
    $tcpServerClassDbRecord = ''
    try {
        $tcpServerClassDbRecord = Get-LasalClassDatabaseRecord `
            -DatabaseText $classDbText `
            -SourcePath '.\Class\TCPIPServer\TCPIPServer.st' `
            -ClassName 'TCPIPServer'
    }
    catch {
        throw (
            'LASAL Classes.lcb is stale for same-peer takeover: it still ' +
            'lacks Class\TCPIPServer\TCPIPServer.st (the previous build ' +
            'registered _TCPIPServer_RT). Open the master LASAL project, ' +
            'Save/Rebuild, and rerun the full static contract.')
    }
    Assert-Match $tcpServerClassDbRecord (
        '(?<![A-Za-z0-9_])SetSocketParameter(?![A-Za-z0-9_])') (
        'LASAL Classes.lcb TCPIPServer record is missing SetSocketParameter.')
    if ($tcpServerClassDbRecord -match
        '(?<![A-Za-z0-9_])(?:RtWork|CyWork)(?![A-Za-z0-9_])') {
        throw 'LASAL Classes.lcb TCPIPServer record still declares RtWork/CyWork.'
    }
}
if (-not ($ControlServiceCheckpoint -eq 'Phase5TransportClean' -and
        $AllowStaleLasalBinaryMetadata)) {
    Assert-Match $tcpClassDbRecord '(?<![A-Za-z0-9_])ControlCommands(?![A-Za-z0-9_])' 'LASAL Classes.lcb TCPMotionInterface record is missing ControlCommands.'
}

if ($topologyIoIdeStructureReady -and -not $SourceOnly) {
    $topologyIoLatchClassDbRecord = Get-LasalClassDatabaseRecord `
        -DatabaseText $classDbText `
        -SourcePath '.\Class\LMCEcatInputLatch\LMCEcatInputLatch.st' `
        -ClassName 'LMCEcatInputLatch'
    foreach ($generatedLatchMemberName in @(
            'Coupler',
            'InputSlot',
            'OutputSlot',
            'OutputRevision',
            'OutputObserved',
            'OutputPreviousValid',
            'OutputPreviousValue',
            'CopyTopologyIoSnapshot',
            'AdvanceOutputRevision')) {
        Assert-Match $topologyIoLatchClassDbRecord (
            '(?<![A-Za-z0-9_])' +
            [regex]::Escape($generatedLatchMemberName) +
            '(?![A-Za-z0-9_])') (
            'LASAL Classes.lcb LMCEcatInputLatch record is missing ' +
            "$generatedLatchMemberName. Save the IDE structure and Rebuild.")
    }

    $topologyIoServiceClassDbRecord = Get-LasalClassDatabaseRecord `
        -DatabaseText $classDbText `
        -SourcePath '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st' `
        -ClassName 'LMCDiagnosticsService'
    Assert-Match $topologyIoServiceClassDbRecord (
        '(?<![A-Za-z0-9_])HandleEtherCATTopologyIoRequest' +
        '(?![A-Za-z0-9_])') (
        'LASAL Classes.lcb LMCDiagnosticsService record is missing ' +
        'HandleEtherCATTopologyIoRequest. Save the IDE structure and Rebuild.')
}

$controlServiceHandleRequestBlock = [regex]::Match(
    $controlCommandService,
    '(?s)FUNCTION GLOBAL LMCControlCommandService::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($controlServiceHandleRequestBlock)) {
    throw 'LMCControlCommandService.HandleRequest implementation was not found.'
}
$controlServicePrivateBlocks = [ordered]@{}
foreach ($methodName in $controlServicePrivateMethods) {
    $privateMethodBlock = [regex]::Match(
        $controlCommandService,
        ('(?s)FUNCTION LMCControlCommandService::' +
         [regex]::Escape($methodName) + '.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($privateMethodBlock)) {
        throw "LMCControlCommandService.$methodName implementation was not found."
    }
    $controlServicePrivateBlocks[$methodName] = $privateMethodBlock
}
$controlServiceMethodBlocks = [ordered]@{
    HandleRequest = $controlServiceHandleRequestBlock
}
foreach ($methodName in $controlServicePrivateMethods) {
    $controlServiceMethodBlocks[$methodName] =
        $controlServicePrivateBlocks[$methodName]
}
foreach ($methodEntry in $controlServiceMethodBlocks.GetEnumerator()) {
    $methodByteCount = [Text.Encoding]::UTF8.GetByteCount($methodEntry.Value)
    if ($methodByteCount -gt 32768) {
        throw ("LMCControlCommandService.$($methodEntry.Key) is " +
            "$methodByteCount bytes, expected at most 32768.")
    }
}

$phase3GroupCommandIds = @(
    '20D2',
    '2047',
    '2048',
    '2049',
    '204A',
    '204B',
    '2085',
    '20A4',
    '2045',
    '2051',
    '20E7')
$phase3AdminCommandIds = @('7D20', '7D22')
$phase4GeneralAdminCommandIds = @('7D00', '7D10')
$phase4RegistryCommandIds = @('103C', '1042', '202B')
$phase4AxisCommandIds = @(
    '2023',
    '2024',
    '2022',
    '2028',
    '202E',
    '209F',
    '20A0',
    '20A2')
$allAdminCommandIds = @(
    $phase4GeneralAdminCommandIds + $phase3AdminCommandIds)
$allControlCommandIds = @(
    $allAdminCommandIds +
    $phase4RegistryCommandIds +
    $phase4AxisCommandIds +
    $phase3GroupCommandIds)
$topologyIoCommandIds = @('7E11', '7E12')
if ($topologyIoReadIntegrated) {
    $topologyIoCommandIds += @('7E13', '7E22')
}
if ($topologyIoOutputIntegrated) {
    $topologyIoCommandIds += '7E23'
}

$diagnosticsCommandIds = @(
    '7E00',
    '7E01',
    '7E02',
    '7E03',
    '7E04',
    '7E10',
    '7E11',
    '7E12')
if ($topologyIoReadIntegrated) {
    $diagnosticsCommandIds += '7E13'
}
$diagnosticsCommandIds += @(
    '7E20',
    '7E21')
if ($topologyIoReadIntegrated) {
    $diagnosticsCommandIds += '7E22'
}
if ($topologyIoOutputIntegrated) {
    $diagnosticsCommandIds += '7E23'
}
$diagnosticsCommandIds += @(
    '7E30',
    '7E31',
    '7E32',
    '7E33',
    '7E40',
    '7E41',
    '7E42',
    '7E43',
    '7E44',
    '7E45',
    '7E46',
    '7E47',
    '7E48',
    '7E49',
    '7E4A',
    '7E4B',
    '7E4C',
    '7E4D',
    '7E50',
    '7E51')

$controlServiceGroupRouted = @(
    'Phase3GroupRouted',
    'Phase4AllControlRouted',
    'Phase4DiagnosticsRouted',
    'Phase5TransportClean') -contains $ControlServiceCheckpoint
$controlServiceAllControlRouted = @(
    'Phase4AllControlRouted',
    'Phase4DiagnosticsRouted',
    'Phase5TransportClean') -contains $ControlServiceCheckpoint
$diagnosticsServiceRouted = @(
    'Phase4DiagnosticsRouted',
    'Phase5TransportClean') -contains $ControlServiceCheckpoint
$transportClean =
    $ControlServiceCheckpoint -eq 'Phase5TransportClean'
if ($transportClean -and $AllowStaleLasalBinaryMetadata) {
    Write-Warning (
        'Phase5TransportClean is bypassing LASAL Classes.lcb/Networks.lcb ' +
        'registration gates, including unsynchronized SDO Write declarations. ' +
        'Source, XML, and generated ONE_* table contracts remain enforced.')
}
$transportControlCommandIds = if ($controlServiceAllControlRouted) {
    $allControlCommandIds
}
else {
    $phase3GroupCommandIds + $phase3AdminCommandIds
}

switch ($ControlServiceCheckpoint) {
    'Phase2Skeleton' {
        Assert-LasalFailClosedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in $controlServicePrivateMethods) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
    }

    'Phase3GroupDormant' {
        Assert-LasalFailClosedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        foreach ($methodName in @(
                'HandleGroupCommands',
                'HandleAdminCommands',
                'MoveLinearAbsEx',
                'GroupReadStatus')) {
            Assert-LasalImplementedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleGroupCommands'] `
            -Owner 'LMCControlCommandService.HandleGroupCommands' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAdminCommands'] `
            -Owner 'LMCControlCommandService.HandleAdminCommands' `
            -ExpectedCommandIds $phase3AdminCommandIds
    }

    'Phase3GroupRouted' {
        Assert-LasalImplementedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            Assert-LasalFailClosedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        foreach ($methodName in @(
                'HandleGroupCommands',
                'HandleAdminCommands',
                'MoveLinearAbsEx',
                'GroupReadStatus')) {
            Assert-LasalImplementedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -ExpectedCommandIds ($phase3GroupCommandIds + $phase3AdminCommandIds)
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleGroupCommands'] `
            -Owner 'LMCControlCommandService.HandleGroupCommands' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAdminCommands'] `
            -Owner 'LMCControlCommandService.HandleAdminCommands' `
            -ExpectedCommandIds $phase3AdminCommandIds
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest group ownership' `
            -CallPattern 'ResponseSize\s*:=\s*HandleGroupCommands\s*\(' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest Admin ownership' `
            -CallPattern 'ResponseSize\s*:=\s*HandleAdminCommands\s*\(' `
            -ExpectedCommandIds $phase3AdminCommandIds
        foreach ($handlerName in @(
                'HandleGroupCommands',
                'HandleAdminCommands')) {
            $handlerCallCount = [regex]::Matches(
                $controlServiceHandleRequestBlock,
                ('(?<![A-Za-z0-9_.])' +
                 [regex]::Escape($handlerName) + '\s*\(')).Count
            if ($handlerCallCount -ne 1) {
                throw (
                    "$ControlServiceCheckpoint LMCControlCommandService." +
                    "HandleRequest $handlerName call count is " +
                    "$handlerCallCount, expected one.")
            }
            Assert-Match $controlServiceHandleRequestBlock (
                '(?s)ResponseSize\s*:=\s*' +
                [regex]::Escape($handlerName) + '\(\s*' +
                'CommandId:=CommandId\s*,\s*' +
                'Reference:=Reference\s*,\s*' +
                'pRequestFrame:=pRequestFrame\s*,\s*' +
                'RequestFrameSize:=RequestFrameSize\s*,\s*' +
                'pResponseFrame:=pResponseFrame\s*,\s*' +
                'ResponseCapacity:=ResponseCapacity\s*\)') (
                "$ControlServiceCheckpoint HandleRequest does not pass the " +
                "complete zero-copy ABI to $handlerName.")
        }
        foreach ($handlerName in @(
                'HandleRegistryCommands',
                'HandleAxisCommands')) {
            if ($controlServiceHandleRequestBlock -match (
                    '(?<![A-Za-z0-9_.])' +
                    [regex]::Escape($handlerName) + '\s*\(')) {
                throw (
                    "$ControlServiceCheckpoint LMCControlCommandService." +
                    "HandleRequest already routes to $handlerName.")
            }
        }
        Assert-Match $controlServiceHandleRequestBlock (
            '(?s)ResponseSize\s*:=\s*-1\s*;.*?' +
            'if\s+\(pRequestFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(pResponseFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(RequestFrameSize\s*<\s*8\)\s+then\s*RETURN;\s*end_if;.*?' +
            'case\s+CommandId\s+of.*?' +
            'else\s+ResponseSize\s*:=\s*-1\s*;\s*end_case') (
            'Phase3GroupRouted HandleRequest unsupported-command fail-closed path is missing.')
    }

    { $_ -in @(
            'Phase4AllControlRouted',
            'Phase4DiagnosticsRouted',
            'Phase5TransportClean') } {
        Assert-LasalImplementedBody `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -Checkpoint $ControlServiceCheckpoint
        foreach ($methodName in $controlServicePrivateMethods) {
            Assert-LasalImplementedBody `
                -FunctionBlock $controlServicePrivateBlocks[$methodName] `
                -Owner "LMCControlCommandService.$methodName" `
                -Checkpoint $ControlServiceCheckpoint
        }
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServiceHandleRequestBlock `
            -Owner 'LMCControlCommandService.HandleRequest' `
            -ExpectedCommandIds $allControlCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleGroupCommands'] `
            -Owner 'LMCControlCommandService.HandleGroupCommands' `
            -ExpectedCommandIds $phase3GroupCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAdminCommands'] `
            -Owner 'LMCControlCommandService.HandleAdminCommands' `
            -ExpectedCommandIds $allAdminCommandIds
        Assert-ExactLasalCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleRegistryCommands'] `
            -Owner 'LMCControlCommandService.HandleRegistryCommands' `
            -ExpectedCommandIds $phase4RegistryCommandIds
        Assert-ExactLasalTopLevelCommandCaseIds `
            -FunctionBlock $controlServicePrivateBlocks['HandleAxisCommands'] `
            -Owner 'LMCControlCommandService.HandleAxisCommands' `
            -ExpectedCommandIds $phase4AxisCommandIds

        foreach ($routeContract in @(
                @{ Handler = 'HandleAdminCommands'; Ids = $allAdminCommandIds },
                @{ Handler = 'HandleRegistryCommands'; Ids = $phase4RegistryCommandIds },
                @{ Handler = 'HandleAxisCommands'; Ids = $phase4AxisCommandIds },
                @{ Handler = 'HandleGroupCommands'; Ids = $phase3GroupCommandIds })) {
            Assert-ExactLasalCommandRouteIds `
                -RouterBlock $controlServiceHandleRequestBlock `
                -Owner (
                    'LMCControlCommandService.HandleRequest ' +
                    $routeContract.Handler + ' ownership') `
                -CallPattern (
                    'ResponseSize\s*:=\s*' +
                    [regex]::Escape($routeContract.Handler) + '\s*\(') `
                -ExpectedCommandIds $routeContract.Ids

            $handlerCallCount = [regex]::Matches(
                $controlServiceHandleRequestBlock,
                ('(?<![A-Za-z0-9_.])' +
                 [regex]::Escape($routeContract.Handler) + '\s*\(')).Count
            if ($handlerCallCount -ne 1) {
                throw (
                    "$ControlServiceCheckpoint LMCControlCommandService." +
                    "HandleRequest $($routeContract.Handler) call count is " +
                    "$handlerCallCount, expected one.")
            }
            Assert-Match $controlServiceHandleRequestBlock (
                '(?s)ResponseSize\s*:=\s*' +
                [regex]::Escape($routeContract.Handler) + '\(\s*' +
                'CommandId:=CommandId\s*,\s*' +
                'Reference:=Reference\s*,\s*' +
                'pRequestFrame:=pRequestFrame\s*,\s*' +
                'RequestFrameSize:=RequestFrameSize\s*,\s*' +
                'pResponseFrame:=pResponseFrame\s*,\s*' +
                'ResponseCapacity:=ResponseCapacity\s*\)') (
                "$ControlServiceCheckpoint HandleRequest does not pass the " +
                "complete zero-copy ABI to $($routeContract.Handler).")
        }
        Assert-Match $controlServiceHandleRequestBlock (
            '(?s)ResponseSize\s*:=\s*-1\s*;.*?' +
            'if\s+\(pRequestFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(pResponseFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(RequestFrameSize\s*<\s*8\)\s+then\s*RETURN;\s*end_if;.*?' +
            'case\s+CommandId\s+of.*?' +
            'else\s+ResponseSize\s*:=\s*-1\s*;\s*end_case') (
            "$ControlServiceCheckpoint HandleRequest unsupported-command fail-closed path is missing.")
    }
}

if (-not $controlServiceGroupRouted) {
    foreach ($methodName in $controlServicePrivateMethods) {
        if ($controlServiceHandleRequestBlock -match (
                '(?<![A-Za-z0-9_.])' +
                [regex]::Escape($methodName) + '\s*\(')) {
            throw (
                "$ControlServiceCheckpoint LMCControlCommandService." +
                "HandleRequest already routes to $methodName.")
        }
    }
    if ($controlServiceHandleRequestBlock -match '(?i)\bcase\s+CommandId\b') {
        throw (
            "$ControlServiceCheckpoint LMCControlCommandService." +
            'HandleRequest must remain dormant without command routing.')
    }
}

$controlServiceOwnedSource = $controlServiceClassBlock + "`n" +
    $controlCommandService.Substring(
        $controlCommandService.IndexOf('//{{LSL_IMPLEMENTATION', [StringComparison]::Ordinal))
$forbiddenControlServiceStatePattern = (
    '(?i)(?:_TCPIPServer|_TCPMI_|sigclib_atomic_|' +
    '\b(?:SendData|CurrentSock|ClientFd|Socket|RequestQueue|RequestBuf|' +
    'ReceiveBuf|Sendbuf|SessionEpoch|Ingress|NotifySessionClosed|CyWork|' +
    'RtWork|BackgroundWork|CyclicCall)\b)')
if ($controlServiceOwnedSource -match $forbiddenControlServiceStatePattern) {
    throw "LMCControlCommandService owns forbidden transport/task state '$($Matches[0])'."
}

$controlServiceRegistrationPattern = '<File\s+Path="\.\\Class\\LMCControlCommandService\\LMCControlCommandService\.st"\s*/>'
$controlServiceRegistrationCount = [regex]::Matches(
    $project,
    $controlServiceRegistrationPattern,
    [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
if ($controlServiceRegistrationCount -ne 1) {
    throw "Elmo_EtherCAT_Test_4Axis.lcp LMCControlCommandService registration count is $controlServiceRegistrationCount, expected one."
}

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

$tcpCommandTableBlock = [regex]::Match(
    $st,
    '(?s)FUNCTION GLOBAL TAB TCPMotionInterface::@CT_.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($tcpCommandTableBlock)) {
    throw 'TCPMotionInterface generated command table was not found.'
}
$tcpClassMetadataBlock = [regex]::Match(
    $st,
    '(?s)<Class\s+.*?Name\s*=\s*"TCPMotionInterface".*?</Class>').Value
if ([string]::IsNullOrWhiteSpace($tcpClassMetadataBlock)) {
    throw 'TCPMotionInterface generated class metadata was not found.'
}
$tcpMetadataChannelsBlock = [regex]::Match(
    $tcpClassMetadataBlock,
    '(?s)<Channels>(?<Body>.*?)</Channels>\s*<Network').Groups['Body'].Value
if ([string]::IsNullOrWhiteSpace($tcpMetadataChannelsBlock)) {
    throw 'TCPMotionInterface top-level generated channel metadata was not found.'
}

if ($transportClean) {
    $phase5DeclaredFunctionPattern =
        '(?m)^\s*FUNCTION(?:\s+VIRTUAL)?(?:\s+GLOBAL)?(?:\s+TAB)?\s+' +
        '(?<Value>[A-Za-z_@][A-Za-z0-9_@]*)\b'
    $phase5ImplementedFunctionPattern =
        '(?m)^\s*FUNCTION(?:\s+VIRTUAL)?(?:\s+GLOBAL)?(?:\s+TAB)?\s+' +
        'TCPMotionInterface::(?<Value>[A-Za-z_@][A-Za-z0-9_@]*)\b'
    Assert-Match $tcpCommandTableBlock '(?m)^\s*4\$UINT,\s*3\$UINT,\s*0\$UINT,\s*$' 'Phase5TransportClean TCPMotionInterface generated server/client/data counts are not 4/3/0.'
    Assert-ExactRegexValueSet `
        -Text $tcpCommandTableBlock `
        -Pattern '\(::TCPMotionInterface\.(?<Value>[A-Za-z_][A-Za-z0-9_]*)\.pMeth\)\$UINT,\s*_CH_SVR\$UINT' `
        -Owner 'Phase5TransportClean TCPMotionInterface generated servers' `
        -ExpectedValues @('CurrentSock', 'CommandID', 'AxisRef', 'Payload')
    Assert-ExactRegexValueSet `
        -Text $tcpCommandTableBlock `
        -Pattern '\(::TCPMotionInterface\.(?<Value>[A-Za-z_][A-Za-z0-9_]*)\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT' `
        -Owner 'Phase5TransportClean TCPMotionInterface generated clients' `
        -ExpectedValues @('_StdLib', 'Diagnostics', 'ControlCommands')
    Assert-ExactRegexValueSet `
        -Text $tcpMetadataChannelsBlock `
        -Pattern '<Server\s+Name="(?<Value>[^"]+)"' `
        -Owner 'Phase5TransportClean TCPMotionInterface metadata servers' `
        -ExpectedValues @('CurrentSock', 'CommandID', 'AxisRef', 'Payload')
    Assert-ExactRegexValueSet `
        -Text $tcpMetadataChannelsBlock `
        -Pattern '<Client\s+Name="(?<Value>[^"]+)"' `
        -Owner 'Phase5TransportClean TCPMotionInterface metadata clients' `
        -ExpectedValues @('_StdLib', 'Diagnostics', 'ControlCommands')
    if ($tcpMetadataChannelsBlock -match '<Data\s+Name=') {
        throw 'Phase5TransportClean TCPMotionInterface metadata still declares a data channel.'
    }
}
else {
    Assert-Match $tcpCommandTableBlock '(?m)^\s*20\$UINT,\s*13\$UINT,\s*0\$UINT,\s*$' 'TCPMotionInterface generated server/client/data counts are not 20/13/0.'
    $clientEntries = [regex]::Matches(
        $tcpCommandTableBlock,
        '\(::TCPMotionInterface\.(LMCAxis[1-9]|LMCRobot|_StdLib|Diagnostics|ControlCommands)\.pCh\)\$UINT').Count
    if ($clientEntries -ne 13) {
        throw "TCPMotionInterface generated client entry count is $clientEntries, expected 13."
    }
}

Assert-Match $tcpCommandTableBlock '\(::TCPMotionInterface\.Diagnostics\.pCh\)\$UINT.*"Diagnostics".*"LMCDiagnosticsService"' 'TCPMotionInterface Diagnostics client metadata is missing.'
Assert-Match $st '(?m)^\s*ControlCommands\s*:\s*CltChCmd_LMCControlCommandService\s*;\s*$' 'TCPMotionInterface.ControlCommands object command client declaration is missing.'
Assert-Match $st '<Client\s+Name="ControlCommands"\s+Required="true"\s+Internal="false"\s*/>' 'TCPMotionInterface.ControlCommands must be generated as a required external client.'
Assert-Match $tcpCommandTableBlock '\(::TCPMotionInterface\.ControlCommands\.pCh\)\$UINT,\s*_CH_CLT_OBJ\$UINT,\s*2#0000000000000010\$UINT,.*"ControlCommands".*"LMCControlCommandService"' 'TCPMotionInterface.ControlCommands required object-client metadata is missing.'
Assert-Match $st '(?m)^\s*#pragma usingLtd LMCControlCommandService\s*$' 'TCPMotionInterface LMCControlCommandService limited-using pragma is missing.'
$controlServiceCallCount = [regex]::Matches(
    $st,
    'ControlCommands\s*\.\s*HandleRequest\s*\(').Count
$expectedControlServiceCallCount = if ($controlServiceGroupRouted) { 1 } else { 0 }
if ($controlServiceCallCount -ne $expectedControlServiceCallCount) {
    throw (
        "$ControlServiceCheckpoint TCPMotionInterface " +
        "ControlCommands.HandleRequest call count is $controlServiceCallCount, " +
        "expected $expectedControlServiceCallCount.")
}

if (-not $transportClean) {
    foreach ($axisNumber in 1..9) {
        $clientName = "LMCAxis$axisNumber"
        $linkPattern = [regex]::Escape("TCPMotionInterface1.$clientName") +
            '.*' +
            [regex]::Escape("_LMCAxis$axisNumber.Control")
        Assert-Match $commNetwork $linkPattern "Missing $clientName -> _LMCAxis$axisNumber.Control link in Comm_Network."
    }
}
else {
    $phase5TcpConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[starts-with(@Source,'TCPMotionInterface1.') ]"))
    $phase5DirectMotionConnections = @(
        $phase5TcpConnections | Where-Object {
            $_.Source -match '^TCPMotionInterface1\.(?:LMCAxis[1-9]|LMCRobot)$'
        })
    if ($phase5DirectMotionConnections.Count -ne 0) {
        throw (
            'Phase5TransportClean TCPMotionInterface still has direct ' +
            'axis/robot network connections: ' +
            (($phase5DirectMotionConnections | ForEach-Object {
                        "$($_.Source) -> $($_.Destination)"
                    }) -join ', '))
    }
    $phase5ExpectedTcpConnections = [ordered]@{
        'TCPMotionInterface1._TCPIPServer' = 'TCPIPServer1.Control'
        'TCPMotionInterface1.Diagnostics' = 'LMCDiagnosticsService1.ClassSvr'
        'TCPMotionInterface1.ControlCommands' = 'LMCControlCommandService1.ClassSvr'
    }
    if ($phase5TcpConnections.Count -ne $phase5ExpectedTcpConnections.Count) {
        throw (
            'Phase5TransportClean TCPMotionInterface outgoing network ' +
            "connection count is $($phase5TcpConnections.Count), expected exactly three.")
    }
    foreach ($phase5Connection in $phase5ExpectedTcpConnections.GetEnumerator()) {
        $connectionCount = @(
            $phase5TcpConnections | Where-Object {
                $_.Source -eq $phase5Connection.Key -and
                $_.Destination -eq $phase5Connection.Value
            }).Count
        if ($connectionCount -ne 1) {
            throw (
                'Phase5TransportClean missing or duplicate network connection ' +
                "$($phase5Connection.Key) -> $($phase5Connection.Value).")
        }
    }

    $phase5ServiceMotionConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[starts-with(@Source,'LMCControlCommandService1.') ]") |
            Where-Object {
                $_.Source -match '^LMCControlCommandService1\.(?:LMCAxis[1-9]|LMCRobot)$'
            })
    if ($phase5ServiceMotionConnections.Count -ne 10) {
        throw (
            'Phase5TransportClean LMCControlCommandService axis/robot ' +
            "connection count is $($phase5ServiceMotionConnections.Count), expected ten.")
    }
    foreach ($axisNumber in 1..9) {
        $source = "LMCControlCommandService1.LMCAxis$axisNumber"
        $destination = "_LMCAxis$axisNumber.Control"
        if (@($phase5ServiceMotionConnections | Where-Object {
                    $_.Source -eq $source -and $_.Destination -eq $destination
                }).Count -ne 1) {
            throw "Phase5TransportClean missing or duplicate $source -> $destination service connection."
        }
    }
    if (@($phase5ServiceMotionConnections | Where-Object {
                $_.Source -eq 'LMCControlCommandService1.LMCRobot' -and
                $_.Destination -eq '_LMCRobotBase1.Control'
            }).Count -ne 1) {
        throw 'Phase5TransportClean missing or duplicate LMCControlCommandService1.LMCRobot -> _LMCRobotBase1.Control service connection.'
    }

    if (-not $SourceOnly -and -not $AllowStaleLasalBinaryMetadata) {
        $commNetworkDbRecord = Get-LasalNetworkDatabaseRecord `
            -DatabaseText $networkDbText `
            -SourcePath '.\Network\Comm_Network\Comm_Network.lcn' `
            -NetworkName 'Comm_Network'
        $tcpObjectRecordStart = $commNetworkDbRecord.IndexOf(
            'TCPMotionInterface1',
            [StringComparison]::OrdinalIgnoreCase)
        $tcpObjectRecordEnd = $commNetworkDbRecord.IndexOf(
            '_base',
            $tcpObjectRecordStart + 'TCPMotionInterface1'.Length,
            [StringComparison]::OrdinalIgnoreCase)
        if ($tcpObjectRecordStart -lt 0 -or
            $tcpObjectRecordEnd -le $tcpObjectRecordStart) {
            throw (
                'Phase5TransportClean Networks.lcb TCPMotionInterface1 ' +
                'object registration record could not be isolated.')
        }
        $tcpObjectDbRecord = $commNetworkDbRecord.Substring(
            $tcpObjectRecordStart,
            $tcpObjectRecordEnd - $tcpObjectRecordStart)
        foreach ($retainedTcpObjectMember in @(
                'CurrentSock', 'CommandID', 'AxisRef', 'Payload',
                '_StdLib', 'Diagnostics', 'ControlCommands')) {
            Assert-Match $tcpObjectDbRecord (
                '(?<![A-Za-z0-9_])' +
                [regex]::Escape($retainedTcpObjectMember) +
                '(?![A-Za-z0-9_])') (
                'Phase5TransportClean Networks.lcb TCPMotionInterface1 ' +
                "object record is missing $retainedTcpObjectMember.")
        }
        $staleTcpObjectMembers = @()
        foreach ($removedTcpObjectMember in @(
                'Power', 'pos', 'velo', 'acc', 'dec', 'jer', 'dir',
                'bufMode', 'Exec', 'Reserved', 'ReadPos', 'RetCode',
                'RobotPowerOn', 'RobotPowerOff', 'RobotLock', 'RobotUnLock',
                'LMCAxis1', 'LMCAxis2', 'LMCAxis3', 'LMCAxis4', 'LMCAxis5',
                'LMCAxis6', 'LMCAxis7', 'LMCAxis8', 'LMCAxis9', 'LMCRobot')) {
            if ($tcpObjectDbRecord -match (
                    '(?<![A-Za-z0-9_])' +
                    [regex]::Escape($removedTcpObjectMember) +
                    '(?![A-Za-z0-9_])')) {
                $staleTcpObjectMembers += $removedTcpObjectMember
            }
        }
        if ($staleTcpObjectMembers.Count -ne 0) {
            throw (
                'Phase5TransportClean Networks.lcb TCPMotionInterface1 ' +
                'object registration is stale: ' +
                ($staleTcpObjectMembers -join ', ') + '.')
        }

        $connectionSearchBlock = $commNetworkDbRecord.Substring(
            $tcpObjectRecordEnd)
        $connectionTableAnchor = [regex]::Match(
            $connectionSearchBlock,
            ('(?s)(?<![0-9])000000(?![0-9]).{0,16}' +
             '(?<![A-Za-z0-9_])TCPMotionInterface1' +
             '(?![A-Za-z0-9_])'))
        if (-not $connectionTableAnchor.Success) {
            throw (
                'Phase5TransportClean Networks.lcb Comm_Network connection ' +
                'table could not be isolated from the TCPMotionInterface1 source tuple.')
        }
        $connectionTableBlock = $connectionSearchBlock.Substring(
            $connectionTableAnchor.Index)
        $connectionRecords = @([regex]::Matches(
            $connectionTableBlock,
            ('(?s)(?<![0-9])(?<Id>[0-9]{6})(?![0-9])' +
             '(?<Body>.*?)(?=(?<![0-9])[0-9]{6}(?![0-9])|\z)')))
        $tcpSourceConnectionRecords = @(
            $connectionRecords | Where-Object {
                $_.Groups['Body'].Value -match (
                    '(?s)^.{0,16}(?<![A-Za-z0-9_])' +
                    'TCPMotionInterface1(?![A-Za-z0-9_])')
            })
        $staleTcpDirectConnectionRecords = @(
            $tcpSourceConnectionRecords | Where-Object {
                $_.Groups['Body'].Value -match
                    '(?<![A-Za-z0-9_])_LMC(?:Axis[1-9]|RobotBase)1?(?![A-Za-z0-9_])'
            })
        if ($staleTcpDirectConnectionRecords.Count -ne 0) {
            $staleTcpDirectConnectionIds = @(
                $staleTcpDirectConnectionRecords | ForEach-Object {
                    $_.Groups['Id'].Value
                }) -join ', '
            throw (
                'Phase5TransportClean Networks.lcb still registers direct ' +
                'TCPMotionInterface1 axis/robot connection tuple IDs: ' +
                $staleTcpDirectConnectionIds + '.')
        }
        if ($tcpSourceConnectionRecords.Count -ne 3) {
            throw (
                'Phase5TransportClean Networks.lcb TCPMotionInterface1 source ' +
                "tuple count is $($tcpSourceConnectionRecords.Count), expected three.")
        }
        foreach ($expectedTcpTuple in @(
                @('LMCDiagnosticsService1', 'Diagnostics', 'ClassSvr'),
                @('TCPIPServer1', '_TCPIPServer', 'Control'),
                @('LMCControlCommandService1', 'ControlCommands', 'ClassSvr'))) {
            $tuplePattern = (
                '(?s)(?<![A-Za-z0-9_])TCPMotionInterface1' +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])' + [regex]::Escape($expectedTcpTuple[0]) +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])' + [regex]::Escape($expectedTcpTuple[1]) +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])' + [regex]::Escape($expectedTcpTuple[2]) +
                '(?![A-Za-z0-9_])')
            if (@($tcpSourceConnectionRecords | Where-Object {
                        $_.Groups['Body'].Value -match $tuplePattern
                    }).Count -ne 1) {
                if ($expectedTcpTuple[0] -eq 'TCPIPServer1' -and
                    @($tcpSourceConnectionRecords | Where-Object {
                            $_.Groups['Body'].Value -match
                                '(?<![A-Za-z0-9_])_TCPIPServer1(?![A-Za-z0-9_])'
                        }).Count -eq 1) {
                    throw (
                        'LASAL Networks.lcb is stale for same-peer takeover: ' +
                        'Comm_Network still targets _TCPIPServer1. Open the ' +
                        'master LASAL project, Save/Rebuild Comm_Network, and ' +
                        'rerun the full static contract.')
                }
                throw (
                    'Phase5TransportClean Networks.lcb missing or duplicate ' +
                    'TCPMotionInterface1 tuple to ' + $expectedTcpTuple[0] + '.')
            }
        }

        $serviceSourceConnectionRecords = @(
            $connectionRecords | Where-Object {
                $_.Groups['Body'].Value -match (
                    '(?s)^.{0,16}(?<![A-Za-z0-9_])' +
                    'LMCControlCommandService1(?![A-Za-z0-9_])')
            })
        if ($serviceSourceConnectionRecords.Count -ne 10) {
            throw (
                'Phase5TransportClean Networks.lcb LMCControlCommandService1 ' +
                "axis/robot source tuple count is $($serviceSourceConnectionRecords.Count), expected ten.")
        }
        foreach ($axisNumber in 1..9) {
            $serviceTuplePattern = (
                '(?s)(?<![A-Za-z0-9_])LMCControlCommandService1' +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])_LMCAxis' + $axisNumber +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])LMCAxis' + $axisNumber +
                '(?![A-Za-z0-9_]).*?' +
                '(?<![A-Za-z0-9_])Control(?![A-Za-z0-9_])')
            if (@($serviceSourceConnectionRecords | Where-Object {
                        $_.Groups['Body'].Value -match $serviceTuplePattern
                    }).Count -ne 1) {
                throw (
                    'Phase5TransportClean Networks.lcb missing or duplicate ' +
                    "LMCControlCommandService1.LMCAxis$axisNumber tuple.")
            }
        }
        $serviceRobotTuplePattern = (
            '(?s)(?<![A-Za-z0-9_])LMCControlCommandService1' +
            '(?![A-Za-z0-9_]).*?' +
            '(?<![A-Za-z0-9_])_LMCRobotBase1(?![A-Za-z0-9_]).*?' +
            '(?<![A-Za-z0-9_])LMCRobot(?![A-Za-z0-9_]).*?' +
            '(?<![A-Za-z0-9_])Control(?![A-Za-z0-9_])')
        if (@($serviceSourceConnectionRecords | Where-Object {
                    $_.Groups['Body'].Value -match $serviceRobotTuplePattern
                }).Count -ne 1) {
            throw (
                'Phase5TransportClean Networks.lcb missing or duplicate ' +
                'LMCControlCommandService1.LMCRobot tuple.')
        }
    }
}

if (-not $SourceOnly) {
    $interfaceObject = $commNetworkXml.SelectSingleNode("//Object[@Name='TCPMotionInterface1']")
    $serverObject = $commNetworkXml.SelectSingleNode(
        "/Network/Components/Object[@Name='TCPIPServer1' and @Class='TCPIPServer']")
    if ($null -eq $interfaceObject -or $null -eq $serverObject) {
        throw 'TCPMotionInterface1 or TCPIPServer1 network object is missing.'
    }
    if ($interfaceObject.HasAttribute('RealTime')) {
        throw 'TCPMotionInterface1 must not have a RealTime task assignment.'
    }
    if ($interfaceObject.CyclicTime -ne '1 ms') {
        throw 'TCPMotionInterface1.CyclicTime must be 1 ms.'
    }
    $configClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='Config']")
    $connectionsPerRunClient = $serverObject.SelectSingleNode(
        "./Channels/Client[@Name='ConnectionsPerRun']")
    $maxConnectionsClient = $serverObject.SelectSingleNode("./Channels/Client[@Name='MaxConnections']")
    if ($null -eq $configClient -or $configClient.Value -ne '0') {
        throw 'TCPIPServer1.Config must be explicitly set to 0.'
    }
    if ($null -eq $connectionsPerRunClient -or
        $connectionsPerRunClient.Value -ne '1') {
        throw 'TCPIPServer1.ConnectionsPerRun must be explicitly set to 1.'
    }
    if ($null -eq $maxConnectionsClient -or $maxConnectionsClient.Value -ne '2') {
        throw 'TCPIPServer1.MaxConnections must be explicitly set to 2.'
    }
    Assert-Match $commNetwork 'TCPMotionInterface1\._TCPIPServer.*TCPIPServer1\.Control' 'TCPMotionInterface1 is not connected to the controlled-shutdown TCP server in Comm_Network.'

    $commControlServiceObjects = @(
        $commNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' and " +
            "@Class='LMCControlCommandService']"))
    $allControlServiceObjects = @(
        $commNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']")
        $motionNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']")
        $etherCatNetworkXml.SelectNodes(
            "/Network/Components/Object[@Name='LMCControlCommandService1' or " +
            "@Class='LMCControlCommandService']"))
    if ($commControlServiceObjects.Count -ne 1 -or
        $allControlServiceObjects.Count -ne 1) {
        throw ('LMCControlCommandService1 must exist exactly once as ' +
            'LMCControlCommandService in Comm_Network and nowhere else.')
    }
    $controlServiceObject = $commControlServiceObjects[0]
    foreach ($taskAttribute in @('RealTime', 'CyclicTime', 'BackgroundTime')) {
        if ($controlServiceObject.HasAttribute($taskAttribute)) {
            throw ("LMCControlCommandService1 must not own a scheduled task; " +
                "$taskAttribute is present.")
        }
    }

    $expectedControlServiceConnections = @(
        @{ Source = 'TCPMotionInterface1.ControlCommands'; Destination = 'LMCControlCommandService1.ClassSvr' })
    foreach ($axisNumber in 1..9) {
        $expectedControlServiceConnections += @{
            Source = "LMCControlCommandService1.LMCAxis$axisNumber"
            Destination = "_LMCAxis$axisNumber.Control"
        }
    }
    $expectedControlServiceConnections += @{
        Source = 'LMCControlCommandService1.LMCRobot'
        Destination = '_LMCRobotBase1.Control'
    }
    foreach ($expectedConnection in $expectedControlServiceConnections) {
        $source = $expectedConnection.Source
        $destination = $expectedConnection.Destination
        $connections = @(
            $commNetworkXml.SelectNodes(
                "/Network/Connections/Connection[@Source='$source' and " +
                "@Destination='$destination']"))
        if ($connections.Count -ne 1) {
            throw "Missing or duplicate $source -> $destination connection in Comm_Network."
        }
    }
    $controlServiceOutgoingConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[starts-with(@Source," +
            "'LMCControlCommandService1.') ]"))
    if ($controlServiceOutgoingConnections.Count -ne 10) {
        throw ("LMCControlCommandService1 outgoing connection count is " +
            "$($controlServiceOutgoingConnections.Count), expected exactly ten.")
    }
    $controlServiceServerConnections = @(
        $commNetworkXml.SelectNodes(
            "/Network/Connections/Connection[" +
            "@Destination='LMCControlCommandService1.ClassSvr']"))
    if ($controlServiceServerConnections.Count -ne 1) {
        throw ("LMCControlCommandService1.ClassSvr connection count is " +
            "$($controlServiceServerConnections.Count), expected exactly one.")
    }

    Assert-Match $commNetworkTable '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCControlCommandService",.*$' 'Comm_Network generated table is stale: LMCControlCommandService class metadata is missing.'
    Assert-Match $commNetworkTable '(?m)^\s*_NO_ATTR,\s*TO_UDINT\(\d+\),\s*"LMCCONTROLCOMMANDSERVICE1",\s*$' 'Comm_Network generated table is stale: LMCControlCommandService1 object metadata is missing.'
    Assert-Match $commNetworkTable '(?m)^\s*TO_UDINT\(\d+\),\s*"ControlCommands",\s*TO_UDINT\(\d+\),\s*"ClassSvr",\s*$' 'Comm_Network generated table is stale: ControlCommands internal connection is missing.'
    $expectedGeneratedMotionConnectionCount = if ($transportClean) { 1 } else { 2 }
    foreach ($axisNumber in 1..9) {
        $generatedAxisConnectionPattern = (
            '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCAxis' + $axisNumber +
            '",\s*C_DIR,\s*TO_UDINT\(\d+\),\s*"_LMCAxis' + $axisNumber +
            '",\s*"Control",\s*$')
        $generatedAxisConnectionCount = [regex]::Matches(
            $commNetworkTable,
            $generatedAxisConnectionPattern).Count
        if ($generatedAxisConnectionCount -ne $expectedGeneratedMotionConnectionCount) {
            throw ("Comm_Network generated LMCAxis$axisNumber connection count is " +
                "$generatedAxisConnectionCount, expected " +
                "$expectedGeneratedMotionConnectionCount retained service/TCP link(s).")
        }
    }
    $generatedRobotConnectionCount = [regex]::Matches(
        $commNetworkTable,
        '(?m)^\s*TO_UDINT\(\d+\),\s*"LMCRobot",\s*C_DIR,\s*TO_UDINT\(\d+\),\s*"_LMCRobotBase1",\s*"Control",\s*$').Count
    if ($generatedRobotConnectionCount -ne $expectedGeneratedMotionConnectionCount) {
        throw ("Comm_Network generated LMCRobot connection count is " +
            "$generatedRobotConnectionCount, expected " +
            "$expectedGeneratedMotionConnectionCount retained service/TCP link(s).")
    }
    if ($transportClean) {
        $phase5ExternalConnectionBlock = [regex]::Match(
            $commNetworkTable,
            '(?s)//External connections\s*' +
            '0\$UDINT,\s*16\$UDINT,\s*' +
            '(?<Body>.*?)//Magic internal connections').Value
        if ([string]::IsNullOrWhiteSpace($phase5ExternalConnectionBlock)) {
            throw 'Phase5TransportClean Comm_Network generated external connection count is not exactly 16.'
        }
        if ($phase5ExternalConnectionBlock -match
            'TO_UDINT\(10\),\s*"(?:LMCAxis[1-9]|LMCRobot)"') {
            throw 'Phase5TransportClean generated table still contains a TCPMotionInterface direct axis/robot connection.'
        }
        $controlServiceOwnerMatch = [regex]::Match(
            $commNetworkTable,
            '(?m)^\s*TO_UDINT\(\d+\),\s*"ControlCommands",\s*' +
            'TO_UDINT\((?<Owner>\d+)\),\s*"ClassSvr",\s*$')
        if (-not $controlServiceOwnerMatch.Success) {
            throw 'Phase5TransportClean generated table does not identify the control-service connection owner.'
        }
        $controlServiceOwner = [regex]::Escape(
            $controlServiceOwnerMatch.Groups['Owner'].Value)
        if ([regex]::Matches(
                $phase5ExternalConnectionBlock,
                'TO_UDINT\(' + $controlServiceOwner +
                '\),\s*"(?:LMCAxis[1-9]|LMCRobot)"').Count -ne 10) {
            throw 'Phase5TransportClean generated table does not retain exactly ten control-service axis/robot connections.'
        }
    }
    $generatedTaskBlock = [regex]::Match(
        $commNetworkTable,
        '(?s)//Configuration of tasks \(RealTime, Cyclic, Background\).*?(?=//External connections)').Value
    if ([string]::IsNullOrWhiteSpace($generatedTaskBlock)) {
        throw 'Comm_Network generated task configuration block was not found.'
    }
    if ($generatedTaskBlock -match 'LMCCONTROLCOMMANDSERVICE1') {
        throw 'Comm_Network generated table assigns a task to LMCControlCommandService1.'
    }

    $diagnosticsServiceObject = $commNetworkXml.SelectSingleNode(
        "/Network/Components/Object[@Name='LMCDiagnosticsService1' and " +
        "@Class='LMCDiagnosticsService']")
    Assert-Match $classDbText 'DiagnosticsBootCounter' 'Classes.lcb metadata is missing DiagnosticsBootCounter. Reload and save LMCDiagnosticsService through LASAL IDE.'
    Assert-Match $classDbText 'GetDiagnosticsBootId' 'Classes.lcb metadata is missing GetDiagnosticsBootId. Reload and save LMCDiagnosticsService through LASAL IDE.'
    $diagnosticsBootCounterServer = $diagnosticsServiceObject.SelectSingleNode(
        "./Channels/Server[@Name='DiagnosticsBootCounter']")
    if ($null -eq $diagnosticsBootCounterServer -or
        $diagnosticsBootCounterServer.Value -ne '0') {
        throw 'LMCDiagnosticsService1.DiagnosticsBootCounter network initialization is missing.'
    }
    Assert-Match $commNetworkTable '"DiagnosticsBootCounter",\s*TO_UDINT\(0\),//\|Comm_Network\.LMCDiagnosticsService1\.DiagnosticsBootCounter;' 'LMCDiagnosticsService1 generated DiagnosticsBootCounter initialization is stale in Comm_Network.'
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
        'TryStartRead',
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
    Assert-Match $classDbText 'TryStartRead(?!4)' 'Classes.lcb still lacks the exact TryStartRead method name. Update the LMCSdoExecutor declaration and save it through LASAL IDE.'
    if ($classDbText -match 'TryStartRead4') {
        throw 'Classes.lcb still contains the stale TryStartRead4 declaration. Replace it with TryStartRead and save through LASAL IDE.'
    }
    if (-not $AllowStaleLasalBinaryMetadata) {
        foreach ($sdoWriteClassDbEntry in @(
                'TryStartWrite',
                'ActiveIsWrite',
                'WriteBuffer',
                'SdoWriteData',
                'GetSdoWritePolicyDetail')) {
            Assert-Match $classDbText (
                [regex]::Escape($sdoWriteClassDbEntry)) (
                'Classes.lcb metadata is missing SDO Write member ' +
                "$sdoWriteClassDbEntry. Reload and save LMCSdoExecutor and " +
                'LMCDiagnosticsService through LASAL IDE, then rebuild.')
        }
    }
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

        $posControllerName = "PosController$axisNumber"
        $posControllerObjects = @(
            $motionNetworkXml.SelectNodes(
                "/Network/Components/Object[@Name='$posControllerName' and " +
                "@Class='PosController']"))
        if ($posControllerObjects.Count -ne 1) {
            throw "$posControllerName must exist exactly once in Motion_Network."
        }
        $posControllerConnections = @(
            $motionNetworkXml.SelectNodes(
                "/Network/Connections/Connection[" +
                "@Source='_LMCAxis$axisNumber.LMCController' and " +
                "@Destination='$posControllerName.Signal_Input']"))
        if ($posControllerConnections.Count -ne 1) {
            throw ("_LMCAxis$axisNumber.LMCController must have exactly one " +
                "connection to $posControllerName.Signal_Input.")
        }
        Assert-Match $motionNetworkTable (
            '"POSCONTROLLER' + $axisNumber + '"') (
            "$posControllerName generated object metadata is missing.")
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
if (-not $transportClean) {
    Assert-Match $st 'TO_UDINT\(1663666918\),\s*"LMCAxis1".*TO_UDINT\(1422175863\),\s*"_LMCAxis"' 'LMCAxis1 client-name/type hashes are incorrect.'
}
Assert-Match $st 'RealtimeTask\s*=\s*"false"' 'TCPMotionInterface still enables a RealTime task.'
Assert-Match $st 'CyclicTask\s*=\s*"true"' 'TCPMotionInterface Cyclic task is disabled.'
Assert-Match $st 'DefCyclictime\s*=\s*"1 ms"' 'TCPMotionInterface default cyclic time is not 1 ms.'
Assert-Match $st 'PayloadData\s*:\s*ARRAY \[0\.\.1319\] OF BYTE' 'LASAL queue does not hold the 1320-byte kinematic payload.'
Assert-Match $st 'ReceiveBuf\s*:\s*ARRAY \[0\.\.2047\] OF BYTE' 'LASAL receive accumulator does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'RequestBuf\s*:\s*ARRAY \[0\.\.1327\] OF BYTE' 'LASAL active request buffer does not hold a 1328-byte kinematic frame.'
Assert-Match $st 'if usPayloadLength > 1320 then' 'LASAL queue payload bound is not 1320 bytes.'
Assert-Match $st 'IngressDiscardRemaining\s*:=\s*udFrameSize - ReceiveFill' 'Oversize frame bounded discard is missing.'
if (-not $transportClean) {
    Assert-Match $st 'GroupMoveRetCode\s*:=\s*_LMCPROF_MOVECMD_ERROR' 'Group move false-success guard is missing.'
}
$classDeclarationBlock = [regex]::Match(
    $st,
    '(?s)TCPMotionInterface\s*:\s*CLASS.*?END_CLASS;').Value
if (-not $transportClean) {
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
}
$removedTcpMethodNames = @(
    'PowerOn',
    'PowerOff',
    'MoveAbs',
    'MoveStop',
    'AxisReset',
    'MoveLinearAbsEx',
    'GroupReadStatus',
    'ClampLRealToDint',
    'HandleAdminCommands',
    'HandleDiagnosticsCommands',
    'HandleRegistryCommands',
    'HandleAxisCommands',
    'HandleGroupCommands',
    'RobotPowerOn',
    'RobotPowerOff',
    'RobotLock',
    'RobotUnLock')
$removedTcpClassDbMethodNames = @(
    'MoveAbs',
    'MoveStop',
    'AxisReset',
    'MoveLinearAbsEx',
    'GroupReadStatus',
    'ClampLRealToDint',
    'HandleAdminCommands',
    'HandleDiagnosticsCommands',
    'HandleRegistryCommands',
    'HandleAxisCommands',
    'HandleGroupCommands',
    'RobotPowerOn',
    'RobotPowerOff',
    'RobotLock',
    'RobotUnLock')
$localFamilyHandlerNames = if ($transportClean) {
    @()
}
else {
    @(
        'HandleAdminCommands',
        'HandleDiagnosticsCommands',
        'HandleRegistryCommands',
        'HandleAxisCommands',
        'HandleGroupCommands')
}
foreach ($handlerName in $localFamilyHandlerNames) {
    Assert-Match $classDeclarationBlock (
        'FUNCTION\s+' + [regex]::Escape($handlerName) + '\s*;') (
        "TCPMotionInterface.$handlerName declaration is missing.")
    Assert-Match $classDbText ([regex]::Escape($handlerName)) (
        "Classes.lcb metadata is missing $handlerName. Save the method through LASAL IDE.")
}
if ($transportClean) {
    Assert-ExactRegexValueSet `
        -Text $classDeclarationBlock `
        -Pattern '(?m)^\s*(?<Value>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*SvrCh_[A-Za-z0-9_]+' `
        -Owner 'Phase5TransportClean TCPMotionInterface declared servers' `
        -ExpectedValues @('CurrentSock', 'CommandID', 'AxisRef', 'Payload')
    Assert-ExactRegexValueSet `
        -Text $classDeclarationBlock `
        -Pattern '(?m)^\s*(?<Value>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*CltChCmd_[A-Za-z0-9_]+' `
        -Owner 'Phase5TransportClean TCPMotionInterface declared clients' `
        -ExpectedValues @('_StdLib', 'Diagnostics', 'ControlCommands')
    Assert-ExactRegexValueSet `
        -Text $classDeclarationBlock `
        -Pattern $phase5DeclaredFunctionPattern `
        -Owner 'Phase5TransportClean TCPMotionInterface declared functions' `
        -ExpectedValues @(
            '@CT_', '@STD', 'CyWork', 'ConnSocketInfo', 'DataHandling',
            'SendData', 'Response', 'MsgPaser')
    Assert-ExactRegexValueSet `
        -Text $st `
        -Pattern $phase5ImplementedFunctionPattern `
        -Owner 'Phase5TransportClean TCPMotionInterface implemented functions' `
        -ExpectedValues @(
            '@CT_', '@STD', 'CyWork', 'ConnSocketInfo', 'DataHandling',
            'SendData', 'Response', 'MsgPaser')
    $phase5DeclaredFunctionCount = [regex]::Matches(
        $classDeclarationBlock,
        $phase5DeclaredFunctionPattern).Count
    $phase5ImplementedFunctionCount = [regex]::Matches(
        $st,
        $phase5ImplementedFunctionPattern).Count
    if ($phase5DeclaredFunctionCount -ne 8 -or
        $phase5ImplementedFunctionCount -ne 8) {
        throw (
            'Phase5TransportClean TCPMotionInterface function count is ' +
            "declared=$phase5DeclaredFunctionCount implemented=$phase5ImplementedFunctionCount, " +
            'expected exactly 8/8 transport functions.')
    }
    Assert-ExactRegexValueSet `
        -Text $st `
        -Pattern '(?m)^\s*#pragma\s+usingLtd\s+(?<Value>[A-Za-z_][A-Za-z0-9_]*)\s*$' `
        -Owner 'Phase5TransportClean TCPMotionInterface limited dependencies' `
        -ExpectedValues @('_StdLib', 'LMCControlCommandService', 'LMCDiagnosticsService')

    foreach ($removedMethodName in $removedTcpMethodNames) {
        if ($classDeclarationBlock -match (
                '(?m)^\s*FUNCTION[^\r\n]*\b' +
                [regex]::Escape($removedMethodName) + '(?:::|\b)') -or
            $st -match (
                '(?m)^\s*FUNCTION[^\r\n]*TCPMotionInterface::' +
                [regex]::Escape($removedMethodName) + '(?:::|\b)') -or
            (-not $AllowStaleLasalBinaryMetadata -and
             $removedTcpClassDbMethodNames -contains $removedMethodName -and
             $tcpClassDbRecord -match (
                 '(?<![A-Za-z0-9_])' +
                 [regex]::Escape($removedMethodName) +
                 '(?![A-Za-z0-9_])'))) {
            throw "Phase5TransportClean removed TCPMotionInterface method $removedMethodName is still declared, implemented, or registered."
        }
    }
    foreach ($removedChannelName in @(
            'bufMode', 'ReadPos', 'RobotPowerOn', 'RobotPowerOff',
            'RobotLock', 'RobotUnLock',
            'LMCAxis1', 'LMCAxis2', 'LMCAxis3', 'LMCAxis4', 'LMCAxis5',
            'LMCAxis6', 'LMCAxis7', 'LMCAxis8', 'LMCAxis9', 'LMCRobot')) {
        if (-not $AllowStaleLasalBinaryMetadata -and
            $tcpClassDbRecord -match (
                '(?<![A-Za-z0-9_])' +
                [regex]::Escape($removedChannelName) +
                '(?![A-Za-z0-9_])')) {
            throw "Phase5TransportClean removed TCPMotionInterface channel $removedChannelName is still registered in Classes.lcb."
        }
    }
    foreach ($removedDomainStatePattern in @(
            '(?<![A-Za-z0-9_])LMCAxis[1-9](?![A-Za-z0-9_])',
            '(?<![A-Za-z0-9_])LMCRobot(?![A-Za-z0-9_])',
            '(?<![A-Za-z0-9_])Group(?:Move|Transition|Velocity|Accel|Decel|Jerk|Coord|Superimposed|Execute|Command|Stop|Read|Kinematic|ObjectName)[A-Za-z0-9_]*',
            '(?<![A-Za-z0-9_])Axis(?:ObjectName|Command|ClientConnected|StatusValue|ErrorValue)[A-Za-z0-9_]*',
            '(?<![A-Za-z0-9_])ObjectRegistryReady(?![A-Za-z0-9_])',
            '(?<![A-Za-z0-9_])PayloadReference(?![A-Za-z0-9_])')) {
        if ($classDeclarationBlock -match $removedDomainStatePattern) {
            throw "Phase5TransportClean TCPMotionInterface still declares domain state '$($Matches[0])'."
        }
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
$localFamilyHandlerBlocks = [ordered]@{}
foreach ($handlerName in $localFamilyHandlerNames) {
    $handlerBlock = [regex]::Match(
        $st,
        ('(?s)FUNCTION TCPMotionInterface::' +
         [regex]::Escape($handlerName) +
         '.*?END_FUNCTION')).Value
    if ([string]::IsNullOrWhiteSpace($handlerBlock)) {
        throw "TCPMotionInterface.$handlerName implementation was not found."
    }
    $handlerByteCount = [Text.Encoding]::UTF8.GetByteCount($handlerBlock)
    if ($handlerByteCount -gt 32768) {
        throw "TCPMotionInterface.$handlerName is $handlerByteCount bytes, expected at most 32768."
    }
    $localFamilyHandlerBlocks[$handlerName] = $handlerBlock
}
$adminHandlerBlock = $localFamilyHandlerBlocks['HandleAdminCommands']
$diagnosticsHandlerBlock = $localFamilyHandlerBlocks['HandleDiagnosticsCommands']
$registryHandlerBlock = $localFamilyHandlerBlocks['HandleRegistryCommands']
$axisHandlerBlock = $localFamilyHandlerBlocks['HandleAxisCommands']
$groupHandlerBlock = $localFamilyHandlerBlocks['HandleGroupCommands']
if (-not $transportClean) {
if ($controlServiceGroupRouted) {
    foreach ($adminLocal in @(
            @{ Name = 'adminSchemaVersion'; Type = 'UINT' },
            @{ Name = 'adminRequestFlags'; Type = 'UINT' },
            @{ Name = 'adminRequestId'; Type = 'UDINT' },
            @{ Name = 'adminParameterKey'; Type = 'UINT' },
            @{ Name = 'adminDetailCode'; Type = 'UDINT' },
            @{ Name = 'adminAxisValue'; Type = 'DINT' },
            @{ Name = 'adminUnitCode'; Type = 'UINT' },
            @{ Name = 'adminAxisReadKind'; Type = 'UINT' },
            @{ Name = 'adminAxisParameter'; Type = '_LMCAXIS_READPARAMETER' },
            @{ Name = 'adminSwEndMode'; Type = '_LMCAXIS_READSWENDPOS' },
            @{ Name = 'adminAxisClientConnected'; Type = 'BOOL' },
            @{ Name = 'adminErrorId'; Type = 'INT' })) {
        Assert-Match $adminHandlerBlock (
            '(?m)^\s*' + [regex]::Escape($adminLocal.Name) +
            '\s*:\s*' + [regex]::Escape($adminLocal.Type) + '\s*;\s*$') (
            "HandleAdminCommands remaining local $($adminLocal.Name) is missing.")
    }
}
else {
    Assert-Match $adminHandlerBlock '(?s)VAR\s+kinIndex\s*:\s*DINT;.*?adminErrorId\s*:\s*INT;\s*END_VAR' 'HandleAdminCommands local declaration contract is incomplete.'
}
if ($diagnosticsServiceRouted) {
    Assert-Match $diagnosticsHandlerBlock (
        '(?s)\AFUNCTION\s+TCPMotionInterface::HandleDiagnosticsCommands\s*' +
        'VAR\s+diagnosticsResponseSize\s*:\s*DINT;\s*END_VAR') (
        'Phase4DiagnosticsRouted HandleDiagnosticsCommands must own only ' +
        'the diagnosticsResponseSize local scratch value.')
    $diagnosticsHandlerVarBlock = [regex]::Match(
        $diagnosticsHandlerBlock,
        '(?s)\AFUNCTION\s+TCPMotionInterface::HandleDiagnosticsCommands\s*' +
        'VAR\s*(?<Body>.*?)\s*END_VAR').Groups['Body'].Value
    $diagnosticsHandlerLocalCount = [regex]::Matches(
        $diagnosticsHandlerVarBlock,
        '(?m)^\s*[A-Za-z_][A-Za-z0-9_]*\s*:\s*[^;]+;\s*$').Count
    if ($diagnosticsHandlerLocalCount -ne 1) {
        throw (
            'Phase4DiagnosticsRouted HandleDiagnosticsCommands local count is ' +
            "$diagnosticsHandlerLocalCount, expected exactly one.")
    }
}
else {
    Assert-Match $diagnosticsHandlerBlock '(?s)VAR\s+diagnosticsSchemaVersion\s*:\s*UINT;.*?diagnosticsBootId\s*:\s*UDINT;\s*END_VAR' 'HandleDiagnosticsCommands local declaration contract is incomplete.'
}
Assert-Match $registryHandlerBlock '(?s)VAR\s+objectNameLength\s*:\s*UDINT;\s*END_VAR' 'HandleRegistryCommands local declaration contract is incomplete.'
if (-not $controlServiceGroupRouted) {
    Assert-Match $groupHandlerBlock '(?s)VAR\s+objectNameLength\s*:\s*UDINT;\s*kinIndex\s*:\s*DINT;\s*kinValid\s*:\s*BOOL;\s*powerIsOn\s*:\s*DINT;\s*profileLockState\s*:\s*DINT;\s*END_VAR' 'HandleGroupCommands local declaration contract is incomplete or reordered.'
}
}
$msgParserByteCount = [Text.Encoding]::UTF8.GetByteCount($msgParserBlock)
if ($msgParserByteCount -gt 32768) {
    throw "TCPMotionInterface.MsgPaser is $msgParserByteCount bytes, expected at most 32768."
}
$caseIndex = $msgParserBlock.IndexOf('case CommandID of')
if ($caseIndex -lt 0) {
    throw 'TCPMotionInterface.MsgPaser command case was not found.'
}
$preCaseBlock = $msgParserBlock.Substring(0, $caseIndex)
foreach ($commandId in @('2023', '2024', '2022', '2028', '202E', '209F', '20A0', '20A2', '20D2', '2047', '2048', '2049', '204A', '204B', '2045', '2051', '2085', '20A4', '20E7', '7D00', '7D10', '7D20', '7D22')) {
    if ($preCaseBlock -match "CommandID = 0x$commandId") {
        throw "Active command 0x$commandId is blocked before its CyWork handler."
    }
}

$topologyIoStaticRouteFixture = @'
FUNCTION TCPMotionInterface::MsgPaser
case CommandID of
0x7E11, 0x7E12:
    Diagnostics.HandleRequest();
end_case;
END_FUNCTION
'@
$topologyIoReadRouteFixture = $topologyIoStaticRouteFixture.Replace(
    'end_case;',
    "0x7E13, 0x7E22:`n    Diagnostics.HandleRequest();`nend_case;")
$topologyIoOutputRouteFixture = $topologyIoReadRouteFixture.Replace(
    'end_case;',
    "0x7E23:`n    Diagnostics.HandleRequest();`nend_case;")
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoStaticRouteFixture `
    -Owner 'Topology/I/O static route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12')
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoReadRouteFixture `
    -Owner 'Topology/I/O read route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22')
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoOutputRouteFixture `
    -Owner 'Topology/I/O output route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoMixedLabelRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        ('0x7E13, 16#7E22, 32291: ' +
         "Diagnostics.HandleRequest();`nend_case;"))
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoMixedLabelRouteFixture `
    -Owner 'Topology/I/O mixed multi-label route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoMultilineLabelRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        ("0x7E13,`n16#7E22,`n32291:`n" +
         "    Diagnostics.HandleRequest();`nend_case;"))
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoMultilineLabelRouteFixture `
    -Owner 'Topology/I/O multiline label route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoDecimalMultiLabelRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        ("32275, 32290, 32291: Diagnostics.HandleRequest();`n" +
         'end_case;'))
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoDecimalMultiLabelRouteFixture `
    -Owner 'Topology/I/O decimal multi-label route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoIecBaseLiteralRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        ('2#0111111000010011, 8#77042, UINT#16#7E23: ' +
         "Diagnostics.HandleRequest();`nend_case;"))
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoIecBaseLiteralRouteFixture `
    -Owner 'Topology/I/O IEC base-literal route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoTypedDecimalRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        ('UINT#32275, UDINT#32_290, LWORD#32_291: ' +
         "Diagnostics.HandleRequest();`nend_case;"))
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoTypedDecimalRouteFixture `
    -Owner 'Topology/I/O typed decimal route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13', '7E22', '7E23')
$topologyIoOctalLiteralRouteFixture =
    $topologyIoStaticRouteFixture.Replace(
        'end_case;',
        "8#77023: Diagnostics.HandleRequest();`nend_case;")
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $topologyIoOctalLiteralRouteFixture `
    -Owner 'Topology/I/O octal literal route canonical fixture' `
    -ExpectedCommandIds @('7E11', '7E12', '7E13')
$topologyIoInlineNestedCaseFixture = @'
FUNCTION TCPMotionInterface::MsgPaser
case CommandID of
0x7E11: case nestedSelector of
1:
    NestedHandler();
end_case;
0x7E13: OtherHandler();
end_case;
END_FUNCTION
'@
$topologyIoInlineNestedCaseCrLfFixture = [regex]::Replace(
    $topologyIoInlineNestedCaseFixture,
    '\r?\n',
    "`r`n")
if ([regex]::IsMatch(
        $topologyIoInlineNestedCaseCrLfFixture,
        '(?<!\r)\n')) {
    throw 'Topology/I/O inline nested-CASE CRLF fixture contains bare LF.'
}
$topologyIoSameLineNestedCloseFixture = @'
FUNCTION TCPMotionInterface::MsgPaser
case CommandID of
0x7E11: case nestedSelector of
1:
    NestedHandler();
end_case; 0x7E13: OtherHandler();
end_case;
END_FUNCTION
'@
$topologyIoSameLineNestedCloseCrLfFixture = [regex]::Replace(
    $topologyIoSameLineNestedCloseFixture,
    '\r?\n',
    "`r`n")
if ([regex]::IsMatch(
        $topologyIoSameLineNestedCloseCrLfFixture,
        '(?<!\r)\n')) {
    throw (
        'Topology/I/O same-line nested END_CASE CRLF fixture contains bare LF.')
}
$topologyIoRouteNegativeFixtures = [ordered]@{
    'SeparateHandler' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "0x7E13:`n    OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'DuplicateLiveCase' = @{
        Text = $topologyIoReadRouteFixture.Replace(
            'end_case;',
            "0x7E13:`n    OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12', '7E13', '7E22')
    }
    'InlineSeparateHandler' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "0x7E13: OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'RadixInlineSeparateHandler' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "16#7E13: OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'DecimalInlineSeparateHandler' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "32275: OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'InlineNestedCaseDepthCrLf' = @{
        Text = $topologyIoInlineNestedCaseCrLfFixture
        Expected = @('7E11')
    }
    'SameLineNestedCloseRouteCrLf' = @{
        Text = $topologyIoSameLineNestedCloseCrLfFixture
        Expected = @('7E11')
    }
    'UnrecognizedIdentifierLabel' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "LMC_CMD_TOPOLOGY_READ: OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'UnrecognizedIdentifierContinuationLabel' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            ("LMC_CMD_TOPOLOGY_READ,`n0x7E12:`n" +
             "    OtherHandler();`nend_case;"))
        Expected = @('7E11', '7E12')
    }
    'UnrecognizedTypedLabel' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            "REAL#32275: OtherHandler();`nend_case;")
        Expected = @('7E11', '7E12')
    }
    'CommentSpoof' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            ("// 0x7E13:`n" +
             "(*`n0x7E22:`n*)`n" +
             'end_case;'))
        Expected = @('7E11', '7E12', '7E13', '7E22')
    }
    'StringSpoof' = @{
        Text = $topologyIoStaticRouteFixture.Replace(
            'end_case;',
            ('probe := "0x7E13: 16#7E22: 32291:";' + "`n" +
             'end_case;'))
        Expected = @('7E11', '7E12', '7E13', '7E22', '7E23')
    }
}
foreach ($negativeFixture in
        $topologyIoRouteNegativeFixtures.GetEnumerator()) {
    $negativeRejected = $false
    try {
        Assert-TopologyIoTopLevelRouteSet `
            -FunctionBlock ([string]$negativeFixture.Value.Text) `
            -Owner ('Topology/I/O route negative fixture ' +
                $negativeFixture.Key) `
            -ExpectedCommandIds ([string[]]$negativeFixture.Value.Expected)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'Topology/I/O route verifier accepted negative fixture ' +
            "'$($negativeFixture.Key)'.")
    }
}
Assert-TopologyIoTopLevelRouteSet `
    -FunctionBlock $msgParserBlock `
    -Owner ("$ControlServiceCheckpoint TCPMotionInterface.MsgPaser " +
        "TopologyIoCheckpoint=$TopologyIoCheckpoint") `
    -ExpectedCommandIds $topologyIoCommandIds

if ($transportClean) {
    Assert-ExactLasalCommandRouteIds `
        -RouterBlock $msgParserBlock `
        -Owner 'Phase5TransportClean TCPMotionInterface diagnostics-service route' `
        -CallPattern 'Diagnostics\s*\.\s*HandleRequest\s*\(' `
        -ExpectedCommandIds $diagnosticsCommandIds
    $phase5DiagnosticsRoutePattern = (
        '(?ms)^(?<Indent>[ \t]*)(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:' +
        '(?<Body>.*?)(?=^\k<Indent>(?:0x[0-9A-Fa-f]{4}|end_case\b))')
    $phase5DiagnosticsRouteMatches = @(
        [regex]::Matches($msgParserBlock, $phase5DiagnosticsRoutePattern) |
            Where-Object {
                $_.Groups['Body'].Value -match
                    'Diagnostics\s*\.\s*HandleRequest\s*\('
            })
    if ($phase5DiagnosticsRouteMatches.Count -ne 1) {
        throw 'Phase5TransportClean diagnostics-service route could not be isolated.'
    }
    $diagnosticsTransportBlock =
        $phase5DiagnosticsRouteMatches[0].Groups['Body'].Value
}
else {
    $diagnosticsAggregatePattern = '(?s)' + (
        @($diagnosticsCommandIds | ForEach-Object { '0x' + $_ }) -join ',\s*') +
        ':\s*HandleDiagnosticsCommands\(\);'
    Assert-Match $msgParserBlock $diagnosticsAggregatePattern (
        'MsgPaser diagnostics-family aggregate route is missing or reordered ' +
        "for TopologyIoCheckpoint=$TopologyIoCheckpoint.")
    if ($diagnosticsServiceRouted) {
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $msgParserBlock `
            -Owner 'Phase4DiagnosticsRouted TCPMotionInterface diagnostics-service route' `
            -CallPattern 'HandleDiagnosticsCommands\s*\(' `
            -ExpectedCommandIds $diagnosticsCommandIds
    }
    $diagnosticsTransportBlock = $diagnosticsHandlerBlock
}
if (-not $controlServiceAllControlRouted) {
    Assert-Match $msgParserBlock '(?s)0x103C,\s*0x1042,\s*0x202B:\s*HandleRegistryCommands\(\);' 'MsgPaser registry-family aggregate route is missing or reordered.'
    Assert-Match $msgParserBlock '(?s)0x2023,\s*0x2024,\s*0x2022,\s*0x2028,\s*0x202E,\s*0x209F,\s*0x20A0,\s*0x20A2:\s*HandleAxisCommands\(\);' 'MsgPaser axis-family aggregate route is missing or reordered.'
}
$localHandlerExpectedCallCounts = [ordered]@{
    HandleAdminCommands = 1
    HandleDiagnosticsCommands = 1
    HandleRegistryCommands = 1
    HandleAxisCommands = 1
    HandleGroupCommands = 1
}
if ($controlServiceGroupRouted) {
    $localHandlerExpectedCallCounts['HandleGroupCommands'] = 0
    if ($controlServiceAllControlRouted) {
        $localHandlerExpectedCallCounts['HandleAdminCommands'] = 0
        $localHandlerExpectedCallCounts['HandleRegistryCommands'] = 0
        $localHandlerExpectedCallCounts['HandleAxisCommands'] = 0
        if ($transportClean) {
            $localHandlerExpectedCallCounts['HandleDiagnosticsCommands'] = 0
        }
    }
    else {
        Assert-Match $msgParserBlock (
            '(?s)0x7D00,\s*0x7D10:\s*HandleAdminCommands\(\);') (
            'Phase3GroupRouted MsgPaser remaining Admin route is missing or reordered.')
    }
    Assert-ExactLasalCommandRouteIds `
        -RouterBlock $msgParserBlock `
        -Owner "$ControlServiceCheckpoint TCPMotionInterface control-service route" `
        -CallPattern 'ControlCommands\s*\.\s*HandleRequest\s*\(' `
        -ExpectedCommandIds $transportControlCommandIds

    $controlServiceRoutePattern = (
        '(?ms)^[ \t]*(?<Labels>0x[0-9A-Fa-f]{4}' +
        '(?:[ \t]*,[ \t]*(?:\r?\n[ \t]*)?' +
        '0x[0-9A-Fa-f]{4})*)[ \t]*:' +
        '(?<Body>.*?)(?=^[ \t]*(?:0x[0-9A-Fa-f]{4}|else\b|end_case\b))')
    $controlServiceRouteMatches = @(
        [regex]::Matches($msgParserBlock, $controlServiceRoutePattern) |
            Where-Object {
                $_.Groups['Body'].Value -match
                    'ControlCommands\s*\.\s*HandleRequest\s*\('
            })
    if ($controlServiceRouteMatches.Count -ne 1) {
        throw ("$ControlServiceCheckpoint control-service route could not be " +
            'isolated for transport-contract validation.')
    }
    $controlServiceRouteBlock = $controlServiceRouteMatches[0].Groups['Body'].Value
    $controlServiceCallMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)(?<Result>[A-Za-z_][A-Za-z0-9_]*)\s*:=\s*' +
         'ControlCommands\s*\.\s*HandleRequest\s*\(\s*' +
         'CommandId\s*:=\s*CommandID\$UINT\s*,\s*' +
         'Reference\s*:=\s*AxisRef\$UINT\s*,\s*' +
         'pRequestFrame\s*:=\s*\(?\s*#RequestBuf\[0\]\s*\)?' +
         '(?:\$\^USINT)?\s*,\s*' +
         'RequestFrameSize\s*:=\s*\(?\s*Payload\s*\+\s*8\s*\)?' +
         '(?:\$UDINT)?\s*,\s*' +
         'pResponseFrame\s*:=\s*\(?\s*#Sendbuf\[0\]\s*\)?' +
         '(?:\$\^USINT)?\s*,\s*' +
         'ResponseCapacity\s*:=\s*sizeof\(Sendbuf\)\s*\)'))
    if (-not $controlServiceCallMatch.Success) {
        throw ("$ControlServiceCheckpoint must pass CommandID, AxisRef, the complete " +
            'request frame and size, and the complete response buffer and ' +
            'capacity to ControlCommands.HandleRequest in ABI order.')
    }
    $controlResponseName = $controlServiceCallMatch.Groups['Result'].Value
    $escapedControlResponseName = [regex]::Escape($controlResponseName)
    $controlResponseDeclarations = [regex]::Matches(
        $msgParserBlock,
        ('(?m)^\s*' + $escapedControlResponseName +
         '\s*:\s*DINT\s*;\s*$'))
    $msgParserVarBlock = [regex]::Match(
        $msgParserBlock,
        '(?s)\AFUNCTION\s+TCPMotionInterface::MsgPaser\s*' +
        'VAR\s*(?<Body>.*?)\s*END_VAR').Groups['Body'].Value
    if ($controlResponseDeclarations.Count -ne 1 -or
        [string]::IsNullOrWhiteSpace($msgParserVarBlock) -or
        $msgParserVarBlock -notmatch (
            '(?m)^\s*' + $escapedControlResponseName +
            '\s*:\s*DINT\s*;\s*$')) {
        throw ("$ControlServiceCheckpoint response scratch $controlResponseName " +
            'must be declared exactly once as a MsgPaser-local DINT.')
    }
    $controlResponseInitMatch = [regex]::Match(
        $controlServiceRouteBlock,
        $escapedControlResponseName + '\s*:=\s*-1\s*;')
    $controlClientCallBlockMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)if\s+IsClientConnected\(#ControlCommands\)\s+then.*?' +
         [regex]::Escape($controlServiceCallMatch.Value) +
         '\s*;\s*end_if;'))
    $controlFallbackBlockMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)if\s+\(' + $escapedControlResponseName +
         '\s*<=\s*0\)\s*\|\s*\(' + $escapedControlResponseName +
         '\s*>\s*sizeof\(Sendbuf\)\)\s+then.*?' +
         $escapedControlResponseName + '\s*:=\s*12;\s*end_if;'))
    $controlSharedSendMatch = [regex]::Match(
        $controlServiceRouteBlock,
        ('(?s)SendData\(\s*pData:=#Sendbuf\[0\],\s*' +
         'udSize:=' + $escapedControlResponseName + '\$UDINT,\s*' +
         'dSocket:=CurrentSock,\s*bDirect:=TRUE\s*\);'))
    if (-not $controlResponseInitMatch.Success -or
        -not $controlClientCallBlockMatch.Success -or
        -not $controlFallbackBlockMatch.Success -or
        -not $controlSharedSendMatch.Success -or
        $controlResponseInitMatch.Index -ge $controlClientCallBlockMatch.Index -or
        $controlServiceCallMatch.Index -lt $controlClientCallBlockMatch.Index -or
        ($controlServiceCallMatch.Index + $controlServiceCallMatch.Length) -gt
            ($controlClientCallBlockMatch.Index + $controlClientCallBlockMatch.Length) -or
        ($controlClientCallBlockMatch.Index + $controlClientCallBlockMatch.Length) -gt
            $controlFallbackBlockMatch.Index -or
        ($controlFallbackBlockMatch.Index + $controlFallbackBlockMatch.Length) -gt
            $controlSharedSendMatch.Index) {
        throw ("$ControlServiceCheckpoint order must be result init, connected " +
            'HandleRequest call, invalid-response normalization, then one ' +
            'shared SendData.')
    }
    Assert-Match $controlServiceRouteBlock (
        '(?s)if\s+\(' + $escapedControlResponseName + '\s*<=\s*0\)\s*\|\s*' +
        '\(' + $escapedControlResponseName +
        '\s*>\s*sizeof\(Sendbuf\)\)\s+then\s*' +
        '_memset\(dest:=#Sendbuf,\s*usByte:=0,\s*cntr:=sizeof\(Sendbuf\)\);.*?' +
        'Sendbuf\[0\]\$UINT\s*:=\s*1;.*?' +
        'Sendbuf\[2\]\$UINT\s*:=\s*4;.*?' +
        'Sendbuf\[4\]\$UDINT\s*:=\s*0;.*?' +
        'Sendbuf\[8\]\$UINT\s*:=\s*1;.*?' +
        'Sendbuf\[10\]\$INT\s*:=\s*-1;.*?' +
        $escapedControlResponseName + '\s*:=\s*12;.*?end_if;.*?' +
        'SendData\(\s*pData:=#Sendbuf\[0\],\s*' +
        'udSize:=' + $escapedControlResponseName + '\$UDINT,\s*' +
        'dSocket:=CurrentSock,\s*bDirect:=TRUE\s*\);') (
        "$ControlServiceCheckpoint invalid-response bound, common fail-closed frame, or single-send path is incomplete.")
    $controlRouteSendCount = [regex]::Matches(
        $controlServiceRouteBlock,
        '(?m)^\s*SendData\s*\(').Count
    if ($controlRouteSendCount -ne 1) {
        throw ("$ControlServiceCheckpoint control-service route SendData call count is " +
            "$controlRouteSendCount, expected exactly one shared send.")
    }
}
else {
    Assert-Match $msgParserBlock '(?s)0x20D2,\s*0x2047,\s*0x2048,\s*0x2049,\s*0x204A,\s*0x204B,\s*0x2085,\s*0x20A4,\s*0x2045,\s*0x2051,\s*0x20E7:\s*HandleGroupCommands\(\);' 'MsgPaser group-family aggregate route is missing or reordered.'
    Assert-Match $msgParserBlock '(?s)0x7D00,\s*0x7D10,\s*0x7D20,\s*0x7D22:\s*HandleAdminCommands\(\);' 'MsgPaser admin-family aggregate route is missing or reordered.'
    Assert-ExactLasalCommandCaseIds `
        -FunctionBlock $groupHandlerBlock `
        -Owner 'TCPMotionInterface.HandleGroupCommands' `
        -ExpectedCommandIds $phase3GroupCommandIds
    Assert-ExactLasalCommandCaseIds `
        -FunctionBlock $adminHandlerBlock `
        -Owner 'TCPMotionInterface.HandleAdminCommands' `
        -ExpectedCommandIds @('7D00', '7D10', '7D20', '7D22')
}
foreach ($handlerName in $localHandlerExpectedCallCounts.Keys) {
    $handlerCallCount = [regex]::Matches(
        $st,
        ('(?m)^\s*' + [regex]::Escape($handlerName) + '\(\);\s*$')).Count
    $expectedCallCount = $localHandlerExpectedCallCounts[$handlerName]
    if ($handlerCallCount -ne $expectedCallCount) {
        throw (
            "$ControlServiceCheckpoint $handlerName call count is " +
            "$handlerCallCount, expected $expectedCallCount MsgPaser caller(s).")
    }
}
if ($transportClean) {
    Assert-ExactRegexValueSet `
        -Text $msgParserVarBlock `
        -Pattern '(?m)^\s*(?<Value>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*DINT\s*;\s*$' `
        -Owner 'Phase5TransportClean TCPMotionInterface.MsgPaser local scratch' `
        -ExpectedValues @('controlResponseSize', 'diagnosticsResponseSize')
}

if (-not $controlServiceAllControlRouted) {
    $adminCapabilitiesCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        '(?s)0x7D00:.*?0x7D10:').Value
    $adminAxisParameterCasePattern = if ($controlServiceGroupRouted) {
        '(?s)0x7D10:.*'
    }
    else {
        '(?s)0x7D10:.*?0x7D20:'
    }
    $adminAxisParameterCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        $adminAxisParameterCasePattern).Value
    if ([string]::IsNullOrWhiteSpace($adminCapabilitiesCaseBlock) -or
        [string]::IsNullOrWhiteSpace($adminAxisParameterCaseBlock)) {
        throw 'The local 0x7D00/0x7D10 admin cases were not found.'
    }

    Assert-Match $adminCapabilitiesCaseBlock '(?s)if Payload >= 8 then.*?RequestBuf\[8\]\$UINT.*?RequestBuf\[10\]\$UINT.*?RequestBuf\[12\]\$UDINT' '0x7D00 common request offsets are incomplete.'
    Assert-Match $adminCapabilitiesCaseBlock '(?s)Payload <> 8.*?AxisRef <> 0.*?adminSchemaVersion <> 1.*?adminRequestFlags <> 0.*?adminRequestId = 0' '0x7D00 request validation is incomplete.'
    Assert-Match $adminCapabilitiesCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*40.*?Sendbuf\[24\]\$UDINT\s*:=\s*0x00000007.*?Sendbuf\[28\]\$UDINT\s*:=\s*0x0000003F.*?Sendbuf\[32\]\$UDINT\s*:=\s*0x00000007.*?Sendbuf\[36\]\$UINT\s*:=\s*4.*?Sendbuf\[40\]\$UINT\s*:=\s*0x0100.*?Sendbuf\[42\]\$UINT\s*:=\s*3.*?udSize:=48' '0x7D00 capability bits, masks, limits, or response framing are incomplete.'

    Assert-Match $adminAxisParameterCaseBlock '(?s)Payload <> 12.*?\(AxisRef < 1\) \| \(AxisRef > 4\).*?adminSchemaVersion <> 1.*?RequestBuf\[18\]\$UINT <> 0' '0x7D10 payload/reference/common/reserved validation is incomplete.'
    Assert-Match $adminAxisParameterCaseBlock '(?s)case adminParameterKey of.*?LMCAXIS_RD_SWMIN_APPUNIT.*?LMCAXIS_RD_SWMAX_APPUNIT.*?LMCAXIS_PAR_RD_SWLIMWINDOW.*?LMCAXIS_PAR_RD_V_MAX.*?LMCAXIS_PAR_RD_A_MAX.*?LMCAXIS_PAR_RD_REFPOS.*?adminDetailCode := 6' '0x7D10 semantic-to-native allowlist mapping is incomplete.'
    if ([regex]::Matches($adminAxisParameterCaseBlock, '\bLMCAxis[1-4]\.ReadSWEndPos\s*\(').Count -ne 4 -or
        [regex]::Matches($adminAxisParameterCaseBlock, '\bLMCAxis[1-4]\.ReadParameter\s*\(').Count -ne 4) {
        throw '0x7D10 must expose both safe native read paths for each physical axis exactly once.'
    }
    Assert-Match $adminAxisParameterCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*28.*?Sendbuf\[24\]\$UINT\s*:=\s*adminParameterKey.*?Sendbuf\[26\]\$UINT\s*:=\s*1.*?Sendbuf\[28\]\$UINT\s*:=\s*adminUnitCode.*?Sendbuf\[32\]\$DINT\s*:=\s*adminAxisValue.*?udSize:=36.*?Sendbuf\[2\]\$UINT\s*:=\s*16.*?Sendbuf\[14\]\$INT\s*:=\s*-31000.*?udSize:=24' '0x7D10 success/error response framing is incomplete.'
}

if (-not $controlServiceGroupRouted) {
    $adminGroupParametersCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        '(?s)0x7D20:.*?0x7D22:').Value
    $adminGroupMoveRelativeCaseBlock = [regex]::Match(
        $adminHandlerBlock,
        '(?s)0x7D22:.*').Value
    if ([string]::IsNullOrWhiteSpace($adminGroupParametersCaseBlock) -or
        [string]::IsNullOrWhiteSpace($adminGroupMoveRelativeCaseBlock)) {
        throw 'The legacy local 0x7D20/0x7D22 admin cases were not found.'
    }

    Assert-Match $adminGroupParametersCaseBlock '(?s)Payload <> 12.*?AxisRef <> 0x0100.*?adminSelectionMask = 0.*?adminSelectionMask and 0xFFFFFFF8.*?IsClientConnected\(#LMCRobot\)' '0x7D20 group reference, mask, or client validation is incomplete.'
    foreach ($groupParameter in @('_LMCPROF_GRP_VEL_LIMIT', '_LMCPROF_GRP_ACCEL_LIMIT', '_LMCPROF_GRP_TJERK')) {
        Assert-Match $adminGroupParametersCaseBlock (
            'LMCRobot\.ReadGroupParameter\(\s*GrpNo:=1,\s*ParNo:=' +
            [regex]::Escape($groupParameter) + '\)') "0x7D20 is missing $groupParameter semantic mapping."
    }
    if ([regex]::Matches($adminGroupParametersCaseBlock, '\bLMCRobot\.ReadGroupParameter\s*\(').Count -ne 3) {
        throw '0x7D20 must issue at most the three selected native parameter reads.'
    }
    Assert-Match $adminGroupParametersCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*32.*?Sendbuf\[24\]\$UDINT\s*:=\s*adminSelectionMask.*?Sendbuf\[28\]\$DINT\s*:=\s*adminGroupVelocityLimit.*?Sendbuf\[32\]\$DINT\s*:=\s*adminGroupAccelerationLimit.*?Sendbuf\[36\]\$DINT\s*:=\s*adminGroupJerkTime.*?udSize:=40' '0x7D20 fixed success response framing is incomplete.'

    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)Payload <> 104.*?AxisRef <> 0x0100.*?adminSchemaVersion <> 1.*?adminRequestFlags <> 0.*?adminRequestId = 0' '0x7D22 payload/reference/common request validation is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock 'adminErrorId := -31000' '0x7D22 local validation and state errors do not use the Admin error ID.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)source:=#RequestBuf\[16\].*?source:=#RequestBuf\[80\].*?source:=#RequestBuf\[84\].*?source:=#RequestBuf\[88\].*?source:=#RequestBuf\[92\].*?source:=#RequestBuf\[96\].*?source:=#RequestBuf\[100\].*?source:=#RequestBuf\[104\].*?source:=#RequestBuf\[108\]' '0x7D22 DINT field offsets are incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)\(GroupVelocity > 0\).*?\(GroupAccel > 0\).*?\(GroupDecel > 0\).*?\(GroupJerk >= 0\).*?\(GroupCoordSystem = 0\).*?\(GroupTransitionModeInput = 0\).*?\(GroupTransitionModeInput = 2\).*?\(bufMode = 1\).*?\(bufMode = 2\).*?\(GroupExecute = 1\).*?adminDetailCode := 9' '0x7D22 approved motion-parameter validation is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)for kinIndex := 4 to 15 do.*?RequestBuf\[\(16 \+ \(kinIndex \* 4\)\)\$DINT\]\$DINT <> 0.*?GroupCommandInputValid := FALSE' '0x7D22 does not reject nonzero distances outside the four-axis topology.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)case GroupTransitionModeInput of.*?_LMCPROF_EXACT_STOP.*?_LMCPROF_CONT_DIRECT.*?if bufMode = 1 then.*?GroupCommandConfig := 16' '0x7D22 transition and buffer-mode mapping is incomplete.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?LMCRobot\.ReadProfileParameter\(\s*ParNo:=_LMCPROF_LockState\).*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?profileLockState <> 0.*?LMCRobot\.MoveRelativeCoord\(.*?pDistances:=#GroupMovePos.*?CmdConfig:=GroupCommandConfig.*?Velocity:=GroupVelocity.*?Accel:=GroupAccel.*?Decel:=GroupDecel.*?TransMode:=GroupTransitionMode.*?TransRadius:=GroupTransitionRadius.*?CoordSystem:=0.*?Jerk:=GroupJerk' '0x7D22 does not gate and dispatch the relative move through the configured, powered, locked four-axis profile.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)GroupMoveRetCode = _LMCPROF_NoError.*?adminErrorId := 0.*?adminDetailCode := 11.*?GroupMoveRetCode\$UDINT <= 32767.*?adminErrorId := GroupMoveRetCode\$INT.*?adminErrorId := -6' '0x7D22 does not preserve a representable native rejection code.'
    Assert-Match $adminGroupMoveRelativeCaseBlock '(?s)adminDetailCode := 10.*?_memset\(dest:=#Sendbuf.*?Sendbuf\[2\]\$UINT\s*:=\s*16.*?Sendbuf\[8\]\$UINT\s*:=\s*1.*?Sendbuf\[10\]\$UINT\s*:=\s*0.*?Sendbuf\[12\]\$UINT\s*:=\s*0.*?Sendbuf\[14\]\$INT\s*:=\s*0.*?Sendbuf\[16\]\$UDINT\s*:=\s*adminRequestId.*?Sendbuf\[20\]\$UDINT\s*:=\s*adminDetailCode.*?adminDetailCode <> 0.*?Sendbuf\[12\]\$UINT\s*:=\s*1.*?Sendbuf\[14\]\$INT\s*:=\s*adminErrorId.*?udSize:=24' '0x7D22 state error or Admin response framing is incomplete.'
}

if (-not $diagnosticsServiceRouted) {
$diagnosticsCapabilitiesCaseBlock = [regex]::Match(
    $diagnosticsHandlerBlock,
    '(?s)0x7E00:.*?0x7E01,').Value
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
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)if diagnosticsBootId <> 0 then\s*Sendbuf\[28\]\$UDINT\s*:=\s*0x0000213F;\s*Sendbuf\[38\]\$UINT\s*:=\s*24;\s*Sendbuf\[40\]\$UINT\s*:=\s*24;\s*Sendbuf\[42\]\$UINT\s*:=\s*1;\s*Sendbuf\[44\]\$UDINT\s*:=\s*320000;\s*Sendbuf\[64\]\$UDINT\s*:=\s*1280000;\s*Sendbuf\[68\]\$UINT\s*:=\s*4' '0x7E00 does not advertise the bounded D2-D4 envelope and general inline D5 SDO Read only for a stable BootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[52\]\$UINT\s*:=\s*1320' '0x7E00 MaxRequestPayloadBytes is not 1320.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[54\]\$UINT\s*:=\s*2040' '0x7E00 MaxResponsePayloadBytes is not 2040.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[56\]\$UINT\s*:=\s*1280' '0x7E00 MaxChunkDataBytes is not 1280.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[58\]\$UINT\s*:=\s*80' '0x7E00 CatalogEntryStride is not 80.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[60\]\$UINT\s*:=\s*16' '0x7E00 SignalValueEntryStride is not 16.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)Sendbuf\[68\]\$UINT\s*:=\s*0.*?if IsClientConnected\(#Diagnostics\).*?if diagnosticsBootId <> 0 then.*?Sendbuf\[68\]\$UINT\s*:=\s*4' '0x7E00 MaxSdoDataBytes must remain zero unless the diagnostics service is connected with a stable BootId, then advertise the general inline 4-byte limit.'
Assert-Match $diagnosticsCapabilitiesCaseBlock 'Sendbuf\[72\]\$UDINT\s*:=\s*diagnosticsBootId' '0x7E00 does not return the runtime DiagnosticsBootId.'
Assert-Match $diagnosticsCapabilitiesCaseBlock '(?s)SendData\(.*?udSize:=76' '0x7E00 does not send the complete 76-byte frame.'

$diagnosticsDispatchBlock = [regex]::Match(
    $diagnosticsHandlerBlock,
    '(?s)0x7E01,\s*0x7E02.*?0x7E50,\s*0x7E51:.*?end_case;').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsDispatchBlock)) {
    throw 'The reserved diagnostics command family is not delegated to LMCDiagnosticsService.'
}
Assert-Match $diagnosticsDispatchBlock '(?s)IsClientConnected\(#Diagnostics\).*?Diagnostics\.HandleRequest\(.*?ResponseCapacity:=2040.*?diagnosticsResponseSize <= 2040.*?SendData' 'Diagnostics service delegation or response bound is incomplete.'
}
else {
    $diagnosticsServiceCallCount = [regex]::Matches(
        $st,
        'Diagnostics\s*\.\s*HandleRequest\s*\(').Count
    if ($diagnosticsServiceCallCount -ne 1) {
        throw (
            "$ControlServiceCheckpoint Diagnostics.HandleRequest call count is " +
            "$diagnosticsServiceCallCount, expected exactly one.")
    }
    $diagnosticsSendCount = [regex]::Matches(
        $diagnosticsTransportBlock,
        '(?m)^\s*SendData\s*\(').Count
    if ($diagnosticsSendCount -ne 1) {
        throw (
            "$ControlServiceCheckpoint diagnostics route SendData count is " +
            "$diagnosticsSendCount, expected exactly one.")
    }
    if ([regex]::Matches(
            $st,
            '(?<![A-Za-z0-9_])GetDiagnosticsBootId\s*\(').Count -ne 0) {
        throw (
            "$ControlServiceCheckpoint TCPMotionInterface must not call " +
            'GetDiagnosticsBootId directly.')
    }
    foreach ($forbiddenLocalCapabilityPattern in @(
            '0x7E00',
            '(?i)\bcase\s+CommandID\s+of\b',
            'diagnosticsSchemaVersion',
            'diagnosticsRequestFlags',
            'diagnosticsRequestId',
            'diagnosticsBootId',
            '0x957F101E',
            '0x0000213F')) {
        if ($diagnosticsTransportBlock -match $forbiddenLocalCapabilityPattern) {
            throw (
                "$ControlServiceCheckpoint diagnostics transport still " +
                "assembles local capability state '$($Matches[0])'.")
        }
    }

    Assert-Match $diagnosticsTransportBlock (
        '(?s)_memset\(dest:=#Sendbuf,\s*usByte:=0,\s*' +
        'cntr:=sizeof\(Sendbuf\)\);\s*' +
        'diagnosticsResponseSize\s*:=\s*-1;\s*' +
        'if\s+IsClientConnected\(#Diagnostics\)\s+then\s*' +
        'diagnosticsResponseSize\s*:=\s*Diagnostics\.HandleRequest\(\s*' +
        'CommandId:=CommandID\$UINT\s*,\s*' +
        'Reference:=AxisRef\$UINT\s*,\s*' +
        'pRequest:=\(#RequestBuf\[8\]\)\$\^USINT\s*,\s*' +
        'RequestSize:=Payload\$UDINT\s*,\s*' +
        'pResponse:=\(#Sendbuf\[8\]\)\$\^USINT\s*,\s*' +
        'ResponseCapacity:=2040\s*,\s*' +
        'CallerSessionEpoch:=SessionEpoch\s*\);\s*end_if;') (
        "$ControlServiceCheckpoint diagnostics payload-only zero-copy ABI, " +
        'disconnected initialization, or connected-call gate is incomplete.')
    Assert-Match $diagnosticsTransportBlock (
        '(?s)if\s+\(diagnosticsResponseSize\s*>=\s*16\)\s*&\s*' +
        '\(diagnosticsResponseSize\s*<=\s*2040\)\s+then\s*' +
        'Sendbuf\[0\]\$UINT\s*:=\s*0;\s*' +
        'Sendbuf\[2\]\$UINT\s*:=\s*diagnosticsResponseSize\$UINT;\s*' +
        'Sendbuf\[4\]\$UDINT\s*:=\s*0;\s*' +
        'diagnosticsResponseSize\s*:=\s*diagnosticsResponseSize\s*\+\s*8;\s*' +
        'else\s*' +
        'Sendbuf\[0\]\$UINT\s*:=\s*1;\s*' +
        'Sendbuf\[2\]\$UINT\s*:=\s*4;\s*' +
        'Sendbuf\[4\]\$UDINT\s*:=\s*0;\s*' +
        'Sendbuf\[8\]\$UINT\s*:=\s*1;\s*' +
        'Sendbuf\[10\]\$INT\s*:=\s*-1;\s*' +
        'diagnosticsResponseSize\s*:=\s*12;\s*end_if;\s*' +
        'SendData\(\s*pData:=#Sendbuf\[0\],\s*' +
        'udSize:=diagnosticsResponseSize\$UDINT,\s*' +
        'dSocket:=CurrentSock,\s*bDirect:=TRUE\s*\);') (
        "$ControlServiceCheckpoint valid 16..2040 response wrapping, +8 outer " +
        'header, disconnected/invalid 12-byte -1 fallback, or shared send is incomplete.')
}

Assert-Match $diagnosticsLatch 'RealtimeTask\s*=\s*"true"' 'LMCEcatInputLatch is not declared as an RT class.'
Assert-Match $diagnosticsLatch 'SnapshotBytes\s*:\s*ARRAY \[0\.\.511\] OF USINT' 'LMCEcatInputLatch fixed snapshot storage is not 512 bytes.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?OS_READMICROSEC\(\).*?Drive1\.ActPos\.Read\(\).*?Drive4\.StateWord\.Read\(\).*?state := READY' 'LMCEcatInputLatch does not latch all four PDO images and timestamp in RtWork.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_setU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not stored atomically.'
Assert-Match $diagnosticsLatch 'sigclib_atomic_getU32\(pValue:=#PublishSequence' 'LMCEcatInputLatch publish sequence is not loaded atomically.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CopySnapshot.*?DestSize < 304.*?retryCount < 3.*?_memcpy.*?sequenceBefore = sequenceAfter' 'LMCEcatInputLatch bounded seqlock copy is incomplete.'
Assert-Match $diagnosticsLatch '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?sigclib_atomic_setU32\(pValue:=#PublishSequence,\s*value:=finalSequence\).*?IsClientConnected\(#RecorderStore\).*?RecorderStore\.AppendSnapshot\(\s*pSnapshot:=#SnapshotBytes\[0\],\s*SnapshotSize:=304\).*?state := READY' 'LMCEcatInputLatch does not append the final immutable 304-byte RT snapshot to RecorderStore.'
Assert-LMCEcatInputLatchGeneratedChannelContract `
    -LatchText $diagnosticsLatch `
    -IncludeCrevis:$topologyIoStructureGenerated `
    -Owner 'LMCEcatInputLatch'
if (-not $SourceOnly) {
    Assert-LMCEcatInputLatchGeneratedExternalConnections `
        -NetworkTableText $motionNetworkTable `
        -IncludeCrevis:$topologyIoStructureGenerated `
        -Owner 'Motion_Network generated table'
}

$legacyLatchGeneratedFixture = @'
<Class Name="LMCEcatInputLatch">
<Channels>
<Client Name="EcatMaster" Required="true" Internal="false"/>
<Client Name="Drive1" Required="true" Internal="false"/>
<Client Name="Drive2" Required="true" Internal="false"/>
<Client Name="Drive3" Required="true" Internal="false"/>
<Client Name="Drive4" Required="true" Internal="false"/>
<Client Name="RecorderStore" Required="true" Internal="false"/>
</Channels>
</Class>
LMCEcatInputLatch : CLASS
EcatMaster : CltChCmd_ECAT_Master_Base;
Drive1 : CltChCmd_Elmo_1;
Drive2 : CltChCmd_Elmo_2;
Drive3 : CltChCmd_Elmo_3;
Drive4 : CltChCmd_Elmo_4;
RecorderStore : CltChCmd_LMCRecorderStore;
END_CLASS;
#pragma usingLtd ECAT_Master_Base
#pragma usingLtd Elmo_1
#pragma usingLtd Elmo_2
#pragma usingLtd Elmo_3
#pragma usingLtd Elmo_4
#pragma usingLtd LMCRecorderStore
FUNCTION GLOBAL TAB LMCEcatInputLatch::@CT_
1$UINT, 6$UINT, 0$UINT,
(::LMCEcatInputLatch.EcatMaster.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(1), "EcatMaster", TO_UDINT(11), "ECAT_Master_Base", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive1.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(2), "Drive1", TO_UDINT(12), "Elmo_1", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive2.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(3), "Drive2", TO_UDINT(13), "Elmo_2", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive3.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(4), "Drive3", TO_UDINT(14), "Elmo_3", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive4.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(5), "Drive4", TO_UDINT(15), "Elmo_4", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.RecorderStore.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(6), "RecorderStore", TO_UDINT(16), "LMCRecorderStore", 0$UINT, 0$UINT,
END_FUNCTION
//}}LSL_DECLARATION
'@
$integratedLatchGeneratedFixture = @'
<Class Name="LMCEcatInputLatch">
<Channels>
<Client Name="EcatMaster" Required="true" Internal="false"/>
<Client Name="Drive1" Required="true" Internal="false"/>
<Client Name="Drive2" Required="true" Internal="false"/>
<Client Name="Drive3" Required="true" Internal="false"/>
<Client Name="Drive4" Required="true" Internal="false"/>
<Client Name="RecorderStore" Required="true" Internal="false"/>
<Client Name="Coupler" Required="true" Internal="false"/>
<Client Name="InputSlot" Required="true" Internal="false"/>
<Client Name="OutputSlot" Required="true" Internal="false"/>
</Channels>
</Class>
LMCEcatInputLatch : CLASS
EcatMaster : CltChCmd_ECAT_Master_Base;
Drive1 : CltChCmd_Elmo_1;
Drive2 : CltChCmd_Elmo_2;
Drive3 : CltChCmd_Elmo_3;
Drive4 : CltChCmd_Elmo_4;
RecorderStore : CltChCmd_LMCRecorderStore;
Coupler : CltChCmd_GL_9086_1;
InputSlot : CltChCmd_GL_9086_1_Slot00;
OutputSlot : CltChCmd_GL_9086_1_Slot01;
END_CLASS;
#pragma usingLtd ECAT_Master_Base
#pragma usingLtd Elmo_1
#pragma usingLtd Elmo_2
#pragma usingLtd Elmo_3
#pragma usingLtd Elmo_4
#pragma usingLtd LMCRecorderStore
#pragma usingLtd GL_9086_1
#pragma usingLtd GL_9086_1_Slot00
#pragma usingLtd GL_9086_1_Slot01
FUNCTION GLOBAL TAB LMCEcatInputLatch::@CT_
1$UINT, 9$UINT, 0$UINT,
(::LMCEcatInputLatch.EcatMaster.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(1), "EcatMaster", TO_UDINT(11), "ECAT_Master_Base", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive1.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(2), "Drive1", TO_UDINT(12), "Elmo_1", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive2.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(3), "Drive2", TO_UDINT(13), "Elmo_2", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive3.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(4), "Drive3", TO_UDINT(14), "Elmo_3", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Drive4.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(5), "Drive4", TO_UDINT(15), "Elmo_4", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.RecorderStore.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(6), "RecorderStore", TO_UDINT(16), "LMCRecorderStore", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.Coupler.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(7), "Coupler", TO_UDINT(17), "GL_9086_1", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.InputSlot.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(8), "InputSlot", TO_UDINT(18), "GL_9086_1_Slot00", 0$UINT, 0$UINT,
(::LMCEcatInputLatch.OutputSlot.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(9), "OutputSlot", TO_UDINT(19), "GL_9086_1_Slot01", 0$UINT, 0$UINT,
END_FUNCTION
//}}LSL_DECLARATION
'@
Assert-LMCEcatInputLatchGeneratedChannelContract `
    -LatchText $legacyLatchGeneratedFixture `
    -Owner 'LMCEcatInputLatch legacy generated-channel canonical fixture'
Assert-LMCEcatInputLatchGeneratedChannelContract `
    -LatchText $integratedLatchGeneratedFixture `
    -IncludeCrevis `
    -Owner 'LMCEcatInputLatch integrated generated-channel canonical fixture'

$legacyDrive4PchFixtureLine = '(::LMCEcatInputLatch.Drive4.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(5), "Drive4", TO_UDINT(15), "Elmo_4", 0$UINT, 0$UINT,'
$integratedInputSlotPchFixtureLine = '(::LMCEcatInputLatch.InputSlot.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(8), "InputSlot", TO_UDINT(18), "GL_9086_1_Slot00", 0$UINT, 0$UINT,'
$integratedOutputSlotPchFixtureLine = '(::LMCEcatInputLatch.OutputSlot.pCh)$UINT, _CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, TO_UDINT(9), "OutputSlot", TO_UDINT(19), "GL_9086_1_Slot01", 0$UINT, 0$UINT,'
$latchGeneratedChannelNegativeFixtures = [ordered]@{
    'LegacyCountDrift' = @{
        Text = $legacyLatchGeneratedFixture.Replace(
            '1$UINT, 6$UINT, 0$UINT,',
            '1$UINT, 7$UINT, 0$UINT,')
        IncludeCrevis = $false
    }
    'LegacyDuplicatePCh' = @{
        Text = $legacyLatchGeneratedFixture.Replace(
            $legacyDrive4PchFixtureLine,
            $legacyDrive4PchFixtureLine + "`n" + $legacyDrive4PchFixtureLine)
        IncludeCrevis = $false
    }
    'IntegratedCountDrift' = @{
        Text = $integratedLatchGeneratedFixture.Replace(
            '1$UINT, 9$UINT, 0$UINT,',
            '1$UINT, 8$UINT, 0$UINT,')
        IncludeCrevis = $true
    }
    'IntegratedMissingPCh' = @{
        Text = $integratedLatchGeneratedFixture.Replace(
            $integratedInputSlotPchFixtureLine,
            '')
        IncludeCrevis = $true
    }
    'IntegratedDuplicatePCh' = @{
        Text = $integratedLatchGeneratedFixture.Replace(
            $integratedOutputSlotPchFixtureLine,
            $integratedOutputSlotPchFixtureLine + "`n" +
                $integratedOutputSlotPchFixtureLine)
        IncludeCrevis = $true
    }
    'IntegratedPChTypeDrift' = @{
        Text = $integratedLatchGeneratedFixture.Replace(
            $integratedInputSlotPchFixtureLine,
            $integratedInputSlotPchFixtureLine.Replace(
                '"GL_9086_1_Slot00"',
                '"GL_9086_1_Slot01"'))
        IncludeCrevis = $true
    }
    'IntegratedMissingPragma' = @{
        Text = $integratedLatchGeneratedFixture.Replace(
            '#pragma usingLtd GL_9086_1_Slot00',
            '')
        IncludeCrevis = $true
    }
}
foreach ($negativeFixture in
        $latchGeneratedChannelNegativeFixtures.GetEnumerator()) {
    $canonicalFixture = if ($negativeFixture.Value.IncludeCrevis) {
        $integratedLatchGeneratedFixture
    }
    else {
        $legacyLatchGeneratedFixture
    }
    if ($negativeFixture.Value.Text -ceq $canonicalFixture) {
        throw (
            'LMCEcatInputLatch generated-channel negative fixture did not ' +
            "mutate the source: $($negativeFixture.Key).")
    }
    $negativeRejected = $false
    try {
        Assert-LMCEcatInputLatchGeneratedChannelContract `
            -LatchText ([string]$negativeFixture.Value.Text) `
            -IncludeCrevis:([bool]$negativeFixture.Value.IncludeCrevis) `
            -Owner ('LMCEcatInputLatch generated-channel negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCEcatInputLatch generated-channel verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
}

$latchIdeVariableFixture = @'
LMCEcatInputLatch : CLASS
//Variables:
OutputRevision : UDINT;
OutputObserved : BOOL;
OutputPreviousValid : BOOL;
OutputPreviousValue : UDINT;
//Functions:
END_CLASS;
'@
Assert-LMCEcatInputLatchIdeVariableContract `
    -LatchText $latchIdeVariableFixture `
    -Owner 'LMCEcatInputLatch IDE-variable canonical fixture'
$latchIdeVariableNegativeFixtures = [ordered]@{
    'WrongTypeCommentSpoof' = $latchIdeVariableFixture.Replace(
        'OutputRevision : UDINT;',
        'OutputRevision : UINT; // OutputRevision : UDINT;')
    'CommentOnlyDeclaration' = $latchIdeVariableFixture.Replace(
        'OutputPreviousValid : BOOL;',
        '// OutputPreviousValid : BOOL;')
    'DuplicateDeclaration' = $latchIdeVariableFixture.Replace(
        'OutputObserved : BOOL;',
        "OutputObserved : BOOL;`nOutputObserved : BOOL;")
    'MethodVarInputSpoof' = $latchIdeVariableFixture.Replace(
        'OutputRevision : UDINT;',
        '').Replace(
        '//Functions:',
        ("//Functions:`nFUNCTION ProbeInput`nVAR_INPUT`n" +
         "OutputRevision : UDINT;`nEND_VAR`nEND_FUNCTION"))
    'MethodVarOutputSpoof' = $latchIdeVariableFixture.Replace(
        'OutputObserved : BOOL;',
        '').Replace(
        '//Functions:',
        ("//Functions:`nFUNCTION ProbeOutput`nVAR_OUTPUT`n" +
         "OutputObserved : BOOL;`nEND_VAR`nEND_FUNCTION"))
    'MethodVarLocalSpoof' = $latchIdeVariableFixture.Replace(
        'OutputPreviousValid : BOOL;',
        '').Replace(
        '//Functions:',
        ("//Functions:`nFUNCTION ProbeLocal`nVAR`n" +
         "OutputPreviousValid : BOOL;`nEND_VAR`nEND_FUNCTION"))
}
foreach ($negativeFixture in
        $latchIdeVariableNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -ceq $latchIdeVariableFixture) {
        throw (
            'LMCEcatInputLatch IDE-variable negative fixture did not mutate ' +
            "the source: $($negativeFixture.Key).")
    }
    $negativeRejected = $false
    try {
        Assert-LMCEcatInputLatchIdeVariableContract `
            -LatchText ([string]$negativeFixture.Value) `
            -Owner ('LMCEcatInputLatch IDE-variable negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCEcatInputLatch IDE-variable verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
}

$legacyLatchNetworkFixture = @'
//External connections
0$UDINT,
5$UDINT,
TO_UDINT(17), "EcatMaster", C_DIR, TO_UDINT(2283968053), "EtherCAT_PLC1","ClassState",
TO_UDINT(17), "Drive1", C_DIR, TO_UDINT(1622340897), "Elmo_11","ClassState",
TO_UDINT(17), "Drive2", C_DIR, TO_UDINT(1268754146), "Elmo_21","ClassState",
TO_UDINT(17), "Drive3", C_DIR, TO_UDINT(1384421283), "Elmo_31","ClassState",
TO_UDINT(17), "Drive4", C_DIR, TO_UDINT(499450212), "Elmo_41","ClassState",
//Magic internal connections
'@
$integratedLatchNetworkFixture = @'
//External connections
0$UDINT,
8$UDINT,
TO_UDINT(17), "EcatMaster", C_DIR, TO_UDINT(2283968053), "EtherCAT_PLC1","ClassState",
TO_UDINT(17), "Drive1", C_DIR, TO_UDINT(1622340897), "Elmo_11","ClassState",
TO_UDINT(17), "Drive2", C_DIR, TO_UDINT(1268754146), "Elmo_21","ClassState",
TO_UDINT(17), "Drive3", C_DIR, TO_UDINT(1384421283), "Elmo_31","ClassState",
TO_UDINT(17), "Drive4", C_DIR, TO_UDINT(499450212), "Elmo_41","ClassState",
TO_UDINT(17), "Coupler", C_DIR, TO_UDINT(2797582533), "GL_9086_11","ClassState",
TO_UDINT(17), "InputSlot", C_DIR, TO_UDINT(2495490262), "GL_9086_1_Slot001","ClassState",
TO_UDINT(17), "OutputSlot", C_DIR, TO_UDINT(2376407447), "GL_9086_1_Slot011","ClassState",
//Magic internal connections
'@
Assert-LMCEcatInputLatchGeneratedExternalConnections `
    -NetworkTableText $legacyLatchNetworkFixture `
    -Owner 'LMCEcatInputLatch legacy generated-network canonical fixture'
Assert-LMCEcatInputLatchGeneratedExternalConnections `
    -NetworkTableText $integratedLatchNetworkFixture `
    -IncludeCrevis `
    -Owner 'LMCEcatInputLatch integrated generated-network canonical fixture'
$integratedLatchNetworkCrLfFixture = [regex]::Replace(
    $integratedLatchNetworkFixture,
    '\r?\n',
    "`r`n")
Assert-LMCEcatInputLatchGeneratedExternalConnections `
    -NetworkTableText $integratedLatchNetworkCrLfFixture `
    -IncludeCrevis `
    -Owner 'LMCEcatInputLatch integrated CRLF generated-network canonical fixture'

$legacyDrive4NetworkFixtureLine = 'TO_UDINT(17), "Drive4", C_DIR, TO_UDINT(499450212), "Elmo_41","ClassState",'
$integratedInputSlotNetworkFixtureLine = 'TO_UDINT(17), "InputSlot", C_DIR, TO_UDINT(2495490262), "GL_9086_1_Slot001","ClassState",'
$integratedOutputSlotNetworkFixtureLine = 'TO_UDINT(17), "OutputSlot", C_DIR, TO_UDINT(2376407447), "GL_9086_1_Slot011","ClassState",'
$integratedCountDriftCrLfFixture =
    Set-LasalGeneratedExternalConnectionCountFixture `
        -Text $integratedLatchNetworkCrLfFixture `
        -DeclaredCount 9 `
        -Owner 'LMCEcatInputLatch CRLF count-drift fixture'
$integratedParallelOwnerCrLfFixture =
    $integratedCountDriftCrLfFixture.Replace(
        '//Magic internal connections',
        ('TO_UDINT(99), "ParallelOutput", C_DIR, ' +
         'TO_UDINT(2376407447), "GL_9086_1_Slot011","ClassState",' +
         "`r`n//Magic internal connections"))
$latchGeneratedNetworkNegativeFixtures = [ordered]@{
    'LegacyMissingDrive' = @{
        Text = $legacyLatchNetworkFixture.Replace(
            $legacyDrive4NetworkFixtureLine,
            '')
        IncludeCrevis = $false
    }
    'LegacyUnexpectedCrevis' = @{
        Text = $legacyLatchNetworkFixture.Replace(
            '//Magic internal connections',
            ('TO_UDINT(17), "Coupler", C_DIR, TO_UDINT(2797582533), ' +
             '"GL_9086_11","ClassState",' + "`n" +
             '//Magic internal connections'))
        IncludeCrevis = $false
    }
    'IntegratedMissingInputSlot' = @{
        Text = $integratedLatchNetworkFixture.Replace(
            $integratedInputSlotNetworkFixtureLine,
            '')
        IncludeCrevis = $true
    }
    'IntegratedDuplicateOutputSlot' = @{
        Text = $integratedLatchNetworkFixture.Replace(
            $integratedOutputSlotNetworkFixtureLine,
            $integratedOutputSlotNetworkFixtureLine + "`n" +
                $integratedOutputSlotNetworkFixtureLine)
        IncludeCrevis = $true
    }
    'IntegratedTargetSwap' = @{
        Text = $integratedLatchNetworkFixture.Replace(
            '"GL_9086_1_Slot001","ClassState"',
            '"GL_9086_1_Slot011","ClassState"')
        IncludeCrevis = $true
    }
    'IntegratedTargetIndexDrift' = @{
        Text = $integratedLatchNetworkFixture.Replace(
            ('TO_UDINT(2495490262), ' +
             '"GL_9086_1_Slot001","ClassState"'),
            ('TO_UDINT(2495490263), ' +
             '"GL_9086_1_Slot001","ClassState"'))
        IncludeCrevis = $true
    }
    'IntegratedDeclaredCountDriftCrLf' = @{
        Text = $integratedCountDriftCrLfFixture
        CanonicalText = $integratedLatchNetworkCrLfFixture
        IncludeCrevis = $true
    }
    'IntegratedParallelOutputOwnerCrLf' = @{
        Text = $integratedParallelOwnerCrLfFixture
        CanonicalText = $integratedLatchNetworkCrLfFixture
        IncludeCrevis = $true
    }
}
foreach ($negativeFixture in
        $latchGeneratedNetworkNegativeFixtures.GetEnumerator()) {
    $canonicalFixture = if (
        $negativeFixture.Value.ContainsKey('CanonicalText')) {
        [string]$negativeFixture.Value.CanonicalText
    }
    elseif ($negativeFixture.Value.IncludeCrevis) {
        $integratedLatchNetworkFixture
    }
    else {
        $legacyLatchNetworkFixture
    }
    if ($negativeFixture.Value.Text -ceq $canonicalFixture) {
        throw (
            'LMCEcatInputLatch generated-network negative fixture did not ' +
            "mutate the source: $($negativeFixture.Key).")
    }
    $negativeRejected = $false
    try {
        Assert-LMCEcatInputLatchGeneratedExternalConnections `
            -NetworkTableText ([string]$negativeFixture.Value.Text) `
            -IncludeCrevis:([bool]$negativeFixture.Value.IncludeCrevis) `
            -Owner ('LMCEcatInputLatch generated-network negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCEcatInputLatch generated-network verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
}

$diagnosticsLatchRtWorkBlock = [regex]::Match(
    $diagnosticsLatch,
    '(?s)FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsLatchRtWorkBlock)) {
    throw 'LMCEcatInputLatch.RtWork implementation was not found.'
}

if ($topologyIoStructureGenerated) {
    foreach ($crevisConnectionContract in @(
            @{ Client = 'Coupler'; Object = 'GL_9086_11' },
            @{ Client = 'InputSlot'; Object = 'GL_9086_1_Slot001' },
            @{ Client = 'OutputSlot'; Object = 'GL_9086_1_Slot011' })) {
        $source = 'LMCEcatInputLatch1.' +
            $crevisConnectionContract.Client
        $destination = $crevisConnectionContract.Object + '.ClassState'
        $allRelatedConnections = @()
        foreach ($networkXml in @(
                $motionNetworkXml,
                $commNetworkXml,
                $etherCatNetworkXml)) {
            $allRelatedConnections += @($networkXml.SelectNodes(
                "//Connection[@Source='$source' or @Destination='$destination']"))
        }
        if ($allRelatedConnections.Count -ne 1 -or
            $allRelatedConnections[0].Source -cne $source -or
            $allRelatedConnections[0].Destination -cne $destination) {
            throw (
                "$source must have exactly one cross-network connection to " +
                "$destination and that destination must have no parallel owner.")
        }
    }
}

# The checkpoint advances only after LASAL IDE has generated the class and
# network structure.  StaticTopologyOnly rejects partial manual declarations;
# later checkpoints require the complete integrated RT owner contract.
if (-not $topologyIoStructureGenerated) {
    $diagnosticsLatchCode = Get-LasalScanText $diagnosticsLatch
    $diagnosticsServiceCode = Get-LasalScanText $diagnosticsService
    $diagnosticsLatchChannelMetadata = [regex]::Match(
        $diagnosticsLatch,
        '(?s)<Channels>.*?</Channels>').Value
    if ($diagnosticsLatchChannelMetadata -match
        '<Client Name="(?:Coupler|InputSlot|OutputSlot)"' -or
        $diagnosticsLatchCode -match
        '\bCltChCmd_GL_9086_1(?:_Slot0[01])?\b' -or
        $diagnosticsLatchCode -match (
            '\b(?:CopyTopologyIoSnapshot|AdvanceOutputRevision|' +
            'OutputRevision|OutputObserved|OutputPreviousValid|' +
            'OutputPreviousValue|TryQueueOutputWrite|' +
            'CopyOutputCompletion|CancelQueuedOutput|IsOutputReusable)\b')) {
        throw ('Static-only EtherCAT topology checkpoint contains a partial ' +
            'LMCEcatInputLatch CREVIS RT owner. Generate the complete IDE ' +
            'structure and advance the verifier checkpoint atomically.')
    }
    $partialCrevisConnections = @()
    foreach ($networkXml in @(
            $motionNetworkXml,
            $commNetworkXml,
            $etherCatNetworkXml)) {
        $partialCrevisConnections += @($networkXml.SelectNodes(
            "//Connection[" +
            "@Source='LMCEcatInputLatch1.Coupler' or " +
            "@Source='LMCEcatInputLatch1.InputSlot' or " +
            "@Source='LMCEcatInputLatch1.OutputSlot' or " +
            "@Destination='GL_9086_11.ClassState' or " +
            "@Destination='GL_9086_1_Slot001.ClassState' or " +
            "@Destination='GL_9086_1_Slot011.ClassState']"))
    }
    if ($partialCrevisConnections.Count -ne 0) {
        throw ('Static-only EtherCAT topology checkpoint contains ' +
            "$($partialCrevisConnections.Count) partial CREVIS diagnostics " +
            'network connection(s). Generate all class and network structure ' +
            'and advance the verifier checkpoint atomically.')
    }
    if ($diagnosticsServiceCode -match
        '(?m)(?:HandleEtherCATTopologyIoRequest|^\s*0x(?:7E13|7E22|7E23)\s*:)') {
        throw ('Static-only EtherCAT topology checkpoint contains a partial ' +
            'topology/I/O diagnostics helper or live 0x7E13/0x7E22/0x7E23 ' +
            'handler while capability bits 15..17 are off.')
    }
}
elseif ($topologyIoIdeStructureReady) {
    foreach ($crevisClient in @(
            @{ Name = 'Coupler'; Type = 'GL_9086_1'; Object = 'GL_9086_11' },
            @{ Name = 'InputSlot'; Type = 'GL_9086_1_Slot00'; Object = 'GL_9086_1_Slot001' },
            @{ Name = 'OutputSlot'; Type = 'GL_9086_1_Slot01'; Object = 'GL_9086_1_Slot011' })) {
        Assert-Match $diagnosticsLatch (
            '<Client Name="' + $crevisClient.Name +
            '" Required="true" Internal="false"/>') (
            "LMCEcatInputLatch.$($crevisClient.Name) required IDE client metadata is missing.")
        Assert-Match $diagnosticsLatch (
            $crevisClient.Name + '\s*:\s*CltChCmd_' +
            $crevisClient.Type + ';') (
            "LMCEcatInputLatch.$($crevisClient.Name) typed client declaration is missing.")

        $crevisConnections = @($motionNetworkXml.SelectNodes(
            "//Connection[@Source='LMCEcatInputLatch1.$($crevisClient.Name)' and " +
            "@Destination='$($crevisClient.Object).ClassState']"))
        if ($crevisConnections.Count -ne 1) {
            throw (
                "LMCEcatInputLatch1.$($crevisClient.Name) -> " +
                "$($crevisClient.Object).ClassState connection count is " +
                "$($crevisConnections.Count), expected exactly one.")
        }
    }

    Assert-LMCEcatInputLatchIdeVariableContract `
        -LatchText $diagnosticsLatch `
        -Owner 'LMCEcatInputLatch IDE structure'

    Assert-Match $diagnosticsLatch (
        '(?s)FUNCTION GLOBAL CopyTopologyIoSnapshot.*?' +
        'pDest\s*:\s*\^void;.*?DestSize\s*:\s*UDINT;.*?' +
        'Result\s*:\s*DINT;') (
        'LMCEcatInputLatch.CopyTopologyIoSnapshot IDE declaration is incomplete.')
    $copyTopologyIoStubMatches = [regex]::Matches(
        (Get-LasalScanText $diagnosticsLatch),
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CopyTopologyIoSnapshot.*?END_FUNCTION')
    if ($copyTopologyIoStubMatches.Count -ne 1) {
        throw ('LMCEcatInputLatch.CopyTopologyIoSnapshot IDE implementation ' +
            'must occur exactly once.')
    }
    Assert-Match $copyTopologyIoStubMatches[0].Value (
        '(?s)\A\s*FUNCTION GLOBAL LMCEcatInputLatch::CopyTopologyIoSnapshot\s*' +
        'VAR_INPUT\s*pDest\s*:\s*\^Void;\s*' +
        'DestSize\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR\s*' +
        'END_FUNCTION\s*\z') (
        'LMCEcatInputLatch.CopyTopologyIoSnapshot must be an exact empty IDE stub.')
    Assert-Match $diagnosticsLatch (
        '(?s)FUNCTION GLOBAL AdvanceOutputRevision.*?' +
        'Revision\s*:\s*UDINT;') (
        'LMCEcatInputLatch.AdvanceOutputRevision IDE declaration is incomplete.')
    $advanceOutputRevisionStubMatches = [regex]::Matches(
        (Get-LasalScanText $diagnosticsLatch),
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::AdvanceOutputRevision.*?END_FUNCTION')
    if ($advanceOutputRevisionStubMatches.Count -ne 1) {
        throw ('LMCEcatInputLatch.AdvanceOutputRevision IDE implementation ' +
            'must occur exactly once.')
    }
    Assert-Match $advanceOutputRevisionStubMatches[0].Value (
        '(?s)\A\s*FUNCTION GLOBAL LMCEcatInputLatch::AdvanceOutputRevision\s*' +
        'VAR_OUTPUT\s*Revision\s*:\s*UDINT;\s*END_VAR\s*' +
        'END_FUNCTION\s*\z') (
        'LMCEcatInputLatch.AdvanceOutputRevision must be an exact empty IDE stub.')

    Assert-Match $diagnosticsService (
        '(?s)FUNCTION HandleEtherCATTopologyIoRequest\s*' +
        'VAR_INPUT\s*CommandId\s*:\s*UINT;\s*' +
        'pRequest\s*:\s*\^USINT;\s*RequestSize\s*:\s*UDINT;\s*' +
        'pResponse\s*:\s*\^USINT;\s*ResponseCapacity\s*:\s*UDINT;\s*' +
        'CallerSessionEpoch\s*:\s*UDINT;\s*' +
        'CurrentDiagnosticsBootId\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*ResponseSize\s*:\s*DINT;\s*END_VAR') (
        'LMCDiagnosticsService.HandleEtherCATTopologyIoRequest private IDE declaration is incomplete.')
    $topologyIoHelperStubMatches = [regex]::Matches(
        (Get-LasalScanText $diagnosticsService),
        '(?s)FUNCTION LMCDiagnosticsService::HandleEtherCATTopologyIoRequest.*?END_FUNCTION')
    if ($topologyIoHelperStubMatches.Count -ne 1) {
        throw ('LMCDiagnosticsService.HandleEtherCATTopologyIoRequest IDE ' +
            'implementation must occur exactly once.')
    }
    Assert-Match $topologyIoHelperStubMatches[0].Value (
        '(?s)\A\s*FUNCTION LMCDiagnosticsService::HandleEtherCATTopologyIoRequest\s*' +
        'VAR_INPUT\s*CommandId\s*:\s*UINT;\s*' +
        'pRequest\s*:\s*\^USINT;\s*RequestSize\s*:\s*UDINT;\s*' +
        'pResponse\s*:\s*\^USINT;\s*ResponseCapacity\s*:\s*UDINT;\s*' +
        'CallerSessionEpoch\s*:\s*UDINT;\s*' +
        'CurrentDiagnosticsBootId\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*ResponseSize\s*:\s*DINT;\s*END_VAR\s*' +
        'END_FUNCTION\s*\z') (
        'LMCDiagnosticsService.HandleEtherCATTopologyIoRequest must be an exact empty IDE stub.')

    $diagnosticsServiceCode = Get-LasalScanText $diagnosticsService
    if ($diagnosticsServiceCode -match
        '(?m)^\s*0x(?:7E13|7E22|7E23)\s*:') {
        throw ('IdeStructureReady must not contain a live topology/I/O case ' +
            'before the external implementation checkpoint is selected.')
    }
    $latchImplementationMarker = $diagnosticsLatch.IndexOf(
        '//{{LSL_IMPLEMENTATION',
        [StringComparison]::Ordinal)
    $serviceImplementationMarker = $diagnosticsService.IndexOf(
        '//{{LSL_IMPLEMENTATION',
        [StringComparison]::Ordinal)
    if ($latchImplementationMarker -lt 0 -or
        $serviceImplementationMarker -lt 0) {
        throw 'IdeStructureReady implementation region marker is missing.'
    }
    $latchImplementationScan = Get-LasalScanText (
        $diagnosticsLatch.Substring($latchImplementationMarker))
    $serviceImplementationScan = Get-LasalScanText (
        $diagnosticsService.Substring($serviceImplementationMarker))
    if ($latchImplementationScan -match
            '\b(?:Coupler|InputSlot|OutputSlot|OutputRevision|' +
            'OutputObserved|OutputPreviousValid|OutputPreviousValue)\b' -or
        $latchImplementationScan -match
            '\.(?:CopyTopologyIoSnapshot|AdvanceOutputRevision)\s*\(' -or
        [regex]::Matches(
            $serviceImplementationScan,
            '\bHandleEtherCATTopologyIoRequest\b').Count -ne 1) {
        throw ('IdeStructureReady implementation must contain only the three ' +
            'empty generated method stubs; CREVIS reads, output state mutation, ' +
            'and topology/I/O helper calls belong to the next checkpoint.')
    }
}
else {
    foreach ($crevisClient in @(
            @{ Name = 'Coupler'; Type = 'GL_9086_1'; Object = 'GL_9086_11' },
            @{ Name = 'InputSlot'; Type = 'GL_9086_1_Slot00'; Object = 'GL_9086_1_Slot001' },
            @{ Name = 'OutputSlot'; Type = 'GL_9086_1_Slot01'; Object = 'GL_9086_1_Slot011' })) {
        Assert-Match $diagnosticsLatch (
            '<Client Name="' + $crevisClient.Name +
            '" Required="true" Internal="false"/>') (
            "LMCEcatInputLatch.$($crevisClient.Name) required IDE client metadata is missing.")
        Assert-Match $diagnosticsLatch (
            $crevisClient.Name + '\s*:\s*CltChCmd_' +
            $crevisClient.Type + ';') (
            "LMCEcatInputLatch.$($crevisClient.Name) typed client declaration is missing.")

        $crevisConnections = @($motionNetworkXml.SelectNodes(
            "//Connection[@Source='LMCEcatInputLatch1.$($crevisClient.Name)' and " +
            "@Destination='$($crevisClient.Object).ClassState']"))
        if ($crevisConnections.Count -ne 1) {
            throw (
                "LMCEcatInputLatch1.$($crevisClient.Name) -> " +
                "$($crevisClient.Object).ClassState connection count is " +
                "$($crevisConnections.Count), expected exactly one.")
        }
    }
    $allOutputSlotClientConnections = @(
        $motionNetworkXml.SelectNodes(
            "//Connection[@Destination='GL_9086_1_Slot011.ClassState']")) + @(
        $commNetworkXml.SelectNodes(
            "//Connection[@Destination='GL_9086_1_Slot011.ClassState']")) + @(
        $etherCatNetworkXml.SelectNodes(
            "//Connection[@Destination='GL_9086_1_Slot011.ClassState']"))
    if ($allOutputSlotClientConnections.Count -ne 1 -or
        $allOutputSlotClientConnections[0].Source -ne
            'LMCEcatInputLatch1.OutputSlot') {
        throw ('GL_9086_1_Slot011.ClassState must have exactly one diagnostics ' +
            'client, LMCEcatInputLatch1.OutputSlot; parallel output owners are forbidden.')
    }

    Assert-LMCEcatInputLatchIdeVariableContract `
        -LatchText $diagnosticsLatch `
        -Owner 'LMCEcatInputLatch integrated structure'
    Assert-Match $diagnosticsLatch (
        '(?s)FUNCTION GLOBAL CopyTopologyIoSnapshot.*?' +
        'pDest\s*:\s*\^void;.*?DestSize\s*:\s*UDINT;.*?' +
        'Result\s*:\s*DINT;') (
        'LMCEcatInputLatch.CopyTopologyIoSnapshot IDE declaration is incomplete.')
    $topologyCopyBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::' +
        'CopyTopologyIoSnapshot.*?END_FUNCTION').Value
    if ([string]::IsNullOrWhiteSpace($topologyCopyBlock)) {
        throw 'LMCEcatInputLatch.CopyTopologyIoSnapshot implementation was not found.'
    }
    Assert-Match $topologyCopyBlock (
        '(?s)\AFUNCTION GLOBAL LMCEcatInputLatch::CopyTopologyIoSnapshot\s*' +
        'VAR_INPUT\s*pDest\s*:\s*\^Void;\s*' +
        'DestSize\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR') (
        'LMCEcatInputLatch.CopyTopologyIoSnapshot implementation ABI is ' +
        'incomplete or narrowed.')
    Assert-Match $topologyCopyBlock (
        '(?s)retryCount\s*:\s*UINT;.*?' +
        'sequenceBefore\s*:\s*UDINT;.*?' +
        'sequenceAfter\s*:\s*UDINT;') (
        'LMCEcatInputLatch.CopyTopologyIoSnapshot retry and U32 seqlock ' +
        'local declarations are incomplete or narrowed.')
    Assert-Match $topologyCopyBlock (
        '(?s)Result := -1;\s*' +
        'if \(pDest = NIL\) \| \(DestSize < 464\) then\s*' +
        'Result := -2;\s*RETURN;\s*end_if;\s*' +
        'retryCount := 0;\s*while retryCount < 3 do\s*' +
        'sequenceBefore := sigclib_atomic_getU32\(' +
        'pValue:=#PublishSequence\);.*?' +
        'if \(sequenceBefore and 1\) = 0 then\s*' +
        '_memcpy\(ptr1:=pDest,\s*ptr2:=#SnapshotBytes\[0\],\s*' +
        'cntr:=464\);.*?' +
        'sequenceAfter := sigclib_atomic_getU32\(' +
        'pValue:=#PublishSequence\);.*?' +
        'if \(sequenceBefore = sequenceAfter\) &\s*' +
        '\(\(sequenceAfter and 1\) = 0\) &\s*' +
        '\(sequenceAfter <> 0\) then\s*' +
        'Result := 0;\s*RETURN;\s*end_if;\s*end_if;\s*' +
        'retryCount \+= 1;\s*end_while;\s*END_FUNCTION') (
        'LMCEcatInputLatch 464-byte bounded fail-closed seqlock copy is incomplete.')
    if ([regex]::Matches(
            $topologyCopyBlock,
            'Result\s*:=\s*0;').Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyBlock,
            'retryCount\s*\+=\s*1;').Count -ne 1) {
        throw ('LMCEcatInputLatch.CopyTopologyIoSnapshot must have one success ' +
            'return and one bounded-loop retry increment.')
    }
    $topologyCopyScanText = Get-LasalScanText $topologyCopyBlock
    $topologyCopyEvenBlock = Get-UniqueLasalIfBlockContaining `
        -Text $topologyCopyBlock `
        -ConditionPattern (
            '\(sequenceBefore\s+and\s+1\)\s*=\s*0\s+then') `
        -RequiredPattern '_memcpy\(' `
        -Message ('LMCEcatInputLatch.CopyTopologyIoSnapshot even-sequence ' +
            'copy guard was not found.')
    $topologyCopyEvenArm = Get-LasalFirstThenArm $topologyCopyEvenBlock
    $topologyCopyStableBlock = Get-UniqueLasalIfBlockContaining `
        -Text $topologyCopyEvenArm `
        -ConditionPattern (
            '\(sequenceBefore\s*=\s*sequenceAfter\)\s*&\s*' +
            '\(\(sequenceAfter\s+and\s+1\)\s*=\s*0\)\s*&\s*' +
            '\(sequenceAfter\s*<>\s*0\)\s+then') `
        -RequiredPattern 'Result\s*:=\s*0;' `
        -Message ('LMCEcatInputLatch.CopyTopologyIoSnapshot stable-sequence ' +
            'success guard was not found.')
    $topologyCopyStableArm = Get-LasalFirstThenArm $topologyCopyStableBlock
    if ([regex]::Matches(
            $topologyCopyScanText,
            '\bsequenceBefore\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyScanText,
            ('sequenceBefore\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#PublishSequence\);')).Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyScanText,
            '\bsequenceAfter\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyEvenArm,
            ('sequenceAfter\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#PublishSequence\);')).Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyScanText,
            '\bretryCount\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $topologyCopyScanText,
            '\bResult\s*(?::=|[+\-*/]=)').Count -ne 3 -or
        [regex]::Matches(
            $topologyCopyScanText,
            '(?i)\bRETURN\s*;').Count -ne 2 -or
        [regex]::Matches(
            $topologyCopyScanText,
            ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
             '(?:dest|ptr1)\s*:=\s*' +
             '(?:pDest\b|\(\s*pDest\s*\+\s*[^\)]+\))')).Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyScanText,
            '(?i)\bpDest\b').Count -ne 3 -or
        [regex]::Matches(
            $topologyCopyEvenArm,
            ('(?is)_memcpy\s*\(\s*ptr1\s*:=\s*pDest\s*,\s*' +
             'ptr2\s*:=\s*#SnapshotBytes\s*\[\s*0\s*\]\s*,\s*' +
             'cntr\s*:=\s*464\s*\)')).Count -ne 1 -or
        [regex]::Matches(
            $topologyCopyStableArm,
            'Result\s*:=\s*0;\s*RETURN;').Count -ne 1 -or
        $topologyCopyScanText -match
            ('(?i)(?:\bpDest\b|' +
             '\(\s*pDest\s*\+\s*[^\)]+\))\s*\^\s*\$\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)') -or
        $topologyCopyScanText -match
            '(?i)\bpDest\b\s*(?::=|[+\-*/]=)' -or
        $topologyCopyScanText -match
            ('(?is)(?:_memset|_memcpy)\s*\(\s*(?:dest|ptr1)\s*:=\s*' +
             '#?SnapshotBytes\s*\[[^\]]+\]|' +
             '#?SnapshotBytes\s*(?::=|[+\-*/]=)|' +
             'SnapshotBytes\s*\[[^\]]+\]\s*\$\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')) {
        throw ('LMCEcatInputLatch.CopyTopologyIoSnapshot must use one immutable ' +
            '464-byte copy dominated by nonzero stable seqlock reads, with no ' +
            'local/result/retry, destination, or source-buffer overwrite.')
    }
    Assert-Match $diagnosticsLatch (
        '(?s)FUNCTION GLOBAL AdvanceOutputRevision.*?' +
        'Revision\s*:\s*UDINT;') (
        'LMCEcatInputLatch.AdvanceOutputRevision IDE declaration is missing.')
    $advanceOutputRevisionBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::AdvanceOutputRevision.*?' +
            'END_FUNCTION').Value
    if ([string]::IsNullOrWhiteSpace($advanceOutputRevisionBlock)) {
        throw 'LMCEcatInputLatch.AdvanceOutputRevision implementation was not found.'
    }
    Assert-Match $advanceOutputRevisionBlock (
        '(?s)\AFUNCTION GLOBAL LMCEcatInputLatch::AdvanceOutputRevision\s*' +
        'VAR_OUTPUT\s*Revision\s*:\s*UDINT;\s*END_VAR.*?' +
        'OutputRevision \+= 1;.*?' +
        'if OutputRevision = 0 then\s*OutputRevision := 1;.*?' +
        'Revision := OutputRevision;') (
        'LMCEcatInputLatch output revision nonzero wrap helper is incomplete.')
    $advanceOutputRevisionScanText =
        Get-LasalScanText $advanceOutputRevisionBlock
    if ([regex]::Matches(
            $advanceOutputRevisionScanText,
            '\bOutputRevision\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $advanceOutputRevisionScanText,
            'OutputRevision\s*\+=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $advanceOutputRevisionScanText,
            'OutputRevision\s*:=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $advanceOutputRevisionScanText,
            '\bRevision\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        $advanceOutputRevisionScanText -notmatch
            '(?is)Revision\s*:=\s*OutputRevision;\s*END_FUNCTION\s*$') {
        throw ('LMCEcatInputLatch.AdvanceOutputRevision must contain only one ' +
            'increment, one zero-wrap correction, and final Revision output.')
    }

    $readRtLocalTypes = [ordered]@{
        'cycleCounter' = 'UDINT'
        'writeSequence' = 'UDINT'
        'finalSequence' = 'UDINT'
        'couplerConnected' = 'BOOL'
        'inputSlotConnected' = 'BOOL'
        'outputSlotConnected' = 'BOOL'
        'couplerNativeOnline' = 'DINT'
        'couplerEtherCATState' = 'UDINT'
        'couplerSlaveState' = 'UDINT'
        'couplerALStatus' = 'UDINT'
        'couplerClassState' = 'UDINT'
        'inputSlotClassState' = 'UDINT'
        'outputSlotClassState' = 'UDINT'
        'inputByte0Value' = 'UDINT'
        'inputByte1Value' = 'UDINT'
        'inputByte2Value' = 'UDINT'
        'inputByte3Value' = 'UDINT'
        'outputByte0Value' = 'UDINT'
        'outputByte1Value' = 'UDINT'
        'outputByte2Value' = 'UDINT'
        'outputByte3Value' = 'UDINT'
        'inputValue' = 'UDINT'
        'outputValue' = 'UDINT'
        'inputValidMask' = 'UDINT'
        'outputValidMask' = 'UDINT'
        'inputStatus' = 'UINT'
        'outputStatus' = 'UINT'
        'couplerDetected' = 'BOOL'
        'couplerIdentityMatched' = 'BOOL'
        'couplerDataValid' = 'BOOL'
        'inputDetected' = 'BOOL'
        'inputIdentityMatched' = 'BOOL'
        'inputDataValid' = 'BOOL'
        'outputDetected' = 'BOOL'
        'outputIdentityMatched' = 'BOOL'
        'outputDataValid' = 'BOOL'
        'inputValid' = 'BOOL'
        'outputValid' = 'BOOL'
        'couplerStateChanged' = 'BOOL'
        'inputStateChanged' = 'BOOL'
        'outputStateChanged' = 'BOOL'
        'couplerLastValidCycle' = 'UDINT'
        'couplerLastStateChangeCycle' = 'UDINT'
        'inputLastValidCycle' = 'UDINT'
        'inputLastStateChangeCycle' = 'UDINT'
        'outputLastValidCycle' = 'UDINT'
        'outputLastStateChangeCycle' = 'UDINT'
    }
    foreach ($readRtLocal in $readRtLocalTypes.GetEnumerator()) {
        Assert-LasalExactDeclaredType `
            -Text $diagnosticsLatchRtWorkBlock `
            -Name $readRtLocal.Key `
            -ExpectedType $readRtLocal.Value `
            -Owner ('LMCEcatInputLatch.RtWork local ' + $readRtLocal.Key)
    }

    $connectionCapturePatterns = [ordered]@{
        'coupler connection capture' =
            'couplerConnected := IsClientConnected\(#Coupler\) <> 0;'
        'input-slot connection capture' =
            'inputSlotConnected := IsClientConnected\(#InputSlot\) <> 0;'
        'output-slot connection capture' =
            'outputSlotConnected := IsClientConnected\(#OutputSlot\) <> 0;'
    }
    foreach ($connectionCapture in $connectionCapturePatterns.GetEnumerator()) {
        Assert-Match $diagnosticsLatchRtWorkBlock $connectionCapture.Value (
            'LMCEcatInputLatch must capture the ' + $connectionCapture.Key +
            ' exactly once before guarded source access.')
    }
    foreach ($sourceClient in @('Coupler', 'InputSlot', 'OutputSlot')) {
        if ([regex]::Matches(
                $diagnosticsLatchRtWorkBlock,
                'IsClientConnected\(#' + $sourceClient + '\)').Count -ne 1) {
            throw ("LMCEcatInputLatch.RtWork must sample $sourceClient " +
                'connection state exactly once per cycle.')
        }
    }
    foreach ($couplerRead in @(
            'Online',
            'EtherCATState',
            'SlaveState',
            'AL_StatusCode',
            'ClassState')) {
        if ([regex]::Matches(
                $diagnosticsLatchRtWorkBlock,
                'Coupler\.' + $couplerRead + '\.Read\(\)').Count -ne 1) {
            throw ("LMCEcatInputLatch.RtWork must read Coupler.$couplerRead exactly once.")
        }
    }
    $couplerGuardPattern = (
        '(?s)if couplerConnected then\s*' +
        'couplerNativeOnline := Coupler\.Online\.Read\(\);\s*' +
        'couplerEtherCATState := TO_UDINT\(Coupler\.EtherCATState\.Read\(\)\);\s*' +
        'couplerSlaveState := Coupler\.SlaveState\.Read\(\);\s*' +
        'couplerALStatus := TO_UDINT\(Coupler\.AL_StatusCode\.Read\(\)\);\s*' +
        'couplerClassState := TO_UDINT\(Coupler\.ClassState\.Read\(\)\);\s*' +
        'else\s*couplerNativeOnline := 0;\s*' +
        'couplerEtherCATState := 0;\s*couplerSlaveState := 0;\s*' +
        'couplerALStatus := 0;\s*couplerClassState := 0xFFFFFFFF;\s*end_if;')
    Assert-Match $diagnosticsLatchRtWorkBlock $couplerGuardPattern (
        'LMCEcatInputLatch must guard every coupler read and publish the ' +
        'source-unavailable sentinel instead of stale values when disconnected.')
    foreach ($ioByteIndex in 0..3) {
        foreach ($ioByteContract in @(
                @{ Prefix = 'input'; Client = 'InputSlot'; Channel = 'InputS_Byte' },
                @{ Prefix = 'output'; Client = 'OutputSlot'; Channel = 'OutputS_Byte' })) {
            $ioReadPattern = (
                $ioByteContract.Prefix + 'Byte' + $ioByteIndex +
                'Value\s*:=\s*TO_UDINT\(' +
                $ioByteContract.Client + '\.' +
                $ioByteContract.Channel + $ioByteIndex +
                '\.Read\(\)\)\s*and\s*0x000000FF')
            if ([regex]::Matches(
                    $diagnosticsLatchRtWorkBlock,
                    $ioReadPattern).Count -ne 1) {
                throw (
                    "LMCEcatInputLatch.RtWork must read and mask " +
                    "$($ioByteContract.Client).$($ioByteContract.Channel)$ioByteIndex " +
                    'exactly once into its canonical byte variable.')
            }
        }
    }
    foreach ($packedIoValue in @('input', 'output')) {
        Assert-Match $diagnosticsLatchRtWorkBlock (
            $packedIoValue + 'Value\s*:=\s*' +
            $packedIoValue + 'Byte0Value\s+or\s+' +
            '\(' + $packedIoValue + 'Byte1Value shl 8\)\s+or\s+' +
            '\(' + $packedIoValue + 'Byte2Value shl 16\)\s+or\s+' +
            '\(' + $packedIoValue + 'Byte3Value shl 24\)') (
            "LMCEcatInputLatch.RtWork $packedIoValue Byte0-LSB pack is incomplete.")
    }
    $slotGuardPatterns = @{}
    foreach ($slotGuard in @(
            @{ Prefix = 'input'; Connected = 'inputSlotConnected'; Client = 'InputSlot'; Channel = 'InputS_Byte' },
            @{ Prefix = 'output'; Connected = 'outputSlotConnected'; Client = 'OutputSlot'; Channel = 'OutputS_Byte' })) {
        $slotGuardPattern = (
            '(?s)if ' + $slotGuard.Connected + ' then\s*' +
            $slotGuard.Prefix + 'SlotClassState := TO_UDINT\(' +
            $slotGuard.Client + '\.ClassState\.Read\(\)\);\s*' +
            $slotGuard.Prefix + 'Byte0Value := TO_UDINT\(' +
            $slotGuard.Client + '\.' + $slotGuard.Channel + '0\.Read\(\)\)' +
            '\s*and\s*0x000000FF;\s*' +
            $slotGuard.Prefix + 'Byte1Value := TO_UDINT\(' +
            $slotGuard.Client + '\.' + $slotGuard.Channel + '1\.Read\(\)\)' +
            '\s*and\s*0x000000FF;\s*' +
            $slotGuard.Prefix + 'Byte2Value := TO_UDINT\(' +
            $slotGuard.Client + '\.' + $slotGuard.Channel + '2\.Read\(\)\)' +
            '\s*and\s*0x000000FF;\s*' +
            $slotGuard.Prefix + 'Byte3Value := TO_UDINT\(' +
            $slotGuard.Client + '\.' + $slotGuard.Channel + '3\.Read\(\)\)' +
            '\s*and\s*0x000000FF;\s*' +
            'else\s*' + $slotGuard.Prefix +
            'SlotClassState := 0xFFFFFFFF;\s*' +
            $slotGuard.Prefix + 'Byte0Value := 0;\s*' +
            $slotGuard.Prefix + 'Byte1Value := 0;\s*' +
            $slotGuard.Prefix + 'Byte2Value := 0;\s*' +
            $slotGuard.Prefix + 'Byte3Value := 0;\s*end_if;')
        $slotGuardPatterns[$slotGuard.Prefix] = $slotGuardPattern
        Assert-Match $diagnosticsLatchRtWorkBlock $slotGuardPattern (
            "LMCEcatInputLatch must guard every $($slotGuard.Client) read and " +
            'zero the disconnected source instead of retaining a stale PDO image.')
    }
    $crevisPresencePattern = (
        '(?s)couplerDetected := couplerConnected &\s*' +
        '\(couplerClassState <> _NoHardware\) &\s*' +
        '\(couplerClassState <> 0xFFFFFFFF\) &\s*' +
        '\(couplerEtherCATState <> 0\);\s*' +
        'if couplerDetected = FALSE then\s*' +
        'couplerNativeOnline := 0;\s*couplerEtherCATState := 0;\s*' +
        'couplerSlaveState := 0;\s*couplerALStatus := 0;\s*end_if;\s*' +
        'couplerIdentityMatched := couplerDetected &\s*' +
        '\(couplerClassState = _ClassOk\) &\s*' +
        '\(\(couplerSlaveState and 0x00000020\) = 0\);\s*' +
        'couplerDataValid := couplerIdentityMatched &\s*' +
        '\(masterState = 8\) &\s*\(consecutiveInvalidCycles = 0\) &\s*' +
        '\(couplerNativeOnline <> 0\) &\s*' +
        '\(couplerEtherCATState = 8\) &\s*\(couplerALStatus = 0\);\s*' +
        'inputDetected := couplerDetected & inputSlotConnected &\s*' +
        '\(inputSlotClassState <> _NoHardware\) &\s*' +
        '\(inputSlotClassState <> 0xFFFFFFFF\);\s*' +
        'inputIdentityMatched := inputDetected & couplerIdentityMatched &\s*' +
        '\(inputSlotClassState = _ClassOk\);\s*' +
        'outputDetected := couplerDetected & outputSlotConnected &\s*' +
        '\(outputSlotClassState <> _NoHardware\) &\s*' +
        '\(outputSlotClassState <> 0xFFFFFFFF\);\s*' +
        'outputIdentityMatched := outputDetected & couplerIdentityMatched &\s*' +
        '\(outputSlotClassState = _ClassOk\);')
    Assert-Match $diagnosticsLatchRtWorkBlock $crevisPresencePattern (
        'LMCEcatInputLatch CREVIS presence and identity must require the parent ' +
        'coupler source/physical state as well as each slot ClassState.')

    $ioQualityPatterns = @{}
    foreach ($ioQuality in @(
            @{ Prefix = 'input'; Connected = 'inputSlotConnected' },
            @{ Prefix = 'output'; Connected = 'outputSlotConnected' })) {
        $ioQualityPattern = (
            '(?s)' + $ioQuality.Prefix + 'Status := 0;\s*' +
            'if consecutiveInvalidCycles <> 0 then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0002;\s*end_if;\s*' +
            'if masterState <> 8 then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0004;\s*end_if;\s*' +
            'if ' + $ioQuality.Prefix + 'Detected = FALSE then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0008;\s*' +
            'elsif \(couplerEtherCATState <> 8\) \|\s*' +
            '\(couplerNativeOnline = 0\) then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0010;\s*end_if;\s*' +
            'if couplerALStatus <> 0 then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0020;\s*end_if;\s*' +
            'if \(couplerConnected = FALSE\) \|\s*' +
            '\(' + $ioQuality.Connected + ' = FALSE\) then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0040;\s*end_if;\s*' +
            'if ' + $ioQuality.Prefix + 'IdentityMatched = FALSE then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0080;\s*end_if;\s*' +
            $ioQuality.Prefix + 'DataValid := ' + $ioQuality.Prefix +
            'Status = 0;\s*' +
            $ioQuality.Prefix + 'Valid := ' + $ioQuality.Prefix +
            'DataValid;\s*' +
            'if ' + $ioQuality.Prefix + 'Valid = FALSE then\s*' +
            $ioQuality.Prefix + 'Status := ' + $ioQuality.Prefix +
            'Status or 0x0100;\s*' +
            $ioQuality.Prefix + 'Value := 0;\s*' +
            $ioQuality.Prefix + 'ValidMask := 0;\s*' +
            'else\s*' + $ioQuality.Prefix + 'Status := 1;\s*' +
            $ioQuality.Prefix + 'ValidMask := 0xFFFFFFFF;\s*end_if;')
        $ioQualityPatterns[$ioQuality.Prefix] = $ioQualityPattern
        Assert-Match $diagnosticsLatchRtWorkBlock $ioQualityPattern (
            "LMCEcatInputLatch $($ioQuality.Prefix) exact stale/master/node/AL/" +
            'source/identity quality, defaulting, and full-mask contract is incomplete.')
    }

    $healthBookkeepingPattern = (
        '(?s)couplerStateChanged :=\s*' +
        '\(SnapshotBytes\[304\]\$DINT <> couplerNativeOnline\) \|\s*' +
        '\(SnapshotBytes\[308\]\$UDINT <> couplerEtherCATState\) \|\s*' +
        '\(SnapshotBytes\[312\]\$UDINT <> couplerSlaveState\) \|\s*' +
        '\(SnapshotBytes\[316\]\$UDINT <> couplerALStatus\) \|\s*' +
        '\(SnapshotBytes\[320\]\$UDINT <> couplerClassState\);\s*' +
        'inputStateChanged :=\s*' +
        '\(SnapshotBytes\[340\]\$DINT <> couplerNativeOnline\) \|\s*' +
        '\(SnapshotBytes\[344\]\$UDINT <> couplerEtherCATState\) \|\s*' +
        '\(SnapshotBytes\[348\]\$UDINT <> couplerSlaveState\) \|\s*' +
        '\(SnapshotBytes\[352\]\$UDINT <> couplerALStatus\) \|\s*' +
        '\(SnapshotBytes\[356\]\$UDINT <> inputSlotClassState\);\s*' +
        'outputStateChanged :=\s*' +
        '\(SnapshotBytes\[376\]\$DINT <> couplerNativeOnline\) \|\s*' +
        '\(SnapshotBytes\[380\]\$UDINT <> couplerEtherCATState\) \|\s*' +
        '\(SnapshotBytes\[384\]\$UDINT <> couplerSlaveState\) \|\s*' +
        '\(SnapshotBytes\[388\]\$UDINT <> couplerALStatus\) \|\s*' +
        '\(SnapshotBytes\[392\]\$UDINT <> outputSlotClassState\);\s*' +
        'couplerLastValidCycle := SnapshotBytes\[332\]\$UDINT;\s*' +
        'couplerLastStateChangeCycle := SnapshotBytes\[336\]\$UDINT;\s*' +
        'inputLastValidCycle := SnapshotBytes\[368\]\$UDINT;\s*' +
        'inputLastStateChangeCycle := SnapshotBytes\[372\]\$UDINT;\s*' +
        'outputLastValidCycle := SnapshotBytes\[404\]\$UDINT;\s*' +
        'outputLastStateChangeCycle := SnapshotBytes\[408\]\$UDINT;\s*' +
        'if couplerDataValid then\s*couplerLastValidCycle := cycleCounter;\s*end_if;\s*' +
        'if inputDataValid then\s*inputLastValidCycle := cycleCounter;\s*end_if;\s*' +
        'if outputDataValid then\s*outputLastValidCycle := cycleCounter;\s*end_if;\s*' +
        'if couplerStateChanged then\s*couplerLastStateChangeCycle := cycleCounter;\s*end_if;\s*' +
        'if inputStateChanged then\s*inputLastStateChangeCycle := cycleCounter;\s*end_if;\s*' +
        'if outputStateChanged then\s*outputLastStateChangeCycle := cycleCounter;\s*end_if;')
    Assert-Match $diagnosticsLatchRtWorkBlock $healthBookkeepingPattern (
        'LMCEcatInputLatch last-valid and state-change cycles are not derived ' +
        'from the exact current health records before publication.')
    $outputObservationPattern = (
        '(?s)if OutputRevision = 0 then\s*' +
        'OutputRevision := 1;\s*end_if;\s*' +
        'if OutputObserved = FALSE then\s*' +
        'OutputObserved := TRUE;\s*' +
        'OutputPreviousValid := outputValid;\s*' +
        'OutputPreviousValue := outputValue;\s*' +
        'elsif \(OutputPreviousValid <> outputValid\) \|\s*' +
        '\(OutputPreviousValue <> outputValue\) then\s*' +
        'OutputPreviousValid := outputValid;\s*' +
        'OutputPreviousValue := outputValue;\s*' +
        'AdvanceOutputRevision\(\);\s*end_if;')
    Assert-Match $diagnosticsLatchRtWorkBlock $outputObservationPattern (
        'LMCEcatInputLatch output revision initialization and observed ' +
        'validity/value transition contract is incomplete.')
    $readRtScanText = Get-LasalScanText $diagnosticsLatchRtWorkBlock
    $diagnosticsLatchScanText = Get-LasalScanText $diagnosticsLatch
    Assert-LasalAddressNamesAllowed `
        -Text $diagnosticsLatch `
        -AllowedNames @(
            'pragma',
            'define',
            'vmt',
            'RtWork',
            'EcatMaster',
            'Drive1',
            'Drive2',
            'Drive3',
            'Drive4',
            'RecorderStore',
            'Coupler',
            'InputSlot',
            'OutputSlot',
            'PublishSequence',
            'SnapshotBytes',
            'OutputMailboxState',
            'OutputRequestBytes',
            'OutputCompletionSequence',
            'OutputCompletionBytes') `
        -Owner 'LMCEcatInputLatch class implementation'
    foreach ($latchClientAddressName in @(
            'EcatMaster',
            'Drive1',
            'Drive2',
            'Drive3',
            'Drive4',
            'RecorderStore',
            'Coupler',
            'InputSlot',
            'OutputSlot')) {
        if ([regex]::Matches(
                $diagnosticsLatchScanText,
                '(?i)#\s*' + $latchClientAddressName + '\b').Count -ne 1) {
            throw ('LMCEcatInputLatch #' + $latchClientAddressName +
                ' address use must occur exactly once at its canonical ' +
                'connection/read owner; client aliases are forbidden.')
        }
    }
    $readRtExactMutationCounts = [ordered]@{
        'couplerConnected' = 1
        'inputSlotConnected' = 1
        'outputSlotConnected' = 1
        'couplerNativeOnline' = 3
        'couplerEtherCATState' = 3
        'couplerSlaveState' = 3
        'couplerALStatus' = 3
        'couplerClassState' = 2
        'inputSlotClassState' = 2
        'outputSlotClassState' = 2
        'inputByte0Value' = 2
        'inputByte1Value' = 2
        'inputByte2Value' = 2
        'inputByte3Value' = 2
        'outputByte0Value' = 2
        'outputByte1Value' = 2
        'outputByte2Value' = 2
        'outputByte3Value' = 2
        'inputValue' = 2
        'inputValidMask' = 2
        'outputValidMask' = 2
        'couplerDetected' = 1
        'couplerIdentityMatched' = 1
        'couplerDataValid' = 1
        'inputDetected' = 1
        'inputIdentityMatched' = 1
        'inputDataValid' = 1
        'inputValid' = 1
        'outputDetected' = 1
        'outputIdentityMatched' = 1
        'outputDataValid' = 1
        'outputValid' = 1
        'couplerStateChanged' = 1
        'inputStateChanged' = 1
        'outputStateChanged' = 1
        'couplerLastValidCycle' = 2
        'couplerLastStateChangeCycle' = 2
        'inputLastValidCycle' = 2
        'inputLastStateChangeCycle' = 2
        'outputLastValidCycle' = 2
        'outputLastStateChangeCycle' = 2
        'OutputObserved' = 1
        'OutputPreviousValid' = 2
        'OutputPreviousValue' = 2
    }
    foreach ($readRtMutation in $readRtExactMutationCounts.GetEnumerator()) {
        if ([regex]::Matches(
                $readRtScanText,
                '\b' + $readRtMutation.Key +
                    '\s*(?::=|[+\-*/]=)').Count -ne
                $readRtMutation.Value) {
            throw ('LMCEcatInputLatch.RtWork ' + $readRtMutation.Key +
                ' mutation count must remain exactly ' +
                $readRtMutation.Value +
                ' so source, quality, and revision observation cannot be overwritten.')
        }
    }
    foreach ($sharedObservationMutation in @(
            @{ Name = 'OutputObserved'; Count = 1 },
            @{ Name = 'OutputPreviousValid'; Count = 2 },
            @{ Name = 'OutputPreviousValue'; Count = 2 })) {
        if ([regex]::Matches(
                $diagnosticsLatchScanText,
                '\b' + $sharedObservationMutation.Name +
                    '\s*(?::=|[+\-*/]=)').Count -ne
                $sharedObservationMutation.Count) {
            throw ('LMCEcatInputLatch shared observation member ' +
                $sharedObservationMutation.Name +
                ' may be mutated only by its canonical RtWork observation branch.')
        }
    }
    foreach ($connectionCapture in $connectionCapturePatterns.GetEnumerator()) {
        if ([regex]::Matches(
                $readRtScanText,
                $connectionCapture.Value).Count -ne 1) {
            throw ('LMCEcatInputLatch ' + $connectionCapture.Key +
                ' must be the sole assignment of its connection BOOL.')
        }
    }
    foreach ($inputStatusAssignment in @(
            'inputStatus\s*:=\s*0;',
            'inputStatus\s*:=\s*inputStatus or 0x0002;',
            'inputStatus\s*:=\s*inputStatus or 0x0004;',
            'inputStatus\s*:=\s*inputStatus or 0x0008;',
            'inputStatus\s*:=\s*inputStatus or 0x0010;',
            'inputStatus\s*:=\s*inputStatus or 0x0020;',
            'inputStatus\s*:=\s*inputStatus or 0x0040;',
            'inputStatus\s*:=\s*inputStatus or 0x0080;',
            'inputStatus\s*:=\s*inputStatus or 0x0100;',
            'inputStatus\s*:=\s*1;')) {
        if ([regex]::Matches(
                $readRtScanText,
                $inputStatusAssignment).Count -ne 1) {
            throw ('LMCEcatInputLatch inputStatus canonical quality assignment ' +
                "'$inputStatusAssignment' must occur exactly once.")
        }
    }
    if ([regex]::Matches(
            $readRtScanText,
            '\binputStatus\s*(?::=|[+\-*/]=)').Count -ne 10) {
        throw ('LMCEcatInputLatch inputStatus may mutate only through the ten ' +
            'canonical fail-closed quality assignments.')
    }
    $outputStatusAssignments = @(
        'outputStatus\s*:=\s*0;',
        'outputStatus\s*:=\s*outputStatus or 0x0002;',
        'outputStatus\s*:=\s*outputStatus or 0x0004;',
        'outputStatus\s*:=\s*outputStatus or 0x0008;',
        'outputStatus\s*:=\s*outputStatus or 0x0010;',
        'outputStatus\s*:=\s*outputStatus or 0x0020;',
        'outputStatus\s*:=\s*outputStatus or 0x0040;',
        'outputStatus\s*:=\s*outputStatus or 0x0080;',
        'outputStatus\s*:=\s*outputStatus or 0x0100;',
        'outputStatus\s*:=\s*1;')
    if ([regex]::Matches(
            $readRtScanText,
            '\boutputStatus\s*(?::=|[+\-*/]=)').Count -ne
            $outputStatusAssignments.Count) {
        throw ('LMCEcatInputLatch outputStatus mutation count does not match ' +
            'the canonical fail-closed quality derivation.')
    }
    foreach ($outputStatusAssignment in $outputStatusAssignments) {
        if ([regex]::Matches(
                $readRtScanText,
                $outputStatusAssignment).Count -ne 1) {
            throw ('LMCEcatInputLatch outputStatus canonical assignment ' +
                "'$outputStatusAssignment' must occur exactly once.")
        }
    }
    if ([regex]::Matches(
            $readRtScanText,
            '\boutputDataValid\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $readRtScanText,
            'outputDataValid\s*:=\s*outputStatus\s*=\s*0;').Count -ne 1 -or
        [regex]::Matches(
            $readRtScanText,
            '\boutputValid\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $readRtScanText,
            'outputValid\s*:=\s*outputDataValid;').Count -ne 1) {
        throw ('LMCEcatInputLatch output validity must be assigned exactly once ' +
            'from the canonical sticky outputStatus quality result.')
    }
    $expectedOutputValueMutationCount = if ($topologyIoOutputIntegrated) { 3 } else { 2 }
    if ([regex]::Matches(
            $readRtScanText,
            '\boutputValue\s*(?::=|[+\-*/]=)').Count -ne
            $expectedOutputValueMutationCount -or
        [regex]::Matches(
            $readRtScanText,
            ('outputValue\s*:=\s*outputByte0Value\s+or\s+' +
             '\(outputByte1Value shl 8\)\s+or\s+' +
             '\(outputByte2Value shl 16\)\s+or\s+' +
             '\(outputByte3Value shl 24\)\s*;')).Count -ne 1 -or
        [regex]::Matches(
            $readRtScanText,
            'outputValue\s*:=\s*0;').Count -ne 1 -or
        ($topologyIoOutputIntegrated -and
            [regex]::Matches(
                $readRtScanText,
                'outputValue\s*:=\s*newOutputValue;').Count -ne 1)) {
        throw ('LMCEcatInputLatch outputValue must contain only one raw byte ' +
            'pack, one invalid-quality default, and optional successful apply shadow.')
    }
    $expectedAdvanceOutputRevisionCalls =
        if ($topologyIoOutputIntegrated) { 2 } else { 1 }
    $advanceOutputRevisionCallPattern =
        '(?i)\bAdvanceOutputRevision\s*\(\s*\)'
    if ([regex]::Matches(
            $readRtScanText,
            '\bOutputRevision\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $readRtScanText,
            'OutputRevision\s*:=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '\bOutputRevision\s*(?::=|[+\-*/]=)').Count -ne 3 -or
        [regex]::Matches(
            $readRtScanText,
            $advanceOutputRevisionCallPattern).Count -ne
            $expectedAdvanceOutputRevisionCalls -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $advanceOutputRevisionCallPattern).Count -ne
            $expectedAdvanceOutputRevisionCalls) {
        throw ('LMCEcatInputLatch OutputRevision may change only through one ' +
            'RT zero initialization and RT-owned canonical revision-helper calls.')
    }

    $writerOpenPattern =
        'sigclib_atomic_setU32\(pValue:=#PublishSequence,\s*value:=writeSequence\)'
    $writerClosePattern =
        'sigclib_atomic_setU32\(pValue:=#PublishSequence,\s*value:=finalSequence\)'
    $writerOpenMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        $writerOpenPattern)
    $writerCloseMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        $writerClosePattern)
    $writerOpenMatch = if ($writerOpenMatches.Count -eq 1) {
        $writerOpenMatches[0]
    }
    else {
        $null
    }
    $writerCloseMatch = if ($writerCloseMatches.Count -eq 1) {
        $writerCloseMatches[0]
    }
    else {
        $null
    }
    if ($null -eq $writerOpenMatch -or $null -eq $writerCloseMatch -or
        $writerOpenMatch.Index -ge $writerCloseMatch.Index) {
        throw ('LMCEcatInputLatch extended snapshot writer does not bracket all ' +
            'writes with exactly one odd/even PublishSequence interval.')
    }
    if ((Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $writerOpenMatch.Index) -ne 0 -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $writerCloseMatch.Index) -ne 0) {
        throw ('LMCEcatInputLatch PublishSequence open and close must be ' +
            'unconditional top-level RtWork statements.')
    }
    Assert-Match $diagnosticsLatchRtWorkBlock (
        '(?s)writeSequence := sigclib_atomic_getU32\(' +
        'pValue:=#PublishSequence\) \+ 1;\s*' +
        'if \(writeSequence and 1\) = 0 then\s*' +
        'writeSequence \+= 1;\s*end_if;\s*' +
        $writerOpenPattern + '.*?' +
        'finalSequence := writeSequence \+ 1;\s*' +
        'if finalSequence = 0 then\s*' +
        'finalSequence := 2;\s*end_if;\s*' +
        'SnapshotBytes\[44\]\$UDINT := finalSequence;\s*' +
        $writerClosePattern) (
        'LMCEcatInputLatch RtWork must derive a nonzero odd writer sequence and ' +
        'publish its next nonzero even sequence at offset 44 before closing.')
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'writeSequence\s*:=\s*sigclib_atomic_getU32\(').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'finalSequence\s*:=\s*writeSequence\s*\+\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'SnapshotBytes\[44\]\$UDINT\s*:=\s*finalSequence;').Count -ne 1) {
        throw ('LMCEcatInputLatch RtWork must calculate and publish one canonical ' +
            'odd/even snapshot sequence pair per cycle.')
    }
    $snapshotWriterScanText = Get-LasalScanText $diagnosticsLatchRtWorkBlock
    if ([regex]::Matches(
            $snapshotWriterScanText,
            '\bwriteSequence\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            ('writeSequence\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#PublishSequence\)\s*\+\s*1;')).Count -ne 1 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            'writeSequence\s*\+=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            '\bfinalSequence\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            'finalSequence\s*:=\s*writeSequence\s*\+\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            'finalSequence\s*:=\s*2;').Count -ne 1 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#PublishSequence')).Count -ne 2 -or
        $snapshotWriterScanText -match
            '(?i)#?PublishSequence\s*(?::=|[+\-*/]=)' -or
        [regex]::Matches(
            $snapshotWriterScanText,
            ('SnapshotBytes\[44\]\$[A-Za-z_][A-Za-z0-9_]*' +
             '\s*(?::=|[+\-*/]=)')).Count -ne 1) {
        throw ('LMCEcatInputLatch RtWork writer sequence may mutate only through ' +
            'the canonical current+1/odd correction and next/even-zero correction.')
    }
    $writerPreCloseSection = $diagnosticsLatchRtWorkBlock.Substring(
        0,
        $writerCloseMatch.Index + $writerCloseMatch.Length)
    if ((Get-LasalScanText $writerPreCloseSection) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch RtWork must not RETURN before the unconditional ' +
            'even PublishSequence close on any cycle.')
    }
    $snapshotTypedMutationPattern =
        ('(?i)(?<![A-Za-z0-9_])#?\s*SnapshotBytes\s*' +
         '\[\s*[^\]]+\]\s*\$\s*' +
         '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
    $snapshotAggregateMutationPattern =
        ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
         '(?:dest|ptr1)\s*:=\s*#?SnapshotBytes\s*\[[^\]]+\]')
    $classSnapshotTypedMutations = [regex]::Matches(
        $diagnosticsLatchScanText,
        $snapshotTypedMutationPattern)
    $rtSnapshotTypedMutations = [regex]::Matches(
        $snapshotWriterScanText,
        $snapshotTypedMutationPattern)
    $classSnapshotAggregateMutations = [regex]::Matches(
        $diagnosticsLatchScanText,
        $snapshotAggregateMutationPattern)
    $rtSnapshotAggregateMutations = [regex]::Matches(
        $snapshotWriterScanText,
        $snapshotAggregateMutationPattern)
    if ($classSnapshotTypedMutations.Count -ne
            $rtSnapshotTypedMutations.Count -or
        $classSnapshotAggregateMutations.Count -ne
            $rtSnapshotAggregateMutations.Count -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*SnapshotBytes\b').Count -ne 7 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            ('(?is)RecorderStore\s*\.\s*AppendSnapshot\s*\(\s*' +
             'pSnapshot\s*:=\s*#\s*SnapshotBytes\s*\[\s*0\s*\]\s*,\s*' +
             'SnapshotSize\s*:=\s*304\s*\)')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            ('(?is)_memcpy\s*\(\s*ptr1\s*:=\s*pDest\s*,\s*' +
             'ptr2\s*:=\s*#\s*SnapshotBytes\s*\[\s*0\s*\]\s*,\s*' +
             'cntr\s*:=\s*304\s*\)')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            ('(?is)_memcpy\s*\(\s*ptr1\s*:=\s*pDest\s*,\s*' +
             'ptr2\s*:=\s*#\s*SnapshotBytes\s*\[\s*0\s*\]\s*,\s*' +
             'cntr\s*:=\s*464\s*\)')).Count -ne 1 -or
        $diagnosticsLatchScanText -match
            '(?i)#?SnapshotBytes\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch SnapshotBytes may be mutated only by RtWork; ' +
            'reader/helper and aggregate-array ownership must remain immutable.')
    }
    foreach ($rtSnapshotMutation in @($rtSnapshotTypedMutations) +
            @($rtSnapshotAggregateMutations)) {
        if ($rtSnapshotMutation.Index -le
                ($writerOpenMatch.Index + $writerOpenMatch.Length) -or
            ($rtSnapshotMutation.Index + $rtSnapshotMutation.Length) -ge
                $writerCloseMatch.Index) {
            throw ('Every LMCEcatInputLatch SnapshotBytes mutation must occur ' +
                'strictly inside the single odd/even PublishSequence interval.')
        }
    }
    $publishSequenceSetPattern =
        ('(?is)sigclib_atomic_setU32\(\s*' +
         'pValue:=#PublishSequence')
    $publishSequenceAtomicMutationPattern =
        ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*PublishSequence\b')
    $publishSequenceAtomicAccessPattern =
        ('(?is)sigclib_atomic_(?:get|cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*PublishSequence\b')
    if ([regex]::Matches(
            $diagnosticsLatchScanText,
            $publishSequenceAtomicMutationPattern).Count -ne 2 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            $publishSequenceAtomicMutationPattern).Count -ne 2 -or
        [regex]::Matches(
            $snapshotWriterScanText,
            $publishSequenceSetPattern).Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*PublishSequence\b').Count -ne
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $publishSequenceAtomicAccessPattern).Count -or
        $diagnosticsLatchScanText -match
            '(?i)#?PublishSequence\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch PublishSequence may be mutated only by the ' +
            'two canonical RtWork atomic writer open/close calls.')
    }
    $extendedSnapshotOffsets = @(
        304, 308, 312, 316, 320, 324, 328, 332, 336,
        340, 344, 348, 352, 356, 360, 364, 368, 372,
        376, 380, 384, 388, 392, 396, 400, 404, 408,
        412, 416, 420, 424, 428, 430, 432,
        436, 440, 444, 448, 452, 454, 456, 460)
    $extendedWriteMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        'SnapshotBytes\[(?:' + ($extendedSnapshotOffsets -join '|') +
            ')\]\$[A-Za-z_][A-Za-z0-9_]*\s*:=')
    if ($extendedWriteMatches.Count -ne $extendedSnapshotOffsets.Count) {
        throw ('LMCEcatInputLatch must assign every extended snapshot offset ' +
            'exactly once with no differently typed or duplicate write.')
    }
    $firstExtendedWriteMatch = $extendedWriteMatches |
        Sort-Object Index |
        Select-Object -First 1
    $inputPackPattern = (
        'inputValue\s*:=\s*inputByte0Value\s+or\s+' +
        '\(inputByte1Value shl 8\)\s+or\s+' +
        '\(inputByte2Value shl 16\)\s+or\s+' +
        '\(inputByte3Value shl 24\)')
    $outputPackPattern = (
        'outputValue\s*:=\s*outputByte0Value\s+or\s+' +
        '\(outputByte1Value shl 8\)\s+or\s+' +
        '\(outputByte2Value shl 16\)\s+or\s+' +
        '\(outputByte3Value shl 24\)')
    $sourcePreparationPatterns = [ordered]@{
        'coupler connection capture' =
            $connectionCapturePatterns['coupler connection capture']
        'input-slot connection capture' =
            $connectionCapturePatterns['input-slot connection capture']
        'output-slot connection capture' =
            $connectionCapturePatterns['output-slot connection capture']
        'coupler guarded read' = $couplerGuardPattern
        'input-slot guarded read' = $slotGuardPatterns['input']
        'output-slot guarded read' = $slotGuardPatterns['output']
        'input byte pack' = $inputPackPattern
        'output byte pack' = $outputPackPattern
        'parent and slot presence' = $crevisPresencePattern
        'input quality' = $ioQualityPatterns['input']
        'output quality' = $ioQualityPatterns['output']
        'health cycle bookkeeping' = $healthBookkeepingPattern
        'output observation' = $outputObservationPattern
    }
    $sourcePreparationMatches = @{}
    $sourcePreparationEndIndex = $writerOpenMatch.Index
    foreach ($sourcePreparationStage in $sourcePreparationPatterns.GetEnumerator()) {
        $sourcePreparationMatch = [regex]::Match(
            $diagnosticsLatchRtWorkBlock,
            $sourcePreparationStage.Value)
        if (-not $sourcePreparationMatch.Success -or
            $sourcePreparationMatch.Index -le $writerOpenMatch.Index -or
            -not $firstExtendedWriteMatch.Success -or
            ($sourcePreparationMatch.Index + $sourcePreparationMatch.Length) -ge
                $firstExtendedWriteMatch.Index -or
            (Get-LasalControlDepthAtIndex `
                -Text $diagnosticsLatchRtWorkBlock `
                -Index $sourcePreparationMatch.Index) -ne 0) {
            throw ('LMCEcatInputLatch ' + $sourcePreparationStage.Key +
                ' must execute unconditionally at RtWork top level inside the ' +
                'odd writer interval before the first extended field is published.')
        }
        $sourcePreparationMatches[$sourcePreparationStage.Key] =
            $sourcePreparationMatch
        $stageEndIndex = $sourcePreparationMatch.Index +
            $sourcePreparationMatch.Length
        if ($stageEndIndex -gt $sourcePreparationEndIndex) {
            $sourcePreparationEndIndex = $stageEndIndex
        }
    }
    foreach ($sourceDependency in @(
            @{ Before = 'coupler connection capture'; After = 'coupler guarded read' },
            @{ Before = 'input-slot connection capture'; After = 'input-slot guarded read' },
            @{ Before = 'output-slot connection capture'; After = 'output-slot guarded read' },
            @{ Before = 'input-slot guarded read'; After = 'input byte pack' },
            @{ Before = 'output-slot guarded read'; After = 'output byte pack' },
            @{ Before = 'coupler guarded read'; After = 'parent and slot presence' },
            @{ Before = 'input-slot guarded read'; After = 'parent and slot presence' },
            @{ Before = 'output-slot guarded read'; After = 'parent and slot presence' },
            @{ Before = 'input byte pack'; After = 'input quality' },
            @{ Before = 'parent and slot presence'; After = 'input quality' },
            @{ Before = 'output byte pack'; After = 'output quality' },
            @{ Before = 'parent and slot presence'; After = 'output quality' },
            @{ Before = 'input quality'; After = 'health cycle bookkeeping' },
            @{ Before = 'output quality'; After = 'health cycle bookkeeping' },
            @{ Before = 'output quality'; After = 'output observation' })) {
        $beforeMatch = $sourcePreparationMatches[$sourceDependency.Before]
        $afterMatch = $sourcePreparationMatches[$sourceDependency.After]
        if (($beforeMatch.Index + $beforeMatch.Length) -ge $afterMatch.Index) {
            throw ('LMCEcatInputLatch source dependency requires ' +
                $sourceDependency.Before + ' before ' +
                $sourceDependency.After + '.')
        }
    }
    $extendedSnapshotFields = @(
        @{ Offset = 304; Type = 'DINT'; Value = 'couplerNativeOnline' },
        @{ Offset = 308; Type = 'UDINT'; Value = 'couplerEtherCATState' },
        @{ Offset = 312; Type = 'UDINT'; Value = 'couplerSlaveState' },
        @{ Offset = 316; Type = 'UDINT'; Value = 'couplerALStatus' },
        @{ Offset = 320; Type = 'UDINT'; Value = 'couplerClassState' },
        @{ Offset = 324; Type = 'UDINT'; Value = '0' },
        @{ Offset = 328; Type = 'DINT'; Value = '0' },
        @{ Offset = 332; Type = 'UDINT'; Value = 'couplerLastValidCycle' },
        @{ Offset = 336; Type = 'UDINT'; Value = 'couplerLastStateChangeCycle' },
        @{ Offset = 340; Type = 'DINT'; Value = 'couplerNativeOnline' },
        @{ Offset = 344; Type = 'UDINT'; Value = 'couplerEtherCATState' },
        @{ Offset = 348; Type = 'UDINT'; Value = 'couplerSlaveState' },
        @{ Offset = 352; Type = 'UDINT'; Value = 'couplerALStatus' },
        @{ Offset = 356; Type = 'UDINT'; Value = 'inputSlotClassState' },
        @{ Offset = 360; Type = 'UDINT'; Value = '0' },
        @{ Offset = 364; Type = 'DINT'; Value = '0' },
        @{ Offset = 368; Type = 'UDINT'; Value = 'inputLastValidCycle' },
        @{ Offset = 372; Type = 'UDINT'; Value = 'inputLastStateChangeCycle' },
        @{ Offset = 376; Type = 'DINT'; Value = 'couplerNativeOnline' },
        @{ Offset = 380; Type = 'UDINT'; Value = 'couplerEtherCATState' },
        @{ Offset = 384; Type = 'UDINT'; Value = 'couplerSlaveState' },
        @{ Offset = 388; Type = 'UDINT'; Value = 'couplerALStatus' },
        @{ Offset = 392; Type = 'UDINT'; Value = 'outputSlotClassState' },
        @{ Offset = 396; Type = 'UDINT'; Value = '0' },
        @{ Offset = 400; Type = 'DINT'; Value = '0' },
        @{ Offset = 404; Type = 'UDINT'; Value = 'outputLastValidCycle' },
        @{ Offset = 408; Type = 'UDINT'; Value = 'outputLastStateChangeCycle' },
        @{ Offset = 412; Type = 'UDINT'; Value = 'inputValue' },
        @{ Offset = 416; Type = 'UDINT'; Value = '0' },
        @{ Offset = 420; Type = 'UDINT'; Value = 'inputValidMask' },
        @{ Offset = 424; Type = 'UDINT'; Value = '0' },
        @{ Offset = 428; Type = 'UINT'; Value = 'inputStatus' },
        @{ Offset = 430; Type = 'UINT'; Value = '0' },
        @{ Offset = 432; Type = 'UDINT'; Value = 'cycleCounter' },
        @{ Offset = 436; Type = 'UDINT'; Value = 'outputValue' },
        @{ Offset = 440; Type = 'UDINT'; Value = '0' },
        @{ Offset = 444; Type = 'UDINT'; Value = 'outputValidMask' },
        @{ Offset = 448; Type = 'UDINT'; Value = '0' },
        @{ Offset = 452; Type = 'UINT'; Value = 'outputStatus' },
        @{ Offset = 454; Type = 'UINT'; Value = '0' },
        @{ Offset = 456; Type = 'UDINT'; Value = 'cycleCounter' },
        @{ Offset = 460; Type = 'UDINT'; Value = 'OutputRevision' })
    foreach ($snapshotField in $extendedSnapshotFields) {
        $allFieldMatches = [regex]::Matches(
            $snapshotWriterScanText,
            'SnapshotBytes\[' + $snapshotField.Offset + '\]\$' +
            $snapshotField.Type + '\s*:=')
        $canonicalFieldMatches = [regex]::Matches(
            $snapshotWriterScanText,
            'SnapshotBytes\[' + $snapshotField.Offset + '\]\$' +
            $snapshotField.Type + '\s*:=\s*' +
            $snapshotField.Value + '\s*;')
        if ($allFieldMatches.Count -ne 1 -or
            $canonicalFieldMatches.Count -ne 1 -or
            $canonicalFieldMatches[0].Index -le $writerOpenMatch.Index -or
            $canonicalFieldMatches[0].Index -ge $writerCloseMatch.Index -or
            (Get-LasalControlDepthAtIndex `
                -Text $diagnosticsLatchRtWorkBlock `
                -Index $canonicalFieldMatches[0].Index) -ne 0) {
            throw ('LMCEcatInputLatch extended snapshot offset ' +
                "$($snapshotField.Offset) must be assigned its exact $($snapshotField.Value) " +
                'source exactly once, with no second overwrite, inside the ' +
                'unconditional top-level odd/even PublishSequence writer interval.')
        }
    }
    $legacySnapshotMutationSpecs = @()
    foreach ($legacyOffset in @(0, 4, 8, 12, 16, 20)) {
        $legacySnapshotMutationSpecs += @{
            Offset = $legacyOffset; Type = 'UDINT'; Count = 1
        }
    }
    foreach ($legacyOffset in @(24, 28, 32, 36)) {
        $legacySnapshotMutationSpecs += @{
            Offset = $legacyOffset; Type = 'UDINT'; Count = 2
        }
    }
    foreach ($legacyOffset in @(40, 44, 48)) {
        $legacySnapshotMutationSpecs += @{
            Offset = $legacyOffset; Type = 'UDINT'; Count = 1
        }
    }
    foreach ($legacyHealthBase in @(64, 100, 136, 172)) {
        $legacySnapshotMutationSpecs += @{
            Offset = $legacyHealthBase; Type = 'DINT'; Count = 1
        }
        foreach ($legacyHealthOffset in @(4, 8, 12, 16, 20)) {
            $legacySnapshotMutationSpecs += @{
                Offset = $legacyHealthBase + $legacyHealthOffset
                Type = 'UDINT'
                Count = 1
            }
        }
        $legacySnapshotMutationSpecs += @{
            Offset = $legacyHealthBase + 24; Type = 'DINT'; Count = 1
        }
        foreach ($legacyHealthOffset in @(28, 32)) {
            $legacySnapshotMutationSpecs += @{
                Offset = $legacyHealthBase + $legacyHealthOffset
                Type = 'UDINT'
                Count = 1
            }
        }
    }
    foreach ($legacyDriveBase in @(208, 232, 256, 280)) {
        foreach ($legacyDriveField in @(
                @{ RelativeOffset = 0; Type = 'DINT' },
                @{ RelativeOffset = 4; Type = 'UDINT' },
                @{ RelativeOffset = 8; Type = 'UDINT' },
                @{ RelativeOffset = 12; Type = 'DINT' },
                @{ RelativeOffset = 16; Type = 'UDINT' },
                @{ RelativeOffset = 20; Type = 'UDINT' })) {
            $legacySnapshotMutationSpecs += @{
                Offset = $legacyDriveBase + $legacyDriveField.RelativeOffset
                Type = $legacyDriveField.Type
                Count = 1
            }
        }
    }
    $legacySnapshotCanonicalFields = @(
        @{ Offset = 0; Type = 'UDINT'; Values = @('cycleCounter'); Depth = 0 },
        @{ Offset = 4; Type = 'UDINT'; Values = @('timestampLow'); Depth = 0 },
        @{ Offset = 8; Type = 'UDINT'; Values = @('timestampHigh'); Depth = 0 },
        @{ Offset = 12; Type = 'UDINT'; Values = @('masterState'); Depth = 0 },
        @{ Offset = 16; Type = 'UDINT'; Values = @('consecutiveInvalidCycles'); Depth = 0 },
        @{ Offset = 20; Type = 'UDINT'; Values = @('invalidCycleTotal'); Depth = 0 },
        @{
            Offset = 24
            Type = 'UDINT'
            Values = @(
                'EcatMaster\s*\.\s*FrameTimeTask0\s*\.\s*Read\s*\(\s*\)',
                '0')
            Depth = 1
        },
        @{
            Offset = 28
            Type = 'UDINT'
            Values = @(
                'EcatMaster\s*\.\s*FrameTimeMaxTask0\s*\.\s*Read\s*\(\s*\)',
                '0')
            Depth = 1
        },
        @{
            Offset = 32
            Type = 'UDINT'
            Values = @(
                'EcatMaster\s*\.\s*Act_RtTime\s*\.\s*Read\s*\(\s*\)',
                '0')
            Depth = 1
        },
        @{
            Offset = 36
            Type = 'UDINT'
            Values = @(
                'EcatMaster\s*\.\s*Max_RtTime\s*\.\s*Read\s*\(\s*\)',
                '0')
            Depth = 1
        },
        @{ Offset = 40; Type = 'UDINT'; Values = @('masterFlags'); Depth = 0 },
        @{ Offset = 44; Type = 'UDINT'; Values = @('finalSequence'); Depth = 0 },
        @{ Offset = 48; Type = 'UDINT'; Values = @('masterClassState'); Depth = 0 })
    foreach ($legacyHealthBase in @(64, 100, 136, 172)) {
        foreach ($legacyHealthField in @(
                @{ RelativeOffset = 0; Type = 'DINT'; Value = 'onlineValue' },
                @{ RelativeOffset = 4; Type = 'UDINT'; Value = 'etherCATStateValue' },
                @{ RelativeOffset = 8; Type = 'UDINT'; Value = 'slaveStateValue' },
                @{ RelativeOffset = 12; Type = 'UDINT'; Value = 'alStatusValue' },
                @{ RelativeOffset = 16; Type = 'UDINT'; Value = 'classStateValue' },
                @{ RelativeOffset = 20; Type = 'UDINT'; Value = 'statusWordValue' },
                @{ RelativeOffset = 24; Type = 'DINT'; Value = 'axisErrorValue' })) {
            $legacySnapshotCanonicalFields += @{
                Offset = $legacyHealthBase + $legacyHealthField.RelativeOffset
                Type = $legacyHealthField.Type
                Values = @($legacyHealthField.Value)
                Depth = 0
            }
        }
        foreach ($legacyHealthCycleOffset in @(28, 32)) {
            $legacySnapshotCanonicalFields += @{
                Offset = $legacyHealthBase + $legacyHealthCycleOffset
                Type = 'UDINT'
                Values = @('cycleCounter')
                Depth = 1
            }
        }
    }
    for ($legacyDriveIndex = 1; $legacyDriveIndex -le 4; $legacyDriveIndex += 1) {
        $legacyDriveBase = 184 + (24 * $legacyDriveIndex)
        $legacyDrivePrefix = 'Drive' + $legacyDriveIndex + '\s*\.\s*'
        foreach ($legacyDriveField in @(
                @{
                    RelativeOffset = 0
                    Type = 'DINT'
                    Value = $legacyDrivePrefix + 'SetPos\s*\.\s*Read\s*\(\s*\)'
                },
                @{
                    RelativeOffset = 4
                    Type = 'UDINT'
                    Value = ('TO_UDINT\s*\(\s*' + $legacyDrivePrefix +
                        'Outputs_DigitalOutputs\s*\.\s*Read\s*\(\s*\)\s*\)')
                },
                @{
                    RelativeOffset = 8
                    Type = 'UDINT'
                    Value = ('TO_UDINT\s*\(\s*' + $legacyDrivePrefix +
                        'ControlWord\s*\.\s*Read\s*\(\s*\)\s*\)\s+and\s+' +
                        '0x0000FFFF')
                },
                @{
                    RelativeOffset = 12
                    Type = 'DINT'
                    Value = $legacyDrivePrefix + 'ActPos\s*\.\s*Read\s*\(\s*\)'
                },
                @{
                    RelativeOffset = 16
                    Type = 'UDINT'
                    Value = ('TO_UDINT\s*\(\s*' + $legacyDrivePrefix +
                        'Inputs_DigitalInputs\s*\.\s*Read\s*\(\s*\)\s*\)')
                },
                @{
                    RelativeOffset = 20
                    Type = 'UDINT'
                    Value = 'statusWordValue'
                })) {
            $legacySnapshotCanonicalFields += @{
                Offset = $legacyDriveBase + $legacyDriveField.RelativeOffset
                Type = $legacyDriveField.Type
                Values = @($legacyDriveField.Value)
                Depth = 1
            }
        }
    }
    $legacySourceLocalMutationCounts = [ordered]@{
        'cycleCounter' = 1
        'timestampLow' = 1
        'previousTimestampLow' = 1
        'timestampHigh' = 2
        'masterState' = 2
        'masterClassState' = 2
        'consecutiveInvalidCycles' = 2
        'invalidCycleTotal' = 2
        'masterFlags' = 4
        'onlineValue' = 8
        'etherCATStateValue' = 8
        'slaveStateValue' = 8
        'alStatusValue' = 8
        'classStateValue' = 8
        'statusWordValue' = 8
        'axisErrorValue' = 8
        'stateChanged' = 4
    }
    foreach ($legacySourceLocalMutation in
            $legacySourceLocalMutationCounts.GetEnumerator()) {
        if ([regex]::Matches(
                $snapshotWriterScanText,
                '\b' + $legacySourceLocalMutation.Key +
                    '\s*(?::=|[+\-*/]=)').Count -ne
                $legacySourceLocalMutation.Value) {
            throw ('LMCEcatInputLatch legacy source local ' +
                $legacySourceLocalMutation.Key + ' mutation count must be ' +
                $legacySourceLocalMutation.Value +
                '; published safety inputs may not be overwritten.')
        }
    }
    $legacyMasterConnectedBlock = Get-UniqueLasalIfBlockContaining `
        -Text $diagnosticsLatchRtWorkBlock `
        -ConditionPattern (
            'IsClientConnected\s*\(\s*#\s*EcatMaster\s*\)\s+then') `
        -RequiredPattern 'SnapshotBytes\s*\[\s*24\s*\]' `
        -Message ('LMCEcatInputLatch legacy EcatMaster connected/fallback ' +
            'branch was not found.')
    $legacyMasterConnectedThenArm =
        Get-LasalFirstThenArm $legacyMasterConnectedBlock
    $legacyMasterConnectedIndex =
        $diagnosticsLatchRtWorkBlock.IndexOf(
            $legacyMasterConnectedBlock,
            [StringComparison]::Ordinal)
    if ($legacyMasterConnectedIndex -lt 0) {
        throw ('LMCEcatInputLatch legacy EcatMaster connected branch range ' +
            'could not be established.')
    }
    $legacyMasterDefaultPrefixScanText = Get-LasalScanText (
        $diagnosticsLatchRtWorkBlock.Substring(
            0,
            $legacyMasterConnectedIndex))
    if ($legacyMasterDefaultPrefixScanText -notmatch
            ('(?is)masterState\s*:=\s*0\s*;\s*' +
             'masterClassState\s*:=\s*0xFFFFFFFF\s*;\s*' +
             'consecutiveInvalidCycles\s*:=\s*0\s*;\s*' +
             'masterFlags\s*:=\s*2\s*;\s*\z')) {
        throw ('LMCEcatInputLatch legacy EcatMaster fail-closed defaults must ' +
            'remain consecutive top-level statements immediately before the ' +
            'connected branch.')
    }
    $legacyMasterReadPattern =
        ('(?i)\bEcatMaster\s*\.\s*[A-Za-z_][A-Za-z0-9_]*' +
         '\s*\.\s*Read\s*\(')
    if ([regex]::Matches(
            $legacyMasterConnectedBlock,
            $legacyMasterReadPattern).Count -ne 7 -or
        [regex]::Matches(
            $legacyMasterConnectedThenArm,
            $legacyMasterReadPattern).Count -ne 7) {
        throw ('LMCEcatInputLatch all seven legacy EcatMaster reads must occur ' +
            'only inside the connected true arm.')
    }
    foreach ($legacyMasterSourceBinding in @(
            @{
                Name = 'masterState'
                Default = 'masterState\s*:=\s*0\s*;'
                Connected = ('masterState\s*:=\s*TO_UDINT\s*\(\s*' +
                    'EcatMaster\s*\.\s*EtherCATState\s*\.\s*Read\s*' +
                    '\(\s*\)\s*\)\s*;')
            },
            @{
                Name = 'masterClassState'
                Default = 'masterClassState\s*:=\s*0xFFFFFFFF\s*;'
                Connected = ('masterClassState\s*:=\s*TO_UDINT\s*\(\s*' +
                    'EcatMaster\s*\.\s*ClassState\s*\.\s*Read\s*' +
                    '\(\s*\)\s*\)\s*;')
            },
            @{
                Name = 'consecutiveInvalidCycles'
                Default = 'consecutiveInvalidCycles\s*:=\s*0\s*;'
                Connected = ('consecutiveInvalidCycles\s*:=\s*' +
                    'EcatMaster\s*\.\s*MissedFrameCounter\s*\.\s*Read\s*' +
                    '\(\s*\)\s*;')
            })) {
        $legacyMasterDefaultMatches = [regex]::Matches(
            $snapshotWriterScanText,
            '(?i)' + $legacyMasterSourceBinding.Default)
        $legacyMasterConnectedMatches = [regex]::Matches(
            $snapshotWriterScanText,
            '(?i)' + $legacyMasterSourceBinding.Connected)
        if ($legacyMasterDefaultMatches.Count -ne 1 -or
            (Get-LasalControlDepthAtIndex `
                -Text $diagnosticsLatchRtWorkBlock `
                -Index $legacyMasterDefaultMatches[0].Index) -ne 0 -or
            [regex]::Matches(
                (Get-LasalScanText $legacyMasterConnectedBlock),
                '(?i)' + $legacyMasterSourceBinding.Default).Count -ne 0 -or
            $legacyMasterConnectedMatches.Count -ne 1 -or
            (Get-LasalControlDepthAtIndex `
                -Text $diagnosticsLatchRtWorkBlock `
                -Index $legacyMasterConnectedMatches[0].Index) -ne 1 -or
            [regex]::Matches(
                (Get-LasalScanText $legacyMasterConnectedThenArm),
                '(?i)' + $legacyMasterSourceBinding.Connected).Count -ne 1) {
            throw ('LMCEcatInputLatch legacy ' +
                $legacyMasterSourceBinding.Name +
                ' must use its exact fail-closed default outside the EcatMaster ' +
                'branch and its exact channel Read RHS once inside the connected ' +
                'true arm.')
        }
    }
    foreach ($legacyMasterFlagsPattern in @(
            'masterFlags\s*:=\s*2\s*;',
            'masterFlags\s*:=\s*0\s*;',
            'masterFlags\s*:=\s*masterFlags\s+or\s+1\s*;',
            'masterFlags\s*:=\s*masterFlags\s+or\s+2\s*;')) {
        if ([regex]::Matches(
                $snapshotWriterScanText,
                '(?i)' + $legacyMasterFlagsPattern).Count -ne 1) {
            throw ('LMCEcatInputLatch legacy masterFlags must retain each exact ' +
                'fail-closed, connected, OP, and invalid-cycle mutation once.')
        }
    }
    if ([regex]::Matches(
            (Get-LasalScanText $legacyMasterConnectedBlock),
            '(?i)masterFlags\s*:=\s*2\s*;').Count -ne 0 -or
        [regex]::Matches(
            (Get-LasalScanText $legacyMasterConnectedThenArm),
            '(?i)masterFlags\s*:=\s*0\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch legacy masterFlags default must remain outside ' +
            'the EcatMaster branch and the connected baseline must remain in its ' +
            'true arm.')
    }
    $legacyMasterOpFlagBlock = Get-UniqueLasalIfBlockContaining `
        -Text $legacyMasterConnectedThenArm `
        -ConditionPattern 'masterState\s*=\s*8\s+then' `
        -RequiredPattern 'masterFlags\s*:=\s*masterFlags\s+or\s+1\s*;' `
        -Message ('LMCEcatInputLatch legacy master OP flag must remain guarded ' +
            'by the sampled EtherCAT OP state.')
    $legacyMasterInvalidFlagBlock = Get-UniqueLasalIfBlockContaining `
        -Text $legacyMasterConnectedThenArm `
        -ConditionPattern 'consecutiveInvalidCycles\s*<>\s*0\s+then' `
        -RequiredPattern 'masterFlags\s*:=\s*masterFlags\s+or\s+2\s*;' `
        -Message ('LMCEcatInputLatch legacy invalid-cycle flag must remain ' +
            'guarded by the sampled missed-frame count.')
    if ([regex]::Matches(
            (Get-LasalScanText (Get-LasalFirstThenArm $legacyMasterOpFlagBlock)),
            '(?i)masterFlags\s*:=\s*masterFlags\s+or\s+1\s*;').Count -ne 1 -or
        [regex]::Matches(
            (Get-LasalScanText (
                Get-LasalFirstThenArm $legacyMasterInvalidFlagBlock)),
            '(?i)masterFlags\s*:=\s*masterFlags\s+or\s+2\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch legacy master flag mutations must remain only ' +
            'inside their exact sampled-state true arms.')
    }
    foreach ($legacyMasterTimingField in
            $legacySnapshotCanonicalFields | Where-Object {
                $_.Offset -in @(24, 28, 32, 36)
            }) {
        $legacyMasterActualPattern =
            ('(?i)\bSnapshotBytes\s*\[\s*' +
             $legacyMasterTimingField.Offset + '\s*\]\s*\$\s*UDINT' +
             '\s*:=\s*' + $legacyMasterTimingField.Values[0] + '\s*;')
        $legacyMasterZeroPattern =
            ('(?i)\bSnapshotBytes\s*\[\s*' +
             $legacyMasterTimingField.Offset + '\s*\]\s*\$\s*UDINT' +
             '\s*:=\s*0\s*;')
        if ([regex]::Matches(
                $legacyMasterConnectedThenArm,
                $legacyMasterActualPattern).Count -ne 1 -or
            [regex]::Matches(
                $legacyMasterConnectedThenArm,
                $legacyMasterZeroPattern).Count -ne 0 -or
            [regex]::Matches(
                $legacyMasterConnectedBlock,
                $legacyMasterZeroPattern).Count -ne 1) {
            throw ('LMCEcatInputLatch legacy master timing offset ' +
                $legacyMasterTimingField.Offset +
                ' must read only in the connected true arm and zero only in ELSE.')
        }
    }
    $legacyPreviousConnectedEnd =
        $legacyMasterConnectedIndex + $legacyMasterConnectedBlock.Length
    for ($legacyDriveIndex = 1; $legacyDriveIndex -le 4; $legacyDriveIndex += 1) {
        $legacyDriveBase = 184 + (24 * $legacyDriveIndex)
        $legacyDriveName = 'Drive' + $legacyDriveIndex
        $legacyDriveConnectedBlock = Get-UniqueLasalIfBlockContaining `
            -Text $diagnosticsLatchRtWorkBlock `
            -ConditionPattern (
                'IsClientConnected\s*\(\s*#\s*' +
                $legacyDriveName + '\s*\)\s+then') `
            -RequiredPattern (
                'SnapshotBytes\s*\[\s*' + $legacyDriveBase + '\s*\]') `
            -Message ('LMCEcatInputLatch ' + $legacyDriveName +
                ' connected/fallback branch was not found.')
        $legacyDriveConnectedThenArm =
            Get-LasalFirstThenArm $legacyDriveConnectedBlock
        $legacyDriveConnectedIndex =
            $diagnosticsLatchRtWorkBlock.IndexOf(
                $legacyDriveConnectedBlock,
                $legacyPreviousConnectedEnd,
                [StringComparison]::Ordinal)
        if ($legacyDriveConnectedIndex -lt $legacyPreviousConnectedEnd) {
            throw ('LMCEcatInputLatch ' + $legacyDriveName +
                ' connected branch ordering could not be established.')
        }
        $legacyDriveDefaultPrefix =
            $diagnosticsLatchRtWorkBlock.Substring(
                $legacyPreviousConnectedEnd,
                $legacyDriveConnectedIndex - $legacyPreviousConnectedEnd)
        $legacyDriveDefaultPrefixScanText =
            Get-LasalScanText $legacyDriveDefaultPrefix
        $legacyDriveConnectedScanText =
            Get-LasalScanText $legacyDriveConnectedBlock
        $legacyDriveConnectedThenArmScanText =
            Get-LasalScanText $legacyDriveConnectedThenArm
        if ($legacyDriveDefaultPrefixScanText -notmatch
                ('(?is)onlineValue\s*:=\s*0\s*;\s*' +
                 'etherCATStateValue\s*:=\s*0\s*;\s*' +
                 'slaveStateValue\s*:=\s*0\s*;\s*' +
                 'alStatusValue\s*:=\s*0\s*;\s*' +
                 'classStateValue\s*:=\s*0xFFFFFFFF\s*;\s*' +
                 'statusWordValue\s*:=\s*0\s*;\s*' +
                 'axisErrorValue\s*:=\s*0\s*;\s*\z')) {
            throw ('LMCEcatInputLatch ' + $legacyDriveName +
                ' fail-closed health defaults must remain consecutive ' +
                'top-level statements immediately before its connected branch.')
        }
        $legacyDriveSourceBindings = @(
            @{
                Name = 'onlineValue'
                Default = 'onlineValue\s*:=\s*0\s*;'
                Connected = ('onlineValue\s*:=\s*' + $legacyDriveName +
                    '\s*\.\s*Online\s*\.\s*Read\s*\(\s*\)\s*;')
            },
            @{
                Name = 'etherCATStateValue'
                Default = 'etherCATStateValue\s*:=\s*0\s*;'
                Connected = ('etherCATStateValue\s*:=\s*TO_UDINT\s*\(\s*' +
                    $legacyDriveName + '\s*\.\s*EtherCATState\s*\.\s*Read' +
                    '\s*\(\s*\)\s*\)\s*;')
            },
            @{
                Name = 'slaveStateValue'
                Default = 'slaveStateValue\s*:=\s*0\s*;'
                Connected = ('slaveStateValue\s*:=\s*' + $legacyDriveName +
                    '\s*\.\s*SlaveState\s*\.\s*Read\s*\(\s*\)\s*;')
            },
            @{
                Name = 'alStatusValue'
                Default = 'alStatusValue\s*:=\s*0\s*;'
                Connected = ('alStatusValue\s*:=\s*TO_UDINT\s*\(\s*' +
                    $legacyDriveName + '\s*\.\s*AL_StatusCode\s*\.\s*Read' +
                    '\s*\(\s*\)\s*\)\s*;')
            },
            @{
                Name = 'classStateValue'
                Default = 'classStateValue\s*:=\s*0xFFFFFFFF\s*;'
                Connected = ('classStateValue\s*:=\s*TO_UDINT\s*\(\s*' +
                    $legacyDriveName + '\s*\.\s*ClassState\s*\.\s*Read' +
                    '\s*\(\s*\)\s*\)\s*;')
            },
            @{
                Name = 'statusWordValue'
                Default = 'statusWordValue\s*:=\s*0\s*;'
                Connected = ('statusWordValue\s*:=\s*TO_UDINT\s*\(\s*' +
                    $legacyDriveName + '\s*\.\s*StateWord\s*\.\s*Read' +
                    '\s*\(\s*\)\s*\)\s+and\s+0x0000FFFF\s*;')
            },
            @{
                Name = 'axisErrorValue'
                Default = 'axisErrorValue\s*:=\s*0\s*;'
                Connected = ('axisErrorValue\s*:=\s*' + $legacyDriveName +
                    '\s*\.\s*AxError\s*\.\s*Read\s*\(\s*\)\s*;')
            })
        foreach ($legacyDriveSourceBinding in $legacyDriveSourceBindings) {
            $legacyDriveDefaultMatches = [regex]::Matches(
                $legacyDriveDefaultPrefixScanText,
                '(?i)' + $legacyDriveSourceBinding.Default)
            $legacyDriveConnectedMatches = [regex]::Matches(
                $snapshotWriterScanText,
                '(?i)' + $legacyDriveSourceBinding.Connected)
            if ($legacyDriveDefaultMatches.Count -ne 1 -or
                (Get-LasalControlDepthAtIndex `
                    -Text $diagnosticsLatchRtWorkBlock `
                    -Index ($legacyPreviousConnectedEnd +
                        $legacyDriveDefaultMatches[0].Index)) -ne 0 -or
                [regex]::Matches(
                    $legacyDriveConnectedScanText,
                    '(?i)' + $legacyDriveSourceBinding.Default).Count -ne 0 -or
                $legacyDriveConnectedMatches.Count -ne 1 -or
                (Get-LasalControlDepthAtIndex `
                    -Text $diagnosticsLatchRtWorkBlock `
                    -Index $legacyDriveConnectedMatches[0].Index) -ne 1 -or
                [regex]::Matches(
                    $legacyDriveConnectedThenArmScanText,
                    '(?i)' + $legacyDriveSourceBinding.Connected).Count -ne 1) {
                throw ('LMCEcatInputLatch ' + $legacyDriveName + ' legacy ' +
                    $legacyDriveSourceBinding.Name +
                    ' must reset to its exact fail-closed default immediately ' +
                    'before the branch and bind once to its exact channel Read ' +
                    'RHS inside the connected true arm.')
            }
        }
        $legacyDriveReadPattern =
            ('(?i)\b' + $legacyDriveName + '\s*\.\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*\.\s*Read\s*\(')
        if ([regex]::Matches(
                $legacyDriveConnectedBlock,
                $legacyDriveReadPattern).Count -ne 12 -or
            [regex]::Matches(
                $legacyDriveConnectedThenArm,
                $legacyDriveReadPattern).Count -ne 12) {
            throw ('LMCEcatInputLatch all twelve ' + $legacyDriveName +
                ' reads must occur only inside its connected true arm.')
        }
        $legacyDriveImageFields = @(
            $legacySnapshotCanonicalFields | Where-Object {
                $_.Offset -ge $legacyDriveBase -and
                $_.Offset -le ($legacyDriveBase + 20)
            })
        foreach ($legacyDriveImageField in $legacyDriveImageFields) {
            $legacyDriveImageWritePattern =
                ('(?i)\bSnapshotBytes\s*\[\s*' +
                 $legacyDriveImageField.Offset + '\s*\]\s*\$\s*' +
                 $legacyDriveImageField.Type + '\s*:=\s*' +
                 $legacyDriveImageField.Values[0] + '\s*;')
            if ([regex]::Matches(
                    $legacyDriveConnectedThenArm,
                    $legacyDriveImageWritePattern).Count -ne 1) {
                throw ('LMCEcatInputLatch ' + $legacyDriveName +
                    ' PDO offset ' + $legacyDriveImageField.Offset +
                    ' must be written exactly once inside the connected true arm.')
            }
        }
        $legacyDriveZeroPattern =
            ('(?is)_memset\s*\(\s*dest\s*:=\s*#\s*SnapshotBytes' +
             '\s*\[\s*' + $legacyDriveBase + '\s*\]\s*,\s*' +
             'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*24\s*\)')
        if ([regex]::Matches(
                $legacyDriveConnectedThenArm,
                $legacyDriveZeroPattern).Count -ne 0 -or
            [regex]::Matches(
                $legacyDriveConnectedBlock,
                $legacyDriveZeroPattern).Count -ne 1) {
            throw ('LMCEcatInputLatch ' + $legacyDriveName +
                ' zero fallback must occur exactly once in ELSE, never in the ' +
                'connected true arm.')
        }
        $legacyPreviousConnectedEnd =
            $legacyDriveConnectedIndex + $legacyDriveConnectedBlock.Length
    }
    $expectedSnapshotTypedMutationCount =
        ($legacySnapshotMutationSpecs |
            Measure-Object -Property Count -Sum).Sum +
        $extendedSnapshotFields.Count
    if ($rtSnapshotTypedMutations.Count -ne
            $expectedSnapshotTypedMutationCount) {
        throw ('LMCEcatInputLatch RtWork SnapshotBytes typed mutation count is ' +
            $rtSnapshotTypedMutations.Count + ', expected exactly ' +
            $expectedSnapshotTypedMutationCount +
            ' across the frozen legacy and extended layouts.')
    }
    foreach ($legacySnapshotMutationSpec in $legacySnapshotMutationSpecs) {
        if ([regex]::Matches(
                $snapshotWriterScanText,
                '(?i)\bSnapshotBytes\s*\[\s*' +
                    $legacySnapshotMutationSpec.Offset + '\s*\]\s*\$\s*' +
                    $legacySnapshotMutationSpec.Type +
                    '\s*(?::=|[+\-*/]=)').Count -ne
                $legacySnapshotMutationSpec.Count) {
            throw ('LMCEcatInputLatch legacy snapshot offset ' +
                $legacySnapshotMutationSpec.Offset + '$' +
                $legacySnapshotMutationSpec.Type + ' mutation count must remain ' +
                $legacySnapshotMutationSpec.Count + '.')
        }
    }
    foreach ($legacySnapshotCanonicalField in $legacySnapshotCanonicalFields) {
        $legacyCanonicalMutationCount = 0
        foreach ($legacyCanonicalValue in
                $legacySnapshotCanonicalField.Values) {
            $legacyCanonicalPattern =
                ('(?i)\bSnapshotBytes\s*\[\s*' +
                 $legacySnapshotCanonicalField.Offset + '\s*\]\s*\$\s*' +
                 $legacySnapshotCanonicalField.Type + '\s*:=\s*' +
                 $legacyCanonicalValue + '\s*;')
            $legacyCanonicalMatches = [regex]::Matches(
                $snapshotWriterScanText,
                $legacyCanonicalPattern)
            if ($legacyCanonicalMatches.Count -ne 1 -or
                (Get-LasalControlDepthAtIndex `
                    -Text $diagnosticsLatchRtWorkBlock `
                    -Index $legacyCanonicalMatches[0].Index) -ne
                    $legacySnapshotCanonicalField.Depth) {
                throw ('LMCEcatInputLatch legacy snapshot offset ' +
                    $legacySnapshotCanonicalField.Offset + '$' +
                    $legacySnapshotCanonicalField.Type +
                    ' must publish canonical source pattern ' +
                    $legacyCanonicalValue + ' exactly once at control depth ' +
                    $legacySnapshotCanonicalField.Depth + '.')
            }
            $legacyCanonicalMutationCount += $legacyCanonicalMatches.Count
        }
        if ($legacyCanonicalMutationCount -ne
                $legacySnapshotCanonicalField.Values.Count) {
            throw ('LMCEcatInputLatch legacy snapshot offset ' +
                $legacySnapshotCanonicalField.Offset +
                ' canonical mutation set is incomplete.')
        }
    }
    if ($rtSnapshotAggregateMutations.Count -ne 4) {
        throw ('LMCEcatInputLatch RtWork must retain only the four canonical ' +
            '24-byte legacy drive-image zeroing operations.')
    }
    foreach ($legacyDriveImageOffset in @(208, 232, 256, 280)) {
        $legacyDriveImageZeroMatches = [regex]::Matches(
                $snapshotWriterScanText,
                ('(?is)_memset\s*\(\s*dest\s*:=\s*#SnapshotBytes\s*\[\s*' +
                 $legacyDriveImageOffset + '\s*\]\s*,\s*' +
                 'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*24\s*\)'))
        if ($legacyDriveImageZeroMatches.Count -ne 1 -or
            (Get-LasalControlDepthAtIndex `
                -Text $diagnosticsLatchRtWorkBlock `
                -Index $legacyDriveImageZeroMatches[0].Index) -ne 1) {
            throw ('LMCEcatInputLatch legacy drive image at offset ' +
                $legacyDriveImageOffset +
                ' must have exactly one canonical depth-1 24-byte zeroing fallback.')
        }
    }
    Assert-Match $diagnosticsService (
        '(?s)FUNCTION HandleEtherCATTopologyIoRequest.*?' +
        'CommandId\s*:\s*UINT;.*?pRequest\s*:\s*\^USINT;.*?' +
        'RequestSize\s*:\s*UDINT;.*?pResponse\s*:\s*\^USINT;.*?' +
        'ResponseCapacity\s*:\s*UDINT;.*?' +
        'CallerSessionEpoch\s*:\s*UDINT;.*?' +
        'CurrentDiagnosticsBootId\s*:\s*UDINT;.*?' +
        'ResponseSize\s*:\s*DINT;') (
        'LMCDiagnosticsService topology/I/O helper IDE declaration is incomplete.')

    if (-not $topologyIoOutputIntegrated -and
        ($diagnosticsLatch -match
            'TryQueueOutputWrite|CopyOutputCompletion|CancelQueuedOutput|IsOutputReusable' -or
         $diagnosticsService -match '(?m)^\s*0x7E23\s*:')) {
        throw ('Integrated read-owner checkpoint contains a partial output-' +
            'write owner or 0x7E23 handler. Advance to ' +
            'IntegratedOutputOwnerDormant only with the complete mailbox owner.')
    }
}

if ($topologyIoOutputIntegrated) {
    foreach ($mailboxVariablePattern in @(
            'OutputMailboxState\s*:\s*UDINT;',
            'OutputRequestBytes\s*:\s*ARRAY \[0\.\.47\] OF USINT;',
            'OutputCompletionSequence\s*:\s*UDINT;',
            'OutputCompletionBytes\s*:\s*ARRAY \[0\.\.31\] OF USINT;')) {
        Assert-Match $diagnosticsLatch $mailboxVariablePattern (
            "LMCEcatInputLatch output mailbox member '$mailboxVariablePattern' is missing.")
    }
    foreach ($mailboxMethod in @(
            'TryQueueOutputWrite',
            'CopyOutputCompletion',
            'CancelQueuedOutput',
            'IsOutputReusable')) {
        Assert-Match $diagnosticsLatch (
            'FUNCTION GLOBAL ' + $mailboxMethod) (
            "LMCEcatInputLatch.$mailboxMethod IDE declaration is missing.")
        Assert-Match $diagnosticsLatch (
            'FUNCTION GLOBAL LMCEcatInputLatch::' + $mailboxMethod) (
            "LMCEcatInputLatch.$mailboxMethod implementation is missing.")
    }
    $diagnosticsLatchClassDeclarationBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)LMCEcatInputLatch\s*:\s*CLASS.*?END_CLASS;').Value
    if ([string]::IsNullOrWhiteSpace($diagnosticsLatchClassDeclarationBlock)) {
        throw 'LMCEcatInputLatch IDE class declaration block was not found.'
    }
    Assert-Match $diagnosticsLatchClassDeclarationBlock (
        '(?s)FUNCTION GLOBAL TryQueueOutputWrite\s*' +
        'VAR_INPUT\s*' +
        'OperationToken\s*:\s*UDINT;\s*' +
        'TopologyRevision\s*:\s*UDINT;\s*' +
        'DiagnosticsBootId\s*:\s*UDINT;\s*' +
        'OwnerSessionEpoch\s*:\s*UDINT;\s*' +
        'IOReference\s*:\s*UDINT;\s*' +
        'ValueLow\s*:\s*UDINT;\s*' +
        'ValueHigh\s*:\s*UDINT;\s*' +
        'MaskLow\s*:\s*UDINT;\s*' +
        'MaskHigh\s*:\s*UDINT;\s*' +
        'ExpectedOutputRevision\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*ret_code(?:\s*\([^)]*\))?\s*:\s*iprStates;\s*END_VAR') (
        'LMCEcatInputLatch.TryQueueOutputWrite IDE signature must preserve ten ' +
        'UDINT inputs and the iprStates result without narrowing.')
    Assert-Match $diagnosticsLatchClassDeclarationBlock (
        '(?s)FUNCTION GLOBAL CopyOutputCompletion\s*' +
        'VAR_INPUT\s*ExpectedToken\s*:\s*UDINT;\s*' +
        'pDest\s*:\s*\^void;\s*DestSize\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR') (
        'LMCEcatInputLatch.CopyOutputCompletion IDE signature is incomplete or narrowed.')
    Assert-Match $diagnosticsLatchClassDeclarationBlock (
        '(?s)FUNCTION GLOBAL CancelQueuedOutput\s*' +
        'VAR_INPUT\s*ExpectedToken\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR') (
        'LMCEcatInputLatch.CancelQueuedOutput IDE signature is incomplete or narrowed.')
    Assert-Match $diagnosticsLatchClassDeclarationBlock (
        '(?s)FUNCTION GLOBAL IsOutputReusable\s*' +
        'VAR_OUTPUT\s*Ready\s*:\s*BOOL;\s*END_VAR') (
        'LMCEcatInputLatch.IsOutputReusable IDE signature must return BOOL Ready.')
    Assert-Match $diagnosticsLatch (
        '(?s)#define LMC_ECAT_IO_MAILBOX_IDLE\s+0.*?' +
        '#define LMC_ECAT_IO_MAILBOX_WRITING_REQUEST\s+1.*?' +
        '#define LMC_ECAT_IO_MAILBOX_READY\s+2.*?' +
        '#define LMC_ECAT_IO_MAILBOX_RUNNING\s+3.*?' +
        '#define LMC_ECAT_IO_MAILBOX_WRITING_COMPLETION\s+4.*?' +
        '#define LMC_ECAT_IO_MAILBOX_COMPLETION_READY\s+5') (
        'LMCEcatInputLatch output mailbox state constants are incomplete or reordered.')
    Assert-Match $diagnosticsLatch (
        '(?s)#define LMC_ECAT_IO_TOPOLOGY_REVISION\s+0x15867EEC.*?' +
        '#define LMC_ECAT_IO_OUTPUT_REFERENCE\s+0x00010002.*?' +
        '#define LMC_ECAT_IO_OUTPUT_VALID_MASK\s+0xFFFFFFFF') (
        'LMCEcatInputLatch output mailbox identity constants are incomplete.')

    $outputTryQueueBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::TryQueueOutputWrite.*?END_FUNCTION').Value
    $outputCopyCompletionBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CopyOutputCompletion.*?END_FUNCTION').Value
    $outputCancelBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::CancelQueuedOutput.*?END_FUNCTION').Value
    $outputReusableBlock = [regex]::Match(
        $diagnosticsLatch,
        '(?s)FUNCTION GLOBAL LMCEcatInputLatch::IsOutputReusable.*?END_FUNCTION').Value
    foreach ($mailboxImplementation in @(
            @{ Name = 'TryQueueOutputWrite'; Block = $outputTryQueueBlock },
            @{ Name = 'CopyOutputCompletion'; Block = $outputCopyCompletionBlock },
            @{ Name = 'CancelQueuedOutput'; Block = $outputCancelBlock },
            @{ Name = 'IsOutputReusable'; Block = $outputReusableBlock })) {
        if ([string]::IsNullOrWhiteSpace($mailboxImplementation.Block)) {
            throw "LMCEcatInputLatch.$($mailboxImplementation.Name) implementation was not found."
        }
    }

    Assert-Match $outputTryQueueBlock (
        '(?s)VAR_INPUT\s*' +
        'OperationToken\s*:\s*UDINT;\s*TopologyRevision\s*:\s*UDINT;\s*' +
        'DiagnosticsBootId\s*:\s*UDINT;\s*OwnerSessionEpoch\s*:\s*UDINT;\s*' +
        'IOReference\s*:\s*UDINT;\s*ValueLow\s*:\s*UDINT;\s*' +
        'ValueHigh\s*:\s*UDINT;\s*MaskLow\s*:\s*UDINT;\s*' +
        'MaskHigh\s*:\s*UDINT;\s*ExpectedOutputRevision\s*:\s*UDINT;\s*' +
        'END_VAR\s*VAR_OUTPUT\s*' +
        'ret_code(?:\s*\([^)]*\))?\s*:\s*iprStates;\s*END_VAR.*?' +
        'previousMailboxState\s*:\s*UDINT;') (
        'LMCEcatInputLatch.TryQueueOutputWrite implementation signature is ' +
        'incomplete or narrowed.')
    Assert-Match $outputCopyCompletionBlock (
        '(?s)VAR_INPUT\s*ExpectedToken\s*:\s*UDINT;\s*' +
        'pDest\s*:\s*\^Void;\s*DestSize\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR.*?' +
        'sequenceBefore\s*:\s*UDINT;.*?' +
        'sequenceAfter\s*:\s*UDINT;.*?' +
        'previousMailboxState\s*:\s*UDINT;') (
        'LMCEcatInputLatch.CopyOutputCompletion implementation must preserve ' +
        'its exact ABI and U32 seqlock/CAS locals.')
    Assert-Match $outputCancelBlock (
        '(?s)VAR_INPUT\s*ExpectedToken\s*:\s*UDINT;\s*END_VAR\s*' +
        'VAR_OUTPUT\s*Result\s*:\s*DINT;\s*END_VAR.*?' +
        'previousMailboxState\s*:\s*UDINT;') (
        'LMCEcatInputLatch.CancelQueuedOutput implementation must preserve ' +
        'its exact token/result ABI and U32 CAS local.')
    Assert-Match $outputReusableBlock (
        '(?s)VAR_OUTPUT\s*Ready\s*:\s*BOOL;\s*END_VAR') (
        'LMCEcatInputLatch.IsOutputReusable implementation must return BOOL Ready.')

    $outputRtLocalTypes = [ordered]@{
        'outputRequestClaimed' = 'BOOL'
        'previousMailboxState' = 'UDINT'
        'outputOperationToken' = 'UDINT'
        'requestTopologyRevision' = 'UDINT'
        'requestDiagnosticsBootId' = 'UDINT'
        'requestOwnerSessionEpoch' = 'UDINT'
        'requestIOReference' = 'UDINT'
        'outputRequestValue' = 'UDINT'
        'outputRequestValueHigh' = 'UDINT'
        'outputMaskValue' = 'UDINT'
        'outputMaskHigh' = 'UDINT'
        'requestExpectedOutputRevision' = 'UDINT'
        'requestReserved0' = 'UDINT'
        'requestReserved1' = 'UDINT'
        'newOutputValue' = 'UDINT'
        'outputResult' = 'DINT'
        'outputDetailCode' = 'UDINT'
        'completionSequence' = 'UDINT'
        'finalCompletionSequence' = 'UDINT'
    }
    foreach ($outputRtLocal in $outputRtLocalTypes.GetEnumerator()) {
        Assert-LasalExactDeclaredType `
            -Text $diagnosticsLatchRtWorkBlock `
            -Name $outputRtLocal.Key `
            -ExpectedType $outputRtLocal.Value `
            -Owner ('LMCEcatInputLatch.RtWork output local ' +
                $outputRtLocal.Key)
    }

    Assert-Match $outputTryQueueBlock (
        '(?s)ret_code := ERROR;\s*' +
        'previousMailboxState := sigclib_atomic_cmpxchgU32\(\s*' +
        'pValue:=#OutputMailboxState,\s*' +
        'cmpVal:=LMC_ECAT_IO_MAILBOX_IDLE,\s*' +
        'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_REQUEST\);\s*' +
        'if previousMailboxState <> LMC_ECAT_IO_MAILBOX_IDLE then\s*' +
        'RETURN;\s*end_if;\s*' +
        '_memset\(dest:=#OutputRequestBytes\[0\],\s*' +
        'usByte:=0,\s*cntr:=48\);.*?' +
        'OutputRequestBytes\[0\]\$UDINT := OperationToken;.*?' +
        'OutputRequestBytes\[4\]\$UDINT := TopologyRevision;.*?' +
        'OutputRequestBytes\[8\]\$UDINT := DiagnosticsBootId;.*?' +
        'OutputRequestBytes\[12\]\$UDINT := OwnerSessionEpoch;.*?' +
        'OutputRequestBytes\[16\]\$UDINT := IOReference;.*?' +
        'OutputRequestBytes\[20\]\$UDINT := ValueLow;.*?' +
        'OutputRequestBytes\[24\]\$UDINT := ValueHigh;.*?' +
        'OutputRequestBytes\[28\]\$UDINT := MaskLow;.*?' +
        'OutputRequestBytes\[32\]\$UDINT := MaskHigh;.*?' +
        'OutputRequestBytes\[36\]\$UDINT := ExpectedOutputRevision;.*?' +
        'sigclib_atomic_setU32\(\s*pValue:=#OutputMailboxState,\s*' +
        'value:=LMC_ECAT_IO_MAILBOX_READY\);\s*' +
        'ret_code := READY;') (
        'LMCEcatInputLatch producer must claim Idle, write the complete immutable ' +
        '48-byte request, publish Ready last, and return success only then.')
    if ([regex]::Matches($outputTryQueueBlock, 'ret_code\s*:=\s*ERROR;').Count -ne 1 -or
        [regex]::Matches($outputTryQueueBlock, 'ret_code\s*:=\s*READY;').Count -ne 1 -or
        [regex]::Matches(
            (Get-LasalScanText $outputTryQueueBlock),
            '\bret_code\s*(?::=|[+\-*/]=)').Count -ne 2) {
        throw ('LMCEcatInputLatch.TryQueueOutputWrite must default to ERROR and ' +
            'return READY exactly once after mailbox publication.')
    }
    $outputTryQueueScanText = Get-LasalScanText $outputTryQueueBlock
    if ($outputTryQueueScanText -notmatch
            '(?is)ret_code\s*:=\s*READY;\s*END_FUNCTION\s*$' -or
        [regex]::Matches(
            $outputTryQueueScanText,
            ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\(\s*' +
             'pValue:=#OutputMailboxState')).Count -ne 2 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            ('(?is)sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_IDLE,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_REQUEST\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'value:=LMC_ECAT_IO_MAILBOX_READY\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            '\bpreviousMailboxState\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        $outputTryQueueScanText -match
            '(?i)#?OutputMailboxState\s*(?::=|[+\-*/]=)' ) {
        throw ('LMCEcatInputLatch.TryQueueOutputWrite must mutate mailbox state ' +
            'only through one IDLE-to-WRITING_REQUEST claim and final READY ' +
            'publish, with READY result as its last executable statement.')
    }
    $outputRequestFields = @(
        @{ Offset = 0; Value = 'OperationToken' },
        @{ Offset = 4; Value = 'TopologyRevision' },
        @{ Offset = 8; Value = 'DiagnosticsBootId' },
        @{ Offset = 12; Value = 'OwnerSessionEpoch' },
        @{ Offset = 16; Value = 'IOReference' },
        @{ Offset = 20; Value = 'ValueLow' },
        @{ Offset = 24; Value = 'ValueHigh' },
        @{ Offset = 28; Value = 'MaskLow' },
        @{ Offset = 32; Value = 'MaskHigh' },
        @{ Offset = 36; Value = 'ExpectedOutputRevision' })
    foreach ($outputRequestField in $outputRequestFields) {
        $allRequestFieldWrites = [regex]::Matches(
            $outputTryQueueScanText,
            'OutputRequestBytes\[' + $outputRequestField.Offset +
                '\]\$[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
        $canonicalRequestFieldWrites = [regex]::Matches(
            $outputTryQueueBlock,
            'OutputRequestBytes\[' + $outputRequestField.Offset +
                '\]\$UDINT\s*:=\s*' + $outputRequestField.Value + '\s*;')
        if ($allRequestFieldWrites.Count -ne 1 -or
            $canonicalRequestFieldWrites.Count -ne 1) {
            throw ('LMCEcatInputLatch.TryQueueOutputWrite request offset ' +
                $outputRequestField.Offset + ' must have exactly one canonical write.')
        }
    }
    foreach ($reservedRequestOffset in @(40, 44)) {
        if ([regex]::Matches(
                $outputTryQueueScanText,
                'OutputRequestBytes\[' + $reservedRequestOffset +
                    '\]\$[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)').Count -ne 0) {
            throw ('LMCEcatInputLatch.TryQueueOutputWrite reserved request offset ' +
                $reservedRequestOffset + ' must remain zero from the 48-byte memset.')
        }
    }
    if ([regex]::Matches(
            $outputTryQueueScanText,
            ('OutputRequestBytes\[[^\]]+\]\$' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')).Count -ne 10 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            '(?is)_memset\(\s*dest:=#OutputRequestBytes\[').Count -ne 1 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            ('(?is)_memset\(\s*dest:=#OutputRequestBytes\[0\],\s*' +
             'usByte:=0,\s*cntr:=48\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            '(?is)_memcpy\(\s*ptr1:=#OutputRequestBytes\[').Count -ne 0) {
        throw ('LMCEcatInputLatch.TryQueueOutputWrite must perform one canonical ' +
            '48-byte zero initialization followed by only the ten declared ' +
            'request field writes; aggregate or compound overwrites are forbidden.')
    }
    $tryQueueClaimFailureBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputTryQueueBlock `
        -ConditionPattern (
            'previousMailboxState\s*<>\s*' +
            'LMC_ECAT_IO_MAILBOX_IDLE\s+then') `
        -RequiredPattern 'RETURN;' `
        -Message ('LMCEcatInputLatch.TryQueueOutputWrite Idle-claim failure ' +
            'guard was not found.')
    $tryQueueClaimFailureArm =
        Get-LasalFirstThenArm $tryQueueClaimFailureBlock
    if ([regex]::Matches(
            (Get-LasalScanText $tryQueueClaimFailureArm),
            '(?i)\bRETURN\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch.TryQueueOutputWrite claim-failure true branch ' +
            'must RETURN exactly once.')
    }
    $tryQueueFailureGuardIndex = $outputTryQueueBlock.IndexOf(
        $tryQueueClaimFailureBlock,
        [StringComparison]::Ordinal)
    $tryQueueReadyResultMatch = [regex]::Match(
        $outputTryQueueBlock,
        '(?s)sigclib_atomic_setU32\(\s*pValue:=#OutputMailboxState,\s*' +
            'value:=LMC_ECAT_IO_MAILBOX_READY\);\s*ret_code := READY;')
    if ($tryQueueFailureGuardIndex -lt 0 -or
        -not $tryQueueReadyResultMatch.Success) {
        throw 'LMCEcatInputLatch.TryQueueOutputWrite success interval was not found.'
    }
    $tryQueueSuccessSection = $outputTryQueueBlock.Substring(
        $tryQueueFailureGuardIndex + $tryQueueClaimFailureBlock.Length,
        ($tryQueueReadyResultMatch.Index + $tryQueueReadyResultMatch.Length) -
            ($tryQueueFailureGuardIndex + $tryQueueClaimFailureBlock.Length))
    if ((Get-LasalScanText $tryQueueSuccessSection) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch.TryQueueOutputWrite must not RETURN after ' +
            'claiming WRITING_REQUEST and before publishing READY/result success.')
    }

    Assert-Match $diagnosticsLatchRtWorkBlock (
        '(?s)outputRequestClaimed := FALSE;.*?' +
        'previousMailboxState := sigclib_atomic_cmpxchgU32\(\s*' +
        'pValue:=#OutputMailboxState,\s*' +
        'cmpVal:=LMC_ECAT_IO_MAILBOX_READY,\s*' +
        'newVal:=LMC_ECAT_IO_MAILBOX_RUNNING\);.*?' +
        'if previousMailboxState = LMC_ECAT_IO_MAILBOX_READY then.*?' +
        'outputRequestClaimed := TRUE;.*?' +
        'outputResult := -1;.*?' +
        'outputDetailCode := 0;.*?' +
        'outputOperationToken := OutputRequestBytes\[0\]\$UDINT;.*?' +
        'requestTopologyRevision := OutputRequestBytes\[4\]\$UDINT;.*?' +
        'requestDiagnosticsBootId := OutputRequestBytes\[8\]\$UDINT;.*?' +
        'requestOwnerSessionEpoch := OutputRequestBytes\[12\]\$UDINT;.*?' +
        'requestIOReference := OutputRequestBytes\[16\]\$UDINT;.*?' +
        'outputRequestValue := OutputRequestBytes\[20\]\$UDINT;.*?' +
        'outputRequestValueHigh := OutputRequestBytes\[24\]\$UDINT;.*?' +
        'outputMaskValue := OutputRequestBytes\[28\]\$UDINT;.*?' +
        'outputMaskHigh := OutputRequestBytes\[32\]\$UDINT;.*?' +
        'requestExpectedOutputRevision := OutputRequestBytes\[36\]\$UDINT;.*?' +
        'requestReserved0 := OutputRequestBytes\[40\]\$UDINT;.*?' +
        'requestReserved1 := OutputRequestBytes\[44\]\$UDINT;') (
        'LMCEcatInputLatch RT owner must atomically claim Ready before reading ' +
        'the complete immutable 48-byte output request.')
    $outputClaimBlock = Get-UniqueLasalIfBlockContaining `
        -Text $diagnosticsLatchRtWorkBlock `
        -ConditionPattern (
            'previousMailboxState\s*=\s*' +
            'LMC_ECAT_IO_MAILBOX_READY\s+then') `
        -RequiredPattern 'OutputSlot\.OutputS_Byte0\.Write\(' `
        -Message ('LMCEcatInputLatch RT output validation, apply, and ' +
            'physical write must remain inside the successful ' +
            'READY-to-RUNNING claim branch.')
    $outputClaimThenArm = Get-LasalFirstThenArm $outputClaimBlock
    Assert-Match $outputClaimThenArm (
        '(?s)requestTopologyRevision <> LMC_ECAT_IO_TOPOLOGY_REVISION.*?' +
        'outputDetailCode := 26;.*?' +
        'requestDiagnosticsBootId = 0.*?outputDetailCode := 25;.*?' +
        'requestOwnerSessionEpoch = 0.*?outputDetailCode := 24;.*?' +
        'requestIOReference <> LMC_ECAT_IO_OUTPUT_REFERENCE.*?' +
        'outputDetailCode := 28;.*?' +
        'outputRequestValueHigh <> 0.*?outputDetailCode := 30;.*?' +
        'outputMaskHigh <> 0.*?outputDetailCode := 30;.*?' +
        'requestReserved0 <> 0.*?outputDetailCode := 24;.*?' +
        'requestReserved1 <> 0.*?outputDetailCode := 24;.*?' +
        'outputValid = FALSE.*?outputDetailCode := 31;.*?' +
        'requestExpectedOutputRevision <> OutputRevision.*?' +
        'outputDetailCode := 29;.*?' +
        'outputMaskValue = 0.*?outputDetailCode := 30;.*?' +
        '\(outputMaskValue and not LMC_ECAT_IO_OUTPUT_VALID_MASK\) <> 0.*?' +
        'outputDetailCode := 30;.*?' +
        '\(outputRequestValue and not outputMaskValue\) <> 0.*?' +
        'outputDetailCode := 30;.*?' +
        'newOutputValue := \(outputValue and not outputMaskValue\) or\s*' +
        '\(outputRequestValue and outputMaskValue\);.*?' +
        'if outputDetailCode = 0 then') (
        'LMCEcatInputLatch RT owner provenance, live validity, output revision, ' +
        'canonical mask, and same-cycle masked-value validation is incomplete.')

    $diagnosticsLatchRtWorkScanText =
        Get-LasalScanText $diagnosticsLatchRtWorkBlock
    $outputClaimThenArmScanText = Get-LasalScanText $outputClaimThenArm
    $rtOutputStickyPrefix =
        '\(outputDetailCode\s*=\s*0\)\s*&\s*'
    $rtOutputFailureGuards = @(
        @{
            Name = 'topology revision'
            Condition = ($rtOutputStickyPrefix +
                '\(requestTopologyRevision\s*<>\s*' +
                'LMC_ECAT_IO_TOPOLOGY_REVISION\)')
            Detail = 26
        },
        @{
            Name = 'nonzero BootId'
            Condition = ($rtOutputStickyPrefix +
                '\(requestDiagnosticsBootId\s*=\s*0\)')
            Detail = 25
        },
        @{
            Name = 'owner session'
            Condition = ($rtOutputStickyPrefix +
                '\(requestOwnerSessionEpoch\s*=\s*0\)')
            Detail = 24
        },
        @{
            Name = 'I/O reference'
            Condition = ($rtOutputStickyPrefix +
                '\(requestIOReference\s*<>\s*' +
                'LMC_ECAT_IO_OUTPUT_REFERENCE\)')
            Detail = 28
        },
        @{
            Name = 'value high half'
            Condition = ($rtOutputStickyPrefix +
                '\(outputRequestValueHigh\s*<>\s*0\)')
            Detail = 30
        },
        @{
            Name = 'mask high half'
            Condition = ($rtOutputStickyPrefix +
                '\(outputMaskHigh\s*<>\s*0\)')
            Detail = 30
        },
        @{
            Name = 'reserved word 0'
            Condition = ($rtOutputStickyPrefix +
                '\(requestReserved0\s*<>\s*0\)')
            Detail = 24
        },
        @{
            Name = 'reserved word 1'
            Condition = ($rtOutputStickyPrefix +
                '\(requestReserved1\s*<>\s*0\)')
            Detail = 24
        },
        @{
            Name = 'live output validity'
            Condition = ($rtOutputStickyPrefix +
                '\(outputValid\s*=\s*FALSE\)')
            Detail = 31
        },
        @{
            Name = 'output revision'
            Condition = ($rtOutputStickyPrefix +
                '\(requestExpectedOutputRevision\s*<>\s*OutputRevision\)')
            Detail = 29
        },
        @{
            Name = 'nonzero mask'
            Condition = ($rtOutputStickyPrefix +
                '\(outputMaskValue\s*=\s*0\)')
            Detail = 30
        },
        @{
            Name = 'configured mask bounds'
            Condition = ($rtOutputStickyPrefix +
                '\(\(outputMaskValue\s+and\s+not\s+' +
                'LMC_ECAT_IO_OUTPUT_VALID_MASK\)\s*<>\s*0\)')
            Detail = 30
        },
        @{
            Name = 'value outside mask'
            Condition = ($rtOutputStickyPrefix +
                '\(\(outputRequestValue\s+and\s+not\s+' +
                'outputMaskValue\)\s*<>\s*0\)')
            Detail = 30
        })
    foreach ($rtOutputFailureGuard in $rtOutputFailureGuards) {
        Assert-LasalExactIfGuard `
            -Text $outputClaimThenArm `
            -ConditionPattern $rtOutputFailureGuard.Condition `
            -AssignmentPattern (
                'outputDetailCode\s*:=\s*' +
                $rtOutputFailureGuard.Detail + '\s*;') `
            -Owner ('LMCEcatInputLatch.RtWork ' +
                $rtOutputFailureGuard.Name + ' failure guard')
    }
    $rtRequestLocalSources = @(
        @{ Name = 'outputOperationToken'; Offset = 0 },
        @{ Name = 'requestTopologyRevision'; Offset = 4 },
        @{ Name = 'requestDiagnosticsBootId'; Offset = 8 },
        @{ Name = 'requestOwnerSessionEpoch'; Offset = 12 },
        @{ Name = 'requestIOReference'; Offset = 16 },
        @{ Name = 'outputRequestValue'; Offset = 20 },
        @{ Name = 'outputRequestValueHigh'; Offset = 24 },
        @{ Name = 'outputMaskValue'; Offset = 28 },
        @{ Name = 'outputMaskHigh'; Offset = 32 },
        @{ Name = 'requestExpectedOutputRevision'; Offset = 36 },
        @{ Name = 'requestReserved0'; Offset = 40 },
        @{ Name = 'requestReserved1'; Offset = 44 })
    foreach ($rtRequestLocalSource in $rtRequestLocalSources) {
        if ([regex]::Matches(
                $diagnosticsLatchRtWorkScanText,
                '\b' + $rtRequestLocalSource.Name +
                    '\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $outputClaimThenArmScanText,
                $rtRequestLocalSource.Name + '\s*:=\s*' +
                    'OutputRequestBytes\[' + $rtRequestLocalSource.Offset +
                    '\]\$UDINT\s*;').Count -ne 1) {
            throw ('LMCEcatInputLatch.RtWork request local ' +
                $rtRequestLocalSource.Name +
                ' must be assigned exactly once from its canonical mailbox offset.')
        }
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            '\bnewOutputValue\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $outputClaimThenArmScanText,
            ('newOutputValue\s*:=\s*' +
             '\(outputValue and not outputMaskValue\) or\s*' +
             '\(outputRequestValue and outputMaskValue\);')).Count -ne 1 -or
        $diagnosticsLatchRtWorkScanText -match
            ('(?is)(?:OutputRequestBytes\[[^\]]+\]\$' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)|' +
             '(?:_memset\(\s*dest|_memcpy\(\s*ptr1)\s*' +
             ':=#OutputRequestBytes\[)')) {
        throw ('LMCEcatInputLatch.RtWork must read the immutable request once, ' +
            'derive one canonical masked value, and never mutate request bytes.')
    }

    foreach ($outputByteIndex in 0..3) {
        $shiftPattern = if ($outputByteIndex -eq 0) {
            'newOutputValue'
        }
        else {
            '\(newOutputValue shr ' + (8 * $outputByteIndex) + '\)'
        }
        $writePattern = (
            'OutputSlot\.OutputS_Byte' + $outputByteIndex +
            '\.Write\(\s*input:=TO_DINT\(' + $shiftPattern +
            '\s*and\s*0x000000FF\)\)')
        if ([regex]::Matches(
                $diagnosticsLatchRtWorkBlock,
                $writePattern).Count -ne 1 -or
            [regex]::Matches(
                $outputClaimThenArm,
                $writePattern).Count -ne 1) {
            throw (
                "LMCEcatInputLatch.RtWork must write OutputS_Byte$outputByteIndex " +
                'exactly once with Byte0-LSB extraction inside the successful apply block.')
        }
    }
    $lasalProjectSourceRoot = Join-Path $root `
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis'
    $projectOutputWriteCallRecords = @()
    foreach ($lasalSourceFile in Get-ChildItem `
            -LiteralPath $lasalProjectSourceRoot `
            -Recurse `
            -File `
            -Filter '*.st') {
        $lasalSourceScanText = Get-LasalScanText (
            Get-Content -Raw -LiteralPath $lasalSourceFile.FullName)
        foreach ($projectOutputWriteCall in [regex]::Matches(
                $lasalSourceScanText,
                ('(?i)\b[A-Za-z_][A-Za-z0-9_]*\.' +
                 'OutputS_Byte[0-3]\.Write\s*\('))) {
            $projectOutputWriteCallRecords += @{
                Path = $lasalSourceFile.FullName
                Call = $projectOutputWriteCall.Value
            }
        }
    }
    if ($projectOutputWriteCallRecords.Count -ne 4 -or
        @($projectOutputWriteCallRecords | Where-Object {
                -not $_.Path.Equals(
                    $diagnosticsLatchPath,
                    [StringComparison]::OrdinalIgnoreCase)
            }).Count -ne 0) {
        throw ('IntegratedOutputOwnerDormant requires exactly four project-wide ' +
            'digital-output Write calls, all owned by LMCEcatInputLatch.RtWork.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            $advanceOutputRevisionCallPattern).Count -ne 2) {
        throw ('LMCEcatInputLatch.RtWork must advance OutputRevision exactly ' +
            'once for observed validity/value changes and once for a successful apply path.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'outputResult\s*:=\s*-1;').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'outputResult\s*:=\s*0;').Count -ne 1 -or
        [regex]::Matches(
            $outputClaimThenArm,
            'outputResult\s*:=\s*-1;').Count -ne 1 -or
        [regex]::Matches(
            $outputClaimThenArm,
            'outputResult\s*:=\s*0;').Count -ne 1) {
        throw ('LMCEcatInputLatch.RtWork must initialize every claimed output ' +
            'request as failure and change Result to zero only once after a ' +
            'successful physical apply.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            '\boutputResult\s*(?::=|[+\-*/]=)').Count -ne 2) {
        throw ('LMCEcatInputLatch.RtWork outputResult must have only its failure ' +
            'default and one success assignment; overwrite/compound mutation is forbidden.')
    }
    $outputApplyCandidates = @([regex]::Matches(
        $outputClaimThenArm,
        '(?s)if outputDetailCode = 0 then\s*' +
        '(?<Body>(?:(?!\bend_if;).)*)\s*end_if;') | Where-Object {
            $_.Groups['Body'].Value -match
                'OutputSlot\.OutputS_Byte0\.Write\('
        })
    if ($outputApplyCandidates.Count -ne 1) {
        throw ('LMCEcatInputLatch.RtWork must contain exactly one simple ' +
            'outputDetailCode=0 apply branch that dominates all physical writes.')
    }
    $outputApplyBody = $outputApplyCandidates[0].Groups['Body'].Value
    Assert-Match $outputApplyBody (
        '(?s)OutputSlot\.OutputS_Byte0\.Write\(.*?' +
        'OutputSlot\.OutputS_Byte1\.Write\(.*?' +
        'OutputSlot\.OutputS_Byte2\.Write\(.*?' +
        'OutputSlot\.OutputS_Byte3\.Write\(.*?' +
        'outputValue := newOutputValue;.*?' +
        'OutputPreviousValue := newOutputValue;.*?' +
        'OutputPreviousValid := TRUE;.*?' +
        'AdvanceOutputRevision\(\).*?' +
        'outputResult := 0;') (
        'LMCEcatInputLatch physical writes, software shadow, observation state, ' +
        'revision, and Result=0 are not dominated by the same success branch.')
    Assert-Match $outputClaimThenArm (
        '(?s)outputResult := 0;\s*end_if;\s*' +
        'if outputResult <> 0 then\s*' +
        'if outputDetailCode = 0 then\s*' +
        'outputDetailCode := 24;\s*end_if;\s*' +
        'else\s*outputDetailCode := 0;\s*end_if;') (
        'LMCEcatInputLatch must normalize every claimed request result/detail ' +
        'inside the successful READY claim branch.')
    $outputDetailMutations = [regex]::Matches(
        $outputClaimThenArmScanText,
        '\boutputDetailCode\s*(?::=|[+\-*/]=)')
    $canonicalOutputDetailAssignments = [regex]::Matches(
        $outputClaimThenArmScanText,
        '\boutputDetailCode\s*:=\s*(?:0|24|25|26|28|29|30|31);')
    if ($outputDetailMutations.Count -ne
            $canonicalOutputDetailAssignments.Count -or
        [regex]::Matches(
            $outputClaimThenArmScanText,
            '\boutputDetailCode\s*:=\s*0;').Count -ne 2) {
        throw ('LMCEcatInputLatch.RtWork outputDetailCode must remain sticky ' +
            'through validation: only canonical constant assignments are allowed, ' +
            'with zero used once at claim start and once after confirmed success.')
    }
    $outputCompletionPublishBlock = Get-UniqueLasalIfBlockContaining `
        -Text $diagnosticsLatchRtWorkBlock `
        -ConditionPattern 'outputRequestClaimed\s+then' `
        -RequiredPattern 'OutputCompletionBytes\[0\]\$UDINT' `
        -Message ('LMCEcatInputLatch completion publication must be guarded by ' +
            'the current-cycle outputRequestClaimed flag.')
    $outputCompletionPublishThenArm =
        Get-LasalFirstThenArm $outputCompletionPublishBlock
    $outputClaimIndex = $diagnosticsLatchRtWorkBlock.IndexOf(
        $outputClaimBlock,
        [StringComparison]::Ordinal)
    $outputRequestResetMatch = [regex]::Match(
        $diagnosticsLatchRtWorkBlock,
        'outputRequestClaimed\s*:=\s*FALSE;')
    $outputCompletionPublishIndex = $diagnosticsLatchRtWorkBlock.IndexOf(
        $outputCompletionPublishBlock,
        [StringComparison]::Ordinal)
    $recorderAppendMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        'RecorderStore\.AppendSnapshot\(')
    $stateReadyMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        'state\s*:=\s*READY;')
    $recorderGuardMatches = [regex]::Matches(
        $diagnosticsLatchRtWorkBlock,
        'if IsClientConnected\(#RecorderStore\) then')
    if ($outputClaimIndex -lt 0 -or
        $outputCompletionPublishIndex -lt 0 -or
        -not $outputRequestResetMatch.Success -or
        $outputRequestResetMatch.Index -ge $outputClaimIndex -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $outputRequestResetMatch.Index) -ne 0 -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $outputClaimIndex) -ne 0 -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $outputCompletionPublishIndex) -ne 0 -or
        $recorderAppendMatches.Count -ne 1 -or
        $stateReadyMatches.Count -ne 1 -or
        $recorderGuardMatches.Count -ne 1 -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $recorderGuardMatches[0].Index) -ne 0 -or
        (Get-LasalControlDepthAtIndex `
            -Text $diagnosticsLatchRtWorkBlock `
            -Index $stateReadyMatches[0].Index) -ne 0 -or
        $recorderGuardMatches[0].Index -le $writerCloseMatch.Index -or
        $recorderAppendMatches[0].Index -le $recorderGuardMatches[0].Index -or
        $stateReadyMatches[0].Index -le $recorderAppendMatches[0].Index -or
        $outputCompletionPublishIndex -le $recorderAppendMatches[0].Index -or
        $outputCompletionPublishIndex -le $stateReadyMatches[0].Index -or
        $outputClaimIndex -le $sourcePreparationEndIndex -or
        ($outputClaimIndex + $outputClaimBlock.Length) -ge
            $firstExtendedWriteMatch.Index -or
        $outputCompletionPublishIndex -le $writerCloseMatch.Index) {
        throw ('LMCEcatInputLatch must validate/apply from current-cycle source ' +
            'quality after preparation and before the first extended snapshot ' +
            'write, then close the unconditional writer and preserve recorder/' +
            'READY tail work before publishing the separately claimed completion.')
    }
    $rtWorkBeforeCompletionPublish = $diagnosticsLatchRtWorkBlock.Substring(
        0,
        $outputCompletionPublishIndex)
    if ((Get-LasalScanText $rtWorkBeforeCompletionPublish) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch RtWork must always close the snapshot writer, ' +
            'append the legacy recorder image, publish READY, and reach the ' +
            'completion-publication branch before any RETURN.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'outputRequestClaimed\s*:=\s*FALSE;').Count -ne 1 -or
        [regex]::Matches(
            $outputClaimThenArm,
            'outputRequestClaimed\s*:=\s*TRUE;').Count -ne 1) {
        throw ('LMCEcatInputLatch must reset outputRequestClaimed every cycle ' +
            'and set it once only after a successful READY claim.')
    }
    Assert-Match $outputCompletionPublishThenArm (
        '(?s)' +
        'previousMailboxState := sigclib_atomic_cmpxchgU32\(\s*' +
        'pValue:=#OutputMailboxState,\s*' +
        'cmpVal:=LMC_ECAT_IO_MAILBOX_RUNNING,\s*' +
        'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_COMPLETION\);\s*' +
        'if previousMailboxState <> LMC_ECAT_IO_MAILBOX_RUNNING then\s*' +
        'RETURN;\s*end_if;.*?' +
        'completionSequence := sigclib_atomic_getU32\(' +
        'pValue:=#OutputCompletionSequence\) \+ 1;.*?' +
        'if completionSequence = 0 then\s*completionSequence := 1;\s*end_if;.*?' +
        'if \(completionSequence and 1\) = 0 then\s*' +
        'completionSequence \+= 1;\s*end_if;.*?' +
        'sigclib_atomic_setU32\(pValue:=#OutputCompletionSequence,\s*' +
        'value:=completionSequence\);.*?' +
        'OutputCompletionBytes\[0\]\$UDINT := outputOperationToken;.*?' +
        'OutputCompletionBytes\[4\]\$DINT := outputResult;.*?' +
        'OutputCompletionBytes\[8\]\$UDINT := outputDetailCode;.*?' +
        'OutputCompletionBytes\[12\]\$UDINT := cycleCounter;.*?' +
        'OutputCompletionBytes\[16\]\$UDINT := OutputRevision;.*?' +
        'OutputCompletionBytes\[20\]\$UDINT := outputValue;.*?' +
        'OutputCompletionBytes\[24\]\$UDINT := 0;.*?' +
        'OutputCompletionBytes\[28\]\$UDINT := 0;.*?' +
        'finalCompletionSequence := completionSequence \+ 1;.*?' +
        'if finalCompletionSequence = 0 then\s*' +
        'finalCompletionSequence := 2;\s*end_if;.*?' +
        'sigclib_atomic_setU32\(pValue:=#OutputCompletionSequence,\s*' +
        'value:=finalCompletionSequence\);.*?' +
        'sigclib_atomic_setU32\(pValue:=#OutputMailboxState,\s*' +
        'value:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY\)') (
        'LMCEcatInputLatch must publish each claimed exact-token completion ' +
        'through Running/WritingCompletion, a nonzero even seqlock, and an ' +
        'exact zero-reserved 32-byte payload after the snapshot writer closes.')

    $outputCompletionFields = @(
        @{ Offset = 0; Type = 'UDINT'; Value = 'outputOperationToken' },
        @{ Offset = 4; Type = 'DINT'; Value = 'outputResult' },
        @{ Offset = 8; Type = 'UDINT'; Value = 'outputDetailCode' },
        @{ Offset = 12; Type = 'UDINT'; Value = 'cycleCounter' },
        @{ Offset = 16; Type = 'UDINT'; Value = 'OutputRevision' },
        @{ Offset = 20; Type = 'UDINT'; Value = 'outputValue' },
        @{ Offset = 24; Type = 'UDINT'; Value = '0' },
        @{ Offset = 28; Type = 'UDINT'; Value = '0' })
    foreach ($outputCompletionField in $outputCompletionFields) {
        $allCompletionFieldWrites = [regex]::Matches(
            $diagnosticsLatchRtWorkBlock,
            'OutputCompletionBytes\[' + $outputCompletionField.Offset +
                '\]\$[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)' )
        $canonicalCompletionFieldWrites = [regex]::Matches(
            $outputCompletionPublishThenArm,
            'OutputCompletionBytes\[' + $outputCompletionField.Offset +
                '\]\$' + $outputCompletionField.Type + '\s*:=\s*' +
                $outputCompletionField.Value + '\s*;')
        if ($allCompletionFieldWrites.Count -ne 1 -or
            $canonicalCompletionFieldWrites.Count -ne 1) {
            throw ('LMCEcatInputLatch completion offset ' +
                $outputCompletionField.Offset +
                ' must have exactly one canonical write inside publication.')
        }
    }
    $outputPublishClaimFailureBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputCompletionPublishThenArm `
        -ConditionPattern (
            'previousMailboxState\s*<>\s*' +
            'LMC_ECAT_IO_MAILBOX_RUNNING\s+then') `
        -RequiredPattern 'RETURN;' `
        -Message ('LMCEcatInputLatch completion publication CAS-failure ' +
            'guard was not found.')
    $outputPublishClaimFailureArm =
        Get-LasalFirstThenArm $outputPublishClaimFailureBlock
    if ([regex]::Matches(
            (Get-LasalScanText $outputPublishClaimFailureArm),
            '(?i)\bRETURN\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch completion CAS-failure true branch must ' +
            'RETURN exactly once.')
    }
    $outputPublishClaimFailureIndex = $outputCompletionPublishThenArm.IndexOf(
        $outputPublishClaimFailureBlock,
        [StringComparison]::Ordinal)
    $outputCompletionReadyPublishMatch = [regex]::Match(
        $outputCompletionPublishThenArm,
        '(?s)sigclib_atomic_setU32\(pValue:=#OutputMailboxState,\s*' +
            'value:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY\)')
    if ($outputPublishClaimFailureIndex -lt 0 -or
        -not $outputCompletionReadyPublishMatch.Success -or
        ($outputPublishClaimFailureIndex +
            $outputPublishClaimFailureBlock.Length) -ge
            $outputCompletionReadyPublishMatch.Index) {
        throw ('LMCEcatInputLatch completion publication critical interval ' +
            'was not found.')
    }
    $outputPublishCriticalStart = $outputPublishClaimFailureIndex +
        $outputPublishClaimFailureBlock.Length
    $outputPublishCriticalSection = $outputCompletionPublishThenArm.Substring(
        $outputPublishCriticalStart,
        ($outputCompletionReadyPublishMatch.Index +
            $outputCompletionReadyPublishMatch.Length) -
            $outputPublishCriticalStart)
    if ((Get-LasalScanText $outputPublishCriticalSection) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch completion publication must not RETURN after ' +
            'claiming WRITING_COMPLETION and before publishing COMPLETION_READY.')
    }
    $outputCompletionPublishScanText =
        Get-LasalScanText $outputCompletionPublishThenArm
    if ([regex]::Matches(
            $outputCompletionPublishScanText,
            ('OutputCompletionBytes\[[^\]]+\]\$' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')).Count -ne 8 -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)(?:_memset\(\s*dest|_memcpy\(\s*ptr1)' +
             ':=#OutputCompletionBytes\[')).Count -ne 0) {
        throw ('LMCEcatInputLatch completion publication must contain only the ' +
            'eight canonical typed field writes and no aggregate overwrite.')
    }
    if ($outputCompletionPublishScanText -notmatch
            ('(?is)sigclib_atomic_setU32\(pValue:=#OutputMailboxState,\s*' +
             'value:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY\)\s*$') -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\(\s*' +
             'pValue:=#OutputMailboxState')).Count -ne 2 -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_RUNNING,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_COMPLETION\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'value:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY\)')).Count -ne 1 -or
        $outputCompletionPublishScanText -match
            '(?i)#?OutputMailboxState\s*(?::=|[+\-*/]=)' ) {
        throw ('LMCEcatInputLatch completion publication may mutate mailbox ' +
            'state only through RUNNING-to-WRITING_COMPLETION and the final ' +
            'COMPLETION_READY publish.')
    }
    if ([regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputCompletionSequence')).Count -ne 2 -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputCompletionSequence,\s*' +
             'value:=completionSequence\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCompletionPublishScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputCompletionSequence,\s*' +
             'value:=finalCompletionSequence\)')).Count -ne 1 -or
        $outputCompletionPublishScanText -match
            '(?i)#?OutputCompletionSequence\s*(?::=|[+\-*/]=)' ) {
        throw ('LMCEcatInputLatch completion publication must write sequence ' +
            'exactly twice: canonical odd open and final nonzero even close.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            '\bcompletionSequence\s*(?::=|[+\-*/]=)').Count -ne 3 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('completionSequence\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#OutputCompletionSequence\)\s*\+\s*1;')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            'completionSequence\s*:=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            'completionSequence\s*\+=\s*1;').Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            '\bfinalCompletionSequence\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('finalCompletionSequence\s*:=\s*' +
             'completionSequence\s*\+\s*1;')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            'finalCompletionSequence\s*:=\s*2;').Count -ne 1) {
        throw ('LMCEcatInputLatch completion sequence locals may mutate only ' +
            'through the canonical current+1, nonzero odd correction, and ' +
            'final nonzero even correction before atomic publication.')
    }
    if ([regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\(\s*' +
             'pValue:=#OutputMailboxState')).Count -ne 3 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('(?is)previousMailboxState\s*:=\s*' +
             'sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_READY,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_RUNNING\)')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('(?is)previousMailboxState\s*:=\s*' +
             'sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_RUNNING,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_COMPLETION\)')).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            '\bpreviousMailboxState\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputCompletionSequence')).Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            ('OutputCompletionBytes\[[^\]]+\]\$' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')).Count -ne 8 -or
        $diagnosticsLatchRtWorkScanText -match
            ('(?is)(?:_memset\(\s*dest|_memcpy\(\s*ptr1)\s*' +
             ':=#OutputCompletionBytes\[')) {
        throw ('LMCEcatInputLatch.RtWork must own exactly the canonical ' +
            'READY-to-RUNNING claim, completion claim/publish, two completion ' +
            'sequence writes, and eight payload writes across the whole method.')
    }

    Assert-Match $outputCopyCompletionBlock (
        '(?s)Result := -2;\s*' +
        'if \(pDest = NIL\) \| \(DestSize < 32\) then\s*' +
        'RETURN;\s*end_if;\s*' +
        'if sigclib_atomic_getU32\(pValue:=#OutputMailboxState\) <>\s*' +
        'LMC_ECAT_IO_MAILBOX_COMPLETION_READY.*?RETURN;.*?' +
        'sequenceBefore := sigclib_atomic_getU32\(' +
        'pValue:=#OutputCompletionSequence\);.*?' +
        '\(sequenceBefore <> 0\) &\s*' +
        '\(\(sequenceBefore and 1\) = 0\).*?' +
        '_memcpy\(ptr1:=pDest,\s*ptr2:=#OutputCompletionBytes\[0\],\s*' +
        'cntr:=32\);.*?' +
        'sequenceAfter := sigclib_atomic_getU32\(' +
        'pValue:=#OutputCompletionSequence\);.*?' +
        '\(sequenceBefore = sequenceAfter\) &\s*' +
        '\(sequenceAfter <> 0\).*?' +
        'pDest\^\$UDINT <> ExpectedToken.*?RETURN;.*?' +
        'previousMailboxState := sigclib_atomic_cmpxchgU32\(\s*' +
        'pValue:=#OutputMailboxState,\s*' +
        'cmpVal:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY,\s*' +
        'newVal:=LMC_ECAT_IO_MAILBOX_IDLE\);\s*' +
        'if previousMailboxState <> LMC_ECAT_IO_MAILBOX_COMPLETION_READY then\s*' +
        'RETURN;\s*end_if;.*?' +
        'Result := 0;') (
        'LMCEcatInputLatch completion consumer does not use an exact-token ' +
        'bounded seqlock and successful CAS before releasing CompletionReady to Idle.')
    if ([regex]::Matches($outputCopyCompletionBlock, 'Result\s*:=\s*-2;').Count -ne 1 -or
        [regex]::Matches($outputCopyCompletionBlock, 'Result\s*:=\s*0;').Count -ne 1 -or
        [regex]::Matches(
            (Get-LasalScanText $outputCopyCompletionBlock),
            '\bResult\s*(?::=|[+\-*/]=)').Count -ne 2) {
        throw ('LMCEcatInputLatch.CopyOutputCompletion must default to pending/' +
            'failure and return success exactly once after exact-token consume.')
    }
    $copyCompletionInitialCoherenceBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputCopyCompletionBlock `
        -ConditionPattern (
            '\(sequenceBefore\s*<>\s*0\)\s*&\s*' +
            '\(\(sequenceBefore\s+and\s+1\)\s*=\s*0\)\s+then') `
        -RequiredPattern '_memcpy\(' `
        -Message ('LMCEcatInputLatch.CopyOutputCompletion initial even-sequence ' +
            'guard was not found.')
    $copyCompletionInitialCoherenceArm =
        Get-LasalFirstThenArm $copyCompletionInitialCoherenceBlock
    $copyCompletionStableCoherenceBlock = Get-UniqueLasalIfBlockContaining `
        -Text $copyCompletionInitialCoherenceArm `
        -ConditionPattern (
            '\(sequenceBefore\s*=\s*sequenceAfter\)\s*&\s*' +
            '\(sequenceAfter\s*<>\s*0\)\s*&\s*' +
            '\(\(sequenceAfter\s+and\s+1\)\s*=\s*0\)\s+then') `
        -RequiredPattern 'sigclib_atomic_cmpxchgU32\(' `
        -Message ('LMCEcatInputLatch.CopyOutputCompletion stable-sequence ' +
            'guard was not found.')
    $copyCompletionStableCoherenceArm =
        Get-LasalFirstThenArm $copyCompletionStableCoherenceBlock
    foreach ($coherentCompletionStep in @(
            @{ Pattern = '_memcpy\('; Whole = 1; Initial = 1; Stable = 0 },
            @{
                Pattern = 'sequenceAfter\s*:=\s*sigclib_atomic_getU32\('
                Whole = 1; Initial = 1; Stable = 0
            },
            @{
                Pattern = 'pDest\^\$UDINT\s*<>\s*ExpectedToken'
                Whole = 1; Initial = 1; Stable = 1
            },
            @{
                Pattern = ('sigclib_atomic_cmpxchgU32\(\s*' +
                    'pValue:=#OutputMailboxState')
                Whole = 1; Initial = 1; Stable = 1
            },
            @{
                Pattern = 'Result\s*:=\s*0;'
                Whole = 1; Initial = 1; Stable = 1
            })) {
        if ([regex]::Matches(
                $outputCopyCompletionBlock,
                $coherentCompletionStep.Pattern).Count -ne
                $coherentCompletionStep.Whole -or
            [regex]::Matches(
                $copyCompletionInitialCoherenceArm,
                $coherentCompletionStep.Pattern).Count -ne
                $coherentCompletionStep.Initial -or
            [regex]::Matches(
                $copyCompletionStableCoherenceArm,
                $coherentCompletionStep.Pattern).Count -ne
                $coherentCompletionStep.Stable) {
            throw ('LMCEcatInputLatch.CopyOutputCompletion coherent step ' +
                "'$($coherentCompletionStep.Pattern)' is outside its required " +
                'nonzero-even/stable-sequence true arm.')
        }
    }
    $copyCompletionClaimFailureBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputCopyCompletionBlock `
        -ConditionPattern (
            'previousMailboxState\s*<>\s*' +
            'LMC_ECAT_IO_MAILBOX_COMPLETION_READY\s+then') `
        -RequiredPattern 'RETURN;' `
        -Message ('LMCEcatInputLatch.CopyOutputCompletion CAS-failure guard ' +
            'was not found.')
    $copyCompletionClaimFailureArm =
        Get-LasalFirstThenArm $copyCompletionClaimFailureBlock
    if ([regex]::Matches(
            (Get-LasalScanText $copyCompletionClaimFailureArm),
            '(?i)\bRETURN\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch.CopyOutputCompletion CAS-failure true branch ' +
            'must RETURN exactly once.')
    }
    $copyCompletionClaimFailureIndex = $outputCopyCompletionBlock.IndexOf(
        $copyCompletionClaimFailureBlock,
        [StringComparison]::Ordinal)
    $copyCompletionSuccessMatch = [regex]::Match(
        $outputCopyCompletionBlock,
        'Result\s*:=\s*0;')
    if ($copyCompletionClaimFailureIndex -lt 0 -or
        -not $copyCompletionSuccessMatch.Success -or
        ($copyCompletionClaimFailureIndex +
            $copyCompletionClaimFailureBlock.Length) -ge
            $copyCompletionSuccessMatch.Index) {
        throw ('LMCEcatInputLatch.CopyOutputCompletion post-CAS success ' +
            'interval was not found.')
    }
    $copyCompletionSuccessStart = $copyCompletionClaimFailureIndex +
        $copyCompletionClaimFailureBlock.Length
    $copyCompletionSuccessSection = $outputCopyCompletionBlock.Substring(
        $copyCompletionSuccessStart,
        ($copyCompletionSuccessMatch.Index +
            $copyCompletionSuccessMatch.Length) -
            $copyCompletionSuccessStart)
    if ((Get-LasalScanText $copyCompletionSuccessSection) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch.CopyOutputCompletion must not RETURN after ' +
            'releasing COMPLETION_READY to IDLE and before reporting success.')
    }
    $outputCopyCompletionScanText = Get-LasalScanText $outputCopyCompletionBlock
    if ($outputCopyCompletionScanText -notmatch
            '(?is)Result\s*:=\s*0;\s*END_FUNCTION\s*$' -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\(\s*' +
             'pValue:=#OutputMailboxState')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('(?is)sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_COMPLETION_READY,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_IDLE\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            '\bpreviousMailboxState\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            '\bsequenceBefore\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('sequenceBefore\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#OutputCompletionSequence\);')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            '\bsequenceAfter\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('sequenceAfter\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#OutputCompletionSequence\);')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('(?is)_memcpy\s*\(\s*ptr1\s*:=\s*pDest\s*,\s*' +
             'ptr2\s*:=\s*#OutputCompletionBytes\s*\[\s*0\s*\]\s*,\s*' +
             'cntr\s*:=\s*32\s*\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
             '(?:dest|ptr1)\s*:=\s*' +
             '(?:pDest\b|\(\s*pDest\s*\+\s*[^\)]+\))')).Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            '(?i)\bpDest\b').Count -ne 4 -or
        $outputCopyCompletionScanText -match
            ('(?i)(?:\bpDest\b|' +
             '\(\s*pDest\s*\+\s*[^\)]+\))\s*\^\s*\$\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)') -or
        $outputCopyCompletionScanText -match
            '(?i)\bpDest\b\s*(?::=|[+\-*/]=)' -or
        $outputCopyCompletionScanText -match
            '(?i)#?OutputMailboxState\s*(?::=|[+\-*/]=)' ) {
        throw ('LMCEcatInputLatch.CopyOutputCompletion must mutate mailbox state ' +
            'only through the stable exact-token COMPLETION_READY-to-IDLE CAS ' +
            'after one coherent immutable destination copy, then end immediately ' +
            'after Result=0 without sequence or destination overwrite.')
    }
    Assert-Match $outputCancelBlock (
        '(?s)Result := -2;\s*' +
        'if sigclib_atomic_getU32\(pValue:=#OutputMailboxState\) <>\s*' +
        'LMC_ECAT_IO_MAILBOX_READY.*?RETURN;.*?' +
        'OutputRequestBytes\[0\]\$UDINT <> ExpectedToken.*?RETURN;.*?' +
        'previousMailboxState := sigclib_atomic_cmpxchgU32\(\s*' +
        'pValue:=#OutputMailboxState,\s*' +
        'cmpVal:=LMC_ECAT_IO_MAILBOX_READY,\s*' +
        'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_REQUEST\);\s*' +
        'if previousMailboxState <> LMC_ECAT_IO_MAILBOX_READY then\s*' +
        'RETURN;\s*end_if;.*?' +
        'sigclib_atomic_setU32\(pValue:=#OutputMailboxState,\s*' +
        'value:=LMC_ECAT_IO_MAILBOX_IDLE\);\s*' +
        'Result := 0;') (
        'LMCEcatInputLatch queued cancel does not exact-match and atomically ' +
        'claim Ready before returning the mailbox to Idle and reporting success.')
    if ([regex]::Matches($outputCancelBlock, 'Result\s*:=\s*-2;').Count -ne 1 -or
        [regex]::Matches($outputCancelBlock, 'Result\s*:=\s*0;').Count -ne 1 -or
        [regex]::Matches(
            (Get-LasalScanText $outputCancelBlock),
            '\bResult\s*(?::=|[+\-*/]=)').Count -ne 2) {
        throw ('LMCEcatInputLatch.CancelQueuedOutput may report success only once ' +
            'after the exact-token READY claim returns the mailbox to Idle.')
    }
    $cancelOutputClaimFailureBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputCancelBlock `
        -ConditionPattern (
            'previousMailboxState\s*<>\s*' +
            'LMC_ECAT_IO_MAILBOX_READY\s+then') `
        -RequiredPattern 'RETURN;' `
        -Message ('LMCEcatInputLatch.CancelQueuedOutput CAS-failure guard ' +
            'was not found.')
    $cancelOutputClaimFailureArm =
        Get-LasalFirstThenArm $cancelOutputClaimFailureBlock
    if ([regex]::Matches(
            (Get-LasalScanText $cancelOutputClaimFailureArm),
            '(?i)\bRETURN\s*;').Count -ne 1) {
        throw ('LMCEcatInputLatch.CancelQueuedOutput CAS-failure true branch ' +
            'must RETURN exactly once.')
    }
    $cancelOutputClaimFailureIndex = $outputCancelBlock.IndexOf(
        $cancelOutputClaimFailureBlock,
        [StringComparison]::Ordinal)
    $cancelOutputSuccessMatch = [regex]::Match(
        $outputCancelBlock,
        'Result\s*:=\s*0;')
    if ($cancelOutputClaimFailureIndex -lt 0 -or
        -not $cancelOutputSuccessMatch.Success -or
        ($cancelOutputClaimFailureIndex +
            $cancelOutputClaimFailureBlock.Length) -ge
            $cancelOutputSuccessMatch.Index) {
        throw ('LMCEcatInputLatch.CancelQueuedOutput post-CAS success interval ' +
            'was not found.')
    }
    $cancelOutputSuccessStart = $cancelOutputClaimFailureIndex +
        $cancelOutputClaimFailureBlock.Length
    $cancelOutputSuccessSection = $outputCancelBlock.Substring(
        $cancelOutputSuccessStart,
        ($cancelOutputSuccessMatch.Index + $cancelOutputSuccessMatch.Length) -
            $cancelOutputSuccessStart)
    if ((Get-LasalScanText $cancelOutputSuccessSection) -match
        '(?i)\bRETURN\s*;') {
        throw ('LMCEcatInputLatch.CancelQueuedOutput must not RETURN after ' +
            'claiming READY and before returning the mailbox to IDLE/success.')
    }
    $outputCancelScanText = Get-LasalScanText $outputCancelBlock
    if ($outputCancelScanText -notmatch
            '(?is)Result\s*:=\s*0;\s*END_FUNCTION\s*$' -or
        [regex]::Matches(
            $outputCancelScanText,
            ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\(\s*' +
             'pValue:=#OutputMailboxState')).Count -ne 2 -or
        [regex]::Matches(
            $outputCancelScanText,
            ('(?is)sigclib_atomic_cmpxchgU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'cmpVal:=LMC_ECAT_IO_MAILBOX_READY,\s*' +
             'newVal:=LMC_ECAT_IO_MAILBOX_WRITING_REQUEST\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCancelScanText,
            ('(?is)sigclib_atomic_setU32\(\s*' +
             'pValue:=#OutputMailboxState,\s*' +
             'value:=LMC_ECAT_IO_MAILBOX_IDLE\)')).Count -ne 1 -or
        [regex]::Matches(
            $outputCancelScanText,
            '\bpreviousMailboxState\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        $outputCancelScanText -match
            '(?i)#?OutputMailboxState\s*(?::=|[+\-*/]=)' ) {
        throw ('LMCEcatInputLatch.CancelQueuedOutput must mutate mailbox state ' +
            'only through READY-to-WRITING_REQUEST and final IDLE, then end ' +
            'immediately after Result=0.')
    }
    Assert-Match $outputReusableBlock (
        '(?s)Ready := sigclib_atomic_getU32\(' +
        'pValue:=#OutputMailboxState\) = LMC_ECAT_IO_MAILBOX_IDLE') (
        'LMCEcatInputLatch.IsOutputReusable must expose only the atomic Idle state.')
    $outputReusableScanText = Get-LasalScanText $outputReusableBlock
    if ([regex]::Matches(
            $outputReusableScanText,
            '\bReady\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $outputReusableScanText,
            ('Ready\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#OutputMailboxState\)\s*=\s*' +
             'LMC_ECAT_IO_MAILBOX_IDLE;')).Count -ne 1 -or
        $outputReusableScanText -notmatch
            ('(?is)Ready\s*:=\s*sigclib_atomic_getU32\(' +
             'pValue:=#OutputMailboxState\)\s*=\s*' +
             'LMC_ECAT_IO_MAILBOX_IDLE;\s*END_FUNCTION\s*$')) {
        throw ('LMCEcatInputLatch.IsOutputReusable must assign Ready exactly ' +
            'once from the atomic IDLE comparison as its last statement.')
    }

    $classMailboxStateMutationPattern =
        ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*OutputMailboxState\b')
    $classMailboxStateAccessPattern =
        ('(?is)sigclib_atomic_(?:get|cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*OutputMailboxState\b')
    $mailboxStateOwnerSpecs = @(
        @{ Name = 'TryQueueOutputWrite'; Text = $outputTryQueueScanText; Count = 2 },
        @{ Name = 'RtWork'; Text = $diagnosticsLatchRtWorkScanText; Count = 3 },
        @{ Name = 'CopyOutputCompletion'; Text = $outputCopyCompletionScanText; Count = 1 },
        @{ Name = 'CancelQueuedOutput'; Text = $outputCancelScanText; Count = 2 })
    foreach ($mailboxStateOwnerSpec in $mailboxStateOwnerSpecs) {
        if ([regex]::Matches(
                $mailboxStateOwnerSpec.Text,
                $classMailboxStateMutationPattern).Count -ne
                $mailboxStateOwnerSpec.Count) {
            throw ('LMCEcatInputLatch ' + $mailboxStateOwnerSpec.Name +
                ' mailbox atomic mutation count must be exactly ' +
                $mailboxStateOwnerSpec.Count + '.')
        }
    }
    if ([regex]::Matches(
            $diagnosticsLatchScanText,
            $classMailboxStateMutationPattern).Count -ne 8 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*OutputMailboxState\b').Count -ne
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $classMailboxStateAccessPattern).Count -or
        $diagnosticsLatchScanText -match
            '(?i)#?OutputMailboxState\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch OutputMailboxState may be mutated only by ' +
            'the canonical producer, RT owner, completion consumer, and queued ' +
            'cancel atomic transitions; direct or extra class-wide writes are forbidden.')
    }

    $completionSequenceSetPattern =
        ('(?is)sigclib_atomic_setU32\(\s*' +
         'pValue:=#OutputCompletionSequence')
    $completionSequenceAtomicMutationPattern =
        ('(?is)sigclib_atomic_(?:cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*OutputCompletionSequence\b')
    $completionSequenceAtomicAccessPattern =
        ('(?is)sigclib_atomic_(?:get|cmpxchg|set)U32\s*\(\s*' +
         'pValue\s*:=\s*#?\s*OutputCompletionSequence\b')
    if ([regex]::Matches(
            $diagnosticsLatchScanText,
            $completionSequenceAtomicMutationPattern).Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            $completionSequenceAtomicMutationPattern).Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            $completionSequenceSetPattern).Count -ne 2 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*OutputCompletionSequence\b').Count -ne
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $completionSequenceAtomicAccessPattern).Count -or
        $diagnosticsLatchScanText -match
            '(?i)#?OutputCompletionSequence\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch OutputCompletionSequence may be mutated only ' +
            'by the two canonical RtWork odd/even atomic publications.')
    }

    $requestTypedMutationPattern =
        ('(?i)(?<![A-Za-z0-9_])#?\s*OutputRequestBytes\s*' +
         '\[\s*[^\]]+\]\s*\$\s*' +
         '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
    $requestAggregateMutationPattern =
        ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
         '(?:dest|ptr1)\s*:=\s*#?OutputRequestBytes\s*\[[^\]]+\]')
    if ([regex]::Matches(
            $diagnosticsLatchScanText,
            $requestTypedMutationPattern).Count -ne 10 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            $requestTypedMutationPattern).Count -ne 10 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $requestAggregateMutationPattern).Count -ne 1 -or
        [regex]::Matches(
            $outputTryQueueScanText,
            $requestAggregateMutationPattern).Count -ne 1 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*OutputRequestBytes\b').Count -ne 1 -or
        $diagnosticsLatchScanText -match
            '(?i)#?OutputRequestBytes\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch OutputRequestBytes may be mutated only by ' +
            'TryQueueOutputWrite canonical zeroing and ten typed field writes.')
    }

    $completionTypedMutationPattern =
        ('(?i)(?<![A-Za-z0-9_])#?\s*OutputCompletionBytes\s*' +
         '\[\s*[^\]]+\]\s*\$\s*' +
         '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
    $completionAggregateMutationPattern =
        ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
         '(?:dest|ptr1)\s*:=\s*#?OutputCompletionBytes\s*\[[^\]]+\]')
    if ([regex]::Matches(
            $diagnosticsLatchScanText,
            $completionTypedMutationPattern).Count -ne 8 -or
        [regex]::Matches(
            $diagnosticsLatchRtWorkScanText,
            $completionTypedMutationPattern).Count -ne 8 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            $completionAggregateMutationPattern).Count -ne 0 -or
        [regex]::Matches(
            $diagnosticsLatchScanText,
            '(?i)#\s*OutputCompletionBytes\b').Count -ne 1 -or
        [regex]::Matches(
            $outputCopyCompletionScanText,
            ('(?is)_memcpy\s*\(\s*ptr1\s*:=\s*pDest\s*,\s*' +
             'ptr2\s*:=\s*#\s*OutputCompletionBytes\s*\[\s*0\s*\]')).Count -ne 1 -or
        $diagnosticsLatchScanText -match
            '(?i)#?OutputCompletionBytes\s*(?::=|[+\-*/]=)') {
        throw ('LMCEcatInputLatch OutputCompletionBytes may be mutated only by ' +
            'the eight canonical RtWork publication fields; readers and cancel ' +
            'paths must keep both mailbox payloads immutable.')
    }
}

$outputWritePolicyBlock = [regex]::Match(
    $diagnosticsTopologyIoModels,
    '(?ms)^[ \t]{4}internal static class ' +
    'LMCDiagnosticsDigitalOutputWritePolicy\s*\{.*?' +
    '^[ \t]{4}\}').Value
if ([string]::IsNullOrWhiteSpace($outputWritePolicyBlock)) {
    throw 'SDK digital-output write policy class was not found.'
}
Assert-Match $outputWritePolicyBlock (
    '(?s)ApprovedIOReferences\s*=\s*(?:' +
    'new ReadOnlyCollection<uint>\((?:new uint\[0\]|Array\.Empty<uint>\(\))\)|' +
    'Array\.AsReadOnly\(new uint\[0\]\)|Array\.Empty<uint>\(\))') (
    'SDK digital-output IOReference allowlist must remain semantically empty ' +
    'before the live output safety matrix passes.')
Assert-Match $outputWritePolicyBlock (
    '(?s)GetApprovedIOReferences\(\).*?' +
    'return ApprovedIOReferences;.*?' +
    'IsApproved\(uint ioReference\).*?' +
    'return ApprovedIOReferences\.Contains\(ioReference\);') (
    'SDK digital-output policy does not use the same immutable empty allowlist ' +
    'for disclosure and authorization.')
Assert-Match $diagnosticsTopologyIo (
    '(?s)public LMCOperationTicket SubmitDigitalOutputWrite\(.*?' +
    'return SubmitDigitalOutputWrite\(.*?' +
    'LMCDiagnosticsDigitalOutputWritePolicy\.IsApproved') (
    'SDK public synchronous digital-output submit path does not use the compile-time policy.')
Assert-Match $diagnosticsTopologyIo (
    '(?s)public async Task<LMCOperationTicket> SubmitDigitalOutputWriteAsync\(.*?' +
    'return await SubmitDigitalOutputWriteAsync\(.*?' +
    'LMCDiagnosticsDigitalOutputWritePolicy\.IsApproved') (
    'SDK public asynchronous digital-output submit path does not use the compile-time policy.')

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
Assert-Match $sdoExecutor '(?s)#define LMC_SDO_EXEC_IDLE\s+0.*?#define LMC_SDO_EXEC_ARMING\s+1.*?#define LMC_SDO_EXEC_RUNNING\s+2.*?#define LMC_SDO_EXEC_RESULT_READY\s+3.*?#define LMC_SDO_EXEC_ORPHANED\s+4.*?#define LMC_SDO_EXEC_QUARANTINED\s+5.*?#define LMC_SDO_EXEC_RELEASING\s+6' 'LMCSdoExecutor atomic state constants are incomplete.'
Assert-Match $sdoExecutor '(?s)ActiveLength\s*:\s*UINT;.*?ActiveIsWrite\s*:\s*BOOL;.*?ReadBuffer\s*:\s*ARRAY \[0\.\.3\] OF USINT;.*?WriteBuffer\s*:\s*ARRAY \[0\.\.3\] OF USINT;' 'LMCSdoExecutor does not retain separate four-byte read/write buffers and the active direction.'
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
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::TryStartRead.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoTryStartBlock)) {
    throw 'LMCSdoExecutor.TryStartRead implementation was not found.'
}
Assert-Match $sdoTryStartBlock '(?s)ObjectIndex\s*:\s*UINT;.*?SubIndex\s*:\s*USINT;.*?ReadLength\s*:\s*UINT;.*?ReadLength <> 1.*?ReadLength <> 2.*?ReadLength <> 4.*?sigclib_atomic_cmpxchgU32\(\s*pValue:=#AdapterState,\s*cmpVal:=LMC_SDO_EXEC_IDLE,\s*newVal:=LMC_SDO_EXEC_ARMING\).*?ActiveIndex := ObjectIndex;.*?ActiveSubIndex := SubIndex;.*?ActiveLength := ReadLength;.*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RUNNING.*?toSlave\.StartReadSDO\(\s*ObjectIndex\$HINT,\s*SubIndex\$HSINT,\s*0,\s*\(#ReadBuffer\[0\]\)\$\^USINT,\s*TO_UDINT\(ReadLength\),\s*TimeoutMs,\s*THIS\)' 'LMCSdoExecutor must publish Running before exposing its exact 1/2/4-byte vendor request and callback buffer.'
if ([regex]::Matches($sdoTryStartBlock, 'toSlave\.StartReadSDO\(').Count -ne 1) {
    throw 'LMCSdoExecutor.TryStartRead must expose exactly one vendor SDO request.'
}
Assert-Match $sdoTryStartBlock '(?s)IsClientConnected\(#toSlave\) = FALSE.*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState = LMC_SDO_EXEC_ARMING then.*?ActiveToken := 0.*?_memset\(dest:=#ReadBuffer\[0\].*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?value:=LMC_SDO_EXEC_QUARANTINED.*?else\s*sigclib_atomic_setU32\(\s*pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED\)' 'LMCSdoExecutor disconnected rollback can overwrite an unsolicited callback or expose Idle before cleanup.'
Assert-Match $sdoTryStartBlock '(?s)startResult <> READY.*?cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState = LMC_SDO_EXEC_RUNNING then.*?ActiveToken := 0.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE' 'LMCSdoExecutor does not exclusively clear and release an unaccepted vendor request.'
Assert-Match $sdoTryStartBlock '(?s)if startResult <> READY then.*?value:=LMC_SDO_EXEC_QUARANTINED\);\s*ret_code := ERROR;.*?end_if;\s*end_if;\s*END_FUNCTION' 'LMCSdoExecutor does not preserve an unaccepted-request invariant failure as hard quarantine.'
if ($sdoTryStartBlock -match '(?s)if startResult <> READY then.*?ret_code := READY') {
    throw 'LMCSdoExecutor incorrectly promotes an unaccepted vendor request to Ready.'
}

$sdoTryStartWriteBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::TryStartWrite.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($sdoTryStartWriteBlock)) {
    throw 'LMCSdoExecutor.TryStartWrite implementation was not found.'
}
Assert-Match $sdoTryStartWriteBlock '(?s)pWriteData\s*:\s*\^USINT;.*?WriteLength\s*:\s*UINT;.*?pWriteData = NIL.*?WriteLength <> 4.*?cmpVal:=LMC_SDO_EXEC_IDLE,\s*newVal:=LMC_SDO_EXEC_ARMING.*?ActiveLength := WriteLength;.*?ActiveIsWrite := TRUE;.*?_memcpy\(ptr1:=#WriteBuffer\[0\], ptr2:=pWriteData,\s*cntr:=sizeof\(WriteBuffer\)\).*?cmpVal:=LMC_SDO_EXEC_ARMING,\s*newVal:=LMC_SDO_EXEC_RUNNING.*?toSlave\.StartWriteSDO\(\s*ObjectIndex\$HINT,\s*SubIndex\$HSINT,\s*0,\s*\(#WriteBuffer\[0\]\)\$\^USINT,\s*TO_UDINT\(WriteLength\),\s*TimeoutMs,\s*THIS\)' 'LMCSdoExecutor must own an exact four-byte write buffer and publish Running before the vendor write can callback.'
if ([regex]::Matches($sdoTryStartWriteBlock, 'toSlave\.StartWriteSDO\(').Count -ne 1) {
    throw 'LMCSdoExecutor.TryStartWrite must expose exactly one vendor SDO write request.'
}
Assert-Match $sdoTryStartWriteBlock '(?s)IsClientConnected\(#toSlave\) = FALSE.*?newVal:=LMC_SDO_EXEC_RELEASING.*?ActiveIsWrite := FALSE;.*?_memset\(dest:=#WriteBuffer\[0\].*?newVal:=LMC_SDO_EXEC_IDLE.*?startResult <> READY.*?newVal:=LMC_SDO_EXEC_RELEASING.*?ActiveIsWrite := FALSE;.*?_memset\(dest:=#WriteBuffer\[0\].*?newVal:=LMC_SDO_EXEC_IDLE' 'LMCSdoExecutor write rollback does not exclusively clear its owned persistent buffer before returning to Idle.'

$sdoCopyCompletionBlock = [regex]::Match(
    $sdoExecutor,
    '(?s)FUNCTION GLOBAL LMCSdoExecutor::CopyCompletion.*?END_FUNCTION').Value
Assert-Match $sdoCopyCompletionBlock '(?s)stateValue := sigclib_atomic_getU32\(pValue:=#AdapterState\);\s*if stateValue <> LMC_SDO_EXEC_RESULT_READY then\s*Result := -2;\s*RETURN;\s*end_if;.*?retryCount < 3.*?sequenceBefore := sigclib_atomic_getU32.*?_memcpy.*?sequenceAfter := sigclib_atomic_getU32.*?sequenceBefore = sequenceAfter.*?localResult\.Token <> ExpectedToken.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor completion copy lacks ResultReady-only admission, bounded seqlock, or token validation.'
if ($sdoCopyCompletionBlock -match 'stateValue <> LMC_SDO_EXEC_QUARANTINED') {
    throw 'LMCSdoExecutor.CopyCompletion must not recover a hard-quarantined adapter.'
}
Assert-Match $sdoCopyCompletionBlock '(?s)cmpVal:=stateValue,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState <> stateValue then.*?RETURN;\s*end_if;\s*ActiveToken := 0.*?ActiveIsWrite := FALSE;.*?_memset\(dest:=#ReadBuffer\[0\].*?_memset\(dest:=#WriteBuffer\[0\].*?_memset\(dest:=#PublishedResult.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?RETURN;\s*end_if;.*?_memcpy\(ptr1:=pDest, ptr2:=#localResult' 'LMCSdoExecutor does not exclusively clear both buffers and release a consumed owned completion before exposing Idle.'

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
Assert-Match $sdoCallbackBlock '(?s)aPara\[0\]\$DINT <> 1.*?stateValue <> LMC_SDO_EXEC_RUNNING.*?stateValue <> LMC_SDO_EXEC_ORPHANED.*?callbackIsWrite = 0.*?ActiveIsWrite = TRUE.*?callbackIsWrite <> 0.*?ActiveIsWrite = FALSE.*?callbackIndex <> ActiveIndex.*?callbackSubIndex <> ActiveSubIndex.*?ActiveToken = 0.*?actualLength <> TO_UDINT\(ActiveLength\)' 'LMCSdoExecutor callback version/state/direction/index/subindex/token/length validation is incomplete.'
Assert-Match $sdoCallbackBlock '(?s)stateValue <> LMC_SDO_EXEC_RUNNING.*?stateValue <> LMC_SDO_EXEC_ORPHANED.*?ActiveToken = 0.*?value:=LMC_SDO_EXEC_QUARANTINED.*?RETURN' 'LMCSdoExecutor does not keep unsolicited or duplicate callbacks quarantined.'
Assert-Match $sdoCallbackBlock '(?s)if stateValue = LMC_SDO_EXEC_ORPHANED then.*?cmpVal:=LMC_SDO_EXEC_ORPHANED,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?if previousState <> LMC_SDO_EXEC_ORPHANED then.*?RETURN;\s*end_if;\s*ActiveToken := 0.*?ActiveIsWrite := FALSE;.*?_memset\(dest:=#ReadBuffer\[0\].*?_memset\(dest:=#WriteBuffer\[0\].*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?if previousState <> LMC_SDO_EXEC_RELEASING then.*?value:=LMC_SDO_EXEC_QUARANTINED.*?end_if;\s*RETURN' 'LMCSdoExecutor does not drain every owned late read/write orphan callback back to Idle.'
Assert-Match $sdoCallbackBlock '(?s)writeSequence := sigclib_atomic_getU32.*?writeSequence and 1.*?sigclib_atomic_setU32\(\s*pValue:=#PublishSequence, value:=writeSequence\).*?PublishedResult\.Token := ActiveToken.*?PublishedResult\.ValidationCode := validationCode.*?if ActiveIsWrite then\s*PublishedResult\.Data := 0;\s*else\s*PublishedResult\.Data := ReadBuffer\[0\]\$UDINT;\s*end_if;.*?finalSequence := writeSequence \+ 1.*?value:=finalSequence.*?cmpVal:=LMC_SDO_EXEC_RUNNING,\s*newVal:=LMC_SDO_EXEC_RESULT_READY' 'LMCSdoExecutor owned callback publication is not an atomic direction-safe seqlock result that remains consumable after validation failure.'
Assert-Match $sdoCallbackBlock '(?s)previousState = LMC_SDO_EXEC_ORPHANED.*?cmpVal:=LMC_SDO_EXEC_ORPHANED,\s*newVal:=LMC_SDO_EXEC_RELEASING.*?ActiveToken := 0.*?_memset\(dest:=#PublishedResult.*?cmpVal:=LMC_SDO_EXEC_RELEASING,\s*newVal:=LMC_SDO_EXEC_IDLE.*?previousState <> LMC_SDO_EXEC_RUNNING.*?value:=LMC_SDO_EXEC_QUARANTINED' 'LMCSdoExecutor does not resolve the callback-publication versus orphan race without overwriting the orphan state.'

Assert-Match $diagnosticsService '#define LMC_DIAG_D1_ENABLED\s+TRUE' 'D1 Health/Catalog/PI Read is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D2_ENABLED\s+TRUE' 'D2 Bulk Snapshot is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D3_ENABLED\s+TRUE' 'D3 single-bank Recorder is not enabled.'
Assert-Match $diagnosticsService '#define LMC_DIAG_D5_SDO_READ_ENABLED\s+TRUE' 'D5 general inline SDO Read gate must remain TRUE while the test project advertises bits 8 and 13 with MaxSdoDataBytes=4.'
Assert-Match $diagnosticsService '#define LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED\s+FALSE' 'EtherCAT digital-output global gate must remain FALSE before the live safety matrix passes.'
Assert-Match $diagnosticsService '#define LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED\s+FALSE' 'GT-22BA digital-output per-node gate must remain FALSE before the live safety matrix passes.'
Assert-Match $diagnosticsService '#define LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE\s+0x00010002' 'EtherCAT digital-output target must remain the configured GT-22BA IOReference.'
Assert-Match $diagnosticsService '#define LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK\s+0xFFFFFFFF' 'EtherCAT digital-output target mask must remain the exact configured 32-bit width.'
Assert-Match $diagnosticsService '#define LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES\s+1000' 'EtherCAT digital-output ticket timeout must remain the fixed 1000-cycle quarantine deadline.'

$sdoExecutorConstructorFixture = @'
LMCSdoExecutor : CLASS
: EtherCAT_SDOBase
    FUNCTION LMCSdoExecutor
        VAR_OUTPUT
            ret_code : ConfStates;
        END_VAR;
END_CLASS;

FUNCTION LMCSdoExecutor::@STD
    VAR_OUTPUT
        ret_code : ConfStates;
    END_VAR

    ret_code := EtherCAT_SDOBase::@STD();
    ret_code := LMCSdoExecutor();
END_FUNCTION

FUNCTION LMCSdoExecutor::LMCSdoExecutor
    VAR_OUTPUT
        ret_code : ConfStates;
    END_VAR

    ActiveToken := 0;
    ActiveIndex := 0;
    ActiveSubIndex := 0;
    ActiveLength := 0;
    ActiveIsWrite := FALSE;
    _memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer));
    _memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer));
    _memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult));
    sigclib_atomic_setU32(pValue:=#PublishSequence, value:=0);
    sigclib_atomic_setU32(pValue:=#AdapterState, value:=LMC_SDO_EXEC_IDLE);
    ret_code := C_OK;
END_FUNCTION
'@
$sdoExecutorConstructorFixture =
    $sdoExecutorConstructorFixture -replace '\r\n?', "`n"
Assert-LMCSdoExecutorConstructorReady `
    -SdoExecutorText $sdoExecutorConstructorFixture `
    -ClassDatabaseText '' `
    -Owner 'LMCSdoExecutor constructor verifier fixture'

$sdoExecutorConstructorMetadataFixture = (
    '.\Class\LMCSdoExecutor\LMCSdoExecutor.st|' +
    'ClassSvr|AdapterState|ActiveToken|ActiveIndex|ActiveSubIndex|' +
    'ActiveLength|ActiveIsWrite|ReadBuffer|WriteBuffer|PublishSequence|' +
    'PublishedResult|LMCSdoExecutor|ret_code|TryStartRead|' +
    '.\Class\FixtureEnd\FixtureEnd.st')
Assert-LMCSdoExecutorConstructorReady `
    -SdoExecutorText $sdoExecutorConstructorFixture `
    -ClassDatabaseText $sdoExecutorConstructorMetadataFixture `
    -RequireClassDatabaseMetadata `
    -Owner 'LMCSdoExecutor constructor metadata verifier fixture'
$sdoExecutorMissingConstructorMetadataFixture =
    $sdoExecutorConstructorMetadataFixture.Replace(
        '|LMCSdoExecutor|ret_code|',
        '|ret_code|')
$sdoConstructorMetadataNegativeRejected = $false
try {
    Assert-LMCSdoExecutorConstructorReady `
        -SdoExecutorText $sdoExecutorConstructorFixture `
        -ClassDatabaseText $sdoExecutorMissingConstructorMetadataFixture `
        -RequireClassDatabaseMetadata `
        -Owner 'LMCSdoExecutor constructor metadata-negative fixture'
}
catch {
    $sdoConstructorMetadataNegativeRejected = $true
}
if (-not $sdoConstructorMetadataNegativeRejected) {
    throw (
        'LMCSdoExecutor constructor readiness verifier accepted missing ' +
        'Classes.lcb constructor metadata.')
}

$sdoConstructorInitializerDeletions = [ordered]@{
    ActiveToken = 'ActiveToken := 0;'
    ActiveIndex = 'ActiveIndex := 0;'
    ActiveSubIndex = 'ActiveSubIndex := 0;'
    ActiveLength = 'ActiveLength := 0;'
    ActiveIsWrite = 'ActiveIsWrite := FALSE;'
    ReadBuffer = '_memset(dest:=#ReadBuffer[0], usByte:=0, cntr:=sizeof(ReadBuffer));'
    WriteBuffer = '_memset(dest:=#WriteBuffer[0], usByte:=0, cntr:=sizeof(WriteBuffer));'
    PublishedResult = '_memset(dest:=#PublishedResult, usByte:=0, cntr:=sizeof(PublishedResult));'
    PublishSequence = 'sigclib_atomic_setU32(pValue:=#PublishSequence, value:=0);'
    AdapterState = 'sigclib_atomic_setU32(pValue:=#AdapterState, value:=LMC_SDO_EXEC_IDLE);'
    ret_code = 'ret_code := C_OK;'
}
$sdoConstructorNegativeDeletionCount = 0
foreach ($deletion in $sdoConstructorInitializerDeletions.GetEnumerator()) {
    $deletionPattern = (
        '(?m)^[ \t]*' + [regex]::Escape([string]$deletion.Value) +
        '[ \t]*(?:\r?\n|$)')
    $negativeFixture = ([regex]::new($deletionPattern)).Replace(
        $sdoExecutorConstructorFixture,
        '',
        1)
    if ($negativeFixture.Length -eq $sdoExecutorConstructorFixture.Length) {
        throw (
            'LMCSdoExecutor constructor negative fixture could not remove ' +
            "'$($deletion.Key)'.")
    }

    $negativeRejected = $false
    try {
        Assert-LMCSdoExecutorConstructorReady `
            -SdoExecutorText $negativeFixture `
            -ClassDatabaseText '' `
            -Owner 'LMCSdoExecutor constructor deletion-negative fixture'
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCSdoExecutor constructor readiness verifier accepted deletion ' +
            "of '$($deletion.Key)'.")
    }
    $sdoConstructorNegativeDeletionCount++
}

$sdoConstructorBypassFixtures = [ordered]@{}
$sdoConstructorBypassFixtures['ExtraDeclarationInput'] =
    $sdoExecutorConstructorFixture.Replace(
        "        END_VAR;`nEND_CLASS;",
        ("        END_VAR;`n" +
         "    VAR_INPUT`n" +
         "        UnexpectedInput : UDINT;`n" +
         "    END_VAR;`n" +
         'END_CLASS;'))
$sdoConstructorBypassFixtures['EarlyReturn'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ActiveToken := 0;',
        "    RETURN;`n    ActiveToken := 0;")
$sdoConstructorBypassFixtures['ConditionalInitialization'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ActiveToken := 0;',
        "    IF FALSE THEN`n    ActiveToken := 0;").Replace(
        '    ret_code := C_OK;',
        "    ret_code := C_OK;`n    END_IF;")
$sdoConstructorBypassFixtures['LoopInitialization'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ActiveToken := 0;',
        "    FOR bypassIndex := 0 TO 0 DO`n    ActiveToken := 0;").Replace(
        '    ret_code := C_OK;',
        "    ret_code := C_OK;`n    END_FOR;")
$sdoConstructorBypassFixtures['BranchInitialization'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ActiveToken := 0;',
        "    CASE 0 OF`n    0:`n    ActiveToken := 0;").Replace(
        '    ret_code := C_OK;',
        "    ret_code := C_OK;`n    END_CASE;")
$sdoConstructorBypassFixtures['PostIdleActiveMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        "    ActiveToken := 1;`n    ret_code := C_OK;")
$sdoConstructorBypassFixtures['PostIdleReadBufferMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        "    ReadBuffer[0] := 1;`n    ret_code := C_OK;")
$sdoConstructorBypassFixtures['PostIdleWriteBufferMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        "    WriteBuffer[0] := 1;`n    ret_code := C_OK;")
$sdoConstructorBypassFixtures['PostIdlePublishedResultMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        "    PublishedResult.Token := 1;`n    ret_code := C_OK;")
$sdoConstructorBypassFixtures['PostIdlePublishSequenceMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        ("    sigclib_atomic_setU32(" +
         "pValue:=#PublishSequence, value:=2);`n" +
         '    ret_code := C_OK;'))
$sdoConstructorBypassFixtures['PostIdleAdapterStateMutation'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        ("    sigclib_atomic_setU32(" +
         "pValue:=#AdapterState, value:=LMC_SDO_EXEC_QUARANTINED);`n" +
         '    ret_code := C_OK;'))
$sdoConstructorBypassFixtures['AlternateReadBufferAddress'] =
    $sdoExecutorConstructorFixture.Replace(
        '    ret_code := C_OK;',
        ("    _memset(dest:=#ReadBuffer[1], usByte:=0, cntr:=1);`n" +
         '    ret_code := C_OK;'))

$sdoConstructorBypassNegativeCount = 0
foreach ($bypassFixture in $sdoConstructorBypassFixtures.GetEnumerator()) {
    if ($bypassFixture.Value -eq $sdoExecutorConstructorFixture) {
        throw (
            'LMCSdoExecutor constructor bypass fixture did not mutate the ' +
            "canonical source for '$($bypassFixture.Key)'.")
    }

    $bypassRejected = $false
    try {
        Assert-LMCSdoExecutorConstructorReady `
            -SdoExecutorText $bypassFixture.Value `
            -ClassDatabaseText '' `
            -Owner 'LMCSdoExecutor constructor bypass-negative fixture'
    }
    catch {
        $bypassRejected = $true
    }
    if (-not $bypassRejected) {
        throw (
            'LMCSdoExecutor constructor readiness verifier accepted bypass ' +
            "'$($bypassFixture.Key)'.")
    }
    $sdoConstructorBypassNegativeCount++
}

if ($ExpectedSdoWriteAxis -eq 0) {
    Assert-Match $diagnosticsService '#define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED\s+FALSE' 'D5 SDO Write global production gate must remain FALSE until the PLC candidate is explicitly approved.'
    Assert-Match $diagnosticsModels 'SdoWriteEnabled\s*=\s*false;' 'The SDK SDO Write global gate must remain false while ExpectedSdoWriteAxis is zero.'
}
else {
    Assert-LMCSdoExecutorConstructorReady `
        -SdoExecutorText $sdoExecutor `
        -ClassDatabaseText $classDbText `
        -RequireClassDatabaseMetadata:(-not $SourceOnly) `
        -Owner 'LMCSdoExecutor'
    Assert-Match $diagnosticsService '#define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED\s+TRUE' 'D5 SDO Write global gate must be TRUE for an explicitly selected test axis.'
    Assert-Match $diagnosticsModels 'SdoWriteEnabled\s*=\s*true;' 'The SDK SDO Write global gate must be true for an explicitly selected test axis.'
}
foreach ($axis in 1..4) {
    $expectedGate = if ($axis -eq $ExpectedSdoWriteAxis) {
        'TRUE'
    }
    else {
        'FALSE'
    }
    Assert-Match $diagnosticsService ("#define LMC_DIAG_D5_SDO_WRITE_UI24_AXIS{0}_ENABLED\s+{1}" -f $axis, $expectedGate) ("The axis {0} UI[24] SDO Write gate does not match ExpectedSdoWriteAxis={1}." -f $axis, $ExpectedSdoWriteAxis)
    $expectedCSharpGate = $expectedGate.ToLowerInvariant()
    Assert-Match $diagnosticsModels ("SdoWriteUi24Axis{0}Enabled\s*=\s*{1};" -f $axis, $expectedCSharpGate) ("The SDK axis {0} UI[24] SDO Write gate does not match ExpectedSdoWriteAxis={1}." -f $axis, $ExpectedSdoWriteAxis)
}
Assert-Match $diagnosticsModels '(?s)new LMCSdoWriteTarget\(\s*"Reserved diagnostic UI\[24\]",\s*slaveReference,\s*0x2F00,\s*24,\s*LMCSignalValueType\.Int32,\s*4,\s*-1073741823,\s*1073741823\)' 'The SDK SDO Write tuple/range does not match the PLC UI[24] Int32/four-byte policy.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::GetSdoWritePolicyDetail.*?ObjectIndex = 0x6040.*?ObjectIndex = 0x607A.*?ObjectIndex = 0x60FF.*?ObjectIndex = 0x6071.*?DetailCode := 8;.*?LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE.*?DetailCode := 7;.*?ObjectIndex <> 0x2F00.*?SubIndex <> 24.*?ValueType <> 4.*?DataLength <> 4.*?case SlaveReference of.*?1:.*?LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED = FALSE.*?DetailCode := 7;.*?2:.*?LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED = FALSE.*?DetailCode := 7;.*?3:.*?LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED = FALSE.*?DetailCode := 7;.*?4:.*?LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED = FALSE.*?DetailCode := 7;.*?end_case;.*?writeValue < -1073741823.*?writeValue > 1073741823.*?DetailCode := 12;.*?CheckAxisState = FALSE.*?pSnapshot = NIL.*?SnapshotSize < 304.*?statusWord and 0x0000006F.*?0x00000040.*?DetailCode := 19;.*?END_FUNCTION' 'LMCDiagnosticsService central SDO Write unsafe-object, disabled per-axis UI[24] tuple, bounded value, and DS402 state policy is incomplete.'
Assert-NoCaseInsensitiveMemberShadowing $diagnosticsService 'LMCDiagnosticsService'
if ([regex]::Matches($diagnosticsService, '<Client Name="SdoAxis[1-4]" Required="true" Internal="false"/>').Count -ne 4 -or
    [regex]::Matches($diagnosticsService, 'SdoAxis[1-4]\s*:\s*CltChCmd_LMCSdoExecutor;').Count -ne 4) {
    throw 'LMCDiagnosticsService does not declare exactly four required LMCSdoExecutor clients.'
}
Assert-Match $diagnosticsService '#define LMC_DIAG_MAP_REVISION\s+0x957F101E' 'LMCDiagnosticsService MapRevision is not the canonical D1 catalog CRC.'
Assert-Match $diagnosticsService '#define LMC_DIAG_TOPOLOGY_REVISION\s+0x15867EEC' 'LMCDiagnosticsService TopologyRevision is not the canonical seven-node CRC.'
Assert-Match $diagnosticsService 'Server Name="DiagnosticsBootCounter".*Initialize="true".*DefValue="0".*Retentive="File"' 'LMCDiagnosticsService retained DiagnosticsBootCounter metadata is missing.'
Assert-Match $diagnosticsService '(?s)FUNCTION GLOBAL LMCDiagnosticsService::GetDiagnosticsBootId.*?DiagnosticsBootCounter\.Read\(\).*?nextBootId = 0xFFFFFFFF.*?DiagnosticsBootCounter\.Write\(input:=nextBootId\).*?DiagnosticsBootCounter\.Read\(\) = nextBootId.*?BootIdFault := TRUE.*?END_FUNCTION' 'LMCDiagnosticsService retained BootId generation or write verification is incomplete.'
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::BuildCatalogEntry.*?CatalogIndex >= 24.*?pEntry \+ 76.*?:= 0' 'LMCDiagnosticsService fixed 80-byte catalog entry builder is incomplete.'
Assert-Match $diagnosticsService '(?s)CatalogIndex = 0x0200.*?_memset\(dest:=pEntry,\s*usByte:=0,\s*cntr:=28\).*?pEntry\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION.*?pEntry \+ 4\)\^\$UINT := 7.*?pEntry \+ 6\)\^\$UINT := 96.*?pEntry \+ 8\)\^\$UINT := 1.*?pEntry \+ 10\)\^\$UINT := 5.*?pEntry \+ 12\)\^\$UINT := 2.*?pEntry \+ 14\)\^\$UINT := 4.*?pEntry \+ 16\)\^\$UDINT := 0x0000000F.*?pEntry \+ 20\)\^\$UDINT := 1' 'LMCDiagnosticsService topology info serializer is not the fixed seven-node v1 contract.'
Assert-Match $diagnosticsService '(?s)CatalogIndex >= 0x8000.*?physicalAxis := \(CatalogIndex shr 8\) and 0x007F.*?topologyCount := CatalogIndex and 0x00FF.*?pEntry \+ 16\)\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION.*?pEntry \+ 20\)\^\$UINT := physicalAxis.*?pEntry \+ 22\)\^\$UINT := topologyCount.*?pEntry \+ 24\)\^\$UINT := 7.*?pEntry \+ 26\)\^\$UINT := 96.*?pEntry \+ 2\)\^\$UINT := 2.*?pTopologyEntry := pEntry \+ 28' 'LMCDiagnosticsService topology chunk header or LastChunk construction is incomplete.'
Assert-Match $diagnosticsService '(?s)topologyCount := CatalogIndex and 0x00FF.*?topologyCount = 0.*?physicalAxis >= 7.*?physicalAxis \+ topologyCount\) > 7.*?RETURN;' 'LMCDiagnosticsService private topology serializer does not reject zero or out-of-range aggregate requests.'
Assert-Match $diagnosticsService '(?s)pdoIndex = 0 then.*?pTopologyEntry\^\$UDINT := 0xEC000001.*?pTopologyEntry \+ 10\)\^\$UINT := 0.*?pTopologyEntry \+ 14\)\^\$UINT := 0x0041.*?pTopologyEntry \+ 20\)\^\$UINT := 0xFFFF.*?pTopologyEntry \+ 24\)\^\$UDINT := 669.*?pTopologyEntry \+ 28\)\^\$UDINT := 1196200070.*?pTopologyEntry \+ 32\)\^\$UDINT := 65536.*?GL_9086_11' 'LMCDiagnosticsService CREVIS coupler topology entry is incomplete.'
Assert-Match $diagnosticsService '(?s)pdoIndex <= 4 then.*?pTopologyEntry\^\$UDINT := 0xEC000100 \+ TO_UDINT\(pdoIndex\).*?pTopologyEntry \+ 10\)\^\$UINT := pdoIndex.*?pTopologyEntry \+ 14\)\^\$UINT := 0x0027.*?pTopologyEntry \+ 16\)\^\$UINT := pdoIndex.*?pTopologyEntry \+ 18\)\^\$UINT := pdoIndex.*?pTopologyEntry \+ 20\)\^\$UINT := 0xFFFF.*?pTopologyEntry \+ 24\)\^\$UDINT := 154.*?pTopologyEntry \+ 28\)\^\$UDINT := 198948.*?pTopologyEntry \+ 32\)\^\$UDINT := 66592.*?Elmo_11.*?pTopologyEntry \+ 49\)\^\$USINT := TO_USINT\(48 \+ pdoIndex\)' 'LMCDiagnosticsService four-drive topology entry generator is incomplete.'
Assert-Match $diagnosticsService '(?s)pTopologyEntry\^\$UDINT := 0xEC00FFFC \+ TO_UDINT\(pdoIndex\).*?pTopologyEntry \+ 4\)\^\$UDINT := 0xEC000001.*?pTopologyEntry \+ 10\)\^\$UINT := 0xFFFF.*?pTopologyEntry \+ 20\)\^\$UINT := pdoIndex - 5.*?pTopologyEntry \+ 92\)\^\$UDINT := 0x0000FFFC \+ TO_UDINT\(pdoIndex\).*?pdoIndex = 5 then.*?0x0088.*?1196692218.*?pTopologyEntry \+ 40\)\^\$UINT := 4.*?GL_9086_1_Slot001.*?0x0090.*?1196696250.*?pTopologyEntry \+ 42\)\^\$UINT := 4.*?GL_9086_1_Slot011.*?pTopologyEntry \+= 96' 'LMCDiagnosticsService CREVIS slot topology entries are incomplete.'
if ($topologyIoReadIntegrated) {
    Assert-Match $diagnosticsService (
        '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?' +
        '0x7E01:.*?0x7E02:.*?0x7E10:.*?' +
        'HandleEtherCATTopologyIoRequest\(.*?0x7E20:') (
        'LMCDiagnosticsService D1 and delegated topology/I/O handlers are missing.')
}
else {
    Assert-Match $diagnosticsService (
        '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?' +
        '0x7E01:.*?0x7E02:.*?0x7E10:.*?' +
        '0x7E11:.*?0x7E12:.*?0x7E20:') (
        'LMCDiagnosticsService D1/static-topology command handlers are missing.')
}
Assert-Match $diagnosticsService '(?s)InputLatch\.CopySnapshot\(.*?DestSize:=sizeof\(snapshot\).*?ResponseSize := 200' 'EtherCAT Health does not use the immutable latch snapshot.'
$diagnosticsServiceHandleBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceHandleBlock)) {
    throw 'LMCDiagnosticsService.HandleRequest implementation was not found.'
}
$readPiCaseBlock = [regex]::Match(
    $diagnosticsServiceHandleBlock,
    '(?s)(?m)^[ \t]*0x7E20:.*?(?=^[ \t]*0x7E03:)').Value
if ([string]::IsNullOrWhiteSpace($readPiCaseBlock)) {
    throw 'LMCDiagnosticsService 0x7E20 ReadPI case block was not found.'
}
Assert-Match $readPiCaseBlock '(?s)entryStatus := 0.*?entryStatus := entryStatus or 4.*?entryStatus := entryStatus or 2.*?entryStatus := 1' 'PI Read entry validity/staleness status construction is incomplete.'
Assert-Match $readPiCaseBlock '(?s)entryDetailCode\s*:=\s*0;.*?if entryStatus = 0 then.*?entryStatus\s*:=\s*1;.*?elsif \(entryStatus and 8\) <> 0 then.*?entryDetailCode\s*:=\s*18;.*?else.*?entryDetailCode\s*:=\s*11;.*?end_if;.*?\(pResponse \+ 48\)\^\$UDINT\s*:=\s*entryDetailCode;.*?ResponseSize\s*:=\s*52' 'PI Read must deterministically publish Valid/detail0, SlaveOffline/detail18 priority, or other-invalid/detail11 in its own 0x7E20 block.'
if ($diagnosticsServiceRouted) {
    $diagnosticsServiceHandleByteCount =
        [Text.Encoding]::UTF8.GetByteCount($diagnosticsServiceHandleBlock)
    if ($diagnosticsServiceHandleByteCount -gt 32768) {
        throw (
            'Phase4DiagnosticsRouted LMCDiagnosticsService.HandleRequest is ' +
            "$diagnosticsServiceHandleByteCount bytes, expected at most 32768.")
    }

    $topologyIoHandlerBlock = $diagnosticsServiceHandleBlock
    if ($topologyIoReadIntegrated) {
        $topologyIoHandlerBlock = [regex]::Match(
            $diagnosticsService,
            '(?s)FUNCTION LMCDiagnosticsService::' +
            'HandleEtherCATTopologyIoRequest.*?END_FUNCTION').Value
        if ([string]::IsNullOrWhiteSpace($topologyIoHandlerBlock)) {
            throw ('LMCDiagnosticsService.HandleEtherCATTopologyIoRequest ' +
                'implementation was not found.')
        }
        Assert-Match $topologyIoHandlerBlock (
            '(?s)\AFUNCTION LMCDiagnosticsService::' +
            'HandleEtherCATTopologyIoRequest\s*' +
            'VAR_INPUT\s*CommandId\s*:\s*UINT;\s*' +
            'pRequest\s*:\s*\^USINT;\s*RequestSize\s*:\s*UDINT;\s*' +
            'pResponse\s*:\s*\^USINT;\s*ResponseCapacity\s*:\s*UDINT;\s*' +
            'CallerSessionEpoch\s*:\s*UDINT;\s*' +
            'CurrentDiagnosticsBootId\s*:\s*UDINT;\s*END_VAR\s*' +
            'VAR_OUTPUT\s*ResponseSize\s*:\s*DINT;\s*END_VAR') (
            'LMCDiagnosticsService topology/I/O helper implementation ABI is ' +
            'incomplete or narrows a request, size, epoch, BootId, or result field.')
        Assert-Match $topologyIoHandlerBlock (
            '(?s)inputLatchConnected\s*:\s*BOOL;.*?' +
            'ResponseSize := -1;\s*' +
            'if \(pResponse = NIL\) \| \(ResponseCapacity < 16\) then\s*' +
            'RETURN;\s*end_if;\s*detailCode := 0;') (
            'LMCDiagnosticsService topology/I/O helper must declare its client ' +
            'state and initialize the response fail-closed before dispatch.')
        Assert-Match $topologyIoHandlerBlock (
            'snapshot\s*:\s*ARRAY \[0\.\.463\] OF USINT;') (
            'LMCDiagnosticsService topology/I/O helper must use one exact ' +
            '464-byte coherent snapshot buffer.')
        $topologyIoReadLocalTypes = [ordered]@{
            'inputLatchConnected' = 'BOOL'
            'detailCode' = 'UDINT'
            'copyResult' = 'DINT'
            'expectedMapRevision' = 'UDINT'
            'requestedNodeId' = 'UDINT'
            'healthOffset' = 'UDINT'
            'nodeNativeOnline' = 'DINT'
            'nodeEtherCATState' = 'UDINT'
            'nodeSlaveState' = 'UDINT'
            'nodeALStatus' = 'UDINT'
            'nodeClassState' = 'UDINT'
            'nodeDS402Status' = 'UDINT'
            'nodeAxisError' = 'UDINT'
            'nodeLastValidCycle' = 'UDINT'
            'nodeLastStateChangeCycle' = 'UDINT'
            'nodeDetected' = 'BOOL'
            'nodeParentIdentityMatched' = 'BOOL'
            'nodeIdentityMatched' = 'BOOL'
            'nodeDataValid' = 'BOOL'
            'nodeHealthFlags' = 'UINT'
            'wireOnline' = 'USINT'
            'requestIOReference' = 'UDINT'
            'requestDirection' = 'USINT'
            'requestBitWidth' = 'USINT'
            'requestReserved' = 'UINT'
            'ioNodeId' = 'UDINT'
            'ioValueLow' = 'UDINT'
            'ioValidMaskLow' = 'UDINT'
            'ioStatus' = 'UINT'
            'ioCycle' = 'UDINT'
            'ioOutputRevision' = 'UDINT'
        }
        foreach ($topologyIoReadLocal in $topologyIoReadLocalTypes.GetEnumerator()) {
            Assert-LasalExactDeclaredType `
                -Text $topologyIoHandlerBlock `
                -Name $topologyIoReadLocal.Key `
                -ExpectedType $topologyIoReadLocal.Value `
                -Owner ('LMCDiagnosticsService topology/I/O helper local ' +
                    $topologyIoReadLocal.Key)
        }
        if ($topologyIoOutputIntegrated) {
            $topologyIoOutputLocalTypes = [ordered]@{
                'requestValueLow' = 'UDINT'
                'requestValueHigh' = 'UDINT'
                'requestMaskLow' = 'UDINT'
                'requestMaskHigh' = 'UDINT'
                'requestExpectedOutputRevision' = 'UDINT'
                'requestDiagnosticsBootId' = 'UDINT'
                'tryQueueResult' = 'iprStates'
            }
            foreach ($topologyIoOutputLocal in
                    $topologyIoOutputLocalTypes.GetEnumerator()) {
                Assert-LasalExactDeclaredType `
                    -Text $topologyIoHandlerBlock `
                    -Name $topologyIoOutputLocal.Key `
                    -ExpectedType $topologyIoOutputLocal.Value `
                    -Owner ('LMCDiagnosticsService topology/I/O output local ' +
                        $topologyIoOutputLocal.Key)
            }
        }
        $topologyIoHandlerScanText = Get-LasalScanText $topologyIoHandlerBlock
        Assert-LasalAddressNamesAllowed `
            -Text $topologyIoHandlerBlock `
            -AllowedNames @('InputLatch', 'snapshot') `
            -Owner 'LMCDiagnosticsService topology/I/O helper'
        $expectedTopologyIoSnapshotAddressUses = 2
        if ($topologyIoOutputIntegrated) {
            $expectedTopologyIoSnapshotAddressUses = 3
        }
        $topologyIoInputLatchAddressUses = [regex]::Matches(
            $topologyIoHandlerScanText,
            '(?i)#\s*InputLatch\b')
        $topologyIoSnapshotAddressUses = [regex]::Matches(
            $topologyIoHandlerScanText,
            '(?i)#\s*snapshot\b(?:\s*\[\s*[^\]]+\])?')
        $topologyIoAllAddressUses = [regex]::Matches(
            $topologyIoHandlerScanText,
            '(?i)#\s*[A-Za-z_][A-Za-z0-9_]*\b')
        if ($topologyIoInputLatchAddressUses.Count -ne 1 -or
            $topologyIoSnapshotAddressUses.Count -ne
                $expectedTopologyIoSnapshotAddressUses -or
            $topologyIoAllAddressUses.Count -ne
                (1 + $expectedTopologyIoSnapshotAddressUses)) {
            throw ('LMCDiagnosticsService topology/I/O helper may take only ' +
                'the one canonical InputLatch address and one canonical ' +
                'snapshot destination address per snapshot command; helper-' +
                'prefix or cross-case address aliases are forbidden.')
        }
        $topologyIoPointerDeclarations = [regex]::Matches(
            $topologyIoHandlerScanText,
            '(?i)\b(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:\s*\^\s*' +
                '(?<Type>[A-Za-z_][A-Za-z0-9_]*)\s*;')
        $topologyIoExecutableStart = [regex]::Match(
            $topologyIoHandlerScanText,
            '(?i)\bResponseSize\s*:=\s*-1\s*;')
        if (-not $topologyIoExecutableStart.Success) {
            throw ('LMCDiagnosticsService topology/I/O helper executable ' +
                'boundary was not found.')
        }
        $topologyIoDeclarationScanText =
            $topologyIoHandlerScanText.Substring(
                0,
                $topologyIoExecutableStart.Index)
        $topologyIoDeclarationCarets = [regex]::Matches(
            $topologyIoDeclarationScanText,
            '(?i)\^\s*[A-Za-z_][A-Za-z0-9_]*\b')
        if ($topologyIoPointerDeclarations.Count -ne 2 -or
            $topologyIoDeclarationCarets.Count -ne 2 -or
            [regex]::Matches(
                $topologyIoDeclarationScanText,
                '(?i)\bpRequest\b').Count -ne 1 -or
            [regex]::Matches(
                $topologyIoDeclarationScanText,
                '(?i)\bpResponse\b').Count -ne 1 -or
            @($topologyIoPointerDeclarations | Where-Object {
                $_.Groups['Name'].Value -notin @('pRequest', 'pResponse') -or
                $_.Groups['Type'].Value -ine 'USINT'
            }).Count -ne 0 -or
            $topologyIoHandlerScanText -match
                '(?i)\b(?:POINTER|REFERENCE)\s+TO\b') {
            throw ('LMCDiagnosticsService topology/I/O helper may declare only ' +
                'the ABI pRequest/pResponse ^USINT pointers; local pointer or ' +
                'reference aliases are forbidden.')
        }
        if ([regex]::Matches(
                $topologyIoHandlerScanText,
                '\binputLatchConnected\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $topologyIoHandlerScanText,
                'inputLatchConnected\s*:=\s*' +
                'IsClientConnected\(#InputLatch\)\s*<>\s*0;').Count -ne 1) {
            throw ('LMCDiagnosticsService topology/I/O helper must capture the ' +
                'InputLatch client connection exactly once and convert DINT to BOOL.')
        }
        $inputLatchCaptureMatch = [regex]::Match(
            $topologyIoHandlerBlock,
            'inputLatchConnected\s*:=\s*' +
            'IsClientConnected\(#InputLatch\)\s*<>\s*0;')
        $topologyIoCaseMatch = [regex]::Match(
            $topologyIoHandlerBlock,
            '(?i)\bcase\s+CommandId\s+of\b')
        if (-not $topologyIoCaseMatch.Success -or
            $inputLatchCaptureMatch.Index -ge $topologyIoCaseMatch.Index) {
            throw ('LMCDiagnosticsService topology/I/O helper must sample the ' +
                'InputLatch connection before dispatching any command case.')
        }
        Assert-ExactLasalCommandRouteIds `
            -RouterBlock $diagnosticsServiceHandleBlock `
            -Owner 'LMCDiagnosticsService delegated topology/I/O route' `
            -CallPattern 'HandleEtherCATTopologyIoRequest\s*\(' `
            -ExpectedCommandIds $topologyIoCommandIds
        Assert-ExactLasalTopLevelCommandCaseIds `
            -FunctionBlock $topologyIoHandlerBlock `
            -Owner 'LMCDiagnosticsService.HandleEtherCATTopologyIoRequest' `
            -ExpectedCommandIds $topologyIoCommandIds
        Assert-Match $diagnosticsServiceHandleBlock (
            '(?s)ResponseSize := HandleEtherCATTopologyIoRequest\(\s*' +
            'CommandId:=CommandId,.*?' +
            'pRequest:=pRequest,.*?RequestSize:=RequestSize,.*?' +
            'pResponse:=pResponse,.*?ResponseCapacity:=ResponseCapacity,.*?' +
            'CallerSessionEpoch:=CallerSessionEpoch,.*?' +
            'CurrentDiagnosticsBootId:=currentBootId\);\s*RETURN;') (
            'LMCDiagnosticsService delegated topology/I/O route must return the ' +
            'helper response directly without falling through to another wrapper.')
        Assert-Match $topologyIoHandlerBlock (
            '(?s)else\s*detailCode:=2;\s*end_case;\s*' +
            'if detailCode <> 0 then\s*' +
            '\(pResponse \+ 4\)\^\$UINT := 1;\s*' +
            '\(pResponse \+ 6\)\^\$INT := LMC_DIAG_ERROR_ID;\s*' +
            '\(pResponse \+ 12\)\^\$UDINT := detailCode;\s*' +
            'ResponseSize := 16;\s*end_if;\s*END_FUNCTION') (
            'LMCDiagnosticsService topology/I/O helper must serialize every ' +
            'local failure as the exact 16-byte diagnostics error envelope.')
    }

    Assert-Match $topologyIoHandlerBlock '(?s)0x7E11:\s*if RequestSize <> 8 then detailCode:=12;.*?ResponseCapacity < 44 then detailCode:=20;.*?CatalogIndex:=0x0200.*?ResponseSize:=44;.*?0x7E12:\s*if RequestSize = 16 then.*?expectedMapRevision.*?LMC_DIAG_TOPOLOGY_REVISION then detailCode:=26;.*?maxEntries <> 1.*?startIndex >= 7.*?ResponseCapacity < 124.*?CatalogIndex:=0x8000 or\s*\(startIndex shl 8\) or 1.*?ResponseSize:=124;' 'LMCDiagnosticsService 0x7E11/0x7E12 exact request, one-entry chunk, and response bounds are incomplete.'

    if ($topologyIoReadIntegrated) {
        $nodeHealthCaseBlock = [regex]::Match(
            $topologyIoHandlerBlock,
            '(?ms)^[ \t]*0x7E13\s*:.*?(?=^[ \t]*0x7E22\s*:)').Value
        $ioReadStartMatch = [regex]::Match(
            $topologyIoHandlerBlock,
            '(?m)^[ \t]*0x7E22\s*:')
        if ([string]::IsNullOrWhiteSpace($nodeHealthCaseBlock) -or
            -not $ioReadStartMatch.Success) {
            throw 'LMCDiagnosticsService 0x7E13/0x7E22 case extraction failed.'
        }
        if ($topologyIoOutputIntegrated) {
            $ioReadCaseBlock = [regex]::Match(
                $topologyIoHandlerBlock,
                '(?ms)^[ \t]*0x7E22\s*:.*?(?=^[ \t]*0x7E23\s*:)').Value
            $outputWriteStartMatch = [regex]::Match(
                $topologyIoHandlerBlock,
                '(?m)^[ \t]*0x7E23\s*:')
            if ([string]::IsNullOrWhiteSpace($ioReadCaseBlock) -or
                -not $outputWriteStartMatch.Success) {
                throw 'LMCDiagnosticsService 0x7E22/0x7E23 case extraction failed.'
            }
            $outputWriteCaseBlock = $topologyIoHandlerBlock.Substring(
                $outputWriteStartMatch.Index)
            $outputWriteCaseDefaultMatch = [regex]::Match(
                $outputWriteCaseBlock,
                '(?is)else\s*detailCode\s*:=\s*2;\s*end_case\s*;')
            if (-not $outputWriteCaseDefaultMatch.Success) {
                throw ('LMCDiagnosticsService 0x7E23 final case-default boundary ' +
                    'was not found.')
            }
            $outputWriteCaseBlock = $outputWriteCaseBlock.Substring(
                0,
                $outputWriteCaseDefaultMatch.Index)
        }
        else {
            $ioReadCaseBlock = $topologyIoHandlerBlock.Substring(
                $ioReadStartMatch.Index)
        }
        $ioReadActualCaseBlock = $ioReadCaseBlock
        if (-not $topologyIoOutputIntegrated) {
            $ioReadCaseDefaultMatch = [regex]::Match(
                $ioReadCaseBlock,
                '(?is)else\s*detailCode\s*:=\s*2;\s*end_case\s*;')
            if (-not $ioReadCaseDefaultMatch.Success) {
                throw ('LMCDiagnosticsService 0x7E22 final case-default boundary ' +
                    'was not found.')
            }
            $ioReadActualCaseBlock = $ioReadCaseBlock.Substring(
                0,
                $ioReadCaseDefaultMatch.Index)
        }

        $topologyInfoCaseBlock = [regex]::Match(
            $topologyIoHandlerBlock,
            '(?ms)^[ \t]*0x7E11\s*:.*?(?=^[ \t]*0x7E12\s*:)').Value
        $topologyChunkCaseBlock = [regex]::Match(
            $topologyIoHandlerBlock,
            '(?ms)^[ \t]*0x7E12\s*:.*?(?=^[ \t]*0x7E13\s*:)').Value
        if ([string]::IsNullOrWhiteSpace($topologyInfoCaseBlock) -or
            [string]::IsNullOrWhiteSpace($topologyChunkCaseBlock)) {
            throw ('LMCDiagnosticsService 0x7E11/0x7E12 pointer-ownership ' +
                'case extraction failed.')
        }
        $topologyInfoCaseIndex = $topologyIoHandlerBlock.IndexOf(
            $topologyInfoCaseBlock,
            [StringComparison]::Ordinal)
        $topologyChunkCaseIndex = $topologyIoHandlerBlock.IndexOf(
            $topologyChunkCaseBlock,
            [StringComparison]::Ordinal)
        $topologyIoExecutablePrefix = $topologyIoHandlerBlock.Substring(
            $topologyIoExecutableStart.Index,
            $topologyInfoCaseIndex - $topologyIoExecutableStart.Index)
        $topologyIoExecutablePrefixScanText =
            Get-LasalScanText $topologyIoExecutablePrefix
        if ([regex]::Matches(
                $topologyIoExecutablePrefixScanText,
                '(?i)\bpRequest\b').Count -ne 0 -or
            [regex]::Matches(
                $topologyIoExecutablePrefixScanText,
                '(?i)\bpResponse\b').Count -ne 1 -or
            [regex]::Matches(
                $topologyIoExecutablePrefixScanText,
                ('(?i)\(\s*pResponse\s*=\s*NIL\s*\)\s*\|\s*' +
                 '\(\s*ResponseCapacity\s*<\s*16\s*\)')).Count -ne 1) {
            throw ('LMCDiagnosticsService topology/I/O executable prefix may ' +
                'use no request pointer and exactly one response pointer in ' +
                'the canonical NIL/capacity guard; pre-dispatch pointer reads, ' +
                'writes, and positional calls are forbidden.')
        }
        $topologyInfoCaseScanText =
            Get-LasalScanText $topologyInfoCaseBlock
        if ([regex]::Matches(
                $topologyInfoCaseScanText,
                '(?i)\bpRequest\b').Count -ne 0 -or
            [regex]::Matches(
                $topologyInfoCaseScanText,
                '(?i)\bpResponse\b').Count -ne 1 -or
            [regex]::Matches(
                $topologyInfoCaseScanText,
                ('(?is)BuildCatalogEntry\s*\(\s*pEntry\s*:=\s*' +
                 'pResponse\s*\+\s*16\s*,\s*CatalogIndex\s*:=\s*' +
                 '0x0200\s*\)')).Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E11 may use pResponse only as the ' +
                'canonical bounded BuildCatalogEntry destination and may not ' +
                'read pRequest.')
        }
        $topologyChunkCaseScanText =
            Get-LasalScanText $topologyChunkCaseBlock
        foreach ($topologyChunkRequestRead in @(
                ('expectedMapRevision\s*:=\s*' +
                 '\(\s*pRequest\s*\+\s*8\s*\)\s*\^\s*\$\s*UDINT\s*;'),
                ('startIndex\s*:=\s*' +
                 '\(\s*pRequest\s*\+\s*12\s*\)\s*\^\s*\$\s*UINT\s*;'),
                ('maxEntries\s*:=\s*' +
                 '\(\s*pRequest\s*\+\s*14\s*\)\s*\^\s*\$\s*UINT\s*;'))) {
            if ([regex]::Matches(
                    $topologyChunkCaseScanText,
                    '(?i)' + $topologyChunkRequestRead).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E12 must execute each exact ' +
                    'bounded request field read once; comments and alternate ' +
                    'offsets cannot satisfy the pointer contract.')
            }
        }
        if ([regex]::Matches(
                $topologyChunkCaseScanText,
                '(?i)\bpRequest\b').Count -ne 3 -or
            [regex]::Matches(
                $topologyChunkCaseScanText,
                '(?i)\bpResponse\b').Count -ne 1 -or
            [regex]::Matches(
                $topologyChunkCaseScanText,
                ('(?is)BuildCatalogEntry\s*\(\s*pEntry\s*:=\s*' +
                 'pResponse\s*,\s*CatalogIndex\s*:=\s*0x8000\s+or\s+' +
                 '\(\s*startIndex\s+shl\s+8\s*\)\s+or\s+1\s*\)')).Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E12 must own exactly three ' +
                'canonical request reads and the one bounded catalog response ' +
                'destination; other pointer use is forbidden.')
        }
        $topologyIoLastCaseBlock = $ioReadActualCaseBlock
        if ($topologyIoOutputIntegrated) {
            $topologyIoLastCaseBlock = $outputWriteCaseBlock
        }
        $topologyIoLastCaseIndex = $topologyIoHandlerBlock.IndexOf(
            $topologyIoLastCaseBlock,
            [StringComparison]::Ordinal)
        $topologyIoExecutableSuffix = $topologyIoHandlerBlock.Substring(
            $topologyIoLastCaseIndex + $topologyIoLastCaseBlock.Length)
        $topologyIoExecutableSuffixScanText =
            Get-LasalScanText $topologyIoExecutableSuffix
        foreach ($topologyIoErrorWrite in @(
                ('\(\s*pResponse\s*\+\s*4\s*\)\s*\^\s*\$\s*UINT' +
                 '\s*:=\s*1\s*;'),
                ('\(\s*pResponse\s*\+\s*6\s*\)\s*\^\s*\$\s*INT' +
                 '\s*:=\s*LMC_DIAG_ERROR_ID\s*;'),
                ('\(\s*pResponse\s*\+\s*12\s*\)\s*\^\s*\$\s*UDINT' +
                 '\s*:=\s*detailCode\s*;'))) {
            if ([regex]::Matches(
                    $topologyIoExecutableSuffixScanText,
                    '(?i)' + $topologyIoErrorWrite).Count -ne 1) {
                throw ('LMCDiagnosticsService topology/I/O helper must execute ' +
                    'each canonical error-envelope response write exactly once.')
            }
        }
        if ([regex]::Matches(
                $topologyIoExecutableSuffixScanText,
                '(?i)\bpRequest\b').Count -ne 0 -or
            [regex]::Matches(
                $topologyIoExecutableSuffixScanText,
                '(?i)\bpResponse\b').Count -ne 3 -or
            [regex]::Matches(
                $topologyIoExecutableSuffixScanText,
                '(?i)\bResponseSize\s*:=\s*16\s*;').Count -ne 1) {
            throw ('LMCDiagnosticsService topology/I/O helper suffix may own ' +
                'only the three exact error-envelope response writes and the ' +
                '16-byte ResponseSize; post-dispatch pointer aliases or calls ' +
                'are forbidden.')
        }

        $topologyIoSnapshotConsumerCases = @(
            @{ Name = '0x7E13'; Block = $nodeHealthCaseBlock },
            @{ Name = '0x7E22'; Block = $ioReadActualCaseBlock })
        if ($topologyIoOutputIntegrated) {
            $topologyIoSnapshotConsumerCases += @{
                Name = '0x7E23'
                Block = $outputWriteCaseBlock
            }
        }
        $localSnapshotTypedMutationPattern =
            ('(?i)(?<![A-Za-z0-9_])#?\s*snapshot\s*' +
             '\[\s*[^\]]+\]\s*\$\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
        $localSnapshotDirectMutationPattern =
            ('(?i)(?<![A-Za-z0-9_])#?\s*snapshot' +
             '\s*(?::=|[+\-*/]=)')
        $localSnapshotAggregateMutationPattern =
            ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
             '(?:dest|ptr1)\s*:=\s*#?snapshot(?:\s*\[[^\]]+\])?')
        $localSnapshotAddressPattern =
            '(?i)#\s*snapshot\b(?:\s*\[\s*[^\]]+\])?'
        $canonicalSnapshotCopyDestinationPattern =
            ('(?is)InputLatch\s*\.\s*CopyTopologyIoSnapshot\s*\(\s*' +
             'pDest\s*:=\s*#\s*snapshot\s*\[\s*0\s*\]\s*,\s*' +
             'DestSize\s*:=\s*464\s*\)')
        foreach ($topologyIoSnapshotConsumerCase in
                $topologyIoSnapshotConsumerCases) {
            $snapshotConsumerScanText = Get-LasalScanText `
                $topologyIoSnapshotConsumerCase.Block
            $snapshotAddressUses = [regex]::Matches(
                $snapshotConsumerScanText,
                $localSnapshotAddressPattern)
            $canonicalSnapshotCopyDestinations = [regex]::Matches(
                $snapshotConsumerScanText,
                $canonicalSnapshotCopyDestinationPattern)
            $allCaseAddressEscapes = [regex]::Matches(
                $snapshotConsumerScanText,
                '(?i)#\s*[A-Za-z_][A-Za-z0-9_]*\b')
            Assert-LasalAddressNamesAllowed `
                -Text $topologyIoSnapshotConsumerCase.Block `
                -AllowedNames @('snapshot') `
                -Owner ('LMCDiagnosticsService ' +
                    $topologyIoSnapshotConsumerCase.Name)
            if ([regex]::Matches(
                    $snapshotConsumerScanText,
                    $localSnapshotTypedMutationPattern).Count -ne 0 -or
                [regex]::Matches(
                    $snapshotConsumerScanText,
                    $localSnapshotDirectMutationPattern).Count -ne 0 -or
                [regex]::Matches(
                    $snapshotConsumerScanText,
                    $localSnapshotAggregateMutationPattern).Count -ne 0 -or
                $canonicalSnapshotCopyDestinations.Count -ne 1 -or
                $snapshotAddressUses.Count -ne
                    $canonicalSnapshotCopyDestinations.Count -or
                $allCaseAddressEscapes.Count -ne 1) {
                throw ('LMCDiagnosticsService ' +
                    $topologyIoSnapshotConsumerCase.Name +
                    ' local snapshot must be immutable after exactly one ' +
                    'canonical CopyTopologyIoSnapshot destination use; typed, ' +
                    'aggregate, direct, or aliased writes are forbidden.')
            }
        }

        Assert-LasalExactIfGuard `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern 'RequestSize\s*<>\s*16' `
            -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E13 request-size guard'
        Assert-LasalExactIfGuard `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(ResponseCapacity\s*<\s*72\)') `
            -AssignmentPattern 'detailCode\s*:=\s*20\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E13 response-capacity guard'
        Assert-LasalExactIfGuard `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(expectedMapRevision\s*<>\s*' +
                'LMC_DIAG_TOPOLOGY_REVISION\)') `
            -AssignmentPattern 'detailCode\s*:=\s*26\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E13 topology-revision guard'
        Assert-LasalExactIfGuard `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(inputLatchConnected\s*=\s*FALSE\)') `
            -AssignmentPattern 'detailCode\s*:=\s*11\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E13 connection guard'
        $nodeHealthParseGuardBlock = Get-UniqueLasalIfBlockContaining `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' `
            -RequiredPattern (
                'expectedMapRevision\s*:=\s*' +
                '\(pRequest \+ 8\)\^\$UDINT;') `
            -Message ('LMCDiagnosticsService 0x7E13 request parsing must be ' +
                'dominated by one post-bounds zero-detail guard.')
        $nodeHealthParseGuardThenArm =
            Get-LasalFirstThenArm $nodeHealthParseGuardBlock
        foreach ($nodeHealthParsePattern in @(
                ('expectedMapRevision\s*:=\s*' +
                 '\(pRequest \+ 8\)\^\$UDINT;'),
                ('requestedNodeId\s*:=\s*' +
                 '\(pRequest \+ 12\)\^\$UDINT;'))) {
            if ([regex]::Matches(
                    (Get-LasalScanText $nodeHealthCaseBlock),
                    $nodeHealthParsePattern).Count -ne 1 -or
                [regex]::Matches(
                    (Get-LasalScanText $nodeHealthParseGuardThenArm),
                    $nodeHealthParsePattern).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E13 canonical request field ' +
                    "'$nodeHealthParsePattern' must be read exactly once inside " +
                    'the post-bounds parse guard.')
            }
        }

        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern 'RequestSize\s*<>\s*20' `
            -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 request-size guard'
        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(ResponseCapacity\s*<\s*56\)') `
            -AssignmentPattern 'detailCode\s*:=\s*20\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 response-capacity guard'
        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(expectedMapRevision\s*<>\s*' +
                'LMC_DIAG_TOPOLOGY_REVISION\)') `
            -AssignmentPattern 'detailCode\s*:=\s*26\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 topology-revision guard'
        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(requestReserved\s*<>\s*0\)') `
            -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 reserved-field guard'
        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*\(\s*' +
                '\(requestDirection\s*<\s*1\)\s*\|\s*' +
                '\(requestDirection\s*>\s*2\)\s*\|\s*' +
                '\(requestBitWidth\s*<\s*1\)\s*\|\s*' +
                '\(requestBitWidth\s*>\s*64\)\s*\)') `
            -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 direction-width guard'
        Assert-LasalExactIfGuard `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(inputLatchConnected\s*=\s*FALSE\)') `
            -AssignmentPattern 'detailCode\s*:=\s*11\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E22 connection guard'
        $ioReadParseGuardBlock = Get-UniqueLasalIfBlockContaining `
            -Text $ioReadActualCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' `
            -RequiredPattern (
                'expectedMapRevision\s*:=\s*' +
                '\(pRequest \+ 8\)\^\$UDINT;') `
            -Message ('LMCDiagnosticsService 0x7E22 request parsing must be ' +
                'dominated by one post-bounds zero-detail guard.')
        $ioReadParseGuardThenArm =
            Get-LasalFirstThenArm $ioReadParseGuardBlock
        foreach ($ioReadParsePattern in @(
                ('expectedMapRevision\s*:=\s*' +
                 '\(pRequest \+ 8\)\^\$UDINT;'),
                ('requestIOReference\s*:=\s*' +
                 '\(pRequest \+ 12\)\^\$UDINT;'),
                ('requestDirection\s*:=\s*' +
                 '\(pRequest \+ 16\)\^\$USINT;'),
                ('requestBitWidth\s*:=\s*' +
                 '\(pRequest \+ 17\)\^\$USINT;'),
                ('requestReserved\s*:=\s*' +
                 '\(pRequest \+ 18\)\^\$UINT;'))) {
            if ([regex]::Matches(
                    (Get-LasalScanText $ioReadActualCaseBlock),
                    $ioReadParsePattern).Count -ne 1 -or
                [regex]::Matches(
                    (Get-LasalScanText $ioReadParseGuardThenArm),
                    $ioReadParsePattern).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E22 canonical request field ' +
                    "'$ioReadParsePattern' must be read exactly once inside " +
                    'the post-bounds parse guard.')
            }
        }
        $nodeHealthRequestReadPattern =
            '(?i)\(pRequest\s*\+\s*(?:8|12)\)\^\$UDINT'
        if ([regex]::Matches(
                (Get-LasalScanText $nodeHealthCaseBlock),
                $nodeHealthRequestReadPattern).Count -ne 2 -or
            [regex]::Matches(
                (Get-LasalScanText $nodeHealthCaseBlock),
                '(?i)\bpRequest\b').Count -ne 2 -or
            [regex]::Matches(
                (Get-LasalScanText $nodeHealthParseGuardThenArm),
                $nodeHealthRequestReadPattern).Count -ne 2) {
            throw ('LMCDiagnosticsService 0x7E13 must perform exactly two ' +
                'request-pointer reads inside its post-bounds parse guard.')
        }
        $ioReadRequestReadPattern =
            ('(?i)\(pRequest\s*\+\s*(?:8|12|16|17|18)\)' +
             '\^\$(?:UDINT|USINT|UINT)')
        if ([regex]::Matches(
                (Get-LasalScanText $ioReadActualCaseBlock),
                $ioReadRequestReadPattern).Count -ne 5 -or
            [regex]::Matches(
                (Get-LasalScanText $ioReadActualCaseBlock),
                '(?i)\bpRequest\b').Count -ne 5 -or
            [regex]::Matches(
                (Get-LasalScanText $ioReadParseGuardThenArm),
                $ioReadRequestReadPattern).Count -ne 5) {
            throw ('LMCDiagnosticsService 0x7E22 must perform exactly five ' +
                'request-pointer reads inside its post-bounds parse guard.')
        }
        foreach ($readBoundsOrder in @(
                @{
                    Name = '0x7E13'
                    Block = $nodeHealthCaseBlock
                    RequestCondition = 'RequestSize\s*<>\s*16\s+then'
                    CapacityCondition = (
                        '\(detailCode\s*=\s*0\)\s*&\s*' +
                        '\(ResponseCapacity\s*<\s*72\)\s+then')
                    ParseBlock = $nodeHealthParseGuardBlock
                },
                @{
                    Name = '0x7E22'
                    Block = $ioReadActualCaseBlock
                    RequestCondition = 'RequestSize\s*<>\s*20\s+then'
                    CapacityCondition = (
                        '\(detailCode\s*=\s*0\)\s*&\s*' +
                        '\(ResponseCapacity\s*<\s*56\)\s+then')
                    ParseBlock = $ioReadParseGuardBlock
                })) {
            $requestBoundsBlock = @(Get-LasalStructuredIfBlocks `
                -Text $readBoundsOrder.Block `
                -ConditionPattern $readBoundsOrder.RequestCondition)[0]
            $capacityBoundsBlock = @(Get-LasalStructuredIfBlocks `
                -Text $readBoundsOrder.Block `
                -ConditionPattern $readBoundsOrder.CapacityCondition)[0]
            $requestBoundsIndex = $readBoundsOrder.Block.IndexOf(
                $requestBoundsBlock,
                [StringComparison]::Ordinal)
            $capacityBoundsIndex = $readBoundsOrder.Block.IndexOf(
                $capacityBoundsBlock,
                [StringComparison]::Ordinal)
            $parseBoundsIndex = $readBoundsOrder.Block.IndexOf(
                $readBoundsOrder.ParseBlock,
                [StringComparison]::Ordinal)
            if ($requestBoundsIndex -lt 0 -or
                $capacityBoundsIndex -le
                    ($requestBoundsIndex + $requestBoundsBlock.Length) -or
                $parseBoundsIndex -le
                    ($capacityBoundsIndex + $capacityBoundsBlock.Length)) {
                throw ('LMCDiagnosticsService ' + $readBoundsOrder.Name +
                    ' must finish exact request-size and response-capacity ' +
                    'guards before any request pointer field is read.')
            }
        }
        foreach ($snapshotConsumer in @(
                @{ Name = '0x7E13'; Block = $nodeHealthCaseBlock },
                @{ Name = '0x7E22'; Block = $ioReadCaseBlock })) {
            if ([regex]::Matches(
                    $snapshotConsumer.Block,
                    'InputLatch\.CopyTopologyIoSnapshot\(').Count -ne 1) {
                throw ("LMCDiagnosticsService $($snapshotConsumer.Name) must " +
                    'consume exactly one coherent topology/I/O snapshot.')
            }
            $connectionGuardPattern = (
                'if \(detailCode = 0\) &\s*' +
                '\(inputLatchConnected = FALSE\) then\s*' +
                'detailCode:=11;\s*end_if;')
            Assert-Match $snapshotConsumer.Block $connectionGuardPattern (
                "LMCDiagnosticsService $($snapshotConsumer.Name) must reject " +
                'a disconnected InputLatch with detail 11.')
            $connectionGuardMatch = [regex]::Match(
                $snapshotConsumer.Block,
                $connectionGuardPattern)
            $copyCallMatch = [regex]::Match(
                $snapshotConsumer.Block,
                'InputLatch\.CopyTopologyIoSnapshot\(')
            if (-not $connectionGuardMatch.Success -or
                -not $copyCallMatch.Success -or
                $connectionGuardMatch.Index -ge $copyCallMatch.Index) {
                throw ("LMCDiagnosticsService $($snapshotConsumer.Name) " +
                    'must reject a disconnected InputLatch before snapshot access.')
            }
            Assert-LasalPatternDominatedByIf `
                -Text $snapshotConsumer.Block `
                -ConditionPattern 'detailCode\s*=\s*0\s+then' `
                -RequiredPattern 'InputLatch\.CopyTopologyIoSnapshot\(' `
                -Message ("LMCDiagnosticsService $($snapshotConsumer.Name) " +
                    'snapshot access must be dominated by a zero-detail guard.')
        }

        Assert-Match $nodeHealthCaseBlock (
            '(?s)RequestSize <> 16.*?' +
            'expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT;.*?' +
            'requestedNodeId := \(pRequest \+ 12\)\^\$UDINT;.*?' +
            'expectedMapRevision <> LMC_DIAG_TOPOLOGY_REVISION.*?' +
            'detailCode:=26.*?' +
            'InputLatch\.CopyTopologyIoSnapshot\(.*?DestSize:=464.*?' +
            'ResponseSize:=72') (
            'LMCDiagnosticsService 0x7E13 exact request, revision, snapshot, ' +
            'and response bounds are incomplete.')
        Assert-Match $nodeHealthCaseBlock (
            '(?s)ResponseCapacity < 72.*?detailCode:=20') (
            'LMCDiagnosticsService 0x7E13 72-byte response capacity guard is missing.')
        foreach ($nodeHealthLookup in @(
                @{ NodeId = '0xEC000001'; Offset = 304 },
                @{ NodeId = '0xEC000101'; Offset = 64 },
                @{ NodeId = '0xEC000102'; Offset = 100 },
                @{ NodeId = '0xEC000103'; Offset = 136 },
                @{ NodeId = '0xEC000104'; Offset = 172 },
                @{ NodeId = '0xEC010001'; Offset = 340 },
                @{ NodeId = '0xEC010002'; Offset = 376 })) {
            Assert-Match $nodeHealthCaseBlock (
                '(?s)case requestedNodeId of.*?' +
                $nodeHealthLookup.NodeId + ':\s*healthOffset\s*:=\s*' +
                $nodeHealthLookup.Offset + ';') (
                'LMCDiagnosticsService 0x7E13 NodeId ' +
                $nodeHealthLookup.NodeId + ' does not map to snapshot offset ' +
                $nodeHealthLookup.Offset + '.')
        }
        Assert-Match $nodeHealthCaseBlock (
            '(?s)case requestedNodeId of.*?' +
            'else\s*detailCode\s*:=\s*27;\s*end_case;') (
            'LMCDiagnosticsService 0x7E13 does not reject unknown NodeId with detail 27.')
        Assert-Match $nodeHealthCaseBlock (
            '(?s)nodeDetected := \(nodeClassState <> _NoHardware\) &\s*' +
            '\(nodeClassState <> 0xFFFFFFFF\) &\s*' +
            '\(nodeEtherCATState <> 0\);.*?' +
            'nodeParentIdentityMatched := TRUE;\s*' +
            'if \(requestedNodeId = 0xEC010001\) \|\s*' +
            '\(requestedNodeId = 0xEC010002\) then\s*' +
            'nodeParentIdentityMatched :=\s*' +
            'snapshot\[320\]\$UDINT = _ClassOk;\s*end_if;.*?' +
            'nodeIdentityMatched := nodeDetected &\s*' +
            'nodeParentIdentityMatched &\s*' +
            '\(nodeClassState = _ClassOk\) &\s*' +
            '\(\(nodeSlaveState and 0x00000020\) = 0\);.*?' +
            'nodeDataValid := nodeIdentityMatched &\s*' +
            '\(snapshot\[12\]\$UDINT = 8\) &\s*' +
            '\(snapshot\[16\]\$UDINT = 0\) &\s*' +
            '\(nodeNativeOnline <> 0\) &\s*' +
            '\(nodeEtherCATState = 8\) &\s*' +
            '\(nodeALStatus = 0\);.*?' +
            'nodeHealthFlags := 0x0001;.*?' +
            'if nodeDetected then\s*' +
            'nodeHealthFlags := nodeHealthFlags or 0x0002;.*?' +
            'if nodeIdentityMatched then\s*' +
            'nodeHealthFlags := nodeHealthFlags or 0x0004;.*?' +
            'if nodeDataValid then.*?' +
            'nodeHealthFlags := nodeHealthFlags or 0x0008;.*?' +
            'else\s*nodeHealthFlags := nodeHealthFlags or 0x0010;.*?' +
            'if nodeDetected then\s*wireOnline := 1;.*?' +
            'else\s*wireOnline := 0;\s*nodeEtherCATState := 0;') (
            'LMCDiagnosticsService 0x7E13 must derive detected/identity/data ' +
            'quality from source sentinel, _NoHardware, slot parent identity, ' +
            'SlaveState identity, master OP/missed-frame freshness, native ' +
            'Online, and AL status.')
        Assert-Match $nodeHealthCaseBlock (
            '(?s)' +
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(.*?' +
            'DestSize:=464\);.*?copyResult <> 0.*?detailCode:=31;.*?' +
            'nodeNativeOnline := snapshot\[TO_DINT\(healthOffset\)\]\$DINT;.*?' +
            'nodeEtherCATState := snapshot\[TO_DINT\(healthOffset \+ 4\)\]\$UDINT;.*?' +
            'nodeSlaveState := snapshot\[TO_DINT\(healthOffset \+ 8\)\]\$UDINT;.*?' +
            'nodeALStatus := snapshot\[TO_DINT\(healthOffset \+ 12\)\]\$UDINT;.*?' +
            'nodeClassState := snapshot\[TO_DINT\(healthOffset \+ 16\)\]\$UDINT;.*?' +
            'nodeDS402Status := snapshot\[TO_DINT\(healthOffset \+ 20\)\]\$UDINT;.*?' +
            'nodeAxisError := snapshot\[TO_DINT\(healthOffset \+ 24\)\]\$UDINT;.*?' +
            'nodeLastValidCycle := snapshot\[TO_DINT\(healthOffset \+ 28\)\]\$UDINT;.*?' +
            'nodeLastStateChangeCycle := snapshot\[TO_DINT\(healthOffset \+ 32\)\]\$UDINT;.*?' +
            'if nodeDataValid &\s*' +
            '\(requestedNodeId >= 0xEC000101\) &\s*' +
            '\(requestedNodeId <= 0xEC000104\) then\s*' +
            'nodeHealthFlags := nodeHealthFlags or 0x0020;\s*' +
            'else\s*nodeDS402Status := 0;\s*nodeAxisError := 0;\s*end_if;.*?' +
            '\(pResponse \+ 16\)\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION;.*?' +
            '\(pResponse \+ 20\)\^\$UDINT := requestedNodeId;.*?' +
            '\(pResponse \+ 24\)\^\$UINT := 1;.*?' +
            '\(pResponse \+ 26\)\^\$UINT := nodeHealthFlags;.*?' +
            '\(pResponse \+ 28\)\^\$UDINT := snapshot\[0\]\$UDINT;.*?' +
            '\(pResponse \+ 32\)\^\$UDINT := snapshot\[4\]\$UDINT;.*?' +
            '\(pResponse \+ 36\)\^\$UDINT := snapshot\[8\]\$UDINT;.*?' +
            '\(pResponse \+ 40\)\^\$UDINT := snapshot\[44\]\$UDINT;.*?' +
            '\(pResponse \+ 44\)\^\$USINT := wireOnline;.*?' +
            '\(pResponse \+ 45\)\^\$USINT := TO_USINT\(nodeEtherCATState\);.*?' +
            '\(pResponse \+ 46\)\^\$UINT := TO_UINT\(nodeALStatus\);.*?' +
            '\(pResponse \+ 48\)\^\$UDINT := nodeSlaveState;.*?' +
            '\(pResponse \+ 52\)\^\$UDINT := nodeClassState;.*?' +
            '\(pResponse \+ 56\)\^\$UDINT := nodeDS402Status;.*?' +
            '\(pResponse \+ 60\)\^\$UDINT := nodeAxisError;.*?' +
            '\(pResponse \+ 64\)\^\$UDINT := nodeLastValidCycle;.*?' +
            '\(pResponse \+ 68\)\^\$UDINT := nodeLastStateChangeCycle;') (
            'LMCDiagnosticsService 0x7E13 does not serialize the selected ' +
            '36-byte health record into the exact 72-byte wire offsets.')
        $nodeHealthSelectorMatches = [regex]::Matches(
            $nodeHealthCaseBlock,
            '(?is)\bcase\s+requestedNodeId\s+of\b.*?\bend_case\s*;')
        if ($nodeHealthSelectorMatches.Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E13 must contain exactly one ' +
                'fail-fast static NodeId selector before snapshot access.')
        }
        $nodeHealthSelectorBlock = $nodeHealthSelectorMatches[0].Value
        $nodeHealthCaseScanText = Get-LasalScanText $nodeHealthCaseBlock
        $nodeHealthSelectorScanText = Get-LasalScanText $nodeHealthSelectorBlock
        if ([regex]::Matches(
                $nodeHealthCaseScanText,
                '\bhealthOffset\s*(?::=|[+\-*/]=)').Count -ne 7 -or
            [regex]::Matches(
                $nodeHealthSelectorScanText,
                '\bhealthOffset\s*(?::=|[+\-*/]=)').Count -ne 7) {
            throw ('LMCDiagnosticsService 0x7E13 healthOffset may be assigned ' +
                'only by the seven canonical NodeId selector arms.')
        }
        foreach ($nodeHealthLookup in @(
                @{ NodeId = '0xEC000001'; Offset = 304 },
                @{ NodeId = '0xEC000101'; Offset = 64 },
                @{ NodeId = '0xEC000102'; Offset = 100 },
                @{ NodeId = '0xEC000103'; Offset = 136 },
                @{ NodeId = '0xEC000104'; Offset = 172 },
                @{ NodeId = '0xEC010001'; Offset = 340 },
                @{ NodeId = '0xEC010002'; Offset = 376 })) {
            if ([regex]::Matches(
                    $nodeHealthSelectorScanText,
                    $nodeHealthLookup.NodeId + ':\s*healthOffset\s*:=\s*' +
                        $nodeHealthLookup.Offset + '\s*;').Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E13 healthOffset selector for ' +
                    $nodeHealthLookup.NodeId + ' must occur exactly once.')
            }
        }
        $nodeHealthLocalMutationCounts = [ordered]@{
            'expectedMapRevision' = 1
            'requestedNodeId' = 1
            'nodeNativeOnline' = 1
            'nodeEtherCATState' = 2
            'nodeSlaveState' = 1
            'nodeALStatus' = 1
            'nodeClassState' = 1
            'nodeDS402Status' = 2
            'nodeAxisError' = 2
            'nodeLastValidCycle' = 1
            'nodeLastStateChangeCycle' = 1
            'nodeDetected' = 1
            'nodeParentIdentityMatched' = 2
            'nodeIdentityMatched' = 1
            'nodeDataValid' = 1
            'nodeHealthFlags' = 6
            'wireOnline' = 2
        }
        foreach ($nodeHealthLocalMutation in
                $nodeHealthLocalMutationCounts.GetEnumerator()) {
            if ([regex]::Matches(
                    $nodeHealthCaseScanText,
                    '\b' + $nodeHealthLocalMutation.Key +
                        '\s*(?::=|[+\-*/]=)').Count -ne
                    $nodeHealthLocalMutation.Value) {
                throw ('LMCDiagnosticsService 0x7E13 local ' +
                    $nodeHealthLocalMutation.Key + ' mutation count must be ' +
                    $nodeHealthLocalMutation.Value +
                    '; derived response values may not be overwritten.')
            }
        }
        $nodeHealthCopyGuardBlocks = @(Get-LasalStructuredIfBlocks `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' | Where-Object {
                (Get-LasalFirstThenArm $_) -match
                    'copyResult := InputLatch\.CopyTopologyIoSnapshot\('
            })
        $nodeHealthPayloadGuardBlocks = @(Get-LasalStructuredIfBlocks `
            -Text $nodeHealthCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' | Where-Object {
                $nodeHealthCandidateArm = Get-LasalFirstThenArm $_
                $nodeHealthCandidateArm -match
                    'nodeNativeOnline := snapshot\[' -and
                $nodeHealthCandidateArm -match
                    '\(pResponse \+ 16\)\^\$UDINT :='
            })
        if ($nodeHealthCopyGuardBlocks.Count -ne 1 -or
            $nodeHealthPayloadGuardBlocks.Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E13 requires separate unique ' +
                'zero-detail guards for coherent copy and post-copy ' +
                'load/derive/serialization.')
        }
        $nodeHealthCopyGuardBlock = $nodeHealthCopyGuardBlocks[0]
        $nodeHealthPayloadGuardBlock = $nodeHealthPayloadGuardBlocks[0]
        $nodeHealthCopyGuardThenArm =
            Get-LasalFirstThenArm $nodeHealthCopyGuardBlock
        $nodeHealthPayloadGuardThenArm =
            Get-LasalFirstThenArm $nodeHealthPayloadGuardBlock
        Assert-Match $nodeHealthCopyGuardThenArm (
            '(?s)copyResult := InputLatch\.CopyTopologyIoSnapshot\(\s*' +
            'pDest:=#snapshot\[0\],\s*DestSize:=464\);\s*' +
            'if copyResult <> 0 then\s*detailCode:=31;\s*end_if;') (
            'LMCDiagnosticsService 0x7E13 copy guard must map failure to detail 31.')
        foreach ($nodeHealthPayloadStep in @(
                'nodeNativeOnline\s*:=\s*snapshot\[',
                'nodeDetected\s*:=',
                '\(pResponse \+ 16\)\^\$UDINT\s*:=')) {
            if ([regex]::Matches(
                    $nodeHealthCaseBlock,
                    $nodeHealthPayloadStep).Count -ne 1 -or
                [regex]::Matches(
                    $nodeHealthPayloadGuardThenArm,
                    $nodeHealthPayloadStep).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E13 payload step ' +
                    "'$nodeHealthPayloadStep' must occur exactly once inside " +
                    'the fresh post-copy zero-detail true arm.')
            }
        }
        $nodeHealthConnectionGuardMatch = [regex]::Match(
            $nodeHealthCaseBlock,
            ('if \(detailCode = 0\) &\s*' +
             '\(inputLatchConnected = FALSE\) then\s*' +
             'detailCode:=11;\s*end_if;'))
        $nodeHealthCopyGuardIndex = $nodeHealthCaseBlock.IndexOf(
            $nodeHealthCopyGuardBlock,
            [StringComparison]::Ordinal)
        $nodeHealthPayloadGuardIndex = $nodeHealthCaseBlock.IndexOf(
            $nodeHealthPayloadGuardBlock,
            [StringComparison]::Ordinal)
        if (-not $nodeHealthConnectionGuardMatch.Success -or
            $nodeHealthCopyGuardIndex -le
                ($nodeHealthConnectionGuardMatch.Index +
                    $nodeHealthConnectionGuardMatch.Length) -or
            $nodeHealthCopyGuardIndex -le
                ($nodeHealthSelectorMatches[0].Index +
                    $nodeHealthSelectorMatches[0].Length) -or
            $nodeHealthPayloadGuardIndex -le
                ($nodeHealthCopyGuardIndex +
                    $nodeHealthCopyGuardBlock.Length)) {
            throw ('LMCDiagnosticsService 0x7E13 must finish connection/static ' +
                'selection before a fresh copy guard, then re-enter a fresh ' +
                'zero-detail guard for all snapshot load/derive/serialization.')
        }
        Assert-Match $ioReadCaseBlock (
            '(?s)RequestSize <> 20.*?' +
            'expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT;.*?' +
            'requestIOReference := \(pRequest \+ 12\)\^\$UDINT;.*?' +
            'requestDirection := \(pRequest \+ 16\)\^\$USINT;.*?' +
            'requestBitWidth := \(pRequest \+ 17\)\^\$USINT;.*?' +
            'requestReserved := \(pRequest \+ 18\)\^\$UINT;.*?' +
            'expectedMapRevision <> LMC_DIAG_TOPOLOGY_REVISION.*?detailCode:=26.*?' +
            'InputLatch\.CopyTopologyIoSnapshot\(.*?DestSize:=464.*?' +
            'ResponseSize:=56') (
            'LMCDiagnosticsService 0x7E22 exact request, revision, snapshot, ' +
            'and response bounds are incomplete.')
        Assert-Match $ioReadCaseBlock (
            '(?s)ResponseCapacity < 56.*?detailCode:=20') (
            'LMCDiagnosticsService 0x7E22 56-byte response capacity guard is missing.')
        Assert-Match $ioReadCaseBlock (
            '(?s)' +
            'requestReserved <> 0.*?detailCode:=12;.*?' +
            'requestDirection < 1.*?requestDirection > 2.*?' +
            'requestBitWidth < 1.*?requestBitWidth > 64.*?detailCode:=12;') (
            'LMCDiagnosticsService 0x7E22 reserved, direction, and width ' +
            'request validation is incomplete.')
        $ioReferenceCaseMatches = [regex]::Matches(
            $ioReadCaseBlock,
            '(?is)\bcase\s+requestIOReference\s+of\b.*?\bend_case\s*;')
        if ($ioReferenceCaseMatches.Count -ne 2) {
            throw ('LMCDiagnosticsService 0x7E22 must use one fail-fast static ' +
                'selector case before the copy and one payload-load case after it.')
        }
        $ioReferenceSelectorBlock = $ioReferenceCaseMatches[0].Value
        $ioReferencePayloadBlock = $ioReferenceCaseMatches[1].Value
        Assert-Match $ioReferenceSelectorBlock (
            '(?s)case requestIOReference of\s*' +
            '0x00010001:\s*' +
            'if \(requestDirection <> 1\) \|\s*' +
            '\(requestBitWidth <> 32\) then\s*' +
            'detailCode:=28;\s*else\s*' +
            'ioNodeId := 0xEC010001;\s*end_if;.*?' +
            '0x00010002:\s*' +
            'if \(requestDirection <> 2\) \|\s*' +
            '\(requestBitWidth <> 32\) then\s*' +
            'detailCode:=28;\s*else\s*' +
            'ioNodeId := 0xEC010002;\s*end_if;.*?' +
            'else\s*detailCode:=28;\s*end_case;') (
            'LMCDiagnosticsService 0x7E22 static IOReference/direction/width ' +
            'selector is incomplete.')
        Assert-Match $ioReferencePayloadBlock (
            '(?s)case requestIOReference of\s*' +
            '0x00010001:\s*' +
            'ioValueLow := snapshot\[412\]\$UDINT;.*?' +
            'ioValidMaskLow := snapshot\[420\]\$UDINT;.*?' +
            'ioStatus := snapshot\[428\]\$UINT;.*?' +
            'ioCycle := snapshot\[432\]\$UDINT;.*?' +
            'ioOutputRevision := 0;.*?' +
            '0x00010002:\s*' +
            'ioValueLow := snapshot\[436\]\$UDINT;.*?' +
            'ioValidMaskLow := snapshot\[444\]\$UDINT;.*?' +
            'ioStatus := snapshot\[452\]\$UINT;.*?' +
            'ioCycle := snapshot\[456\]\$UDINT;.*?' +
            'ioOutputRevision := snapshot\[460\]\$UDINT;.*?' +
            'end_case;') (
            'LMCDiagnosticsService 0x7E22 post-copy snapshot payload lookup ' +
            'contract is incomplete.')
        $ioReadCopyForSelectorMatch = [regex]::Match(
            $ioReadCaseBlock,
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(')
        Assert-Match $ioReadCaseBlock (
            '(?s)copyResult := InputLatch\.CopyTopologyIoSnapshot\(\s*' +
            'pDest:=#snapshot\[0\],\s*DestSize:=464\);\s*' +
            'if copyResult <> 0 then\s*detailCode:=31;\s*end_if;') (
            'LMCDiagnosticsService 0x7E22 must map coherent snapshot copy ' +
            'failure to detail 31 before any payload load.')
        if (-not $ioReadCopyForSelectorMatch.Success -or
            ($ioReferenceCaseMatches[0].Index +
                $ioReferenceCaseMatches[0].Length) -ge
                    $ioReadCopyForSelectorMatch.Index -or
            ($ioReadCopyForSelectorMatch.Index +
                $ioReadCopyForSelectorMatch.Length) -ge
                    $ioReferenceCaseMatches[1].Index) {
            throw ('LMCDiagnosticsService 0x7E22 must finish static fail-fast ' +
                'selection before the coherent copy and load payload only after it.')
        }
        Assert-LasalPatternDominatedByIf `
            -Text $ioReadCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' `
            -RequiredPattern 'ioValueLow\s*:=\s*snapshot\[412\]\$UDINT;' `
            -Message ('LMCDiagnosticsService 0x7E22 payload load must be ' +
                'dominated by a zero-detail guard after a successful copy.')
        $ioReadCopyGuardBlocks = @(Get-LasalStructuredIfBlocks `
            -Text $ioReadCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' | Where-Object {
                (Get-LasalFirstThenArm $_) -match
                    'copyResult := InputLatch\.CopyTopologyIoSnapshot\('
            })
        $ioReadPayloadGuardBlocks = @(Get-LasalStructuredIfBlocks `
            -Text $ioReadCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' | Where-Object {
                (Get-LasalFirstThenArm $_) -match
                    'ioValueLow\s*:=\s*snapshot\[412\]\$UDINT;'
            })
        if ($ioReadCopyGuardBlocks.Count -ne 1 -or
            $ioReadPayloadGuardBlocks.Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E22 requires separate unique ' +
                'zero-detail guards for copy and post-copy payload load.')
        }
        $ioReadCopyGuardBlock = $ioReadCopyGuardBlocks[0]
        $ioReadPayloadGuardBlock = $ioReadPayloadGuardBlocks[0]
        $ioReadCopyGuardThenArm =
            Get-LasalFirstThenArm $ioReadCopyGuardBlock
        $ioReadPayloadGuardThenArm =
            Get-LasalFirstThenArm $ioReadPayloadGuardBlock
        Assert-Match $ioReadCopyGuardThenArm (
            '(?s)copyResult := InputLatch\.CopyTopologyIoSnapshot\(\s*' +
            'pDest:=#snapshot\[0\],\s*DestSize:=464\);\s*' +
            'if copyResult <> 0 then\s*detailCode:=31;\s*end_if;') (
            'LMCDiagnosticsService 0x7E22 copy guard must map failure before exit.')
        if ($ioReadPayloadGuardThenArm.IndexOf(
                $ioReferencePayloadBlock,
                [StringComparison]::Ordinal) -lt 0) {
            throw ('LMCDiagnosticsService 0x7E22 payload selector must be fully ' +
                'inside the post-copy zero-detail true branch.')
        }
        $ioReadCaseScanText = Get-LasalScanText $ioReadActualCaseBlock
        $ioReadLocalMutationCounts = [ordered]@{
            'expectedMapRevision' = 1
            'requestIOReference' = 1
            'requestDirection' = 1
            'requestBitWidth' = 1
            'requestReserved' = 1
            'ioNodeId' = 2
            'ioValueLow' = 2
            'ioValidMaskLow' = 2
            'ioStatus' = 2
            'ioCycle' = 2
            'ioOutputRevision' = 2
        }
        foreach ($ioReadLocalMutation in
                $ioReadLocalMutationCounts.GetEnumerator()) {
            if ([regex]::Matches(
                    $ioReadCaseScanText,
                    '\b' + $ioReadLocalMutation.Key +
                        '\s*(?::=|[+\-*/]=)').Count -ne
                    $ioReadLocalMutation.Value) {
                throw ('LMCDiagnosticsService 0x7E22 local ' +
                    $ioReadLocalMutation.Key + ' mutation count must be ' +
                    $ioReadLocalMutation.Value +
                    '; parsed and derived payload values may not be overwritten.')
            }
        }
        $nodeHealthCanonicalResponseWrites = @(
            ('\(\s*pResponse\s*\+\s*16\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*LMC_DIAG_TOPOLOGY_REVISION\s*;'),
            ('\(\s*pResponse\s*\+\s*20\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*requestedNodeId\s*;'),
            ('\(\s*pResponse\s*\+\s*24\s*\)\s*\^\s*\$\s*UINT' +
             '\s*:=\s*1\s*;'),
            ('\(\s*pResponse\s*\+\s*26\s*\)\s*\^\s*\$\s*UINT' +
             '\s*:=\s*nodeHealthFlags\s*;'),
            ('\(\s*pResponse\s*\+\s*28\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*snapshot\s*\[\s*0\s*\]\s*\$\s*UDINT\s*;'),
            ('\(\s*pResponse\s*\+\s*32\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*snapshot\s*\[\s*4\s*\]\s*\$\s*UDINT\s*;'),
            ('\(\s*pResponse\s*\+\s*36\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*snapshot\s*\[\s*8\s*\]\s*\$\s*UDINT\s*;'),
            ('\(\s*pResponse\s*\+\s*40\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*snapshot\s*\[\s*44\s*\]\s*\$\s*UDINT\s*;'),
            ('\(\s*pResponse\s*\+\s*44\s*\)\s*\^\s*\$\s*USINT' +
             '\s*:=\s*wireOnline\s*;'),
            ('\(\s*pResponse\s*\+\s*45\s*\)\s*\^\s*\$\s*USINT' +
             '\s*:=\s*TO_USINT\s*\(\s*nodeEtherCATState\s*\)\s*;'),
            ('\(\s*pResponse\s*\+\s*46\s*\)\s*\^\s*\$\s*UINT' +
             '\s*:=\s*TO_UINT\s*\(\s*nodeALStatus\s*\)\s*;'),
            ('\(\s*pResponse\s*\+\s*48\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeSlaveState\s*;'),
            ('\(\s*pResponse\s*\+\s*52\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeClassState\s*;'),
            ('\(\s*pResponse\s*\+\s*56\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeDS402Status\s*;'),
            ('\(\s*pResponse\s*\+\s*60\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeAxisError\s*;'),
            ('\(\s*pResponse\s*\+\s*64\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeLastValidCycle\s*;'),
            ('\(\s*pResponse\s*\+\s*68\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*nodeLastStateChangeCycle\s*;'))
        $ioReadCanonicalResponseWrites = @(
            ('\(\s*pResponse\s*\+\s*16\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*LMC_DIAG_TOPOLOGY_REVISION\s*;'),
            ('\(\s*pResponse\s*\+\s*20\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*requestIOReference\s*;'),
            ('\(\s*pResponse\s*\+\s*24\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*ioNodeId\s*;'),
            ('\(\s*pResponse\s*\+\s*28\s*\)\s*\^\s*\$\s*USINT' +
             '\s*:=\s*requestDirection\s*;'),
            ('\(\s*pResponse\s*\+\s*29\s*\)\s*\^\s*\$\s*USINT' +
             '\s*:=\s*requestBitWidth\s*;'),
            ('\(\s*pResponse\s*\+\s*30\s*\)\s*\^\s*\$\s*UINT' +
             '\s*:=\s*ioStatus\s*;'),
            ('\(\s*pResponse\s*\+\s*32\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*ioValueLow\s*;'),
            ('\(\s*pResponse\s*\+\s*36\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*0\s*;'),
            ('\(\s*pResponse\s*\+\s*40\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*ioValidMaskLow\s*;'),
            ('\(\s*pResponse\s*\+\s*44\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*0\s*;'),
            ('\(\s*pResponse\s*\+\s*48\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*ioCycle\s*;'),
            ('\(\s*pResponse\s*\+\s*52\s*\)\s*\^\s*\$\s*UDINT' +
             '\s*:=\s*ioOutputRevision\s*;'))
        foreach ($readCaseOwnership in @(
                @{
                    Name = '0x7E13'
                    CaseBlock = $nodeHealthCaseBlock
                    CopyArm = $nodeHealthCopyGuardThenArm
                    PayloadBlock = $nodeHealthPayloadGuardBlock
                    PayloadArm = $nodeHealthPayloadGuardThenArm
                    AllowedDetails = '11|12|20|26|27|31'
                    ExpectedResponseWrites = 17
                    ExpectedResponseSize = 72
                    CanonicalResponseWrites = $nodeHealthCanonicalResponseWrites
                },
                @{
                    Name = '0x7E22'
                    CaseBlock = $ioReadActualCaseBlock
                    CopyArm = $ioReadCopyGuardThenArm
                    PayloadBlock = $ioReadPayloadGuardBlock
                    PayloadArm = $ioReadPayloadGuardThenArm
                    AllowedDetails = '11|12|20|26|28|31'
                    ExpectedResponseWrites = 12
                    ExpectedResponseSize = 56
                    CanonicalResponseWrites = $ioReadCanonicalResponseWrites
                })) {
            $readCaseScanText = Get-LasalScanText $readCaseOwnership.CaseBlock
            $readCopyArmScanText = Get-LasalScanText $readCaseOwnership.CopyArm
            $readPayloadArmScanText = Get-LasalScanText $readCaseOwnership.PayloadArm
            if ([regex]::Matches(
                    $readCaseScanText,
                    '\bcopyResult\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                [regex]::Matches(
                    $readCopyArmScanText,
                    ('copyResult\s*:=\s*' +
                     'InputLatch\.CopyTopologyIoSnapshot\(\s*' +
                     'pDest:=#snapshot\[0\],\s*DestSize:=464\);')).Count -ne 1) {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' must assign copyResult exactly once inside its coherent-copy guard.')
            }
            $readDetailMutations = [regex]::Matches(
                $readCaseScanText,
                '\bdetailCode\s*(?::=|[+\-*/]=)')
            $readCanonicalDetailMutations = [regex]::Matches(
                $readCaseScanText,
                '\bdetailCode\s*:=\s*(?:' +
                    $readCaseOwnership.AllowedDetails + ')\s*;')
            if ($readDetailMutations.Count -ne
                    $readCanonicalDetailMutations.Count -or
                $readCaseScanText -match
                    '\bdetailCode\s*:=\s*0\s*;') {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' detailCode must remain sticky and use only canonical ' +
                    'nonzero failure constants inside the command case.')
            }
            $readResponseMutationPattern =
                ('(?i)(?:\bpResponse\b|' +
                 '\(\s*pResponse\s*\+\s*[^\)]+\))\s*\^\s*\$\s*' +
                 '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
            $readCaseResponseMutations = [regex]::Matches(
                $readCaseScanText,
                $readResponseMutationPattern)
            $readPayloadResponseMutations = [regex]::Matches(
                $readPayloadArmScanText,
                $readResponseMutationPattern)
            foreach ($readCanonicalResponseWrite in
                    $readCaseOwnership.CanonicalResponseWrites) {
                $readCaseCanonicalResponseMatches = [regex]::Matches(
                    $readCaseScanText,
                    '(?i)' + $readCanonicalResponseWrite)
                $readPayloadCanonicalResponseMatches = [regex]::Matches(
                    $readPayloadArmScanText,
                    '(?i)' + $readCanonicalResponseWrite)
                if ($readCaseCanonicalResponseMatches.Count -ne 1 -or
                    $readPayloadCanonicalResponseMatches.Count -ne 1 -or
                    (Get-LasalControlDepthAtIndex `
                        -Text $readCaseOwnership.PayloadArm `
                        -Index $readPayloadCanonicalResponseMatches[0].Index) -ne 1) {
                    throw ('LMCDiagnosticsService ' +
                        $readCaseOwnership.Name +
                        ' must execute each canonical response offset, type, ' +
                        'and RHS exactly once at the direct control depth of ' +
                        'the fresh post-copy zero-detail payload guard; comments ' +
                        'or nested conditional writes cannot satisfy the ' +
                        'executable wire contract.')
                }
            }
            $readCanonicalResponseSizeMatches = [regex]::Matches(
                $readPayloadArmScanText,
                ('(?i)\bResponseSize\s*:=\s*' +
                 $readCaseOwnership.ExpectedResponseSize + '\s*;'))
            if ($readCaseResponseMutations.Count -ne
                    $readCaseOwnership.ExpectedResponseWrites -or
                $readPayloadResponseMutations.Count -ne
                    $readCaseOwnership.ExpectedResponseWrites -or
                [regex]::Matches(
                    $readCaseScanText,
                    '(?i)\bpResponse\b').Count -ne
                    $readCaseOwnership.ExpectedResponseWrites -or
                [regex]::Matches(
                    $readCaseScanText,
                    '\bResponseSize\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                [regex]::Matches(
                    $readPayloadArmScanText,
                    '\bResponseSize\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                $readCanonicalResponseSizeMatches.Count -ne 1 -or
                (Get-LasalControlDepthAtIndex `
                    -Text $readCaseOwnership.PayloadArm `
                    -Index $readCanonicalResponseSizeMatches[0].Index) -ne 1 -or
                $readCaseScanText -match
                    '(?i)\bpResponse\b\s*(?::=|[+\-*/]=)' -or
                $readCaseScanText -match
                    ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
                     '(?:dest|ptr1)\s*:=\s*' +
                     '(?:pResponse\b|\(\s*pResponse\s*\+\s*[^\)]+\))')) {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' must serialize exactly ' +
                    $readCaseOwnership.ExpectedResponseWrites +
                    ' canonical response fields, and ResponseSize, only inside ' +
                    'the fresh post-copy zero-detail payload guard without ' +
                    'additional, aliased, or aggregate overwrite.')
            }
            if ($readCaseScanText -match
                    '(?i)\b(?:RETURN|EXIT|CONTINUE|GOTO)\b') {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' may not transfer control around its sticky guarded ' +
                    'response stages.')
            }
            $readPayloadBlockIndex = $readCaseOwnership.CaseBlock.IndexOf(
                $readCaseOwnership.PayloadBlock,
                [StringComparison]::Ordinal)
            if ($readPayloadBlockIndex -lt 0) {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' payload guard range was not found.')
            }
            $readCaseTail = $readCaseOwnership.CaseBlock.Substring(
                $readPayloadBlockIndex + $readCaseOwnership.PayloadBlock.Length)
            if (-not [string]::IsNullOrWhiteSpace(
                    (Get-LasalScanText $readCaseTail))) {
                throw ('LMCDiagnosticsService ' + $readCaseOwnership.Name +
                    ' payload guard must be the final executable command-case ' +
                    'stage so a failed copy cannot be reset or serialized later.')
            }
        }
        $ioReadConnectionGuardMatch = [regex]::Match(
            $ioReadCaseBlock,
            ('if \(detailCode = 0\) &\s*' +
             '\(inputLatchConnected = FALSE\) then\s*' +
             'detailCode:=11;\s*end_if;'))
        $ioReadCopyGuardIndex = $ioReadCaseBlock.IndexOf(
            $ioReadCopyGuardBlock,
            [StringComparison]::Ordinal)
        $ioReadPayloadGuardIndex = $ioReadCaseBlock.IndexOf(
            $ioReadPayloadGuardBlock,
            [StringComparison]::Ordinal)
        if (-not $ioReadConnectionGuardMatch.Success -or
            $ioReadCopyGuardIndex -le
                ($ioReadConnectionGuardMatch.Index +
                    $ioReadConnectionGuardMatch.Length) -or
            $ioReadCopyGuardIndex -le
                ($ioReferenceCaseMatches[0].Index +
                    $ioReferenceCaseMatches[0].Length) -or
            $ioReadPayloadGuardIndex -le
                ($ioReadCopyGuardIndex + $ioReadCopyGuardBlock.Length)) {
            throw ('LMCDiagnosticsService 0x7E22 must re-enter a fresh zero-detail ' +
                'guard after connection/static selection for copy, then another ' +
                'fresh guard after copy/failure mapping for payload load.')
        }
        Assert-Match $ioReadCaseBlock (
            '(?s)' +
            '\(pResponse \+ 16\)\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION;.*?' +
            '\(pResponse \+ 20\)\^\$UDINT := requestIOReference;.*?' +
            '\(pResponse \+ 24\)\^\$UDINT := ioNodeId;.*?' +
            '\(pResponse \+ 28\)\^\$USINT := requestDirection;.*?' +
            '\(pResponse \+ 29\)\^\$USINT := requestBitWidth;.*?' +
            '\(pResponse \+ 30\)\^\$UINT := ioStatus;.*?' +
            '\(pResponse \+ 32\)\^\$UDINT := ioValueLow;.*?' +
            '\(pResponse \+ 36\)\^\$UDINT := 0;.*?' +
            '\(pResponse \+ 40\)\^\$UDINT := ioValidMaskLow;.*?' +
            '\(pResponse \+ 44\)\^\$UDINT := 0;.*?' +
            '\(pResponse \+ 48\)\^\$UDINT := ioCycle;.*?' +
            '\(pResponse \+ 52\)\^\$UDINT := ioOutputRevision;') (
            'LMCDiagnosticsService 0x7E22 does not serialize the selected I/O ' +
            'snapshot into the exact 56-byte wire offsets.')

        $nodeHealthCopyMatch = [regex]::Match(
            $nodeHealthCaseBlock,
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(')
        $nodeHealthLoadMatch = [regex]::Match(
            $nodeHealthCaseBlock,
            'nodeNativeOnline := snapshot\[TO_DINT\(healthOffset\)\]\$DINT;')
        $nodeHealthDeriveMatch = [regex]::Match(
            $nodeHealthCaseBlock,
            'nodeDetected :=')
        $nodeHealthSerializeMatch = [regex]::Match(
            $nodeHealthCaseBlock,
            '\(pResponse \+ 16\)\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION;')
        $ioReadCopyMatch = [regex]::Match(
            $ioReadCaseBlock,
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(')
        $ioReadPayloadLoadMatch = [regex]::Match(
            $ioReadCaseBlock,
            'ioValueLow := snapshot\[(?:412|436)\]\$UDINT;')
        $ioReadSerializeMatch = [regex]::Match(
            $ioReadCaseBlock,
            '\(pResponse \+ 16\)\^\$UDINT := LMC_DIAG_TOPOLOGY_REVISION;')
        foreach ($orderedSnapshotConsumer in @(
                @{ Name = '0x7E13'; Matches = @(
                    $nodeHealthCopyMatch,
                    $nodeHealthLoadMatch,
                    $nodeHealthDeriveMatch,
                    $nodeHealthSerializeMatch) },
                @{ Name = '0x7E22'; Matches = @(
                    $ioReadCopyMatch,
                    $ioReadPayloadLoadMatch,
                    $ioReadSerializeMatch) })) {
            $previousConsumerIndex = -1
            foreach ($consumerMatch in $orderedSnapshotConsumer.Matches) {
                if (-not $consumerMatch.Success -or
                    $consumerMatch.Index -le $previousConsumerIndex) {
                    throw ("LMCDiagnosticsService $($orderedSnapshotConsumer.Name) " +
                        'must execute one coherent copy before any payload load, ' +
                        'then derive and serialize; static reference validation ' +
                        'may run before the copy.')
                }
                $previousConsumerIndex = $consumerMatch.Index
            }
        }
    }
    if ($topologyIoOutputIntegrated) {
        if ([regex]::Matches(
                $outputWriteCaseBlock,
                'InputLatch\.CopyTopologyIoSnapshot\(').Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E23 must validate and queue from ' +
                'exactly one coherent topology/I/O snapshot.')
        }
        $outputConnectionGuardPattern = (
            'if \(detailCode = 0\) &\s*' +
            '\(inputLatchConnected = FALSE\) then\s*' +
            'detailCode:=11;\s*end_if;')
        Assert-Match $outputWriteCaseBlock $outputConnectionGuardPattern (
            'LMCDiagnosticsService 0x7E23 must reject a disconnected ' +
            'InputLatch with detail 11.')
        $outputConnectionGuardMatch = [regex]::Match(
            $outputWriteCaseBlock,
            $outputConnectionGuardPattern)
        $firstOutputClientCall = [regex]::Match(
            $outputWriteCaseBlock,
            'InputLatch\.(?:IsOutputReusable|CopyTopologyIoSnapshot|' +
            'TryQueueOutputWrite)\(')
        if (-not $firstOutputClientCall.Success -or
            $outputConnectionGuardMatch.Index -ge $firstOutputClientCall.Index) {
            throw ('LMCDiagnosticsService 0x7E23 must reject a disconnected ' +
                'InputLatch before any output-owner client call.')
        }
        foreach ($outputClientCallPattern in @(
                'InputLatch\.IsOutputReusable\(',
                'InputLatch\.CopyTopologyIoSnapshot\(',
                'InputLatch\.TryQueueOutputWrite\(')) {
            Assert-LasalPatternDominatedByIf `
                -Text $outputWriteCaseBlock `
                -ConditionPattern 'detailCode\s*=\s*0\s+then' `
                -RequiredPattern $outputClientCallPattern `
                -Message ('LMCDiagnosticsService 0x7E23 client call must be ' +
                    'dominated by a zero-detail guard after connection validation.')
        }
        Assert-Match $outputWriteCaseBlock (
            '(?s)RequestSize <> 40.*?' +
            'ResponseCapacity < 32.*?' +
            'expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT;.*?' +
            'requestIOReference := \(pRequest \+ 12\)\^\$UDINT;.*?' +
            'requestValueLow := \(pRequest \+ 16\)\^\$UDINT;.*?' +
            'requestValueHigh := \(pRequest \+ 20\)\^\$UDINT;.*?' +
            'requestMaskLow := \(pRequest \+ 24\)\^\$UDINT;.*?' +
            'requestMaskHigh := \(pRequest \+ 28\)\^\$UDINT;.*?' +
            'requestExpectedOutputRevision := \(pRequest \+ 32\)\^\$UDINT;.*?' +
            'requestDiagnosticsBootId := \(pRequest \+ 36\)\^\$UDINT;.*?' +
            'CurrentDiagnosticsBootId = 0.*?detailCode:=11.*?' +
            'requestDiagnosticsBootId <> CurrentDiagnosticsBootId.*?' +
            'detailCode:=25;.*?' +
            'CallerSessionEpoch = 0.*?detailCode:=24;.*?' +
            'expectedMapRevision <> LMC_DIAG_TOPOLOGY_REVISION.*?' +
            'detailCode:=26;.*?' +
            'requestIOReference <> LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE.*?' +
            'detailCode:=28;.*?' +
            'OperationState = LMC_DIAG_SDO_STATE_QUEUED.*?' +
            'OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?detailCode:=9;.*?' +
            'SdoInternalDrainState <> 0.*?detailCode:=9;.*?' +
            'InputLatch\.IsOutputReusable\(\) = FALSE.*?detailCode:=16;.*?' +
            'if \(detailCode = 0\) &\s*' +
            '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = FALSE\) then\s*' +
            'detailCode:=7;\s*end_if;.*?' +
            'if \(detailCode = 0\) &\s*' +
            '\(LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED = FALSE\) then\s*' +
            'detailCode:=7;\s*end_if;.*?' +
            'requestValueHigh <> 0.*?detailCode:=30;.*?' +
            'requestMaskHigh <> 0.*?detailCode:=30;.*?' +
            'requestMaskLow = 0.*?detailCode:=30;.*?' +
            '\(requestMaskLow and not LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK\) <> 0.*?' +
            'detailCode:=30;.*?' +
            '\(requestValueLow and not requestMaskLow\) <> 0.*?' +
            'detailCode:=30;') (
            'LMCDiagnosticsService dormant 0x7E23 exact request, identity, ' +
            'BootId, high-half, canonical mask, and fail-closed policy ' +
            'validation is incomplete.')
        $outputWriteCaseScanText = Get-LasalScanText $outputWriteCaseBlock
        Assert-LasalExactIfGuard `
            -Text $outputWriteCaseBlock `
            -ConditionPattern 'RequestSize\s*<>\s*40' `
            -AssignmentPattern 'detailCode\s*:=\s*12\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E23 request-size guard'
        Assert-LasalExactIfGuard `
            -Text $outputWriteCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(ResponseCapacity\s*<\s*32\)') `
            -AssignmentPattern 'detailCode\s*:=\s*20\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E23 response-capacity guard'
        Assert-LasalExactIfGuard `
            -Text $outputWriteCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(inputLatchConnected\s*=\s*FALSE\)') `
            -AssignmentPattern 'detailCode\s*:=\s*11\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E23 connection guard'
        $outputWriteParseGuardBlock = Get-UniqueLasalIfBlockContaining `
            -Text $outputWriteCaseBlock `
            -ConditionPattern 'detailCode\s*=\s*0\s+then' `
            -RequiredPattern (
                'expectedMapRevision\s*:=\s*' +
                '\(pRequest \+ 8\)\^\$UDINT;') `
            -Message ('LMCDiagnosticsService 0x7E23 request parsing must be ' +
                'dominated by one post-bounds zero-detail guard.')
        $outputWriteParseGuardThenArm =
            Get-LasalFirstThenArm $outputWriteParseGuardBlock
        $outputServiceStickyPrefix =
            '\(detailCode\s*=\s*0\)\s*&\s*'
        $outputServiceFailureGuards = @(
            @{
                Name = 'current BootId'
                Condition = ($outputServiceStickyPrefix +
                    '\(CurrentDiagnosticsBootId\s*=\s*0\)')
                Detail = 11
            },
            @{
                Name = 'request BootId'
                Condition = ($outputServiceStickyPrefix +
                    '\(requestDiagnosticsBootId\s*<>\s*' +
                    'CurrentDiagnosticsBootId\)')
                Detail = 25
            },
            @{
                Name = 'caller session'
                Condition = ($outputServiceStickyPrefix +
                    '\(CallerSessionEpoch\s*=\s*0\)')
                Detail = 24
            },
            @{
                Name = 'topology revision'
                Condition = ($outputServiceStickyPrefix +
                    '\(expectedMapRevision\s*<>\s*' +
                    'LMC_DIAG_TOPOLOGY_REVISION\)')
                Detail = 26
            },
            @{
                Name = 'I/O reference'
                Condition = ($outputServiceStickyPrefix +
                    '\(requestIOReference\s*<>\s*' +
                    'LMC_DIAG_ECAT_IO_OUTPUT_REFERENCE\)')
                Detail = 28
            },
            @{
                Name = 'active shared ticket'
                Condition = ($outputServiceStickyPrefix +
                    '\(\s*\(OperationState\s*=\s*' +
                    'LMC_DIAG_SDO_STATE_QUEUED\)\s*\|\s*' +
                    '\(OperationState\s*=\s*' +
                    'LMC_DIAG_SDO_STATE_RUNNING\)\s*\)')
                Detail = 9
            },
            @{
                Name = 'late-callback drain'
                Condition = ($outputServiceStickyPrefix +
                    '\(SdoInternalDrainState\s*<>\s*0\)')
                Detail = 9
            },
            @{
                Name = 'global output gate'
                Condition = ($outputServiceStickyPrefix +
                    '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED\s*=\s*FALSE\)')
                Detail = 7
            },
            @{
                Name = 'module output gate'
                Condition = ($outputServiceStickyPrefix +
                    '\(LMC_DIAG_ECAT_IO_WRITE_GT22BA_ENABLED\s*=\s*FALSE\)')
                Detail = 7
            },
            @{
                Name = 'value high half'
                Condition = ($outputServiceStickyPrefix +
                    '\(requestValueHigh\s*<>\s*0\)')
                Detail = 30
            },
            @{
                Name = 'mask high half'
                Condition = ($outputServiceStickyPrefix +
                    '\(requestMaskHigh\s*<>\s*0\)')
                Detail = 30
            },
            @{
                Name = 'nonzero mask'
                Condition = ($outputServiceStickyPrefix +
                    '\(requestMaskLow\s*=\s*0\)')
                Detail = 30
            },
            @{
                Name = 'configured mask bounds'
                Condition = ($outputServiceStickyPrefix +
                    '\(\(requestMaskLow\s+and\s+not\s+' +
                    'LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK\)\s*<>\s*0\)')
                Detail = 30
            },
            @{
                Name = 'value outside mask'
                Condition = ($outputServiceStickyPrefix +
                    '\(\(requestValueLow\s+and\s+not\s+' +
                    'requestMaskLow\)\s*<>\s*0\)')
                Detail = 30
            })
        foreach ($outputServiceFailureGuard in $outputServiceFailureGuards) {
            Assert-LasalExactIfGuard `
                -Text $outputWriteCaseBlock `
                -ConditionPattern $outputServiceFailureGuard.Condition `
                -AssignmentPattern (
                    'detailCode\s*:=\s*' +
                    $outputServiceFailureGuard.Detail + '\s*;') `
                -Owner ('LMCDiagnosticsService 0x7E23 ' +
                    $outputServiceFailureGuard.Name + ' failure guard')
        }
        $outputWriteParsedLocals = @(
            @{
                Name = 'expectedMapRevision'
                Pattern = ('expectedMapRevision\s*:=\s*' +
                    '\(pRequest \+ 8\)\^\$UDINT;')
            },
            @{
                Name = 'requestIOReference'
                Pattern = ('requestIOReference\s*:=\s*' +
                    '\(pRequest \+ 12\)\^\$UDINT;')
            },
            @{
                Name = 'requestValueLow'
                Pattern = ('requestValueLow\s*:=\s*' +
                    '\(pRequest \+ 16\)\^\$UDINT;')
            },
            @{
                Name = 'requestValueHigh'
                Pattern = ('requestValueHigh\s*:=\s*' +
                    '\(pRequest \+ 20\)\^\$UDINT;')
            },
            @{
                Name = 'requestMaskLow'
                Pattern = ('requestMaskLow\s*:=\s*' +
                    '\(pRequest \+ 24\)\^\$UDINT;')
            },
            @{
                Name = 'requestMaskHigh'
                Pattern = ('requestMaskHigh\s*:=\s*' +
                    '\(pRequest \+ 28\)\^\$UDINT;')
            },
            @{
                Name = 'requestExpectedOutputRevision'
                Pattern = ('requestExpectedOutputRevision\s*:=\s*' +
                    '\(pRequest \+ 32\)\^\$UDINT;')
            },
            @{
                Name = 'requestDiagnosticsBootId'
                Pattern = ('requestDiagnosticsBootId\s*:=\s*' +
                    '\(pRequest \+ 36\)\^\$UDINT;')
            })
        $outputWriteParseGuardThenArmScanText =
            Get-LasalScanText $outputWriteParseGuardThenArm
        foreach ($outputWriteParsedLocal in $outputWriteParsedLocals) {
            if ([regex]::Matches(
                    $outputWriteCaseScanText,
                    '\b' + $outputWriteParsedLocal.Name +
                        '\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                [regex]::Matches(
                    $outputWriteCaseScanText,
                    $outputWriteParsedLocal.Pattern).Count -ne 1 -or
                [regex]::Matches(
                    $outputWriteParseGuardThenArmScanText,
                    $outputWriteParsedLocal.Pattern).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 parsed local ' +
                    $outputWriteParsedLocal.Name +
                    ' must be assigned exactly once from its canonical wire ' +
                    'offset inside the post-bounds parse guard.')
            }
        }
        $outputRequestBoundsBlock = @(Get-LasalStructuredIfBlocks `
            -Text $outputWriteCaseBlock `
            -ConditionPattern 'RequestSize\s*<>\s*40\s+then')[0]
        $outputCapacityBoundsBlock = @(Get-LasalStructuredIfBlocks `
            -Text $outputWriteCaseBlock `
            -ConditionPattern (
                '\(detailCode\s*=\s*0\)\s*&\s*' +
                '\(ResponseCapacity\s*<\s*32\)\s+then'))[0]
        $outputRequestBoundsIndex = $outputWriteCaseBlock.IndexOf(
            $outputRequestBoundsBlock,
            [StringComparison]::Ordinal)
        $outputCapacityBoundsIndex = $outputWriteCaseBlock.IndexOf(
            $outputCapacityBoundsBlock,
            [StringComparison]::Ordinal)
        $outputParseGuardIndex = $outputWriteCaseBlock.IndexOf(
            $outputWriteParseGuardBlock,
            [StringComparison]::Ordinal)
        if ($outputRequestBoundsIndex -lt 0 -or
            $outputCapacityBoundsIndex -le
                ($outputRequestBoundsIndex + $outputRequestBoundsBlock.Length) -or
            $outputParseGuardIndex -le
                ($outputCapacityBoundsIndex + $outputCapacityBoundsBlock.Length)) {
            throw ('LMCDiagnosticsService 0x7E23 must finish exact request-size ' +
                'and response-capacity guards before reading any request field.')
        }
        $outputRequestPointerReadPattern =
            ('(?i)\(pRequest\s*\+\s*(?:8|12|16|20|24|28|32|36)\)' +
             '\^\$UDINT')
        if ([regex]::Matches(
                $outputWriteCaseScanText,
                $outputRequestPointerReadPattern).Count -ne 8 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                '(?i)\bpRequest\b').Count -ne 8 -or
            [regex]::Matches(
                $outputWriteParseGuardThenArmScanText,
                $outputRequestPointerReadPattern).Count -ne 8) {
            throw ('LMCDiagnosticsService 0x7E23 must perform exactly eight ' +
                'canonical request-pointer reads, all inside the post-bounds parse guard.')
        }
        Assert-Match $outputWriteCaseBlock (
            '(?s)if detailCode = 0 then\s*' +
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(.*?' +
            'DestSize:=464\);.*?' +
            'copyResult <> 0.*?detailCode:=31;.*?' +
            'snapshot\[12\]\$UDINT <> 8.*?detailCode:=31;.*?' +
            'snapshot\[16\]\$UDINT <> 0.*?detailCode:=31;.*?' +
            'snapshot\[376\]\$DINT = 0.*?detailCode:=31;.*?' +
            'snapshot\[380\]\$UDINT <> 8.*?detailCode:=31;.*?' +
            'snapshot\[388\]\$UDINT <> 0.*?detailCode:=31;.*?' +
            'snapshot\[392\]\$UDINT <> _ClassOk.*?detailCode:=31;.*?' +
            'snapshot\[452\]\$UINT <> 1.*?detailCode:=31;.*?' +
            'snapshot\[444\]\$UDINT <> LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK.*?' +
            'detailCode:=31;.*?' +
            'snapshot\[448\]\$UDINT <> 0.*?detailCode:=31;.*?' +
            'snapshot\[460\]\$UDINT <> requestExpectedOutputRevision.*?' +
            'detailCode:=29;') (
            'LMCDiagnosticsService 0x7E23 does not reject an incoherent, ' +
            'offline, non-OP, invalid-mask, or stale-revision output snapshot ' +
            'before queueing.')
        Assert-Match $outputWriteCaseBlock (
            '(?s)if detailCode = 0 then\s*' +
            'if \(NextTicketId = 0xFFFFFFFF\) \|\s*' +
            '\(NextOperationToken = 0xFFFFFFFF\) then\s*' +
            'detailCode:=24;\s*else\s*' +
            'NextTicketId \+= 1;\s*' +
            'NextOperationToken \+= 1;\s*' +
            'if \(NextTicketId = 0\) \|\s*' +
            '\(NextOperationToken = 0\) then\s*' +
            'detailCode:=24;\s*end_if;\s*end_if;\s*end_if;.*?' +
            'if detailCode = 0 then\s*' +
            'tryQueueResult := InputLatch\.TryQueueOutputWrite\(\s*' +
            'OperationToken:=NextOperationToken,\s*' +
            'TopologyRevision:=expectedMapRevision,\s*' +
            'DiagnosticsBootId:=requestDiagnosticsBootId,\s*' +
            'OwnerSessionEpoch:=CallerSessionEpoch,\s*' +
            'IOReference:=requestIOReference,\s*' +
            'ValueLow:=requestValueLow,\s*ValueHigh:=requestValueHigh,\s*' +
            'MaskLow:=requestMaskLow,\s*MaskHigh:=requestMaskHigh,\s*' +
            'ExpectedOutputRevision:=requestExpectedOutputRevision\);') (
            'LMCDiagnosticsService 0x7E23 does not allocate a fresh nonzero ' +
            'ticket/token or queue the exact retained identity after validation.')
        $outputDetailMutations = [regex]::Matches(
            $outputWriteCaseScanText,
            '\bdetailCode\s*(?::=|[+\-*/]=)')
        $canonicalOutputDetailMutations = [regex]::Matches(
            $outputWriteCaseScanText,
            ('\bdetailCode\s*:=\s*' +
             '(?:7|9|11|12|16|20|24|25|26|28|29|30|31);'))
        if ($outputDetailMutations.Count -ne
                $canonicalOutputDetailMutations.Count -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                '\bdetailCode\s*:=\s*0;').Count -ne 0 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                '\bcopyResult\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                ('copyResult\s*:=\s*' +
                 'InputLatch\.CopyTopologyIoSnapshot\(\s*' +
                 'pDest:=#snapshot\[0\],\s*DestSize:=464\);')).Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E23 detailCode/copyResult must stay ' +
                'sticky: no zero/dynamic/compound reset and exactly one coherent copy result.')
        }
        $outputStageSpecs = @(
            @{
                Name = 'reusable check'
                Pattern = 'InputLatch\.IsOutputReusable\(\)'
            },
            @{
                Name = 'coherent copy'
                Pattern = 'copyResult := InputLatch\.CopyTopologyIoSnapshot\('
            },
            @{
                Name = 'snapshot validation'
                Pattern = 'snapshot\[460\]\$UDINT <> requestExpectedOutputRevision'
            },
            @{
                Name = 'ticket/token allocation'
                Pattern = 'NextTicketId\s*\+=\s*1;'
            },
            @{
                Name = 'mailbox queue'
                Pattern = 'tryQueueResult := InputLatch\.TryQueueOutputWrite\('
            })
        $outputStageBlocks = @()
        foreach ($outputStageSpec in $outputStageSpecs) {
            $matchingOutputStageBlocks = @(Get-LasalStructuredIfBlocks `
                -Text $outputWriteCaseBlock `
                -ConditionPattern 'detailCode\s*=\s*0\s+then' | Where-Object {
                    (Get-LasalFirstThenArm $_) -match $outputStageSpec.Pattern
                })
            if ($matchingOutputStageBlocks.Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 ' + $outputStageSpec.Name +
                    ' must have one dedicated fresh zero-detail guard.')
            }
            $outputStageBlocks += $matchingOutputStageBlocks[0]
        }
        Assert-LasalExactIfGuard `
            -Text $outputStageBlocks[0] `
            -ConditionPattern (
                'InputLatch\.IsOutputReusable\(\)\s*=\s*FALSE') `
            -AssignmentPattern 'detailCode\s*:=\s*16\s*;' `
            -Owner 'LMCDiagnosticsService 0x7E23 reusable-mailbox guard'
        $outputCopyStageArm = Get-LasalFirstThenArm $outputStageBlocks[1]
        Assert-Match $outputCopyStageArm (
            '(?s)copyResult := InputLatch\.CopyTopologyIoSnapshot\(\s*' +
            'pDest:=#snapshot\[0\],\s*DestSize:=464\);\s*' +
            'if copyResult <> 0 then\s*detailCode:=31;\s*end_if;') (
            'LMCDiagnosticsService 0x7E23 copy/failure mapping must be adjacent ' +
            'inside its dedicated zero-detail guard.')
        $outputValidationStageArm = Get-LasalFirstThenArm $outputStageBlocks[2]
        $outputSnapshotFailureGuards = @(
            @{ Name = 'master OP'; Condition = 'snapshot\[12\]\$UDINT\s*<>\s*8'; Detail = 31 },
            @{ Name = 'master freshness'; Condition = 'snapshot\[16\]\$UDINT\s*<>\s*0'; Detail = 31 },
            @{ Name = 'output physical presence'; Condition = 'snapshot\[376\]\$DINT\s*=\s*0'; Detail = 31 },
            @{ Name = 'output EtherCAT OP'; Condition = 'snapshot\[380\]\$UDINT\s*<>\s*8'; Detail = 31 },
            @{ Name = 'output AL status'; Condition = 'snapshot\[388\]\$UDINT\s*<>\s*0'; Detail = 31 },
            @{ Name = 'output ClassState'; Condition = 'snapshot\[392\]\$UDINT\s*<>\s*_ClassOk'; Detail = 31 },
            @{ Name = 'output valid status'; Condition = 'snapshot\[452\]\$UINT\s*<>\s*1'; Detail = 31 },
            @{
                Name = 'output valid mask'
                Condition = ('snapshot\[444\]\$UDINT\s*<>\s*' +
                    'LMC_DIAG_ECAT_IO_OUTPUT_VALID_MASK')
                Detail = 31
            },
            @{ Name = 'output reserved high'; Condition = 'snapshot\[448\]\$UDINT\s*<>\s*0'; Detail = 31 },
            @{
                Name = 'expected output revision'
                Condition = ('snapshot\[460\]\$UDINT\s*<>\s*' +
                    'requestExpectedOutputRevision')
                Detail = 29
            })
        foreach ($outputSnapshotFailureGuard in $outputSnapshotFailureGuards) {
            Assert-LasalExactIfGuard `
                -Text $outputValidationStageArm `
                -ConditionPattern (
                    '\(detailCode\s*=\s*0\)\s*&\s*\(' +
                    $outputSnapshotFailureGuard.Condition + '\)') `
                -AssignmentPattern (
                    'detailCode\s*:=\s*' +
                    $outputSnapshotFailureGuard.Detail + '\s*;') `
                -Owner ('LMCDiagnosticsService 0x7E23 ' +
                    $outputSnapshotFailureGuard.Name + ' snapshot guard')
        }
        foreach ($outputValidationMarker in @(
                'snapshot\[12\]\$UDINT <> 8',
                'snapshot\[376\]\$DINT = 0',
                'snapshot\[452\]\$UINT <> 1',
                'snapshot\[460\]\$UDINT <> requestExpectedOutputRevision')) {
            if ([regex]::Matches(
                    $outputValidationStageArm,
                    $outputValidationMarker).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 snapshot validation marker ' +
                    "'$outputValidationMarker' must be inside its fresh guard.")
            }
        }
        $previousOutputStageEnd =
            $outputConnectionGuardMatch.Index + $outputConnectionGuardMatch.Length
        for ($outputStageIndex = 0;
             $outputStageIndex -lt $outputStageBlocks.Count;
             $outputStageIndex += 1) {
            $currentOutputStageIndex = $outputWriteCaseBlock.IndexOf(
                $outputStageBlocks[$outputStageIndex],
                [StringComparison]::Ordinal)
            if ($currentOutputStageIndex -le $previousOutputStageEnd) {
                throw ('LMCDiagnosticsService 0x7E23 stages must close and ' +
                    're-enter fresh zero-detail guards in reusable/copy/' +
                    'validation/allocation/queue order.')
            }
            $previousOutputStageEnd = $currentOutputStageIndex +
                $outputStageBlocks[$outputStageIndex].Length
        }
        $ticketPublishBlock = Get-UniqueLasalIfBlockContaining `
            -Text $outputWriteCaseBlock `
            -ConditionPattern 'tryQueueResult\s*=\s*READY\s+then' `
            -RequiredPattern '\(pResponse \+ 16\)\^\$UDINT\s*:=\s*TicketId;' `
            -Message ('LMCDiagnosticsService 0x7E23 accepted ticket identity ' +
                'and response publication must be dominated by READY.')
        $ticketPublishThenArm = Get-LasalFirstThenArm $ticketPublishBlock
        Assert-Match $ticketPublishThenArm (
            '(?s)if tryQueueResult = READY then\s*' +
            'TicketId := NextTicketId;.*?' +
            'OwnerSessionEpoch := CallerSessionEpoch;.*?' +
            'TicketBootId := requestDiagnosticsBootId;.*?' +
            'TicketMapRevision := expectedMapRevision;.*?' +
            'OperationToken := NextOperationToken;.*?' +
            'OperationKind := 4;.*?' +
            'OperationState := LMC_DIAG_SDO_STATE_QUEUED;.*?' +
            'SdoInternalDrainState := 0;.*?' +
            'OperationOutcome := 0;.*?' +
            'SdoTimeoutCycles := LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES;.*?' +
            'SdoSubmitCycle := snapshot\[0\]\$UDINT;.*?' +
            'SdoCompletionCycle := 0;.*?' +
            'SdoLastProcessedCycle := SdoSubmitCycle - 1;.*?' +
            'SdoOperationErrorId := 0;.*?' +
            'SdoOperationDetail := 0;.*?' +
            'SdoResultLength := 0;.*?' +
            'SdoResultData := 0;.*?' +
            '\(pResponse \+ 16\)\^\$UDINT := TicketId;.*?' +
            '\(pResponse \+ 20\)\^\$UINT := 4;.*?' +
            '\(pResponse \+ 22\)\^\$UINT := LMC_DIAG_SDO_STATE_QUEUED;.*?' +
            '\(pResponse \+ 24\)\^\$UDINT := SdoSubmitCycle;.*?' +
            '\(pResponse \+ 28\)\^\$UDINT := CurrentDiagnosticsBootId;.*?' +
            'ResponseSize:=32;') (
            'LMCDiagnosticsService 0x7E23 does not bind/reset/publish the exact ' +
            'ticket only inside the READY success branch.')
        Assert-Match $ticketPublishBlock (
            '(?s)ResponseSize:=32;\s*else\s*detailCode:=16;\s*end_if;') (
            'LMCDiagnosticsService 0x7E23 must map a non-READY queue result to busy.')
        foreach ($readyOnlyAssignment in @(
                'TicketId\s*:=\s*NextTicketId;',
                'OwnerSessionEpoch\s*:=\s*CallerSessionEpoch;',
                'TicketBootId\s*:=\s*requestDiagnosticsBootId;',
                'TicketMapRevision\s*:=\s*expectedMapRevision;',
                'OperationToken\s*:=\s*NextOperationToken;',
                'OperationKind\s*:=\s*4;',
                'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_QUEUED;',
                'SdoInternalDrainState\s*:=\s*0;',
                'OperationOutcome\s*:=\s*0;',
                'SdoTimeoutCycles\s*:=\s*LMC_DIAG_ECAT_IO_TIMEOUT_CYCLES;',
                'SdoSubmitCycle\s*:=\s*snapshot\[0\]\$UDINT;',
                'SdoCompletionCycle\s*:=\s*0;',
                'SdoLastProcessedCycle\s*:=\s*SdoSubmitCycle - 1;',
                'SdoOperationErrorId\s*:=\s*0;',
                'SdoOperationDetail\s*:=\s*0;',
                'SdoResultLength\s*:=\s*0;',
                'SdoResultData\s*:=\s*0;',
                '\(pResponse \+ 16\)\^\$UDINT\s*:=\s*TicketId;',
                '\(pResponse \+ 20\)\^\$UINT\s*:=\s*4;',
                '\(pResponse \+ 22\)\^\$UINT\s*:=\s*LMC_DIAG_SDO_STATE_QUEUED;',
                '\(pResponse \+ 24\)\^\$UDINT\s*:=\s*SdoSubmitCycle;',
                '\(pResponse \+ 28\)\^\$UDINT\s*:=\s*CurrentDiagnosticsBootId;',
                'ResponseSize\s*:=\s*32;')) {
            if ([regex]::Matches(
                    $outputWriteCaseBlock,
                    $readyOnlyAssignment).Count -ne 1 -or
                [regex]::Matches(
                    $ticketPublishThenArm,
                    $readyOnlyAssignment).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 accepted-ticket assignment ' +
                    "'$readyOnlyAssignment' must occur exactly once inside READY.")
            }
        }
        $ticketPublishThenArmScanText =
            Get-LasalScanText $ticketPublishThenArm
        foreach ($readyOnlySharedLocal in @(
                'TicketId',
                'OwnerSessionEpoch',
                'TicketBootId',
                'TicketMapRevision',
                'OperationToken',
                'OperationKind',
                'OperationState',
                'SdoInternalDrainState',
                'OperationOutcome',
                'SdoTimeoutCycles',
                'SdoSubmitCycle',
                'SdoCompletionCycle',
                'SdoLastProcessedCycle',
                'SdoOperationErrorId',
                'SdoOperationDetail',
                'SdoResultLength',
                'SdoResultData')) {
            if ([regex]::Matches(
                    $outputWriteCaseScanText,
                    '\b' + $readyOnlySharedLocal +
                        '\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                [regex]::Matches(
                    $ticketPublishThenArmScanText,
                    '\b' + $readyOnlySharedLocal +
                        '\s*(?::=|[+\-*/]=)').Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 shared ticket local ' +
                    $readyOnlySharedLocal +
                    ' may be mutated exactly once and only inside READY.')
            }
        }
        foreach ($readyOnlyResponseOffset in @(16, 20, 22, 24, 28)) {
            $readyOnlyResponseMutationPattern =
                '\(pResponse \+ ' + $readyOnlyResponseOffset +
                '\)\^\$[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)'
            if ([regex]::Matches(
                    $outputWriteCaseScanText,
                    $readyOnlyResponseMutationPattern).Count -ne 1 -or
                [regex]::Matches(
                    $ticketPublishThenArmScanText,
                    $readyOnlyResponseMutationPattern).Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 success response offset ' +
                    $readyOnlyResponseOffset +
                    ' may be written exactly once and only inside READY.')
            }
        }
        $ticketPublishBlockScanText = Get-LasalScanText $ticketPublishBlock
        if ([regex]::Matches(
                $ticketPublishBlockScanText,
                '\bResponseSize\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                '\bResponseSize\s*:=\s*32;').Count -ne 1 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                ('(?i)\(\s*pResponse\s*\+\s*[^\)]+\)\s*\^\s*\$\s*' +
                 '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')).Count -ne 5 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                ('(?i)(?:\bpResponse\b|' +
                 '\(\s*pResponse\s*\+\s*[^\)]+\))\s*\^\s*\$\s*' +
                 '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')).Count -ne 5 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                ('(?i)\bpResponse\b\s*\^\s*\$\s*' +
                 '[A-Za-z_][A-Za-z0-9_]*' +
                  '\s*(?::=|[+\-*/]=)')).Count -ne 0 -or
            $ticketPublishThenArmScanText -match
                '(?i)\bpResponse\b\s*(?::=|[+\-*/]=)' -or
            $ticketPublishThenArmScanText -match
                ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
                 '(?:dest|ptr1)\s*:=\s*' +
                 '(?:pResponse\b|\(\s*pResponse\s*\+\s*[^\)]+\))')) {
            throw ('LMCDiagnosticsService 0x7E23 READY branch must publish one ' +
                '32-byte response size and may not aggregate-overwrite pResponse.')
        }
        $outputQueueStageArm = Get-LasalFirstThenArm $outputStageBlocks[4]
        $outputQueueStageScanText = Get-LasalScanText $outputQueueStageArm
        if ([regex]::Matches(
                $outputQueueStageScanText,
                '\bdetailCode\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $ticketPublishBlockScanText,
                '\bdetailCode\s*:=\s*16;').Count -ne 1 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                '\bdetailCode\s*(?::=|[+\-*/]=)').Count -ne 0 -or
            $outputQueueStageScanText -match '(?i)\bRETURN\s*;') {
            throw ('LMCDiagnosticsService 0x7E23 queue stage may assign only ' +
                'the canonical busy detail in the non-READY ELSE; READY must ' +
                'publish ticket/response without detail mutation or early exit.')
        }
        $outputQueueStageIndex = $outputWriteCaseBlock.IndexOf(
            $outputStageBlocks[4],
            [StringComparison]::Ordinal)
        if ($outputQueueStageIndex -lt 0) {
            throw 'LMCDiagnosticsService 0x7E23 final queue-stage range was not found.'
        }
        $outputWriteCaseTail = $outputWriteCaseBlock.Substring(
            $outputQueueStageIndex + $outputStageBlocks[4].Length)
        if (-not [string]::IsNullOrWhiteSpace(
                (Get-LasalScanText $outputWriteCaseTail))) {
            throw ('LMCDiagnosticsService 0x7E23 final queue guard must be the ' +
                'last executable command-case stage; accepted ticket, detail, ' +
                'response, and mailbox state may not be changed afterward.')
        }
        $outputResponseMutationPattern =
            ('(?i)(?:\bpResponse\b|' +
             '\(\s*pResponse\s*\+\s*[^\)]+\))\s*\^\s*\$\s*' +
             '[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)')
        if ([regex]::Matches(
                $outputWriteCaseScanText,
                $outputResponseMutationPattern).Count -ne 5 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                '(?i)\bpResponse\b').Count -ne 5 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                $outputResponseMutationPattern).Count -ne 5 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                '\bResponseSize\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $ticketPublishThenArmScanText,
                '\bResponseSize\s*:=\s*32;').Count -ne 1 -or
            $outputWriteCaseScanText -match
                '(?i)\bpResponse\b\s*(?::=|[+\-*/]=)' -or
            $outputWriteCaseScanText -match
                ('(?is)\b(?:_memset|_memcpy)\s*\(\s*' +
                 '(?:dest|ptr1)\s*:=\s*' +
                 '(?:pResponse\b|\(\s*pResponse\s*\+\s*[^\)]+\))')) {
            throw ('LMCDiagnosticsService 0x7E23 response fields and size may ' +
                'be written only by the READY branch, with no aggregate or tail overwrite.')
        }
        foreach ($nextIdentityCounter in @('NextTicketId', 'NextOperationToken')) {
            if ([regex]::Matches(
                    $outputWriteCaseScanText,
                    '\b' + $nextIdentityCounter +
                        '\s*(?::=|[+\-*/]=)').Count -ne 1 -or
                [regex]::Matches(
                    $outputWriteCaseScanText,
                    '\b' + $nextIdentityCounter + '\s*\+=\s*1;').Count -ne 1) {
                throw ('LMCDiagnosticsService 0x7E23 ' + $nextIdentityCounter +
                    ' must advance exactly once before mailbox submission.')
            }
        }
        if ((Get-LasalScanText $ticketPublishThenArm) -match
            '(?i)\bRETURN\s*;') {
            throw ('LMCDiagnosticsService 0x7E23 READY ticket publication ' +
                'must not RETURN before all shared identity and response fields are bound.')
        }
        $outputWriteQueueCallMatch = [regex]::Match(
            $outputWriteCaseBlock,
            '(?s)tryQueueResult := InputLatch\.TryQueueOutputWrite\(\s*' +
                'OperationToken:=NextOperationToken,\s*' +
                'TopologyRevision:=expectedMapRevision,\s*' +
                'DiagnosticsBootId:=requestDiagnosticsBootId,\s*' +
                'OwnerSessionEpoch:=CallerSessionEpoch,\s*' +
                'IOReference:=requestIOReference,\s*' +
                'ValueLow:=requestValueLow,\s*ValueHigh:=requestValueHigh,\s*' +
                'MaskLow:=requestMaskLow,\s*MaskHigh:=requestMaskHigh,\s*' +
                'ExpectedOutputRevision:=requestExpectedOutputRevision\);')
        if ([regex]::Matches(
                $outputWriteCaseScanText,
                '\btryQueueResult\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $outputWriteCaseScanText,
                ('(?s)tryQueueResult := InputLatch\.TryQueueOutputWrite\(\s*' +
                 'OperationToken:=NextOperationToken,\s*' +
                 'TopologyRevision:=expectedMapRevision,\s*' +
                 'DiagnosticsBootId:=requestDiagnosticsBootId,\s*' +
                 'OwnerSessionEpoch:=CallerSessionEpoch,\s*' +
                 'IOReference:=requestIOReference,\s*' +
                 'ValueLow:=requestValueLow,\s*ValueHigh:=requestValueHigh,\s*' +
                 'MaskLow:=requestMaskLow,\s*MaskHigh:=requestMaskHigh,\s*' +
                 'ExpectedOutputRevision:=requestExpectedOutputRevision\);')).Count -ne 1) {
            throw ('LMCDiagnosticsService 0x7E23 tryQueueResult must be assigned ' +
                'exactly once from the canonical mailbox submission call.')
        }
        $ticketPublishIndex = $outputWriteCaseBlock.IndexOf(
            $ticketPublishBlock,
            [StringComparison]::Ordinal)
        if (-not $outputWriteQueueCallMatch.Success -or
            $ticketPublishIndex -lt 0 -or
            ($outputWriteQueueCallMatch.Index +
                $outputWriteQueueCallMatch.Length) -ge $ticketPublishIndex) {
            throw ('LMCDiagnosticsService 0x7E23 queue-to-ticket publication ' +
                'interval was not found.')
        }
        $queueToTicketPublishSection = $outputWriteCaseBlock.Substring(
            $outputWriteQueueCallMatch.Index +
                $outputWriteQueueCallMatch.Length,
            $ticketPublishIndex -
                ($outputWriteQueueCallMatch.Index +
                    $outputWriteQueueCallMatch.Length))
        if (-not [string]::IsNullOrWhiteSpace(
                (Get-LasalScanText $queueToTicketPublishSection))) {
            throw ('LMCDiagnosticsService 0x7E23 READY decision must immediately ' +
                'follow the mailbox call; no result/token/ticket mutation or ' +
                'early exit is allowed in between.')
        }
        $outputWriteCopyMatch = [regex]::Match(
            $outputWriteCaseBlock,
            'copyResult := InputLatch\.CopyTopologyIoSnapshot\(')
        $outputWritePublishMatch = [regex]::Match(
            $outputWriteCaseBlock,
            '\(pResponse \+ 16\)\^\$UDINT := TicketId;')
        if (-not $outputWriteCopyMatch.Success -or
            -not $outputWriteQueueCallMatch.Success -or
            -not $outputWritePublishMatch.Success -or
            $outputWriteCopyMatch.Index -ge $outputWriteQueueCallMatch.Index -or
            $outputWriteQueueCallMatch.Index -ge $outputWritePublishMatch.Index) {
            throw ('LMCDiagnosticsService 0x7E23 must copy and validate one ' +
                'snapshot before queueing, then publish a ticket only after READY.')
        }
    }

    Assert-Match $diagnosticsServiceHandleBlock (
        '(?s)ResponseSize\s*:=\s*-1;.*?' +
        '\(CommandId\s*=\s*0x7E00\)\s*&\s*' +
        '\(ResponseCapacity\s*<\s*68\).*?RETURN;.*?' +
        'schemaVersion\s*:=\s*pRequest\^\$UINT;.*?' +
        'requestFlags\s*:=\s*\(pRequest\s*\+\s*2\)\^\$UINT;.*?' +
        'requestId\s*:=\s*\(pRequest\s*\+\s*4\)\^\$UDINT;.*?' +
        '\(pRequest\s*=\s*NIL\)\s*\|\s*' +
        '\(RequestSize\s*<\s*8\)\s*\|\s*' +
        '\(Reference\s*<>\s*0\).*?detailCode\s*:=\s*12;.*?' +
        'schemaVersion\s*<>\s*LMC_DIAG_SCHEMA_VERSION.*?' +
        'requestFlags\s*<>\s*0.*?' +
        'requestId\s*=\s*0.*?detailCode\s*:=\s*12;') (
        'Phase4DiagnosticsRouted 0x7E00 common request decode, exact ' +
        'reference/schema/flags/nonzero-requestId gate, or 68-byte capacity guard is incomplete.')

    $diagnosticsServiceCapabilitiesBlock = [regex]::Match(
        $diagnosticsServiceHandleBlock,
        '(?s)if\s+CommandId\s*=\s*0x7E00\s+then.*?' +
        'ResponseSize\s*:=\s*68;\s*RETURN;\s*end_if;').Value
    if ([string]::IsNullOrWhiteSpace($diagnosticsServiceCapabilitiesBlock)) {
        throw 'Phase4DiagnosticsRouted LMCDiagnosticsService 0x7E00 case was not found.'
    }
    Assert-DiagnosticsCapabilityWriteInventory `
        -CapabilitiesBlock $diagnosticsServiceCapabilitiesBlock `
        -ExpectedBootZeroCapabilities $expectedTopologyIoBootZeroCapabilities `
        -ExpectedStableCapabilities $expectedTopologyIoStableCapabilities `
        -Owner ('Phase4DiagnosticsRouted 0x7E00 ' +
            "TopologyIoCheckpoint=$TopologyIoCheckpoint")

    $capabilityWriteCanonicalFixture = @"
(pResponse + 20)^`$UDINT := $expectedTopologyIoBootZeroCapabilities;
(pResponse + 20)^`$UDINT := $expectedTopologyIoStableCapabilities;
(pResponse + 20)^`$UDINT := (pResponse + 20)^`$UDINT | 0x00000200;
"@
    Assert-DiagnosticsCapabilityWriteInventory `
        -CapabilitiesBlock $capabilityWriteCanonicalFixture `
        -ExpectedBootZeroCapabilities $expectedTopologyIoBootZeroCapabilities `
        -ExpectedStableCapabilities $expectedTopologyIoStableCapabilities `
        -Owner '0x7E00 capability-write canonical fixture'
    foreach ($forbiddenCapabilityBit in @(
            '0x00008000',
            '0x00010000',
            '0x00020000')) {
        $capabilityWriteNegativeFixture =
            $capabilityWriteCanonicalFixture +
            "(pResponse + 20)^`$UDINT := (pResponse + 20)^`$UDINT | " +
            $forbiddenCapabilityBit + ';'
        $capabilityNegativeRejected = $false
        try {
            Assert-DiagnosticsCapabilityWriteInventory `
                -CapabilitiesBlock $capabilityWriteNegativeFixture `
                -ExpectedBootZeroCapabilities $expectedTopologyIoBootZeroCapabilities `
                -ExpectedStableCapabilities $expectedTopologyIoStableCapabilities `
                -Owner "0x7E00 forbidden $forbiddenCapabilityBit fixture"
        }
        catch {
            $capabilityNegativeRejected = $true
        }
        if (-not $capabilityNegativeRejected) {
            throw (
                '0x7E00 capability-write verifier accepted forbidden bit fixture ' +
                $forbiddenCapabilityBit + '.')
        }
    }
    Assert-Match $diagnosticsServiceCapabilitiesBlock (
        '(?s)if\s+RequestSize\s*<>\s*8\s+then\s*' +
        'detailCode\s*:=\s*12;\s*end_if;.*?' +
        'if\s+detailCode\s*<>\s*0\s+then\s*' +
        '\(pResponse\s*\+\s*4\)\^\$UINT\s*:=\s*1;\s*' +
        '\(pResponse\s*\+\s*6\)\^\$INT\s*:=\s*LMC_DIAG_ERROR_ID;\s*' +
        '\(pResponse\s*\+\s*12\)\^\$UDINT\s*:=\s*detailCode;\s*' +
        'end_if;.*?ResponseSize\s*:=\s*68;\s*RETURN;') (
        'Phase4DiagnosticsRouted 0x7E00 must return the fixed 68-byte ' +
        'capability payload even for malformed requests.')

    $capabilityOffsetContracts = [ordered]@{
        0 = 'pResponse\^\$UINT\s*:=\s*LMC_DIAG_SCHEMA_VERSION'
        2 = '\(pResponse\s*\+\s*2\)\^\$UINT\s*:=\s*0'
        4 = '\(pResponse\s*\+\s*4\)\^\$UINT\s*:=\s*0'
        6 = '\(pResponse\s*\+\s*6\)\^\$INT\s*:=\s*0'
        8 = '\(pResponse\s*\+\s*8\)\^\$UDINT\s*:=\s*requestId'
        12 = '\(pResponse\s*\+\s*12\)\^\$UDINT\s*:=\s*0'
        16 = '\(pResponse\s*\+\s*16\)\^\$UDINT\s*:=\s*1'
        20 = ('\(pResponse\s*\+\s*20\)\^\$UDINT\s*:=\s*' +
            $expectedTopologyIoBootZeroCapabilities)
        24 = '\(pResponse\s*\+\s*24\)\^\$UDINT\s*:=\s*LMC_DIAG_MAP_REVISION'
        28 = '\(pResponse\s*\+\s*28\)\^\$UINT\s*:=\s*24'
        30 = '\(pResponse\s*\+\s*30\)\^\$UINT\s*:=\s*0'
        32 = '\(pResponse\s*\+\s*32\)\^\$UINT\s*:=\s*0'
        34 = '\(pResponse\s*\+\s*34\)\^\$UINT\s*:=\s*0'
        36 = '\(pResponse\s*\+\s*36\)\^\$UDINT\s*:=\s*0'
        40 = '\(pResponse\s*\+\s*40\)\^\$UDINT\s*:=\s*1000'
        44 = '\(pResponse\s*\+\s*44\)\^\$UINT\s*:=\s*1320'
        46 = '\(pResponse\s*\+\s*46\)\^\$UINT\s*:=\s*2040'
        48 = '\(pResponse\s*\+\s*48\)\^\$UINT\s*:=\s*1280'
        50 = '\(pResponse\s*\+\s*50\)\^\$UINT\s*:=\s*80'
        52 = '\(pResponse\s*\+\s*52\)\^\$UINT\s*:=\s*16'
        54 = '\(pResponse\s*\+\s*54\)\^\$UINT\s*:=\s*0'
        56 = '\(pResponse\s*\+\s*56\)\^\$UDINT\s*:=\s*0'
        60 = '\(pResponse\s*\+\s*60\)\^\$UINT\s*:=\s*0'
        62 = '\(pResponse\s*\+\s*62\)\^\$UINT\s*:=\s*0'
        64 = '\(pResponse\s*\+\s*64\)\^\$UDINT\s*:=\s*currentBootId'
    }
    foreach ($capabilityOffset in $capabilityOffsetContracts.Keys) {
        $capabilityOffsetOwnerBlock = if ($capabilityOffset -lt 16) {
            $diagnosticsServiceHandleBlock
        }
        else {
            $diagnosticsServiceCapabilitiesBlock
        }
        Assert-Match $capabilityOffsetOwnerBlock `
            $capabilityOffsetContracts[$capabilityOffset] (
            'Phase4DiagnosticsRouted 0x7E00 response field at inner offset ' +
            "$capabilityOffset is missing or has the wrong value.")
    }
    Assert-Match $diagnosticsServiceCapabilitiesBlock (
        '(?s)if\s+currentBootId\s*<>\s*0\s+then\s*' +
        '\(pResponse\s*\+\s*20\)\^\$UDINT\s*:=\s*' +
        $expectedTopologyIoStableCapabilities + ';\s*' +
        '\(pResponse\s*\+\s*30\)\^\$UINT\s*:=\s*24;\s*' +
        '\(pResponse\s*\+\s*32\)\^\$UINT\s*:=\s*24;\s*' +
        '\(pResponse\s*\+\s*34\)\^\$UINT\s*:=\s*1;\s*' +
        '\(pResponse\s*\+\s*36\)\^\$UDINT\s*:=\s*320000;\s*' +
        '\(pResponse\s*\+\s*56\)\^\$UDINT\s*:=\s*1280000;\s*' +
        '\(pResponse\s*\+\s*60\)\^\$UINT\s*:=\s*4;') (
        'Phase4DiagnosticsRouted 0x7E00 stable-BootId capability bits for ' +
        "TopologyIoCheckpoint=$TopologyIoCheckpoint, stateful limits, " +
        'recorder bytes, or inline SDO limit is incomplete.')
    Assert-Match $diagnosticsServiceCapabilitiesBlock '(?s)\(LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE\)\s*&\s*\(\(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS1_ENABLED = TRUE\)\s*\|\s*\(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS2_ENABLED = TRUE\)\s*\|\s*\(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS3_ENABLED = TRUE\)\s*\|\s*\(LMC_DIAG_D5_SDO_WRITE_UI24_AXIS4_ENABLED = TRUE\)\).*?\(pResponse \+ 20\)\^\$UDINT :=\s*\(pResponse \+ 20\)\^\$UDINT \| 0x00000200' '0x7E00 must keep SDOWrite bit 9 behind the disabled global gate and at least one explicit per-axis gate.'
}
Assert-Match $diagnosticsServiceHandleBlock '(?s)currentBootId := GetDiagnosticsBootId\(\).*?\(CommandId >= 0x7E30\).*?\(CommandId <= 0x7E33\).*?\(CommandId >= 0x7E40\).*?\(CommandId <= 0x7E4D\).*?currentBootId = 0.*?detailCode := 11' 'LMCDiagnosticsService does not fail closed for raw stateful D2/D3/D4 calls when BootId is unavailable.'
Assert-Match $diagnosticsServiceHandleBlock '(?s)if \(CommandId >= 0x7E40\)\s*&\s*\(CommandId <= 0x7E4D\).*?IsClientConnected\(#RecorderStore\) = FALSE.*?\(pResponse \+ 4\)\^\$UINT := 1.*?\(pResponse \+ 12\)\^\$UDINT := 11.*?ResponseSize := 16.*?RETURN' 'LMCDiagnosticsService RecorderStore disconnected path is not fail-closed.'
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
Assert-Match $sdoStatusBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_COMPLETED.*?OperationOutcome = LMC_DIAG_SDO_OUTCOME_SUCCESS.*?OperationKind = 2.*?\(pResponse \+ 40\)\^\$UDINT := SdoResultLength.*?\(pResponse \+ 44\)\^\$USINT := SdoValueType.*?\(pResponse \+ 45\)\^\$USINT := SdoResultLength\$USINT.*?\(pResponse \+ 48\)\^\$UDINT := SdoResultData' 'GetOperationStatus 0x7E03 must expose typed data only for successful SDO Read; SDO Write result fields stay zero.'

Assert-Match $sdoCancelBlock '(?s)RequestSize <> 16.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?ResponseCapacity < 28.*?sdoTicketId <> TicketId.*?sdoBootId <> TicketBootId.*?CallerSessionEpoch <> OwnerSessionEpoch.*?OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?detailCode := 19.*?OperationState := LMC_DIAG_SDO_STATE_CANCELLED.*?OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED.*?ResponseSize := 28' 'CancelOperation 0x7E04 is not restricted to the owning queued ticket.'

if ($topologyIoOutputIntegrated) {
    Assert-LasalExactDeclaredType `
        -Text $diagnosticsServiceHandleBlock `
        -Name 'outputCancelResult' `
        -ExpectedType 'DINT' `
        -Owner 'LMCDiagnosticsService.HandleRequest outputCancelResult'
    foreach ($sharedKind4Handler in @(
            @{ Name = '0x7E03'; Block = $sdoStatusBlock },
            @{ Name = '0x7E04'; Block = $sdoCancelBlock })) {
        Assert-Match $sharedKind4Handler.Block (
            '(?s)\(LMC_DIAG_D5_SDO_READ_ENABLED = FALSE\) &\s*' +
            '\(LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE\) &\s*' +
            '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = FALSE\) then\s*' +
            'detailCode := 2;') (
            "LMCDiagnosticsService $($sharedKind4Handler.Name) must remain " +
            'available when EtherCAT output is the only enabled operation kind.')
    }
    Assert-Match $diagnosticsServiceHandleBlock (
        '(?s)\(\(CommandId = 0x7E03\) \| \(CommandId = 0x7E04\).*?' +
        '\(LMC_DIAG_D5_SDO_READ_ENABLED = TRUE\).*?' +
        '\(LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE\).*?' +
        '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = TRUE\).*?' +
        '\(currentBootId = 0\) then\s*detailCode := 11;') (
        'LMCDiagnosticsService shared status/cancel BootId gate must include ' +
        'future EtherCAT output-only activation.')
    Assert-Match $sdoStatusBlock (
        '(?s)OperationKind = 4.*?' +
        '\(pResponse \+ 40\)\^\$UDINT := 0.*?' +
        '\(pResponse \+ 48\)\^\$UDINT := 0') (
        'GetOperationStatus must expose DigitalOutputWrite kind 4 without ' +
        'fabricating typed SDO result data.')
    $kind4CancelBlock = Get-UniqueLasalIfBlockContaining `
        -Text $sdoCancelBlock `
        -ConditionPattern 'OperationKind\s*=\s*4\s+then' `
        -RequiredPattern 'InputLatch\.CancelQueuedOutput\(' `
        -Message ('CancelOperation kind 4 must be isolated from the generic ' +
            'queued SDO cancellation path.')
    Assert-Match $kind4CancelBlock (
        '(?s)if OperationKind = 4 then\s*' +
        'outputCancelResult := -2;\s*' +
        'if IsClientConnected\(#InputLatch\) then\s*' +
        'outputCancelResult := InputLatch\.CancelQueuedOutput\(\s*' +
        'ExpectedToken:=OperationToken\);\s*end_if;\s*' +
        'if outputCancelResult = 0 then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_CANCELLED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED;.*?' +
        'else\s*detailCode := 19;\s*end_if;\s*' +
        'else\s*.*?' +
        'OperationState := LMC_DIAG_SDO_STATE_CANCELLED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED;.*?' +
        'end_if;') (
        'CancelOperation must report kind 4 Cancelled only after exact-token ' +
        'mailbox CAS and keep generic queued SDO cancellation in the outer ELSE.')
    $kind4CancelSuccessBlock = Get-UniqueLasalIfBlockContaining `
        -Text $kind4CancelBlock `
        -ConditionPattern 'outputCancelResult\s*=\s*0\s+then' `
        -RequiredPattern 'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;' `
        -Message ('CancelOperation kind-4 terminal state must be dominated by ' +
            'successful exact-token mailbox cancellation.')
    if ([regex]::Matches(
            $kind4CancelBlock,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;').Count -ne 2 -or
        [regex]::Matches(
            $kind4CancelSuccessBlock,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;').Count -ne 1) {
        throw ('CancelOperation must contain one CAS-dominated kind-4 Cancelled ' +
            'assignment and one mutually exclusive generic SDO assignment.')
    }
    if ([regex]::Matches(
            $kind4CancelBlock,
            '\boutputCancelResult\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $kind4CancelBlock,
            'outputCancelResult\s*:=\s*-2;').Count -ne 1 -or
        [regex]::Matches(
            $kind4CancelBlock,
            'outputCancelResult\s*:=\s*InputLatch\.CancelQueuedOutput\(').Count -ne 1) {
        throw ('CancelOperation kind 4 must assign its DINT cancel result only ' +
            'through one pending default and one exact-token mailbox call.')
    }
    $kind4CancelThenArm = Get-LasalFirstThenArm $kind4CancelBlock
    $kind4CancelSuccessArm =
        Get-LasalFirstThenArm $kind4CancelSuccessBlock
    $kind4CancelThenScanText = Get-LasalScanText $kind4CancelThenArm
    $kind4CancelSuccessScanText = Get-LasalScanText $kind4CancelSuccessArm
    if ($kind4CancelThenScanText -match
            ('(?i)\b(?:TicketId|OperationToken|OperationKind|' +
             'OwnerSessionEpoch|TicketBootId|TicketMapRevision|' +
             'SdoSubmitCycle|SdoTimeoutCycles|SdoInternalDrainState)' +
             '\s*(?::=|[+\-*/]=)') -or
        [regex]::Matches(
            $kind4CancelThenScanText,
            '\bOperationState\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $kind4CancelThenScanText,
            '\bOperationOutcome\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $kind4CancelSuccessScanText,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;').Count -ne 1 -or
        [regex]::Matches(
            $kind4CancelSuccessScanText,
            'OperationOutcome\s*:=\s*LMC_DIAG_SDO_OUTCOME_CANCELLED;').Count -ne 1) {
        throw ('CancelOperation kind 4 must preserve all retained mailbox ' +
            'identity/drain state and publish its sole Cancelled state/outcome ' +
            'only inside successful exact-token cancellation.')
    }
    if ($kind4CancelThenScanText -match '(?i)\bRETURN\s*;' -or
        [regex]::Matches(
            $kind4CancelThenScanText,
            '\bdetailCode\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $kind4CancelThenScanText,
            '\bdetailCode\s*:=\s*19;').Count -ne 1) {
        throw ('CancelOperation kind 4 must not RETURN before publishing the ' +
            'CAS result, and CAS failure must retain exactly one nonzero detail 19.')
    }
    $cancelProtectedMutationPattern =
        ('(?i)\b(?:outputCancelResult|TicketId|OperationToken|OperationKind|' +
         'OwnerSessionEpoch|TicketBootId|TicketMapRevision|OperationState|' +
         'OperationOutcome|SdoSubmitCycle|SdoCompletionCycle|' +
         'SdoLastProcessedCycle|SdoTimeoutCycles|SdoOperationErrorId|' +
         'SdoOperationDetail|SdoResultLength|SdoResultData|' +
         'SdoInternalDrainState|SdoSlaveReference|SdoObjectIndex|' +
         'SdoSubIndex|SdoValueType|SdoRequestedLength|SdoWriteData)' +
         '\s*(?::=|[+\-*/]=)')
    $sdoCancelScanText = Get-LasalScanText $sdoCancelBlock
    $kind4CancelRangeStart = $sdoCancelBlock.IndexOf(
        $kind4CancelBlock,
        [StringComparison]::Ordinal)
    $kind4CancelRangeEnd =
        $kind4CancelRangeStart + $kind4CancelBlock.Length
    if ($kind4CancelRangeStart -lt 0) {
        throw 'CancelOperation kind-4 ownership range was not found.'
    }
    foreach ($cancelProtectedMutation in [regex]::Matches(
            $sdoCancelScanText,
            $cancelProtectedMutationPattern)) {
        if ($cancelProtectedMutation.Index -lt $kind4CancelRangeStart -or
            ($cancelProtectedMutation.Index +
                $cancelProtectedMutation.Length) -gt $kind4CancelRangeEnd) {
            throw ('CancelOperation operation/mailbox state may mutate only ' +
                'inside the mutually exclusive kind-4 versus generic-SDO branch.')
        }
    }
    $kind4CancelTailScanText = Get-LasalScanText (
        $sdoCancelBlock.Substring($kind4CancelRangeEnd))
    if ($kind4CancelTailScanText -match
            '(?i)\bdetailCode\s*(?::=|[+\-*/]=)|\bRETURN\s*;') {
        throw ('CancelOperation must not reset the kind-4 CAS failure detail or ' +
            'exit after the mutually exclusive cancellation branch.')
    }
}

if ([regex]::Matches($diagnosticsService, '(?m)^\s*TicketId\s*:\s*UDINT;').Count -ne 1 -or
    $diagnosticsService -match '(?m)^\s*TicketId\s*:\s*ARRAY') {
    throw 'LMCDiagnosticsService must own one global D5 ticket, not a ticket array.'
}
Assert-Match $sdoSubmitBlock '(?s)RequestSize < 32.*?expectedMapRevision := \(pRequest \+ 8\)\^\$UDINT.*?requestSdoSlaveReference := \(pRequest \+ 12\)\^\$UINT.*?sdoOperationFlags := \(pRequest \+ 14\)\^\$UINT.*?requestSdoObjectIndex := \(pRequest \+ 16\)\^\$UINT.*?requestSdoSubIndex := \(pRequest \+ 18\)\^\$USINT.*?requestSdoValueType := \(pRequest \+ 19\)\^\$USINT.*?requestSdoTimeoutCycles := \(pRequest \+ 20\)\^\$UDINT.*?sdoDataLength := \(pRequest \+ 24\)\^\$UINT.*?sdoReserved := \(pRequest \+ 26\)\^\$UINT.*?sdoBootId := \(pRequest \+ 28\)\^\$UDINT.*?expectedRequestSize := 32.*?sdoOperationFlags = 1.*?expectedRequestSize \+= TO_UDINT\(sdoDataLength\).*?RequestSize <> expectedRequestSize' 'SubmitSDO 0x7E50 generic request envelope validation is incomplete.'
Assert-Match $sdoSubmitBlock '(?s)sdoOperationFlags = 0.*?LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?requestSdoSlaveReference < 1.*?requestSdoSlaveReference > 4.*?requestSdoTimeoutCycles < 1.*?requestSdoTimeoutCycles > 60000.*?expectedMapRevision <> LMC_DIAG_MAP_REVISION.*?sdoBootId <> currentBootId.*?sdoOperationFlags = 0.*?sdoDataLength > 4.*?requestSdoObjectIndex = 0' 'SubmitSDO 0x7E50 does not enforce the gated read axes 1..4, timeout, identity, inline capacity, and nonzero object-index policy.'
Assert-Match $sdoSubmitBlock '(?s)sdoDataLength <> 1.*?sdoDataLength <> 2.*?sdoDataLength <> 4.*?requestSdoValueType < 1.*?requestSdoValueType > 11' 'SubmitSDO 0x7E50 does not bound general Read lengths and SDO ValueType codes.'
Assert-Match $sdoSubmitBlock '(?s)requestSdoValueType = 1.*?requestSdoValueType = 9.*?requestSdoValueType = 10.*?requestSdoValueType = 11.*?sdoDataLength <> 1.*?requestSdoValueType = 2.*?requestSdoValueType = 3.*?requestSdoValueType = 7.*?sdoDataLength <> 2.*?requestSdoValueType = 4.*?requestSdoValueType = 5.*?requestSdoValueType = 6.*?requestSdoValueType = 8.*?sdoDataLength <> 4' 'SubmitSDO 0x7E50 does not enforce exact 8/16/32-bit ValueType-to-length mapping.'
Assert-Match $sdoSubmitBlock '(?s)sdoOperationFlags = 1.*?sdoDataLength = 4.*?RequestSize = 36.*?requestSdoWriteData := \(pRequest \+ 32\)\^\$UDINT.*?GetSdoWritePolicyDetail\(.*?FALSE, NIL, 0\).*?sdoOperationFlags = 1.*?requestSdoValueType <> 4.*?sdoDataLength <> 4.*?detailCode := 5.*?writePolicyDetail <> 0.*?detailCode := writePolicyDetail' 'SubmitSDO 0x7E50 does not copy and centrally validate the exact Int32/four-byte write payload.'
Assert-Match $sdoSubmitBlock '(?s)OperationKind := 2;.*?sdoOperationFlags = 1.*?OperationKind := 3;.*?SdoSlaveReference := requestSdoSlaveReference;.*?SdoObjectIndex := requestSdoObjectIndex;.*?SdoSubIndex := requestSdoSubIndex;.*?SdoValueType := requestSdoValueType;.*?SdoRequestedLength := sdoDataLength;.*?SdoTimeoutCycles := requestSdoTimeoutCycles;.*?SdoWriteData := requestSdoWriteData;' 'SubmitSDO 0x7E50 does not retain read/write kind and all parsed ticket values.'
Assert-Match $sdoSubmitBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_QUEUED.*?OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?SdoInternalDrainState <> 0.*?detailCode := 9.*?case requestSdoSlaveReference of.*?SdoAxis1\.IsReusable\(\).*?SdoAxis4\.IsReusable\(\).*?NextTicketId = 0xFFFFFFFF.*?NextOperationToken = 0xFFFFFFFF.*?NextTicketId \+= 1.*?NextOperationToken \+= 1.*?TicketId := NextTicketId.*?OperationToken := NextOperationToken.*?OperationState := LMC_DIAG_SDO_STATE_QUEUED.*?SdoInternalDrainState := 0.*?ResponseSize := 32' 'SubmitSDO 0x7E50 does not allocate exactly one reusable queued ticket with wrap and drain guards.'
Assert-Match $sdoSubmitBlock '(?s)executorConnected = FALSE.*?detailCode := 11.*?executorReusable = FALSE.*?detailCode := 24' 'SubmitSDO 0x7E50 does not distinguish a disconnected executor from an unowned non-Idle invariant fault.'
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
Assert-Match $sdoProcessBlock '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE.*?RETURN.*?TicketId = 0.*?SdoInternalDrainState = 0' 'ProcessOperations does not remain inert when both D5 SDO compile gates and the ticket slot are inactive.'
Assert-Match $sdoProcessBlock '(?s)SdoInternalDrainState <> 0.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) = FALSE.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?IsSdoReadReady\(SlaveReference:=SdoSlaveReference\) then.*?SdoInternalDrainState := 0.*?RETURN' 'ProcessOperations does not drain late timeout/disconnect callbacks before releasing the executor.'
Assert-Match $sdoProcessBlock '(?s)OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?CopyCompletion\(\s*ExpectedToken:=OperationToken.*?elapsedCycles := currentCycle - SdoSubmitCycle.*?if \(completionResult = 0\)\s*&\s*\(elapsedCycles > SdoTimeoutCycles\) then.*?elsif completionResult = 0 then.*?OperationState := LMC_DIAG_SDO_STATE_COMPLETED.*?RETURN.*?elapsedCycles >= SdoTimeoutCycles.*?MarkOrphan\(\s*ExpectedToken:=OperationToken\).*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED.*?SdoInternalDrainState := LMC_DIAG_SDO_DRAIN_EXPIRED' 'ProcessOperations must consume a completion at the deadline before timeout and quarantine an incomplete timed-out adapter for late-callback drain.'
Assert-Match $sdoProcessBlock '(?s)completion\.ValidationCode = 7.*?SdoOperationDetail := 5.*?else\s*SdoOperationDetail := 24.*?completion\.OsResult <> 0.*?completion\.AbortCode = 0x08000000.*?SdoOperationDetail := completion\.OsResult\$UDINT.*?elsif completion\.AbortCode <> 0.*?elsif completion\.ActualLength <> TO_UDINT\(SdoRequestedLength\) then.*?SdoOperationDetail := 5.*?completion\.ObjectIndex <> SdoObjectIndex.*?SdoOperationDetail := 24' 'ProcessOperations does not preserve the general-read validation, OS/abort priority, exact length, and metadata error mapping.'
Assert-Match $sdoProcessBlock '(?s)completion\.ObjectIndex <> SdoObjectIndex.*?OperationKind = 2.*?completion\.IsWrite <> 0.*?OperationKind = 3.*?completion\.IsWrite = 0.*?OperationKind = 2 then.*?SdoResultLength := completion\.ActualLength.*?else\s*SdoResultLength := 0;\s*SdoResultData := 0;' 'ProcessOperations does not validate completion direction and keep successful SDO Write result fields zero.'
Assert-Match $sdoProcessBlock '(?s)OperationState <> LMC_DIAG_SDO_STATE_QUEUED.*?OperationState <> LMC_DIAG_SDO_STATE_RUNNING.*?currentCycle = SdoLastProcessedCycle.*?OperationKind = 3.*?GetSdoWritePolicyDetail\(.*?TRUE, #snapshot\[0\].*?writePolicyDetail <> 0.*?SdoOperationDetail := writePolicyDetail.*?remainingCycles := SdoTimeoutCycles - elapsedCycles.*?case SdoSlaveReference of.*?SdoAxis1\.TryStartWrite\(.*?pWriteData:=\(#SdoWriteData\)\$\^USINT.*?SdoAxis1\.TryStartRead\(.*?ReadLength:=SdoRequestedLength.*?SdoAxis4\.TryStartWrite\(.*?pWriteData:=\(#SdoWriteData\)\$\^USINT.*?SdoAxis4\.TryStartRead\(.*?ReadLength:=SdoRequestedLength.*?startResult = READY.*?OperationState := LMC_DIAG_SDO_STATE_RUNNING' 'ProcessOperations does not recheck write policy/state and dispatch exact queued read/write operations through the selected executor.'
Assert-Match $sdoProcessBlock '(?s)if copyResult <> 0 then.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;.*?if OperationState = LMC_DIAG_SDO_STATE_RUNNING then.*?if executorConnected = FALSE then.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;.*?if \(completionResult = 0\).*?elapsedCycles > SdoTimeoutCycles.*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED;.*?SdoWriteData := 0;.*?elsif completionResult = 0 then.*?SdoWriteData := 0;.*?elsif \(completionResult <> -2\).*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;' 'ProcessOperations does not clear retained SDO Write data on snapshot, disconnect, completion, late-completion, and completion-contract terminal paths.'
Assert-Match $sdoProcessBlock '(?s)if elapsedCycles >= SdoTimeoutCycles then.*?orphanResult = 0.*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED;.*?SdoWriteData := 0;.*?orphanResult = -1.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;.*?if elapsedCycles >= SdoTimeoutCycles then.*?OperationState := LMC_DIAG_SDO_STATE_EXPIRED;.*?SdoWriteData := 0;.*?writePolicyDetail <> 0.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;.*?if executorConnected = FALSE then.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;.*?elsif startResult = READY then.*?OperationKind = 3.*?SdoWriteData := 0;.*?elsif startResult = ERROR then.*?OperationState := LMC_DIAG_SDO_STATE_FAILED;.*?SdoWriteData := 0;' 'ProcessOperations does not clear retained SDO Write data on running timeout, queued timeout, policy rejection, dispatch success, and dispatch error terminal paths.'
Assert-Match $diagnosticsService '(?s)SdoWriteData\s*:\s*UDINT;.*?FUNCTION GLOBAL LMCDiagnosticsService::NotifySessionClosed.*?OperationState = LMC_DIAG_SDO_STATE_RUNNING.*?SdoWriteData := 0;.*?OperationState = LMC_DIAG_SDO_STATE_QUEUED.*?SdoWriteData := 0;.*?FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest.*?OperationState := LMC_DIAG_SDO_STATE_CANCELLED;.*?SdoWriteData := 0;.*?FUNCTION LMCDiagnosticsService::LMCDiagnosticsService.*?SdoWriteData := 0;' 'LMCDiagnosticsService does not clear retained write data on running/queued session cleanup, cancel, and construction.'

if ($topologyIoOutputIntegrated) {
    Assert-Match $sdoProcessBlock (
        '(?s)topologySnapshot\s*:\s*ARRAY \[0\.\.463\] OF USINT;.*?' +
        'outputCompletion\s*:\s*ARRAY \[0\.\.31\] OF USINT;.*?' +
        'outputSnapshotCopyResult\s*:\s*DINT;.*?' +
        'outputCopyResult\s*:\s*DINT;.*?' +
        'outputTimeoutCancelResult\s*:\s*DINT;.*?' +
        'outputCompletionResult\s*:\s*DINT;.*?' +
        'outputCompletionDetail\s*:\s*UDINT;.*?' +
        'outputCompletionCycle\s*:\s*UDINT;.*?' +
        'outputCompletionElapsedCycles\s*:\s*UDINT;') (
        'ProcessOperations kind-4 snapshot/completion locals are incomplete.')
    $processOutputLocalTypes = [ordered]@{
        'topologySnapshot' = 'ARRAY [0..463] OF USINT'
        'outputCompletion' = 'ARRAY [0..31] OF USINT'
        'outputSnapshotCopyResult' = 'DINT'
        'outputCopyResult' = 'DINT'
        'outputTimeoutCancelResult' = 'DINT'
        'outputCompletionResult' = 'DINT'
        'outputCompletionDetail' = 'UDINT'
        'outputCompletionCycle' = 'UDINT'
        'outputCompletionElapsedCycles' = 'UDINT'
        'currentCycle' = 'UDINT'
        'elapsedCycles' = 'UDINT'
    }
    foreach ($processOutputLocal in $processOutputLocalTypes.GetEnumerator()) {
        Assert-LasalExactDeclaredType `
            -Text $sdoProcessBlock `
            -Name $processOutputLocal.Key `
            -ExpectedType $processOutputLocal.Value `
            -Owner ('LMCDiagnosticsService.ProcessOperations local ' +
                $processOutputLocal.Key)
    }
    Assert-Match $sdoProcessBlock (
        '(?s)LMC_DIAG_D5_SDO_READ_ENABLED = FALSE.*?' +
        'LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE.*?' +
        'LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = FALSE.*?RETURN') (
        'ProcessOperations inert gate must include the dormant EtherCAT output ' +
        'feature so future activation cannot strand kind-4 tickets.')
    $outputOperationIfBlock = Get-UniqueLasalIfBlockContaining `
        -Text $sdoProcessBlock `
        -ConditionPattern 'OperationKind\s*=\s*4\s+then' `
        -RequiredPattern 'InputLatch\.CopyOutputCompletion\(' `
        -Message ('ProcessOperations kind-4 lifecycle must be isolated in one ' +
            'top-priority structured branch.')
    $outputOperationBlock = Get-LasalFirstThenArm $outputOperationIfBlock
    Assert-Match $outputOperationBlock (
        '(?s)outputSnapshotCopyResult := -1;\s*' +
        'if IsClientConnected\(#InputLatch\) then\s*' +
        'outputSnapshotCopyResult := InputLatch\.CopyTopologyIoSnapshot\(\s*' +
        'pDest:=#topologySnapshot\[0\],\s*DestSize:=464\);\s*end_if;\s*' +
        'if outputSnapshotCopyResult <> 0 then\s*RETURN;\s*end_if;\s*' +
        'currentCycle := topologySnapshot\[0\]\$UDINT;.*?' +
        'outputCopyResult := InputLatch\.CopyOutputCompletion\(\s*' +
        'ExpectedToken:=OperationToken,\s*' +
        'pDest:=#outputCompletion\[0\],\s*' +
        'DestSize:=32\);.*?' +
        'elapsedCycles := currentCycle - SdoSubmitCycle;') (
        'ProcessOperations kind-4 path must acquire a coherent 464-byte cycle ' +
        'snapshot fail-closed before completion handling.')
    if ([regex]::Matches(
            $outputOperationBlock,
            'InputLatch\.CopyOutputCompletion\(').Count -ne 1 -or
        [regex]::Matches(
            $outputOperationBlock,
            'outputCopyResult\s*:=\s*InputLatch\.CopyOutputCompletion\(').Count -ne 1 -or
        [regex]::Matches(
            $sdoProcessBlock,
            'InputLatch\.CopyOutputCompletion\(').Count -ne 1) {
        throw ('ProcessOperations kind 4 must call and assign the exact-token ' +
            '32-byte CopyOutputCompletion consumer exactly once.')
    }
    $outputOperationScanText = Get-LasalScanText $outputOperationBlock
    if ([regex]::Matches(
            $outputOperationScanText,
            'pDest:=#outputCompletion\[0\]').Count -ne 1 -or
        [regex]::Matches(
            $outputOperationScanText,
            'pDest:=#topologySnapshot\[0\]').Count -ne 1 -or
        $outputOperationScanText -match
            ('(?is)(?:(?:outputCompletion|topologySnapshot)\[[^\]]+\]' +
             '\$[A-Za-z_][A-Za-z0-9_]*\s*(?::=|[+\-*/]=)|' +
             '(?:_memset\(\s*dest|_memcpy\(\s*ptr1)\s*:=' +
             '#(?:outputCompletion|topologySnapshot)\[|pDest\s*:=' +
             '#(?:outputCompletion|topologySnapshot)\[(?!0\])))')) {
        throw ('ProcessOperations kind 4 must treat topology/completion buffers ' +
            'as immutable outputs of their one exact copy call; direct or ' +
            'aggregate overwrite is forbidden.')
    }
    $singleOutputLocalAssignments = @(
        @{
            Name = 'outputCopyResult'
            Pattern = (
                'outputCopyResult\s*:=\s*InputLatch\.CopyOutputCompletion\(\s*' +
                'ExpectedToken:=OperationToken,\s*' +
                'pDest:=#outputCompletion\[0\],\s*DestSize:=32\);')
        },
        @{
            Name = 'currentCycle'
            Pattern = 'currentCycle\s*:=\s*topologySnapshot\[0\]\$UDINT;'
        },
        @{
            Name = 'elapsedCycles'
            Pattern = 'elapsedCycles\s*:=\s*currentCycle - SdoSubmitCycle;'
        },
        @{
            Name = 'outputCompletionCycle'
            Pattern = ('outputCompletionCycle\s*:=\s*' +
                'outputCompletion\[12\]\$UDINT;')
        },
        @{
            Name = 'outputCompletionElapsedCycles'
            Pattern = ('outputCompletionElapsedCycles\s*:=\s*' +
                'outputCompletionCycle - SdoSubmitCycle;')
        },
        @{
            Name = 'outputCompletionResult'
            Pattern = ('outputCompletionResult\s*:=\s*' +
                'outputCompletion\[4\]\$DINT;')
        },
        @{
            Name = 'outputCompletionDetail'
            Pattern = ('outputCompletionDetail\s*:=\s*' +
                'outputCompletion\[8\]\$UDINT;')
        })
    foreach ($singleOutputLocalAssignment in $singleOutputLocalAssignments) {
        if ([regex]::Matches(
                $outputOperationScanText,
                '\b' + $singleOutputLocalAssignment.Name +
                    '\s*(?::=|[+\-*/]=)').Count -ne 1 -or
            [regex]::Matches(
                $outputOperationScanText,
                $singleOutputLocalAssignment.Pattern).Count -ne 1) {
            throw ('ProcessOperations kind 4 local ' +
                $singleOutputLocalAssignment.Name +
                ' must have exactly one canonical assignment.')
        }
    }
    if ([regex]::Matches(
            $outputOperationScanText,
            '\boutputSnapshotCopyResult\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $outputOperationScanText,
            'outputSnapshotCopyResult\s*:=\s*-1;').Count -ne 1 -or
        [regex]::Matches(
            $outputOperationScanText,
            ('outputSnapshotCopyResult\s*:=\s*' +
             'InputLatch\.CopyTopologyIoSnapshot\(')).Count -ne 1) {
        throw ('ProcessOperations kind 4 snapshot result must have exactly one ' +
            'failure default and one coherent-copy assignment.')
    }
    if ($outputOperationBlock -match
        'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_RUNNING;') {
        throw ('ProcessOperations must keep kind 4 publicly Queued until exact ' +
            'completion is consumed; only mailbox CAS owns the unobservable ' +
            'READY/RUNNING transition so cancel remains safe.')
    }
    $genericSnapshotCopyMatch = [regex]::Match(
        $sdoProcessBlock,
        'copyResult\s*:=\s*-1;')
    $outputOperationIndex = $sdoProcessBlock.IndexOf(
        $outputOperationBlock,
        [StringComparison]::Ordinal)
    $preOutputGuardSpecs = @(
        @{
            Name = 'inert feature gate'
            Condition = (
                '\(LMC_DIAG_D5_SDO_READ_ENABLED = FALSE\)[\s\S]*?' +
                '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = FALSE\)\s+then')
        },
        @{
            Name = 'empty ticket gate'
            Condition = (
                '\(TicketId = 0\) & \(SdoInternalDrainState = 0\)\s+then')
        },
        @{
            Name = 'late-callback drain gate'
            Condition = 'SdoInternalDrainState <> 0\s+then'
        },
        @{
            Name = 'active state gate'
            Condition = (
                '\(OperationState <> LMC_DIAG_SDO_STATE_QUEUED\) &\s*' +
                '\(OperationState <> LMC_DIAG_SDO_STATE_RUNNING\)\s+then')
        })
    foreach ($preOutputGuardSpec in $preOutputGuardSpecs) {
        $preOutputGuardBlock = Get-UniqueLasalIfBlockContaining `
            -Text $sdoProcessBlock `
            -ConditionPattern $preOutputGuardSpec.Condition `
            -RequiredPattern 'RETURN;' `
            -Message ('ProcessOperations ' + $preOutputGuardSpec.Name +
                ' was not found as one structured guard.')
        $preOutputGuardThenArm = Get-LasalFirstThenArm $preOutputGuardBlock
        if ([regex]::Matches(
                (Get-LasalScanText $preOutputGuardThenArm),
                '(?i)\bRETURN\s*;').Count -ne 1) {
            throw ('ProcessOperations ' + $preOutputGuardSpec.Name +
                ' must RETURN exactly once from its true branch.')
        }
        $preOutputGuardIndex = $sdoProcessBlock.IndexOf(
            $preOutputGuardBlock,
            [StringComparison]::Ordinal)
        if ($preOutputGuardIndex -lt 0 -or
            ($preOutputGuardIndex + $preOutputGuardBlock.Length) -ge
                $outputOperationIndex) {
            throw ('ProcessOperations ' + $preOutputGuardSpec.Name +
                ' must finish before the kind-4 lifecycle branch.')
        }
    }
    if (-not $genericSnapshotCopyMatch.Success -or
        $outputOperationIndex -lt 0 -or
        ($outputOperationIndex + $outputOperationBlock.Length) -ge
            $genericSnapshotCopyMatch.Index) {
        throw ('ProcessOperations must finish and RETURN from kind 4 before ' +
            'the generic SDO snapshot/running/queued paths can execute.')
    }
    $outputCompletionConsumedBlock = Get-UniqueLasalIfBlockContaining `
        -Text $outputOperationBlock `
        -ConditionPattern 'outputCopyResult\s*=\s*0\s+then' `
        -RequiredPattern 'outputCompletion\[4\]\$DINT' `
        -Message ('ProcessOperations may read a kind-4 completion payload only ' +
            'inside the successful exact-token CopyOutputCompletion branch.')
    $outputCompletionSuccessArm =
        Get-LasalFirstThenArm $outputCompletionConsumedBlock
    Assert-Match $outputCompletionSuccessArm (
        '(?s)if outputCopyResult = 0 then\s*' +
        'outputCompletionCycle := outputCompletion\[12\]\$UDINT;\s*' +
        'outputCompletionElapsedCycles := outputCompletionCycle - SdoSubmitCycle;\s*' +
        'outputCompletionResult := outputCompletion\[4\]\$DINT;\s*' +
        'outputCompletionDetail := outputCompletion\[8\]\$UDINT;\s*' +
        'SdoCompletionCycle := outputCompletionCycle;\s*' +
        'SdoResultLength := 0;\s*SdoResultData := 0;\s*' +
        'if outputCompletionElapsedCycles > SdoTimeoutCycles then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_EXPIRED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_TIMED_OUT;\s*' +
        'SdoOperationErrorId := 0;\s*' +
        'SdoOperationDetail := LMC_DIAG_SDO_ABORT_TIMEOUT;\s*' +
        'else\s*' +
        'if outputCompletionResult = 0 then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_COMPLETED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_SUCCESS;\s*' +
        'SdoOperationErrorId := 0;\s*' +
        'SdoOperationDetail := 0;\s*' +
        'else\s*OperationState := LMC_DIAG_SDO_STATE_FAILED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_FAILED;\s*' +
        'SdoOperationErrorId := LMC_DIAG_SDO_LOCAL_ERROR_ID;\s*' +
        'SdoOperationDetail := outputCompletionDetail;.*?' +
        'if SdoOperationDetail = 0 then\s*' +
        'SdoOperationDetail := 24;.*?end_if;\s*end_if;\s*end_if;') (
        'ProcessOperations consumed kind-4 completion must use the RT-applied ' +
        'cycle for deadline classification and map only payload Result=0 to success.')
    if ($outputCompletionSuccessArm -match
            'if\s+elapsedCycles\s*[><=].*?SdoTimeoutCycles' -or
        $outputCompletionSuccessArm -match
            'SdoCompletionCycle\s*:=\s*currentCycle;') {
        throw ('ProcessOperations must not classify a consumed kind-4 completion ' +
            'from the stale service snapshot cycle; payload AppliedCycle is authoritative.')
    }
    foreach ($completionPayloadRead in @(
            'outputCompletion\[4\]\$DINT',
            'outputCompletion\[8\]\$UDINT',
            'outputCompletion\[12\]\$UDINT')) {
        if ([regex]::Matches(
                $outputOperationBlock,
                $completionPayloadRead).Count -ne 1 -or
            [regex]::Matches(
                $outputCompletionSuccessArm,
                $completionPayloadRead).Count -ne 1) {
            throw ('ProcessOperations kind-4 completion payload field ' +
                "'$completionPayloadRead' must be read exactly once only after " +
                'a successful exact-token copy.')
        }
    }
    Assert-Match $outputOperationBlock (
        '(?s)if outputCopyResult = 0 then.*?' +
        'elsif outputCopyResult = -2 then\s*' +
        'if elapsedCycles >= SdoTimeoutCycles then\s*' +
        'outputTimeoutCancelResult := InputLatch\.CancelQueuedOutput\(\s*' +
        'ExpectedToken:=OperationToken\);\s*' +
        'if outputTimeoutCancelResult = 0 then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_EXPIRED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_TIMED_OUT;\s*' +
        'SdoCompletionCycle := currentCycle;\s*' +
        'SdoOperationErrorId := 0;\s*' +
        'SdoOperationDetail := LMC_DIAG_SDO_ABORT_TIMEOUT;\s*' +
        'SdoResultLength := 0;\s*SdoResultData := 0;\s*' +
        'end_if;\s*end_if;\s*' +
        'else\s*' +
        'outputTimeoutCancelResult := InputLatch\.CancelQueuedOutput\(\s*' +
        'ExpectedToken:=OperationToken\);\s*' +
        'if outputTimeoutCancelResult = 0 then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_FAILED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_FAILED;\s*' +
        'SdoCompletionCycle := currentCycle;\s*' +
        'SdoOperationErrorId := LMC_DIAG_SDO_LOCAL_ERROR_ID;\s*' +
        'SdoOperationDetail := 24;\s*' +
        'SdoResultLength := 0;\s*SdoResultData := 0;\s*' +
        'end_if;\s*end_if;\s*RETURN;\s*' +
        'end_if;') (
        'ProcessOperations must inspect CopyOutputCompletion before reading ' +
        'the payload, let a deadline completion win, and publish a terminal ' +
        'pending/error result only after exact-token queued cancellation; a ' +
        'CAS-lost RT claim remains running until completion.')
    if ([regex]::Matches(
            $outputOperationBlock,
            'InputLatch\.CancelQueuedOutput\(\s*' +
            'ExpectedToken:=OperationToken\)').Count -ne 2) {
        throw ('ProcessOperations kind 4 must use exact-token queued cancellation ' +
            'for both pending timeout and unexpected completion-copy failure.')
    }
    if ([regex]::Matches(
            $outputOperationScanText,
            '\boutputTimeoutCancelResult\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $outputOperationScanText,
            ('outputTimeoutCancelResult\s*:=\s*' +
             'InputLatch\.CancelQueuedOutput\(\s*' +
             'ExpectedToken:=OperationToken\);')).Count -ne 2) {
        throw ('ProcessOperations kind 4 must not overwrite either exact-token ' +
            'cancel result before terminal-state decisions.')
    }
    $outputCancelSuccessBlocks = @(Get-LasalStructuredIfBlocks `
        -Text $outputOperationBlock `
        -ConditionPattern 'outputTimeoutCancelResult\s*=\s*0\s+then')
    $outputCancelSuccessArms = @($outputCancelSuccessBlocks | ForEach-Object {
            Get-LasalFirstThenArm $_
        })
    $expiredCancelBlocks = @($outputCancelSuccessArms | Where-Object {
            $_ -match 'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_EXPIRED;'
        })
    $failedCancelBlocks = @($outputCancelSuccessArms | Where-Object {
            $_ -match 'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_FAILED;'
        })
    if ($outputCancelSuccessBlocks.Count -ne 2 -or
        $expiredCancelBlocks.Count -ne 1 -or
        $failedCancelBlocks.Count -ne 1) {
        throw ('ProcessOperations kind-4 pending-timeout and unexpected-error ' +
            'terminal states must each be dominated by successful exact-token ' +
            'queued cancellation.')
    }
    $allowedOutputTerminalBlocks = @($outputCompletionSuccessArm) +
        @($outputCancelSuccessArms)
    $allowedOutputTerminalRanges = @()
    foreach ($allowedOutputTerminalBlock in $allowedOutputTerminalBlocks) {
        $allowedOutputTerminalIndex = $outputOperationBlock.IndexOf(
            $allowedOutputTerminalBlock,
            [StringComparison]::Ordinal)
        if ($allowedOutputTerminalIndex -lt 0) {
            throw 'ProcessOperations kind-4 terminal dominance block was not found.'
        }
        $allowedOutputTerminalRanges += @{
            Start = $allowedOutputTerminalIndex
            End = $allowedOutputTerminalIndex + $allowedOutputTerminalBlock.Length
        }
    }
    $outputTerminalAssignments = [regex]::Matches(
        $outputOperationScanText,
        ('OperationState\s*:=\s*LMC_DIAG_SDO_STATE_' +
         '(?:COMPLETED|FAILED|EXPIRED|CANCELLED);'))
    foreach ($outputTerminalAssignment in $outputTerminalAssignments) {
        $terminalAssignmentAllowed = $false
        foreach ($allowedOutputTerminalRange in $allowedOutputTerminalRanges) {
            if ($outputTerminalAssignment.Index -ge
                    $allowedOutputTerminalRange.Start -and
                ($outputTerminalAssignment.Index +
                    $outputTerminalAssignment.Length) -le
                    $allowedOutputTerminalRange.End) {
                $terminalAssignmentAllowed = $true
                break
            }
        }
        if (-not $terminalAssignmentAllowed) {
            throw ('ProcessOperations kind-4 terminal state assignment is not ' +
                'dominated by a consumed completion or successful exact-token cancel.')
        }
    }
    $outputOutcomeAssignments = [regex]::Matches(
        $outputOperationScanText,
        '\bOperationOutcome\s*(?::=|[+\-*/]=)' )
    if ($outputTerminalAssignments.Count -ne 5 -or
        [regex]::Matches(
            $outputOperationScanText,
            '\bOperationState\s*(?::=|[+\-*/]=)').Count -ne 5 -or
        $outputOutcomeAssignments.Count -ne 5 -or
        [regex]::Matches(
            $outputOperationBlock,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_COMPLETED;').Count -ne 1 -or
        [regex]::Matches(
            $outputOperationBlock,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_FAILED;').Count -ne 2 -or
        [regex]::Matches(
            $outputOperationBlock,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_EXPIRED;').Count -ne 2 -or
        [regex]::Matches(
            $outputOperationScanText,
            ('OperationOutcome\s*:=\s*' +
             'LMC_DIAG_SDO_OUTCOME_SUCCESS;')).Count -ne 1 -or
        [regex]::Matches(
            $outputOperationScanText,
            ('OperationOutcome\s*:=\s*' +
             'LMC_DIAG_SDO_OUTCOME_FAILED;')).Count -ne 2 -or
        [regex]::Matches(
            $outputOperationScanText,
            ('OperationOutcome\s*:=\s*' +
             'LMC_DIAG_SDO_OUTCOME_TIMED_OUT;')).Count -ne 2) {
        throw ('ProcessOperations kind 4 must expose exactly one completion, ' +
            'two failure, and two expiry terminal state/outcome assignments ' +
            'with no numeric or alternate overwrite.')
    }
    if ($outputOperationScanText -match
        ('(?i)\b(?:TicketId|OperationToken|OperationKind|' +
         'OwnerSessionEpoch|TicketBootId|TicketMapRevision|' +
         'SdoSubmitCycle|SdoTimeoutCycles|SdoInternalDrainState)' +
         '\s*(?::=|[+\-*/]=)')) {
        throw ('ProcessOperations kind 4 must not mutate retained ticket, token, ' +
            'kind, owner, BootId, map, submit-cycle, or timeout identity.')
    }
    $outputOperationReturns = [regex]::Matches(
        $outputOperationScanText,
        '(?i)\bRETURN\s*;')
    $outputCopyCallMatch = [regex]::Match(
        $outputOperationScanText,
        'outputCopyResult\s*:=\s*InputLatch\.CopyOutputCompletion\(')
    $outputCompletionDispatchIndex = $outputOperationBlock.IndexOf(
        $outputCompletionConsumedBlock,
        [StringComparison]::Ordinal)
    if ($outputOperationReturns.Count -ne 2 -or
        -not $outputCopyCallMatch.Success -or
        $outputOperationReturns[0].Index -ge $outputCopyCallMatch.Index -or
        $outputCompletionDispatchIndex -lt 0 -or
        $outputOperationReturns[1].Index -le
            ($outputCompletionDispatchIndex +
                $outputCompletionConsumedBlock.Length)) {
        throw ('ProcessOperations kind 4 must RETURN exactly once on snapshot ' +
            'copy failure and once after all completion/cancel mapping, with no ' +
            'post-mailbox-consume early exit.')
    }
}

$diagnosticsServiceNotifyBlock = [regex]::Match(
    $diagnosticsService,
    '(?s)FUNCTION GLOBAL LMCDiagnosticsService::NotifySessionClosed.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($diagnosticsServiceNotifyBlock)) {
    throw 'LMCDiagnosticsService.NotifySessionClosed implementation was not found.'
}
Assert-Match $diagnosticsServiceNotifyBlock '(?s)SessionEpoch = BulkOwnerSessionEpoch.*?BulkState := 0.*?RecorderStore\.NotifySessionClosed\(SessionEpoch:=SessionEpoch\)' 'LMCDiagnosticsService does not release the matching Bulk owner and notify RecorderStore on session close.'
if ($topologyIoOutputIntegrated) {
    Assert-LasalExactDeclaredType `
        -Text $diagnosticsServiceNotifyBlock `
        -Name 'outputCancelResult' `
        -ExpectedType 'DINT' `
        -Owner 'LMCDiagnosticsService.NotifySessionClosed outputCancelResult'
    Assert-Match $diagnosticsServiceNotifyBlock (
        '(?s)\(LMC_DIAG_D5_SDO_READ_ENABLED = TRUE\).*?' +
        '\(LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = TRUE\).*?' +
        '\(LMC_DIAG_ECAT_IO_WRITE_GLOBAL_ENABLED = TRUE\).*?' +
        '\(SessionEpoch = OwnerSessionEpoch\) then') (
        'NotifySessionClosed owner cleanup gate must include future kind-4 ' +
        'output activation as well as the existing SDO gates.')
    $kind4NotifyBlock = Get-UniqueLasalIfBlockContaining `
        -Text $diagnosticsServiceNotifyBlock `
        -ConditionPattern 'OperationKind\s*=\s*4\s+then' `
        -RequiredPattern 'InputLatch\.CancelQueuedOutput\(' `
        -Message ('NotifySessionClosed kind 4 must be isolated ahead of generic ' +
            'SDO running/queued cleanup.')
    $notifyOwnerCleanupBlock = Get-UniqueLasalIfBlockContaining `
        -Text $diagnosticsServiceNotifyBlock `
        -ConditionPattern (
            '\(\(LMC_DIAG_D5_SDO_READ_ENABLED = TRUE\)[\s\S]*?' +
            '\(SessionEpoch = OwnerSessionEpoch\)\s+then') `
        -RequiredPattern 'if OperationKind = 4 then' `
        -Message ('NotifySessionClosed operation-owner cleanup must contain ' +
            'the complete kind-4 versus generic-SDO branch.')
    $notifyOwnerCleanupThenArm =
        Get-LasalFirstThenArm $notifyOwnerCleanupBlock
    if ($notifyOwnerCleanupThenArm.IndexOf(
            $kind4NotifyBlock,
            [StringComparison]::Ordinal) -lt 0) {
        throw ('NotifySessionClosed kind-4 branch must be dominated by the ' +
            'matching owner-session cleanup guard.')
    }
    Assert-Match $kind4NotifyBlock (
        '(?s)if OperationKind = 4 then\s*' +
        'if OperationState = LMC_DIAG_SDO_STATE_QUEUED then\s*' +
        'outputCancelResult := -2;\s*' +
        'if IsClientConnected\(#InputLatch\) then\s*' +
        'outputCancelResult := InputLatch\.CancelQueuedOutput\(\s*' +
        'ExpectedToken:=OperationToken\);\s*end_if;\s*' +
        'if outputCancelResult = 0 then\s*' +
        'OperationState := LMC_DIAG_SDO_STATE_CANCELLED;\s*' +
        'OperationOutcome := LMC_DIAG_SDO_OUTCOME_CANCELLED;.*?' +
        'end_if;\s*end_if;\s*' +
        'else\s*.*?' +
        'if OperationState = LMC_DIAG_SDO_STATE_RUNNING then.*?' +
        'MarkOrphan\(.*?' +
        'elsif \(OperationState = LMC_DIAG_SDO_STATE_QUEUED\).*?' +
        'end_if;\s*end_if;') (
        'NotifySessionClosed must cancel only an exact queued kind-4 mailbox, ' +
        'retain CAS-lost/running kind 4 for ProcessOperations, and keep generic ' +
        'SDO cleanup in the outer ELSE.')
    $kind4NotifyCancelSuccess = Get-UniqueLasalIfBlockContaining `
        -Text $kind4NotifyBlock `
        -ConditionPattern 'outputCancelResult\s*=\s*0\s+then' `
        -RequiredPattern 'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;' `
        -Message ('NotifySessionClosed kind-4 Cancelled state must be dominated ' +
            'by successful exact-token mailbox cancellation.')
    $kind4NotifyThenBlock = [regex]::Match(
        $kind4NotifyBlock,
        '(?s)\Aif OperationKind = 4 then(?<Body>.*?)\s*else\s*').Groups['Body'].Value
    if ([regex]::Matches(
            $kind4NotifyCancelSuccess,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;').Count -ne 1 -or
        [string]::IsNullOrWhiteSpace($kind4NotifyThenBlock) -or
        $kind4NotifyThenBlock -match
            '(?m)^\s*(?:TicketId|OwnerSessionEpoch|OperationToken)\s*:=\s*0;' ) {
        throw ('NotifySessionClosed must not clear kind-4 identity when RT may ' +
            'have claimed the write; only a successful queued CAS may mark it Cancelled.')
    }
    if ([regex]::Matches(
            $kind4NotifyBlock,
            '\boutputCancelResult\s*(?::=|[+\-*/]=)').Count -ne 2 -or
        [regex]::Matches(
            $kind4NotifyBlock,
            'outputCancelResult\s*:=\s*-2;').Count -ne 1 -or
        [regex]::Matches(
            $kind4NotifyBlock,
            'outputCancelResult\s*:=\s*InputLatch\.CancelQueuedOutput\(').Count -ne 1) {
        throw ('NotifySessionClosed kind 4 must assign its DINT cancel result ' +
            'only through one pending default and one exact-token mailbox call.')
    }
    $kind4NotifyThenArm = Get-LasalFirstThenArm $kind4NotifyBlock
    $kind4NotifyCancelSuccessArm =
        Get-LasalFirstThenArm $kind4NotifyCancelSuccess
    $kind4NotifyThenScanText = Get-LasalScanText $kind4NotifyThenArm
    $kind4NotifySuccessScanText =
        Get-LasalScanText $kind4NotifyCancelSuccessArm
    if ($kind4NotifyThenScanText -match
            ('(?i)\b(?:TicketId|OperationToken|OperationKind|' +
             'OwnerSessionEpoch|TicketBootId|TicketMapRevision|' +
             'SdoSubmitCycle|SdoTimeoutCycles|SdoInternalDrainState)' +
             '\s*(?::=|[+\-*/]=)') -or
        [regex]::Matches(
            $kind4NotifyThenScanText,
            '\bOperationState\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $kind4NotifyThenScanText,
            '\bOperationOutcome\s*(?::=|[+\-*/]=)').Count -ne 1 -or
        [regex]::Matches(
            $kind4NotifySuccessScanText,
            'OperationState\s*:=\s*LMC_DIAG_SDO_STATE_CANCELLED;').Count -ne 1 -or
        [regex]::Matches(
            $kind4NotifySuccessScanText,
            'OperationOutcome\s*:=\s*LMC_DIAG_SDO_OUTCOME_CANCELLED;').Count -ne 1) {
        throw ('NotifySessionClosed kind 4 must preserve all retained mailbox ' +
            'identity/drain state and publish its sole Cancelled state/outcome ' +
            'only inside successful exact-token cancellation.')
    }
    $diagnosticsServiceNotifyScanText =
        Get-LasalScanText $diagnosticsServiceNotifyBlock
    if ($kind4NotifyThenScanText -match '(?i)\bRETURN\s*;' -or
        $diagnosticsServiceNotifyScanText -match '(?i)\bRETURN\s*;') {
        throw ('NotifySessionClosed must not RETURN from kind-4 cleanup or the ' +
            'function before RecorderStore receives the session-close notification.')
    }
    $notifyProtectedMutationPattern =
        ('(?i)\b(?:outputCancelResult|TicketId|OperationToken|OperationKind|' +
         'OwnerSessionEpoch|TicketBootId|TicketMapRevision|OperationState|' +
         'OperationOutcome|SdoSubmitCycle|SdoCompletionCycle|' +
         'SdoLastProcessedCycle|SdoTimeoutCycles|SdoOperationErrorId|' +
         'SdoOperationDetail|SdoResultLength|SdoResultData|' +
         'SdoInternalDrainState|SdoSlaveReference|SdoObjectIndex|' +
         'SdoSubIndex|SdoValueType|SdoRequestedLength|SdoWriteData)' +
         '\s*(?::=|[+\-*/]=)')
    $kind4NotifyRangeStart = $diagnosticsServiceNotifyBlock.IndexOf(
        $kind4NotifyBlock,
        [StringComparison]::Ordinal)
    $kind4NotifyRangeEnd =
        $kind4NotifyRangeStart + $kind4NotifyBlock.Length
    if ($kind4NotifyRangeStart -lt 0) {
        throw 'NotifySessionClosed kind-4 ownership range was not found.'
    }
    foreach ($notifyProtectedMutation in [regex]::Matches(
            $diagnosticsServiceNotifyScanText,
            $notifyProtectedMutationPattern)) {
        if ($notifyProtectedMutation.Index -lt $kind4NotifyRangeStart -or
            ($notifyProtectedMutation.Index +
                $notifyProtectedMutation.Length) -gt $kind4NotifyRangeEnd) {
            throw ('NotifySessionClosed operation/mailbox state may mutate only ' +
                'inside the mutually exclusive kind-4 versus generic-SDO branch.')
        }
    }
    foreach ($genericCleanupPattern in @(
            'SdoAxis[1-4]\.MarkOrphan\(',
            'TicketId\s*:=\s*0;',
            'SdoInternalDrainState\s*:=')) {
        $notifyCleanupCount = [regex]::Matches(
            $diagnosticsServiceNotifyBlock,
            $genericCleanupPattern).Count
        $isolatedCleanupCount = [regex]::Matches(
            $kind4NotifyBlock,
            $genericCleanupPattern).Count
        if ($notifyCleanupCount -ne $isolatedCleanupCount) {
            throw ('NotifySessionClosed generic SDO cleanup token ' +
                "'$genericCleanupPattern' exists outside the kind-4 ELSE and " +
                'could fall through after a CAS-lost output write.')
        }
    }
}
Assert-Match $diagnosticsService '(?s)FUNCTION LMCDiagnosticsService::@STD.*?ret_code\s*:=\s*LMCDiagnosticsService\(\).*?END_FUNCTION' 'LMCDiagnosticsService @STD does not invoke its constructor.'
Assert-LMCDiagnosticsServiceConstructorReady `
    -DiagnosticsServiceText $diagnosticsService `
    -Owner 'LMCDiagnosticsService'

$diagnosticsServiceConstructorFixtureBlock = [regex]::Match(
    $diagnosticsService,
    ('(?ims)^[ \t]*FUNCTION[ \t]+' +
     'LMCDiagnosticsService::LMCDiagnosticsService[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
if ([string]::IsNullOrWhiteSpace(
        $diagnosticsServiceConstructorFixtureBlock)) {
    throw 'LMCDiagnosticsService constructor negative-fixture block was not found.'
}

$diagnosticsServiceConstructorNegativeFixtures = [ordered]@{}
$diagnosticsServiceConstructorNegativeFixtures['DeleteBulkScalar'] =
    ([regex]::new(
        ('(?im)^[ \t]*NextBulkConfigRevision\s*:=\s*0\s*;' +
         '[ \t]*(?:\r?\n|$)'))).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        '',
        1)
$diagnosticsServiceConstructorNegativeFixtures['DeleteD5Scalar'] =
    ([regex]::new(
        ('(?im)^[ \t]*SdoCompletionCycle\s*:=\s*0\s*;' +
         '[ \t]*(?:\r?\n|$)'))).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        '',
        1)
$diagnosticsServiceConstructorNegativeFixtures['DuplicateScalar'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*BulkState\s*:=\s*0\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        { param($match)
            $match.Groups['Line'].Value + "`n" +
                $match.Groups['Line'].Value
        },
        1)
$diagnosticsServiceConstructorNegativeFixtures['DeleteArrayZero'] =
    ([regex]::new(
        ('(?ims)^[ \t]*_memset\s*\(\s*dest\s*:=\s*' +
         '#\s*BulkSignalIds\s*\[\s*0\s*\]\s*,\s*' +
         'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
         'sizeof\s*\(\s*BulkSignalIds\s*\)\s*\)\s*;' +
         '[ \t]*(?:\r?\n|$)'))).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        '',
        1)
$diagnosticsServiceConstructorNegativeFixtures['PartialArrayZero'] =
    ([regex]::new(
        '(?i)sizeof\s*\(\s*BulkSignalIds\s*\)')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        'sizeof(BulkSignalIds) - 4',
        1)
$diagnosticsServiceConstructorNegativeFixtures['ArrayLoop'] =
    ([regex]::new(
        ('(?ims)^[ \t]*_memset\s*\(\s*dest\s*:=\s*' +
         '#\s*BulkSignalIds\s*\[\s*0\s*\]\s*,\s*' +
         'usByte\s*:=\s*0\s*,\s*cntr\s*:=\s*' +
         'sizeof\s*\(\s*BulkSignalIds\s*\)\s*\)\s*;'))).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        ("`tfor fixtureIndex := 0 to 23 do`n" +
         "`t`tBulkSignalIds[fixtureIndex] := 0;`n" +
         "`tend_for;"),
        1)
$diagnosticsServiceConstructorNegativeFixtures['EarlyReturn'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*NextBulkId\s*:=\s*0\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        { param($match)
            $match.Groups['Line'].Value + "`n`treturn;"
        },
        1)
$diagnosticsServiceConstructorNegativeFixtures['ConditionalBranch'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*BulkId\s*:=\s*0\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        { param($match)
            "`tif TRUE then`n" + $match.Groups['Line'].Value +
                "`n`tend_if;"
        },
        1)
$diagnosticsServiceConstructorNegativeFixtures['EarlySuccess'] =
    ([regex]::new(
        '(?im)^[ \t]*ret_code\s*:=\s*C_OK\s*;[ \t]*\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        '',
        1)
$diagnosticsServiceConstructorNegativeFixtures['EarlySuccess'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*NextBulkId\s*:=\s*0\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorNegativeFixtures['EarlySuccess'],
        { param($match)
            $match.Groups['Line'].Value + "`n`tret_code := C_OK;"
        },
        1)
$diagnosticsServiceConstructorNegativeFixtures['DuplicateSuccess'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*ret_code\s*:=\s*C_OK\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        { param($match)
            $match.Groups['Line'].Value + "`n" +
                $match.Groups['Line'].Value
        },
        1)
$diagnosticsServiceConstructorNegativeFixtures['UnexpectedCall'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*ret_code\s*:=\s*C_OK\s*;[ \t]*)\r?$')).Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        { param($match)
            "`tUnexpectedInitializer();`n" + $match.Groups['Line'].Value
        },
        1)

foreach ($negativeFixture in
    $diagnosticsServiceConstructorNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -ceq
        $diagnosticsServiceConstructorFixtureBlock) {
        throw (
            'LMCDiagnosticsService constructor negative fixture did not ' +
            "mutate the source: $($negativeFixture.Key).")
    }

    $negativeDiagnosticsService = $diagnosticsService.Replace(
        $diagnosticsServiceConstructorFixtureBlock,
        [string]$negativeFixture.Value)
    $negativeRejected = $false
    try {
        Assert-LMCDiagnosticsServiceConstructorReady `
            -DiagnosticsServiceText $negativeDiagnosticsService `
            -Owner (
                'LMCDiagnosticsService constructor negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCDiagnosticsService constructor verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
}

$diagnosticsServiceInventoryNegative = ([regex]::new(
    ('(?im)^[ \t]*BulkConfiguredCycle[ \t]*:[ \t]*UDINT[ \t]*;' +
     '[ \t]*(?:\r?\n|$)'))).Replace(
    $diagnosticsService,
    '',
    1)
if ($diagnosticsServiceInventoryNegative -ceq $diagnosticsService) {
    throw 'LMCDiagnosticsService state-inventory negative fixture did not mutate the source.'
}
$diagnosticsServiceInventoryRejected = $false
try {
    Assert-LMCDiagnosticsServiceConstructorReady `
        -DiagnosticsServiceText $diagnosticsServiceInventoryNegative `
        -Owner 'LMCDiagnosticsService state-inventory negative fixture'
}
catch {
    $diagnosticsServiceInventoryRejected = $true
}
if (-not $diagnosticsServiceInventoryRejected) {
    throw 'LMCDiagnosticsService constructor verifier accepted state inventory drift.'
}

$diagnosticsServiceArrayShapeNegative = ([regex]::new(
    ('(?im)^(?<Prefix>[ \t]*BulkSignalIds[ \t]*:[ \t]*ARRAY[ \t]*' +
     '\[[ \t]*0[ \t]*\.\.[ \t]*)23(?<Suffix>[ \t]*\][ \t]*OF[ \t]*' +
     'UDINT[ \t]*;[ \t]*)$'))).Replace(
    $diagnosticsService,
    { param($match)
        $match.Groups['Prefix'].Value + '22' +
            $match.Groups['Suffix'].Value
    },
    1)
if ($diagnosticsServiceArrayShapeNegative -ceq $diagnosticsService) {
    throw 'LMCDiagnosticsService array-shape negative fixture did not mutate the source.'
}
$diagnosticsServiceArrayShapeRejected = $false
try {
    Assert-LMCDiagnosticsServiceConstructorReady `
        -DiagnosticsServiceText $diagnosticsServiceArrayShapeNegative `
        -Owner 'LMCDiagnosticsService array-shape negative fixture'
}
catch {
    $diagnosticsServiceArrayShapeRejected = $true
}
if (-not $diagnosticsServiceArrayShapeRejected) {
    throw 'LMCDiagnosticsService constructor verifier accepted BulkSignalIds shape drift.'
}

$diagnosticsServiceTypeNegativeFixtures = [ordered]@{
    BulkState = ([regex]::new(
        ('(?im)^(?<Prefix>[ \t]*BulkState[ \t]*:[ \t]*)UINT' +
         '(?<Suffix>[ \t]*;[ \t]*)$'))).Replace(
        $diagnosticsService,
        { param($match)
            $match.Groups['Prefix'].Value + 'UDINT' +
                $match.Groups['Suffix'].Value
        },
        1)
    BootIdFault = ([regex]::new(
        ('(?im)^(?<Prefix>[ \t]*BootIdFault[ \t]*:[ \t]*)BOOL' +
         '(?<Suffix>[ \t]*;[ \t]*)$'))).Replace(
        $diagnosticsService,
        { param($match)
            $match.Groups['Prefix'].Value + 'UDINT' +
                $match.Groups['Suffix'].Value
        },
        1)
}
foreach ($typeNegativeFixture in
    $diagnosticsServiceTypeNegativeFixtures.GetEnumerator()) {
    if ($typeNegativeFixture.Value -ceq $diagnosticsService) {
        throw (
            'LMCDiagnosticsService type negative fixture did not mutate ' +
            "'$($typeNegativeFixture.Key)'.")
    }

    $typeNegativeRejected = $false
    try {
        Assert-LMCDiagnosticsServiceConstructorReady `
            -DiagnosticsServiceText ([string]$typeNegativeFixture.Value) `
            -Owner (
                'LMCDiagnosticsService type negative fixture ' +
                $typeNegativeFixture.Key)
    }
    catch {
        $typeNegativeRejected = $true
    }
    if (-not $typeNegativeRejected) {
        throw (
            'LMCDiagnosticsService constructor verifier accepted type drift ' +
            "for '$($typeNegativeFixture.Key)'.")
    }
}

Assert-Match $recorderStore '#define LMC_RECORDER_STORAGE_BYTES\s+1280000' 'LMCRecorderStore per-bank storage-size constant is not 1,280,000 bytes.'
Assert-Match $recorderStore '#define LMC_RECORDER_TOTAL_STORAGE_BYTES\s+2560000' 'LMCRecorderStore total two-bank storage-size constant is not 2,560,000 bytes.'
Assert-Match $recorderStore '#define LMC_RECORDER_BANK_COUNT\s+2' 'LMCRecorderStore fixed bank count is not two.'
Assert-Match $recorderStore '#define LMC_RECORDER_DOUBLE_BANK_ENABLED\s+FALSE' 'LMCRecorderStore Double-bank runtime gate must remain fail closed before PLC qualification.'
Assert-LMCRecorderStoreConstructorReady `
    -RecorderStoreText $recorderStore `
    -Owner 'LMCRecorderStore'

$recorderConstructorFixtureBlock = [regex]::Match(
    $recorderStore,
    ('(?ims)^[ \t]*FUNCTION[ \t]+' +
     'LMCRecorderStore::LMCRecorderStore[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
if ([string]::IsNullOrWhiteSpace($recorderConstructorFixtureBlock)) {
    throw 'LMCRecorderStore constructor negative-fixture block was not found.'
}

$recorderConstructorNegativeFixtures = [ordered]@{}
$recorderConstructorNegativeFixtures['DeleteScalar'] =
    ([regex]::new(
        '(?im)^[ \t]*ClosedSessionEpoch\s*:=\s*0\s*;[ \t]*(?:\r?\n|$)')).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderConstructorNegativeFixtures['DeleteArray'] =
    ([regex]::new(
        ('(?im)^[ \t]*_memset\s*\(\s*dest\s*:=\s*' +
         '#SignalOffsets\[0\]\s*,\s*usByte\s*:=\s*0\s*,\s*' +
         'cntr\s*:=\s*sizeof\(SignalOffsets\)\s*\)\s*;' +
         '[ \t]*(?:\r?\n|$)'))).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderConstructorNegativeFixtures['DeleteToken'] =
    ([regex]::new(
        ('(?im)^[ \t]*g_LMCRecorderLastReleasedRecoveryToken' +
         '\[2\]\s*:=\s*0\s*;[ \t]*(?:\r?\n|$)'))).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderConstructorNegativeFixtures['DeleteBankDescriptor'] =
    ([regex]::new(
        ('(?im)^[ \t]*g_LMCRecorderBankClosedSessionEpoch' +
         '\[bankIndex\]\s*:=\s*0\s*;[ \t]*(?:\r?\n|$)'))).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderBankStateCallPattern = (
    '(?ims)^[ \t]*sigclib_atomic_setU32\s*\(\s*' +
    'pValue\s*:=\s*#g_LMCRecorderBankState\s*\[\s*bankIndex\s*\]\s*,' +
    '\s*value\s*:=\s*LMC_RECORDER_EMPTY\s*\)\s*;')
$recorderBankStateCallMatch = [regex]::Match(
    $recorderConstructorFixtureBlock,
    $recorderBankStateCallPattern)
if (-not $recorderBankStateCallMatch.Success) {
    throw 'LMCRecorderStore bank-state negative-fixture call was not found.'
}
$recorderConstructorNegativeFixtures['DeleteBankStatePublication'] =
    ([regex]::new($recorderBankStateCallPattern)).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderConstructorNegativeFixtures['DeleteRetCode'] =
    ([regex]::new(
        '(?im)^[ \t]*ret_code\s*:=\s*C_OK\s*;[ \t]*(?:\r?\n|$)')).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)

$recorderConstructorNegativeFixtures['DuplicateScalar'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*ConfigId\s*:=\s*0\s*;[ \t]*)\r?$')).Replace(
        $recorderConstructorFixtureBlock,
        { param($match) $match.Groups['Line'].Value + "`n" +
            $match.Groups['Line'].Value },
        1)
$recorderConstructorNegativeFixtures['DuplicateToken'] =
    ([regex]::new(
        ('(?im)^(?<Line>[ \t]*g_LMCRecorderRecoveryToken' +
         '\[1\]\s*:=\s*0\s*;[ \t]*)\r?$'))).Replace(
        $recorderConstructorFixtureBlock,
        { param($match) $match.Groups['Line'].Value + "`n" +
            $match.Groups['Line'].Value },
        1)
$recorderConstructorNegativeFixtures['DuplicateBankDescriptor'] =
    ([regex]::new(
        ('(?im)^(?<Line>[ \t]*g_LMCRecorderBankMapRevision' +
         '\[bankIndex\]\s*:=\s*0\s*;[ \t]*)\r?$'))).Replace(
        $recorderConstructorFixtureBlock,
        { param($match) $match.Groups['Line'].Value + "`n" +
            $match.Groups['Line'].Value },
        1)

$recorderConstructorNegativeFixtures['EarlyReturn'] =
    ([regex]::new(
        ('(?im)^(?<Line>[ \t]*StateValue\s*:=\s*' +
         'LMC_RECORDER_EMPTY\s*;[ \t]*)\r?$'))).Replace(
        $recorderConstructorFixtureBlock,
        { param($match) "`tRETURN;`n" + $match.Groups['Line'].Value },
        1)
$branchedRecorderConstructor =
    ([regex]::new(
        ('(?im)^(?<Line>[ \t]*StateValue\s*:=\s*' +
         'LMC_RECORDER_EMPTY\s*;[ \t]*)\r?$'))).Replace(
        $recorderConstructorFixtureBlock,
        { param($match) "`tif TRUE then`n" + $match.Groups['Line'].Value },
        1)
$recorderConstructorNegativeFixtures['ConditionalBranch'] =
    ([regex]::new(
        '(?im)^(?<Line>[ \t]*BufferReleased\s*:=\s*TRUE\s*;[ \t]*)\r?$')).Replace(
        $branchedRecorderConstructor,
        { param($match) $match.Groups['Line'].Value + "`n`tend_if;" },
        1)
$recorderConstructorNegativeFixtures['LoopBound'] =
    ([regex]::new(
        ('(?im)(for[ \t]+bankIndex[ \t]*:=[ \t]*0[ \t]+to[ \t]+' +
         'LMC_RECORDER_BANK_COUNT[ \t]*-[ \t]*)1([ \t]+do)'))).Replace(
        $recorderConstructorFixtureBlock,
        '${1}2${2}',
        1)

$constructorWithoutBankState =
    ([regex]::new($recorderBankStateCallPattern)).Replace(
        $recorderConstructorFixtureBlock,
        '',
        1)
$recorderBankLoopHeaderPattern = (
    '(?im)^(?<Header>[ \t]*for[ \t]+bankIndex[ \t]*:=[ \t]*0[ \t]+' +
    'to[ \t]+LMC_RECORDER_BANK_COUNT[ \t]*-[ \t]*1[ \t]+do[ \t]*)\r?$')
$recorderConstructorNegativeFixtures['EarlyBankStatePublication'] =
    ([regex]::new($recorderBankLoopHeaderPattern)).Replace(
        $constructorWithoutBankState,
        { param($match) $match.Groups['Header'].Value + "`n" +
            $recorderBankStateCallMatch.Value },
        1)

$recorderConstructorNegativeFixtureCount = 0
foreach ($negativeFixture in
        $recorderConstructorNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -eq $recorderConstructorFixtureBlock) {
        throw (
            'LMCRecorderStore constructor negative fixture did not mutate ' +
            "the source for '$($negativeFixture.Key)'.")
    }
    $negativeSource = $recorderStore.Replace(
        $recorderConstructorFixtureBlock,
        $negativeFixture.Value)
    if ($negativeSource -eq $recorderStore) {
        throw (
            'LMCRecorderStore constructor negative fixture could not replace ' +
            "the source for '$($negativeFixture.Key)'.")
    }

    $negativeRejected = $false
    try {
        Assert-LMCRecorderStoreConstructorReady `
            -RecorderStoreText $negativeSource `
            -Owner 'LMCRecorderStore constructor negative fixture'
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'LMCRecorderStore constructor verifier accepted mutation ' +
            "'$($negativeFixture.Key)'.")
    }
    $recorderConstructorNegativeFixtureCount++
}

Assert-Match $recorderStore '(?s)VAR_GLOBAL\s+g_LMCRecorderData\s*:\s*ARRAY \[0\.\.2559999\] OF USINT;.*?g_LMCRecorderBankState\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderBankRecordId\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderBankOwnerSessionEpoch\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderBankDiagnosticsBootId\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderBankSampleCount\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderBankFrozenFirstSampleIndex\s*:\s*ARRAY \[0\.\.1\] OF UDINT;.*?g_LMCRecorderRecoveryToken\s*:\s*ARRAY \[0\.\.3\] OF UDINT;.*?g_LMCRecorderLastReleasedRecoveryToken\s*:\s*ARRAY \[0\.\.3\] OF UDINT;.*?g_LMCRecorderActiveGeneration\s*:\s*UDINT;\s*END_VAR' 'LMCRecorderStore fixed two-bank data, identity, recovery-token, tombstone, metadata, and active-generation storage is incomplete.'
Assert-Match $recorderStore '(?s)stride := TO_UDINT\(requestedChannelCount\) \* 4;.*?acceptedCapacity := LMC_RECORDER_STORAGE_BYTES / stride;.*?if acceptedCapacity > requestedCapacity then\s*acceptedCapacity := requestedCapacity;\s*end_if' 'LMCRecorderStore ConfigureRecorder does not clamp AcceptedCapacity to the fixed bank size and requested sample count.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::@STD.*?ret_code\s*:=\s*LMCRecorderStore\(\).*?END_FUNCTION' 'LMCRecorderStore @STD does not invoke its constructor.'
Assert-Match $recorderStore '(?s)FUNCTION LMCRecorderStore::LMCRecorderStore.*?StateValue := LMC_RECORDER_EMPTY.*?SamplePeriodCycles := 1.*?NextConfigId := 1.*?NextRecordId := 1.*?BufferReleased := TRUE.*?g_LMCRecorderRecoveryToken\[0\] := 0.*?g_LMCRecorderRecoveryToken\[3\] := 0.*?g_LMCRecorderLastReleasedRecoveryToken\[0\] := 0.*?g_LMCRecorderLastReleasedRecoveryToken\[3\] := 0.*?g_LMCRecorderActiveGeneration, value:=0.*?for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] := 0.*?g_LMCRecorderBankTriggerIndex\[bankIndex\] := 0xFFFFFFFF.*?pValue:=#g_LMCRecorderBankState\[bankIndex\].*?value:=LMC_RECORDER_EMPTY.*?ret_code := C_OK.*?END_FUNCTION' 'LMCRecorderStore constructor does not initialize recorder identity, timing, active and last-released recovery tokens, active generation, and both bank descriptors.'
Assert-Match $recorderStore '(?s)elsif \(CurrentDiagnosticsBootId = 0\) then\s*detailCode := 11' 'LMCRecorderStore does not reject the BootId-zero sentinel before stateful D3 processing.'

$recorderHandleRequestBlock = [regex]::Match(
    $recorderStore,
    '(?s)FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?END_FUNCTION').Value
if ([string]::IsNullOrWhiteSpace($recorderHandleRequestBlock)) {
    throw 'LMCRecorderStore.HandleRequest implementation was not found.'
}
$recorderConfigureLabelCount = [regex]::Matches(
    $recorderHandleRequestBlock,
    '(?m)^\s*0x7E40,\s*0x7E4C\s*:').Count
if ($recorderConfigureLabelCount -ne 1) {
    throw ('LMCRecorderStore shared 0x7E40/0x7E4C Configure handler count is ' +
        "$recorderConfigureLabelCount, expected one.")
}
foreach ($recorderCommandId in @(
    '0x7E41', '0x7E42', '0x7E43', '0x7E44',
    '0x7E45', '0x7E46', '0x7E47', '0x7E48', '0x7E49',
    '0x7E4A', '0x7E4B', '0x7E4D')) {
    $recorderCommandCount = [regex]::Matches(
        $recorderHandleRequestBlock,
        "(?m)^\s*$recorderCommandId\s*:").Count
    if ($recorderCommandCount -ne 1) {
        throw "LMCRecorderStore command $recorderCommandId handler count is $recorderCommandCount, expected one."
    }
}
$recorderConfigureBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E40,\s*0x7E4C:.*?(?=\s*0x7E41:)').Value
$recorderAppendBlock = [regex]::Match(
    $recorderStore,
    '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?END_FUNCTION').Value
$recorderStartBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E41:.*?(?=\s*0x7E42:)').Value
$recorderHeaderBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E45:.*?(?=\s*0x7E46:)').Value
$recorderChunkBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E46:.*?(?=\s*0x7E47:)').Value
$recorderReleaseBufferBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E47:.*?(?=\s*0x7E48:)').Value
$recorderReleaseConfigBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E48:.*?(?=\s*0x7E49:)').Value
$recorderAdoptBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E49:.*?(?=\s*0x7E4A:)').Value
$recorderInventoryBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E4A:.*?(?=\s*0x7E4B:)').Value
$recorderAdoptEmptyConfigurationBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E4B:.*?(?=\s*0x7E4D:)').Value
$recorderRecoverableInventoryBlock = [regex]::Match(
    $recorderHandleRequestBlock,
    '(?s)0x7E4D:.*?(?=\s*else\s*detailCode := 2)').Value
foreach ($recorderRequiredBlock in ([ordered]@{
        Configure = $recorderConfigureBlock
        Append = $recorderAppendBlock
        Start = $recorderStartBlock
        Header = $recorderHeaderBlock
        Chunk = $recorderChunkBlock
        ReleaseBuffer = $recorderReleaseBufferBlock
        ReleaseConfig = $recorderReleaseConfigBlock
        Adopt = $recorderAdoptBlock
        Inventory = $recorderInventoryBlock
        AdoptEmptyConfiguration = $recorderAdoptEmptyConfigurationBlock
        RecoverableInventory = $recorderRecoverableInventoryBlock
    }).GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($recorderRequiredBlock.Value)) {
        throw "LMCRecorderStore $($recorderRequiredBlock.Key) implementation block was not found."
    }
}
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:\s*.*?RequestSize <> 28.*?requestRecordId := \(pRequest \+ 8\)\^\$UDINT.*?requestBufferId := \(pRequest \+ 12\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 16\)\^\$UDINT.*?requestOwnerEpoch := \(pRequest \+ 20\)\^\$UDINT.*?requestBootId := \(pRequest \+ 24\)\^\$UDINT.*?TriggerType = 0.*?TriggerRequestSequence.*?ResponseSize := 16.*?0x7E43:' 'TriggerRecorder 0x7E42 does not validate identity/ownership and queue an RT trigger request.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E43:.*?requestRecordId <>\s*g_LMCRecorderBankRecordId\[targetBankIndex\].*?expectedMapRevision <>\s*g_LMCRecorderBankMapRevision\[targetBankIndex\].*?requestBootId <>\s*g_LMCRecorderBankDiagnosticsBootId\[targetBankIndex\].*?requestOwnerEpoch <>\s*g_LMCRecorderBankOwnerSessionEpoch\[targetBankIndex\].*?bankState = LMC_RECORDER_READY.*?bankState = LMC_RECORDER_UPLOADING.*?ResponseSize := 16.*?bankState <> LMC_RECORDER_ARMED.*?bankState <> LMC_RECORDER_RECORDING.*?detailCode := 19.*?requestRecordId <> RecordId.*?requestBufferId <> BufferId.*?StopRequestSequence.*?ResponseSize := 16.*?0x7E44:' 'StopRecorder must preserve target-bank identity/ownership checks, acknowledge Ready/Uploading idempotently, and queue only the active-bank stop.'
Assert-Match $recorderHandleRequestBlock '(?s)if detailCode <> 0 then.*?\(pResponse \+ 4\)\^\$UINT := 1.*?ResponseSize := 16' 'LMCRecorderStore reserved and error commands do not return the common 16-byte error envelope.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?StateValue.*?g_LMCRecorderData.*?SampleCount \+= 1.*?END_FUNCTION' 'LMCRecorderStore RT AppendSnapshot capture path is incomplete.'
Assert-Match $recorderAppendBlock '(?s)generationBefore := sigclib_atomic_getU32\(\s*pValue:=#g_LMCRecorderActiveGeneration\).*?state := sigclib_atomic_getU32\(pValue:=#StateValue\).*?enteredRecordId := RecordId.*?enteredBufferId := BufferId.*?generationAfter := sigclib_atomic_getU32\(\s*pValue:=#g_LMCRecorderActiveGeneration\).*?generationBefore <> generationAfter.*?enteredActiveState := \(state = LMC_RECORDER_ARMED\).*?state = LMC_RECORDER_RECORDING.*?if enteredActiveState = FALSE then.*?RETURN' 'AppendSnapshot does not take a stable even-generation active identity snapshot before touching a recorder bank.'
Assert-Match $recorderAppendBlock '(?s)bankBaseOffset := enteredBufferId \* LMC_RECORDER_STORAGE_BYTES.*?dataOffset := bankBaseOffset \+ WriteSampleIndex.*?g_LMCRecorderData\[TO_DINT\(dataOffset\)\].*?generationBefore = generationAfter.*?RecordId = enteredRecordId.*?BufferId = enteredBufferId.*?publishedBankState = LMC_RECORDER_ARMED.*?publishedBankState = LMC_RECORDER_RECORDING.*?g_LMCRecorderBankSampleCount\[bankIndex\] := SampleCount.*?g_LMCRecorderBankFrozenFirstSampleIndex\[bankIndex\] :=\s*FrozenFirstSampleIndex.*?sigclib_atomic_setU32\(\s*pValue:=#g_LMCRecorderBankState\[bankIndex\], value:=state\)' 'AppendSnapshot does not isolate active-bank data, guard terminal immutability, and publish dynamic metadata before the atomic bank state.'
foreach ($recorderImmutableTailToken in @(
        'g_LMCRecorderBankRecordId[bankIndex] := RecordId',
        'g_LMCRecorderBankConfigId[bankIndex] := ConfigId',
        'g_LMCRecorderBankConfigRevision[bankIndex] := ConfigRevision',
        'g_LMCRecorderBankMapRevision[bankIndex] := MapRevision',
        'g_LMCRecorderBankOwnerSessionEpoch[bankIndex] := OwnerSessionEpoch',
        'g_LMCRecorderBankDiagnosticsBootId[bankIndex] := DiagnosticsBootId',
        'g_LMCRecorderBankSampleCapacity[bankIndex] := SampleCapacity')) {
    if ($recorderAppendBlock.Contains($recorderImmutableTailToken)) {
        throw "AppendSnapshot must not rewrite Start-published immutable bank identity '$recorderImmutableTailToken'."
    }
}
Assert-Match $recorderStartBlock '(?s)activeGeneration := sigclib_atomic_getU32\(\s*pValue:=#g_LMCRecorderActiveGeneration\) \+ 1.*?pValue:=#g_LMCRecorderActiveGeneration.*?RecordId := NextRecordId.*?BufferId := TO_UDINT\(selectedBankIndex\).*?g_LMCRecorderBankRecordId\[selectedBankIndex\] := RecordId.*?g_LMCRecorderBankOwnerSessionEpoch\[selectedBankIndex\] :=\s*OwnerSessionEpoch.*?g_LMCRecorderBankDiagnosticsBootId\[selectedBankIndex\] :=\s*DiagnosticsBootId.*?pValue:=#g_LMCRecorderBankState\[selectedBankIndex\].*?value:=LMC_RECORDER_ARMED.*?pValue:=#StartRequestSequence.*?pValue:=#StateValue, value:=LMC_RECORDER_ARMED.*?activeGeneration \+= 1.*?pValue:=#g_LMCRecorderActiveGeneration' 'StartRecorder does not publish active identity, selected-bank descriptor, request, and state inside an odd/even generation bracket.'
Assert-Match $recorderStartBlock '(?s)recorderBufferCount := 1.*?BufferMode = 2.*?recorderBufferCount := LMC_RECORDER_BANK_COUNT.*?for bankIndex := 0 to recorderBufferCount - 1 do.*?bankState = LMC_RECORDER_ARMED.*?bankState = LMC_RECORDER_RECORDING.*?activeCaptureFound := TRUE.*?bankState = LMC_RECORDER_CONFIGURED.*?g_LMCRecorderBankRecordId\[bankIndex\] = 0.*?selectedBankIndex := bankIndex.*?activeCaptureFound \| \(freeBankFound = FALSE\) then\s*detailCode := 9' 'StartRecorder does not select the first free bank, preserve one active capture, and reject a full two-bank recorder as ResourceBusy.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::AppendSnapshot.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of.*?LMC_RECORDER_STOP_TRIGGER_COMPLETE.*?END_FUNCTION' 'LMCRecorderStore D4 edge/window/mask RT trigger path is incomplete.'
Assert-Match $recorderStore '(?s)triggerInputValid :=\s*\(\(pSnapshot \+ 12\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ 16\)\^\$UDINT = 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset\)\^\$DINT <> 0\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 4\)\^\$UDINT = 8\) &\s*\(\(pSnapshot \+ triggerHealthOffset \+ 12\)\^\$UDINT = 0\)' 'LMCRecorderStore trigger validity must require master OP/no missed frame and axis Online/OP/AL=0.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64.*?TriggerSignalId.*?triggerInputValid :=.*?triggerHealthOffset.*?prehistoryReady := SampleCount >= PreTriggerSamples.*?if prehistoryReady then.*?triggerRequest <> TriggerAppliedSequence.*?triggerEvent := TRUE.*?elsif triggerInputValid then.*?TriggerType = 1.*?TriggerType = 2.*?case TriggerOperator of' 'LMCRecorderStore automatic edge/window/mask trigger evaluation is not gated by a valid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)if triggerInputValid then\s*PreviousTriggerValue := triggerRaw;\s*PreviousTriggerValid := TRUE;\s*else\s*.*?PreviousTriggerValid := FALSE;\s*end_if' 'LMCRecorderStore does not reset edge/window history across an invalid EtherCAT trigger sample.'
Assert-Match $recorderStore '(?s)FrozenFirstSampleIndex :=.*?WriteSampleIndex \+ SampleCapacity - SampleCount.*?FUNCTION GLOBAL LMCRecorderStore::HandleRequest.*?physicalSampleIndex :=.*?g_LMCRecorderBankFrozenFirstSampleIndex\[.*?targetBankIndex\].*?offsetSample.*?MOD\s*g_LMCRecorderBankSampleCapacity\[targetBankIndex\].*?_memcpy' 'LMCRecorderStore does not preserve and upload the selected bank pre-trigger ring data in chronological order.'
Assert-Match $recorderStore '(?s)stopRequest <> StopAppliedSequence.*?TriggerIndex = 0xFFFFFFFF.*?FrozenFirstSampleIndex :=\s*\(WriteSampleIndex \+ SampleCapacity - SampleCount\).*?StopReason := LMC_RECORDER_STOP_USER' 'LMCRecorderStore does not freeze chronological pre-trigger ring order when the user stops before a trigger.'
Assert-Match $recorderStore '(?s)StopReason := LMC_RECORDER_STOP_USER;\s*if SampleCount = 0 then.*?EndCycle := cycleCounter' 'LMCRecorderStore user stop must preserve the End metadata of the last copied sample.'
Assert-Match $recorderHandleRequestBlock '(?s)0x7E42:.*?TriggerType = 0.*?g_LMCRecorderBankTriggerIndex\[targetBankIndex\] <>\s*0xFFFFFFFF then.*?detailCode := 19.*?TriggerRequestSequence' 'TriggerRecorder must reject a second force-trigger after the selected record has already triggered.'
Assert-Match $recorderConfigureBlock '(?s)recoverableConfigure := \(CommandId = 0x7E4C\).*?configureHeaderSize := 56.*?signalListOffset := 56.*?if recoverableConfigure then\s*configureHeaderSize := 72;\s*signalListOffset := 72.*?RequestSize < TO_UDINT\(configureHeaderSize\).*?requestRecoveryToken0 := \(pRequest \+ 56\)\^\$UDINT.*?requestRecoveryToken3 := \(pRequest \+ 68\)\^\$UDINT.*?RequestSize <> TO_UDINT\(configureHeaderSize \+\s*TO_DINT\(requestedChannelCount\) \* 4\).*?signalId := \(pRequest \+ signalListOffset \+\s*TO_DINT\(channelIndex\) \* 4\)\^\$UDINT' 'ConfigureRecorder/ConfigureRecoverableRecorder does not preserve the exact unsigned 56+4N and 72+4N request layouts with recovery token P56..71 and signals at P72.'
Assert-Match $recorderConfigureBlock '(?s)\(CommandId = 0x7E40\) & \(bufferMode = 2\) then\s*detailCode := 2.*?recoverableConfigure &\s*\(LMC_RECORDER_DOUBLE_BANK_ENABLED = FALSE\) then\s*detailCode := 2.*?recoverableConfigure & \(bufferMode <> 2\) then\s*detailCode := 12.*?requestedConfigId = 0.*?requestRecoveryTokenNonzero = FALSE.*?detailCode := 12' 'Ordinary ConfigureRecorder must always reject Double while 0x7E4C remains gate-disabled and accepts only a nonzero-token exact Double request.'
Assert-Match $recorderConfigureBlock '(?s)requestMatchesActiveRecoveryToken :=.*?g_LMCRecorderRecoveryToken\[3\].*?requestMatchesLastReleasedRecoveryToken :=.*?g_LMCRecorderLastReleasedRecoveryToken\[3\].*?requestMatchesActiveRecoveryToken \|\s*requestMatchesLastReleasedRecoveryToken\) then\s*detailCode := 10.*?ConfigRevision \+= 1' 'ConfigureRecoverableRecorder does not reject active-token replay and per-boot last-released-token reuse before revision mutation.'
Assert-Match $recorderConfigureBlock '(?s)if recoverableConfigure &\s*\(\(ConfigId <> 0\).*?state <> LMC_RECORDER_EMPTY.*?MapRevision <> 0.*?OwnerSessionEpoch <> 0.*?ClosedSessionEpoch <> 0.*?DiagnosticsBootId <> 0.*?RecordId <> 0.*?BufferId <> 0.*?BufferReleased = FALSE.*?BufferMode <> 0.*?ChannelCount <> 0.*?SampleCapacity <> 0.*?SampleStrideBytes <> 0.*?storedRecoveryTokenNonzero.*?bankState <> LMC_RECORDER_EMPTY.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankMapRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankDiagnosticsBootId\[bankIndex\] <> 0.*?recoverableConfigure & \(inventoryValid = FALSE\) then\s*detailCode := 10' 'ConfigureRecoverableRecorder does not enforce canonical empty-store no-overwrite semantics before mutation.'
Assert-Match $recorderConfigureBlock '(?s)\(recoverableConfigure = FALSE\) &\s*storedRecoveryTokenNonzero then\s*detailCode := 10.*?triggerType = 0.*?bufferMode <> 0.*?bufferMode <> 2.*?triggerType <> 0.*?bufferMode <> 1.*?bufferMode <> 2.*?expectedTriggerValueType.*?preTriggerSamples >= requestedCapacity.*?triggerOperator < 5.*?triggerValue <> 0.*?TriggerSignalOffset := triggerSignalOffset' 'ConfigureRecorder does not preserve an active recovery token or retain complete Manual/Triggered validation.'
Assert-Match $recorderConfigureBlock '(?s)recorderBufferCount := 1.*?if bufferMode = 2 then\s*recorderBufferCount := LMC_RECORDER_BANK_COUNT.*?acceptedCapacity := LMC_RECORDER_STORAGE_BYTES / stride.*?ResponseCapacity < TO_UDINT\(configureHeaderSize\).*?if recoverableConfigure then\s*g_LMCRecorderRecoveryToken\[0\] :=\s*requestRecoveryToken0.*?g_LMCRecorderRecoveryToken\[3\] :=\s*requestRecoveryToken3.*?pValue:=#StateValue.*?value:=LMC_RECORDER_CONFIGURED.*?reservedDataBytes := acceptedCapacity \* stride \*\s*TO_UDINT\(recorderBufferCount\).*?\(pResponse \+ 42\)\^\$UINT := recorderBufferCount.*?\(pResponse \+ 56\)\^\$UDINT :=\s*g_LMCRecorderRecoveryToken\[0\].*?\(pResponse \+ 68\)\^\$UDINT :=\s*g_LMCRecorderRecoveryToken\[3\].*?ResponseSize := 72;\s*else\s*ResponseSize := 56' 'ConfigureRecoverableRecorder does not enforce the unsigned response capacity, publish the active token before Configured state, or return its exact 72-byte echo while preserving ordinary 56-byte responses.'
Assert-Match $recorderStore '(?s)triggerHealthOffset := 64 \+\s*\(\(\(TriggerSignalId shr 8\) and 0xFF\) - 1\) \* 36' 'AppendSnapshot does not bind trigger validity to the configured physical axis health image.'
Assert-Match $recorderStore '(?s)FUNCTION GLOBAL LMCRecorderStore::NotifySessionClosed.*?SessionEpoch = OwnerSessionEpoch.*?ClosedSessionEpoch := SessionEpoch.*?for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] = SessionEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] := SessionEpoch.*?END_FUNCTION' 'LMCRecorderStore does not retain the shared and per-bank closed owner epoch for Recorder adoption.'
Assert-Match $recorderHeaderBlock '(?s)pValue:=#g_LMCRecorderBankState\[targetBankIndex\].*?state <> LMC_RECORDER_READY.*?state <> LMC_RECORDER_UPLOADING.*?g_LMCRecorderBankRecordId\[targetBankIndex\].*?g_LMCRecorderBankSampleCount\[targetBankIndex\].*?g_LMCRecorderBankTriggerIndex\[targetBankIndex\]' 'ReadRecorderHeader does not acquire and return the selected immutable terminal bank.'
Assert-Match $recorderChunkBlock '(?s)pValue:=#g_LMCRecorderBankState\[targetBankIndex\].*?state <> LMC_RECORDER_READY.*?state <> LMC_RECORDER_UPLOADING.*?pValue:=#g_LMCRecorderBankState\[targetBankIndex\].*?value:=LMC_RECORDER_UPLOADING.*?bankDataBaseOffset := requestBufferId \*\s*LMC_RECORDER_STORAGE_BYTES.*?g_LMCRecorderBankFrozenFirstSampleIndex\[.*?targetBankIndex\].*?MOD\s*g_LMCRecorderBankSampleCapacity\[targetBankIndex\].*?#g_LMCRecorderData\[TO_DINT\(dataOffset\)\]' 'ReadRecorderChunk does not preserve target-bank Uploading state and bank-derived bounded data offsets.'
if ($recorderChunkBlock -match 'pValue:=#StateValue') {
    throw 'ReadRecorderChunk must not change the active capture StateValue while uploading another bank.'
}
Assert-Match $recorderReleaseBufferBlock '(?s)targetBankIndex := TO_UINT\(requestBufferId\).*?g_LMCRecorderBankRecordId\[targetBankIndex\] := 0.*?g_LMCRecorderBankFrozenFirstSampleIndex\[targetBankIndex\] := 0.*?pValue:=#g_LMCRecorderBankState\[targetBankIndex\].*?value:=LMC_RECORDER_CONFIGURED.*?currentBankSelected then.*?RecordId := 0.*?pValue:=#StateValue.*?value:=LMC_RECORDER_CONFIGURED.*?for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?BufferReleased := FALSE.*?if BufferReleased then\s*.*?BufferId := 0' 'ReleaseRecorderBuffer does not clear only the exact target bank, preserve other occupied banks, and neutralize the class BufferId after the last bank is free.'
if ($recorderReleaseBufferBlock -match 'g_LMCRecorderBank\w+\[bankIndex\]\s*:=' ) {
    throw 'ReleaseRecorderBuffer must not mutate non-target bank metadata through its all-bank occupancy scan.'
}
Assert-Match $recorderReleaseConfigBlock '(?s)for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?bankState > LMC_RECORDER_CONFIGURED.*?activeCaptureFound := TRUE.*?elsif activeCaptureFound then\s*detailCode := 9.*?for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] := 0.*?pValue:=#g_LMCRecorderBankState\[bankIndex\].*?value:=LMC_RECORDER_EMPTY.*?g_LMCRecorderLastReleasedRecoveryToken\[0\] :=\s*g_LMCRecorderRecoveryToken\[0\].*?g_LMCRecorderLastReleasedRecoveryToken\[3\] :=\s*g_LMCRecorderRecoveryToken\[3\].*?g_LMCRecorderRecoveryToken\[0\] := 0.*?g_LMCRecorderRecoveryToken\[3\] := 0.*?pValue:=#StateValue, value:=LMC_RECORDER_EMPTY' 'ReleaseRecorder must reject any occupied bank, clear both bank descriptors, and publish the active token into the per-boot last-released tombstone before clearing it and publishing Empty.'
Assert-Match $recorderReleaseConfigBlock '(?s)if \(g_LMCRecorderRecoveryToken\[0\] <> 0\) \|\s*\(g_LMCRecorderRecoveryToken\[1\] <> 0\) \|\s*\(g_LMCRecorderRecoveryToken\[2\] <> 0\) \|\s*\(g_LMCRecorderRecoveryToken\[3\] <> 0\) then.*?g_LMCRecorderLastReleasedRecoveryToken\[0\] :=\s*g_LMCRecorderRecoveryToken\[0\].*?g_LMCRecorderLastReleasedRecoveryToken\[3\] :=\s*g_LMCRecorderRecoveryToken\[3\].*?end_if;\s*g_LMCRecorderRecoveryToken\[0\] := 0.*?g_LMCRecorderRecoveryToken\[3\] := 0' 'ReleaseRecorder must preserve the last nonzero recovery-token tombstone across an ordinary zero-token 0x7E40 configuration release while always clearing the active token.'
foreach ($recoveryTokenWord in 0..3) {
    $activeTokenWriteCount = [regex]::Matches(
        $recorderStore,
        "(?m)^\s*g_LMCRecorderRecoveryToken\[$recoveryTokenWord\]\s*:=").Count
    if ($activeTokenWriteCount -ne 3) {
        throw ("Active Recorder recovery token word $recoveryTokenWord has " +
            "$activeTokenWriteCount writes; expected only configure, exact release, and constructor.")
    }
    $lastReleasedTokenWriteCount = [regex]::Matches(
        $recorderStore,
        "(?m)^\s*g_LMCRecorderLastReleasedRecoveryToken\[$recoveryTokenWord\]\s*:=").Count
    if ($lastReleasedTokenWriteCount -ne 2) {
        throw ("Last-released Recorder recovery token word $recoveryTokenWord has " +
            "$lastReleasedTokenWriteCount writes; expected only exact release and constructor.")
    }
}
Assert-Match $recorderAdoptBlock '(?s)if requestRecordId = 0 then.*?requestBufferId <> 0.*?detailCode := 22.*?elsif BufferMode = 2 then\s*detailCode := 22' 'AdoptRecorder does not reject ambiguous zero-ID discovery in Double mode.'
Assert-Match $recorderAdoptBlock '(?s)requestRecordId <>\s*g_LMCRecorderBankRecordId\[targetBankIndex\].*?requestBootId <> localBootId.*?state < LMC_RECORDER_ARMED.*?state > LMC_RECORDER_UPLOADING.*?localConfigId <> ConfigId.*?localConfigRevision <> ConfigRevision.*?localMapRevision <> MapRevision.*?g_LMCRecorderBankOwnerSessionEpoch\[targetBankIndex\] =\s*CallerSessionEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[targetBankIndex\] = 0.*?adoptionEligible := TRUE' 'AdoptRecorder does not preserve exact target identity or idempotent exact adoption by the rebound owner.'
Assert-Match $recorderAdoptBlock '(?s)oldOwnerEpoch :=\s*g_LMCRecorderBankOwnerSessionEpoch\[targetBankIndex\].*?for bankIndex := 0 to recorderBufferCount - 1 do.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\].*?oldOwnerEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\].*?oldOwnerEpoch.*?g_LMCRecorderBankDiagnosticsBootId\[bankIndex\].*?localBootId.*?g_LMCRecorderBankConfigId\[bankIndex\].*?localConfigId.*?g_LMCRecorderBankConfigRevision\[bankIndex\].*?localConfigRevision.*?g_LMCRecorderBankMapRevision\[bankIndex\].*?localMapRevision.*?adoptionEligible := FALSE.*?OwnerSessionEpoch := CallerSessionEpoch.*?for bankIndex := 0 to recorderBufferCount - 1 do.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] :=\s*CallerSessionEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] := 0.*?ClosedSessionEpoch := 0.*?g_LMCRecorderBankRecordId\[targetBankIndex\].*?requestBufferId.*?CallerSessionEpoch' 'AdoptRecorder does not fail closed on inconsistent occupied-bank identity or atomically rebind all same-lease banks for later exact idempotent adoption.'
Assert-Match $recorderInventoryBlock '(?s)RequestSize <> 24.*?LMC_RECORDER_DOUBLE_BANK_ENABLED = FALSE then\s*.*?detailCode := 2.*?requestBootId := \(pRequest \+ 8\)\^\$UDINT.*?requestConfigId := \(pRequest \+ 12\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 16\)\^\$UDINT.*?requestConfigRevision := \(pRequest \+ 20\)\^\$UDINT' 'ReadRecorderBankInventory 0x7E4A must preserve its exact 24-byte identity request and dormant Double-bank gate.'
Assert-Match $recorderInventoryBlock '(?s)requestBootId = 0.*?requestConfigId = 0.*?expectedMapRevision = 0.*?requestBootId <> CurrentDiagnosticsBootId.*?expectedMapRevision <> LMC_RECORDER_MAP_REVISION.*?elsif ConfigId = 0 then' 'ReadRecorderBankInventory must validate the nonzero requested identity and current BootId/map before considering canonical configuration absence.'
Assert-Match $recorderInventoryBlock '(?s)elsif ConfigId = 0 then.*?requestConfigRevision = 0.*?state <> LMC_RECORDER_EMPTY.*?MapRevision <> 0.*?OwnerSessionEpoch <> 0.*?ClosedSessionEpoch <> 0.*?DiagnosticsBootId <> 0.*?RecordId <> 0.*?BufferId <> 0.*?BufferReleased = FALSE.*?BufferMode <> 0.*?ChannelCount <> 0.*?SampleCapacity <> 0.*?SampleStrideBytes <> 0.*?inventoryValid := FALSE' 'ReadRecorderBankInventory canonical-absence proof does not require a known nonzero revision and every class-level Recorder field to be empty.'
Assert-Match $recorderInventoryBlock '(?s)elsif ConfigId = 0 then.*?for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?bankState <> LMC_RECORDER_EMPTY.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankMapRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankDiagnosticsBootId\[bankIndex\] <> 0.*?inventoryValid := FALSE.*?if inventoryValid then\s*.*?detailCode := 32;\s*else\s*detailCode := 10' 'ReadRecorderBankInventory canonical-absence proof does not require both physical banks to be empty or return only typed detail 32 after the complete predicate.'
Assert-Match $recorderInventoryBlock '(?s)elsif requestBootId <> DiagnosticsBootId.*?expectedMapRevision <> MapRevision.*?requestConfigId <> ConfigId.*?requestConfigRevision <> 0.*?requestConfigRevision <> ConfigRevision.*?BufferMode <> 2.*?OwnerSessionEpoch <> CallerSessionEpoch.*?ClosedSessionEpoch = 0' 'ReadRecorderBankInventory does not fail closed for active configuration BootId, ConfigId, optional ConfigRevision, map, mode, or foreign-owner mismatches.'
Assert-Match $recorderInventoryBlock '(?s)for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] = 0.*?bankState <> LMC_RECORDER_CONFIGURED.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?inventoryValid := FALSE.*?g_LMCRecorderBankConfigId\[bankIndex\] <> ConfigId.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <>\s*ConfigRevision.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <>\s*OwnerSessionEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <>\s*ClosedSessionEpoch.*?bankState < LMC_RECORDER_ARMED.*?bankState > LMC_RECORDER_UPLOADING.*?inventoryValid := FALSE' 'ReadRecorderBankInventory does not validate every empty and occupied bank before publishing recovery metadata.'
Assert-Match $recorderInventoryBlock '(?s)localRecordId := 0.*?localRecordId = 0 then.*?localRecordId :=\s*g_LMCRecorderBankRecordId\[bankIndex\].*?elsif localRecordId =\s*g_LMCRecorderBankRecordId\[bankIndex\] then\s*inventoryValid := FALSE' 'ReadRecorderBankInventory does not reject duplicate nonzero RecordId values across occupied banks.'
Assert-Match $recorderInventoryBlock '(?s)inventoryCount = 0.*?state <> LMC_RECORDER_CONFIGURED.*?\(pResponse \+ 16\)\^\$UDINT := DiagnosticsBootId.*?\(pResponse \+ 20\)\^\$UDINT := ConfigId.*?\(pResponse \+ 24\)\^\$UDINT := ConfigRevision.*?\(pResponse \+ 32\)\^\$UDINT := OwnerSessionEpoch.*?\(pResponse \+ 36\)\^\$UDINT := ClosedSessionEpoch.*?\(pResponse \+ 40\)\^\$UINT := TO_UINT\(state\).*?\(pResponse \+ 44\)\^\$USINT := TO_USINT\(inventoryCount\).*?g_LMCRecorderBankRecordId\[bankIndex\].*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\].*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\].*?ResponseSize := 88' 'ReadRecorderBankInventory does not return configuration identity/state for zero banks and compact exact identity/state/owner entries for occupied banks.'
$recorderDeclarationVariables = [regex]::Match(
    $recorderStore,
    '(?s)LMCRecorderStore\s*:\s*CLASS.*?//Variables:(?<Variables>.*?)//Functions:'
).Groups['Variables'].Value
$recorderPersistentStateNames = @(
    [regex]::Matches(
        $recorderDeclarationVariables,
        '(?m)^\s*(?<Name>[A-Za-z_][A-Za-z0-9_]*)\s*:') |
        ForEach-Object { $_.Groups['Name'].Value }
)
if ($recorderPersistentStateNames.Count -eq 0) {
    throw 'LMCRecorderStore persistent state declarations were not found.'
}

$recorderDirectMutationSuffix = (
    '(?:\s*\[[^\]\r\n]+\])?' +
    '(?:\$[A-Za-z_][A-Za-z0-9_]*)?\s*' +
    '(?::=|\+=|-=|\*=|/=|&=|\|=|\^=|\+\+|--)'
)
$recorderStorageMutationPatterns = @(
    ('(?m)\bg_LMCRecorder[A-Za-z0-9_]*' +
        $recorderDirectMutationSuffix),
    '(?s)(?:_memset|_memcpy)\s*\(',
    '(?s)sigclib_atomic_(?!get[A-Za-z0-9_]*\s*\()[A-Za-z0-9_]+\s*\('
)

foreach ($persistentStateName in $recorderPersistentStateNames) {
    $persistentAssignmentPattern = (
        '(?m)(?<![A-Za-z0-9_])' +
        [regex]::Escape($persistentStateName) +
        $recorderDirectMutationSuffix
    )
    if ([regex]::IsMatch(
            $recorderInventoryBlock,
            $persistentAssignmentPattern)) {
        throw (
            'ReadRecorderBankInventory must remain read-only; found a write to ' +
            "persistent state '$persistentStateName'.")
    }
}
foreach ($storageMutationPattern in $recorderStorageMutationPatterns) {
    if ([regex]::IsMatch(
            $recorderInventoryBlock,
            $storageMutationPattern)) {
        throw 'ReadRecorderBankInventory must not mutate Recorder bank/data storage.'
    }
}
Assert-Match $recorderRecoverableInventoryBlock '(?s)RequestSize <> 36.*?CallerSessionEpoch = 0.*?LMC_RECORDER_DOUBLE_BANK_ENABLED = FALSE then\s*.*?detailCode := 2.*?requestBootId := \(pRequest \+ 8\)\^\$UDINT.*?requestConfigId := \(pRequest \+ 12\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 16\)\^\$UDINT.*?requestRecoveryToken0 := \(pRequest \+ 20\)\^\$UDINT.*?requestRecoveryToken1 := \(pRequest \+ 24\)\^\$UDINT.*?requestRecoveryToken2 := \(pRequest \+ 28\)\^\$UDINT.*?requestRecoveryToken3 := \(pRequest \+ 32\)\^\$UDINT' 'ReadRecoverableRecorderInventory 0x7E4D must preserve its exact 36-byte identity/token request and dormant Double-bank gate.'
Assert-Match $recorderRecoverableInventoryBlock '(?s)requestBootId = 0.*?requestConfigId = 0.*?expectedMapRevision = 0.*?requestRecoveryTokenNonzero = FALSE.*?requestBootId <> CurrentDiagnosticsBootId.*?expectedMapRevision <> LMC_RECORDER_MAP_REVISION.*?elsif ConfigId = 0 then' 'ReadRecoverableRecorderInventory must validate the nonzero requested identity/token and current BootId/map before considering canonical absence.'
Assert-Match $recorderRecoverableInventoryBlock '(?s)elsif ConfigId = 0 then.*?state <> LMC_RECORDER_EMPTY.*?MapRevision <> 0.*?OwnerSessionEpoch <> 0.*?ClosedSessionEpoch <> 0.*?DiagnosticsBootId <> 0.*?RecordId <> 0.*?BufferId <> 0.*?BufferReleased = FALSE.*?BufferMode <> 0.*?ChannelCount <> 0.*?SampleCapacity <> 0.*?SampleStrideBytes <> 0.*?storedRecoveryTokenNonzero.*?bankState <> LMC_RECORDER_EMPTY.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankMapRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankDiagnosticsBootId\[bankIndex\] <> 0.*?if inventoryValid then\s*detailCode := 32;\s*else\s*detailCode := 10' 'ReadRecoverableRecorderInventory canonical-absence proof must require a zero active token and the complete class/bank empty predicate before typed detail 32.'
$recoverableInventoryAbsenceCount = [regex]::Matches(
    $recorderRecoverableInventoryBlock,
    '(?m)^\s*detailCode := 32;').Count
if ($recoverableInventoryAbsenceCount -ne 1) {
    throw ('ReadRecoverableRecorderInventory typed canonical-absence detail count is ' +
        "$recoverableInventoryAbsenceCount, expected one.")
}
if ($recorderRecoverableInventoryBlock -match
    'g_LMCRecorderLastReleasedRecoveryToken') {
    throw ('ReadRecoverableRecorderInventory canonical-empty proof must ignore the ' +
        'per-boot last-released token tombstone.')
}
Assert-Match $recorderRecoverableInventoryBlock '(?s)requestBootId <> DiagnosticsBootId.*?expectedMapRevision <> MapRevision.*?requestConfigId <> ConfigId.*?ConfigRevision = 0.*?BufferMode <> 2.*?state < LMC_RECORDER_CONFIGURED.*?state > LMC_RECORDER_UPLOADING.*?storedRecoveryTokenNonzero = FALSE.*?requestRecoveryToken0 <> g_LMCRecorderRecoveryToken\[0\].*?requestRecoveryToken3 <> g_LMCRecorderRecoveryToken\[3\].*?OwnerSessionEpoch <> CallerSessionEpoch.*?ClosedSessionEpoch = 0' 'ReadRecoverableRecorderInventory does not fail closed for active configuration identity, mode/state, active token, or foreign-owner mismatch.'
Assert-Match $recorderRecoverableInventoryBlock '(?s)for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?g_LMCRecorderBankRecordId\[bankIndex\] = 0.*?bankState <> LMC_RECORDER_CONFIGURED.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?inventoryValid := FALSE.*?g_LMCRecorderBankConfigId\[bankIndex\] <> ConfigId.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <>\s*ConfigRevision.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <>\s*OwnerSessionEpoch.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <>\s*ClosedSessionEpoch.*?bankState < LMC_RECORDER_ARMED.*?bankState > LMC_RECORDER_UPLOADING.*?inventoryValid := FALSE' 'ReadRecoverableRecorderInventory does not validate every empty and occupied bank before publishing recovery metadata.'
Assert-Match $recorderRecoverableInventoryBlock '(?s)localRecordId := 0.*?localRecordId = 0 then.*?localRecordId :=\s*g_LMCRecorderBankRecordId\[bankIndex\].*?elsif localRecordId =\s*g_LMCRecorderBankRecordId\[bankIndex\] then\s*inventoryValid := FALSE.*?inventoryCount = 0.*?state <> LMC_RECORDER_CONFIGURED' 'ReadRecoverableRecorderInventory does not reject duplicate occupied RecordIds or inconsistent zero-bank active state.'
Assert-Match $recorderRecoverableInventoryBlock '(?s)ResponseCapacity < 104.*?\(pResponse \+ 16\)\^\$UDINT := DiagnosticsBootId.*?\(pResponse \+ 20\)\^\$UDINT := ConfigId.*?\(pResponse \+ 24\)\^\$UDINT := ConfigRevision.*?\(pResponse \+ 32\)\^\$UDINT := OwnerSessionEpoch.*?\(pResponse \+ 36\)\^\$UDINT := ClosedSessionEpoch.*?\(pResponse \+ 40\)\^\$UINT := TO_UINT\(state\).*?\(pResponse \+ 44\)\^\$USINT := TO_USINT\(inventoryCount\).*?\(pResponse \+ 88\)\^\$UDINT :=\s*g_LMCRecorderRecoveryToken\[0\].*?\(pResponse \+ 100\)\^\$UDINT :=\s*g_LMCRecorderRecoveryToken\[3\].*?ResponseSize := 104' 'ReadRecoverableRecorderInventory does not return the exact 104-byte inventory plus active-token echo.'
foreach ($persistentStateName in $recorderPersistentStateNames) {
    $persistentAssignmentPattern = (
        '(?m)(?<![A-Za-z0-9_])' +
        [regex]::Escape($persistentStateName) +
        $recorderDirectMutationSuffix
    )
    if ([regex]::IsMatch(
            $recorderRecoverableInventoryBlock,
            $persistentAssignmentPattern)) {
        throw (
            'ReadRecoverableRecorderInventory must remain read-only; found a write to ' +
            "persistent state '$persistentStateName'.")
    }
}
foreach ($storageMutationPattern in $recorderStorageMutationPatterns) {
    if ([regex]::IsMatch(
            $recorderRecoverableInventoryBlock,
            $storageMutationPattern)) {
        throw 'ReadRecoverableRecorderInventory must not mutate Recorder bank/data storage.'
    }
}
Assert-Match $recorderAdoptEmptyConfigurationBlock '(?s)RequestSize <> 28.*?CallerSessionEpoch = 0.*?LMC_RECORDER_DOUBLE_BANK_ENABLED = FALSE then\s*.*?detailCode := 2.*?requestBootId := \(pRequest \+ 8\)\^\$UDINT.*?requestConfigId := \(pRequest \+ 12\)\^\$UDINT.*?requestConfigRevision := \(pRequest \+ 16\)\^\$UDINT.*?expectedMapRevision := \(pRequest \+ 20\)\^\$UDINT.*?requestOwnerEpoch := \(pRequest \+ 24\)\^\$UDINT' 'AdoptEmptyRecorderConfiguration 0x7E4B must preserve its exact 28-byte identity request and dormant Double-bank gate.'
Assert-Match $recorderAdoptEmptyConfigurationBlock '(?s)CallerSessionEpoch = requestOwnerEpoch.*?requestBootId <> CurrentDiagnosticsBootId.*?requestBootId <> DiagnosticsBootId.*?expectedMapRevision <> LMC_RECORDER_MAP_REVISION.*?expectedMapRevision <> MapRevision.*?requestConfigId <> ConfigId.*?requestConfigRevision <> ConfigRevision.*?BufferMode <> 2.*?state <> LMC_RECORDER_CONFIGURED.*?RecordId <> 0.*?BufferId <> 0.*?BufferReleased = FALSE.*?OwnerSessionEpoch <> requestOwnerEpoch.*?ClosedSessionEpoch <> requestOwnerEpoch' 'AdoptEmptyRecorderConfiguration does not fail closed for caller, exact configuration identity, state, or previous-owner closure mismatches.'
Assert-Match $recorderAdoptEmptyConfigurationBlock '(?s)for bankIndex := 0 to LMC_RECORDER_BANK_COUNT - 1 do.*?bankState <> LMC_RECORDER_CONFIGURED.*?g_LMCRecorderBankRecordId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigId\[bankIndex\] <> 0.*?g_LMCRecorderBankConfigRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankMapRevision\[bankIndex\] <> 0.*?g_LMCRecorderBankOwnerSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankClosedSessionEpoch\[bankIndex\] <> 0.*?g_LMCRecorderBankDiagnosticsBootId\[bankIndex\] <> 0.*?inventoryValid := FALSE' 'AdoptEmptyRecorderConfiguration does not require both physical banks to be Configured with zero bank identity fields.'
Assert-Match $recorderAdoptEmptyConfigurationBlock '(?s)OwnerSessionEpoch := CallerSessionEpoch;\s*ClosedSessionEpoch := 0;.*?\(pResponse \+ 16\)\^\$UDINT := DiagnosticsBootId.*?\(pResponse \+ 20\)\^\$UDINT := ConfigId.*?\(pResponse \+ 24\)\^\$UDINT := ConfigRevision.*?\(pResponse \+ 28\)\^\$UDINT := MapRevision.*?\(pResponse \+ 32\)\^\$UDINT := CallerSessionEpoch.*?\(pResponse \+ 36\)\^\$UINT :=\s*LMC_RECORDER_CONFIGURED.*?\(pResponse \+ 38\)\^\$USINT := 2.*?\(pResponse \+ 39\)\^\$USINT :=\s*TO_USINT\(LMC_RECORDER_BANK_COUNT\).*?ResponseSize := 40' 'AdoptEmptyRecorderConfiguration does not rebind only the configuration owner and return exact Configured/Double/count-2 metadata.'
$emptyAdoptAllowedStateWrites = @(
    'OwnerSessionEpoch',
    'ClosedSessionEpoch'
)
foreach ($persistentStateName in $recorderPersistentStateNames) {
    if ($emptyAdoptAllowedStateWrites -contains $persistentStateName) {
        continue
    }

    $persistentAssignmentPattern = (
        '(?m)(?<![A-Za-z0-9_])' +
        [regex]::Escape($persistentStateName) +
        $recorderDirectMutationSuffix
    )
    if ([regex]::IsMatch(
            $recorderAdoptEmptyConfigurationBlock,
            $persistentAssignmentPattern)) {
        throw (
            'AdoptEmptyRecorderConfiguration may only rebind the shared owner; ' +
            "found a write to persistent state '$persistentStateName'.")
    }
}
foreach ($storageMutationPattern in $recorderStorageMutationPatterns) {
    if ([regex]::IsMatch(
            $recorderAdoptEmptyConfigurationBlock,
            $storageMutationPattern)) {
        throw 'AdoptEmptyRecorderConfiguration must not mutate Recorder bank/data storage.'
    }
}

$emptyAdoptOwnerWrites = [regex]::Matches(
    $recorderAdoptEmptyConfigurationBlock,
    ('(?m)(?<![A-Za-z0-9_])OwnerSessionEpoch' +
        '(?<TargetSuffix>(?:\s*\[[^\]\r\n]+\])?' +
        '(?:\$[A-Za-z_][A-Za-z0-9_]*)?)\s*' +
        '(?<Operator>:=|\+=|-=|\*=|/=|&=|\|=|\^=|\+\+|--)\s*' +
        '(?<Value>[^;]*);'))
if (($emptyAdoptOwnerWrites.Count -ne 1) -or
    ($emptyAdoptOwnerWrites[0].Groups['TargetSuffix'].Value.Trim().Length -ne 0) -or
    ($emptyAdoptOwnerWrites[0].Groups['Operator'].Value -cne ':=') -or
    ($emptyAdoptOwnerWrites[0].Groups['Value'].Value.Trim() -cne
        'CallerSessionEpoch')) {
    throw 'AdoptEmptyRecorderConfiguration must write OwnerSessionEpoch exactly once from CallerSessionEpoch.'
}
$emptyAdoptClosedWrites = [regex]::Matches(
    $recorderAdoptEmptyConfigurationBlock,
    ('(?m)(?<![A-Za-z0-9_])ClosedSessionEpoch' +
        '(?<TargetSuffix>(?:\s*\[[^\]\r\n]+\])?' +
        '(?:\$[A-Za-z_][A-Za-z0-9_]*)?)\s*' +
        '(?<Operator>:=|\+=|-=|\*=|/=|&=|\|=|\^=|\+\+|--)\s*' +
        '(?<Value>[^;]*);'))
if (($emptyAdoptClosedWrites.Count -ne 1) -or
    ($emptyAdoptClosedWrites[0].Groups['TargetSuffix'].Value.Trim().Length -ne 0) -or
    ($emptyAdoptClosedWrites[0].Groups['Operator'].Value -cne ':=') -or
    ($emptyAdoptClosedWrites[0].Groups['Value'].Value.Trim() -cne '0')) {
    throw 'AdoptEmptyRecorderConfiguration must clear ClosedSessionEpoch exactly once.'
}

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
    ReadRecorderBankInventory = '0x7E4A'
    AdoptEmptyRecorderConfiguration = '0x7E4B'
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
Assert-Match $protocol 'internal const ushort GetAdminCapabilities = 0x7D00;' 'C# admin capability command ID is missing.'
Assert-Match $protocol 'internal const ushort ReadAxisParameter = 0x7D10;' 'C# axis parameter command ID is missing.'
Assert-Match $protocol 'internal const ushort ReadGroupParameters = 0x7D20;' 'C# group parameter command ID is missing.'
Assert-Match $protocol 'internal const ushort GroupMoveLinearRelative = 0x7D22;' 'C# group relative-move command ID is missing.'
Assert-Match $adminProtocol '(?s)GetCapabilities\(uint requestId\).*?CreateCommonRequest\(\s*LMC_CommandId\.GetAdminCapabilities,\s*0,\s*CommonRequestPayloadLength,\s*requestId\)' 'C# 0x7D00 request builder is incomplete.'
Assert-Match $adminProtocol '(?s)ReadAxisParameter\(.*?CreateCommonRequest\(\s*LMC_CommandId\.ReadAxisParameter,\s*axisReference,\s*ReadParameterRequestPayloadLength,\s*requestId\).*?CommonRequestPayloadLength,\s*\(ushort\)key' 'C# 0x7D10 request builder is incomplete.'
Assert-Match $adminProtocol '(?s)ReadGroupParameters\(.*?CreateCommonRequest\(\s*LMC_CommandId\.ReadGroupParameters,\s*groupReference,\s*ReadParameterRequestPayloadLength,\s*requestId\).*?CommonRequestPayloadLength,\s*\(uint\)selection' 'C# 0x7D20 request builder is incomplete.'
Assert-Match $adminProtocol 'GroupMoveLinearRelativeRequestPayloadLength = 104;' 'C# 0x7D22 request payload length is not 104 bytes.'
$adminGroupMoveRelativeFrameBlock = [regex]::Match(
    $adminProtocol,
    '(?s)internal static byte\[\] GroupMoveLinearRelative\(.*?internal static void ValidateGroupLinearRelative').Value
if ([string]::IsNullOrWhiteSpace($adminGroupMoveRelativeFrameBlock)) {
    throw 'C# 0x7D22 request builder was not found.'
}
Assert-Match $adminGroupMoveRelativeFrameBlock '(?s)CreateCommonRequest\(\s*LMC_CommandId\.GroupMoveLinearRelative,\s*groupReference,\s*GroupMoveLinearRelativeRequestPayloadLength,\s*requestId\).*?motionOffset = LMC_Frame\.HeaderSize\s*\+ CommonRequestPayloadLength.*?WriteGroupLinearVector\(\s*buffer,\s*motionOffset,\s*distance\)' 'C# 0x7D22 common envelope or 16-slot distance vector is incomplete.'
Assert-Match $adminGroupMoveRelativeFrameBlock '(?s)motionOffset \+ 64, velocity.*?motionOffset \+ 68, acceleration.*?motionOffset \+ 72, deceleration.*?motionOffset \+ 76, jerk.*?motionOffset \+ 80,\s*\(int\)options\.CoordinateSystem.*?motionOffset \+ 84,\s*\(int\)options\.TransitionMode.*?motionOffset \+ 88,\s*\(int\)options\.BufferMode.*?motionOffset \+ 92,\s*options\.Execute \? 1 : 0' 'C# 0x7D22 motion field offsets are incomplete.'
Assert-Match $diagnosticsProtocol '(?s)GetDiagnosticsCapabilities\(uint requestId\).*?CreateRequest\(\s*LMC_CommandId\.GetDiagnosticsCapabilities,\s*0,\s*CommonRequestPayloadLength\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize, SchemaVersion\).*?WriteUInt16\(buffer, LMC_Frame\.HeaderSize \+ 2, 0\).*?WriteUInt32\(buffer, LMC_Frame\.HeaderSize \+ 4, requestId\)' 'C# diagnostics capability common request builder is incomplete.'

if (-not $controlServiceAllControlRouted) {
$axisLookupBlock = [regex]::Match(
    $registryHandlerBlock,
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
    $registryHandlerBlock,
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

$powerCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2023:.*?0x2024:').Value
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

$resetCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2024:.*?0x2022:').Value
Assert-Match $resetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef >= 1\).*?\(AxisRef <= 9\).*?AxisReset\(\);' '0x2024 exact reset validation/dispatch is missing.'
$axisResetBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::AxisReset.*?END_FUNCTION').Value
if ([regex]::Matches($axisResetBlock, 'IsClientConnected\(#LMCAxis[1-9]\)').Count -ne 9 -or
    [regex]::Matches($axisResetBlock, '\bLMCAxis[1-9]\.QuitError\s*\(').Count -ne 9) {
    throw 'AxisReset does not validate and dispatch all nine LASAL axis clients.'
}

$stopCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x2022:.*?0x2028:').Value
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
    $axisHandlerBlock,
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
    $axisHandlerBlock,
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

$moveShortestCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x209F:.*?0x20A0:').Value
$moveRelativeCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x20A0:.*?0x20A2:').Value
$moveVelocityCaseBlock = [regex]::Match($axisHandlerBlock, '(?s)0x20A2:.*?end_case;').Value
foreach ($entry in @(
    @{ Name = '0x209F'; Block = $moveShortestCaseBlock },
    @{ Name = '0x20A0'; Block = $moveRelativeCaseBlock })) {
    Assert-Match $entry.Block '(?s)if Payload = 32 then.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' "$($entry.Name) exact payload and shortest-only validation is missing."
    Assert-Match $entry.Block '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[24\],\s*size:=4\);' "$($entry.Name) does not read Jerk from request offset 24."
}
Assert-Match $moveVelocityCaseBlock '(?s)if Payload = 24 then.*?\(dec = 0\).*?\(Exec = 1\).*?\(dir = 1\).*?\(velo >= 0\).*?\(dir = 3\).*?\(velo <= 0\).*?else\s*AxisRef := 0;.*?MoveAbs\(\);' '0x20A2 exact payload, direction, and execute validation is missing.'
Assert-Match $moveVelocityCaseBlock '_StdLib\.MemCpy\(dest:=#jer,\s*source:=#RequestBuf\[20\],\s*size:=4\);' '0x20A2 does not read Jerk from request offset 20.'

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
}

if (-not $controlServiceGroupRouted) {
$groupMembersCaseBlock = [regex]::Match(
    $groupHandlerBlock,
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

$groupEnableCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2047:.*?0x2048:').Value
$groupDisableCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2048:.*?0x2049:').Value
Assert-Match $groupEnableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?GroupReadErrorId := -6;.*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?LMCRobot\.LockProfile\(.*?Axis1:=1.*?Axis4:=1.*?Axis5:=0.*?Axis9:=0.*?GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?elsif GroupReadRetCode\$UDINT <= 32767 then.*?GroupReadErrorId := GroupReadRetCode\$DINT;.*?udSize:=16' '0x2047 preconditions, four-axis profile-lock dispatch, acceptance mapping, native error preservation, or ACK is missing.'
if ($groupEnableCaseBlock -match 'ReadProfileParameter|_LMCPROF_LockState') {
    throw '0x2047 still treats the same-CyWork LockState read as command completion.'
}
Assert-Match $groupDisableCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?GroupReadInPosition <> 0.*?GroupReadRetCode := LMCRobot\.UnlockProfile\(\).*?GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?elsif GroupReadRetCode\$UDINT <= 32767 then.*?GroupReadErrorId := GroupReadRetCode\$DINT;.*?udSize:=16' '0x2048 group profile-unlock standstill validation/dispatch, acceptance mapping, native error preservation, or ACK is missing.'
if ($groupDisableCaseBlock -match 'ReadProfileParameter|_LMCPROF_LockState') {
    throw '0x2048 still treats the same-CyWork LockState read as command completion instead of polling 0x2045.'
}

$groupPowerOnCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x204A:.*?0x204B:').Value
$groupPowerOffCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x204B:.*?0x2085:').Value
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

$groupStatusCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2045:.*?0x2051:').Value
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

$groupResetCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2049:.*?0x204A:').Value
Assert-Match $groupResetCaseBlock '(?s)\(Payload = 1\).*?\(RequestBuf\[8\] = 1\).*?\(AxisRef = 0x0100\).*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.AxQuitError\(AxisNo:=0\).*?AxisCommandStatus := 0;.*?AxisCommandErrorId := 0;.*?udSize:=16' '0x2049 axis-error reset validation/AxQuitError dispatch/ACK is missing.'

$groupStopCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2085:.*?0x20A4:').Value
Assert-Match $groupStopCaseBlock '(?s)if Payload = 16 then.*?RequestBuf\[8\].*?RequestBuf\[12\].*?\(bufMode = 1\).*?\(GroupExecute = 1\).*?\(GroupDecel >= 0\).*?\(GroupJerk >= 0\).*?\(\(GroupJerk = 0\) \| \(GroupDecel > 0\)\).*?GroupStopCommandNo\s*:=\s*LMCRobot\.StopMove\(\s*Mode:=3, Decel:=GroupDecel, Jerk:=GroupJerk\).*?GroupReadErrorId\s*:=\s*0;.*?udSize:=16' '0x2085 group stop validation/StopMove dispatch/ACK is missing.'
$groupStopCommandNoUseCount = [regex]::Matches(
    $groupStopCaseBlock,
    '\bGroupStopCommandNo\b').Count
if ($groupStopCommandNoUseCount -ne 2) {
    throw '0x2085 incorrectly treats StopMove StopCmdNo as an error or acceptance code.'
}

$groupMoveCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x20A4:.*?0x2045:').Value
Assert-Match $groupMoveCaseBlock '(?s)\(Payload = 96\).*?\(AxisRef = 0x0100\).*?source:=#RequestBuf\[72\].*?source:=#RequestBuf\[76\].*?source:=#RequestBuf\[80\].*?source:=#RequestBuf\[84\].*?source:=#RequestBuf\[88\].*?source:=#RequestBuf\[92\].*?source:=#RequestBuf\[96\].*?source:=#RequestBuf\[100\]' '0x20A4 DINT field offsets are incomplete.'
Assert-Match $groupMoveCaseBlock '(?s)for kinIndex := 4 to 15 do.*?GroupCommandInputValid := FALSE.*?end_for' '0x20A4 does not reject nonzero positions outside the four-axis topology.'
Assert-Match $groupMoveCaseBlock '(?s)\(GroupCoordSystem = 0\).*?\(GroupTransitionModeInput = 0\).*?\(GroupTransitionModeInput = 2\).*?\(bufMode = 1\).*?\(bufMode = 2\).*?MoveLinearAbsEx\(\);' '0x20A4 approved coordinate/transition/buffer validation is missing.'
$groupMoveBlock = [regex]::Match($st, '(?s)FUNCTION TCPMotionInterface::MoveLinearAbsEx.*?END_FUNCTION').Value
Assert-Match $groupMoveBlock '(?s)GroupCommandInputValid = TRUE.*?IsClientConnected\(#LMCRobot\).*?LMCRobot\.RobotIsOn\(\).*?ReadProfileParameter\(.*?_LMCPROF_LockState.*?GroupKinematicReady = TRUE.*?powerIsOn <> 0.*?profileLocked = TRUE.*?LMCRobot\.MoveLinearCoord\(.*?CmdConfig:=GroupCommandConfig.*?CoordSystem:=0.*?Jerk:=GroupJerk.*?udSize:=16' 'MoveLinearAbsEx does not gate and dispatch the validated configured/powered/locked command.'
Assert-Match $groupMoveBlock '(?s)GroupMoveRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[12\]\$UINT := 0;.*?else.*?Sendbuf\[12\]\$UINT := 1;' 'MoveLinearAbsEx does not gate success on the MotionLib return code.'

$groupPositionCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x2051:.*?0x20E7:').Value
Assert-Match $groupPositionCaseBlock '(?s)GroupCoordSystem := -1;.*?GroupReadErrorId := -3;.*?\(Payload = 8\).*?\(AxisRef = 0x0100\).*?\(GroupExecute = 1\).*?if \(GroupCoordSystem = 0\) \| \(GroupCoordSystem = 1\) then.*?LMCRobot\.GetRobotPosition\(.*?Mode:=_ACTPOS_APPUNITS.*?CoordSystem:=0.*?pPositions:=#GroupReadPos.*?elsif \(GroupCoordSystem = 2\) \| \(GroupCoordSystem = 3\) then.*?GroupReadErrorId := -7.*?end_if;.*?end_if;' '0x2051 None/ACS member-slot mapping, MCS/PCS rejection, or unknown-enum -3 default is missing.'
Assert-Match $groupPositionCaseBlock '(?s)GroupReadRetCode = _LMCPROF_NoError then.*?GroupReadErrorId := 0;.*?if GroupReadErrorId = 0 then.*?Sendbuf\[2\]\$UINT\s*:=\s*68;.*?else.*?Sendbuf\[2\]\$UINT\s*:=\s*4;' '0x2051 does not gate the typed success payload on the MotionLib return code.'
Assert-Match $groupPositionCaseBlock '(?s)_memset\(dest:=#Sendbuf, usByte:=0, cntr:=sizeof\(Sendbuf\)\);.*?Sendbuf\[2\]\$UINT\s*:=\s*68;.*?MemCpy\(dest:=#Sendbuf\[8\], source:=#GroupReadPos, size:=36\).*?Sendbuf\[72\]\$UINT\s*:=\s*0x4000;.*?udSize:=76' '0x2051 68-byte DINT position response or zero-tail initialization is missing.'

$kinCaseBlock = [regex]::Match($groupHandlerBlock, '(?s)0x20E7:.*?end_case;').Value
Assert-Match $kinCaseBlock '(?s)kinValid := \(Payload = 1320\).*?for kinIndex := 0 to 3 do.*?0x3FF00000.*?RequestBuf\[648\]\$DINT <> 4.*?RequestBuf\[1316\]\$DINT <> 2.*?RequestBuf\[1320\]\$DINT <> 1' '0x20E7 identity-shift Cartesian4 payload validation is missing.'
Assert-Match $kinCaseBlock '(?s)IsClientConnected\(#LMCRobot\).*?IsClientConnected\(#LMCAxis1\).*?IsClientConnected\(#LMCAxis2\).*?IsClientConnected\(#LMCAxis3\).*?IsClientConnected\(#LMCAxis4\).*?GroupKinematicReady := TRUE;.*?GroupReadErrorId := 0;' '0x20E7 static four-axis mapping registration is missing.'
if ($kinCaseBlock -match 'LockProfile|UnlockProfile|RobotOn|RobotOff') {
    throw '0x20E7 mapping validation still changes profile-lock or group-power state.'
}
Assert-Match $kinCaseBlock '(?s)if GroupReadErrorId = 0 then.*?Sendbuf\[8\]\$UINT := 0;.*?else.*?Sendbuf\[8\]\$UINT := 1;' '0x20E7 does not gate acknowledgement success on mapping validation.'
Assert-Match $kinCaseBlock '(?s)Sendbuf\[2\]\$UINT\s*:=\s*4;.*?Sendbuf\[8\]\$UINT.*?Sendbuf\[10\]\$INT.*?udSize:=12' '0x20E7 short acknowledgement framing is missing.'
}

if ($ControlServiceCheckpoint -ne 'Phase2Skeleton') {
    $serviceGroupHandlerBlock =
        $controlServicePrivateBlocks['HandleGroupCommands']
    $serviceAdminHandlerBlock =
        $controlServicePrivateBlocks['HandleAdminCommands']
    $serviceMoveLinearBlock =
        $controlServicePrivateBlocks['MoveLinearAbsEx']
    $serviceGroupReadStatusBlock =
        $controlServicePrivateBlocks['GroupReadStatus']

    $serviceGroupMembersCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20D2:.*?0x2047:').Value
    $serviceGroupEnableCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2047:.*?0x2048:').Value
    $serviceGroupDisableCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2048:.*?0x2049:').Value
    $serviceGroupResetCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2049:.*?0x204A:').Value
    $serviceGroupPowerOnCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x204A:.*?0x204B:').Value
    $serviceGroupPowerOffCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x204B:.*?0x2085:').Value
    $serviceGroupStopCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2085:.*?0x20A4:').Value
    $serviceGroupMoveCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20A4:.*?0x2045:').Value
    $serviceGroupStatusCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2045:.*?0x2051:').Value
    $serviceGroupPositionCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x2051:.*?0x20E7:').Value
    $serviceKinematicCaseBlock = [regex]::Match(
        $serviceGroupHandlerBlock,
        '(?s)0x20E7:.*?end_case;').Value
    $serviceAdminGroupParametersCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D20:.*?0x7D22:').Value
    $serviceAdminRelativeMoveCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D22:.*(?=\s+else\s+ResponseSize\s*:=\s*-1\s*;\s*end_case;)').Value

    $serviceSemanticBlocks = [ordered]@{
        '0x20D2' = $serviceGroupMembersCaseBlock
        '0x2047' = $serviceGroupEnableCaseBlock
        '0x2048' = $serviceGroupDisableCaseBlock
        '0x2049' = $serviceGroupResetCaseBlock
        '0x204A' = $serviceGroupPowerOnCaseBlock
        '0x204B' = $serviceGroupPowerOffCaseBlock
        '0x2085' = $serviceGroupStopCaseBlock
        '0x20A4' = $serviceGroupMoveCaseBlock
        '0x2045' = $serviceGroupStatusCaseBlock
        '0x2051' = $serviceGroupPositionCaseBlock
        '0x20E7' = $serviceKinematicCaseBlock
        '0x7D20' = $serviceAdminGroupParametersCaseBlock
        '0x7D22' = $serviceAdminRelativeMoveCaseBlock
    }
    foreach ($semanticEntry in $serviceSemanticBlocks.GetEnumerator()) {
        if ([string]::IsNullOrWhiteSpace($semanticEntry.Value)) {
            throw (
                'LMCControlCommandService semantic block ' +
                "$($semanticEntry.Key) was not found.")
        }
    }

    $fourAxisServiceClients = @(
        'LMCRobot',
        'LMCAxis1',
        'LMCAxis2',
        'LMCAxis3',
        'LMCAxis4')
    foreach ($clientGate in @(
            @{ Owner = 'Service 0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Owner = 'Service 0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Owner = 'Service MoveLinearAbsEx'; Block = $serviceMoveLinearBlock },
            @{ Owner = 'Service 0x20E7'; Block = $serviceKinematicCaseBlock },
            @{ Owner = 'Service 0x7D22'; Block = $serviceAdminRelativeMoveCaseBlock })) {
        Assert-ExactLasalConnectedClientSet `
            -Text $clientGate.Block `
            -Owner $clientGate.Owner `
            -ExpectedClients $fourAxisServiceClients
        Assert-Match $clientGate.Block (
            '(?s)if\s+\(IsClientConnected\(#LMCRobot\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis1\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis2\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis3\)\s*=\s*1\)\s*&\s*' +
            '\(IsClientConnected\(#LMCAxis4\)\s*=\s*1\)\s+then') (
            "$($clientGate.Owner) must conjunct all five exact client gates.")
    }

    Assert-Match $serviceGroupEnableCaseBlock (
        'if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s+then') (
        'Service 0x2047 must conjunct kinematic readiness and group power.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLocked\s*=\s*TRUE\)\s+then') (
        'Service MoveLinearAbsEx must conjunct kinematic, power, and lock readiness.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLockState\s*<>\s*0\)\s+then') (
        'Service 0x7D22 must conjunct kinematic, power, and lock readiness.')

    $serviceFrameContracts = @(
        @{ Owner = '0x20D2'; Block = $serviceGroupMembersCaseBlock;
            Sizes = @('12', '1358'); Outer = @('0', '1') },
        @{ Owner = '0x2047'; Block = $serviceGroupEnableCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2048'; Block = $serviceGroupDisableCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2049'; Block = $serviceGroupResetCaseBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = '0x204A'; Block = $serviceGroupPowerOnCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x204B'; Block = $serviceGroupPowerOffCaseBlock;
            Sizes = @('12', '16'); Outer = @('0', '1') },
        @{ Owner = '0x2085'; Block = $serviceGroupStopCaseBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = '0x2045'; Block = $serviceGroupStatusCaseBlock;
            Sizes = @('12'); Outer = @('1') },
        @{ Owner = '0x2051'; Block = $serviceGroupPositionCaseBlock;
            Sizes = @('12', '76'); Outer = @('0') },
        @{ Owner = '0x20E7'; Block = $serviceKinematicCaseBlock;
            Sizes = @('12'); Outer = @('0') },
        @{ Owner = '0x7D20'; Block = $serviceAdminGroupParametersCaseBlock;
            Sizes = @('24', '40'); Outer = @('0') },
        @{ Owner = '0x7D22'; Block = $serviceAdminRelativeMoveCaseBlock;
            Sizes = @('24'); Outer = @('0') },
        @{ Owner = 'MoveLinearAbsEx'; Block = $serviceMoveLinearBlock;
            Sizes = @('16'); Outer = @('0') },
        @{ Owner = 'GroupReadStatus'; Block = $serviceGroupReadStatusBlock;
            Sizes = @('20'); Outer = @('0') })
    foreach ($frameContract in $serviceFrameContracts) {
        Assert-ExactRegexValueSet `
            -Text $frameContract.Block `
            -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
            -Owner "LMCControlCommandService $($frameContract.Owner) response sizes" `
            -ExpectedValues $frameContract.Sizes
        Assert-ExactRegexValueSet `
            -Text $frameContract.Block `
            -Pattern 'pResponseFrame\^\$UINT\s*:=\s*(?<Value>[0-9]+)\s*;' `
            -Owner "LMCControlCommandService $($frameContract.Owner) outer statuses" `
            -ExpectedValues $frameContract.Outer
    }

    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)objectRegistryReady\s*:=\s*FALSE.*?' +
        'if\s+RequestFrameSize\s*=\s*9\s+then\s*' +
        'objectRegistryReady\s*:=\s*' +
        '\(\(pRequestFrame\s*\+\s*8\)\^\$USINT\s*=\s*1\).*?' +
        'Reference\s*=\s*0x0100.*?;\s*end_if;\s*' +
        'if\s+objectRegistryReady\s*=\s*TRUE\s+then.*?' +
        'ResponseCapacity\s*<\s*1358') (
        'Service 0x20D2 exact request envelope or response capacity is missing.')
    Assert-ExactLasalConnectedClientSet `
        -Text $serviceGroupMembersCaseBlock `
        -Owner 'Service 0x20D2 registry gate' `
        -ExpectedClients @(
            'LMCRobot',
            'LMCAxis1',
            'LMCAxis2',
            'LMCAxis3',
            'LMCAxis4',
            'LMCAxis5',
            'LMCAxis6',
            'LMCAxis7',
            'LMCAxis8',
            'LMCAxis9')
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '_GetObjName\(\s*pThis:=(?:LMCAxis[1-9]|LMCRobot)\.pCmd').Count -ne 10) {
        throw 'Service 0x20D2 must refresh exactly nine axis names and one robot name.'
    }
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '_memset\(dest:=#objectName\[0\]').Count -ne 10) {
        throw 'Service 0x20D2 must clear its shared object-name scratch before every lookup.'
    }
    if ([regex]::Matches(
            $serviceGroupMembersCaseBlock,
            '(?s)_memcpy\(ptr1:=pResponseFrame\s*\+\s*\d+,\s*' +
            'ptr2:=#objectName\[0\],\s*cntr:=80\)').Count -ne 9) {
        throw 'Service 0x20D2 must copy exactly the nine axis names into the wire response.'
    }
    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)objectNameLength\s*=\s*0.*?objectNameLength\s*>\s*79.*?' +
        'objectRegistryReady\s*:=\s*FALSE') (
        'Service 0x20D2 empty/overlength object-name rejection is missing.')
    $serviceRobotNameTail = [regex]::Match(
        $serviceGroupMembersCaseBlock,
        '(?s)_GetObjName\(\s*pThis:=LMCRobot\.pCmd.*?(?=\s*end_if;\s*\r?\n\s*if objectRegistryReady)').Value
    if ([string]::IsNullOrWhiteSpace($serviceRobotNameTail) -or
        $serviceRobotNameTail -match '_memcpy\(') {
        throw ('Service 0x20D2 must validate the robot object name without ' +
            'publishing it as a member-axis name.')
    }
    foreach ($entry in @(
            @{ Axis = 1; Offset = 76 },
            @{ Axis = 2; Offset = 156 },
            @{ Axis = 3; Offset = 236 },
            @{ Axis = 4; Offset = 316 },
            @{ Axis = 5; Offset = 396 },
            @{ Axis = 6; Offset = 476 },
            @{ Axis = 7; Offset = 556 },
            @{ Axis = 8; Offset = 636 },
            @{ Axis = 9; Offset = 716 })) {
        Assert-Match $serviceGroupMembersCaseBlock (
            '(?s)pThis:=LMCAxis' + $entry.Axis + '\.pCmd.*?' +
            '_memcpy\(ptr1:=pResponseFrame\s*\+\s*' + $entry.Offset +
            ',\s*ptr2:=#objectName\[0\],\s*cntr:=80\)') (
            "Service 0x20D2 axis $($entry.Axis) name slot is missing.")
    }
    foreach ($entry in @(
            @{ Offset = 8; Value = 1 },
            @{ Offset = 10; Value = 2 },
            @{ Offset = 12; Value = 3 },
            @{ Offset = 14; Value = 4 },
            @{ Offset = 16; Value = 5 },
            @{ Offset = 18; Value = 6 },
            @{ Offset = 20; Value = 7 },
            @{ Offset = 22; Value = 8 },
            @{ Offset = 24; Value = 9 },
            @{ Offset = 40; Value = 0 },
            @{ Offset = 42; Value = 1 },
            @{ Offset = 44; Value = 2 },
            @{ Offset = 46; Value = 3 },
            @{ Offset = 48; Value = 4 },
            @{ Offset = 50; Value = 5 },
            @{ Offset = 52; Value = 6 },
            @{ Offset = 54; Value = 7 },
            @{ Offset = 56; Value = 8 })) {
        Assert-Match $serviceGroupMembersCaseBlock (
            '\(pResponseFrame\s*\+\s*' + $entry.Offset +
            '\)\^\$UINT\s*:=\s*' + $entry.Value + '\s*;') (
            "Service 0x20D2 slot $($entry.Offset) value is missing.")
    }
    Assert-Match $serviceGroupMembersCaseBlock (
        '(?s)pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*1350.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*1356\)\^\$USINT\s*:=\s*9') (
        'Service 0x20D2 opaque outer reference, payload length, or AxisCount is missing.')
    if ($serviceGroupMembersCaseBlock -match
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=.*?Reference') {
        throw 'Service 0x20D2 must keep the outer reference opaque zero.'
    }

    foreach ($singleByteCommand in @(
            @{ Name = '0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Name = '0x2048'; Block = $serviceGroupDisableCaseBlock },
            @{ Name = '0x2049'; Block = $serviceGroupResetCaseBlock },
            @{ Name = '0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Name = '0x204B'; Block = $serviceGroupPowerOffCaseBlock })) {
        Assert-Match $singleByteCommand.Block (
            '(?s)groupCommandInputValid\s*:=\s*FALSE.*?' +
            'if\s+RequestFrameSize\s*=\s*9\s+then\s*' +
            'groupCommandInputValid\s*:=\s*' +
            '\(\(pRequestFrame\s*\+\s*8\)\^\$USINT\s*=\s*1\).*?' +
            'Reference\s*=\s*0x0100.*?;\s*end_if;\s*' +
            'if\s+groupCommandInputValid\s*=\s*TRUE\s+then') (
            "Service $($singleByteCommand.Name) exact request envelope is missing.")
        if ($singleByteCommand.Block -match (
            '(?s)if\b(?:(?!\bthen\b).)*' +
            'RequestFrameSize\s*=\s*9(?:(?!\bthen\b).)*' +
            'pRequestFrame(?:(?!\bthen\b).)*\bthen\b')) {
            throw (
                "Service $($singleByteCommand.Name) dereferences byte 8 " +
                'inside the size-test expression instead of after its nested gate.')
        }
    }
    if ($serviceGroupMembersCaseBlock -match (
        '(?s)if\b(?:(?!\bthen\b).)*' +
        'RequestFrameSize\s*=\s*9(?:(?!\bthen\b).)*' +
        'pRequestFrame(?:(?!\bthen\b).)*\bthen\b')) {
        throw ('Service 0x20D2 dereferences byte 8 inside the size-test ' +
            'expression instead of after its nested gate.')
    }
    Assert-Match $serviceGroupEnableCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'LMCRobot\.RobotIsOn\(\).*?GroupKinematicReady\s*=\s*TRUE.*?' +
        'LMCRobot\.LockProfile\(.*?Axis1:=1.*?Axis4:=1.*?' +
        'Axis5:=0.*?Axis9:=0.*?groupReadRetCode\s*=\s*_LMCPROF_NoError') (
        'Service 0x2047 configured/powered four-axis LockProfile dispatch is missing.')
    Assert-Match $serviceGroupEnableCaseBlock (
        '(?s)LMCRobot\.LockProfile\(\s*' +
        'Axis1:=1\s*,\s*Axis2:=1\s*,\s*Axis3:=1\s*,\s*Axis4:=1\s*,\s*' +
        'Axis5:=0\s*,\s*Axis6:=0\s*,\s*Axis7:=0\s*,\s*Axis8:=0\s*,\s*' +
        'Axis9:=0\s*\)') (
        'Service 0x2047 LockProfile must enable exactly Axis1..4 and ' +
        'disable Axis5..9.')
    if ($serviceGroupEnableCaseBlock -match
        'ReadProfileParameter|_LMCPROF_LockState') {
        throw 'Service 0x2047 must not treat the same-call LockState as completion.'
    }
    Assert-Match $serviceGroupDisableCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?' +
        'groupReadInPosition\s*<>\s*0.*?' +
        'groupReadRetCode\s*:=\s*LMCRobot\.UnlockProfile\(\).*?' +
        'groupReadRetCode\s*=\s*_LMCPROF_NoError\s+then\s*' +
        'groupReadErrorId\s*:=\s*0;.*?' +
        'elsif\s+groupReadRetCode\$UDINT\s*<=\s*32767\s+then\s*' +
        'groupReadErrorId\s*:=\s*groupReadRetCode\$DINT') (
        'Service 0x2048 standstill-gated profile unlock acceptance mapping is missing.')
    if ($serviceGroupDisableCaseBlock -match
        'ReadProfileParameter|_LMCPROF_LockState') {
        throw (
            'Service 0x2048 must not treat the same-call LockState as completion; ' +
            'completion is proven by 0x2045 polling.')
    }
    Assert-Match $serviceGroupResetCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.AxQuitError\(AxisNo:=0\).*?' +
        'axisCommandStatus\s*:=\s*0.*?axisCommandErrorId\s*:=\s*0') (
        'Service 0x2049 group-axis error reset dispatch is missing.')
    Assert-Match $serviceGroupResetCaseBlock (
        '(?s)ResponseCapacity\s*<\s*16.*?' +
        'axisCommandStatus\s*:=\s*1;.*?' +
        'axisCommandErrorId\s*:=\s*-3;.*?' +
        'if\s+groupCommandInputValid\s*=\s*TRUE\s+then\s*' +
        'axisCommandErrorId\s*:=\s*-2;\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'axisCommandStatus\s*:=\s*0;.*?' +
        'axisCommandErrorId\s*:=\s*0;.*?end_if;\s*end_if;\s*' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\);.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*' +
        'axisCommandStatus;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
        'axisCommandErrorId;.*?ResponseSize\s*:=\s*16') (
        'Service 0x2049 must always return the 16-byte typed ACK with ' +
        'malformed -3, disconnected -2, and accepted zero status semantics.')
    Assert-Match $serviceGroupPowerOnCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'LMCRobot\.RobotOn\(Mode:=_ACTIVE\)') (
        'Service 0x204A four-axis RobotOn dispatch is missing.')
    if ($serviceGroupPowerOnCaseBlock -match
        'GroupKinematicReady\s*=\s*TRUE') {
        throw 'Service 0x204A must not gate power-on on kinematic readiness.'
    }
    Assert-Match $serviceGroupPowerOffCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'LMCRobot\.RobotOff\(\)') (
        'Service 0x204B RobotOff dispatch is missing.')
    foreach ($signedAck in @(
            @{ Name = '0x2047'; Block = $serviceGroupEnableCaseBlock },
            @{ Name = '0x2048'; Block = $serviceGroupDisableCaseBlock },
            @{ Name = '0x204A'; Block = $serviceGroupPowerOnCaseBlock },
            @{ Name = '0x204B'; Block = $serviceGroupPowerOffCaseBlock })) {
        Assert-Match $signedAck.Block (
            '(?s)groupReadErrorId\s*:=\s*-2;\s*' +
            'if\s+\(?IsClientConnected\(#LMCRobot\).*?' +
            'end_if;\s*' +
            '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
            'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
            '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
            'TO_UDINT\(Reference\);.*?' +
            'if\s+groupReadErrorId\s*=\s*0\s+then.*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*0;.*?' +
            'elsif\s+\(groupReadErrorId\s*>=\s*-32768\)\s*&\s*' +
            '\(groupReadErrorId\s*<=\s*32767\).*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
            'groupReadErrorId\$INT.*?else.*?' +
            '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*-6;.*?' +
            'ResponseSize\s*:=\s*16;\s*else\s*' +
            'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
            '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
            'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
            '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
            '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
            '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-3;.*?' +
            'ResponseSize\s*:=\s*12') (
            "Service $($signedAck.Name) typed ACK, disconnected -2 " +
            'mapping, malformed -3 short frame, or signed native-error ' +
            'mapping is incomplete.')
    }

    Assert-Match $serviceGroupStopCaseBlock (
        '(?s)ResponseCapacity\s*<\s*16.*?RequestFrameSize\s*=\s*24.*?' +
        'pRequestFrame\s*\+\s*8.*?pRequestFrame\s*\+\s*12.*?' +
        'pRequestFrame\s*\+\s*16.*?pRequestFrame\s*\+\s*20.*?' +
        'Reference\s*=\s*0x0100.*?bufferMode\s*=\s*1.*?' +
        'groupExecute\s*=\s*1.*?groupDecel\s*>=\s*0.*?' +
        '\(groupJerk\s*>=\s*0\)\s*&\s*' +
        '\(\(groupJerk\s*=\s*0\)\s*\|\s*' +
        '\(groupDecel\s*>\s*0\)\).*?LMCRobot\.StopMove\(.*?' +
        'Mode:=3.*?Decel:=groupDecel.*?Jerk:=groupJerk.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service 0x2085 exact offsets, validation, or StopMove dispatch is missing.')
    if ([regex]::Matches(
            $serviceGroupStopCaseBlock,
            '\bgroupStopCommandNo\b').Count -ne 2) {
        throw 'Service 0x2085 must treat StopMove output only as an opaque command number.'
    }
    Assert-Match $serviceGroupStopCaseBlock (
        '(?s)groupReadErrorId\s*:=\s*-3;.*?' +
        'if\s+groupCommandInputValid\s*=\s*TRUE\s+then\s*' +
        'groupReadErrorId\s*:=\s*-2;\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'LMCRobot\.StopMove\(.*?groupReadErrorId\s*:=\s*0;.*?' +
        'end_if;\s*else\s*groupReadErrorId\s*:=\s*-7;\s*end_if;\s*' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=16\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\);.*?' +
        'if\s+groupReadErrorId\s*=\s*0\s+then.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*0;.*?' +
        'else.*?\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*' +
        'groupReadErrorId\$INT;.*?ResponseSize\s*:=\s*16') (
        'Service 0x2085 must always return the 16-byte typed ACK with ' +
        'disconnected -2, invalid-motion -7, and accepted zero semantics.')

    Assert-Match $serviceGroupMoveCaseBlock (
        '(?s)ResponseSize\s*:=\s*MoveLinearAbsEx\(.*?' +
        'Reference:=Reference.*?pResponseFrame:=pResponseFrame.*?' +
        'ResponseCapacity:=ResponseCapacity.*?' +
        'pRequestFrame:=pRequestFrame.*?' +
        'RequestFrameSize:=RequestFrameSize') (
        'Service 0x20A4 does not delegate the unchanged zero-copy frame ABI.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)pResponseFrame\s*=\s*NIL.*?ResponseCapacity\s*<\s*16.*?' +
        'pRequestFrame\s*<>\s*NIL.*?RequestFrameSize\s*=\s*104.*?' +
        'Reference\s*=\s*0x0100.*?_memcpy\(ptr1:=#GroupMovePos,\s*' +
        'ptr2:=pRequestFrame\s*\+\s*8,\s*cntr:=16\)') (
        'Service MoveLinearAbsEx exact request/capacity/position-vector contract is missing.')
    foreach ($offset in @(72, 76, 80, 84, 88, 92, 96, 100)) {
        Assert-Match $serviceMoveLinearBlock (
            '\(pRequestFrame\s*\+\s*' + $offset + '\)\^\$DINT') (
            "Service 0x20A4 request DINT offset $offset is missing.")
    }
    Assert-Match $serviceMoveLinearBlock (
        '(?s)for kinIndex\s*:=\s*4 to 15 do.*?' +
        'pRequestFrame\s*\+\s*8\s*\+\s*' +
        'TO_UDINT\(kinIndex \* 4\).*?' +
        'groupCommandInputValid\s*:=\s*FALSE') (
        'Service 0x20A4 non-four-axis position rejection is missing.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)groupVelocity\s*>\s*0.*?groupAccel\s*>\s*0.*?' +
        'groupDecel\s*>\s*0.*?groupJerk\s*>=\s*0.*?' +
        'groupCoordSystem\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*2.*?' +
        'bufferMode\s*=\s*1.*?bufferMode\s*=\s*2.*?' +
        'groupExecute\s*=\s*1') (
        'Service 0x20A4 approved motion parameter validation is incomplete.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?' +
        'LMCRobot\.ReadProfileParameter\(.*?_LMCPROF_LockState.*?' +
        'GroupKinematicReady\s*=\s*TRUE.*?powerIsOn\s*<>\s*0.*?' +
        'profileLocked\s*=\s*TRUE.*?LMCRobot\.MoveLinearCoord\(.*?' +
        'pPositions:=#GroupMovePos.*?CmdConfig:=groupCommandConfig.*?' +
        'Velocity:=groupVelocity.*?Accel:=groupAccel.*?' +
        'Decel:=groupDecel.*?TransMode:=groupTransitionMode.*?' +
        'TransRadius:=groupTransitionRadius.*?CoordSystem:=0.*?' +
        'Jerk:=groupJerk.*?' +
        'groupMoveRetCode\s*=\s*_LMCPROF_NoError.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service MoveLinearAbsEx powered/locked dispatch and return-code gate is missing.')
    Assert-Match $serviceMoveLinearBlock (
        '(?s)\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*8.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\).*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        'else.*?\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1') (
        'Service MoveLinearAbsEx 16-byte typed acknowledgement is incomplete.')

    Assert-Match $serviceGroupStatusCaseBlock (
        '(?s)RequestFrameSize\s*=\s*16.*?' +
        'payloadReference\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'executeRequest\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'Reference\s*=\s*0x0100.*?' +
        'payloadReference\s*=\s*TO_DINT\(Reference\).*?' +
        'executeRequest\s*=\s*1.*?IsClientConnected\(#LMCRobot\).*?' +
        'ResponseSize\s*:=\s*GroupReadStatus\(\s*' +
        'pResponseFrame:=pResponseFrame\s*,\s*' +
        'ResponseCapacity:=ResponseCapacity\s*\)') (
        'Service 0x2045 exact descriptor request or GroupReadStatus dispatch is missing.')
    Assert-Match $serviceGroupStatusCaseBlock (
        '(?s)if\s+\(RequestFrameSize\s*=\s*16\)\s*&\s*' +
        '\(Reference\s*=\s*0x0100\)\s*&\s*' +
        '\(payloadReference\s*=\s*TO_DINT\(Reference\)\)\s*&\s*' +
        '\(executeRequest\s*=\s*1\)\s+then\s*' +
        'if\s+IsClientConnected\(#LMCRobot\)\s*=\s*1\s+then.*?' +
        'ResponseSize\s*:=\s*GroupReadStatus\(.*?' +
        'else\s*if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-2;.*?' +
        'ResponseSize\s*:=\s*12;\s*end_if;\s*else\s*' +
        'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*-3;.*?' +
        'ResponseSize\s*:=\s*12') (
        'Service 0x2045 must distinguish disconnected -2 from malformed ' +
        '-3 using the exact 12-byte outer fail-closed frame.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)ResponseCapacity\s*<\s*20.*?' +
        'LMCRobot\.ProfileInPosition\(.*?_LMCPROF_ProfileFinished.*?' +
        'LMCRobot\.RobotIsOn\(\).*?LMCRobot\.ReadProfileParameter\(.*?' +
        '_LMCPROF_LockState.*?LMCRobot\.ReadRobotParameter\(.*?' +
        '_ROBOT_STATE.*?powerIsOn\s*<>\s*0.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00040000.*?' +
        'profileLocked\s*=\s*TRUE.*?groupReadInPosition\s*<>\s*0.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00020000.*?' +
        'profileLocked\s*=\s*FALSE.*?' +
        'groupReadState\s*:=\s*groupReadState or 0x00010000') (
        'Service GroupReadStatus power/lock/in-position state mapping is missing.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)robotState\s*=\s*_ROBOT_ERROR\$DINT.*?' +
        'LMCRobot\.ReadProfileError\(\).*?' +
        'groupReadErrorId\s*:=\s*profileErrorInfo\.ErrorNo\$DINT.*?' +
        'groupReadErrorId\s*=\s*0.*?groupReadErrorId\s*:=\s*-6.*?' +
        'robotState\s*<\s*_ROBOT_PASSIVE\$DINT.*?' +
        'robotState\s*>\s*_ROBOT_MODE_CHANGE\$DINT') (
        'Service GroupReadStatus native error and false-success guards are missing.')
    Assert-Match $serviceGroupReadStatusBlock (
        '(?s)_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=20\).*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*12.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*groupReadState.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*16\)\^\$UINT\s*:=\s*' +
        'groupReadErrorId\$UINT.*?ResponseSize\s*:=\s*20') (
        'Service GroupReadStatus 20-byte typed response is incomplete.')
    if ($serviceGroupReadStatusBlock -match '\bgroupMoveRetCode\b') {
        throw 'Service GroupReadStatus must not report stale move return state.'
    }

    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)RequestFrameSize\s*=\s*16.*?' +
        'groupCoordSystem\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'groupExecute\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'Reference\s*=\s*0x0100.*?groupExecute\s*=\s*1.*?' +
        'groupCoordSystem\s*=\s*0.*?groupCoordSystem\s*=\s*1.*?' +
        'LMCRobot\.GetRobotPosition\(.*?_ACTPOS_APPUNITS.*?' +
        'CoordSystem:=0.*?pPositions:=#groupReadPos.*?' +
        'groupCoordSystem\s*=\s*2.*?groupCoordSystem\s*=\s*3.*?' +
        'groupReadErrorId\s*:=\s*-7') (
        'Service 0x2051 coordinate validation or GetRobotPosition mapping is missing.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)groupReadRetCode\s*:=\s*LMCRobot\.GetRobotPosition\(.*?' +
        'if\s+groupReadRetCode\s*=\s*_LMCPROF_NoError\s+then\s*' +
        'groupReadErrorId\s*:=\s*0;\s*' +
        'elsif\s+groupReadRetCode\$UDINT\s*<=\s*32767\s+then\s*' +
        'groupReadErrorId\s*:=\s*groupReadRetCode\$DINT;\s*' +
        'else\s*groupReadErrorId\s*:=\s*-6;\s*end_if;') (
        'Service 0x2051 must map only _LMCPROF_NoError to success, ' +
        'preserve representable native errors, and map overflow to -6.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)ResponseCapacity\s*<\s*76.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=76\).*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*68.*?' +
        '_memcpy\(ptr1:=pResponseFrame\s*\+\s*8,\s*' +
        'ptr2:=#groupReadPos,\s*cntr:=36\).*?' +
        '\(pResponseFrame\s*\+\s*72\)\^\$UINT\s*:=\s*0x4000.*?' +
        'ResponseCapacity\s*<\s*12.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4') (
        'Service 0x2051 success frame or outer-status-zero error frame is incomplete.')
    Assert-Match $serviceGroupPositionCaseBlock (
        '(?s)ResponseSize\s*:=\s*76;\s*else\s*' +
        'if\s+ResponseCapacity\s*<\s*12\s+then.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\);.*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4;.*?' +
        '\(pResponseFrame\s*\+\s*4\)\^\$UDINT\s*:=\s*0;.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1;.*?' +
        '\(pResponseFrame\s*\+\s*10\)\^\$INT\s*:=\s*' +
        'groupReadErrorId\$INT;.*?ResponseSize\s*:=\s*12') (
        'Service 0x2051 error path must return the exact outer-success ' +
        '12-byte status/error frame.')

    $kinSizeGuard = [regex]::Match(
        $serviceKinematicCaseBlock,
        'kinValid\s*:=\s*\(RequestFrameSize\s*=\s*1328\)\s*&\s*' +
        '\(Reference\s*=\s*0x0100\)')
    $kinFirstGate = [regex]::Match(
        $serviceKinematicCaseBlock,
        'if\s+kinValid\s*=\s*TRUE\s+then')
    $kinFirstDereference = [regex]::Match(
        $serviceKinematicCaseBlock,
        '\(pRequestFrame\s*\+')
    if (-not $kinSizeGuard.Success -or -not $kinFirstGate.Success -or
        -not $kinFirstDereference.Success -or
        $kinSizeGuard.Index -ge $kinFirstGate.Index -or
        $kinFirstGate.Index -ge $kinFirstDereference.Index) {
        throw ('Service 0x20E7 must establish the exact 1328-byte guard ' +
            'before its first request-pointer dereference.')
    }
    if ([regex]::Matches(
            $serviceKinematicCaseBlock,
            'if\s+kinValid\s*=\s*TRUE\s+then').Count -ne 4) {
        throw ('Service 0x20E7 must retain three bounded validation ' +
            'stages followed by one guarded dispatch stage.')
    }
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)if\s+kinValid\s*=\s*TRUE\s+then\s*' +
        'for kinIndex\s*:=\s*0 to 3 do.*?' +
        'pRequestFrame\s*\+\s*8.*?' +
        '0x3FF00000.*?TO_UDINT\(kinIndex \+ 1\).*?' +
        'pRequestFrame\s*\+\s*44') (
        'Service 0x20E7 four-axis identity-entry validation is incomplete.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)for kinIndex\s*:=\s*168 to 647 do.*?' +
        'for kinIndex\s*:=\s*652 to 1311 do.*?' +
        'for kinIndex\s*:=\s*1321 to 1327 do') (
        'Service 0x20E7 reserved zero ranges do not cover the complete frame tail.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)\(pRequestFrame\s*\+\s*648\)\^\$DINT\s*<>\s*4.*?' +
        '\(pRequestFrame\s*\+\s*1312\)\^\$DINT\s*<>\s*0.*?' +
        '\(pRequestFrame\s*\+\s*1316\)\^\$DINT\s*<>\s*2.*?' +
        '\(pRequestFrame\s*\+\s*1320\)\^\$DINT\s*<>\s*1') (
        'Service 0x20E7 Cartesian4 topology constants are incomplete.')
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?' +
        'GroupKinematicReady\s*:=\s*TRUE.*?' +
        'groupReadErrorId\s*:=\s*0') (
        'Service 0x20E7 four-axis mapping registration is missing.')
    if ($serviceKinematicCaseBlock -match
        'LockProfile|UnlockProfile|RobotOn|RobotOff') {
        throw 'Service 0x20E7 must not change profile-lock or group-power state.'
    }
    Assert-Match $serviceKinematicCaseBlock (
        '(?s)ResponseCapacity\s*<\s*12.*?' +
        '_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=12\).*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*4.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*0.*?' +
        'else.*?\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1') (
        'Service 0x20E7 short acknowledgement framing is incomplete.')

    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)RequestFrameSize\s*>=\s*16.*?' +
        'pRequestFrame\s*\+\s*8.*?pRequestFrame\s*\+\s*10.*?' +
        'pRequestFrame\s*\+\s*12.*?RequestFrameSize\s*>=\s*20.*?' +
        'pRequestFrame\s*\+\s*16.*?RequestFrameSize\s*<>\s*20.*?' +
        'Reference\s*<>\s*0x0100.*?adminSchemaVersion\s*<>\s*1.*?' +
        'adminRequestFlags\s*<>\s*0.*?adminRequestId\s*=\s*0.*?' +
        'adminSelectionMask\s*=\s*0.*?' +
        'adminSelectionMask and 0xFFFFFFF8.*?' +
        'IsClientConnected\(#LMCRobot\)\s*<>\s*1') (
        'Service 0x7D20 exact request offsets/reference/mask validation is incomplete.')
    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)if\s+RequestFrameSize\s*<>\s*20\s+then\s*' +
        'adminDetailCode\s*:=\s*5;\s*' +
        'elsif\s+Reference\s*<>\s*0x0100\s+then\s*' +
        'adminDetailCode\s*:=\s*4;\s*' +
        'elsif\s+adminSchemaVersion\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*1;\s*' +
        'elsif\s+adminRequestFlags\s*<>\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*2;\s*' +
        'elsif\s+adminRequestId\s*=\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*3;\s*' +
        'elsif\s+\(adminSelectionMask\s*=\s*0\)\s*\|\s*' +
        '\(\(adminSelectionMask\s+and\s+0xFFFFFFF8\)\s*<>\s*0\)\s+then\s*' +
        'adminDetailCode\s*:=\s*8;\s*' +
        'elsif\s+IsClientConnected\(#LMCRobot\)\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*7;\s*end_if;') (
        'Service 0x7D20 Admin detail mapping must remain exactly ' +
        'size=5, reference=4, schema=1, flags=2, request=3, mask=8, client=7.')
    foreach ($groupParameter in @(
            '_LMCPROF_GRP_VEL_LIMIT',
            '_LMCPROF_GRP_ACCEL_LIMIT',
            '_LMCPROF_GRP_TJERK')) {
        Assert-Match $serviceAdminGroupParametersCaseBlock (
            'LMCRobot\.ReadGroupParameter\(\s*GrpNo:=1,\s*ParNo:=' +
            [regex]::Escape($groupParameter) + '\)') (
            "Service 0x7D20 is missing $groupParameter mapping.")
    }
    if ([regex]::Matches(
            $serviceAdminGroupParametersCaseBlock,
            '\bLMCRobot\.ReadGroupParameter\s*\(').Count -ne 3) {
        throw 'Service 0x7D20 must expose exactly three selected native reads.'
    }
    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)if\s+adminDetailCode\s*=\s*0\s+then\s*' +
        'if\s+ResponseCapacity\s*<\s*40\s+then\s*RETURN;\s*end_if;.*?' +
        'LMCRobot\.ReadGroupParameter\(.*?' +
        'else\s*if\s+ResponseCapacity\s*<\s*24\s+then\s*' +
        'RETURN;\s*end_if;\s*end_if;') (
        'Service 0x7D20 must prove success/error response capacity before ' +
        'performing any selected native read.')
    Assert-Match $serviceAdminGroupParametersCaseBlock (
        '(?s)ResponseCapacity\s*<\s*40.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*32.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*' +
        'adminSelectionMask.*?' +
        '\(pResponseFrame\s*\+\s*28\)\^\$DINT\s*:=\s*' +
        'adminGroupVelocityLimit.*?' +
        '\(pResponseFrame\s*\+\s*36\)\^\$DINT\s*:=\s*' +
        'adminGroupJerkTime.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*16.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*-31000') (
        'Service 0x7D20 success/error Admin frames are incomplete.')

    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)ResponseCapacity\s*<\s*24.*?' +
        'RequestFrameSize\s*>=\s*16.*?pRequestFrame\s*\+\s*8.*?' +
        'pRequestFrame\s*\+\s*10.*?pRequestFrame\s*\+\s*12.*?' +
        'RequestFrameSize\s*<>\s*112.*?Reference\s*<>\s*0x0100.*?' +
        'adminSchemaVersion\s*<>\s*1.*?adminRequestFlags\s*<>\s*0.*?' +
        'adminRequestId\s*=\s*0') (
        'Service 0x7D22 exact request envelope validation is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+RequestFrameSize\s*<>\s*112\s+then\s*' +
        'adminDetailCode\s*:=\s*5;\s*' +
        'elsif\s+Reference\s*<>\s*0x0100\s+then\s*' +
        'adminDetailCode\s*:=\s*4;\s*' +
        'elsif\s+adminSchemaVersion\s*<>\s*1\s+then\s*' +
        'adminDetailCode\s*:=\s*1;\s*' +
        'elsif\s+adminRequestFlags\s*<>\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*2;\s*' +
        'elsif\s+adminRequestId\s*=\s*0\s+then\s*' +
        'adminDetailCode\s*:=\s*3;\s*else.*?' +
        'if\s+groupCommandInputValid\s*=\s*FALSE\s+then\s*' +
        'adminDetailCode\s*:=\s*9;\s*end_if;\s*end_if;') (
        'Service 0x7D22 Admin input detail mapping must remain exactly ' +
        'size=5, reference=4, schema=1, flags=2, request=3, motion=9.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)_memcpy\(ptr1:=#GroupMovePos,\s*' +
        'ptr2:=pRequestFrame\s*\+\s*16,\s*cntr:=16\).*?' +
        'pRequestFrame\s*\+\s*80.*?pRequestFrame\s*\+\s*84.*?' +
        'pRequestFrame\s*\+\s*88.*?pRequestFrame\s*\+\s*92.*?' +
        'pRequestFrame\s*\+\s*96.*?pRequestFrame\s*\+\s*100.*?' +
        'pRequestFrame\s*\+\s*104.*?pRequestFrame\s*\+\s*108') (
        'Service 0x7D22 position and DINT field offsets are incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)groupVelocity\s*>\s*0.*?groupAccel\s*>\s*0.*?' +
        'groupDecel\s*>\s*0.*?groupJerk\s*>=\s*0.*?' +
        'groupCoordSystem\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*0.*?' +
        'groupTransitionModeInput\s*=\s*2.*?' +
        'bufferMode\s*=\s*1.*?bufferMode\s*=\s*2.*?' +
        'groupExecute\s*=\s*1.*?for kinIndex\s*:=\s*4 to 15 do.*?' +
        'groupCommandInputValid\s*:=\s*FALSE.*?adminDetailCode\s*:=\s*9') (
        'Service 0x7D22 motion and four-axis tail validation is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)case groupTransitionModeInput of.*?_LMCPROF_EXACT_STOP.*?' +
        '_LMCPROF_CONT_DIRECT.*?bufferMode\s*=\s*1.*?' +
        'groupCommandConfig\s*:=\s*16') (
        'Service 0x7D22 transition/buffer mapping is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)IsClientConnected\(#LMCRobot\).*?' +
        'IsClientConnected\(#LMCAxis1\).*?' +
        'IsClientConnected\(#LMCAxis4\).*?LMCRobot\.RobotIsOn\(\).*?' +
        'LMCRobot\.ReadProfileParameter\(.*?_LMCPROF_LockState.*?' +
        'GroupKinematicReady\s*=\s*TRUE.*?powerIsOn\s*<>\s*0.*?' +
        'profileLockState\s*<>\s*0.*?LMCRobot\.MoveRelativeCoord\(.*?' +
        'pDistances:=#GroupMovePos.*?CmdConfig:=groupCommandConfig.*?' +
        'Velocity:=groupVelocity.*?Accel:=groupAccel.*?' +
        'Decel:=groupDecel.*?TransMode:=groupTransitionMode.*?' +
        'TransRadius:=groupTransitionRadius.*?CoordSystem:=0.*?' +
        'Jerk:=groupJerk') (
        'Service 0x7D22 powered/locked MoveRelativeCoord dispatch is missing.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)if\s+adminDetailCode\s*=\s*0\s+then\s*' +
        'if\s+\(IsClientConnected\(#LMCRobot\)\s*=\s*1\).*?then.*?' +
        'if\s+\(GroupKinematicReady\s*=\s*TRUE\)\s*&\s*' +
        '\(powerIsOn\s*<>\s*0\)\s*&\s*' +
        '\(profileLockState\s*<>\s*0\)\s+then.*?' +
        'if\s+groupMoveRetCode\s*=\s*_LMCPROF_NoError\s+then\s*' +
        'adminErrorId\s*:=\s*0;\s*else\s*' +
        'adminDetailCode\s*:=\s*11;.*?end_if;\s*' +
        'else\s*adminDetailCode\s*:=\s*10;\s*end_if;\s*' +
        'else\s*adminDetailCode\s*:=\s*10;\s*end_if;\s*end_if;') (
        'Service 0x7D22 must map readiness/client failure to detail 10 ' +
        'and native rejection to detail 11.')
    if ([regex]::Matches(
            $serviceAdminRelativeMoveCaseBlock,
            'adminDetailCode\s*:=\s*10\s*;').Count -ne 2 -or
        [regex]::Matches(
            $serviceAdminRelativeMoveCaseBlock,
            'adminDetailCode\s*:=\s*11\s*;').Count -ne 1) {
        throw 'Service 0x7D22 state detail 10 and native detail 11 assignments are not exact.'
    }
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)groupMoveRetCode\s*=\s*_LMCPROF_NoError.*?' +
        'adminErrorId\s*:=\s*0.*?adminDetailCode\s*:=\s*11.*?' +
        'groupMoveRetCode\$UDINT\s*<=\s*32767.*?' +
        'adminErrorId\s*:=\s*groupMoveRetCode\$INT.*?' +
        'adminErrorId\s*:=\s*-6.*?adminDetailCode\s*:=\s*10') (
        'Service 0x7D22 native rejection/state detail mapping is incomplete.')
    Assert-Match $serviceAdminRelativeMoveCaseBlock (
        '(?s)_memset\(dest:=pResponseFrame,\s*usByte:=0,\s*cntr:=24\).*?' +
        'pResponseFrame\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*16.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*0.*?' +
        '\(pResponseFrame\s*\+\s*16\)\^\$UDINT\s*:=\s*' +
        'adminRequestId.*?' +
        '\(pResponseFrame\s*\+\s*20\)\^\$UDINT\s*:=\s*' +
        'adminDetailCode.*?adminDetailCode\s*<>\s*0.*?' +
        '\(pResponseFrame\s*\+\s*12\)\^\$UINT\s*:=\s*1.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*adminErrorId') (
        'Service 0x7D22 fixed outer-success Admin response framing is incomplete.')
}

if ($controlServiceAllControlRouted) {
    $serviceRegistryHandlerBlock =
        $controlServicePrivateBlocks['HandleRegistryCommands']
    $serviceAxisHandlerBlock =
        $controlServicePrivateBlocks['HandleAxisCommands']
    $serviceAdminHandlerBlock =
        $controlServicePrivateBlocks['HandleAdminCommands']

    foreach ($pointerHandler in @(
            @{ Name = 'Registry'; Block = $serviceRegistryHandlerBlock },
            @{ Name = 'Axis'; Block = $serviceAxisHandlerBlock },
            @{ Name = 'Admin'; Block = $serviceAdminHandlerBlock })) {
        Assert-Match $pointerHandler.Block (
            '(?s)ResponseSize\s*:=\s*-1\s*;.*?' +
            'if\s+\(pRequestFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(pResponseFrame\s*=\s*NIL\)\s*\|\s*' +
            '\(RequestFrameSize\s*<\s*8\)\s+then\s*' +
            'RETURN;\s*end_if;') (
            "Service $($pointerHandler.Name) pointer/minimum-frame guard is missing.")
    }

    $serviceRegistryAxisLookupCaseBlock = [regex]::Match(
        $serviceRegistryHandlerBlock,
        '(?s)0x103C:.*?0x1042:').Value
    $serviceRegistryGroupLookupCaseBlock = [regex]::Match(
        $serviceRegistryHandlerBlock,
        '(?s)0x1042:.*?0x202B:').Value
    $serviceRegistryReferenceCaseBlock = [regex]::Match(
        $serviceRegistryHandlerBlock,
        '(?s)0x202B:.*?(?=\s+else\s+ResponseSize\s*:=\s*-1)').Value
    foreach ($registryCase in @(
            @{ Name = '0x103C'; Block = $serviceRegistryAxisLookupCaseBlock },
            @{ Name = '0x1042'; Block = $serviceRegistryGroupLookupCaseBlock },
            @{ Name = '0x202B'; Block = $serviceRegistryReferenceCaseBlock })) {
        if ([string]::IsNullOrWhiteSpace($registryCase.Block)) {
            throw "Service Registry $($registryCase.Name) case was not found."
        }
    }
    Assert-ExactRegexValueSet `
        -Text $serviceRegistryHandlerBlock `
        -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
        -Owner 'Service Registry response sizes' `
        -ExpectedValues @('12', '14', '16')
    Assert-ExactRegexValueSet `
        -Text $serviceRegistryHandlerBlock `
        -Pattern 'ResponseCapacity\s*<\s*(?<Value>[1-9][0-9]*)' `
        -Owner 'Service Registry response capacities' `
        -ExpectedValues @('12', '14', '16')

    $registryAxisSizeGate = [regex]::Match(
        $serviceRegistryAxisLookupCaseBlock,
        'if\s+RequestFrameSize\s*=\s*88\s+then')
    $registryAxisFirstDereference = [regex]::Match(
        $serviceRegistryAxisLookupCaseBlock,
        '\(pRequestFrame\s*\+')
    if (-not $registryAxisSizeGate.Success -or
        -not $registryAxisFirstDereference.Success -or
        $registryAxisSizeGate.Index -ge $registryAxisFirstDereference.Index) {
        throw 'Service 0x103C must prove the exact 88-byte frame before dereference.'
    }
    Assert-Match $serviceRegistryAxisLookupCaseBlock (
        '(?s)ResponseCapacity\s*<\s*14.*?' +
        '\(pRequestFrame\s*\+\s*87\)\^\s*:=\s*0.*?' +
        '_stricmp\(str1:=\(pRequestFrame\s*\+\s*8\)\$\^CHAR') (
        'Service 0x103C capacity, bounded name terminator, or name-pointer ' +
        'offset is missing.')
    Assert-ExactLasalConnectedClientSet `
        -Text $serviceRegistryAxisLookupCaseBlock `
        -Owner 'Service 0x103C axis lookup' `
        -ExpectedClients @(
            'LMCAxis1', 'LMCAxis2', 'LMCAxis3',
            'LMCAxis4', 'LMCAxis5', 'LMCAxis6',
            'LMCAxis7', 'LMCAxis8', 'LMCAxis9')
    if ([regex]::Matches(
            $serviceRegistryAxisLookupCaseBlock,
            '_GetObjName\(\s*pThis:=LMCAxis[1-9]\.pCmd').Count -ne 9 -or
        [regex]::Matches(
            $serviceRegistryAxisLookupCaseBlock,
            '_stricmp\(str1:=\(pRequestFrame\s*\+\s*8\)\$\^CHAR').Count -ne 9) {
        throw 'Service 0x103C must query and compare all nine axis names once.'
    }
    if ($serviceRegistryAxisLookupCaseBlock -match
        'ObjectRegistryReady|_strcmp') {
        throw 'Service 0x103C must use independent case-insensitive lookup state.'
    }

    $registryGroupSizeGate = [regex]::Match(
        $serviceRegistryGroupLookupCaseBlock,
        '(?s)if\s+\(RequestFrameSize\s*=\s*88\)\s*&\s*' +
        '\(IsClientConnected\(#LMCRobot\)\s*=\s*1\)\s+then')
    $registryGroupFirstDereference = [regex]::Match(
        $serviceRegistryGroupLookupCaseBlock,
        '\(pRequestFrame\s*\+')
    if (-not $registryGroupSizeGate.Success -or
        -not $registryGroupFirstDereference.Success -or
        $registryGroupSizeGate.Index -ge $registryGroupFirstDereference.Index) {
        throw 'Service 0x1042 must prove the exact 88-byte frame before dereference.'
    }
    Assert-Match $serviceRegistryGroupLookupCaseBlock (
        '(?s)ResponseCapacity\s*<\s*14.*?' +
        '\(pRequestFrame\s*\+\s*87\)\^\s*:=\s*0.*?' +
        '_GetObjName\(\s*pThis:=LMCRobot\.pCmd.*?' +
        '_stricmp\(str1:=\(pRequestFrame\s*\+\s*8\)\$\^CHAR.*?' +
        'resolvedReference\s*:=\s*0x0100') (
        'Service 0x1042 pointer bounds or robot lookup mapping is incomplete.')
    Assert-Match $serviceRegistryReferenceCaseBlock (
        '(?s)\(RequestFrameSize\s*=\s*20\)\s*&\s*' +
        '\(Reference\s*>=\s*1\)\s*&\s*\(Reference\s*<=\s*9\).*?' +
        'ResponseCapacity\s*<\s*16.*?' +
        '\(pResponseFrame\s*\+\s*8\)\^\$UDINT\s*:=\s*' +
        'TO_UDINT\(Reference\).*?ResponseSize\s*:=\s*16.*?' +
        'else\s*if\s+ResponseCapacity\s*<\s*12') (
        'Service 0x202B exact envelope or reference response is incomplete.')
    if ($serviceRegistryReferenceCaseBlock -match '\(pRequestFrame\s*\+') {
        throw 'Service 0x202B must not dereference its payload-free request frame.'
    }

    $serviceAxisPowerCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x2023:.*?0x2024:').Value
    $serviceAxisResetCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x2024:.*?0x2022:').Value
    $serviceAxisStopCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x2022:.*?0x2028:').Value
    $serviceAxisStatusCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x2028:.*?0x202E:').Value
    $serviceAxisPositionCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x202E:.*?0x209F,\s*0x20A0,\s*0x20A2:').Value
    $serviceAxisMotionCaseBlock = [regex]::Match(
        $serviceAxisHandlerBlock,
        '(?s)0x209F,\s*0x20A0,\s*0x20A2:.*').Value
    foreach ($axisCase in @(
            @{ Name = '0x2023'; Block = $serviceAxisPowerCaseBlock },
            @{ Name = '0x2024'; Block = $serviceAxisResetCaseBlock },
            @{ Name = '0x2022'; Block = $serviceAxisStopCaseBlock },
            @{ Name = '0x2028'; Block = $serviceAxisStatusCaseBlock },
            @{ Name = '0x202E'; Block = $serviceAxisPositionCaseBlock },
            @{ Name = '0x209F/0x20A0/0x20A2'; Block = $serviceAxisMotionCaseBlock })) {
        if ([string]::IsNullOrWhiteSpace($axisCase.Block)) {
            throw "Service Axis $($axisCase.Name) case was not found."
        }
    }
    Assert-ExactRegexValueSet `
        -Text $serviceAxisHandlerBlock `
        -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
        -Owner 'Service Axis response sizes' `
        -ExpectedValues @('12', '16', '20')
    Assert-ExactRegexValueSet `
        -Text $serviceAxisHandlerBlock `
        -Pattern 'ResponseCapacity\s*<\s*(?<Value>[1-9][0-9]*)' `
        -Owner 'Service Axis response capacities' `
        -ExpectedValues @('12', '16', '20')

    foreach ($exactAxisFrame in @(
            @{ Name = '0x2023'; Block = $serviceAxisPowerCaseBlock; Size = 16 },
            @{ Name = '0x2024'; Block = $serviceAxisResetCaseBlock; Size = 9 },
            @{ Name = '0x202E'; Block = $serviceAxisPositionCaseBlock; Size = 9 })) {
        $sizeGate = [regex]::Match(
            $exactAxisFrame.Block,
            'if\s+RequestFrameSize\s*=\s*' + $exactAxisFrame.Size +
            '\s+then')
        $firstDereference = [regex]::Match(
            $exactAxisFrame.Block,
            '\(pRequestFrame\s*\+')
        if (-not $sizeGate.Success -or -not $firstDereference.Success -or
            $sizeGate.Index -ge $firstDereference.Index) {
            throw (
                "Service $($exactAxisFrame.Name) must prove the exact " +
                "$($exactAxisFrame.Size)-byte frame before dereference.")
        }
    }
    Assert-Match $serviceAxisStopCaseBlock (
        '(?s)if\s+\(RequestFrameSize\s*<>\s*24\).*?' +
        'ResponseSize\s*:=\s*12;\s*RETURN;\s*end_if;.*?' +
        'ResponseCapacity\s*<\s*16.*?' +
        'deceleration\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'jerk\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'bufferMode\s*:=\s*\(pRequestFrame\s*\+\s*16\)\^\$DINT.*?' +
        'execute\s*:=\s*\(pRequestFrame\s*\+\s*20\)\^\$DINT.*?' +
        'axisCommandInputValid\s*:=\s*\(bufferMode\s*=\s*1\)\s*&\s*' +
        '\(execute\s*=\s*1\)\s*&\s*\(deceleration\s*>\s*0\)\s*&\s*' +
        '\(jerk\s*>=\s*0\)') (
        'Service 0x2022 exact frame rejection or pointer offsets are incomplete.')
    Assert-Match $serviceAxisStopCaseBlock (
        '(?s)elsif\s+axisCommandInputValid\s*=\s*TRUE\s+then.*?' +
        'axisCommandErrorId\s*:=\s*-2.*?' +
        'else\s*axisCommandStatus\s*:=\s*1;\s*' +
        'axisCommandErrorId\s*:=\s*-7') (
        'Service 0x2022 semantic rejection must return ErrorId -7 without dispatch.')
    Assert-Match $serviceAxisStatusCaseBlock (
        '(?s)if\s+RequestFrameSize\s*>=\s*16\s+then\s*' +
        'payloadReference\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'execute\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$DINT.*?' +
        'if\s+\(RequestFrameSize\s*=\s*16\)\s*&.*?' +
        'ResponseCapacity\s*<\s*20') (
        'Service 0x2028 bounded decode or exact envelope validation is incomplete.')
    Assert-Match $serviceAxisMotionCaseBlock (
        '(?s)\(CommandId\s*=\s*0x209F\)\s*\|\s*' +
        '\(CommandId\s*=\s*0x20A0\).*?' +
        'if\s+RequestFrameSize\s*=\s*40\s+then.*?' +
        'position\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'jerk\s*:=\s*\(pRequestFrame\s*\+\s*24\)\^\$DINT.*?' +
        'else\s*if\s+RequestFrameSize\s*=\s*32\s+then.*?' +
        'speed\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$DINT.*?' +
        'jerk\s*:=\s*\(pRequestFrame\s*\+\s*20\)\^\$DINT') (
        'Service axis motion exact frame gates or pointer offsets are incomplete.')
    foreach ($nativeAxisCall in @(
            'PowerOn', 'PowerOff', 'QuitError', 'StopMove',
            'ReadAxisStatus', 'ReadAxisError', 'ReadPosition',
            'MoveShortestWay', 'MoveRelative', 'MoveEndless')) {
        $nativeAxisCallCount = [regex]::Matches(
            $serviceAxisHandlerBlock,
            ('\bLMCAxis[1-9]\.' + [regex]::Escape($nativeAxisCall) +
             '\s*\(')).Count
        if ($nativeAxisCallCount -ne 9) {
            throw (
                "Service Axis $nativeAxisCall call count is " +
                "$nativeAxisCallCount, expected exactly nine.")
        }
    }

    $serviceAdminCapabilitiesCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D00:.*?0x7D10:').Value
    $serviceAdminAxisParameterCaseBlock = [regex]::Match(
        $serviceAdminHandlerBlock,
        '(?s)0x7D10:.*?0x7D20:').Value
    if ([string]::IsNullOrWhiteSpace($serviceAdminCapabilitiesCaseBlock) -or
        [string]::IsNullOrWhiteSpace($serviceAdminAxisParameterCaseBlock)) {
        throw 'Service Admin 0x7D00/0x7D10 cases were not found.'
    }
    Assert-ExactRegexValueSet `
        -Text $serviceAdminCapabilitiesCaseBlock `
        -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
        -Owner 'Service Admin 0x7D00 response sizes' `
        -ExpectedValues @('48')
    Assert-ExactRegexValueSet `
        -Text $serviceAdminAxisParameterCaseBlock `
        -Pattern 'ResponseSize\s*:=\s*(?<Value>[1-9][0-9]*)\s*;' `
        -Owner 'Service Admin 0x7D10 response sizes' `
        -ExpectedValues @('24', '36')
    Assert-Match $serviceAdminCapabilitiesCaseBlock (
        '(?s)ResponseCapacity\s*<\s*48.*?' +
        'if\s+RequestFrameSize\s*>=\s*16\s+then\s*' +
        'adminSchemaVersion\s*:=\s*\(pRequestFrame\s*\+\s*8\)\^\$UINT.*?' +
        'adminRequestFlags\s*:=\s*\(pRequestFrame\s*\+\s*10\)\^\$UINT.*?' +
        'adminRequestId\s*:=\s*\(pRequestFrame\s*\+\s*12\)\^\$UDINT.*?' +
        'RequestFrameSize\s*<>\s*16.*?Reference\s*<>\s*0.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*40.*?' +
        '\(pResponseFrame\s*\+\s*24\)\^\$UDINT\s*:=\s*0x00000007.*?' +
        '\(pResponseFrame\s*\+\s*42\)\^\$UINT\s*:=\s*3.*?' +
        'ResponseSize\s*:=\s*48') (
        'Service 0x7D00 pointer envelope or capability response is incomplete.')
    Assert-Match $serviceAdminAxisParameterCaseBlock (
        '(?s)ResponseCapacity\s*<\s*36.*?' +
        'if\s+RequestFrameSize\s*>=\s*16\s+then.*?' +
        'pRequestFrame\s*\+\s*8.*?pRequestFrame\s*\+\s*10.*?' +
        'pRequestFrame\s*\+\s*12.*?' +
        'if\s+RequestFrameSize\s*>=\s*20\s+then.*?' +
        'adminParameterKey\s*:=\s*\(pRequestFrame\s*\+\s*16\)\^\$UINT.*?' +
        'RequestFrameSize\s*<>\s*20.*?' +
        '\(pRequestFrame\s*\+\s*18\)\^\$UINT\s*<>\s*0') (
        'Service 0x7D10 bounded common/parameter pointer decode is incomplete.')
    Assert-Match $serviceAdminAxisParameterCaseBlock (
        '(?s)case\s+adminParameterKey\s+of.*?' +
        'LMCAXIS_RD_SWMIN_APPUNIT.*?LMCAXIS_RD_SWMAX_APPUNIT.*?' +
        'LMCAXIS_PAR_RD_SWLIMWINDOW.*?LMCAXIS_PAR_RD_V_MAX.*?' +
        'LMCAXIS_PAR_RD_A_MAX.*?LMCAXIS_PAR_RD_REFPOS.*?' +
        'adminDetailCode\s*:=\s*6') (
        'Service 0x7D10 semantic-to-native allowlist mapping is incomplete.')
    if ([regex]::Matches(
            $serviceAdminAxisParameterCaseBlock,
            '\bLMCAxis[1-4]\.ReadSWEndPos\s*\(').Count -ne 4 -or
        [regex]::Matches(
            $serviceAdminAxisParameterCaseBlock,
            '\bLMCAxis[1-4]\.ReadParameter\s*\(').Count -ne 4) {
        throw 'Service 0x7D10 must expose both native read paths for four axes.'
    }
    Assert-Match $serviceAdminAxisParameterCaseBlock (
        '(?s)\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*28.*?' +
        '\(pResponseFrame\s*\+\s*24\)\^\$UINT\s*:=\s*adminParameterKey.*?' +
        '\(pResponseFrame\s*\+\s*32\)\^\$DINT\s*:=\s*adminAxisValue.*?' +
        'ResponseSize\s*:=\s*36.*?else.*?' +
        '\(pResponseFrame\s*\+\s*2\)\^\$UINT\s*:=\s*16.*?' +
        '\(pResponseFrame\s*\+\s*14\)\^\$INT\s*:=\s*-31000.*?' +
        'ResponseSize\s*:=\s*24') (
        'Service 0x7D10 success/error pointer response framing is incomplete.')
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

Assert-TCPMotionInterfaceSamePeerTakeover `
    -TcpText $st `
    -Owner 'TCPMotionInterface same-peer takeover'

Assert-TCPMotionInterfaceFreshOwnerReset `
    -TcpText $st `
    -Owner 'TCPMotionInterface'

$tcpServerTakeoverNegativeFixtures = [ordered]@{}
$tcpServerTakeoverNegativeFixtures['WrongCustomCommand'] =
    ([regex]::new('(?i)if\s+Cmd\s*=\s*100\s+then')).Replace(
        $tcpServer,
        'if Cmd = 101 then',
        1)
$tcpServerTakeoverNegativeFixtures['WrongShutdownState'] =
    ([regex]::new(
        '(?i)SocketArray\s*\[\s*i\s*\]\.FSM_TCP\s*:=\s*_STATE_SHUTDOWN\s*;')).Replace(
        $tcpServer,
        'SocketArray[i].FSM_TCP := _STATE_ACCEPT;',
        1)
$tcpServerTakeoverNegativeFixtures['DirectClose'] =
    ([regex]::new(
        '(?im)^(?<Indent>[ \t]*)SocketArray\s*\[\s*i\s*\]\.FSM_TCP\s*:=\s*_STATE_SHUTDOWN\s*;')).Replace(
        $tcpServer,
        ('${Indent}CLOSESOCKET(dSock);' + [Environment]::NewLine +
         '${Indent}SocketArray[i].FSM_TCP := _STATE_SHUTDOWN;'),
        1)
$tcpServerTakeoverNegativeFixtures['DirectListMutation'] =
    ([regex]::new(
        '(?im)^(?<Indent>[ \t]*)SocketArray\s*\[\s*i\s*\]\.FSM_TCP\s*:=\s*_STATE_SHUTDOWN\s*;')).Replace(
        $tcpServer,
        ('${Indent}ActConn -= 1;' + [Environment]::NewLine +
         '${Indent}SocketArray[i].FSM_TCP := _STATE_SHUTDOWN;'),
        1)
$tcpServerTakeoverNegativeFixtures['CyWorkOverride'] =
    ($tcpServer + [Environment]::NewLine +
     'FUNCTION VIRTUAL GLOBAL TCPIPServer::CyWork' + [Environment]::NewLine +
     'END_FUNCTION' + [Environment]::NewLine)
$tcpServerTakeoverNegativeFixtures['ShutdownCommandUnregistered'] =
    ([regex]::new(
        ('(?im)^[ \t]*vmt\.UserFcts\[\s*4\s*\]\s*:=\s*' +
         '#SetSocketParameter\s*\(\s*\)\s*;[ \t]*\r?\n'))).Replace(
        $tcpServer,
        '',
        1)

$tcpServerTakeoverNegativeFixtureCount = 0
foreach ($negativeFixture in
        $tcpServerTakeoverNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -ceq $tcpServer) {
        throw (
            'TCPIPServer takeover negative fixture did not mutate the source ' +
            "for '$($negativeFixture.Key)'.")
    }
    $negativeRejected = $false
    try {
        Assert-TCPIPServerControlledShutdownContract `
            -ServerText ([string]$negativeFixture.Value) `
            -Owner (
                'TCPIPServer takeover negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'TCPIPServer controlled-shutdown verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
    $tcpServerTakeoverNegativeFixtureCount++
}
if ($tcpServerTakeoverNegativeFixtureCount -ne 6) {
    throw (
        'TCPIPServer takeover negative fixture count is ' +
        "$tcpServerTakeoverNegativeFixtureCount, expected six.")
}

$tcpTakeoverConnFixtureBlock = [regex]::Match(
    $st,
    ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
     'TCPMotionInterface::ConnSocketInfo[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
$tcpTakeoverDataHandlingFixtureBlock = [regex]::Match(
    $st,
    ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
     'TCPMotionInterface::DataHandling[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
$tcpTakeoverResponseFixtureBlock = [regex]::Match(
    $st,
    ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
     'TCPMotionInterface::Response[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
if ([string]::IsNullOrWhiteSpace($tcpTakeoverConnFixtureBlock) -or
    [string]::IsNullOrWhiteSpace($tcpTakeoverDataHandlingFixtureBlock) -or
    [string]::IsNullOrWhiteSpace($tcpTakeoverResponseFixtureBlock)) {
    throw 'TCPMotionInterface takeover negative-fixture blocks were not found.'
}

$tcpTakeoverNegativeFixtures = [ordered]@{}
$tcpTakeoverNegativeFixtures['SameIPv4ComparisonReversed'] =
    ([regex]::new(
        '(?i)candidatePeerIPv4\s*<>\s*CurrentPeerIPv4')).Replace(
        $st,
        'candidatePeerIPv4 = CurrentPeerIPv4',
        1)
$tcpTakeoverNegativeFixtures['CandidateLookupFailureAccepted'] =
    ([regex]::new(
        '(?i)if\s+candidatePeerValid\s*=\s*FALSE\s+then')).Replace(
        $st,
        'if candidatePeerValid = TRUE then',
        1)
$tcpTakeoverNegativeFixtures['CurrentLookupFailureAccepted'] =
    ([regex]::new(
        '(?i)elsif\s+CurrentPeerValid\s*=\s*FALSE\s+then')).Replace(
        $st,
        'elsif CurrentPeerValid = TRUE then',
        1)
$tcpTakeoverNegativeFixtures['OldOwnerShutdownCommandDrift'] =
    ([regex]::new(
        ('(?is)(LastOwnerDisconnectRequestRet\s*:=\s*' +
         '_TCPIPServerInterface::SetSocketParameter\s*\(\s*' +
         'dSock\s*:=\s*CurrentSock\s*,\s*Cmd\s*:=\s*)100'))).Replace(
        $st,
        '${1}101',
        1)
$negativeEarlyPublishConnBlock = ([regex]::new(
    '(?im)^(?<Indent>[ \t]*)ActiveRequestValid\s*:=\s*FALSE\s*;')).Replace(
        $tcpTakeoverConnFixtureBlock,
        ('${Indent}CurrentSock := dSock;' + [Environment]::NewLine +
         '${Indent}ActiveRequestValid := FALSE;'),
        1)
$tcpTakeoverNegativeFixtures['EarlyOwnerPublish'] =
    $st.Replace(
        $tcpTakeoverConnFixtureBlock,
        $negativeEarlyPublishConnBlock)
$tcpTakeoverNegativeFixtures['CandidateShutdownTargetsOwner'] =
    ([regex]::new(
        ('(?is)(LastCandidateDisconnectRequestRet\s*:=\s*' +
         '_TCPIPServerInterface::SetSocketParameter\s*\(\s*' +
         'dSock\s*:=\s*)dSock'))).Replace(
        $st,
        '${1}CurrentSock',
        1)

$negativeDataHandlingBlock = ([regex]::new(
    '(?i)if\s+dSocket\s*<>\s*CurrentSock\s+then')).Replace(
        $tcpTakeoverDataHandlingFixtureBlock,
        'if dSocket = CurrentSock then',
        1)
$tcpTakeoverNegativeFixtures['DataHandlingOwnerIsolationRemoved'] =
    $st.Replace(
        $tcpTakeoverDataHandlingFixtureBlock,
        $negativeDataHandlingBlock)
$negativeResponseBlock = ([regex]::new(
    '(?is)if\s+dSock\s*<>\s*CurrentSock\s+then\s*RETURN\s*;\s*end_if\s*;')).Replace(
        $tcpTakeoverResponseFixtureBlock,
        '',
        1)
$tcpTakeoverNegativeFixtures['ResponseOwnerIsolationRemoved'] =
    $st.Replace(
        $tcpTakeoverResponseFixtureBlock,
        $negativeResponseBlock)

$negativeDisconnectConnBlock = ([regex]::new(
    ('(?is)(TCP_SVR_SOCK_INFO_DISCONNECT\s*:.*?' +
     'if\s+)CurrentSock(\s*=\s*dSock\s+then)'))).Replace(
        $tcpTakeoverConnFixtureBlock,
        '${1}RetiringSock${2}',
        1)
$tcpTakeoverNegativeFixtures['LateDisconnectClearsNewOwner'] =
    $st.Replace(
        $tcpTakeoverConnFixtureBlock,
        $negativeDisconnectConnBlock)
$negativeRpcDisconnectConnBlock = ([regex]::new(
    ('(?is)(TCP_SVR_SOCK_INFO_DISCONNECT\s*:.*?)' +
     'if\s+RpcSocket\s*=\s*dSock\s+then'))).Replace(
        $tcpTakeoverConnFixtureBlock,
        '${1}if TRUE then',
        1)
$tcpTakeoverNegativeFixtures['LateDisconnectClearsNewRpcOwner'] =
    $st.Replace(
        $tcpTakeoverConnFixtureBlock,
        $negativeRpcDisconnectConnBlock)
$tcpTakeoverNegativeFixtures['RetiringOwnerNotCaptured'] =
    ([regex]::new(
        '(?i)RetiringSock\s*:=\s*CurrentSock\s*;')).Replace(
        $st,
        'RetiringSock := dSock;',
        1)

$tcpTakeoverNegativeFixtureCount = 0
foreach ($negativeFixture in $tcpTakeoverNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -ceq $st) {
        throw (
            'TCPMotionInterface takeover negative fixture did not mutate the ' +
            "source for '$($negativeFixture.Key)'.")
    }
    $negativeRejected = $false
    try {
        Assert-TCPMotionInterfaceSamePeerTakeover `
            -TcpText ([string]$negativeFixture.Value) `
            -Owner (
                'TCPMotionInterface takeover negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'TCPMotionInterface same-peer verifier accepted negative fixture ' +
            "'$($negativeFixture.Key)'.")
    }
    $tcpTakeoverNegativeFixtureCount++
}
if ($tcpTakeoverNegativeFixtureCount -ne 11) {
    throw (
        'TCPMotionInterface takeover negative fixture count is ' +
        "$tcpTakeoverNegativeFixtureCount, expected eleven.")
}

$tcpFreshOwnerFixtureBlock = [regex]::Match(
    $st,
    ('(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
     'TCPMotionInterface::ConnSocketInfo[ \t]*\r?$' +
     '.*?^[ \t]*END_FUNCTION[ \t]*\r?$')).Value
if ([string]::IsNullOrWhiteSpace($tcpFreshOwnerFixtureBlock)) {
    throw 'TCPMotionInterface fresh-owner negative-fixture block was not found.'
}

$tcpFreshOwnerNegativeFixtures = [ordered]@{}
$tcpFreshOwnerNegativeFixtures['DeleteQueueReset'] =
    ([regex]::new(
        ('(?im)^[ \t]*_memset\s*\(\s*dest\s*:=\s*#\s*' +
         'RequestQueue\s*\[\s*0\s*\]\s*,\s*usByte\s*:=\s*0\s*,' +
         '\s*cntr\s*:=\s*sizeof\s*\(\s*RequestQueue\s*\)' +
         '\s*\)\s*;[ \t]*\r?\n'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            '',
            1)
$tcpFreshOwnerNegativeFixtures['NonZeroWriteIndex'] =
    ([regex]::new(
        '(?i)QueueWriteIndex\s*:=\s*0\s*;')).Replace(
            $tcpFreshOwnerFixtureBlock,
            'QueueWriteIndex := 1;',
            1)
$tcpFreshOwnerNegativeFixtures['ActiveRequestPublishedValid'] =
    ([regex]::new(
        '(?i)ActiveRequestValid\s*:=\s*FALSE\s*;')).Replace(
            $tcpFreshOwnerFixtureBlock,
            'ActiveRequestValid := TRUE;',
            1)
$tcpFreshOwnerNegativeFixtures['RpcRemainsInitialized'] =
    ([regex]::new(
        '(?i)RpcInitialized\s*:=\s*FALSE\s*;')).Replace(
            $tcpFreshOwnerFixtureBlock,
            'RpcInitialized := TRUE;',
            1)
$tcpFreshOwnerNegativeFixtures['ClearPendingClosedEpoch'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)SessionEpoch\s*\+=\s*1\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}PendingClosedSessionEpoch := 0;' +
             [Environment]::NewLine +
             '${Indent}SessionEpoch += 1;'),
            1)
$tcpFreshOwnerNegativeFixtures['EarlyCurrentSocketPublish'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)ActiveRequestValid\s*:=\s*' +
         'FALSE\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}CurrentSock := dSock;' +
             [Environment]::NewLine +
             '${Indent}ActiveRequestValid := FALSE;'),
            1)
$tcpFreshOwnerNegativeFixtures['RpcReenabledAfterReset'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)RpcInitialized\s*:=\s*' +
         'FALSE\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}RpcInitialized := FALSE;' +
             [Environment]::NewLine +
             '${Indent}RpcInitialized := TRUE;'),
            1)
$tcpFreshOwnerNegativeFixtures['GuardedRpcReset'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)RpcInitialized\s*:=\s*' +
         'FALSE\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}if FALSE then' +
             [Environment]::NewLine +
             '${Indent}  RpcInitialized := FALSE;' +
             [Environment]::NewLine +
             '${Indent}end_if;'),
            1)
$tcpFreshOwnerNegativeFixtures['EpochConditionDrift'] =
    ([regex]::new(
        ('(?i)if\s+SessionEpoch\s*=\s*0\s+then'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            'if TRUE then',
            1)
$tcpFreshOwnerNegativeFixtures['QueueReactivatedAfterReset'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)_memset\s*\(\s*dest\s*:=\s*#\s*' +
         'RequestQueue\s*\[\s*0\s*\]\s*,\s*usByte\s*:=\s*0\s*,' +
         '\s*cntr\s*:=\s*sizeof\s*\(\s*RequestQueue\s*\)' +
         '\s*\)\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}_memset(dest:=#RequestQueue[0], usByte:=0, ' +
             'cntr:=sizeof(RequestQueue));' +
             [Environment]::NewLine +
             '${Indent}RequestQueue[0].State := TCPMI_QUEUE_READY;'),
            1)
$tcpFreshOwnerNegativeFixtures['EarlyReturn'] =
    ([regex]::new(
        ('(?im)^(?<Indent>[ \t]*)ActiveRequestValid\s*:=\s*' +
         'FALSE\s*;'))).Replace(
            $tcpFreshOwnerFixtureBlock,
            ('${Indent}ActiveRequestValid := FALSE;' +
             [Environment]::NewLine +
             '${Indent}RETURN;'),
            1)

$tcpFreshOwnerNegativeFixtureCount = 0
foreach ($negativeFixture in
        $tcpFreshOwnerNegativeFixtures.GetEnumerator()) {
    if ($negativeFixture.Value -ceq $tcpFreshOwnerFixtureBlock) {
        throw (
            'TCPMotionInterface fresh-owner negative fixture did not mutate ' +
            "the source for '$($negativeFixture.Key)'.")
    }
    $negativeSource = $st.Replace(
        $tcpFreshOwnerFixtureBlock,
        [string]$negativeFixture.Value)
    if ($negativeSource -ceq $st) {
        throw (
            'TCPMotionInterface fresh-owner negative fixture could not replace ' +
            "the source for '$($negativeFixture.Key)'.")
    }

    $negativeRejected = $false
    try {
        Assert-TCPMotionInterfaceFreshOwnerReset `
            -TcpText $negativeSource `
            -Owner (
                'TCPMotionInterface fresh-owner negative fixture ' +
                $negativeFixture.Key)
    }
    catch {
        $negativeRejected = $true
    }
    if (-not $negativeRejected) {
        throw (
            'TCPMotionInterface fresh-owner verifier accepted negative ' +
            "fixture '$($negativeFixture.Key)'.")
    }
    $tcpFreshOwnerNegativeFixtureCount++
}
if ($tcpFreshOwnerNegativeFixtureCount -ne 11) {
    throw (
        'TCPMotionInterface fresh-owner negative fixture count is ' +
        "$tcpFreshOwnerNegativeFixtureCount, expected 11.")
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

if ($st -match '(?m)^\s*0x208[1-4]\s*:') {
    throw 'Legacy 0x2081..0x2084 handler is active.'
}

$upperBitMappings = [regex]::Matches($st, 'AxisCommandState\$UDINT\s+and\s+0xFFFF0000').Count
if ($transportClean) {
    if ($upperBitMappings -ne 0 -or
        $st -match '(?<![A-Za-z0-9_])AxisObjectName[1-9](?![A-Za-z0-9_])' -or
        $st -match '(?<![A-Za-z0-9_])AxisCommandInputValid(?![A-Za-z0-9_])') {
        throw 'Phase5TransportClean TCPMotionInterface still contains legacy axis-domain implementation state.'
    }
}
else {
    if ($upperBitMappings -ne 4) {
        throw "32-bit axis error truncation guards=$upperBitMappings, expected 4."
    }
    Assert-Match $st 'AxisObjectName1\s*:\s*ARRAY \[0\.\.255\] OF CHAR' 'LASAL object-name buffer is not 256 bytes.'
    if ($st -match 'AxisObjectName[5-9]\s*:\s*ARRAY') {
        throw 'Axes 5..9 must reuse an IDE-registered object-name buffer instead of adding CodeGenerator-only class variables.'
    }
    Assert-Match $st '(?s)AxisCommandInputValid\s*:=.*?\(dir = 2\).*?\(bufMode = 1\).*?\(Exec = 1\)' 'Shortest-only axis direction validation is missing.'
    Assert-Match $st '(?s)\(dec = 0\).*?\(Exec = 1\)' 'MoveVelocity deceleration/execute validation is missing.'
}
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize, reference\);\s*WriteInt32\(buffer, HeaderSize \+ 4, 1\);' 'C# read-status descriptor payload is missing.'
Assert-Match $protocol 'WriteInt32\(buffer, HeaderSize \+ 64, velocity\);' 'C# group velocity offset is not 64 bytes into payload.'
Assert-Match $protocol 'WriteInt32\(\s*buffer,\s*HeaderSize \+ 92,\s*options\.Execute \? 1 : 0\s*\);' 'C# group execute option is not serialized at payload offset 92.'

$binaryMetadataResult = if ($transportClean -and
    $AllowStaleLasalBinaryMetadata) {
    '; LASAL binary metadata explicitly bypassed'
}
else {
    ''
}
if ($SourceOnly) {
    Write-Host (
        "PASS LASAL.StaticContract.SourceOnly ($ControlServiceCheckpoint; " +
        "TopologyIoCheckpoint=$TopologyIoCheckpoint; " +
        'Admin reads and 0x7D22 relative motion, CyWork queue, ' +
        'control-service checkpoint, diagnostics D1-D5, recorder bank, ' +
        'and session-close wiring' + $binaryMetadataResult + ')')
}
else {
    Write-Host (
        "PASS LASAL.StaticContract ($ControlServiceCheckpoint; " +
        "TopologyIoCheckpoint=$TopologyIoCheckpoint; " +
        'Admin reads and 0x7D22 relative motion, CyWork queue, ' +
        'control-service checkpoint, diagnostics D1-D5, nine-axis network, ' +
        'recorder wiring, and generated metadata/tables' +
        $binaryMetadataResult + ')')
}

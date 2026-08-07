[CmdletBinding(DefaultParameterSetName = 'Current')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Current')]
    [switch]$VerifyCurrent,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,

    [Parameter(ParameterSetName = 'Current')]
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..\..'),

    [Parameter(ParameterSetName = 'Current')]
    [switch]$AllowPreImportAbsent,

    [Parameter(ParameterSetName = 'Current')]
    [ValidateSet('Auto', 'Absent', 'VendorImported', 'DerivedCandidate')]
    [string]$ExpectedState = 'Auto'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Owner = 'LASAL.UdpCallbackContract'
$Utf8 = [Text.UTF8Encoding]::new($false, $true)
$DerivedCandidateApproved = $false

$TargetRootRelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
$TransceiverRelativePath =
    "$TargetRootRelativePath/Class/_UDPTransceiver/_UDPTransceiver.st"
$InterfaceRelativePath =
    "$TargetRootRelativePath/Class/_UDPTransceiverInterface/_UDPTransceiverInterface.st"
$DerivedRelativePath =
    "$TargetRootRelativePath/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st"
$TcpRelativePath =
    "$TargetRootRelativePath/Class/TCPMotionInterface/TCPMotionInterface.st"
$ClassesRelativePath = "$TargetRootRelativePath/Class/Classes.lcb"
$ProjectRelativePath = "$TargetRootRelativePath/Elmo_EtherCAT_Test_4Axis.lcb"
$ProjectDefinitionRelativePath =
    "$TargetRootRelativePath/Elmo_EtherCAT_Test_4Axis.lcp"
$GeneratedIncludeRootRelativePath = "$TargetRootRelativePath/Include"
$CommNetworkRelativePath =
    "$TargetRootRelativePath/Network/Comm_Network/Comm_Network.lcn"
$CommTableRelativePath =
    "$TargetRootRelativePath/Network/Comm_Network/ONE_Comm_Network_Table.st"
$ConfigObjectsRelativePath = "$TargetRootRelativePath/Network/ConfigObjects.st"
$NetworksDatabaseRelativePath = "$TargetRootRelativePath/Network/Networks.lcb"

$ExpectedVendor = [ordered]@{
    Transceiver = [ordered]@{
        Name = '_UDPTransceiver'
        RawDemoSha256 =
            'B3883DF82C942196EB2AA4313DEDBD7BE9430C850052140BAE323B35B272D95D'
        CanonicalLfSha256 =
            'D0D35828725B41B0E1C2323FE2120A1F492F7C6DA56254CAF9A10D07E7492DD1'
        CanonicalLfBytes = 71787
        CodeGeneratorCrLfSha256 =
            'C3713C1E76E0027F6E90007268BFC2DFA8962F778A7B2EE2B3E50C11F520C321'
        CodeGeneratorCrLfBytes = 73380
        LineBreakCount = 1593
    }
    Interface = [ordered]@{
        Name = '_UDPTransceiverInterface'
        RawDemoSha256 =
            '6FC3C64D84DDE21EEA8ADC44E89CEF3966A2597D03CD87AB799B344935E7A505'
        CanonicalLfSha256 =
            '9575ED267B9629D811E18C9A5156EC4089F8223464D42DA4ADA6F1F8E8188D80'
        CanonicalLfBytes = 27756
        CodeGeneratorCrLfSha256 =
            '544EA49B22CF5C6CEB6B316E3E4AF1DE87494F9385B193A29349CC2C1C6577B6'
        CodeGeneratorCrLfBytes = 28304
        LineBreakCount = 548
    }
}

$ProtectedDependencies = @(
    [ordered]@{
        Name = '_StdLib'
        Path = "$TargetRootRelativePath/Class/_StdLib/_StdLib.st"
        Bytes = 10412
        Sha256 =
            '53DA7E459AE214D28AB8D77CC2F1FDED9E2F7D8D552C91D71488D63DD22050EA'
    },
    [ordered]@{
        Name = 'CriticalSection'
        Path = "$TargetRootRelativePath/Class/CriticalSection/CriticalSection.st"
        Bytes = 5215
        Sha256 =
            '752ED61394D1B708176613DE8B002E197ED46D46EDB3C0BA497560D222A8B9EE'
    },
    [ordered]@{
        Name = 'lsl_st_tcp_user.h'
        Path = "$TargetRootRelativePath/Source/interfaces/lsl_st_tcp_user.h"
        Bytes = 19972
        Sha256 =
            '2DEC7C124CEC1B44766367188D5F00F6B2B812F372A3868EA1604F19C9621EDD'
    }
)

$ProtectedGeneratedRecordContracts = @(
    [ordered]@{
        Name = '_StdLib'
        SourcePath = '.\Class\_StdLib\_StdLib.st'
        ClassName = '_StdLib'
        NextSourcePath = '.\Class\_SyncMeasure\_SyncMeasure.st'
        NextClassName = '_SyncMeasure'
        Bytes = 22680
        Sha256 =
            '2339EB3663D28B6BAD53F02415823F839AD58A59AFC755CDEFB1B667E3C4CCF0'
    },
    [ordered]@{
        Name = 'CriticalSection'
        SourcePath = '.\Class\CriticalSection\CriticalSection.st'
        ClassName = 'CriticalSection'
        NextSourcePath = '.\Class\DiasMaster\DiasMaster.st'
        NextClassName = 'DiasMaster'
        Bytes = 3464
        Sha256 =
            'BEC8B29BB4D1D15532DDCB6711A1861829FC0FFFF502443D6A8E2A8AB84F614C'
    }
)

$VendorGeneratedRecordContracts = @(
    [ordered]@{
        Name = '_UDPTransceiver'
        SourcePath = '.\Class\_UDPTransceiver\_UDPTransceiver.st'
        ClassName = '_UDPTransceiver'
        NextSourcePath =
            '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st'
        NextClassName = '_UDPTransceiverInterface'
        Bytes = 52552
        Sha256 =
            '958A2EC0945A01878261A7B055A25EBB5A44AFCADDD3BE7A2309744B69F90FAB'
    },
    [ordered]@{
        Name = '_UDPTransceiverInterface'
        SourcePath =
            '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st'
        ClassName = '_UDPTransceiverInterface'
        NextSourcePath = '.\Class\ASCII_BIN\ASCII_BIN.st'
        NextClassName = 'ASCII_BIN'
        Bytes = 25583
        Sha256 =
            '7FC931079DCFBB894D29EC1A92B291E67D21A01F250B0B1639B22A82BEB614EB'
    }
)

$ExpectedAbsentClassesSha256 =
    'CA5CE9AB4B6AFB498D55CF6E5D3460A2C35D54FF8E4FE9C9D3B59636C3603F78'
$ExpectedVendorImportedClassesBytes = 8512773
$ExpectedVendorImportedClassesSha256 =
    '0CB9A3D3A4E8EB27E9A5BEB44E91D46BAEE23A051736AA83622D790249C61DC6'
$ExpectedBaselineTcpSha256 =
    '9210C199A02153FEE4110556C5396CD49C9AEAC7F22B1405AA20F00FC522A129'
$ExpectedBaselineTrackedNetworkCount = 15
$ExpectedBaselineTrackedNetworkSha256 =
    '3118354B56EB68369999D96C53603083F562E4610995BFA935D483BD2BC01CCA'
$ExpectedBaselineNetworkInventoryCount = 23
$ExpectedBaselineNetworkInventorySha256 =
    'B80867C9A0E1EF8CBB380F118B92E4E0B54B9705AA676E955A6C1CCB7A74C759'
$ExpectedVendorImportedTrackedNetworkSha256 =
    'A3D515F6F08186F5C48385EA34EE886F54D03A7A59A0925D747A4D6CD4CECDF5'
$ExpectedVendorImportedNetworkInventorySha256 =
    '36B8875A3B5848F4E8AC758E22C97FD928C27E0A66BEFEA6595416838BB30E53'
$ExpectedVendorImportedConfigObjectsBytes = 8751
$ExpectedVendorImportedConfigObjectsSha256 =
    '65B645564D285F2D058DFB80138BCB4B09FAA6BDDF143BDC5CD1FBCC1E69DAA4'
$ExpectedVendorImportedNetworksDatabaseBytes = 239778
$ExpectedVendorImportedNetworksDatabaseSha256 =
    '6D818CBAB462A8BFB7C5F080AE326D0554D9856810DCC40696805FDBBF458D42'
$ExpectedProtectedTrackedNetworkCount = 11
$ExpectedProtectedTrackedNetworkSha256 =
    'FE60FAA30C61E1CFF545E257C5581A77EF43DD6164940B4B18E4593F9A31B4E0'

$GeneratedIncludeContracts = @(
    [ordered]@{
        Name = 'C_channels.h'
        Path = "$GeneratedIncludeRootRelativePath/C_channels.h"
        AbsentCanonicalLfBytes = 23060
        AbsentCanonicalLfSha256 =
            'BAEA43E8828719CBCF5FD2C290C105A61825A5BEA20D7F4D8E9BDF0F3D0A8A6C'
        AbsentLineBreakCount = 1101
        VendorCanonicalLfBytes = 23978
        VendorCanonicalLfSha256 =
            '65838BC9C51EE6CA23019E67B887F1E39B49CD95E5D6A510CFD24F0BF7D9300D'
        VendorCrLfBytes = 25114
        VendorCrLfSha256 =
            '41DB02AD76D78D54E4105E83B2053619D015BEB1BDF67F779F42D1173D225011'
        VendorLineBreakCount = 1136
    },
    [ordered]@{
        Name = 'channels.h'
        Path = "$GeneratedIncludeRootRelativePath/channels.h"
        AbsentCanonicalLfBytes = 19615
        AbsentCanonicalLfSha256 =
            '2D72626E0FCB2209BAE1F9CD42DF51804A5AA004CC8B48CE9A9FD7C28EDA98CB'
        AbsentLineBreakCount = 789
        VendorCanonicalLfBytes = 20433
        VendorCanonicalLfSha256 =
            'AC64DB27F1E35208603542B4A65D6D19C189CF27A8F8005ABE2D044D8490B4B6'
        VendorCrLfBytes = 21247
        VendorCrLfSha256 =
            '5AF63A94429C8DB45511C38F7E75EF73612066FEDFD94FE52D8A5DF42CC2BA94'
        VendorLineBreakCount = 814
    },
    [ordered]@{
        Name = 'lslpublictypes.h'
        Path = "$GeneratedIncludeRootRelativePath/lslpublictypes.h"
        AbsentCanonicalLfBytes = 60603
        AbsentCanonicalLfSha256 =
            '0998F9257464DC2BB2FFA51225EC68978450DF107B3081AB603AFC6BA830945B'
        AbsentLineBreakCount = 2467
        VendorCanonicalLfBytes = 61906
        VendorCanonicalLfSha256 =
            '4DDCD07636E534D3B1A41B3FCCA55F76CC68EACAA413A24F9BBC14CC76820B24'
        VendorCrLfBytes = 64423
        VendorCrLfSha256 =
            'EDD3F17794126577E17A841AD1D562F1CB6D41D03D7C7A7E3690146918E029AD'
        VendorLineBreakCount = 2517
    }
)

$ExpectedCChannelStructBlocks = [ordered]@{
    SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver =
        'typedef struct SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver { ' +
        'CMDMETH *pMeth; _FSM_UDP_USER dData; SVRDSC *pDsc; } ' +
        'SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver;'
    CltChCmd__UDPTransceiver =
        'typedef struct CltChCmd__UDPTransceiver { struct ' +
        'SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver *pCh; ' +
        '_FSM_UDP_USER dData; _UDPTransceiver *pCmd; } ' +
        'CltChCmd__UDPTransceiver;'
    SvrCh__FSM_UDP_USER_PTofCls__UDPTransceiver =
        'typedef struct SvrCh__FSM_UDP_USER_PTofCls__UDPTransceiver { ' +
        'CHMETH *pMeth; _FSM_UDP_USER dData; SVRDSC *pDsc; } ' +
        'SvrCh__FSM_UDP_USER_PTofCls__UDPTransceiver;'
    SvrCh__STATE_UDP_INTF_PTofCls__UDPTransceiverInterface =
        'typedef struct ' +
        'SvrCh__STATE_UDP_INTF_PTofCls__UDPTransceiverInterface { ' +
        'CHMETH *pMeth; _STATE_UDP_INTF dData; SVRDSC *pDsc; } ' +
        'SvrCh__STATE_UDP_INTF_PTofCls__UDPTransceiverInterface;'
    SvrCh__UDP_ERROR_PTofCls__UDPTransceiver =
        'typedef struct SvrCh__UDP_ERROR_PTofCls__UDPTransceiver { ' +
        'CHMETH *pMeth; _UDP_ERROR dData; SVRDSC *pDsc; } ' +
        'SvrCh__UDP_ERROR_PTofCls__UDPTransceiver;'
}

$ExpectedStChannelStructBlocks = [ordered]@{
    SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver =
        'SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver : STRUCT ' +
        'pMeth : ^CMDMETH; dData : _UDPTransceiver::_FSM_UDP_USER; ' +
        'pDsc : ^SVRDSC; END_STRUCT;'
    CltChCmd__UDPTransceiver =
        'CltChCmd__UDPTransceiver : STRUCT pCh : ' +
        '^SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver; dData : ' +
        '_UDPTransceiver::_FSM_UDP_USER; pCmd : ^_UDPTransceiver; END_STRUCT;'
    SvrCh__FSM_UDP_USER_PTofCls__UDPTransceiver =
        'SvrCh__FSM_UDP_USER_PTofCls__UDPTransceiver : STRUCT ' +
        'pMeth : ^CHMETH; dData : _UDPTransceiver::_FSM_UDP_USER; ' +
        'pDsc : ^SVRDSC; END_STRUCT;'
    SvrCh__STATE_UDP_INTF_PTofCls__UDPTransceiverInterface =
        'SvrCh__STATE_UDP_INTF_PTofCls__UDPTransceiverInterface : STRUCT ' +
        'pMeth : ^CHMETH; dData : ' +
        '_UDPTransceiverInterface::_STATE_UDP_INTF; pDsc : ^SVRDSC; ' +
        'END_STRUCT;'
    SvrCh__UDP_ERROR_PTofCls__UDPTransceiver =
        'SvrCh__UDP_ERROR_PTofCls__UDPTransceiver : STRUCT ' +
        'pMeth : ^CHMETH; dData : _UDPTransceiver::_UDP_ERROR; ' +
        'pDsc : ^SVRDSC; END_STRUCT;'
}

$ExpectedPublicTypeBlocks = [ordered]@{
    _UDPTransceiver =
        '_UDPTransceiver : CLASS_PUBLIC TYPE _FSM_UDP_USER : ( ' +
        '_STATE_INIT_UDP, _STATE_IDLE_UDP, _STATE_SOCK_UDP, ' +
        '_STATE_BIND_SOCK_UDP, _STATE_ONLY_SEND_UDP, _STATE_RECV_UDP, ' +
        '_STATE_SEND_UDP, _STATE_SHUTDOWN_UDP, _STATE_CLOSE_SOCK_UDP, ' +
        '_STATE_ERROR_UDP )$UDINT; _UDP_ERROR : ( ' +
        '_NO_ERROR_UDP_ERROR:=0, _INIT_TCP_USER_UDP_ERROR:=4294967295, ' +
        '_NO_IP_ADDRESS_UDP_ERROR:=4294967294, ' +
        '_NO_MEMORY_SOCKET_UDP_ERROR:=4294967293, ' +
        '_NO_MEMORY_SENDBUFFER_UDP_ERROR:=4294967292, ' +
        '_INVALID_HANDLE_UDP_ERROR:=4294967291, ' +
        '_SHUTDOWN_UDP_ERROR:=4294967290, ' +
        '_CLOSESOCKET_UDP_ERROR:=4294967289, ' +
        '_ALLOCATE_SOCKET_UDP_ERROR:=4294967288, ' +
        '_SET_BIND_UDP_ERROR:=4294967287, _RECV_UDP_ERROR:=4294967286, ' +
        '_SEND_UDP_ERROR:=4294967285, _NO_LOCAL_IP_UDP_ERROR:=4294967284, ' +
        '_NO_DESTINATION_IP_UDP_ERROR:=4294967283, ' +
        '_NO_MEMORY_RECVBUFFER_UDP_ERROR:=4294967282, ' +
        '_INSTALL_CALLBACK_UDP_ERROR )$DINT; END_TYPE END_CLASS;'
    _UDPTransceiverInterface =
        '_UDPTransceiverInterface : CLASS_PUBLIC TYPE _STATE_UDP_INTF : ( ' +
        '_STATE_NO_SOCKET, _STATE_INIT_SOCKET, _STATE_ADDED_SOCKET, ' +
        '_STATE_BOUND_SOCKET, _STATE_CLOSED_SOCKET )$UDINT; ' +
        'END_TYPE END_CLASS;'
}

$AllowedDerivedNetworkPaths = @(
    $CommNetworkRelativePath,
    $CommTableRelativePath,
    $ConfigObjectsRelativePath,
    $NetworksDatabaseRelativePath
)

$ForbiddenDemoClassNames = @(
    '_SigTCPComReceive',
    '_SigTCPDataManager',
    'DataManager',
    'DataManagerFIFO',
    'DataManagerPriority',
    'RamRingBuffer',
    'ReceiveMsg',
    'TCP_MotionIF_Dummy',
    'TCPCommunication',
    'TCPCommunicationLogFilter',
    'TCPRT_DataManager',
    'UDPTransmission'
)

$ForbiddenDemoNetworkNames = @('MotionNet', 'TCPComNet', 'UDPComNet')

$PublicFunctionSpecs = @(
    [ordered]@{
        Name = 'ArmEndpoint'
        Inputs = @(
            'ProtocolVersion:UINT',
            'EventMask:UDINT',
            'CallbackIPv4:UDINT',
            'CallbackPort:DINT',
            'SessionEpoch:UDINT',
            'BootId:UDINT',
            'CookieLo:UDINT',
            'CookieHi:UDINT',
            'MaxDatagramBytes:UDINT')
        Outputs = @('Result:DINT')
    },
    [ordered]@{
        Name = 'DisarmEndpoint'
        Inputs = @(
            'ExpectedSessionEpoch:UDINT',
            'ExpectedCookieLo:UDINT',
            'ExpectedCookieHi:UDINT')
        Outputs = @('Result:DINT')
    },
    [ordered]@{
        Name = 'PublishEvent'
        Inputs = @(
            'EventMaskBit:UDINT',
            'EventType:UINT',
            'DeliveryClass:UINT',
            'EventId:UDINT',
            'ProducerSessionEpoch:UDINT',
            'pPayload:^void',
            'PayloadBytes:UDINT')
        Outputs = @('Result:DINT')
    }
)

$PrivateFunctionNames = @(
    'EnsureSocketReady',
    'ValidateEndpoint',
    'BuildDatagram',
    'FindFreeOrVictimSlot',
    'ServiceTransmitQueue',
    'SendSlot',
    'RetryOrDropSlot',
    'ClearPendingFrames',
    'FenceMatches'
)

$AllFunctionNames = @(
    @($PublicFunctionSpecs | ForEach-Object { $_.Name }) +
    $PrivateFunctionNames)

function Throw-UdpCallbackBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)

    throw "$Owner blocker: $Message"
}

function Get-BytesSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Bytes))
}

function Get-TextSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return Get-BytesSha256 -Bytes $Utf8.GetBytes($Text)
}

function ConvertTo-CanonicalLf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function ConvertFrom-StrictAsciiBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$SourceOwner
    )

    if (($Bytes.Length -ge 3) -and
        ($Bytes[0] -eq 0xEF) -and ($Bytes[1] -eq 0xBB) -and
        ($Bytes[2] -eq 0xBF)) {
        Throw-UdpCallbackBlocker "$SourceOwner has a UTF-8 BOM."
    }
    foreach ($value in $Bytes) {
        if ($value -gt 0x7F) {
            Throw-UdpCallbackBlocker "$SourceOwner contains a non-ASCII byte."
        }
    }
    return $Utf8.GetString($Bytes)
}

function Get-LexicalScanText {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $scan = [regex]::Replace(
        (ConvertTo-CanonicalLf -Text $Text),
        '(?s)\(\*.*?\*\)',
        { param($match) [regex]::Replace($match.Value, '[^\r\n]', ' ') })
    $scan = [regex]::Replace(
        $scan,
        '(?m)//[^\r\n]*',
        { param($match) ' ' * $match.Length })
    return [regex]::Replace(
        $scan,
        '"(?:[^"]|"")*"',
        { param($match) ' ' * $match.Length })
}

function Get-OrdinalCount {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [switch]$IgnoreCase
    )

    $comparison = if ($IgnoreCase) {
        [StringComparison]::OrdinalIgnoreCase
    }
    else {
        [StringComparison]::Ordinal
    }
    $count = 0
    $offset = 0
    while ($offset -lt $Text.Length) {
        $found = $Text.IndexOf($Needle, $offset, $comparison)
        if ($found -lt 0) {
            break
        }
        $count++
        $offset = $found + $Needle.Length
    }
    return $count
}

function Get-ClassDatabaseRecord {
    param(
        [Parameter(Mandatory = $true)][string]$DatabaseText,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$RecordOwner,
        [switch]$AllowAbsent
    )

    $count = Get-OrdinalCount `
        -Text $DatabaseText -Needle $SourcePath -IgnoreCase
    if ($count -eq 0 -and $AllowAbsent) {
        return ''
    }
    if ($count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$RecordOwner source record count is $count, expected 1.")
    }
    $start = $DatabaseText.IndexOf(
        $SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    $end = $DatabaseText.IndexOf(
        '.\Class\',
        $start + $SourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($end -lt 0) {
        $end = $DatabaseText.Length
    }
    return $DatabaseText.Substring($start, $end - $start)
}

function Assert-OrderedTokens {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$Tokens,
        [Parameter(Mandatory = $true)][string]$TokenOwner
    )

    $offset = 0
    foreach ($token in $Tokens) {
        $found = $Text.IndexOf($token, $offset, [StringComparison]::Ordinal)
        if ($found -lt 0) {
            Throw-UdpCallbackBlocker (
                "$TokenOwner ordered token '$token' is missing.")
        }
        $offset = $found + $token.Length
    }
}

function Get-FunctionRecords {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][ValidateSet('Declaration', 'Implementation')]
        [string]$Kind
    )

    $pattern = if ($Kind -ceq 'Declaration') {
        '(?im)^[ \t]*FUNCTION[ \t]+' +
            '(?<Modifiers>(?:(?:VIRTUAL|GLOBAL)[ \t]+)*)' +
            '(?<Name>[A-Za-z_][A-Za-z0-9_]*)\b'
    }
    else {
        '(?im)^[ \t]*FUNCTION[ \t]+' +
            '(?<Modifiers>(?:(?:VIRTUAL|GLOBAL)[ \t]+)*)' +
            'LMCUdpCallbackSender::(?<Name>[A-Za-z_][A-Za-z0-9_]*)\b'
    }
    $matches = @([regex]::Matches($Text, $pattern))
    $records = [Collections.Generic.List[object]]::new()
    for ($index = 0; $index -lt $matches.Count; $index++) {
        $match = $matches[$index]
        $name = $match.Groups['Name'].Value
        if ($AllFunctionNames -cnotcontains $name) {
            continue
        }
        $end = if ($Kind -ceq 'Declaration') {
            $boundaryRegex = [regex]::new(
                '(?im)^[ \t]*(?:FUNCTION\b|END_CLASS\s*;)')
            $next = $boundaryRegex.Match(
                $Text,
                $match.Index + $match.Length)
            if (-not $next.Success) {
                Throw-UdpCallbackBlocker "$name declaration has no boundary."
            }
            $next.Index
        }
        else {
            $endRegex = [regex]::new(
                '(?im)^[ \t]*END_FUNCTION[ \t]*$')
            $endMatch = $endRegex.Match(
                $Text,
                $match.Index + $match.Length)
            if (-not $endMatch.Success) {
                Throw-UdpCallbackBlocker "$name implementation has no END_FUNCTION."
            }
            $endMatch.Index + $endMatch.Length
        }
        $records.Add([pscustomobject]@{
                Name = $name
                Modifiers = $match.Groups['Modifiers'].Value.Trim()
                Block = $Text.Substring($match.Index, $end - $match.Index)
                Index = $match.Index
            })
    }
    return $records.ToArray()
}

function Get-VariableInventory {
    param(
        [Parameter(Mandatory = $true)][string]$FunctionBlock,
        [Parameter(Mandatory = $true)][ValidateSet('VAR_INPUT', 'VAR_OUTPUT')]
        [string]$Section,
        [Parameter(Mandatory = $true)][string]$FunctionOwner
    )

    $matches = @([regex]::Matches(
            $FunctionBlock,
            "(?ims)^[ \t]*$Section[ \t]*`$" +
                '(?<Body>.*?)^[ \t]*END_VAR[ \t]*;?[ \t]*$'))
    if ($matches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$FunctionOwner $Section count is $($matches.Count), expected 1.")
    }
    $body = $matches[0].Groups['Body'].Value
    $inventory = [Collections.Generic.List[string]]::new()
    foreach ($line in $body -split '\r?\n') {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0) {
            continue
        }
        $variable = [regex]::Match(
            $trimmed,
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                '(?<Type>\^?[A-Za-z_][A-Za-z0-9_]*)[ \t]*;[ \t]*$')
        if (-not $variable.Success) {
            Throw-UdpCallbackBlocker (
                "$FunctionOwner $Section has an unapproved declaration: $trimmed")
        }
        $inventory.Add(
            $variable.Groups['Name'].Value + ':' +
            $variable.Groups['Type'].Value)
    }
    return $inventory.ToArray()
}

function Assert-ExactInventory {
    param(
        [Parameter(Mandatory = $true)][string[]]$Actual,
        [Parameter(Mandatory = $true)][string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$InventoryOwner
    )

    if ([string]::Join('|', $Actual) -cne [string]::Join('|', $Expected)) {
        Throw-UdpCallbackBlocker (
            "$InventoryOwner is '$([string]::Join('|', $Actual))', expected " +
            "'$([string]::Join('|', $Expected))'.")
    }
}

function Assert-DerivedSourceContract {
    param([Parameter(Mandatory = $true)][string]$SourceText)

    foreach ($metadataPattern in @(
            '(?m)^[ \t]*Name[ \t]*=[ \t]*"LMCUdpCallbackSender"[ \t]*$',
            '(?m)^[ \t]*RealtimeTask[ \t]*=[ \t]*"false"[ \t]*$',
            '(?m)^[ \t]*CyclicTask[ \t]*=[ \t]*"true"[ \t]*$',
            '(?m)^[ \t]*Sigmatek[ \t]*=[ \t]*"false"[ \t]*$')) {
        if ([regex]::Matches($SourceText, $metadataPattern).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "derived class metadata drifted: $metadataPattern")
        }
    }

    $scan = Get-LexicalScanText -Text $SourceText
    if ([regex]::Matches(
            $scan,
            '(?im)^[ \t]*#pragma[ \t]+using[ \t]+' +
                '_UDPTransceiverInterface[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived class must pragma-use _UDPTransceiverInterface exactly once.')
    }
    if ([regex]::Matches(
            $scan,
            '(?im)^[ \t]*LMCUdpCallbackSender[ \t]*:[ \t]*CLASS[ \t]*$' +
                '[\r\n]+[ \t]*:[ \t]*_UDPTransceiverInterface[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived class base is not exact _UDPTransceiverInterface.')
    }

    $classMatch = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*LMCUdpCallbackSender[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    if (-not $classMatch.Success) {
        Throw-UdpCallbackBlocker 'derived declaration class block is missing.'
    }
    $declarations = @(Get-FunctionRecords `
            -Text $classMatch.Value -Kind Declaration)
    $implementations = @(Get-FunctionRecords `
            -Text $scan -Kind Implementation)
    Assert-ExactInventory `
        -Actual @($declarations.Name) `
        -Expected $AllFunctionNames `
        -InventoryOwner 'derived declaration function order'
    Assert-ExactInventory `
        -Actual @($implementations.Name) `
        -Expected $AllFunctionNames `
        -InventoryOwner 'derived implementation function order'

    foreach ($spec in $PublicFunctionSpecs) {
        $declaration = @($declarations | Where-Object {
                $_.Name -ceq $spec.Name
            })
        $implementation = @($implementations | Where-Object {
                $_.Name -ceq $spec.Name
            })
        if (($declaration.Count -ne 1) -or ($implementation.Count -ne 1)) {
            Throw-UdpCallbackBlocker (
                "$($spec.Name) public declaration/implementation count drifted.")
        }
        foreach ($record in @($declaration[0], $implementation[0])) {
            if ($record.Modifiers -cne 'GLOBAL') {
                Throw-UdpCallbackBlocker (
                    "$($spec.Name) must be GLOBAL and not VIRTUAL GLOBAL.")
            }
            Assert-ExactInventory `
                -Actual @(Get-VariableInventory `
                    -FunctionBlock $record.Block `
                    -Section VAR_INPUT `
                    -FunctionOwner $spec.Name) `
                -Expected @($spec.Inputs) `
                -InventoryOwner "$($spec.Name) input ABI"
            Assert-ExactInventory `
                -Actual @(Get-VariableInventory `
                    -FunctionBlock $record.Block `
                    -Section VAR_OUTPUT `
                    -FunctionOwner $spec.Name) `
                -Expected @($spec.Outputs) `
                -InventoryOwner "$($spec.Name) output ABI"
        }
    }

    foreach ($name in $PrivateFunctionNames) {
        $declaration = @($declarations | Where-Object { $_.Name -ceq $name })
        $implementation = @($implementations | Where-Object { $_.Name -ceq $name })
        if (($declaration.Count -ne 1) -or ($implementation.Count -ne 1)) {
            Throw-UdpCallbackBlocker (
                "$name private declaration/implementation count drifted.")
        }
        if (($declaration[0].Modifiers.Length -ne 0) -or
            ($implementation[0].Modifiers.Length -ne 0)) {
            Throw-UdpCallbackBlocker (
                "$name must remain private without GLOBAL or VIRTUAL GLOBAL.")
        }
        foreach ($record in @($declaration[0], $implementation[0])) {
            if (($record.Block -match '(?im)^[ \t]*VAR_INPUT[ \t]*$') -or
                ($record.Block -match '(?im)^[ \t]*VAR_OUTPUT[ \t]*$')) {
                Throw-UdpCallbackBlocker (
                    "$name private ABI must have no inputs or outputs.")
            }
        }
    }

    if ($scan -match '(?i)(?<![A-Za-z0-9_])' +
        '(?:MallocV1|Malloc|Realloc|Free)[ \t]*\(') {
        Throw-UdpCallbackBlocker (
            'derived source contains forbidden dynamic allocation.')
    }
    if ($scan -match '(?i)\bbDirect[ \t]*:=[ \t]*TRUE\b') {
        Throw-UdpCallbackBlocker (
            'derived source selects forbidden direct UDP send.')
    }
}

function Assert-TcpDerivedClientContract {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    if ([regex]::Matches(
            $TcpSource,
            '<Client[ \t]+Name="CallbackSender"[ \t]+' +
                'Required="false"[ \t]+Internal="false"[^>]*/>').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface CallbackSender metadata is not exact optional external.')
    }
    $scan = Get-LexicalScanText -Text $TcpSource
    if ([regex]::Matches(
            $scan,
            '(?im)^[ \t]*CallbackSender[ \t]*:[ \t]*' +
                'CltChCmd_LMCUdpCallbackSender[ \t]*;[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface CallbackSender declaration is not exact.')
    }
}

function Assert-DerivedNetworkContract {
    param(
        [Parameter(Mandatory = $true)][string]$CommNetworkText,
        [Parameter(Mandatory = $true)][string]$CommTableText,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseText
    )

    try {
        [xml]$xml = $CommNetworkText
    }
    catch {
        Throw-UdpCallbackBlocker (
            "Comm_Network XML cannot be parsed: $($_.Exception.Message)")
    }
    $objects = @($xml.SelectNodes('/Network/Components/Object'))
    $transceivers = @($objects | Where-Object {
            $_.GetAttribute('Name') -ceq 'LMCUdpTransceiver1'
        })
    $senders = @($objects | Where-Object {
            $_.GetAttribute('Name') -ceq 'LMCUdpCallbackSender1'
        })
    if (($transceivers.Count -ne 1) -or
        ($transceivers[0].GetAttribute('Class') -cne '_UDPTransceiver')) {
        Throw-UdpCallbackBlocker (
            'Comm_Network needs one exact LMCUdpTransceiver1 object.')
    }
    if (($senders.Count -ne 1) -or
        ($senders[0].GetAttribute('Class') -cne 'LMCUdpCallbackSender')) {
        Throw-UdpCallbackBlocker (
            'Comm_Network needs one exact LMCUdpCallbackSender1 object.')
    }
    if (@($objects | Where-Object {
                $_.GetAttribute('Name') -ceq '_UDPTransceiver1'
            }).Count -ne 0) {
        Throw-UdpCallbackBlocker (
            'Comm_Network contains forbidden demo-style _UDPTransceiver1.')
    }
    foreach ($forbidden in $ForbiddenDemoClassNames) {
        if (@($objects | Where-Object {
                    $_.GetAttribute('Class') -ceq $forbidden
                }).Count -ne 0) {
            Throw-UdpCallbackBlocker (
                "Comm_Network contains forbidden demo class $forbidden.")
        }
    }

    foreach ($buffer in @(
            @{ Name = 'cSizeOfRXBuffer'; Value = '512' },
            @{ Name = 'cSizeOfTXBuffer'; Value = '8 kb' })) {
        $clients = @($transceivers[0].SelectNodes(
                "./Channels/Client[@Name='$($buffer.Name)']"))
        if (($clients.Count -ne 1) -or
            ($clients[0].GetAttribute('Value') -cne $buffer.Value)) {
            Throw-UdpCallbackBlocker (
                "LMCUdpTransceiver1.$($buffer.Name) must equal $($buffer.Value).")
        }
    }

    $connections = @($xml.SelectNodes('/Network/Connections/Connection'))
    $allowedConnections = @(
        ('LMCUdpCallbackSender1._UDPTransceiver|' +
            'LMCUdpTransceiver1.sControl')
        ('TCPMotionInterface1.CallbackSender|' +
            'LMCUdpCallbackSender1.ClassSvr'))
    $observedRelevant = [Collections.Generic.List[string]]::new()
    foreach ($connection in $connections) {
        $identity = $connection.GetAttribute('Source') + '|' +
            $connection.GetAttribute('Destination')
        if ($identity -match '(?:LMCUdp|CallbackSender|_UDPTransceiver1)') {
            $observedRelevant.Add($identity)
        }
    }
    Assert-ExactInventory `
        -Actual @($observedRelevant | Sort-Object) `
        -Expected @($allowedConnections | Sort-Object) `
        -InventoryOwner 'UDP callback Network connection inventory'

    foreach ($generatedText in @($CommTableText, $NetworksDatabaseText)) {
        foreach ($token in @(
                'LMCUdpTransceiver1',
                '_UDPTransceiver',
                'LMCUdpCallbackSender1',
                'LMCUdpCallbackSender',
                'CallbackSender')) {
            if ($generatedText.IndexOf(
                    $token,
                    [StringComparison]::Ordinal) -lt 0) {
                Throw-UdpCallbackBlocker (
                    "generated Network metadata lacks $token.")
            }
        }
    }
}

function Assert-VendorImportedNetworkRegistryContract {
    param(
        [Parameter(Mandatory = $true)][string]$ConfigObjectsText,
        [Parameter(Mandatory = $true)][int]$ConfigObjectsBytes,
        [Parameter(Mandatory = $true)][string]$ConfigObjectsSha256,
        [Parameter(Mandatory = $true)][int]$NetworksDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseSha256
    )

    if (($ConfigObjectsBytes -ne $ExpectedVendorImportedConfigObjectsBytes) -or
        ($ConfigObjectsSha256 -cne
            $ExpectedVendorImportedConfigObjectsSha256)) {
        Throw-UdpCallbackBlocker (
            'Gate A ConfigObjects.st generated registry drifted.')
    }
    if (($NetworksDatabaseBytes -ne
            $ExpectedVendorImportedNetworksDatabaseBytes) -or
        ($NetworksDatabaseSha256 -cne
            $ExpectedVendorImportedNetworksDatabaseSha256)) {
        Throw-UdpCallbackBlocker (
            'Gate A Networks.lcb generated registry drifted.')
    }

    $scan = ConvertTo-CanonicalLf -Text $ConfigObjectsText
    if ([regex]::Matches(
            $scan,
            '(?m)^0\$UINT, [0-9]+, [0-9]+, "[^"]+",[ \t]*$').Count -ne
        119) {
        Throw-UdpCallbackBlocker (
            'Gate A ConfigObjects.st class registry count is not 119.')
    }
    foreach ($row in @(
            '0$UINT, 1, 2, "_UDPTRANSCEIVER",',
            '0$UINT, 1, 3, "_UDPTRANSCEIVERINTERFACE",')) {
        $pattern = '(?m)^' + [regex]::Escape($row) + '[ \t]*$'
        if ([regex]::Matches($scan, $pattern).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "Gate A ConfigObjects.st row is not exact: $row")
        }
    }
    foreach ($residue in @(
            'LMCUDPCALLBACKSENDER',
            'LMCUDPTRANSCEIVER1',
            '_UDPTRANSCEIVER1',
            'UDPCOMNET',
            'UDPTRANSMISSION')) {
        if ($scan.IndexOf($residue, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Throw-UdpCallbackBlocker (
                "Gate A ConfigObjects.st contains premature or demo residue $residue.")
        }
    }
}

function Test-ClassDatabaseByteSequence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$DatabaseBytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][byte[]]$ExpectedBytes
    )

    if (($Start -lt 0) -or
        (($Start + $ExpectedBytes.Count) -gt $DatabaseBytes.Count)) {
        return $false
    }
    for ($index = 0; $index -lt $ExpectedBytes.Count; $index++) {
        if ($DatabaseBytes[$Start + $index] -ne $ExpectedBytes[$index]) {
            return $false
        }
    }
    return $true
}

function Assert-ClassDatabaseMethodHeader {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][byte]$VirtualFlag,
        [Parameter(Mandatory = $true)][byte]$GlobalFlag,
        [Parameter(Mandatory = $true)][uint32]$InputCount,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $header = [byte[]]@(
        0x0B, 0x00, 0x00, 0x00,
        $VirtualFlag, $GlobalFlag, 0x00, 0x00,
        [byte]($InputCount -band 0xFF),
        [byte](($InputCount -shr 8) -band 0xFF),
        [byte](($InputCount -shr 16) -band 0xFF),
        [byte](($InputCount -shr 24) -band 0xFF))
    $count = 0
    $searchStart = 0
    while ($searchStart -lt $RecordText.Length) {
        $nameStart = $RecordText.IndexOf(
            $FunctionName,
            $searchStart,
            [StringComparison]::Ordinal)
        if ($nameStart -lt 0) {
            break
        }
        if (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($nameStart + $FunctionName.Length) `
                -ExpectedBytes $header) {
            $count++
        }
        $searchStart = $nameStart + $FunctionName.Length
    }
    if ($count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$RecordOwner exact generated method header count is $count, expected 1.")
    }
}

function Get-ClassDatabaseRecordByteView {
    param(
        [Parameter(Mandatory = $true)][byte[]]$DatabaseBytes,
        [Parameter(Mandatory = $true)][string]$DatabaseText,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $text = Get-ClassDatabaseRecord `
        -DatabaseText $DatabaseText `
        -SourcePath $SourcePath `
        -RecordOwner $RecordOwner
    $start = $DatabaseText.IndexOf(
        $SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    $bytes = [byte[]]::new($text.Length)
    [Array]::Copy($DatabaseBytes, $start, $bytes, 0, $text.Length)
    return [pscustomobject]@{ Bytes = $bytes; Text = $text }
}

function Get-ClassDatabaseBoundedRecordByteView {
    param(
        [Parameter(Mandatory = $true)][byte[]]$DatabaseBytes,
        [Parameter(Mandatory = $true)][string]$DatabaseText,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Contract,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    if ($DatabaseBytes.Count -ne $DatabaseText.Length) {
        Throw-UdpCallbackBlocker "$RecordOwner byte/text offsets diverged."
    }
    if ((Get-OrdinalCount `
            -Text $DatabaseText -Needle $Contract.SourcePath -IgnoreCase) -ne 1) {
        Throw-UdpCallbackBlocker "$RecordOwner source path count is not 1."
    }
    $sourceStart = $DatabaseText.IndexOf(
        $Contract.SourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    $startWindow = [Math]::Max(0, $sourceStart - 500)
    $recordStart = $DatabaseText.IndexOf(
        $Contract.ClassName,
        $startWindow,
        [StringComparison]::Ordinal)
    if (($recordStart -lt $startWindow) -or ($recordStart -ge $sourceStart)) {
        Throw-UdpCallbackBlocker "$RecordOwner start boundary is missing."
    }

    $nextSourceStart = $DatabaseText.IndexOf(
        $Contract.NextSourcePath,
        $sourceStart + $Contract.SourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($nextSourceStart -lt 0) {
        Throw-UdpCallbackBlocker "$RecordOwner next-source boundary is missing."
    }
    $endWindow = [Math]::Max(
        $sourceStart + $Contract.SourcePath.Length,
        $nextSourceStart - 500)
    $recordEnd = $DatabaseText.IndexOf(
        $Contract.NextClassName,
        $endWindow,
        [StringComparison]::Ordinal)
    if (($recordEnd -lt $endWindow) -or ($recordEnd -ge $nextSourceStart)) {
        Throw-UdpCallbackBlocker "$RecordOwner end boundary is missing."
    }
    $length = $recordEnd - $recordStart
    $bytes = [byte[]]::new($length)
    [Array]::Copy($DatabaseBytes, $recordStart, $bytes, 0, $length)
    return [pscustomobject]@{
        Bytes = $bytes
        Text = $DatabaseText.Substring($recordStart, $length)
        SourceOffset = $sourceStart - $recordStart
    }
}

function Get-ProtectedGeneratedRecordEvidence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText
    )

    if ($ClassesDatabaseBytes.Count -ne $ClassesDatabaseText.Length) {
        Throw-UdpCallbackBlocker (
            'Classes.lcb protected-record byte/text offsets diverged.')
    }
    $records = [Collections.Generic.List[object]]::new()
    foreach ($contract in $ProtectedGeneratedRecordContracts) {
        if ((Get-OrdinalCount `
                -Text $ClassesDatabaseText `
                -Needle $contract.SourcePath `
                -IgnoreCase) -ne 1) {
            Throw-UdpCallbackBlocker (
                "$($contract.Name) protected source record count drifted.")
        }
        $sourceStart = $ClassesDatabaseText.IndexOf(
            $contract.SourcePath,
            [StringComparison]::OrdinalIgnoreCase)
        $startWindow = [Math]::Max(0, $sourceStart - 500)
        $recordStart = $ClassesDatabaseText.IndexOf(
            $contract.ClassName,
            $startWindow,
            [StringComparison]::Ordinal)
        if (($recordStart -lt $startWindow) -or ($recordStart -ge $sourceStart)) {
            Throw-UdpCallbackBlocker (
                "$($contract.Name) protected record start boundary is missing.")
        }

        $nextSourceStart = $ClassesDatabaseText.IndexOf(
            $contract.NextSourcePath,
            $sourceStart + $contract.SourcePath.Length,
            [StringComparison]::OrdinalIgnoreCase)
        if ($nextSourceStart -lt 0) {
            Throw-UdpCallbackBlocker (
                "$($contract.Name) protected next-source boundary is missing.")
        }
        $endWindow = [Math]::Max(
            $sourceStart + $contract.SourcePath.Length,
            $nextSourceStart - 500)
        $recordEnd = $ClassesDatabaseText.IndexOf(
            $contract.NextClassName,
            $endWindow,
            [StringComparison]::Ordinal)
        if (($recordEnd -lt $endWindow) -or ($recordEnd -ge $nextSourceStart)) {
            Throw-UdpCallbackBlocker (
                "$($contract.Name) protected record end boundary is missing.")
        }
        $length = $recordEnd - $recordStart
        $bytes = [byte[]]::new($length)
        [Array]::Copy(
            $ClassesDatabaseBytes,
            $recordStart,
            $bytes,
            0,
            $length)
        $records.Add([pscustomobject]@{
                Name = $contract.Name
                Bytes = $length
                Sha256 = Get-BytesSha256 -Bytes $bytes
            })
    }
    return $records.ToArray()
}

function Get-VendorGeneratedRecordEvidence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText
    )

    $records = [Collections.Generic.List[object]]::new()
    foreach ($contract in $VendorGeneratedRecordContracts) {
        $record = Get-ClassDatabaseBoundedRecordByteView `
            -DatabaseBytes $ClassesDatabaseBytes `
            -DatabaseText $ClassesDatabaseText `
            -Contract $contract `
            -RecordOwner "$($contract.Name) Classes.lcb"
        $records.Add([pscustomobject]@{
                Name = $contract.Name
                Bytes = $record.Bytes.Count
                Sha256 = Get-BytesSha256 -Bytes $record.Bytes
            })
    }
    return $records.ToArray()
}

function Assert-VendorGeneratedRecordContract {
    param([Parameter(Mandatory = $true)][object[]]$Observed)

    if ($Observed.Count -ne $VendorGeneratedRecordContracts.Count) {
        Throw-UdpCallbackBlocker (
            'vendor generated Classes.lcb record inventory count drifted.')
    }
    foreach ($expected in $VendorGeneratedRecordContracts) {
        $matches = @($Observed | Where-Object { $_.Name -ceq $expected.Name })
        if (($matches.Count -ne 1) -or
            ($matches[0].Bytes -ne $expected.Bytes) -or
            ($matches[0].Sha256 -cne $expected.Sha256)) {
            Throw-UdpCallbackBlocker (
                "vendor generated $($expected.Name) Classes.lcb record drifted.")
        }
    }
}

function Assert-VendorRecordUnknownHeaderContract {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Record,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $unknowns = @([regex]::Matches(
            $Record.Text,
            '(?i)<Unknown[^>\x00\r\n]*>'))
    if (($unknowns.Count -ne 1) -or
        ($unknowns[0].Value -cne '<Unknown>') -or
        ($unknowns[0].Index -ge $Record.SourceOffset)) {
        Throw-UdpCallbackBlocker (
            "$RecordOwner generated Unknown header contract drifted.")
    }
}

function Assert-VendorGeneratedAbiContract {
    param(
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText
    )

    $transceiverContract = @($VendorGeneratedRecordContracts | Where-Object {
            $_.Name -ceq '_UDPTransceiver'
        })[0]
    $transceiver = Get-ClassDatabaseBoundedRecordByteView `
        -DatabaseBytes $ClassesDatabaseBytes `
        -DatabaseText $ClassesDatabaseText `
        -Contract $transceiverContract `
        -RecordOwner '_UDPTransceiver Classes.lcb'
    Assert-VendorRecordUnknownHeaderContract `
        -Record $transceiver `
        -RecordOwner '_UDPTransceiver Classes.lcb'
    Assert-OrderedTokens `
        -Text $transceiver.Text `
        -Tokens @('sControl') `
        -TokenOwner '_UDPTransceiver generated channel ABI'
    Assert-ClassDatabaseMethodHeader `
        -RecordBytes $transceiver.Bytes `
        -RecordText $transceiver.Text `
        -FunctionName 'SendData' `
        -VirtualFlag 1 `
        -GlobalFlag 1 `
        -InputCount 6 `
        -RecordOwner '_UDPTransceiver.SendData Classes.lcb'

    $interfaceContract = @($VendorGeneratedRecordContracts | Where-Object {
            $_.Name -ceq '_UDPTransceiverInterface'
        })[0]
    $interface = Get-ClassDatabaseBoundedRecordByteView `
        -DatabaseBytes $ClassesDatabaseBytes `
        -DatabaseText $ClassesDatabaseText `
        -Contract $interfaceContract `
        -RecordOwner '_UDPTransceiverInterface Classes.lcb'
    Assert-VendorRecordUnknownHeaderContract `
        -Record $interface `
        -RecordOwner '_UDPTransceiverInterface Classes.lcb'
    Assert-OrderedTokens `
        -Text $interface.Text `
        -Tokens @('ClassSvr', '_UDPTransceiver') `
        -TokenOwner '_UDPTransceiverInterface generated channel ABI'
    $methodCounts = [ordered]@{
        AddSocket = 0
        BindSocket = 2
        DelSocket = 0
        SendData = 5
        FLUSHRingbuffer = 0
        IsOpen = 0
        ConvertStrToUdint = 1
        ConvertUdintToStr = 3
        GetIpInfo = 3
        SendDataBlocked = 5
        Response = 4
        InfoCallback = 3
        ErrorCallback = 3
    }
    foreach ($entry in $methodCounts.GetEnumerator()) {
        Assert-ClassDatabaseMethodHeader `
            -RecordBytes $interface.Bytes `
            -RecordText $interface.Text `
            -FunctionName $entry.Key `
            -VirtualFlag 1 `
            -GlobalFlag 1 `
            -InputCount ([uint32]$entry.Value) `
            -RecordOwner "_UDPTransceiverInterface.$($entry.Key) Classes.lcb"
    }
}

function Assert-ClassDatabaseFunctionAbiRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [bool]$IsVirtual = $false,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Inputs,
        [AllowEmptyCollection()]
        [string[]]$Outputs = @(),
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    if ($RecordBytes.Count -ne $RecordText.Length) {
        Throw-UdpCallbackBlocker "$RecordOwner byte/text offsets diverged."
    }
    if ($Outputs.Count -gt 1) {
        Throw-UdpCallbackBlocker "$RecordOwner verifier supports at most one output."
    }
    $inputCount = [uint32]$Inputs.Count
    $virtual = if ($IsVirtual) { 1 } else { 0 }
    $scope = if ($IsGlobal) { 1 } else { 0 }
    $header = [byte[]]@(
        0x0B, 0x00, 0x00, 0x00,
        [byte]$virtual, [byte]$scope, 0x00, 0x00,
        [byte]($inputCount -band 0xFF),
        [byte](($inputCount -shr 8) -band 0xFF),
        [byte](($inputCount -shr 16) -band 0xFF),
        [byte](($inputCount -shr 24) -band 0xFF))
    $candidates = [Collections.Generic.List[int]]::new()
    $searchStart = 0
    while ($searchStart -lt $RecordText.Length) {
        $nameStart = $RecordText.IndexOf(
            $FunctionName,
            $searchStart,
            [StringComparison]::Ordinal)
        if ($nameStart -lt 0) {
            break
        }
        if (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($nameStart + $FunctionName.Length) `
                -ExpectedBytes $header) {
            $candidates.Add($nameStart)
        }
        $searchStart = $nameStart + $FunctionName.Length
    }
    if ($candidates.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$RecordOwner exact generated ABI record count is " +
            "$($candidates.Count), expected 1.")
    }

    $headerEnd = $candidates[0] + $FunctionName.Length + $header.Count
    $exactMetadata = [Collections.Generic.List[byte]]::new()
    foreach ($entry in $Inputs) {
        $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
        if ($separator -lt 1) {
            Throw-UdpCallbackBlocker "$RecordOwner has an invalid verifier input spec."
        }
        $name = $entry.Substring(0, $separator)
        $type = $entry.Substring($separator + 1)
        $nameLength = [uint32]$name.Length
        Add-BytesToList -List $exactMetadata -Bytes ([byte[]]@(
                0x00, 0x01,
                [byte]($nameLength -band 0xFF),
                [byte](($nameLength -shr 8) -band 0xFF),
                [byte](($nameLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $exactMetadata -Text $name
        $typeLength = [uint32]$type.Length
        Add-BytesToList -List $exactMetadata -Bytes ([byte[]]@(
                [byte]($typeLength -band 0xFF),
                [byte](($typeLength -shr 8) -band 0xFF),
                [byte](($typeLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $exactMetadata -Text $type
    }
    if ($Outputs.Count -eq 0) {
        Add-BytesToList `
            -List $exactMetadata -Bytes ([byte[]]@(0x00, 0x00, 0x00, 0x00))
    }
    else {
        $entry = $Outputs[0]
        $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
        if ($separator -lt 1) {
            Throw-UdpCallbackBlocker "$RecordOwner has an invalid verifier output spec."
        }
        $name = $entry.Substring(0, $separator)
        $type = $entry.Substring($separator + 1)
        $nameLength = [uint32]$name.Length
        Add-BytesToList -List $exactMetadata -Bytes ([byte[]]@(
                0x01, 0x00, 0x00, 0x00,
                0x00, 0x01,
                [byte]($nameLength -band 0xFF),
                [byte](($nameLength -shr 8) -band 0xFF),
                [byte](($nameLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $exactMetadata -Text $name
        $typeLength = [uint32]$type.Length
        Add-BytesToList -List $exactMetadata -Bytes ([byte[]]@(
                [byte]($typeLength -band 0xFF),
                [byte](($typeLength -shr 8) -band 0xFF),
                [byte](($typeLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $exactMetadata -Text $type
    }
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $headerEnd `
                -ExpectedBytes $exactMetadata.ToArray())) {
        Throw-UdpCallbackBlocker "$RecordOwner exact parameter metadata drifted."
    }
    $cursor = $headerEnd
    $expectedTokens = [Collections.Generic.List[string]]::new()
    foreach ($entry in $Inputs) {
        $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
        if ($separator -lt 1) {
            Throw-UdpCallbackBlocker "$RecordOwner has an invalid verifier input spec."
        }
        $name = $entry.Substring(0, $separator)
        $type = $entry.Substring($separator + 1)
        $nameStart = $RecordText.IndexOf(
            $name,
            $cursor,
            [StringComparison]::Ordinal)
        if ($nameStart -lt 0) {
            Throw-UdpCallbackBlocker "$RecordOwner input $name is missing."
        }
        $nameLength = [uint32]$name.Length
        $namePrefix = [byte[]]@(
            0x00, 0x01,
            [byte]($nameLength -band 0xFF),
            [byte](($nameLength -shr 8) -band 0xFF),
            [byte](($nameLength -shr 16) -band 0xFF),
            0xAA)
        if (-not (Test-ClassDatabaseByteSequence `
                    -DatabaseBytes $RecordBytes `
                    -Start ($nameStart - $namePrefix.Count) `
                    -ExpectedBytes $namePrefix)) {
            Throw-UdpCallbackBlocker "$RecordOwner input $name metadata drifted."
        }
        $typeStart = $RecordText.IndexOf(
            $type,
            $nameStart + $name.Length,
            [StringComparison]::Ordinal)
        if ($typeStart -lt 0) {
            Throw-UdpCallbackBlocker "$RecordOwner input $name type $type is missing."
        }
        $typeLength = [uint32]$type.Length
        $typePrefix = [byte[]]@(
            [byte]($typeLength -band 0xFF),
            [byte](($typeLength -shr 8) -band 0xFF),
            [byte](($typeLength -shr 16) -band 0xFF),
            0xAA)
        if (-not (Test-ClassDatabaseByteSequence `
                    -DatabaseBytes $RecordBytes `
                    -Start ($typeStart - $typePrefix.Count) `
                    -ExpectedBytes $typePrefix)) {
            Throw-UdpCallbackBlocker "$RecordOwner input $name type metadata drifted."
        }
        $expectedTokens.Add($name)
        $expectedTokens.Add($type)
        $cursor = $typeStart + $type.Length
    }

    if ($Outputs.Count -eq 0) {
        if (-not (Test-ClassDatabaseByteSequence `
                    -DatabaseBytes $RecordBytes `
                    -Start $cursor `
                    -ExpectedBytes ([byte[]]@(0x00, 0x00, 0x00, 0x00)))) {
            Throw-UdpCallbackBlocker "$RecordOwner generated output count is not zero."
        }
        return
    }

    $outputEntry = $Outputs[0]
    $outputSeparator = $outputEntry.IndexOf(':', [StringComparison]::Ordinal)
    if ($outputSeparator -lt 1) {
        Throw-UdpCallbackBlocker "$RecordOwner has an invalid verifier output spec."
    }
    $outputName = $outputEntry.Substring(0, $outputSeparator)
    $outputType = $outputEntry.Substring($outputSeparator + 1)
    $outputNameStart = $RecordText.IndexOf(
        $outputName,
        $cursor,
        [StringComparison]::Ordinal)
    if ($outputNameStart -lt 0) {
        Throw-UdpCallbackBlocker "$RecordOwner output $outputName is missing."
    }
    $outputNameLength = [uint32]$outputName.Length
    $outputPrefix = [byte[]]@(
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x01,
        [byte]($outputNameLength -band 0xFF),
        [byte](($outputNameLength -shr 8) -band 0xFF),
        [byte](($outputNameLength -shr 16) -band 0xFF),
        0xAA)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($outputNameStart - $outputPrefix.Count) `
                -ExpectedBytes $outputPrefix)) {
        Throw-UdpCallbackBlocker "$RecordOwner output $outputName metadata drifted."
    }
    $outputTypeStart = $RecordText.IndexOf(
        $outputType,
        $outputNameStart + $outputName.Length,
        [StringComparison]::Ordinal)
    if ($outputTypeStart -lt 0) {
        Throw-UdpCallbackBlocker "$RecordOwner output type $outputType is missing."
    }
    $outputTypeLength = [uint32]$outputType.Length
    $outputTypePrefix = [byte[]]@(
        [byte]($outputTypeLength -band 0xFF),
        [byte](($outputTypeLength -shr 8) -band 0xFF),
        [byte](($outputTypeLength -shr 16) -band 0xFF),
        0xAA)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($outputTypeStart - $outputTypePrefix.Count) `
                -ExpectedBytes $outputTypePrefix)) {
        Throw-UdpCallbackBlocker "$RecordOwner output type metadata drifted."
    }
    $expectedTokens.Add($outputName)
    $expectedTokens.Add($outputType)
    $segment = $RecordText.Substring(
        $headerEnd,
        ($outputTypeStart + $outputType.Length) - $headerEnd)
    $actualTokens = @(
        [regex]::Matches(
            $segment,
            '(?<![A-Za-z0-9_])(?<Token>\^?[A-Za-z_][A-Za-z0-9_]*)(?![A-Za-z0-9_])') |
            ForEach-Object { $_.Groups['Token'].Value })
    Assert-ExactInventory `
        -Actual $actualTokens `
        -Expected $expectedTokens.ToArray() `
        -InventoryOwner "$RecordOwner generated parameter token inventory"
}

function Assert-GeneratedDerivedMetadata {
    param(
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText
    )

    $derivedPath = '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'
    $record = Get-ClassDatabaseRecord `
        -DatabaseText $ClassesDatabaseText `
        -SourcePath $derivedPath `
        -RecordOwner 'LMCUdpCallbackSender Classes.lcb'
    $recordStart = $ClassesDatabaseText.IndexOf(
        $derivedPath,
        [StringComparison]::OrdinalIgnoreCase)
    $recordBytes = [byte[]]::new($record.Length)
    [Array]::Copy(
        $ClassesDatabaseBytes,
        $recordStart,
        $recordBytes,
        0,
        $record.Length)
    $positions = [Collections.Generic.List[int]]::new()
    foreach ($name in $AllFunctionNames) {
        $count = Get-OrdinalCount -Text $record -Needle $name
        if ($count -ne 1) {
            Throw-UdpCallbackBlocker (
                "Classes.lcb $name count is $count, expected 1.")
        }
        $positions.Add(
            $record.IndexOf($name, [StringComparison]::Ordinal))
    }
    for ($index = 1; $index -lt $positions.Count; $index++) {
        if ($positions[$index] -le $positions[$index - 1]) {
            Throw-UdpCallbackBlocker (
                'Classes.lcb sender function order drifted.')
        }
    }
    foreach ($spec in $PublicFunctionSpecs) {
        $start = $record.IndexOf($spec.Name, [StringComparison]::Ordinal)
        $nextPositions = @($positions | Where-Object { $_ -gt $start })
        $end = if ($nextPositions.Count -gt 0) {
            ($nextPositions | Measure-Object -Minimum).Minimum
        }
        else {
            $record.Length
        }
        $methodRecord = $record.Substring($start, $end - $start)
        $tokens = [Collections.Generic.List[string]]::new()
        $tokens.Add($spec.Name)
        foreach ($entry in @($spec.Inputs + $spec.Outputs)) {
            $parts = $entry.Split(':', 2)
            $tokens.Add($parts[0])
            $tokens.Add($parts[1])
        }
        Assert-OrderedTokens `
            -Text $methodRecord `
            -Tokens $tokens.ToArray() `
            -TokenOwner "Classes.lcb $($spec.Name) ABI"
        Assert-ClassDatabaseFunctionAbiRecord `
            -RecordBytes $recordBytes `
            -RecordText $record `
            -FunctionName $spec.Name `
            -IsGlobal $true `
            -Inputs @($spec.Inputs) `
            -Outputs @($spec.Outputs) `
            -RecordOwner "Classes.lcb $($spec.Name)"
    }
    foreach ($name in $PrivateFunctionNames) {
        Assert-ClassDatabaseFunctionAbiRecord `
            -RecordBytes $recordBytes `
            -RecordText $record `
            -FunctionName $name `
            -IsGlobal $false `
            -Inputs @() `
            -Outputs @() `
            -RecordOwner "Classes.lcb $name"
    }

    $tcpRecord = Get-ClassDatabaseRecord `
        -DatabaseText $ClassesDatabaseText `
        -SourcePath '.\Class\TCPMotionInterface\TCPMotionInterface.st' `
        -RecordOwner 'TCPMotionInterface Classes.lcb'
    if ([regex]::Matches(
            $tcpRecord,
            '(?<![A-Za-z0-9_])CallbackSender(?![A-Za-z0-9_])').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'Classes.lcb TCPMotionInterface CallbackSender count is not 1.')
    }
    Assert-OrderedTokens `
        -Text $tcpRecord `
        -Tokens @('CallbackSender', 'LMCUdpCallbackSender') `
        -TokenOwner 'Classes.lcb TCPMotionInterface CallbackSender ABI'
}

function Assert-VendorGeneratedRepresentation {
    param(
        [Parameter(Mandatory = $true)][string]$CanonicalLfSha256,
        [Parameter(Mandatory = $true)][string]$RawSha256,
        [Parameter(Mandatory = $true)][int]$RawBytes,
        [Parameter(Mandatory = $true)][string]$EolStyle,
        [Parameter(Mandatory = $true)][int]$LineBreakCount,
        [Parameter(Mandatory = $true)][object]$Expected,
        [Parameter(Mandatory = $true)][string]$VendorOwner
    )

    if ($CanonicalLfSha256 -cne $Expected.CanonicalLfSha256) {
        Throw-UdpCallbackBlocker (
            "$VendorOwner sanctioned CodeGenerator source hash is " +
            "$CanonicalLfSha256, " +
            "expected $($Expected.CanonicalLfSha256).")
    }
    if ($LineBreakCount -ne $Expected.LineBreakCount) {
        Throw-UdpCallbackBlocker "$VendorOwner line-break inventory drifted."
    }
    $physicalExact = if ($EolStyle -ceq 'LF') {
        ($RawBytes -eq $Expected.CanonicalLfBytes) -and
            ($RawSha256 -ceq $Expected.CanonicalLfSha256)
    }
    elseif ($EolStyle -ceq 'CRLF') {
        ($RawBytes -eq $Expected.CodeGeneratorCrLfBytes) -and
            ($RawSha256 -ceq $Expected.CodeGeneratorCrLfSha256)
    }
    else {
        $false
    }
    if (-not $physicalExact) {
        Throw-UdpCallbackBlocker (
            "$VendorOwner physical source is not an approved exact LF or CRLF form.")
    }
}

function ConvertTo-NormalizedGeneratedBlock {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [regex]::Replace(
        (ConvertTo-CanonicalLf -Text $Text).Trim(),
        '\s+',
        ' ')
}

function Get-UniqueGeneratedBlock {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$BlockOwner
    )

    $matches = @([regex]::Matches($Text, $Pattern))
    if ($matches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$BlockOwner generated block count is $($matches.Count), expected 1.")
    }
    return ConvertTo-NormalizedGeneratedBlock -Text $matches[0].Value
}

function Assert-GeneratedIncludeRepresentation {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Observed,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)][string]$State
    )

    if ($State -ceq 'DerivedCandidate') {
        if ($Observed.EolStyle -notin @('LF', 'CRLF')) {
            Throw-UdpCallbackBlocker (
                "$($Observed.Name) DerivedCandidate EOL form is not uniform.")
        }
        return
    }

    $prefix = if ($State -ceq 'Absent') { 'Absent' } else { 'Vendor' }
    $canonicalBytes = $Expected["${prefix}CanonicalLfBytes"]
    $canonicalSha256 = $Expected["${prefix}CanonicalLfSha256"]
    $lineBreakCount = $Expected["${prefix}LineBreakCount"]
    if (($Observed.CanonicalLfBytes -ne $canonicalBytes) -or
        ($Observed.CanonicalLfSha256 -cne $canonicalSha256) -or
        ($Observed.LineBreakCount -ne $lineBreakCount)) {
        Throw-UdpCallbackBlocker (
            "$($Observed.Name) $State canonical generated Include drifted.")
    }

    $physicalExact = if ($Observed.EolStyle -ceq 'LF') {
        ($Observed.RawBytes -eq $canonicalBytes) -and
            ($Observed.RawSha256 -ceq $canonicalSha256)
    }
    elseif (($State -ceq 'VendorImported') -and
        ($Observed.EolStyle -ceq 'CRLF')) {
        ($Observed.RawBytes -eq $Expected.VendorCrLfBytes) -and
            ($Observed.RawSha256 -ceq $Expected.VendorCrLfSha256)
    }
    else {
        $false
    }
    if (-not $physicalExact) {
        Throw-UdpCallbackBlocker (
            "$($Observed.Name) $State physical generated Include is not exact.")
    }
}

function Assert-GeneratedUdpIncludeAbiContract {
    param([Parameter(Mandatory = $true)][object[]]$Observed)

    $cHeader = @($Observed | Where-Object { $_.Name -ceq 'C_channels.h' })[0]
    $stHeader = @($Observed | Where-Object { $_.Name -ceq 'channels.h' })[0]
    $publicHeader = @(
        $Observed | Where-Object { $_.Name -ceq 'lslpublictypes.h' })[0]
    foreach ($header in @($cHeader, $stHeader, $publicHeader)) {
        if ($header.Text.IndexOf(
                '<Unknown>',
                [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Throw-UdpCallbackBlocker (
                "$($header.Name) contains generated Unknown residue.")
        }
    }

    $cInventory = @([regex]::Matches(
            $cHeader.Text,
            '(?m)^[ \t]*typedef[ \t]+struct[ \t]+' +
                '(?<Name>[A-Za-z0-9_]*_UDPTransceiver[A-Za-z0-9_]*)\b') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $cInventory `
        -Expected @($ExpectedCChannelStructBlocks.Keys) `
        -InventoryOwner 'C_channels.h UDP channel struct inventory'
    foreach ($name in $ExpectedCChannelStructBlocks.Keys) {
        $actual = Get-UniqueGeneratedBlock `
            -Text $cHeader.Text `
            -Pattern (
                '(?ms)^[ \t]*typedef[ \t]+struct[ \t]+' +
                [regex]::Escape($name) + '\b.*?\}[ \t]*' +
                [regex]::Escape($name) + '[ \t]*;[ \t]*(?=\r?$)') `
            -BlockOwner "C_channels.h $name"
        if ($actual -cne $ExpectedCChannelStructBlocks[$name]) {
            Throw-UdpCallbackBlocker "C_channels.h $name ABI drifted."
        }
    }

    $stInventory = @([regex]::Matches(
            $stHeader.Text,
            '(?m)^[ \t]*(?<Name>[A-Za-z0-9_]*_UDPTransceiver' +
                '[A-Za-z0-9_]*)[ \t]*:[ \t]*STRUCT\b') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $stInventory `
        -Expected @($ExpectedStChannelStructBlocks.Keys) `
        -InventoryOwner 'channels.h UDP channel struct inventory'
    foreach ($name in $ExpectedStChannelStructBlocks.Keys) {
        $actual = Get-UniqueGeneratedBlock `
            -Text $stHeader.Text `
            -Pattern (
                '(?ms)^[ \t]*' + [regex]::Escape($name) +
                '[ \t]*:[ \t]*STRUCT\b.*?END_STRUCT[ \t]*;' +
                '[ \t]*(?=\r?$)') `
            -BlockOwner "channels.h $name"
        if ($actual -cne $ExpectedStChannelStructBlocks[$name]) {
            Throw-UdpCallbackBlocker "channels.h $name ABI drifted."
        }
    }

    $publicInventory = @([regex]::Matches(
            $publicHeader.Text,
            '(?m)^[ \t]*(?<Name>_UDPTransceiver[A-Za-z0-9_]*)' +
                '[ \t]*:[ \t]*CLASS_PUBLIC\b') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $publicInventory `
        -Expected @($ExpectedPublicTypeBlocks.Keys) `
        -InventoryOwner 'lslpublictypes.h UDP public class inventory'
    foreach ($name in $ExpectedPublicTypeBlocks.Keys) {
        $actual = Get-UniqueGeneratedBlock `
            -Text $publicHeader.Text `
            -Pattern (
                '(?ms)^[ \t]*' + [regex]::Escape($name) +
                '[ \t]*:[ \t]*CLASS_PUBLIC\b.*?END_CLASS[ \t]*;' +
                '[ \t]*(?=\r?$)') `
            -BlockOwner "lslpublictypes.h $name"
        if ($actual -cne $ExpectedPublicTypeBlocks[$name]) {
            Throw-UdpCallbackBlocker "lslpublictypes.h $name ABI drifted."
        }
    }
}

function Assert-GeneratedIncludeContract {
    param(
        [Parameter(Mandatory = $true)][object[]]$Observed,
        [Parameter(Mandatory = $true)][string]$State
    )

    Assert-ExactInventory `
        -Actual @($Observed | ForEach-Object { $_.Name }) `
        -Expected @($GeneratedIncludeContracts.Name) `
        -InventoryOwner 'generated Include file inventory'
    foreach ($expected in $GeneratedIncludeContracts) {
        $file = @($Observed | Where-Object { $_.Name -ceq $expected.Name })[0]
        Assert-GeneratedIncludeRepresentation `
            -Observed $file -Expected $expected -State $State
    }
    if ($State -ceq 'Absent') {
        foreach ($file in $Observed) {
            if ($file.Text.IndexOf(
                    '_UDPTransceiver',
                    [StringComparison]::OrdinalIgnoreCase) -ge 0) {
                Throw-UdpCallbackBlocker (
                    "$($file.Name) Absent state contains UDP generated residue.")
            }
        }
    }
    else {
        Assert-GeneratedUdpIncludeAbiContract -Observed $Observed
    }
}

function Get-VendorCallbackFunctionBlock {
    param(
        [Parameter(Mandatory = $true)][string]$ScanText,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][bool]$Implementation
    )

    $pattern = if ($Implementation) {
        '(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
            '_UDPTransceiverInterface::' + [regex]::Escape($FunctionName) +
            '\b.*?^[ \t]*END_FUNCTION[ \t]*$'
    }
    else {
        '(?ims)^[ \t]*FUNCTION[ \t]+VIRTUAL[ \t]+GLOBAL[ \t]+' +
            [regex]::Escape($FunctionName) +
            '\b.*?(?=^[ \t]*FUNCTION\b|^[ \t]*END_CLASS[ \t]*;)'
    }
    $matches = @([regex]::Matches($ScanText, $pattern))
    if ($matches.Count -ne 1) {
        $kind = if ($Implementation) { 'implementation' } else { 'declaration' }
        Throw-UdpCallbackBlocker (
            "_UDPTransceiverInterface $FunctionName $kind count is " +
            "$($matches.Count), expected 1.")
    }
    return $matches[0].Value
}

function Get-VendorCallbackInputInventory {
    param(
        [Parameter(Mandatory = $true)][string]$FunctionBlock,
        [Parameter(Mandatory = $true)][string]$FunctionOwner
    )

    $sections = @([regex]::Matches(
            $FunctionBlock,
            '(?ims)^[ \t]*VAR_INPUT[ \t]*$' +
                '(?<Body>.*?)^[ \t]*END_VAR[ \t]*;?[ \t]*$'))
    if ($sections.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$FunctionOwner VAR_INPUT count is $($sections.Count), expected 1.")
    }
    if ($FunctionBlock -match '(?im)^[ \t]*VAR_OUTPUT[ \t]*$') {
        Throw-UdpCallbackBlocker "$FunctionOwner must not declare outputs."
    }
    $inventory = [Collections.Generic.List[string]]::new()
    foreach ($line in $sections[0].Groups['Body'].Value -split '\r?\n') {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0) {
            continue
        }
        $match = [regex]::Match(
            $trimmed,
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                '(?<Type>[A-Za-z_][A-Za-z0-9_:]*)[ \t]*;[ \t]*$')
        if (-not $match.Success) {
            Throw-UdpCallbackBlocker (
                "$FunctionOwner has an unapproved input declaration: $trimmed")
        }
        $inventory.Add(
            $match.Groups['Name'].Value + ':' +
            $match.Groups['Type'].Value)
    }
    return $inventory.ToArray()
}

function Assert-VendorEmbeddedNetworkContract {
    param(
        [Parameter(Mandatory = $true)][string]$SourceText,
        [Parameter(Mandatory = $true)][string]$ClassName
    )

    if ([regex]::Matches(
            $SourceText,
            '(?is)<Client[ \t]+Name="CriticalSection_UDP"[ \t]+' +
                'Required="true"[ \t]+Internal="true"[^>]*/>').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$ClassName CriticalSection_UDP internal client metadata is not exact.")
    }
    $networkMatches = @([regex]::Matches(
            $SourceText,
            '(?is)<Network[ \t]+Name="' + [regex]::Escape($ClassName) +
                '"[^>]*>.*?</Network>'))
    if ($networkMatches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$ClassName embedded Network record count is not 1.")
    }
    try {
        [xml]$network = $networkMatches[0].Value
    }
    catch {
        Throw-UdpCallbackBlocker (
            "$ClassName embedded Network XML cannot be parsed: " +
            $_.Exception.Message)
    }
    $objects = @($network.SelectNodes('/Network/Components/Object'))
    $criticalObjects = @($objects | Where-Object {
            $_.GetAttribute('Name') -ceq 'CriticalSection_UDP'
        })
    if (($criticalObjects.Count -ne 1) -or
        ($criticalObjects[0].GetAttribute('Class') -cne 'CriticalSection')) {
        Throw-UdpCallbackBlocker (
            "$ClassName embedded Network needs one CriticalSection_UDP object.")
    }
    $connections = @($network.SelectNodes('/Network/Connections/Connection'))
    $criticalConnections = @($connections | Where-Object {
            ($_.GetAttribute('Source') -match 'CriticalSection_UDP') -or
            ($_.GetAttribute('Destination') -match 'CriticalSection_UDP')
        })
    if (($criticalConnections.Count -ne 1) -or
        ($criticalConnections[0].GetAttribute('Source') -cne
            'this.CriticalSection_UDP') -or
        ($criticalConnections[0].GetAttribute('Destination') -cne
            'CriticalSection_UDP.ClassSvr')) {
        Throw-UdpCallbackBlocker (
            "$ClassName embedded CriticalSection_UDP connection is not exact.")
    }
}

function Assert-VendorSourceAbiContract {
    param(
        [Parameter(Mandatory = $true)][string]$TransceiverSource,
        [Parameter(Mandatory = $true)][string]$InterfaceSource
    )

    $sErrorTags = @([regex]::Matches(
            $TransceiverSource,
            '(?is)<Server[ \t]+Name="sError"[^>]*/>'))
    if ($sErrorTags.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiver sError metadata record count is not 1.')
    }
    if (($sErrorTags[0].Value -match '(?i)\bClass[ \t]*=') -or
        ($sErrorTags[0].Value -match '(?i)<Unknown')) {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiver sError metadata was promoted to an object channel.')
    }
    if ($TransceiverSource -match '(?i)<Unknown') {
        Throw-UdpCallbackBlocker '_UDPTransceiver contains generated Unknown metadata.'
    }
    if ($InterfaceSource -match '(?i)<Unknown') {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiverInterface contains generated Unknown metadata.')
    }
    Assert-VendorEmbeddedNetworkContract `
        -SourceText $TransceiverSource `
        -ClassName '_UDPTransceiver'
    Assert-VendorEmbeddedNetworkContract `
        -SourceText $InterfaceSource `
        -ClassName '_UDPTransceiverInterface'
    $transceiverScan = Get-LexicalScanText -Text $TransceiverSource
    if ([regex]::Matches(
            $transceiverScan,
            '(?im)^[ \t]*sError[ \t]*:[ \t]*SvrCh_DINT[ \t]*;[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiver.sError must remain the exact SvrCh_DINT value server.')
    }
    if ([regex]::Matches(
            $transceiverScan,
            '(?im)^[ \t]*\(::?_UDPTransceiver\.sError\.pMeth\)\$UINT,[ \t]*' +
                '_CH_SVR\$UINT,').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiver.sError command-table entry is not exact _CH_SVR.')
    }
    if ($transceiverScan -match
        '(?im)^[ \t]*sError\.pMeth[ \t]*:=[ \t]*StoreCmd\b') {
        Throw-UdpCallbackBlocker (
            '_UDPTransceiver.sError must not own a StoreCmd dispatcher.')
    }

    $interfaceScan = Get-LexicalScanText -Text $InterfaceSource
    $callbackContracts = @(
        @{
            Name = 'InfoCallback'
            Inputs = @(
                'FSM_UDP:_UDPTransceiver::_FSM_UDP_USER',
                'InfoPara1:DINT',
                'InfoPara2:DINT')
        },
        @{
            Name = 'ErrorCallback'
            Inputs = @(
                'FSM_UDP:_UDPTransceiver::_FSM_UDP_USER',
                'UdpError:_UDPTransceiver::_UDP_ERROR',
                'ErrCode:DINT')
        })
    foreach ($contract in $callbackContracts) {
        foreach ($implementation in @($false, $true)) {
            $block = Get-VendorCallbackFunctionBlock `
                -ScanText $interfaceScan `
                -FunctionName $contract.Name `
                -Implementation $implementation
            Assert-ExactInventory `
                -Actual @(Get-VendorCallbackInputInventory `
                    -FunctionBlock $block `
                    -FunctionOwner $contract.Name) `
                -Expected @($contract.Inputs) `
                -InventoryOwner "$($contract.Name) vendor callback input ABI"
        }
    }
}

function Assert-ProtectedDependencies {
    param([Parameter(Mandatory = $true)][object[]]$Observed)

    if ($Observed.Count -ne $ProtectedDependencies.Count) {
        Throw-UdpCallbackBlocker 'protected dependency inventory count drifted.'
    }
    foreach ($expected in $ProtectedDependencies) {
        $matches = @($Observed | Where-Object { $_.Name -ceq $expected.Name })
        if (($matches.Count -ne 1) -or
            ($matches[0].Bytes -ne $expected.Bytes) -or
            ($matches[0].Sha256 -cne $expected.Sha256)) {
            Throw-UdpCallbackBlocker (
                "protected dependency $($expected.Name) was overwritten or drifted.")
        }
    }
}

function Assert-ProtectedGeneratedDependencyContract {
    param([Parameter(Mandatory = $true)][string]$ClassesDatabaseText)

    # These are whole-file snapshot ratchets. An intentional dependency-graph
    # change requires an evidence-backed rebaseline; a UDP import does not.
    foreach ($entry in @(
            @{
                Owner = '_StdLib source record'
                Needle = '.\Class\_StdLib\_StdLib.st'
                Count = 1
            },
            @{
                Owner = 'CriticalSection source record'
                Needle = '.\Class\CriticalSection\CriticalSection.st'
                Count = 1
            },
            @{
                Owner = '_StdLib OsiBaseNew dependency records'
                Needle = '.\Source\code\OsiBaseNew.h'
                Count = 14
            },
            @{
                Owner = 'CriticalSection exact lsl_st_mt dependency record'
                Needle = '.\lsl_st_mt.h'
                Count = 38
            })) {
        $count = Get-OrdinalCount `
            -Text $ClassesDatabaseText `
            -Needle $entry.Needle
        if ($count -ne $entry.Count) {
            Throw-UdpCallbackBlocker (
                "$($entry.Owner) count is $count, expected $($entry.Count); " +
                'Classes.lcb retains overwritten or stale protected metadata.')
        }
    }
}

function Assert-ProtectedGeneratedRecordContract {
    param([Parameter(Mandatory = $true)][object[]]$Observed)

    if ($Observed.Count -ne $ProtectedGeneratedRecordContracts.Count) {
        Throw-UdpCallbackBlocker (
            'protected generated Classes.lcb record inventory count drifted.')
    }
    foreach ($expected in $ProtectedGeneratedRecordContracts) {
        $matches = @($Observed | Where-Object { $_.Name -ceq $expected.Name })
        if (($matches.Count -ne 1) -or
            ($matches[0].Bytes -ne $expected.Bytes) -or
            ($matches[0].Sha256 -cne $expected.Sha256)) {
            Throw-UdpCallbackBlocker (
                "protected generated $($expected.Name) Classes.lcb record " +
                'was overwritten or regenerated from a noncanonical dependency.')
        }
    }
}

function Assert-VendorGeneratedDependencyContract {
    param(
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText,
        [Parameter(Mandatory = $true)][bool]$VendorPresent
    )

    $expectedExact = if ($VendorPresent) { 4 } else { 2 }
    $expectedTotal = if ($VendorPresent) { 10 } else { 8 }
    $exactCount = Get-OrdinalCount `
        -Text $ClassesDatabaseText -Needle '.\lsl_st_tcp_user.h'
    $totalCount = Get-OrdinalCount `
        -Text $ClassesDatabaseText -Needle 'lsl_st_tcp_user.h'
    if (($exactCount -ne $expectedExact) -or
        ($totalCount -ne $expectedTotal)) {
        Throw-UdpCallbackBlocker (
            'Classes.lcb lsl_st_tcp_user dependency registration count is ' +
            "$exactCount/$totalCount, expected $expectedExact/$expectedTotal.")
    }
}

function Assert-NoForbiddenDemoImport {
    param([Parameter(Mandatory = $true)][pscustomobject]$Snapshot)

    if ($Snapshot.ForbiddenPaths.Count -ne 0) {
        Throw-UdpCallbackBlocker (
            'forbidden MotionTCPDemo paths exist: ' +
            [string]::Join(', ', $Snapshot.ForbiddenPaths))
    }
    foreach ($forbidden in @(
            $ForbiddenDemoClassNames + $ForbiddenDemoNetworkNames)) {
        $needle = if ($ForbiddenDemoClassNames -ccontains $forbidden) {
            ".\Class\$forbidden\"
        }
        else {
            ".\Network\$forbidden\"
        }
        if ((Get-OrdinalCount `
                -Text $Snapshot.ClassesDatabaseText `
                -Needle $needle `
                -IgnoreCase) -ne 0) {
            Throw-UdpCallbackBlocker (
                "Classes.lcb contains forbidden demo registration $forbidden.")
        }
    }
    if ($Snapshot.ProjectDefinitionText -match
        '(?i)MotionTCPDemo') {
        Throw-UdpCallbackBlocker (
            'canonical project retains forbidden MotionTCPDemo library reference.')
    }
}

function Assert-SourceRecordCounts {
    param(
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText,
        [Parameter(Mandatory = $true)][int]$VendorCount,
        [Parameter(Mandatory = $true)][int]$DerivedCount
    )

    foreach ($path in @(
            '.\Class\_UDPTransceiver\_UDPTransceiver.st',
            '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st')) {
        $count = Get-OrdinalCount `
            -Text $ClassesDatabaseText -Needle $path -IgnoreCase
        if ($count -ne $VendorCount) {
            Throw-UdpCallbackBlocker (
                "Classes.lcb $path count is $count, expected $VendorCount.")
        }
    }
    $derivedPath = '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'
    $derivedObserved = Get-OrdinalCount `
        -Text $ClassesDatabaseText -Needle $derivedPath -IgnoreCase
    if ($derivedObserved -ne $DerivedCount) {
        Throw-UdpCallbackBlocker (
            "Classes.lcb derived source count is $derivedObserved, " +
            "expected $DerivedCount.")
    }
}

function Assert-ProjectDefinitionRegistrations {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectDefinitionText,
        [Parameter(Mandatory = $true)][int]$VendorCount,
        [Parameter(Mandatory = $true)][int]$DerivedCount
    )

    foreach ($entry in @(
            @{
                Name = '_UDPTransceiver'
                Path = '.\Class\_UDPTransceiver\_UDPTransceiver.st'
            },
            @{
                Name = '_UDPTransceiverInterface'
                Path =
                    '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st'
            })) {
        $filePattern = '<File[ \t]+Path="' +
            [regex]::Escape($entry.Path) + '"[ \t]*/>'
        $classPattern = '<Class[ \t]+Name="' +
            [regex]::Escape($entry.Name) + '"[ \t]*/>'
        $fileCount = [regex]::Matches(
            $ProjectDefinitionText,
            $filePattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
        $classCount = [regex]::Matches(
            $ProjectDefinitionText,
            $classPattern,
            [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
        if (($fileCount -ne $VendorCount) -or
            ($classCount -ne $VendorCount)) {
            Throw-UdpCallbackBlocker (
                "project definition $($entry.Name) file/class counts are " +
                "$fileCount/$classCount, expected $VendorCount/$VendorCount.")
        }
    }
    $derivedFileCount = [regex]::Matches(
        $ProjectDefinitionText,
        '<File[ \t]+Path="\.\\Class\\LMCUdpCallbackSender\\' +
            'LMCUdpCallbackSender\.st"[ \t]*/>',
        [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
    $derivedClassCount = [regex]::Matches(
        $ProjectDefinitionText,
        '<Class[ \t]+Name="LMCUdpCallbackSender"[ \t]*/>',
        [Text.RegularExpressions.RegexOptions]::IgnoreCase).Count
    if (($derivedFileCount -ne $DerivedCount) -or
        ($derivedClassCount -ne $DerivedCount)) {
        Throw-UdpCallbackBlocker (
            'project definition derived file/class registration count drifted.')
    }
}

function Assert-ProjectDatabaseResidueContract {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectDatabaseText,
        [Parameter(Mandatory = $true)][string]$State
    )

    $forbiddenTokens = [Collections.Generic.List[string]]::new()
    foreach ($name in @($ForbiddenDemoClassNames + $ForbiddenDemoNetworkNames)) {
        $forbiddenTokens.Add($name)
    }
    $forbiddenTokens.Add('MotionTCPDemo')
    $forbiddenTokens.Add('_UDPTransceiver1')
    if ($State -cne 'DerivedCandidate') {
        $forbiddenTokens.Add('LMCUdpCallbackSender')
        $forbiddenTokens.Add('LMCUdpTransceiver1')
    }
    foreach ($token in $forbiddenTokens) {
        if ([regex]::IsMatch(
                $ProjectDatabaseText,
                '(?<![A-Za-z0-9_])' + [regex]::Escape($token) +
                    '(?![A-Za-z0-9_])',
                [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            Throw-UdpCallbackBlocker (
                "project .lcb contains forbidden or premature token $token.")
        }
    }
}

function Assert-LasalUdpCallbackStateContract {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][bool]$PermitAbsent,
        [ValidateSet('Auto', 'Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$RequiredState = 'Auto'
    )

    Assert-ProtectedDependencies -Observed @($Snapshot.ProtectedDependencies)
    Assert-ProtectedGeneratedRecordContract `
        -Observed @($Snapshot.ProtectedGeneratedRecords)
    Assert-ProtectedGeneratedDependencyContract `
        -ClassesDatabaseText $Snapshot.ClassesDatabaseText
    Assert-NoForbiddenDemoImport -Snapshot $Snapshot

    if ($Snapshot.TransceiverPresent -ne $Snapshot.InterfacePresent) {
        Throw-UdpCallbackBlocker (
            'vendor import is partial; both approved classes must exist together.')
    }
    if ($Snapshot.DerivedPresent -and (-not $Snapshot.TransceiverPresent)) {
        Throw-UdpCallbackBlocker (
            'derived sender exists without both approved vendor classes.')
    }

    $state = if (-not $Snapshot.TransceiverPresent) {
        'Absent'
    }
    elseif (-not $Snapshot.DerivedPresent) {
        'VendorImported'
    }
    else {
        'DerivedCandidate'
    }
    if (($RequiredState -cne 'Auto') -and ($state -cne $RequiredState)) {
        Throw-UdpCallbackBlocker (
            "resolved state is $state, required state is $RequiredState.")
    }
    if (($state -ceq 'DerivedCandidate') -and
        (-not $DerivedCandidateApproved)) {
        Throw-UdpCallbackBlocker (
            'DerivedCandidate is fail-closed until the corrected Gate B ' +
            'ABI, scheduler, queue, socket, wire, and result-domain contract ' +
            'is frozen and re-enabled.')
    }
    Assert-VendorGeneratedDependencyContract `
        -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
        -VendorPresent ($state -cne 'Absent')
    Assert-ProjectDefinitionRegistrations `
        -ProjectDefinitionText $Snapshot.ProjectDefinitionText `
        -VendorCount $(if ($state -ceq 'Absent') { 0 } else { 1 }) `
        -DerivedCount $(if ($state -ceq 'DerivedCandidate') { 1 } else { 0 })
    Assert-ProjectDatabaseResidueContract `
        -ProjectDatabaseText $Snapshot.ProjectDatabaseText `
        -State $state
    Assert-GeneratedIncludeContract `
        -Observed @($Snapshot.GeneratedIncludes) `
        -State $state

    if ($state -ceq 'Absent') {
        if (-not $PermitAbsent) {
            Throw-UdpCallbackBlocker (
                'Absent is allowed only with explicit pre-import mode.')
        }
        if ($Snapshot.ClassesSha256 -cne $ExpectedAbsentClassesSha256) {
            Throw-UdpCallbackBlocker 'Absent Classes.lcb baseline drifted.'
        }
        if (($Snapshot.TcpSha256 -cne $ExpectedBaselineTcpSha256) -or
            ($Snapshot.TrackedNetworkCount -ne
                $ExpectedBaselineTrackedNetworkCount) -or
            ($Snapshot.TrackedNetworkSha256 -cne
                $ExpectedBaselineTrackedNetworkSha256) -or
            ($Snapshot.FullNetworkCount -ne
                $ExpectedBaselineNetworkInventoryCount) -or
            ($Snapshot.FullNetworkSha256 -cne
                $ExpectedBaselineNetworkInventorySha256)) {
            Throw-UdpCallbackBlocker (
                'Absent source or tracked Network baseline drifted.')
        }
        Assert-SourceRecordCounts `
            -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
            -VendorCount 0 `
            -DerivedCount 0
        if ($Snapshot.TcpSource.IndexOf(
                'CallbackSender',
                [StringComparison]::Ordinal) -ge 0) {
            Throw-UdpCallbackBlocker 'Absent state contains CallbackSender residue.'
        }
    }
    else {
        Assert-VendorGeneratedRepresentation `
            -CanonicalLfSha256 $Snapshot.TransceiverCanonicalLfSha256 `
            -RawSha256 $Snapshot.TransceiverRawSha256 `
            -RawBytes $Snapshot.TransceiverRawBytes `
            -EolStyle $Snapshot.TransceiverEolStyle `
            -LineBreakCount $Snapshot.TransceiverLineBreakCount `
            -Expected $ExpectedVendor.Transceiver `
            -VendorOwner '_UDPTransceiver'
        Assert-VendorGeneratedRepresentation `
            -CanonicalLfSha256 $Snapshot.InterfaceCanonicalLfSha256 `
            -RawSha256 $Snapshot.InterfaceRawSha256 `
            -RawBytes $Snapshot.InterfaceRawBytes `
            -EolStyle $Snapshot.InterfaceEolStyle `
            -LineBreakCount $Snapshot.InterfaceLineBreakCount `
            -Expected $ExpectedVendor.Interface `
            -VendorOwner '_UDPTransceiverInterface'
        Assert-VendorSourceAbiContract `
            -TransceiverSource $Snapshot.TransceiverSource `
            -InterfaceSource $Snapshot.InterfaceSource
        Assert-VendorGeneratedAbiContract `
            -ClassesDatabaseBytes $Snapshot.ClassesDatabaseBytes `
            -ClassesDatabaseText $Snapshot.ClassesDatabaseText
        Assert-VendorGeneratedRecordContract `
            -Observed @($Snapshot.VendorGeneratedRecords)

        if ($state -ceq 'VendorImported') {
            if (($Snapshot.ClassesBytes -ne $ExpectedVendorImportedClassesBytes) -or
                ($Snapshot.ClassesSha256 -cne
                    $ExpectedVendorImportedClassesSha256) -or
                ($Snapshot.TcpSha256 -cne $ExpectedBaselineTcpSha256) -or
                ($Snapshot.TrackedNetworkCount -ne
                    $ExpectedBaselineTrackedNetworkCount) -or
                ($Snapshot.TrackedNetworkSha256 -cne
                    $ExpectedVendorImportedTrackedNetworkSha256) -or
                ($Snapshot.FullNetworkCount -ne
                    $ExpectedBaselineNetworkInventoryCount) -or
                ($Snapshot.FullNetworkSha256 -cne
                    $ExpectedVendorImportedNetworkInventorySha256) -or
                ($Snapshot.ProtectedTrackedNetworkCount -ne
                    $ExpectedProtectedTrackedNetworkCount) -or
                ($Snapshot.ProtectedTrackedNetworkSha256 -cne
                    $ExpectedProtectedTrackedNetworkSha256)) {
                Throw-UdpCallbackBlocker (
                    'Gate A changed TCPMotionInterface or Network outside ' +
                    'the exact generated registries.')
            }
            Assert-VendorImportedNetworkRegistryContract `
                -ConfigObjectsText $Snapshot.ConfigObjectsText `
                -ConfigObjectsBytes $Snapshot.ConfigObjectsBytes `
                -ConfigObjectsSha256 $Snapshot.ConfigObjectsSha256 `
                -NetworksDatabaseBytes $Snapshot.NetworksDatabaseBytes `
                -NetworksDatabaseSha256 $Snapshot.NetworksDatabaseSha256
            Assert-SourceRecordCounts `
                -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
                -VendorCount 1 `
                -DerivedCount 0
            if ($Snapshot.TcpSource.IndexOf(
                    'CallbackSender',
                    [StringComparison]::Ordinal) -ge 0) {
                Throw-UdpCallbackBlocker (
                    'Gate A contains premature CallbackSender residue.')
            }
        }
        else {
            if (($Snapshot.TrackedNetworkCount -ne
                    $ExpectedBaselineTrackedNetworkCount) -or
                ($Snapshot.ProtectedTrackedNetworkCount -ne
                    $ExpectedProtectedTrackedNetworkCount) -or
                ($Snapshot.ProtectedTrackedNetworkSha256 -cne
                    $ExpectedProtectedTrackedNetworkSha256)) {
                Throw-UdpCallbackBlocker (
                    'DerivedCandidate changed Network outside the four approved artifacts.')
            }
            Assert-SourceRecordCounts `
                -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
                -VendorCount 1 `
                -DerivedCount 1
            Assert-DerivedSourceContract -SourceText $Snapshot.DerivedSource
            Assert-TcpDerivedClientContract -TcpSource $Snapshot.TcpSource
            Assert-DerivedNetworkContract `
                -CommNetworkText $Snapshot.CommNetworkText `
                -CommTableText $Snapshot.CommTableText `
                -NetworksDatabaseText $Snapshot.NetworksDatabaseText
            Assert-GeneratedDerivedMetadata `
                -ClassesDatabaseBytes $Snapshot.ClassesDatabaseBytes `
                -ClassesDatabaseText $Snapshot.ClassesDatabaseText
        }
    }

    return [pscustomobject]@{
        State = $state
        VendorPairExact = ($state -cne 'Absent')
        ProtectedDependenciesExact = $true
        DerivedContractChecked = ($state -ceq 'DerivedCandidate')
    }
}

function Get-RequiredFileBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$FileOwner
    )

    $fullPath = Join-Path $Root $RelativePath.Replace('/', '\')
    if (-not [IO.File]::Exists($fullPath)) {
        Throw-UdpCallbackBlocker "$FileOwner is missing: $RelativePath"
    }
    try {
        $bytes = [IO.File]::ReadAllBytes($fullPath)
    }
    catch {
        Throw-UdpCallbackBlocker (
            "$FileOwner could not be read once: $($_.Exception.Message)")
    }
    return ,$bytes
}

function Assert-LasalIdeClosed {
    $processes = @(Get-Process -Name Lasal2 -ErrorAction SilentlyContinue)
    if ($processes.Count -ne 0) {
        Throw-UdpCallbackBlocker (
            'LASAL2 must be closed during the repository snapshot; running PIDs: ' +
            [string]::Join(',', @($processes.Id)))
    }
}

function Get-AsciiTextEvidence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$SourceOwner,
        [switch]$RequireUniformEol
    )

    $text = ConvertFrom-StrictAsciiBytes -Bytes $Bytes -SourceOwner $SourceOwner
    $crLfCount = [regex]::Matches($text, "`r`n").Count
    $bareLfCount = [regex]::Matches($text, "(?<!`r)`n").Count
    $bareCrCount = [regex]::Matches($text, "`r(?!`n)").Count
    if ($RequireUniformEol -and
        (($bareCrCount -ne 0) -or
         (($crLfCount -ne 0) -and ($bareLfCount -ne 0)))) {
        Throw-UdpCallbackBlocker "$SourceOwner has mixed or bare-CR line endings."
    }
    $eolStyle = if ($bareCrCount -ne 0) {
        'BareCR'
    }
    elseif (($crLfCount -ne 0) -and ($bareLfCount -ne 0)) {
        'Mixed'
    }
    elseif ($crLfCount -ne 0) {
        'CRLF'
    }
    elseif ($bareLfCount -ne 0) {
        'LF'
    }
    else {
        'None'
    }
    $canonicalLf = ConvertTo-CanonicalLf -Text $text
    return [pscustomobject]@{
        Bytes = $Bytes
        ByteCount = $Bytes.Count
        RawSha256 = Get-BytesSha256 -Bytes $Bytes
        Text = $text
        CanonicalLfBytes = $Utf8.GetByteCount($canonicalLf)
        CanonicalLfSha256 = Get-TextSha256 -Text $canonicalLf
        EolStyle = $eolStyle
        LineBreakCount = $crLfCount + $bareLfCount + $bareCrCount
    }
}

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $rootPrefix = $Root.TrimEnd('\') + '\'
    if (-not $Path.StartsWith(
            $rootPrefix,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-UdpCallbackBlocker "path resolved outside repository: $Path"
    }
    return $Path.Substring($rootPrefix.Length).Replace('\', '/')
}

function Get-NetworkSnapshotEvidence {
    param([Parameter(Mandatory = $true)][string]$Root)

    $networkRelativeRoot = "$TargetRootRelativePath/Network"
    $networkRoot = Join-Path $Root $networkRelativeRoot.Replace('/', '\')
    if (-not [IO.Directory]::Exists($networkRoot)) {
        Throw-UdpCallbackBlocker 'canonical Network directory is missing.'
    }
    $trackedOutput = @(& git -C $Root ls-files -- "$networkRelativeRoot/**")
    if ($LASTEXITCODE -ne 0) {
        Throw-UdpCallbackBlocker 'git ls-files failed for canonical Network.'
    }
    $trackedPaths = @(
        $trackedOutput |
            ForEach-Object { ([string]$_).Trim().Replace('\', '/') } |
            Where-Object { $_ -ne '' } |
            Sort-Object -Unique)
    $availablePaths = @(
        Get-ChildItem -LiteralPath $networkRoot -File -Recurse -Force |
            ForEach-Object {
                Get-RepositoryRelativePath -Root $Root -Path $_.FullName
            } |
            Sort-Object -Unique)
    $allPaths = @(@($trackedPaths + $availablePaths) | Sort-Object -Unique)
    if ($allPaths.Count -eq 0) {
        Throw-UdpCallbackBlocker 'canonical Network inventory is empty.'
    }

    $files = [Collections.Generic.List[object]]::new()
    foreach ($relativePath in $allPaths) {
        $fullPath = Join-Path $Root $relativePath.Replace('/', '\')
        $tracked = $trackedPaths -contains $relativePath
        $available = [IO.File]::Exists($fullPath)
        if ($tracked -and (-not $available)) {
            Throw-UdpCallbackBlocker "tracked Network file is missing: $relativePath"
        }
        $bytes = if ($available) {
            Get-RequiredFileBytes `
                -Root $Root `
                -RelativePath $relativePath `
                -FileOwner 'Network artifact'
        }
        else {
            [byte[]]@()
        }
        $files.Add([pscustomobject]@{
                Path = $relativePath
                Tracked = $tracked
                Available = $available
                Bytes = $bytes
                ByteCount = if ($available) { $bytes.Count } else { $null }
                Sha256 = if ($available) {
                    Get-BytesSha256 -Bytes $bytes
                }
                else {
                    $null
                }
            })
    }

    $fullIdentity = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}' -f
                    $file.Path,
                    ([int][bool]$file.Tracked),
                    ([int][bool]$file.Available),
                    $file.ByteCount,
                    $file.Sha256
            }))
    $trackedFiles = @($files | Where-Object { $_.Tracked })
    $trackedIdentity = [string]::Join("`n", @(
            $trackedFiles |
                Sort-Object Path |
                ForEach-Object {
                    "$($_.Path)|$($_.ByteCount)|$($_.Sha256)"
                }))
    $protectedTrackedFiles = @($trackedFiles | Where-Object {
            $AllowedDerivedNetworkPaths -cnotcontains $_.Path
        })
    $protectedIdentity = [string]::Join("`n", @(
            $protectedTrackedFiles |
                Sort-Object Path |
                ForEach-Object {
                    "$($_.Path)|$($_.ByteCount)|$($_.Sha256)"
                }))
    return [pscustomobject]@{
        Files = $files.ToArray()
        FullCount = $files.Count
        FullSha256 = Get-TextSha256 -Text $fullIdentity
        TrackedCount = $trackedFiles.Count
        TrackedSha256 = Get-TextSha256 -Text $trackedIdentity
        ProtectedTrackedCount = $protectedTrackedFiles.Count
        ProtectedTrackedSha256 = Get-TextSha256 -Text $protectedIdentity
    }
}

function Get-NetworkFileEvidence {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$NetworkEvidence,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )

    $matches = @($NetworkEvidence.Files | Where-Object {
            $_.Path -ceq $RelativePath
        })
    if (($matches.Count -ne 1) -or (-not $matches[0].Available)) {
        Throw-UdpCallbackBlocker "required Network artifact is missing: $RelativePath"
    }
    return $matches[0]
}

function Get-NetworkFileText {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$NetworkEvidence,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )

    $file = Get-NetworkFileEvidence `
        -NetworkEvidence $NetworkEvidence `
        -RelativePath $RelativePath
    return [Text.Encoding]::Latin1.GetString($file.Bytes)
}

function Get-ForbiddenImportPaths {
    param([Parameter(Mandatory = $true)][string]$Root)

    $found = [Collections.Generic.List[string]]::new()
    foreach ($name in $ForbiddenDemoClassNames) {
        $relativePath = "$TargetRootRelativePath/Class/$name"
        if ([IO.Directory]::Exists(
                (Join-Path $Root $relativePath.Replace('/', '\')))) {
            $found.Add($relativePath)
        }
    }
    foreach ($name in $ForbiddenDemoNetworkNames) {
        $relativePath = "$TargetRootRelativePath/Network/$name"
        if ([IO.Directory]::Exists(
                (Join-Path $Root $relativePath.Replace('/', '\')))) {
            $found.Add($relativePath)
        }
    }
    return $found.ToArray()
}

function Get-OptionalSourceEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$SourceOwner
    )

    $fullPath = Join-Path $Root $RelativePath.Replace('/', '\')
    if (-not [IO.File]::Exists($fullPath)) {
        return $null
    }
    $bytes = Get-RequiredFileBytes `
        -Root $Root -RelativePath $RelativePath -FileOwner $SourceOwner
    return Get-AsciiTextEvidence `
        -Bytes $bytes -SourceOwner $SourceOwner -RequireUniformEol
}

function Get-GeneratedIncludeSnapshotEvidence {
    param([Parameter(Mandatory = $true)][string]$Root)

    $observed = [Collections.Generic.List[object]]::new()
    foreach ($expected in $GeneratedIncludeContracts) {
        $bytes = Get-RequiredFileBytes `
            -Root $Root `
            -RelativePath $expected.Path `
            -FileOwner "generated Include $($expected.Name)"
        $text = Get-AsciiTextEvidence `
            -Bytes $bytes `
            -SourceOwner "generated Include $($expected.Name)" `
            -RequireUniformEol
        $observed.Add([pscustomobject]@{
                Name = $expected.Name
                Path = $expected.Path
                Text = $text.Text
                RawBytes = $text.ByteCount
                RawSha256 = $text.RawSha256
                CanonicalLfBytes = $text.CanonicalLfBytes
                CanonicalLfSha256 = $text.CanonicalLfSha256
                EolStyle = $text.EolStyle
                LineBreakCount = $text.LineBreakCount
            })
    }
    return $observed.ToArray()
}

function Get-CurrentRepositorySnapshot {
    param([Parameter(Mandatory = $true)][string]$Root)

    $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
    $gitRootOutput = @(& git -C $resolvedRoot rev-parse --show-toplevel)
    if (($LASTEXITCODE -ne 0) -or ($gitRootOutput.Count -ne 1)) {
        Throw-UdpCallbackBlocker 'RepositoryRoot is not a readable Git worktree.'
    }
    $gitRoot = (Resolve-Path -LiteralPath $gitRootOutput[0]).Path
    if ($gitRoot -cne $resolvedRoot) {
        Throw-UdpCallbackBlocker 'RepositoryRoot must be the Git worktree root.'
    }

    $protectedObserved = [Collections.Generic.List[object]]::new()
    foreach ($expected in $ProtectedDependencies) {
        $bytes = Get-RequiredFileBytes `
            -Root $resolvedRoot `
            -RelativePath $expected.Path `
            -FileOwner "protected dependency $($expected.Name)"
        $protectedObserved.Add([pscustomobject]@{
                Name = $expected.Name
                Bytes = $bytes.Count
                Sha256 = Get-BytesSha256 -Bytes $bytes
            })
    }

    $transceiver = Get-OptionalSourceEvidence `
        -Root $resolvedRoot `
        -RelativePath $TransceiverRelativePath `
        -SourceOwner '_UDPTransceiver source'
    $interface = Get-OptionalSourceEvidence `
        -Root $resolvedRoot `
        -RelativePath $InterfaceRelativePath `
        -SourceOwner '_UDPTransceiverInterface source'
    $derived = Get-OptionalSourceEvidence `
        -Root $resolvedRoot `
        -RelativePath $DerivedRelativePath `
        -SourceOwner 'LMCUdpCallbackSender source'

    $tcpBytes = Get-RequiredFileBytes `
        -Root $resolvedRoot -RelativePath $TcpRelativePath `
        -FileOwner 'TCPMotionInterface source'
    $tcp = Get-AsciiTextEvidence `
        -Bytes $tcpBytes -SourceOwner 'TCPMotionInterface source'
    $classesBytes = Get-RequiredFileBytes `
        -Root $resolvedRoot -RelativePath $ClassesRelativePath `
        -FileOwner 'Classes.lcb'
    $projectBytes = Get-RequiredFileBytes `
        -Root $resolvedRoot -RelativePath $ProjectRelativePath `
        -FileOwner 'project Classes database'
    $projectDefinitionBytes = Get-RequiredFileBytes `
        -Root $resolvedRoot -RelativePath $ProjectDefinitionRelativePath `
        -FileOwner 'canonical project definition'
    $projectDefinition = Get-AsciiTextEvidence `
        -Bytes $projectDefinitionBytes `
        -SourceOwner 'canonical project definition'
    $network = Get-NetworkSnapshotEvidence -Root $resolvedRoot
    $generatedIncludes = Get-GeneratedIncludeSnapshotEvidence -Root $resolvedRoot
    $configObjectsNetworkFile = Get-NetworkFileEvidence `
        -NetworkEvidence $network `
        -RelativePath $ConfigObjectsRelativePath
    $networksDatabaseNetworkFile = Get-NetworkFileEvidence `
        -NetworkEvidence $network `
        -RelativePath $NetworksDatabaseRelativePath

    return [pscustomobject]@{
        ProtectedDependencies = $protectedObserved.ToArray()
        GeneratedIncludes = $generatedIncludes
        ProtectedGeneratedRecords = @(
            Get-ProtectedGeneratedRecordEvidence `
                -ClassesDatabaseBytes $classesBytes `
                -ClassesDatabaseText (
                    [Text.Encoding]::Latin1.GetString($classesBytes)))
        VendorGeneratedRecords = if (($null -ne $transceiver) -and
            ($null -ne $interface)) {
            @(Get-VendorGeneratedRecordEvidence `
                    -ClassesDatabaseBytes $classesBytes `
                    -ClassesDatabaseText (
                        [Text.Encoding]::Latin1.GetString($classesBytes)))
        }
        else {
            @()
        }
        ForbiddenPaths = @(Get-ForbiddenImportPaths -Root $resolvedRoot)
        TransceiverPresent = ($null -ne $transceiver)
        InterfacePresent = ($null -ne $interface)
        DerivedPresent = ($null -ne $derived)
        TransceiverSource = if ($null -ne $transceiver) {
            $transceiver.Text
        } else { '' }
        InterfaceSource = if ($null -ne $interface) {
            $interface.Text
        } else { '' }
        TransceiverCanonicalLfSha256 = if ($null -ne $transceiver) {
            $transceiver.CanonicalLfSha256
        } else { '' }
        TransceiverRawSha256 = if ($null -ne $transceiver) {
            $transceiver.RawSha256
        } else { '' }
        TransceiverRawBytes = if ($null -ne $transceiver) {
            $transceiver.ByteCount
        } else { 0 }
        TransceiverEolStyle = if ($null -ne $transceiver) {
            $transceiver.EolStyle
        } else { 'Absent' }
        TransceiverLineBreakCount = if ($null -ne $transceiver) {
            $transceiver.LineBreakCount
        } else { 0 }
        InterfaceCanonicalLfSha256 = if ($null -ne $interface) {
            $interface.CanonicalLfSha256
        } else { '' }
        InterfaceRawSha256 = if ($null -ne $interface) {
            $interface.RawSha256
        } else { '' }
        InterfaceRawBytes = if ($null -ne $interface) {
            $interface.ByteCount
        } else { 0 }
        InterfaceEolStyle = if ($null -ne $interface) {
            $interface.EolStyle
        } else { 'Absent' }
        InterfaceLineBreakCount = if ($null -ne $interface) {
            $interface.LineBreakCount
        } else { 0 }
        DerivedSource = if ($null -ne $derived) { $derived.Text } else { '' }
        TcpSource = $tcp.Text
        TcpSha256 = $tcp.RawSha256
        ClassesDatabaseBytes = $classesBytes
        ClassesDatabaseText = [Text.Encoding]::Latin1.GetString($classesBytes)
        ClassesBytes = $classesBytes.Count
        ClassesSha256 = Get-BytesSha256 -Bytes $classesBytes
        ProjectBytes = $projectBytes.Count
        ProjectSha256 = Get-BytesSha256 -Bytes $projectBytes
        ProjectDatabaseText = [Text.Encoding]::Latin1.GetString($projectBytes)
        ProjectDefinitionBytes = $projectDefinitionBytes.Count
        ProjectDefinitionSha256 = Get-BytesSha256 -Bytes $projectDefinitionBytes
        ProjectDefinitionText = $projectDefinition.Text
        FullNetworkCount = $network.FullCount
        FullNetworkSha256 = $network.FullSha256
        TrackedNetworkCount = $network.TrackedCount
        TrackedNetworkSha256 = $network.TrackedSha256
        ProtectedTrackedNetworkCount = $network.ProtectedTrackedCount
        ProtectedTrackedNetworkSha256 = $network.ProtectedTrackedSha256
        ConfigObjectsText = [Text.Encoding]::Latin1.GetString(
            $configObjectsNetworkFile.Bytes)
        ConfigObjectsBytes = $configObjectsNetworkFile.ByteCount
        ConfigObjectsSha256 = $configObjectsNetworkFile.Sha256
        NetworksDatabaseBytes = $networksDatabaseNetworkFile.ByteCount
        NetworksDatabaseSha256 = $networksDatabaseNetworkFile.Sha256
        CommNetworkText = Get-NetworkFileText `
            -NetworkEvidence $network -RelativePath $CommNetworkRelativePath
        CommTableText = Get-NetworkFileText `
            -NetworkEvidence $network -RelativePath $CommTableRelativePath
        NetworksDatabaseText = Get-NetworkFileText `
            -NetworkEvidence $network -RelativePath $NetworksDatabaseRelativePath
    }
}

function Add-BytesToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][byte[]]$Bytes
    )

    foreach ($value in $Bytes) {
        $List.Add($value)
    }
}

function Add-AsciiTextToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Text
    )

    Add-BytesToList -List $List -Bytes ([Text.Encoding]::ASCII.GetBytes($Text))
}

function New-SyntheticFunctionMetadataBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [bool]$IsVirtual = $false,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Inputs,
        [AllowEmptyCollection()]
        [string[]]$Outputs = @()
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    Add-AsciiTextToList -List $bytes -Text $Name
    $inputCount = [uint32]$Inputs.Count
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            0x0B, 0x00, 0x00, 0x00,
            [byte]$(if ($IsVirtual) { 1 } else { 0 }),
            [byte]$(if ($IsGlobal) { 1 } else { 0 }), 0x00, 0x00,
            [byte]($inputCount -band 0xFF),
            [byte](($inputCount -shr 8) -band 0xFF),
            [byte](($inputCount -shr 16) -band 0xFF),
            [byte](($inputCount -shr 24) -band 0xFF)))
    foreach ($entry in $Inputs) {
        $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
        $name = $entry.Substring(0, $separator)
        $type = $entry.Substring($separator + 1)
        $nameLength = [uint32]$name.Length
        Add-BytesToList -List $bytes -Bytes ([byte[]]@(
                0x00, 0x01,
                [byte]($nameLength -band 0xFF),
                [byte](($nameLength -shr 8) -band 0xFF),
                [byte](($nameLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $bytes -Text $name
        $typeLength = [uint32]$type.Length
        Add-BytesToList -List $bytes -Bytes ([byte[]]@(
                [byte]($typeLength -band 0xFF),
                [byte](($typeLength -shr 8) -band 0xFF),
                [byte](($typeLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $bytes -Text $type
    }
    if ($Outputs.Count -eq 0) {
        Add-BytesToList -List $bytes -Bytes ([byte[]]@(0, 0, 0, 0))
    }
    else {
        $entry = $Outputs[0]
        $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
        $name = $entry.Substring(0, $separator)
        $type = $entry.Substring($separator + 1)
        $nameLength = [uint32]$name.Length
        Add-BytesToList -List $bytes -Bytes ([byte[]]@(
                0x01, 0x00, 0x00, 0x00,
                0x00, 0x01,
                [byte]($nameLength -band 0xFF),
                [byte](($nameLength -shr 8) -band 0xFF),
                [byte](($nameLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $bytes -Text $name
        $typeLength = [uint32]$type.Length
        Add-BytesToList -List $bytes -Bytes ([byte[]]@(
                [byte]($typeLength -band 0xFF),
                [byte](($typeLength -shr 8) -band 0xFF),
                [byte](($typeLength -shr 16) -band 0xFF),
                0xAA))
        Add-AsciiTextToList -List $bytes -Text $type
    }
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(0, 0, 0, 0xAA))
    return ,$bytes.ToArray()
}

function New-SyntheticVendorMethodHeaderBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][uint32]$InputCount
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    Add-AsciiTextToList -List $bytes -Text $Name
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            0x0B, 0x00, 0x00, 0x00,
            0x01, 0x01, 0x00, 0x00,
            [byte]($InputCount -band 0xFF),
            [byte](($InputCount -shr 8) -band 0xFF),
            [byte](($InputCount -shr 16) -band 0xFF),
            [byte](($InputCount -shr 24) -band 0xFF),
            0x00, 0x00, 0x00, 0x00))
    return ,$bytes.ToArray()
}

function New-SyntheticClassesDatabase {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$State
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\_StdLib\_StdLib.st' + [char]0)
    for ($index = 0; $index -lt 14; $index++) {
        Add-AsciiTextToList -List $bytes -Text (
            '.\Source\code\OsiBaseNew.h' + [char]0)
    }
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\CriticalSection\CriticalSection.st' + [char]0)
    for ($index = 0; $index -lt 38; $index++) {
        Add-AsciiTextToList -List $bytes -Text ('.\lsl_st_mt.h' + [char]0)
    }
    $exactTcpHeaderCount = if ($State -ceq 'Absent') { 2 } else { 4 }
    $totalTcpHeaderCount = if ($State -ceq 'Absent') { 8 } else { 10 }
    for ($index = 0; $index -lt $exactTcpHeaderCount; $index++) {
        Add-AsciiTextToList -List $bytes -Text (
            '.\lsl_st_tcp_user.h' + [char]0)
    }
    for ($index = $exactTcpHeaderCount;
         $index -lt $totalTcpHeaderCount;
         $index++) {
        Add-AsciiTextToList -List $bytes -Text ('lsl_st_tcp_user.h' + [char]0)
    }
    if ($State -cne 'Absent') {
        Add-AsciiTextToList -List $bytes -Text (
            '_UDPTransceiver' + [char]0 + '<Unknown>' + [char]0)
        Add-AsciiTextToList -List $bytes -Text (
            '.\Class\_UDPTransceiver\_UDPTransceiver.st' + [char]0)
        Add-AsciiTextToList -List $bytes -Text (
            'sControl' + [char]0 +
            'SvrChCmd__FSM_UDP_USER_PTofCls__UDPTransceiver' + [char]0 +
            'sError' + [char]0 + 'SvrCh_DINT' + [char]0 +
            'cSizeOfTXBuffer' + [char]0 + 'CltCh_UDINT' + [char]0 +
            'cSizeOfRXBuffer' + [char]0)
        Add-BytesToList -List $bytes -Bytes (
            New-SyntheticVendorMethodHeaderBytes `
                -Name SendData -InputCount 6)
        Add-AsciiTextToList -List $bytes -Text (
            '_UDPTransceiverInterface' + [char]0 + '<Unknown>' + [char]0)
        Add-AsciiTextToList -List $bytes -Text (
            '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st' +
            [char]0)
        Add-AsciiTextToList -List $bytes -Text (
            'ClassSvr' + [char]0 + 'SvrChCmd_DINT' + [char]0 +
            '_UDPTransceiver' + [char]0 +
            'CltChCmd__UDPTransceiver' + [char]0)
        $vendorMethods = [ordered]@{
            AddSocket = 0
            BindSocket = 2
            DelSocket = 0
            SendData = 5
            FLUSHRingbuffer = 0
            IsOpen = 0
            ConvertStrToUdint = 1
            ConvertUdintToStr = 3
            GetIpInfo = 3
            SendDataBlocked = 5
            Response = 4
            InfoCallback = 3
            ErrorCallback = 3
        }
        foreach ($entry in $vendorMethods.GetEnumerator()) {
            if ($entry.Key -ceq 'InfoCallback') {
                Add-BytesToList -List $bytes -Bytes (
                    New-SyntheticFunctionMetadataBytes `
                        -Name $entry.Key `
                        -IsVirtual $true `
                        -IsGlobal $true `
                        -Inputs @(
                            'FSM_UDP:_UDPTransceiver::_FSM_UDP_USER',
                            'InfoPara1:DINT',
                            'InfoPara2:DINT') `
                        -Outputs @())
            }
            elseif ($entry.Key -ceq 'ErrorCallback') {
                Add-BytesToList -List $bytes -Bytes (
                    New-SyntheticFunctionMetadataBytes `
                        -Name $entry.Key `
                        -IsVirtual $true `
                        -IsGlobal $true `
                        -Inputs @(
                            'FSM_UDP:_UDPTransceiver::_FSM_UDP_USER',
                            'UdpError:_UDPTransceiver::_UDP_ERROR',
                            'ErrCode:DINT') `
                        -Outputs @())
            }
            else {
                Add-BytesToList -List $bytes -Bytes (
                    New-SyntheticVendorMethodHeaderBytes `
                        -Name $entry.Key -InputCount ([uint32]$entry.Value))
            }
        }
        Add-AsciiTextToList -List $bytes -Text (
            'ASCII_BIN' + [char]0 + '<Unknown>' + [char]0 +
            '.\Class\ASCII_BIN\ASCII_BIN.st' + [char]0)
    }
    if ($State -ceq 'DerivedCandidate') {
        Add-AsciiTextToList -List $bytes -Text (
            '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st' + [char]0)
        foreach ($spec in $PublicFunctionSpecs) {
            Add-BytesToList -List $bytes -Bytes (
                New-SyntheticFunctionMetadataBytes `
                    -Name $spec.Name `
                    -IsGlobal $true `
                    -Inputs @($spec.Inputs) `
                    -Outputs @($spec.Outputs))
        }
        foreach ($name in $PrivateFunctionNames) {
            Add-BytesToList -List $bytes -Bytes (
                New-SyntheticFunctionMetadataBytes `
                    -Name $name `
                    -IsGlobal $false `
                    -Inputs @() `
                    -Outputs @())
        }
    }
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\TCPMotionInterface\TCPMotionInterface.st' + [char]0)
    if ($State -ceq 'DerivedCandidate') {
        Add-AsciiTextToList -List $bytes -Text (
            'CallbackSender' + [char]0 + 'LMCUdpCallbackSender' + [char]0)
    }
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\ZZFixtureBoundary\ZZFixtureBoundary.st' + [char]0)
    $array = $bytes.ToArray()
    return [pscustomobject]@{
        Bytes = $array
        Text = [Text.Encoding]::Latin1.GetString($array)
    }
}

function New-SyntheticDerivedSource {
    $lines = [Collections.Generic.List[string]]::new()
    foreach ($line in @(
            '(*!',
            '<Class',
            '    Name = "LMCUdpCallbackSender"',
            '    RealtimeTask = "false"',
            '    CyclicTask = "true"',
            '    Sigmatek = "false"',
            '    Objectsize = "(320,120)">',
            '</Class>',
            '*)',
            '#pragma using _UDPTransceiverInterface',
            '',
            'LMCUdpCallbackSender : CLASS',
            ': _UDPTransceiverInterface')) {
        $lines.Add($line)
    }
    foreach ($spec in $PublicFunctionSpecs) {
        $lines.Add("    FUNCTION GLOBAL $($spec.Name)")
        $lines.Add('        VAR_INPUT')
        foreach ($entry in $spec.Inputs) {
            $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
            $lines.Add(
                '            ' + $entry.Substring(0, $separator) + ' : ' +
                $entry.Substring($separator + 1) + ';')
        }
        $lines.Add('        END_VAR')
        $lines.Add('        VAR_OUTPUT')
        $lines.Add('            Result : DINT;')
        $lines.Add('        END_VAR;')
    }
    foreach ($name in $PrivateFunctionNames) {
        $lines.Add("    FUNCTION $name;")
    }
    $lines.Add('END_CLASS;')
    foreach ($spec in $PublicFunctionSpecs) {
        $lines.Add('')
        $lines.Add("FUNCTION GLOBAL LMCUdpCallbackSender::$($spec.Name)")
        $lines.Add('    VAR_INPUT')
        foreach ($entry in $spec.Inputs) {
            $separator = $entry.IndexOf(':', [StringComparison]::Ordinal)
            $lines.Add(
                '        ' + $entry.Substring(0, $separator) + ' : ' +
                $entry.Substring($separator + 1) + ';')
        }
        $lines.Add('    END_VAR')
        $lines.Add('    VAR_OUTPUT')
        $lines.Add('        Result : DINT;')
        $lines.Add('    END_VAR')
        $lines.Add('    Result := 0;')
        $lines.Add('END_FUNCTION')
    }
    foreach ($name in $PrivateFunctionNames) {
        $lines.Add('')
        $lines.Add("FUNCTION LMCUdpCallbackSender::$name")
        $lines.Add('    VAR')
        $lines.Add('        fixtureValue : DINT;')
        $lines.Add('    END_VAR')
        if ($name -ceq 'SendSlot') {
            $lines.Add('    fixtureValue := 0; // bDirect remains FALSE')
        }
        else {
            $lines.Add('    fixtureValue := 0;')
        }
        $lines.Add('END_FUNCTION')
    }
    return [string]::Join("`n", $lines) + "`n"
}

function New-SyntheticTcpSource {
    param([switch]$WithCallbackSender)

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('TCPMotionInterface : CLASS')
    if ($WithCallbackSender) {
        $lines.Add(
            '<Client Name="CallbackSender" Required="false" Internal="false"/>')
        $lines.Add(
            '    CallbackSender : CltChCmd_LMCUdpCallbackSender;')
    }
    $lines.Add('END_CLASS;')
    return [string]::Join("`n", $lines) + "`n"
}

function New-SyntheticDerivedNetwork {
    $xml = @'
<Network Name="Comm_Network">
  <Components>
    <Object Name="LMCUdpTransceiver1" Class="_UDPTransceiver">
      <Channels>
        <Client Name="cSizeOfRXBuffer" Value="512"/>
        <Client Name="cSizeOfTXBuffer" Value="8 kb"/>
      </Channels>
    </Object>
    <Object Name="LMCUdpCallbackSender1" Class="LMCUdpCallbackSender">
      <Channels/>
    </Object>
  </Components>
  <Connections>
    <Connection Source="LMCUdpCallbackSender1._UDPTransceiver" Destination="LMCUdpTransceiver1.sControl"/>
    <Connection Source="TCPMotionInterface1.CallbackSender" Destination="LMCUdpCallbackSender1.ClassSvr"/>
  </Connections>
</Network>
'@
    $generated = [string]::Join('|', @(
            'LMCUdpTransceiver1',
            '_UDPTransceiver',
            'LMCUdpCallbackSender1',
            'LMCUdpCallbackSender',
            'CallbackSender'))
    return [pscustomobject]@{
        Xml = $xml
        Table = $generated
        Database = $generated
    }
}

function New-SyntheticProjectDefinition {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$State
    )

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('<Project>')
    $lines.Add('<ClassFiles>')
    if ($State -cne 'Absent') {
        $lines.Add(
            '<File Path=".\Class\_UDPTransceiver\_UDPTransceiver.st"/>')
        $lines.Add(
            '<File Path=".\Class\_UDPTransceiverInterface\' +
            '_UDPTransceiverInterface.st"/>')
    }
    if ($State -ceq 'DerivedCandidate') {
        $lines.Add(
            '<File Path=".\Class\LMCUdpCallbackSender\' +
            'LMCUdpCallbackSender.st"/>')
    }
    $lines.Add('</ClassFiles>')
    $lines.Add('<Classes>')
    if ($State -cne 'Absent') {
        $lines.Add('<Class Name="_UDPTransceiver"/>')
        $lines.Add('<Class Name="_UDPTransceiverInterface"/>')
    }
    if ($State -ceq 'DerivedCandidate') {
        $lines.Add('<Class Name="LMCUdpCallbackSender"/>')
    }
    $lines.Add('</Classes>')
    $lines.Add('</Project>')
    return [string]::Join("`n", $lines)
}

function New-SyntheticVendorTransceiverSource {
    return @'
(*!
<Class>
  <Channels>
    <Server Name="sError" Visualized="true"/>
    <Client Name="CriticalSection_UDP" Required="true" Internal="true"/>
  </Channels>
  <Network Name="_UDPTransceiver">
    <Components>
      <Object Name="CriticalSection_UDP" Class="CriticalSection"/>
    </Components>
    <Connections>
      <Connection Source="this.CriticalSection_UDP" Destination="CriticalSection_UDP.ClassSvr"/>
    </Connections>
  </Network>
</Class>
*)
_UDPTransceiver : CLASS
    sError : SvrCh_DINT;
END_CLASS;
(::_UDPTransceiver.sError.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, "sError",
'@
}

function New-SyntheticVendorInterfaceSource {
    return @'
(*!
<Class>
  <Channels>
    <Client Name="CriticalSection_UDP" Required="true" Internal="true"/>
  </Channels>
  <Network Name="_UDPTransceiverInterface">
    <Components>
      <Object Name="CriticalSection_UDP" Class="CriticalSection"/>
    </Components>
    <Connections>
      <Connection Source="this.CriticalSection_UDP" Destination="CriticalSection_UDP.ClassSvr"/>
    </Connections>
  </Network>
</Class>
*)
_UDPTransceiverInterface : CLASS
    FUNCTION VIRTUAL GLOBAL InfoCallback
        VAR_INPUT
            FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
            InfoPara1 : DINT;
            InfoPara2 : DINT;
        END_VAR;
    FUNCTION VIRTUAL GLOBAL ErrorCallback
        VAR_INPUT
            FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
            UdpError : _UDPTransceiver::_UDP_ERROR;
            ErrCode : DINT;
        END_VAR;
END_CLASS;

FUNCTION VIRTUAL GLOBAL _UDPTransceiverInterface::InfoCallback
    VAR_INPUT
        FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
        InfoPara1 : DINT;
        InfoPara2 : DINT;
    END_VAR
END_FUNCTION

FUNCTION VIRTUAL GLOBAL _UDPTransceiverInterface::ErrorCallback
    VAR_INPUT
        FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
        UdpError : _UDPTransceiver::_UDP_ERROR;
        ErrCode : DINT;
    END_VAR
END_FUNCTION
'@
}

function New-SyntheticConfigObjects {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$State
    )

    $lines = [Collections.Generic.List[string]]::new()
    $baselineCount = if ($State -ceq 'Absent') { 117 } else { 117 }
    foreach ($index in 1..$baselineCount) {
        $lines.Add(('0$UINT, 0, 0, "BASE_{0:D3}",' -f $index))
    }
    if ($State -cne 'Absent') {
        $lines.Add('0$UINT, 1, 2, "_UDPTRANSCEIVER",')
        $lines.Add('0$UINT, 1, 3, "_UDPTRANSCEIVERINTERFACE",')
    }
    if ($State -ceq 'DerivedCandidate') {
        $lines.Add('0$UINT, 1, 0, "LMCUDPCALLBACKSENDER",')
    }
    return [string]::Join("`n", $lines)
}

function New-SyntheticGeneratedIncludes {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$State
    )

    $vendorPresent = $State -cne 'Absent'
    $textByName = @{
        'C_channels.h' = if ($vendorPresent) {
            [string]::Join("`n", @($ExpectedCChannelStructBlocks.Values))
        } else { 'BASE C CHANNELS' }
        'channels.h' = if ($vendorPresent) {
            [string]::Join("`n", @($ExpectedStChannelStructBlocks.Values))
        } else { 'BASE ST CHANNELS' }
        'lslpublictypes.h' = if ($vendorPresent) {
            [string]::Join("`n", @($ExpectedPublicTypeBlocks.Values))
        } else { 'BASE PUBLIC TYPES' }
    }
    return @(
        foreach ($expected in $GeneratedIncludeContracts) {
            $prefix = if ($vendorPresent) { 'Vendor' } else { 'Absent' }
            [pscustomobject]@{
                Name = $expected.Name
                Path = $expected.Path
                Text = $textByName[$expected.Name]
                RawBytes = $expected["${prefix}CanonicalLfBytes"]
                RawSha256 = $expected["${prefix}CanonicalLfSha256"]
                CanonicalLfBytes = $expected["${prefix}CanonicalLfBytes"]
                CanonicalLfSha256 =
                    $expected["${prefix}CanonicalLfSha256"]
                EolStyle = 'LF'
                LineBreakCount = $expected["${prefix}LineBreakCount"]
            }
        })
}

function New-UdpCallbackTestSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedCandidate')]
        [string]$State
    )

    $classes = New-SyntheticClassesDatabase -State $State
    $derivedNetwork = New-SyntheticDerivedNetwork
    $configObjects = New-SyntheticConfigObjects -State $State
    $vendorPresent = $State -cne 'Absent'
    $derivedPresent = $State -ceq 'DerivedCandidate'
    $protected = @(
        foreach ($expected in $ProtectedDependencies) {
            [pscustomobject]@{
                Name = $expected.Name
                Bytes = $expected.Bytes
                Sha256 = $expected.Sha256
            }
        })
    return [pscustomobject]@{
        ProtectedDependencies = $protected
        GeneratedIncludes = @(New-SyntheticGeneratedIncludes -State $State)
        ProtectedGeneratedRecords = @(
            foreach ($expected in $ProtectedGeneratedRecordContracts) {
                [pscustomobject]@{
                    Name = $expected.Name
                    Bytes = $expected.Bytes
                    Sha256 = $expected.Sha256
                }
            })
        VendorGeneratedRecords = @(
            if ($vendorPresent) {
                foreach ($expected in $VendorGeneratedRecordContracts) {
                    [pscustomobject]@{
                        Name = $expected.Name
                        Bytes = $expected.Bytes
                        Sha256 = $expected.Sha256
                    }
                }
            })
        ForbiddenPaths = @()
        TransceiverPresent = $vendorPresent
        InterfacePresent = $vendorPresent
        DerivedPresent = $derivedPresent
        TransceiverSource = if ($vendorPresent) {
            New-SyntheticVendorTransceiverSource
        } else { '' }
        InterfaceSource = if ($vendorPresent) {
            New-SyntheticVendorInterfaceSource
        } else { '' }
        TransceiverCanonicalLfSha256 = if ($vendorPresent) {
            $ExpectedVendor.Transceiver.CanonicalLfSha256
        } else { '' }
        TransceiverRawSha256 = if ($vendorPresent) {
            $ExpectedVendor.Transceiver.CanonicalLfSha256
        } else { '' }
        TransceiverRawBytes = if ($vendorPresent) {
            $ExpectedVendor.Transceiver.CanonicalLfBytes
        } else { 0 }
        TransceiverEolStyle = if ($vendorPresent) { 'LF' } else { 'Absent' }
        TransceiverLineBreakCount = if ($vendorPresent) {
            $ExpectedVendor.Transceiver.LineBreakCount
        } else { 0 }
        InterfaceCanonicalLfSha256 = if ($vendorPresent) {
            $ExpectedVendor.Interface.CanonicalLfSha256
        } else { '' }
        InterfaceRawSha256 = if ($vendorPresent) {
            $ExpectedVendor.Interface.CanonicalLfSha256
        } else { '' }
        InterfaceRawBytes = if ($vendorPresent) {
            $ExpectedVendor.Interface.CanonicalLfBytes
        } else { 0 }
        InterfaceEolStyle = if ($vendorPresent) { 'LF' } else { 'Absent' }
        InterfaceLineBreakCount = if ($vendorPresent) {
            $ExpectedVendor.Interface.LineBreakCount
        } else { 0 }
        DerivedSource = if ($derivedPresent) {
            New-SyntheticDerivedSource
        } else { '' }
        TcpSource = New-SyntheticTcpSource -WithCallbackSender:$derivedPresent
        TcpSha256 = if ($derivedPresent) {
            'SYNTHETIC-DERIVED-TCP'
        } else { $ExpectedBaselineTcpSha256 }
        ClassesDatabaseBytes = $classes.Bytes
        ClassesDatabaseText = $classes.Text
        ClassesBytes = if ($State -ceq 'VendorImported') {
            $ExpectedVendorImportedClassesBytes
        } else { $classes.Bytes.Count }
        ClassesSha256 = if ($State -ceq 'Absent') {
            $ExpectedAbsentClassesSha256
        } elseif ($State -ceq 'VendorImported') {
            $ExpectedVendorImportedClassesSha256
        } else { 'SYNTHETIC-CLASSES' }
        ProjectBytes = 1
        ProjectSha256 = 'SYNTHETIC-PROJECT'
        ProjectDefinitionBytes = 1
        ProjectDefinitionSha256 = 'SYNTHETIC-LCP'
        ProjectDefinitionText = New-SyntheticProjectDefinition -State $State
        ProjectDatabaseText = 'canonical-project-database'
        FullNetworkCount = if ($derivedPresent) {
            $ExpectedBaselineNetworkInventoryCount
        } else { $ExpectedBaselineNetworkInventoryCount }
        FullNetworkSha256 = if ($derivedPresent) {
            'SYNTHETIC-DERIVED-NETWORK'
        } elseif ($vendorPresent) {
            $ExpectedVendorImportedNetworkInventorySha256
        } else { $ExpectedBaselineNetworkInventorySha256 }
        TrackedNetworkCount = $ExpectedBaselineTrackedNetworkCount
        TrackedNetworkSha256 = if ($derivedPresent) {
            'SYNTHETIC-DERIVED-NETWORK'
        } elseif ($vendorPresent) {
            $ExpectedVendorImportedTrackedNetworkSha256
        } else { $ExpectedBaselineTrackedNetworkSha256 }
        ProtectedTrackedNetworkCount = $ExpectedProtectedTrackedNetworkCount
        ProtectedTrackedNetworkSha256 =
            $ExpectedProtectedTrackedNetworkSha256
        ConfigObjectsText = $configObjects
        ConfigObjectsBytes = if ($vendorPresent) {
            $ExpectedVendorImportedConfigObjectsBytes
        } else { 0 }
        ConfigObjectsSha256 = if ($vendorPresent) {
            $ExpectedVendorImportedConfigObjectsSha256
        } else { '' }
        NetworksDatabaseBytes = if ($vendorPresent) {
            $ExpectedVendorImportedNetworksDatabaseBytes
        } else { 0 }
        NetworksDatabaseSha256 = if ($vendorPresent) {
            $ExpectedVendorImportedNetworksDatabaseSha256
        } else { '' }
        CommNetworkText = if ($derivedPresent) { $derivedNetwork.Xml } else { '' }
        CommTableText = if ($derivedPresent) { $derivedNetwork.Table } else { '' }
        NetworksDatabaseText = if ($derivedPresent) {
            $derivedNetwork.Database
        } else { '' }
    }
}

function Set-SyntheticGeneratedHeaderByte {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][int]$HeaderOffset,
        [Parameter(Mandatory = $true)][byte]$Value
    )

    $bytes = [byte[]]::new($Snapshot.ClassesDatabaseBytes.Count)
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        $bytes,
        $Snapshot.ClassesDatabaseBytes.Count)
    $text = [Text.Encoding]::Latin1.GetString($bytes)
    $nameStart = $text.IndexOf($FunctionName, [StringComparison]::Ordinal)
    if ($nameStart -lt 0) {
        throw "self-test fixture method is missing: $FunctionName"
    }
    $headerStart = $nameStart + $FunctionName.Length
    if ($bytes[$headerStart] -ne 0x0B) {
        throw "self-test fixture method header is malformed: $FunctionName"
    }
    $bytes[$headerStart + $HeaderOffset] = $Value
    $Snapshot.ClassesDatabaseBytes = $bytes
    $Snapshot.ClassesDatabaseText = [Text.Encoding]::Latin1.GetString($bytes)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Invalidate-SyntheticClassesEvidence {
    param([Parameter(Mandatory = $true)][pscustomobject]$Snapshot)

    $Snapshot.ClassesSha256 = 'SYNTHETIC-MUTATION'
    foreach ($record in @($Snapshot.VendorGeneratedRecords)) {
        $record.Sha256 = 'SYNTHETIC-MUTATION'
    }
}

function Replace-SyntheticClassesToken {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)][string]$New,
        [string]$After = ''
    )

    $start = if ($After.Length -eq 0) {
        0
    }
    else {
        $anchor = $Snapshot.ClassesDatabaseText.IndexOf(
            $After,
            [StringComparison]::Ordinal)
        if ($anchor -lt 0) {
            throw "synthetic Classes replacement anchor is missing: $After"
        }
        $anchor + $After.Length
    }
    $index = $Snapshot.ClassesDatabaseText.IndexOf(
        $Old,
        $start,
        [StringComparison]::Ordinal)
    if ($index -lt 0) {
        throw "synthetic Classes replacement token is missing: $Old"
    }
    $before = [byte[]]::new($index)
    [Array]::Copy($Snapshot.ClassesDatabaseBytes, 0, $before, 0, $index)
    $oldEnd = $index + $Old.Length
    $afterBytes = [byte[]]::new(
        $Snapshot.ClassesDatabaseBytes.Count - $oldEnd)
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        $oldEnd,
        $afterBytes,
        0,
        $afterBytes.Count)
    $replacement = [Text.Encoding]::ASCII.GetBytes($New)
    $updated = [byte[]]::new(
        $before.Count + $replacement.Count + $afterBytes.Count)
    [Array]::Copy($before, 0, $updated, 0, $before.Count)
    [Array]::Copy(
        $replacement,
        0,
        $updated,
        $before.Count,
        $replacement.Count)
    [Array]::Copy(
        $afterBytes,
        0,
        $updated,
        $before.Count + $replacement.Count,
        $afterBytes.Count)
    $Snapshot.ClassesDatabaseBytes = $updated
    $Snapshot.ClassesDatabaseText = [Text.Encoding]::Latin1.GetString($updated)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Swap-SyntheticClassesTokens {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$First,
        [Parameter(Mandatory = $true)][string]$Second,
        [Parameter(Mandatory = $true)][string]$After
    )

    if ($First.Length -ne $Second.Length) {
        throw 'synthetic Classes swap tokens must have equal lengths.'
    }
    $anchor = $Snapshot.ClassesDatabaseText.IndexOf(
        $After,
        [StringComparison]::Ordinal)
    $firstIndex = $Snapshot.ClassesDatabaseText.IndexOf(
        $First,
        $anchor + $After.Length,
        [StringComparison]::Ordinal)
    $secondIndex = $Snapshot.ClassesDatabaseText.IndexOf(
        $Second,
        $firstIndex + $First.Length,
        [StringComparison]::Ordinal)
    if (($anchor -lt 0) -or ($firstIndex -lt 0) -or ($secondIndex -lt 0)) {
        throw 'synthetic Classes swap token is missing.'
    }
    $firstBytes = [Text.Encoding]::ASCII.GetBytes($First)
    $secondBytes = [Text.Encoding]::ASCII.GetBytes($Second)
    [Array]::Copy(
        $secondBytes,
        0,
        $Snapshot.ClassesDatabaseBytes,
        $firstIndex,
        $secondBytes.Count)
    [Array]::Copy(
        $firstBytes,
        0,
        $Snapshot.ClassesDatabaseBytes,
        $secondIndex,
        $firstBytes.Count)
    $Snapshot.ClassesDatabaseText = [Text.Encoding]::Latin1.GetString(
        $Snapshot.ClassesDatabaseBytes)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Set-SyntheticCallbackOutputCount {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][int]$InputCount,
        [Parameter(Mandatory = $true)][byte]$Value
    )

    $text = $Snapshot.ClassesDatabaseText
    $nameStart = $text.IndexOf($FunctionName, [StringComparison]::Ordinal)
    if ($nameStart -lt 0) {
        throw "synthetic callback is missing: $FunctionName"
    }
    $cursor = $nameStart + $FunctionName.Length + 12
    for ($index = 0; $index -lt $InputCount; $index++) {
        $nameLength = [uint32](
            $Snapshot.ClassesDatabaseBytes[$cursor + 2] -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 3] -shl 8) -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 4] -shl 16))
        $cursor += 6 + $nameLength
        $typeLength = [uint32](
            $Snapshot.ClassesDatabaseBytes[$cursor] -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 1] -shl 8) -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 2] -shl 16))
        $cursor += 4 + $typeLength
    }
    $Snapshot.ClassesDatabaseBytes[$cursor] = $Value
    $Snapshot.ClassesDatabaseText = [Text.Encoding]::Latin1.GetString(
        $Snapshot.ClassesDatabaseBytes)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Assert-UdpCallbackNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Action
    )

    $message = $null
    try {
        & $Action
    }
    catch {
        $message = $_.Exception.Message
    }
    if ($null -eq $message) {
        throw "UDP callback verifier accepted negative fixture '$Name'."
    }
    if (-not $message.StartsWith(
            "$Owner blocker:",
            [StringComparison]::Ordinal)) {
        throw (
            "UDP callback negative fixture '$Name' did not fail through the " +
            "owned blocker: $message")
    }
    return 1
}

function Invoke-UdpCallbackVerifierSelfTest {
    foreach ($positive in @(
            @{ State = 'Absent'; PermitAbsent = $true },
            @{ State = 'VendorImported'; PermitAbsent = $false })) {
        $snapshot = New-UdpCallbackTestSnapshot -State $positive.State
        $result = Assert-LasalUdpCallbackStateContract `
            -Snapshot $snapshot `
            -PermitAbsent $positive.PermitAbsent `
            -RequiredState $positive.State
        if ($result.State -cne $positive.State) {
            throw "UDP callback positive fixture state drifted: $($positive.State)"
        }
    }

    $crLfVendor = New-UdpCallbackTestSnapshot -State VendorImported
    $crLfVendor.TransceiverSource =
        (ConvertTo-CanonicalLf -Text $crLfVendor.TransceiverSource).
            Replace("`n", "`r`n")
    $crLfVendor.TransceiverRawSha256 =
        $ExpectedVendor.Transceiver.CodeGeneratorCrLfSha256
    $crLfVendor.TransceiverRawBytes =
        $ExpectedVendor.Transceiver.CodeGeneratorCrLfBytes
    $crLfVendor.TransceiverEolStyle = 'CRLF'
    $crLfVendor.InterfaceSource =
        (ConvertTo-CanonicalLf -Text $crLfVendor.InterfaceSource).
            Replace("`n", "`r`n")
    $crLfVendor.InterfaceRawSha256 =
        $ExpectedVendor.Interface.CodeGeneratorCrLfSha256
    $crLfVendor.InterfaceRawBytes =
        $ExpectedVendor.Interface.CodeGeneratorCrLfBytes
    $crLfVendor.InterfaceEolStyle = 'CRLF'
    foreach ($include in $crLfVendor.GeneratedIncludes) {
        $expectedInclude = @($GeneratedIncludeContracts | Where-Object {
                $_.Name -ceq $include.Name
            })[0]
        $include.Text = (ConvertTo-CanonicalLf -Text $include.Text).
            Replace("`n", "`r`n")
        $include.RawBytes = $expectedInclude.VendorCrLfBytes
        $include.RawSha256 = $expectedInclude.VendorCrLfSha256
        $include.EolStyle = 'CRLF'
    }
    $crLfResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $crLfVendor `
        -PermitAbsent $false `
        -RequiredState VendorImported
    if ($crLfResult.State -cne 'VendorImported') {
        throw 'UDP callback CRLF positive fixture state drifted.'
    }

    $negativeCount = 0
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Absent without explicit pre-import mode' -Action {
            $s = New-UdpCallbackTestSnapshot -State Absent
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'partial vendor transceiver only' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfacePresent = $false
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'partial vendor interface only' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverPresent = $false
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'complete DerivedCandidate before Gate B approval' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'transceiver canonical drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverCanonicalLfSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'transceiver physical drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverRawBytes++
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'interface canonical drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceCanonicalLfSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor mixed line endings' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceEolStyle = 'Mixed'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Include canonical drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[0].CanonicalLfSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Include physical drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[1].RawBytes++
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Include mixed line endings' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[2].EolStyle = 'Mixed'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'C channel struct field drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[0].Text =
                $s.GeneratedIncludes[0].Text.Replace(
                    'CMDMETH *pMeth;', 'CHMETH *pMeth;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'ST channel struct extra' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[1].Text +=
                "`nCltChCmd__UDPTransceiverExtra : STRUCT END_STRUCT;"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'public UDP enum drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[2].Text =
                $s.GeneratedIncludes[2].Text.Replace(
                    '_STATE_ERROR_UDP', '_STATE_ERROR_UDP_DRIFT')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'public UDP class block extra' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[2].Text +=
                "`n_UDPTransceiverExtra : CLASS_PUBLIC END_CLASS;"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Include Unknown residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.GeneratedIncludes[0].Text += '`n<Unknown>'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor sError promoted to command server' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource = $s.TransceiverSource.Replace(
                'sError : SvrCh_DINT;', 'sError : SvrChCmd_DINT;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor sError gains object Class metadata' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource = $s.TransceiverSource.Replace(
                'Name="sError"',
                'Name="sError" Class="_UDPTransceiver"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor sError command-table kind drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource = $s.TransceiverSource.Replace(
                '_CH_SVR$UINT', '_CH_SVR_OBJ$UINT')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor sError gains StoreCmd' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource +=
                "`nsError.pMeth := StoreCmd(pCmd := 0, SHARED);`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor generated Unknown metadata' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource += "`n<Unknown/>`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor embedded CriticalSection object drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TransceiverSource = $s.TransceiverSource.Replace(
                'Class="CriticalSection"', 'Class="WrongCriticalSection"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor embedded CriticalSection connection drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceSource = $s.InterfaceSource.Replace(
                'Destination="CriticalSection_UDP.ClassSvr"',
                'Destination="WrongCriticalSection.ClassSvr"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor interface Unknown metadata' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceSource += "`n<Unknown/>`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor InfoCallback input missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceSource = $s.InterfaceSource.Replace(
                'InfoPara2 : DINT;', '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor ErrorCallback input missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.InterfaceSource = $s.InterfaceSource.Replace(
                'ErrCode : DINT;', '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected dependency overwrite' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProtectedDependencies[0].Sha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected dependency missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProtectedDependencies = @($s.ProtectedDependencies | Select-Object -Skip 1)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected generated dependency metadata missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesDatabaseText = $s.ClassesDatabaseText.Replace(
                '.\Source\code\OsiBaseNew.h', 'WrongOsiBaseHeader')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected generated record hash drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProtectedGeneratedRecords[0].Sha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor generated record hash drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.VendorGeneratedRecords[0].Sha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A whole Classes hash drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A whole Classes byte count drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesBytes++
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A TCP drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TcpSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A tracked Network drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.TrackedNetworkSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A full Network drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.FullNetworkSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A protected topology drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProtectedTrackedNetworkSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A ConfigObjects hash drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ConfigObjectsSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A ConfigObjects vendor revision drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ConfigObjectsText = $s.ConfigObjectsText.Replace(
                '0$UINT, 1, 3, "_UDPTRANSCEIVERINTERFACE",',
                '0$UINT, 1, 4, "_UDPTRANSCEIVERINTERFACE",')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate A Networks database hash drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.NetworksDatabaseSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'duplicate vendor source registration' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesDatabaseText +=
                '.\Class\_UDPTransceiver\_UDPTransceiver.st'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'forbidden demo path' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ForbiddenPaths = @('Class/UDPTransmission')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'forbidden demo registration' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesDatabaseText += '.\Class\UDPTransmission\UDPTransmission.st'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'forbidden MotionTCPDemo library reference' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProjectDefinitionText = 'Library Name="MotionTCPDemo"'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'project definition vendor class missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProjectDefinitionText = $s.ProjectDefinitionText.Replace(
                '<Class Name="_UDPTransceiver"/>', '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'project definition vendor file duplicate' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProjectDefinitionText +=
                '<File Path=".\Class\_UDPTransceiver\_UDPTransceiver.st"/>'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'project definition premature derived registration' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProjectDefinitionText +=
                '<Class Name="LMCUdpCallbackSender"/>'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'project database wrong UDP object residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ProjectDatabaseText += ' _UDPTransceiver1 '
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor generated method scope drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName AddSocket -HeaderOffset 4 -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor generated channel ABI missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $s.ClassesDatabaseText = $s.ClassesDatabaseText.Replace(
                'ClassSvr', 'WrongSvr')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'transceiver generated record Unknown residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $boundary = '_UDPTransceiverInterface' + [char]0 +
                '<Unknown>' + [char]0 +
                '.\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st'
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -Old $boundary `
                -New ('<UnknownVendorCallbackType>' + $boundary)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'interface generated record Unknown residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $boundary = 'ASCII_BIN' + [char]0 + '<Unknown>' + [char]0 +
                '.\Class\ASCII_BIN\ASCII_BIN.st'
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -Old $boundary `
                -New ('<UnknownVendorCallbackType>' + $boundary)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated sError promoted to command server' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -Old ('sError' + [char]0 + 'SvrCh_DINT') `
                -New ('sError' + [char]0 + 'SvrChCmd_DINT')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated InfoCallback input name drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Replace-SyntheticClassesToken `
                -Snapshot $s -After 'InfoCallback' `
                -Old 'InfoPara1' -New 'WrongPara'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated InfoCallback input type drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Replace-SyntheticClassesToken `
                -Snapshot $s -After 'InfoCallback' `
                -Old 'DINT' -New 'UINT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated InfoCallback input reorder' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Swap-SyntheticClassesTokens `
                -Snapshot $s -After 'InfoCallback' `
                -First 'InfoPara1' -Second 'InfoPara2'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated ErrorCallback output count drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            Set-SyntheticCallbackOutputCount `
                -Snapshot $s `
                -FunctionName ErrorCallback `
                -InputCount 3 `
                -Value 1
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    if ($DerivedCandidateApproved) {
        $negativeCount += Assert-UdpCallbackNegativeFixture `
            -Name 'derived cyclic metadata drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'CyclicTask = "true"', 'CyclicTask = "false"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived base class drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                ': _UDPTransceiverInterface', ': WrongBase')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'public function made private' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'FUNCTION GLOBAL ArmEndpoint', 'FUNCTION ArmEndpoint')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'public function made virtual global' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'FUNCTION GLOBAL DisarmEndpoint',
                'FUNCTION VIRTUAL GLOBAL DisarmEndpoint')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'public input ABI drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'ProtocolVersion : UINT;', 'ProtocolVersion : UDINT;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'private function made global' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'FUNCTION EnsureSocketReady;',
                'FUNCTION GLOBAL EnsureSocketReady;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'private function gains input ABI' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'FUNCTION ValidateEndpoint;',
                "FUNCTION ValidateEndpoint`n        VAR_INPUT`n" +
                "            Drift : DINT;`n        END_VAR;")
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'duplicate implementation' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource +=
                "FUNCTION LMCUdpCallbackSender::FenceMatches`nEND_FUNCTION`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'custom dynamic allocation' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource += "MallocV1(4);`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'direct UDP send' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource += "bDirect := TRUE;`n"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'CallbackSender required client' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.TcpSource = $s.TcpSource.Replace(
                'Required="false"', 'Required="true"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'CallbackSender declaration missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.TcpSource = $s.TcpSource.Replace(
                'CallbackSender : CltChCmd_LMCUdpCallbackSender;',
                'WrongClient : CltChCmd_LMCUdpCallbackSender;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'wrong UDP object name' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                'LMCUdpTransceiver1', '_UDPTransceiver1')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'UDP TX buffer drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                'Value="8 kb"', 'Value="4 kb"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'extra direct UDP callback link' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $extra = '<Connection Source="LMCUdpCallbackSender1.Control" ' +
                'Destination="LMCControlCommandService1.ClassSvr"/>'
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                '</Connections>', "$extra</Connections>")
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected Network artifact drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.ProtectedTrackedNetworkSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated public scope drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName ArmEndpoint -HeaderOffset 5 -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated public input count drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName ArmEndpoint -HeaderOffset 8 -Value 8
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated private scope drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName EnsureSocketReady -HeaderOffset 5 -Value 1
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated TCP client ABI missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.ClassesDatabaseText = $s.ClassesDatabaseText.Replace(
                'CallbackSender', 'CallbackSendeX')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Network table missing token' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommTableText = $s.CommTableText.Replace(
                'LMCUdpCallbackSender1', 'WrongSender')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'source UTF-8 BOM' -Action {
            $null = Get-AsciiTextEvidence `
                -Bytes ([byte[]]@(0xEF, 0xBB, 0xBF, 0x41)) `
                -SourceOwner 'BOM fixture' -RequireUniformEol
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'source non-ASCII byte' -Action {
            $null = Get-AsciiTextEvidence `
                -Bytes ([byte[]]@(0x41, 0x80, 0x0A)) `
                -SourceOwner 'non-ASCII fixture' -RequireUniformEol
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'source mixed EOL' -Action {
            $null = Get-AsciiTextEvidence `
                -Bytes ([Text.Encoding]::ASCII.GetBytes("A`r`nB`n")) `
                -SourceOwner 'mixed-EOL fixture' -RequireUniformEol
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Absent Classes baseline drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State Absent
            $s.ClassesSha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $true
        }
    return $negativeCount
}

if ($RunSelfTest) {
    $negativeCount = Invoke-UdpCallbackVerifierSelfTest
    Write-Output (
        'PASS LASAL.UdpCallbackContract.SelfTest ' +
        "($negativeCount/$negativeCount negative fixtures rejected; " +
        'Absent explicit and VendorImported positives accepted; ' +
        'DerivedCandidate fail-closed pending corrected Gate B contract)')
    return
}

if ($VerifyCurrent) {
    Assert-LasalIdeClosed
    $snapshot = Get-CurrentRepositorySnapshot -Root $RepositoryRoot
    Assert-LasalIdeClosed
    $result = Assert-LasalUdpCallbackStateContract `
        -Snapshot $snapshot `
        -PermitAbsent $AllowPreImportAbsent.IsPresent `
        -RequiredState $ExpectedState
    $dependencyEvidence = [string]::Join(',', @(
            $snapshot.ProtectedDependencies |
                ForEach-Object { "$($_.Name)=$($_.Bytes)/$($_.Sha256)" }))
    $includeEvidence = [string]::Join(',', @(
            $snapshot.GeneratedIncludes |
                ForEach-Object {
                    "$($_.Name)=$($_.RawBytes)/$($_.RawSha256)"
                }))
    Write-Output (
        'PASS LASAL.UdpCallbackContract.Current ' +
        "(state=$($result.State); IDEClosed=true; " +
        "vendor=$($snapshot.TransceiverRawBytes)/" +
        "$($snapshot.TransceiverRawSha256)," +
        "$($snapshot.InterfaceRawBytes)/$($snapshot.InterfaceRawSha256); " +
        "Classes=$($snapshot.ClassesBytes)/$($snapshot.ClassesSha256); " +
        "project=$($snapshot.ProjectBytes)/$($snapshot.ProjectSha256); " +
        "lcp=$($snapshot.ProjectDefinitionBytes)/" +
        "$($snapshot.ProjectDefinitionSha256); " +
        "Includes=$includeEvidence; " +
        "TCP=$($snapshot.TcpSha256); " +
        "Network=$($snapshot.FullNetworkCount)/" +
        "$($snapshot.FullNetworkSha256),tracked=" +
        "$($snapshot.TrackedNetworkCount)/$($snapshot.TrackedNetworkSha256); " +
        "protected=$dependencyEvidence)")
    return
}

throw "$Owner blocker: no operation was selected."

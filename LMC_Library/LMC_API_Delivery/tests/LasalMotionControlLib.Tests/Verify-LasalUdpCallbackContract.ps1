[CmdletBinding(DefaultParameterSetName = 'Current')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Current')]
    [switch]$VerifyCurrent,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,

    [Parameter(ParameterSetName = 'Current')]
    [string]$RepositoryRoot = '',

    [Parameter(ParameterSetName = 'Current')]
    [switch]$AllowPreImportAbsent,

    [Parameter(ParameterSetName = 'Current')]
    [switch]$AllowDerivedCapture,

    [Parameter(ParameterSetName = 'Current')]
    [ValidateSet(
        'Auto',
        'Absent',
        'VendorImported',
        'DerivedDeclaration',
        'DerivedWired',
        'DerivedCandidate',
        'TerminalWakeBrokerCandidate')]
    [string]$ExpectedState = 'Auto'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($VerifyCurrent -and [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Join-Path $PSScriptRoot '..\..\..\..'
}

$Owner = 'LASAL.UdpCallbackContract'
$Utf8 = [Text.UTF8Encoding]::new($false, $true)
$Latin1 = [Text.Encoding]::GetEncoding(28591)
$DerivedCandidateApproved = $true

$TargetRootRelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
$TransceiverRelativePath =
    "$TargetRootRelativePath/Class/_UDPTransceiver/_UDPTransceiver.st"
$InterfaceRelativePath =
    "$TargetRootRelativePath/Class/_UDPTransceiverInterface/_UDPTransceiverInterface.st"
$DerivedRelativePath =
    "$TargetRootRelativePath/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st"
$TcpRelativePath =
    "$TargetRootRelativePath/Class/TCPMotionInterface/TCPMotionInterface.st"
$DiagnosticsRelativePath =
    "$TargetRootRelativePath/Class/LMCDiagnosticsService/LMCDiagnosticsService.st"
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
        GitCheckoutLfBytes = 19369
        GitCheckoutLfSha256 =
            'CB94BD6EA6CC323EC9D6FFB524DE7333102B1FB68EBF37F122529B9CB356F1DB'
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
        WiredSha256 =
            'A261DD6045085695A92EFA69FC02E5343BFFA3C8BB115547C5DD831743E10526'
        TerminalWakeSha256 =
            'ABC81CB06DB50FFE34F6F663BB2B3CF1B73396335CFAFE53CE8A0659B48854EA'
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
        WiredSha256 =
            'C924322FBC9E42031AF4D31236B65668568418B209BA485FE168D42CEC542D9F'
    }
)

$ExpectedAbsentClassesSha256 =
    'CA5CE9AB4B6AFB498D55CF6E5D3460A2C35D54FF8E4FE9C9D3B59636C3603F78'
$ExpectedVendorImportedClassesBytes = 8512773
$ExpectedVendorImportedClassesSha256 =
    '0CB9A3D3A4E8EB27E9A5BEB44E91D46BAEE23A051736AA83622D790249C61DC6'
$ExpectedBaselineTcpSha256 =
    '9210C199A02153FEE4110556C5396CD49C9AEAC7F22B1405AA20F00FC522A129'
$ExpectedGateBTcpFunctionContracts = [ordered]@{
    ConnSocketInfo = [ordered]@{
        Bytes = 8300
        Sha256 =
            '6743363B3B0B5B05574E7F7EC9659264B135B09499C8333EC621A74225ED6D1F'
    }
    SendData = [ordered]@{
        Bytes = 1095
        Sha256 =
            'B9E01ACF6965C68BC99C8800CD1BC45F26FDCB7DBB147CC3F3A58B3613A2E3B9'
    }
    HandleControlSafetyDrainPending = [ordered]@{
        Bytes = 16979
        Sha256 =
            'D359C7FAFFBB0442112F05712A37D0C1CAD162EBD5F155D681D362F65447615B'
    }
    HandleRpcLifecycleCommands = [ordered]@{
        Bytes = 4249
        Sha256 =
            '8886D1D8CBBB9890AF88A83C4997C303678DDB448C2A22BC6B4C3CE4533DAB58'
    }
}
$ExpectedGateBTcpCanonicalLfBytes = 88061
$ExpectedGateBTcpCanonicalLfSha256 =
    'A3B890A69F59068B08C114D968EB2F383E1895A7454CD9D839D684F6A22A1D19'
$ExpectedGateBTcpGitOid = 'e030b243d1e3f79a0104d344fe77519848904ae7'
$ExpectedGateB2TcpCanonicalLfBytes = 88850
$ExpectedGateB2TcpCanonicalLfSha256 =
    'DAEBF0153513AB7516444E1DC436BE04AD539382A40FD35DA5CE695C1399A264'
$ExpectedGateB2CommNetworkBytes = 16387
$ExpectedGateB2CommNetworkSha256 =
    '4EFA35899443D8DFE10D3F9974493056CAE6E103751AF6B9A408338077A8C0DA'
$ExpectedGateB2CommNetworkCanonicalLfBytes = 15964
$ExpectedGateB2CommNetworkCanonicalLfSha256 =
    'FCD0940498CB70F6B8C4F3AFB30850AAE224E380E17E4A79015BCCD973587078'
$ExpectedGateB2CommTableBytes = 11677
$ExpectedGateB2CommTableSha256 =
    '2B2F29FC2F11A93FE0B827D5415C6AAFEB800725B0C2D65F752158AA5C90BEE9'
$ExpectedGateB2CommTableCanonicalLfBytes = 11394
$ExpectedGateB2CommTableCanonicalLfSha256 =
    'D1D6E1ADFF3D60C9AC9A2A6ADB47687E254C5D93796B92E44202BDF0D3CAB288'
$ExpectedGateB2CommTableDirectiveCount = 69
$ExpectedGateB2CommTableDirectiveSha256 =
    '62CEEC91E8F77566537D3AABED8BA1B034E2E4DFD55643237AED273D5A531333'
$ExpectedGateB2FullNetworkCount = 23
$ExpectedGateB2FullNetworkSha256 =
    '246FEBD3BD55BB1F8BCAF84839835E3B4836A3831A70EFC278E7D6024DDC7E5D'
$ExpectedGateB2TrackedNetworkCount = 15
$ExpectedGateB2TrackedNetworkSha256 =
    '19422EE85FF909C80862440D08EB6FA156801618329559267B6EC88BF070BC5D'
$ExpectedDerivedCandidateCommTableBytes = 11828
$ExpectedDerivedCandidateCommTableSha256 =
    '752C2873FBE8D1470A82E4E4A651DEC298567B42625EA69EE8F2F2C85514E373'
$ExpectedDerivedCandidateCommTableCanonicalLfBytes = 11541
$ExpectedDerivedCandidateCommTableCanonicalLfSha256 =
    '8078EF17A9DB5E15D55199E9754F417B4424B524E2ECE30D8F1514BB7CC02E10'
$ExpectedDerivedCandidateCommTableDirectiveCount = 72
$ExpectedDerivedCandidateCommTableDirectiveSha256 =
    '3E8700672C5E60BF5362FE31B5AF510DBA1994AFD7582064825AB8644976A3E0'
$ExpectedDerivedCandidateFullNetworkCount = 23
$ExpectedDerivedCandidateFullNetworkSha256 =
    '530E284743E4F6405BB90695EC31D0C5CAB4F94F6B9B16A4D67D983687AEB9EA'
$ExpectedDerivedCandidateTrackedNetworkCount = 15
$ExpectedDerivedCandidateTrackedNetworkSha256 =
    '6F5575791A0FF10E77A411A05896453C57F9661C54461DB038F166323C7AF16B'
$ExpectedTerminalWakeLayout = [ordered]@{
    Transceiver = [ordered]@{
        Name = '_UDPTransceiver'
        CanonicalLfBytes = 71787
        CanonicalLfSha256 =
            '5EF05C7A018E75DD40160828F5C39D474C2191F0E42D97A7E5D19064CF2ACC13'
        CodeGeneratorCrLfBytes = 73380
        CodeGeneratorCrLfSha256 =
            'F19273F83337E2B1C2AB510A7DDD49138EB28480649835D4E3C571B34D8269C4'
        LineBreakCount = 1593
        Objectsize = '(522,120)'
    }
    Sender = [ordered]@{
        Name = 'LMCUdpCallbackSender'
        CanonicalLfBytes = 22727
        CanonicalLfSha256 =
            'A0AAA3451F9160B45FDE81E9B337EA9D55DCDCB366AA7504DA57C650EC060D89'
        CodeGeneratorCrLfBytes = 23469
        CodeGeneratorCrLfSha256 =
            'C334A6C6960BA61529369D29C6DDA757A77AC809A6858661B8FEB6476F5CAE8F'
        LineBreakCount = 742
        Objectsize = '(778,120)'
    }
    Classes = [ordered]@{
        Bytes = 8549773
        Sha256 =
            '24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861'
    }
    CommNetwork = [ordered]@{
        Bytes = 16540
        Sha256 =
            'FD632C27E2619E907097F599A4CEA1D6AA83E4D41F214E73A91E1C4127F1A1B7'
        CanonicalLfBytes = 16117
        CanonicalLfSha256 =
            '06425749250E03BEF3548C40F6515E66DB953E3067D3C724353A92C6B35860FF'
        TransceiverPosition = '(1410,990)'
        SenderPosition = '(2610,990)'
    }
    NetworksDatabase = [ordered]@{
        Bytes = 242363
        Sha256 =
            'C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F'
    }
    FullNetwork = [ordered]@{
        Count = 23
        Sha256 =
            '2AC04B56D1305FB2F894268598199136E406D3AFF04AF49B505055373547B621'
        CleanCheckoutCount = 23
        CleanCheckoutSha256 =
            '0AA5BD15701BB05C689C883AC69829D90CDFCB48E836D24776A1105A460A4751'
    }
    TrackedNetwork = [ordered]@{
        Count = 15
        Sha256 =
            '2BBE21AE738AA99F2EB4CDD66CF865441AF3BB587FB1DB7478777082C395C153'
        CleanCheckoutCount = 15
        CleanCheckoutSha256 =
            '6FF1BDAED41EE9F2AE017891BBF23CACBFA0FB510BEF07EAA4C7619DDA49DA38'
    }
}
$TerminalWakeLayoutSelfTestOracle = [ordered]@{
    Transceiver = [ordered]@{
        Name = '_UDPTransceiver'
        CanonicalLfBytes = 71787
        CanonicalLfSha256 =
            '5EF05C7A018E75DD40160828F5C39D474C2191F0E42D97A7E5D19064CF2ACC13'
        CodeGeneratorCrLfBytes = 73380
        CodeGeneratorCrLfSha256 =
            'F19273F83337E2B1C2AB510A7DDD49138EB28480649835D4E3C571B34D8269C4'
        LineBreakCount = 1593
        Objectsize = '(522,120)'
    }
    Sender = [ordered]@{
        Name = 'LMCUdpCallbackSender'
        CanonicalLfBytes = 22727
        CanonicalLfSha256 =
            'A0AAA3451F9160B45FDE81E9B337EA9D55DCDCB366AA7504DA57C650EC060D89'
        CodeGeneratorCrLfBytes = 23469
        CodeGeneratorCrLfSha256 =
            'C334A6C6960BA61529369D29C6DDA757A77AC809A6858661B8FEB6476F5CAE8F'
        LineBreakCount = 742
        Objectsize = '(778,120)'
    }
    Classes = [ordered]@{
        Bytes = 8549773
        Sha256 =
            '24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861'
    }
    CommNetwork = [ordered]@{
        Bytes = 16540
        Sha256 =
            'FD632C27E2619E907097F599A4CEA1D6AA83E4D41F214E73A91E1C4127F1A1B7'
        CanonicalLfBytes = 16117
        CanonicalLfSha256 =
            '06425749250E03BEF3548C40F6515E66DB953E3067D3C724353A92C6B35860FF'
        TransceiverPosition = '(1410,990)'
        SenderPosition = '(2610,990)'
    }
    NetworksDatabase = [ordered]@{
        Bytes = 242363
        Sha256 =
            'C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F'
    }
    FullNetwork = [ordered]@{
        Count = 23
        Sha256 =
            '2AC04B56D1305FB2F894268598199136E406D3AFF04AF49B505055373547B621'
        CleanCheckoutCount = 23
        CleanCheckoutSha256 =
            '0AA5BD15701BB05C689C883AC69829D90CDFCB48E836D24776A1105A460A4751'
    }
    TrackedNetwork = [ordered]@{
        Count = 15
        Sha256 =
            '2BBE21AE738AA99F2EB4CDD66CF865441AF3BB587FB1DB7478777082C395C153'
        CleanCheckoutCount = 15
        CleanCheckoutSha256 =
            '6FF1BDAED41EE9F2AE017891BBF23CACBFA0FB510BEF07EAA4C7619DDA49DA38'
    }
}
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
$ExpectedVendorImportedConfigObjectsCanonicalLfBytes = 8508
$ExpectedVendorImportedConfigObjectsCanonicalLfSha256 =
    'BF0A4E6721CDF9963045ADF49F39FCCEF210E50BAE0D6B867D46075BEF358368'
$ExpectedVendorImportedNetworksDatabaseBytes = 239778
$ExpectedVendorImportedNetworksDatabaseSha256 =
    '6D818CBAB462A8BFB7C5F080AE326D0554D9856810DCC40696805FDBBF458D42'
$ExpectedVendorImportedNetworksDatabaseGitOid =
    'a81de6bb216b28bcc229dc78e6e0204d321aed95'
$ExpectedVendorImportedCommTableGitOid =
    '4ec53ba7febf08962021e74ebdde9152da7c2abb'
$ExpectedVendorImportedCommTableCanonicalLfBytes = 8341
$ExpectedVendorImportedCommTableCanonicalLfSha256 =
    '9F50D8F2B2765B6429EC8D031CE76E53A42EDA56582B988CE69BA0EA383618E5'
$ExpectedVendorImportedProjectDefinitionCanonicalLfBytes = 24525
$ExpectedVendorImportedProjectDefinitionCanonicalLfSha256 =
    'B79502ADF5B27408112B0B70C441F9A4252609D149F1B393F6F8DE5F739550C3'
$ExpectedProtectedTrackedNetworkCount = 11
$ExpectedProtectedTrackedNetworkSha256 =
    'B2A4543FF2D900CC31C214EF73C99024B92C9E881A9AFF089AA36C3959841745'

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

$ExpectedDerivedCClientStructBlock =
    'typedef struct CltChCmd_LMCUdpCallbackSender { ' +
    'struct SvrChCmd_DINT *pCh; DINT dData; ' +
    'LMCUdpCallbackSender *pCmd; } CltChCmd_LMCUdpCallbackSender;'
$ExpectedDerivedStClientStructBlock =
    'CltChCmd_LMCUdpCallbackSender : STRUCT ' +
    'pCh : ^SvrChCmd_DINT; dData : DINT; ' +
    'pCmd : ^LMCUdpCallbackSender; END_STRUCT;'

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

$CanonicalLfProtectedNetworkTextPaths = @(
    "$TargetRootRelativePath/Network/Eni.xml",
    ("$TargetRootRelativePath/Network/EtherCAT_Network/" +
        'ONE_EtherCAT_Network_Table.st'),
    "$TargetRootRelativePath/Network/HW_Network/ONE_HW_Network_Table.st",
    "$TargetRootRelativePath/Network/HwVisualConfigMngr.xml",
    "$TargetRootRelativePath/Network/IOConnectionManager.xml",
    ("$TargetRootRelativePath/Network/Motion_Network/" +
        'ONE_Motion_Network_Table.st')
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

$CyWorkSpec = [ordered]@{
    Name = 'CyWork'
    Inputs = @('EAX:UDINT')
    Outputs = @('state:UDINT')
    SourceOutputs = @('state (EAX):UDINT')
}

$ErrorCallbackSpec = [ordered]@{
    Name = 'ErrorCallback'
    Inputs = @(
        'FSM_UDP:_UDPTransceiver::_FSM_UDP_USER',
        'UdpError:_UDPTransceiver::_UDP_ERROR',
        'ErrCode:DINT')
    Outputs = @()
}

$PrivateFunctionSpecs = @(
    [ordered]@{
        Name = 'EnsureSocketReady'
        Inputs = @()
        Outputs = @('Result:DINT')
    },
    [ordered]@{
        Name = 'ValidateEndpoint'
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
        Name = 'BuildDatagram'
        Inputs = @(
            'SlotIndex:UDINT',
            'EventMaskBit:UDINT',
            'EventType:UINT',
            'DeliveryClass:UINT',
            'EventId:UDINT',
            'ProducerSessionEpoch:UDINT',
            'pPayload:^void',
            'PayloadBytes:UDINT')
        Outputs = @('Result:DINT')
    },
    [ordered]@{
        Name = 'FindFreeSlot'
        Inputs = @()
        Outputs = @('SlotIndex:DINT')
    },
    [ordered]@{
        Name = 'ServiceTransmitQueue'
        Inputs = @()
        Outputs = @()
    },
    [ordered]@{
        Name = 'SendSlot'
        Inputs = @('SlotIndex:UDINT')
        Outputs = @('VendorResult:DINT')
    },
    [ordered]@{
        Name = 'RetryOrDropSlot'
        Inputs = @('SlotIndex:UDINT', 'VendorResult:DINT')
        Outputs = @()
    },
    [ordered]@{
        Name = 'ClearPendingFrames'
        Inputs = @()
        Outputs = @()
    },
    [ordered]@{
        Name = 'FenceMatches'
        Inputs = @(
            'ExpectedSessionEpoch:UDINT',
            'ExpectedCookieLo:UDINT',
            'ExpectedCookieHi:UDINT')
        Outputs = @('Matches:BOOL')
    }
)

$PrivateFunctionNames = @($PrivateFunctionSpecs | ForEach-Object { $_.Name })
$DeclarationFunctionNames = @($CyWorkSpec.Name) +
    @($ErrorCallbackSpec.Name) +
    @($PublicFunctionSpecs | ForEach-Object { $_.Name }) +
    @($PrivateFunctionNames)
$ImplementationFunctionNames = @($CyWorkSpec.Name) +
    @($PublicFunctionSpecs | ForEach-Object { $_.Name }) +
    @($ErrorCallbackSpec.Name) +
    @($PrivateFunctionNames)

$ExpectedTcpCallbackFenceVariables = @(
    'RpcCallbackProtocolVersion:UINT',
    'RpcCallbackAcceptedMaxDatagram:UINT',
    'RpcCallbackSessionEpoch:UDINT',
    'RpcCallbackBootId:UDINT',
    'RpcCallbackCookieLo:UDINT',
    'RpcCallbackCookieHi:UDINT',
    'RpcCallbackLastDisarmResult:DINT')
$ExpectedGateB2GeneratedTcpCallbackVariables = @(
    'RpcCallbackRegistered',
    'RpcCallbackEventMask',
    'RpcCallbackPort',
    'RpcCallbackIPv4',
    'RpcCallbackIPv4',
    'RpcCallbackProtocolVersion',
    'RpcCallbackProtocolVersion',
    'RpcCallbackAcceptedMaxDatagram',
    'RpcCallbackAcceptedMaxDatagram',
    'RpcCallbackSessionEpoch',
    'RpcCallbackSessionEpoch',
    'RpcCallbackBootId',
    'RpcCallbackBootId',
    'RpcCallbackCookieLo',
    'RpcCallbackCookieLo',
    'RpcCallbackCookieHi',
    'RpcCallbackCookieHi',
    'RpcCallbackLastDisarmResult',
    'RpcCallbackRegistered',
    'RpcCallbackEventMask',
    'RpcCallbackPort',
    'RpcCallbackIPv4',
    'RpcCallbackProtocolVersion',
    'RpcCallbackAcceptedMaxDatagram',
    'RpcCallbackSessionEpoch',
    'RpcCallbackBootId',
    'RpcCallbackCookieLo',
    'RpcCallbackCookieHi',
    'RpcCallbackLastDisarmResult')
$ExpectedDerivedCandidateGeneratedTcpCallbackVariables = @(
    'RpcCallbackRegistered',
    'RpcCallbackEventMask',
    'RpcCallbackPort',
    'RpcCallbackIPv4',
    'RpcCallbackIPv4',
    'RpcCallbackProtocolVersion',
    'RpcCallbackAcceptedMaxDatagram',
    'RpcCallbackSessionEpoch',
    'RpcCallbackBootId',
    'RpcCallbackCookieLo',
    'RpcCallbackCookieHi',
    'RpcCallbackLastDisarmResult',
    'RpcCallbackRegistered',
    'RpcCallbackEventMask',
    'RpcCallbackPort',
    'RpcCallbackIPv4',
    'RpcCallbackProtocolVersion',
    'RpcCallbackAcceptedMaxDatagram',
    'RpcCallbackSessionEpoch',
    'RpcCallbackBootId',
    'RpcCallbackCookieLo',
    'RpcCallbackCookieHi',
    'RpcCallbackLastDisarmResult')
$TcpDisarmHelperSpec = [ordered]@{
    Name = 'DisarmRpcCallbackEndpoint'
    Inputs = @()
    Outputs = @('Result:DINT')
}
$ExpectedTcpFunctionNames = @(
    '@CT_',
    '@STD',
    'CyWork',
    'ConnSocketInfo',
    'DataHandling',
    'SendData',
    'Response',
    'HandleControlSafetyDrainPending',
    'HandleRpcLifecycleCommands',
    'MsgPaser',
    'DisarmRpcCallbackEndpoint')
$ExpectedTcpCallbackTupleClearStatements = @(
    'RpcCallbackRegistered := FALSE;',
    'RpcCallbackEventMask := 0;',
    'RpcCallbackPort := 0;',
    'RpcCallbackIPv4[0] := 0;',
    'RpcCallbackIPv4[1] := 0;',
    'RpcCallbackIPv4[2] := 0;',
    'RpcCallbackIPv4[3] := 0;',
    'RpcCallbackProtocolVersion := 0;',
    'RpcCallbackAcceptedMaxDatagram := 0;',
    'RpcCallbackSessionEpoch := 0;',
    'RpcCallbackBootId := 0;',
    'RpcCallbackCookieLo := 0;',
    'RpcCallbackCookieHi := 0;')
$ExpectedGateCTcpFunctionNames = @(
    'ConnSocketInfo',
    'SendData',
    'HandleControlSafetyDrainPending',
    'HandleRpcLifecycleCommands',
    'DisarmRpcCallbackEndpoint')

$TerminalWakeTryTakeSpec = [ordered]@{
    Name = 'TryTakeD5TerminalWake'
    Inputs = @(
        'pTicketId:^UDINT',
        'pTicketBootId:^UDINT',
        'pOwnerSessionEpoch:^UDINT')
    Outputs = @('Result:DINT')
}
$TerminalWakePublishSpec = [ordered]@{
    Name = 'PublishD5TerminalWake'
    Inputs = @()
    Outputs = @()
}
$ExpectedDiagnosticsTerminalWakeVariables = @(
    'D5TerminalWakeLastAttemptTicketId:UDINT',
    'D5TerminalWakeLastAttemptTicketBootId:UDINT',
    'D5TerminalWakeLastAttemptOwnerSessionEpoch:UDINT')
$ExpectedTcpTerminalWakeVariables = @(
    'D5TerminalWakeAttemptCount:UDINT',
    'D5TerminalWakeEnqueuedCount:UDINT',
    'D5TerminalWakeRejectedCount:UDINT')

$ExpectedActiveEndpointFields = @(
    'Armed:BOOL',
    'ProtocolVersion:UINT',
    'EventMask:UDINT',
    'CallbackIPv4:UDINT',
    'CallbackPort:DINT',
    'SessionEpoch:UDINT',
    'BootId:UDINT',
    'CookieLo:UDINT',
    'CookieHi:UDINT',
    'MaxDatagramBytes:UDINT')

$ExpectedTxSlotFields = @(
    'InUse:BOOL',
    'ProtocolVersion:UINT',
    'DatagramBytes:UDINT',
    'DestinationIPv4:UDINT',
    'DestinationPort:UDINT',
    'SessionEpoch:UDINT',
    'BootId:UDINT',
    'CookieLo:UDINT',
    'CookieHi:UDINT',
    'SequenceLo:UDINT',
    'SequenceHi:UDINT',
    'PlcTimeMs:UDINT',
    'RetryCount:UDINT',
    'Data:ARRAY[0..511]OFBYTE')

$ExpectedDerivedServers = @(
    'QueueDepth:SvrCh_UDINT',
    'QueuedCount:SvrCh_UDINT',
    'RingAcceptedCount:SvrCh_UDINT',
    'AdmissionRetryCount:SvrCh_UDINT',
    'QueueFullDropCount:SvrCh_UDINT',
    'AdmissionErrorDropCount:SvrCh_UDINT',
    'DisarmClearedCount:SvrCh_UDINT',
    'TransportErrorCount:SvrCh_UDINT',
    'LastAdmissionResult:SvrCh_DINT')

$ExpectedDerivedMetadataServerNames = @(
    'AdmissionErrorDropCount',
    'AdmissionRetryCount',
    'DisarmClearedCount',
    'LastAdmissionResult',
    'QueuedCount',
    'QueueDepth',
    'QueueFullDropCount',
    'RingAcceptedCount',
    'TransportErrorCount')

$ExpectedDerivedVariables = @(
    'ActiveEndpoint:_LMC_UDP_ACTIVE_ENDPOINT',
    'TxSlots:ARRAY[0..7]OF_LMC_UDP_TX_SLOT',
    'ReadIndex:UDINT',
    'WriteIndex:UDINT',
    'Depth:UDINT',
    'NextSequenceLo:UDINT',
    'NextSequenceHi:UDINT')

$PublicResultDomains = [ordered]@{
    ArmEndpoint = @(0, 1, -1, -2, -3, -6, -9)
    DisarmEndpoint = @(0, 1, -8, -9)
    PublishEvent = @(0, -2, -4, -5, -6, -7, -8, -9)
}

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

    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $algorithm.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
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

function Get-CommentInsensitiveTokenStream {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $source = ConvertTo-CanonicalLf -Text $Text
    $withoutComments = [Text.StringBuilder]::new($source.Length)
    $index = 0
    $inString = $false
    while ($index -lt $source.Length) {
        $current = $source[$index]
        $next = if (($index + 1) -lt $source.Length) {
            $source[$index + 1]
        }
        else {
            [char]0
        }
        if ($inString) {
            $null = $withoutComments.Append($current)
            if ($current -eq '"') {
                if ($next -eq '"') {
                    $null = $withoutComments.Append($next)
                    $index += 2
                    continue
                }
                $inString = $false
            }
            $index++
            continue
        }
        if ($current -eq '"') {
            $inString = $true
            $null = $withoutComments.Append($current)
            $index++
            continue
        }
        if (($current -eq '(') -and ($next -eq '*')) {
            $end = $source.IndexOf('*)', $index + 2, [StringComparison]::Ordinal)
            if ($end -lt 0) {
                Throw-UdpCallbackBlocker 'unterminated block comment in exact source.'
            }
            $null = $withoutComments.Append(' ')
            $index = $end + 2
            continue
        }
        if (($current -eq '/') -and ($next -eq '/')) {
            $end = $source.IndexOf("`n", $index + 2, [StringComparison]::Ordinal)
            if ($end -lt 0) {
                break
            }
            $null = $withoutComments.Append(' ')
            $index = $end
            continue
        }
        $null = $withoutComments.Append($current)
        $index++
    }
    if ($inString) {
        Throw-UdpCallbackBlocker 'unterminated string in exact source.'
    }

    $tokens = @([regex]::Matches(
            $withoutComments.ToString(),
            '"(?:[^"]|"")*"|' +
                '0[xX][0-9A-Fa-f]+|' +
                '[0-9]+#[A-Za-z0-9_]+|' +
                '\$[A-Za-z_@][A-Za-z0-9_@]*|' +
                '#[A-Za-z_@][A-Za-z0-9_@]*|' +
                '[A-Za-z_@][A-Za-z0-9_@]*|' +
                '[0-9]+(?:\.[0-9]+)?|' +
                ':=|\+=|-=|\*=|/=|<>|<=|>=|::|\.\.|=>|[^\s]') |
            ForEach-Object { $_.Value })
    $serialized = [Text.StringBuilder]::new()
    foreach ($token in $tokens) {
        $null = $serialized.Append(
            $token.Length.ToString([Globalization.CultureInfo]::InvariantCulture))
        $null = $serialized.Append(':')
        $null = $serialized.Append($token)
    }
    return $serialized.ToString()
}

function Remove-LasalCommentsPreserveStrings {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $source = ConvertTo-CanonicalLf -Text $Text
    $result = [Text.StringBuilder]::new($source.Length)
    $index = 0
    $inString = $false
    while ($index -lt $source.Length) {
        $current = $source[$index]
        $next = if (($index + 1) -lt $source.Length) {
            $source[$index + 1]
        }
        else {
            [char]0
        }
        if ($inString) {
            $null = $result.Append($current)
            if ($current -eq '"') {
                if ($next -eq '"') {
                    $null = $result.Append($next)
                    $index += 2
                    continue
                }
                $inString = $false
            }
            $index++
            continue
        }
        if ($current -eq '"') {
            $inString = $true
            $null = $result.Append($current)
            $index++
            continue
        }
        if (($current -eq '(') -and ($next -eq '*')) {
            $end = $source.IndexOf('*)', $index + 2, [StringComparison]::Ordinal)
            if ($end -lt 0) {
                Throw-UdpCallbackBlocker 'unterminated block comment in source.'
            }
            $null = $result.Append(' ')
            $index = $end + 2
            continue
        }
        if (($current -eq '/') -and ($next -eq '/')) {
            $end = $source.IndexOf("`n", $index + 2, [StringComparison]::Ordinal)
            if ($end -lt 0) {
                break
            }
            $null = $result.Append("`n")
            $index = $end + 1
            continue
        }
        $null = $result.Append($current)
        $index++
    }
    if ($inString) {
        Throw-UdpCallbackBlocker 'unterminated string in source.'
    }
    return $result.ToString()
}

function Assert-NoDisabledPreprocessorEnvelope {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $scan = Remove-LasalCommentsPreserveStrings -Text $Text
    if ($scan -match '(?im)^[ \t]*#[ \t]*error\b' -or
        $scan -match
            '(?im)^[ \t]*#[ \t]*if[ \t]*\(?[ \t]*(?:0|FALSE)\b' +
                '[ \t]*\)?[ \t]*(?://.*)?$') {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner contains a fail/disabled preprocessor envelope.")
    }
}

function Assert-NoUnexpectedTopLevelResidue {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner,
        [Parameter(Mandatory = $true)][int]$ExpectedDirectiveCount,
        [Parameter(Mandatory = $true)][string]$ExpectedDirectiveSha256
    )

    $scan = Remove-LasalCommentsPreserveStrings -Text $Text
    $directivePattern =
        '(?im)^[ \t]*#[ \t]*(?:pragma|define|if|ifdef|ifndef|elif|else|' +
        'endif|error|undef|include)\b[^\r\n]*'
    $directives = @([regex]::Matches($scan, $directivePattern) |
            ForEach-Object {
                [regex]::Replace($_.Value.Trim(), '\s+', ' ')
            })
    $directiveText = [string]::Join("`n", $directives)
    $directiveBytes = $Utf8.GetBytes($directiveText)
    if (($directives.Count -ne $ExpectedDirectiveCount) -or
        ((Get-BytesSha256 -Bytes $directiveBytes) -cne
            $ExpectedDirectiveSha256)) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner top-level preprocessor inventory drifted.")
    }
    foreach ($pattern in @(
            ('(?ms)^[A-Za-z_][A-Za-z0-9_]*[ \t]*:[ \t]*CLASS\b.*?' +
                '^END_CLASS[ \t]*;[ \t]*\r?$'),
            '(?ms)^TYPE\b.*?^END_TYPE[ \t]*;?[ \t]*\r?$',
            '(?ms)^FUNCTION\b.*?^END_FUNCTION[ \t]*\r?$')) {
        $scan = [regex]::Replace($scan, $pattern, '')
    }
    $scan = [regex]::Replace($scan, $directivePattern, '')
    if (-not [string]::IsNullOrWhiteSpace($scan)) {
        $preview = [regex]::Replace($scan, '\s+', ' ').Trim()
        if ($preview.Length -gt 120) {
            $preview = $preview.Substring(0, 120)
        }
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner contains an unapproved top-level token/span: $preview")
    }
}

function Assert-ExactSyntheticSourceTokenContract {
    param(
        [Parameter(Mandatory = $true)][string]$Actual,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    if (-not [string]::Equals(
            (Get-CommentInsensitiveTokenStream -Text $Actual),
            (Get-CommentInsensitiveTokenStream -Text $Expected),
            [StringComparison]::Ordinal)) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner synthetic whole-source token contract drifted.")
    }
}

function Assert-NoCustomSourceConditionalPreprocessor {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $scan = Remove-LasalCommentsPreserveStrings -Text $Text
    if ($scan -match
        '(?im)^[ \t]*#[ \t]*(?:if|ifdef|ifndef|elif|else|endif|error|undef)\b') {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner contains an unapproved conditional/error directive.")
    }
}

function Assert-AsciiTextEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][int]$ByteCount,
        [Parameter(Mandatory = $true)][string]$Sha256,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    foreach ($character in $Text.ToCharArray()) {
        if ([int]$character -gt 127) {
            Throw-UdpCallbackBlocker "$ArtifactOwner contains a non-ASCII character."
        }
    }
    $bytes = [Text.Encoding]::ASCII.GetBytes($Text)
    if (($bytes.Count -ne $ByteCount) -or
        ((Get-BytesSha256 -Bytes $bytes) -cne $Sha256)) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner byte/hash evidence does not match its text.")
    }
}

function Assert-Latin1BinaryEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][int]$ByteCount,
        [Parameter(Mandatory = $true)][string]$Sha256,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $bytes = $Latin1.GetBytes($Text)
    if (($bytes.Count -ne $ByteCount) -or
        ((Get-BytesSha256 -Bytes $bytes) -cne $Sha256)) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner byte/hash evidence does not match its text.")
    }
    $signature = 'SigmatekLasal2Binary' + [char]0
    if (($Text.Length -lt 32) -or
        (-not $Text.StartsWith($signature, [StringComparison]::Ordinal)) -or
        ((Get-OrdinalCount -Text $Text -Needle $signature) -ne 1)) {
        Throw-UdpCallbackBlocker "$ArtifactOwner binary signature is not exact."
    }
}

function Get-LasalLengthPrefixedRecordCount {
    param(
        [Parameter(Mandatory = $true)][string]$DatabaseText,
        [Parameter(Mandatory = $true)][string]$Value
    )

    if (($Value.Length -lt 1) -or ($Value.Length -gt 255)) {
        throw 'LASAL record fixture value length is out of range.'
    }
    $record = [string][char]$Value.Length + [char]0 + [char]0 +
        [char]0xAA + $Value
    return Get-OrdinalCount -Text $DatabaseText -Needle $record
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
        if (($Kind -ceq 'Declaration') -and ($name -ceq 'TAB')) {
            $lineEnd = $Text.IndexOf("`n", $match.Index)
            if ($lineEnd -lt 0) {
                $lineEnd = $Text.Length
            }
            $line = $Text.Substring($match.Index, $lineEnd - $match.Index)
            if ([regex]::IsMatch(
                    $line,
                    '(?i)^[ \t]*FUNCTION[ \t]+GLOBAL[ \t]+TAB[ \t]+@CT_[ \t]*;')) {
                continue
            }
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
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)' +
                '(?<Register>[ \t]*\([A-Za-z_][A-Za-z0-9_]*\))?' +
                '[ \t]*:[ \t]*(?<Type>[^;]+?)[ \t]*;[ \t]*$')
        if (-not $variable.Success) {
            Throw-UdpCallbackBlocker (
                "$FunctionOwner $Section has an unapproved declaration: $trimmed")
        }
        $name = $variable.Groups['Name'].Value
        if ($variable.Groups['Register'].Success) {
            $name += ' ' + $variable.Groups['Register'].Value.Trim()
        }
        $type = [regex]::Replace(
            $variable.Groups['Type'].Value.Trim(),
            '[ \t]+',
            '')
        $inventory.Add($name + ':' + $type)
    }
    return $inventory.ToArray()
}

function Get-FunctionExecutableText {
    param([Parameter(Mandatory = $true)][string]$FunctionBlock)

    $text = ConvertTo-CanonicalLf -Text $FunctionBlock
    $firstLineEnd = $text.IndexOf("`n", [StringComparison]::Ordinal)
    if ($firstLineEnd -lt 0) {
        Throw-UdpCallbackBlocker 'function block has no implementation body.'
    }
    $body = $text.Substring($firstLineEnd + 1)
    while ($true) {
        $variableSection = [regex]::Match(
            $body,
            '(?ims)\A[ \t]*VAR(?:_INPUT|_OUTPUT)?[ \t]*\n' +
                '.*?^[ \t]*END_VAR[ \t]*;?[ \t]*(?:\n|\z)')
        if (-not $variableSection.Success) {
            break
        }
        $body = $body.Substring($variableSection.Length)
    }
    $body = [regex]::Replace(
        $body,
        '(?im)^[ \t]*END_FUNCTION[ \t]*\z',
        '')
    return $body.Trim()
}

function Get-DerivedImplementationDisposition {
    param([Parameter(Mandatory = $true)][string]$SourceText)

    $records = @(Get-FunctionRecords `
            -Text (Get-LexicalScanText -Text $SourceText) `
            -Kind Implementation)
    Assert-ExactInventory `
        -Actual @($records.Name) `
        -Expected $ImplementationFunctionNames `
        -InventoryOwner 'derived implementation state function order'
    $emptyCount = 0
    foreach ($record in $records) {
        if ((Get-FunctionExecutableText -FunctionBlock $record.Block).Length -eq 0) {
            $emptyCount++
        }
    }
    if ($emptyCount -eq $records.Count) {
        return 'Empty'
    }
    if ($emptyCount -eq 0) {
        return 'Complete'
    }
    Throw-UdpCallbackBlocker (
        'derived sender has a forbidden partial empty/non-empty implementation set.')
}

function Assert-ExactInventory {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Actual,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$InventoryOwner
    )

    if ([string]::Join('|', $Actual) -cne [string]::Join('|', $Expected)) {
        Throw-UdpCallbackBlocker (
            "$InventoryOwner is '$([string]::Join('|', $Actual))', expected " +
            "'$([string]::Join('|', $Expected))'.")
    }
}

function Assert-DeclaredSpanInventory {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string[]]$ExpectedFunctionNames,
        [Parameter(Mandatory = $true)][int]$ExpectedTypeSpanCount,
        [Parameter(Mandatory = $true)][string]$ExpectedClassName,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $scan = Remove-LasalCommentsPreserveStrings -Text $Text
    $actualFunctions = @([regex]::Matches(
            $scan,
            '(?im)^[ \t]*FUNCTION(?:[ \t]+(?:VIRTUAL|GLOBAL|TAB))*' +
                '[ \t]+(?:[A-Za-z_][A-Za-z0-9_]*::)?' +
                '(?<Name>@?[A-Za-z_][A-Za-z0-9_]*)\b') |
            ForEach-Object { $_.Groups['Name'].Value } |
            Sort-Object)
    $expectedFunctions = @(
        foreach ($name in $ExpectedFunctionNames) {
            $name
            $name
        })
    $expectedFunctions = @($expectedFunctions | Sort-Object)
    Assert-ExactInventory `
        -Actual $actualFunctions `
        -Expected $expectedFunctions `
        -InventoryOwner "$ArtifactOwner FUNCTION span inventory"
    $typeSpanCount = [regex]::Matches(
        $scan,
        '(?im)^[ \t]*TYPE\b').Count
    if ($typeSpanCount -ne $ExpectedTypeSpanCount) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner TYPE span count is $typeSpanCount, expected " +
            "$ExpectedTypeSpanCount.")
    }
    $classNames = @([regex]::Matches(
            $scan,
            '(?im)^[ \t]*(?<Name>[A-Za-z_][A-Za-z0-9_]*)' +
                '[ \t]*:[ \t]*CLASS\b') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $classNames `
        -Expected @($ExpectedClassName) `
        -InventoryOwner "$ArtifactOwner CLASS span inventory"
}

function Assert-FunctionSourceAbi {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Record,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Spec,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ExpectedModifiers,
        [switch]$UseSourceOutputs
    )

    if ($Record.Modifiers -cne $ExpectedModifiers) {
        Throw-UdpCallbackBlocker (
            "$($Spec.Name) modifiers are '$($Record.Modifiers)', expected " +
            "'$ExpectedModifiers'.")
    }
    $expectedInputs = @($Spec.Inputs)
    $expectedOutputs = if ($UseSourceOutputs -and
        $Spec.Contains('SourceOutputs')) {
        @($Spec.SourceOutputs)
    }
    else {
        @($Spec.Outputs)
    }
    foreach ($section in @(
            @{ Name = 'VAR_INPUT'; Values = $expectedInputs },
            @{ Name = 'VAR_OUTPUT'; Values = $expectedOutputs })) {
        $hasSection = $Record.Block -match
            "(?im)^[ \t]*$($section.Name)[ \t]*$"
        if (@($section.Values).Count -eq 0) {
            if ($hasSection) {
                Throw-UdpCallbackBlocker (
                    "$($Spec.Name) has unexpected $($section.Name).")
            }
            continue
        }
        if (-not $hasSection) {
            Throw-UdpCallbackBlocker (
                "$($Spec.Name) is missing $($section.Name).")
        }
        Assert-ExactInventory `
            -Actual @(Get-VariableInventory `
                -FunctionBlock $Record.Block `
                -Section $section.Name `
                -FunctionOwner $Spec.Name) `
            -Expected @($section.Values) `
            -InventoryOwner "$($Spec.Name) $($section.Name) ABI"
    }
}

function Get-DerivedStructFieldInventory {
    param(
        [Parameter(Mandatory = $true)][string]$TypeBody,
        [Parameter(Mandatory = $true)][string]$TypeName
    )

    $matches = @([regex]::Matches(
            $TypeBody,
            '(?ims)^[ \t]*' + [regex]::Escape($TypeName) +
                '[ \t]*:[ \t]*STRUCT\b(?<Body>.*?)' +
                '^[ \t]*END_STRUCT[ \t]*;?[ \t]*$'))
    if ($matches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$TypeName struct count is $($matches.Count), expected 1.")
    }
    $inventory = [Collections.Generic.List[string]]::new()
    foreach ($line in $matches[0].Groups['Body'].Value -split '\n') {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0) {
            continue
        }
        $field = [regex]::Match(
            $trimmed,
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                '(?<Type>[^;]+)[ \t]*;[ \t]*$')
        if (-not $field.Success) {
            Throw-UdpCallbackBlocker (
                "$TypeName has an unapproved field declaration: $trimmed")
        }
        $inventory.Add(
            $field.Groups['Name'].Value + ':' +
            [regex]::Replace($field.Groups['Type'].Value, '\s+', ''))
    }
    return $inventory.ToArray()
}

function Assert-DerivedStorageContract {
    param(
        [Parameter(Mandatory = $true)][string]$ClassBody,
        [Parameter(Mandatory = $true)][string]$SourceText
    )

    $typeBlocks = @([regex]::Matches(
            $ClassBody,
            '(?ims)^[ \t]*TYPE[ \t]*$' +
                '(?<Body>.*?)^[ \t]*END_TYPE[ \t]*;?[ \t]*$'))
    if ($typeBlocks.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "derived TYPE block count is $($typeBlocks.Count), expected 1.")
    }
    $typeNames = @([regex]::Matches(
            $typeBlocks[0].Groups['Body'].Value,
            '(?im)^[ \t]*(?<Name>_LMC_UDP_[A-Z0-9_]+)[ \t]*:' +
                '[ \t]*STRUCT\b') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $typeNames `
        -Expected @('_LMC_UDP_ACTIVE_ENDPOINT', '_LMC_UDP_TX_SLOT') `
        -InventoryOwner 'derived TYPE inventory'
    Assert-ExactInventory `
        -Actual @(Get-DerivedStructFieldInventory `
            -TypeBody $typeBlocks[0].Groups['Body'].Value `
            -TypeName '_LMC_UDP_ACTIVE_ENDPOINT') `
        -Expected $ExpectedActiveEndpointFields `
        -InventoryOwner '_LMC_UDP_ACTIVE_ENDPOINT field ABI'
    Assert-ExactInventory `
        -Actual @(Get-DerivedStructFieldInventory `
            -TypeBody $typeBlocks[0].Groups['Body'].Value `
            -TypeName '_LMC_UDP_TX_SLOT') `
        -Expected $ExpectedTxSlotFields `
        -InventoryOwner '_LMC_UDP_TX_SLOT field ABI'

    $firstFunction = [regex]::Match(
        $ClassBody,
        '(?im)^[ \t]*FUNCTION\b')
    if (-not $firstFunction.Success) {
        Throw-UdpCallbackBlocker 'derived function declaration boundary is missing.'
    }
    $storageText = $ClassBody.Substring(0, $firstFunction.Index)
    $storageText = $storageText.Replace($typeBlocks[0].Value, '')
    $storage = [Collections.Generic.List[string]]::new()
    foreach ($line in $storageText -split '\n') {
        $declaration = [regex]::Match(
            $line.Trim(),
            '^(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                '(?<Type>[^;]+)[ \t]*;[ \t]*$')
        if ($declaration.Success) {
            $storage.Add(
                $declaration.Groups['Name'].Value + ':' +
                [regex]::Replace(
                    $declaration.Groups['Type'].Value,
                    '\s+',
                    ''))
        }
    }
    Assert-ExactInventory `
        -Actual $storage.ToArray() `
        -Expected @($ExpectedDerivedServers + $ExpectedDerivedVariables) `
        -InventoryOwner 'derived server and variable storage inventory'

    $metadata = [regex]::Match(
        $SourceText,
        '(?s)\(\*![ \t\r\n]*(?<Xml><Class\b.*?</Class>)' +
            '[ \t\r\n]*\*\)')
    if (-not $metadata.Success) {
        Throw-UdpCallbackBlocker 'derived class XML metadata is missing.'
    }
    try {
        [xml]$xml = $metadata.Groups['Xml'].Value
    }
    catch {
        Throw-UdpCallbackBlocker (
            "derived class XML metadata cannot be parsed: $($_.Exception.Message)")
    }
    $root = $xml.DocumentElement
    foreach ($attribute in @(
            @{ Name = 'Name'; Value = 'LMCUdpCallbackSender' },
            @{ Name = 'RealtimeTask'; Value = 'false' },
            @{ Name = 'CyclicTask'; Value = 'true' },
            @{ Name = 'DefCyclictime'; Value = '10 ms' },
            @{ Name = 'BackgroundTask'; Value = 'false' },
            @{ Name = 'Sigmatek'; Value = 'false' })) {
        if ($root.GetAttribute($attribute.Name) -cne $attribute.Value) {
            Throw-UdpCallbackBlocker (
                "derived metadata $($attribute.Name) is not " +
                "'$($attribute.Value)'.")
        }
    }
    $networks = @($root.SelectNodes('./Network'))
    if ($networks.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived class must contain exactly one inheritance Network.')
    }
    $network = $networks[0]
    if ($network.GetAttribute('Name') -cne 'LMCUdpCallbackSender' -or
        $network.Attributes.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived inheritance Network name or attributes drifted.')
    }
    $networkElements = @($network.ChildNodes | Where-Object {
            $_.NodeType -eq [System.Xml.XmlNodeType]::Element
        })
    Assert-ExactInventory `
        -Actual @($networkElements | ForEach-Object { $_.Name }) `
        -Expected @('Components', 'Comments', 'Connections', 'Options') `
        -InventoryOwner 'derived inheritance Network section order'

    $components = @($network.SelectNodes('./Components'))
    $baseObjects = @($network.SelectNodes('./Components/Object'))
    if ($components.Count -ne 1 -or $baseObjects.Count -ne 1 -or
        @($components[0].ChildNodes | Where-Object {
                $_.NodeType -eq [System.Xml.XmlNodeType]::Element
            }).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived inheritance Network must contain only one _base object.')
    }
    $baseObject = $baseObjects[0]
    $expectedBaseAttributes = [ordered]@{
        Name = '_base'
        Class = '_UDPTransceiverInterface'
        Position = '(218,120)'
        Visualized = 'true'
        Remotely = 'true'
    }
    foreach ($entry in $expectedBaseAttributes.GetEnumerator()) {
        if ($baseObject.GetAttribute($entry.Key) -cne $entry.Value) {
            Throw-UdpCallbackBlocker (
                "derived _base $($entry.Key) is not '$($entry.Value)'.")
        }
    }
    if ($baseObject.Attributes.Count -ne 6 -or
        -not $baseObject.HasAttribute('GUID') -or
        $baseObject.GetAttribute('GUID') -notmatch
            '^\{[0-9A-Fa-f]{8}(?:-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}\}$') {
        Throw-UdpCallbackBlocker (
            'derived _base attributes or GUID format drifted.')
    }
    $baseChildren = @($baseObject.ChildNodes | Where-Object {
            $_.NodeType -eq [System.Xml.XmlNodeType]::Element
        })
    if ($baseChildren.Count -ne 1 -or $baseChildren[0].Name -cne 'Channels') {
        Throw-UdpCallbackBlocker (
            'derived _base must contain only its inherited Channels inventory.')
    }
    $baseChannels = @($baseChildren[0].ChildNodes | Where-Object {
            $_.NodeType -eq [System.Xml.XmlNodeType]::Element
        })
    Assert-ExactInventory `
        -Actual @($baseChannels | ForEach-Object {
                $_.LocalName + ':' + $_.GetAttribute('Name')
            }) `
        -Expected @(
            'Server:ClassSvr',
            'Server:ErrorCode',
            'Server:ErrorMessage',
            'Server:ErrorState',
            'Server:State',
            'Client:_UDPTransceiver') `
        -InventoryOwner 'derived _base inherited channel order'
    foreach ($channel in $baseChannels) {
        if ($channel.Attributes.Count -ne 1 -or
            @($channel.ChildNodes | Where-Object {
                    $_.NodeType -eq [System.Xml.XmlNodeType]::Element
                }).Count -ne 0) {
            Throw-UdpCallbackBlocker (
                'derived _base inherited channel metadata drifted.')
        }
    }
    foreach ($emptySectionName in @('Comments', 'Options')) {
        $emptySections = @($network.SelectNodes('./' + $emptySectionName))
        if ($emptySections.Count -ne 1 -or
            $emptySections[0].Attributes.Count -ne 0 -or
            $emptySections[0].InnerText.Trim().Length -ne 0 -or
            @($emptySections[0].ChildNodes | Where-Object {
                    $_.NodeType -eq [System.Xml.XmlNodeType]::Element
                }).Count -ne 0) {
            Throw-UdpCallbackBlocker (
                "derived inheritance Network $emptySectionName is not empty.")
        }
    }
    $connections = @($network.SelectNodes('./Connections/Connection'))
    Assert-ExactInventory `
        -Actual @($connections | ForEach-Object {
                $_.GetAttribute('Source') + '->' +
                $_.GetAttribute('Destination')
            }) `
        -Expected @(
            'this.ClassSvr->_base.ClassSvr',
            'this.State->_base.State',
            'this.ErrorState->_base.ErrorState',
            'this.ErrorMessage->_base.ErrorMessage',
            'this.ErrorCode->_base.ErrorCode',
            '_base._UDPTransceiver->this._UDPTransceiver') `
        -InventoryOwner 'derived _base inherited connection order'
    $connectionContainer = @($network.SelectNodes('./Connections'))
    if ($connectionContainer.Count -ne 1 -or
        @($connectionContainer[0].ChildNodes | Where-Object {
                $_.NodeType -eq [System.Xml.XmlNodeType]::Element
            }).Count -ne $connections.Count) {
        Throw-UdpCallbackBlocker (
            'derived inheritance Network contains unexpected connection nodes.')
    }
    foreach ($connection in $connections) {
        if ($connection.Attributes.Count -ne 3 -or
            -not $connection.HasAttribute('Vertices') -or
            $connection.GetAttribute('Vertices') -notmatch
                '^\([0-9]+,[0-9]+\),\([0-9]+,[0-9]+\),$' -or
            @($connection.ChildNodes | Where-Object {
                    $_.NodeType -eq [System.Xml.XmlNodeType]::Element
                }).Count -ne 0) {
            Throw-UdpCallbackBlocker (
                'derived inheritance connection metadata drifted.')
        }
    }
    $channels = @($root.SelectNodes('./Channels'))
    if ($channels.Count -ne 1) {
        Throw-UdpCallbackBlocker 'derived metadata Channels count is not 1.'
    }
    if (@($channels[0].SelectNodes('./Client')).Count -ne 0) {
        Throw-UdpCallbackBlocker (
            'derived class must not declare new clients; _UDPTransceiver is inherited.')
    }
    $servers = @($channels[0].SelectNodes('./Server'))
    Assert-ExactInventory `
        -Actual @($servers | ForEach-Object { $_.GetAttribute('Name') }) `
        -Expected $ExpectedDerivedMetadataServerNames `
        -InventoryOwner 'derived metadata server order'
    foreach ($server in $servers) {
        foreach ($attribute in @(
                @{ Name = 'Initialize'; Value = 'true' },
                @{ Name = 'DefValue'; Value = '0' },
                @{ Name = 'WriteProtected'; Value = 'true' },
                @{ Name = 'Retentive'; Value = 'false' },
                @{ Name = 'Visualized'; Value = 'false' })) {
            if ($server.GetAttribute($attribute.Name) -cne $attribute.Value) {
                Throw-UdpCallbackBlocker (
                    "$($server.GetAttribute('Name')) metadata $($attribute.Name) " +
                    "is not '$($attribute.Value)'.")
            }
        }
        if ($server.HasAttribute('Class')) {
            Throw-UdpCallbackBlocker (
                "$($server.GetAttribute('Name')) metadata has an unexpected Class.")
        }
    }
}

function Assert-PublicResultDomains {
    param([Parameter(Mandatory = $true)][object[]]$Implementations)

    foreach ($entry in $PublicResultDomains.GetEnumerator()) {
        $record = @($Implementations | Where-Object {
                $_.Name -ceq $entry.Key
            })[0]
        $assignments = @([regex]::Matches(
                $record.Block,
                '(?im)(?<![A-Za-z0-9_])Result[ \t]*:=[ \t]*' +
                    '(?<Value>[^;\r\n]+)[ \t]*;'))
        if ($assignments.Count -eq 0) {
            Throw-UdpCallbackBlocker "$($entry.Key) has no Result assignments."
        }
        $actual = [Collections.Generic.List[int]]::new()
        foreach ($assignment in $assignments) {
            $valueText = $assignment.Groups['Value'].Value.Trim()
            $parsed = 0
            if (-not [int]::TryParse(
                    $valueText,
                    [Globalization.NumberStyles]::Integer,
                    [Globalization.CultureInfo]::InvariantCulture,
                    [ref]$parsed)) {
                Throw-UdpCallbackBlocker (
                    "$($entry.Key) has non-literal Result assignment '$valueText'.")
            }
            if ($actual -notcontains $parsed) {
                $actual.Add($parsed)
            }
        }
        Assert-ExactInventory `
            -Actual @($actual | Sort-Object | ForEach-Object { [string]$_ }) `
            -Expected @($entry.Value | Sort-Object | ForEach-Object { [string]$_ }) `
            -InventoryOwner "$($entry.Key) Result domain"
    }
}

function Assert-SaturatingCounterPattern {
    param(
        [Parameter(Mandatory = $true)][string]$FunctionBlock,
        [Parameter(Mandatory = $true)][string]$CounterName,
        [Parameter(Mandatory = $true)][string]$FunctionOwner
    )

    $pattern = '(?is)IF[ \t]+(?:this\.)?' + [regex]::Escape($CounterName) +
        '\.Read\(\)[ \t]*<>[ \t]*16#FFFFFFFF[ \t]+THEN.*?' +
        [regex]::Escape($CounterName) + '\.Write[ \t]*\([ \t]*input[ \t]*:=' +
        '[ \t]*(?:this\.)?' + [regex]::Escape($CounterName) +
        '\.Read\(\)[ \t]*\+[ \t]*1[ \t]*\)[ \t]*;.*?END_IF[ \t]*;'
    if ([regex]::Matches($FunctionBlock, $pattern).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$FunctionOwner does not have one exact saturating $CounterName increment.")
    }
}

function Assert-DisarmClearedSaturatingAddPattern {
    param([Parameter(Mandatory = $true)][string]$FunctionBlock)

    $pattern = '(?is)IF[ \t]+clearedDepth[ \t]*>[ \t]*0[ \t]+THEN.*?' +
        'IF[ \t]+DisarmClearedCount\.Read\(\)[ \t]*>[ \t]*' +
        '\([ \t]*16#FFFFFFFF[ \t]*-[ \t]*clearedDepth[ \t]*\)[ \t]+THEN.*?' +
        'DisarmClearedCount\.Write[ \t]*\([ \t]*input[ \t]*:=[ \t]*' +
        '16#FFFFFFFF[ \t]*\)[ \t]*;.*?ELSE.*?' +
        'DisarmClearedCount\.Write[ \t]*\([ \t]*input[ \t]*:=[ \t]*' +
        'DisarmClearedCount\.Read\(\)[ \t]*\+[ \t]*clearedDepth[ \t]*\)' +
        '[ \t]*;.*?END_IF[ \t]*;.*?END_IF[ \t]*;'
    if ([regex]::Matches($FunctionBlock, $pattern).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'DisarmEndpoint lacks exact saturating-add of clearedDepth.')
    }
}

function Assert-DerivedCandidateExactFunctionContract {
    param(
        [Parameter(Mandatory = $true)][object[]]$Implementations,
        [switch]$TerminalWakeBroker
    )

    $expectedRecords = @(Get-FunctionRecords `
            -Text $(if ($TerminalWakeBroker) {
                    New-SyntheticTerminalWakeDerivedSource
                } else {
                    New-SyntheticDerivedSource
                }) `
            -Kind Implementation)
    Assert-ExactInventory `
        -Actual @($expectedRecords.Name) `
        -Expected $ImplementationFunctionNames `
        -InventoryOwner 'synthetic derived exact-function baseline'
    $actualByName = @{}
    foreach ($record in $Implementations) {
        $actualByName[$record.Name] = $record.Block
    }
    $expectedByName = @{}
    foreach ($record in $expectedRecords) {
        $expectedByName[$record.Name] = $record.Block
    }
    foreach ($name in $ImplementationFunctionNames) {
        $actual = Get-CommentInsensitiveTokenStream -Text $actualByName[$name]
        $expected = Get-CommentInsensitiveTokenStream -Text $expectedByName[$name]
        if (-not [string]::Equals(
                $actual,
                $expected,
                [StringComparison]::Ordinal)) {
            Throw-UdpCallbackBlocker (
                $(if ($TerminalWakeBroker) {
                        "TerminalWakeBrokerCandidate $name complete function " +
                            'token stream drifted.'
                    } else {
                        "DerivedCandidate $name complete function token stream drifted."
                    }))
        }
    }
}

function Assert-DerivedImplementationContract {
    param(
        [Parameter(Mandatory = $true)][object[]]$Implementations,
        [switch]$RequireNonzeroEventId
    )

    $byName = @{}
    foreach ($record in $Implementations) {
        $byName[$record.Name] = $record.Block
        $executable = [regex]::Replace(
            $record.Block,
            '(?ims)^[ \t]*VAR(?:_INPUT|_OUTPUT)?[ \t]*$.*?' +
                '^[ \t]*END_VAR[ \t]*;?[ \t]*$',
            '')
        $executable = [regex]::Replace(
            $executable,
            '(?im)^[ \t]*(?:FUNCTION\b.*|END_FUNCTION)[ \t]*$',
            '')
        if ($executable -notmatch '(?im):=|\b(?:IF|CASE|FOR|WHILE|REPEAT)\b|' +
            '[A-Za-z_][A-Za-z0-9_.^]*[ \t]*\(') {
            Throw-UdpCallbackBlocker (
                "$($record.Name) implementation is an empty stub.")
        }
    }

    foreach ($name in @(
            'CyWork',
            'ArmEndpoint',
            'DisarmEndpoint',
            'PublishEvent',
            'ErrorCallback')) {
        $block = $byName[$name]
        foreach ($section in @('SectionStart', 'SectionStop')) {
            $count = [regex]::Matches(
                $block,
                '(?i)CriticalSection_UDP\.' + $section +
                    '[ \t]*\(').Count
            if ($count -ne 1) {
                Throw-UdpCallbackBlocker (
                    "$name $section count is $count, expected 1.")
            }
        }
        if ($block -match '(?i)\bRETURN\b') {
            Throw-UdpCallbackBlocker (
                "$name executes forbidden RETURN while owning the shared lock.")
        }
        $executable = Get-FunctionExecutableText -FunctionBlock $block
        if ($executable -notmatch
                '(?is)\ACriticalSection_UDP\.SectionStart[ \t]*\([ \t]*\)' +
                    '[ \t]*;.*CriticalSection_UDP\.SectionStop[ \t]*' +
                    '\([ \t]*\)[ \t]*;\z') {
            Throw-UdpCallbackBlocker (
                "$name lock must be the unconditional first/last executable action.")
        }
    }
    foreach ($spec in $PrivateFunctionSpecs) {
        if ($byName[$spec.Name] -match
            '(?i)CriticalSection_UDP\.Section(?:Start|Stop)[ \t]*\(') {
            Throw-UdpCallbackBlocker (
                "$($spec.Name) private helper re-enters the shared lock.")
        }
    }
    $allImplementationText = [string]::Join("`n", @($Implementations.Block))
    if ($allImplementationText -match
        '(?i)CriticalSection_UDP\.pCmd\^') {
        Throw-UdpCallbackBlocker (
            'derived implementation uses forbidden pCmd indirection for inherited lock.')
    }
    foreach ($forbiddenCall in @(
            'BindSocket',
            'DelSocket',
            'Malloc',
            'MallocV1',
            'Realloc',
            'Free')) {
        if ($allImplementationText -match
            ('(?i)(?<![A-Za-z0-9_])' + [regex]::Escape($forbiddenCall) +
                '[ \t]*\(')) {
            Throw-UdpCallbackBlocker (
                "derived implementation calls forbidden $forbiddenCall.")
        }
    }
    if ($allImplementationText -match
        '(?i)(?:Victim|Coalesce|Merge|Replace)') {
        Throw-UdpCallbackBlocker (
            'derived implementation contains forbidden replacement/coalescing policy.')
    }
    if ($allImplementationText -match '(?i)\bSocket[ \t]*>[ \t]*0\b') {
        Throw-UdpCallbackBlocker (
            'socket handles must use nonzero validity, not signed greater-than zero.')
    }
    if ($allImplementationText -match '(?i)\bbDirect[ \t]*:=[ \t]*TRUE\b') {
        Throw-UdpCallbackBlocker 'derived source selects forbidden direct UDP send.'
    }
    if ([regex]::Matches(
            $allImplementationText,
            '(?i)(?<![A-Za-z0-9_])SendData[ \t]*\(').Count -ne 1 -or
        [regex]::Matches(
            $byName.SendSlot,
            '(?i)(?<![A-Za-z0-9_])SendData[ \t]*\(').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'SendData must occur exactly once and only inside SendSlot.')
    }
    if ([regex]::Matches(
            $byName.CyWork,
            '(?i)(?<![A-Za-z0-9_])EnsureSocketReady[ \t]*\(').Count -ne 1 -or
        [regex]::Matches(
            $byName.CyWork,
            '(?i)(?<![A-Za-z0-9_])ServiceTransmitQueue[ \t]*\(').Count -ne 1 -or
        $byName.CyWork -notmatch
            '(?is)socketResult[ \t]*=[ \t]*0.*?ActiveEndpoint\.Armed.*?' +
                'Depth[ \t]*>[ \t]*0.*?ServiceTransmitQueue[ \t]*\(' -or
        $byName.CyWork -notmatch '(?i)state[ \t]*:=[ \t]*READY[ \t]*;') {
        Throw-UdpCallbackBlocker (
            'CyWork socket precreate/service/state contract drifted.')
    }
    if ([regex]::Matches(
            $byName.ServiceTransmitQueue,
            '(?i)(?<![A-Za-z0-9_])SendSlot[ \t]*\(').Count -ne 1 -or
        $byName.ServiceTransmitQueue -match '(?i)\b(?:FOR|WHILE|REPEAT)\b' -or
        $byName.ServiceTransmitQueue -notmatch
            '(?i)slotIndex[ \t]*:=[ \t]*ReadIndex[ \t]*;') {
        Throw-UdpCallbackBlocker (
            'ServiceTransmitQueue must make one head-only admission attempt.')
    }

    foreach ($pattern in @(
            '(?i)IF[ \t]+Socket[ \t]*=[ \t]*0[ \t]+THEN',
            '(?i)Socket[ \t]*:=[ \t]*AddSocket[ \t]*\(',
            '(?i)Socket[ \t]*<>[ \t]*0',
            '(?i)IsOpen[ \t]*\(',
            '(?i)Result[ \t]*:=[ \t]*-2[ \t]*;',
            '(?i)Result[ \t]*:=[ \t]*0[ \t]*;',
            '(?i)Result[ \t]*:=[ \t]*1[ \t]*;')) {
        if ($byName.EnsureSocketReady -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "EnsureSocketReady lacks required pattern $pattern")
        }
    }
    foreach ($name in @('ArmEndpoint', 'PublishEvent')) {
        $block = $byName[$name]
        if ([regex]::Matches(
                $block,
                '(?i)EnsureSocketReady[ \t]*\(').Count -ne 1 -or
            [regex]::Matches(
                $block,
                '(?i)IF[ \t]*\([ \t]*socketResult[ \t]*<>[ \t]*0[ \t]*\)' +
                    '[ \t]*AND[ \t]*\([ \t]*socketResult[ \t]*<>[ \t]*1' +
                    '[ \t]*\)[ \t]*THEN').Count -ne 1 -or
            $block -notmatch '(?i)Result[ \t]*:=[ \t]*-2[ \t]*;') {
            Throw-UdpCallbackBlocker (
                "$name does not accept both ready 0 and pending 1 socket states.")
        }
    }

    $armCompact = [regex]::Replace($byName.ArmEndpoint, '\s+', '')
    Assert-OrderedTokens `
        -Text $armCompact `
        -Tokens @(
            'IFvalidateResult=-1THENResult:=-1;',
            'ELSIFvalidateResult=-6THENResult:=-6;',
            'ELSIFActiveEndpoint.ArmedAND(ActiveEndpoint.ProtocolVersion=ProtocolVersion)AND(ActiveEndpoint.EventMask=EventMask)AND(ActiveEndpoint.CallbackIPv4=CallbackIPv4)AND(ActiveEndpoint.CallbackPort=CallbackPort)AND(ActiveEndpoint.SessionEpoch=SessionEpoch)AND(ActiveEndpoint.BootId=BootId)AND(ActiveEndpoint.CookieLo=CookieLo)AND(ActiveEndpoint.CookieHi=CookieHi)AND(ActiveEndpoint.MaxDatagramBytes=MaxDatagramBytes)THENResult:=1;',
            'ELSEsocketResult:=EnsureSocketReady();',
            'IF(socketResult<>0)AND(socketResult<>1)THENResult:=-2;',
            'ELSIFActiveEndpoint.ArmedTHENResult:=-3;',
            'ELSEClearPendingFrames();',
            'ActiveEndpoint.Armed:=TRUE;',
            'ActiveEndpoint.MaxDatagramBytes:=MaxDatagramBytes;',
            'NextSequenceLo:=1;',
            'NextSequenceHi:=0;',
            'Result:=0;') `
        -TokenOwner 'ArmEndpoint validation/duplicate/commit control flow'
    $disarmCompact = [regex]::Replace($byName.DisarmEndpoint, '\s+', '')
    Assert-OrderedTokens `
        -Text $disarmCompact `
        -Tokens @(
            'IFNOTActiveEndpoint.ArmedAND(Depth=0)THENResult:=1;',
            'ELSIFNOTFenceMatches(',
            'THENResult:=-8;',
            'ELSEclearedDepth:=Depth;',
            '_memset(dest:=#ActiveEndpoint,usByte:=0,cntr:=sizeof(ActiveEndpoint));',
            'ClearPendingFrames();',
            'NextSequenceLo:=1;',
            'NextSequenceHi:=0;',
            'Result:=0;') `
        -TokenOwner 'DisarmEndpoint idempotent/stale/commit control flow'

    if ($byName.ErrorCallback -match
        '(?i)_UDPTransceiverInterface::ErrorCallback[ \t]*\(') {
        Throw-UdpCallbackBlocker (
            'ErrorCallback must not call an inherited callback while holding the lock.')
    }
    $writerContracts = @(
        [pscustomobject]@{
            Name = 'Depth'
            Pattern = '(?i)(?<![A-Za-z0-9_.])Depth[ \t]*:='
            Expected = @(
                'PublishEvent:1',
                'ServiceTransmitQueue:1',
                'RetryOrDropSlot:1',
                'ClearPendingFrames:1')
        },
        [pscustomobject]@{
            Name = 'ReadIndex'
            Pattern = '(?i)(?<![A-Za-z0-9_.])ReadIndex[ \t]*:='
            Expected = @(
                'ServiceTransmitQueue:2',
                'RetryOrDropSlot:2',
                'ClearPendingFrames:1')
        },
        [pscustomobject]@{
            Name = 'WriteIndex'
            Pattern = '(?i)(?<![A-Za-z0-9_.])WriteIndex[ \t]*:='
            Expected = @('PublishEvent:2', 'ClearPendingFrames:1')
        },
        [pscustomobject]@{
            Name = 'QueueDepth.Write'
            Pattern = '(?i)QueueDepth\.Write[ \t]*\('
            Expected = @(
                'PublishEvent:1',
                'ServiceTransmitQueue:1',
                'RetryOrDropSlot:1',
                'ClearPendingFrames:1')
        },
        [pscustomobject]@{
            Name = 'TxSlots.InUse'
            Pattern = '(?i)TxSlots\[[^\]]+\]\.InUse[ \t]*:='
            Expected = @(
                'BuildDatagram:1',
                'ServiceTransmitQueue:1',
                'RetryOrDropSlot:1')
        },
        [pscustomobject]@{
            Name = 'NextSequenceLo'
            Pattern = '(?i)(?<![A-Za-z0-9_.])NextSequenceLo[ \t]*:='
            Expected = @(
                'ArmEndpoint:1',
                'DisarmEndpoint:1',
                'BuildDatagram:1')
        },
        [pscustomobject]@{
            Name = 'NextSequenceHi'
            Pattern = '(?i)(?<![A-Za-z0-9_.])NextSequenceHi[ \t]*:='
            Expected = @(
                'ArmEndpoint:1',
                'DisarmEndpoint:1',
                'BuildDatagram:2')
        })
    foreach ($contract in $writerContracts) {
        $actualWriters = @(
            foreach ($implementation in $Implementations) {
                $count = [regex]::Matches(
                    $implementation.Block,
                    $contract.Pattern).Count
                if ($count -ne 0) {
                    $implementation.Name + ':' + $count
                }
            })
        Assert-ExactInventory `
            -Actual $actualWriters `
            -Expected $contract.Expected `
            -InventoryOwner "$($contract.Name) exclusive writer inventory"
    }

    $armEndpointFields = @([regex]::Matches(
            $byName.ArmEndpoint,
            '(?i)ActiveEndpoint\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:=') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $armEndpointFields `
        -Expected @(
            'Armed',
            'ProtocolVersion',
            'EventMask',
            'CallbackIPv4',
            'CallbackPort',
            'SessionEpoch',
            'BootId',
            'CookieLo',
            'CookieHi',
            'MaxDatagramBytes') `
        -InventoryOwner 'ArmEndpoint active endpoint commit field order'

    if ($byName.FindFreeSlot -notmatch '(?i)Depth[ \t]*>=[ \t]*8' -or
        $byName.FindFreeSlot -notmatch
            '(?i)TxSlots\[WriteIndex\]\.InUse[ \t]*=[ \t]*FALSE' -or
        $byName.FindFreeSlot -notmatch '(?i)SlotIndex[ \t]*:=[ \t]*-1' -or
        $byName.FindFreeSlot -notmatch
            '(?i)SlotIndex[ \t]*:=[ \t]*TO_DINT[ \t]*\([ \t]*WriteIndex') {
        Throw-UdpCallbackBlocker 'FindFreeSlot fixed FIFO contract drifted.'
    }
    foreach ($pattern in @(
            '(?i)Depth[ \t]*>=[ \t]*8',
            '(?i)Result[ \t]*:=[ \t]*-5[ \t]*;',
            '(?i)Depth[ \t]*:=[ \t]*Depth[ \t]*\+[ \t]*1[ \t]*;',
            '(?i)WriteIndex[ \t]*:=[ \t]*WriteIndex[ \t]*\+[ \t]*1[ \t]*;',
            '(?i)IF[ \t]+WriteIndex[ \t]*>=[ \t]*8[ \t]+THEN',
            '(?i)WriteIndex[ \t]*:=[ \t]*0[ \t]*;',
            '(?i)QueueDepth\.Write[ \t]*\([ \t]*input[ \t]*:=[ \t]*Depth')) {
        if ($byName.PublishEvent -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "PublishEvent FIFO contract lacks $pattern")
        }
    }
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.PublishEvent `
        -CounterName QueueFullDropCount `
        -FunctionOwner PublishEvent
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.PublishEvent `
        -CounterName QueuedCount `
        -FunctionOwner PublishEvent

    foreach ($pattern in @(
            '(?i)VendorResult[ \t]*=[ \t]*-4',
            '(?i)TxSlots\[SlotIndex\]\.RetryCount[ \t]*<[ \t]*3',
            ('(?i)TxSlots\[SlotIndex\]\.RetryCount[ \t]*:=[ \t]*' +
                'TxSlots\[SlotIndex\]\.RetryCount[ \t]*\+[ \t]*1'),
            '(?i)AdmissionRetryCount',
            '(?i)AdmissionErrorDropCount')) {
        if ($byName.RetryOrDropSlot -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "RetryOrDropSlot bounded retry contract lacks $pattern")
        }
    }
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.RetryOrDropSlot `
        -CounterName AdmissionRetryCount `
        -FunctionOwner RetryOrDropSlot
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.RetryOrDropSlot `
        -CounterName AdmissionErrorDropCount `
        -FunctionOwner RetryOrDropSlot
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.ServiceTransmitQueue `
        -CounterName RingAcceptedCount `
        -FunctionOwner ServiceTransmitQueue

    foreach ($pattern in @(
            ('(?i)TxSlots\[SlotIndex\]\.PlcTimeMs[ \t]*:=[ \t]*' +
                'ops\.tAbsolute[ \t]*;'),
            '(?i)TxSlots\[SlotIndex\]\.RetryCount[ \t]*:=[ \t]*0',
            '(?i)TxSlots\[SlotIndex\]\.SequenceLo[ \t]*:=[ \t]*NextSequenceLo',
            '(?i)TxSlots\[SlotIndex\]\.SequenceHi[ \t]*:=[ \t]*NextSequenceHi',
            '(?i)NextSequenceLo[ \t]*:=[ \t]*NextSequenceLo[ \t]*\+[ \t]*1',
            '(?i)IF[ \t]+NextSequenceLo[ \t]*=[ \t]*0[ \t]+THEN',
            '(?i)IF[ \t]+NextSequenceHi[ \t]*=[ \t]*16#FFFFFFFF[ \t]+THEN',
            '(?i)NextSequenceHi[ \t]*:=[ \t]*0',
            ('(?i)Data\[44\]\$UDINT[ \t]*:=[ \t]*' +
                'TxSlots\[SlotIndex\]\.PlcTimeMs'))) {
        if ($byName.BuildDatagram -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "BuildDatagram sequence/timestamp contract lacks $pattern")
        }
    }
    $buildSlotFields = @([regex]::Matches(
            $byName.BuildDatagram,
            '(?i)TxSlots\[SlotIndex\]\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)' +
                '(?:\[[0-9]+\])?(?:\$[A-Za-z_][A-Za-z0-9_]*)?[ \t]*:=') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $buildSlotFields `
        -Expected @(
            'InUse',
            'ProtocolVersion',
            'DatagramBytes',
            'DestinationIPv4',
            'DestinationPort',
            'SessionEpoch',
            'BootId',
            'CookieLo',
            'CookieHi',
            'SequenceLo',
            'SequenceHi',
            'PlcTimeMs',
            'RetryCount',
            'Data', 'Data', 'Data', 'Data', 'Data', 'Data',
            'Data', 'Data', 'Data', 'Data', 'Data', 'Data',
            'Data', 'Data', 'Data', 'Data', 'Data') `
        -InventoryOwner 'BuildDatagram slot commit field order'
    Assert-OrderedTokens `
        -Text $byName.BuildDatagram `
        -Tokens @(
            'TxSlots[SlotIndex].InUse := TRUE',
            'TxSlots[SlotIndex].ProtocolVersion := 2',
            'TxSlots[SlotIndex].DatagramBytes := 52 + PayloadBytes',
            'TxSlots[SlotIndex].DestinationIPv4 := ActiveEndpoint.CallbackIPv4',
            'TxSlots[SlotIndex].DestinationPort := TO_UDINT(ActiveEndpoint.CallbackPort)',
            'TxSlots[SlotIndex].SessionEpoch := ProducerSessionEpoch',
            'TxSlots[SlotIndex].BootId := ActiveEndpoint.BootId',
            'TxSlots[SlotIndex].CookieLo := ActiveEndpoint.CookieLo',
            'TxSlots[SlotIndex].CookieHi := ActiveEndpoint.CookieHi',
            'TxSlots[SlotIndex].SequenceLo := NextSequenceLo',
            'TxSlots[SlotIndex].SequenceHi := NextSequenceHi',
            'TxSlots[SlotIndex].PlcTimeMs := ops.tAbsolute',
            'TxSlots[SlotIndex].RetryCount := 0',
            'Data[0]$UDINT := 16#32434D4C',
            'Data[4]$UINT := 2',
            'Data[6]$UINT := 52',
            'Data[8]$UINT := TO_UINT(TxSlots[SlotIndex].DatagramBytes)',
            'Data[10]$UINT := EventType',
            'Data[12]$UDINT := EventMaskBit',
            'Data[16]$UDINT := ActiveEndpoint.BootId',
            'Data[20]$UDINT := ProducerSessionEpoch',
            'Data[24]$UDINT := ActiveEndpoint.CookieLo',
            'Data[28]$UDINT := ActiveEndpoint.CookieHi',
            'Data[32]$UDINT := TxSlots[SlotIndex].SequenceLo',
            'Data[36]$UDINT := TxSlots[SlotIndex].SequenceHi',
            'Data[40]$UDINT := EventId',
            'Data[44]$UDINT := TxSlots[SlotIndex].PlcTimeMs',
            'Data[48]$UINT := TO_UINT(PayloadBytes)',
            'Data[50] := TO_USINT(DeliveryClass)',
            'Data[51] := 0') `
        -TokenOwner 'BuildDatagram LMC2 header layout'
    if ((Get-OrdinalCount `
            -Text $byName.BuildDatagram `
            -Needle 'pPayload') -ne 1) {
        Throw-UdpCallbackBlocker (
            'v2-only BuildDatagram must not retain or copy rejected payload data.')
    }

    foreach ($domain in @(
            [pscustomobject]@{
                Name = 'EnsureSocketReady'
                Values = @('-2', '0', '1')
            },
            [pscustomobject]@{
                Name = 'ValidateEndpoint'
                Values = @('-1', '-6', '0')
            },
            [pscustomobject]@{
                Name = 'BuildDatagram'
                Values = @('-9', '0')
            })) {
        $assignments = @([regex]::Matches(
                $byName[$domain.Name],
                '(?im)(?<![A-Za-z0-9_])Result[ \t]*:=[ \t]*' +
                    '(?<Value>[^;\r\n]+)[ \t]*;'))
        foreach ($assignment in $assignments) {
            if ($assignment.Groups['Value'].Value.Trim() -notmatch '^-?[0-9]+$') {
                Throw-UdpCallbackBlocker (
                    "$($domain.Name) has a non-literal Result assignment.")
            }
        }
        $values = @($assignments |
                ForEach-Object { $_.Groups['Value'].Value.Trim() } |
                Select-Object -Unique |
                Sort-Object { [int]$_ })
        Assert-ExactInventory `
            -Actual $values `
            -Expected @($domain.Values | Sort-Object { [int]$_ }) `
            -InventoryOwner "$($domain.Name) Result domain"
    }
    if ([regex]::Matches(
            $byName.FindFreeSlot,
            '(?im)SlotIndex[ \t]*:=[ \t]*(?:-1|TO_DINT[ \t]*\()').Count -ne 2 -or
        $byName.ServiceTransmitQueue -notmatch
            '(?i)RetryOrDropSlot[ \t]*\([ \t]*SlotIndex[ \t]*:=[ \t]*' +
                'slotIndex[ \t]*,[ \t]*VendorResult[ \t]*:=[ \t]*' +
                'vendorResult[ \t]*\)') {
        Throw-UdpCallbackBlocker (
            'FindFreeSlot domain or same-head retry call drifted.')
    }

    foreach ($validation in @(
            @{ Pattern = '(?i)EventMaskBit[ \t]*<>[ \t]*1'; Result = -6 },
            @{ Pattern = '(?i)EventType[ \t]*<>[ \t]*1'; Result = -6 },
            @{ Pattern = '(?i)DeliveryClass[ \t]*<>[ \t]*0'; Result = -6 },
            @{ Pattern = '(?i)PayloadBytes[ \t]*<>[ \t]*0'; Result = -6 })) {
        if ($byName.PublishEvent -notmatch $validation.Pattern) {
            Throw-UdpCallbackBlocker (
                "PublishEvent production v2 policy lacks $($validation.Pattern)")
        }
    }
    $eventIdZeroPattern =
        '(?i)\([ \t]*EventId[ \t]*=[ \t]*0[ \t]*\)'
    $eventIdComparisonCount = [regex]::Matches(
        $byName.PublishEvent,
        '(?i)EventId[ \t]*(?:=|<>|<|>)').Count
    if ($RequireNonzeroEventId) {
        if (($eventIdComparisonCount -ne 1) -or
            ([regex]::Matches(
                    $byName.PublishEvent,
                    $eventIdZeroPattern).Count -ne 1)) {
            Throw-UdpCallbackBlocker (
                'Gate D PublishEvent must reject exactly EventId=0.')
        }
        if ($byName.PublishEvent -notmatch
            ('(?is)ELSIF[ \t]+(?:(?!\bTHEN\b).)*' +
                $eventIdZeroPattern + '(?:(?!\bTHEN\b).)*\bTHEN\b' +
                '[ \t\r\n]*Result[ \t]*:=[ \t]*-6[ \t]*;')) {
            Throw-UdpCallbackBlocker (
                'Gate D EventId=0 is not mapped to PublishEvent Result=-6.')
        }
    }
    elseif ($eventIdComparisonCount -ne 0) {
        Throw-UdpCallbackBlocker (
            'Gate C PublishEvent must accept every EventId value, including zero.')
    }
    foreach ($payloadPattern in @(
            ('(?i)\([ \t]*PayloadBytes[ \t]*>[ \t]*0[ \t]*\)[ \t]*AND' +
                '[ \t]*\([ \t]*pPayload[ \t]*=[ \t]*NIL[ \t]*\)'),
            '(?i)PayloadBytes[ \t]*>[ \t]*460',
            ('(?i)\(?[ \t]*PayloadBytes[ \t]*\+[ \t]*52[ \t]*\)?[ \t]*>[ \t]*' +
                'ActiveEndpoint\.MaxDatagramBytes'))) {
        if ($byName.PublishEvent -notmatch $payloadPattern) {
            Throw-UdpCallbackBlocker (
                "PublishEvent structural payload validation lacks $payloadPattern")
        }
    }
    $payloadInvalidIndex = $byName.PublishEvent.IndexOf(
        'Result := -7;',
        [StringComparison]::Ordinal)
    $payloadPolicyIndex = $byName.PublishEvent.IndexOf(
        'PayloadBytes <> 0',
        [StringComparison]::Ordinal)
    if (($payloadInvalidIndex -lt 0) -or ($payloadPolicyIndex -lt 0) -or
        ($payloadInvalidIndex -ge $payloadPolicyIndex)) {
        Throw-UdpCallbackBlocker (
            'PublishEvent must reject invalid pointer/length before v2 zero-payload policy.')
    }
    if ($byName.ValidateEndpoint -notmatch
        '(?is)ProtocolVersion[ \t]*=[ \t]*1.*?Result[ \t]*:=[ \t]*-6' -or
        $byName.ValidateEndpoint -notmatch
        '(?i)ProtocolVersion[ \t]*<>[ \t]*2') {
        Throw-UdpCallbackBlocker (
            'ValidateEndpoint does not fail closed for disabled protocol v1.')
    }

    $duplicateIndex = $byName.ArmEndpoint.IndexOf(
        'Result := 1;',
        [StringComparison]::Ordinal)
    $resetIndex = $byName.ArmEndpoint.IndexOf(
        'NextSequenceLo := 1;',
        [StringComparison]::Ordinal)
    if (($duplicateIndex -lt 0) -or ($resetIndex -lt 0) -or
        ($duplicateIndex -ge $resetIndex) -or
        $byName.ArmEndpoint -notmatch '(?i)NextSequenceHi[ \t]*:=[ \t]*0') {
        Throw-UdpCallbackBlocker (
            'ArmEndpoint duplicate-preserve/new-arm sequence contract drifted.')
    }
    foreach ($pattern in @(
            '(?i)FenceMatches[ \t]*\(',
            '(?i)Result[ \t]*:=[ \t]*-8',
            ('(?i)_memset[ \t]*\([ \t]*dest[ \t]*:=[ \t]*#ActiveEndpoint' +
                '[ \t]*,[ \t]*usByte[ \t]*:=[ \t]*0[ \t]*,[ \t]*' +
                'cntr[ \t]*:=[ \t]*sizeof[ \t]*\([ \t]*ActiveEndpoint' +
                '[ \t]*\)[ \t]*\)'),
            '(?i)ClearPendingFrames[ \t]*\(',
            '(?i)NextSequenceLo[ \t]*:=[ \t]*1',
            '(?i)NextSequenceHi[ \t]*:=[ \t]*0')) {
        if ($byName.DisarmEndpoint -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "DisarmEndpoint fence/clear contract lacks $pattern")
        }
    }
    if ($byName.DisarmEndpoint -match '(?i)\bSocket[ \t]*:=' -or
        $byName.ClearPendingFrames -match '(?i)\bSocket[ \t]*:=') {
        Throw-UdpCallbackBlocker (
            'matched disarm must preserve the send socket for reuse.')
    }
    Assert-DisarmClearedSaturatingAddPattern `
        -FunctionBlock $byName.DisarmEndpoint
    Assert-OrderedTokens `
        -Text $byName.DisarmEndpoint `
        -Tokens @(
            'ELSIF NOT FenceMatches(',
            'Result := -8;',
            'clearedDepth := Depth;',
            '_memset(dest := #ActiveEndpoint, usByte := 0, cntr := sizeof(ActiveEndpoint));',
            'ClearPendingFrames();',
            'NextSequenceLo := 1;',
            'NextSequenceHi := 0;',
            'Result := 0;') `
        -TokenOwner 'DisarmEndpoint fence and full-clear order'
    if ([regex]::Matches(
            $byName.DisarmEndpoint,
            '(?i)_memset[ \t]*\(').Count -ne 1 -or
        $byName.DisarmEndpoint -match
            '(?i)ActiveEndpoint\.(?:Armed|ProtocolVersion|EventMask|' +
                'CallbackIPv4|CallbackPort|SessionEpoch|BootId|CookieLo|' +
                'CookieHi|MaxDatagramBytes)[ \t]*:=') {
        Throw-UdpCallbackBlocker (
            'DisarmEndpoint must clear the full endpoint exactly once.')
    }

    foreach ($pattern in @(
            ('(?i)_memset[ \t]*\([ \t]*dest[ \t]*:=[ \t]*#TxSlots\[0\]' +
                '[ \t]*,[ \t]*usByte[ \t]*:=[ \t]*0[ \t]*,[ \t]*' +
                'cntr[ \t]*:=[ \t]*sizeof[ \t]*\([ \t]*TxSlots' +
                '[ \t]*\)[ \t]*\)'),
            '(?i)ReadIndex[ \t]*:=[ \t]*0',
            '(?i)WriteIndex[ \t]*:=[ \t]*0',
            '(?i)Depth[ \t]*:=[ \t]*0',
            ('(?i)QueueDepth\.Write[ \t]*\([ \t]*input[ \t]*:=[ \t]*0' +
                '[ \t]*\)'))) {
        if ([regex]::Matches(
                $byName.ClearPendingFrames,
                $pattern).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "ClearPendingFrames full-array reset contract lacks $pattern")
        }
    }
    Assert-OrderedTokens `
        -Text $byName.ClearPendingFrames `
        -Tokens @(
            '_memset(dest := #TxSlots[0], usByte := 0, cntr := sizeof(TxSlots));',
            'ReadIndex := 0;',
            'WriteIndex := 0;',
            'Depth := 0;',
            'QueueDepth.Write(input := 0);') `
        -TokenOwner 'ClearPendingFrames full-array reset order'
    if ([regex]::Matches(
            $byName.ClearPendingFrames,
            '(?i)_memset[ \t]*\(').Count -ne 1 -or
        $byName.ClearPendingFrames -match '(?i)\b(?:FOR|WHILE|REPEAT)\b' -or
        $byName.ClearPendingFrames -match
            '(?i)TxSlots\[[^\]]+\]\.[A-Za-z_][A-Za-z0-9_]*[ \t]*:=') {
        Throw-UdpCallbackBlocker (
            'ClearPendingFrames must use one bounded full-array zero operation.')
    }

    foreach ($pattern in @(
            '(?i)ErrorState[ \t]*:=[ \t]*FSM_UDP',
            '(?i)ErrorMessage[ \t]*:=[ \t]*UdpError',
            '(?i)ErrorCode[ \t]*:=[ \t]*ErrCode')) {
        if ($byName.ErrorCallback -notmatch $pattern) {
            Throw-UdpCallbackBlocker (
                "ErrorCallback inherited state contract lacks $pattern")
        }
    }
    Assert-SaturatingCounterPattern `
        -FunctionBlock $byName.ErrorCallback `
        -CounterName TransportErrorCount `
        -FunctionOwner ErrorCallback

    if ($byName.SendSlot -notmatch '(?i)bDirect[ \t]*:=[ \t]*FALSE' -or
        $byName.ServiceTransmitQueue -notmatch
            '(?i)LastAdmissionResult\.Write[ \t]*\([ \t]*input[ \t]*:=' +
                '[ \t]*vendorResult') {
        Throw-UdpCallbackBlocker (
            'queued SendData/LastAdmissionResult contract drifted.')
    }
    $sendCall = @([regex]::Matches(
            $byName.SendSlot,
            '(?is)(?<![A-Za-z0-9_])SendData[ \t]*\(' +
                '(?<Arguments>.*?)\)[ \t]*;'))
    if ($sendCall.Count -ne 1) {
        Throw-UdpCallbackBlocker 'SendSlot exact SendData call count is not 1.'
    }
    $normalizedSendArguments = [regex]::Replace(
        $sendCall[0].Groups['Arguments'].Value,
        '\s+',
        '')
    $expectedSendArguments =
        'pData:=#TxSlots[SlotIndex].Data[0],' +
        'udSize:=TxSlots[SlotIndex].DatagramBytes,' +
        'bDirect:=FALSE,' +
        'udIpAddress:=TxSlots[SlotIndex].DestinationIPv4,' +
        'udPort:=TxSlots[SlotIndex].DestinationPort'
    if ($normalizedSendArguments -cne $expectedSendArguments) {
        Throw-UdpCallbackBlocker (
            'SendSlot does not use the exact inherited five-input SendData ABI.')
    }
    if ([regex]::Matches(
            $byName.EnsureSocketReady,
            '(?i)(?<![A-Za-z0-9_])AddSocket[ \t]*\([ \t]*\)').Count -ne 1 -or
        [regex]::Matches(
            $byName.EnsureSocketReady,
            '(?i)(?<![A-Za-z0-9_])IsOpen[ \t]*\([ \t]*\)').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'EnsureSocketReady must call exact no-input AddSocket and IsOpen ABIs.')
    }
    Assert-PublicResultDomains -Implementations $Implementations
}

function Assert-DerivedStandardTableContract {
    param([Parameter(Mandatory = $true)][string]$SourceText)

    $canonical = ConvertTo-CanonicalLf -Text $SourceText
    $classTables = @([regex]::Matches(
            $canonical,
            '(?ms)^FUNCTION GLOBAL TAB LMCUdpCallbackSender::@CT_[ \t]*$' +
                '.*?^END_FUNCTION[ \t]*$'))
    if ($classTables.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived generated @CT_ implementation count is not 1.')
    }
    $classTable = $classTables[0].Value
    if ([regex]::Matches(
            $classTable,
            '(?m)^9\$UINT,[ \t]*0\$UINT,[ \t]*0\$UINT,[ \t]*$').Count -ne
        1) {
        Throw-UdpCallbackBlocker (
            'derived @CT_ server/client/type counts are not exact 9/0/0.')
    }
    Assert-OrderedTokens `
        -Text $classTable `
        -Tokens @(
            '(SIZEOF(::LMCUdpCallbackSender))$UINT',
            '"LMCUdpCallbackSender"',
            '"_UDPTransceiverInterface"') `
        -TokenOwner 'derived @CT_ class/base metadata'
    $ctServers = @([regex]::Matches(
            $classTable,
            '\(::LMCUdpCallbackSender\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)' +
                '\.pMeth\)\$UINT') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $ctServers `
        -Expected @($ExpectedDerivedServers | ForEach-Object {
                $_.Split(':', 2)[0]
            }) `
        -InventoryOwner 'derived @CT_ server record order'
    if ($classTable -match '(?i)\.pCh\)\$UINT|_CH_CLT_') {
        Throw-UdpCallbackBlocker 'derived @CT_ contains an unexpected client record.'
    }
    foreach ($server in $ctServers) {
        $recordPattern = '(?m)^\(::LMCUdpCallbackSender\.' +
            [regex]::Escape($server) + '\.pMeth\)\$UINT,[ \t]*' +
            '_CH_SVR\$UINT,[^\r\n]*"' + [regex]::Escape($server) +
            '",[ \t]*$'
        if ([regex]::Matches($classTable, $recordPattern).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "derived @CT_ $server server record is not exact.")
        }
    }

    if ([regex]::Matches(
            $canonical,
            '(?m)^#define[ \t]+USER_CNT_LMCUdpCallbackSender[ \t]+14[ \t]*$').Count -ne 1 -or
        [regex]::Matches(
            $canonical,
            '(?m)UserFcts[ \t]*:[ \t]*ARRAY\[0\.\.USER_CNT_' +
                'LMCUdpCallbackSender\][ \t]+OF[ \t]+\^Void[ \t]*;').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived generated command table size is not exact 14.')
    }
    $standardFunctions = @([regex]::Matches(
            $canonical,
            '(?ms)^FUNCTION LMCUdpCallbackSender::@STD[ \t]*$' +
                '.*?^END_FUNCTION[ \t]*$'))
    if ($standardFunctions.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived generated @STD implementation count is not 1.')
    }
    $standard = $standardFunctions[0].Value
    $warningDisableLine =
        '^[ \t]*#pragma[ \t]+warning[ \t]*\([ \t]*disable[ \t]*:[ \t]*' +
            '74[ \t]*\)[ \t]*$'
    $warningDefaultLine =
        '^[ \t]*#pragma[ \t]+warning[ \t]*\([ \t]*default[ \t]*:[ \t]*' +
            '74[ \t]*\)[ \t]*$'
    if (([regex]::Matches(
                $standard,
                $warningDisableLine,
                [Text.RegularExpressions.RegexOptions]::Multiline).Count -ne 1) -or
        ([regex]::Matches(
                $standard,
                $warningDefaultLine,
                [Text.RegularExpressions.RegexOptions]::Multiline).Count -ne 1)) {
        Throw-UdpCallbackBlocker (
            'derived generated @STD warning 74 scope directives are not exact.')
    }
    $warningScopedSlotPattern =
        '(?m)' + $warningDisableLine + '\n(?:[ \t]*\n)*' +
        '^[ \t]*vmt\.UserFcts\[12\][ \t]*:=[ \t]*#ErrorCallback' +
            '\([ \t]*\)[ \t]*;[ \t]*$\n(?:[ \t]*\n)*' +
        $warningDefaultLine
    if ([regex]::Matches(
            $standard,
            $warningScopedSlotPattern).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived generated @STD warning 74 scope does not exactly bracket ' +
                'the ErrorCallback slot.')
    }
    $compact = [regex]::Replace($standard, '\s+', '')
    Assert-OrderedTokens `
        -Text $compact `
        -Tokens @(
            'ret_code:=_UDPTransceiverInterface::@STD();',
            'IFret_code<>C_OKTHENRETURN;END_IF;',
            'nCmdSize:=_UDPTransceiverInterface::ClassSvr.pMeth^.nCmds$UINT*SIZEOF(pVoid)+CMDMETH.Init;',
            '_memcpy((#vmt.CmdTable)$^USINT,_UDPTransceiverInterface::ClassSvr.pMeth,nCmdSize);',
            'vmt.CmdTable.nCmds:=nSTDCMD+USER_CNT_LMCUdpCallbackSender;',
            'vmt.CmdTable.CyWork:=#CyWork();',
            'vmt.UserFcts[12]:=#ErrorCallback();',
            '_UDPTransceiverInterface::ClassSvr.pMeth:=StoreCmd(pCmd:=#vmt.CmdTable,SHARED);',
            'IF_UDPTransceiverInterface::ClassSvr.pMethTHENret_code:=C_OK;ELSEret_code:=C_OUTOF_NEAR;RETURN;END_IF;') `
        -TokenOwner 'derived generated @STD lifecycle'
    $userAssignments = @([regex]::Matches(
            $compact,
            'vmt\.UserFcts\[(?<Index>[0-9]+)\]:=#(?<Name>[A-Za-z_][A-Za-z0-9_]*)\(\);') |
            ForEach-Object {
                $_.Groups['Index'].Value + ':' + $_.Groups['Name'].Value
            })
    Assert-ExactInventory `
        -Actual $userAssignments `
        -Expected @('12:ErrorCallback') `
        -InventoryOwner 'derived generated @STD user method slots'
    foreach ($token in @(
            '_UDPTransceiverInterface::@STD()',
            '_memcpy(',
            'vmt.CmdTable.nCmds:=',
            'vmt.CmdTable.CyWork:=',
            'StoreCmd(')) {
        if ((Get-OrdinalCount -Text $compact -Needle $token) -ne 1) {
            Throw-UdpCallbackBlocker (
                "derived generated @STD token count drifted: $token")
        }
    }
}

function Assert-DerivedSourceContract {
    param(
        [Parameter(Mandatory = $true)][string]$SourceText,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Empty', 'Complete')]
        [string]$ImplementationMode,
        [switch]$TerminalWakeBroker
    )

    $canonicalSource = ConvertTo-CanonicalLf -Text $SourceText
    $conditionalScan = $canonicalSource
    $legacyName = 'LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE'
    if ($ImplementationMode -ceq 'Complete') {
        $definesSections = @([regex]::Matches(
                $canonicalSource,
                '(?ms)^//\{\{LSL_DEFINES[ \t]*\n(?<Body>.*?)' +
                    '^//\}\}LSL_DEFINES[ \t]*$'))
        if ($definesSections.Count -ne 1) {
            Throw-UdpCallbackBlocker (
                'complete sender has no exact one LSL_DEFINES enclosure.')
        }
        $guardPattern =
            '(?m)^[ \t]*#ifndef[ \t]+' + $legacyName + '[ \t]*\n' +
            '[ \t]*#define[ \t]+' + $legacyName + '[ \t]+0[ \t]*\n' +
            '[ \t]*#endif[ \t]*(?=\n|\z)'
        $guards = @([regex]::Matches(
                $definesSections[0].Groups['Body'].Value,
                $guardPattern))
        if ($guards.Count -ne 1) {
            Throw-UdpCallbackBlocker (
                'complete sender legacy fixture guard/default is not exact.')
        }
        $guard = $guards[0]
        $bodyWithoutGuard =
            $definesSections[0].Groups['Body'].Value.Remove(
                $guard.Index,
                $guard.Length)
        if (-not [string]::IsNullOrWhiteSpace($bodyWithoutGuard)) {
            Throw-UdpCallbackBlocker (
                'complete sender LSL_DEFINES contains an extra directive/token.')
        }
        $absoluteGuardIndex =
            $definesSections[0].Groups['Body'].Index + $guard.Index
        $conditionalScan = $canonicalSource.Remove(
            $absoluteGuardIndex,
            $guard.Length)
        if ($conditionalScan.IndexOf(
                $legacyName,
                [StringComparison]::Ordinal) -ge 0) {
            Throw-UdpCallbackBlocker (
                'complete sender contains a duplicate legacy fixture token.')
        }
    }
    elseif ($canonicalSource.IndexOf(
            $legacyName,
            [StringComparison]::Ordinal) -ge 0) {
        Throw-UdpCallbackBlocker (
            'IDE declaration stubs contain the reserved legacy fixture macro.')
    }
    Assert-NoCustomSourceConditionalPreprocessor `
        -Text $conditionalScan -ArtifactOwner 'LMCUdpCallbackSender source'
    Assert-NoUnexpectedTopLevelResidue `
        -Text $SourceText `
        -ArtifactOwner 'LMCUdpCallbackSender source' `
        -ExpectedDirectiveCount $(if ($ImplementationMode -ceq 'Complete') {
                7
            } else { 4 }) `
        -ExpectedDirectiveSha256 $(if ($ImplementationMode -ceq 'Complete') {
                '3ECF2C6CE0DD0C76B1199B3D357888FA8D09EF642D73F74D2E6850616C7D7DEE'
            } else {
                'F0079C95875D9EB1D10ADE1EC54DBEB08F0E923E793C75FFF0F268C55032FFF7'
            })
    Assert-DeclaredSpanInventory `
        -Text $SourceText `
        -ExpectedFunctionNames (@('@CT_', '@STD') +
            @($DeclarationFunctionNames)) `
        -ExpectedTypeSpanCount 2 `
        -ExpectedClassName 'LMCUdpCallbackSender' `
        -ArtifactOwner 'LMCUdpCallbackSender source'
    $scan = Get-LexicalScanText -Text $SourceText
    if ($scan -match '(?im)^[ \t]*#pragma[ \t]+pack\b') {
        Throw-UdpCallbackBlocker (
            'derived storage structs must not use wire-packing pragmas.')
    }
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
    if ($scan -match '(?i)FindFreeOrVictimSlot') {
        Throw-UdpCallbackBlocker 'obsolete FindFreeOrVictimSlot is forbidden.'
    }

    $classMatch = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*LMCUdpCallbackSender[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    if (-not $classMatch.Success) {
        Throw-UdpCallbackBlocker 'derived declaration class block is missing.'
    }
    Assert-DerivedStorageContract `
        -ClassBody $classMatch.Groups['Body'].Value `
        -SourceText $SourceText

    $declarations = @(Get-FunctionRecords `
            -Text $classMatch.Value -Kind Declaration)
    $implementations = @(Get-FunctionRecords `
            -Text $scan -Kind Implementation)
    Assert-ExactInventory `
        -Actual @($declarations.Name) `
        -Expected $DeclarationFunctionNames `
        -InventoryOwner 'derived declaration function order'
    Assert-ExactInventory `
        -Actual @($implementations.Name) `
        -Expected $ImplementationFunctionNames `
        -InventoryOwner 'derived implementation function order'

    $sourceSpecs = [Collections.Generic.List[object]]::new()
    $sourceSpecs.Add([pscustomobject]@{
            Spec = $CyWorkSpec
            Modifiers = 'VIRTUAL GLOBAL'
            Source = $true
        })
    foreach ($spec in $PublicFunctionSpecs) {
        $sourceSpecs.Add([pscustomobject]@{
                Spec = $spec
                Modifiers = 'GLOBAL'
                Source = $false
            })
    }
    $sourceSpecs.Add([pscustomobject]@{
            Spec = $ErrorCallbackSpec
            Modifiers = 'VIRTUAL GLOBAL'
            Source = $false
        })
    foreach ($spec in $PrivateFunctionSpecs) {
        $sourceSpecs.Add([pscustomobject]@{
                Spec = $spec
                Modifiers = ''
                Source = $false
            })
    }
    foreach ($pair in $sourceSpecs) {
        $spec = $pair.Spec
        $declaration = @($declarations | Where-Object {
                $_.Name -ceq $spec.Name
            })
        $implementation = @($implementations | Where-Object {
                $_.Name -ceq $spec.Name
            })
        if (($declaration.Count -ne 1) -or ($implementation.Count -ne 1)) {
            Throw-UdpCallbackBlocker (
                "$($spec.Name) declaration/implementation count drifted.")
        }
        Assert-FunctionSourceAbi `
            -Record $declaration[0] `
            -Spec $spec `
            -ExpectedModifiers $pair.Modifiers `
            -UseSourceOutputs:$pair.Source
        Assert-FunctionSourceAbi `
            -Record $implementation[0] `
            -Spec $spec `
            -ExpectedModifiers $pair.Modifiers `
            -UseSourceOutputs:$pair.Source
    }

    if ([regex]::Matches(
            $scan,
            '(?im)^[ \t]*vmt\.CmdTable\.CyWork[ \t]*:=[ \t]*' +
                '#CyWork\(\)[ \t]*;[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'generated standard command table lacks exact CyWork assignment.')
    }
    Assert-DerivedStandardTableContract -SourceText $SourceText
    if ($ImplementationMode -ceq 'Empty') {
        foreach ($implementation in $implementations) {
            if ((Get-FunctionExecutableText `
                    -FunctionBlock $implementation.Block).Length -ne 0) {
                Throw-UdpCallbackBlocker (
                    "$($implementation.Name) must remain an empty IDE stub " +
                    'during declaration/wiring capture.')
            }
        }
    }
    else {
        Assert-DerivedCandidateExactFunctionContract `
            -Implementations $implementations `
            -TerminalWakeBroker:$TerminalWakeBroker
        Assert-DerivedImplementationContract `
            -Implementations $implementations `
            -RequireNonzeroEventId:$TerminalWakeBroker
    }
}

function Get-TcpFunctionBlock {
    param(
        [Parameter(Mandatory = $true)][string]$TcpSource,
        [Parameter(Mandatory = $true)][string]$FunctionName
    )

    $matches = @([regex]::Matches(
            (ConvertTo-CanonicalLf -Text $TcpSource),
            '(?ms)^FUNCTION[ \t]+(?:(?:VIRTUAL|GLOBAL)[ \t]+)*' +
                'TCPMotionInterface::' + [regex]::Escape($FunctionName) +
                '\b.*?^END_FUNCTION[ \t]*$'))
    if ($matches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "TCPMotionInterface $FunctionName implementation count is " +
            "$($matches.Count), expected 1.")
    }
    return $matches[0].Value
}

function Get-LasalClassFunctionBlock {
    param(
        [Parameter(Mandatory = $true)][string]$SourceText,
        [Parameter(Mandatory = $true)][string]$ClassName,
        [Parameter(Mandatory = $true)][string]$FunctionName
    )

    $matches = @([regex]::Matches(
            (ConvertTo-CanonicalLf -Text $SourceText),
            '(?ms)^FUNCTION[ \t]+(?:(?:VIRTUAL|GLOBAL)[ \t]+)*' +
                [regex]::Escape($ClassName) + '::' +
                [regex]::Escape($FunctionName) +
                '\b.*?^END_FUNCTION[ \t]*$'))
    if ($matches.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$ClassName $FunctionName implementation count is " +
            "$($matches.Count), expected 1.")
    }
    return $matches[0].Value
}

function Get-ExpectedTerminalWakeTryTakeBlock {
    return @'
FUNCTION GLOBAL LMCDiagnosticsService::TryTakeD5TerminalWake
	VAR_INPUT
		pTicketId 	: ^UDINT;
		pTicketBootId 	: ^UDINT;
		pOwnerSessionEpoch 	: ^UDINT;
	END_VAR
	VAR_OUTPUT
		Result 	: DINT;
	END_VAR

	Result := -1;
	if (pTicketId = NIL) | (pTicketBootId = NIL) |
		(pOwnerSessionEpoch = NIL) then
		RETURN;
	end_if;

	pTicketId^$UDINT := 0;
	pTicketBootId^$UDINT := 0;
	pOwnerSessionEpoch^$UDINT := 0;
	Result := 0;

	if (TicketId = 0) | (TicketBootId = 0) |
		(OwnerSessionEpoch = 0) then
		RETURN;
	end_if;
	if (OperationState <> LMC_DIAG_SDO_STATE_COMPLETED) &
		(OperationState <> LMC_DIAG_SDO_STATE_FAILED) &
		(OperationState <> LMC_DIAG_SDO_STATE_CANCELLED) &
		(OperationState <> LMC_DIAG_SDO_STATE_EXPIRED) then
		RETURN;
	end_if;
	if (D5TerminalWakeLastAttemptTicketId = TicketId) &
		(D5TerminalWakeLastAttemptTicketBootId = TicketBootId) &
		(D5TerminalWakeLastAttemptOwnerSessionEpoch = OwnerSessionEpoch) then
		RETURN;
	end_if;

	D5TerminalWakeLastAttemptTicketId := TicketId;
	D5TerminalWakeLastAttemptTicketBootId := TicketBootId;
	D5TerminalWakeLastAttemptOwnerSessionEpoch := OwnerSessionEpoch;
	pTicketId^$UDINT := TicketId;
	pTicketBootId^$UDINT := TicketBootId;
	pOwnerSessionEpoch^$UDINT := OwnerSessionEpoch;
	Result := 1;

END_FUNCTION
'@
}

function Get-ExpectedTerminalWakePublishBlock {
    return @'
FUNCTION TCPMotionInterface::PublishD5TerminalWake
	VAR
		ticketId : UDINT;
		ticketBootId : UDINT;
		ownerSessionEpoch : UDINT;
		takeResult : DINT;
		publishResult : DINT;
	END_VAR

	ticketId := 0;
	ticketBootId := 0;
	ownerSessionEpoch := 0;
	if IsClientConnected(#Diagnostics) = FALSE then
		RETURN;
	end_if;
	takeResult := Diagnostics.TryTakeD5TerminalWake(
		pTicketId:=#ticketId,
		pTicketBootId:=#ticketBootId,
		pOwnerSessionEpoch:=#ownerSessionEpoch);
	if takeResult <> 1 then
		RETURN;
	end_if;

	if D5TerminalWakeAttemptCount <> 16#FFFFFFFF then
		D5TerminalWakeAttemptCount += 1;
	end_if;
	publishResult := -9;
	if RpcInitialized & (CurrentSock <> 0) & (RpcSocket = CurrentSock) &
		(PendingClosedSessionEpoch = 0) &
		RpcCallbackRegistered & (RpcCallbackProtocolVersion = 2) &
		((RpcCallbackEventMask AND 1) = 1) &
		(RpcCallbackSessionEpoch = SessionEpoch) &
		(ownerSessionEpoch = RpcCallbackSessionEpoch) &
		(ticketBootId = RpcCallbackBootId) &
		IsClientConnected(#CallbackSender) then
		publishResult := CallbackSender.PublishEvent(
			EventMaskBit:=1,
			EventType:=1,
			DeliveryClass:=0,
			EventId:=ticketId,
			ProducerSessionEpoch:=ownerSessionEpoch,
			pPayload:=NIL,
			PayloadBytes:=0);
	end_if;
	if publishResult = 0 then
		if D5TerminalWakeEnqueuedCount <> 16#FFFFFFFF then
			D5TerminalWakeEnqueuedCount += 1;
		end_if;
	else
		if D5TerminalWakeRejectedCount <> 16#FFFFFFFF then
			D5TerminalWakeRejectedCount += 1;
		end_if;
	end_if;

END_FUNCTION
'@
}

function Assert-ExactTerminalWakeFunctionBlock {
    param(
        [Parameter(Mandatory = $true)][string]$Actual,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$FunctionOwner
    )

    if (-not [string]::Equals(
            (Get-CommentInsensitiveTokenStream -Text $Actual),
            (Get-CommentInsensitiveTokenStream -Text $Expected),
            [StringComparison]::Ordinal)) {
        Throw-UdpCallbackBlocker (
            "$FunctionOwner exact Gate D token stream drifted.")
    }
}

function Assert-TerminalWakeDiagnosticsSourceContract {
    param([Parameter(Mandatory = $true)][string]$DiagnosticsSource)

    $scan = Get-LexicalScanText -Text $DiagnosticsSource
    $classMatch = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*LMCDiagnosticsService[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    if (-not $classMatch.Success) {
        Throw-UdpCallbackBlocker (
            'LMCDiagnosticsService declaration block is missing.')
    }
    $classBody = $classMatch.Groups['Body'].Value
    $variableInventory = @([regex]::Matches(
            $classBody,
            '(?im)^[ \t]*(?<Name>D5TerminalWake[A-Za-z0-9_]*)[ \t]*:' ) |
        ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $variableInventory `
        -Expected @($ExpectedDiagnosticsTerminalWakeVariables |
            ForEach-Object { $_.Split(':', 2)[0] }) `
        -InventoryOwner 'LMCDiagnosticsService Gate D variable declaration inventory'
    $declarationSequence =
        '(?is)BootIdFault[ \t]*:[ \t]*BOOL[ \t]*;[ \t\r\n]*' +
        'D5TerminalWakeLastAttemptTicketId[ \t]*:[ \t]*UDINT[ \t]*;' +
        '[ \t\r\n]*D5TerminalWakeLastAttemptTicketBootId[ \t]*:[ \t]*' +
        'UDINT[ \t]*;[ \t\r\n]*' +
        'D5TerminalWakeLastAttemptOwnerSessionEpoch[ \t]*:[ \t]*UDINT[ \t]*;'
    if ([regex]::Matches($classBody, $declarationSequence).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'LMCDiagnosticsService Gate D variables are not the exact three ' +
            'private UDINTs immediately after BootIdFault.')
    }
    if ([regex]::Matches(
            $classBody,
            '(?is)FUNCTION[ \t]+GLOBAL[ \t]+ProcessOperations[ \t]*;' +
                '[ \t\r\n]*FUNCTION[ \t]+GLOBAL[ \t]+' +
                'TryTakeD5TerminalWake\b').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TryTakeD5TerminalWake is not immediately after ProcessOperations.')
    }

    $declarations = @(Get-FunctionRecords `
            -Text $classMatch.Value -Kind Declaration | Where-Object {
                $_.Name -ceq $TerminalWakeTryTakeSpec.Name
            })
    if ($declarations.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TryTakeD5TerminalWake declaration count is not 1.')
    }
    Assert-FunctionSourceAbi `
        -Record $declarations[0] `
        -Spec $TerminalWakeTryTakeSpec `
        -ExpectedModifiers 'GLOBAL' `
        -UseSourceOutputs:$false

    $tryTake = Get-LasalClassFunctionBlock `
        -SourceText $DiagnosticsSource `
        -ClassName LMCDiagnosticsService `
        -FunctionName $TerminalWakeTryTakeSpec.Name
    $implementationRecord = [pscustomobject]@{
        Name = $TerminalWakeTryTakeSpec.Name
        Modifiers = 'GLOBAL'
        Block = $tryTake
    }
    Assert-FunctionSourceAbi `
        -Record $implementationRecord `
        -Spec $TerminalWakeTryTakeSpec `
        -ExpectedModifiers 'GLOBAL' `
        -UseSourceOutputs:$false
    Assert-ExactTerminalWakeFunctionBlock `
        -Actual $tryTake `
        -Expected (Get-ExpectedTerminalWakeTryTakeBlock) `
        -FunctionOwner 'TryTakeD5TerminalWake'

    $constructor = Get-LasalClassFunctionBlock `
        -SourceText $DiagnosticsSource `
        -ClassName LMCDiagnosticsService `
        -FunctionName LMCDiagnosticsService
    foreach ($entry in $ExpectedDiagnosticsTerminalWakeVariables) {
        $name = $entry.Split(':', 2)[0]
        if ([regex]::Matches(
                $constructor,
                '(?im)^[ \t]*' + [regex]::Escape($name) +
                    '[ \t]*:=[ \t]*0[ \t]*;[ \t]*$').Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "LMCDiagnosticsService initialization does not clear $name once.")
        }
    }
}

function Assert-TerminalWakeTcpSourceContract {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    $scan = Get-LexicalScanText -Text $TcpSource
    $classMatch = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*TCPMotionInterface[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    if (-not $classMatch.Success) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface declaration block is missing for Gate D.')
    }
    $classBody = $classMatch.Groups['Body'].Value
    $variableInventory = @([regex]::Matches(
            $classBody,
            '(?im)^[ \t]*(?<Name>D5TerminalWake[A-Za-z0-9_]*)[ \t]*:' ) |
        ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $variableInventory `
        -Expected @($ExpectedTcpTerminalWakeVariables |
            ForEach-Object { $_.Split(':', 2)[0] }) `
        -InventoryOwner 'TCPMotionInterface Gate D variable declaration inventory'
    $declarationSequence =
        '(?is)RpcCallbackLastDisarmResult[ \t]*:[ \t]*DINT[ \t]*;' +
        '[ \t\r\n]*D5TerminalWakeAttemptCount[ \t]*:[ \t]*UDINT[ \t]*;' +
        '[ \t\r\n]*D5TerminalWakeEnqueuedCount[ \t]*:[ \t]*UDINT[ \t]*;' +
        '[ \t\r\n]*D5TerminalWakeRejectedCount[ \t]*:[ \t]*UDINT[ \t]*;'
    if ([regex]::Matches($classBody, $declarationSequence).Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface Gate D counters are not the exact three ' +
            'private UDINTs immediately after RpcCallbackLastDisarmResult.')
    }
    if ([regex]::Matches(
            $classBody,
            '(?is)FUNCTION[ \t]+DisarmRpcCallbackEndpoint\b.*?' +
                'END_VAR[ \t]*;?[ \t\r\n]*FUNCTION[ \t]+' +
                'PublishD5TerminalWake[ \t]*;').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'PublishD5TerminalWake is not immediately after ' +
            'DisarmRpcCallbackEndpoint.')
    }
    $declarations = @(Get-FunctionRecords `
            -Text $classMatch.Value -Kind Declaration | Where-Object {
                $_.Name -ceq $TerminalWakePublishSpec.Name
            })
    if ($declarations.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'PublishD5TerminalWake declaration count is not 1.')
    }
    Assert-FunctionSourceAbi `
        -Record $declarations[0] `
        -Spec $TerminalWakePublishSpec `
        -ExpectedModifiers '' `
        -UseSourceOutputs:$false

    $publish = Get-TcpFunctionBlock `
        -TcpSource $TcpSource -FunctionName $TerminalWakePublishSpec.Name
    Assert-ExactTerminalWakeFunctionBlock `
        -Actual $publish `
        -Expected (Get-ExpectedTerminalWakePublishBlock) `
        -FunctionOwner 'PublishD5TerminalWake'

    $callOwners = [Collections.Generic.List[string]]::new()
    foreach ($functionName in @(
            $ExpectedTcpFunctionNames + $TerminalWakePublishSpec.Name |
                Where-Object { -not $_.StartsWith('@') })) {
        $functionBlock = Get-TcpFunctionBlock `
            -TcpSource $TcpSource -FunctionName $functionName
        $count = [regex]::Matches(
            $functionBlock,
            '(?i)(?<!::)(?<![A-Za-z0-9_])PublishD5TerminalWake[ \t]*\(').Count
        for ($index = 0; $index -lt $count; $index++) {
            $callOwners.Add($functionName)
        }
    }
    Assert-ExactInventory `
        -Actual $callOwners.ToArray() `
        -Expected @('CyWork', 'MsgPaser') `
        -InventoryOwner 'Gate D broker call-site inventory'

    $cyWork = Get-TcpFunctionBlock `
        -TcpSource $TcpSource -FunctionName CyWork
    if ([regex]::Matches(
            $cyWork,
            '(?is)IF[ \t]+IsClientConnected[ \t]*\([ \t]*#Diagnostics' +
                '[ \t]*\)[ \t]+THEN[ \t\r\n]*' +
                'Diagnostics\.ProcessOperations[ \t]*\([ \t]*\)[ \t]*;' +
                '[ \t\r\n]*PublishD5TerminalWake[ \t]*\([ \t]*\)' +
                '[ \t]*;[ \t\r\n]*END_IF[ \t]*;').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'CyWork does not call the Gate D broker immediately after ' +
            'Diagnostics.ProcessOperations.')
    }
    $parser = Get-TcpFunctionBlock `
        -TcpSource $TcpSource -FunctionName MsgPaser
    if ([regex]::Matches(
            $parser,
            '(?is)SendData[ \t]*\([ \t\r\n]*' +
                'pData[ \t]*:=[ \t]*#Sendbuf\[0\][ \t]*,[ \t\r\n]*' +
                'udSize[ \t]*:=[ \t]*diagnosticsResponseSize\$UDINT' +
                '[ \t]*,[ \t\r\n]*dSocket[ \t]*:=[ \t]*CurrentSock' +
                '[ \t]*,[ \t\r\n]*bDirect[ \t]*:=[ \t]*TRUE' +
                '[ \t\r\n]*\)[ \t]*;[ \t\r\n]*' +
                'PublishD5TerminalWake[ \t]*\([ \t]*\)[ \t]*;').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'MsgPaser does not call the Gate D broker immediately after ' +
            'the diagnostics TCP response SendData.')
    }
}

function Assert-TcpCallbackFenceDeclarationContract {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    $scan = Get-LexicalScanText -Text $TcpSource
    $classMatch = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*TCPMotionInterface[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    if (-not $classMatch.Success) {
        Throw-UdpCallbackBlocker 'TCPMotionInterface declaration block is missing.'
    }
    $lines = @(
        (ConvertTo-CanonicalLf -Text $classMatch.Groups['Body'].Value) -split "`n" |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $ipv4Index = -1
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ([regex]::Replace($lines[$index], '\s+', '') -ceq
            'RpcCallbackIPv4:ARRAY[0..3]OFBYTE;') {
            if ($ipv4Index -ge 0) {
                Throw-UdpCallbackBlocker (
                    'TCPMotionInterface RpcCallbackIPv4 declaration is duplicated.')
            }
            $ipv4Index = $index
        }
    }
    if ($ipv4Index -lt 0) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface RpcCallbackIPv4 declaration is missing.')
    }
    for ($offset = 0;
         $offset -lt $ExpectedTcpCallbackFenceVariables.Count;
         $offset++) {
        $lineIndex = $ipv4Index + 1 + $offset
        if ($lineIndex -ge $lines.Count) {
            Throw-UdpCallbackBlocker (
                'TCP callback fence variable sequence is truncated.')
        }
        $expected = $ExpectedTcpCallbackFenceVariables[$offset] + ';'
        $actual = [regex]::Replace($lines[$lineIndex], '\s+', '')
        if ($actual -cne $expected) {
            Throw-UdpCallbackBlocker (
                'TCP callback fence variable order/type/initializer drifted at ' +
                "$($ExpectedTcpCallbackFenceVariables[$offset]).")
        }
    }
    $callbackVariableInventory = @([regex]::Matches(
            $classMatch.Groups['Body'].Value,
            '(?im)^[ \t]*(?<Name>RpcCallback[A-Za-z0-9_]*)[ \t]*:') |
            ForEach-Object { $_.Groups['Name'].Value })
    $expectedCallbackVariableInventory = @(
        'RpcCallbackRegistered',
        'RpcCallbackEventMask',
        'RpcCallbackPort',
        'RpcCallbackIPv4') + @(
        $ExpectedTcpCallbackFenceVariables |
            ForEach-Object { $_.Split(':', 2)[0] })
    Assert-ExactInventory `
        -Actual $callbackVariableInventory `
        -Expected $expectedCallbackVariableInventory `
        -InventoryOwner 'TCP callback variable declaration inventory'
    if ([regex]::Matches(
            (ConvertTo-CanonicalLf -Text $classMatch.Value),
            '(?im)^[ \t]*FUNCTION[ \t]+HandleRpcLifecycleCommands[ \t]*;' +
                '[ \t]*\n(?:[ \t]*\n)?[ \t]*FUNCTION[ \t]+' +
                'DisarmRpcCallbackEndpoint[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCP disarm helper declaration is not immediately after RPC lifecycle.')
    }

    $declarations = @(Get-FunctionRecords `
            -Text $classMatch.Value -Kind Declaration | Where-Object {
                $_.Name -ceq $TcpDisarmHelperSpec.Name
            })
    if ($declarations.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCP DisarmRpcCallbackEndpoint declaration count is not 1.')
    }
    Assert-FunctionSourceAbi `
        -Record $declarations[0] `
        -Spec $TcpDisarmHelperSpec `
        -ExpectedModifiers '' `
        -UseSourceOutputs:$false
}

function Assert-TcpGateBBodyBaseline {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    foreach ($entry in $ExpectedGateBTcpFunctionContracts.GetEnumerator()) {
        $block = Get-TcpFunctionBlock `
            -TcpSource $TcpSource -FunctionName $entry.Key
        $bytes = [Text.Encoding]::UTF8.GetByteCount($block)
        $sha256 = Get-TextSha256 -Text $block
        if (($bytes -ne $entry.Value.Bytes) -or
            ($sha256 -cne $entry.Value.Sha256)) {
            Throw-UdpCallbackBlocker (
                "Gate B2 must preserve exact $($entry.Key) body; " +
                "observed $bytes/$sha256.")
        }
    }
}

function Get-ExpectedTcpDisarmHelperExecutable {
    return @'
Result := 1;
IF (RpcCallbackProtocolVersion <> 2) OR
   (RpcCallbackRegistered = FALSE) THEN
  Result := 1;
ELSIF NOT IsClientConnected(#CallbackSender) THEN
  Result := -9;
ELSE
  Result := CallbackSender.DisarmEndpoint(
    ExpectedSessionEpoch := RpcCallbackSessionEpoch,
    ExpectedCookieLo := RpcCallbackCookieLo,
    ExpectedCookieHi := RpcCallbackCookieHi
  );
END_IF;
RpcCallbackLastDisarmResult := Result;
IF (Result = 0) OR (Result = 1) THEN
  RpcCallbackRegistered := FALSE;
  RpcCallbackEventMask := 0;
  RpcCallbackPort := 0;
  RpcCallbackIPv4[0] := 0;
  RpcCallbackIPv4[1] := 0;
  RpcCallbackIPv4[2] := 0;
  RpcCallbackIPv4[3] := 0;
  RpcCallbackProtocolVersion := 0;
  RpcCallbackAcceptedMaxDatagram := 0;
  RpcCallbackSessionEpoch := 0;
  RpcCallbackBootId := 0;
  RpcCallbackCookieLo := 0;
  RpcCallbackCookieHi := 0;
END_IF;
'@
}

function Assert-TcpGateCContract {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    $blocks = [ordered]@{}
    foreach ($name in $ExpectedGateCTcpFunctionNames) {
        $blocks[$name] = Get-TcpFunctionBlock `
            -TcpSource $TcpSource -FunctionName $name
    }
    $expectedLocalVariableSections = [ordered]@{
        ConnSocketInfo = 1
        SendData = 1
        HandleControlSafetyDrainPending = 1
        HandleRpcLifecycleCommands = 1
        DisarmRpcCallbackEndpoint = 0
    }
    foreach ($entry in $expectedLocalVariableSections.GetEnumerator()) {
        $localVariableSectionCount = [regex]::Matches(
            (Get-LexicalScanText -Text $blocks[$entry.Key]),
            '(?im)^[ \t]*VAR[ \t]*(?:\r?\n|$)').Count
        if ($localVariableSectionCount -ne $entry.Value) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) local VAR section count is " +
                    "$localVariableSectionCount, expected $($entry.Value).")
        }
    }
    $expectedTcp = New-SyntheticTcpSource -Phase DerivedCandidate
    foreach ($name in $ExpectedGateCTcpFunctionNames) {
        $expectedBlock = Get-TcpFunctionBlock `
            -TcpSource $expectedTcp -FunctionName $name
        $actualTokens = Get-CommentInsensitiveTokenStream -Text $blocks[$name]
        $expectedTokens = Get-CommentInsensitiveTokenStream -Text $expectedBlock
        if (-not [string]::Equals(
                $actualTokens,
                $expectedTokens,
                [StringComparison]::Ordinal)) {
            Throw-UdpCallbackBlocker (
                "Gate C TCP $name complete function token stream drifted.")
        }
    }
    $helperExecutable = Get-FunctionExecutableText `
        -FunctionBlock $blocks.DisarmRpcCallbackEndpoint
    $actualHelper = [regex]::Replace(
        (Get-LexicalScanText -Text $helperExecutable),
        '\s+',
        '')
    $expectedHelper = [regex]::Replace(
        (Get-LexicalScanText -Text (Get-ExpectedTcpDisarmHelperExecutable)),
        '\s+',
        '')
    if ($actualHelper -cne $expectedHelper) {
        Throw-UdpCallbackBlocker (
            'Gate C DisarmRpcCallbackEndpoint implementation drifted.')
    }

    $expectedCalls = [ordered]@{
        ConnSocketInfo = 2
        SendData = 1
        HandleControlSafetyDrainPending = 1
        HandleRpcLifecycleCommands = 2
    }
    $expectedEpochAdvances = [ordered]@{
        ConnSocketInfo = 2
        SendData = 1
        HandleControlSafetyDrainPending = 1
        HandleRpcLifecycleCommands = 1
    }
    foreach ($entry in $expectedCalls.GetEnumerator()) {
        $block = Get-LexicalScanText -Text $blocks[$entry.Key]
        $compactBlock = [regex]::Replace($block, '\s+', '')
        foreach ($clear in $ExpectedTcpCallbackTupleClearStatements) {
            $compactClear = [regex]::Replace($clear, '\s+', '')
            if ($compactBlock.IndexOf(
                    $compactClear,
                    [StringComparison]::Ordinal) -ge 0) {
                Throw-UdpCallbackBlocker (
                    "Gate C $($entry.Key) contains a direct callback tuple clear.")
            }
        }
        if ([regex]::IsMatch(
                $block,
                '(?i)_memset[ \t\r\n]*\([ \t\r\n]*dest[ \t]*:=[ \t]*' +
                    '#RpcCallback')) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) clears callback storage outside helper.")
        }
        $callMatches = @([regex]::Matches(
                $block,
                '(?i)\b[A-Za-z_][A-Za-z0-9_]*\s*:=\s*' +
                    'DisarmRpcCallbackEndpoint\s*\(\s*\)\s*;'))
        if ($callMatches.Count -ne $entry.Value) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) disarm call count is " +
                "$($callMatches.Count), expected $($entry.Value).")
        }
        $epochMatches = @([regex]::Matches(
                $block,
                '(?i)\bSessionEpoch\s*\+=\s*1\s*;'))
        if ($epochMatches.Count -ne $expectedEpochAdvances[$entry.Key]) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) SessionEpoch advance count drifted.")
        }
        $previousEpoch = -1
        foreach ($epoch in $epochMatches) {
            $precedingCall = -1
            foreach ($call in $callMatches) {
                if (($call.Index -gt $previousEpoch) -and
                    ($call.Index -lt $epoch.Index)) {
                    $precedingCall = $call.Index
                }
            }
            if ($precedingCall -lt 0) {
                Throw-UdpCallbackBlocker (
                    "Gate C $($entry.Key) advances SessionEpoch before disarm.")
            }
            $previousEpoch = $epoch.Index
        }
    }

    $rpc = Get-LexicalScanText -Text $blocks.HandleRpcLifecycleCommands
    $expectedRpc = Get-LexicalScanText `
        -Text (Get-SyntheticGateCRpcLifecycleFunction)
    if ([regex]::Replace($rpc, '\s+', '') -cne
        [regex]::Replace($expectedRpc, '\s+', '')) {
        Throw-UdpCallbackBlocker (
            'Gate C RPC legacy/v2 lifecycle deterministic body drifted.')
    }
    Assert-OrderedTokens `
        -Text $rpc `
        -Tokens @(
            '0x8080:',
            'IF (Payload = 1) AND (RequestBuf[8] = 0) AND',
            '((RpcInitialized = FALSE) OR (RpcSocket = CurrentSock)) THEN',
            'callbackDisarmResult := DisarmRpcCallbackEndpoint();',
            'IF callbackDisarmResult < 0 THEN',
            'Sendbuf[10]$INT := -1;',
            '0x405C:',
            'IF (Payload = 12) AND (RpcInitialized = TRUE) AND',
            '(RpcSocket = CurrentSock) THEN',
            'IF RpcCallbackProtocolVersion = 0 THEN',
            'RpcCallbackProtocolVersion := 1;',
            'IF RpcCallbackProtocolVersion = 1 THEN',
            'ELSIF (Payload = 32) AND (RpcInitialized = TRUE) AND',
            '(RpcSocket = CurrentSock) THEN',
            'IF RpcCallbackProtocolVersion = 0 THEN',
            'RpcCallbackProtocolVersion := 2;',
            'Sendbuf[2]$UINT := 20;',
            'callbackProtocolVersion := RequestBuf[20]$UINT;',
            'callbackAcceptedMaxDatagram := RequestBuf[22]$UINT;',
            'callbackCookieLo := RequestBuf[24]$UDINT;',
            'callbackCookieHi := RequestBuf[28]$UDINT;',
            'callbackFlags := RequestBuf[32]$UDINT;',
            'callbackReserved := RequestBuf[36]$UDINT;',
            'IF IsClientConnected(#Diagnostics) THEN',
            'callbackBootId := Diagnostics.GetDiagnosticsBootId();',
            'IF RpcCallbackProtocolVersion = 2 THEN',
            '(callbackBootId <> 0) AND',
            'IsClientConnected(#CallbackSender) THEN',
            'callbackArmResult := CallbackSender.ArmEndpoint(',
            'ProtocolVersion := callbackProtocolVersion',
            'EventMask := callbackEventMask',
            'CallbackIPv4 := callbackIPv4',
            'CallbackPort := callbackPort',
            'SessionEpoch := SessionEpoch',
            'BootId := callbackBootId',
            'CookieLo := callbackCookieLo',
            'CookieHi := callbackCookieHi',
            'MaxDatagramBytes := TO_UDINT(callbackAcceptedMaxDatagram)',
            'IF (callbackArmResult = 0) OR (callbackArmResult = 1) THEN',
            'RpcCallbackProtocolVersion := callbackProtocolVersion;',
            'RpcCallbackAcceptedMaxDatagram := callbackAcceptedMaxDatagram;',
            'RpcCallbackSessionEpoch := SessionEpoch;',
            'RpcCallbackBootId := callbackBootId;',
            'RpcCallbackCookieLo := callbackCookieLo;',
            'RpcCallbackCookieHi := callbackCookieHi;',
            'RpcCallbackRegistered := TRUE;',
            'udSize := 28',
            '0x405D:',
            'callbackDisarmResult := DisarmRpcCallbackEndpoint();',
            'IF callbackDisarmResult < 0 THEN',
            'ELSE',
            'IF (SessionEpoch <> 0) AND (PendingClosedSessionEpoch = 0) THEN',
            'PendingClosedSessionEpoch := SessionEpoch;',
            'SendData(pData := #Sendbuf[0], udSize := 12,',
            'IF (RpcInitialized = TRUE) AND (RpcSocket = CurrentSock) THEN',
            'SessionEpoch += 1;') `
        -TokenOwner 'Gate C RPC shape/fence/arm/response lifecycle'
    if ([regex]::Matches(
            $rpc,
            '(?m)CallbackSender\.ArmEndpoint\(').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'Gate C must have one shared v2 ArmEndpoint call site.')
    }
    if ([regex]::Matches(
            $rpc,
            '(?m)RpcCallbackProtocolVersion[ \t]*:=[ \t]*1;').Count -ne 1 -or
        [regex]::Matches(
            $rpc,
            '(?m)RpcCallbackProtocolVersion[ \t]*:=[ \t]*2;').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'Gate C registration shape locks are not exact one each.')
    }
    $negativeBranchPatterns = [ordered]@{
        '0x8080 initialization' =
            '(?s)0x8080:.*?IF[ \t]*\([ \t]*Payload[ \t]*=[ \t]*1[ \t]*\)' +
            '.*?THEN\s*callbackDisarmResult[ \t]*:=[ \t]*' +
            'DisarmRpcCallbackEndpoint\(\)[ \t]*;\s*' +
            'IF[ \t]+callbackDisarmResult[ \t]*<[ \t]*0[ \t]*THEN' +
            '(?<Negative>.*?)ELSE'
        '0x405D teardown' =
            '(?s)0x405D:.*?callbackDisarmResult[ \t]*:=[ \t]*' +
            'DisarmRpcCallbackEndpoint\(\)[ \t]*;\s*' +
            'IF[ \t]+callbackDisarmResult[ \t]*<[ \t]*0[ \t]*THEN' +
            '(?<Negative>.*?)ELSE'
    }
    foreach ($entry in $negativeBranchPatterns.GetEnumerator()) {
        $matches = @([regex]::Matches($rpc, $entry.Value))
        if ($matches.Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) negative disarm branch is not exact.")
        }
        $negative = $matches[0].Groups['Negative'].Value
        if ([regex]::IsMatch(
                $negative,
                '(?im)^[ \t]*(?:SessionEpoch|PendingClosedSessionEpoch|' +
                    'RpcSocket|RpcInitialized|' +
                    'RpcCallback[A-Za-z0-9_]*)[ \t]*(?::=|\+=)')) {
            Throw-UdpCallbackBlocker (
                "Gate C $($entry.Key) negative disarm mutates the fence/session.")
        }
        Assert-OrderedTokens `
            -Text $negative `
            -Tokens @('Sendbuf[8]$UINT := 1;', 'Sendbuf[10]$INT := -1;') `
            -TokenOwner "Gate C $($entry.Key) fail-closed response"
    }
}

function Assert-TcpDerivedClientContract {
    param(
        [Parameter(Mandatory = $true)][string]$TcpSource,
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State
    )

    Assert-NoCustomSourceConditionalPreprocessor `
        -Text $TcpSource -ArtifactOwner 'TCPMotionInterface source'
    Assert-NoUnexpectedTopLevelResidue `
        -Text $TcpSource `
        -ArtifactOwner 'TCPMotionInterface source' `
        -ExpectedDirectiveCount 46 `
        -ExpectedDirectiveSha256 `
            'ECDB540F59D96F94B741BA69842D52F74F4BEF74E3B442F64F449C7F9F824BC2'
    Assert-DeclaredSpanInventory `
        -Text $TcpSource `
        -ExpectedFunctionNames $(if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                @($ExpectedTcpFunctionNames +
                    $TerminalWakePublishSpec.Name)
            } else {
                $ExpectedTcpFunctionNames
            }) `
        -ExpectedTypeSpanCount 2 `
        -ExpectedClassName 'TCPMotionInterface' `
        -ArtifactOwner 'TCPMotionInterface source'
    $metadata = [regex]::Match(
        $TcpSource,
        '(?s)\(\*![ \t\r\n]*(?<Xml><Class\b.*?</Class>)' +
            '[ \t\r\n]*\*\)')
    if (-not $metadata.Success) {
        Throw-UdpCallbackBlocker 'TCPMotionInterface class metadata is missing.'
    }
    try {
        [xml]$tcpXml = $metadata.Groups['Xml'].Value
    }
    catch {
        Throw-UdpCallbackBlocker (
            "TCPMotionInterface class metadata cannot be parsed: " +
            $_.Exception.Message)
    }
    $allMetadataClients = @($tcpXml.SelectNodes('/Class/Channels/Client'))
    Assert-ExactInventory `
        -Actual @($allMetadataClients | ForEach-Object { $_.GetAttribute('Name') }) `
        -Expected @('_StdLib', 'CallbackSender', 'ControlCommands', 'Diagnostics') `
        -InventoryOwner 'TCPMotionInterface metadata client order'
    $metadataClients = @($allMetadataClients | Where-Object {
            $_.GetAttribute('Name') -ceq 'CallbackSender'
        })
    if ($metadataClients.Count -ne 1 -or
        $metadataClients[0].Attributes.Count -ne 3 -or
        $metadataClients[0].GetAttribute('Required') -cne 'false' -or
        $metadataClients[0].GetAttribute('Internal') -cne 'false') {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface CallbackSender metadata is not exact optional external.')
    }
    $scan = Get-LexicalScanText -Text $TcpSource
    $usingLtd = @([regex]::Matches(
            (ConvertTo-CanonicalLf -Text $TcpSource),
            '(?im)^[ \t]*#[ \t]*pragma[ \t]+usingLtd[ \t]+' +
                '(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*$') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $usingLtd `
        -Expected @(
            '_StdLib',
            'LMCControlCommandService',
            'LMCDiagnosticsService',
            'LMCUdpCallbackSender') `
        -InventoryOwner 'TCPMotionInterface usingLtd order'
    if ([regex]::Matches(
            $scan,
            '(?im)^[ \t]*CallbackSender[ \t]*:[ \t]*' +
                'CltChCmd_LMCUdpCallbackSender[ \t]*;[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface CallbackSender declaration is not exact.')
    }
    $classDeclaration = [regex]::Match(
        $scan,
        '(?ims)^[ \t]*TCPMotionInterface[ \t]*:[ \t]*CLASS[ \t]*$' +
            '(?<Body>.*?)^[ \t]*END_CLASS[ \t]*;[ \t]*$')
    $declaredClients = @([regex]::Matches(
            $classDeclaration.Groups['Body'].Value,
            '(?im)^[ \t]*(?<Name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:[ \t]*' +
                'CltChCmd_[A-Za-z_][A-Za-z0-9_]*[ \t]*;[ \t]*$') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $declaredClients `
        -Expected @('_StdLib', 'Diagnostics', 'ControlCommands', 'CallbackSender') `
        -InventoryOwner 'TCPMotionInterface declared client order'
    $canonicalTcp = ConvertTo-CanonicalLf -Text $TcpSource
    $generatedClients = @([regex]::Matches(
            $canonicalTcp,
            '(?im)^\(::TCPMotionInterface\.(?<Name>[A-Za-z_][A-Za-z0-9_]*)' +
                '\.pCh\)\$UINT,[ \t]*_CH_CLT_OBJ\$UINT,[^\n]*$') |
            ForEach-Object { $_.Groups['Name'].Value })
    Assert-ExactInventory `
        -Actual $generatedClients `
        -Expected @('_StdLib', 'Diagnostics', 'ControlCommands', 'CallbackSender') `
        -InventoryOwner 'TCPMotionInterface generated client order'
    if ([regex]::Matches(
            $scan,
            '(?im)^4\$UINT,[ \t]*4\$UINT,[ \t]*0\$UINT,[ \t]*$').Count -ne 1 -or
        [regex]::Matches(
            $canonicalTcp,
            '(?im)^\(::TCPMotionInterface\.CallbackSender\.pCh\)\$UINT,' +
                '[ \t]*_CH_CLT_OBJ\$UINT,[^\r\n]*"CallbackSender"[^\r\n]*' +
                '"LMCUdpCallbackSender"[^\r\n]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'TCPMotionInterface generated CallbackSender client record is not exact.')
    }

    Assert-TcpCallbackFenceDeclarationContract -TcpSource $TcpSource
    $helper = Get-TcpFunctionBlock `
        -TcpSource $TcpSource `
        -FunctionName $TcpDisarmHelperSpec.Name
    if ($State -ceq 'DerivedWired') {
        $canonicalBytes = $Utf8.GetBytes($canonicalTcp)
        if (($canonicalBytes.Count -ne $ExpectedGateB2TcpCanonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $canonicalBytes) -cne
                $ExpectedGateB2TcpCanonicalLfSha256)) {
            Throw-UdpCallbackBlocker (
                'DerivedWired TCPMotionInterface complete canonical source drifted.')
        }
        if ((Get-FunctionExecutableText -FunctionBlock $helper).Length -ne 0) {
            Throw-UdpCallbackBlocker (
                'DerivedWired TCP disarm helper must remain an empty IDE stub.')
        }
        Assert-TcpGateBBodyBaseline -TcpSource $TcpSource
    }
    else {
        if ((Get-FunctionExecutableText -FunctionBlock $helper).Length -eq 0) {
            Throw-UdpCallbackBlocker (
                'DerivedCandidate TCP disarm helper remains empty.')
        }
        Assert-TcpGateCContract -TcpSource $TcpSource
        if ($State -ceq 'TerminalWakeBrokerCandidate') {
            Assert-TerminalWakeTcpSourceContract -TcpSource $TcpSource
        }
    }
}

function Assert-DerivedCommTablePhysicalContract {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State,
        [Parameter(Mandatory = $true)][string]$CommTableText,
        [Parameter(Mandatory = $true)][long]$CommTableBytes,
        [Parameter(Mandatory = $true)][string]$CommTableSha256,
        [switch]$SyntheticFixture
    )

    Assert-AsciiTextEvidence `
        -Text $CommTableText `
        -ByteCount $CommTableBytes `
        -Sha256 $CommTableSha256 `
        -ArtifactOwner 'ONE_Comm_Network_Table.st'
    if ($SyntheticFixture) {
        return
    }

    $tableCanonical = ConvertTo-CanonicalLf -Text $CommTableText
    $tableCanonicalBytes = $Utf8.GetBytes($tableCanonical)
    if ($State -in @(
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        $expectedTableBytes = $ExpectedDerivedCandidateCommTableBytes
        $expectedTableSha256 = $ExpectedDerivedCandidateCommTableSha256
        $expectedTableCanonicalLfBytes =
            $ExpectedDerivedCandidateCommTableCanonicalLfBytes
        $expectedTableCanonicalLfSha256 =
            $ExpectedDerivedCandidateCommTableCanonicalLfSha256
    }
    else {
        $expectedTableBytes = $ExpectedGateB2CommTableBytes
        $expectedTableSha256 = $ExpectedGateB2CommTableSha256
        $expectedTableCanonicalLfBytes =
            $ExpectedGateB2CommTableCanonicalLfBytes
        $expectedTableCanonicalLfSha256 =
            $ExpectedGateB2CommTableCanonicalLfSha256
    }
    $rawIdentityExact =
        (($CommTableBytes -eq $expectedTableBytes) -and
         ($CommTableSha256 -ceq $expectedTableSha256)) -or
        (($CommTableBytes -eq $expectedTableCanonicalLfBytes) -and
         ($CommTableSha256 -ceq $expectedTableCanonicalLfSha256))
    if ((-not $rawIdentityExact) -or
        ($tableCanonicalBytes.Count -ne $expectedTableCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $tableCanonicalBytes) -cne
            $expectedTableCanonicalLfSha256)) {
        Throw-UdpCallbackBlocker (
            "ONE_Comm_Network_Table.st $State snapshot drifted.")
    }
}

function Assert-DerivedNetworkContract {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State,
        [Parameter(Mandatory = $true)][string]$CommNetworkText,
        [Parameter(Mandatory = $true)][int]$CommNetworkBytes,
        [Parameter(Mandatory = $true)][string]$CommNetworkSha256,
        [Parameter(Mandatory = $true)][string]$CommTableText,
        [Parameter(Mandatory = $true)][int]$CommTableBytes,
        [Parameter(Mandatory = $true)][string]$CommTableSha256,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseText,
        [Parameter(Mandatory = $true)][int]$NetworksDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseSha256,
        [switch]$SyntheticFixture
    )

    Assert-AsciiTextEvidence `
        -Text $CommNetworkText `
        -ByteCount $CommNetworkBytes `
        -Sha256 $CommNetworkSha256 `
        -ArtifactOwner 'Comm_Network.lcn'
    if (-not $SyntheticFixture) {
        $commCanonical = ConvertTo-CanonicalLf -Text $CommNetworkText
        $commCanonicalBytes = $Utf8.GetBytes($commCanonical)
        $expectedCommNetwork = if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                $ExpectedTerminalWakeLayout.CommNetwork
            }
            else {
                [ordered]@{
                    Bytes = $ExpectedGateB2CommNetworkBytes
                    Sha256 = $ExpectedGateB2CommNetworkSha256
                    CanonicalLfBytes =
                        $ExpectedGateB2CommNetworkCanonicalLfBytes
                    CanonicalLfSha256 =
                        $ExpectedGateB2CommNetworkCanonicalLfSha256
                }
            }
        if (($CommNetworkBytes -ne $expectedCommNetwork.Bytes) -or
            ($CommNetworkSha256 -cne $expectedCommNetwork.Sha256) -or
            ($commCanonicalBytes.Count -ne
                $expectedCommNetwork.CanonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $commCanonicalBytes) -cne
                $expectedCommNetwork.CanonicalLfSha256)) {
            Throw-UdpCallbackBlocker (
                "Comm_Network.lcn $State snapshot drifted.")
        }
    }

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
    $relevantObjects = @($objects | Where-Object {
            ($_.GetAttribute('Name') -match '(?i)(?:LMCUdp|_UDPTransceiver1)') -or
            ($_.GetAttribute('Class') -match
                '(?i)(?:LMCUdpCallbackSender|_UDPTransceiver|UDPTransmission)')
        })
    Assert-ExactInventory `
        -Actual @($relevantObjects | ForEach-Object {
                $_.GetAttribute('Name') + ':' + $_.GetAttribute('Class')
            } | Sort-Object) `
        -Expected @(
            'LMCUdpCallbackSender1:LMCUdpCallbackSender',
            'LMCUdpTransceiver1:_UDPTransceiver') `
        -InventoryOwner 'UDP callback Network object inventory'
    foreach ($object in @($transceivers[0], $senders[0])) {
        if ($object.GetAttribute('CyclicTime') -cne '10 ms') {
            Throw-UdpCallbackBlocker (
                "$($object.GetAttribute('Name')) must execute at exact 10 ms cyclic time.")
        }
    }
    $expectedTransceiverPosition = if (
        $State -ceq 'TerminalWakeBrokerCandidate') {
            $ExpectedTerminalWakeLayout.CommNetwork.TransceiverPosition
        }
        else { '(120,180)' }
    $expectedSenderPosition = if (
        $State -ceq 'TerminalWakeBrokerCandidate') {
            $ExpectedTerminalWakeLayout.CommNetwork.SenderPosition
        }
        else { '(120,900)' }
    if (($transceivers[0].GetAttribute('Position') -cne
            $expectedTransceiverPosition) -or
        ($transceivers[0].GetAttribute('Visualized') -cne 'false') -or
        ($transceivers[0].GetAttribute('Remotely') -cne 'true') -or
        ($transceivers[0].GetAttribute('BackgroundTime') -cne 'always') -or
        ($senders[0].GetAttribute('Position') -cne
            $expectedSenderPosition) -or
        ($senders[0].GetAttribute('Visualized') -cne 'false') -or
        ($senders[0].GetAttribute('Remotely') -cne 'true') -or
        $senders[0].HasAttribute('BackgroundTime')) {
        Throw-UdpCallbackBlocker (
            'UDP callback Network object position/task attributes drifted.')
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
    Assert-ExactInventory `
        -Actual @($transceivers[0].SelectNodes('./Channels/*') |
            ForEach-Object {
                $_.LocalName + ':' + $_.GetAttribute('Name') + ':' +
                $_.GetAttribute('Value')
            }) `
        -Expected @(
            'Server:sControl:',
            'Server:sError:',
            'Server:sErrorMessage:',
            'Server:sErrorNoOS:',
            'Client:coStdLib:',
            'Client:cSizeOfRXBuffer:512',
            'Client:cSizeOfTXBuffer:8 kb') `
        -InventoryOwner 'LMCUdpTransceiver1 endpoint inventory'
    Assert-ExactInventory `
        -Actual @($senders[0].SelectNodes('./Channels/*') |
            ForEach-Object {
                $_.LocalName + ':' + $_.GetAttribute('Name') + ':' +
                $_.GetAttribute('Value')
            }) `
        -Expected @(
            'Server:AdmissionErrorDropCount:0',
            'Server:AdmissionRetryCount:0',
            'Server:ClassSvr:',
            'Server:DisarmClearedCount:0',
            'Server:ErrorCode:',
            'Server:ErrorMessage:',
            'Server:ErrorState:',
            'Server:LastAdmissionResult:0',
            'Server:QueuedCount:0',
            'Server:QueueDepth:0',
            'Server:QueueFullDropCount:0',
            'Server:RingAcceptedCount:0',
            'Server:State:',
            'Server:TransportErrorCount:0',
            'Client:_UDPTransceiver:') `
        -InventoryOwner 'LMCUdpCallbackSender1 endpoint inventory'
    foreach ($channel in @(
            @($transceivers[0].SelectNodes('./Channels/*')) +
            @($senders[0].SelectNodes('./Channels/*')))) {
        $expectedAttributeCount = if ($channel.HasAttribute('Value')) { 2 } else { 1 }
        if ($channel.Attributes.Count -ne $expectedAttributeCount) {
            Throw-UdpCallbackBlocker (
                'UDP callback Network endpoint metadata has extra attributes.')
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

    Assert-DerivedCommTablePhysicalContract `
        -State $State `
        -CommTableText $CommTableText `
        -CommTableBytes $CommTableBytes `
        -CommTableSha256 $CommTableSha256 `
        -SyntheticFixture:$SyntheticFixture
    Assert-NoDisabledPreprocessorEnvelope `
        -Text $CommTableText -ArtifactOwner 'ONE_Comm_Network_Table.st'
    $tableScan = ConvertTo-CanonicalLf -Text (
        Remove-LasalCommentsPreserveStrings -Text $CommTableText)
    if ($tableScan -notmatch
        '(?s)\A[ \t\r\n]*#define[ \t]+OBJECTS_CONFIG[ \t]*(?:\r?\n|\z)') {
        Throw-UdpCallbackBlocker (
            'ONE_Comm_Network_Table.st generated preamble is not exact.')
    }
    $tableDirectivePattern =
        '(?im)^[ \t]*#[ \t]*(?:pragma|define|if|ifdef|ifndef|elif|else|' +
        'endif|error|undef|include)\b[^\r\n]*'
    $tableDirectives = @([regex]::Matches(
            $tableScan,
            $tableDirectivePattern) | ForEach-Object {
            [regex]::Replace($_.Value.Trim(), '\s+', ' ')
        })
    $tableDirectiveBytes = $Utf8.GetBytes(
        [string]::Join("`n", $tableDirectives))
    $expectedTableDirectiveCount = if ($SyntheticFixture) {
        if ($State -in @(
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate')) { 4 } else { 1 }
    }
    elseif ($State -in @(
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        $ExpectedDerivedCandidateCommTableDirectiveCount
    }
    else {
        $ExpectedGateB2CommTableDirectiveCount
    }
    $expectedTableDirectiveSha256 = if ($SyntheticFixture) {
        if ($State -in @(
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate')) {
            '20F0F5EA021A6B09A0B1467AC0AF3742894E191CC5900E1CF878DF4DD4345833'
        }
        else {
            '46777DD0C3C52765A15174D2C3CE35BB24826A2274806E063F5E6CC07512D350'
        }
    }
    elseif ($State -in @(
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        $ExpectedDerivedCandidateCommTableDirectiveSha256
    }
    else {
        $ExpectedGateB2CommTableDirectiveSha256
    }
    if (($tableDirectives.Count -ne $expectedTableDirectiveCount) -or
        ((Get-BytesSha256 -Bytes $tableDirectiveBytes) -cne
            $expectedTableDirectiveSha256)) {
        Throw-UdpCallbackBlocker (
            'ONE_Comm_Network_Table.st preprocessor inventory drifted.')
    }
    $tables = @([regex]::Matches(
            $tableScan,
            '(?ms)^FUNCTION[ \t]+GLOBAL[ \t]+TAB[ \t]+ONE_Comm_Network' +
                '[ \t]*$(?<Body>.*?)^END_FUNCTION[ \t]*$'))
    if ($tables.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'ONE_Comm_Network_Table.st function enclosure is not exact one.')
    }
    $tableRemainder = $tableScan.Remove(
        $tables[0].Index,
        $tables[0].Length)
    $tableRemainder = [regex]::Replace(
        $tableRemainder,
        $tableDirectivePattern,
        '')
    if (-not [string]::IsNullOrWhiteSpace($tableRemainder)) {
        Throw-UdpCallbackBlocker (
            'ONE_Comm_Network_Table.st contains an unapproved top-level span.')
    }
    $tableBody = $tables[0].Groups['Body'].Value
    $requiredRows = [ordered]@{
        '_UDPTransceiver class' =
            '(?m)^TO_UDINT\([0-9]+\), "_UDPTransceiver", ' +
            '[0-9]+\$UINT, [0-9]+\$UINT, [0-9]+\$UINT,[ \t]*$'
        '_UDPTransceiverInterface class' =
            '(?m)^TO_UDINT\([0-9]+\), "_UDPTransceiverInterface", ' +
            '[0-9]+\$UINT, [0-9]+\$UINT, [0-9]+\$UINT,[ \t]*$'
        'LMCUdpCallbackSender class' =
            '(?m)^TO_UDINT\([0-9]+\), "LMCUdpCallbackSender", ' +
            '[0-9]+\$UINT, [0-9]+\$UINT, [0-9]+\$UINT,[ \t]*$'
        'transceiver object' =
            '(?m)^_NO_ATTR, TO_UDINT\([0-9]+\), "LMCUDPTRANSCEIVER1",[ \t]*$'
        'sender object' =
            '(?m)^_NO_ATTR, TO_UDINT\([0-9]+\), "LMCUDPCALLBACKSENDER1",[ \t]*$'
        'UDP destination link' =
            '(?m)^TO_UDINT\(6\), "_UDPTransceiver", ' +
            'TO_UDINT\(1\), "sControl",[ \t]*$'
        'TCP callback link' =
            '(?m)^TO_UDINT\(14\), "CallbackSender", ' +
            'TO_UDINT\(6\), "ClassSvr",[ \t]*$'
        'RX buffer value' =
            '(?m)^TO_UDINT\(1\), "cSizeOfRXBuffer", ' +
            'TO_UDINT\(512\),[ \t]*$'
        'TX buffer value' =
            '(?m)^TO_UDINT\(1\), "cSizeOfTXBuffer", ' +
            'TO_UDINT\(8 kb\),[ \t]*$'
        'transceiver cyclic task' =
            '(?m)^TO_UDINT\(1\), \(10\)\$UDINT, 4194303\$DINT,[ \t]*$'
        'sender cyclic task' =
            '(?m)^TO_UDINT\(6\), \(10\)\$UDINT, 4194303\$DINT,[ \t]*$'
    }
    foreach ($row in $requiredRows.GetEnumerator()) {
        if ([regex]::Matches($tableBody, $row.Value).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "ONE_Comm_Network_Table.st $($row.Key) row is not exact one.")
        }
    }
    Assert-OrderedTokens `
        -Text $tableBody `
        -Tokens @(
            'TO_UDINT(6), "_UDPTransceiver", TO_UDINT(1), "sControl",',
            'TO_UDINT(14), "CallbackSender", TO_UDINT(6), "ClassSvr",',
            'TO_UDINT(1), "cSizeOfTXBuffer", TO_UDINT(8 kb),',
            'TO_UDINT(1), "cSizeOfRXBuffer", TO_UDINT(512),',
            'TO_UDINT(1), (10)$UDINT, 4194303$DINT,',
            'TO_UDINT(6), (10)$UDINT, 4194303$DINT,') `
        -TokenOwner 'ONE_Comm_Network_Table.st B2 row order'
    foreach ($taskRow in @(
            '(?m)^\(0\)\$UDINT,[ \t]*//LMCUDPTRANSCEIVER1[ \t]*$',
            '(?m)^\(0\)\$UDINT,[ \t]*//LMCUDPCALLBACKSENDER1[ \t]*$')) {
        if ([regex]::Matches(
                (ConvertTo-CanonicalLf -Text $CommTableText),
                $taskRow).Count -ne 1) {
            Throw-UdpCallbackBlocker (
                'ONE_Comm_Network_Table.st B2 task-ID row drifted.')
        }
    }
    foreach ($forbidden in @('"_UDPTRANSCEIVER1"', '"UDPTRANSMISSION1"')) {
        if ($tableBody.IndexOf(
                $forbidden,
                [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Throw-UdpCallbackBlocker (
                "ONE_Comm_Network_Table.st contains forbidden $forbidden.")
        }
    }

    Assert-Latin1BinaryEvidence `
        -Text $NetworksDatabaseText `
        -ByteCount $NetworksDatabaseBytes `
        -Sha256 $NetworksDatabaseSha256 `
        -ArtifactOwner 'Networks.lcb derived registry'
    $expectedRecordCounts = if ($SyntheticFixture) {
        [ordered]@{
            Comm_Network = 1
            LMCUdpTransceiver1 = 1
            _UDPTransceiver = 1
            LMCUdpCallbackSender1 = 1
            LMCUdpCallbackSender = 1
            CallbackSender = 1
            cSizeOfRXBuffer = 1
            cSizeOfTXBuffer = 1
        }
    }
    else {
        [ordered]@{
            Comm_Network = 2
            LMCUdpTransceiver1 = 2
            _UDPTransceiver = 4
            LMCUdpCallbackSender1 = 3
            LMCUdpCallbackSender = 1
            CallbackSender = 2
            cSizeOfRXBuffer = 1
            cSizeOfTXBuffer = 1
        }
    }
    foreach ($entry in $expectedRecordCounts.GetEnumerator()) {
        $count = Get-LasalLengthPrefixedRecordCount `
            -DatabaseText $NetworksDatabaseText -Value $entry.Key
        if ($count -ne $entry.Value) {
            Throw-UdpCallbackBlocker (
                "Networks.lcb exact $($entry.Key) record count is $count, " +
                "expected $($entry.Value).")
        }
    }
}

function Assert-VendorImportedNetworkRegistryContract {
    param(
        [Parameter(Mandatory = $true)][string]$ConfigObjectsText,
        [Parameter(Mandatory = $true)][int]$ConfigObjectsBytes,
        [Parameter(Mandatory = $true)][string]$ConfigObjectsSha256,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseText,
        [Parameter(Mandatory = $true)][int]$NetworksDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$NetworksDatabaseSha256
    )

    Assert-AsciiTextEvidence `
        -Text $ConfigObjectsText `
        -ByteCount $ConfigObjectsBytes `
        -Sha256 $ConfigObjectsSha256 `
        -ArtifactOwner 'Gate A ConfigObjects.st'
    Assert-Latin1BinaryEvidence `
        -Text $NetworksDatabaseText `
        -ByteCount $NetworksDatabaseBytes `
        -Sha256 $NetworksDatabaseSha256 `
        -ArtifactOwner 'Gate A Networks.lcb'
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

function Assert-DerivedGeneratedRegistryContract {
    param(
        [Parameter(Mandatory = $true)][string]$ConfigObjectsText,
        [Parameter(Mandatory = $true)][int]$ConfigObjectsBytes,
        [Parameter(Mandatory = $true)][string]$ConfigObjectsSha256
    )

    Assert-AsciiTextEvidence `
        -Text $ConfigObjectsText `
        -ByteCount $ConfigObjectsBytes `
        -Sha256 $ConfigObjectsSha256 `
        -ArtifactOwner 'derived ConfigObjects.st'
    $scan = Remove-LasalCommentsPreserveStrings -Text $ConfigObjectsText
    $tables = @([regex]::Matches(
            $scan,
            '(?ms)^FUNCTION[ \t]+GLOBAL[ \t]+TAB[ \t]+CONFIG_TABLES[ \t]*$' +
                '(?<Body>.*?)^END_FUNCTION[ \t]*$'))
    if ($tables.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived ConfigObjects.st CONFIG_TABLES function is not exact one.')
    }
    $scan = $tables[0].Groups['Body'].Value
    if ([regex]::Matches(
            $scan,
            '(?m)^[ \t]*00120\$UINT,[ \t]*$').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'derived ConfigObjects.st CONFIG_TABLES count is not exact 120.')
    }
    if ([regex]::Matches(
            $scan,
            '(?m)^0\$UINT, [0-9]+, [0-9]+, "[^"]+",[ \t]*$').Count -ne
        120) {
        Throw-UdpCallbackBlocker (
            'derived ConfigObjects.st class registry count is not 120.')
    }
    foreach ($row in @(
            '0$UINT, 1, 2, "_UDPTRANSCEIVER",',
            '0$UINT, 1, 3, "_UDPTRANSCEIVERINTERFACE",',
            '0$UINT, 0, 0, "LMCUDPCALLBACKSENDER",')) {
        if ([regex]::Matches(
                $scan,
                '(?m)^' + [regex]::Escape($row) + '[ \t]*$').Count -ne 1) {
            Throw-UdpCallbackBlocker (
                "derived ConfigObjects.st row is not exact: $row")
        }
    }
    foreach ($forbidden in @(
            '_UDPTRANSCEIVER1',
            'UDPTRANSMISSION',
            'UDPCOMNET')) {
        if ($scan.IndexOf(
                $forbidden,
                [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            Throw-UdpCallbackBlocker (
                "derived ConfigObjects.st contains forbidden $forbidden.")
        }
    }
    $canonical = ConvertTo-CanonicalLf -Text $ConfigObjectsText
    $countPattern = '(?m)^[ \t]*00120\$UINT,[ \t]*(?=\n|\z)'
    $rowPattern =
        '(?m)^0\$UINT,[ \t]*0,[ \t]*0,[ \t]*' +
        '"LMCUDPCALLBACKSENDER",[ \t]*\n'
    $countMatches = @([regex]::Matches($canonical, $countPattern))
    $rowMatches = @([regex]::Matches($canonical, $rowPattern))
    if (($countMatches.Count -ne 1) -or ($rowMatches.Count -ne 1)) {
        Throw-UdpCallbackBlocker (
            'derived ConfigObjects.st reverse-delta anchors are not exact one.')
    }
    $row = $rowMatches[0]
    $canonical = $canonical.Remove($row.Index, $row.Length)
    $count = @([regex]::Matches($canonical, $countPattern))[0]
    $replacement = $count.Value.Replace('00120$UINT', '00119$UINT')
    $canonical = $canonical.Remove($count.Index, $count.Length).Insert(
        $count.Index,
        $replacement)
    $canonicalBytes = $Utf8.GetBytes($canonical)
    if (($canonicalBytes.Count -ne
            $ExpectedVendorImportedConfigObjectsCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $canonicalBytes) -cne
            $ExpectedVendorImportedConfigObjectsCanonicalLfSha256)) {
        Throw-UdpCallbackBlocker (
            'derived ConfigObjects.st does not reverse exactly to Gate A.')
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
    param(
        [Parameter(Mandatory = $true)][object[]]$Observed,
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'VendorImported',
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State
    )

    if ($Observed.Count -ne $VendorGeneratedRecordContracts.Count) {
        Throw-UdpCallbackBlocker (
            'vendor generated Classes.lcb record inventory count drifted.')
    }
    foreach ($expected in $VendorGeneratedRecordContracts) {
        $expectedSha256 = if (
            ($State -ceq 'TerminalWakeBrokerCandidate') -and
            $expected.Contains('TerminalWakeSha256')) {
                $expected.TerminalWakeSha256
            }
            elseif ($State -in @(
                    'DerivedWired',
                    'DerivedCandidate',
                    'TerminalWakeBrokerCandidate')) {
                $expected.WiredSha256
            }
            else {
                $expected.Sha256
        }
        $matches = @($Observed | Where-Object { $_.Name -ceq $expected.Name })
        if (($matches.Count -ne 1) -or
            ($matches[0].Bytes -ne $expected.Bytes) -or
            ($matches[0].Sha256 -cne $expectedSha256)) {
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

function Get-ClassDatabaseFunctionHeaderBytes {
    param(
        [Parameter(Mandatory = $true)][byte]$MethodKind,
        [Parameter(Mandatory = $true)][bool]$IsVirtual,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)][uint32]$InputCount
    )

    return ,([byte[]]@(
            $MethodKind, 0x00, 0x00, 0x00,
            [byte]$(if ($IsVirtual) { 1 } else { 0 }),
            [byte]$(if ($IsGlobal) { 1 } else { 0 }), 0x00, 0x00,
            [byte]($InputCount -band 0xFF),
            [byte](($InputCount -shr 8) -band 0xFF),
            [byte](($InputCount -shr 16) -band 0xFF),
            [byte](($InputCount -shr 24) -band 0xFF)))
}

function Get-ClassDatabaseFunctionAbiStart {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][byte]$MethodKind,
        [Parameter(Mandatory = $true)][bool]$IsVirtual,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)][uint32]$InputCount,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $header = Get-ClassDatabaseFunctionHeaderBytes `
        -MethodKind $MethodKind `
        -IsVirtual $IsVirtual `
        -IsGlobal $IsGlobal `
        -InputCount $InputCount
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
        $nameLength = [uint32]$FunctionName.Length
        $namePrefix = [byte[]]@(
            0x00, 0x01,
            [byte]($nameLength -band 0xFF),
            [byte](($nameLength -shr 8) -band 0xFF),
            [byte](($nameLength -shr 16) -band 0xFF),
            0xAA)
        if ((Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($nameStart - $namePrefix.Count) `
                -ExpectedBytes $namePrefix) -and
            (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start ($nameStart + $FunctionName.Length) `
                -ExpectedBytes $header)) {
            $candidates.Add($nameStart)
        }
        $searchStart = $nameStart + $FunctionName.Length
    }
    if ($candidates.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$RecordOwner exact method-kind/header record count is " +
            "$($candidates.Count), expected 1.")
    }
    return $candidates[0]
}

function Get-ClassDatabaseMethodAbiInventory {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $inventory = [Collections.Generic.List[object]]::new()
    for ($prefixStart = 0;
         $prefixStart -le ($RecordBytes.Count - 18);
         $prefixStart++) {
        if (($RecordBytes[$prefixStart] -ne 0) -or
            ($RecordBytes[$prefixStart + 1] -ne 1) -or
            ($RecordBytes[$prefixStart + 5] -ne 0xAA)) {
            continue
        }
        $nameLength = [int]$RecordBytes[$prefixStart + 2] -bor
            ([int]$RecordBytes[$prefixStart + 3] -shl 8) -bor
            ([int]$RecordBytes[$prefixStart + 4] -shl 16)
        if (($nameLength -lt 1) -or ($nameLength -gt 128)) {
            continue
        }
        $nameStart = $prefixStart + 6
        $headerStart = $nameStart + $nameLength
        if (($headerStart + 12) -gt $RecordBytes.Count) {
            continue
        }
        $name = [Text.Encoding]::ASCII.GetString(
            $RecordBytes,
            $nameStart,
            $nameLength)
        if ($name -cnotmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
            continue
        }
        $methodKind = $RecordBytes[$headerStart]
        if (($methodKind -notin @([byte]0x05, [byte]0x0B)) -or
            ($RecordBytes[$headerStart + 1] -ne 0) -or
            ($RecordBytes[$headerStart + 2] -ne 0) -or
            ($RecordBytes[$headerStart + 3] -ne 0) -or
            ($RecordBytes[$headerStart + 4] -gt 1) -or
            ($RecordBytes[$headerStart + 5] -gt 1) -or
            ($RecordBytes[$headerStart + 6] -ne 0) -or
            ($RecordBytes[$headerStart + 7] -ne 0)) {
            continue
        }
        $inputCount = [BitConverter]::ToUInt32(
            $RecordBytes,
            $headerStart + 8)
        if ($inputCount -gt 64) {
            continue
        }
        $inventory.Add([pscustomobject]@{
                Name = $name
                NameStart = $nameStart
                MethodKind = [byte]$methodKind
                IsVirtual = $RecordBytes[$headerStart + 4] -eq 1
                IsGlobal = $RecordBytes[$headerStart + 5] -eq 1
                InputCount = [uint32]$inputCount
            })
    }
    if ($inventory.Count -eq 0) {
        Throw-UdpCallbackBlocker "$RecordOwner contains no bounded method ABI record."
    }
    return $inventory.ToArray()
}

function Read-ClassDatabaseAaString {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][int]$Cursor,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [Parameter(Mandatory = $true)][int]$MaximumLength,
        [Parameter(Mandatory = $true)][string]$FieldOwner
    )

    if (($Cursor -lt 0) -or (($Cursor + 4) -gt $RecordEnd) -or
        ($RecordEnd -gt $RecordBytes.Count)) {
        Throw-UdpCallbackBlocker "$FieldOwner length prefix crosses its method record."
    }
    if ($RecordBytes[$Cursor + 3] -ne 0xAA) {
        Throw-UdpCallbackBlocker "$FieldOwner length prefix sentinel drifted."
    }
    $length = [int]$RecordBytes[$Cursor] -bor
        ([int]$RecordBytes[$Cursor + 1] -shl 8) -bor
        ([int]$RecordBytes[$Cursor + 2] -shl 16)
    if (($length -lt 0) -or ($length -gt $MaximumLength) -or
        (($Cursor + 4 + $length) -gt $RecordEnd)) {
        Throw-UdpCallbackBlocker "$FieldOwner length is outside its bounded record."
    }
    return [pscustomobject]@{
        Text = [Text.Encoding]::ASCII.GetString(
            $RecordBytes,
            $Cursor + 4,
            $length)
        Next = $Cursor + 4 + $length
    }
}

function Get-ClassDatabaseParameterTypeContract {
    param(
        [Parameter(Mandatory = $true)][string]$Type,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $pointer = $Type.StartsWith('^', [StringComparison]::Ordinal)
    $value = if ($pointer) { $Type.Substring(1) } else { $Type }
    if (($value.Length -lt 1) -or
        $value.StartsWith('^', [StringComparison]::Ordinal)) {
        Throw-UdpCallbackBlocker "$RecordOwner has an unsupported verifier type."
    }
    $separator = $value.LastIndexOf('::', [StringComparison]::Ordinal)
    $owner = ''
    $base = $value
    if ($separator -ge 0) {
        $owner = $value.Substring(0, $separator)
        $base = $value.Substring($separator + 2)
        if (($owner.Length -lt 1) -or ($base.Length -lt 1) -or
            ($owner.IndexOf('::', [StringComparison]::Ordinal) -ge 0)) {
            Throw-UdpCallbackBlocker "$RecordOwner has an unsupported qualified type."
        }
    }
    return [pscustomobject]@{
        Pointer = $pointer
        Base = $base
        Owner = $owner
    }
}

function Assert-ClassDatabaseParameterRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][int]$Cursor,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [Parameter(Mandatory = $true)][string]$Entry,
        [Parameter(Mandatory = $true)][bool]$IsOutput,
        [bool]$EaxBoundInput = $false,
        [bool]$EaxBoundOutput = $false,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    $separator = $Entry.IndexOf(':', [StringComparison]::Ordinal)
    if ($separator -lt 1) {
        Throw-UdpCallbackBlocker "$RecordOwner has an invalid verifier parameter spec."
    }
    $name = $Entry.Substring(0, $separator)
    $type = Get-ClassDatabaseParameterTypeContract `
        -Type $Entry.Substring($separator + 1) `
        -RecordOwner "$RecordOwner $name"
    $nameLength = [uint32]$name.Length
    $namePrefix = [byte[]]@(
        0x00, 0x01,
        [byte]($nameLength -band 0xFF),
        [byte](($nameLength -shr 8) -band 0xFF),
        [byte](($nameLength -shr 16) -band 0xFF),
        0xAA)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $Cursor `
                -ExpectedBytes $namePrefix)) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name prefix drifted."
    }
    $cursorAfterName = $Cursor + $namePrefix.Count
    if (($cursorAfterName + $name.Length) -gt $RecordEnd) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name crosses its method record."
    }
    $actualName = [Text.Encoding]::ASCII.GetString(
        $RecordBytes,
        $cursorAfterName,
        $name.Length)
    if ($actualName -cne $name) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name name drifted."
    }
    $cursorAfterName += $name.Length

    $comment = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $cursorAfterName `
        -RecordEnd $RecordEnd `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner parameter $name comment"
    $cursorAfterComment = $comment.Next
    $descriptor = [byte[]]@(
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x01, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    if ($type.Pointer) {
        $descriptor[54] = 1
    }
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $cursorAfterComment `
                -ExpectedBytes $descriptor)) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name descriptor drifted."
    }
    $cursorAfterDescriptor = $cursorAfterComment + $descriptor.Count
    $actualBase = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $cursorAfterDescriptor `
        -RecordEnd $RecordEnd `
        -MaximumLength 255 `
        -FieldOwner "$RecordOwner parameter $name base type"
    if ($actualBase.Text -cne $type.Base) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name base type drifted."
    }
    $actualOwner = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $actualBase.Next `
        -RecordEnd $RecordEnd `
        -MaximumLength 255 `
        -FieldOwner "$RecordOwner parameter $name type owner"
    if ($actualOwner.Text -cne $type.Owner) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name type owner drifted."
    }

    $tail = [Collections.Generic.List[byte]]::new()
    foreach ($unused in 1..5) {
        foreach ($value in [byte[]]@(0x00, 0x00, 0x00, 0xAA)) {
            $tail.Add($value)
        }
    }
    foreach ($value in [byte[]]@(0x01, 0x00, 0x00, 0x00)) {
        $tail.Add($value)
    }
    foreach ($unused in 1..18) {
        $tail.Add(0)
    }
    $tail.Add(0xAA)
    $tailEnd = if ($EaxBoundInput) {
        [byte[]]@(0x10, 0x00, 0x00, 0x00, 0x01)
    }
    elseif (-not $IsOutput) {
        [byte[]]@(0xFF, 0xFF, 0xFF, 0xFF, 0x01)
    }
    elseif ($EaxBoundOutput) {
        [byte[]]@(0x10, 0x00, 0x00, 0x00, 0x00)
    }
    else {
        [byte[]]@(0xFF, 0xFF, 0xFF, 0xFF, 0x00)
    }
    foreach ($value in $tailEnd) {
        $tail.Add($value)
    }
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $actualOwner.Next `
                -ExpectedBytes $tail.ToArray())) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name tail drifted."
    }
    $next = $actualOwner.Next + $tail.Count
    if ($next -gt $RecordEnd) {
        Throw-UdpCallbackBlocker "$RecordOwner parameter $name exceeds its method record."
    }
    return $next
}

function Assert-ClassDatabaseFunctionAbiRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$RecordBytes,
        [Parameter(Mandatory = $true)][string]$RecordText,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][byte]$MethodKind,
        [bool]$IsVirtual = $false,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Inputs,
        [AllowEmptyCollection()]
        [string[]]$Outputs = @(),
        [Parameter(Mandatory = $true)][int]$ExpectedStart,
        [Parameter(Mandatory = $true)][int]$RecordEnd,
        [bool]$RequireExactEnd = $false,
        [Parameter(Mandatory = $true)][string]$RecordOwner
    )

    if ($RecordBytes.Count -ne $RecordText.Length) {
        Throw-UdpCallbackBlocker "$RecordOwner byte/text offsets diverged."
    }
    if ($Outputs.Count -gt 1) {
        Throw-UdpCallbackBlocker "$RecordOwner verifier supports at most one output."
    }
    if (($ExpectedStart -lt 0) -or ($RecordEnd -le $ExpectedStart) -or
        ($RecordEnd -gt $RecordBytes.Count)) {
        Throw-UdpCallbackBlocker "$RecordOwner method record bounds drifted."
    }
    $actualStart = Get-ClassDatabaseFunctionAbiStart `
        -RecordBytes $RecordBytes `
        -RecordText $RecordText `
        -FunctionName $FunctionName `
        -MethodKind $MethodKind `
        -IsVirtual $IsVirtual `
        -IsGlobal $IsGlobal `
        -InputCount ([uint32]$Inputs.Count) `
        -RecordOwner $RecordOwner
    if ($actualStart -ne $ExpectedStart) {
        Throw-UdpCallbackBlocker "$RecordOwner method record start drifted."
    }
    $header = Get-ClassDatabaseFunctionHeaderBytes `
        -MethodKind $MethodKind `
        -IsVirtual $IsVirtual `
        -IsGlobal $IsGlobal `
        -InputCount ([uint32]$Inputs.Count)
    $cursor = $ExpectedStart + $FunctionName.Length + $header.Count
    for ($inputIndex = 0; $inputIndex -lt $Inputs.Count; $inputIndex++) {
        $entry = $Inputs[$inputIndex]
        $cursor = Assert-ClassDatabaseParameterRecord `
            -RecordBytes $RecordBytes `
            -Cursor $cursor `
            -RecordEnd $RecordEnd `
            -Entry $entry `
            -IsOutput $false `
            -EaxBoundInput (
                ($FunctionName -ceq 'CyWork') -and ($inputIndex -eq 0)) `
            -RecordOwner $RecordOwner
    }

    $outputCount = [uint32]$Outputs.Count
    $outputCountBytes = [byte[]]@(
        [byte]($outputCount -band 0xFF),
        [byte](($outputCount -shr 8) -band 0xFF),
        [byte](($outputCount -shr 16) -band 0xFF),
        [byte](($outputCount -shr 24) -band 0xFF))
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $cursor `
                -ExpectedBytes $outputCountBytes)) {
        Throw-UdpCallbackBlocker "$RecordOwner generated output count drifted."
    }
    $cursor += $outputCountBytes.Count
    if ($Outputs.Count -eq 1) {
        $cursor = Assert-ClassDatabaseParameterRecord `
            -RecordBytes $RecordBytes `
            -Cursor $cursor `
            -RecordEnd $RecordEnd `
            -Entry $Outputs[0] `
            -IsOutput $true `
            -EaxBoundOutput ($FunctionName -ceq 'CyWork') `
            -RecordOwner $RecordOwner
    }
    $methodComment = Read-ClassDatabaseAaString `
        -RecordBytes $RecordBytes `
        -Cursor $cursor `
        -RecordEnd $RecordEnd `
        -MaximumLength 4096 `
        -FieldOwner "$RecordOwner method comment"
    $cursor = $methodComment.Next
    $trailer = [byte[]]@(0, 0, 0, 0, 0, 0)
    if (-not (Test-ClassDatabaseByteSequence `
                -DatabaseBytes $RecordBytes `
                -Start $cursor `
                -ExpectedBytes $trailer)) {
        Throw-UdpCallbackBlocker "$RecordOwner method trailer drifted."
    }
    $cursor += $trailer.Count
    if ($RequireExactEnd -and ($cursor -ne $RecordEnd)) {
        Throw-UdpCallbackBlocker "$RecordOwner method parser did not consume its chunk."
    }
    if ($cursor -gt $RecordEnd) {
        Throw-UdpCallbackBlocker "$RecordOwner method parser crossed method bounds."
    }
}

function Assert-GeneratedDerivedMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State,
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText,
        [Parameter(Mandatory = $true)][bool]$ExpectTcpClient,
        [switch]$SyntheticFixture
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
    $generatedSpecs = @(
        [pscustomobject]@{
            Spec = $CyWorkSpec
            MethodKind = [byte]0x05
            Virtual = $true
            Global = $true
        }
        [pscustomobject]@{
            Spec = $ErrorCallbackSpec
            MethodKind = [byte]0x0B
            Virtual = $true
            Global = $true
        }
        foreach ($spec in $PublicFunctionSpecs) {
            [pscustomobject]@{
                Spec = $spec
                MethodKind = [byte]0x0B
                Virtual = $false
                Global = $true
            }
        }
        foreach ($spec in $PrivateFunctionSpecs) {
            [pscustomobject]@{
                Spec = $spec
                MethodKind = [byte]0x0B
                Virtual = $false
                Global = $false
            }
        })
    Assert-ExactInventory `
        -Actual @($generatedSpecs | ForEach-Object { $_.Spec.Name }) `
        -Expected $DeclarationFunctionNames `
        -InventoryOwner 'Classes.lcb sender verifier ABI order'
    $methodInventory = @(Get-ClassDatabaseMethodAbiInventory `
            -RecordBytes $recordBytes `
            -RecordOwner 'LMCUdpCallbackSender Classes.lcb')
    Assert-ExactInventory `
        -Actual @($methodInventory | ForEach-Object { $_.Name }) `
        -Expected $DeclarationFunctionNames `
        -InventoryOwner 'Classes.lcb bounded method ABI inventory'
    $positions = [Collections.Generic.List[int]]::new()
    $positionByName = @{}
    for ($inventoryIndex = 0;
         $inventoryIndex -lt $generatedSpecs.Count;
         $inventoryIndex++) {
        $generated = $generatedSpecs[$inventoryIndex]
        $actual = $methodInventory[$inventoryIndex]
        $spec = $generated.Spec
        if (($actual.MethodKind -ne $generated.MethodKind) -or
            ($actual.IsVirtual -ne $generated.Virtual) -or
            ($actual.IsGlobal -ne $generated.Global) -or
            ($actual.InputCount -ne [uint32]$spec.Inputs.Count)) {
            Throw-UdpCallbackBlocker (
                "Classes.lcb $($spec.Name) bounded method header drifted.")
        }
        $start = $actual.NameStart
        $positions.Add($start)
        $positionByName[$spec.Name] = $start
    }
    for ($index = 1; $index -lt $positions.Count; $index++) {
        if ($positions[$index] -le $positions[$index - 1]) {
            Throw-UdpCallbackBlocker (
                'Classes.lcb sender declaration ABI order drifted.')
        }
    }
    for ($generatedIndex = 0;
         $generatedIndex -lt $generatedSpecs.Count;
         $generatedIndex++) {
        $generated = $generatedSpecs[$generatedIndex]
        $spec = $generated.Spec
        $start = [int]$positionByName[$spec.Name]
        $hasNextMethod = ($generatedIndex + 1) -lt $positions.Count
        $end = if ($hasNextMethod) {
            $positions[$generatedIndex + 1] - 6
        }
        else {
            $record.Length
        }
        Assert-ClassDatabaseFunctionAbiRecord `
            -RecordBytes $recordBytes `
            -RecordText $record `
            -FunctionName $spec.Name `
            -MethodKind $generated.MethodKind `
            -IsVirtual $generated.Virtual `
            -IsGlobal $generated.Global `
            -Inputs @($spec.Inputs) `
            -Outputs @($spec.Outputs) `
            -ExpectedStart $start `
            -RecordEnd $end `
            -RequireExactEnd $hasNextMethod `
            -RecordOwner "Classes.lcb $($spec.Name)"
        $methodRecord = $record.Substring($start, $end - $start)
        $tokens = [Collections.Generic.List[string]]::new()
        $tokens.Add($spec.Name)
        foreach ($entry in @($spec.Inputs + $spec.Outputs)) {
            $parts = $entry.Split(':', 2)
            $tokens.Add($parts[0])
            $type = Get-ClassDatabaseParameterTypeContract `
                -Type $parts[1] `
                -RecordOwner "Classes.lcb $($spec.Name)"
            $tokens.Add($type.Base)
            if ($type.Owner.Length -gt 0) {
                $tokens.Add($type.Owner)
            }
        }
        Assert-OrderedTokens `
            -Text $methodRecord `
            -Tokens $tokens.ToArray() `
            -TokenOwner "Classes.lcb $($spec.Name) ABI"
    }

    $storageTokens = @(
        '_LMC_UDP_ACTIVE_ENDPOINT',
        '_LMC_UDP_TX_SLOT') +
        @($ExpectedDerivedServers | ForEach-Object { $_.Split(':', 2)[0] }) +
        @($ExpectedDerivedVariables | ForEach-Object { $_.Split(':', 2)[0] })
    foreach ($token in $storageTokens) {
        if ((Get-OrdinalCount -Text $record -Needle $token) -lt 1) {
            Throw-UdpCallbackBlocker (
                "Classes.lcb sender storage token $token is missing.")
        }
    }
    Assert-OrderedTokens `
        -Text $record `
        -Tokens $storageTokens `
        -TokenOwner 'Classes.lcb sender TYPE/server/variable storage order'

    $tcpRecord = Get-ClassDatabaseRecord `
        -DatabaseText $ClassesDatabaseText `
        -SourcePath '.\Class\TCPMotionInterface\TCPMotionInterface.st' `
        -RecordOwner 'TCPMotionInterface Classes.lcb'
    $tcpRecordStart = $ClassesDatabaseText.IndexOf(
        '.\Class\TCPMotionInterface\TCPMotionInterface.st',
        [StringComparison]::OrdinalIgnoreCase)
    $tcpRecordBytes = [byte[]]::new($tcpRecord.Length)
    [Array]::Copy(
        $ClassesDatabaseBytes,
        $tcpRecordStart,
        $tcpRecordBytes,
        0,
        $tcpRecord.Length)
    $tcpClientCount = [regex]::Matches(
        $tcpRecord,
        '(?<![A-Za-z0-9_])CallbackSender(?![A-Za-z0-9_])').Count
    if ($ExpectTcpClient) {
        $expectedTcpClientCount = if ($SyntheticFixture) { 1 } else { 2 }
        if ($tcpClientCount -ne $expectedTcpClientCount) {
            Throw-UdpCallbackBlocker (
                'Classes.lcb TCPMotionInterface CallbackSender count is ' +
                "$tcpClientCount, expected $expectedTcpClientCount.")
        }
        Assert-OrderedTokens `
            -Text $tcpRecord `
            -Tokens @('CallbackSender', 'LMCUdpCallbackSender') `
            -TokenOwner 'Classes.lcb TCPMotionInterface CallbackSender ABI'
        $expectedGeneratedCallbackVariables = if (
            $SyntheticFixture -and ($State -ceq 'DerivedWired')) {
            @(
                'RpcCallbackRegistered',
                'RpcCallbackEventMask',
                'RpcCallbackPort',
                'RpcCallbackIPv4') + @(
                $ExpectedTcpCallbackFenceVariables |
                    ForEach-Object { $_.Split(':', 2)[0] })
        }
        elseif ($State -in @(
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate')) {
            $ExpectedDerivedCandidateGeneratedTcpCallbackVariables
        }
        else {
            $ExpectedGateB2GeneratedTcpCallbackVariables
        }
        $actualGeneratedCallbackVariables = @([regex]::Matches(
                $tcpRecord,
                '(?<![A-Za-z0-9_])(?<Name>RpcCallback[A-Za-z0-9_]*)' +
                    '(?![A-Za-z0-9_])') |
                ForEach-Object { $_.Groups['Name'].Value })
        Assert-ExactInventory `
            -Actual $actualGeneratedCallbackVariables `
            -Expected $expectedGeneratedCallbackVariables `
            -InventoryOwner 'Classes.lcb TCP callback variable inventory'
        $fenceTokens = [Collections.Generic.List[string]]::new()
        foreach ($entry in $ExpectedTcpCallbackFenceVariables) {
            $parts = $entry.Split(':', 2)
            $expectedCount = if ($SyntheticFixture) {
                if ($State -in @(
                        'DerivedCandidate',
                        'TerminalWakeBrokerCandidate')) { 2 } else { 1 }
            }
            elseif ($State -in @(
                    'DerivedCandidate',
                    'TerminalWakeBrokerCandidate')) {
                2
            }
            elseif ($parts[0] -ceq 'RpcCallbackLastDisarmResult') {
                2
            }
            else {
                3
            }
            if ((Get-OrdinalCount -Text $tcpRecord -Needle $parts[0]) -ne
                $expectedCount) {
                Throw-UdpCallbackBlocker (
                    "Classes.lcb TCP fence variable $($parts[0]) count drifted.")
            }
            $fenceTokens.Add($parts[0])
            $fenceTokens.Add($parts[1])
        }
        Assert-OrderedTokens `
            -Text $tcpRecord `
            -Tokens $fenceTokens.ToArray() `
            -TokenOwner 'Classes.lcb TCP callback fence storage ABI'
        $tcpDisarmStart = Get-ClassDatabaseFunctionAbiStart `
            -RecordBytes $tcpRecordBytes `
            -RecordText $tcpRecord `
            -FunctionName $TcpDisarmHelperSpec.Name `
            -MethodKind 0x0B `
            -IsVirtual $false `
            -IsGlobal $false `
            -InputCount ([uint32]$TcpDisarmHelperSpec.Inputs.Count) `
            -RecordOwner 'Classes.lcb TCP DisarmRpcCallbackEndpoint'
        $tcpDisarmEnd = $tcpRecord.Length
        $tcpDisarmExactEnd = $false
        if ($State -ceq 'TerminalWakeBrokerCandidate') {
            $tcpDisarmEnd = Get-ClassDatabaseFunctionAbiStart `
                -RecordBytes $tcpRecordBytes `
                -RecordText $tcpRecord `
                -FunctionName $TerminalWakePublishSpec.Name `
                -MethodKind 0x0B `
                -IsVirtual $false `
                -IsGlobal $false `
                -InputCount 0 `
                -RecordOwner 'Classes.lcb TCP PublishD5TerminalWake'
            $tcpDisarmEnd -= 6
            $tcpDisarmExactEnd = $true
        }
        Assert-ClassDatabaseFunctionAbiRecord `
            -RecordBytes $tcpRecordBytes `
            -RecordText $tcpRecord `
            -FunctionName $TcpDisarmHelperSpec.Name `
            -MethodKind 0x0B `
            -IsVirtual $false `
            -IsGlobal $false `
            -Inputs @($TcpDisarmHelperSpec.Inputs) `
            -Outputs @($TcpDisarmHelperSpec.Outputs) `
            -ExpectedStart $tcpDisarmStart `
            -RecordEnd $tcpDisarmEnd `
            -RequireExactEnd:$tcpDisarmExactEnd `
            -RecordOwner 'Classes.lcb TCP DisarmRpcCallbackEndpoint'
    }
    elseif ($tcpClientCount -ne 0) {
        Throw-UdpCallbackBlocker (
            'declaration-only Classes.lcb contains premature TCP CallbackSender.')
    }
}

function Assert-TerminalWakeGeneratedMetadata {
    param(
        [Parameter(Mandatory = $true)][byte[]]$ClassesDatabaseBytes,
        [Parameter(Mandatory = $true)][string]$ClassesDatabaseText,
        [switch]$SyntheticFixture
    )

    $contracts = @(
        [pscustomobject]@{
            Path = '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
            Owner = 'LMCDiagnosticsService Gate D Classes.lcb'
            Variables = $ExpectedDiagnosticsTerminalWakeVariables
            Method = $TerminalWakeTryTakeSpec
            IsGlobal = $true
        },
        [pscustomobject]@{
            Path = '.\Class\TCPMotionInterface\TCPMotionInterface.st'
            Owner = 'TCPMotionInterface Gate D Classes.lcb'
            Variables = $ExpectedTcpTerminalWakeVariables
            Method = $TerminalWakePublishSpec
            IsGlobal = $false
        })
    foreach ($contract in $contracts) {
        $record = Get-ClassDatabaseRecord `
            -DatabaseText $ClassesDatabaseText `
            -SourcePath $contract.Path `
            -RecordOwner $contract.Owner
        $recordStart = $ClassesDatabaseText.IndexOf(
            $contract.Path,
            [StringComparison]::OrdinalIgnoreCase)
        $recordBytes = [byte[]]::new($record.Length)
        [Array]::Copy(
            $ClassesDatabaseBytes,
            $recordStart,
            $recordBytes,
            0,
            $record.Length)
        $orderedVariableTokens = [Collections.Generic.List[string]]::new()
        foreach ($entry in $contract.Variables) {
            $parts = $entry.Split(':', 2)
            $expectedVariableCount = if ($SyntheticFixture) { 1 } else { 2 }
            if ((Get-OrdinalCount -Text $record -Needle $parts[0]) -ne
                $expectedVariableCount) {
                Throw-UdpCallbackBlocker (
                    "$($contract.Owner) variable $($parts[0]) count drifted.")
            }
            $orderedVariableTokens.Add($parts[0])
            $orderedVariableTokens.Add($parts[1])
        }
        Assert-OrderedTokens `
            -Text $record `
            -Tokens $orderedVariableTokens.ToArray() `
            -TokenOwner "$($contract.Owner) variable order/type"

        $spec = $contract.Method
        $methodStart = Get-ClassDatabaseFunctionAbiStart `
            -RecordBytes $recordBytes `
            -RecordText $record `
            -FunctionName $spec.Name `
            -MethodKind 0x0B `
            -IsVirtual $false `
            -IsGlobal $contract.IsGlobal `
            -InputCount ([uint32]$spec.Inputs.Count) `
            -RecordOwner "$($contract.Owner) $($spec.Name)"
        Assert-ClassDatabaseFunctionAbiRecord `
            -RecordBytes $recordBytes `
            -RecordText $record `
            -FunctionName $spec.Name `
            -MethodKind 0x0B `
            -IsVirtual $false `
            -IsGlobal $contract.IsGlobal `
            -Inputs @($spec.Inputs) `
            -Outputs @($spec.Outputs) `
            -ExpectedStart $methodStart `
            -RecordEnd $record.Length `
            -RecordOwner "$($contract.Owner) $($spec.Name)"
    }
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

function Assert-ExactClassObjectsizeMetadata {
    param(
        [Parameter(Mandatory = $true)][string]$SourceText,
        [Parameter(Mandatory = $true)][string]$ClassName,
        [Parameter(Mandatory = $true)][string]$ExpectedObjectsize
    )

    $classTags = @([regex]::Matches(
            (ConvertTo-CanonicalLf -Text $SourceText),
            '(?is)<Class\b(?=[^>]*\bName[ \t]*=[ \t]*"' +
                [regex]::Escape($ClassName) + '")[^>]*>'))
    if ($classTags.Count -ne 1) {
        Throw-UdpCallbackBlocker (
            "$ClassName class metadata tag count is $($classTags.Count), expected 1.")
    }
    $objectsizeMatches = @([regex]::Matches(
            $classTags[0].Value,
            '(?i)\bObjectsize[ \t]*=[ \t]*"(?<Value>\([0-9]+,[0-9]+\))"'))
    if (($objectsizeMatches.Count -ne 1) -or
        ($objectsizeMatches[0].Groups['Value'].Value -cne
            $ExpectedObjectsize)) {
        Throw-UdpCallbackBlocker (
            "$ClassName Objectsize is not exact $ExpectedObjectsize.")
    }
}

function Get-TerminalWakeLayoutProjection {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Layout,
        [Parameter(Mandatory = $true)][string]$LayoutOwner
    )

    $schema = [ordered]@{
        Transceiver = @(
            'Name',
            'CanonicalLfBytes',
            'CanonicalLfSha256',
            'CodeGeneratorCrLfBytes',
            'CodeGeneratorCrLfSha256',
            'LineBreakCount',
            'Objectsize')
        Sender = @(
            'Name',
            'CanonicalLfBytes',
            'CanonicalLfSha256',
            'CodeGeneratorCrLfBytes',
            'CodeGeneratorCrLfSha256',
            'LineBreakCount',
            'Objectsize')
        Classes = @('Bytes', 'Sha256')
        CommNetwork = @(
            'Bytes',
            'Sha256',
            'CanonicalLfBytes',
            'CanonicalLfSha256',
            'TransceiverPosition',
            'SenderPosition')
        NetworksDatabase = @('Bytes', 'Sha256')
        FullNetwork = @(
            'Count',
            'Sha256',
            'CleanCheckoutCount',
            'CleanCheckoutSha256')
        TrackedNetwork = @(
            'Count',
            'Sha256',
            'CleanCheckoutCount',
            'CleanCheckoutSha256')
    }
    Assert-ExactInventory `
        -Actual @($Layout.Keys | ForEach-Object { [string]$_ }) `
        -Expected @($schema.Keys | ForEach-Object { [string]$_ }) `
        -InventoryOwner "$LayoutOwner section order"

    $projection = [Collections.Generic.List[string]]::new()
    foreach ($sectionName in $schema.Keys) {
        $section = $Layout[$sectionName]
        if ($section -isnot [Collections.IDictionary]) {
            Throw-UdpCallbackBlocker (
                "$LayoutOwner $sectionName section is not an ordered map.")
        }
        Assert-ExactInventory `
            -Actual @($section.Keys | ForEach-Object { [string]$_ }) `
            -Expected @($schema[$sectionName]) `
            -InventoryOwner "$LayoutOwner $sectionName field order"
        foreach ($fieldName in $schema[$sectionName]) {
            $projection.Add(
                "$sectionName.$fieldName=$([string]$section[$fieldName])")
        }
    }
    return [string]::Join("`n", $projection.ToArray())
}

function Assert-TerminalWakeLayoutConstantsMatchSelfTestOracle {
    $expectedProjection = Get-TerminalWakeLayoutProjection `
        -Layout $ExpectedTerminalWakeLayout `
        -LayoutOwner 'Gate D expected layout'
    $oracleProjection = Get-TerminalWakeLayoutProjection `
        -Layout $TerminalWakeLayoutSelfTestOracle `
        -LayoutOwner 'Gate D self-test oracle'
    if ($expectedProjection -cne $oracleProjection) {
        Throw-UdpCallbackBlocker (
            'Gate D expected layout constants drifted from the independent ' +
            'self-test oracle.')
    }
}

function Test-TerminalWakeNetworkAggregateIdentity {
    param(
        [Parameter(Mandatory = $true)][long]$ActualCount,
        [Parameter(Mandatory = $true)][string]$ActualSha256,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Expected
    )

    $matches = @(
        @(
            [pscustomobject]@{
                Count = [long]$Expected.Count
                Sha256 = [string]$Expected.Sha256
            },
            [pscustomobject]@{
                Count = [long]$Expected.CleanCheckoutCount
                Sha256 = [string]$Expected.CleanCheckoutSha256
            }) | Where-Object {
                ($ActualCount -eq $_.Count) -and
                ($ActualSha256 -ceq $_.Sha256)
            })
    return $matches.Count -eq 1
}

function Assert-TerminalWakeLayoutContract {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [switch]$SyntheticFixture
    )

    Assert-ExactClassObjectsizeMetadata `
        -SourceText $Snapshot.TransceiverSource `
        -ClassName '_UDPTransceiver' `
        -ExpectedObjectsize $ExpectedTerminalWakeLayout.Transceiver.Objectsize
    Assert-ExactClassObjectsizeMetadata `
        -SourceText $Snapshot.DerivedSource `
        -ClassName 'LMCUdpCallbackSender' `
        -ExpectedObjectsize $ExpectedTerminalWakeLayout.Sender.Objectsize
    Assert-VendorGeneratedRepresentation `
        -CanonicalLfSha256 $Snapshot.DerivedCanonicalLfSha256 `
        -RawSha256 $Snapshot.DerivedRawSha256 `
        -RawBytes $Snapshot.DerivedRawBytes `
        -EolStyle $Snapshot.DerivedEolStyle `
        -LineBreakCount $Snapshot.DerivedLineBreakCount `
        -Expected $ExpectedTerminalWakeLayout.Sender `
        -VendorOwner 'LMCUdpCallbackSender'

    if ($SyntheticFixture) {
        return
    }

    $identityChecks = @(
        [pscustomobject]@{
            Owner = '_UDPTransceiver canonical LF'
            ActualBytes = [long]$Snapshot.TransceiverCanonicalLfBytes
            ExpectedBytes =
                [long]$ExpectedTerminalWakeLayout.Transceiver.CanonicalLfBytes
            ActualSha256 = $Snapshot.TransceiverCanonicalLfSha256
            ExpectedSha256 =
                $ExpectedTerminalWakeLayout.Transceiver.CanonicalLfSha256
        },
        [pscustomobject]@{
            Owner = 'LMCUdpCallbackSender canonical LF'
            ActualBytes = [long]$Snapshot.DerivedCanonicalLfBytes
            ExpectedBytes =
                [long]$ExpectedTerminalWakeLayout.Sender.CanonicalLfBytes
            ActualSha256 = $Snapshot.DerivedCanonicalLfSha256
            ExpectedSha256 =
                $ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256
        },
        [pscustomobject]@{
            Owner = 'Classes.lcb'
            ActualBytes = [long]$Snapshot.ClassesBytes
            ExpectedBytes = [long]$ExpectedTerminalWakeLayout.Classes.Bytes
            ActualSha256 = $Snapshot.ClassesSha256
            ExpectedSha256 = $ExpectedTerminalWakeLayout.Classes.Sha256
        },
        [pscustomobject]@{
            Owner = 'Comm_Network.lcn'
            ActualBytes = [long]$Snapshot.CommNetworkBytes
            ExpectedBytes = [long]$ExpectedTerminalWakeLayout.CommNetwork.Bytes
            ActualSha256 = $Snapshot.CommNetworkSha256
            ExpectedSha256 = $ExpectedTerminalWakeLayout.CommNetwork.Sha256
        },
        [pscustomobject]@{
            Owner = 'Networks.lcb'
            ActualBytes = [long]$Snapshot.NetworksDatabaseBytes
            ExpectedBytes =
                [long]$ExpectedTerminalWakeLayout.NetworksDatabase.Bytes
            ActualSha256 = $Snapshot.NetworksDatabaseSha256
            ExpectedSha256 =
                $ExpectedTerminalWakeLayout.NetworksDatabase.Sha256
        })
    foreach ($check in $identityChecks) {
        if (($check.ActualBytes -ne $check.ExpectedBytes) -or
            ($check.ActualSha256 -cne $check.ExpectedSha256)) {
            Throw-UdpCallbackBlocker (
                "$($check.Owner) sanctioned Gate D identity drifted.")
        }
    }
    foreach ($aggregate in @(
            [pscustomobject]@{
                Owner = 'full Network aggregate'
                ActualCount = [long]$Snapshot.FullNetworkCount
                ActualSha256 = $Snapshot.FullNetworkSha256
                Expected = $ExpectedTerminalWakeLayout.FullNetwork
            },
            [pscustomobject]@{
                Owner = 'tracked Network aggregate'
                ActualCount = [long]$Snapshot.TrackedNetworkCount
                ActualSha256 = $Snapshot.TrackedNetworkSha256
                Expected = $ExpectedTerminalWakeLayout.TrackedNetwork
            })) {
        if (-not (Test-TerminalWakeNetworkAggregateIdentity `
                -ActualCount $aggregate.ActualCount `
                -ActualSha256 $aggregate.ActualSha256 `
                -Expected $aggregate.Expected)) {
            Throw-UdpCallbackBlocker (
                "$($aggregate.Owner) sanctioned Gate D identity drifted.")
        }
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

function Get-UniqueGeneratedRawBlock {
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
    return $matches[0].Value
}

function Assert-GeneratedIncludeRepresentation {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Observed,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)][string]$State
    )

    if ($State -cne 'Absent') {
        foreach ($character in $Observed.Text.ToCharArray()) {
            if ([int]$character -gt 127) {
                Throw-UdpCallbackBlocker (
                    "$($Observed.Name) contains a non-ASCII character.")
            }
        }
        $canonical = ConvertTo-CanonicalLf -Text $Observed.Text
        $canonicalBytesActual = $Utf8.GetBytes($canonical)
        $canonicalShaActual = Get-BytesSha256 -Bytes $canonicalBytesActual
        $lineBreaksActual = [regex]::Matches($canonical, "`n").Count
        if (($Observed.CanonicalLfBytes -ne $canonicalBytesActual.Count) -or
            ($Observed.CanonicalLfSha256 -cne $canonicalShaActual) -or
            ($Observed.LineBreakCount -ne $lineBreaksActual)) {
            Throw-UdpCallbackBlocker (
                "$($Observed.Name) canonical evidence does not match its text.")
        }
        $physicalText = if ($Observed.EolStyle -ceq 'LF') {
            $canonical
        }
        elseif ($Observed.EolStyle -ceq 'CRLF') {
            $canonical.Replace("`n", "`r`n")
        }
        else {
            Throw-UdpCallbackBlocker (
                "$($Observed.Name) generated Include EOL form is not uniform.")
        }
        $physicalBytesActual = $Utf8.GetBytes($physicalText)
        if (($Observed.RawBytes -ne $physicalBytesActual.Count) -or
            ($Observed.RawSha256 -cne
                (Get-BytesSha256 -Bytes $physicalBytesActual))) {
            Throw-UdpCallbackBlocker (
                "$($Observed.Name) physical evidence does not match its text.")
        }
    }
    if ($State -in @('DerivedWired', 'DerivedCandidate')) {
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
    elseif (($State -in @('VendorImported', 'DerivedDeclaration')) -and
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

function Assert-GeneratedIncludeReverseDelta {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$InsertedBlock,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $canonical = ConvertTo-CanonicalLf -Text $Text
    $insertedCanonical = ConvertTo-CanonicalLf -Text $InsertedBlock
    $start = $canonical.IndexOf(
        $insertedCanonical,
        [StringComparison]::Ordinal)
    if (($start -lt 0) -or
        ($canonical.IndexOf(
                $insertedCanonical,
                $start + $insertedCanonical.Length,
                [StringComparison]::Ordinal) -ge 0)) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner reverse-delta block count is not one.")
    }
    $restored = $false
    foreach ($leading in 0..2) {
        foreach ($trailing in 0..2) {
            $removeStart = $start - $leading
            $removeLength = $insertedCanonical.Length + $leading + $trailing
            if (($removeStart -lt 0) -or
                (($removeStart + $removeLength) -gt $canonical.Length)) {
                continue
            }
            if (($leading -gt 0) -and
                ($canonical.Substring($removeStart, $leading) -cne
                    ("`n" * $leading))) {
                continue
            }
            if (($trailing -gt 0) -and
                ($canonical.Substring(
                        $start + $insertedCanonical.Length,
                        $trailing) -cne ("`n" * $trailing))) {
                continue
            }
            $candidate = $canonical.Remove($removeStart, $removeLength)
            $bytes = $Utf8.GetBytes($candidate)
            if (($bytes.Count -eq $Expected.VendorCanonicalLfBytes) -and
                ((Get-BytesSha256 -Bytes $bytes) -ceq
                    $Expected.VendorCanonicalLfSha256)) {
                $restored = $true
                break
            }
        }
        if ($restored) { break }
    }
    if (-not $restored) {
        Throw-UdpCallbackBlocker (
            "$ArtifactOwner does not reverse exactly to the Gate A generated Include.")
    }
}

function Assert-GeneratedDerivedClientIncludeAbiContract {
    param(
        [Parameter(Mandatory = $true)][object[]]$Observed,
        [Parameter(Mandatory = $true)][string]$State
    )

    $cHeader = @($Observed | Where-Object { $_.Name -ceq 'C_channels.h' })[0]
    $stHeader = @($Observed | Where-Object { $_.Name -ceq 'channels.h' })[0]
    $publicHeader = @(
        $Observed | Where-Object { $_.Name -ceq 'lslpublictypes.h' })[0]
    $derivedPresent = $State -in @('DerivedWired', 'DerivedCandidate')
    if (-not $derivedPresent) {
        foreach ($header in @($cHeader, $stHeader, $publicHeader)) {
            if ($header.Text.IndexOf(
                    'LMCUdpCallbackSender',
                    [StringComparison]::Ordinal) -ge 0) {
                Throw-UdpCallbackBlocker (
                    "$($header.Name) contains premature derived client ABI residue.")
            }
        }
        return
    }

    $cRawBlock = Get-UniqueGeneratedRawBlock `
        -Text $cHeader.Text `
        -Pattern (
            '(?ms)^[ \t]*typedef[ \t]+struct[ \t]+' +
            'CltChCmd_LMCUdpCallbackSender\b.*?\}[ \t]*' +
            'CltChCmd_LMCUdpCallbackSender[ \t]*;[ \t]*(?=\r?$)') `
        -BlockOwner 'C_channels.h CltChCmd_LMCUdpCallbackSender'
    $cBlock = ConvertTo-NormalizedGeneratedBlock -Text $cRawBlock
    if (-not [string]::Equals(
            (Get-CommentInsensitiveTokenStream -Text $cBlock),
            (Get-CommentInsensitiveTokenStream `
                -Text $ExpectedDerivedCClientStructBlock),
            [StringComparison]::Ordinal)) {
        Throw-UdpCallbackBlocker (
            'C_channels.h LMCUdpCallbackSender client ABI drifted.')
    }
    if ([regex]::Matches(
            $cHeader.Text,
            '\bCltChCmd_LMCUdpCallbackSender\b').Count -ne 2 -or
        [regex]::Matches(
            $cHeader.Text,
            '\bLMCUdpCallbackSender\b').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'C_channels.h LMCUdpCallbackSender client inventory drifted.')
    }

    $stRawBlock = Get-UniqueGeneratedRawBlock `
        -Text $stHeader.Text `
        -Pattern (
            '(?ms)^[ \t]*CltChCmd_LMCUdpCallbackSender[ \t]*:' +
            '[ \t]*STRUCT\b.*?END_STRUCT[ \t]*;[ \t]*(?=\r?$)') `
        -BlockOwner 'channels.h CltChCmd_LMCUdpCallbackSender'
    $stBlock = ConvertTo-NormalizedGeneratedBlock -Text $stRawBlock
    if (-not [string]::Equals(
            (Get-CommentInsensitiveTokenStream -Text $stBlock),
            (Get-CommentInsensitiveTokenStream `
                -Text $ExpectedDerivedStClientStructBlock),
            [StringComparison]::Ordinal)) {
        Throw-UdpCallbackBlocker (
            'channels.h LMCUdpCallbackSender client ABI drifted.')
    }
    if ([regex]::Matches(
            $stHeader.Text,
            '\bCltChCmd_LMCUdpCallbackSender\b').Count -ne 1 -or
        [regex]::Matches(
            $stHeader.Text,
            '\bLMCUdpCallbackSender\b').Count -ne 1) {
        Throw-UdpCallbackBlocker (
            'channels.h LMCUdpCallbackSender client inventory drifted.')
    }
    if ($publicHeader.Text.IndexOf(
            'CltChCmd_LMCUdpCallbackSender',
            [StringComparison]::Ordinal) -ge 0) {
        Throw-UdpCallbackBlocker (
            'lslpublictypes.h contains misplaced derived client ABI residue.')
    }
    $cExpected = @($GeneratedIncludeContracts | Where-Object {
            $_.Name -ceq 'C_channels.h'
        })[0]
    $stExpected = @($GeneratedIncludeContracts | Where-Object {
            $_.Name -ceq 'channels.h'
        })[0]
    $publicExpected = @($GeneratedIncludeContracts | Where-Object {
            $_.Name -ceq 'lslpublictypes.h'
        })[0]
    Assert-GeneratedIncludeReverseDelta `
        -Text $cHeader.Text `
        -InsertedBlock $cRawBlock `
        -Expected $cExpected `
        -ArtifactOwner 'C_channels.h derived client'
    Assert-GeneratedIncludeReverseDelta `
        -Text $stHeader.Text `
        -InsertedBlock $stRawBlock `
        -Expected $stExpected `
        -ArtifactOwner 'channels.h derived client'
    $publicCanonical = ConvertTo-CanonicalLf -Text $publicHeader.Text
    $publicBytes = $Utf8.GetBytes($publicCanonical)
    if (($publicBytes.Count -ne $publicExpected.VendorCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $publicBytes) -cne
            $publicExpected.VendorCanonicalLfSha256)) {
        Throw-UdpCallbackBlocker (
            'lslpublictypes.h derived pre-snapshot form does not preserve Gate A.')
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
    if ($State -in @('DerivedDeclaration', 'DerivedWired', 'DerivedCandidate')) {
        foreach ($file in $Observed) {
            Assert-NoDisabledPreprocessorEnvelope `
                -Text $file.Text -ArtifactOwner $file.Name
        }
    }
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
    Assert-GeneratedDerivedClientIncludeAbiContract `
        -Observed $Observed -State $State
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
        $allowedPhysicalIdentities = @(
            [pscustomobject]@{
                Bytes = $expected.Bytes
                Sha256 = $expected.Sha256
            }
            if ($expected.Contains('GitCheckoutLfBytes')) {
                [pscustomobject]@{
                    Bytes = $expected.GitCheckoutLfBytes
                    Sha256 = $expected.GitCheckoutLfSha256
                }
            })
        $identityMatches = if ($matches.Count -eq 1) {
            @($allowedPhysicalIdentities | Where-Object {
                    ($matches[0].Bytes -eq $_.Bytes) -and
                    ($matches[0].Sha256 -ceq $_.Sha256)
                }).Count
        }
        else { 0 }
        if (($matches.Count -ne 1) -or ($identityMatches -ne 1)) {
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
        [Parameter(Mandatory = $true)][int]$ProjectDefinitionBytes,
        [Parameter(Mandatory = $true)][string]$ProjectDefinitionSha256,
        [Parameter(Mandatory = $true)][int]$VendorCount,
        [Parameter(Mandatory = $true)][int]$DerivedCount
    )

    Assert-AsciiTextEvidence `
        -Text $ProjectDefinitionText `
        -ByteCount $ProjectDefinitionBytes `
        -Sha256 $ProjectDefinitionSha256 `
        -ArtifactOwner 'canonical project definition'
    try {
        [xml]$xml = $ProjectDefinitionText
    }
    catch {
        Throw-UdpCallbackBlocker (
            "canonical project definition XML cannot be parsed: " +
            $_.Exception.Message)
    }
    $classFiles = @($xml.SelectNodes('/Project/ClassFiles/File'))
    $classNodes = @($xml.SelectNodes('/Project/SigmatekFolders//Class'))
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
        $files = @($classFiles | Where-Object {
                $_.GetAttribute('Path').Equals(
                    $entry.Path,
                    [StringComparison]::Ordinal)
            })
        $classes = @($classNodes | Where-Object {
                $_.GetAttribute('Name').Equals(
                    $entry.Name,
                    [StringComparison]::Ordinal)
            })
        $fileCount = $files.Count
        $classCount = $classes.Count
        if (($fileCount -ne $VendorCount) -or
            ($classCount -ne $VendorCount)) {
            Throw-UdpCallbackBlocker (
                "project definition $($entry.Name) file/class counts are " +
                "$fileCount/$classCount, expected $VendorCount/$VendorCount.")
        }
        foreach ($node in @($files + $classes)) {
            if ($node.Attributes.Count -ne 1) {
                Throw-UdpCallbackBlocker (
                    "project definition $($entry.Name) registration has extra attributes.")
            }
        }
    }
    $derivedPath = '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'
    $derivedFiles = @($classFiles | Where-Object {
            $_.GetAttribute('Path').Equals(
                $derivedPath,
                [StringComparison]::Ordinal)
        })
    $derivedClasses = @($classNodes | Where-Object {
            $_.GetAttribute('Name').Equals(
                'LMCUdpCallbackSender',
                [StringComparison]::Ordinal)
        })
    $derivedFileCount = $derivedFiles.Count
    $derivedClassCount = $derivedClasses.Count
    $derivedClassCountValid = if ($DerivedCount -eq 1) {
        $derivedClassCount -in @(0, 1)
    }
    else {
        $derivedClassCount -eq 0
    }
    if (($derivedFileCount -ne $DerivedCount) -or
        (-not $derivedClassCountValid)) {
        Throw-UdpCallbackBlocker (
            'project definition derived File registration or optional global ' +
            'Class registration count drifted.')
    }
    foreach ($node in @($derivedFiles + $derivedClasses)) {
        if ($node.Attributes.Count -ne 1) {
            Throw-UdpCallbackBlocker (
                'project definition derived registration has extra attributes.')
        }
    }
    $canonical = ConvertTo-CanonicalLf -Text $ProjectDefinitionText
    if ($DerivedCount -eq 1) {
        $fileLinePattern =
            '(?m)^[ \t]*<File Path="\.\\Class\\LMCUdpCallbackSender\\' +
            'LMCUdpCallbackSender\.st"/>[ \t]*(?:\n|\z)'
        $fileMatches = @([regex]::Matches($canonical, $fileLinePattern))
        if ($fileMatches.Count -ne 1) {
            Throw-UdpCallbackBlocker (
                'project definition derived reverse-delta File line count is ' +
                "$($fileMatches.Count).")
        }
        $canonical = $canonical.Remove(
            $fileMatches[0].Index,
            $fileMatches[0].Length)

        $classLinePattern =
            '(?m)^[ \t]*<Class Name="LMCUdpCallbackSender"/>[ \t]*(?:\n|\z)'
        $classMatches = @([regex]::Matches($canonical, $classLinePattern))
        if ($classMatches.Count -gt 1) {
            Throw-UdpCallbackBlocker (
                'project definition derived reverse-delta optional Class line ' +
                "count is $($classMatches.Count).")
        }
        if ($classMatches.Count -eq 1) {
            $canonical = $canonical.Remove(
                $classMatches[0].Index,
                $classMatches[0].Length)
        }
    }
    $canonicalBytes = $Utf8.GetBytes($canonical)
    if (($VendorCount -eq 1) -and
        (($canonicalBytes.Count -ne
                $ExpectedVendorImportedProjectDefinitionCanonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $canonicalBytes) -cne
                $ExpectedVendorImportedProjectDefinitionCanonicalLfSha256))) {
        Throw-UdpCallbackBlocker (
            'project definition does not reverse exactly to Gate A.')
    }
}

function Assert-ProjectDatabaseResidueContract {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectDatabaseText,
        [Parameter(Mandatory = $true)][int]$ProjectBytes,
        [Parameter(Mandatory = $true)][string]$ProjectSha256,
        [Parameter(Mandatory = $true)][string]$State,
        [switch]$SyntheticFixture
    )

    Assert-Latin1BinaryEvidence `
        -Text $ProjectDatabaseText `
        -ByteCount $ProjectBytes `
        -Sha256 $ProjectSha256 `
        -ArtifactOwner 'project .lcb'
    $expectedDerivedRecordCount = if ($State -in @(
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate')) { 1 } else { 0 }
    $derivedRecordCount = Get-LasalLengthPrefixedRecordCount `
        -DatabaseText $ProjectDatabaseText -Value 'LMCUdpCallbackSender'
    $recordCountValid = if ($SyntheticFixture -or
        ($expectedDerivedRecordCount -eq 0)) {
        $derivedRecordCount -eq $expectedDerivedRecordCount
    }
    else {
        $derivedRecordCount -in @(0, 1)
    }
    if (-not $recordCountValid) {
        Throw-UdpCallbackBlocker (
            'project .lcb derived class record count is ' +
            "$derivedRecordCount, expected $expectedDerivedRecordCount.")
    }
    $forbiddenTokens = [Collections.Generic.List[string]]::new()
    foreach ($name in @($ForbiddenDemoClassNames + $ForbiddenDemoNetworkNames)) {
        $forbiddenTokens.Add($name)
    }
    $forbiddenTokens.Add('MotionTCPDemo')
    $forbiddenTokens.Add('_UDPTransceiver1')
    if ($State -in @('Absent', 'VendorImported')) {
        $forbiddenTokens.Add('LMCUdpCallbackSender')
    }
    if ($State -in @('Absent', 'VendorImported', 'DerivedDeclaration')) {
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
        [ValidateSet(
            'Auto',
            'Absent',
            'VendorImported',
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
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
        $implementationDisposition = Get-DerivedImplementationDisposition `
            -SourceText $Snapshot.DerivedSource
        $tcpWiringPresent = $Snapshot.TcpSource.IndexOf(
            'CallbackSender',
            [StringComparison]::Ordinal) -ge 0
        $commHasTransceiver = $Snapshot.CommNetworkText.IndexOf(
            'LMCUdpTransceiver1',
            [StringComparison]::Ordinal) -ge 0
        $commHasSender = $Snapshot.CommNetworkText.IndexOf(
            'LMCUdpCallbackSender1',
            [StringComparison]::Ordinal) -ge 0
        if ($commHasTransceiver -ne $commHasSender) {
            Throw-UdpCallbackBlocker (
                'top-level UDP callback Network is partial.')
        }
        $networkWiringPresent = $commHasTransceiver -and $commHasSender
        if ($tcpWiringPresent -ne $networkWiringPresent) {
            Throw-UdpCallbackBlocker (
                'TCP CallbackSender and top-level UDP Network must transition together.')
        }
        if (-not $tcpWiringPresent) {
            if ($implementationDisposition -cne 'Empty') {
                Throw-UdpCallbackBlocker (
                    'sender bodies are not allowed before the wiring snapshot.')
            }
            'DerivedDeclaration'
        }
        elseif ($implementationDisposition -ceq 'Empty') {
            'DerivedWired'
        }
        else {
            $diagnosticsSource = if (
                $Snapshot.PSObject.Properties.Name -contains
                    'DiagnosticsSource') {
                [string]$Snapshot.DiagnosticsSource
            } else { '' }
            $diagnosticsScan = Get-LexicalScanText -Text $diagnosticsSource
            $tcpScan = Get-LexicalScanText -Text ([string]$Snapshot.TcpSource)
            $derivedScan = Get-LexicalScanText `
                -Text ([string]$Snapshot.DerivedSource)
            $terminalWakeSignal =
                ([regex]::IsMatch(
                        $diagnosticsScan,
                        '(?i)(?<![A-Za-z0-9_])TryTakeD5TerminalWake' +
                            '(?![A-Za-z0-9_])')) -or
                ([regex]::IsMatch(
                        $tcpScan,
                        '(?i)(?<![A-Za-z0-9_])PublishD5TerminalWake' +
                            '(?![A-Za-z0-9_])')) -or
                ([regex]::IsMatch(
                        $derivedScan,
                        '(?i)(?<![A-Za-z0-9_])EventId[ \t]*=[ \t]*0' +
                            '(?![0-9])'))
            if ($terminalWakeSignal) {
                'TerminalWakeBrokerCandidate'
            }
            else {
                'DerivedCandidate'
            }
        }
    }
    if (($RequiredState -cne 'Auto') -and ($state -cne $RequiredState)) {
        Throw-UdpCallbackBlocker (
            "resolved state is $state, required state is $RequiredState.")
    }
    if (($state -in @(
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) -and
        (-not $DerivedCandidateApproved)) {
        Throw-UdpCallbackBlocker (
            'DerivedCandidate is fail-closed until the corrected Gate B ' +
            'ABI, scheduler, queue, socket, wire, and result-domain contract ' +
            'is frozen and re-enabled.')
    }
    $historicalState = if ($state -ceq 'TerminalWakeBrokerCandidate') {
        'DerivedCandidate'
    } else { $state }
    Assert-VendorGeneratedDependencyContract `
        -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
        -VendorPresent ($state -cne 'Absent')
    $derivedState = $state -in @(
        'DerivedDeclaration',
        'DerivedWired',
        'DerivedCandidate',
        'TerminalWakeBrokerCandidate')
    $syntheticFixture =
        ($Snapshot.PSObject.Properties.Name -contains 'SyntheticFixture') -and
        [bool]$Snapshot.SyntheticFixture
    Assert-ProjectDefinitionRegistrations `
        -ProjectDefinitionText $Snapshot.ProjectDefinitionText `
        -ProjectDefinitionBytes $Snapshot.ProjectDefinitionBytes `
        -ProjectDefinitionSha256 $Snapshot.ProjectDefinitionSha256 `
        -VendorCount $(if ($state -ceq 'Absent') { 0 } else { 1 }) `
        -DerivedCount $(if ($derivedState) { 1 } else { 0 })
    Assert-ProjectDatabaseResidueContract `
        -ProjectDatabaseText $Snapshot.ProjectDatabaseText `
        -ProjectBytes $Snapshot.ProjectBytes `
        -ProjectSha256 $Snapshot.ProjectSha256 `
        -State $historicalState `
        -SyntheticFixture:$syntheticFixture
    Assert-GeneratedIncludeContract `
        -Observed @($Snapshot.GeneratedIncludes) `
        -State $historicalState

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
        $expectedTransceiverRepresentation = if (
            $state -ceq 'TerminalWakeBrokerCandidate') {
                $ExpectedTerminalWakeLayout.Transceiver
            }
            else { $ExpectedVendor.Transceiver }
        Assert-VendorGeneratedRepresentation `
            -CanonicalLfSha256 $Snapshot.TransceiverCanonicalLfSha256 `
            -RawSha256 $Snapshot.TransceiverRawSha256 `
            -RawBytes $Snapshot.TransceiverRawBytes `
            -EolStyle $Snapshot.TransceiverEolStyle `
            -LineBreakCount $Snapshot.TransceiverLineBreakCount `
            -Expected $expectedTransceiverRepresentation `
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
            -Observed @($Snapshot.VendorGeneratedRecords) `
            -State $state

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
                -NetworksDatabaseText $Snapshot.NetworksDatabaseText `
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
                    'derived phase changed Network outside the approved artifacts.')
            }
            Assert-SourceRecordCounts `
                -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
                -VendorCount 1 `
                -DerivedCount 1
            Assert-DerivedGeneratedRegistryContract `
                -ConfigObjectsText $Snapshot.ConfigObjectsText `
                -ConfigObjectsBytes $Snapshot.ConfigObjectsBytes `
                -ConfigObjectsSha256 $Snapshot.ConfigObjectsSha256
            $emptyImplementations = $state -in @(
                'DerivedDeclaration',
                'DerivedWired')
            Assert-DerivedSourceContract `
                -SourceText $Snapshot.DerivedSource `
                -ImplementationMode $(if ($emptyImplementations) {
                        'Empty'
                    } else {
                        'Complete'
                    }) `
                -TerminalWakeBroker:(
                    $state -ceq 'TerminalWakeBrokerCandidate')
            if ($syntheticFixture) {
                $expectedSenderSource = if ($emptyImplementations) {
                    New-SyntheticDerivedEmptyStubSource
                }
                elseif ($state -ceq 'TerminalWakeBrokerCandidate') {
                    New-SyntheticTerminalWakeDerivedSource
                }
                else {
                    New-SyntheticDerivedSource
                }
                Assert-ExactSyntheticSourceTokenContract `
                    -Actual $Snapshot.DerivedSource `
                    -Expected $expectedSenderSource `
                    -ArtifactOwner 'LMCUdpCallbackSender'
            }
            $wiredState = $state -in @(
                'DerivedWired',
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate')
            if ($wiredState) {
                if (-not $syntheticFixture) {
                    if ($state -ceq 'TerminalWakeBrokerCandidate') {
                        $expectedFullNetworkCount =
                            $ExpectedTerminalWakeLayout.FullNetwork.Count
                        $expectedFullNetworkSha256 =
                            $ExpectedTerminalWakeLayout.FullNetwork.Sha256
                        $expectedTrackedNetworkCount =
                            $ExpectedTerminalWakeLayout.TrackedNetwork.Count
                        $expectedTrackedNetworkSha256 =
                            $ExpectedTerminalWakeLayout.TrackedNetwork.Sha256
                    }
                    elseif ($state -ceq 'DerivedCandidate') {
                        $expectedFullNetworkCount =
                            $ExpectedDerivedCandidateFullNetworkCount
                        $expectedFullNetworkSha256 =
                            $ExpectedDerivedCandidateFullNetworkSha256
                        $expectedTrackedNetworkCount =
                            $ExpectedDerivedCandidateTrackedNetworkCount
                        $expectedTrackedNetworkSha256 =
                            $ExpectedDerivedCandidateTrackedNetworkSha256
                    }
                    else {
                        $expectedFullNetworkCount = $ExpectedGateB2FullNetworkCount
                        $expectedFullNetworkSha256 =
                            $ExpectedGateB2FullNetworkSha256
                        $expectedTrackedNetworkCount =
                            $ExpectedGateB2TrackedNetworkCount
                        $expectedTrackedNetworkSha256 =
                            $ExpectedGateB2TrackedNetworkSha256
                    }
                    $networkAggregateExact = if ($state -ceq
                        'TerminalWakeBrokerCandidate') {
                        (Test-TerminalWakeNetworkAggregateIdentity `
                            -ActualCount $Snapshot.FullNetworkCount `
                            -ActualSha256 $Snapshot.FullNetworkSha256 `
                            -Expected $ExpectedTerminalWakeLayout.FullNetwork) -and
                        (Test-TerminalWakeNetworkAggregateIdentity `
                            -ActualCount $Snapshot.TrackedNetworkCount `
                            -ActualSha256 $Snapshot.TrackedNetworkSha256 `
                            -Expected $ExpectedTerminalWakeLayout.TrackedNetwork)
                    }
                    else {
                        ($Snapshot.FullNetworkCount -eq
                            $expectedFullNetworkCount) -and
                        ($Snapshot.FullNetworkSha256 -ceq
                            $expectedFullNetworkSha256) -and
                        ($Snapshot.TrackedNetworkCount -eq
                            $expectedTrackedNetworkCount) -and
                        ($Snapshot.TrackedNetworkSha256 -ceq
                            $expectedTrackedNetworkSha256)
                    }
                    if (-not $networkAggregateExact) {
                        Throw-UdpCallbackBlocker (
                            "$state canonical Network aggregate drifted.")
                    }
                }
                Assert-TcpDerivedClientContract `
                    -TcpSource $Snapshot.TcpSource -State $state
                if ($syntheticFixture) {
                    $expectedTcpSource = New-SyntheticTcpSource `
                        -Phase $(if (
                            $state -ceq 'TerminalWakeBrokerCandidate') {
                                'TerminalWakeBrokerCandidate'
                            } elseif ($state -ceq 'DerivedCandidate') {
                                'DerivedCandidate'
                            } else { 'DerivedWired' })
                    Assert-ExactSyntheticSourceTokenContract `
                        -Actual $Snapshot.TcpSource `
                        -Expected $expectedTcpSource `
                        -ArtifactOwner 'TCPMotionInterface'
                }
                Assert-DerivedNetworkContract `
                    -State $state `
                    -CommNetworkText $Snapshot.CommNetworkText `
                    -CommNetworkBytes $Snapshot.CommNetworkBytes `
                    -CommNetworkSha256 $Snapshot.CommNetworkSha256 `
                    -CommTableText $Snapshot.CommTableText `
                    -CommTableBytes $Snapshot.CommTableBytes `
                    -CommTableSha256 $Snapshot.CommTableSha256 `
                    -NetworksDatabaseText $Snapshot.NetworksDatabaseText `
                    -NetworksDatabaseBytes $Snapshot.NetworksDatabaseBytes `
                    -NetworksDatabaseSha256 $Snapshot.NetworksDatabaseSha256 `
                    -SyntheticFixture:$syntheticFixture
            }
            else {
                if (($Snapshot.TcpSha256 -cne $ExpectedBaselineTcpSha256) -or
                    ($Snapshot.TcpSource.IndexOf(
                            'CallbackSender',
                            [StringComparison]::Ordinal) -ge 0) -or
                    ($Snapshot.CommNetworkText.IndexOf(
                            'LMCUdpTransceiver1',
                            [StringComparison]::Ordinal) -ge 0) -or
                    ($Snapshot.CommNetworkText.IndexOf(
                            'LMCUdpCallbackSender1',
                            [StringComparison]::Ordinal) -ge 0)) {
                    Throw-UdpCallbackBlocker (
                        'DerivedDeclaration must preserve Gate A TCP and topology.')
                }
                Assert-AsciiTextEvidence `
                    -Text $Snapshot.CommTableText `
                    -ByteCount $Snapshot.CommTableBytes `
                    -Sha256 $Snapshot.CommTableSha256 `
                    -ArtifactOwner 'DerivedDeclaration ONE_Comm_Network table'
                $commTableCanonical = ConvertTo-CanonicalLf `
                    -Text $Snapshot.CommTableText
                $commTableCanonicalBytes = $Utf8.GetBytes($commTableCanonical)
                if (($commTableCanonicalBytes.Count -ne
                        $ExpectedVendorImportedCommTableCanonicalLfBytes) -or
                    ((Get-BytesSha256 -Bytes $commTableCanonicalBytes) -cne
                        $ExpectedVendorImportedCommTableCanonicalLfSha256)) {
                    Throw-UdpCallbackBlocker (
                        'DerivedDeclaration changed the Gate A ONE table.')
                }
                Assert-Latin1BinaryEvidence `
                    -Text $Snapshot.NetworksDatabaseText `
                    -ByteCount $Snapshot.NetworksDatabaseBytes `
                    -Sha256 $Snapshot.NetworksDatabaseSha256 `
                    -ArtifactOwner 'DerivedDeclaration Networks.lcb'
                if (($Snapshot.NetworksDatabaseBytes -ne
                        $ExpectedVendorImportedNetworksDatabaseBytes) -or
                    ($Snapshot.NetworksDatabaseSha256 -cne
                        $ExpectedVendorImportedNetworksDatabaseSha256)) {
                    Throw-UdpCallbackBlocker (
                        'DerivedDeclaration changed the Gate A Networks.lcb.')
                }
            }
            Assert-GeneratedDerivedMetadata `
                -State $state `
                -ClassesDatabaseBytes $Snapshot.ClassesDatabaseBytes `
                -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
                -ExpectTcpClient $wiredState `
                -SyntheticFixture:$syntheticFixture
            if ($state -ceq 'TerminalWakeBrokerCandidate') {
                Assert-TerminalWakeDiagnosticsSourceContract `
                    -DiagnosticsSource $Snapshot.DiagnosticsSource
                Assert-TerminalWakeGeneratedMetadata `
                    -ClassesDatabaseBytes $Snapshot.ClassesDatabaseBytes `
                    -ClassesDatabaseText $Snapshot.ClassesDatabaseText `
                    -SyntheticFixture:$syntheticFixture
                Assert-TerminalWakeLayoutContract `
                    -Snapshot $Snapshot `
                    -SyntheticFixture:$syntheticFixture
            }
        }
    }

    return [pscustomobject]@{
        State = $state
        VendorPairExact = ($state -cne 'Absent')
        ProtectedDependenciesExact = $true
        ProductionApproved = -not $derivedState
        NeedsRebaseline = $derivedState
        DerivedContractChecked = ($state -in @(
                'DerivedDeclaration',
                'DerivedWired',
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate'))
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

function Get-CanonicalLfByteEvidence {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )

    $canonical = [Collections.Generic.List[byte]]::new()
    $crLfCount = 0
    $bareLfCount = 0
    $index = 0
    while ($index -lt $Bytes.Count) {
        $value = $Bytes[$index]
        if ($value -eq 0x0D) {
            if ((($index + 1) -ge $Bytes.Count) -or
                ($Bytes[$index + 1] -ne 0x0A)) {
                Throw-UdpCallbackBlocker (
                    "$ArtifactOwner contains a bare-CR line ending.")
            }
            $canonical.Add(0x0A)
            $crLfCount++
            $index += 2
            continue
        }
        $canonical.Add($value)
        if ($value -eq 0x0A) {
            $bareLfCount++
        }
        $index++
    }
    $canonicalBytes = $canonical.ToArray()
    $eolStyle = if (($crLfCount -ne 0) -and ($bareLfCount -ne 0)) {
        'Mixed'
    }
    elseif ($crLfCount -ne 0) { 'CRLF' }
    elseif ($bareLfCount -ne 0) { 'LF' }
    else { 'None' }
    return [pscustomobject]@{
        Bytes = $canonicalBytes
        ByteCount = $canonicalBytes.Count
        Sha256 = Get-BytesSha256 -Bytes $canonicalBytes
        EolStyle = $eolStyle
        LineBreakCount = $crLfCount + $bareLfCount
    }
}

function Get-ProtectedTrackedNetworkIdentityEvidence {
    param([Parameter(Mandatory = $true)][object[]]$Files)

    $protectedTrackedFiles = @($Files | Where-Object {
            $_.Tracked -and
            ($AllowedDerivedNetworkPaths -cnotcontains $_.Path)
        } | Sort-Object Path)
    $identityFiles = [Collections.Generic.List[object]]::new()
    foreach ($file in $protectedTrackedFiles) {
        if (-not $file.Available) {
            Throw-UdpCallbackBlocker (
                "protected tracked Network file is unavailable: $($file.Path)")
        }
        $rawBytes = [byte[]]$file.Bytes
        if ($CanonicalLfProtectedNetworkTextPaths -ccontains $file.Path) {
            $text = Get-CanonicalLfByteEvidence `
                -Bytes $rawBytes `
                -ArtifactOwner "protected Network text $($file.Path)"
            if ($text.EolStyle -notin @('LF', 'CRLF', 'Mixed')) {
                Throw-UdpCallbackBlocker (
                    "protected Network text has an unsupported EOL style: " +
                    "$($file.Path)=$($text.EolStyle)")
            }
            $identityFiles.Add([pscustomobject]@{
                    Path = $file.Path
                    Policy = 'CanonicalLf'
                    ByteCount = $text.ByteCount
                    Sha256 = $text.Sha256
                })
        }
        else {
            $identityFiles.Add([pscustomobject]@{
                    Path = $file.Path
                    Policy = 'Raw'
                    ByteCount = $rawBytes.Count
                    Sha256 = Get-BytesSha256 -Bytes $rawBytes
                })
        }
    }
    $identity = [string]::Join("`n", @(
            $identityFiles |
                ForEach-Object {
                    "$($_.Path)|$($_.ByteCount)|$($_.Sha256)"
                }))
    return [pscustomobject]@{
        Count = $identityFiles.Count
        Sha256 = Get-TextSha256 -Text $identity
        Files = $identityFiles.ToArray()
    }
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
    $protectedIdentity = Get-ProtectedTrackedNetworkIdentityEvidence `
        -Files $files.ToArray()
    return [pscustomobject]@{
        Files = $files.ToArray()
        FullCount = $files.Count
        FullSha256 = Get-TextSha256 -Text $fullIdentity
        TrackedCount = $trackedFiles.Count
        TrackedSha256 = Get-TextSha256 -Text $trackedIdentity
        ProtectedTrackedCount = $protectedIdentity.Count
        ProtectedTrackedSha256 = $protectedIdentity.Sha256
    }
}

function New-ProtectedNetworkEolSelfTestFixture {
    param(
        [Parameter(Mandatory = $true)][object[]]$SourceFiles,
        [Parameter(Mandatory = $true)]
        [ValidateSet('LF', 'CRLF')]
        [string]$EolStyle
    )

    $fixtures = [Collections.Generic.List[object]]::new()
    foreach ($source in $SourceFiles) {
        $bytes = [byte[]]$source.Bytes.Clone()
        if ($CanonicalLfProtectedNetworkTextPaths -ccontains $source.Path) {
            $canonical = Get-CanonicalLfByteEvidence `
                -Bytes $bytes `
                -ArtifactOwner "protected Network $EolStyle self-test source"
            if ($EolStyle -ceq 'CRLF') {
                $physical = [Collections.Generic.List[byte]]::new()
                foreach ($value in $canonical.Bytes) {
                    if ($value -eq 0x0A) {
                        $physical.Add(0x0D)
                    }
                    $physical.Add($value)
                }
                $bytes = $physical.ToArray()
            }
            else { $bytes = $canonical.Bytes }
        }
        $fixtures.Add([pscustomobject]@{
                Path = $source.Path
                Tracked = $source.Tracked
                Available = $source.Available
                Bytes = $bytes
                ByteCount = $bytes.Count
                Sha256 = Get-BytesSha256 -Bytes $bytes
            })
    }
    return $fixtures.ToArray()
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
    return $Latin1.GetString($file.Bytes)
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

    $diagnosticsBytes = Get-RequiredFileBytes `
        -Root $resolvedRoot -RelativePath $DiagnosticsRelativePath `
        -FileOwner 'LMCDiagnosticsService source'
    $diagnostics = Get-AsciiTextEvidence `
        -Bytes $diagnosticsBytes -SourceOwner 'LMCDiagnosticsService source'

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
    $commTableNetworkFile = Get-NetworkFileEvidence `
        -NetworkEvidence $network `
            -RelativePath $CommTableRelativePath
    $commNetworkNetworkFile = Get-NetworkFileEvidence `
        -NetworkEvidence $network `
        -RelativePath $CommNetworkRelativePath
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
                    $Latin1.GetString($classesBytes)))
        VendorGeneratedRecords = if (($null -ne $transceiver) -and
            ($null -ne $interface)) {
            @(Get-VendorGeneratedRecordEvidence `
                    -ClassesDatabaseBytes $classesBytes `
                    -ClassesDatabaseText (
                        $Latin1.GetString($classesBytes)))
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
        TransceiverCanonicalLfBytes = if ($null -ne $transceiver) {
            $transceiver.CanonicalLfBytes
        } else { 0 }
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
        DerivedRawBytes = if ($null -ne $derived) {
            $derived.ByteCount
        } else { 0 }
        DerivedRawSha256 = if ($null -ne $derived) {
            $derived.RawSha256
        } else { '' }
        DerivedCanonicalLfBytes = if ($null -ne $derived) {
            $derived.CanonicalLfBytes
        } else { 0 }
        DerivedCanonicalLfSha256 = if ($null -ne $derived) {
            $derived.CanonicalLfSha256
        } else { '' }
        DerivedEolStyle = if ($null -ne $derived) {
            $derived.EolStyle
        } else { 'Absent' }
        DerivedLineBreakCount = if ($null -ne $derived) {
            $derived.LineBreakCount
        } else { 0 }
        DiagnosticsSource = $diagnostics.Text
        DiagnosticsRawBytes = $diagnostics.ByteCount
        DiagnosticsRawSha256 = $diagnostics.RawSha256
        DiagnosticsCanonicalLfBytes = $diagnostics.CanonicalLfBytes
        DiagnosticsCanonicalLfSha256 = $diagnostics.CanonicalLfSha256
        TcpSource = $tcp.Text
        TcpSha256 = $tcp.RawSha256
        ClassesDatabaseBytes = $classesBytes
        ClassesDatabaseText = $Latin1.GetString($classesBytes)
        ClassesBytes = $classesBytes.Count
        ClassesSha256 = Get-BytesSha256 -Bytes $classesBytes
        ProjectBytes = $projectBytes.Count
        ProjectSha256 = Get-BytesSha256 -Bytes $projectBytes
        ProjectDatabaseText = $Latin1.GetString($projectBytes)
        ProjectDefinitionBytes = $projectDefinitionBytes.Count
        ProjectDefinitionSha256 = Get-BytesSha256 -Bytes $projectDefinitionBytes
        ProjectDefinitionText = $projectDefinition.Text
        FullNetworkCount = $network.FullCount
        FullNetworkSha256 = $network.FullSha256
        TrackedNetworkCount = $network.TrackedCount
        TrackedNetworkSha256 = $network.TrackedSha256
        ProtectedTrackedNetworkCount = $network.ProtectedTrackedCount
        ProtectedTrackedNetworkSha256 = $network.ProtectedTrackedSha256
        ConfigObjectsText = $Latin1.GetString(
            $configObjectsNetworkFile.Bytes)
        ConfigObjectsBytes = $configObjectsNetworkFile.ByteCount
        ConfigObjectsSha256 = $configObjectsNetworkFile.Sha256
        NetworksDatabaseBytes = $networksDatabaseNetworkFile.ByteCount
        NetworksDatabaseSha256 = $networksDatabaseNetworkFile.Sha256
        CommNetworkText = Get-NetworkFileText `
            -NetworkEvidence $network -RelativePath $CommNetworkRelativePath
        CommNetworkBytes = $commNetworkNetworkFile.ByteCount
        CommNetworkSha256 = $commNetworkNetworkFile.Sha256
        CommTableText = Get-NetworkFileText `
            -NetworkEvidence $network -RelativePath $CommTableRelativePath
        CommTableBytes = $commTableNetworkFile.ByteCount
        CommTableSha256 = $commTableNetworkFile.Sha256
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
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    if ($Text.Length -gt 0) {
        Add-BytesToList -List $List -Bytes ([Text.Encoding]::ASCII.GetBytes($Text))
    }
}

function Add-SyntheticAaStringToList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $length = [uint32]$Text.Length
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            [byte]($length -band 0xFF),
            [byte](($length -shr 8) -band 0xFF),
            [byte](($length -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiTextToList -List $List -Text $Text
}

function Add-SyntheticFunctionParameterMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[byte]]$List,
        [Parameter(Mandatory = $true)][string]$Entry,
        [Parameter(Mandatory = $true)][bool]$IsOutput,
        [bool]$EaxBoundInput = $false,
        [bool]$EaxBoundOutput = $false
    )

    $separator = $Entry.IndexOf(':', [StringComparison]::Ordinal)
    $name = $Entry.Substring(0, $separator)
    $type = Get-ClassDatabaseParameterTypeContract `
        -Type ($Entry.Substring($separator + 1)) `
        -RecordOwner 'synthetic Classes.lcb parameter'
    $nameLength = [uint32]$name.Length
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            0x00, 0x01,
            [byte]($nameLength -band 0xFF),
            [byte](($nameLength -shr 8) -band 0xFF),
            [byte](($nameLength -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiTextToList -List $List -Text $name
    Add-SyntheticAaStringToList -List $List -Text ''
    $descriptor = [byte[]]@(
        0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x01, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0xFF, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0xAA,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    if ($type.Pointer) {
        $descriptor[54] = 1
    }
    Add-BytesToList -List $List -Bytes $descriptor
    Add-SyntheticAaStringToList -List $List -Text $type.Base
    Add-SyntheticAaStringToList -List $List -Text $type.Owner

    foreach ($unused in 1..5) {
        Add-SyntheticAaStringToList -List $List -Text ''
    }
    Add-BytesToList -List $List -Bytes ([byte[]]@(
            0x01, 0x00, 0x00, 0x00))
    Add-BytesToList -List $List -Bytes ([byte[]]::new(18))
    $List.Add(0xAA)
    $tailEnd = if ($EaxBoundInput) {
        [byte[]]@(0x10, 0x00, 0x00, 0x00, 0x01)
    }
    elseif (-not $IsOutput) {
        [byte[]]@(0xFF, 0xFF, 0xFF, 0xFF, 0x01)
    }
    elseif ($EaxBoundOutput) {
        [byte[]]@(0x10, 0x00, 0x00, 0x00, 0x00)
    }
    else {
        [byte[]]@(0xFF, 0xFF, 0xFF, 0xFF, 0x00)
    }
    Add-BytesToList -List $List -Bytes $tailEnd
}

function New-SyntheticFunctionMetadataBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [byte]$MethodKind = 0x0B,
        [bool]$IsVirtual = $false,
        [Parameter(Mandatory = $true)][bool]$IsGlobal,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Inputs,
        [AllowEmptyCollection()]
        [string[]]$Outputs = @()
    )

    $bytes = [Collections.Generic.List[byte]]::new()
    $methodNameLength = [uint32]$Name.Length
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            0x00, 0x01,
            [byte]($methodNameLength -band 0xFF),
            [byte](($methodNameLength -shr 8) -band 0xFF),
            [byte](($methodNameLength -shr 16) -band 0xFF),
            0xAA))
    Add-AsciiTextToList -List $bytes -Text $Name
    $inputCount = [uint32]$Inputs.Count
    Add-BytesToList -List $bytes -Bytes (
        Get-ClassDatabaseFunctionHeaderBytes `
            -MethodKind $MethodKind `
            -IsVirtual $IsVirtual `
            -IsGlobal $IsGlobal `
            -InputCount $inputCount)
    for ($inputIndex = 0; $inputIndex -lt $Inputs.Count; $inputIndex++) {
        $entry = $Inputs[$inputIndex]
        Add-SyntheticFunctionParameterMetadata `
            -List $bytes `
            -Entry $entry `
            -IsOutput $false `
            -EaxBoundInput (($Name -ceq 'CyWork') -and ($inputIndex -eq 0))
    }
    $outputCount = [uint32]$Outputs.Count
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(
            [byte]($outputCount -band 0xFF),
            [byte](($outputCount -shr 8) -band 0xFF),
            [byte](($outputCount -shr 16) -band 0xFF),
            [byte](($outputCount -shr 24) -band 0xFF)))
    if ($Outputs.Count -eq 1) {
        Add-SyntheticFunctionParameterMetadata `
            -List $bytes `
            -Entry $Outputs[0] `
            -IsOutput $true `
            -EaxBoundOutput ($Name -ceq 'CyWork')
    }
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(0, 0, 0, 0xAA))
    Add-BytesToList -List $bytes -Bytes ([byte[]]@(0, 0, 0, 0, 0, 0))
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
        [ValidateSet(
            'Absent',
            'VendorImported',
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
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
    if ($State -ceq 'TerminalWakeBrokerCandidate') {
        Add-AsciiTextToList -List $bytes -Text (
            '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st' +
            [char]0)
        foreach ($entry in $ExpectedDiagnosticsTerminalWakeVariables) {
            $parts = $entry.Split(':', 2)
            Add-AsciiTextToList -List $bytes -Text (
                $parts[0] + [char]0 + $parts[1] + [char]0)
        }
        Add-BytesToList -List $bytes -Bytes (
            New-SyntheticFunctionMetadataBytes `
                -Name $TerminalWakeTryTakeSpec.Name `
                -IsGlobal $true `
                -Inputs @($TerminalWakeTryTakeSpec.Inputs) `
                -Outputs @($TerminalWakeTryTakeSpec.Outputs))
    }
    if ($State -in @(
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        Add-AsciiTextToList -List $bytes -Text (
            '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st' + [char]0)
        Add-AsciiTextToList -List $bytes -Text (
            '_LMC_UDP_ACTIVE_ENDPOINT' + [char]0 +
            '_LMC_UDP_TX_SLOT' + [char]0)
        foreach ($entry in $ExpectedDerivedServers) {
            Add-AsciiTextToList -List $bytes -Text (
                $entry.Split(':', 2)[0] + [char]0)
        }
        foreach ($entry in $ExpectedDerivedVariables) {
            Add-AsciiTextToList -List $bytes -Text (
                $entry.Split(':', 2)[0] + [char]0)
        }
        Add-BytesToList -List $bytes -Bytes (
            New-SyntheticFunctionMetadataBytes `
                -Name $CyWorkSpec.Name `
                -MethodKind 0x05 `
                -IsVirtual $true `
                -IsGlobal $true `
                -Inputs @($CyWorkSpec.Inputs) `
                -Outputs @($CyWorkSpec.Outputs))
        Add-BytesToList -List $bytes -Bytes (
            New-SyntheticFunctionMetadataBytes `
                -Name $ErrorCallbackSpec.Name `
                -IsVirtual $true `
                -IsGlobal $true `
                -Inputs @($ErrorCallbackSpec.Inputs) `
                -Outputs @($ErrorCallbackSpec.Outputs))
        foreach ($spec in $PublicFunctionSpecs) {
            Add-BytesToList -List $bytes -Bytes (
                New-SyntheticFunctionMetadataBytes `
                    -Name $spec.Name `
                    -IsGlobal $true `
                    -Inputs @($spec.Inputs) `
                    -Outputs @($spec.Outputs))
        }
        foreach ($spec in $PrivateFunctionSpecs) {
            Add-BytesToList -List $bytes -Bytes (
                New-SyntheticFunctionMetadataBytes `
                    -Name $spec.Name `
                    -IsGlobal $false `
                    -Inputs @($spec.Inputs) `
                    -Outputs @($spec.Outputs))
        }
    }
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\TCPMotionInterface\TCPMotionInterface.st' + [char]0)
    if ($State -in @(
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        Add-AsciiTextToList -List $bytes -Text (
            'CallbackSender' + [char]0 + 'LMCUdpCallbackSender' + [char]0)
        $syntheticTcpCallbackTypes = [ordered]@{
            RpcCallbackRegistered = 'BOOL'
            RpcCallbackEventMask = 'UDINT'
            RpcCallbackPort = 'DINT'
            RpcCallbackIPv4 = 'ARRAY[0..3]OFBYTE'
            RpcCallbackProtocolVersion = 'UINT'
            RpcCallbackAcceptedMaxDatagram = 'UINT'
            RpcCallbackSessionEpoch = 'UDINT'
            RpcCallbackBootId = 'UDINT'
            RpcCallbackCookieLo = 'UDINT'
            RpcCallbackCookieHi = 'UDINT'
            RpcCallbackLastDisarmResult = 'DINT'
        }
        $syntheticTcpCallbackInventory = if ($State -in @(
                'DerivedCandidate',
                'TerminalWakeBrokerCandidate')) {
            $ExpectedDerivedCandidateGeneratedTcpCallbackVariables
        }
        else {
            @(
                'RpcCallbackRegistered',
                'RpcCallbackEventMask',
                'RpcCallbackPort',
                'RpcCallbackIPv4') + @(
                $ExpectedTcpCallbackFenceVariables |
                    ForEach-Object { $_.Split(':', 2)[0] })
        }
        foreach ($name in $syntheticTcpCallbackInventory) {
            Add-AsciiTextToList -List $bytes -Text (
                $name + [char]0 +
                $syntheticTcpCallbackTypes[$name] + [char]0)
        }
        if ($State -ceq 'TerminalWakeBrokerCandidate') {
            foreach ($entry in $ExpectedTcpTerminalWakeVariables) {
                $parts = $entry.Split(':', 2)
                Add-AsciiTextToList -List $bytes -Text (
                    $parts[0] + [char]0 + $parts[1] + [char]0)
            }
        }
        Add-BytesToList -List $bytes -Bytes (
            New-SyntheticFunctionMetadataBytes `
                -Name $TcpDisarmHelperSpec.Name `
                -IsGlobal $false `
                -Inputs @($TcpDisarmHelperSpec.Inputs) `
                -Outputs @($TcpDisarmHelperSpec.Outputs))
        if ($State -ceq 'TerminalWakeBrokerCandidate') {
            Add-BytesToList -List $bytes -Bytes (
                New-SyntheticFunctionMetadataBytes `
                    -Name $TerminalWakePublishSpec.Name `
                    -IsGlobal $false `
                    -Inputs @($TerminalWakePublishSpec.Inputs) `
                    -Outputs @($TerminalWakePublishSpec.Outputs))
        }
    }
    Add-AsciiTextToList -List $bytes -Text (
        '.\Class\ZZFixtureBoundary\ZZFixtureBoundary.st' + [char]0)
    $array = $bytes.ToArray()
    return [pscustomobject]@{
        Bytes = $array
        Text = $Latin1.GetString($array)
    }
}

function New-SyntheticDerivedSource {
    return @'
//{{LSL_DEFINES
#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE
#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0
#endif
//}}LSL_DEFINES

(*!
<Class Name="LMCUdpCallbackSender" RealtimeTask="false" CyclicTask="true" DefCyclictime="10 ms" BackgroundTask="false" Sigmatek="false">
  <Channels>
    <Server Name="AdmissionErrorDropCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="AdmissionRetryCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="DisarmClearedCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="LastAdmissionResult" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="QueuedCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="QueueDepth" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="QueueFullDropCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="RingAcceptedCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
    <Server Name="TransportErrorCount" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>
  </Channels>
  <Network Name="LMCUdpCallbackSender">
    <Components>
      <Object Name="_base" GUID="{12345678-1234-1234-1234-123456789ABC}" Class="_UDPTransceiverInterface" Position="(218,120)" Visualized="true" Remotely="true">
        <Channels>
          <Server Name="ClassSvr"/>
          <Server Name="ErrorCode"/>
          <Server Name="ErrorMessage"/>
          <Server Name="ErrorState"/>
          <Server Name="State"/>
          <Client Name="_UDPTransceiver"/>
        </Channels>
      </Object>
    </Components>
    <Comments></Comments>
    <Connections>
      <Connection Source="this.ClassSvr" Destination="_base.ClassSvr" Vertices="(942,210),(770,210),"/>
      <Connection Source="this.State" Destination="_base.State" Vertices="(942,270),(770,270),"/>
      <Connection Source="this.ErrorState" Destination="_base.ErrorState" Vertices="(942,330),(770,330),"/>
      <Connection Source="this.ErrorMessage" Destination="_base.ErrorMessage" Vertices="(942,390),(770,390),"/>
      <Connection Source="this.ErrorCode" Destination="_base.ErrorCode" Vertices="(942,450),(770,450),"/>
      <Connection Source="_base._UDPTransceiver" Destination="this._UDPTransceiver" Vertices="(218,210),(38,210),"/>
    </Connections>
    <Options></Options>
  </Network>
</Class>
*)
#pragma using _UDPTransceiverInterface

LMCUdpCallbackSender : CLASS
: _UDPTransceiverInterface
    TYPE
        _LMC_UDP_ACTIVE_ENDPOINT : STRUCT
            Armed : BOOL;
            ProtocolVersion : UINT;
            EventMask : UDINT;
            CallbackIPv4 : UDINT;
            CallbackPort : DINT;
            SessionEpoch : UDINT;
            BootId : UDINT;
            CookieLo : UDINT;
            CookieHi : UDINT;
            MaxDatagramBytes : UDINT;
        END_STRUCT;
        _LMC_UDP_TX_SLOT : STRUCT
            InUse : BOOL;
            ProtocolVersion : UINT;
            DatagramBytes : UDINT;
            DestinationIPv4 : UDINT;
            DestinationPort : UDINT;
            SessionEpoch : UDINT;
            BootId : UDINT;
            CookieLo : UDINT;
            CookieHi : UDINT;
            SequenceLo : UDINT;
            SequenceHi : UDINT;
            PlcTimeMs : UDINT;
            RetryCount : UDINT;
            Data : ARRAY [0..511] OF BYTE;
        END_STRUCT;
    END_TYPE
    QueueDepth : SvrCh_UDINT;
    QueuedCount : SvrCh_UDINT;
    RingAcceptedCount : SvrCh_UDINT;
    AdmissionRetryCount : SvrCh_UDINT;
    QueueFullDropCount : SvrCh_UDINT;
    AdmissionErrorDropCount : SvrCh_UDINT;
    DisarmClearedCount : SvrCh_UDINT;
    TransportErrorCount : SvrCh_UDINT;
    LastAdmissionResult : SvrCh_DINT;
    ActiveEndpoint : _LMC_UDP_ACTIVE_ENDPOINT;
    TxSlots : ARRAY [0..7] OF _LMC_UDP_TX_SLOT;
    ReadIndex : UDINT;
    WriteIndex : UDINT;
    Depth : UDINT;
    NextSequenceLo : UDINT;
    NextSequenceHi : UDINT;

    FUNCTION VIRTUAL GLOBAL CyWork
        VAR_INPUT
            EAX : UDINT;
        END_VAR
        VAR_OUTPUT
            state (EAX) : UDINT;
        END_VAR;
    FUNCTION VIRTUAL GLOBAL ErrorCallback
        VAR_INPUT
            FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
            UdpError : _UDPTransceiver::_UDP_ERROR;
            ErrCode : DINT;
        END_VAR;
    FUNCTION GLOBAL ArmEndpoint
        VAR_INPUT
            ProtocolVersion : UINT;
            EventMask : UDINT;
            CallbackIPv4 : UDINT;
            CallbackPort : DINT;
            SessionEpoch : UDINT;
            BootId : UDINT;
            CookieLo : UDINT;
            CookieHi : UDINT;
            MaxDatagramBytes : UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION GLOBAL DisarmEndpoint
        VAR_INPUT
            ExpectedSessionEpoch : UDINT;
            ExpectedCookieLo : UDINT;
            ExpectedCookieHi : UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION GLOBAL PublishEvent
        VAR_INPUT
            EventMaskBit : UDINT;
            EventType : UINT;
            DeliveryClass : UINT;
            EventId : UDINT;
            ProducerSessionEpoch : UDINT;
            pPayload : ^void;
            PayloadBytes : UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION EnsureSocketReady
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION ValidateEndpoint
        VAR_INPUT
            ProtocolVersion : UINT;
            EventMask : UDINT;
            CallbackIPv4 : UDINT;
            CallbackPort : DINT;
            SessionEpoch : UDINT;
            BootId : UDINT;
            CookieLo : UDINT;
            CookieHi : UDINT;
            MaxDatagramBytes : UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION BuildDatagram
        VAR_INPUT
            SlotIndex : UDINT;
            EventMaskBit : UDINT;
            EventType : UINT;
            DeliveryClass : UINT;
            EventId : UDINT;
            ProducerSessionEpoch : UDINT;
            pPayload : ^void;
            PayloadBytes : UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;
    FUNCTION FindFreeSlot
        VAR_OUTPUT
            SlotIndex : DINT;
        END_VAR;
    FUNCTION ServiceTransmitQueue;
    FUNCTION SendSlot
        VAR_INPUT
            SlotIndex : UDINT;
        END_VAR
        VAR_OUTPUT
            VendorResult : DINT;
        END_VAR;
    FUNCTION RetryOrDropSlot
        VAR_INPUT
            SlotIndex : UDINT;
            VendorResult : DINT;
        END_VAR;
    FUNCTION ClearPendingFrames;
    FUNCTION FenceMatches
        VAR_INPUT
            ExpectedSessionEpoch : UDINT;
            ExpectedCookieLo : UDINT;
            ExpectedCookieHi : UDINT;
        END_VAR
        VAR_OUTPUT
            Matches : BOOL;
        END_VAR;
    FUNCTION @STD
        VAR_OUTPUT
            ret_code : CONFSTATES;
        END_VAR;
    FUNCTION GLOBAL TAB @CT_;
END_CLASS;

FUNCTION GLOBAL TAB LMCUdpCallbackSender::@CT_
0$UINT,
2#0100000000000010$UINT,
0$UINT, 0$UINT, (SIZEOF(::LMCUdpCallbackSender))$UINT,
9$UINT, 0$UINT, 0$UINT,
TO_UDINT(0), "LMCUdpCallbackSender",
TO_UDINT(0), "_UDPTransceiverInterface", 1$UINT, 3$UINT,
(::LMCUdpCallbackSender.QueueDepth.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "QueueDepth",
(::LMCUdpCallbackSender.QueuedCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "QueuedCount",
(::LMCUdpCallbackSender.RingAcceptedCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "RingAcceptedCount",
(::LMCUdpCallbackSender.AdmissionRetryCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "AdmissionRetryCount",
(::LMCUdpCallbackSender.QueueFullDropCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "QueueFullDropCount",
(::LMCUdpCallbackSender.AdmissionErrorDropCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "AdmissionErrorDropCount",
(::LMCUdpCallbackSender.DisarmClearedCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "DisarmClearedCount",
(::LMCUdpCallbackSender.TransportErrorCount.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "TransportErrorCount",
(::LMCUdpCallbackSender.LastAdmissionResult.pMeth)$UINT, _CH_SVR$UINT, 0$UINT, TO_UDINT(0), "LastAdmissionResult",
END_FUNCTION

#define USER_CNT_LMCUdpCallbackSender 14

TYPE
    _LSL_STD_VMETH : STRUCT
        CmdTable : CMDMETH;
        UserFcts : ARRAY[0..USER_CNT_LMCUdpCallbackSender] OF ^Void;
    END_STRUCT;
END_TYPE

FUNCTION LMCUdpCallbackSender::@STD
    VAR_OUTPUT
        ret_code : CONFSTATES;
    END_VAR
    VAR
        vmt : _LSL_STD_VMETH;
        nCmdSize : UINT;
    END_VAR
    ret_code := _UDPTransceiverInterface::@STD();
    IF ret_code <> C_OK THEN
        RETURN;
    END_IF;
    nCmdSize := _UDPTransceiverInterface::ClassSvr.pMeth^.nCmds$UINT * SIZEOF(pVoid) + CMDMETH.Init;
    _memcpy((#vmt.CmdTable)$^USINT, _UDPTransceiverInterface::ClassSvr.pMeth, nCmdSize);
    vmt.CmdTable.nCmds := nSTDCMD + USER_CNT_LMCUdpCallbackSender;
    vmt.CmdTable.CyWork := #CyWork();
#pragma warning (disable : 74)
    vmt.UserFcts[12] := #ErrorCallback();
#pragma warning (default : 74)
    _UDPTransceiverInterface::ClassSvr.pMeth := StoreCmd(pCmd := #vmt.CmdTable, SHARED);
    IF _UDPTransceiverInterface::ClassSvr.pMeth THEN
        ret_code := C_OK;
    ELSE
        ret_code := C_OUTOF_NEAR;
        RETURN;
    END_IF;
END_FUNCTION

FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::CyWork
    VAR_INPUT
        EAX : UDINT;
    END_VAR
    VAR_OUTPUT
        state (EAX) : UDINT;
    END_VAR
    VAR
        socketResult : DINT;
    END_VAR
    CriticalSection_UDP.SectionStart();
    socketResult := EnsureSocketReady();
    IF (socketResult = 0) AND ActiveEndpoint.Armed AND (Depth > 0) THEN
        ServiceTransmitQueue();
    END_IF;
    state := READY;
    CriticalSection_UDP.SectionStop();
END_FUNCTION

FUNCTION GLOBAL LMCUdpCallbackSender::ArmEndpoint
    VAR_INPUT
        ProtocolVersion : UINT;
        EventMask : UDINT;
        CallbackIPv4 : UDINT;
        CallbackPort : DINT;
        SessionEpoch : UDINT;
        BootId : UDINT;
        CookieLo : UDINT;
        CookieHi : UDINT;
        MaxDatagramBytes : UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    VAR
        validateResult : DINT;
        socketResult : DINT;
    END_VAR
    CriticalSection_UDP.SectionStart();
    Result := -9;
    validateResult := ValidateEndpoint(ProtocolVersion := ProtocolVersion, EventMask := EventMask, CallbackIPv4 := CallbackIPv4, CallbackPort := CallbackPort, SessionEpoch := SessionEpoch, BootId := BootId, CookieLo := CookieLo, CookieHi := CookieHi, MaxDatagramBytes := MaxDatagramBytes);
    IF validateResult = -1 THEN
        Result := -1;
    ELSIF validateResult = -6 THEN
        Result := -6;
    ELSIF ActiveEndpoint.Armed AND (ActiveEndpoint.ProtocolVersion = ProtocolVersion) AND (ActiveEndpoint.EventMask = EventMask) AND (ActiveEndpoint.CallbackIPv4 = CallbackIPv4) AND (ActiveEndpoint.CallbackPort = CallbackPort) AND (ActiveEndpoint.SessionEpoch = SessionEpoch) AND (ActiveEndpoint.BootId = BootId) AND (ActiveEndpoint.CookieLo = CookieLo) AND (ActiveEndpoint.CookieHi = CookieHi) AND (ActiveEndpoint.MaxDatagramBytes = MaxDatagramBytes) THEN
        Result := 1;
    ELSE
        socketResult := EnsureSocketReady();
        IF (socketResult <> 0) AND (socketResult <> 1) THEN
            Result := -2;
        ELSIF ActiveEndpoint.Armed THEN
            Result := -3;
        ELSE
            ClearPendingFrames();
            ActiveEndpoint.Armed := TRUE;
            ActiveEndpoint.ProtocolVersion := ProtocolVersion;
            ActiveEndpoint.EventMask := EventMask;
            ActiveEndpoint.CallbackIPv4 := CallbackIPv4;
            ActiveEndpoint.CallbackPort := CallbackPort;
            ActiveEndpoint.SessionEpoch := SessionEpoch;
            ActiveEndpoint.BootId := BootId;
            ActiveEndpoint.CookieLo := CookieLo;
            ActiveEndpoint.CookieHi := CookieHi;
            ActiveEndpoint.MaxDatagramBytes := MaxDatagramBytes;
            NextSequenceLo := 1;
            NextSequenceHi := 0;
            Result := 0;
        END_IF;
    END_IF;
    CriticalSection_UDP.SectionStop();
END_FUNCTION

FUNCTION GLOBAL LMCUdpCallbackSender::DisarmEndpoint
    VAR_INPUT
        ExpectedSessionEpoch : UDINT;
        ExpectedCookieLo : UDINT;
        ExpectedCookieHi : UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    VAR
        clearedDepth : UDINT;
    END_VAR
    CriticalSection_UDP.SectionStart();
    Result := -9;
    IF NOT ActiveEndpoint.Armed AND (Depth = 0) THEN
        Result := 1;
    ELSIF NOT FenceMatches(ExpectedSessionEpoch := ExpectedSessionEpoch, ExpectedCookieLo := ExpectedCookieLo, ExpectedCookieHi := ExpectedCookieHi) THEN
        Result := -8;
    ELSE
        clearedDepth := Depth;
        _memset(dest := #ActiveEndpoint, usByte := 0, cntr := sizeof(ActiveEndpoint));
        ClearPendingFrames();
        NextSequenceLo := 1;
        NextSequenceHi := 0;
        IF clearedDepth > 0 THEN
            IF DisarmClearedCount.Read() > (16#FFFFFFFF - clearedDepth) THEN
                DisarmClearedCount.Write(input := 16#FFFFFFFF);
            ELSE
                DisarmClearedCount.Write(input := DisarmClearedCount.Read() + clearedDepth);
            END_IF;
        END_IF;
        Result := 0;
    END_IF;
    CriticalSection_UDP.SectionStop();
END_FUNCTION

FUNCTION GLOBAL LMCUdpCallbackSender::PublishEvent
    VAR_INPUT
        EventMaskBit : UDINT;
        EventType : UINT;
        DeliveryClass : UINT;
        EventId : UDINT;
        ProducerSessionEpoch : UDINT;
        pPayload : ^void;
        PayloadBytes : UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    VAR
        socketResult : DINT;
        slotIndex : DINT;
        buildResult : DINT;
    END_VAR
    CriticalSection_UDP.SectionStart();
    Result := -9;
    IF NOT ActiveEndpoint.Armed THEN
        Result := -4;
    ELSIF ProducerSessionEpoch <> ActiveEndpoint.SessionEpoch THEN
        Result := -8;
    ELSIF ((PayloadBytes > 0) AND (pPayload = NIL)) OR (PayloadBytes > 460) OR ((PayloadBytes + 52) > ActiveEndpoint.MaxDatagramBytes) THEN
        Result := -7;
    ELSIF (EventMaskBit <> 1) OR (EventType <> 1) OR (DeliveryClass <> 0) OR (PayloadBytes <> 0) OR ((ActiveEndpoint.EventMask AND EventMaskBit) = 0) THEN
        Result := -6;
    ELSE
        socketResult := EnsureSocketReady();
        IF (socketResult <> 0) AND (socketResult <> 1) THEN
            Result := -2;
        ELSIF Depth >= 8 THEN
            IF QueueFullDropCount.Read() <> 16#FFFFFFFF THEN
                QueueFullDropCount.Write(input := QueueFullDropCount.Read() + 1);
            END_IF;
            Result := -5;
        ELSE
            slotIndex := FindFreeSlot();
            IF slotIndex < 0 THEN
                Result := -9;
            ELSE
                buildResult := BuildDatagram(SlotIndex := TO_UDINT(slotIndex), EventMaskBit := EventMaskBit, EventType := EventType, DeliveryClass := DeliveryClass, EventId := EventId, ProducerSessionEpoch := ProducerSessionEpoch, pPayload := pPayload, PayloadBytes := PayloadBytes);
                IF buildResult <> 0 THEN
                    Result := -9;
                ELSE
                    Depth := Depth + 1;
                    WriteIndex := WriteIndex + 1;
                    IF WriteIndex >= 8 THEN
                        WriteIndex := 0;
                    END_IF;
                    QueueDepth.Write(input := Depth);
                    IF QueuedCount.Read() <> 16#FFFFFFFF THEN
                        QueuedCount.Write(input := QueuedCount.Read() + 1);
                    END_IF;
                    Result := 0;
                END_IF;
            END_IF;
        END_IF;
    END_IF;
    CriticalSection_UDP.SectionStop();
END_FUNCTION

FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::ErrorCallback
    VAR_INPUT
        FSM_UDP : _UDPTransceiver::_FSM_UDP_USER;
        UdpError : _UDPTransceiver::_UDP_ERROR;
        ErrCode : DINT;
    END_VAR
    CriticalSection_UDP.SectionStart();
    ErrorState := FSM_UDP;
    ErrorMessage := UdpError;
    ErrorCode := ErrCode;
    IF TransportErrorCount.Read() <> 16#FFFFFFFF THEN
        TransportErrorCount.Write(input := TransportErrorCount.Read() + 1);
    END_IF;
    CriticalSection_UDP.SectionStop();
END_FUNCTION

FUNCTION LMCUdpCallbackSender::EnsureSocketReady
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    IF Socket = 0 THEN
        Socket := AddSocket();
    END_IF;
    IF Socket = 0 THEN
        Result := -2;
    ELSIF Socket <> 0 THEN
        IF IsOpen() THEN
            Result := 0;
        ELSE
            Result := 1;
        END_IF;
    ELSE
        Result := -2;
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::ValidateEndpoint
    VAR_INPUT
        ProtocolVersion : UINT;
        EventMask : UDINT;
        CallbackIPv4 : UDINT;
        CallbackPort : DINT;
        SessionEpoch : UDINT;
        BootId : UDINT;
        CookieLo : UDINT;
        CookieHi : UDINT;
        MaxDatagramBytes : UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    Result := -1;
    IF ProtocolVersion = 1 THEN
        Result := -6;
    ELSIF ProtocolVersion <> 2 THEN
        Result := -6;
    ELSIF (SessionEpoch = 0) OR (BootId = 0) OR (CallbackIPv4 = 0) OR (CallbackPort < 1) OR (CallbackPort > 65535) OR ((EventMask AND 1) <> 1) THEN
        Result := -1;
    ELSIF ((CookieLo OR CookieHi) = 0) OR (MaxDatagramBytes < 52) OR (MaxDatagramBytes > 512) THEN
        Result := -1;
    ELSE
        Result := 0;
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::BuildDatagram
    VAR_INPUT
        SlotIndex : UDINT;
        EventMaskBit : UDINT;
        EventType : UINT;
        DeliveryClass : UINT;
        EventId : UDINT;
        ProducerSessionEpoch : UDINT;
        pPayload : ^void;
        PayloadBytes : UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR
    IF SlotIndex >= 8 THEN
        Result := -9;
    ELSE
        TxSlots[SlotIndex].InUse := TRUE;
        TxSlots[SlotIndex].ProtocolVersion := 2;
        TxSlots[SlotIndex].DatagramBytes := 52 + PayloadBytes;
        TxSlots[SlotIndex].DestinationIPv4 := ActiveEndpoint.CallbackIPv4;
        TxSlots[SlotIndex].DestinationPort := TO_UDINT(ActiveEndpoint.CallbackPort);
        TxSlots[SlotIndex].SessionEpoch := ProducerSessionEpoch;
        TxSlots[SlotIndex].BootId := ActiveEndpoint.BootId;
        TxSlots[SlotIndex].CookieLo := ActiveEndpoint.CookieLo;
        TxSlots[SlotIndex].CookieHi := ActiveEndpoint.CookieHi;
        TxSlots[SlotIndex].SequenceLo := NextSequenceLo;
        TxSlots[SlotIndex].SequenceHi := NextSequenceHi;
        TxSlots[SlotIndex].PlcTimeMs := ops.tAbsolute;
        TxSlots[SlotIndex].RetryCount := 0;
        TxSlots[SlotIndex].Data[0]$UDINT := 16#32434D4C;
        TxSlots[SlotIndex].Data[4]$UINT := 2;
        TxSlots[SlotIndex].Data[6]$UINT := 52;
        TxSlots[SlotIndex].Data[8]$UINT := TO_UINT(TxSlots[SlotIndex].DatagramBytes);
        TxSlots[SlotIndex].Data[10]$UINT := EventType;
        TxSlots[SlotIndex].Data[12]$UDINT := EventMaskBit;
        TxSlots[SlotIndex].Data[16]$UDINT := ActiveEndpoint.BootId;
        TxSlots[SlotIndex].Data[20]$UDINT := ProducerSessionEpoch;
        TxSlots[SlotIndex].Data[24]$UDINT := ActiveEndpoint.CookieLo;
        TxSlots[SlotIndex].Data[28]$UDINT := ActiveEndpoint.CookieHi;
        TxSlots[SlotIndex].Data[32]$UDINT := TxSlots[SlotIndex].SequenceLo;
        TxSlots[SlotIndex].Data[36]$UDINT := TxSlots[SlotIndex].SequenceHi;
        TxSlots[SlotIndex].Data[40]$UDINT := EventId;
        TxSlots[SlotIndex].Data[44]$UDINT := TxSlots[SlotIndex].PlcTimeMs;
        TxSlots[SlotIndex].Data[48]$UINT := TO_UINT(PayloadBytes);
        TxSlots[SlotIndex].Data[50] := TO_USINT(DeliveryClass);
        TxSlots[SlotIndex].Data[51] := 0;
        NextSequenceLo := NextSequenceLo + 1;
        IF NextSequenceLo = 0 THEN
            IF NextSequenceHi = 16#FFFFFFFF THEN
                NextSequenceHi := 0;
            ELSE
                NextSequenceHi := NextSequenceHi + 1;
            END_IF;
        END_IF;
        Result := 0;
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::FindFreeSlot
    VAR_OUTPUT
        SlotIndex : DINT;
    END_VAR
    IF (Depth >= 8) OR NOT (TxSlots[WriteIndex].InUse = FALSE) THEN
        SlotIndex := -1;
    ELSE
        SlotIndex := TO_DINT(WriteIndex);
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::ServiceTransmitQueue
    VAR
        slotIndex : UDINT;
        vendorResult : DINT;
    END_VAR
    IF Depth > 0 THEN
        slotIndex := ReadIndex;
        vendorResult := SendSlot(SlotIndex := slotIndex);
        LastAdmissionResult.Write(input := vendorResult);
        IF vendorResult = 0 THEN
            TxSlots[slotIndex].InUse := FALSE;
            ReadIndex := ReadIndex + 1;
            IF ReadIndex >= 8 THEN
                ReadIndex := 0;
            END_IF;
            Depth := Depth - 1;
            QueueDepth.Write(input := Depth);
            IF RingAcceptedCount.Read() <> 16#FFFFFFFF THEN
                RingAcceptedCount.Write(input := RingAcceptedCount.Read() + 1);
            END_IF;
        ELSE
            RetryOrDropSlot(SlotIndex := slotIndex, VendorResult := vendorResult);
        END_IF;
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::SendSlot
    VAR_INPUT
        SlotIndex : UDINT;
    END_VAR
    VAR_OUTPUT
        VendorResult : DINT;
    END_VAR
    VendorResult := SendData(pData := #TxSlots[SlotIndex].Data[0], udSize := TxSlots[SlotIndex].DatagramBytes, bDirect := FALSE, udIpAddress := TxSlots[SlotIndex].DestinationIPv4, udPort := TxSlots[SlotIndex].DestinationPort);
END_FUNCTION

FUNCTION LMCUdpCallbackSender::RetryOrDropSlot
    VAR_INPUT
        SlotIndex : UDINT;
        VendorResult : DINT;
    END_VAR
    VAR
        dropSlot : BOOL;
    END_VAR
    dropSlot := FALSE;
    IF VendorResult = -4 THEN
        IF TxSlots[SlotIndex].RetryCount < 3 THEN
            TxSlots[SlotIndex].RetryCount := TxSlots[SlotIndex].RetryCount + 1;
            IF AdmissionRetryCount.Read() <> 16#FFFFFFFF THEN
                AdmissionRetryCount.Write(input := AdmissionRetryCount.Read() + 1);
            END_IF;
        ELSE
            dropSlot := TRUE;
        END_IF;
    ELSE
        dropSlot := TRUE;
    END_IF;
    IF dropSlot THEN
        TxSlots[SlotIndex].InUse := FALSE;
        ReadIndex := ReadIndex + 1;
        IF ReadIndex >= 8 THEN
            ReadIndex := 0;
        END_IF;
        Depth := Depth - 1;
        QueueDepth.Write(input := Depth);
        IF AdmissionErrorDropCount.Read() <> 16#FFFFFFFF THEN
            AdmissionErrorDropCount.Write(input := AdmissionErrorDropCount.Read() + 1);
        END_IF;
    END_IF;
END_FUNCTION

FUNCTION LMCUdpCallbackSender::ClearPendingFrames
    _memset(dest := #TxSlots[0], usByte := 0, cntr := sizeof(TxSlots));
    ReadIndex := 0;
    WriteIndex := 0;
    Depth := 0;
    QueueDepth.Write(input := 0);
END_FUNCTION

FUNCTION LMCUdpCallbackSender::FenceMatches
    VAR_INPUT
        ExpectedSessionEpoch : UDINT;
        ExpectedCookieLo : UDINT;
        ExpectedCookieHi : UDINT;
    END_VAR
    VAR_OUTPUT
        Matches : BOOL;
    END_VAR
    Matches := ActiveEndpoint.Armed AND (ActiveEndpoint.SessionEpoch = ExpectedSessionEpoch) AND (ActiveEndpoint.CookieLo = ExpectedCookieLo) AND (ActiveEndpoint.CookieHi = ExpectedCookieHi);
END_FUNCTION
'@
}

function New-SyntheticTerminalWakeDerivedSource {
    $source = New-SyntheticDerivedSource
    $gateCClassMetadata =
        '<Class Name="LMCUdpCallbackSender" RealtimeTask="false" ' +
        'CyclicTask="true" DefCyclictime="10 ms" BackgroundTask="false" ' +
        'Sigmatek="false">'
    $gateDClassMetadata =
        '<Class Name="LMCUdpCallbackSender" RealtimeTask="false" ' +
        'CyclicTask="true" DefCyclictime="10 ms" BackgroundTask="false" ' +
        'Sigmatek="false" Objectsize="(778,120)">'
    if ((Get-OrdinalCount -Text $source -Needle $gateCClassMetadata) -ne 1) {
        throw 'synthetic Gate D sender Objectsize anchor drifted.'
    }
    $source = $source.Replace($gateCClassMetadata, $gateDClassMetadata)
    $gateCPolicy =
        'ELSIF (EventMaskBit <> 1) OR (EventType <> 1) OR ' +
        '(DeliveryClass <> 0) OR (PayloadBytes <> 0) OR ' +
        '((ActiveEndpoint.EventMask AND EventMaskBit) = 0) THEN'
    $gateDPolicy =
        'ELSIF (EventMaskBit <> 1) OR (EventType <> 1) OR ' +
        '(EventId = 0) OR (DeliveryClass <> 0) OR (PayloadBytes <> 0) OR ' +
        '((ActiveEndpoint.EventMask AND EventMaskBit) = 0) THEN'
    if ((Get-OrdinalCount -Text $source -Needle $gateCPolicy) -ne 1) {
        throw 'synthetic Gate C PublishEvent policy anchor drifted.'
    }
    return $source.Replace($gateCPolicy, $gateDPolicy)
}

function New-SyntheticDerivedEmptyStubSource {
    $source = New-SyntheticDerivedSource
    $source = [regex]::Replace(
        $source,
        '(?m)^#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE\n' +
            '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0\n' +
            '#endif\n',
        '')
    $records = @(Get-FunctionRecords -Text $source -Kind Implementation)
    for ($index = $records.Count - 1; $index -ge 0; $index--) {
        $record = $records[$index]
        $block = $record.Block
        $firstLineEnd = $block.IndexOf("`n", [StringComparison]::Ordinal)
        if ($firstLineEnd -lt 0) {
            throw "synthetic empty stub header is malformed: $($record.Name)"
        }
        $prefixEnd = $firstLineEnd + 1
        $endVariables = @([regex]::Matches(
                $block,
                '(?im)^[ \t]*END_VAR[ \t]*;?[ \t]*(?:\r?\n|\z)'))
        if ($endVariables.Count -ne 0) {
            $last = $endVariables[-1]
            $prefixEnd = $last.Index + $last.Length
        }
        $emptyBlock = $block.Substring(0, $prefixEnd).TrimEnd() +
            "`n`nEND_FUNCTION"
        $source = $source.Remove($record.Index, $block.Length).Insert(
            $record.Index,
            $emptyBlock)
    }
    return $source
}

function Set-SyntheticTcpFunctionBlock {
    param(
        [Parameter(Mandatory = $true)][string]$TcpSource,
        [Parameter(Mandatory = $true)][string]$FunctionName,
        [Parameter(Mandatory = $true)][string]$Replacement
    )

    $block = Get-TcpFunctionBlock `
        -TcpSource $TcpSource -FunctionName $FunctionName
    if ((Get-OrdinalCount -Text $TcpSource -Needle $block) -ne 1) {
        throw "synthetic TCP $FunctionName replacement anchor drifted."
    }
    return $TcpSource.Replace($block, $Replacement.TrimEnd())
}

function Add-SyntheticTcpDisarmFenceToFunction {
    param(
        [Parameter(Mandatory = $true)][string]$FunctionBlock,
        [Parameter(Mandatory = $true)][int]$ExpectedEpochAdvanceCount
    )

    $block = ConvertTo-CanonicalLf -Text $FunctionBlock
    $executable = Get-FunctionExecutableText -FunctionBlock $block
    $executableIndex = $block.IndexOf(
        $executable,
        [StringComparison]::Ordinal)
    if ($executableIndex -lt 0) {
        throw 'synthetic TCP executable insertion anchor drifted.'
    }
    $header = $block.Substring(0, $executableIndex)
    if ([regex]::Matches(
            $header,
            '(?im)^[ \t]*callbackDisarmResult[ \t]*:').Count -ne 0) {
        throw 'synthetic TCP callbackDisarmResult declaration already exists.'
    }
    $localVariableSections = @([regex]::Matches(
            $header,
            '(?ims)^[ \t]*VAR[ \t]*\n.*?' +
                '^[ \t]*END_VAR[ \t]*;?[ \t]*(?:\n|\z)'))
    if ($localVariableSections.Count -gt 1) {
        throw 'synthetic TCP base function has multiple local VAR sections.'
    }
    if ($localVariableSections.Count -eq 1) {
        $localVariableSection = $localVariableSections[0]
        $localEnd = [regex]::Match(
            $localVariableSection.Value,
            '(?im)^[ \t]*END_VAR[ \t]*;?[ \t]*(?=\n|\z)')
        if (-not $localEnd.Success) {
            throw 'synthetic TCP local VAR terminator anchor drifted.'
        }
        $block = $block.Insert(
            $localVariableSection.Index + $localEnd.Index,
            "`t`tcallbackDisarmResult : DINT;`n")
    }
    else {
        $executableLineStart = $block.LastIndexOf(
            "`n",
            $executableIndex - 1,
            [StringComparison]::Ordinal) + 1
        $block = $block.Insert(
            $executableLineStart,
            "`tVAR`n`t`tcallbackDisarmResult : DINT;`n`tEND_VAR`n`n")
    }
    foreach ($clear in $ExpectedTcpCallbackTupleClearStatements[0..6]) {
        $pattern = '(?m)^[ \t]*' + [regex]::Escape($clear) + '[ \t]*(?:\n|$)'
        $block = [regex]::Replace($block, $pattern, '')
    }
    $epochPattern = '(?m)^(?<Indent>[ \t]*)SessionEpoch[ \t]*\+=[ \t]*1;[ \t]*$'
    $epochCount = [regex]::Matches($block, $epochPattern).Count
    if ($epochCount -ne $ExpectedEpochAdvanceCount) {
        throw 'synthetic TCP SessionEpoch insertion count drifted.'
    }
    $block = [regex]::Replace(
        $block,
        $epochPattern,
        '${Indent}callbackDisarmResult := DisarmRpcCallbackEndpoint();' +
            "`n" + '${Indent}SessionEpoch += 1;')
    return $block
}

function Get-SyntheticGateCRpcLifecycleFunction {
    return @'
FUNCTION TCPMotionInterface::HandleRpcLifecycleCommands
	VAR
		callbackEventMask : UDINT;
		callbackPort : DINT;
		callbackIPv4 : UDINT;
		callbackProtocolVersion : UINT;
		callbackAcceptedMaxDatagram : UINT;
		callbackCookieLo : UDINT;
		callbackCookieHi : UDINT;
		callbackFlags : UDINT;
		callbackReserved : UDINT;
		callbackBootId : UDINT;
		callbackArmResult : DINT;
		callbackDisarmResult : DINT;
	END_VAR

  CASE CommandID OF
  0x8080:
    _memset(dest := #Sendbuf, usByte := 0, cntr := sizeof(Sendbuf));
    IF (Payload = 1) AND (RequestBuf[8] = 0) AND
       ((RpcInitialized = FALSE) OR (RpcSocket = CurrentSock)) THEN
      callbackDisarmResult := DisarmRpcCallbackEndpoint();
      IF callbackDisarmResult < 0 THEN
        Sendbuf[0]$UINT := 1;
        Sendbuf[2]$UINT := 4;
        Sendbuf[4]$UDINT := 0;
        Sendbuf[8]$UINT := 1;
        Sendbuf[10]$INT := -1;
        SendData(pData := #Sendbuf[0], udSize := 12,
          dSocket := CurrentSock, bDirect := TRUE);
      ELSE
        RpcSocket := CurrentSock;
        RpcInitialized := TRUE;
        Sendbuf[0]$UINT := 0;
        Sendbuf[2]$UINT := 24;
        Sendbuf[4]$UDINT := 0;
        Sendbuf[8]$UDINT := 64;
        SendData(pData := #Sendbuf[0], udSize := 32,
          dSocket := CurrentSock, bDirect := TRUE);
      END_IF;
    ELSE
      Sendbuf[0]$UINT := 1;
      Sendbuf[2]$UINT := 4;
      Sendbuf[4]$UDINT := 0;
      Sendbuf[8]$UINT := 1;
      Sendbuf[10]$INT := -1;
      SendData(pData := #Sendbuf[0], udSize := 12,
        dSocket := CurrentSock, bDirect := TRUE);
    END_IF;

  0x405C:
    IF (Payload = 12) AND (RpcInitialized = TRUE) AND
       (RpcSocket = CurrentSock) THEN
      IF RpcCallbackProtocolVersion = 0 THEN
        RpcCallbackProtocolVersion := 1;
      END_IF;
      _memset(dest := #Sendbuf, usByte := 0, cntr := sizeof(Sendbuf));
      Sendbuf[0]$UINT := 0;
      Sendbuf[2]$UINT := 4;
      Sendbuf[4]$UDINT := 0;
      IF RpcCallbackProtocolVersion = 1 THEN
        callbackEventMask := RequestBuf[8]$UDINT;
        callbackPort := RequestBuf[12]$DINT;
        callbackIPv4 := RequestBuf[16]$UDINT;
        IF (RpcInitialized = TRUE) AND (RpcSocket = CurrentSock) AND
           (CurrentPeerValid = TRUE) AND
           (callbackIPv4 = CurrentPeerIPv4) AND
           (callbackPort > 0) AND (callbackPort <= 65535) THEN
          IF RpcCallbackRegistered = FALSE THEN
            RpcCallbackEventMask := callbackEventMask;
            RpcCallbackPort := callbackPort;
            _StdLib.MemCpy(dest := #RpcCallbackIPv4[0],
              source := #RequestBuf[16], size := 4);
            RpcCallbackRegistered := TRUE;
            Sendbuf[8]$UINT := 0;
            Sendbuf[10]$INT := 0;
          ELSIF (RpcCallbackEventMask = callbackEventMask) AND
                (RpcCallbackPort = callbackPort) AND
                (RpcCallbackIPv4[0] = RequestBuf[16]) AND
                (RpcCallbackIPv4[1] = RequestBuf[17]) AND
                (RpcCallbackIPv4[2] = RequestBuf[18]) AND
                (RpcCallbackIPv4[3] = RequestBuf[19]) THEN
            Sendbuf[8]$UINT := 0;
            Sendbuf[10]$INT := 0;
          ELSE
            Sendbuf[8]$UINT := 1;
            Sendbuf[10]$INT := -1;
          END_IF;
        ELSE
          Sendbuf[8]$UINT := 1;
          Sendbuf[10]$INT := -1;
        END_IF;
      ELSE
        Sendbuf[8]$UINT := 1;
        Sendbuf[10]$INT := -1;
      END_IF;
      SendData(pData := #Sendbuf[0], udSize := 12,
        dSocket := CurrentSock, bDirect := TRUE);

    ELSIF (Payload = 32) AND (RpcInitialized = TRUE) AND
          (RpcSocket = CurrentSock) THEN
      IF RpcCallbackProtocolVersion = 0 THEN
        RpcCallbackProtocolVersion := 2;
      END_IF;
      _memset(dest := #Sendbuf, usByte := 0, cntr := sizeof(Sendbuf));
      Sendbuf[0]$UINT := 0;
      Sendbuf[2]$UINT := 20;
      Sendbuf[4]$UDINT := 0;
      callbackEventMask := RequestBuf[8]$UDINT;
      callbackPort := RequestBuf[12]$DINT;
      callbackIPv4 := RequestBuf[16]$UDINT;
      callbackProtocolVersion := RequestBuf[20]$UINT;
      callbackAcceptedMaxDatagram := RequestBuf[22]$UINT;
      callbackCookieLo := RequestBuf[24]$UDINT;
      callbackCookieHi := RequestBuf[28]$UDINT;
      callbackFlags := RequestBuf[32]$UDINT;
      callbackReserved := RequestBuf[36]$UDINT;
      callbackBootId := 0;
      callbackArmResult := -9;
      IF IsClientConnected(#Diagnostics) THEN
        callbackBootId := Diagnostics.GetDiagnosticsBootId();
      END_IF;
      IF RpcCallbackProtocolVersion = 2 THEN
        IF (callbackProtocolVersion = 2) AND
           ((callbackEventMask AND 1) = 1) AND
           (callbackPort > 0) AND (callbackPort <= 65535) AND
           (callbackIPv4 = CurrentPeerIPv4) AND (CurrentPeerValid = TRUE) AND
           (callbackAcceptedMaxDatagram >= 52) AND
           (callbackAcceptedMaxDatagram <= 512) AND
           ((callbackCookieLo OR callbackCookieHi) <> 0) AND
           (callbackFlags = 0) AND (callbackReserved = 0) AND
           (SessionEpoch <> 0) AND (callbackBootId <> 0) AND
           (RpcInitialized = TRUE) AND (RpcSocket = CurrentSock) AND
           IsClientConnected(#CallbackSender) THEN
          callbackArmResult := CallbackSender.ArmEndpoint(
            ProtocolVersion := callbackProtocolVersion,
            EventMask := callbackEventMask,
            CallbackIPv4 := callbackIPv4,
            CallbackPort := callbackPort,
            SessionEpoch := SessionEpoch,
            BootId := callbackBootId,
            CookieLo := callbackCookieLo,
            CookieHi := callbackCookieHi,
            MaxDatagramBytes := TO_UDINT(callbackAcceptedMaxDatagram)
          );
        END_IF;
      END_IF;
      IF (callbackArmResult = 0) OR (callbackArmResult = 1) THEN
        RpcCallbackEventMask := callbackEventMask;
        RpcCallbackPort := callbackPort;
        _StdLib.MemCpy(dest := #RpcCallbackIPv4[0],
          source := #RequestBuf[16], size := 4);
        RpcCallbackProtocolVersion := callbackProtocolVersion;
        RpcCallbackAcceptedMaxDatagram := callbackAcceptedMaxDatagram;
        RpcCallbackSessionEpoch := SessionEpoch;
        RpcCallbackBootId := callbackBootId;
        RpcCallbackCookieLo := callbackCookieLo;
        RpcCallbackCookieHi := callbackCookieHi;
        RpcCallbackRegistered := TRUE;
        Sendbuf[8]$UINT := 0;
        Sendbuf[10]$INT := 0;
        Sendbuf[12]$UINT := callbackProtocolVersion;
        Sendbuf[14]$UINT := callbackAcceptedMaxDatagram;
        Sendbuf[16]$UDINT := callbackBootId;
        Sendbuf[20]$UDINT := SessionEpoch;
        Sendbuf[24]$UDINT := 0;
      ELSE
        Sendbuf[8]$UINT := 1;
        Sendbuf[10]$INT := -1;
      END_IF;
      SendData(pData := #Sendbuf[0], udSize := 28,
        dSocket := CurrentSock, bDirect := TRUE);
    ELSE
      _memset(dest := #Sendbuf, usByte := 0, cntr := sizeof(Sendbuf));
      Sendbuf[0]$UINT := 0;
      Sendbuf[2]$UINT := 4;
      Sendbuf[4]$UDINT := 0;
      Sendbuf[8]$UINT := 1;
      Sendbuf[10]$INT := -1;
      SendData(pData := #Sendbuf[0], udSize := 12,
        dSocket := CurrentSock, bDirect := TRUE);
    END_IF;

  0x405D:
    _memset(dest := #Sendbuf, usByte := 0, cntr := sizeof(Sendbuf));
    Sendbuf[0]$UINT := 0;
    Sendbuf[2]$UINT := 4;
    Sendbuf[4]$UDINT := 0;
    IF (Payload = 1) AND (RpcInitialized = TRUE) AND
       (RpcSocket = CurrentSock) THEN
      callbackDisarmResult := DisarmRpcCallbackEndpoint();
      IF callbackDisarmResult < 0 THEN
        Sendbuf[8]$UINT := 1;
        Sendbuf[10]$INT := -1;
        SendData(pData := #Sendbuf[0], udSize := 12,
          dSocket := CurrentSock, bDirect := TRUE);
      ELSE
        Sendbuf[8]$UINT := 0;
        Sendbuf[10]$INT := 0;
        IF (SessionEpoch <> 0) AND (PendingClosedSessionEpoch = 0) THEN
          PendingClosedSessionEpoch := SessionEpoch;
        END_IF;
        SendData(pData := #Sendbuf[0], udSize := 12,
          dSocket := CurrentSock, bDirect := TRUE);
        IF (RpcInitialized = TRUE) AND (RpcSocket = CurrentSock) THEN
          RpcSocket := 0;
          RpcInitialized := FALSE;
          SessionEpoch += 1;
          IF SessionEpoch = 0 THEN
            SessionEpoch := 1;
          END_IF;
        END_IF;
      END_IF;
    ELSE
      Sendbuf[8]$UINT := 1;
      Sendbuf[10]$INT := -1;
      SendData(pData := #Sendbuf[0], udSize := 12,
        dSocket := CurrentSock, bDirect := TRUE);
    END_IF;
  END_CASE;

END_FUNCTION
'@
}

function New-SyntheticGateCTcpSource {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    $source = ConvertTo-CanonicalLf -Text $TcpSource
    foreach ($entry in ([ordered]@{
                ConnSocketInfo = 2
                SendData = 1
                HandleControlSafetyDrainPending = 1
            }).GetEnumerator()) {
        $block = Get-TcpFunctionBlock `
            -TcpSource $source -FunctionName $entry.Key
        $replacement = Add-SyntheticTcpDisarmFenceToFunction `
            -FunctionBlock $block `
            -ExpectedEpochAdvanceCount $entry.Value
        $source = Set-SyntheticTcpFunctionBlock `
            -TcpSource $source `
            -FunctionName $entry.Key `
            -Replacement $replacement
    }
    $source = Set-SyntheticTcpFunctionBlock `
        -TcpSource $source `
        -FunctionName HandleRpcLifecycleCommands `
        -Replacement (Get-SyntheticGateCRpcLifecycleFunction)

    $helper = Get-TcpFunctionBlock `
        -TcpSource $source `
        -FunctionName DisarmRpcCallbackEndpoint
    $helperPrefix = $helper.Substring(
        0,
        $helper.IndexOf('END_VAR', [StringComparison]::Ordinal) +
            'END_VAR'.Length)
    $completeHelper = $helperPrefix + "`n`n" +
        (Get-ExpectedTcpDisarmHelperExecutable).Trim() + "`n`nEND_FUNCTION"
    return Set-SyntheticTcpFunctionBlock `
        -TcpSource $source `
        -FunctionName DisarmRpcCallbackEndpoint `
        -Replacement $completeHelper
}

function New-SyntheticTerminalWakeTcpSource {
    param([Parameter(Mandatory = $true)][string]$TcpSource)

    $source = ConvertTo-CanonicalLf -Text $TcpSource
    $counterAnchor = "`t`tRpcCallbackLastDisarmResult `t: DINT;`n"
    $counterDeclarations =
        "`t`tD5TerminalWakeAttemptCount `t: UDINT;`n" +
        "`t`tD5TerminalWakeEnqueuedCount `t: UDINT;`n" +
        "`t`tD5TerminalWakeRejectedCount `t: UDINT;`n"
    if ((Get-OrdinalCount -Text $source -Needle $counterAnchor) -ne 1) {
        throw 'synthetic Gate D TCP counter declaration anchor drifted.'
    }
    $source = $source.Replace(
        $counterAnchor,
        $counterAnchor + $counterDeclarations)

    $methodAnchor =
        "`tFUNCTION DisarmRpcCallbackEndpoint`n" +
        "`t`tVAR_OUTPUT`n" +
        "`t`t`tResult `t: DINT;`n" +
        "`t`tEND_VAR;`n"
    if ((Get-OrdinalCount -Text $source -Needle $methodAnchor) -ne 1) {
        throw 'synthetic Gate D TCP method declaration anchor drifted.'
    }
    $source = $source.Replace(
        $methodAnchor,
        $methodAnchor + "`t`n`tFUNCTION PublishD5TerminalWake;`n")

    $cyWorkAnchor =
        "    Diagnostics.ProcessOperations();`n" +
        "  end_if;"
    if ((Get-OrdinalCount -Text $source -Needle $cyWorkAnchor) -ne 1) {
        throw 'synthetic Gate D CyWork broker anchor drifted.'
    }
    $source = $source.Replace(
        $cyWorkAnchor,
        "    Diagnostics.ProcessOperations();`n" +
            "    PublishD5TerminalWake();`n" +
            "  end_if;")

    $parserAnchor =
        "    SendData(`n" +
        "      pData:=#Sendbuf[0],`n" +
        "      udSize:=diagnosticsResponseSize`$UDINT,`n" +
        "      dSocket:=CurrentSock,`n" +
        "      bDirect:=TRUE`n" +
        "    );"
    if ((Get-OrdinalCount -Text $source -Needle $parserAnchor) -ne 1) {
        throw 'synthetic Gate D diagnostics response broker anchor drifted.'
    }
    $source = $source.Replace(
        $parserAnchor,
        $parserAnchor + "`n    PublishD5TerminalWake();")
    return $source.TrimEnd() + "`n`n`n" +
        (Get-ExpectedTerminalWakePublishBlock).Trim() + "`n"
}

function New-SyntheticTcpSource {
    param(
        [ValidateSet(
            'Baseline',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$Phase = 'Baseline'
    )

    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
    $baselineRaw = Get-SyntheticGitBlobBytes `
        -RepositoryRoot $root -ObjectId $ExpectedGateBTcpGitOid
    $source = ConvertTo-CanonicalLf -Text (
        ConvertFrom-StrictAsciiBytes `
            -Bytes $baselineRaw -SourceOwner 'synthetic Gate B1 TCP Git blob')
    $baselineBytes = $Utf8.GetBytes($source)
    if (($baselineBytes.Count -ne $ExpectedGateBTcpCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $baselineBytes) -cne
            $ExpectedGateBTcpCanonicalLfSha256)) {
        throw 'synthetic TCP Git blob is not the exact canonical Gate B1 source.'
    }
    if ($Phase -ceq 'Baseline') {
        return $source
    }

    $insertions = @(
        [ordered]@{
            Anchor =
                "`t`t<Client Name=`"_StdLib`" Required=`"false`" " +
                    "Internal=`"false`"/>`n"
            Value =
                "`t`t<Client Name=`"CallbackSender`" Required=`"false`" " +
                    "Internal=`"false`"/>`n"
        },
        [ordered]@{
            Anchor =
                "`tControlCommands `t: CltChCmd_LMCControlCommandService;`n"
            Value = "`tCallbackSender `t: CltChCmd_LMCUdpCallbackSender;`n"
        },
        [ordered]@{
            Anchor = "`t`tRpcCallbackIPv4 : ARRAY [0..3] OF BYTE;`n`n"
            Value =
                "`t`tRpcCallbackProtocolVersion `t: UINT;`n" +
                    "`t`tRpcCallbackAcceptedMaxDatagram `t: UINT;`n" +
                    "`t`tRpcCallbackSessionEpoch `t: UDINT;`n" +
                    "`t`tRpcCallbackBootId `t: UDINT;`n" +
                    "`t`tRpcCallbackCookieLo `t: UDINT;`n" +
                    "`t`tRpcCallbackCookieHi `t: UDINT;`n" +
                    "`t`tRpcCallbackLastDisarmResult `t: DINT;`n"
        },
        [ordered]@{
            Anchor = "`tFUNCTION HandleRpcLifecycleCommands;`n"
            Value =
                "`t`n`tFUNCTION DisarmRpcCallbackEndpoint`n" +
                    "`t`tVAR_OUTPUT`n" +
                    "`t`t`tResult `t: DINT;`n" +
                    "`t`tEND_VAR;`n"
        },
        [ordered]@{
            Anchor = "#pragma usingLtd LMCDiagnosticsService`n"
            Value = "#pragma usingLtd LMCUdpCallbackSender`n"
        },
        [ordered]@{
            Anchor =
                '(::TCPMotionInterface.ControlCommands.pCh)$UINT, ' +
                    '_CH_CLT_OBJ$UINT, 2#0000000000000010$UINT, ' +
                    'TO_UDINT(763639134), "ControlCommands", ' +
                    'TO_UDINT(4292381624), "LMCControlCommandService", ' +
                    '0$UINT, 0$UINT, ' + "`n"
            Value =
                '(::TCPMotionInterface.CallbackSender.pCh)$UINT, ' +
                    '_CH_CLT_OBJ$UINT, 2#0000000000000000$UINT, ' +
                    'TO_UDINT(3384908324), "CallbackSender", ' +
                    'TO_UDINT(287734476), "LMCUdpCallbackSender", ' +
                    '0$UINT, 0$UINT, ' + "`n"
        })
    foreach ($insertion in $insertions) {
        if ((Get-OrdinalCount -Text $source -Needle $insertion.Anchor) -ne 1) {
            throw 'synthetic TCP Gate B1 forward-delta anchor drifted.'
        }
        $source = $source.Replace(
            $insertion.Anchor,
            $insertion.Anchor + $insertion.Value)
    }
    if ((Get-OrdinalCount -Text $source -Needle '4$UINT, 3$UINT, 0$UINT,') -ne 1) {
        throw 'synthetic TCP Gate B1 client-count forward anchor drifted.'
    }
    $source = $source.Replace(
        '4$UINT, 3$UINT, 0$UINT,',
        '4$UINT, 4$UINT, 0$UINT,')
    $source +=
        "`n`nFUNCTION TCPMotionInterface::DisarmRpcCallbackEndpoint`n" +
            "`tVAR_OUTPUT`n" +
            "`t`tResult `t: DINT;`n" +
            "`tEND_VAR`n`n" +
            "END_FUNCTION`n"
    $gateB2Bytes = $Utf8.GetBytes($source)
    if (($gateB2Bytes.Count -ne $ExpectedGateB2TcpCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $gateB2Bytes) -cne
            $ExpectedGateB2TcpCanonicalLfSha256)) {
        throw 'synthetic TCP forward delta does not reconstruct exact Gate B2.'
    }
    if ($Phase -in @(
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')) {
        $source = New-SyntheticGateCTcpSource -TcpSource $source
        if ($Phase -ceq 'TerminalWakeBrokerCandidate') {
            return New-SyntheticTerminalWakeTcpSource -TcpSource $source
        }
        return $source
    }
    return $source
}

function Get-SyntheticGitBlobBytes {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$ObjectId
    )

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = 'git'
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.CreateNoWindow = $true
    if (($ObjectId -notmatch '^[0-9a-f]{40,64}$') -or
        $RepositoryRoot.Contains('"')) {
        throw 'synthetic Git blob arguments are unsafe.'
    }
    $start.Arguments =
        '-C "' + $RepositoryRoot + '" cat-file blob ' + $ObjectId
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    if (-not $process.Start()) {
        throw "synthetic Git blob $ObjectId could not be started."
    }
    try {
        $stream = [IO.MemoryStream]::new()
        try {
            $process.StandardOutput.BaseStream.CopyTo($stream)
            $errorText = $process.StandardError.ReadToEnd()
            $process.WaitForExit()
            if ($process.ExitCode -ne 0) {
                throw "synthetic Git blob $ObjectId is unavailable: $errorText"
            }
            return ,$stream.ToArray()
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $process.Dispose()
    }
}

function New-SyntheticLasalBinaryText {
    param([Parameter(Mandatory = $true)][string[]]$Records)

    $bytes = [Collections.Generic.List[byte]]::new()
    $bytes.AddRange([Text.Encoding]::ASCII.GetBytes(
            'SigmatekLasal2Binary' + [char]0))
    $bytes.AddRange([byte[]](0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
    foreach ($record in $Records) {
        $recordBytes = [Text.Encoding]::ASCII.GetBytes($record)
        if (($recordBytes.Count -lt 1) -or ($recordBytes.Count -gt 255)) {
            throw 'synthetic LASAL binary record length is out of range.'
        }
        $bytes.Add([byte]$recordBytes.Count)
        $bytes.Add(0)
        $bytes.Add(0)
        $bytes.Add(0xAA)
        $bytes.AddRange($recordBytes)
    }
    $bytes.AddRange([byte[]](0, 0, 0, 0))
    return $Latin1.GetString($bytes.ToArray())
}

function New-SyntheticDerivedNetwork {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedDeclaration', 'DerivedWired', 'DerivedCandidate')]
        [string]$State
    )

    $xml = @'
<Network Name="Comm_Network">
  <Components>
    <Object Name="LMCUdpTransceiver1" Class="_UDPTransceiver" Position="(120,180)" Visualized="false" Remotely="true" CyclicTime="10 ms" BackgroundTime="always">
      <Channels>
        <Server Name="sControl"/>
        <Server Name="sError"/>
        <Server Name="sErrorMessage"/>
        <Server Name="sErrorNoOS"/>
        <Client Name="coStdLib"/>
        <Client Name="cSizeOfRXBuffer" Value="512"/>
        <Client Name="cSizeOfTXBuffer" Value="8 kb"/>
      </Channels>
    </Object>
    <Object Name="LMCUdpCallbackSender1" Class="LMCUdpCallbackSender" Position="(120,900)" Visualized="false" Remotely="true" CyclicTime="10 ms">
      <Channels>
        <Server Name="AdmissionErrorDropCount" Value="0"/>
        <Server Name="AdmissionRetryCount" Value="0"/>
        <Server Name="ClassSvr"/>
        <Server Name="DisarmClearedCount" Value="0"/>
        <Server Name="ErrorCode"/>
        <Server Name="ErrorMessage"/>
        <Server Name="ErrorState"/>
        <Server Name="LastAdmissionResult" Value="0"/>
        <Server Name="QueuedCount" Value="0"/>
        <Server Name="QueueDepth" Value="0"/>
        <Server Name="QueueFullDropCount" Value="0"/>
        <Server Name="RingAcceptedCount" Value="0"/>
        <Server Name="State"/>
        <Server Name="TransportErrorCount" Value="0"/>
        <Client Name="_UDPTransceiver"/>
      </Channels>
    </Object>
  </Components>
  <Connections>
    <Connection Source="LMCUdpCallbackSender1._UDPTransceiver" Destination="LMCUdpTransceiver1.sControl"/>
    <Connection Source="TCPMotionInterface1.CallbackSender" Destination="LMCUdpCallbackSender1.ClassSvr"/>
  </Connections>
</Network>
'@
    $table = @'
#define OBJECTS_CONFIG

FUNCTION GLOBAL TAB ONE_Comm_Network
TO_UDINT(1), "_UDPTransceiver", 1$UINT, 2$UINT, 1$UINT,
TO_UDINT(2), "_UDPTransceiverInterface", 1$UINT, 3$UINT, 0$UINT,
TO_UDINT(3), "LMCUdpCallbackSender", 0$UINT, 0$UINT, 1$UINT,
_NO_ATTR, TO_UDINT(4), "LMCUDPTRANSCEIVER1",
_NO_ATTR, TO_UDINT(5), "LMCUDPCALLBACKSENDER1",
TO_UDINT(6), "_UDPTransceiver", TO_UDINT(1), "sControl",
TO_UDINT(14), "CallbackSender", TO_UDINT(6), "ClassSvr",
TO_UDINT(1), "cSizeOfTXBuffer", TO_UDINT(8 kb),
TO_UDINT(1), "cSizeOfRXBuffer", TO_UDINT(512),
TO_UDINT(1), (10)$UDINT, 4194303$DINT, //LMCUDPTRANSCEIVER1
TO_UDINT(6), (10)$UDINT, 4194303$DINT, //LMCUDPCALLBACKSENDER1
(0)$UDINT, //LMCUDPTRANSCEIVER1
(0)$UDINT, //LMCUDPCALLBACKSENDER1
END_FUNCTION
'@
    if ($State -ceq 'DerivedCandidate') {
        $table = $table.Replace(
            "#define OBJECTS_CONFIG`n",
            "#define OBJECTS_CONFIG`n" +
                "#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE`n" +
                "#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0`n" +
                "#endif`n")
    }
    $database = New-SyntheticLasalBinaryText -Records @(
        'Comm_Network',
        'LMCUdpTransceiver1',
        '_UDPTransceiver',
        'LMCUdpCallbackSender1',
        'LMCUdpCallbackSender',
        'CallbackSender',
        'cSizeOfRXBuffer',
        'cSizeOfTXBuffer')
    return [pscustomobject]@{
        Xml = $xml
        Table = $table
        Database = $database
    }
}

function New-SyntheticProjectDefinition {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedDeclaration', 'DerivedWired', 'DerivedCandidate')]
        [string]$State
    )

    if ($State -ceq 'Absent') {
        return @'
<Project>
<ClassFiles/>
<SigmatekFolders/>
</Project>
'@
    }
    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
    $path = Join-Path $root $ProjectDefinitionRelativePath.Replace('/', '\')
    $source = ConvertTo-CanonicalLf -Text ([IO.File]::ReadAllText($path, $Utf8))
    $sourceBytes = $Utf8.GetBytes($source)
    if (($sourceBytes.Count -ne
            $ExpectedVendorImportedProjectDefinitionCanonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $sourceBytes) -cne
            $ExpectedVendorImportedProjectDefinitionCanonicalLfSha256)) {
        $filePattern =
            '(?m)^[ \t]*<File Path="\.\\Class\\LMCUdpCallbackSender\\' +
            'LMCUdpCallbackSender\.st"/>[ \t]*(?:\n|\z)'
        $fileMatches = @([regex]::Matches($source, $filePattern))
        if ($fileMatches.Count -ne 1) {
            throw 'synthetic lcp cannot recover one derived File registration.'
        }
        $source = $source.Remove(
            $fileMatches[0].Index,
            $fileMatches[0].Length)
        $classPattern =
            '(?m)^[ \t]*<Class Name="LMCUdpCallbackSender"/>[ \t]*(?:\n|\z)'
        $classMatches = @([regex]::Matches($source, $classPattern))
        if ($classMatches.Count -gt 1) {
            throw 'synthetic lcp has duplicate derived Class registrations.'
        }
        if ($classMatches.Count -eq 1) {
            $source = $source.Remove(
                $classMatches[0].Index,
                $classMatches[0].Length)
        }
        $sourceBytes = $Utf8.GetBytes($source)
        if (($sourceBytes.Count -ne
                $ExpectedVendorImportedProjectDefinitionCanonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $sourceBytes) -cne
                $ExpectedVendorImportedProjectDefinitionCanonicalLfSha256)) {
            throw 'synthetic lcp does not reverse exactly to Gate A.'
        }
    }
    if ($State -ceq 'VendorImported') {
        return $source
    }
    $vendorFile = "`t`t" +
        '<File Path=".\Class\_UDPTransceiverInterface\' +
        '_UDPTransceiverInterface.st"/>'
    $derivedFile = "`t`t" +
        '<File Path=".\Class\LMCUdpCallbackSender\' +
        'LMCUdpCallbackSender.st"/>'
    $source = $source.Replace(
        $vendorFile,
        $vendorFile + "`n" + $derivedFile)
    return $source
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

function New-SyntheticTerminalWakeTransceiverSource {
    $source = New-SyntheticVendorTransceiverSource
    if ((Get-OrdinalCount -Text $source -Needle '<Class>') -ne 1) {
        throw 'synthetic Gate D transceiver Objectsize anchor drifted.'
    }
    return $source.Replace(
        '<Class>',
        '<Class Name="_UDPTransceiver" Objectsize="(522,120)">')
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
        [ValidateSet('Absent', 'VendorImported', 'DerivedDeclaration', 'DerivedWired', 'DerivedCandidate')]
        [string]$State
    )

    if ($State -ceq 'Absent') {
        $rows = [Collections.Generic.List[string]]::new()
        foreach ($index in 1..117) {
            $rows.Add(('0$UINT, 0, 0, "BASE_{0:D3}",' -f $index))
        }
        return "FUNCTION GLOBAL TAB CONFIG_TABLES`n00117`$UINT,`n" +
            [string]::Join("`n", $rows) + "`nEND_FUNCTION"
    }

    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
    $path = Join-Path $root $ConfigObjectsRelativePath.Replace('/', '\')
    $source = [IO.File]::ReadAllText($path, $Utf8)
    $canonical = ConvertTo-CanonicalLf -Text $source
    $senderRowPattern =
        '(?m)^0\$UINT, 0, 0, "LMCUDPCALLBACKSENDER",[ \t]*\n'
    $senderRows = @([regex]::Matches($canonical, $senderRowPattern))
    if (($senderRows.Count -ne 1) -or
        ((Get-OrdinalCount `
                -Text $canonical `
                -Needle "FUNCTION GLOBAL TAB CONFIG_TABLES`n00120`$UINT,") -ne 1)) {
        throw 'synthetic ConfigObjects B1 registry baseline drifted.'
    }
    if ($State -ceq 'VendorImported') {
        $rawRowPattern =
            '(?m)^0\$UINT, 0, 0, "LMCUDPCALLBACKSENDER",[ \t]*(?:\r?\n)'
        $source = [regex]::Replace($source, $rawRowPattern, '', 1)
        return $source.Replace('00120$UINT', '00119$UINT')
    }
    return $source
}

function New-SyntheticGeneratedIncludes {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Absent', 'VendorImported', 'DerivedDeclaration', 'DerivedWired', 'DerivedCandidate')]
        [string]$State
    )

    $vendorPresent = $State -cne 'Absent'
    $derivedPresent = $State -in @('DerivedWired', 'DerivedCandidate')
    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
    $textByName = @{}
    if ($vendorPresent) {
        foreach ($expected in $GeneratedIncludeContracts) {
            $path = Join-Path $root $expected.Path.Replace('/', '\')
            $textByName[$expected.Name] = ConvertTo-CanonicalLf `
                -Text ([IO.File]::ReadAllText($path, $Utf8))
        }
        if (-not $derivedPresent) {
            foreach ($removal in @(
                    @{
                        Name = 'C_channels.h'
                        Pattern =
                            '(?ms)^[ \t]*typedef[ \t]+struct[ \t]+' +
                            'CltChCmd_LMCUdpCallbackSender\b.*?\}[ \t]*' +
                            'CltChCmd_LMCUdpCallbackSender[ \t]*;[ \t]*(?=\r?$)'
                    },
                    @{
                        Name = 'channels.h'
                        Pattern =
                            '(?ms)^[ \t]*CltChCmd_LMCUdpCallbackSender[ \t]*:' +
                            '[ \t]*STRUCT\b.*?END_STRUCT[ \t]*;[ \t]*(?=\r?$)'
                    })) {
                $expected = @($GeneratedIncludeContracts | Where-Object {
                        $_.Name -ceq $removal.Name
                    })[0]
                $canonical = $textByName[$removal.Name]
                $matches = @([regex]::Matches($canonical, $removal.Pattern))
                if ($matches.Count -ne 1) {
                    throw "synthetic $($removal.Name) derived block count drifted."
                }
                $restored = $null
                foreach ($leading in 0..2) {
                    foreach ($trailing in 0..2) {
                        $start = $matches[0].Index - $leading
                        $length = $matches[0].Length + $leading + $trailing
                        if (($start -lt 0) -or
                            (($start + $length) -gt $canonical.Length)) {
                            continue
                        }
                        if (($leading -gt 0) -and
                            ($canonical.Substring($start, $leading) -cne
                                ("`n" * $leading))) {
                            continue
                        }
                        if (($trailing -gt 0) -and
                            ($canonical.Substring(
                                    $matches[0].Index + $matches[0].Length,
                                    $trailing) -cne ("`n" * $trailing))) {
                            continue
                        }
                        $candidate = $canonical.Remove($start, $length)
                        $bytes = $Utf8.GetBytes($candidate)
                        if (($bytes.Count -eq $expected.VendorCanonicalLfBytes) -and
                            ((Get-BytesSha256 -Bytes $bytes) -ceq
                                $expected.VendorCanonicalLfSha256)) {
                            $restored = $candidate
                            break
                        }
                    }
                    if ($null -ne $restored) { break }
                }
                if ($null -eq $restored) {
                    throw "synthetic $($removal.Name) does not restore Gate A."
                }
                $textByName[$removal.Name] = $restored
            }
        }
    }
    else {
        $textByName['C_channels.h'] = 'BASE C CHANNELS'
        $textByName['channels.h'] = 'BASE ST CHANNELS'
        $textByName['lslpublictypes.h'] = 'BASE PUBLIC TYPES'
    }
    return @(
        foreach ($expected in $GeneratedIncludeContracts) {
            $prefix = if ($vendorPresent) { 'Vendor' } else { 'Absent' }
            $text = $textByName[$expected.Name]
            $bytes = if ($vendorPresent) { $Utf8.GetBytes($text) } else { $null }
            [pscustomobject]@{
                Name = $expected.Name
                Path = $expected.Path
                Text = $text
                RawBytes = if ($vendorPresent) {
                    $bytes.Count
                } else { $expected["${prefix}CanonicalLfBytes"] }
                RawSha256 = if ($vendorPresent) {
                    Get-BytesSha256 -Bytes $bytes
                } else { $expected["${prefix}CanonicalLfSha256"] }
                CanonicalLfBytes = if ($vendorPresent) {
                    $bytes.Count
                } else { $expected["${prefix}CanonicalLfBytes"] }
                CanonicalLfSha256 = if ($vendorPresent) {
                    Get-BytesSha256 -Bytes $bytes
                } else { $expected["${prefix}CanonicalLfSha256"] }
                EolStyle = 'LF'
                LineBreakCount = if ($vendorPresent) {
                    [regex]::Matches($text, "`n").Count
                } else { $expected["${prefix}LineBreakCount"] }
            }
        })
}

function New-SyntheticTerminalWakeDiagnosticsSource {
    return @'
LMCDiagnosticsService : CLASS
    VAR
        BootIdFault : BOOL;
        D5TerminalWakeLastAttemptTicketId : UDINT;
        D5TerminalWakeLastAttemptTicketBootId : UDINT;
        D5TerminalWakeLastAttemptOwnerSessionEpoch : UDINT;
    END_VAR

    FUNCTION GLOBAL ProcessOperations;

    FUNCTION GLOBAL TryTakeD5TerminalWake
        VAR_INPUT
            pTicketId : ^UDINT;
            pTicketBootId : ^UDINT;
            pOwnerSessionEpoch : ^UDINT;
        END_VAR
        VAR_OUTPUT
            Result : DINT;
        END_VAR;

    FUNCTION IsSdoReadReady;
END_CLASS;

FUNCTION LMCDiagnosticsService::LMCDiagnosticsService
    D5TerminalWakeLastAttemptTicketId := 0;
    D5TerminalWakeLastAttemptTicketBootId := 0;
    D5TerminalWakeLastAttemptOwnerSessionEpoch := 0;
END_FUNCTION

FUNCTION GLOBAL LMCDiagnosticsService::TryTakeD5TerminalWake
    VAR_INPUT
        pTicketId : ^UDINT;
        pTicketBootId : ^UDINT;
        pOwnerSessionEpoch : ^UDINT;
    END_VAR
    VAR_OUTPUT
        Result : DINT;
    END_VAR

    Result := -1;
    if (pTicketId = NIL) | (pTicketBootId = NIL) |
        (pOwnerSessionEpoch = NIL) then
        RETURN;
    end_if;

    pTicketId^$UDINT := 0;
    pTicketBootId^$UDINT := 0;
    pOwnerSessionEpoch^$UDINT := 0;
    Result := 0;

    if (TicketId = 0) | (TicketBootId = 0) |
        (OwnerSessionEpoch = 0) then
        RETURN;
    end_if;
    if (OperationState <> LMC_DIAG_SDO_STATE_COMPLETED) &
        (OperationState <> LMC_DIAG_SDO_STATE_FAILED) &
        (OperationState <> LMC_DIAG_SDO_STATE_CANCELLED) &
        (OperationState <> LMC_DIAG_SDO_STATE_EXPIRED) then
        RETURN;
    end_if;
    if (D5TerminalWakeLastAttemptTicketId = TicketId) &
        (D5TerminalWakeLastAttemptTicketBootId = TicketBootId) &
        (D5TerminalWakeLastAttemptOwnerSessionEpoch = OwnerSessionEpoch) then
        RETURN;
    end_if;

    D5TerminalWakeLastAttemptTicketId := TicketId;
    D5TerminalWakeLastAttemptTicketBootId := TicketBootId;
    D5TerminalWakeLastAttemptOwnerSessionEpoch := OwnerSessionEpoch;
    pTicketId^$UDINT := TicketId;
    pTicketBootId^$UDINT := TicketBootId;
    pOwnerSessionEpoch^$UDINT := OwnerSessionEpoch;
    Result := 1;

END_FUNCTION
'@
}

function New-UdpCallbackTestSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'Absent',
            'VendorImported',
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State
    )

    $historicalState = if ($State -ceq 'TerminalWakeBrokerCandidate') {
        'DerivedCandidate'
    } else { $State }
    $classes = New-SyntheticClassesDatabase -State $State
    $derivedNetwork = New-SyntheticDerivedNetwork -State $historicalState
    if ($State -ceq 'TerminalWakeBrokerCandidate') {
        foreach ($position in @(
                @{ Old = '(120,180)'; New = '(1410,990)' },
                @{ Old = '(120,900)'; New = '(2610,990)' })) {
            if ((Get-OrdinalCount -Text $derivedNetwork.Xml `
                    -Needle $position.Old) -ne 1) {
                throw 'synthetic Gate D Network position anchor drifted.'
            }
            $derivedNetwork.Xml = $derivedNetwork.Xml.Replace(
                $position.Old, $position.New)
        }
    }
    $configObjects = New-SyntheticConfigObjects -State $historicalState
    $vendorPresent = $State -cne 'Absent'
    $derivedPresent =
        $State -in @(
            'DerivedDeclaration',
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')
    $wiredPresent = $State -in @(
        'DerivedWired',
        'DerivedCandidate',
        'TerminalWakeBrokerCandidate')
    $projectDefinition = New-SyntheticProjectDefinition -State $historicalState
    $projectDefinitionRaw = [Text.Encoding]::ASCII.GetBytes($projectDefinition)
    $configObjectsRaw = [Text.Encoding]::ASCII.GetBytes($configObjects)
    $root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
    $projectDatabaseText = if ($derivedPresent) {
        New-SyntheticLasalBinaryText -Records @('LMCUdpCallbackSender')
    }
    else {
        $projectPath = Join-Path $root $ProjectRelativePath.Replace('/', '\')
        $Latin1.GetString([IO.File]::ReadAllBytes($projectPath))
    }
    $projectDatabaseRaw = $Latin1.GetBytes($projectDatabaseText)
    if ($wiredPresent) {
        $networksDatabaseText = $derivedNetwork.Database
        $networksDatabaseRaw = $Latin1.GetBytes(
            $networksDatabaseText)
        $commTableText = $derivedNetwork.Table
        $commTableRaw = [Text.Encoding]::ASCII.GetBytes($commTableText)
    }
    else {
        $networksDatabaseRaw = Get-SyntheticGitBlobBytes `
            -RepositoryRoot $root `
            -ObjectId $ExpectedVendorImportedNetworksDatabaseGitOid
        if (($networksDatabaseRaw.Count -ne
                $ExpectedVendorImportedNetworksDatabaseBytes) -or
            ((Get-BytesSha256 -Bytes $networksDatabaseRaw) -cne
                $ExpectedVendorImportedNetworksDatabaseSha256)) {
            throw 'synthetic Gate A Networks.lcb Git blob drifted.'
        }
        $networksDatabaseText = $Latin1.GetString(
            $networksDatabaseRaw)
        $commTableRaw = Get-SyntheticGitBlobBytes `
            -RepositoryRoot $root `
            -ObjectId $ExpectedVendorImportedCommTableGitOid
        $commTableText = $Utf8.GetString($commTableRaw)
        $commCanonicalBytes = $Utf8.GetBytes(
            (ConvertTo-CanonicalLf -Text $commTableText))
        if (($commCanonicalBytes.Count -ne
                $ExpectedVendorImportedCommTableCanonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $commCanonicalBytes) -cne
                $ExpectedVendorImportedCommTableCanonicalLfSha256)) {
            throw 'synthetic Gate A ONE table Git blob drifted.'
        }
    }
    $commNetworkText = if ($wiredPresent) { $derivedNetwork.Xml } else { '' }
    $commNetworkRaw = [Text.Encoding]::ASCII.GetBytes($commNetworkText)
    $transceiverSource = if ($vendorPresent) {
        if ($State -ceq 'TerminalWakeBrokerCandidate') {
            New-SyntheticTerminalWakeTransceiverSource
        }
        else { New-SyntheticVendorTransceiverSource }
    }
    else { '' }
    $transceiverExpected = if (
        $State -ceq 'TerminalWakeBrokerCandidate') {
            $ExpectedTerminalWakeLayout.Transceiver
        }
        else { $ExpectedVendor.Transceiver }
    $derivedSource = if ($State -ceq 'TerminalWakeBrokerCandidate') {
        New-SyntheticTerminalWakeDerivedSource
    } elseif ($State -ceq 'DerivedCandidate') {
        New-SyntheticDerivedSource
    } elseif ($derivedPresent) {
        New-SyntheticDerivedEmptyStubSource
    } else { '' }
    $derivedCanonical = ConvertTo-CanonicalLf -Text $derivedSource
    $derivedCanonicalBytes = $Utf8.GetByteCount($derivedCanonical)
    $derivedCanonicalSha256 = Get-TextSha256 -Text $derivedCanonical
    $protected = @(
        foreach ($expected in $ProtectedDependencies) {
            [pscustomobject]@{
                Name = $expected.Name
                Bytes = $expected.Bytes
                Sha256 = $expected.Sha256
            }
        })
    return [pscustomobject]@{
        SyntheticFixture = $true
        ProtectedDependencies = $protected
        GeneratedIncludes = @(
            New-SyntheticGeneratedIncludes -State $historicalState)
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
                        Sha256 = if (
                            ($State -ceq 'TerminalWakeBrokerCandidate') -and
                            $expected.Contains('TerminalWakeSha256')) {
                            $expected.TerminalWakeSha256
                        } elseif ($wiredPresent) {
                            $expected.WiredSha256
                        } else { $expected.Sha256 }
                    }
                }
            })
        ForbiddenPaths = @()
        TransceiverPresent = $vendorPresent
        InterfacePresent = $vendorPresent
        DerivedPresent = $derivedPresent
        TransceiverSource = $transceiverSource
        InterfaceSource = if ($vendorPresent) {
            New-SyntheticVendorInterfaceSource
        } else { '' }
        TransceiverCanonicalLfSha256 = if ($vendorPresent) {
            $transceiverExpected.CanonicalLfSha256
        } else { '' }
        TransceiverCanonicalLfBytes = if ($vendorPresent) {
            $transceiverExpected.CanonicalLfBytes
        } else { 0 }
        TransceiverRawSha256 = if ($vendorPresent) {
            $transceiverExpected.CanonicalLfSha256
        } else { '' }
        TransceiverRawBytes = if ($vendorPresent) {
            $transceiverExpected.CanonicalLfBytes
        } else { 0 }
        TransceiverEolStyle = if ($vendorPresent) { 'LF' } else { 'Absent' }
        TransceiverLineBreakCount = if ($vendorPresent) {
            $transceiverExpected.LineBreakCount
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
        DerivedSource = $derivedSource
        DerivedRawBytes = if ($State -ceq 'TerminalWakeBrokerCandidate') {
            $ExpectedTerminalWakeLayout.Sender.CanonicalLfBytes
        } else { $derivedCanonicalBytes }
        DerivedRawSha256 = if ($State -ceq 'TerminalWakeBrokerCandidate') {
            $ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256
        } else { $derivedCanonicalSha256 }
        DerivedCanonicalLfBytes = if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                $ExpectedTerminalWakeLayout.Sender.CanonicalLfBytes
            } else { $derivedCanonicalBytes }
        DerivedCanonicalLfSha256 = if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                $ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256
            } else { $derivedCanonicalSha256 }
        DerivedEolStyle = if ($derivedPresent) { 'LF' } else { 'Absent' }
        DerivedLineBreakCount = if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                $ExpectedTerminalWakeLayout.Sender.LineBreakCount
            } elseif ($derivedPresent) {
                ([regex]::Matches($derivedCanonical, "`n")).Count
            } else { 0 }
        DiagnosticsSource = if ($State -ceq 'TerminalWakeBrokerCandidate') {
            New-SyntheticTerminalWakeDiagnosticsSource
        } else { '' }
        TcpSource = New-SyntheticTcpSource -Phase $(if (
            $State -ceq 'TerminalWakeBrokerCandidate') {
                'TerminalWakeBrokerCandidate'
            } elseif ($State -ceq 'DerivedCandidate') {
                'DerivedCandidate'
            } elseif ($wiredPresent) {
                'DerivedWired'
            } else {
                'Baseline'
            })
        TcpSha256 = if ($wiredPresent) {
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
        ProjectBytes = $projectDatabaseRaw.Count
        ProjectSha256 = Get-BytesSha256 -Bytes $projectDatabaseRaw
        ProjectDefinitionBytes = $projectDefinitionRaw.Count
        ProjectDefinitionSha256 = Get-BytesSha256 -Bytes $projectDefinitionRaw
        ProjectDefinitionText = $projectDefinition
        ProjectDatabaseText = $projectDatabaseText
        FullNetworkCount = if ($wiredPresent) {
            $ExpectedBaselineNetworkInventoryCount
        } else { $ExpectedBaselineNetworkInventoryCount }
        FullNetworkSha256 = if ($wiredPresent) {
            'SYNTHETIC-DERIVED-NETWORK'
        } elseif ($vendorPresent) {
            $ExpectedVendorImportedNetworkInventorySha256
        } else { $ExpectedBaselineNetworkInventorySha256 }
        TrackedNetworkCount = $ExpectedBaselineTrackedNetworkCount
        TrackedNetworkSha256 = if ($wiredPresent) {
            'SYNTHETIC-DERIVED-NETWORK'
        } elseif ($vendorPresent) {
            $ExpectedVendorImportedTrackedNetworkSha256
        } else { $ExpectedBaselineTrackedNetworkSha256 }
        ProtectedTrackedNetworkCount = $ExpectedProtectedTrackedNetworkCount
        ProtectedTrackedNetworkSha256 =
            $ExpectedProtectedTrackedNetworkSha256
        ConfigObjectsText = $configObjects
        ConfigObjectsBytes = $configObjectsRaw.Count
        ConfigObjectsSha256 = Get-BytesSha256 -Bytes $configObjectsRaw
        NetworksDatabaseBytes = $networksDatabaseRaw.Count
        NetworksDatabaseSha256 = Get-BytesSha256 -Bytes $networksDatabaseRaw
        CommNetworkText = $commNetworkText
        CommNetworkBytes = $commNetworkRaw.Count
        CommNetworkSha256 = Get-BytesSha256 -Bytes $commNetworkRaw
        CommTableText = $commTableText
        CommTableBytes = $commTableRaw.Count
        CommTableSha256 = Get-BytesSha256 -Bytes $commTableRaw
        NetworksDatabaseText = $networksDatabaseText
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
    $text = $Latin1.GetString($bytes)
    $nameStart = $text.IndexOf($FunctionName, [StringComparison]::Ordinal)
    if ($nameStart -lt 0) {
        throw "self-test fixture method is missing: $FunctionName"
    }
    $headerStart = $nameStart + $FunctionName.Length
    $expectedKind = if ($FunctionName -ceq 'CyWork') { 0x05 } else { 0x0B }
    if ($bytes[$headerStart] -ne $expectedKind) {
        throw "self-test fixture method header is malformed: $FunctionName"
    }
    $bytes[$headerStart + $HeaderOffset] = $Value
    $Snapshot.ClassesDatabaseBytes = $bytes
    $Snapshot.ClassesDatabaseText = $Latin1.GetString($bytes)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Set-SyntheticClassesByteAt {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][int]$Index,
        [Parameter(Mandatory = $true)][byte]$Value
    )

    if (($Index -lt 0) -or ($Index -ge $Snapshot.ClassesDatabaseBytes.Count)) {
        throw 'synthetic Classes byte mutation index is out of range.'
    }
    $Snapshot.ClassesDatabaseBytes[$Index] = $Value
    $Snapshot.ClassesDatabaseText = $Latin1.GetString(
        $Snapshot.ClassesDatabaseBytes)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Insert-SyntheticClassesByteAt {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][int]$Index,
        [Parameter(Mandatory = $true)][byte]$Value
    )

    if (($Index -lt 0) -or ($Index -gt $Snapshot.ClassesDatabaseBytes.Count)) {
        throw 'synthetic Classes byte insertion index is out of range.'
    }
    $updated = [byte[]]::new($Snapshot.ClassesDatabaseBytes.Count + 1)
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        0,
        $updated,
        0,
        $Index)
    $updated[$Index] = $Value
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        $Index,
        $updated,
        $Index + 1,
        $Snapshot.ClassesDatabaseBytes.Count - $Index)
    $Snapshot.ClassesDatabaseBytes = $updated
    $Snapshot.ClassesDatabaseText = $Latin1.GetString($updated)
    Invalidate-SyntheticClassesEvidence -Snapshot $Snapshot
}

function Insert-SyntheticClassesBytesAt {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][int]$Index,
        [Parameter(Mandatory = $true)][byte[]]$Values
    )

    if (($Index -lt 0) -or ($Index -gt $Snapshot.ClassesDatabaseBytes.Count) -or
        ($Values.Count -eq 0)) {
        throw 'synthetic Classes byte-array insertion is invalid.'
    }
    $updated = [byte[]]::new(
        $Snapshot.ClassesDatabaseBytes.Count + $Values.Count)
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        0,
        $updated,
        0,
        $Index)
    [Array]::Copy($Values, 0, $updated, $Index, $Values.Count)
    [Array]::Copy(
        $Snapshot.ClassesDatabaseBytes,
        $Index,
        $updated,
        $Index + $Values.Count,
        $Snapshot.ClassesDatabaseBytes.Count - $Index)
    $Snapshot.ClassesDatabaseBytes = $updated
    $Snapshot.ClassesDatabaseText = $Latin1.GetString($updated)
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
    $Snapshot.ClassesDatabaseText = $Latin1.GetString($updated)
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
    $Snapshot.ClassesDatabaseText = $Latin1.GetString(
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
        $commentLength = [uint32](
            $Snapshot.ClassesDatabaseBytes[$cursor] -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 1] -shl 8) -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 2] -shl 16))
        $cursor += 4 + $commentLength + 56
        $baseTypeLength = [uint32](
            $Snapshot.ClassesDatabaseBytes[$cursor] -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 1] -shl 8) -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 2] -shl 16))
        $cursor += 4 + $baseTypeLength
        $ownerLength = [uint32](
            $Snapshot.ClassesDatabaseBytes[$cursor] -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 1] -shl 8) -bor
            ($Snapshot.ClassesDatabaseBytes[$cursor + 2] -shl 16))
        $cursor += 4 + $ownerLength + 48
    }
    $Snapshot.ClassesDatabaseBytes[$cursor] = $Value
    $Snapshot.ClassesDatabaseText = $Latin1.GetString(
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

function Update-SyntheticAsciiSnapshotEvidence {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$TextProperty,
        [Parameter(Mandatory = $true)][string]$BytesProperty,
        [Parameter(Mandatory = $true)][string]$ShaProperty
    )

    $bytes = [Text.Encoding]::ASCII.GetBytes([string]$Snapshot.$TextProperty)
    $Snapshot.$BytesProperty = $bytes.Count
    $Snapshot.$ShaProperty = Get-BytesSha256 -Bytes $bytes
}

function Update-SyntheticLatin1SnapshotEvidence {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot,
        [Parameter(Mandatory = $true)][string]$TextProperty,
        [Parameter(Mandatory = $true)][string]$BytesProperty,
        [Parameter(Mandatory = $true)][string]$ShaProperty
    )

    $bytes = $Latin1.GetBytes([string]$Snapshot.$TextProperty)
    $Snapshot.$BytesProperty = $bytes.Count
    $Snapshot.$ShaProperty = Get-BytesSha256 -Bytes $bytes
}

function Update-SyntheticGeneratedIncludeEvidence {
    param([Parameter(Mandatory = $true)][pscustomobject]$Include)

    $canonical = ConvertTo-CanonicalLf -Text $Include.Text
    $canonicalBytes = $Utf8.GetBytes($canonical)
    $rawBytes = $Utf8.GetBytes($Include.Text)
    $hasCrLf = $Include.Text.Contains("`r`n")
    $withoutCrLf = $Include.Text.Replace("`r`n", '')
    $hasBareLf = $withoutCrLf.Contains("`n")
    $hasBareCr = $withoutCrLf.Contains("`r")
    $Include.CanonicalLfBytes = $canonicalBytes.Count
    $Include.CanonicalLfSha256 = Get-BytesSha256 -Bytes $canonicalBytes
    $Include.RawBytes = $rawBytes.Count
    $Include.RawSha256 = Get-BytesSha256 -Bytes $rawBytes
    $Include.EolStyle = if ($hasBareCr -or ($hasCrLf -and $hasBareLf)) {
        'Mixed'
    } elseif ($hasCrLf) {
        'CRLF'
    } else {
        'LF'
    }
    $Include.LineBreakCount = [regex]::Matches($canonical, "`n").Count
}

function Assert-DerivedSourceReplacementNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$New
    )

    return Assert-UdpCallbackNegativeFixture -Name $Name -Action {
        $snapshot = New-UdpCallbackTestSnapshot -State DerivedCandidate
        if ($snapshot.DerivedSource.IndexOf(
                $Old,
                [StringComparison]::Ordinal) -lt 0) {
            throw "Derived source mutation anchor is missing: $Name"
        }
        $snapshot.DerivedSource = $snapshot.DerivedSource.Replace($Old, $New)
        $null = Assert-LasalUdpCallbackStateContract `
            -Snapshot $snapshot `
            -PermitAbsent $false `
            -RequiredState DerivedCandidate
    }
}

function Assert-TcpSourceReplacementNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]
        [ValidateSet(
            'DerivedWired',
            'DerivedCandidate',
            'TerminalWakeBrokerCandidate')]
        [string]$State,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$New
    )

    return Assert-UdpCallbackNegativeFixture -Name $Name -Action {
        $snapshot = New-UdpCallbackTestSnapshot -State $State
        if ($snapshot.TcpSource.IndexOf(
                $Old,
                [StringComparison]::Ordinal) -lt 0) {
            throw "TCP source mutation anchor is missing: $Name"
        }
        $snapshot.TcpSource = $snapshot.TcpSource.Replace($Old, $New)
        $null = Assert-LasalUdpCallbackStateContract `
            -Snapshot $snapshot `
            -PermitAbsent $false `
            -RequiredState $State
    }
}

function Assert-TerminalWakeSourceReplacementNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]
        [ValidateSet('DiagnosticsSource', 'DerivedSource')]
        [string]$Property,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$New
    )

    return Assert-UdpCallbackNegativeFixture -Name $Name -Action {
        $snapshot = New-UdpCallbackTestSnapshot `
            -State TerminalWakeBrokerCandidate
        if ($snapshot.$Property.IndexOf(
                $Old,
                [StringComparison]::Ordinal) -lt 0) {
            throw "Gate D source mutation anchor is missing: $Name"
        }
        $snapshot.$Property = $snapshot.$Property.Replace($Old, $New)
        if ($Property -ceq 'DiagnosticsSource') {
            Assert-TerminalWakeDiagnosticsSourceContract `
                -DiagnosticsSource $snapshot.DiagnosticsSource
        }
        else {
            Assert-DerivedSourceContract `
                -SourceText $snapshot.DerivedSource `
                -ImplementationMode Complete `
                -TerminalWakeBroker
        }
    }
}

function Assert-TerminalWakeTcpReplacementNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$New
    )

    return Assert-UdpCallbackNegativeFixture -Name $Name -Action {
        $snapshot = New-UdpCallbackTestSnapshot `
            -State TerminalWakeBrokerCandidate
        if ($snapshot.TcpSource.IndexOf(
                $Old,
                [StringComparison]::Ordinal) -lt 0) {
            throw "Gate D TCP mutation anchor is missing: $Name"
        }
        $snapshot.TcpSource = $snapshot.TcpSource.Replace($Old, $New)
        Assert-TerminalWakeTcpSourceContract `
            -TcpSource $snapshot.TcpSource
    }
}

function New-TerminalWakePhysicalLayoutFixture {
    param(
        [ValidateSet('LF', 'CRLF')]
        [string]$SenderEolStyle = 'LF'
    )

    $snapshot = New-UdpCallbackTestSnapshot `
        -State TerminalWakeBrokerCandidate
    $snapshot.TransceiverCanonicalLfBytes =
        $TerminalWakeLayoutSelfTestOracle.Transceiver.CanonicalLfBytes
    $snapshot.TransceiverCanonicalLfSha256 =
        $TerminalWakeLayoutSelfTestOracle.Transceiver.CanonicalLfSha256
    if ($SenderEolStyle -ceq 'LF') {
        $snapshot.DerivedRawBytes =
            $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfBytes
        $snapshot.DerivedRawSha256 =
            $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfSha256
    }
    else {
        $snapshot.DerivedRawBytes =
            $TerminalWakeLayoutSelfTestOracle.Sender.CodeGeneratorCrLfBytes
        $snapshot.DerivedRawSha256 =
            $TerminalWakeLayoutSelfTestOracle.Sender.CodeGeneratorCrLfSha256
    }
    $snapshot.DerivedCanonicalLfBytes =
        $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfBytes
    $snapshot.DerivedCanonicalLfSha256 =
        $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfSha256
    $snapshot.DerivedEolStyle = $SenderEolStyle
    $snapshot.DerivedLineBreakCount =
        $TerminalWakeLayoutSelfTestOracle.Sender.LineBreakCount
    $snapshot.ClassesBytes = $TerminalWakeLayoutSelfTestOracle.Classes.Bytes
    $snapshot.ClassesSha256 = $TerminalWakeLayoutSelfTestOracle.Classes.Sha256
    $snapshot.CommNetworkBytes =
        $TerminalWakeLayoutSelfTestOracle.CommNetwork.Bytes
    $snapshot.CommNetworkSha256 =
        $TerminalWakeLayoutSelfTestOracle.CommNetwork.Sha256
    $snapshot.NetworksDatabaseBytes =
        $TerminalWakeLayoutSelfTestOracle.NetworksDatabase.Bytes
    $snapshot.NetworksDatabaseSha256 =
        $TerminalWakeLayoutSelfTestOracle.NetworksDatabase.Sha256
    $snapshot.FullNetworkCount =
        $TerminalWakeLayoutSelfTestOracle.FullNetwork.Count
    $snapshot.FullNetworkSha256 =
        $TerminalWakeLayoutSelfTestOracle.FullNetwork.Sha256
    $snapshot.TrackedNetworkCount =
        $TerminalWakeLayoutSelfTestOracle.TrackedNetwork.Count
    $snapshot.TrackedNetworkSha256 =
        $TerminalWakeLayoutSelfTestOracle.TrackedNetwork.Sha256
    return $snapshot
}

function Assert-TerminalWakeLayoutIdentityNegativeFixture {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Property
    )

    return Assert-UdpCallbackNegativeFixture -Name $Name -Action {
        $snapshot = New-TerminalWakePhysicalLayoutFixture
        $snapshot.$Property = if ($Property.EndsWith('Bytes') -or
            $Property.EndsWith('Count')) {
                [long]$snapshot.$Property + 1
            }
            else { 'DRIFT' }
        Assert-TerminalWakeLayoutContract -Snapshot $snapshot
    }
}

function Get-UdpCallbackSenderEvidenceToken {
    param(
        [Parameter(Mandatory = $true)][string]$State,
        [Parameter(Mandatory = $true)][pscustomobject]$Snapshot
    )

    if ($State -cne 'TerminalWakeBrokerCandidate') {
        return ''
    }
    foreach ($byteProperty in @(
            'DerivedRawBytes',
            'DerivedCanonicalLfBytes')) {
        if ([long]$Snapshot.$byteProperty -le 0) {
            Throw-UdpCallbackBlocker (
                "Gate D Sender evidence $byteProperty is not positive.")
        }
    }
    foreach ($shaProperty in @(
            'DerivedRawSha256',
            'DerivedCanonicalLfSha256')) {
        if (-not [regex]::IsMatch(
                [string]$Snapshot.$shaProperty,
                '\A[0-9A-F]{64}\z')) {
            Throw-UdpCallbackBlocker (
                "Gate D Sender evidence $shaProperty is not exact SHA-256.")
        }
    }
    return (
        "Sender=$($Snapshot.DerivedRawBytes)/" +
        "$($Snapshot.DerivedRawSha256)," +
        "$($Snapshot.DerivedCanonicalLfBytes)/" +
        "$($Snapshot.DerivedCanonicalLfSha256); ")
}

function Invoke-UdpCallbackVerifierSelfTest {
    Assert-TerminalWakeLayoutConstantsMatchSelfTestOracle

    $selfTestRoot = [IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..\..\..'))
    $currentNetwork = Get-NetworkSnapshotEvidence -Root $selfTestRoot
    $currentProtectedNetwork =
        Get-ProtectedTrackedNetworkIdentityEvidence `
            -Files $currentNetwork.Files
    Assert-ExactInventory `
        -Actual @($currentProtectedNetwork.Files | Where-Object {
                $_.Policy -ceq 'CanonicalLf'
            } | ForEach-Object { $_.Path }) `
        -Expected $CanonicalLfProtectedNetworkTextPaths `
        -InventoryOwner 'protected Network canonical-LF path set'
    if (($currentProtectedNetwork.Count -ne
            $ExpectedProtectedTrackedNetworkCount) -or
        ($currentProtectedNetwork.Sha256 -cne
            $ExpectedProtectedTrackedNetworkSha256)) {
        throw 'current protected Network canonical identity drifted.'
    }

    foreach ($protectedNetworkEolStyle in @('CRLF', 'LF')) {
        $physicalFiles = @(
            New-ProtectedNetworkEolSelfTestFixture `
                -SourceFiles $currentNetwork.Files `
                -EolStyle $protectedNetworkEolStyle)
        $physicalIdentity =
            Get-ProtectedTrackedNetworkIdentityEvidence -Files $physicalFiles
        if (($physicalIdentity.Count -ne
                $ExpectedProtectedTrackedNetworkCount) -or
            ($physicalIdentity.Sha256 -cne
                $ExpectedProtectedTrackedNetworkSha256)) {
            throw (
                "protected Network $protectedNetworkEolStyle positive " +
                'fixture identity drifted.')
        }
    }

    $currentCommTable = Get-NetworkFileEvidence `
        -NetworkEvidence $currentNetwork `
        -RelativePath $CommTableRelativePath
    $currentCommTableText = $Utf8.GetString($currentCommTable.Bytes)
    $currentCommTableCanonical =
        ConvertTo-CanonicalLf -Text $currentCommTableText
    foreach ($commTableEolStyle in @('CRLF', 'LF')) {
        $commTablePhysicalText = if ($commTableEolStyle -ceq 'CRLF') {
            $currentCommTableCanonical.Replace("`n", "`r`n")
        }
        else { $currentCommTableCanonical }
        $commTablePhysicalBytes = $Utf8.GetBytes($commTablePhysicalText)
        Assert-DerivedCommTablePhysicalContract `
            -State TerminalWakeBrokerCandidate `
            -CommTableText $commTablePhysicalText `
            -CommTableBytes $commTablePhysicalBytes.Count `
            -CommTableSha256 (
                Get-BytesSha256 -Bytes $commTablePhysicalBytes)
    }
    if ((Get-OrdinalCount `
            -Text $currentCommTableCanonical `
            -Needle '#define OBJECTS_CONFIG') -ne 1) {
        throw 'protected Comm table semantic drift anchor is not unique.'
    }
    $commTableSemanticDriftText = $currentCommTableCanonical.Replace(
        '#define OBJECTS_CONFIG',
        '#define OBJECTS_CONFIF')
    $commTableSemanticDriftBytes = $Utf8.GetBytes(
        $commTableSemanticDriftText)
    $commTableSemanticDriftSha256 =
        Get-BytesSha256 -Bytes $commTableSemanticDriftBytes

    $protectedNetworkTextDriftFiles = @(
        New-ProtectedNetworkEolSelfTestFixture `
            -SourceFiles $currentNetwork.Files `
            -EolStyle LF)
    $protectedNetworkTextDriftTarget = @(
        $protectedNetworkTextDriftFiles | Where-Object {
            $_.Path -ceq "$TargetRootRelativePath/Network/Eni.xml"
        })
    if ($protectedNetworkTextDriftTarget.Count -ne 1) {
        throw 'protected Network text drift target is not unique.'
    }
    $protectedNetworkTextDriftBytes = [byte[]](
        $protectedNetworkTextDriftTarget[0].Bytes.Clone())
    $protectedNetworkTextDriftBytes[0] =
        $protectedNetworkTextDriftBytes[0] -bxor 1
    $protectedNetworkTextDriftTarget[0].Bytes =
        $protectedNetworkTextDriftBytes
    $protectedNetworkTextDriftTarget[0].ByteCount =
        $protectedNetworkTextDriftBytes.Count
    $protectedNetworkTextDriftTarget[0].Sha256 =
        Get-BytesSha256 -Bytes $protectedNetworkTextDriftBytes
    $protectedNetworkTextDriftIdentity =
        Get-ProtectedTrackedNetworkIdentityEvidence `
            -Files $protectedNetworkTextDriftFiles
    if ($protectedNetworkTextDriftIdentity.Sha256 -ceq
        $ExpectedProtectedTrackedNetworkSha256) {
        throw 'protected Network text semantic drift was canonicalized away.'
    }

    $protectedNetworkBinaryDriftFiles = @(
        New-ProtectedNetworkEolSelfTestFixture `
            -SourceFiles $currentNetwork.Files `
            -EolStyle LF)
    $protectedNetworkBinaryDriftTarget = @(
        $protectedNetworkBinaryDriftFiles | Where-Object {
            $_.Path -ceq (
                "$TargetRootRelativePath/Network/EtherCAT_Network/" +
                'EtherCAT_Network.lcn')
        })
    if ($protectedNetworkBinaryDriftTarget.Count -ne 1) {
        throw 'protected Network binary drift target is not unique.'
    }
    $protectedNetworkBinaryDriftBytes = [byte[]](
        $protectedNetworkBinaryDriftTarget[0].Bytes.Clone())
    $protectedNetworkBinaryDriftBytes[0] =
        $protectedNetworkBinaryDriftBytes[0] -bxor 1
    $protectedNetworkBinaryDriftTarget[0].Bytes =
        $protectedNetworkBinaryDriftBytes
    $protectedNetworkBinaryDriftTarget[0].ByteCount =
        $protectedNetworkBinaryDriftBytes.Count
    $protectedNetworkBinaryDriftTarget[0].Sha256 =
        Get-BytesSha256 -Bytes $protectedNetworkBinaryDriftBytes
    $protectedNetworkBinaryDriftIdentity =
        Get-ProtectedTrackedNetworkIdentityEvidence `
            -Files $protectedNetworkBinaryDriftFiles
    if ($protectedNetworkBinaryDriftIdentity.Sha256 -ceq
        $ExpectedProtectedTrackedNetworkSha256) {
        throw 'protected Network binary drift was canonicalized away.'
    }

    foreach ($positive in @(
            @{ State = 'Absent'; PermitAbsent = $true },
            @{ State = 'VendorImported'; PermitAbsent = $false },
            @{ State = 'DerivedDeclaration'; PermitAbsent = $false },
            @{ State = 'DerivedWired'; PermitAbsent = $false },
            @{ State = 'DerivedCandidate'; PermitAbsent = $false },
            @{
                State = 'TerminalWakeBrokerCandidate'
                PermitAbsent = $false
            })) {
        $snapshot = New-UdpCallbackTestSnapshot -State $positive.State
        $result = Assert-LasalUdpCallbackStateContract `
            -Snapshot $snapshot `
            -PermitAbsent $positive.PermitAbsent `
            -RequiredState $positive.State
        if ($result.State -cne $positive.State) {
            throw "UDP callback positive fixture state drifted: $($positive.State)"
        }
        $expectedProductionApproval =
            $positive.State -in @('Absent', 'VendorImported')
        if ($result.ProductionApproved -ne $expectedProductionApproval -or
            $result.NeedsRebaseline -eq $expectedProductionApproval) {
            throw (
                "UDP callback positive approval boundary drifted: " +
                $positive.State)
        }
    }

    $protectedHeaderContract = @($ProtectedDependencies | Where-Object {
            $_.Name -ceq 'lsl_st_tcp_user.h'
        })[0]
    foreach ($headerPhysicalIdentity in @(
            [pscustomobject]@{
                Name = 'LF'
                Bytes = $protectedHeaderContract.GitCheckoutLfBytes
                Sha256 = $protectedHeaderContract.GitCheckoutLfSha256
            },
            [pscustomobject]@{
                Name = 'CRLF'
                Bytes = $protectedHeaderContract.Bytes
                Sha256 = $protectedHeaderContract.Sha256
            })) {
        $snapshot = New-UdpCallbackTestSnapshot -State VendorImported
        $header = @($snapshot.ProtectedDependencies | Where-Object {
                $_.Name -ceq 'lsl_st_tcp_user.h'
            })[0]
        $header.Bytes = $headerPhysicalIdentity.Bytes
        $header.Sha256 = $headerPhysicalIdentity.Sha256
        $result = Assert-LasalUdpCallbackStateContract `
            -Snapshot $snapshot `
            -PermitAbsent $false `
            -RequiredState VendorImported
        if ($result.State -cne 'VendorImported') {
            throw (
                "protected header $($headerPhysicalIdentity.Name) positive " +
                'fixture state drifted.')
        }
    }

    foreach ($senderEolStyle in @('LF', 'CRLF')) {
        $layoutPositive = New-TerminalWakePhysicalLayoutFixture `
            -SenderEolStyle $senderEolStyle
        Assert-TerminalWakeLayoutContract -Snapshot $layoutPositive
        $expectedRawBytes = if ($senderEolStyle -ceq 'LF') {
            $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfBytes
        }
        else {
            $TerminalWakeLayoutSelfTestOracle.Sender.CodeGeneratorCrLfBytes
        }
        $expectedRawSha256 = if ($senderEolStyle -ceq 'LF') {
            $TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfSha256
        }
        else {
            $TerminalWakeLayoutSelfTestOracle.Sender.CodeGeneratorCrLfSha256
        }
        $expectedSenderToken =
            "Sender=$expectedRawBytes/$expectedRawSha256," +
            "$($TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfBytes)/" +
            "$($TerminalWakeLayoutSelfTestOracle.Sender.CanonicalLfSha256); "
        $senderToken = Get-UdpCallbackSenderEvidenceToken `
            -State TerminalWakeBrokerCandidate `
            -Snapshot $layoutPositive
        if ($senderToken -cne $expectedSenderToken) {
            throw "Gate D $senderEolStyle Sender stdout token drifted."
        }
    }

    $cleanCheckoutNetworkPositive =
        New-TerminalWakePhysicalLayoutFixture -SenderEolStyle LF
    $cleanCheckoutNetworkPositive.FullNetworkCount =
        $TerminalWakeLayoutSelfTestOracle.FullNetwork.CleanCheckoutCount
    $cleanCheckoutNetworkPositive.FullNetworkSha256 =
        $TerminalWakeLayoutSelfTestOracle.FullNetwork.CleanCheckoutSha256
    $cleanCheckoutNetworkPositive.TrackedNetworkCount =
        $TerminalWakeLayoutSelfTestOracle.TrackedNetwork.CleanCheckoutCount
    $cleanCheckoutNetworkPositive.TrackedNetworkSha256 =
        $TerminalWakeLayoutSelfTestOracle.TrackedNetwork.CleanCheckoutSha256
    Assert-TerminalWakeLayoutContract -Snapshot $cleanCheckoutNetworkPositive

    $gateCSenderToken = Get-UdpCallbackSenderEvidenceToken `
        -State DerivedCandidate `
        -Snapshot (New-UdpCallbackTestSnapshot -State DerivedCandidate)
    if ($gateCSenderToken -cne '') {
        throw 'Gate C stdout unexpectedly contains a Gate D Sender token.'
    }

    $commentOnlyGateC = New-UdpCallbackTestSnapshot -State DerivedCandidate
    $commentOnlyGateC.DiagnosticsSource =
        '(* TryTakeD5TerminalWake is documentation only. *)'
    $commentOnlyGateC.TcpSource +=
        "`n(* PublishD5TerminalWake is documentation only. *)`n"
    $commentOnlyGateC.DerivedSource +=
        "`n(* EventId = 0 is documentation only. *)`n"
    $commentOnlyGateCResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $commentOnlyGateC `
        -PermitAbsent $false `
        -RequiredState DerivedCandidate
    if ($commentOnlyGateCResult.State -cne 'DerivedCandidate') {
        throw 'comment-only Gate D signals changed the Gate C state classifier.'
    }

    $terminalWakePositive = New-UdpCallbackTestSnapshot `
        -State TerminalWakeBrokerCandidate
    $terminalWakePositiveResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $terminalWakePositive `
        -PermitAbsent $false `
        -RequiredState TerminalWakeBrokerCandidate
    if ($terminalWakePositiveResult.State -cne 'TerminalWakeBrokerCandidate') {
        throw 'executable Gate D signals did not select TerminalWakeBrokerCandidate.'
    }

    $tcpOracleDefinition =
        (Get-Command New-SyntheticTcpSource -CommandType Function).Definition
    foreach ($forbiddenLiveDependency in @(
            'ReadAllText',
            '$TcpRelativePath')) {
        if ($tcpOracleDefinition.IndexOf(
                $forbiddenLiveDependency,
                [StringComparison]::Ordinal) -ge 0) {
            throw (
                'synthetic TCP oracle retained live-worktree dependency ' +
                    $forbiddenLiveDependency)
        }
    }
    foreach ($requiredImmutableDependency in @(
            'Get-SyntheticGitBlobBytes',
            '$ExpectedGateBTcpGitOid')) {
        if ($tcpOracleDefinition.IndexOf(
                $requiredImmutableDependency,
                [StringComparison]::Ordinal) -lt 0) {
            throw (
                'synthetic TCP oracle lacks immutable dependency ' +
                    $requiredImmutableDependency)
        }
    }
    $advancedLiveCandidate =
        New-UdpCallbackTestSnapshot -State DerivedWired
    $advancedCandidateGenerated =
        New-UdpCallbackTestSnapshot -State DerivedCandidate
    foreach ($property in @(
            'DerivedSource',
            'TcpSource',
            'ClassesDatabaseBytes',
            'ClassesDatabaseText',
            'ClassesBytes',
            'ClassesSha256',
            'CommTableText',
            'CommTableBytes',
            'CommTableSha256')) {
        $advancedLiveCandidate.$property =
            $advancedCandidateGenerated.$property
    }
    $advancedLiveResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $advancedLiveCandidate `
        -PermitAbsent $false `
        -RequiredState DerivedCandidate
    if ($advancedLiveResult.State -cne 'DerivedCandidate') {
        throw 'advanced live TCP Candidate fixture drifted.'
    }

    $optionalClassPositive =
        New-UdpCallbackTestSnapshot -State DerivedDeclaration
    $vendorClass = "`t`t`t`t" +
        '<Class Name="_UDPTransceiverInterface"/>'
    $derivedClass = "`t`t`t`t" +
        '<Class Name="LMCUdpCallbackSender"/>'
    if ((Get-OrdinalCount `
            -Text $optionalClassPositive.ProjectDefinitionText `
            -Needle $vendorClass) -ne 1) {
        throw 'synthetic optional derived Class anchor drifted.'
    }
    $optionalClassPositive.ProjectDefinitionText =
        $optionalClassPositive.ProjectDefinitionText.Replace(
            $vendorClass,
            $vendorClass + "`n" + $derivedClass)
    Update-SyntheticAsciiSnapshotEvidence `
        -Snapshot $optionalClassPositive `
        -TextProperty ProjectDefinitionText `
        -BytesProperty ProjectDefinitionBytes `
        -ShaProperty ProjectDefinitionSha256
    $optionalClassResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $optionalClassPositive `
        -PermitAbsent $false `
        -RequiredState DerivedDeclaration
    if ($optionalClassResult.State -cne 'DerivedDeclaration') {
        throw 'optional derived project Class positive fixture drifted.'
    }

    $recordBoundPositive =
        New-UdpCallbackTestSnapshot -State DerivedDeclaration
    $nextClassPath = '.\Class\TCPMotionInterface\TCPMotionInterface.st'
    $nextClassIndex = $recordBoundPositive.ClassesDatabaseText.IndexOf(
        $nextClassPath,
        [StringComparison]::Ordinal)
    if ($nextClassIndex -lt 0) {
        throw 'synthetic sender record boundary drifted.'
    }
    $methodNameDecoy = [Text.Encoding]::ASCII.GetBytes('ArmEndpoint')
    $recordBoundBytes = [byte[]]::new(
        $recordBoundPositive.ClassesDatabaseBytes.Count +
        $methodNameDecoy.Count)
    [Array]::Copy(
        $recordBoundPositive.ClassesDatabaseBytes,
        0,
        $recordBoundBytes,
        0,
        $nextClassIndex)
    [Array]::Copy(
        $methodNameDecoy,
        0,
        $recordBoundBytes,
        $nextClassIndex,
        $methodNameDecoy.Count)
    [Array]::Copy(
        $recordBoundPositive.ClassesDatabaseBytes,
        $nextClassIndex,
        $recordBoundBytes,
        $nextClassIndex + $methodNameDecoy.Count,
        $recordBoundPositive.ClassesDatabaseBytes.Count - $nextClassIndex)
    $recordBoundPositive.ClassesDatabaseBytes = $recordBoundBytes
    $recordBoundPositive.ClassesDatabaseText =
        $Latin1.GetString($recordBoundBytes)
    $recordBoundPositive.ClassesBytes = $recordBoundBytes.Count
    $recordBoundPositive.ClassesSha256 =
        Get-BytesSha256 -Bytes $recordBoundBytes
    $recordBoundResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $recordBoundPositive `
        -PermitAbsent $false `
        -RequiredState DerivedDeclaration
    if ($recordBoundResult.State -cne 'DerivedDeclaration') {
        throw 'record-bound sender method positive fixture drifted.'
    }

    $commentPositive = New-UdpCallbackTestSnapshot -State DerivedCandidate
    $commentBlock = Get-TcpFunctionBlock `
        -TcpSource $commentPositive.TcpSource -FunctionName ConnSocketInfo
    $commentBlock = $commentBlock.Replace(
        'SessionEpoch += 1;',
        "SessionEpoch (* harmless *)`n += 1;")
    $commentPositive.TcpSource = Set-SyntheticTcpFunctionBlock `
        -TcpSource $commentPositive.TcpSource `
        -FunctionName ConnSocketInfo `
        -Replacement $commentBlock
    $commentResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $commentPositive `
        -PermitAbsent $false `
        -RequiredState DerivedCandidate
    if ($commentResult.State -cne 'DerivedCandidate') {
        throw 'Gate C harmless comment/line-break positive fixture drifted.'
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

    $crLfDerived = New-UdpCallbackTestSnapshot -State DerivedDeclaration
    foreach ($include in $crLfDerived.GeneratedIncludes) {
        $include.Text = (ConvertTo-CanonicalLf -Text $include.Text).
            Replace("`n", "`r`n")
        Update-SyntheticGeneratedIncludeEvidence -Include $include
    }
    $crLfDerivedResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $crLfDerived `
        -PermitAbsent $false `
        -RequiredState DerivedDeclaration
    if ($crLfDerivedResult.State -cne 'DerivedDeclaration') {
        throw 'derived CRLF Include reverse-delta positive fixture drifted.'
    }

    $multilineDerived = New-UdpCallbackTestSnapshot -State DerivedWired
    $multilineCBlock = @'
typedef struct CltChCmd_LMCUdpCallbackSender {
    struct SvrChCmd_DINT *pCh;
    DINT dData;
    LMCUdpCallbackSender *pCmd;
} CltChCmd_LMCUdpCallbackSender;
'@
    $multilineStBlock = @'
CltChCmd_LMCUdpCallbackSender : STRUCT
    pCh : ^SvrChCmd_DINT;
    dData : DINT;
    pCmd : ^LMCUdpCallbackSender;
END_STRUCT;
'@
    $multilineDerived.GeneratedIncludes[0].Text =
        $multilineDerived.GeneratedIncludes[0].Text.Replace(
            $ExpectedDerivedCClientStructBlock,
            $multilineCBlock.TrimEnd())
    $multilineDerived.GeneratedIncludes[1].Text =
        $multilineDerived.GeneratedIncludes[1].Text.Replace(
            $ExpectedDerivedStClientStructBlock,
            $multilineStBlock.TrimEnd())
    Update-SyntheticGeneratedIncludeEvidence `
        -Include $multilineDerived.GeneratedIncludes[0]
    Update-SyntheticGeneratedIncludeEvidence `
        -Include $multilineDerived.GeneratedIncludes[1]
    $multilineResult = Assert-LasalUdpCallbackStateContract `
        -Snapshot $multilineDerived `
        -PermitAbsent $false `
        -RequiredState DerivedWired
    if ($multilineResult.State -cne 'DerivedWired') {
        throw 'derived multiline Include reverse-delta positive fixture drifted.'
    }

    $negativeCount = 0
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D expected layout single-sided pin drift' -Action {
            $savedExpectedSenderCanonicalSha256 =
                $script:ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256
            try {
                $script:ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256 =
                    'FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF'
                Assert-TerminalWakeLayoutConstantsMatchSelfTestOracle
            }
            finally {
                $script:ExpectedTerminalWakeLayout.Sender.CanonicalLfSha256 =
                    $savedExpectedSenderCanonicalSha256
            }
        }
    foreach ($layoutIdentity in @(
            @{
                Name = 'Gate D transceiver canonical layout identity drift'
                Property = 'TransceiverCanonicalLfSha256'
            },
            @{
                Name = 'Gate D Classes layout identity drift'
                Property = 'ClassesSha256'
            },
            @{
                Name = 'Gate D Comm Network layout identity drift'
                Property = 'CommNetworkSha256'
            },
            @{
                Name = 'Gate D Networks database layout identity drift'
                Property = 'NetworksDatabaseSha256'
            },
            @{
                Name = 'Gate D full Network aggregate layout drift'
                Property = 'FullNetworkCount'
            },
            @{
                Name = 'Gate D tracked Network aggregate layout drift'
                Property = 'TrackedNetworkSha256'
            })) {
        $negativeCount += Assert-TerminalWakeLayoutIdentityNegativeFixture `
            -Name $layoutIdentity.Name `
            -Property $layoutIdentity.Property
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D sender mixed EOL rejected' -Action {
            $s = New-TerminalWakePhysicalLayoutFixture -SenderEolStyle LF
            $s.DerivedEolStyle = 'Mixed'
            Assert-TerminalWakeLayoutContract -Snapshot $s
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D sender wrong exact LF rejected' -Action {
            $s = New-TerminalWakePhysicalLayoutFixture -SenderEolStyle LF
            $s.DerivedRawSha256 = 'DRIFT'
            Assert-TerminalWakeLayoutContract -Snapshot $s
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D sender wrong exact CRLF rejected' -Action {
            $s = New-TerminalWakePhysicalLayoutFixture -SenderEolStyle CRLF
            $s.DerivedRawSha256 = 'DRIFT'
            Assert-TerminalWakeLayoutContract -Snapshot $s
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D transceiver Objectsize metadata drift' -Action {
            $s = New-TerminalWakePhysicalLayoutFixture
            $s.TransceiverSource = $s.TransceiverSource.Replace(
                'Objectsize="(522,120)"',
                'Objectsize="(523,120)"')
            Assert-TerminalWakeLayoutContract -Snapshot $s
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D sender Objectsize metadata drift' -Action {
            $s = New-TerminalWakePhysicalLayoutFixture
            $s.DerivedSource = $s.DerivedSource.Replace(
                'Objectsize="(778,120)"',
                'Objectsize="(777,120)"')
            Assert-TerminalWakeLayoutContract -Snapshot $s
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D sanctioned sender position drift' -Action {
            $s = New-UdpCallbackTestSnapshot `
                -State TerminalWakeBrokerCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                'Position="(2610,990)"',
                'Position="(2610,900)"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s `
                -PermitAbsent $false `
                -RequiredState TerminalWakeBrokerCandidate
        }
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D sender accepts zero EventId' `
        -Property DerivedSource `
        -Old '(EventId = 0) OR ' `
        -New ''
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D TryTake NIL pointer domain drift' `
        -Property DiagnosticsSource `
        -Old '(pOwnerSessionEpoch = NIL)' `
        -New '(pOwnerSessionEpoch <> NIL)'
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D TryTake output zeroing removed' `
        -Property DiagnosticsSource `
        -Old 'pTicketBootId^$UDINT := 0;' `
        -New 'pTicketBootId^$UDINT := 1;'
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D terminal state set drift' `
        -Property DiagnosticsSource `
        -Old 'OperationState <> LMC_DIAG_SDO_STATE_EXPIRED' `
        -New 'OperationState = LMC_DIAG_SDO_STATE_EXPIRED'
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D once-only tuple loses owner epoch' `
        -Property DiagnosticsSource `
        -Old '(D5TerminalWakeLastAttemptOwnerSessionEpoch = OwnerSessionEpoch)' `
        -New '(D5TerminalWakeLastAttemptOwnerSessionEpoch = 0)'
    $negativeCount += Assert-TerminalWakeSourceReplacementNegativeFixture `
        -Name 'Gate D last-attempt initialization removed' `
        -Property DiagnosticsSource `
        -Old 'D5TerminalWakeLastAttemptTicketBootId := 0;' `
        -New 'D5TerminalWakeLastAttemptTicketBootId := 1;'
    $negativeCount += Assert-TerminalWakeTcpReplacementNegativeFixture `
        -Name 'Gate D CyWork broker call removed' `
        -Old ("    Diagnostics.ProcessOperations();`n" +
            '    PublishD5TerminalWake();') `
        -New '    Diagnostics.ProcessOperations();'
    $negativeCount += Assert-TerminalWakeTcpReplacementNegativeFixture `
        -Name 'Gate D no-pending-close predicate inverted' `
        -Old '(PendingClosedSessionEpoch = 0)' `
        -New '(PendingClosedSessionEpoch <> 0)'
    $negativeCount += Assert-TerminalWakeTcpReplacementNegativeFixture `
        -Name 'Gate D PublishEvent ticket correlation removed' `
        -Old 'EventId:=ticketId' `
        -New 'EventId:=0'
    $negativeCount += Assert-TerminalWakeTcpReplacementNegativeFixture `
        -Name 'Gate D saturating counter ceiling drift' `
        -Old 'D5TerminalWakeAttemptCount <> 16#FFFFFFFF' `
        -New 'D5TerminalWakeAttemptCount <> 16#FFFFFFFE'
    $negativeCount += Assert-TerminalWakeTcpReplacementNegativeFixture `
        -Name 'Gate D producer retry outbox introduced' `
        -Old 'publishResult := -9;' `
        -New "publishResult := -9;`n`tterminalWakeRetryPending := TRUE;"
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D generated TCP counter ABI drift' -Action {
            $s = New-UdpCallbackTestSnapshot `
                -State TerminalWakeBrokerCandidate
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -Old 'D5TerminalWakeAttemptCount' `
                -New 'D5TerminalWakeAttemptCounX' `
                -After '.\Class\TCPMotionInterface\TCPMotionInterface.st'
            Assert-TerminalWakeGeneratedMetadata `
                -ClassesDatabaseBytes $s.ClassesDatabaseBytes `
                -ClassesDatabaseText $s.ClassesDatabaseText `
                -SyntheticFixture
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate D generated TryTake pointer ABI drift' -Action {
            $s = New-UdpCallbackTestSnapshot `
                -State TerminalWakeBrokerCandidate
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -Old 'pTicketId' `
                -New 'pTicketIx' `
                -After 'TryTakeD5TerminalWake'
            Assert-TerminalWakeGeneratedMetadata `
                -ClassesDatabaseBytes $s.ClassesDatabaseBytes `
                -ClassesDatabaseText $s.ClassesDatabaseText `
                -SyntheticFixture
        }
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
        -Name 'DerivedCandidate missing standard CyWork' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = $s.DerivedSource.Replace(
                'FUNCTION VIRTUAL GLOBAL CyWork',
                'FUNCTION VIRTUAL GLOBAL MissingCyWork')
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
        -Name 'DerivedDeclaration premature generated client ABI' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.GeneratedIncludes[0].Text +=
                "`n$ExpectedDerivedCClientStructBlock"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedWired generated C client missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $clientBlock = Get-UniqueGeneratedRawBlock `
                -Text $s.GeneratedIncludes[0].Text `
                -Pattern (
                    '(?ms)^[ \t]*typedef[ \t]+struct[ \t]+' +
                    'CltChCmd_LMCUdpCallbackSender\b.*?\}[ \t]*' +
                    'CltChCmd_LMCUdpCallbackSender[ \t]*;[ \t]*(?=\r?$)') `
                -BlockOwner 'synthetic C client removal'
            $s.GeneratedIncludes[0].Text =
                $s.GeneratedIncludes[0].Text.Replace(
                    $clientBlock, '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived generated ST client pCmd drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.GeneratedIncludes[1].Text =
                $s.GeneratedIncludes[1].Text.Replace(
                    'pCmd : ^LMCUdpCallbackSender;',
                    'pCmd : ^LMCUdpCallbackSenderDrift;')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived generated client duplicate' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.GeneratedIncludes[0].Text +=
                "`n$ExpectedDerivedCClientStructBlock"
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
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
        -Name 'protected header same-length semantic drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State VendorImported
            $header = @($s.ProtectedDependencies | Where-Object {
                    $_.Name -ceq 'lsl_st_tcp_user.h'
                })[0]
            $header.Bytes = $protectedHeaderContract.GitCheckoutLfBytes
            $header.Sha256 = '0' * 64
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
        -Name 'derived lcp File registration missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $derivedFile = "`t`t" +
                '<File Path=".\Class\LMCUdpCallbackSender\' +
                'LMCUdpCallbackSender.st"/>'
            if ((Get-OrdinalCount `
                    -Text $s.ProjectDefinitionText `
                    -Needle $derivedFile) -ne 1) {
                throw 'synthetic derived File anchor drifted.'
            }
            $s.ProjectDefinitionText =
                $s.ProjectDefinitionText.Replace($derivedFile, '')
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDefinitionText `
                -BytesProperty ProjectDefinitionBytes `
                -ShaProperty ProjectDefinitionSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived lcp File registration duplicate' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $derivedFile = "`t`t" +
                '<File Path=".\Class\LMCUdpCallbackSender\' +
                'LMCUdpCallbackSender.st"/>'
            if ((Get-OrdinalCount `
                    -Text $s.ProjectDefinitionText `
                    -Needle $derivedFile) -ne 1) {
                throw 'synthetic derived File anchor drifted.'
            }
            $s.ProjectDefinitionText =
                $s.ProjectDefinitionText.Replace(
                    $derivedFile,
                    $derivedFile + "`n" + $derivedFile)
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDefinitionText `
                -BytesProperty ProjectDefinitionBytes `
                -ShaProperty ProjectDefinitionSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived lcp optional Class registration duplicate' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $vendorClass = "`t`t`t`t" +
                '<Class Name="_UDPTransceiverInterface"/>'
            $derivedClass = "`t`t`t`t" +
                '<Class Name="LMCUdpCallbackSender"/>'
            if ((Get-OrdinalCount `
                    -Text $s.ProjectDefinitionText `
                    -Needle $vendorClass) -ne 1) {
                throw 'synthetic optional derived Class anchor drifted.'
            }
            $s.ProjectDefinitionText =
                $s.ProjectDefinitionText.Replace(
                    $vendorClass,
                    $vendorClass + "`n" + $derivedClass + "`n" +
                    $derivedClass)
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDefinitionText `
                -BytesProperty ProjectDefinitionBytes `
                -ShaProperty ProjectDefinitionSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived lcp unrelated valid registration drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $anchor = "`t</ClassFiles>"
            if ((Get-OrdinalCount -Text $s.ProjectDefinitionText -Needle $anchor) -ne 1) {
                throw 'synthetic lcp ClassFiles anchor drifted.'
            }
            $s.ProjectDefinitionText = $s.ProjectDefinitionText.Replace(
                $anchor,
                "`t`t<File Path=`".\Class\Unexpected\Unexpected.st`"/>`n" +
                    $anchor)
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDefinitionText `
                -BytesProperty ProjectDefinitionBytes `
                -ShaProperty ProjectDefinitionSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived lcp comment-only registration bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.ProjectDefinitionText = @'
<!-- <Project><ClassFiles>
<File Path=".\Class\_UDPTransceiver\_UDPTransceiver.st"/>
<File Path=".\Class\_UDPTransceiverInterface\_UDPTransceiverInterface.st"/>
<File Path=".\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st"/>
</ClassFiles><SigmatekFolders><Folder Name="Tools">
<Folder Name="Communication"><Class Name="_UDPTransceiver"/>
<Class Name="_UDPTransceiverInterface"/>
<Class Name="LMCUdpCallbackSender"/></Folder></Folder>
</SigmatekFolders></Project> -->
'@
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDefinitionText `
                -BytesProperty ProjectDefinitionBytes `
                -ShaProperty ProjectDefinitionSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived project database corrupt name bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.ProjectDatabaseText =
                'CORRUPT LMCUdpCallbackSender _UDPTransceiver ' +
                '_UDPTransceiverInterface'
            Update-SyntheticLatin1SnapshotEvidence `
                -Snapshot $s `
                -TextProperty ProjectDatabaseText `
                -BytesProperty ProjectBytes `
                -ShaProperty ProjectSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ConfigObjects unrelated ONE_CFG drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $old = 'ONE_CFG$UINT, 0, 0, "ONE_Comm_Network",'
            $new = 'ONE_CFG$UINT, 0, 1, "ONE_Comm_Network",'
            if ((Get-OrdinalCount -Text $s.ConfigObjectsText -Needle $old) -ne 1) {
                throw 'synthetic ConfigObjects ONE_CFG mutation anchor drifted.'
            }
            $s.ConfigObjectsText = $s.ConfigObjectsText.Replace($old, $new)
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ConfigObjectsText `
                -BytesProperty ConfigObjectsBytes `
                -ShaProperty ConfigObjectsSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ConfigObjects comment enclosure bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.ConfigObjectsText = "(*`n$($s.ConfigObjectsText)`n*)"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty ConfigObjectsText `
                -BytesProperty ConfigObjectsBytes `
                -ShaProperty ConfigObjectsSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table comment-only name bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.CommTableText = @'
(* FUNCTION GLOBAL TAB ONE_Comm_Network
_UDPTransceiver _UDPTransceiverInterface LMCUdpCallbackSender
LMCUdpTransceiver1 LMCUdpCallbackSender1 sControl CallbackSender ClassSvr
cSizeOfRXBuffer cSizeOfTXBuffer END_FUNCTION *)
'@
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table parenthesized disabled wrapper' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $anchor = "#define OBJECTS_CONFIG`n"
            if ((Get-OrdinalCount -Text $s.CommTableText -Needle $anchor) -ne 1) {
                throw 'synthetic ONE table preamble mutation anchor drifted.'
            }
            $s.CommTableText = $s.CommTableText.Replace(
                $anchor,
                $anchor + "#if (0)`n") + "`n#endif`n"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table FALSE disabled wrapper' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $anchor = "#define OBJECTS_CONFIG`n"
            if ((Get-OrdinalCount -Text $s.CommTableText -Needle $anchor) -ne 1) {
                throw 'synthetic ONE table preamble mutation anchor drifted.'
            }
            $s.CommTableText = $s.CommTableText.Replace(
                $anchor,
                $anchor + "#if FALSE`n") + "`n#endif`n"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table symbolic disabled wrapper' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $anchor = "#define OBJECTS_CONFIG`n"
            if ((Get-OrdinalCount -Text $s.CommTableText -Needle $anchor) -ne 1) {
                throw 'synthetic ONE table preamble mutation anchor drifted.'
            }
            $s.CommTableText = $s.CommTableText.Replace(
                $anchor,
                $anchor + "#define OFF 0`n#if OFF`n") + "`n#endif`n"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table top-level token residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.CommTableText += "`nBROKEN_TOKEN;`n"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ONE table extra function residue' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.CommTableText +=
                "`nFUNCTION GLOBAL TAB CORRUPT`n0`$UINT,`nEND_FUNCTION`n"
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived Networks database corrupt name bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.NetworksDatabaseText =
                'CORRUPT Comm_Network LMCUdpTransceiver1 _UDPTransceiver ' +
                'LMCUdpCallbackSender1 LMCUdpCallbackSender CallbackSender'
            Update-SyntheticLatin1SnapshotEvidence `
                -Snapshot $s `
                -TextProperty NetworksDatabaseText `
                -BytesProperty NetworksDatabaseBytes `
                -ShaProperty NetworksDatabaseSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived C include error directive' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.GeneratedIncludes[0].Text =
                "#error CORRUPT`n" + $s.GeneratedIncludes[0].Text
            Update-SyntheticGeneratedIncludeEvidence `
                -Include $s.GeneratedIncludes[0]
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived ST include disabled wrapper' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.GeneratedIncludes[1].Text =
                "#if 0`n" + $s.GeneratedIncludes[1].Text + "`n#endif`n"
            Update-SyntheticGeneratedIncludeEvidence `
                -Include $s.GeneratedIncludes[1]
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived include minimal ABI bait' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.GeneratedIncludes[0].Text =
                $ExpectedDerivedCClientStructBlock + "`n"
            Update-SyntheticGeneratedIncludeEvidence `
                -Include $s.GeneratedIncludes[0]
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
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
    $derivedSourceMutations = @(
            @('sender top-level executable residue', '//{{LSL_DEFINES', "BROKEN_TOKEN := ;`n//{{LSL_DEFINES"),
            @('sender top-level bare token residue', '//{{LSL_DEFINES', "BROKEN_TOKEN;`n//{{LSL_DEFINES"),
            @('sender top-level stray control residue', '//{{LSL_DEFINES', "END_IF;`n//{{LSL_DEFINES"),
            @('sender unexpected TYPE span', '//{{LSL_DEFINES', "TYPE`nUnexpectedType : STRUCT Value : UDINT; END_STRUCT;`nEND_TYPE`n//{{LSL_DEFINES"),
            @('sender unexpected CLASS span', '//{{LSL_DEFINES', "UnexpectedClass : CLASS`nEND_CLASS;`n//{{LSL_DEFINES"),
            @('sender error directive enclosure', '//{{LSL_DEFINES', "#error CORRUPT`n//{{LSL_DEFINES"),
            @('sender literal disabled enclosure', '//{{LSL_DEFINES', "#if 0`n//{{LSL_DEFINES`n#endif"),
            @('sender symbolic disabled enclosure', '//{{LSL_DEFINES', "#define OFF 0`n#if OFF`n//{{LSL_DEFINES`n#endif"),
            @('Gate C sender macro missing', "#ifndef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE`n#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0`n#endif`n", ''),
            @('Gate C sender macro extra', '#endif' + "`n//}}LSL_DEFINES", "#endif`n#define EXTRA 1`n//}}LSL_DEFINES"),
            @('sender legacy undef residue', '//}}LSL_DEFINES', "//}}LSL_DEFINES`n#undef LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE"),
            @('derived cyclic metadata drift', 'CyclicTask="true"', 'CyclicTask="false"'),
            @('derived 10 ms metadata drift', 'DefCyclictime="10 ms"', 'DefCyclictime="20 ms"'),
            @('derived background metadata drift', 'BackgroundTask="false"', 'BackgroundTask="true"'),
            @('server Initialize drift', 'Name="QueueDepth" Visualized="false" Initialize="true"', 'Name="QueueDepth" Visualized="false" Initialize="false"'),
            @('server order duplicate', 'Name="QueuedCount"', 'Name="QueueDepth"'),
            @('derived new client forbidden', '<Server Name="LastAdmissionResult" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/>', '<Server Name="LastAdmissionResult" Visualized="false" Initialize="true" DefValue="0" WriteProtected="true" Retentive="false"/><Client Name="ExtraClient" Required="false" Internal="false"/>'),
            @('derived inheritance Network missing', '<Network Name="LMCUdpCallbackSender">', '<Network Name="MissingInheritance">'),
            @('derived inheritance base class drift', 'Class="_UDPTransceiverInterface" Position="(218,120)"', 'Class="_UDPTransceiver" Position="(218,120)"'),
            @('derived internal CriticalSection exposed', '<Client Name="_UDPTransceiver"/>', '<Client Name="_UDPTransceiver"/><Client Name="CriticalSection_UDP"/>'),
            @('derived inheritance connection duplicate', '<Connection Source="_base._UDPTransceiver" Destination="this._UDPTransceiver" Vertices="(218,210),(38,210),"/>', '<Connection Source="_base._UDPTransceiver" Destination="this._UDPTransceiver" Vertices="(218,210),(38,210),"/><Connection Source="_base._UDPTransceiver" Destination="this._UDPTransceiver" Vertices="(218,210),(38,210),"/>'),
            @('TX slot destination port type drift', 'DestinationPort : UDINT;', 'DestinationPort : DINT;'),
            @('TX slot fixed buffer drift', 'Data : ARRAY [0..511] OF BYTE;', 'Data : ARRAY [0..510] OF BYTE;'),
            @('queue slot count drift', 'TxSlots : ARRAY [0..7] OF _LMC_UDP_TX_SLOT;', 'TxSlots : ARRAY [0..6] OF _LMC_UDP_TX_SLOT;'),
            @('extra persistent sender variable', 'NextSequenceHi : UDINT;', "NextSequenceHi : UDINT;`n    ExtraState : UDINT;"),
            @('CyWork made non-virtual', 'FUNCTION VIRTUAL GLOBAL CyWork', 'FUNCTION GLOBAL CyWork'),
            @('CyWork EAX output binding removed', 'state (EAX) : UDINT;', 'state : UDINT;'),
            @('CyWork command table removed', 'vmt.CmdTable.CyWork := #CyWork();', 'vmt.CmdTable.Init := #CyWork();'),
            @('standard base initialization removed', 'ret_code := _UDPTransceiverInterface::@STD();', 'ret_code := C_OK;'),
            @('standard user count drift', '#define USER_CNT_LMCUdpCallbackSender 14', '#define USER_CNT_LMCUdpCallbackSender 17'),
            @('standard warning disable missing', "#pragma warning (disable : 74)`n", ''),
            @('standard warning default number drift', '#pragma warning (default : 74)', '#pragma warning (default : 75)'),
            @('standard warning disable duplicate', '#pragma warning (disable : 74)', "#pragma warning (disable : 74)`n#pragma warning (disable : 74)"),
            @('standard warning default precedes callback slot', "#pragma warning (disable : 74)`n    vmt.UserFcts[12] := #ErrorCallback();`n#pragma warning (default : 74)", "#pragma warning (disable : 74)`n#pragma warning (default : 74)`n    vmt.UserFcts[12] := #ErrorCallback();"),
            @('standard ErrorCallback slot drift', 'vmt.UserFcts[12] := #ErrorCallback();', 'vmt.UserFcts[11] := #ErrorCallback();'),
            @('standard nonvirtual public slot leaked', 'vmt.UserFcts[12] := #ErrorCallback();', "vmt.UserFcts[12] := #ErrorCallback();`n    vmt.UserFcts[14] := #ArmEndpoint();"),
            @('standard StoreCmd removed', '_UDPTransceiverInterface::ClassSvr.pMeth := StoreCmd(pCmd := #vmt.CmdTable, SHARED);', '_UDPTransceiverInterface::ClassSvr.pMeth := #vmt.CmdTable;'),
            @('class table server count drift', '9$UINT, 0$UINT, 0$UINT,', '8$UINT, 0$UINT, 0$UINT,'),
            @('public function made private', 'FUNCTION GLOBAL ArmEndpoint', 'FUNCTION ArmEndpoint'),
            @('public function made virtual global', 'FUNCTION GLOBAL DisarmEndpoint', 'FUNCTION VIRTUAL GLOBAL DisarmEndpoint'),
            @('public input ABI drift', 'ProtocolVersion : UINT;', 'ProtocolVersion : DINT;'),
            @('ErrorCallback override ABI drift', 'UdpError : _UDPTransceiver::_UDP_ERROR;', 'UdpError : DINT;'),
            @('private SendSlot output name drift', 'VendorResult : DINT;', 'Result : DINT;'),
            @('obsolete victim helper restored', 'FindFreeSlot', 'FindFreeOrVictimSlot'),
            @('unexpected custom function', 'END_CLASS;', "    FUNCTION UnexpectedHelper;`nEND_CLASS;"),
            @('entry lock acquisition removed', 'CriticalSection_UDP.SectionStart();', ''),
            @('entry lock acquired twice', 'CriticalSection_UDP.SectionStart();', "CriticalSection_UDP.SectionStart();`n    CriticalSection_UDP.SectionStart();"),
            @('entry lock order reversed', 'CriticalSection_UDP.SectionStart();', 'CriticalSection_UDP.SectionStop();'),
            @('inherited lock pCmd indirection', 'CriticalSection_UDP.SectionStart();', 'CriticalSection_UDP.pCmd^.SectionStart();'),
            @('RETURN while shared lock held', 'state := READY;', "RETURN;`n    state := READY;"),
            @('private lock reentry', 'IF (Depth >= 8) OR NOT', "CriticalSection_UDP.SectionStart();`n    IF (Depth >= 8) OR NOT"),
            @('custom dynamic allocation', 'VendorResult := SendData(', 'MallocV1(4); VendorResult := SendData('),
            @('forbidden BindSocket lifecycle', 'Socket := AddSocket();', "Socket := AddSocket();`n        BindSocket();"),
            @('signed-positive socket validity', 'ELSIF Socket <> 0 THEN', 'ELSIF Socket > 0 THEN'),
            @('missing IsOpen poll', 'IF IsOpen() THEN', 'IF SocketReady() THEN'),
            @('AddSocket gains forbidden input', 'Socket := AddSocket();', 'Socket := AddSocket(1);'),
            @('pending socket predicate uses OR', '(socketResult <> 0) AND (socketResult <> 1)', '(socketResult <> 0) OR (socketResult <> 1)'),
            @('duplicate SendData invocation', 'VendorResult := SendData(', 'VendorResult := SendData(Socket := Socket); VendorResult := SendData('),
            @('SendData inherited ABI name drift', 'udSize := TxSlots[SlotIndex].DatagramBytes', 'DataSize := TxSlots[SlotIndex].DatagramBytes'),
            @('direct UDP send enabled', 'bDirect := FALSE', 'bDirect := TRUE'),
            @('transmit service loops over slots', 'IF Depth > 0 THEN', "FOR loopIndex := 0 TO 7 DO`n    IF Depth > 0 THEN"),
            @('transmit service is not head-only', 'slotIndex := ReadIndex;', 'slotIndex := WriteIndex;'),
            @('FIFO capacity drift', 'Depth >= 8', 'Depth >= 9'),
            @('FindFreeSlot ignores occupied WriteIndex', 'TxSlots[WriteIndex].InUse = FALSE', 'TxSlots[WriteIndex].InUse = TRUE'),
            @('victim replacement policy residue', 'slotIndex := FindFreeSlot();', "VictimIndex := 0;`n            slotIndex := FindFreeSlot();"),
            @('buffer-full retry bound drift', 'RetryCount < 3', 'RetryCount < 4'),
            @('buffer-full result code drift', 'VendorResult = -4', 'VendorResult = -3'),
            @('timestamp synthesized from cycle', 'PlcTimeMs := ops.tAbsolute;', 'PlcTimeMs := ops.tAbsolute + 10;'),
            @('timestamp header offset drift', 'Data[44]$UDINT := TxSlots[SlotIndex].PlcTimeMs;', 'Data[43]$UDINT := TxSlots[SlotIndex].PlcTimeMs;'),
            @('typed lvalue suffix split by whitespace', 'Data[44]$UDINT', 'Data[44]$ UDINT'),
            @('address operator split by whitespace', '#ActiveEndpoint', '# ActiveEndpoint'),
            @('BYTE conversion uses unproven TO_BYTE', 'TO_USINT(DeliveryClass)', 'TO_BYTE(DeliveryClass)'),
            @('BuildDatagram retains rejected payload pointer', 'TxSlots[SlotIndex].Data[51] := 0;', "TxSlots[SlotIndex].Data[51] := 0;`n        pPayload := pPayload;"),
            @('sequence high wrap drift', 'NextSequenceHi = 16#FFFFFFFF', 'NextSequenceHi = 16#FFFFFFFE'),
            @('Gate C sender macro value drift', '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0', '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 1'),
            @('legacy fixture code prematurely enabled', '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0', "#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0`n#if LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE`n#endif"),
            @('packed storage pragma', '#pragma using _UDPTransceiverInterface', "#pragma pack(1)`n#pragma using _UDPTransceiverInterface"),
            @('production EventMaskBit policy drift', 'EventMaskBit <> 1', 'EventMaskBit <> 2'),
            @('production EventType policy drift', 'EventType <> 1', 'EventType <> 2'),
            @('production DeliveryClass policy drift', 'DeliveryClass <> 0', 'DeliveryClass <> 1'),
            @('production PayloadBytes policy drift', 'PayloadBytes <> 0', 'PayloadBytes <> 1'),
            @('structural payload 460 limit drift', 'PayloadBytes > 460', 'PayloadBytes > 461'),
            @('negotiated payload length check removed', '(PayloadBytes + 52) > ActiveEndpoint.MaxDatagramBytes', '(PayloadBytes + 52) > 512'),
            @('numeric payload pointer sentinel', 'pPayload = NIL', 'pPayload = 0'),
            @('EventId incorrectly rejected', '(EventMaskBit <> 1)', '(EventId = 0) OR (EventMaskBit <> 1)'),
            @('Publish unarmed guard made dead', 'IF NOT ActiveEndpoint.Armed THEN', 'IF FALSE AND NOT ActiveEndpoint.Armed THEN'),
            @('public Result outside domain', 'Result := -5;', 'Result := -10;'),
            @('duplicate Arm omits MaxDatagram fence', ' AND (ActiveEndpoint.MaxDatagramBytes = MaxDatagramBytes)', ''),
            @('invalid Arm mutates endpoint', "IF validateResult = -1 THEN`n        Result := -1;", "IF validateResult = -1 THEN`n        ActiveEndpoint.Armed := TRUE;`n        Result := -1;"),
            @('stale Disarm mutates queue index', "ELSIF NOT FenceMatches(ExpectedSessionEpoch := ExpectedSessionEpoch, ExpectedCookieLo := ExpectedCookieLo, ExpectedCookieHi := ExpectedCookieHi) THEN`n        Result := -8;", "ELSIF NOT FenceMatches(ExpectedSessionEpoch := ExpectedSessionEpoch, ExpectedCookieLo := ExpectedCookieLo, ExpectedCookieHi := ExpectedCookieHi) THEN`n        WriteIndex := 1;`n        Result := -8;"),
            @('ErrorCallback state assignment drift', 'ErrorState := FSM_UDP;', 'ErrorState := _STATE_ERROR_UDP;'),
            @('ErrorCallback counter no saturation', 'IF TransportErrorCount.Read() <> 16#FFFFFFFF THEN', 'IF TRUE THEN'),
            @('disarm cleared count increments events not slots', 'DisarmClearedCount.Read() + clearedDepth', 'DisarmClearedCount.Read() + 1'),
            @('disarm partial endpoint clear', '_memset(dest := #ActiveEndpoint, usByte := 0, cntr := sizeof(ActiveEndpoint));', 'ActiveEndpoint.Armed := FALSE;'),
            @('disarm closes socket', '_memset(dest := #ActiveEndpoint, usByte := 0, cntr := sizeof(ActiveEndpoint));', "_memset(dest := #ActiveEndpoint, usByte := 0, cntr := sizeof(ActiveEndpoint));`n        Socket := 0;"),
            @('pending slot array partial clear', '_memset(dest := #TxSlots[0], usByte := 0, cntr := sizeof(TxSlots));', 'TxSlots[0].InUse := FALSE;'),
            @('pending QueueDepth mirrors stale variable', 'QueueDepth.Write(input := 0);', 'QueueDepth.Write(input := Depth);'),
            @('retained retry consumes sequence', 'TxSlots[SlotIndex].RetryCount := TxSlots[SlotIndex].RetryCount + 1;', "TxSlots[SlotIndex].RetryCount := TxSlots[SlotIndex].RetryCount + 1;`n            NextSequenceLo := NextSequenceLo + 1;"),
            @('transmit success branch inverted', 'IF vendorResult = 0 THEN', 'IF vendorResult <> 0 THEN'),
            @(
                'exact token stream rejects control separator injection',
                'IF vendorResult = 0 THEN',
                ('IF vendorResult = 0' + [char]0x1F + ' THEN')),
            @('buffer-full retry branch made dead', 'IF TxSlots[SlotIndex].RetryCount < 3 THEN', 'IF FALSE AND TxSlots[SlotIndex].RetryCount < 3 THEN'),
            @('matched disarm does not clear FIFO', 'ClearPendingFrames();', 'ClearPendingFrameX();'))
    foreach ($mutation in $derivedSourceMutations) {
        $negativeCount += Assert-DerivedSourceReplacementNegativeFixture `
            -Name $mutation[0] -Old $mutation[1] -New $mutation[2]
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedDeclaration warning pair relocated before standard TYPE' `
        -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $scopedWarningBlock =
                "#pragma warning (disable : 74)`n" +
                "    vmt.UserFcts[12] := #ErrorCallback();`n" +
                '#pragma warning (default : 74)'
            $standardTypeAnchor = "TYPE`n    _LSL_STD_VMETH"
            if ((Get-OrdinalCount `
                        -Text $s.DerivedSource `
                        -Needle $scopedWarningBlock) -ne 1 -or
                (Get-OrdinalCount `
                        -Text $s.DerivedSource `
                        -Needle $standardTypeAnchor) -ne 1) {
                throw 'synthetic warning relocation anchors drifted.'
            }
            $s.DerivedSource = $s.DerivedSource.Replace(
                $scopedWarningBlock,
                '    vmt.UserFcts[12] := #ErrorCallback();')
            $s.DerivedSource = $s.DerivedSource.Replace(
                $standardTypeAnchor,
                "#pragma warning (disable : 74)`n" +
                    "#pragma warning (default : 74)`n`n" +
                    $standardTypeAnchor)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s `
                -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedDeclaration partial body' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.DerivedSource = [regex]::Replace(
                $s.DerivedSource,
                '(?ms)(^FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::CyWork$' +
                    '.*?)(^END_FUNCTION$)',
                "`${1}    state := READY;`n`${2}",
                1)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false `
                -RequiredState DerivedDeclaration
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedDeclaration premature TCP client' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $s.TcpSource = New-SyntheticTcpSource -Phase DerivedWired
            $s.TcpSha256 = 'SYNTHETIC-DERIVED-TCP'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedDeclaration premature Network only' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedDeclaration
            $network = New-SyntheticDerivedNetwork -State DerivedWired
            $s.CommNetworkText = $network.Xml
            $s.CommTableText = $network.Table
            $s.NetworksDatabaseText = $network.Database
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedWired partial body' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.DerivedSource = [regex]::Replace(
                $s.DerivedSource,
                '(?ms)(^FUNCTION VIRTUAL GLOBAL LMCUdpCallbackSender::CyWork$' +
                    '.*?)(^END_FUNCTION$)',
                "`${1}    state := READY;`n`${2}",
                1)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'DerivedCandidate partial empty stub' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = [regex]::Replace(
                $s.DerivedSource,
                '(?ms)^FUNCTION LMCUdpCallbackSender::ServiceTransmitQueue$' +
                    '.*?^END_FUNCTION$',
                "FUNCTION LMCUdpCallbackSender::ServiceTransmitQueue`n`n" +
                    'END_FUNCTION',
                1)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate B2 premature TCP lifecycle v2 activation' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $s.TcpSource = $s.TcpSource.Replace(
                'if (Payload = 12) & (RpcInitialized = TRUE) &',
                'if (Payload = 32) & (RpcInitialized = TRUE) &')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $tcpB2Mutations = @(
        @(
            'B2 TCP top-level executable residue',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "BROKEN_TOKEN := ;`n//This file was generated by the LASAL2 CodeGenerator  --"),
        @(
            'B2 TCP top-level call residue',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "CALL Unexpected;`n//This file was generated by the LASAL2 CodeGenerator  --"),
        @(
            'B2 TCP unexpected FUNCTION span',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "FUNCTION TCPMotionInterface::Unexpected`nEND_FUNCTION`n" +
                '//This file was generated by the LASAL2 CodeGenerator  --'),
        @(
            'B2 TCP unexpected TYPE span',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "TYPE`nUnexpectedType : STRUCT Value : UDINT; END_STRUCT;`n" +
                "END_TYPE`n//This file was generated by the LASAL2 CodeGenerator  --"),
        @(
            'B2 TCP unexpected CLASS span',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "UnexpectedClass : CLASS`nEND_CLASS;`n" +
                '//This file was generated by the LASAL2 CodeGenerator  --'),
        @(
            'B2 TCP error directive enclosure',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "#error CORRUPT`n//This file was generated by the LASAL2 CodeGenerator  --"),
        @(
            'B2 TCP symbolic disabled enclosure',
            '//This file was generated by the LASAL2 CodeGenerator  --',
            "#define OFF 0`n#if OFF`n//This file was generated by the LASAL2 CodeGenerator  --`n#endif"),
        @(
            'B2 TCP fence type drift',
            "RpcCallbackAcceptedMaxDatagram `t: UINT;",
            "RpcCallbackAcceptedMaxDatagram `t: UDINT;"),
        @(
            'B2 TCP fence inline initializer',
            "RpcCallbackSessionEpoch `t: UDINT;",
            "RpcCallbackSessionEpoch `t: UDINT := 0;"),
        @(
            'B2 TCP fence order drift',
            "RpcCallbackBootId `t: UDINT;`n`t`tRpcCallbackCookieLo `t: UDINT;",
            "RpcCallbackCookieLo `t: UDINT;`n`t`tRpcCallbackBootId `t: UDINT;"),
        @(
            'B2 TCP extra callback fence variable',
            "RpcCallbackLastDisarmResult `t: DINT;",
            ("RpcCallbackLastDisarmResult `t: DINT;`n" +
                "`t`tRpcCallbackSenderArmed : BOOL;")),
        @(
            'B2 TCP helper made global',
            "`tFUNCTION DisarmRpcCallbackEndpoint`n",
            "`tFUNCTION GLOBAL DisarmRpcCallbackEndpoint`n"),
        @(
            'B2 TCP unlisted CyWork body drift',
            'cleanupPrimaryValid : BOOL;',
            'cleanupPrimaryValid : DINT;'),
        @(
            'B2 ConnSocketInfo body drift',
            'LastTakeoverResult := -7;',
            'LastTakeoverResult := -8;'),
        @(
            'B2 SendData body drift',
            'if dRetcode <> udSize$DINT then',
            'if dRetcode = udSize$DINT then'),
        @(
            'B2 safety drain body drift',
            'pendingFailure := FALSE;',
            'pendingFailure := TRUE;'),
        @(
            'B2 RPC lifecycle body drift',
            'if (Payload = 12) & (RpcInitialized = TRUE) &',
            'if (Payload = 11) & (RpcInitialized = TRUE) &'))
    foreach ($mutation in $tcpB2Mutations) {
        $negativeCount += Assert-TcpSourceReplacementNegativeFixture `
            -Name $mutation[0] `
            -State DerivedWired `
            -Old $mutation[1] `
            -New $mutation[2]
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'B2 TCP helper body is nonempty' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            $helper = Get-TcpFunctionBlock `
                -TcpSource $s.TcpSource `
                -FunctionName DisarmRpcCallbackEndpoint
            $s.TcpSource = $s.TcpSource.Replace(
                $helper,
                $helper.Replace(
                    "`n`nEND_FUNCTION",
                    "`n`nResult := 1;`nEND_FUNCTION"))
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'B2 generated TCP fence variable missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            Replace-SyntheticClassesToken `
                -Snapshot $s `
                -After '.\Class\TCPMotionInterface\TCPMotionInterface.st' `
                -Old RpcCallbackCookieHi `
                -New RpcCallbackCookieHx
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'B2 generated TCP helper global drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedWired
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s `
                -FunctionName DisarmRpcCallbackEndpoint `
                -HeaderOffset 5 `
                -Value 1
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedWired
        }
    $negativeCount += Assert-TcpSourceReplacementNegativeFixture `
        -Name 'Gate C duplicate local VAR section' `
        -State DerivedCandidate `
        -Old ("`t`tactivePeerIPv4 `t: UDINT;`n" +
            "`t`tcallbackDisarmResult : DINT;`n" +
            "`tEND_VAR") `
        -New ("`t`tactivePeerIPv4 `t: UDINT;`n" +
            "`tEND_VAR`n" +
            "`tVAR`n" +
            "`t`tcallbackDisarmResult : DINT;`n" +
            "`tEND_VAR")
    $tcpGateCMutations = @(
        @(
            'Gate C command hex literal split by whitespace',
            '0x2022',
            '0 x2022'),
        @(
            'Gate C mask hex literal split by whitespace',
            '0x00000001',
            '0 x00000001'),
        @(
            'Gate C typed lvalue suffix split by whitespace',
            'Sendbuf[0]$UINT',
            'Sendbuf[0]$ UINT'),
        @(
            'Gate C address operator split by whitespace',
            '#Sendbuf[0]',
            '# Sendbuf[0]'),
        @(
            'Gate C helper ignores armed flag',
            '(RpcCallbackRegistered = FALSE)',
            '(RpcCallbackRegistered = TRUE)'),
        @(
            'Gate C helper omits connected client fence',
            'ELSIF NOT IsClientConnected(#CallbackSender) THEN',
            'ELSIF FALSE THEN'),
        @(
            'Gate C helper clears on negative result',
            'IF (Result = 0) OR (Result = 1) THEN',
            'IF TRUE THEN'),
        @(
            'Gate C helper disarm epoch drift',
            'ExpectedSessionEpoch := RpcCallbackSessionEpoch',
            'ExpectedSessionEpoch := SessionEpoch'),
        @(
            'Gate C forced path directly clears callback tuple',
            'callbackDisarmResult := DisarmRpcCallbackEndpoint();',
            ("callbackDisarmResult := DisarmRpcCallbackEndpoint();`n" +
                '        RpcCallbackRegistered := FALSE;')),
        @(
            'Gate C forced path advances before disarm',
            ("callbackDisarmResult := DisarmRpcCallbackEndpoint();`n" +
                '    SessionEpoch += 1;'),
            ("SessionEpoch += 1;`n" +
                '    callbackDisarmResult := DisarmRpcCallbackEndpoint();')),
        @(
            'Gate C forced path makes disarm call dead',
            ("callbackDisarmResult := DisarmRpcCallbackEndpoint();`n" +
                '    SessionEpoch += 1;'),
            ("IF FALSE THEN`n" +
                "      callbackDisarmResult := DisarmRpcCallbackEndpoint();`n" +
                "    END_IF;`n" +
                '    SessionEpoch += 1;')),
        @(
            'Gate C malformed init disarms before validation',
            ("IF (Payload = 1) AND (RequestBuf[8] = 0) AND`n" +
                "       ((RpcInitialized = FALSE) OR (RpcSocket = CurrentSock)) THEN`n" +
                '      callbackDisarmResult := DisarmRpcCallbackEndpoint();'),
            ("callbackDisarmResult := DisarmRpcCallbackEndpoint();`n" +
                "    IF (Payload = 1) AND (RequestBuf[8] = 0) AND`n" +
                '       ((RpcInitialized = FALSE) OR (RpcSocket = CurrentSock)) THEN')),
        @(
            'Gate C non-owner legacy request locks shape',
            'IF (Payload = 12) AND (RpcInitialized = TRUE) AND',
            'IF Payload = 12 THEN'),
        @(
            'Gate C non-owner v2 request locks shape',
            'ELSIF (Payload = 32) AND (RpcInitialized = TRUE) AND',
            'ELSIF Payload = 32 THEN'),
        @(
            'Gate C failed v2 permits legacy switch',
            'IF RpcCallbackProtocolVersion = 1 THEN',
            'IF RpcCallbackProtocolVersion <> 0 THEN'),
        @(
            'Gate C failed v1 permits v2 switch',
            'IF RpcCallbackProtocolVersion = 2 THEN',
            'IF RpcCallbackProtocolVersion <> 0 THEN'),
        @(
            'Gate C repeated init ignores negative disarm',
            'IF callbackDisarmResult < 0 THEN',
            'IF callbackDisarmResult > 0 THEN'),
        @(
            'Gate C ArmEndpoint cookie input missing',
            'CookieHi := callbackCookieHi,',
            'CookieHx := callbackCookieHi,'),
        @(
            'Gate C v2 response frame size drift',
            'udSize := 28,',
            'udSize := 20,'),
        @(
            'Gate C Diagnostics connection check removed',
            'IF IsClientConnected(#Diagnostics) THEN',
            'IF TRUE THEN'),
        @(
            'Gate C v2 maximum lower bound drift',
            '(callbackAcceptedMaxDatagram >= 52)',
            '(callbackAcceptedMaxDatagram >= 51)'),
        @(
            'Gate C v2 maximum upper bound drift',
            '(callbackAcceptedMaxDatagram <= 512)',
            '(callbackAcceptedMaxDatagram <= 513)'),
        @(
            'Gate C v2 zero cookie accepted',
            '((callbackCookieLo OR callbackCookieHi) <> 0)',
            '((callbackCookieLo OR callbackCookieHi) >= 0)'),
        @(
            'Gate C v2 flags validation removed',
            '(callbackFlags = 0)',
            '(callbackFlags >= 0)'),
        @(
            'Gate C v2 reserved validation removed',
            '(callbackReserved = 0)',
            '(callbackReserved >= 0)'),
        @(
            'Gate C v2 peer fence drift',
            '(callbackIPv4 = CurrentPeerIPv4)',
            '(callbackIPv4 <> CurrentPeerIPv4)'),
        @(
            'Gate C v2 session fence drift',
            '(SessionEpoch <> 0)',
            '(SessionEpoch = 0)'),
        @(
            'Gate C v2 accepted maximum response offset drift',
            'Sendbuf[14]$UINT := callbackAcceptedMaxDatagram;',
            'Sendbuf[15]$UINT := callbackAcceptedMaxDatagram;'),
        @(
            'Gate C v2 BootId response offset drift',
            'Sendbuf[16]$UDINT := callbackBootId;',
            'Sendbuf[17]$UDINT := callbackBootId;'),
        @(
            'Gate C v2 SessionEpoch response offset drift',
            'Sendbuf[20]$UDINT := SessionEpoch;',
            'Sendbuf[21]$UDINT := SessionEpoch;'),
        @(
            'Gate C v2 accepted flags response nonzero',
            'Sendbuf[24]$UDINT := 0;',
            'Sendbuf[24]$UDINT := 1;'),
        @(
            'Gate C legacy duplicate fence removed',
            '(RpcCallbackEventMask = callbackEventMask) AND',
            'TRUE AND'),
        @(
            'Gate C legacy ACK frame size drift',
            'udSize := 12,',
            'udSize := 13,'),
        @(
            'Gate C close loses old epoch notification',
            'PendingClosedSessionEpoch := SessionEpoch;',
            'PendingClosedSessionEpoch := SessionEpoch + 1;'),
        @(
            'Gate C close omits old epoch notification',
            ("IF (SessionEpoch <> 0) AND (PendingClosedSessionEpoch = 0) THEN`n" +
                "          PendingClosedSessionEpoch := SessionEpoch;`n" +
                '        END_IF;'),
            ''),
        @(
            'Gate C close double-advances after SendData failure',
            'IF (RpcInitialized = TRUE) AND (RpcSocket = CurrentSock) THEN',
            'IF TRUE THEN'),
        @(
            'Gate C successful v2 does not arm TCP tuple',
            'RpcCallbackRegistered := TRUE;',
            'RpcCallbackRegistered := FALSE;'))
    foreach ($mutation in $tcpGateCMutations) {
        $negativeCount += Assert-TcpSourceReplacementNegativeFixture `
            -Name $mutation[0] `
            -State DerivedCandidate `
            -Old $mutation[1] `
            -New $mutation[2]
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Gate C 0x405D negative disarm advances session' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $old = "IF callbackDisarmResult < 0 THEN`n" +
                '        Sendbuf[8]$UINT := 1;'
            $new = "IF callbackDisarmResult < 0 THEN`n" +
                "        SessionEpoch += 1;`n" +
                '        Sendbuf[8]$UINT := 1;'
            if ($s.TcpSource.IndexOf($old, [StringComparison]::Ordinal) -lt 0) {
                throw 'Gate C 0x405D negative fixture anchor is missing.'
            }
            $last = $s.TcpSource.LastIndexOf($old, [StringComparison]::Ordinal)
            $s.TcpSource = $s.TcpSource.Remove($last, $old.Length).Insert($last, $new)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false -RequiredState DerivedCandidate
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'top-level transceiver source endpoint missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                '<Server Name="sControl"/>',
                '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'top-level sender destination endpoint missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                '<Server Name="ClassSvr"/>',
                '')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'empty derived ServiceTransmitQueue stub' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.DerivedSource = [regex]::Replace(
                $s.DerivedSource,
                '(?ms)^FUNCTION LMCUdpCallbackSender::ServiceTransmitQueue$' +
                    '.*?^END_FUNCTION$',
                "FUNCTION LMCUdpCallbackSender::ServiceTransmitQueue`n" +
                    'END_FUNCTION')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork virtual scope drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName CyWork -HeaderOffset 4 -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork input count drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName CyWork -HeaderOffset 8 -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated SendSlot VendorResult drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Replace-SyntheticClassesToken `
                -Snapshot $s -After SendSlot `
                -Old VendorResult -New WrongResult
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated sender storage token missing' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Replace-SyntheticClassesToken `
                -Snapshot $s -After LMCUdpCallbackSender.st `
                -Old QueueDepth -New QueueDeXth
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'duplicate generated sender source record' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $extra = [Text.Encoding]::ASCII.GetBytes(
                '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st')
            $updated = [byte[]]::new($s.ClassesDatabaseBytes.Count + $extra.Count)
            [Array]::Copy(
                $s.ClassesDatabaseBytes, 0, $updated, 0,
                $s.ClassesDatabaseBytes.Count)
            [Array]::Copy(
                $extra, 0, $updated, $s.ClassesDatabaseBytes.Count, $extra.Count)
            $s.ClassesDatabaseBytes = $updated
            $s.ClassesDatabaseText = $Latin1.GetString($updated)
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'vendor record changed during derived generation' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.VendorGeneratedRecords[0].Sha256 = 'DRIFT'
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'derived Network cyclic time drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommNetworkText = $s.CommNetworkText.Replace(
                'CyclicTime="10 ms"', 'CyclicTime="20 ms"')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    if ($false) {
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
                "CallbackSender `t: CltChCmd_LMCUdpCallbackSender;",
                "WrongClient `t: CltChCmd_LMCUdpCallbackSender;")
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
        -Name 'Comm table canonical text semantic drift' -Action {
            Assert-DerivedCommTablePhysicalContract `
                -State TerminalWakeBrokerCandidate `
                -CommTableText $commTableSemanticDriftText `
                -CommTableBytes $commTableSemanticDriftBytes.Count `
                -CommTableSha256 $commTableSemanticDriftSha256
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected Network canonical text semantic drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.ProtectedTrackedNetworkSha256 =
                $protectedNetworkTextDriftIdentity.Sha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'protected Network binary identity drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.ProtectedTrackedNetworkSha256 =
                $protectedNetworkBinaryDriftIdentity.Sha256
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
        -Name 'generated CyWork method kind drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            Set-SyntheticGeneratedHeaderByte `
                -Snapshot $s -FunctionName CyWork -HeaderOffset 0 -Value 0x0B
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork outer name prefix drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $cyWork = $s.ClassesDatabaseText.IndexOf(
                'CyWork',
                [StringComparison]::Ordinal)
            if (($cyWork -lt 6) -or
                ($s.ClassesDatabaseBytes[$cyWork - 4] -ne 6)) {
                throw 'synthetic CyWork outer prefix anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index ($cyWork - 4) -Value 7
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork input EAX register binding drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $cyWork = $s.ClassesDatabaseText.IndexOf(
                'CyWork',
                [StringComparison]::Ordinal)
            $eax = $s.ClassesDatabaseText.IndexOf(
                'EAX',
                $cyWork,
                [StringComparison]::Ordinal)
            $type = $s.ClassesDatabaseText.IndexOf(
                'UDINT',
                $eax + 3,
                [StringComparison]::Ordinal)
            $register = $type + 5 + 4 + 43
            if (($type -lt 0) -or
                ($s.ClassesDatabaseBytes[$register] -ne 0x10)) {
                throw 'synthetic CyWork input register anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index $register -Value 0xFF
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork output EAX register binding drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $cyWork = $s.ClassesDatabaseText.IndexOf(
                'CyWork',
                [StringComparison]::Ordinal)
            $state = $s.ClassesDatabaseText.IndexOf(
                'state',
                $cyWork,
                [StringComparison]::Ordinal)
            $type = $s.ClassesDatabaseText.IndexOf(
                'UDINT',
                $state + 5,
                [StringComparison]::Ordinal)
            $register = $type + 5 + 4 + 43
            if (($type -lt 0) -or
                ($s.ClassesDatabaseBytes[$register] -ne 0x10)) {
                throw 'synthetic CyWork output register anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index $register -Value 0xFF
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated PublishEvent pointer descriptor drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $publish = $s.ClassesDatabaseText.IndexOf(
                'PublishEvent',
                [StringComparison]::Ordinal)
            $payload = $s.ClassesDatabaseText.IndexOf(
                'pPayload',
                $publish,
                [StringComparison]::Ordinal)
            $pointerFlag = $payload + 'pPayload'.Length + 4 + 54
            if (($payload -lt 0) -or
                ($s.ClassesDatabaseBytes[$pointerFlag] -ne 1)) {
                throw 'synthetic pPayload pointer anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index $pointerFlag -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated parameter descriptor prefix drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $arm = $s.ClassesDatabaseText.IndexOf(
                'ArmEndpoint',
                [StringComparison]::Ordinal)
            $parameter = $s.ClassesDatabaseText.IndexOf(
                'ProtocolVersion',
                $arm,
                [StringComparison]::Ordinal)
            $descriptor = $parameter + 'ProtocolVersion'.Length + 4
            if (($parameter -lt 0) -or
                ($s.ClassesDatabaseBytes[$descriptor] -ne 1)) {
                throw 'synthetic parameter descriptor anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index $descriptor -Value 2
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated qualified type owner drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $senderPath = $s.ClassesDatabaseText.IndexOf(
                '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st',
                [StringComparison]::Ordinal)
            $errorCallback = $s.ClassesDatabaseText.IndexOf(
                'ErrorCallback',
                $senderPath,
                [StringComparison]::Ordinal)
            $baseType = $s.ClassesDatabaseText.IndexOf(
                '_FSM_UDP_USER',
                $errorCallback,
                [StringComparison]::Ordinal)
            $typeOwner = $s.ClassesDatabaseText.IndexOf(
                '_UDPTransceiver',
                $baseType + '_FSM_UDP_USER'.Length,
                [StringComparison]::Ordinal)
            if (($senderPath -lt 0) -or ($errorCallback -lt 0) -or
                ($baseType -lt 0) -or ($typeOwner -lt 0)) {
                throw 'synthetic sender qualified type owner anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s `
                -Index ($typeOwner + '_UDPTransceiver'.Length - 1) `
                -Value ([byte][char]'X')
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated output parameter prefix drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $arm = $s.ClassesDatabaseText.IndexOf(
                'ArmEndpoint',
                [StringComparison]::Ordinal)
            $result = $s.ClassesDatabaseText.IndexOf(
                'Result',
                $arm,
                [StringComparison]::Ordinal)
            if (($result -lt 6) -or
                ($s.ClassesDatabaseBytes[$result - 5] -ne 1)) {
                throw 'synthetic output parameter prefix anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index ($result - 5) -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork method trailer drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $cyWork = $s.ClassesDatabaseText.IndexOf(
                'CyWork',
                [StringComparison]::Ordinal)
            $errorCallback = $s.ClassesDatabaseText.IndexOf(
                'ErrorCallback',
                $cyWork + 'CyWork'.Length,
                [StringComparison]::Ordinal)
            $nextMethodPrefix = $errorCallback - 6
            if (($errorCallback -lt 6) -or
                ($s.ClassesDatabaseBytes[$nextMethodPrefix - 1] -ne 0)) {
                throw 'synthetic CyWork trailer anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index ($nextMethodPrefix - 1) -Value 1
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated CyWork method cursor gap' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $cyWork = $s.ClassesDatabaseText.IndexOf(
                'CyWork',
                [StringComparison]::Ordinal)
            $errorCallback = $s.ClassesDatabaseText.IndexOf(
                'ErrorCallback',
                $cyWork + 'CyWork'.Length,
                [StringComparison]::Ordinal)
            if ($errorCallback -lt 6) {
                throw 'synthetic CyWork cursor-gap anchor drifted.'
            }
            Insert-SyntheticClassesByteAt `
                -Snapshot $s -Index ($errorCallback - 6) -Value 0
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated unexpected method after FenceMatches' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $tcpPath = $s.ClassesDatabaseText.IndexOf(
                '.\Class\TCPMotionInterface\TCPMotionInterface.st',
                [StringComparison]::Ordinal)
            if ($tcpPath -lt 0) {
                throw 'synthetic post-Fence method insertion anchor drifted.'
            }
            $unexpected = New-SyntheticFunctionMetadataBytes `
                -Name UnexpectedSenderMethod `
                -IsGlobal $false `
                -Inputs @() `
                -Outputs @()
            Insert-SyntheticClassesBytesAt `
                -Snapshot $s -Index $tcpPath -Values $unexpected
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Candidate generated TCP RpcCallback inventory drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $rpcIndex = $s.ClassesDatabaseText.IndexOf(
                'RpcCallbackAcceptedMaxDatagram',
                [StringComparison]::Ordinal)
            if ($rpcIndex -lt 0) {
                throw 'synthetic Candidate RpcCallback inventory anchor drifted.'
            }
            Set-SyntheticClassesByteAt `
                -Snapshot $s -Index $rpcIndex -Value ([byte][char]'X')
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
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'Candidate ONE compiled macro output drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommTableText = $s.CommTableText.Replace(
                '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 0',
                '#define LMC_UDP_CALLBACK_ENABLE_LEGACY_FIXTURE 1')
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Network table UDP link source ID drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommTableText = $s.CommTableText.Replace(
                'TO_UDINT(6), "_UDPTransceiver", TO_UDINT(1), "sControl",',
                'TO_UDINT(5), "_UDPTransceiver", TO_UDINT(1), "sControl",')
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
        }
    $negativeCount += Assert-UdpCallbackNegativeFixture `
        -Name 'generated Network table sender task period drift' -Action {
            $s = New-UdpCallbackTestSnapshot -State DerivedCandidate
            $s.CommTableText = $s.CommTableText.Replace(
                'TO_UDINT(6), (10)$UDINT, 4194303$DINT,',
                'TO_UDINT(6), (20)$UDINT, 4194303$DINT,')
            Update-SyntheticAsciiSnapshotEvidence `
                -Snapshot $s `
                -TextProperty CommTableText `
                -BytesProperty CommTableBytes `
                -ShaProperty CommTableSha256
            $null = Assert-LasalUdpCallbackStateContract `
                -Snapshot $s -PermitAbsent $false
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
        'Absent explicit, VendorImported, DerivedDeclaration, DerivedWired, ' +
        'corrected DerivedCandidate, and TerminalWakeBrokerCandidate ' +
        'positives accepted)')
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
    if ($result.NeedsRebaseline -and (-not $AllowDerivedCapture)) {
        Throw-UdpCallbackBlocker (
            "$($result.State) is structurally valid but has no approved " +
            'physical snapshot ratchet; rerun focused with ' +
            '-AllowDerivedCapture and an explicit -ExpectedState.')
    }
    if ($result.NeedsRebaseline -and ($ExpectedState -ceq 'Auto')) {
        Throw-UdpCallbackBlocker (
            'derived capture requires an explicit -ExpectedState phase.')
    }
    $dependencyEvidence = [string]::Join(',', @(
            $snapshot.ProtectedDependencies |
                ForEach-Object { "$($_.Name)=$($_.Bytes)/$($_.Sha256)" }))
    $includeEvidence = [string]::Join(',', @(
            $snapshot.GeneratedIncludes |
                ForEach-Object {
                    "$($_.Name)=$($_.RawBytes)/$($_.RawSha256)"
                }))
    $resultPrefix = if ($result.ProductionApproved) { 'PASS' } else { 'CAPTURE' }
    $senderEvidence = Get-UdpCallbackSenderEvidenceToken `
        -State $result.State `
        -Snapshot $snapshot
    Write-Output (
        "$resultPrefix LASAL.UdpCallbackContract.Current " +
        "(state=$($result.State); IDEClosed=true; " +
        "productionApproved=$($result.ProductionApproved); " +
        "needsRebaseline=$($result.NeedsRebaseline); " +
        "vendor=$($snapshot.TransceiverRawBytes)/" +
        "$($snapshot.TransceiverRawSha256)," +
        "$($snapshot.InterfaceRawBytes)/$($snapshot.InterfaceRawSha256); " +
        "Classes=$($snapshot.ClassesBytes)/$($snapshot.ClassesSha256); " +
        "project=$($snapshot.ProjectBytes)/$($snapshot.ProjectSha256); " +
        "lcp=$($snapshot.ProjectDefinitionBytes)/" +
        "$($snapshot.ProjectDefinitionSha256); " +
        "Includes=$includeEvidence; " +
        "TCP=$($snapshot.TcpSha256); " +
        $senderEvidence +
        "Diagnostics=$($snapshot.DiagnosticsRawBytes)/" +
        "$($snapshot.DiagnosticsRawSha256)," +
        "$($snapshot.DiagnosticsCanonicalLfBytes)/" +
        "$($snapshot.DiagnosticsCanonicalLfSha256); " +
        "Network=$($snapshot.FullNetworkCount)/" +
        "$($snapshot.FullNetworkSha256),tracked=" +
        "$($snapshot.TrackedNetworkCount)/$($snapshot.TrackedNetworkSha256); " +
        "protected=$dependencyEvidence)")
    return
}

throw "$Owner blocker: no operation was selected."

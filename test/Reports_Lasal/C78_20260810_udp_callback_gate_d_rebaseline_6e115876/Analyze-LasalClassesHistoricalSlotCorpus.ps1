[CmdletBinding(DefaultParameterSetName = 'Analyze')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Analyze')]
    [switch]$AnalyzePinnedCorpus,
    [Parameter(ParameterSetName = 'Analyze')]
    [string]$RepositoryRoot,
    [Parameter(ParameterSetName = 'Analyze')]
    [string]$OutputPath,
    [Parameter(ParameterSetName = 'Analyze')]
    [switch]$CreateNew,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,
    [Parameter(Mandatory = $true, ParameterSetName = 'Fixture', DontShow = $true)]
    [switch]$EmitJsonSelfTestFixtureBase64,
    [Parameter(ParameterSetName = 'SelfTest', DontShow = $true)]
    [switch]$InternalHostSelfTest
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Owner = 'LASAL.ClassesHistoricalSlotCorpusAnalyzer'
$Schema = 'LasalClassesHistoricalSlotCorpusEvidence/v1'
$CanonicalClassesPath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
$AnchorCommit = '55435791f6e91c9dcb4e06dcd25a11d77b382da7'
$CanonicalTableSha256 =
    'C14E87F49F3D23E57DBED5462A9AC8319089831E9B287A688747559710B5C8F4'
$EvidenceRelativeDirectory =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876'
$ToolRelativePath = $EvidenceRelativeDirectory +
    '/Analyze-LasalClassesHistoricalSlotCorpus.ps1'
$ReportFileName =
    'classes_lcb_historical_slot_corpus_bd9dcb0c_55435791_99014dd9_' +
    '6e115876.analysis.json'
$CheckpointCommit = $AnchorCommit
$CheckpointBlobOid = '7b0faebb1450ff67b7dad44f081ad5c4ac141ee2'
$CheckpointBytes = 8549773L
$CheckpointSha256 =
    '24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861'
$KnownRebuildBlobOid = 'bd47dd96f0df4be54c898e9bc18e70ebfd439e95'
$KnownRebuildBytes = 8549773L
$KnownRebuildSha256 =
    '6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712'
$KnownRebuildCaptureHead = 'f352476fd6f93061b105d2e3663414cc6c24669c'
$ThirdCommit = 'b2019db3af5a9990d2e0fe0afd0f02cbfbfaff53'
$ThirdBlobOid = '726f5ed4498592dba13e358c0d7320d2e5d02a1a'
$ThirdBytes = 8549773L
$ThirdSha256 =
    '99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD'
$KnownPatchPath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.binary.patch'
$KnownManifestPath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.manifest.json'
$KnownOraclePath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json'
$ThirdSnapshotPath = $EvidenceRelativeDirectory +
    '/candidate_finalization_gate_d_rebaseline_6e115876/' +
    'Classes.post-rebuild.snapshot.lcb'
$Utf8Strict = New-Object Text.UTF8Encoding($false, $true)
$Utf8NoBom = New-Object Text.UTF8Encoding($false)
$Latin1 = [Text.Encoding]::GetEncoding(28591)
$MarkerBytes = [byte[]](
    0x8F, 0x68, 0x1A, 0x16, 0x6D, 0xB0,
    0x6E, 0x37, 0x85, 0xCA, 0x73, 0x41)
$MarkerHex = '8F681A166DB06E3785CA7341'

$HistoryTable = @(
    @('bd9dcb0c10caa6c53f4d9a2b4f3897584164bc02','38cf50fba87d3f5e37d1d1dd3bb2ec20cfc7b932',7806258L,'44EF49D510001192A6315C4EEE6D88D3E2B79D386FAA445EF3F397FCD113498B'),
    @('6109c9b53edad70010defb4556862f52319c9b28','09d8a134d1dc1505bdf7b0147fd2653c2f440afd',7836027L,'2E5325DC23D3150C8B93015A3AFADDC67B5398008A278B45C346B3595422A341'),
    @('5dfddc35b57f03b2cf9dbc24287a4c0de7658e53','378d1e8b7aef27e713fca678dfa008d0b99eb75f',7921675L,'8B613E72A634205B18AD8FB9E436EB6AD0CB9174BB0F722C824354AE6D88F87E'),
    @('3ccfb92af84319aed260fe63e910810b43f24d75','e4604dd0e7ff30c324ed91a7a08fedbcb2afbecb',7928502L,'0504D84D584C91DF7C06E057D16523C455358190A5A9E4E77B1C0ED409403F79'),
    @('f56e2693d62886f649585f86bb9fa03dcb2541f9','4e9cd00320bb07a58e2d062a5dc0f6a1eafc1d80',7975666L,'B21D1FE02BE13429F5D7176C70B38AE6D40164B628D1C546DFC8B970F8EDB122'),
    @('c03a43bb3cad409a19e94aeada13bb0f0b60c4fc','1fdb93f43476cb65ef37917ca95b0d418b8ef1ae',7994920L,'FB5597F75B1DFD94F0044FF47E62CE53D077D0A7894B76422C6DF5EA0DFCE1EE'),
    @('975cab0c96d28afc78b9491168103cfb826d7021','02fa478418267411f77019297310c34a728dd728',8023945L,'98C58B66EC78AA9F53EE7FAD9102E66488C4701E045C5C6E69AE73A084E891C5'),
    @('8c4f7fd175c04ac8386447c2a1fc9d652afac860','df20a347f8737ec2a0b50ad261c6ce3512094d3a',8025483L,'A29A2F9B06210A69AD29521E2098B803ECA62CAE2CBB7A132422312AC8F4C3FD'),
    @('8063404163e6815a23f63159768d23965404a397','b60a2efc58bbf437213cebefc98a8e95f85f37e8',8051581L,'BDF737F2EC4A4EAC97A1F395EBD5041CB49ADFAA768E803BDE5F4AF931AE2D41'),
    @('837758f2a66c2ba81957f8da7f33531fd9fa0f69','0e587b173dec6b5f40c933542595012aff74fc65',8015178L,'B97DAD457BA086E90F34C1A0DFA6E19013B03CC83879676F25BF9D84DF60E71C'),
    @('ba39d41f7b8e1a12fcdb3f4553c4bb2a0f00a6db','e1f60f1d99aa0ef9266966d85e95bf3a41977ccc',8153475L,'B409DBF40DDCDB150FE554B0784AB885258BD2DB7BF79ECD09CDC7A4F24C8F31'),
    @('3a74642223a6715f8f8abf9ab02d9ef6e96b8e21','dc01b92e45653e328714d5f4f6f154b1656eabb4',8419903L,'F31AF6663C305EAE7FCBD42E0A87A54347B34D592DF91725257CDBDF0655279C'),
    @('b9c0c77e800ab302feb10b4872ae5acc2e2cd310','65777b172beaa5fb110d63dc9f21ad78d8789dfd',8430171L,'3B5D814F566F20D49D8033CC6E6F735A1503D91B7A3D5F87D3E6339FECC3421B'),
    @('12df8d90fdc0f26373705037c652f0dd786f4d5c','891791285b83ea377c3ab32faef03fb26713e5ab',8433167L,'6B90C4DB117AB5C2B01BF773BA5A19DA845F3533ABBF1273CC4A25E4B8710E22'),
    @('3e78529aea2a35fcb50cb5a54d4a8fb253786c6e','c640285264ad161d5bb4045fba83ceede1507014',8434505L,'CA5CE9AB4B6AFB498D55CF6E5D3460A2C35D54FF8E4FE9C9D3B59636C3603F78'),
    @('275ab134ae62b37429dfe4ea3fea867bec5e5614','c3520b3de9feec5177c153f1b85436e85cd1e092',8512773L,'0CB9A3D3A4E8EB27E9A5BEB44E91D46BAEE23A051736AA83622D790249C61DC6'),
    @('6a36d5eefa6c724c94bde587305e5ed68901bda6','b26fae6a3702997b92045596a3b709c5a15fb7e3',8539670L,'52381099FF4A4AA21B563248D00E145756A4960B790FA1BB1EFDE777045159EA'),
    @('2554d3b3b89a40626c01a1838e29a8cbf7cb0274','c3520b3de9feec5177c153f1b85436e85cd1e092',8512773L,'0CB9A3D3A4E8EB27E9A5BEB44E91D46BAEE23A051736AA83622D790249C61DC6'),
    @('95c76fead591117e72c247f47650966dabbbea28','b26fae6a3702997b92045596a3b709c5a15fb7e3',8539670L,'52381099FF4A4AA21B563248D00E145756A4960B790FA1BB1EFDE777045159EA'),
    @('cc833112cbb13ad00fc55595977fd7cd6fff0576','7b3ad8e0ef3330c8f7d13945622f690b5cbc5abe',8541810L,'068D8B2237DCC296B61D0ECD8FAC7BDD0D72B693E7E6CAFA63E3AC6AC3D220DE'),
    @('70c08ea101a5bf74385ca8b65debe45ca0692e7e','a8a9f0c99897fc6275516fa2110b211ae7501c6a',8546276L,'B711D1A0566ECE23798960952C2B3316DDFB5F854EDED47EBEB6C1DBA629DE7A'),
    @('55435791f6e91c9dcb4e06dcd25a11d77b382da7','7b0faebb1450ff67b7dad44f081ad5c4ac141ee2',8549773L,'24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861')
)

$PinnedAugmentedInputs = @(
    [ordered]@{
        role = 'known6eBinaryPatch'; relativePath = $KnownPatchPath
        commit = '703844576c658460a018373894db85e43cda3096'
        blobOid = 'fc36eb76c3293e04a7aa0acf4674d408865ffa70'
        rawBytes = 2553L
        sha256 = 'AF9A4D32B6F568036E4200BD3F47C9CD63ABB4027D37A1F60BEDB7287731A160'
        format = 'git-binary-patch'
    },
    [ordered]@{
        role = 'known6ePatchManifest'; relativePath = $KnownManifestPath
        commit = '703844576c658460a018373894db85e43cda3096'
        blobOid = 'e181b57a15bd10465ba6de100aa239d4dfe8709b'
        rawBytes = 2427L
        sha256 = 'B919A2EC25ABE99C7C8D5D37E19F0EDDB3D7998C1DF7C1F7C74FB3B9B5D8956C'
        format = 'strict-utf8-json'
    },
    [ordered]@{
        role = 'known6eComparisonOracle'; relativePath = $KnownOraclePath
        commit = '2e8ca8a84a141390424ce859ac8c315a90ec3430'
        blobOid = '2a73c039391a487082bc0958233ef1930a298f91'
        rawBytes = 51102L
        sha256 = '9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639'
        format = 'comparator-canonical-json'
    },
    [ordered]@{
        role = 'third990BundleSnapshot'; relativePath = $ThirdSnapshotPath
        commit = $ThirdCommit; blobOid = $ThirdBlobOid
        rawBytes = $ThirdBytes; sha256 = $ThirdSha256
        format = 'sigmatek-lasal-classes'
    }
)

$PriorDiagnosticPins = @(
    [ordered]@{
        role = 'priorPinnedTriadAnalyzer'
        relativePath = $EvidenceRelativeDirectory +
            '/Compare-LasalClassesVolatilityTriad.ps1'
        commit = '998e7132c0892788db79a0868c5b129fb20edd96'
        blobOid = 'a7dd4dba67e30c4adc80549a1d9b6a4d1acb6bce'
        rawBytes = 139073L
        sha256 = 'E3E2C586C62379339EECFD8038189D9959C655CD206A4E894B846A2D79783663'
        format = 'powershell-source'
    },
    [ordered]@{
        role = 'priorPinnedTriadEvidence'
        relativePath = $EvidenceRelativeDirectory +
            '/classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_' +
            '99014dd9.volatility.json'
        commit = 'e7c812ad7cfc6ef2162ed1197dc615e2aebe45db'
        blobOid = '3c4411e26493043b80828a5355bdc8b621457e09'
        rawBytes = 29412L
        sha256 = '09C76BB3BC313642C3012A915C14C022EDF75965A8A431B87F26B463005489DC'
        format = 'canonical-json-evidence'
    }
)

$FrozenOpaqueVendorOwners = @(
    '_LMCABSEncoder', '_LMCAxis', '_LMCAxisBase', '_LMCAxisRef',
    '_LMCAxisVis', '_LMCAxisVisInt', '_LMCAxisVisLogHandle',
    '_LMCAxisVisLogViewer', '_LMCAxisVisPara', '_LMCAxisVOVMonitoring',
    '_LMCBaseCoord', '_LMCBeltAxis', '_LMCCalcModelBase',
    '_LMCCalcModelController', '_LMCMath_SO3', '_LMCMathFunctions',
    '_LMCProfile', '_LMCProfileBase', '_LMCProfileBuffer', '_LMCProfileLog',
    '_LMCProfileVis', '_LMCProfileVisAxis', '_LMCProfileVisInt',
    '_LMCProfileVisMovePara', '_LMCPublisher', '_LMCRefBase',
    '_LMCRobotBase', '_LMCRobotLog', '_LMCSafety', '_LMCSplineBuffer',
    '_LMCTableBuffer', '_LMCTool', 'Controller', 'MoveSplineTable',
    'PosController', 'SigCLib')

function Throw-CorpusBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw "BLOCKED: $Message"
}

function Assert-PowerShell7Production {
    if (($PSVersionTable.PSEdition -cne 'Core') -or
        ($PSVersionTable.PSVersion.Major -lt 7)) {
        Throw-CorpusBlocker (
            'production analysis requires PowerShell 7 before evidence or ' +
            'output is read; PS5 remains a canonical/self-test host only.')
    }
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $algorithm.ComputeHash($Bytes))).Replace('-', '')
    }
    finally { $algorithm.Dispose() }
}

function Get-GitBlobOid {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    [byte[]]$header = [Text.Encoding]::ASCII.GetBytes(
        'blob ' + $Bytes.LongLength + [char]0)
    $algorithm = [Security.Cryptography.SHA1]::Create()
    try {
        [void]$algorithm.TransformBlock(
            $header, 0, $header.Length, $null, 0)
        [void]$algorithm.TransformFinalBlock($Bytes, 0, $Bytes.Length)
        return ([BitConverter]::ToString(
                $algorithm.Hash)).Replace('-', '').ToLowerInvariant()
    }
    finally { $algorithm.Dispose() }
}

function Test-ByteSequencesExact {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][byte[]]$Right
    )
    if ($Left.LongLength -ne $Right.LongLength) { return $false }
    return [Linq.Enumerable]::SequenceEqual([byte[]]$Left, [byte[]]$Right)
}

function Copy-Bytes {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $copy = New-Object byte[] $Bytes.Length
    [Array]::Copy($Bytes, 0, $copy, 0, $Bytes.Length)
    return ,$copy
}

function Get-HexRange {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$Length
    )
    if (($Start -lt 0) -or ($Length -lt 0) -or
        (($Start + $Length) -gt $Bytes.Length)) {
        Throw-CorpusBlocker 'hex range is outside the artifact.'
    }
    if ($Length -eq 0) { return '' }
    return ([BitConverter]::ToString(
            $Bytes, $Start, $Length)).Replace('-', '')
}

function Get-GitExecutable {
    $command = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $command) { Throw-CorpusBlocker 'git is not available.' }
    return $command.Source
}

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Operation
    )
    foreach ($argument in $Arguments) {
        if ([string]::IsNullOrWhiteSpace($argument) -or
            ($argument.IndexOfAny(@([char]' ', [char]9, [char]'"')) -ge 0)) {
            Throw-CorpusBlocker (
                "$Operation contains an unsupported native argument.")
        }
    }
    $startInfo = New-Object Diagnostics.ProcessStartInfo
    $startInfo.FileName = Get-GitExecutable
    $startInfo.WorkingDirectory = $Root
    $startInfo.Arguments = [string]::Join(' ', $Arguments)
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            Throw-CorpusBlocker "$Operation could not start git."
        }
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-CorpusBlocker (
                "$Operation failed with git exit $($process.ExitCode): " +
                $stderr.Trim())
        }
        return $stdout.Trim()
    }
    finally { $process.Dispose() }
}

function Invoke-GitExitCode {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Operation
    )
    foreach ($argument in $Arguments) {
        if ([string]::IsNullOrWhiteSpace($argument) -or
            ($argument.IndexOfAny(@([char]' ', [char]9, [char]'"')) -ge 0)) {
            Throw-CorpusBlocker (
                "$Operation contains an unsupported native argument.")
        }
    }
    $startInfo = New-Object Diagnostics.ProcessStartInfo
    $startInfo.FileName = Get-GitExecutable
    $startInfo.WorkingDirectory = $Root
    $startInfo.Arguments = [string]::Join(' ', $Arguments)
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            Throw-CorpusBlocker "$Operation could not start git."
        }
        [void]$process.StandardOutput.ReadToEnd()
        [void]$process.StandardError.ReadToEnd()
        $process.WaitForExit()
        return [int]$process.ExitCode
    }
    finally { $process.Dispose() }
}

function Read-GitBlobBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$BlobOid,
        [Parameter(Mandatory = $true)][string]$BlobOwner
    )
    if ($BlobOid -cnotmatch '^[0-9a-f]{40}$') {
        Throw-CorpusBlocker "$BlobOwner blob OID is invalid."
    }
    $startInfo = New-Object Diagnostics.ProcessStartInfo
    $startInfo.FileName = Get-GitExecutable
    $startInfo.WorkingDirectory = $Root
    $startInfo.Arguments = "cat-file blob $BlobOid"
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $startInfo
    $memory = New-Object IO.MemoryStream
    try {
        if (-not $process.Start()) {
            Throw-CorpusBlocker "$BlobOwner Git blob read did not start."
        }
        $process.StandardOutput.BaseStream.CopyTo($memory)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-CorpusBlocker (
                "$BlobOwner Git blob read failed with exit " +
                "$($process.ExitCode): $($stderr.Trim())")
        }
        return ,$memory.ToArray()
    }
    finally {
        $memory.Dispose()
        $process.Dispose()
    }
}

function Resolve-RepositoryContext {
    param([AllowEmptyString()][string]$RequestedRoot)
    $scriptBoundRoot = [IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..\..')).TrimEnd('\')
    if (-not (Test-Path -LiteralPath (Join-Path $scriptBoundRoot '.git'))) {
        Throw-CorpusBlocker 'script-bound repository root has no .git entry.'
    }
    $root = $scriptBoundRoot
    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        if (-not (Test-Path -LiteralPath $RequestedRoot -PathType Container)) {
            Throw-CorpusBlocker 'RepositoryRoot is not a directory.'
        }
        $requested = (Resolve-Path -LiteralPath $RequestedRoot).Path.TrimEnd('\')
        if (-not [string]::Equals(
                $requested, $scriptBoundRoot,
                [StringComparison]::OrdinalIgnoreCase)) {
            Throw-CorpusBlocker (
                'RepositoryRoot differs from the script-bound root.')
        }
        $root = $requested
    }
    $gitRoot = Invoke-GitText -Root $root `
        -Arguments @('rev-parse', '--show-toplevel') `
        -Operation 'Git root resolution'
    $resolvedGitRoot = (Resolve-Path -LiteralPath $gitRoot).Path.TrimEnd('\')
    if (-not [string]::Equals(
            $resolvedGitRoot, $root,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-CorpusBlocker (
            'script-bound root is not the exact Git worktree root.')
    }
    return $root
}

function ConvertTo-DeterministicJson {
    param([Parameter(Mandatory = $true)]$Value)
    $json = ($Value | ConvertTo-Json -Depth 40 -Compress)
    return [regex]::Replace(
        $json,
        "[^\x00-\x7F]|[&'<>]",
        [Text.RegularExpressions.MatchEvaluator]{
            param($match)
            $code = [int][char]$match.Value[0]
            if ($code -in @(0x26, 0x27, 0x3C, 0x3E)) {
                return ('\u{0:x4}' -f $code)
            }
            return ('\u{0:X4}' -f $code)
        })
}

function Get-DeterministicJsonBytes {
    param([Parameter(Mandatory = $true)][string]$Json)
    if ([regex]::IsMatch($Json, '[^\x00-\x7F]')) {
        Throw-CorpusBlocker 'deterministic JSON is not 7-bit ASCII.'
    }
    return ,$Utf8NoBom.GetBytes($Json + "`n")
}

function ConvertFrom-StrictJsonBytes {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$JsonOwner,
        [switch]$RequireComparatorCanonical
    )
    try {
        $text = $Utf8Strict.GetString($Bytes)
        $value = $text | ConvertFrom-Json
    }
    catch {
        Throw-CorpusBlocker "$JsonOwner is not strict UTF-8 JSON."
    }
    if ($RequireComparatorCanonical) {
        $canonical = ConvertTo-DeterministicJson -Value $value
        [byte[]]$canonicalBytes = Get-DeterministicJsonBytes -Json $canonical
        if (-not (Test-ByteSequencesExact -Left $Bytes -Right $canonicalBytes)) {
            Throw-CorpusBlocker "$JsonOwner is not comparator-canonical JSON."
        }
    }
    return $value
}

function Write-JsonStdout {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $stdout = [Console]::OpenStandardOutput()
    $stdout.Write($Bytes, 0, $Bytes.Length)
    $stdout.Flush()
}

function Test-PathInsideRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root
    )
    $fullPath = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    $fullRoot = [IO.Path]::GetFullPath($Root).TrimEnd('\')
    return [string]::Equals(
            $fullPath, $fullRoot, [StringComparison]::OrdinalIgnoreCase) -or
        $fullPath.StartsWith(
            $fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)
}

function Assert-NoReparsePointChain {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\')
    $cursorPath = $Path
    if (-not (Test-Path -LiteralPath $cursorPath)) {
        $cursorPath = [IO.Path]::GetDirectoryName(
            [IO.Path]::GetFullPath($cursorPath))
    }
    if ([string]::IsNullOrWhiteSpace($cursorPath) -or
        (-not (Test-Path -LiteralPath $cursorPath))) {
        Throw-CorpusBlocker "$PathOwner parent does not exist."
    }
    $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $cursorPath -Force)
    while ($null -ne $cursor) {
        if (-not (Test-PathInsideRoot `
                -Path $cursor.FullName -Root $resolvedRoot)) {
            Throw-CorpusBlocker "$PathOwner escapes its allowed root."
        }
        if (($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-CorpusBlocker "$PathOwner uses a reparse-point parent."
        }
        if ([string]::Equals(
                $cursor.FullName.TrimEnd('\'), $resolvedRoot,
                [StringComparison]::OrdinalIgnoreCase)) {
            return
        }
        $cursor = $cursor.Parent
    }
    Throw-CorpusBlocker "$PathOwner did not reach its allowed root."
}

function Resolve-ProducerIdentity {
    param([Parameter(Mandatory = $true)][string]$Root)
    $expectedPath = [IO.Path]::GetFullPath(
        (Join-Path $Root $ToolRelativePath.Replace('/', '\')))
    $actualPath = [IO.Path]::GetFullPath($PSCommandPath)
    if (-not [string]::Equals(
            $actualPath, $expectedPath,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-CorpusBlocker (
            'producer script path differs from the exact tool path.')
    }
    $item = Get-Item -LiteralPath $actualPath -Force -ErrorAction Stop
    if (($item -isnot [IO.FileInfo]) -or
        (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) -or
        ([string]$item.Name -cne [IO.Path]::GetFileName($expectedPath))) {
        Throw-CorpusBlocker 'producer script physical identity differs.'
    }
    Assert-NoReparsePointChain -Path $item.DirectoryName -Root $Root `
        -PathOwner 'producer script'
    $scopedStatus = Invoke-GitText -Root $Root `
        -Arguments @(
            'status', '--porcelain=v1', '--untracked-files=all',
            '--', $ToolRelativePath) `
        -Operation 'producer scoped HEAD-clean check'
    if (-not [string]::IsNullOrEmpty($scopedStatus)) {
        Throw-CorpusBlocker (
            'producer script is not tracked and scoped HEAD-clean.')
    }
    $treeLine = Invoke-GitText -Root $Root `
        -Arguments @('ls-tree', 'HEAD', '--', $ToolRelativePath) `
        -Operation 'producer HEAD tree resolution'
    $treeMatch = [regex]::Match(
        $treeLine, '^100644 blob ([0-9a-f]{40})\t(.+)$')
    if ((-not $treeMatch.Success) -or
        ($treeMatch.Groups[2].Value -cne $ToolRelativePath)) {
        Throw-CorpusBlocker (
            'producer script is not exact mode 100644 at HEAD.')
    }
    $headBlobOid = $treeMatch.Groups[1].Value
    $producerCommit = Invoke-GitText -Root $Root `
        -Arguments @('log', '-1', '--format=%H', '--', $ToolRelativePath) `
        -Operation 'producer stable commit resolution'
    if ($producerCommit -cnotmatch '^[0-9a-f]{40}$') {
        Throw-CorpusBlocker 'producer stable commit identity is invalid.'
    }
    $producerBlobOid = Invoke-GitText -Root $Root `
        -Arguments @(
            'rev-parse', '--verify',
            "$producerCommit`:$ToolRelativePath") `
        -Operation 'producer stable commit blob resolution'
    if ($producerBlobOid -cne $headBlobOid) {
        Throw-CorpusBlocker (
            'producer stable commit blob differs from HEAD path.')
    }
    [byte[]]$headBlobBytes = Read-GitBlobBytes -Root $Root `
        -BlobOid $headBlobOid -BlobOwner 'producer HEAD script'
    [byte[]]$physicalBytes = [IO.File]::ReadAllBytes($actualPath)
    if ((-not (Test-ByteSequencesExact `
                -Left $physicalBytes -Right $headBlobBytes)) -or
        ((Get-GitBlobOid -Bytes $physicalBytes) -cne $headBlobOid)) {
        Throw-CorpusBlocker 'producer physical bytes differ from HEAD blob.'
    }
    return [ordered]@{
        head = $producerCommit
        headRole = 'LAST_COMMIT_CHANGING_EXACT_TOOL_PATH'
        relativePath = $ToolRelativePath
        blobOid = $headBlobOid
        rawBytes = [long]$physicalBytes.LongLength
        sha256 = Get-BytesSha256 -Bytes $physicalBytes
        mode = '100644'
        scopedHeadClean = $true
        physicalSnapshotEqualsHeadBlob = $true
        executingBytesAuthenticated = $false
        producerTrustBoundary = 'NON_ADVERSARIAL_WORKSPACE'
    }
}

function Get-SelfTestProducerIdentity {
    return [ordered]@{
        head = ('0' * 40)
        headRole = 'SELFTEST_FIXTURE_NOT_PRODUCTION'
        relativePath = $ToolRelativePath
        blobOid = ('0' * 40)
        rawBytes = 0L
        sha256 = ('0' * 64)
        mode = 'SELFTEST'
        scopedHeadClean = $false
        physicalSnapshotEqualsHeadBlob = $false
        executingBytesAuthenticated = $false
        producerTrustBoundary = 'SELFTEST_FIXTURE'
    }
}

function Resolve-PinnedInput {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Definition
    )
    $commit = Invoke-GitText -Root $Root `
        -Arguments @(
            'rev-parse', '--verify', "$($Definition.commit)^{commit}") `
        -Operation "$($Definition.role) commit resolution"
    if ($commit -cne [string]$Definition.commit) {
        Throw-CorpusBlocker "$($Definition.role) commit identity differs."
    }
    $treeLine = Invoke-GitText -Root $Root `
        -Arguments @(
            'ls-tree', $commit, '--', [string]$Definition.relativePath) `
        -Operation "$($Definition.role) tree resolution"
    $match = [regex]::Match(
        $treeLine, '^100644 blob ([0-9a-f]{40})\t(.+)$')
    if ((-not $match.Success) -or
        ($match.Groups[1].Value -cne [string]$Definition.blobOid) -or
        ($match.Groups[2].Value -cne [string]$Definition.relativePath)) {
        Throw-CorpusBlocker "$($Definition.role) mode/path/blob identity differs."
    }
    [byte[]]$bytes = Read-GitBlobBytes -Root $Root `
        -BlobOid $match.Groups[1].Value `
        -BlobOwner ([string]$Definition.role)
    $sha256 = Get-BytesSha256 -Bytes $bytes
    if (($bytes.LongLength -ne [long]$Definition.rawBytes) -or
        ($sha256 -cne [string]$Definition.sha256) -or
        ((Get-GitBlobOid -Bytes $bytes) -cne [string]$Definition.blobOid)) {
        Throw-CorpusBlocker (
            "$($Definition.role) committed bytes differ from the pin.")
    }
    return [ordered]@{
        role = [string]$Definition.role
        relativePath = [string]$Definition.relativePath
        commit = [string]$Definition.commit
        blobOid = [string]$Definition.blobOid
        rawBytes = [long]$bytes.LongLength
        sha256 = $sha256
        format = [string]$Definition.format
        matched = $true
        bytes = $bytes
    }
}

function Assert-OnlyDefaultDataStream {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    try {
        $streams = @(Get-Item -LiteralPath $Path -Stream * -ErrorAction Stop)
    }
    catch {
        Throw-CorpusBlocker (
            "$PathOwner stream inventory failed: $($_.Exception.Message)")
    }
    if (Test-Path -LiteralPath $Path -PathType Container) {
        if ($streams.Count -ne 0) {
            Throw-CorpusBlocker (
                "$PathOwner contains a directory alternate stream.")
        }
        return
    }
    if (($streams.Count -ne 1) -or
        ([string]$streams[0].Stream -cne ':$DATA')) {
        Throw-CorpusBlocker (
            "$PathOwner contains a non-default data stream.")
    }
}

function Assert-SafeWindowsOutputPathText {
    param([Parameter(Mandatory = $true)][string]$RequestedPath)
    if ([string]::IsNullOrWhiteSpace($RequestedPath)) {
        Throw-CorpusBlocker 'OutputPath is empty.'
    }
    if ($RequestedPath.StartsWith('\\?\', [StringComparison]::Ordinal) -or
        $RequestedPath.StartsWith('\\.\', [StringComparison]::Ordinal)) {
        Throw-CorpusBlocker (
            'OutputPath uses a Windows device namespace alias.')
    }
    if ([regex]::IsMatch($RequestedPath, '[\x00-\x1F<>"|?*]')) {
        Throw-CorpusBlocker (
            'OutputPath contains a prohibited Windows path character.')
    }
    $normalizedSeparators = $RequestedPath.Replace('/', '\')
    if ([regex]::IsMatch($normalizedSeparators, '^[A-Za-z]:[^\\]')) {
        Throw-CorpusBlocker 'OutputPath uses a drive-relative alias.'
    }
    $rootText = [IO.Path]::GetPathRoot($normalizedSeparators)
    $nonRootText = $normalizedSeparators.Substring($rootText.Length)
    if ($nonRootText.IndexOf(':') -ge 0) {
        Throw-CorpusBlocker 'OutputPath names an alternate data stream.'
    }
    foreach ($segment in @($nonRootText.Split('\'))) {
        if ($segment.Length -eq 0) { continue }
        if (($segment -ceq '.') -or ($segment -ceq '..')) {
            Throw-CorpusBlocker (
                'OutputPath is not already lexically normalized.')
        }
        if ($segment.EndsWith(' ', [StringComparison]::Ordinal) -or
            $segment.EndsWith('.', [StringComparison]::Ordinal)) {
            Throw-CorpusBlocker (
                'OutputPath contains a trailing dot or space alias.')
        }
        $deviceStem = $segment.Split('.')[0]
        if ([regex]::IsMatch(
                $deviceStem,
                '^(CON|PRN|AUX|NUL|CLOCK\$|CONIN\$|CONOUT\$|' +
                'COM([1-9]|\xB9|\xB2|\xB3)|' +
                'LPT([1-9]|\xB9|\xB2|\xB3))$',
                [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            Throw-CorpusBlocker (
                'OutputPath uses a reserved Windows device alias.')
        }
    }
}

function Assert-OutputDescriptorState {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Descriptor,
        [switch]$RequireTargetAbsent,
        [switch]$RequireTargetPresent
    )
    if ($RequireTargetAbsent -and $RequireTargetPresent) {
        Throw-CorpusBlocker (
            'output descriptor state request is contradictory.')
    }
    [string[]]$keys = @($Descriptor.Keys)
    if (($keys.Count -ne 3) -or
        ($keys[0] -cne 'FullPath') -or
        ($keys[1] -cne 'ExactParent') -or
        ($keys[2] -cne 'AllowedRoot')) {
        Throw-CorpusBlocker (
            'output descriptor exact key sequence differs.')
    }
    $fullPath = [IO.Path]::GetFullPath([string]$Descriptor.FullPath)
    $exactParent = [IO.Path]::GetFullPath(
        [string]$Descriptor.ExactParent).TrimEnd('\')
    $allowedRoot = [IO.Path]::GetFullPath(
        [string]$Descriptor.AllowedRoot).TrimEnd('\')
    if (($fullPath -cne [string]$Descriptor.FullPath) -or
        ($exactParent -cne [string]$Descriptor.ExactParent) -or
        ($allowedRoot -cne [string]$Descriptor.AllowedRoot) -or
        ([IO.Path]::GetFileName($fullPath) -cne $ReportFileName) -or
        ([IO.Path]::GetDirectoryName($fullPath).TrimEnd('\') -cne
            $exactParent) -or
        ((Join-Path $exactParent $ReportFileName) -cne $fullPath) -or
        (-not (Test-PathInsideRoot -Path $exactParent -Root $allowedRoot))) {
        Throw-CorpusBlocker (
            'output descriptor normalized identity differs.')
    }
    if ((-not [IO.Directory]::Exists($allowedRoot)) -or
        (-not [IO.Directory]::Exists($exactParent))) {
        Throw-CorpusBlocker (
            'output descriptor root or exact parent is missing.')
    }
    $resolvedAllowedRoot =
        (Resolve-Path -LiteralPath $allowedRoot).Path.TrimEnd('\')
    $resolvedExactParent =
        (Resolve-Path -LiteralPath $exactParent).Path.TrimEnd('\')
    if (($resolvedAllowedRoot -cne $allowedRoot) -or
        ($resolvedExactParent -cne $exactParent)) {
        Throw-CorpusBlocker (
            'output descriptor root or parent resolution differs.')
    }
    Assert-NoReparsePointChain -Path $exactParent -Root $allowedRoot `
        -PathOwner 'output descriptor parent chain'
    $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $exactParent -Force)
    while ($null -ne $cursor) {
        Assert-OnlyDefaultDataStream -Path $cursor.FullName `
            -PathOwner 'output descriptor parent chain'
        if ($cursor.FullName.TrimEnd('\') -ceq $allowedRoot) { break }
        $cursor = $cursor.Parent
    }
    if ($null -eq $cursor) {
        Throw-CorpusBlocker (
            'output descriptor parent chain did not reach its root.')
    }
    $targetExists = [IO.File]::Exists($fullPath) -or
        [IO.Directory]::Exists($fullPath)
    if ($RequireTargetAbsent -and $targetExists) {
        Throw-CorpusBlocker (
            'OutputPath already exists; overwrite is prohibited.')
    }
    if ($RequireTargetPresent) {
        if (-not [IO.File]::Exists($fullPath)) {
            Throw-CorpusBlocker 'CreateNew output target is missing.'
        }
        $target = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
        if (($target -isnot [IO.FileInfo]) -or
            (($target.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) -or
            ($target.FullName -cne $fullPath) -or
            ([string]$target.Name -cne $ReportFileName)) {
            Throw-CorpusBlocker (
                'CreateNew output target identity differs.')
        }
        Assert-OnlyDefaultDataStream -Path $fullPath `
            -PathOwner 'CreateNew output target'
    }
}

function Resolve-CreateNewOutputPath {
    param(
        [Parameter(Mandatory = $true)][string]$RequestedPath,
        [string]$AllowedRoot = $PSScriptRoot,
        [string]$ExactParent = $PSScriptRoot
    )
    Assert-SafeWindowsOutputPathText -RequestedPath $RequestedPath
    $requestedFileName = [IO.Path]::GetFileName($RequestedPath)
    if ($requestedFileName -cne $ReportFileName) {
        Throw-CorpusBlocker (
            'OutputPath must use the exact historical corpus report basename.')
    }
    if ((-not [IO.Directory]::Exists($AllowedRoot)) -or
        (-not [IO.Directory]::Exists($ExactParent))) {
        Throw-CorpusBlocker (
            'OutputPath allowed root or exact parent is missing.')
    }
    $resolvedAllowedRoot =
        (Resolve-Path -LiteralPath $AllowedRoot).Path.TrimEnd('\')
    $resolvedExactParent =
        (Resolve-Path -LiteralPath $ExactParent).Path.TrimEnd('\')
    $combined = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        $RequestedPath
    }
    else { Join-Path $resolvedExactParent $RequestedPath }
    $fullPath = [IO.Path]::GetFullPath($combined)
    if (([IO.Path]::GetFullPath($fullPath) -cne $fullPath) -or
        ([IO.Path]::GetFileName($fullPath) -cne $requestedFileName)) {
        Throw-CorpusBlocker (
            'OutputPath normalized identity differs from the request.')
    }
    $parent = [IO.Path]::GetDirectoryName($fullPath).TrimEnd('\')
    if ($parent -cne $resolvedExactParent) {
        Throw-CorpusBlocker (
            'OutputPath must be a direct child of the exact report parent.')
    }
    $expectedFullPath = Join-Path $resolvedExactParent $ReportFileName
    if (($fullPath -cne $expectedFullPath) -or
        (-not (Test-PathInsideRoot `
                -Path $resolvedExactParent -Root $resolvedAllowedRoot))) {
        Throw-CorpusBlocker (
            'OutputPath differs from the exact historical corpus report path.')
    }
    $descriptor = [ordered]@{
        FullPath = $fullPath
        ExactParent = $resolvedExactParent
        AllowedRoot = $resolvedAllowedRoot
    }
    Assert-OutputDescriptorState `
        -Descriptor $descriptor -RequireTargetAbsent
    return $descriptor
}

function Write-CreateNewBytes {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Descriptor,
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [scriptblock]$BeforeCreateSelfTestHook
    )
    $path = [string]$Descriptor.FullPath
    $stream = $null
    $created = $false
    $completed = $false
    try {
        if ($null -ne $BeforeCreateSelfTestHook) {
            [void](& $BeforeCreateSelfTestHook)
        }
        Assert-OutputDescriptorState `
            -Descriptor $Descriptor -RequireTargetAbsent
        $stream = New-Object IO.FileStream(
            $path,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        $created = $true
        $stream.Write($Bytes, 0, $Bytes.Length)
        $stream.Flush($true)
        $stream.Dispose()
        $stream = $null
        Assert-OutputDescriptorState `
            -Descriptor $Descriptor -RequireTargetPresent
        [byte[]]$readBack = [IO.File]::ReadAllBytes($path)
        Assert-OutputDescriptorState `
            -Descriptor $Descriptor -RequireTargetPresent
        if (($readBack.LongLength -ne $Bytes.LongLength) -or
            ((Get-BytesSha256 -Bytes $readBack) -cne
                (Get-BytesSha256 -Bytes $Bytes))) {
            Throw-CorpusBlocker 'CreateNew output read-back differs.'
        }
        $completed = $true
    }
    catch [IO.IOException] {
        Throw-CorpusBlocker (
            'OutputPath already exists or could not be created with CreateNew.')
    }
    finally {
        if ($null -ne $stream) { $stream.Dispose() }
        if ($created -and (-not $completed)) {
            try {
                Assert-OutputDescriptorState `
                    -Descriptor $Descriptor -RequireTargetPresent
                [IO.File]::Delete($path)
            }
            catch {
                # Never follow an untrusted replacement path during cleanup.
            }
        }
    }
}

function Initialize-CorpusBinaryType {
    if ($null -ne ('CodexLasalHistoricalCorpusBinaryV1' -as [type])) {
        return
    }
    Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public sealed class CodexLasalHistoricalInventoryV1
{
    public string[] Owners;
    public string[] SourcePaths;
    public int[] Starts;
    public int[] Ends;
    public int[] TailOffsets;
    public int[][] MarkerOffsets;
}

public sealed class CodexLasalHistoricalTransitionV1
{
    public bool RawExact;
    public bool CandidateOnlyChanged;
    public bool OutsideCandidateChanged;
    public bool TailSingleTargetCounterexample;
    public int MarkerSingleTargetCounterexamples;
    public bool BothCandidateWordsChanged;
}

public static class CodexLasalHistoricalCorpusBinaryV1
{
    private static readonly byte[] Signature =
        Encoding.ASCII.GetBytes("SigmatekLasal2Binary\0");
    private static readonly byte[] Marker = new byte[] {
        0x8F, 0x68, 0x1A, 0x16, 0x6D, 0xB0,
        0x6E, 0x37, 0x85, 0xCA, 0x73, 0x41 };

    private static bool IsIdentifierStart(byte value)
    {
        return value == (byte)'_' ||
            (value >= (byte)'A' && value <= (byte)'Z') ||
            (value >= (byte)'a' && value <= (byte)'z');
    }

    private static bool IsIdentifierPart(byte value)
    {
        return IsIdentifierStart(value) ||
            (value >= (byte)'0' && value <= (byte)'9');
    }

    private static int ReadLe24(byte[] bytes, int offset)
    {
        return bytes[offset] |
            (bytes[offset + 1] << 8) |
            (bytes[offset + 2] << 16);
    }

    private static bool MatchesAsciiIgnoreCase(
        byte[] bytes, int offset, string value)
    {
        if (offset < 0 || offset + value.Length > bytes.Length) return false;
        for (int index = 0; index < value.Length; index++)
        {
            byte observed = bytes[offset + index];
            byte expected = (byte)value[index];
            if (observed >= (byte)'a' && observed <= (byte)'z')
                observed = (byte)(observed - 32);
            if (expected >= (byte)'a' && expected <= (byte)'z')
                expected = (byte)(expected - 32);
            if (observed != expected) return false;
        }
        return true;
    }

    private static string GetAscii(byte[] bytes, int start, int length)
    {
        return Encoding.ASCII.GetString(bytes, start, length);
    }

    private static bool Matches(byte[] bytes, int offset, byte[] value)
    {
        if (offset < 0 || offset + value.Length > bytes.Length) return false;
        for (int index = 0; index < value.Length; index++)
            if (bytes[offset + index] != value[index]) return false;
        return true;
    }

    private static void AssertSignature(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 32 || !Matches(bytes, 0, Signature))
            throw new FormatException("Classes signature differs");
        for (int offset = 1; offset + Signature.Length <= bytes.Length; offset++)
            if (Matches(bytes, offset, Signature))
                throw new FormatException("Classes signature is duplicated");
    }

    public static CodexLasalHistoricalInventoryV1 ParseInventory(byte[] bytes)
    {
        AssertSignature(bytes);
        List<int> sourcePathStarts = new List<int>();
        List<int> sourceMarkerStarts = new List<int>();
        List<string> sourceOwners = new List<string>();
        List<string> sourcePaths = new List<string>();
        List<int> headerStarts = new List<int>();
        List<string> headerOwners = new List<string>();
        const string sourcePrefix = ".\\Class\\";

        for (int offset = 0; offset < bytes.Length; offset++)
        {
            if (offset + 6 < bytes.Length &&
                bytes[offset] == 0xAA && bytes[offset + 1] == 0x03)
            {
                int nameLength = ReadLe24(bytes, offset + 2);
                int nameStart = offset + 6;
                if (nameLength > 0 && nameLength <= 255 &&
                    bytes[offset + 5] == 0xAA &&
                    nameStart + nameLength <= bytes.Length &&
                    IsIdentifierStart(bytes[nameStart]))
                {
                    bool valid = true;
                    for (int index = 1; index < nameLength; index++)
                    {
                        if (!IsIdentifierPart(bytes[nameStart + index]))
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid &&
                        (nameStart + nameLength == bytes.Length ||
                         !IsIdentifierPart(bytes[nameStart + nameLength])))
                    {
                        headerStarts.Add(offset);
                        headerOwners.Add(
                            GetAscii(bytes, nameStart, nameLength));
                    }
                }
            }

            if (!MatchesAsciiIgnoreCase(bytes, offset, sourcePrefix)) continue;
            int ownerStart = offset + sourcePrefix.Length;
            if (ownerStart >= bytes.Length ||
                !IsIdentifierStart(bytes[ownerStart])) continue;
            int ownerEnd = ownerStart + 1;
            while (ownerEnd < bytes.Length &&
                IsIdentifierPart(bytes[ownerEnd])) ownerEnd++;
            if (ownerEnd >= bytes.Length || bytes[ownerEnd] != (byte)'\\')
                continue;
            int fileStart = ownerEnd + 1;
            if (fileStart >= bytes.Length ||
                !IsIdentifierStart(bytes[fileStart])) continue;
            int fileEnd = fileStart + 1;
            while (fileEnd < bytes.Length &&
                IsIdentifierPart(bytes[fileEnd])) fileEnd++;
            if (!MatchesAsciiIgnoreCase(bytes, fileEnd, ".st")) continue;
            int pathEnd = fileEnd + 3;
            string owner = GetAscii(bytes, ownerStart, ownerEnd - ownerStart);
            string file = GetAscii(bytes, fileStart, fileEnd - fileStart);
            if (!String.Equals(owner, file, StringComparison.Ordinal))
                throw new FormatException(
                    "class owner/file identity differs at byte " + offset);
            int pathLength = pathEnd - offset;
            if (offset < 4 || bytes[offset - 1] != 0xAA ||
                ReadLe24(bytes, offset - 4) != pathLength)
                throw new FormatException(
                    "source marker boundary differs at byte " + offset);
            sourcePathStarts.Add(offset);
            sourceMarkerStarts.Add(offset - 4);
            sourceOwners.Add(owner);
            sourcePaths.Add(GetAscii(bytes, offset, pathLength));
            offset = pathEnd - 1;
        }

        if (sourceOwners.Count < 104 || sourceOwners.Count > 120)
            throw new FormatException(
                "owner inventory is outside the pinned 104..120 range");
        if (headerOwners.Count != sourceOwners.Count - 1)
            throw new FormatException("true-header/source count differs");
        if (!String.Equals(sourceOwners[0], "_AxisBase",
                StringComparison.Ordinal))
            throw new FormatException("first owner is not _AxisBase");

        HashSet<string> seenOwners =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> seenPaths =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int count = sourceOwners.Count;
        int[] starts = new int[count];
        int[] ends = new int[count];
        int[] tails = new int[count];
        int[][] markerOffsets = new int[count][];

        for (int index = 0; index < count; index++)
        {
            if (!seenOwners.Add(sourceOwners[index]) ||
                !seenPaths.Add(sourcePaths[index]))
                throw new FormatException("duplicate owner or source path");
            if (index > 0 &&
                !String.Equals(headerOwners[index - 1], sourceOwners[index],
                    StringComparison.Ordinal))
                throw new FormatException(
                    "true-header/source order differs at ordinal " +
                    (index + 1));
            starts[index] = index == 0 ? 0 : headerStarts[index - 1];
            ends[index] = index + 1 < count ?
                headerStarts[index] : bytes.Length;
            if (ends[index] <= starts[index] ||
                sourceMarkerStarts[index] < starts[index] ||
                sourcePathStarts[index] + sourcePaths[index].Length >
                    ends[index] ||
                ends[index] - 48 < starts[index] ||
                ends[index] - 46 > ends[index])
                throw new FormatException(
                    "owner record boundary differs at ordinal " +
                    (index + 1));
            tails[index] = ends[index] - 48;
            List<int> followers = new List<int>();
            for (int offset = starts[index]; offset < ends[index]; offset++)
            {
                if (Matches(bytes, offset, Marker))
                {
                    if (offset + Marker.Length + 2 > ends[index])
                        throw new FormatException(
                            "marker follower crosses owner record boundary");
                    followers.Add(offset + Marker.Length);
                    offset += Marker.Length - 1;
                }
            }
            markerOffsets[index] = followers.ToArray();
        }

        return new CodexLasalHistoricalInventoryV1 {
            Owners = sourceOwners.ToArray(),
            SourcePaths = sourcePaths.ToArray(),
            Starts = starts,
            Ends = ends,
            TailOffsets = tails,
            MarkerOffsets = markerOffsets
        };
    }

    public static byte[] BuildSyntheticArtifactForSelfTest(int ownerCount)
    {
        if (ownerCount < 1 || ownerCount > 999)
            throw new ArgumentOutOfRangeException("ownerCount");
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
            stream.Write(Signature, 0, Signature.Length);
            for (int ordinal = 0; ordinal < ownerCount; ordinal++)
            {
                string owner = ordinal == 0 ?
                    "_AxisBase" : "C" + ordinal.ToString("D3");
                byte[] ownerBytes = Encoding.ASCII.GetBytes(owner);
                if (ordinal > 0)
                {
                    stream.WriteByte(0xAA);
                    stream.WriteByte(0x03);
                    stream.WriteByte((byte)(ownerBytes.Length & 0xFF));
                    stream.WriteByte((byte)((ownerBytes.Length >> 8) & 0xFF));
                    stream.WriteByte((byte)((ownerBytes.Length >> 16) & 0xFF));
                    stream.WriteByte(0xAA);
                    stream.Write(ownerBytes, 0, ownerBytes.Length);
                    stream.WriteByte(0x00);
                }
                for (int filler = 0; filler < 16; filler++)
                    stream.WriteByte((byte)(0x30 + (filler % 10)));
                byte[] pathBytes = Encoding.ASCII.GetBytes(
                    ".\\Class\\" + owner + "\\" + owner + ".st");
                stream.WriteByte((byte)(pathBytes.Length & 0xFF));
                stream.WriteByte((byte)((pathBytes.Length >> 8) & 0xFF));
                stream.WriteByte((byte)((pathBytes.Length >> 16) & 0xFF));
                stream.WriteByte(0xAA);
                stream.Write(pathBytes, 0, pathBytes.Length);
                for (int filler = 0; filler < 64; filler++)
                    stream.WriteByte((byte)(ordinal + filler));
            }
            return stream.ToArray();
        }
    }

    public static byte[] ReplaceAsciiAllForSelfTest(
        byte[] bytes, string oldValue, string newValue)
    {
        if (bytes == null || oldValue == null || newValue == null ||
            oldValue.Length == 0 || oldValue.Length != newValue.Length)
            throw new ArgumentException("equal nonempty ASCII values required");
        byte[] result = new byte[bytes.Length];
        Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
        byte[] oldBytes = Encoding.ASCII.GetBytes(oldValue);
        byte[] newBytes = Encoding.ASCII.GetBytes(newValue);
        for (int offset = 0; offset + oldBytes.Length <= result.Length; offset++)
        {
            if (!Matches(result, offset, oldBytes)) continue;
            Buffer.BlockCopy(newBytes, 0, result, offset, newBytes.Length);
            offset += oldBytes.Length - 1;
        }
        return result;
    }

    public static string Sha256WordZeroed(
        byte[] bytes, int start, int length, int wordOffset)
    {
        if (bytes == null || start < 0 || length < 2 ||
            start + length > bytes.Length ||
            wordOffset < start || wordOffset + 2 > start + length)
            throw new ArgumentOutOfRangeException("record range");
        byte[] copy = new byte[length];
        Buffer.BlockCopy(bytes, start, copy, 0, length);
        copy[wordOffset - start] = 0;
        copy[wordOffset - start + 1] = 0;
        using (SHA256 algorithm = SHA256.Create())
            return BitConverter.ToString(
                algorithm.ComputeHash(copy)).Replace("-", "");
    }

    public static bool RecordsEqualWordZeroed(
        byte[] left, int leftStart, int leftLength, int leftWordOffset,
        byte[] right, int rightStart, int rightLength, int rightWordOffset)
    {
        if (left == null || right == null || leftStart < 0 || rightStart < 0 ||
            leftLength < 2 || rightLength < 2 ||
            leftStart + leftLength > left.Length ||
            rightStart + rightLength > right.Length ||
            leftWordOffset < leftStart || leftWordOffset + 2 >
                leftStart + leftLength ||
            rightWordOffset < rightStart || rightWordOffset + 2 >
                rightStart + rightLength)
            throw new ArgumentOutOfRangeException("zeroed record comparison");
        if (leftLength != rightLength ||
            leftWordOffset - leftStart != rightWordOffset - rightStart)
            return false;
        int ignored = leftWordOffset - leftStart;
        for (int offset = 0; offset < leftLength; offset++)
        {
            if (offset == ignored || offset == ignored + 1) continue;
            if (left[leftStart + offset] != right[rightStart + offset])
                return false;
        }
        return true;
    }

    public static string Sha256MaskedArtifact(
        byte[] bytes, CodexLasalHistoricalInventoryV1 inventory)
    {
        if (bytes == null || inventory == null)
            throw new ArgumentNullException("masked artifact");
        byte[] copy = new byte[bytes.Length];
        Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
        for (int index = 0; index < inventory.TailOffsets.Length; index++)
        {
            int tail = inventory.TailOffsets[index];
            copy[tail] = 0;
            copy[tail + 1] = 0;
            foreach (int marker in inventory.MarkerOffsets[index])
            {
                copy[marker] = 0;
                copy[marker + 1] = 0;
            }
        }
        using (SHA256 algorithm = SHA256.Create())
            return BitConverter.ToString(
                algorithm.ComputeHash(copy)).Replace("-", "");
    }

    private static bool SameRelativeMarkers(
        CodexLasalHistoricalInventoryV1 left, int leftIndex,
        CodexLasalHistoricalInventoryV1 right, int rightIndex)
    {
        int[] a = left.MarkerOffsets[leftIndex];
        int[] b = right.MarkerOffsets[rightIndex];
        if (a.Length != b.Length) return false;
        for (int index = 0; index < a.Length; index++)
            if (a[index] - left.Starts[leftIndex] !=
                b[index] - right.Starts[rightIndex]) return false;
        return true;
    }

    private static bool RangeEqualIgnoring(
        byte[] left, int leftStart, byte[] right, int rightStart,
        int length, HashSet<int> ignoredRelativeOffsets)
    {
        for (int offset = 0; offset < length; offset++)
            if (!ignoredRelativeOffsets.Contains(offset) &&
                left[leftStart + offset] != right[rightStart + offset])
                return false;
        return true;
    }

    private static ushort ReadWord(byte[] bytes, int offset)
    {
        return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
    }

    public static CodexLasalHistoricalTransitionV1 CompareRecord(
        byte[] leftBytes, CodexLasalHistoricalInventoryV1 left, int leftIndex,
        byte[] rightBytes, CodexLasalHistoricalInventoryV1 right, int rightIndex)
    {
        int leftLength = left.Ends[leftIndex] - left.Starts[leftIndex];
        int rightLength = right.Ends[rightIndex] - right.Starts[rightIndex];
        CodexLasalHistoricalTransitionV1 result =
            new CodexLasalHistoricalTransitionV1();
        if (leftLength != rightLength ||
            !SameRelativeMarkers(left, leftIndex, right, rightIndex))
        {
            result.OutsideCandidateChanged = true;
            return result;
        }
        HashSet<int> none = new HashSet<int>();
        result.RawExact = RangeEqualIgnoring(
            leftBytes, left.Starts[leftIndex],
            rightBytes, right.Starts[rightIndex], leftLength, none);
        if (result.RawExact) return result;

        HashSet<int> candidates = new HashSet<int>();
        int leftTailRelative =
            left.TailOffsets[leftIndex] - left.Starts[leftIndex];
        int rightTailRelative =
            right.TailOffsets[rightIndex] - right.Starts[rightIndex];
        if (leftTailRelative != rightTailRelative)
        {
            result.OutsideCandidateChanged = true;
            return result;
        }
        candidates.Add(leftTailRelative);
        candidates.Add(leftTailRelative + 1);
        for (int index = 0; index < left.MarkerOffsets[leftIndex].Length; index++)
        {
            int relative = left.MarkerOffsets[leftIndex][index] -
                left.Starts[leftIndex];
            candidates.Add(relative);
            candidates.Add(relative + 1);
        }
        result.CandidateOnlyChanged = RangeEqualIgnoring(
            leftBytes, left.Starts[leftIndex],
            rightBytes, right.Starts[rightIndex], leftLength, candidates);
        result.OutsideCandidateChanged = !result.CandidateOnlyChanged;

        HashSet<int> tailOnly = new HashSet<int>();
        tailOnly.Add(leftTailRelative);
        tailOnly.Add(leftTailRelative + 1);
        result.TailSingleTargetCounterexample =
            ReadWord(leftBytes, left.TailOffsets[leftIndex]) !=
                ReadWord(rightBytes, right.TailOffsets[rightIndex]) &&
            RangeEqualIgnoring(
                leftBytes, left.Starts[leftIndex],
                rightBytes, right.Starts[rightIndex], leftLength, tailOnly);

        bool markerChanged = false;
        for (int index = 0; index < left.MarkerOffsets[leftIndex].Length; index++)
        {
            int leftMarker = left.MarkerOffsets[leftIndex][index];
            int rightMarker = right.MarkerOffsets[rightIndex][index];
            int relative = leftMarker - left.Starts[leftIndex];
            if (ReadWord(leftBytes, leftMarker) !=
                ReadWord(rightBytes, rightMarker))
            {
                markerChanged = true;
                HashSet<int> markerOnly = new HashSet<int>();
                markerOnly.Add(relative);
                markerOnly.Add(relative + 1);
                if (RangeEqualIgnoring(
                        leftBytes, left.Starts[leftIndex],
                        rightBytes, right.Starts[rightIndex],
                        leftLength, markerOnly))
                    result.MarkerSingleTargetCounterexamples++;
            }
        }
        result.BothCandidateWordsChanged = result.CandidateOnlyChanged &&
            markerChanged &&
            ReadWord(leftBytes, left.TailOffsets[leftIndex]) !=
                ReadWord(rightBytes, right.TailOffsets[rightIndex]);
        return result;
    }

    private static ushort CrcReflected(
        byte[] bytes, ushort initial, ushort polynomial)
    {
        uint value = initial;
        foreach (byte item in bytes)
        {
            value ^= item;
            for (int bit = 0; bit < 8; bit++)
                value = (value & 1) != 0 ?
                    (value >> 1) ^ polynomial : value >> 1;
        }
        return (ushort)value;
    }

    private static ushort CrcNormal(
        byte[] bytes, ushort initial, ushort polynomial)
    {
        uint value = initial;
        foreach (byte item in bytes)
        {
            value ^= (uint)item << 8;
            for (int bit = 0; bit < 8; bit++)
                value = (value & 0x8000) != 0 ?
                    ((value << 1) ^ polynomial) & 0xFFFF :
                    (value << 1) & 0xFFFF;
        }
        return (ushort)value;
    }

    public static ushort[] GetCrcCheckValues(byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException("bytes");
        return new ushort[] {
            CrcReflected(bytes, 0x0000, 0xA001),
            CrcReflected(bytes, 0xFFFF, 0xA001),
            CrcNormal(bytes, 0xFFFF, 0x1021),
            CrcNormal(bytes, 0x0000, 0x1021),
            CrcReflected(bytes, 0x0000, 0x8408),
            (ushort)(CrcReflected(bytes, 0xFFFF, 0x8408) ^ 0xFFFF)
        };
    }

    public static ushort[] GetBoundedHypothesisValues(
        byte[] bytes, int start, int length, int wordOffset)
    {
        if (bytes == null || start < 0 || length < 2 ||
            start + length > bytes.Length || wordOffset < start ||
            wordOffset + 2 > start + length)
            throw new ArgumentOutOfRangeException("hypothesis range");
        byte[] copy = new byte[length];
        Buffer.BlockCopy(bytes, start, copy, 0, length);
        copy[wordOffset - start] = 0;
        copy[wordOffset - start + 1] = 0;
        ushort[] result = new ushort[15];
        result[0] = CrcReflected(copy, 0x0000, 0xA001);
        result[1] = CrcReflected(copy, 0xFFFF, 0xA001);
        result[2] = CrcNormal(copy, 0xFFFF, 0x1021);
        result[3] = CrcNormal(copy, 0x0000, 0x1021);
        result[4] = CrcReflected(copy, 0x0000, 0x8408);
        result[5] = (ushort)(
            CrcReflected(copy, 0xFFFF, 0x8408) ^ 0xFFFF);
        uint byteSum = 0;
        uint wordSum = 0;
        uint ones = 0;
        uint fletcher1 = 0;
        uint fletcher2 = 0;
        uint fnv = 2166136261;
        for (int index = 0; index < copy.Length; index++)
        {
            byteSum = (byteSum + copy[index]) & 0xFFFF;
            fletcher1 = (fletcher1 + copy[index]) % 255;
            fletcher2 = (fletcher2 + fletcher1) % 255;
            fnv ^= copy[index];
            fnv = unchecked(fnv * 16777619u);
            if ((index & 1) == 0)
            {
                uint word = (uint)copy[index] << 8;
                if (index + 1 < copy.Length) word |= copy[index + 1];
                ones += word;
                while (ones > 0xFFFF)
                    ones = (ones & 0xFFFF) + (ones >> 16);
                uint little = copy[index];
                if (index + 1 < copy.Length)
                    little |= (uint)copy[index + 1] << 8;
                wordSum = (wordSum + little) & 0xFFFF;
            }
        }
        result[6] = (ushort)byteSum;
        result[7] = (ushort)wordSum;
        result[8] = (ushort)(~ones & 0xFFFF);
        result[9] = (ushort)((fletcher2 << 8) | fletcher1);
        result[10] = (ushort)((fnv & 0xFFFF) ^ (fnv >> 16));
        using (SHA256 algorithm = SHA256.Create())
        {
            byte[] hash = algorithm.ComputeHash(copy);
            result[11] = (ushort)(hash[0] | (hash[1] << 8));
            result[12] = (ushort)((hash[0] << 8) | hash[1]);
            result[13] = (ushort)(hash[30] | (hash[31] << 8));
            result[14] = (ushort)((hash[30] << 8) | hash[31]);
        }
        return result;
    }
}
'@
}

function Get-LittleEndianWord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Offset
    )
    if (($Offset -lt 0) -or (($Offset + 2) -gt $Bytes.Length)) {
        Throw-CorpusBlocker 'little-endian word is outside the artifact.'
    }
    return [uint16](([uint16]$Bytes[$Offset]) -bor
        ([uint16]$Bytes[$Offset + 1] -shl 8))
}

function Get-AsciiByteSum16 {
    param([Parameter(Mandatory = $true)][string]$Value)
    $sum = 0
    foreach ($byte in [Text.Encoding]::ASCII.GetBytes($Value)) {
        $sum = ($sum + [int]$byte) -band 0xFFFF
    }
    return [uint16]$sum
}

function Assert-PatchContract {
    param(
        [Parameter(Mandatory = $true)][byte[]]$PatchBytes,
        [Parameter(Mandatory = $true)]$Manifest
    )
    foreach ($value in $PatchBytes) {
        if ($value -gt 0x7F) {
            Throw-CorpusBlocker (
                'known 6E binary patch is not 7-bit ASCII.')
        }
    }
    $text =
        [Text.Encoding]::ASCII.GetString($PatchBytes).Replace("`r`n", "`n")
    $expectedHeader =
        "diff --git a/$CanonicalClassesPath b/$CanonicalClassesPath`n" +
        "index $CheckpointBlobOid..$KnownRebuildBlobOid 100644`n" +
        "GIT binary patch`n"
    if (-not $text.StartsWith($expectedHeader, [StringComparison]::Ordinal)) {
        Throw-CorpusBlocker 'known 6E binary patch header/index differs.'
    }
    if (([regex]::Matches($text, '(?m)^diff --git ')).Count -ne 1 -or
        ([regex]::Matches($text, '(?m)^GIT binary patch$')).Count -ne 1 -or
        ([regex]::Matches($text, '(?m)^delta ')).Count -ne 2 -or
        ([regex]::Matches($text, '(?m)^literal ')).Count -ne 0) {
        Throw-CorpusBlocker (
            'known 6E binary patch section inventory differs.')
    }
    if (($Manifest.Schema -cne 'LasalClassesBinaryDelta/v1') -or
        ([string]$Manifest.CaptureHead -cne $KnownRebuildCaptureHead) -or
        ([string]$Manifest.RelativePath -cne $CanonicalClassesPath) -or
        ([string]$Manifest.Baseline.Commit -cne $CheckpointCommit) -or
        ([string]$Manifest.Baseline.GitBlobSha1 -cne $CheckpointBlobOid) -or
        ([long]$Manifest.Baseline.RawBytes -ne $CheckpointBytes) -or
        ([string]$Manifest.Baseline.Sha256 -cne $CheckpointSha256) -or
        ([string]$Manifest.Captured.GitBlobSha1 -cne $KnownRebuildBlobOid) -or
        ([long]$Manifest.Captured.RawBytes -ne $KnownRebuildBytes) -or
        ([string]$Manifest.Captured.Sha256 -cne $KnownRebuildSha256) -or
        ([int]$Manifest.ObservedDelta.ChangedBytes -ne 99) -or
        ([int]$Manifest.ObservedDelta.ContiguousRuns -ne 58) -or
        ([int]$Manifest.ObservedDelta.OpaqueVendorOwnerSegments -ne 36) -or
        ([long]$Manifest.Patch.RawBytes -ne 2553L) -or
        ([string]$Manifest.Patch.Sha256 -cne
            'AF9A4D32B6F568036E4200BD3F47C9CD63ABB4027D37A1F60BEDB7287731A160') -or
        (-not [bool]$Manifest.Validation.ForwardApplyCheckAgainstBaselineIndex) -or
        (-not [bool]$Manifest.Validation.ReverseApplyCheckAgainstCapturedWorktree) -or
        (-not [bool]$Manifest.Validation.DetachedWorktreeReconstruction) -or
        ([string]$Manifest.Validation.ReconstructedSha256 -cne
            $KnownRebuildSha256) -or
        ([string]$Manifest.Validation.ReconstructedGitBlobSha1 -cne
            $KnownRebuildBlobOid) -or
        [bool]$Manifest.ProductionApproved -or
        [bool]$Manifest.SemanticEquivalenceProven) {
        Throw-CorpusBlocker 'known 6E patch manifest contract differs.'
    }
}

function Convert-HexToBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Hex,
        [Parameter(Mandatory = $true)][string]$ValueOwner
    )
    if (($Hex.Length -eq 0) -or (($Hex.Length % 2) -ne 0) -or
        ($Hex -cnotmatch '^[0-9A-F]+$')) {
        Throw-CorpusBlocker "$ValueOwner is not canonical uppercase hex."
    }
    $result = New-Object byte[] ($Hex.Length / 2)
    for ($index = 0; $index -lt $result.Length; $index++) {
        $result[$index] =
            [Convert]::ToByte($Hex.Substring($index * 2, 2), 16)
    }
    return ,$result
}

function Test-JsonSectionsExact {
    param(
        [Parameter(Mandatory = $true)]$Left,
        [Parameter(Mandatory = $true)]$Right
    )
    return (ConvertTo-DeterministicJson -Value $Left) -ceq
        (ConvertTo-DeterministicJson -Value $Right)
}

function Assert-ExactJsonKeys {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$ObjectOwner
    )
    if ($null -eq $Object) {
        Throw-CorpusBlocker "$ObjectOwner is null."
    }
    [string[]]$actual = @($Object.PSObject.Properties.Name)
    if (($actual.Count -ne $Expected.Count) -or
        ((ConvertTo-DeterministicJson -Value $actual) -cne
            (ConvertTo-DeterministicJson -Value $Expected))) {
        Throw-CorpusBlocker "$ObjectOwner exact key sequence differs."
    }
}

function Test-JsonInteger {
    param($Value)
    return ($Value -is [byte]) -or ($Value -is [sbyte]) -or
        ($Value -is [int16]) -or ($Value -is [uint16]) -or
        ($Value -is [int32]) -or ($Value -is [uint32]) -or
        ($Value -is [int64]) -or ($Value -is [uint64])
}

function Assert-OracleJsonShape {
    param(
        [Parameter(Mandatory = $true)]$Oracle,
        [Parameter(Mandatory = $true)][string]$OracleOwner
    )
    Assert-ExactJsonKeys -Object $Oracle -ObjectOwner $OracleOwner -Expected @(
        'schema', 'decision', 'checkpoint', 'candidate', 'comparison',
        'recordParser', 'gateDTargetRecords', 'protectedDependencyRecords',
        'changedCheckpointOwners', 'diffRuns')
    Assert-ExactJsonKeys -Object $Oracle.decision `
        -ObjectOwner "$OracleOwner.decision" -Expected @(
            'disposition', 'checkpointIdentityAccepted', 'approvalScope',
            'productionApproved', 'exactCheckpointMatch',
            'semanticEquivalenceProven', 'recordEqualityCannotApproveArtifact',
            'exitCode')
    Assert-ExactJsonKeys -Object $Oracle.checkpoint `
        -ObjectOwner "$OracleOwner.checkpoint" -Expected @(
            'requested', 'kind', 'resolvedRevision', 'relativePath', 'blobOid',
            'rawBytes', 'sha256')
    Assert-ExactJsonKeys -Object $Oracle.candidate `
        -ObjectOwner "$OracleOwner.candidate" -Expected @(
            'path', 'rawBytes', 'sha256')
    Assert-ExactJsonKeys -Object $Oracle.comparison `
        -ObjectOwner "$OracleOwner.comparison" -Expected @(
            'byteExact', 'equalLength', 'lengthDelta', 'alignment',
            'changedByteCountDefined', 'changedByteCount',
            'contiguousRunCount', 'checkpointChangedOwnerCount',
            'unmappedRunCount', 'changedOwnersAreFrozenOpaqueSubset',
            'frozenOpaqueOwnerCount', 'frozenOpaqueOwners',
            'proprietaryFieldSemanticsDecoded')
    foreach ($value in @(
            $Oracle.decision.exitCode, $Oracle.checkpoint.rawBytes,
            $Oracle.candidate.rawBytes, $Oracle.comparison.lengthDelta,
            $Oracle.comparison.changedByteCount,
            $Oracle.comparison.contiguousRunCount,
            $Oracle.comparison.checkpointChangedOwnerCount,
            $Oracle.comparison.unmappedRunCount,
            $Oracle.comparison.frozenOpaqueOwnerCount)) {
        if (-not (Test-JsonInteger -Value $value)) {
            Throw-CorpusBlocker (
                "$OracleOwner contains a non-integer numeric field.")
        }
    }
    foreach ($value in @(
            $Oracle.decision.checkpointIdentityAccepted,
            $Oracle.decision.productionApproved,
            $Oracle.decision.exactCheckpointMatch,
            $Oracle.decision.semanticEquivalenceProven,
            $Oracle.decision.recordEqualityCannotApproveArtifact,
            $Oracle.comparison.byteExact, $Oracle.comparison.equalLength,
            $Oracle.comparison.changedByteCountDefined,
            $Oracle.comparison.changedOwnersAreFrozenOpaqueSubset,
            $Oracle.comparison.proprietaryFieldSemanticsDecoded)) {
        if ($value -isnot [bool]) {
            Throw-CorpusBlocker (
                "$OracleOwner contains a non-Boolean contract field.")
        }
    }
    if (($Oracle.diffRuns -isnot [Array]) -or
        ($Oracle.changedCheckpointOwners -isnot [Array]) -or
        ($Oracle.comparison.frozenOpaqueOwners -isnot [Array])) {
        Throw-CorpusBlocker (
            "$OracleOwner contains a non-array contract field.")
    }
    foreach ($summary in @($Oracle.changedCheckpointOwners)) {
        Assert-ExactJsonKeys -Object $summary `
            -ObjectOwner "$OracleOwner.changedCheckpointOwners[]" -Expected @(
                'owner', 'sourcePath', 'diffRunCount',
                'changedCheckpointBytes', 'classification')
        if ((-not (Test-JsonInteger $summary.diffRunCount)) -or
            (-not (Test-JsonInteger $summary.changedCheckpointBytes))) {
            Throw-CorpusBlocker (
                "$OracleOwner changed-owner counts are not integers.")
        }
    }
    foreach ($run in @($Oracle.diffRuns)) {
        Assert-ExactJsonKeys -Object $run `
            -ObjectOwner "$OracleOwner.diffRuns[]" -Expected @(
                'ordinal', 'checkpointStart', 'checkpointBytes',
                'candidateStart', 'candidateBytes', 'checkpointPreview',
                'candidatePreview', 'checkpointOwners', 'candidateOwners',
                'mappingComplete')
        foreach ($previewName in @('checkpointPreview', 'candidatePreview')) {
            $preview = $run.$previewName
            Assert-ExactJsonKeys -Object $preview `
                -ObjectOwner "$OracleOwner.$previewName" -Expected @(
                    'hex', 'previewBytes', 'truncated')
            if ((-not (Test-JsonInteger $preview.previewBytes)) -or
                ($preview.truncated -isnot [bool]) -or
                ($preview.hex -isnot [string])) {
                Throw-CorpusBlocker (
                    "$OracleOwner preview property type differs.")
            }
        }
        foreach ($ownersName in @('checkpointOwners', 'candidateOwners')) {
            if ($run.$ownersName -isnot [Array]) {
                Throw-CorpusBlocker (
                    "$OracleOwner $ownersName is not an array.")
            }
            foreach ($mapping in @($run.$ownersName)) {
                Assert-ExactJsonKeys -Object $mapping `
                    -ObjectOwner "$OracleOwner.$ownersName[]" -Expected @(
                        'owner', 'sourcePath', 'recordStart',
                        'recordEndExclusive', 'overlapStart', 'overlapBytes')
                foreach ($value in @(
                        $mapping.recordStart, $mapping.recordEndExclusive,
                        $mapping.overlapStart, $mapping.overlapBytes)) {
                    if (-not (Test-JsonInteger $value)) {
                        Throw-CorpusBlocker (
                            "$OracleOwner owner mapping is not integer typed.")
                    }
                }
            }
        }
        foreach ($value in @(
                $run.ordinal, $run.checkpointStart, $run.checkpointBytes,
                $run.candidateStart, $run.candidateBytes)) {
            if (-not (Test-JsonInteger $value)) {
                Throw-CorpusBlocker (
                    "$OracleOwner run field is not integer typed.")
            }
        }
        if ($run.mappingComplete -isnot [bool]) {
            Throw-CorpusBlocker (
                "$OracleOwner mappingComplete is not Boolean.")
        }
    }
}

function Get-OracleReconstruction {
    param(
        [Parameter(Mandatory = $true)]$Oracle,
        [Parameter(Mandatory = $true)][byte[]]$CheckpointArtifact,
        [Parameter(Mandatory = $true)][string]$OracleOwner
    )
    Assert-OracleJsonShape -Oracle $Oracle -OracleOwner $OracleOwner
    if (($Oracle.schema -cne 'LasalClassesArtifactComparison/v1') -or
        ($Oracle.decision.disposition -cne
            'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT') -or
        ([int]$Oracle.decision.exitCode -ne 2) -or
        [bool]$Oracle.decision.checkpointIdentityAccepted -or
        [bool]$Oracle.decision.productionApproved -or
        [bool]$Oracle.decision.exactCheckpointMatch -or
        [bool]$Oracle.decision.semanticEquivalenceProven -or
        ([string]$Oracle.checkpoint.requested -cne $CheckpointCommit) -or
        ([string]$Oracle.checkpoint.resolvedRevision -cne $CheckpointCommit) -or
        ([string]$Oracle.checkpoint.relativePath -cne $CanonicalClassesPath) -or
        ([string]$Oracle.checkpoint.blobOid -cne $CheckpointBlobOid) -or
        ([long]$Oracle.checkpoint.rawBytes -ne $CheckpointBytes) -or
        ([string]$Oracle.checkpoint.sha256 -cne $CheckpointSha256) -or
        ([long]$Oracle.candidate.rawBytes -ne $KnownRebuildBytes) -or
        ([string]$Oracle.candidate.sha256 -cne $KnownRebuildSha256) -or
        [bool]$Oracle.comparison.byteExact -or
        (-not [bool]$Oracle.comparison.equalLength) -or
        ([long]$Oracle.comparison.lengthDelta -ne 0L) -or
        ($Oracle.comparison.alignment -cne 'equal-length-indexed') -or
        (-not [bool]$Oracle.comparison.changedByteCountDefined) -or
        ([int]$Oracle.comparison.changedByteCount -ne 99) -or
        ([int]$Oracle.comparison.contiguousRunCount -ne 58) -or
        ([int]$Oracle.comparison.checkpointChangedOwnerCount -ne 36) -or
        ([int]$Oracle.comparison.unmappedRunCount -ne 0) -or
        (-not [bool]$Oracle.comparison.changedOwnersAreFrozenOpaqueSubset) -or
        [bool]$Oracle.comparison.proprietaryFieldSemanticsDecoded -or
        (-not [bool]$Oracle.gateDTargetRecords.allEqual) -or
        (-not [bool]$Oracle.protectedDependencyRecords.allEqual) -or
        (@($Oracle.diffRuns).Count -ne 58) -or
        (@($Oracle.changedCheckpointOwners).Count -ne 36)) {
        Throw-CorpusBlocker "$OracleOwner common contract differs."
    }
    foreach ($record in @($Oracle.gateDTargetRecords.records)) {
        if (-not [bool]$record.exact) {
            Throw-CorpusBlocker (
                "$OracleOwner has a non-exact Gate D record.")
        }
    }
    foreach ($record in @($Oracle.protectedDependencyRecords.records)) {
        if ((-not [bool]$record.exact) -or
            (-not [bool]$record.legacyWindowExact)) {
            Throw-CorpusBlocker (
                "$OracleOwner has a non-exact protected record.")
        }
    }

    [byte[]]$candidate = Copy-Bytes -Bytes $CheckpointArtifact
    $offsets = New-Object Collections.Generic.List[int]
    $boundaries = @{}
    $previousEnd = -2
    $changedSum = 0
    for ($runIndex = 0; $runIndex -lt $Oracle.diffRuns.Count; $runIndex++) {
        $run = $Oracle.diffRuns[$runIndex]
        $ordinal = $runIndex + 1
        $start = [int]$run.checkpointStart
        $length = [int]$run.checkpointBytes
        if (([int]$run.ordinal -ne $ordinal) -or
            ($start -ne [int]$run.candidateStart) -or
            ($length -ne [int]$run.candidateBytes) -or
            ($length -notin @(1, 2)) -or
            ($start -le ($previousEnd + 1)) -or
            ($start -lt 0) -or (($start + $length) -gt $candidate.Length) -or
            (-not [bool]$run.mappingComplete) -or
            [bool]$run.checkpointPreview.truncated -or
            [bool]$run.candidatePreview.truncated -or
            ([int]$run.checkpointPreview.previewBytes -ne $length) -or
            ([int]$run.candidatePreview.previewBytes -ne $length)) {
            Throw-CorpusBlocker (
                "$OracleOwner diff run $ordinal shape differs.")
        }
        $checkpointHex = [string]$run.checkpointPreview.hex
        $candidateHex = [string]$run.candidatePreview.hex
        [byte[]]$checkpointRun = Convert-HexToBytes `
            -Hex $checkpointHex `
            -ValueOwner "$OracleOwner checkpoint preview $ordinal"
        [byte[]]$candidateRun = Convert-HexToBytes `
            -Hex $candidateHex `
            -ValueOwner "$OracleOwner candidate preview $ordinal"
        if (($checkpointRun.Length -ne $length) -or
            ($candidateRun.Length -ne $length) -or
            ((Get-HexRange -Bytes $CheckpointArtifact `
                    -Start $start -Length $length) -cne $checkpointHex)) {
            Throw-CorpusBlocker (
                "$OracleOwner diff run $ordinal preview differs.")
        }
        for ($byteIndex = 0; $byteIndex -lt $length; $byteIndex++) {
            if ($checkpointRun[$byteIndex] -eq $candidateRun[$byteIndex]) {
                Throw-CorpusBlocker (
                    "$OracleOwner diff run $ordinal contains equal bytes.")
            }
            $candidate[$start + $byteIndex] = $candidateRun[$byteIndex]
            [void]$offsets.Add($start + $byteIndex)
        }
        $checkpointOwners = @($run.checkpointOwners)
        $candidateOwners = @($run.candidateOwners)
        if (($checkpointOwners.Count -ne 1) -or
            ($candidateOwners.Count -ne 1)) {
            Throw-CorpusBlocker (
                "$OracleOwner diff run $ordinal owner mapping differs.")
        }
        $checkpointOwner = $checkpointOwners[0]
        $candidateOwner = $candidateOwners[0]
        if (($checkpointOwner.owner -cne $candidateOwner.owner) -or
            ($checkpointOwner.sourcePath -cne $candidateOwner.sourcePath) -or
            ([int]$checkpointOwner.recordStart -ne
                [int]$candidateOwner.recordStart) -or
            ([int]$checkpointOwner.recordEndExclusive -ne
                [int]$candidateOwner.recordEndExclusive) -or
            ([int]$checkpointOwner.overlapStart -ne $start) -or
            ([int]$checkpointOwner.overlapBytes -ne $length)) {
            Throw-CorpusBlocker (
                "$OracleOwner diff run $ordinal owner fields differ.")
        }
        $ownerName = [string]$checkpointOwner.owner
        $boundary = [ordered]@{
            owner = $ownerName
            sourcePath = [string]$checkpointOwner.sourcePath
            recordStart = [int]$checkpointOwner.recordStart
            recordEndExclusive = [int]$checkpointOwner.recordEndExclusive
        }
        if ($boundaries.ContainsKey($ownerName)) {
            if (-not (Test-JsonSectionsExact `
                    -Left $boundaries[$ownerName] -Right $boundary)) {
                Throw-CorpusBlocker (
                    "$OracleOwner owner boundary is inconsistent.")
            }
        }
        else { $boundaries[$ownerName] = $boundary }
        $previousEnd = $start + $length - 1
        $changedSum += $length
    }
    if (($changedSum -ne 99) -or ($offsets.Count -ne 99) -or
        ($boundaries.Count -ne 36) -or
        ($candidate.LongLength -ne $KnownRebuildBytes) -or
        ((Get-BytesSha256 -Bytes $candidate) -cne $KnownRebuildSha256) -or
        ((Get-GitBlobOid -Bytes $candidate) -cne $KnownRebuildBlobOid)) {
        Throw-CorpusBlocker (
            "$OracleOwner reconstructed candidate identity differs.")
    }
    $changedNames = @($Oracle.changedCheckpointOwners | ForEach-Object {
            [string]$_.owner
        })
    $boundaryNames = @($boundaries.Keys | Sort-Object)
    $summaryNames = @($changedNames | Sort-Object)
    if ((ConvertTo-DeterministicJson -Value $boundaryNames) -cne
        (ConvertTo-DeterministicJson -Value $summaryNames)) {
        Throw-CorpusBlocker (
            "$OracleOwner changed owner summary differs.")
    }
    foreach ($name in $changedNames) {
        if ($FrozenOpaqueVendorOwners -cnotcontains $name) {
            Throw-CorpusBlocker "$OracleOwner includes a non-frozen owner."
        }
    }
    return [pscustomobject]@{
        CandidateBytes = $candidate
        ChangedOffsets = $offsets.ToArray()
        OwnerBoundaries = $boundaries
        ChangedOwnerNames = $changedNames
    }
}

function Get-AsciiSha256 {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$TextOwner
    )
    if ([regex]::IsMatch($Text, '[^\x00-\x7F]')) {
        Throw-CorpusBlocker "$TextOwner is not 7-bit ASCII."
    }
    return Get-BytesSha256 -Bytes ([Text.Encoding]::ASCII.GetBytes($Text))
}

function Get-OrdinalSortedStrings {
    param([Parameter(Mandatory = $true)][object[]]$Values)
    [string[]]$items = @($Values | ForEach-Object { [string]$_ })
    [Array]::Sort($items, [StringComparer]::Ordinal)
    return ,$items
}

function New-ArtifactObservation {
    param(
        [Parameter(Mandatory = $true)][string]$Role,
        [Parameter(Mandatory = $true)][string]$ArtifactId,
        [Parameter(Mandatory = $true)][long]$RawBytes,
        [Parameter(Mandatory = $true)][string]$Sha256,
        [Parameter(Mandatory = $true)][byte[]]$Bytes
    )
    Initialize-CorpusBinaryType
    if (($Bytes.LongLength -ne $RawBytes) -or
        ((Get-BytesSha256 -Bytes $Bytes) -cne $Sha256) -or
        ((Get-GitBlobOid -Bytes $Bytes) -cne $ArtifactId)) {
        Throw-CorpusBlocker "$Role artifact identity differs before parsing."
    }
    try {
        $inventory =
            [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($Bytes)
    }
    catch {
        Throw-CorpusBlocker "$Role inventory parse failed: $($_.Exception.Message)"
    }
    $topologyLines = New-Object Collections.Generic.List[string]
    $samples = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $inventory.Owners.Length; $index++) {
        $ownerName = [string]$inventory.Owners[$index]
        $sourcePath = [string]$inventory.SourcePaths[$index]
        $recordStart = [int]$inventory.Starts[$index]
        $recordEnd = [int]$inventory.Ends[$index]
        $recordLength = $recordEnd - $recordStart
        [void]$topologyLines.Add(
            "$(($index + 1))|$ownerName|$sourcePath")
        $tailOffset = [int]$inventory.TailOffsets[$index]
        $tailContext =
            [CodexLasalHistoricalCorpusBinaryV1]::Sha256WordZeroed(
                $Bytes, $recordStart, $recordLength, $tailOffset)
        [void]$samples.Add([pscustomobject]@{
                Family = 'tail'
                Owner = $ownerName
                SourcePath = $sourcePath
                RecordLength = [int]$recordLength
                TargetRelativeOffset = [int]($tailOffset - $recordStart)
                ContextSha256 = [string]$tailContext
                TargetValue = [uint16](Get-LittleEndianWord `
                    -Bytes $Bytes -Offset $tailOffset)
                TargetOffset = [int]$tailOffset
                RecordStart = [int]$recordStart
                RecordIndex = [int]$index
                ArtifactId = $ArtifactId
                ArtifactRole = $Role
                Bytes = $Bytes
                Inventory = $inventory
            })
        foreach ($markerOffset in @($inventory.MarkerOffsets[$index])) {
            $marker = [int]$markerOffset
            $markerContext =
                [CodexLasalHistoricalCorpusBinaryV1]::Sha256WordZeroed(
                    $Bytes, $recordStart, $recordLength, $marker)
            [void]$samples.Add([pscustomobject]@{
                    Family = 'marker'
                    Owner = $ownerName
                    SourcePath = $sourcePath
                    RecordLength = [int]$recordLength
                    TargetRelativeOffset = [int]($marker - $recordStart)
                    ContextSha256 = [string]$markerContext
                    TargetValue = [uint16](Get-LittleEndianWord `
                        -Bytes $Bytes -Offset $marker)
                    TargetOffset = [int]$marker
                    RecordStart = [int]$recordStart
                    RecordIndex = [int]$index
                    ArtifactId = $ArtifactId
                    ArtifactRole = $Role
                    Bytes = $Bytes
                    Inventory = $inventory
                })
        }
    }
    $topologyText = [string]::Join("`n", $topologyLines.ToArray())
    $maskedSha256 =
        [CodexLasalHistoricalCorpusBinaryV1]::Sha256MaskedArtifact(
            $Bytes, $inventory)
    return [pscustomobject]@{
        Role = $Role
        ArtifactId = $ArtifactId
        RawBytes = [long]$RawBytes
        Sha256 = $Sha256
        Bytes = $Bytes
        Inventory = $inventory
        Samples = $samples.ToArray()
        RecordCount = [int]$inventory.Owners.Length
        MarkerCount = [int](@($samples | Where-Object {
                    $_.Family -ceq 'marker'
                }).Count)
        TopologySha256 = Get-AsciiSha256 `
            -Text $topologyText -TextOwner "$Role topology serialization"
        BothSlotMaskedSha256 = [string]$maskedSha256
    }
}

function Get-CanonicalHistoryCorpus {
    param([Parameter(Mandatory = $true)][string]$Root)
    if ($HistoryTable.Count -ne 22) {
        Throw-CorpusBlocker 'frozen canonical history table is not 22 rows.'
    }
    $serializedRows = New-Object Collections.Generic.List[string]
    $publicRows = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $HistoryTable.Count; $index++) {
        $row = $HistoryTable[$index]
        if (($row.Count -ne 4) -or
            ([string]$row[0] -cnotmatch '^[0-9a-f]{40}$') -or
            ([string]$row[1] -cnotmatch '^[0-9a-f]{40}$') -or
            ([long]$row[2] -le 0) -or
            ([string]$row[3] -cnotmatch '^[0-9A-F]{64}$')) {
            Throw-CorpusBlocker 'frozen canonical history row shape differs.'
        }
        $ordinal = $index + 1
        [void]$serializedRows.Add(
            "$ordinal|$($row[0])|$CanonicalClassesPath|$($row[1])|" +
            "$($row[2])|$($row[3])")
        [void]$publicRows.Add([ordered]@{
                ordinal = [int]$ordinal
                commit = [string]$row[0]
                relativePath = $CanonicalClassesPath
                blobOid = [string]$row[1]
                rawBytes = [long]$row[2]
                sha256 = [string]$row[3]
                mode = '100644'
            })
    }
    $tableText = [string]::Join("`n", $serializedRows.ToArray())
    [byte[]]$tableBytes = [Text.Encoding]::ASCII.GetBytes($tableText)
    $tableSha256 = Get-BytesSha256 -Bytes $tableBytes
    if (($tableBytes.LongLength -ne 4632L) -or
        ($tableSha256 -cne $CanonicalTableSha256) -or
        ($tableBytes[$tableBytes.Length - 1] -in @(10, 13))) {
        Throw-CorpusBlocker 'canonical history table serialization differs.'
    }

    $selectedText = Invoke-GitText -Root $Root `
        -Arguments @(
            'rev-list', '--first-parent', '--reverse', $AnchorCommit,
            '--', $CanonicalClassesPath) `
        -Operation 'canonical oldest-to-newest history selection'
    [string[]]$selected = @([regex]::Split($selectedText, '\r?\n'))
    if ($selected.Count -ne 22) {
        Throw-CorpusBlocker 'canonical history selector did not return 22 rows.'
    }
    for ($index = 0; $index -lt $selected.Count; $index++) {
        if ($selected[$index] -cne [string]$HistoryTable[$index][0]) {
            Throw-CorpusBlocker (
                'canonical history selector order/identity differs at ' +
                "ordinal $($index + 1).")
        }
    }
    if (($HistoryTable[15][1] -cne $HistoryTable[17][1]) -or
        ($HistoryTable[16][1] -cne $HistoryTable[18][1]) -or
        ($HistoryTable[15][1] -cne
            'c3520b3de9feec5177c153f1b85436e85cd1e092') -or
        ($HistoryTable[16][1] -cne
            'b26fae6a3702997b92045596a3b709c5a15fb7e3')) {
        Throw-CorpusBlocker 'canonical duplicate occurrence contract differs.'
    }

    $artifactByBlob = @{}
    $artifacts = New-Object Collections.Generic.List[object]
    $occurrences = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $HistoryTable.Count; $index++) {
        $row = $HistoryTable[$index]
        $commit = [string]$row[0]
        $blobOid = [string]$row[1]
        $treeLine = Invoke-GitText -Root $Root `
            -Arguments @('ls-tree', $commit, '--', $CanonicalClassesPath) `
            -Operation "canonical row $($index + 1) tree resolution"
        $match = [regex]::Match(
            $treeLine, '^100644 blob ([0-9a-f]{40})\t(.+)$')
        if ((-not $match.Success) -or
            ($match.Groups[1].Value -cne $blobOid) -or
            ($match.Groups[2].Value -cne $CanonicalClassesPath)) {
            Throw-CorpusBlocker (
                "canonical row $($index + 1) mode/path/blob differs.")
        }
        if (-not $artifactByBlob.ContainsKey($blobOid)) {
            [byte[]]$bytes = Read-GitBlobBytes -Root $Root `
                -BlobOid $blobOid `
                -BlobOwner "canonical history blob $blobOid"
            if (($bytes.LongLength -ne [long]$row[2]) -or
                ((Get-BytesSha256 -Bytes $bytes) -cne [string]$row[3]) -or
                ((Get-GitBlobOid -Bytes $bytes) -cne $blobOid)) {
                Throw-CorpusBlocker (
                    "canonical history blob $blobOid differs from its pin.")
            }
            $artifact = New-ArtifactObservation `
                -Role 'CANONICAL_MAINLINE_UNIQUE_ARTIFACT' `
                -ArtifactId $blobOid -RawBytes ([long]$row[2]) `
                -Sha256 ([string]$row[3]) -Bytes $bytes
            $artifactByBlob[$blobOid] = $artifact
            [void]$artifacts.Add($artifact)
        }
        [void]$occurrences.Add([pscustomobject]@{
                Ordinal = [int]($index + 1)
                Commit = $commit
                Artifact = $artifactByBlob[$blobOid]
            })
    }
    if (($artifacts.Count -ne 20) -or ($occurrences.Count -ne 22)) {
        Throw-CorpusBlocker (
            'canonical occurrence/artifact de-duplication differs.')
    }
    return [pscustomobject]@{
        Table = $publicRows.ToArray()
        TableRawBytes = [long]$tableBytes.LongLength
        TableSha256 = $tableSha256
        Artifacts = $artifacts.ToArray()
        Occurrences = $occurrences.ToArray()
    }
}

function Get-ExclusionEvidence {
    param([Parameter(Mandatory = $true)][string]$Root)
    $branchCommit = 'f4c7bb1614fc90e22e635cfdbdfd7df0a5af0ebb'
    $ancestorExit = Invoke-GitExitCode -Root $Root `
        -Arguments @('merge-base', '--is-ancestor', $branchCommit, $AnchorCommit) `
        -Operation 'branch duplicate ancestor exclusion'
    if ($ancestorExit -ne 1) {
        Throw-CorpusBlocker (
            'f4c7 branch duplicate ancestor exclusion differs.')
    }
    $branchTree = Invoke-GitText -Root $Root `
        -Arguments @('ls-tree', $branchCommit, '--', $CanonicalClassesPath) `
        -Operation 'branch duplicate path resolution'
    if ($branchTree -cne
        "100644 blob 38cf50fba87d3f5e37d1d1dd3bb2ec20cfc7b932`t$CanonicalClassesPath") {
        Throw-CorpusBlocker 'f4c7 branch duplicate blob identity differs.'
    }
    $oldPath = 'Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
    $followOnly = @(
        'c97bc4ecfac6f1487ab91a8729dd8ef5516bcdeb',
        '646a2909545aff8dfd3a65aec62765441861c255',
        'a461d8ba441a8339b6d97620738741c3365cce5f',
        '5d40d31808e2ba073b6ec70e04f675d97508fbba')
    $rows = New-Object Collections.Generic.List[object]
    [string[]]$canonicalCommits = @($HistoryTable | ForEach-Object {
            [string]$_[0]
        })
    foreach ($commit in $followOnly) {
        if ($canonicalCommits -contains $commit) {
            Throw-CorpusBlocker 'pre-canonical follow-only commit leaked in.'
        }
        $line = Invoke-GitText -Root $Root `
            -Arguments @('ls-tree', $commit, '--', $oldPath) `
            -Operation 'pre-canonical follow-only path resolution'
        $match = [regex]::Match(
            $line, '^100644 blob ([0-9a-f]{40})\t(.+)$')
        if ((-not $match.Success) -or
            ($match.Groups[2].Value -cne $oldPath)) {
            Throw-CorpusBlocker (
                'pre-canonical follow-only path identity differs.')
        }
        [void]$rows.Add([ordered]@{
                commit = $commit
                oldRelativePath = $oldPath
                blobOid = $match.Groups[1].Value
                reason = 'PRECANONICAL_PATH_VISIBLE_ONLY_WITH_FOLLOW'
            })
    }
    return [ordered]@{
        branchDuplicate = [ordered]@{
            commit = $branchCommit
            blobOid = '38cf50fba87d3f5e37d1d1dd3bb2ec20cfc7b932'
            reason = 'NOT_ANCESTOR_BRANCH_DUPLICATE'
            anchorAncestor = $false
        }
        preCanonicalFollowOnly = $rows.ToArray()
        allRefsFloatingState = [ordered]@{
            included = $false
            reason = 'ALL_REFS_NOT_PART_OF_PINNED_FIRST_PARENT_SELECTOR'
        }
        mutableWorktreeClasses = [ordered]@{
            read = $false
            included = $false
            reason = 'MUTABLE_WORKTREE_FORBIDDEN'
        }
        localKnownRebuildObject = [ordered]@{
            objectId = $KnownRebuildBlobOid
            required = $false
            read = $false
            reason = 'RECONSTRUCTED_FROM_FULL_COMMITTED_ORACLE_IN_MEMORY'
        }
    }
}

function Get-GroupKey {
    param([Parameter(Mandatory = $true)]$Sample)
    return [string]::Join([char]0x1F, @(
            [string]$Sample.Family,
            [string]$Sample.Owner,
            [string]$Sample.SourcePath,
            [string]$Sample.RecordLength,
            [string]$Sample.TargetRelativeOffset,
            [string]$Sample.ContextSha256))
}

function Get-FamilyGrouping {
    param(
        [Parameter(Mandatory = $true)][object[]]$Samples,
        [Parameter(Mandatory = $true)][string]$Family,
        [switch]$IncludeVaryingDetails
    )
    $groups = @{}
    foreach ($sample in @($Samples | Where-Object {
                $_.Family -ceq $Family
            })) {
        $key = Get-GroupKey -Sample $sample
        if (-not $groups.ContainsKey($key)) {
            $groups[$key] = New-Object Collections.Generic.List[object]
        }
        elseif ($groups[$key].Count -gt 0) {
            $representative = $groups[$key][0]
            if (-not [CodexLasalHistoricalCorpusBinaryV1]::RecordsEqualWordZeroed(
                    $representative.Bytes,
                    [int]$representative.RecordStart,
                    [int]$representative.RecordLength,
                    [int]$representative.TargetOffset,
                    $sample.Bytes,
                    [int]$sample.RecordStart,
                    [int]$sample.RecordLength,
                    [int]$sample.TargetOffset)) {
                Throw-CorpusBlocker (
                    "$Family context SHA collision or key contamination detected.")
            }
        }
        [void]$groups[$key].Add($sample)
    }
    [string[]]$keys = Get-OrdinalSortedStrings -Values @($groups.Keys)
    $multiGroups = 0
    $multiSamples = 0
    $varyingGroups = 0
    $varyingSamples = 0
    $pairCount = 0
    $varyingOwners = @{}
    $details = New-Object Collections.Generic.List[object]
    $serialized = New-Object Collections.Generic.List[string]
    foreach ($key in $keys) {
        $items = @($groups[$key].ToArray())
        foreach ($item in @($items | Sort-Object ArtifactId)) {
            [void]$serialized.Add([string]::Join('|', @(
                        $item.Family, $item.Owner, $item.SourcePath,
                        $item.RecordLength, $item.TargetRelativeOffset,
                        $item.ContextSha256, $item.ArtifactId,
                        ([string][int]$item.TargetValue))))
        }
        $artifactIds = @($items | ForEach-Object {
                $_.ArtifactId
            } | Sort-Object -Unique)
        if ($artifactIds.Count -ne $items.Count) {
            Throw-CorpusBlocker (
                "$Family group contains duplicate samples from one artifact.")
        }
        $values = @($items | ForEach-Object {
                [int]$_.TargetValue
            } | Sort-Object -Unique)
        if ($artifactIds.Count -ge 2) {
            $multiGroups++
            $multiSamples += $items.Count
        }
        if (($artifactIds.Count -ge 2) -and ($values.Count -ge 2)) {
            $varyingGroups++
            $varyingSamples += $items.Count
            foreach ($item in $items) {
                $varyingOwners[[string]$item.Owner] = $true
            }
            for ($left = 0; $left -lt $items.Count; $left++) {
                for ($right = $left + 1; $right -lt $items.Count; $right++) {
                    if (($items[$left].ArtifactId -cne
                            $items[$right].ArtifactId) -and
                        ([int]$items[$left].TargetValue -ne
                            [int]$items[$right].TargetValue)) {
                        $pairCount++
                    }
                }
            }
            if ($IncludeVaryingDetails) {
                $first = $items[0]
                $publicSamples = New-Object Collections.Generic.List[object]
                foreach ($item in @($items | Sort-Object ArtifactId)) {
                    [void]$publicSamples.Add([ordered]@{
                            artifactId = [string]$item.ArtifactId
                            artifactRole = [string]$item.ArtifactRole
                            littleEndianValue = [int]$item.TargetValue
                        })
                }
                [void]$details.Add([ordered]@{
                        family = $Family
                        owner = [string]$first.Owner
                        sourcePath = [string]$first.SourcePath
                        recordLength = [int]$first.RecordLength
                        targetRelativeOffset =
                            [int]$first.TargetRelativeOffset
                        exactOtherBytesSha256 =
                            [string]$first.ContextSha256
                        samples = $publicSamples.ToArray()
                    })
            }
        }
    }
    $serializedText = [string]::Join("`n", $serialized.ToArray())
    $summary = [ordered]@{
        family = $Family
        sampleCount = [int](@($Samples | Where-Object {
                    $_.Family -ceq $Family
                }).Count)
        allGroupCount = [int]$groups.Count
        multiArtifactGroupCount = [int]$multiGroups
        multiArtifactSampleCount = [int]$multiSamples
        varyingGroupCount = [int]$varyingGroups
        varyingSampleCount = [int]$varyingSamples
        varyingOwnerCount = [int]$varyingOwners.Count
        counterexamplePairCount = [int]$pairCount
        exactGroupSampleTableRows = [int]$serialized.Count
        exactGroupSampleTableSha256 = Get-AsciiSha256 `
            -Text $serializedText -TextOwner "$Family group sample table"
    }
    return [pscustomobject]@{
        Summary = $summary
        Groups = $groups
        VaryingDetails = $details.ToArray()
    }
}

function Get-LayerAnalysis {
    param(
        [Parameter(Mandatory = $true)][string]$Layer,
        [Parameter(Mandatory = $true)][object[]]$Artifacts,
        [switch]$IncludeVaryingDetails
    )
    $samples = New-Object Collections.Generic.List[object]
    $topologies = @{}
    $masked = @{}
    $recordCount = 0
    $markerCount = 0
    foreach ($artifact in $Artifacts) {
        $recordCount += [int]$artifact.RecordCount
        $markerCount += [int]$artifact.MarkerCount
        $topologies[[string]$artifact.TopologySha256] = $true
        $masked[[string]$artifact.BothSlotMaskedSha256] = $true
        foreach ($sample in $artifact.Samples) { [void]$samples.Add($sample) }
    }
    $tail = Get-FamilyGrouping -Samples $samples.ToArray() `
        -Family 'tail' -IncludeVaryingDetails:$IncludeVaryingDetails
    $marker = Get-FamilyGrouping -Samples $samples.ToArray() `
        -Family 'marker' -IncludeVaryingDetails:$IncludeVaryingDetails
    return [pscustomobject]@{
        Summary = [ordered]@{
            layer = $Layer
            uniqueArtifactCount = [int]$Artifacts.Count
            recordSampleCount = [int]$recordCount
            markerSampleCount = [int]$markerCount
            inventoryTopologyCount = [int]$topologies.Count
            wholeArtifactBothSlotMaskedUniqueCount = [int]$masked.Count
            tail = $tail.Summary
            marker = $marker.Summary
        }
        Samples = $samples.ToArray()
        Tail = $tail
        Marker = $marker
    }
}

function Assert-LayerGolden {
    param(
        [Parameter(Mandatory = $true)]$LayerAnalysis,
        [Parameter(Mandatory = $true)][int[]]$Golden
    )
    $summary = $LayerAnalysis.Summary
    [int[]]$actual = @(
        $summary.uniqueArtifactCount,
        $summary.recordSampleCount,
        $summary.markerSampleCount,
        $summary.inventoryTopologyCount,
        $summary.wholeArtifactBothSlotMaskedUniqueCount,
        $summary.tail.allGroupCount,
        $summary.tail.multiArtifactGroupCount,
        $summary.tail.multiArtifactSampleCount,
        $summary.tail.varyingGroupCount,
        $summary.tail.varyingSampleCount,
        $summary.tail.varyingOwnerCount,
        $summary.tail.counterexamplePairCount,
        $summary.marker.allGroupCount,
        $summary.marker.multiArtifactGroupCount,
        $summary.marker.multiArtifactSampleCount,
        $summary.marker.varyingGroupCount,
        $summary.marker.varyingSampleCount,
        $summary.marker.varyingOwnerCount,
        $summary.marker.counterexamplePairCount)
    if ((ConvertTo-DeterministicJson -Value $actual) -cne
        (ConvertTo-DeterministicJson -Value $Golden)) {
        Throw-CorpusUnexpected (
            "$($summary.layer) golden tuple differs: " +
            (ConvertTo-DeterministicJson -Value $actual))
    }
}

function Throw-CorpusUnexpected {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw "UNEXPECTED: $Message"
}

function Get-RecordMap {
    param([Parameter(Mandatory = $true)]$Artifact)
    $map = @{}
    for ($index = 0;
         $index -lt $Artifact.Inventory.Owners.Length; $index++) {
        $key = [string]$Artifact.Inventory.Owners[$index] +
            [char]0x1F + [string]$Artifact.Inventory.SourcePaths[$index]
        if ($map.ContainsKey($key)) {
            Throw-CorpusBlocker 'transition record map contains a duplicate key.'
        }
        $map[$key] = [int]$index
    }
    return $map
}

function Get-MainlineTransitionAnalysis {
    param([Parameter(Mandatory = $true)][object[]]$Occurrences)
    if ($Occurrences.Count -ne 22) {
        Throw-CorpusBlocker 'mainline transition input is not 22 occurrences.'
    }
    Initialize-CorpusBinaryType
    $common = 0
    $rawIdentical = 0
    $candidateOnly = 0
    $outside = 0
    $ownersAdded = 0
    $ownersRemoved = 0
    $tailOnly = 0
    $markerOnly = 0
    $bothFamilies = 0
    for ($edge = 0; $edge -lt 21; $edge++) {
        $left = $Occurrences[$edge].Artifact
        $right = $Occurrences[$edge + 1].Artifact
        $leftMap = Get-RecordMap -Artifact $left
        $rightMap = Get-RecordMap -Artifact $right
        foreach ($key in $leftMap.Keys) {
            if (-not $rightMap.ContainsKey($key)) { $ownersRemoved++ }
        }
        foreach ($key in $rightMap.Keys) {
            if (-not $leftMap.ContainsKey($key)) { $ownersAdded++ }
        }
        foreach ($key in $leftMap.Keys) {
            if (-not $rightMap.ContainsKey($key)) { continue }
            $common++
            $result =
                [CodexLasalHistoricalCorpusBinaryV1]::CompareRecord(
                    $left.Bytes, $left.Inventory, [int]$leftMap[$key],
                    $right.Bytes, $right.Inventory, [int]$rightMap[$key])
            if ([bool]$result.RawExact) {
                $rawIdentical++
                if ([bool]$result.CandidateOnlyChanged -or
                    [bool]$result.OutsideCandidateChanged -or
                    [bool]$result.TailSingleTargetCounterexample -or
                    ([int]$result.MarkerSingleTargetCounterexamples -ne 0) -or
                    [bool]$result.BothCandidateWordsChanged) {
                    Throw-CorpusUnexpected (
                        'raw-identical transition has a changed classification.')
                }
            }
            elseif ([bool]$result.CandidateOnlyChanged) {
                $candidateOnly++
                if ([bool]$result.OutsideCandidateChanged) {
                    Throw-CorpusUnexpected (
                        'candidate-only and outside classifications overlap.')
                }
                if ([bool]$result.TailSingleTargetCounterexample -and
                    ([int]$result.MarkerSingleTargetCounterexamples -eq 0) -and
                    (-not [bool]$result.BothCandidateWordsChanged)) {
                    $tailOnly++
                }
                elseif ((-not [bool]$result.TailSingleTargetCounterexample) -and
                    ([int]$result.MarkerSingleTargetCounterexamples -eq 1) -and
                    (-not [bool]$result.BothCandidateWordsChanged)) {
                    $markerOnly++
                }
                elseif ((-not [bool]$result.TailSingleTargetCounterexample) -and
                    ([int]$result.MarkerSingleTargetCounterexamples -eq 0) -and
                    [bool]$result.BothCandidateWordsChanged) {
                    $bothFamilies++
                }
                else {
                    Throw-CorpusUnexpected (
                        'candidate-only transition family partition differs.')
                }
            }
            elseif ([bool]$result.OutsideCandidateChanged) {
                $outside++
            }
            else {
                Throw-CorpusUnexpected (
                    'common transition record has no primary classification.')
            }
        }
    }
    [int[]]$actual = @(
        $common, $rawIdentical, $candidateOnly, $outside,
        $ownersAdded, $ownersRemoved, $tailOnly, $markerOnly, $bothFamilies)
    [int[]]$golden = @(2378, 1155, 538, 685, 18, 2, 55, 97, 386)
    if (($common -ne ($rawIdentical + $candidateOnly + $outside)) -or
        ($candidateOnly -ne ($tailOnly + $markerOnly + $bothFamilies)) -or
        ((ConvertTo-DeterministicJson -Value $actual) -cne
            (ConvertTo-DeterministicJson -Value $golden))) {
        Throw-CorpusUnexpected (
            'mainline adjacent transition golden tuple differs: ' +
            (ConvertTo-DeterministicJson -Value $actual))
    }
    return [ordered]@{
        ordering = 'OLDEST_TO_NEWEST_CANONICAL_OCCURRENCES'
        occurrenceCount = 22
        adjacentTransitionCount = 21
        recordMatchKey = 'owner+sourcePath'
        commonOwnerRecordPairs = [int]$common
        rawIdentical = [int]$rawIdentical
        candidateOnlyChanged = [int]$candidateOnly
        outsideTargetChanged = [int]$outside
        ownersAdded = [int]$ownersAdded
        ownersRemoved = [int]$ownersRemoved
        candidateOnlyPartition = [ordered]@{
            tailOnly = [int]$tailOnly
            markerOnly = [int]$markerOnly
            bothFamilies = [int]$bothFamilies
        }
        primaryPartitionExact = $true
        candidateOnlyPartitionExact = $true
        semanticEquivalentRebuildCorpus = $false
    }
}

function Get-FirstCounterexamplePair {
    param(
        [Parameter(Mandatory = $true)]$Grouping,
        [Parameter(Mandatory = $true)][string]$Family
    )
    [string[]]$keys = Get-OrdinalSortedStrings `
        -Values @($Grouping.Groups.Keys)
    foreach ($key in $keys) {
        $items = @($Grouping.Groups[$key].ToArray() | Sort-Object ArtifactId)
        for ($left = 0; $left -lt $items.Count; $left++) {
            for ($right = $left + 1; $right -lt $items.Count; $right++) {
                if (($items[$left].ArtifactId -cne
                        $items[$right].ArtifactId) -and
                    ([int]$items[$left].TargetValue -ne
                        [int]$items[$right].TargetValue)) {
                    if (-not
                        [CodexLasalHistoricalCorpusBinaryV1]::RecordsEqualWordZeroed(
                            $items[$left].Bytes,
                            [int]$items[$left].RecordStart,
                            [int]$items[$left].RecordLength,
                            [int]$items[$left].TargetOffset,
                            $items[$right].Bytes,
                            [int]$items[$right].RecordStart,
                            [int]$items[$right].RecordLength,
                            [int]$items[$right].TargetOffset)) {
                        Throw-CorpusUnexpected (
                            "$Family selected witness is not exact-other bytes.")
                    }
                    return @($items[$left], $items[$right])
                }
            }
        }
    }
    Throw-CorpusUnexpected "$Family has no exact-other counterexample pair."
}

function Get-HypothesisValueVector {
    param([Parameter(Mandatory = $true)]$Sample)
    [uint16[]]$core =
        [CodexLasalHistoricalCorpusBinaryV1]::GetBoundedHypothesisValues(
            $Sample.Bytes, [int]$Sample.RecordStart,
            [int]$Sample.RecordLength, [int]$Sample.TargetOffset)
    [uint16[]]$metadata = @(
        [uint16]([int]$Sample.RecordLength -band 0xFFFF),
        [uint16](Get-AsciiByteSum16 -Value ([string]$Sample.Owner)),
        [uint16](([string]$Sample.Owner).Length -band 0xFFFF),
        [uint16](Get-AsciiByteSum16 -Value ([string]$Sample.SourcePath)),
        [uint16](([string]$Sample.SourcePath).Length -band 0xFFFF))
    [uint16[]]$result = @($core) + @($metadata)
    return ,$result
}

function Get-BoundedHypothesisReport {
    param([Parameter(Mandatory = $true)]$FullLayer)
    Initialize-CorpusBinaryType
    [byte[]]$checkBytes = [Text.Encoding]::ASCII.GetBytes('123456789')
    [uint16[]]$crcCheck =
        [CodexLasalHistoricalCorpusBinaryV1]::GetCrcCheckValues($checkBytes)
    [uint16[]]$crcExpected = @(
        0xBB3D, 0x4B37, 0x29B1, 0x31C3, 0x2189, 0x906E)
    if ((ConvertTo-DeterministicJson -Value @($crcCheck)) -cne
        (ConvertTo-DeterministicJson -Value @($crcExpected))) {
        Throw-CorpusUnexpected 'CRC check vector differs.'
    }
    [string[]]$names = @(
        'CRC16_ARC', 'CRC16_MODBUS', 'CRC16_CCITT_FALSE',
        'CRC16_XMODEM', 'CRC16_KERMIT', 'CRC16_X25',
        'BYTE_SUM16_MOD65536', 'LE_WORD_SUM16_MOD65536',
        'ONES_COMPLEMENT_BE_WORD_SUM16', 'FLETCHER16_MOD255',
        'FNV1A32_XOR_FOLDED16', 'SHA256_FIRST16_LE',
        'SHA256_FIRST16_BE', 'SHA256_LAST16_LE', 'SHA256_LAST16_BE',
        'RECORD_LENGTH_LOW16', 'OWNER_ASCII_BYTE_SUM16',
        'OWNER_ASCII_BYTE_LENGTH', 'SOURCE_PATH_ASCII_BYTE_SUM16',
        'SOURCE_PATH_ASCII_BYTE_LENGTH')
    $witnessByFamily = @{}
    foreach ($family in @('tail', 'marker')) {
        $grouping = if ($family -ceq 'tail') {
            $FullLayer.Tail
        }
        else { $FullLayer.Marker }
        $pair = Get-FirstCounterexamplePair `
            -Grouping $grouping -Family $family
        [uint16[]]$leftValues = Get-HypothesisValueVector -Sample $pair[0]
        [uint16[]]$rightValues = Get-HypothesisValueVector -Sample $pair[1]
        if (($leftValues.Count -ne $names.Count) -or
            ((ConvertTo-DeterministicJson -Value @($leftValues)) -cne
                (ConvertTo-DeterministicJson -Value @($rightValues)))) {
            Throw-CorpusUnexpected (
                "$family hypothesis predictions differ on exact-other input.")
        }
        $witnessByFamily[$family] = [pscustomobject]@{
            Pair = $pair
            Values = $leftValues
        }
    }
    $tests = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $names.Count; $index++) {
        $witnesses = New-Object Collections.Generic.List[object]
        foreach ($family in @('tail', 'marker')) {
            $entry = $witnessByFamily[$family]
            $pair = $entry.Pair
            [void]$witnesses.Add([ordered]@{
                    family = $family
                    owner = [string]$pair[0].Owner
                    sourcePath = [string]$pair[0].SourcePath
                    recordLength = [int]$pair[0].RecordLength
                    targetRelativeOffset =
                        [int]$pair[0].TargetRelativeOffset
                    exactOtherBytesSha256 =
                        [string]$pair[0].ContextSha256
                    predictedValue = [int]$entry.Values[$index]
                    firstArtifactId = [string]$pair[0].ArtifactId
                    firstObservedLittleEndianValue =
                        [int]$pair[0].TargetValue
                    secondArtifactId = [string]$pair[1].ArtifactId
                    secondObservedLittleEndianValue =
                        [int]$pair[1].TargetValue
                    exactTargetZeroedRecordBytesEqual = $true
                })
        }
        [void]$tests.Add([ordered]@{
                hypothesis = $names[$index]
                classification =
                    'REFUTED_BY_EXACT_OTHER_BYTES_COUNTEREXAMPLE'
                witnesses = $witnesses.ToArray()
            })
    }
    return [ordered]@{
        testedFunctionScope =
            'FIXED_STATELESS_FUNCTION_OF_TARGET_ZEROED_RECORD_INPUT_ONLY'
        targetEncoding = 'LITTLE_ENDIAN_UINT16'
        crcCheckVector = [ordered]@{
            ascii = '123456789'
            arc = 'BB3D'
            modbus = '4B37'
            ccittFalse = '29B1'
            xmodem = '31C3'
            kermit = '2189'
            x25 = '906E'
            matched = $true
        }
        algorithmDefinitions = [ordered]@{
            sums =
                'BYTE_MOD65536; LE16_WORDS_MOD65536_ODD_FINAL_LOW_BYTE'
            onesComplement =
                'BE16_WORDS_END_AROUND_CARRY_ODD_FINAL_HIGH_BYTE_THEN_INVERT'
            fletcher = 'FLETCHER16_MOD255_SUM2_HIGH_SUM1_LOW'
            fnv = 'FNV1A32_OFFSET2166136261_PRIME16777619_XOR_FOLD16'
            shaEndpoints = 'SHA256_FIRST_OR_LAST_TWO_BYTES_EXPLICIT_LE_OR_BE'
            metadataEncoding = '7_BIT_ASCII_BYTES_AND_LOW16_LENGTHS'
        }
        classification =
            'REFUTED_BY_EXACT_OTHER_BYTES_COUNTEREXAMPLE'
        boundedHypotheses = $tests.ToArray()
        claimLimit =
            'DOES_NOT_REFUTE_ARTIFACT_HASH_EXTERNAL_SEED_OR_LASAL_INTERNAL_STATE'
    }
}

function Get-PublicPinnedInput {
    param([Parameter(Mandatory = $true)]$Resolved)
    return [ordered]@{
        role = [string]$Resolved.role
        relativePath = [string]$Resolved.relativePath
        commit = [string]$Resolved.commit
        blobOid = [string]$Resolved.blobOid
        rawBytes = [long]$Resolved.rawBytes
        sha256 = [string]$Resolved.sha256
        format = [string]$Resolved.format
        matched = [bool]$Resolved.matched
    }
}

function New-SuccessReport {
    param(
        [Parameter(Mandatory = $true)]$ProducerIdentity,
        [Parameter(Mandatory = $true)]$History,
        [Parameter(Mandatory = $true)]$Exclusions,
        [Parameter(Mandatory = $true)][object[]]$AugmentedProvenance,
        [Parameter(Mandatory = $true)][object[]]$PriorProvenance,
        [Parameter(Mandatory = $true)]$HistoryLayer,
        [Parameter(Mandatory = $true)]$HistoryCLayer,
        [Parameter(Mandatory = $true)]$FullLayer,
        [Parameter(Mandatory = $true)]$Transitions,
        [Parameter(Mandatory = $true)]$Hypotheses
    )
    return [ordered]@{
        schema = $Schema
        tool = [ordered]@{
            owner = $Owner
            supportedProductionInvocation = 'pwsh -File'
            producer = $ProducerIdentity
            outputPublicationTrustBoundary = 'NON_ADVERSARIAL_WORKSPACE'
            handleRelativeCreationUsed = $false
            concurrentParentReplacementResistance = $false
        }
        corpus = [ordered]@{
            analysisScope = 'pinned-historical-corpus-only'
            canonicalPath = $CanonicalClassesPath
            anchorCommit = $AnchorCommit
            selector = [ordered]@{
                command =
                    'git rev-list --first-parent --reverse ANCHOR -- PATH'
                anchor = $AnchorCommit
                path = $CanonicalClassesPath
                firstParent = $true
                oldestToNewest = $true
                follow = $false
                allRefs = $false
            }
            frozenOccurrenceTable = [ordered]@{
                serialization =
                    'ordinal|commit|P|blob|rawBytes|UPPER_SHA256; ' +
                    'P=canonicalPath'
                encoding = 'ASCII'
                separator = 'LF'
                finalLf = $false
                rawBytes = [long]$History.TableRawBytes
                sha256 = [string]$History.TableSha256
                occurrenceCount = 22
                uniqueArtifactCount = 20
                rows = $History.Table
            }
            duplicateOccurrences = @(
                [ordered]@{
                    blobOid =
                        'c3520b3de9feec5177c153f1b85436e85cd1e092'
                    ordinals = @(16, 18)
                },
                [ordered]@{
                    blobOid =
                        'b26fae6a3702997b92045596a3b709c5a15fb7e3'
                    ordinals = @(17, 19)
                })
            exclusions = $Exclusions
            augmentedObservations = $AugmentedProvenance
            priorPinnedDiagnosticProvenance = $PriorProvenance
            priorPolicyDocsBaseline = [ordered]@{
                commit = 'be74ce3ecacf33337a1fa987b01f1ea45418ce21'
                role = 'DECISION_BOUNDARY_NOT_MATHEMATICAL_CORPUS_INPUT'
            }
        }
        parser = [ordered]@{
            implementation = 'SELF_CONTAINED_COMPILED_AA03_INVENTORY_V1'
            signature = 'SigmatekLasal2Binary\u0000'
            signatureCount = 1
            sourceBoundary = 'LE24_PATH_LENGTH_THEN_AA'
            trueHeader = 'AA03_LE24_OWNER_LENGTH_AA_OWNER'
            acceptedOwnerCountInclusive = [ordered]@{
                minimum = 104
                maximum = 120
            }
            trueHeaderCount = 'OWNER_COUNT_MINUS_ONE'
            duplicateOwnerOrPathPermitted = $false
            markerHex = $MarkerHex
            markerFollowerWidthBytes = 2
            tailTargetRelativeToRecordEnd = -48
        }
        groupingContract = [ordered]@{
            targetContextKey = @(
                'family', 'owner', 'sourcePath', 'recordLength',
                'targetRelativeOffset',
                'SHA256_RECORD_WITH_EXACTLY_THIS_TARGET_WORD_ZEROED')
            exactOtherBytesComparedAfterHashGrouping = $true
            targetEncoding = 'LITTLE_ENDIAN_UINT16'
            otherFamilyMasked = $false
            otherMarkerTargetsMasked = $false
            revisionOccurrencesUsedAsIndependentSamples = $false
            artifactIdentityDeduplication = 'GIT_BLOB_OID_OR_RECONSTRUCTED_BLOB_OID'
            topologyIdentity = 'ORDERED_OWNER_AND_SOURCE_PATH_ONLY'
            wholeArtifactBothSlotMasking =
                'SEPARATE_DIAGNOSTIC_IDENTITY_NOT_GROUP_CONTEXT'
            counterexample =
                'SAME_EXACT_CONTEXT_AT_LEAST_TWO_ARTIFACTS_DIFFERENT_LE16'
            counterexamplePairs =
                'UNORDERED_DISTINCT_ARTIFACT_PAIRS_WITH_DIFFERENT_LE16'
        }
        analysis = [ordered]@{
            layers = @(
                $HistoryLayer.Summary,
                $HistoryCLayer.Summary,
                $FullLayer.Summary)
            fullLayerVaryingGroups = [ordered]@{
                tail = $FullLayer.Tail.VaryingDetails
                marker = $FullLayer.Marker.VaryingDetails
            }
            mainlineAdjacentTransitions = $Transitions
            hypotheses = $Hypotheses
            fieldMeaning =
                'UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT'
        }
        externalInputLimits = [ordered]@{
            allGeneratorInputsEquivalent = $false
            generatorTimestamp = 'UNTESTABLE_NOT_CAPTURED'
            processSession = 'UNTESTABLE_NOT_CAPTURED'
            filesystemTimestamp = 'UNTESTABLE_NOT_CAPTURED'
            lasalInternalState = 'UNPROVEN_EXTERNAL_INPUT'
            generatorTimestampRole = 'UNPROVEN_EXTERNAL_INPUT'
            processSessionRole = 'UNPROVEN_EXTERNAL_INPUT'
            filesystemTimestampRole = 'UNPROVEN_EXTERNAL_INPUT'
            gitAuthorTimeUsedAsGeneratorProxy = $false
            gitCommitTimeUsedAsGeneratorProxy = $false
        }
        decision = [ordered]@{
            disposition =
                'CONFIRMED_PINNED_HISTORICAL_SLOT_COUNTEREXAMPLES_REVIEW_ONLY'
            exitCode = 2
            toolCompleted = $true
            evidenceContractSatisfied = $true
            analysisScope = 'pinned-historical-corpus-only'
            productionApproved = $false
            semanticEquivalenceProven = $false
            rebaselinePermitted = $false
            downloadPermitted = $false
            runtimeQualificationPermitted = $false
            futureArtifactAcceptancePermitted = $false
            normalizationUsedForDecision = $false
            requiresReviewedTransition = $true
            diagnosticTargetWordMaskingUsed = $true
            targetWordMaskingUsedForAcceptance = $false
        }
    }
}

function Invoke-PinnedCorpusAnalysis {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)]$ProducerIdentity
    )
    $history = Get-CanonicalHistoryCorpus -Root $Root
    $exclusions = Get-ExclusionEvidence -Root $Root
    $resolved = @{}
    $augmentedPublic = New-Object Collections.Generic.List[object]
    foreach ($definition in $PinnedAugmentedInputs) {
        $item = Resolve-PinnedInput -Root $Root -Definition $definition
        $resolved[[string]$item.role] = $item
        [void]$augmentedPublic.Add((Get-PublicPinnedInput -Resolved $item))
    }
    $priorPublic = New-Object Collections.Generic.List[object]
    foreach ($definition in $PriorDiagnosticPins) {
        $item = Resolve-PinnedInput -Root $Root -Definition $definition
        [void]$priorPublic.Add((Get-PublicPinnedInput -Resolved $item))
    }
    $patchManifest = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['known6ePatchManifest'].bytes `
        -JsonOwner 'known 6E patch manifest'
    $knownOracle = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['known6eComparisonOracle'].bytes `
        -JsonOwner 'known 6E comparison oracle' `
        -RequireComparatorCanonical
    Assert-PatchContract `
        -PatchBytes $resolved['known6eBinaryPatch'].bytes `
        -Manifest $patchManifest

    $anchorArtifact = $history.Occurrences[21].Artifact
    if (($anchorArtifact.ArtifactId -cne $CheckpointBlobOid) -or
        ($anchorArtifact.Sha256 -cne $CheckpointSha256)) {
        Throw-CorpusBlocker 'history anchor artifact differs before B reconstruction.'
    }
    $known = Get-OracleReconstruction `
        -Oracle $knownOracle -CheckpointArtifact $anchorArtifact.Bytes `
        -OracleOwner 'known 6E comparison oracle'
    $thirdArtifactBytes = $resolved['third990BundleSnapshot'].bytes
    $thirdArtifact = New-ArtifactObservation `
        -Role 'COMMITTED_AUGMENTED_REBUILD_OBSERVATION' `
        -ArtifactId $ThirdBlobOid -RawBytes $ThirdBytes `
        -Sha256 $ThirdSha256 -Bytes $thirdArtifactBytes
    [void]$augmentedPublic.Add([ordered]@{
            role = 'COMMITTED_AUGMENTED_REBUILD_OBSERVATION'
            commit = $ThirdCommit
            relativePath = $ThirdSnapshotPath
            blobOid = $ThirdBlobOid
            rawBytes = $ThirdBytes
            sha256 = $ThirdSha256
            captureHead = 'e2dd560fe008cbe62cd7cebe56583cd0102a7cb5'
            publicationCommitDistinctFromCaptureHead = $true
        })
    $knownArtifact = New-ArtifactObservation `
        -Role 'RECONSTRUCTED_AUGMENTED_OBSERVATION' `
        -ArtifactId $KnownRebuildBlobOid -RawBytes $KnownRebuildBytes `
        -Sha256 $KnownRebuildSha256 -Bytes $known.CandidateBytes
    [void]$augmentedPublic.Add([ordered]@{
            role = 'RECONSTRUCTED_AUGMENTED_OBSERVATION'
            method = 'IN_MEMORY_FULL_COMMITTED_ORACLE_DELTA'
            resultBlobOid = $KnownRebuildBlobOid
            rawBytes = $KnownRebuildBytes
            sha256 = $KnownRebuildSha256
            localGitObjectRequired = $false
            patchReapplied = $false
            mutableWorktreeRead = $false
        })

    $historyArtifacts = @($history.Artifacts)
    $historyPlusC = @($history.Artifacts) + @($thirdArtifact)
    $historyPlusCB = $historyPlusC + @($knownArtifact)
    $historyLayer = Get-LayerAnalysis `
        -Layer 'CANONICAL_HISTORY' -Artifacts $historyArtifacts
    $historyCLayer = Get-LayerAnalysis `
        -Layer 'CANONICAL_HISTORY_PLUS_C' -Artifacts $historyPlusC
    $fullLayer = Get-LayerAnalysis `
        -Layer 'CANONICAL_HISTORY_PLUS_C_PLUS_B' `
        -Artifacts $historyPlusCB -IncludeVaryingDetails
    Assert-LayerGolden -LayerAnalysis $historyLayer -Golden @(
        20,2261,740,9,20,1155,430,1536,68,176,28,149,
        578,91,253,86,242,33,288)
    Assert-LayerGolden -LayerAnalysis $historyCLayer -Golden @(
        21,2381,777,9,20,1171,445,1655,79,207,31,183,
        600,98,275,90,258,34,319)
    Assert-LayerGolden -LayerAnalysis $fullLayer -Golden @(
        22,2501,814,9,20,1192,456,1765,87,227,31,202,
        618,102,298,95,282,34,369)
    $transitions = Get-MainlineTransitionAnalysis `
        -Occurrences $history.Occurrences
    $hypotheses = Get-BoundedHypothesisReport -FullLayer $fullLayer
    return New-SuccessReport `
        -ProducerIdentity $ProducerIdentity -History $history `
        -Exclusions $exclusions `
        -AugmentedProvenance $augmentedPublic.ToArray() `
        -PriorProvenance $priorPublic.ToArray() `
        -HistoryLayer $historyLayer -HistoryCLayer $historyCLayer `
        -FullLayer $fullLayer -Transitions $transitions `
        -Hypotheses $hypotheses
}

function Assert-SelfTestTrue {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if (-not $Condition) { throw "SELFTEST: $Message" }
}

function Assert-SelfTestThrows {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string]$Message,
        [string]$ExpectedText
    )
    $threw = $false
    try { [void](& $Action) }
    catch {
        $threw = $true
        if ((-not [string]::IsNullOrWhiteSpace($ExpectedText)) -and
            ($_.Exception.Message.IndexOf(
                    $ExpectedText, [StringComparison]::Ordinal) -lt 0)) {
            throw (
                "SELFTEST: $Message threw unexpected text: " +
                $_.Exception.Message)
        }
    }
    if (-not $threw) { throw "SELFTEST: $Message did not throw." }
}

function Get-JsonSelfTestFixture {
    $nonAscii = [string]([char]0xD55C) + [string]([char]0xAE00)
    return [ordered]@{
        schema = 'LasalClassesHistoricalSlotCorpusJsonSelfTest/v1'
        singleton = @([ordered]@{
                ordinal = 1
                text = $nonAscii
                symbols = "<>&'"
            })
        empty = @()
        nullValue = $null
        boolean = $true
        reviewExitCode = 2
    }
}

function Get-PowerShellHosts {
    $ps5 = Get-Command powershell.exe -ErrorAction SilentlyContinue
    $ps7 = Get-Command pwsh.exe -ErrorAction SilentlyContinue
    if (($null -eq $ps5) -or ($null -eq $ps7)) {
        throw 'SELFTEST: both powershell.exe and pwsh.exe are required.'
    }
    return [ordered]@{ ps5 = $ps5.Source; ps7 = $ps7.Source }
}

function Invoke-ScriptHost {
    param(
        [Parameter(Mandatory = $true)][string]$HostPath,
        [Parameter(Mandatory = $true)][string]$ModeArgument
    )
    if ($PSCommandPath.IndexOf('"') -ge 0) {
        throw 'SELFTEST: script path contains an unsupported quote.'
    }
    $startInfo = New-Object Diagnostics.ProcessStartInfo
    $startInfo.FileName = $HostPath
    $startInfo.Arguments =
        '-NoLogo -NoProfile -ExecutionPolicy Bypass -File "' +
        $PSCommandPath + '" ' + $ModeArgument
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $startInfo
    $stdout = New-Object IO.MemoryStream
    try {
        if (-not $process.Start()) {
            throw 'SELFTEST: child host did not start.'
        }
        $process.StandardOutput.BaseStream.CopyTo($stdout)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        return [pscustomobject]@{
            ExitCode = [int]$process.ExitCode
            StdoutBytes = $stdout.ToArray()
            Stderr = $stderr
        }
    }
    finally {
        $stdout.Dispose()
        $process.Dispose()
    }
}

function Remove-SelfTestDirectory {
    param([Parameter(Mandatory = $true)][string]$Directory)
    $fullDirectory = [IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $fullRoot = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
    if ((-not $fullDirectory.StartsWith(
                $fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)) -or
        (-not (Split-Path -Leaf $fullDirectory).StartsWith(
                'HistoricalCorpusSelfTest-', [StringComparison]::Ordinal))) {
        throw 'SELFTEST: cleanup target is unsafe.'
    }
    if ([IO.Directory]::Exists($fullDirectory)) {
        $item = Get-Item -LiteralPath $fullDirectory -Force
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw 'SELFTEST: cleanup target became a reparse point.'
        }
        [IO.Directory]::Delete($fullDirectory, $true)
    }
}

function Test-CommonSelfTestContracts {
    param([Parameter(Mandatory = $true)]$Hosts)
    $positive = 0
    $negative = 0
    $fixturePs5 = Invoke-ScriptHost `
        -HostPath $Hosts.ps5 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    $fixturePs7 = Invoke-ScriptHost `
        -HostPath $Hosts.ps7 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    Assert-SelfTestTrue -Condition (
        $fixturePs5.ExitCode -eq 0 -and $fixturePs7.ExitCode -eq 0 -and
        [string]::IsNullOrWhiteSpace($fixturePs5.Stderr) -and
        [string]::IsNullOrWhiteSpace($fixturePs7.Stderr) -and
        (Test-ByteSequencesExact `
            -Left $fixturePs5.StdoutBytes -Right $fixturePs7.StdoutBytes)) `
        -Message 'PowerShell 5/7 canonical JSON fixture bytes differ.'
    $fixture = $Utf8Strict.GetString(
        [Convert]::FromBase64String(
            $Utf8Strict.GetString($fixturePs7.StdoutBytes).Trim())) |
        ConvertFrom-Json
    Assert-SelfTestTrue -Condition (
        @($fixture.singleton).Count -eq 1 -and
        @($fixture.empty).Count -eq 0) `
        -Message 'singleton/empty JSON array shape differs.'
    $positive += 2

    [byte[]]$sourceBytes = [IO.File]::ReadAllBytes($PSCommandPath)
    $sourceText = $Utf8Strict.GetString($sourceBytes)
    $tokens = $null
    $parseErrors = $null
    [void][Management.Automation.Language.Parser]::ParseInput(
        $sourceText, [ref]$tokens, [ref]$parseErrors)
    Assert-SelfTestTrue -Condition ($parseErrors.Count -eq 0) `
        -Message 'script AST parse contains errors.'
    Assert-SelfTestTrue -Condition (
        -not [regex]::IsMatch($sourceText, '[^\x00-\x7F]')) `
        -Message 'script source is not 7-bit ASCII.'
    $positive += 2
    return [pscustomobject]@{ Positive = $positive; Negative = $negative }
}

function Invoke-PowerShell5SelfTest {
    $hosts = Get-PowerShellHosts
    $counts = Test-CommonSelfTestContracts -Hosts $hosts
    $positive = [int]$counts.Positive
    $negative = [int]$counts.Negative
    $invalidRoot = Join-Path ([IO.Path]::GetTempPath()) (
        'HistoricalCorpusMissing-' + [Guid]::NewGuid().ToString('N'))
    $production = Invoke-ScriptHost -HostPath $hosts.ps5 `
        -ModeArgument '-AnalyzePinnedCorpus'
    $productionInvalidRoot = Invoke-ScriptHost -HostPath $hosts.ps5 `
        -ModeArgument (
            '-AnalyzePinnedCorpus -RepositoryRoot "' + $invalidRoot + '"')
    $blocked = $Utf8Strict.GetString($production.StdoutBytes) |
        ConvertFrom-Json
    Assert-SelfTestTrue -Condition (
        $production.ExitCode -eq 4 -and
        $productionInvalidRoot.ExitCode -eq 4 -and
        [string]::IsNullOrWhiteSpace($production.Stderr) -and
        [string]::IsNullOrWhiteSpace($productionInvalidRoot.Stderr) -and
        (Test-ByteSequencesExact `
            -Left $production.StdoutBytes `
            -Right $productionInvalidRoot.StdoutBytes) -and
        $blocked.decision.disposition -ceq
            'BLOCKED_INVALID_OR_UNTRUSTED_INPUT' -and
        $blocked.error.message -ceq
            'production analysis requires PowerShell 7 before evidence or ' +
            'output is read; PS5 remains a canonical/self-test host only.') `
        -Message 'PowerShell 5 production early rejection differs.'
    $negative++
    if (-not $InternalHostSelfTest) {
        $core = Invoke-ScriptHost -HostPath $hosts.ps7 `
            -ModeArgument '-RunSelfTest -InternalHostSelfTest'
        $coreText = $Utf8Strict.GetString($core.StdoutBytes)
        Assert-SelfTestTrue -Condition (
            $core.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($core.Stderr) -and
            $coreText -cmatch
                '^PASS LasalClassesHistoricalSlotCorpus\.SelfTest\.Core ' +
                'Positive=[0-9]+ Negative=[0-9]+\r?\n$') `
            -Message 'PowerShell 7 delegated corpus self-test differs.'
        $positive++
    }
    [Console]::Out.WriteLine(
        'PASS LasalClassesHistoricalSlotCorpus.SelfTest.PS5 ' +
        "Positive=$positive Negative=$negative DelegatedCore=" +
        $(if ($InternalHostSelfTest) { 'SKIPPED' } else { 'PS7' }))
}

function Invoke-PowerShell7SelfTest {
    param([Parameter(Mandatory = $true)][string]$Root)
    $hosts = Get-PowerShellHosts
    $counts = Test-CommonSelfTestContracts -Hosts $hosts
    $positive = [int]$counts.Positive
    $negative = [int]$counts.Negative
    $report = Invoke-PinnedCorpusAnalysis -Root $Root `
        -ProducerIdentity (Get-SelfTestProducerIdentity)
    Assert-SelfTestTrue -Condition (
        $report.decision.disposition -ceq
            'CONFIRMED_PINNED_HISTORICAL_SLOT_COUNTEREXAMPLES_REVIEW_ONLY' -and
        [int]$report.decision.exitCode -eq 2 -and
        @($report.analysis.layers).Count -eq 3) `
        -Message 'full pinned corpus golden analysis differs.'
    $positive++

    [byte[]]$firstBytes = Read-GitBlobBytes -Root $Root `
        -BlobOid ([string]$HistoryTable[0][1]) -BlobOwner 'self-test 104-owner'
    [byte[]]$lastBytes = Read-GitBlobBytes -Root $Root `
        -BlobOid $CheckpointBlobOid -BlobOwner 'self-test 120-owner'
    $firstInventory =
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($firstBytes)
    $lastInventory =
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($lastBytes)
    Assert-SelfTestTrue -Condition (
        $firstInventory.Owners.Count -eq 104 -and
        $lastInventory.Owners.Count -eq 120) `
        -Message 'inclusive 104/120 parser endpoints differ.'
    $positive++
    [byte[]]$synthetic104 =
        [CodexLasalHistoricalCorpusBinaryV1]::BuildSyntheticArtifactForSelfTest(104)
    [byte[]]$synthetic120 =
        [CodexLasalHistoricalCorpusBinaryV1]::BuildSyntheticArtifactForSelfTest(120)
    Assert-SelfTestTrue -Condition (
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory(
            $synthetic104).Owners.Count -eq 104 -and
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory(
            $synthetic120).Owners.Count -eq 120) `
        -Message 'synthetic 104/120 parser endpoints differ.'
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory(
            [CodexLasalHistoricalCorpusBinaryV1]::BuildSyntheticArtifactForSelfTest(103))
    } -Message '103-owner rejection' -ExpectedText '104..120'
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory(
            [CodexLasalHistoricalCorpusBinaryV1]::BuildSyntheticArtifactForSelfTest(121))
    } -Message '121-owner rejection' -ExpectedText '104..120'
    [byte[]]$duplicateOwner =
        [CodexLasalHistoricalCorpusBinaryV1]::ReplaceAsciiAllForSelfTest(
            $synthetic104, 'C002', 'C001')
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($duplicateOwner)
    } -Message 'case-insensitive duplicate owner rejection' `
        -ExpectedText 'duplicate owner'
    $syntheticInventory =
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($synthetic104)
    [byte[]]$incompleteMarker = Copy-Bytes -Bytes $synthetic104
    $incompleteStart = [int]$syntheticInventory.Ends[0] - $MarkerBytes.Length
    [Array]::Copy(
        $MarkerBytes, 0, $incompleteMarker, $incompleteStart,
        $MarkerBytes.Length)
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($incompleteMarker)
    } -Message 'incomplete marker follower rejection' `
        -ExpectedText 'crosses owner record boundary'
    $positive++
    $negative += 4
    [byte[]]$badSignature = Copy-Bytes -Bytes $firstBytes
    $badSignature[0] = $badSignature[0] -bxor 1
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($badSignature)
    } -Message 'bad signature rejection' -ExpectedText 'signature'
    [byte[]]$badHeader = Copy-Bytes -Bytes $lastBytes
    $badHeader[[int]$lastInventory.Starts[1]] = 0xAB
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($badHeader)
    } -Message 'missing true header rejection' -ExpectedText 'count differs'
    [byte[]]$badPathLength = Copy-Bytes -Bytes $firstBytes
    $firstPath = '.\Class\' + $firstInventory.Owners[0] + '\' +
        $firstInventory.Owners[0] + '.st'
    $pathIndex = $Latin1.GetString($badPathLength).IndexOf(
        $firstPath, [StringComparison]::Ordinal)
    if ($pathIndex -lt 4) { throw 'SELFTEST: first source path was not found.' }
    $badPathLength[$pathIndex - 4] =
        $badPathLength[$pathIndex - 4] -bxor 1
    Assert-SelfTestThrows -Action {
        [CodexLasalHistoricalCorpusBinaryV1]::ParseInventory($badPathLength)
    } -Message 'wrong source LE24 rejection' -ExpectedText 'boundary differs'
    $negative += 3

    $oracleDefinition = @($PinnedAugmentedInputs | Where-Object {
            $_.role -ceq 'known6eComparisonOracle'
        })[0]
    $oracleInput = Resolve-PinnedInput `
        -Root $Root -Definition $oracleDefinition
    $oracleBase = ConvertFrom-StrictJsonBytes `
        -Bytes $oracleInput.bytes -JsonOwner 'self-test known 6E oracle' `
        -RequireComparatorCanonical
    $oracleJson = ConvertTo-DeterministicJson -Value $oracleBase
    $truncatedOracle = $oracleJson | ConvertFrom-Json
    $truncatedOracle.diffRuns[0].checkpointPreview.truncated = $true
    Assert-SelfTestThrows -Action {
        Get-OracleReconstruction -Oracle $truncatedOracle `
            -CheckpointArtifact $lastBytes `
            -OracleOwner 'self-test truncated oracle'
    } -Message 'truncated oracle rejection' -ExpectedText 'shape differs'
    $overlapOracle = $oracleJson | ConvertFrom-Json
    $overlapOracle.diffRuns[1].checkpointStart =
        [int]$overlapOracle.diffRuns[0].checkpointStart
    $overlapOracle.diffRuns[1].candidateStart =
        [int]$overlapOracle.diffRuns[0].checkpointStart
    Assert-SelfTestThrows -Action {
        Get-OracleReconstruction -Oracle $overlapOracle `
            -CheckpointArtifact $lastBytes `
            -OracleOwner 'self-test overlapping oracle'
    } -Message 'overlapping oracle rejection' -ExpectedText 'shape differs'
    $mutatedOracle = $oracleJson | ConvertFrom-Json
    $oldHex = [string]$mutatedOracle.diffRuns[0].candidatePreview.hex
    $newFirst = if ($oldHex[0] -ceq '0') { '1' } else { '0' }
    $mutatedOracle.diffRuns[0].candidatePreview.hex =
        $newFirst + $oldHex.Substring(1)
    Assert-SelfTestThrows -Action {
        Get-OracleReconstruction -Oracle $mutatedOracle `
            -CheckpointArtifact $lastBytes `
            -OracleOwner 'self-test mutated oracle'
    } -Message 'mutated oracle rejection'
    $negative += 3

    [byte[]]$contextA = @(1, 2, 3, 4, 5, 6)
    [byte[]]$contextB = @(1, 2, 9, 9, 5, 6)
    $hashA =
        [CodexLasalHistoricalCorpusBinaryV1]::Sha256WordZeroed(
            $contextA, 0, 6, 0)
    $hashB =
        [CodexLasalHistoricalCorpusBinaryV1]::Sha256WordZeroed(
            $contextB, 0, 6, 0)
    Assert-SelfTestTrue -Condition ($hashA -cne $hashB) `
        -Message 'target-specific masking contaminated another target.'
    $positive++
    $fixturePairs = 0
    [int[]]$fixtureValues = @(7, 7, 9)
    for ($left = 0; $left -lt 3; $left++) {
        for ($right = $left + 1; $right -lt 3; $right++) {
            if ($fixtureValues[$left] -ne $fixtureValues[$right]) {
                $fixturePairs++
            }
        }
    }
    Assert-SelfTestTrue -Condition ($fixturePairs -eq 2) `
        -Message 'A,A,B unordered differing-value pair count differs.'
    $positive++

    $reversed = @($HistoryTable)
    [Array]::Reverse($reversed)
    $reversedRows = New-Object Collections.Generic.List[string]
    for ($index = 0; $index -lt $reversed.Count; $index++) {
        $row = $reversed[$index]
        [void]$reversedRows.Add(
            "$(($index + 1))|$($row[0])|$CanonicalClassesPath|" +
            "$($row[1])|$($row[2])|$($row[3])")
    }
    $reversedSha = Get-AsciiSha256 `
        -Text ([string]::Join("`n", $reversedRows.ToArray())) `
        -TextOwner 'reversed history self-test table'
    Assert-SelfTestTrue -Condition (
        $reversedSha -cne $CanonicalTableSha256 -and
        @($HistoryTable | ForEach-Object { $_[1] } |
            Sort-Object -Unique).Count -eq 20) `
        -Message 'order negative or duplicate-collapse fixture differs.'
    $negative += 2

    $tempDirectory = Join-Path $PSScriptRoot (
        'HistoricalCorpusSelfTest-' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($tempDirectory)
    try {
        $descriptor = Resolve-CreateNewOutputPath `
            -RequestedPath $ReportFileName `
            -AllowedRoot $tempDirectory -ExactParent $tempDirectory
        [byte[]]$payload = Get-DeterministicJsonBytes `
            -Json (ConvertTo-DeterministicJson `
                -Value (Get-JsonSelfTestFixture))
        Write-CreateNewBytes -Descriptor $descriptor -Bytes $payload
        Assert-SelfTestTrue -Condition (
            [IO.File]::Exists($descriptor.FullPath) -and
            (Test-ByteSequencesExact `
                -Left ([IO.File]::ReadAllBytes($descriptor.FullPath)) `
                -Right $payload)) `
            -Message 'CreateNew positive/read-back differs.'
        Assert-SelfTestThrows -Action {
            Resolve-CreateNewOutputPath -RequestedPath $ReportFileName `
                -AllowedRoot $tempDirectory -ExactParent $tempDirectory
        } -Message 'CreateNew overwrite rejection' `
            -ExpectedText 'already exists'
        [byte[]]$sentinel = [IO.File]::ReadAllBytes($descriptor.FullPath)
        Assert-SelfTestTrue -Condition (
            Test-ByteSequencesExact -Left $sentinel -Right $payload) `
            -Message 'existing output sentinel changed.'
        [IO.File]::Delete($descriptor.FullPath)
        Assert-SelfTestThrows -Action {
            Resolve-CreateNewOutputPath `
                -RequestedPath ('nested\' + $ReportFileName) `
                -AllowedRoot $tempDirectory -ExactParent $tempDirectory
        } -Message 'nested output rejection' -ExpectedText 'direct child'
        Assert-SelfTestThrows -Action {
            Resolve-CreateNewOutputPath `
                -RequestedPath ($ReportFileName + ':evil') `
                -AllowedRoot $tempDirectory -ExactParent $tempDirectory
        } -Message 'ADS output rejection' -ExpectedText 'alternate data stream'
        Assert-SelfTestThrows -Action {
            Resolve-CreateNewOutputPath -RequestedPath 'CON' `
                -AllowedRoot $tempDirectory -ExactParent $tempDirectory
        } -Message 'device output rejection' -ExpectedText 'reserved'
        $swapParent = Join-Path $tempDirectory 'swap-parent'
        $junctionTarget = Join-Path $tempDirectory 'junction-target'
        [void][IO.Directory]::CreateDirectory($swapParent)
        [void][IO.Directory]::CreateDirectory($junctionTarget)
        $swapDescriptor = Resolve-CreateNewOutputPath `
            -RequestedPath $ReportFileName `
            -AllowedRoot $tempDirectory -ExactParent $swapParent
        try {
            Assert-SelfTestThrows -Action {
                Write-CreateNewBytes -Descriptor $swapDescriptor `
                    -Bytes $payload -BeforeCreateSelfTestHook {
                        [IO.Directory]::Delete($swapParent, $false)
                        [void](New-Item -ItemType Junction `
                            -Path $swapParent -Target $junctionTarget)
                    }
            } -Message 'junction-swap output rejection' `
                -ExpectedText 'reparse-point'
            Assert-SelfTestTrue -Condition (
                -not [IO.File]::Exists(
                    (Join-Path $junctionTarget $ReportFileName))) `
                -Message 'junction-swap wrote through the replacement parent.'
        }
        finally {
            $swapItem = Get-Item -LiteralPath $swapParent -Force `
                -ErrorAction SilentlyContinue
            if (($null -ne $swapItem) -and
                (($swapItem.Attributes -band
                        [IO.FileAttributes]::ReparsePoint) -ne 0)) {
                [IO.Directory]::Delete($swapParent, $false)
            }
        }
        $positive += 2
        $negative += 5
    }
    finally { Remove-SelfTestDirectory -Directory $tempDirectory }

    $toolStatus = Invoke-GitText -Root $Root `
        -Arguments @(
            'status', '--porcelain=v1', '--untracked-files=all',
            '--', $ToolRelativePath) `
        -Operation 'self-test producer status'
    if (-not [string]::IsNullOrEmpty($toolStatus)) {
        $canonicalReportPath = Join-Path $PSScriptRoot $ReportFileName
        $reportExistedBefore = [IO.File]::Exists($canonicalReportPath)
        $reportHashBefore = if ($reportExistedBefore) {
            Get-BytesSha256 -Bytes (
                [IO.File]::ReadAllBytes($canonicalReportPath))
        }
        else { $null }
        $production = Invoke-ScriptHost -HostPath $hosts.ps7 `
            -ModeArgument (
                '-AnalyzePinnedCorpus -CreateNew -OutputPath ' +
                $ReportFileName)
        $blocked = $Utf8Strict.GetString($production.StdoutBytes) |
            ConvertFrom-Json
        $reportExistsAfter = [IO.File]::Exists($canonicalReportPath)
        $reportStateUnchanged =
            $reportExistsAfter -eq $reportExistedBefore
        if ($reportExistedBefore -and $reportExistsAfter) {
            $reportStateUnchanged = $reportStateUnchanged -and
                ((Get-BytesSha256 -Bytes (
                        [IO.File]::ReadAllBytes($canonicalReportPath))) -ceq
                    $reportHashBefore)
        }
        Assert-SelfTestTrue -Condition (
            $production.ExitCode -eq 4 -and
            $blocked.error.message -ceq
                'producer script is not tracked and scoped HEAD-clean.' -and
            $reportStateUnchanged) `
            -Message 'precommit producer fail-closed path differs.'
        $negative++
    }

    if (-not $InternalHostSelfTest) {
        $ps5 = Invoke-ScriptHost -HostPath $hosts.ps5 `
            -ModeArgument '-RunSelfTest -InternalHostSelfTest'
        $ps5Text = $Utf8Strict.GetString($ps5.StdoutBytes)
        Assert-SelfTestTrue -Condition (
            $ps5.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($ps5.Stderr) -and
            $ps5Text -cmatch
                '^PASS LasalClassesHistoricalSlotCorpus\.SelfTest\.PS5 ' +
                'Positive=[0-9]+ Negative=[0-9]+ DelegatedCore=SKIPPED\r?\n$') `
            -Message 'PowerShell 5 delegated host self-test differs.'
        $positive++
    }
    [Console]::Out.WriteLine(
        'PASS LasalClassesHistoricalSlotCorpus.SelfTest.Core ' +
        "Positive=$positive Negative=$negative")
}

function Invoke-SelfTest {
    param([Parameter(Mandatory = $true)][string]$Root)
    if (($PSVersionTable.PSEdition -ceq 'Desktop') -or
        ($PSVersionTable.PSVersion.Major -lt 7)) {
        Invoke-PowerShell5SelfTest
        return
    }
    Invoke-PowerShell7SelfTest -Root $Root
}

function Get-ExactScalarProcessExitCode {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$ValueOwner
    )
    if (($Value -is [Array]) -or ($Value -isnot [int])) {
        Throw-CorpusBlocker (
            "$ValueOwner must be exactly one System.Int32 process exit code.")
    }
    if ([int]$Value -notin @(0, 2, 3, 4)) {
        Throw-CorpusBlocker (
            "$ValueOwner is outside the exact 0/2/3/4 contract.")
    }
    return [int]$Value
}

function Assert-InvocationContract {
    param([Parameter(Mandatory = $true)][Collections.IDictionary]$Bound)
    if ($CreateNew -and [string]::IsNullOrWhiteSpace($OutputPath)) {
        Throw-CorpusBlocker '-CreateNew requires -OutputPath.'
    }
    if ((-not $CreateNew) -and $Bound.ContainsKey('OutputPath')) {
        Throw-CorpusBlocker '-OutputPath requires -CreateNew.'
    }
}

function New-StopDecision {
    param(
        [Parameter(Mandatory = $true)][string]$Disposition,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][bool]$ToolCompleted
    )
    return [ordered]@{
        disposition = $Disposition
        exitCode = $ExitCode
        toolCompleted = $ToolCompleted
        evidenceContractSatisfied = $false
        analysisScope = 'pinned-historical-corpus-only'
        productionApproved = $false
        semanticEquivalenceProven = $false
        rebaselinePermitted = $false
        downloadPermitted = $false
        runtimeQualificationPermitted = $false
        futureArtifactAcceptancePermitted = $false
        normalizationUsedForDecision = $false
        requiresReviewedTransition = $true
        diagnosticTargetWordMaskingUsed = $true
        targetWordMaskingUsedForAcceptance = $false
    }
}

function New-BlockedReport {
    param([Parameter(Mandatory = $true)][string]$Message)
    $normalized = $Message
    if ($normalized.StartsWith('BLOCKED: ', [StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(9)
    }
    return [ordered]@{
        schema = $Schema
        tool = [ordered]@{
            owner = $Owner
            supportedProductionInvocation = 'pwsh -File'
            outputPublicationTrustBoundary = 'NON_ADVERSARIAL_WORKSPACE'
            handleRelativeCreationUsed = $false
            concurrentParentReplacementResistance = $false
        }
        decision = New-StopDecision `
            -Disposition 'BLOCKED_INVALID_OR_UNTRUSTED_INPUT' `
            -ExitCode 4 -ToolCompleted $false
        error = [ordered]@{ message = $normalized }
    }
}

function New-UnexpectedReport {
    param([Parameter(Mandatory = $true)][string]$Message)
    $normalized = $Message
    if ($normalized.StartsWith('UNEXPECTED: ', [StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(12)
    }
    return [ordered]@{
        schema = $Schema
        tool = [ordered]@{
            owner = $Owner
            supportedProductionInvocation = 'pwsh -File'
            outputPublicationTrustBoundary = 'NON_ADVERSARIAL_WORKSPACE'
            handleRelativeCreationUsed = $false
            concurrentParentReplacementResistance = $false
        }
        decision = New-StopDecision `
            -Disposition 'UNEXPECTED_PINNED_HISTORICAL_CORPUS_RESULT_STOP' `
            -ExitCode 3 -ToolCompleted $true
        error = [ordered]@{ message = $normalized }
    }
}

try {
    Assert-InvocationContract -Bound $PSBoundParameters
    if ($EmitJsonSelfTestFixtureBase64) {
        $fixtureJson = ConvertTo-DeterministicJson `
            -Value (Get-JsonSelfTestFixture)
        [byte[]]$fixtureBytes = Get-DeterministicJsonBytes -Json $fixtureJson
        [Console]::Out.WriteLine(
            [Convert]::ToBase64String($fixtureBytes))
        exit 0
    }
    if ($RunSelfTest) {
        $resolvedRoot = Resolve-RepositoryContext -RequestedRoot $null
        Invoke-SelfTest -Root $resolvedRoot
        exit 0
    }
    Assert-PowerShell7Production
    $resolvedRoot = Resolve-RepositoryContext -RequestedRoot $RepositoryRoot
    $producerIdentity = Resolve-ProducerIdentity -Root $resolvedRoot
    $resolvedOutput = $null
    if ($CreateNew) {
        $resolvedOutput = Resolve-CreateNewOutputPath `
            -RequestedPath $OutputPath -AllowedRoot $resolvedRoot `
            -ExactParent $PSScriptRoot
    }
    $report = Invoke-PinnedCorpusAnalysis `
        -Root $resolvedRoot -ProducerIdentity $producerIdentity
    $json = ConvertTo-DeterministicJson -Value $report
    [byte[]]$jsonBytes = Get-DeterministicJsonBytes -Json $json
    if ($CreateNew) {
        Write-CreateNewBytes -Descriptor $resolvedOutput -Bytes $jsonBytes
    }
    Write-JsonStdout -Bytes $jsonBytes
    $finalExitCode = Get-ExactScalarProcessExitCode `
        -Value ([int]$report.decision.exitCode) `
        -ValueOwner 'production historical corpus report exit code'
    exit $finalExitCode
}
catch {
    $message = $_.Exception.Message
    $isUnexpected = $message.StartsWith(
        'UNEXPECTED: ', [StringComparison]::Ordinal)
    $stopReport = if ($isUnexpected) {
        New-UnexpectedReport -Message $message
    }
    else { New-BlockedReport -Message $message }
    $stopJson = ConvertTo-DeterministicJson -Value $stopReport
    [byte[]]$stopBytes = Get-DeterministicJsonBytes -Json $stopJson
    Write-JsonStdout -Bytes $stopBytes
    $stopExitCode = Get-ExactScalarProcessExitCode `
        -Value ([int]$stopReport.decision.exitCode) `
        -ValueOwner 'stopped historical corpus report exit code'
    exit $stopExitCode
}

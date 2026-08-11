[CmdletBinding(DefaultParameterSetName = 'Analyze')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Analyze')]
    [switch]$AnalyzePinnedTriad,
    [Parameter(ParameterSetName = 'Analyze')]
    [string]$RepositoryRoot,
    [Parameter(ParameterSetName = 'Analyze')]
    [string]$OutputPath,
    [Parameter(ParameterSetName = 'Analyze')]
    [switch]$CreateNew,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,
    [Parameter(Mandatory = $true, ParameterSetName = 'Fixture', DontShow = $true)]
    [switch]$EmitJsonSelfTestFixtureBase64
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Owner = 'LASAL.ClassesVolatilityTriadComparator'
$Schema = 'LasalClassesVolatilityTriadEvidence/v1'
$CanonicalClassesPath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
$CheckpointCommit = '55435791f6e91c9dcb4e06dcd25a11d77b382da7'
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
$ThirdCaptureHead = 'e2dd560fe008cbe62cd7cebe56583cd0102a7cb5'
$ThirdBlobOid = '726f5ed4498592dba13e358c0d7320d2e5d02a1a'
$ThirdBytes = 8549773L
$ThirdSha256 =
    '99014DD95A5580381D2D3A46C03D98EB38B6B7A81DBC78E302CBBA22FEFCFCFD'
$EvidenceRelativeDirectory =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876'
$ToolRelativePath = $EvidenceRelativeDirectory +
    '/Compare-LasalClassesVolatilityTriad.ps1'
$ReportFileName =
    'classes_lcb_gate_d_rebuild_triad_24402bfa_6e115876_99014dd9.' +
    'volatility.json'
$KnownPatchPath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.binary.patch'
$KnownManifestPath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.manifest.json'
$KnownOraclePath = $EvidenceRelativeDirectory +
    '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json'
$CheckpointBuildBaselinePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/' +
    'build_baseline_gate_d_visual_layout.json'
$ThirdBuildBaselinePath = $EvidenceRelativeDirectory +
    '/build_baseline_gate_d_rebaseline_6e115876.json'
$ThirdBundleDirectory = $EvidenceRelativeDirectory +
    '/candidate_finalization_gate_d_rebaseline_6e115876'
$ThirdSnapshotPath = $ThirdBundleDirectory +
    '/Classes.post-rebuild.snapshot.lcb'
$ThirdOraclePath = $ThirdBundleDirectory +
    '/classes_lcb_gate_d_rebuild_candidate.comparison.json'
$ThirdManifestPath = $ThirdBundleDirectory +
    '/classes_lcb_gate_d_rebuild_candidate.finalization.json'
$Utf8Strict = New-Object Text.UTF8Encoding($false, $true)
$Utf8NoBom = New-Object Text.UTF8Encoding($false)
$AsciiStrict = New-Object Text.ASCIIEncoding
$Latin1 = [Text.Encoding]::GetEncoding(28591)
$MarkerBytes = [byte[]](
    0x8F, 0x68, 0x1A, 0x16, 0x6D, 0xB0,
    0x6E, 0x37, 0x85, 0xCA, 0x73, 0x41)
$MarkerHex = '8F681A166DB06E3785CA7341'
$ExpectedCandidateTableSha256 =
    'AD8A7FC5D6CB2277819FF28A7B7994C0FD6EAFBE6940419159662B8EFE83924D'
$ExpectedVolatileSlotTableSha256 =
    '9D12A54145C409AC257F011C88F782108BCB3D73E9EDCCD8D2653A387F0F193C'

$PinnedInputs = @(
    [ordered]@{
        role = 'checkpoint244'
        relativePath = $CanonicalClassesPath
        commit = $CheckpointCommit
        blobOid = $CheckpointBlobOid
        rawBytes = $CheckpointBytes
        sha256 = $CheckpointSha256
        format = 'sigmatek-lasal-classes'
    },
    [ordered]@{
        role = 'checkpointBuildBaseline'
        relativePath = $CheckpointBuildBaselinePath
        commit = $KnownRebuildCaptureHead
        blobOid = '1e0c6cc0786907e31d0cbae8391df98dfa61e5e9'
        rawBytes = 6887L
        sha256 =
            '247E41E7ABBD5E59681BC65CBB03F465050146C1FE246B3DE23B200E5903ABFE'
        format = 'strict-utf8-json'
    },
    [ordered]@{
        role = 'known6eBinaryPatch'
        relativePath = $KnownPatchPath
        commit = '703844576c658460a018373894db85e43cda3096'
        blobOid = 'fc36eb76c3293e04a7aa0acf4674d408865ffa70'
        rawBytes = 2553L
        sha256 =
            'AF9A4D32B6F568036E4200BD3F47C9CD63ABB4027D37A1F60BEDB7287731A160'
        format = 'git-binary-patch'
    },
    [ordered]@{
        role = 'known6ePatchManifest'
        relativePath = $KnownManifestPath
        commit = '703844576c658460a018373894db85e43cda3096'
        blobOid = 'e181b57a15bd10465ba6de100aa239d4dfe8709b'
        rawBytes = 2427L
        sha256 =
            'B919A2EC25ABE99C7C8D5D37E19F0EDDB3D7998C1DF7C1F7C74FB3B9B5D8956C'
        format = 'strict-utf8-json'
    },
    [ordered]@{
        role = 'known6eComparisonOracle'
        relativePath = $KnownOraclePath
        commit = '2e8ca8a84a141390424ce859ac8c315a90ec3430'
        blobOid = '2a73c039391a487082bc0958233ef1930a298f91'
        rawBytes = 51102L
        sha256 =
            '9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639'
        format = 'comparator-canonical-json'
    },
    [ordered]@{
        role = 'third990BundleSnapshot'
        relativePath = $ThirdSnapshotPath
        commit = $ThirdCommit
        blobOid = $ThirdBlobOid
        rawBytes = $ThirdBytes
        sha256 = $ThirdSha256
        format = 'sigmatek-lasal-classes'
    },
    [ordered]@{
        role = 'thirdBuildBaseline'
        relativePath = $ThirdBuildBaselinePath
        commit = '703844576c658460a018373894db85e43cda3096'
        blobOid = '42cfc4ed624cd9d197ec33f36c383458d50a9cdf'
        rawBytes = 6887L
        sha256 =
            'BF55B202377C52D0880A7D1E1B7C5B719B3060F2E17BECF4A895820F13AC29C3'
        format = 'strict-utf8-json'
    },
    [ordered]@{
        role = 'third990ComparisonOracle'
        relativePath = $ThirdOraclePath
        commit = $ThirdCommit
        blobOid = '518cc6f709df34692ce8d44822a860f8672ff6c1'
        rawBytes = 46891L
        sha256 =
            '40B2879ED307C17774F823EC4B3AF0A7457B06B4007D71452C1A8BBE0E8550E6'
        format = 'comparator-canonical-json'
    },
    [ordered]@{
        role = 'third990FinalizationManifest'
        relativePath = $ThirdManifestPath
        commit = $ThirdCommit
        blobOid = '4b00199484f9a24cfbf40d2accb7b4e853987872'
        rawBytes = 19197L
        sha256 =
            '1A643464BAA51364059D5ADF8BA992FF80FF18C65B2B155BA40A147B5E4AEF4A'
        format = 'strict-utf8-json'
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

function Throw-TriadBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw [InvalidOperationException]::new("BLOCKED: $Message")
}

function Assert-PowerShell7Production {
    if (($PSVersionTable.PSEdition -cne 'Core') -or
        ($PSVersionTable.PSVersion.Major -lt 7)) {
        Throw-TriadBlocker (
            'production analysis requires PowerShell 7 before evidence or output ' +
            'is read; PS5 remains a canonical/self-test host only.')
    }
}

function Get-ExactScalarProcessExitCode {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$ValueOwner
    )
    if (($Value -is [Array]) -or ($Value -isnot [int])) {
        Throw-TriadBlocker (
            "$ValueOwner must be exactly one System.Int32 process exit code.")
    }
    if ([int]$Value -notin @(0, 2, 3, 4)) {
        Throw-TriadBlocker "$ValueOwner is outside the exact 0/2/3/4 contract."
    }
    return [int]$Value
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $algorithm.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
}

function Get-GitBlobOid {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    [byte[]]$header = [Text.Encoding]::ASCII.GetBytes(
        'blob ' + $Bytes.LongLength + [char]0)
    $algorithm = [Security.Cryptography.SHA1]::Create()
    try {
        $algorithm.TransformBlock($header, 0, $header.Length, $null, 0) | Out-Null
        $algorithm.TransformFinalBlock($Bytes, 0, $Bytes.Length) | Out-Null
        return ([BitConverter]::ToString($algorithm.Hash)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $algorithm.Dispose()
    }
}

function Test-ByteSequencesExact {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][byte[]]$Right
    )
    if ($Left.LongLength -ne $Right.LongLength) { return $false }
    return [Linq.Enumerable]::SequenceEqual([byte[]]$Left, [byte[]]$Right)
}

function Get-HexRange {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$Length
    )
    if (($Start -lt 0) -or ($Length -lt 0) -or
        (($Start + $Length) -gt $Bytes.Length)) {
        Throw-TriadBlocker 'hex range is outside the artifact.'
    }
    if ($Length -eq 0) { return '' }
    return ([BitConverter]::ToString($Bytes, $Start, $Length)).Replace('-', '')
}

function Copy-Bytes {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $copy = New-Object byte[] $Bytes.Length
    [Array]::Copy($Bytes, 0, $copy, 0, $Bytes.Length)
    return ,$copy
}

function Get-GitExecutable {
    $command = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $command) { Throw-TriadBlocker 'git is not available.' }
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
            Throw-TriadBlocker "$Operation contains an unsupported native argument."
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
            Throw-TriadBlocker "$Operation could not start git."
        }
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-TriadBlocker (
                "$Operation failed with git exit $($process.ExitCode): " +
                $stderr.Trim())
        }
        return $stdout.Trim()
    }
    finally {
        $process.Dispose()
    }
}

function Read-GitBlobBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$BlobOid,
        [Parameter(Mandatory = $true)][string]$BlobOwner
    )
    if ($BlobOid -cnotmatch '^[0-9a-f]{40}$') {
        Throw-TriadBlocker "$BlobOwner blob OID is invalid."
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
            Throw-TriadBlocker "$BlobOwner Git blob read did not start."
        }
        $process.StandardOutput.BaseStream.CopyTo($memory)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-TriadBlocker (
                "$BlobOwner Git blob read failed with exit $($process.ExitCode): " +
                $stderr.Trim())
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
        Throw-TriadBlocker 'script-bound repository root has no .git entry.'
    }
    $root = $scriptBoundRoot
    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        if (-not (Test-Path -LiteralPath $RequestedRoot -PathType Container)) {
            Throw-TriadBlocker 'RepositoryRoot is not a directory.'
        }
        $requested = (Resolve-Path -LiteralPath $RequestedRoot).Path.TrimEnd('\')
        if (-not [string]::Equals(
                $requested, $scriptBoundRoot,
                [StringComparison]::OrdinalIgnoreCase)) {
            Throw-TriadBlocker 'RepositoryRoot differs from the script-bound root.'
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
        Throw-TriadBlocker 'script-bound root is not the exact Git worktree root.'
    }
    return $root
}

function Resolve-ProducerIdentity {
    param([Parameter(Mandatory = $true)][string]$Root)
    $expectedPath = [IO.Path]::GetFullPath(
        (Join-Path $Root $ToolRelativePath.Replace('/', '\')))
    $actualPath = [IO.Path]::GetFullPath($PSCommandPath)
    if (-not [string]::Equals(
            $actualPath, $expectedPath,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-TriadBlocker 'producer script path differs from the exact tool path.'
    }
    $item = Get-Item -LiteralPath $actualPath -Force -ErrorAction Stop
    if (($item -isnot [IO.FileInfo]) -or
        (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) -or
        ([string]$item.Name -cne [IO.Path]::GetFileName($expectedPath))) {
        Throw-TriadBlocker 'producer script physical identity differs.'
    }
    Assert-NoReparsePointChain -Path $item.DirectoryName -Root $Root `
        -PathOwner 'producer script'
    $scopedStatus = Invoke-GitText -Root $Root `
        -Arguments @(
            'status', '--porcelain=v1', '--untracked-files=all',
            '--', $ToolRelativePath) `
        -Operation 'producer scoped HEAD-clean check'
    if (-not [string]::IsNullOrEmpty($scopedStatus)) {
        Throw-TriadBlocker 'producer script is not tracked and scoped HEAD-clean.'
    }
    $treeLine = Invoke-GitText -Root $Root `
        -Arguments @('ls-tree', 'HEAD', '--', $ToolRelativePath) `
        -Operation 'producer HEAD tree resolution'
    $treeMatch = [regex]::Match(
        $treeLine, '^100644 blob ([0-9a-f]{40})\t(.+)$')
    if ((-not $treeMatch.Success) -or
        ($treeMatch.Groups[2].Value -cne $ToolRelativePath)) {
        Throw-TriadBlocker 'producer script is not exact mode 100644 at HEAD.'
    }
    $headBlobOid = $treeMatch.Groups[1].Value
    $producerCommit = Invoke-GitText -Root $Root `
        -Arguments @(
            'log', '-1', '--format=%H', '--', $ToolRelativePath) `
        -Operation 'producer stable commit resolution'
    if ($producerCommit -cnotmatch '^[0-9a-f]{40}$') {
        Throw-TriadBlocker 'producer stable commit identity is invalid.'
    }
    $producerBlobOid = Invoke-GitText -Root $Root `
        -Arguments @(
            'rev-parse', '--verify',
            "$producerCommit`:$ToolRelativePath") `
        -Operation 'producer stable commit blob resolution'
    if ($producerBlobOid -cne $headBlobOid) {
        Throw-TriadBlocker 'producer stable commit blob differs from HEAD path.'
    }
    [byte[]]$headBlobBytes = Read-GitBlobBytes -Root $Root `
        -BlobOid $headBlobOid -BlobOwner 'producer HEAD script'
    [byte[]]$physicalBytes = [IO.File]::ReadAllBytes($actualPath)
    if ((-not (Test-ByteSequencesExact `
                -Left $physicalBytes -Right $headBlobBytes)) -or
        ((Get-GitBlobOid -Bytes $physicalBytes) -cne $headBlobOid)) {
        Throw-TriadBlocker 'producer physical bytes differ from HEAD blob.'
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
        -Arguments @('rev-parse', '--verify', "$($Definition.commit)^{commit}") `
        -Operation "$($Definition.role) commit resolution"
    if ($commit -cne [string]$Definition.commit) {
        Throw-TriadBlocker "$($Definition.role) commit identity differs."
    }
    $resolvedBlob = Invoke-GitText -Root $Root `
        -Arguments @(
            'rev-parse', '--verify',
            "$commit`:$($Definition.relativePath)") `
        -Operation "$($Definition.role) path resolution"
    if ($resolvedBlob -cne [string]$Definition.blobOid) {
        Throw-TriadBlocker "$($Definition.role) path/blob identity differs."
    }
    [byte[]]$bytes = Read-GitBlobBytes -Root $Root `
        -BlobOid $resolvedBlob -BlobOwner ([string]$Definition.role)
    $sha256 = Get-BytesSha256 -Bytes $bytes
    if (($bytes.LongLength -ne [long]$Definition.rawBytes) -or
        ($sha256 -cne [string]$Definition.sha256) -or
        ((Get-GitBlobOid -Bytes $bytes) -cne [string]$Definition.blobOid)) {
        Throw-TriadBlocker "$($Definition.role) committed bytes differ from the pin."
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

function ConvertTo-DeterministicJson {
    param([Parameter(Mandatory = $true)]$Value)
    $json = ($Value | ConvertTo-Json -Depth 32 -Compress)
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
        Throw-TriadBlocker 'deterministic JSON is not 7-bit ASCII.'
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
        Throw-TriadBlocker "$JsonOwner is not strict UTF-8 JSON."
    }
    if ($RequireComparatorCanonical) {
        $canonical = ConvertTo-DeterministicJson -Value $value
        [byte[]]$canonicalBytes = Get-DeterministicJsonBytes -Json $canonical
        if (-not (Test-ByteSequencesExact -Left $Bytes -Right $canonicalBytes)) {
            Throw-TriadBlocker "$JsonOwner is not comparator-canonical JSON."
        }
    }
    return $value
}

function Write-JsonBytesToStream {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][IO.Stream]$Stream
    )
    $Stream.Write($Bytes, 0, $Bytes.Length)
    $Stream.Flush()
}

function Write-JsonStdout {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $stdout = [Console]::OpenStandardOutput()
    Write-JsonBytesToStream -Bytes $Bytes -Stream $stdout
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
        Throw-TriadBlocker "$PathOwner parent does not exist."
    }
    $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $cursorPath -Force)
    while ($null -ne $cursor) {
        if (-not (Test-PathInsideRoot -Path $cursor.FullName -Root $resolvedRoot)) {
            Throw-TriadBlocker "$PathOwner escapes its allowed root."
        }
        if (($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-TriadBlocker "$PathOwner uses a reparse-point parent."
        }
        if ([string]::Equals(
                $cursor.FullName.TrimEnd('\'), $resolvedRoot,
                [StringComparison]::OrdinalIgnoreCase)) {
            return
        }
        $cursor = $cursor.Parent
    }
    Throw-TriadBlocker "$PathOwner did not reach its allowed root."
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
        Throw-TriadBlocker (
            "$PathOwner stream inventory failed: $($_.Exception.Message)")
    }
    if (Test-Path -LiteralPath $Path -PathType Container) {
        if ($streams.Count -ne 0) {
            Throw-TriadBlocker "$PathOwner contains a directory alternate stream."
        }
        return
    }
    if (($streams.Count -ne 1) -or
        ([string]$streams[0].Stream -cne ':$DATA')) {
        Throw-TriadBlocker "$PathOwner contains a non-default data stream."
    }
}

function Assert-SafeWindowsOutputPathText {
    param([Parameter(Mandatory = $true)][string]$RequestedPath)
    if ([string]::IsNullOrWhiteSpace($RequestedPath)) {
        Throw-TriadBlocker 'OutputPath is empty.'
    }
    if ($RequestedPath.StartsWith('\\?\', [StringComparison]::Ordinal) -or
        $RequestedPath.StartsWith('\\.\', [StringComparison]::Ordinal)) {
        Throw-TriadBlocker 'OutputPath uses a Windows device namespace alias.'
    }
    if ([regex]::IsMatch($RequestedPath, '[\x00-\x1F<>"|?*]')) {
        Throw-TriadBlocker 'OutputPath contains a prohibited Windows path character.'
    }
    $normalizedSeparators = $RequestedPath.Replace('/', '\')
    if ([regex]::IsMatch($normalizedSeparators, '^[A-Za-z]:[^\\]')) {
        Throw-TriadBlocker 'OutputPath uses a drive-relative alias.'
    }
    $rootText = [IO.Path]::GetPathRoot($normalizedSeparators)
    $nonRootText = $normalizedSeparators.Substring($rootText.Length)
    if ($nonRootText.IndexOf(':') -ge 0) {
        Throw-TriadBlocker 'OutputPath names an alternate data stream.'
    }
    foreach ($segment in @($nonRootText.Split('\'))) {
        if ($segment.Length -eq 0) { continue }
        if (($segment -ceq '.') -or ($segment -ceq '..')) {
            Throw-TriadBlocker 'OutputPath is not already lexically normalized.'
        }
        if ($segment.EndsWith(' ', [StringComparison]::Ordinal) -or
            $segment.EndsWith('.', [StringComparison]::Ordinal)) {
            Throw-TriadBlocker 'OutputPath contains a trailing dot or space alias.'
        }
        $deviceStem = $segment.Split('.')[0]
        if ([regex]::IsMatch(
                $deviceStem,
                '^(CON|PRN|AUX|NUL|CLOCK\$|CONIN\$|CONOUT\$|' +
                'COM([1-9]|\xB9|\xB2|\xB3)|' +
                'LPT([1-9]|\xB9|\xB2|\xB3))$',
                [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            Throw-TriadBlocker 'OutputPath uses a reserved Windows device alias.'
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
        Throw-TriadBlocker 'output descriptor state request is contradictory.'
    }
    [string[]]$keys = @($Descriptor.Keys)
    if (($keys.Count -ne 3) -or
        ($keys[0] -cne 'FullPath') -or
        ($keys[1] -cne 'ExactParent') -or
        ($keys[2] -cne 'AllowedRoot')) {
        Throw-TriadBlocker 'output descriptor exact key sequence differs.'
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
        Throw-TriadBlocker 'output descriptor normalized identity differs.'
    }
    if ((-not [IO.Directory]::Exists($allowedRoot)) -or
        (-not [IO.Directory]::Exists($exactParent))) {
        Throw-TriadBlocker 'output descriptor root or exact parent is missing.'
    }
    $resolvedAllowedRoot = (Resolve-Path -LiteralPath $allowedRoot).Path.TrimEnd('\')
    $resolvedExactParent = (Resolve-Path -LiteralPath $exactParent).Path.TrimEnd('\')
    if (($resolvedAllowedRoot -cne $allowedRoot) -or
        ($resolvedExactParent -cne $exactParent)) {
        Throw-TriadBlocker 'output descriptor root or parent resolution differs.'
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
        Throw-TriadBlocker 'output descriptor parent chain did not reach its root.'
    }
    $targetExists = [IO.File]::Exists($fullPath) -or
        [IO.Directory]::Exists($fullPath)
    if ($RequireTargetAbsent -and $targetExists) {
        Throw-TriadBlocker 'OutputPath already exists; overwrite is prohibited.'
    }
    if ($RequireTargetPresent) {
        if (-not [IO.File]::Exists($fullPath)) {
            Throw-TriadBlocker 'CreateNew output target is missing.'
        }
        $target = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
        if (($target -isnot [IO.FileInfo]) -or
            (($target.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) -or
            ($target.FullName -cne $fullPath) -or
            ([string]$target.Name -cne $ReportFileName)) {
            Throw-TriadBlocker 'CreateNew output target identity differs.'
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
        Throw-TriadBlocker 'OutputPath must use the exact triad report basename.'
    }
    if ((-not [IO.Directory]::Exists($AllowedRoot)) -or
        (-not [IO.Directory]::Exists($ExactParent))) {
        Throw-TriadBlocker 'OutputPath allowed root or exact parent is missing.'
    }
    $resolvedAllowedRoot = (Resolve-Path -LiteralPath $AllowedRoot).Path.TrimEnd('\')
    $resolvedExactParent = (Resolve-Path -LiteralPath $ExactParent).Path.TrimEnd('\')
    $combined = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        $RequestedPath
    }
    else {
        Join-Path $resolvedExactParent $RequestedPath
    }
    $fullPath = [IO.Path]::GetFullPath($combined)
    if (([IO.Path]::GetFullPath($fullPath) -cne $fullPath) -or
        ([IO.Path]::GetFileName($fullPath) -cne $requestedFileName)) {
        Throw-TriadBlocker 'OutputPath normalized identity differs from the request.'
    }
    $parent = [IO.Path]::GetDirectoryName($fullPath).TrimEnd('\')
    if ($parent -cne $resolvedExactParent) {
        Throw-TriadBlocker 'OutputPath must be a direct child of the exact report parent.'
    }
    $expectedFullPath = Join-Path $resolvedExactParent $ReportFileName
    if (($fullPath -cne $expectedFullPath) -or
        (-not (Test-PathInsideRoot `
                -Path $resolvedExactParent -Root $resolvedAllowedRoot))) {
        Throw-TriadBlocker 'OutputPath differs from the exact triad report path.'
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
            Throw-TriadBlocker 'CreateNew output read-back differs.'
        }
        $completed = $true
    }
    catch [IO.IOException] {
        Throw-TriadBlocker (
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
                # Fail closed: never follow an untrusted replacement path to clean up.
            }
        }
    }
}

function Assert-InvocationContract {
    param([Parameter(Mandatory = $true)][Collections.IDictionary]$Bound)
    if ($CreateNew -and [string]::IsNullOrWhiteSpace($OutputPath)) {
        Throw-TriadBlocker '-CreateNew requires -OutputPath.'
    }
    if ((-not $CreateNew) -and $Bound.ContainsKey('OutputPath')) {
        Throw-TriadBlocker '-OutputPath requires -CreateNew.'
    }
}

function Assert-PatchContract {
    param(
        [Parameter(Mandatory = $true)][byte[]]$PatchBytes,
        [Parameter(Mandatory = $true)]$Manifest
    )
    foreach ($value in $PatchBytes) {
        if ($value -gt 0x7F) {
            Throw-TriadBlocker 'known 6E binary patch is not 7-bit ASCII.'
        }
    }
    $text = [Text.Encoding]::ASCII.GetString($PatchBytes).Replace("`r`n", "`n")
    $expectedHeader =
        "diff --git a/$CanonicalClassesPath b/$CanonicalClassesPath`n" +
        "index $CheckpointBlobOid..$KnownRebuildBlobOid 100644`n" +
        "GIT binary patch`n"
    if (-not $text.StartsWith($expectedHeader, [StringComparison]::Ordinal)) {
        Throw-TriadBlocker 'known 6E binary patch header/index differs.'
    }
    if (([regex]::Matches($text, '(?m)^diff --git ')).Count -ne 1 -or
        ([regex]::Matches($text, '(?m)^GIT binary patch$')).Count -ne 1 -or
        ([regex]::Matches($text, '(?m)^delta ')).Count -ne 2 -or
        ([regex]::Matches($text, '(?m)^literal ')).Count -ne 0) {
        Throw-TriadBlocker 'known 6E binary patch section inventory differs.'
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
        Throw-TriadBlocker 'known 6E patch manifest contract differs.'
    }
}

function Convert-HexToBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Hex,
        [Parameter(Mandatory = $true)][string]$ValueOwner
    )
    if (($Hex.Length -eq 0) -or (($Hex.Length % 2) -ne 0) -or
        ($Hex -cnotmatch '^[0-9A-F]+$')) {
        Throw-TriadBlocker "$ValueOwner is not canonical uppercase hex."
    }
    $result = New-Object byte[] ($Hex.Length / 2)
    for ($index = 0; $index -lt $result.Length; $index++) {
        $result[$index] = [Convert]::ToByte($Hex.Substring($index * 2, 2), 16)
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
    if ($null -eq $Object) { Throw-TriadBlocker "$ObjectOwner is null." }
    [string[]]$actual = @($Object.PSObject.Properties.Name)
    if (($actual.Count -ne $Expected.Count) -or
        ((ConvertTo-DeterministicJson -Value $actual) -cne
            (ConvertTo-DeterministicJson -Value $Expected))) {
        Throw-TriadBlocker "$ObjectOwner exact key sequence differs."
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
            Throw-TriadBlocker "$OracleOwner contains a non-integer numeric field."
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
            Throw-TriadBlocker "$OracleOwner contains a non-Boolean contract field."
        }
    }
    if (($Oracle.diffRuns -isnot [Array]) -or
        ($Oracle.changedCheckpointOwners -isnot [Array]) -or
        ($Oracle.comparison.frozenOpaqueOwners -isnot [Array])) {
        Throw-TriadBlocker "$OracleOwner contains a non-array contract field."
    }
    foreach ($summary in @($Oracle.changedCheckpointOwners)) {
        Assert-ExactJsonKeys -Object $summary `
            -ObjectOwner "$OracleOwner.changedCheckpointOwners[]" -Expected @(
                'owner', 'sourcePath', 'diffRunCount',
                'changedCheckpointBytes', 'classification')
        if ((-not (Test-JsonInteger $summary.diffRunCount)) -or
            (-not (Test-JsonInteger $summary.changedCheckpointBytes))) {
            Throw-TriadBlocker "$OracleOwner changed-owner counts are not integers."
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
                Throw-TriadBlocker "$OracleOwner preview property type differs."
            }
        }
        foreach ($ownersName in @('checkpointOwners', 'candidateOwners')) {
            if ($run.$ownersName -isnot [Array]) {
                Throw-TriadBlocker "$OracleOwner $ownersName is not an array."
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
                        Throw-TriadBlocker "$OracleOwner owner mapping is not integer typed."
                    }
                }
            }
        }
        foreach ($value in @(
                $run.ordinal, $run.checkpointStart, $run.checkpointBytes,
                $run.candidateStart, $run.candidateBytes)) {
            if (-not (Test-JsonInteger $value)) {
                Throw-TriadBlocker "$OracleOwner run field is not integer typed."
            }
        }
        if ($run.mappingComplete -isnot [bool]) {
            Throw-TriadBlocker "$OracleOwner mappingComplete is not Boolean."
        }
    }
}

function Get-OracleReconstruction {
    param(
        [Parameter(Mandatory = $true)]$Oracle,
        [Parameter(Mandatory = $true)][byte[]]$CheckpointArtifact,
        [Parameter(Mandatory = $true)][string]$OracleOwner,
        [Parameter(Mandatory = $true)][long]$ExpectedCandidateBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedCandidateSha256,
        [Parameter(Mandatory = $true)][string]$ExpectedCandidateBlobOid,
        [Parameter(Mandatory = $true)][int]$ExpectedChangedBytes,
        [Parameter(Mandatory = $true)][int]$ExpectedRunCount,
        [Parameter(Mandatory = $true)][int]$ExpectedOwnerCount
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
        ([long]$Oracle.candidate.rawBytes -ne $ExpectedCandidateBytes) -or
        ([string]$Oracle.candidate.sha256 -cne $ExpectedCandidateSha256) -or
        [bool]$Oracle.comparison.byteExact -or
        (-not [bool]$Oracle.comparison.equalLength) -or
        ([long]$Oracle.comparison.lengthDelta -ne 0L) -or
        ($Oracle.comparison.alignment -cne 'equal-length-indexed') -or
        (-not [bool]$Oracle.comparison.changedByteCountDefined) -or
        ([int]$Oracle.comparison.changedByteCount -ne $ExpectedChangedBytes) -or
        ([int]$Oracle.comparison.contiguousRunCount -ne $ExpectedRunCount) -or
        ([int]$Oracle.comparison.checkpointChangedOwnerCount -ne
            $ExpectedOwnerCount) -or
        ([int]$Oracle.comparison.unmappedRunCount -ne 0) -or
        (-not [bool]$Oracle.comparison.changedOwnersAreFrozenOpaqueSubset) -or
        [bool]$Oracle.comparison.proprietaryFieldSemanticsDecoded -or
        (-not [bool]$Oracle.gateDTargetRecords.allEqual) -or
        (-not [bool]$Oracle.protectedDependencyRecords.allEqual)) {
        Throw-TriadBlocker "$OracleOwner common contract differs."
    }
    if (($Oracle.diffRuns -isnot [Array]) -or
        (@($Oracle.diffRuns).Count -ne $ExpectedRunCount) -or
        ($Oracle.changedCheckpointOwners -isnot [Array]) -or
        (@($Oracle.changedCheckpointOwners).Count -ne $ExpectedOwnerCount)) {
        Throw-TriadBlocker "$OracleOwner array shape differs."
    }
    foreach ($record in @($Oracle.gateDTargetRecords.records)) {
        if (-not [bool]$record.exact) {
            Throw-TriadBlocker "$OracleOwner has a non-exact Gate D record."
        }
    }
    foreach ($record in @($Oracle.protectedDependencyRecords.records)) {
        if ((-not [bool]$record.exact) -or
            (-not [bool]$record.legacyWindowExact)) {
            Throw-TriadBlocker "$OracleOwner has a non-exact protected record."
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
            (-not [bool]$run.mappingComplete)) {
            Throw-TriadBlocker "$OracleOwner diff run $ordinal shape differs."
        }
        if ([bool]$run.checkpointPreview.truncated -or
            [bool]$run.candidatePreview.truncated -or
            ([int]$run.checkpointPreview.previewBytes -ne $length) -or
            ([int]$run.candidatePreview.previewBytes -ne $length)) {
            Throw-TriadBlocker "$OracleOwner diff run $ordinal preview is incomplete."
        }
        $checkpointHex = [string]$run.checkpointPreview.hex
        $candidateHex = [string]$run.candidatePreview.hex
        [byte[]]$checkpointRun = Convert-HexToBytes `
            -Hex $checkpointHex -ValueOwner "$OracleOwner checkpoint preview $ordinal"
        [byte[]]$candidateRun = Convert-HexToBytes `
            -Hex $candidateHex -ValueOwner "$OracleOwner candidate preview $ordinal"
        if (($checkpointRun.Length -ne $length) -or
            ($candidateRun.Length -ne $length) -or
            ((Get-HexRange -Bytes $CheckpointArtifact `
                    -Start $start -Length $length) -cne $checkpointHex)) {
            Throw-TriadBlocker "$OracleOwner diff run $ordinal preview differs."
        }
        for ($byteIndex = 0; $byteIndex -lt $length; $byteIndex++) {
            if ($checkpointRun[$byteIndex] -eq $candidateRun[$byteIndex]) {
                Throw-TriadBlocker "$OracleOwner diff run $ordinal contains equal bytes."
            }
            $candidate[$start + $byteIndex] = $candidateRun[$byteIndex]
            [void]$offsets.Add($start + $byteIndex)
        }
        $checkpointOwners = @($run.checkpointOwners)
        $candidateOwners = @($run.candidateOwners)
        if (($checkpointOwners.Count -ne 1) -or
            ($candidateOwners.Count -ne 1)) {
            Throw-TriadBlocker "$OracleOwner diff run $ordinal owner mapping differs."
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
            Throw-TriadBlocker "$OracleOwner diff run $ordinal owner fields differ."
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
                Throw-TriadBlocker "$OracleOwner owner boundary is inconsistent."
            }
        }
        else {
            $boundaries[$ownerName] = $boundary
        }
        $previousEnd = $start + $length - 1
        $changedSum += $length
    }
    if (($changedSum -ne $ExpectedChangedBytes) -or
        ($offsets.Count -ne $ExpectedChangedBytes) -or
        ($boundaries.Count -ne $ExpectedOwnerCount) -or
        ($candidate.LongLength -ne $ExpectedCandidateBytes) -or
        ((Get-BytesSha256 -Bytes $candidate) -cne $ExpectedCandidateSha256) -or
        ((Get-GitBlobOid -Bytes $candidate) -cne $ExpectedCandidateBlobOid)) {
        Throw-TriadBlocker "$OracleOwner reconstructed candidate identity differs."
    }
    $changedNames = @($Oracle.changedCheckpointOwners | ForEach-Object {
            [string]$_.owner
        })
    $boundaryNames = @($boundaries.Keys | Sort-Object)
    $summaryNames = @($changedNames | Sort-Object)
    if ((ConvertTo-DeterministicJson -Value $boundaryNames) -cne
        (ConvertTo-DeterministicJson -Value $summaryNames)) {
        Throw-TriadBlocker "$OracleOwner changed owner summary differs."
    }
    foreach ($name in $changedNames) {
        if ($FrozenOpaqueVendorOwners -cnotcontains $name) {
            Throw-TriadBlocker "$OracleOwner includes a non-frozen owner."
        }
    }
    return [pscustomobject]@{
        CandidateBytes = $candidate
        ChangedOffsets = $offsets.ToArray()
        OwnerBoundaries = $boundaries
        ChangedOwnerNames = $changedNames
    }
}

function Assert-OracleInvariantSections {
    param(
        [Parameter(Mandatory = $true)]$KnownOracle,
        [Parameter(Mandatory = $true)]$ThirdOracle
    )
    foreach ($name in @(
            'recordParser', 'gateDTargetRecords',
            'protectedDependencyRecords')) {
        if (-not (Test-JsonSectionsExact `
                -Left $KnownOracle.$name -Right $ThirdOracle.$name)) {
            Throw-TriadBlocker "oracle invariant section $name differs."
        }
    }
    if (-not (Test-JsonSectionsExact `
            -Left $KnownOracle.comparison.frozenOpaqueOwners `
            -Right $ThirdOracle.comparison.frozenOpaqueOwners)) {
        Throw-TriadBlocker 'oracle frozen opaque owner universe differs.'
    }
}

function Get-FullOwnerBoundaries {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )
    $signature = 'SigmatekLasal2Binary' + [char]0
    $text = $Latin1.GetString($Bytes)
    if (($text.Length -ne $Bytes.Length) -or
        (-not $text.StartsWith($signature, [StringComparison]::Ordinal)) -or
        ($text.IndexOf($signature, $signature.Length,
                [StringComparison]::Ordinal) -ge 0)) {
        Throw-TriadBlocker "$ArtifactOwner Classes signature differs."
    }
    $matches = [regex]::Matches(
        $text, '\.\\Class\\([^\\\x00]+)\\\1\.st')
    if ($matches.Count -ne 120) {
        Throw-TriadBlocker "$ArtifactOwner source owner inventory count differs."
    }
    $sources = New-Object Collections.Generic.List[object]
    $seen = @{}
    foreach ($match in $matches) {
        $ownerName = [string]$match.Groups[1].Value
        $sourcePath = [string]$match.Value
        $pathStart = [int]$match.Index
        $markerStart = $pathStart - 4
        if (($markerStart -lt 0) -or
            ($Bytes[$pathStart - 1] -ne 0xAA)) {
            Throw-TriadBlocker "$ArtifactOwner source marker is missing."
        }
        $encodedLength = [int]$Bytes[$markerStart] -bor
            ([int]$Bytes[$markerStart + 1] -shl 8) -bor
            ([int]$Bytes[$markerStart + 2] -shl 16)
        if ($encodedLength -ne $sourcePath.Length) {
            Throw-TriadBlocker "$ArtifactOwner source marker length differs."
        }
        if ($seen.ContainsKey($ownerName.ToUpperInvariant())) {
            Throw-TriadBlocker "$ArtifactOwner has a duplicate owner $ownerName."
        }
        $seen[$ownerName.ToUpperInvariant()] = $true
        $sources.Add([pscustomobject]@{
                owner = $ownerName
                sourcePath = $sourcePath
                pathStart = $pathStart
                markerStart = $markerStart
            })
    }
    if ($sources[0].owner -cne '_AxisBase') {
        Throw-TriadBlocker "$ArtifactOwner first owner is not _AxisBase."
    }
    $headers = New-Object Collections.Generic.List[int]
    for ($index = 1; $index -lt $sources.Count; $index++) {
        $ownerName = [string]$sources[$index].owner
        $headerText = [string]([char]0xAA) + [char]0x03 +
            [char]($ownerName.Length -band 0xFF) +
            [char](($ownerName.Length -shr 8) -band 0xFF) +
            [char](($ownerName.Length -shr 16) -band 0xFF) +
            [char]0xAA + $ownerName
        $searchStart = [int]$sources[$index - 1].pathStart
        $headerStart = $text.IndexOf(
            $headerText, $searchStart, [StringComparison]::Ordinal)
        if (($headerStart -lt 0) -or
            ($headerStart -ge [int]$sources[$index].markerStart)) {
            Throw-TriadBlocker "$ArtifactOwner true header is missing for $ownerName."
        }
        $second = $text.IndexOf(
            $headerText, $headerStart + 1, [StringComparison]::Ordinal)
        if (($second -ge 0) -and
            ($second -lt [int]$sources[$index].markerStart)) {
            Throw-TriadBlocker "$ArtifactOwner true header is ambiguous for $ownerName."
        }
        [void]$headers.Add($headerStart)
    }
    if ($headers.Count -ne 119) {
        Throw-TriadBlocker "$ArtifactOwner true header count differs."
    }
    $boundaries = [ordered]@{}
    for ($index = 0; $index -lt $sources.Count; $index++) {
        $start = if ($index -eq 0) { 0 } else { $headers[$index - 1] }
        $end = if (($index + 1) -lt $sources.Count) {
            $headers[$index]
        }
        else {
            $Bytes.Length
        }
        if (($end -le $start) -or
            ([int]$sources[$index].markerStart -lt $start) -or
            (([int]$sources[$index].pathStart +
                    ([string]$sources[$index].sourcePath).Length) -gt $end) -or
            (($end - 48) -lt $start)) {
            Throw-TriadBlocker "$ArtifactOwner owner boundary differs."
        }
        $name = [string]$sources[$index].owner
        $boundaries[$name] = [ordered]@{
            owner = $name
            sourcePath = [string]$sources[$index].sourcePath
            recordStart = [int]$start
            recordEndExclusive = [int]$end
        }
    }
    return $boundaries
}

function Assert-OwnerInventoriesExact {
    param(
        [Parameter(Mandatory = $true)][Collections.IDictionary]$A,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$B,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$C,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$KnownOracle,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$ThirdOracle
    )
    if (($A.Count -ne 120) -or
        (-not (Test-JsonSectionsExact -Left $A -Right $B)) -or
        (-not (Test-JsonSectionsExact -Left $A -Right $C))) {
        Throw-TriadBlocker 'full 120-owner inventory differs across triad artifacts.'
    }
    foreach ($source in @($KnownOracle, $ThirdOracle)) {
        foreach ($name in $source.Keys) {
            if ((-not $A.Contains($name)) -or
                (-not (Test-JsonSectionsExact -Left $A[$name] -Right $source[$name]))) {
                Throw-TriadBlocker "oracle owner boundary $name differs from full inventory."
            }
        }
    }
}

function Assert-ThirdManifestContract {
    param(
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)]$ThirdOracle
    )
    if (($Manifest.schema -cne
            'LasalClassesRebuildCandidateFinalization/v1') -or
        (-not [bool]$Manifest.complete) -or
        ([string]$Manifest.repository.headObserved -cne $ThirdCaptureHead) -or
        ([string]$Manifest.regeneratedOutputs.classes.sourceRelativePath -cne
            $CanonicalClassesPath) -or
        ([string]$Manifest.regeneratedOutputs.classes.snapshotFileName -cne
            'Classes.post-rebuild.snapshot.lcb') -or
        ([long]$Manifest.regeneratedOutputs.classes.bytes -ne $ThirdBytes) -or
        ([string]$Manifest.regeneratedOutputs.classes.sha256 -cne $ThirdSha256) -or
        (-not [bool]$Manifest.regeneratedOutputs.comparatorMatchedClassesSnapshot) -or
        (-not [bool]$Manifest.regeneratedOutputs.finalProductionRehashMatchedSnapshots) -or
        ([string]$Manifest.decision.disposition -cne
            'UNSTABLE_THIRD_CLASSES_HASH_STOP') -or
        ([int]$Manifest.decision.exitCode -ne 3) -or
        [bool]$Manifest.decision.productionApproved -or
        [bool]$Manifest.decision.semanticEquivalenceClaimedForOpaqueDrift -or
        [bool]$Manifest.decision.staticReplayPermitted -or
        [bool]$Manifest.decision.onlineRuntimeQualificationPermitted -or
        [bool]$Manifest.productionApproved -or
        ([int]$Manifest.tools.comparator.exitCode -ne 2) -or
        ([string]$Manifest.tools.comparator.disposition -cne
            [string]$ThirdOracle.decision.disposition)) {
        Throw-TriadBlocker 'third 990 finalization manifest contract differs.'
    }
}

function Merge-OwnerBoundaries {
    param(
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Known,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Third
    )
    $merged = @{}
    foreach ($source in @($Known, $Third)) {
        foreach ($name in $source.Keys) {
            if ($merged.ContainsKey($name)) {
                if (-not (Test-JsonSectionsExact `
                        -Left $merged[$name] -Right $source[$name])) {
                    Throw-TriadBlocker "owner boundary $name differs across oracles."
                }
            }
            else {
                $merged[$name] = $source[$name]
            }
        }
    }
    if ($merged.Count -ne 36) {
        Throw-TriadBlocker 'merged frozen opaque owner boundary count differs.'
    }
    foreach ($name in $FrozenOpaqueVendorOwners) {
        if (-not $merged.ContainsKey($name)) {
            Throw-TriadBlocker "merged owner boundary is missing $name."
        }
    }
    return $merged
}

function New-IntSet {
    param([int[]]$Values)
    $set = New-Object 'Collections.Generic.HashSet[int]'
    foreach ($value in @($Values)) { [void]$set.Add([int]$value) }
    return ,$set
}

function Get-ContiguousRuns {
    param([int[]]$Offsets)
    [int[]]$sorted = @($Offsets | Sort-Object -Unique)
    $runs = New-Object Collections.Generic.List[object]
    if ($sorted.Count -eq 0) { return $runs.ToArray() }
    $start = $sorted[0]
    $previous = $start
    for ($index = 1; $index -lt $sorted.Count; $index++) {
        $current = $sorted[$index]
        if ($current -ne ($previous + 1)) {
            $runs.Add([ordered]@{
                    start = [int]$start
                    bytes = [int]($previous - $start + 1)
                })
            $start = $current
        }
        $previous = $current
    }
    $runs.Add([ordered]@{
            start = [int]$start
            bytes = [int]($previous - $start + 1)
        })
    return $runs.ToArray()
}

function Get-RunLengthHistogram {
    param([object[]]$Runs)
    $counts = @{}
    foreach ($run in @($Runs)) {
        $key = [string][int]$run.bytes
        if (-not $counts.ContainsKey($key)) { $counts[$key] = 0 }
        $counts[$key]++
    }
    $reports = New-Object Collections.Generic.List[object]
    foreach ($key in @($counts.Keys | Sort-Object {[int]$_})) {
        $reports.Add([ordered]@{
                bytes = [int]$key
                count = [int]$counts[$key]
            })
    }
    return $reports.ToArray()
}

function Get-OwnerAtOffset {
    param(
        [Parameter(Mandatory = $true)][int]$Offset,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Boundaries
    )
    $matches = @($Boundaries.Values | Where-Object {
            ($Offset -ge [int]$_.recordStart) -and
            ($Offset -lt [int]$_.recordEndExclusive)
        })
    if ($matches.Count -ne 1) {
        Throw-TriadBlocker "offset $Offset does not map to one frozen owner."
    }
    return $matches[0]
}

function Get-ChangedOwnerNames {
    param(
        [Parameter(Mandatory = $true)][int[]]$Offsets,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Boundaries
    )
    $names = @{}
    foreach ($offset in $Offsets) {
        $owner = Get-OwnerAtOffset -Offset $offset -Boundaries $Boundaries
        $names[[string]$owner.owner] = $true
    }
    return @($FrozenOpaqueVendorOwners | Where-Object {
            $names.ContainsKey($_)
        })
}

function Get-MarkerFollowerPositions {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $text = $Latin1.GetString($Bytes)
    $marker = $Latin1.GetString($MarkerBytes)
    $positions = New-Object Collections.Generic.List[int]
    $cursor = 0
    while ($cursor -lt $text.Length) {
        $position = $text.IndexOf(
            $marker, $cursor, [StringComparison]::Ordinal)
        if ($position -lt 0) { break }
        if (($position + $MarkerBytes.Length + 2) -gt $Bytes.Length) {
            Throw-TriadBlocker 'marker occurrence has no complete 16-bit follower.'
        }
        [void]$positions.Add($position + $MarkerBytes.Length)
        $cursor = $position + 1
    }
    return $positions.ToArray()
}

function Test-MarkerBefore {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$FollowerOffset
    )
    if ($FollowerOffset -lt $MarkerBytes.Length) { return $false }
    $start = $FollowerOffset - $MarkerBytes.Length
    for ($index = 0; $index -lt $MarkerBytes.Length; $index++) {
        if ($Bytes[$start + $index] -ne $MarkerBytes[$index]) {
            return $false
        }
    }
    return $true
}

function Get-LittleEndianWord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Offset
    )
    return [uint16](([uint16]$Bytes[$Offset]) -bor
        ([uint16]$Bytes[$Offset + 1] -shl 8))
}

function Get-WordEqualityClass {
    param(
        [Parameter(Mandatory = $true)][byte[]]$A,
        [Parameter(Mandatory = $true)][byte[]]$B,
        [Parameter(Mandatory = $true)][byte[]]$C,
        [Parameter(Mandatory = $true)][int]$Offset
    )
    $aWord = Get-LittleEndianWord -Bytes $A -Offset $Offset
    $bWord = Get-LittleEndianWord -Bytes $B -Offset $Offset
    $cWord = Get-LittleEndianWord -Bytes $C -Offset $Offset
    if (($aWord -eq $bWord) -and ($bWord -eq $cWord)) { return 'ALL_EQUAL' }
    if ($aWord -eq $bWord) { return 'A_B_EQUAL' }
    if ($aWord -eq $cWord) { return 'A_C_EQUAL' }
    if ($bWord -eq $cWord) { return 'B_C_EQUAL' }
    return 'ALL_DISTINCT'
}

function Get-EqualityCounts {
    param([string[]]$Classes)
    return [ordered]@{
        allDistinct = @($Classes | Where-Object { $_ -ceq 'ALL_DISTINCT' }).Count
        abEqual = @($Classes | Where-Object { $_ -ceq 'A_B_EQUAL' }).Count
        acEqual = @($Classes | Where-Object { $_ -ceq 'A_C_EQUAL' }).Count
        bcEqual = @($Classes | Where-Object { $_ -ceq 'B_C_EQUAL' }).Count
        allEqual = @($Classes | Where-Object { $_ -ceq 'ALL_EQUAL' }).Count
    }
}

function Get-ValueFrequency {
    param(
        [Parameter(Mandatory = $true)][object[]]$Slots,
        [Parameter(Mandatory = $true)][ValidateSet('aHex', 'bHex', 'cHex')]
        [string]$Property
    )
    $counts = @{}
    foreach ($slot in @($Slots)) {
        $value = [string]$slot[$Property]
        if (-not $counts.ContainsKey($value)) { $counts[$value] = 0 }
        $counts[$value]++
    }
    $entries = @($counts.Keys | ForEach-Object {
            [pscustomobject]@{ hex = [string]$_; count = [int]$counts[$_] }
        })
    $sorted = @($entries | Sort-Object `
            @{ Expression = 'count'; Descending = $true },
            @{ Expression = 'hex'; Descending = $false })
    return @($sorted | ForEach-Object {
            [ordered]@{ hex = $_.hex; count = [int]$_.count }
        })
}

function Get-TransformDistinctCounts {
    param(
        [Parameter(Mandatory = $true)][object[]]$Slots,
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][byte[]]$Right
    )
    $xorValues = @{}
    $deltaValues = @{}
    foreach ($slot in @($Slots)) {
        $offset = [int]$slot.offset
        $leftWord = [int](Get-LittleEndianWord -Bytes $Left -Offset $offset)
        $rightWord = [int](Get-LittleEndianWord -Bytes $Right -Offset $offset)
        $xorValues[[string]($leftWord -bxor $rightWord)] = $true
        $deltaValues[[string](($rightWord - $leftWord + 65536) % 65536)] = $true
    }
    return [ordered]@{
        distinctXorValues = [int]$xorValues.Count
        distinctModulo65536LittleEndianDeltas = [int]$deltaValues.Count
    }
}

function New-PairwiseReport {
    param(
        [Parameter(Mandatory = $true)][string]$Left,
        [Parameter(Mandatory = $true)][string]$Right,
        [Parameter(Mandatory = $true)][int[]]$Offsets,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Boundaries
    )
    $runs = @(Get-ContiguousRuns -Offsets $Offsets)
    $owners = @(Get-ChangedOwnerNames -Offsets $Offsets -Boundaries $Boundaries)
    return [ordered]@{
        left = $Left
        right = $Right
        changedBytes = [int]$Offsets.Count
        contiguousRuns = [int]$runs.Count
        runLengthHistogram = @(Get-RunLengthHistogram -Runs $runs)
        changedOwnerCount = [int]$owners.Count
        changedOwners = $owners
        unmappedRuns = 0
        equalLengthIndexed = $true
    }
}

function Get-TriadObservation {
    param(
        [Parameter(Mandatory = $true)][byte[]]$A,
        [Parameter(Mandatory = $true)][byte[]]$B,
        [Parameter(Mandatory = $true)][byte[]]$C,
        [Parameter(Mandatory = $true)][int[]]$AbOffsets,
        [Parameter(Mandatory = $true)][int[]]$AcOffsets,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Boundaries
    )
    if (($A.LongLength -ne $B.LongLength) -or
        ($A.LongLength -ne $C.LongLength)) {
        Throw-TriadBlocker 'triad artifacts do not have equal length.'
    }
    $abSet = New-IntSet -Values $AbOffsets
    $acSet = New-IntSet -Values $AcOffsets
    $unionSet = New-Object 'Collections.Generic.HashSet[int]'
    $unionSet.UnionWith($abSet)
    $unionSet.UnionWith($acSet)
    [int[]]$unionOffsets = @($unionSet | Sort-Object)
    $bcOffsetsList = New-Object Collections.Generic.List[int]
    foreach ($offset in $unionOffsets) {
        if ($B[$offset] -ne $C[$offset]) { [void]$bcOffsetsList.Add($offset) }
    }
    [int[]]$bcOffsets = $bcOffsetsList.ToArray()

    $intersection = New-Object 'Collections.Generic.HashSet[int]'
    $intersection.UnionWith($abSet)
    $intersection.IntersectWith($acSet)
    $abOnly = New-Object 'Collections.Generic.HashSet[int]'
    $abOnly.UnionWith($abSet)
    $abOnly.ExceptWith($acSet)
    $acOnly = New-Object 'Collections.Generic.HashSet[int]'
    $acOnly.UnionWith($acSet)
    $acOnly.ExceptWith($abSet)

    $byteClasses = New-Object Collections.Generic.List[string]
    foreach ($offset in $unionOffsets) {
        if (($A[$offset] -eq $B[$offset]) -and
            ($B[$offset] -eq $C[$offset])) {
            [void]$byteClasses.Add('ALL_EQUAL')
        }
        elseif ($A[$offset] -eq $B[$offset]) {
            [void]$byteClasses.Add('A_B_EQUAL')
        }
        elseif ($A[$offset] -eq $C[$offset]) {
            [void]$byteClasses.Add('A_C_EQUAL')
        }
        elseif ($B[$offset] -eq $C[$offset]) {
            [void]$byteClasses.Add('B_C_EQUAL')
        }
        else {
            [void]$byteClasses.Add('ALL_DISTINCT')
        }
    }

    [int[]]$markerA = Get-MarkerFollowerPositions -Bytes $A
    [int[]]$markerB = Get-MarkerFollowerPositions -Bytes $B
    [int[]]$markerC = Get-MarkerFollowerPositions -Bytes $C
    if ((ConvertTo-DeterministicJson -Value $markerA) -cne
            (ConvertTo-DeterministicJson -Value $markerB) -or
        (ConvertTo-DeterministicJson -Value $markerA) -cne
            (ConvertTo-DeterministicJson -Value $markerC)) {
        Throw-TriadBlocker 'marker follower positions differ across triad artifacts.'
    }

    $markerBases = @{}
    foreach ($offset in $unionOffsets) {
        foreach ($candidateBase in @([int]$offset, [int]($offset - 1))) {
            if ((Test-MarkerBefore -Bytes $A -FollowerOffset $candidateBase) -and
                (Test-MarkerBefore -Bytes $B -FollowerOffset $candidateBase) -and
                (Test-MarkerBefore -Bytes $C -FollowerOffset $candidateBase)) {
                $markerBases[[string]$candidateBase] = $true
            }
        }
    }
    $tailBases = @{}
    foreach ($boundary in $Boundaries.Values) {
        $base = [int]$boundary.recordEndExclusive - 48
        if ($unionSet.Contains($base) -or $unionSet.Contains($base + 1)) {
            $tailBases[[string]$base] = $true
        }
    }
    foreach ($base in $markerBases.Keys) {
        if ($tailBases.ContainsKey($base)) {
            Throw-TriadBlocker "slot base $base belongs to two families."
        }
    }
    [int[]]$slotBases = @(
        @($markerBases.Keys) + @($tailBases.Keys) |
            ForEach-Object { [int]$_ } | Sort-Object -Unique)
    $unionRuns = @(Get-ContiguousRuns -Offsets $unionOffsets)
    foreach ($offset in $unionOffsets) {
        $coverCount = @($slotBases | Where-Object {
                ($_ -le $offset) -and (($_ + 1) -ge $offset)
            }).Count
        if ($coverCount -ne 1) {
            Throw-TriadBlocker "changed offset $offset has $coverCount slot covers."
        }
    }
    foreach ($run in $unionRuns) {
        $runStart = [int]$run.start
        $runEnd = $runStart + [int]$run.bytes - 1
        $coverCount = @($slotBases | Where-Object {
                ($_ -le $runEnd) -and (($_ + 1) -ge $runStart)
            }).Count
        if ($coverCount -ne 1) {
            Throw-TriadBlocker "union run $runStart has $coverCount slot covers."
        }
    }

    $slots = New-Object Collections.Generic.List[object]
    $ordinal = 0
    foreach ($base in $slotBases) {
        $ordinal++
        $owner = Get-OwnerAtOffset -Offset $base -Boundaries $Boundaries
        $family = if ($markerBases.ContainsKey([string]$base)) {
            'MARKER_FOLLOWER_16BIT'
        }
        else {
            'OWNER_END_MINUS_48_16BIT'
        }
        $slots.Add([ordered]@{
                ordinal = $ordinal
                family = $family
                offset = [int]$base
                owner = [string]$owner.owner
                ownerRelativeOffset =
                    [int]($base - [int]$owner.recordEndExclusive)
                aHex = Get-HexRange -Bytes $A -Start $base -Length 2
                bHex = Get-HexRange -Bytes $B -Start $base -Length 2
                cHex = Get-HexRange -Bytes $C -Start $base -Length 2
                equalityClass = Get-WordEqualityClass `
                    -A $A -B $B -C $C -Offset $base
                abChanged = [bool](
                    ($A[$base] -ne $B[$base]) -or
                    ($A[$base + 1] -ne $B[$base + 1]))
                acChanged = [bool](
                    ($A[$base] -ne $C[$base]) -or
                    ($A[$base + 1] -ne $C[$base + 1]))
                bcChanged = [bool](
                    ($B[$base] -ne $C[$base]) -or
                    ($B[$base + 1] -ne $C[$base + 1]))
            })
    }
    $slotArray = $slots.ToArray()
    $markerSlots = @($slotArray | Where-Object {
            $_.family -ceq 'MARKER_FOLLOWER_16BIT'
        })
    $tailSlots = @($slotArray | Where-Object {
            $_.family -ceq 'OWNER_END_MINUS_48_16BIT'
        })
    $stableMarkerFollowers = @($markerA | Where-Object {
            -not $markerBases.ContainsKey([string]$_)
        })
    $candidateKinds = @{}
    foreach ($base in $markerA) {
        $candidateKinds[[string]$base] = 'MARKER_FOLLOWER_16BIT'
    }
    foreach ($boundary in $Boundaries.Values) {
        $base = [int]$boundary.recordEndExclusive - 48
        if ($candidateKinds.ContainsKey([string]$base)) {
            Throw-TriadBlocker "full structural candidate $base belongs to two families."
        }
        $candidateKinds[[string]$base] = 'OWNER_END_MINUS_48_16BIT'
    }
    $volatileSet = New-IntSet -Values $slotBases
    $candidateTable = New-Object Collections.Generic.List[object]
    $candidateOrdinal = 0
    foreach ($base in @($candidateKinds.Keys | ForEach-Object {
                [int]$_
            } | Sort-Object)) {
        $candidateOrdinal++
        $owner = Get-OwnerAtOffset -Offset $base -Boundaries $Boundaries
        $candidateTable.Add([ordered]@{
                ordinal = $candidateOrdinal
                family = [string]$candidateKinds[[string]$base]
                offset = [int]$base
                owner = [string]$owner.owner
                ownerRelativeOffset =
                    [int]($base - [int]$owner.recordEndExclusive)
                aHex = Get-HexRange -Bytes $A -Start $base -Length 2
                bHex = Get-HexRange -Bytes $B -Start $base -Length 2
                cHex = Get-HexRange -Bytes $C -Start $base -Length 2
                equalityClass = Get-WordEqualityClass `
                    -A $A -B $B -C $C -Offset $base
                volatileObserved = [bool]$volatileSet.Contains($base)
            })
    }
    $candidateArray = $candidateTable.ToArray()
    $candidateJson = ConvertTo-DeterministicJson -Value $candidateArray
    [byte[]]$candidateBytes = Get-DeterministicJsonBytes -Json $candidateJson
    $slotJson = ConvertTo-DeterministicJson -Value $slotArray
    [byte[]]$slotBytes = Get-DeterministicJsonBytes -Json $slotJson
    $commonSame = @($intersection | Where-Object {
            $B[$_] -eq $C[$_]
        }).Count

    return [ordered]@{
        pairwise = @(
            (New-PairwiseReport -Left 'A_24402BFA' -Right 'B_6E115876' `
                -Offsets $AbOffsets -Boundaries $Boundaries),
            (New-PairwiseReport -Left 'A_24402BFA' -Right 'C_99014DD9' `
                -Offsets $AcOffsets -Boundaries $Boundaries),
            (New-PairwiseReport -Left 'B_6E115876' -Right 'C_99014DD9' `
                -Offsets $bcOffsets -Boundaries $Boundaries))
        changedOffsetRelations = [ordered]@{
            unionBytes = [int]$unionSet.Count
            abAcIntersectionBytes = [int]$intersection.Count
            abOnlyBytes = [int]$abOnly.Count
            acOnlyBytes = [int]$acOnly.Count
            commonOffsetsWithSameBAndC = [int]$commonSame
            commonOffsetsWithDifferentBAndC =
                [int]($intersection.Count - $commonSame)
            byteEquality = Get-EqualityCounts -Classes $byteClasses.ToArray()
        }
        slotModel = [ordered]@{
            structuralCandidateUniverse = [ordered]@{
                markerFollowerCandidates = [int]$markerA.Count
                ownerEndMinus48Candidates = [int]$Boundaries.Count
                totalCandidates = [int]$candidateArray.Count
                volatileObservedCandidates = [int]$slotArray.Count
                stableObservedCandidates =
                    [int]($candidateArray.Count - $slotArray.Count)
                candidateTableSha256 = Get-BytesSha256 -Bytes $candidateBytes
            }
            markerHex = $MarkerHex
            markerOccurrenceCount = [int]$markerA.Count
            markerFollowerPositionsEqual = $true
            volatileMarkerFollowerSlots = [int]$markerSlots.Count
            stableMarkerFollowerSlots = [int]$stableMarkerFollowers.Count
            ownerEndMinus48Slots = [int]$tailSlots.Count
            totalVolatileSlots = [int]$slotArray.Count
            changedOffsetCoverage = 'EXACTLY_ONE_SLOT'
            uncoveredChangedOffsets = 0
            multiplyCoveredChangedOffsets = 0
            unionContiguousRuns = [int]$unionRuns.Count
            unionRunLengthHistogram = @(Get-RunLengthHistogram -Runs $unionRuns)
            slotEquality = Get-EqualityCounts -Classes @(
                $slotArray | ForEach-Object { [string]$_.equalityClass })
            markerFollowerValueFrequency = [ordered]@{
                a = @(Get-ValueFrequency -Slots $markerSlots -Property aHex)
                b = @(Get-ValueFrequency -Slots $markerSlots -Property bHex)
                c = @(Get-ValueFrequency -Slots $markerSlots -Property cHex)
            }
            tailUniqueWordCount = [ordered]@{
                a = @($tailSlots.aHex | Sort-Object -Unique).Count
                b = @($tailSlots.bHex | Sort-Object -Unique).Count
                c = @($tailSlots.cHex | Sort-Object -Unique).Count
            }
            transforms = [ordered]@{
                ab = Get-TransformDistinctCounts -Slots $slotArray -Left $A -Right $B
                ac = Get-TransformDistinctCounts -Slots $slotArray -Left $A -Right $C
                bc = Get-TransformDistinctCounts -Slots $slotArray -Left $B -Right $C
            }
            volatileSlotTableSha256 = Get-BytesSha256 -Bytes $slotBytes
            slots = $slotArray
        }
        internal = [pscustomobject]@{
            AbOffsets = $AbOffsets
            AcOffsets = $AcOffsets
            BcOffsets = $bcOffsets
            SlotArray = $slotArray
            MarkerSlots = $markerSlots
            TailSlots = $tailSlots
        }
    }
}

function Test-HistogramExact {
    param(
        [Parameter(Mandatory = $true)][object[]]$Actual,
        [Parameter(Mandatory = $true)][int]$OneByteRuns,
        [Parameter(Mandatory = $true)][int]$TwoByteRuns
    )
    if ($Actual.Count -ne 2) { return $false }
    return ([int]$Actual[0].bytes -eq 1) -and
        ([int]$Actual[0].count -eq $OneByteRuns) -and
        ([int]$Actual[1].bytes -eq 2) -and
        ([int]$Actual[1].count -eq $TwoByteRuns)
}

function Test-GoldenObservation {
    param([Parameter(Mandatory = $true)]$Observation)
    $pairs = @($Observation.pairwise)
    if (($pairs.Count -ne 3) -or
        ([int]$pairs[0].changedBytes -ne 99) -or
        ([int]$pairs[0].contiguousRuns -ne 58) -or
        ([int]$pairs[0].changedOwnerCount -ne 36) -or
        (-not (Test-HistogramExact `
                -Actual @($pairs[0].runLengthHistogram) `
                -OneByteRuns 17 -TwoByteRuns 41)) -or
        ([int]$pairs[1].changedBytes -ne 96) -or
        ([int]$pairs[1].contiguousRuns -ne 52) -or
        ([int]$pairs[1].changedOwnerCount -ne 34) -or
        (-not (Test-HistogramExact `
                -Actual @($pairs[1].runLengthHistogram) `
                -OneByteRuns 8 -TwoByteRuns 44)) -or
        ([int]$pairs[2].changedBytes -ne 105) -or
        ([int]$pairs[2].contiguousRuns -ne 61) -or
        ([int]$pairs[2].changedOwnerCount -ne 36) -or
        (-not (Test-HistogramExact `
                -Actual @($pairs[2].runLengthHistogram) `
                -OneByteRuns 17 -TwoByteRuns 44))) {
        return $false
    }
    if (($pairs[1].changedOwners -ccontains '_LMCAxisVis') -or
        ($pairs[1].changedOwners -ccontains '_LMCAxisVisLogViewer') -or
        ($pairs[0].changedOwners -cnotcontains '_LMCAxisVis') -or
        ($pairs[2].changedOwners -cnotcontains '_LMCAxisVisLogViewer')) {
        return $false
    }
    $relations = $Observation.changedOffsetRelations
    $byteEquality = $relations.byteEquality
    if (([int]$relations.unionBytes -ne 124) -or
        ([int]$relations.abAcIntersectionBytes -ne 71) -or
        ([int]$relations.abOnlyBytes -ne 28) -or
        ([int]$relations.acOnlyBytes -ne 25) -or
        ([int]$relations.commonOffsetsWithSameBAndC -ne 19) -or
        ([int]$relations.commonOffsetsWithDifferentBAndC -ne 52) -or
        ([int]$byteEquality.allDistinct -ne 52) -or
        ([int]$byteEquality.abEqual -ne 25) -or
        ([int]$byteEquality.acEqual -ne 28) -or
        ([int]$byteEquality.bcEqual -ne 19) -or
        ([int]$byteEquality.allEqual -ne 0)) {
        return $false
    }
    $model = $Observation.slotModel
    $slotEquality = $model.slotEquality
    $universe = $model.structuralCandidateUniverse
    if (([int]$universe.markerFollowerCandidates -ne 37) -or
        ([int]$universe.ownerEndMinus48Candidates -ne 120) -or
        ([int]$universe.totalCandidates -ne 157) -or
        ([int]$universe.volatileObservedCandidates -ne 66) -or
        ([int]$universe.stableObservedCandidates -ne 91) -or
        ([string]$universe.candidateTableSha256 -cne
            $ExpectedCandidateTableSha256) -or
        ($model.markerHex -cne $MarkerHex) -or
        ([int]$model.markerOccurrenceCount -ne 37) -or
        (-not [bool]$model.markerFollowerPositionsEqual) -or
        ([int]$model.volatileMarkerFollowerSlots -ne 35) -or
        ([int]$model.stableMarkerFollowerSlots -ne 2) -or
        ([int]$model.ownerEndMinus48Slots -ne 31) -or
        ([int]$model.totalVolatileSlots -ne 66) -or
        ($model.changedOffsetCoverage -cne 'EXACTLY_ONE_SLOT') -or
        ([int]$model.uncoveredChangedOffsets -ne 0) -or
        ([int]$model.multiplyCoveredChangedOffsets -ne 0) -or
        ([int]$model.unionContiguousRuns -ne 66) -or
        (-not (Test-HistogramExact `
                -Actual @($model.unionRunLengthHistogram) `
                -OneByteRuns 8 -TwoByteRuns 58)) -or
        ([int]$slotEquality.allDistinct -ne 39) -or
        ([int]$slotEquality.abEqual -ne 8) -or
        ([int]$slotEquality.acEqual -ne 14) -or
        ([int]$slotEquality.bcEqual -ne 5) -or
        ([int]$slotEquality.allEqual -ne 0) -or
        ([int]$model.tailUniqueWordCount.a -ne 31) -or
        ([int]$model.tailUniqueWordCount.b -ne 31) -or
        ([int]$model.tailUniqueWordCount.c -ne 31) -or
        ([string]$model.volatileSlotTableSha256 -cne
            $ExpectedVolatileSlotTableSha256) -or
        (@($model.slots).Count -ne 66)) {
        return $false
    }
    if (([int]$model.transforms.ab.distinctXorValues -ne 34) -or
        ([int]$model.transforms.ab.distinctModulo65536LittleEndianDeltas -ne 40) -or
        ([int]$model.transforms.ac.distinctXorValues -ne 30) -or
        ([int]$model.transforms.ac.distinctModulo65536LittleEndianDeltas -ne 32) -or
        ([int]$model.transforms.bc.distinctXorValues -ne 39) -or
        ([int]$model.transforms.bc.distinctModulo65536LittleEndianDeltas -ne 43)) {
        return $false
    }
    $expectedA = @(
        [ordered]@{ hex = '9FE9'; count = 24 },
        [ordered]@{ hex = '0000'; count = 10 },
        [ordered]@{ hex = 'FDE9'; count = 1 })
    $expectedB = @(
        [ordered]@{ hex = '6200'; count = 10 },
        [ordered]@{ hex = 'FDE9'; count = 7 },
        [ordered]@{ hex = 'E623'; count = 5 },
        [ordered]@{ hex = '0000'; count = 4 },
        [ordered]@{ hex = '9FE9'; count = 3 },
        [ordered]@{ hex = 'C3E9'; count = 2 },
        [ordered]@{ hex = 'FAE9'; count = 2 },
        [ordered]@{ hex = '6500'; count = 1 },
        [ordered]@{ hex = '79CA'; count = 1 })
    $expectedC = @(
        [ordered]@{ hex = '5C00'; count = 21 },
        [ordered]@{ hex = '9FE9'; count = 12 },
        [ordered]@{ hex = '0000'; count = 1 },
        [ordered]@{ hex = 'C3E9'; count = 1 })
    if ((ConvertTo-DeterministicJson -Value $expectedA) -cne
            (ConvertTo-DeterministicJson `
                -Value $model.markerFollowerValueFrequency.a) -or
        (ConvertTo-DeterministicJson -Value $expectedB) -cne
            (ConvertTo-DeterministicJson `
                -Value $model.markerFollowerValueFrequency.b) -or
        (ConvertTo-DeterministicJson -Value $expectedC) -cne
            (ConvertTo-DeterministicJson `
                -Value $model.markerFollowerValueFrequency.c)) {
        return $false
    }
    return $true
}

function Get-ExactSlot {
    param(
        [Parameter(Mandatory = $true)][object[]]$Slots,
        [Parameter(Mandatory = $true)][string]$OwnerName,
        [Parameter(Mandatory = $true)][string]$Family
    )
    $matches = @($Slots | Where-Object {
            ([string]$_.owner -ceq $OwnerName) -and
            ([string]$_.family -ceq $Family)
        })
    if ($matches.Count -ne 1) {
        Throw-TriadBlocker "$OwnerName $Family does not select one slot."
    }
    return $matches[0]
}

function Get-ChecksumCounterexamples {
    param([Parameter(Mandatory = $true)][object[]]$Slots)
    $sameContentTailChanged = New-Object Collections.Generic.List[object]
    foreach ($name in @('_LMCMath_SO3', 'SigCLib')) {
        $marker = Get-ExactSlot -Slots $Slots -OwnerName $name `
            -Family 'MARKER_FOLLOWER_16BIT'
        $tail = Get-ExactSlot -Slots $Slots -OwnerName $name `
            -Family 'OWNER_END_MINUS_48_16BIT'
        if (($marker.aHex -cne $marker.bHex) -or
            ($tail.aHex -ceq $tail.bHex)) {
            Throw-TriadBlocker "$name no longer supplies the A/B checksum counterexample."
        }
        $sameContentTailChanged.Add([ordered]@{
                owner = $name
                comparison = 'A_B'
                markerFollowerHex = [string]$marker.aHex
                tailAHex = [string]$tail.aHex
                tailBHex = [string]$tail.bHex
            })
    }
    $changedContentTailSame = New-Object Collections.Generic.List[object]
    foreach ($name in @(
            '_LMCProfileBuffer', '_LMCSplineBuffer',
            '_LMCTool', 'MoveSplineTable')) {
        $marker = Get-ExactSlot -Slots $Slots -OwnerName $name `
            -Family 'MARKER_FOLLOWER_16BIT'
        $tail = Get-ExactSlot -Slots $Slots -OwnerName $name `
            -Family 'OWNER_END_MINUS_48_16BIT'
        if (($marker.bHex -ceq $marker.cHex) -or
            ($tail.bHex -cne $tail.cHex)) {
            Throw-TriadBlocker "$name no longer supplies the B/C checksum counterexample."
        }
        $changedContentTailSame.Add([ordered]@{
                owner = $name
                comparison = 'B_C'
                markerBHex = [string]$marker.bHex
                markerCHex = [string]$marker.cHex
                commonTailHex = [string]$tail.bHex
            })
    }
    return [ordered]@{
        sameOtherObservedSlotTailChanged = $sameContentTailChanged.ToArray()
        changedMarkerFollowerTailSame = $changedContentTailSame.ToArray()
        contentOnlyTailChecksumConsistent = $false
    }
}

function Get-PublicProvenanceReport {
    param([Parameter(Mandatory = $true)][Collections.IDictionary]$Resolved)
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

function Get-ImplicitInputProvenanceMatrix {
    param(
        [Parameter(Mandatory = $true)]$CheckpointBaseline,
        [Parameter(Mandatory = $true)]$ThirdBaseline
    )
    $topKeys = @(
        'Schema', 'EvidenceProfile', 'CapturedAtUtc', 'RepositoryRoot',
        'CanonicalProjectPath', 'LasalLogPath', 'LogPrefixLength',
        'LogPrefixSha256', 'RequiredCompileRelativePaths', 'Files')
    $binaryFileKeys = @('RelativePath', 'Role', 'Sha256')
    $textFileKeys = @(
        'RelativePath', 'Role', 'Sha256', 'RawBytes', 'RawSha256',
        'CanonicalLfBytes', 'CanonicalLfSha256', 'EolStyle', 'CrLfCount',
        'LfOnlyCount', 'CrOnlyCount', 'LineBreakCount')
    foreach ($item in @(
            [ordered]@{ value = $CheckpointBaseline; owner = 'checkpoint baseline' },
            [ordered]@{ value = $ThirdBaseline; owner = 'third baseline' })) {
        $baseline = $item.value
        $owner = [string]$item.owner
        Assert-ExactJsonKeys -Object $baseline -Expected $topKeys `
            -ObjectOwner $owner
        if (($baseline.Schema -cne 'LasalC78RebuildEvidence/v1') -or
            ($baseline.EvidenceProfile -cne 'GateDVisualLayout') -or
            (@($baseline.RequiredCompileRelativePaths).Count -ne 5) -or
            (@($baseline.Files).Count -ne 12)) {
            Throw-TriadBlocker "$owner contract differs."
        }
        $paths = @{}
        foreach ($file in @($baseline.Files)) {
            $relativePath = [string]$file.RelativePath
            $expectedFileKeys = if ($relativePath.EndsWith(
                    '.st', [StringComparison]::Ordinal)) {
                $textFileKeys
            }
            else {
                $binaryFileKeys
            }
            Assert-ExactJsonKeys -Object $file -Expected $expectedFileKeys `
                -ObjectOwner "$owner.Files[]"
            if ([string]::IsNullOrWhiteSpace($relativePath) -or
                ($relativePath.IndexOf('\') -ge 0) -or
                ($file.Role -cnotin @(
                        'inputIdentity', 'expectedRegeneratedOutput')) -or
                ([string]$file.Sha256 -cnotmatch '^[0-9a-f]{64}$') -or
                $paths.ContainsKey($relativePath)) {
                Throw-TriadBlocker "$owner file identity shape differs."
            }
            $paths[$relativePath] = $true
        }
        if ((@($baseline.Files | Where-Object {
                        $_.Role -ceq 'inputIdentity'
                    }).Count -ne 10) -or
            (@($baseline.Files | Where-Object {
                        $_.Role -ceq 'expectedRegeneratedOutput'
                    }).Count -ne 2)) {
            Throw-TriadBlocker "$owner role inventory differs."
        }
    }
    $checkpointInputs = @($CheckpointBaseline.Files | Where-Object {
            $_.Role -ceq 'inputIdentity'
        })
    $thirdInputs = @($ThirdBaseline.Files | Where-Object {
            $_.Role -ceq 'inputIdentity'
        })
    $thirdByPath = @{}
    foreach ($file in $thirdInputs) {
        $thirdByPath[[string]$file.RelativePath] = $file
    }
    $exact = 0
    foreach ($file in $checkpointInputs) {
        $path = [string]$file.RelativePath
        if ($thirdByPath.ContainsKey($path) -and
            ((ConvertTo-DeterministicJson -Value $file) -ceq
                (ConvertTo-DeterministicJson -Value $thirdByPath[$path]))) {
            $exact++
        }
    }
    if (($checkpointInputs.Count -ne 10) -or
        ($thirdInputs.Count -ne 10) -or ($exact -ne 10)) {
        Throw-TriadBlocker 'pinned baseline inputIdentity entries are not exact 10/10.'
    }
    return [ordered]@{
        observationScope = 'pinned-historical-triad-only'
        checkpointBaselineArtifactRole = 'checkpointBuildBaseline'
        thirdBaselineArtifactRole = 'thirdBuildBaseline'
        explicitScopedInputIdentity = [ordered]@{
            evidenceSelector = 'Files.Role=inputIdentity'
            equivalence = 'EXACT'
            checkpointEntries = 10
            thirdEntries = 10
            exactEntryMatches = 10
        }
        expectedRegeneratedOutputs = [ordered]@{
            evidenceSelector = 'Files.Role=expectedRegeneratedOutput'
            checkpointEntries = 2
            thirdEntries = 2
            equivalence = 'EXCLUDED_FROM_INPUT_EQUIVALENCE_DECISION'
        }
        matrix = @(
            [ordered]@{
                generatorInput = 'EXPLICIT_SCOPED_INPUT_IDENTITY_FILES'
                equivalence = 'EXACT'
                evidence = 'PINNED_BUILD_BASELINES_10_OF_10'
            },
            [ordered]@{
                generatorInput = 'LASAL_EXECUTABLE'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            },
            [ordered]@{
                generatorInput = 'LASAL_COMPILER'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            },
            [ordered]@{
                generatorInput = 'VENDOR_LIBRARY_SET'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            },
            [ordered]@{
                generatorInput = 'GENERATOR_CACHE_STATE'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            },
            [ordered]@{
                generatorInput = 'FILESYSTEM_TIMESTAMPS'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            },
            [ordered]@{
                generatorInput = 'PROCESS_SESSION_STATE'
                equivalence = 'UNPROVEN'
                evidence = 'NOT_RECORDED'
            })
        allGeneratorInputsEquivalent = $false
    }
}

function New-TriadReport {
    param(
        [Parameter(Mandatory = $true)][object[]]$Provenance,
        [Parameter(Mandatory = $true)]$ProducerIdentity,
        [Parameter(Mandatory = $true)]$Observation,
        [Parameter(Mandatory = $true)]$ImplicitInputProvenance,
        [Parameter(Mandatory = $true)][bool]$PatchOracleReconstructionExact,
        [Parameter(Mandatory = $true)][bool]$ThirdOracleSnapshotExact
    )
    $golden = Test-GoldenObservation -Observation $Observation
    $contractMatched = $golden -and $PatchOracleReconstructionExact -and
        $ThirdOracleSnapshotExact
    $disposition = if ($contractMatched) {
        'CONFIRMED_PINNED_TRIAD_FIXED_16BIT_SLOT_PATTERN_REVIEW_ONLY'
    }
    else {
        'REJECTED_TRIAD_STRUCTURAL_CONTRACT_MISMATCH'
    }
    $exitCode = if ($contractMatched) { 2 } else { 3 }
    $counterexamples = Get-ChecksumCounterexamples `
        -Slots @($Observation.slotModel.slots)
    return [ordered]@{
        schema = $Schema
        tool = [ordered]@{
            owner = $Owner
            supportedProductionInvocation = 'pwsh -File'
            outputPublicationTrustBoundary = 'NON_ADVERSARIAL_WORKSPACE'
            handleRelativeCreationUsed = $false
            concurrentParentReplacementResistance = $false
        }
        decision = [ordered]@{
            disposition = $disposition
            exitCode = [int]$exitCode
            toolCompleted = $true
            evidenceContractSatisfied = [bool]$contractMatched
            analysisScope = 'pinned-historical-triad-only'
            productionApproved = $false
            semanticEquivalenceProven = $false
            rebaselinePermitted = $false
            downloadPermitted = $false
            runtimeQualificationPermitted = $false
            futureArtifactAcceptancePermitted = $false
            normalizationUsedForDecision = $false
            requiresReviewedTransition = $true
        }
        inputProvenance = [ordered]@{
            sourcePolicy = 'PINNED_COMMIT_PATH_BLOB_ONLY'
            producer = $ProducerIdentity
            mutableWorktreeClassesRead = $false
            localKnown6eObjectRequired = $false
            known6eCaptureContextHead = $KnownRebuildCaptureHead
            thirdCaptureContextHead = $ThirdCaptureHead
            thirdBundlePublicationCommit = $ThirdCommit
            artifacts = $Provenance
            implicitInputProvenanceMatrix = $ImplicitInputProvenance
        }
        reconstruction = [ordered]@{
            known6e = [ordered]@{
                method = 'IN_MEMORY_FULL_COMMITTED_ORACLE_DELTA'
                fullNonTruncatedPreviewCoverage = $true
                binaryPatchApplied = $false
                binaryPatchRole =
                    'PINNED_HISTORICAL_PRESERVATION_EVIDENCE_NOT_REAPPLIED'
                reconstructedRawBytes = $KnownRebuildBytes
                reconstructedSha256 = $KnownRebuildSha256
                reconstructedGitBlobOid = $KnownRebuildBlobOid
                oracleAndPatchManifestIdentityExact =
                    [bool]$PatchOracleReconstructionExact
            }
            third990 = [ordered]@{
                method = 'IN_MEMORY_FULL_COMMITTED_ORACLE_DELTA'
                fullNonTruncatedPreviewCoverage = $true
                reconstructedRawBytes = $ThirdBytes
                reconstructedSha256 = $ThirdSha256
                reconstructedGitBlobOid = $ThirdBlobOid
                oracleAndCommittedBundleSnapshotExact =
                    [bool]$ThirdOracleSnapshotExact
            }
        }
        pairwise = $Observation.pairwise
        changedOffsetRelations = $Observation.changedOffsetRelations
        slotModel = $Observation.slotModel
        counterexamples = $counterexamples
        semanticBoundary = [ordered]@{
            structuralFinding = 'TWO_FIXED_16BIT_SLOT_FAMILIES'
            fixedSlotStructureProven = [bool]$contractMatched
            contentOnlyTailChecksumConsistent = $false
            simpleGlobalCounterConsistent = $false
            simpleFixedXorConsistent = $false
            timestampProven = $false
            pointerOrHandleProven = $false
            fieldMeaning = 'UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT'
            leadingHypothesis = 'NONE_PROVEN'
            hypothesisConfidence = 'UNCLASSIFIED'
            proprietaryFieldSemanticsDecoded = $false
            semanticEquivalenceProven = $false
            productionApproved = $false
            rebaselinePermitted = $false
            downloadPermitted = $false
            runtimeQualificationPermitted = $false
            futureArtifactAcceptancePermitted = $false
            normalizationUsedForDecision = $false
            requiresReviewedTransition = $true
        }
    }
}

function Invoke-PinnedTriadAnalysis {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)]$ProducerIdentity
    )
    $resolved = @{}
    $publicProvenance = New-Object Collections.Generic.List[object]
    foreach ($definition in $PinnedInputs) {
        $artifact = Resolve-PinnedInput -Root $Root -Definition $definition
        $resolved[[string]$artifact.role] = $artifact
        $publicProvenance.Add((Get-PublicProvenanceReport -Resolved $artifact))
    }
    [byte[]]$checkpoint = $resolved['checkpoint244'].bytes
    $checkpointBaseline = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['checkpointBuildBaseline'].bytes `
        -JsonOwner 'checkpoint build baseline'
    [byte[]]$patch = $resolved['known6eBinaryPatch'].bytes
    $patchManifest = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['known6ePatchManifest'].bytes `
        -JsonOwner 'known 6E patch manifest'
    $knownOracle = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['known6eComparisonOracle'].bytes `
        -JsonOwner 'known 6E comparison oracle' `
        -RequireComparatorCanonical
    [byte[]]$thirdSnapshot = $resolved['third990BundleSnapshot'].bytes
    $thirdBaseline = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['thirdBuildBaseline'].bytes `
        -JsonOwner 'third build baseline'
    $thirdOracle = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['third990ComparisonOracle'].bytes `
        -JsonOwner 'third 990 comparison oracle' `
        -RequireComparatorCanonical
    $thirdManifest = ConvertFrom-StrictJsonBytes `
        -Bytes $resolved['third990FinalizationManifest'].bytes `
        -JsonOwner 'third 990 finalization manifest'

    Assert-PatchContract -PatchBytes $patch -Manifest $patchManifest
    Assert-OracleInvariantSections `
        -KnownOracle $knownOracle -ThirdOracle $thirdOracle
    Assert-ThirdManifestContract `
        -Manifest $thirdManifest -ThirdOracle $thirdOracle
    $implicitInputProvenance = Get-ImplicitInputProvenanceMatrix `
        -CheckpointBaseline $checkpointBaseline -ThirdBaseline $thirdBaseline
    $known = Get-OracleReconstruction `
        -Oracle $knownOracle -CheckpointArtifact $checkpoint `
        -OracleOwner 'known 6E comparison oracle' `
        -ExpectedCandidateBytes $KnownRebuildBytes `
        -ExpectedCandidateSha256 $KnownRebuildSha256 `
        -ExpectedCandidateBlobOid $KnownRebuildBlobOid `
        -ExpectedChangedBytes 99 -ExpectedRunCount 58 -ExpectedOwnerCount 36
    $third = Get-OracleReconstruction `
        -Oracle $thirdOracle -CheckpointArtifact $checkpoint `
        -OracleOwner 'third 990 comparison oracle' `
        -ExpectedCandidateBytes $ThirdBytes `
        -ExpectedCandidateSha256 $ThirdSha256 `
        -ExpectedCandidateBlobOid $ThirdBlobOid `
        -ExpectedChangedBytes 96 -ExpectedRunCount 52 -ExpectedOwnerCount 34
    $thirdExact = Test-ByteSequencesExact `
        -Left $third.CandidateBytes -Right $thirdSnapshot
    if (-not $thirdExact) {
        Throw-TriadBlocker 'third oracle reconstruction differs from bundle snapshot.'
    }
    $inventoryA = Get-FullOwnerBoundaries `
        -Bytes $checkpoint -ArtifactOwner 'checkpoint A'
    $inventoryB = Get-FullOwnerBoundaries `
        -Bytes $known.CandidateBytes -ArtifactOwner 'known rebuild B'
    $inventoryC = Get-FullOwnerBoundaries `
        -Bytes $thirdSnapshot -ArtifactOwner 'third bundle C'
    Assert-OwnerInventoriesExact `
        -A $inventoryA -B $inventoryB -C $inventoryC `
        -KnownOracle $known.OwnerBoundaries `
        -ThirdOracle $third.OwnerBoundaries
    $boundaries = $inventoryA
    $observation = Get-TriadObservation `
        -A $checkpoint -B $known.CandidateBytes -C $thirdSnapshot `
        -AbOffsets $known.ChangedOffsets -AcOffsets $third.ChangedOffsets `
        -Boundaries $boundaries
    return New-TriadReport `
        -Provenance $publicProvenance.ToArray() `
        -ProducerIdentity $ProducerIdentity `
        -Observation $observation `
        -ImplicitInputProvenance $implicitInputProvenance `
        -PatchOracleReconstructionExact $true `
        -ThirdOracleSnapshotExact $thirdExact
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
    try { & $Action }
    catch {
        $threw = $true
        if ((-not [string]::IsNullOrWhiteSpace($ExpectedText)) -and
            ($_.Exception.Message.IndexOf(
                    $ExpectedText, [StringComparison]::Ordinal) -lt 0)) {
            throw "SELFTEST: $Message threw unexpected text: $($_.Exception.Message)"
        }
    }
    if (-not $threw) { throw "SELFTEST: $Message did not throw." }
}

function Get-JsonSelfTestFixture {
    $nonAscii = [string]([char]0xD55C) + [string]([char]0xAE00)
    return [ordered]@{
        schema = 'LasalClassesVolatilityTriadJsonSelfTest/v1'
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
    return [ordered]@{
        ps5 = $ps5.Source
        ps7 = $ps7.Source
    }
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
        if (-not $process.Start()) { throw 'SELFTEST: child host did not start.' }
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
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][string]$TempRoot
    )
    $fullDirectory = [IO.Path]::GetFullPath($Directory).TrimEnd('\')
    $fullTemp = [IO.Path]::GetFullPath($TempRoot).TrimEnd('\')
    if ((-not $fullDirectory.StartsWith(
                $fullTemp + '\', [StringComparison]::OrdinalIgnoreCase)) -or
        (-not (Split-Path -Leaf $fullDirectory).StartsWith(
                'LasalClassesTriadSelfTest-', [StringComparison]::Ordinal))) {
        throw 'SELFTEST: cleanup target is unsafe.'
    }
    if ([IO.Directory]::Exists($fullDirectory)) {
        [IO.Directory]::Delete($fullDirectory, $true)
    }
}

function Invoke-PowerShell5SelfTest {
    $positive = 0
    $negative = 0
    $hosts = Get-PowerShellHosts
    $fixturePs5 = Invoke-ScriptHost `
        -HostPath $hosts.ps5 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    $fixturePs7 = Invoke-ScriptHost `
        -HostPath $hosts.ps7 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    Assert-SelfTestTrue `
        -Condition (
            $fixturePs5.ExitCode -eq 0 -and
            $fixturePs7.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($fixturePs5.Stderr) -and
            [string]::IsNullOrWhiteSpace($fixturePs7.Stderr) -and
            (Test-ByteSequencesExact `
                -Left $fixturePs5.StdoutBytes -Right $fixturePs7.StdoutBytes)) `
        -Message 'PowerShell 5/7 canonical fixture bytes differ.'
    $positive++

    $scalar = @(Get-ExactScalarProcessExitCode `
            -Value ([int]2) -ValueOwner 'PowerShell 5 self-test exit')
    Assert-SelfTestTrue `
        -Condition (
            $scalar.Count -eq 1 -and $scalar[0] -is [int] -and
            $scalar[0] -eq 2) `
        -Message 'PowerShell 5 scalar exit 2 differs.'
    Assert-SelfTestThrows `
        -Action {
            Get-ExactScalarProcessExitCode `
                -Value @([int]2, 'status') `
                -ValueOwner 'PowerShell 5 contaminated exit'
        } `
        -Message 'PowerShell 5 pipeline array exit rejection' `
        -ExpectedText 'must be exactly one System.Int32'
    $positive++; $negative++

    $productionPs5 = Invoke-ScriptHost `
        -HostPath $hosts.ps5 -ModeArgument '-AnalyzePinnedTriad'
    $invalidRoot = Join-Path ([IO.Path]::GetTempPath()) (
        'LasalClassesTriadMissing-' + [Guid]::NewGuid().ToString('N'))
    $productionPs5InvalidRoot = Invoke-ScriptHost `
        -HostPath $hosts.ps5 `
        -ModeArgument (
            '-AnalyzePinnedTriad -RepositoryRoot "' + $invalidRoot + '"')
    $blocked = $Utf8Strict.GetString(
        $productionPs5.StdoutBytes) | ConvertFrom-Json
    Assert-ExactJsonKeys -Object $blocked.tool `
        -ObjectOwner 'PowerShell 5 blocked report.tool' -Expected @(
            'owner', 'supportedProductionInvocation',
            'outputPublicationTrustBoundary', 'handleRelativeCreationUsed',
            'concurrentParentReplacementResistance')
    Assert-SelfTestTrue `
        -Condition (
            $productionPs5.ExitCode -eq 4 -and
            $productionPs5InvalidRoot.ExitCode -eq 4 -and
            [string]::IsNullOrWhiteSpace($productionPs5.Stderr) -and
            [string]::IsNullOrWhiteSpace($productionPs5InvalidRoot.Stderr) -and
            (Test-ByteSequencesExact `
                -Left $productionPs5.StdoutBytes `
                -Right $productionPs5InvalidRoot.StdoutBytes) -and
            $blocked.tool.outputPublicationTrustBoundary -ceq
                'NON_ADVERSARIAL_WORKSPACE' -and
            (-not $blocked.tool.handleRelativeCreationUsed) -and
            (-not $blocked.tool.concurrentParentReplacementResistance) -and
            $blocked.decision.disposition -ceq
                'BLOCKED_INVALID_OR_UNTRUSTED_INPUT') `
        -Message 'PowerShell 5 production early gate differs.'
    $negative++

    $core = Invoke-ScriptHost `
        -HostPath $hosts.ps7 -ModeArgument '-RunSelfTest'
    $coreText = $Utf8Strict.GetString($core.StdoutBytes)
    Assert-SelfTestTrue `
        -Condition (
            $core.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($core.Stderr) -and
            $coreText -cmatch
                '^PASS LasalClassesVolatilityTriad\.SelfTest ' +
                'Positive=7 Negative=16\r?\n$') `
        -Message 'PowerShell 7 delegated core self-test differs.'
    $positive++

    [Console]::Out.WriteLine(
        "PASS LasalClassesVolatilityTriad.SelfTest.PS5 " +
        "Positive=$positive Negative=$negative DelegatedCore=PS7")
}

function Invoke-SelfTest {
    param([Parameter(Mandatory = $true)][string]$Root)
    if (($PSVersionTable.PSEdition -ceq 'Desktop') -and
        ($PSVersionTable.PSVersion.Major -eq 5)) {
        Invoke-PowerShell5SelfTest
        return
    }
    $positive = 0
    $negative = 0
    $producerReady = $true
    try {
        $producerIdentity = Resolve-ProducerIdentity -Root $Root
    }
    catch {
        $producerReady = $false
        $producerIdentity = Get-SelfTestProducerIdentity
    }
    $report = Invoke-PinnedTriadAnalysis `
        -Root $Root -ProducerIdentity $producerIdentity
    Assert-SelfTestTrue `
        -Condition (
            $report.schema -ceq $Schema -and
            $report.tool.supportedProductionInvocation -ceq 'pwsh -File' -and
            $report.tool.outputPublicationTrustBoundary -ceq
                'NON_ADVERSARIAL_WORKSPACE' -and
            (-not $report.tool.handleRelativeCreationUsed) -and
            (-not $report.tool.concurrentParentReplacementResistance) -and
            $report.decision.exitCode -eq 2 -and
            $report.decision.disposition -ceq
                'CONFIRMED_PINNED_TRIAD_FIXED_16BIT_SLOT_PATTERN_REVIEW_ONLY' -and
            $report.decision.evidenceContractSatisfied -and
            (-not $report.decision.productionApproved) -and
            (-not $report.decision.rebaselinePermitted) -and
            (-not $report.decision.downloadPermitted) -and
            (-not $report.decision.runtimeQualificationPermitted) -and
            (-not $report.decision.futureArtifactAcceptancePermitted) -and
            (-not $report.decision.normalizationUsedForDecision) -and
            $report.decision.requiresReviewedTransition -and
            $report.decision.analysisScope -ceq
                'pinned-historical-triad-only' -and
            (-not $report.inputProvenance.mutableWorktreeClassesRead) -and
            (-not $report.inputProvenance.localKnown6eObjectRequired) -and
            $report.inputProvenance.producer.relativePath -ceq
                $ToolRelativePath -and
            (-not $report.inputProvenance.producer.
                executingBytesAuthenticated) -and
            (($producerReady -and
                    $report.inputProvenance.producer.headRole -ceq
                        'LAST_COMMIT_CHANGING_EXACT_TOOL_PATH' -and
                    $report.inputProvenance.producer.mode -ceq '100644' -and
                    $report.inputProvenance.producer.scopedHeadClean -and
                    $report.inputProvenance.producer.
                        physicalSnapshotEqualsHeadBlob -and
                    (-not $report.inputProvenance.producer.
                        executingBytesAuthenticated) -and
                    $report.inputProvenance.producer.producerTrustBoundary -ceq
                        'NON_ADVERSARIAL_WORKSPACE') -or
                ((-not $producerReady) -and
                    $report.inputProvenance.producer.headRole -ceq
                        'SELFTEST_FIXTURE_NOT_PRODUCTION')) -and
            (@($report.inputProvenance.artifacts).Count -eq 9) -and
            $report.inputProvenance.implicitInputProvenanceMatrix.
                explicitScopedInputIdentity.equivalence -ceq 'EXACT' -and
            $report.inputProvenance.implicitInputProvenanceMatrix.
                explicitScopedInputIdentity.exactEntryMatches -eq 10 -and
            (-not $report.inputProvenance.implicitInputProvenanceMatrix.
                allGeneratorInputsEquivalent) -and
            (-not $report.reconstruction.known6e.binaryPatchApplied) -and
            $report.reconstruction.known6e.binaryPatchRole -ceq
                'PINNED_HISTORICAL_PRESERVATION_EVIDENCE_NOT_REAPPLIED' -and
            $report.semanticBoundary.fieldMeaning -ceq
                'UNCLASSIFIED_OPAQUE_BYTES_IN_GENERATED_ARTIFACT' -and
            $report.semanticBoundary.leadingHypothesis -ceq 'NONE_PROVEN' -and
            $report.semanticBoundary.hypothesisConfidence -ceq
                'UNCLASSIFIED') `
        -Message 'pinned triad report did not produce exact review-only exit 2.'
    $positive++

    $jsonA = ConvertTo-DeterministicJson -Value $report
    $jsonB = ConvertTo-DeterministicJson -Value $report
    $canonicalReport = $jsonA | ConvertFrom-Json
    Assert-ExactJsonKeys -Object $canonicalReport.tool `
        -ObjectOwner 'self-test success report.tool' -Expected @(
            'owner', 'supportedProductionInvocation',
            'outputPublicationTrustBoundary', 'handleRelativeCreationUsed',
            'concurrentParentReplacementResistance')
    Assert-SelfTestTrue `
        -Condition (
            $canonicalReport.tool.outputPublicationTrustBoundary -ceq
                'NON_ADVERSARIAL_WORKSPACE' -and
            (-not $canonicalReport.tool.handleRelativeCreationUsed) -and
            (-not $canonicalReport.tool.concurrentParentReplacementResistance)) `
        -Message 'success report output publication boundary differs.'
    [byte[]]$reportBytes = Get-DeterministicJsonBytes -Json $jsonA
    Assert-SelfTestTrue -Condition ($jsonA -ceq $jsonB) `
        -Message 'repeated report serialization differs.'
    Assert-SelfTestTrue `
        -Condition (
            $reportBytes[0] -ne 0xEF -and
            $reportBytes[$reportBytes.Length - 1] -eq 0x0A -and
            (-not [regex]::IsMatch($jsonA, '[^\x00-\x7F]'))) `
        -Message 'report bytes are not canonical 7-bit UTF-8 LF bytes.'
    $positive++

    $memory = New-Object IO.MemoryStream
    try {
        $pipelineOutput = @(Write-JsonBytesToStream `
                -Bytes $reportBytes -Stream $memory)
        Assert-SelfTestTrue -Condition ($pipelineOutput.Count -eq 0) `
            -Message 'canonical JSON stream write contaminated the success pipeline.'
        Assert-SelfTestTrue `
            -Condition (Test-ByteSequencesExact `
                -Left $reportBytes -Right $memory.ToArray()) `
            -Message 'canonical JSON stream bytes differ.'
    }
    finally { $memory.Dispose() }
    $positive++

    $scalar = @(Get-ExactScalarProcessExitCode `
            -Value ([int]2) -ValueOwner 'self-test review exit')
    Assert-SelfTestTrue `
        -Condition (
            $scalar.Count -eq 1 -and
            $scalar[0] -is [int] -and
            $scalar[0] -eq 2) `
        -Message 'exact scalar exit 2 was not preserved.'
    Assert-SelfTestThrows `
        -Action {
            Get-ExactScalarProcessExitCode `
                -Value @([int]2, 'status') `
                -ValueOwner 'self-test contaminated exit'
        } `
        -Message 'pipeline array exit rejection' `
        -ExpectedText 'must be exactly one System.Int32'
    $positive++; $negative++

    $mutatedReport = $jsonA | ConvertFrom-Json
    $mutatedReport.slotModel.totalVolatileSlots = 65
    Assert-SelfTestTrue `
        -Condition (-not (Test-GoldenObservation -Observation $mutatedReport)) `
        -Message 'mutated golden slot count was accepted.'
    $negative++

    $knownOracleDefinition = @($PinnedInputs | Where-Object {
            $_.role -ceq 'known6eComparisonOracle'
        })[0]
    $knownOracleArtifact = Resolve-PinnedInput `
        -Root $Root -Definition $knownOracleDefinition
    $knownOracle = ConvertFrom-StrictJsonBytes `
        -Bytes $knownOracleArtifact.bytes `
        -JsonOwner 'self-test known oracle' -RequireComparatorCanonical
    $badPreview = (
        (ConvertTo-DeterministicJson -Value $knownOracle) | ConvertFrom-Json)
    $badPreview.diffRuns[0].candidatePreview.truncated = $true
    $checkpointDefinition = @($PinnedInputs | Where-Object {
            $_.role -ceq 'checkpoint244'
        })[0]
    $checkpointArtifact = Resolve-PinnedInput `
        -Root $Root -Definition $checkpointDefinition
    Assert-SelfTestThrows `
        -Action {
            Get-OracleReconstruction `
                -Oracle $badPreview -CheckpointArtifact $checkpointArtifact.bytes `
                -OracleOwner 'self-test truncated oracle' `
                -ExpectedCandidateBytes $KnownRebuildBytes `
                -ExpectedCandidateSha256 $KnownRebuildSha256 `
                -ExpectedCandidateBlobOid $KnownRebuildBlobOid `
                -ExpectedChangedBytes 99 -ExpectedRunCount 58 `
                -ExpectedOwnerCount 36
        } `
        -Message 'truncated oracle preview rejection' `
        -ExpectedText 'preview is incomplete'
    $negative++
    $badOrder = (
        (ConvertTo-DeterministicJson -Value $knownOracle) | ConvertFrom-Json)
    $badOrder.diffRuns[1].checkpointStart =
        [int]$badOrder.diffRuns[0].checkpointStart
    $badOrder.diffRuns[1].candidateStart =
        [int]$badOrder.diffRuns[0].candidateStart
    Assert-SelfTestThrows `
        -Action {
            Get-OracleReconstruction `
                -Oracle $badOrder -CheckpointArtifact $checkpointArtifact.bytes `
                -OracleOwner 'self-test overlapping oracle' `
                -ExpectedCandidateBytes $KnownRebuildBytes `
                -ExpectedCandidateSha256 $KnownRebuildSha256 `
                -ExpectedCandidateBlobOid $KnownRebuildBlobOid `
                -ExpectedChangedBytes 99 -ExpectedRunCount 58 `
                -ExpectedOwnerCount 36
        } `
        -Message 'overlapping oracle run rejection' `
        -ExpectedText 'shape differs'
    $negative++

    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    $tempDirectory = Join-Path $tempRoot (
        'LasalClassesTriadSelfTest-' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($tempDirectory)
    try {
        $exactParent = Join-Path $tempDirectory 'evidence'
        [void][IO.Directory]::CreateDirectory($exactParent)
        $output = Resolve-CreateNewOutputPath `
            -RequestedPath $ReportFileName `
            -AllowedRoot $tempDirectory -ExactParent $exactParent
        Assert-SelfTestTrue `
            -Condition (
                $output.FullPath -ceq (Join-Path $exactParent $ReportFileName) -and
                $output.ExactParent -ceq $exactParent -and
                $output.AllowedRoot -ceq $tempDirectory) `
            -Message 'normalized CreateNew output identity differs.'
        Write-CreateNewBytes -Descriptor $output -Bytes $reportBytes
        Assert-SelfTestTrue `
            -Condition (Test-ByteSequencesExact `
                -Left $reportBytes `
                -Right ([IO.File]::ReadAllBytes($output.FullPath))) `
            -Message 'CreateNew report readback differs.'
        $positive++
        Assert-SelfTestThrows `
            -Action {
                Write-CreateNewBytes -Descriptor $output -Bytes $reportBytes
            } `
            -Message 'CreateNew overwrite rejection' `
            -ExpectedText 'already exists'
        Assert-SelfTestTrue `
            -Condition (Test-ByteSequencesExact `
                -Left $reportBytes `
                -Right ([IO.File]::ReadAllBytes($output.FullPath))) `
            -Message 'CreateNew rejection changed the sentinel.'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath 'wrong-volatility.json' `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'wrong report basename rejection' `
            -ExpectedText 'exact triad report basename'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath (Join-Path 'nested' $ReportFileName) `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'nested report path rejection' `
            -ExpectedText 'direct child'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath (Join-Path `
                        'candidate_finalization_gate_d_rebaseline_6e115876' `
                        $ReportFileName) `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'frozen bundle report path rejection' `
            -ExpectedText 'direct child'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath ($ReportFileName + ':stream') `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'alternate data stream output rejection' `
            -ExpectedText 'alternate data stream'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath ($ReportFileName + '. ') `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'trailing dot or space output rejection' `
            -ExpectedText 'trailing dot or space'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath 'CON.json' `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'reserved device output rejection' `
            -ExpectedText 'reserved Windows device alias'
        $negative++
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath ('.\' + $ReportFileName) `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'lexically unnormalized output rejection' `
            -ExpectedText 'lexically normalized'
        $negative++
        $outsideDirectory = Join-Path $tempDirectory 'outside'
        [void][IO.Directory]::CreateDirectory($outsideDirectory)
        $outside = Join-Path $outsideDirectory $ReportFileName
        Assert-SelfTestThrows `
            -Action {
                Resolve-CreateNewOutputPath `
                    -RequestedPath $outside `
                    -AllowedRoot $tempDirectory -ExactParent $exactParent
            } `
            -Message 'output root escape rejection' `
            -ExpectedText 'direct child'
        Assert-SelfTestTrue -Condition (-not [IO.File]::Exists($outside)) `
            -Message 'escaped output was created.'
        $negative++

        $swapParent = Join-Path $tempDirectory 'swap-parent'
        $junctionTarget = Join-Path $tempRoot (
            'LasalClassesTriadSelfTest-' + [Guid]::NewGuid().ToString('N'))
        [void][IO.Directory]::CreateDirectory($swapParent)
        [void][IO.Directory]::CreateDirectory($junctionTarget)
        $swapDescriptor = Resolve-CreateNewOutputPath `
            -RequestedPath $ReportFileName `
            -AllowedRoot $tempDirectory -ExactParent $swapParent
        try {
            Assert-SelfTestThrows `
                -Action {
                    Write-CreateNewBytes `
                        -Descriptor $swapDescriptor -Bytes $reportBytes `
                        -BeforeCreateSelfTestHook {
                            [IO.Directory]::Delete($swapParent, $false)
                            [void](New-Item -ItemType Junction `
                                -Path $swapParent -Target $junctionTarget)
                        }
                } `
                -Message 'junction-swap output rejection' `
                -ExpectedText 'reparse-point'
            Assert-SelfTestTrue `
                -Condition (-not [IO.File]::Exists(
                        (Join-Path $junctionTarget $ReportFileName))) `
                -Message 'junction-swap wrote outside the exact parent.'
            $negative++
        }
        finally {
            $swapItem = Get-Item -LiteralPath $swapParent `
                -Force -ErrorAction SilentlyContinue
            if (($null -ne $swapItem) -and
                (($swapItem.Attributes -band
                        [IO.FileAttributes]::ReparsePoint) -ne 0)) {
                [IO.Directory]::Delete($swapParent, $false)
            }
            Remove-SelfTestDirectory `
                -Directory $junctionTarget -TempRoot $tempRoot
        }
    }
    finally {
        Remove-SelfTestDirectory `
            -Directory $tempDirectory -TempRoot $tempRoot
    }

    $hosts = Get-PowerShellHosts
    $fixturePs5 = Invoke-ScriptHost `
        -HostPath $hosts.ps5 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    $fixturePs7 = Invoke-ScriptHost `
        -HostPath $hosts.ps7 -ModeArgument '-EmitJsonSelfTestFixtureBase64'
    Assert-SelfTestTrue `
        -Condition (
            $fixturePs5.ExitCode -eq 0 -and
            $fixturePs7.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($fixturePs5.Stderr) -and
            [string]::IsNullOrWhiteSpace($fixturePs7.Stderr) -and
            (Test-ByteSequencesExact `
                -Left $fixturePs5.StdoutBytes -Right $fixturePs7.StdoutBytes)) `
        -Message 'PowerShell 5/7 canonical fixture bytes differ.'
    $positive++

    $productionPs7 = Invoke-ScriptHost `
        -HostPath $hosts.ps7 -ModeArgument '-AnalyzePinnedTriad'
    if ($producerReady) {
        Assert-SelfTestTrue `
            -Condition (
                $productionPs7.ExitCode -eq 2 -and
                [string]::IsNullOrWhiteSpace($productionPs7.Stderr) -and
                (Test-ByteSequencesExact `
                    -Left $reportBytes -Right $productionPs7.StdoutBytes)) `
            -Message 'PowerShell 7 production stdout plus exit 2 contract differs.'
    }
    else {
        $productionBlocked = $Utf8Strict.GetString(
            $productionPs7.StdoutBytes) | ConvertFrom-Json
        Assert-SelfTestTrue `
            -Condition (
                $productionPs7.ExitCode -eq 4 -and
                [string]::IsNullOrWhiteSpace($productionPs7.Stderr) -and
                $productionBlocked.tool.outputPublicationTrustBoundary -ceq
                    'NON_ADVERSARIAL_WORKSPACE' -and
                (-not $productionBlocked.tool.handleRelativeCreationUsed) -and
                (-not $productionBlocked.tool.
                    concurrentParentReplacementResistance) -and
                $productionBlocked.decision.disposition -ceq
                    'BLOCKED_INVALID_OR_UNTRUSTED_INPUT' -and
                $productionBlocked.error.message -ceq
                    'producer script is not tracked and scoped HEAD-clean.') `
            -Message 'uncommitted producer did not fail closed before evidence.'
    }
    $positive++
    $wrongOutputPs7 = Invoke-ScriptHost `
        -HostPath $hosts.ps7 `
        -ModeArgument (
            '-AnalyzePinnedTriad -CreateNew -OutputPath wrong.json')
    $wrongOutputBlocked = $Utf8Strict.GetString(
        $wrongOutputPs7.StdoutBytes) | ConvertFrom-Json
    Assert-ExactJsonKeys -Object $wrongOutputBlocked.tool `
        -ObjectOwner 'PowerShell 7 blocked report.tool' -Expected @(
            'owner', 'supportedProductionInvocation',
            'outputPublicationTrustBoundary', 'handleRelativeCreationUsed',
            'concurrentParentReplacementResistance')
    Assert-SelfTestTrue `
        -Condition (
            $wrongOutputBlocked.tool.outputPublicationTrustBoundary -ceq
                'NON_ADVERSARIAL_WORKSPACE' -and
            (-not $wrongOutputBlocked.tool.handleRelativeCreationUsed) -and
            (-not $wrongOutputBlocked.tool.
                concurrentParentReplacementResistance)) `
        -Message 'blocked report output publication boundary differs.'
    $wrongOutputCondition = if ($producerReady) {
        ($wrongOutputPs7.ExitCode -eq 4) -and
        [string]::IsNullOrWhiteSpace($wrongOutputPs7.Stderr) -and
        ($wrongOutputBlocked.error.message -ceq
            'OutputPath must use the exact triad report basename.')
    }
    else {
        ($wrongOutputPs7.ExitCode -eq 4) -and
        [string]::IsNullOrWhiteSpace($wrongOutputPs7.Stderr) -and
        (Test-ByteSequencesExact `
            -Left $productionPs7.StdoutBytes `
            -Right $wrongOutputPs7.StdoutBytes)
    }
    Assert-SelfTestTrue -Condition $wrongOutputCondition `
        -Message 'producer-before-output-path ordering differs.'
    $negative++
    $productionPs5 = Invoke-ScriptHost `
        -HostPath $hosts.ps5 -ModeArgument '-AnalyzePinnedTriad'
    $invalidRoot = Join-Path ([IO.Path]::GetTempPath()) (
        'LasalClassesTriadMissing-' + [Guid]::NewGuid().ToString('N'))
    $productionPs5InvalidRoot = Invoke-ScriptHost `
        -HostPath $hosts.ps5 `
        -ModeArgument (
            '-AnalyzePinnedTriad -RepositoryRoot "' + $invalidRoot + '"')
    $ps5Text = $Utf8Strict.GetString($productionPs5.StdoutBytes)
    $ps5Blocked = $ps5Text | ConvertFrom-Json
    Assert-SelfTestTrue `
        -Condition (
            $productionPs5.ExitCode -eq 4 -and
            $productionPs5InvalidRoot.ExitCode -eq 4 -and
            [string]::IsNullOrWhiteSpace($productionPs5.Stderr) -and
            [string]::IsNullOrWhiteSpace($productionPs5InvalidRoot.Stderr) -and
            (Test-ByteSequencesExact `
                -Left $productionPs5.StdoutBytes `
                -Right $productionPs5InvalidRoot.StdoutBytes) -and
            $ps5Blocked.tool.outputPublicationTrustBoundary -ceq
                'NON_ADVERSARIAL_WORKSPACE' -and
            (-not $ps5Blocked.tool.handleRelativeCreationUsed) -and
            (-not $ps5Blocked.tool.concurrentParentReplacementResistance) -and
            $ps5Blocked.decision.exitCode -eq 4 -and
            $ps5Blocked.decision.disposition -ceq
                'BLOCKED_INVALID_OR_UNTRUSTED_INPUT') `
        -Message 'PowerShell 5 production did not fail closed before evidence.'
    $negative++

    [Console]::Out.WriteLine(
        "PASS LasalClassesVolatilityTriad.SelfTest Positive=$positive " +
        "Negative=$negative")
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
        decision = [ordered]@{
            disposition = 'BLOCKED_INVALID_OR_UNTRUSTED_INPUT'
            exitCode = 4
            toolCompleted = $false
            evidenceContractSatisfied = $false
            analysisScope = 'pinned-historical-triad-only'
            productionApproved = $false
            semanticEquivalenceProven = $false
            rebaselinePermitted = $false
            downloadPermitted = $false
            runtimeQualificationPermitted = $false
            futureArtifactAcceptancePermitted = $false
            normalizationUsedForDecision = $false
            requiresReviewedTransition = $true
        }
        error = [ordered]@{ message = $normalized }
    }
}

try {
    Assert-InvocationContract -Bound $PSBoundParameters
    if ($EmitJsonSelfTestFixtureBase64) {
        $fixture = ConvertTo-DeterministicJson -Value (Get-JsonSelfTestFixture)
        [byte[]]$fixtureBytes = Get-DeterministicJsonBytes -Json $fixture
        [Console]::Out.WriteLine([Convert]::ToBase64String($fixtureBytes))
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
    $report = Invoke-PinnedTriadAnalysis `
        -Root $resolvedRoot -ProducerIdentity $producerIdentity
    $json = ConvertTo-DeterministicJson -Value $report
    [byte[]]$jsonBytes = Get-DeterministicJsonBytes -Json $json
    if ($CreateNew) {
        Write-CreateNewBytes -Descriptor $resolvedOutput -Bytes $jsonBytes
    }
    Write-JsonStdout -Bytes $jsonBytes
    $finalExitCode = Get-ExactScalarProcessExitCode `
        -Value ([int]$report.decision.exitCode) `
        -ValueOwner 'production triad report exit code'
    exit $finalExitCode
}
catch {
    $blocked = New-BlockedReport -Message $_.Exception.Message
    $blockedJson = ConvertTo-DeterministicJson -Value $blocked
    [byte[]]$blockedBytes = Get-DeterministicJsonBytes -Json $blockedJson
    Write-JsonStdout -Bytes $blockedBytes
    $blockedExitCode = Get-ExactScalarProcessExitCode `
        -Value ([int]4) -ValueOwner 'blocked triad report exit code'
    exit $blockedExitCode
}

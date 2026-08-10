[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [switch]$VerifyBundle,
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$RepositoryRoot,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Owner = 'LASAL.ClassesRebuildFinalizationBundleVerifier'
$EvidenceRelativeDirectory =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876'
$ValidatorRelativePath =
    $EvidenceRelativeDirectory + '/Verify-LasalClassesRebuildFinalizationBundle.ps1'
$FinalizerRelativePath =
    $EvidenceRelativeDirectory + '/Finalize-LasalClassesRebuildCandidate.ps1'
$BaselineRelativePath =
    $EvidenceRelativeDirectory + '/build_baseline_gate_d_rebaseline_6e115876.json'
$FinalDirectoryName = 'candidate_finalization_gate_d_rebaseline_6e115876'
$OwnerMarkerName = '.finalizer-owner.json'
$ClassesSnapshotName = 'Classes.post-rebuild.snapshot.lcb'
$NetworksSnapshotName = 'Networks.post-rebuild.snapshot.lcb'
$TranscriptName = 'derived_build_transcript_gate_d_rebaseline_6e115876.txt'
$RawDeltaName = 'bounded_lasal2_delta_gate_d_rebaseline_6e115876.raw.txt'
$RawManifestName = 'bounded_lasal2_delta_gate_d_rebaseline_6e115876.manifest.json'
$ComparisonName = 'classes_lcb_gate_d_rebuild_candidate.comparison.json'
$CompleteManifestName = 'classes_lcb_gate_d_rebuild_candidate.finalization.json'
$CompleteSchema = 'LasalClassesRebuildCandidateFinalization/v1'
$OwnerMarkerSchema = 'LasalClassesRebuildCandidateFinalizerOwner/v1'
$RawManifestSchema = 'LasalC78BoundedLogDelta/v1'
$ComparisonSchema = 'LasalClassesArtifactComparison/v1'
$CheckpointCommit = '55435791f6e91c9dcb4e06dcd25a11d77b382da7'
$CheckpointBlobOid = '7b0faebb1450ff67b7dad44f081ad5c4ac141ee2'
$CheckpointClassesBytes = 8549773L
$CheckpointClassesSha256 =
    '24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861'
$KnownRebuiltClassesSha256 =
    '6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712'
$KnownNetworksSha256 =
    'C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F'
$KnownBaselinePrefixBytes = 8788633L
$KnownBaselinePrefixSha256 =
    '03F222F7F02E1466F86FDD6D91BB76DAC860CDC4E36674F42CF8A6A314B9AD56'
$KnownFinalizerBytes = 187443L
$KnownFinalizerSha256 =
    '1551A121D49C3C3169B0DADA45B4EEAAFDD8F8636425E470D1A6840159CBC0D5'
$KnownFinalizerBlobOid = '5495e5636462d8aa67e13abb70c310a1ee8f9e67'
$ClassesRelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
$NetworksRelativePath = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
$Utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$ArtifactNames = @(
    $OwnerMarkerName,
    $ClassesSnapshotName,
    $NetworksSnapshotName,
    $TranscriptName,
    $RawDeltaName,
    $RawManifestName,
    $ComparisonName,
    $CompleteManifestName)
$PreManifestArtifactNames = @($ArtifactNames[0..6])
$PinnedTrustedArtifacts = @(
    [ordered]@{
        owner = 'rebuild baseline'
        relativePath = $BaselineRelativePath
        bytes = 6887L
        sha256 = 'BF55B202377C52D0880A7D1E1B7C5B719B3060F2E17BECF4A895820F13AC29C3'
    },
    [ordered]@{
        owner = 'log converter'
        relativePath =
            'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/' +
            'Convert-Lasal2LogToBuildTranscript.ps1'
        bytes = 32701L
        sha256 = '1A92CDE9AA7D45F6A2A250068A8A940ADAA46F856099E2D0174CC9CA09E61CEF'
    },
    [ordered]@{
        owner = 'C78 verifier'
        relativePath =
            'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/' +
            'Verify-LasalC78RebuildEvidence.ps1'
        bytes = 137844L
        sha256 = '7AE60A0BBD1356797E6431D29D3F6D0E39270D56C20B59AD835A3E8F0391A6E0'
    },
    [ordered]@{
        owner = 'Classes comparator'
        relativePath = $EvidenceRelativeDirectory + '/Compare-LasalClassesArtifact.ps1'
        bytes = 79592L
        sha256 = 'B91BFB5AFE131F0ECB3F23DC00373BEC7FC91B2C37CF626D128E912F633EBBA4'
    },
    [ordered]@{
        owner = 'known 6E comparison oracle'
        relativePath =
            $EvidenceRelativeDirectory +
            '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json'
        bytes = 51102L
        sha256 = '9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639'
    })
$HistoricalTextIdentityBridges = @(
    [ordered]@{
        relativePath =
            'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/' +
            'Convert-Lasal2LogToBuildTranscript.ps1'
        physicalBytes = 32701L
        physicalSha256 =
            '1A92CDE9AA7D45F6A2A250068A8A940ADAA46F856099E2D0174CC9CA09E61CEF'
        gitBlobOid = '9a74e8744e5e4d6ada8700c5ada52372429d048f'
        canonicalLfBytes = 31837L
        canonicalLfSha256 =
            'D97E6939A0CB3C9E01D062DB455C9A6085C34BDF50DEF890B137DCA5F340FD9E'
    },
    [ordered]@{
        relativePath =
            'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/' +
            'Verify-LasalC78RebuildEvidence.ps1'
        physicalBytes = 137844L
        physicalSha256 =
            '7AE60A0BBD1356797E6431D29D3F6D0E39270D56C20B59AD835A3E8F0391A6E0'
        gitBlobOid = 'ea31780f5cbc2cb9b23f6d4e92a7bde5233b49f0'
        canonicalLfBytes = 134998L
        canonicalLfSha256 =
            '63E32F77F33A8F84F8519A23FDD65FF40E0D0715D817C9098AB94583323FF5A6'
    })

function Throw-BundleBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw [InvalidOperationException]::new($Message)
}

function Assert-PowerShell7 {
    param([Parameter(Mandatory = $true)][string]$Phase)
    if (($PSVersionTable.PSEdition -cne 'Core') -or
        ($PSVersionTable.PSVersion.Major -lt 7)) {
        Throw-BundleBlocker (
            "$Phase requires PowerShell 7 before any bundle evidence is read; " +
            "observed $($PSVersionTable.PSEdition) $($PSVersionTable.PSVersion).")
    }
}

function Get-NormalizedFullPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) {
        Throw-BundleBlocker 'a required path is empty.'
    }
    return [IO.Path]::GetFullPath($Path).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
}

function Test-PathInsideRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$AllowEqual
    )
    $normalizedRoot = Get-NormalizedFullPath -Path $Root
    $normalizedPath = Get-NormalizedFullPath -Path $Path
    if ($AllowEqual -and [string]::Equals(
            $normalizedRoot,
            $normalizedPath,
            [StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }
    $prefix = $normalizedRoot
    if (-not ($prefix.EndsWith(
                [IO.Path]::DirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal) -or
            $prefix.EndsWith(
                [IO.Path]::AltDirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal))) {
        $prefix += [IO.Path]::DirectorySeparatorChar
    }
    return $normalizedPath.StartsWith(
        $prefix,
        [StringComparison]::OrdinalIgnoreCase)
}

function Assert-NoReparsePointChain {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$IncludeLeaf,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    $normalizedRoot = Get-NormalizedFullPath -Path $Root
    $normalizedPath = Get-NormalizedFullPath -Path $Path
    if (-not (Test-PathInsideRoot -Root $normalizedRoot -Path $normalizedPath -AllowEqual)) {
        Throw-BundleBlocker "$PathOwner escapes its trusted root."
    }
    $targets = New-Object Collections.Generic.List[string]
    $volumeRoot = [IO.Path]::GetPathRoot($normalizedRoot)
    $current = $volumeRoot
    $targets.Add($current)
    $rootRelative = $normalizedRoot.Substring($volumeRoot.Length).TrimStart('\', '/')
    if (-not [string]::IsNullOrEmpty($rootRelative)) {
        foreach ($part in ($rootRelative -split '[\\/]')) {
            $current = Join-Path $current $part
            $targets.Add($current)
        }
    }
    $relative = $normalizedPath.Substring($normalizedRoot.Length).TrimStart('\', '/')
    $current = $normalizedRoot
    if (-not [string]::IsNullOrEmpty($relative)) {
        $parts = $relative -split '[\\/]'
        for ($index = 0; $index -lt $parts.Count; $index++) {
            $current = Join-Path $current $parts[$index]
            $isLeaf = $index -eq ($parts.Count - 1)
            if ($isLeaf -and -not $IncludeLeaf) { break }
            $targets.Add($current)
        }
    }
    foreach ($target in $targets) {
        if (-not (Test-Path -LiteralPath $target)) { break }
        $attributes = [IO.File]::GetAttributes($target)
        if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-BundleBlocker "$PathOwner contains a reparse point: $target"
        }
    }
}

function Assert-NoReparsePointDescendants {
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][string]$DirectoryOwner
    )
    $queue = New-Object Collections.Generic.Queue[string]
    $queue.Enqueue((Get-NormalizedFullPath -Path $Directory))
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        foreach ($entry in [IO.Directory]::GetFileSystemEntries($current)) {
            $attributes = [IO.File]::GetAttributes($entry)
            if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                Throw-BundleBlocker (
                    "$DirectoryOwner contains a descendant reparse point: $entry")
            }
            if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
                $queue.Enqueue($entry)
            }
        }
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
        Throw-BundleBlocker (
            "$PathOwner stream inventory failed: $($_.Exception.Message)")
    }
    if (Test-Path -LiteralPath $Path -PathType Container) {
        if ($streams.Count -ne 0) {
            Throw-BundleBlocker "$PathOwner contains a directory alternate data stream."
        }
        return
    }
    if (($streams.Count -ne 1) -or ([string]$streams[0].Stream -cne ':$DATA')) {
        Throw-BundleBlocker "$PathOwner contains a non-default data stream."
    }
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function Test-ByteSequencesExact {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Actual,
        [Parameter(Mandatory = $true)][byte[]]$Expected
    )
    if ($Actual.LongLength -ne $Expected.LongLength) { return $false }
    for ($index = 0L; $index -lt $Actual.LongLength; $index++) {
        if ($Actual[$index] -ne $Expected[$index]) { return $false }
    }
    return $true
}

function Get-FileArtifact {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$FileOwner
    )
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-BundleBlocker "$FileOwner does not exist: $Path"
    }
    [byte[]]$first = [IO.File]::ReadAllBytes($Path)
    [byte[]]$second = [IO.File]::ReadAllBytes($Path)
    $firstSha = Get-BytesSha256 -Bytes $first
    $secondSha = Get-BytesSha256 -Bytes $second
    if (($first.LongLength -ne $second.LongLength) -or ($firstSha -cne $secondSha)) {
        Throw-BundleBlocker "$FileOwner changed while it was read."
    }
    return [pscustomobject]@{
        path = Get-NormalizedFullPath -Path $Path
        bytes = [long]$second.LongLength
        sha256 = $secondSha
        content = $second
    }
}

function Test-IsJsonObject {
    param($Value)
    return $null -ne $Value -and $Value -is [pscustomobject]
}

function Test-IsJsonArray {
    param($Value)
    return $null -ne $Value -and $Value -is [Array]
}

function Test-IsJsonInteger {
    param($Value)
    return ($Value -is [byte]) -or ($Value -is [sbyte]) -or
        ($Value -is [int16]) -or ($Value -is [uint16]) -or
        ($Value -is [int32]) -or ($Value -is [uint32]) -or
        ($Value -is [int64]) -or ($Value -is [uint64])
}

function Assert-ExactObjectKeys {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string[]]$Expected,
        [Parameter(Mandatory = $true)][string]$ObjectOwner
    )
    if (-not (Test-IsJsonObject -Value $Object)) {
        Throw-BundleBlocker "$ObjectOwner is not a JSON object."
    }
    $actual = @($Object.PSObject.Properties.Name)
    if ($actual.Count -ne $Expected.Count) {
        Throw-BundleBlocker "$ObjectOwner key count differs."
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if ([string]$actual[$index] -cne [string]$Expected[$index]) {
            Throw-BundleBlocker (
                "$ObjectOwner exact key order/case differs at index $index.")
        }
    }
}

function Assert-JsonBoolean {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    if ($Value -isnot [bool]) {
        Throw-BundleBlocker "$ValueOwner is not a JSON boolean."
    }
}

function Assert-JsonInteger {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    if (-not (Test-IsJsonInteger -Value $Value)) {
        Throw-BundleBlocker "$ValueOwner is not a JSON integer."
    }
}

function Assert-JsonString {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    if ($Value -isnot [string]) {
        Throw-BundleBlocker "$ValueOwner is not a JSON string."
    }
}

function Assert-JsonDateString {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    if (($Value -isnot [string]) -and ($Value -isnot [DateTime]) -and
        ($Value -isnot [DateTimeOffset])) {
        Throw-BundleBlocker "$ValueOwner is not a JSON date string."
    }
}

function Assert-Sha256Text {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    Assert-JsonString -Value $Value -ValueOwner $ValueOwner
    if ([string]$Value -cnotmatch '^[0-9A-Fa-f]{64}$') {
        Throw-BundleBlocker "$ValueOwner is not a SHA-256 value."
    }
}

function Assert-LowercaseSha256Text {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    Assert-Sha256Text -Value $Value -ValueOwner $ValueOwner
    if ([string]$Value -cnotmatch '^[0-9a-f]{64}$') {
        Throw-BundleBlocker "$ValueOwner is not canonical lowercase SHA-256."
    }
}

function Assert-UppercaseSha256Text {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    Assert-Sha256Text -Value $Value -ValueOwner $ValueOwner
    if ([string]$Value -cnotmatch '^[0-9A-F]{64}$') {
        Throw-BundleBlocker "$ValueOwner is not canonical uppercase SHA-256."
    }
}

function Assert-GitObjectIdText {
    param($Value, [Parameter(Mandatory = $true)][string]$ValueOwner)
    Assert-JsonString -Value $Value -ValueOwner $ValueOwner
    if ([string]$Value -cnotmatch '^[0-9A-Fa-f]{40}$') {
        Throw-BundleBlocker "$ValueOwner is not a 40-character Git object id."
    }
}

function Assert-SafeRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    if ([string]::IsNullOrWhiteSpace($RelativePath) -or
        [IO.Path]::IsPathRooted($RelativePath) -or
        $RelativePath.Contains('\') -or
        $RelativePath.Contains(':') -or
        $RelativePath.Contains([char]0)) {
        Throw-BundleBlocker "$PathOwner is not a canonical repository-relative path."
    }
    foreach ($part in ($RelativePath -split '/')) {
        if ([string]::IsNullOrEmpty($part) -or $part -in @('.', '..')) {
            Throw-BundleBlocker "$PathOwner contains an unsafe path segment."
        }
    }
}

function Assert-NoDuplicateJsonKeys {
    param(
        [Parameter(Mandatory = $true)]$Element,
        [Parameter(Mandatory = $true)][string]$JsonPath
    )
    if ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Object) {
        $exact = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $folded = [Collections.Generic.HashSet[string]]::new(
            [StringComparer]::OrdinalIgnoreCase)
        foreach ($property in $Element.EnumerateObject()) {
            if (-not $exact.Add($property.Name)) {
                Throw-BundleBlocker "$JsonPath has duplicate key '$($property.Name)'."
            }
            if (-not $folded.Add($property.Name)) {
                Throw-BundleBlocker (
                    "$JsonPath has case-colliding key '$($property.Name)'.")
            }
            Assert-NoDuplicateJsonKeys `
                -Element $property.Value `
                -JsonPath ($JsonPath + '.' + $property.Name)
        }
        return
    }
    if ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Array) {
        $index = 0
        foreach ($item in $Element.EnumerateArray()) {
            Assert-NoDuplicateJsonKeys `
                -Element $item `
                -JsonPath ($JsonPath + '[' + $index + ']')
            $index++
        }
    }
}

function Assert-ExactUtcTimestampText {
    param(
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$ValueOwner
    )
    if ($Value -cnotmatch
        '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z$') {
        Throw-BundleBlocker "$ValueOwner is not canonical seven-digit UTC ISO text."
    }
    [DateTimeOffset]$parsed = [DateTimeOffset]::MinValue
    $styles = [Globalization.DateTimeStyles]::AssumeUniversal -bor
        [Globalization.DateTimeStyles]::AdjustToUniversal
    if (-not [DateTimeOffset]::TryParseExact(
            $Value,
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
            [Globalization.CultureInfo]::InvariantCulture,
            $styles,
            [ref]$parsed)) {
        Throw-BundleBlocker "$ValueOwner is not a valid UTC timestamp."
    }
    $canonical = $parsed.ToUniversalTime().ToString(
        "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
        [Globalization.CultureInfo]::InvariantCulture)
    if ($canonical -cne $Value) {
        Throw-BundleBlocker "$ValueOwner does not round-trip exactly as UTC."
    }
}

function Assert-ExactUtcJsonProperties {
    param(
        [Parameter(Mandatory = $true)]$Element,
        [Parameter(Mandatory = $true)][string]$JsonPath
    )
    if ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Object) {
        foreach ($property in $Element.EnumerateObject()) {
            $propertyPath = $JsonPath + '.' + $property.Name
            if ($property.Name -cin @('capturedAtUtc', 'CapturedAtUtc')) {
                if ($property.Value.ValueKind -ne
                    [System.Text.Json.JsonValueKind]::String) {
                    Throw-BundleBlocker "$propertyPath is not a JSON string."
                }
                Assert-ExactUtcTimestampText `
                    -Value ([string]$property.Value.GetString()) `
                    -ValueOwner $propertyPath
            }
            Assert-ExactUtcJsonProperties `
                -Element $property.Value -JsonPath $propertyPath
        }
        return
    }
    if ($Element.ValueKind -eq [System.Text.Json.JsonValueKind]::Array) {
        $index = 0
        foreach ($item in $Element.EnumerateArray()) {
            Assert-ExactUtcJsonProperties `
                -Element $item -JsonPath ($JsonPath + '[' + $index + ']')
            $index++
        }
    }
}

function Read-StrictJsonArtifact {
    param(
        [Parameter(Mandatory = $true)]$Artifact,
        [Parameter(Mandatory = $true)][string]$JsonOwner
    )
    [byte[]]$bytes = $Artifact.content
    if (($bytes.Length -ge 3) -and
        ($bytes[0] -eq 0xEF) -and ($bytes[1] -eq 0xBB) -and ($bytes[2] -eq 0xBF)) {
        Throw-BundleBlocker "$JsonOwner contains a UTF-8 BOM."
    }
    try {
        $text = $Utf8Strict.GetString($bytes)
    }
    catch {
        Throw-BundleBlocker "$JsonOwner is not strict UTF-8: $($_.Exception.Message)"
    }
    $document = $null
    try {
        $options = [System.Text.Json.JsonDocumentOptions]::new()
        $options.AllowTrailingCommas = $false
        $options.CommentHandling = [System.Text.Json.JsonCommentHandling]::Disallow
        $document = [System.Text.Json.JsonDocument]::Parse($text, $options)
        Assert-NoDuplicateJsonKeys -Element $document.RootElement -JsonPath '$'
        Assert-ExactUtcJsonProperties -Element $document.RootElement -JsonPath '$'
    }
    catch {
        Throw-BundleBlocker "$JsonOwner is not strict unique-key JSON: $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $document) { $document.Dispose() }
    }
    try {
        return ($text | ConvertFrom-Json)
    }
    catch {
        Throw-BundleBlocker "$JsonOwner cannot be converted from JSON: $($_.Exception.Message)"
    }
}

function ConvertTo-PrettyJsonBytes {
    param([Parameter(Mandatory = $true)]$Value)
    $text = ($Value | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n"
    return ,$Utf8NoBom.GetBytes($text)
}

function ConvertTo-CanonicalLfTextBytes {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$TextOwner
    )
    if (($Bytes.Length -ge 3) -and
        ($Bytes[0] -eq 0xEF) -and ($Bytes[1] -eq 0xBB) -and
        ($Bytes[2] -eq 0xBF)) {
        Throw-BundleBlocker "$TextOwner contains a UTF-8 BOM."
    }
    try {
        $text = $Utf8Strict.GetString($Bytes)
    }
    catch {
        Throw-BundleBlocker "$TextOwner is not strict UTF-8: $($_.Exception.Message)"
    }
    $text = $text.Replace("`r`n", "`n").Replace("`r", "`n")
    return ,$Utf8NoBom.GetBytes($text)
}

function Assert-HistoricalManifestIdentityMatchesGitBlob {
    param(
        [Parameter(Mandatory = $true)]$Report,
        [Parameter(Mandatory = $true)]$Blob,
        [Parameter(Mandatory = $true)][string]$BlobOid,
        $TextIdentityBridge
    )
    if (($Blob.bytes -eq $Report.bytes) -and
        ($Blob.sha256 -ceq $Report.sha256)) {
        return
    }
    if ($null -eq $TextIdentityBridge) {
        Throw-BundleBlocker (
            "$($Report.owner) bytes/SHA differ from the Git blob and no " +
            'reviewed historical text identity bridge is allowed.')
    }
    [byte[]]$canonical = ConvertTo-CanonicalLfTextBytes `
        -Bytes $Blob.content `
        -TextOwner "$($Report.owner) historical Git blob"
    if (([string]$Report.relativePath -cne
            [string]$TextIdentityBridge.relativePath) -or
        ([long]$Report.bytes -ne [long]$TextIdentityBridge.physicalBytes) -or
        ([string]$Report.sha256 -cne
            [string]$TextIdentityBridge.physicalSha256) -or
        ($BlobOid -cne [string]$TextIdentityBridge.gitBlobOid) -or
        ($canonical.LongLength -ne
            [long]$TextIdentityBridge.canonicalLfBytes) -or
        ((Get-BytesSha256 -Bytes $canonical) -cne
            [string]$TextIdentityBridge.canonicalLfSha256)) {
        Throw-BundleBlocker (
            "$($Report.owner) differs from its exact reviewed physical/Git " +
            'text identity bridge.')
    }
}

function ConvertTo-ComparatorCanonicalJsonBytes {
    param([Parameter(Mandatory = $true)]$Value)
    $json = ($Value | ConvertTo-Json -Depth 18 -Compress)
    $canonical = [regex]::Replace(
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
    if ([regex]::IsMatch($canonical, '[^\x00-\x7F]')) {
        Throw-BundleBlocker 'comparator canonical JSON is not 7-bit ASCII.'
    }
    return ,$Utf8NoBom.GetBytes($canonical + "`n")
}

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Operation,
        [int[]]$AllowedExitCodes = @(0)
    )
    $info = [Diagnostics.ProcessStartInfo]::new()
    $info.FileName = 'git.exe'
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.ArgumentList.Add('-C')
    $info.ArgumentList.Add($Root)
    foreach ($argument in $Arguments) { $info.ArgumentList.Add($argument) }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $info
    try {
        if (-not $process.Start()) {
            Throw-BundleBlocker "$Operation could not start git."
        }
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -notin $AllowedExitCodes) {
            Throw-BundleBlocker (
                "$Operation failed with git exit $($process.ExitCode): $stderr")
        }
        return [pscustomobject]@{
            exitCode = [int]$process.ExitCode
            stdout = $stdout.Trim()
            stderr = $stderr.Trim()
        }
    }
    finally {
        $process.Dispose()
    }
}

function Read-GitBlobArtifact {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$BlobOid,
        [Parameter(Mandatory = $true)][string]$BlobOwner
    )
    if ($BlobOid -cnotmatch '^[0-9A-Fa-f]{40}$') {
        Throw-BundleBlocker "$BlobOwner has an invalid blob oid."
    }
    $info = [Diagnostics.ProcessStartInfo]::new()
    $info.FileName = 'git.exe'
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.ArgumentList.Add('-C')
    $info.ArgumentList.Add($Root)
    $info.ArgumentList.Add('cat-file')
    $info.ArgumentList.Add('blob')
    $info.ArgumentList.Add($BlobOid)
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $info
    $memory = [IO.MemoryStream]::new()
    try {
        if (-not $process.Start()) {
            Throw-BundleBlocker "$BlobOwner could not start git cat-file."
        }
        $process.StandardOutput.BaseStream.CopyTo($memory)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-BundleBlocker (
                "$BlobOwner git cat-file failed with exit $($process.ExitCode): $stderr")
        }
        [byte[]]$bytes = $memory.ToArray()
        return [pscustomobject]@{
            bytes = [long]$bytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $bytes
            content = $bytes
            blobOid = $BlobOid.ToLowerInvariant()
        }
    }
    finally {
        $memory.Dispose()
        $process.Dispose()
    }
}

function Resolve-ProductionRepository {
    param([Parameter(Mandatory = $true)][string]$RequestedRoot)
    $scriptBoundRoot = Get-NormalizedFullPath -Path (Join-Path $PSScriptRoot '..\..\..')
    $requested = Get-NormalizedFullPath -Path $RequestedRoot
    if (-not [string]::Equals(
            $scriptBoundRoot,
            $requested,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-BundleBlocker 'RepositoryRoot differs from the script-bound repository.'
    }
    Assert-NoReparsePointChain `
        -Root $requested -Path $requested -IncludeLeaf -PathOwner 'repository root'
    $gitRoot = (Invoke-GitText `
            -Root $requested `
            -Arguments @('rev-parse', '--show-toplevel') `
            -Operation 'repository root resolution').stdout
    if (-not [string]::Equals(
            (Get-NormalizedFullPath -Path $gitRoot),
            $requested,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-BundleBlocker 'script-bound root differs from the Git toplevel.'
    }
    return $requested
}

function Assert-ValidatorTrackedAndHeadClean {
    param([Parameter(Mandatory = $true)][string]$Root)
    $tracked = Invoke-GitText `
        -Root $Root `
        -Arguments @('ls-files', '--error-unmatch', '--', $ValidatorRelativePath) `
        -Operation 'validator tracked-file check'
    if ([string]$tracked.stdout -cne $ValidatorRelativePath) {
        Throw-BundleBlocker 'validator tracked-file result differs.'
    }
    $status = Invoke-GitText `
        -Root $Root `
        -Arguments @(
            'status', '--porcelain=v1', '--untracked-files=all', '--',
            $ValidatorRelativePath) `
        -Operation 'validator HEAD-clean check'
    if (-not [string]::IsNullOrEmpty([string]$status.stdout)) {
        Throw-BundleBlocker 'validator is not HEAD-clean.'
    }
    $blobOid = (Invoke-GitText `
            -Root $Root `
            -Arguments @('rev-parse', '--verify', "HEAD:$ValidatorRelativePath") `
            -Operation 'validator HEAD blob resolution').stdout
    if ($blobOid -cnotmatch '^[0-9a-f]{40}$') {
        Throw-BundleBlocker 'validator HEAD blob oid is invalid.'
    }
    $physical = Get-FileArtifact `
        -Path (Join-Path $Root $ValidatorRelativePath) `
        -FileOwner 'validator physical file'
    $headBlob = Read-GitBlobArtifact `
        -Root $Root -BlobOid $blobOid -BlobOwner 'validator HEAD blob'
    [byte[]]$physicalCanonical = ConvertTo-CanonicalLfTextBytes `
        -Bytes $physical.content -TextOwner 'validator physical file'
    [byte[]]$headCanonical = ConvertTo-CanonicalLfTextBytes `
        -Bytes $headBlob.content -TextOwner 'validator HEAD blob'
    if (($physicalCanonical.LongLength -ne $headCanonical.LongLength) -or
        (-not (Test-ByteSequencesExact `
                -Actual $physicalCanonical -Expected $headCanonical))) {
        Throw-BundleBlocker (
            'validator physical file differs from its HEAD blob after ' +
            'canonical LF normalization.')
    }
    return [pscustomobject]@{
        relativePath = $ValidatorRelativePath
        blobOid = $blobOid
        bytes = [long]$physical.bytes
        sha256 = [string]$physical.sha256
        headBlobBytes = [long]$headBlob.bytes
        headBlobSha256 = [string]$headBlob.sha256
        canonicalLfSha256 = Get-BytesSha256 -Bytes $physicalCanonical
    }
}

function Get-ExactBundleInventory {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][string[]]$ExpectedNames
    )
    if (-not (Test-Path -LiteralPath $BundlePath -PathType Container)) {
        Throw-BundleBlocker "fixed final bundle directory is absent: $BundlePath"
    }
    Assert-NoReparsePointChain `
        -Root $RepositoryRoot -Path $BundlePath -IncludeLeaf -PathOwner 'final bundle'
    Assert-NoReparsePointDescendants `
        -Directory $BundlePath -DirectoryOwner 'final bundle'
    Assert-OnlyDefaultDataStream -Path $BundlePath -PathOwner 'final bundle directory'
    $entries = @([IO.Directory]::GetFileSystemEntries($BundlePath))
    if ($entries.Count -ne $ExpectedNames.Count) {
        Throw-BundleBlocker (
            "final bundle inventory count is $($entries.Count), expected " +
            "$($ExpectedNames.Count).")
    }
    $artifacts = [Collections.Generic.Dictionary[string,object]]::new(
        [StringComparer]::Ordinal)
    foreach ($entry in $entries) {
        $attributes = [IO.File]::GetAttributes($entry)
        if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
            Throw-BundleBlocker "final bundle contains a directory: $entry"
        }
        if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-BundleBlocker "final bundle contains a reparse-point file: $entry"
        }
        $name = [IO.Path]::GetFileName($entry)
        $matches = @($ExpectedNames | Where-Object { $_ -ceq $name })
        if ($matches.Count -ne 1) {
            Throw-BundleBlocker "final bundle contains an unexpected file: $name"
        }
        Assert-OnlyDefaultDataStream `
            -Path $entry -PathOwner "final bundle file $name"
        $artifacts.Add($name, (Get-FileArtifact `
                -Path $entry -FileOwner "final bundle file $name"))
    }
    foreach ($name in $ExpectedNames) {
        if (-not $artifacts.ContainsKey($name)) {
            Throw-BundleBlocker "final bundle is missing exact file: $name"
        }
    }
    return $artifacts
}

function Assert-ArtifactIdentityReport {
    param(
        [Parameter(Mandatory = $true)]$Report,
        [Parameter(Mandatory = $true)][string]$ExpectedName,
        [Parameter(Mandatory = $true)]$Artifact,
        [Parameter(Mandatory = $true)][string]$ReportOwner
    )
    Assert-ExactObjectKeys `
        -Object $Report `
        -Expected @('fileName', 'bytes', 'sha256') `
        -ObjectOwner $ReportOwner
    Assert-JsonString -Value $Report.fileName -ValueOwner "$ReportOwner.fileName"
    Assert-JsonInteger -Value $Report.bytes -ValueOwner "$ReportOwner.bytes"
    Assert-Sha256Text -Value $Report.sha256 -ValueOwner "$ReportOwner.sha256"
    if (([string]$Report.fileName -cne $ExpectedName) -or
        ([long]$Report.bytes -ne [long]$Artifact.bytes) -or
        ([string]$Report.sha256 -cne [string]$Artifact.sha256)) {
        Throw-BundleBlocker "$ReportOwner does not bind the exact bundle file."
    }
}

function Assert-OwnerMarkerContract {
    param(
        [Parameter(Mandatory = $true)]$MarkerArtifact,
        [Parameter(Mandatory = $true)][string]$ExpectedStageName
    )
    $marker = Read-StrictJsonArtifact `
        -Artifact $MarkerArtifact -JsonOwner 'owner marker'
    Assert-ExactObjectKeys `
        -Object $marker `
        -Expected @(
            'schema', 'ownerToken', 'stageDirectoryName',
            'overwriteAllowed', 'productionApproved') `
        -ObjectOwner 'owner marker'
    foreach ($name in @('schema', 'ownerToken', 'stageDirectoryName')) {
        Assert-JsonString -Value $marker.$name -ValueOwner "owner marker.$name"
    }
    Assert-JsonBoolean `
        -Value $marker.overwriteAllowed -ValueOwner 'owner marker.overwriteAllowed'
    Assert-JsonBoolean `
        -Value $marker.productionApproved -ValueOwner 'owner marker.productionApproved'
    if (($marker.schema -cne $OwnerMarkerSchema) -or
        ([string]$marker.ownerToken -cnotmatch '^[0-9a-f]{32}$') -or
        ([string]$marker.stageDirectoryName -cne $ExpectedStageName) -or
        [bool]$marker.overwriteAllowed -or
        [bool]$marker.productionApproved) {
        Throw-BundleBlocker 'owner marker contract differs or contains approval.'
    }
    [byte[]]$expectedBytes = ConvertTo-PrettyJsonBytes -Value $marker
    if (-not (Test-ByteSequencesExact `
            -Actual $MarkerArtifact.content -Expected $expectedBytes)) {
        Throw-BundleBlocker 'owner marker is not exact finalizer-canonical JSON bytes.'
    }
}

function Assert-ReportListShape {
    param(
        [Parameter(Mandatory = $true)]$Reports,
        [Parameter(Mandatory = $true)][int]$ExpectedCount,
        [Parameter(Mandatory = $true)][string[]]$ExpectedKeys,
        [Parameter(Mandatory = $true)][string]$ReportsOwner,
        [Parameter(Mandatory = $true)][string]$NameProperty
    )
    if (-not (Test-IsJsonArray -Value $Reports)) {
        Throw-BundleBlocker "$ReportsOwner is not a JSON array."
    }
    if (@($Reports).Count -ne $ExpectedCount) {
        Throw-BundleBlocker (
            "$ReportsOwner count differs: observed $(@($Reports).Count), " +
            "expected $ExpectedCount; type=$($Reports.GetType().FullName).")
    }
    $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 0; $index -lt $ExpectedCount; $index++) {
        $report = $Reports[$index]
        Assert-ExactObjectKeys `
            -Object $report -Expected $ExpectedKeys `
            -ObjectOwner "$ReportsOwner[$index]"
        $name = [string]$report.$NameProperty
        if ([string]::IsNullOrWhiteSpace($name) -or (-not $names.Add($name))) {
            Throw-BundleBlocker "$ReportsOwner has an empty or duplicate name."
        }
    }
}

function Assert-BaselineInputIdentityBinding {
    param(
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)]$CurrentInputs
    )
    Assert-ExactObjectKeys `
        -Object $Baseline `
        -Expected @(
            'Schema', 'EvidenceProfile', 'CapturedAtUtc', 'RepositoryRoot',
            'CanonicalProjectPath', 'LasalLogPath', 'LogPrefixLength',
            'LogPrefixSha256', 'RequiredCompileRelativePaths', 'Files') `
        -ObjectOwner 'historical rebuild baseline'
    foreach ($name in @(
            'Schema', 'EvidenceProfile', 'RepositoryRoot', 'CanonicalProjectPath',
            'LasalLogPath', 'LogPrefixSha256')) {
        Assert-JsonString `
            -Value $Baseline.$name -ValueOwner "historical baseline.$name"
    }
    Assert-JsonDateString `
        -Value $Baseline.CapturedAtUtc `
        -ValueOwner 'historical baseline.CapturedAtUtc'
    Assert-JsonInteger `
        -Value $Baseline.LogPrefixLength `
        -ValueOwner 'historical baseline.LogPrefixLength'
    Assert-LowercaseSha256Text `
        -Value $Baseline.LogPrefixSha256 `
        -ValueOwner 'historical baseline.LogPrefixSha256'
    if (($Baseline.Schema -cne 'LasalC78RebuildEvidence/v1') -or
        ($Baseline.EvidenceProfile -cne 'GateDVisualLayout') -or
        (-not (Test-IsJsonArray -Value $Baseline.RequiredCompileRelativePaths)) -or
        (-not (Test-IsJsonArray -Value $Baseline.Files)) -or
        (-not (Test-IsJsonArray -Value $CurrentInputs))) {
        Throw-BundleBlocker 'historical baseline header/array contract differs.'
    }
    $inputFiles = @($Baseline.Files | Where-Object { $_.Role -ceq 'inputIdentity' })
    if (($inputFiles.Count -ne 10) -or (@($CurrentInputs).Count -ne 10)) {
        Throw-BundleBlocker 'historical baseline inputIdentity count differs.'
    }
    $paths = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 0; $index -lt 10; $index++) {
        $inputFile = $inputFiles[$index]
        $hasRawIdentity =
            $inputFile.PSObject.Properties.Name -ccontains 'RawBytes'
        Assert-ExactObjectKeys `
            -Object $inputFile `
            -Expected $(if ($hasRawIdentity) {
                @(
                    'RelativePath', 'Role', 'Sha256', 'RawBytes', 'RawSha256',
                    'CanonicalLfBytes', 'CanonicalLfSha256', 'EolStyle',
                    'CrLfCount', 'LfOnlyCount', 'CrOnlyCount', 'LineBreakCount')
            } else {
                @('RelativePath', 'Role', 'Sha256')
            }) `
            -ObjectOwner "historical baseline inputIdentity[$index]"
        Assert-JsonString `
            -Value $inputFile.RelativePath -ValueOwner 'baseline input RelativePath'
        Assert-SafeRelativePath `
            -RelativePath ([string]$inputFile.RelativePath) `
            -PathOwner 'baseline input RelativePath'
        Assert-JsonString -Value $inputFile.Role -ValueOwner 'baseline input Role'
        Assert-LowercaseSha256Text `
            -Value $inputFile.Sha256 -ValueOwner 'baseline input Sha256'
        if (($inputFile.Role -cne 'inputIdentity') -or
            (-not $paths.Add([string]$inputFile.RelativePath))) {
            Throw-BundleBlocker 'historical baseline input role/path differs.'
        }
        [long]$expectedBytes = 0
        [string]$expectedSha = [string]$inputFile.Sha256
        if ($hasRawIdentity) {
            Assert-JsonInteger `
                -Value $inputFile.RawBytes -ValueOwner 'baseline input RawBytes'
            Assert-LowercaseSha256Text `
                -Value $inputFile.RawSha256 -ValueOwner 'baseline input RawSha256'
            $expectedBytes = [long]$inputFile.RawBytes
            $expectedSha = [string]$inputFile.RawSha256
        }
        else {
            switch -CaseSensitive ([string]$inputFile.RelativePath) {
                'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp' {
                    $expectedBytes = 25188L
                }
                'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn' {
                    $expectedBytes = 16540L
                }
                default {
                    Throw-BundleBlocker (
                        'historical baseline lacks RawBytes for an unpinned input.')
                }
            }
        }
        $current = $CurrentInputs[$index]
        Assert-ExactObjectKeys `
            -Object $current `
            -Expected @(
                'relativePath', 'bytes', 'sha256',
                'exactBaselineInputIdentity') `
            -ObjectOwner "complete baseline.currentInputs[$index]"
        Assert-JsonString -Value $current.relativePath -ValueOwner 'current input path'
        Assert-JsonInteger -Value $current.bytes -ValueOwner 'current input bytes'
        Assert-UppercaseSha256Text `
            -Value $current.sha256 -ValueOwner 'current input sha256'
        Assert-JsonBoolean `
            -Value $current.exactBaselineInputIdentity `
            -ValueOwner 'current input exactBaselineInputIdentity'
        if (($current.relativePath -cne $inputFile.RelativePath) -or
            ([long]$current.bytes -ne $expectedBytes) -or
            ([string]$current.sha256 -cne $expectedSha.ToUpperInvariant()) -or
            (-not [bool]$current.exactBaselineInputIdentity)) {
            Throw-BundleBlocker (
                "complete baseline.currentInputs[$index] differs from the " +
                'ordered historical baseline identity.')
        }
    }
}

function Assert-CompleteManifestShape {
    param(
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [switch]$SkipPinnedProductionIdentities
    )
    Assert-ExactObjectKeys `
        -Object $Manifest `
        -Expected @(
            'schema', 'complete', 'capturedAtUtc', 'repository',
            'powershellEngine', 'trustedArtifacts', 'finalizer', 'baseline',
            'log', 'isolatedSession', 'tools', 'regeneratedOutputs',
            'artifactsWrittenBeforeCompleteManifest', 'publication', 'decision',
            'productionApproved') `
        -ObjectOwner 'complete manifest'
    Assert-JsonString -Value $Manifest.schema -ValueOwner 'complete manifest.schema'
    Assert-JsonBoolean -Value $Manifest.complete -ValueOwner 'complete manifest.complete'
    Assert-JsonDateString `
        -Value $Manifest.capturedAtUtc -ValueOwner 'complete manifest.capturedAtUtc'
    Assert-JsonBoolean `
        -Value $Manifest.productionApproved `
        -ValueOwner 'complete manifest.productionApproved'
    if (($Manifest.schema -cne $CompleteSchema) -or
        (-not [bool]$Manifest.complete) -or
        [bool]$Manifest.productionApproved) {
        Throw-BundleBlocker 'complete manifest header differs or contains approval.'
    }

    Assert-ExactObjectKeys `
        -Object $Manifest.repository `
        -Expected @(
            'root', 'headObserved', 'headPinnedForDecision', 'checkpointCommit') `
        -ObjectOwner 'complete manifest.repository'
    Assert-JsonString -Value $Manifest.repository.root -ValueOwner 'repository.root'
    Assert-GitObjectIdText `
        -Value $Manifest.repository.headObserved -ValueOwner 'repository.headObserved'
    Assert-JsonBoolean `
        -Value $Manifest.repository.headPinnedForDecision `
        -ValueOwner 'repository.headPinnedForDecision'
    Assert-GitObjectIdText `
        -Value $Manifest.repository.checkpointCommit `
        -ValueOwner 'repository.checkpointCommit'
    if ((-not [IO.Path]::IsPathRooted([string]$Manifest.repository.root)) -or
        [bool]$Manifest.repository.headPinnedForDecision -or
        ([string]$Manifest.repository.checkpointCommit -cne $CheckpointCommit)) {
        Throw-BundleBlocker 'complete manifest repository contract differs.'
    }

    Assert-ExactObjectKeys `
        -Object $Manifest.powershellEngine `
        -Expected @(
            'psEdition', 'major', 'version', 'minimumSupportedMajor',
            'directoryNtfsStreamEnumerationRequired',
            'productionFinalizationSupported') `
        -ObjectOwner 'complete manifest.powershellEngine'
    Assert-JsonString `
        -Value $Manifest.powershellEngine.psEdition -ValueOwner 'powershellEngine.psEdition'
    Assert-JsonInteger `
        -Value $Manifest.powershellEngine.major -ValueOwner 'powershellEngine.major'
    Assert-JsonString `
        -Value $Manifest.powershellEngine.version -ValueOwner 'powershellEngine.version'
    Assert-JsonInteger `
        -Value $Manifest.powershellEngine.minimumSupportedMajor `
        -ValueOwner 'powershellEngine.minimumSupportedMajor'
    Assert-JsonBoolean `
        -Value $Manifest.powershellEngine.directoryNtfsStreamEnumerationRequired `
        -ValueOwner 'powershellEngine.directoryNtfsStreamEnumerationRequired'
    Assert-JsonBoolean `
        -Value $Manifest.powershellEngine.productionFinalizationSupported `
        -ValueOwner 'powershellEngine.productionFinalizationSupported'
    if (($Manifest.powershellEngine.psEdition -cne 'Core') -or
        ([int]$Manifest.powershellEngine.major -lt 7) -or
        ([int]$Manifest.powershellEngine.minimumSupportedMajor -ne 7) -or
        (-not [bool]$Manifest.powershellEngine.directoryNtfsStreamEnumerationRequired) -or
        (-not [bool]$Manifest.powershellEngine.productionFinalizationSupported)) {
        Throw-BundleBlocker 'complete manifest PowerShell engine contract differs.'
    }

    Assert-ReportListShape `
        -Reports $Manifest.trustedArtifacts `
        -ExpectedCount 5 `
        -ExpectedKeys @(
            'owner', 'relativePath', 'bytes', 'sha256', 'gitTrackedAndHeadClean') `
        -ReportsOwner 'complete manifest.trustedArtifacts' `
        -NameProperty 'relativePath'
    foreach ($report in @($Manifest.trustedArtifacts)) {
        Assert-JsonString -Value $report.owner -ValueOwner 'trusted artifact owner'
        Assert-JsonString `
            -Value $report.relativePath -ValueOwner 'trusted artifact relativePath'
        Assert-SafeRelativePath `
            -RelativePath ([string]$report.relativePath) `
            -PathOwner 'trusted artifact relativePath'
        Assert-JsonInteger -Value $report.bytes -ValueOwner 'trusted artifact bytes'
        Assert-Sha256Text -Value $report.sha256 -ValueOwner 'trusted artifact sha256'
        Assert-JsonBoolean `
            -Value $report.gitTrackedAndHeadClean `
            -ValueOwner 'trusted artifact gitTrackedAndHeadClean'
        if (([long]$report.bytes -le 0) -or
            (-not [bool]$report.gitTrackedAndHeadClean)) {
            Throw-BundleBlocker 'trusted artifact report is not a positive clean identity.'
        }
    }
    if (-not $SkipPinnedProductionIdentities) {
        for ($index = 0; $index -lt $PinnedTrustedArtifacts.Count; $index++) {
            $actual = $Manifest.trustedArtifacts[$index]
            $expected = $PinnedTrustedArtifacts[$index]
            if (($actual.owner -cne $expected.owner) -or
                ($actual.relativePath -cne $expected.relativePath) -or
                ([long]$actual.bytes -ne [long]$expected.bytes) -or
                ([string]$actual.sha256 -cne [string]$expected.sha256)) {
                Throw-BundleBlocker (
                    "trusted artifact pinned tuple differs at index $index.")
            }
        }
    }

    Assert-ExactObjectKeys `
        -Object $Manifest.finalizer `
        -Expected @(
            'relativePath', 'bytes', 'sha256', 'headBlobOid',
            'gitTrackedAndHeadClean') `
        -ObjectOwner 'complete manifest.finalizer'
    Assert-JsonString `
        -Value $Manifest.finalizer.relativePath -ValueOwner 'finalizer.relativePath'
    Assert-JsonInteger -Value $Manifest.finalizer.bytes -ValueOwner 'finalizer.bytes'
    Assert-Sha256Text -Value $Manifest.finalizer.sha256 -ValueOwner 'finalizer.sha256'
    Assert-GitObjectIdText `
        -Value $Manifest.finalizer.headBlobOid -ValueOwner 'finalizer.headBlobOid'
    Assert-JsonBoolean `
        -Value $Manifest.finalizer.gitTrackedAndHeadClean `
        -ValueOwner 'finalizer.gitTrackedAndHeadClean'
    if (($Manifest.finalizer.relativePath -cne $FinalizerRelativePath) -or
        ([long]$Manifest.finalizer.bytes -le 0) -or
        (-not [bool]$Manifest.finalizer.gitTrackedAndHeadClean)) {
        Throw-BundleBlocker 'complete manifest finalizer contract differs.'
    }
    if ((-not $SkipPinnedProductionIdentities) -and
        (([long]$Manifest.finalizer.bytes -ne $KnownFinalizerBytes) -or
            ([string]$Manifest.finalizer.sha256 -cne $KnownFinalizerSha256) -or
            ([string]$Manifest.finalizer.headBlobOid -cne
                $KnownFinalizerBlobOid))) {
        Throw-BundleBlocker 'complete manifest finalizer pinned tuple differs.'
    }

    Assert-ExactObjectKeys `
        -Object $Manifest.baseline `
        -Expected @('relativePath', 'inputIdentityCount', 'currentInputs') `
        -ObjectOwner 'complete manifest.baseline'
    Assert-JsonString `
        -Value $Manifest.baseline.relativePath -ValueOwner 'baseline.relativePath'
    Assert-JsonInteger `
        -Value $Manifest.baseline.inputIdentityCount `
        -ValueOwner 'baseline.inputIdentityCount'
    if (($Manifest.baseline.relativePath -cne $BaselineRelativePath) -or
        ([int]$Manifest.baseline.inputIdentityCount -ne 10)) {
        Throw-BundleBlocker 'complete manifest baseline header differs.'
    }
    Assert-ReportListShape `
        -Reports $Manifest.baseline.currentInputs `
        -ExpectedCount 10 `
        -ExpectedKeys @(
            'relativePath', 'bytes', 'sha256', 'exactBaselineInputIdentity') `
        -ReportsOwner 'complete manifest.baseline.currentInputs' `
        -NameProperty 'relativePath'
    foreach ($inputReport in @($Manifest.baseline.currentInputs)) {
        Assert-JsonString `
            -Value $inputReport.relativePath -ValueOwner 'baseline input relativePath'
        Assert-SafeRelativePath `
            -RelativePath ([string]$inputReport.relativePath) `
            -PathOwner 'baseline input relativePath'
        Assert-JsonInteger -Value $inputReport.bytes -ValueOwner 'baseline input bytes'
        Assert-Sha256Text -Value $inputReport.sha256 -ValueOwner 'baseline input sha256'
        Assert-JsonBoolean `
            -Value $inputReport.exactBaselineInputIdentity `
            -ValueOwner 'baseline input exactBaselineInputIdentity'
        if (([long]$inputReport.bytes -le 0) -or
            (-not [bool]$inputReport.exactBaselineInputIdentity)) {
            Throw-BundleBlocker 'baseline input identity report differs.'
        }
    }

    Assert-ExactObjectKeys `
        -Object $Manifest.log `
        -Expected @(
            'path', 'baselinePrefixBytes', 'baselinePrefixSha256',
            'frozenEndOffset', 'frozenFullSha256', 'tailAppendPolicy') `
        -ObjectOwner 'complete manifest.log'
    Assert-JsonString -Value $Manifest.log.path -ValueOwner 'log.path'
    Assert-JsonInteger `
        -Value $Manifest.log.baselinePrefixBytes -ValueOwner 'log.baselinePrefixBytes'
    Assert-Sha256Text `
        -Value $Manifest.log.baselinePrefixSha256 `
        -ValueOwner 'log.baselinePrefixSha256'
    Assert-JsonInteger `
        -Value $Manifest.log.frozenEndOffset -ValueOwner 'log.frozenEndOffset'
    Assert-Sha256Text `
        -Value $Manifest.log.frozenFullSha256 -ValueOwner 'log.frozenFullSha256'
    Assert-JsonString `
        -Value $Manifest.log.tailAppendPolicy -ValueOwner 'log.tailAppendPolicy'
    if (([long]$Manifest.log.baselinePrefixBytes -lt 0) -or
        ([long]$Manifest.log.frozenEndOffset -le
            [long]$Manifest.log.baselinePrefixBytes) -or
        ($Manifest.log.tailAppendPolicy -cne
            'forbidden-until-atomic-publish-full-length-and-sha-must-remain-exact')) {
        Throw-BundleBlocker 'complete manifest log contract differs.'
    }
    if ((-not $SkipPinnedProductionIdentities) -and
        (([long]$Manifest.log.baselinePrefixBytes -ne $KnownBaselinePrefixBytes) -or
            ([string]$Manifest.log.baselinePrefixSha256 -cne
                $KnownBaselinePrefixSha256))) {
        Throw-BundleBlocker 'complete manifest baseline log prefix tuple differs.'
    }
}

function Assert-IsolatedSessionContract {
    param([Parameter(Mandatory = $true)]$Session)
    Assert-ExactObjectKeys `
        -Object $Session `
        -Expected @(
            'sessionPid', 'loadTid', 'rebuildTid', 'closeTid', 'startLineIndex',
            'preStartPrologue', 'loadLineIndex', 'loadResultLineIndex',
            'loadTerminalLineIndex', 'rebuildLineIndex', 'rebuildResultLineIndex',
            'rebuildTerminalLineIndex', 'doExitLineIndex', 'closeLineIndex',
            'closeTerminalLineIndex', 'exitDoneLineIndex', 'exactSessionCount',
            'exactRebuildCount', 'loadRestorationCommands', 'commandTerminalLedger',
            'knownLoadErrors', 'prohibitedCommandCount', 'cInvalidArgExceptionCount',
            'rebuildErrorCount') `
        -ObjectOwner 'complete manifest.isolatedSession'
    foreach ($name in @(
            'sessionPid', 'loadTid', 'rebuildTid', 'closeTid', 'startLineIndex',
            'loadLineIndex', 'loadResultLineIndex', 'loadTerminalLineIndex',
            'rebuildLineIndex', 'rebuildResultLineIndex', 'rebuildTerminalLineIndex',
            'doExitLineIndex', 'closeLineIndex', 'closeTerminalLineIndex',
            'exitDoneLineIndex', 'exactSessionCount', 'exactRebuildCount',
            'prohibitedCommandCount', 'cInvalidArgExceptionCount', 'rebuildErrorCount')) {
        Assert-JsonInteger -Value $Session.$name -ValueOwner "isolatedSession.$name"
    }
    foreach ($name in @(
            'preStartPrologue', 'loadRestorationCommands', 'commandTerminalLedger',
            'knownLoadErrors')) {
        if (-not (Test-IsJsonArray -Value $Session.$name)) {
            Throw-BundleBlocker "isolatedSession.$name is not a JSON array."
        }
    }
    if (([int]$Session.exactSessionCount -ne 1) -or
        ([int]$Session.exactRebuildCount -ne 1) -or
        ([int]$Session.prohibitedCommandCount -ne 0) -or
        ([int]$Session.cInvalidArgExceptionCount -ne 0) -or
        ([int]$Session.rebuildErrorCount -ne 0) -or
        (@($Session.knownLoadErrors).Count -gt 1)) {
        Throw-BundleBlocker 'isolated session contains a prohibited or failed action.'
    }
}

function Assert-ToolContract {
    param([Parameter(Mandatory = $true)]$Tools)
    Assert-ExactObjectKeys `
        -Object $Tools `
        -Expected @('converter', 'c78VerifyBuild', 'comparator') `
        -ObjectOwner 'complete manifest.tools'
    Assert-ExactObjectKeys `
        -Object $Tools.converter `
        -Expected @('exitCode', 'outputLines') `
        -ObjectOwner 'complete manifest.tools.converter'
    Assert-ExactObjectKeys `
        -Object $Tools.c78VerifyBuild `
        -Expected @('exitCode', 'runFullStatic', 'outputLines') `
        -ObjectOwner 'complete manifest.tools.c78VerifyBuild'
    Assert-ExactObjectKeys `
        -Object $Tools.comparator `
        -Expected @('exitCode', 'disposition', 'outputFileName') `
        -ObjectOwner 'complete manifest.tools.comparator'
    foreach ($value in @($Tools.converter.exitCode, $Tools.c78VerifyBuild.exitCode,
            $Tools.comparator.exitCode)) {
        Assert-JsonInteger -Value $value -ValueOwner 'tool exitCode'
    }
    if ((-not (Test-IsJsonArray -Value $Tools.converter.outputLines)) -or
        (-not (Test-IsJsonArray -Value $Tools.c78VerifyBuild.outputLines))) {
        Throw-BundleBlocker 'tool outputLines are not JSON arrays.'
    }
    Assert-JsonBoolean `
        -Value $Tools.c78VerifyBuild.runFullStatic `
        -ValueOwner 'tools.c78VerifyBuild.runFullStatic'
    Assert-JsonString `
        -Value $Tools.comparator.disposition `
        -ValueOwner 'tools.comparator.disposition'
    Assert-JsonString `
        -Value $Tools.comparator.outputFileName `
        -ValueOwner 'tools.comparator.outputFileName'
    if (([int]$Tools.converter.exitCode -ne 0) -or
        ([int]$Tools.c78VerifyBuild.exitCode -ne 0) -or
        [bool]$Tools.c78VerifyBuild.runFullStatic -or
        ($Tools.comparator.outputFileName -cne $ComparisonName)) {
        Throw-BundleBlocker 'tool execution contract differs.'
    }
}

function Assert-PublicationContract {
    param([Parameter(Mandatory = $true)]$Publication)
    Assert-ExactObjectKeys `
        -Object $Publication `
        -Expected @(
            'stagingDirectoryName', 'finalDirectoryName',
            'finalDirectoryAtomicMoveRequired', 'existingOutputOverwriteAllowed',
            'retryPolicy', 'completeManifestWrittenLast') `
        -ObjectOwner 'complete manifest.publication'
    foreach ($name in @('stagingDirectoryName', 'finalDirectoryName', 'retryPolicy')) {
        Assert-JsonString -Value $Publication.$name -ValueOwner "publication.$name"
    }
    foreach ($name in @(
            'finalDirectoryAtomicMoveRequired', 'existingOutputOverwriteAllowed',
            'completeManifestWrittenLast')) {
        Assert-JsonBoolean -Value $Publication.$name -ValueOwner "publication.$name"
    }
    if (([string]$Publication.stagingDirectoryName -cnotmatch
            '^\.finalize-stage-[0-9a-f]{32}$') -or
        ($Publication.finalDirectoryName -cne $FinalDirectoryName) -or
        (-not [bool]$Publication.finalDirectoryAtomicMoveRequired) -or
        [bool]$Publication.existingOutputOverwriteAllowed -or
        (-not [bool]$Publication.completeManifestWrittenLast) -or
        ($Publication.retryPolicy -cne
            'failed exact-owned current stage is removed; stale/ambiguous/final bundles require manual review')) {
        Throw-BundleBlocker 'publication contract differs or permits overwrite.'
    }
}

function Assert-RegeneratedOutputContract {
    param(
        [Parameter(Mandatory = $true)]$RegeneratedOutputs,
        [Parameter(Mandatory = $true)]$Artifacts
    )
    Assert-ExactObjectKeys `
        -Object $RegeneratedOutputs `
        -Expected @(
            'classes', 'networks', 'converterManifestMatchedSnapshots',
            'c78VerifierAcceptedManifestAndCurrentOutputs',
            'comparatorMatchedClassesSnapshot',
            'finalProductionRehashMatchedSnapshots') `
        -ObjectOwner 'complete manifest.regeneratedOutputs'
    foreach ($name in @('classes', 'networks')) {
        Assert-ExactObjectKeys `
            -Object $RegeneratedOutputs.$name `
            -Expected @('sourceRelativePath', 'snapshotFileName', 'bytes', 'sha256') `
            -ObjectOwner "regeneratedOutputs.$name"
    }
    $definitions = @(
        [pscustomobject]@{
            name = 'classes'
            source = $ClassesRelativePath
            snapshot = $ClassesSnapshotName
        },
        [pscustomobject]@{
            name = 'networks'
            source = $NetworksRelativePath
            snapshot = $NetworksSnapshotName
        })
    foreach ($definition in $definitions) {
        $report = $RegeneratedOutputs.($definition.name)
        Assert-JsonString `
            -Value $report.sourceRelativePath `
            -ValueOwner "regeneratedOutputs.$($definition.name).sourceRelativePath"
        Assert-JsonString `
            -Value $report.snapshotFileName `
            -ValueOwner "regeneratedOutputs.$($definition.name).snapshotFileName"
        Assert-JsonInteger `
            -Value $report.bytes -ValueOwner "regeneratedOutputs.$($definition.name).bytes"
        Assert-Sha256Text `
            -Value $report.sha256 -ValueOwner "regeneratedOutputs.$($definition.name).sha256"
        $artifact = $Artifacts[$definition.snapshot]
        if (($report.sourceRelativePath -cne $definition.source) -or
            ($report.snapshotFileName -cne $definition.snapshot) -or
            ([long]$report.bytes -ne [long]$artifact.bytes) -or
            ([string]$report.sha256 -cne [string]$artifact.sha256)) {
            Throw-BundleBlocker (
                "regeneratedOutputs.$($definition.name) does not bind its snapshot.")
        }
    }
    foreach ($name in @(
            'converterManifestMatchedSnapshots',
            'c78VerifierAcceptedManifestAndCurrentOutputs',
            'comparatorMatchedClassesSnapshot',
            'finalProductionRehashMatchedSnapshots')) {
        Assert-JsonBoolean `
            -Value $RegeneratedOutputs.$name `
            -ValueOwner "regeneratedOutputs.$name"
        if (-not [bool]$RegeneratedOutputs.$name) {
            Throw-BundleBlocker "regeneratedOutputs.$name is not true."
        }
    }
}

function Assert-CompleteDecisionContract {
    param([Parameter(Mandatory = $true)]$Decision)
    Assert-ExactObjectKeys `
        -Object $Decision `
        -Expected @(
            'disposition', 'exitCode', 'checkpointReproduced', 'known6EReproduced',
            'productionApproved', 'semanticEquivalenceClaimedForOpaqueDrift',
            'staticReplayPermitted', 'onlineRuntimeQualificationPermitted') `
        -ObjectOwner 'complete manifest.decision'
    Assert-JsonString -Value $Decision.disposition -ValueOwner 'decision.disposition'
    Assert-JsonInteger -Value $Decision.exitCode -ValueOwner 'decision.exitCode'
    foreach ($name in @(
            'checkpointReproduced', 'known6EReproduced', 'productionApproved',
            'semanticEquivalenceClaimedForOpaqueDrift', 'staticReplayPermitted',
            'onlineRuntimeQualificationPermitted')) {
        Assert-JsonBoolean -Value $Decision.$name -ValueOwner "decision.$name"
    }
    if ([bool]$Decision.productionApproved -or
        [bool]$Decision.semanticEquivalenceClaimedForOpaqueDrift -or
        [bool]$Decision.onlineRuntimeQualificationPermitted) {
        Throw-BundleBlocker 'complete decision contains a production/runtime approval.'
    }
    switch ([int]$Decision.exitCode) {
        0 {
            if (($Decision.disposition -cne
                    'CHECKPOINT_24402BFA_REPRODUCED_STATIC_REPLAY_ONLY') -or
                (-not [bool]$Decision.checkpointReproduced) -or
                [bool]$Decision.known6EReproduced -or
                (-not [bool]$Decision.staticReplayPermitted)) {
                Throw-BundleBlocker 'exit-0 complete decision truth table differs.'
            }
        }
        2 {
            if (($Decision.disposition -cne
                    'KNOWN_6E115876_REPRODUCIBLE_REVIEW_ONLY') -or
                [bool]$Decision.checkpointReproduced -or
                (-not [bool]$Decision.known6EReproduced) -or
                [bool]$Decision.staticReplayPermitted) {
                Throw-BundleBlocker 'exit-2 complete decision truth table differs.'
            }
        }
        3 {
            if (($Decision.disposition -cne 'UNSTABLE_THIRD_CLASSES_HASH_STOP') -or
                [bool]$Decision.checkpointReproduced -or
                [bool]$Decision.known6EReproduced -or
                [bool]$Decision.staticReplayPermitted) {
                Throw-BundleBlocker 'exit-3 complete decision truth table differs.'
            }
        }
        default {
            Throw-BundleBlocker 'complete decision exit must be 0, 2, or 3.'
        }
    }
}

function Assert-ComparatorRecordIdentityShape {
    param(
        [Parameter(Mandatory = $true)]$Identity,
        [Parameter(Mandatory = $true)][long]$ArtifactBytes,
        [Parameter(Mandatory = $true)][string]$IdentityOwner
    )
    Assert-ExactObjectKeys `
        -Object $Identity `
        -Expected @('startOffset', 'endOffsetExclusive', 'sourceOffset', 'bytes', 'sha256') `
        -ObjectOwner $IdentityOwner
    foreach ($name in @('startOffset', 'endOffsetExclusive', 'sourceOffset', 'bytes')) {
        Assert-JsonInteger -Value $Identity.$name -ValueOwner "$IdentityOwner.$name"
    }
    Assert-UppercaseSha256Text `
        -Value $Identity.sha256 -ValueOwner "$IdentityOwner.sha256"
    if (([long]$Identity.startOffset -lt 0) -or
        ([long]$Identity.endOffsetExclusive -le [long]$Identity.startOffset) -or
        ([long]$Identity.endOffsetExclusive -gt $ArtifactBytes) -or
        ([long]$Identity.bytes -ne
            ([long]$Identity.endOffsetExclusive - [long]$Identity.startOffset)) -or
        ([long]$Identity.sourceOffset -lt 0) -or
        ([long]$Identity.sourceOffset -ge [long]$Identity.bytes)) {
        Throw-BundleBlocker "$IdentityOwner offset/length contract differs."
    }
}

function Assert-ComparatorRecordShape {
    param(
        [Parameter(Mandatory = $true)]$Record,
        [Parameter(Mandatory = $true)][long]$CheckpointBytes,
        [Parameter(Mandatory = $true)][long]$CandidateBytes,
        [Parameter(Mandatory = $true)][string]$RecordOwner,
        [switch]$Protected
    )
    $keys = @('owner', 'sourcePath', 'parser', 'exact', 'checkpoint', 'candidate')
    if ($Protected) { $keys += 'legacyWindowExact' }
    Assert-ExactObjectKeys -Object $Record -Expected $keys -ObjectOwner $RecordOwner
    foreach ($name in @('owner', 'sourcePath', 'parser')) {
        Assert-JsonString -Value $Record.$name -ValueOwner "$RecordOwner.$name"
        if ([string]::IsNullOrWhiteSpace([string]$Record.$name)) {
            Throw-BundleBlocker "$RecordOwner.$name is empty."
        }
    }
    Assert-JsonBoolean -Value $Record.exact -ValueOwner "$RecordOwner.exact"
    if ($Protected) {
        Assert-JsonBoolean `
            -Value $Record.legacyWindowExact `
            -ValueOwner "$RecordOwner.legacyWindowExact"
    }
    Assert-ComparatorRecordIdentityShape `
        -Identity $Record.checkpoint -ArtifactBytes $CheckpointBytes `
        -IdentityOwner "$RecordOwner.checkpoint"
    Assert-ComparatorRecordIdentityShape `
        -Identity $Record.candidate -ArtifactBytes $CandidateBytes `
        -IdentityOwner "$RecordOwner.candidate"
    if ([bool]$Record.exact -and
        (([long]$Record.checkpoint.bytes -ne [long]$Record.candidate.bytes) -or
            ([string]$Record.checkpoint.sha256 -cne
                [string]$Record.candidate.sha256))) {
        Throw-BundleBlocker "$RecordOwner exact identity differs."
    }
}

function Assert-ComparatorRecordSetShape {
    param(
        [Parameter(Mandatory = $true)]$RecordSet,
        [Parameter(Mandatory = $true)][int]$ExpectedCount,
        [Parameter(Mandatory = $true)][long]$CheckpointBytes,
        [Parameter(Mandatory = $true)][long]$CandidateBytes,
        [Parameter(Mandatory = $true)][string]$SetOwner,
        [switch]$Protected
    )
    Assert-ExactObjectKeys `
        -Object $RecordSet -Expected @('allEqual', 'records') -ObjectOwner $SetOwner
    Assert-JsonBoolean -Value $RecordSet.allEqual -ValueOwner "$SetOwner.allEqual"
    if ((-not (Test-IsJsonArray -Value $RecordSet.records)) -or
        (@($RecordSet.records).Count -ne $ExpectedCount)) {
        Throw-BundleBlocker "$SetOwner records count/type differs."
    }
    $allExact = $true
    for ($index = 0; $index -lt $ExpectedCount; $index++) {
        $record = $RecordSet.records[$index]
        Assert-ComparatorRecordShape `
            -Record $record -CheckpointBytes $CheckpointBytes `
            -CandidateBytes $CandidateBytes `
            -RecordOwner "$SetOwner.records[$index]" -Protected:$Protected
        if (-not [bool]$record.exact) { $allExact = $false }
        if ($Protected -and (-not [bool]$record.legacyWindowExact)) {
            $allExact = $false
        }
    }
    if ([bool]$RecordSet.allEqual -ne $allExact) {
        Throw-BundleBlocker "$SetOwner allEqual differs from its records."
    }
}

function Assert-ComparatorPreviewShape {
    param(
        [Parameter(Mandatory = $true)]$Preview,
        [Parameter(Mandatory = $true)][long]$RunBytes,
        [Parameter(Mandatory = $true)][string]$PreviewOwner
    )
    Assert-ExactObjectKeys `
        -Object $Preview -Expected @('hex', 'previewBytes', 'truncated') `
        -ObjectOwner $PreviewOwner
    Assert-JsonString -Value $Preview.hex -ValueOwner "$PreviewOwner.hex"
    Assert-JsonInteger `
        -Value $Preview.previewBytes -ValueOwner "$PreviewOwner.previewBytes"
    Assert-JsonBoolean `
        -Value $Preview.truncated -ValueOwner "$PreviewOwner.truncated"
    $expectedPreviewBytes = [Math]::Min(32L, $RunBytes)
    if (([long]$Preview.previewBytes -ne $expectedPreviewBytes) -or
        ([string]$Preview.hex -cnotmatch '^[0-9A-F]*$') -or
        ([string]$Preview.hex).Length -ne (2 * $expectedPreviewBytes) -or
        ([bool]$Preview.truncated -ne ($RunBytes -gt 32))) {
        Throw-BundleBlocker "$PreviewOwner content contract differs."
    }
}

function Assert-ComparatorOwnerMappingShape {
    param(
        [Parameter(Mandatory = $true)]$Mapping,
        [Parameter(Mandatory = $true)][long]$ArtifactBytes,
        [Parameter(Mandatory = $true)][string]$MappingOwner
    )
    Assert-ExactObjectKeys `
        -Object $Mapping `
        -Expected @(
            'owner', 'sourcePath', 'recordStart', 'recordEndExclusive',
            'overlapStart', 'overlapBytes') `
        -ObjectOwner $MappingOwner
    foreach ($name in @('owner', 'sourcePath')) {
        Assert-JsonString -Value $Mapping.$name -ValueOwner "$MappingOwner.$name"
    }
    foreach ($name in @(
            'recordStart', 'recordEndExclusive', 'overlapStart', 'overlapBytes')) {
        Assert-JsonInteger -Value $Mapping.$name -ValueOwner "$MappingOwner.$name"
    }
    if (([long]$Mapping.recordStart -lt 0) -or
        ([long]$Mapping.recordEndExclusive -le [long]$Mapping.recordStart) -or
        ([long]$Mapping.recordEndExclusive -gt $ArtifactBytes) -or
        ([long]$Mapping.overlapStart -lt [long]$Mapping.recordStart) -or
        ([long]$Mapping.overlapStart -gt [long]$Mapping.recordEndExclusive) -or
        ([long]$Mapping.overlapBytes -lt 0) -or
        (([long]$Mapping.overlapStart + [long]$Mapping.overlapBytes) -gt
            [long]$Mapping.recordEndExclusive)) {
        Throw-BundleBlocker "$MappingOwner range contract differs."
    }
}

function Assert-ComparatorNestedShape {
    param([Parameter(Mandatory = $true)]$Comparison)
    $checkpointBytes = [long]$Comparison.checkpoint.rawBytes
    $candidateBytes = [long]$Comparison.candidate.rawBytes
    Assert-ExactObjectKeys `
        -Object $Comparison.recordParser `
        -Expected @(
            'convention', 'latin1ByteOffsetPreserving', 'sourceMarkerBoundary',
            'trueHeaderBoundary', 'sourcePathSegmentsDiagnosticOnly',
            'checkpointOwnerRecordCount', 'candidateOwnerRecordCount',
            'headerSourceInventory', 'firstSpecialRecord') `
        -ObjectOwner 'comparison.recordParser'
    foreach ($name in @('convention', 'sourceMarkerBoundary', 'trueHeaderBoundary')) {
        Assert-JsonString `
            -Value $Comparison.recordParser.$name `
            -ValueOwner "comparison.recordParser.$name"
    }
    foreach ($name in @(
            'latin1ByteOffsetPreserving', 'sourcePathSegmentsDiagnosticOnly')) {
        Assert-JsonBoolean `
            -Value $Comparison.recordParser.$name `
            -ValueOwner "comparison.recordParser.$name"
    }
    foreach ($name in @('checkpointOwnerRecordCount', 'candidateOwnerRecordCount')) {
        Assert-JsonInteger `
            -Value $Comparison.recordParser.$name `
            -ValueOwner "comparison.recordParser.$name"
    }
    if (($Comparison.recordParser.convention -cne
            'first-special-record-then-aa03-header-to-next-header-or-eof') -or
        (-not [bool]$Comparison.recordParser.latin1ByteOffsetPreserving) -or
        ($Comparison.recordParser.sourceMarkerBoundary -cne
            'path-length-le24-plus-aa') -or
        ($Comparison.recordParser.trueHeaderBoundary -cne
            'aa-03-plus-class-name-length-le24-plus-aa-plus-class-name') -or
        (-not [bool]$Comparison.recordParser.sourcePathSegmentsDiagnosticOnly) -or
        ([int]$Comparison.recordParser.checkpointOwnerRecordCount -le 0) -or
        ([int]$Comparison.recordParser.candidateOwnerRecordCount -le 0)) {
        Throw-BundleBlocker 'comparison recordParser fixed contract differs.'
    }
    $inventory = $Comparison.recordParser.headerSourceInventory
    Assert-ExactObjectKeys `
        -Object $inventory `
        -Expected @(
            'exact', 'checkpointCount', 'candidateCount', 'firstMismatch',
            'comparedFields') `
        -ObjectOwner 'comparison.recordParser.headerSourceInventory'
    Assert-JsonBoolean -Value $inventory.exact -ValueOwner 'header inventory exact'
    foreach ($name in @('checkpointCount', 'candidateCount')) {
        Assert-JsonInteger -Value $inventory.$name -ValueOwner "header inventory $name"
    }
    if (($null -ne $inventory.firstMismatch) -and
        ($inventory.firstMismatch -isnot [string])) {
        Throw-BundleBlocker 'header inventory firstMismatch is not null/string.'
    }
    $expectedFields = @(
        'owner', 'sourcePath', 'headerOffset', 'recordEndOffset',
        'sourcePathOffset', 'sourceMarkerOffset', 'parser')
    if ((-not (Test-IsJsonArray -Value $inventory.comparedFields)) -or
        (@($inventory.comparedFields).Count -ne $expectedFields.Count)) {
        Throw-BundleBlocker 'header inventory comparedFields count/type differs.'
    }
    for ($index = 0; $index -lt $expectedFields.Count; $index++) {
        Assert-JsonString `
            -Value $inventory.comparedFields[$index] `
            -ValueOwner "header inventory comparedFields[$index]"
        if ($inventory.comparedFields[$index] -cne $expectedFields[$index]) {
            Throw-BundleBlocker 'header inventory comparedFields order differs.'
        }
    }
    if (([int]$inventory.checkpointCount -ne
            [int]$Comparison.recordParser.checkpointOwnerRecordCount) -or
        ([int]$inventory.candidateCount -ne
            [int]$Comparison.recordParser.candidateOwnerRecordCount) -or
        ([bool]$inventory.exact -ne ($null -eq $inventory.firstMismatch))) {
        Throw-BundleBlocker 'header inventory count/exact contract differs.'
    }
    Assert-ComparatorRecordShape `
        -Record $Comparison.recordParser.firstSpecialRecord `
        -CheckpointBytes $checkpointBytes -CandidateBytes $candidateBytes `
        -RecordOwner 'comparison.recordParser.firstSpecialRecord'
    Assert-ComparatorRecordSetShape `
        -RecordSet $Comparison.gateDTargetRecords -ExpectedCount 4 `
        -CheckpointBytes $checkpointBytes -CandidateBytes $candidateBytes `
        -SetOwner 'comparison.gateDTargetRecords'
    Assert-ComparatorRecordSetShape `
        -RecordSet $Comparison.protectedDependencyRecords -ExpectedCount 2 `
        -CheckpointBytes $checkpointBytes -CandidateBytes $candidateBytes `
        -SetOwner 'comparison.protectedDependencyRecords' -Protected

    $frozenExact = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $frozenFolded = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($owner in @($Comparison.comparison.frozenOpaqueOwners)) {
        Assert-JsonString -Value $owner -ValueOwner 'comparison frozen opaque owner'
        if ([string]::IsNullOrWhiteSpace([string]$owner) -or
            (-not $frozenExact.Add([string]$owner)) -or
            (-not $frozenFolded.Add([string]$owner))) {
            Throw-BundleBlocker 'comparison frozen owner is empty/duplicate/colliding.'
        }
    }
    for ($index = 0; $index -lt @($Comparison.changedCheckpointOwners).Count; $index++) {
        $owner = $Comparison.changedCheckpointOwners[$index]
        Assert-ExactObjectKeys `
            -Object $owner `
            -Expected @(
                'owner', 'sourcePath', 'diffRunCount', 'changedCheckpointBytes',
                'classification') `
            -ObjectOwner "comparison.changedCheckpointOwners[$index]"
        foreach ($name in @('owner', 'sourcePath', 'classification')) {
            Assert-JsonString `
                -Value $owner.$name `
                -ValueOwner "comparison.changedCheckpointOwners[$index].$name"
        }
        foreach ($name in @('diffRunCount', 'changedCheckpointBytes')) {
            Assert-JsonInteger `
                -Value $owner.$name `
                -ValueOwner "comparison.changedCheckpointOwners[$index].$name"
        }
        if (([int]$owner.diffRunCount -le 0) -or
            ([long]$owner.changedCheckpointBytes -lt 0) -or
            ($owner.classification -cnotin @(
                'frozen-opaque-vendor-owner-record',
                'contract-or-unclassified-owner-record'))) {
            Throw-BundleBlocker 'comparison changed owner value contract differs.'
        }
    }
    $unmappedRuns = 0
    for ($index = 0; $index -lt @($Comparison.diffRuns).Count; $index++) {
        $run = $Comparison.diffRuns[$index]
        Assert-ExactObjectKeys `
            -Object $run `
            -Expected @(
                'ordinal', 'checkpointStart', 'checkpointBytes', 'candidateStart',
                'candidateBytes', 'checkpointPreview', 'candidatePreview',
                'checkpointOwners', 'candidateOwners', 'mappingComplete') `
            -ObjectOwner "comparison.diffRuns[$index]"
        foreach ($name in @(
                'ordinal', 'checkpointStart', 'checkpointBytes', 'candidateStart',
                'candidateBytes')) {
            Assert-JsonInteger `
                -Value $run.$name -ValueOwner "comparison.diffRuns[$index].$name"
        }
        Assert-JsonBoolean `
            -Value $run.mappingComplete `
            -ValueOwner "comparison.diffRuns[$index].mappingComplete"
        if (([int]$run.ordinal -ne ($index + 1)) -or
            ([long]$run.checkpointStart -lt 0) -or
            ([long]$run.checkpointBytes -lt 0) -or
            (([long]$run.checkpointStart + [long]$run.checkpointBytes) -gt
                $checkpointBytes) -or
            ([long]$run.candidateStart -lt 0) -or
            ([long]$run.candidateBytes -lt 0) -or
            (([long]$run.candidateStart + [long]$run.candidateBytes) -gt
                $candidateBytes) -or
            (-not (Test-IsJsonArray -Value $run.checkpointOwners)) -or
            (-not (Test-IsJsonArray -Value $run.candidateOwners))) {
            Throw-BundleBlocker 'comparison diff run range/array contract differs.'
        }
        Assert-ComparatorPreviewShape `
            -Preview $run.checkpointPreview -RunBytes ([long]$run.checkpointBytes) `
            -PreviewOwner "comparison.diffRuns[$index].checkpointPreview"
        Assert-ComparatorPreviewShape `
            -Preview $run.candidatePreview -RunBytes ([long]$run.candidateBytes) `
            -PreviewOwner "comparison.diffRuns[$index].candidatePreview"
        for ($mappingIndex = 0;
            $mappingIndex -lt @($run.checkpointOwners).Count;
            $mappingIndex++) {
            Assert-ComparatorOwnerMappingShape `
                -Mapping $run.checkpointOwners[$mappingIndex] `
                -ArtifactBytes $checkpointBytes `
                -MappingOwner "comparison.diffRuns[$index].checkpointOwners[$mappingIndex]"
        }
        for ($mappingIndex = 0;
            $mappingIndex -lt @($run.candidateOwners).Count;
            $mappingIndex++) {
            Assert-ComparatorOwnerMappingShape `
                -Mapping $run.candidateOwners[$mappingIndex] `
                -ArtifactBytes $candidateBytes `
                -MappingOwner "comparison.diffRuns[$index].candidateOwners[$mappingIndex]"
        }
        $derivedComplete =
            (@($run.checkpointOwners).Count -gt 0) -and
            (@($run.candidateOwners).Count -gt 0)
        if ([bool]$run.mappingComplete -ne $derivedComplete) {
            Throw-BundleBlocker 'comparison diff run mappingComplete differs.'
        }
        if (-not [bool]$run.mappingComplete) { $unmappedRuns++ }
    }
    if (([int]$Comparison.comparison.unmappedRunCount -ne $unmappedRuns) -or
        ([long]$Comparison.comparison.lengthDelta -ne
            ($candidateBytes - $checkpointBytes)) -or
        ([bool]$Comparison.comparison.equalLength -ne
            ($candidateBytes -eq $checkpointBytes)) -or
        ([bool]$Comparison.comparison.changedByteCountDefined -ne
            [bool]$Comparison.comparison.equalLength)) {
        Throw-BundleBlocker 'comparison nested aggregate contract differs.'
    }
}

function Assert-ComparatorContract {
    param(
        [Parameter(Mandatory = $true)]$Comparison,
        [Parameter(Mandatory = $true)]$ComparisonArtifact,
        [Parameter(Mandatory = $true)]$CompleteManifest,
        [Parameter(Mandatory = $true)][string]$EvidenceRelativeRoot,
        [Parameter(Mandatory = $true)]$ClassesArtifact
    )
    [byte[]]$canonical = ConvertTo-ComparatorCanonicalJsonBytes -Value $Comparison
    if (-not (Test-ByteSequencesExact `
            -Actual $ComparisonArtifact.content -Expected $canonical)) {
        Throw-BundleBlocker 'comparison JSON is not exact comparator-canonical bytes.'
    }
    Assert-ExactObjectKeys `
        -Object $Comparison `
        -Expected @(
            'schema', 'decision', 'checkpoint', 'candidate', 'comparison',
            'recordParser', 'gateDTargetRecords', 'protectedDependencyRecords',
            'changedCheckpointOwners', 'diffRuns') `
        -ObjectOwner 'comparison report'
    Assert-JsonString -Value $Comparison.schema -ValueOwner 'comparison.schema'
    if ($Comparison.schema -cne $ComparisonSchema) {
        Throw-BundleBlocker 'comparison schema differs.'
    }
    Assert-ExactObjectKeys `
        -Object $Comparison.decision `
        -Expected @(
            'disposition', 'checkpointIdentityAccepted', 'approvalScope',
            'productionApproved', 'exactCheckpointMatch', 'semanticEquivalenceProven',
            'recordEqualityCannotApproveArtifact', 'exitCode') `
        -ObjectOwner 'comparison.decision'
    Assert-ExactObjectKeys `
        -Object $Comparison.checkpoint `
        -Expected @(
            'requested', 'kind', 'resolvedRevision', 'relativePath', 'blobOid',
            'rawBytes', 'sha256') `
        -ObjectOwner 'comparison.checkpoint'
    Assert-ExactObjectKeys `
        -Object $Comparison.candidate `
        -Expected @('path', 'rawBytes', 'sha256') `
        -ObjectOwner 'comparison.candidate'
    Assert-ExactObjectKeys `
        -Object $Comparison.comparison `
        -Expected @(
            'byteExact', 'equalLength', 'lengthDelta', 'alignment',
            'changedByteCountDefined', 'changedByteCount', 'contiguousRunCount',
            'checkpointChangedOwnerCount', 'unmappedRunCount',
            'changedOwnersAreFrozenOpaqueSubset', 'frozenOpaqueOwnerCount',
            'frozenOpaqueOwners', 'proprietaryFieldSemanticsDecoded') `
        -ObjectOwner 'comparison.comparison'

    foreach ($name in @('disposition', 'approvalScope')) {
        Assert-JsonString -Value $Comparison.decision.$name -ValueOwner "comparison.decision.$name"
    }
    foreach ($name in @(
            'checkpointIdentityAccepted', 'productionApproved', 'exactCheckpointMatch',
            'semanticEquivalenceProven', 'recordEqualityCannotApproveArtifact')) {
        Assert-JsonBoolean `
            -Value $Comparison.decision.$name -ValueOwner "comparison.decision.$name"
    }
    Assert-JsonInteger `
        -Value $Comparison.decision.exitCode -ValueOwner 'comparison.decision.exitCode'
    if (($Comparison.decision.approvalScope -cne 'checkpoint-byte-identity-only') -or
        [bool]$Comparison.decision.productionApproved -or
        (-not [bool]$Comparison.decision.recordEqualityCannotApproveArtifact)) {
        Throw-BundleBlocker 'comparison decision contains approval or a widened scope.'
    }

    foreach ($name in @('requested', 'kind', 'resolvedRevision', 'relativePath', 'blobOid', 'sha256')) {
        Assert-JsonString -Value $Comparison.checkpoint.$name -ValueOwner "comparison.checkpoint.$name"
    }
    Assert-JsonInteger `
        -Value $Comparison.checkpoint.rawBytes -ValueOwner 'comparison.checkpoint.rawBytes'
    if (($Comparison.checkpoint.requested -cne $CheckpointCommit) -or
        ($Comparison.checkpoint.kind -cne 'revision') -or
        ($Comparison.checkpoint.resolvedRevision -cne $CheckpointCommit) -or
        ($Comparison.checkpoint.relativePath -cne $ClassesRelativePath) -or
        ($Comparison.checkpoint.blobOid -cne $CheckpointBlobOid) -or
        ([long]$Comparison.checkpoint.rawBytes -ne $CheckpointClassesBytes) -or
        ($Comparison.checkpoint.sha256 -cne $CheckpointClassesSha256)) {
        Throw-BundleBlocker 'comparison checkpoint contract differs.'
    }

    Assert-JsonString -Value $Comparison.candidate.path -ValueOwner 'comparison.candidate.path'
    Assert-JsonInteger `
        -Value $Comparison.candidate.rawBytes -ValueOwner 'comparison.candidate.rawBytes'
    Assert-Sha256Text `
        -Value $Comparison.candidate.sha256 -ValueOwner 'comparison.candidate.sha256'
    $expectedCandidatePath =
        $EvidenceRelativeRoot.TrimEnd('/') + '/' +
        [string]$CompleteManifest.publication.stagingDirectoryName + '/' +
        $ClassesSnapshotName
    if (($Comparison.candidate.path -cne $expectedCandidatePath) -or
        ([long]$Comparison.candidate.rawBytes -ne [long]$ClassesArtifact.bytes) -or
        ([string]$Comparison.candidate.sha256 -cne [string]$ClassesArtifact.sha256)) {
        Throw-BundleBlocker (
            'comparison candidate does not bind the pre-move path and final snapshot identity.')
    }

    foreach ($name in @(
            'byteExact', 'equalLength', 'changedByteCountDefined',
            'changedOwnersAreFrozenOpaqueSubset', 'proprietaryFieldSemanticsDecoded')) {
        Assert-JsonBoolean -Value $Comparison.comparison.$name -ValueOwner "comparison.$name"
    }
    foreach ($name in @(
            'lengthDelta', 'contiguousRunCount', 'checkpointChangedOwnerCount',
            'unmappedRunCount', 'frozenOpaqueOwnerCount')) {
        Assert-JsonInteger -Value $Comparison.comparison.$name -ValueOwner "comparison.$name"
    }
    if ([bool]$Comparison.comparison.changedByteCountDefined) {
        Assert-JsonInteger `
            -Value $Comparison.comparison.changedByteCount `
            -ValueOwner 'comparison.changedByteCount'
    }
    elseif ($null -ne $Comparison.comparison.changedByteCount) {
        Throw-BundleBlocker 'undefined changedByteCount must be null.'
    }
    Assert-JsonString `
        -Value $Comparison.comparison.alignment -ValueOwner 'comparison.alignment'
    if (([bool]$Comparison.comparison.equalLength -and
            ($Comparison.comparison.alignment -cne 'equal-length-indexed')) -or
        ((-not [bool]$Comparison.comparison.equalLength) -and
            ($Comparison.comparison.alignment -cne 'bounded-common-prefix-suffix')) -or
        [bool]$Comparison.comparison.proprietaryFieldSemanticsDecoded -or
        (-not (Test-IsJsonArray -Value $Comparison.comparison.frozenOpaqueOwners)) -or
        ([int]$Comparison.comparison.frozenOpaqueOwnerCount -ne 36) -or
        ([int]$Comparison.comparison.frozenOpaqueOwnerCount -ne
            @($Comparison.comparison.frozenOpaqueOwners).Count) -or
        (-not (Test-IsJsonObject -Value $Comparison.recordParser)) -or
        (-not (Test-IsJsonObject -Value $Comparison.gateDTargetRecords)) -or
        (-not (Test-IsJsonObject -Value $Comparison.protectedDependencyRecords)) -or
        (-not (Test-IsJsonArray -Value $Comparison.changedCheckpointOwners)) -or
        (-not (Test-IsJsonArray -Value $Comparison.diffRuns)) -or
        ([int]$Comparison.comparison.checkpointChangedOwnerCount -ne
            @($Comparison.changedCheckpointOwners).Count) -or
        ([int]$Comparison.comparison.contiguousRunCount -ne @($Comparison.diffRuns).Count)) {
        Throw-BundleBlocker 'comparison structural count/type contract differs.'
    }
    Assert-ComparatorNestedShape -Comparison $Comparison

    $comparatorExit = [int]$Comparison.decision.exitCode
    if (($comparatorExit -ne [int]$CompleteManifest.tools.comparator.exitCode) -or
        ($Comparison.decision.disposition -cne
            $CompleteManifest.tools.comparator.disposition)) {
        Throw-BundleBlocker 'comparison decision differs from the tool report.'
    }
    switch ([int]$CompleteManifest.decision.exitCode) {
        0 {
            if (($comparatorExit -ne 0) -or
                ($Comparison.decision.disposition -cne 'EXACT_CHECKPOINT_MATCH') -or
                (-not [bool]$Comparison.decision.checkpointIdentityAccepted) -or
                (-not [bool]$Comparison.decision.exactCheckpointMatch) -or
                (-not [bool]$Comparison.decision.semanticEquivalenceProven) -or
                (-not [bool]$Comparison.comparison.byteExact) -or
                (-not [bool]$Comparison.comparison.equalLength) -or
                (-not [bool]$Comparison.comparison.changedByteCountDefined) -or
                ([long]$Comparison.comparison.lengthDelta -ne 0) -or
                (-not [bool]$Comparison.recordParser.headerSourceInventory.exact) -or
                (-not [bool]$Comparison.recordParser.firstSpecialRecord.exact) -or
                (-not [bool]$Comparison.gateDTargetRecords.allEqual) -or
                (-not [bool]$Comparison.protectedDependencyRecords.allEqual) -or
                ($ClassesArtifact.sha256 -cne $CheckpointClassesSha256) -or
                ([long]$Comparison.comparison.changedByteCount -ne 0) -or
                (@($Comparison.changedCheckpointOwners).Count -ne 0) -or
                (@($Comparison.diffRuns).Count -ne 0)) {
                Throw-BundleBlocker 'exit-0 comparison truth table differs.'
            }
        }
        2 {
            if (($comparatorExit -ne 2) -or
                ($Comparison.decision.disposition -cne
                    'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT') -or
                [bool]$Comparison.decision.checkpointIdentityAccepted -or
                [bool]$Comparison.decision.exactCheckpointMatch -or
                [bool]$Comparison.decision.semanticEquivalenceProven -or
                [bool]$Comparison.comparison.byteExact -or
                (-not [bool]$Comparison.comparison.equalLength) -or
                (-not [bool]$Comparison.comparison.changedByteCountDefined) -or
                ([long]$Comparison.comparison.lengthDelta -ne 0) -or
                (-not [bool]$Comparison.recordParser.headerSourceInventory.exact) -or
                (-not [bool]$Comparison.recordParser.firstSpecialRecord.exact) -or
                (-not [bool]$Comparison.gateDTargetRecords.allEqual) -or
                (-not [bool]$Comparison.protectedDependencyRecords.allEqual) -or
                ($ClassesArtifact.sha256 -cne $KnownRebuiltClassesSha256) -or
                ([long]$Comparison.comparison.changedByteCount -ne 99) -or
                ([int]$Comparison.comparison.contiguousRunCount -ne 58) -or
                ([int]$Comparison.comparison.checkpointChangedOwnerCount -ne 36) -or
                ([int]$Comparison.comparison.unmappedRunCount -ne 0)) {
                Throw-BundleBlocker 'exit-2 comparison truth table differs.'
            }
        }
        3 {
            if (($comparatorExit -notin @(2, 3)) -or
                (($comparatorExit -eq 2) -and
                    ($Comparison.decision.disposition -cne
                        'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT')) -or
                (($comparatorExit -eq 3) -and
                    ($Comparison.decision.disposition -cne
                        'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT')) -or
                [bool]$Comparison.decision.checkpointIdentityAccepted -or
                [bool]$Comparison.decision.exactCheckpointMatch -or
                [bool]$Comparison.decision.semanticEquivalenceProven -or
                [bool]$Comparison.comparison.byteExact -or
                ($ClassesArtifact.sha256 -ceq $CheckpointClassesSha256) -or
                ($ClassesArtifact.sha256 -ceq $KnownRebuiltClassesSha256)) {
                Throw-BundleBlocker 'exit-3 comparison truth table differs.'
            }
        }
    }
}

function Assert-RawManifestContract {
    param(
        [Parameter(Mandatory = $true)]$RawManifest,
        [Parameter(Mandatory = $true)]$CompleteManifest,
        [Parameter(Mandatory = $true)]$Artifacts
    )
    Assert-ExactObjectKeys `
        -Object $RawManifest `
        -Expected @(
            'Schema', 'EvidenceProfile', 'Provenance', 'CapturedAtUtc',
            'BaselineFileName', 'BaselineByteCount', 'BaselineSha256',
            'BaselinePrefixLength', 'BaselinePrefixSha256', 'SourceLogPath',
            'SourceStartOffset', 'SourceEndOffset', 'RawDeltaFileName',
            'RawDeltaByteCount', 'RawDeltaSha256', 'Encoding', 'SessionPid',
            'RebuildTid', 'TranscriptFileName', 'TranscriptByteCount',
            'TranscriptSha256', 'RegeneratedOutputs') `
        -ObjectOwner 'raw bounded manifest'
    foreach ($name in @(
            'Schema', 'EvidenceProfile', 'Provenance',
            'BaselineFileName', 'BaselineSha256', 'BaselinePrefixSha256',
            'SourceLogPath', 'RawDeltaFileName', 'RawDeltaSha256', 'Encoding',
            'TranscriptFileName', 'TranscriptSha256')) {
        Assert-JsonString -Value $RawManifest.$name -ValueOwner "raw manifest.$name"
    }
    Assert-JsonDateString `
        -Value $RawManifest.CapturedAtUtc -ValueOwner 'raw manifest.CapturedAtUtc'
    foreach ($name in @(
            'BaselineByteCount', 'BaselinePrefixLength', 'SourceStartOffset',
            'SourceEndOffset', 'RawDeltaByteCount', 'SessionPid', 'RebuildTid',
            'TranscriptByteCount')) {
        Assert-JsonInteger -Value $RawManifest.$name -ValueOwner "raw manifest.$name"
    }
    foreach ($name in @(
            'BaselineSha256', 'BaselinePrefixSha256', 'RawDeltaSha256',
            'TranscriptSha256')) {
        Assert-LowercaseSha256Text `
            -Value $RawManifest.$name -ValueOwner "raw manifest.$name"
    }
    $rawArtifact = $Artifacts[$RawDeltaName]
    $transcriptArtifact = $Artifacts[$TranscriptName]
    if (($RawManifest.Schema -cne $RawManifestSchema) -or
        ($RawManifest.EvidenceProfile -cne 'GateDVisualLayout') -or
        ($RawManifest.Provenance -cne
            'Exact byte slice from prefix-validated Lasal2.log') -or
        ($RawManifest.Encoding -cne 'UTF-8') -or
        ($RawManifest.BaselineFileName -cne
            [IO.Path]::GetFileName($BaselineRelativePath)) -or
        ([long]$RawManifest.BaselinePrefixLength -ne
            [long]$CompleteManifest.log.baselinePrefixBytes) -or
        ([string]$RawManifest.BaselinePrefixSha256 -cne
            ([string]$CompleteManifest.log.baselinePrefixSha256).ToLowerInvariant()) -or
        (-not [string]::Equals(
            [string]$RawManifest.SourceLogPath,
            [string]$CompleteManifest.log.path,
            [StringComparison]::OrdinalIgnoreCase)) -or
        ([long]$RawManifest.SourceStartOffset -ne
            [long]$CompleteManifest.log.baselinePrefixBytes) -or
        ([long]$RawManifest.SourceEndOffset -ne
            [long]$CompleteManifest.log.frozenEndOffset) -or
        (([long]$RawManifest.SourceEndOffset - [long]$RawManifest.SourceStartOffset) -ne
            [long]$RawManifest.RawDeltaByteCount) -or
        ($RawManifest.RawDeltaFileName -cne $RawDeltaName) -or
        ([long]$RawManifest.RawDeltaByteCount -ne [long]$rawArtifact.bytes) -or
        ([string]$RawManifest.RawDeltaSha256 -cne
            ([string]$rawArtifact.sha256).ToLowerInvariant()) -or
        ($RawManifest.TranscriptFileName -cne $TranscriptName) -or
        ([long]$RawManifest.TranscriptByteCount -ne [long]$transcriptArtifact.bytes) -or
        ([string]$RawManifest.TranscriptSha256 -cne
            ([string]$transcriptArtifact.sha256).ToLowerInvariant()) -or
        ([int]$RawManifest.SessionPid -ne
            [int]$CompleteManifest.isolatedSession.sessionPid) -or
        ([int]$RawManifest.RebuildTid -ne
            [int]$CompleteManifest.isolatedSession.rebuildTid)) {
        Throw-BundleBlocker 'raw bounded manifest file/session linkage differs.'
    }

    $baselineReports = @($CompleteManifest.trustedArtifacts | Where-Object {
            $_.relativePath -ceq $BaselineRelativePath
        })
    if (($baselineReports.Count -ne 1) -or
        ([long]$RawManifest.BaselineByteCount -ne
            [long]$baselineReports[0].bytes) -or
        ([string]$RawManifest.BaselineSha256 -cne
            ([string]$baselineReports[0].sha256).ToLowerInvariant())) {
        Throw-BundleBlocker 'raw manifest baseline identity differs from trustedArtifacts.'
    }

    if (-not (Test-IsJsonArray -Value $RawManifest.RegeneratedOutputs) -or
        @($RawManifest.RegeneratedOutputs).Count -ne 2) {
        Throw-BundleBlocker 'raw manifest RegeneratedOutputs count/type differs.'
    }
    $definitions = @(
        [pscustomobject]@{
            relativePath = $ClassesRelativePath
            snapshotName = $ClassesSnapshotName
        },
        [pscustomobject]@{
            relativePath = $NetworksRelativePath
            snapshotName = $NetworksSnapshotName
        })
    for ($index = 0; $index -lt 2; $index++) {
        $report = $RawManifest.RegeneratedOutputs[$index]
        Assert-ExactObjectKeys `
            -Object $report -Expected @('RelativePath', 'Bytes', 'Sha256') `
            -ObjectOwner "raw manifest.RegeneratedOutputs[$index]"
        Assert-JsonString -Value $report.RelativePath -ValueOwner 'raw output RelativePath'
        Assert-JsonInteger -Value $report.Bytes -ValueOwner 'raw output Bytes'
        Assert-LowercaseSha256Text `
            -Value $report.Sha256 -ValueOwner 'raw output Sha256'
        $artifact = $Artifacts[$definitions[$index].snapshotName]
        if (($report.RelativePath -cne $definitions[$index].relativePath) -or
            ([long]$report.Bytes -ne [long]$artifact.bytes) -or
            ([string]$report.Sha256 -cne
                ([string]$artifact.sha256).ToLowerInvariant())) {
            Throw-BundleBlocker "raw regenerated output $index does not bind its snapshot."
        }
    }
}

function Get-RawEvidenceLine {
    param(
        [object[]]$Lines,
        [Parameter(Mandatory = $true)][int]$Index,
        [Parameter(Mandatory = $true)][string]$LineOwner
    )
    if (($Index -lt 0) -or ($Index -ge $Lines.Count)) {
        Throw-BundleBlocker "$LineOwner index is outside the raw delta."
    }
    return [string]$Lines[$Index]
}

function Assert-RawLineProcessIdentity {
    param(
        [Parameter(Mandatory = $true)][string]$Line,
        [Parameter(Mandatory = $true)][int]$ExpectedPid,
        [Parameter(Mandatory = $true)][int]$ExpectedTid,
        [switch]$IgnoreTid,
        [Parameter(Mandatory = $true)][string]$LineOwner
    )
    $match = [regex]::Match($Line, ' P:(?<Pid>\d+) T:(?<Tid>\d+) ')
    if ((-not $match.Success) -or
        ([int]$match.Groups['Pid'].Value -ne $ExpectedPid) -or
        ((-not $IgnoreTid) -and
            ([int]$match.Groups['Tid'].Value -ne $ExpectedTid))) {
        Throw-BundleBlocker "$LineOwner PID/TID differs from its report."
    }
}

function Add-ExactRestorationCommandLineIndex {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()]
        [Collections.Generic.HashSet[int]]$CommandLineIndexes,
        [Parameter(Mandatory = $true)][int]$CommandLineIndex
    )
    if (-not $CommandLineIndexes.Add($CommandLineIndex)) {
        Throw-BundleBlocker (
            "load restoration report reuses commandLineIndex $CommandLineIndex.")
    }
}

function Get-ReplayedCommandTerminalLedger {
    param([Parameter(Mandatory = $true)][object[]]$Lines)
    $commands = New-Object Collections.Generic.List[object]
    $terminals = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        $line = [string]$Lines[$index]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $parsed = [regex]::Match(
            $line,
            ('^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*?' +
                '\((?<Level>INFO|WARN|ERROR|FATAL|DEBUG|NOTICE)\)\s+' +
                '(?<Source>[^\]]+)\]\s*(?<Body>.*)$'))
        $hasCommandToken = $line -match '(?i)Executing\s+command'
        $hasTerminalToken = $line -match
            '(?i)Last\s+command\s+(?:succeeded|failed)\.'
        if (-not ($hasCommandToken -or $hasTerminalToken)) { continue }
        if (-not $parsed.Success) {
            Throw-BundleBlocker 'raw command/terminal occurrence is not parseable.'
        }
        if (($parsed.Groups['Source'].Value -cne 'CmdProc') -or
            ($parsed.Groups['Level'].Value -cne 'INFO')) {
            Throw-BundleBlocker 'raw command/terminal is not exact CmdProc/INFO.'
        }
        if ($hasCommandToken) {
            $commandMatch = [regex]::Match(
                $parsed.Groups['Body'].Value,
                "^Executing command '(?<Command>.*)'$")
            if ((-not $commandMatch.Success) -or $hasTerminalToken) {
                Throw-BundleBlocker 'raw command occurrence is malformed.'
            }
            $commands.Add([pscustomobject]@{
                    index = $index
                    pid = [int]$parsed.Groups['Pid'].Value
                    tid = [int]$parsed.Groups['Tid'].Value
                    command = $commandMatch.Groups['Command'].Value
                    raw = $line
                })
            continue
        }
        $terminalMatch = [regex]::Match(
            $parsed.Groups['Body'].Value,
            '^Last command (?<Result>succeeded|failed)\.(?: \([0-9]+(?:\.[0-9]+)?ms\))?$')
        if (-not $terminalMatch.Success) {
            Throw-BundleBlocker 'raw command terminal occurrence is malformed.'
        }
        $terminals.Add([pscustomobject]@{
                index = $index
                pid = [int]$parsed.Groups['Pid'].Value
                tid = [int]$parsed.Groups['Tid'].Value
                succeeded = $terminalMatch.Groups['Result'].Value -ceq 'succeeded'
                raw = $line
            })
    }
    if ($commands.Count -ne $terminals.Count) {
        Throw-BundleBlocker 'raw command/terminal count is not exact 1:1.'
    }
    $consumed = [Collections.Generic.HashSet[int]]::new()
    $reports = New-Object Collections.Generic.List[object]
    foreach ($command in @($commands | Sort-Object -Property index)) {
        $nextSameThread = @($commands | Where-Object {
                $_.index -gt $command.index -and
                $_.pid -eq $command.pid -and $_.tid -eq $command.tid
            } | Sort-Object -Property index | Select-Object -First 1)
        $endExclusive = $Lines.Count
        if ($nextSameThread.Count -eq 1) {
            $endExclusive = [int]$nextSameThread[0].index
        }
        $matches = @($terminals | Where-Object {
                $_.index -gt $command.index -and $_.index -lt $endExclusive -and
                $_.pid -eq $command.pid -and $_.tid -eq $command.tid
            })
        if ($matches.Count -ne 1) {
            Throw-BundleBlocker (
                'raw command does not have exactly one next same-thread terminal.')
        }
        $terminal = $matches[0]
        if ((-not [bool]$terminal.succeeded) -or
            (-not $consumed.Add([int]$terminal.index))) {
            Throw-BundleBlocker 'raw command terminal failed or was shared.'
        }
        $reports.Add([ordered]@{
                command = [string]$command.command
                pid = [int]$command.pid
                tid = [int]$command.tid
                commandLineIndex = [int]$command.index
                terminalLineIndex = [int]$terminal.index
                commandRaw = [string]$command.raw
                terminalRaw = [string]$terminal.raw
                uniqueNextSameThreadSuccess = $true
            })
    }
    foreach ($terminal in $terminals) {
        if (-not $consumed.Contains([int]$terminal.index)) {
            Throw-BundleBlocker 'raw command terminal is orphaned.'
        }
    }
    return $reports.ToArray()
}

function Assert-ReplayedRawSessionMarkers {
    param(
        [Parameter(Mandatory = $true)][object[]]$Lines,
        [Parameter(Mandatory = $true)]$Session
    )
    $markers = @{
        start = New-Object Collections.Generic.List[object]
        doExit = New-Object Collections.Generic.List[object]
        exitDone = New-Object Collections.Generic.List[object]
    }
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        $line = [string]$Lines[$index]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $kinds = New-Object Collections.Generic.List[string]
        if ($line -match '(?i)Start\s*Application') { $kinds.Add('start') }
        if ($line -match '(?i)Do\s*exit\s*Lasal2') { $kinds.Add('doExit') }
        if ($line -match '(?i)LC2\s*exit\s*done') { $kinds.Add('exitDone') }
        if ($kinds.Count -eq 0) { continue }
        if ($kinds.Count -ne 1) {
            Throw-BundleBlocker 'raw session marker occurrence is ambiguous.'
        }
        $parsed = [regex]::Match(
            $line,
            ('^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*?' +
                '\((?<Level>INFO|WARN|ERROR|FATAL|DEBUG|NOTICE)\)\s+' +
                '(?<Source>[^\]]+)\]\s*(?<Body>.*)$'))
        if ((-not $parsed.Success) -or
            ($parsed.Groups['Level'].Value -cne 'INFO') -or
            ($parsed.Groups['Source'].Value -cne 'GUI')) {
            Throw-BundleBlocker 'raw session marker occurrence is malformed.'
        }
        $kind = $kinds[0]
        $body = $parsed.Groups['Body'].Value
        $bodyAccepted = switch ($kind) {
            'start' {
                $body -cmatch
                    '^Start Application at \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$'
            }
            'doExit' { $body -ceq 'Do exit Lasal2...' }
            'exitDone' { $body -ceq '...LC2 exit done.' }
        }
        if (-not $bodyAccepted) {
            Throw-BundleBlocker 'raw session marker occurrence is malformed.'
        }
        $markers[$kind].Add([pscustomobject]@{
                index = $index
                pid = [int]$parsed.Groups['Pid'].Value
                raw = $line
            })
    }
    if (($markers.start.Count -ne 1) -or
        ($markers.doExit.Count -ne 1) -or
        ($markers.exitDone.Count -ne 1)) {
        Throw-BundleBlocker 'raw session marker global count differs from exact one each.'
    }
    $bindings = @(
        [pscustomobject]@{
            marker = $markers.start[0]
            expectedIndex = [int]$Session.startLineIndex
        },
        [pscustomobject]@{
            marker = $markers.doExit[0]
            expectedIndex = [int]$Session.doExitLineIndex
        },
        [pscustomobject]@{
            marker = $markers.exitDone[0]
            expectedIndex = [int]$Session.exitDoneLineIndex
        })
    foreach ($binding in $bindings) {
        if (($binding.marker.index -ne $binding.expectedIndex) -or
            ($binding.marker.pid -ne [int]$Session.sessionPid)) {
            Throw-BundleBlocker 'raw session marker index/PID differs from its report.'
        }
    }
    if ([int]$Session.exactSessionCount -ne 1) {
        Throw-BundleBlocker 'raw-derived exactSessionCount is not one.'
    }
}

function Assert-RawSessionReplayContract {
    param(
        [Parameter(Mandatory = $true)]$RawArtifact,
        [Parameter(Mandatory = $true)]$Session
    )
    [byte[]]$bytes = $RawArtifact.content
    if (($bytes.Length -ge 3) -and
        ($bytes[0] -eq 0xEF) -and ($bytes[1] -eq 0xBB) -and ($bytes[2] -eq 0xBF)) {
        Throw-BundleBlocker 'raw bounded delta contains a UTF-8 BOM.'
    }
    try {
        $text = $Utf8Strict.GetString($bytes)
    }
    catch {
        Throw-BundleBlocker (
            "raw bounded delta is not strict UTF-8: $($_.Exception.Message)")
    }
    if ($text -match
        '(?i)CInvalidArgException|Last\s+command\s+failed\.|ios_base::failure|Find\s+in\s+Implementation|Edit\s+Method') {
        Throw-BundleBlocker 'raw bounded delta contains a forbidden failure/UI token.'
    }
    $lines = [regex]::Split($text, "\r?\n")
    while (($lines.Count -gt 0) -and
        [string]::IsNullOrEmpty([string]$lines[$lines.Count - 1])) {
        if ($lines.Count -eq 1) {
            $lines = @()
        }
        else {
            $lines = @($lines[0..($lines.Count - 2)])
        }
    }
    if ($lines.Count -le 1) {
        Throw-BundleBlocker 'raw bounded delta has no replayable line sequence.'
    }

    [object[]]$parsedLines = New-Object object[] $lines.Count
    $parsedPids = [Collections.Generic.HashSet[int]]::new()
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex++) {
        $lineText = [string]$lines[$lineIndex]
        if ([string]::IsNullOrWhiteSpace($lineText)) { continue }
        if ($lineIndex -eq 0) {
            if ($lineText -cnotmatch
                '^\[\d{2}:\d{2}:\d{2} \(INFO\) Application\] Log File is ok$') {
                Throw-BundleBlocker 'raw startup line 0 is not the exact Application record.'
            }
            $parsedLines[$lineIndex] = [pscustomobject]@{
                pid = $null
                tid = $null
                level = 'INFO'
                source = 'Application'
                body = 'Log File is ok'
            }
            continue
        }
        $match = [regex]::Match(
            $lineText,
            ('^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*?' +
                '\((?<Level>INFO|WARN|ERROR|FATAL|DEBUG|NOTICE)\)\s+' +
                '(?<Source>[^\]]+)\]\s*(?<Body>.*)$'))
        if (-not $match.Success) {
            Throw-BundleBlocker "raw nonempty line $lineIndex is not exactly parseable."
        }
        [void]$parsedPids.Add([int]$match.Groups['Pid'].Value)
        $parsedLines[$lineIndex] = [pscustomobject]@{
            pid = $match.Groups['Pid'].Value
            tid = $match.Groups['Tid'].Value
            level = $match.Groups['Level'].Value
            source = $match.Groups['Source'].Value
            body = $match.Groups['Body'].Value
        }
    }
    if (($parsedPids.Count -ne 1) -or
        (-not $parsedPids.Contains([int]$Session.sessionPid))) {
        Throw-BundleBlocker 'raw bounded delta contains a different session PID.'
    }
    Assert-ReplayedRawSessionMarkers -Lines $lines -Session $Session

    if (@($Session.preStartPrologue).Count -ne 6) {
        Throw-BundleBlocker 'isolated session preStartPrologue count is not 6.'
    }
    $startupDefinitions = @(
        [pscustomobject]@{
            source = 'Application'
            bodyPattern = '^Log File is ok$'
        },
        [pscustomobject]@{
            source = 'OutputSkripting'
            bodyPattern =
                "^Run Scriptfile 'C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2\.py'\.$"
        },
        [pscustomobject]@{
            source = 'OutputSkripting'
            bodyPattern = '^Total Script need [0-9]+(?:\.[0-9]+)? ms$'
        },
        [pscustomobject]@{
            source = 'OutputDataAnalyzer'
            bodyPattern =
                '^Loading DataAnalyzer configuration file "C:\\ProgramData\\Sigmatek\\Drive\(C\)\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Config\\DataAnalyser\.lcc"\.$'
        },
        [pscustomobject]@{
            source = 'OutputDataAnalyzer'
            bodyPattern =
                '^Loading DataAnalyzer configuration file "C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\DataAnalyserSDD\.lcc"\.$'
        },
        [pscustomobject]@{
            source = 'OutputDataAnalyzer'
            bodyPattern =
                '^Cannot find configuration file: "C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\DataAnalyserSDD\.lcc"$'
        })
    for ($index = 0; $index -lt 6; $index++) {
        $report = $Session.preStartPrologue[$index]
        Assert-ExactObjectKeys `
            -Object $report `
            -Expected @(
                'source', 'body', 'raw', 'lineIndex',
                'acceptedAsStartupOnlyPrologue') `
            -ObjectOwner "isolatedSession.preStartPrologue[$index]"
        Assert-JsonString -Value $report.source -ValueOwner 'prologue source'
        Assert-JsonString -Value $report.body -ValueOwner 'prologue body'
        Assert-JsonString -Value $report.raw -ValueOwner 'prologue raw'
        Assert-JsonInteger -Value $report.lineIndex -ValueOwner 'prologue lineIndex'
        Assert-JsonBoolean `
            -Value $report.acceptedAsStartupOnlyPrologue `
            -ValueOwner 'prologue acceptedAsStartupOnlyPrologue'
        $rawLine = Get-RawEvidenceLine `
            -Lines $lines -Index ([int]$report.lineIndex) -LineOwner 'prologue line'
        $parsedLine = $parsedLines[$index]
        if (([int]$report.lineIndex -ne $index) -or
            (-not [bool]$report.acceptedAsStartupOnlyPrologue) -or
            ([string]$report.raw -cne $rawLine) -or
            ([string]$report.source -cne $startupDefinitions[$index].source) -or
            ([string]$report.source -cne [string]$parsedLine.source) -or
            ([string]$report.body -cne [string]$parsedLine.body) -or
            ([string]$report.body -cnotmatch
                [string]$startupDefinitions[$index].bodyPattern) -or
            ([string]$parsedLine.level -cne 'INFO') -or
            (($index -gt 0) -and
                ([int]$parsedLine.pid -ne [int]$Session.sessionPid))) {
            Throw-BundleBlocker "preStartPrologue[$index] does not bind its raw line."
        }
    }
    if ([int]$Session.startLineIndex -ne 6) {
        Throw-BundleBlocker 'isolated session Start Application line index is not 6.'
    }
    $startLine = Get-RawEvidenceLine `
        -Lines $lines -Index ([int]$Session.startLineIndex) -LineOwner 'Start Application'
    if ($startLine -cnotmatch
        '\(INFO\) GUI\] Start Application at \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$') {
        Throw-BundleBlocker 'raw Start Application line differs.'
    }
    Assert-RawLineProcessIdentity `
        -Line $startLine -ExpectedPid ([int]$Session.sessionPid) -ExpectedTid 1 `
        -IgnoreTid -LineOwner 'Start Application'

    $restorationCommandLineIndexes =
        [Collections.Generic.HashSet[int]]::new()
    $restorationReports = @($Session.loadRestorationCommands)
    foreach ($report in $restorationReports) {
        Assert-ExactObjectKeys `
            -Object $report `
            -Expected @(
                'command', 'raw', 'commandLineIndex', 'successLineIndex',
                'acceptedAsLoadRestorationByBoundedOrdering', 'operatorOriginProven') `
            -ObjectOwner 'isolatedSession.loadRestorationCommands item'
        Assert-JsonString -Value $report.command -ValueOwner 'restoration command'
        Assert-JsonString -Value $report.raw -ValueOwner 'restoration raw'
        Assert-JsonInteger `
            -Value $report.commandLineIndex -ValueOwner 'restoration commandLineIndex'
        Assert-JsonInteger `
            -Value $report.successLineIndex -ValueOwner 'restoration successLineIndex'
        Assert-JsonBoolean `
            -Value $report.acceptedAsLoadRestorationByBoundedOrdering `
            -ValueOwner 'restoration accepted flag'
        Assert-JsonBoolean `
            -Value $report.operatorOriginProven `
            -ValueOwner 'restoration operatorOriginProven'
        $rawLine = Get-RawEvidenceLine `
            -Lines $lines -Index ([int]$report.commandLineIndex) `
            -LineOwner 'restoration command'
        $isAllowedRestoration =
            ([string]$report.command -match
                "^Open Network Editor for '[A-Za-z_][A-Za-z0-9_]*'$") -or
            ([string]$report.command -match
                '^Open Implementation Editor for "[A-Za-z_][A-Za-z0-9_]*"$') -or
            ([string]$report.command -match '^Open File Editor for "[^"]+"$')
        if (([string]$report.raw -cne $rawLine) -or
            (-not [bool]$report.acceptedAsLoadRestorationByBoundedOrdering) -or
            [bool]$report.operatorOriginProven -or
            (-not $isAllowedRestoration) -or
            ([int]$report.commandLineIndex -le [int]$Session.loadLineIndex) -or
            ([int]$report.commandLineIndex -ge [int]$Session.loadResultLineIndex) -or
            ([int]$report.successLineIndex -ge [int]$Session.loadResultLineIndex)) {
            Throw-BundleBlocker 'load restoration report differs from its raw line.'
        }
        Add-ExactRestorationCommandLineIndex `
            -CommandLineIndexes $restorationCommandLineIndexes `
            -CommandLineIndex ([int]$report.commandLineIndex)
    }

    $ledger = @($Session.commandTerminalLedger)
    $replayedLedger = @(Get-ReplayedCommandTerminalLedger -Lines $lines)
    if ($replayedLedger.Count -ne $ledger.Count) {
        Throw-BundleBlocker 'replayed raw command ledger count differs from the report.'
    }
    $loadEntries = New-Object Collections.Generic.List[object]
    $rebuildEntries = New-Object Collections.Generic.List[object]
    $closeEntries = New-Object Collections.Generic.List[object]
    $ledgerIndex = 0
    foreach ($report in $ledger) {
        Assert-ExactObjectKeys `
            -Object $report `
            -Expected @(
                'command', 'pid', 'tid', 'commandLineIndex', 'terminalLineIndex',
                'commandRaw', 'terminalRaw', 'uniqueNextSameThreadSuccess') `
            -ObjectOwner 'isolatedSession.commandTerminalLedger item'
        Assert-JsonString -Value $report.command -ValueOwner 'ledger command'
        foreach ($name in @('pid', 'tid', 'commandLineIndex', 'terminalLineIndex')) {
            Assert-JsonInteger -Value $report.$name -ValueOwner "ledger $name"
        }
        Assert-JsonString -Value $report.commandRaw -ValueOwner 'ledger commandRaw'
        Assert-JsonString -Value $report.terminalRaw -ValueOwner 'ledger terminalRaw'
        Assert-JsonBoolean `
            -Value $report.uniqueNextSameThreadSuccess `
            -ValueOwner 'ledger uniqueNextSameThreadSuccess'
        $commandLine = Get-RawEvidenceLine `
            -Lines $lines -Index ([int]$report.commandLineIndex) `
            -LineOwner 'ledger command'
        $terminalLine = Get-RawEvidenceLine `
            -Lines $lines -Index ([int]$report.terminalLineIndex) `
            -LineOwner 'ledger terminal'
        $commandSuffix = "Executing command '$($report.command)'"
        if (($report.commandRaw -cne $commandLine) -or
            ($report.terminalRaw -cne $terminalLine) -or
            (-not $commandLine.EndsWith($commandSuffix, [StringComparison]::Ordinal)) -or
            ($terminalLine -cnotmatch
                '\(INFO\) CmdProc\] Last command succeeded\.(?: \([0-9]+(?:\.[0-9]+)?ms\))?$') -or
            (-not [bool]$report.uniqueNextSameThreadSuccess) -or
            ([int]$report.commandLineIndex -ge [int]$report.terminalLineIndex)) {
            Throw-BundleBlocker 'command ledger item differs from its raw command/terminal.'
        }
        Assert-RawLineProcessIdentity `
            -Line $commandLine -ExpectedPid ([int]$report.pid) `
            -ExpectedTid ([int]$report.tid) -LineOwner 'ledger command'
        Assert-RawLineProcessIdentity `
            -Line $terminalLine -ExpectedPid ([int]$report.pid) `
            -ExpectedTid ([int]$report.tid) -LineOwner 'ledger terminal'
        $replayed = $replayedLedger[$ledgerIndex]
        foreach ($name in @(
                'command', 'pid', 'tid', 'commandLineIndex', 'terminalLineIndex',
                'commandRaw', 'terminalRaw', 'uniqueNextSameThreadSuccess')) {
            if ($report.$name -cne $replayed.$name) {
                Throw-BundleBlocker (
                    "reported command ledger item $ledgerIndex differs from raw replay.")
            }
        }
        $ledgerIndex++
        if ([string]$report.command -match '^Load Project "[^"]+"$') {
            $loadEntries.Add($report)
        }
        elseif ([string]$report.command -ceq 'Rebuild project') {
            $rebuildEntries.Add($report)
        }
        elseif ([string]$report.command -ceq 'Close Project') {
            $closeEntries.Add($report)
        }
        elseif (-not $restorationCommandLineIndexes.Contains(
                [int]$report.commandLineIndex)) {
            Throw-BundleBlocker 'raw ledger contains an unapproved command.'
        }
    }
    foreach ($restoration in $restorationReports) {
        $matches = @($ledger | Where-Object {
                [int]$_.commandLineIndex -eq [int]$restoration.commandLineIndex
            })
        if ($matches.Count -ne 1) {
            Throw-BundleBlocker 'load restoration does not select one ledger entry.'
        }
        $entry = $matches[0]
        $successLine = Get-RawEvidenceLine `
            -Lines $lines -Index ([int]$restoration.successLineIndex) `
            -LineOwner 'restoration success terminal'
        if (($entry.command -cne $restoration.command) -or
            ($entry.commandRaw -cne $restoration.raw) -or
            ([int]$entry.terminalLineIndex -ne
                [int]$restoration.successLineIndex) -or
            ($entry.terminalRaw -cne $successLine)) {
            Throw-BundleBlocker (
                'load restoration successLineIndex differs from its exact ledger terminal.')
        }
    }
    if (($loadEntries.Count -ne 1) -or
        ($rebuildEntries.Count -ne 1) -or
        ($closeEntries.Count -ne 1)) {
        Throw-BundleBlocker 'raw ledger does not contain exact Load/Rebuild/Close commands.'
    }
    $load = $loadEntries[0]
    $rebuild = $rebuildEntries[0]
    $close = $closeEntries[0]
    if (([int]$Session.loadLineIndex -ne [int]$load.commandLineIndex) -or
        ([int]$Session.loadTerminalLineIndex -ne [int]$load.terminalLineIndex) -or
        ([int]$Session.rebuildLineIndex -ne [int]$rebuild.commandLineIndex) -or
        ([int]$Session.rebuildTerminalLineIndex -ne [int]$rebuild.terminalLineIndex) -or
        ([int]$Session.closeLineIndex -ne [int]$close.commandLineIndex) -or
        ([int]$Session.closeTerminalLineIndex -ne [int]$close.terminalLineIndex) -or
        ([int]$Session.sessionPid -ne [int]$rebuild.pid) -or
        ([int]$Session.loadTid -ne [int]$load.tid) -or
        ([int]$Session.rebuildTid -ne [int]$rebuild.tid) -or
        ([int]$Session.closeTid -ne [int]$close.tid)) {
        Throw-BundleBlocker 'isolated session indices/PID/TIDs differ from the raw ledger.'
    }

    $loadResult = Get-RawEvidenceLine `
        -Lines $lines -Index ([int]$Session.loadResultLineIndex) -LineOwner 'Load ResultCount'
    $rebuildResult = Get-RawEvidenceLine `
        -Lines $lines -Index ([int]$Session.rebuildResultLineIndex) `
        -LineOwner 'Rebuild ResultCount'
    $doExit = Get-RawEvidenceLine `
        -Lines $lines -Index ([int]$Session.doExitLineIndex) -LineOwner 'Do exit'
    $exitDone = Get-RawEvidenceLine `
        -Lines $lines -Index ([int]$Session.exitDoneLineIndex) -LineOwner 'exit done'
    if (($loadResult -cnotmatch '\(INFO\) Compiler\] \{ResultCount\}$') -or
        ($rebuildResult -cnotmatch '\(INFO\) Compiler\] \{ResultCount\}$') -or
        ($doExit -cnotmatch '\(INFO\) GUI\] Do exit Lasal2\.\.\.$') -or
        ($exitDone -cnotmatch '\(INFO\) GUI\] \.\.\.LC2 exit done\.$') -or
        (-not ([int]$Session.startLineIndex -lt [int]$Session.loadLineIndex -and
            [int]$Session.loadLineIndex -lt [int]$Session.loadResultLineIndex -and
            [int]$Session.loadResultLineIndex -lt [int]$Session.loadTerminalLineIndex -and
            [int]$Session.loadTerminalLineIndex -lt [int]$Session.rebuildLineIndex -and
            [int]$Session.rebuildLineIndex -lt [int]$Session.rebuildResultLineIndex -and
            [int]$Session.rebuildResultLineIndex -lt [int]$Session.rebuildTerminalLineIndex -and
            [int]$Session.rebuildTerminalLineIndex -lt [int]$Session.doExitLineIndex -and
            [int]$Session.doExitLineIndex -lt [int]$Session.closeLineIndex -and
            [int]$Session.closeLineIndex -lt [int]$Session.closeTerminalLineIndex -and
            [int]$Session.closeTerminalLineIndex -lt [int]$Session.exitDoneLineIndex))) {
        Throw-BundleBlocker 'raw isolated session ordering/result/exit contract differs.'
    }
    $saveLines = @($lines | Where-Object {
            $_ -match '\(INFO\) OutputCommand\] Save project ''[^'']+''\.$'
        })
    if (($saveLines.Count -ne 1) -or
        ([Array]::IndexOf($lines, $saveLines[0]) -le [int]$Session.rebuildLineIndex) -or
        ([Array]::IndexOf($lines, $saveLines[0]) -ge
            [int]$Session.rebuildResultLineIndex)) {
        Throw-BundleBlocker 'raw isolated session auto-save contract differs.'
    }
    for ($index = [int]$Session.exitDoneLineIndex + 1;
        $index -lt $lines.Count;
        $index++) {
        if (-not [string]::IsNullOrWhiteSpace([string]$lines[$index])) {
            Throw-BundleBlocker 'raw bounded delta has records after exit done.'
        }
    }

    $knownErrors = @($Session.knownLoadErrors)
    $rawErrors = @($lines | Where-Object { $_ -match '\((?:ERROR|FATAL)\)' })
    if (($knownErrors.Count -gt 1) -or ($rawErrors.Count -ne $knownErrors.Count)) {
        Throw-BundleBlocker 'raw ERROR/FATAL inventory differs from knownLoadErrors.'
    }
    foreach ($report in $knownErrors) {
        Assert-ExactObjectKeys `
            -Object $report `
            -Expected @('raw', 'acceptedAsKnownVendorLoadError', 'rebuildError') `
            -ObjectOwner 'isolatedSession.knownLoadErrors item'
        Assert-JsonString -Value $report.raw -ValueOwner 'known load error raw'
        Assert-JsonBoolean `
            -Value $report.acceptedAsKnownVendorLoadError `
            -ValueOwner 'known load error accepted flag'
        Assert-JsonBoolean -Value $report.rebuildError -ValueOwner 'known rebuildError'
        if ((-not [bool]$report.acceptedAsKnownVendorLoadError) -or
            [bool]$report.rebuildError -or
            ([string]$report.raw -cnotmatch
                '\(ERROR\) Compiler\] E 0015 ".*"\(\d+\) Error reading file ''.*\\Class\\_DriveMngBase\\DriveComL2\.h''\|\*000000\*\|15\|11015\|\|$') -or
            (@($rawErrors | Where-Object { $_ -ceq $report.raw }).Count -ne 1)) {
            Throw-BundleBlocker 'known load error does not bind one raw ERROR line.'
        }
    }
}

function Assert-HistoricalGitObjects {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)]$Comparison
    )
    $head = [string]$Manifest.repository.headObserved
    $resolvedHead = (Invoke-GitText `
            -Root $RepositoryRoot `
            -Arguments @('rev-parse', '--verify', "$head`^{commit}") `
            -Operation 'historical head resolution').stdout
    if ($resolvedHead -cne $head) {
        Throw-BundleBlocker 'historical headObserved does not resolve exactly.'
    }
    $reports = @([pscustomobject]@{
            relativePath = [string]$Manifest.finalizer.relativePath
            bytes = [long]$Manifest.finalizer.bytes
            sha256 = [string]$Manifest.finalizer.sha256
            expectedBlobOid = [string]$Manifest.finalizer.headBlobOid
            owner = 'historical finalizer'
        })
    foreach ($trusted in @($Manifest.trustedArtifacts)) {
        $reports += [pscustomobject]@{
            relativePath = [string]$trusted.relativePath
            bytes = [long]$trusted.bytes
            sha256 = [string]$trusted.sha256
            expectedBlobOid = $null
            owner = "historical trusted artifact $($trusted.relativePath)"
        }
    }
    $historicalBaseline = $null
    $knownComparisonOracle = $null
    foreach ($report in $reports) {
        Assert-SafeRelativePath `
            -RelativePath $report.relativePath -PathOwner "$($report.owner) path"
        $blobOid = (Invoke-GitText `
                -Root $RepositoryRoot `
                -Arguments @(
                    'rev-parse', '--verify', "$head`:$($report.relativePath)") `
                -Operation "$($report.owner) path/blob resolution").stdout
        if ($blobOid -cnotmatch '^[0-9a-f]{40}$') {
            Throw-BundleBlocker "$($report.owner) blob oid is invalid."
        }
        if (($null -ne $report.expectedBlobOid) -and
            ([string]$blobOid -cne [string]$report.expectedBlobOid)) {
            Throw-BundleBlocker 'historical finalizer blob oid differs from the manifest.'
        }
        $blob = Read-GitBlobArtifact `
            -Root $RepositoryRoot -BlobOid $blobOid -BlobOwner $report.owner
        $textIdentityBridges =
            @($HistoricalTextIdentityBridges | Where-Object {
                    [string]$_.relativePath -ceq [string]$report.relativePath
                })
        if ($textIdentityBridges.Count -gt 1) {
            Throw-BundleBlocker (
                "$($report.owner) selects multiple historical text identity bridges.")
        }
        $textIdentityBridge = if ($textIdentityBridges.Count -eq 1) {
            $textIdentityBridges[0]
        } else {
            $null
        }
        Assert-HistoricalManifestIdentityMatchesGitBlob `
            -Report $report `
            -Blob $blob `
            -BlobOid $blobOid `
            -TextIdentityBridge $textIdentityBridge
        if ($report.relativePath -ceq $BaselineRelativePath) {
            $historicalBaseline = Read-StrictJsonArtifact `
                -Artifact $blob -JsonOwner 'historical pinned rebuild baseline'
        }
        elseif ($report.relativePath -ceq
            ($EvidenceRelativeDirectory +
                '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json')) {
            $knownComparisonOracle = Read-StrictJsonArtifact `
                -Artifact $blob -JsonOwner 'historical pinned 6E comparison oracle'
        }
    }
    if ($null -eq $historicalBaseline) {
        Throw-BundleBlocker 'historical pinned rebuild baseline was not resolved.'
    }
    Assert-BaselineInputIdentityBinding `
        -Baseline $historicalBaseline `
        -CurrentInputs $Manifest.baseline.currentInputs
    if ([int]$Manifest.decision.exitCode -in @(0, 2)) {
        if ($null -eq $knownComparisonOracle) {
            Throw-BundleBlocker 'historical pinned comparison oracle was not resolved.'
        }
        foreach ($name in @(
                'recordParser', 'gateDTargetRecords',
                'protectedDependencyRecords')) {
            [byte[]]$actualBytes = ConvertTo-ComparatorCanonicalJsonBytes `
                -Value $Comparison.$name
            [byte[]]$oracleBytes = ConvertTo-ComparatorCanonicalJsonBytes `
                -Value $knownComparisonOracle.$name
            if (-not (Test-ByteSequencesExact `
                    -Actual $actualBytes -Expected $oracleBytes)) {
                Throw-BundleBlocker "comparison.$name differs from the pinned oracle."
            }
        }
        [byte[]]$actualFrozen = ConvertTo-ComparatorCanonicalJsonBytes `
            -Value $Comparison.comparison.frozenOpaqueOwners
        [byte[]]$oracleFrozen = ConvertTo-ComparatorCanonicalJsonBytes `
            -Value $knownComparisonOracle.comparison.frozenOpaqueOwners
        if (-not (Test-ByteSequencesExact `
                -Actual $actualFrozen -Expected $oracleFrozen)) {
            Throw-BundleBlocker 'comparison frozenOpaqueOwners differ from the pinned oracle.'
        }
        if ([int]$Manifest.decision.exitCode -eq 2) {
            foreach ($name in @('changedCheckpointOwners', 'diffRuns')) {
                [byte[]]$actualBytes = ConvertTo-ComparatorCanonicalJsonBytes `
                    -Value $Comparison.$name
                [byte[]]$oracleBytes = ConvertTo-ComparatorCanonicalJsonBytes `
                    -Value $knownComparisonOracle.$name
                if (-not (Test-ByteSequencesExact `
                        -Actual $actualBytes -Expected $oracleBytes)) {
                    Throw-BundleBlocker (
                        "comparison.$name differs from the pinned 6E oracle.")
                }
            }
        }
    }
}

function Assert-EndInventoryAndIdentities {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)]$InitialArtifacts
    )
    $finalArtifacts = Get-ExactBundleInventory `
        -RepositoryRoot $RepositoryRoot `
        -BundlePath $BundlePath `
        -ExpectedNames $ArtifactNames
    foreach ($name in $ArtifactNames) {
        $initial = $InitialArtifacts[$name]
        $final = $finalArtifacts[$name]
        if (($initial.bytes -ne $final.bytes) -or
            ($initial.sha256 -cne $final.sha256) -or
            (-not (Test-ByteSequencesExact `
                    -Actual $initial.content -Expected $final.content))) {
            Throw-BundleBlocker "bundle file changed during verification: $name"
        }
    }
}

function Invoke-BundleVerificationCore {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$EvidenceRelativeRoot,
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [switch]$SkipHistoricalGitObjects,
        [switch]$SkipPinnedProductionIdentities
    )
    $artifacts = Get-ExactBundleInventory `
        -RepositoryRoot $RepositoryRoot `
        -BundlePath $BundlePath `
        -ExpectedNames $ArtifactNames
    $complete = Read-StrictJsonArtifact `
        -Artifact $artifacts[$CompleteManifestName] `
        -JsonOwner 'complete finalization manifest'
    Assert-CompleteManifestShape `
        -Manifest $complete `
        -RepositoryRoot $RepositoryRoot `
        -SkipPinnedProductionIdentities:$SkipPinnedProductionIdentities
    Assert-IsolatedSessionContract -Session $complete.isolatedSession
    Assert-ToolContract -Tools $complete.tools
    Assert-PublicationContract -Publication $complete.publication
    Assert-RegeneratedOutputContract `
        -RegeneratedOutputs $complete.regeneratedOutputs -Artifacts $artifacts
    if ((-not $SkipPinnedProductionIdentities) -and
        ([string]$artifacts[$NetworksSnapshotName].sha256 -cne
            $KnownNetworksSha256)) {
        Throw-BundleBlocker 'Networks snapshot differs from the pinned output identity.'
    }
    Assert-CompleteDecisionContract -Decision $complete.decision

    $reports = $complete.artifactsWrittenBeforeCompleteManifest
    if (-not (Test-IsJsonArray -Value $reports) -or @($reports).Count -ne 7) {
        Throw-BundleBlocker 'complete manifest preceding-artifact report count/type differs.'
    }
    for ($index = 0; $index -lt $PreManifestArtifactNames.Count; $index++) {
        $name = $PreManifestArtifactNames[$index]
        Assert-ArtifactIdentityReport `
            -Report $reports[$index] `
            -ExpectedName $name `
            -Artifact $artifacts[$name] `
            -ReportOwner "artifactsWrittenBeforeCompleteManifest[$index]"
    }

    Assert-OwnerMarkerContract `
        -MarkerArtifact $artifacts[$OwnerMarkerName] `
        -ExpectedStageName ([string]$complete.publication.stagingDirectoryName)
    $rawManifest = Read-StrictJsonArtifact `
        -Artifact $artifacts[$RawManifestName] `
        -JsonOwner 'raw bounded manifest'
    Assert-RawManifestContract `
        -RawManifest $rawManifest -CompleteManifest $complete -Artifacts $artifacts
    Assert-RawSessionReplayContract `
        -RawArtifact $artifacts[$RawDeltaName] `
        -Session $complete.isolatedSession
    $comparison = Read-StrictJsonArtifact `
        -Artifact $artifacts[$ComparisonName] `
        -JsonOwner 'Classes comparison report'
    Assert-ComparatorContract `
        -Comparison $comparison `
        -ComparisonArtifact $artifacts[$ComparisonName] `
        -CompleteManifest $complete `
        -EvidenceRelativeRoot $EvidenceRelativeRoot `
        -ClassesArtifact $artifacts[$ClassesSnapshotName]
    if (-not $SkipHistoricalGitObjects) {
        Assert-HistoricalGitObjects `
            -RepositoryRoot $RepositoryRoot -Manifest $complete -Comparison $comparison
    }
    Assert-EndInventoryAndIdentities `
        -RepositoryRoot $RepositoryRoot `
        -BundlePath $BundlePath `
        -InitialArtifacts $artifacts
    return [pscustomobject]@{
        disposition = [string]$complete.decision.disposition
        classificationExitCode = [int]$complete.decision.exitCode
        productionApproved = $false
        onlineRuntimeQualificationPermitted = $false
        bundlePath = Get-NormalizedFullPath -Path $BundlePath
    }
}

function Invoke-ProductionVerification {
    param([Parameter(Mandatory = $true)][string]$RequestedRepositoryRoot)
    $root = Resolve-ProductionRepository -RequestedRoot $RequestedRepositoryRoot
    $validatorIdentity = Assert-ValidatorTrackedAndHeadClean -Root $root
    $bundlePath = Get-NormalizedFullPath -Path (
        Join-Path (Join-Path $root $EvidenceRelativeDirectory) $FinalDirectoryName)
    $result = Invoke-BundleVerificationCore `
        -RepositoryRoot $root `
        -EvidenceRelativeRoot $EvidenceRelativeDirectory `
        -BundlePath $bundlePath
    Write-Output (
        "PASS $Owner.Verify disposition=$($result.disposition) " +
        "classificationExit=$($result.classificationExitCode) " +
        "ProductionApproved=false onlineRuntimeQualificationPermitted=false " +
        "validatorBlob=$($validatorIdentity.blobOid) " +
        "validatorBytes=$($validatorIdentity.bytes) " +
        "validatorSha256=$($validatorIdentity.sha256) " +
        "validatorHeadBlobBytes=$($validatorIdentity.headBlobBytes) " +
        "validatorHeadBlobSha256=$($validatorIdentity.headBlobSha256) " +
        "validatorCanonicalLfSha256=$($validatorIdentity.canonicalLfSha256) " +
        "bundle=$($result.bundlePath)")
}

function Write-SelfTestBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][byte[]]$Bytes
    )
    $parent = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        [void][IO.Directory]::CreateDirectory($parent)
    }
    [IO.File]::WriteAllBytes($Path, $Bytes)
}

function New-SelfTestComparison {
    param(
        [Parameter(Mandatory = $true)][string]$CandidatePath,
        [Parameter(Mandatory = $true)]$ClassesIdentity
    )
    $frozenOwners = @(for ($index = 0; $index -lt 36; $index++) {
            'SyntheticOpaqueOwner{0:D2}' -f $index
        })
    $newIdentity = {
        param([string]$Sha = ('A' * 64))
        [ordered]@{
            startOffset = 0L
            endOffsetExclusive = 1L
            sourceOffset = 0L
            bytes = 1L
            sha256 = $Sha
        }
    }
    $newRecord = {
        param([string]$Owner, [string]$Parser, [bool]$Protected)
        $record = [ordered]@{
            owner = $Owner
            sourcePath = ".\\Class\\$Owner\\$Owner.st"
            parser = $Parser
            exact = $true
            checkpoint = & $newIdentity
            candidate = & $newIdentity
        }
        if ($Protected) { $record.legacyWindowExact = $true }
        return $record
    }
    $targetRecords = @(for ($index = 0; $index -lt 4; $index++) {
            & $newRecord "SyntheticTarget$index" `
                'aa03-header-to-next-header-or-eof' $false
        })
    $protectedRecords = @(for ($index = 0; $index -lt 2; $index++) {
            & $newRecord "SyntheticProtected$index" `
                'aa03-header-to-next-header+legacy-window-cross-check' $true
        })
    $checkpointMapping = [ordered]@{
        owner = 'SyntheticChangedOwner'
        sourcePath = '.\Class\SyntheticChangedOwner\SyntheticChangedOwner.st'
        recordStart = 0L
        recordEndExclusive = 1L
        overlapStart = 0L
        overlapBytes = 1L
    }
    $candidateMapping = [ordered]@{
        owner = 'SyntheticChangedOwner'
        sourcePath = '.\Class\SyntheticChangedOwner\SyntheticChangedOwner.st'
        recordStart = 0L
        recordEndExclusive = 1L
        overlapStart = 0L
        overlapBytes = 1L
    }
    return [ordered]@{
        schema = $ComparisonSchema
        decision = [ordered]@{
            disposition = 'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
            checkpointIdentityAccepted = $false
            approvalScope = 'checkpoint-byte-identity-only'
            productionApproved = $false
            exactCheckpointMatch = $false
            semanticEquivalenceProven = $false
            recordEqualityCannotApproveArtifact = $true
            exitCode = 3
        }
        checkpoint = [ordered]@{
            requested = $CheckpointCommit
            kind = 'revision'
            resolvedRevision = $CheckpointCommit
            relativePath = $ClassesRelativePath
            blobOid = $CheckpointBlobOid
            rawBytes = $CheckpointClassesBytes
            sha256 = $CheckpointClassesSha256
        }
        candidate = [ordered]@{
            path = $CandidatePath
            rawBytes = [long]$ClassesIdentity.bytes
            sha256 = [string]$ClassesIdentity.sha256
        }
        comparison = [ordered]@{
            byteExact = $false
            equalLength = $false
            lengthDelta = [long]$ClassesIdentity.bytes - $CheckpointClassesBytes
            alignment = 'bounded-common-prefix-suffix'
            changedByteCountDefined = $false
            changedByteCount = $null
            contiguousRunCount = 1L
            checkpointChangedOwnerCount = 1L
            unmappedRunCount = 0L
            changedOwnersAreFrozenOpaqueSubset = $false
            frozenOpaqueOwnerCount = 36L
            frozenOpaqueOwners = $frozenOwners
            proprietaryFieldSemanticsDecoded = $false
        }
        recordParser = [ordered]@{
            convention =
                'first-special-record-then-aa03-header-to-next-header-or-eof'
            latin1ByteOffsetPreserving = $true
            sourceMarkerBoundary = 'path-length-le24-plus-aa'
            trueHeaderBoundary =
                'aa-03-plus-class-name-length-le24-plus-aa-plus-class-name'
            sourcePathSegmentsDiagnosticOnly = $true
            checkpointOwnerRecordCount = 7L
            candidateOwnerRecordCount = 7L
            headerSourceInventory = [ordered]@{
                exact = $true
                checkpointCount = 7L
                candidateCount = 7L
                firstMismatch = $null
                comparedFields = @(
                    'owner', 'sourcePath', 'headerOffset', 'recordEndOffset',
                    'sourcePathOffset', 'sourceMarkerOffset', 'parser')
            }
            firstSpecialRecord = & $newRecord 'SyntheticFirst' `
                'first-special-preamble-to-next-header' $false
        }
        gateDTargetRecords = [ordered]@{
            allEqual = $true
            records = $targetRecords
        }
        protectedDependencyRecords = [ordered]@{
            allEqual = $true
            records = $protectedRecords
        }
        changedCheckpointOwners = @([ordered]@{
                owner = 'SyntheticChangedOwner'
                sourcePath =
                    '.\Class\SyntheticChangedOwner\SyntheticChangedOwner.st'
                diffRunCount = 1L
                changedCheckpointBytes = 1L
                classification = 'contract-or-unclassified-owner-record'
            })
        diffRuns = @([ordered]@{
                ordinal = 1L
                checkpointStart = 0L
                checkpointBytes = 1L
                candidateStart = 0L
                candidateBytes = 1L
                checkpointPreview = [ordered]@{
                    hex = '41'
                    previewBytes = 1L
                    truncated = $false
                }
                candidatePreview = [ordered]@{
                    hex = '42'
                    previewBytes = 1L
                    truncated = $false
                }
                checkpointOwners = @($checkpointMapping)
                candidateOwners = @($candidateMapping)
                mappingComplete = $true
            })
    }
}

function Get-SelfTestFileArtifact {
    param([Parameter(Mandatory = $true)][string]$Path)
    [byte[]]$bytes = [IO.File]::ReadAllBytes($Path)
    return [pscustomobject]@{
        bytes = [long]$bytes.LongLength
        sha256 = Get-BytesSha256 -Bytes $bytes
    }
}

function New-SelfTestBundle {
    param(
        [Parameter(Mandatory = $true)][string]$CaseRoot,
        [string]$EvidenceRoot = 'evidence'
    )
    $bundle = Join-Path (Join-Path $CaseRoot $EvidenceRoot) $FinalDirectoryName
    [void][IO.Directory]::CreateDirectory($bundle)
    $stageName = '.finalize-stage-0123456789abcdef0123456789abcdef'
    $ownerMarker = [ordered]@{
        schema = $OwnerMarkerSchema
        ownerToken = 'abcdef0123456789abcdef0123456789'
        stageDirectoryName = $stageName
        overwriteAllowed = $false
        productionApproved = $false
    }
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $OwnerMarkerName) `
        -Bytes (ConvertTo-PrettyJsonBytes -Value $ownerMarker)
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $ClassesSnapshotName) `
        -Bytes $Utf8NoBom.GetBytes('third-hash-classes')
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $NetworksSnapshotName) `
        -Bytes $Utf8NoBom.GetBytes('networks')
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $TranscriptName) `
        -Bytes $Utf8NoBom.GetBytes("synthetic transcript`n")
    $rawLines = @(
        '[11:59:56 (INFO) Application] Log File is ok',
        '[11:59:56 P:00100 T:00001 (INFO) OutputSkripting] Run Scriptfile ''C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\Lasal2.py''.',
        '[11:59:57 P:00100 T:00001 (INFO) OutputSkripting] Total Script need 12.5 ms',
        '[11:59:58 P:00100 T:00001 (INFO) OutputDataAnalyzer] Loading DataAnalyzer configuration file "C:\ProgramData\Sigmatek\Drive(C)\Program Files (x86)\Sigmatek\Lasal\Class2\Config\DataAnalyser.lcc".',
        '[11:59:59 P:00100 T:00001 (INFO) OutputDataAnalyzer] Loading DataAnalyzer configuration file "C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\DataAnalyserSDD.lcc".',
        '[12:00:00 P:00100 T:00001 (INFO) OutputDataAnalyzer] Cannot find configuration file: "C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\DataAnalyserSDD.lcc"',
        '[12:00:00 P:00100 T:00001 (INFO) GUI] Start Application at 2026-08-11 12:00:00',
        '[12:00:01 P:00100 T:00001 (INFO) CmdProc] Executing command ''Load Project "C:\Synthetic\project.lcp"''',
        '[12:00:02 P:00100 T:00004 (INFO) CmdProc] Executing command ''Open Implementation Editor for "TCPMotionInterface"''',
        '[12:00:03 P:00100 T:00004 (INFO) CmdProc] Last command succeeded. (1ms)',
        '[12:00:04 P:00100 T:00001 (INFO) Compiler] {ResultCount}',
        '[12:00:05 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (2ms)',
        '[12:00:06 P:00100 T:00002 (INFO) CmdProc] Executing command ''Rebuild project''',
        '[12:00:07 P:00100 T:00002 (INFO) OutputCommand] Save project ''C:\Synthetic\project.lcp''.',
        '[12:00:08 P:00100 T:00002 (INFO) Compiler] {ResultCount}',
        '[12:00:09 P:00100 T:00002 (INFO) CmdProc] Last command succeeded. (3ms)',
        '[12:00:10 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
        '[12:00:11 P:00100 T:00003 (INFO) CmdProc] Executing command ''Close Project''',
        '[12:00:12 P:00100 T:00003 (INFO) CmdProc] Last command succeeded. (1ms)',
        '[12:00:13 P:00100 T:00001 (INFO) GUI] ...LC2 exit done.')
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $RawDeltaName) `
        -Bytes $Utf8NoBom.GetBytes(($rawLines -join "`n") + "`n")

    $classes = Get-SelfTestFileArtifact -Path (Join-Path $bundle $ClassesSnapshotName)
    $networks = Get-SelfTestFileArtifact -Path (Join-Path $bundle $NetworksSnapshotName)
    $transcript = Get-SelfTestFileArtifact -Path (Join-Path $bundle $TranscriptName)
    $raw = Get-SelfTestFileArtifact -Path (Join-Path $bundle $RawDeltaName)
    $baselineBytes = 6887L
    $baselineSha = 'B' * 64
    $prefixBytes = 100L
    $prefixSha = 'C' * 64
    $rawManifest = [ordered]@{
        Schema = $RawManifestSchema
        EvidenceProfile = 'GateDVisualLayout'
        Provenance = 'Exact byte slice from prefix-validated Lasal2.log'
        CapturedAtUtc = '2026-08-11T00:00:00.0000000Z'
        BaselineFileName = [IO.Path]::GetFileName($BaselineRelativePath)
        BaselineByteCount = $baselineBytes
        BaselineSha256 = $baselineSha.ToLowerInvariant()
        BaselinePrefixLength = $prefixBytes
        BaselinePrefixSha256 = $prefixSha.ToLowerInvariant()
        SourceLogPath = 'C:\Synthetic\Lasal2.log'
        SourceStartOffset = $prefixBytes
        SourceEndOffset = $prefixBytes + [long]$raw.bytes
        RawDeltaFileName = $RawDeltaName
        RawDeltaByteCount = [long]$raw.bytes
        RawDeltaSha256 = ([string]$raw.sha256).ToLowerInvariant()
        Encoding = 'UTF-8'
        SessionPid = 100
        RebuildTid = 2
        TranscriptFileName = $TranscriptName
        TranscriptByteCount = [long]$transcript.bytes
        TranscriptSha256 = ([string]$transcript.sha256).ToLowerInvariant()
        RegeneratedOutputs = @(
            [ordered]@{
                RelativePath = $ClassesRelativePath
                Bytes = [long]$classes.bytes
                Sha256 = ([string]$classes.sha256).ToLowerInvariant()
            },
            [ordered]@{
                RelativePath = $NetworksRelativePath
                Bytes = [long]$networks.bytes
                Sha256 = ([string]$networks.sha256).ToLowerInvariant()
            })
    }
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $RawManifestName) `
        -Bytes (ConvertTo-PrettyJsonBytes -Value $rawManifest)
    $candidatePath = $EvidenceRoot.TrimEnd('/') + '/' + $stageName + '/' +
        $ClassesSnapshotName
    $comparison = New-SelfTestComparison `
        -CandidatePath $candidatePath -ClassesIdentity $classes
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $ComparisonName) `
        -Bytes (ConvertTo-ComparatorCanonicalJsonBytes -Value $comparison)

    $artifactReports = @()
    foreach ($name in $PreManifestArtifactNames) {
        $identity = Get-SelfTestFileArtifact -Path (Join-Path $bundle $name)
        $artifactReports += [ordered]@{
            fileName = $name
            bytes = [long]$identity.bytes
            sha256 = [string]$identity.sha256
        }
    }
    $trustedPaths = @(
        $BaselineRelativePath,
        'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/Convert-Lasal2LogToBuildTranscript.ps1',
        'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/Verify-LasalC78RebuildEvidence.ps1',
        ($EvidenceRelativeDirectory + '/Compare-LasalClassesArtifact.ps1'),
        ($EvidenceRelativeDirectory +
            '/classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json'))
    $trustedArtifacts = @()
    for ($index = 0; $index -lt $trustedPaths.Count; $index++) {
        $trustedArtifacts += [ordered]@{
            owner = "synthetic trusted $index"
            relativePath = $trustedPaths[$index]
            bytes = $(if ($index -eq 0) { $baselineBytes } else { 1L })
            sha256 = $(if ($index -eq 0) { $baselineSha } else { 'D' * 64 })
            gitTrackedAndHeadClean = $true
        }
    }
    $inputs = @()
    $baselineInputFiles = @()
    for ($index = 0; $index -lt 10; $index++) {
        $relativeInputPath = "synthetic/input-$index.st"
        $inputs += [ordered]@{
            relativePath = $relativeInputPath
            bytes = 1L
            sha256 = 'E' * 64
            exactBaselineInputIdentity = $true
        }
        $baselineInputFiles += [ordered]@{
            RelativePath = $relativeInputPath
            Role = 'inputIdentity'
            Sha256 = 'e' * 64
            RawBytes = 1L
            RawSha256 = 'e' * 64
            CanonicalLfBytes = 1L
            CanonicalLfSha256 = 'e' * 64
            EolStyle = 'LF'
            CrLfCount = 0L
            LfOnlyCount = 0L
            CrOnlyCount = 0L
            LineBreakCount = 0L
        }
    }
    $syntheticBaselineValue = [ordered]@{
        Schema = 'LasalC78RebuildEvidence/v1'
        EvidenceProfile = 'GateDVisualLayout'
        CapturedAtUtc = '2026-08-11T00:00:00.0000000Z'
        RepositoryRoot = 'C:\Synthetic'
        CanonicalProjectPath = 'C:\Synthetic\project.lcp'
        LasalLogPath = 'C:\Synthetic\Lasal2.log'
        LogPrefixLength = $prefixBytes
        LogPrefixSha256 = $prefixSha.ToLowerInvariant()
        RequiredCompileRelativePaths = @()
        Files = $baselineInputFiles
    }
    [byte[]]$syntheticBaselineBytes =
        ConvertTo-PrettyJsonBytes -Value $syntheticBaselineValue
    $syntheticBaseline = Read-StrictJsonArtifact `
        -Artifact ([pscustomobject]@{ content = $syntheticBaselineBytes }) `
        -JsonOwner 'self-test historical baseline'
    $complete = [ordered]@{
        schema = $CompleteSchema
        complete = $true
        capturedAtUtc = '2026-08-11T00:00:01.0000000Z'
        repository = [ordered]@{
            root = Get-NormalizedFullPath -Path $CaseRoot
            headObserved = '1' * 40
            headPinnedForDecision = $false
            checkpointCommit = $CheckpointCommit
        }
        powershellEngine = [ordered]@{
            psEdition = 'Core'
            major = 7
            version = '7.5.0'
            minimumSupportedMajor = 7
            directoryNtfsStreamEnumerationRequired = $true
            productionFinalizationSupported = $true
        }
        trustedArtifacts = $trustedArtifacts
        finalizer = [ordered]@{
            relativePath = $FinalizerRelativePath
            bytes = 1L
            sha256 = 'F' * 64
            headBlobOid = '2' * 40
            gitTrackedAndHeadClean = $true
        }
        baseline = [ordered]@{
            relativePath = $BaselineRelativePath
            inputIdentityCount = 10
            currentInputs = $inputs
        }
        log = [ordered]@{
            path = 'C:\Synthetic\Lasal2.log'
            baselinePrefixBytes = $prefixBytes
            baselinePrefixSha256 = $prefixSha
            frozenEndOffset = $prefixBytes + [long]$raw.bytes
            frozenFullSha256 = 'A' * 64
            tailAppendPolicy =
                'forbidden-until-atomic-publish-full-length-and-sha-must-remain-exact'
        }
        isolatedSession = [ordered]@{
            sessionPid = 100
            loadTid = 1
            rebuildTid = 2
            closeTid = 3
            startLineIndex = 6
            preStartPrologue = @(
                for ($index = 0; $index -lt 6; $index++) {
                    [ordered]@{
                        source = $(if ($index -eq 0) {
                                'Application'
                            } elseif ($index -lt 3) {
                                'OutputSkripting'
                            } else {
                                'OutputDataAnalyzer'
                            })
                        body = $(if ($index -eq 0) {
                                'Log File is ok'
                            } else {
                                $bodyMatch = [regex]::Match(
                                    $rawLines[$index], '\]\s*(?<Body>.*)$')
                                $bodyMatch.Groups['Body'].Value
                            })
                        raw = $rawLines[$index]
                        lineIndex = $index
                        acceptedAsStartupOnlyPrologue = $true
                    }
                })
            loadLineIndex = 7
            loadResultLineIndex = 10
            loadTerminalLineIndex = 11
            rebuildLineIndex = 12
            rebuildResultLineIndex = 14
            rebuildTerminalLineIndex = 15
            doExitLineIndex = 16
            closeLineIndex = 17
            closeTerminalLineIndex = 18
            exitDoneLineIndex = 19
            exactSessionCount = 1
            exactRebuildCount = 1
            loadRestorationCommands = @([ordered]@{
                    command =
                        'Open Implementation Editor for "TCPMotionInterface"'
                    raw = $rawLines[8]
                    commandLineIndex = 8
                    successLineIndex = 9
                    acceptedAsLoadRestorationByBoundedOrdering = $true
                    operatorOriginProven = $false
                })
            commandTerminalLedger = @(
                [ordered]@{
                    command = 'Load Project "C:\Synthetic\project.lcp"'
                    pid = 100
                    tid = 1
                    commandLineIndex = 7
                    terminalLineIndex = 11
                    commandRaw = $rawLines[7]
                    terminalRaw = $rawLines[11]
                    uniqueNextSameThreadSuccess = $true
                },
                [ordered]@{
                    command =
                        'Open Implementation Editor for "TCPMotionInterface"'
                    pid = 100
                    tid = 4
                    commandLineIndex = 8
                    terminalLineIndex = 9
                    commandRaw = $rawLines[8]
                    terminalRaw = $rawLines[9]
                    uniqueNextSameThreadSuccess = $true
                },
                [ordered]@{
                    command = 'Rebuild project'
                    pid = 100
                    tid = 2
                    commandLineIndex = 12
                    terminalLineIndex = 15
                    commandRaw = $rawLines[12]
                    terminalRaw = $rawLines[15]
                    uniqueNextSameThreadSuccess = $true
                },
                [ordered]@{
                    command = 'Close Project'
                    pid = 100
                    tid = 3
                    commandLineIndex = 17
                    terminalLineIndex = 18
                    commandRaw = $rawLines[17]
                    terminalRaw = $rawLines[18]
                    uniqueNextSameThreadSuccess = $true
                })
            knownLoadErrors = @()
            prohibitedCommandCount = 0
            cInvalidArgExceptionCount = 0
            rebuildErrorCount = 0
        }
        tools = [ordered]@{
            converter = [ordered]@{ exitCode = 0; outputLines = @('PASS') }
            c78VerifyBuild = [ordered]@{
                exitCode = 0
                runFullStatic = $false
                outputLines = @('PASS')
            }
            comparator = [ordered]@{
                exitCode = 3
                disposition = 'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
                outputFileName = $ComparisonName
            }
        }
        regeneratedOutputs = [ordered]@{
            classes = [ordered]@{
                sourceRelativePath = $ClassesRelativePath
                snapshotFileName = $ClassesSnapshotName
                bytes = [long]$classes.bytes
                sha256 = [string]$classes.sha256
            }
            networks = [ordered]@{
                sourceRelativePath = $NetworksRelativePath
                snapshotFileName = $NetworksSnapshotName
                bytes = [long]$networks.bytes
                sha256 = [string]$networks.sha256
            }
            converterManifestMatchedSnapshots = $true
            c78VerifierAcceptedManifestAndCurrentOutputs = $true
            comparatorMatchedClassesSnapshot = $true
            finalProductionRehashMatchedSnapshots = $true
        }
        artifactsWrittenBeforeCompleteManifest = $artifactReports
        publication = [ordered]@{
            stagingDirectoryName = $stageName
            finalDirectoryName = $FinalDirectoryName
            finalDirectoryAtomicMoveRequired = $true
            existingOutputOverwriteAllowed = $false
            retryPolicy =
                'failed exact-owned current stage is removed; stale/ambiguous/final bundles require manual review'
            completeManifestWrittenLast = $true
        }
        decision = [ordered]@{
            disposition = 'UNSTABLE_THIRD_CLASSES_HASH_STOP'
            exitCode = 3
            checkpointReproduced = $false
            known6EReproduced = $false
            productionApproved = $false
            semanticEquivalenceClaimedForOpaqueDrift = $false
            staticReplayPermitted = $false
            onlineRuntimeQualificationPermitted = $false
        }
        productionApproved = $false
    }
    Write-SelfTestBytes `
        -Path (Join-Path $bundle $CompleteManifestName) `
        -Bytes (ConvertTo-PrettyJsonBytes -Value $complete)
    return [pscustomobject]@{
        root = Get-NormalizedFullPath -Path $CaseRoot
        evidenceRoot = $EvidenceRoot
        bundle = Get-NormalizedFullPath -Path $bundle
        baseline = $syntheticBaseline
    }
}

function Update-SelfTestCompleteManifest {
    param(
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][scriptblock]$Mutation
    )
    $path = Join-Path $BundlePath $CompleteManifestName
    $artifact = Get-FileArtifact -Path $path -FileOwner 'self-test complete manifest'
    $manifest = Read-StrictJsonArtifact `
        -Artifact $artifact -JsonOwner 'self-test complete manifest'
    & $Mutation $manifest
    if ($manifest.capturedAtUtc -is [DateTime]) {
        $manifest.capturedAtUtc = $manifest.capturedAtUtc.ToUniversalTime().ToString(
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
            [Globalization.CultureInfo]::InvariantCulture)
    }
    elseif ($manifest.capturedAtUtc -is [DateTimeOffset]) {
        $manifest.capturedAtUtc = $manifest.capturedAtUtc.ToUniversalTime().ToString(
            "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
            [Globalization.CultureInfo]::InvariantCulture)
    }
    Write-SelfTestBytes -Path $path -Bytes (ConvertTo-PrettyJsonBytes -Value $manifest)
}

function Update-SelfTestArtifactReport {
    param(
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][string]$FileName
    )
    $identity = Get-SelfTestFileArtifact -Path (Join-Path $BundlePath $FileName)
    Update-SelfTestCompleteManifest `
        -BundlePath $BundlePath `
        -Mutation {
            param($manifest)
            $matches = @($manifest.artifactsWrittenBeforeCompleteManifest | Where-Object {
                    $_.fileName -ceq $FileName
                })
            if ($matches.Count -ne 1) { throw 'self-test report lookup failed.' }
            $matches[0].bytes = [long]$identity.bytes
            $matches[0].sha256 = [string]$identity.sha256
        }
}

function Update-SelfTestComparison {
    param(
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][scriptblock]$Mutation
    )
    $path = Join-Path $BundlePath $ComparisonName
    $artifact = Get-FileArtifact -Path $path -FileOwner 'self-test comparison'
    $comparison = Read-StrictJsonArtifact `
        -Artifact $artifact -JsonOwner 'self-test comparison'
    & $Mutation $comparison
    Write-SelfTestBytes `
        -Path $path -Bytes (ConvertTo-ComparatorCanonicalJsonBytes -Value $comparison)
    Update-SelfTestArtifactReport -BundlePath $BundlePath -FileName $ComparisonName
}

$script:selfTestPositive = 0
$script:selfTestNegative = 0

function Assert-SelfTestTrue {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if (-not $Condition) { throw "self-test failed: $Message" }
    $script:selfTestPositive++
}

function Assert-SelfTestThrows {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string]$Message,
        [Parameter(Mandatory = $true)][string]$ExpectedText
    )
    try {
        & $Action
    }
    catch {
        $observed = [string]$_.Exception.Message
        if ($observed.IndexOf($ExpectedText, [StringComparison]::Ordinal) -lt 0) {
            throw (
                "self-test negative failed for the wrong reason: $Message; " +
                "expected='$ExpectedText'; observed='$observed'")
        }
        $script:selfTestNegative++
        return
    }
    throw "self-test negative did not fail: $Message"
}

function Invoke-SelfTestCore {
    param([Parameter(Mandatory = $true)]$Case)
    $completePath = Join-Path $Case.bundle $CompleteManifestName
    $completeArtifact = Get-FileArtifact `
        -Path $completePath -FileOwner 'self-test complete manifest'
    $complete = Read-StrictJsonArtifact `
        -Artifact $completeArtifact -JsonOwner 'self-test complete manifest'
    Assert-BaselineInputIdentityBinding `
        -Baseline $Case.baseline -CurrentInputs $complete.baseline.currentInputs
    return Invoke-BundleVerificationCore `
        -RepositoryRoot $Case.root `
        -EvidenceRelativeRoot $Case.evidenceRoot `
        -BundlePath $Case.bundle `
        -SkipHistoricalGitObjects `
        -SkipPinnedProductionIdentities
}

function Invoke-BundleVerifierSelfTest {
    $tempBase = Get-NormalizedFullPath -Path ([IO.Path]::GetTempPath())
    $tempRoot = Join-Path $tempBase (
        'lasal-bundle-verifier-selftest-' + [Guid]::NewGuid().ToString('N'))
    if (-not (Test-PathInsideRoot -Root $tempBase -Path $tempRoot)) {
        throw 'self-test temp root escaped the system temp directory.'
    }
    [void][IO.Directory]::CreateDirectory($tempRoot)
    $junctionPath = $null
    try {
        $valid = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'valid')
        $validResult = Invoke-SelfTestCore -Case $valid
        Assert-SelfTestTrue `
            -Condition (
                $validResult.classificationExitCode -eq 3 -and
                -not $validResult.productionApproved -and
                -not $validResult.onlineRuntimeQualificationPermitted) `
            -Message 'valid nonapproval bundle'

        $repeatedRestorationIndexes =
            [Collections.Generic.HashSet[int]]::new()
        Add-ExactRestorationCommandLineIndex `
            -CommandLineIndexes $repeatedRestorationIndexes `
            -CommandLineIndex 8
        Add-ExactRestorationCommandLineIndex `
            -CommandLineIndexes $repeatedRestorationIndexes `
            -CommandLineIndex 10
        Assert-SelfTestTrue `
            -Condition (
                $repeatedRestorationIndexes.Count -eq 2 -and
                $repeatedRestorationIndexes.Contains(8) -and
                $repeatedRestorationIndexes.Contains(10)) `
            -Message 'same restoration command at distinct line indexes is accepted'
        Assert-SelfTestTrue `
            -Condition (-not $repeatedRestorationIndexes.Contains(12)) `
            -Message 'unreported restoration occurrence is not registered'
        Assert-SelfTestThrows `
            -Message 'duplicate restoration commandLineIndex' `
            -ExpectedText 'load restoration report reuses commandLineIndex 8' `
            -Action {
            Add-ExactRestorationCommandLineIndex `
                -CommandLineIndexes $repeatedRestorationIndexes `
                -CommandLineIndex 8
        }

        [byte[]]$historicalLfBytes = $Utf8NoBom.GetBytes("alpha`nbeta`n")
        [byte[]]$historicalMixedBytes = $Utf8NoBom.GetBytes("alpha`r`nbeta`n")
        $historicalBlobOid = 'a' * 40
        $historicalLfBlob = [pscustomobject]@{
            bytes = [long]$historicalLfBytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $historicalLfBytes
            content = $historicalLfBytes
        }
        $historicalLfReport = [pscustomobject]@{
            owner = 'self-test historical LF artifact'
            relativePath = 'self-test/exact.ps1'
            bytes = [long]$historicalLfBytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $historicalLfBytes
        }
        Assert-HistoricalManifestIdentityMatchesGitBlob `
            -Report $historicalLfReport `
            -Blob $historicalLfBlob `
            -BlobOid $historicalBlobOid
        Assert-SelfTestTrue `
            -Condition $true `
            -Message 'historical raw Git identity is accepted exactly'
        $historicalMixedReport = [pscustomobject]@{
            owner = 'self-test historical mixed-EOL physical artifact'
            relativePath = 'self-test/bridged.ps1'
            bytes = [long]$historicalMixedBytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $historicalMixedBytes
        }
        $historicalMixedBridge = [ordered]@{
            relativePath = 'self-test/bridged.ps1'
            physicalBytes = [long]$historicalMixedBytes.LongLength
            physicalSha256 = Get-BytesSha256 -Bytes $historicalMixedBytes
            gitBlobOid = $historicalBlobOid
            canonicalLfBytes = [long]$historicalLfBytes.LongLength
            canonicalLfSha256 = Get-BytesSha256 -Bytes $historicalLfBytes
        }
        Assert-HistoricalManifestIdentityMatchesGitBlob `
            -Report $historicalMixedReport `
            -Blob $historicalLfBlob `
            -BlobOid $historicalBlobOid `
            -TextIdentityBridge $historicalMixedBridge
        Assert-SelfTestTrue `
            -Condition $true `
            -Message 'historical mixed-EOL physical/Git bridge is accepted exactly'
        Assert-SelfTestThrows `
            -Message 'historical EOL bridge requires an exact allowlist' `
            -ExpectedText 'no reviewed historical text identity bridge is allowed' `
            -Action {
            Assert-HistoricalManifestIdentityMatchesGitBlob `
                -Report $historicalMixedReport `
                -Blob $historicalLfBlob `
                -BlobOid $historicalBlobOid
        }
        $historicalMutatedReport = [pscustomobject]@{
            owner = 'self-test historical physical tuple mutation'
            relativePath = 'self-test/bridged.ps1'
            bytes = [long]$historicalMixedBytes.LongLength
            sha256 = 'B' * 64
        }
        Assert-SelfTestThrows `
            -Message 'historical physical tuple mutation' `
            -ExpectedText 'exact reviewed physical/Git text identity bridge' `
            -Action {
            Assert-HistoricalManifestIdentityMatchesGitBlob `
                -Report $historicalMutatedReport `
                -Blob $historicalLfBlob `
                -BlobOid $historicalBlobOid `
                -TextIdentityBridge $historicalMixedBridge
        }
        Assert-SelfTestThrows `
            -Message 'historical Git blob oid mutation' `
            -ExpectedText 'exact reviewed physical/Git text identity bridge' `
            -Action {
            Assert-HistoricalManifestIdentityMatchesGitBlob `
                -Report $historicalMixedReport `
                -Blob $historicalLfBlob `
                -BlobOid ('c' * 40) `
                -TextIdentityBridge $historicalMixedBridge
        }
        [byte[]]$historicalMutatedLfBytes = $Utf8NoBom.GetBytes("alpha`nbetb`n")
        $historicalMutatedBlob = [pscustomobject]@{
            bytes = [long]$historicalMutatedLfBytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $historicalMutatedLfBytes
            content = $historicalMutatedLfBytes
        }
        Assert-SelfTestThrows `
            -Message 'historical canonical content mutation' `
            -ExpectedText 'exact reviewed physical/Git text identity bridge' `
            -Action {
            Assert-HistoricalManifestIdentityMatchesGitBlob `
                -Report $historicalMixedReport `
                -Blob $historicalMutatedBlob `
                -BlobOid $historicalBlobOid `
                -TextIdentityBridge $historicalMixedBridge
        }

        $validRawText = $Utf8Strict.GetString([IO.File]::ReadAllBytes(
                (Join-Path $valid.bundle $RawDeltaName)))
        $validRawLines = @([regex]::Split($validRawText, '\r?\n'))
        $validCompleteArtifact = Get-FileArtifact `
            -Path (Join-Path $valid.bundle $CompleteManifestName) `
            -FileOwner 'self-test marker complete manifest'
        $validComplete = Read-StrictJsonArtifact `
            -Artifact $validCompleteArtifact `
            -JsonOwner 'self-test marker complete manifest'
        Assert-SelfTestThrows -Message 'duplicate Start Application marker' `
            -ExpectedText 'raw session marker global count differs' -Action {
            [void](Assert-ReplayedRawSessionMarkers `
                    -Lines @($validRawLines +
                        '[12:00:20 P:00100 T:00001 (INFO) GUI] Start Application at 2026-08-11 12:00:20') `
                    -Session $validComplete.isolatedSession)
        }
        Assert-SelfTestThrows -Message 'duplicate Do exit marker' `
            -ExpectedText 'raw session marker global count differs' -Action {
            [void](Assert-ReplayedRawSessionMarkers `
                    -Lines @($validRawLines +
                        '[12:00:20 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...') `
                    -Session $validComplete.isolatedSession)
        }
        Assert-SelfTestThrows -Message 'duplicate exit done marker' `
            -ExpectedText 'raw session marker global count differs' -Action {
            [void](Assert-ReplayedRawSessionMarkers `
                    -Lines @($validRawLines +
                        '[12:00:20 P:00100 T:00001 (INFO) GUI] ...LC2 exit done.') `
                    -Session $validComplete.isolatedSession)
        }

        $tamper = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'tamper')
        [IO.File]::AppendAllText(
            (Join-Path $tamper.bundle $TranscriptName),
            'tamper',
            $Utf8NoBom)
        Assert-SelfTestThrows -Message 'tampered transcript' `
            -ExpectedText 'does not bind the exact bundle file' -Action {
            [void](Invoke-SelfTestCore -Case $tamper)
        }

        $extra = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'extra')
        Write-SelfTestBytes `
            -Path (Join-Path $extra.bundle 'extra.txt') -Bytes $Utf8NoBom.GetBytes('x')
        Assert-SelfTestThrows -Message 'extra file' `
            -ExpectedText 'final bundle inventory count' -Action {
            [void](Invoke-SelfTestCore -Case $extra)
        }

        $missing = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'missing')
        [IO.File]::Delete((Join-Path $missing.bundle $RawDeltaName))
        Assert-SelfTestThrows -Message 'missing file' `
            -ExpectedText 'final bundle inventory count' -Action {
            [void](Invoke-SelfTestCore -Case $missing)
        }

        $reparse = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'reparse')
        $junctionTarget = Join-Path $tempRoot 'junction-target'
        [void][IO.Directory]::CreateDirectory($junctionTarget)
        $junctionPath = Join-Path $reparse.bundle 'junction'
        [void](New-Item `
                -ItemType Junction -Path $junctionPath -Target $junctionTarget -ErrorAction Stop)
        Assert-SelfTestThrows -Message 'reparse descendant' `
            -ExpectedText 'contains a descendant reparse point' -Action {
            [void](Invoke-SelfTestCore -Case $reparse)
        }
        [IO.Directory]::Delete($junctionPath)
        $junctionPath = $null

        $ads = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'ads')
        $adsPath = Join-Path $ads.bundle $TranscriptName
        [IO.File]::WriteAllText($adsPath + ':bundle-verifier-selftest', 'x', $Utf8NoBom)
        Assert-SelfTestThrows -Message 'file ADS' `
            -ExpectedText 'contains a non-default data stream' -Action {
            [void](Invoke-SelfTestCore -Case $ads)
        }

        $duplicate = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'duplicate')
        $duplicatePath = Join-Path $duplicate.bundle $CompleteManifestName
        $duplicateText = $Utf8Strict.GetString([IO.File]::ReadAllBytes($duplicatePath))
        $duplicateText = $duplicateText.Replace(
            '  "complete": true,',
            "  `"complete`": true,`n  `"complete`": true,")
        Write-SelfTestBytes -Path $duplicatePath -Bytes $Utf8NoBom.GetBytes($duplicateText)
        Assert-SelfTestThrows -Message 'duplicate JSON key' `
            -ExpectedText 'has duplicate key' -Action {
            [void](Invoke-SelfTestCore -Case $duplicate)
        }

        $caseCollision = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'case-collision')
        $casePath = Join-Path $caseCollision.bundle $CompleteManifestName
        $caseText = $Utf8Strict.GetString([IO.File]::ReadAllBytes($casePath))
        $caseText = $caseText.Replace(
            '  "productionApproved": false',
            "  `"ProductionApproved`": false,`n  `"productionApproved`": false")
        Write-SelfTestBytes -Path $casePath -Bytes $Utf8NoBom.GetBytes($caseText)
        Assert-SelfTestThrows -Message 'case-colliding JSON key' `
            -ExpectedText 'has case-colliding key' -Action {
            [void](Invoke-SelfTestCore -Case $caseCollision)
        }

        $extraKey = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'extra-key')
        Update-SelfTestCompleteManifest -BundlePath $extraKey.bundle -Mutation {
            param($manifest)
            $manifest | Add-Member -NotePropertyName 'unexpected' -NotePropertyValue 0
        }
        Assert-SelfTestThrows -Message 'extra manifest key' `
            -ExpectedText 'complete manifest key count differs' -Action {
            [void](Invoke-SelfTestCore -Case $extraKey)
        }

        $approval = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'approval')
        Update-SelfTestCompleteManifest -BundlePath $approval.bundle -Mutation {
            param($manifest)
            $manifest.decision.productionApproved = $true
        }
        Assert-SelfTestThrows -Message 'approval flag' `
            -ExpectedText 'complete decision contains a production/runtime approval' `
            -Action {
            [void](Invoke-SelfTestCore -Case $approval)
        }

        $badHash = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'bad-hash')
        Update-SelfTestCompleteManifest -BundlePath $badHash.bundle -Mutation {
            param($manifest)
            $manifest.artifactsWrittenBeforeCompleteManifest[3].sha256 = '0' * 64
        }
        Assert-SelfTestThrows -Message 'artifact identity hash' `
            -ExpectedText 'does not bind the exact bundle file' -Action {
            [void](Invoke-SelfTestCore -Case $badHash)
        }

        $noncanonical = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'noncanonical-comparison')
        $comparisonPath = Join-Path $noncanonical.bundle $ComparisonName
        [IO.File]::AppendAllText($comparisonPath, " `n", $Utf8NoBom)
        Update-SelfTestArtifactReport `
            -BundlePath $noncanonical.bundle -FileName $ComparisonName
        Assert-SelfTestThrows -Message 'noncanonical comparison JSON' `
            -ExpectedText 'comparison JSON is not exact comparator-canonical bytes' `
            -Action {
            [void](Invoke-SelfTestCore -Case $noncanonical)
        }

        $rawLink = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'raw-link')
        $rawManifestPath = Join-Path $rawLink.bundle $RawManifestName
        $rawManifestArtifact = Get-FileArtifact `
            -Path $rawManifestPath -FileOwner 'self-test raw manifest'
        $rawManifestObject = Read-StrictJsonArtifact `
            -Artifact $rawManifestArtifact -JsonOwner 'self-test raw manifest'
        $rawManifestObject.TranscriptSha256 = '0' * 64
        if ($rawManifestObject.CapturedAtUtc -is [DateTime]) {
            $rawManifestObject.CapturedAtUtc =
                $rawManifestObject.CapturedAtUtc.ToUniversalTime().ToString(
                    "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
                    [Globalization.CultureInfo]::InvariantCulture)
        }
        Write-SelfTestBytes `
            -Path $rawManifestPath `
            -Bytes (ConvertTo-PrettyJsonBytes -Value $rawManifestObject)
        Update-SelfTestArtifactReport `
            -BundlePath $rawLink.bundle -FileName $RawManifestName
        Assert-SelfTestThrows -Message 'raw manifest transcript linkage' `
            -ExpectedText 'raw bounded manifest file/session linkage differs' -Action {
            [void](Invoke-SelfTestCore -Case $rawLink)
        }

        $comparisonExtra = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'comparison-nested-extra')
        Update-SelfTestComparison -BundlePath $comparisonExtra.bundle -Mutation {
            param($comparison)
            $comparison.recordParser | Add-Member `
                -NotePropertyName unexpected -NotePropertyValue $true
        }
        Assert-SelfTestThrows -Message 'comparison nested extra key' `
            -ExpectedText 'comparison.recordParser key count differs' -Action {
            [void](Invoke-SelfTestCore -Case $comparisonExtra)
        }

        $comparisonType = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'comparison-nested-type')
        Update-SelfTestComparison -BundlePath $comparisonType.bundle -Mutation {
            param($comparison)
            $comparison.changedCheckpointOwners[0].owner = 7
        }
        Assert-SelfTestThrows -Message 'comparison nested primitive type' `
            -ExpectedText 'comparison.changedCheckpointOwners[0].owner is not a JSON string' `
            -Action {
            [void](Invoke-SelfTestCore -Case $comparisonType)
        }

        $prologue = New-SelfTestBundle -CaseRoot (Join-Path $tempRoot 'prologue')
        Update-SelfTestCompleteManifest -BundlePath $prologue.bundle -Mutation {
            param($manifest)
            $manifest.isolatedSession.preStartPrologue[0].source = 'FORGED'
            $manifest.isolatedSession.preStartPrologue[0].body = 'FORGED'
        }
        Assert-SelfTestThrows -Message 'prologue source/body raw binding' `
            -ExpectedText 'preStartPrologue[0] does not bind its raw line' -Action {
            [void](Invoke-SelfTestCore -Case $prologue)
        }

        $restoration = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'restoration-terminal')
        Update-SelfTestCompleteManifest -BundlePath $restoration.bundle -Mutation {
            param($manifest)
            $manifest.isolatedSession.loadRestorationCommands[0].successLineIndex = 8
        }
        Assert-SelfTestThrows -Message 'restoration exact ledger terminal' `
            -ExpectedText 'load restoration successLineIndex differs from its exact ledger terminal' `
            -Action {
            [void](Invoke-SelfTestCore -Case $restoration)
        }

        $sharedTerminal = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'shared-terminal-report')
        $sharedRawText = $Utf8Strict.GetString([IO.File]::ReadAllBytes(
                (Join-Path $sharedTerminal.bundle $RawDeltaName)))
        $sharedRawLines = [regex]::Split($sharedRawText.TrimEnd("`r", "`n"), '\r?\n')
        Update-SelfTestCompleteManifest -BundlePath $sharedTerminal.bundle -Mutation {
            param($manifest)
            $manifest.isolatedSession.commandTerminalLedger[1].terminalLineIndex = 11
            $manifest.isolatedSession.commandTerminalLedger[1].terminalRaw =
                $sharedRawLines[11]
        }
        Assert-SelfTestThrows -Message 'shared reported terminal' `
            -ExpectedText 'ledger terminal PID/TID differs from its report' -Action {
            [void](Invoke-SelfTestCore -Case $sharedTerminal)
        }

        Assert-SelfTestThrows -Message 'missing raw terminal' `
            -ExpectedText 'raw command/terminal count is not exact 1:1' -Action {
            [void](Get-ReplayedCommandTerminalLedger -Lines @(
                    '[12:00:00 P:00100 T:00001 (INFO) CmdProc] Executing command ''Rebuild project'''))
        }
        Assert-SelfTestThrows -Message 'orphan raw terminal' `
            -ExpectedText 'raw command/terminal count is not exact 1:1' -Action {
            [void](Get-ReplayedCommandTerminalLedger -Lines @(
                    '[12:00:00 P:00100 T:00001 (INFO) CmdProc] Last command succeeded.'))
        }
        Assert-SelfTestThrows -Message 'intervening same-thread command' `
            -ExpectedText 'raw command does not have exactly one next same-thread terminal' `
            -Action {
            [void](Get-ReplayedCommandTerminalLedger -Lines @(
                    '[12:00:00 P:00100 T:00001 (INFO) CmdProc] Executing command ''First''',
                    '[12:00:01 P:00100 T:00001 (INFO) CmdProc] Executing command ''Second''',
                    '[12:00:02 P:00100 T:00001 (INFO) CmdProc] Last command succeeded.',
                    '[12:00:03 P:00100 T:00001 (INFO) CmdProc] Last command succeeded.'))
        }

        $baselinePath = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'baseline-path')
        Update-SelfTestCompleteManifest -BundlePath $baselinePath.bundle -Mutation {
            param($manifest)
            $manifest.baseline.currentInputs[0].relativePath = 'synthetic/forged.st'
        }
        Assert-SelfTestThrows -Message 'baseline ordered input path' `
            -ExpectedText 'differs from the ordered historical baseline identity' -Action {
            [void](Invoke-SelfTestCore -Case $baselinePath)
        }

        $baselineHash = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'baseline-hash')
        Update-SelfTestCompleteManifest -BundlePath $baselineHash.bundle -Mutation {
            param($manifest)
            $manifest.baseline.currentInputs[0].sha256 = '0' * 64
        }
        Assert-SelfTestThrows -Message 'baseline ordered input hash' `
            -ExpectedText 'differs from the ordered historical baseline identity' -Action {
            [void](Invoke-SelfTestCore -Case $baselineHash)
        }

        $rawShaCase = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'raw-sha-case')
        $rawShaPath = Join-Path $rawShaCase.bundle $RawManifestName
        $rawShaArtifact = Get-FileArtifact `
            -Path $rawShaPath -FileOwner 'self-test raw SHA manifest'
        $rawShaObject = Read-StrictJsonArtifact `
            -Artifact $rawShaArtifact -JsonOwner 'self-test raw SHA manifest'
        $rawShaObject.RawDeltaSha256 =
            ([string]$rawShaObject.RawDeltaSha256).ToUpperInvariant()
        if ($rawShaObject.CapturedAtUtc -is [DateTime]) {
            $rawShaObject.CapturedAtUtc =
                $rawShaObject.CapturedAtUtc.ToUniversalTime().ToString(
                    "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
                    [Globalization.CultureInfo]::InvariantCulture)
        }
        Write-SelfTestBytes `
            -Path $rawShaPath -Bytes (ConvertTo-PrettyJsonBytes -Value $rawShaObject)
        Update-SelfTestArtifactReport `
            -BundlePath $rawShaCase.bundle -FileName $RawManifestName
        Assert-SelfTestThrows -Message 'raw manifest SHA casing' `
            -ExpectedText 'raw manifest.RawDeltaSha256 is not canonical lowercase SHA-256' `
            -Action {
            [void](Invoke-SelfTestCore -Case $rawShaCase)
        }

        $invalidDate = New-SelfTestBundle `
            -CaseRoot (Join-Path $tempRoot 'invalid-date')
        Update-SelfTestCompleteManifest -BundlePath $invalidDate.bundle -Mutation {
            param($manifest)
            $manifest.capturedAtUtc = 'not-a-date'
        }
        Assert-SelfTestThrows -Message 'invalid capturedAtUtc' `
            -ExpectedText '$.capturedAtUtc is not canonical seven-digit UTC ISO text' `
            -Action {
            [void](Invoke-SelfTestCore -Case $invalidDate)
        }
    }
    finally {
        if ($null -ne $junctionPath -and (Test-Path -LiteralPath $junctionPath)) {
            $attributes = [IO.File]::GetAttributes($junctionPath)
            if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                [IO.Directory]::Delete($junctionPath)
            }
        }
        if (Test-Path -LiteralPath $tempRoot -PathType Container) {
            Assert-NoReparsePointDescendants `
                -Directory $tempRoot -DirectoryOwner 'self-test cleanup'
            if (-not (Test-PathInsideRoot -Root $tempBase -Path $tempRoot)) {
                throw 'self-test cleanup escaped the system temp directory.'
            }
            [IO.Directory]::Delete($tempRoot, $true)
        }
    }
    Write-Output (
        "PASS $Owner.SelfTest Positive=$script:selfTestPositive " +
        "Negative=$script:selfTestNegative")
}

try {
    Assert-PowerShell7 -Phase 'bundle verifier entry'
    if ($PSCmdlet.ParameterSetName -ceq 'SelfTest') {
        Invoke-BundleVerifierSelfTest
        exit 0
    }
    Invoke-ProductionVerification -RequestedRepositoryRoot $RepositoryRoot
    exit 0
}
catch {
    [Console]::Error.WriteLine("BLOCKED: $($_.Exception.Message)")
    exit 4
}

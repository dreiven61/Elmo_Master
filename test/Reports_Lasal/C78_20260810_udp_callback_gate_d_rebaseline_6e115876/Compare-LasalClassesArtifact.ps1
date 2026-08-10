[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$Checkpoint =
        '55435791f6e91c9dcb4e06dcd25a11d77b382da7',
    [string]$CheckpointPath =
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    [string]$CandidatePath =
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    [string]$OutputPath,
    [switch]$CreateNew,
    [switch]$RunSelfTest,
    [Parameter(DontShow = $true)]
    [switch]$EmitJsonSelfTestFixtureBase64
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$ScriptSchema = 'LasalClassesArtifactComparison/v1'
$ClassesSignature = 'SigmatekLasal2Binary' + [char]0
$Latin1 = [Text.Encoding]::GetEncoding(28591)
$Utf8NoBom = New-Object Text.UTF8Encoding($false, $true)
$CanonicalClassesPath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'

# Source-path-to-next-source-path segments mirror Get-ClassDatabaseRecord in the
# focused verifier, but they are diagnostic only. Approval and record equality
# use the independent AA 03 + class-name-length + AA true record headers.
$GateDTargetDefinitions = @(
    [ordered]@{
        owner = '_UDPTransceiver'
        sourcePath = '.\Class\_UDPTransceiver\_UDPTransceiver.st'
    },
    [ordered]@{
        owner = 'LMCDiagnosticsService'
        sourcePath =
            '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
    },
    [ordered]@{
        owner = 'LMCUdpCallbackSender'
        sourcePath =
            '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'
    },
    [ordered]@{
        owner = 'TCPMotionInterface'
        sourcePath =
            '.\Class\TCPMotionInterface\TCPMotionInterface.st'
    }
)

# These are the stronger bounded-record contracts already used by
# Verify-LasalUdpCallbackContract.ps1. They include metadata before the source
# path and stop at the next class-name boundary.
$ProtectedRecordDefinitions = @(
    [ordered]@{
        owner = '_StdLib'
        sourcePath = '.\Class\_StdLib\_StdLib.st'
        className = '_StdLib'
        nextSourcePath = '.\Class\_SyncMeasure\_SyncMeasure.st'
        nextClassName = '_SyncMeasure'
    },
    [ordered]@{
        owner = 'CriticalSection'
        sourcePath = '.\Class\CriticalSection\CriticalSection.st'
        className = 'CriticalSection'
        nextSourcePath = '.\Class\DiasMaster\DiasMaster.st'
        nextClassName = 'DiasMaster'
    }
)

$FrozenOpaqueVendorOwners = @(
    '_LMCABSEncoder',
    '_LMCAxis',
    '_LMCAxisBase',
    '_LMCAxisRef',
    '_LMCAxisVis',
    '_LMCAxisVisInt',
    '_LMCAxisVisLogHandle',
    '_LMCAxisVisLogViewer',
    '_LMCAxisVisPara',
    '_LMCAxisVOVMonitoring',
    '_LMCBaseCoord',
    '_LMCBeltAxis',
    '_LMCCalcModelBase',
    '_LMCCalcModelController',
    '_LMCMath_SO3',
    '_LMCMathFunctions',
    '_LMCProfile',
    '_LMCProfileBase',
    '_LMCProfileBuffer',
    '_LMCProfileLog',
    '_LMCProfileVis',
    '_LMCProfileVisAxis',
    '_LMCProfileVisInt',
    '_LMCProfileVisMovePara',
    '_LMCPublisher',
    '_LMCRefBase',
    '_LMCRobotBase',
    '_LMCRobotLog',
    '_LMCSafety',
    '_LMCSplineBuffer',
    '_LMCTableBuffer',
    '_LMCTool',
    'Controller',
    'MoveSplineTable',
    'PosController',
    'SigCLib'
)

function Throw-ComparatorBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw "BLOCKED: $Message"
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace(
            '-',
            '')
    }
    finally {
        $sha.Dispose()
    }
}

function Get-ByteRangeSha256 {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$Length
    )
    if (($Start -lt 0) -or ($Length -lt 0) -or
        (($Start + $Length) -gt $Bytes.Length)) {
        Throw-ComparatorBlocker 'SHA-256 byte range is outside the artifact.'
    }
    $slice = New-Object byte[] $Length
    if ($Length -gt 0) {
        [Array]::Copy($Bytes, $Start, $slice, 0, $Length)
    }
    return Get-BytesSha256 -Bytes $slice
}

function Test-ByteRangesEqual {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][int]$LeftStart,
        [Parameter(Mandatory = $true)][int]$LeftLength,
        [Parameter(Mandatory = $true)][byte[]]$Right,
        [Parameter(Mandatory = $true)][int]$RightStart,
        [Parameter(Mandatory = $true)][int]$RightLength
    )
    Initialize-ByteDiffType
    return [CodexLasalClassesComparatorByteDiffV1]::RangeEquals(
        $Left,
        $LeftStart,
        $LeftLength,
        $Right,
        $RightStart,
        $RightLength)
}

function Get-OrdinalCount {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [switch]$IgnoreCase
    )
    if ($Needle.Length -eq 0) {
        Throw-ComparatorBlocker 'An empty ordinal-search token is invalid.'
    }
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

function Assert-ClassesSignature {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )
    if ($Bytes.Length -lt 32) {
        Throw-ComparatorBlocker "$ArtifactOwner is too short to be Classes.lcb."
    }
    $text = $Latin1.GetString($Bytes)
    if ((-not $text.StartsWith(
                $ClassesSignature,
                [StringComparison]::Ordinal)) -or
        ((Get-OrdinalCount -Text $text -Needle $ClassesSignature) -ne 1)) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner does not have one exact SigmatekLasal2Binary " +
            'signature at byte zero.')
    }
}

function Get-GitExecutable {
    $command = Get-Command git -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        Throw-ComparatorBlocker 'git is not available.'
    }
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
            Throw-ComparatorBlocker (
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
            Throw-ComparatorBlocker "$Operation could not start git."
        }
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            $detail = $stderr.Trim()
            if ($detail.Length -gt 240) {
                $detail = $detail.Substring(0, 240)
            }
            Throw-ComparatorBlocker (
                "$Operation failed with exit $($process.ExitCode): $detail")
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
        [Parameter(Mandatory = $true)][string]$BlobOid
    )
    if ($BlobOid -cnotmatch '^[0-9a-fA-F]{40,64}$') {
        Throw-ComparatorBlocker 'resolved Git blob OID is invalid.'
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
            Throw-ComparatorBlocker 'Git blob read could not start git.'
        }
        $process.StandardOutput.BaseStream.CopyTo($memory)
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            Throw-ComparatorBlocker (
                "Git blob read failed with exit $($process.ExitCode): " +
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
    param([string]$RequestedRoot)
    if ([string]::IsNullOrWhiteSpace($RequestedRoot)) {
        $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $PSScriptRoot)
        while (($null -ne $cursor) -and
            (-not (Test-Path -LiteralPath (Join-Path $cursor.FullName '.git')))) {
            $cursor = $cursor.Parent
        }
        if ($null -eq $cursor) {
            Throw-ComparatorBlocker 'repository root could not be inferred.'
        }
        $resolved = $cursor.FullName.TrimEnd('\')
    }
    else {
        if (-not (Test-Path -LiteralPath $RequestedRoot -PathType Container)) {
            Throw-ComparatorBlocker 'RepositoryRoot is not a directory.'
        }
        $resolved = (Resolve-Path -LiteralPath $RequestedRoot).Path.TrimEnd('\')
    }
    $gitRoot = Invoke-GitText `
        -Root $resolved `
        -Arguments @('rev-parse', '--show-toplevel') `
        -Operation 'Git root resolution'
    $resolvedGitRoot = (Resolve-Path -LiteralPath $gitRoot).Path.TrimEnd('\')
    if (-not [string]::Equals(
            $resolved,
            $resolvedGitRoot,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-ComparatorBlocker 'RepositoryRoot must be the Git worktree root.'
    }
    return $resolved
}

function Get-NormalizedGitPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    $normalized = $Path.Replace('\', '/')
    if ([string]::IsNullOrWhiteSpace($normalized) -or
        [IO.Path]::IsPathRooted($normalized) -or
        ($normalized -notmatch '^[A-Za-z0-9_./-]+$') -or
        ($normalized.StartsWith('/')) -or
        ($normalized.EndsWith('/')) -or
        ($normalized.Split('/') -contains '..')) {
        Throw-ComparatorBlocker 'CheckpointPath is not a safe repository path.'
    }
    return $normalized
}

function Resolve-CheckpointBlob {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RequestedCheckpoint,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )
    if ([string]::IsNullOrWhiteSpace($RequestedCheckpoint) -or
        ($RequestedCheckpoint -cnotmatch '^[0-9a-fA-F]{40}$')) {
        Throw-ComparatorBlocker (
            'Checkpoint must be one full 40-hex Git commit or blob OID.')
    }
    $path = Get-NormalizedGitPath -Path $RelativePath
    $objectOid = Invoke-GitText `
        -Root $Root `
        -Arguments @(
            'rev-parse',
            '--verify',
            "$RequestedCheckpoint^{object}") `
        -Operation 'Checkpoint object resolution'
    if ($objectOid -cnotmatch '^[0-9a-fA-F]{40,64}$') {
        Throw-ComparatorBlocker 'Checkpoint did not resolve to one Git object.'
    }
    $objectType = Invoke-GitText `
        -Root $Root `
        -Arguments @('cat-file', '-t', $objectOid) `
        -Operation 'Checkpoint object type query'
    if ($objectType -ceq 'blob') {
        return [ordered]@{
            requested = $RequestedCheckpoint
            kind = 'blob'
            resolvedRevision = $null
            relativePath = $path
            blobOid = $objectOid.ToLowerInvariant()
        }
    }
    $commitOid = Invoke-GitText `
        -Root $Root `
        -Arguments @(
            'rev-parse',
            '--verify',
            "$RequestedCheckpoint^{commit}") `
        -Operation 'Checkpoint commit resolution'
    if ($commitOid -cnotmatch '^[0-9a-fA-F]{40,64}$') {
        Throw-ComparatorBlocker 'Checkpoint is neither a blob nor a commit revision.'
    }
    $blobOid = Invoke-GitText `
        -Root $Root `
        -Arguments @(
            'rev-parse',
            '--verify',
            "${commitOid}:$path") `
        -Operation 'Checkpoint path resolution'
    $blobType = Invoke-GitText `
        -Root $Root `
        -Arguments @('cat-file', '-t', $blobOid) `
        -Operation 'Checkpoint path type query'
    if (($blobOid -cnotmatch '^[0-9a-fA-F]{40,64}$') -or
        ($blobType -cne 'blob')) {
        Throw-ComparatorBlocker 'Checkpoint path did not resolve to a Git blob.'
    }
    return [ordered]@{
        requested = $RequestedCheckpoint
        kind = 'revision'
        resolvedRevision = $commitOid.ToLowerInvariant()
        relativePath = $path
        blobOid = $blobOid.ToLowerInvariant()
    }
}

function Resolve-CandidateFile {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RequestedPath
    )
    if ([string]::IsNullOrWhiteSpace($RequestedPath)) {
        Throw-ComparatorBlocker 'CandidatePath is empty.'
    }
    $combined = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        $RequestedPath
    }
    else {
        Join-Path $Root $RequestedPath.Replace('/', '\')
    }
    if (-not (Test-Path -LiteralPath $combined -PathType Leaf)) {
        Throw-ComparatorBlocker 'CandidatePath is not a readable file.'
    }
    $fullPath = (Resolve-Path -LiteralPath $combined).Path
    $rootPrefix = $Root.TrimEnd('\') + '\'
    if (-not $fullPath.StartsWith(
            $rootPrefix,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-ComparatorBlocker 'CandidatePath must stay inside RepositoryRoot.'
    }
    $item = Get-Item -LiteralPath $fullPath -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        Throw-ComparatorBlocker 'CandidatePath must not be a reparse-point alias.'
    }
    $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $item.DirectoryName -Force)
    while (($null -ne $cursor) -and
        $cursor.FullName.StartsWith(
            $Root,
            [StringComparison]::OrdinalIgnoreCase)) {
        if (($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-ComparatorBlocker (
                'CandidatePath parent must not use a reparse point.')
        }
        if ([string]::Equals(
                $cursor.FullName.TrimEnd('\'),
                $Root.TrimEnd('\'),
                [StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        $cursor = $cursor.Parent
    }
    $displayPath = $fullPath.Substring($rootPrefix.Length).Replace('\', '/')
    return [ordered]@{
        fullPath = $fullPath
        displayPath = $displayPath
    }
}

function Read-StableFileBytes {
    param([Parameter(Mandatory = $true)][string]$Path)
    $stream = New-Object IO.FileStream(
        $Path,
        [IO.FileMode]::Open,
        [IO.FileAccess]::Read,
        [IO.FileShare]::Read)
    $memory = New-Object IO.MemoryStream
    try {
        if ($stream.Length -gt [int]::MaxValue) {
            Throw-ComparatorBlocker 'CandidatePath is too large for this verifier.'
        }
        $stream.CopyTo($memory)
        if ($memory.Length -ne $stream.Length) {
            Throw-ComparatorBlocker 'CandidatePath changed while being read.'
        }
        return ,$memory.ToArray()
    }
    finally {
        $memory.Dispose()
        $stream.Dispose()
    }
}

function Get-LittleEndian24 {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][string]$FieldOwner
    )
    if (($Start -lt 0) -or (($Start + 3) -gt $Bytes.Length)) {
        Throw-ComparatorBlocker "$FieldOwner LE24 field is outside the artifact."
    }
    return [int](
        [int]$Bytes[$Start] -bor
        ([int]$Bytes[$Start + 1] -shl 8) -bor
        ([int]$Bytes[$Start + 2] -shl 16))
}

function Get-ClassOwnerInventory {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )
    Assert-ClassesSignature -Bytes $Bytes -ArtifactOwner $ArtifactOwner
    $text = $Latin1.GetString($Bytes)
    if ($text.Length -ne $Bytes.Length) {
        Throw-ComparatorBlocker "$ArtifactOwner byte/text offsets diverged."
    }
    Initialize-ByteDiffType
    $compiledInventory =
        [CodexLasalClassesComparatorByteDiffV1]::ParseInventory($Bytes)
    if ($compiledInventory.SourceOwners.Length -lt 2) {
        Throw-ComparatorBlocker "$ArtifactOwner has no bounded class inventory."
    }
    $seenOwners = @{}
    $seenPaths = @{}
    $sources = New-Object Collections.Generic.List[object]
    for ($index = 0;
         $index -lt $compiledInventory.SourceOwners.Length;
         $index++) {
        $owner = [string]$compiledInventory.SourceOwners[$index]
        $sourcePath = [string]$compiledInventory.SourcePaths[$index]
        $pathStart = [int]$compiledInventory.SourcePathStarts[$index]
        $markerStart = [int]$compiledInventory.SourceMarkerStarts[$index]
        $ownerKey = $owner.ToUpperInvariant()
        $pathKey = $sourcePath.ToUpperInvariant()
        if ($seenOwners.ContainsKey($ownerKey) -or
            $seenPaths.ContainsKey($pathKey)) {
            Throw-ComparatorBlocker (
                "$ArtifactOwner contains an ambiguous class owner record: $owner")
        }
        $seenOwners[$ownerKey] = $true
        $seenPaths[$pathKey] = $true
        $sources.Add([pscustomobject]@{
                Owner = $owner
                SourcePath = $sourcePath
                PathStart = $pathStart
                MarkerStart = $markerStart
            })
    }

    $headers = New-Object Collections.Generic.List[object]
    for ($index = 0;
         $index -lt $compiledInventory.HeaderOwners.Length;
         $index++) {
        $headerStart = [int]$compiledInventory.HeaderStarts[$index]
        $headers.Add([pscustomobject]@{
                Owner = [string]$compiledInventory.HeaderOwners[$index]
                Start = $headerStart
                NameStart = [int]($headerStart + 6)
            })
    }
    # The first _AxisBase entry is a format-specific preamble record. Every
    # following class must have one exact AA 03 true-record header.
    if ($headers.Count -ne ($sources.Count - 1)) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner true-header/source inventory count differs.")
    }
    $records = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $sources.Count; $index++) {
        $source = $sources[$index]
        if ($index -eq 0) {
            $start = 0
            $parser = 'first-special-preamble-to-next-header'
            if ($source.Owner -cne '_AxisBase') {
                Throw-ComparatorBlocker (
                    "$ArtifactOwner first special owner is not _AxisBase.")
            }
        }
        else {
            $header = $headers[$index - 1]
            if ($header.Owner -cne $source.Owner) {
                Throw-ComparatorBlocker (
                    "$ArtifactOwner true-header/source owner order differs at " +
                    "ordinal $($index + 1).")
            }
            $start = [int]$header.Start
            $parser = 'aa03-header-to-next-header-or-eof'
            if (($header.NameStart -ne ($header.Start + 6)) -or
                ($start -le $sources[$index - 1].PathStart) -or
                ($start -ge $source.MarkerStart)) {
                Throw-ComparatorBlocker (
                    "$ArtifactOwner true-header boundary is ambiguous: " +
                    $source.Owner)
            }
        }
        $end = if (($index + 1) -lt $sources.Count) {
            [int]$headers[$index].Start
        }
        else {
            $Bytes.Length
        }
        if (($start -lt 0) -or ($end -le $start) -or
            ($source.MarkerStart -lt $start) -or
            (($source.PathStart + $source.SourcePath.Length) -gt $end)) {
            Throw-ComparatorBlocker (
                "$ArtifactOwner true record does not bound its source: " +
                $source.Owner)
        }
        $diagnosticEnd = if (($index + 1) -lt $sources.Count) {
            [int]$sources[$index + 1].PathStart
        }
        else {
            $Bytes.Length
        }
        $records.Add([pscustomobject]@{
                Owner = $source.Owner
                SourcePath = $source.SourcePath
                Start = [int]$start
                End = [int]$end
                SourceOffset = [int]($source.PathStart - $start)
                SourcePathStart = [int]$source.PathStart
                SourceMarkerStart = [int]$source.MarkerStart
                DiagnosticSourceSegmentStart = [int]$source.PathStart
                DiagnosticSourceSegmentEnd = [int]$diagnosticEnd
                Parser = $parser
            })
    }
    return [pscustomobject]@{
        Text = $text
        Records = $records.ToArray()
    }
}

function Get-OwnerRecord {
    param(
        [Parameter(Mandatory = $true)][object[]]$Inventory,
        [Parameter(Mandatory = $true)][string]$Owner,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )
    $matches = @($Inventory | Where-Object {
            ([string]$_.Owner -ceq $Owner) -and
            ([string]$_.SourcePath -ceq $SourcePath)
        })
    if ($matches.Count -ne 1) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner target class owner record is not exact: $Owner")
    }
    return $matches[0]
}

function Get-ProtectedBoundedRecord {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][Collections.IDictionary]$Definition,
        [Parameter(Mandatory = $true)][string]$ArtifactOwner
    )
    if ($Bytes.Length -ne $Text.Length) {
        Throw-ComparatorBlocker "$ArtifactOwner protected byte/text offsets diverged."
    }
    if ((Get-OrdinalCount `
            -Text $Text -Needle $Definition.sourcePath -IgnoreCase) -ne 1) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner protected source path count is not one: " +
            $Definition.owner)
    }
    $sourceStart = $Text.IndexOf(
        $Definition.sourcePath,
        [StringComparison]::OrdinalIgnoreCase)
    $startWindow = [Math]::Max(0, $sourceStart - 500)
    $recordStart = $Text.IndexOf(
        $Definition.className,
        $startWindow,
        [StringComparison]::Ordinal)
    if (($recordStart -lt $startWindow) -or ($recordStart -ge $sourceStart)) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner protected start boundary is missing: " +
            $Definition.owner)
    }
    $nextSourceStart = $Text.IndexOf(
        $Definition.nextSourcePath,
        $sourceStart + $Definition.sourcePath.Length,
        [StringComparison]::OrdinalIgnoreCase)
    if ($nextSourceStart -lt 0) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner protected next-source boundary is missing: " +
            $Definition.owner)
    }
    $endWindow = [Math]::Max(
        $sourceStart + $Definition.sourcePath.Length,
        $nextSourceStart - 500)
    $recordEnd = $Text.IndexOf(
        $Definition.nextClassName,
        $endWindow,
        [StringComparison]::Ordinal)
    if (($recordEnd -lt $endWindow) -or ($recordEnd -ge $nextSourceStart)) {
        Throw-ComparatorBlocker (
            "$ArtifactOwner protected end boundary is missing: " +
            $Definition.owner)
    }
    return [pscustomobject]@{
        Owner = $Definition.owner
        SourcePath = $Definition.sourcePath
        Start = [int]$recordStart
        End = [int]$recordEnd
        SourceOffset = [int]($sourceStart - $recordStart)
        Parser = 'bounded-class-name-and-next-source'
    }
}

function Get-RecordIdentity {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][pscustomobject]$Record
    )
    $length = [int]($Record.End - $Record.Start)
    return [ordered]@{
        startOffset = [int]$Record.Start
        endOffsetExclusive = [int]$Record.End
        sourceOffset = [int]$Record.SourceOffset
        bytes = $length
        sha256 = Get-ByteRangeSha256 `
            -Bytes $Bytes -Start $Record.Start -Length $length
    }
}

function New-RecordEqualityReport {
    param(
        [Parameter(Mandatory = $true)][string]$Owner,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$Parser,
        [Parameter(Mandatory = $true)][byte[]]$CheckpointBytes,
        [Parameter(Mandatory = $true)][pscustomobject]$CheckpointRecord,
        [Parameter(Mandatory = $true)][byte[]]$CandidateBytes,
        [Parameter(Mandatory = $true)][pscustomobject]$CandidateRecord
    )
    $checkpointIdentity = Get-RecordIdentity `
        -Bytes $CheckpointBytes -Record $CheckpointRecord
    $candidateIdentity = Get-RecordIdentity `
        -Bytes $CandidateBytes -Record $CandidateRecord
    $exact = Test-ByteRangesEqual `
        -Left $CheckpointBytes `
        -LeftStart $CheckpointRecord.Start `
        -LeftLength ($CheckpointRecord.End - $CheckpointRecord.Start) `
        -Right $CandidateBytes `
        -RightStart $CandidateRecord.Start `
        -RightLength ($CandidateRecord.End - $CandidateRecord.Start)
    return [ordered]@{
        owner = $Owner
        sourcePath = $SourcePath
        parser = $Parser
        exact = [bool]$exact
        checkpoint = $checkpointIdentity
        candidate = $candidateIdentity
    }
}

function Initialize-ByteDiffType {
    if ($null -ne ('CodexLasalClassesComparatorByteDiffV1' -as [type])) {
        return
    }
    Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;

public sealed class CodexLasalClassesComparatorByteDiffResultV1
{
    public long ChangedByteCount;
    public int[] Starts;
    public int[] Lengths;
}

public sealed class CodexLasalClassesComparatorInventoryV1
{
    public int[] SourcePathStarts;
    public int[] SourceMarkerStarts;
    public string[] SourceOwners;
    public string[] SourcePaths;
    public int[] HeaderStarts;
    public string[] HeaderOwners;
}

public static class CodexLasalClassesComparatorByteDiffV1
{
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
        byte[] bytes,
        int offset,
        string value)
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
        return System.Text.Encoding.ASCII.GetString(bytes, start, length);
    }

    public static CodexLasalClassesComparatorInventoryV1 ParseInventory(
        byte[] bytes)
    {
        if (bytes == null) throw new ArgumentNullException("bytes");
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
                        headerOwners.Add(GetAscii(bytes, nameStart, nameLength));
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
                    "source marker LE24/AA boundary differs at byte " + offset);
            sourcePathStarts.Add(offset);
            sourceMarkerStarts.Add(offset - 4);
            sourceOwners.Add(owner);
            sourcePaths.Add(GetAscii(bytes, offset, pathLength));
            offset = pathEnd - 1;
        }
        return new CodexLasalClassesComparatorInventoryV1
        {
            SourcePathStarts = sourcePathStarts.ToArray(),
            SourceMarkerStarts = sourceMarkerStarts.ToArray(),
            SourceOwners = sourceOwners.ToArray(),
            SourcePaths = sourcePaths.ToArray(),
            HeaderStarts = headerStarts.ToArray(),
            HeaderOwners = headerOwners.ToArray()
        };
    }

    public static bool RangeEquals(
        byte[] left,
        int leftStart,
        int leftLength,
        byte[] right,
        int rightStart,
        int rightLength)
    {
        if (left == null || right == null ||
            leftStart < 0 || rightStart < 0 ||
            leftLength < 0 || rightLength < 0 ||
            leftStart + leftLength > left.Length ||
            rightStart + rightLength > right.Length)
            throw new ArgumentOutOfRangeException("byte range");
        if (leftLength != rightLength) return false;
        for (int index = 0; index < leftLength; index++)
            if (left[leftStart + index] != right[rightStart + index])
                return false;
        return true;
    }

    public static CodexLasalClassesComparatorByteDiffResultV1 Compare(
        byte[] left,
        byte[] right)
    {
        if (left == null || right == null || left.Length != right.Length)
            throw new ArgumentException("equal-length byte arrays are required");
        List<int> starts = new List<int>();
        List<int> lengths = new List<int>();
        long changed = 0;
        int runStart = -1;
        for (int index = 0; index < left.Length; index++)
        {
            if (left[index] != right[index])
            {
                changed++;
                if (runStart < 0) runStart = index;
            }
            else if (runStart >= 0)
            {
                starts.Add(runStart);
                lengths.Add(index - runStart);
                runStart = -1;
            }
        }
        if (runStart >= 0)
        {
            starts.Add(runStart);
            lengths.Add(left.Length - runStart);
        }
        return new CodexLasalClassesComparatorByteDiffResultV1
        {
            ChangedByteCount = changed,
            Starts = starts.ToArray(),
            Lengths = lengths.ToArray()
        };
    }
}
'@
}

function Get-InventoryEqualityReport {
    param(
        [Parameter(Mandatory = $true)][object[]]$CheckpointInventory,
        [Parameter(Mandatory = $true)][object[]]$CandidateInventory
    )
    $firstMismatch = $null
    if ($CheckpointInventory.Count -ne $CandidateInventory.Count) {
        $firstMismatch = 'count'
    }
    else {
        for ($index = 0; $index -lt $CheckpointInventory.Count; $index++) {
            $left = $CheckpointInventory[$index]
            $right = $CandidateInventory[$index]
            if (([string]$left.Owner -cne [string]$right.Owner) -or
                ([string]$left.SourcePath -cne [string]$right.SourcePath) -or
                ([int]$left.Start -ne [int]$right.Start) -or
                ([int]$left.End -ne [int]$right.End) -or
                ([int]$left.SourcePathStart -ne
                    [int]$right.SourcePathStart) -or
                ([int]$left.SourceMarkerStart -ne
                    [int]$right.SourceMarkerStart) -or
                ([string]$left.Parser -cne [string]$right.Parser)) {
                $firstMismatch = "ordinal-$($index + 1)"
                break
            }
        }
    }
    return [ordered]@{
        exact = [bool]($null -eq $firstMismatch)
        checkpointCount = $CheckpointInventory.Count
        candidateCount = $CandidateInventory.Count
        firstMismatch = $firstMismatch
        comparedFields = @(
            'owner',
            'sourcePath',
            'headerOffset',
            'recordEndOffset',
            'sourcePathOffset',
            'sourceMarkerOffset',
            'parser')
    }
}

function Get-DiffRuns {
    param(
        [Parameter(Mandatory = $true)][byte[]]$CheckpointBytes,
        [Parameter(Mandatory = $true)][byte[]]$CandidateBytes
    )
    $runs = New-Object Collections.Generic.List[object]
    if ($CheckpointBytes.Length -eq $CandidateBytes.Length) {
        Initialize-ByteDiffType
        $compiled = [CodexLasalClassesComparatorByteDiffV1]::Compare(
            $CheckpointBytes,
            $CandidateBytes)
        for ($index = 0; $index -lt $compiled.Starts.Length; $index++) {
            $runStart = [int]$compiled.Starts[$index]
            $runLength = [int]$compiled.Lengths[$index]
            $runs.Add([pscustomobject]@{
                    CheckpointStart = [int]$runStart
                    CheckpointLength = $runLength
                    CandidateStart = [int]$runStart
                    CandidateLength = $runLength
                })
        }
        return [pscustomobject]@{
            Alignment = 'equal-length-indexed'
            ChangedByteCount = [long]$compiled.ChangedByteCount
            ChangedByteCountDefined = $true
            Runs = $runs.ToArray()
        }
    }

    $minimumLength = [Math]::Min(
        $CheckpointBytes.Length,
        $CandidateBytes.Length)
    $prefix = 0
    while (($prefix -lt $minimumLength) -and
        ($CheckpointBytes[$prefix] -eq $CandidateBytes[$prefix])) {
        $prefix++
    }
    $suffix = 0
    while (($suffix -lt ($minimumLength - $prefix)) -and
        ($CheckpointBytes[$CheckpointBytes.Length - 1 - $suffix] -eq
            $CandidateBytes[$CandidateBytes.Length - 1 - $suffix])) {
        $suffix++
    }
    $runs.Add([pscustomobject]@{
            CheckpointStart = [int]$prefix
            CheckpointLength =
                [int]($CheckpointBytes.Length - $prefix - $suffix)
            CandidateStart = [int]$prefix
            CandidateLength =
                [int]($CandidateBytes.Length - $prefix - $suffix)
        })
    return [pscustomobject]@{
        Alignment = 'bounded-common-prefix-suffix'
        ChangedByteCount = $null
        ChangedByteCountDefined = $false
        Runs = $runs.ToArray()
    }
}

function Get-HexPreview {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$Length
    )
    $previewLength = [Math]::Min(32, $Length)
    if ($previewLength -eq 0) {
        return [ordered]@{
            hex = ''
            previewBytes = 0
            truncated = $false
        }
    }
    $preview = New-Object byte[] $previewLength
    [Array]::Copy($Bytes, $Start, $preview, 0, $previewLength)
    return [ordered]@{
        hex = ([BitConverter]::ToString($preview)).Replace('-', '')
        previewBytes = $previewLength
        truncated = [bool]($Length -gt $previewLength)
    }
}

function Get-RunOwnerMappings {
    param(
        [Parameter(Mandatory = $true)][object[]]$Inventory,
        [Parameter(Mandatory = $true)][int]$Start,
        [Parameter(Mandatory = $true)][int]$Length,
        [Parameter(Mandatory = $true)][int]$ArtifactLength
    )
    $end = $Start + $Length
    $mappings = New-Object Collections.Generic.List[object]
    foreach ($record in $Inventory) {
        $overlapStart = [Math]::Max($Start, [int]$record.Start)
        $overlapEnd = [Math]::Min($end, [int]$record.End)
        $overlapLength = $overlapEnd - $overlapStart
        if (($Length -eq 0) -and ($Start -eq $ArtifactLength) -and
            ($record.End -eq $ArtifactLength)) {
            $overlapStart = $Start
            $overlapLength = 0
        }
        elseif (($Length -eq 0) -and
            ($Start -ge $record.Start) -and ($Start -lt $record.End)) {
            $overlapStart = $Start
            $overlapLength = 0
        }
        elseif ($overlapLength -le 0) {
            continue
        }
        $mappings.Add([ordered]@{
                owner = $record.Owner
                sourcePath = $record.SourcePath
                recordStart = [int]$record.Start
                recordEndExclusive = [int]$record.End
                overlapStart = [int]$overlapStart
                overlapBytes = [int]$overlapLength
            })
    }
    return $mappings.ToArray()
}

function Get-ChangedOwnerSummary {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$RunReports
    )
    $order = New-Object Collections.Generic.List[string]
    $byOwner = @{}
    foreach ($run in $RunReports) {
        foreach ($mapping in @($run.checkpointOwners)) {
            $key = ([string]$mapping.owner).ToUpperInvariant()
            if (-not $byOwner.ContainsKey($key)) {
                $order.Add($key)
                $byOwner[$key] = [ordered]@{
                    owner = $mapping.owner
                    sourcePath = $mapping.sourcePath
                    diffRunCount = 0
                    changedCheckpointBytes = 0
                    classification = 'unclassified-owner-record'
                }
            }
            $byOwner[$key].diffRunCount++
            $byOwner[$key].changedCheckpointBytes +=
                [int]$mapping.overlapBytes
        }
    }
    $result = New-Object Collections.Generic.List[object]
    foreach ($key in $order) {
        $result.Add($byOwner[$key])
    }
    return $result.ToArray()
}

function New-ArtifactComparisonReport {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$CheckpointDescriptor,
        [Parameter(Mandatory = $true)][byte[]]$CheckpointBytes,
        [Parameter(Mandatory = $true)][string]$CandidateDisplayPath,
        [Parameter(Mandatory = $true)][byte[]]$CandidateBytes
    )
    $checkpointInventory = Get-ClassOwnerInventory `
        -Bytes $CheckpointBytes -ArtifactOwner 'checkpoint Classes.lcb'
    $candidateInventory = Get-ClassOwnerInventory `
        -Bytes $CandidateBytes -ArtifactOwner 'candidate Classes.lcb'
    $inventoryEquality = Get-InventoryEqualityReport `
        -CheckpointInventory $checkpointInventory.Records `
        -CandidateInventory $candidateInventory.Records

    $targetReports = New-Object Collections.Generic.List[object]
    foreach ($definition in $GateDTargetDefinitions) {
        $checkpointRecord = Get-OwnerRecord `
            -Inventory $checkpointInventory.Records `
            -Owner $definition.owner `
            -SourcePath $definition.sourcePath `
            -ArtifactOwner 'checkpoint Classes.lcb'
        $candidateRecord = Get-OwnerRecord `
            -Inventory $candidateInventory.Records `
            -Owner $definition.owner `
            -SourcePath $definition.sourcePath `
            -ArtifactOwner 'candidate Classes.lcb'
        $targetReports.Add((New-RecordEqualityReport `
                -Owner $definition.owner `
                -SourcePath $definition.sourcePath `
                -Parser $checkpointRecord.Parser `
                -CheckpointBytes $CheckpointBytes `
                -CheckpointRecord $checkpointRecord `
                -CandidateBytes $CandidateBytes `
                -CandidateRecord $candidateRecord))
    }

    $protectedReports = New-Object Collections.Generic.List[object]
    foreach ($definition in $ProtectedRecordDefinitions) {
        $checkpointRecord = Get-OwnerRecord `
            -Inventory $checkpointInventory.Records `
            -Owner $definition.owner `
            -SourcePath $definition.sourcePath `
            -ArtifactOwner 'checkpoint Classes.lcb'
        $candidateRecord = Get-OwnerRecord `
            -Inventory $candidateInventory.Records `
            -Owner $definition.owner `
            -SourcePath $definition.sourcePath `
            -ArtifactOwner 'candidate Classes.lcb'
        $checkpointLegacy = Get-ProtectedBoundedRecord `
            -Bytes $CheckpointBytes `
            -Text $checkpointInventory.Text `
            -Definition $definition `
            -ArtifactOwner 'checkpoint Classes.lcb'
        $candidateLegacy = Get-ProtectedBoundedRecord `
            -Bytes $CandidateBytes `
            -Text $candidateInventory.Text `
            -Definition $definition `
            -ArtifactOwner 'candidate Classes.lcb'
        $legacyExact = Test-ByteRangesEqual `
            -Left $CheckpointBytes `
            -LeftStart $checkpointLegacy.Start `
            -LeftLength ($checkpointLegacy.End - $checkpointLegacy.Start) `
            -Right $CandidateBytes `
            -RightStart $candidateLegacy.Start `
            -RightLength ($candidateLegacy.End - $candidateLegacy.Start)
        $protectedReport = New-RecordEqualityReport `
                -Owner $definition.owner `
                -SourcePath $definition.sourcePath `
                -Parser (
                    'aa03-header-to-next-header+' +
                    'legacy-window-cross-check') `
                -CheckpointBytes $CheckpointBytes `
                -CheckpointRecord $checkpointRecord `
                -CandidateBytes $CandidateBytes `
                -CandidateRecord $candidateRecord
        $protectedReport['legacyWindowExact'] = [bool]$legacyExact
        $protectedReports.Add($protectedReport)
    }

    $diff = Get-DiffRuns `
        -CheckpointBytes $CheckpointBytes -CandidateBytes $CandidateBytes
    $runReports = New-Object Collections.Generic.List[object]
    $ordinal = 0
    foreach ($run in @($diff.Runs)) {
        $ordinal++
        $checkpointOwners = @(Get-RunOwnerMappings `
                -Inventory $checkpointInventory.Records `
                -Start $run.CheckpointStart `
                -Length $run.CheckpointLength `
                -ArtifactLength $CheckpointBytes.Length)
        $candidateOwners = @(Get-RunOwnerMappings `
                -Inventory $candidateInventory.Records `
                -Start $run.CandidateStart `
                -Length $run.CandidateLength `
                -ArtifactLength $CandidateBytes.Length)
        $runReports.Add([ordered]@{
                ordinal = $ordinal
                checkpointStart = [int]$run.CheckpointStart
                checkpointBytes = [int]$run.CheckpointLength
                candidateStart = [int]$run.CandidateStart
                candidateBytes = [int]$run.CandidateLength
                checkpointPreview = Get-HexPreview `
                    -Bytes $CheckpointBytes `
                    -Start $run.CheckpointStart `
                    -Length $run.CheckpointLength
                candidatePreview = Get-HexPreview `
                    -Bytes $CandidateBytes `
                    -Start $run.CandidateStart `
                    -Length $run.CandidateLength
                checkpointOwners = $checkpointOwners
                candidateOwners = $candidateOwners
                mappingComplete = [bool](
                    ($checkpointOwners.Count -gt 0) -and
                    ($candidateOwners.Count -gt 0))
            })
    }
    $runReportArray = $runReports.ToArray()
    $changedOwners = @(Get-ChangedOwnerSummary -RunReports $runReportArray)
    $frozenOpaqueSet = @{}
    foreach ($owner in $FrozenOpaqueVendorOwners) {
        $frozenOpaqueSet[$owner.ToUpperInvariant()] = $true
    }
    $changedOwnersAreFrozenOpaqueSubset = $true
    foreach ($changedOwner in $changedOwners) {
        $ownerKey = ([string]$changedOwner.owner).ToUpperInvariant()
        if ($frozenOpaqueSet.ContainsKey($ownerKey)) {
            $changedOwner.classification = 'frozen-opaque-vendor-owner-record'
        }
        else {
            $changedOwnersAreFrozenOpaqueSubset = $false
            $changedOwner.classification = 'contract-or-unclassified-owner-record'
        }
    }
    $byteExact =
        ($CheckpointBytes.Length -eq $CandidateBytes.Length) -and
        ($runReportArray.Count -eq 0)
    $targetArray = $targetReports.ToArray()
    $protectedArray = $protectedReports.ToArray()
    $targetsAllEqual =
        @($targetArray | Where-Object { -not [bool]$_.exact }).Count -eq 0
    $protectedAllEqual =
        @($protectedArray | Where-Object {
                (-not [bool]$_.exact) -or
                (-not [bool]$_.legacyWindowExact)
            }).Count -eq 0
    $unmappedRunCount = @($runReportArray | Where-Object {
            -not [bool]$_.mappingComplete
        }).Count
    $checkpointSha256 = Get-BytesSha256 -Bytes $CheckpointBytes
    $candidateSha256 = Get-BytesSha256 -Bytes $CandidateBytes
    $firstCheckpointRecord = $checkpointInventory.Records[0]
    $firstCandidateRecord = $candidateInventory.Records[0]
    $firstSpecialRecord = New-RecordEqualityReport `
        -Owner $firstCheckpointRecord.Owner `
        -SourcePath $firstCheckpointRecord.SourcePath `
        -Parser 'first-special-preamble-to-next-header' `
        -CheckpointBytes $CheckpointBytes `
        -CheckpointRecord $firstCheckpointRecord `
        -CandidateBytes $CandidateBytes `
        -CandidateRecord $firstCandidateRecord
    $reviewRequiredOpaqueVendorDrift =
        (-not $byteExact) -and
        ($CheckpointBytes.Length -eq $CandidateBytes.Length) -and
        [bool]$inventoryEquality.exact -and
        $targetsAllEqual -and
        $protectedAllEqual -and
        ($unmappedRunCount -eq 0) -and
        ($changedOwners.Count -gt 0) -and
        $changedOwnersAreFrozenOpaqueSubset
    $disposition = if ($byteExact) {
        'EXACT_CHECKPOINT_MATCH'
    }
    elseif ($reviewRequiredOpaqueVendorDrift) {
        'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
    }
    else {
        'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
    }
    $decisionExitCode = if ($byteExact) {
        0
    }
    elseif ($reviewRequiredOpaqueVendorDrift) {
        2
    }
    else {
        3
    }

    return [ordered]@{
        schema = $ScriptSchema
        decision = [ordered]@{
            disposition = $disposition
            checkpointIdentityAccepted = [bool]$byteExact
            approvalScope = 'checkpoint-byte-identity-only'
            productionApproved = $false
            exactCheckpointMatch = [bool]$byteExact
            semanticEquivalenceProven = [bool]$byteExact
            recordEqualityCannotApproveArtifact = $true
            exitCode = $decisionExitCode
        }
        checkpoint = [ordered]@{
            requested = $CheckpointDescriptor.requested
            kind = $CheckpointDescriptor.kind
            resolvedRevision = $CheckpointDescriptor.resolvedRevision
            relativePath = $CheckpointDescriptor.relativePath
            blobOid = $CheckpointDescriptor.blobOid
            rawBytes = $CheckpointBytes.Length
            sha256 = $checkpointSha256
        }
        candidate = [ordered]@{
            path = $CandidateDisplayPath
            rawBytes = $CandidateBytes.Length
            sha256 = $candidateSha256
        }
        comparison = [ordered]@{
            byteExact = [bool]$byteExact
            equalLength = [bool](
                $CheckpointBytes.Length -eq $CandidateBytes.Length)
            lengthDelta = [long](
                $CandidateBytes.Length - $CheckpointBytes.Length)
            alignment = $diff.Alignment
            changedByteCountDefined = [bool]$diff.ChangedByteCountDefined
            changedByteCount = $diff.ChangedByteCount
            contiguousRunCount = $runReportArray.Count
            checkpointChangedOwnerCount = $changedOwners.Count
            unmappedRunCount = $unmappedRunCount
            changedOwnersAreFrozenOpaqueSubset =
                [bool]$changedOwnersAreFrozenOpaqueSubset
            frozenOpaqueOwnerCount = $FrozenOpaqueVendorOwners.Count
            frozenOpaqueOwners = $FrozenOpaqueVendorOwners
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
            checkpointOwnerRecordCount =
                $checkpointInventory.Records.Count
            candidateOwnerRecordCount = $candidateInventory.Records.Count
            headerSourceInventory = $inventoryEquality
            firstSpecialRecord = $firstSpecialRecord
        }
        gateDTargetRecords = [ordered]@{
            allEqual = [bool]$targetsAllEqual
            records = $targetArray
        }
        protectedDependencyRecords = [ordered]@{
            allEqual = [bool]$protectedAllEqual
            records = $protectedArray
        }
        changedCheckpointOwners = $changedOwners
        diffRuns = $runReportArray
    }
}

function ConvertTo-DeterministicJson {
    param([Parameter(Mandatory = $true)]$Value)
    # Ordered dictionaries and ordered arrays plus compressed JSON avoid
    # PowerShell 5/7 indentation differences.
    $json = ($Value | ConvertTo-Json -Depth 18 -Compress)
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

function Get-JsonFileBytes {
    param([Parameter(Mandatory = $true)][string]$Json)
    if ([regex]::IsMatch($Json, '[^\x00-\x7F]')) {
        Throw-ComparatorBlocker 'deterministic JSON is not 7-bit ASCII.'
    }
    return ,$Utf8NoBom.GetBytes($Json + "`n")
}

function Write-JsonStdout {
    param([Parameter(Mandatory = $true)][string]$Json)
    [byte[]]$bytes = Get-JsonFileBytes -Json $Json
    $stdout = [Console]::OpenStandardOutput()
    $stdout.Write($bytes, 0, $bytes.Length)
    $stdout.Flush()
}

function Write-CreateNewJson {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$AllowedRoot,
        [Parameter(Mandatory = $true)][string]$RequestedPath,
        [Parameter(Mandatory = $true)][string]$Json
    )
    $combined = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        $RequestedPath
    }
    else {
        Join-Path $AllowedRoot $RequestedPath
    }
    $fullPath = [IO.Path]::GetFullPath($combined)
    $parent = [IO.Path]::GetDirectoryName($fullPath)
    if ([string]::IsNullOrWhiteSpace($parent) -or
        (-not [IO.Directory]::Exists($parent))) {
        Throw-ComparatorBlocker 'OutputPath parent directory does not exist.'
    }
    $resolvedAllowedRoot = (Resolve-Path -LiteralPath $AllowedRoot).Path.TrimEnd('\')
    $resolvedParent = (Resolve-Path -LiteralPath $parent).Path.TrimEnd('\')
    if ((-not [string]::Equals(
            $resolvedParent,
            $resolvedAllowedRoot,
            [StringComparison]::OrdinalIgnoreCase)) -and
        (-not $resolvedParent.StartsWith(
                $resolvedAllowedRoot + '\',
                [StringComparison]::OrdinalIgnoreCase))) {
        Throw-ComparatorBlocker 'OutputPath escapes its allowed evidence root.'
    }
    $cursor = [IO.DirectoryInfo](Get-Item -LiteralPath $resolvedParent -Force)
    while (($null -ne $cursor) -and
        $cursor.FullName.StartsWith(
            $resolvedAllowedRoot,
            [StringComparison]::OrdinalIgnoreCase)) {
        if (($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-ComparatorBlocker 'OutputPath parent must not use a reparse point.'
        }
        if ($cursor.FullName -ceq $resolvedAllowedRoot) {
            break
        }
        $cursor = $cursor.Parent
    }
    [byte[]]$bytes = Get-JsonFileBytes -Json $Json
    $stream = $null
    $created = $false
    $completed = $false
    try {
        $stream = New-Object IO.FileStream(
            $fullPath,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        $created = $true
        $stream.Write($bytes, 0, $bytes.Length)
        $stream.Flush($true)
        $stream.Dispose()
        $stream = $null
        $readBack = [IO.File]::ReadAllBytes($fullPath)
        if (($readBack.Length -ne $bytes.Length) -or
            ((Get-BytesSha256 -Bytes $readBack) -cne
                (Get-BytesSha256 -Bytes $bytes))) {
            Throw-ComparatorBlocker 'CreateNew output read-back differs.'
        }
        $completed = $true
        return $fullPath
    }
    catch [IO.IOException] {
        Throw-ComparatorBlocker (
            'OutputPath already exists or could not be created with CreateNew.')
    }
    finally {
        if ($null -ne $stream) {
            $stream.Dispose()
        }
        if ($created -and (-not $completed) -and
            [IO.File]::Exists($fullPath)) {
            [IO.File]::Delete($fullPath)
        }
    }
}

function Assert-InvocationContract {
    param([Collections.IDictionary]$Bound)
    if ($EmitJsonSelfTestFixtureBase64) {
        foreach ($name in $Bound.Keys) {
            if ($name -cne 'EmitJsonSelfTestFixtureBase64') {
                Throw-ComparatorBlocker (
                    '-EmitJsonSelfTestFixtureBase64 must be used alone.')
            }
        }
        return
    }
    if ($RunSelfTest) {
        foreach ($name in @(
                'Checkpoint',
                'CheckpointPath',
                'CandidatePath',
                'OutputPath',
                'CreateNew',
                'EmitJsonSelfTestFixtureBase64')) {
            if ($Bound.ContainsKey($name)) {
                Throw-ComparatorBlocker (
                    "-RunSelfTest cannot be combined with -$name.")
            }
        }
    }
    if ($CreateNew -and [string]::IsNullOrWhiteSpace($OutputPath)) {
        Throw-ComparatorBlocker '-CreateNew requires -OutputPath.'
    }
    if ((-not $CreateNew) -and $Bound.ContainsKey('OutputPath')) {
        Throw-ComparatorBlocker '-OutputPath requires -CreateNew.'
    }
}

function Get-JsonSelfTestFixture {
    $nonAscii = [string]([char]0xD55C) + [string]([char]0xAE00)
    return [ordered]@{
        schema = 'JsonDeterminismSelfTest/v1'
        singleton = @(
            [ordered]@{
                ordinal = 1
                text = $nonAscii
                symbols = '<>&'
            })
        empty = @()
        nullValue = $null
        boolean = $true
    }
}

function Invoke-JsonFixtureHost {
    param(
        [Parameter(Mandatory = $true)][string]$HostPath,
        [Parameter(Mandatory = $true)][string]$ScriptPath
    )
    if ($ScriptPath.IndexOf('"') -ge 0) {
        throw 'SELFTEST: script path contains an unsupported quote.'
    }
    $startInfo = New-Object Diagnostics.ProcessStartInfo
    $startInfo.FileName = $HostPath
    $startInfo.Arguments =
        '-NoLogo -NoProfile -ExecutionPolicy Bypass -File "' +
        $ScriptPath + '" -EmitJsonSelfTestFixtureBase64'
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $process = New-Object Diagnostics.Process
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw 'SELFTEST: JSON fixture host did not start.'
        }
        $stdout = $process.StandardOutput.ReadToEnd().Trim()
        $stderr = $process.StandardError.ReadToEnd().Trim()
        $process.WaitForExit()
        if (($process.ExitCode -ne 0) -or ($stderr.Length -ne 0) -or
            ($stdout -cnotmatch '^[A-Za-z0-9+/]+={0,2}$')) {
            throw (
                'SELFTEST: JSON fixture host failed: exit=' +
                $process.ExitCode + ' stderr=' + $stderr)
        }
        return $stdout
    }
    finally {
        $process.Dispose()
    }
}

function Copy-Bytes {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $copy = New-Object byte[] $Bytes.Length
    [Array]::Copy($Bytes, 0, $copy, 0, $Bytes.Length)
    return ,$copy
}

function New-SyntheticClassesArtifact {
    $builder = New-Object Text.StringBuilder
    [void]$builder.Append($ClassesSignature)
    [void]$builder.Append('SYNTHETIC-HEADER|')
    $owners = @(
            '_AxisBase',
            '_StdLib',
            '_SyncMeasure',
            '_UDPTransceiver',
            '_UDPTransceiverInterface',
            'CriticalSection',
            'DiasMaster',
            'LMCDiagnosticsService',
            'LMCUdpCallbackSender',
            'TCPMotionInterface',
            'PosController')
    for ($index = 0; $index -lt $owners.Count; $index++) {
        $owner = $owners[$index]
        if ($index -gt 0) {
            [void]$builder.Append([char]0xAA)
            [void]$builder.Append([char]0x03)
            [void]$builder.Append([char]($owner.Length -band 0xFF))
            [void]$builder.Append([char](($owner.Length -shr 8) -band 0xFF))
            [void]$builder.Append([char](($owner.Length -shr 16) -band 0xFF))
            [void]$builder.Append([char]0xAA)
            [void]$builder.Append($owner)
        }
        [void]$builder.Append('|META|')
        $sourcePath = ".\Class\$owner\$owner.st"
        [void]$builder.Append([char]($sourcePath.Length -band 0xFF))
        [void]$builder.Append([char](($sourcePath.Length -shr 8) -band 0xFF))
        [void]$builder.Append([char](($sourcePath.Length -shr 16) -band 0xFF))
        [void]$builder.Append([char]0xAA)
        [void]$builder.Append($sourcePath)
        [void]$builder.Append("|PAYLOAD-$owner-0123456789|")
    }
    return ,$Latin1.GetBytes($builder.ToString())
}

function Assert-SelfTestTrue {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if (-not $Condition) {
        throw "SELFTEST: $Message"
    }
}

function Assert-SelfTestThrows {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string]$Message
    )
    $threw = $false
    try {
        & $Action
    }
    catch {
        $threw = $true
    }
    if (-not $threw) {
        throw "SELFTEST: $Message"
    }
}

function Invoke-SelfTest {
    param([Parameter(Mandatory = $true)][string]$Root)
    $positive = 0
    $negative = 0
    $descriptor = [ordered]@{
        requested = 'synthetic'
        kind = 'blob'
        resolvedRevision = $null
        relativePath = $CanonicalClassesPath
        blobOid = ('0' * 40)
    }
    [byte[]]$baseline = New-SyntheticClassesArtifact
    [byte[]]$exactCandidate = Copy-Bytes -Bytes $baseline
    $exact = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-exact.lcb' `
        -CandidateBytes $exactCandidate
    Assert-SelfTestTrue `
        -Condition (
            $exact.decision.checkpointIdentityAccepted -and
            $exact.comparison.byteExact -and
            $exact.gateDTargetRecords.allEqual -and
            $exact.protectedDependencyRecords.allEqual -and
            ($exact.decision.exitCode -eq 0)) `
        -Message 'exact artifact was not accepted.'
    $positive++

    $baselineInventory = Get-ClassOwnerInventory `
        -Bytes $baseline -ArtifactOwner 'synthetic baseline'
    $opaqueRecord = Get-OwnerRecord `
        -Inventory $baselineInventory.Records `
        -Owner 'PosController' `
        -SourcePath '.\Class\PosController\PosController.st' `
        -ArtifactOwner 'synthetic baseline'
    [byte[]]$opaqueMutation = Copy-Bytes -Bytes $baseline
    $opaqueOffset = $opaqueRecord.SourcePathStart +
        '.\Class\PosController\PosController.st'.Length + 2
    $opaqueMutation[$opaqueOffset] = $opaqueMutation[$opaqueOffset] -bxor 1
    $opaque = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-opaque-mutation.lcb' `
        -CandidateBytes $opaqueMutation
    Assert-SelfTestTrue `
        -Condition (
            (-not $opaque.decision.checkpointIdentityAccepted) -and
            ($opaque.decision.exitCode -eq 2) -and
            ($opaque.comparison.changedByteCount -eq 1) -and
            ($opaque.comparison.contiguousRunCount -eq 1) -and
            ($opaque.changedCheckpointOwners.Count -eq 1) -and
            ($opaque.changedCheckpointOwners[0].owner -ceq 'PosController') -and
            $opaque.gateDTargetRecords.allEqual -and
            $opaque.protectedDependencyRecords.allEqual) `
        -Message 'opaque non-target mutation was not fail-closed or mapped.'
    $negative++

    $targetRecord = Get-OwnerRecord `
        -Inventory $baselineInventory.Records `
        -Owner 'LMCUdpCallbackSender' `
        -SourcePath '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st' `
        -ArtifactOwner 'synthetic baseline'
    [byte[]]$targetMutation = Copy-Bytes -Bytes $baseline
    $targetOffset = $targetRecord.SourcePathStart +
        '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'.Length + 2
    $targetMutation[$targetOffset] = $targetMutation[$targetOffset] -bxor 1
    $target = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-target-mutation.lcb' `
        -CandidateBytes $targetMutation
    Assert-SelfTestTrue `
        -Condition (
            (-not $target.decision.checkpointIdentityAccepted) -and
            (-not $target.gateDTargetRecords.allEqual)) `
        -Message 'Gate D target mutation was accepted or hidden.'
    $negative++

    $protected = Get-ProtectedBoundedRecord `
        -Bytes $baseline `
        -Text $baselineInventory.Text `
        -Definition $ProtectedRecordDefinitions[0] `
        -ArtifactOwner 'synthetic baseline'
    [byte[]]$protectedMutation = Copy-Bytes -Bytes $baseline
    $protectedTrueRecord = Get-OwnerRecord `
        -Inventory $baselineInventory.Records `
        -Owner '_StdLib' `
        -SourcePath '.\Class\_StdLib\_StdLib.st' `
        -ArtifactOwner 'synthetic baseline'
    $protectedOffset = $protectedTrueRecord.SourceMarkerStart - 1
    $protectedMutation[$protectedOffset] =
        $protectedMutation[$protectedOffset] -bxor 1
    $protectedReport = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-protected-mutation.lcb' `
        -CandidateBytes $protectedMutation
    Assert-SelfTestTrue `
        -Condition (
            (-not $protectedReport.decision.checkpointIdentityAccepted) -and
            (-not $protectedReport.protectedDependencyRecords.allEqual)) `
        -Message 'protected pre-source mutation was accepted or hidden.'
    $negative++

    $insertAt = $opaqueRecord.SourcePathStart +
        $opaqueRecord.SourcePath.Length + 2
    $inserted = New-Object byte[] ($baseline.Length + 1)
    [Array]::Copy($baseline, 0, $inserted, 0, $insertAt)
    $inserted[$insertAt] = 0x7E
    [Array]::Copy(
        $baseline,
        $insertAt,
        $inserted,
        $insertAt + 1,
        $baseline.Length - $insertAt)
    $insertion = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-inserted.lcb' `
        -CandidateBytes $inserted
    Assert-SelfTestTrue `
        -Condition (
            (-not $insertion.decision.checkpointIdentityAccepted) -and
            (-not $insertion.comparison.equalLength) -and
            (-not $insertion.comparison.changedByteCountDefined) -and
            (-not $insertion.recordParser.headerSourceInventory.exact) -and
            ($insertion.comparison.alignment -ceq
                'bounded-common-prefix-suffix') -and
            ($insertion.comparison.contiguousRunCount -eq 1)) `
        -Message 'length-changing artifact was accepted or misrepresented.'
    $negative++

    [byte[]]$preambleMutation = Copy-Bytes -Bytes $baseline
    $preambleOffset = $ClassesSignature.Length + 2
    $preambleMutation[$preambleOffset] =
        $preambleMutation[$preambleOffset] -bxor 1
    $preamble = New-ArtifactComparisonReport `
        -CheckpointDescriptor $descriptor `
        -CheckpointBytes $baseline `
        -CandidateDisplayPath 'synthetic-preamble-mutation.lcb' `
        -CandidateBytes $preambleMutation
    Assert-SelfTestTrue `
        -Condition (
            (-not $preamble.decision.checkpointIdentityAccepted) -and
            ($preamble.decision.disposition -ceq
                'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT') -and
            (-not $preamble.recordParser.firstSpecialRecord.exact)) `
        -Message 'first special preamble mutation was not rejected.'
    $negative++

    [byte[]]$badSignature = Copy-Bytes -Bytes $baseline
    $badSignature[0] = $badSignature[0] -bxor 1
    Assert-SelfTestThrows `
        -Action {
            Get-ClassOwnerInventory `
                -Bytes $badSignature -ArtifactOwner 'bad signature'
        } `
        -Message 'corrupt signature was accepted.'
    $negative++

    foreach ($relativePrefixOffset in @(-4, -1)) {
        [byte[]]$boundaryMutation = Copy-Bytes -Bytes $baseline
        $boundaryOffset =
            $opaqueRecord.SourcePathStart + $relativePrefixOffset
        $boundaryMutation[$boundaryOffset] =
            $boundaryMutation[$boundaryOffset] -bxor 1
        Assert-SelfTestThrows `
            -Action {
                Get-ClassOwnerInventory `
                    -Bytes $boundaryMutation `
                    -ArtifactOwner 'mutated source marker boundary'
            } `
            -Message (
                "source marker offset $relativePrefixOffset mutation was accepted.")
        $negative++
    }

    $duplicateText = $Latin1.GetString($baseline) +
        '.\Class\PosController\PosController.st|DUPLICATE|'
    [byte[]]$duplicate = $Latin1.GetBytes($duplicateText)
    Assert-SelfTestThrows `
        -Action {
            Get-ClassOwnerInventory `
                -Bytes $duplicate -ArtifactOwner 'duplicate owner'
        } `
        -Message 'duplicate owner record was accepted.'
    $negative++

    $jsonA = ConvertTo-DeterministicJson -Value $opaque
    $jsonB = ConvertTo-DeterministicJson -Value $opaque
    Assert-SelfTestTrue `
        -Condition ($jsonA -ceq $jsonB) `
        -Message 'JSON serialization is not deterministic.'
    $positive++

    $fixtureJson = ConvertTo-DeterministicJson `
        -Value (Get-JsonSelfTestFixture)
    $fixtureBase64 = [Convert]::ToBase64String(
        (Get-JsonFileBytes -Json $fixtureJson))
    $windowsPowerShell = Get-Command powershell.exe -ErrorAction SilentlyContinue
    $powerShell7 = Get-Command pwsh.exe -ErrorAction SilentlyContinue
    Assert-SelfTestTrue `
        -Condition (
            ($null -ne $windowsPowerShell) -and ($null -ne $powerShell7)) `
        -Message 'both PowerShell 5 and PowerShell 7 are required for JSON proof.'
    $ps5Fixture = Invoke-JsonFixtureHost `
        -HostPath $windowsPowerShell.Source -ScriptPath $PSCommandPath
    $ps7Fixture = Invoke-JsonFixtureHost `
        -HostPath $powerShell7.Source -ScriptPath $PSCommandPath
    Assert-SelfTestTrue `
        -Condition (
            ($fixtureBase64 -ceq $ps5Fixture) -and
            ($fixtureBase64 -ceq $ps7Fixture)) `
        -Message 'PowerShell 5/7 UTF-8 JSON bytes differ.'
    $positive++

    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    $tempDirectory = Join-Path `
        $tempRoot `
        ('LasalClassesComparatorSelfTest-' + [Guid]::NewGuid().ToString('N'))
    $junction = $null
    [void][IO.Directory]::CreateDirectory($tempDirectory)
    try {
        $output = Join-Path $tempDirectory 'comparison.json'
        $written = Write-CreateNewJson `
            -Root $Root -AllowedRoot $tempDirectory `
            -RequestedPath $output -Json $jsonA
        $expectedBytes = $Utf8NoBom.GetBytes($jsonA + "`n")
        $actualBytes = [IO.File]::ReadAllBytes($written)
        Assert-SelfTestTrue `
            -Condition (
                (Get-BytesSha256 -Bytes $expectedBytes) -ceq
                (Get-BytesSha256 -Bytes $actualBytes)) `
            -Message 'CreateNew output read-back differs.'
        $positive++
        Assert-SelfTestThrows `
            -Action {
                Write-CreateNewJson `
                    -Root $Root -AllowedRoot $tempDirectory `
                    -RequestedPath $output -Json $jsonA
            } `
            -Message 'CreateNew output overwrote an existing file.'
        $negative++
        $outsideOutput = Join-Path `
            $tempRoot `
            ('LasalClassesComparatorOutside-' +
                [Guid]::NewGuid().ToString('N') + '.json')
        Assert-SelfTestThrows `
            -Action {
                Write-CreateNewJson `
                    -Root $Root -AllowedRoot $tempDirectory `
                    -RequestedPath $outsideOutput -Json $jsonA
            } `
            -Message 'CreateNew output escaped its allowed root.'
        Assert-SelfTestTrue `
            -Condition (-not [IO.File]::Exists($outsideOutput)) `
            -Message 'escaped CreateNew output was created.'
        $negative++

        $candidateRoot = Join-Path $tempDirectory 'candidate-root'
        $candidateOutside = Join-Path $tempDirectory 'candidate-outside'
        [void][IO.Directory]::CreateDirectory($candidateRoot)
        [void][IO.Directory]::CreateDirectory($candidateOutside)
        $outsideCandidate = Join-Path $candidateOutside 'Classes.lcb'
        [IO.File]::WriteAllBytes($outsideCandidate, $baseline)
        $junction = Join-Path $candidateRoot 'escape'
        [void](New-Item `
                -ItemType Junction `
                -Path $junction `
                -Target $candidateOutside `
                -ErrorAction Stop)
        Assert-SelfTestThrows `
            -Action {
                Resolve-CandidateFile `
                    -Root $candidateRoot `
                    -RequestedPath 'escape\Classes.lcb'
            } `
            -Message 'candidate parent junction escaped its root.'
        $negative++
    }
    finally {
        $resolvedTemp = [IO.Path]::GetFullPath($tempDirectory)
        if (($null -ne $junction) -and
            [IO.Directory]::Exists($junction)) {
            [IO.Directory]::Delete($junction)
        }
        if (($resolvedTemp.StartsWith(
                    $tempRoot + '\',
                    [StringComparison]::OrdinalIgnoreCase)) -and
            [IO.Directory]::Exists($resolvedTemp)) {
            [IO.Directory]::Delete($resolvedTemp, $true)
        }
    }

    $headCommit = Invoke-GitText `
        -Root $Root `
        -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
        -Operation 'Self-test HEAD resolution'
    $head = Resolve-CheckpointBlob `
        -Root $Root -RequestedCheckpoint $headCommit `
        -RelativePath $CanonicalClassesPath
    [byte[]]$headBytes = Read-GitBlobBytes `
        -Root $Root -BlobOid $head.blobOid
    $directBlob = Resolve-CheckpointBlob `
        -Root $Root -RequestedCheckpoint $head.blobOid `
        -RelativePath $CanonicalClassesPath
    [byte[]]$directBytes = Read-GitBlobBytes `
        -Root $Root -BlobOid $directBlob.blobOid
    Assert-SelfTestTrue `
        -Condition (
            ($head.kind -ceq 'revision') -and
            ($directBlob.kind -ceq 'blob') -and
            ($head.blobOid -ceq $directBlob.blobOid) -and
            ((Get-BytesSha256 -Bytes $headBytes) -ceq
                (Get-BytesSha256 -Bytes $directBytes))) `
        -Message 'Git revision/blob resolution differs.'
    $positive++
    Assert-SelfTestThrows `
        -Action {
            Resolve-CheckpointBlob `
                -Root $Root `
                -RequestedCheckpoint ('f' * 40) `
                -RelativePath $CanonicalClassesPath
        } `
        -Message 'invalid Git revision was accepted.'
    $negative++
    Assert-SelfTestThrows `
        -Action {
            Resolve-CheckpointBlob `
                -Root $Root `
                -RequestedCheckpoint '5543579' `
                -RelativePath $CanonicalClassesPath
        } `
        -Message 'short checkpoint OID was accepted.'
    $negative++

    Assert-SelfTestTrue `
        -Condition (
            (-not $opaque.decision.checkpointIdentityAccepted) -and
            (-not $target.decision.checkpointIdentityAccepted) -and
            (-not $protectedReport.decision.checkpointIdentityAccepted) -and
            (-not $insertion.decision.checkpointIdentityAccepted) -and
            (-not $preamble.decision.checkpointIdentityAccepted)) `
        -Message 'a non-exact candidate acquired approval.'
    $positive++
    Write-Output (
        "PASS LasalClassesArtifactComparator.SelfTest Positive=$positive " +
        "Negative=$negative")
}

function New-BlockedJson {
    param([Parameter(Mandatory = $true)][string]$Message)
    $normalized = $Message
    if ($normalized.StartsWith('BLOCKED: ', [StringComparison]::Ordinal)) {
        $normalized = $normalized.Substring(9)
    }
    return ConvertTo-DeterministicJson -Value ([ordered]@{
            schema = $ScriptSchema
            decision = [ordered]@{
                disposition = 'BLOCKED_INVALID_INPUT'
                checkpointIdentityAccepted = $false
                approvalScope = 'checkpoint-byte-identity-only'
                productionApproved = $false
                exactCheckpointMatch = $false
                semanticEquivalenceProven = $false
                exitCode = 4
            }
            error = [ordered]@{
                message = $normalized
            }
        })
}

try {
    Assert-InvocationContract -Bound $PSBoundParameters
    if ($EmitJsonSelfTestFixtureBase64) {
        $fixtureJson = ConvertTo-DeterministicJson `
            -Value (Get-JsonSelfTestFixture)
        Write-Output ([Convert]::ToBase64String(
                (Get-JsonFileBytes -Json $fixtureJson)))
        exit 0
    }
    $resolvedRoot = Resolve-RepositoryContext -RequestedRoot $RepositoryRoot
    if ($RunSelfTest) {
        Invoke-SelfTest -Root $resolvedRoot
        exit 0
    }

    $checkpointDescriptor = Resolve-CheckpointBlob `
        -Root $resolvedRoot `
        -RequestedCheckpoint $Checkpoint `
        -RelativePath $CheckpointPath
    [byte[]]$checkpointBytes = Read-GitBlobBytes `
        -Root $resolvedRoot -BlobOid $checkpointDescriptor.blobOid
    $candidate = Resolve-CandidateFile `
        -Root $resolvedRoot -RequestedPath $CandidatePath
    [byte[]]$candidateBytes = Read-StableFileBytes -Path $candidate.fullPath
    $report = New-ArtifactComparisonReport `
        -CheckpointDescriptor $checkpointDescriptor `
        -CheckpointBytes $checkpointBytes `
        -CandidateDisplayPath $candidate.displayPath `
        -CandidateBytes $candidateBytes
    $json = ConvertTo-DeterministicJson -Value $report
    if ($CreateNew) {
        [void](Write-CreateNewJson `
                -Root $resolvedRoot `
                -AllowedRoot $PSScriptRoot `
                -RequestedPath $OutputPath `
                -Json $json)
    }
    Write-JsonStdout -Json $json
    exit ([int]$report.decision.exitCode)
}
catch {
    Write-JsonStdout -Json (New-BlockedJson -Message $_.Exception.Message)
    exit 4
}

function ConvertTo-LmcDistributionFullPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'Distribution path must not be empty.'
    }

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $pathRoot = [System.IO.Path]::GetPathRoot($fullPath)
    if (-not $fullPath.Equals(
        $pathRoot,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        $fullPath = $fullPath.TrimEnd(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar)
    }
    return $fullPath
}

function Test-LmcDistributionReparsePoint {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileSystemInfo]$Item
    )

    return (($Item.Attributes -band
        [System.IO.FileAttributes]::ReparsePoint) -ne 0)
}

function Resolve-LmcDistributionManualInputs {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalPdfPath,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalDocxPath,
        [string]$ManualPdfPath,
        [string]$ManualDocxPath
    )

    $pdfSpecified = -not [string]::IsNullOrWhiteSpace($ManualPdfPath)
    $docxSpecified = -not [string]::IsNullOrWhiteSpace($ManualDocxPath)
    if ($pdfSpecified -xor $docxSpecified) {
        throw 'ManualPdfPath and ManualDocxPath must be supplied together.'
    }

    $repository = ConvertTo-LmcDistributionFullPath -Path $RepositoryRoot
    if (-not (Test-Path -LiteralPath $repository -PathType Container)) {
        throw "Manual input repository root was not found: $repository"
    }

    if ($pdfSpecified) {
        $pdf = ConvertTo-LmcDistributionFullPath -Path $ManualPdfPath
        $docx = ConvertTo-LmcDistributionFullPath -Path $ManualDocxPath
    }
    else {
        $pdf = ConvertTo-LmcDistributionFullPath -Path $CanonicalPdfPath
        $docx = ConvertTo-LmcDistributionFullPath -Path $CanonicalDocxPath
    }

    $repositoryPrefix = $repository + `
        [System.IO.Path]::DirectorySeparatorChar
    foreach ($entry in @(
        [pscustomobject]@{
            Name = 'PDF'
            Path = $pdf
            Extension = '.pdf'
        },
        [pscustomobject]@{
            Name = 'DOCX'
            Path = $docx
            Extension = '.docx'
        })) {
        if (-not $entry.Path.StartsWith(
            $repositoryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Manual $($entry.Name) input escaped the repository: $($entry.Path)"
        }
        if (-not [System.IO.Path]::GetExtension($entry.Path).Equals(
            $entry.Extension,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Manual $($entry.Name) input must use the $($entry.Extension) extension: $($entry.Path)"
        }
        if (-not (Test-Path -LiteralPath $entry.Path -PathType Leaf)) {
            throw "Manual $($entry.Name) input was not found: $($entry.Path)"
        }

        $current = Get-Item -LiteralPath $entry.Path -Force
        while ($true) {
            if (Test-LmcDistributionReparsePoint -Item $current) {
                throw "Manual $($entry.Name) input traverses a reparse point: $($entry.Path)"
            }
            if ($current.FullName.Equals(
                $repository,
                [System.StringComparison]::OrdinalIgnoreCase)) {
                break
            }
            if ($current -is [System.IO.FileInfo]) {
                $parent = $current.Directory
            }
            else {
                $parent = $current.Parent
            }
            if ($null -eq $parent) {
                throw "Manual $($entry.Name) input escaped the repository: $($entry.Path)"
            }
            $current = $parent
        }
    }

    return [pscustomobject]@{
        PdfPath = $pdf
        DocxPath = $docx
        UsesCanonicalInputs = -not $pdfSpecified
    }
}

function Get-LmcDistributionManualWorktreeState {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [bool]$UsesCanonicalInputs,
        [Parameter(Mandatory = $true)]
        [ValidateSet('clean', 'dirty-preview')]
        [string]$WorktreeState,
        [switch]$AllowDirty
    )

    if ($UsesCanonicalInputs) {
        return $WorktreeState
    }
    if (-not $AllowDirty) {
        throw 'Noncanonical manual inputs require -AllowDirty for a preview build.'
    }
    return 'dirty-preview'
}

function Get-LmcDistributionBytesSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
            $sha256.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $sha256.Dispose()
    }
}

function Resolve-LmcDistributionRunExampleExecutable {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$StagingRoot,
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,
        [string]$Context = 'Executable relaunch gate input'
    )

    $root = ConvertTo-LmcDistributionFullPath -Path $StagingRoot
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "$Context staging root was not found: $root"
    }
    $rootItem = Get-Item -LiteralPath $root -Force
    if (Test-LmcDistributionReparsePoint -Item $rootItem) {
        throw "$Context staging root must not be a reparse point: $root"
    }

    $expected = ConvertTo-LmcDistributionFullPath -Path (
        Join-Path $root (
            '02_Example_Program\Run\LasalMotionControlApiExample.exe'))
    $actual = ConvertTo-LmcDistributionFullPath -Path $ExecutablePath
    if (-not $actual.Equals(
            $expected,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Context path must be the exact staged Run EXE. expected=$expected actual=$actual"
    }
    if (-not (Test-Path -LiteralPath $actual -PathType Leaf)) {
        throw "$Context was not found: $actual"
    }

    $current = Get-Item -LiteralPath $actual -Force
    while ($true) {
        if (Test-LmcDistributionReparsePoint -Item $current) {
            throw "$Context traverses a reparse point: $actual"
        }
        if ($current.FullName.Equals(
                $root,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        if ($current -is [System.IO.FileInfo]) {
            $current = $current.Directory
        }
        else {
            $current = $current.Parent
        }
        if ($null -eq $current) {
            throw "$Context escaped its staging root: $actual"
        }
    }

    return $actual
}

function Invoke-LmcDistributionExecutableRelaunchGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$StagingRoot,
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,
        [Parameter(Mandatory = $true)]
        [scriptblock]$GateAction
    )

    $resolvedExecutable = Resolve-LmcDistributionRunExampleExecutable `
        -StagingRoot $StagingRoot `
        -ExecutablePath $ExecutablePath
    $beforeHash = (Get-FileHash `
        -LiteralPath $resolvedExecutable `
        -Algorithm SHA256).Hash.ToUpperInvariant()

    & $GateAction $resolvedExecutable | Out-Null

    $resolvedAfterGate = Resolve-LmcDistributionRunExampleExecutable `
        -StagingRoot $StagingRoot `
        -ExecutablePath $ExecutablePath `
        -Context 'Executable relaunch gate output'
    $afterHash = (Get-FileHash `
        -LiteralPath $resolvedAfterGate `
        -Algorithm SHA256).Hash.ToUpperInvariant()
    if (-not [string]::Equals(
            $beforeHash,
            $afterHash,
            [System.StringComparison]::Ordinal)) {
        throw "The staged example EXE changed while the executable relaunch gate ran. before=$beforeHash after=$afterHash"
    }

    return $beforeHash
}

function Assert-LmcDistributionExecutableRelaunchIdentity {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$StagingRoot,
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,
        [Parameter(Mandatory = $true)]
        [string]$TestedSha256
    )

    if ($TestedSha256 -cnotmatch '^[0-9A-F]{64}$') {
        throw 'Executable relaunch tested SHA256 must be exactly 64 uppercase hexadecimal characters.'
    }
    $resolvedExecutable = Resolve-LmcDistributionRunExampleExecutable `
        -StagingRoot $StagingRoot `
        -ExecutablePath $ExecutablePath `
        -Context 'Final executable relaunch identity input'
    $finalHash = (Get-FileHash `
        -LiteralPath $resolvedExecutable `
        -Algorithm SHA256).Hash.ToUpperInvariant()
    if (-not [string]::Equals(
            $TestedSha256,
            $finalHash,
            [System.StringComparison]::Ordinal)) {
        throw "The final example EXE bytes do not match the executable relaunch gate input. tested=$TestedSha256 final=$finalHash"
    }

    return $finalHash
}

function Read-LmcDistributionLockedStreamBytes {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileStream]$Stream
    )

    $Stream.Position = 0
    $memory = New-Object System.IO.MemoryStream
    try {
        $Stream.CopyTo($memory)
        return ,$memory.ToArray()
    }
    finally {
        $memory.Dispose()
    }
}

function New-LmcDistributionManualInputSnapshot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [string]$PdfPath,
        [Parameter(Mandatory = $true)]
        [string]$DocxPath
    )

    $resolved = Resolve-LmcDistributionManualInputs `
        -RepositoryRoot $RepositoryRoot `
        -CanonicalPdfPath $PdfPath `
        -CanonicalDocxPath $DocxPath
    $pdfStream = $null
    $docxStream = $null
    try {
        $pdfStream = [System.IO.File]::Open(
            $resolved.PdfPath,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::Read)
        $docxStream = [System.IO.File]::Open(
            $resolved.DocxPath,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::Read)

        $resolvedWhileLocked = Resolve-LmcDistributionManualInputs `
            -RepositoryRoot $RepositoryRoot `
            -CanonicalPdfPath $resolved.PdfPath `
            -CanonicalDocxPath $resolved.DocxPath
        if (-not $resolvedWhileLocked.PdfPath.Equals(
            $resolved.PdfPath,
            [System.StringComparison]::OrdinalIgnoreCase) -or
            -not $resolvedWhileLocked.DocxPath.Equals(
                $resolved.DocxPath,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Manual input paths changed while their snapshot was locked.'
        }

        [byte[]]$pdfBytes = Read-LmcDistributionLockedStreamBytes `
            -Stream $pdfStream
        [byte[]]$docxBytes = Read-LmcDistributionLockedStreamBytes `
            -Stream $docxStream

        $null = Resolve-LmcDistributionManualInputs `
            -RepositoryRoot $RepositoryRoot `
            -CanonicalPdfPath $resolved.PdfPath `
            -CanonicalDocxPath $resolved.DocxPath

        return [pscustomobject]@{
            PdfPath = $resolved.PdfPath
            PdfBytes = $pdfBytes
            PdfLength = [long]$pdfBytes.LongLength
            PdfSha256 = Get-LmcDistributionBytesSha256 -Bytes $pdfBytes
            DocxPath = $resolved.DocxPath
            DocxBytes = $docxBytes
            DocxLength = [long]$docxBytes.LongLength
            DocxSha256 = Get-LmcDistributionBytesSha256 -Bytes $docxBytes
        }
    }
    finally {
        if ($null -ne $docxStream) {
            $docxStream.Dispose()
        }
        if ($null -ne $pdfStream) {
            $pdfStream.Dispose()
        }
    }
}

function Assert-LmcDistributionTreeHasNoReparsePoints {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [string]$Context = 'distribution tree'
    )

    $fullRoot = ConvertTo-LmcDistributionFullPath -Path $Root
    if (-not (Test-Path -LiteralPath $fullRoot -PathType Container)) {
        throw "$Context directory was not found: $fullRoot"
    }

    $pending = New-Object 'System.Collections.Generic.Stack[string]'
    $pending.Push($fullRoot)
    while ($pending.Count -gt 0) {
        $directoryPath = $pending.Pop()
        $directory = Get-Item -LiteralPath $directoryPath -Force
        if (Test-LmcDistributionReparsePoint -Item $directory) {
            throw "$Context contains a reparse point: $($directory.FullName)"
        }

        foreach ($child in @(Get-ChildItem -LiteralPath $directoryPath -Force)) {
            if (Test-LmcDistributionReparsePoint -Item $child) {
                throw "$Context contains a reparse point: $($child.FullName)"
            }
            if ($child.PSIsContainer) {
                $pending.Push($child.FullName)
            }
        }
    }
}

function Get-LmcDistributionTreeSnapshot {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $fullRoot = ConvertTo-LmcDistributionFullPath -Path $Root
    if (-not (Test-Path -LiteralPath $fullRoot -PathType Container)) {
        throw "Distribution snapshot root was not found: $fullRoot"
    }

    $rootItem = Get-Item -LiteralPath $fullRoot -Force
    if (Test-LmcDistributionReparsePoint -Item $rootItem) {
        throw "Distribution snapshot root is a reparse point: $fullRoot"
    }

    $rootPrefix = $fullRoot + [System.IO.Path]::DirectorySeparatorChar
    $pending = New-Object 'System.Collections.Generic.Stack[string]'
    $pending.Push($fullRoot)
    $records = New-Object 'System.Collections.Generic.List[object]'
    $encodedRecords = New-Object 'System.Collections.Generic.List[string]'

    while ($pending.Count -gt 0) {
        $directoryPath = ConvertTo-LmcDistributionFullPath -Path $pending.Pop()
        $directoryItem = Get-Item -LiteralPath $directoryPath -Force
        if (Test-LmcDistributionReparsePoint -Item $directoryItem) {
            throw "Distribution snapshot contains a reparse point: $directoryPath"
        }

        if ($directoryPath.Equals(
            $fullRoot,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $directoryRelativePath = '.'
        }
        else {
            if (-not $directoryPath.StartsWith(
                $rootPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Distribution snapshot enumeration escaped its root: $directoryPath"
            }
            $directoryRelativePath = $directoryPath.Substring(
                $rootPrefix.Length).Replace('\', '/')
        }

        $directoryPathToken = [System.Convert]::ToBase64String(
            [System.Text.Encoding]::UTF8.GetBytes($directoryRelativePath))
        $directoryEncoded = "D|$directoryPathToken"
        $records.Add([pscustomobject]@{
            Kind = 'Directory'
            RelativePath = $directoryRelativePath
            Length = $null
            Sha256 = $null
            Encoded = $directoryEncoded
        })
        $encodedRecords.Add($directoryEncoded)

        foreach ($child in @(Get-ChildItem -LiteralPath $directoryPath -Force)) {
            $childFullPath = [System.IO.Path]::GetFullPath($child.FullName)
            if (-not $childFullPath.StartsWith(
                $rootPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Distribution snapshot enumeration escaped its root: $childFullPath"
            }
            if (Test-LmcDistributionReparsePoint -Item $child) {
                throw "Distribution snapshot contains a reparse point: $childFullPath"
            }

            if ($child.PSIsContainer) {
                $pending.Push($childFullPath)
                continue
            }

            $relativePath = $childFullPath.Substring(
                $rootPrefix.Length).Replace('\', '/')
            $fileHash = (Get-FileHash -LiteralPath $childFullPath `
                -Algorithm SHA256).Hash.ToUpperInvariant()
            $pathToken = [System.Convert]::ToBase64String(
                [System.Text.Encoding]::UTF8.GetBytes($relativePath))
            $encoded = "F|$pathToken|$([Int64]$child.Length)|$fileHash"
            $records.Add([pscustomobject]@{
                Kind = 'File'
                RelativePath = $relativePath
                Length = [Int64]$child.Length
                Sha256 = $fileHash
                Encoded = $encoded
            })
            $encodedRecords.Add($encoded)
        }
    }

    $encodedArray = [string[]]$encodedRecords.ToArray()
    [System.Array]::Sort(
        $encodedArray,
        [System.StringComparer]::Ordinal)
    $serialized = [System.String]::Join("`n", $encodedArray) + "`n"
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $digestBytes = $sha256.ComputeHash(
            [System.Text.Encoding]::UTF8.GetBytes($serialized))
    }
    finally {
        $sha256.Dispose()
    }
    $treeHash = [System.BitConverter]::ToString(
        $digestBytes).Replace('-', '')

    return [pscustomobject]@{
        Root = $fullRoot
        Sha256 = $treeHash
        RecordCount = $encodedArray.Count
        Records = @($records | Sort-Object -Property Encoded)
    }
}

function Assert-LmcDistributionTreeSnapshotEqual {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [object]$Expected,
        [Parameter(Mandatory = $true)]
        [object]$Actual,
        [string]$Context = 'Distribution tree'
    )

    if ([string]::Equals(
        [string]$Expected.Sha256,
        [string]$Actual.Sha256,
        [System.StringComparison]::Ordinal)) {
        return
    }

    $expectedRecords = @($Expected.Records)
    $actualRecords = @($Actual.Records)
    $maximum = [System.Math]::Max(
        $expectedRecords.Count,
        $actualRecords.Count)
    $firstDifference = 'record count differs'
    for ($index = 0; $index -lt $maximum; $index += 1) {
        $expectedRecord = if ($index -lt $expectedRecords.Count) {
            [string]$expectedRecords[$index].Encoded
        }
        else {
            '<missing>'
        }
        $actualRecord = if ($index -lt $actualRecords.Count) {
            [string]$actualRecords[$index].Encoded
        }
        else {
            '<missing>'
        }
        if (-not [string]::Equals(
            $expectedRecord,
            $actualRecord,
            [System.StringComparison]::Ordinal)) {
            $firstDifference = "index=$index expected=$expectedRecord actual=$actualRecord"
            break
        }
    }

    throw "$Context changed. expected SHA256=$($Expected.Sha256), actual SHA256=$($Actual.Sha256), first difference: $firstDifference"
}

function Remove-LmcDistributionStagingDirectory {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$StagingPath,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedParent
    )

    $fullParent = ConvertTo-LmcDistributionFullPath -Path $ExpectedParent
    if (-not (Test-Path -LiteralPath $fullParent -PathType Container)) {
        throw "Expected staging parent was not found: $fullParent"
    }
    $fullStagingPath = ConvertTo-LmcDistributionFullPath -Path $StagingPath
    $actualParent = ConvertTo-LmcDistributionFullPath -Path (
        [System.IO.Path]::GetDirectoryName($fullStagingPath))
    if (-not $actualParent.Equals(
        $fullParent,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Staging cleanup refused a path outside the expected parent: $fullStagingPath"
    }

    $leafName = [System.IO.Path]::GetFileName($fullStagingPath)
    if ($leafName -notmatch
        '(?i)^\.LMC_API_Distribution\.stage\.[0-9a-f]{32}$') {
        throw "Staging cleanup refused an unexpected directory name: $leafName"
    }

    if (-not (Test-Path -LiteralPath $fullStagingPath)) {
        return
    }
    if (-not (Test-Path -LiteralPath $fullStagingPath -PathType Container)) {
        throw "Staging cleanup target is not a directory: $fullStagingPath"
    }

    Assert-LmcDistributionTreeHasNoReparsePoints `
        -Root $fullStagingPath `
        -Context 'Staging cleanup tree'
    [System.IO.Directory]::Delete($fullStagingPath, $true)
}

function Get-LmcDistributionInputFingerprint {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Provider,
        [AllowNull()]
        [object]$PreparedInputs
    )

    $values = @(& $Provider $PreparedInputs)
    if ($values.Count -ne 1) {
        throw 'The input fingerprint provider must return exactly one value.'
    }
    $fingerprint = [string]$values[0]
    if ([string]::IsNullOrWhiteSpace($fingerprint)) {
        throw 'The input fingerprint provider returned an empty value.'
    }
    return $fingerprint
}

function Open-LmcDistributionTransactionLock {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ParentPath
    )

    $lockPath = Join-Path $ParentPath `
        '.LMC_API_Distribution.transaction.lock'
    if (Test-Path -LiteralPath $lockPath) {
        $lockItem = Get-Item -LiteralPath $lockPath -Force
        if ($lockItem.PSIsContainer -or
            (Test-LmcDistributionReparsePoint -Item $lockItem)) {
            throw "Distribution transaction lock path is unsafe: $lockPath"
        }
    }

    try {
        $stream = [System.IO.File]::Open(
            $lockPath,
            [System.IO.FileMode]::OpenOrCreate,
            [System.IO.FileAccess]::ReadWrite,
            [System.IO.FileShare]::None)
    }
    catch [System.IO.IOException] {
        throw "The exclusive distribution transaction lock is already held: $lockPath"
    }

    try {
        $ownerText = "pid=$PID`r`nacquired-utc=$([DateTime]::UtcNow.ToString('o'))`r`n"
        $ownerBytes = [System.Text.Encoding]::UTF8.GetBytes($ownerText)
        $stream.SetLength(0)
        $stream.Write($ownerBytes, 0, $ownerBytes.Length)
        $stream.Flush()
        return $stream
    }
    catch {
        $stream.Dispose()
        throw
    }
}

function Remove-LmcDistributionTransactionLock {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LockPath,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedParent
    )

    $fullParent = ConvertTo-LmcDistributionFullPath -Path $ExpectedParent
    $fullLockPath = ConvertTo-LmcDistributionFullPath -Path $LockPath
    $actualParent = ConvertTo-LmcDistributionFullPath -Path (
        [System.IO.Path]::GetDirectoryName($fullLockPath))
    if (-not $actualParent.Equals(
        $fullParent,
        [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($fullLockPath) -ne
            '.LMC_API_Distribution.transaction.lock') {
        return
    }
    if (-not (Test-Path -LiteralPath $fullLockPath)) {
        return
    }

    $lockItem = Get-Item -LiteralPath $fullLockPath -Force
    if ($lockItem.PSIsContainer -or
        (Test-LmcDistributionReparsePoint -Item $lockItem)) {
        return
    }

    try {
        [System.IO.File]::Delete($fullLockPath)
    }
    catch [System.IO.IOException] {
        # A successor may already hold the same FileShare.None lock. Leave it.
    }
    catch [System.UnauthorizedAccessException] {
        # Refuse to force deletion when ownership or sharing cannot be proven.
    }
}

function Invoke-LmcDistributionCandidateTransaction {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalRoot,
        [Parameter(Mandatory = $true)]
        [string]$CandidatePath,
        [Parameter(Mandatory = $true)]
        [scriptblock]$PopulateAndValidate,
        [Parameter(Mandatory = $true)]
        [scriptblock]$GetInputFingerprint,
        [scriptblock]$PrepareInputs,
        [scriptblock]$ValidatePreparedInputs,
        [scriptblock]$BeforePromotion
    )

    $canonical = ConvertTo-LmcDistributionFullPath -Path $CanonicalRoot
    if (-not (Test-Path -LiteralPath $canonical -PathType Container)) {
        throw "Canonical distribution root was not found: $canonical"
    }
    $canonicalItem = Get-Item -LiteralPath $canonical -Force
    if (Test-LmcDistributionReparsePoint -Item $canonicalItem) {
        throw "Canonical distribution root must not be a reparse point: $canonical"
    }

    $parent = ConvertTo-LmcDistributionFullPath -Path (
        [System.IO.Path]::GetDirectoryName($canonical))
    $candidate = ConvertTo-LmcDistributionFullPath -Path $CandidatePath
    $candidateParent = ConvertTo-LmcDistributionFullPath -Path (
        [System.IO.Path]::GetDirectoryName($candidate))
    if (-not $candidateParent.Equals(
        $parent,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'CandidatePath must be a direct sibling of CanonicalRoot.'
    }

    $candidateName = [System.IO.Path]::GetFileName($candidate)
    if ($candidateName -notmatch
        '(?i)^LMC_API_Distribution_candidate_[A-Za-z0-9][A-Za-z0-9._-]*$') {
        throw 'CandidatePath name must match LMC_API_Distribution_candidate_*.'
    }

    $canonicalVolume = [System.IO.Path]::GetPathRoot($canonical)
    $candidateVolume = [System.IO.Path]::GetPathRoot($candidate)
    if (-not $canonicalVolume.Equals(
        $candidateVolume,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'CanonicalRoot and CandidatePath must be on the same volume.'
    }
    if (Test-Path -LiteralPath $candidate) {
        throw "CandidatePath must not already exist: $candidate"
    }

    $lockPath = Join-Path $parent `
        '.LMC_API_Distribution.transaction.lock'
    $lockStream = $null
    $lockAcquired = $false
    $stage = $null
    $stageCreated = $false
    $committed = $false
    try {
        $lockStream = Open-LmcDistributionTransactionLock -ParentPath $parent
        $lockAcquired = $true
        if (Test-Path -LiteralPath $candidate) {
            throw "CandidatePath must not already exist: $candidate"
        }

        $canonicalBaseline = Get-LmcDistributionTreeSnapshot -Root $canonical
        $preparedInputs = $null
        if ($null -ne $PrepareInputs) {
            $preparedValues = @(& $PrepareInputs)
            if ($preparedValues.Count -ne 1 -or
                $null -eq $preparedValues[0]) {
                throw 'PrepareInputs must return exactly one non-null value.'
            }
            $preparedInputs = $preparedValues[0]
        }
        $inputBaseline = Get-LmcDistributionInputFingerprint `
            -Provider $GetInputFingerprint `
            -PreparedInputs $preparedInputs

        $stageName = '.LMC_API_Distribution.stage.' +
            [System.Guid]::NewGuid().ToString('N')
        $stage = Join-Path $parent $stageName
        if (Test-Path -LiteralPath $stage) {
            throw "Generated staging path already exists: $stage"
        }
        $stageVolume = [System.IO.Path]::GetPathRoot($stage)
        if (-not $canonicalVolume.Equals(
            $stageVolume,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Generated staging path is not on the canonical volume.'
        }
        [System.IO.Directory]::CreateDirectory($stage) | Out-Null
        $stageCreated = $true

        & $PopulateAndValidate $stage $inputBaseline $preparedInputs |
            Out-Null
        if (-not (Test-Path -LiteralPath $stage -PathType Container)) {
            throw 'PopulateAndValidate removed the staging directory.'
        }
        $seal = Get-LmcDistributionTreeSnapshot -Root $stage

        if ($null -ne $BeforePromotion) {
            & $BeforePromotion $stage $candidate | Out-Null
        }

        if (-not (Test-Path -LiteralPath $stage -PathType Container)) {
            throw 'Candidate staging directory disappeared after validation.'
        }
        $prePromotionSnapshot = Get-LmcDistributionTreeSnapshot -Root $stage
        Assert-LmcDistributionTreeSnapshotEqual `
            -Expected $seal `
            -Actual $prePromotionSnapshot `
            -Context 'Candidate staging tree after validation'

        $inputBeforePromotion = Get-LmcDistributionInputFingerprint `
            -Provider $GetInputFingerprint `
            -PreparedInputs $null
        if (-not [string]::Equals(
            $inputBaseline,
            $inputBeforePromotion,
            [System.StringComparison]::Ordinal)) {
            throw "Distribution input fingerprint changed before promotion. expected=$inputBaseline actual=$inputBeforePromotion"
        }
        if ($null -ne $ValidatePreparedInputs) {
            & $ValidatePreparedInputs $preparedInputs $stage $candidate |
                Out-Null
        }

        $canonicalBeforePromotion = Get-LmcDistributionTreeSnapshot `
            -Root $canonical
        Assert-LmcDistributionTreeSnapshotEqual `
            -Expected $canonicalBaseline `
            -Actual $canonicalBeforePromotion `
            -Context 'Canonical distribution before promotion'

        if (Test-Path -LiteralPath $candidate) {
            throw "CandidatePath must not already exist before promotion: $candidate"
        }

        [System.IO.Directory]::Move($stage, $candidate)
        $committed = $true

        $publishedSnapshot = Get-LmcDistributionTreeSnapshot -Root $candidate
        Assert-LmcDistributionTreeSnapshotEqual `
            -Expected $seal `
            -Actual $publishedSnapshot `
            -Context 'Published candidate after promotion'

        $canonicalAfterPromotion = Get-LmcDistributionTreeSnapshot `
            -Root $canonical
        Assert-LmcDistributionTreeSnapshotEqual `
            -Expected $canonicalBaseline `
            -Actual $canonicalAfterPromotion `
            -Context 'Canonical distribution after promotion'

        return [pscustomobject]@{
            Committed = $true
            CanonicalRoot = $canonical
            CandidatePath = $candidate
            InputFingerprint = $inputBaseline
            CanonicalSnapshotSha256 = $canonicalBaseline.Sha256
            CandidateSnapshotSha256 = $seal.Sha256
            CandidateRecordCount = $seal.RecordCount
        }
    }
    catch {
        $transactionFailure = $_.Exception
        if (-not $committed -and $stageCreated -and
            -not [string]::IsNullOrWhiteSpace($stage) -and
            (Test-Path -LiteralPath $stage)) {
            try {
                Remove-LmcDistributionStagingDirectory `
                    -StagingPath $stage `
                    -ExpectedParent $parent
            }
            catch {
                $cleanupFailure = $_.Exception
                throw "Distribution candidate transaction failed before promotion: $($transactionFailure.Message) Staging cleanup also failed or was refused: $($cleanupFailure.Message)"
            }
        }
        throw $transactionFailure
    }
    finally {
        if ($null -ne $lockStream) {
            $lockStream.Dispose()
        }
        if ($lockAcquired) {
            Remove-LmcDistributionTransactionLock `
                -LockPath $lockPath `
                -ExpectedParent $parent
        }
    }
}

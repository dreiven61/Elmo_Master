if ($null -eq (Get-Command `
        -Name 'Assert-LmcDistributionToolchainManifestBinding' `
        -CommandType Function `
        -ErrorAction SilentlyContinue)) {
    $provenanceImplementation = Join-Path $PSScriptRoot `
        'DistributionToolchainProvenance.ps1'
    if (-not (Test-Path `
            -LiteralPath $provenanceImplementation `
            -PathType Leaf)) {
        throw "Distribution toolchain provenance implementation not found: $provenanceImplementation"
    }
    . $provenanceImplementation
}

function Assert-LmcReleaseManifestSafeText {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [string]$Context = 'release manifest'
    )

    $forbiddenPatterns = @(
        @('[A-Za-z]:[\\/]', 'absolute Windows path'),
        @('\\\\[^\\/\r\n]+[\\/]', 'UNC path'),
        @('(?im)(^|[\s`"''|(])/(Users|home|work|git|tmp|var|mnt|opt)/',
            'absolute host path'),
        @('(^|[\\/])\.\.([\\/]|$)', 'parent-directory traversal'),
        @('LMC_API_Delivery', 'internal delivery source path'),
        @('Lasal_PRG', 'internal LASAL source path'),
        @('ProjectInternal', 'LASAL IDE internal path'),
        @('Codex_', 'internal Codex project path'),
        @('Elmo_API_Packet2', 'internal packet-analysis path')
    )

    foreach ($entry in $forbiddenPatterns) {
        if ([regex]::IsMatch($Text, $entry[0])) {
            throw "$Context contains a forbidden $($entry[1])."
        }
    }
}

function Get-LmcReleaseManifestInputGitStatus {
    [CmdletBinding()]
    param(
        [AllowNull()]
        [object]$GitStatus,
        [Parameter(Mandatory = $true)]
        [string]$ManifestRepositoryRelativePath
    )

    $manifestPath = $ManifestRepositoryRelativePath.Replace('\', '/')
    if ([System.IO.Path]::IsPathRooted($ManifestRepositoryRelativePath) -or
        $manifestPath -notmatch '^[A-Za-z0-9._/-]+$' -or
        $manifestPath.StartsWith('./') -or
        $manifestPath.StartsWith('../') -or
        $manifestPath.Contains('/../') -or
        $manifestPath.EndsWith('/')) {
        throw 'Manifest Git path must be an exact repository-relative path.'
    }

    $lastSeparator = $manifestPath.LastIndexOf('/')
    if ($lastSeparator -ge 0) {
        $manifestDirectory = $manifestPath.Substring(0, $lastSeparator)
        $manifestFileName = $manifestPath.Substring($lastSeparator + 1)
        $temporaryPrefix = $manifestDirectory + '/.' + $manifestFileName + '.'
    }
    else {
        $manifestFileName = $manifestPath
        $temporaryPrefix = '.' + $manifestFileName + '.'
    }
    if ([string]::IsNullOrWhiteSpace($manifestFileName)) {
        throw 'Manifest Git path must include a file name.'
    }

    $temporaryPattern = '^' + [regex]::Escape($temporaryPrefix) +
        '[0-9a-fA-F]{32}\.(tmp|bak)$'
    $generatedOutputStatusCodes = @(
        '??',
        ' M',
        'M ',
        'MM',
        'A ',
        'AM',
        ' D',
        'D ',
        'AD',
        'MD'
    )
    $inputStatus = @()
    foreach ($rawLine in @($GitStatus)) {
        $line = [string]$rawLine
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $isGeneratedOutput = $false
        if ($line.Length -ge 4 -and $line[2] -eq ' ') {
            $statusCode = $line.Substring(0, 2)
            $statusPath = $line.Substring(3).Replace('\', '/')
            if ($generatedOutputStatusCodes -contains $statusCode -and
                $statusPath.IndexOf(' -> ', [System.StringComparison]::Ordinal) -lt 0 -and
                -not $statusPath.StartsWith('"')) {
                $isGeneratedOutput = $statusPath.Equals(
                    $manifestPath,
                    [System.StringComparison]::Ordinal) -or
                    [regex]::IsMatch($statusPath, $temporaryPattern)
            }
        }

        if (-not $isGeneratedOutput) {
            $inputStatus += $line
        }
    }

    return @($inputStatus)
}

function Get-LmcReleaseManifestWorktreeState {
    [CmdletBinding()]
    param(
        [AllowNull()]
        [object]$GitStatus,
        [switch]$AllowDirty
    )

    $changes = @(
        @($GitStatus) |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }
    )
    if ($changes.Count -eq 0) {
        return 'clean'
    }

    if (-not $AllowDirty) {
        throw 'The worktree is dirty. Commit release inputs or use -AllowDirty for a preview build.'
    }

    return 'dirty-preview'
}

function Get-LmcReleaseManifestRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot
    )

    if (-not (Test-Path -LiteralPath $DistributionRoot -PathType Container)) {
        throw "Distribution root not found: $DistributionRoot"
    }

    return [System.IO.Path]::GetFullPath($DistributionRoot).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
}

function Resolve-LmcReleaseManifestFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ([System.IO.Path]::IsPathRooted($RelativePath) -or
        $RelativePath.IndexOfAny([char[]]@('|', '`', "`r", "`n")) -ge 0) {
        throw "Artifact path must be a safe relative path: $RelativePath"
    }

    $canonicalRelative = $RelativePath.Replace('\', '/')
    Assert-LmcReleaseManifestSafeText `
        -Text $canonicalRelative `
        -Context 'artifact relative path'

    $root = Get-LmcReleaseManifestRoot -DistributionRoot $DistributionRoot
    $candidate = [System.IO.Path]::GetFullPath(
        (Join-Path $root $canonicalRelative.Replace('/', '\')))
    $prefix = $root + [System.IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith(
        $prefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Artifact path escapes the distribution root: $RelativePath"
    }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Distribution artifact not found: $canonicalRelative"
    }

    return $candidate
}

function Get-LmcReleaseManifestArtifactRecords {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot
    )

    $root = Get-LmcReleaseManifestRoot -DistributionRoot $DistributionRoot
    $prefix = $root + [System.IO.Path]::DirectorySeparatorChar
    $records = @()
    foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File -Force) {
        $fullPath = [System.IO.Path]::GetFullPath($file.FullName)
        if (-not $fullPath.StartsWith(
            $prefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Distribution enumeration escaped its root: $fullPath"
        }

        $relativePath = $fullPath.Substring($prefix.Length).Replace('\', '/')
        if ($relativePath.Equals(
            'RELEASE_MANIFEST.md',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }
        if ($relativePath -match '(^|/)\.RELEASE_MANIFEST\.md\..+\.(tmp|bak)$') {
            throw "Stale release-manifest temporary file found: $relativePath"
        }

        Assert-LmcReleaseManifestSafeText `
            -Text $relativePath `
            -Context 'artifact relative path'
        $records += [pscustomobject]@{
            RelativePath = $relativePath
            Size = [Int64]$file.Length
            Sha256 = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToUpperInvariant()
        }
    }

    if ($records.Count -eq 0) {
        throw 'The distribution does not contain any manifestable artifacts.'
    }

    $recordByPath = New-Object `
        'System.Collections.Generic.Dictionary[string,object]' `
        ([System.StringComparer]::Ordinal)
    foreach ($record in $records) {
        $recordByPath.Add($record.RelativePath, $record)
    }
    [string[]]$ordinalPaths = @($recordByPath.Keys)
    [System.Array]::Sort(
        $ordinalPaths,
        [System.StringComparer]::Ordinal)
    return @($ordinalPaths | ForEach-Object { $recordByPath[$_] })
}

function Get-LmcReleaseManifestContent {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalDllPath,
        [Parameter(Mandatory = $true)]
        [string[]]$DllReplicaRelativePaths,
        [Parameter(Mandatory = $true)]
        [string]$SourceCommit,
        [Parameter(Mandatory = $true)]
        [ValidateSet('clean', 'dirty-preview')]
        [string]$WorktreeState,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyVersion,
        [Parameter(Mandatory = $true)]
        [string]$FileVersion,
        [Parameter(Mandatory = $true)]
        [string]$ProductVersion,
        [Parameter(Mandatory = $true)]
        [string]$InputTreeSha256,
        [Parameter(Mandatory = $true)]
        [string]$SemanticPolicySha256,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$SemanticPolicyResult,
        [Parameter(Mandatory = $true)]
        [string]$ToolchainSha256,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolchainRecords,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$ToolingPreflightResult,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightSuiteCount,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightRunCount,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightDigest,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightFileCount,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolingPreflightHostRecords,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightSha256
    )

    if ($SourceCommit -notmatch '^[0-9a-fA-F]{40}$') {
        throw 'Source commit must be an exact 40-character Git object id.'
    }
    if ($InputTreeSha256 -notmatch '^[0-9a-fA-F]{64}$') {
        throw 'Release input tree SHA-256 must be an exact 64-character hexadecimal value.'
    }
    if ($SemanticPolicySha256 -notmatch '^[0-9a-fA-F]{64}$') {
        throw 'Semantic policy SHA-256 must be an exact 64-character hexadecimal value.'
    }
    if (-not $SemanticPolicyResult.Equals(
        'PASS',
        [System.StringComparison]::Ordinal)) {
        throw 'Semantic policy result must be exactly PASS.'
    }
    $validatedToolchainRecords = @(
        Assert-LmcDistributionToolchainManifestBinding `
            -Records $ToolchainRecords `
            -Sha256 $ToolchainSha256)
    $validatedPreflight = `
        Assert-LmcDistributionToolingPreflightManifestBinding `
            -Result $ToolingPreflightResult `
            -SuiteCount $ToolingPreflightSuiteCount `
            -RunCount $ToolingPreflightRunCount `
            -ToolingDigest $ToolingPreflightDigest `
            -HostRecords $ToolingPreflightHostRecords `
            -Sha256 $ToolingPreflightSha256 `
            -ToolingFileCount $ToolingPreflightFileCount
    foreach ($version in @($AssemblyVersion, $FileVersion, $ProductVersion)) {
        if ($version -notmatch '^[0-9A-Za-z][0-9A-Za-z.+-]*$') {
            throw "Release version contains an unsupported value: $version"
        }
    }
    if (-not (Test-Path -LiteralPath $CanonicalDllPath -PathType Leaf)) {
        throw 'Canonical release DLL was not found.'
    }
    if ($DllReplicaRelativePaths.Count -ne 2) {
        throw 'Exactly two package DLL paths are required for the three-replica identity check.'
    }

    $canonicalDllFullPath = [System.IO.Path]::GetFullPath($CanonicalDllPath)
    $dllPaths = @($canonicalDllFullPath)
    $canonicalDllHash = (
        Get-FileHash -LiteralPath $canonicalDllFullPath -Algorithm SHA256).Hash.ToUpperInvariant()
    $dllHashes = @($canonicalDllHash)
    foreach ($relativePath in $DllReplicaRelativePaths) {
        $replicaPath = Resolve-LmcReleaseManifestFile `
            -DistributionRoot $DistributionRoot `
            -RelativePath $relativePath
        $dllPaths += $replicaPath
        $dllHashes += (
            Get-FileHash -LiteralPath $replicaPath -Algorithm SHA256).Hash.ToUpperInvariant()
    }
    if (@($dllPaths | ForEach-Object {
        [System.IO.Path]::GetFullPath($_).ToUpperInvariant()
    } | Select-Object -Unique).Count -ne 3) {
        throw 'The DLL identity check requires three distinct replica files.'
    }
    if (@($dllHashes | Select-Object -Unique).Count -ne 1) {
        throw 'Canonical, API-package, and example-runtime DLL replicas are not byte-identical.'
    }

    $records = @(Get-LmcReleaseManifestArtifactRecords `
        -DistributionRoot $DistributionRoot)
    $newLine = [Environment]::NewLine
    $lines = @(
        '# LASAL Motion Control API Release Manifest',
        '',
        '- Manifest schema: `3`',
        "- Source commit: ``$($SourceCommit.ToLowerInvariant())``",
        "- Worktree state: ``$WorktreeState``",
        "- Release input tree SHA-256: ``$($InputTreeSha256.ToUpperInvariant())``",
        "- Release toolchain SHA-256: ``$($ToolchainSha256.ToUpperInvariant())``",
        "- Tooling preflight result: ``$($validatedPreflight.Result)``",
        "- Tooling preflight suite count: ``$($validatedPreflight.SuiteCount)``",
        "- Tooling preflight run count: ``$($validatedPreflight.RunCount)``",
        "- Tooling preflight digest: ``$($validatedPreflight.ToolingDigest)``",
        "- Tooling preflight file count: ``$($validatedPreflight.ToolingFileCount)``",
        "- Tooling preflight attestation SHA-256: ``$($validatedPreflight.Sha256)``",
        "- Semantic policy SHA-256: ``$($SemanticPolicySha256.ToUpperInvariant())``",
        "- Semantic policy result: ``$SemanticPolicyResult``",
        '- Configuration: `Release`',
        "- Assembly version: ``$AssemblyVersion``",
        "- File version: ``$FileVersion``",
        "- Product version: ``$ProductVersion``",
        '- DLL replica count: `3`',
        '- DLL replica identity: `PASS`',
        "- Canonical DLL SHA-256: ``$canonicalDllHash``",
        '',
        '> `dirty-preview` identifies an uncommitted integration build and is not a production approval.',
        '',
        '## Tooling preflight hosts',
        '',
        '| Host | Edition | Major | Version | Executable SHA-256 |',
        '|---|---|---:|---|---|'
    )
    foreach ($hostRecord in $validatedPreflight.HostRecords) {
        $hostParts = @($hostRecord -split '\|')
        $lines += "| ``$($hostParts[0])`` | ``$($hostParts[1])`` | $($hostParts[2]) | ``$($hostParts[3])`` | ``$($hostParts[4])`` |"
    }
    $lines += @(
        '',
        '## Release toolchain',
        '',
        '| Logical role | Version | Identity SHA-256 |',
        '|---|---|---|'
    )
    foreach ($toolchainRecord in $validatedToolchainRecords) {
        $toolchainParts = @($toolchainRecord -split '\|')
        $lines += "| ``$($toolchainParts[0])`` | ``$($toolchainParts[1])`` | ``$($toolchainParts[2])`` |"
    }
    $lines += @(
        '',
        '## Artifacts',
        '',
        'Every shipped file except this manifest is listed with a package-relative path.',
        '',
        '| Relative path | Bytes | SHA-256 |',
        '|---|---:|---|'
    )
    foreach ($record in $records) {
        $lines += "| ``$($record.RelativePath)`` | $($record.Size) | ``$($record.Sha256)`` |"
    }
    $content = ($lines -join $newLine) + $newLine
    Assert-LmcReleaseManifestSafeText -Text $content
    return $content
}

function Test-LmcReleaseManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalDllPath,
        [Parameter(Mandatory = $true)]
        [string[]]$DllReplicaRelativePaths,
        [Parameter(Mandatory = $true)]
        [string]$SourceCommit,
        [Parameter(Mandatory = $true)]
        [ValidateSet('clean', 'dirty-preview')]
        [string]$WorktreeState,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyVersion,
        [Parameter(Mandatory = $true)]
        [string]$FileVersion,
        [Parameter(Mandatory = $true)]
        [string]$ProductVersion,
        [Parameter(Mandatory = $true)]
        [string]$InputTreeSha256,
        [Parameter(Mandatory = $true)]
        [string]$SemanticPolicySha256,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$SemanticPolicyResult,
        [Parameter(Mandatory = $true)]
        [string]$ToolchainSha256,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolchainRecords,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$ToolingPreflightResult,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightSuiteCount,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightRunCount,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightDigest,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightFileCount,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolingPreflightHostRecords,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightSha256
    )

    $root = Get-LmcReleaseManifestRoot -DistributionRoot $DistributionRoot
    $manifestPath = Join-Path $root 'RELEASE_MANIFEST.md'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw 'RELEASE_MANIFEST.md was not generated.'
    }

    $bytes = [System.IO.File]::ReadAllBytes($manifestPath)
    if ($bytes.Length -ge 3 -and
        $bytes[0] -eq 0xEF -and
        $bytes[1] -eq 0xBB -and
        $bytes[2] -eq 0xBF) {
        throw 'RELEASE_MANIFEST.md must be UTF-8 without a BOM.'
    }
    $actual = [System.IO.File]::ReadAllText($manifestPath)
    Assert-LmcReleaseManifestSafeText -Text $actual
    $expected = Get-LmcReleaseManifestContent @PSBoundParameters
    if (-not $actual.Equals($expected, [System.StringComparison]::Ordinal)) {
        throw 'RELEASE_MANIFEST.md does not match the current package artifacts and release metadata.'
    }

    return $true
}

function Write-LmcReleaseManifestAtomic {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalDllPath,
        [Parameter(Mandatory = $true)]
        [string[]]$DllReplicaRelativePaths,
        [Parameter(Mandatory = $true)]
        [string]$SourceCommit,
        [Parameter(Mandatory = $true)]
        [ValidateSet('clean', 'dirty-preview')]
        [string]$WorktreeState,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyVersion,
        [Parameter(Mandatory = $true)]
        [string]$FileVersion,
        [Parameter(Mandatory = $true)]
        [string]$ProductVersion,
        [Parameter(Mandatory = $true)]
        [string]$InputTreeSha256,
        [Parameter(Mandatory = $true)]
        [string]$SemanticPolicySha256,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$SemanticPolicyResult,
        [Parameter(Mandatory = $true)]
        [string]$ToolchainSha256,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolchainRecords,
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$ToolingPreflightResult,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightSuiteCount,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightRunCount,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightDigest,
        [Parameter(Mandatory = $true)]
        [int]$ToolingPreflightFileCount,
        [Parameter(Mandatory = $true)]
        [string[]]$ToolingPreflightHostRecords,
        [Parameter(Mandatory = $true)]
        [string]$ToolingPreflightSha256
    )

    $root = Get-LmcReleaseManifestRoot -DistributionRoot $DistributionRoot
    $manifestPath = Join-Path $root 'RELEASE_MANIFEST.md'
    $temporaryPath = Join-Path $root (
        '.RELEASE_MANIFEST.md.' + [Guid]::NewGuid().ToString('N') + '.tmp')
    $backupPath = Join-Path $root (
        '.RELEASE_MANIFEST.md.' + [Guid]::NewGuid().ToString('N') + '.bak')
    $content = Get-LmcReleaseManifestContent @PSBoundParameters
    $encoding = New-Object System.Text.UTF8Encoding($false)

    try {
        [System.IO.File]::WriteAllText($temporaryPath, $content, $encoding)
        if (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
            [System.IO.File]::Replace(
                $temporaryPath,
                $manifestPath,
                $backupPath,
                $true)
        }
        else {
            [System.IO.File]::Move($temporaryPath, $manifestPath)
        }
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
        if (Test-Path -LiteralPath $backupPath) {
            Remove-Item -LiteralPath $backupPath -Force
        }
    }

    Test-LmcReleaseManifest @PSBoundParameters | Out-Null
    return $manifestPath
}

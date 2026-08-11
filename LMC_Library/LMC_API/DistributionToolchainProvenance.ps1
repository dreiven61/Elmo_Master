Set-StrictMode -Version Latest

function Get-LmcDistributionProvenanceOrdinalStrings {
    param(
        [AllowNull()]
        [object]$Values,
        [switch]$IgnoreCaseForUniqueness
    )

    $comparer = if ($IgnoreCaseForUniqueness) {
        [System.StringComparer]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparer]::Ordinal
    }
    $set = New-Object 'System.Collections.Generic.HashSet[string]' $comparer
    foreach ($value in @($Values)) {
        $text = [string]$value
        if (-not $set.Add($text)) {
            throw "Distribution provenance value is duplicated: $text"
        }
    }
    [string[]]$sorted = @($set)
    [System.Array]::Sort($sorted, [System.StringComparer]::Ordinal)
    return @($sorted)
}

function Get-LmcDistributionProvenanceTextSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $algorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
            $algorithm.ComputeHash(
                [System.Text.Encoding]::UTF8.GetBytes($Text)))).
            Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
}

function Assert-LmcDistributionProvenanceSafeToken {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    if ($Value -notmatch '^[0-9A-Za-z][0-9A-Za-z._+-]{0,127}$') {
        throw "$Context is malformed or contains a path: $Value"
    }
    if ($Value -match '(?i)([A-Z]:[\\/]|\\\\|/(Users|home|work|git|tmp|var|mnt|opt)/)') {
        throw "$Context contains an absolute path."
    }
}

function Resolve-LmcDistributionProvenancePhysicalFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Context,
        [switch]$AllowEmpty
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or
        -not [System.IO.Path]::IsPathRooted($Path)) {
        throw "$Context path must be absolute."
    }
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "$Context file was not found: $fullPath"
    }
    $leaf = Get-Item -LiteralPath $fullPath -Force
    if (-not $AllowEmpty -and $leaf.Length -le 0) {
        throw "$Context file is empty: $fullPath"
    }

    $current = $fullPath
    while (-not [string]::IsNullOrWhiteSpace($current)) {
        $item = Get-Item -LiteralPath $current -Force
        if (($item.Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Context path contains a reparse point: $current"
        }
        $parent = [System.IO.Path]::GetDirectoryName($current)
        if ([string]::IsNullOrWhiteSpace($parent) -or
            $parent.Equals(
                $current,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        $current = $parent
    }
    return $fullPath
}

function Resolve-LmcDistributionProvenancePhysicalDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or
        -not [System.IO.Path]::IsPathRooted($Path)) {
        throw "$Context path must be absolute."
    }
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not (Test-Path -LiteralPath $fullPath -PathType Container)) {
        throw "$Context directory was not found: $fullPath"
    }
    $current = $fullPath
    while (-not [string]::IsNullOrWhiteSpace($current)) {
        $item = Get-Item -LiteralPath $current -Force
        if (($item.Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Context path contains a reparse point: $current"
        }
        $parent = [System.IO.Path]::GetDirectoryName($current)
        if ([string]::IsNullOrWhiteSpace($parent) -or
            $parent.Equals(
                $current,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        $current = $parent
    }
    return $fullPath
}

function Get-LmcDistributionPhysicalInventoryFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [string]$Context,
        [string[]]$ExcludedRelativePrefixes = @()
    )

    $physicalRoot = Resolve-LmcDistributionProvenancePhysicalDirectory `
        -Path $Root `
        -Context "$Context root"
    $rootPrefix = $physicalRoot + '\'
    $excluded = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($rawPrefix in @($ExcludedRelativePrefixes)) {
        $prefix = ([string]$rawPrefix).Replace('\', '/').Trim('/')
        if ([string]::IsNullOrWhiteSpace($prefix) -or
            [System.IO.Path]::IsPathRooted($prefix) -or
            $prefix -match '(^|/)\.\.(/|$)' -or
            $prefix.IndexOfAny([char[]]@('|', "`r", "`n")) -ge 0 -or
            -not $excluded.Add($prefix)) {
            throw "$Context excluded inventory prefix is malformed or duplicated: $prefix"
        }
    }

    $relativeFiles = New-Object 'System.Collections.Generic.List[string]'
    $pendingDirectories = New-Object `
        'System.Collections.Generic.Stack[string]'
    $pendingDirectories.Push($physicalRoot)
    while ($pendingDirectories.Count -gt 0) {
        $directory = $pendingDirectories.Pop()
        foreach ($entry in @(Get-ChildItem `
                -LiteralPath $directory `
                -Force)) {
            $entryPath = [System.IO.Path]::GetFullPath($entry.FullName)
            if (-not $entryPath.StartsWith(
                    $rootPrefix,
                    [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "$Context inventory entry escaped its root."
            }
            $relative = $entryPath.Substring(
                $physicalRoot.Length + 1).Replace('\', '/')
            if (($entry.Attributes -band
                    [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "$Context inventory contains a reparse point: $relative"
            }
            $isExcluded = $false
            foreach ($excludedPrefix in $excluded) {
                if ($relative.Equals(
                        $excludedPrefix,
                        [System.StringComparison]::OrdinalIgnoreCase) -or
                    $relative.StartsWith(
                        $excludedPrefix + '/',
                        [System.StringComparison]::OrdinalIgnoreCase)) {
                    $isExcluded = $true
                    break
                }
            }
            if ($isExcluded) {
                continue
            }
            if ($entry.PSIsContainer) {
                $pendingDirectories.Push($entryPath)
            }
            else {
                $relativeFiles.Add($relative)
            }
        }
    }
    if ($relativeFiles.Count -eq 0) {
        throw "$Context physical inventory is empty."
    }
    return @(Get-LmcDistributionProvenanceOrdinalStrings `
        -Values $relativeFiles)
}

function Get-LmcDistributionInstalledPackageDigest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DistributionRoot,
        [Parameter(Mandatory = $true)]
        [string[]]$RelativeFiles,
        [Parameter(Mandatory = $true)]
        [string]$ImportedModulePath,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $root = Resolve-LmcDistributionProvenancePhysicalDirectory `
        -Path $DistributionRoot `
        -Context "$Context distribution root"
    $rootPrefix = $root + '\'
    $importedModule = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path $ImportedModulePath `
        -Context "$Context imported module"
    if (-not $importedModule.StartsWith(
            $rootPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Context imported module escapes its distribution root."
    }
    if ($RelativeFiles.Count -eq 0) {
        throw "$Context distribution file inventory is empty."
    }
    $records = New-Object 'System.Collections.Generic.List[string]'
    $importedModuleBound = $false
    $relativeSet = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    $validatedDirectories = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    $null = $validatedDirectories.Add($root)
    $algorithm = [System.Security.Cryptography.SHA256]::Create()
    try {
    foreach ($rawRelative in @($RelativeFiles)) {
        $relative = ([string]$rawRelative).Replace('\', '/')
        if ([string]::IsNullOrWhiteSpace($relative) -or
            [System.IO.Path]::IsPathRooted($relative) -or
            $relative -match '(^|/)\.\.(/|$)' -or
            $relative.IndexOfAny([char[]]@('|', "`r", "`n")) -ge 0 -or
            -not $relativeSet.Add($relative)) {
            throw "$Context distribution file inventory is malformed or duplicated: $relative"
        }
        $fullPath = [System.IO.Path]::GetFullPath(
            (Join-Path $root $relative.Replace('/', '\')))
        if (-not $fullPath.StartsWith(
                $rootPrefix,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "$Context distribution file escapes its root: $relative"
        }
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "$Context distribution file was not found: $relative"
        }
        $physical = $fullPath
        $leaf = Get-Item -LiteralPath $physical -Force
        if (($leaf.Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "$Context distribution file path contains a reparse point: $relative"
        }
        $parent = [System.IO.Path]::GetDirectoryName($physical)
        while (-not $parent.Equals(
                $root,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            if (-not $parent.StartsWith(
                    $rootPrefix,
                    [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "$Context distribution file parent escaped its root: $relative"
            }
            if ($validatedDirectories.Add($parent)) {
                $parentItem = Get-Item -LiteralPath $parent -Force
                if (($parentItem.Attributes -band
                        [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                    throw "$Context distribution file path contains a reparse point: $relative"
                }
            }
            $parent = [System.IO.Path]::GetDirectoryName($parent)
        }
        if ($physical.Equals(
                $importedModule,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $importedModuleBound = $true
        }
        $stream = [System.IO.File]::OpenRead($physical)
        try {
            $length = $stream.Length
            $hash = ([System.BitConverter]::ToString(
                $algorithm.ComputeHash($stream))).Replace('-', '')
        }
        finally {
            $stream.Dispose()
        }
        $records.Add("$relative|$length|$hash")
    }
    }
    finally {
        $algorithm.Dispose()
    }
    if (-not $importedModuleBound) {
        throw "$Context imported module is absent from its distribution inventory."
    }
    $records = @(Get-LmcDistributionProvenanceOrdinalStrings `
        -Values $records)
    return [pscustomobject]@{
        FileCount = $records.Count
        Sha256 = Get-LmcDistributionProvenanceTextSha256 `
            -Text (($records -join "`n") + "`n")
    }
}

function ConvertTo-LmcDistributionProcessArgument {
    param([AllowEmptyString()][string]$Value)

    if ($Value.Length -gt 0 -and $Value -notmatch '[\s"]') {
        return $Value
    }
    $builder = New-Object System.Text.StringBuilder
    $null = $builder.Append('"')
    $slashCount = 0
    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq '\') {
            $slashCount++
            continue
        }
        if ($character -eq '"') {
            $null = $builder.Append(('\' * (($slashCount * 2) + 1)))
            $null = $builder.Append('"')
        }
        else {
            if ($slashCount -gt 0) {
                $null = $builder.Append(('\' * $slashCount))
            }
            $null = $builder.Append($character)
        }
        $slashCount = 0
    }
    if ($slashCount -gt 0) {
        $null = $builder.Append(('\' * ($slashCount * 2)))
    }
    $null = $builder.Append('"')
    return $builder.ToString()
}

function Invoke-LmcDistributionToolchainProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,
        [AllowEmptyCollection()]
        [string[]]$Arguments = @(),
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        [ValidateRange(1, 120)]
        [int]$TimeoutSeconds = 30
    )

    $executable = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path $ExecutablePath `
        -Context 'toolchain probe executable'
    $working = [System.IO.Path]::GetFullPath($WorkingDirectory)
    if (-not (Test-Path -LiteralPath $working -PathType Container)) {
        throw "Toolchain probe working directory was not found: $working"
    }
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $executable
    $startInfo.Arguments = (@($Arguments | ForEach-Object {
        ConvertTo-LmcDistributionProcessArgument -Value ([string]$_)
    }) -join ' ')
    $startInfo.WorkingDirectory = $working
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw 'Toolchain probe process did not start.'
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            & "$env:SystemRoot\System32\taskkill.exe" `
                /PID $process.Id /T /F 2>$null | Out-Null
            $null = $process.WaitForExit(10000)
            throw "Toolchain probe timed out after $TimeoutSeconds seconds."
        }
        $process.WaitForExit()
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if ($stdout.Length -gt 1048576 -or $stderr.Length -gt 1048576) {
            throw 'Toolchain probe output exceeded the 1 MiB bound.'
        }
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StandardOutput = $stdout
            StandardError = $stderr
        }
    }
    finally {
        $process.Dispose()
    }
}

function Get-LmcDistributionToolchainProbeLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $result = Invoke-LmcDistributionToolchainProcess `
        -ExecutablePath $ExecutablePath `
        -Arguments $Arguments `
        -WorkingDirectory $WorkingDirectory
    if ($result.ExitCode -ne 0) {
        throw "$Context probe exited nonzero: $($result.ExitCode)"
    }
    if (-not [string]::IsNullOrWhiteSpace($result.StandardError)) {
        throw "$Context probe wrote stderr."
    }
    $lines = @($result.StandardOutput -split "`r?`n" | Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    })
    if ($lines.Count -ne 1) {
        throw "$Context probe must return exactly one non-empty line."
    }
    return ([string]$lines[0]).Trim()
}

function Assert-LmcDistributionToolingPreflightAttestation {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Attestation
    )

    foreach ($propertyName in @(
        'Result', 'HostCount', 'SuiteCount', 'RunCount',
        'ToolingDigest', 'ToolingFileCount', 'Hosts')) {
        if ($Attestation.PSObject.Properties.Name -notcontains $propertyName) {
            throw "Tooling preflight attestation is missing $propertyName."
        }
    }
    if ([string]$Attestation.Result -cne 'PASS' -or
        [int]$Attestation.HostCount -ne 2 -or
        [int]$Attestation.SuiteCount -ne 6 -or
        [int]$Attestation.RunCount -ne 12 -or
        [int]$Attestation.ToolingFileCount -le 0 -or
        [string]$Attestation.ToolingDigest -notmatch '^[0-9A-Fa-f]{64}$') {
        throw 'Tooling preflight attestation is not an exact 12/12 PASS.'
    }
    $hosts = @($Attestation.Hosts)
    if ($hosts.Count -ne 2) {
        throw 'Tooling preflight attestation must contain exactly two hosts.'
    }
    $hostRecords = @()
    foreach ($hostIdentity in $hosts) {
        foreach ($propertyName in @(
                'Label', 'Edition', 'Major', 'Version', 'Path',
                'ExecutableSha256')) {
            if ($hostIdentity.PSObject.Properties.Name -notcontains $propertyName) {
                throw "Tooling preflight host is missing $propertyName."
            }
        }
        $label = [string]$hostIdentity.Label
        $edition = [string]$hostIdentity.Edition
        $major = [int]$hostIdentity.Major
        $version = [string]$hostIdentity.Version
        Assert-LmcDistributionProvenanceSafeToken `
            -Value $version `
            -Context 'tooling preflight host version'
        if (($label -ceq 'PS5' -and
                ($edition -cne 'Desktop' -or $major -ne 5)) -or
            ($label -ceq 'PS7' -and
                ($edition -cne 'Core' -or $major -lt 7)) -or
            ($label -cne 'PS5' -and $label -cne 'PS7')) {
            throw "Tooling preflight host identity is invalid: $label"
        }
        $hostPath = Resolve-LmcDistributionProvenancePhysicalFile `
            -Path ([string]$hostIdentity.Path) `
            -Context "tooling preflight $label host"
        $hostSha256 = (Get-FileHash `
            -LiteralPath $hostPath `
            -Algorithm SHA256).Hash.ToUpperInvariant()
        if ([string]$hostIdentity.ExecutableSha256 -notmatch
                '^[0-9A-Fa-f]{64}$' -or
            -not $hostSha256.Equals(
                [string]$hostIdentity.ExecutableSha256,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Tooling preflight host executable snapshot changed: $label"
        }
        $hostRecords += "$label|$edition|$major|$version|$hostSha256"
    }
    $hostRecords = @(Get-LmcDistributionProvenanceOrdinalStrings `
        -Values $hostRecords)
    if ($hostRecords[0] -notmatch '^PS5\|' -or
        $hostRecords[1] -notmatch '^PS7\|') {
        throw 'Tooling preflight host identities are missing or duplicated.'
    }
    $digest = ([string]$Attestation.ToolingDigest).ToUpperInvariant()
    $canonicalLines = @(
        'Result|PASS',
        'HostCount|2',
        'SuiteCount|6',
        'RunCount|12',
        "ToolingDigest|$digest",
        "ToolingFileCount|$([int]$Attestation.ToolingFileCount)")
    foreach ($hostRecord in $hostRecords) {
        $canonicalLines += "Host|$hostRecord"
    }
    $canonical = ($canonicalLines -join "`n") + "`n"
    return [pscustomobject]@{
        Result = 'PASS'
        HostCount = 2
        SuiteCount = 6
        RunCount = 12
        ToolingDigest = $digest
        ToolingFileCount = [int]$Attestation.ToolingFileCount
        HostRecords = @($hostRecords)
        Sha256 = Get-LmcDistributionProvenanceTextSha256 -Text $canonical
    }
}

function Assert-LmcDistributionToolingPreflightManifestBinding {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('PASS')]
        [string]$Result,
        [Parameter(Mandatory = $true)]
        [int]$RunCount,
        [Parameter(Mandatory = $true)]
        [string]$ToolingDigest,
        [Parameter(Mandatory = $true)]
        [string[]]$HostRecords,
        [Parameter(Mandatory = $true)]
        [string]$Sha256,
        [Parameter(Mandatory = $true)]
        [int]$ToolingFileCount
    )

    $validatedHostRecords = @()
    foreach ($record in @($HostRecords)) {
        $parts = @($record -split '\|')
        if ($parts.Count -ne 5 -or
            $parts[2] -notmatch '^[0-9]+$' -or
            $parts[4] -notmatch '^[0-9A-Fa-f]{64}$') {
            throw "Tooling preflight manifest host record is malformed: $record"
        }
        Assert-LmcDistributionProvenanceSafeToken `
            -Value $parts[3] `
            -Context 'tooling preflight host version'
        $major = [int]$parts[2]
        if (($parts[0] -ceq 'PS5' -and
                ($parts[1] -cne 'Desktop' -or $major -ne 5)) -or
            ($parts[0] -ceq 'PS7' -and
                ($parts[1] -cne 'Core' -or $major -lt 7)) -or
            ($parts[0] -cne 'PS5' -and $parts[0] -cne 'PS7')) {
            throw "Tooling preflight manifest host identity is invalid: $record"
        }
        $validatedHostRecords += (
            "$($parts[0])|$($parts[1])|$major|$($parts[3])|" +
            $parts[4].ToUpperInvariant())
    }
    $validatedHostRecords = @(
        Get-LmcDistributionProvenanceOrdinalStrings `
            -Values $validatedHostRecords)
    if ($validatedHostRecords.Count -ne 2 -or
        $validatedHostRecords[0] -notmatch '^PS5\|' -or
        $validatedHostRecords[1] -notmatch '^PS7\|') {
        throw 'Tooling preflight manifest host identities are missing or duplicated.'
    }
    if ($Result -cne 'PASS' -or $RunCount -ne 12 -or
        $ToolingFileCount -le 0 -or
        $ToolingDigest -notmatch '^[0-9A-Fa-f]{64}$') {
        throw 'Tooling preflight manifest binding is not an exact 12/12 PASS.'
    }
    $digest = $ToolingDigest.ToUpperInvariant()
    $canonicalLines = @(
        'Result|PASS',
        'HostCount|2',
        'SuiteCount|6',
        'RunCount|12',
        "ToolingDigest|$digest",
        "ToolingFileCount|$ToolingFileCount")
    foreach ($hostRecord in $validatedHostRecords) {
        $canonicalLines += "Host|$hostRecord"
    }
    $actualSha256 = Get-LmcDistributionProvenanceTextSha256 `
        -Text (($canonicalLines -join "`n") + "`n")
    if (-not $actualSha256.Equals(
            $Sha256,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Tooling preflight manifest attestation SHA-256 is invalid.'
    }
    return [pscustomobject]@{
        Result = 'PASS'
        HostCount = 2
        SuiteCount = 6
        RunCount = 12
        ToolingDigest = $digest
        ToolingFileCount = $ToolingFileCount
        HostRecords = @($validatedHostRecords)
        Sha256 = $actualSha256
    }
}

function Assert-LmcDistributionInvokingPowerShellHostBound {
    param(
        [Parameter(Mandatory = $true)]
        [object]$ToolingPreflight
    )

    $validated = Assert-LmcDistributionToolingPreflightAttestation `
        -Attestation $ToolingPreflight
    $hostPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path ([System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName) `
        -Context 'invoking release PowerShell host'
    $hostVersion = [string]$PSVersionTable.PSVersion
    Assert-LmcDistributionProvenanceSafeToken `
        -Value $hostVersion `
        -Context 'invoking release PowerShell version'
    $matchingHostCount = 0
    foreach ($attestedHost in @($ToolingPreflight.Hosts)) {
        $attestedHostPath = Resolve-LmcDistributionProvenancePhysicalFile `
            -Path ([string]$attestedHost.Path) `
            -Context 'attested invoking PowerShell host'
        if ($hostPath.Equals(
                $attestedHostPath,
                [System.StringComparison]::OrdinalIgnoreCase) -and
            [string]$attestedHost.Edition -ceq
                [string]$PSVersionTable.PSEdition -and
            [int]$attestedHost.Major -eq
                [int]$PSVersionTable.PSVersion.Major -and
            [string]$attestedHost.Version -ceq $hostVersion) {
            $matchingHostCount++
        }
    }
    if ($matchingHostCount -ne 1) {
        throw 'Invoking release PowerShell host is not exactly bound to the tooling preflight attestation.'
    }
    return $validated
}

function New-LmcDistributionToolchainSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Descriptors,
        [Parameter(Mandatory = $true)]
        [object]$ToolingPreflight
    )

    $expectedRoles = @(
        'CSharpCompiler',
        'Git',
        'MSBuild',
        'PowerShell',
        'PyPdf',
        'Python',
        'PythonDocx',
        'VsWhere')
    if ($Descriptors.Count -ne $expectedRoles.Count) {
        throw "Release toolchain descriptor count must be 8; actual=$($Descriptors.Count)."
    }
    $byRole = @{}
    $records = @()
    $runtimePaths = @{}
    $inventoryFileCounts = @{}
    foreach ($descriptor in @($Descriptors)) {
        foreach ($propertyName in @('Role', 'Version', 'Path')) {
            if ($descriptor.PSObject.Properties.Name -notcontains $propertyName) {
                throw "Release toolchain descriptor is missing $propertyName."
            }
        }
        $role = [string]$descriptor.Role
        if ($expectedRoles -cnotcontains $role) {
            throw "Release toolchain logical role is malformed: $role"
        }
        if ($byRole.ContainsKey($role)) {
            throw "Release toolchain logical role is duplicated: $role"
        }
        $version = [string]$descriptor.Version
        Assert-LmcDistributionProvenanceSafeToken `
            -Value $version `
            -Context "release toolchain $role version"
        $physicalPath = Resolve-LmcDistributionProvenancePhysicalFile `
            -Path ([string]$descriptor.Path) `
            -Context "release toolchain $role"
        $hasDistributionInventory =
            $descriptor.PSObject.Properties.Name -contains
                'DistributionRoot' -or
            $descriptor.PSObject.Properties.Name -contains
                'DistributionFiles'
        if ($hasDistributionInventory) {
            if ($descriptor.PSObject.Properties.Name -notcontains
                    'DistributionRoot' -or
                $descriptor.PSObject.Properties.Name -notcontains
                    'DistributionFiles' -or
                ($role -cne 'CSharpCompiler' -and
                    $role -cne 'Python' -and
                    $role -cne 'PythonDocx' -and
                    $role -cne 'PyPdf')) {
                throw "Release toolchain $role distribution inventory is incomplete or unexpected."
            }
            $packageDigest = Get-LmcDistributionInstalledPackageDigest `
                -DistributionRoot ([string]$descriptor.DistributionRoot) `
                -RelativeFiles ([string[]]@($descriptor.DistributionFiles)) `
                -ImportedModulePath $physicalPath `
                -Context $role
            $hash = $packageDigest.Sha256
            $inventoryFileCounts[$role] = $packageDigest.FileCount
        }
        else {
            $hash = (Get-FileHash `
                -LiteralPath $physicalPath `
                -Algorithm SHA256).Hash.ToUpperInvariant()
            $inventoryFileCounts[$role] = 1
        }
        $record = "$role|$version|$hash"
        $byRole[$role] = $record
        $records += $record
        $runtimePaths[$role] = $physicalPath
    }
    foreach ($role in $expectedRoles) {
        if (-not $byRole.ContainsKey($role)) {
            throw "Release toolchain logical role is missing: $role"
        }
    }
    $records = @(Get-LmcDistributionProvenanceOrdinalStrings `
        -Values $records)
    $canonical = ($records -join "`n") + "`n"
    $preflight = Assert-LmcDistributionToolingPreflightAttestation `
        -Attestation $ToolingPreflight
    return [pscustomobject]@{
        Result = 'PASS'
        RecordCount = $records.Count
        Records = @($records)
        ToolchainSha256 = Get-LmcDistributionProvenanceTextSha256 `
            -Text $canonical
        RuntimePaths = [pscustomobject]$runtimePaths
        InventoryFileCounts = [pscustomobject]$inventoryFileCounts
        ToolingPreflightResult = $preflight.Result
        ToolingPreflightRunCount = $preflight.RunCount
        ToolingPreflightDigest = $preflight.ToolingDigest
        ToolingPreflightFileCount = $preflight.ToolingFileCount
        ToolingPreflightHostRecords = @($preflight.HostRecords)
        ToolingPreflightSha256 = $preflight.Sha256
    }
}

function Assert-LmcDistributionToolchainManifestBinding {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Records,
        [Parameter(Mandatory = $true)]
        [string]$Sha256
    )

    $expectedRoles = @(
        'CSharpCompiler', 'Git', 'MSBuild', 'PowerShell',
        'PyPdf', 'Python', 'PythonDocx', 'VsWhere')
    if ($Records.Count -ne 8) {
        throw 'Release toolchain manifest must contain exactly eight records.'
    }
    $validated = @()
    foreach ($record in @($Records)) {
        $parts = @($record -split '\|')
        if ($parts.Count -ne 3 -or
            $expectedRoles -cnotcontains $parts[0] -or
            $parts[2] -notmatch '^[0-9A-Fa-f]{64}$') {
            throw "Release toolchain manifest record is malformed: $record"
        }
        Assert-LmcDistributionProvenanceSafeToken `
            -Value $parts[1] `
            -Context "release toolchain $($parts[0]) version"
        $validated += "$($parts[0])|$($parts[1])|$($parts[2].ToUpperInvariant())"
    }
    $validated = @(Get-LmcDistributionProvenanceOrdinalStrings `
        -Values $validated)
    $actualRoles = @($validated | ForEach-Object {
        (@($_ -split '\|'))[0]
    })
    foreach ($expectedRole in $expectedRoles) {
        if ($actualRoles -cnotcontains $expectedRole) {
            throw "Release toolchain manifest role is missing: $expectedRole"
        }
    }
    $canonical = ($validated -join "`n") + "`n"
    $actualSha = Get-LmcDistributionProvenanceTextSha256 -Text $canonical
    if (-not $actualSha.Equals(
            $Sha256,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'Release toolchain manifest SHA-256 is invalid.'
    }
    return @($validated)
}

function Get-LmcDistributionApplicationPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $commands = @(Get-Command `
        -Name $Name `
        -CommandType Application `
        -All `
        -ErrorAction SilentlyContinue)
    if ($commands.Count -eq 0) {
        throw "$Name application was not found."
    }
    return Resolve-LmcDistributionProvenancePhysicalFile `
        -Path ([string]$commands[0].Source) `
        -Context $Name
}

function Resolve-LmcDistributionGitDescriptor {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory
    )

    $launcherPath = Get-LmcDistributionApplicationPath -Name 'git'
    $execPathLine = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $launcherPath `
        -Arguments @('--exec-path') `
        -WorkingDirectory $WorkingDirectory `
        -Context 'Git exec-path'
    $execPath = Resolve-LmcDistributionProvenancePhysicalDirectory `
        -Path $execPathLine `
        -Context 'Git exec-path'
    $corePath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path (Join-Path $execPath 'git.exe') `
        -Context 'Git core executable'
    $versionLine = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $corePath `
        -Arguments @('--version') `
        -WorkingDirectory $WorkingDirectory `
        -Context 'Git core'
    $versionMatch = [regex]::Match(
        $versionLine,
        '^git version (?<Version>[0-9A-Za-z][0-9A-Za-z._+-]*)$')
    if (-not $versionMatch.Success) {
        throw 'Git core version evidence is malformed.'
    }
    return [pscustomobject]@{
        Role = 'Git'
        Version = $versionMatch.Groups['Version'].Value
        Path = $corePath
    }
}

function Resolve-LmcDistributionCSharpCompiler {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath,
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory
    )

    $msbuildDirectory = Split-Path -Parent $MSBuildPath
    $configurationPath = "$MSBuildPath.config"
    $configurationPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path $configurationPath `
        -Context 'MSBuild toolset configuration'
    [xml]$configuration = Get-Content -LiteralPath $configurationPath -Raw
    $nodes = @($configuration.SelectNodes(
        '/configuration/msbuildToolsets/toolset/property[@name="RoslynTargetsPath"]'))
    if ($nodes.Count -ne 1) {
        throw 'Selected MSBuild toolset has an ambiguous RoslynTargetsPath.'
    }
    $value = [string]$nodes[0].GetAttribute('value')
    $tools32Prefix = '$([MSBuild]::GetToolsDirectory32())\'
    $toolsPrefix = '$(MSBuildToolsPath)\'
    if ($value.StartsWith(
            $tools32Prefix,
            [System.StringComparison]::Ordinal)) {
        $relative = $value.Substring($tools32Prefix.Length)
    }
    elseif ($value.StartsWith(
            $toolsPrefix,
            [System.StringComparison]::Ordinal)) {
        $relative = $value.Substring($toolsPrefix.Length)
    }
    else {
        throw 'Selected MSBuild RoslynTargetsPath is not toolset-relative.'
    }
    if ([string]::IsNullOrWhiteSpace($relative) -or
        $relative -match '(^|[\\/])\.\.([\\/]|$)' -or
        [System.IO.Path]::IsPathRooted($relative)) {
        throw 'Selected MSBuild RoslynTargetsPath escapes its toolset.'
    }
    $compilerPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path (Join-Path $msbuildDirectory (Join-Path $relative 'csc.exe')) `
        -Context 'selected MSBuild C# compiler'
    $compilerVersionLine = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $compilerPath `
        -Arguments @('-version') `
        -WorkingDirectory $WorkingDirectory `
        -Context 'C# compiler'
    $versionMatch = [regex]::Match(
        $compilerVersionLine,
        '^(?<Version>[0-9A-Za-z][0-9A-Za-z._+-]*)')
    if (-not $versionMatch.Success) {
        throw 'C# compiler version evidence is malformed.'
    }
    $compilerRoot = Split-Path -Parent $compilerPath
    $compilerFiles = @(Get-LmcDistributionPhysicalInventoryFiles `
        -Root $compilerRoot `
        -Context 'selected MSBuild Roslyn toolset')
    return [pscustomobject]@{
        Role = 'CSharpCompiler'
        Version = $versionMatch.Groups['Version'].Value
        Path = $compilerPath
        DistributionRoot = $compilerRoot
        DistributionFiles = [string[]]$compilerFiles
    }
}

function Resolve-LmcDistributionPythonDescriptors {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$CandidatePaths,
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory
    )

    $candidates = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    $orderedCandidates = @()
    foreach ($candidate in @($CandidatePaths)) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }
        try {
            $physical = Resolve-LmcDistributionProvenancePhysicalFile `
                -Path $candidate `
                -Context 'Python candidate'
            if ($candidates.Add($physical)) {
                $orderedCandidates += $physical
            }
        }
        catch {
            continue
        }
    }
    if ($orderedCandidates.Count -eq 0) {
        throw 'No physical Python candidate was found.'
    }
    $probeCode = @'
import base64, importlib.metadata as m, json, sys
import docx, pypdf
e=lambda s:base64.b64encode(str(s).encode("utf-8")).decode("ascii")
d=lambda n:m.distribution(n)
f=lambda x:json.dumps([str(v) for v in x.files],separators=(",",":"))
print("LMC_PY|"+"|".join([e(sys.executable),e(sys.version.split()[0]),e(docx.__version__),e(d("python-docx").version),e(docx.__file__),e(d("python-docx").locate_file("")),e(f(d("python-docx"))),e(pypdf.__version__),e(d("pypdf").version),e(pypdf.__file__),e(d("pypdf").locate_file("")),e(f(d("pypdf"))),e(sys.base_prefix)]))
'@.Trim()
    $rejections = @()
    foreach ($candidate in $orderedCandidates) {
        try {
            $line = Get-LmcDistributionToolchainProbeLine `
                -ExecutablePath $candidate `
                -Arguments @('-c', $probeCode) `
                -WorkingDirectory $WorkingDirectory `
                -Context 'Python'
            $parts = @($line -split '\|')
            if ($parts.Count -ne 14 -or $parts[0] -cne 'LMC_PY') {
                throw 'Python provenance evidence is malformed.'
            }
            $decoded = @()
            for ($index = 1; $index -lt $parts.Count; $index++) {
                try {
                    $decoded += [System.Text.Encoding]::UTF8.GetString(
                        [System.Convert]::FromBase64String($parts[$index]))
                }
                catch {
                    throw 'Python provenance evidence contains invalid base64.'
                }
            }
            $reportedExecutable = [System.IO.Path]::GetFullPath($decoded[0])
            if (-not $reportedExecutable.Equals(
                    $candidate,
                    [System.StringComparison]::OrdinalIgnoreCase)) {
                throw 'Python reported a different executable path.'
            }
            if ($decoded[2] -cne $decoded[3]) {
                throw 'python-docx module and distribution versions differ.'
            }
            if ($decoded[7] -cne $decoded[8]) {
                throw 'pypdf module and distribution versions differ.'
            }
            foreach ($version in @($decoded[1], $decoded[2], $decoded[7])) {
                Assert-LmcDistributionProvenanceSafeToken `
                    -Value $version `
                    -Context 'Python provenance version'
            }
            $docxPath = Resolve-LmcDistributionProvenancePhysicalFile `
                -Path $decoded[4] `
                -Context 'python-docx imported module'
            $docxParsedFiles = ConvertFrom-Json `
                -InputObject $decoded[6]
            $docxFiles = @()
            foreach ($docxParsedFile in $docxParsedFiles) {
                $docxFiles += [string]$docxParsedFile
            }
            $pypdfPath = Resolve-LmcDistributionProvenancePhysicalFile `
                -Path $decoded[9] `
                -Context 'pypdf imported module'
            $pypdfParsedFiles = ConvertFrom-Json `
                -InputObject $decoded[11]
            $pypdfFiles = @()
            foreach ($pypdfParsedFile in $pypdfParsedFiles) {
                $pypdfFiles += [string]$pypdfParsedFile
            }
            $pythonRoot = Resolve-LmcDistributionProvenancePhysicalDirectory `
                -Path $decoded[12] `
                -Context 'Python runtime distribution'
            $pythonFiles = @(Get-LmcDistributionPhysicalInventoryFiles `
                -Root $pythonRoot `
                -Context 'Python runtime distribution' `
                -ExcludedRelativePrefixes @('Lib/site-packages'))
            return @(
                [pscustomobject]@{
                    Role = 'Python'
                    Version = $decoded[1]
                    Path = $candidate
                    DistributionRoot = $pythonRoot
                    DistributionFiles = [string[]]$pythonFiles
                },
                [pscustomobject]@{
                    Role = 'PythonDocx'
                    Version = $decoded[2]
                    Path = $docxPath
                    DistributionRoot = $decoded[5]
                    DistributionFiles = [string[]]$docxFiles
                },
                [pscustomobject]@{
                    Role = 'PyPdf'
                    Version = $decoded[7]
                    Path = $pypdfPath
                    DistributionRoot = $decoded[10]
                    DistributionFiles = [string[]]$pypdfFiles
                })
        }
        catch {
            $rejections += $_.Exception.Message
        }
    }
    throw "A compatible Python provenance candidate was not found: $($rejections -join '; ')"
}

function Get-LmcDistributionReleaseToolchainSnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [object]$ToolingPreflight,
        [string[]]$PythonCandidatePaths
    )

    $root = [System.IO.Path]::GetFullPath($RepositoryRoot)
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        throw "Release toolchain repository was not found: $root"
    }
    $hostPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path ([System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName) `
        -Context 'invoking release PowerShell host'
    $hostVersion = [string]$PSVersionTable.PSVersion
    Assert-LmcDistributionProvenanceSafeToken `
        -Value $hostVersion `
        -Context 'invoking release PowerShell version'

    $null = Assert-LmcDistributionInvokingPowerShellHostBound `
        -ToolingPreflight $ToolingPreflight

    $git = Resolve-LmcDistributionGitDescriptor -WorkingDirectory $root

    $vswherePath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path (Join-Path ${env:ProgramFiles(x86)} `
            'Microsoft Visual Studio\Installer\vswhere.exe') `
        -Context 'vswhere'
    $vswhereVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(
        $vswherePath).ProductVersion
    Assert-LmcDistributionProvenanceSafeToken `
        -Value $vswhereVersion `
        -Context 'vswhere version'
    $msbuildResult = Invoke-LmcDistributionToolchainProcess `
        -ExecutablePath $vswherePath `
        -Arguments @(
            '-latest', '-products', '*',
            '-requires', 'Microsoft.Component.MSBuild',
            '-find', 'MSBuild\**\Bin\MSBuild.exe') `
        -WorkingDirectory $root
    if ($msbuildResult.ExitCode -ne 0) {
        throw "vswhere MSBuild discovery exited nonzero: $($msbuildResult.ExitCode)"
    }
    if (-not [string]::IsNullOrWhiteSpace($msbuildResult.StandardError)) {
        throw 'vswhere MSBuild discovery wrote stderr.'
    }
    $msbuildLines = @($msbuildResult.StandardOutput -split "`r?`n" |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($msbuildLines.Count -ne 1) {
        throw 'vswhere MSBuild discovery is missing or ambiguous.'
    }
    $msbuildPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path ([string]$msbuildLines[0]).Trim() `
        -Context 'MSBuild'
    $msbuildVersion = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $msbuildPath `
        -Arguments @('-version', '-nologo') `
        -WorkingDirectory $root `
        -Context 'MSBuild'
    Assert-LmcDistributionProvenanceSafeToken `
        -Value $msbuildVersion `
        -Context 'MSBuild version'

    $compiler = Resolve-LmcDistributionCSharpCompiler `
        -MSBuildPath $msbuildPath `
        -WorkingDirectory $root

    if ($null -eq $PythonCandidatePaths) {
        $PythonCandidatePaths = @()
        $bundledPython = Join-Path $env:USERPROFILE `
            '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
        if (Test-Path -LiteralPath $bundledPython -PathType Leaf) {
            $PythonCandidatePaths += $bundledPython
        }
        foreach ($command in @(Get-Command `
                -Name 'python' `
                -CommandType Application `
                -All `
                -ErrorAction SilentlyContinue)) {
            $PythonCandidatePaths += [string]$command.Source
        }
    }
    $pythonDescriptors = @(Resolve-LmcDistributionPythonDescriptors `
        -CandidatePaths $PythonCandidatePaths `
        -WorkingDirectory $root)

    $descriptors = @(
        [pscustomobject]@{
            Role = 'PowerShell'
            Version = $hostVersion
            Path = $hostPath
        },
        [pscustomobject]@{
            Role = 'Git'
            Version = $git.Version
            Path = $git.Path
        },
        [pscustomobject]@{
            Role = 'VsWhere'
            Version = $vswhereVersion
            Path = $vswherePath
        },
        [pscustomobject]@{
            Role = 'MSBuild'
            Version = $msbuildVersion
            Path = $msbuildPath
        },
        $compiler) + $pythonDescriptors
    return New-LmcDistributionToolchainSnapshot `
        -Descriptors $descriptors `
        -ToolingPreflight $ToolingPreflight
}

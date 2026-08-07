[CmdletBinding()]
param(
    [ValidateSet(
        'GateA_VendorImported',
        'GateB1_DerivedDeclaration',
        'GateB2_DerivedWired')]
    [string]$Phase,

    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..'),

    [string]$OutputPath,

    [switch]$Capture,

    [switch]$ValidateOnly,

    [switch]$AllowUncommittedToolBootstrap,

    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Utf8NoBom = [Text.UTF8Encoding]::new($false, $true)
$EvidenceRelativeRoot =
    'test/Reports_Lasal/C78_20260807_udp_callback_gate_b'
$TargetRelativeRoot = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
$VerifierRelativePath =
    'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/' +
    'Verify-LasalUdpCallbackContract.ps1'
$ExpectedVerifierCanonicalLfBytes = 409934
$ExpectedVerifierCanonicalLfSha256 =
    'E5211F3D44712ADE1B4CDE5F6AB72729993AEF530152BC36BDD695C81CDFE6FC'

$PhaseContracts = [ordered]@{
    GateA_VendorImported = [ordered]@{
        Sequence = 0
        ExpectedState = 'VendorImported'
        OutputFile = 'gate_a_vendor_imported_baseline.json'
        ParentPhase = $null
        ParentState = $null
        ParentFile = $null
        ProductionApproved = $true
        NeedsRebaseline = $false
    }
    GateB1_DerivedDeclaration = [ordered]@{
        Sequence = 1
        ExpectedState = 'DerivedDeclaration'
        OutputFile = 'gate_b1_derived_declaration_checkpoint.json'
        ParentPhase = 'GateA_VendorImported'
        ParentState = 'VendorImported'
        ParentFile = 'gate_a_vendor_imported_baseline.json'
        ProductionApproved = $false
        NeedsRebaseline = $true
    }
    GateB2_DerivedWired = [ordered]@{
        Sequence = 2
        ExpectedState = 'DerivedWired'
        OutputFile = 'gate_b2_derived_wired_checkpoint.json'
        ParentPhase = 'GateB1_DerivedDeclaration'
        ParentState = 'DerivedDeclaration'
        ParentFile = 'gate_b1_derived_declaration_checkpoint.json'
        ProductionApproved = $false
        NeedsRebaseline = $true
    }
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

    return Get-BytesSha256 -Bytes $Utf8NoBom.GetBytes($Text)
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
        throw "Path resolved outside repository: $Path"
    }
    return $Path.Substring($rootPrefix.Length).Replace('\', '/')
}

function Assert-PathComponentsNoReparse {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$AllowMissingLeaf
    )

    $rootFull = [IO.Path]::GetFullPath($Root).TrimEnd('\')
    $pathFull = [IO.Path]::GetFullPath($Path)
    $rootPrefix = $rootFull + '\'
    if ((-not [string]::Equals(
                $pathFull.TrimEnd('\'),
                $rootFull,
                [StringComparison]::OrdinalIgnoreCase)) -and
        (-not $pathFull.StartsWith(
                $rootPrefix,
                [StringComparison]::OrdinalIgnoreCase))) {
        throw "Path component check escaped the repository: $pathFull"
    }

    $rootItem = Get-Item -LiteralPath $rootFull -Force
    $ancestor = $rootItem
    while ($null -ne $ancestor) {
        if (($ancestor.Attributes -band
                [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Path ancestor cannot be a reparse point: $($ancestor.FullName)"
        }
        $ancestor = $ancestor.Parent
    }
    if ([string]::Equals(
            $pathFull.TrimEnd('\'),
            $rootFull,
            [StringComparison]::OrdinalIgnoreCase)) {
        return $pathFull
    }

    $relative = $pathFull.Substring($rootPrefix.Length)
    $segments = @($relative.Split(
            [char[]]@('\'),
            [StringSplitOptions]::RemoveEmptyEntries))
    $current = $rootFull
    for ($index = 0; $index -lt $segments.Count; $index++) {
        $current = Join-Path $current $segments[$index]
        $isLeaf = $index -eq ($segments.Count - 1)
        if (-not [IO.File]::Exists($current) -and
            -not [IO.Directory]::Exists($current)) {
            if ($AllowMissingLeaf -and $isLeaf) {
                continue
            }
            throw "Path component is missing: $current"
        }
        $item = Get-Item -LiteralPath $current -Force
        if (($item.Attributes -band
                [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Path component cannot be a reparse point: $current"
        }
    }
    return $pathFull
}

function Resolve-ExactEvidenceDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RequestedPath
    )

    $expected = [IO.Path]::GetFullPath((Join-Path $Root (
                $EvidenceRelativeRoot.Replace('/', '\'))))
    if (-not [IO.Directory]::Exists($expected)) {
        throw "Evidence directory is missing: $expected"
    }
    $requested = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        [IO.Path]::GetFullPath($RequestedPath)
    }
    else {
        [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $RequestedPath))
    }
    $null = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $expected
    $expectedResolved = (Resolve-Path -LiteralPath $expected).Path
    $requestedResolved = (Resolve-Path -LiteralPath $requested).Path
    if (-not [string]::Equals(
            $expectedResolved.TrimEnd('\'),
            $requestedResolved.TrimEnd('\'),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw (
            'OutputPath must be the exact Gate B evidence directory: ' +
            $expectedResolved)
    }
    $null = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $requestedResolved
    return $expectedResolved
}

function Assert-LasalClosed {
    $processIds = @(
        Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty Id |
            Sort-Object)
    if ($processIds.Count -ne 0) {
        throw (
            'LASAL must be fully closed; observed Lasal2 PID(s): ' +
            [string]::Join(',', $processIds))
    }
    return @($processIds)
}

function Get-RawTextTraits {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    $isAscii = $true
    $crLf = 0
    $bareLf = 0
    $bareCr = 0
    for ($index = 0; $index -lt $Bytes.Length; $index++) {
        $value = $Bytes[$index]
        if ($value -gt 0x7F) {
            $isAscii = $false
        }
        if ($value -eq 0x0D) {
            if ((($index + 1) -lt $Bytes.Length) -and
                ($Bytes[$index + 1] -eq 0x0A)) {
                $crLf++
                $index++
            }
            else {
                $bareCr++
            }
        }
        elseif ($value -eq 0x0A) {
            $bareLf++
        }
    }

    $bom = if (($Bytes.Length -ge 4) -and
        ($Bytes[0] -eq 0x00) -and ($Bytes[1] -eq 0x00) -and
        ($Bytes[2] -eq 0xFE) -and ($Bytes[3] -eq 0xFF)) {
        'UTF32BE'
    }
    elseif (($Bytes.Length -ge 4) -and
        ($Bytes[0] -eq 0xFF) -and ($Bytes[1] -eq 0xFE) -and
        ($Bytes[2] -eq 0x00) -and ($Bytes[3] -eq 0x00)) {
        'UTF32LE'
    }
    elseif (($Bytes.Length -ge 3) -and
        ($Bytes[0] -eq 0xEF) -and ($Bytes[1] -eq 0xBB) -and
        ($Bytes[2] -eq 0xBF)) {
        'UTF8'
    }
    elseif (($Bytes.Length -ge 2) -and
        ($Bytes[0] -eq 0xFE) -and ($Bytes[1] -eq 0xFF)) {
        'UTF16BE'
    }
    elseif (($Bytes.Length -ge 2) -and
        ($Bytes[0] -eq 0xFF) -and ($Bytes[1] -eq 0xFE)) {
        'UTF16LE'
    }
    else {
        'None'
    }

    $styles = @(
        if ($crLf -gt 0) { 'CRLF' }
        if ($bareLf -gt 0) { 'LF' }
        if ($bareCr -gt 0) { 'CR' })
    $eolStyle = if ($styles.Count -eq 0) {
        'None'
    }
    elseif ($styles.Count -eq 1) {
        $styles[0]
    }
    else {
        'Mixed'
    }

    return [ordered]@{
        is7BitAscii = $isAscii
        bom = $bom
        eolStyle = $eolStyle
        crlfCount = $crLf
        bareLfCount = $bareLf
        bareCrCount = $bareCr
    }
}

function Set-SanitizedChildProcessEnvironment {
    param(
        [Parameter(Mandatory = $true)]
        [Diagnostics.ProcessStartInfo]$StartInfo
    )

    # A caller-controlled Git environment can redirect repository, index,
    # object, ref, shallow, namespace, or injected configuration state even
    # when every command supplies an explicit -C repository root. Remove the
    # complete Git control namespace, then add back only deterministic controls
    # needed by this evidence tool. This applies to Git and verifier children.
    foreach ($name in @($StartInfo.Environment.Keys)) {
        if ([string]$name -and
            ([string]$name).StartsWith(
                'GIT_',
                [StringComparison]::OrdinalIgnoreCase)) {
            $null = $StartInfo.Environment.Remove([string]$name)
        }
    }
    $StartInfo.Environment['GIT_NO_REPLACE_OBJECTS'] = '1'
    $StartInfo.Environment['GIT_PAGER'] = 'cat'
    $StartInfo.Environment['GIT_TERMINAL_PROMPT'] = '0'
    $StartInfo.Environment['NO_COLOR'] = '1'
}

function Invoke-ProcessCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [int]$TimeoutMilliseconds = 600000
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = $Utf8NoBom
    $startInfo.StandardErrorEncoding = $Utf8NoBom
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }
    Set-SanitizedChildProcessEnvironment -StartInfo $startInfo

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    if (-not $process.Start()) {
        throw "Failed to start process: $FileName"
    }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutMilliseconds)) {
        try {
            $process.Kill($true)
        }
        catch {
        }
        throw "Process timed out: $FileName"
    }
    [Threading.Tasks.Task]::WaitAll(@($stdoutTask, $stderrTask))
    $stopwatch.Stop()
    return [pscustomobject]@{
        FileName = $FileName
        Arguments = @($Arguments)
        ExitCode = $process.ExitCode
        Stdout = $stdoutTask.Result
        Stderr = $stderrTask.Result
        DurationMilliseconds = $stopwatch.ElapsedMilliseconds
    }
}

function ConvertTo-CommandEvidence {
    param([Parameter(Mandatory = $true)][pscustomobject]$Result)

    return [ordered]@{
        executable = $Result.FileName
        arguments = @($Result.Arguments)
        exitCode = $Result.ExitCode
        durationMilliseconds = $Result.DurationMilliseconds
        stdout = $Result.Stdout.Replace("`r`n", "`n").Replace("`r", "`n")
        stderr = $Result.Stderr.Replace("`r`n", "`n").Replace("`r", "`n")
    }
}

function Assert-CommandPassed {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Result,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ($Result.ExitCode -ne 0) {
        throw (
            "$Owner failed with exit code $($Result.ExitCode). " +
            "stdout=$($Result.Stdout) stderr=$($Result.Stderr)")
    }
}

function Get-FileMetadata {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not [IO.File]::Exists($Path)) {
        return [ordered]@{ exists = $false }
    }
    $item = Get-Item -LiteralPath $Path -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Capture input cannot be a reparse point: $Path"
    }
    return [ordered]@{
        exists = $true
        length = $item.Length
        lastWriteTimeUtcTicks = $item.LastWriteTimeUtc.Ticks
    }
}

function Read-SingleFileEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.HashSet[string]]$TrackedPaths
    )

    $fullPath = [IO.Path]::GetFullPath((Join-Path $Root (
                $RelativePath.Replace('/', '\'))))
    $null = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $fullPath
    $normalizedRelativePath =
        Get-RepositoryRelativePath -Root $Root -Path $fullPath
    if ($normalizedRelativePath -cne $RelativePath) {
        throw "Noncanonical relative path requested: $RelativePath"
    }
    if (-not [IO.File]::Exists($fullPath)) {
        throw "Required capture input is missing: $RelativePath"
    }
    $metadata = Get-FileMetadata -Path $fullPath
    $bytes = [IO.File]::ReadAllBytes($fullPath)
    $postReadMetadata = Get-FileMetadata -Path $fullPath
    if (($metadata.length -ne $bytes.Length) -or
        ($metadata.length -ne $postReadMetadata.length) -or
        ($metadata.lastWriteTimeUtcTicks -ne
            $postReadMetadata.lastWriteTimeUtcTicks)) {
        throw "Input changed while it was read: $RelativePath"
    }
    $traits = Get-RawTextTraits -Bytes $bytes
    $blobResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'hash-object', "--path=$RelativePath", $fullPath)
    Assert-CommandPassed `
        -Result $blobResult `
        -Owner "filtered capture blob for $RelativePath"
    $filteredGitBlobOid = $blobResult.Stdout.Trim().ToUpperInvariant()
    Assert-GitObjectId `
        -Value $filteredGitBlobOid `
        -Owner "filtered capture blob for $RelativePath"
    return [pscustomobject]@{
        RawBytes = $bytes
        Metadata = $metadata
        Public = [ordered]@{
            path = $RelativePath
            gitTracked = $TrackedPaths.Contains($RelativePath)
            available = $true
            bytes = $bytes.Length
            sha256 = Get-BytesSha256 -Bytes $bytes
            filteredGitBlobOid = $filteredGitBlobOid
            lastWriteTimeUtc = [DateTime]::new(
                $metadata.lastWriteTimeUtcTicks,
                [DateTimeKind]::Utc).ToString('o')
            text = $traits
        }
    }
}

function Assert-AsciiNoBomText {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$File,
        [Parameter(Mandatory = $true)][string]$Owner,
        [switch]$RequireUniformEol
    )

    $traits = $File.Public.text
    if (-not $traits.is7BitAscii) {
        throw "$Owner is not 7-bit ASCII: $($File.Public.path)"
    }
    if ($traits.bom -cne 'None') {
        throw "$Owner has a BOM: $($File.Public.path)"
    }
    if ($RequireUniformEol -and
        ($traits.eolStyle -notin @('LF', 'CRLF'))) {
        throw (
            "$Owner must use one uniform LF or CRLF style: " +
            "$($File.Public.path); observed $($traits.eolStyle)")
    }
}

function Get-AsciiText {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$File,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-AsciiNoBomText `
        -File $File `
        -Owner $Owner `
        -RequireUniformEol
    return [Text.Encoding]::ASCII.GetString($File.RawBytes)
}

function Get-CanonicalAsciiPinEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Owner,
        [Parameter(Mandatory = $true)][int]$ExpectedCanonicalLfBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedCanonicalLfSha256
    )

    $traits = Get-RawTextTraits -Bytes $Bytes
    if ($traits.bom -cne 'None') {
        throw "$Owner has a BOM."
    }
    if (-not $traits.is7BitAscii) {
        throw "$Owner is not 7-bit ASCII."
    }
    if ($traits.eolStyle -notin @('LF', 'CRLF')) {
        throw (
            "$Owner must use one uniform LF or CRLF style; observed " +
            "$($traits.eolStyle).")
    }
    $physicalText = [Text.Encoding]::ASCII.GetString($Bytes)
    $canonicalText =
        $physicalText.Replace("`r`n", "`n").Replace("`r", "`n")
    $canonicalBytes = $Utf8NoBom.GetBytes($canonicalText)
    $canonicalSha256 = Get-BytesSha256 -Bytes $canonicalBytes
    if (($canonicalBytes.Length -ne $ExpectedCanonicalLfBytes) -or
        ($canonicalSha256 -cne $ExpectedCanonicalLfSha256)) {
        throw (
            "$Owner canonical-LF identity drifted; expected " +
            "$ExpectedCanonicalLfBytes/$ExpectedCanonicalLfSha256, observed " +
            "$($canonicalBytes.Length)/$canonicalSha256.")
    }
    return [pscustomobject]@{
        Text = $canonicalText
        Public = [ordered]@{
            policy = 'strict ASCII; no BOM; one uniform LF or CRLF; canonicalize to LF'
            physicalEolStyle = $traits.eolStyle
            canonicalLfBytes = $canonicalBytes.Length
            canonicalLfSha256 = $canonicalSha256
        }
    }
}

function Invoke-CanonicalAsciiPinSelfTest {
    $canonicalText = "alpha`nbeta`n"
    $canonicalBytes = $Utf8NoBom.GetBytes($canonicalText)
    $expectedBytes = $canonicalBytes.Length
    $expectedSha256 = Get-BytesSha256 -Bytes $canonicalBytes
    $positiveFixtures = @(
        [ordered]@{
            Name = 'LF'
            Bytes = $canonicalBytes
        },
        [ordered]@{
            Name = 'CRLF'
            Bytes = $Utf8NoBom.GetBytes(
                $canonicalText.Replace("`n", "`r`n"))
        })
    $accepted = 0
    foreach ($fixture in $positiveFixtures) {
        $result = Get-CanonicalAsciiPinEvidence `
            -Bytes $fixture.Bytes `
            -Owner "Synthetic $($fixture.Name) positive" `
            -ExpectedCanonicalLfBytes $expectedBytes `
            -ExpectedCanonicalLfSha256 $expectedSha256
        if (($result.Public.canonicalLfBytes -ne $expectedBytes) -or
            ($result.Public.canonicalLfSha256 -cne $expectedSha256)) {
            throw "Synthetic $($fixture.Name) positive did not canonicalize exactly."
        }
        $accepted++
    }

    $negativeFixtures = @(
        [ordered]@{
            Name = 'MixedEol'
            Bytes = $Utf8NoBom.GetBytes("alpha`r`nbeta`n")
        },
        [ordered]@{
            Name = 'Utf8Bom'
            Bytes = [byte[]](
                0xEF, 0xBB, 0xBF, 0x61, 0x6C, 0x70, 0x68, 0x61,
                0x0A)
        },
        [ordered]@{
            Name = 'NonAscii'
            Bytes = [byte[]](0x61, 0x80, 0x0A)
        })
    $rejected = 0
    foreach ($fixture in $negativeFixtures) {
        $didReject = $false
        try {
            $null = Get-CanonicalAsciiPinEvidence `
                -Bytes $fixture.Bytes `
                -Owner "Synthetic $($fixture.Name) negative" `
                -ExpectedCanonicalLfBytes $expectedBytes `
                -ExpectedCanonicalLfSha256 $expectedSha256
        }
        catch {
            $didReject = $true
        }
        if (-not $didReject) {
            throw "Synthetic $($fixture.Name) negative was accepted."
        }
        $rejected++
    }
    return [ordered]@{
        acceptedPositiveCount = $accepted
        acceptedPositiveNames = @($positiveFixtures.Name)
        rejectedNegativeCount = $rejected
        rejectedNegativeNames = @($negativeFixtures.Name)
    }
}

function Assert-KnownTextHeader {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$File,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $traits = $File.Public.text
    if ($traits.bom -cne 'None') {
        throw "$Owner has a BOM: $($File.Public.path)"
    }
    if ($traits.eolStyle -notin @('LF', 'CRLF')) {
        throw (
            "$Owner must use one uniform LF or CRLF style: " +
            "$($File.Public.path); observed $($traits.eolStyle)")
    }
}

function Get-AstEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $tokens = $null
    $errors = $null
    $null = [Management.Automation.Language.Parser]::ParseInput(
        $Text, [ref]$tokens, [ref]$errors)
    if ($errors.Count -ne 0) {
        throw (
            "$Owner AST parse failed: " +
            [string]::Join('; ', @($errors | ForEach-Object Message)))
    }
    return [ordered]@{
        owner = $Owner
        parseErrorCount = 0
        tokenCount = $tokens.Count
    }
}

function Get-AvailableRelativeFiles {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RelativeDirectory
    )

    $fullDirectory = Join-Path $Root $RelativeDirectory.Replace('/', '\')
    if (-not [IO.Directory]::Exists($fullDirectory)) {
        throw "Required directory is missing: $RelativeDirectory"
    }
    $rootDirectory = Get-Item -LiteralPath $fullDirectory -Force
    if (($rootDirectory.Attributes -band
            [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Capture directory root cannot be a reparse point: $RelativeDirectory"
    }
    $directories = @(
        Get-ChildItem -LiteralPath $fullDirectory -Directory -Recurse -Force)
    foreach ($directory in $directories) {
        if (($directory.Attributes -band
                [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Capture directory tree contains a reparse point: $($directory.FullName)"
        }
    }
    return @(
        Get-ChildItem -LiteralPath $fullDirectory -File -Recurse -Force |
            ForEach-Object {
                Get-RepositoryRelativePath -Root $Root -Path $_.FullName
            } |
            Sort-Object -Unique)
}

function Get-InventoryEvidence {
    param(
        [Parameter(Mandatory = $true)][string[]]$TrackedPaths,
        [Parameter(Mandatory = $true)][string[]]$AvailablePaths,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, object]]$ReadFiles,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $allPaths = @(@($TrackedPaths + $AvailablePaths) | Sort-Object -Unique)
    if ($allPaths.Count -eq 0) {
        throw "$Owner inventory is empty."
    }
    $files = @(
        foreach ($relativePath in $allPaths) {
            $tracked = $TrackedPaths -contains $relativePath
            $available = $AvailablePaths -contains $relativePath
            if ($tracked -and (-not $available)) {
                throw "$Owner tracked file is missing: $relativePath"
            }
            $public = if ($available) {
                $ReadFiles[$relativePath].Public
            }
            else {
                $null
            }
            [ordered]@{
                path = $relativePath
                gitTracked = $tracked
                available = $available
                bytes = if ($available) { $public.bytes } else { $null }
                sha256 = if ($available) { $public.sha256 } else { $null }
                filteredGitBlobOid = if ($available) {
                    $public.filteredGitBlobOid
                }
                else {
                    $null
                }
                lastWriteTimeUtc = if ($available) {
                    $public.lastWriteTimeUtc
                }
                else {
                    $null
                }
                text = if ($available) { $public.text } else { $null }
            }
        })
    $identity = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}' -f
                    $file.path,
                    ([int][bool]$file.gitTracked),
                    ([int][bool]$file.available),
                    $file.bytes,
                    $file.sha256
            }))
    $trackedIdentity = [string]::Join("`n", @(
            $files |
                Where-Object { $_.gitTracked } |
                Sort-Object path |
                ForEach-Object {
                    "$($_.path)|$($_.bytes)|$($_.sha256)"
                }))
    return [ordered]@{
        trackedCount = @($TrackedPaths).Count
        availableCount = @($AvailablePaths).Count
        unionCount = $allPaths.Count
        inventoryAlgorithm = (
            'sort unique tracked-plus-available relative paths; join ' +
            'path|tracked01|available01|bytes|uppercase-sha256 with LF; ' +
            'UTF-8 SHA-256')
        inventorySha256 = Get-TextSha256 -Text $identity
        trackedInventoryAlgorithm = (
            'sort tracked relative paths; join path|bytes|uppercase-sha256 ' +
            'with LF; UTF-8 SHA-256')
        trackedInventorySha256 = Get-TextSha256 -Text $trackedIdentity
        files = $files
    }
}

function Get-PresenceEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.HashSet[string]]$TrackedPaths,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, object]]$ReadFiles
    )

    if ($ReadFiles.ContainsKey($RelativePath)) {
        return $ReadFiles[$RelativePath].Public
    }
    return [ordered]@{
        path = $RelativePath
        gitTracked = $TrackedPaths.Contains($RelativePath)
        available = $false
        bytes = $null
        sha256 = $null
        filteredGitBlobOid = $null
        lastWriteTimeUtc = $null
        text = $null
    }
}

function Get-RequiredMapValue {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Map,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (-not $Map.Contains($Name)) {
        throw "$Owner is missing required property: $Name"
    }
    return $Map[$Name]
}

function ConvertTo-AsciiLfJsonText {
    param(
        [Parameter(Mandatory = $true)][object]$Value,
        [int]$Depth = 50
    )

    $json = $Value | ConvertTo-Json -Depth $Depth -EscapeHandling EscapeNonAscii
    $json = $json.Replace("`r`n", "`n").Replace("`r", "`n") + "`n"
    foreach ($character in $json.ToCharArray()) {
        if ([int]$character -gt 0x7F) {
            throw 'JSON payload is not 7-bit ASCII.'
        }
    }
    return $json
}

function Get-ManifestSealPlaceholderBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $traits = Get-RawTextTraits -Bytes $Bytes
    if ((-not $traits.is7BitAscii) -or
        ($traits.bom -cne 'None') -or
        ($traits.eolStyle -cne 'LF')) {
        throw "$Owner seal input must be ASCII JSON with LF EOL and no BOM."
    }
    $text = $Utf8NoBom.GetString($Bytes)
    $pattern = '(?m)^(?<Prefix>[ \t]*"sealSha256"[ \t]*:[ \t]*")' +
        '(?<Seal>[A-F0-9]{64})(?<Suffix>"[ \t]*,?[ \t]*)$'
    $matches = @([regex]::Matches(
            $text,
            $pattern,
            [Text.RegularExpressions.RegexOptions]::CultureInvariant))
    if ($matches.Count -ne 1) {
        throw "$Owner must contain exactly one canonical sealSha256 property."
    }
    $sealedSha256 = $matches[0].Groups['Seal'].Value
    $placeholderText = [regex]::Replace(
        $text,
        $pattern,
        { param($match)
            $match.Groups['Prefix'].Value + ('0' * 64) +
                $match.Groups['Suffix'].Value
        },
        1)
    return [pscustomobject]@{
        SealedSha256 = $sealedSha256
        PlaceholderBytes = $Utf8NoBom.GetBytes($placeholderText)
    }
}

function Assert-NoDuplicateJsonPropertyNames {
    param(
        [Parameter(Mandatory = $true)]
        [Text.Json.JsonElement]$Element,
        [Parameter(Mandatory = $true)][string]$Owner,
        [string]$JsonPath = '$'
    )

    if ($Element.ValueKind -eq [Text.Json.JsonValueKind]::Object) {
        $names = [Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
        foreach ($property in $Element.EnumerateObject()) {
            if (-not $names.Add($property.Name)) {
                throw (
                    "$Owner contains duplicate JSON property '$($property.Name)' " +
                    "at $JsonPath.")
            }
            Assert-NoDuplicateJsonPropertyNames `
                -Element $property.Value `
                -Owner $Owner `
                -JsonPath "$JsonPath.$($property.Name)"
        }
    }
    elseif ($Element.ValueKind -eq [Text.Json.JsonValueKind]::Array) {
        $index = 0
        foreach ($item in $Element.EnumerateArray()) {
            Assert-NoDuplicateJsonPropertyNames `
                -Element $item `
                -Owner $Owner `
                -JsonPath "$JsonPath[$index]"
            $index++
        }
    }
}

function Assert-ManifestSealBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $text = $Utf8NoBom.GetString($Bytes)
    $document = $null
    try {
        $document = [Text.Json.JsonDocument]::Parse($text)
        Assert-NoDuplicateJsonPropertyNames `
            -Element $document.RootElement `
            -Owner $Owner
    }
    catch {
        throw "$Owner strict JSON member validation failed: $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $document) {
            $document.Dispose()
        }
    }
    $data = $null
    try {
        $data = $text | ConvertFrom-Json -AsHashtable -Depth 50
    }
    catch {
        throw "$Owner sealed JSON parse failed: $($_.Exception.Message)"
    }
    if ($data -isnot [Collections.IDictionary]) {
        throw "$Owner sealed JSON root is not an object."
    }
    $integrity = Get-RequiredMapValue $data integrity $Owner
    if ($integrity -isnot [Collections.IDictionary]) {
        throw "$Owner integrity property is not an object."
    }
    $sealedBytes = Get-RequiredMapValue $integrity sealedPayloadBytes (
        "$Owner integrity")
    if (($sealedBytes -isnot [int] -and $sealedBytes -isnot [long]) -or
        ([long]$sealedBytes -ne $Bytes.Length) -or
        ((Get-RequiredMapValue $integrity algorithm "$Owner integrity") -cne
            'SHA-256') -or
        ((Get-RequiredMapValue $integrity canonicalization (
                "$Owner integrity")) -cne
            'exact UTF-8 ASCII/LF JSON bytes with sealSha256 set to 64 zeros')) {
        throw "$Owner integrity metadata drifted."
    }
    $placeholder = Get-ManifestSealPlaceholderBytes -Bytes $Bytes -Owner $Owner
    $computed = Get-BytesSha256 -Bytes $placeholder.PlaceholderBytes
    if (($placeholder.SealedSha256 -ceq ('0' * 64)) -or
        ($placeholder.SealedSha256 -cne $computed)) {
        throw (
            "$Owner seal mismatch; recorded=$($placeholder.SealedSha256), " +
            "computed=$computed")
    }
    return [ordered]@{
        valid = $true
        algorithm = 'SHA-256'
        sealedPayloadBytes = $Bytes.Length
        sealSha256 = $computed
    }
}

function ConvertTo-SealedManifestBytes {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Manifest
    )

    $Manifest.integrity = [ordered]@{
        algorithm = 'SHA-256'
        canonicalization =
            'exact UTF-8 ASCII/LF JSON bytes with sealSha256 set to 64 zeros'
        sealedPayloadBytes = 0
        sealSha256 = '0' * 64
    }
    $stableLength = $false
    foreach ($attempt in 1..8) {
        $placeholderText = ConvertTo-AsciiLfJsonText -Value $Manifest -Depth 50
        $placeholderBytes = $Utf8NoBom.GetBytes($placeholderText)
        if ([long]$Manifest.integrity.sealedPayloadBytes -eq
            $placeholderBytes.Length) {
            $stableLength = $true
            break
        }
        $Manifest.integrity.sealedPayloadBytes = $placeholderBytes.Length
    }
    if (-not $stableLength) {
        throw 'Manifest sealedPayloadBytes did not converge.'
    }
    $sealSha256 = Get-BytesSha256 -Bytes $placeholderBytes
    $Manifest.integrity.sealSha256 = $sealSha256
    $finalText = ConvertTo-AsciiLfJsonText -Value $Manifest -Depth 50
    $finalBytes = $Utf8NoBom.GetBytes($finalText)
    if ($finalBytes.Length -ne $placeholderBytes.Length) {
        throw 'Manifest seal replacement changed the payload byte length.'
    }
    $sealEvidence = Assert-ManifestSealBytes `
        -Bytes $finalBytes `
        -Owner 'new checkpoint manifest'
    return [pscustomobject]@{
        Bytes = $finalBytes
        Public = $sealEvidence
    }
}

function ConvertFrom-StrictCheckpointJson {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$File,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $traits = $File.Public.text
    if ((-not $traits.is7BitAscii) -or
        ($traits.bom -cne 'None') -or
        ($traits.eolStyle -cne 'LF')) {
        throw "$Owner must be 7-bit ASCII JSON with no BOM and LF EOL."
    }
    $text = $Utf8NoBom.GetString($File.RawBytes)
    $document = $null
    try {
        $document = [Text.Json.JsonDocument]::Parse($text)
        Assert-NoDuplicateJsonPropertyNames `
            -Element $document.RootElement `
            -Owner $Owner
    }
    catch {
        throw "$Owner JSON parse failed: $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $document) {
            $document.Dispose()
        }
    }
    $sealEvidence = Assert-ManifestSealBytes -Bytes $File.RawBytes -Owner $Owner
    try {
        $data = $text | ConvertFrom-Json -AsHashtable -Depth 50
    }
    catch {
        throw "$Owner JSON object conversion failed: $($_.Exception.Message)"
    }
    return [pscustomobject]@{
        Data = $data
        Seal = $sealEvidence
    }
}

function Assert-ExactMapKeys {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Map,
        [Parameter(Mandatory = $true)][string[]]$Keys,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $expected = @($Keys | Sort-Object -Unique)
    $actual = @($Map.Keys | ForEach-Object { [string]$_ } | Sort-Object -Unique)
    if ([string]::Join("`n", $actual) -cne
        [string]::Join("`n", $expected)) {
        throw (
            "$Owner property set drifted; expected=" +
            [string]::Join(',', $expected) + '; observed=' +
            [string]::Join(',', $actual))
    }
}

function Test-IsJsonInteger {
    param([AllowNull()][object]$Value)

    return ($Value -is [byte]) -or ($Value -is [sbyte]) -or
        ($Value -is [int16]) -or ($Value -is [uint16]) -or
        ($Value -is [int32]) -or ($Value -is [uint32]) -or
        ($Value -is [int64]) -or ($Value -is [uint64])
}

function Assert-UpperSha256 {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($Value -isnot [string]) -or
        ([string]$Value -notmatch '^[A-F0-9]{64}$')) {
        throw "$Owner is not an uppercase SHA-256 value."
    }
}

function Assert-GitObjectId {
    param(
        [AllowNull()][object]$Value,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($Value -isnot [string]) -or
        ([string]$Value -notmatch '^[A-F0-9]{40,64}$')) {
        throw "$Owner is not an uppercase Git object identity."
    }
}

function Assert-TextTraitsEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Traits,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Traits `
        -Keys @(
            'is7BitAscii', 'bom', 'eolStyle', 'crlfCount',
            'bareLfCount', 'bareCrCount') `
        -Owner $Owner
    if (($Traits.is7BitAscii -isnot [bool]) -or
        ($Traits.bom -isnot [string]) -or
        ($Traits.bom -notin @(
                'None', 'UTF8', 'UTF16LE', 'UTF16BE', 'UTF32LE', 'UTF32BE')) -or
        ($Traits.eolStyle -isnot [string]) -or
        ($Traits.eolStyle -notin @('None', 'LF', 'CRLF', 'CR', 'Mixed'))) {
        throw "$Owner text trait values are malformed."
    }
    foreach ($name in @('crlfCount', 'bareLfCount', 'bareCrCount')) {
        $value = $Traits[$name]
        if ((-not (Test-IsJsonInteger -Value $value)) -or ([long]$value -lt 0)) {
            throw "$Owner $name is not a nonnegative integer."
        }
    }
    $styles = @(
        if ([long]$Traits.crlfCount -gt 0) { 'CRLF' }
        if ([long]$Traits.bareLfCount -gt 0) { 'LF' }
        if ([long]$Traits.bareCrCount -gt 0) { 'CR' })
    $computedStyle = if ($styles.Count -eq 0) {
        'None'
    }
    elseif ($styles.Count -eq 1) {
        $styles[0]
    }
    else {
        'Mixed'
    }
    if ($Traits.eolStyle -cne $computedStyle) {
        throw "$Owner EOL trait counts do not reproduce eolStyle."
    }
}

function Assert-PublicFileEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$File,
        [Parameter(Mandatory = $true)][string]$Owner,
        [AllowNull()][string]$ExpectedPath
    )

    Assert-ExactMapKeys `
        -Map $File `
        -Keys @(
            'path', 'gitTracked', 'available', 'bytes', 'sha256',
            'filteredGitBlobOid', 'lastWriteTimeUtc', 'text') `
        -Owner $Owner
    if (($File.path -isnot [string]) -or
        [string]::IsNullOrWhiteSpace([string]$File.path) -or
        ([string]$File.path -match '\\') -or
        (($null -ne $ExpectedPath) -and ($File.path -cne $ExpectedPath)) -or
        ($File.gitTracked -isnot [bool]) -or
        ($File.available -isnot [bool])) {
        throw "$Owner path/presence evidence is malformed."
    }
    if ([bool]$File.available) {
        if ((-not (Test-IsJsonInteger -Value $File.bytes)) -or
            ([long]$File.bytes -lt 0)) {
            throw "$Owner byte count is malformed."
        }
        Assert-UpperSha256 -Value $File.sha256 -Owner "$Owner sha256"
        Assert-GitObjectId `
            -Value $File.filteredGitBlobOid `
            -Owner "$Owner filtered Git blob"
        if ($File.lastWriteTimeUtc -isnot [string]) {
            throw "$Owner lastWriteTimeUtc is missing."
        }
        $parsedTime = [DateTimeOffset]::MinValue
        if (-not [DateTimeOffset]::TryParseExact(
                [string]$File.lastWriteTimeUtc,
                'o',
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::RoundtripKind,
                [ref]$parsedTime)) {
            throw "$Owner lastWriteTimeUtc is not round-trip ISO-8601."
        }
        if ($File.text -isnot [Collections.IDictionary]) {
            throw "$Owner text traits are missing."
        }
        Assert-TextTraitsEvidence -Traits $File.text -Owner "$Owner text"
    }
    elseif (($null -ne $File.bytes) -or ($null -ne $File.sha256) -or
        ($null -ne $File.filteredGitBlobOid) -or
        ($null -ne $File.lastWriteTimeUtc) -or ($null -ne $File.text)) {
        throw "$Owner absent-file evidence contains fabricated content."
    }
}

function Assert-InventoryEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Inventory,
        [Parameter(Mandatory = $true)][string]$Owner,
        [Parameter(Mandatory = $true)][string]$RequiredPathPrefix
    )

    Assert-ExactMapKeys `
        -Map $Inventory `
        -Keys @(
            'trackedCount', 'availableCount', 'unionCount',
            'inventoryAlgorithm', 'inventorySha256',
            'trackedInventoryAlgorithm', 'trackedInventorySha256', 'files') `
        -Owner $Owner
    foreach ($name in @('trackedCount', 'availableCount', 'unionCount')) {
        if ((-not (Test-IsJsonInteger -Value $Inventory[$name])) -or
            ([long]$Inventory[$name] -lt 0)) {
            throw "$Owner $name is not a nonnegative integer."
        }
    }
    if (($Inventory.inventoryAlgorithm -cne
            ('sort unique tracked-plus-available relative paths; join ' +
                'path|tracked01|available01|bytes|uppercase-sha256 with LF; ' +
                'UTF-8 SHA-256')) -or
        ($Inventory.trackedInventoryAlgorithm -cne
            ('sort tracked relative paths; join path|bytes|uppercase-sha256 ' +
                'with LF; UTF-8 SHA-256'))) {
        throw "$Owner inventory algorithm drifted."
    }
    Assert-UpperSha256 `
        -Value $Inventory.inventorySha256 `
        -Owner "$Owner inventorySha256"
    Assert-UpperSha256 `
        -Value $Inventory.trackedInventorySha256 `
        -Owner "$Owner trackedInventorySha256"
    if ($Inventory.files -isnot [Collections.IList]) {
        throw "$Owner files is not an array."
    }
    $files = @($Inventory.files)
    $paths = @()
    foreach ($file in $files) {
        if ($file -isnot [Collections.IDictionary]) {
            throw "$Owner contains a non-object file record."
        }
        Assert-PublicFileEvidence -File $file -Owner "$Owner file"
        if (-not ([string]$file.path).StartsWith(
                $RequiredPathPrefix,
                [StringComparison]::Ordinal)) {
            throw "$Owner path escaped its required prefix: $($file.path)"
        }
        $paths += [string]$file.path
    }
    $sortedPaths = @($paths | Sort-Object -Unique)
    if (($files.Count -ne [long]$Inventory.unionCount) -or
        ([string]::Join("`n", $paths) -cne
            [string]::Join("`n", $sortedPaths)) -or
        (@($files | Where-Object { $_.gitTracked }).Count -ne
            [long]$Inventory.trackedCount) -or
        (@($files | Where-Object { $_.available }).Count -ne
            [long]$Inventory.availableCount)) {
        throw "$Owner counts or canonical path order drifted."
    }
    $identity = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}' -f
                    $file.path,
                    ([int][bool]$file.gitTracked),
                    ([int][bool]$file.available),
                    $file.bytes,
                    $file.sha256
            }))
    $trackedIdentity = [string]::Join("`n", @(
            $files |
                Where-Object { $_.gitTracked } |
                Sort-Object path |
                ForEach-Object {
                    "$($_.path)|$($_.bytes)|$($_.sha256)"
                }))
    if (((Get-TextSha256 -Text $identity) -cne
            $Inventory.inventorySha256) -or
        ((Get-TextSha256 -Text $trackedIdentity) -cne
            $Inventory.trackedInventorySha256)) {
        throw "$Owner inventory digest does not reproduce from its file records."
    }
}

function Assert-TargetWorktreeEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Inventory,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Inventory `
        -Keys @(
            'trackedCount', 'nonIgnoredUntrackedCount', 'unionCount',
            'identityAlgorithm', 'identitySha256', 'files') `
        -Owner $Owner
    foreach ($name in @(
            'trackedCount', 'nonIgnoredUntrackedCount', 'unionCount')) {
        if ((-not (Test-IsJsonInteger -Value $Inventory[$name])) -or
            ([long]$Inventory[$name] -lt 0)) {
            throw "$Owner $name is not a nonnegative integer."
        }
    }
    if ($Inventory.identityAlgorithm -cne
        ('sort unique target tracked-plus-nonignored-untracked paths; ' +
            'join path|tracked01|untracked01|available01|bytes|' +
            'uppercase-sha256|lastWriteTimeUtcTicks with LF; UTF-8 SHA-256')) {
        throw "$Owner identity algorithm drifted."
    }
    Assert-UpperSha256 -Value $Inventory.identitySha256 -Owner "$Owner digest"
    if ($Inventory.files -isnot [Collections.IList]) {
        throw "$Owner files is not an array."
    }
    $files = @($Inventory.files)
    $paths = @()
    foreach ($file in $files) {
        if ($file -isnot [Collections.IDictionary]) {
            throw "$Owner contains a non-object file record."
        }
        Assert-ExactMapKeys `
            -Map $file `
            -Keys @(
                'path', 'gitTracked', 'nonIgnoredUntracked', 'available',
                'bytes', 'sha256', 'lastWriteTimeUtcTicks') `
            -Owner "$Owner file"
        if (($file.path -isnot [string]) -or
            (-not ([string]$file.path).StartsWith(
                    $TargetRelativeRoot + '/',
                    [StringComparison]::Ordinal)) -or
            ($file.gitTracked -isnot [bool]) -or
            ($file.nonIgnoredUntracked -isnot [bool]) -or
            ($file.available -isnot [bool]) -or
            (([bool]$file.gitTracked) -eq
                ([bool]$file.nonIgnoredUntracked))) {
            throw "$Owner file membership is malformed."
        }
        if ([bool]$file.available) {
            if ((-not (Test-IsJsonInteger -Value $file.bytes)) -or
                ([long]$file.bytes -lt 0) -or
                (-not (Test-IsJsonInteger -Value $file.lastWriteTimeUtcTicks)) -or
                ([long]$file.lastWriteTimeUtcTicks -lt 0)) {
                throw "$Owner file byte/time evidence is malformed."
            }
            Assert-UpperSha256 -Value $file.sha256 -Owner "$Owner file sha256"
        }
        elseif (($null -ne $file.bytes) -or ($null -ne $file.sha256) -or
            ($null -ne $file.lastWriteTimeUtcTicks)) {
            throw "$Owner absent file contains fabricated raw evidence."
        }
        $paths += [string]$file.path
    }
    $sortedPaths = @($paths | Sort-Object -Unique)
    if (($files.Count -ne [long]$Inventory.unionCount) -or
        ([string]::Join("`n", $paths) -cne
            [string]::Join("`n", $sortedPaths)) -or
        (@($files | Where-Object { $_.gitTracked }).Count -ne
            [long]$Inventory.trackedCount) -or
        (@($files | Where-Object { $_.nonIgnoredUntracked }).Count -ne
            [long]$Inventory.nonIgnoredUntrackedCount)) {
        throw "$Owner counts or canonical path order drifted."
    }
    $identity = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}|{5}|{6}' -f
                    $file.path,
                    ([int][bool]$file.gitTracked),
                    ([int][bool]$file.nonIgnoredUntracked),
                    ([int][bool]$file.available),
                    $file.bytes,
                    $file.sha256,
                    $file.lastWriteTimeUtcTicks
            }))
    if ((Get-TextSha256 -Text $identity) -cne $Inventory.identitySha256) {
        throw "$Owner identity digest does not reproduce from its file records."
    }
}

function ConvertTo-NulTerminatedEvidenceText {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][object[]]$Entries
    )

    if ($Entries.Count -eq 0) {
        return ''
    }
    foreach ($entry in $Entries) {
        if (($entry -isnot [string]) -or ([string]$entry -match "[`r`n`0]")) {
            throw 'Recorded Git NUL-stream entry is malformed.'
        }
    }
    return [string]::Join([char]0, @($Entries)) + [char]0
}

function Assert-GitSnapshotEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Snapshot,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Snapshot `
        -Keys @(
            'scope', 'repositoryContext', 'head',
            'indexEntryCount', 'indexRawTextSha256', 'indexEntries',
            'trackedPathCount', 'trackedPathRawTextSha256', 'trackedPaths',
            'statusEntryCount', 'statusRawTextSha256', 'statusEntries',
            'targetWorktree') `
        -Owner $Owner
    if ($Snapshot.scope -cne 'full-repository-index-and-status') {
        throw "$Owner Git snapshot scope is not full-repository."
    }
    if ($Snapshot.repositoryContext -isnot [Collections.IDictionary]) {
        throw "$Owner Git repository context is not an object."
    }
    Assert-GitRepositoryContextEvidence `
        -Context $Snapshot.repositoryContext `
        -Owner "$Owner repository context"
    Assert-GitObjectId -Value $Snapshot.head -Owner "$Owner HEAD"
    Assert-UpperSha256 `
        -Value $Snapshot.indexRawTextSha256 `
        -Owner "$Owner index raw digest"
    Assert-UpperSha256 `
        -Value $Snapshot.statusRawTextSha256 `
        -Owner "$Owner status raw digest"
    Assert-UpperSha256 `
        -Value $Snapshot.trackedPathRawTextSha256 `
        -Owner "$Owner tracked path raw digest"
    foreach ($name in @(
            'indexEntryCount', 'trackedPathCount', 'statusEntryCount')) {
        if ((-not (Test-IsJsonInteger -Value $Snapshot[$name])) -or
            ([long]$Snapshot[$name] -lt 0)) {
            throw "$Owner $name is not a nonnegative integer."
        }
    }
    if (($Snapshot.indexEntries -isnot [Collections.IList]) -or
        ($Snapshot.trackedPaths -isnot [Collections.IList]) -or
        ($Snapshot.statusEntries -isnot [Collections.IList])) {
        throw "$Owner Git entry evidence is not array-valued."
    }
    $indexEntries = @($Snapshot.indexEntries)
    $trackedPaths = @($Snapshot.trackedPaths)
    $statusEntries = @($Snapshot.statusEntries)
    if (($indexEntries.Count -ne [long]$Snapshot.indexEntryCount) -or
        ($trackedPaths.Count -ne [long]$Snapshot.trackedPathCount) -or
        ($statusEntries.Count -ne [long]$Snapshot.statusEntryCount) -or
        ((Get-TextSha256 -Text (
                ConvertTo-NulTerminatedEvidenceText -Entries $indexEntries)) -cne
            $Snapshot.indexRawTextSha256) -or
        ((Get-TextSha256 -Text (
                ConvertTo-NulTerminatedEvidenceText -Entries $trackedPaths)) -cne
            $Snapshot.trackedPathRawTextSha256) -or
        ((Get-TextSha256 -Text (
                ConvertTo-NulTerminatedEvidenceText -Entries $statusEntries)) -cne
            $Snapshot.statusRawTextSha256)) {
        throw "$Owner Git NUL-stream digest or count does not reproduce."
    }
    $uniqueTrackedPaths = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    foreach ($path in $trackedPaths) {
        if (($path -isnot [string]) -or ([string]$path -match '\\') -or
            (-not $uniqueTrackedPaths.Add([string]$path))) {
            throw "$Owner tracked path list is malformed or duplicated."
        }
    }
    if ($Snapshot.targetWorktree -isnot [Collections.IDictionary]) {
        throw "$Owner targetWorktree is not an object."
    }
    Assert-TargetWorktreeEvidence `
        -Inventory $Snapshot.targetWorktree `
        -Owner "$Owner targetWorktree"
}

function Invoke-ProcessRawStdout {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [int]$TimeoutMilliseconds = 600000
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardErrorEncoding = $Utf8NoBom
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }
    Set-SanitizedChildProcessEnvironment -StartInfo $startInfo

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $output = [IO.MemoryStream]::new()
    try {
        if (-not $process.Start()) {
            throw "Failed to start process: $FileName"
        }
        $copyTask = $process.StandardOutput.BaseStream.CopyToAsync($output)
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $allTasks = [Threading.Tasks.Task[]]@($copyTask, $stderrTask)
        if (-not [Threading.Tasks.Task]::WaitAll(
                $allTasks,
                $TimeoutMilliseconds)) {
            try {
                $process.Kill($true)
            }
            catch {
            }
            throw "Process timed out: $FileName"
        }
        $process.WaitForExit()
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StdoutBytes = $output.ToArray()
            Stderr = $stderrTask.Result
        }
    }
    finally {
        $output.Dispose()
        $process.Dispose()
    }
}

function ConvertTo-NormalizedAbsolutePathString {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $full = [IO.Path]::GetFullPath($Path)
    if (-not [IO.Path]::IsPathFullyQualified($full)) {
        throw "$Owner is not an absolute path."
    }
    $normalized = $full.Replace('\', '/')
    $pathRoot = ([IO.Path]::GetPathRoot($full)).Replace('\', '/')
    if ([StringComparer]::OrdinalIgnoreCase.Equals($normalized, $pathRoot)) {
        return $pathRoot
    }
    return $normalized.TrimEnd('/')
}

function Get-GitRepositoryContext {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $result = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'rev-parse',
            '--is-inside-work-tree',
            '--path-format=absolute',
            '--show-toplevel',
            '--absolute-git-dir',
            '--git-common-dir',
            '--show-object-format')
    Assert-CommandPassed -Result $result -Owner "$Owner Git repository identity"
    $lines = @($result.Stdout -split "`r?`n" | Where-Object { $_.Length -gt 0 })
    if ($lines.Count -ne 5) {
        throw "$Owner Git repository identity output is malformed."
    }
    if ($lines[0] -cne 'true') {
        throw "$Owner is not inside a Git worktree."
    }
    if ($lines[4] -cne 'sha1') {
        throw "$Owner requires the Git SHA-1 object format."
    }
    $expectedRoot = ConvertTo-NormalizedAbsolutePathString `
        -Path $Root `
        -Owner "$Owner expected worktree root"
    $workTreeRoot = ConvertTo-NormalizedAbsolutePathString `
        -Path $lines[1] `
        -Owner "$Owner reported worktree root"
    if (-not [StringComparer]::OrdinalIgnoreCase.Equals(
            $workTreeRoot,
            $expectedRoot)) {
        throw (
            "$Owner Git worktree root differs from the requested root; " +
            "requested=$expectedRoot; reported=$workTreeRoot")
    }
    $gitDirectory = ConvertTo-NormalizedAbsolutePathString `
        -Path $lines[2] `
        -Owner "$Owner Git directory"
    $gitCommonDirectory = ConvertTo-NormalizedAbsolutePathString `
        -Path $lines[3] `
        -Owner "$Owner Git common directory"
    $legacyInfoGraftsPath = ConvertTo-NormalizedAbsolutePathString `
        -Path (Join-Path $gitCommonDirectory 'info/grafts') `
        -Owner "$Owner legacy info/grafts path"
    if ([IO.File]::Exists($legacyInfoGraftsPath)) {
        throw "$Owner refuses a legacy Git info/grafts file: $legacyInfoGraftsPath"
    }
    return [ordered]@{
        insideWorkTree = $true
        workTreeRoot = $workTreeRoot
        gitDirectory = $gitDirectory
        gitCommonDirectory = $gitCommonDirectory
        objectFormat = 'sha1'
        inheritedGitControlEnvironmentRemoved = $true
        replacementRefsDisabled = $true
        legacyInfoGraftsPath = $legacyInfoGraftsPath
        legacyInfoGraftsAbsent = $true
    }
}

function Assert-GitRepositoryContextEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Context,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Context `
        -Keys @(
            'insideWorkTree', 'workTreeRoot', 'gitDirectory',
            'gitCommonDirectory', 'objectFormat',
            'inheritedGitControlEnvironmentRemoved',
            'replacementRefsDisabled', 'legacyInfoGraftsPath',
            'legacyInfoGraftsAbsent') `
        -Owner $Owner
    foreach ($name in @(
            'insideWorkTree', 'inheritedGitControlEnvironmentRemoved',
            'replacementRefsDisabled', 'legacyInfoGraftsAbsent')) {
        if (($Context[$name] -isnot [bool]) -or (-not [bool]$Context[$name])) {
            throw "$Owner $name is not true."
        }
    }
    if ($Context.objectFormat -cne 'sha1') {
        throw "$Owner Git object format is not sha1."
    }
    foreach ($name in @(
            'workTreeRoot', 'gitDirectory', 'gitCommonDirectory',
            'legacyInfoGraftsPath')) {
        if (($Context[$name] -isnot [string]) -or
            [string]::IsNullOrWhiteSpace([string]$Context[$name]) -or
            (-not [IO.Path]::IsPathFullyQualified([string]$Context[$name])) -or
            ([string]$Context[$name]).Contains('\')) {
            throw "$Owner $name is not a normalized absolute path."
        }
    }
    $expectedGrafts = ConvertTo-NormalizedAbsolutePathString `
        -Path (Join-Path ([string]$Context.gitCommonDirectory) 'info/grafts') `
        -Owner "$Owner expected legacy info/grafts path"
    if (-not [StringComparer]::OrdinalIgnoreCase.Equals(
            [string]$Context.legacyInfoGraftsPath,
            $expectedGrafts)) {
        throw "$Owner legacy info/grafts path is not bound to gitCommonDirectory."
    }
}

function Assert-GitRepositoryContextStable {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Observed,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-GitRepositoryContextEvidence -Context $Expected -Owner "$Owner expected"
    Assert-GitRepositoryContextEvidence -Context $Observed -Owner "$Owner observed"
    Assert-JsonStructuralEquality `
        -Expected $Expected `
        -Observed $Observed `
        -Owner $Owner
}

function Get-GitBlobEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-GitObjectId -Value $Commit -Owner "$Owner commit"
    $oidResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'rev-parse', "$Commit`:$Path")
    Assert-CommandPassed -Result $oidResult -Owner "$Owner blob lookup"
    $oid = $oidResult.Stdout.Trim().ToUpperInvariant()
    Assert-GitObjectId -Value $oid -Owner "$Owner blob"
    $rawResult = Invoke-ProcessRawStdout `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'cat-file', 'blob', "$Commit`:$Path")
    if ($rawResult.ExitCode -ne 0) {
        throw "$Owner raw blob read failed: $($rawResult.Stderr)"
    }
    $header = [Text.Encoding]::ASCII.GetBytes(
        "blob $($rawResult.StdoutBytes.Length)`0")
    $hasher = [Security.Cryptography.IncrementalHash]::CreateHash(
        [Security.Cryptography.HashAlgorithmName]::SHA1)
    try {
        $hasher.AppendData($header)
        $hasher.AppendData($rawResult.StdoutBytes)
        $rawBlobOid = [Convert]::ToHexString($hasher.GetHashAndReset())
    }
    finally {
        $hasher.Dispose()
    }
    if ($rawBlobOid -cne $oid) {
        throw (
            "$Owner raw blob bytes do not hash to the resolved object ID; " +
            "resolved=$oid; raw=$rawBlobOid")
    }
    return [ordered]@{
        blobOid = $oid
        bytes = $rawResult.StdoutBytes.Length
        sha256 = Get-BytesSha256 -Bytes $rawResult.StdoutBytes
        rawBytes = $rawResult.StdoutBytes
    }
}

function Assert-BytesHashTuple {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Tuple,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys -Map $Tuple -Keys @('bytes', 'sha256') -Owner $Owner
    if ((-not (Test-IsJsonInteger -Value $Tuple.bytes)) -or
        ([long]$Tuple.bytes -lt 0)) {
        throw "$Owner bytes is malformed."
    }
    Assert-UpperSha256 -Value $Tuple.sha256 -Owner "$Owner sha256"
}

function Get-InventoryFileEvidenceByPath {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Inventory,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $matches = @($Inventory.files | Where-Object { $_.path -ceq $Path })
    if ($matches.Count -ne 1) {
        throw "$Owner does not contain exactly one path: $Path"
    }
    return $matches[0]
}

function Assert-RecordedVerifierEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Decision,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Artifacts,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $evidence = Get-RequiredMapValue `
        $Decision structuredEvidence "$Owner verifierDecision"
    if ($evidence -isnot [Collections.IDictionary]) {
        throw "$Owner verifier structuredEvidence is not an object."
    }
    Assert-ExactMapKeys `
        -Map $evidence `
        -Keys @(
            'vendor', 'classes', 'project', 'projectDefinition',
            'generatedIncludes', 'tcpSha256', 'network',
            'protectedDependencies') `
        -Owner "$Owner verifier structuredEvidence"
    if (($evidence.vendor -isnot [Collections.IList]) -or
        (@($evidence.vendor).Count -ne 2)) {
        throw "$Owner verifier vendor evidence is malformed."
    }
    $vendor = @($Artifacts.vendorSources)
    for ($index = 0; $index -lt 2; $index++) {
        Assert-BytesHashTuple `
            -Tuple $evidence.vendor[$index] `
            -Owner "$Owner verifier vendor[$index]"
        Assert-PublicFileTupleMatches `
            -Expected $evidence.vendor[$index] `
            -Observed $vendor[$index] `
            -Owner "$Owner verifier vendor[$index]"
    }
    foreach ($mapping in @(
            @('classes', 'classesDatabase'),
            @('project', 'projectDatabase'),
            @('projectDefinition', 'projectDefinition'))) {
        $tuple = $evidence[$mapping[0]]
        if ($tuple -isnot [Collections.IDictionary]) {
            throw "$Owner verifier $($mapping[0]) evidence is malformed."
        }
        Assert-BytesHashTuple -Tuple $tuple -Owner "$Owner verifier $($mapping[0])"
        Assert-PublicFileTupleMatches `
            -Expected $tuple `
            -Observed $Artifacts[$mapping[1]] `
            -Owner "$Owner verifier $($mapping[0])"
    }
    Assert-UpperSha256 `
        -Value $evidence.tcpSha256 `
        -Owner "$Owner verifier TCP SHA-256"
    if ($evidence.tcpSha256 -cne $Artifacts.tcpMotionInterface.sha256) {
        throw "$Owner verifier TCP evidence differs from artifacts."
    }

    $expectedIncludePaths = [ordered]@{
        'C_channels.h' = "$TargetRelativeRoot/Include/C_channels.h"
        'channels.h' = "$TargetRelativeRoot/Include/channels.h"
        'lslpublictypes.h' = "$TargetRelativeRoot/Include/lslpublictypes.h"
    }
    if ($evidence.generatedIncludes -isnot [Collections.IList]) {
        throw "$Owner verifier generatedIncludes is not an array."
    }
    $includeNames = @($evidence.generatedIncludes | ForEach-Object { $_.name })
    if ([string]::Join("`n", @($includeNames | Sort-Object)) -cne
        [string]::Join("`n", @($expectedIncludePaths.Keys | Sort-Object))) {
        throw "$Owner verifier generated Include name set drifted."
    }
    foreach ($item in @($evidence.generatedIncludes)) {
        if ($item -isnot [Collections.IDictionary]) {
            throw "$Owner verifier generated Include item is not an object."
        }
        Assert-ExactMapKeys `
            -Map $item `
            -Keys @('name', 'bytes', 'sha256') `
            -Owner "$Owner verifier generated Include"
        $tuple = [ordered]@{ bytes = $item.bytes; sha256 = $item.sha256 }
        Assert-BytesHashTuple -Tuple $tuple -Owner "$Owner verifier Include tuple"
        $captured = Get-InventoryFileEvidenceByPath `
            -Inventory $Artifacts.generatedIncludes `
            -Path $expectedIncludePaths[$item.name] `
            -Owner "$Owner generated Includes"
        Assert-PublicFileTupleMatches `
            -Expected $item `
            -Observed $captured `
            -Owner "$Owner verifier Include $($item.name)"
    }

    $network = $evidence.network
    if ($network -isnot [Collections.IDictionary]) {
        throw "$Owner verifier Network evidence is not an object."
    }
    Assert-ExactMapKeys `
        -Map $network `
        -Keys @('fullCount', 'fullSha256', 'trackedCount', 'trackedSha256') `
        -Owner "$Owner verifier Network"
    foreach ($name in @('fullCount', 'trackedCount')) {
        if ((-not (Test-IsJsonInteger -Value $network[$name])) -or
            ([long]$network[$name] -lt 0)) {
            throw "$Owner verifier Network $name is malformed."
        }
    }
    Assert-UpperSha256 -Value $network.fullSha256 -Owner "$Owner full Network"
    Assert-UpperSha256 `
        -Value $network.trackedSha256 `
        -Owner "$Owner tracked Network"
    if (([long]$network.fullCount -ne [long]$Artifacts.fullNetwork.unionCount) -or
        ($network.fullSha256 -cne $Artifacts.fullNetwork.inventorySha256) -or
        ([long]$network.trackedCount -ne
            [long]$Artifacts.fullNetwork.trackedCount) -or
        ($network.trackedSha256 -cne
            $Artifacts.fullNetwork.trackedInventorySha256)) {
        throw "$Owner verifier Network evidence differs from artifacts."
    }

    $protectedNames = @('_StdLib', 'CriticalSection', 'lsl_st_tcp_user.h')
    if ($evidence.protectedDependencies -isnot [Collections.IList]) {
        throw "$Owner verifier protectedDependencies is not an array."
    }
    $observedProtectedNames = @(
        $evidence.protectedDependencies | ForEach-Object { $_.name })
    if ([string]::Join("`n", @($observedProtectedNames | Sort-Object)) -cne
        [string]::Join("`n", @($protectedNames | Sort-Object))) {
        throw "$Owner verifier protected dependency name set drifted."
    }
    $protectedFiles = @($Artifacts.protectedDependencies)
    foreach ($item in @($evidence.protectedDependencies)) {
        if ($item -isnot [Collections.IDictionary]) {
            throw "$Owner verifier protected item is not an object."
        }
        Assert-ExactMapKeys `
            -Map $item `
            -Keys @('name', 'bytes', 'sha256') `
            -Owner "$Owner verifier protected item"
        $tuple = [ordered]@{ bytes = $item.bytes; sha256 = $item.sha256 }
        Assert-BytesHashTuple -Tuple $tuple -Owner "$Owner verifier protected tuple"
        $protectedIndex = [Array]::IndexOf($protectedNames, [string]$item.name)
        Assert-PublicFileTupleMatches `
            -Expected $item `
            -Observed $protectedFiles[$protectedIndex] `
            -Owner "$Owner verifier protected $($item.name)"
    }
}

function Assert-CommandEvidenceContract {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Command,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Command `
        -Keys @(
            'executable', 'arguments', 'exitCode', 'durationMilliseconds',
            'stdout', 'stderr') `
        -Owner $Owner
    if (($Command.executable -isnot [string]) -or
        [string]::IsNullOrWhiteSpace([string]$Command.executable) -or
        ($Command.arguments -isnot [Collections.IList]) -or
        (-not (Test-IsJsonInteger -Value $Command.exitCode)) -or
        ([long]$Command.exitCode -ne 0) -or
        (-not (Test-IsJsonInteger -Value $Command.durationMilliseconds)) -or
        ([long]$Command.durationMilliseconds -lt 0) -or
        ($Command.stdout -isnot [string]) -or
        ($Command.stderr -isnot [string])) {
        throw "$Owner command evidence is malformed or did not pass."
    }
    foreach ($argument in @($Command.arguments)) {
        if ($argument -isnot [string]) {
            throw "$Owner contains a non-string command argument."
        }
    }
}

function Assert-ArtifactEvidenceContract {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Artifacts,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Decision,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$TargetWorktree,
        [Parameter(Mandatory = $true)][string]$ExpectedState,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Artifacts `
        -Keys @(
            'classesDatabase', 'projectDatabase', 'projectDefinition',
            'generatedIncludes', 'vendorSources', 'protectedDependencies',
            'tcpMotionInterface', 'derivedSender', 'configObjects',
            'networksDatabase', 'commNetwork', 'commNetworkTable',
            'fullNetwork') `
        -Owner "$Owner artifacts"

    $fixedPaths = [ordered]@{
        classesDatabase = "$TargetRelativeRoot/Class/Classes.lcb"
        projectDatabase = "$TargetRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcb"
        projectDefinition = "$TargetRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcp"
        tcpMotionInterface =
            "$TargetRelativeRoot/Class/TCPMotionInterface/TCPMotionInterface.st"
        derivedSender =
            "$TargetRelativeRoot/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st"
        configObjects = "$TargetRelativeRoot/Network/ConfigObjects.st"
        networksDatabase = "$TargetRelativeRoot/Network/Networks.lcb"
        commNetwork =
            "$TargetRelativeRoot/Network/Comm_Network/Comm_Network.lcn"
        commNetworkTable =
            "$TargetRelativeRoot/Network/Comm_Network/ONE_Comm_Network_Table.st"
    }
    foreach ($name in $fixedPaths.Keys) {
        $file = $Artifacts[$name]
        if ($file -isnot [Collections.IDictionary]) {
            throw "$Owner artifact $name is not an object."
        }
        Assert-PublicFileEvidence `
            -File $file `
            -ExpectedPath $fixedPaths[$name] `
            -Owner "$Owner artifact $name"
    }
    foreach ($name in @(
            'classesDatabase', 'projectDatabase', 'projectDefinition',
            'tcpMotionInterface', 'configObjects', 'networksDatabase',
            'commNetwork', 'commNetworkTable')) {
        if (-not [bool]$Artifacts[$name].available) {
            throw "$Owner required artifact is unavailable: $name"
        }
    }
    $derivedExpected = $ExpectedState -cne 'VendorImported'
    if ([bool]$Artifacts.derivedSender.available -ne $derivedExpected) {
        throw "$Owner derived sender presence does not match phase state."
    }

    $vendorPaths = @(
        "$TargetRelativeRoot/Class/_UDPTransceiver/_UDPTransceiver.st",
        ("$TargetRelativeRoot/Class/_UDPTransceiverInterface/" +
            '_UDPTransceiverInterface.st'))
    $protectedPaths = @(
        "$TargetRelativeRoot/Class/_StdLib/_StdLib.st",
        "$TargetRelativeRoot/Class/CriticalSection/CriticalSection.st",
        "$TargetRelativeRoot/Source/interfaces/lsl_st_tcp_user.h")
    foreach ($listContract in @(
            [ordered]@{
                Name = 'vendorSources'
                Paths = $vendorPaths
            },
            [ordered]@{
                Name = 'protectedDependencies'
                Paths = $protectedPaths
            })) {
        $list = $Artifacts[$listContract.Name]
        if (($list -isnot [Collections.IList]) -or
            (@($list).Count -ne $listContract.Paths.Count)) {
            throw "$Owner artifact $($listContract.Name) list drifted."
        }
        for ($index = 0; $index -lt $listContract.Paths.Count; $index++) {
            $file = @($list)[$index]
            if ($file -isnot [Collections.IDictionary]) {
                throw "$Owner artifact $($listContract.Name) item is not an object."
            }
            Assert-PublicFileEvidence `
                -File $file `
                -ExpectedPath $listContract.Paths[$index] `
                -Owner "$Owner artifact $($listContract.Name)[$index]"
            if (-not [bool]$file.available) {
                throw "$Owner required artifact list item is unavailable."
            }
        }
    }

    if (($Artifacts.generatedIncludes -isnot [Collections.IDictionary]) -or
        ($Artifacts.fullNetwork -isnot [Collections.IDictionary])) {
        throw "$Owner artifact inventories are not objects."
    }
    Assert-InventoryEvidence `
        -Inventory $Artifacts.generatedIncludes `
        -Owner "$Owner generated Includes" `
        -RequiredPathPrefix "$TargetRelativeRoot/Include/"
    Assert-InventoryEvidence `
        -Inventory $Artifacts.fullNetwork `
        -Owner "$Owner full Network" `
        -RequiredPathPrefix "$TargetRelativeRoot/Network/"

    foreach ($name in @(
            'configObjects', 'networksDatabase', 'commNetwork',
            'commNetworkTable')) {
        $networkFile = Get-InventoryFileEvidenceByPath `
            -Inventory $Artifacts.fullNetwork `
            -Path $Artifacts[$name].path `
            -Owner "$Owner full Network"
        Assert-PublicFileTupleMatches `
            -Expected $Artifacts[$name] `
            -Observed $networkFile `
            -Owner "$Owner Network duplicate $name"
    }

    $targetByPath = @{}
    foreach ($targetFile in @($TargetWorktree.files)) {
        $targetByPath[[string]$targetFile.path] = $targetFile
    }
    $artifactFiles = @(
        @($fixedPaths.Keys | ForEach-Object { $Artifacts[$_] }) +
        @($Artifacts.vendorSources) +
        @($Artifacts.protectedDependencies) +
        @($Artifacts.generatedIncludes.files) +
        @($Artifacts.fullNetwork.files))
    foreach ($file in $artifactFiles) {
        if ([bool]$file.available) {
            if (-not $targetByPath.ContainsKey([string]$file.path)) {
                throw "$Owner artifact is absent from full target inventory: $($file.path)"
            }
            Assert-PublicFileTupleMatches `
                -Expected $file `
                -Observed $targetByPath[[string]$file.path] `
                -Owner "$Owner artifact/target inventory $($file.path)"
        }
        elseif ($targetByPath.ContainsKey([string]$file.path) -and
            [bool]$targetByPath[[string]$file.path].available) {
            throw "$Owner absent artifact is present in full target inventory."
        }
    }
    Assert-RecordedVerifierEvidence `
        -Decision $Decision `
        -Artifacts $Artifacts `
        -Owner $Owner
}

function Assert-ToolingEvidenceContract {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Tooling,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$RecordedGit,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Decision,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Artifacts,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-ExactMapKeys `
        -Map $Tooling `
        -Keys @(
            'trust', 'captureScript', 'verifier', 'verifierCanonicalPin',
            'canonicalPinSelfTest', 'ast', 'verifierSelfTest',
            'verifierCurrent', 'verifierCrossCheck', 'diffCheck',
            'cachedDiffCheck') `
        -Owner "$Owner tooling"
    foreach ($name in @('captureScript', 'verifier')) {
        if ($Tooling[$name] -isnot [Collections.IDictionary]) {
            throw "$Owner tooling $name is not an object."
        }
    }
    $capturePath = "$EvidenceRelativeRoot/Capture-UdpCallbackGateBCheckpoint.ps1"
    Assert-PublicFileEvidence `
        -File $Tooling.captureScript `
        -ExpectedPath $capturePath `
        -Owner "$Owner capture script"
    Assert-PublicFileEvidence `
        -File $Tooling.verifier `
        -ExpectedPath $VerifierRelativePath `
        -Owner "$Owner verifier"
    if ((-not [bool]$Tooling.captureScript.available) -or
        (-not [bool]$Tooling.verifier.available)) {
        throw "$Owner tooling files are not both available."
    }

    $pin = $Tooling.verifierCanonicalPin
    if ($pin -isnot [Collections.IDictionary]) {
        throw "$Owner verifier pin is not an object."
    }
    Assert-ExactMapKeys `
        -Map $pin `
        -Keys @(
            'policy', 'physicalEolStyle', 'canonicalLfBytes',
            'canonicalLfSha256', 'pinSource') `
        -Owner "$Owner verifier pin"
    if (($pin.policy -cne
            'strict ASCII; no BOM; one uniform LF or CRLF; canonicalize to LF') -or
        ($pin.physicalEolStyle -notin @('LF', 'CRLF')) -or
        (-not (Test-IsJsonInteger -Value $pin.canonicalLfBytes)) -or
        ([long]$pin.canonicalLfBytes -ne $ExpectedVerifierCanonicalLfBytes) -or
        ($pin.canonicalLfSha256 -cne $ExpectedVerifierCanonicalLfSha256) -or
        ($pin.pinSource -cne 'committed-reviewed-pin')) {
        throw "$Owner verifier canonical pin differs from the reviewed final pin."
    }

    $trust = $Tooling.trust
    if ($trust -isnot [Collections.IDictionary]) {
        throw "$Owner tool trust is not an object."
    }
    Assert-ExactMapKeys `
        -Map $trust `
        -Keys @(
            'trustedCommittedHead', 'startHead', 'mode',
            'workingTreeDiffExitCode', 'indexDiffExitCode', 'pathIdentities') `
        -Owner "$Owner tool trust"
    if (($trust.trustedCommittedHead -isnot [bool]) -or
        (-not [bool]$trust.trustedCommittedHead) -or
        ($trust.mode -cne 'committed-clean') -or
        ($trust.startHead -cne $RecordedGit.head) -or
        (-not (Test-IsJsonInteger -Value $trust.workingTreeDiffExitCode)) -or
        ([long]$trust.workingTreeDiffExitCode -ne 0) -or
        (-not (Test-IsJsonInteger -Value $trust.indexDiffExitCode)) -or
        ([long]$trust.indexDiffExitCode -ne 0) -or
        ($trust.pathIdentities -isnot [Collections.IList]) -or
        (@($trust.pathIdentities).Count -ne 2)) {
        throw "$Owner tool trust is not committed-clean at recorded HEAD."
    }
    $toolFiles = [ordered]@{
        $capturePath = $Tooling.captureScript
        $VerifierRelativePath = $Tooling.verifier
    }
    $identityByPath = @{}
    foreach ($identity in @($trust.pathIdentities)) {
        if ($identity -isnot [Collections.IDictionary]) {
            throw "$Owner tool identity is not an object."
        }
        Assert-ExactMapKeys `
            -Map $identity `
            -Keys @(
                'path', 'committedExact', 'startHead', 'indexMode',
                'indexStage', 'indexFlagTag', 'indexDebugFlags',
                'stageBlobOid', 'headBlobOid', 'filteredWorktreeBlobOid',
                'physicalBytes', 'physicalSha256', 'canonicalLfBytes',
                'canonicalLfSha256', 'headCanonicalLfBytes',
                'headCanonicalLfSha256', 'canonicalHeadMatch') `
            -Owner "$Owner tool identity"
        if (($identity.path -isnot [string]) -or
            (-not $toolFiles.Contains([string]$identity.path)) -or
            $identityByPath.ContainsKey([string]$identity.path)) {
            throw "$Owner tool identity path set drifted."
        }
        $identityByPath[[string]$identity.path] = $identity
        if (($identity.committedExact -isnot [bool]) -or
            (-not [bool]$identity.committedExact) -or
            ($identity.startHead -cne $RecordedGit.head) -or
            ($identity.indexMode -notmatch '^[0-9]{6}$') -or
            (-not (Test-IsJsonInteger -Value $identity.indexStage)) -or
            ([long]$identity.indexStage -ne 0) -or
            ($identity.indexFlagTag -cne 'H') -or
            (-not (Test-IsJsonInteger -Value $identity.indexDebugFlags)) -or
            ([long]$identity.indexDebugFlags -ne 0)) {
            throw "$Owner tool identity does not prove normal stage-0 flags."
        }
        foreach ($name in @(
                'stageBlobOid', 'headBlobOid', 'filteredWorktreeBlobOid')) {
            Assert-GitObjectId `
                -Value $identity[$name] `
                -Owner "$Owner tool $($identity.path) $name"
        }
        if (($identity.stageBlobOid -cne $identity.headBlobOid) -or
            ($identity.filteredWorktreeBlobOid -cne $identity.headBlobOid) -or
            ($identity.filteredWorktreeBlobOid -cne
                $toolFiles[[string]$identity.path].filteredGitBlobOid) -or
            (-not (Test-IsJsonInteger -Value $identity.physicalBytes)) -or
            ([long]$identity.physicalBytes -ne
                [long]$toolFiles[[string]$identity.path].bytes) -or
            ($identity.physicalSha256 -cne
                $toolFiles[[string]$identity.path].sha256) -or
            (-not (Test-IsJsonInteger -Value $identity.canonicalLfBytes)) -or
            (-not (Test-IsJsonInteger -Value $identity.headCanonicalLfBytes)) -or
            ($identity.canonicalHeadMatch -isnot [bool]) -or
            (-not [bool]$identity.canonicalHeadMatch) -or
            ([long]$identity.headCanonicalLfBytes -ne
                [long]$identity.canonicalLfBytes) -or
            ($identity.headCanonicalLfSha256 -cne
                $identity.canonicalLfSha256)) {
            throw "$Owner tool identity raw/blob evidence differs."
        }
        Assert-UpperSha256 `
            -Value $identity.physicalSha256 `
            -Owner "$Owner tool physical SHA-256"
        Assert-UpperSha256 `
            -Value $identity.canonicalLfSha256 `
            -Owner "$Owner tool canonical SHA-256"
        Assert-UpperSha256 `
            -Value $identity.headCanonicalLfSha256 `
            -Owner "$Owner tool HEAD canonical SHA-256"
        $historical = Get-GitBlobEvidence `
            -Root $Root `
            -GitPath $GitPath `
            -Commit $RecordedGit.head `
            -Path ([string]$identity.path) `
            -Owner "$Owner historical tool"
        if ($historical.blobOid -cne $identity.headBlobOid) {
            throw "$Owner historical Git tool blob differs from recorded identity."
        }
        $traits = Get-RawTextTraits -Bytes $historical.rawBytes
        if (($traits.bom -cne 'None') -or (-not $traits.is7BitAscii) -or
            ($traits.eolStyle -notin @('LF', 'CRLF'))) {
            throw "$Owner historical tool is not canonicalizable ASCII text."
        }
        $historicalText = [Text.Encoding]::ASCII.GetString($historical.rawBytes)
        $historicalCanonicalBytes = $Utf8NoBom.GetBytes(
            $historicalText.Replace("`r`n", "`n").Replace("`r", "`n"))
        if (($historicalCanonicalBytes.Length -ne
                [long]$identity.canonicalLfBytes) -or
            ((Get-BytesSha256 -Bytes $historicalCanonicalBytes) -cne
                $identity.canonicalLfSha256)) {
            throw "$Owner historical tool canonical identity differs."
        }
    }
    foreach ($path in $toolFiles.Keys) {
        if (-not $identityByPath.ContainsKey([string]$path)) {
            throw "$Owner tool identity is missing: $path"
        }
    }
    $verifierIdentity = $identityByPath[$VerifierRelativePath]
    if (([long]$verifierIdentity.canonicalLfBytes -ne
            $ExpectedVerifierCanonicalLfBytes) -or
        ($verifierIdentity.canonicalLfSha256 -cne
            $ExpectedVerifierCanonicalLfSha256)) {
        throw "$Owner verifier tool identity differs from the final pin."
    }

    $pinSelfTest = $Tooling.canonicalPinSelfTest
    if ($pinSelfTest -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $pinSelfTest `
            -Keys @(
                'acceptedPositiveCount', 'acceptedPositiveNames',
                'rejectedNegativeCount', 'rejectedNegativeNames') `
            -Owner "$Owner pin self-test"
    }
    if (($pinSelfTest -isnot [Collections.IDictionary]) -or
        ([long](Get-RequiredMapValue $pinSelfTest acceptedPositiveCount (
                "$Owner pin self-test")) -ne 2) -or
        ([long](Get-RequiredMapValue $pinSelfTest rejectedNegativeCount (
                "$Owner pin self-test")) -ne 3) -or
        ($pinSelfTest.acceptedPositiveNames -isnot [Collections.IList]) -or
        ($pinSelfTest.rejectedNegativeNames -isnot [Collections.IList]) -or
        ([string]::Join("`n", @($pinSelfTest.acceptedPositiveNames)) -cne
            "LF`nCRLF") -or
        ([string]::Join("`n", @($pinSelfTest.rejectedNegativeNames)) -cne
            "MixedEol`nUtf8Bom`nNonAscii")) {
        throw "$Owner canonical pin self-test evidence drifted."
    }
    if (($Tooling.ast -isnot [Collections.IList]) -or
        (@($Tooling.ast).Count -ne 2)) {
        throw "$Owner AST evidence is malformed."
    }
    foreach ($ast in @($Tooling.ast)) {
        if (($ast -isnot [Collections.IDictionary]) -or
            ([long](Get-RequiredMapValue $ast parseErrorCount "$Owner AST") -ne 0) -or
            ([long](Get-RequiredMapValue $ast tokenCount "$Owner AST") -le 0) -or
            ((Get-RequiredMapValue $ast owner "$Owner AST") -isnot [string])) {
            throw "$Owner AST evidence did not prove a clean parse."
        }
        Assert-ExactMapKeys `
            -Map $ast `
            -Keys @('owner', 'parseErrorCount', 'tokenCount') `
            -Owner "$Owner AST"
    }
    $astOwners = @($Tooling.ast | ForEach-Object { [string]$_.owner })
    if ([string]::Join("`n", @($astOwners | Sort-Object)) -cne
        [string]::Join("`n", @(
                'Gate B capture script', 'UDP callback verifier' |
                    Sort-Object))) {
        throw "$Owner AST owner set drifted."
    }
    foreach ($name in @(
            'verifierSelfTest', 'verifierCurrent', 'diffCheck',
            'cachedDiffCheck')) {
        if ($Tooling[$name] -isnot [Collections.IDictionary]) {
            throw "$Owner tooling $name is not an object."
        }
        Assert-CommandEvidenceContract `
            -Command $Tooling[$name] `
            -Owner "$Owner $name"
    }
    if ($Tooling.verifierSelfTest.stdout -notmatch
        '(?m)^PASS LASAL\.UdpCallbackContract\.SelfTest ') {
        throw "$Owner verifier self-test PASS evidence is absent."
    }
    $authoritativeLine = Get-RequiredMapValue `
        $Decision authoritativeLine "$Owner verifierDecision"
    $currentLines = @(
        $Tooling.verifierCurrent.stdout.Split("`n") |
            Where-Object { $_ -match 'LASAL\.UdpCallbackContract\.Current' })
    if (($currentLines.Count -ne 1) -or
        ($currentLines[0] -cne $authoritativeLine)) {
        throw "$Owner verifier command output differs from parsed decision."
    }
    $crossCheck = $Tooling.verifierCrossCheck
    if ($crossCheck -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $crossCheck `
            -Keys @(
                'exactRawEvidenceCrossChecked', 'vendorCount',
                'generatedIncludeCount', 'protectedDependencyCount',
                'networkUnionCount', 'note') `
            -Owner "$Owner verifierCrossCheck"
    }
    if (($crossCheck -isnot [Collections.IDictionary]) -or
        ((Get-RequiredMapValue $crossCheck exactRawEvidenceCrossChecked (
                "$Owner verifierCrossCheck")) -isnot [bool]) -or
        (-not [bool]$crossCheck.exactRawEvidenceCrossChecked) -or
        ([long](Get-RequiredMapValue $crossCheck vendorCount (
                "$Owner verifierCrossCheck")) -ne 2) -or
        ([long](Get-RequiredMapValue $crossCheck generatedIncludeCount (
                "$Owner verifierCrossCheck")) -ne 3) -or
        ([long](Get-RequiredMapValue $crossCheck protectedDependencyCount (
                "$Owner verifierCrossCheck")) -ne 3) -or
        ([long](Get-RequiredMapValue $crossCheck networkUnionCount (
                "$Owner verifierCrossCheck")) -ne
            [long]$Artifacts.fullNetwork.unionCount)) {
        throw "$Owner verifier/capture cross-check evidence drifted."
    }
}

function Assert-JsonStructuralEquality {
    param(
        [Parameter(Mandatory = $true)][object]$Expected,
        [Parameter(Mandatory = $true)][object]$Observed,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $expectedText = ConvertTo-AsciiLfJsonText -Value $Expected -Depth 50
    $observedText = ConvertTo-AsciiLfJsonText -Value $Observed -Depth 50
    if ($expectedText -cne $observedText) {
        throw "$Owner structured evidence differs."
    }
}

function Assert-TargetInventoryBoundToCommit {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$TargetWorktree,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $treeResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'ls-tree', '-r', '-z', '--name-only', $Commit,
            '--', $TargetRelativeRoot)
    Assert-CommandPassed -Result $treeResult -Owner "$Owner target tree lookup"
    $treePaths = @(
        ConvertFrom-NulPathOutput -Output $treeResult.Stdout |
            Sort-Object -Unique)
    $recordedAvailablePaths = @(
        $TargetWorktree.files |
            Where-Object { $_.available } |
            ForEach-Object { [string]$_.path } |
            Sort-Object -Unique)
    if ([string]::Join("`n", $treePaths) -cne
        [string]::Join("`n", $recordedAvailablePaths)) {
        throw (
            "$Owner full target path inventory is not exactly enclosed by " +
            'the binding commit.')
    }
}

function Assert-ArtifactFilesBoundToCommit {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Artifacts,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $files = @(
        @(
            'classesDatabase', 'projectDatabase', 'projectDefinition',
            'tcpMotionInterface', 'derivedSender', 'configObjects',
            'networksDatabase', 'commNetwork', 'commNetworkTable' |
                ForEach-Object { $Artifacts[$_] }) +
        @($Artifacts.vendorSources) +
        @($Artifacts.protectedDependencies) +
        @($Artifacts.generatedIncludes.files) +
        @($Artifacts.fullNetwork.files))
    $byPath = @{}
    foreach ($file in $files) {
        $path = [string]$file.path
        if ($byPath.ContainsKey($path)) {
            Assert-PublicFileTupleMatches `
                -Expected $byPath[$path] `
                -Observed $file `
                -Owner "$Owner duplicate artifact $path"
        }
        else {
            $byPath[$path] = $file
        }
    }
    foreach ($path in @($byPath.Keys | Sort-Object)) {
        $file = $byPath[$path]
        if ([bool]$file.available) {
            $historical = Get-GitBlobEvidence `
                -Root $Root `
                -GitPath $GitPath `
                -Commit $Commit `
                -Path $path `
                -Owner "$Owner artifact $path"
            if ($historical.blobOid -cne $file.filteredGitBlobOid) {
                throw "$Owner binding-commit artifact blob differs: $path"
            }
        }
        else {
            $lookup = Invoke-ProcessCapture `
                -FileName $GitPath `
                -Arguments @('-C', $Root, 'cat-file', '-e', "$Commit`:$path")
            if ($lookup.ExitCode -eq 0) {
                throw "$Owner absent artifact exists in binding commit: $path"
            }
        }
    }
}

function Assert-GitAncestor {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Ancestor,
        [Parameter(Mandatory = $true)][string]$Descendant,
        [Parameter(Mandatory = $true)][string]$Owner,
        [switch]$RequireDistinct
    )

    Assert-GitObjectId -Value $Ancestor -Owner "$Owner ancestor"
    Assert-GitObjectId -Value $Descendant -Owner "$Owner descendant"
    if ($RequireDistinct -and ($Ancestor -ceq $Descendant)) {
        throw "$Owner requires a distinct descendant commit."
    }
    $contextBefore = Get-GitRepositoryContext `
        -Root $Root `
        -GitPath $GitPath `
        -Owner "$Owner pre-ancestry"
    $result = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'merge-base', '--is-ancestor',
            $Ancestor, $Descendant)
    $contextAfter = Get-GitRepositoryContext `
        -Root $Root `
        -GitPath $GitPath `
        -Owner "$Owner post-ancestry"
    Assert-GitRepositoryContextStable `
        -Expected $contextBefore `
        -Observed $contextAfter `
        -Owner "$Owner ancestry repository context"
    if ($result.ExitCode -ne 0) {
        throw (
            "$Owner Git ancestry check failed; ancestor=$Ancestor; " +
            "descendant=$Descendant; exit=$($result.ExitCode)")
    }
}

function Get-GateAArtifactBindingHeadForPhase {
    param(
        [Parameter(Mandatory = $true)][string]$CurrentPhase,
        [Parameter(Mandatory = $true)][string]$StartHead,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [AllowNull()][Collections.IDictionary]$ImmediateParentData
    )

    Assert-GitObjectId -Value $StartHead -Owner 'lineage current start HEAD'
    $contract = $PhaseContracts[$CurrentPhase]
    if ([int]$contract.Sequence -eq 1) {
        return $StartHead
    }
    if ([int]$contract.Sequence -ne 2) {
        throw "Gate A artifact binding is undefined for phase: $CurrentPhase"
    }
    if (($null -eq $ImmediateParentData) -or
        ((Get-RequiredMapValue $ImmediateParentData phase (
                'immediate parent manifest')) -cne
            'GateB1_DerivedDeclaration')) {
        throw 'Gate B2 requires a validated Gate B1 immediate parent.'
    }
    $parentLineage = Get-RequiredMapValue `
        $ImmediateParentData lineage 'Gate B1 parent lineage'
    if (($parentLineage -isnot [Collections.IDictionary]) -or
        ([long](Get-RequiredMapValue $parentLineage sequence (
                'Gate B1 parent lineage')) -ne 1)) {
        throw 'Gate B1 parent lineage sequence is invalid.'
    }
    $parentRoot = Get-RequiredMapValue `
        $parentLineage rootGateA 'Gate B1 parent lineage'
    if ($parentRoot -isnot [Collections.IDictionary]) {
        throw 'Gate B1 parent rootGateA is not an object.'
    }
    $bindingHead = Get-RequiredMapValue `
        $parentRoot artifactBindingHead 'Gate B1 parent rootGateA'
    Assert-GitObjectId `
        -Value $bindingHead `
        -Owner 'Gate A ancestor artifact binding HEAD'
    if ($bindingHead -ceq $StartHead) {
        throw (
            'Gate B2 must preserve Gate A artifact state at the Gate B1 ' +
            'capture start; it cannot rebind Gate A artifacts to the current ' +
            'Gate B1 commit.')
    }
    Assert-GitAncestor `
        -Root $Root `
        -GitPath $GitPath `
        -Ancestor $bindingHead `
        -Descendant $StartHead `
        -Owner 'Gate A to Gate B1 artifact ratchet' `
        -RequireDistinct
    return [string]$bindingHead
}

function Assert-CheckpointManifestContract {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Data,
        [Parameter(Mandatory = $true)][string]$ExpectedPhase,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$ExpectedContract,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$SealEvidence,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$RepositoryBindingHead,
        [AllowNull()][pscustomobject]$ExpectedParentFile,
        [AllowNull()][Collections.IDictionary]$ExpectedParentData,
        [AllowNull()][pscustomobject]$ExpectedRootFile
    )

    $owner = "Checkpoint $ExpectedPhase"
    if (($SealEvidence.valid -isnot [bool]) -or
        (-not [bool]$SealEvidence.valid) -or
        ($SealEvidence.sealSha256 -notmatch '^[A-F0-9]{64}$')) {
        throw "$owner does not have a validated raw manifest seal."
    }
    Assert-ExactMapKeys `
        -Map $Data `
        -Keys @(
            'schema', 'phase', 'observedAt', 'lineage', 'targetProject',
            'verifierDecision', 'approvalRatchet', 'captureSafety', 'git',
            'tooling', 'artifacts', 'integrity') `
        -Owner $owner
    $integrity = Get-RequiredMapValue $Data integrity $owner
    if ($integrity -isnot [Collections.IDictionary]) {
        throw "$owner integrity is not an object."
    }
    Assert-ExactMapKeys `
        -Map $integrity `
        -Keys @(
            'algorithm', 'canonicalization', 'sealedPayloadBytes',
            'sealSha256') `
        -Owner "$owner integrity"
    if (($integrity.algorithm -cne 'SHA-256') -or
        ($integrity.canonicalization -cne
            'exact UTF-8 ASCII/LF JSON bytes with sealSha256 set to 64 zeros') -or
        (-not (Test-IsJsonInteger -Value $integrity.sealedPayloadBytes)) -or
        ([long]$integrity.sealedPayloadBytes -ne
            [long]$SealEvidence.sealedPayloadBytes) -or
        ($integrity.sealSha256 -cne $SealEvidence.sealSha256)) {
        throw "$owner integrity metadata differs from its raw validated seal."
    }
    $observedAt = Get-RequiredMapValue $Data observedAt $owner
    $parsedObservedAt = [DateTimeOffset]::MinValue
    if (($observedAt -isnot [string]) -or
        (-not [DateTimeOffset]::TryParseExact(
                [string]$observedAt,
                'o',
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::RoundtripKind,
                [ref]$parsedObservedAt))) {
        throw "$owner observedAt is not round-trip ISO-8601."
    }
    if ((Get-RequiredMapValue $Data schema $owner) -cne
        'LasalUdpCallbackGateBCheckpoint/v2') {
        throw "$owner has an unsupported schema."
    }
    if ((Get-RequiredMapValue $Data phase $owner) -cne $ExpectedPhase) {
        throw "$owner phase does not match its required lineage position."
    }
    $target = Get-RequiredMapValue $Data targetProject $owner
    if ($target -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $target `
            -Keys @('path', 'compilerVersion', 'targetArchitecture') `
            -Owner "$owner targetProject"
    }
    if (($target -isnot [Collections.IDictionary]) -or
        ((Get-RequiredMapValue $target path "$owner targetProject") -cne
            $TargetRelativeRoot) -or
        ((Get-RequiredMapValue $target compilerVersion "$owner targetProject") -cne
            'C78') -or
        ((Get-RequiredMapValue $target targetArchitecture "$owner targetProject") -cne
            'ARM')) {
        throw "$owner targetProject contract drifted."
    }
    $decision = Get-RequiredMapValue $Data verifierDecision $owner
    if ($decision -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $decision `
            -Keys @(
                'authoritativeLine', 'state', 'productionApproved',
                'needsRebaseline', 'structuredEvidence') `
            -Owner "$owner verifierDecision"
    }
    $decisionApproved = if ($decision -is [Collections.IDictionary]) {
        Get-RequiredMapValue $decision productionApproved (
            "$owner verifierDecision")
    }
    else { $null }
    $decisionRebaseline = if ($decision -is [Collections.IDictionary]) {
        Get-RequiredMapValue $decision needsRebaseline (
            "$owner verifierDecision")
    }
    else { $null }
    if (($decision -isnot [Collections.IDictionary]) -or
        ((Get-RequiredMapValue $decision state "$owner verifierDecision") -cne
            $ExpectedContract.ExpectedState) -or
        ($decisionApproved -isnot [bool]) -or
        ([bool]$decisionApproved -ne
            [bool]$ExpectedContract.ProductionApproved) -or
        ($decisionRebaseline -isnot [bool]) -or
        ([bool]$decisionRebaseline -ne
            [bool]$ExpectedContract.NeedsRebaseline)) {
        throw "$owner verifier decision contract drifted."
    }
    $ratchet = Get-RequiredMapValue $Data approvalRatchet $owner
    if ($ratchet -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $ratchet `
            -Keys @('productionApproved', 'needsRebaseline', 'note') `
            -Owner "$owner approvalRatchet"
    }
    $ratchetApproved = if ($ratchet -is [Collections.IDictionary]) {
        Get-RequiredMapValue $ratchet productionApproved (
            "$owner approvalRatchet")
    }
    else { $null }
    $ratchetRebaseline = if ($ratchet -is [Collections.IDictionary]) {
        Get-RequiredMapValue $ratchet needsRebaseline (
            "$owner approvalRatchet")
    }
    else { $null }
    if (($ratchet -isnot [Collections.IDictionary]) -or
        ($ratchetApproved -isnot [bool]) -or
        ([bool]$ratchetApproved -ne
            [bool]$ExpectedContract.ProductionApproved) -or
        ($ratchetRebaseline -isnot [bool]) -or
        ([bool]$ratchetRebaseline -ne
            [bool]$ExpectedContract.NeedsRebaseline) -or
        ($ratchet.note -isnot [string]) -or
        [string]::IsNullOrWhiteSpace([string]$ratchet.note)) {
        throw "$owner approval ratchet drifted."
    }
    $safety = Get-RequiredMapValue $Data captureSafety $owner
    if ($safety -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $safety `
            -Keys @(
                'lasalProcessName', 'initialPidCount',
                'finalPrePublishPidCount', 'finalCommitGuardPidCount',
                'lasalObservedClosedAtAllGuards',
                'continuousProcessAbsenceClaimed', 'outputDirectory',
                'outputFile', 'outputMode', 'writeScope',
                'capturedInputsStable', 'rawReadStrategy', 'textPolicy',
                'finalizationProtocol',
                'atomicMoveIsFinalExternalStateCommitPoint',
                'postMoveExternalStateChecks', 'orphanStagePolicy',
                'derivedSenderExpectedPresent') `
            -Owner "$owner captureSafety"
    }
    if (($safety -isnot [Collections.IDictionary]) -or
        ((Get-RequiredMapValue $safety outputFile "$owner captureSafety") -cne
            $ExpectedContract.OutputFile) -or
        ($safety.lasalProcessName -cne 'Lasal2') -or
        ($safety.outputDirectory -cne $EvidenceRelativeRoot) -or
        ($safety.finalizationProtocol -cne
            'verified-stage/all-final-guards/atomic-move-last/v1') -or
        ($safety.atomicMoveIsFinalExternalStateCommitPoint -isnot [bool]) -or
        (-not [bool]$safety.atomicMoveIsFinalExternalStateCommitPoint) -or
        ($safety.postMoveExternalStateChecks -isnot [bool]) -or
        [bool]$safety.postMoveExternalStateChecks -or
        ($safety.lasalObservedClosedAtAllGuards -isnot [bool]) -or
        (-not [bool]$safety.lasalObservedClosedAtAllGuards) -or
        ($safety.continuousProcessAbsenceClaimed -isnot [bool]) -or
        [bool]$safety.continuousProcessAbsenceClaimed -or
        ($safety.capturedInputsStable -isnot [bool]) -or
        (-not [bool]$safety.capturedInputsStable) -or
        ($safety.derivedSenderExpectedPresent -isnot [bool]) -or
        ([bool]$safety.derivedSenderExpectedPresent -ne
            ($ExpectedContract.ExpectedState -cne 'VendorImported'))) {
        throw "$owner output identity drifted."
    }
    foreach ($name in @(
            'initialPidCount', 'finalPrePublishPidCount',
            'finalCommitGuardPidCount')) {
        if ((-not (Test-IsJsonInteger -Value $safety[$name])) -or
            ([long]$safety[$name] -ne 0)) {
            throw "$owner captureSafety $name did not prove LASAL closed."
        }
    }
    foreach ($name in @(
            'outputMode', 'writeScope', 'rawReadStrategy', 'textPolicy',
            'orphanStagePolicy')) {
        if (($safety[$name] -isnot [string]) -or
            [string]::IsNullOrWhiteSpace([string]$safety[$name])) {
            throw "$owner captureSafety $name is missing."
        }
    }

    $recordedGit = Get-RequiredMapValue $Data git $owner
    if ($recordedGit -isnot [Collections.IDictionary]) {
        throw "$owner git evidence is not an object."
    }
    Assert-ExactMapKeys `
        -Map $recordedGit `
        -Keys @(
            'head', 'gatedPathspec', 'start', 'prePublish',
            'finalCommitGuard', 'stageGuardRevalidationRequired',
            'fullRepositoryTrackedPathCount',
            'fullRepositoryTrackedPathInventorySha256',
            'fullRepositoryTrackedPaths') `
        -Owner "$owner git"
    Assert-GitObjectId -Value $recordedGit.head -Owner "$owner git HEAD"
    foreach ($name in @('start', 'prePublish', 'finalCommitGuard')) {
        if ($recordedGit[$name] -isnot [Collections.IDictionary]) {
            throw "$owner git $name is not an object."
        }
        Assert-GitSnapshotEvidence `
            -Snapshot $recordedGit[$name] `
            -Owner "$owner git $name"
        if ($recordedGit[$name].head -cne $recordedGit.head) {
            throw "$owner git $name HEAD differs from recorded head."
        }
    }
    $currentRepositoryContext = Get-GitRepositoryContext `
        -Root $RepositoryRoot `
        -GitPath $GitPath `
        -Owner "$owner current repository context"
    Assert-GitRepositoryContextStable `
        -Expected $recordedGit.start.repositoryContext `
        -Observed $currentRepositoryContext `
        -Owner "$owner recorded/current repository context"
    Assert-JsonStructuralEquality `
        -Expected $recordedGit.start `
        -Observed $recordedGit.prePublish `
        -Owner "$owner start/prePublish Git state"
    Assert-JsonStructuralEquality `
        -Expected $recordedGit.start `
        -Observed $recordedGit.finalCommitGuard `
        -Owner "$owner start/finalCommitGuard Git state"
    if (($recordedGit.stageGuardRevalidationRequired -isnot [bool]) -or
        (-not [bool]$recordedGit.stageGuardRevalidationRequired)) {
        throw "$owner did not require post-stage final Git guard equality."
    }
    if (($recordedGit.gatedPathspec -isnot [Collections.IList]) -or
        ($recordedGit.fullRepositoryTrackedPaths -isnot [Collections.IList]) -or
        (-not (Test-IsJsonInteger -Value `
                $recordedGit.fullRepositoryTrackedPathCount))) {
        throw "$owner Git path inventories are malformed."
    }
    $expectedGatedPathspec = @(
        $TargetRelativeRoot,
        $VerifierRelativePath,
        "$EvidenceRelativeRoot/Capture-UdpCallbackGateBCheckpoint.ps1")
    if ([int]$ExpectedContract.Sequence -ge 1) {
        $expectedGatedPathspec +=
            "$EvidenceRelativeRoot/$($PhaseContracts.GateA_VendorImported.OutputFile)"
    }
    if ([int]$ExpectedContract.Sequence -ge 2) {
        $expectedGatedPathspec +=
            "$EvidenceRelativeRoot/$($PhaseContracts.GateB1_DerivedDeclaration.OutputFile)"
    }
    $expectedGatedPathspec = @($expectedGatedPathspec | Sort-Object -Unique)
    if ([string]::Join("`n", @($recordedGit.gatedPathspec)) -cne
        [string]::Join("`n", $expectedGatedPathspec)) {
        throw "$owner gated pathspec differs from the phase contract."
    }
    $trackedPaths = @($recordedGit.fullRepositoryTrackedPaths)
    if (($trackedPaths.Count -ne
            [long]$recordedGit.fullRepositoryTrackedPathCount) -or
        ([string]::Join("`n", $trackedPaths) -cne
            [string]::Join("`n", @($recordedGit.start.trackedPaths))) -or
        ([long]$recordedGit.fullRepositoryTrackedPathCount -ne
            [long]$recordedGit.start.trackedPathCount) -or
        ((Get-TextSha256 -Text ([string]::Join("`n", $trackedPaths))) -cne
            $recordedGit.fullRepositoryTrackedPathInventorySha256)) {
        throw "$owner full repository tracked path inventory drifted."
    }
    Assert-UpperSha256 `
        -Value $recordedGit.fullRepositoryTrackedPathInventorySha256 `
        -Owner "$owner full tracked path digest"
    $commitExists = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $RepositoryRoot, 'cat-file', '-e',
            "$($recordedGit.head)^{commit}")
    Assert-CommandPassed `
        -Result $commitExists `
        -Owner "$owner recorded Git commit"

    $tooling = Get-RequiredMapValue $Data tooling $owner
    $trust = if ($tooling -is [Collections.IDictionary]) {
        Get-RequiredMapValue $tooling trust "$owner tooling"
    }
    else { $null }
    $pin = if ($tooling -is [Collections.IDictionary]) {
        Get-RequiredMapValue $tooling verifierCanonicalPin "$owner tooling"
    }
    else { $null }
    $trustedHead = if ($trust -is [Collections.IDictionary]) {
        Get-RequiredMapValue $trust trustedCommittedHead "$owner tool trust"
    }
    else { $null }
    if (($tooling -isnot [Collections.IDictionary]) -or
        ($trust -isnot [Collections.IDictionary]) -or
        ($trustedHead -isnot [bool]) -or
        (-not [bool]$trustedHead) -or
        ((Get-RequiredMapValue $trust mode "$owner tool trust") -cne
            'committed-clean') -or
        ($pin -isnot [Collections.IDictionary]) -or
        ((Get-RequiredMapValue $pin pinSource "$owner verifier pin") -cne
            'committed-reviewed-pin') -or
        ((Get-RequiredMapValue $pin canonicalLfSha256 "$owner verifier pin") -notmatch
            '^[A-F0-9]{64}$')) {
        throw "$owner was not produced by committed reviewed tooling."
    }
    $artifacts = Get-RequiredMapValue $Data artifacts $owner
    if ($artifacts -isnot [Collections.IDictionary]) {
        throw "$owner artifacts is not an object."
    }
    Assert-ArtifactEvidenceContract `
        -Artifacts $artifacts `
        -Decision $decision `
        -TargetWorktree $recordedGit.start.targetWorktree `
        -ExpectedState $ExpectedContract.ExpectedState `
        -Owner $owner
    Assert-ToolingEvidenceContract `
        -Tooling $tooling `
        -RecordedGit $recordedGit `
        -Decision $decision `
        -Artifacts $artifacts `
        -Root $RepositoryRoot `
        -GitPath $GitPath `
        -Owner $owner
    Assert-GitObjectId `
        -Value $RepositoryBindingHead `
        -Owner "$owner binding HEAD"
    Assert-GitAncestor `
        -Root $RepositoryRoot `
        -GitPath $GitPath `
        -Ancestor $recordedGit.head `
        -Descendant $RepositoryBindingHead `
        -Owner "$owner capture-to-binding ratchet" `
        -RequireDistinct
    Assert-TargetInventoryBoundToCommit `
        -TargetWorktree $recordedGit.start.targetWorktree `
        -Root $RepositoryRoot `
        -GitPath $GitPath `
        -Commit $RepositoryBindingHead `
        -Owner $owner
    Assert-ArtifactFilesBoundToCommit `
        -Artifacts $artifacts `
        -Root $RepositoryRoot `
        -GitPath $GitPath `
        -Commit $RepositoryBindingHead `
        -Owner $owner
    $lineage = Get-RequiredMapValue $Data lineage $owner
    if ($lineage -is [Collections.IDictionary]) {
        Assert-ExactMapKeys `
            -Map $lineage `
            -Keys @('sequence', 'parent', 'rootGateA', 'validatedAncestorCount') `
            -Owner "$owner lineage"
    }
    $lineageSequence = if ($lineage -is [Collections.IDictionary]) {
        Get-RequiredMapValue $lineage sequence "$owner lineage"
    }
    else { $null }
    $ancestorCount = if ($lineage -is [Collections.IDictionary]) {
        Get-RequiredMapValue $lineage validatedAncestorCount "$owner lineage"
    }
    else { $null }
    if (($lineage -isnot [Collections.IDictionary]) -or
        ($lineageSequence -isnot [int] -and
            $lineageSequence -isnot [long]) -or
        ([int]$lineageSequence -ne
            [int]$ExpectedContract.Sequence) -or
        ($ancestorCount -isnot [int] -and $ancestorCount -isnot [long]) -or
        ([int]$ancestorCount -ne
            [int]$ExpectedContract.Sequence)) {
        throw "$owner lineage sequence drifted."
    }
    $parent = Get-RequiredMapValue $lineage parent "$owner lineage"
    $rootGateA = Get-RequiredMapValue $lineage rootGateA "$owner lineage"
    if ([int]$ExpectedContract.Sequence -eq 0) {
        if (($null -ne $parent) -or ($null -ne $rootGateA)) {
            throw 'Gate A lineage must not have a parent or root self-reference.'
        }
        return
    }
    if (($null -eq $ExpectedParentFile) -or
        ($null -eq $ExpectedParentData) -or
        ($null -eq $ExpectedRootFile) -or
        ($parent -isnot [Collections.IDictionary]) -or
        ($rootGateA -isnot [Collections.IDictionary])) {
        throw "$owner lineage parent evidence is incomplete."
    }
    Assert-ExactMapKeys `
        -Map $parent `
        -Keys @(
            'path', 'bytes', 'sha256', 'schema', 'phase', 'state',
            'observedAt', 'gitHead', 'verifierCanonicalLfSha256',
            'artifactBindingHead', 'manifestCommittedAtChildStartHead',
            'manifestCommittedBlobOid') `
        -Owner "$owner lineage parent"
    Assert-ExactMapKeys `
        -Map $rootGateA `
        -Keys @(
            'path', 'bytes', 'sha256', 'artifactBindingHead',
            'manifestCommittedAtChildStartHead',
            'manifestCommittedBlobOid') `
        -Owner "$owner rootGateA"
    $expectedParentPath =
        "$EvidenceRelativeRoot/$($ExpectedContract.ParentFile)"
    if (((Get-RequiredMapValue $parent path "$owner lineage parent") -cne
            $expectedParentPath) -or
        ([long](Get-RequiredMapValue $parent bytes "$owner lineage parent") -ne
            [long]$ExpectedParentFile.Public.bytes) -or
        ((Get-RequiredMapValue $parent sha256 "$owner lineage parent") -cne
            $ExpectedParentFile.Public.sha256) -or
        ((Get-RequiredMapValue $parent schema "$owner lineage parent") -cne
            'LasalUdpCallbackGateBCheckpoint/v2') -or
        ((Get-RequiredMapValue $parent phase "$owner lineage parent") -cne
            $ExpectedContract.ParentPhase) -or
        ((Get-RequiredMapValue $parent state "$owner lineage parent") -cne
            $ExpectedContract.ParentState)) {
        throw "$owner parent manifest link drifted."
    }
    $parentGit = Get-RequiredMapValue $ExpectedParentData git (
        "$owner parent manifest")
    $parentTooling = Get-RequiredMapValue $ExpectedParentData tooling (
        "$owner parent manifest")
    $parentPin = Get-RequiredMapValue $parentTooling verifierCanonicalPin (
        "$owner parent tooling")
    if (((Get-RequiredMapValue $parent gitHead "$owner lineage parent") -cne
            (Get-RequiredMapValue $parentGit head "$owner parent git")) -or
        ((Get-RequiredMapValue $parent observedAt "$owner lineage parent") -cne
            (Get-RequiredMapValue $ExpectedParentData observedAt (
                "$owner parent manifest"))) -or
        ((Get-RequiredMapValue $parent verifierCanonicalLfSha256 (
                "$owner lineage parent")) -cne
            (Get-RequiredMapValue $parentPin canonicalLfSha256 (
                "$owner parent verifier pin")))) {
        throw "$owner parent provenance link drifted."
    }
    $expectedRootArtifactBindingHead =
        Get-GateAArtifactBindingHeadForPhase `
            -CurrentPhase $ExpectedPhase `
            -StartHead $recordedGit.head `
            -Root $RepositoryRoot `
            -GitPath $GitPath `
            -ImmediateParentData $ExpectedParentData
    if (($parent.artifactBindingHead -cne $recordedGit.head) -or
        ($rootGateA.artifactBindingHead -cne
            $expectedRootArtifactBindingHead) -or
        ($parent.manifestCommittedAtChildStartHead -cne $recordedGit.head) -or
        ($rootGateA.manifestCommittedAtChildStartHead -cne
            $recordedGit.head)) {
        throw "$owner lineage artifact/manifest ratchet HEAD drifted."
    }
    Assert-GitObjectId `
        -Value $parent.artifactBindingHead `
        -Owner "$owner parent artifact binding HEAD"
    Assert-GitObjectId `
        -Value $rootGateA.artifactBindingHead `
        -Owner "$owner Gate A artifact binding HEAD"
    Assert-GitObjectId `
        -Value $parent.manifestCommittedBlobOid `
        -Owner "$owner parent committed blob"
    Assert-GitObjectId `
        -Value $rootGateA.manifestCommittedBlobOid `
        -Owner "$owner Gate A committed blob"
    $parentBlob = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $RepositoryRoot, 'rev-parse',
            "$($parent.manifestCommittedAtChildStartHead):$expectedParentPath")
    Assert-CommandPassed -Result $parentBlob -Owner "$owner parent lineage blob"
    if ($parentBlob.Stdout.Trim().ToUpperInvariant() -cne
        $parent.manifestCommittedBlobOid) {
        throw "$owner parent lineage blob identity drifted."
    }
    $gateAPath =
        "$EvidenceRelativeRoot/$($PhaseContracts.GateA_VendorImported.OutputFile)"
    if (((Get-RequiredMapValue $rootGateA path "$owner rootGateA") -cne
            $gateAPath) -or
        ([long](Get-RequiredMapValue $rootGateA bytes "$owner rootGateA") -ne
            [long]$ExpectedRootFile.Public.bytes) -or
        ((Get-RequiredMapValue $rootGateA sha256 "$owner rootGateA") -cne
            $ExpectedRootFile.Public.sha256)) {
        throw "$owner Gate A root link drifted."
    }
    $rootBlob = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $RepositoryRoot, 'rev-parse',
            "$($rootGateA.manifestCommittedAtChildStartHead):$gateAPath")
    Assert-CommandPassed -Result $rootBlob -Owner "$owner Gate A lineage blob"
    if ($rootBlob.Stdout.Trim().ToUpperInvariant() -cne
        $rootGateA.manifestCommittedBlobOid) {
        throw "$owner Gate A lineage blob identity drifted."
    }
}

function Get-ValidatedLineageEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$CurrentPhase,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, object]]$ReadFiles,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$StartHead
    )

    $contract = $PhaseContracts[$CurrentPhase]
    if ([int]$contract.Sequence -eq 0) {
        return [ordered]@{
            sequence = 0
            parent = $null
            rootGateA = $null
            validatedAncestorCount = 0
        }
    }
    $gateAContract = $PhaseContracts.GateA_VendorImported
    $gateAPath = "$EvidenceRelativeRoot/$($gateAContract.OutputFile)"
    $gateAFile = $ReadFiles[$gateAPath]
    $gateAIdentity = Get-CommittedPathIdentity `
        -Root $Root `
        -GitPath $GitPath `
        -Path $gateAPath `
        -StartHead $StartHead `
        -RequireAsciiCanonical
    if (([long]$gateAIdentity.physicalBytes -ne
            [long]$gateAFile.Public.bytes) -or
        ($gateAIdentity.physicalSha256 -cne $gateAFile.Public.sha256)) {
        throw 'Gate A parent manifest differs from its committed-path identity.'
    }
    $gateAParsed = ConvertFrom-StrictCheckpointJson `
        -File $gateAFile `
        -Owner 'Gate A parent manifest'
    $gateAData = $gateAParsed.Data

    $parentContract = $PhaseContracts[$contract.ParentPhase]
    $parentPath = "$EvidenceRelativeRoot/$($parentContract.OutputFile)"
    $parentFile = $ReadFiles[$parentPath]
    $parentIdentity = if ($parentPath -ceq $gateAPath) {
        $gateAIdentity
    }
    else {
        Get-CommittedPathIdentity `
            -Root $Root `
            -GitPath $GitPath `
            -Path $parentPath `
            -StartHead $StartHead `
            -RequireAsciiCanonical
    }
    if (([long]$parentIdentity.physicalBytes -ne
            [long]$parentFile.Public.bytes) -or
        ($parentIdentity.physicalSha256 -cne $parentFile.Public.sha256)) {
        throw 'Parent manifest differs from its committed-path identity.'
    }
    $parentParsed = if ($contract.ParentPhase -ceq 'GateA_VendorImported') {
        $gateAParsed
    }
    else {
        ConvertFrom-StrictCheckpointJson `
            -File $parentFile `
            -Owner 'Gate B1 parent manifest'
    }
    $parentData = if ($contract.ParentPhase -ceq 'GateA_VendorImported') {
        $gateAData
    }
    else {
        $parentParsed.Data
    }

    $gateAArtifactBindingHead = $StartHead
    if ($contract.ParentPhase -cne 'GateA_VendorImported') {
        Assert-CheckpointManifestContract `
            -Data $parentData `
            -ExpectedPhase $contract.ParentPhase `
            -ExpectedContract $parentContract `
            -SealEvidence $parentParsed.Seal `
            -RepositoryRoot $Root `
            -GitPath $GitPath `
            -RepositoryBindingHead $StartHead `
            -ExpectedParentFile $gateAFile `
            -ExpectedParentData $gateAData `
            -ExpectedRootFile $gateAFile
        $gateAArtifactBindingHead =
            Get-GateAArtifactBindingHeadForPhase `
                -CurrentPhase $CurrentPhase `
                -StartHead $StartHead `
                -Root $Root `
                -GitPath $GitPath `
                -ImmediateParentData $parentData
    }
    Assert-GitObjectId `
        -Value $gateAArtifactBindingHead `
        -Owner 'Gate A artifact binding HEAD'
    Assert-CheckpointManifestContract `
        -Data $gateAData `
        -ExpectedPhase 'GateA_VendorImported' `
        -ExpectedContract $gateAContract `
        -SealEvidence $gateAParsed.Seal `
        -RepositoryRoot $Root `
        -GitPath $GitPath `
        -RepositoryBindingHead $gateAArtifactBindingHead `
        -ExpectedParentFile $null `
        -ExpectedParentData $null `
        -ExpectedRootFile $null

    $parentGit = Get-RequiredMapValue $parentData git 'parent manifest'
    $parentTooling = Get-RequiredMapValue $parentData tooling 'parent manifest'
    $parentPin = Get-RequiredMapValue $parentTooling verifierCanonicalPin (
        'parent manifest tooling')
    return [ordered]@{
        sequence = [int]$contract.Sequence
        parent = [ordered]@{
            path = $parentPath
            bytes = $parentFile.Public.bytes
            sha256 = $parentFile.Public.sha256
            schema = 'LasalUdpCallbackGateBCheckpoint/v2'
            phase = $contract.ParentPhase
            state = $contract.ParentState
            observedAt = Get-RequiredMapValue $parentData observedAt (
                'parent manifest')
            gitHead = Get-RequiredMapValue $parentGit head 'parent manifest git'
            verifierCanonicalLfSha256 = Get-RequiredMapValue `
                $parentPin canonicalLfSha256 'parent verifier pin'
            artifactBindingHead = $StartHead
            manifestCommittedAtChildStartHead = $StartHead
            manifestCommittedBlobOid = $parentIdentity.headBlobOid
        }
        rootGateA = [ordered]@{
            path = $gateAPath
            bytes = $gateAFile.Public.bytes
            sha256 = $gateAFile.Public.sha256
            artifactBindingHead = $gateAArtifactBindingHead
            manifestCommittedAtChildStartHead = $StartHead
            manifestCommittedBlobOid = $gateAIdentity.headBlobOid
        }
        validatedAncestorCount = [int]$contract.Sequence
    }
}

function Assert-InputContentStable {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, object]]$ReadFiles,
        [Parameter(Mandatory = $true)][string[]]$InitialIncludePaths,
        [Parameter(Mandatory = $true)][string[]]$InitialNetworkPaths
    )

    $finalIncludePaths = Get-AvailableRelativeFiles `
        -Root $Root `
        -RelativeDirectory "$TargetRelativeRoot/Include"
    $finalNetworkPaths = Get-AvailableRelativeFiles `
        -Root $Root `
        -RelativeDirectory "$TargetRelativeRoot/Network"
    if ([string]::Join("`n", $InitialIncludePaths) -cne
        [string]::Join("`n", $finalIncludePaths)) {
        throw 'Generated Include inventory changed during capture.'
    }
    if ([string]::Join("`n", $InitialNetworkPaths) -cne
        [string]::Join("`n", $finalNetworkPaths)) {
        throw 'Network inventory changed during capture.'
    }
    foreach ($entry in $ReadFiles.GetEnumerator()) {
        $fullPath = Join-Path $Root $entry.Key.Replace('/', '\')
        $null = Assert-PathComponentsNoReparse `
            -Root $Root `
            -Path $fullPath
        $current = Get-FileMetadata -Path $fullPath
        $initial = $entry.Value.Metadata
        if ((-not $current.exists) -or
            ($current.length -ne $initial.length) -or
            ($current.lastWriteTimeUtcTicks -ne
                $initial.lastWriteTimeUtcTicks)) {
            throw "Capture input metadata changed: $($entry.Key)"
        }
        $currentBytes = [IO.File]::ReadAllBytes($fullPath)
        $currentSha256 = Get-BytesSha256 -Bytes $currentBytes
        if (($currentBytes.Length -ne $entry.Value.Public.bytes) -or
            ($currentSha256 -cne $entry.Value.Public.sha256)) {
            throw "Capture input raw content changed: $($entry.Key)"
        }
    }
}

function ConvertFrom-NulPathOutput {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()][string]$Output
    )

    return @(
        $Output.Split([char]0) |
            Where-Object { $_ -ne '' } |
            ForEach-Object { $_.Replace('\', '/') })
}

function Get-TargetWorktreeInventory {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][string[]]$TrackedPaths,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][string[]]$UntrackedPaths
    )

    $trackedSet = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($path in $TrackedPaths) {
        $null = $trackedSet.Add($path)
    }
    $untrackedSet = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
    foreach ($path in $UntrackedPaths) {
        $null = $untrackedSet.Add($path)
    }
    $allPaths = @(
        @($TrackedPaths + $UntrackedPaths) |
            Sort-Object -Unique)
    $files = @(
        foreach ($relativePath in $allPaths) {
            if (-not $relativePath.StartsWith(
                    $TargetRelativeRoot + '/',
                    [StringComparison]::OrdinalIgnoreCase)) {
                throw "Target inventory escaped its scope: $relativePath"
            }
            $fullPath = [IO.Path]::GetFullPath((Join-Path $Root (
                        $relativePath.Replace('/', '\'))))
            $canonical = Get-RepositoryRelativePath `
                -Root $Root `
                -Path $fullPath
            if ($canonical -cne $relativePath) {
                throw "Target inventory path is not canonical: $relativePath"
            }
            $available = [IO.File]::Exists($fullPath)
            if ($available) {
                $null = Assert-PathComponentsNoReparse `
                    -Root $Root `
                    -Path $fullPath
                $before = Get-FileMetadata -Path $fullPath
                $bytes = [IO.File]::ReadAllBytes($fullPath)
                $after = Get-FileMetadata -Path $fullPath
                if (($before.length -ne $bytes.Length) -or
                    ($before.length -ne $after.length) -or
                    ($before.lastWriteTimeUtcTicks -ne
                        $after.lastWriteTimeUtcTicks)) {
                    throw "Target inventory input changed while read: $relativePath"
                }
            }
            [ordered]@{
                path = $relativePath
                gitTracked = $trackedSet.Contains($relativePath)
                nonIgnoredUntracked = $untrackedSet.Contains($relativePath)
                available = $available
                bytes = if ($available) { $bytes.Length } else { $null }
                sha256 = if ($available) {
                    Get-BytesSha256 -Bytes $bytes
                }
                else {
                    $null
                }
                lastWriteTimeUtcTicks = if ($available) {
                    $before.lastWriteTimeUtcTicks
                }
                else {
                    $null
                }
            }
        })
    $identity = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}|{5}|{6}' -f
                    $file.path,
                    ([int][bool]$file.gitTracked),
                    ([int][bool]$file.nonIgnoredUntracked),
                    ([int][bool]$file.available),
                    $file.bytes,
                    $file.sha256,
                    $file.lastWriteTimeUtcTicks
            }))
    return [ordered]@{
        trackedCount = $TrackedPaths.Count
        nonIgnoredUntrackedCount = $UntrackedPaths.Count
        unionCount = $files.Count
        identityAlgorithm = (
            'sort unique target tracked-plus-nonignored-untracked paths; ' +
            'join path|tracked01|untracked01|available01|bytes|' +
            'uppercase-sha256|lastWriteTimeUtcTicks with LF; UTF-8 SHA-256')
        identitySha256 = Get-TextSha256 -Text $identity
        files = $files
    }
}

function Get-GitStateSnapshot {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string[]]$GatedPathspec
    )

    $repositoryContextBefore = Get-GitRepositoryContext `
        -Root $Root `
        -GitPath $GitPath `
        -Owner 'Git snapshot start'
    $headResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'rev-parse', '--verify', 'HEAD')
    Assert-CommandPassed -Result $headResult -Owner 'git rev-parse HEAD'
    $head = $headResult.Stdout.Trim()
    if ($head -notmatch '^[0-9a-fA-F]{40,64}$') {
        throw "Git HEAD has an unexpected identity: $head"
    }

    if ($GatedPathspec.Count -eq 0) {
        throw 'Gated pathspec must not be empty.'
    }
    $indexArguments = @('-C', $Root, 'ls-files', '--stage', '-z')
    $indexResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments $indexArguments
    Assert-CommandPassed -Result $indexResult -Owner 'gated git index snapshot'

    $repositoryTrackedResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'ls-files', '-z')
    Assert-CommandPassed `
        -Result $repositoryTrackedResult `
        -Owner 'full repository tracked path snapshot'

    $statusArguments = @(
        '-C', $Root, 'status', '--porcelain=v2', '-z',
        '--untracked-files=all')
    $statusResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments $statusArguments
    Assert-CommandPassed -Result $statusResult -Owner 'gated git status snapshot'

    $trackedResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'ls-files', '-z', '--', $TargetRelativeRoot)
    Assert-CommandPassed `
        -Result $trackedResult `
        -Owner 'target tracked path snapshot'
    $untrackedResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @(
            '-C', $Root, 'ls-files', '--others', '--exclude-standard',
            '-z', '--', $TargetRelativeRoot)
    Assert-CommandPassed `
        -Result $untrackedResult `
        -Owner 'target nonignored untracked path snapshot'
    $trackedPaths = @(
        ConvertFrom-NulPathOutput -Output $trackedResult.Stdout |
            Sort-Object -Unique)
    $untrackedPaths = @(
        ConvertFrom-NulPathOutput -Output $untrackedResult.Stdout |
            Sort-Object -Unique)
    $inventory = Get-TargetWorktreeInventory `
        -Root $Root `
        -TrackedPaths $trackedPaths `
        -UntrackedPaths $untrackedPaths
    $indexEntries = @(ConvertFrom-NulPathOutput -Output $indexResult.Stdout)
    $repositoryTrackedPaths = @(
        ConvertFrom-NulPathOutput -Output $repositoryTrackedResult.Stdout)
    $statusEntries = @(ConvertFrom-NulPathOutput -Output $statusResult.Stdout)
    $repositoryContextAfter = Get-GitRepositoryContext `
        -Root $Root `
        -GitPath $GitPath `
        -Owner 'Git snapshot end'
    Assert-GitRepositoryContextStable `
        -Expected $repositoryContextBefore `
        -Observed $repositoryContextAfter `
        -Owner 'Git snapshot repository context'
    return [ordered]@{
        scope = 'full-repository-index-and-status'
        repositoryContext = $repositoryContextBefore
        head = $head.ToUpperInvariant()
        indexEntryCount = $indexEntries.Count
        indexRawTextSha256 = Get-TextSha256 -Text $indexResult.Stdout
        indexEntries = $indexEntries
        trackedPathCount = $repositoryTrackedPaths.Count
        trackedPathRawTextSha256 =
            Get-TextSha256 -Text $repositoryTrackedResult.Stdout
        trackedPaths = $repositoryTrackedPaths
        statusEntryCount = $statusEntries.Count
        statusRawTextSha256 = Get-TextSha256 -Text $statusResult.Stdout
        statusEntries = $statusEntries
        targetWorktree = $inventory
    }
}

function Assert-GitStateStable {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Observed,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    Assert-GitRepositoryContextStable `
        -Expected $Expected.repositoryContext `
        -Observed $Observed.repositoryContext `
        -Owner "$Owner Git repository context"
    if (($Observed.scope -cne $Expected.scope) -or
        ($Observed.head -cne $Expected.head) -or
        ($Observed.indexEntryCount -ne $Expected.indexEntryCount) -or
        ($Observed.indexRawTextSha256 -cne $Expected.indexRawTextSha256) -or
        ($Observed.trackedPathCount -ne $Expected.trackedPathCount) -or
        ($Observed.trackedPathRawTextSha256 -cne
            $Expected.trackedPathRawTextSha256) -or
        ($Observed.statusEntryCount -ne $Expected.statusEntryCount) -or
        ($Observed.statusRawTextSha256 -cne $Expected.statusRawTextSha256) -or
        ($Observed.targetWorktree.trackedCount -ne
            $Expected.targetWorktree.trackedCount) -or
        ($Observed.targetWorktree.nonIgnoredUntrackedCount -ne
            $Expected.targetWorktree.nonIgnoredUntrackedCount) -or
        ($Observed.targetWorktree.unionCount -ne
            $Expected.targetWorktree.unionCount) -or
        ($Observed.targetWorktree.identitySha256 -cne
            $Expected.targetWorktree.identitySha256)) {
        throw "$Owner Git/index/target-worktree snapshot changed."
    }
}

function Get-CommittedPathIdentity {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$StartHead,
        [switch]$PermitUncommitted,
        [switch]$RequireAsciiCanonical
    )

    $fullPath = [IO.Path]::GetFullPath((Join-Path $Root $Path.Replace('/', '\')))
    if (-not [IO.File]::Exists($fullPath)) {
        if (-not $PermitUncommitted) {
            throw "Committed-path input is missing: $Path"
        }
        return [ordered]@{
            path = $Path
            committedExact = $false
            reason = 'missing-worktree-file'
        }
    }
    $null = Assert-PathComponentsNoReparse -Root $Root -Path $fullPath

    $stageResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'ls-files', '--stage', '-z', '--', $Path)
    Assert-CommandPassed -Result $stageResult -Owner "index stage for $Path"
    $stageRecords = @(ConvertFrom-NulPathOutput -Output $stageResult.Stdout)
    $stageMatch = if ($stageRecords.Count -eq 1) {
        [regex]::Match(
            $stageRecords[0],
            '^(?<Mode>[0-9]{6}) (?<Oid>[0-9a-f]{40,64}) 0\t(?<Path>.+)$',
            [Text.RegularExpressions.RegexOptions]::CultureInvariant)
    }
    else {
        $null
    }
    $stage0Exact = ($null -ne $stageMatch) -and $stageMatch.Success -and
        ($stageMatch.Groups['Path'].Value -ceq $Path)

    $flagResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'ls-files', '-v', '-z', '--', $Path)
    Assert-CommandPassed -Result $flagResult -Owner "index flags for $Path"
    $flagRecords = @(ConvertFrom-NulPathOutput -Output $flagResult.Stdout)
    $normalFlagTag = ($flagRecords.Count -eq 1) -and
        ($flagRecords[0] -ceq "H $Path")

    $debugResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'ls-files', '--debug', '--', $Path)
    Assert-CommandPassed -Result $debugResult -Owner "index debug flags for $Path"
    $debugFlagMatches = @([regex]::Matches(
            $debugResult.Stdout,
            '(?m)(?:^|[ \t])flags: (?<Flags>[0-9a-fA-F]+)[ \t]*$'))
    $debugFlagsZero = ($debugFlagMatches.Count -eq 1) -and
        ($debugFlagMatches[0].Groups['Flags'].Value -match '^0+$')

    $headBlobResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'rev-parse', "${StartHead}:$Path")
    $headBlobOid = if ($headBlobResult.ExitCode -eq 0) {
        $headBlobResult.Stdout.Trim().ToUpperInvariant()
    }
    else {
        $null
    }
    $worktreeBlobResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments @('-C', $Root, 'hash-object', "--path=$Path", $fullPath)
    Assert-CommandPassed `
        -Result $worktreeBlobResult `
        -Owner "filtered worktree blob for $Path"
    $worktreeBlobOid = $worktreeBlobResult.Stdout.Trim().ToUpperInvariant()
    $stageBlobOid = if ($stage0Exact) {
        $stageMatch.Groups['Oid'].Value.ToUpperInvariant()
    }
    else {
        $null
    }

    $bytes = [IO.File]::ReadAllBytes($fullPath)
    $traits = Get-RawTextTraits -Bytes $bytes
    $canonicalBytes = $null
    $canonicalSha256 = $null
    $headCanonicalBytes = $null
    $headCanonicalSha256 = $null
    $canonicalHeadMatch = $false
    if ($RequireAsciiCanonical) {
        if (($traits.bom -cne 'None') -or
            (-not $traits.is7BitAscii) -or
            ($traits.eolStyle -notin @('LF', 'CRLF'))) {
            if (-not $PermitUncommitted) {
                throw "Committed tool is not canonicalizable ASCII text: $Path"
            }
        }
        else {
            $text = [Text.Encoding]::ASCII.GetString($bytes)
            $text = $text.Replace("`r`n", "`n").Replace("`r", "`n")
            $canonicalBytes = $Utf8NoBom.GetBytes($text)
            $canonicalSha256 = Get-BytesSha256 -Bytes $canonicalBytes
        }
    }
    if ($RequireAsciiCanonical -and ($null -ne $headBlobOid)) {
        $headBlob = Get-GitBlobEvidence `
            -Root $Root `
            -GitPath $GitPath `
            -Commit $StartHead `
            -Path $Path `
            -Owner "committed canonical tool $Path"
        if ($headBlob.blobOid -cne $headBlobOid) {
            throw "Committed canonical tool blob lookup drifted: $Path"
        }
        $headTraits = Get-RawTextTraits -Bytes $headBlob.rawBytes
        if (($headTraits.bom -ceq 'None') -and
            $headTraits.is7BitAscii -and
            ($headTraits.eolStyle -in @('LF', 'CRLF'))) {
            $headText = [Text.Encoding]::ASCII.GetString($headBlob.rawBytes)
            $headText = $headText.Replace("`r`n", "`n").Replace("`r", "`n")
            $headCanonicalBytes = $Utf8NoBom.GetBytes($headText)
            $headCanonicalSha256 = Get-BytesSha256 -Bytes $headCanonicalBytes
            $canonicalHeadMatch = ($null -ne $canonicalBytes) -and
                ($headCanonicalBytes.Length -eq $canonicalBytes.Length) -and
                ($headCanonicalSha256 -ceq $canonicalSha256)
        }
    }
    $committedExact = $stage0Exact -and $normalFlagTag -and
        $debugFlagsZero -and ($null -ne $headBlobOid) -and
        ($stageBlobOid -ceq $headBlobOid) -and
        ($worktreeBlobOid -ceq $headBlobOid) -and
        ((-not $RequireAsciiCanonical) -or $canonicalHeadMatch)
    if ((-not $committedExact) -and (-not $PermitUncommitted)) {
        throw (
            "Committed-path identity mismatch: $Path; " +
            "stage0=$stage0Exact; normalFlagTag=$normalFlagTag; " +
            "debugFlagsZero=$debugFlagsZero; stage=$stageBlobOid; " +
            "head=$headBlobOid; worktree=$worktreeBlobOid")
    }
    return [ordered]@{
        path = $Path
        committedExact = $committedExact
        startHead = $StartHead
        indexMode = if ($stage0Exact) {
            $stageMatch.Groups['Mode'].Value
        }
        else { $null }
        indexStage = if ($stage0Exact) { 0 } else { $null }
        indexFlagTag = if ($flagRecords.Count -eq 1) {
            $flagRecords[0].Substring(0, 1)
        }
        else { $null }
        indexDebugFlags = if ($debugFlagMatches.Count -eq 1) {
            [Convert]::ToInt64(
                $debugFlagMatches[0].Groups['Flags'].Value,
                16)
        }
        else { $null }
        stageBlobOid = $stageBlobOid
        headBlobOid = $headBlobOid
        filteredWorktreeBlobOid = $worktreeBlobOid
        physicalBytes = $bytes.Length
        physicalSha256 = Get-BytesSha256 -Bytes $bytes
        canonicalLfBytes = if ($null -ne $canonicalBytes) {
            $canonicalBytes.Length
        }
        else { $null }
        canonicalLfSha256 = $canonicalSha256
        headCanonicalLfBytes = if ($null -ne $headCanonicalBytes) {
            $headCanonicalBytes.Length
        }
        else { $null }
        headCanonicalLfSha256 = $headCanonicalSha256
        canonicalHeadMatch = $canonicalHeadMatch
    }
}

function Get-ToolTrustEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$GitPath,
        [Parameter(Mandatory = $true)][string[]]$ToolPaths,
        [Parameter(Mandatory = $true)][string]$StartHead,
        [switch]$PermitUncommitted
    )

    $worktreeResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments (@('-C', $Root, 'diff', '--quiet', '--') + $ToolPaths)
    if ($worktreeResult.ExitCode -notin @(0, 1)) {
        Assert-CommandPassed `
            -Result $worktreeResult `
            -Owner 'tool working-tree cleanliness'
    }
    $indexResult = Invoke-ProcessCapture `
        -FileName $GitPath `
        -Arguments (@('-C', $Root, 'diff', '--cached', '--quiet', '--') +
            $ToolPaths)
    if ($indexResult.ExitCode -notin @(0, 1)) {
        Assert-CommandPassed `
            -Result $indexResult `
            -Owner 'tool index cleanliness'
    }
    $identities = @(
        foreach ($path in $ToolPaths) {
            Get-CommittedPathIdentity `
                -Root $Root `
                -GitPath $GitPath `
                -Path $path `
                -StartHead $StartHead `
                -PermitUncommitted:$PermitUncommitted.IsPresent `
                -RequireAsciiCanonical
        })
    $trusted = (@($identities | Where-Object {
                -not $_.committedExact
            }).Count -eq 0) -and
        ($worktreeResult.ExitCode -eq 0) -and
        ($indexResult.ExitCode -eq 0)
    if ((-not $trusted) -and (-not $PermitUncommitted)) {
        throw (
            'Checkpoint tooling must be tracked and clean at committed HEAD. ' +
            "worktreeExit=$($worktreeResult.ExitCode); " +
            "indexExit=$($indexResult.ExitCode)")
    }
    return [ordered]@{
        trustedCommittedHead = $trusted
        startHead = $StartHead
        mode = if ($trusted) {
            'committed-clean'
        }
        else {
            'untrusted-bootstrap-validate-only'
        }
        workingTreeDiffExitCode = $worktreeResult.ExitCode
        indexDiffExitCode = $indexResult.ExitCode
        pathIdentities = $identities
    }
}

function Get-CurrentDecisionFromVerifierOutput {
    param(
        [Parameter(Mandatory = $true)][string]$Output,
        [Parameter(Mandatory = $true)][string]$ExpectedState,
        [Parameter(Mandatory = $true)][bool]$ExpectedProductionApproved,
        [Parameter(Mandatory = $true)][bool]$ExpectedNeedsRebaseline
    )

    $canonicalOutput = $Output.Replace("`r`n", "`n").Replace("`r", "`n")
    $lines = @($canonicalOutput.Split("`n") | Where-Object {
            $_ -match 'LASAL\.UdpCallbackContract\.Current'
        })
    if ($lines.Count -ne 1) {
        throw 'Verifier did not emit exactly one authoritative current-state line.'
    }
    $pattern =
        '^(?<Prefix>PASS|CAPTURE) LASAL\.UdpCallbackContract\.Current ' +
        '\(state=(?<State>[A-Za-z]+); IDEClosed=true; ' +
        'productionApproved=(?<Approved>True|False); ' +
        'needsRebaseline=(?<Rebaseline>True|False); ' +
        'vendor=(?<Vendor1Bytes>\d+)/(?<Vendor1Sha>[A-F0-9]{64}),' +
        '(?<Vendor2Bytes>\d+)/(?<Vendor2Sha>[A-F0-9]{64}); ' +
        'Classes=(?<ClassesBytes>\d+)/(?<ClassesSha>[A-F0-9]{64}); ' +
        'project=(?<ProjectBytes>\d+)/(?<ProjectSha>[A-F0-9]{64}); ' +
        'lcp=(?<LcpBytes>\d+)/(?<LcpSha>[A-F0-9]{64}); ' +
        'Includes=(?<Includes>.*?); ' +
        'TCP=(?<TcpSha>[A-F0-9]{64}); ' +
        'Network=(?<NetworkCount>\d+)/(?<NetworkSha>[A-F0-9]{64}),' +
        'tracked=(?<TrackedNetworkCount>\d+)/' +
        '(?<TrackedNetworkSha>[A-F0-9]{64}); ' +
        'protected=(?<Protected>.*?)\)$'
    $match = [regex]::Match(
        $lines[0],
        $pattern,
        [Text.RegularExpressions.RegexOptions]::CultureInvariant)
    if (-not $match.Success) {
        throw 'Verifier current-state line has an unexpected evidence format.'
    }
    $state = $match.Groups['State'].Value
    $approved = [bool]::Parse($match.Groups['Approved'].Value)
    $needsRebaseline = [bool]::Parse(
        $match.Groups['Rebaseline'].Value)
    $expectedPrefix = if ($ExpectedProductionApproved) { 'PASS' } else { 'CAPTURE' }
    if (($state -cne $ExpectedState) -or
        ($match.Groups['Prefix'].Value -cne $expectedPrefix) -or
        ($approved -ne $ExpectedProductionApproved) -or
        ($needsRebaseline -ne $ExpectedNeedsRebaseline)) {
        throw (
            'Verifier decision does not match the requested phase ratchet: ' +
            $lines[0])
    }
    return [ordered]@{
        authoritativeLine = $lines[0]
        state = $state
        productionApproved = $approved
        needsRebaseline = $needsRebaseline
        structuredEvidence = [ordered]@{
            vendor = @(
                [ordered]@{
                    bytes = [long]$match.Groups['Vendor1Bytes'].Value
                    sha256 = $match.Groups['Vendor1Sha'].Value
                },
                [ordered]@{
                    bytes = [long]$match.Groups['Vendor2Bytes'].Value
                    sha256 = $match.Groups['Vendor2Sha'].Value
                })
            classes = [ordered]@{
                bytes = [long]$match.Groups['ClassesBytes'].Value
                sha256 = $match.Groups['ClassesSha'].Value
            }
            project = [ordered]@{
                bytes = [long]$match.Groups['ProjectBytes'].Value
                sha256 = $match.Groups['ProjectSha'].Value
            }
            projectDefinition = [ordered]@{
                bytes = [long]$match.Groups['LcpBytes'].Value
                sha256 = $match.Groups['LcpSha'].Value
            }
            generatedIncludes = ConvertFrom-NamedFileEvidenceList `
                -Text $match.Groups['Includes'].Value `
                -Owner 'verifier generated Includes'
            tcpSha256 = $match.Groups['TcpSha'].Value
            network = [ordered]@{
                fullCount = [int]$match.Groups['NetworkCount'].Value
                fullSha256 = $match.Groups['NetworkSha'].Value
                trackedCount = [int]$match.Groups['TrackedNetworkCount'].Value
                trackedSha256 = $match.Groups['TrackedNetworkSha'].Value
            }
            protectedDependencies = ConvertFrom-NamedFileEvidenceList `
                -Text $match.Groups['Protected'].Value `
                -Owner 'verifier protected dependencies'
        }
    }
}

function ConvertFrom-NamedFileEvidenceList {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if ($Text -ceq '') {
        return @()
    }
    $observedNames = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    return @(
        foreach ($item in $Text.Split(',')) {
            $match = [regex]::Match(
                $item,
                '^(?<Name>[^=,]+)=(?<Bytes>\d+)/(?<Sha>[A-F0-9]{64})$',
                [Text.RegularExpressions.RegexOptions]::CultureInvariant)
            if (-not $match.Success) {
                throw "$Owner contains malformed file evidence: $item"
            }
            $name = $match.Groups['Name'].Value
            if (-not $observedNames.Add($name)) {
                throw "$Owner contains duplicate evidence name: $name"
            }
            [ordered]@{
                name = $name
                bytes = [long]$match.Groups['Bytes'].Value
                sha256 = $match.Groups['Sha'].Value
            }
        })
}

function Assert-PublicFileTupleMatches {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Expected,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Observed,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (([long]$Expected.bytes -ne [long]$Observed.bytes) -or
        ($Expected.sha256 -cne $Observed.sha256)) {
        throw (
            "$Owner verifier/capture raw evidence differs; verifier=" +
            "$($Expected.bytes)/$($Expected.sha256), capture=" +
            "$($Observed.bytes)/$($Observed.sha256)")
    }
}

function Assert-VerifierEvidenceMatchesCapture {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Decision,
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, object]]$ReadFiles,
        [Parameter(Mandatory = $true)][string[]]$VendorPaths,
        [Parameter(Mandatory = $true)][string]$ClassesPath,
        [Parameter(Mandatory = $true)][string]$ProjectDatabasePath,
        [Parameter(Mandatory = $true)][string]$ProjectDefinitionPath,
        [Parameter(Mandatory = $true)][string]$TcpPath,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$NetworkInventory,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$IncludeNameToPath,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$ProtectedNameToPath
    )

    $evidence = $Decision.structuredEvidence
    Assert-PublicFileTupleMatches `
        -Expected $evidence.vendor[0] `
        -Observed $ReadFiles[$VendorPaths[0]].Public `
        -Owner 'vendor _UDPTransceiver'
    Assert-PublicFileTupleMatches `
        -Expected $evidence.vendor[1] `
        -Observed $ReadFiles[$VendorPaths[1]].Public `
        -Owner 'vendor _UDPTransceiverInterface'
    Assert-PublicFileTupleMatches `
        -Expected $evidence.classes `
        -Observed $ReadFiles[$ClassesPath].Public `
        -Owner 'Classes.lcb'
    Assert-PublicFileTupleMatches `
        -Expected $evidence.project `
        -Observed $ReadFiles[$ProjectDatabasePath].Public `
        -Owner 'project lcb'
    Assert-PublicFileTupleMatches `
        -Expected $evidence.projectDefinition `
        -Observed $ReadFiles[$ProjectDefinitionPath].Public `
        -Owner 'project lcp'
    if ($evidence.tcpSha256 -cne $ReadFiles[$TcpPath].Public.sha256) {
        throw 'TCP verifier/capture raw SHA-256 differs.'
    }

    $includeExpectedNames = @($IncludeNameToPath.Keys | Sort-Object)
    $includeObservedNames = @(
        $evidence.generatedIncludes |
            ForEach-Object name |
            Sort-Object)
    if ([string]::Join("`n", $includeExpectedNames) -cne
        [string]::Join("`n", $includeObservedNames)) {
        throw 'Verifier generated Include evidence name set drifted.'
    }
    foreach ($item in $evidence.generatedIncludes) {
        $path = $IncludeNameToPath[$item.name]
        Assert-PublicFileTupleMatches `
            -Expected $item `
            -Observed $ReadFiles[$path].Public `
            -Owner "generated Include $($item.name)"
    }

    $protectedExpectedNames = @($ProtectedNameToPath.Keys | Sort-Object)
    $protectedObservedNames = @(
        $evidence.protectedDependencies |
            ForEach-Object name |
            Sort-Object)
    if ([string]::Join("`n", $protectedExpectedNames) -cne
        [string]::Join("`n", $protectedObservedNames)) {
        throw 'Verifier protected dependency evidence name set drifted.'
    }
    foreach ($item in $evidence.protectedDependencies) {
        $path = $ProtectedNameToPath[$item.name]
        Assert-PublicFileTupleMatches `
            -Expected $item `
            -Observed $ReadFiles[$path].Public `
            -Owner "protected dependency $($item.name)"
    }
    if (($evidence.network.fullCount -ne $NetworkInventory.unionCount) -or
        ($evidence.network.fullSha256 -cne
            $NetworkInventory.inventorySha256) -or
        ($evidence.network.trackedCount -ne
            $NetworkInventory.trackedCount) -or
        ($evidence.network.trackedSha256 -cne
            $NetworkInventory.trackedInventorySha256)) {
        throw 'Network verifier/capture inventory evidence differs.'
    }
    return [ordered]@{
        exactRawEvidenceCrossChecked = $true
        vendorCount = 2
        generatedIncludeCount = $includeObservedNames.Count
        protectedDependencyCount = $protectedObservedNames.Count
        networkUnionCount = $NetworkInventory.unionCount
        note = (
            'Verifier structured stdout evidence was parsed and matched to ' +
            'the independently captured raw artifact hashes and inventories.')
    }
}

function Assert-VerifiedJsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][byte[]]$ExpectedBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedPhase,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $null = Assert-PathComponentsNoReparse -Root $Root -Path $Path
    $actualBytes = [IO.File]::ReadAllBytes($Path)
    $expectedSha256 = Get-BytesSha256 -Bytes $ExpectedBytes
    $actualSha256 = Get-BytesSha256 -Bytes $actualBytes
    if (($actualBytes.Length -ne $ExpectedBytes.Length) -or
        ($actualSha256 -cne $expectedSha256) -or
        (-not [Security.Cryptography.CryptographicOperations]::FixedTimeEquals(
            $actualBytes,
            $ExpectedBytes))) {
        throw (
            "$Owner raw reread differs; expected " +
            "$($ExpectedBytes.Length)/$expectedSha256, observed " +
            "$($actualBytes.Length)/$actualSha256")
    }
    $traits = Get-RawTextTraits -Bytes $actualBytes
    if ((-not $traits.is7BitAscii) -or
        ($traits.bom -cne 'None') -or
        ($traits.eolStyle -cne 'LF')) {
        throw "$Owner must be strict ASCII JSON with no BOM and LF EOL."
    }
    $text = $Utf8NoBom.GetString($actualBytes)
    $document = $null
    try {
        $document = [Text.Json.JsonDocument]::Parse($text)
        $rootElement = $document.RootElement
        if ($rootElement.ValueKind -ne [Text.Json.JsonValueKind]::Object) {
            throw "$Owner JSON root is not an object."
        }
        $phaseElement = [Text.Json.JsonElement]::new()
        if ((-not $rootElement.TryGetProperty('phase', [ref]$phaseElement)) -or
            ($phaseElement.GetString() -cne $ExpectedPhase)) {
            throw "$Owner JSON phase differs from the requested phase."
        }
    }
    catch {
        throw "$Owner strict JSON verification failed: $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $document) {
            $document.Dispose()
        }
    }
    $sealEvidence = Assert-ManifestSealBytes -Bytes $actualBytes -Owner $Owner
    return [ordered]@{
        bytes = $actualBytes.Length
        sha256 = $actualSha256
        exactByteEquality = $true
        strictJsonParsed = $true
        sealValidated = [bool]$sealEvidence.valid
        sealSha256 = $sealEvidence.sealSha256
        phase = $ExpectedPhase
    }
}

function Get-CheckpointOrphanStages {
    param(
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][string]$ManifestPath
    )

    $prefix = '.' + [IO.Path]::GetFileName($ManifestPath) + '.'
    return @(
        Get-ChildItem -LiteralPath $EvidenceDirectory -File -Force |
            Where-Object {
                $_.Name.StartsWith($prefix, [StringComparison]::Ordinal) -and
                $_.Name.EndsWith('.tmp', [StringComparison]::Ordinal)
            } |
            Sort-Object Name)
}

function New-VerifiedJsonStage {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][byte[]]$ManifestBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedPhase
    )

    $directory = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $EvidenceDirectory
    $manifestFull = [IO.Path]::GetFullPath($ManifestPath)
    $expectedManifest = Join-Path $directory ([IO.Path]::GetFileName($ManifestPath))
    if (-not [string]::Equals(
            $manifestFull,
            [IO.Path]::GetFullPath($expectedManifest),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Atomic checkpoint output escaped the evidence directory.'
    }
    $null = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $manifestFull `
        -AllowMissingLeaf
    if ([IO.File]::Exists($manifestFull) -or
        [IO.Directory]::Exists($manifestFull)) {
        throw "Checkpoint manifest already exists: $manifestFull"
    }

    $orphans = @(Get-CheckpointOrphanStages `
            -EvidenceDirectory $directory `
            -ManifestPath $manifestFull)
    if ($orphans.Count -ne 0) {
        throw (
            'Orphan checkpoint stage requires explicit manual inspection; ' +
            'no automatic deletion is permitted: ' +
            [string]::Join(',', @($orphans.Name)))
    }
    $tempName = '.{0}.{1}.{2}.tmp' -f
        [IO.Path]::GetFileName($manifestFull),
        $PID,
        [Guid]::NewGuid().ToString('N')
    $tempPath = Join-Path $directory $tempName
    $completed = $false
    try {
        $null = Assert-PathComponentsNoReparse `
            -Root $Root `
            -Path $tempPath `
            -AllowMissingLeaf
        $stream = [IO.File]::Open(
            $tempPath,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        try {
            $stream.Write($ManifestBytes, 0, $ManifestBytes.Length)
            $stream.Flush($true)
        }
        finally {
            $stream.Dispose()
        }
        $tempEvidence = Assert-VerifiedJsonFile `
            -Root $Root `
            -Path $tempPath `
            -ExpectedBytes $ManifestBytes `
            -ExpectedPhase $ExpectedPhase `
            -Owner 'temporary checkpoint'

        $completed = $true
        return [ordered]@{
            outputPath = $manifestFull
            tempPath = $tempPath
            tempCreateNewVerified = $tempEvidence
            finalCommitPending = $true
        }
    }
    finally {
        if ((-not $completed) -and [IO.File]::Exists($tempPath)) {
            Write-Warning (
                'Checkpoint stage was not automatically deleted after failure. ' +
                "Inspect explicitly before retry: $tempPath")
        }
    }
}

function Publish-VerifiedJsonStage {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Stage,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()][byte[]]$ManifestBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedPhase
    )

    $directory = Assert-PathComponentsNoReparse `
        -Root $Root `
        -Path $EvidenceDirectory
    $tempPath = [IO.Path]::GetFullPath([string]$Stage.tempPath)
    $manifestPath = [IO.Path]::GetFullPath([string]$Stage.outputPath)
    if (([IO.Path]::GetDirectoryName($tempPath) -cne $directory) -or
        ([IO.Path]::GetDirectoryName($manifestPath) -cne $directory)) {
        throw 'Checkpoint stage or final path escaped the evidence directory.'
    }
    $stageEvidence = Assert-VerifiedJsonFile `
        -Root $Root `
        -Path $tempPath `
        -ExpectedBytes $ManifestBytes `
        -ExpectedPhase $ExpectedPhase `
        -Owner 'final checkpoint stage'
    if ([IO.File]::Exists($manifestPath) -or
        [IO.Directory]::Exists($manifestPath)) {
        throw "Checkpoint appeared before final commit: $manifestPath"
    }
    [IO.File]::Move($tempPath, $manifestPath, $false)
    $finalEvidence = Assert-VerifiedJsonFile `
        -Root $Root `
        -Path $manifestPath `
        -ExpectedBytes $ManifestBytes `
        -ExpectedPhase $ExpectedPhase `
        -Owner 'committed checkpoint'
    return [ordered]@{
        outputPath = $manifestPath
        stageRereadAtCommit = $stageEvidence
        atomicMoveOverwrite = $false
        atomicMoveWasFinalExternalStateCommitPoint = $true
        finalReread = $finalEvidence
    }
}

function Invoke-CaptureToolSelfTest {
    $positive = 0
    $negative = 0
    $pin = Invoke-CanonicalAsciiPinSelfTest
    $positive += $pin.acceptedPositiveCount
    $negative += $pin.rejectedNegativeCount

    $hashA = 'A' * 64
    $hashB = 'B' * 64
    $hashC = 'C' * 64
    $line =
        'CAPTURE LASAL.UdpCallbackContract.Current ' +
        '(state=DerivedWired; IDEClosed=true; productionApproved=False; ' +
        'needsRebaseline=True; vendor=1/' + $hashA + ',2/' + $hashB +
        '; Classes=3/' + $hashC + '; project=4/' + $hashA +
        '; lcp=5/' + $hashB + '; Includes=C_channels.h=6/' + $hashC +
        ',channels.h=7/' + $hashA + ',lslpublictypes.h=8/' + $hashB +
        '; TCP=' + $hashC + '; Network=9/' + $hashA +
        ',tracked=10/' + $hashB + '; protected=_StdLib=11/' + $hashC +
        ',CriticalSection=12/' + $hashA + ',lsl_st_tcp_user.h=13/' +
        $hashB + ')'
    $decision = Get-CurrentDecisionFromVerifierOutput `
        -Output ($line + "`n") `
        -ExpectedState 'DerivedWired' `
        -ExpectedProductionApproved $false `
        -ExpectedNeedsRebaseline $true
    if (($decision.state -cne 'DerivedWired') -or
        ($decision.structuredEvidence.network.fullCount -ne 9) -or
        ($decision.structuredEvidence.generatedIncludes.Count -ne 3)) {
        throw 'Synthetic verifier evidence positive did not parse exactly.'
    }
    $positive++
    try {
        $null = Get-CurrentDecisionFromVerifierOutput `
            -Output (($line.Replace('Network=9/', 'Network=9/DEAD')) + "`n") `
            -ExpectedState 'DerivedWired' `
            -ExpectedProductionApproved $false `
            -ExpectedNeedsRebaseline $true
        throw 'Malformed synthetic verifier evidence was accepted.'
    }
    catch {
        if ($_.Exception.Message -ceq
            'Malformed synthetic verifier evidence was accepted.') {
            throw
        }
        $negative++
    }

    $sealed = ConvertTo-SealedManifestBytes -Manifest ([ordered]@{
            schema = 'Synthetic/v1'
            phase = 'Synthetic'
            value = 7
        })
    $sealCheck = Assert-ManifestSealBytes `
        -Bytes $sealed.Bytes `
        -Owner 'synthetic sealed manifest'
    if ((-not $sealCheck.valid) -or
        ($sealCheck.sealSha256 -cne $sealed.Public.sealSha256)) {
        throw 'Synthetic sealed manifest did not validate exactly.'
    }
    $positive++
    $mutated = [byte[]]$sealed.Bytes.Clone()
    $mutationIndex = [Array]::IndexOf($mutated, [byte][char]'7')
    if ($mutationIndex -lt 0) {
        throw 'Synthetic seal mutation byte was not found.'
    }
    $mutated[$mutationIndex] = [byte][char]'8'
    try {
        $null = Assert-ManifestSealBytes `
            -Bytes $mutated `
            -Owner 'synthetic mutated manifest'
        throw 'Synthetic mutated seal was accepted.'
    }
    catch {
        if ($_.Exception.Message -ceq 'Synthetic mutated seal was accepted.') {
            throw
        }
        $negative++
    }

    $duplicateLength = 0
    $duplicatePlaceholderBytes = $null
    foreach ($attempt in 1..8) {
        $duplicateText = [string]::Join("`n", @(
                '{',
                '  "phase": "Synthetic",',
                '  "phase": "Synthetic",',
                '  "integrity": {',
                '    "algorithm": "SHA-256",',
                ('    "canonicalization": "exact UTF-8 ASCII/LF JSON ' +
                    'bytes with sealSha256 set to 64 zeros",'),
                "    `"sealedPayloadBytes`": $duplicateLength,",
                ('    "sealSha256": "' + ('0' * 64) + '"'),
                '  }',
                '}')) + "`n"
        $duplicatePlaceholderBytes = $Utf8NoBom.GetBytes($duplicateText)
        if ($duplicateLength -eq $duplicatePlaceholderBytes.Length) {
            break
        }
        $duplicateLength = $duplicatePlaceholderBytes.Length
    }
    if ($duplicateLength -ne $duplicatePlaceholderBytes.Length) {
        throw 'Synthetic duplicate-key seal length did not converge.'
    }
    $duplicateSeal = Get-BytesSha256 -Bytes $duplicatePlaceholderBytes
    $duplicateFinalText = $duplicateText.Replace(
        '"sealSha256": "' + ('0' * 64) + '"',
        '"sealSha256": "' + $duplicateSeal + '"')
    $duplicateBytes = $Utf8NoBom.GetBytes($duplicateFinalText)
    try {
        $null = Assert-ManifestSealBytes `
            -Bytes $duplicateBytes `
            -Owner 'synthetic duplicate-key manifest'
        throw 'Synthetic duplicate JSON property was accepted.'
    }
    catch {
        if ($_.Exception.Message -ceq
            'Synthetic duplicate JSON property was accepted.') {
            throw
        }
        if ($_.Exception.Message -notmatch
            "duplicate JSON property 'phase'") {
            throw
        }
        $negative++
    }

    $minimal = ConvertTo-SealedManifestBytes -Manifest ([ordered]@{
            schema = 'LasalUdpCallbackGateBCheckpoint/v2'
            phase = 'GateA_VendorImported'
            observedAt = '2026-08-07T00:00:00.0000000+09:00'
            lineage = [ordered]@{
                sequence = 0
                parent = $null
                rootGateA = $null
                validatedAncestorCount = 0
            }
            targetProject = [ordered]@{
                path = $TargetRelativeRoot
                compilerVersion = 'C78'
                targetArchitecture = 'ARM'
            }
            verifierDecision = [ordered]@{
                state = 'VendorImported'
                productionApproved = $true
                needsRebaseline = $false
            }
            approvalRatchet = [ordered]@{
                productionApproved = $true
                needsRebaseline = $false
            }
            captureSafety = [ordered]@{
                outputFile =
                    $PhaseContracts.GateA_VendorImported.OutputFile
            }
            git = [ordered]@{ head = 'B' * 40 }
            tooling = [ordered]@{
                trust = [ordered]@{
                    trustedCommittedHead = $true
                    mode = 'committed-clean'
                }
                verifierCanonicalPin = [ordered]@{
                    canonicalLfSha256 = $hashC
                    pinSource = 'committed-reviewed-pin'
                }
            }
        })
    $minimalData = $Utf8NoBom.GetString($minimal.Bytes) |
        ConvertFrom-Json -AsHashtable -Depth 50
    try {
        Assert-CheckpointManifestContract `
            -Data $minimalData `
            -ExpectedPhase 'GateA_VendorImported' `
            -ExpectedContract $PhaseContracts.GateA_VendorImported `
            -SealEvidence $minimal.Public `
            -RepositoryRoot ([IO.Path]::GetTempPath()) `
            -GitPath 'git' `
            -RepositoryBindingHead ('B' * 40) `
            -ExpectedParentFile $null `
            -ExpectedParentData $null `
            -ExpectedRootFile $null
        throw 'Minimal self-asserted predecessor was accepted.'
    }
    catch {
        if ($_.Exception.Message -ceq
            'Minimal self-asserted predecessor was accepted.') {
            throw
        }
        $negative++
    }

    $lfHeaderBytes = $Utf8NoBom.GetBytes("A`n")
    $lfHeader = [pscustomobject]@{
        RawBytes = $lfHeaderBytes
        Public = [ordered]@{
            path = 'synthetic.h'
            text = Get-RawTextTraits -Bytes $lfHeaderBytes
        }
    }
    Assert-KnownTextHeader -File $lfHeader -Owner 'synthetic LF header'
    $positive++
    $crHeaderBytes = $Utf8NoBom.GetBytes("A`r")
    $crHeader = [pscustomobject]@{
        RawBytes = $crHeaderBytes
        Public = [ordered]@{
            path = 'synthetic.h'
            text = Get-RawTextTraits -Bytes $crHeaderBytes
        }
    }
    try {
        Assert-KnownTextHeader -File $crHeader -Owner 'synthetic CR header'
        throw 'Synthetic bare-CR header was accepted.'
    }
    catch {
        if ($_.Exception.Message -ceq
            'Synthetic bare-CR header was accepted.') {
            throw
        }
        $negative++
    }

    $tempBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    $testRoot = [IO.Path]::GetFullPath((Join-Path $tempBase (
                'ElmoUdpCheckpointSelfTest_' + [Guid]::NewGuid().ToString('N'))))
    if (([IO.Path]::GetDirectoryName($testRoot) -cne $tempBase) -or
        (-not [IO.Path]::GetFileName($testRoot).StartsWith(
                'ElmoUdpCheckpointSelfTest_',
                [StringComparison]::Ordinal))) {
        throw 'Synthetic self-test directory scope validation failed.'
    }
    $null = [IO.Directory]::CreateDirectory($testRoot)
    $output = Join-Path $testRoot 'checkpoint.json'
    try {
        $stage = New-VerifiedJsonStage `
            -Root $testRoot `
            -EvidenceDirectory $testRoot `
            -ManifestPath $output `
            -ManifestBytes $sealed.Bytes `
            -ExpectedPhase 'Synthetic'
        if ([IO.File]::Exists($output) -or
            (-not [IO.File]::Exists($stage.tempPath))) {
            throw 'Synthetic stage became visible as a final manifest.'
        }
        $written = Publish-VerifiedJsonStage `
            -Root $testRoot `
            -EvidenceDirectory $testRoot `
            -Stage $stage `
            -ManifestBytes $sealed.Bytes `
            -ExpectedPhase 'Synthetic'
        if ((-not $written.finalReread.exactByteEquality) -or
            (-not $written.finalReread.strictJsonParsed) -or
            (-not $written.finalReread.sealValidated) -or
            (-not $written.atomicMoveWasFinalExternalStateCommitPoint)) {
            throw 'Synthetic atomic publish did not verify its final reread.'
        }
        $positive++
        try {
            $null = New-VerifiedJsonStage `
                -Root $testRoot `
                -EvidenceDirectory $testRoot `
                -ManifestPath $output `
                -ManifestBytes $sealed.Bytes `
                -ExpectedPhase 'Synthetic'
            throw 'Synthetic atomic overwrite was accepted.'
        }
        catch {
            if ($_.Exception.Message -ceq
                'Synthetic atomic overwrite was accepted.') {
                throw
            }
            $negative++
        }
        [IO.File]::Delete($output)

        $orphan = Join-Path $testRoot '.checkpoint.json.synthetic.tmp'
        [IO.File]::WriteAllBytes($orphan, [byte[]](1, 2, 3))
        try {
            $null = New-VerifiedJsonStage `
                -Root $testRoot `
                -EvidenceDirectory $testRoot `
                -ManifestPath $output `
                -ManifestBytes $sealed.Bytes `
                -ExpectedPhase 'Synthetic'
            throw 'Synthetic orphan stage was ignored.'
        }
        catch {
            if ($_.Exception.Message -ceq 'Synthetic orphan stage was ignored.') {
                throw
            }
            $negative++
        }
        [IO.File]::Delete($orphan)

        $retained = New-VerifiedJsonStage `
            -Root $testRoot `
            -EvidenceDirectory $testRoot `
            -ManifestPath $output `
            -ManifestBytes $sealed.Bytes `
            -ExpectedPhase 'Synthetic'
        if ([IO.File]::Exists($output) -or
            (-not [IO.File]::Exists($retained.tempPath))) {
            throw 'Synthetic failed-guard stage semantics drifted.'
        }
        $positive++
        [IO.File]::Delete([string]$retained.tempPath)

        $gitRoot = Join-Path $testRoot 'git-trust'
        $null = [IO.Directory]::CreateDirectory($gitRoot)
        $toolPath = Join-Path $gitRoot 'tool.ps1'
        $toolBytes = $Utf8NoBom.GetBytes("Write-Output 'ok'`n")
        [IO.File]::WriteAllBytes($toolPath, $toolBytes)
        foreach ($arguments in @(
                @('-C', $gitRoot, 'init', '--quiet'),
                @('-C', $gitRoot, 'config', 'user.name', 'Synthetic Test'),
                @('-C', $gitRoot, 'config', 'user.email', 'synthetic@example.invalid'),
                @('-C', $gitRoot, 'add', '--', 'tool.ps1'),
                @('-C', $gitRoot, 'commit', '--quiet', '-m', 'synthetic'))) {
            $command = Invoke-ProcessCapture -FileName 'git' -Arguments $arguments
            Assert-CommandPassed -Result $command -Owner 'synthetic Git setup'
        }
        $headCommand = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @('-C', $gitRoot, 'rev-parse', 'HEAD')
        Assert-CommandPassed -Result $headCommand -Owner 'synthetic Git HEAD'
        $head = $headCommand.Stdout.Trim().ToUpperInvariant()
        $normalIdentity = Get-CommittedPathIdentity `
            -Root $gitRoot `
            -GitPath 'git' `
            -Path 'tool.ps1' `
            -StartHead $head `
            -RequireAsciiCanonical
        if (-not $normalIdentity.committedExact) {
            throw 'Synthetic normal stage-0 tool was not trusted.'
        }
        $positive++
        $historicalTool = Get-GitBlobEvidence `
            -Root $gitRoot `
            -GitPath 'git' `
            -Commit $head `
            -Path 'tool.ps1' `
            -Owner 'synthetic historical tool'
        if (($historicalTool.blobOid -cne $normalIdentity.headBlobOid) -or
            ([long]$historicalTool.bytes -ne [long]$toolBytes.Length) -or
            ($historicalTool.sha256 -cne (Get-BytesSha256 -Bytes $toolBytes))) {
            throw 'Synthetic raw historical tool evidence drifted.'
        }
        $positive++
        $gitSnapshot = Get-GitStateSnapshot `
            -Root $gitRoot `
            -GitPath 'git' `
            -GatedPathspec @('tool.ps1')
        Assert-GitSnapshotEvidence `
            -Snapshot $gitSnapshot `
            -Owner 'synthetic full Git snapshot'
        $positive++
        $hadGitIndexFile = Test-Path -LiteralPath 'Env:GIT_INDEX_FILE'
        $originalGitIndexFile = $env:GIT_INDEX_FILE
        try {
            $env:GIT_INDEX_FILE = Join-Path $testRoot 'attacker.index'
            $sanitizedSnapshot = Get-GitStateSnapshot `
                -Root $gitRoot `
                -GitPath 'git' `
                -GatedPathspec @('tool.ps1')
            $sanitizedIdentity = Get-CommittedPathIdentity `
                -Root $gitRoot `
                -GitPath 'git' `
                -Path 'tool.ps1' `
                -StartHead $head `
                -RequireAsciiCanonical
            if (($sanitizedSnapshot.indexRawTextSha256 -cne
                    $gitSnapshot.indexRawTextSha256) -or
                ($sanitizedSnapshot.trackedPathRawTextSha256 -cne
                    $gitSnapshot.trackedPathRawTextSha256) -or
                ($sanitizedSnapshot.statusRawTextSha256 -cne
                    $gitSnapshot.statusRawTextSha256) -or
                (-not $sanitizedIdentity.committedExact) -or
                [IO.File]::Exists($env:GIT_INDEX_FILE)) {
                throw 'Synthetic alternate Git index environment was honored.'
            }
            $negative++
        }
        finally {
            if ($hadGitIndexFile) {
                $env:GIT_INDEX_FILE = $originalGitIndexFile
            }
            else {
                Remove-Item -LiteralPath 'Env:GIT_INDEX_FILE' -ErrorAction SilentlyContinue
            }
        }
        $badGitSnapshot = $gitSnapshot | ConvertTo-Json -Depth 20 |
            ConvertFrom-Json -AsHashtable -Depth 20
        $badGitSnapshot.indexRawTextSha256 = $hashA
        try {
            Assert-GitSnapshotEvidence `
                -Snapshot $badGitSnapshot `
                -Owner 'synthetic broken Git snapshot'
            throw 'Synthetic broken Git raw hash was accepted.'
        }
        catch {
            if ($_.Exception.Message -ceq
                'Synthetic broken Git raw hash was accepted.') {
                throw
            }
            $negative++
        }

        $gateAHead = $head
        [IO.File]::WriteAllBytes(
            (Join-Path $gitRoot 'LMCUdpCallbackSender.st'),
            $Utf8NoBom.GetBytes("// derived sender`n"))
        foreach ($arguments in @(
                @('-C', $gitRoot, 'add', '--', 'LMCUdpCallbackSender.st'),
                @('-C', $gitRoot, 'commit', '--quiet', '-m', 'gate-b1'))) {
            $command = Invoke-ProcessCapture -FileName 'git' -Arguments $arguments
            Assert-CommandPassed -Result $command -Owner 'synthetic Gate B1 commit'
        }
        $gateB1HeadResult = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @('-C', $gitRoot, 'rev-parse', 'HEAD')
        Assert-CommandPassed `
            -Result $gateB1HeadResult `
            -Owner 'synthetic Gate B1 HEAD'
        $gateB1Head = $gateB1HeadResult.Stdout.Trim().ToUpperInvariant()
        $derivedBlobResult = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'rev-parse',
                "$gateB1Head`:LMCUdpCallbackSender.st")
        Assert-CommandPassed `
            -Result $derivedBlobResult `
            -Owner 'synthetic derived sender blob lookup'
        $blobReplacement = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'replace',
                $normalIdentity.headBlobOid,
                $derivedBlobResult.Stdout.Trim())
        Assert-CommandPassed `
            -Result $blobReplacement `
            -Owner 'synthetic blob replacement setup'
        $replacementSafeTool = Get-GitBlobEvidence `
            -Root $gitRoot `
            -GitPath 'git' `
            -Commit $gateAHead `
            -Path 'tool.ps1' `
            -Owner 'synthetic replacement-safe historical tool'
        if (($replacementSafeTool.blobOid -cne $normalIdentity.headBlobOid) -or
            ($replacementSafeTool.sha256 -cne
                (Get-BytesSha256 -Bytes $toolBytes))) {
            throw 'Synthetic Git blob replacement influenced raw evidence.'
        }
        $negative++
        $syntheticB1Parent = [ordered]@{
            phase = 'GateB1_DerivedDeclaration'
            lineage = [ordered]@{
                sequence = 1
                rootGateA = [ordered]@{
                    artifactBindingHead = $gateAHead
                }
            }
        }
        $resolvedGateAHead = Get-GateAArtifactBindingHeadForPhase `
            -CurrentPhase 'GateB2_DerivedWired' `
            -StartHead $gateB1Head `
            -Root $gitRoot `
            -GitPath 'git' `
            -ImmediateParentData $syntheticB1Parent
        $gateAAbsent = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'cat-file', '-e',
                "$resolvedGateAHead`:LMCUdpCallbackSender.st")
        $gateB1Present = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'cat-file', '-e',
                "$gateB1Head`:LMCUdpCallbackSender.st")
        if (($resolvedGateAHead -cne $gateAHead) -or
            ($gateAAbsent.ExitCode -eq 0) -or
            ($gateB1Present.ExitCode -ne 0)) {
            throw (
                'Synthetic Gate A -> B1 -> B2 artifact binding conflated ' +
                'the absent and present derived states.')
        }
        $positive++
        $badSyntheticB1Parent = [ordered]@{
            phase = 'GateB1_DerivedDeclaration'
            lineage = [ordered]@{
                sequence = 1
                rootGateA = [ordered]@{
                    artifactBindingHead = $gateB1Head
                }
            }
        }
        try {
            $null = Get-GateAArtifactBindingHeadForPhase `
                -CurrentPhase 'GateB2_DerivedWired' `
                -StartHead $gateB1Head `
                -Root $gitRoot `
                -GitPath 'git' `
                -ImmediateParentData $badSyntheticB1Parent
            throw 'Synthetic current-HEAD Gate A rebinding was accepted.'
        }
        catch {
            if ($_.Exception.Message -ceq
                'Synthetic current-HEAD Gate A rebinding was accepted.') {
                throw
            }
            $negative++
        }
        $gateATreeResult = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @('-C', $gitRoot, 'rev-parse', "$gateAHead^{tree}")
        Assert-CommandPassed `
            -Result $gateATreeResult `
            -Owner 'synthetic Gate A tree'
        $unrelatedCommitResult = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'commit-tree',
                $gateATreeResult.Stdout.Trim(), '-m', 'unrelated Gate A')
        Assert-CommandPassed `
            -Result $unrelatedCommitResult `
            -Owner 'synthetic unrelated Gate A commit'
        $unrelatedGateAHead =
            $unrelatedCommitResult.Stdout.Trim().ToUpperInvariant()
        $unrelatedSyntheticB1Parent = [ordered]@{
            phase = 'GateB1_DerivedDeclaration'
            lineage = [ordered]@{
                sequence = 1
                rootGateA = [ordered]@{
                    artifactBindingHead = $unrelatedGateAHead
                }
            }
        }
        try {
            $null = Get-GateAArtifactBindingHeadForPhase `
                -CurrentPhase 'GateB2_DerivedWired' `
                -StartHead $gateB1Head `
                -Root $gitRoot `
                -GitPath 'git' `
                -ImmediateParentData $unrelatedSyntheticB1Parent
            throw 'Synthetic unrelated Gate A commit was accepted.'
        }
        catch {
            if ($_.Exception.Message -ceq
                'Synthetic unrelated Gate A commit was accepted.') {
                throw
            }
            $negative++
        }
        $commitReplacement = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'replace', '--graft',
                $gateB1Head, $unrelatedGateAHead)
        Assert-CommandPassed `
            -Result $commitReplacement `
            -Owner 'synthetic commit replacement graft setup'
        try {
            $null = Get-GateAArtifactBindingHeadForPhase `
                -CurrentPhase 'GateB2_DerivedWired' `
                -StartHead $gateB1Head `
                -Root $gitRoot `
                -GitPath 'git' `
                -ImmediateParentData $unrelatedSyntheticB1Parent
            throw 'Synthetic replacement-grafted ancestry was accepted.'
        }
        catch {
            if ($_.Exception.Message -ceq
                'Synthetic replacement-grafted ancestry was accepted.') {
                throw
            }
            $negative++
        }
        $legacyGraftsPath = Join-Path `
            $gitSnapshot.repositoryContext.gitCommonDirectory `
            'info/grafts'
        [IO.File]::WriteAllBytes(
            $legacyGraftsPath,
            $Utf8NoBom.GetBytes("$gateB1Head $unrelatedGateAHead`n"))
        try {
            try {
                $null = Get-GateAArtifactBindingHeadForPhase `
                    -CurrentPhase 'GateB2_DerivedWired' `
                    -StartHead $gateB1Head `
                    -Root $gitRoot `
                    -GitPath 'git' `
                    -ImmediateParentData $unrelatedSyntheticB1Parent
                throw 'Synthetic legacy info/grafts ancestry was accepted.'
            }
            catch {
                if ($_.Exception.Message -ceq
                    'Synthetic legacy info/grafts ancestry was accepted.') {
                    throw
                }
                $negative++
            }
        }
        finally {
            if ([IO.File]::Exists($legacyGraftsPath)) {
                [IO.File]::Delete($legacyGraftsPath)
            }
        }

        $alternateWorktree = Join-Path $testRoot 'attacker-worktree'
        $null = [IO.Directory]::CreateDirectory($alternateWorktree)
        $setCoreWorktree = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'config', 'core.worktree',
                $alternateWorktree)
        Assert-CommandPassed `
            -Result $setCoreWorktree `
            -Owner 'synthetic core.worktree redirect setup'
        try {
            try {
                $null = Get-GitStateSnapshot `
                    -Root $gitRoot `
                    -GitPath 'git' `
                    -GatedPathspec @('tool.ps1')
                throw 'Synthetic core.worktree redirect was accepted.'
            }
            catch {
                if ($_.Exception.Message -ceq
                    'Synthetic core.worktree redirect was accepted.') {
                    throw
                }
                $negative++
            }
        }
        finally {
            $clearCoreWorktree = Invoke-ProcessCapture `
                -FileName 'git' `
                -Arguments @('-C', $gitRoot, 'config', '--unset', 'core.worktree')
            Assert-CommandPassed `
                -Result $clearCoreWorktree `
                -Owner 'synthetic core.worktree redirect cleanup'
        }

        $assume = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'update-index', '--assume-unchanged',
                'tool.ps1')
        Assert-CommandPassed -Result $assume -Owner 'synthetic assume-unchanged setup'
        [IO.File]::WriteAllBytes(
            $toolPath,
            $Utf8NoBom.GetBytes("Write-Output 'changed'`n"))
        $assumeIdentity = Get-CommittedPathIdentity `
            -Root $gitRoot `
            -GitPath 'git' `
            -Path 'tool.ps1' `
            -StartHead $head `
            -PermitUncommitted `
            -RequireAsciiCanonical
        if ($assumeIdentity.committedExact) {
            throw 'Synthetic assume-unchanged modification was trusted.'
        }
        $negative++
        $clearAssume = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'update-index', '--no-assume-unchanged',
                'tool.ps1')
        Assert-CommandPassed `
            -Result $clearAssume `
            -Owner 'synthetic assume-unchanged cleanup'
        [IO.File]::WriteAllBytes($toolPath, $toolBytes)

        $skip = Invoke-ProcessCapture `
            -FileName 'git' `
            -Arguments @(
                '-C', $gitRoot, 'update-index', '--skip-worktree',
                'tool.ps1')
        Assert-CommandPassed -Result $skip -Owner 'synthetic skip-worktree setup'
        [IO.File]::WriteAllBytes(
            $toolPath,
            $Utf8NoBom.GetBytes("Write-Output 'skip changed'`n"))
        $skipIdentity = Get-CommittedPathIdentity `
            -Root $gitRoot `
            -GitPath 'git' `
            -Path 'tool.ps1' `
            -StartHead $head `
            -PermitUncommitted `
            -RequireAsciiCanonical
        if ($skipIdentity.committedExact) {
            throw 'Synthetic skip-worktree modification was trusted.'
        }
        $negative++
    }
    finally {
        if ([IO.Directory]::Exists($testRoot)) {
            $resolved = [IO.Path]::GetFullPath($testRoot)
            if (([IO.Path]::GetDirectoryName($resolved) -cne $tempBase) -or
                (-not [IO.Path]::GetFileName($resolved).StartsWith(
                        'ElmoUdpCheckpointSelfTest_',
                        [StringComparison]::Ordinal))) {
                throw 'Refusing unsafe synthetic self-test cleanup.'
            }
            foreach ($file in [IO.Directory]::EnumerateFiles(
                    $resolved,
                    '*',
                    [IO.SearchOption]::AllDirectories)) {
                [IO.File]::SetAttributes($file, [IO.FileAttributes]::Normal)
            }
            foreach ($directory in @(
                    [IO.Directory]::EnumerateDirectories(
                        $resolved,
                        '*',
                        [IO.SearchOption]::AllDirectories) |
                        Sort-Object Length -Descending)) {
                [IO.File]::SetAttributes(
                    $directory,
                    [IO.FileAttributes]::Directory)
            }
            [IO.File]::SetAttributes($resolved, [IO.FileAttributes]::Directory)
            [IO.Directory]::Delete($resolved, $true)
        }
    }
    return [ordered]@{
        positiveCount = $positive
        negativeCount = $negative
        canonicalPin = $pin
    }
}

function Assert-OptionalPathPresenceStable {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][bool]$ExpectedPresent
    )

    $fullPath = [IO.Path]::GetFullPath((Join-Path $Root (
                $RelativePath.Replace('/', '\'))))
    $present = [IO.File]::Exists($fullPath)
    if ($present -ne $ExpectedPresent) {
        throw "Optional path presence changed during capture: $RelativePath"
    }
    if ($present) {
        $null = Assert-PathComponentsNoReparse -Root $Root -Path $fullPath
    }
}

function Get-CompactGitStateEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [Collections.IDictionary]$Snapshot
    )

    return [ordered]@{
        head = $Snapshot.head
        indexEntryCount = $Snapshot.indexEntryCount
        indexRawTextSha256 = $Snapshot.indexRawTextSha256
        statusEntryCount = $Snapshot.statusEntryCount
        statusRawTextSha256 = $Snapshot.statusRawTextSha256
        targetTrackedCount = $Snapshot.targetWorktree.trackedCount
        targetNonIgnoredUntrackedCount =
            $Snapshot.targetWorktree.nonIgnoredUntrackedCount
        targetUnionCount = $Snapshot.targetWorktree.unionCount
        targetWorktreeIdentitySha256 =
            $Snapshot.targetWorktree.identitySha256
    }
}

$selectedOperationCount = @(
    $Capture.IsPresent,
    $ValidateOnly.IsPresent,
    $RunSelfTest.IsPresent |
        Where-Object { $_ }).Count
if ($selectedOperationCount -ne 1) {
    throw 'Select exactly one operation: -Capture, -ValidateOnly, or -RunSelfTest.'
}
if ($RunSelfTest) {
    if ($AllowUncommittedToolBootstrap -or $Phase -or $OutputPath) {
        throw '-RunSelfTest cannot be combined with phase, output, or bootstrap options.'
    }
    $result = Invoke-CaptureToolSelfTest
    Write-Output (
        'PASS LASAL.UdpCallbackGateBCheckpoint.SelfTest ' +
        "(positive=$($result.positiveCount); negative=$($result.negativeCount); " +
        'productionRead=false; manifestCreated=false)')
    return
}
if ([string]::IsNullOrWhiteSpace($Phase) -or
    [string]::IsNullOrWhiteSpace($OutputPath)) {
    throw '-Phase and -OutputPath are required for capture or validation.'
}
if ($AllowUncommittedToolBootstrap -and (-not $ValidateOnly)) {
    throw '-AllowUncommittedToolBootstrap is restricted to -ValidateOnly.'
}

$phaseContract = $PhaseContracts[$Phase]
$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$null = Assert-PathComponentsNoReparse -Root $root -Path $root
$evidenceDirectory = Resolve-ExactEvidenceDirectory `
    -Root $root `
    -RequestedPath $OutputPath
$manifestPath = Join-Path $evidenceDirectory $phaseContract.OutputFile
if ([IO.Directory]::Exists($manifestPath)) {
    throw "Checkpoint output path is an existing directory: $manifestPath"
}
if ($Capture -and [IO.File]::Exists($manifestPath)) {
    throw "Checkpoint manifest already exists and will not be overwritten: $manifestPath"
}

$initialLasalProcessIds = @(Assert-LasalClosed)
$gitPath = (Get-Command git -ErrorAction Stop).Source
$pwshPath = Join-Path $PSHOME 'pwsh.exe'
if (-not [IO.File]::Exists($pwshPath)) {
    throw "pwsh.exe is unavailable: $pwshPath"
}

$classesPath = "$TargetRelativeRoot/Class/Classes.lcb"
$projectDatabasePath = "$TargetRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcb"
$projectDefinitionPath = "$TargetRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcp"
$vendorPaths = @(
    "$TargetRelativeRoot/Class/_UDPTransceiver/_UDPTransceiver.st",
    ("$TargetRelativeRoot/Class/_UDPTransceiverInterface/" +
        '_UDPTransceiverInterface.st'))
$protectedPaths = @(
    "$TargetRelativeRoot/Class/_StdLib/_StdLib.st",
    "$TargetRelativeRoot/Class/CriticalSection/CriticalSection.st",
    "$TargetRelativeRoot/Source/interfaces/lsl_st_tcp_user.h")
$tcpPath = "$TargetRelativeRoot/Class/TCPMotionInterface/TCPMotionInterface.st"
$derivedPath =
    "$TargetRelativeRoot/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st"
$configObjectsPath = "$TargetRelativeRoot/Network/ConfigObjects.st"
$networksDatabasePath = "$TargetRelativeRoot/Network/Networks.lcb"
$commNetworkPath =
    "$TargetRelativeRoot/Network/Comm_Network/Comm_Network.lcn"
$commTablePath =
    "$TargetRelativeRoot/Network/Comm_Network/ONE_Comm_Network_Table.st"
$scriptRelativePath =
    Get-RepositoryRelativePath -Root $root -Path $PSCommandPath
$expectedScriptRelativePath =
    "$EvidenceRelativeRoot/Capture-UdpCallbackGateBCheckpoint.ps1"
if ($scriptRelativePath -cne $expectedScriptRelativePath) {
    throw (
        'The checkpoint script must run from its canonical evidence path: ' +
        $expectedScriptRelativePath)
}
$toolPaths = @($scriptRelativePath, $VerifierRelativePath)

$startHeadResult = Invoke-ProcessCapture `
    -FileName $gitPath `
    -Arguments @('-C', $root, 'rev-parse', '--verify', 'HEAD')
Assert-CommandPassed -Result $startHeadResult -Owner 'fixed start Git HEAD'
$startHead = $startHeadResult.Stdout.Trim().ToUpperInvariant()
if ($startHead -notmatch '^[A-F0-9]{40,64}$') {
    throw "Fixed start Git HEAD has an unexpected identity: $startHead"
}
$toolTrust = Get-ToolTrustEvidence `
    -Root $root `
    -GitPath $gitPath `
    -ToolPaths $toolPaths `
    -StartHead $startHead `
    -PermitUncommitted:$AllowUncommittedToolBootstrap.IsPresent

$lineageRelativePaths = @()
if ([int]$phaseContract.Sequence -ge 1) {
    $lineageRelativePaths +=
        "$EvidenceRelativeRoot/$($PhaseContracts.GateA_VendorImported.OutputFile)"
}
if ([int]$phaseContract.Sequence -ge 2) {
    $lineageRelativePaths +=
        "$EvidenceRelativeRoot/$($PhaseContracts.GateB1_DerivedDeclaration.OutputFile)"
}
foreach ($lineageRelativePath in $lineageRelativePaths) {
    $lineageFullPath = Join-Path $root $lineageRelativePath.Replace('/', '\')
    if (-not [IO.File]::Exists($lineageFullPath)) {
        throw "Required predecessor checkpoint is missing: $lineageRelativePath"
    }
    $null = Assert-PathComponentsNoReparse `
        -Root $root `
        -Path $lineageFullPath
}
$gatedPathspec = @(
    @($TargetRelativeRoot, $VerifierRelativePath, $scriptRelativePath) +
        $lineageRelativePaths |
        Sort-Object -Unique)
$gitStart = Get-GitStateSnapshot `
    -Root $root `
    -GitPath $gitPath `
    -GatedPathspec $gatedPathspec
if ($gitStart.head -cne $startHead) {
    throw 'Git HEAD changed between tool binding and the start snapshot.'
}
$trackedPathArray = @($gitStart.trackedPaths)
$trackedPathSet =
    [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::OrdinalIgnoreCase)
foreach ($trackedPath in $trackedPathArray) {
    if (-not $trackedPathSet.Add($trackedPath)) {
        throw "Full tracked path snapshot contains a duplicate: $trackedPath"
    }
}

$includeRelativeRoot = "$TargetRelativeRoot/Include"
$networkRelativeRoot = "$TargetRelativeRoot/Network"
$includeAvailablePaths = Get-AvailableRelativeFiles `
    -Root $root -RelativeDirectory $includeRelativeRoot
$networkAvailablePaths = Get-AvailableRelativeFiles `
    -Root $root -RelativeDirectory $networkRelativeRoot
$includeTrackedPaths = @($trackedPathArray | Where-Object {
        $_.StartsWith($includeRelativeRoot + '/',
            [StringComparison]::OrdinalIgnoreCase)
    })
$networkTrackedPaths = @($trackedPathArray | Where-Object {
        $_.StartsWith($networkRelativeRoot + '/',
            [StringComparison]::OrdinalIgnoreCase)
    })

$requiredFixedPaths = @(
    $classesPath,
    $projectDatabasePath,
    $projectDefinitionPath,
    $tcpPath,
    $VerifierRelativePath,
    $scriptRelativePath) + $vendorPaths + $protectedPaths + $lineageRelativePaths
$availableFixedPaths = @($requiredFixedPaths)
$derivedFullPath = Join-Path $root $derivedPath.Replace('/', '\')
$initialDerivedPresent = [IO.File]::Exists($derivedFullPath)
if ($initialDerivedPresent) {
    $availableFixedPaths += $derivedPath
}
elseif ($phaseContract.ExpectedState -cne 'VendorImported') {
    throw "Derived sender is missing for requested phase: $derivedPath"
}

$allAvailablePaths = @(
    @($availableFixedPaths + $includeAvailablePaths +
        $networkAvailablePaths) |
        Sort-Object -Unique)
$readFiles =
    [Collections.Generic.Dictionary[string, object]]::new(
        [StringComparer]::OrdinalIgnoreCase)
foreach ($relativePath in $allAvailablePaths) {
    if ($readFiles.ContainsKey($relativePath)) {
        throw "Duplicate single-read capture input: $relativePath"
    }
    $readFiles.Add(
        $relativePath,
        (Read-SingleFileEvidence `
            -Root $root `
            -GitPath $gitPath `
            -RelativePath $relativePath `
            -TrackedPaths $trackedPathSet))
}

foreach ($sourcePath in @($vendorPaths + $protectedPaths)) {
    Assert-AsciiNoBomText `
        -File $readFiles[$sourcePath] `
        -Owner 'LASAL source/header' `
        -RequireUniformEol
}
Assert-AsciiNoBomText `
    -File $readFiles[$tcpPath] `
    -Owner 'existing TCPMotionInterface source'
if ($readFiles.ContainsKey($derivedPath)) {
    Assert-AsciiNoBomText `
        -File $readFiles[$derivedPath] `
        -Owner 'Derived LASAL source' `
        -RequireUniformEol
}
Assert-AsciiNoBomText `
    -File $readFiles[$projectDefinitionPath] `
    -Owner 'LASAL project XML' `
    -RequireUniformEol
foreach ($networkTextPath in @(
        $configObjectsPath,
        $commNetworkPath,
        $commTablePath)) {
    Assert-KnownTextHeader `
        -File $readFiles[$networkTextPath] `
        -Owner 'LASAL Network text artifact'
}
foreach ($includePath in @($includeAvailablePaths | Where-Object {
            [IO.Path]::GetExtension($_) -ieq '.h'
        })) {
    Assert-KnownTextHeader `
        -File $readFiles[$includePath] `
        -Owner 'Generated Include artifact'
}

$scriptText = Get-AsciiText `
    -File $readFiles[$scriptRelativePath] `
    -Owner 'Gate B capture script'
$canonicalPinSelfTest = Invoke-CanonicalAsciiPinSelfTest
if ($AllowUncommittedToolBootstrap) {
    $verifierTraits = Get-RawTextTraits `
        -Bytes $readFiles[$VerifierRelativePath].RawBytes
    if (($verifierTraits.bom -cne 'None') -or
        (-not $verifierTraits.is7BitAscii) -or
        ($verifierTraits.eolStyle -notin @('LF', 'CRLF'))) {
        throw 'Bootstrap verifier must still be strict ASCII with LF or CRLF EOL.'
    }
    $observedVerifierText = [Text.Encoding]::ASCII.GetString(
        $readFiles[$VerifierRelativePath].RawBytes)
    $observedVerifierText =
        $observedVerifierText.Replace("`r`n", "`n").Replace("`r", "`n")
    $observedVerifierBytes = $Utf8NoBom.GetBytes($observedVerifierText)
    $pinBytes = $observedVerifierBytes.Length
    $pinSha256 = Get-BytesSha256 -Bytes $observedVerifierBytes
    $pinSource = 'observed-untrusted-bootstrap'
}
else {
    if (($ExpectedVerifierCanonicalLfBytes -le 0) -or
        ($ExpectedVerifierCanonicalLfSha256 -notmatch '^[A-F0-9]{64}$')) {
        throw (
            'Final reviewed verifier canonical identity is not pinned. ' +
            'Capture and trusted ValidateOnly are disabled.')
    }
    $pinBytes = $ExpectedVerifierCanonicalLfBytes
    $pinSha256 = $ExpectedVerifierCanonicalLfSha256
    $pinSource = 'committed-reviewed-pin'
}
$verifierCanonicalPin = Get-CanonicalAsciiPinEvidence `
    -Bytes $readFiles[$VerifierRelativePath].RawBytes `
    -Owner 'UDP callback verifier' `
    -ExpectedCanonicalLfBytes $pinBytes `
    -ExpectedCanonicalLfSha256 $pinSha256
$verifierCanonicalPin.Public.pinSource = $pinSource
$scriptAst = Get-AstEvidence `
    -Text $scriptText `
    -Owner 'Gate B capture script'
$verifierAst = Get-AstEvidence `
    -Text $verifierCanonicalPin.Text `
    -Owner 'UDP callback verifier'

$includeInventory = Get-InventoryEvidence `
    -TrackedPaths $includeTrackedPaths `
    -AvailablePaths $includeAvailablePaths `
    -ReadFiles $readFiles `
    -Owner 'Generated Include'
$networkInventory = Get-InventoryEvidence `
    -TrackedPaths $networkTrackedPaths `
    -AvailablePaths $networkAvailablePaths `
    -ReadFiles $readFiles `
    -Owner 'Full Network'
$lineageEvidence = Get-ValidatedLineageEvidence `
    -CurrentPhase $Phase `
    -ReadFiles $readFiles `
    -Root $root `
    -GitPath $gitPath `
    -StartHead $startHead

$verifierFullPath = Join-Path $root $VerifierRelativePath.Replace('/', '\')
$verifierLock = [IO.File]::Open(
    $verifierFullPath,
    [IO.FileMode]::Open,
    [IO.FileAccess]::Read,
    [IO.FileShare]::Read)
try {
    $lockedVerifierBytes = [byte[]]::new($verifierLock.Length)
    $totalRead = 0
    while ($totalRead -lt $lockedVerifierBytes.Length) {
        $read = $verifierLock.Read(
            $lockedVerifierBytes,
            $totalRead,
            $lockedVerifierBytes.Length - $totalRead)
        if ($read -eq 0) {
            throw 'Verifier read lock ended before the expected byte count.'
        }
        $totalRead += $read
    }
    if (-not [Security.Cryptography.CryptographicOperations]::FixedTimeEquals(
            $lockedVerifierBytes,
            $readFiles[$VerifierRelativePath].RawBytes)) {
        throw 'Verifier changed before its execution lock was acquired.'
    }
    $selfTestResult = Invoke-ProcessCapture `
        -FileName $pwshPath `
        -Arguments @(
            '-NoLogo', '-NoProfile', '-NonInteractive',
            '-File', $verifierFullPath,
            '-RunSelfTest')
    Assert-CommandPassed `
        -Result $selfTestResult `
        -Owner 'UDP callback verifier self-test'
    if ($selfTestResult.Stdout -notmatch
        '(?m)^PASS LASAL\.UdpCallbackContract\.SelfTest ') {
        throw 'UDP callback verifier self-test did not emit PASS evidence.'
    }

    $currentArguments = @(
        '-NoLogo', '-NoProfile', '-NonInteractive',
        '-File', $verifierFullPath,
        '-VerifyCurrent',
        '-RepositoryRoot', $root,
        '-ExpectedState', $phaseContract.ExpectedState)
    if ($phaseContract.NeedsRebaseline) {
        $currentArguments += '-AllowDerivedCapture'
    }
    $currentResult = Invoke-ProcessCapture `
        -FileName $pwshPath `
        -Arguments $currentArguments
    Assert-CommandPassed `
        -Result $currentResult `
        -Owner 'UDP callback current-state verifier'
}
finally {
    $verifierLock.Dispose()
}
$verifierDecision = Get-CurrentDecisionFromVerifierOutput `
    -Output $currentResult.Stdout `
    -ExpectedState $phaseContract.ExpectedState `
    -ExpectedProductionApproved $phaseContract.ProductionApproved `
    -ExpectedNeedsRebaseline $phaseContract.NeedsRebaseline
$includeNameToPath = [ordered]@{
    'C_channels.h' = "$includeRelativeRoot/C_channels.h"
    'channels.h' = "$includeRelativeRoot/channels.h"
    'lslpublictypes.h' = "$includeRelativeRoot/lslpublictypes.h"
}
$protectedNameToPath = [ordered]@{
    '_StdLib' = $protectedPaths[0]
    'CriticalSection' = $protectedPaths[1]
    'lsl_st_tcp_user.h' = $protectedPaths[2]
}
$verifierCrossCheck = Assert-VerifierEvidenceMatchesCapture `
    -Decision $verifierDecision `
    -ReadFiles $readFiles `
    -VendorPaths $vendorPaths `
    -ClassesPath $classesPath `
    -ProjectDatabasePath $projectDatabasePath `
    -ProjectDefinitionPath $projectDefinitionPath `
    -TcpPath $tcpPath `
    -NetworkInventory $networkInventory `
    -IncludeNameToPath $includeNameToPath `
    -ProtectedNameToPath $protectedNameToPath

$diffCheckResult = Invoke-ProcessCapture `
    -FileName $gitPath `
    -Arguments @('-C', $root, 'diff', '--check')
Assert-CommandPassed -Result $diffCheckResult -Owner 'git diff --check'
$cachedDiffCheckResult = Invoke-ProcessCapture `
    -FileName $gitPath `
    -Arguments @('-C', $root, 'diff', '--cached', '--check')
Assert-CommandPassed `
    -Result $cachedDiffCheckResult `
    -Owner 'git diff --cached --check'

$gitPrePublish = Get-GitStateSnapshot `
    -Root $root `
    -GitPath $gitPath `
    -GatedPathspec $gatedPathspec
Assert-GitStateStable `
    -Expected $gitStart `
    -Observed $gitPrePublish `
    -Owner 'pre-publish'
$finalLasalProcessIds = @(Assert-LasalClosed)
Assert-InputContentStable `
    -Root $root `
    -ReadFiles $readFiles `
    -InitialIncludePaths $includeAvailablePaths `
    -InitialNetworkPaths $networkAvailablePaths
Assert-OptionalPathPresenceStable `
    -Root $root `
    -RelativePath $derivedPath `
    -ExpectedPresent $initialDerivedPresent
$gitPublishGuard = Get-GitStateSnapshot `
    -Root $root `
    -GitPath $gitPath `
    -GatedPathspec $gatedPathspec
Assert-GitStateStable `
    -Expected $gitStart `
    -Observed $gitPublishGuard `
    -Owner 'publish guard'
$null = Resolve-ExactEvidenceDirectory `
    -Root $root `
    -RequestedPath $OutputPath

$manifest = [ordered]@{
    schema = 'LasalUdpCallbackGateBCheckpoint/v2'
    phase = $Phase
    observedAt = [DateTimeOffset]::Now.ToString('o')
    lineage = $lineageEvidence
    targetProject = [ordered]@{
        path = $TargetRelativeRoot
        compilerVersion = 'C78'
        targetArchitecture = 'ARM'
    }
    verifierDecision = $verifierDecision
    approvalRatchet = [ordered]@{
        productionApproved = $verifierDecision.productionApproved
        needsRebaseline = $verifierDecision.needsRebaseline
        note = if ($verifierDecision.needsRebaseline) {
            'Physical derived hashes are capture-only until separately reviewed and rebaselined.'
        }
        else {
            'Gate A VendorImported baseline is verifier-approved.'
        }
    }
    captureSafety = [ordered]@{
        lasalProcessName = 'Lasal2'
        initialPidCount = $initialLasalProcessIds.Count
        finalPrePublishPidCount = $finalLasalProcessIds.Count
        finalCommitGuardPidCount = $finalLasalProcessIds.Count
        lasalObservedClosedAtAllGuards = $true
        continuousProcessAbsenceClaimed = $false
        outputDirectory = $EvidenceRelativeRoot
        outputFile = $phaseContract.OutputFile
        outputMode = (
            'same-directory temp FileMode.CreateNew; Flush(true); strict raw ' +
            'and JSON reread; atomic Move(overwrite=false); final reread')
        writeScope = (
            'one transient same-directory temp and the exact manifest; ' +
            'the temp is retained for explicit inspection if a failure occurs ' +
            'after CreateNew and before atomic Move; after Move, the final path ' +
            'remains for inspection')
        capturedInputsStable = $true
        rawReadStrategy = (
            'Recorded bytes and SHA-256 came from initial ReadAllBytes ' +
            'snapshots. Existing inputs, full Include and Network inventories, ' +
            'optional derived presence, committed tool identity, Git HEAD, ' +
            'full-repository index/status, and the complete nonignored target worktree ' +
            'inventory were checked again before publication.')
        textPolicy = (
            'Tooling, vendor/protected sources, and the newly derived sender ' +
            'are 7-bit ASCII without BOM and use one LF or CRLF style. The ' +
            'existing TCP source remains ASCII/no-BOM and its physical EOL ' +
            'traits and raw hash are recorded without a uniform-EOL claim. ' +
            'Gate-B writable Network text and ' +
            'generated Include headers may retain vendor single-byte high ' +
            'characters but must have no BOM and must use one LF or CRLF ' +
            'style. Other protected Network artifacts retain exact raw hashes; ' +
            'binary artifacts are raw-only.')
        finalizationProtocol =
            'verified-stage/all-final-guards/atomic-move-last/v1'
        atomicMoveIsFinalExternalStateCommitPoint = $true
        postMoveExternalStateChecks = $false
        orphanStagePolicy = (
            'A pre-existing or failed same-output temp blocks retry and is never ' +
            'auto-deleted; inspect and remove it explicitly. Only a final raw, ' +
            'strict JSON, and seal reread follows the atomic move.')
        derivedSenderExpectedPresent = $initialDerivedPresent
    }
    git = [ordered]@{
        head = $gitStart.head
        gatedPathspec = $gatedPathspec
        start = $gitStart
        prePublish = $gitPrePublish
        finalCommitGuard = $gitPublishGuard
        stageGuardRevalidationRequired = $true
        fullRepositoryTrackedPathCount = $trackedPathArray.Count
        fullRepositoryTrackedPathInventorySha256 = Get-TextSha256 -Text (
            [string]::Join("`n", $trackedPathArray))
        fullRepositoryTrackedPaths = $trackedPathArray
    }
    tooling = [ordered]@{
        trust = $toolTrust
        captureScript = $readFiles[$scriptRelativePath].Public
        verifier = $readFiles[$VerifierRelativePath].Public
        verifierCanonicalPin = $verifierCanonicalPin.Public
        canonicalPinSelfTest = $canonicalPinSelfTest
        ast = @($scriptAst, $verifierAst)
        verifierSelfTest = ConvertTo-CommandEvidence -Result $selfTestResult
        verifierCurrent = ConvertTo-CommandEvidence -Result $currentResult
        verifierCrossCheck = $verifierCrossCheck
        diffCheck = ConvertTo-CommandEvidence -Result $diffCheckResult
        cachedDiffCheck = ConvertTo-CommandEvidence -Result $cachedDiffCheckResult
    }
    artifacts = [ordered]@{
        classesDatabase = $readFiles[$classesPath].Public
        projectDatabase = $readFiles[$projectDatabasePath].Public
        projectDefinition = $readFiles[$projectDefinitionPath].Public
        generatedIncludes = $includeInventory
        vendorSources = @($vendorPaths | ForEach-Object {
                $readFiles[$_].Public
            })
        protectedDependencies = @($protectedPaths | ForEach-Object {
                $readFiles[$_].Public
            })
        tcpMotionInterface = $readFiles[$tcpPath].Public
        derivedSender = Get-PresenceEvidence `
            -RelativePath $derivedPath `
            -TrackedPaths $trackedPathSet `
            -ReadFiles $readFiles
        configObjects = $readFiles[$configObjectsPath].Public
        networksDatabase = $readFiles[$networksDatabasePath].Public
        commNetwork = $readFiles[$commNetworkPath].Public
        commNetworkTable = $readFiles[$commTablePath].Public
        fullNetwork = $networkInventory
    }
}

$sealedManifest = ConvertTo-SealedManifestBytes -Manifest $manifest
$manifestBytes = $sealedManifest.Bytes
if ($ValidateOnly) {
    $prefix = if ($AllowUncommittedToolBootstrap) {
        'UNTRUSTED'
    }
    else {
        'PASS'
    }
    Write-Output (
        "$prefix LASAL.UdpCallbackGateBCheckpoint.ValidateOnly " +
        "(phase=$Phase; state=$($verifierDecision.state); " +
        "productionApproved=$($verifierDecision.productionApproved); " +
        "needsRebaseline=$($verifierDecision.needsRebaseline); " +
        "toolTrust=$($toolTrust.mode); Lasal2PID=0; outputCreated=false; " +
        "plannedBytes=$($manifestBytes.Length); " +
        "plannedSha256=$(Get-BytesSha256 -Bytes $manifestBytes))")
    return
}

$stage = New-VerifiedJsonStage `
    -Root $root `
    -EvidenceDirectory $evidenceDirectory `
    -ManifestPath $manifestPath `
    -ManifestBytes $manifestBytes `
    -ExpectedPhase $Phase

$stageGuardLasalProcessIds = @(Assert-LasalClosed)
Assert-InputContentStable `
    -Root $root `
    -ReadFiles $readFiles `
    -InitialIncludePaths $includeAvailablePaths `
    -InitialNetworkPaths $networkAvailablePaths
Assert-OptionalPathPresenceStable `
    -Root $root `
    -RelativePath $derivedPath `
    -ExpectedPresent $initialDerivedPresent
$stageGitGuard = Get-GitStateSnapshot `
    -Root $root `
    -GitPath $gitPath `
    -GatedPathspec $gatedPathspec
Assert-GitStateStable `
    -Expected $gitStart `
    -Observed $stageGitGuard `
    -Owner 'post-stage final commit guard'
Assert-JsonStructuralEquality `
    -Expected $gitPublishGuard `
    -Observed $stageGitGuard `
    -Owner 'recorded/post-stage final Git guard'
$stageToolTrust = Get-ToolTrustEvidence `
    -Root $root `
    -GitPath $gitPath `
    -ToolPaths $toolPaths `
    -StartHead $startHead
Assert-JsonStructuralEquality `
    -Expected $toolTrust `
    -Observed $stageToolTrust `
    -Owner 'start/post-stage committed tool identity'
$stageLineageEvidence = Get-ValidatedLineageEvidence `
    -CurrentPhase $Phase `
    -ReadFiles $readFiles `
    -Root $root `
    -GitPath $gitPath `
    -StartHead $startHead
Assert-JsonStructuralEquality `
    -Expected $lineageEvidence `
    -Observed $stageLineageEvidence `
    -Owner 'start/post-stage predecessor lineage'
$finalEvidenceDirectory = Resolve-ExactEvidenceDirectory `
    -Root $root `
    -RequestedPath $OutputPath
if (-not [string]::Equals(
        $finalEvidenceDirectory,
        $evidenceDirectory,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Evidence directory identity changed before final commit.'
}
$publishEvidence = Publish-VerifiedJsonStage `
    -Root $root `
    -EvidenceDirectory $evidenceDirectory `
    -Stage $stage `
    -ManifestBytes $manifestBytes `
    -ExpectedPhase $Phase
$finalOutputEvidence = $publishEvidence.finalReread

Write-Output (
    'PASS LASAL.UdpCallbackGateBCheckpoint ' +
    "(phase=$Phase; state=$($verifierDecision.state); " +
    "productionApproved=$($verifierDecision.productionApproved); " +
    "needsRebaseline=$($verifierDecision.needsRebaseline); " +
    'Lasal2PID=0; finalCommitGuardEqual=true; atomicMoveLast=true; ' +
    "output=$manifestPath; " +
    "bytes=$($finalOutputEvidence.bytes); " +
    "sha256=$($finalOutputEvidence.sha256))")

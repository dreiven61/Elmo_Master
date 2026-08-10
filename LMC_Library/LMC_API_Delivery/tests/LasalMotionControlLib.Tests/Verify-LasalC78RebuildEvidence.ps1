[CmdletBinding(DefaultParameterSetName = 'Capture')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Capture')]
    [switch]$CaptureBuildBaseline,
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [switch]$VerifyBuild,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,
    [Parameter(ParameterSetName = 'Capture')]
    [Parameter(ParameterSetName = 'Verify')]
    [string]$RepositoryRoot,
    [Parameter(Mandatory = $true, ParameterSetName = 'Capture')]
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$EvidencePath,
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$BuildTranscriptPath,
    [Parameter(ParameterSetName = 'Capture')]
    [Parameter(ParameterSetName = 'Verify')]
    [ValidateSet('Historical', 'GateDVisualLayout')]
    [string]$EvidenceProfile = 'Historical',
    [Parameter(ParameterSetName = 'Verify')]
    [string]$BoundedLogDeltaPath,
    [Parameter(ParameterSetName = 'Verify')]
    [string]$BoundedLogDeltaManifestPath,
    [Parameter(ParameterSetName = 'Capture')]
    [Parameter(ParameterSetName = 'Verify')]
    [string]$LasalLogPath = (Join-Path $env:TEMP 'Lasal2.log'),
    [Parameter(ParameterSetName = 'Verify')]
    [switch]$RunFullStatic
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Join-Path $PSScriptRoot '..\..\..\..'
}

$Owner = 'LASAL.C78RebuildEvidence'
$EvidenceSchema = 'LasalC78RebuildEvidence/v1'
$HistoricalEvidenceProfile = 'Historical'
$GateDVisualLayoutEvidenceProfile = 'GateDVisualLayout'
$BoundedDeltaSchema = 'LasalC78BoundedLogDelta/v1'
$BoundedDeltaProvenance = 'Exact byte slice from prefix-validated Lasal2.log'
$CanonicalProjectRelativePath = (
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/' +
    'Elmo_EtherCAT_Test_4Axis.lcp')
$HistoricalEvidenceRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st',
    $CanonicalProjectRelativePath,
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn'
)
$HistoricalRequiredCompileRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st'
)
$GateDVisualLayoutAdditionalEvidenceRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiver/_UDPTransceiver.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
)
$GateDVisualLayoutAdditionalRequiredCompileRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiver/_UDPTransceiver.st'
)
$ExpectedRegeneratedOutputRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
)
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$Utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
$GateDExpectedSourceWarningCount = 76
$GateDExpectedSourceWarningHistogram = @{
    '0069' = 35
    '0070' = 21
    '0072' = 17
    '0073' = 3
}
$GateDExpectedCompilerDoneCount = 2
$GateDExpectedLinkerDoneCount = 1
$GateDCompatibilityCompilerVersion = 'C82'
$GateDBoundedDeltaStartOffset = 4962208L
$GateDBoundedDeltaEndOffset = 5727932L
$GateDBoundedDeltaByteCount = 765724
$GateDBoundedDeltaSha256 = (
    'e786adb33c44cd6308e02c6fe9351bba858569cdb3e8e097db9489079380fd2a')
$GateDBoundedSessionPid = 7288
$GateDBoundedRebuildTid = 31624
$GateDBoundedManifestByteCount = 1062
$GateDBoundedManifestSha256 = (
    '587203ed5c45dc8a09bd037b07673d57902bbaba3d73939f476356bbac64c31e')
$GateDTranscriptByteCount = 30111
$GateDTranscriptSha256 = (
    'f32122d318dbfd8f53bc9e5ad0ff693f9b6f05368d40fc64138a010a1bc810af')

function Throw-RebuildEvidenceBlocker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    throw "$Owner blocker: $Message"
}

function Get-NormalizedFullPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
}

function Test-PathIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Left,
        [Parameter(Mandatory = $true)]
        [string]$Right
    )

    return [string]::Equals(
        (Get-NormalizedFullPath -Path $Left),
        (Get-NormalizedFullPath -Path $Right),
        [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-EvidenceProfileContract {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile
    )

    $evidencePaths = @($HistoricalEvidenceRelativePaths)
    $requiredCompilePaths = @($HistoricalRequiredCompileRelativePaths)
    if ($Profile -ceq $GateDVisualLayoutEvidenceProfile) {
        $evidencePaths += $GateDVisualLayoutAdditionalEvidenceRelativePaths
        $requiredCompilePaths +=
            $GateDVisualLayoutAdditionalRequiredCompileRelativePaths
    }

    $entries = @()
    foreach ($relativePath in $evidencePaths) {
        $role = 'inputIdentity'
        if ($Profile -ceq $GateDVisualLayoutEvidenceProfile -and
            $ExpectedRegeneratedOutputRelativePaths -ccontains $relativePath) {
            $role = 'expectedRegeneratedOutput'
        }
        $entries += [pscustomobject]@{
            RelativePath = $relativePath
            Role = $role
        }
    }

    return [pscustomobject]@{
        Profile = $Profile
        EvidenceEntries = $entries
        EvidenceRelativePaths = @($evidencePaths)
        RequiredCompileRelativePaths = @($requiredCompilePaths)
    }
}

function Get-BaselineEvidenceProfile {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Baseline
    )

    if ($Baseline.PSObject.Properties.Match('EvidenceProfile').Count -eq 0) {
        return $HistoricalEvidenceProfile
    }
    $profile = [string]$Baseline.EvidenceProfile
    if ($profile -cnotin @(
            $HistoricalEvidenceProfile,
            $GateDVisualLayoutEvidenceProfile)) {
        Throw-RebuildEvidenceBlocker "baseline evidence profile is invalid: $profile"
    }
    return $profile
}

function Assert-ExactStringSequence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OwnerName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Actual,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Expected
    )

    if ($Actual.Count -ne $Expected.Count) {
        Throw-RebuildEvidenceBlocker (
            "$OwnerName count is $($Actual.Count), expected $($Expected.Count).")
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if ($Actual[$index] -cne $Expected[$index]) {
            Throw-RebuildEvidenceBlocker (
                "$OwnerName entry $index is '$($Actual[$index])', expected " +
                "'$($Expected[$index])'.")
        }
    }
}

function Get-CanonicalRepositoryContext {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile
    )

    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        Throw-RebuildEvidenceBlocker "repository root does not exist: $Root"
    }
    $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\')
    $projectPath = Get-NormalizedFullPath -Path (
        Join-Path $resolvedRoot $CanonicalProjectRelativePath)
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker (
            "canonical project does not exist: $projectPath")
    }

    $contract = Get-EvidenceProfileContract -Profile $Profile
    return [pscustomobject]@{
        RepositoryRoot = $resolvedRoot
        CanonicalProjectPath = $projectPath
        Contract = $contract
        RequiredCompilePaths = @(
            $contract.RequiredCompileRelativePaths | ForEach-Object {
                Get-NormalizedFullPath -Path (Join-Path $resolvedRoot $_)
            }
        )
    }
}

function Get-Sha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
                $sha256.ComputeHash($Bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-FileSha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "hash input does not exist: $Path"
    }
    return Get-Sha256Hex -Bytes ([System.IO.File]::ReadAllBytes($Path))
}

function Get-FileRawIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "identity input does not exist: $Path"
    }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    return [pscustomobject]@{
        Bytes = [long]$bytes.LongLength
        Sha256 = Get-Sha256Hex -Bytes $bytes
    }
}

function Get-GateDStReplayIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [string]$IdentityOwner
    )

    try {
        $text = $Utf8Strict.GetString($Bytes)
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "$IdentityOwner is not strict UTF-8 text: $($_.Exception.Message)")
    }

    $crLfCount = [regex]::Matches($text, "`r`n").Count
    $lfOnlyCount = [regex]::Matches($text, "(?<!`r)`n").Count
    $crOnlyCount = [regex]::Matches($text, "`r(?!`n)").Count
    if ($crOnlyCount -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "$IdentityOwner contains $crOnlyCount standalone CR line ending(s).")
    }
    $lineBreakCount = $crLfCount + $lfOnlyCount
    $eolStyle = if ($crLfCount -gt 0 -and $lfOnlyCount -gt 0) {
        'Mixed'
    }
    elseif ($crLfCount -gt 0) {
        'CRLF'
    }
    elseif ($lfOnlyCount -gt 0) {
        'LF'
    }
    else {
        'None'
    }
    $canonicalLfBytes = $Utf8NoBom.GetBytes($text.Replace("`r`n", "`n"))
    return [pscustomobject]@{
        RawBytes = [long]$Bytes.LongLength
        RawSha256 = Get-Sha256Hex -Bytes $Bytes
        CanonicalLfBytes = [long]$canonicalLfBytes.LongLength
        CanonicalLfSha256 = Get-Sha256Hex -Bytes $canonicalLfBytes
        EolStyle = $eolStyle
        CrLfCount = [long]$crLfCount
        LfOnlyCount = [long]$lfOnlyCount
        CrOnlyCount = [long]$crOnlyCount
        LineBreakCount = [long]$lineBreakCount
    }
}

function Get-GateDStReplayIdentityFromFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$IdentityOwner
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "$IdentityOwner does not exist: $Path"
    }
    return Get-GateDStReplayIdentity `
        -Bytes ([System.IO.File]::ReadAllBytes($Path)) `
        -IdentityOwner $IdentityOwner
}

function Assert-GateDStBaselineIdentityMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [object]$BaselineEntry,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $requiredProperties = @(
        'RawBytes',
        'RawSha256',
        'CanonicalLfBytes',
        'CanonicalLfSha256',
        'EolStyle',
        'CrLfCount',
        'LfOnlyCount',
        'CrOnlyCount',
        'LineBreakCount')
    foreach ($propertyName in $requiredProperties) {
        if ($BaselineEntry.PSObject.Properties.Name -notcontains $propertyName) {
            Throw-RebuildEvidenceBlocker (
                "GateDVisualLayout ST identity property $propertyName is " +
                "missing: $RelativePath")
        }
    }
    foreach ($countProperty in @(
            'RawBytes',
            'CanonicalLfBytes',
            'CrLfCount',
            'LfOnlyCount',
            'CrOnlyCount',
            'LineBreakCount')) {
        $value = $BaselineEntry.$countProperty
        if (($value -isnot [int]) -and ($value -isnot [long])) {
            Throw-RebuildEvidenceBlocker (
                "GateDVisualLayout ST $countProperty is not an integer: " +
                $RelativePath)
        }
        if ([long]$value -lt 0) {
            Throw-RebuildEvidenceBlocker (
                "GateDVisualLayout ST $countProperty is negative: $RelativePath")
        }
    }
    foreach ($hashProperty in @('RawSha256', 'CanonicalLfSha256')) {
        if ([string]$BaselineEntry.$hashProperty -cnotmatch '^[0-9a-f]{64}$') {
            Throw-RebuildEvidenceBlocker (
                "GateDVisualLayout ST $hashProperty is invalid: $RelativePath")
        }
    }
    if ([string]$BaselineEntry.EolStyle -cnotin @(
            'LF', 'CRLF', 'Mixed', 'None')) {
        Throw-RebuildEvidenceBlocker (
            "GateDVisualLayout ST EolStyle is invalid: $RelativePath")
    }
    if ([long]$BaselineEntry.CrOnlyCount -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "GateDVisualLayout ST baseline contains standalone CR: $RelativePath")
    }
    if ([long]$BaselineEntry.LineBreakCount -ne
        ([long]$BaselineEntry.CrLfCount + [long]$BaselineEntry.LfOnlyCount)) {
        Throw-RebuildEvidenceBlocker (
            "GateDVisualLayout ST line-break inventory is inconsistent: $RelativePath")
    }
    $expectedStyle = if ([long]$BaselineEntry.CrLfCount -gt 0 -and
        [long]$BaselineEntry.LfOnlyCount -gt 0) {
        'Mixed'
    }
    elseif ([long]$BaselineEntry.CrLfCount -gt 0) {
        'CRLF'
    }
    elseif ([long]$BaselineEntry.LfOnlyCount -gt 0) {
        'LF'
    }
    else {
        'None'
    }
    if ([string]$BaselineEntry.EolStyle -cne $expectedStyle) {
        Throw-RebuildEvidenceBlocker (
            "GateDVisualLayout ST EolStyle/count inventory differs: $RelativePath")
    }
    if ([string]$BaselineEntry.RawSha256 -cne
        [string]$BaselineEntry.Sha256) {
        Throw-RebuildEvidenceBlocker (
            "GateDVisualLayout ST raw SHA aliases differ: $RelativePath")
    }
}

function Assert-BaselineFileInventory {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$BaselineFiles,
        [Parameter(Mandatory = $true)]
        [object]$Contract,
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile
    )

    if ($BaselineFiles.Count -ne $Contract.EvidenceEntries.Count) {
        Throw-RebuildEvidenceBlocker (
            "baseline file count is $($BaselineFiles.Count), expected " +
            "$($Contract.EvidenceEntries.Count).")
    }
    foreach ($baselineFile in $BaselineFiles) {
        if ($baselineFile.PSObject.Properties.Name -notcontains 'RelativePath' -or
            $baselineFile.PSObject.Properties.Name -notcontains 'Sha256') {
            Throw-RebuildEvidenceBlocker 'baseline file entry is malformed.'
        }
        if ([string]$baselineFile.Sha256 -cnotmatch '^[0-9a-f]{64}$') {
            Throw-RebuildEvidenceBlocker (
                "baseline file SHA-256 is invalid: $($baselineFile.RelativePath)")
        }
        if ($Contract.EvidenceRelativePaths -cnotcontains
            [string]$baselineFile.RelativePath) {
            Throw-RebuildEvidenceBlocker (
                "baseline contains an unexpected file: $($baselineFile.RelativePath)")
        }
    }

    $inputIdentityCount = 0
    $rawInputUnchangedCount = 0
    $replayEquivalentStCount = 0
    $regeneratedOutputCount = 0
    foreach ($expectedEntry in $Contract.EvidenceEntries) {
        $relativePath = $expectedEntry.RelativePath
        $matchingEntries = @($BaselineFiles | Where-Object {
                [string]$_.RelativePath -ceq $relativePath
            })
        if ($matchingEntries.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "baseline entry count for $relativePath is $($matchingEntries.Count), " +
                'expected 1.')
        }
        $matchingEntry = $matchingEntries[0]
        if ($matchingEntry.PSObject.Properties.Name -contains 'Role') {
            if ([string]$matchingEntry.Role -cne $expectedEntry.Role) {
                Throw-RebuildEvidenceBlocker (
                    "baseline role for $relativePath is '$($matchingEntry.Role)', " +
                    "expected '$($expectedEntry.Role)'.")
            }
        }
        elseif ($Profile -ceq $GateDVisualLayoutEvidenceProfile) {
            Throw-RebuildEvidenceBlocker (
                "GateDVisualLayout baseline role is missing: $relativePath")
        }

        if ($expectedEntry.Role -ceq 'expectedRegeneratedOutput') {
            $regeneratedOutputCount++
            continue
        }
        $inputIdentityCount++
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $RepositoryRoot $relativePath)
        $isGateDStInput = (
            $Profile -ceq $GateDVisualLayoutEvidenceProfile -and
            $relativePath.EndsWith(
                '.st',
                [System.StringComparison]::OrdinalIgnoreCase))
        if ($isGateDStInput) {
            Assert-GateDStBaselineIdentityMetadata `
                -BaselineEntry $matchingEntry `
                -RelativePath $relativePath
            $currentIdentity = Get-GateDStReplayIdentityFromFile `
                -Path $fullPath `
                -IdentityOwner "GateDVisualLayout current ST $relativePath"
            if ($currentIdentity.RawBytes -eq [long]$matchingEntry.RawBytes -and
                $currentIdentity.RawSha256 -ceq
                    [string]$matchingEntry.RawSha256) {
                if ($currentIdentity.CanonicalLfBytes -ne
                        [long]$matchingEntry.CanonicalLfBytes -or
                    $currentIdentity.CanonicalLfSha256 -cne
                        [string]$matchingEntry.CanonicalLfSha256 -or
                    $currentIdentity.EolStyle -cne
                        [string]$matchingEntry.EolStyle -or
                    $currentIdentity.CrLfCount -ne
                        [long]$matchingEntry.CrLfCount -or
                    $currentIdentity.LfOnlyCount -ne
                        [long]$matchingEntry.LfOnlyCount -or
                    $currentIdentity.CrOnlyCount -ne
                        [long]$matchingEntry.CrOnlyCount -or
                    $currentIdentity.LineBreakCount -ne
                        [long]$matchingEntry.LineBreakCount) {
                    Throw-RebuildEvidenceBlocker (
                        "GateDVisualLayout ST baseline metadata differs from " +
                        "its raw input: $relativePath")
                }
                $rawInputUnchangedCount++
            }
            elseif ($currentIdentity.CanonicalLfBytes -eq
                    [long]$matchingEntry.CanonicalLfBytes -and
                $currentIdentity.CanonicalLfSha256 -ceq
                    [string]$matchingEntry.CanonicalLfSha256 -and
                $currentIdentity.LineBreakCount -eq
                    [long]$matchingEntry.LineBreakCount) {
                $replayEquivalentStCount++
            }
            else {
                Throw-RebuildEvidenceBlocker (
                    "captured ST input is not canonical-LF replay-equivalent: " +
                    $relativePath)
            }
        }
        else {
            $currentHash = Get-FileSha256Hex -Path $fullPath
            if ($currentHash -cne [string]$matchingEntry.Sha256) {
                Throw-RebuildEvidenceBlocker (
                    "captured input changed after baseline: $relativePath")
            }
            $rawInputUnchangedCount++
        }
    }
    return [pscustomobject]@{
        InputIdentityCount = $inputIdentityCount
        RegeneratedOutputCount = $regeneratedOutputCount
        RawInputUnchangedCount = $rawInputUnchangedCount
        ReplayEquivalentStCount = $replayEquivalentStCount
        InputsEquivalent = $true
    }
}

function Assert-RegeneratedOutputsBound {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Manifest,
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [string[]]$ExpectedRelativePaths
    )

    if ($Manifest.PSObject.Properties.Name -notcontains 'RegeneratedOutputs') {
        Throw-RebuildEvidenceBlocker (
            'profile bounded manifest RegeneratedOutputs property is missing.')
    }
    $outputs = @($Manifest.RegeneratedOutputs)
    if ($outputs.Count -ne $ExpectedRelativePaths.Count) {
        Throw-RebuildEvidenceBlocker (
            "profile bounded regenerated output count is $($outputs.Count), " +
            "expected $($ExpectedRelativePaths.Count).")
    }
    $expectedProperties = @('RelativePath', 'Bytes', 'Sha256')
    for ($index = 0; $index -lt $ExpectedRelativePaths.Count; $index++) {
        $entry = $outputs[$index]
        Assert-ExactStringSequence `
            -OwnerName "regenerated output $index property inventory" `
            -Actual @($entry.PSObject.Properties.Name) `
            -Expected $expectedProperties
        $expectedPath = $ExpectedRelativePaths[$index]
        if ([string]$entry.RelativePath -cne $expectedPath) {
            Throw-RebuildEvidenceBlocker (
                "profile bounded regenerated output $index path is " +
                "'$($entry.RelativePath)', expected '$expectedPath'.")
        }
        if ($entry.Bytes -isnot [int] -and $entry.Bytes -isnot [long]) {
            Throw-RebuildEvidenceBlocker (
                "profile bounded regenerated output bytes are invalid: $expectedPath")
        }
        $manifestBytes = [long]$entry.Bytes
        $manifestSha256 = [string]$entry.Sha256
        if ($manifestBytes -lt 0 -or
            $manifestSha256 -cnotmatch '^[0-9a-f]{64}$') {
            Throw-RebuildEvidenceBlocker (
                "profile bounded regenerated output identity is invalid: $expectedPath")
        }
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $RepositoryRoot $expectedPath)
        $currentIdentity = Get-FileRawIdentity -Path $fullPath
        if ($currentIdentity.Bytes -ne $manifestBytes -or
            $currentIdentity.Sha256 -cne $manifestSha256) {
            Throw-RebuildEvidenceBlocker (
                "current regenerated output differs from manifest: $expectedPath")
        }
    }
    return $outputs.Count
}

function Get-LogBytes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return ,([byte[]]::new(0))
    }
    return ,([System.IO.File]::ReadAllBytes($Path))
}

function Get-PrefixSha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [long]$Length
    )

    if ($Length -lt 0 -or $Length -gt [int]::MaxValue) {
        Throw-RebuildEvidenceBlocker "unsupported log prefix length: $Length"
    }
    if ($Bytes.LongLength -lt $Length) {
        Throw-RebuildEvidenceBlocker (
            "Lasal2.log was truncated: current length $($Bytes.LongLength), " +
            "baseline prefix length $Length")
    }

    $prefix = [byte[]]::new([int]$Length)
    if ($Length -gt 0) {
        [System.Array]::Copy($Bytes, 0, $prefix, 0, [int]$Length)
    }
    return Get-Sha256Hex -Bytes $prefix
}

function Get-AppendedLogText {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [long]$Offset
    )

    if ($Offset -lt 0 -or $Offset -gt [int]::MaxValue -or
        $Bytes.LongLength -gt [int]::MaxValue) {
        Throw-RebuildEvidenceBlocker 'Lasal2.log is too large for evidence parsing.'
    }
    if ($Bytes.LongLength -lt $Offset) {
        Throw-RebuildEvidenceBlocker 'Lasal2.log is shorter than the baseline offset.'
    }

    $length = [int]($Bytes.LongLength - $Offset)
    $appendedBytes = [byte[]]::new($length)
    if ($length -gt 0) {
        [System.Array]::Copy(
            $Bytes,
            [int]$Offset,
            $appendedBytes,
            0,
            $length)
    }
    return [System.Text.Encoding]::UTF8.GetString($appendedBytes)
}

function Get-IndexedLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $rawLines = $Text -split '\r?\n'
    $lines = @()
    for ($index = 0; $index -lt $rawLines.Count; $index++) {
        $rawLine = $rawLines[$index].TrimEnd("`r")
        $pidValue = $null
        $tidValue = $null
        $prefixMatch = [regex]::Match(
            $rawLine,
            '^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*\]\s*(?<Body>.*)$')
        if ($prefixMatch.Success) {
            $pidValue = $prefixMatch.Groups['Pid'].Value
            $tidValue = $prefixMatch.Groups['Tid'].Value
        }
        $lines += [pscustomobject]@{
            Index = $index
            Text = $rawLine
            Pid = $pidValue
            Tid = $tidValue
        }
    }
    return $lines
}

function Assert-TranscriptEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TranscriptText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(Get-IndexedLines -Text $TranscriptText)
    $headerLines = @($lines | Where-Object {
            $_.Text -ceq (
                'Compiler: [INFO] Rebuild project with compiler version C78 ' +
                '(target architecture: ARM)')
        })
    if ($headerLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "transcript C78/ARM rebuild header count is $($headerLines.Count), expected 1.")
    }
    $headerLine = $headerLines[0]

    $summaries = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Text -match '^Done - (?<Errors>\d+) error\(s\), ' +
                '(?<Warnings>\d+) warning\(s\)\.$'
        })
    if ($summaries.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "transcript terminal result count is $($summaries.Count), expected 1.")
    }
    $summaryLine = $summaries[0]

    $saveLine = "OutputCommand: [INFO] Save project '$CanonicalProjectPath'."
    $saveLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Index -lt $summaryLine.Index -and
            $_.Text -ceq $saveLine
        })
    if ($saveLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "canonical Save project line count inside the rebuild window is " +
            "$($saveLines.Count), expected 1.")
    }

    foreach ($compilePath in $RequiredCompilePaths) {
        $compileLine = 'Compiler: [INFO] Compiling "' + $compilePath + '"'
        $compileLines = @($lines | Where-Object {
                $_.Index -gt $saveLines[0].Index -and
                $_.Index -lt $summaryLine.Index -and
                $_.Text -ceq $compileLine
            })
        if ($compileLines.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "transcript compile line count for $compilePath inside the rebuild " +
                "window is $($compileLines.Count), expected 1.")
        }
    }

    $summaryMatch = [regex]::Match(
        $summaryLine.Text,
        '^Done - (?<Errors>\d+) error\(s\), (?<Warnings>\d+) warning\(s\)\.$')
    $errorCount = [int]$summaryMatch.Groups['Errors'].Value
    $warningCount = [int]$summaryMatch.Groups['Warnings'].Value
    if ($errorCount -ne 0 -or
        $warningCount -ne $GateDExpectedSourceWarningCount) {
        Throw-RebuildEvidenceBlocker (
            "transcript result is $errorCount error(s), $warningCount warning(s); " +
            "expected 0 error(s), $GateDExpectedSourceWarningCount warning(s).")
    }

    $preResultLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Index -lt $summaryLine.Index
        })
    $compilerErrors = @($preResultLines | Where-Object {
            $_.Text -match '^Compiler: \[ERROR\]'
        })
    if ($compilerErrors.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "pre-result Compiler error diagnostic count is $($compilerErrors.Count), expected 0.")
    }

    $compilerWarnings = @($preResultLines | Where-Object {
            $_.Text -match '^Compiler: \[WARN\] W \d{4}\b'
        })
    if ($compilerWarnings.Count -ne $GateDExpectedSourceWarningCount) {
        Throw-RebuildEvidenceBlocker (
            "pre-result Compiler warning diagnostic count is $($compilerWarnings.Count), " +
            "expected $GateDExpectedSourceWarningCount.")
    }

    $warningHistogram = @{}
    foreach ($warningLine in $compilerWarnings) {
        $warningMatch = [regex]::Match(
            $warningLine.Text,
            '^Compiler: \[WARN\] W (?<Code>\d{4})\b')
        $code = $warningMatch.Groups['Code'].Value
        if (-not $warningHistogram.ContainsKey($code)) {
            $warningHistogram[$code] = 0
        }
        $warningHistogram[$code]++
    }
    if ($warningHistogram.Count -ne $GateDExpectedSourceWarningHistogram.Count) {
        Throw-RebuildEvidenceBlocker (
            'pre-result warning codes differ from W0069/W0070/W0072/W0073.')
    }
    foreach ($code in $GateDExpectedSourceWarningHistogram.Keys) {
        if (-not $warningHistogram.ContainsKey($code) -or
            $warningHistogram[$code] -ne
                $GateDExpectedSourceWarningHistogram[$code]) {
            $actualCount = 0
            if ($warningHistogram.ContainsKey($code)) {
                $actualCount = $warningHistogram[$code]
            }
            Throw-RebuildEvidenceBlocker (
                "pre-result W$code count is $actualCount, expected " +
                "$($GateDExpectedSourceWarningHistogram[$code]).")
        }
    }

    $unexpectedErrorLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Text -match '^(?:Compiler|OutputCommand): \[ERROR\]'
        })
    if ($unexpectedErrorLines.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "transcript contains $($unexpectedErrorLines.Count) error output line(s).")
    }
    if ($TranscriptText -match "(?im)Command 'Rebuild project' failed") {
        Throw-RebuildEvidenceBlocker 'transcript reports Rebuild project failure.'
    }
}

function Assert-AppendedLasalLogEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    if ($AppendedLogText.Length -eq 0) {
        Throw-RebuildEvidenceBlocker 'no Lasal2.log bytes were appended after capture.'
    }
    if ($AppendedLogText -match '(?i)CInvalidArgException') {
        Throw-RebuildEvidenceBlocker (
            'appended Lasal2.log contains CInvalidArgException.')
    }
    $prohibitedPattern = (
        "(?i)Executing command '(?:Download Project|Connect(?: to )?|" +
        "go online|Link(?: project)?)")
    if ($AppendedLogText -match $prohibitedPattern) {
        Throw-RebuildEvidenceBlocker (
            "appended Lasal2.log contains prohibited explicit command: $($Matches[0])")
    }

    $lines = @(Get-IndexedLines -Text $AppendedLogText)
    $startLines = @($lines | Where-Object {
            $_.Text -match (
                '\(INFO\) GUI\]\s+Start Application at ' +
                '\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\s*$')
        })
    if ($startLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "appended GUI Start Application count is $($startLines.Count), expected 1.")
    }
    $sessionStart = $startLines[0]
    if ([string]::IsNullOrEmpty($sessionStart.Pid)) {
        Throw-RebuildEvidenceBlocker 'GUI Start Application has no PID identity.'
    }

    $rebuildLines = @($lines | Where-Object {
            $_.Text -match "\(INFO\) CmdProc\] Executing command 'Rebuild project'"
        })
    if ($rebuildLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "appended Rebuild project command count is $($rebuildLines.Count), expected 1.")
    }
    $rebuild = $rebuildLines[0]
    if ([string]::IsNullOrEmpty($rebuild.Pid) -or
        [string]::IsNullOrEmpty($rebuild.Tid)) {
        Throw-RebuildEvidenceBlocker 'Rebuild project command has no PID/TID identity.'
    }
    if ($rebuild.Index -le $sessionStart.Index -or
        $rebuild.Pid -cne $sessionStart.Pid) {
        Throw-RebuildEvidenceBlocker (
            'Rebuild project is not inside the single appended GUI session.')
    }

    $sessionLoadLines = @($lines | Where-Object {
            $_.Index -gt $sessionStart.Index -and
            $_.Index -lt $rebuild.Index -and
            $_.Text -match (
                "\(INFO\) CmdProc\] Executing command 'Load Project `"[^`"]+`"'")
        })
    if ($sessionLoadLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "project Load command count between GUI start and Rebuild is " +
            "$($sessionLoadLines.Count), expected 1.")
    }
    $loadNeedle = (
        "Executing command 'Load Project `"$CanonicalProjectPath`"'")
    if ($sessionLoadLines[0].Pid -cne $rebuild.Pid -or
        -not $sessionLoadLines[0].Text.Contains($loadNeedle)) {
        Throw-RebuildEvidenceBlocker (
            'the single session Load Project is not the canonical project.')
    }

    $headerNeedle = (
        'Compiler] Rebuild project with compiler version C78 ' +
        '(target architecture: ARM)')
    $headerLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text.Contains($headerNeedle)
        })
    if ($headerLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command C78/ARM compiler header count is $($headerLines.Count), " +
            'expected 1.')
    }
    $header = $headerLines[0]

    $clearLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Index -lt $header.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) Compiler\] \{Clear\}\s*$'
        })
    if ($clearLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command {Clear} count before the compiler header is " +
            "$($clearLines.Count), expected 1.")
    }

    $resultLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) Compiler\] \{ResultCount\}\s*$'
        })
    if ($resultLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command {ResultCount} count is $($resultLines.Count), expected 1.")
    }
    $resultLine = $resultLines[0]

    $rawPreResultLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Index -lt $resultLine.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid
        })
    $rawCompilerErrors = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(ERROR\) Compiler\]'
        })
    if ($rawCompilerErrors.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Compiler error count is " +
            "$($rawCompilerErrors.Count), expected 0.")
    }
    $rawCompilerWarnings = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(WARN\) Compiler\]'
        })
    if ($rawCompilerWarnings.Count -ne $GateDExpectedSourceWarningCount) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Compiler warning count is " +
            "$($rawCompilerWarnings.Count), expected " +
            "$GateDExpectedSourceWarningCount.")
    }
    $rawWarningHistogram = @{}
    foreach ($rawWarning in $rawCompilerWarnings) {
        $rawWarningMatch = [regex]::Match(
            $rawWarning.Text,
            '\(WARN\) Compiler\] W (?<Code>\d{4})\b')
        if (-not $rawWarningMatch.Success) {
            Throw-RebuildEvidenceBlocker (
                'raw pre-{ResultCount} warning has no W #### diagnostic code.')
        }
        $rawCode = $rawWarningMatch.Groups['Code'].Value
        if (-not $rawWarningHistogram.ContainsKey($rawCode)) {
            $rawWarningHistogram[$rawCode] = 0
        }
        $rawWarningHistogram[$rawCode]++
    }
    if ($rawWarningHistogram.Count -ne
        $GateDExpectedSourceWarningHistogram.Count) {
        Throw-RebuildEvidenceBlocker (
            'raw pre-{ResultCount} warning codes differ from ' +
            'W0069/W0070/W0072/W0073.')
    }
    foreach ($rawCode in $GateDExpectedSourceWarningHistogram.Keys) {
        if (-not $rawWarningHistogram.ContainsKey($rawCode) -or
            $rawWarningHistogram[$rawCode] -ne
                $GateDExpectedSourceWarningHistogram[$rawCode]) {
            $rawActualCount = 0
            if ($rawWarningHistogram.ContainsKey($rawCode)) {
                $rawActualCount = $rawWarningHistogram[$rawCode]
            }
            Throw-RebuildEvidenceBlocker (
                "raw pre-{ResultCount} W$rawCode count is $rawActualCount, expected " +
                "$($GateDExpectedSourceWarningHistogram[$rawCode]).")
        }
    }

    $compilerDoneLines = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(INFO\) Compiler\] Done\s*$'
        })
    if ($compilerDoneLines.Count -ne $GateDExpectedCompilerDoneCount) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Compiler Done count is " +
            "$($compilerDoneLines.Count), expected " +
            "$GateDExpectedCompilerDoneCount.")
    }
    $linkerDoneLines = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(INFO\) Linker\] Done\s*$'
        })
    if ($linkerDoneLines.Count -ne $GateDExpectedLinkerDoneCount) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Linker Done count is " +
            "$($linkerDoneLines.Count), expected $GateDExpectedLinkerDoneCount.")
    }
    if ($compilerDoneLines[-1].Index -ge $linkerDoneLines[0].Index) {
        Throw-RebuildEvidenceBlocker (
            'raw same-command Linker Done does not follow both Compiler Done lines.')
    }

    $rawSaveLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Index -lt $resultLine.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) OutputCommand\] Save project '
        })
    $rawSaveNeedle = (
        "(INFO) OutputCommand] Save project '$CanonicalProjectPath'.")
    if ($rawSaveLines.Count -ne 1 -or
        -not $rawSaveLines[0].Text.EndsWith($rawSaveNeedle)) {
        Throw-RebuildEvidenceBlocker (
            'same-command raw canonical Save project line is missing or duplicated.')
    }
    $rawSaveLine = $rawSaveLines[0]

    foreach ($compilePath in $RequiredCompilePaths) {
        $compileNeedle = 'Compiler] Compiling "' + $compilePath + '"'
        $compileLines = @($lines | Where-Object {
                $_.Index -gt $rawSaveLine.Index -and
                $_.Index -lt $resultLine.Index -and
                $_.Pid -ceq $rebuild.Pid -and
                $_.Tid -ceq $rebuild.Tid -and
                $_.Text.Contains($compileNeedle)
            })
        if ($compileLines.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "same-command compile line count for $compilePath inside the result " +
                "window is $($compileLines.Count), expected 1.")
        }
    }

    $nextCommandLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Text -match '\(INFO\) CmdProc\] Executing command '
        } | Sort-Object -Property Index)
    $commandEndIndex = $lines.Count
    if ($nextCommandLines.Count -gt 0) {
        $commandEndIndex = $nextCommandLines[0].Index
    }

    $terminalLines = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and
            $_.Index -lt $commandEndIndex -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) CmdProc\] Last command succeeded\.'
        })
    if ($terminalLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command CmdProc success terminal count is $($terminalLines.Count), " +
            'expected 1.')
    }
    $terminal = $terminalLines[0]

    $sameCommandFailures = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and
            $_.Index -lt $commandEndIndex -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            ($_.Text -match 'Last command failed\.' -or
                $_.Text -match "Command 'Rebuild project' failed" -or
                $_.Text -match '\(ERROR\)')
        })
    if ($sameCommandFailures.Count -ne 0) {
        Throw-RebuildEvidenceBlocker 'same-command log reports Rebuild failure.'
    }

    $postResultLines = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and $_.Index -lt $terminal.Index
        })
    $postCompilerDiagnostics = @($postResultLines | Where-Object {
            $_.Text -match '\((?:WARN|ERROR)\)'
        })
    if ($postCompilerDiagnostics.Count -ne 6) {
        Throw-RebuildEvidenceBlocker (
            "post-{ResultCount} Compiler diagnostic count is " +
            "$($postCompilerDiagnostics.Count), expected exactly 6 compatibility warnings.")
    }
    foreach ($diagnostic in $postCompilerDiagnostics) {
        if ($diagnostic.Pid -cne $rebuild.Pid -or
            $diagnostic.Tid -cne $rebuild.Tid -or
            $diagnostic.Text -notmatch '\(WARN\) Compiler\]') {
            Throw-RebuildEvidenceBlocker (
                'post-{ResultCount} diagnostics are interleaved or contain an error.')
        }
    }

    $compatibilityPatterns = @(
        ('The current project "Elmo_EtherCAT_Test_4Axis" is using an old ' +
            'compiler version \(C78\)\. Latest version: ' +
            $GateDCompatibilityCompilerVersion + '\..*\|~1' +
            [regex]::Escape($CanonicalProjectPath) + ';'),
        ('The compiler version of library "Hardware"\(' +
            $GateDCompatibilityCompilerVersion +
            '\) differs from the compiler version of the current project \(C78\)'),
        ('The compiler version of library "MotionLib"\(' +
            $GateDCompatibilityCompilerVersion +
            '\) differs from the compiler version of the current project \(C78\)'),
        ('The compiler version of library "OS Interface"\(' +
            $GateDCompatibilityCompilerVersion +
            '\) differs from the compiler version of the current project \(C78\)'),
        ('The compiler version of library "System"\(' +
            $GateDCompatibilityCompilerVersion +
            '\) differs from the compiler version of the current project \(C78\)'),
        ('The compiler version of library "Tools"\(' +
            $GateDCompatibilityCompilerVersion +
            '\) differs from the compiler version of the current project \(C78\)')
    )
    foreach ($compatibilityPattern in $compatibilityPatterns) {
        $matchingWarnings = @($postCompilerDiagnostics | Where-Object {
                $_.Text -match $compatibilityPattern
            })
        if ($matchingWarnings.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                'required post-{ResultCount} compatibility warning is missing or duplicated: ' +
                $compatibilityPattern)
        }
    }
}

function Assert-LasalC78RebuildEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TranscriptText,
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    Assert-TranscriptEvidence `
        -TranscriptText $TranscriptText `
        -CanonicalProjectPath $CanonicalProjectPath `
        -RequiredCompilePaths $RequiredCompilePaths
    Assert-AppendedLasalLogEvidence `
        -AppendedLogText $AppendedLogText `
        -CanonicalProjectPath $CanonicalProjectPath `
        -RequiredCompilePaths $RequiredCompilePaths
}

function Capture-BuildBaseline {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [string]$OutputPath,
        [Parameter(Mandatory = $true)]
        [string]$LogPath,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile
    )

    $runningLasal = @(Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
    if ($runningLasal.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "capture requires Lasal2 to be stopped; found $($runningLasal.Count) process(es).")
    }

    $context = Get-CanonicalRepositoryContext -Root $Root -Profile $Profile
    $resolvedEvidencePath = Get-NormalizedFullPath -Path $OutputPath
    if (Test-Path -LiteralPath $resolvedEvidencePath) {
        Throw-RebuildEvidenceBlocker (
            "evidence file already exists; refusing to overwrite: $resolvedEvidencePath")
    }
    $evidenceDirectory = Split-Path -Parent $resolvedEvidencePath
    if (-not (Test-Path -LiteralPath $evidenceDirectory -PathType Container)) {
        Throw-RebuildEvidenceBlocker (
            "evidence directory does not exist: $evidenceDirectory")
    }

    $resolvedLogPath = Get-NormalizedFullPath -Path $LogPath
    $logBytes = Get-LogBytes -Path $resolvedLogPath
    $files = @()
    foreach ($entry in $context.Contract.EvidenceEntries) {
        $relativePath = $entry.RelativePath
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $context.RepositoryRoot $relativePath)
        $fileEntry = [ordered]@{
            RelativePath = $relativePath
            Role = $entry.Role
            Sha256 = Get-FileSha256Hex -Path $fullPath
        }
        if ($Profile -ceq $GateDVisualLayoutEvidenceProfile -and
            $entry.Role -ceq 'inputIdentity' -and
            $relativePath.EndsWith(
                '.st',
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $stIdentity = Get-GateDStReplayIdentityFromFile `
                -Path $fullPath `
                -IdentityOwner "GateDVisualLayout baseline ST $relativePath"
            $fileEntry['RawBytes'] = $stIdentity.RawBytes
            $fileEntry['RawSha256'] = $stIdentity.RawSha256
            $fileEntry['CanonicalLfBytes'] = $stIdentity.CanonicalLfBytes
            $fileEntry['CanonicalLfSha256'] =
                $stIdentity.CanonicalLfSha256
            $fileEntry['EolStyle'] = $stIdentity.EolStyle
            $fileEntry['CrLfCount'] = $stIdentity.CrLfCount
            $fileEntry['LfOnlyCount'] = $stIdentity.LfOnlyCount
            $fileEntry['CrOnlyCount'] = $stIdentity.CrOnlyCount
            $fileEntry['LineBreakCount'] = $stIdentity.LineBreakCount
        }
        $files += $fileEntry
    }
    $runningLasalAfterHash = @(
        Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
    if ($runningLasalAfterHash.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            'Lasal2 started while the baseline was being captured; no evidence was written.')
    }

    $evidence = [ordered]@{
        Schema = $EvidenceSchema
        EvidenceProfile = $context.Contract.Profile
        CapturedAtUtc = [DateTime]::UtcNow.ToString('o')
        RepositoryRoot = $context.RepositoryRoot
        CanonicalProjectPath = $context.CanonicalProjectPath
        LasalLogPath = $resolvedLogPath
        LogPrefixLength = $logBytes.LongLength
        LogPrefixSha256 = Get-Sha256Hex -Bytes $logBytes
        RequiredCompileRelativePaths = @(
            $context.Contract.RequiredCompileRelativePaths)
        Files = $files
    }
    $json = $evidence | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText($resolvedEvidencePath, $json, $Utf8NoBom)
    Write-Output (
        "PASS $Owner.Capture prefixBytes=$($logBytes.LongLength) " +
        "profile=$Profile files=$($files.Count) " +
        "requiredCompileFiles=$($context.RequiredCompilePaths.Count) " +
        "evidence=$resolvedEvidencePath")
}

function Get-RequiredEvidenceProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Evidence,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($Evidence.PSObject.Properties.Name -notcontains $Name) {
        Throw-RebuildEvidenceBlocker "baseline property is missing: $Name"
    }
    return $Evidence.$Name
}

function Assert-FrozenBoundedDeltaBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [long]$ManifestByteCount,
        [Parameter(Mandatory = $true)]
        [string]$ManifestSha256
    )

    $actualSha256 = Get-Sha256Hex -Bytes $Bytes
    if ($Bytes.LongLength -ne $GateDBoundedDeltaByteCount -or
        $ManifestByteCount -ne $GateDBoundedDeltaByteCount -or
        $actualSha256 -cne $GateDBoundedDeltaSha256 -or
        $ManifestSha256 -cne $GateDBoundedDeltaSha256) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta length or SHA-256 differs from the frozen Gate D evidence.')
    }
}

function Assert-BoundedSessionIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [object]$Manifest
    )

    $lines = @(Get-IndexedLines -Text $AppendedLogText)
    $sessionStarts = @($lines | Where-Object {
            $_.Text -match (
                '\(INFO\) GUI\]\s+Start Application at ' +
                '\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\s*$')
        })
    $rebuildCommands = @($lines | Where-Object {
            $_.Text -match (
                "\(INFO\) CmdProc\] Executing command 'Rebuild project'\s*$")
        })
    if ($sessionStarts.Count -ne 1 -or $rebuildCommands.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta does not contain one GUI start and one Rebuild command.')
    }

    try {
        $manifestPid = [int]$Manifest.SessionPid
        $manifestTid = [int]$Manifest.RebuildTid
        $actualSessionPid = [int]$sessionStarts[0].Pid
        $actualRebuildPid = [int]$rebuildCommands[0].Pid
        $actualRebuildTid = [int]$rebuildCommands[0].Tid
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            'bounded session PID/TID identity is missing or is not an integer.')
    }
    if ($manifestPid -ne $GateDBoundedSessionPid -or
        $manifestTid -ne $GateDBoundedRebuildTid -or
        $actualSessionPid -ne $GateDBoundedSessionPid -or
        $actualRebuildPid -ne $GateDBoundedSessionPid -or
        $actualRebuildTid -ne $GateDBoundedRebuildTid) {
        Throw-RebuildEvidenceBlocker (
            'bounded manifest PID/TID differs from the frozen raw session identity.')
    }
}

function Get-FrozenHistoricalBoundedLogDeltaText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaselinePath,
        [Parameter(Mandatory = $true)]
        [object]$Baseline,
        [Parameter(Mandatory = $true)]
        [string]$DeltaPath,
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath,
        [Parameter(Mandatory = $true)]
        [string]$TranscriptPath
    )

    $resolvedDeltaPath = Get-NormalizedFullPath -Path $DeltaPath
    $resolvedManifestPath = Get-NormalizedFullPath -Path $ManifestPath
    foreach ($requiredPath in @($resolvedDeltaPath, $resolvedManifestPath)) {
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            Throw-RebuildEvidenceBlocker (
                "bounded repository evidence does not exist: $requiredPath")
        }
    }

    $manifestBytes = [System.IO.File]::ReadAllBytes($resolvedManifestPath)
    $manifestSha256 = Get-Sha256Hex -Bytes $manifestBytes
    if ($manifestBytes.LongLength -ne $GateDBoundedManifestByteCount -or
        $manifestSha256 -cne $GateDBoundedManifestSha256) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta manifest length or SHA-256 differs from frozen evidence.')
    }
    try {
        $manifestText = $Utf8Strict.GetString($manifestBytes)
        $manifest = $manifestText | ConvertFrom-Json
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "bounded delta manifest cannot be parsed: $($_.Exception.Message)")
    }

    $requiredManifestProperties = @(
        'Schema',
        'Provenance',
        'CapturedAtUtc',
        'BaselineFileName',
        'BaselineByteCount',
        'BaselineSha256',
        'BaselinePrefixLength',
        'BaselinePrefixSha256',
        'SourceLogPath',
        'SourceStartOffset',
        'SourceEndOffset',
        'RawDeltaFileName',
        'RawDeltaByteCount',
        'RawDeltaSha256',
        'Encoding',
        'SessionPid',
        'RebuildTid',
        'TranscriptFileName',
        'TranscriptByteCount',
        'TranscriptSha256')
    foreach ($propertyName in $requiredManifestProperties) {
        [void](Get-RequiredEvidenceProperty `
                -Evidence $manifest `
                -Name $propertyName)
    }

    if ([string]$manifest.Schema -cne $BoundedDeltaSchema) {
        Throw-RebuildEvidenceBlocker (
            "bounded delta schema is '$($manifest.Schema)', expected " +
            "'$BoundedDeltaSchema'.")
    }
    if ([string]$manifest.Provenance -cne $BoundedDeltaProvenance) {
        Throw-RebuildEvidenceBlocker 'bounded delta provenance is not canonical.'
    }
    if ([string]$manifest.Encoding -cne 'UTF-8') {
        Throw-RebuildEvidenceBlocker 'bounded delta encoding is not UTF-8.'
    }

    $baselineBytes = [System.IO.File]::ReadAllBytes($BaselinePath)
    $baselineSha256 = Get-Sha256Hex -Bytes $baselineBytes
    if ([string]$manifest.BaselineFileName -cne
            [System.IO.Path]::GetFileName($BaselinePath) -or
        [long]$manifest.BaselineByteCount -ne $baselineBytes.LongLength -or
        [string]$manifest.BaselineSha256 -cne $baselineSha256) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta manifest is not bound to the supplied baseline file.')
    }

    $baselinePrefixLength = [long](Get-RequiredEvidenceProperty `
            -Evidence $Baseline -Name 'LogPrefixLength')
    $baselinePrefixSha256 = [string](Get-RequiredEvidenceProperty `
            -Evidence $Baseline -Name 'LogPrefixSha256')
    if ([long]$manifest.BaselinePrefixLength -ne $baselinePrefixLength -or
        [string]$manifest.BaselinePrefixSha256 -cne $baselinePrefixSha256) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta manifest prefix does not match the baseline capture.')
    }
    if (-not (Test-PathIdentity `
            -Left ([string]$manifest.SourceLogPath) `
            -Right ([string]$Baseline.LasalLogPath))) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta source path differs from the baseline log path.')
    }

    $sourceStartOffset = [long]$manifest.SourceStartOffset
    $sourceEndOffset = [long]$manifest.SourceEndOffset
    $manifestDeltaByteCount = [long]$manifest.RawDeltaByteCount
    if ($sourceStartOffset -ne $GateDBoundedDeltaStartOffset -or
        $sourceStartOffset -ne $baselinePrefixLength -or
        $sourceEndOffset -ne $GateDBoundedDeltaEndOffset -or
        ($sourceEndOffset - $sourceStartOffset) -ne $manifestDeltaByteCount -or
        $manifestDeltaByteCount -ne $GateDBoundedDeltaByteCount) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta range differs from the frozen Gate D byte range.')
    }

    if ([string]$manifest.RawDeltaFileName -cne
            [System.IO.Path]::GetFileName($resolvedDeltaPath)) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta file name differs from its manifest.')
    }
    $deltaBytes = [System.IO.File]::ReadAllBytes($resolvedDeltaPath)
    Assert-FrozenBoundedDeltaBytes `
        -Bytes $deltaBytes `
        -ManifestByteCount ([long]$manifest.RawDeltaByteCount) `
        -ManifestSha256 ([string]$manifest.RawDeltaSha256)

    $resolvedTranscriptPath = Get-NormalizedFullPath -Path $TranscriptPath
    if ([string]$manifest.TranscriptFileName -cne
            [System.IO.Path]::GetFileName($resolvedTranscriptPath)) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta transcript file name differs from its manifest.')
    }
    $transcriptBytes = [System.IO.File]::ReadAllBytes($resolvedTranscriptPath)
    $transcriptSha256 = Get-Sha256Hex -Bytes $transcriptBytes
    if ($transcriptBytes.LongLength -ne $GateDTranscriptByteCount -or
        [long]$manifest.TranscriptByteCount -ne $GateDTranscriptByteCount -or
        $transcriptSha256 -cne $GateDTranscriptSha256 -or
        [string]$manifest.TranscriptSha256 -cne $GateDTranscriptSha256) {
        Throw-RebuildEvidenceBlocker (
            'bounded delta transcript length or SHA-256 differs from the frozen evidence.')
    }

    try {
        $deltaText = $Utf8Strict.GetString($deltaBytes)
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "bounded delta is not strict UTF-8: $($_.Exception.Message)")
    }
    Assert-BoundedSessionIdentity `
        -AppendedLogText $deltaText `
        -Manifest $manifest
    return $deltaText
}

function Assert-ProfileBoundedSessionIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [object]$Manifest
    )

    $lines = @(Get-IndexedLines -Text $AppendedLogText)
    $sessionStarts = @($lines | Where-Object {
            $_.Text -match (
                '\(INFO\) GUI\]\s+Start Application at ' +
                '\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\s*$')
        })
    $rebuildCommands = @($lines | Where-Object {
            $_.Text -match (
                "\(INFO\) CmdProc\] Executing command 'Rebuild project'\s*$")
        })
    if ($sessionStarts.Count -ne 1 -or $rebuildCommands.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta does not contain one GUI start and one Rebuild command.')
    }

    try {
        $manifestPid = [int]$Manifest.SessionPid
        $manifestTid = [int]$Manifest.RebuildTid
        $actualSessionPid = [int]$sessionStarts[0].Pid
        $actualRebuildPid = [int]$rebuildCommands[0].Pid
        $actualRebuildTid = [int]$rebuildCommands[0].Tid
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            'profile bounded PID/TID identity is missing or is not an integer.')
    }
    if ($manifestPid -le 0 -or $manifestTid -le 0 -or
        $actualSessionPid -ne $manifestPid -or
        $actualRebuildPid -ne $manifestPid -or
        $actualRebuildTid -ne $manifestTid -or
        $rebuildCommands[0].Index -le $sessionStarts[0].Index) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded manifest PID/TID differs from the raw session identity.')
    }
}

function Get-ProfileBoundedLogDeltaText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaselinePath,
        [Parameter(Mandatory = $true)]
        [object]$Baseline,
        [Parameter(Mandatory = $true)]
        [string]$DeltaPath,
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath,
        [Parameter(Mandatory = $true)]
        [string]$TranscriptPath,
        [Parameter(Mandatory = $true)]
        [ValidateSet('GateDVisualLayout')]
        [string]$Profile,
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $resolvedDeltaPath = Get-NormalizedFullPath -Path $DeltaPath
    $resolvedManifestPath = Get-NormalizedFullPath -Path $ManifestPath
    foreach ($requiredPath in @($resolvedDeltaPath, $resolvedManifestPath)) {
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            Throw-RebuildEvidenceBlocker (
                "profile bounded repository evidence does not exist: $requiredPath")
        }
    }

    try {
        $manifestBytes = [System.IO.File]::ReadAllBytes($resolvedManifestPath)
        $manifestText = $Utf8Strict.GetString($manifestBytes)
        $manifest = $manifestText | ConvertFrom-Json
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "profile bounded delta manifest cannot be parsed: $($_.Exception.Message)")
    }
    $requiredManifestProperties = @(
        'Schema',
        'EvidenceProfile',
        'Provenance',
        'CapturedAtUtc',
        'BaselineFileName',
        'BaselineByteCount',
        'BaselineSha256',
        'BaselinePrefixLength',
        'BaselinePrefixSha256',
        'SourceLogPath',
        'SourceStartOffset',
        'SourceEndOffset',
        'RawDeltaFileName',
        'RawDeltaByteCount',
        'RawDeltaSha256',
        'Encoding',
        'SessionPid',
        'RebuildTid',
        'TranscriptFileName',
        'TranscriptByteCount',
        'TranscriptSha256',
        'RegeneratedOutputs')
    foreach ($propertyName in $requiredManifestProperties) {
        [void](Get-RequiredEvidenceProperty `
                -Evidence $manifest `
                -Name $propertyName)
    }
    if ([string]$manifest.Schema -cne $BoundedDeltaSchema -or
        [string]$manifest.EvidenceProfile -cne $Profile) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta schema or evidence profile is not canonical.')
    }
    if ([string]$manifest.Provenance -cne $BoundedDeltaProvenance -or
        [string]$manifest.Encoding -cne 'UTF-8') {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta provenance or encoding is not canonical.')
    }
    $capturedAt = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse(
            [string]$manifest.CapturedAtUtc,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [System.Globalization.DateTimeStyles]::RoundtripKind,
            [ref]$capturedAt)) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta capture time is not an ISO-8601 timestamp.')
    }

    $baselineBytes = [System.IO.File]::ReadAllBytes($BaselinePath)
    $baselineSha256 = Get-Sha256Hex -Bytes $baselineBytes
    if ([string]$manifest.BaselineFileName -cne
            [System.IO.Path]::GetFileName($BaselinePath) -or
        [long]$manifest.BaselineByteCount -ne $baselineBytes.LongLength -or
        [string]$manifest.BaselineSha256 -cne $baselineSha256) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded manifest is not bound to the supplied baseline file.')
    }
    $baselinePrefixLength = [long](Get-RequiredEvidenceProperty `
            -Evidence $Baseline -Name 'LogPrefixLength')
    $baselinePrefixSha256 = [string](Get-RequiredEvidenceProperty `
            -Evidence $Baseline -Name 'LogPrefixSha256')
    if ([long]$manifest.BaselinePrefixLength -ne $baselinePrefixLength -or
        [string]$manifest.BaselinePrefixSha256 -cne $baselinePrefixSha256 -or
        -not (Test-PathIdentity `
            -Left ([string]$manifest.SourceLogPath) `
            -Right ([string]$Baseline.LasalLogPath))) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded manifest prefix or source differs from the baseline.')
    }

    $sourceStartOffset = [long]$manifest.SourceStartOffset
    $sourceEndOffset = [long]$manifest.SourceEndOffset
    $manifestDeltaByteCount = [long]$manifest.RawDeltaByteCount
    if ($sourceStartOffset -ne $baselinePrefixLength -or
        $sourceEndOffset -le $sourceStartOffset -or
        ($sourceEndOffset - $sourceStartOffset) -ne $manifestDeltaByteCount) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta range is not bound to the baseline prefix.')
    }
    if ([string]$manifest.RawDeltaFileName -cne
            [System.IO.Path]::GetFileName($resolvedDeltaPath)) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta file name differs from its manifest.')
    }
    $deltaBytes = [System.IO.File]::ReadAllBytes($resolvedDeltaPath)
    $deltaSha256 = Get-Sha256Hex -Bytes $deltaBytes
    if ($deltaBytes.LongLength -ne $manifestDeltaByteCount -or
        [string]$manifest.RawDeltaSha256 -cne $deltaSha256) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded delta length or SHA-256 differs from its manifest.')
    }

    $resolvedTranscriptPath = Get-NormalizedFullPath -Path $TranscriptPath
    if ([string]$manifest.TranscriptFileName -cne
            [System.IO.Path]::GetFileName($resolvedTranscriptPath)) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded transcript file name differs from its manifest.')
    }
    $transcriptBytes = [System.IO.File]::ReadAllBytes($resolvedTranscriptPath)
    $transcriptSha256 = Get-Sha256Hex -Bytes $transcriptBytes
    if ($transcriptBytes.LongLength -ne [long]$manifest.TranscriptByteCount -or
        [string]$manifest.TranscriptSha256 -cne $transcriptSha256) {
        Throw-RebuildEvidenceBlocker (
            'profile bounded transcript length or SHA-256 differs from its manifest.')
    }

    try {
        $deltaText = $Utf8Strict.GetString($deltaBytes)
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "profile bounded delta is not strict UTF-8: $($_.Exception.Message)")
    }
    Assert-ProfileBoundedSessionIdentity `
        -AppendedLogText $deltaText `
        -Manifest $manifest
    [void](Assert-RegeneratedOutputsBound `
            -Manifest $manifest `
            -RepositoryRoot $RepositoryRoot `
            -ExpectedRelativePaths $ExpectedRegeneratedOutputRelativePaths)
    return $deltaText
}

function Get-BoundedLogDeltaText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaselinePath,
        [Parameter(Mandatory = $true)]
        [object]$Baseline,
        [Parameter(Mandatory = $true)]
        [string]$DeltaPath,
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath,
        [Parameter(Mandatory = $true)]
        [string]$TranscriptPath,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile,
        [AllowEmptyString()]
        [string]$RepositoryRoot
    )

    if ($Profile -ceq $HistoricalEvidenceProfile) {
        return Get-FrozenHistoricalBoundedLogDeltaText `
            -BaselinePath $BaselinePath `
            -Baseline $Baseline `
            -DeltaPath $DeltaPath `
            -ManifestPath $ManifestPath `
            -TranscriptPath $TranscriptPath
    }
    if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        Throw-RebuildEvidenceBlocker (
            'GateDVisualLayout bounded verification requires RepositoryRoot.')
    }
    return Get-ProfileBoundedLogDeltaText `
        -BaselinePath $BaselinePath `
        -Baseline $Baseline `
        -DeltaPath $DeltaPath `
        -ManifestPath $ManifestPath `
        -TranscriptPath $TranscriptPath `
        -Profile $Profile `
        -RepositoryRoot $RepositoryRoot
}

function Verify-BuildEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [string]$BaselinePath,
        [Parameter(Mandatory = $true)]
        [string]$TranscriptPath,
        [Parameter(Mandatory = $true)]
        [string]$LogPath,
        [AllowEmptyString()]
        [string]$BoundedDeltaPath,
        [AllowEmptyString()]
        [string]$BoundedDeltaManifestPath,
        [Parameter(Mandatory = $true)]
        [bool]$InvokeFullStatic,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Historical', 'GateDVisualLayout')]
        [string]$Profile
    )

    $context = Get-CanonicalRepositoryContext -Root $Root -Profile $Profile
    $resolvedBaselinePath = Get-NormalizedFullPath -Path $BaselinePath
    $resolvedTranscriptPath = Get-NormalizedFullPath -Path $TranscriptPath
    if (-not (Test-Path -LiteralPath $resolvedBaselinePath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "baseline file does not exist: $resolvedBaselinePath"
    }
    if (-not (Test-Path -LiteralPath $resolvedTranscriptPath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker (
            "build transcript does not exist: $resolvedTranscriptPath")
    }
    $hasBoundedDelta = -not [string]::IsNullOrWhiteSpace($BoundedDeltaPath)
    $hasBoundedManifest = -not [string]::IsNullOrWhiteSpace(
        $BoundedDeltaManifestPath)
    if ($hasBoundedDelta -ne $hasBoundedManifest) {
        Throw-RebuildEvidenceBlocker (
            'BoundedLogDeltaPath and BoundedLogDeltaManifestPath must be supplied together.')
    }
    if ($Profile -ceq $GateDVisualLayoutEvidenceProfile -and
        -not $hasBoundedDelta) {
        Throw-RebuildEvidenceBlocker (
            'GateDVisualLayout verification requires bounded raw and manifest evidence.')
    }

    try {
        $baseline = Get-Content -LiteralPath $resolvedBaselinePath -Raw |
            ConvertFrom-Json
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "baseline JSON cannot be parsed: $($_.Exception.Message)")
    }
    $schema = Get-RequiredEvidenceProperty -Evidence $baseline -Name 'Schema'
    if ($schema -cne $EvidenceSchema) {
        Throw-RebuildEvidenceBlocker (
            "baseline schema is '$schema', expected '$EvidenceSchema'.")
    }
    $baselineProfile = Get-BaselineEvidenceProfile -Baseline $baseline
    if ($baselineProfile -cne $Profile) {
        Throw-RebuildEvidenceBlocker (
            "baseline evidence profile is '$baselineProfile', requested '$Profile'.")
    }
    if ($Profile -ceq $GateDVisualLayoutEvidenceProfile -and
        $baseline.PSObject.Properties.Name -notcontains 'EvidenceProfile') {
        Throw-RebuildEvidenceBlocker (
            'GateDVisualLayout baseline must record EvidenceProfile explicitly.')
    }
    if ($baseline.PSObject.Properties.Name -contains
        'RequiredCompileRelativePaths') {
        Assert-ExactStringSequence `
            -OwnerName 'baseline required compile inventory' `
            -Actual @($baseline.RequiredCompileRelativePaths | ForEach-Object {
                    [string]$_
                }) `
            -Expected $context.Contract.RequiredCompileRelativePaths
    }
    elseif ($Profile -ceq $GateDVisualLayoutEvidenceProfile) {
        Throw-RebuildEvidenceBlocker (
            'GateDVisualLayout baseline required compile inventory is missing.')
    }
    $baselineRoot = Get-RequiredEvidenceProperty `
        -Evidence $baseline -Name 'RepositoryRoot'
    if (-not (Test-PathIdentity -Left $baselineRoot -Right $context.RepositoryRoot)) {
        Throw-RebuildEvidenceBlocker 'baseline repository root differs from verification root.'
    }
    $baselineProject = Get-RequiredEvidenceProperty `
        -Evidence $baseline -Name 'CanonicalProjectPath'
    if (-not (Test-PathIdentity `
            -Left $baselineProject -Right $context.CanonicalProjectPath)) {
        Throw-RebuildEvidenceBlocker 'baseline canonical project path differs.'
    }

    $baselineFiles = @(
        Get-RequiredEvidenceProperty -Evidence $baseline -Name 'Files')
    $baselineIdentityResult = Assert-BaselineFileInventory `
        -BaselineFiles $baselineFiles `
        -Contract $context.Contract `
        -RepositoryRoot $context.RepositoryRoot `
        -Profile $Profile

    $transcriptText = [System.IO.File]::ReadAllText($resolvedTranscriptPath)
    $evidenceSource = 'live-log'
    if ($hasBoundedDelta) {
        $appendedLogText = Get-BoundedLogDeltaText `
            -BaselinePath $resolvedBaselinePath `
            -Baseline $baseline `
            -DeltaPath $BoundedDeltaPath `
            -ManifestPath $BoundedDeltaManifestPath `
            -TranscriptPath $resolvedTranscriptPath `
            -Profile $Profile `
            -RepositoryRoot $context.RepositoryRoot
        $evidenceSource = 'bounded-repository'
    }
    else {
        $baselineLogPath = Get-RequiredEvidenceProperty `
            -Evidence $baseline -Name 'LasalLogPath'
        $resolvedLogPath = Get-NormalizedFullPath -Path $LogPath
        if (-not (Test-PathIdentity `
                -Left $baselineLogPath `
                -Right $resolvedLogPath)) {
            Throw-RebuildEvidenceBlocker (
                'verification Lasal2.log path differs from baseline.')
        }
        $prefixLength = [long](Get-RequiredEvidenceProperty `
                -Evidence $baseline -Name 'LogPrefixLength')
        $prefixSha256 = [string](Get-RequiredEvidenceProperty `
                -Evidence $baseline -Name 'LogPrefixSha256')
        $logBytes = Get-LogBytes -Path $resolvedLogPath
        $currentPrefixSha256 = Get-PrefixSha256Hex `
            -Bytes $logBytes -Length $prefixLength
        if ($currentPrefixSha256 -cne $prefixSha256) {
            Throw-RebuildEvidenceBlocker (
                'Lasal2.log baseline prefix hash changed; log is not append-only.')
        }
        $appendedLogText = Get-AppendedLogText `
            -Bytes $logBytes -Offset $prefixLength
    }
    Assert-LasalC78RebuildEvidence `
        -TranscriptText $transcriptText `
        -AppendedLogText $appendedLogText `
        -CanonicalProjectPath $context.CanonicalProjectPath `
        -RequiredCompilePaths $context.RequiredCompilePaths

    if ($InvokeFullStatic) {
        $contractScript = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
        if (-not (Test-Path -LiteralPath $contractScript -PathType Leaf)) {
            Throw-RebuildEvidenceBlocker (
                "full static verifier does not exist: $contractScript")
        }
        & $contractScript `
            -RepositoryRoot $context.RepositoryRoot `
            -ExpectedSdoWriteAxis 1 `
            -UdpCallbackExpectedState TerminalWakeBrokerCandidate `
            -AllowUdpCallbackDerivedCapture
        if (-not $?) {
            Throw-RebuildEvidenceBlocker 'Verify-LasalContract.ps1 full mode failed.'
        }
    }

    $inputIdentitySummary = if (
        $Profile -ceq $GateDVisualLayoutEvidenceProfile) {
        "inputsEquivalent=true; rawInputsUnchanged=" +
            "$($baselineIdentityResult.RawInputUnchangedCount)/" +
            "$($baselineIdentityResult.InputIdentityCount) " +
            "replayEquivalentSt=" +
            "$($baselineIdentityResult.ReplayEquivalentStCount)"
    }
    else {
        'inputsUnchanged=true'
    }
    Write-Output (
        "PASS $Owner.Verify C78/ARM errors=0 " +
        "warnings=$GateDExpectedSourceWarningCount " +
        "compilerDone=$GateDExpectedCompilerDoneCount " +
        "linkerDone=$GateDExpectedLinkerDoneCount " +
        "postResultCompatibilityWarnings=6/$GateDCompatibilityCompilerVersion " +
        "profile=$Profile $inputIdentitySummary " +
        "regeneratedOutputsBound=$($baselineIdentityResult.RegeneratedOutputCount) " +
        "evidenceSource=$evidenceSource")
}

function New-SyntheticTranscript {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(
        'Compiler: [INFO] Rebuild project with compiler version C78 (target architecture: ARM)',
        "OutputCommand: [INFO] Save project '$CanonicalProjectPath'."
    )
    foreach ($compilePath in $RequiredCompilePaths) {
        $lines += 'Compiler: [INFO] Compiling "' + $compilePath + '"'
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0069'];
        $index++) {
        $lines += (
            'Compiler: [WARN] W 0069 "synthetic.st"(' +
            ($index + 1) + ') Condition is always TRUE')
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0070'];
        $index++) {
        $lines += (
            'Compiler: [WARN] W 0070 "synthetic.st"(' +
            ($index + 51) + ") Possibly mixing of '&' and 'AND'")
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0072'];
        $index++) {
        $lines += (
            'Compiler: [WARN] W 0072 "synthetic.st"(' +
            ($index + 101) + ") 'unused' declared but never used")
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0073'];
        $index++) {
        $lines += (
            'Compiler: [WARN] W 0073 "synthetic.st"(' +
            ($index + 201) + ") Parameter 'input' is never used")
    }
    $lines += (
        "Done - 0 error(s), $GateDExpectedSourceWarningCount warning(s).")
    return $lines -join "`r`n"
}

function New-SyntheticAppendedLog {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(
        '[11:59:59 P:01234 T:00001 (INFO) GUI] Start Application at 2026-08-05 11:59:56',
        "[12:00:00 P:01234 T:00001 (INFO) CmdProc] Executing command 'Load Project `"$CanonicalProjectPath`"'",
        "[12:00:01 P:01234 T:05678 (INFO) CmdProc] Executing command 'Rebuild project'",
        '[12:00:01 P:01234 T:05678 (INFO) Compiler] {Clear}',
        '[12:00:01 P:01234 T:05678 (INFO) Compiler] Rebuild project with compiler version C78 (target architecture: ARM)',
        "[12:00:01 P:01234 T:05678 (INFO) OutputCommand] Save project '$CanonicalProjectPath'."
    )
    foreach ($compilePath in $RequiredCompilePaths) {
        $lines += (
            '[12:00:02 P:01234 T:05678 (INFO) Compiler] Compiling "' +
            $compilePath + '"')
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0069'];
        $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
            '"synthetic.st"(' + ($index + 1) + ') Condition is always TRUE')
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0070'];
        $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0070 ' +
            '"synthetic.st"(' + ($index + 51) +
            ") Possibly mixing of '&' and 'AND'")
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0072'];
        $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0072 ' +
            '"synthetic.st"(' + ($index + 101) +
            ") 'unused' declared but never used")
    }
    for ($index = 0;
        $index -lt $GateDExpectedSourceWarningHistogram['0073'];
        $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0073 ' +
            '"synthetic.st"(' + ($index + 201) +
            ") Parameter 'input' is never used")
    }
    $lines += @(
        '[12:00:03 P:01234 T:05678 (INFO) Compiler] Done',
        '[12:00:03 P:01234 T:05678 (INFO) Compiler] Done',
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Linking...',
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Done',
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The current project ' +
            '"Elmo_EtherCAT_Test_4Axis" is using an old compiler version (C78). ' +
            "Latest version: $GateDCompatibilityCompilerVersion. To benefit " +
            'from the latest bugfixes, change the ' +
            'compiler version in the project-properties. |~1' +
            $CanonicalProjectPath + ';1225~'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "Hardware"(' + $GateDCompatibilityCompilerVersion +
            ') differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "MotionLib"(' + $GateDCompatibilityCompilerVersion +
            ') differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "OS Interface"(' + $GateDCompatibilityCompilerVersion +
            ') differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "System"(' + $GateDCompatibilityCompilerVersion +
            ') differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "Tools"(' + $GateDCompatibilityCompilerVersion +
            ') differs from the compiler version of the current project (C78)'),
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] Last command succeeded. (1000.0ms)'
    )
    return $lines -join "`r`n"
}

function Invoke-RebuildEvidenceSelfTest {
    $historicalContract = Get-EvidenceProfileContract `
        -Profile $HistoricalEvidenceProfile
    $layoutContract = Get-EvidenceProfileContract `
        -Profile $GateDVisualLayoutEvidenceProfile
    $expectedEvidenceFileCount = 9
    $expectedRequiredCompileFileCount = 4
    $expectedLayoutEvidenceFileCount = 12
    $expectedLayoutRequiredCompileFileCount = 5
    $expectedGateDWarningCodes = @('0069', '0070', '0072', '0073')
    $gateDSenderRelativePath = (
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/' +
        'LMCUdpCallbackSender/LMCUdpCallbackSender.st')
    if ($historicalContract.EvidenceRelativePaths.Count -ne
        $expectedEvidenceFileCount) {
        throw (
            "$Owner self-test historical evidence file count is " +
            "$($historicalContract.EvidenceRelativePaths.Count), expected " +
            "$expectedEvidenceFileCount.")
    }
    if ($historicalContract.RequiredCompileRelativePaths.Count -ne
        $expectedRequiredCompileFileCount) {
        throw (
            "$Owner self-test historical required compile file count is " +
            "$($historicalContract.RequiredCompileRelativePaths.Count), expected " +
            "$expectedRequiredCompileFileCount.")
    }
    if ($layoutContract.EvidenceRelativePaths.Count -ne
        $expectedLayoutEvidenceFileCount -or
        $layoutContract.RequiredCompileRelativePaths.Count -ne
        $expectedLayoutRequiredCompileFileCount) {
        throw (
            "$Owner self-test layout inventory is evidence=" +
            "$($layoutContract.EvidenceRelativePaths.Count)/" +
            "$expectedLayoutEvidenceFileCount compile=" +
            "$($layoutContract.RequiredCompileRelativePaths.Count)/" +
            "$expectedLayoutRequiredCompileFileCount.")
    }
    foreach ($inventory in @(
            [pscustomobject]@{
                Name = 'historical evidence'
                Paths = $historicalContract.EvidenceRelativePaths
            },
            [pscustomobject]@{
                Name = 'historical required compile'
                Paths = $historicalContract.RequiredCompileRelativePaths
            },
            [pscustomobject]@{
                Name = 'layout evidence'
                Paths = $layoutContract.EvidenceRelativePaths
            },
            [pscustomobject]@{
                Name = 'layout required compile'
                Paths = $layoutContract.RequiredCompileRelativePaths
            })) {
        $uniquePaths = @($inventory.Paths | Select-Object -Unique)
        if ($uniquePaths.Count -ne $inventory.Paths.Count) {
            throw (
                "$Owner self-test $($inventory.Name) inventory contains " +
                'a duplicate path.')
        }
    }
    $layoutExpectedSpecialEntries = @(
        [pscustomobject]@{
            Path = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
            Role = 'expectedRegeneratedOutput'
            RequiredCompile = $false
        },
        [pscustomobject]@{
            Path = (
                'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/' +
                '_UDPTransceiver/_UDPTransceiver.st')
            Role = 'inputIdentity'
            RequiredCompile = $true
        },
        [pscustomobject]@{
            Path = (
                'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/' +
                'Comm_Network/Comm_Network.lcn')
            Role = 'inputIdentity'
            RequiredCompile = $false
        },
        [pscustomobject]@{
            Path = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
            Role = 'expectedRegeneratedOutput'
            RequiredCompile = $false
        })
    foreach ($expectedEntry in $layoutExpectedSpecialEntries) {
        $matches = @($layoutContract.EvidenceEntries | Where-Object {
                $_.RelativePath -ceq $expectedEntry.Path -and
                $_.Role -ceq $expectedEntry.Role
            })
        $compileMatches = @(
            $layoutContract.RequiredCompileRelativePaths | Where-Object {
                $_ -ceq $expectedEntry.Path
            })
        if ($matches.Count -ne 1 -or
            (($compileMatches.Count -eq 1) -ne $expectedEntry.RequiredCompile)) {
            throw (
                "$Owner self-test layout role/compile contract failed: " +
                $expectedEntry.Path)
        }
    }
    $legacyBaselineProfile = Get-BaselineEvidenceProfile `
        -Baseline ([pscustomobject]@{})
    $explicitLayoutBaselineProfile = Get-BaselineEvidenceProfile `
        -Baseline ([pscustomobject]@{
                EvidenceProfile = $GateDVisualLayoutEvidenceProfile
            })
    if ($legacyBaselineProfile -cne $HistoricalEvidenceProfile -or
        $explicitLayoutBaselineProfile -cne
        $GateDVisualLayoutEvidenceProfile) {
        throw "$Owner self-test baseline profile compatibility failed."
    }
    $gateDWarningHistogramTotal = 0
    foreach ($warningCode in $expectedGateDWarningCodes) {
        if (-not $GateDExpectedSourceWarningHistogram.ContainsKey($warningCode)) {
            throw "$Owner self-test Gate D warning code W$warningCode is missing."
        }
        $gateDWarningHistogramTotal +=
            $GateDExpectedSourceWarningHistogram[$warningCode]
    }
    if ($GateDExpectedSourceWarningHistogram.Count -ne
        $expectedGateDWarningCodes.Count -or
        $gateDWarningHistogramTotal -ne $GateDExpectedSourceWarningCount) {
        throw (
            "$Owner self-test Gate D warning contract is inconsistent: " +
            "codes=$($GateDExpectedSourceWarningHistogram.Count)/" +
            "$($expectedGateDWarningCodes.Count) histogramTotal=" +
            "$gateDWarningHistogramTotal/" +
            "$GateDExpectedSourceWarningCount.")
    }
    foreach ($inventory in @(
            [pscustomobject]@{
                Name = 'evidence'
                Paths = $historicalContract.EvidenceRelativePaths
            },
            [pscustomobject]@{
                Name = 'required compile'
                Paths = $historicalContract.RequiredCompileRelativePaths
            })) {
        $senderEntries = @($inventory.Paths | Where-Object {
                $_ -ceq $gateDSenderRelativePath
            })
        if ($senderEntries.Count -ne 1) {
            throw (
                "$Owner self-test Gate D sender $($inventory.Name) entry count is " +
                "$($senderEntries.Count), expected 1.")
        }
    }

    $syntheticRoot = 'C:\work\Elmo\Elmo_Master'
    $canonicalProjectPath = Get-NormalizedFullPath -Path (
        Join-Path $syntheticRoot $CanonicalProjectRelativePath)
    $requiredCompilePaths = @(
        $historicalContract.RequiredCompileRelativePaths | ForEach-Object {
            Get-NormalizedFullPath -Path (Join-Path $syntheticRoot $_)
        }
    )
    $goodTranscript = New-SyntheticTranscript `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths
    $goodLog = New-SyntheticAppendedLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths

    $transcriptCompileCount = @(
        $goodTranscript -split '\r?\n' | Where-Object {
            $_ -match '^Compiler: \[INFO\] Compiling "'
        }).Count
    $logCompileCount = @(
        $goodLog -split '\r?\n' | Where-Object {
            $_ -match '\(INFO\) Compiler\] Compiling "'
        }).Count
    if ($transcriptCompileCount -ne $expectedRequiredCompileFileCount -or
        $logCompileCount -ne $expectedRequiredCompileFileCount) {
        throw (
            "$Owner self-test Gate D synthetic compile counts are " +
            "transcript=$transcriptCompileCount/log=$logCompileCount, expected " +
            "$expectedRequiredCompileFileCount/$expectedRequiredCompileFileCount.")
    }

    Assert-LasalC78RebuildEvidence `
        -TranscriptText $goodTranscript `
        -AppendedLogText $goodLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths

    $layoutRequiredCompilePaths = @(
        $layoutContract.RequiredCompileRelativePaths | ForEach-Object {
            Get-NormalizedFullPath -Path (Join-Path $syntheticRoot $_)
        })
    $layoutTranscript = New-SyntheticTranscript `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $layoutRequiredCompilePaths
    $layoutLog = New-SyntheticAppendedLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $layoutRequiredCompilePaths
    Assert-LasalC78RebuildEvidence `
        -TranscriptText $layoutTranscript `
        -AppendedLogText $layoutLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $layoutRequiredCompilePaths
    $layoutUdpPath = Get-NormalizedFullPath -Path (
        Join-Path $syntheticRoot `
            $GateDVisualLayoutAdditionalRequiredCompileRelativePaths[0])
    $missingLayoutUdpTranscript = $layoutTranscript.Replace(
        'Compiler: [INFO] Compiling "' + $layoutUdpPath + '"' + "`r`n",
        '')
    $layoutMissingCompileRejected = $false
    try {
        Assert-LasalC78RebuildEvidence `
            -TranscriptText $missingLayoutUdpTranscript `
            -AppendedLogText $layoutLog `
            -CanonicalProjectPath $canonicalProjectPath `
            -RequiredCompilePaths $layoutRequiredCompilePaths
    }
    catch {
        if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
            throw
        }
        $layoutMissingCompileRejected = $true
    }
    if (-not $layoutMissingCompileRejected) {
        throw "$Owner self-test failed to require the layout UDP compile line."
    }

    $currentLikeErrors = @(
        'Compiler: [ERROR] E 0166 "synthetic.st"(4368) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4370) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4399) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4401) Incompatible types.'
    ) -join "`r`n"
    $goodSummary = (
        "Done - 0 error(s), $GateDExpectedSourceWarningCount warning(s).")
    $badFourErrorTranscript = $goodTranscript.Replace(
        $goodSummary,
        $currentLikeErrors + "`r`nDone - 4 error(s), " +
            "$GateDExpectedSourceWarningCount warning(s)." +
            "`r`nOutputCommand: [ERROR] Command 'Rebuild project' failed.")
    $badFourErrorLog = $goodLog.Replace(
        'Last command succeeded.',
        'Last command failed.')

    $postResultWarning = (
        '[12:00:04 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
        '"synthetic.st"(999) Condition is always TRUE')
    $badPostResultLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}' +
            "`r`n$postResultWarning")

    $noncanonicalRoot = 'C:\work\Elmo\Elmo_Master_test'
    $badNoncanonicalTranscript = $goodTranscript.Replace(
        $syntheticRoot,
        $noncanonicalRoot)
    $badNoncanonicalLog = $goodLog.Replace(
        $syntheticRoot,
        $noncanonicalRoot)
    $badInterleavedPidLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:04 P:09999 T:05678 (INFO) Compiler] {ResultCount}')
    $badMissingResultLog = $goodLog.Replace(
        "[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}`r`n",
        '')
    $badMissingTerminalLog = $goodLog.Replace(
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] Last command succeeded. (1000.0ms)',
        '')
    $badMissingClearLog = $goodLog.Replace(
        "[12:00:01 P:01234 T:05678 (INFO) Compiler] {Clear}`r`n",
        '')
    $badMissingRawSaveLog = $goodLog.Replace(
        "[12:00:01 P:01234 T:05678 (INFO) OutputCommand] Save project '$canonicalProjectPath'.`r`n",
        '')
    $loadLine = (
        "[12:00:00 P:01234 T:00001 (INFO) CmdProc] Executing command " +
        "'Load Project `"$canonicalProjectPath`"'")
    $badSessionBoundaryLog = $goodLog.Replace(
        $loadLine,
        $loadLine + "`r`n" +
            '[12:00:00 P:01234 T:00002 (INFO) GUI] Start Application at 2026-08-05 12:00:00')
    $terminalLine = (
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] ' +
        'Last command succeeded. (1000.0ms)')
    $badCommandBoundaryLog = $goodLog.Replace(
        $terminalLine,
        "[12:00:05 P:01234 T:00001 (INFO) CmdProc] Executing command 'Close Project'`r`n" +
            $terminalLine)
    $badSuccessThenFailureLog = $goodLog + "`r`n" +
        '[12:00:06 P:01234 T:05678 (ERROR) OutputCommand] ' +
        "Command 'Rebuild project' failed."
    $firstRawWarning = (
        '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
        '"synthetic.st"(1) Condition is always TRUE')
    $badMissingRawWarningLog = $goodLog.Replace(
        $firstRawWarning + "`r`n",
        '')
    $badRawErrorLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:03 P:01234 T:05678 (ERROR) Compiler] E 0166 ' +
            '"synthetic.st"(4368) Incompatible types.' + "`r`n" +
            '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}')
    $badRawHistogramLog = $goodLog.Replace(
        $firstRawWarning,
        $firstRawWarning.Replace('W 0069', 'W 0072'))
    $firstTranscriptWarning = (
        'Compiler: [WARN] W 0069 "synthetic.st"(1) Condition is always TRUE')
    $badTranscriptHistogram = $goodTranscript.Replace(
        $firstTranscriptWarning,
        $firstTranscriptWarning.Replace('W 0069', 'W 0070'))
    $historical55Transcript = @(
        $goodTranscript -split '\r?\n' | Where-Object {
            $_ -notmatch '^Compiler: \[WARN\] W 0070\b'
        }) -join "`r`n"
    $historical55Transcript = $historical55Transcript.Replace(
        $goodSummary,
        'Done - 0 error(s), 55 warning(s).')
    $historical55Log = @(
        $goodLog -split '\r?\n' | Where-Object {
            $_ -notmatch '\(WARN\) Compiler\] W 0070\b'
        }) -join "`r`n"

    $compilerDoneLine = (
        '[12:00:03 P:01234 T:05678 (INFO) Compiler] Done')
    $linkerLinkingLine = (
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Linking...')
    $linkerDoneLine = (
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Done')
    $badCompilerDoneCountLog = $goodLog.Replace(
        $compilerDoneLine + "`r`n" + $compilerDoneLine,
        $compilerDoneLine)
    $badMissingLinkerDoneLog = $goodLog.Replace(
        $linkerDoneLine + "`r`n",
        '')
    $badCompletionOrderLog = $goodLog.Replace(
        $compilerDoneLine + "`r`n" +
            $compilerDoneLine + "`r`n" +
            $linkerLinkingLine + "`r`n" +
            $linkerDoneLine,
        $linkerDoneLine + "`r`n" +
            $compilerDoneLine + "`r`n" +
            $compilerDoneLine + "`r`n" +
            $linkerLinkingLine)
    $badC81CompatibilityLog = $goodLog.Replace(
        $GateDCompatibilityCompilerVersion,
        'C81')
    $badDownloadLog = $goodLog + "`r`n" +
        "[12:00:06 P:01234 T:00001 (INFO) CmdProc] Executing command 'Download Project (5 sub commands)"

    $firstCompileLine = (
        'Compiler: [INFO] Compiling "' + $requiredCompilePaths[0] + '"')
    $badTranscriptOrdering = $goodTranscript.Replace(
        $firstCompileLine + "`r`n",
        '').Replace(
        $goodSummary,
        "$goodSummary`r`n$firstCompileLine")
    $gateDSenderPath = Get-NormalizedFullPath -Path (
        Join-Path $syntheticRoot $gateDSenderRelativePath)
    $gateDSenderTranscriptCompileLine = (
        'Compiler: [INFO] Compiling "' + $gateDSenderPath + '"')
    $gateDSenderLogCompileLine = (
        '[12:00:02 P:01234 T:05678 (INFO) Compiler] Compiling "' +
        $gateDSenderPath + '"')
    $badMissingGateDSenderTranscript = $goodTranscript.Replace(
        $gateDSenderTranscriptCompileLine + "`r`n",
        '')
    $badMissingGateDSenderLog = $goodLog.Replace(
        $gateDSenderLogCompileLine + "`r`n",
        '')

    $negativeFixtures = @(
        [pscustomobject]@{
            Name = 'current-like 4/76 failure'
            Transcript = $badFourErrorTranscript
            Log = $badFourErrorLog
        },
        [pscustomobject]@{
            Name = 'post-result warning exclusion'
            Transcript = $goodTranscript
            Log = $badPostResultLog
        },
        [pscustomobject]@{
            Name = 'noncanonical project'
            Transcript = $badNoncanonicalTranscript
            Log = $badNoncanonicalLog
        },
        [pscustomobject]@{
            Name = 'interleaved PID'
            Transcript = $goodTranscript
            Log = $badInterleavedPidLog
        },
        [pscustomobject]@{
            Name = 'missing ResultCount'
            Transcript = $goodTranscript
            Log = $badMissingResultLog
        },
        [pscustomobject]@{
            Name = 'missing terminal'
            Transcript = $goodTranscript
            Log = $badMissingTerminalLog
        },
        [pscustomobject]@{
            Name = 'missing raw Clear'
            Transcript = $goodTranscript
            Log = $badMissingClearLog
        },
        [pscustomobject]@{
            Name = 'missing raw canonical Save'
            Transcript = $goodTranscript
            Log = $badMissingRawSaveLog
        },
        [pscustomobject]@{
            Name = 'PID reuse session boundary'
            Transcript = $goodTranscript
            Log = $badSessionBoundaryLog
        },
        [pscustomobject]@{
            Name = 'terminal after next command boundary'
            Transcript = $goodTranscript
            Log = $badCommandBoundaryLog
        },
        [pscustomobject]@{
            Name = 'success followed by rebuild failure'
            Transcript = $goodTranscript
            Log = $badSuccessThenFailureLog
        },
        [pscustomobject]@{
            Name = 'missing raw compiler warning'
            Transcript = $goodTranscript
            Log = $badMissingRawWarningLog
        },
        [pscustomobject]@{
            Name = 'raw compiler error'
            Transcript = $goodTranscript
            Log = $badRawErrorLog
        },
        [pscustomobject]@{
            Name = 'raw warning histogram drift'
            Transcript = $goodTranscript
            Log = $badRawHistogramLog
        },
        [pscustomobject]@{
            Name = 'transcript warning histogram drift'
            Transcript = $badTranscriptHistogram
            Log = $goodLog
        },
        [pscustomobject]@{
            Name = 'historical 55 transcript warning contract'
            Transcript = $historical55Transcript
            Log = $goodLog
        },
        [pscustomobject]@{
            Name = 'historical 55 raw warning contract'
            Transcript = $goodTranscript
            Log = $historical55Log
        },
        [pscustomobject]@{
            Name = 'missing one Compiler Done'
            Transcript = $goodTranscript
            Log = $badCompilerDoneCountLog
        },
        [pscustomobject]@{
            Name = 'missing Linker Done'
            Transcript = $goodTranscript
            Log = $badMissingLinkerDoneLog
        },
        [pscustomobject]@{
            Name = 'Linker Done before Compiler Done'
            Transcript = $goodTranscript
            Log = $badCompletionOrderLog
        },
        [pscustomobject]@{
            Name = 'stale C81 compatibility warnings'
            Transcript = $goodTranscript
            Log = $badC81CompatibilityLog
        },
        [pscustomobject]@{
            Name = 'transcript compile after terminal result'
            Transcript = $badTranscriptOrdering
            Log = $goodLog
        },
        [pscustomobject]@{
            Name = 'missing Gate D sender transcript compile'
            Transcript = $badMissingGateDSenderTranscript
            Log = $goodLog
        },
        [pscustomobject]@{
            Name = 'missing Gate D sender raw compile'
            Transcript = $goodTranscript
            Log = $badMissingGateDSenderLog
        },
        [pscustomobject]@{
            Name = 'prohibited download'
            Transcript = $goodTranscript
            Log = $badDownloadLog
        }
    )

    $rejected = 0
    foreach ($fixture in $negativeFixtures) {
        $wasRejected = $false
        try {
            Assert-LasalC78RebuildEvidence `
                -TranscriptText $fixture.Transcript `
                -AppendedLogText $fixture.Log `
                -CanonicalProjectPath $canonicalProjectPath `
                -RequiredCompilePaths $requiredCompilePaths
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $wasRejected = $true
        }
        if (-not $wasRejected) {
            throw "$Owner self-test failed to reject fixture: $($fixture.Name)"
        }
        $rejected++
    }

    $selfTestRepositoryRoot = Get-NormalizedFullPath -Path (
        Join-Path $PSScriptRoot '..\..\..\..')
    $boundedReportDirectory = Join-Path `
        $selfTestRepositoryRoot `
        'test/Reports_Lasal/C78_20260810_udp_callback_gate_d'
    $boundedBaselinePath = Join-Path $boundedReportDirectory 'build_baseline.json'
    $boundedDeltaPath = Join-Path `
        $boundedReportDirectory `
        'bounded_lasal2_delta_4962208_5727932.raw.txt'
    $boundedManifestPath = Join-Path `
        $boundedReportDirectory `
        'bounded_lasal2_delta_4962208_5727932.manifest.json'
    $boundedTranscriptPath = Join-Path `
        $boundedReportDirectory `
        'derived_build_transcript_from_lasal2_log.txt'
    foreach ($boundedPath in @(
            $boundedBaselinePath,
            $boundedDeltaPath,
            $boundedManifestPath,
            $boundedTranscriptPath)) {
        if (-not (Test-Path -LiteralPath $boundedPath -PathType Leaf)) {
            throw "$Owner self-test bounded fixture is missing: $boundedPath"
        }
    }
    $boundedBaseline = Get-Content -Raw -LiteralPath $boundedBaselinePath |
        ConvertFrom-Json
    $boundedText = Get-BoundedLogDeltaText `
        -BaselinePath $boundedBaselinePath `
        -Baseline $boundedBaseline `
        -DeltaPath $boundedDeltaPath `
        -ManifestPath $boundedManifestPath `
        -TranscriptPath $boundedTranscriptPath `
        -Profile $HistoricalEvidenceProfile
    if ([string]::IsNullOrEmpty($boundedText)) {
        throw "$Owner self-test bounded positive fixture is empty."
    }

    $boundedManifest = Get-Content -Raw -LiteralPath $boundedManifestPath |
        ConvertFrom-Json
    $boundedBytes = [System.IO.File]::ReadAllBytes($boundedDeltaPath)
    $boundedNegativeFixtures = @()
    $tamperedBoundedBytes = [byte[]]($boundedBytes.Clone())
    $tamperedBoundedBytes[0] = $tamperedBoundedBytes[0] -bxor 1
    $boundedNegativeFixtures += [pscustomobject]@{
        Name = 'bounded byte tamper'
        Bytes = $tamperedBoundedBytes
    }
    $truncatedBoundedBytes = [byte[]]::new($boundedBytes.Length - 1)
    [System.Array]::Copy(
        $boundedBytes,
        0,
        $truncatedBoundedBytes,
        0,
        $truncatedBoundedBytes.Length)
    $boundedNegativeFixtures += [pscustomobject]@{
        Name = 'bounded byte truncation'
        Bytes = $truncatedBoundedBytes
    }
    $boundedRejected = 0
    foreach ($boundedFixture in $boundedNegativeFixtures) {
        $boundedWasRejected = $false
        try {
            Assert-FrozenBoundedDeltaBytes `
                -Bytes $boundedFixture.Bytes `
                -ManifestByteCount ([long]$boundedManifest.RawDeltaByteCount) `
                -ManifestSha256 ([string]$boundedManifest.RawDeltaSha256)
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $boundedWasRejected = $true
        }
        if (-not $boundedWasRejected) {
            throw (
                "$Owner self-test failed to reject bounded fixture: " +
                $boundedFixture.Name)
        }
        $boundedRejected++
    }

    $badPidManifest = ($boundedManifest | ConvertTo-Json -Depth 4) |
        ConvertFrom-Json
    $badPidManifest.SessionPid = $GateDBoundedSessionPid + 1
    $badTidManifest = ($boundedManifest | ConvertTo-Json -Depth 4) |
        ConvertFrom-Json
    $badTidManifest.RebuildTid = $GateDBoundedRebuildTid + 1
    $boundedManifestNegativeFixtures = @(
        [pscustomobject]@{
            Name = 'bounded manifest session PID mutation'
            Manifest = $badPidManifest
        },
        [pscustomobject]@{
            Name = 'bounded manifest Rebuild TID mutation'
            Manifest = $badTidManifest
        }
    )
    $boundedManifestRejected = 0
    foreach ($manifestFixture in $boundedManifestNegativeFixtures) {
        $manifestWasRejected = $false
        try {
            Assert-BoundedSessionIdentity `
                -AppendedLogText $boundedText `
                -Manifest $manifestFixture.Manifest
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $manifestWasRejected = $true
        }
        if (-not $manifestWasRejected) {
            throw (
                "$Owner self-test failed to reject bounded manifest fixture: " +
                $manifestFixture.Name)
        }
        $boundedManifestRejected++
    }

    $profileFixtureRoot = Join-Path `
        ([System.IO.Path]::GetTempPath()) `
        ('LasalC78ProfileSelfTest_' + [guid]::NewGuid().ToString('N'))
    [void][System.IO.Directory]::CreateDirectory($profileFixtureRoot)
    $profileBoundedRejected = $false
    $profileManifestNegativeRejected = 0
    $profileManifestNegativeExpected = 7
    $layoutReplayEquivalentAccepted = $false
    $layoutInputNegativeRejected = 0
    $layoutInputNegativeExpected = 3
    try {
        $layoutBaselineFiles = @()
        $layoutOriginalBytes = @{}
        $layoutStInputPathsByStyle = @{
            LF = @()
            CRLF = @()
            Mixed = @()
        }
        for ($index = 0; $index -lt
            $layoutContract.EvidenceEntries.Count; $index++) {
            $entry = $layoutContract.EvidenceEntries[$index]
            $fullPath = Get-NormalizedFullPath -Path (
                Join-Path $profileFixtureRoot $entry.RelativePath)
            [void][System.IO.Directory]::CreateDirectory(
                (Split-Path -Parent $fullPath))
            $isStInput = (
                $entry.Role -ceq 'inputIdentity' -and
                $entry.RelativePath.EndsWith(
                    '.st',
                    [System.StringComparison]::OrdinalIgnoreCase))
            if ($isStInput) {
                $line1 = "baseline-$index"
                $line2 = $entry.RelativePath
                $line3 = 'END_FUNCTION'
                switch ($index % 3) {
                    0 {
                        $fileText = $line1 + "`n" + $line2 + "`n" +
                            $line3 + "`n"
                    }
                    1 {
                        $fileText = $line1 + "`r`n" + $line2 + "`r`n" +
                            $line3 + "`r`n"
                    }
                    default {
                        $fileText = $line1 + "`r`n" + $line2 + "`n" +
                            $line3 + "`r`n"
                    }
                }
                $fileBytes = $Utf8NoBom.GetBytes($fileText)
            }
            else {
                $fileBytes = $Utf8NoBom.GetBytes(
                    "baseline-$index-$($entry.RelativePath)")
            }
            [System.IO.File]::WriteAllBytes($fullPath, $fileBytes)
            $layoutOriginalBytes[$entry.RelativePath] = $fileBytes
            $baselineFileEntry = [ordered]@{
                RelativePath = $entry.RelativePath
                Role = $entry.Role
                Sha256 = Get-Sha256Hex -Bytes $fileBytes
            }
            if ($isStInput) {
                $identity = Get-GateDStReplayIdentity `
                    -Bytes $fileBytes `
                    -IdentityOwner "self-test baseline ST $($entry.RelativePath)"
                $baselineFileEntry['RawBytes'] = $identity.RawBytes
                $baselineFileEntry['RawSha256'] = $identity.RawSha256
                $baselineFileEntry['CanonicalLfBytes'] =
                    $identity.CanonicalLfBytes
                $baselineFileEntry['CanonicalLfSha256'] =
                    $identity.CanonicalLfSha256
                $baselineFileEntry['EolStyle'] = $identity.EolStyle
                $baselineFileEntry['CrLfCount'] = $identity.CrLfCount
                $baselineFileEntry['LfOnlyCount'] = $identity.LfOnlyCount
                $baselineFileEntry['CrOnlyCount'] = $identity.CrOnlyCount
                $baselineFileEntry['LineBreakCount'] = $identity.LineBreakCount
                $layoutStInputPathsByStyle[$identity.EolStyle] +=
                    $entry.RelativePath
            }
            $layoutBaselineFiles += [pscustomobject]$baselineFileEntry
        }
        $layoutUnchangedResult = Assert-BaselineFileInventory `
            -BaselineFiles $layoutBaselineFiles `
            -Contract $layoutContract `
            -RepositoryRoot $profileFixtureRoot `
            -Profile $GateDVisualLayoutEvidenceProfile
        if ($layoutUnchangedResult.InputIdentityCount -ne 10 -or
            $layoutUnchangedResult.RegeneratedOutputCount -ne 2 -or
            $layoutUnchangedResult.RawInputUnchangedCount -ne 10 -or
            $layoutUnchangedResult.ReplayEquivalentStCount -ne 0 -or
            -not $layoutUnchangedResult.InputsEquivalent) {
            throw "$Owner self-test layout unchanged role counts are invalid."
        }
        $layoutUnchangedOutputs = @()
        foreach ($relativePath in $ExpectedRegeneratedOutputRelativePaths) {
            $identity = Get-FileRawIdentity -Path (
                Join-Path $profileFixtureRoot $relativePath)
            $layoutUnchangedOutputs += [pscustomobject][ordered]@{
                RelativePath = $relativePath
                Bytes = $identity.Bytes
                Sha256 = $identity.Sha256
            }
        }
        [void](Assert-RegeneratedOutputsBound `
                -Manifest ([pscustomobject]@{
                    RegeneratedOutputs = @($layoutUnchangedOutputs)
                }) `
                -RepositoryRoot $profileFixtureRoot `
                -ExpectedRelativePaths $ExpectedRegeneratedOutputRelativePaths)

        foreach ($relativePath in $ExpectedRegeneratedOutputRelativePaths) {
            $fullPath = Get-NormalizedFullPath -Path (
                Join-Path $profileFixtureRoot $relativePath)
            $changedBytes = $Utf8NoBom.GetBytes(
                "regenerated-$relativePath")
            [System.IO.File]::WriteAllBytes($fullPath, $changedBytes)
        }
        $layoutChangedResult = Assert-BaselineFileInventory `
            -BaselineFiles $layoutBaselineFiles `
            -Contract $layoutContract `
            -RepositoryRoot $profileFixtureRoot `
            -Profile $GateDVisualLayoutEvidenceProfile
        if ($layoutChangedResult.InputIdentityCount -ne 10 -or
            $layoutChangedResult.RegeneratedOutputCount -ne 2 -or
            $layoutChangedResult.RawInputUnchangedCount -ne 10 -or
            $layoutChangedResult.ReplayEquivalentStCount -ne 0 -or
            -not $layoutChangedResult.InputsEquivalent) {
            throw "$Owner self-test layout changed role counts are invalid."
        }
        $layoutChangedOutputs = @()
        foreach ($relativePath in $ExpectedRegeneratedOutputRelativePaths) {
            $identity = Get-FileRawIdentity -Path (
                Join-Path $profileFixtureRoot $relativePath)
            $layoutChangedOutputs += [pscustomobject][ordered]@{
                RelativePath = $relativePath
                Bytes = $identity.Bytes
                Sha256 = $identity.Sha256
            }
        }
        [void](Assert-RegeneratedOutputsBound `
                -Manifest ([pscustomobject]@{
                    RegeneratedOutputs = @($layoutChangedOutputs)
                }) `
                -RepositoryRoot $profileFixtureRoot `
                -ExpectedRelativePaths $ExpectedRegeneratedOutputRelativePaths)

        if ($layoutStInputPathsByStyle.LF.Count -lt 1 -or
            $layoutStInputPathsByStyle.CRLF.Count -lt 1 -or
            $layoutStInputPathsByStyle.Mixed.Count -lt 1) {
            throw "$Owner self-test did not create LF, CRLF, and Mixed ST inputs."
        }
        $replayEquivalentPaths = @(
            $layoutStInputPathsByStyle.LF[0],
            $layoutStInputPathsByStyle.CRLF[0],
            $layoutStInputPathsByStyle.Mixed[0])
        foreach ($relativePath in $replayEquivalentPaths) {
            $fullPath = Get-NormalizedFullPath -Path (
                Join-Path $profileFixtureRoot $relativePath)
            $originalText = $Utf8Strict.GetString(
                [byte[]]$layoutOriginalBytes[$relativePath])
            $baselineEntry = @($layoutBaselineFiles | Where-Object {
                    $_.RelativePath -ceq $relativePath
                })[0]
            $equivalentText = switch ([string]$baselineEntry.EolStyle) {
                'LF' { $originalText.Replace("`n", "`r`n") }
                'CRLF' { $originalText.Replace("`r`n", "`n") }
                'Mixed' { $originalText.Replace("`r`n", "`n") }
                default {
                    throw (
                        "$Owner self-test replay style is unsupported: " +
                        $baselineEntry.EolStyle)
                }
            }
            [System.IO.File]::WriteAllBytes(
                $fullPath,
                $Utf8NoBom.GetBytes($equivalentText))
        }
        $layoutReplayResult = Assert-BaselineFileInventory `
            -BaselineFiles $layoutBaselineFiles `
            -Contract $layoutContract `
            -RepositoryRoot $profileFixtureRoot `
            -Profile $GateDVisualLayoutEvidenceProfile
        if ($layoutReplayResult.InputIdentityCount -ne 10 -or
            $layoutReplayResult.RawInputUnchangedCount -ne 7 -or
            $layoutReplayResult.ReplayEquivalentStCount -ne 3 -or
            -not $layoutReplayResult.InputsEquivalent) {
            throw "$Owner self-test replay-equivalent ST counts are invalid."
        }
        $layoutReplayEquivalentAccepted = $true
        foreach ($relativePath in $replayEquivalentPaths) {
            [System.IO.File]::WriteAllBytes(
                (Join-Path $profileFixtureRoot $relativePath),
                [byte[]]$layoutOriginalBytes[$relativePath])
        }

        $semanticDriftPath = $layoutStInputPathsByStyle.CRLF[0]
        $semanticOriginalText = $Utf8Strict.GetString(
            [byte[]]$layoutOriginalBytes[$semanticDriftPath])
        $nonStInputPath = @($layoutContract.EvidenceEntries | Where-Object {
                $_.Role -ceq 'inputIdentity' -and
                -not $_.RelativePath.EndsWith(
                    '.st',
                    [System.StringComparison]::OrdinalIgnoreCase)
            })[0].RelativePath
        $inputNegativeFixtures = @(
            [pscustomobject]@{
                Name = 'standalone CR ST'
                RelativePath = $layoutStInputPathsByStyle.LF[0]
                Bytes = $Utf8NoBom.GetBytes("line-one`rline-two`n")
            },
            [pscustomobject]@{
                Name = 'semantic ST text drift'
                RelativePath = $semanticDriftPath
                Bytes = $Utf8NoBom.GetBytes(
                    $semanticOriginalText.Replace('baseline-', 'changed--'))
            },
            [pscustomobject]@{
                Name = 'non-ST raw drift'
                RelativePath = $nonStInputPath
                Bytes = $Utf8NoBom.GetBytes('mutated-non-st-input')
            })
        foreach ($inputFixture in $inputNegativeFixtures) {
            $fixturePath = Get-NormalizedFullPath -Path (
                Join-Path $profileFixtureRoot $inputFixture.RelativePath)
            [System.IO.File]::WriteAllBytes($fixturePath, $inputFixture.Bytes)
            $fixtureRejected = $false
            try {
                [void](Assert-BaselineFileInventory `
                        -BaselineFiles $layoutBaselineFiles `
                        -Contract $layoutContract `
                        -RepositoryRoot $profileFixtureRoot `
                        -Profile $GateDVisualLayoutEvidenceProfile)
            }
            catch {
                if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                    throw
                }
                $fixtureRejected = $true
            }
            finally {
                [System.IO.File]::WriteAllBytes(
                    $fixturePath,
                    [byte[]]$layoutOriginalBytes[$inputFixture.RelativePath])
            }
            if (-not $fixtureRejected) {
                throw (
                    "$Owner self-test accepted layout input negative: " +
                    $inputFixture.Name)
            }
            $layoutInputNegativeRejected++
        }
        if ($layoutInputNegativeRejected -ne $layoutInputNegativeExpected) {
            throw (
                "$Owner self-test layout input negative count is " +
                "$layoutInputNegativeRejected, expected " +
                "$layoutInputNegativeExpected.")
        }

        $profileBaselinePath = Join-Path $profileFixtureRoot 'baseline.json'
        $profileDeltaPath = Join-Path $profileFixtureRoot 'delta.raw.txt'
        $profileTranscriptPath = Join-Path $profileFixtureRoot 'transcript.txt'
        $profileManifestPath = Join-Path $profileFixtureRoot 'manifest.json'
        $profileBadManifestPath = Join-Path $profileFixtureRoot 'bad-manifest.json'
        $emptyBytes = [byte[]]::new(0)
        $profileBaseline = [ordered]@{
            Schema = $EvidenceSchema
            EvidenceProfile = $GateDVisualLayoutEvidenceProfile
            RepositoryRoot = $profileFixtureRoot
            CanonicalProjectPath = Get-NormalizedFullPath -Path (
                Join-Path $profileFixtureRoot $CanonicalProjectRelativePath)
            LasalLogPath = 'C:\synthetic\Lasal2.log'
            LogPrefixLength = 0
            LogPrefixSha256 = Get-Sha256Hex -Bytes $emptyBytes
            RequiredCompileRelativePaths = @(
                $layoutContract.RequiredCompileRelativePaths)
            Files = @($layoutBaselineFiles)
        }
        [System.IO.File]::WriteAllText(
            $profileBaselinePath,
            ($profileBaseline | ConvertTo-Json -Depth 4),
            $Utf8NoBom)
        $profileBaselineObject = Get-Content `
            -Raw -LiteralPath $profileBaselinePath | ConvertFrom-Json
        $profileDeltaBytes = $Utf8NoBom.GetBytes($layoutLog)
        $profileTranscriptBytes = $Utf8NoBom.GetBytes($layoutTranscript)
        [System.IO.File]::WriteAllBytes($profileDeltaPath, $profileDeltaBytes)
        [System.IO.File]::WriteAllBytes(
            $profileTranscriptPath,
            $profileTranscriptBytes)
        $profileBaselineBytes = [System.IO.File]::ReadAllBytes(
            $profileBaselinePath)
        $profileRegeneratedOutputs = @()
        foreach ($relativePath in $ExpectedRegeneratedOutputRelativePaths) {
            $identity = Get-FileRawIdentity -Path (
                Join-Path $profileFixtureRoot $relativePath)
            $profileRegeneratedOutputs += [ordered]@{
                RelativePath = $relativePath
                Bytes = $identity.Bytes
                Sha256 = $identity.Sha256
            }
        }
        $profileManifest = [ordered]@{
            Schema = $BoundedDeltaSchema
            EvidenceProfile = $GateDVisualLayoutEvidenceProfile
            Provenance = $BoundedDeltaProvenance
            CapturedAtUtc = [DateTime]::UtcNow.ToString('o')
            BaselineFileName = [System.IO.Path]::GetFileName(
                $profileBaselinePath)
            BaselineByteCount = $profileBaselineBytes.Length
            BaselineSha256 = Get-Sha256Hex -Bytes $profileBaselineBytes
            BaselinePrefixLength = 0
            BaselinePrefixSha256 = Get-Sha256Hex -Bytes $emptyBytes
            SourceLogPath = $profileBaseline.LasalLogPath
            SourceStartOffset = 0
            SourceEndOffset = $profileDeltaBytes.Length
            RawDeltaFileName = [System.IO.Path]::GetFileName($profileDeltaPath)
            RawDeltaByteCount = $profileDeltaBytes.Length
            RawDeltaSha256 = Get-Sha256Hex -Bytes $profileDeltaBytes
            Encoding = 'UTF-8'
            SessionPid = 1234
            RebuildTid = 5678
            TranscriptFileName = [System.IO.Path]::GetFileName(
                $profileTranscriptPath)
            TranscriptByteCount = $profileTranscriptBytes.Length
            TranscriptSha256 = Get-Sha256Hex -Bytes $profileTranscriptBytes
            RegeneratedOutputs = @($profileRegeneratedOutputs)
        }
        [System.IO.File]::WriteAllText(
            $profileManifestPath,
            ($profileManifest | ConvertTo-Json -Depth 4),
            $Utf8NoBom)
        $profileBoundedText = Get-BoundedLogDeltaText `
            -BaselinePath $profileBaselinePath `
            -Baseline $profileBaselineObject `
            -DeltaPath $profileDeltaPath `
            -ManifestPath $profileManifestPath `
            -TranscriptPath $profileTranscriptPath `
            -Profile $GateDVisualLayoutEvidenceProfile `
            -RepositoryRoot $profileFixtureRoot
        if ($profileBoundedText -cne $layoutLog) {
            throw "$Owner self-test profile bounded text changed."
        }

        $profileBadManifest = ($profileManifest | ConvertTo-Json -Depth 4) |
            ConvertFrom-Json
        $profileBadManifest.EvidenceProfile = $HistoricalEvidenceProfile
        [System.IO.File]::WriteAllText(
            $profileBadManifestPath,
            ($profileBadManifest | ConvertTo-Json -Depth 4),
            $Utf8NoBom)
        try {
            [void](Get-BoundedLogDeltaText `
                    -BaselinePath $profileBaselinePath `
                    -Baseline $profileBaselineObject `
                    -DeltaPath $profileDeltaPath `
                    -ManifestPath $profileBadManifestPath `
                    -TranscriptPath $profileTranscriptPath `
                    -Profile $GateDVisualLayoutEvidenceProfile `
                    -RepositoryRoot $profileFixtureRoot)
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $profileBoundedRejected = $true
        }
        if (-not $profileBoundedRejected) {
            throw "$Owner self-test accepted a mismatched bounded profile."
        }

        $manifestNegativeFixtures = @()
        $missingProperty = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $missingProperty.PSObject.Properties.Remove('RegeneratedOutputs')
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'missing regenerated outputs property'
            Manifest = $missingProperty
        }
        $missingEntry = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $missingEntry.RegeneratedOutputs = @(
            $missingEntry.RegeneratedOutputs[0])
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'missing regenerated output entry'
            Manifest = $missingEntry
        }
        $reordered = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $reordered.RegeneratedOutputs = @(
            $reordered.RegeneratedOutputs[1],
            $reordered.RegeneratedOutputs[0])
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'reordered regenerated outputs'
            Manifest = $reordered
        }
        $badPath = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $badPath.RegeneratedOutputs[0].RelativePath += '.wrong'
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'regenerated output path mutation'
            Manifest = $badPath
        }
        $badHash = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $badHash.RegeneratedOutputs[0].Sha256 = '0' * 64
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'regenerated output hash mutation'
            Manifest = $badHash
        }
        $badBytes = ($profileManifest | ConvertTo-Json -Depth 8) |
            ConvertFrom-Json
        $badBytes.RegeneratedOutputs[0].Bytes =
            [long]$badBytes.RegeneratedOutputs[0].Bytes + 1
        $manifestNegativeFixtures += [pscustomobject]@{
            Name = 'regenerated output byte count mutation'
            Manifest = $badBytes
        }
        foreach ($fixture in $manifestNegativeFixtures) {
            $wasRejected = $false
            try {
                [void](Assert-RegeneratedOutputsBound `
                        -Manifest $fixture.Manifest `
                        -RepositoryRoot $profileFixtureRoot `
                        -ExpectedRelativePaths `
                            $ExpectedRegeneratedOutputRelativePaths)
            }
            catch {
                if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                    throw
                }
                $wasRejected = $true
            }
            if (-not $wasRejected) {
                throw (
                    "$Owner self-test accepted manifest fixture: " +
                    $fixture.Name)
            }
            $profileManifestNegativeRejected++
        }

        $driftRelativePath = $ExpectedRegeneratedOutputRelativePaths[0]
        $driftFullPath = Get-NormalizedFullPath -Path (
            Join-Path $profileFixtureRoot $driftRelativePath)
        $driftOriginalBytes = [System.IO.File]::ReadAllBytes($driftFullPath)
        [System.IO.File]::WriteAllBytes(
            $driftFullPath,
            $Utf8NoBom.GetBytes('post-manifest-current-drift'))
        $currentDriftRejected = $false
        try {
            [void](Assert-RegeneratedOutputsBound `
                    -Manifest $profileManifest `
                    -RepositoryRoot $profileFixtureRoot `
                    -ExpectedRelativePaths `
                        $ExpectedRegeneratedOutputRelativePaths)
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $currentDriftRejected = $true
        }
        [System.IO.File]::WriteAllBytes($driftFullPath, $driftOriginalBytes)
        if (-not $currentDriftRejected) {
            throw "$Owner self-test accepted current regenerated output drift."
        }
        $profileManifestNegativeRejected++
        if ($profileManifestNegativeRejected -ne
            $profileManifestNegativeExpected) {
            throw (
                "$Owner self-test regenerated manifest rejections are " +
                "$profileManifestNegativeRejected/" +
                "$profileManifestNegativeExpected.")
        }

        $converterScript = Join-Path `
            $boundedReportDirectory `
            'Convert-Lasal2LogToBuildTranscript.ps1'
        if (-not (Test-Path -LiteralPath $converterScript -PathType Leaf)) {
            throw "$Owner self-test converter is missing: $converterScript"
        }
        $converterBaselinePath = Join-Path `
            $profileFixtureRoot `
            'converter-layout-baseline.json'
        $converterTranscriptPath = Join-Path `
            $profileFixtureRoot `
            'converter-layout-transcript.txt'
        $converterRawPath = Join-Path `
            $profileFixtureRoot `
            'converter-layout-delta.raw.txt'
        $converterManifestPath = Join-Path `
            $profileFixtureRoot `
            'converter-layout-manifest.json'
        $converterBaseline = [ordered]@{
            Schema = $EvidenceSchema
            EvidenceProfile = $GateDVisualLayoutEvidenceProfile
            RepositoryRoot = $profileFixtureRoot
            CanonicalProjectPath = [string]$boundedBaseline.CanonicalProjectPath
            LasalLogPath = Get-NormalizedFullPath -Path $boundedDeltaPath
            LogPrefixLength = 0
            LogPrefixSha256 = Get-Sha256Hex -Bytes $emptyBytes
            RequiredCompileRelativePaths = @(
                $layoutContract.RequiredCompileRelativePaths)
            Files = @($layoutBaselineFiles)
        }
        [System.IO.File]::WriteAllText(
            $converterBaselinePath,
            ($converterBaseline | ConvertTo-Json -Depth 8),
            $Utf8NoBom)
        & $converterScript `
            -EvidenceProfile $GateDVisualLayoutEvidenceProfile `
            -BaselinePath $converterBaselinePath `
            -LasalLogPath $boundedDeltaPath `
            -LogEndOffset ([System.IO.FileInfo]$boundedDeltaPath).Length `
            -OutputPath $converterTranscriptPath `
            -RawDeltaOutputPath $converterRawPath `
            -RawDeltaManifestPath $converterManifestPath |
            Out-Null
        $converterManifest = Get-Content `
            -Raw -LiteralPath $converterManifestPath | ConvertFrom-Json
        $converterBoundCount = Assert-RegeneratedOutputsBound `
            -Manifest $converterManifest `
            -RepositoryRoot $profileFixtureRoot `
            -ExpectedRelativePaths $ExpectedRegeneratedOutputRelativePaths
        if ($converterBoundCount -ne 2) {
            throw (
                "$Owner self-test converter regenerated output count is " +
                "$converterBoundCount, expected 2.")
        }
    }
    finally {
        if ([System.IO.Directory]::Exists($profileFixtureRoot)) {
            [System.IO.Directory]::Delete($profileFixtureRoot, $true)
        }
    }

    Write-Output (
        "PASS $Owner.SelfTest successFixture=accepted " +
        "historicalEvidenceFiles=" +
        "$($historicalContract.EvidenceRelativePaths.Count) " +
        "requiredCompileFiles=$($requiredCompilePaths.Count) " +
        "layoutEvidenceFiles=$($layoutContract.EvidenceRelativePaths.Count) " +
        "layoutRequiredCompileFiles=$($layoutRequiredCompilePaths.Count) " +
        'layoutMissingCompileRejected=true ' +
        'profileBoundedPositive=accepted ' +
        'profileBoundedMismatchRejected=true ' +
        'layoutOutputUnchanged=accepted ' +
        'layoutOutputChanged=accepted ' +
        "layoutReplayEquivalentAccepted=$layoutReplayEquivalentAccepted " +
        "layoutInputNegatives=$layoutInputNegativeRejected/" +
        "$layoutInputNegativeExpected " +
        "regeneratedManifestNegatives=$profileManifestNegativeRejected/" +
        "$profileManifestNegativeExpected " +
        'layoutConverterIntegration=accepted ' +
        "sourceWarnings=$GateDExpectedSourceWarningCount " +
        "compilerDone=$GateDExpectedCompilerDoneCount " +
        "linkerDone=$GateDExpectedLinkerDoneCount " +
        "compatibility=$GateDCompatibilityCompilerVersion " +
        "negativeFixturesRejected=$rejected/$($negativeFixtures.Count) " +
        'boundedPositive=accepted ' +
        "boundedNegativeFixturesRejected=$boundedRejected/" +
        "$($boundedNegativeFixtures.Count) " +
        "boundedManifestNegativeFixturesRejected=$boundedManifestRejected/" +
        $boundedManifestNegativeFixtures.Count)
}

switch ($PSCmdlet.ParameterSetName) {
    'Capture' {
        Capture-BuildBaseline `
            -Root $RepositoryRoot `
            -OutputPath $EvidencePath `
            -LogPath $LasalLogPath `
            -Profile $EvidenceProfile
    }
    'Verify' {
        Verify-BuildEvidence `
            -Root $RepositoryRoot `
            -BaselinePath $EvidencePath `
            -TranscriptPath $BuildTranscriptPath `
            -LogPath $LasalLogPath `
            -BoundedDeltaPath $BoundedLogDeltaPath `
            -BoundedDeltaManifestPath $BoundedLogDeltaManifestPath `
            -InvokeFullStatic $RunFullStatic.IsPresent `
            -Profile $EvidenceProfile
    }
    'SelfTest' {
        Invoke-RebuildEvidenceSelfTest
    }
    default {
        Throw-RebuildEvidenceBlocker 'an action switch is required.'
    }
}

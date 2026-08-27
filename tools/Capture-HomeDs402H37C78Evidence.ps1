[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$BuildLogPath,
    [string]$MethodDirectOpenEvidencePath,
    [string]$NetworkSmokeEvidencePath,
    [string]$OutputPath,
    [datetime]$BuildStartedUtc,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-H37EvidenceCondition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-H37Sha256Hex {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-H37GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $output = & git -C $Root @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    if (($exitCode -ne 0) -and (-not $AllowFailure)) {
        throw "git $($Arguments -join ' ') failed with exit code ${exitCode}: $($output -join [Environment]::NewLine)"
    }
    if ($exitCode -ne 0) {
        return $null
    }
    return (($output | ForEach-Object { $_.ToString() }) -join "`n").Trim()
}

function Get-H37RelativeRepoPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $rootWithSeparator = $Root.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = [System.Uri]$rootWithSeparator
    $pathUri = [System.Uri]$Path
    return [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Assert-H37FreshFile {
    param(
        [Parameter(Mandatory = $true)][System.IO.FileInfo]$File,
        [Parameter(Mandatory = $true)][datetime]$BuildStart
    )
    Assert-H37EvidenceCondition ($File.LastWriteTimeUtc -ge $BuildStart) (
        "Freshness check failed: $($File.FullName) last write $($File.LastWriteTimeUtc.ToString('o')) precedes build start $($BuildStart.ToString('o')).")
}

function Assert-H37PassLine {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Description
    )
    Assert-H37EvidenceCondition ([regex]::IsMatch($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline)) (
        "Missing H37 evidence: $Description")
}

function Invoke-HomeDs402H37C78EvidenceCapture {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$LogPath,
        [Parameter(Mandatory = $true)][string]$DirectOpenPath,
        [Parameter(Mandatory = $true)][string]$NetworkPath,
        [Parameter(Mandatory = $true)][string]$EvidencePath,
        [Parameter(Mandatory = $true)][datetime]$BuildStartUtc,
        [switch]$SkipGitRequirements
    )

    Assert-H37EvidenceCondition ($BuildStartUtc -ne [datetime]::MinValue) 'BuildStartedUtc must be supplied explicitly.'

    $rootFull = [System.IO.Path]::GetFullPath($Root)
    $buildStart = $BuildStartUtc.ToUniversalTime()
    $logFull = [System.IO.Path]::GetFullPath($LogPath)
    $directOpenFull = [System.IO.Path]::GetFullPath($DirectOpenPath)
    $networkFull = [System.IO.Path]::GetFullPath($NetworkPath)
    $evidenceFull = [System.IO.Path]::GetFullPath($EvidencePath)

    $classesPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
    $projectPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'
    $networksPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Networks.lcb'

    $criticalRelativePaths = @(
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st',
        'tools\Verify-HomeDs402H37Activation.ps1',
        'tools\Verify-HomeDs402H37Ownership.ps1',
        'tools\Verify-HomeDs402H37MethodSize.ps1',
        'docs\api\design\HOME_DS402_DESIGN.md'
    )
    $criticalPaths = $criticalRelativePaths | ForEach-Object { Join-Path $rootFull $_ }

    foreach ($required in @($classesPath, $projectPath, $networksPath, $logFull, $directOpenFull, $networkFull) + $criticalPaths) {
        Assert-H37EvidenceCondition (Test-Path -LiteralPath $required -PathType Leaf) "Required H37 evidence input is missing: $required"
    }

    $classesInfo = Get-Item -LiteralPath $classesPath
    $projectInfo = Get-Item -LiteralPath $projectPath
    $networksInfo = Get-Item -LiteralPath $networksPath
    $logInfo = Get-Item -LiteralPath $logFull
    $directOpenInfo = Get-Item -LiteralPath $directOpenFull
    $networkInfo = Get-Item -LiteralPath $networkFull

    foreach ($fresh in @($classesInfo, $projectInfo, $networksInfo, $logInfo, $directOpenInfo, $networkInfo)) {
        Assert-H37FreshFile -File $fresh -BuildStart $buildStart
    }

    $logText = [System.IO.File]::ReadAllText($logFull)
    Assert-H37EvidenceCondition ([regex]::IsMatch($logText, '(?im)\bC78\b')) 'Build log does not contain C78 target evidence.'
    Assert-H37EvidenceCondition ([regex]::IsMatch($logText, '(?im)\bARM\b')) 'Build log does not contain ARM target evidence.'
    Assert-H37EvidenceCondition ([regex]::IsMatch($logText, '(?im)\b0\s+errors?\b')) 'Build log does not contain an explicit zero-error compiler result.'
    Assert-H37EvidenceCondition (-not [regex]::IsMatch($logText, '(?im)\b[1-9][0-9]*\s+errors?\b')) 'Build log contains a nonzero error count.'
    Assert-H37EvidenceCondition ([regex]::IsMatch($logText, '(?im)(?:linker[^\r\n]*\bdone\b|\blink(?:ing)?[^\r\n]*successful\b)')) 'Build log does not contain Linker Done or successful link evidence.'
    Assert-H37EvidenceCondition (-not [regex]::IsMatch($logText, '(?im)CInvalidArgException')) 'Build log contains CInvalidArgException evidence.'

    $directOpenText = [System.IO.File]::ReadAllText($directOpenFull)
    foreach ($method in @(
        'LMCDiagnosticsService::HandleAxisDs402HomeStart',
        'LMCDiagnosticsService::HandleAxisDs402HomeOutcome',
        'LMCDiagnosticsService::HandleAxisDs402HomeRetire',
        'LMCDiagnosticsService::ProcessAxisDs402Home'
    )) {
        Assert-H37PassLine -Text $directOpenText -Pattern ("(?im)^\s*PASS\s+direct-open\s+" + [regex]::Escape($method) + "\s*$") -Description "direct-open $method"
    }

    $networkText = [System.IO.File]::ReadAllText($networkFull)
    foreach ($component in @('TCPMotionInterface', 'LMCControlCommandService', 'LMCDiagnosticsService', 'LMCEcatInputLatch')) {
        Assert-H37PassLine -Text $networkText -Pattern ("(?im)^\s*PASS\s+network-smoke\s+" + [regex]::Escape($component) + "\s*$") -Description "Network smoke $component"
    }

    $gitHead = '<self-test-no-git>'
    $gitStatus = '<self-test-no-git>'
    $artifactRows = New-Object System.Collections.Generic.List[string]
    foreach ($artifactPath in @($classesPath, $projectPath, $networksPath)) {
        $info = Get-Item -LiteralPath $artifactPath
        $relative = (Get-H37RelativeRepoPath -Root $rootFull -Path $artifactPath).Replace('\', '/')
        $headBlob = '<self-test-no-git>'
        $workingBlob = '<self-test-no-git>'
        if (-not $SkipGitRequirements) {
            $headBlob = Get-H37GitText -Root $rootFull -Arguments @('rev-parse', "HEAD:$relative") -AllowFailure
            if ([string]::IsNullOrWhiteSpace($headBlob)) { $headBlob = '<untracked-at-head>' }
            $workingBlob = Get-H37GitText -Root $rootFull -Arguments @('hash-object', '--', $artifactPath)
        }
        $artifactRows.Add("| ``$(Get-H37RelativeRepoPath -Root $rootFull -Path $artifactPath)`` | $($info.Length) | ``$($info.LastWriteTimeUtc.ToString('o'))`` | ``$(Get-H37Sha256Hex -Path $artifactPath)`` | ``$headBlob`` | ``$workingBlob`` |")
    }

    if (-not $SkipGitRequirements) {
        $gitHead = Get-H37GitText -Root $rootFull -Arguments @('rev-parse', 'HEAD')
        Assert-H37EvidenceCondition (-not [string]::IsNullOrWhiteSpace($gitHead)) 'Unable to resolve repository HEAD.'
        $gitStatus = Get-H37GitText -Root $rootFull -Arguments @('status', '--porcelain=v1') -AllowFailure
        if ([string]::IsNullOrEmpty($gitStatus)) { $gitStatus = '<clean>' }
    }

    $sourceRows = New-Object System.Collections.Generic.List[string]
    foreach ($sourcePath in $criticalPaths) {
        $sourceRows.Add("| ``$(Get-H37RelativeRepoPath -Root $rootFull -Path $sourcePath)`` | $((Get-Item -LiteralPath $sourcePath).Length) | ``$(Get-H37Sha256Hex -Path $sourcePath)`` |")
    }

    $capturedUtc = [datetime]::UtcNow
    $markdown = @"
# HomeDS402 H37 Fresh C78 Evidence Capture

- CapturedUtc: ``$($capturedUtc.ToString('o'))``
- BuildStartedUtc: ``$($buildStart.ToString('o'))``
- RepositoryHead: ``$gitHead``
- Target evidence: ``C78 / ARM``
- Compiler evidence: ``0 errors``
- Link evidence: ``PASS pattern found``
- CInvalidArgException: ``0 observed``
- MethodDirectOpen: **PASS**
- NetworkSmoke: **PASS**
- GateResult: **CAPTURED_FOR_REVIEW**
- ArtifactRatchetDecision: **REVIEW_REQUIRED**
- SourceOnlyPhysicalIdentityGate: **REVIEW_REQUIRED**
- CapabilityActivation: **KEEP_OFF**

This receipt proves only that the supplied fresh build/direct-open/network evidence and generated artifact
files form one post-build capture set. It does not approve a changed ``Classes.lcb`` identity by itself.
The repository SourceOnly physical-identity ratchet must be reviewed and updated separately from this
capture, then rerun green before H37-05/H37-06 can be closed. HomeDS402 activation gates remain OFF.

## Generated artifact identity

| Artifact | Bytes | LastWriteUtc | SHA-256 | HEAD blob | Working blob |
|---|---:|---|---|---|---|
$($artifactRows -join "`n")

## Critical source identity

| Source | Bytes | SHA-256 |
|---|---:|---|
$($sourceRows -join "`n")

## Evidence inputs

| Evidence | Bytes | SHA-256 |
|---|---:|---|
| ``$(Get-H37RelativeRepoPath -Root $rootFull -Path $logFull)`` | $($logInfo.Length) | ``$(Get-H37Sha256Hex -Path $logFull)`` |
| ``$(Get-H37RelativeRepoPath -Root $rootFull -Path $directOpenFull)`` | $($directOpenInfo.Length) | ``$(Get-H37Sha256Hex -Path $directOpenFull)`` |
| ``$(Get-H37RelativeRepoPath -Root $rootFull -Path $networkFull)`` | $($networkInfo.Length) | ``$(Get-H37Sha256Hex -Path $networkFull)`` |

## Working tree at capture

    $($gitStatus.Replace("`n", "`n    "))

## Mandatory next review

1. Confirm generated HomeDS402 method declarations/ABI against tracked source and packet map.
2. Confirm ``Classes.lcb``, project ``.lcb`` and ``Network/Networks.lcb`` were produced by the same fresh C78/ARM rebuild represented by this receipt.
3. Review the old/new physical artifact identities; do not ratchet a hash from change alone.
4. After approved ratchet update, rerun full repository SourceOnly on the same source tree.
5. Download/load the approved artifact to the PLC and record DiagnosticsBuild/DiagnosticsBootId/MapRevision before any hardware packet qualification.
6. Keep all five HomeDS402 activation values OFF until H37-07/H37-08 evidence is complete and H37-09 paired activation is explicitly approved.
"@

    $evidenceDirectory = Split-Path -Parent $evidenceFull
    if (-not (Test-Path -LiteralPath $evidenceDirectory)) {
        New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null
    }
    [System.IO.File]::WriteAllText($evidenceFull, $markdown.Replace("`r`n", "`n"), (New-Object System.Text.UTF8Encoding($false)))

    Write-Host 'PASS HomeDS402 H37 fresh C78/ARM build evidence'
    Write-Host 'PASS HomeDS402 H37 method direct-open evidence'
    Write-Host 'PASS HomeDS402 H37 Network smoke evidence'
    Write-Host 'PASS HomeDS402 H37 generated artifact freshness and identity capture'
    Write-Host "PASS evidence captured: $evidenceFull"
    Write-Host 'REVIEW_REQUIRED physical artifact ratchet and SourceOnly closure'
    Write-Host 'KEEP_OFF HomeDS402 capability activation'
    return $evidenceFull
}

function Invoke-H37SelfTest {
    $root = Join-Path ([System.IO.Path]::GetTempPath()) ('HomeDs402H37C78-' + [guid]::NewGuid().ToString('N'))
    try {
        foreach ($relative in @(
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network',
            'tools',
            'docs\api\design',
            'evidence')) {
            New-Item -ItemType Directory -Path (Join-Path $root $relative) -Force | Out-Null
        }

        $critical = @(
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st',
            'tools\Verify-HomeDs402H37Activation.ps1',
            'tools\Verify-HomeDs402H37Ownership.ps1',
            'tools\Verify-HomeDs402H37MethodSize.ps1',
            'docs\api\design\HOME_DS402_DESIGN.md'
        )
        foreach ($relative in $critical) {
            [System.IO.File]::WriteAllText((Join-Path $root $relative), "self-test $relative")
        }

        [System.IO.File]::WriteAllBytes((Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'), [byte[]](1,2,3,4,5))
        [System.IO.File]::WriteAllBytes((Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'), [byte[]](6,7,8,9))
        [System.IO.File]::WriteAllBytes((Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Networks.lcb'), [byte[]](10,11,12))

        $buildStart = [datetime]::UtcNow.AddSeconds(-2)
        $buildLog = Join-Path $root 'evidence\c78-build.log'
        [System.IO.File]::WriteAllText($buildLog, "Target C78 ARM`r`nCompiler: 0 errors / 12 warnings`r`nLinker: Done`r`n")

        $directOpen = Join-Path $root 'evidence\method-direct-open.log'
        [System.IO.File]::WriteAllText($directOpen, @"
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeStart
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeOutcome
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeRetire
PASS direct-open LMCDiagnosticsService::ProcessAxisDs402Home
"@)

        $networkSmoke = Join-Path $root 'evidence\network-smoke.log'
        [System.IO.File]::WriteAllText($networkSmoke, @"
PASS network-smoke TCPMotionInterface
PASS network-smoke LMCControlCommandService
PASS network-smoke LMCDiagnosticsService
PASS network-smoke LMCEcatInputLatch
"@)

        $output = Join-Path $root 'evidence\h37-c78-evidence.md'
        $captured = Invoke-HomeDs402H37C78EvidenceCapture `
            -Root $root `
            -LogPath $buildLog `
            -DirectOpenPath $directOpen `
            -NetworkPath $networkSmoke `
            -EvidencePath $output `
            -BuildStartUtc $buildStart `
            -SkipGitRequirements

        Assert-H37EvidenceCondition (Test-Path -LiteralPath $captured) 'H37 C78 self-test did not create an evidence file.'
        $receipt = [System.IO.File]::ReadAllText($captured)
        Assert-H37EvidenceCondition ($receipt.Contains('ArtifactRatchetDecision: **REVIEW_REQUIRED**')) 'H37 C78 self-test did not preserve artifact review.'
        Assert-H37EvidenceCondition ($receipt.Contains('SourceOnlyPhysicalIdentityGate: **REVIEW_REQUIRED**')) 'H37 C78 self-test did not preserve SourceOnly physical-identity review.'
        Assert-H37EvidenceCondition ($receipt.Contains('CapabilityActivation: **KEEP_OFF**')) 'H37 C78 self-test did not preserve activation OFF.'
        Write-Host 'PASS Capture-HomeDs402H37C78Evidence self-test'
    }
    finally {
        if (Test-Path -LiteralPath $root) {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($SelfTest) {
    Invoke-H37SelfTest
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = (Get-Location).Path }
foreach ($requiredArgument in @(
    @{ Name = 'BuildLogPath'; Value = $BuildLogPath },
    @{ Name = 'MethodDirectOpenEvidencePath'; Value = $MethodDirectOpenEvidencePath },
    @{ Name = 'NetworkSmokeEvidencePath'; Value = $NetworkSmokeEvidencePath },
    @{ Name = 'OutputPath'; Value = $OutputPath })) {
    if ([string]::IsNullOrWhiteSpace($requiredArgument.Value)) {
        throw "$($requiredArgument.Name) is required unless -SelfTest is used."
    }
}

Invoke-HomeDs402H37C78EvidenceCapture `
    -Root $RepositoryRoot `
    -LogPath $BuildLogPath `
    -DirectOpenPath $MethodDirectOpenEvidencePath `
    -NetworkPath $NetworkSmokeEvidencePath `
    -EvidencePath $OutputPath `
    -BuildStartUtc $BuildStartedUtc | Out-Null

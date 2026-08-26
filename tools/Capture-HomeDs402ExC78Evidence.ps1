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

function Assert-HomeExEvidenceCondition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )
    if (-not $Condition) { throw $Message }
}

function Get-HomeExSha256Hex {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-HomeExGitText {
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
    if ($exitCode -ne 0) { return $null }
    return (($output | ForEach-Object { $_.ToString() }) -join "`n").Trim()
}

function Get-HomeExRelativeRepoPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )
    $rootWithSeparator = $Root.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $rootUri = [Uri]$rootWithSeparator
    $pathUri = [Uri]$Path
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Assert-HomeExFreshFile {
    param(
        [Parameter(Mandatory = $true)][IO.FileInfo]$File,
        [Parameter(Mandatory = $true)][datetime]$BuildStart
    )
    Assert-HomeExEvidenceCondition ($File.LastWriteTimeUtc -ge $BuildStart) (
        "Freshness check failed: $($File.FullName) last write $($File.LastWriteTimeUtc.ToString('o')) precedes build start $($BuildStart.ToString('o')).")
}

function Assert-HomeExPassLine {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Description
    )
    $options = [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [Text.RegularExpressions.RegexOptions]::Multiline
    Assert-HomeExEvidenceCondition ([regex]::IsMatch($Text, $Pattern, $options)) "Missing HomeDS402Ex evidence: $Description"
}

function Invoke-HomeDs402ExC78EvidenceCapture {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$LogPath,
        [Parameter(Mandatory = $true)][string]$DirectOpenPath,
        [Parameter(Mandatory = $true)][string]$NetworkPath,
        [Parameter(Mandatory = $true)][string]$EvidencePath,
        [Parameter(Mandatory = $true)][datetime]$BuildStartUtc,
        [switch]$SkipGitRequirements
    )

    Assert-HomeExEvidenceCondition ($BuildStartUtc -ne [datetime]::MinValue) 'BuildStartedUtc must be supplied explicitly.'

    $rootFull = [IO.Path]::GetFullPath($Root)
    $buildStart = $BuildStartUtc.ToUniversalTime()
    $logFull = [IO.Path]::GetFullPath($LogPath)
    $directOpenFull = [IO.Path]::GetFullPath($DirectOpenPath)
    $networkFull = [IO.Path]::GetFullPath($NetworkPath)
    $evidenceFull = [IO.Path]::GetFullPath($EvidencePath)

    $classesPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
    $projectPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'
    $networksPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Networks.lcb'

    $criticalRelativePaths = @(
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCEcatInputLatch\LMCEcatInputLatch.st',
        'tools\Verify-HomeDs402ExHomeex07Ownership.ps1',
        'tools\Verify-HomeDs402ExHomeex09Static.ps1',
        'tools\Verify-HomeDs402ExRetainedStore.ps1',
        'docs\api\design\HOME_DS402_EX_DESIGN.md'
    )
    $criticalPaths = $criticalRelativePaths | ForEach-Object { Join-Path $rootFull $_ }

    foreach ($required in @($classesPath, $projectPath, $networksPath, $logFull, $directOpenFull, $networkFull) + $criticalPaths) {
        Assert-HomeExEvidenceCondition (Test-Path -LiteralPath $required -PathType Leaf) "Required HomeDS402Ex evidence input is missing: $required"
    }

    $freshFiles = @(
        (Get-Item -LiteralPath $classesPath),
        (Get-Item -LiteralPath $projectPath),
        (Get-Item -LiteralPath $networksPath),
        (Get-Item -LiteralPath $logFull),
        (Get-Item -LiteralPath $directOpenFull),
        (Get-Item -LiteralPath $networkFull)
    )
    foreach ($fresh in $freshFiles) { Assert-HomeExFreshFile -File $fresh -BuildStart $buildStart }

    $logText = [IO.File]::ReadAllText($logFull)
    Assert-HomeExEvidenceCondition ([regex]::IsMatch($logText, '(?im)\bC78\b')) 'Build log does not contain C78 target evidence.'
    Assert-HomeExEvidenceCondition ([regex]::IsMatch($logText, '(?im)\bARM\b')) 'Build log does not contain ARM target evidence.'
    Assert-HomeExEvidenceCondition ([regex]::IsMatch($logText, '(?im)\b0\s+errors?\b')) 'Build log does not contain an explicit zero-error compiler result.'
    Assert-HomeExEvidenceCondition (-not [regex]::IsMatch($logText, '(?im)\b[1-9][0-9]*\s+errors?\b')) 'Build log contains a nonzero error count.'
    Assert-HomeExEvidenceCondition ([regex]::IsMatch($logText, '(?im)(?:linker[^\r\n]*\bdone\b|\blink(?:ing)?[^\r\n]*successful\b)')) 'Build log does not contain Linker Done or successful link evidence.'
    Assert-HomeExEvidenceCondition (-not [regex]::IsMatch($logText, '(?im)CInvalidArgException')) 'Build log contains CInvalidArgException evidence.'

    $directOpenText = [IO.File]::ReadAllText($directOpenFull)
    foreach ($method in @(
        'LMCDiagnosticsService::HandleAxisDs402HomeExStart',
        'LMCDiagnosticsService::HandleAxisDs402HomeExOutcome',
        'LMCDiagnosticsService::HandleAxisDs402HomeExRetire',
        'LMCDiagnosticsService::ProcessAxisDs402HomeEx'
    )) {
        Assert-HomeExPassLine -Text $directOpenText -Pattern ("(?im)^\s*PASS\s+direct-open\s+" + [regex]::Escape($method) + "\s*$") -Description "direct-open $method"
    }

    $networkText = [IO.File]::ReadAllText($networkFull)
    foreach ($component in @('TCPMotionInterface', 'LMCControlCommandService', 'LMCDiagnosticsService', 'LMCEcatInputLatch')) {
        Assert-HomeExPassLine -Text $networkText -Pattern ("(?im)^\s*PASS\s+network-smoke\s+" + [regex]::Escape($component) + "\s*$") -Description "Network smoke $component"
    }

    $gitHead = '<self-test-no-git>'
    $gitStatus = '<self-test-no-git>'
    if (-not $SkipGitRequirements) {
        $gitHead = Get-HomeExGitText -Root $rootFull -Arguments @('rev-parse', 'HEAD')
        Assert-HomeExEvidenceCondition (-not [string]::IsNullOrWhiteSpace($gitHead)) 'Unable to resolve repository HEAD.'
        $gitStatus = Get-HomeExGitText -Root $rootFull -Arguments @('status', '--porcelain=v1') -AllowFailure
        if ([string]::IsNullOrEmpty($gitStatus)) { $gitStatus = '<clean>' }
    }

    $artifactRows = New-Object Collections.Generic.List[string]
    foreach ($artifactPath in @($classesPath, $projectPath, $networksPath)) {
        $info = Get-Item -LiteralPath $artifactPath
        $relative = (Get-HomeExRelativeRepoPath -Root $rootFull -Path $artifactPath).Replace('\', '/')
        $headBlob = '<self-test-no-git>'
        $workingBlob = '<self-test-no-git>'
        if (-not $SkipGitRequirements) {
            $headBlob = Get-HomeExGitText -Root $rootFull -Arguments @('rev-parse', "HEAD:$relative") -AllowFailure
            if ([string]::IsNullOrWhiteSpace($headBlob)) { $headBlob = '<untracked-at-head>' }
            $workingBlob = Get-HomeExGitText -Root $rootFull -Arguments @('hash-object', '--', $artifactPath)
        }
        $artifactRows.Add("| ``$(Get-HomeExRelativeRepoPath -Root $rootFull -Path $artifactPath)`` | $($info.Length) | ``$($info.LastWriteTimeUtc.ToString('o'))`` | ``$(Get-HomeExSha256Hex -Path $artifactPath)`` | ``$headBlob`` | ``$workingBlob`` |")
    }

    $sourceRows = New-Object Collections.Generic.List[string]
    foreach ($sourcePath in $criticalPaths) {
        $sourceRows.Add("| ``$(Get-HomeExRelativeRepoPath -Root $rootFull -Path $sourcePath)`` | $((Get-Item -LiteralPath $sourcePath).Length) | ``$(Get-HomeExSha256Hex -Path $sourcePath)`` |")
    }

    $logInfo = Get-Item -LiteralPath $logFull
    $directOpenInfo = Get-Item -LiteralPath $directOpenFull
    $networkInfo = Get-Item -LiteralPath $networkFull
    $capturedUtc = [datetime]::UtcNow
    $markdown = @"
# HomeDS402Ex HOMEEX-09 Fresh C78 Evidence Capture

- CapturedUtc: ``$($capturedUtc.ToString('o'))``
- BuildStartedUtc: ``$($buildStart.ToString('o'))``
- RepositoryHead: ``$gitHead``
- Target evidence: ``C78 / ARM``
- Compiler evidence: ``0 errors``
- Link evidence: ``PASS pattern found``
- CInvalidArgException: ``0 observed``
- HomeExMethodDirectOpen: **PASS**
- NetworkSmoke: **PASS**
- GateResult: **CAPTURED_FOR_REVIEW**
- ArtifactRatchetDecision: **REVIEW_REQUIRED**
- SourceOnlyPhysicalIdentityGate: **REVIEW_REQUIRED**
- HOMEEX09Completion: **NOT_YET_COMPLETE**
- HOMEEX08Runtime: **KEEP_OFF**
- AdminCapabilityBit11: **KEEP_OFF**

This receipt proves only that the supplied fresh C78/ARM build, HomeDS402Ex direct-open evidence,
Network smoke evidence and generated artifacts form one post-build capture set. It does not approve a
changed ``Classes.lcb`` identity, does not approve HOMEEX-01/02 hardware profile values, and does not
open HOMEEX-08 physical homing execution. The physical artifact ratchet must be reviewed separately and
full repository SourceOnly must subsequently pass on the same source tree.

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
| ``$(Get-HomeExRelativeRepoPath -Root $rootFull -Path $logFull)`` | $($logInfo.Length) | ``$(Get-HomeExSha256Hex -Path $logFull)`` |
| ``$(Get-HomeExRelativeRepoPath -Root $rootFull -Path $directOpenFull)`` | $($directOpenInfo.Length) | ``$(Get-HomeExSha256Hex -Path $directOpenFull)`` |
| ``$(Get-HomeExRelativeRepoPath -Root $rootFull -Path $networkFull)`` | $($networkInfo.Length) | ``$(Get-HomeExSha256Hex -Path $networkFull)`` |

## Working tree at capture

    $($gitStatus.Replace("`n", "`n    "))

## Mandatory next review

1. Confirm generated HomeDS402Ex declarations and method bodies direct-open from this exact source tree.
2. Confirm ``Classes.lcb``, project ``.lcb`` and ``Network/Networks.lcb`` were produced by the same fresh C78/ARM rebuild represented by this receipt.
3. Review old/new physical artifact identities; never ratchet a hash from change alone.
4. After explicit artifact review, update only the justified physical identity ratchet and rerun full repository SourceOnly.
5. Keep ``LMC_DIAG_DS402_HOME_EX_ENABLED FALSE`` and Admin bit 11 OFF.
6. Issue #28 must still provide approved axis wiring/method/scale/MapRevision before HOMEEX-08 physical runtime can open.
7. HOMEEX-10/11 hardware matrices remain separate after artifact and profile approval.
"@

    $evidenceDirectory = Split-Path -Parent $evidenceFull
    if (-not (Test-Path -LiteralPath $evidenceDirectory)) { New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null }
    [IO.File]::WriteAllText($evidenceFull, $markdown.Replace("`r`n", "`n"), (New-Object Text.UTF8Encoding($false)))

    Write-Host 'PASS HomeDS402Ex fresh C78/ARM build evidence'
    Write-Host 'PASS HomeDS402Ex method direct-open evidence'
    Write-Host 'PASS HomeDS402Ex Network smoke evidence'
    Write-Host 'PASS HomeDS402Ex generated artifact freshness and identity capture'
    Write-Host "PASS evidence captured: $evidenceFull"
    Write-Host 'REVIEW_REQUIRED physical artifact ratchet and SourceOnly closure'
    Write-Host 'KEEP_OFF HOMEEX-08 runtime and Admin capability bit 11'
    return $evidenceFull
}

function Invoke-HomeExExpectedFailure {
    param([Parameter(Mandatory = $true)][scriptblock]$Action, [Parameter(Mandatory = $true)][string]$Label)
    $failed = $false
    try { & $Action } catch { $failed = $true; Write-Host "PASS expected failure: $Label" }
    Assert-HomeExEvidenceCondition $failed "Expected fail-closed self-test did not fail: $Label"
}

function Invoke-HomeExSelfTest {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('HomeDs402ExC78-' + [guid]::NewGuid().ToString('N'))
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
            'tools\Verify-HomeDs402ExHomeex07Ownership.ps1',
            'tools\Verify-HomeDs402ExHomeex09Static.ps1',
            'tools\Verify-HomeDs402ExRetainedStore.ps1',
            'docs\api\design\HOME_DS402_EX_DESIGN.md'
        )
        foreach ($relative in $critical) { [IO.File]::WriteAllText((Join-Path $root $relative), "self-test $relative") }

        $classes = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
        $project = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'
        $networks = Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Networks.lcb'
        [IO.File]::WriteAllBytes($classes, [byte[]](1,2,3,4,5))
        [IO.File]::WriteAllBytes($project, [byte[]](6,7,8,9))
        [IO.File]::WriteAllBytes($networks, [byte[]](10,11,12))

        $buildStart = [datetime]::UtcNow.AddSeconds(-2)
        $buildLog = Join-Path $root 'evidence\c78-build.log'
        [IO.File]::WriteAllText($buildLog, "Target C78 ARM`r`nCompiler: 0 errors / 12 warnings`r`nLinker: Done`r`n")
        $directOpen = Join-Path $root 'evidence\homeex-direct-open.log'
        [IO.File]::WriteAllText($directOpen, @"
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeExStart
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeExOutcome
PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeExRetire
PASS direct-open LMCDiagnosticsService::ProcessAxisDs402HomeEx
"@)
        $networkSmoke = Join-Path $root 'evidence\network-smoke.log'
        [IO.File]::WriteAllText($networkSmoke, @"
PASS network-smoke TCPMotionInterface
PASS network-smoke LMCControlCommandService
PASS network-smoke LMCDiagnosticsService
PASS network-smoke LMCEcatInputLatch
"@)
        $output = Join-Path $root 'evidence\homeex-c78-evidence.md'

        $captured = Invoke-HomeDs402ExC78EvidenceCapture -Root $root -LogPath $buildLog -DirectOpenPath $directOpen -NetworkPath $networkSmoke -EvidencePath $output -BuildStartUtc $buildStart -SkipGitRequirements
        Assert-HomeExEvidenceCondition (Test-Path -LiteralPath $captured) 'HomeDS402Ex C78 self-test did not create an evidence file.'
        $receipt = [IO.File]::ReadAllText($captured)
        Assert-HomeExEvidenceCondition ($receipt.Contains('ArtifactRatchetDecision: **REVIEW_REQUIRED**')) 'Self-test did not preserve artifact review.'
        Assert-HomeExEvidenceCondition ($receipt.Contains('HOMEEX09Completion: **NOT_YET_COMPLETE**')) 'Self-test over-claimed HOMEEX-09 completion.'
        Assert-HomeExEvidenceCondition ($receipt.Contains('HOMEEX08Runtime: **KEEP_OFF**')) 'Self-test did not preserve HOMEEX-08 runtime OFF.'
        Assert-HomeExEvidenceCondition ($receipt.Contains('AdminCapabilityBit11: **KEEP_OFF**')) 'Self-test did not preserve bit11 OFF.'

        [IO.File]::SetLastWriteTimeUtc($classes, $buildStart.AddSeconds(-5))
        Invoke-HomeExExpectedFailure -Label 'stale generated artifact rejected' -Action {
            Invoke-HomeDs402ExC78EvidenceCapture -Root $root -LogPath $buildLog -DirectOpenPath $directOpen -NetworkPath $networkSmoke -EvidencePath $output -BuildStartUtc $buildStart -SkipGitRequirements | Out-Null
        }
        [IO.File]::SetLastWriteTimeUtc($classes, [datetime]::UtcNow)

        [IO.File]::WriteAllText($directOpen, "PASS direct-open LMCDiagnosticsService::HandleAxisDs402HomeExStart`n")
        Invoke-HomeExExpectedFailure -Label 'incomplete HomeEx direct-open evidence rejected' -Action {
            Invoke-HomeDs402ExC78EvidenceCapture -Root $root -LogPath $buildLog -DirectOpenPath $directOpen -NetworkPath $networkSmoke -EvidencePath $output -BuildStartUtc $buildStart -SkipGitRequirements | Out-Null
        }

        Write-Host 'PASS Capture-HomeDs402ExC78Evidence self-test'
    }
    finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

if ($SelfTest) {
    Invoke-HomeExSelfTest
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = (Get-Location).Path }
foreach ($requiredArgument in @(
    @{ Name = 'BuildLogPath'; Value = $BuildLogPath },
    @{ Name = 'MethodDirectOpenEvidencePath'; Value = $MethodDirectOpenEvidencePath },
    @{ Name = 'NetworkSmokeEvidencePath'; Value = $NetworkSmokeEvidencePath },
    @{ Name = 'OutputPath'; Value = $OutputPath })) {
    if ([string]::IsNullOrWhiteSpace($requiredArgument.Value)) { throw "$($requiredArgument.Name) is required unless -SelfTest is used." }
}

Invoke-HomeDs402ExC78EvidenceCapture `
    -Root $RepositoryRoot `
    -LogPath $BuildLogPath `
    -DirectOpenPath $MethodDirectOpenEvidencePath `
    -NetworkPath $NetworkSmokeEvidencePath `
    -EvidencePath $OutputPath `
    -BuildStartUtc $BuildStartedUtc | Out-Null

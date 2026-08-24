[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$BuildLogPath,
    [string]$OutputPath,
    [datetime]$BuildStartedUtc,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $output = & git -C $Root @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0 -and -not $AllowFailure) {
        throw "git $($Arguments -join ' ') failed with exit code ${exitCode}: $($output -join [Environment]::NewLine)"
    }

    if ($exitCode -ne 0) {
        return $null
    }

    return (($output | ForEach-Object { $_.ToString() }) -join "`n").Trim()
}

function Get-RelativeRepoPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $rootWithSeparator = $Root.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = [System.Uri]$rootWithSeparator
    $pathUri = [System.Uri]$Path
    return [System.Uri]::UnescapeDataString(
        $rootUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Assert-EvidenceCondition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-SetOperationModeC78EvidenceCapture {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$LogPath,
        [Parameter(Mandatory = $true)][string]$EvidencePath,
        [Parameter(Mandatory = $true)][datetime]$BuildStartUtc,
        [switch]$SkipGitRequirements
    )

    Assert-EvidenceCondition ($BuildStartUtc -ne [datetime]::MinValue) 'BuildStartedUtc must be supplied explicitly.'

    $rootFull = [System.IO.Path]::GetFullPath($Root)
    $logFull = [System.IO.Path]::GetFullPath($LogPath)
    $evidenceFull = [System.IO.Path]::GetFullPath($EvidencePath)
    $buildStart = $BuildStartUtc.ToUniversalTime()

    $classesPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
    $projectLcbPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'
    $criticalPaths = @(
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
        'tools\Verify-SetOperationModeStatic.ps1',
        'docs\api\design\SET_OPERATION_MODE_DESIGN.md'
    ) | ForEach-Object { Join-Path $rootFull $_ }

    foreach ($required in @($classesPath, $projectLcbPath, $logFull) + $criticalPaths) {
        Assert-EvidenceCondition (Test-Path -LiteralPath $required -PathType Leaf) "Required evidence input is missing: $required"
    }

    $classesInfo = Get-Item -LiteralPath $classesPath
    $projectInfo = Get-Item -LiteralPath $projectLcbPath
    $logInfo = Get-Item -LiteralPath $logFull
    foreach ($artifact in @($classesInfo, $projectInfo, $logInfo)) {
        Assert-EvidenceCondition ($artifact.LastWriteTimeUtc -ge $buildStart) (
            "Freshness check failed: $($artifact.FullName) last write $($artifact.LastWriteTimeUtc.ToString('o')) precedes build start $($buildStart.ToString('o')).")
    }

    $logText = [System.IO.File]::ReadAllText($logFull)
    Assert-EvidenceCondition ([regex]::IsMatch($logText, '(?im)\bC78\b')) 'Build log does not contain C78 target evidence.'
    Assert-EvidenceCondition ([regex]::IsMatch($logText, '(?im)\bARM\b')) 'Build log does not contain ARM target evidence.'
    Assert-EvidenceCondition ([regex]::IsMatch($logText, '(?im)\b0\s+errors?\b')) 'Build log does not contain an explicit zero-error compiler result.'
    Assert-EvidenceCondition (-not [regex]::IsMatch($logText, '(?im)\b[1-9][0-9]*\s+errors?\b')) 'Build log contains a nonzero error count.'
    Assert-EvidenceCondition ([regex]::IsMatch($logText, '(?im)(?:linker[^\r\n]*\bdone\b|\blink(?:ing)?[^\r\n]*successful\b)')) 'Build log does not contain Linker Done or successful link evidence.'

    $gitHead = '<self-test-no-git>'
    $gitStatus = '<self-test-no-git>'
    $headClassesBlob = '<self-test-no-git>'
    $workingClassesBlob = '<self-test-no-git>'
    $headProjectBlob = '<self-test-no-git>'
    $workingProjectBlob = '<self-test-no-git>'
    if (-not $SkipGitRequirements) {
        $gitHead = Get-GitText -Root $rootFull -Arguments @('rev-parse', 'HEAD')
        Assert-EvidenceCondition (-not [string]::IsNullOrWhiteSpace($gitHead)) 'Unable to resolve repository HEAD.'
        $gitStatus = Get-GitText -Root $rootFull -Arguments @('status', '--porcelain=v1') -AllowFailure
        if ([string]::IsNullOrEmpty($gitStatus)) {
            $gitStatus = '<clean>'
        }

        $classesRelative = (Get-RelativeRepoPath -Root $rootFull -Path $classesPath).Replace('\', '/')
        $projectRelative = (Get-RelativeRepoPath -Root $rootFull -Path $projectLcbPath).Replace('\', '/')
        $headClassesBlob = Get-GitText -Root $rootFull -Arguments @('rev-parse', "HEAD:$classesRelative") -AllowFailure
        if ([string]::IsNullOrWhiteSpace($headClassesBlob)) {
            $headClassesBlob = '<untracked-at-head>'
        }
        $headProjectBlob = Get-GitText -Root $rootFull -Arguments @('rev-parse', "HEAD:$projectRelative") -AllowFailure
        if ([string]::IsNullOrWhiteSpace($headProjectBlob)) {
            $headProjectBlob = '<untracked-at-head>'
        }
        $workingClassesBlob = Get-GitText -Root $rootFull -Arguments @('hash-object', '--', $classesPath)
        $workingProjectBlob = Get-GitText -Root $rootFull -Arguments @('hash-object', '--', $projectLcbPath)
    }

    $sourceRows = New-Object System.Collections.Generic.List[string]
    foreach ($sourcePath in $criticalPaths) {
        $sourceRows.Add(
            "| ``$(Get-RelativeRepoPath -Root $rootFull -Path $sourcePath)`` | ``$(Get-Sha256Hex -Path $sourcePath)`` | $((Get-Item -LiteralPath $sourcePath).Length) |")
    }

    $capturedUtc = [datetime]::UtcNow
    $classesSha = Get-Sha256Hex -Path $classesPath
    $projectSha = Get-Sha256Hex -Path $projectLcbPath
    $logSha = Get-Sha256Hex -Path $logFull

    $markdown = @"
# SetOperationMode Fresh C78 Artifact Capture

- CapturedUtc: ``$($capturedUtc.ToString('o'))``
- BuildStartedUtc: ``$($buildStart.ToString('o'))``
- RepositoryHead: ``$gitHead``
- Target evidence: ``C78 / ARM``
- Compiler evidence: ``0 errors``
- Link evidence: ``PASS pattern found``
- GateResult: **CAPTURED_FOR_REVIEW**
- ArtifactRatchetDecision: **REVIEW_REQUIRED**
- CapabilityActivation: **KEEP_OFF**

This file is evidence capture, not automatic IDE/artifact approval. Do not update the physical artifact
ratchet or enable SetOperationMode capability bits 8/9/10 until generated ABI review, same-image PLC
identity, MODE-11 packet evidence, and MODE-12 hardware/recovery evidence are complete.

## Artifact identity

| Artifact | Bytes | LastWriteUtc | SHA-256 | HEAD blob | Working blob |
|---|---:|---|---|---|---|
| ``Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb`` | $($classesInfo.Length) | ``$($classesInfo.LastWriteTimeUtc.ToString('o'))`` | ``$classesSha`` | ``$headClassesBlob`` | ``$workingClassesBlob`` |
| ``Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb`` | $($projectInfo.Length) | ``$($projectInfo.LastWriteTimeUtc.ToString('o'))`` | ``$projectSha`` | ``$headProjectBlob`` | ``$workingProjectBlob`` |
| ``$(Get-RelativeRepoPath -Root $rootFull -Path $logFull)`` | $($logInfo.Length) | ``$($logInfo.LastWriteTimeUtc.ToString('o'))`` | ``$logSha`` | n/a | n/a |

## Critical source identity

| Source | SHA-256 | Bytes |
|---|---|---:|
$($sourceRows -join "`n")

## Working tree at capture

    $($gitStatus.Replace("`n", "`n    "))

## Mandatory next review

1. Compare generated declarations/ABI against tracked source expectations.
2. Confirm the new ``Classes.lcb`` is from the same fresh C78/ARM build represented by the supplied log.
3. Record same-image DiagnosticsBuild, DiagnosticsBootId, and MapRevision after PLC download/load.
4. Do not approve the artifact identity ratchet from hash change alone.
5. Run MODE-11 same-mode/no-write and exact one-write/readback packet qualification.
6. Run MODE-12 timeout/disconnect/mismatch/quarantine/retire matrix starting with axis 1.
"@

    $evidenceDirectory = Split-Path -Parent $evidenceFull
    if (-not (Test-Path -LiteralPath $evidenceDirectory)) {
        New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null
    }
    [System.IO.File]::WriteAllText(
        $evidenceFull,
        $markdown.Replace("`r`n", "`n"),
        (New-Object System.Text.UTF8Encoding($false)))

    Write-Host "PASS fresh C78 evidence inputs exist"
    Write-Host "PASS artifact/log freshness >= BuildStartedUtc"
    Write-Host "PASS build log C78/ARM + zero-error + link evidence"
    Write-Host "PASS evidence captured: $evidenceFull"
    Write-Host "REVIEW_REQUIRED artifact ratchet and activation remain closed"
    return $evidenceFull
}

function Invoke-SelfTest {
    $root = Join-Path ([System.IO.Path]::GetTempPath()) ('ElmoC78EvidenceSelfTest-' + [guid]::NewGuid().ToString('N'))
    try {
        $paths = @(
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface',
            'tools',
            'docs\api\design'
        )
        foreach ($relative in $paths) {
            New-Item -ItemType Directory -Path (Join-Path $root $relative) -Force | Out-Null
        }

        $buildStart = [datetime]::UtcNow.AddSeconds(-2)
        [System.IO.File]::WriteAllBytes(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'),
            [byte[]](1,2,3,4))
        [System.IO.File]::WriteAllBytes(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'),
            [byte[]](5,6,7,8))
        foreach ($relative in @(
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
            'tools\Verify-SetOperationModeStatic.ps1',
            'docs\api\design\SET_OPERATION_MODE_DESIGN.md')) {
            [System.IO.File]::WriteAllText((Join-Path $root $relative), "self-test $relative")
        }

        $log = Join-Path $root 'fresh-build.log'
        [System.IO.File]::WriteAllText(
            $log,
            "Target C78 ARM`r`nCompiler: 0 errors / 79 warnings`r`nLinker: Done`r`n")
        $output = Join-Path $root 'evidence.md'
        $captured = Invoke-SetOperationModeC78EvidenceCapture `
            -Root $root `
            -LogPath $log `
            -EvidencePath $output `
            -BuildStartUtc $buildStart `
            -SkipGitRequirements
        Assert-EvidenceCondition (Test-Path -LiteralPath $captured) 'Self-test evidence file was not created.'
        $text = [System.IO.File]::ReadAllText($captured)
        Assert-EvidenceCondition ($text.Contains('ArtifactRatchetDecision: **REVIEW_REQUIRED**')) 'Self-test did not preserve manual ratchet review.'
        Assert-EvidenceCondition ($text.Contains('CapabilityActivation: **KEEP_OFF**')) 'Self-test did not preserve activation OFF.'
        Write-Host 'PASS Capture-SetOperationModeC78Evidence self-test'
    }
    finally {
        if (Test-Path -LiteralPath $root) {
            Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($SelfTest) {
    Invoke-SelfTest
    exit 0
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
}
if ([string]::IsNullOrWhiteSpace($BuildLogPath)) {
    throw 'BuildLogPath is required. Supply a fresh LASAL IDE C78/ARM build log captured for this rebuild.'
}
if ($BuildStartedUtc -eq [datetime]::MinValue) {
    throw 'BuildStartedUtc is required. Record the UTC start time immediately before the fresh LASAL IDE rebuild.'
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $stamp = [datetime]::UtcNow.ToString('yyyyMMdd-HHmmss')
    $OutputPath = Join-Path $RepositoryRoot "docs\api\design\evidence\SET_OPERATION_MODE_C78_CAPTURE_$stamp.md"
}

Invoke-SetOperationModeC78EvidenceCapture `
    -Root $RepositoryRoot `
    -LogPath $BuildLogPath `
    -EvidencePath $OutputPath `
    -BuildStartUtc $BuildStartedUtc | Out-Null

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

function Assert-EvidenceCondition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

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
    return [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Assert-SetOperationModeQualificationSource {
    param([Parameter(Mandatory = $true)][string]$Root)

    $diagnosticsPath = Join-Path $Root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
    $controlPath = Join-Path $Root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'
    $networkPath = Join-Path $Root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn'
    $generatedNetworkPath = Join-Path $Root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st'

    foreach ($path in @($diagnosticsPath, $controlPath, $networkPath, $generatedNetworkPath)) {
        Assert-EvidenceCondition (Test-Path -LiteralPath $path -PathType Leaf) "Required SetOperationMode qualification source is missing: $path"
    }

    $diagnostics = [System.IO.File]::ReadAllText($diagnosticsPath)
    $control = [System.IO.File]::ReadAllText($controlPath)
    $network = [System.IO.File]::ReadAllText($networkPath)
    $generatedNetwork = [System.IO.File]::ReadAllText($generatedNetworkPath)

    Assert-EvidenceCondition ([regex]::IsMatch(
        $diagnostics,
        '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_ENABLED[\t ]+TRUE[\t ]*$')) `
        'Current source does not have LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE.'
    Assert-EvidenceCondition ([regex]::IsMatch(
        $diagnostics,
        '(?m)^#define[\t ]+LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES[\t ]+TRUE[\t ]*$')) `
        'Current source does not have LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE.'
    Assert-EvidenceCondition ($control.Contains('(pResponseFrame + 24)^$UDINT := 0x00000717;')) `
        'Admin capability mask does not advertise the SetOperationMode triad (0x00000717).'
    Assert-EvidenceCondition ($control.Contains('(pResponseFrame + 46)^$UINT := 0x018A;')) `
        'Admin supported-mode mask is not PP/PV/IP/CSP (0x018A).'
    Assert-EvidenceCondition ($network.Contains(
        '<Connection Source="LMCDiagnosticsService1.AxisOwnership" Destination="LMCControlCommandService1.ClassSvr"')) `
        'Comm_Network.lcn does not connect LMCDiagnosticsService1.AxisOwnership to LMCControlCommandService1.ClassSvr.'
    Assert-EvidenceCondition ([regex]::IsMatch(
        $generatedNetwork,
        'TO_UDINT\(6\),[\t ]*"AxisOwnership",[\t ]*TO_UDINT\(5\),[\t ]*"ClassSvr"')) `
        'Generated ONE_Comm_Network_Table.st does not contain the AxisOwnership connection.'

    Write-Host 'PASS qualification gate is ON in current source'
    Write-Host 'PASS PP/PV/IP/CSP software-mode mask is active'
    Write-Host 'PASS AxisOwnership is present in Comm_Network.lcn'
    Write-Host 'PASS AxisOwnership is present in generated ONE_Comm_Network_Table.st'
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

    Assert-SetOperationModeQualificationSource -Root $rootFull

    $classesPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'
    $projectLcbPath = Join-Path $rootFull 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'
    $criticalRelativePaths = @(
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn',
        'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st',
        'tools\Verify-SetOperationModeStatic.ps1',
        'docs\api\design\SET_OPERATION_MODE_DESIGN.md'
    )
    $criticalPaths = $criticalRelativePaths | ForEach-Object { Join-Path $rootFull $_ }

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
    if (-not $SkipGitRequirements) {
        $gitHead = Get-GitText -Root $rootFull -Arguments @('rev-parse', 'HEAD')
        Assert-EvidenceCondition (-not [string]::IsNullOrWhiteSpace($gitHead)) 'Unable to resolve repository HEAD.'
        $gitStatus = Get-GitText -Root $rootFull -Arguments @('status', '--porcelain=v1') -AllowFailure
        if ([string]::IsNullOrEmpty($gitStatus)) {
            $gitStatus = '<clean>'
        }
    }

    $sourceRows = New-Object System.Collections.Generic.List[string]
    foreach ($sourcePath in $criticalPaths) {
        $sourceRows.Add("| ``$(Get-RelativeRepoPath -Root $rootFull -Path $sourcePath)`` | ``$(Get-Sha256Hex -Path $sourcePath)`` | $((Get-Item -LiteralPath $sourcePath).Length) |")
    }

    $capturedUtc = [datetime]::UtcNow
    $classesSha = Get-Sha256Hex -Path $classesPath
    $projectSha = Get-Sha256Hex -Path $projectLcbPath
    $logSha = Get-Sha256Hex -Path $logFull

    $markdown = @"
# SetOperationMode Fresh C78 Qualification Artifact Capture

- CapturedUtc: ``$($capturedUtc.ToString('o'))``
- BuildStartedUtc: ``$($buildStart.ToString('o'))``
- RepositoryHead: ``$gitHead``
- Target evidence: ``C78 / ARM``
- Compiler evidence: ``0 errors``
- Link evidence: ``PASS pattern found``
- QualificationActivation: **ON_EXPECTED**
- AxisOwnershipSourceWiring: **PASS**
- AxisOwnershipGeneratedWiring: **PASS**
- ProductionRelease: **NO-GO**
- RuntimeLoadedImageIdentity: **NOT_PROVEN_BY_THIS_CAPTURE**

This capture proves that the source tree used for the fresh C78/ARM build has the SetOperationMode
qualification gate enabled and contains the required AxisOwnership wiring in both the network source and
generated network table. It does not prove that the exact captured artifact was downloaded to the PLC.
After PLC download, record DiagnosticsBuild, DiagnosticsBootId, MapRevision and the runtime SetOperationMode
result as one evidence set. Production release remains NO-GO until the physical matrix is complete.

## Artifact identity

| Artifact | Bytes | LastWriteUtc | SHA-256 |
|---|---:|---|---|
| ``Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb`` | $($classesInfo.Length) | ``$($classesInfo.LastWriteTimeUtc.ToString('o'))`` | ``$classesSha`` |
| ``Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb`` | $($projectInfo.Length) | ``$($projectInfo.LastWriteTimeUtc.ToString('o'))`` | ``$projectSha`` |
| ``$(Get-RelativeRepoPath -Root $rootFull -Path $logFull)`` | $($logInfo.Length) | ``$($logInfo.LastWriteTimeUtc.ToString('o'))`` | ``$logSha`` |

## Critical source identity

| Source | SHA-256 | Bytes |
|---|---|---:|
$($sourceRows -join "`n")

## Working tree at capture

    $($gitStatus.Replace("`n", "`n    "))

## Mandatory physical follow-up

1. Download/load the exact fresh C78/ARM artifact represented above to the PLC.
2. Record same-image DiagnosticsBuild, DiagnosticsBootId and MapRevision after load.
3. If Start returns SetOperationModeOutcomeStorageUnavailable(49), do not infer a 0x6060 write; the PLC rejected before mutation.
4. For a fresh qualification-active image, investigate runtime ``LMCDiagnosticsService1.AxisOwnership`` connectivity/ownership admission before changing any safety fence.
5. Run CSP->CSP no-write and CSP->PP/PV/IP exact one-write/readback qualification.
6. Keep production release NO-GO until failure/recovery and Axis2..4 matrices are complete.
"@

    $evidenceDirectory = Split-Path -Parent $evidenceFull
    if (-not (Test-Path -LiteralPath $evidenceDirectory)) {
        New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null
    }
    [System.IO.File]::WriteAllText(
        $evidenceFull,
        $markdown.Replace("`r`n", "`n"),
        (New-Object System.Text.UTF8Encoding($false)))

    Write-Host 'PASS current SetOperationMode qualification source contract'
    Write-Host 'PASS artifact/log freshness >= BuildStartedUtc'
    Write-Host 'PASS build log C78/ARM + zero-error + link evidence'
    Write-Host "PASS evidence captured: $evidenceFull"
    Write-Host 'NOTE exact PLC loaded-image identity still requires post-download runtime evidence'
    Write-Host 'NO-GO production release remains closed'
    return $evidenceFull
}

function Invoke-SelfTest {
    $root = Join-Path ([System.IO.Path]::GetTempPath()) ('ElmoSetOperationModeC78EvidenceSelfTest-' + [guid]::NewGuid().ToString('N'))
    try {
        foreach ($relative in @(
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface',
            'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network',
            'tools',
            'docs\api\design')) {
            New-Item -ItemType Directory -Path (Join-Path $root $relative) -Force | Out-Null
        }

        $buildStart = [datetime]::UtcNow.AddSeconds(-2)
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'),
            "#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE`n#define LMC_DIAG_SET_OPERATION_MODE_SOFTWARE_MODES TRUE`n")
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\LMCControlCommandService\LMCControlCommandService.st'),
            "(pResponseFrame + 24)^`$UDINT := 0x00000717;`n(pResponseFrame + 46)^`$UINT := 0x018A;`n")
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\TCPMotionInterface\TCPMotionInterface.st'),
            'self-test tcp')
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\Comm_Network.lcn'),
            '<Connection Source="LMCDiagnosticsService1.AxisOwnership" Destination="LMCControlCommandService1.ClassSvr"/>')
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network\Comm_Network\ONE_Comm_Network_Table.st'),
            'TO_UDINT(6), "AxisOwnership", TO_UDINT(5), "ClassSvr",')
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'tools\Verify-SetOperationModeStatic.ps1'),
            'self-test static verifier')
        [System.IO.File]::WriteAllText(
            (Join-Path $root 'docs\api\design\SET_OPERATION_MODE_DESIGN.md'),
            'self-test design')
        [System.IO.File]::WriteAllBytes(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb'),
            [byte[]](1,2,3,4))
        [System.IO.File]::WriteAllBytes(
            (Join-Path $root 'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Elmo_EtherCAT_Test_4Axis.lcb'),
            [byte[]](5,6,7,8))

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
        Assert-EvidenceCondition ($text.Contains('QualificationActivation: **ON_EXPECTED**')) 'Self-test did not record qualification-active posture.'
        Assert-EvidenceCondition ($text.Contains('AxisOwnershipGeneratedWiring: **PASS**')) 'Self-test did not record generated AxisOwnership wiring.'
        Assert-EvidenceCondition ($text.Contains('ProductionRelease: **NO-GO**')) 'Self-test did not preserve production NO-GO.'
        Assert-EvidenceCondition (-not $text.Contains('CapabilityActivation: **KEEP_OFF**')) 'Legacy activation-OFF wording is still present.'
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

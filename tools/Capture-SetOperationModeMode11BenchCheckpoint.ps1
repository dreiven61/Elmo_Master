[CmdletBinding()]
param(
    [string]$RepositoryRoot = '',
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [string]$BuildLogPath = '',
    [datetime]$BuildStartedUtc = [datetime]::MinValue,
    [uint32]$DiagnosticsBuild = 0,
    [uint32]$DiagnosticsBootId = 0,
    [uint32]$MapRevision = 0,
    [string]$Endpoint = '',
    [string]$PlcLoadTimestamp = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ExpectedBranch = 'codex/setopmode-mode11-bench-activation'
$DiagnosticsRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
$ControlRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st'
$ClassesRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
$ProjectRelative = 'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcb'

function Assert-Condition {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )

    $output = & git -C $Root @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code ${exitCode}: $($output -join [Environment]::NewLine)"
    }
    return (($output | ForEach-Object { $_.ToString() }) -join "`n").TrimEnd()
}

function Get-FileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}

function Get-TextSha256 {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)

    $utf8 = New-Object System.Text.UTF8Encoding($false)
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
            $algorithm.ComputeHash($utf8.GetBytes($Text)))).Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Join-Path $PSScriptRoot '..'
}
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$outputFull = [IO.Path]::GetFullPath($OutputPath)

$diagnosticsPath = Join-Path $root $DiagnosticsRelative
$controlPath = Join-Path $root $ControlRelative
$classesPath = Join-Path $root $ClassesRelative
$projectPath = Join-Path $root $ProjectRelative
$candidateVerifier = Join-Path $root 'tools/Verify-SetOperationModeMode11Candidate.ps1'
$defineVerifier = Join-Path $root 'tools/Verify-SetOperationModeDefineOrder.ps1'
$prepareScript = Join-Path $root 'tools/Prepare-SetOperationModeMode11Bench.ps1'
$c78Capture = Join-Path $root 'tools/Capture-SetOperationModeC78Evidence.ps1'

foreach ($required in @(
    $diagnosticsPath,
    $controlPath,
    $classesPath,
    $projectPath,
    $candidateVerifier,
    $defineVerifier,
    $prepareScript,
    $c78Capture)) {
    Assert-Condition (Test-Path -LiteralPath $required -PathType Leaf) "Required MODE-11 checkpoint input is missing: $required"
}

$branch = Get-GitText -Root $root -Arguments @('rev-parse', '--abbrev-ref', 'HEAD')
Assert-Condition ($branch -ceq $ExpectedBranch) "MODE-11 checkpoint capture is allowed only on branch '$ExpectedBranch'. Current='$branch'."

& $prepareScript -Verify
Assert-Condition ($LASTEXITCODE -eq 0) 'MODE-11 bench source state verification failed.'
& $candidateVerifier
Assert-Condition ($LASTEXITCODE -eq 0) 'MODE-11 candidate safety-contract verifier failed.'
& $defineVerifier
Assert-Condition ($LASTEXITCODE -eq 0) 'SetOperationMode define-order verifier failed.'

$diagnosticsText = [IO.File]::ReadAllText($diagnosticsPath)
$controlText = [IO.File]::ReadAllText($controlPath)
Assert-Condition ($diagnosticsText.Contains('#define LMC_DIAG_SET_OPERATION_MODE_ENABLED TRUE')) 'Checkpoint requires BENCH_ACTIVE Diagnostics TRUE.'
Assert-Condition (-not $diagnosticsText.Contains('#define LMC_DIAG_SET_OPERATION_MODE_ENABLED FALSE')) 'Checkpoint refuses mixed/off Diagnostics activation state.'
Assert-Condition ($controlText.Contains('(pResponseFrame + 24)^$UDINT := 0x00000717;')) 'Checkpoint requires BENCH_ACTIVE Admin mask 0x00000717.'
Assert-Condition (-not $controlText.Contains('(pResponseFrame + 24)^$UDINT := 0x00000017;')) 'Checkpoint refuses mixed/off Admin capability state.'

$identityAny = ($DiagnosticsBuild -ne 0) -or ($DiagnosticsBootId -ne 0) -or ($MapRevision -ne 0)
if ($identityAny) {
    Assert-Condition (($DiagnosticsBuild -ne 0) -and ($DiagnosticsBootId -ne 0) -and ($MapRevision -ne 0)) 'DiagnosticsBuild, DiagnosticsBootId and MapRevision must be supplied together and all be nonzero.'
}

$hasBuildLog = -not [string]::IsNullOrWhiteSpace($BuildLogPath)
if ($hasBuildLog) {
    Assert-Condition ($BuildStartedUtc -ne [datetime]::MinValue) 'BuildStartedUtc is required when BuildLogPath is supplied.'
}
elseif ($BuildStartedUtc -ne [datetime]::MinValue) {
    throw 'BuildLogPath is required when BuildStartedUtc is supplied.'
}

$head = Get-GitText -Root $root -Arguments @('rev-parse', 'HEAD')
$status = Get-GitText -Root $root -Arguments @('status', '--porcelain=v1')
if ([string]::IsNullOrWhiteSpace($status)) {
    $status = '<clean>'
}
$activeDiff = Get-GitText -Root $root -Arguments @('diff', '--binary', '--', $DiagnosticsRelative, $ControlRelative)
Assert-Condition (-not [string]::IsNullOrWhiteSpace($activeDiff)) 'BENCH_ACTIVE checkpoint requires a visible activation diff against HEAD.'
$activeDiffSha = Get-TextSha256 -Text $activeDiff

$classes = Get-Item -LiteralPath $classesPath
$project = Get-Item -LiteralPath $projectPath
$capturedUtc = [datetime]::UtcNow

$c78Evidence = '<not supplied>'
if ($hasBuildLog) {
    $outputDirectory = Split-Path -Parent $outputFull
    if ([string]::IsNullOrWhiteSpace($outputDirectory)) {
        $outputDirectory = $root
    }
    $c78Evidence = Join-Path $outputDirectory (([IO.Path]::GetFileNameWithoutExtension($outputFull)) + '.c78.md')
    & $c78Capture `
        -RepositoryRoot $root `
        -BuildLogPath ([IO.Path]::GetFullPath($BuildLogPath)) `
        -OutputPath $c78Evidence `
        -BuildStartedUtc $BuildStartedUtc
    Assert-Condition ($LASTEXITCODE -eq 0) 'Fresh C78 evidence collector failed.'
    Assert-Condition (Test-Path -LiteralPath $c78Evidence -PathType Leaf) 'Fresh C78 evidence collector did not create its output.'
}

$identityText = '<pending same-image PLC read>'
if ($identityAny) {
    $identityText = "Build=$DiagnosticsBuild / BootId=$DiagnosticsBootId / MapRevision=$MapRevision"
}
$endpointText = if ([string]::IsNullOrWhiteSpace($Endpoint)) { '<pending>' } else { $Endpoint }
$loadText = if ([string]::IsNullOrWhiteSpace($PlcLoadTimestamp)) { '<pending>' } else { $PlcLoadTimestamp }

$markdown = @"
# SetOperationMode MODE-11 Bench Candidate Identity Checkpoint

- Status: **PRE-HARDWARE / MODE-11 NOT YET PASSED**
- CapturedUtc: ``$($capturedUtc.ToString('o'))``
- Branch: ``$branch``
- RepositoryHead: ``$head``
- CandidateSourceState: ``BENCH_ACTIVE``
- ProductionActivation: **OFF / DO NOT MERGE**
- ActiveSourceDiffSha256: ``$activeDiffSha``

This checkpoint proves candidate identity and static safety contracts only. It does not prove PLC load,
command terminal behavior, SDO packet causality, physical drive effect, MODE-11, MODE-12, or MODE-14.

## Source identity

| Source | Bytes | SHA-256 |
|---|---:|---|
| ``$DiagnosticsRelative`` | $((Get-Item -LiteralPath $diagnosticsPath).Length) | ``$(Get-FileSha256 -Path $diagnosticsPath)`` |
| ``$ControlRelative`` | $((Get-Item -LiteralPath $controlPath).Length) | ``$(Get-FileSha256 -Path $controlPath)`` |

## Generated artifact identity

| Artifact | Bytes | LastWriteUtc | SHA-256 |
|---|---:|---|---|
| ``$ClassesRelative`` | $($classes.Length) | ``$($classes.LastWriteTimeUtc.ToString('o'))`` | ``$(Get-FileSha256 -Path $classesPath)`` |
| ``$ProjectRelative`` | $($project.Length) | ``$($project.LastWriteTimeUtc.ToString('o'))`` | ``$(Get-FileSha256 -Path $projectPath)`` |

## Build / PLC identity

- C78 evidence: ``$c78Evidence``
- PLC load timestamp: ``$loadText``
- Diagnostics identity: ``$identityText``
- Endpoint: ``$endpointText``

## Working tree at capture

```text
$status
```

## Required next evidence

1. Fresh C78/ARM rebuild/link of this BENCH_ACTIVE source and same-image PLC load.
2. Record nonzero DiagnosticsBuild, DiagnosticsBootId and MapRevision after load.
3. MODE-11A axis-1 already-CSP proof: ``0x6061=8`` and zero ``0x6060`` writes.
4. MODE-11B axis-1 independently approved non-CSP setup and exactly one one-byte ``0x6060:0=8`` write.
5. Correlate ``0x7D23`` Start, ``0x7D24`` terminal outcome and exact-generation ``0x7D25`` retire.
6. Preserve packet/SDO evidence; any uncertainty moves to MODE-12 without replaying Start.
"@

$outputDirectory = Split-Path -Parent $outputFull
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
[IO.File]::WriteAllText(
    $outputFull,
    $markdown.Replace("`r`n", "`n"),
    (New-Object System.Text.UTF8Encoding($false)))

Write-Host "PASS MODE-11 BENCH_ACTIVE source identity"
Write-Host "PASS MODE-11 candidate safety contract and define-order gates"
Write-Host "PASS artifact identity captured: Classes=$($classes.Length) bytes, Project=$($project.Length) bytes"
Write-Host "PASS active source diff SHA-256: $activeDiffSha"
Write-Host "PASS MODE-11 bench checkpoint written: $outputFull"
Write-Host 'NOT_RUN hardware/packet qualification remains pending'

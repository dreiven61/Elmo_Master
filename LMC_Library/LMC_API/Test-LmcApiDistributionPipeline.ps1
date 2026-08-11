[CmdletBinding()]
param()

$env:PSModulePath = [System.IO.Path]::Combine($PSHOME, 'Modules')
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

$implementation = Join-Path $PSScriptRoot 'DistributionPipeline.ps1'
if (-not (Test-Path -LiteralPath $implementation -PathType Leaf)) {
    throw "Distribution pipeline implementation not found: $implementation"
}
. $implementation

$toolingHostParity = Join-Path $PSScriptRoot `
    'Test-LmcDistributionToolingHostParity.ps1'
if (-not (Test-Path -LiteralPath $toolingHostParity -PathType Leaf)) {
    throw "Distribution tooling host-parity implementation not found: $toolingHostParity"
}
. $toolingHostParity

$script:Passed = 0
$script:TrackedReparsePaths = New-Object `
    'System.Collections.Generic.List[string]'

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
    $script:Passed += 1
}

function Assert-Equal {
    param(
        [AllowNull()]
        [object]$Expected,
        [AllowNull()]
        [object]$Actual,
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not [object]::Equals($Expected, $Actual)) {
        throw "$Message expected='$Expected' actual='$Actual'"
    }
    $script:Passed += 1
}

function Assert-Throws {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedMessage
    )

    $caught = $null
    try {
        & $Action | Out-Null
    }
    catch {
        $caught = $_.Exception.Message
    }
    if ($null -eq $caught) {
        throw "Expected an exception containing: $ExpectedMessage"
    }
    if ($caught.IndexOf(
        $ExpectedMessage,
        [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Unexpected exception. expected='$ExpectedMessage' actual='$caught'"
    }
    $script:Passed += 1
    return $caught
}

function Write-TestFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $parent = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    [System.IO.File]::WriteAllBytes(
        $Path,
        [System.Text.Encoding]::ASCII.GetBytes($Content))
}

function Get-LasalValidationFixtureFingerprint {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $repository = [System.IO.Path]::GetFullPath(
        $RepositoryRoot).TrimEnd('\')
    $repositoryPrefix = $repository + '\'
    $records = @()
    foreach ($file in @(Get-LmcLasalValidationInputFiles `
        -RepositoryRoot $repository)) {
        $fullPath = [System.IO.Path]::GetFullPath($file)
        if (-not $fullPath.StartsWith(
            $repositoryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Fixture LASAL input escaped its repository: $fullPath"
        }
        $relativePath = $fullPath.Substring(
            $repositoryPrefix.Length).Replace('\', '/')
        $item = Get-Item -LiteralPath $fullPath
        $hash = (Get-FileHash `
            -LiteralPath $fullPath `
            -Algorithm SHA256).Hash.ToUpperInvariant()
        $records += "$relativePath|$($item.Length)|$hash"
    }
    $canonical = ($records -join "`n") + "`n"
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
            $sha.ComputeHash(
                [System.Text.Encoding]::UTF8.GetBytes($canonical)))).
            Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function New-ExampleSolutionFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $fixtureRoot = Join-Path $script:TestRoot (
        'example-solution-' + $Name)
    $solutionPath = Join-Path $fixtureRoot (
        '02_Example_Program\LasalApiWpfTestApp.sln')
    $projectPath = Join-Path $fixtureRoot (
        '02_Example_Program\LasalApiWpfTestApp\' +
        'LasalApiWpfTestApp.csproj')
    $projectGuid = '{337B4AB9-AFEC-4706-9DBB-4D78122DB2D2}'
    $projectText = @'
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectGuid>{337B4AB9-AFEC-4706-9DBB-4D78122DB2D2}</ProjectGuid>
  </PropertyGroup>
</Project>
'@
    $solutionText = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 16
VisualStudioVersion = 16.0.33027.164
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "LasalApiWpfTestApp", "LasalApiWpfTestApp\LasalApiWpfTestApp.csproj", "$projectGuid"
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
    GlobalSection(ProjectConfigurationPlatforms) = postSolution
        ${projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        ${projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
        ${projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
        ${projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU
    EndGlobalSection
    GlobalSection(SolutionProperties) = preSolution
        HideSolutionNode = FALSE
    EndGlobalSection
EndGlobal
"@
    $solutionText = $solutionText -replace "`r?`n", "`r`n"
    Write-TestFile -Path $projectPath -Content $projectText
    Write-TestFile -Path $solutionPath -Content $solutionText

    return [pscustomobject]@{
        Root = $fixtureRoot
        Solution = $solutionPath
        Project = $projectPath
        ProjectGuid = $projectGuid
    }
}

function New-TestFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $fixtureRoot = Join-Path $script:TestRoot $Name
    $canonical = Join-Path $fixtureRoot 'LMC_API_Distribution'
    New-Item -ItemType Directory -Path (
        Join-Path $canonical '01_API') -Force | Out-Null
    New-Item -ItemType Directory -Path (
        Join-Path $canonical '02_Example_Program/EmptyDirectory') `
        -Force | Out-Null
    Write-TestFile `
        -Path (Join-Path $canonical '01_API/LasalMotionControlLib.dll') `
        -Content 'canonical-dll-v1'
    Write-TestFile `
        -Path (Join-Path $canonical 'README.md') `
        -Content 'canonical-readme-v1'
    Write-TestFile `
        -Path (Join-Path $canonical '.hidden-fixture') `
        -Content 'hidden-canonical-input'

    return [pscustomobject]@{
        Root = $fixtureRoot
        Canonical = $canonical
        Parent = $fixtureRoot
        Candidate = Join-Path $fixtureRoot (
            'LMC_API_Distribution_candidate_' + $Name)
    }
}

function Populate-TestCandidate {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Stage,
        [string]$Marker = 'candidate-v1'
    )

    New-Item -ItemType Directory -Path (
        Join-Path $Stage '01_API') -Force | Out-Null
    New-Item -ItemType Directory -Path (
        Join-Path $Stage '02_Example_Program/EmptyDirectory') `
        -Force | Out-Null
    Write-TestFile `
        -Path (Join-Path $Stage '01_API/LasalMotionControlLib.dll') `
        -Content $Marker
    Write-TestFile `
        -Path (Join-Path $Stage 'README.md') `
        -Content 'candidate-readme'
}

function Get-StagingDirectories {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Parent
    )

    return @(
        Get-ChildItem -LiteralPath $Parent -Directory -Force |
            Where-Object {
                $_.Name -like '.LMC_API_Distribution.stage.*'
            }
    )
}

function Assert-NoTransactionResidue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Fixture,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    Assert-Equal `
        -Expected 0 `
        -Actual (@(Get-StagingDirectories -Parent $Fixture.Parent).Count) `
        -Message "$Context left a staging directory."
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath (
            Join-Path $Fixture.Parent `
                '.LMC_API_Distribution.transaction.lock'))) `
        -Message "$Context left the transaction lock file behind."
}

function Assert-CanonicalUnchanged {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Before,
        [Parameter(Mandatory = $true)]
        [string]$Canonical,
        [Parameter(Mandatory = $true)]
        [string]$Context
    )

    $after = Get-LmcDistributionTreeSnapshot -Root $Canonical
    Assert-Equal `
        -Expected $Before.Sha256 `
        -Actual $after.Sha256 `
        -Message "$Context changed the canonical package."
}

function Remove-TestRootSafely {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    foreach ($reparsePath in @($script:TrackedReparsePaths)) {
        if (-not (Test-Path -LiteralPath $reparsePath)) {
            continue
        }
        $item = Get-Item -LiteralPath $reparsePath -Force
        $fullReparsePath = [System.IO.Path]::GetFullPath($item.FullName)
        $fullTestRoot = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
        if (-not $fullReparsePath.StartsWith(
            $fullTestRoot + '\',
            [System.StringComparison]::OrdinalIgnoreCase) -or
            -not (Test-LmcDistributionReparsePoint -Item $item)) {
            throw "Refusing test cleanup for an unexpected path: $fullReparsePath"
        }
        [System.IO.Directory]::Delete($fullReparsePath)
    }

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    $tempRoot = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::GetTempPath()).TrimEnd('\')
    if (-not $fullPath.StartsWith(
        $tempRoot + '\',
        [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($fullPath) -notmatch
            '^LmcDistributionPipelineTest-[0-9a-f]{32}$') {
        throw "Refusing to remove an unsafe test root: $fullPath"
    }
    Assert-LmcDistributionTreeHasNoReparsePoints `
        -Root $fullPath `
        -Context 'Test cleanup tree'
    foreach ($file in @(
        Get-ChildItem -LiteralPath $fullPath -Recurse -Force -File)) {
        $attributes = [System.IO.File]::GetAttributes($file.FullName)
        if (($attributes -band [System.IO.FileAttributes]::ReadOnly) -ne 0) {
            [System.IO.File]::SetAttributes(
                $file.FullName,
                [System.IO.FileAttributes](
                    $attributes -bxor [System.IO.FileAttributes]::ReadOnly))
        }
    }
    [System.IO.Directory]::Delete($fullPath, $true)
}

function Get-CurrentTestPowerShellExecutable {
    if ($PSVersionTable.PSEdition -ceq 'Desktop') {
        return Join-Path $env:WINDIR `
            'System32\WindowsPowerShell\v1.0\powershell.exe'
    }
    return Join-Path $PSHOME 'pwsh.exe'
}

function ConvertTo-TestEncodedPowerShellArguments {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command
    )

    $isolatedCommand =
        '$ProgressPreference = ''SilentlyContinue''; ' + $Command
    $encoded = [System.Convert]::ToBase64String(
        [System.Text.Encoding]::Unicode.GetBytes($isolatedCommand))
    return @(
        '-NoLogo',
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-EncodedCommand', $encoded)
}

$systemTemp = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::GetTempPath()).TrimEnd('\')
$script:TestRoot = Join-Path $systemTemp (
    'LmcDistributionPipelineTest-' + [System.Guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $script:TestRoot | Out-Null

    # The dual-host tooling gate is exact, non-recursive, and ordered before
    # every canonical/manual/tool-discovery or transaction operation.
    $repositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..'))
    $builderPath = Join-Path $PSScriptRoot `
        'Build-LmcApiDistribution.ps1'
    $builderText = [System.IO.File]::ReadAllText($builderPath)
    Assert-True `
        -Condition (Assert-LmcDistributionBuilderPreflightOrder `
            -BuilderText $builderText) `
        -Message 'Current builder tooling-preflight order was rejected.'
    $requiredInventoryOccurrences = @{
        'Test-LmcDistributionToolingHostParity.ps1' = 2
        'Test-LmcApiDistributionPipeline.ps1' = 1
        'Test-LmcDistributionSemanticPolicy.ps1' = 1
        'Test-LmcReleaseManifest.ps1' = 1
    }
    foreach ($requiredInventoryName in @(
            $requiredInventoryOccurrences.Keys | Sort-Object)) {
        Assert-Equal `
            -Expected $requiredInventoryOccurrences[$requiredInventoryName] `
            -Actual ([regex]::Matches(
                $builderText,
                [regex]::Escape("'$requiredInventoryName'"))).Count `
            -Message "Builder release-input inventory does not pin $requiredInventoryName."
    }
    Assert-True `
        -Condition ($builderText.Contains(
                '@validated-tooling-preflight|$ValidatedToolingDigest')) `
        -Message 'Builder input-tree fingerprint does not bind the validated tooling digest.'
    Assert-True `
        -Condition ([regex]::Matches(
                $builderText,
                'Get-LmcDistributionOrdinalSortedUniqueStrings').Count -eq 2) `
        -Message 'Builder release-input path inventories are not both ordinal-canonicalized.'
    Assert-True `
        -Condition ([regex]::Matches(
                $builderText,
                'Assert-LmcDistributionMonitoredFileSnapshot').Count -ge 4) `
        -Message 'Builder does not reassert tooling bytes across both transaction gaps.'
    Assert-True `
        -Condition ($builderText.Contains(
                '$selectedToolingSnapshot = if ($null -eq $preparedInputs)') -and
            $builderText.Contains('$toolingPreflight')) `
        -Message 'Builder does not select the preflight snapshot for the second fingerprint call.'

    $orderedBuilderFixture = @'
Invoke-LmcDistributionToolingHostParityPreflight -RepositoryRoot $RepositoryRoot
$canonicalDistribution = 'canonical'
$null = Resolve-LmcDistributionManualInputs
$vswhere = 'vswhere'
$pythonCandidates = @()
if ([string]::IsNullOrWhiteSpace($CandidatePath)) { $CandidatePath = 'candidate' }
$null = Invoke-LmcDistributionCandidateTransaction
'@
    Assert-True `
        -Condition (Assert-LmcDistributionBuilderPreflightOrder `
            -BuilderText $orderedBuilderFixture) `
        -Message 'Ordered builder fixture was rejected.'
    $latePreflightFixture = $orderedBuilderFixture.Replace(
        'Invoke-LmcDistributionToolingHostParityPreflight -RepositoryRoot $RepositoryRoot',
        '').Replace(
        '$null = Invoke-LmcDistributionCandidateTransaction',
        "$null = Invoke-LmcDistributionCandidateTransaction`n" +
        'Invoke-LmcDistributionToolingHostParityPreflight -RepositoryRoot $RepositoryRoot')
    Assert-Throws `
        -Action {
            Assert-LmcDistributionBuilderPreflightOrder `
                -BuilderText $latePreflightFixture
        } `
        -ExpectedMessage 'must precede' | Out-Null
    $duplicatePreflightFixture =
        $orderedBuilderFixture + "`n" +
        'Invoke-LmcDistributionToolingHostParityPreflight -RepositoryRoot $RepositoryRoot'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionBuilderPreflightOrder `
                -BuilderText $duplicatePreflightFixture
        } `
        -ExpectedMessage 'exactly once' | Out-Null

    $suiteSpecifications = @(
        Get-LmcDistributionToolingSuiteSpecifications `
            -RepositoryRoot $repositoryRoot)
    Assert-LmcDistributionToolingSuiteSpecifications `
        -Specifications $suiteSpecifications
    Assert-Equal `
        -Expected 6 `
        -Actual $suiteSpecifications.Count `
        -Message 'Dual-host tooling suite inventory is not exact.'
    $expectedTimeouts = @{
        Pipeline = 300
        SemanticPolicy = 120
        ReleaseManifest = 120
        MethodSize = 180
        UdpCallback = 900
        ControlHandleRequest = 180
    }
    foreach ($suiteSpecification in $suiteSpecifications) {
        Assert-Equal `
            -Expected $expectedTimeouts[$suiteSpecification.Id] `
            -Actual $suiteSpecification.TimeoutSeconds `
            -Message "Tooling timeout map drifted for $($suiteSpecification.Id)."
    }
    $reorderedSpecifications = @(
        $suiteSpecifications[1],
        $suiteSpecifications[0],
        $suiteSpecifications[2],
        $suiteSpecifications[3],
        $suiteSpecifications[4],
        $suiteSpecifications[5])
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $reorderedSpecifications
        } `
        -ExpectedMessage 'suite order' | Out-Null
    $duplicateSpecifications = @(
        $suiteSpecifications[0],
        $suiteSpecifications[1],
        $suiteSpecifications[2],
        $suiteSpecifications[3],
        $suiteSpecifications[4],
        $suiteSpecifications[0])
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $duplicateSpecifications
        } `
        -ExpectedMessage 'duplicated' | Out-Null
    $recursiveSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $recursiveSpecifications[0].RelativePath =
        'LMC_Library/LMC_API/Build-LmcApiDistribution.ps1'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $recursiveSpecifications
        } `
        -ExpectedMessage 'recursion is forbidden' | Out-Null
    $pathDriftSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $pathDriftSpecifications[0].RelativePath =
        'LMC_Library/LMC_API/Test-AlternatePipeline.ps1'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $pathDriftSpecifications
        } `
        -ExpectedMessage 'exact contract drifted' | Out-Null
    $evidenceDriftSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $evidenceDriftSpecifications[0].EvidencePattern =
        '^PASS: [0-9]+ distribution pipeline assertions$'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $evidenceDriftSpecifications
        } `
        -ExpectedMessage 'exact contract drifted' | Out-Null
    $evidenceLineDriftSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $evidenceLineDriftSpecifications[0].EvidenceLine =
        'PASS: non-exact pipeline assertions'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $evidenceLineDriftSpecifications
        } `
        -ExpectedMessage 'exact contract drifted' | Out-Null
    $timeoutDriftSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $timeoutDriftSpecifications[0].TimeoutSeconds = 301
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $timeoutDriftSpecifications
        } `
        -ExpectedMessage 'exact contract drifted' | Out-Null
    $terminationDriftSpecifications = @(
        $suiteSpecifications |
            ForEach-Object { $_.PSObject.Copy() })
    $terminationDriftSpecifications[0].WorkerTerminates = $true
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingSuiteSpecifications `
                -Specifications $terminationDriftSpecifications
        } `
        -ExpectedMessage 'exact contract drifted' | Out-Null

    # Host discovery rejects missing/spoofed/ambiguous executables while
    # accepting the normal real-pwsh plus zero-byte AppExecutionAlias set.
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionPowerShellHost `
                -Name 'MissingFixture' `
                -CandidatePaths @(
                    (Join-Path $script:TestRoot 'missing-powershell.exe')) `
                -WorkingDirectory $repositoryRoot `
                -ExpectedEdition 'Core' `
                -MinimumMajor 7 `
                -MaximumMajor ([int]::MaxValue)
        } `
        -ExpectedMessage 'was not found as a physical executable' | Out-Null

    $currentHostExecutable = Get-CurrentTestPowerShellExecutable
    $spoofEdition = if ($PSVersionTable.PSEdition -ceq 'Desktop') {
        'Core'
    }
    else {
        'Desktop'
    }
    $spoofMinimum = if ($spoofEdition -ceq 'Core') { 7 } else { 5 }
    $spoofMaximum = if ($spoofEdition -ceq 'Core') {
        [int]::MaxValue
    }
    else {
        5
    }
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionPowerShellHost `
                -Name 'SpoofFixture' `
                -CandidatePaths @($currentHostExecutable) `
                -WorkingDirectory $repositoryRoot `
                -ExpectedEdition $spoofEdition `
                -MinimumMajor $spoofMinimum `
                -MaximumMajor $spoofMaximum
        } `
        -ExpectedMessage 'identity was not accepted' | Out-Null

    $pwshCandidates = @(
        Get-Command pwsh.exe -CommandType Application -All `
            -ErrorAction SilentlyContinue |
            ForEach-Object { [string]$_.Source })
    $structuralProbe = {
        param($path, $working, $edition, $minimum, $maximum)
        [pscustomobject]@{
            Edition = 'Core'
            Major = 7
            Version = '7.fixture'
            PowerShellHome = Split-Path -Parent $path
            ModulePath = Join-Path (Split-Path -Parent $path) 'Modules'
        }
    }
    $resolvedPhysicalPwsh = Resolve-LmcDistributionPowerShellHost `
        -Name 'AliasFilterFixture' `
        -CandidatePaths $pwshCandidates `
        -WorkingDirectory $repositoryRoot `
        -ExpectedEdition 'Core' `
        -MinimumMajor 7 `
        -MaximumMajor ([int]::MaxValue) `
        -IdentityProbe $structuralProbe
    Assert-True `
        -Condition ((Get-Item -LiteralPath $resolvedPhysicalPwsh.Path).Length -gt 0 -and
            (((Get-Item -LiteralPath $resolvedPhysicalPwsh.Path).Attributes -band
                [System.IO.FileAttributes]::ReparsePoint) -eq 0)) `
        -Message 'Host resolver accepted the zero-byte/reparse AppExecutionAlias.'

    $snapshotParityCommand = @"
`$ProgressPreference = 'SilentlyContinue'
`$env:PSModulePath = [IO.Path]::Combine(`$PSHOME, 'Modules')
. '$($toolingHostParity.Replace("'", "''"))'
`$snapshot = Get-LmcDistributionMonitoredFileSnapshot -RepositoryRoot '$($repositoryRoot.Replace("'", "''"))'
foreach (`$record in `$snapshot.Records) {
    [Console]::Out.WriteLine('RECORD|' + `$record)
}
[Console]::Out.WriteLine('DIGEST|' + `$snapshot.Digest)
"@
    $snapshotParityArguments = ConvertTo-TestEncodedPowerShellArguments `
        -Command $snapshotParityCommand
    $snapshotPs5 = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath (Join-Path $env:WINDIR `
            'System32\WindowsPowerShell\v1.0\powershell.exe') `
        -Arguments $snapshotParityArguments `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 60 `
        -RemoveEnvironmentVariables @('PSModulePath') `
        -EnvironmentOverrides @{ PSModulePath = 'LMC_SORT_POISON_PS5' }
    $snapshotPs7 = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $resolvedPhysicalPwsh.Path `
        -Arguments $snapshotParityArguments `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 60 `
        -RemoveEnvironmentVariables @('PSModulePath') `
        -EnvironmentOverrides @{ PSModulePath = 'LMC_SORT_POISON_PS7' }
    Assert-True `
        -Condition ($snapshotPs5.ExitCode -eq 0 -and
            $snapshotPs7.ExitCode -eq 0 -and
            [string]::IsNullOrWhiteSpace($snapshotPs5.StandardError) -and
            [string]::IsNullOrWhiteSpace($snapshotPs7.StandardError)) `
        -Message 'Cross-host production snapshot fixture did not exit cleanly.'
    $snapshotPs5Text = $snapshotPs5.StandardOutput.Replace(
        "`r`n", "`n").TrimEnd("`n")
    $snapshotPs7Text = $snapshotPs7.StandardOutput.Replace(
        "`r`n", "`n").TrimEnd("`n")
    Assert-Equal `
        -Expected $snapshotPs5Text `
        -Actual $snapshotPs7Text `
        -Message 'PS5/PS7 production snapshot records or digest bytes drifted.'
    $snapshotParityLines = @($snapshotPs5Text -split "`n")
    $snapshotRecordLines = @($snapshotParityLines | Where-Object {
            $_.StartsWith('RECORD|', [System.StringComparison]::Ordinal)
        })
    $snapshotRelativePaths = [string[]]@(
        $snapshotRecordLines |
            ForEach-Object { (($_ -split '\|', 4)[1]) })
    [string[]]$snapshotOrdinalPaths = @($snapshotRelativePaths)
    [System.Array]::Sort(
        $snapshotOrdinalPaths,
        [System.StringComparer]::Ordinal)
    Assert-True `
        -Condition ($snapshotRecordLines.Count -eq 92 -and
            $snapshotParityLines.Count -eq 93 -and
            $snapshotParityLines[-1] -match '^DIGEST\|[0-9A-F]{64}$' -and
            (($snapshotRelativePaths -join "`n") -ceq
                ($snapshotOrdinalPaths -join "`n"))) `
        -Message 'Cross-host production snapshot is not exact 92-record ordinal canonical data.'

    $ambiguousRoot = Join-Path $script:TestRoot 'ambiguous-hosts'
    New-Item -ItemType Directory -Path $ambiguousRoot -Force |
        Out-Null
    $ambiguousA = Join-Path $ambiguousRoot 'host-a.exe'
    $ambiguousB = Join-Path $ambiguousRoot 'host-b.exe'
    Copy-Item -LiteralPath $currentHostExecutable `
        -Destination $ambiguousA -Force
    Copy-Item -LiteralPath $currentHostExecutable `
        -Destination $ambiguousB -Force
    $currentEdition = [string]$PSVersionTable.PSEdition
    $currentMajor = [int]$PSVersionTable.PSVersion.Major
    $ambiguousProbe = {
        param($path, $working, $edition, $minimum, $maximum)
        [pscustomobject]@{
            Edition = $currentEdition
            Major = $currentMajor
            Version = [string]$PSVersionTable.PSVersion
            PowerShellHome = $PSHOME
            ModulePath = Join-Path $PSHOME 'Modules'
        }
    }
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionPowerShellHost `
                -Name 'AmbiguousFixture' `
                -CandidatePaths @($ambiguousA, $ambiguousB) `
                -WorkingDirectory $repositoryRoot `
                -ExpectedEdition $currentEdition `
                -MinimumMajor $currentMajor `
                -MaximumMajor $currentMajor `
                -IdentityProbe $ambiguousProbe
        } `
        -ExpectedMessage 'resolution is ambiguous' | Out-Null

    # Exit code, terminal line, required evidence, timeout/tree kill, and
    # redirected-stream draining are all independently non-vacuous.
    $fixtureMarker = 'LMC_FIXTURE_' +
        [System.Guid]::NewGuid().ToString('N')
    $nonzeroResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command "Write-Output '$fixtureMarker'; exit 7") `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-Throws `
        -Action {
            Assert-LmcDistributionProcessResult `
                -Result $nonzeroResult `
                -ExpectedTerminalLine $fixtureMarker `
                -ExpectedEvidencePatterns @(
                    '^' + [regex]::Escape($fixtureMarker) + '$')
        } `
        -ExpectedMessage 'exited abnormally' | Out-Null

    $noEvidenceResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command "Write-Output '$fixtureMarker'") `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-Throws `
        -Action {
            Assert-LmcDistributionProcessResult `
                -Result $noEvidenceResult `
                -ExpectedTerminalLine $fixtureMarker `
                -ExpectedEvidencePatterns @('^REQUIRED_NONVACUOUS_EVIDENCE$')
        } `
        -ExpectedMessage 'evidence occurrence drifted' | Out-Null

    $duplicateEvidenceTerminal = 'LMC_DUPLICATE_TERMINAL_PASS'
    $duplicateEvidenceResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command (
                "Write-Output '$fixtureMarker'; " +
                "Write-Output '$fixtureMarker'; " +
                "Write-Output '$duplicateEvidenceTerminal'")) `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-Throws `
        -Action {
            Assert-LmcDistributionProcessResult `
                -Result $duplicateEvidenceResult `
                -ExpectedTerminalLine $duplicateEvidenceTerminal `
                -ExpectedEvidencePatterns @(
                    '^' + [regex]::Escape($fixtureMarker) + '$')
        } `
        -ExpectedMessage 'count=2' | Out-Null

    $tamperedTerminalResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command (
                "Write-Output '$fixtureMarker'; " +
                "Write-Output 'TAMPER_AFTER_PASS'")) `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-Throws `
        -Action {
            Assert-LmcDistributionProcessResult `
                -Result $tamperedTerminalResult `
                -ExpectedTerminalLine $fixtureMarker `
                -ExpectedEvidencePatterns @(
                    '^' + [regex]::Escape($fixtureMarker) + '$')
        } `
        -ExpectedMessage 'terminal evidence drifted' | Out-Null

    $timeoutPidPath = Join-Path $script:TestRoot 'timeout-child.pid'
    $timeoutCommand =
        "[IO.File]::WriteAllText('$($timeoutPidPath.Replace("'", "''"))',[string]`$PID);" +
        '[System.Threading.Thread]::Sleep(5000)'
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionRawPowerShellProcess `
                -ExecutablePath $currentHostExecutable `
                -Arguments (ConvertTo-TestEncodedPowerShellArguments `
                    -Command $timeoutCommand) `
                -WorkingDirectory $repositoryRoot `
                -TimeoutSeconds 1
        } `
        -ExpectedMessage 'timed out after 1 seconds' | Out-Null
    $timedOutProcessId = [int]([System.IO.File]::ReadAllText(
        $timeoutPidPath))
    $timedOutProcessGone = $false
    try {
        $null = [System.Diagnostics.Process]::GetProcessById(
            $timedOutProcessId)
    }
    catch {
        $timedOutProcessGone = $true
    }
    Assert-True `
        -Condition $timedOutProcessGone `
        -Message 'Timeout fixture left its exact child process alive.'

    $stderrResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command (
                "[Console]::Error.WriteLine('UNEXPECTED_STDERR');" +
                "Write-Output '$fixtureMarker'")) `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-Throws `
        -Action {
            Assert-LmcDistributionProcessResult `
                -Result $stderrResult `
                -ExpectedTerminalLine $fixtureMarker `
                -ExpectedEvidencePatterns @(
                    '^' + [regex]::Escape($fixtureMarker) + '$')
        } `
        -ExpectedMessage 'wrote stderr' | Out-Null

    $largeOutputCommand = @'
$chunk = 'x' * 256
for ($i = 0; $i -lt 4096; $i++) {
    [Console]::Out.WriteLine($chunk)
    [Console]::Error.WriteLine($chunk)
}
[Console]::Out.WriteLine('LMC_LARGE_OUTPUT_PASS')
'@
    $largeOutputResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments (ConvertTo-TestEncodedPowerShellArguments `
            -Command $largeOutputCommand) `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-LmcDistributionProcessResult `
        -Result $largeOutputResult `
        -ExpectedTerminalLine 'LMC_LARGE_OUTPUT_PASS' `
        -ExpectedEvidencePatterns @('^LMC_LARGE_OUTPUT_PASS$') `
        -AllowStandardError
    Assert-True `
        -Condition ($largeOutputResult.StandardOutput.Length -gt 1000000 -and
            $largeOutputResult.StandardError.Length -gt 1000000) `
        -Message 'Large-output fixture did not exercise both redirected streams.'

    # A poisoned inherited PSModulePath cannot reach the suite: the internal
    # worker resets it to the validated host's exact PSHOME\Modules first.
    $moduleNonce = [System.Guid]::NewGuid().ToString('N')
    $workerArguments = @(
        '-NoLogo', '-NoProfile', '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-File', $toolingHostParity,
        '-WorkerSuite', 'ReleaseManifest',
        '-WorkerRepositoryRootBase64',
            (ConvertTo-LmcDistributionBase64 -Text $repositoryRoot),
        '-WorkerPowerShellHomeBase64',
            (ConvertTo-LmcDistributionBase64 -Text $PSHOME),
        '-WorkerNonce', $moduleNonce)
    $poisonedModuleResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments $workerArguments `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 120 `
        -RemoveEnvironmentVariables @('PSModulePath') `
        -EnvironmentOverrides @{
            PSModulePath = "LMC_USER_MODULE_POISON_$moduleNonce"
        }
    Assert-LmcDistributionProcessResult `
        -Result $poisonedModuleResult `
        -ExpectedTerminalLine (
            "PASS LMC.DistributionToolingWorker ReleaseManifest $moduleNonce") `
        -ExpectedEvidencePatterns @(
            ('^LMC_TOOLING_MODULE_PATH ' + [regex]::Escape($moduleNonce) +
                ' ' + [regex]::Escape((ConvertTo-LmcDistributionBase64 `
                    -Text (Join-Path $PSHOME 'Modules'))) + '$')
            '^TOTAL 56, PASSED 56, FAILED 0$')
    Assert-True `
        -Condition ($poisonedModuleResult.StandardOutput -notmatch
            'LMC_USER_MODULE_POISON') `
        -Message 'Poisoned parent PSModulePath leaked into worker evidence.'

    $noOperationResult = Invoke-LmcDistributionRawPowerShellProcess `
        -ExecutablePath $currentHostExecutable `
        -Arguments @(
            '-NoLogo', '-NoProfile', '-NonInteractive',
            '-ExecutionPolicy', 'Bypass',
            '-File', $toolingHostParity) `
        -WorkingDirectory $repositoryRoot `
        -TimeoutSeconds 30
    Assert-True `
        -Condition ($noOperationResult.ExitCode -ne 0 -and
            $noOperationResult.StandardOutput -notmatch
                '(?m)^PASS LMC\.DistributionToolingHostParity') `
        -Message 'Direct no-operation tooling invocation exited vacuously with PASS.'

    # The validated-byte snapshot blocks both the pre-fingerprint gap and the
    # post-populate/pre-promotion gap without touching a canonical package.
    $toolingGuard = Join-Path $script:TestRoot 'tooling-guard.ps1'
    Write-TestFile -Path $toolingGuard -Content 'guard-v1'
    $guardRelative = 'tooling-guard.ps1'
    $guardSnapshot = Get-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $script:TestRoot `
        -RelativePaths @($guardRelative)
    Write-TestFile -Path $toolingGuard -Content 'guard-v2'
    $preFingerprintCallbackCount = 0
    Assert-Throws `
        -Action {
            Assert-LmcDistributionMonitoredFileSnapshot `
                -RepositoryRoot $script:TestRoot `
                -ExpectedSnapshot $guardSnapshot `
                -RelativePaths @($guardRelative) | Out-Null
            $preFingerprintCallbackCount++
        } `
        -ExpectedMessage 'monitored bytes changed after validation' | Out-Null
    Assert-Equal `
        -Expected 0 `
        -Actual $preFingerprintCallbackCount `
        -Message 'Pre-fingerprint mutation reached a transaction callback.'

    Write-TestFile -Path $toolingGuard -Content 'guard-v1'
    $guardSnapshot = Get-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $script:TestRoot `
        -RelativePaths @($guardRelative)
    $fixture = New-TestFixture -Name 'tooling_post_populate_drift'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $toolingGapObservation = [pscustomobject]@{
        FingerprintCalls = 0
        PromotionValidations = 0
    }
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PrepareInputs { $guardSnapshot } `
                -GetInputFingerprint {
                    $toolingGapObservation.FingerprintCalls++
                    Assert-LmcDistributionMonitoredFileSnapshot `
                        -RepositoryRoot $script:TestRoot `
                        -ExpectedSnapshot $guardSnapshot `
                        -RelativePaths @($guardRelative) | Out-Null
                    'tooling-guard-v1'
                } `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    Write-TestFile -Path $toolingGuard -Content 'guard-v2'
                } `
                -ValidatePreparedInputs {
                    $toolingGapObservation.PromotionValidations++
                }
        } `
        -ExpectedMessage 'monitored bytes changed after validation' | Out-Null
    Write-TestFile -Path $toolingGuard -Content 'guard-v1'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Post-populate tooling drift published a candidate.'
    Assert-Equal `
        -Expected 2 `
        -Actual $toolingGapObservation.FingerprintCalls `
        -Message 'Post-populate tooling drift did not reach exact fingerprint call two.'
    Assert-Equal `
        -Expected 0 `
        -Actual $toolingGapObservation.PromotionValidations `
        -Message 'Post-populate tooling drift reached promotion validation.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Post-populate tooling drift'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Post-populate tooling drift'

    # Manual inputs default to canonical files or accept one explicit pair.
    $manualFixtureRoot = Join-Path $script:TestRoot 'manual-inputs'
    $manualCanonicalRoot = Join-Path $manualFixtureRoot `
        'LMC_API_Distribution/03_API_User_Manual'
    $manualAuthoringRoot = Join-Path $manualFixtureRoot 'output'
    $canonicalPdf = Join-Path $manualCanonicalRoot 'manual.pdf'
    $canonicalDocx = Join-Path $manualCanonicalRoot 'manual.docx'
    $candidatePdf = Join-Path $manualAuthoringRoot 'candidate.pdf'
    $candidateDocx = Join-Path $manualAuthoringRoot 'candidate.docx'
    Write-TestFile -Path $canonicalPdf -Content 'canonical-pdf'
    Write-TestFile -Path $canonicalDocx -Content 'canonical-docx'
    Write-TestFile -Path $candidatePdf -Content 'candidate-pdf'
    Write-TestFile -Path $candidateDocx -Content 'candidate-docx'

    $manualInputs = Resolve-LmcDistributionManualInputs `
        -RepositoryRoot $manualFixtureRoot `
        -CanonicalPdfPath $canonicalPdf `
        -CanonicalDocxPath $canonicalDocx
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($canonicalPdf)) `
        -Actual $manualInputs.PdfPath `
        -Message 'Default manual PDF input was not canonical.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($canonicalDocx)) `
        -Actual $manualInputs.DocxPath `
        -Message 'Default manual DOCX input was not canonical.'
    Assert-True `
        -Condition $manualInputs.UsesCanonicalInputs `
        -Message 'Default manual inputs were not marked canonical.'

    $manualInputs = Resolve-LmcDistributionManualInputs `
        -RepositoryRoot $manualFixtureRoot `
        -CanonicalPdfPath $canonicalPdf `
        -CanonicalDocxPath $canonicalDocx `
        -ManualPdfPath $candidatePdf `
        -ManualDocxPath $candidateDocx
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($candidatePdf)) `
        -Actual $manualInputs.PdfPath `
        -Message 'Explicit manual PDF input was not selected.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($candidateDocx)) `
        -Actual $manualInputs.DocxPath `
        -Message 'Explicit manual DOCX input was not selected.'
    Assert-True `
        -Condition (-not $manualInputs.UsesCanonicalInputs) `
        -Message 'Explicit manual inputs were marked canonical.'

    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $candidatePdf
        } `
        -ExpectedMessage 'must be supplied together' | Out-Null
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath (Join-Path $manualAuthoringRoot 'missing.pdf') `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input was not found' | Out-Null
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $candidateDocx `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input must use the .pdf extension' |
        Out-Null

    $outsidePdf = Join-Path $script:TestRoot 'outside-manual.pdf'
    Write-TestFile -Path $outsidePdf -Content 'outside-pdf'
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $outsidePdf `
                -ManualDocxPath $candidateDocx
        } `
        -ExpectedMessage 'Manual PDF input escaped the repository' | Out-Null

    $manualTarget = Join-Path $manualFixtureRoot 'manual-target'
    $manualLink = Join-Path $manualFixtureRoot 'manual-link'
    $linkedPdf = Join-Path $manualLink 'linked.pdf'
    $linkedDocx = Join-Path $manualLink 'linked.docx'
    Write-TestFile `
        -Path (Join-Path $manualTarget 'linked.pdf') `
        -Content 'linked-pdf'
    Write-TestFile `
        -Path (Join-Path $manualTarget 'linked.docx') `
        -Content 'linked-docx'
    New-Item -ItemType Junction `
        -Path $manualLink `
        -Target $manualTarget | Out-Null
    $script:TrackedReparsePaths.Add($manualLink)
    Assert-Throws `
        -Action {
            Resolve-LmcDistributionManualInputs `
                -RepositoryRoot $manualFixtureRoot `
                -CanonicalPdfPath $canonicalPdf `
                -CanonicalDocxPath $canonicalDocx `
                -ManualPdfPath $linkedPdf `
                -ManualDocxPath $linkedDocx
        } `
        -ExpectedMessage 'traverses a reparse point' | Out-Null
    [System.IO.Directory]::Delete($manualLink)

    Assert-Equal `
        -Expected 'clean' `
        -Actual (Get-LmcDistributionManualWorktreeState `
            -UsesCanonicalInputs $true `
            -WorktreeState 'clean') `
        -Message 'Canonical manual inputs changed a clean worktree state.'
    Assert-Throws `
        -Action {
            Get-LmcDistributionManualWorktreeState `
                -UsesCanonicalInputs $false `
                -WorktreeState 'clean'
        } `
        -ExpectedMessage 'Noncanonical manual inputs require -AllowDirty' |
        Out-Null
    Assert-Equal `
        -Expected 'dirty-preview' `
        -Actual (Get-LmcDistributionManualWorktreeState `
            -UsesCanonicalInputs $false `
            -WorktreeState 'clean' `
            -AllowDirty) `
        -Message 'Noncanonical manual inputs were not forced dirty-preview.'

    $candidatePdfHash = (Get-FileHash `
        -LiteralPath $candidatePdf -Algorithm SHA256).Hash
    $candidateDocxHash = (Get-FileHash `
        -LiteralPath $candidateDocx -Algorithm SHA256).Hash
    $manualSnapshot = New-LmcDistributionManualInputSnapshot `
        -RepositoryRoot $manualFixtureRoot `
        -PdfPath $candidatePdf `
        -DocxPath $candidateDocx
    Assert-True `
        -Condition ($manualSnapshot.PdfBytes -is [byte[]] -and
            $manualSnapshot.DocxBytes -is [byte[]]) `
        -Message 'Manual input snapshot did not retain byte arrays.'
    Assert-Equal `
        -Expected $candidatePdfHash `
        -Actual $manualSnapshot.PdfSha256 `
        -Message 'Manual PDF snapshot hash does not match the source.'
    Assert-Equal `
        -Expected $candidateDocxHash `
        -Actual $manualSnapshot.DocxSha256 `
        -Message 'Manual DOCX snapshot hash does not match the source.'
    Assert-Equal `
        -Expected ([long](Get-Item -LiteralPath $candidatePdf).Length) `
        -Actual $manualSnapshot.PdfLength `
        -Message 'Manual PDF snapshot length does not match the source.'
    Assert-Equal `
        -Expected ([long](Get-Item -LiteralPath $candidateDocx).Length) `
        -Actual $manualSnapshot.DocxLength `
        -Message 'Manual DOCX snapshot length does not match the source.'
    Write-TestFile -Path $candidatePdf -Content 'candidate-pdf-mutated'
    Assert-Equal `
        -Expected $candidatePdfHash `
        -Actual (Get-LmcDistributionBytesSha256 `
            -Bytes $manualSnapshot.PdfBytes) `
        -Message 'Manual PDF snapshot changed with the original file.'

    # The staged example solution is a one-project, two-configuration contract.
    $validSolutionFixture = New-ExampleSolutionFixture -Name 'valid'
    $validSolutionContract =
        Assert-LmcDistributionExampleSolutionContract `
            -StagingRoot $validSolutionFixture.Root `
            -SolutionPath $validSolutionFixture.Solution `
            -ProjectPath $validSolutionFixture.Project
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath(
            $validSolutionFixture.Solution)) `
        -Actual $validSolutionContract.SolutionPath `
        -Message 'Valid solution contract returned the wrong solution path.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath(
            $validSolutionFixture.Project)) `
        -Actual $validSolutionContract.ProjectPath `
        -Message 'Valid solution contract returned the wrong project path.'
    Assert-Equal `
        -Expected $validSolutionFixture.ProjectGuid `
        -Actual $validSolutionContract.ProjectGuid `
        -Message 'Valid solution contract returned the wrong project GUID.'

    $wrongPathFixture = New-ExampleSolutionFixture -Name 'wrong-path'
    $wrongPathText = [System.IO.File]::ReadAllText(
        $wrongPathFixture.Solution).Replace(
            'LasalApiWpfTestApp\LasalApiWpfTestApp.csproj',
            'WrongProject\WrongProject.csproj')
    Write-TestFile `
        -Path $wrongPathFixture.Solution `
        -Content $wrongPathText
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExampleSolutionContract `
                -StagingRoot $wrongPathFixture.Root `
                -SolutionPath $wrongPathFixture.Solution `
                -ProjectPath $wrongPathFixture.Project
        } `
        -ExpectedMessage 'solution project path is invalid' | Out-Null

    $wrongGuidFixture = New-ExampleSolutionFixture -Name 'wrong-guid'
    $wrongSolutionGuid = '{337B4AB9-AFEC-4706-9DBB-4D78122DB2D3}'
    $wrongGuidText = [System.IO.File]::ReadAllText(
        $wrongGuidFixture.Solution).Replace(
            $wrongGuidFixture.ProjectGuid,
            $wrongSolutionGuid)
    Write-TestFile `
        -Path $wrongGuidFixture.Solution `
        -Content $wrongGuidText
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExampleSolutionContract `
                -StagingRoot $wrongGuidFixture.Root `
                -SolutionPath $wrongGuidFixture.Solution `
                -ProjectPath $wrongGuidFixture.Project
        } `
        -ExpectedMessage 'project GUID does not match' | Out-Null

    $extraProjectFixture = New-ExampleSolutionFixture -Name 'extra-project'
    $extraProjectText = [System.IO.File]::ReadAllText(
        $extraProjectFixture.Solution) -replace '(?m)^Global\r?$', @'
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ExtraProject", "ExtraProject\ExtraProject.csproj", "{11111111-1111-1111-1111-111111111111}"
EndProject
Global
'@
    Write-TestFile `
        -Path $extraProjectFixture.Solution `
        -Content $extraProjectText
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExampleSolutionContract `
                -StagingRoot $extraProjectFixture.Root `
                -SolutionPath $extraProjectFixture.Solution `
                -ProjectPath $extraProjectFixture.Project
        } `
        -ExpectedMessage 'exactly one project declaration' | Out-Null

    $missingConfigurationFixture =
        New-ExampleSolutionFixture -Name 'missing-configuration'
    $missingConfigurationLine =
        $missingConfigurationFixture.ProjectGuid +
        '.Release|Any CPU.Build.0 = Release|Any CPU'
    $missingConfigurationText = [regex]::Replace(
        [System.IO.File]::ReadAllText(
            $missingConfigurationFixture.Solution),
        '(?m)^[\t ]*' + [regex]::Escape($missingConfigurationLine) +
            '[\t ]*(?:\r?\n|$)',
        '')
    Write-TestFile `
        -Path $missingConfigurationFixture.Solution `
        -Content $missingConfigurationText
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExampleSolutionContract `
                -StagingRoot $missingConfigurationFixture.Root `
                -SolutionPath $missingConfigurationFixture.Solution `
                -ProjectPath $missingConfigurationFixture.Project
        } `
        -ExpectedMessage 'project configuration contract is invalid' |
        Out-Null

    # The release builder must validate and build the exact staged solution
    # before the Run copy/gate/manifest/final identity sequence.
    $builderPath = Join-Path $PSScriptRoot 'Build-LmcApiDistribution.ps1'
    $builderTokens = $null
    $builderParseErrors = $null
    $builderAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $builderPath,
        [ref]$builderTokens,
        [ref]$builderParseErrors)
    Assert-Equal `
        -Expected 0 `
        -Actual @($builderParseErrors).Count `
        -Message 'Release builder has PowerShell parser errors.'
    $lasalInputInventoryFunctions = @($builderAst.FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq 'Get-LmcLasalValidationInputFiles'
        },
        $true))
    Assert-Equal `
        -Expected 1 `
        -Actual $lasalInputInventoryFunctions.Count `
        -Message 'Release builder LASAL input inventory function count drifted.'
    . ([scriptblock]::Create(
        $lasalInputInventoryFunctions[0].Extent.Text))
    $releaseInputFunctions = @($builderAst.FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq 'Get-LmcReleaseInputFiles'
        },
        $true))
    Assert-Equal `
        -Expected 1 `
        -Actual $releaseInputFunctions.Count `
        -Message 'Release builder aggregate input function count drifted.'
    $lasalInventoryCalls = @($releaseInputFunctions[0].FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst] -and
                $node.GetCommandName() -ceq
                    'Get-LmcLasalValidationInputFiles'
        },
        $true))
    Assert-Equal `
        -Expected 1 `
        -Actual $lasalInventoryCalls.Count `
        -Message (
            'Release builder does not bind exactly one LASAL validation ' +
            'inventory into the aggregate fingerprint.')
    Assert-True `
        -Condition ($lasalInventoryCalls[0].Extent.Text -match
            '(?is)-RepositoryRoot\s+\$RepositoryRoot\b') `
        -Message (
            'Release builder LASAL validation inventory is not bound to the ' +
            'selected repository root.')
    $msbuildFunctions = @($builderAst.FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $node.Name -ceq 'Invoke-LmcMSBuild'
        },
        $true))
    Assert-Equal `
        -Expected 1 `
        -Actual $msbuildFunctions.Count `
        -Message 'Release builder MSBuild wrapper count drifted.'
    Assert-True `
        -Condition ($msbuildFunctions[0].Extent.Text -match (
            '(?is)\[string\]\$Platform\s*=\s*''AnyCPU''.*?' +
            '"/p:Platform=\$Platform"')) `
        -Message (
            'Release builder MSBuild wrapper no longer maps its Platform ' +
            'parameter into the MSBuild command line.')
    $builderCommands = @($builderAst.FindAll(
        {
            param($node)
            $node -is [System.Management.Automation.Language.CommandAst]
        },
        $true))
    $transactionCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Invoke-LmcDistributionCandidateTransaction'
        })
    $solutionContractCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Assert-LmcDistributionExampleSolutionContract'
        })
    $solutionBuildCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Invoke-LmcMSBuild' -and
            $_.Extent.Text -match (
                '(?is)-Project\s+' +
                '\$candidateSolutionContract\.SolutionPath\b.*?' +
                "-Target\s+'Rebuild'")
        })
    $runCopyCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Copy-Item' -and
            $_.Extent.Text -match
                '(?is)-LiteralPath\s+\$exampleExe\b.*?-Destination\s+\$runDirectory\b'
        })
    $gateCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Invoke-LmcDistributionExecutableRelaunchGate'
        })
    $semanticCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Test-LmcDistributionSemanticPolicy'
        })
    $manifestCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq 'Write-LmcReleaseManifestAtomic'
        })
    $finalIdentityCommands = @($builderCommands | Where-Object {
            $_.GetCommandName() -ceq
                'Assert-LmcDistributionExecutableRelaunchIdentity'
        })
    foreach ($commandInventory in @(
        [pscustomobject]@{
            Name = 'candidate transaction'
            Commands = $transactionCommands
        },
        [pscustomobject]@{
            Name = 'staged solution contract helper'
            Commands = $solutionContractCommands
        },
        [pscustomobject]@{
            Name = 'Run EXE copy'
            Commands = $runCopyCommands
        },
        [pscustomobject]@{
            Name = 'executable relaunch gate helper'
            Commands = $gateCommands
        },
        [pscustomobject]@{
            Name = 'semantic policy'
            Commands = $semanticCommands
        },
        [pscustomobject]@{
            Name = 'manifest writer'
            Commands = $manifestCommands
        },
        [pscustomobject]@{
            Name = 'final executable identity helper'
            Commands = $finalIdentityCommands
        })) {
        Assert-Equal `
            -Expected 1 `
            -Actual @($commandInventory.Commands).Count `
            -Message (
                "Release builder $($commandInventory.Name) command count drifted.")
    }
    Assert-Equal `
        -Expected 2 `
        -Actual @($solutionBuildCommands).Count `
        -Message 'Release builder exact staged solution build count drifted.'
    $solutionBuildCommands = @($solutionBuildCommands |
        Sort-Object { $_.Extent.StartOffset })
    $transactionCommand = $transactionCommands[0]
    $solutionContractCommand = $solutionContractCommands[0]
    $debugSolutionBuildCommand = $solutionBuildCommands[0]
    $releaseSolutionBuildCommand = $solutionBuildCommands[1]
    $runCopyCommand = $runCopyCommands[0]
    $gateCommand = $gateCommands[0]
    $semanticCommand = $semanticCommands[0]
    $manifestCommand = $manifestCommands[0]
    $finalIdentityCommand = $finalIdentityCommands[0]
    Assert-True `
        -Condition (
            $transactionCommand.Extent.StartOffset -lt
                $solutionContractCommand.Extent.StartOffset -and
            $solutionContractCommand.Extent.EndOffset -lt
                $debugSolutionBuildCommand.Extent.StartOffset -and
            $debugSolutionBuildCommand.Extent.EndOffset -lt
                $releaseSolutionBuildCommand.Extent.StartOffset -and
            $releaseSolutionBuildCommand.Extent.EndOffset -lt
                $runCopyCommand.Extent.StartOffset -and
            $runCopyCommand.Extent.EndOffset -lt
                $gateCommand.Extent.StartOffset -and
            $gateCommand.Extent.EndOffset -lt
                $semanticCommand.Extent.StartOffset -and
            $semanticCommand.Extent.EndOffset -lt
                $manifestCommand.Extent.StartOffset -and
            $manifestCommand.Extent.EndOffset -lt
                $finalIdentityCommand.Extent.StartOffset -and
            $finalIdentityCommand.Extent.EndOffset -lt
                $transactionCommand.Extent.EndOffset) `
        -Message (
            'Release builder order must remain transaction -> solution ' +
            'contract -> Debug solution build -> Release solution build -> ' +
            'Run copy -> EXE gate -> semantic policy -> manifest -> final ' +
            'EXE identity -> transaction completion.')
    Assert-True `
        -Condition ($solutionContractCommand.Extent.Text -match (
            '(?is)-StagingRoot\s+\$stagingRoot\b.*?' +
            '-SolutionPath\s+\$candidateSolution\b.*?' +
            '-ProjectPath\s+\$candidateProject\b')) `
        -Message (
            'Release builder solution helper is not bound to the exact ' +
            'staging root, staged solution, and staged project.')
    Assert-True `
        -Condition ($debugSolutionBuildCommand.Extent.Text -match (
            '(?is)-Project\s+' +
            '\$candidateSolutionContract\.SolutionPath\b.*?' +
            "-Target\s+'Rebuild'.*?-Configuration\s+'Debug'.*?" +
            "-Platform\s+'Any CPU'")) `
        -Message 'Release builder does not rebuild the staged Debug solution.'
    Assert-True `
        -Condition ($releaseSolutionBuildCommand.Extent.Text -match (
            '(?is)-Project\s+' +
            '\$candidateSolutionContract\.SolutionPath\b.*?' +
            "-Target\s+'Rebuild'.*?-Configuration\s+'Release'.*?" +
            "-Platform\s+'Any CPU'")) `
        -Message 'Release builder does not rebuild the staged Release solution.'
    Assert-True `
        -Condition ($gateCommand.Extent.Text -match (
            '(?is)-StagingRoot\s+\$stagingRoot\b.*?' +
            '-ExecutablePath\s+\$runExampleExe\b.*?' +
            '-GateAction\s*\{.*?' +
            "-Target\s+'RunWpfExecutableRelaunchTest'.*?" +
            'WpfExecutableRelaunchExe\s*=\s*\$testedExecutable\b')) `
        -Message (
            'Release builder gate helper no longer invokes the actual-EXE ' +
            'relaunch target with the exact staged Run path.')
    Assert-True `
        -Condition ($finalIdentityCommand.Extent.Text -match (
            '(?is)-StagingRoot\s+\$stagingRoot\b.*?' +
            '-ExecutablePath\s+\$runExampleExe\b.*?' +
            '-TestedSha256\s*\(\s*' +
            '\$buildSummary\.ExecutableRelaunchTestedExeSha256\s*\)')) `
        -Message (
            'Release builder final identity helper is not bound to the exact ' +
            'staged Run path and tested SHA.')

    # The release fingerprint binds every tracked LASAL validation input and
    # every physical Network aggregate input. Source-like untracked files are
    # rejected; build-only artifacts outside that aggregate remain excluded.
    $lasalInventoryRepository = Join-Path $script:TestRoot `
        'lasal-validation-input-repository'
    $lasalProjectRelativeRoot =
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
    $lasalProjectRoot = Join-Path $lasalInventoryRepository `
        $lasalProjectRelativeRoot.Replace('/', '\')
    $lasalTrackedFixtureContent = [ordered]@{
        'Elmo_EtherCAT_Test_4Axis.lcp' = '<Project />'
        'Elmo_EtherCAT_Test_4Axis.lcb' = 'project-database-v1'
        'Class/LMCControlCommandService/LMCControlCommandService.st' =
            'FUNCTION GLOBAL LMCControlCommandService::Run'
        'Class/Classes.lcb' = 'class-database-v1'
        'Class/LMCDiagnosticsService/LMCDiagnosticsService.st' =
            'FUNCTION GLOBAL LMCDiagnosticsService::Run'
        'Network/Networks.lcb' = 'network-database-v1'
        'Network/Comm_Network/Comm_Network.lcn' = '<Network />'
        'Network/ConfigObjects.st' = 'FUNCTION GLOBAL CONFIG_TABLES'
        'Network/Eni.xml' = '<EtherCATConfig />'
        'Network/VerifierInventory.dat' = 'tracked-network-opaque-v1'
        'Include/global.h' = '#define FIXTURE_GLOBAL 1'
        'Source/interfaces/lsl_st_tcp_user.h' =
            '#define FIXTURE_TCP_USER 1'
    }
    Write-TestFile `
        -Path (Join-Path $lasalInventoryRepository '.gitignore') `
        -Content @'
*.lba
*.lob
Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/IgnoredGenerated/
'@
    foreach ($fixtureInput in
        $lasalTrackedFixtureContent.GetEnumerator()) {
        Write-TestFile `
            -Path (Join-Path $lasalProjectRoot `
                $fixtureInput.Key.Replace('/', '\')) `
            -Content $fixtureInput.Value
    }
    $gitOutput = @(& git -C $lasalInventoryRepository init --quiet 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Fixture git init failed: $($gitOutput -join [Environment]::NewLine)"
    }
    $gitOutput = @(& git -c core.autocrlf=false `
        -C $lasalInventoryRepository add -- `
        '.gitignore' $lasalProjectRelativeRoot 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "Fixture git add failed: $($gitOutput -join [Environment]::NewLine)"
    }
    $ignoredBuildOutput = Join-Path $lasalProjectRoot `
        'Class/bin/Elmo_EtherCAT_Test_4Axis.lba'
    $irrelevantUntrackedIcon = Join-Path $lasalProjectRoot `
        'Class/Tool.ico'
    Write-TestFile -Path $ignoredBuildOutput -Content 'ignored-build-output-v1'
    Write-TestFile -Path $irrelevantUntrackedIcon -Content 'untracked-icon-v1'

    $lasalInventoryFiles = @(Get-LmcLasalValidationInputFiles `
        -RepositoryRoot $lasalInventoryRepository)
    $lasalInventoryPrefix =
        [System.IO.Path]::GetFullPath($lasalInventoryRepository).TrimEnd('\') +
        '\'
    $lasalInventoryRelativeFiles = @($lasalInventoryFiles |
        ForEach-Object {
            $_.Substring($lasalInventoryPrefix.Length).Replace('\', '/')
        })
    Assert-Equal `
        -Expected $lasalTrackedFixtureContent.Count `
        -Actual $lasalInventoryRelativeFiles.Count `
        -Message 'LASAL release input inventory count drifted.'
    foreach ($expectedRelativePath in $lasalTrackedFixtureContent.Keys) {
        $expectedRepositoryRelativePath =
            "$lasalProjectRelativeRoot/$expectedRelativePath"
        Assert-True `
            -Condition ($lasalInventoryRelativeFiles -ccontains
                $expectedRepositoryRelativePath) `
            -Message (
                'LASAL release input inventory omitted tracked input ' +
                $expectedRepositoryRelativePath)
    }
    Assert-True `
        -Condition ($lasalInventoryFiles -notcontains $ignoredBuildOutput) `
        -Message 'LASAL release input inventory bound an ignored .lba output.'
    Assert-True `
        -Condition ($lasalInventoryFiles -notcontains $irrelevantUntrackedIcon) `
        -Message 'LASAL release input inventory bound an untracked icon.'

    $lasalFingerprintBeforeIgnoredMutation =
        Get-LasalValidationFixtureFingerprint `
            -RepositoryRoot $lasalInventoryRepository
    Write-TestFile `
        -Path $ignoredBuildOutput `
        -Content 'ignored-build-output-v2'
    Assert-Equal `
        -Expected $lasalFingerprintBeforeIgnoredMutation `
        -Actual (Get-LasalValidationFixtureFingerprint `
            -RepositoryRoot $lasalInventoryRepository) `
        -Message 'An ignored .lba build output changed the release fingerprint.'

    $seededNetworkRelativeOutputs = @(
        'Network/Comm_Network/ONE_Comm_Network_Table.lba',
        'Network/ConfigObjects.lba',
        'Network/ConfigObjects.lob',
        'Network/EtherCAT_Network/ONE_EtherCAT_Network_Table.lba',
        'Network/HW_Network/ONE_HW_Network_Table.lba',
        'Network/HW_Network/ONE_HW_Network_Table.lob',
        'Network/Motion_Network/ONE_Motion_Network_Table.lba',
        'Network/Motion_Network/ONE_Motion_Network_Table.lob')
    $seededNetworkOutputs = @()
    for ($seedIndex = 0;
        $seedIndex -lt $seededNetworkRelativeOutputs.Count;
        $seedIndex++) {
        $seedPath = Join-Path $lasalProjectRoot `
            $seededNetworkRelativeOutputs[$seedIndex].Replace('/', '\')
        Write-TestFile `
            -Path $seedPath `
            -Content "seeded-network-output-$seedIndex"
        $seededNetworkOutputs += $seedPath
    }
    $seededNetworkInventory = @(Get-LmcLasalValidationInputFiles `
        -RepositoryRoot $lasalInventoryRepository)
    Assert-Equal `
        -Expected ($lasalTrackedFixtureContent.Count +
            $seededNetworkOutputs.Count) `
        -Actual $seededNetworkInventory.Count `
        -Message 'Seeded FullNetwork physical input count drifted.'
    foreach ($seededNetworkOutput in $seededNetworkOutputs) {
        Assert-True `
            -Condition ($seededNetworkInventory -contains
                $seededNetworkOutput) `
            -Message (
                'Seeded FullNetwork inventory omitted ignored input ' +
                $seededNetworkOutput)
    }
    $fixture = New-TestFixture -Name 'lasal_network_seeded_input_drift'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $seededMutationOriginalBytes =
        [System.IO.File]::ReadAllBytes($seededNetworkOutputs[0])
    try {
        Assert-Throws `
            -Action {
                Invoke-LmcDistributionCandidateTransaction `
                    -CanonicalRoot $fixture.Canonical `
                    -CandidatePath $fixture.Candidate `
                    -PopulateAndValidate {
                        param($stage)
                        Populate-TestCandidate -Stage $stage
                    } `
                    -GetInputFingerprint {
                        Get-LasalValidationFixtureFingerprint `
                            -RepositoryRoot $lasalInventoryRepository
                    } `
                    -BeforePromotion {
                        param($stage, $candidate)
                        Write-TestFile `
                            -Path $seededNetworkOutputs[0] `
                            -Content 'seeded-network-output-mutated'
                    }
            } `
            -ExpectedMessage (
                'Distribution input fingerprint changed before promotion') |
            Out-Null
    }
    finally {
        [System.IO.File]::WriteAllBytes(
            $seededNetworkOutputs[0],
            $seededMutationOriginalBytes)
    }
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Seeded ignored Network mutation promoted a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Seeded ignored Network mutation'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Seeded ignored Network mutation'
    foreach ($seededNetworkOutput in $seededNetworkOutputs) {
        [System.IO.File]::Delete($seededNetworkOutput)
    }
    Assert-Equal `
        -Expected $lasalFingerprintBeforeIgnoredMutation `
        -Actual (Get-LasalValidationFixtureFingerprint `
            -RepositoryRoot $lasalInventoryRepository) `
        -Message 'Removing seeded Network binaries did not restore pure-Git identity.'

    foreach ($mutation in @(
        [pscustomobject]@{
            Name = 'control_service'
            RelativePath =
                'Class/LMCControlCommandService/LMCControlCommandService.st'
        },
        [pscustomobject]@{
            Name = 'classes_database'
            RelativePath = 'Class/Classes.lcb'
        },
        [pscustomobject]@{
            Name = 'networks_database'
            RelativePath = 'Network/Networks.lcb'
        })) {
        $fixture = New-TestFixture `
            -Name ('lasal_input_drift_' + $mutation.Name)
        $canonicalBefore = Get-LmcDistributionTreeSnapshot `
            -Root $fixture.Canonical
        $mutationPath = Join-Path $lasalProjectRoot `
            $mutation.RelativePath.Replace('/', '\')
        $originalBytes = [System.IO.File]::ReadAllBytes($mutationPath)
        try {
            Assert-Throws `
                -Action {
                    Invoke-LmcDistributionCandidateTransaction `
                        -CanonicalRoot $fixture.Canonical `
                        -CandidatePath $fixture.Candidate `
                        -PopulateAndValidate {
                            param($stage)
                            Populate-TestCandidate -Stage $stage
                        } `
                        -GetInputFingerprint {
                            Get-LasalValidationFixtureFingerprint `
                                -RepositoryRoot $lasalInventoryRepository
                        } `
                        -BeforePromotion {
                            param($stage, $candidate)
                            Write-TestFile `
                                -Path $mutationPath `
                                -Content ('mutated-after-populate-' +
                                    $mutation.Name)
                        }
                } `
                -ExpectedMessage (
                    'Distribution input fingerprint changed before promotion') |
                Out-Null
        }
        finally {
            [System.IO.File]::WriteAllBytes($mutationPath, $originalBytes)
        }
        Assert-True `
            -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
            -Message (
                "$($mutation.Name) input drift promoted a candidate.")
        Assert-CanonicalUnchanged `
            -Before $canonicalBefore `
            -Canonical $fixture.Canonical `
            -Context "$($mutation.Name) input drift"
        Assert-NoTransactionResidue `
            -Fixture $fixture `
            -Context "$($mutation.Name) input drift"
    }

    # A pure-Git Network snapshot has no ignored generated binaries. If one
    # appears after validation, it is a new FullNetwork input and must prevent
    # promotion even though Git still classifies it as ignored.
    $fixture = New-TestFixture -Name 'lasal_network_ignored_input_appeared'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $appearingNetworkOutput = Join-Path $lasalProjectRoot `
        'Network/Comm_Network/ONE_Comm_Network_Table.lba'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $appearingNetworkOutput)) `
        -Message 'Ignored Network appearance fixture did not start absent.'
    try {
        Assert-Throws `
            -Action {
                Invoke-LmcDistributionCandidateTransaction `
                    -CanonicalRoot $fixture.Canonical `
                    -CandidatePath $fixture.Candidate `
                    -PopulateAndValidate {
                        param($stage)
                        Populate-TestCandidate -Stage $stage
                    } `
                    -GetInputFingerprint {
                        Get-LasalValidationFixtureFingerprint `
                            -RepositoryRoot $lasalInventoryRepository
                    } `
                    -BeforePromotion {
                        param($stage, $candidate)
                        Write-TestFile `
                            -Path $appearingNetworkOutput `
                            -Content 'appeared-after-populate'
                    }
            } `
            -ExpectedMessage (
                'Distribution input fingerprint changed before promotion') |
            Out-Null
    }
    finally {
        if (Test-Path -LiteralPath $appearingNetworkOutput -PathType Leaf) {
            [System.IO.File]::Delete($appearingNetworkOutput)
        }
    }
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Ignored Network input appearance promoted a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Ignored Network input appearance'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Ignored Network input appearance'

    $untrackedSourcePath = Join-Path $lasalProjectRoot `
        'Class/TestClass/TestClass.st'
    Write-TestFile `
        -Path $untrackedSourcePath `
        -Content 'FUNCTION GLOBAL TestClass::Run'
    Assert-Throws `
        -Action {
            Get-LmcLasalValidationInputFiles `
                -RepositoryRoot $lasalInventoryRepository
        } `
        -ExpectedMessage 'Class/TestClass/TestClass.st' | Out-Null
    [System.IO.File]::Delete($untrackedSourcePath)

    $ignoredSourcePath = Join-Path $lasalProjectRoot `
        'Class/IgnoredGenerated/IgnoredGenerated.st'
    Write-TestFile `
        -Path $ignoredSourcePath `
        -Content 'FUNCTION GLOBAL IgnoredGenerated::Run'
    Assert-Throws `
        -Action {
            Get-LmcLasalValidationInputFiles `
                -RepositoryRoot $lasalInventoryRepository
        } `
        -ExpectedMessage (
            'Class/IgnoredGenerated/IgnoredGenerated.st') | Out-Null
    [System.IO.File]::Delete($ignoredSourcePath)

    $trackedOpaqueNetworkPath = Join-Path $lasalProjectRoot `
        'Network/VerifierInventory.dat'
    $trackedOpaqueNetworkBytes =
        [System.IO.File]::ReadAllBytes($trackedOpaqueNetworkPath)
    try {
        [System.IO.File]::Delete($trackedOpaqueNetworkPath)
        Assert-Throws `
            -Action {
                Get-LmcLasalValidationInputFiles `
                    -RepositoryRoot $lasalInventoryRepository
            } `
            -ExpectedMessage 'Tracked LASAL release input not found' | Out-Null
    }
    finally {
        [System.IO.File]::WriteAllBytes(
            $trackedOpaqueNetworkPath,
            $trackedOpaqueNetworkBytes)
    }

    $lasalReparseTarget = Join-Path $script:TestRoot `
        'lasal-validation-reparse-target'
    $lasalReparseLink = Join-Path $lasalProjectRoot `
        'Class/ReparseProbe'
    Write-TestFile `
        -Path (Join-Path $lasalReparseTarget 'sentinel.st') `
        -Content 'FUNCTION GLOBAL ReparseSentinel::Run'
    New-Item -ItemType Junction `
        -Path $lasalReparseLink `
        -Target $lasalReparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($lasalReparseLink)
    Assert-Throws `
        -Action {
            Get-LmcLasalValidationInputFiles `
                -RepositoryRoot $lasalInventoryRepository
        } `
        -ExpectedMessage (
            'LASAL validation input tree contains a reparse point') | Out-Null
    [System.IO.Directory]::Delete($lasalReparseLink)
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $lasalReparseTarget 'sentinel.st') -PathType Leaf) `
        -Message 'LASAL input inventory reparse check followed its target.'

    $lasalAncestorRepository = Join-Path $script:TestRoot `
        'lasal-validation-reparse-ancestor-repository'
    $lasalAncestorTarget = Join-Path $script:TestRoot `
        'lasal-validation-reparse-ancestor-target'
    $lasalAncestorLink = Join-Path $lasalAncestorRepository 'Lasal_PRG'
    $lasalAncestorProject = Join-Path $lasalAncestorTarget `
        'Elmo_EtherCAT_Test_4Axis'
    Write-TestFile `
        -Path (Join-Path $lasalAncestorProject 'Class/sentinel.st') `
        -Content 'FUNCTION GLOBAL AncestorSentinel::Run'
    New-Item -ItemType Directory `
        -Path $lasalAncestorRepository -Force | Out-Null
    New-Item -ItemType Junction `
        -Path $lasalAncestorLink `
        -Target $lasalAncestorTarget | Out-Null
    $script:TrackedReparsePaths.Add($lasalAncestorLink)
    Assert-Throws `
        -Action {
            Get-LmcLasalValidationInputFiles `
                -RepositoryRoot $lasalAncestorRepository
        } `
        -ExpectedMessage (
            'LASAL validation input path traverses a reparse point') | Out-Null
    [System.IO.Directory]::Delete($lasalAncestorLink)
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $lasalAncestorProject 'Class/sentinel.st') `
            -PathType Leaf) `
        -Message 'LASAL input ancestry check followed its junction target.'

    # The helper executes only the exact staged Run EXE and binds its bytes.
    $gateRoot = Join-Path $script:TestRoot 'executable-relaunch-helper'
    $gateExecutable = Join-Path $gateRoot (
        '02_Example_Program/Run/LasalMotionControlApiExample.exe')
    Write-TestFile -Path $gateExecutable -Content 'tested-executable-v1'
    $expectedGateHash = (Get-FileHash `
        -LiteralPath $gateExecutable `
        -Algorithm SHA256).Hash.ToUpperInvariant()
    $gateObservation = [pscustomobject]@{
        Calls = 0
        Path = $null
    }
    $testedGateHash = Invoke-LmcDistributionExecutableRelaunchGate `
        -StagingRoot $gateRoot `
        -ExecutablePath $gateExecutable `
        -GateAction {
            param($testedExecutable)
            $gateObservation.Calls += 1
            $gateObservation.Path = $testedExecutable
            'gate-output-must-not-escape-the-helper'
        }
    Assert-Equal `
        -Expected $expectedGateHash `
        -Actual $testedGateHash `
        -Message 'Executable relaunch gate did not return the exact tested SHA.'
    Assert-Equal `
        -Expected 1 `
        -Actual $gateObservation.Calls `
        -Message 'Executable relaunch gate action did not run exactly once.'
    Assert-Equal `
        -Expected ([System.IO.Path]::GetFullPath($gateExecutable)) `
        -Actual $gateObservation.Path `
        -Message 'Executable relaunch gate action received the wrong EXE path.'
    Assert-Equal `
        -Expected $expectedGateHash `
        -Actual (Assert-LmcDistributionExecutableRelaunchIdentity `
            -StagingRoot $gateRoot `
            -ExecutablePath $gateExecutable `
            -TestedSha256 $testedGateHash) `
        -Message 'Final executable identity did not return the exact SHA.'

    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -GateAction {
                    throw 'fixture executable relaunch gate failed'
                }
        } `
        -ExpectedMessage 'fixture executable relaunch gate failed' | Out-Null

    $missingGateRoot = Join-Path $script:TestRoot `
        'executable-relaunch-missing'
    New-Item -ItemType Directory -Path $missingGateRoot -Force | Out-Null
    $missingGateExecutable = Join-Path $missingGateRoot (
        '02_Example_Program/Run/LasalMotionControlApiExample.exe')
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $missingGateRoot `
                -ExecutablePath $missingGateExecutable `
                -GateAction { }
        } `
        -ExpectedMessage 'Executable relaunch gate input was not found' |
        Out-Null

    $wrongGateExecutable = Join-Path $gateRoot (
        '02_Example_Program/Run/WrongExample.exe')
    Write-TestFile -Path $wrongGateExecutable -Content 'wrong-executable'
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $wrongGateExecutable `
                -GateAction { }
        } `
        -ExpectedMessage 'path must be the exact staged Run EXE' | Out-Null

    Write-TestFile -Path $gateExecutable -Content 'mutated-after-gate'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionExecutableRelaunchIdentity `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -TestedSha256 $testedGateHash
        } `
        -ExpectedMessage (
            'final example EXE bytes do not match the executable relaunch gate input') |
        Out-Null

    Write-TestFile -Path $gateExecutable -Content 'tested-executable-v1'
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $gateRoot `
                -ExecutablePath $gateExecutable `
                -GateAction {
                    param($testedExecutable)
                    Write-TestFile `
                        -Path $testedExecutable `
                        -Content 'mutated-during-gate'
                }
        } `
        -ExpectedMessage (
            'staged example EXE changed while the executable relaunch gate ran') |
        Out-Null

    # Snapshot coverage includes file content, hidden files, and empty directories.
    $fixture = New-TestFixture -Name 'snapshot'
    $snapshotBaseline = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $extraEmptyDirectory = Join-Path $fixture.Canonical 'new-empty-directory'
    New-Item -ItemType Directory -Path $extraEmptyDirectory | Out-Null
    $snapshotWithDirectory = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($snapshotBaseline.Sha256 -ne $snapshotWithDirectory.Sha256) `
        -Message 'An empty directory did not change the tree snapshot.'
    [System.IO.Directory]::Delete($extraEmptyDirectory)
    Assert-CanonicalUnchanged `
        -Before $snapshotBaseline `
        -Canonical $fixture.Canonical `
        -Context 'Snapshot directory restoration'

    $hiddenPath = Join-Path $fixture.Canonical '.hidden-fixture'
    Write-TestFile -Path $hiddenPath -Content 'hidden-canonical-input-mutated'
    $snapshotWithHiddenChange = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($snapshotBaseline.Sha256 -ne $snapshotWithHiddenChange.Sha256) `
        -Message 'A hidden file change did not change the tree snapshot.'
    Write-TestFile -Path $hiddenPath -Content 'hidden-canonical-input'
    Assert-CanonicalUnchanged `
        -Before $snapshotBaseline `
        -Canonical $fixture.Canonical `
        -Context 'Snapshot hidden-file restoration'

    # Successful promotion seals a staging tree, renames once, and preserves canonical.
    $fixture = New-TestFixture -Name 'success'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $fingerprintState = [pscustomobject]@{
        Value = 'input-v1'
        Calls = 0
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PopulateAndValidate {
            param($stage)
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint {
            $fingerprintState.Calls += 1
            $fingerprintState.Value
        }
    Assert-True `
        -Condition $result.Committed `
        -Message 'Successful transaction did not report Committed=True.'
    Assert-True `
        -Condition (Test-Path -LiteralPath $fixture.Candidate -PathType Container) `
        -Message 'Successful transaction did not publish the candidate directory.'
    Assert-Equal `
        -Expected 2 `
        -Actual $fingerprintState.Calls `
        -Message 'Successful transaction did not check the input fingerprint twice.'
    $publishedSnapshot = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Candidate
    Assert-Equal `
        -Expected $result.CandidateSnapshotSha256 `
        -Actual $publishedSnapshot.Sha256 `
        -Message 'Published candidate does not match its sealed snapshot.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Successful transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Successful transaction'

    # Prepared inputs bind the transaction baseline and populate callback.
    $fixture = New-TestFixture -Name 'prepared_inputs'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $preparedFixtureInput = [pscustomobject]@{ Value = 'prepared-input-v1' }
    $preparedObservation = [pscustomobject]@{
        Baseline = $null
        PreparedValue = $null
        ProviderCalls = 0
        ValidationCalls = 0
        ValidationStage = $null
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PrepareInputs { $preparedFixtureInput } `
        -PopulateAndValidate {
            param($stage, $inputBaseline, $preparedInput)
            $preparedObservation.Baseline = $inputBaseline
            $preparedObservation.PreparedValue = $preparedInput.Value
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint {
            param($preparedInput)
            $preparedObservation.ProviderCalls += 1
            if ($null -eq $preparedInput) {
                return 'prepared-input-v1'
            }
            return $preparedInput.Value
        } `
        -ValidatePreparedInputs {
            param($preparedInput, $stage)
            $preparedObservation.ValidationCalls += 1
            $preparedObservation.ValidationStage = $stage
            if ($preparedInput.Value -ne 'prepared-input-v1') {
                throw 'prepared input changed before validation'
            }
        }
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $preparedObservation.Baseline `
        -Message 'Populate callback did not receive the transaction baseline.'
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $preparedObservation.PreparedValue `
        -Message 'Populate callback did not receive the prepared input.'
    Assert-Equal `
        -Expected 'prepared-input-v1' `
        -Actual $result.InputFingerprint `
        -Message 'Transaction result did not preserve the prepared baseline.'
    Assert-Equal `
        -Expected 2 `
        -Actual $preparedObservation.ProviderCalls `
        -Message 'Prepared transaction did not fingerprint baseline and live inputs.'
    Assert-Equal `
        -Expected 1 `
        -Actual $preparedObservation.ValidationCalls `
        -Message 'Prepared transaction did not validate metadata once.'
    Assert-True `
        -Condition (-not [string]::IsNullOrWhiteSpace(
            $preparedObservation.ValidationStage)) `
        -Message 'Prepared metadata validation did not receive the stage.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Prepared-input transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Prepared-input transaction'

    $fixture = New-TestFixture -Name 'prepared_validation_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PrepareInputs {
                    [pscustomobject]@{ Value = 'prepared-input-v1' }
                } `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'prepared-input-v1' } `
                -ValidatePreparedInputs {
                    throw 'prepared metadata changed'
                }
        } `
        -ExpectedMessage 'prepared metadata changed' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Prepared metadata failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Prepared metadata failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Prepared metadata failure'

    # A validation callback failure removes only staging and keeps canonical exact.
    $fixture = New-TestFixture -Name 'callback_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    throw 'fixture populate failure'
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage 'fixture populate failure' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Callback failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Callback failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Callback failure'

    # An actual-EXE gate failure remains inside PopulateAndValidate, so the
    # transaction removes staging without publishing or touching canonical.
    $fixture = New-TestFixture -Name 'executable_gate_failure'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    $stageExecutable = Join-Path $stage (
                        '02_Example_Program/Run/' +
                        'LasalMotionControlApiExample.exe')
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-gate-executable'
                    Invoke-LmcDistributionExecutableRelaunchGate `
                        -StagingRoot $stage `
                        -ExecutablePath $stageExecutable `
                        -GateAction {
                            throw 'transaction executable gate failed'
                        } | Out-Null
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage 'transaction executable gate failed' | Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Executable gate failure published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Executable gate failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Executable gate failure'

    # A mutation after a successful gate must fail the final identity check
    # before the candidate transaction can seal or promote the stage.
    $fixture = New-TestFixture -Name 'executable_identity_mutation'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                    $stageExecutable = Join-Path $stage (
                        '02_Example_Program/Run/' +
                        'LasalMotionControlApiExample.exe')
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-tested-executable'
                    $transactionTestedSha =
                        Invoke-LmcDistributionExecutableRelaunchGate `
                            -StagingRoot $stage `
                            -ExecutablePath $stageExecutable `
                            -GateAction { }
                    Write-TestFile `
                        -Path $stageExecutable `
                        -Content 'transaction-mutated-executable'
                    Assert-LmcDistributionExecutableRelaunchIdentity `
                        -StagingRoot $stage `
                        -ExecutablePath $stageExecutable `
                        -TestedSha256 $transactionTestedSha | Out-Null
                } `
                -GetInputFingerprint { 'input-v1' }
        } `
        -ExpectedMessage (
            'final example EXE bytes do not match the executable relaunch gate input') |
        Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Executable identity mutation published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Executable identity mutation'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Executable identity mutation'

    # Candidate bytes changed after validation must fail before Directory.Move.
    $fixture = New-TestFixture -Name 'tamper'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    Write-TestFile `
                        -Path (Join-Path $stage 'tampered-after-seal.txt') `
                        -Content 'tamper'
                }
        } `
        -ExpectedMessage 'Candidate staging tree after validation changed' |
        Out-Null
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Tampered staging tree was promoted.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Candidate tamper failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Candidate tamper failure'

    # Inputs are fingerprinted before population and again before promotion.
    $fixture = New-TestFixture -Name 'input_drift'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $fingerprintState = [pscustomobject]@{
        Value = 'input-v1'
        Calls = 0
    }
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint {
                    $fingerprintState.Calls += 1
                    $fingerprintState.Value
                } `
                -BeforePromotion {
                    param($stage, $candidate)
                    $fingerprintState.Value = 'input-v2'
                }
        } `
        -ExpectedMessage 'Distribution input fingerprint changed before promotion' |
        Out-Null
    Assert-Equal `
        -Expected 2 `
        -Actual $fingerprintState.Calls `
        -Message 'Input drift test did not execute both fingerprint checks.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Input drift promoted a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Input drift failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Input drift failure'

    # A target that appears during the transaction must never be replaced.
    $fixture = New-TestFixture -Name 'occupied_target'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    New-Item -ItemType Directory -Path $candidate | Out-Null
                    Write-TestFile `
                        -Path (Join-Path $candidate 'sentinel.txt') `
                        -Content 'external-owner'
                }
        } `
        -ExpectedMessage 'CandidatePath must not already exist before promotion' |
        Out-Null
    Assert-Equal `
        -Expected 'external-owner' `
        -Actual ([System.Text.Encoding]::ASCII.GetString(
            [System.IO.File]::ReadAllBytes(
                (Join-Path $fixture.Candidate 'sentinel.txt')))) `
        -Message 'Occupied candidate target was overwritten.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Occupied target failure'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Occupied target failure'

    # Canonical mutation is detected before promotion. The pipeline does not
    # attempt a risky automatic rollback of external canonical writes.
    $fixture = New-TestFixture -Name 'canonical_mutation'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $canonicalReadme = Join-Path $fixture.Canonical 'README.md'
    $canonicalReadmeBytes = [System.IO.File]::ReadAllBytes($canonicalReadme)
    Assert-Throws `
        -Action {
            Invoke-LmcDistributionCandidateTransaction `
                -CanonicalRoot $fixture.Canonical `
                -CandidatePath $fixture.Candidate `
                -PopulateAndValidate {
                    param($stage)
                    Populate-TestCandidate -Stage $stage
                } `
                -GetInputFingerprint { 'input-v1' } `
                -BeforePromotion {
                    param($stage, $candidate)
                    Write-TestFile `
                        -Path $canonicalReadme `
                        -Content 'external-canonical-mutation'
                }
        } `
        -ExpectedMessage 'Canonical distribution before promotion changed' |
        Out-Null
    $canonicalAfterMutation = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    Assert-True `
        -Condition ($canonicalAfterMutation.Sha256 -ne $canonicalBefore.Sha256) `
        -Message 'Canonical mutation fixture did not actually change canonical.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $fixture.Candidate)) `
        -Message 'Canonical mutation promoted a candidate.'
    [System.IO.File]::WriteAllBytes($canonicalReadme, $canonicalReadmeBytes)
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Canonical mutation fixture restoration'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Canonical mutation failure'

    # A nested contender proves FileShare.None exclusion without a child
    # process, polling, or an unbounded wait.
    $fixture = New-TestFixture -Name 'concurrency_lock'
    $canonicalBefore = Get-LmcDistributionTreeSnapshot `
        -Root $fixture.Canonical
    $nestedCandidate = Join-Path $fixture.Parent `
        'LMC_API_Distribution_candidate_concurrency_nested'
    $lockObservation = [pscustomobject]@{
        Message = $null
    }
    $result = Invoke-LmcDistributionCandidateTransaction `
        -CanonicalRoot $fixture.Canonical `
        -CandidatePath $fixture.Candidate `
        -PopulateAndValidate {
            param($stage)
            try {
                Invoke-LmcDistributionCandidateTransaction `
                    -CanonicalRoot $fixture.Canonical `
                    -CandidatePath $nestedCandidate `
                    -PopulateAndValidate {
                        param($nestedStage)
                        Populate-TestCandidate -Stage $nestedStage
                    } `
                    -GetInputFingerprint { 'nested-input-v1' } |
                    Out-Null
            }
            catch {
                $lockObservation.Message = $_.Exception.Message
            }
            Populate-TestCandidate -Stage $stage
        } `
        -GetInputFingerprint { 'outer-input-v1' }
    Assert-True `
        -Condition $result.Committed `
        -Message 'Outer concurrency transaction did not complete.'
    Assert-True `
        -Condition ($null -ne $lockObservation.Message -and
            $lockObservation.Message.IndexOf(
                'exclusive distribution transaction lock',
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0) `
        -Message 'Nested transaction was not rejected by the exclusive lock.'
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $nestedCandidate)) `
        -Message 'Nested lock contender published a candidate.'
    Assert-CanonicalUnchanged `
        -Before $canonicalBefore `
        -Canonical $fixture.Canonical `
        -Context 'Concurrency transaction'
    Assert-NoTransactionResidue `
        -Fixture $fixture `
        -Context 'Concurrency transaction'

    # Cleanup accepts only a direct child with the exact generated stage name.
    $fixture = New-TestFixture -Name 'cleanup_safety'
    $unexpectedName = Join-Path $fixture.Parent 'not-a-stage'
    New-Item -ItemType Directory -Path $unexpectedName | Out-Null
    Write-TestFile `
        -Path (Join-Path $unexpectedName 'sentinel.txt') `
        -Content 'must-survive'
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $unexpectedName `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'unexpected directory name' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $unexpectedName 'sentinel.txt') -PathType Leaf) `
        -Message 'Cleanup removed an unexpected-name directory.'

    $outsideParent = Join-Path $script:TestRoot 'outside-cleanup-parent'
    New-Item -ItemType Directory -Path $outsideParent | Out-Null
    $outsideStage = Join-Path $outsideParent (
        '.LMC_API_Distribution.stage.' +
        [System.Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $outsideStage | Out-Null
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $outsideStage `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'outside the expected parent' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath $outsideStage -PathType Container) `
        -Message 'Cleanup removed a stage outside the expected parent.'

    $reparseStage = Join-Path $fixture.Parent (
        '.LMC_API_Distribution.stage.' +
        [System.Guid]::NewGuid().ToString('N'))
    $reparseTarget = Join-Path $fixture.Parent 'reparse-target'
    $reparseLink = Join-Path $reparseStage 'linked-directory'
    New-Item -ItemType Directory -Path $reparseStage | Out-Null
    New-Item -ItemType Directory -Path $reparseTarget | Out-Null
    Write-TestFile `
        -Path (Join-Path $reparseTarget 'sentinel.txt') `
        -Content 'reparse-target-must-survive'
    New-Item -ItemType Junction `
        -Path $reparseLink `
        -Target $reparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($reparseLink)
    Assert-Throws `
        -Action {
            Remove-LmcDistributionStagingDirectory `
                -StagingPath $reparseStage `
                -ExpectedParent $fixture.Parent
        } `
        -ExpectedMessage 'contains a reparse point' | Out-Null
    Assert-True `
        -Condition (Test-Path -LiteralPath (
            Join-Path $reparseTarget 'sentinel.txt') -PathType Leaf) `
        -Message 'Cleanup followed a reparse point into its target.'
    [System.IO.Directory]::Delete($reparseLink)
    Remove-LmcDistributionStagingDirectory `
        -StagingPath $reparseStage `
        -ExpectedParent $fixture.Parent
    Assert-True `
        -Condition (-not (Test-Path -LiteralPath $reparseStage)) `
        -Message 'Verified safe staging cleanup did not remove the stage.'
    Assert-CanonicalUnchanged `
        -Before (Get-LmcDistributionTreeSnapshot -Root $fixture.Canonical) `
        -Canonical $fixture.Canonical `
        -Context 'Cleanup safety checks'

    Write-Host "PASS: $script:Passed distribution pipeline assertions"
}
finally {
    Remove-TestRootSafely -Path $script:TestRoot
}

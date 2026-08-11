[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$implementation = Join-Path $PSScriptRoot 'ReleaseManifest.ps1'
if (-not (Test-Path -LiteralPath $implementation -PathType Leaf)) {
    throw "Release-manifest implementation not found: $implementation"
}
. $implementation

$script:Passed = 0

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
}

function Write-TestBytes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes
    )

    $parent = Split-Path -Parent $Path
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    [System.IO.File]::WriteAllBytes($Path, $Bytes)
}

function Invoke-FixtureGit {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = @(& git -C $fixtureRoot @Arguments)
    if ($LASTEXITCODE -ne 0) {
        throw "Fixture Git command failed: git $($Arguments -join ' ')"
    }
    return @($output)
}

$systemTemp = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::GetTempPath()).TrimEnd('\')
$fixtureRoot = Join-Path $systemTemp (
    'LmcReleaseManifestTest-' + [Guid]::NewGuid().ToString('N'))
$distribution = Join-Path $fixtureRoot 'package'
$canonicalDll = Join-Path $fixtureRoot 'canonical.dll'
$dllBytes = [System.Text.Encoding]::ASCII.GetBytes(
    'canonical-release-dll-fixture')
$dllReplicaPaths = @(
    '01_API/LasalMotionControlLib.dll',
    '02_Example_Program/Run/LasalMotionControlLib.dll'
)

try {
    New-Item -ItemType Directory -Path $distribution | Out-Null
    Write-TestBytes -Path $canonicalDll -Bytes $dllBytes
    Write-TestBytes `
        -Path (Join-Path $distribution '01_API/LasalMotionControlLib.dll') `
        -Bytes $dllBytes
    Write-TestBytes `
        -Path (Join-Path $distribution '02_Example_Program/Run/LasalMotionControlLib.dll') `
        -Bytes $dllBytes
    Write-TestBytes `
        -Path (Join-Path $distribution '02_Example_Program/Run/LasalMotionControlApiExample.exe') `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('example-exe'))
    Write-TestBytes `
        -Path (Join-Path $distribution '03_API_User_Manual/manual.pdf') `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('manual-pdf'))
    Write-TestBytes `
        -Path (Join-Path $distribution '03_API_User_Manual/manual.docx') `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('manual-docx'))
    Write-TestBytes `
        -Path (Join-Path $distribution 'README.md') `
        -Bytes ([System.Text.Encoding]::UTF8.GetBytes('fixture readme'))

    Invoke-FixtureGit -Arguments @('init', '--quiet') | Out-Null
    Invoke-FixtureGit -Arguments @(
        'config', 'user.email', 'manifest-test@example.invalid') | Out-Null
    Invoke-FixtureGit -Arguments @(
        'config', 'user.name', 'Release Manifest Test') | Out-Null
    Invoke-FixtureGit -Arguments @('config', 'core.autocrlf', 'false') | Out-Null
    Invoke-FixtureGit -Arguments @('add', '--all') | Out-Null
    Invoke-FixtureGit -Arguments @(
        'commit', '--quiet', '-m', 'fixture baseline') | Out-Null
    $commitOutput = @(Invoke-FixtureGit -Arguments @('rev-parse', 'HEAD'))
    $sourceCommit = ([string]$commitOutput[0]).Trim()

    $parameters = @{
        DistributionRoot = $distribution
        CanonicalDllPath = $canonicalDll
        DllReplicaRelativePaths = $dllReplicaPaths
        SourceCommit = $sourceCommit
        WorktreeState = 'clean'
        InputTreeSha256 = ('A1' * 32)
        SemanticPolicySha256 = ('B2' * 32)
        SemanticPolicyResult = 'PASS'
        AssemblyVersion = '0.9.1.0'
        FileVersion = '0.9.1.0'
        ProductVersion = '0.9.1-preview'
    }
    $fixtureManifestGitPath = 'package/RELEASE_MANIFEST.md'

    foreach ($functionName in @(
        'Get-LmcReleaseManifestContent',
        'Test-LmcReleaseManifest',
        'Write-LmcReleaseManifestAtomic')) {
        $command = Get-Command -Name $functionName -CommandType Function
        foreach ($parameterName in @(
            'InputTreeSha256',
            'SemanticPolicySha256',
            'SemanticPolicyResult')) {
            $parameter = $command.Parameters[$parameterName]
            $mandatoryAttributes = @(
                $parameter.Attributes |
                    Where-Object {
                        $_ -is [System.Management.Automation.ParameterAttribute] -and
                        $_.Mandatory
                    })
            Assert-True `
                -Condition ($null -ne $parameter -and
                    $mandatoryAttributes.Count -eq 1) `
                -Message "$functionName parameter $parameterName is not mandatory."
        }
    }

    $manifestPath = Write-LmcReleaseManifestAtomic @parameters
    Assert-True `
        -Condition (Test-Path -LiteralPath $manifestPath -PathType Leaf) `
        -Message 'Atomic writer did not create RELEASE_MANIFEST.md.'
    Assert-True `
        -Condition (Test-LmcReleaseManifest @parameters) `
        -Message 'Immediate manifest verification did not pass.'

    $firstBytes = [System.IO.File]::ReadAllBytes($manifestPath)
    $postFirstBuildStatus = @(Invoke-FixtureGit -Arguments @(
        'status', '--porcelain=v1', '--untracked-files=all'))
    Assert-True `
        -Condition ($postFirstBuildStatus.Count -eq 1 -and
            $postFirstBuildStatus[0] -eq "?? $fixtureManifestGitPath") `
        -Message 'First clean build did not produce only the untracked manifest output.'
    $secondBuildInputStatus = @(Get-LmcReleaseManifestInputGitStatus `
        -GitStatus $postFirstBuildStatus `
        -ManifestRepositoryRelativePath $fixtureManifestGitPath)
    Assert-True `
        -Condition ($secondBuildInputStatus.Count -eq 0) `
        -Message 'The first manifest output was treated as a second-build input change.'
    $secondBuildState = Get-LmcReleaseManifestWorktreeState `
        -GitStatus $secondBuildInputStatus `
        -AllowDirty:$false
    Assert-True `
        -Condition ($secondBuildState -eq 'clean') `
        -Message 'Same-input second build was not classified clean.'
    $parameters.WorktreeState = $secondBuildState
    Write-LmcReleaseManifestAtomic @parameters | Out-Null
    $secondBytes = [System.IO.File]::ReadAllBytes($manifestPath)
    Assert-True `
        -Condition ([System.Convert]::ToBase64String($firstBytes) -eq
            [System.Convert]::ToBase64String($secondBytes)) `
        -Message 'Same-input second clean build was not deterministic.'
    Assert-True `
        -Condition (@(
            Get-ChildItem -LiteralPath $distribution -File -Force |
                Where-Object {
                    $_.Name -like '.RELEASE_MANIFEST.md.*.tmp' -or
                    $_.Name -like '.RELEASE_MANIFEST.md.*.bak'
                }
        ).Count -eq 0) `
        -Message 'Atomic writer left a temporary manifest file behind.'

    $manifestText = [System.IO.File]::ReadAllText($manifestPath)
    foreach ($requiredText in @(
        'Manifest schema: `2`',
        "Source commit: ``$sourceCommit``",
        'Worktree state: `clean`',
        ('Release input tree SHA-256: `' + ('A1' * 32) + '`'),
        ('Semantic policy SHA-256: `' + ('B2' * 32) + '`'),
        'Semantic policy result: `PASS`',
        'Assembly version: `0.9.1.0`',
        'File version: `0.9.1.0`',
        'Product version: `0.9.1-preview`',
        'DLL replica count: `3`',
        'DLL replica identity: `PASS`',
        '`README.md`',
        '`01_API/LasalMotionControlLib.dll`',
        '`02_Example_Program/Run/LasalMotionControlLib.dll`')) {
        Assert-True `
            -Condition $manifestText.Contains($requiredText) `
            -Message "Manifest is missing required text: $requiredText"
    }
    Assert-True `
        -Condition (-not $manifestText.Contains($fixtureRoot)) `
        -Message 'Manifest leaked the absolute fixture path.'

    Assert-True `
        -Condition ((Get-LmcReleaseManifestWorktreeState `
            -GitStatus @() `
            -AllowDirty:$false) -eq 'clean') `
        -Message 'A clean worktree did not map to clean.'
    Assert-True `
        -Condition ((Get-LmcReleaseManifestWorktreeState `
            -GitStatus ' M source.cs' `
            -AllowDirty) -eq 'dirty-preview') `
        -Message 'An allowed dirty worktree did not map to dirty-preview.'
    Assert-Throws `
        -Action {
            Get-LmcReleaseManifestWorktreeState `
                -GitStatus ' M source.cs' `
                -AllowDirty:$false
        } `
        -ExpectedMessage 'worktree is dirty'

    $invalidInputTreeParameters = $parameters.Clone()
    $invalidInputTreeParameters.InputTreeSha256 = 'not-a-sha256'
    Assert-Throws `
        -Action {
            Write-LmcReleaseManifestAtomic @invalidInputTreeParameters
        } `
        -ExpectedMessage 'Release input tree SHA-256 must be an exact 64-character hexadecimal value'

    $invalidSemanticPolicyHashParameters = $parameters.Clone()
    $invalidSemanticPolicyHashParameters.SemanticPolicySha256 = ('G0' * 32)
    Assert-Throws `
        -Action {
            Write-LmcReleaseManifestAtomic @invalidSemanticPolicyHashParameters
        } `
        -ExpectedMessage 'Semantic policy SHA-256 must be an exact 64-character hexadecimal value'

    $invalidSemanticPolicyResultParameters = $parameters.Clone()
    $invalidSemanticPolicyResultParameters.SemanticPolicyResult = 'FAIL'
    Assert-Throws `
        -Action {
            Write-LmcReleaseManifestAtomic @invalidSemanticPolicyResultParameters
        } `
        -ExpectedMessage 'SemanticPolicyResult'

    $lowercaseSemanticPolicyResultParameters = $parameters.Clone()
    $lowercaseSemanticPolicyResultParameters.SemanticPolicyResult = 'pass'
    Assert-Throws `
        -Action {
            Write-LmcReleaseManifestAtomic @lowercaseSemanticPolicyResultParameters
        } `
        -ExpectedMessage 'Semantic policy result must be exactly PASS'

    $generatedGuid = '0123456789abcdef0123456789abcdef'
    $generatedOnlyStatus = @(
        " M $fixtureManifestGitPath",
        "?? package/.RELEASE_MANIFEST.md.$generatedGuid.tmp",
        "?? package/.RELEASE_MANIFEST.md.$generatedGuid.bak"
    )
    $generatedOnlyInput = @(Get-LmcReleaseManifestInputGitStatus `
        -GitStatus $generatedOnlyStatus `
        -ManifestRepositoryRelativePath $fixtureManifestGitPath)
    Assert-True `
        -Condition ($generatedOnlyInput.Count -eq 0) `
        -Message 'Exact manifest/temp/backup outputs were not excluded.'

    $otherDistributionStatus = " M package/README.md"
    $mixedInput = @(Get-LmcReleaseManifestInputGitStatus `
        -GitStatus @($generatedOnlyStatus + $otherDistributionStatus) `
        -ManifestRepositoryRelativePath $fixtureManifestGitPath)
    Assert-True `
        -Condition ($mixedInput.Count -eq 1 -and
            $mixedInput[0] -eq $otherDistributionStatus) `
        -Message 'A non-manifest Distribution change was incorrectly excluded.'
    Assert-Throws `
        -Action {
            Get-LmcReleaseManifestWorktreeState `
                -GitStatus $mixedInput `
                -AllowDirty:$false
        } `
        -ExpectedMessage 'worktree is dirty'
    Assert-True `
        -Condition ((Get-LmcReleaseManifestWorktreeState `
            -GitStatus $mixedInput `
            -AllowDirty) -eq 'dirty-preview') `
        -Message 'A real Distribution input change did not remain dirty-preview.'

    $nearMatchStatus = @(
        "?? $fixtureManifestGitPath.old",
        '?? package/.RELEASE_MANIFEST.md.deadbeef.bak',
        "R  package/README.md -> $fixtureManifestGitPath",
        "UU $fixtureManifestGitPath"
    )
    $nearMatchInput = @(Get-LmcReleaseManifestInputGitStatus `
        -GitStatus $nearMatchStatus `
        -ManifestRepositoryRelativePath $fixtureManifestGitPath)
    Assert-True `
        -Condition ($nearMatchInput.Count -eq $nearMatchStatus.Count) `
        -Message 'A near-match, rename, or unmerged manifest path was incorrectly excluded.'

    $buildScriptText = [System.IO.File]::ReadAllText(
        (Join-Path $PSScriptRoot 'Build-LmcApiDistribution.ps1'))
    foreach ($requiredBuildContract in @(
        'Get-LmcReleaseManifestInputGitStatus',
        "'LMC_Library/LMC_API_Distribution/RELEASE_MANIFEST.md'",
        '--porcelain=v1 --untracked-files=all',
        '-GitStatus $releaseInputGitStatus')) {
        Assert-True `
            -Condition $buildScriptText.Contains($requiredBuildContract) `
            -Message "Build script is missing status-filter contract: $requiredBuildContract"
    }

    $duplicateParameters = $parameters.Clone()
    $duplicateParameters.DllReplicaRelativePaths = @(
        $dllReplicaPaths[0],
        $dllReplicaPaths[0]
    )
    Assert-Throws `
        -Action { Write-LmcReleaseManifestAtomic @duplicateParameters } `
        -ExpectedMessage 'three distinct replica files'

    $stableManifest = [System.IO.File]::ReadAllBytes($manifestPath)
    Write-TestBytes `
        -Path $canonicalDll `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('different-dll'))
    Assert-Throws `
        -Action { Write-LmcReleaseManifestAtomic @parameters } `
        -ExpectedMessage 'DLL replicas are not byte-identical'
    Assert-True `
        -Condition ([System.Convert]::ToBase64String($stableManifest) -eq
            [System.Convert]::ToBase64String(
                [System.IO.File]::ReadAllBytes($manifestPath))) `
        -Message 'A failed generation changed the previously valid manifest.'
    Write-TestBytes -Path $canonicalDll -Bytes $dllBytes

    $readmePath = Join-Path $distribution 'README.md'
    $readmeBytes = [System.IO.File]::ReadAllBytes($readmePath)
    Write-TestBytes `
        -Path $readmePath `
        -Bytes ([System.Text.Encoding]::UTF8.GetBytes('tampered readme'))
    Assert-Throws `
        -Action { Test-LmcReleaseManifest @parameters } `
        -ExpectedMessage 'does not match the current package artifacts'
    Write-TestBytes -Path $readmePath -Bytes $readmeBytes

    [System.IO.File]::AppendAllText(
        $manifestPath,
        "Host path: C:\work\private`r`n")
    Assert-Throws `
        -Action { Test-LmcReleaseManifest @parameters } `
        -ExpectedMessage 'absolute Windows path'
    Write-LmcReleaseManifestAtomic @parameters | Out-Null

    $internalLeak = Join-Path $distribution '01_API/LMC_API_Delivery.txt'
    Write-TestBytes `
        -Path $internalLeak `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('leak'))
    Assert-Throws `
        -Action { Write-LmcReleaseManifestAtomic @parameters } `
        -ExpectedMessage 'internal delivery source path'
    Remove-Item -LiteralPath $internalLeak -Force

    $staleBackup = Join-Path $distribution `
        '.RELEASE_MANIFEST.md.deadbeef.bak'
    Write-TestBytes `
        -Path $staleBackup `
        -Bytes ([System.Text.Encoding]::ASCII.GetBytes('stale'))
    Assert-Throws `
        -Action { Write-LmcReleaseManifestAtomic @parameters } `
        -ExpectedMessage 'Stale release-manifest temporary file'
    Remove-Item -LiteralPath $staleBackup -Force

    $parameters.WorktreeState = 'dirty-preview'
    Write-LmcReleaseManifestAtomic @parameters | Out-Null
    $dirtyText = [System.IO.File]::ReadAllText($manifestPath)
    Assert-True `
        -Condition $dirtyText.Contains('Worktree state: `dirty-preview`') `
        -Message 'Dirty preview state was not preserved in the manifest.'
    Assert-True `
        -Condition (Test-LmcReleaseManifest @parameters) `
        -Message 'Dirty preview manifest did not verify.'

    Write-Output "TOTAL $script:Passed, PASSED $script:Passed, FAILED 0"
}
finally {
    $resolvedFixture = [System.IO.Path]::GetFullPath($fixtureRoot)
    $tempPrefix = $systemTemp + [System.IO.Path]::DirectorySeparatorChar
    if ($resolvedFixture.StartsWith(
        $tempPrefix,
        [System.StringComparison]::OrdinalIgnoreCase) -and
        ([System.IO.Path]::GetFileName($resolvedFixture) -like
            'LmcReleaseManifestTest-*')) {
        if (Test-Path -LiteralPath $resolvedFixture) {
            Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
        }
    }
    else {
        throw "Refusing to remove unexpected fixture path: $resolvedFixture"
    }
}

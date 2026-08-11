[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$implementation = Join-Path $PSScriptRoot `
    'DistributionToolchainProvenance.ps1'
if (-not (Test-Path -LiteralPath $implementation -PathType Leaf)) {
    throw "Distribution toolchain provenance implementation not found: $implementation"
}
. $implementation

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
    $script:Passed++
}

function Assert-Equal {
    param(
        [AllowNull()][object]$Expected,
        [AllowNull()][object]$Actual,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not [object]::Equals($Expected, $Actual)) {
        throw "$Message expected='$Expected' actual='$Actual'"
    }
    $script:Passed++
}

function Assert-Throws {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string]$ExpectedMessage
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
    $script:Passed++
}

function Write-FixtureFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )

    $parent = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    [System.IO.File]::WriteAllBytes(
        $Path,
        [System.Text.Encoding]::ASCII.GetBytes($Content))
}

function New-FixtureAttestation {
    param(
        [string]$Result = 'PASS',
        [int]$SuiteCount = 7,
        [int]$RunCount = 14
    )

    $physicalHost = [System.Diagnostics.Process]::GetCurrentProcess().
        MainModule.FileName
    $physicalHostSha256 = (Get-FileHash `
        -LiteralPath $physicalHost `
        -Algorithm SHA256).Hash.ToUpperInvariant()
    $currentVersion = [string]$PSVersionTable.PSVersion
    $ps5Version = if ($PSVersionTable.PSEdition -ceq 'Desktop') {
        $currentVersion
    }
    else {
        '5.1.19041.5608'
    }
    $ps7Version = if ($PSVersionTable.PSEdition -ceq 'Core') {
        $currentVersion
    }
    else {
        '7.6.4'
    }
    return [pscustomobject]@{
        Result = $Result
        HostCount = 2
        SuiteCount = $SuiteCount
        RunCount = $RunCount
        ToolingDigest = ('A1' * 32)
        ToolingFileCount = 94
        Hosts = @(
            [pscustomobject]@{
                Label = 'PS5'
                Edition = 'Desktop'
                Major = 5
                Version = $ps5Version
                Path = $physicalHost
                ExecutableSha256 = $physicalHostSha256
            },
            [pscustomobject]@{
                Label = 'PS7'
                Edition = 'Core'
                Major = 7
                Version = $ps7Version
                Path = $physicalHost
                ExecutableSha256 = $physicalHostSha256
            })
    }
}

function New-FixtureDescriptors {
    param([Parameter(Mandatory = $true)][string]$Root)

    $roles = @(
        'CSharpCompiler', 'Git', 'MSBuild', 'PowerShell',
        'PyPdf', 'Python', 'PythonDocx', 'VsWhere')
    $descriptors = @()
    $index = 0
    foreach ($role in $roles) {
        $path = Join-Path $Root ("$role.bin")
        Write-FixtureFile -Path $path -Content ("fixture-$role-v1")
        $descriptors += [pscustomobject]@{
            Role = $role
            Version = "1.0.$index"
            Path = $path
        }
        $index++
    }
    return @($descriptors)
}

function Remove-FixtureRoot {
    param([Parameter(Mandatory = $true)][string]$Path)

    foreach ($reparsePath in @($script:TrackedReparsePaths)) {
        if (Test-Path -LiteralPath $reparsePath) {
            [System.IO.Directory]::Delete($reparsePath)
        }
    }
    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    $temp = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::GetTempPath()).TrimEnd('\')
    if (-not $fullPath.StartsWith(
            $temp + '\',
            [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($fullPath) -notmatch
            '^LmcToolchainProvenanceTest-[0-9a-f]{32}$') {
        throw "Refusing unsafe provenance fixture cleanup: $fullPath"
    }
    [System.IO.Directory]::Delete($fullPath, $true)
}

$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    'LmcToolchainProvenanceTest-' + [Guid]::NewGuid().ToString('N'))

try {
    New-Item -ItemType Directory -Path $fixtureRoot | Out-Null
    $attestation = New-FixtureAttestation
    $descriptors = @(New-FixtureDescriptors -Root $fixtureRoot)
    $snapshot = New-LmcDistributionToolchainSnapshot `
        -Descriptors $descriptors `
        -ToolingPreflight $attestation

    Assert-Equal -Expected 'PASS' -Actual $snapshot.Result `
        -Message 'Fixture toolchain snapshot did not pass.'
    Assert-Equal -Expected 8 -Actual $snapshot.RecordCount `
        -Message 'Fixture toolchain snapshot role count drifted.'
    Assert-True `
        -Condition ($snapshot.ToolchainSha256 -match '^[0-9A-F]{64}$') `
        -Message 'Fixture toolchain digest is malformed.'
    Assert-True `
        -Condition ($snapshot.ToolingPreflightSha256 -match '^[0-9A-F]{64}$') `
        -Message 'Fixture preflight attestation digest is malformed.'
    Assert-Equal -Expected 7 -Actual $snapshot.ToolingPreflightSuiteCount `
        -Message 'Fixture preflight suite count did not preserve 7/7 per host.'
    Assert-True `
        -Condition ((@($snapshot.Records | Where-Object {
            $_ -match '(?i)([A-Z]:[\\/]|\\\\|/Users/|/home/|/work/)'
        })).Count -eq 0) `
        -Message 'Fixture toolchain records leaked an absolute path.'

    $reversed = @($descriptors)
    [System.Array]::Reverse($reversed)
    $reversedSnapshot = New-LmcDistributionToolchainSnapshot `
        -Descriptors $reversed `
        -ToolingPreflight $attestation
    Assert-Equal `
        -Expected ($snapshot.Records -join "`n") `
        -Actual ($reversedSnapshot.Records -join "`n") `
        -Message 'Descriptor order changed canonical toolchain records.'
    Assert-Equal `
        -Expected $snapshot.ToolchainSha256 `
        -Actual $reversedSnapshot.ToolchainSha256 `
        -Message 'Descriptor order changed toolchain digest.'

    $manifestRecords = @(
        Assert-LmcDistributionToolchainManifestBinding `
            -Records $snapshot.Records `
            -Sha256 $snapshot.ToolchainSha256)
    Assert-Equal -Expected 8 -Actual $manifestRecords.Count `
        -Message 'Manifest toolchain binding did not preserve eight roles.'
    $manifestAttestation = `
        Assert-LmcDistributionToolingPreflightManifestBinding `
            -Result $snapshot.ToolingPreflightResult `
            -SuiteCount $snapshot.ToolingPreflightSuiteCount `
            -RunCount $snapshot.ToolingPreflightRunCount `
            -ToolingDigest $snapshot.ToolingPreflightDigest `
            -HostRecords $snapshot.ToolingPreflightHostRecords `
            -Sha256 $snapshot.ToolingPreflightSha256 `
            -ToolingFileCount $snapshot.ToolingPreflightFileCount
    Assert-Equal -Expected 7 -Actual $manifestAttestation.SuiteCount `
        -Message 'Manifest preflight binding did not preserve seven suites.'
    Assert-Equal -Expected 14 -Actual $manifestAttestation.RunCount `
        -Message 'Manifest preflight binding did not preserve 14/14.'

    $null = Assert-LmcDistributionInvokingPowerShellHostBound `
        -ToolingPreflight $attestation
    Assert-True -Condition $true `
        -Message 'Invoking PowerShell host did not match fixture attestation.'
    $mismatchedHostAttestation = New-FixtureAttestation
    $matchingLabel = if ($PSVersionTable.PSEdition -ceq 'Desktop') {
        'PS5'
    }
    else {
        'PS7'
    }
    $matchingHost = @($mismatchedHostAttestation.Hosts | Where-Object {
        $_.Label -ceq $matchingLabel
    })[0]
    $matchingHost.Version = '0.0.0-host-mismatch'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionInvokingPowerShellHostBound `
                -ToolingPreflight $mismatchedHostAttestation
        } `
        -ExpectedMessage 'not exactly bound'

    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors @($descriptors | Select-Object -First 7) `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'descriptor count must be 8'

    $duplicateDescriptors = @($descriptors | Select-Object -First 7) +
        @($descriptors[0])
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $duplicateDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'logical role is duplicated'

    $malformedDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $malformedDescriptors[0].Version = 'C:\leaked\compiler'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $malformedDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'malformed or contains a path'

    $missingDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $missingDescriptors[0].Path = Join-Path $fixtureRoot 'missing.bin'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $missingDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'file was not found'

    $roslynRoot = Join-Path $fixtureRoot 'roslyn-toolset'
    $roslynCompiler = Join-Path $roslynRoot 'csc.exe'
    $roslynSecondary = Join-Path $roslynRoot 'Microsoft.CodeAnalysis.dll'
    Write-FixtureFile -Path $roslynCompiler -Content 'compiler-v1'
    Write-FixtureFile -Path $roslynSecondary -Content 'compiler-task-v1'
    $roslynDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $roslynDescriptor = @($roslynDescriptors | Where-Object {
        $_.Role -ceq 'CSharpCompiler'
    })[0]
    $roslynDescriptor.Path = $roslynCompiler
    $roslynDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionRoot -Value $roslynRoot
    $roslynDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionFiles -Value @(
            'csc.exe', 'Microsoft.CodeAnalysis.dll')
    $roslynBefore = New-LmcDistributionToolchainSnapshot `
        -Descriptors $roslynDescriptors `
        -ToolingPreflight $attestation
    Write-FixtureFile -Path $roslynSecondary -Content 'compiler-task-v2'
    $roslynAfter = New-LmcDistributionToolchainSnapshot `
        -Descriptors $roslynDescriptors `
        -ToolingPreflight $attestation
    Assert-True `
        -Condition ($roslynBefore.ToolchainSha256 -cne
            $roslynAfter.ToolchainSha256) `
        -Message 'Secondary Roslyn toolset drift was not bound.'
    $roslynDescriptor.DistributionFiles = @('csc.exe', 'missing.dll')
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $roslynDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'file was not found'
    $roslynDescriptor.DistributionFiles = @('csc.exe', '../escape.dll')
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $roslynDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'malformed or duplicated'

    $roslynReparseRoot = Join-Path $fixtureRoot 'roslyn-reparse-root'
    $roslynReparseTarget = Join-Path $fixtureRoot 'roslyn-reparse-target'
    $roslynReparseLink = Join-Path $roslynReparseRoot 'linked-toolset'
    Write-FixtureFile `
        -Path (Join-Path $roslynReparseRoot 'csc.exe') `
        -Content 'compiler'
    Write-FixtureFile `
        -Path (Join-Path $roslynReparseTarget 'task.dll') `
        -Content 'task'
    New-Item -ItemType Junction `
        -Path $roslynReparseLink `
        -Target $roslynReparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($roslynReparseLink)
    Assert-Throws `
        -Action {
            Get-LmcDistributionPhysicalInventoryFiles `
                -Root $roslynReparseRoot `
                -Context 'Roslyn reparse fixture'
        } `
        -ExpectedMessage 'contains a reparse point'

    $pythonRoot = Join-Path $fixtureRoot 'python-runtime'
    $pythonExecutable = Join-Path $pythonRoot 'python.exe'
    $pythonStdlib = Join-Path $pythonRoot 'Lib\core.py'
    $pythonExcluded = Join-Path $pythonRoot `
        'Lib\site-packages\unrelated.py'
    Write-FixtureFile -Path $pythonExecutable -Content 'python-runtime-v1'
    Write-FixtureFile -Path $pythonStdlib -Content 'stdlib-v1'
    Write-FixtureFile -Path $pythonExcluded -Content 'excluded-v1'
    $pythonDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $pythonDescriptor = @($pythonDescriptors | Where-Object {
        $_.Role -ceq 'Python'
    })[0]
    $pythonDescriptor.Path = $pythonExecutable
    $pythonDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionRoot -Value $pythonRoot
    $pythonDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionFiles -Value @('python.exe', 'Lib/core.py')
    $pythonBefore = New-LmcDistributionToolchainSnapshot `
        -Descriptors $pythonDescriptors `
        -ToolingPreflight $attestation
    Write-FixtureFile -Path $pythonStdlib -Content 'stdlib-v2'
    $pythonAfterStdlib = New-LmcDistributionToolchainSnapshot `
        -Descriptors $pythonDescriptors `
        -ToolingPreflight $attestation
    Assert-True `
        -Condition ($pythonBefore.ToolchainSha256 -cne
            $pythonAfterStdlib.ToolchainSha256) `
        -Message 'Secondary Python runtime drift was not bound.'
    Write-FixtureFile -Path $pythonStdlib -Content 'stdlib-v1'
    Write-FixtureFile -Path $pythonExcluded -Content 'excluded-v2'
    $pythonAfterExcluded = New-LmcDistributionToolchainSnapshot `
        -Descriptors $pythonDescriptors `
        -ToolingPreflight $attestation
    Assert-Equal `
        -Expected $pythonBefore.ToolchainSha256 `
        -Actual $pythonAfterExcluded.ToolchainSha256 `
        -Message 'Excluded third-party Python bytes changed runtime provenance.'

    $reparseTarget = Join-Path $fixtureRoot 'reparse-target'
    $reparseLink = Join-Path $fixtureRoot 'reparse-link'
    Write-FixtureFile `
        -Path (Join-Path $reparseTarget 'PowerShell.bin') `
        -Content 'reparse-fixture'
    New-Item -ItemType Junction `
        -Path $reparseLink `
        -Target $reparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($reparseLink)
    $reparseDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $powerShellDescriptor = @($reparseDescriptors | Where-Object {
        $_.Role -ceq 'PowerShell'
    })[0]
    $powerShellDescriptor.Path = Join-Path $reparseLink 'PowerShell.bin'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $reparseDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'contains a reparse point'

    $packageRoot = Join-Path $fixtureRoot 'installed-package'
    $packageModule = Join-Path $packageRoot 'docx\__init__.py'
    $packageSecondary = Join-Path $packageRoot 'docx\api.py'
    Write-FixtureFile -Path $packageModule -Content 'module-v1'
    Write-FixtureFile -Path $packageSecondary -Content 'secondary-v1'
    $packageDescriptors = @($descriptors | ForEach-Object {
        [pscustomobject]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
    })
    $packageDescriptor = @($packageDescriptors | Where-Object {
        $_.Role -ceq 'PythonDocx'
    })[0]
    $packageDescriptor.Path = $packageModule
    $packageDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionRoot -Value $packageRoot
    $packageDescriptor | Add-Member -MemberType NoteProperty `
        -Name DistributionFiles -Value @(
            'docx/__init__.py', 'docx/api.py')
    $packageBefore = New-LmcDistributionToolchainSnapshot `
        -Descriptors $packageDescriptors `
        -ToolingPreflight $attestation
    Write-FixtureFile -Path $packageSecondary -Content 'secondary-v2'
    $packageAfter = New-LmcDistributionToolchainSnapshot `
        -Descriptors $packageDescriptors `
        -ToolingPreflight $attestation
    Assert-True `
        -Condition ($packageBefore.ToolchainSha256 -cne
            $packageAfter.ToolchainSha256) `
        -Message 'Secondary installed-package byte drift was not bound.'
    $packageDescriptor.DistributionFiles = @('docx/api.py')
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $packageDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'imported module is absent'
    $packageDescriptor.DistributionFiles = @(
        'docx/__init__.py', 'docx/missing.py')
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $packageDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'file was not found'
    $packageDescriptor.DistributionFiles = @(
        'docx/__init__.py', 'DOCX/__INIT__.PY')
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $packageDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'malformed or duplicated'

    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $descriptors `
                -ToolingPreflight (New-FixtureAttestation `
                    -Result 'FAIL')
        } `
        -ExpectedMessage 'not an exact 14/14 PASS'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $descriptors `
                -ToolingPreflight (New-FixtureAttestation `
                    -SuiteCount 6 `
                    -RunCount 12)
        } `
        -ExpectedMessage 'not an exact 14/14 PASS'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $descriptors `
                -ToolingPreflight (New-FixtureAttestation `
                    -SuiteCount 7 `
                    -RunCount 13)
        } `
        -ExpectedMessage 'not an exact 14/14 PASS'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingPreflightManifestBinding `
                -Result 'PASS' `
                -SuiteCount 6 `
                -RunCount 12 `
                -ToolingDigest $snapshot.ToolingPreflightDigest `
                -HostRecords $snapshot.ToolingPreflightHostRecords `
                -Sha256 $snapshot.ToolingPreflightSha256 `
                -ToolingFileCount $snapshot.ToolingPreflightFileCount
        } `
        -ExpectedMessage 'not an exact 14/14 PASS'
    Assert-Throws `
        -Action {
            Assert-LmcDistributionToolingPreflightManifestBinding `
                -Result 'PASS' `
                -SuiteCount 7 `
                -RunCount 13 `
                -ToolingDigest $snapshot.ToolingPreflightDigest `
                -HostRecords $snapshot.ToolingPreflightHostRecords `
                -Sha256 $snapshot.ToolingPreflightSha256 `
                -ToolingFileCount $snapshot.ToolingPreflightFileCount
        } `
        -ExpectedMessage 'not an exact 14/14 PASS'

    $duplicateHostAttestation = New-FixtureAttestation
    $duplicateHostAttestation.Hosts = @(
        $duplicateHostAttestation.Hosts[0],
        $duplicateHostAttestation.Hosts[0])
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $descriptors `
                -ToolingPreflight $duplicateHostAttestation
        } `
        -ExpectedMessage 'duplicated'

    $pathHostAttestation = New-FixtureAttestation
    $pathHostAttestation.Hosts[0].Version = 'C:\host\version'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $descriptors `
                -ToolingPreflight $pathHostAttestation
        } `
        -ExpectedMessage 'malformed or contains a path'

    $currentHost = [System.Diagnostics.Process]::GetCurrentProcess().
        MainModule.FileName
    $nonzeroArguments = if ($PSVersionTable.PSEdition -ceq 'Desktop') {
        @('-NoLogo', '-NoProfile', '-NonInteractive',
            '-Command', 'exit 7')
    }
    else {
        @('-NoLogo', '-NoProfile', '-NonInteractive',
            '-Command', 'exit 7')
    }
    Assert-Throws `
        -Action {
            Get-LmcDistributionToolchainProbeLine `
                -ExecutablePath $currentHost `
                -Arguments $nonzeroArguments `
                -WorkingDirectory $fixtureRoot `
                -Context 'nonzero fixture'
        } `
        -ExpectedMessage 'exited nonzero: 7'

    $tamperedPath = $descriptors[0].Path
    $beforeTamper = $snapshot.ToolchainSha256
    Write-FixtureFile -Path $tamperedPath -Content 'fixture-tampered'
    $afterTamper = New-LmcDistributionToolchainSnapshot `
        -Descriptors $descriptors `
        -ToolingPreflight $attestation
    Assert-True `
        -Condition ($beforeTamper -cne $afterTamper.ToolchainSha256) `
        -Message 'Tool byte tamper did not change the provenance digest.'

    $production = Get-LmcDistributionReleaseToolchainSnapshot `
        -RepositoryRoot ([System.IO.Path]::GetFullPath(
            (Join-Path $PSScriptRoot '..\..'))) `
        -ToolingPreflight $attestation
    Assert-Equal -Expected 'PASS' -Actual $production.Result `
        -Message 'Production release toolchain did not resolve.'
    Assert-Equal -Expected 8 -Actual $production.RecordCount `
        -Message 'Production release toolchain role count drifted.'
    Assert-True `
        -Condition ((@($production.Records | Where-Object {
            $_ -match '(?i)([A-Z]:[\\/]|\\\\|/Users/|/home/|/work/)'
        })).Count -eq 0) `
        -Message 'Production release toolchain records leaked a path.'
    $expectedCompilerRoot = Join-Path (
        Split-Path -Parent $production.RuntimePaths.MSBuild) 'Roslyn'
    Assert-True `
        -Condition ($production.RuntimePaths.CSharpCompiler.StartsWith(
            $expectedCompilerRoot + '\',
            [System.StringComparison]::OrdinalIgnoreCase)) `
        -Message 'C# compiler was not derived from the selected MSBuild toolset.'
    $gitLauncher = Get-LmcDistributionApplicationPath -Name 'git'
    Assert-True `
        -Condition (-not $production.RuntimePaths.Git.Equals(
            $gitLauncher,
            [System.StringComparison]::OrdinalIgnoreCase)) `
        -Message 'Git launcher was not distinguished from the actual core executable.'
    $resolvedGit = Resolve-LmcDistributionGitDescriptor `
        -WorkingDirectory ([System.IO.Path]::GetFullPath(
            (Join-Path $PSScriptRoot '..\..')))
    Assert-Equal `
        -Expected $resolvedGit.Path `
        -Actual $production.RuntimePaths.Git `
        -Message 'Runtime Git path is not the resolved Git core executable.'
    Assert-True `
        -Condition ($production.InventoryFileCounts.CSharpCompiler -gt 1) `
        -Message 'Roslyn toolset provenance did not bind multiple physical files.'
    Assert-True `
        -Condition ($production.InventoryFileCounts.Python -gt 1) `
        -Message 'Python runtime provenance did not bind multiple physical files.'
    Assert-True `
        -Condition ($production.InventoryFileCounts.PythonDocx -gt 1 -and
            $production.InventoryFileCounts.PyPdf -gt 1) `
        -Message 'Imported Python distributions were not fully inventoried.'
    Assert-True `
        -Condition ($production.Records -match '^PythonDocx\|' -and
            $production.Records -match '^PyPdf\|') `
        -Message 'Imported Python package provenance is missing.'
    $pythonImportEvidence = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $production.RuntimePaths.Python `
        -Arguments @('-c', 'import docx, pypdf; print("READY")') `
        -WorkingDirectory $fixtureRoot `
        -Context 'post-snapshot Python import'
    Assert-Equal -Expected 'READY' -Actual $pythonImportEvidence `
        -Message 'Post-snapshot Python imports failed.'
    $productionAfterImports = Get-LmcDistributionReleaseToolchainSnapshot `
        -RepositoryRoot ([System.IO.Path]::GetFullPath(
            (Join-Path $PSScriptRoot '..\..'))) `
        -ToolingPreflight $attestation
    Assert-Equal `
        -Expected $production.ToolchainSha256 `
        -Actual $productionAfterImports.ToolchainSha256 `
        -Message 'Python imports caused mutable toolchain provenance drift.'

    Write-Host "PASS: $script:Passed distribution toolchain provenance assertions"
}
finally {
    Remove-FixtureRoot -Path $fixtureRoot
}

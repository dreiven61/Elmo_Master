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

    $roles = @(Get-LmcDistributionExpectedToolchainRoles)
    $inventoryRequiredRoles = @(
        Get-LmcDistributionInventoryRequiredRoles)
    $pythonRoot = Join-Path $Root 'python-runtime'
    $descriptors = @()
    $index = 0
    foreach ($role in $roles) {
        if ($inventoryRequiredRoles -ccontains $role) {
            if ($role -ceq 'CSharpCompiler') {
                $distributionRoot = Join-Path $Root 'inventory-CSharpCompiler'
                $moduleRelative = 'CSharpCompiler.bin'
                $secondaryRelative = 'CSharpCompiler-secondary.bin'
            }
            elseif ($role -ceq 'Python') {
                $distributionRoot = $pythonRoot
                $moduleRelative = 'python.exe'
                $secondaryRelative = 'Lib/core.py'
            }
            else {
                $distributionRoot = $pythonRoot
                $moduleRelative = if ($role -ceq 'PythonCryptography') {
                    'Lib/site-packages/cryptography/__init__.py'
                }
                else {
                    "Lib/site-packages/$role/$role.bin"
                }
                $secondaryRelative = if ($role -ceq 'PythonCffi') {
                    'Scripts/cffi-gen-src.exe'
                }
                elseif ($role -ceq 'PythonLxml') {
                    'Lib/site-packages/PythonLxml/__pycache__/lxml.fixture.pyc'
                }
                else {
                    "Lib/site-packages/$role/$role-secondary.bin"
                }
            }
            $path = Join-Path $distributionRoot $moduleRelative
            Write-FixtureFile `
                -Path $path `
                -Content ("fixture-$role-v1")
            Write-FixtureFile `
                -Path (Join-Path $distributionRoot $secondaryRelative) `
                -Content ("fixture-$role-secondary-v1")
            $descriptors += [pscustomobject]@{
                Role = $role
                Version = "1.0.$index"
                Path = $path
                DistributionRoot = $distributionRoot
                DistributionFiles = @(
                    $moduleRelative,
                    $secondaryRelative)
            }
        }
        else {
            $path = Join-Path $Root ("$role.bin")
            Write-FixtureFile -Path $path -Content ("fixture-$role-v1")
            $descriptors += [pscustomobject]@{
                Role = $role
                Version = "1.0.$index"
                Path = $path
            }
        }
        $index++
    }
    return @($descriptors)
}

function Copy-FixtureDescriptors {
    param([Parameter(Mandatory = $true)][object[]]$Descriptors)

    return @($Descriptors | ForEach-Object {
        $copy = [ordered]@{
            Role = $_.Role
            Version = $_.Version
            Path = $_.Path
        }
        if ($_.PSObject.Properties.Name -contains 'DistributionRoot') {
            $copy.DistributionRoot = $_.DistributionRoot
            $copy.DistributionFiles = [string[]]@($_.DistributionFiles)
        }
        [pscustomobject]$copy
    })
}

function New-FixturePythonEvidence {
    param([Parameter(Mandatory = $true)][object[]]$Descriptors)

    $contracts = @(Get-LmcDistributionPythonPackageContracts)
    $python = @($Descriptors | Where-Object {
        $_.Role -ceq 'Python'
    })[0]
    $packages = @()
    foreach ($contract in $contracts) {
        $descriptor = @($Descriptors | Where-Object {
            $_.Role -ceq $contract.Role
        })[0]
        $packages += [pscustomobject]@{
            Role = $contract.Role
            Distribution = $contract.Distribution
            Version = $descriptor.Version
            Module = $contract.Module
            ModulePath = $descriptor.Path
            DistributionLocation = Join-Path `
                $python.DistributionRoot `
                'Lib\site-packages'
            DistributionFiles = [string[]]@(
                $descriptor.DistributionFiles | ForEach-Object {
                    $relative = ([string]$_).Replace('\', '/')
                    if ($relative.StartsWith(
                            'Lib/site-packages/',
                            [System.StringComparison]::Ordinal)) {
                        $relative.Substring('Lib/site-packages/'.Length)
                    }
                    else {
                        '../../' + $relative
                    }
                })
        }
    }
    return [pscustomobject]@{
        Executable = $python.Path
        PythonVersion = $python.Version
        BasePrefix = $python.DistributionRoot
        ActiveOwners = [string[]]@($contracts | ForEach-Object {
            $_.Distribution
        })
        OwnerlessModules = @()
        Packages = @($packages)
    }
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
    Assert-Equal -Expected 13 -Actual $snapshot.RecordCount `
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
    Assert-Equal -Expected 13 -Actual $manifestRecords.Count `
        -Message 'Manifest toolchain binding did not preserve thirteen roles.'
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
                -Descriptors @($descriptors | Select-Object -First 12) `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'descriptor count must be 13'

    $oldEightRoleDescriptors = @($descriptors | Where-Object {
        @(
            'CSharpCompiler', 'Git', 'MSBuild', 'PowerShell',
            'PyPdf', 'Python', 'PythonDocx', 'VsWhere') -ccontains
            $_.Role
    })
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $oldEightRoleDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'descriptor count must be 13'

    $duplicateDescriptors = @($descriptors | Select-Object -First 12) +
        @($descriptors[0])
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $duplicateDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'logical role is duplicated'

    $malformedDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $malformedDescriptors[0].Version = 'C:\leaked\compiler'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $malformedDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'malformed or contains a path'

    $missingDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $missingDescriptors[0].Path = Join-Path $fixtureRoot 'missing.bin'
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $missingDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'file was not found'

    foreach ($inventoryRequiredRole in @(
            Get-LmcDistributionInventoryRequiredRoles)) {
        $missingInventoryDescriptors = @(
            Copy-FixtureDescriptors -Descriptors $descriptors)
        $missingInventoryDescriptor = @(
            $missingInventoryDescriptors | Where-Object {
                $_.Role -ceq $inventoryRequiredRole
            })[0]
        $missingInventoryDescriptor.PSObject.Properties.Remove(
            'DistributionRoot')
        $missingInventoryDescriptor.PSObject.Properties.Remove(
            'DistributionFiles')
        Assert-Throws `
            -Action {
                New-LmcDistributionToolchainSnapshot `
                    -Descriptors $missingInventoryDescriptors `
                    -ToolingPreflight $attestation
            } `
            -ExpectedMessage 'distribution inventory is required'
    }

    $unexpectedInventoryDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $unexpectedInventoryDescriptor = @(
        $unexpectedInventoryDescriptors | Where-Object {
            $_.Role -ceq 'Git'
        })[0]
    $unexpectedInventoryDescriptor | Add-Member `
        -MemberType NoteProperty `
        -Name DistributionRoot `
        -Value $fixtureRoot
    $unexpectedInventoryDescriptor | Add-Member `
        -MemberType NoteProperty `
        -Name DistributionFiles `
        -Value @([System.IO.Path]::GetFileName(
            $unexpectedInventoryDescriptor.Path))
    Assert-Throws `
        -Action {
            New-LmcDistributionToolchainSnapshot `
                -Descriptors $unexpectedInventoryDescriptors `
                -ToolingPreflight $attestation
        } `
        -ExpectedMessage 'inventory is incomplete or unexpected'

    $fixturePythonRoot = @($descriptors | Where-Object {
        $_.Role -ceq 'Python'
    })[0].DistributionRoot
    $fixturePythonScriptsUnrelated = Join-Path `
        $fixturePythonRoot `
        'Scripts\unrelated.exe'
    Write-FixtureFile `
        -Path $fixturePythonScriptsUnrelated `
        -Content 'scripts-unrelated-v1'
    $pythonEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $parsedPythonDescriptors = @(
        ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
            -Evidence $pythonEvidence `
            -CandidatePath $pythonEvidence.Executable)
    Assert-Equal -Expected 8 -Actual $parsedPythonDescriptors.Count `
        -Message 'Exact active Python package evidence did not produce eight descriptors.'
    Assert-True `
        -Condition ($parsedPythonDescriptors.Role -ccontains 'PythonCffi' -and
            @($parsedPythonDescriptors | Where-Object {
                $_.Role -ceq 'PythonCffi'
            })[0].Path.EndsWith(
                'PythonCffi.bin',
                [System.StringComparison]::Ordinal) -and
            @($parsedPythonDescriptors | Where-Object {
                $_.Role -ceq 'PythonCffi'
            })[0].DistributionFiles -ccontains
                'Scripts/cffi-gen-src.exe') `
        -Message 'cffi backend or root-relative Scripts inventory was not bound.'
    Assert-True `
        -Condition (@($parsedPythonDescriptors | Where-Object {
            $_.Role -ceq 'PythonLxml'
        })[0].DistributionFiles -ccontains
            'Lib/site-packages/PythonLxml/__pycache__/lxml.fixture.pyc') `
        -Message 'Active dependency pyc inventory was not retained.'

    $ownerlessStdlibPath = Join-Path $fixturePythonRoot `
        'Lib\ownerless_stdlib.py'
    Write-FixtureFile `
        -Path $ownerlessStdlibPath `
        -Content 'ownerless-stdlib-v1'
    $validOwnerlessEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $validOwnerlessEvidence.OwnerlessModules = @(
        [pscustomobject]@{
            Name = '_fixture_builtin'
            Origin = 'built-in'
            Paths = @()
        },
        [pscustomobject]@{
            Name = 'fixture_stdlib'
            Origin = $ownerlessStdlibPath
            Paths = @($ownerlessStdlibPath)
        },
        [pscustomobject]@{
            Name = '_openssl'
            Origin = ''
            Paths = @()
        },
        [pscustomobject]@{
            Name = '_openssl.lib'
            Origin = ''
            Paths = @()
        },
        [pscustomobject]@{
            Name = 'cython_runtime'
            Origin = ''
            Paths = @()
        },
        [pscustomobject]@{
            Name = '_cython_9_12_3'
            Origin = ''
            Paths = @()
        },
        [pscustomobject]@{
            Name = 'pyexpat.errors'
            Origin = ''
            Paths = @()
        })
    Assert-Equal `
        -Expected 8 `
        -Actual @(ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
            -Evidence $validOwnerlessEvidence `
            -CandidatePath $validOwnerlessEvidence.Executable).Count `
        -Message 'Validated runtime and narrow synthetic ownerless modules were rejected.'

    $ownerlessSitePackagesPath = Join-Path $fixturePythonRoot `
        'Lib\site-packages\unowned_namespace'
    Write-FixtureFile `
        -Path (Join-Path $ownerlessSitePackagesPath '__init__.py') `
        -Content 'unowned-site-packages-v1'
    $ownerlessSitePackagesEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $ownerlessSitePackagesEvidence.OwnerlessModules = @(
        [pscustomobject]@{
            Name = 'unowned_namespace'
            Origin = ''
            Paths = @($ownerlessSitePackagesPath)
        })
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $ownerlessSitePackagesEvidence `
                -CandidatePath $ownerlessSitePackagesEvidence.Executable
        } `
        -ExpectedMessage 'excluded runtime path'

    $ownerlessExternalPath = Join-Path $fixtureRoot `
        'external-pythonpath\unowned_namespace'
    Write-FixtureFile `
        -Path (Join-Path $ownerlessExternalPath '__init__.py') `
        -Content 'unowned-external-v1'
    $ownerlessExternalEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $ownerlessExternalEvidence.OwnerlessModules = @(
        [pscustomobject]@{
            Name = 'external_namespace'
            Origin = ''
            Paths = @($ownerlessExternalPath)
        })
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $ownerlessExternalEvidence `
                -CandidatePath $ownerlessExternalEvidence.Executable
        } `
        -ExpectedMessage 'escaped the runtime root'

    $unexpectedNoOriginEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $unexpectedNoOriginEvidence.OwnerlessModules = @(
        [pscustomobject]@{
            Name = 'unexpected_no_origin'
            Origin = ''
            Paths = @()
        })
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $unexpectedNoOriginEvidence `
                -CandidatePath $unexpectedNoOriginEvidence.Executable
        } `
        -ExpectedMessage 'unexpected no-origin contract'

    $parsedFullDescriptors = @($descriptors | Where-Object {
        @(
            'CSharpCompiler', 'Git', 'MSBuild', 'PowerShell', 'VsWhere') `
            -ccontains $_.Role
    }) + $parsedPythonDescriptors
    $parsedBeforeUnrelatedScripts = `
        New-LmcDistributionToolchainSnapshot `
            -Descriptors $parsedFullDescriptors `
            -ToolingPreflight $attestation
    Write-FixtureFile `
        -Path $fixturePythonScriptsUnrelated `
        -Content 'scripts-unrelated-v2'
    $parsedAfterUnrelatedScripts = `
        New-LmcDistributionToolchainSnapshot `
            -Descriptors $parsedFullDescriptors `
            -ToolingPreflight $attestation
    Assert-Equal `
        -Expected $parsedBeforeUnrelatedScripts.ToolchainSha256 `
        -Actual $parsedAfterUnrelatedScripts.ToolchainSha256 `
        -Message 'Unrelated Python Scripts entrypoint changed provenance.'
    Write-FixtureFile `
        -Path $fixturePythonScriptsUnrelated `
        -Content 'scripts-unrelated-v1'

    $escapeInventoryEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $escapeCffiPackage = @(
        $escapeInventoryEvidence.Packages | Where-Object {
            $_.Role -ceq 'PythonCffi'
        })[0]
    $escapeCffiPackage.DistributionFiles = @(
        $escapeCffiPackage.DistributionFiles) + @('../../../escape.bin')
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $escapeInventoryEvidence `
                -CandidatePath $escapeInventoryEvidence.Executable
        } `
        -ExpectedMessage 'escaped the Python root'

    $metadataReparseTarget = Join-Path $fixtureRoot `
        'metadata-reparse-target'
    $metadataReparseLink = Join-Path `
        $fixturePythonRoot `
        'metadata-reparse-link'
    Write-FixtureFile `
        -Path (Join-Path $metadataReparseTarget 'module.bin') `
        -Content 'metadata-reparse'
    New-Item -ItemType Junction `
        -Path $metadataReparseLink `
        -Target $metadataReparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($metadataReparseLink)
    $metadataReparseEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    @($metadataReparseEvidence.Packages | Where-Object {
        $_.Role -ceq 'PythonCffi'
    })[0].DistributionLocation = $metadataReparseLink
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $metadataReparseEvidence `
                -CandidatePath $metadataReparseEvidence.Executable
        } `
        -ExpectedMessage 'contains a reparse point'

    $missingOwnerEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $missingOwnerEvidence.ActiveOwners = @(
        $missingOwnerEvidence.ActiveOwners | Where-Object {
            $_ -cne 'cffi'
        })
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $missingOwnerEvidence `
                -CandidatePath $missingOwnerEvidence.Executable
        } `
        -ExpectedMessage 'owner set is not exact'

    $duplicateOwnerEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $duplicateOwnerEvidence.ActiveOwners = @(
        $duplicateOwnerEvidence.ActiveOwners) + @('cffi')
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $duplicateOwnerEvidence `
                -CandidatePath $duplicateOwnerEvidence.Executable
        } `
        -ExpectedMessage 'value is duplicated'

    $startupOwnerEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $startupOwnerEvidence.ActiveOwners = @(
        $startupOwnerEvidence.ActiveOwners) + @('setuptools')
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $startupOwnerEvidence `
                -CandidatePath $startupOwnerEvidence.Executable
        } `
        -ExpectedMessage 'owner set is not exact'

    $ownershipEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    @($ownershipEvidence.Packages | Where-Object {
        $_.Role -ceq 'PythonCffi'
    })[0].Distribution = 'pycparser'
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $ownershipEvidence `
                -CandidatePath $ownershipEvidence.Executable
        } `
        -ExpectedMessage 'ownership is invalid'

    $extraPackageEvidence = New-FixturePythonEvidence `
        -Descriptors $descriptors
    $extraPackageEvidence.Packages = @(
        $extraPackageEvidence.Packages) + @(
        [pscustomobject]@{
            Role = 'PythonPycparser'
            Distribution = 'pycparser'
            Version = '3.0'
            Module = 'pycparser'
            ModulePath = $descriptors[0].Path
            DistributionLocation = $fixtureRoot
            DistributionFiles = @(
                [System.IO.Path]::GetFileName($descriptors[0].Path))
        })
    Assert-Throws `
        -Action {
            ConvertTo-LmcDistributionPythonDescriptorsFromEvidence `
                -Evidence $extraPackageEvidence `
                -CandidatePath $extraPackageEvidence.Executable
        } `
        -ExpectedMessage 'package evidence count is not exact'

    $roslynRoot = Join-Path $fixtureRoot 'roslyn-toolset'
    $roslynCompiler = Join-Path $roslynRoot 'csc.exe'
    $roslynSecondary = Join-Path $roslynRoot 'Microsoft.CodeAnalysis.dll'
    Write-FixtureFile -Path $roslynCompiler -Content 'compiler-v1'
    Write-FixtureFile -Path $roslynSecondary -Content 'compiler-task-v1'
    $roslynDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $roslynDescriptor = @($roslynDescriptors | Where-Object {
        $_.Role -ceq 'CSharpCompiler'
    })[0]
    $roslynDescriptor.Path = $roslynCompiler
    $roslynDescriptor.DistributionRoot = $roslynRoot
    $roslynDescriptor.DistributionFiles = @(
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
    $pythonPycparser = Join-Path $pythonRoot `
        'Lib\site-packages\pycparser\__init__.py'
    $pythonStartupSetuptools = Join-Path $pythonRoot `
        'Lib\site-packages\_distutils_hack\__init__.py'
    Write-FixtureFile -Path $pythonExecutable -Content 'python-runtime-v1'
    Write-FixtureFile -Path $pythonStdlib -Content 'stdlib-v1'
    Write-FixtureFile -Path $pythonExcluded -Content 'excluded-v1'
    Write-FixtureFile -Path $pythonPycparser -Content 'pycparser-v1'
    Write-FixtureFile `
        -Path $pythonStartupSetuptools `
        -Content 'setuptools-startup-v1'
    $pythonDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $pythonDescriptor = @($pythonDescriptors | Where-Object {
        $_.Role -ceq 'Python'
    })[0]
    $pythonDescriptor.Path = $pythonExecutable
    $pythonDescriptor.DistributionRoot = $pythonRoot
    $pythonDescriptor.DistributionFiles = @(
        'python.exe', 'Lib/core.py')
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
    Write-FixtureFile -Path $pythonPycparser -Content 'pycparser-v2'
    Write-FixtureFile `
        -Path $pythonStartupSetuptools `
        -Content 'setuptools-startup-v2'
    $pythonAfterExcluded = New-LmcDistributionToolchainSnapshot `
        -Descriptors $pythonDescriptors `
        -ToolingPreflight $attestation
    Assert-Equal `
        -Expected $pythonBefore.ToolchainSha256 `
        -Actual $pythonAfterExcluded.ToolchainSha256 `
        -Message 'Unrelated, pycparser, or startup setuptools bytes changed runtime provenance.'

    $reparseTarget = Join-Path $fixtureRoot 'reparse-target'
    $reparseLink = Join-Path $fixtureRoot 'reparse-link'
    Write-FixtureFile `
        -Path (Join-Path $reparseTarget 'PowerShell.bin') `
        -Content 'reparse-fixture'
    New-Item -ItemType Junction `
        -Path $reparseLink `
        -Target $reparseTarget | Out-Null
    $script:TrackedReparsePaths.Add($reparseLink)
    $reparseDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
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
    $packageDescriptors = @(
        Copy-FixtureDescriptors -Descriptors $descriptors)
    $packageDescriptor = @($packageDescriptors | Where-Object {
        $_.Role -ceq 'PythonDocx'
    })[0]
    $packageDescriptor.Path = $packageModule
    $packageDescriptor.DistributionRoot = $packageRoot
    $packageDescriptor.DistributionFiles = @(
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

    foreach ($dependencyRole in @(
            'PythonCffi',
            'PythonCryptography',
            'PythonLxml',
            'PythonPillow',
            'PythonTypingExtensions')) {
        $dependencyDescriptors = @(
            Copy-FixtureDescriptors -Descriptors $descriptors)
        $dependencyDescriptor = @(
            $dependencyDescriptors | Where-Object {
                $_.Role -ceq $dependencyRole
            })[0]
        $dependencySecondaryRelative = @(
            $dependencyDescriptor.DistributionFiles | Where-Object {
                -not ([System.IO.Path]::GetFullPath(
                    (Join-Path $dependencyDescriptor.DistributionRoot $_))).
                    Equals(
                        [System.IO.Path]::GetFullPath(
                            $dependencyDescriptor.Path),
                        [System.StringComparison]::OrdinalIgnoreCase)
            })[0]
        $dependencySecondaryPath = Join-Path `
            $dependencyDescriptor.DistributionRoot `
            $dependencySecondaryRelative
        $dependencyBefore = New-LmcDistributionToolchainSnapshot `
            -Descriptors $dependencyDescriptors `
            -ToolingPreflight $attestation
        Write-FixtureFile `
            -Path $dependencySecondaryPath `
            -Content ("fixture-$dependencyRole-secondary-v2")
        $dependencyAfter = New-LmcDistributionToolchainSnapshot `
            -Descriptors $dependencyDescriptors `
            -ToolingPreflight $attestation
        Assert-True `
            -Condition ($dependencyBefore.ToolchainSha256 -cne
                $dependencyAfter.ToolchainSha256) `
            -Message "$dependencyRole secondary byte drift was not bound."
        Write-FixtureFile `
            -Path $dependencySecondaryPath `
            -Content ("fixture-$dependencyRole-secondary-v1")
    }

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
    Assert-Equal -Expected 13 -Actual $production.RecordCount `
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
    $productionPythonPackageRoles = @(
        'PyPdf',
        'PythonCffi',
        'PythonCryptography',
        'PythonDocx',
        'PythonLxml',
        'PythonPillow',
        'PythonTypingExtensions')
    Assert-True `
        -Condition (@($productionPythonPackageRoles | Where-Object {
            [int]$production.InventoryFileCounts.$_ -le 1
        }).Count -eq 0) `
        -Message 'Active Python distributions were not fully inventoried.'
    Assert-True `
        -Condition (@($productionPythonPackageRoles | Where-Object {
            $role = $_
            @($production.Records | Where-Object {
                $_ -match ('^' + [regex]::Escape($role) + '\|')
            }).Count -ne 1
        }).Count -eq 0) `
        -Message 'Active Python package provenance is incomplete.'
    Assert-True `
        -Condition (@($production.Records | Where-Object {
            $_ -match '^Python(Pycparser|Setuptools)\|'
        }).Count -eq 0) `
        -Message 'Execution-unreached or startup-only packages entered provenance.'

    $legacyMetadataPython = $null
    foreach ($pythonCommand in @(Get-Command `
            -Name 'python' `
            -CommandType Application `
            -All `
            -ErrorAction SilentlyContinue)) {
        try {
            $pythonCommandPath = `
                Resolve-LmcDistributionProvenancePhysicalFile `
                    -Path ([string]$pythonCommand.Source) `
                    -Context 'legacy metadata Python candidate'
            $metadataCapability = `
                Get-LmcDistributionToolchainProbeLine `
                    -ExecutablePath $pythonCommandPath `
                    -Arguments @(
                        '-B', '-c',
                        ('import importlib.metadata as m; ' +
                            'print("LEGACY" if not hasattr(m, ' +
                            '"packages_distributions") else "MODERN")')) `
                    -WorkingDirectory $fixtureRoot `
                    -Context 'Python metadata compatibility'
            if ($metadataCapability -ceq 'LEGACY') {
                $legacyMetadataPython = $pythonCommandPath
                break
            }
        }
        catch {
            continue
        }
    }
    Assert-True `
        -Condition (-not [System.IO.File]::ReadAllText(
            $implementation).Contains('packages_distributions(')) `
        -Message 'Python provenance silently requires the post-3.8 packages_distributions API.'
    $legacyRejection = ''
    if (-not [string]::IsNullOrWhiteSpace($legacyMetadataPython)) {
        try {
            Resolve-LmcDistributionPythonDescriptors `
                -CandidatePaths @($legacyMetadataPython) `
                -WorkingDirectory $fixtureRoot | Out-Null
        }
        catch {
            $legacyRejection = $_.Exception.Message
        }
    }
    Assert-True `
        -Condition ([string]::IsNullOrWhiteSpace($legacyMetadataPython) -or
            $legacyRejection.Contains(
                'active distribution owner set mismatch')) `
        -Message 'Legacy Python was not rejected by the exact owner-set contract.'
    Assert-True `
        -Condition ([string]::IsNullOrWhiteSpace($legacyMetadataPython) -or
            $legacyRejection.Contains('numpy')) `
        -Message 'Legacy Python controlled rejection did not identify extra numpy ownership.'
    $pythonImportEvidence = Get-LmcDistributionToolchainProbeLine `
        -ExecutablePath $production.RuntimePaths.Python `
        -Arguments @('-B', '-c', 'import docx, pypdf; print("READY")') `
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

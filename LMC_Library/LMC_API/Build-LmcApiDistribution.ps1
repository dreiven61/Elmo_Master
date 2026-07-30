[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [switch]$AllowDirty
)

$ErrorActionPreference = 'Stop'

$releaseManifestImplementation = Join-Path $PSScriptRoot 'ReleaseManifest.ps1'
if (-not (Test-Path -LiteralPath $releaseManifestImplementation -PathType Leaf)) {
    throw "Release-manifest implementation not found: $releaseManifestImplementation"
}
. $releaseManifestImplementation

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..'))
}

$libraryProject = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.csproj'
$testProject = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj'
$sourceDll = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\src\bin\Release\LasalMotionControlLib.dll'

$distribution = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Distribution'
$distributionDll = Join-Path $distribution `
    '01_API\LasalMotionControlLib.dll'
$exampleProject = Join-Path $distribution `
    '02_Example_Program\LasalApiWpfTestApp\LasalApiWpfTestApp.csproj'
$exampleOutput = Join-Path $distribution `
    '02_Example_Program\LasalApiWpfTestApp\bin\Release'
$runDirectory = Join-Path $distribution '02_Example_Program\Run'
$distributionManual = Join-Path $distribution `
    '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
$distributionManualDocx = Join-Path $distribution `
    '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'

$developmentExampleRoot = Join-Path $RepositoryRoot `
    'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
$distributionExampleRoot = Split-Path -Parent $exampleProject
$manualDirectory = Split-Path -Parent $distributionManual

foreach ($requiredPath in @(
    $libraryProject,
    $testProject,
    $distribution,
    $exampleProject,
    $developmentExampleRoot,
    $distributionManual,
    $distributionManualDocx)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required path not found: $requiredPath"
    }
}

# The distributed example must remain source-identical to the development
# example, except for its binary Reference path and distribution README.
foreach ($relativeSource in @(
    'App.xaml',
    'App.xaml.cs',
    'MainWindow.xaml',
    'MainWindow.xaml.cs',
    'Properties\AssemblyInfo.cs')) {
    $developmentSource = Join-Path $developmentExampleRoot $relativeSource
    $distributionSource = Join-Path $distributionExampleRoot $relativeSource
    if ((Get-FileHash -LiteralPath $developmentSource -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $distributionSource -Algorithm SHA256).Hash) {
        throw "Distribution example source is stale: $relativeSource"
    }
}

$expectedDirectories = @(
    '01_API',
    '02_Example_Program',
    '03_API_User_Manual'
)
$actualDirectories = @(
    Get-ChildItem -LiteralPath $distribution -Directory |
        Select-Object -ExpandProperty Name
)
$directoryDifference = Compare-Object `
    -ReferenceObject $expectedDirectories `
    -DifferenceObject $actualDirectories
if ($directoryDifference) {
    throw 'Distribution must contain exactly the three numbered deliverable directories.'
}

$unexpectedTopFiles = @(
    Get-ChildItem -LiteralPath $distribution -File |
        Where-Object {
            $_.Name -notin @('README.md', 'RELEASE_MANIFEST.md')
        }
)
if ($unexpectedTopFiles) {
    throw 'Only README.md and RELEASE_MANIFEST.md are allowed next to the three deliverable directories.'
}

$expectedManualFiles = @(
    'LASAL_Motion_Control_API_User_Manual_KO.docx',
    'LASAL_Motion_Control_API_User_Manual_KO.pdf'
)
$actualManualFiles = @(
    Get-ChildItem -LiteralPath $manualDirectory -File |
        Select-Object -ExpandProperty Name
)
$manualFileDifference = Compare-Object `
    -ReferenceObject $expectedManualFiles `
    -DifferenceObject $actualManualFiles
if ($manualFileDifference) {
    throw 'The manual directory must contain only the canonical DOCX and PDF.'
}

$manifestRepositoryRelativePath = `
    'LMC_Library/LMC_API_Distribution/RELEASE_MANIFEST.md'
$gitStatus = & git -C $RepositoryRoot status `
    --porcelain=v1 --untracked-files=all
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read Git status.'
}
$releaseInputGitStatus = @(Get-LmcReleaseManifestInputGitStatus `
    -GitStatus $gitStatus `
    -ManifestRepositoryRelativePath $manifestRepositoryRelativePath)
$worktreeState = Get-LmcReleaseManifestWorktreeState `
    -GitStatus $releaseInputGitStatus `
    -AllowDirty:$AllowDirty
$sourceCommitOutput = @(& git -C $RepositoryRoot rev-parse --verify HEAD)
if ($LASTEXITCODE -ne 0 -or $sourceCommitOutput.Count -ne 1) {
    throw 'Unable to resolve the source commit for the release manifest.'
}
$sourceCommit = ([string]$sourceCommitOutput[0]).Trim()

$vswhere = Join-Path ${env:ProgramFiles(x86)} `
    'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'vswhere.exe was not found. Install Visual Studio Build Tools.'
}

$msbuild = & $vswhere -latest -products * `
    -requires Microsoft.Component.MSBuild `
    -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($msbuild)) {
    throw 'MSBuild.exe was not found.'
}

$pythonCandidates = @()
$bundledPython = Join-Path $env:USERPROFILE `
    '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
if (Test-Path -LiteralPath $bundledPython) {
    $pythonCandidates += $bundledPython
}
$pythonCommand = Get-Command python -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($null -ne $pythonCommand) {
    $pythonCandidates += $pythonCommand.Source
}

$python = $null
foreach ($candidate in $pythonCandidates | Select-Object -Unique) {
    & $candidate -c `
        'import docx, pypdf' `
        2>$null
    if ($LASTEXITCODE -eq 0) {
        $python = $candidate
        break
    }
}
if ([string]::IsNullOrWhiteSpace($python)) {
    throw 'A compatible Python with python-docx, reportlab, and pypdf was not found.'
}

function Invoke-MSBuild {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Project,
        [Parameter(Mandatory = $true)]
        [string]$Target,
        [string]$Configuration = 'Release'
    )

    & $msbuild $Project "/t:$Target" `
        "/p:Configuration=$Configuration" '/p:Platform=AnyCPU' `
        '/nologo' '/verbosity:minimal'
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed: $Project target=$Target configuration=$Configuration"
    }
}

Invoke-MSBuild -Project $libraryProject -Target 'Rebuild'
Copy-Item -LiteralPath $sourceDll -Destination $distributionDll -Force

$manualPageCount = & $python -c `
    'from pypdf import PdfReader; import sys; print(len(PdfReader(sys.argv[1]).pages))' `
    $distributionManual
if ($LASTEXITCODE -ne 0 -or [int]$manualPageCount -lt 10) {
    throw 'The edited API user manual PDF is missing or incomplete.'
}

& $python -c `
    'from docx import Document; import sys; d=Document(sys.argv[1]); h=sum(1 for p in d.paragraphs if p.style.name.startswith("Heading ")); assert h >= 30 and len(d.tables) >= 45 and "LASAL Motion Control API" in d.core_properties.title' `
    $distributionManualDocx
if ($LASTEXITCODE -ne 0) {
    throw 'The edited API user manual DOCX is missing or incomplete.'
}

Invoke-MSBuild -Project $exampleProject -Target 'Rebuild'
Invoke-MSBuild -Project $testProject -Target 'RunTests'
Invoke-MSBuild -Project $testProject -Target 'RunLasalNetworkContract'

$exampleExe = Join-Path $exampleOutput 'LasalMotionControlApiExample.exe'
$exampleDll = Join-Path $exampleOutput 'LasalMotionControlLib.dll'
if (-not (Test-Path -LiteralPath $exampleExe) -or
    -not (Test-Path -LiteralPath $exampleDll)) {
    throw 'The distribution example output is incomplete.'
}

New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
Copy-Item -LiteralPath $exampleExe -Destination $runDirectory -Force
Copy-Item -LiteralPath $exampleDll -Destination $runDirectory -Force

$exampleConfig = "$exampleExe.config"
if (Test-Path -LiteralPath $exampleConfig) {
    Copy-Item -LiteralPath $exampleConfig -Destination $runDirectory -Force
}

$sourceHash = (Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
$distributionHash = (
    Get-FileHash -LiteralPath $distributionDll -Algorithm SHA256).Hash
$runtimeDll = Join-Path $runDirectory 'LasalMotionControlLib.dll'
$runtimeHash = (
    Get-FileHash -LiteralPath $runtimeDll -Algorithm SHA256).Hash
if ($sourceHash -ne $distributionHash -or $sourceHash -ne $runtimeHash) {
    throw 'Source, API deliverable, and example runtime DLLs are not byte-identical.'
}

$distributionManualHash = (
    Get-FileHash -LiteralPath $distributionManual -Algorithm SHA256).Hash
$distributionManualDocxHash = (
    Get-FileHash -LiteralPath $distributionManualDocx -Algorithm SHA256).Hash

$textExtensions = @('.csproj', '.sln', '.cs', '.xaml', '.md', '.txt', '.json')
$forbiddenPatterns = @(
    '<ProjectReference\b',
    'LMC_API_Delivery',
    '[A-Za-z]:\\',
    'Codex_',
    'Elmo_API_Packet2',
    'Lasal_PRG',
    'BUILD_METADATA'
)
foreach ($textFile in Get-ChildItem -LiteralPath $distribution -Recurse -File |
    Where-Object {
        $textExtensions -contains $_.Extension.ToLowerInvariant() -and
        $_.FullName -notmatch '[\\/](bin|obj|\.vs)[\\/]'
    }) {
    foreach ($pattern in $forbiddenPatterns) {
        if (Select-String -LiteralPath $textFile.FullName -Pattern $pattern -Quiet) {
            throw "Distribution contains an internal reference: $($textFile.FullName) pattern=$pattern"
        }
    }
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ('LmcApiDistribution-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

try {
    Get-ChildItem -LiteralPath $distribution -Recurse -File |
        Where-Object {
            $_.FullName -notmatch '[\\/](bin|obj|\.vs)[\\/]'
        } |
        ForEach-Object {
            $relative = $_.FullName.Substring($distribution.Length).TrimStart('\')
            $destination = Join-Path $temporaryRoot $relative
            New-Item -ItemType Directory -Path `
                (Split-Path -Parent $destination) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
        }

    $temporaryProject = Join-Path $temporaryRoot `
        '02_Example_Program\LasalApiWpfTestApp\LasalApiWpfTestApp.csproj'
    Invoke-MSBuild -Project $temporaryProject -Target 'Rebuild' `
        -Configuration 'Debug'
    Invoke-MSBuild -Project $temporaryProject -Target 'Rebuild' `
        -Configuration 'Release'
}
finally {
    $resolvedTemp = [System.IO.Path]::GetFullPath($temporaryRoot)
    $systemTemp = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::GetTempPath())
    if ($resolvedTemp.StartsWith(
        $systemTemp,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
    }
}

# Do not leave IDE/build output in the folder that will be zipped and shipped.
$resolvedDistribution = [System.IO.Path]::GetFullPath($distribution).TrimEnd('\')
$generatedDirectories = @(
    Get-ChildItem -LiteralPath $distribution -Recurse -Directory -Force |
        Where-Object { $_.Name -in @('bin', 'obj', '.vs') } |
        Sort-Object FullName -Descending
)
foreach ($generatedDirectory in $generatedDirectories) {
    $resolvedGenerated = [System.IO.Path]::GetFullPath(
        $generatedDirectory.FullName)
    if (-not $resolvedGenerated.StartsWith(
        $resolvedDistribution + '\',
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove output outside distribution: $resolvedGenerated"
    }
    Remove-Item -LiteralPath $resolvedGenerated -Recurse -Force
}

$exampleRunHash = (
    Get-FileHash -LiteralPath `
        (Join-Path $runDirectory 'LasalMotionControlApiExample.exe') `
        -Algorithm SHA256).Hash

$assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($sourceDll)
$versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($sourceDll)
$assemblyVersion = $assemblyName.Version.ToString()
$fileVersion = $versionInfo.FileVersion
$productVersion = $versionInfo.ProductVersion
if ([string]::IsNullOrWhiteSpace($assemblyVersion) -or
    [string]::IsNullOrWhiteSpace($fileVersion) -or
    [string]::IsNullOrWhiteSpace($productVersion)) {
    throw 'Release DLL version metadata is incomplete.'
}

$releaseManifestParameters = @{
    DistributionRoot = $distribution
    CanonicalDllPath = $sourceDll
    DllReplicaRelativePaths = @(
        '01_API/LasalMotionControlLib.dll',
        '02_Example_Program/Run/LasalMotionControlLib.dll'
    )
    SourceCommit = $sourceCommit
    WorktreeState = $worktreeState
    AssemblyVersion = $assemblyVersion
    FileVersion = $fileVersion
    ProductVersion = $productVersion
}
$releaseManifestPath = Write-LmcReleaseManifestAtomic `
    @releaseManifestParameters
$releaseManifestHash = (
    Get-FileHash -LiteralPath $releaseManifestPath -Algorithm SHA256).Hash

Write-Host "Distribution build completed: $distribution"
Write-Host "Deliverables: 01_API, 02_Example_Program, 03_API_User_Manual"
Write-Host "Release manifest verification: PASS ($worktreeState)"
Write-Host "Release manifest SHA256: $releaseManifestHash"
Write-Host "DLL SHA256: $distributionHash"
Write-Host "Example EXE SHA256: $exampleRunHash"
Write-Host "Manual pages: $manualPageCount"
Write-Host "Manual PDF SHA256: $distributionManualHash"
Write-Host "Manual DOCX SHA256: $distributionManualDocxHash"

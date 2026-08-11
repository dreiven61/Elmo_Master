[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [switch]$AllowDirty,
    [string]$CandidatePath,
    [string]$ManualDocxPath,
    [string]$ManualPdfPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

foreach ($implementationName in @(
    'DistributionToolchainProvenance.ps1',
    'ReleaseManifest.ps1',
    'DistributionPipeline.ps1',
    'DistributionSemanticPolicy.ps1',
    'Test-LmcDistributionToolingHostParity.ps1')) {
    $implementationPath = Join-Path $PSScriptRoot $implementationName
    if (-not (Test-Path -LiteralPath $implementationPath -PathType Leaf)) {
        throw "Distribution implementation not found: $implementationPath"
    }
    . $implementationPath
}

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot '..\..'))
}
else {
    $RepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
}

$toolingPreflight = Invoke-LmcDistributionToolingHostParityPreflight `
    -RepositoryRoot $RepositoryRoot
if ($toolingPreflight.Result -cne 'PASS' -or
    $toolingPreflight.HostCount -ne 2 -or
    $toolingPreflight.SuiteCount -ne 7 -or
    $toolingPreflight.RunCount -ne 14 -or
    $toolingPreflight.ToolingDigest -notmatch '^[0-9A-F]{64}$') {
    throw 'Distribution tooling host-parity preflight returned an invalid result.'
}
Write-Host (
    'PASS: bounded distribution tooling host parity 14/14; ' +
    "files=$($toolingPreflight.ToolingFileCount); " +
    "SHA256=$($toolingPreflight.ToolingDigest)")

$libraryProject = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\src\LasalMotionControlLib.csproj'
$testProject = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\tests\LasalMotionControlLib.Tests\LasalMotionControlLib.Tests.csproj'
$wpfSmokeProject = Join-Path $RepositoryRoot `
    'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp.SmokeTests\LasalApiWpfTestApp.SmokeTests.csproj'
$sourceDll = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Delivery\src\bin\Release\LasalMotionControlLib.dll'

$canonicalDistribution = Join-Path $RepositoryRoot `
    'LMC_Library\LMC_API_Distribution'
$distributionParent = Split-Path -Parent $canonicalDistribution
$canonicalManual = Join-Path $canonicalDistribution `
    '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.pdf'
$canonicalManualDocx = Join-Path $canonicalDistribution `
    '03_API_User_Manual\LASAL_Motion_Control_API_User_Manual_KO.docx'
$canonicalSolution = Join-Path $canonicalDistribution `
    '02_Example_Program\LasalApiWpfTestApp.sln'
$manualInputs = Resolve-LmcDistributionManualInputs `
    -RepositoryRoot $RepositoryRoot `
    -CanonicalPdfPath $canonicalManual `
    -CanonicalDocxPath $canonicalManualDocx `
    -ManualPdfPath $ManualPdfPath `
    -ManualDocxPath $ManualDocxPath
$manualPdfInput = $manualInputs.PdfPath
$manualDocxInput = $manualInputs.DocxPath

$developmentExampleRoot = Join-Path $RepositoryRoot `
    'LMC_Library\LasalApiWpfTestApp\LasalApiWpfTestApp'
$developmentExampleProject = Join-Path $developmentExampleRoot `
    'LasalApiWpfTestApp.csproj'
$distributionReadmeTemplate = Join-Path $PSScriptRoot `
    'DistributionREADME.md'
$distributionExampleReadmeTemplate = Join-Path $PSScriptRoot `
    'DistributionExampleREADME.md'
$semanticPolicyImplementation = Join-Path $PSScriptRoot `
    'DistributionSemanticPolicy.ps1'

foreach ($requiredPath in @(
    $libraryProject,
    $testProject,
    $wpfSmokeProject,
    $canonicalDistribution,
    $manualPdfInput,
    $manualDocxInput,
    $canonicalSolution,
    $developmentExampleRoot,
    $developmentExampleProject,
    $distributionReadmeTemplate,
    $distributionExampleReadmeTemplate,
    $semanticPolicyImplementation)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required release input not found: $requiredPath"
    }
}

function Get-LmcProjectSourceEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    [xml]$projectXml = Get-Content -LiteralPath $ProjectPath -Raw
    $namespace = New-Object System.Xml.XmlNamespaceManager(
        $projectXml.NameTable)
    $namespace.AddNamespace('m', $projectXml.Project.NamespaceURI)
    $nodes = $projectXml.SelectNodes(
        '/m:Project/m:ItemGroup/m:ApplicationDefinition | ' +
        '/m:Project/m:ItemGroup/m:Page | ' +
        '/m:Project/m:ItemGroup/m:Compile',
        $namespace)
    foreach ($node in @($nodes)) {
        $metadata = @(
            $node.ChildNodes |
                Where-Object {
                    $_.NodeType -eq [System.Xml.XmlNodeType]::Element
                } |
                ForEach-Object { "$($_.LocalName)=$($_.InnerText)" } |
                Sort-Object
        ) -join ';'
        [pscustomobject]@{
            ItemType = $node.LocalName
            Include = [string]$node.GetAttribute('Include')
            Metadata = $metadata
        }
    }
}

function Get-LmcProjectEntryKey {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Entry
    )

    return "$($Entry.ItemType)|$($Entry.Include)|$($Entry.Metadata)"
}

function Resolve-LmcSafeProjectFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectRoot,
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    if ([string]::IsNullOrWhiteSpace($RelativePath) -or
        [System.IO.Path]::IsPathRooted($RelativePath)) {
        throw "Project source path must be relative: $RelativePath"
    }
    $root = [System.IO.Path]::GetFullPath($ProjectRoot).TrimEnd('\')
    $resolved = [System.IO.Path]::GetFullPath(
        (Join-Path $root $RelativePath))
    if (-not $resolved.StartsWith(
        $root + '\',
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Project source path escapes its root: $RelativePath"
    }
    if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
        throw "Project source file not found: $resolved"
    }
    return $resolved
}

function ConvertTo-LmcDistributionExampleProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceProject,
        [Parameter(Mandatory = $true)]
        [string]$DestinationProject
    )

    [xml]$projectXml = Get-Content -LiteralPath $SourceProject -Raw
    $namespaceUri = $projectXml.Project.NamespaceURI
    $namespace = New-Object System.Xml.XmlNamespaceManager(
        $projectXml.NameTable)
    $namespace.AddNamespace('m', $namespaceUri)

    foreach ($node in @($projectXml.SelectNodes(
        '/m:Project/m:ItemGroup/m:ProjectReference | ' +
        '/m:Project/m:ItemGroup/m:None',
        $namespace))) {
        $null = $node.ParentNode.RemoveChild($node)
    }

    $referenceGroup = $projectXml.SelectSingleNode(
        '/m:Project/m:ItemGroup[m:Reference]',
        $namespace)
    if ($null -eq $referenceGroup) {
        throw 'Development example project has no assembly-reference ItemGroup.'
    }
    $reference = $projectXml.CreateElement('Reference', $namespaceUri)
    $reference.SetAttribute('Include', 'LasalMotionControlLib')
    $hintPath = $projectXml.CreateElement('HintPath', $namespaceUri)
    $hintPath.InnerText = '..\..\01_API\LasalMotionControlLib.dll'
    $private = $projectXml.CreateElement('Private', $namespaceUri)
    $private.InnerText = 'True'
    $null = $reference.AppendChild($hintPath)
    $null = $reference.AppendChild($private)
    $null = $referenceGroup.AppendChild($reference)

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
    $settings.Indent = $true
    $settings.NewLineChars = "`r`n"
    $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
    $writer = [System.Xml.XmlWriter]::Create($DestinationProject, $settings)
    try {
        $projectXml.Save($writer)
    }
    finally {
        $writer.Dispose()
    }
}

function Copy-LmcDevelopmentExample {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DestinationRoot
    )

    New-Item -ItemType Directory -Path $DestinationRoot -Force |
        Out-Null
    $destinationProject = Join-Path $DestinationRoot `
        'LasalApiWpfTestApp.csproj'
    ConvertTo-LmcDistributionExampleProject `
        -SourceProject $developmentExampleProject `
        -DestinationProject $destinationProject

    $developmentEntries = @(
        Get-LmcProjectSourceEntries `
            -ProjectPath $developmentExampleProject)
    foreach ($entry in $developmentEntries) {
        $source = Resolve-LmcSafeProjectFile `
            -ProjectRoot $developmentExampleRoot `
            -RelativePath $entry.Include
        $destination = Join-Path $DestinationRoot $entry.Include
        New-Item -ItemType Directory -Path `
            (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $source -Destination $destination -Force
    }

    $candidateEntries = @(
        Get-LmcProjectSourceEntries -ProjectPath $destinationProject)
    $developmentKeys = @(
        $developmentEntries |
            ForEach-Object { Get-LmcProjectEntryKey -Entry $_ } |
            Sort-Object)
    $candidateKeys = @(
        $candidateEntries |
            ForEach-Object { Get-LmcProjectEntryKey -Entry $_ } |
            Sort-Object)
    $difference = Compare-Object `
        -ReferenceObject $developmentKeys `
        -DifferenceObject $candidateKeys
    if ($difference) {
        throw "Candidate WPF project source metadata drifted:`n$($difference | Out-String)"
    }

    foreach ($entry in $developmentEntries) {
        $source = Resolve-LmcSafeProjectFile `
            -ProjectRoot $developmentExampleRoot `
            -RelativePath $entry.Include
        $destination = Resolve-LmcSafeProjectFile `
            -ProjectRoot $DestinationRoot `
            -RelativePath $entry.Include
        $sourceHash = (
            Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
        $destinationHash = (
            Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
        if ($sourceHash -ne $destinationHash) {
            throw "Candidate WPF source is not byte-identical: $($entry.Include)"
        }
    }

    [xml]$candidateXml = Get-Content -LiteralPath $destinationProject -Raw
    $namespace = New-Object System.Xml.XmlNamespaceManager(
        $candidateXml.NameTable)
    $namespace.AddNamespace('m', $candidateXml.Project.NamespaceURI)
    if ($candidateXml.SelectNodes('//m:ProjectReference', $namespace).Count -ne 0) {
        throw 'Candidate example retained an internal ProjectReference.'
    }
    $binaryReference = $candidateXml.SelectSingleNode(
        '//m:Reference[@Include="LasalMotionControlLib"]',
        $namespace)
    if ($null -eq $binaryReference -or
        $binaryReference.HintPath -ne `
            '..\..\01_API\LasalMotionControlLib.dll' -or
        $binaryReference.Private -ne 'True') {
        throw 'Candidate example binary-reference contract is invalid.'
    }

    return $destinationProject
}

function Get-LmcLasalValidationInputFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,
        [Parameter(Mandatory = $true)]
        [string]$GitPath
    )

    $repository = [System.IO.Path]::GetFullPath(
        $RepositoryRoot).TrimEnd('\')
    if (-not (Test-Path -LiteralPath $repository -PathType Container)) {
        throw "Release repository root not found: $repository"
    }

    $projectRelativeRoot =
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis'
    $projectRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $repository $projectRelativeRoot.Replace('/', '\')))
    if (-not (Test-Path -LiteralPath $projectRoot -PathType Container)) {
        throw "Canonical LASAL project directory not found: $projectRoot"
    }
    $currentProjectPath = Get-Item -LiteralPath $projectRoot -Force
    while ($true) {
        if (Test-LmcDistributionReparsePoint -Item $currentProjectPath) {
            throw (
                'LASAL validation input path traverses a reparse point: ' +
                $projectRoot)
        }
        if ($currentProjectPath.FullName.Equals(
            $repository,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }
        $currentProjectPath = $currentProjectPath.Parent
        if ($null -eq $currentProjectPath) {
            throw "LASAL validation input path escaped the repository: $projectRoot"
        }
    }
    Assert-LmcDistributionTreeHasNoReparsePoints `
        -Root $projectRoot `
        -Context 'LASAL validation input tree'
    $inventoryRoots = @(
        "$projectRelativeRoot/Class",
        "$projectRelativeRoot/Network",
        "$projectRelativeRoot/Include",
        "$projectRelativeRoot/Source")
    $projectDefinitionRelativePath =
        "$projectRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcp"
    $projectDatabaseRelativePath =
        "$projectRelativeRoot/Elmo_EtherCAT_Test_4Axis.lcb"
    $networkRelativeRoot = "$projectRelativeRoot/Network"
    $pathSpecs = @(
        $inventoryRoots +
        $projectDefinitionRelativePath +
        $projectDatabaseRelativePath)
    $relevantExtensions = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($extension in @(
        '.st', '.h', '.c', '.cpp', '.lcb', '.lcn', '.lcp', '.xml')) {
        $null = $relevantExtensions.Add($extension)
    }

    function Invoke-LmcLasalGitInventoryQuery {
        param(
            [string[]]$QueryArguments
        )

        $gitArguments = @(
            '-c', 'core.quotepath=false',
            '-C', $repository,
            'ls-files') + @($QueryArguments) + @('--') + $pathSpecs
        $queryOutput = @(& $GitPath @gitArguments 2>&1)
        $queryExitCode = $LASTEXITCODE
        if ($queryExitCode -ne 0) {
            throw (
                'Git LASAL release input query failed with exit code ' +
                "${queryExitCode}: $($queryOutput -join [Environment]::NewLine)")
        }
        return @($queryOutput | ForEach-Object { [string]$_ })
    }

    function Resolve-LmcLasalInventoryPath {
        param(
            [Parameter(Mandatory = $true)]
            [string]$RelativePath
        )

        $normalized = $RelativePath.Replace('\', '/')
        if ([string]::IsNullOrWhiteSpace($normalized) -or
            [System.IO.Path]::IsPathRooted($normalized)) {
            throw "Git returned an invalid LASAL release input path: $RelativePath"
        }
        $fullPath = [System.IO.Path]::GetFullPath(
            (Join-Path $repository $normalized.Replace('/', '\')))
        if (-not $fullPath.StartsWith(
            $repository + '\',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Git LASAL release input escaped the repository: $RelativePath"
        }
        return [pscustomobject]@{
            RelativePath = $normalized
            FullPath = $fullPath
        }
    }

    function Test-LmcLasalInventoryPathRelevant {
        param(
            [Parameter(Mandatory = $true)]
            [string]$RelativePath
        )

        if ($RelativePath.Equals(
            $projectDefinitionRelativePath,
            [System.StringComparison]::OrdinalIgnoreCase) -or
            $RelativePath.Equals(
            $projectDatabaseRelativePath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
        $underInventoryRoot = $false
        foreach ($inventoryRoot in $inventoryRoots) {
            if ($RelativePath.StartsWith(
                $inventoryRoot + '/',
                [System.StringComparison]::OrdinalIgnoreCase)) {
                $underInventoryRoot = $true
                break
            }
        }
        if (-not $underInventoryRoot) {
            return $false
        }
        return $relevantExtensions.Contains(
            [System.IO.Path]::GetExtension($RelativePath))
    }

    $inventoryFiles = New-Object `
        'System.Collections.Generic.Dictionary[string,string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    $projectDefinitionTracked = $false
    $projectDatabaseTracked = $false
    foreach ($trackedPath in @(Invoke-LmcLasalGitInventoryQuery)) {
        $resolved = Resolve-LmcLasalInventoryPath -RelativePath $trackedPath
        $trackedNetworkPath = $resolved.RelativePath.StartsWith(
            $networkRelativeRoot + '/',
            [System.StringComparison]::OrdinalIgnoreCase)
        if (-not $trackedNetworkPath -and
            -not (Test-LmcLasalInventoryPathRelevant `
                -RelativePath $resolved.RelativePath)) {
            continue
        }
        if (-not (Test-Path -LiteralPath $resolved.FullPath -PathType Leaf)) {
            throw "Tracked LASAL release input not found: $($resolved.FullPath)"
        }
        $inventoryFiles[$resolved.RelativePath] = $resolved.FullPath
        if ($resolved.RelativePath.Equals(
            $projectDefinitionRelativePath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $projectDefinitionTracked = $true
        }
        elseif ($resolved.RelativePath.Equals(
            $projectDatabaseRelativePath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $projectDatabaseTracked = $true
        }
    }
    if (-not $projectDefinitionTracked) {
        throw (
            'Tracked LASAL project definition input not found: ' +
            $projectDefinitionRelativePath)
    }
    if (-not $projectDatabaseTracked) {
        throw (
            'Tracked LASAL project database input not found: ' +
            $projectDatabaseRelativePath)
    }
    if ($inventoryFiles.Count -eq 0) {
        throw 'Git returned no tracked LASAL validation inputs.'
    }

    $networkRoot = Join-Path $repository `
        $networkRelativeRoot.Replace('/', '\')
    if (-not (Test-Path -LiteralPath $networkRoot -PathType Container)) {
        throw "Canonical LASAL Network directory not found: $networkRoot"
    }
    foreach ($networkFile in @(
        Get-ChildItem -LiteralPath $networkRoot -Recurse -Force -File |
            Sort-Object FullName)) {
        $fullNetworkPath = [System.IO.Path]::GetFullPath(
            $networkFile.FullName)
        if (-not $fullNetworkPath.StartsWith(
            $repository + '\',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw (
                'LASAL Network validation input escaped the repository: ' +
                $fullNetworkPath)
        }
        $relativeNetworkPath = $fullNetworkPath.Substring(
            $repository.Length + 1).Replace('\', '/')
        $inventoryFiles[$relativeNetworkPath] = $fullNetworkPath
    }

    $untrackedRelevant = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($queryArguments in @(
        @('--others', '--exclude-standard'),
        @('--others', '--ignored', '--exclude-standard'))) {
        foreach ($untrackedPath in @(
            Invoke-LmcLasalGitInventoryQuery `
                -QueryArguments $queryArguments)) {
            $resolved = Resolve-LmcLasalInventoryPath `
                -RelativePath $untrackedPath
            if (Test-LmcLasalInventoryPathRelevant `
                -RelativePath $resolved.RelativePath) {
                $null = $untrackedRelevant.Add($resolved.RelativePath)
            }
        }
    }
    if ($untrackedRelevant.Count -ne 0) {
        $inventory = @($untrackedRelevant | Sort-Object) -join ', '
        throw "Untracked LASAL validation input is not allowed: $inventory"
    }

    return @(Get-LmcDistributionOrdinalSortedUniqueStrings `
        -Values @($inventoryFiles.Values) `
        -IgnoreCaseForUniqueness)
}

function Get-LmcReleaseInputFiles {
    param(
        [Parameter(Mandatory = $true)]
        [string]$GitPath
    )

    $files = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)

    function Add-InputFile {
        param([string]$Path)
        $fullPath = [System.IO.Path]::GetFullPath($Path)
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Release input file not found: $fullPath"
        }
        $null = $files.Add($fullPath)
    }

    foreach ($path in @(
        $libraryProject,
        $developmentExampleProject,
        $manualPdfInput,
        $manualDocxInput,
        $canonicalSolution,
        $distributionReadmeTemplate,
        $distributionExampleReadmeTemplate,
        (Join-Path $PSScriptRoot 'Build-LmcApiDistribution.ps1'),
        (Join-Path $PSScriptRoot 'DistributionPipeline.ps1'),
        (Join-Path $PSScriptRoot 'DistributionSemanticPolicy.ps1'),
        (Join-Path $PSScriptRoot `
            'DistributionToolchainProvenance.ps1'),
        (Join-Path $PSScriptRoot 'ReleaseManifest.ps1'),
        (Join-Path $PSScriptRoot `
            'Test-LmcDistributionToolingHostParity.ps1'),
        (Join-Path $PSScriptRoot `
            'Test-LmcApiDistributionPipeline.ps1'),
        (Join-Path $PSScriptRoot `
            'Test-LmcDistributionSemanticPolicy.ps1'),
        (Join-Path $PSScriptRoot `
            'Test-LmcDistributionToolchainProvenance.ps1'),
        (Join-Path $PSScriptRoot 'Test-LmcReleaseManifest.ps1'),
        (Join-Path $PSScriptRoot 'API_USER_MANUAL_KO.md'),
        (Join-Path $RepositoryRoot `
            'LMC_Library\LMC_API_Delivery\docs\DINT_PACKET_MAP.txt'))) {
        Add-InputFile -Path $path
    }

    foreach ($path in @(Get-LmcLasalValidationInputFiles `
        -RepositoryRoot $RepositoryRoot `
        -GitPath $GitPath)) {
        Add-InputFile -Path $path
    }

    foreach ($project in @($libraryProject, $developmentExampleProject)) {
        $projectRoot = Split-Path -Parent $project
        foreach ($entry in @(Get-LmcProjectSourceEntries -ProjectPath $project)) {
            Add-InputFile -Path (Resolve-LmcSafeProjectFile `
                -ProjectRoot $projectRoot `
                -RelativePath $entry.Include)
        }
    }

    foreach ($testRoot in @(
        (Split-Path -Parent $testProject),
        (Split-Path -Parent $wpfSmokeProject))) {
        foreach ($file in Get-ChildItem -LiteralPath $testRoot -Recurse -File |
            Where-Object {
                $_.FullName -notmatch '[\\/](bin|obj|\.vs)[\\/]' -and
                $_.Extension.ToLowerInvariant() -in @(
                    '.cs', '.csproj', '.ps1', '.json')
            }) {
            Add-InputFile -Path $file.FullName
        }
    }

    return @(Get-LmcDistributionOrdinalSortedUniqueStrings `
        -Values @($files) `
        -IgnoreCaseForUniqueness)
}

function Get-LmcReleaseInputTreeSha256 {
    param(
        [AllowNull()]
        [object]$ManualInputSnapshot,

        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9A-F]{64}$')]
        [string]$ValidatedToolingDigest,

        [Parameter(Mandatory = $true)]
        [string]$GitPath
    )

    if ($null -eq $ManualInputSnapshot) {
        $liveManualSnapshot = New-LmcDistributionManualInputSnapshot `
            -RepositoryRoot $RepositoryRoot `
            -PdfPath $manualPdfInput `
            -DocxPath $manualDocxInput
        return Get-LmcReleaseInputTreeSha256 `
            -ManualInputSnapshot $liveManualSnapshot `
            -ValidatedToolingDigest $ValidatedToolingDigest `
            -GitPath $GitPath
    }
    else {
        foreach ($propertyName in @(
            'PdfPath', 'PdfBytes', 'PdfLength', 'PdfSha256',
            'DocxPath', 'DocxBytes', 'DocxLength', 'DocxSha256')) {
            if ($ManualInputSnapshot.PSObject.Properties.Name -notcontains
                $propertyName) {
                throw "Manual input snapshot is missing $propertyName."
            }
        }
        if (-not $ManualInputSnapshot.PdfPath.Equals(
            $manualPdfInput,
            [System.StringComparison]::OrdinalIgnoreCase) -or
            -not $ManualInputSnapshot.DocxPath.Equals(
                $manualDocxInput,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Manual input snapshot paths do not match the selected inputs.'
        }
        $snapshotPdfHash = Get-LmcDistributionBytesSha256 `
            -Bytes $ManualInputSnapshot.PdfBytes
        $snapshotDocxHash = Get-LmcDistributionBytesSha256 `
            -Bytes $ManualInputSnapshot.DocxBytes
        if ($ManualInputSnapshot.PdfLength -ne
                $ManualInputSnapshot.PdfBytes.LongLength -or
            $ManualInputSnapshot.DocxLength -ne
                $ManualInputSnapshot.DocxBytes.LongLength -or
            -not $snapshotPdfHash.Equals(
                $ManualInputSnapshot.PdfSha256,
                [System.StringComparison]::OrdinalIgnoreCase) -or
            -not $snapshotDocxHash.Equals(
                $ManualInputSnapshot.DocxSha256,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Manual input snapshot bytes do not match its descriptor.'
        }
    }

    $repositoryPrefix = $RepositoryRoot.TrimEnd('\') + '\'
    $records = @()
    foreach ($file in @(Get-LmcReleaseInputFiles -GitPath $GitPath)) {
        if (-not $file.StartsWith(
            $repositoryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Release input escaped the repository: $file"
        }
        $relative = $file.Substring($repositoryPrefix.Length).Replace('\', '/')
        if ($null -ne $ManualInputSnapshot -and $file.Equals(
            $ManualInputSnapshot.PdfPath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $length = $ManualInputSnapshot.PdfLength
            $hash = $ManualInputSnapshot.PdfSha256
        }
        elseif ($null -ne $ManualInputSnapshot -and $file.Equals(
            $ManualInputSnapshot.DocxPath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            $length = $ManualInputSnapshot.DocxLength
            $hash = $ManualInputSnapshot.DocxSha256
        }
        else {
            $item = Get-Item -LiteralPath $file
            $length = $item.Length
            $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
        }
        $records += "$relative|$length|$($hash.ToUpperInvariant())"
    }
    $records += "@validated-tooling-preflight|$ValidatedToolingDigest"
    $canonical = ($records -join "`n") + "`n"
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($canonical)
        return ([System.BitConverter]::ToString(
            $sha.ComputeHash($bytes))).Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function Get-LmcReleaseTransactionFingerprint {
    param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9A-F]{64}$')]
        [string]$InputTreeSha256,
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9A-F]{64}$')]
        [string]$ToolchainSha256,
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[0-9A-F]{64}$')]
        [string]$ToolingPreflightSha256
    )

    $canonical = @(
        "InputTree|$InputTreeSha256",
        "Toolchain|$ToolchainSha256",
        "ToolingPreflight|$ToolingPreflightSha256") -join "`n"
    return Get-LmcDistributionProvenanceTextSha256 `
        -Text ($canonical + "`n")
}

function Remove-LmcCandidateBuildOutputs {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CandidateRoot
    )

    $root = [System.IO.Path]::GetFullPath($CandidateRoot).TrimEnd('\')
    $generatedDirectories = @(
        Get-ChildItem -LiteralPath $root -Recurse -Directory -Force |
            Where-Object { $_.Name -in @('bin', 'obj', '.vs') } |
            Sort-Object FullName -Descending
    )
    foreach ($generatedDirectory in $generatedDirectories) {
        $resolved = [System.IO.Path]::GetFullPath(
            $generatedDirectory.FullName)
        if (-not $resolved.StartsWith(
            $root + '\',
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove output outside candidate: $resolved"
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}

function Assert-LmcCandidateStructure {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CandidateRoot,
        [switch]$ManifestExpected
    )

    $expectedDirectories = @(
        '01_API',
        '02_Example_Program',
        '03_API_User_Manual')
    $actualDirectories = @(
        Get-ChildItem -LiteralPath $CandidateRoot -Directory |
            Select-Object -ExpandProperty Name |
            Sort-Object)
    if (Compare-Object `
        -ReferenceObject $expectedDirectories `
        -DifferenceObject $actualDirectories) {
        throw 'Candidate must contain exactly the three numbered deliverable directories.'
    }

    $expectedTopFiles = @('README.md')
    if ($ManifestExpected) {
        $expectedTopFiles += 'RELEASE_MANIFEST.md'
    }
    $actualTopFiles = @(
        Get-ChildItem -LiteralPath $CandidateRoot -File |
            Select-Object -ExpandProperty Name |
            Sort-Object)
    if (Compare-Object `
        -ReferenceObject @($expectedTopFiles | Sort-Object) `
        -DifferenceObject $actualTopFiles) {
        throw 'Candidate top-level file set is invalid.'
    }

    $manualDirectory = Join-Path $CandidateRoot '03_API_User_Manual'
    $expectedManualFiles = @(
        'LASAL_Motion_Control_API_User_Manual_KO.docx',
        'LASAL_Motion_Control_API_User_Manual_KO.pdf')
    $actualManualFiles = @(
        Get-ChildItem -LiteralPath $manualDirectory -File |
            Select-Object -ExpandProperty Name |
            Sort-Object)
    if (Compare-Object `
        -ReferenceObject $expectedManualFiles `
        -DifferenceObject $actualManualFiles) {
        throw 'Candidate manual directory file set is invalid.'
    }

    if (@(Get-ChildItem -LiteralPath $CandidateRoot -Recurse -Directory -Force |
        Where-Object { $_.Name -in @('bin', 'obj', '.vs') }).Count -ne 0) {
        throw 'Candidate contains generated bin, obj, or .vs directories.'
    }
}

function Assert-LmcCandidateNoInternalReferences {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CandidateRoot
    )

    $textExtensions = @(
        '.csproj', '.sln', '.cs', '.xaml', '.config',
        '.md', '.txt', '.json')
    $forbiddenPatterns = @(
        '<ProjectReference\b',
        'LMC_API_Delivery',
        '[A-Za-z]:\\',
        'Codex_',
        'Elmo_API_Packet2',
        'Lasal_PRG',
        'BUILD_METADATA')
    foreach ($textFile in Get-ChildItem `
        -LiteralPath $CandidateRoot -Recurse -File |
        Where-Object {
            $textExtensions -contains $_.Extension.ToLowerInvariant()
        }) {
        foreach ($pattern in $forbiddenPatterns) {
            if (Select-String -LiteralPath $textFile.FullName `
                -Pattern $pattern -Quiet) {
                throw "Candidate contains an internal reference: $($textFile.FullName) pattern=$pattern"
            }
        }
    }
}

function Invoke-LmcMSBuild {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath,
        [Parameter(Mandatory = $true)]
        [string]$CSharpCompilerPath,
        [Parameter(Mandatory = $true)]
        [string]$Project,
        [Parameter(Mandatory = $true)]
        [string]$Target,
        [string]$Configuration = 'Release',
        [string]$Platform = 'AnyCPU',
        [hashtable]$AdditionalProperties = @{},
        [string]$CompilerBindingTargetsPath
    )

    $compilerPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path $CSharpCompilerPath `
        -Context 'release MSBuild C# compiler'
    $compilerDirectory = Split-Path -Parent $compilerPath
    $compilerCoreTargets = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path (Join-Path $compilerDirectory `
            'Microsoft.CSharp.Core.targets') `
        -Context 'release MSBuild C# core targets'
    $arguments = @(
        $Project,
        "/t:$Target",
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform",
        "/p:CscToolPath=$compilerDirectory",
        "/p:CscToolExe=$([System.IO.Path]::GetFileName($compilerPath))",
        "/p:RoslynTargetsPath=$compilerDirectory",
        "/p:CSharpCoreTargetsPath=$compilerCoreTargets",
        '/p:UseSharedCompilation=false',
        '/nologo',
        '/verbosity:minimal'
    )
    if (-not [string]::IsNullOrWhiteSpace($CompilerBindingTargetsPath)) {
        $bindingTargets = Resolve-LmcDistributionProvenancePhysicalFile `
            -Path $CompilerBindingTargetsPath `
            -Context 'release MSBuild compiler binding targets'
        $arguments += "/p:CustomAfterMicrosoftCSharpTargets=$bindingTargets"
    }
    $reservedCompilerProperties = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($reservedCompilerProperty in @(
            'CscToolPath', 'CscToolExe', 'RoslynTargetsPath',
            'CSharpCoreTargetsPath', 'UseSharedCompilation',
            'CustomAfterMicrosoftCSharpTargets')) {
        $null = $reservedCompilerProperties.Add($reservedCompilerProperty)
    }
    $propertyNames = [string[]]@($AdditionalProperties.Keys | ForEach-Object {
        [string]$_
    })
    $propertyNameSet = New-Object `
        'System.Collections.Generic.HashSet[string]' `
        ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($propertyName in $propertyNames) {
        if (-not $propertyNameSet.Add($propertyName)) {
            throw "MSBuild property name is duplicated by case: $propertyName"
        }
    }
    [System.Array]::Sort(
        $propertyNames,
        [System.StringComparer]::Ordinal)
    foreach ($propertyName in $propertyNames) {
        if ([string]::IsNullOrWhiteSpace([string]$propertyName) -or
            ([string]$propertyName) -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
            throw "Invalid MSBuild property name: $propertyName"
        }
        $propertyValue = [string]$AdditionalProperties[$propertyName]
        if ([string]::IsNullOrWhiteSpace($propertyValue)) {
            throw "MSBuild property value is empty: $propertyName"
        }
        if ($reservedCompilerProperties.Contains(
                [string]$propertyName)) {
            throw "MSBuild compiler property override is forbidden: $propertyName"
        }
        $arguments += "/p:$propertyName=$propertyValue"
    }

    & $MSBuildPath @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed: $Project target=$Target configuration=$Configuration"
    }
}

function Assert-LmcMSBuildCompilerBinding {
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath,
        [Parameter(Mandatory = $true)]
        [string]$CSharpCompilerPath,
        [Parameter(Mandatory = $true)]
        [string]$ProbeRoot,
        [Parameter(Mandatory = $true)]
        [string[]]$Projects
    )

    $compilerPath = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path $CSharpCompilerPath `
        -Context 'release MSBuild binding compiler'
    $compilerDirectory = Split-Path -Parent $compilerPath
    $compilerExe = [System.IO.Path]::GetFileName($compilerPath)
    $compilerCoreTargets = Resolve-LmcDistributionProvenancePhysicalFile `
        -Path (Join-Path $compilerDirectory `
            'Microsoft.CSharp.Core.targets') `
        -Context 'release MSBuild binding core targets'
    $physicalProbeRoot = Resolve-LmcDistributionProvenancePhysicalDirectory `
        -Path $ProbeRoot `
        -Context 'release MSBuild binding probe root'
    $probeName = '.lmc-msbuild-binding-' +
        [System.Guid]::NewGuid().ToString('N')
    $probeDirectory = [System.IO.Path]::GetFullPath(
        (Join-Path $physicalProbeRoot $probeName))
    if (-not $probeDirectory.StartsWith(
            $physicalProbeRoot.TrimEnd('\') + '\',
            [System.StringComparison]::OrdinalIgnoreCase) -or
        [System.IO.Path]::GetFileName($probeDirectory) -notmatch
            '^\.lmc-msbuild-binding-[0-9a-f]{32}$' -or
        (Test-Path -LiteralPath $probeDirectory)) {
        throw 'Release MSBuild compiler binding probe path is unsafe.'
    }

    $probeTargets = Join-Path $probeDirectory 'CompilerBinding.targets'
    $probeOutput = Join-Path $probeDirectory 'binding.txt'
    $probeTemplate = @'
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Target Name="VerifyLmcCompilerBinding">
    <Error Condition="'$(CscToolPath)' != '{0}'" Text="CscToolPath was not provenance-pinned." />
    <Error Condition="'$(CscToolExe)' != '{1}'" Text="CscToolExe was not provenance-pinned." />
    <Error Condition="'$(RoslynTargetsPath)' != '{0}'" Text="RoslynTargetsPath was not provenance-pinned." />
    <Error Condition="'$(CSharpCoreTargetsPath)' != '{2}'" Text="CSharpCoreTargetsPath was not provenance-pinned." />
    <Error Condition="'$(UseSharedCompilation)' != 'false'" Text="UseSharedCompilation was not disabled." />
    <WriteLinesToFile File="$(ProbeOutputPath)" Overwrite="false" Lines="$(MSBuildProjectFullPath)|$(CscToolPath)|$(CscToolExe)|$(RoslynTargetsPath)|$(CSharpCoreTargetsPath)|$(UseSharedCompilation)" />
  </Target>
</Project>
'@
    $probeText = [string]::Format(
        [System.Globalization.CultureInfo]::InvariantCulture,
        $probeTemplate,
        [System.Security.SecurityElement]::Escape($compilerDirectory),
        [System.Security.SecurityElement]::Escape($compilerExe),
        [System.Security.SecurityElement]::Escape($compilerCoreTargets))
    try {
        New-Item -ItemType Directory -Path $probeDirectory | Out-Null
        [System.IO.File]::WriteAllText(
            $probeTargets,
            $probeText,
            (New-Object System.Text.UTF8Encoding($false)))
        $projectSet = New-Object `
            'System.Collections.Generic.HashSet[string]' `
            ([System.StringComparer]::OrdinalIgnoreCase)
        $physicalProjects = @()
        foreach ($project in @($Projects)) {
            $physicalProject = Resolve-LmcDistributionProvenancePhysicalFile `
                -Path $project `
                -Context 'release MSBuild compiler binding project'
            if (-not $projectSet.Add($physicalProject)) {
                throw 'Release MSBuild compiler binding project is duplicated.'
            }
            $physicalProjects += $physicalProject
        }
        if ($physicalProjects.Count -eq 0) {
            throw 'Release MSBuild compiler binding project inventory is empty.'
        }
        foreach ($physicalProject in $physicalProjects) {
            Invoke-LmcMSBuild `
                -MSBuildPath $MSBuildPath `
                -CSharpCompilerPath $compilerPath `
                -Project $physicalProject `
                -Target 'VerifyLmcCompilerBinding' `
                -AdditionalProperties @{
                    ProbeOutputPath = $probeOutput
                } `
                -CompilerBindingTargetsPath $probeTargets
        }
        if (-not (Test-Path -LiteralPath $probeOutput -PathType Leaf)) {
            throw 'MSBuild compiler binding probe emitted no evidence.'
        }
        $evidenceLines = @(
            [System.IO.File]::ReadAllLines($probeOutput) | Where-Object {
                -not [string]::IsNullOrWhiteSpace($_)
            })
        if ($evidenceLines.Count -ne $physicalProjects.Count) {
            throw 'MSBuild compiler binding probe evidence count is invalid.'
        }
        for ($index = 0; $index -lt $physicalProjects.Count; $index++) {
            $expected = @(
                $physicalProjects[$index],
                $compilerDirectory,
                $compilerExe,
                $compilerDirectory,
                $compilerCoreTargets,
                'false') -join '|'
            if (-not ([string]$evidenceLines[$index]).Equals(
                    $expected,
                    [System.StringComparison]::Ordinal)) {
                throw 'MSBuild compiler binding probe evidence did not match the attested compiler.'
            }
        }
    }
    finally {
        if (Test-Path -LiteralPath $probeDirectory -PathType Container) {
            [System.IO.Directory]::Delete($probeDirectory, $true)
        }
    }
}

function Get-LmcReleaseBuildMetadata {
    param(
        [Parameter(Mandatory = $true)]
        [string]$GitPath,
        [string[]]$IgnoredRootPaths = @()
    )

    $gitStatus = @(& $GitPath -C $RepositoryRoot status `
        --porcelain=v1 --untracked-files=all)
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to read Git status.'
    }

    $repositoryPrefix = $RepositoryRoot.TrimEnd('\') + '\'
    $ignoredRelativePaths = @()
    $transactionLockPath = Join-Path $distributionParent `
        '.LMC_API_Distribution.transaction.lock'
    foreach ($ignoredRootPath in @(
        @($IgnoredRootPaths) + @($transactionLockPath))) {
        if ([string]::IsNullOrWhiteSpace($ignoredRootPath)) {
            continue
        }
        $ignoredFullPath = [System.IO.Path]::GetFullPath(
            $ignoredRootPath).TrimEnd('\')
        if (-not $ignoredFullPath.StartsWith(
            $repositoryPrefix,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Ignored Git status root escaped the repository: $ignoredFullPath"
        }
        $ignoredRelativePaths += $ignoredFullPath.Substring(
            $repositoryPrefix.Length).Replace('\', '/').TrimEnd('/')
    }
    if ($ignoredRelativePaths.Count -gt 0) {
        $gitStatus = @($gitStatus | Where-Object {
            $line = [string]$_
            if ($line.Length -lt 4 -or $line[2] -ne ' ') {
                return $true
            }
            $statusPath = $line.Substring(3).Replace('\', '/')
            foreach ($ignoredRelativePath in $ignoredRelativePaths) {
                if ($statusPath.Equals(
                        $ignoredRelativePath,
                        [System.StringComparison]::OrdinalIgnoreCase) -or
                    $statusPath.StartsWith(
                        $ignoredRelativePath + '/',
                        [System.StringComparison]::OrdinalIgnoreCase)) {
                    return $false
                }
            }
            return $true
        })
    }

    $manifestRepositoryRelativePath = `
        'LMC_Library/LMC_API_Distribution/RELEASE_MANIFEST.md'
    $releaseInputGitStatus = @(Get-LmcReleaseManifestInputGitStatus `
        -GitStatus $gitStatus `
        -ManifestRepositoryRelativePath $manifestRepositoryRelativePath)
    $worktreeState = Get-LmcReleaseManifestWorktreeState `
        -GitStatus $releaseInputGitStatus `
        -AllowDirty:$AllowDirty
    $worktreeState = Get-LmcDistributionManualWorktreeState `
        -UsesCanonicalInputs $manualInputs.UsesCanonicalInputs `
        -WorktreeState $worktreeState `
        -AllowDirty:$AllowDirty

    $sourceCommitOutput = @(
        & $GitPath -C $RepositoryRoot rev-parse --verify HEAD)
    if ($LASTEXITCODE -ne 0 -or $sourceCommitOutput.Count -ne 1) {
        throw 'Unable to resolve the source commit for the release manifest.'
    }
    return [pscustomobject]@{
        SourceCommit = ([string]$sourceCommitOutput[0]).Trim()
        WorktreeState = $worktreeState
    }
}

function New-LmcReleasePreparedInputs {
    Assert-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -ExpectedSnapshot $toolingPreflight | Out-Null
    $preparedToolchain = Get-LmcDistributionReleaseToolchainSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -ToolingPreflight $toolingPreflight
    $preparedInputs = New-LmcDistributionManualInputSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -PdfPath $manualPdfInput `
        -DocxPath $manualDocxInput
    $metadata = Get-LmcReleaseBuildMetadata `
        -GitPath $preparedToolchain.RuntimePaths.Git
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name SourceCommit -Value $metadata.SourceCommit
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name WorktreeState -Value $metadata.WorktreeState
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name ValidatedToolingDigest `
        -Value $toolingPreflight.ToolingDigest
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name ValidatedToolingSnapshot `
        -Value $toolingPreflight
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name ToolchainSnapshot `
        -Value $preparedToolchain
    $inputTreeSha256 = Get-LmcReleaseInputTreeSha256 `
        -ManualInputSnapshot $preparedInputs `
        -ValidatedToolingDigest $toolingPreflight.ToolingDigest `
        -GitPath $preparedToolchain.RuntimePaths.Git
    $preparedInputs | Add-Member -MemberType NoteProperty `
        -Name InputTreeSha256 `
        -Value $inputTreeSha256
    return $preparedInputs
}

function Assert-LmcReleasePreparedMetadataCurrent {
    param(
        [Parameter(Mandatory = $true)]
        [object]$PreparedInputs,
        [Parameter(Mandatory = $true)]
        [string]$StagingRoot
    )

    $currentToolchain = Get-LmcDistributionReleaseToolchainSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -ToolingPreflight $toolingPreflight
    if (-not $PreparedInputs.ToolchainSnapshot.ToolchainSha256.Equals(
            $currentToolchain.ToolchainSha256,
            [System.StringComparison]::Ordinal) -or
        -not $PreparedInputs.ToolchainSnapshot.ToolingPreflightSha256.Equals(
            $currentToolchain.ToolingPreflightSha256,
            [System.StringComparison]::Ordinal)) {
        throw "Release toolchain provenance changed before promotion. expected=$($PreparedInputs.ToolchainSnapshot.ToolchainSha256) actual=$($currentToolchain.ToolchainSha256)"
    }
    $current = Get-LmcReleaseBuildMetadata `
        -GitPath $currentToolchain.RuntimePaths.Git `
        -IgnoredRootPaths @($StagingRoot)
    if (-not [string]::Equals(
            $PreparedInputs.SourceCommit,
            $current.SourceCommit,
            [System.StringComparison]::Ordinal) -or
        -not [string]::Equals(
            $PreparedInputs.WorktreeState,
            $current.WorktreeState,
            [System.StringComparison]::Ordinal)) {
        throw "Release Git metadata changed before promotion. expectedCommit=$($PreparedInputs.SourceCommit) actualCommit=$($current.SourceCommit) expectedState=$($PreparedInputs.WorktreeState) actualState=$($current.WorktreeState)"
    }
    Assert-LmcDistributionMonitoredFileSnapshot `
        -RepositoryRoot $RepositoryRoot `
        -ExpectedSnapshot $PreparedInputs.ValidatedToolingSnapshot |
        Out-Null
}

Assert-LmcDistributionMonitoredFileSnapshot `
    -RepositoryRoot $RepositoryRoot `
    -ExpectedSnapshot $toolingPreflight | Out-Null

if ([string]::IsNullOrWhiteSpace($CandidatePath)) {
    $releaseId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' +
        [System.Guid]::NewGuid().ToString('N').Substring(0, 8)
    $CandidatePath = Join-Path $distributionParent `
        ('LMC_API_Distribution_candidate_' + $releaseId)
}
else {
    $CandidatePath = [System.IO.Path]::GetFullPath($CandidatePath)
}

$buildSummary = @{}
$transaction = Invoke-LmcDistributionCandidateTransaction `
    -CanonicalRoot $canonicalDistribution `
    -CandidatePath $CandidatePath `
    -PrepareInputs {
        New-LmcReleasePreparedInputs
    } `
    -GetInputFingerprint {
        param($preparedInputs)
        $selectedToolingSnapshot = if ($null -eq $preparedInputs) {
            $toolingPreflight
        }
        else {
            $preparedInputs.ValidatedToolingSnapshot
        }
        Assert-LmcDistributionMonitoredFileSnapshot `
            -RepositoryRoot $RepositoryRoot `
            -ExpectedSnapshot $selectedToolingSnapshot |
            Out-Null
        $selectedToolchain = if ($null -eq $preparedInputs) {
            Get-LmcDistributionReleaseToolchainSnapshot `
                -RepositoryRoot $RepositoryRoot `
                -ToolingPreflight $toolingPreflight
        }
        else {
            $preparedInputs.ToolchainSnapshot
        }
        $inputTreeSha256 = Get-LmcReleaseInputTreeSha256 `
            -ManualInputSnapshot $preparedInputs `
            -ValidatedToolingDigest `
                $selectedToolingSnapshot.Digest `
            -GitPath $selectedToolchain.RuntimePaths.Git
        if ($null -ne $preparedInputs -and
            -not $preparedInputs.InputTreeSha256.Equals(
                $inputTreeSha256,
                [System.StringComparison]::Ordinal)) {
            throw 'Prepared release input tree changed before transaction fingerprinting.'
        }
        Get-LmcReleaseTransactionFingerprint `
            -InputTreeSha256 $inputTreeSha256 `
            -ToolchainSha256 $selectedToolchain.ToolchainSha256 `
            -ToolingPreflightSha256 `
                $selectedToolchain.ToolingPreflightSha256
    } `
    -ValidatePreparedInputs {
        param($preparedInputs, $stagingRoot)
        Assert-LmcReleasePreparedMetadataCurrent `
            -PreparedInputs $preparedInputs `
            -StagingRoot $stagingRoot
    } `
    -PopulateAndValidate {
        param($stagingRoot, $transactionInputFingerprint, $preparedInputs)

        $releaseMSBuild = `
            $preparedInputs.ToolchainSnapshot.RuntimePaths.MSBuild
        $releaseCSharpCompiler = `
            $preparedInputs.ToolchainSnapshot.RuntimePaths.CSharpCompiler
        $releasePython = `
            $preparedInputs.ToolchainSnapshot.RuntimePaths.Python

        $apiDirectory = Join-Path $stagingRoot '01_API'
        $exampleDirectory = Join-Path $stagingRoot '02_Example_Program'
        $exampleProjectRoot = Join-Path $exampleDirectory `
            'LasalApiWpfTestApp'
        $runDirectory = Join-Path $exampleDirectory 'Run'
        $manualDirectory = Join-Path $stagingRoot '03_API_User_Manual'
        foreach ($directory in @(
            $apiDirectory,
            $exampleDirectory,
            $exampleProjectRoot,
            $runDirectory,
            $manualDirectory)) {
            New-Item -ItemType Directory -Path $directory -Force |
                Out-Null
        }

        Copy-Item -LiteralPath $distributionReadmeTemplate `
            -Destination (Join-Path $stagingRoot 'README.md') -Force
        Copy-Item -LiteralPath $distributionExampleReadmeTemplate `
            -Destination (Join-Path $exampleDirectory 'README.md') -Force
        $candidateSolution = Join-Path $exampleDirectory `
            'LasalApiWpfTestApp.sln'
        Copy-Item -LiteralPath $canonicalSolution `
            -Destination $candidateSolution -Force
        $stagedManualPdf = Join-Path $manualDirectory `
            'LASAL_Motion_Control_API_User_Manual_KO.pdf'
        $stagedManualDocx = Join-Path $manualDirectory `
            'LASAL_Motion_Control_API_User_Manual_KO.docx'
        [System.IO.File]::WriteAllBytes(
            $stagedManualPdf,
            $preparedInputs.PdfBytes)
        [System.IO.File]::WriteAllBytes(
            $stagedManualDocx,
            $preparedInputs.DocxBytes)
        $stagedManualPdfHash = (Get-FileHash `
            -LiteralPath $stagedManualPdf -Algorithm SHA256).Hash
        $stagedManualDocxHash = (Get-FileHash `
            -LiteralPath $stagedManualDocx -Algorithm SHA256).Hash
        if (-not $stagedManualPdfHash.Equals(
                $preparedInputs.PdfSha256,
                [System.StringComparison]::OrdinalIgnoreCase) -or
            -not $stagedManualDocxHash.Equals(
                $preparedInputs.DocxSha256,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Staged manual bytes do not match the locked input snapshot.'
        }

        $candidateProject = Copy-LmcDevelopmentExample `
            -DestinationRoot $exampleProjectRoot
        $candidateSolutionContract =
            Assert-LmcDistributionExampleSolutionContract `
                -StagingRoot $stagingRoot `
                -SolutionPath $candidateSolution `
                -ProjectPath $candidateProject

        Assert-LmcMSBuildCompilerBinding `
            -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -ProbeRoot $stagingRoot `
            -Projects @(
                $testProject,
                $wpfSmokeProject,
                $libraryProject,
                $candidateProject)
        $buildSummary.MSBuildCompilerBindingGate = 'PASS'

        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $testProject -Target 'RunTests' `
            -Configuration 'Debug'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $testProject -Target 'RunTests' `
            -Configuration 'Release'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $testProject `
            -Target 'RunLasalNetworkContract' -Configuration 'Release'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $wpfSmokeProject `
            -Target 'RunWpfSmokeTests' -Configuration 'Debug'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $wpfSmokeProject `
            -Target 'RunWpfSmokeTests' -Configuration 'Release'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $libraryProject -Target 'Rebuild' `
            -Configuration 'Release'

        $distributionDll = Join-Path $apiDirectory `
            'LasalMotionControlLib.dll'
        Copy-Item -LiteralPath $sourceDll `
            -Destination $distributionDll -Force

        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $candidateSolutionContract.SolutionPath `
            -Target 'Rebuild' `
            -Configuration 'Debug' `
            -Platform 'Any CPU'
        Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
            -CSharpCompilerPath $releaseCSharpCompiler `
            -Project $candidateSolutionContract.SolutionPath `
            -Target 'Rebuild' `
            -Configuration 'Release' `
            -Platform 'Any CPU'

        $exampleOutput = Join-Path $exampleProjectRoot 'bin\Release'
        $exampleExe = Join-Path $exampleOutput `
            'LasalMotionControlApiExample.exe'
        $exampleDll = Join-Path $exampleOutput `
            'LasalMotionControlLib.dll'
        if (-not (Test-Path -LiteralPath $exampleExe -PathType Leaf) -or
            -not (Test-Path -LiteralPath $exampleDll -PathType Leaf)) {
            throw 'Candidate example Release output is incomplete.'
        }
        Copy-Item -LiteralPath $exampleExe `
            -Destination $runDirectory -Force
        Copy-Item -LiteralPath $exampleDll `
            -Destination $runDirectory -Force
        $exampleConfig = "$exampleExe.config"
        if (Test-Path -LiteralPath $exampleConfig -PathType Leaf) {
            Copy-Item -LiteralPath $exampleConfig `
                -Destination $runDirectory -Force
        }

        $runExampleExe = Join-Path $runDirectory `
            'LasalMotionControlApiExample.exe'
        $buildSummary.ExecutableRelaunchTestedExeSha256 =
            Invoke-LmcDistributionExecutableRelaunchGate `
                -StagingRoot $stagingRoot `
                -ExecutablePath $runExampleExe `
                -GateAction {
                    param($testedExecutable)
                    Invoke-LmcMSBuild -MSBuildPath $releaseMSBuild `
                        -CSharpCompilerPath $releaseCSharpCompiler `
                        -Project $wpfSmokeProject `
                        -Target 'RunWpfExecutableRelaunchTest' `
                        -Configuration 'Release' `
                        -AdditionalProperties @{
                            WpfExecutableRelaunchExe = $testedExecutable
                        }
                }
        $buildSummary.ExecutableRelaunchGate = 'PASS'

        $manualPageCountOutput = @(& $releasePython -B -c `
            'from pypdf import PdfReader; import sys; print(len(PdfReader(sys.argv[1]).pages))' `
            (Join-Path $manualDirectory `
                'LASAL_Motion_Control_API_User_Manual_KO.pdf'))
        if ($LASTEXITCODE -ne 0 -or
            $manualPageCountOutput.Count -eq 0 -or
            [int]$manualPageCountOutput[-1] -lt 10) {
            throw 'Candidate API user manual PDF is missing or incomplete.'
        }
        $manualDocxValidationCode =
            "from docx import Document; import sys; d=Document(sys.argv[1]); h=sum(1 for p in d.paragraphs if p.style.name.startswith('Heading ')); assert h >= 30 and len(d.tables) >= 45 and 'LASAL Motion Control API' in d.core_properties.title"
        & $releasePython -B -c $manualDocxValidationCode `
            (Join-Path $manualDirectory `
                'LASAL_Motion_Control_API_User_Manual_KO.docx')
        if ($LASTEXITCODE -ne 0) {
            throw 'Candidate API user manual DOCX is missing or incomplete.'
        }

        $policyResult = Test-LmcDistributionSemanticPolicy `
            -RepositoryRoot $RepositoryRoot `
            -CandidateRoot $stagingRoot `
            -PythonPath $releasePython
        if ($policyResult.Result -ne 'PASS' -or
            $policyResult.PolicySha256 -notmatch '^[0-9A-Fa-f]{64}$') {
            throw 'Semantic policy preflight returned an invalid result.'
        }

        $sourceHash = (
            Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
        $distributionHash = (
            Get-FileHash -LiteralPath $distributionDll -Algorithm SHA256).Hash
        $runtimeDll = Join-Path $runDirectory `
            'LasalMotionControlLib.dll'
        $runtimeHash = (
            Get-FileHash -LiteralPath $runtimeDll -Algorithm SHA256).Hash
        if ($sourceHash -ne $distributionHash -or
            $sourceHash -ne $runtimeHash) {
            throw 'Canonical, API, and example runtime DLLs are not byte-identical.'
        }

        Remove-LmcCandidateBuildOutputs -CandidateRoot $stagingRoot
        Assert-LmcCandidateStructure -CandidateRoot $stagingRoot
        Assert-LmcCandidateNoInternalReferences `
            -CandidateRoot $stagingRoot

        $assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName(
            $sourceDll)
        $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(
            $sourceDll)
        $assemblyVersion = $assemblyName.Version.ToString()
        $fileVersion = $versionInfo.FileVersion
        $productVersion = $versionInfo.ProductVersion
        if ([string]::IsNullOrWhiteSpace($assemblyVersion) -or
            [string]::IsNullOrWhiteSpace($fileVersion) -or
            [string]::IsNullOrWhiteSpace($productVersion)) {
            throw 'Release DLL version metadata is incomplete.'
        }

        $manifestParameters = @{
            DistributionRoot = $stagingRoot
            CanonicalDllPath = $sourceDll
            DllReplicaRelativePaths = @(
                '01_API/LasalMotionControlLib.dll',
                '02_Example_Program/Run/LasalMotionControlLib.dll')
            SourceCommit = $preparedInputs.SourceCommit
            WorktreeState = $preparedInputs.WorktreeState
            AssemblyVersion = $assemblyVersion
            FileVersion = $fileVersion
            ProductVersion = $productVersion
            InputTreeSha256 = $preparedInputs.InputTreeSha256
            ToolchainSha256 = `
                $preparedInputs.ToolchainSnapshot.ToolchainSha256
            ToolchainRecords = @(
                $preparedInputs.ToolchainSnapshot.Records)
            ToolingPreflightResult = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightResult
            ToolingPreflightSuiteCount = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightSuiteCount
            ToolingPreflightRunCount = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightRunCount
            ToolingPreflightDigest = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightDigest
            ToolingPreflightFileCount = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightFileCount
            ToolingPreflightHostRecords = @(
                $preparedInputs.ToolchainSnapshot.ToolingPreflightHostRecords)
            ToolingPreflightSha256 = `
                $preparedInputs.ToolchainSnapshot.ToolingPreflightSha256
            SemanticPolicySha256 = $policyResult.PolicySha256
            SemanticPolicyResult = $policyResult.Result
        }
        $manifestPath = Write-LmcReleaseManifestAtomic `
            @manifestParameters
        Test-LmcReleaseManifest @manifestParameters | Out-Null
        Assert-LmcCandidateStructure -CandidateRoot $stagingRoot `
            -ManifestExpected

        $buildSummary.DllSha256 = $distributionHash
        $buildSummary.ExampleExeSha256 =
            Assert-LmcDistributionExecutableRelaunchIdentity `
                -StagingRoot $stagingRoot `
                -ExecutablePath $runExampleExe `
                -TestedSha256 (
                    $buildSummary.ExecutableRelaunchTestedExeSha256)
        $buildSummary.ManifestSha256 = (
            Get-FileHash -LiteralPath $manifestPath `
                -Algorithm SHA256).Hash
        $buildSummary.SemanticPolicySha256 = `
            $policyResult.PolicySha256
        $buildSummary.SemanticCheckCount = $policyResult.CheckCount
        $buildSummary.SourceCommit = $preparedInputs.SourceCommit
        $buildSummary.WorktreeState = $preparedInputs.WorktreeState
        $buildSummary.InputTreeSha256 = $preparedInputs.InputTreeSha256
        $buildSummary.ToolchainSha256 = `
            $preparedInputs.ToolchainSnapshot.ToolchainSha256
        $buildSummary.ToolingPreflightSha256 = `
            $preparedInputs.ToolchainSnapshot.ToolingPreflightSha256
        $buildSummary.ManualPageCount = `
            [int]$manualPageCountOutput[-1]
    }

Write-Host "Distribution candidate completed: $CandidatePath"
Write-Host "Canonical distribution preserved: $canonicalDistribution"
Write-Host "Manual PDF input: $manualPdfInput"
Write-Host "Manual DOCX input: $manualDocxInput"
Write-Host "Transaction committed: $($transaction.Committed)"
Write-Host "Source commit: $($buildSummary.SourceCommit)"
Write-Host "Worktree state: $($buildSummary.WorktreeState)"
Write-Host "Release input tree SHA256: $($buildSummary.InputTreeSha256)"
Write-Host "Release transaction fingerprint: $($transaction.InputFingerprint)"
Write-Host "Release toolchain SHA256: $($buildSummary.ToolchainSha256)"
Write-Host "Tooling preflight attestation SHA256: $($buildSummary.ToolingPreflightSha256)"
Write-Host "Semantic policy: PASS ($($buildSummary.SemanticCheckCount) checks)"
Write-Host "Semantic policy SHA256: $($buildSummary.SemanticPolicySha256)"
Write-Host "Release manifest SHA256: $($buildSummary.ManifestSha256)"
Write-Host "DLL SHA256: $($buildSummary.DllSha256)"
Write-Host "Example EXE SHA256: $($buildSummary.ExampleExeSha256)"
Write-Host "Executable relaunch gate: $($buildSummary.ExecutableRelaunchGate)"
Write-Host "Executable relaunch tested EXE SHA256: $($buildSummary.ExecutableRelaunchTestedExeSha256)"
Write-Host "Manual pages: $($buildSummary.ManualPageCount)"

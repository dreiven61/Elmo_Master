param(
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..'),
    [switch]$Capture,
    [string]$OutputPath,
    [switch]$RunSelfTest
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Validation is always read-only. Files are created only when both -Capture
# and the exact evidence directory in $PSScriptRoot are supplied. LASAL must
# be closed, inputs are revalidated, and existing files are never overwritten.
$ExpectedBaselineCanonicalLfSha256 =
    '7EAB9F0E71A85C1459FD01A381859D9EC5095949D536E78B056A67BE91C2D1BE'
$ExpectedBaselineIdeCrlfSha256 =
    'A51E716363E8DB38E7BE6D849BC2C29D4FE7B51E801D5704BA7F95D73CCC8753'
$SnapshotFileName = 'post_ide_pre_split_LMCControlCommandService.st'
$ManifestFileName = 'post_ide_pre_split_manifest.json'
$Utf8 = [Text.UTF8Encoding]::new($false, $true)

$HomeSpec = [pscustomobject]@{
    Name = 'HandleAxisOwnershipPublishHomeReceipt'
    Inputs = @(
        [pscustomobject]@{ Name = 'AxisMask'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'AdmissionToken'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'OwnerGeneration'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ReportKind'; Type = 'UINT' }
        [pscustomobject]@{ Name = 'ReportValue0'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ReportValue1'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ObservationCycle'; Type = 'UDINT' }
    )
    Output = [pscustomobject]@{ Name = 'Result'; Type = 'DINT' }
}
$DecisionSpec = [pscustomobject]@{
    Name = 'PrepareAxisOwnershipPublishDecision'
    Inputs = @(
        [pscustomobject]@{ Name = 'AxisMask'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'AdmissionToken'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'OwnerGeneration'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ReportKind'; Type = 'UINT' }
        [pscustomobject]@{ Name = 'ExpectedSession'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ExpectedSequence'; Type = 'UDINT' }
        [pscustomobject]@{ Name = 'ExpectedCommandId'; Type = 'DINT' }
        [pscustomobject]@{ Name = 'ExpectedReference'; Type = 'DINT' }
        [pscustomobject]@{ Name = 'ExpectedAdmissionMode'; Type = 'DINT' }
        [pscustomobject]@{ Name = 'ExpectedOwnerKind'; Type = 'DINT' }
    )
    Output = [pscustomobject]@{ Name = 'Result'; Type = 'DINT' }
}

function ConvertTo-CanonicalLf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return $Text.Replace("`r`n", "`n").Replace("`r", "`n")
}

function ConvertTo-IdeCrlf {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return (ConvertTo-CanonicalLf -Text $Text).Replace("`n", "`r`n")
}

function Get-TextSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($Text)))
}

function Get-BytesSha256 {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Bytes))
}

function Get-FileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    return Get-BytesSha256 -Bytes ([IO.File]::ReadAllBytes($Path))
}

function ConvertFrom-AsciiSourceBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    if (($Bytes.Length -ge 3) -and
        ($Bytes[0] -eq 0xEF) -and ($Bytes[1] -eq 0xBB) -and
        ($Bytes[2] -eq 0xBF)) {
        throw "$Owner has a UTF-8 BOM."
    }
    foreach ($value in $Bytes) {
        if ($value -gt 0x7F) {
            throw "$Owner contains a non-ASCII byte."
        }
    }
    return $Utf8.GetString($Bytes)
}

function Resolve-CaptureOutputDirectory {
    param([Parameter(Mandatory = $true)][string]$RequestedPath)

    $requested = if ([IO.Path]::IsPathRooted($RequestedPath)) {
        [IO.Path]::GetFullPath($RequestedPath)
    }
    else {
        [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $RequestedPath))
    }
    $allowed = [IO.Path]::GetFullPath($PSScriptRoot)
    $requestedResolved = if ([IO.Directory]::Exists($requested)) {
        (Resolve-Path -LiteralPath $requested).Path
    }
    else {
        $requested
    }
    $allowedResolved = (Resolve-Path -LiteralPath $allowed).Path
    $allowedItem = Get-Item -LiteralPath $allowedResolved -Force
    if (($allowedItem.Attributes -band
            [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'The publish-split evidence directory cannot be a reparse point.'
    }
    if (-not [string]::Equals(
            $requestedResolved.TrimEnd('\'),
            $allowedResolved.TrimEnd('\'),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw (
            'OutputPath must be the exact publish-split evidence directory: ' +
            $allowedResolved)
    }
    return $allowedResolved
}

function Get-RelativeRepositoryPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $relative = [IO.Path]::GetRelativePath($Root, $Path)
    return $relative.Replace('\', '/')
}

function Get-InputLineEnding {
    param([Parameter(Mandatory = $true)][string]$Text)

    $canonicalLf = ConvertTo-CanonicalLf -Text $Text
    if ($Text -ceq $canonicalLf) {
        return 'LF'
    }
    $ideCrlf = ConvertTo-IdeCrlf -Text $canonicalLf
    if ($Text -ceq $ideCrlf) {
        return 'CRLF'
    }
    throw 'Source uses mixed or unsupported line endings.'
}

function Get-LineNumberAtOffset {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][int]$Offset
    )

    if (($Offset -lt 0) -or ($Offset -gt $Text.Length)) {
        throw "Line offset $Offset is outside the text."
    }
    if ($Offset -eq 0) {
        return 1
    }
    return ([regex]::Matches($Text.Substring(0, $Offset), "`n").Count + 1)
}

function Get-DeclarationBlockPattern {
    param([Parameter(Mandatory = $true)][pscustomobject]$Spec)

    $pattern = '^\tFUNCTION[ \t]+' + [regex]::Escape($Spec.Name) +
        '[ \t]*\n'
    $pattern += '^\t\tVAR_INPUT[ \t]*\n'
    foreach ($input in $Spec.Inputs) {
        $pattern += '^\t\t\t' + [regex]::Escape($input.Name) +
            '[ \t]+:[ \t]+' + [regex]::Escape($input.Type) +
            ';[ \t]*\n'
    }
    $pattern += '^\t\tEND_VAR[ \t]*\n'
    $pattern += '^\t\tVAR_OUTPUT[ \t]*\n'
    $pattern += '^\t\t\t' + [regex]::Escape($Spec.Output.Name) +
        '[ \t]+:[ \t]+' + [regex]::Escape($Spec.Output.Type) +
        ';[ \t]*\n'
    $pattern += '^\t\tEND_VAR;[ \t]*\n'
    return $pattern
}

function Get-EmptyStubPattern {
    param([Parameter(Mandatory = $true)][pscustomobject]$Spec)

    $pattern = '^FUNCTION[ \t]+LMCControlCommandService::' +
        [regex]::Escape($Spec.Name) + '[ \t]*\n'
    $pattern += '^\tVAR_INPUT[ \t]*\n'
    foreach ($input in $Spec.Inputs) {
        $pattern += '^\t\t' + [regex]::Escape($input.Name) +
            '[ \t]+:[ \t]+' + [regex]::Escape($input.Type) +
            ';[ \t]*\n'
    }
    $pattern += '^\tEND_VAR[ \t]*\n'
    $pattern += '^\tVAR_OUTPUT[ \t]*\n'
    $pattern += '^\t\t' + [regex]::Escape($Spec.Output.Name) +
        '[ \t]+:[ \t]+' + [regex]::Escape($Spec.Output.Type) +
        ';[ \t]*\n'
    $pattern += '^\tEND_VAR[ \t]*\n'
    $pattern += '^\n'
    $pattern += '^END_FUNCTION[ \t]*\n'
    return $pattern
}

function New-GeneratedDeclarationText {
    param([Parameter(Mandatory = $true)][pscustomobject]$Spec)

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add("`tFUNCTION $($Spec.Name)")
    $lines.Add("`t`tVAR_INPUT")
    foreach ($input in $Spec.Inputs) {
        $lines.Add("`t`t`t$($input.Name) `t: $($input.Type);")
    }
    $lines.Add("`t`tEND_VAR")
    $lines.Add("`t`tVAR_OUTPUT")
    $lines.Add("`t`t`t$($Spec.Output.Name) `t: $($Spec.Output.Type);")
    $lines.Add("`t`tEND_VAR;")
    return [string]::Join("`n", $lines) + "`n"
}

function New-GeneratedEmptyStubText {
    param([Parameter(Mandatory = $true)][pscustomobject]$Spec)

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add(
        "FUNCTION LMCControlCommandService::$($Spec.Name)")
    $lines.Add("`tVAR_INPUT")
    foreach ($input in $Spec.Inputs) {
        $lines.Add("`t`t$($input.Name) `t: $($input.Type);")
    }
    $lines.Add("`tEND_VAR")
    $lines.Add("`tVAR_OUTPUT")
    $lines.Add("`t`t$($Spec.Output.Name) `t: $($Spec.Output.Type);")
    $lines.Add("`tEND_VAR")
    $lines.Add('')
    $lines.Add('END_FUNCTION')
    return [string]::Join("`n", $lines) + "`n"
}

function Assert-HelperNameInventory {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][pscustomobject]$Spec,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $namePattern = '(?<![A-Za-z0-9_])' +
        [regex]::Escape($Spec.Name) + '(?![A-Za-z0-9_])'
    $nameCount = [regex]::Matches($Source, $namePattern).Count
    if ($nameCount -ne 2) {
        throw (
            "$Owner helper '$($Spec.Name)' source token count is " +
            "$nameCount, expected exactly two (declaration and empty stub).")
    }

    $illegalHeaderPattern =
        '(?m)^[ \t]*FUNCTION[ \t]+' +
        '(?:GLOBAL[ \t]+|VIRTUAL[ \t]+GLOBAL[ \t]+)' +
        '(?:LMCControlCommandService::)?' +
        [regex]::Escape($Spec.Name) + '[ \t]*$'
    if ([regex]::IsMatch($Source, $illegalHeaderPattern)) {
        throw "$Owner helper '$($Spec.Name)' is GLOBAL or VIRTUAL GLOBAL."
    }
}

function Assert-PublishIdeBaselineShape {
    param(
        [Parameter(Mandatory = $true)][string]$InputText,
        [Parameter(Mandatory = $true)][string]$ExpectedCanonicalLfSha256,
        [Parameter(Mandatory = $true)][string]$ExpectedIdeCrlfSha256,
        [Parameter(Mandatory = $true)][string]$Owner
    )

    $lineEnding = Get-InputLineEnding -Text $InputText
    $source = ConvertTo-CanonicalLf -Text $InputText

    Assert-HelperNameInventory -Source $source -Spec $HomeSpec -Owner $Owner
    Assert-HelperNameInventory -Source $source -Spec $DecisionSpec -Owner $Owner

    $tableAnchorCount = [regex]::Matches(
        $source, '(?m)^  //Tables:[ \t]*\n').Count
    if ($tableAnchorCount -ne 1) {
        throw "$Owner //Tables anchor count is $tableAnchorCount, expected one."
    }

    $homeDeclarationPattern = Get-DeclarationBlockPattern -Spec $HomeSpec
    $decisionDeclarationPattern =
        Get-DeclarationBlockPattern -Spec $DecisionSpec
    $declarationInsertionPattern =
        '(?<All>^\t\n(?<Home>' + $homeDeclarationPattern +
        ')^\t\n(?<Decision>' + $decisionDeclarationPattern +
        '))(?=^  //Tables:[ \t]*\n)'
    $declarationMatches = [regex]::Matches(
        $source,
        $declarationInsertionPattern,
        [Text.RegularExpressions.RegexOptions]::Multiline)
    if ($declarationMatches.Count -ne 1) {
        throw (
            "$Owner exact ordered private declaration insertion count is " +
            "$($declarationMatches.Count), expected one.")
    }

    $homeStubPattern = Get-EmptyStubPattern -Spec $HomeSpec
    $decisionStubPattern = Get-EmptyStubPattern -Spec $DecisionSpec
    $implementationInsertionPattern =
        '(?<All>\n\n(?<Home>' + $homeStubPattern +
        ')\n\n(?<Decision>' + $decisionStubPattern + '))\z'
    $implementationMatches = [regex]::Matches(
        $source,
        $implementationInsertionPattern,
        [Text.RegularExpressions.RegexOptions]::Multiline)
    if ($implementationMatches.Count -ne 1) {
        throw (
            "$Owner exact ordered qualified empty-stub insertion count is " +
            "$($implementationMatches.Count), expected one.")
    }

    $declarationMatch = $declarationMatches[0].Groups['All']
    $implementationMatch = $implementationMatches[0].Groups['All']
    if ($declarationMatch.Index -ge $implementationMatch.Index) {
        throw "$Owner generated declaration/stub order is invalid."
    }

    # Remove the later range first so the declaration offsets remain stable.
    $stripped = $source.Remove(
        $implementationMatch.Index, $implementationMatch.Length)
    $stripped = $stripped.Remove(
        $declarationMatch.Index, $declarationMatch.Length)
    $strippedCanonicalSha256 = Get-TextSha256 -Text $stripped
    $strippedIdeCrlf = ConvertTo-IdeCrlf -Text $stripped
    $strippedIdeCrlfSha256 = Get-TextSha256 -Text $strippedIdeCrlf
    if (($strippedCanonicalSha256 -cne $ExpectedCanonicalLfSha256) -or
        ($strippedIdeCrlfSha256 -cne $ExpectedIdeCrlfSha256)) {
        throw (
            "$Owner generated-only removal did not restore the approved " +
            "baseline ($strippedCanonicalSha256/$strippedIdeCrlfSha256).")
    }

    $helperResults = @(
        foreach ($pair in @(
                [pscustomobject]@{
                    Spec = $HomeSpec
                    Declaration = $declarationMatches[0].Groups['Home']
                    Stub = $implementationMatches[0].Groups['Home']
                },
                [pscustomobject]@{
                    Spec = $DecisionSpec
                    Declaration = $declarationMatches[0].Groups['Decision']
                    Stub = $implementationMatches[0].Groups['Decision']
                })) {
            [ordered]@{
                name = $pair.Spec.Name
                private = $true
                global = $false
                virtualGlobal = $false
                inputCount = $pair.Spec.Inputs.Count
                inputs = @(
                    foreach ($input in $pair.Spec.Inputs) {
                        "$($input.Name) : $($input.Type)"
                    })
                output =
                    "$($pair.Spec.Output.Name) : $($pair.Spec.Output.Type)"
                declarationLine = Get-LineNumberAtOffset `
                    -Text $source -Offset $pair.Declaration.Index
                declarationCanonicalLfBytes =
                    $Utf8.GetByteCount($pair.Declaration.Value)
                declarationCanonicalLfSha256 =
                    Get-TextSha256 -Text $pair.Declaration.Value
                implementationLine = Get-LineNumberAtOffset `
                    -Text $source -Offset $pair.Stub.Index
                implementationQualified = $true
                implementationEmpty = $true
                stubCanonicalLfBytes =
                    $Utf8.GetByteCount($pair.Stub.Value)
                stubCanonicalLfSha256 =
                    Get-TextSha256 -Text $pair.Stub.Value
            }
        })

    return [pscustomobject]@{
        InputLineEnding = $lineEnding
        PhysicalBytes = $Utf8.GetByteCount($InputText)
        PhysicalSha256 = Get-TextSha256 -Text $InputText
        CanonicalLfBytes = $Utf8.GetByteCount($source)
        CanonicalLfSha256 = Get-TextSha256 -Text $source
        DeclarationInsertionLine = Get-LineNumberAtOffset `
            -Text $source -Offset $declarationMatch.Index
        DeclarationInsertionCanonicalLfBytes =
            $Utf8.GetByteCount($declarationMatch.Value)
        DeclarationInsertionCanonicalLfSha256 =
            Get-TextSha256 -Text $declarationMatch.Value
        ImplementationInsertionLine = Get-LineNumberAtOffset `
            -Text $source -Offset $implementationMatch.Index
        ImplementationInsertionCanonicalLfBytes =
            $Utf8.GetByteCount($implementationMatch.Value)
        ImplementationInsertionCanonicalLfSha256 =
            Get-TextSha256 -Text $implementationMatch.Value
        StrippedCanonicalLf = $stripped
        StrippedCanonicalLfBytes = $Utf8.GetByteCount($stripped)
        StrippedCanonicalLfSha256 = $strippedCanonicalSha256
        StrippedIdeCrlfBytes = $Utf8.GetByteCount($strippedIdeCrlf)
        StrippedIdeCrlfSha256 = $strippedIdeCrlfSha256
        Helpers = $helperResults
    }
}

function Get-ClassesLcbEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not [IO.File]::Exists($Path)) {
        throw "Classes.lcb is missing: $Path"
    }
    $bytes = [IO.File]::ReadAllBytes($Path)
    $ascii = [Text.Encoding]::ASCII.GetString($bytes)
    $signature = if ($bytes.Length -ge 20) {
        [Text.Encoding]::ASCII.GetString($bytes, 0, 20)
    }
    else {
        ''
    }
    if ($signature -cne 'SigmatekLasal2Binary') {
        throw "Classes.lcb signature is not SigmatekLasal2Binary."
    }

    $helpers = @(
        foreach ($spec in @($HomeSpec, $DecisionSpec)) {
            $pattern = '(?<![A-Za-z0-9_])' +
                [regex]::Escape($spec.Name) + '(?![A-Za-z0-9_])'
            $matches = [regex]::Matches($ascii, $pattern)
            if ($matches.Count -lt 1) {
                throw (
                    "Classes.lcb has no exact ASCII name record for " +
                    "'$($spec.Name)'.")
            }
            [ordered]@{
                name = $spec.Name
                exactAsciiNameCount = $matches.Count
                asciiByteOffsets = @($matches | ForEach-Object { $_.Index })
                sourceVisibility = 'private'
                sourceGlobal = $false
                sourceVirtualGlobal = $false
                binaryVisibilityDecoded = $false
                binaryVisibilityNote =
                    'Proprietary flag records are not decoded; privacy is proven from exact source declarations.'
            }
        })

    return [ordered]@{
        path = Get-RelativeRepositoryPath -Root $Root -Path $Path
        bytes = $bytes.Length
        sha256 = Get-BytesSha256 -Bytes $bytes
        formatSignature = $signature
        parser = 'safe exact ASCII name scan only'
        helpers = $helpers
    }
}

function Get-NetworkFileEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$NetworkRoot
    )

    if (-not [IO.Directory]::Exists($NetworkRoot)) {
        throw "Network directory is missing: $NetworkRoot"
    }
    $pathSpec =
        'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/**'
    $trackedOutput = @(& git -C $Root ls-files -- $pathSpec)
    if ($LASTEXITCODE -ne 0) {
        throw 'git ls-files failed while enumerating tracked Network files.'
    }
    $trackedPaths = @(
        $trackedOutput |
            ForEach-Object { ([string]$_).Trim().Replace('\', '/') } |
            Where-Object { $_ -ne '' })
    $availablePaths = @(
        Get-ChildItem -LiteralPath $NetworkRoot -File -Recurse -Force |
            ForEach-Object {
                Get-RelativeRepositoryPath -Root $Root -Path $_.FullName
            })
    $allPaths = @(
        @($trackedPaths + $availablePaths) |
            Sort-Object -Unique)
    if ($allPaths.Count -eq 0) {
        throw 'No tracked or available Network files were found.'
    }

    $files = @(
        foreach ($relativePath in $allPaths) {
            $fullPath = Join-Path $Root $relativePath.Replace('/', '\')
            $exists = [IO.File]::Exists($fullPath)
            $tracked = $trackedPaths -contains $relativePath
            if ($tracked -and (-not $exists)) {
                throw "Tracked Network file is unavailable: $relativePath"
            }
            $bytes = if ($exists) {
                [IO.File]::ReadAllBytes($fullPath)
            }
            else {
                $null
            }
            [ordered]@{
                path = $relativePath
                tracked = $tracked
                available = $exists
                bytes = if ($exists) {
                    $bytes.Length
                }
                else {
                    $null
                }
                sha256 = if ($exists) {
                    Get-BytesSha256 -Bytes $bytes
                }
                else {
                    $null
                }
            }
        })

    $identityText = [string]::Join("`n", @(
            foreach ($file in $files) {
                '{0}|{1}|{2}|{3}|{4}' -f
                    $file.path,
                    ([int][bool]$file.tracked),
                    ([int][bool]$file.available),
                    $file.bytes,
                    $file.sha256
            }))

    return [ordered]@{
        trackedCount = $trackedPaths.Count
        availableCount = $availablePaths.Count
        unionCount = $allPaths.Count
        allTrackedFilesAvailable = $true
        inventorySha256 = Get-TextSha256 -Text $identityText
        files = $files
    }
}

function Assert-CaptureInputsUnchanged {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$ExpectedSourceSha256,
        [Parameter(Mandatory = $true)][string]$ClassesLcbPath,
        [Parameter(Mandatory = $true)][string]$ExpectedClassesSha256,
        [Parameter(Mandatory = $true)][string]$NetworkRoot,
        [Parameter(Mandatory = $true)][string]$ExpectedNetworkInventorySha256
    )

    $currentSourceSha256 = Get-FileSha256 -Path $SourcePath
    if ($currentSourceSha256 -cne $ExpectedSourceSha256) {
        throw 'Control source changed during baseline capture.'
    }
    $currentClasses = Get-ClassesLcbEvidence `
        -Root $Root -Path $ClassesLcbPath
    if ($currentClasses.sha256 -cne $ExpectedClassesSha256) {
        throw 'Classes.lcb changed during baseline capture.'
    }
    $currentNetwork = Get-NetworkFileEvidence `
        -Root $Root -NetworkRoot $NetworkRoot
    if ($currentNetwork.inventorySha256 -cne
        $ExpectedNetworkInventorySha256) {
        throw 'Network inventory changed during baseline capture.'
    }
}

function New-SyntheticBaseline {
    $lines = @(
        'LMCControlCommandService : CLASS',
        "`tFUNCTION ExistingPrivate",
        "`t`tVAR_OUTPUT",
        "`t`t`tResult `t: DINT;",
        "`t`tEND_VAR;",
        '  //Tables:',
        "`tFUNCTION @STD",
        'END_CLASS;',
        '',
        '//{{LSL_IMPLEMENTATION',
        '',
        'FUNCTION LMCControlCommandService::ExistingPrivate',
        "`tVAR_OUTPUT",
        "`t`tResult `t: DINT;",
        "`tEND_VAR",
        '',
        'END_FUNCTION'
    )
    return [string]::Join("`n", $lines) + "`n"
}

function New-SyntheticPositive {
    param([Parameter(Mandatory = $true)][string]$Baseline)

    $homeDeclaration = New-GeneratedDeclarationText -Spec $HomeSpec
    $decisionDeclaration =
        New-GeneratedDeclarationText -Spec $DecisionSpec
    $homeStub = New-GeneratedEmptyStubText -Spec $HomeSpec
    $decisionStub = New-GeneratedEmptyStubText -Spec $DecisionSpec
    $declarationInsertion = "`t`n" + $homeDeclaration +
        "`t`n" + $decisionDeclaration
    $withDeclarations = $Baseline.Replace(
        "  //Tables:`n", $declarationInsertion + "  //Tables:`n")
    if ($withDeclarations -ceq $Baseline) {
        throw 'Synthetic //Tables replacement failed.'
    }
    return $withDeclarations + "`n`n" + $homeStub +
        "`n`n" + $decisionStub
}

function Invoke-SelfTest {
    $baseline = New-SyntheticBaseline
    $baselineLfSha256 = Get-TextSha256 -Text $baseline
    $baselineCrlfSha256 = Get-TextSha256 -Text (
        ConvertTo-IdeCrlf -Text $baseline)
    $positive = New-SyntheticPositive -Baseline $baseline

    $lfResult = Assert-PublishIdeBaselineShape `
        -InputText $positive `
        -ExpectedCanonicalLfSha256 $baselineLfSha256 `
        -ExpectedIdeCrlfSha256 $baselineCrlfSha256 `
        -Owner 'Synthetic LF positive'
    if ($lfResult.StrippedCanonicalLf -cne $baseline) {
        throw 'Synthetic LF positive did not restore exact baseline text.'
    }
    $crlfResult = Assert-PublishIdeBaselineShape `
        -InputText (ConvertTo-IdeCrlf -Text $positive) `
        -ExpectedCanonicalLfSha256 $baselineLfSha256 `
        -ExpectedIdeCrlfSha256 $baselineCrlfSha256 `
        -Owner 'Synthetic CRLF positive'
    if ($crlfResult.StrippedCanonicalLf -cne $baseline) {
        throw 'Synthetic CRLF positive did not restore exact baseline text.'
    }

    $homeDeclaration = New-GeneratedDeclarationText -Spec $HomeSpec
    $decisionDeclaration =
        New-GeneratedDeclarationText -Spec $DecisionSpec
    $homeStub = New-GeneratedEmptyStubText -Spec $HomeSpec
    $decisionStub = New-GeneratedEmptyStubText -Spec $DecisionSpec
    $negativeFixtures = @(
        [pscustomobject]@{
            Name = 'PartialDecisionStubMissing'
            Text = $positive.Replace("`n`n" + $decisionStub, '')
        }
        [pscustomobject]@{
            Name = 'DuplicateHomeStub'
            Text = $positive + "`n`n" + $homeStub
        }
        [pscustomobject]@{
            Name = 'HomeDeclarationGlobal'
            Text = $positive.Replace(
                "`tFUNCTION $($HomeSpec.Name)",
                "`tFUNCTION GLOBAL $($HomeSpec.Name)")
        }
        [pscustomobject]@{
            Name = 'DecisionDeclarationVirtualGlobal'
            Text = $positive.Replace(
                "`tFUNCTION $($DecisionSpec.Name)",
                "`tFUNCTION VIRTUAL GLOBAL $($DecisionSpec.Name)")
        }
        [pscustomobject]@{
            Name = 'HomeStubGlobal'
            Text = $positive.Replace(
                "FUNCTION LMCControlCommandService::$($HomeSpec.Name)",
                "FUNCTION GLOBAL LMCControlCommandService::$($HomeSpec.Name)")
        }
        [pscustomobject]@{
            Name = 'DecisionStubUnqualified'
            Text = $positive.Replace(
                "FUNCTION LMCControlCommandService::$($DecisionSpec.Name)",
                "FUNCTION $($DecisionSpec.Name)")
        }
        [pscustomobject]@{
            Name = 'HomeStubBodyAdded'
            Text = $positive.Replace(
                $homeStub,
                $homeStub.Replace(
                    "`tEND_VAR`n`nEND_FUNCTION",
                    "`tEND_VAR`n`n`tResult := 0;`n`nEND_FUNCTION"))
        }
        [pscustomobject]@{
            Name = 'DeclarationOrderSwapped'
            Text = $positive.Replace(
                "`t`n" + $homeDeclaration + "`t`n" +
                    $decisionDeclaration,
                "`t`n" + $decisionDeclaration + "`t`n" +
                    $homeDeclaration)
        }
        [pscustomobject]@{
            Name = 'HomeReportKindTypeChanged'
            Text = $positive.Replace(
                $homeDeclaration,
                $homeDeclaration.Replace(
                    "`t`t`tReportKind `t: UINT;",
                    "`t`t`tReportKind `t: UDINT;"))
        }
        [pscustomobject]@{
            Name = 'GeneratedRemovalBaselineDrift'
            Text = (New-SyntheticPositive `
                -Baseline ($baseline + "// baseline drift`n"))
        }
        [pscustomobject]@{
            Name = 'MixedLineEndings'
            Text = $positive.Replace(
                "LMCControlCommandService : CLASS`n",
                "LMCControlCommandService : CLASS`r`n")
        }
    )

    $rejected = 0
    foreach ($fixture in $negativeFixtures) {
        $didReject = $false
        try {
            $null = Assert-PublishIdeBaselineShape `
                -InputText $fixture.Text `
                -ExpectedCanonicalLfSha256 $baselineLfSha256 `
                -ExpectedIdeCrlfSha256 $baselineCrlfSha256 `
                -Owner "Synthetic negative $($fixture.Name)"
        }
        catch {
            $didReject = $true
        }
        if (-not $didReject) {
            throw "Synthetic negative '$($fixture.Name)' was accepted."
        }
        $rejected++
    }

    $asciiBytes = $Utf8.GetBytes($positive)
    if ((ConvertFrom-AsciiSourceBytes `
            -Bytes $asciiBytes -Owner 'Synthetic ASCII positive') -cne
        $positive) {
        throw 'Synthetic ASCII source bytes did not round-trip.'
    }
    foreach ($byteFixture in @(
            [pscustomobject]@{
                Name = 'Utf8Bom'
                Bytes = [byte[]](0xEF, 0xBB, 0xBF, 0x41)
            },
            [pscustomobject]@{
                Name = 'NonAscii'
                Bytes = [byte[]](0x41, 0x80)
            })) {
        $didReject = $false
        try {
            $null = ConvertFrom-AsciiSourceBytes `
                -Bytes $byteFixture.Bytes `
                -Owner "Synthetic negative $($byteFixture.Name)"
        }
        catch {
            $didReject = $true
        }
        if (-not $didReject) {
            throw "Synthetic negative '$($byteFixture.Name)' was accepted."
        }
        $rejected++
    }

    $resolvedAllowed = Resolve-CaptureOutputDirectory `
        -RequestedPath $PSScriptRoot
    if (-not [string]::Equals(
            $resolvedAllowed.TrimEnd('\'),
            ([IO.Path]::GetFullPath($PSScriptRoot)).TrimEnd('\'),
            [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Synthetic allowed capture directory did not resolve exactly.'
    }
    $didRejectPath = $false
    try {
        $null = Resolve-CaptureOutputDirectory `
            -RequestedPath (Join-Path $PSScriptRoot '..')
    }
    catch {
        $didRejectPath = $true
    }
    if (-not $didRejectPath) {
        throw 'Synthetic unsafe parent OutputPath was accepted.'
    }
    $rejected++
    return $rejected
}

if ($RunSelfTest) {
    if ($Capture -or $PSBoundParameters.ContainsKey('OutputPath')) {
        throw '-RunSelfTest cannot be combined with -Capture or -OutputPath.'
    }
    $negativeCount = Invoke-SelfTest
    Write-Host (
        'PASS LASAL.AxisOwnershipPublishIdeBaseline.SelfTest (' +
        "$negativeCount/$negativeCount negative fixtures rejected; " +
        'LF/CRLF positive fixtures restored exact baselines)')
    return
}

if ($Capture -and [string]::IsNullOrWhiteSpace($OutputPath)) {
    throw '-Capture requires an explicit -OutputPath directory.'
}
if ((-not $Capture) -and $PSBoundParameters.ContainsKey('OutputPath')) {
    throw '-OutputPath is accepted only together with -Capture.'
}
if (@(Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue).Count -ne 0) {
    throw 'LASAL must be fully closed before post-IDE baseline validation.'
}

$root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$sourcePath = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$classesLcbPath = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\Classes.lcb')
$networkRoot = Join-Path $root (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Network')
if (-not [IO.File]::Exists($sourcePath)) {
    throw "Control source is missing: $sourcePath"
}

$sourceBytes = [IO.File]::ReadAllBytes($sourcePath)
$sourcePhysicalSha256 = Get-BytesSha256 -Bytes $sourceBytes
$sourceInput = ConvertFrom-AsciiSourceBytes `
    -Bytes $sourceBytes -Owner 'Control source'
$validation = Assert-PublishIdeBaselineShape `
    -InputText $sourceInput `
    -ExpectedCanonicalLfSha256 $ExpectedBaselineCanonicalLfSha256 `
    -ExpectedIdeCrlfSha256 $ExpectedBaselineIdeCrlfSha256 `
    -Owner 'Publish post-IDE pre-split source'
if ($validation.PhysicalSha256 -cne $sourcePhysicalSha256) {
    throw 'Control source text and raw-byte hashes diverged.'
}
$classesLcb = Get-ClassesLcbEvidence `
    -Root $root -Path $classesLcbPath
$network = Get-NetworkFileEvidence `
    -Root $root -NetworkRoot $networkRoot

$manifest = [ordered]@{
    phase = 'post_ide_pre_split'
    mode = if ($Capture) { 'captured' } else { 'validation_only' }
    observedAt = [DateTimeOffset]::Now.ToString('o')
    source = [ordered]@{
        path = Get-RelativeRepositoryPath -Root $root -Path $sourcePath
        inputLineEnding = $validation.InputLineEnding
        physicalBytes = $validation.PhysicalBytes
        physicalSha256 = $sourcePhysicalSha256
        asciiOnly = $true
        utf8Bom = $false
        canonicalLfBytes = $validation.CanonicalLfBytes
        canonicalLfSha256 = $validation.CanonicalLfSha256
    }
    generatedOnlyProof = [ordered]@{
        exactOrderedPrivateDeclarations = $true
        exactOrderedQualifiedEmptyStubs = $true
        declarationInsertionLine = $validation.DeclarationInsertionLine
        declarationInsertionCanonicalLfBytes =
            $validation.DeclarationInsertionCanonicalLfBytes
        declarationInsertionCanonicalLfSha256 =
            $validation.DeclarationInsertionCanonicalLfSha256
        implementationInsertionLine =
            $validation.ImplementationInsertionLine
        implementationInsertionCanonicalLfBytes =
            $validation.ImplementationInsertionCanonicalLfBytes
        implementationInsertionCanonicalLfSha256 =
            $validation.ImplementationInsertionCanonicalLfSha256
        helpers = $validation.Helpers
        removalRestoresApprovedBaseline = $true
        restoredCanonicalLfBytes = $validation.StrippedCanonicalLfBytes
        restoredCanonicalLfSha256 =
            $validation.StrippedCanonicalLfSha256
        restoredIdeCrlfBytes = $validation.StrippedIdeCrlfBytes
        restoredIdeCrlfSha256 = $validation.StrippedIdeCrlfSha256
    }
    classesLcb = $classesLcb
    network = $network
    capture = [ordered]@{
        performed = $false
        outputDirectory = $null
        snapshotFile = $null
        manifestFile = $null
    }
}

if ($Capture) {
    $outputDirectory = Resolve-CaptureOutputDirectory `
        -RequestedPath $OutputPath
    $snapshotPath = Join-Path $outputDirectory $SnapshotFileName
    $manifestPath = Join-Path $outputDirectory $ManifestFileName
    $manifest.capture.performed = $true
    $manifest.capture.outputDirectory = $outputDirectory
    $manifest.capture.snapshotFile = $snapshotPath
    $manifest.capture.manifestFile = $manifestPath
    $json = $manifest | ConvertTo-Json -Depth 12
    $manifestBytes = $Utf8.GetBytes($json + "`n")

    Assert-CaptureInputsUnchanged `
        -Root $root `
        -SourcePath $sourcePath `
        -ExpectedSourceSha256 $sourcePhysicalSha256 `
        -ClassesLcbPath $classesLcbPath `
        -ExpectedClassesSha256 $classesLcb.sha256 `
        -NetworkRoot $networkRoot `
        -ExpectedNetworkInventorySha256 $network.inventorySha256

    $createdSnapshot = $false
    $createdManifest = $false
    try {
        $snapshotStream = [IO.File]::Open(
            $snapshotPath,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        $createdSnapshot = $true
        try {
            $snapshotStream.Write($sourceBytes, 0, $sourceBytes.Length)
            $snapshotStream.Flush($true)
        }
        finally {
            $snapshotStream.Dispose()
        }

        $manifestStream = [IO.File]::Open(
            $manifestPath,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        $createdManifest = $true
        try {
            $manifestStream.Write(
                $manifestBytes, 0, $manifestBytes.Length)
            $manifestStream.Flush($true)
        }
        finally {
            $manifestStream.Dispose()
        }

        if ((Get-FileSha256 -Path $snapshotPath) -cne
            $sourcePhysicalSha256) {
            throw 'Captured source bytes do not match the validated source.'
        }
        Assert-CaptureInputsUnchanged `
            -Root $root `
            -SourcePath $sourcePath `
            -ExpectedSourceSha256 $sourcePhysicalSha256 `
            -ClassesLcbPath $classesLcbPath `
            -ExpectedClassesSha256 $classesLcb.sha256 `
            -NetworkRoot $networkRoot `
            -ExpectedNetworkInventorySha256 $network.inventorySha256
    }
    catch {
        if ($createdManifest -and [IO.File]::Exists($manifestPath)) {
            [IO.File]::Delete($manifestPath)
        }
        if ($createdSnapshot -and [IO.File]::Exists($snapshotPath)) {
            [IO.File]::Delete($snapshotPath)
        }
        throw
    }
}

$manifest | ConvertTo-Json -Depth 12

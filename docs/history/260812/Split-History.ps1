[CmdletBinding()]
param(
    [string]$SourcePath,
    [string]$OutputDirectory,
    [int]$ChunkLineCount = 250,
    [switch]$ReplaceGeneratedOutputs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$historyDirectory = Split-Path -Parent $scriptDirectory

if ([string]::IsNullOrWhiteSpace($SourcePath)) {
    $SourcePath = Join-Path $historyDirectory 'Elmo_Master_history_260812.md'
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = $scriptDirectory
}

if ($ChunkLineCount -lt 1) {
    throw 'ChunkLineCount must be at least 1.'
}

$SourcePath = [System.IO.Path]::GetFullPath($SourcePath)
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$baseName = 'Elmo_Master_history_260812'
$manifestPath = Join-Path $OutputDirectory 'split_manifest.json'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false, $true)

function ConvertTo-HexString {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    return ([System.BitConverter]::ToString($Bytes)).Replace('-', '')
}

function Test-ByteArrayEqual {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Left,
        [Parameter(Mandatory = $true)][byte[]]$Right
    )

    if ($Left.Length -ne $Right.Length) {
        return $false
    }

    for ($index = 0; $index -lt $Left.Length; $index++) {
        if ($Left[$index] -ne $Right[$index]) {
            return $false
        }
    }

    return $true
}

function Get-RelativePathPortable {
    param(
        [Parameter(Mandatory = $true)][string]$BaseDirectory,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BaseDirectory).TrimEnd('\') + '\'
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = [System.Uri]$baseFullPath
    $targetUri = [System.Uri]$targetFullPath
    if ($baseUri.Scheme -ne $targetUri.Scheme) {
        return $targetFullPath
    }

    return [System.Uri]::UnescapeDataString(
        $baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
    throw "Source history was not found: $SourcePath"
}

[System.IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null

$generatedOutputs = @(
    Get-ChildItem -LiteralPath $OutputDirectory -File -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Name -like "$baseName`_part_*.md" -or
            $_.Name -eq 'split_manifest.json'
        }
)

if ($generatedOutputs.Count -gt 0 -and -not $ReplaceGeneratedOutputs) {
    throw "Generated outputs already exist. Refusing to overwrite: $($generatedOutputs.Name -join ', ')"
}

if ($ReplaceGeneratedOutputs) {
    foreach ($item in $generatedOutputs) {
        Remove-Item -LiteralPath $item.FullName -Force
    }
}

$sourceBytes = [System.IO.File]::ReadAllBytes($SourcePath)
$sourceHashBefore = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourcePath).Hash
$hasUtf8Bom = (
    $sourceBytes.Length -ge 3 -and
    $sourceBytes[0] -eq 0xEF -and
    $sourceBytes[1] -eq 0xBB -and
    $sourceBytes[2] -eq 0xBF
)

if ($hasUtf8Bom) {
    throw 'This splitter expects UTF-8 without BOM so the byte rejoin remains exact.'
}

$sourceText = $utf8NoBom.GetString($sourceBytes)
$crlfCount = [System.Text.RegularExpressions.Regex]::Matches($sourceText, "`r`n").Count
$bareLfCount = [System.Text.RegularExpressions.Regex]::Matches($sourceText, "(?<!`r)`n").Count
$bareCrCount = [System.Text.RegularExpressions.Regex]::Matches($sourceText, "`r(?!`n)").Count
$hasFinalCrlf = $sourceText.EndsWith("`r`n", [System.StringComparison]::Ordinal)

if ($bareLfCount -ne 0 -or $bareCrCount -ne 0 -or -not $hasFinalCrlf) {
    throw 'This splitter expects CRLF-only input with a final CRLF.'
}

$lines = [System.IO.File]::ReadAllLines($SourcePath, $utf8NoBom)
if ($lines.Count -ne $crlfCount) {
    throw "Line-count mismatch: ReadAllLines=$($lines.Count), CRLF=$crlfCount"
}

$chunks = [System.Collections.Generic.List[object]]::new()
$partCount = [int][System.Math]::Ceiling($lines.Count / [double]$ChunkLineCount)

for ($part = 1; $part -le $partCount; $part++) {
    $startIndex = ($part - 1) * $ChunkLineCount
    $count = [System.Math]::Min($ChunkLineCount, $lines.Count - $startIndex)
    $startLine = $startIndex + 1
    $endLine = $startIndex + $count
    $fileName = '{0}_part_{1:D3}_lines_{2:D5}_{3:D5}.md' -f (
        $baseName,
        $part,
        $startLine,
        $endLine
    )
    $chunkPath = Join-Path $OutputDirectory $fileName
    $chunkText = [string]::Join("`r`n", $lines[$startIndex..($startIndex + $count - 1)]) + "`r`n"
    [System.IO.File]::WriteAllText($chunkPath, $chunkText, $utf8NoBom)

    $chunkItem = Get-Item -LiteralPath $chunkPath
    [void]$chunks.Add([pscustomobject][ordered]@{
        part = $part
        path = $fileName
        startLine = $startLine
        endLine = $endLine
        lineCount = $count
        bytes = $chunkItem.Length
        sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $chunkPath).Hash
    })
}

$rejoinedBytes = [System.IO.MemoryStream]::new()
try {
    foreach ($chunk in $chunks) {
        $chunkBytes = [System.IO.File]::ReadAllBytes((Join-Path $OutputDirectory $chunk.path))
        $rejoinedBytes.Write($chunkBytes, 0, $chunkBytes.Length)
    }
    $rejoinedArray = $rejoinedBytes.ToArray()
}
finally {
    $rejoinedBytes.Dispose()
}

$sha256 = [System.Security.Cryptography.SHA256]::Create()
try {
    $rejoinedHash = ConvertTo-HexString -Bytes ($sha256.ComputeHash($rejoinedArray))
}
finally {
    $sha256.Dispose()
}

$sourceHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $SourcePath).Hash
$byteRejoinMatchesSource = Test-ByteArrayEqual -Left $sourceBytes -Right $rejoinedArray

$manifest = [ordered]@{
    generatedAt = [System.DateTimeOffset]::Now.ToString('o')
    source = [ordered]@{
        path = (Get-RelativePathPortable `
                -BaseDirectory $historyDirectory `
                -TargetPath $SourcePath).Replace('\', '/')
        bytes = $sourceBytes.Length
        lines = $lines.Count
        encoding = 'UTF-8 without BOM'
        lineEndings = 'CRLF'
        finalCrlf = $hasFinalCrlf
        sha256Before = $sourceHashBefore
        sha256After = $sourceHashAfter
        unchanged = ($sourceHashBefore -eq $sourceHashAfter)
    }
    split = [ordered]@{
        chunkLineCount = $ChunkLineCount
        chunkCount = $chunks.Count
        totalChunkBytes = ($chunks | Measure-Object -Property bytes -Sum).Sum
        totalChunkLines = ($chunks | Measure-Object -Property lineCount -Sum).Sum
        rejoinedBytes = $rejoinedArray.Length
        rejoinedSha256 = $rejoinedHash
        byteRejoinMatchesSource = $byteRejoinMatchesSource
        hashRejoinMatchesSource = ($rejoinedHash -eq $sourceHashBefore)
    }
    chunks = $chunks
}

[System.IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 8) + "`n",
    [System.Text.UTF8Encoding]::new($false)
)

if (-not $manifest.source.unchanged) {
    throw 'Source hash changed during splitting.'
}

if (-not $manifest.split.byteRejoinMatchesSource -or -not $manifest.split.hashRejoinMatchesSource) {
    throw 'Chunk rejoin does not match the source.'
}

[pscustomobject]@{
    Source = $SourcePath
    SourceBytes = $sourceBytes.Length
    SourceLines = $lines.Count
    SourceSha256 = $sourceHashBefore
    ChunkCount = $chunks.Count
    ByteRejoinMatchesSource = $byteRejoinMatchesSource
    RejoinedSha256 = $rejoinedHash
    Manifest = $manifestPath
}

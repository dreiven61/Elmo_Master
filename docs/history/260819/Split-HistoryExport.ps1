[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [ValidateRange(50, 2000)]
    [int]$LinesPerChunk = 250,

    [ValidateRange(1000, 100000)]
    [int]$OversizedLineThreshold = 4000,

    [switch]$RefreshGenerated
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-TextSha256 {
    param([Parameter(Mandatory = $true)][string]$Text)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
        return ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function Get-Preview {
    param([Parameter(Mandatory = $true)][string]$Text)

    $preview = $Text
    $base64Marker = $preview.IndexOf('base64,', [System.StringComparison]::OrdinalIgnoreCase)
    if ($base64Marker -ge 0) {
        $preview = $preview.Substring(0, [Math]::Min($base64Marker + 7, 240)) + '[payload]'
    }
    elseif ($preview.Length -gt 240) {
        $preview = $preview.Substring(0, 240) + '...'
    }

    return (($preview -replace '\s+', ' ').Replace('|', '\|').Trim())
}

function Get-TopicHint {
    param([AllowEmptyString()][string[]]$Lines)

    $knownTopics = @(
        'SetPosition',
        'LMCSetPositionStore',
        'LMCEcatInputLatch',
        'HomeDS402Ex',
        'callback',
        'EventMask',
        'Gate D',
        'reconnect',
        'journal',
        'Servo On',
        'C78',
        'SRAMRETAIN',
        'Distribution',
        'SourceOnly',
        'LASAL',
        'WPF',
        'cleanup',
        'history',
        'TestClass'
    )

    $joined = [string]::Join("`n", $Lines)
    $hits = foreach ($topic in $knownTopics) {
        if ($joined.IndexOf($topic, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $topic
        }
    }

    if (@($hits).Count -gt 0) {
        return ((@($hits) | Select-Object -First 6) -join ', ')
    }

    foreach ($line in $Lines) {
        $candidate = ($line -replace '^\s*>\s?', '').Trim()
        if ($candidate.Length -eq 0) { continue }
        if ($candidate -match '^(MCP tool call|Ran a command|Called .*tool|```|\{|\}|\[OVERSIZED_)') { continue }
        if ($candidate.Length -gt 100) {
            $candidate = $candidate.Substring(0, 100) + '...'
        }
        return $candidate.Replace('|', '\|')
    }

    return 'No short-text hint; inspect the chunk directly.'
}

$source = (Resolve-Path -LiteralPath $SourcePath).Path
$sourceItem = Get-Item -LiteralPath $source
$output = [System.IO.Path]::GetFullPath($OutputDirectory)
$scriptPath = [System.IO.Path]::GetFullPath($PSCommandPath)

if (-not (Test-Path -LiteralPath $output)) {
    [System.IO.Directory]::CreateDirectory($output) | Out-Null
}

$existing = @(Get-ChildItem -LiteralPath $output -File -Force | Where-Object {
    [System.IO.Path]::GetFullPath($_.FullName) -ne $scriptPath
})
$unexpectedExisting = @($existing | Where-Object {
    $_.Name -ne '00_index.md' -and
    $_.Name -ne '01_omitted_payload_manifest.csv' -and
    $_.Name -ne '99_analysis_summary.md' -and
    $_.Name -notmatch '^part-\d{3}-lines-\d{5}-\d{5}\.md$'
})
if ($unexpectedExisting.Count -ne 0) {
    throw "Output directory contains unexpected files: $($unexpectedExisting.Name -join ', ')"
}
if ($existing.Count -ne 0 -and -not $RefreshGenerated) {
    throw "Generated output already exists; pass -RefreshGenerated to replace only the known generated files in: $output"
}

$sourceHashBefore = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$sourceLines = [System.IO.File]::ReadAllLines($source)
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$chunkCount = [Math]::Ceiling($sourceLines.Count / [double]$LinesPerChunk)
$partWidth = [Math]::Max(3, $chunkCount.ToString().Length)
$lineWidth = [Math]::Max(5, $sourceLines.Count.ToString().Length)
$omissions = [System.Collections.Generic.List[object]]::new()
$chunkRows = [System.Collections.Generic.List[object]]::new()

for ($chunkIndex = 0; $chunkIndex -lt $chunkCount; $chunkIndex++) {
    $startIndex = $chunkIndex * $LinesPerChunk
    $endIndex = [Math]::Min($sourceLines.Count - 1, $startIndex + $LinesPerChunk - 1)
    $chunkLines = [System.Collections.Generic.List[string]]::new()

    for ($lineIndex = $startIndex; $lineIndex -le $endIndex; $lineIndex++) {
        $sourceLine = $sourceLines[$lineIndex]
        if ($sourceLine.Length -le $OversizedLineThreshold) {
            $chunkLines.Add($sourceLine)
            continue
        }

        $reason = if (
            $sourceLine.IndexOf('base64', [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $sourceLine.IndexOf('data:image', [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $sourceLine.IndexOf('image/jpeg', [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
            $sourceLine.IndexOf('image/png', [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        ) {
            'embedded-image-or-base64'
        }
        else {
            'oversized-tool-output'
        }

        $textHash = Get-TextSha256 -Text $sourceLine
        $preview = Get-Preview -Text $sourceLine
        $quotePrefix = if ($sourceLine -match '^(\s*>\s*)') { $Matches[1] } else { '' }
        $placeholder = ('{0}[OVERSIZED_LINE_OMITTED source_line={1} chars={2} reason={3} text_utf8_sha256={4} preview="{5}"]' -f
            $quotePrefix, ($lineIndex + 1), $sourceLine.Length, $reason, $textHash, $preview.Replace('"', "'"))
        $chunkLines.Add($placeholder)
        $omissions.Add([pscustomobject]@{
            SourceLine = $lineIndex + 1
            OriginalCharacters = $sourceLine.Length
            Reason = $reason
            TextUtf8Sha256 = $textHash
            Preview = $preview
        })
    }

    $partNumber = ($chunkIndex + 1).ToString("D$partWidth")
    $startLine = ($startIndex + 1).ToString("D$lineWidth")
    $endLine = ($endIndex + 1).ToString("D$lineWidth")
    $fileName = "part-$partNumber-lines-$startLine-$endLine.md"
    $filePath = Join-Path $output $fileName
    [System.IO.File]::WriteAllLines($filePath, $chunkLines, $utf8NoBom)

    $chunkRows.Add([pscustomobject]@{
        Part = $chunkIndex + 1
        FileName = $fileName
        StartLine = $startIndex + 1
        EndLine = $endIndex + 1
        SourceLineCount = $endIndex - $startIndex + 1
        OutputBytes = (Get-Item -LiteralPath $filePath).Length
        OmittedLines = @($omissions | Where-Object { $_.SourceLine -ge ($startIndex + 1) -and $_.SourceLine -le ($endIndex + 1) }).Count
        TopicHint = Get-TopicHint -Lines $chunkLines
    })
}

$manifestPath = Join-Path $output '01_omitted_payload_manifest.csv'
$omissions | Export-Csv -LiteralPath $manifestPath -NoTypeInformation -Encoding utf8

$relativeSource = [System.IO.Path]::GetRelativePath($output, $source).Replace('\', '/')
$chunkByteStats = $chunkRows | Measure-Object -Property OutputBytes -Sum -Maximum
$indexLines = [System.Collections.Generic.List[string]]::new()
$indexLines.Add('# Elmo Master history split index - 2026-08-19')
$indexLines.Add('')
$indexLines.Add('작성일: 2026-08-19 (KST)')
$indexLines.Add('')
$indexLines.Add('## Source and integrity')
$indexLines.Add('')
$indexLines.Add("- Source: ``$relativeSource``")
$indexLines.Add("- Source bytes: $($sourceItem.Length)")
$indexLines.Add("- Source lines: $($sourceLines.Count)")
$indexLines.Add("- Source SHA-256: ``$sourceHashBefore``")
$indexLines.Add("- Split rule: $LinesPerChunk source lines per chunk")
$indexLines.Add("- Oversized-line rule: source lines longer than $OversizedLineThreshold characters are replaced only in split copies")
$indexLines.Add("- Omitted oversized lines: $($omissions.Count)")
$indexLines.Add("- Omission manifest: [01_omitted_payload_manifest.csv](./01_omitted_payload_manifest.csv)")
$indexLines.Add('- Splitter: [Split-HistoryExport.ps1](./Split-HistoryExport.ps1)')
$indexLines.Add("- Readable chunk total: $($chunkByteStats.Sum) bytes; maximum chunk: $($chunkByteStats.Maximum) bytes")
$indexLines.Add('- The original source file is unchanged and remains the lossless record.')
$indexLines.Add('')
$indexLines.Add('## Topic navigation')
$indexLines.Add('')
$indexLines.Add('The ranges below follow transcript order, not trustworthy per-turn timestamps.')
$indexLines.Add('')
$indexLines.Add('| Source lines | Parts | Topic | Evidence boundary |')
$indexLines.Add('|---:|---:|---|---|')
$indexLines.Add('| 1-497 | [001](./part-001-lines-00001-00250.md)-[002](./part-002-lines-00251-00500.md) | 260813 handoff, reconnect V2, shared WPF journal interlock | History + PC/source diagnosis |')
$indexLines.Add('| 498-4564 | [002](./part-002-lines-00251-00500.md)-[019](./part-019-lines-04501-04750.md) | recovery quarantine/retirement and Close/X recovery | PC tests + limited live UI; Servo remained gated |')
$indexLines.Add('| 4565-9205 | [019](./part-019-lines-04501-04750.md)-[037](./part-037-lines-09001-09250.md) | collapsible recovery UI, TW19 default and terminal observation | SDO terminal observed; physical multi-turn effect open |')
$indexLines.Add('| 9206-13887 | [037](./part-037-lines-09001-09250.md)-[056](./part-056-lines-13751-14000.md) | SetPosition query/retire and retained-store design | PC/static; activation OFF |')
$indexLines.Add('| 13888-25510 | [056](./part-056-lines-13751-14000.md)-[103](./part-103-lines-25501-25750.md) | LASAL Store/CheckSum/Network/method creation | IDE/C78 only; no PLC download |')
$indexLines.Add('| 25511-34539 | [103](./part-103-lines-25501-25750.md)-[139](./part-139-lines-34501-34750.md) | retained Store lifecycle and Control route implementation | PC/static/C78; dormant |')
$indexLines.Add('| 34540-35622 | [139](./part-139-lines-34501-34750.md)-[143](./part-143-lines-35501-35750.md) | Gate D rebaseline and method-size blocker | Artifact/static only |')
$indexLines.Add('| 35623-46825 | [143](./part-143-lines-35501-35750.md)-[188](./part-188-lines-46751-47000.md) | handler/dispatcher split, coordinate and ownership dormant slice | Source/static/C78; no runtime |')
$indexLines.Add('| 46826-52551 | [188](./part-188-lines-46751-47000.md)-[211](./part-211-lines-52501-52750.md) | RT task audit and native-zero preflight declarations | Design/IDE declarations |')
$indexLines.Add('| 52552-53754 | [211](./part-211-lines-52501-52750.md)-[216](./part-216-lines-53751-53925.md) | P0 preflight implementation, verification, docs and commits | PC/static/C78; activation OFF |')
$indexLines.Add('| 53755-53925 | [216](./part-216-lines-53751-53925.md) | scoped cache/test/history cleanup | Local filesystem + commits |')
$indexLines.Add('')
$indexLines.Add('## Chunks')
$indexLines.Add('')
$indexLines.Add('| Part | Source lines | File | Bytes | Omitted lines | Topic hint |')
$indexLines.Add('|---:|---:|---|---:|---:|---|')
foreach ($row in $chunkRows) {
    $indexLines.Add("| $($row.Part) | $($row.StartLine)-$($row.EndLine) | [$($row.FileName)](./$($row.FileName)) | $($row.OutputBytes) | $($row.OmittedLines) | $($row.TopicHint) |")
}
$indexLines.Add('')
$indexLines.Add('## Resume artifact')
$indexLines.Add('')
$indexLines.Add('- Read [99_analysis_summary.md](./99_analysis_summary.md) after the chunk analysis is complete.')

[System.IO.File]::WriteAllLines((Join-Path $output '00_index.md'), $indexLines, $utf8NoBom)

$sourceHashAfter = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
if ($sourceHashAfter -ne $sourceHashBefore) {
    throw 'Source history hash changed during splitting.'
}

$writtenParts = @(Get-ChildItem -LiteralPath $output -Filter 'part-*.md' -File | Sort-Object Name)
$writtenLineCount = 0
$maxOutputLineLength = 0
foreach ($part in $writtenParts) {
    $partLines = [System.IO.File]::ReadAllLines($part.FullName)
    $writtenLineCount += $partLines.Count
    foreach ($line in $partLines) {
        if ($line.Length -gt $maxOutputLineLength) {
            $maxOutputLineLength = $line.Length
        }
    }
}

if ($writtenParts.Count -ne $chunkCount) {
    throw "Chunk count mismatch: expected $chunkCount, found $($writtenParts.Count)."
}
if ($writtenLineCount -ne $sourceLines.Count) {
    throw "Line coverage mismatch: expected $($sourceLines.Count), found $writtenLineCount."
}

[pscustomobject]@{
    Source = $source
    SourceBytes = $sourceItem.Length
    SourceLines = $sourceLines.Count
    SourceSha256 = $sourceHashAfter
    Chunks = $writtenParts.Count
    CoveredLines = $writtenLineCount
    OmittedOversizedLines = $omissions.Count
    MaxOutputLineLength = $maxOutputLineLength
    OutputDirectory = $output
}

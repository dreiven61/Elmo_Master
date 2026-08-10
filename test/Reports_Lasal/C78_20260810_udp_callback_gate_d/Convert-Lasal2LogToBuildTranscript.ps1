[CmdletBinding()]
param(
    [string]$BaselinePath,
    [string]$LasalLogPath,
    [string]$OutputPath,
    [long]$LogEndOffset = -1,
    [string]$RawDeltaOutputPath,
    [string]$RawDeltaManifestPath,
    [switch]$RawDeltaOnly,
    [ValidateSet('Historical', 'GateDVisualLayout')]
    [string]$EvidenceProfile = 'Historical'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Owner = 'LASAL.DerivedBuildTranscript'
$ExpectedBaselineSchema = 'LasalC78RebuildEvidence/v1'
$HistoricalEvidenceProfile = 'Historical'
$GateDVisualLayoutEvidenceProfile = 'GateDVisualLayout'
$BoundedDeltaSchema = 'LasalC78BoundedLogDelta/v1'
$ExpectedHeader = (
    'Rebuild project with compiler version C78 (target architecture: ARM)')
$Utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$GateDVisualLayoutEvidenceRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiver/_UDPTransceiver.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
)
$GateDVisualLayoutRequiredCompileRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCUdpCallbackSender/LMCUdpCallbackSender.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/_UDPTransceiver/_UDPTransceiver.st'
)
$GateDVisualLayoutExpectedRegeneratedOutputRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
)

if ([string]::IsNullOrWhiteSpace($BaselinePath)) {
    $BaselinePath = Join-Path $PSScriptRoot 'build_baseline.json'
}
if ([string]::IsNullOrWhiteSpace($LasalLogPath)) {
    $LasalLogPath = Join-Path $env:TEMP 'Lasal2.log'
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path `
        $PSScriptRoot `
        'derived_build_transcript_from_lasal2_log.txt'
}

function Throw-ConversionBlocker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    throw "$Owner blocker: $Message"
}

function Assert-ExactStringSequence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OwnerName,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Actual,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Expected
    )

    if ($Actual.Count -ne $Expected.Count) {
        Throw-ConversionBlocker (
            "$OwnerName count is $($Actual.Count), expected $($Expected.Count).")
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if ($Actual[$index] -cne $Expected[$index]) {
            Throw-ConversionBlocker (
                "$OwnerName entry $index is '$($Actual[$index])', expected " +
                "'$($Expected[$index])'.")
        }
    }
}

function Get-NormalizedFullPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
}

function Test-PathIdentity {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Left,
        [Parameter(Mandatory = $true)]
        [string]$Right
    )

    return [string]::Equals(
        (Get-NormalizedFullPath -Path $Left),
        (Get-NormalizedFullPath -Path $Right),
        [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-Sha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [int]$Offset,
        [Parameter(Mandatory = $true)]
        [int]$Count
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
                $sha256.ComputeHash($Bytes, $Offset, $Count))).Replace(
            '-', '').ToLowerInvariant()
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-RegeneratedOutputIdentities {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $identities = @()
    foreach ($relativePath in
        $GateDVisualLayoutExpectedRegeneratedOutputRelativePaths) {
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $RepositoryRoot $relativePath)
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            Throw-ConversionBlocker (
                "regenerated output does not exist: $relativePath")
        }
        $bytes = [System.IO.File]::ReadAllBytes($fullPath)
        $identities += [ordered]@{
            RelativePath = $relativePath
            Bytes = [long]$bytes.LongLength
            Sha256 = Get-Sha256Hex `
                -Bytes $bytes `
                -Offset 0 `
                -Count $bytes.Length
        }
    }
    return $identities
}

function Assert-RegeneratedOutputIdentitiesEqual {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Expected,
        [Parameter(Mandatory = $true)]
        [object[]]$Actual
    )

    if ($Expected.Count -ne $Actual.Count) {
        Throw-ConversionBlocker (
            'regenerated output identity count changed during manifest export.')
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if ([string]$Expected[$index].RelativePath -cne
                [string]$Actual[$index].RelativePath -or
            [long]$Expected[$index].Bytes -ne [long]$Actual[$index].Bytes -or
            [string]$Expected[$index].Sha256 -cne
                [string]$Actual[$index].Sha256) {
            Throw-ConversionBlocker (
                'regenerated output changed during manifest export: ' +
                [string]$Expected[$index].RelativePath)
        }
    }
}

function Get-IndexedLogLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $rawLines = $Text -split '\r?\n'
    $lines = @()
    for ($index = 0; $index -lt $rawLines.Count; $index++) {
        $rawLine = $rawLines[$index].TrimEnd("`r")
        $pidValue = $null
        $tidValue = $null
        $levelValue = $null
        $ownerValue = $null
        $messageValue = $null

        $identityMatch = [regex]::Match(
            $rawLine,
            '^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*\]')
        if ($identityMatch.Success) {
            $pidValue = $identityMatch.Groups['Pid'].Value
            $tidValue = $identityMatch.Groups['Tid'].Value
        }

        $detailMatch = [regex]::Match(
            $rawLine,
            ('^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*?' +
                '\((?<Level>INFO|WARN|ERROR|DEBUG)\)\s+' +
                '(?<Owner>[^\]]+)\]\s*(?<Message>.*)$'))
        if ($detailMatch.Success) {
            $levelValue = $detailMatch.Groups['Level'].Value
            $ownerValue = $detailMatch.Groups['Owner'].Value
            $messageValue = $detailMatch.Groups['Message'].Value
        }

        $lines += [pscustomobject]@{
            Index = $index
            Text = $rawLine
            Pid = $pidValue
            Tid = $tidValue
            Level = $levelValue
            Owner = $ownerValue
            Message = $messageValue
        }
    }
    return $lines
}

$resolvedBaselinePath = Get-NormalizedFullPath -Path $BaselinePath
$resolvedLogPath = Get-NormalizedFullPath -Path $LasalLogPath
$resolvedOutputPath = Get-NormalizedFullPath -Path $OutputPath
$hasRawDeltaOutput = -not [string]::IsNullOrWhiteSpace($RawDeltaOutputPath)
$hasRawDeltaManifest = -not [string]::IsNullOrWhiteSpace($RawDeltaManifestPath)
if ($hasRawDeltaOutput -ne $hasRawDeltaManifest) {
    Throw-ConversionBlocker (
        'RawDeltaOutputPath and RawDeltaManifestPath must be supplied together.')
}
if ($RawDeltaOnly.IsPresent -and -not $hasRawDeltaOutput) {
    Throw-ConversionBlocker 'RawDeltaOnly requires both raw delta output paths.'
}
if ($EvidenceProfile -ceq $GateDVisualLayoutEvidenceProfile -and
    -not $hasRawDeltaOutput) {
    Throw-ConversionBlocker (
        'GateDVisualLayout conversion requires raw delta and manifest outputs.')
}
$resolvedRawDeltaOutputPath = $null
$resolvedRawDeltaManifestPath = $null
if ($hasRawDeltaOutput) {
    $resolvedRawDeltaOutputPath = Get-NormalizedFullPath -Path $RawDeltaOutputPath
    $resolvedRawDeltaManifestPath = Get-NormalizedFullPath -Path $RawDeltaManifestPath
}

if (-not (Test-Path -LiteralPath $resolvedBaselinePath -PathType Leaf)) {
    Throw-ConversionBlocker "baseline file does not exist: $resolvedBaselinePath"
}
if (-not (Test-Path -LiteralPath $resolvedLogPath -PathType Leaf)) {
    Throw-ConversionBlocker "Lasal2.log does not exist: $resolvedLogPath"
}
if (-not $RawDeltaOnly.IsPresent -and
    (Test-Path -LiteralPath $resolvedOutputPath)) {
    Throw-ConversionBlocker "refusing to overwrite existing output: $resolvedOutputPath"
}
$existingTranscriptBytes = $null
if ($RawDeltaOnly.IsPresent) {
    if (-not (Test-Path -LiteralPath $resolvedOutputPath -PathType Leaf)) {
        Throw-ConversionBlocker (
            "RawDeltaOnly requires the existing transcript: $resolvedOutputPath")
    }
    $existingTranscriptBytes = [System.IO.File]::ReadAllBytes($resolvedOutputPath)
}
$outputDirectory = Split-Path -Parent $resolvedOutputPath
if (-not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
    Throw-ConversionBlocker "output directory does not exist: $outputDirectory"
}
if ($hasRawDeltaOutput) {
    foreach ($rawOutputPath in @(
            $resolvedRawDeltaOutputPath,
            $resolvedRawDeltaManifestPath)) {
        if (Test-Path -LiteralPath $rawOutputPath) {
            Throw-ConversionBlocker (
                "refusing to overwrite existing raw evidence: $rawOutputPath")
        }
        $rawOutputDirectory = Split-Path -Parent $rawOutputPath
        if (-not (Test-Path -LiteralPath $rawOutputDirectory -PathType Container)) {
            Throw-ConversionBlocker (
                "raw evidence directory does not exist: $rawOutputDirectory")
        }
    }
}

try {
    $baseline = Get-Content -Raw -LiteralPath $resolvedBaselinePath |
        ConvertFrom-Json
}
catch {
    Throw-ConversionBlocker "baseline JSON is invalid: $($_.Exception.Message)"
}

$requiredBaselineProperties = @(
    'Schema',
    'CanonicalProjectPath',
    'LasalLogPath',
    'LogPrefixLength',
    'LogPrefixSha256')
foreach ($propertyName in $requiredBaselineProperties) {
    if ($baseline.PSObject.Properties.Name -cnotcontains $propertyName) {
        Throw-ConversionBlocker "baseline property is missing: $propertyName"
    }
}
if ($baseline.Schema -cne $ExpectedBaselineSchema) {
    Throw-ConversionBlocker (
        "baseline schema is '$($baseline.Schema)', expected " +
        "'$ExpectedBaselineSchema'.")
}
$baselineEvidenceProfile = $HistoricalEvidenceProfile
$resolvedRepositoryRoot = $null
if ($baseline.PSObject.Properties.Name -contains 'EvidenceProfile') {
    $baselineEvidenceProfile = [string]$baseline.EvidenceProfile
}
if ($baselineEvidenceProfile -cnotin @(
        $HistoricalEvidenceProfile,
        $GateDVisualLayoutEvidenceProfile)) {
    Throw-ConversionBlocker (
        "baseline evidence profile is invalid: $baselineEvidenceProfile")
}
if ($baselineEvidenceProfile -cne $EvidenceProfile) {
    Throw-ConversionBlocker (
        "baseline evidence profile is '$baselineEvidenceProfile', requested " +
        "'$EvidenceProfile'.")
}
if ($EvidenceProfile -ceq $GateDVisualLayoutEvidenceProfile) {
    foreach ($propertyName in @('EvidenceProfile', 'RepositoryRoot', 'Files',
            'RequiredCompileRelativePaths')) {
        if ($baseline.PSObject.Properties.Name -cnotcontains $propertyName) {
            Throw-ConversionBlocker (
                "GateDVisualLayout baseline property is missing: $propertyName")
        }
    }
    $baselineFiles = @($baseline.Files)
    $baselineEvidencePaths = @()
    foreach ($baselineFile in $baselineFiles) {
        foreach ($propertyName in @('RelativePath', 'Role', 'Sha256')) {
            if ($baselineFile.PSObject.Properties.Name -cnotcontains
                $propertyName) {
                Throw-ConversionBlocker (
                    "GateDVisualLayout baseline file property is missing: " +
                    $propertyName)
            }
        }
        $relativePath = [string]$baselineFile.RelativePath
        $expectedRole = 'inputIdentity'
        if ($GateDVisualLayoutExpectedRegeneratedOutputRelativePaths -ccontains
            $relativePath) {
            $expectedRole = 'expectedRegeneratedOutput'
        }
        if ([string]$baselineFile.Role -cne $expectedRole) {
            Throw-ConversionBlocker (
                "GateDVisualLayout baseline role for $relativePath is " +
                "'$($baselineFile.Role)', expected '$expectedRole'.")
        }
        $baselineEvidencePaths += $relativePath
    }
    Assert-ExactStringSequence `
        -OwnerName 'GateDVisualLayout baseline evidence inventory' `
        -Actual $baselineEvidencePaths `
        -Expected $GateDVisualLayoutEvidenceRelativePaths
    Assert-ExactStringSequence `
        -OwnerName 'GateDVisualLayout baseline required compile inventory' `
        -Actual @($baseline.RequiredCompileRelativePaths | ForEach-Object {
                [string]$_
            }) `
        -Expected $GateDVisualLayoutRequiredCompileRelativePaths
    $resolvedRepositoryRoot = Get-NormalizedFullPath -Path (
        [string]$baseline.RepositoryRoot)
    if (-not (Test-Path -LiteralPath $resolvedRepositoryRoot -PathType Container)) {
        Throw-ConversionBlocker (
            "GateDVisualLayout repository root does not exist: " +
            $resolvedRepositoryRoot)
    }
    $runningLasal = @(Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
    if ($runningLasal.Count -ne 0) {
        Throw-ConversionBlocker (
            'GateDVisualLayout conversion requires Lasal2 to be stopped.')
    }
}
if (-not (Test-PathIdentity -Left $baseline.LasalLogPath -Right $resolvedLogPath)) {
    Throw-ConversionBlocker (
        "Lasal2.log path differs from baseline: $resolvedLogPath")
}
if ([string]::IsNullOrWhiteSpace($baseline.CanonicalProjectPath)) {
    Throw-ConversionBlocker 'baseline canonical project path is empty.'
}
if ([string]::IsNullOrWhiteSpace($baseline.LogPrefixSha256) -or
    $baseline.LogPrefixSha256 -notmatch '^[0-9a-fA-F]{64}$') {
    Throw-ConversionBlocker 'baseline log prefix SHA-256 is invalid.'
}

try {
    [long]$prefixLength = $baseline.LogPrefixLength
}
catch {
    Throw-ConversionBlocker 'baseline log prefix length is not an integer.'
}
if ($prefixLength -lt 0 -or $prefixLength -gt [int]::MaxValue) {
    Throw-ConversionBlocker "unsupported baseline log prefix length: $prefixLength"
}

$logBytes = [System.IO.File]::ReadAllBytes($resolvedLogPath)
if ($logBytes.LongLength -lt $prefixLength) {
    Throw-ConversionBlocker (
        "Lasal2.log was truncated: current length $($logBytes.LongLength), " +
        "baseline prefix length $prefixLength")
}
$actualPrefixSha256 = Get-Sha256Hex `
    -Bytes $logBytes `
    -Offset 0 `
    -Count ([int]$prefixLength)
if ($actualPrefixSha256 -cne $baseline.LogPrefixSha256.ToLowerInvariant()) {
    Throw-ConversionBlocker (
        "Lasal2.log prefix SHA-256 is $actualPrefixSha256, expected " +
        "$($baseline.LogPrefixSha256.ToLowerInvariant()).")
}

$effectiveLogEndOffset = $logBytes.LongLength
if ($LogEndOffset -ge 0) {
    $effectiveLogEndOffset = $LogEndOffset
}
if ($effectiveLogEndOffset -lt $prefixLength -or
    $effectiveLogEndOffset -gt $logBytes.LongLength -or
    $effectiveLogEndOffset -gt [int]::MaxValue) {
    Throw-ConversionBlocker (
        "bounded log end offset $effectiveLogEndOffset is outside " +
        "[$prefixLength,$($logBytes.LongLength)].")
}
$appendedByteCount = [int]($effectiveLogEndOffset - $prefixLength)
if ($appendedByteCount -eq 0) {
    Throw-ConversionBlocker 'no Lasal2.log bytes were appended after the baseline.'
}
try {
    $appendedLogText = $Utf8Strict.GetString(
        $logBytes,
        [int]$prefixLength,
        $appendedByteCount)
}
catch {
    Throw-ConversionBlocker (
        "appended Lasal2.log bytes are not valid UTF-8: $($_.Exception.Message)")
}
$lines = @(Get-IndexedLogLines -Text $appendedLogText)

$sessionStarts = @($lines | Where-Object {
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'GUI' -and
        $_.Message -match (
            '^Start Application at \d{4}-\d{2}-\d{2} ' +
            '\d{2}:\d{2}:\d{2}$')
    })
if ($sessionStarts.Count -ne 1) {
    Throw-ConversionBlocker (
        "appended GUI session count is $($sessionStarts.Count), expected 1.")
}
$sessionStart = $sessionStarts[0]

$appendedPids = @($lines | Where-Object {
        -not [string]::IsNullOrEmpty($_.Pid)
    } | Select-Object -ExpandProperty Pid -Unique)
if ($appendedPids.Count -ne 1 -or $appendedPids[0] -cne $sessionStart.Pid) {
    Throw-ConversionBlocker (
        'appended log contains PID-tagged lines outside the single GUI session.')
}

$rebuildCommands = @($lines | Where-Object {
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'CmdProc' -and
        $_.Message -ceq "Executing command 'Rebuild project'"
    })
if ($rebuildCommands.Count -ne 1) {
    Throw-ConversionBlocker (
        "appended Rebuild command count is $($rebuildCommands.Count), expected 1.")
}
$rebuild = $rebuildCommands[0]
if ($rebuild.Index -le $sessionStart.Index -or
    $rebuild.Pid -cne $sessionStart.Pid -or
    [string]::IsNullOrEmpty($rebuild.Tid)) {
    Throw-ConversionBlocker (
        'Rebuild PID/TID is not inside the single appended GUI session.')
}

$loadCommands = @($lines | Where-Object {
        $_.Index -gt $sessionStart.Index -and
        $_.Index -lt $rebuild.Index -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'CmdProc' -and
        $_.Message -match '^Executing command ''Load Project "(?<Path>[^"]+)"''$'
    })
if ($loadCommands.Count -ne 1) {
    Throw-ConversionBlocker (
        "canonical project Load command count is $($loadCommands.Count), expected 1.")
}
$loadMatch = [regex]::Match(
    $loadCommands[0].Message,
    '^Executing command ''Load Project "(?<Path>[^"]+)"''$')
if (-not (Test-PathIdentity `
        -Left $loadMatch.Groups['Path'].Value `
        -Right $baseline.CanonicalProjectPath)) {
    Throw-ConversionBlocker 'the loaded project differs from the baseline project.'
}

$nextCommands = @($lines | Where-Object {
        $_.Index -gt $rebuild.Index -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'CmdProc' -and
        $_.Message -match '^Executing command '
    } | Sort-Object -Property Index)
$commandEndIndex = $lines.Count
if ($nextCommands.Count -gt 0) {
    $commandEndIndex = $nextCommands[0].Index
}

$headers = @($lines | Where-Object {
        $_.Index -gt $rebuild.Index -and
        $_.Index -lt $commandEndIndex -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Tid -ceq $rebuild.Tid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'Compiler' -and
        $_.Message -ceq $ExpectedHeader
    })
if ($headers.Count -ne 1) {
    Throw-ConversionBlocker (
        "same-command C78/ARM header count is $($headers.Count), expected 1.")
}
$header = $headers[0]

$clearLines = @($lines | Where-Object {
        $_.Index -gt $rebuild.Index -and
        $_.Index -lt $header.Index -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Tid -ceq $rebuild.Tid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'Compiler' -and
        $_.Message -ceq '{Clear}'
    })
if ($clearLines.Count -ne 1) {
    Throw-ConversionBlocker (
        "same-command pre-header Clear count is $($clearLines.Count), expected 1.")
}

$resultLines = @($lines | Where-Object {
        $_.Index -gt $header.Index -and
        $_.Index -lt $commandEndIndex -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Tid -ceq $rebuild.Tid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'Compiler' -and
        $_.Message -ceq '{ResultCount}'
    })
if ($resultLines.Count -ne 1) {
    Throw-ConversionBlocker (
        "same-command ResultCount count is $($resultLines.Count), expected 1.")
}
$resultLine = $resultLines[0]

$terminalLines = @($lines | Where-Object {
        $_.Index -gt $resultLine.Index -and
        $_.Index -lt $commandEndIndex -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Tid -ceq $rebuild.Tid -and
        $_.Level -ceq 'INFO' -and
        $_.Owner -ceq 'CmdProc' -and
        $_.Message -match '^Last command succeeded\.'
    })
if ($terminalLines.Count -ne 1) {
    Throw-ConversionBlocker (
        "same-command success terminal count is $($terminalLines.Count), expected 1.")
}
$sameCommandFailures = @($lines | Where-Object {
        $_.Index -gt $rebuild.Index -and
        $_.Index -lt $commandEndIndex -and
        $_.Pid -ceq $rebuild.Pid -and
        $_.Tid -ceq $rebuild.Tid -and
        ($_.Level -ceq 'ERROR' -or
            $_.Message -match '^Last command failed\.' -or
            $_.Message -match "Command 'Rebuild project' failed")
    })
if ($sameCommandFailures.Count -ne 0) {
    Throw-ConversionBlocker 'same-command log reports Rebuild failure.'
}

$convertedLines = @()
$warningCount = 0
$errorCount = 0
$strippedSuffixCount = 0
foreach ($line in $lines) {
    if ($line.Index -lt $header.Index -or
        $line.Index -ge $resultLine.Index -or
        $line.Pid -cne $rebuild.Pid -or
        $line.Tid -cne $rebuild.Tid -or
        $line.Level -notin @('INFO', 'WARN', 'ERROR') -or
        $line.Owner -notin @('Compiler', 'OutputCommand')) {
        continue
    }

    $message = $line.Message
    $messageWithoutSuffix = [regex]::Replace(
        $message,
        '\|\*[^\r\n]*$',
        '')
    if ($messageWithoutSuffix -cne $message) {
        $strippedSuffixCount++
    }
    $convertedLines += (
        "$($line.Owner): [$($line.Level)] $messageWithoutSuffix")
    if ($line.Level -ceq 'WARN') {
        $warningCount++
    }
    elseif ($line.Level -ceq 'ERROR') {
        $errorCount++
    }
}

if ($convertedLines.Count -eq 0 -or
    $convertedLines[0] -cne "Compiler: [INFO] $ExpectedHeader") {
    Throw-ConversionBlocker 'converted transcript does not start with the C78/ARM header.'
}
$saveLine = (
    "OutputCommand: [INFO] Save project '$($baseline.CanonicalProjectPath)'.")
$saveLineCount = @($convertedLines | Where-Object { $_ -ceq $saveLine }).Count
if ($saveLineCount -ne 1) {
    Throw-ConversionBlocker (
        "converted canonical Save project count is $saveLineCount, expected 1.")
}

$summaryLine = (
    "Done - $errorCount error(s), $warningCount warning(s).")
$transcriptLines = @($convertedLines) + $summaryLine
$transcriptText = ($transcriptLines -join "`r`n") + "`r`n"
$transcriptBytes = $Utf8NoBom.GetBytes($transcriptText)
$transcriptSha256 = Get-Sha256Hex `
    -Bytes $transcriptBytes `
    -Offset 0 `
    -Count $transcriptBytes.Length

if ($RawDeltaOnly.IsPresent) {
    $existingTranscriptSha256 = Get-Sha256Hex `
        -Bytes $existingTranscriptBytes `
        -Offset 0 `
        -Count $existingTranscriptBytes.Length
    if ($existingTranscriptBytes.Length -ne $transcriptBytes.Length -or
        $existingTranscriptSha256 -cne $transcriptSha256) {
        Throw-ConversionBlocker (
            'existing transcript differs from the deterministic bounded-log projection.')
    }
}

if (-not $RawDeltaOnly.IsPresent) {
    $outputStream = $null
    try {
        $outputStream = [System.IO.File]::Open(
            $resolvedOutputPath,
            [System.IO.FileMode]::CreateNew,
            [System.IO.FileAccess]::Write,
            [System.IO.FileShare]::None)
        $outputStream.Write($transcriptBytes, 0, $transcriptBytes.Length)
    }
    catch [System.IO.IOException] {
        Throw-ConversionBlocker (
            "refusing to overwrite or replace output: $resolvedOutputPath")
    }
    finally {
        if ($null -ne $outputStream) {
            $outputStream.Dispose()
        }
    }

    $writtenTranscriptBytes = [System.IO.File]::ReadAllBytes($resolvedOutputPath)
    $writtenTranscriptSha256 = Get-Sha256Hex `
        -Bytes $writtenTranscriptBytes `
        -Offset 0 `
        -Count $writtenTranscriptBytes.Length
    if ($writtenTranscriptBytes.Length -ne $transcriptBytes.Length -or
        $writtenTranscriptSha256 -cne $transcriptSha256) {
        Throw-ConversionBlocker 'create-new transcript failed its read-back check.'
    }
}

$rawDeltaSha256 = Get-Sha256Hex `
    -Bytes $logBytes `
    -Offset ([int]$prefixLength) `
    -Count $appendedByteCount
$regeneratedOutputs = @()
if ($hasRawDeltaOutput) {
    $rawOutputStream = $null
    try {
        $rawOutputStream = [System.IO.File]::Open(
            $resolvedRawDeltaOutputPath,
            [System.IO.FileMode]::CreateNew,
            [System.IO.FileAccess]::Write,
            [System.IO.FileShare]::None)
        $rawOutputStream.Write(
            $logBytes,
            [int]$prefixLength,
            $appendedByteCount)
    }
    catch [System.IO.IOException] {
        Throw-ConversionBlocker (
            "refusing to overwrite or replace raw delta: $resolvedRawDeltaOutputPath")
    }
    finally {
        if ($null -ne $rawOutputStream) {
            $rawOutputStream.Dispose()
        }
    }

    $writtenRawBytes = [System.IO.File]::ReadAllBytes($resolvedRawDeltaOutputPath)
    $writtenRawSha256 = Get-Sha256Hex `
        -Bytes $writtenRawBytes `
        -Offset 0 `
        -Count $writtenRawBytes.Length
    if ($writtenRawBytes.Length -ne $appendedByteCount -or
        $writtenRawSha256 -cne $rawDeltaSha256) {
        Throw-ConversionBlocker 'create-new raw delta failed its read-back check.'
    }

    $baselineBytes = [System.IO.File]::ReadAllBytes($resolvedBaselinePath)
    $baselineSha256 = Get-Sha256Hex `
        -Bytes $baselineBytes `
        -Offset 0 `
        -Count $baselineBytes.Length
    if ($EvidenceProfile -ceq $GateDVisualLayoutEvidenceProfile) {
        $runningLasalBeforeOutputRead = @(
            Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
        if ($runningLasalBeforeOutputRead.Count -ne 0) {
            Throw-ConversionBlocker (
                'Lasal2 started before regenerated outputs were captured.')
        }
        $regeneratedOutputs = @(
            Get-RegeneratedOutputIdentities `
                -RepositoryRoot $resolvedRepositoryRoot)
    }
    $rawManifest = [ordered]@{
        Schema = $BoundedDeltaSchema
        EvidenceProfile = $EvidenceProfile
        Provenance = 'Exact byte slice from prefix-validated Lasal2.log'
        CapturedAtUtc = [DateTime]::UtcNow.ToString('o')
        BaselineFileName = [System.IO.Path]::GetFileName($resolvedBaselinePath)
        BaselineByteCount = $baselineBytes.Length
        BaselineSha256 = $baselineSha256
        BaselinePrefixLength = $prefixLength
        BaselinePrefixSha256 = $actualPrefixSha256
        SourceLogPath = $resolvedLogPath
        SourceStartOffset = $prefixLength
        SourceEndOffset = $effectiveLogEndOffset
        RawDeltaFileName = [System.IO.Path]::GetFileName(
            $resolvedRawDeltaOutputPath)
        RawDeltaByteCount = $appendedByteCount
        RawDeltaSha256 = $rawDeltaSha256
        Encoding = 'UTF-8'
        SessionPid = [int]$rebuild.Pid
        RebuildTid = [int]$rebuild.Tid
        TranscriptFileName = [System.IO.Path]::GetFileName($resolvedOutputPath)
        TranscriptByteCount = $transcriptBytes.Length
        TranscriptSha256 = $transcriptSha256
    }
    if ($EvidenceProfile -ceq $GateDVisualLayoutEvidenceProfile) {
        $rawManifest['RegeneratedOutputs'] = @($regeneratedOutputs)
    }
    $rawManifestText = $rawManifest | ConvertTo-Json -Depth 4
    $rawManifestBytes = $Utf8NoBom.GetBytes($rawManifestText)
    $manifestOutputStream = $null
    try {
        $manifestOutputStream = [System.IO.File]::Open(
            $resolvedRawDeltaManifestPath,
            [System.IO.FileMode]::CreateNew,
            [System.IO.FileAccess]::Write,
            [System.IO.FileShare]::None)
        $manifestOutputStream.Write(
            $rawManifestBytes,
            0,
            $rawManifestBytes.Length)
    }
    catch [System.IO.IOException] {
        Throw-ConversionBlocker (
            "refusing to overwrite or replace raw manifest: " +
            $resolvedRawDeltaManifestPath)
    }
    finally {
        if ($null -ne $manifestOutputStream) {
            $manifestOutputStream.Dispose()
        }
    }


    $rawManifestSha256 = Get-Sha256Hex `
        -Bytes $rawManifestBytes `
        -Offset 0 `
        -Count $rawManifestBytes.Length
    $writtenManifestBytes = [System.IO.File]::ReadAllBytes(
        $resolvedRawDeltaManifestPath)
    $writtenManifestSha256 = Get-Sha256Hex `
        -Bytes $writtenManifestBytes `
        -Offset 0 `
        -Count $writtenManifestBytes.Length
    if ($writtenManifestBytes.Length -ne $rawManifestBytes.Length -or
        $writtenManifestSha256 -cne $rawManifestSha256) {
        Throw-ConversionBlocker 'create-new raw manifest failed its read-back check.'
    }
    if ($EvidenceProfile -ceq $GateDVisualLayoutEvidenceProfile) {
        $runningLasalAfterManifest = @(
            Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
        if ($runningLasalAfterManifest.Count -ne 0) {
            Throw-ConversionBlocker (
                'Lasal2 started while regenerated output evidence was exported.')
        }
        $regeneratedOutputReadback = @(
            Get-RegeneratedOutputIdentities `
                -RepositoryRoot $resolvedRepositoryRoot)
        Assert-RegeneratedOutputIdentitiesEqual `
            -Expected $regeneratedOutputs `
            -Actual $regeneratedOutputReadback
    }
}

[pscustomobject]@{
    Provenance = 'Derived from validated appended Lasal2.log rebuild output'
    EvidenceProfile = $EvidenceProfile
    BaselinePath = $resolvedBaselinePath
    LasalLogPath = $resolvedLogPath
    OutputPath = $resolvedOutputPath
    BaselinePrefixLength = $prefixLength
    BaselinePrefixSha256 = $actualPrefixSha256
    LogEndOffset = $effectiveLogEndOffset
    AppendedByteCount = $appendedByteCount
    AppendedSha256 = $rawDeltaSha256
    SessionPid = $rebuild.Pid
    RebuildTid = $rebuild.Tid
    ConvertedLineCount = $convertedLines.Count
    ErrorCount = $errorCount
    WarningCount = $warningCount
    StrippedDiagnosticSuffixCount = $strippedSuffixCount
    OutputByteCount = $transcriptBytes.Length
    OutputSha256 = $transcriptSha256
    TranscriptCreated = -not $RawDeltaOnly.IsPresent
    RawDeltaOutputPath = $resolvedRawDeltaOutputPath
    RawDeltaManifestPath = $resolvedRawDeltaManifestPath
    RegeneratedOutputsBound = $regeneratedOutputs.Count
}

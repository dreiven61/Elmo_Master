[CmdletBinding(DefaultParameterSetName = 'Capture')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Capture')]
    [switch]$CaptureBuildBaseline,
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [switch]$VerifyBuild,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest,
    [Parameter(ParameterSetName = 'Capture')]
    [Parameter(ParameterSetName = 'Verify')]
    [string]$RepositoryRoot = (Join-Path $PSScriptRoot '..\..\..\..'),
    [Parameter(Mandatory = $true, ParameterSetName = 'Capture')]
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$EvidencePath,
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$BuildTranscriptPath,
    [Parameter(ParameterSetName = 'Capture')]
    [Parameter(ParameterSetName = 'Verify')]
    [string]$LasalLogPath = (Join-Path $env:TEMP 'Lasal2.log'),
    [Parameter(ParameterSetName = 'Verify')]
    [switch]$RunFullStatic
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Owner = 'LASAL.C78RebuildEvidence'
$EvidenceSchema = 'LasalC78RebuildEvidence/v1'
$CanonicalProjectRelativePath = (
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/' +
    'Elmo_EtherCAT_Test_4Axis.lcp')
$EvidenceRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCEcatInputLatch/LMCEcatInputLatch.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCRecorderStore/LMCRecorderStore.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCSdoExecutor/LMCSdoExecutor.st',
    $CanonicalProjectRelativePath,
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Comm_Network/Comm_Network.lcn'
)
$RequiredCompileRelativePaths = @(
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/TCPMotionInterface/TCPMotionInterface.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCControlCommandService/LMCControlCommandService.st',
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/LMCDiagnosticsService/LMCDiagnosticsService.st'
)
$Utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Throw-RebuildEvidenceBlocker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    throw "$Owner blocker: $Message"
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

function Get-CanonicalRepositoryContext {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        Throw-RebuildEvidenceBlocker "repository root does not exist: $Root"
    }
    $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\')
    $projectPath = Get-NormalizedFullPath -Path (
        Join-Path $resolvedRoot $CanonicalProjectRelativePath)
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker (
            "canonical project does not exist: $projectPath")
    }

    return [pscustomobject]@{
        RepositoryRoot = $resolvedRoot
        CanonicalProjectPath = $projectPath
        RequiredCompilePaths = @(
            $RequiredCompileRelativePaths | ForEach-Object {
                Get-NormalizedFullPath -Path (Join-Path $resolvedRoot $_)
            }
        )
    }
}

function Get-Sha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString(
                $sha256.ComputeHash($Bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-FileSha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "hash input does not exist: $Path"
    }
    return Get-Sha256Hex -Bytes ([System.IO.File]::ReadAllBytes($Path))
}

function Get-LogBytes {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return ,([byte[]]::new(0))
    }
    return ,([System.IO.File]::ReadAllBytes($Path))
}

function Get-PrefixSha256Hex {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [long]$Length
    )

    if ($Length -lt 0 -or $Length -gt [int]::MaxValue) {
        Throw-RebuildEvidenceBlocker "unsupported log prefix length: $Length"
    }
    if ($Bytes.LongLength -lt $Length) {
        Throw-RebuildEvidenceBlocker (
            "Lasal2.log was truncated: current length $($Bytes.LongLength), " +
            "baseline prefix length $Length")
    }

    $prefix = [byte[]]::new([int]$Length)
    if ($Length -gt 0) {
        [System.Array]::Copy($Bytes, 0, $prefix, 0, [int]$Length)
    }
    return Get-Sha256Hex -Bytes $prefix
}

function Get-AppendedLogText {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [byte[]]$Bytes,
        [Parameter(Mandatory = $true)]
        [long]$Offset
    )

    if ($Offset -lt 0 -or $Offset -gt [int]::MaxValue -or
        $Bytes.LongLength -gt [int]::MaxValue) {
        Throw-RebuildEvidenceBlocker 'Lasal2.log is too large for evidence parsing.'
    }
    if ($Bytes.LongLength -lt $Offset) {
        Throw-RebuildEvidenceBlocker 'Lasal2.log is shorter than the baseline offset.'
    }

    $length = [int]($Bytes.LongLength - $Offset)
    $appendedBytes = [byte[]]::new($length)
    if ($length -gt 0) {
        [System.Array]::Copy(
            $Bytes,
            [int]$Offset,
            $appendedBytes,
            0,
            $length)
    }
    return [System.Text.Encoding]::UTF8.GetString($appendedBytes)
}

function Get-IndexedLines {
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
        $prefixMatch = [regex]::Match(
            $rawLine,
            '^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*\]\s*(?<Body>.*)$')
        if ($prefixMatch.Success) {
            $pidValue = $prefixMatch.Groups['Pid'].Value
            $tidValue = $prefixMatch.Groups['Tid'].Value
        }
        $lines += [pscustomobject]@{
            Index = $index
            Text = $rawLine
            Pid = $pidValue
            Tid = $tidValue
        }
    }
    return $lines
}

function Assert-TranscriptEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TranscriptText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(Get-IndexedLines -Text $TranscriptText)
    $headerLines = @($lines | Where-Object {
            $_.Text -ceq (
                'Compiler: [INFO] Rebuild project with compiler version C78 ' +
                '(target architecture: ARM)')
        })
    if ($headerLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "transcript C78/ARM rebuild header count is $($headerLines.Count), expected 1.")
    }
    $headerLine = $headerLines[0]

    $summaries = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Text -match '^Done - (?<Errors>\d+) error\(s\), ' +
                '(?<Warnings>\d+) warning\(s\)\.$'
        })
    if ($summaries.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "transcript terminal result count is $($summaries.Count), expected 1.")
    }
    $summaryLine = $summaries[0]

    $saveLine = "OutputCommand: [INFO] Save project '$CanonicalProjectPath'."
    $saveLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Index -lt $summaryLine.Index -and
            $_.Text -ceq $saveLine
        })
    if ($saveLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "canonical Save project line count inside the rebuild window is " +
            "$($saveLines.Count), expected 1.")
    }

    foreach ($compilePath in $RequiredCompilePaths) {
        $compileLine = 'Compiler: [INFO] Compiling "' + $compilePath + '"'
        $compileLines = @($lines | Where-Object {
                $_.Index -gt $saveLines[0].Index -and
                $_.Index -lt $summaryLine.Index -and
                $_.Text -ceq $compileLine
            })
        if ($compileLines.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "transcript compile line count for $compilePath inside the rebuild " +
                "window is $($compileLines.Count), expected 1.")
        }
    }

    $summaryMatch = [regex]::Match(
        $summaryLine.Text,
        '^Done - (?<Errors>\d+) error\(s\), (?<Warnings>\d+) warning\(s\)\.$')
    $errorCount = [int]$summaryMatch.Groups['Errors'].Value
    $warningCount = [int]$summaryMatch.Groups['Warnings'].Value
    if ($errorCount -ne 0 -or $warningCount -ne 55) {
        Throw-RebuildEvidenceBlocker (
            "transcript result is $errorCount error(s), $warningCount warning(s); " +
            'expected 0 error(s), 55 warning(s).')
    }

    $preResultLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Index -lt $summaryLine.Index
        })
    $compilerErrors = @($preResultLines | Where-Object {
            $_.Text -match '^Compiler: \[ERROR\]'
        })
    if ($compilerErrors.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "pre-result Compiler error diagnostic count is $($compilerErrors.Count), expected 0.")
    }

    $compilerWarnings = @($preResultLines | Where-Object {
            $_.Text -match '^Compiler: \[WARN\] W \d{4}\b'
        })
    if ($compilerWarnings.Count -ne 55) {
        Throw-RebuildEvidenceBlocker (
            "pre-result Compiler warning diagnostic count is $($compilerWarnings.Count), " +
            'expected 55.')
    }

    $warningHistogram = @{}
    foreach ($warningLine in $compilerWarnings) {
        $warningMatch = [regex]::Match(
            $warningLine.Text,
            '^Compiler: \[WARN\] W (?<Code>\d{4})\b')
        $code = $warningMatch.Groups['Code'].Value
        if (-not $warningHistogram.ContainsKey($code)) {
            $warningHistogram[$code] = 0
        }
        $warningHistogram[$code]++
    }
    $expectedHistogram = @{
        '0069' = 35
        '0072' = 17
        '0073' = 3
    }
    if ($warningHistogram.Count -ne $expectedHistogram.Count) {
        Throw-RebuildEvidenceBlocker (
            'pre-result warning codes differ from W0069/W0072/W0073.')
    }
    foreach ($code in $expectedHistogram.Keys) {
        if (-not $warningHistogram.ContainsKey($code) -or
            $warningHistogram[$code] -ne $expectedHistogram[$code]) {
            $actualCount = 0
            if ($warningHistogram.ContainsKey($code)) {
                $actualCount = $warningHistogram[$code]
            }
            Throw-RebuildEvidenceBlocker (
                "pre-result W$code count is $actualCount, expected " +
                "$($expectedHistogram[$code]).")
        }
    }

    $unexpectedErrorLines = @($lines | Where-Object {
            $_.Index -gt $headerLine.Index -and
            $_.Text -match '^(?:Compiler|OutputCommand): \[ERROR\]'
        })
    if ($unexpectedErrorLines.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "transcript contains $($unexpectedErrorLines.Count) error output line(s).")
    }
    if ($TranscriptText -match "(?im)Command 'Rebuild project' failed") {
        Throw-RebuildEvidenceBlocker 'transcript reports Rebuild project failure.'
    }
}

function Assert-AppendedLasalLogEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    if ($AppendedLogText.Length -eq 0) {
        Throw-RebuildEvidenceBlocker 'no Lasal2.log bytes were appended after capture.'
    }
    if ($AppendedLogText -match '(?i)CInvalidArgException') {
        Throw-RebuildEvidenceBlocker (
            'appended Lasal2.log contains CInvalidArgException.')
    }
    $prohibitedPattern = (
        "(?i)Executing command '(?:Download Project|Connect(?: to )?|" +
        "go online|Link(?: project)?)")
    if ($AppendedLogText -match $prohibitedPattern) {
        Throw-RebuildEvidenceBlocker (
            "appended Lasal2.log contains prohibited explicit command: $($Matches[0])")
    }

    $lines = @(Get-IndexedLines -Text $AppendedLogText)
    $startLines = @($lines | Where-Object {
            $_.Text -match (
                '\(INFO\) GUI\]\s+Start Application at ' +
                '\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\s*$')
        })
    if ($startLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "appended GUI Start Application count is $($startLines.Count), expected 1.")
    }
    $sessionStart = $startLines[0]
    if ([string]::IsNullOrEmpty($sessionStart.Pid)) {
        Throw-RebuildEvidenceBlocker 'GUI Start Application has no PID identity.'
    }

    $rebuildLines = @($lines | Where-Object {
            $_.Text -match "\(INFO\) CmdProc\] Executing command 'Rebuild project'"
        })
    if ($rebuildLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "appended Rebuild project command count is $($rebuildLines.Count), expected 1.")
    }
    $rebuild = $rebuildLines[0]
    if ([string]::IsNullOrEmpty($rebuild.Pid) -or
        [string]::IsNullOrEmpty($rebuild.Tid)) {
        Throw-RebuildEvidenceBlocker 'Rebuild project command has no PID/TID identity.'
    }
    if ($rebuild.Index -le $sessionStart.Index -or
        $rebuild.Pid -cne $sessionStart.Pid) {
        Throw-RebuildEvidenceBlocker (
            'Rebuild project is not inside the single appended GUI session.')
    }

    $sessionLoadLines = @($lines | Where-Object {
            $_.Index -gt $sessionStart.Index -and
            $_.Index -lt $rebuild.Index -and
            $_.Text -match (
                "\(INFO\) CmdProc\] Executing command 'Load Project `"[^`"]+`"'")
        })
    if ($sessionLoadLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "project Load command count between GUI start and Rebuild is " +
            "$($sessionLoadLines.Count), expected 1.")
    }
    $loadNeedle = (
        "Executing command 'Load Project `"$CanonicalProjectPath`"'")
    if ($sessionLoadLines[0].Pid -cne $rebuild.Pid -or
        -not $sessionLoadLines[0].Text.Contains($loadNeedle)) {
        Throw-RebuildEvidenceBlocker (
            'the single session Load Project is not the canonical project.')
    }

    $headerNeedle = (
        'Compiler] Rebuild project with compiler version C78 ' +
        '(target architecture: ARM)')
    $headerLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text.Contains($headerNeedle)
        })
    if ($headerLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command C78/ARM compiler header count is $($headerLines.Count), " +
            'expected 1.')
    }
    $header = $headerLines[0]

    $clearLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Index -lt $header.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) Compiler\] \{Clear\}\s*$'
        })
    if ($clearLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command {Clear} count before the compiler header is " +
            "$($clearLines.Count), expected 1.")
    }

    $resultLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) Compiler\] \{ResultCount\}\s*$'
        })
    if ($resultLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command {ResultCount} count is $($resultLines.Count), expected 1.")
    }
    $resultLine = $resultLines[0]

    $rawPreResultLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Index -lt $resultLine.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid
        })
    $rawCompilerErrors = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(ERROR\) Compiler\]'
        })
    if ($rawCompilerErrors.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Compiler error count is " +
            "$($rawCompilerErrors.Count), expected 0.")
    }
    $rawCompilerWarnings = @($rawPreResultLines | Where-Object {
            $_.Text -match '\(WARN\) Compiler\]'
        })
    if ($rawCompilerWarnings.Count -ne 55) {
        Throw-RebuildEvidenceBlocker (
            "raw pre-{ResultCount} Compiler warning count is " +
            "$($rawCompilerWarnings.Count), expected 55.")
    }
    $rawWarningHistogram = @{}
    foreach ($rawWarning in $rawCompilerWarnings) {
        $rawWarningMatch = [regex]::Match(
            $rawWarning.Text,
            '\(WARN\) Compiler\] W (?<Code>\d{4})\b')
        if (-not $rawWarningMatch.Success) {
            Throw-RebuildEvidenceBlocker (
                'raw pre-{ResultCount} warning has no W #### diagnostic code.')
        }
        $rawCode = $rawWarningMatch.Groups['Code'].Value
        if (-not $rawWarningHistogram.ContainsKey($rawCode)) {
            $rawWarningHistogram[$rawCode] = 0
        }
        $rawWarningHistogram[$rawCode]++
    }
    $expectedRawHistogram = @{
        '0069' = 35
        '0072' = 17
        '0073' = 3
    }
    if ($rawWarningHistogram.Count -ne $expectedRawHistogram.Count) {
        Throw-RebuildEvidenceBlocker (
            'raw pre-{ResultCount} warning codes differ from W0069/W0072/W0073.')
    }
    foreach ($rawCode in $expectedRawHistogram.Keys) {
        if (-not $rawWarningHistogram.ContainsKey($rawCode) -or
            $rawWarningHistogram[$rawCode] -ne $expectedRawHistogram[$rawCode]) {
            $rawActualCount = 0
            if ($rawWarningHistogram.ContainsKey($rawCode)) {
                $rawActualCount = $rawWarningHistogram[$rawCode]
            }
            Throw-RebuildEvidenceBlocker (
                "raw pre-{ResultCount} W$rawCode count is $rawActualCount, expected " +
                "$($expectedRawHistogram[$rawCode]).")
        }
    }

    $rawSaveLines = @($lines | Where-Object {
            $_.Index -gt $header.Index -and
            $_.Index -lt $resultLine.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) OutputCommand\] Save project '
        })
    $rawSaveNeedle = (
        "(INFO) OutputCommand] Save project '$CanonicalProjectPath'.")
    if ($rawSaveLines.Count -ne 1 -or
        -not $rawSaveLines[0].Text.EndsWith($rawSaveNeedle)) {
        Throw-RebuildEvidenceBlocker (
            'same-command raw canonical Save project line is missing or duplicated.')
    }
    $rawSaveLine = $rawSaveLines[0]

    foreach ($compilePath in $RequiredCompilePaths) {
        $compileNeedle = 'Compiler] Compiling "' + $compilePath + '"'
        $compileLines = @($lines | Where-Object {
                $_.Index -gt $rawSaveLine.Index -and
                $_.Index -lt $resultLine.Index -and
                $_.Pid -ceq $rebuild.Pid -and
                $_.Tid -ceq $rebuild.Tid -and
                $_.Text.Contains($compileNeedle)
            })
        if ($compileLines.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "same-command compile line count for $compilePath inside the result " +
                "window is $($compileLines.Count), expected 1.")
        }
    }

    $nextCommandLines = @($lines | Where-Object {
            $_.Index -gt $rebuild.Index -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Text -match '\(INFO\) CmdProc\] Executing command '
        } | Sort-Object -Property Index)
    $commandEndIndex = $lines.Count
    if ($nextCommandLines.Count -gt 0) {
        $commandEndIndex = $nextCommandLines[0].Index
    }

    $terminalLines = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and
            $_.Index -lt $commandEndIndex -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            $_.Text -match '\(INFO\) CmdProc\] Last command succeeded\.'
        })
    if ($terminalLines.Count -ne 1) {
        Throw-RebuildEvidenceBlocker (
            "same-command CmdProc success terminal count is $($terminalLines.Count), " +
            'expected 1.')
    }
    $terminal = $terminalLines[0]

    $sameCommandFailures = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and
            $_.Index -lt $commandEndIndex -and
            $_.Pid -ceq $rebuild.Pid -and
            $_.Tid -ceq $rebuild.Tid -and
            ($_.Text -match 'Last command failed\.' -or
                $_.Text -match "Command 'Rebuild project' failed" -or
                $_.Text -match '\(ERROR\)')
        })
    if ($sameCommandFailures.Count -ne 0) {
        Throw-RebuildEvidenceBlocker 'same-command log reports Rebuild failure.'
    }

    $postResultLines = @($lines | Where-Object {
            $_.Index -gt $resultLine.Index -and $_.Index -lt $terminal.Index
        })
    $postCompilerDiagnostics = @($postResultLines | Where-Object {
            $_.Text -match '\((?:WARN|ERROR)\)'
        })
    if ($postCompilerDiagnostics.Count -ne 6) {
        Throw-RebuildEvidenceBlocker (
            "post-{ResultCount} Compiler diagnostic count is " +
            "$($postCompilerDiagnostics.Count), expected exactly 6 compatibility warnings.")
    }
    foreach ($diagnostic in $postCompilerDiagnostics) {
        if ($diagnostic.Pid -cne $rebuild.Pid -or
            $diagnostic.Tid -cne $rebuild.Tid -or
            $diagnostic.Text -notmatch '\(WARN\) Compiler\]') {
            Throw-RebuildEvidenceBlocker (
                'post-{ResultCount} diagnostics are interleaved or contain an error.')
        }
    }

    $compatibilityPatterns = @(
        ('The current project "Elmo_EtherCAT_Test_4Axis" is using an old ' +
            'compiler version \(C78\)\. Latest version: C81\..*\|~1' +
            [regex]::Escape($CanonicalProjectPath) + ';'),
        'The compiler version of library "Hardware"\(C81\) differs from the compiler version of the current project \(C78\)',
        'The compiler version of library "MotionLib"\(C81\) differs from the compiler version of the current project \(C78\)',
        'The compiler version of library "OS Interface"\(C81\) differs from the compiler version of the current project \(C78\)',
        'The compiler version of library "System"\(C81\) differs from the compiler version of the current project \(C78\)',
        'The compiler version of library "Tools"\(C81\) differs from the compiler version of the current project \(C78\)'
    )
    foreach ($compatibilityPattern in $compatibilityPatterns) {
        $matchingWarnings = @($postCompilerDiagnostics | Where-Object {
                $_.Text -match $compatibilityPattern
            })
        if ($matchingWarnings.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                'required post-{ResultCount} compatibility warning is missing or duplicated: ' +
                $compatibilityPattern)
        }
    }
}

function Assert-LasalC78RebuildEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TranscriptText,
        [Parameter(Mandatory = $true)]
        [string]$AppendedLogText,
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    Assert-TranscriptEvidence `
        -TranscriptText $TranscriptText `
        -CanonicalProjectPath $CanonicalProjectPath `
        -RequiredCompilePaths $RequiredCompilePaths
    Assert-AppendedLasalLogEvidence `
        -AppendedLogText $AppendedLogText `
        -CanonicalProjectPath $CanonicalProjectPath `
        -RequiredCompilePaths $RequiredCompilePaths
}

function Capture-BuildBaseline {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [string]$OutputPath,
        [Parameter(Mandatory = $true)]
        [string]$LogPath
    )

    $runningLasal = @(Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
    if ($runningLasal.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            "capture requires Lasal2 to be stopped; found $($runningLasal.Count) process(es).")
    }

    $context = Get-CanonicalRepositoryContext -Root $Root
    $resolvedEvidencePath = Get-NormalizedFullPath -Path $OutputPath
    if (Test-Path -LiteralPath $resolvedEvidencePath) {
        Throw-RebuildEvidenceBlocker (
            "evidence file already exists; refusing to overwrite: $resolvedEvidencePath")
    }
    $evidenceDirectory = Split-Path -Parent $resolvedEvidencePath
    if (-not (Test-Path -LiteralPath $evidenceDirectory -PathType Container)) {
        Throw-RebuildEvidenceBlocker (
            "evidence directory does not exist: $evidenceDirectory")
    }

    $resolvedLogPath = Get-NormalizedFullPath -Path $LogPath
    $logBytes = Get-LogBytes -Path $resolvedLogPath
    $files = @()
    foreach ($relativePath in $EvidenceRelativePaths) {
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $context.RepositoryRoot $relativePath)
        $files += [ordered]@{
            RelativePath = $relativePath
            Sha256 = Get-FileSha256Hex -Path $fullPath
        }
    }
    $runningLasalAfterHash = @(
        Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue)
    if ($runningLasalAfterHash.Count -ne 0) {
        Throw-RebuildEvidenceBlocker (
            'Lasal2 started while the baseline was being captured; no evidence was written.')
    }

    $evidence = [ordered]@{
        Schema = $EvidenceSchema
        CapturedAtUtc = [DateTime]::UtcNow.ToString('o')
        RepositoryRoot = $context.RepositoryRoot
        CanonicalProjectPath = $context.CanonicalProjectPath
        LasalLogPath = $resolvedLogPath
        LogPrefixLength = $logBytes.LongLength
        LogPrefixSha256 = Get-Sha256Hex -Bytes $logBytes
        Files = $files
    }
    $json = $evidence | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText($resolvedEvidencePath, $json, $Utf8NoBom)
    Write-Output (
        "PASS $Owner.Capture prefixBytes=$($logBytes.LongLength) " +
        "files=$($files.Count) evidence=$resolvedEvidencePath")
}

function Get-RequiredEvidenceProperty {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Evidence,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if ($Evidence.PSObject.Properties.Name -notcontains $Name) {
        Throw-RebuildEvidenceBlocker "baseline property is missing: $Name"
    }
    return $Evidence.$Name
}

function Verify-BuildEvidence {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,
        [Parameter(Mandatory = $true)]
        [string]$BaselinePath,
        [Parameter(Mandatory = $true)]
        [string]$TranscriptPath,
        [Parameter(Mandatory = $true)]
        [string]$LogPath,
        [Parameter(Mandatory = $true)]
        [bool]$InvokeFullStatic
    )

    $context = Get-CanonicalRepositoryContext -Root $Root
    $resolvedBaselinePath = Get-NormalizedFullPath -Path $BaselinePath
    $resolvedTranscriptPath = Get-NormalizedFullPath -Path $TranscriptPath
    if (-not (Test-Path -LiteralPath $resolvedBaselinePath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker "baseline file does not exist: $resolvedBaselinePath"
    }
    if (-not (Test-Path -LiteralPath $resolvedTranscriptPath -PathType Leaf)) {
        Throw-RebuildEvidenceBlocker (
            "build transcript does not exist: $resolvedTranscriptPath")
    }

    try {
        $baseline = Get-Content -LiteralPath $resolvedBaselinePath -Raw |
            ConvertFrom-Json
    }
    catch {
        Throw-RebuildEvidenceBlocker (
            "baseline JSON cannot be parsed: $($_.Exception.Message)")
    }
    $schema = Get-RequiredEvidenceProperty -Evidence $baseline -Name 'Schema'
    if ($schema -cne $EvidenceSchema) {
        Throw-RebuildEvidenceBlocker (
            "baseline schema is '$schema', expected '$EvidenceSchema'.")
    }
    $baselineRoot = Get-RequiredEvidenceProperty `
        -Evidence $baseline -Name 'RepositoryRoot'
    if (-not (Test-PathIdentity -Left $baselineRoot -Right $context.RepositoryRoot)) {
        Throw-RebuildEvidenceBlocker 'baseline repository root differs from verification root.'
    }
    $baselineProject = Get-RequiredEvidenceProperty `
        -Evidence $baseline -Name 'CanonicalProjectPath'
    if (-not (Test-PathIdentity `
            -Left $baselineProject -Right $context.CanonicalProjectPath)) {
        Throw-RebuildEvidenceBlocker 'baseline canonical project path differs.'
    }

    $baselineLogPath = Get-RequiredEvidenceProperty `
        -Evidence $baseline -Name 'LasalLogPath'
    $resolvedLogPath = Get-NormalizedFullPath -Path $LogPath
    if (-not (Test-PathIdentity -Left $baselineLogPath -Right $resolvedLogPath)) {
        Throw-RebuildEvidenceBlocker 'verification Lasal2.log path differs from baseline.'
    }
    $prefixLength = [long](Get-RequiredEvidenceProperty `
            -Evidence $baseline -Name 'LogPrefixLength')
    $prefixSha256 = [string](Get-RequiredEvidenceProperty `
            -Evidence $baseline -Name 'LogPrefixSha256')
    $logBytes = Get-LogBytes -Path $resolvedLogPath
    $currentPrefixSha256 = Get-PrefixSha256Hex `
        -Bytes $logBytes -Length $prefixLength
    if ($currentPrefixSha256 -cne $prefixSha256) {
        Throw-RebuildEvidenceBlocker (
            'Lasal2.log baseline prefix hash changed; log is not append-only.')
    }

    $baselineFiles = @(
        Get-RequiredEvidenceProperty -Evidence $baseline -Name 'Files')
    if ($baselineFiles.Count -ne $EvidenceRelativePaths.Count) {
        Throw-RebuildEvidenceBlocker (
            "baseline file count is $($baselineFiles.Count), expected " +
            "$($EvidenceRelativePaths.Count).")
    }
    foreach ($baselineFile in $baselineFiles) {
        if ($baselineFile.PSObject.Properties.Name -notcontains 'RelativePath' -or
            $baselineFile.PSObject.Properties.Name -notcontains 'Sha256') {
            Throw-RebuildEvidenceBlocker 'baseline file entry is malformed.'
        }
        if ($EvidenceRelativePaths -cnotcontains [string]$baselineFile.RelativePath) {
            Throw-RebuildEvidenceBlocker (
                "baseline contains an unexpected file: $($baselineFile.RelativePath)")
        }
    }
    foreach ($relativePath in $EvidenceRelativePaths) {
        $matchingEntries = @($baselineFiles | Where-Object {
                [string]$_.RelativePath -ceq $relativePath
            })
        if ($matchingEntries.Count -ne 1) {
            Throw-RebuildEvidenceBlocker (
                "baseline entry count for $relativePath is $($matchingEntries.Count), " +
                'expected 1.')
        }
        $fullPath = Get-NormalizedFullPath -Path (
            Join-Path $context.RepositoryRoot $relativePath)
        $currentHash = Get-FileSha256Hex -Path $fullPath
        if ($currentHash -cne [string]$matchingEntries[0].Sha256) {
            Throw-RebuildEvidenceBlocker (
                "captured input changed after baseline: $relativePath")
        }
    }

    $transcriptText = [System.IO.File]::ReadAllText($resolvedTranscriptPath)
    $appendedLogText = Get-AppendedLogText `
        -Bytes $logBytes -Offset $prefixLength
    Assert-LasalC78RebuildEvidence `
        -TranscriptText $transcriptText `
        -AppendedLogText $appendedLogText `
        -CanonicalProjectPath $context.CanonicalProjectPath `
        -RequiredCompilePaths $context.RequiredCompilePaths

    if ($InvokeFullStatic) {
        $contractScript = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
        if (-not (Test-Path -LiteralPath $contractScript -PathType Leaf)) {
            Throw-RebuildEvidenceBlocker (
                "full static verifier does not exist: $contractScript")
        }
        & $contractScript `
            -RepositoryRoot $context.RepositoryRoot `
            -ExpectedSdoWriteAxis 1
        if (-not $?) {
            Throw-RebuildEvidenceBlocker 'Verify-LasalContract.ps1 full mode failed.'
        }
    }

    Write-Output (
        "PASS $Owner.Verify C78/ARM errors=0 warnings=55 " +
        'postResultCompatibilityWarnings=6 inputsUnchanged=true')
}

function New-SyntheticTranscript {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(
        'Compiler: [INFO] Rebuild project with compiler version C78 (target architecture: ARM)',
        "OutputCommand: [INFO] Save project '$CanonicalProjectPath'."
    )
    foreach ($compilePath in $RequiredCompilePaths) {
        $lines += 'Compiler: [INFO] Compiling "' + $compilePath + '"'
    }
    for ($index = 0; $index -lt 35; $index++) {
        $lines += (
            'Compiler: [WARN] W 0069 "synthetic.st"(' +
            ($index + 1) + ') Condition is always TRUE')
    }
    for ($index = 0; $index -lt 17; $index++) {
        $lines += (
            'Compiler: [WARN] W 0072 "synthetic.st"(' +
            ($index + 101) + ") 'unused' declared but never used")
    }
    for ($index = 0; $index -lt 3; $index++) {
        $lines += (
            'Compiler: [WARN] W 0073 "synthetic.st"(' +
            ($index + 201) + ") Parameter 'input' is never used")
    }
    $lines += 'Done - 0 error(s), 55 warning(s).'
    return $lines -join "`r`n"
}

function New-SyntheticAppendedLog {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalProjectPath,
        [Parameter(Mandatory = $true)]
        [string[]]$RequiredCompilePaths
    )

    $lines = @(
        '[11:59:59 P:01234 T:00001 (INFO) GUI] Start Application at 2026-08-05 11:59:56',
        "[12:00:00 P:01234 T:00001 (INFO) CmdProc] Executing command 'Load Project `"$CanonicalProjectPath`"'",
        "[12:00:01 P:01234 T:05678 (INFO) CmdProc] Executing command 'Rebuild project'",
        '[12:00:01 P:01234 T:05678 (INFO) Compiler] {Clear}',
        '[12:00:01 P:01234 T:05678 (INFO) Compiler] Rebuild project with compiler version C78 (target architecture: ARM)',
        "[12:00:01 P:01234 T:05678 (INFO) OutputCommand] Save project '$CanonicalProjectPath'."
    )
    foreach ($compilePath in $RequiredCompilePaths) {
        $lines += (
            '[12:00:02 P:01234 T:05678 (INFO) Compiler] Compiling "' +
            $compilePath + '"')
    }
    for ($index = 0; $index -lt 35; $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
            '"synthetic.st"(' + ($index + 1) + ') Condition is always TRUE')
    }
    for ($index = 0; $index -lt 17; $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0072 ' +
            '"synthetic.st"(' + ($index + 101) +
            ") 'unused' declared but never used")
    }
    for ($index = 0; $index -lt 3; $index++) {
        $lines += (
            '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0073 ' +
            '"synthetic.st"(' + ($index + 201) +
            ") Parameter 'input' is never used")
    }
    $lines += @(
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Linking...',
        '[12:00:03 P:01234 T:05678 (INFO) Linker] Done',
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The current project ' +
            '"Elmo_EtherCAT_Test_4Axis" is using an old compiler version (C78). ' +
            'Latest version: C81. To benefit from the latest bugfixes, change the ' +
            'compiler version in the project-properties. |~1' +
            $CanonicalProjectPath + ';1225~'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "Hardware"(C81) differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "MotionLib"(C81) differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "OS Interface"(C81) differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "System"(C81) differs from the compiler version of the current project (C78)'),
        ('[12:00:04 P:01234 T:05678 (WARN) Compiler] The compiler version of ' +
            'library "Tools"(C81) differs from the compiler version of the current project (C78)'),
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] Last command succeeded. (1000.0ms)'
    )
    return $lines -join "`r`n"
}

function Invoke-RebuildEvidenceSelfTest {
    $syntheticRoot = 'C:\work\Elmo\Elmo_Master'
    $canonicalProjectPath = Get-NormalizedFullPath -Path (
        Join-Path $syntheticRoot $CanonicalProjectRelativePath)
    $requiredCompilePaths = @(
        $RequiredCompileRelativePaths | ForEach-Object {
            Get-NormalizedFullPath -Path (Join-Path $syntheticRoot $_)
        }
    )
    $goodTranscript = New-SyntheticTranscript `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths
    $goodLog = New-SyntheticAppendedLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths

    Assert-LasalC78RebuildEvidence `
        -TranscriptText $goodTranscript `
        -AppendedLogText $goodLog `
        -CanonicalProjectPath $canonicalProjectPath `
        -RequiredCompilePaths $requiredCompilePaths

    $currentLikeErrors = @(
        'Compiler: [ERROR] E 0166 "synthetic.st"(4368) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4370) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4399) Incompatible types.',
        'Compiler: [ERROR] E 0166 "synthetic.st"(4401) Incompatible types.'
    ) -join "`r`n"
    $badFourErrorTranscript = $goodTranscript.Replace(
        'Done - 0 error(s), 55 warning(s).',
        $currentLikeErrors + "`r`nDone - 4 error(s), 55 warning(s)." +
            "`r`nOutputCommand: [ERROR] Command 'Rebuild project' failed.")
    $badFourErrorLog = $goodLog.Replace(
        'Last command succeeded.',
        'Last command failed.')

    $postResultWarning = (
        '[12:00:04 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
        '"synthetic.st"(999) Condition is always TRUE')
    $badPostResultLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}' +
            "`r`n$postResultWarning")

    $noncanonicalRoot = 'C:\work\Elmo\Elmo_Master_test'
    $badNoncanonicalTranscript = $goodTranscript.Replace(
        $syntheticRoot,
        $noncanonicalRoot)
    $badNoncanonicalLog = $goodLog.Replace(
        $syntheticRoot,
        $noncanonicalRoot)
    $badInterleavedPidLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:04 P:09999 T:05678 (INFO) Compiler] {ResultCount}')
    $badMissingResultLog = $goodLog.Replace(
        "[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}`r`n",
        '')
    $badMissingTerminalLog = $goodLog.Replace(
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] Last command succeeded. (1000.0ms)',
        '')
    $badMissingClearLog = $goodLog.Replace(
        "[12:00:01 P:01234 T:05678 (INFO) Compiler] {Clear}`r`n",
        '')
    $badMissingRawSaveLog = $goodLog.Replace(
        "[12:00:01 P:01234 T:05678 (INFO) OutputCommand] Save project '$canonicalProjectPath'.`r`n",
        '')
    $loadLine = (
        "[12:00:00 P:01234 T:00001 (INFO) CmdProc] Executing command " +
        "'Load Project `"$canonicalProjectPath`"'")
    $badSessionBoundaryLog = $goodLog.Replace(
        $loadLine,
        $loadLine + "`r`n" +
            '[12:00:00 P:01234 T:00002 (INFO) GUI] Start Application at 2026-08-05 12:00:00')
    $terminalLine = (
        '[12:00:05 P:01234 T:05678 (INFO) CmdProc] ' +
        'Last command succeeded. (1000.0ms)')
    $badCommandBoundaryLog = $goodLog.Replace(
        $terminalLine,
        "[12:00:05 P:01234 T:00001 (INFO) CmdProc] Executing command 'Close Project'`r`n" +
            $terminalLine)
    $badSuccessThenFailureLog = $goodLog + "`r`n" +
        '[12:00:06 P:01234 T:05678 (ERROR) OutputCommand] ' +
        "Command 'Rebuild project' failed."
    $firstRawWarning = (
        '[12:00:03 P:01234 T:05678 (WARN) Compiler] W 0069 ' +
        '"synthetic.st"(1) Condition is always TRUE')
    $badMissingRawWarningLog = $goodLog.Replace(
        $firstRawWarning + "`r`n",
        '')
    $badRawErrorLog = $goodLog.Replace(
        '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}',
        '[12:00:03 P:01234 T:05678 (ERROR) Compiler] E 0166 ' +
            '"synthetic.st"(4368) Incompatible types.' + "`r`n" +
            '[12:00:04 P:01234 T:05678 (INFO) Compiler] {ResultCount}')
    $badRawHistogramLog = $goodLog.Replace(
        $firstRawWarning,
        $firstRawWarning.Replace('W 0069', 'W 0072'))
    $badDownloadLog = $goodLog + "`r`n" +
        "[12:00:06 P:01234 T:00001 (INFO) CmdProc] Executing command 'Download Project (5 sub commands)"

    $firstCompileLine = (
        'Compiler: [INFO] Compiling "' + $requiredCompilePaths[0] + '"')
    $badTranscriptOrdering = $goodTranscript.Replace(
        $firstCompileLine + "`r`n",
        '').Replace(
        'Done - 0 error(s), 55 warning(s).',
        "Done - 0 error(s), 55 warning(s).`r`n$firstCompileLine")

    $negativeFixtures = @(
        [pscustomobject]@{
            Name = 'current-like 4/55 failure'
            Transcript = $badFourErrorTranscript
            Log = $badFourErrorLog
        },
        [pscustomobject]@{
            Name = 'post-result warning exclusion'
            Transcript = $goodTranscript
            Log = $badPostResultLog
        },
        [pscustomobject]@{
            Name = 'noncanonical project'
            Transcript = $badNoncanonicalTranscript
            Log = $badNoncanonicalLog
        },
        [pscustomobject]@{
            Name = 'interleaved PID'
            Transcript = $goodTranscript
            Log = $badInterleavedPidLog
        },
        [pscustomobject]@{
            Name = 'missing ResultCount'
            Transcript = $goodTranscript
            Log = $badMissingResultLog
        },
        [pscustomobject]@{
            Name = 'missing terminal'
            Transcript = $goodTranscript
            Log = $badMissingTerminalLog
        },
        [pscustomobject]@{
            Name = 'missing raw Clear'
            Transcript = $goodTranscript
            Log = $badMissingClearLog
        },
        [pscustomobject]@{
            Name = 'missing raw canonical Save'
            Transcript = $goodTranscript
            Log = $badMissingRawSaveLog
        },
        [pscustomobject]@{
            Name = 'PID reuse session boundary'
            Transcript = $goodTranscript
            Log = $badSessionBoundaryLog
        },
        [pscustomobject]@{
            Name = 'terminal after next command boundary'
            Transcript = $goodTranscript
            Log = $badCommandBoundaryLog
        },
        [pscustomobject]@{
            Name = 'success followed by rebuild failure'
            Transcript = $goodTranscript
            Log = $badSuccessThenFailureLog
        },
        [pscustomobject]@{
            Name = 'missing raw compiler warning'
            Transcript = $goodTranscript
            Log = $badMissingRawWarningLog
        },
        [pscustomobject]@{
            Name = 'raw compiler error'
            Transcript = $goodTranscript
            Log = $badRawErrorLog
        },
        [pscustomobject]@{
            Name = 'raw warning histogram drift'
            Transcript = $goodTranscript
            Log = $badRawHistogramLog
        },
        [pscustomobject]@{
            Name = 'transcript compile after terminal result'
            Transcript = $badTranscriptOrdering
            Log = $goodLog
        },
        [pscustomobject]@{
            Name = 'prohibited download'
            Transcript = $goodTranscript
            Log = $badDownloadLog
        }
    )

    $rejected = 0
    foreach ($fixture in $negativeFixtures) {
        $wasRejected = $false
        try {
            Assert-LasalC78RebuildEvidence `
                -TranscriptText $fixture.Transcript `
                -AppendedLogText $fixture.Log `
                -CanonicalProjectPath $canonicalProjectPath `
                -RequiredCompilePaths $requiredCompilePaths
        }
        catch {
            if (-not $_.Exception.Message.StartsWith("$Owner blocker:")) {
                throw
            }
            $wasRejected = $true
        }
        if (-not $wasRejected) {
            throw "$Owner self-test failed to reject fixture: $($fixture.Name)"
        }
        $rejected++
    }

    Write-Output (
        "PASS $Owner.SelfTest successFixture=accepted " +
        "negativeFixturesRejected=$rejected/$($negativeFixtures.Count)")
}

switch ($PSCmdlet.ParameterSetName) {
    'Capture' {
        Capture-BuildBaseline `
            -Root $RepositoryRoot `
            -OutputPath $EvidencePath `
            -LogPath $LasalLogPath
    }
    'Verify' {
        Verify-BuildEvidence `
            -Root $RepositoryRoot `
            -BaselinePath $EvidencePath `
            -TranscriptPath $BuildTranscriptPath `
            -LogPath $LasalLogPath `
            -InvokeFullStatic $RunFullStatic.IsPresent
    }
    'SelfTest' {
        Invoke-RebuildEvidenceSelfTest
    }
    default {
        Throw-RebuildEvidenceBlocker 'an action switch is required.'
    }
}

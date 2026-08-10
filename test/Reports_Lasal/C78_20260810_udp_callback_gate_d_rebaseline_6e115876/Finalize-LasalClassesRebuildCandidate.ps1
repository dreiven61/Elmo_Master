[CmdletBinding(DefaultParameterSetName = 'Finalize')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Finalize')]
    [switch]$FinalizeCandidate,
    [Parameter(ParameterSetName = 'Finalize')]
    [string]$RepositoryRoot,
    [Parameter(ParameterSetName = 'Finalize')]
    [string]$LasalLogPath,
    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$RunSelfTest
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$Owner = 'LASAL.ClassesRebuildCandidateFinalizer'
$Schema = 'LasalClassesRebuildCandidateFinalization/v1'
$CheckpointCommit = '55435791f6e91c9dcb4e06dcd25a11d77b382da7'
$CheckpointClassesSha256 =
    '24402BFA76F1989319381388D4354E1528052078BA08504CC5C967A6DE1AA861'
$CheckpointClassesBlobOid = '7b0faebb1450ff67b7dad44f081ad5c4ac141ee2'
$CheckpointClassesBytes = 8549773L
$KnownRebuiltClassesSha256 =
    '6E11587634F11848832FA0E8D6702FB0AFF3CB60376F34728E69B667AEE00712'
$KnownNetworksSha256 =
    'C307547E097655AAE75BF1E8505B2A0C9DBFC998B3AF5BDD391BD8109604C23F'
$CanonicalProjectRelativePath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Elmo_EtherCAT_Test_4Axis.lcp'
$ClassesRelativePath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Class/Classes.lcb'
$NetworksRelativePath =
    'Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Network/Networks.lcb'
$BaselineRelativePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/' +
    'build_baseline_gate_d_rebaseline_6e115876.json'
$ConverterRelativePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d/' +
    'Convert-Lasal2LogToBuildTranscript.ps1'
$VerifierRelativePath =
    'LMC_Library/LMC_API_Delivery/tests/LasalMotionControlLib.Tests/' +
    'Verify-LasalC78RebuildEvidence.ps1'
$ComparatorRelativePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/' +
    'Compare-LasalClassesArtifact.ps1'
$ComparisonOracleRelativePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/' +
    'classes_lcb_gate_d_rebuild_24402bfa_to_6e115876.comparison.json'
$FinalizerRelativePath =
    'test/Reports_Lasal/C78_20260810_udp_callback_gate_d_rebaseline_6e115876/' +
    'Finalize-LasalClassesRebuildCandidate.ps1'
$FinalDirectoryName = 'candidate_finalization_gate_d_rebaseline_6e115876'
$OwnerMarkerName = '.finalizer-owner.json'
$TranscriptName = 'derived_build_transcript_gate_d_rebaseline_6e115876.txt'
$RawDeltaName = 'bounded_lasal2_delta_gate_d_rebaseline_6e115876.raw.txt'
$RawManifestName = 'bounded_lasal2_delta_gate_d_rebaseline_6e115876.manifest.json'
$ClassesSnapshotName = 'Classes.post-rebuild.snapshot.lcb'
$NetworksSnapshotName = 'Networks.post-rebuild.snapshot.lcb'
$ComparisonName = 'classes_lcb_gate_d_rebuild_candidate.comparison.json'
$CompleteManifestName = 'classes_lcb_gate_d_rebuild_candidate.finalization.json'
$Utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)

$TrustedArtifacts = @(
    [ordered]@{
        owner = 'rebuild baseline'
        relativePath = $BaselineRelativePath
        bytes = 6887L
        sha256 = 'BF55B202377C52D0880A7D1E1B7C5B719B3060F2E17BECF4A895820F13AC29C3'
    },
    [ordered]@{
        owner = 'log converter'
        relativePath = $ConverterRelativePath
        bytes = 32701L
        sha256 = '1A92CDE9AA7D45F6A2A250068A8A940ADAA46F856099E2D0174CC9CA09E61CEF'
    },
    [ordered]@{
        owner = 'C78 verifier'
        relativePath = $VerifierRelativePath
        bytes = 137844L
        sha256 = '7AE60A0BBD1356797E6431D29D3F6D0E39270D56C20B59AD835A3E8F0391A6E0'
    },
    [ordered]@{
        owner = 'Classes comparator'
        relativePath = $ComparatorRelativePath
        bytes = 79592L
        sha256 = 'B91BFB5AFE131F0ECB3F23DC00373BEC7FC91B2C37CF626D128E912F633EBBA4'
    },
    [ordered]@{
        owner = 'known 6E comparison oracle'
        relativePath = $ComparisonOracleRelativePath
        bytes = 51102L
        sha256 = '9E5EAC6B45840468E61B501D48FD6B58ADA42E3D1113EB10F1FC85B1D807A639'
    }
)

$HeaderComparedFieldsOracle = @(
    'owner',
    'sourcePath',
    'headerOffset',
    'recordEndOffset',
    'sourcePathOffset',
    'sourceMarkerOffset',
    'parser'
)
$FirstSpecialRecordOracle = [ordered]@{
    owner = '_AxisBase'
    sourcePath = '.\Class\_AxisBase\_AxisBase.st'
    parser = 'first-special-preamble-to-next-header'
    startOffset = 0L
    endOffsetExclusive = 170463L
    sourceOffset = 3147L
    bytes = 170463L
    sha256 = '43F8850C57209EDAC0312C17AEA735EEEE25035460D1F36437CDF590B7C11E86'
}
$GateDTargetRecordOracles = @(
    [ordered]@{
        owner = '_UDPTransceiver'
        sourcePath = '.\Class\_UDPTransceiver\_UDPTransceiver.st'
        parser = 'aa03-header-to-next-header-or-eof'
        startOffset = 4829520L
        endOffsetExclusive = 4882045L
        sourceOffset = 563L
        bytes = 52525L
        sha256 = '05D1B0DE7D36848AF1DBC3090AF7781F7D43F57ED8D36B546135881A5A512DA8'
    },
    [ordered]@{
        owner = 'LMCDiagnosticsService'
        sourcePath = '.\Class\LMCDiagnosticsService\LMCDiagnosticsService.st'
        parser = 'aa03-header-to-next-header-or-eof'
        startOffset = 6377241L
        endOffsetExclusive = 6486658L
        sourceOffset = 131L
        bytes = 109417L
        sha256 = '712DC6F0701296049FBD7BEB5FD1FE8F4A90633AD6C71B111C6C9B74667A9994'
    },
    [ordered]@{
        owner = 'LMCUdpCallbackSender'
        sourcePath = '.\Class\LMCUdpCallbackSender\LMCUdpCallbackSender.st'
        parser = 'aa03-header-to-next-header-or-eof'
        startOffset = 6586866L
        endOffsetExclusive = 6616783L
        sourceOffset = 1320L
        bytes = 29917L
        sha256 = '0B563A6F381729C7FF1856F8499324DD6ECAC2242F12EFEDD9EA1681F4D9A345'
    },
    [ordered]@{
        owner = 'TCPMotionInterface'
        sourcePath = '.\Class\TCPMotionInterface\TCPMotionInterface.st'
        parser = 'aa03-header-to-next-header-or-eof'
        startOffset = 7977214L
        endOffsetExclusive = 8024576L
        sourceOffset = 1609L
        bytes = 47362L
        sha256 = '4E34698FA046CE444067F5EDF15BAE0A1EB28BB3998D2702B81377E0F2B2BD21'
    }
)
$ProtectedDependencyRecordOracles = @(
    [ordered]@{
        owner = '_StdLib'
        sourcePath = '.\Class\_StdLib\_StdLib.st'
        parser = 'aa03-header-to-next-header+legacy-window-cross-check'
        startOffset = 4617622L
        endOffsetExclusive = 4640265L
        sourceOffset = 117L
        bytes = 22643L
        sha256 = '9C94AAE8601DFB04B10B5AC938AB147541D722120E1BD31AF8E54ACDD82668D3'
        legacyWindowExact = $true
    },
    [ordered]@{
        owner = 'CriticalSection'
        sourcePath = '.\Class\CriticalSection\CriticalSection.st'
        parser = 'aa03-header-to-next-header+legacy-window-cross-check'
        startOffset = 5013130L
        endOffsetExclusive = 5016594L
        sourceOffset = 125L
        bytes = 3464L
        sha256 = '5EB32DD30B652CC3B6A8BF61DAE20E709E856C787D60AEA8D59F77A1D5241966'
        legacyWindowExact = $true
    }
)
$FrozenOpaqueOwnersOracle = @(
    '_LMCABSEncoder',
    '_LMCAxis',
    '_LMCAxisBase',
    '_LMCAxisRef',
    '_LMCAxisVis',
    '_LMCAxisVisInt',
    '_LMCAxisVisLogHandle',
    '_LMCAxisVisLogViewer',
    '_LMCAxisVisPara',
    '_LMCAxisVOVMonitoring',
    '_LMCBaseCoord',
    '_LMCBeltAxis',
    '_LMCCalcModelBase',
    '_LMCCalcModelController',
    '_LMCMath_SO3',
    '_LMCMathFunctions',
    '_LMCProfile',
    '_LMCProfileBase',
    '_LMCProfileBuffer',
    '_LMCProfileLog',
    '_LMCProfileVis',
    '_LMCProfileVisAxis',
    '_LMCProfileVisInt',
    '_LMCProfileVisMovePara',
    '_LMCPublisher',
    '_LMCRefBase',
    '_LMCRobotBase',
    '_LMCRobotLog',
    '_LMCSafety',
    '_LMCSplineBuffer',
    '_LMCTableBuffer',
    '_LMCTool',
    'Controller',
    'MoveSplineTable',
    'PosController',
    'SigCLib'
)

function Throw-FinalizerBlocker {
    param([Parameter(Mandatory = $true)][string]$Message)
    throw "$Owner blocker: $Message"
}

function Get-FinalizationEngineIdentity {
    return [pscustomobject][ordered]@{
        psEdition = [string]$PSVersionTable.PSEdition
        major = [int]$PSVersionTable.PSVersion.Major
        version = [string]$PSVersionTable.PSVersion.ToString()
    }
}

function Assert-PowerShell7FinalizationEngine {
    param(
        [Parameter(Mandatory = $true)][string]$Phase,
        $Expected
    )
    $actual = Get-FinalizationEngineIdentity
    if (($actual.psEdition -cne 'Core') -or ($actual.major -lt 7)) {
        Throw-FinalizerBlocker (
            'production finalization requires PowerShell Core 7 or newer because ' +
            'Windows PowerShell 5 cannot reliably enumerate directory NTFS streams; ' +
            "phase=$Phase edition=$($actual.psEdition) major=$($actual.major).")
    }
    if ($null -ne $Expected -and
        (($actual.psEdition -cne [string]$Expected.psEdition) -or
            ($actual.major -ne [int]$Expected.major) -or
            ($actual.version -cne [string]$Expected.version))) {
        Throw-FinalizerBlocker (
            "PowerShell engine identity changed at $Phase.")
    }
    return $actual
}

function Get-NormalizedFullPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    $full = [IO.Path]::GetFullPath($Path)
    $root = [IO.Path]::GetPathRoot($full)
    if ([string]::Equals(
            $full,
            $root,
            [StringComparison]::OrdinalIgnoreCase)) {
        return $full
    }
    return $full.TrimEnd('\', '/')
}

function Test-PathInsideRoot {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$AllowEqual
    )
    $normalizedRoot = Get-NormalizedFullPath -Path $Root
    $normalizedPath = Get-NormalizedFullPath -Path $Path
    if ([string]::Equals(
            $normalizedRoot,
            $normalizedPath,
            [StringComparison]::OrdinalIgnoreCase)) {
        return [bool]$AllowEqual
    }
    $rootPrefix = $normalizedRoot
    if (-not ($rootPrefix.EndsWith(
                [IO.Path]::DirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal) -or
            $rootPrefix.EndsWith(
                [IO.Path]::AltDirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal))) {
        $rootPrefix += [IO.Path]::DirectorySeparatorChar
    }
    return $normalizedPath.StartsWith(
        $rootPrefix,
        [StringComparison]::OrdinalIgnoreCase)
}

function Assert-NoReparsePointChain {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Path,
        [switch]$IncludeLeaf,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    $normalizedRoot = Get-NormalizedFullPath -Path $Root
    $normalizedPath = Get-NormalizedFullPath -Path $Path
    if (-not (Test-PathInsideRoot `
            -Root $normalizedRoot -Path $normalizedPath -AllowEqual)) {
        Throw-FinalizerBlocker "$PathOwner escapes its trusted root."
    }
    $targets = New-Object Collections.Generic.List[string]
    $volumeRoot = [IO.Path]::GetPathRoot($normalizedRoot)
    $absoluteCurrent = $volumeRoot
    $targets.Add($absoluteCurrent)
    $rootRelative = $normalizedRoot.Substring($volumeRoot.Length).TrimStart('\', '/')
    if (-not [string]::IsNullOrEmpty($rootRelative)) {
        foreach ($rootPart in ($rootRelative -split '[\\/]')) {
            $absoluteCurrent = Join-Path $absoluteCurrent $rootPart
            $targets.Add($absoluteCurrent)
        }
    }
    $relative = $normalizedPath.Substring($normalizedRoot.Length).TrimStart('\', '/')
    $current = $normalizedRoot
    if (-not [string]::IsNullOrEmpty($relative)) {
        $parts = $relative -split '[\\/]'
        for ($index = 0; $index -lt $parts.Count; $index++) {
            $current = Join-Path $current $parts[$index]
            $isLeaf = ($index -eq ($parts.Count - 1))
            if ($isLeaf -and -not $IncludeLeaf) {
                break
            }
            $targets.Add($current)
        }
    }
    foreach ($target in $targets) {
        if (-not (Test-Path -LiteralPath $target)) {
            break
        }
        $attributes = [IO.File]::GetAttributes($target)
        if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-FinalizerBlocker "$PathOwner contains a reparse point: $target"
        }
    }
}

function Get-BytesSha256 {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($sha.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function Get-ByteRangeSha256 {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][int]$Offset,
        [Parameter(Mandatory = $true)][int]$Count
    )
    if (($Offset -lt 0) -or ($Count -lt 0) -or
        (($Offset + $Count) -gt $Bytes.Length)) {
        Throw-FinalizerBlocker 'SHA-256 byte range is outside the file.'
    }
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $sha.ComputeHash($Bytes, $Offset, $Count))).Replace('-', '')
    }
    finally {
        $sha.Dispose()
    }
}

function Read-StableFileBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$FileOwner
    )
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        Throw-FinalizerBlocker "$FileOwner does not exist: $Path"
    }
    [byte[]]$first = [IO.File]::ReadAllBytes($Path)
    [byte[]]$second = [IO.File]::ReadAllBytes($Path)
    if (($first.Length -ne $second.Length) -or
        ((Get-BytesSha256 -Bytes $first) -cne
            (Get-BytesSha256 -Bytes $second))) {
        Throw-FinalizerBlocker "$FileOwner changed while it was read."
    }
    return $second
}

function Get-FileIdentity {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$FileOwner
    )
    [byte[]]$bytes = Read-StableFileBytes -Path $Path -FileOwner $FileOwner
    return [pscustomobject]@{
        path = Get-NormalizedFullPath -Path $Path
        bytes = [long]$bytes.LongLength
        sha256 = Get-BytesSha256 -Bytes $bytes
    }
}

function Assert-IdentityValue {
    param(
        [Parameter(Mandatory = $true)]$Actual,
        [Parameter(Mandatory = $true)][long]$ExpectedBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedSha256,
        [Parameter(Mandatory = $true)][string]$IdentityOwner
    )
    if (([long]$Actual.bytes -ne $ExpectedBytes) -or
        ([string]$Actual.sha256 -cne $ExpectedSha256.ToUpperInvariant())) {
        Throw-FinalizerBlocker (
            "$IdentityOwner identity is $($Actual.bytes)/$($Actual.sha256), " +
            "expected $ExpectedBytes/$($ExpectedSha256.ToUpperInvariant()).")
    }
}

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Operation
    )
    $output = @(& git -C $Root @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        Throw-FinalizerBlocker (
            "$Operation failed with git exit ${exitCode}: " +
            (($output | ForEach-Object { [string]$_ }) -join ' | '))
    }
    return (($output | ForEach-Object { [string]$_ }) -join "`n").Trim()
}

function Assert-GitTrackedCleanPath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    [void](Invoke-GitText `
            -Root $Root `
            -Arguments @('ls-files', '--error-unmatch', '--', $RelativePath) `
            -Operation "$PathOwner tracked-path check")
    & git -C $Root diff --quiet --no-ext-diff HEAD -- $RelativePath
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        Throw-FinalizerBlocker (
            "$PathOwner differs from the current committed HEAD (git exit $exitCode).")
    }
}

function Get-LasalProcessCount {
    return @(Get-Process -Name 'Lasal2' -ErrorAction SilentlyContinue).Count
}

function Assert-LasalStopped {
    param([Parameter(Mandatory = $true)][string]$Phase)
    $count = Get-LasalProcessCount
    if ($count -ne 0) {
        Throw-FinalizerBlocker (
            "$Phase requires Lasal2 process count 0; observed $count.")
    }
}

function Get-IndexedLogLines {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )
    $rawLines = $Text -split '\r?\n'
    $lines = New-Object Collections.Generic.List[object]
    for ($index = 0; $index -lt $rawLines.Count; $index++) {
        $textLine = $rawLines[$index].TrimEnd("`r")
        $match = [regex]::Match(
            $textLine,
            ('^\[[^\]]*\bP:(?<Pid>\d+)\s+T:(?<Tid>\d+)[^\]]*?' +
                '\((?<Level>INFO|WARN|ERROR|FATAL|DEBUG|NOTICE)\)\s+' +
                '(?<Source>[^\]]+)\]\s*(?<Body>.*)$'))
        $pidValue = $null
        $tidValue = $null
        $levelValue = $null
        $sourceValue = $null
        $bodyValue = $null
        if ($match.Success) {
            $pidValue = $match.Groups['Pid'].Value
            $tidValue = $match.Groups['Tid'].Value
            $levelValue = $match.Groups['Level'].Value
            $sourceValue = $match.Groups['Source'].Value
            $bodyValue = $match.Groups['Body'].Value
        }
        $lines.Add([pscustomobject]@{
                index = $index
                text = $textLine
                pid = $pidValue
                tid = $tidValue
                level = $levelValue
                source = $sourceValue
                body = $bodyValue
            })
    }
    return $lines.ToArray()
}

function Get-ExactSingleLine {
    param(
        [Parameter(Mandatory = $true)][object[]]$Lines,
        [Parameter(Mandatory = $true)][scriptblock]$Predicate,
        [Parameter(Mandatory = $true)][string]$LineOwner
    )
    $matches = @($Lines | Where-Object $Predicate)
    if ($matches.Count -ne 1) {
        Throw-FinalizerBlocker (
            "$LineOwner count is $($matches.Count), expected exactly 1.")
    }
    return $matches[0]
}

function Get-CmdProcTerminalLedger {
    param(
        [Parameter(Mandatory = $true)][object[]]$Lines,
        [Parameter(Mandatory = $true)][object[]]$Commands
    )
    $terminals = @($Lines | Where-Object {
            $_.text -match '(?i)Last\s+command\s+(?:succeeded|failed)\.'
        } | Sort-Object -Property index)
    foreach ($terminal in $terminals) {
        if (($terminal.source -cne 'CmdProc') -or
            ($terminal.level -cne 'INFO') -or
            ($terminal.body -cnotmatch
                '^Last command (?:succeeded|failed)\.(?: \([0-9]+(?:\.[0-9]+)?ms\))?$')) {
            Throw-FinalizerBlocker (
                "malformed or non-CmdProc command terminal: $($terminal.text)")
        }
    }
    if ($terminals.Count -ne $Commands.Count) {
        Throw-FinalizerBlocker (
            "CmdProc terminal count is $($terminals.Count), command count is " +
            "$($Commands.Count); exact 1:1 pairing is required.")
    }
    $consumed = @{}
    $reports = New-Object Collections.Generic.List[object]
    foreach ($command in ($Commands | Sort-Object -Property index)) {
        $nextSameThreadCommand = @($Commands | Where-Object {
                $_.index -gt $command.index -and
                $_.pid -ceq $command.pid -and
                $_.tid -ceq $command.tid
            } | Sort-Object -Property index | Select-Object -First 1)
        $endExclusive = $Lines.Count
        if ($nextSameThreadCommand.Count -eq 1) {
            $endExclusive = [int]$nextSameThreadCommand[0].index
        }
        $matches = @($terminals | Where-Object {
                $_.index -gt $command.index -and
                $_.index -lt $endExclusive -and
                $_.pid -ceq $command.pid -and
                $_.tid -ceq $command.tid
            })
        if ($matches.Count -ne 1) {
            Throw-FinalizerBlocker (
                "command '$($command.command)' has $($matches.Count) next same-thread " +
                'terminals before the next same-thread command; expected exactly 1.')
        }
        $terminal = $matches[0]
        if ($terminal.body -cnotmatch '^Last command succeeded\.') {
            Throw-FinalizerBlocker (
                "command '$($command.command)' did not end in success.")
        }
        $terminalKey = [string]$terminal.index
        if ($consumed.ContainsKey($terminalKey)) {
            Throw-FinalizerBlocker 'one CmdProc terminal was shared by multiple commands.'
        }
        $consumed[$terminalKey] = $true
        $reports.Add([ordered]@{
                command = $command.command
                pid = [int]$command.pid
                tid = [int]$command.tid
                commandLineIndex = [int]$command.index
                terminalLineIndex = [int]$terminal.index
                commandRaw = $command.raw
                terminalRaw = $terminal.text
                uniqueNextSameThreadSuccess = $true
            })
    }
    foreach ($terminal in $terminals) {
        if (-not $consumed.ContainsKey([string]$terminal.index)) {
            Throw-FinalizerBlocker (
                "orphan CmdProc terminal was not consumed: $($terminal.text)")
        }
    }
    return $reports.ToArray()
}

function Get-TerminalLedgerEntry {
    param(
        [Parameter(Mandatory = $true)][object[]]$Ledger,
        [Parameter(Mandatory = $true)]$Command,
        [Parameter(Mandatory = $true)][string]$CommandOwner
    )
    $matches = @($Ledger | Where-Object {
            [int]$_.commandLineIndex -eq [int]$Command.index
        })
    if ($matches.Count -ne 1) {
        Throw-FinalizerBlocker (
            "$CommandOwner terminal-ledger count is $($matches.Count), expected 1.")
    }
    return $matches[0]
}

function Test-LoadRestorationCommand {
    param(
        [Parameter(Mandatory = $true)][string]$CommandText,
        [Parameter(Mandatory = $true)][string]$CanonicalProjectRoot
    )
    if ($CommandText -match
        "^Open Network Editor for '[A-Za-z_][A-Za-z0-9_]*'$" ) {
        return $true
    }
    if ($CommandText -match
        '^Open Implementation Editor for "[A-Za-z_][A-Za-z0-9_]*"$') {
        return $true
    }
    $fileMatch = [regex]::Match(
        $CommandText,
        '^Open File Editor for "(?<Path>[^"]+)"$')
    if ($fileMatch.Success) {
        if (-not [IO.Path]::IsPathRooted($fileMatch.Groups['Path'].Value)) {
            return $false
        }
        return Test-PathInsideRoot `
            -Root $CanonicalProjectRoot `
            -Path $fileMatch.Groups['Path'].Value
    }
    return $false
}

function Assert-IsolatedRebuildSessionContract {
    param(
        [Parameter(Mandatory = $true)][string]$AppendedLogText,
        [Parameter(Mandatory = $true)][string]$CanonicalProjectPath
    )
    if ([string]::IsNullOrEmpty($AppendedLogText)) {
        Throw-FinalizerBlocker 'bounded log delta is empty.'
    }
    if ($AppendedLogText -match '(?i)CInvalidArgException') {
        Throw-FinalizerBlocker 'bounded log delta contains CInvalidArgException.'
    }
    if ($AppendedLogText -match
        '(?i)Last\s+command\s+failed\.|ios_base::failure') {
        Throw-FinalizerBlocker (
            'bounded log delta contains a command failure or persistence failure.')
    }
    if ($AppendedLogText -match
        '(?i)Find\s+in\s+Implementation|Edit\s+Method') {
        Throw-FinalizerBlocker 'bounded log delta contains a forbidden Find/Edit method action.'
    }
    $lines = @(Get-IndexedLogLines -Text $AppendedLogText)
    $start = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'GUI Start Application' `
        -Predicate {
            $_.source -ceq 'GUI' -and $_.level -ceq 'INFO' -and
            $_.body -cmatch
                '^Start Application at \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$'
        }
    $unparsedNonempty = @($lines | Where-Object {
            -not [string]::IsNullOrWhiteSpace($_.text) -and
            [string]::IsNullOrEmpty($_.pid)
        })
    if (($unparsedNonempty.Count -ne 1) -or
        ($unparsedNonempty[0].index -ne 0) -or
        ($unparsedNonempty[0].text -cnotmatch
            '^\[\d{2}:\d{2}:\d{2} \(INFO\) Application\] Log File is ok$')) {
        Throw-FinalizerBlocker (
            'bounded log must contain exactly one known first unparsed startup record.')
    }
    $preStart = @($lines | Where-Object {
            $_.index -lt $start.index -and
            -not [string]::IsNullOrWhiteSpace($_.text)
        } | Sort-Object -Property index)
    if (($start.index -ne 6) -or ($preStart.Count -ne 6)) {
        Throw-FinalizerBlocker (
            'pre-Start startup prologue count/order differs from the six-record contract.')
    }
    $startupDefinitions = @(
        [ordered]@{
            source = 'OutputSkripting'
            bodyPattern = "^Run Scriptfile 'C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\Lasal2\.py'\.$"
        },
        [ordered]@{
            source = 'OutputSkripting'
            bodyPattern = '^Total Script need [0-9]+(?:\.[0-9]+)? ms$'
        },
        [ordered]@{
            source = 'OutputDataAnalyzer'
            bodyPattern = '^Loading DataAnalyzer configuration file "C:\\ProgramData\\Sigmatek\\Drive\(C\)\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Config\\DataAnalyser\.lcc"\.$'
        },
        [ordered]@{
            source = 'OutputDataAnalyzer'
            bodyPattern = '^Loading DataAnalyzer configuration file "C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\DataAnalyserSDD\.lcc"\.$'
        },
        [ordered]@{
            source = 'OutputDataAnalyzer'
            bodyPattern = '^Cannot find configuration file: "C:\\Program Files \(x86\)\\Sigmatek\\Lasal\\Class2\\Bin\\DataAnalyserSDD\.lcc"$'
        }
    )
    $preStartReports = New-Object Collections.Generic.List[object]
    $preStartReports.Add([ordered]@{
            source = 'Application'
            body = 'Log File is ok'
            raw = $unparsedNonempty[0].text
            lineIndex = [int]$unparsedNonempty[0].index
            acceptedAsStartupOnlyPrologue = $true
        })
    for ($startupIndex = 0; $startupIndex -lt $startupDefinitions.Count; $startupIndex++) {
        $line = $preStart[$startupIndex + 1]
        $definition = $startupDefinitions[$startupIndex]
        if (($line.index -ne ($startupIndex + 1)) -or
            ($line.pid -cne $start.pid) -or
            ($line.level -cne 'INFO') -or
            ($line.source -cne $definition.source) -or
            ($line.body -cnotmatch $definition.bodyPattern)) {
            Throw-FinalizerBlocker (
                "pre-Start startup record differs at index $($startupIndex + 1).")
        }
        $preStartReports.Add([ordered]@{
                source = $line.source
                body = $line.body
                raw = $line.text
                lineIndex = [int]$line.index
                acceptedAsStartupOnlyPrologue = $true
            })
    }
    $sessionPids = @($lines | Where-Object {
            -not [string]::IsNullOrEmpty($_.pid)
        } | Select-Object -ExpandProperty pid -Unique)
    if (($sessionPids.Count -ne 1) -or
        ($sessionPids[0] -cne $start.pid)) {
        Throw-FinalizerBlocker 'bounded log is not one PID-tagged LASAL session.'
    }

    $rawCommandOccurrences = @($lines | Where-Object {
            $_.text -match '(?i)Executing\s+command'
        } | Sort-Object -Property index)
    $commands = @($rawCommandOccurrences | ForEach-Object {
            $match = [regex]::Match(
                [string]$_.body,
                "^Executing command '(?<Command>.*)'$")
            if (($_.source -cne 'CmdProc') -or
                ($_.level -cne 'INFO') -or
                (-not $match.Success)) {
                Throw-FinalizerBlocker (
                    "Executing command occurrence is malformed or not exact CmdProc/INFO: " +
                    $_.text)
            }
            [pscustomobject]@{
                index = $_.index
                pid = $_.pid
                tid = $_.tid
                command = $match.Groups['Command'].Value
                raw = $_.text
            }
        })
    $terminalLedger = @(Get-CmdProcTerminalLedger -Lines $lines -Commands $commands)
    $loadCommands = @($commands | Where-Object {
            $_.command -match '^Load Project "[^"]+"$'
        })
    if ($loadCommands.Count -ne 1) {
        Throw-FinalizerBlocker (
            "canonical Load Project count is $($loadCommands.Count), expected 1.")
    }
    $load = $loadCommands[0]
    $loadMatch = [regex]::Match(
        $load.command,
        '^Load Project "(?<Path>[^"]+)"$')
    if (-not [string]::Equals(
            (Get-NormalizedFullPath -Path $loadMatch.Groups['Path'].Value),
            (Get-NormalizedFullPath -Path $CanonicalProjectPath),
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-FinalizerBlocker 'the single Load Project path is not canonical.'
    }
    $rebuildCommands = @($commands | Where-Object {
            $_.command -ceq 'Rebuild project'
        })
    if ($rebuildCommands.Count -ne 1) {
        Throw-FinalizerBlocker (
            "Rebuild project count is $($rebuildCommands.Count), expected 1.")
    }
    $rebuild = $rebuildCommands[0]
    $closeCommands = @($commands | Where-Object {
            $_.command -ceq 'Close Project'
        })
    if ($closeCommands.Count -ne 1) {
        Throw-FinalizerBlocker (
            "Close Project count is $($closeCommands.Count), expected 1.")
    }
    $close = $closeCommands[0]
    if (-not ($start.index -lt $load.index -and
            $load.index -lt $rebuild.index -and
            $rebuild.index -lt $close.index)) {
        Throw-FinalizerBlocker 'Start/Load/Rebuild/Close ordering is invalid.'
    }

    $loadResult = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'same-Load-TID ResultCount' `
        -Predicate {
            $_.index -gt $load.index -and $_.index -lt $rebuild.index -and
            $_.pid -ceq $load.pid -and $_.tid -ceq $load.tid -and
            $_.source -ceq 'Compiler' -and $_.level -ceq 'INFO' -and
            $_.body -ceq '{ResultCount}'
        }
    $loadTerminal = Get-TerminalLedgerEntry `
        -Ledger $terminalLedger `
        -Command $load `
        -CommandOwner 'Load Project'
    if (-not ($loadResult.index -lt $loadTerminal.terminalLineIndex -and
            $loadTerminal.terminalLineIndex -lt $rebuild.index)) {
        Throw-FinalizerBlocker 'Load Project did not complete before Rebuild.'
    }
    $canonicalProjectRoot = Split-Path -Parent $CanonicalProjectPath
    $restorationReports = New-Object Collections.Generic.List[object]
    foreach ($command in $commands) {
        if (($command.index -eq $load.index) -or
            ($command.index -eq $rebuild.index) -or
            ($command.index -eq $close.index)) {
            continue
        }
        if (($command.index -le $load.index) -or
            ($command.index -ge $loadResult.index) -or
            (-not (Test-LoadRestorationCommand `
                    -CommandText $command.command `
                    -CanonicalProjectRoot $canonicalProjectRoot))) {
            Throw-FinalizerBlocker (
                "unknown, mutating, or out-of-window command: $($command.command)")
        }
        $restoreTerminal = Get-TerminalLedgerEntry `
            -Ledger $terminalLedger `
            -Command $command `
            -CommandOwner 'load-restoration command'
        if ($restoreTerminal.terminalLineIndex -ge $loadResult.index) {
            Throw-FinalizerBlocker (
                'load-restoration command did not finish before Load ResultCount.')
        }
        $restorationReports.Add([ordered]@{
                command = $command.command
                raw = $command.raw
                commandLineIndex = [int]$command.index
                successLineIndex = [int]$restoreTerminal.terminalLineIndex
                acceptedAsLoadRestorationByBoundedOrdering = $true
                operatorOriginProven = $false
            })
    }

    $result = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'same-Rebuild-TID ResultCount' `
        -Predicate {
            $_.index -gt $rebuild.index -and $_.index -lt $close.index -and
            $_.pid -ceq $rebuild.pid -and $_.tid -ceq $rebuild.tid -and
            $_.source -ceq 'Compiler' -and $_.level -ceq 'INFO' -and
            $_.body -ceq '{ResultCount}'
        }
    $saveNeedle = "Save project '$CanonicalProjectPath'."
    $allOutputCommandSaves = @($lines | Where-Object {
            $_.text -match '(?i)\bSave\s+project\b'
        })
    if ($allOutputCommandSaves.Count -ne 1) {
        Throw-FinalizerBlocker (
            "global OutputCommand Save count is $($allOutputCommandSaves.Count), " +
            'expected exactly one canonical same-Rebuild auto-save.')
    }
    $autoSave = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'same-Rebuild-TID OutputCommand auto Save' `
        -Predicate {
            $_.index -gt $rebuild.index -and $_.index -lt $result.index -and
            $_.pid -ceq $rebuild.pid -and $_.tid -ceq $rebuild.tid -and
            $_.source -ceq 'OutputCommand' -and $_.level -ceq 'INFO' -and
            $_.body -ceq $saveNeedle
        }
    $rebuildTerminal = Get-TerminalLedgerEntry `
        -Ledger $terminalLedger `
        -Command $rebuild `
        -CommandOwner 'Rebuild project'
    if (-not ($rebuild.index -lt $autoSave.index -and
            $autoSave.index -lt $result.index -and
            $result.index -lt $rebuildTerminal.terminalLineIndex)) {
        Throw-FinalizerBlocker 'Rebuild Save/Result/success ordering is invalid.'
    }

    $doExit = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'GUI Do exit Lasal2' `
        -Predicate {
            $_.source -ceq 'GUI' -and $_.level -ceq 'INFO' -and
            $_.body -ceq 'Do exit Lasal2...'
        }
    $closeTerminal = Get-TerminalLedgerEntry `
        -Ledger $terminalLedger `
        -Command $close `
        -CommandOwner 'Close Project'
    $exitDone = Get-ExactSingleLine `
        -Lines $lines `
        -LineOwner 'GUI LC2 exit done' `
        -Predicate {
            $_.source -ceq 'GUI' -and $_.level -ceq 'INFO' -and
            $_.body -ceq '...LC2 exit done.'
        }
    if (-not ($rebuildTerminal.terminalLineIndex -lt $doExit.index -and
            $doExit.index -lt $close.index -and
            $close.index -lt $closeTerminal.terminalLineIndex -and
            $closeTerminal.terminalLineIndex -lt $exitDone.index)) {
        Throw-FinalizerBlocker 'Rebuild/exit/Close/exit-done ordering is invalid.'
    }
    $postExitRecords = @($lines | Where-Object {
            $_.index -gt $exitDone.index -and
            -not [string]::IsNullOrWhiteSpace($_.text)
        })
    if ($postExitRecords.Count -ne 0) {
        Throw-FinalizerBlocker 'bounded log contains nonempty records after LC2 exit done.'
    }

    $errorLines = @($lines | Where-Object {
            $_.level -in @('ERROR', 'FATAL')
        })
    $knownLoadErrors = New-Object Collections.Generic.List[object]
    foreach ($errorLine in $errorLines) {
        $isKnownLoadError =
            $errorLine.level -ceq 'ERROR' -and
            $errorLine.source -ceq 'Compiler' -and
            $errorLine.pid -ceq $load.pid -and
            $errorLine.tid -ceq $load.tid -and
            $errorLine.index -gt $load.index -and
            $errorLine.index -lt $loadResult.index -and
            $errorLine.body -match (
                '^E 0015 ".*"\(\d+\) Error reading file ' +
                "'.*\\Class\\_DriveMngBase\\DriveComL2\.h'" +
                '\|\*000000\*\|15\|11015\|\|$')
        if (-not $isKnownLoadError) {
            Throw-FinalizerBlocker (
                "unexpected ERROR/FATAL line: $($errorLine.text)")
        }
        $knownLoadErrors.Add([ordered]@{
                raw = $errorLine.text
                acceptedAsKnownVendorLoadError = $true
                rebuildError = $false
            })
    }
    if ($knownLoadErrors.Count -gt 1) {
        Throw-FinalizerBlocker (
            "known vendor Load E0015 count is $($knownLoadErrors.Count), maximum 1.")
    }

    return [ordered]@{
        sessionPid = [int]$start.pid
        loadTid = [int]$load.tid
        rebuildTid = [int]$rebuild.tid
        closeTid = [int]$close.tid
        startLineIndex = [int]$start.index
        preStartPrologue = $preStartReports.ToArray()
        loadLineIndex = [int]$load.index
        loadResultLineIndex = [int]$loadResult.index
        loadTerminalLineIndex = [int]$loadTerminal.terminalLineIndex
        rebuildLineIndex = [int]$rebuild.index
        rebuildResultLineIndex = [int]$result.index
        rebuildTerminalLineIndex = [int]$rebuildTerminal.terminalLineIndex
        doExitLineIndex = [int]$doExit.index
        closeLineIndex = [int]$close.index
        closeTerminalLineIndex = [int]$closeTerminal.terminalLineIndex
        exitDoneLineIndex = [int]$exitDone.index
        exactSessionCount = 1
        exactRebuildCount = 1
        loadRestorationCommands = $restorationReports.ToArray()
        commandTerminalLedger = $terminalLedger
        knownLoadErrors = $knownLoadErrors.ToArray()
        prohibitedCommandCount = 0
        cInvalidArgExceptionCount = 0
        rebuildErrorCount = 0
    }
}

function ConvertTo-ComparatorCanonicalJsonBytes {
    param([Parameter(Mandatory = $true)]$Value)
    $json = ($Value | ConvertTo-Json -Depth 18 -Compress)
    $canonical = [regex]::Replace(
        $json,
        "[^\x00-\x7F]|[&'<>]",
        [Text.RegularExpressions.MatchEvaluator]{
            param($match)
            $code = [int][char]$match.Value[0]
            if ($code -in @(0x26, 0x27, 0x3C, 0x3E)) {
                return ('\u{0:x4}' -f $code)
            }
            return ('\u{0:X4}' -f $code)
        })
    if ([regex]::IsMatch($canonical, '[^\x00-\x7F]')) {
        Throw-FinalizerBlocker 'comparator canonical JSON is not 7-bit ASCII.'
    }
    return ,$Utf8NoBom.GetBytes($canonical + "`n")
}

function Test-ByteSequencesExact {
    param(
        [Parameter(Mandatory = $true)][byte[]]$Actual,
        [Parameter(Mandatory = $true)][byte[]]$Expected
    )
    if ($Actual.LongLength -ne $Expected.LongLength) { return $false }
    for ($index = 0L; $index -lt $Actual.LongLength; $index++) {
        if ($Actual[$index] -ne $Expected[$index]) { return $false }
    }
    return $true
}

function Read-StrictJsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$FileOwner,
        [switch]$RequireComparatorCanonicalRoundTrip
    )
    [byte[]]$bytes = Read-StableFileBytes -Path $Path -FileOwner $FileOwner
    try {
        $text = $Utf8Strict.GetString($bytes)
        $parsed = $text | ConvertFrom-Json
    }
    catch {
        Throw-FinalizerBlocker (
            "$FileOwner is not strict UTF-8 JSON: $($_.Exception.Message)")
    }
    if ($RequireComparatorCanonicalRoundTrip) {
        [byte[]]$canonicalBytes = ConvertTo-ComparatorCanonicalJsonBytes -Value $parsed
        if (-not (Test-ByteSequencesExact -Actual $bytes -Expected $canonicalBytes)) {
            Throw-FinalizerBlocker (
                "$FileOwner is not exact comparator-canonical JSON bytes.")
        }
    }
    return $parsed
}

function ConvertTo-JsonBytes {
    param([Parameter(Mandatory = $true)]$Value)
    $text = ($Value | ConvertTo-Json -Depth 30).Replace("`r`n", "`n") + "`n"
    return $Utf8NoBom.GetBytes($text)
}

function Write-CreateNewBytes {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][byte[]]$Bytes,
        [Parameter(Mandatory = $true)][string]$FileOwner
    )
    $stream = $null
    try {
        $stream = [IO.File]::Open(
            $Path,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None)
        $stream.Write($Bytes, 0, $Bytes.Length)
    }
    catch [IO.IOException] {
        Throw-FinalizerBlocker "$FileOwner already exists or cannot be created: $Path"
    }
    finally {
        if ($null -ne $stream) {
            $stream.Dispose()
        }
    }
    [byte[]]$readback = [IO.File]::ReadAllBytes($Path)
    if (($readback.Length -ne $Bytes.Length) -or
        ((Get-BytesSha256 -Bytes $readback) -cne
            (Get-BytesSha256 -Bytes $Bytes))) {
        Throw-FinalizerBlocker "$FileOwner CreateNew readback differs."
    }
}

function Assert-PublicationNamespaceAvailable {
    param([Parameter(Mandatory = $true)][string]$FinalDirectory)
    Assert-NoReparsePointChain `
        -Root $PSScriptRoot `
        -Path $FinalDirectory `
        -PathOwner 'final output directory'
    if (Test-Path -LiteralPath $FinalDirectory) {
        Throw-FinalizerBlocker (
            "final output directory already exists; no overwrite or implicit retry is " +
            "allowed: $FinalDirectory")
    }
    $staleStages = @(Get-ChildItem `
            -LiteralPath $PSScriptRoot `
            -Force `
            -Directory `
            -ErrorAction Stop | Where-Object {
                $_.Name.StartsWith(
                    '.finalize-stage-',
                    [StringComparison]::Ordinal)
            })
    if ($staleStages.Count -ne 0) {
        Throw-FinalizerBlocker (
            'stale or ambiguous finalizer staging directory exists; review it ' +
            'manually and do not overwrite/delete it automatically: ' +
            (($staleStages | ForEach-Object { $_.FullName }) -join ', '))
    }
}

function New-OwnedStageDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$OwnerToken,
        [string]$StageRoot = $PSScriptRoot,
        [switch]$ForceMarkerWriteFailureForSelfTest
    )
    $resolvedStageRoot = Get-NormalizedFullPath -Path $StageRoot
    $stageName = '.finalize-stage-' + ([Guid]::NewGuid().ToString('N'))
    $stagePath = Join-Path $resolvedStageRoot $stageName
    Assert-NoReparsePointChain `
        -Root $resolvedStageRoot `
        -Path $stagePath `
        -PathOwner 'new staging directory'
    if (Test-Path -LiteralPath $stagePath) {
        Throw-FinalizerBlocker 'new staging directory unexpectedly already exists.'
    }
    [void][IO.Directory]::CreateDirectory($stagePath)
    $markerPath = Join-Path $stagePath $OwnerMarkerName
    $marker = [ordered]@{
        schema = 'LasalClassesRebuildCandidateFinalizerOwner/v1'
        ownerToken = $OwnerToken
        stageDirectoryName = $stageName
        overwriteAllowed = $false
        productionApproved = $false
    }
    try {
        if ($ForceMarkerWriteFailureForSelfTest) {
            Throw-FinalizerBlocker 'forced owner-marker write failure for self-test.'
        }
        Write-CreateNewBytes `
            -Path $markerPath `
            -Bytes (ConvertTo-JsonBytes -Value $marker) `
            -FileOwner 'staging owner marker'
    }
    catch {
        $markerFailure = $_
        try {
            Assert-NoReparsePointChain `
                -Root $resolvedStageRoot `
                -Path $stagePath `
                -IncludeLeaf `
                -PathOwner 'failed new staging directory cleanup'
            Assert-NoReparsePointDescendants `
                -Directory $stagePath `
                -DirectoryOwner 'failed new staging directory cleanup'
            Assert-OwnedStageCleanupInventory `
                -Stage ([pscustomobject]@{ path = $stagePath })
            [IO.Directory]::Delete($stagePath, $true)
        }
        catch {
            Throw-FinalizerBlocker (
                "owner-marker failure: $($markerFailure.Exception.Message); " +
                "new-stage cleanup also failed: $($_.Exception.Message)")
        }
        throw $markerFailure
    }
    return [pscustomobject]@{
        path = Get-NormalizedFullPath -Path $stagePath
        name = $stageName
        markerPath = Get-NormalizedFullPath -Path $markerPath
        ownerToken = $OwnerToken
        stageRoot = $resolvedStageRoot
    }
}

function Assert-OwnedStageMarkerContract {
    param([Parameter(Mandatory = $true)]$Stage)
    $expectedStagePath = Get-NormalizedFullPath -Path (
        Join-Path $Stage.stageRoot ([string]$Stage.name))
    $expectedMarkerPath = Get-NormalizedFullPath -Path (
        Join-Path $expectedStagePath $OwnerMarkerName)
    if ([string]::IsNullOrWhiteSpace([string]$Stage.ownerToken) -or
        (-not [string]::Equals(
            (Get-NormalizedFullPath -Path $Stage.path),
            $expectedStagePath,
            [StringComparison]::OrdinalIgnoreCase)) -or
        (-not [string]::Equals(
            (Get-NormalizedFullPath -Path $Stage.markerPath),
            $expectedMarkerPath,
            [StringComparison]::OrdinalIgnoreCase))) {
        Throw-FinalizerBlocker 'staging owner marker path/token context differs.'
    }
    Assert-NoReparsePointChain `
        -Root $Stage.stageRoot `
        -Path $expectedMarkerPath `
        -IncludeLeaf `
        -PathOwner 'staging owner marker contract'
    $expectedMarker = [ordered]@{
        schema = 'LasalClassesRebuildCandidateFinalizerOwner/v1'
        ownerToken = [string]$Stage.ownerToken
        stageDirectoryName = [string]$Stage.name
        overwriteAllowed = $false
        productionApproved = $false
    }
    [byte[]]$expectedBytes = ConvertTo-JsonBytes -Value $expectedMarker
    $actualIdentity = Get-FileIdentity `
        -Path $expectedMarkerPath `
        -FileOwner 'staging owner marker contract'
    Assert-IdentityValue `
        -Actual $actualIdentity `
        -ExpectedBytes $expectedBytes.LongLength `
        -ExpectedSha256 (Get-BytesSha256 -Bytes $expectedBytes) `
        -IdentityOwner 'staging owner marker exact content'
    return $actualIdentity
}

function Assert-NoReparsePointDescendants {
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][string]$DirectoryOwner
    )
    $queue = New-Object Collections.Generic.Queue[string]
    $queue.Enqueue((Get-NormalizedFullPath -Path $Directory))
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        foreach ($entry in [IO.Directory]::GetFileSystemEntries($current)) {
            $attributes = [IO.File]::GetAttributes($entry)
            if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                Throw-FinalizerBlocker (
                    "$DirectoryOwner contains a descendant reparse point: $entry")
            }
            if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
                $queue.Enqueue($entry)
            }
        }
    }
}

function Assert-OnlyDefaultDataStream {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$PathOwner
    )
    try {
        $streams = @(Get-Item `
                -LiteralPath $Path `
                -Stream * `
                -ErrorAction Stop)
    }
    catch {
        Throw-FinalizerBlocker (
            "$PathOwner stream inventory failed: $($_.Exception.Message)")
    }
    if (Test-Path -LiteralPath $Path -PathType Container) {
        if ($streams.Count -ne 0) {
            Throw-FinalizerBlocker "$PathOwner contains a directory alternate data stream."
        }
        return
    }
    if (($streams.Count -ne 1) -or
        ([string]$streams[0].Stream -cne ':$DATA')) {
        Throw-FinalizerBlocker "$PathOwner contains a non-default data stream."
    }
}

function Assert-OwnedStageCleanupInventory {
    param([Parameter(Mandatory = $true)]$Stage)
    $allowedNames = @(
        $OwnerMarkerName,
        $ClassesSnapshotName,
        $NetworksSnapshotName,
        $TranscriptName,
        $RawDeltaName,
        $RawManifestName,
        $ComparisonName,
        $CompleteManifestName)
    Assert-OnlyDefaultDataStream `
        -Path $Stage.path `
        -PathOwner 'owned staging cleanup directory'
    $entries = @([IO.Directory]::GetFileSystemEntries($Stage.path))
    foreach ($entry in $entries) {
        $attributes = [IO.File]::GetAttributes($entry)
        if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
            Throw-FinalizerBlocker (
                "owned staging cleanup contains a directory: $entry")
        }
        if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            Throw-FinalizerBlocker (
                "owned staging cleanup contains a reparse point: $entry")
        }
        $leaf = [IO.Path]::GetFileName($entry)
        $matches = @($allowedNames | Where-Object { $_ -ceq $leaf })
        if ($matches.Count -ne 1) {
            Throw-FinalizerBlocker (
                "owned staging cleanup contains an unknown file: $leaf")
        }
        Assert-OnlyDefaultDataStream `
            -Path $entry `
            -PathOwner "owned staging cleanup file $leaf"
    }
}

function Remove-ExactOwnedStageDirectory {
    param(
        [Parameter(Mandatory = $true)]$Stage,
        [string]$StageRoot = $PSScriptRoot
    )
    if ($null -eq $Stage -or
        -not (Test-Path -LiteralPath $Stage.path -PathType Container)) {
        return
    }
    $resolvedParent = Get-NormalizedFullPath -Path (Split-Path -Parent $Stage.path)
    $resolvedScriptRoot = Get-NormalizedFullPath -Path $StageRoot
    $leaf = Split-Path -Leaf $Stage.path
    if ((-not [string]::Equals(
                $resolvedParent,
                $resolvedScriptRoot,
                [StringComparison]::OrdinalIgnoreCase)) -or
        (-not $leaf.StartsWith(
                '.finalize-stage-',
                [StringComparison]::Ordinal)) -or
        ($leaf -cne [string]$Stage.name)) {
        Throw-FinalizerBlocker (
            'refusing cleanup because staging path ownership is ambiguous.')
    }
    Assert-NoReparsePointChain `
        -Root $resolvedScriptRoot `
        -Path $Stage.path `
        -IncludeLeaf `
        -PathOwner 'owned staging cleanup target'
    [void](Assert-OwnedStageMarkerContract -Stage $Stage)
    Assert-NoReparsePointDescendants `
        -Directory $Stage.path `
        -DirectoryOwner 'owned staging cleanup target'
    Assert-OwnedStageCleanupInventory -Stage $Stage
    [IO.Directory]::Delete($Stage.path, $true)
}

function Get-PowerShellExecutable {
    $candidate = if ($PSVersionTable.PSEdition -eq 'Core') {
        Join-Path $PSHOME 'pwsh.exe'
    }
    else {
        Join-Path $PSHOME 'powershell.exe'
    }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        Throw-FinalizerBlocker "current PowerShell executable is missing: $candidate"
    }
    return $candidate
}

function Invoke-IsolatedPowerShellTool {
    param(
        [Parameter(Mandatory = $true)][string]$ScriptPath,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ToolOwner
    )
    $executable = Get-PowerShellExecutable
    $nativeArguments = @(
        '-NoLogo',
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $ScriptPath
    ) + $Arguments
    $hasNativePreference = Test-Path Variable:PSNativeCommandUseErrorActionPreference
    $savedNativePreference = $null
    if ($hasNativePreference) {
        $savedNativePreference = $PSNativeCommandUseErrorActionPreference
        $PSNativeCommandUseErrorActionPreference = $false
    }
    try {
        $output = @(& $executable @nativeArguments 2>&1)
        $exitCode = $LASTEXITCODE
    }
    finally {
        if ($hasNativePreference) {
            $PSNativeCommandUseErrorActionPreference = $savedNativePreference
        }
    }
    return [pscustomobject]@{
        owner = $ToolOwner
        executable = $executable
        exitCode = [int]$exitCode
        outputLines = @($output | ForEach-Object { [string]$_ })
        outputText = (($output | ForEach-Object { [string]$_ }) -join "`n")
    }
}

function Assert-LogSnapshotUnchanged {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][long]$ExpectedBytes,
        [Parameter(Mandatory = $true)][string]$ExpectedSha256,
        [Parameter(Mandatory = $true)][string]$Phase
    )
    $logVolumeRoot = [IO.Path]::GetPathRoot((Get-NormalizedFullPath -Path $Path))
    Assert-NoReparsePointChain `
        -Root $logVolumeRoot `
        -Path $Path `
        -IncludeLeaf `
        -PathOwner "Lasal2.log at $Phase"
    $identity = Get-FileIdentity -Path $Path -FileOwner "Lasal2.log at $Phase"
    if (($identity.bytes -ne $ExpectedBytes) -or
        ($identity.sha256 -cne $ExpectedSha256)) {
        Throw-FinalizerBlocker (
            "Lasal2.log changed after the frozen end-offset snapshot at $Phase; " +
            'tail append and in-place modification are both forbidden.')
    }
}

function Assert-OutputIdentityUnchanged {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Expected,
        [Parameter(Mandatory = $true)][string]$OutputOwner
    )
    $actual = Get-FileIdentity -Path $Path -FileOwner $OutputOwner
    Assert-IdentityValue `
        -Actual $actual `
        -ExpectedBytes $Expected.bytes `
        -ExpectedSha256 $Expected.sha256 `
        -IdentityOwner $OutputOwner
    return $actual
}

function Assert-ConverterManifestContract {
    param(
        [Parameter(Mandatory = $true)]$Manifest,
        [Parameter(Mandatory = $true)][long]$FrozenLogEndOffset,
        [Parameter(Mandatory = $true)]$ClassesIdentity,
        [Parameter(Mandatory = $true)]$NetworksIdentity
    )
    if (($Manifest.Schema -cne 'LasalC78BoundedLogDelta/v1') -or
        ($Manifest.EvidenceProfile -cne 'GateDVisualLayout') -or
        ([long]$Manifest.SourceEndOffset -ne $FrozenLogEndOffset) -or
        ($Manifest.TranscriptFileName -cne $TranscriptName) -or
        ($Manifest.RawDeltaFileName -cne $RawDeltaName)) {
        Throw-FinalizerBlocker 'converter manifest header/file/end-offset contract differs.'
    }
    $outputs = @($Manifest.RegeneratedOutputs)
    if ($outputs.Count -ne 2) {
        Throw-FinalizerBlocker (
            "converter regenerated-output count is $($outputs.Count), expected 2.")
    }
    $expectations = @(
        [ordered]@{
            relativePath = $ClassesRelativePath
            identity = $ClassesIdentity
        },
        [ordered]@{
            relativePath = $NetworksRelativePath
            identity = $NetworksIdentity
        }
    )
    for ($index = 0; $index -lt $expectations.Count; $index++) {
        $actual = $outputs[$index]
        $expected = $expectations[$index]
        if (([string]$actual.RelativePath -cne $expected.relativePath) -or
            ([long]$actual.Bytes -ne [long]$expected.identity.bytes) -or
            ([string]$actual.Sha256).ToUpperInvariant() -cne
                [string]$expected.identity.sha256) {
            Throw-FinalizerBlocker (
                "converter regenerated-output identity differs at index $index.")
        }
    }
}

function Test-JsonStringProperty {
    param($Object, [string]$Name)
    if ($null -eq $Object) { return $false }
    $property = $Object.PSObject.Properties[$Name]
    return ($null -ne $property -and $property.Value -is [string])
}

function Test-JsonBooleanProperty {
    param($Object, [string]$Name)
    if ($null -eq $Object) { return $false }
    $property = $Object.PSObject.Properties[$Name]
    return ($null -ne $property -and $property.Value -is [bool])
}

function Test-JsonIntegerProperty {
    param($Object, [string]$Name)
    if ($null -eq $Object) { return $false }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $false }
    return ($property.Value -is [int] -or $property.Value -is [long])
}

function Test-ExactStringSequence {
    param($Actual, [Parameter(Mandatory = $true)][string[]]$Expected)
    $actualItems = @($Actual)
    if ($actualItems.Count -ne $Expected.Count) { return $false }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if (($actualItems[$index] -isnot [string]) -or
            ([string]$actualItems[$index] -cne $Expected[$index])) {
            return $false
        }
    }
    return $true
}

function Test-RecordIdentityOracle {
    param(
        [Parameter(Mandatory = $true)]$Identity,
        [Parameter(Mandatory = $true)]$Oracle
    )
    foreach ($integerName in @(
            'startOffset',
            'endOffsetExclusive',
            'sourceOffset',
            'bytes')) {
        if (-not (Test-JsonIntegerProperty -Object $Identity -Name $integerName)) {
            return $false
        }
    }
    if ((-not (Test-JsonStringProperty -Object $Identity -Name 'sha256')) -or
        ([long]$Identity.startOffset -ne [long]$Oracle.startOffset) -or
        ([long]$Identity.endOffsetExclusive -ne [long]$Oracle.endOffsetExclusive) -or
        ([long]$Identity.sourceOffset -ne [long]$Oracle.sourceOffset) -or
        ([long]$Identity.bytes -ne [long]$Oracle.bytes) -or
        ([string]$Identity.sha256 -cne [string]$Oracle.sha256)) {
        return $false
    }
    return $true
}

function Test-ExactComparisonRecordSet {
    param(
        [Parameter(Mandatory = $true)]$Container,
        [Parameter(Mandatory = $true)][object[]]$ExpectedRecords
    )
    if (($null -eq $Container) -or
        (-not ($Container.PSObject.Properties.Name -contains 'records')) -or
        (-not (Test-JsonBooleanProperty -Object $Container -Name 'allEqual')) -or
        (-not [bool]$Container.allEqual)) {
        return $false
    }
    $records = @($Container.records)
    if ($records.Count -ne $ExpectedRecords.Count) { return $false }
    for ($index = 0; $index -lt $ExpectedRecords.Count; $index++) {
        $record = $records[$index]
        $oracle = $ExpectedRecords[$index]
        if (($null -eq $record) -or
            (-not ($record.PSObject.Properties.Name -contains 'checkpoint')) -or
            (-not ($record.PSObject.Properties.Name -contains 'candidate')) -or
            (-not (Test-JsonStringProperty -Object $record -Name 'owner')) -or
            ([string]$record.owner -cne [string]$oracle.owner) -or
            (-not (Test-JsonStringProperty -Object $record -Name 'sourcePath')) -or
            ([string]$record.sourcePath -cne [string]$oracle.sourcePath) -or
            (-not (Test-JsonStringProperty -Object $record -Name 'parser')) -or
            ([string]$record.parser -cne [string]$oracle.parser) -or
            (-not (Test-JsonBooleanProperty -Object $record -Name 'exact')) -or
            (-not [bool]$record.exact) -or
            (-not (Test-RecordIdentityOracle `
                    -Identity $record.checkpoint -Oracle $oracle)) -or
            (-not (Test-RecordIdentityOracle `
                    -Identity $record.candidate -Oracle $oracle))) {
            return $false
        }
        if ($oracle.PSObject.Properties.Name -contains 'legacyWindowExact') {
            if ((-not (Test-JsonBooleanProperty `
                        -Object $record -Name 'legacyWindowExact')) -or
                (-not [bool]$record.legacyWindowExact)) {
                return $false
            }
        }
    }
    return $true
}

function Test-RecordParserOracle {
    param([Parameter(Mandatory = $true)]$Parser)
    if (($null -eq $Parser) -or
        (-not ($Parser.PSObject.Properties.Name -contains 'headerSourceInventory')) -or
        (-not ($Parser.PSObject.Properties.Name -contains 'firstSpecialRecord')) -or
        (-not (Test-JsonStringProperty -Object $Parser -Name 'convention')) -or
        ([string]$Parser.convention -cne
            'first-special-record-then-aa03-header-to-next-header-or-eof') -or
        (-not (Test-JsonBooleanProperty `
                -Object $Parser -Name 'latin1ByteOffsetPreserving')) -or
        (-not [bool]$Parser.latin1ByteOffsetPreserving) -or
        (-not (Test-JsonStringProperty -Object $Parser -Name 'sourceMarkerBoundary')) -or
        ([string]$Parser.sourceMarkerBoundary -cne 'path-length-le24-plus-aa') -or
        (-not (Test-JsonStringProperty -Object $Parser -Name 'trueHeaderBoundary')) -or
        ([string]$Parser.trueHeaderBoundary -cne
            'aa-03-plus-class-name-length-le24-plus-aa-plus-class-name') -or
        (-not (Test-JsonBooleanProperty `
                -Object $Parser -Name 'sourcePathSegmentsDiagnosticOnly')) -or
        (-not [bool]$Parser.sourcePathSegmentsDiagnosticOnly) -or
        (-not (Test-JsonIntegerProperty `
                -Object $Parser -Name 'checkpointOwnerRecordCount')) -or
        ([long]$Parser.checkpointOwnerRecordCount -ne 120L) -or
        (-not (Test-JsonIntegerProperty `
                -Object $Parser -Name 'candidateOwnerRecordCount')) -or
        ([long]$Parser.candidateOwnerRecordCount -ne 120L)) {
        return $false
    }
    $header = $Parser.headerSourceInventory
    if (($null -eq $header) -or
        (-not ($header.PSObject.Properties.Name -contains 'firstMismatch')) -or
        ($null -ne $header.firstMismatch) -or
        (-not ($header.PSObject.Properties.Name -contains 'comparedFields')) -or
        (-not (Test-JsonBooleanProperty -Object $header -Name 'exact')) -or
        (-not [bool]$header.exact) -or
        (-not (Test-JsonIntegerProperty -Object $header -Name 'checkpointCount')) -or
        ([long]$header.checkpointCount -ne 120L) -or
        (-not (Test-JsonIntegerProperty -Object $header -Name 'candidateCount')) -or
        ([long]$header.candidateCount -ne 120L) -or
        (-not (Test-ExactStringSequence `
                -Actual $header.comparedFields `
                -Expected $HeaderComparedFieldsOracle))) {
        return $false
    }
    $first = $Parser.firstSpecialRecord
    if (($null -eq $first) -or
        (-not ($first.PSObject.Properties.Name -contains 'checkpoint')) -or
        (-not ($first.PSObject.Properties.Name -contains 'candidate')) -or
        (-not (Test-JsonStringProperty -Object $first -Name 'owner')) -or
        ([string]$first.owner -cne [string]$FirstSpecialRecordOracle.owner) -or
        (-not (Test-JsonStringProperty -Object $first -Name 'sourcePath')) -or
        ([string]$first.sourcePath -cne
            [string]$FirstSpecialRecordOracle.sourcePath) -or
        (-not (Test-JsonStringProperty -Object $first -Name 'parser')) -or
        ([string]$first.parser -cne [string]$FirstSpecialRecordOracle.parser) -or
        (-not (Test-JsonBooleanProperty -Object $first -Name 'exact')) -or
        (-not [bool]$first.exact) -or
        (-not (Test-RecordIdentityOracle `
                -Identity $first.checkpoint -Oracle $FirstSpecialRecordOracle)) -or
        (-not (Test-RecordIdentityOracle `
                -Identity $first.candidate -Oracle $FirstSpecialRecordOracle))) {
        return $false
    }
    return $true
}

function Test-ExactComparisonOracle {
    param(
        [Parameter(Mandatory = $true)]$Comparison,
        [switch]$RequireByteExact
    )
    foreach ($booleanName in @(
            'byteExact',
            'equalLength',
            'changedByteCountDefined',
            'changedOwnersAreFrozenOpaqueSubset',
            'proprietaryFieldSemanticsDecoded')) {
        if (-not (Test-JsonBooleanProperty `
                    -Object $Comparison -Name $booleanName)) {
            return $false
        }
    }
    foreach ($integerName in @(
            'lengthDelta',
            'changedByteCount',
            'contiguousRunCount',
            'checkpointChangedOwnerCount',
            'unmappedRunCount',
            'frozenOpaqueOwnerCount')) {
        if (-not (Test-JsonIntegerProperty `
                    -Object $Comparison -Name $integerName)) {
            return $false
        }
    }
    $expectedChangedBytes = if ($RequireByteExact) { 0L } else { 99L }
    $expectedRuns = if ($RequireByteExact) { 0L } else { 58L }
    $expectedChangedOwners = if ($RequireByteExact) { 0L } else { 36L }
    if (([bool]$Comparison.byteExact -ne [bool]$RequireByteExact) -or
        (-not [bool]$Comparison.equalLength) -or
        ([long]$Comparison.lengthDelta -ne 0L) -or
        (-not (Test-JsonStringProperty -Object $Comparison -Name 'alignment')) -or
        ([string]$Comparison.alignment -cne 'equal-length-indexed') -or
        (-not [bool]$Comparison.changedByteCountDefined) -or
        ([long]$Comparison.changedByteCount -ne $expectedChangedBytes) -or
        ([long]$Comparison.contiguousRunCount -ne $expectedRuns) -or
        ([long]$Comparison.checkpointChangedOwnerCount -ne $expectedChangedOwners) -or
        ([long]$Comparison.unmappedRunCount -ne 0L) -or
        (-not [bool]$Comparison.changedOwnersAreFrozenOpaqueSubset) -or
        ([long]$Comparison.frozenOpaqueOwnerCount -ne 36L) -or
        (-not ($Comparison.PSObject.Properties.Name -contains 'frozenOpaqueOwners')) -or
        (-not (Test-ExactStringSequence `
                -Actual $Comparison.frozenOpaqueOwners `
                -Expected $FrozenOpaqueOwnersOracle)) -or
        [bool]$Comparison.proprietaryFieldSemanticsDecoded) {
        return $false
    }
    return $true
}

function Test-JsonDeepExact {
    param($Actual, $Expected)
    if (($null -eq $Actual) -or ($null -eq $Expected)) { return $false }
    try {
        $actualJson = $Actual | ConvertTo-Json -Depth 100 -Compress
        $expectedJson = $Expected | ConvertTo-Json -Depth 100 -Compress
        return ([string]$actualJson -ceq [string]$expectedJson)
    }
    catch {
        return $false
    }
}

function Test-ExactJsonKeySequence {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string[]]$Expected
    )
    if ($null -eq $Object) { return $false }
    $actual = @($Object.PSObject.Properties | ForEach-Object { $_.Name })
    return (Test-ExactStringSequence -Actual $actual -Expected $Expected)
}

function Copy-JsonObject {
    param([Parameter(Mandatory = $true)]$Value)
    return (($Value | ConvertTo-Json -Depth 100 -Compress) | ConvertFrom-Json)
}

function New-ExpectedComparatorReport {
    param(
        [Parameter(Mandatory = $true)]$ComparisonOracle,
        [Parameter(Mandatory = $true)]$ClassesIdentity,
        [Parameter(Mandatory = $true)][string]$ExpectedCandidatePath,
        [switch]$ExactCheckpoint
    )
    $expected = Copy-JsonObject -Value $ComparisonOracle
    $expected.candidate.path = $ExpectedCandidatePath
    $expected.candidate.rawBytes = [long]$ClassesIdentity.bytes
    $expected.candidate.sha256 = [string]$ClassesIdentity.sha256
    if ($ExactCheckpoint) {
        $expected.decision = [pscustomobject][ordered]@{
            disposition = 'EXACT_CHECKPOINT_MATCH'
            checkpointIdentityAccepted = $true
            approvalScope = 'checkpoint-byte-identity-only'
            productionApproved = $false
            exactCheckpointMatch = $true
            semanticEquivalenceProven = $true
            recordEqualityCannotApproveArtifact = $true
            exitCode = 0
        }
        $expected.comparison = [pscustomobject][ordered]@{
            byteExact = $true
            equalLength = $true
            lengthDelta = 0L
            alignment = 'equal-length-indexed'
            changedByteCountDefined = $true
            changedByteCount = 0L
            contiguousRunCount = 0L
            checkpointChangedOwnerCount = 0L
            unmappedRunCount = 0L
            changedOwnersAreFrozenOpaqueSubset = $true
            frozenOpaqueOwnerCount = 36L
            frozenOpaqueOwners = @($FrozenOpaqueOwnersOracle)
            proprietaryFieldSemanticsDecoded = $false
        }
        $expected.changedCheckpointOwners = @()
        $expected.diffRuns = @()
    }
    return $expected
}

function Test-StrictThirdComparisonContract {
    param([Parameter(Mandatory = $true)]$Comparison)
    foreach ($booleanName in @(
            'byteExact',
            'equalLength',
            'changedByteCountDefined',
            'changedOwnersAreFrozenOpaqueSubset',
            'proprietaryFieldSemanticsDecoded')) {
        if (-not (Test-JsonBooleanProperty `
                    -Object $Comparison -Name $booleanName)) {
            return $false
        }
    }
    foreach ($integerName in @(
            'lengthDelta',
            'contiguousRunCount',
            'checkpointChangedOwnerCount',
            'unmappedRunCount',
            'frozenOpaqueOwnerCount')) {
        if (-not (Test-JsonIntegerProperty `
                    -Object $Comparison -Name $integerName)) {
            return $false
        }
    }
    $changedByteCountProperty = $Comparison.PSObject.Properties['changedByteCount']
    if ($null -eq $changedByteCountProperty) { return $false }
    if ([bool]$Comparison.changedByteCountDefined) {
        if (-not (Test-JsonIntegerProperty `
                    -Object $Comparison -Name 'changedByteCount')) {
            return $false
        }
    }
    elseif ($null -ne $changedByteCountProperty.Value) {
        return $false
    }
    if ((-not (Test-JsonStringProperty -Object $Comparison -Name 'alignment')) -or
        (-not (Test-ExactStringSequence `
                -Actual $Comparison.frozenOpaqueOwners `
                -Expected $FrozenOpaqueOwnersOracle)) -or
        ([long]$Comparison.frozenOpaqueOwnerCount -ne 36L) -or
        [bool]$Comparison.proprietaryFieldSemanticsDecoded) {
        return $false
    }
    return $true
}

function Assert-ComparatorCommonContract {
    param(
        [Parameter(Mandatory = $true)]$Report,
        [Parameter(Mandatory = $true)][int]$ProcessExitCode,
        [Parameter(Mandatory = $true)]$ClassesIdentity,
        [Parameter(Mandatory = $true)][string]$ExpectedCandidatePath
    )
    $topKeys = @(
        'schema',
        'decision',
        'checkpoint',
        'candidate',
        'comparison',
        'recordParser',
        'gateDTargetRecords',
        'protectedDependencyRecords',
        'changedCheckpointOwners',
        'diffRuns')
    $decisionKeys = @(
        'disposition',
        'checkpointIdentityAccepted',
        'approvalScope',
        'productionApproved',
        'exactCheckpointMatch',
        'semanticEquivalenceProven',
        'recordEqualityCannotApproveArtifact',
        'exitCode')
    $checkpointKeys = @(
        'requested',
        'kind',
        'resolvedRevision',
        'relativePath',
        'blobOid',
        'rawBytes',
        'sha256')
    $candidateKeys = @('path', 'rawBytes', 'sha256')
    $comparisonKeys = @(
        'byteExact',
        'equalLength',
        'lengthDelta',
        'alignment',
        'changedByteCountDefined',
        'changedByteCount',
        'contiguousRunCount',
        'checkpointChangedOwnerCount',
        'unmappedRunCount',
        'changedOwnersAreFrozenOpaqueSubset',
        'frozenOpaqueOwnerCount',
        'frozenOpaqueOwners',
        'proprietaryFieldSemanticsDecoded')
    if (($null -eq $Report) -or
        (-not ($Report.PSObject.Properties.Name -contains 'decision')) -or
        (-not ($Report.PSObject.Properties.Name -contains 'checkpoint')) -or
        (-not ($Report.PSObject.Properties.Name -contains 'candidate')) -or
        (-not ($Report.PSObject.Properties.Name -contains 'comparison'))) {
        Throw-FinalizerBlocker 'comparator report top-level contract is incomplete.'
    }
    if ((-not (Test-ExactJsonKeySequence -Object $Report -Expected $topKeys)) -or
        (-not (Test-ExactJsonKeySequence `
                -Object $Report.decision -Expected $decisionKeys)) -or
        (-not (Test-ExactJsonKeySequence `
                -Object $Report.checkpoint -Expected $checkpointKeys)) -or
        (-not (Test-ExactJsonKeySequence `
                -Object $Report.candidate -Expected $candidateKeys)) -or
        (-not (Test-ExactJsonKeySequence `
                -Object $Report.comparison -Expected $comparisonKeys))) {
        Throw-FinalizerBlocker 'comparator report exact case-sensitive key contract differs.'
    }
    foreach ($booleanName in @(
            'checkpointIdentityAccepted',
            'productionApproved',
            'exactCheckpointMatch',
            'semanticEquivalenceProven',
            'recordEqualityCannotApproveArtifact')) {
        if (-not (Test-JsonBooleanProperty `
                    -Object $Report.decision -Name $booleanName)) {
            Throw-FinalizerBlocker (
                "comparator decision boolean type differs: $booleanName")
        }
    }
    if ((-not (Test-JsonStringProperty -Object $Report -Name 'schema')) -or
        (-not (Test-JsonStringProperty -Object $Report.decision -Name 'disposition')) -or
        (-not (Test-JsonStringProperty -Object $Report.decision -Name 'approvalScope')) -or
        (-not (Test-JsonIntegerProperty -Object $Report.decision -Name 'exitCode')) -or
        (-not (Test-JsonBooleanProperty -Object $Report.comparison -Name 'byteExact')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'requested')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'kind')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'resolvedRevision')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'relativePath')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'blobOid')) -or
        (-not (Test-JsonIntegerProperty -Object $Report.checkpoint -Name 'rawBytes')) -or
        (-not (Test-JsonStringProperty -Object $Report.checkpoint -Name 'sha256')) -or
        (-not (Test-JsonStringProperty -Object $Report.candidate -Name 'path')) -or
        (-not (Test-JsonIntegerProperty -Object $Report.candidate -Name 'rawBytes')) -or
        (-not (Test-JsonStringProperty -Object $Report.candidate -Name 'sha256'))) {
        Throw-FinalizerBlocker 'comparator report field types differ.'
    }
    if (($Report.schema -cne 'LasalClassesArtifactComparison/v1') -or
        ([int]$Report.decision.exitCode -ne $ProcessExitCode) -or
        ($Report.decision.approvalScope -cne 'checkpoint-byte-identity-only') -or
        [bool]$Report.decision.productionApproved -or
        (-not [bool]$Report.decision.recordEqualityCannotApproveArtifact) -or
        ([string]$Report.checkpoint.requested -cne $CheckpointCommit) -or
        ([string]$Report.checkpoint.kind -cne 'revision') -or
        ([string]$Report.checkpoint.resolvedRevision -cne $CheckpointCommit) -or
        ([string]$Report.checkpoint.relativePath -cne $ClassesRelativePath) -or
        ([string]$Report.checkpoint.blobOid -cne $CheckpointClassesBlobOid) -or
        ([long]$Report.checkpoint.rawBytes -ne $CheckpointClassesBytes) -or
        ([string]$Report.checkpoint.sha256 -cne $CheckpointClassesSha256) -or
        ([string]$Report.candidate.path -cne $ExpectedCandidatePath) -or
        ([long]$Report.candidate.rawBytes -ne [long]$ClassesIdentity.bytes) -or
        ([string]$Report.candidate.sha256 -cne [string]$ClassesIdentity.sha256)) {
        Throw-FinalizerBlocker 'comparator report common contract differs.'
    }
    if ($ProcessExitCode -notin @(0, 2, 3)) {
        Throw-FinalizerBlocker (
            "comparator exit $ProcessExitCode is blocked; exit 4 and unknown exits " +
            'cannot publish evidence.')
    }
}

function Get-FinalDecision {
    param(
        [Parameter(Mandatory = $true)]$Report,
        [Parameter(Mandatory = $true)][int]$ComparatorExitCode,
        [Parameter(Mandatory = $true)]$ClassesIdentity,
        [Parameter(Mandatory = $true)]$NetworksIdentity,
        [Parameter(Mandatory = $true)][string]$ExpectedCandidatePath,
        [Parameter(Mandatory = $true)]$ComparisonOracle
    )
    Assert-ComparatorCommonContract `
        -Report $Report `
        -ProcessExitCode $ComparatorExitCode `
        -ClassesIdentity $ClassesIdentity `
        -ExpectedCandidatePath $ExpectedCandidatePath
    $classesSha = [string]$ClassesIdentity.sha256
    if ([string]$NetworksIdentity.sha256 -cne $KnownNetworksSha256) {
        Throw-FinalizerBlocker (
            'Networks regenerated output differs from the exact C307547E ' +
            'invariant; no Classes decision bundle may be published.')
    }
    if ($classesSha -ceq $CheckpointClassesSha256) {
        $expectedExactReport = New-ExpectedComparatorReport `
            -ComparisonOracle $ComparisonOracle `
            -ClassesIdentity $ClassesIdentity `
            -ExpectedCandidatePath $ExpectedCandidatePath `
            -ExactCheckpoint
        $hasExactSections =
            ($Report.PSObject.Properties.Name -contains 'recordParser') -and
            ($Report.PSObject.Properties.Name -contains 'gateDTargetRecords') -and
            ($Report.PSObject.Properties.Name -contains 'protectedDependencyRecords') -and
            ($Report.PSObject.Properties.Name -contains 'changedCheckpointOwners') -and
            ($Report.PSObject.Properties.Name -contains 'diffRuns')
        if (($ComparatorExitCode -ne 0) -or
            ($Report.decision.disposition -cne 'EXACT_CHECKPOINT_MATCH') -or
            (-not [bool]$Report.decision.exactCheckpointMatch) -or
            (-not [bool]$Report.decision.checkpointIdentityAccepted) -or
            (-not [bool]$Report.comparison.byteExact) -or
            (-not [bool]$Report.decision.semanticEquivalenceProven) -or
            (-not $hasExactSections) -or
            (-not (Test-ExactComparisonOracle `
                    -Comparison $Report.comparison -RequireByteExact)) -or
            (-not (Test-RecordParserOracle -Parser $Report.recordParser)) -or
            (-not (Test-ExactComparisonRecordSet `
                    -Container $Report.gateDTargetRecords `
                    -ExpectedRecords $GateDTargetRecordOracles)) -or
            (-not (Test-ExactComparisonRecordSet `
                    -Container $Report.protectedDependencyRecords `
                    -ExpectedRecords $ProtectedDependencyRecordOracles)) -or
            (-not (Test-JsonDeepExact `
                    -Actual $Report.recordParser `
                    -Expected $ComparisonOracle.recordParser)) -or
            (-not (Test-JsonDeepExact `
                    -Actual $Report.gateDTargetRecords `
                    -Expected $ComparisonOracle.gateDTargetRecords)) -or
            (-not (Test-JsonDeepExact `
                    -Actual $Report.protectedDependencyRecords `
                    -Expected $ComparisonOracle.protectedDependencyRecords)) -or
            ($Report.changedCheckpointOwners -isnot [Array]) -or
            ($Report.diffRuns -isnot [Array]) -or
            (@($Report.changedCheckpointOwners).Count -ne 0) -or
            (@($Report.diffRuns).Count -ne 0) -or
            (-not (Test-JsonDeepExact `
                    -Actual $Report -Expected $expectedExactReport))) {
            Throw-FinalizerBlocker '24402BFA candidate lacks exact comparator exit-0 proof.'
        }
        return [ordered]@{
            disposition = 'CHECKPOINT_24402BFA_REPRODUCED_STATIC_REPLAY_ONLY'
            exitCode = 0
            checkpointReproduced = $true
            known6EReproduced = $false
            productionApproved = $false
            semanticEquivalenceClaimedForOpaqueDrift = $false
            staticReplayPermitted = $true
            onlineRuntimeQualificationPermitted = $false
        }
    }
    if ($classesSha -ceq $KnownRebuiltClassesSha256) {
        $expectedKnownReport = New-ExpectedComparatorReport `
            -ComparisonOracle $ComparisonOracle `
            -ClassesIdentity $ClassesIdentity `
            -ExpectedCandidatePath $ExpectedCandidatePath
        $hasKnownSections =
            ($Report.PSObject.Properties.Name -contains 'recordParser') -and
            ($Report.PSObject.Properties.Name -contains 'gateDTargetRecords') -and
            ($Report.PSObject.Properties.Name -contains 'protectedDependencyRecords') -and
            ($Report.PSObject.Properties.Name -contains 'changedCheckpointOwners') -and
            ($Report.PSObject.Properties.Name -contains 'diffRuns')
        $opaqueContract =
            ($ComparatorExitCode -eq 2) -and
            ($Report.decision.disposition -ceq
                'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT') -and
            (-not [bool]$Report.decision.checkpointIdentityAccepted) -and
            (-not [bool]$Report.decision.exactCheckpointMatch) -and
            (-not [bool]$Report.decision.semanticEquivalenceProven) -and
            (-not [bool]$Report.comparison.byteExact) -and
            $hasKnownSections -and
            (Test-ExactComparisonOracle -Comparison $Report.comparison) -and
            (Test-RecordParserOracle -Parser $Report.recordParser) -and
            (Test-ExactComparisonRecordSet `
                -Container $Report.gateDTargetRecords `
                -ExpectedRecords $GateDTargetRecordOracles) -and
            (Test-ExactComparisonRecordSet `
                -Container $Report.protectedDependencyRecords `
                -ExpectedRecords $ProtectedDependencyRecordOracles) -and
            (Test-JsonDeepExact `
                -Actual $Report.decision -Expected $ComparisonOracle.decision) -and
            (Test-JsonDeepExact `
                -Actual $Report.comparison -Expected $ComparisonOracle.comparison) -and
            (Test-JsonDeepExact `
                -Actual $Report.recordParser -Expected $ComparisonOracle.recordParser) -and
            (Test-JsonDeepExact `
                -Actual $Report.gateDTargetRecords `
                -Expected $ComparisonOracle.gateDTargetRecords) -and
            (Test-JsonDeepExact `
                -Actual $Report.protectedDependencyRecords `
                -Expected $ComparisonOracle.protectedDependencyRecords) -and
            (Test-JsonDeepExact `
                -Actual $Report.changedCheckpointOwners `
                -Expected $ComparisonOracle.changedCheckpointOwners) -and
            (Test-JsonDeepExact `
                -Actual $Report.diffRuns -Expected $ComparisonOracle.diffRuns) -and
            (Test-JsonDeepExact `
                -Actual $Report -Expected $expectedKnownReport)
        if (-not $opaqueContract) {
            Throw-FinalizerBlocker '6E115876 candidate lacks the exact exit-2 99/58/36 contract.'
        }
        return [ordered]@{
            disposition = 'KNOWN_6E115876_REPRODUCIBLE_REVIEW_ONLY'
            exitCode = 2
            checkpointReproduced = $false
            known6EReproduced = $true
            productionApproved = $false
            semanticEquivalenceClaimedForOpaqueDrift = $false
            staticReplayPermitted = $false
            onlineRuntimeQualificationPermitted = $false
        }
    }
    if (($Report.changedCheckpointOwners -isnot [Array]) -or
        ($Report.diffRuns -isnot [Array]) -or
        (-not (Test-StrictThirdComparisonContract `
                -Comparison $Report.comparison)) -or
        ($ComparatorExitCode -eq 0) -or
        [bool]$Report.decision.checkpointIdentityAccepted -or
        [bool]$Report.decision.exactCheckpointMatch -or
        [bool]$Report.decision.semanticEquivalenceProven -or
        [bool]$Report.decision.productionApproved -or
        [bool]$Report.comparison.byteExact) {
        Throw-FinalizerBlocker 'third Classes hash acquired an impossible approval signal.'
    }
    if (($ComparatorExitCode -eq 2 -and
            $Report.decision.disposition -cne
                'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT') -or
        ($ComparatorExitCode -eq 3 -and
            $Report.decision.disposition -cne
                'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT')) {
        Throw-FinalizerBlocker 'third Classes hash comparator exit/disposition differs.'
    }
    return [ordered]@{
        disposition = 'UNSTABLE_THIRD_CLASSES_HASH_STOP'
        exitCode = 3
        checkpointReproduced = $false
        known6EReproduced = $false
        productionApproved = $false
        semanticEquivalenceClaimedForOpaqueDrift = $false
        staticReplayPermitted = $false
        onlineRuntimeQualificationPermitted = $false
    }
}

function Resolve-RepositoryContext {
    param([AllowEmptyString()][string]$RequestedRoot)
    $scriptBoundRoot = Get-NormalizedFullPath -Path (
        Join-Path $PSScriptRoot '..\..\..')
    $root = $scriptBoundRoot
    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        $root = Get-NormalizedFullPath -Path $RequestedRoot
    }
    if (-not [string]::Equals(
            $root,
            $scriptBoundRoot,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-FinalizerBlocker 'RepositoryRoot differs from the script-bound repository.'
    }
    Assert-NoReparsePointChain `
        -Root $root `
        -Path $root `
        -IncludeLeaf `
        -PathOwner 'repository root'
    $gitRoot = Get-NormalizedFullPath -Path (Invoke-GitText `
            -Root $root `
            -Arguments @('rev-parse', '--show-toplevel') `
            -Operation 'repository root resolution')
    if (-not [string]::Equals(
            $root,
            $gitRoot,
            [StringComparison]::OrdinalIgnoreCase)) {
        Throw-FinalizerBlocker 'script-bound root differs from git toplevel.'
    }
    return $root
}

function Assert-TrustedArtifacts {
    param([Parameter(Mandatory = $true)][string]$Root)
    $reports = New-Object Collections.Generic.List[object]
    foreach ($definition in $TrustedArtifacts) {
        $path = Join-Path $Root $definition.relativePath
        Assert-NoReparsePointChain `
            -Root $Root `
            -Path $path `
            -IncludeLeaf `
            -PathOwner $definition.owner
        Assert-GitTrackedCleanPath `
            -Root $Root `
            -RelativePath $definition.relativePath `
            -PathOwner $definition.owner
        $identity = Get-FileIdentity `
            -Path $path `
            -FileOwner $definition.owner
        Assert-IdentityValue `
            -Actual $identity `
            -ExpectedBytes $definition.bytes `
            -ExpectedSha256 $definition.sha256 `
            -IdentityOwner $definition.owner
        $reports.Add([ordered]@{
                owner = $definition.owner
                relativePath = $definition.relativePath
                bytes = [long]$identity.bytes
                sha256 = [string]$identity.sha256
                gitTrackedAndHeadClean = $true
            })
    }
    return $reports.ToArray()
}

function Assert-BaselineAndInputIdentities {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)]$Baseline,
        [Parameter(Mandatory = $true)][string]$ResolvedLogPath
    )
    $canonicalProject = Get-NormalizedFullPath -Path (
        Join-Path $Root $CanonicalProjectRelativePath)
    if (($Baseline.Schema -cne 'LasalC78RebuildEvidence/v1') -or
        ($Baseline.EvidenceProfile -cne 'GateDVisualLayout') -or
        (-not [string]::Equals(
            (Get-NormalizedFullPath -Path ([string]$Baseline.RepositoryRoot)),
            $Root,
            [StringComparison]::OrdinalIgnoreCase)) -or
        (-not [string]::Equals(
            (Get-NormalizedFullPath -Path ([string]$Baseline.CanonicalProjectPath)),
            $canonicalProject,
            [StringComparison]::OrdinalIgnoreCase)) -or
        (-not [string]::Equals(
            (Get-NormalizedFullPath -Path ([string]$Baseline.LasalLogPath)),
            $ResolvedLogPath,
            [StringComparison]::OrdinalIgnoreCase)) -or
        ([long]$Baseline.LogPrefixLength -ne 8788633L) -or
        ([string]$Baseline.LogPrefixSha256).ToUpperInvariant() -cne
            '03F222F7F02E1466F86FDD6D91BB76DAC860CDC4E36674F42CF8A6A314B9AD56') {
        Throw-FinalizerBlocker 'rebuild baseline fixed contract differs.'
    }
    $inputFiles = @($Baseline.Files | Where-Object {
            $_.Role -ceq 'inputIdentity'
        })
    if ($inputFiles.Count -ne 10) {
        Throw-FinalizerBlocker (
            "baseline inputIdentity count is $($inputFiles.Count), expected 10.")
    }
    $reports = New-Object Collections.Generic.List[object]
    foreach ($inputFile in $inputFiles) {
        $relativePath = [string]$inputFile.RelativePath
        $fullPath = Join-Path $Root $relativePath
        Assert-NoReparsePointChain `
            -Root $Root `
            -Path $fullPath `
            -IncludeLeaf `
            -PathOwner "baseline input $relativePath"
        $identity = Get-FileIdentity `
            -Path $fullPath `
            -FileOwner "baseline input $relativePath"
        $expectedSha = [string]$inputFile.Sha256
        if ($inputFile.PSObject.Properties.Name -contains 'RawSha256') {
            $expectedSha = [string]$inputFile.RawSha256
        }
        if ([string]$identity.sha256 -cne $expectedSha.ToUpperInvariant()) {
            Throw-FinalizerBlocker (
                "current input identity differs: $relativePath")
        }
        if ($inputFile.PSObject.Properties.Name -contains 'RawBytes' -and
            [long]$identity.bytes -ne [long]$inputFile.RawBytes) {
            Throw-FinalizerBlocker (
                "current input byte count differs: $relativePath")
        }
        $reports.Add([ordered]@{
                relativePath = $relativePath
                bytes = [long]$identity.bytes
                sha256 = [string]$identity.sha256
                exactBaselineInputIdentity = $true
            })
    }
    return [pscustomobject]@{
        canonicalProjectPath = $canonicalProject
        inputIdentities = $reports.ToArray()
    }
}

function Get-StageArtifactReports {
    param(
        [Parameter(Mandatory = $true)][string]$StagePath,
        [Parameter(Mandatory = $true)][string[]]$FileNames,
        [string]$ArtifactRoot = $PSScriptRoot
    )
    $reports = New-Object Collections.Generic.List[object]
    foreach ($fileName in $FileNames) {
        $path = Join-Path $StagePath $fileName
        Assert-NoReparsePointChain `
            -Root $ArtifactRoot `
            -Path $path `
            -IncludeLeaf `
            -PathOwner "staging artifact $fileName"
        $identity = Get-FileIdentity `
            -Path $path `
            -FileOwner "staging artifact $fileName"
        $reports.Add([ordered]@{
                fileName = $fileName
                bytes = [long]$identity.bytes
                sha256 = [string]$identity.sha256
            })
    }
    return $reports.ToArray()
}

function Assert-ArtifactReportSequencesEqual {
    param(
        [Parameter(Mandatory = $true)][object[]]$Expected,
        [Parameter(Mandatory = $true)][object[]]$Actual,
        [Parameter(Mandatory = $true)][string]$SequenceOwner
    )
    if ($Expected.Count -ne $Actual.Count) {
        Throw-FinalizerBlocker "$SequenceOwner count changed."
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        if (([string]$Expected[$index].fileName -cne
                [string]$Actual[$index].fileName) -or
            ([long]$Expected[$index].bytes -ne
                [long]$Actual[$index].bytes) -or
            ([string]$Expected[$index].sha256 -cne
                [string]$Actual[$index].sha256)) {
            Throw-FinalizerBlocker (
                "$SequenceOwner differs at index $index.")
        }
    }
}

function Get-ExactNamedIdentityReportValue {
    param(
        [Parameter(Mandatory = $true)]$Item,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$SequenceOwner,
        [Parameter(Mandatory = $true)][int]$Index
    )
    if ($Item -is [Collections.IDictionary]) {
        $matchingKeys = @($Item.Keys | Where-Object {
                $_ -is [string] -and ([string]$_ -ceq $Name)
            })
        if ($matchingKeys.Count -ne 1) {
            Throw-FinalizerBlocker (
                "$SequenceOwner item $Index lacks exact field '$Name'.")
        }
        return $Item[$matchingKeys[0]]
    }

    $matchingProperties = @($Item.PSObject.Properties | Where-Object {
            $_.Name -ceq $Name
        })
    if ($matchingProperties.Count -ne 1) {
        Throw-FinalizerBlocker (
            "$SequenceOwner item $Index lacks exact field '$Name'.")
    }
    return $matchingProperties[0].Value
}

function Assert-NamedIdentityReportSequencesEqual {
    param(
        [Parameter(Mandatory = $true)][object[]]$Expected,
        [Parameter(Mandatory = $true)][object[]]$Actual,
        [Parameter(Mandatory = $true)][string]$NameProperty,
        [Parameter(Mandatory = $true)][string]$SequenceOwner
    )
    if ($Expected.Count -ne $Actual.Count) {
        Throw-FinalizerBlocker "$SequenceOwner count changed."
    }
    for ($index = 0; $index -lt $Expected.Count; $index++) {
        $expectedName = Get-ExactNamedIdentityReportValue `
            -Item $Expected[$index] `
            -Name $NameProperty `
            -SequenceOwner $SequenceOwner `
            -Index $index
        $actualName = Get-ExactNamedIdentityReportValue `
            -Item $Actual[$index] `
            -Name $NameProperty `
            -SequenceOwner $SequenceOwner `
            -Index $index
        $expectedBytes = Get-ExactNamedIdentityReportValue `
            -Item $Expected[$index] `
            -Name 'bytes' `
            -SequenceOwner $SequenceOwner `
            -Index $index
        $actualBytes = Get-ExactNamedIdentityReportValue `
            -Item $Actual[$index] `
            -Name 'bytes' `
            -SequenceOwner $SequenceOwner `
            -Index $index
        $expectedSha256 = Get-ExactNamedIdentityReportValue `
            -Item $Expected[$index] `
            -Name 'sha256' `
            -SequenceOwner $SequenceOwner `
            -Index $index
        $actualSha256 = Get-ExactNamedIdentityReportValue `
            -Item $Actual[$index] `
            -Name 'sha256' `
            -SequenceOwner $SequenceOwner `
            -Index $index
        if (([string]$expectedName -cne [string]$actualName) -or
            ([long]$expectedBytes -ne [long]$actualBytes) -or
            ([string]$expectedSha256 -cne [string]$actualSha256)) {
            Throw-FinalizerBlocker "$SequenceOwner differs at index $index."
        }
    }
}

function Get-ExactStageInventoryReports {
    param(
        [Parameter(Mandatory = $true)][string]$StagePath,
        [Parameter(Mandatory = $true)][string[]]$ExpectedFileNames,
        [string]$ArtifactRoot = $PSScriptRoot
    )
    if (($ExpectedFileNames | Select-Object -Unique).Count -ne
        $ExpectedFileNames.Count) {
        Throw-FinalizerBlocker 'expected stage inventory contains duplicate names.'
    }
    Assert-NoReparsePointChain `
        -Root $ArtifactRoot `
        -Path $StagePath `
        -IncludeLeaf `
        -PathOwner 'staging inventory root'
    Assert-NoReparsePointDescendants `
        -Directory $StagePath `
        -DirectoryOwner 'staging inventory'
    Assert-OnlyDefaultDataStream `
        -Path $StagePath `
        -PathOwner 'staging inventory directory'
    $entries = @([IO.Directory]::GetFileSystemEntries($StagePath))
    if ($entries.Count -ne $ExpectedFileNames.Count) {
        Throw-FinalizerBlocker (
            "staging inventory entry count is $($entries.Count), expected " +
            "$($ExpectedFileNames.Count).")
    }
    foreach ($entry in $entries) {
        $attributes = [IO.File]::GetAttributes($entry)
        if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
            Throw-FinalizerBlocker "staging inventory contains a directory: $entry"
        }
        $leaf = [IO.Path]::GetFileName($entry)
        Assert-OnlyDefaultDataStream `
            -Path $entry `
            -PathOwner "staging inventory file $leaf"
        $exactMatches = @($ExpectedFileNames | Where-Object { $_ -ceq $leaf })
        if ($exactMatches.Count -ne 1) {
            Throw-FinalizerBlocker "staging inventory contains an unexpected file: $leaf"
        }
    }
    return @(Get-StageArtifactReports `
            -StagePath $StagePath `
            -FileNames $ExpectedFileNames `
            -ArtifactRoot $ArtifactRoot)
}

function Invoke-CandidateFinalization {
    param(
        [AllowEmptyString()][string]$RequestedRepositoryRoot,
        [AllowEmptyString()][string]$RequestedLasalLogPath,
        [Parameter(Mandatory = $true)]$ProductionEngine
    )
    [void](Assert-PowerShell7FinalizationEngine `
            -Phase 'candidate finalization entry' `
            -Expected $ProductionEngine)
    $root = Resolve-RepositoryContext -RequestedRoot $RequestedRepositoryRoot
    $finalDirectory = Get-NormalizedFullPath -Path (
        Join-Path $PSScriptRoot $FinalDirectoryName)
    Assert-PublicationNamespaceAvailable -FinalDirectory $finalDirectory
    Assert-LasalStopped -Phase 'initial preflight'

    Assert-GitTrackedCleanPath `
        -Root $root `
        -RelativePath $FinalizerRelativePath `
        -PathOwner 'candidate finalizer'
    $initialHead = Invoke-GitText `
        -Root $root `
        -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
        -Operation 'initial HEAD resolution'
    $finalizerIdentity = Get-FileIdentity `
        -Path (Join-Path $root $FinalizerRelativePath) `
        -FileOwner 'candidate finalizer'
    $finalizerHeadBlobOid = Invoke-GitText `
        -Root $root `
        -Arguments @('rev-parse', '--verify', "HEAD:$FinalizerRelativePath") `
        -Operation 'candidate finalizer HEAD blob resolution'
    $trustedArtifactReports = @(Assert-TrustedArtifacts -Root $root)
    $comparisonOracle = Read-StrictJsonFile `
        -Path (Join-Path $root $ComparisonOracleRelativePath) `
        -FileOwner 'known 6E comparison oracle' `
        -RequireComparatorCanonicalRoundTrip
    $resolvedCheckpoint = Invoke-GitText `
        -Root $root `
        -Arguments @('rev-parse', '--verify', "$CheckpointCommit^{commit}") `
        -Operation 'full checkpoint commit resolution'
    if ($resolvedCheckpoint -cne $CheckpointCommit) {
        Throw-FinalizerBlocker 'full checkpoint commit does not resolve exactly.'
    }
    $baselinePath = Join-Path $root $BaselineRelativePath
    $baseline = Read-StrictJsonFile `
        -Path $baselinePath `
        -FileOwner 'rebuild baseline'
    $resolvedLogPath = $RequestedLasalLogPath
    if ([string]::IsNullOrWhiteSpace($resolvedLogPath)) {
        $resolvedLogPath = [string]$baseline.LasalLogPath
    }
    $resolvedLogPath = Get-NormalizedFullPath -Path $resolvedLogPath
    $logVolumeRoot = [IO.Path]::GetPathRoot($resolvedLogPath)
    Assert-NoReparsePointChain `
        -Root $logVolumeRoot `
        -Path $resolvedLogPath `
        -IncludeLeaf `
        -PathOwner 'Lasal2.log frozen source'
    $baselineInputs = Assert-BaselineAndInputIdentities `
        -Root $root `
        -Baseline $baseline `
        -ResolvedLogPath $resolvedLogPath

    $classesPath = Get-NormalizedFullPath -Path (
        Join-Path $root $ClassesRelativePath)
    $networksPath = Get-NormalizedFullPath -Path (
        Join-Path $root $NetworksRelativePath)
    Assert-NoReparsePointChain `
        -Root $root -Path $classesPath -IncludeLeaf -PathOwner 'Classes candidate'
    Assert-NoReparsePointChain `
        -Root $root -Path $networksPath -IncludeLeaf -PathOwner 'Networks candidate'
    $classesIdentity = Get-FileIdentity `
        -Path $classesPath -FileOwner 'Classes post-Rebuild candidate'
    $networksIdentity = Get-FileIdentity `
        -Path $networksPath -FileOwner 'Networks post-Rebuild candidate'

    [byte[]]$logBytes = Read-StableFileBytes `
        -Path $resolvedLogPath `
        -FileOwner 'Lasal2.log frozen snapshot'
    if ($logBytes.LongLength -gt [int]::MaxValue) {
        Throw-FinalizerBlocker 'Lasal2.log is too large for bounded conversion.'
    }
    $prefixLength = [long]$baseline.LogPrefixLength
    if ($logBytes.LongLength -le $prefixLength) {
        Throw-FinalizerBlocker 'Lasal2.log has no completed post-baseline session.'
    }
    $prefixSha = Get-ByteRangeSha256 `
        -Bytes $logBytes -Offset 0 -Count ([int]$prefixLength)
    if ($prefixSha -cne
        ([string]$baseline.LogPrefixSha256).ToUpperInvariant()) {
        Throw-FinalizerBlocker 'Lasal2.log committed baseline prefix differs.'
    }
    $logEndOffset = [long]$logBytes.LongLength
    $logSnapshotSha = Get-BytesSha256 -Bytes $logBytes
    try {
        $appendedText = $Utf8Strict.GetString(
            $logBytes,
            [int]$prefixLength,
            [int]($logEndOffset - $prefixLength))
    }
    catch {
        Throw-FinalizerBlocker (
            "post-baseline Lasal2.log is not strict UTF-8: $($_.Exception.Message)")
    }
    $sessionContract = Assert-IsolatedRebuildSessionContract `
        -AppendedLogText $appendedText `
        -CanonicalProjectPath $baselineInputs.canonicalProjectPath
    Assert-LasalStopped -Phase 'post-log-contract pre-write'
    Assert-LogSnapshotUnchanged `
        -Path $resolvedLogPath `
        -ExpectedBytes $logEndOffset `
        -ExpectedSha256 $logSnapshotSha `
        -Phase 'post-log-contract pre-write'
    [void](Assert-OutputIdentityUnchanged `
            -Path $classesPath `
            -Expected $classesIdentity `
            -OutputOwner 'Classes pre-write stability')
    [void](Assert-OutputIdentityUnchanged `
            -Path $networksPath `
            -Expected $networksIdentity `
            -OutputOwner 'Networks pre-write stability')

    $stage = $null
    $published = $false
    $ownerToken = [Guid]::NewGuid().ToString('N')
    try {
        $stage = New-OwnedStageDirectory -OwnerToken $ownerToken
        $stageMarkerIdentity = Assert-OwnedStageMarkerContract -Stage $stage
        $stageTranscript = Join-Path $stage.path $TranscriptName
        $stageRawDelta = Join-Path $stage.path $RawDeltaName
        $stageRawManifest = Join-Path $stage.path $RawManifestName
        $stageClassesSnapshot = Join-Path $stage.path $ClassesSnapshotName
        $stageNetworksSnapshot = Join-Path $stage.path $NetworksSnapshotName
        $stageComparison = Join-Path $stage.path $ComparisonName
        $stageCompleteManifest = Join-Path $stage.path $CompleteManifestName

        [byte[]]$classesBytes = Read-StableFileBytes `
            -Path $classesPath -FileOwner 'Classes snapshot source'
        [byte[]]$networksBytes = Read-StableFileBytes `
            -Path $networksPath -FileOwner 'Networks snapshot source'
        Write-CreateNewBytes `
            -Path $stageClassesSnapshot `
            -Bytes $classesBytes `
            -FileOwner 'Classes staging snapshot'
        Write-CreateNewBytes `
            -Path $stageNetworksSnapshot `
            -Bytes $networksBytes `
            -FileOwner 'Networks staging snapshot'
        $classesSnapshotIdentity = Get-FileIdentity `
            -Path $stageClassesSnapshot -FileOwner 'Classes staging snapshot'
        $networksSnapshotIdentity = Get-FileIdentity `
            -Path $stageNetworksSnapshot -FileOwner 'Networks staging snapshot'
        Assert-IdentityValue `
            -Actual $classesSnapshotIdentity `
            -ExpectedBytes $classesIdentity.bytes `
            -ExpectedSha256 $classesIdentity.sha256 `
            -IdentityOwner 'Classes staging snapshot'
        Assert-IdentityValue `
            -Actual $networksSnapshotIdentity `
            -ExpectedBytes $networksIdentity.bytes `
            -ExpectedSha256 $networksIdentity.sha256 `
            -IdentityOwner 'Networks staging snapshot'

        Assert-LasalStopped -Phase 'converter invocation'
        Assert-LogSnapshotUnchanged `
            -Path $resolvedLogPath `
            -ExpectedBytes $logEndOffset `
            -ExpectedSha256 $logSnapshotSha `
            -Phase 'converter invocation'
        $converterPath = Join-Path $root $ConverterRelativePath
        $converterRun = Invoke-IsolatedPowerShellTool `
            -ScriptPath $converterPath `
            -ToolOwner 'bounded log converter' `
            -Arguments @(
                '-BaselinePath', $baselinePath,
                '-LasalLogPath', $resolvedLogPath,
                '-OutputPath', $stageTranscript,
                '-LogEndOffset', $logEndOffset.ToString(
                    [Globalization.CultureInfo]::InvariantCulture),
                '-RawDeltaOutputPath', $stageRawDelta,
                '-RawDeltaManifestPath', $stageRawManifest,
                '-EvidenceProfile', 'GateDVisualLayout')
        if ($converterRun.exitCode -ne 0) {
            Throw-FinalizerBlocker (
                "converter exit is $($converterRun.exitCode), expected 0: " +
                $converterRun.outputText)
        }
        $converterManifest = Read-StrictJsonFile `
            -Path $stageRawManifest `
            -FileOwner 'converter bounded manifest'
        Assert-ConverterManifestContract `
            -Manifest $converterManifest `
            -FrozenLogEndOffset $logEndOffset `
            -ClassesIdentity $classesSnapshotIdentity `
            -NetworksIdentity $networksSnapshotIdentity
        [void](Assert-OutputIdentityUnchanged `
                -Path $classesPath `
                -Expected $classesSnapshotIdentity `
                -OutputOwner 'Classes after converter')
        [void](Assert-OutputIdentityUnchanged `
                -Path $networksPath `
                -Expected $networksSnapshotIdentity `
                -OutputOwner 'Networks after converter')

        Assert-LasalStopped -Phase 'C78 VerifyBuild invocation'
        Assert-LogSnapshotUnchanged `
            -Path $resolvedLogPath `
            -ExpectedBytes $logEndOffset `
            -ExpectedSha256 $logSnapshotSha `
            -Phase 'C78 VerifyBuild invocation'
        $verifierPath = Join-Path $root $VerifierRelativePath
        $verifierRun = Invoke-IsolatedPowerShellTool `
            -ScriptPath $verifierPath `
            -ToolOwner 'C78 VerifyBuild without full static' `
            -Arguments @(
                '-VerifyBuild',
                '-RepositoryRoot', $root,
                '-EvidencePath', $baselinePath,
                '-BuildTranscriptPath', $stageTranscript,
                '-EvidenceProfile', 'GateDVisualLayout',
                '-BoundedLogDeltaPath', $stageRawDelta,
                '-BoundedLogDeltaManifestPath', $stageRawManifest,
                '-LasalLogPath', $resolvedLogPath)
        if (($verifierRun.exitCode -ne 0) -or
            ($verifierRun.outputText -notmatch
                'PASS LASAL\.C78RebuildEvidence\.Verify')) {
            Throw-FinalizerBlocker (
                "C78 VerifyBuild exit/output is invalid: exit=$($verifierRun.exitCode); " +
                $verifierRun.outputText)
        }
        $verifiedTranscriptIdentity = Get-FileIdentity `
            -Path $stageTranscript `
            -FileOwner 'C78-verified build transcript'
        $verifiedRawDeltaIdentity = Get-FileIdentity `
            -Path $stageRawDelta `
            -FileOwner 'C78-verified raw bounded delta'
        $verifiedRawManifestIdentity = Get-FileIdentity `
            -Path $stageRawManifest `
            -FileOwner 'C78-verified raw bounded manifest'
        [void](Assert-OutputIdentityUnchanged `
                -Path $classesPath `
                -Expected $classesSnapshotIdentity `
                -OutputOwner 'Classes after C78 VerifyBuild')
        [void](Assert-OutputIdentityUnchanged `
                -Path $networksPath `
                -Expected $networksSnapshotIdentity `
                -OutputOwner 'Networks after C78 VerifyBuild')

        Assert-LasalStopped -Phase 'Classes comparator invocation'
        Assert-LogSnapshotUnchanged `
            -Path $resolvedLogPath `
            -ExpectedBytes $logEndOffset `
            -ExpectedSha256 $logSnapshotSha `
            -Phase 'Classes comparator invocation'
        $snapshotRelative = $stageClassesSnapshot.Substring($root.Length + 1).Replace('\', '/')
        $comparatorPath = Join-Path $root $ComparatorRelativePath
        $comparatorRun = Invoke-IsolatedPowerShellTool `
            -ScriptPath $comparatorPath `
            -ToolOwner 'Classes comparator' `
            -Arguments @(
                '-RepositoryRoot', $root,
                '-Checkpoint', $CheckpointCommit,
                '-CheckpointPath', $ClassesRelativePath,
                '-CandidatePath', $snapshotRelative,
                '-OutputPath', $stageComparison,
                '-CreateNew')
        if ($comparatorRun.exitCode -notin @(0, 2, 3)) {
            Throw-FinalizerBlocker (
                "Classes comparator blocked with exit $($comparatorRun.exitCode): " +
                $comparatorRun.outputText)
        }
        $comparison = Read-StrictJsonFile `
            -Path $stageComparison `
            -FileOwner 'Classes comparison JSON' `
            -RequireComparatorCanonicalRoundTrip
        $decision = Get-FinalDecision `
            -Report $comparison `
            -ComparatorExitCode $comparatorRun.exitCode `
            -ClassesIdentity $classesSnapshotIdentity `
            -NetworksIdentity $networksSnapshotIdentity `
            -ExpectedCandidatePath $snapshotRelative `
            -ComparisonOracle $comparisonOracle
        [byte[]]$comparisonCanonicalBytes =
            ConvertTo-ComparatorCanonicalJsonBytes -Value $comparison
        $validatedComparisonIdentity = Get-FileIdentity `
            -Path $stageComparison `
            -FileOwner 'decision-validated Classes comparison JSON'
        Assert-IdentityValue `
            -Actual $validatedComparisonIdentity `
            -ExpectedBytes $comparisonCanonicalBytes.LongLength `
            -ExpectedSha256 (Get-BytesSha256 -Bytes $comparisonCanonicalBytes) `
            -IdentityOwner 'decision-validated Classes comparison JSON'

        Assert-LasalStopped -Phase 'complete-manifest preflight'
        [void](Assert-PowerShell7FinalizationEngine `
                -Phase 'complete-manifest preflight' `
                -Expected $ProductionEngine)
        Assert-LogSnapshotUnchanged `
            -Path $resolvedLogPath `
            -ExpectedBytes $logEndOffset `
            -ExpectedSha256 $logSnapshotSha `
            -Phase 'complete-manifest preflight'
        [void](Assert-OutputIdentityUnchanged `
                -Path $classesPath `
                -Expected $classesSnapshotIdentity `
                -OutputOwner 'Classes final rehash')
        [void](Assert-OutputIdentityUnchanged `
                -Path $networksPath `
                -Expected $networksSnapshotIdentity `
                -OutputOwner 'Networks final rehash')
        $preManifestHead = Invoke-GitText `
            -Root $root `
            -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
            -Operation 'pre-manifest HEAD resolution'
        if ($preManifestHead -cne $initialHead) {
            Throw-FinalizerBlocker 'Git HEAD changed during finalization.'
        }
        Assert-GitTrackedCleanPath `
            -Root $root `
            -RelativePath $FinalizerRelativePath `
            -PathOwner 'candidate finalizer final recheck'
        [void](Assert-OutputIdentityUnchanged `
                -Path (Join-Path $root $FinalizerRelativePath) `
                -Expected $finalizerIdentity `
                -OutputOwner 'candidate finalizer final recheck')
        [void](Assert-TrustedArtifacts -Root $root)
        [void](Assert-BaselineAndInputIdentities `
                -Root $root `
                -Baseline $baseline `
                -ResolvedLogPath $resolvedLogPath)
        $artifactNames = @(
            $OwnerMarkerName,
            $ClassesSnapshotName,
            $NetworksSnapshotName,
            $TranscriptName,
            $RawDeltaName,
            $RawManifestName,
            $ComparisonName)
        [void](Assert-OwnedStageMarkerContract -Stage $stage)
        $artifactReports = @(
            [pscustomobject]@{
                fileName = $OwnerMarkerName
                bytes = [long]$stageMarkerIdentity.bytes
                sha256 = [string]$stageMarkerIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $ClassesSnapshotName
                bytes = [long]$classesSnapshotIdentity.bytes
                sha256 = [string]$classesSnapshotIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $NetworksSnapshotName
                bytes = [long]$networksSnapshotIdentity.bytes
                sha256 = [string]$networksSnapshotIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $TranscriptName
                bytes = [long]$verifiedTranscriptIdentity.bytes
                sha256 = [string]$verifiedTranscriptIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $RawDeltaName
                bytes = [long]$verifiedRawDeltaIdentity.bytes
                sha256 = [string]$verifiedRawDeltaIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $RawManifestName
                bytes = [long]$verifiedRawManifestIdentity.bytes
                sha256 = [string]$verifiedRawManifestIdentity.sha256
            },
            [pscustomobject]@{
                fileName = $ComparisonName
                bytes = [long]$validatedComparisonIdentity.bytes
                sha256 = [string]$validatedComparisonIdentity.sha256
            })
        $observedArtifactReports = @(Get-ExactStageInventoryReports `
                -StagePath $stage.path `
                -ExpectedFileNames $artifactNames)
        Assert-ArtifactReportSequencesEqual `
            -Expected $artifactReports `
            -Actual $observedArtifactReports `
            -SequenceOwner 'validation-adjacent seven staged artifact identities'
        $completeManifest = [ordered]@{
            schema = $Schema
            complete = $true
            capturedAtUtc = [DateTime]::UtcNow.ToString('o')
            repository = [ordered]@{
                root = $root
                headObserved = $initialHead
                headPinnedForDecision = $false
                checkpointCommit = $CheckpointCommit
            }
            powershellEngine = [ordered]@{
                psEdition = [string]$ProductionEngine.psEdition
                major = [int]$ProductionEngine.major
                version = [string]$ProductionEngine.version
                minimumSupportedMajor = 7
                directoryNtfsStreamEnumerationRequired = $true
                productionFinalizationSupported = $true
            }
            trustedArtifacts = $trustedArtifactReports
            finalizer = [ordered]@{
                relativePath = $FinalizerRelativePath
                bytes = $finalizerIdentity.bytes
                sha256 = $finalizerIdentity.sha256
                headBlobOid = $finalizerHeadBlobOid
                gitTrackedAndHeadClean = $true
            }
            baseline = [ordered]@{
                relativePath = $BaselineRelativePath
                inputIdentityCount = 10
                currentInputs = $baselineInputs.inputIdentities
            }
            log = [ordered]@{
                path = $resolvedLogPath
                baselinePrefixBytes = $prefixLength
                baselinePrefixSha256 = $prefixSha
                frozenEndOffset = $logEndOffset
                frozenFullSha256 = $logSnapshotSha
                tailAppendPolicy =
                    'forbidden-until-atomic-publish-full-length-and-sha-must-remain-exact'
            }
            isolatedSession = $sessionContract
            tools = [ordered]@{
                converter = [ordered]@{
                    exitCode = $converterRun.exitCode
                    outputLines = $converterRun.outputLines
                }
                c78VerifyBuild = [ordered]@{
                    exitCode = $verifierRun.exitCode
                    runFullStatic = $false
                    outputLines = $verifierRun.outputLines
                }
                comparator = [ordered]@{
                    exitCode = $comparatorRun.exitCode
                    disposition = $comparison.decision.disposition
                    outputFileName = $ComparisonName
                }
            }
            regeneratedOutputs = [ordered]@{
                classes = [ordered]@{
                    sourceRelativePath = $ClassesRelativePath
                    snapshotFileName = $ClassesSnapshotName
                    bytes = $classesSnapshotIdentity.bytes
                    sha256 = $classesSnapshotIdentity.sha256
                }
                networks = [ordered]@{
                    sourceRelativePath = $NetworksRelativePath
                    snapshotFileName = $NetworksSnapshotName
                    bytes = $networksSnapshotIdentity.bytes
                    sha256 = $networksSnapshotIdentity.sha256
                }
                converterManifestMatchedSnapshots = $true
                c78VerifierAcceptedManifestAndCurrentOutputs = $true
                comparatorMatchedClassesSnapshot = $true
                finalProductionRehashMatchedSnapshots = $true
            }
            artifactsWrittenBeforeCompleteManifest = $artifactReports
            publication = [ordered]@{
                stagingDirectoryName = $stage.name
                finalDirectoryName = $FinalDirectoryName
                finalDirectoryAtomicMoveRequired = $true
                existingOutputOverwriteAllowed = $false
                retryPolicy =
                    'failed exact-owned current stage is removed; stale/ambiguous/final bundles require manual review'
                completeManifestWrittenLast = $true
            }
            decision = $decision
            productionApproved = $false
        }
        [byte[]]$completeManifestBytes = ConvertTo-JsonBytes -Value $completeManifest
        $completeManifestExpectedIdentity = [pscustomobject]@{
            bytes = [long]$completeManifestBytes.LongLength
            sha256 = Get-BytesSha256 -Bytes $completeManifestBytes
        }
        Write-CreateNewBytes `
            -Path $stageCompleteManifest `
            -Bytes $completeManifestBytes `
            -FileOwner 'complete finalization manifest'

        $completeManifestIdentity = Get-FileIdentity `
            -Path $stageCompleteManifest `
            -FileOwner 'complete finalization manifest readback'
        Assert-IdentityValue `
            -Actual $completeManifestIdentity `
            -ExpectedBytes $completeManifestExpectedIdentity.bytes `
            -ExpectedSha256 $completeManifestExpectedIdentity.sha256 `
            -IdentityOwner 'intended complete finalization manifest bytes'
        $completeManifestReadback = Read-StrictJsonFile `
            -Path $stageCompleteManifest `
            -FileOwner 'complete finalization manifest readback'
        if (($completeManifestIdentity.bytes -le 0) -or
            ($completeManifestReadback.schema -cne $Schema) -or
            (-not [bool]$completeManifestReadback.complete) -or
            [bool]$completeManifestReadback.productionApproved -or
            ([string]$completeManifestReadback.decision.disposition -cne
                [string]$decision.disposition) -or
            (-not (Test-JsonDeepExact `
                    -Actual $completeManifestReadback `
                    -Expected $completeManifest))) {
            Throw-FinalizerBlocker 'complete finalization manifest readback differs.'
        }
        $finalArtifactNames = @($artifactNames) + @($CompleteManifestName)
        $expectedFinalArtifactReports = @($artifactReports) + @(
            [pscustomobject]@{
                fileName = $CompleteManifestName
                bytes = [long]$completeManifestExpectedIdentity.bytes
                sha256 = [string]$completeManifestExpectedIdentity.sha256
            })
        $artifactReportsFinal = @(Get-ExactStageInventoryReports `
                -StagePath $stage.path `
                -ExpectedFileNames $finalArtifactNames)
        Assert-ArtifactReportSequencesEqual `
            -Expected $expectedFinalArtifactReports `
            -Actual $artifactReportsFinal `
            -SequenceOwner 'eight staged artifacts after complete-manifest write'

        Assert-LasalStopped -Phase 'atomic publish'
        [void](Assert-PowerShell7FinalizationEngine `
                -Phase 'atomic publish' `
                -Expected $ProductionEngine)
        Assert-LogSnapshotUnchanged `
            -Path $resolvedLogPath `
            -ExpectedBytes $logEndOffset `
            -ExpectedSha256 $logSnapshotSha `
            -Phase 'atomic publish'
        [void](Assert-OutputIdentityUnchanged `
                -Path $classesPath `
                -Expected $classesSnapshotIdentity `
                -OutputOwner 'Classes atomic-publish rehash')
        [void](Assert-OutputIdentityUnchanged `
                -Path $networksPath `
                -Expected $networksSnapshotIdentity `
                -OutputOwner 'Networks atomic-publish rehash')
        $publishHead = Invoke-GitText `
            -Root $root `
            -Arguments @('rev-parse', '--verify', 'HEAD^{commit}') `
            -Operation 'atomic-publish HEAD resolution'
        if ($publishHead -cne $initialHead) {
            Throw-FinalizerBlocker 'Git HEAD changed before atomic publish.'
        }
        Assert-GitTrackedCleanPath `
            -Root $root `
            -RelativePath $FinalizerRelativePath `
            -PathOwner 'candidate finalizer atomic-publish recheck'
        [void](Assert-OutputIdentityUnchanged `
                -Path (Join-Path $root $FinalizerRelativePath) `
                -Expected $finalizerIdentity `
                -OutputOwner 'candidate finalizer atomic-publish recheck')
        $publishTrustedArtifactReports = @(Assert-TrustedArtifacts -Root $root)
        Assert-NamedIdentityReportSequencesEqual `
            -Expected $trustedArtifactReports `
            -Actual $publishTrustedArtifactReports `
            -NameProperty 'relativePath' `
            -SequenceOwner 'trusted tools/baseline at atomic publish'
        $publishBaselineInputs = Assert-BaselineAndInputIdentities `
            -Root $root `
            -Baseline $baseline `
            -ResolvedLogPath $resolvedLogPath
        Assert-NamedIdentityReportSequencesEqual `
            -Expected $baselineInputs.inputIdentities `
            -Actual $publishBaselineInputs.inputIdentities `
            -NameProperty 'relativePath' `
            -SequenceOwner 'ten baseline inputs at atomic publish'
        [void](Assert-OwnedStageMarkerContract -Stage $stage)
        $publishArtifactReports = @(Get-ExactStageInventoryReports `
                -StagePath $stage.path `
                -ExpectedFileNames $finalArtifactNames)
        Assert-ArtifactReportSequencesEqual `
            -Expected $expectedFinalArtifactReports `
            -Actual $publishArtifactReports `
            -SequenceOwner 'move-adjacent exact eight-file stage inventory'
        Assert-NoReparsePointChain `
            -Root $PSScriptRoot `
            -Path $finalDirectory `
            -PathOwner 'atomic-publish final directory'
        if (Test-Path -LiteralPath $finalDirectory) {
            Throw-FinalizerBlocker 'final output directory appeared before atomic publish.'
        }
        [IO.Directory]::Move($stage.path, $finalDirectory)
        $published = $true
        $stage = $null
        Write-Output (
            "$($decision.disposition) exit=$($decision.exitCode) " +
            "ProductionApproved=false bundle=$finalDirectory")
        return [int]$decision.exitCode
    }
    catch {
        $original = $_
        if (-not $published -and $null -ne $stage) {
            try {
                Remove-ExactOwnedStageDirectory -Stage $stage
            }
            catch {
                Throw-FinalizerBlocker (
                    "original failure: $($original.Exception.Message); exact-owned stage " +
                    "cleanup also failed and was left for manual review: $($_.Exception.Message)")
            }
        }
        throw $original
    }
}

function New-SyntheticRecordIdentityFromOracle {
    param([Parameter(Mandatory = $true)]$Oracle)
    return [pscustomobject]@{
        startOffset = [long]$Oracle.startOffset
        endOffsetExclusive = [long]$Oracle.endOffsetExclusive
        sourceOffset = [long]$Oracle.sourceOffset
        bytes = [long]$Oracle.bytes
        sha256 = [string]$Oracle.sha256
    }
}

function New-SyntheticComparisonRecordFromOracle {
    param([Parameter(Mandatory = $true)]$Oracle)
    $record = [ordered]@{
        owner = [string]$Oracle.owner
        sourcePath = [string]$Oracle.sourcePath
        parser = [string]$Oracle.parser
        exact = $true
        checkpoint = New-SyntheticRecordIdentityFromOracle -Oracle $Oracle
        candidate = New-SyntheticRecordIdentityFromOracle -Oracle $Oracle
    }
    if ($Oracle.PSObject.Properties.Name -contains 'legacyWindowExact') {
        $record.legacyWindowExact = $true
    }
    return [pscustomobject]$record
}

function New-SyntheticComparisonReport {
    param(
        [Parameter(Mandatory = $true)][string]$CandidateSha256,
        [Parameter(Mandatory = $true)][int]$ExitCode,
        [Parameter(Mandatory = $true)][string]$Disposition,
        [string]$CandidatePath = 'test/synthetic/Classes.post-rebuild.snapshot.lcb',
        [long]$CandidateBytes = 8549773L
    )
    $isExact = ($ExitCode -eq 0)
    $changedBytes = if ($isExact) { 0L } else { 99L }
    $changedRuns = if ($isExact) { 0 } else { 58 }
    $changedOwners = if ($isExact) { 0 } else { 36 }
    [object[]]$changedOwnerEvidence = @()
    [object[]]$diffRunEvidence = @()
    if (-not $isExact) {
        $changedOwnerEvidence = @(
            [pscustomobject]@{ synthetic = 'changed-owner-oracle' })
        $diffRunEvidence = @(
            [pscustomobject]@{ synthetic = 'diff-run-oracle' })
    }
    return [pscustomobject]@{
        schema = 'LasalClassesArtifactComparison/v1'
        decision = [pscustomobject]@{
            disposition = $Disposition
            checkpointIdentityAccepted = ($ExitCode -eq 0)
            approvalScope = 'checkpoint-byte-identity-only'
            productionApproved = $false
            exactCheckpointMatch = ($ExitCode -eq 0)
            semanticEquivalenceProven = ($ExitCode -eq 0)
            recordEqualityCannotApproveArtifact = $true
            exitCode = $ExitCode
        }
        checkpoint = [pscustomobject]@{
            requested = $CheckpointCommit
            kind = 'revision'
            resolvedRevision = $CheckpointCommit
            relativePath = $ClassesRelativePath
            blobOid = $CheckpointClassesBlobOid
            rawBytes = $CheckpointClassesBytes
            sha256 = $CheckpointClassesSha256
        }
        candidate = [pscustomobject]@{
            path = $CandidatePath
            rawBytes = $CandidateBytes
            sha256 = $CandidateSha256
        }
        comparison = [pscustomobject]@{
            byteExact = $isExact
            equalLength = $true
            lengthDelta = 0L
            alignment = 'equal-length-indexed'
            changedByteCountDefined = $true
            changedByteCount = $changedBytes
            contiguousRunCount = $changedRuns
            checkpointChangedOwnerCount = $changedOwners
            unmappedRunCount = 0
            changedOwnersAreFrozenOpaqueSubset = $true
            frozenOpaqueOwnerCount = 36
            frozenOpaqueOwners = @($FrozenOpaqueOwnersOracle)
            proprietaryFieldSemanticsDecoded = $false
        }
        recordParser = [pscustomobject]@{
            convention = 'first-special-record-then-aa03-header-to-next-header-or-eof'
            latin1ByteOffsetPreserving = $true
            sourceMarkerBoundary = 'path-length-le24-plus-aa'
            trueHeaderBoundary =
                'aa-03-plus-class-name-length-le24-plus-aa-plus-class-name'
            sourcePathSegmentsDiagnosticOnly = $true
            checkpointOwnerRecordCount = 120
            candidateOwnerRecordCount = 120
            headerSourceInventory = [pscustomobject]@{
                exact = $true
                checkpointCount = 120
                candidateCount = 120
                firstMismatch = $null
                comparedFields = @($HeaderComparedFieldsOracle)
            }
            firstSpecialRecord =
                New-SyntheticComparisonRecordFromOracle `
                    -Oracle $FirstSpecialRecordOracle
        }
        gateDTargetRecords = [pscustomobject]@{
            allEqual = $true
            records = @($GateDTargetRecordOracles | ForEach-Object {
                    New-SyntheticComparisonRecordFromOracle -Oracle $_
                })
        }
        protectedDependencyRecords = [pscustomobject]@{
            allEqual = $true
            records = @($ProtectedDependencyRecordOracles | ForEach-Object {
                    New-SyntheticComparisonRecordFromOracle -Oracle $_
                })
        }
        changedCheckpointOwners = $changedOwnerEvidence
        diffRuns = $diffRunEvidence
    }
}

function New-SyntheticIsolatedRebuildLog {
    param(
        [Parameter(Mandatory = $true)][string]$CanonicalProjectPath,
        [switch]$WithoutRestoration,
        [switch]$WithoutKnownLoadError
    )
    $projectRoot = Split-Path -Parent $CanonicalProjectPath
    $filePath = Join-Path $projectRoot 'Class\TCPMotionInterface\TCPMotionInterface.st'
    $lines = New-Object Collections.Generic.List[string]
    $lines.Add('[11:59:56 (INFO) Application] Log File is ok')
    $lines.Add("[11:59:56 P:00100 T:00001 (INFO) OutputSkripting] Run Scriptfile 'C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\Lasal2.py'.")
    $lines.Add('[11:59:57 P:00100 T:00001 (INFO) OutputSkripting] Total Script need 112.193 ms')
    $lines.Add('[11:59:58 P:00100 T:00001 (INFO) OutputDataAnalyzer] Loading DataAnalyzer configuration file "C:\ProgramData\Sigmatek\Drive(C)\Program Files (x86)\Sigmatek\Lasal\Class2\Config\DataAnalyser.lcc".')
    $lines.Add('[11:59:58 P:00100 T:00001 (INFO) OutputDataAnalyzer] Loading DataAnalyzer configuration file "C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\DataAnalyserSDD.lcc".')
    $lines.Add('[11:59:58 P:00100 T:00001 (INFO) OutputDataAnalyzer] Cannot find configuration file: "C:\Program Files (x86)\Sigmatek\Lasal\Class2\Bin\DataAnalyserSDD.lcc"')
    $lines.Add('[12:00:00 P:00100 T:00001 (INFO) GUI] Start Application at 2026-08-10 12:00:00')
    $lines.Add(
        "[12:00:01 P:00100 T:00002 (INFO) CmdProc] Executing command 'Load Project `"$CanonicalProjectPath`"'")
    if (-not $WithoutKnownLoadError) {
        $lines.Add(
            ('[12:00:02 P:00100 T:00002 (ERROR) Compiler] E 0015 ' +
                '"C:\vendor\global.h"(15) Error reading file ' +
                "'C:\vendor\Class\_DriveMngBase\DriveComL2.h'" +
                '|*000000*|15|11015||'))
    }
    if (-not $WithoutRestoration) {
        $lines.Add(
            "[12:00:03 P:00100 T:00001 (INFO) CmdProc] Executing command 'Open Network Editor for 'Comm_Network''")
        $lines.Add('[12:00:03 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)')
        $lines.Add(
            '[12:00:04 P:00100 T:00001 (INFO) CmdProc] Executing command ' +
            "'Open Implementation Editor for `"TCPMotionInterface`"'")
        $lines.Add('[12:00:04 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)')
        $lines.Add(
            '[12:00:05 P:00100 T:00001 (INFO) CmdProc] Executing command ' +
            "'Open File Editor for `"$filePath`"'")
        $lines.Add('[12:00:05 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)')
    }
    $lines.Add('[12:00:06 P:00100 T:00002 (INFO) Compiler] {ResultCount}')
    $lines.Add('[12:00:06 P:00100 T:00002 (INFO) CmdProc] Last command succeeded. (6ms)')
    $lines.Add("[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
    $lines.Add('[12:00:07 P:00100 T:00003 (INFO) Compiler] {Clear}')
    $lines.Add(
        '[12:00:07 P:00100 T:00003 (INFO) Compiler] ' +
        'Rebuild project with compiler version C78 (target architecture: ARM)')
    $lines.Add(
        "[12:00:08 P:00100 T:00003 (INFO) OutputCommand] Save project '$CanonicalProjectPath'.")
    $lines.Add('[12:00:09 P:00100 T:00003 (INFO) Compiler] Done')
    $lines.Add('[12:00:10 P:00100 T:00003 (INFO) Compiler] Done')
    $lines.Add('[12:00:11 P:00100 T:00003 (INFO) Linker] Done')
    $lines.Add('[12:00:12 P:00100 T:00003 (INFO) Compiler] {ResultCount}')
    $lines.Add('[12:00:13 P:00100 T:00003 (INFO) CmdProc] Last command succeeded. (6ms)')
    $lines.Add('[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...')
    $lines.Add("[12:00:15 P:00100 T:00001 (INFO) CmdProc] Executing command 'Close Project'")
    $lines.Add('[12:00:16 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)')
    $lines.Add('[12:00:17 P:00100 T:00001 (INFO) GUI] ...LC2 exit done.')
    return ($lines.ToArray() -join "`n") + "`n"
}

function Invoke-FinalizerSelfTest {
    $script:selfTestPositive = 0
    $script:selfTestNegative = 0
    function Assert-SelfTestTrue {
        param([bool]$Condition, [string]$Message)
        if (-not $Condition) {
            throw "self-test assertion failed: $Message"
        }
        $script:selfTestPositive++
    }
    function Assert-SelfTestThrows {
        param([scriptblock]$Action, [string]$Message)
        $threw = $false
        try {
            & $Action
        }
        catch {
            $threw = $true
        }
        if (-not $threw) {
            throw "self-test negative was accepted: $Message"
        }
        $script:selfTestNegative++
    }

    $windowsPowerShell = Join-Path `
        $env:SystemRoot `
        'System32\WindowsPowerShell\v1.0\powershell.exe'
    if (-not (Test-Path -LiteralPath $windowsPowerShell -PathType Leaf)) {
        throw 'self-test Windows PowerShell 5 executable is missing.'
    }
    $selfTestScriptPath = Join-Path `
        $PSScriptRoot `
        'Finalize-LasalClassesRebuildCandidate.ps1'
    if (-not (Test-Path -LiteralPath $selfTestScriptPath -PathType Leaf)) {
        throw 'self-test production-entry script path is missing.'
    }
    $ps5StartInfo = New-Object Diagnostics.ProcessStartInfo
    $ps5StartInfo.FileName = $windowsPowerShell
    $ps5StartInfo.Arguments =
        '-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' +
        $selfTestScriptPath + '" -FinalizeCandidate'
    $ps5StartInfo.UseShellExecute = $false
    $ps5StartInfo.CreateNoWindow = $true
    $ps5StartInfo.RedirectStandardOutput = $true
    $ps5StartInfo.RedirectStandardError = $true
    $ps5Process = New-Object Diagnostics.Process
    $ps5Process.StartInfo = $ps5StartInfo
    try {
        if (-not $ps5Process.Start()) {
            throw 'self-test Windows PowerShell 5 process did not start.'
        }
        $ps5StandardOutput = $ps5Process.StandardOutput.ReadToEnd()
        $ps5StandardError = $ps5Process.StandardError.ReadToEnd()
        $ps5Process.WaitForExit()
        $ps5ProductionExitCode = [int]$ps5Process.ExitCode
    }
    finally {
        $ps5Process.Dispose()
    }
    $ps5ProductionText = $ps5StandardOutput + "`n" + $ps5StandardError
    Assert-SelfTestTrue `
        -Condition (
            $ps5ProductionExitCode -eq 4 -and
            $ps5ProductionText -match
                'production finalization requires PowerShell Core 7 or newer' -and
            $ps5ProductionText -notmatch 'tracked-path check') `
        -Message 'PS5 top-level production finalization rejects before evidence work'

    $selfTestVolumeRoot = [IO.Path]::GetPathRoot($selfTestScriptPath)
    Assert-SelfTestTrue `
        -Condition (Test-PathInsideRoot `
            -Root $selfTestVolumeRoot `
            -Path $selfTestScriptPath) `
        -Message 'volume-root containment accepts a descendant without a doubled separator'
    Assert-SelfTestTrue `
        -Condition (
            (-not (Test-PathInsideRoot `
                    -Root $selfTestVolumeRoot `
                    -Path $selfTestVolumeRoot)) -and
            (Test-PathInsideRoot `
                -Root $selfTestVolumeRoot `
                -Path $selfTestVolumeRoot `
                -AllowEqual)) `
        -Message 'volume-root containment preserves strict versus allow-equal semantics'

    $project = 'C:\synthetic\repo\Lasal_PRG\Elmo_EtherCAT_Test_4Axis\' +
        'Elmo_EtherCAT_Test_4Axis.lcp'
    $syntheticCandidatePath = 'test/synthetic/Classes.post-rebuild.snapshot.lcb'
    $goodLog = New-SyntheticIsolatedRebuildLog -CanonicalProjectPath $project
    $goodAnalysis = Assert-IsolatedRebuildSessionContract `
        -AppendedLogText $goodLog `
        -CanonicalProjectPath $project
    Assert-SelfTestTrue `
        -Condition (
            $goodAnalysis.exactSessionCount -eq 1 -and
            $goodAnalysis.exactRebuildCount -eq 1 -and
            $goodAnalysis.preStartPrologue.Count -eq 6 -and
            $goodAnalysis.commandTerminalLedger.Count -eq 6 -and
            $goodAnalysis.loadRestorationCommands.Count -eq 3 -and
            $goodAnalysis.knownLoadErrors.Count -eq 1) `
        -Message 'positive load-restoration/E0015 contract'
    $minimalLog = New-SyntheticIsolatedRebuildLog `
        -CanonicalProjectPath $project `
        -WithoutRestoration `
        -WithoutKnownLoadError
    $minimalAnalysis = Assert-IsolatedRebuildSessionContract `
        -AppendedLogText $minimalLog `
        -CanonicalProjectPath $project
    Assert-SelfTestTrue `
        -Condition (
            $minimalAnalysis.loadRestorationCommands.Count -eq 0 -and
            $minimalAnalysis.knownLoadErrors.Count -eq 0) `
        -Message 'positive no-restoration/no-E0015 contract'

    $classes244 = [pscustomobject]@{
        bytes = 8549773L
        sha256 = $CheckpointClassesSha256
    }
    $classes6E = [pscustomobject]@{
        bytes = 8549773L
        sha256 = $KnownRebuiltClassesSha256
    }
    $classesThird = [pscustomobject]@{
        bytes = 8549773L
        sha256 = ('A' * 64)
    }
    $networks = [pscustomobject]@{
        bytes = 242363L
        sha256 = $KnownNetworksSha256
    }
    $syntheticComparisonOracle = New-SyntheticComparisonReport `
        -CandidateSha256 $KnownRebuiltClassesSha256 `
        -ExitCode 2 `
        -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
    $exactReport = New-SyntheticComparisonReport `
        -CandidateSha256 $CheckpointClassesSha256 `
        -ExitCode 0 `
        -Disposition 'EXACT_CHECKPOINT_MATCH'
    $exactDecision = Get-FinalDecision `
        -Report $exactReport `
        -ComparatorExitCode 0 `
        -ClassesIdentity $classes244 `
        -NetworksIdentity $networks `
        -ExpectedCandidatePath $syntheticCandidatePath `
        -ComparisonOracle $syntheticComparisonOracle
    Assert-SelfTestTrue `
        -Condition (
            $exactDecision.exitCode -eq 0 -and
            $exactDecision.staticReplayPermitted -and
            -not $exactDecision.productionApproved) `
        -Message '24402 checkpoint decision'
    $reviewReport = New-SyntheticComparisonReport `
        -CandidateSha256 $KnownRebuiltClassesSha256 `
        -ExitCode 2 `
        -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
    $reviewReport.decision.semanticEquivalenceProven = $false
    $reviewDecision = Get-FinalDecision `
        -Report $reviewReport `
        -ComparatorExitCode 2 `
        -ClassesIdentity $classes6E `
        -NetworksIdentity $networks `
        -ExpectedCandidatePath $syntheticCandidatePath `
        -ComparisonOracle $syntheticComparisonOracle
    Assert-SelfTestTrue `
        -Condition (
            $reviewDecision.exitCode -eq 2 -and
            $reviewDecision.known6EReproduced -and
            -not $reviewDecision.productionApproved) `
        -Message '6E review-only decision'
    $thirdReport = New-SyntheticComparisonReport `
        -CandidateSha256 $classesThird.sha256 `
        -ExitCode 3 `
        -Disposition 'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
    $thirdReport.decision.semanticEquivalenceProven = $false
    $thirdDecision = Get-FinalDecision `
        -Report $thirdReport `
        -ComparatorExitCode 3 `
        -ClassesIdentity $classesThird `
        -NetworksIdentity $networks `
        -ExpectedCandidatePath $syntheticCandidatePath `
        -ComparisonOracle $syntheticComparisonOracle
    Assert-SelfTestTrue `
        -Condition (
            $thirdDecision.exitCode -eq 3 -and
            $thirdDecision.disposition -ceq 'UNSTABLE_THIRD_CLASSES_HASH_STOP' -and
            -not $thirdDecision.productionApproved) `
        -Message 'third-hash unstable-stop decision'
    $thirdReviewReport = New-SyntheticComparisonReport `
        -CandidateSha256 $classesThird.sha256 `
        -ExitCode 2 `
        -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
    $thirdReviewReport.decision.semanticEquivalenceProven = $false
    $thirdReviewDecision = Get-FinalDecision `
        -Report $thirdReviewReport `
        -ComparatorExitCode 2 `
        -ClassesIdentity $classesThird `
        -NetworksIdentity $networks `
        -ExpectedCandidatePath $syntheticCandidatePath `
        -ComparisonOracle $syntheticComparisonOracle
    Assert-SelfTestTrue `
        -Condition ($thirdReviewDecision.exitCode -eq 3) `
        -Message 'third-hash review still stops'

    $logNegatives = @(
        [ordered]@{
            name = 'missing GUI start'
            value = $goodLog.Replace('Start Application at', 'StartX at')
        },
        [ordered]@{
            name = 'wrong canonical project'
            value = $goodLog.Replace($project, 'C:\synthetic\wrong\Wrong.lcp')
        },
        [ordered]@{
            name = 'duplicate Rebuild'
            value = $goodLog.Replace(
                "Executing command 'Rebuild project'",
                "Executing command 'Rebuild project'`n" +
                "[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        },
        [ordered]@{
            name = 'additional Build'
            value = $goodLog.Replace(
                "Executing command 'Rebuild project'",
                "Executing command 'Build project'`n" +
                '[12:00:06 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' +
                "`n[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        },
        [ordered]@{
            name = 'post-load Open Implementation'
            value = $goodLog.Replace(
                "[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'",
                '[12:00:06 P:00100 T:00001 (INFO) CmdProc] Executing command ' +
                "'Open Implementation Editor for `"TCPMotionInterface`"'`n" +
                '[12:00:06 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' +
                "`n[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        },
        [ordered]@{
            name = 'Find in Implementation anywhere'
            value = $goodLog.Replace(
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Find in Implementation' +
                "`n[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...")
        },
        [ordered]@{
            name = 'Edit Method anywhere'
            value = $goodLog.Replace('Do exit Lasal2...', 'Edit Method then Do exit Lasal2...')
        },
        [ordered]@{
            name = 'CmdProc Save'
            value = $goodLog.Replace(
                "Executing command 'Rebuild project'",
                "Executing command 'Save Project'`n" +
                '[12:00:06 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' +
                "`n[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        },
        [ordered]@{
            name = 'extra OutputCommand Save'
            value = $goodLog.Replace(
                '[12:00:09 P:00100 T:00003 (INFO) Compiler] Done',
                "[12:00:08 P:00100 T:00003 (INFO) OutputCommand] Save project '$project'.`n" +
                '[12:00:09 P:00100 T:00003 (INFO) Compiler] Done')
        },
        [ordered]@{
            name = 'malformed OutputCommandX Save bypass'
            value = $goodLog.Replace(
                '[12:00:09 P:00100 T:00003 (INFO) Compiler] Done',
                "[12:00:08 P:00100 T:00003 (INFO) OutputCommandX] Save project '$project'.`n" +
                '[12:00:09 P:00100 T:00003 (INFO) Compiler] Done')
        },
        [ordered]@{
            name = 'Last command failed'
            value = $goodLog.Replace(
                'Last command succeeded. (6ms)',
                'Last command failed. (6ms)')
        },
        [ordered]@{
            name = 'ios_base failure'
            value = $goodLog.Replace(
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
                '[12:00:14 P:00100 T:00001 (ERROR) Persistence] ios_base::failure' +
                "`n[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...")
        },
        [ordered]@{
            name = 'unexpected ERROR'
            value = $goodLog.Replace(
                '[12:00:09 P:00100 T:00003 (INFO) Compiler] Done',
                '[12:00:09 P:00100 T:00003 (ERROR) Compiler] unexpected' +
                "`n[12:00:09 P:00100 T:00003 (INFO) Compiler] Done")
        },
        [ordered]@{
            name = 'duplicate known E0015'
            value = $goodLog.Replace(
                '[12:00:02 P:00100 T:00002 (ERROR) Compiler] E 0015',
                '[12:00:02 P:00100 T:00002 (ERROR) Compiler] E 0015' +
                "`n[12:00:02 P:00100 T:00002 (ERROR) Compiler] E 0015")
        },
        [ordered]@{
            name = 'extra PID'
            value = $goodLog.Replace(
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
                '[12:00:14 P:00999 T:00001 (INFO) GUI] unrelated' +
                "`n[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...")
        },
        [ordered]@{
            name = 'post-exit record'
            value = $goodLog + '[12:00:18 P:00100 T:00001 (INFO) GUI] trailing record' + "`n"
        },
        [ordered]@{
            name = 'missing close success'
            value = $goodLog.Replace(
                '[12:00:16 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' + "`n",
                '')
        },
        [ordered]@{
            name = 'missing LC2 exit done'
            value = $goodLog.Replace(
                '[12:00:17 P:00100 T:00001 (INFO) GUI] ...LC2 exit done.' + "`n",
                '')
        },
        [ordered]@{
            name = 'CInvalidArgException'
            value = $goodLog.Replace('Do exit Lasal2...', 'CInvalidArgException Do exit Lasal2...')
        },
        [ordered]@{
            name = 'double-space hidden Executing command'
            value = $goodLog.Replace(
                "Executing command 'Rebuild project'",
                "Executing  command 'Rebuild project'")
        },
        [ordered]@{
            name = 'tab hidden Executing command'
            value = $goodLog.Replace(
                "Executing command 'Rebuild project'",
                "Executing`tcommand 'Rebuild project'")
        },
        [ordered]@{
            name = 'double-space hidden Last command terminal'
            value = $goodLog.Replace(
                'Last command succeeded. (6ms)',
                'Last  command succeeded. (6ms)')
        },
        [ordered]@{
            name = 'tab hidden Last command terminal'
            value = $goodLog.Replace(
                'Last command succeeded. (6ms)',
                "Last`tcommand succeeded. (6ms)")
        },
        [ordered]@{
            name = 'double-space hidden OutputCommand Save'
            value = $goodLog.Replace(
                "Save project '$project'.",
                "Save  project '$project'.")
        },
        [ordered]@{
            name = 'tab hidden OutputCommand Save'
            value = $goodLog.Replace(
                "Save project '$project'.",
                "Save`tproject '$project'.")
        },
        [ordered]@{
            name = 'double-space hidden Find in Implementation'
            value = $goodLog.Replace(
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Find  in  Implementation' +
                "`n[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...")
        },
        [ordered]@{
            name = 'tab hidden Edit Method'
            value = $goodLog.Replace(
                'Do exit Lasal2...',
                "Edit`tMethod then Do exit Lasal2...")
        },
        [ordered]@{
            name = 'malformed CmdProcX Reset bypass'
            value = $goodLog.Replace(
                "[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'",
                "[12:00:06 P:00100 T:00001 (INFO) CmdProcX] Executing command 'Reset Project'`n" +
                "[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        },
        [ordered]@{
            name = 'orphan success terminal'
            value = $goodLog.Replace(
                '[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...',
                '[12:00:13 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' +
                "`n[12:00:14 P:00100 T:00001 (INFO) GUI] Do exit Lasal2...")
        },
        [ordered]@{
            name = 'unknown unparsed nonempty record'
            value = $goodLog.Replace(
                '[11:59:56 (INFO) Application] Log File is ok',
                '[11:59:56 (INFO) Application] Log File is ok' +
                "`n[11:59:56 (INFO) Unknown] malformed")
        }
    )
    foreach ($negative in $logNegatives) {
        Assert-SelfTestThrows `
            -Message $negative.name `
            -Action {
                Assert-IsolatedRebuildSessionContract `
                    -AppendedLogText $negative.value `
                    -CanonicalProjectPath $project
            }
    }
    foreach ($command in @(
            'Connect',
            'Go online',
            'Download Project',
            'Link project',
            'Reset Project',
            'Restart Project',
            'Open Network Editor for ''Comm_Network''')) {
        $mutated = $goodLog.Replace(
            "[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'",
            "[12:00:06 P:00100 T:00001 (INFO) CmdProc] Executing command '$command'`n" +
            '[12:00:06 P:00100 T:00001 (INFO) CmdProc] Last command succeeded. (1ms)' +
            "`n[12:00:07 P:00100 T:00003 (INFO) CmdProc] Executing command 'Rebuild project'")
        Assert-SelfTestThrows `
            -Message "post-load prohibited command $command" `
            -Action {
                Assert-IsolatedRebuildSessionContract `
                    -AppendedLogText $mutated `
                    -CanonicalProjectPath $project
            }
    }

    Assert-SelfTestThrows `
        -Message '244 checkpoint with exit 2' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $CheckpointClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes244 -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E wrong diff count' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.decision.semanticEquivalenceProven = $false
            $bad.comparison.changedByteCount = 98
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E target record mismatch' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.decision.semanticEquivalenceProven = $false
            $bad.gateDTargetRecords.allEqual = $false
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'Networks drift blocker' `
        -Action {
            $badNetworks = [pscustomobject]@{
                bytes = 242363L
                sha256 = ('B' * 64)
            }
            Get-FinalDecision `
                -Report $exactReport -ComparatorExitCode 0 `
                -ClassesIdentity $classes244 -NetworksIdentity $badNetworks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'comparator exit 4 blocked' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 4 `
                -Disposition 'BLOCKED_INVALID_INPUT'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 4 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E contradictory checkpoint acceptance' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.decision.checkpointIdentityAccepted = $true
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'third hash contradictory byteExact' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $classesThird.sha256 `
                -ExitCode 3 `
                -Disposition 'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
            $bad.comparison.byteExact = $true
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 3 `
                -ClassesIdentity $classesThird -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'comparator candidate snapshot path mismatch' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.candidate.path = 'wrong/Classes.lcb'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'exact checkpoint nonzero diff aggregate' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $CheckpointClassesSha256 `
                -ExitCode 0 `
                -Disposition 'EXACT_CHECKPOINT_MATCH'
            $bad.comparison.changedByteCount = 99L
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 0 `
                -ClassesIdentity $classes244 -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'global record equality approval guard false' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $CheckpointClassesSha256 `
                -ExitCode 0 `
                -Disposition 'EXACT_CHECKPOINT_MATCH'
            $bad.decision.recordEqualityCannotApproveArtifact = $false
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 0 `
                -ClassesIdentity $classes244 -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E equal fake target record identity' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.gateDTargetRecords.records[0].checkpoint.bytes = 1L
            $bad.gateDTargetRecords.records[0].candidate.bytes = 1L
            $bad.gateDTargetRecords.records[0].checkpoint.sha256 = ('0' * 64)
            $bad.gateDTargetRecords.records[0].candidate.sha256 = ('0' * 64)
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E bogus target record parser' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.gateDTargetRecords.records[0].parser = 'bogus'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E changed-owner boolean string false' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.comparison.changedOwnersAreFrozenOpaqueSubset = 'false'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E header exact boolean string false' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.recordParser.headerSourceInventory.exact = 'false'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E first-special exact false' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.recordParser.firstSpecialRecord.exact = $false
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E empty compared-fields oracle' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.recordParser.headerSourceInventory.comparedFields = @()
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E contradictory comparison semantics' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.comparison.equalLength = $false
            $bad.comparison.lengthDelta = 99L
            $bad.comparison.alignment = 'bogus'
            $bad.comparison.changedByteCountDefined = $false
            $bad.comparison.proprietaryFieldSemanticsDecoded = $true
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E emptied changed-owner and diff-run evidence' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad.changedCheckpointOwners = @()
            $bad.diffRuns = @()
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message '6E extra top-level JSON key' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $KnownRebuiltClassesSha256 `
                -ExitCode 2 `
                -Disposition 'REVIEW_REQUIRED_OPAQUE_VENDOR_DRIFT'
            $bad | Add-Member -NotePropertyName unexpected -NotePropertyValue $true
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 2 `
                -ClassesIdentity $classes6E -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'third hash comparison boolean string' `
        -Action {
            $bad = New-SyntheticComparisonReport `
                -CandidateSha256 $classesThird.sha256 `
                -ExitCode 3 `
                -Disposition 'REJECTED_BOUNDARY_OR_CONTRACT_DRIFT'
            $bad.comparison.equalLength = 'false'
            Get-FinalDecision `
                -Report $bad -ComparatorExitCode 3 `
                -ClassesIdentity $classesThird -NetworksIdentity $networks `
                -ExpectedCandidatePath $syntheticCandidatePath `
                -ComparisonOracle $syntheticComparisonOracle
        }
    Assert-SelfTestThrows `
        -Message 'identity mismatch' `
        -Action {
            Assert-IdentityValue `
                -Actual $classes244 `
                -ExpectedBytes 1 `
                -ExpectedSha256 $CheckpointClassesSha256 `
                -IdentityOwner 'synthetic identity'
        }

    $tempBase = Get-NormalizedFullPath -Path ([IO.Path]::GetTempPath())
    $tempRoot = Join-Path `
        $tempBase `
        ('lasal-finalizer-selftest-' + [Guid]::NewGuid().ToString('N'))
    $selfStage = $null
    $junctionPath = $null
    $selfTestAdsPresent = $false
    $selfTestDirectoryAdsPresent = $false
    $extraStagePath = $null
    $selfMarkerOriginalBytes = $null
    $selfMarkerNeedsRestore = $false
    try {
        [void][IO.Directory]::CreateDirectory($tempRoot)
        Assert-NoReparsePointChain `
            -Root $tempBase `
            -Path $tempRoot `
            -IncludeLeaf `
            -PathOwner 'self-test temp root'
        $stageCountBeforeMarkerFailure = @([IO.Directory]::GetDirectories(
                $tempRoot,
                '.finalize-stage-*',
                [IO.SearchOption]::TopDirectoryOnly)).Count
        Assert-SelfTestThrows `
            -Message 'new-stage marker failure cleanup' `
            -Action {
                New-OwnedStageDirectory `
                    -OwnerToken 'forced-marker-failure' `
                    -StageRoot $tempRoot `
                    -ForceMarkerWriteFailureForSelfTest
            }
        $stageCountAfterMarkerFailure = @([IO.Directory]::GetDirectories(
                $tempRoot,
                '.finalize-stage-*',
                [IO.SearchOption]::TopDirectoryOnly)).Count
        Assert-SelfTestTrue `
            -Condition ($stageCountAfterMarkerFailure -eq $stageCountBeforeMarkerFailure) `
            -Message 'new-stage marker failure left no orphan'

        $canonicalFixtureValue = "A&B'<>" + [char]0xD55C
        $canonicalFixture = [ordered]@{
            schema = 'ComparatorCanonicalSelfTest/v1'
            value = $canonicalFixtureValue
        }
        [byte[]]$canonicalFixtureBytes =
            ConvertTo-ComparatorCanonicalJsonBytes -Value $canonicalFixture
        $canonicalFixturePath = Join-Path $tempRoot 'canonical.json'
        Write-CreateNewBytes `
            -Path $canonicalFixturePath `
            -Bytes $canonicalFixtureBytes `
            -FileOwner 'canonical comparator JSON fixture'
        $canonicalReadback = Read-StrictJsonFile `
            -Path $canonicalFixturePath `
            -FileOwner 'canonical comparator JSON fixture' `
            -RequireComparatorCanonicalRoundTrip
        Assert-SelfTestTrue `
            -Condition (
                $canonicalReadback.schema -ceq 'ComparatorCanonicalSelfTest/v1' -and
                $canonicalReadback.value -ceq $canonicalFixtureValue) `
            -Message 'comparator canonical JSON exact roundtrip'
        $prettyFixturePath = Join-Path $tempRoot 'pretty.json'
        Write-CreateNewBytes `
            -Path $prettyFixturePath `
            -Bytes $Utf8NoBom.GetBytes("{`n  `"schema`": `"pretty`"`n}`n") `
            -FileOwner 'noncanonical pretty JSON fixture'
        Assert-SelfTestThrows `
            -Message 'noncanonical pretty comparator JSON rejection' `
            -Action {
                Read-StrictJsonFile `
                    -Path $prettyFixturePath `
                    -FileOwner 'noncanonical pretty JSON fixture' `
                    -RequireComparatorCanonicalRoundTrip
            }
        $duplicateFixturePath = Join-Path $tempRoot 'duplicate.json'
        Write-CreateNewBytes `
            -Path $duplicateFixturePath `
            -Bytes $Utf8NoBom.GetBytes('{"schema":"one","schema":"two"}' + "`n") `
            -FileOwner 'duplicate-key JSON fixture'
        Assert-SelfTestThrows `
            -Message 'duplicate-key comparator JSON rejection' `
            -Action {
                Read-StrictJsonFile `
                    -Path $duplicateFixturePath `
                    -FileOwner 'duplicate-key JSON fixture' `
                    -RequireComparatorCanonicalRoundTrip
            }
        [IO.File]::Delete($canonicalFixturePath)
        [IO.File]::Delete($prettyFixturePath)
        [IO.File]::Delete($duplicateFixturePath)

        $nativeFixturePath = Join-Path $tempRoot 'native-exit-fixture.ps1'
        Write-CreateNewBytes `
            -Path $nativeFixturePath `
            -Bytes $Utf8NoBom.GetBytes("Write-Output 'native fixture'`nexit 3`n") `
            -FileOwner 'native command preference fixture'
        $nativePreferenceExists = Test-Path Variable:PSNativeCommandUseErrorActionPreference
        $nativePreferenceSaved = $null
        if ($nativePreferenceExists) {
            $nativePreferenceSaved = $PSNativeCommandUseErrorActionPreference
            $PSNativeCommandUseErrorActionPreference = $true
        }
        try {
            $nativeFixtureRun = Invoke-IsolatedPowerShellTool `
                -ScriptPath $nativeFixturePath `
                -Arguments @() `
                -ToolOwner 'native preference fixture'
            Assert-SelfTestTrue `
                -Condition (
                    $nativeFixtureRun.exitCode -eq 3 -and
                    $nativeFixtureRun.outputText -match 'native fixture' -and
                    ((-not $nativePreferenceExists) -or
                        $PSNativeCommandUseErrorActionPreference)) `
                -Message 'native nonzero exit captured with caller preference preserved'
        }
        finally {
            if ($nativePreferenceExists) {
                $PSNativeCommandUseErrorActionPreference = $nativePreferenceSaved
            }
        }
        [IO.File]::Delete($nativeFixturePath)
        $selfStage = New-OwnedStageDirectory `
            -OwnerToken ('selftest-' + [Guid]::NewGuid().ToString('N')) `
            -StageRoot $tempRoot
        $selfMarkerIdentity = Assert-OwnedStageMarkerContract -Stage $selfStage
        Assert-SelfTestTrue `
            -Condition ($selfMarkerIdentity.bytes -gt 0) `
            -Message 'exact owner-marker content/token contract'
        Assert-SelfTestThrows `
            -Message 'owner-marker token mismatch' `
            -Action {
                $wrongStage = [pscustomobject]@{
                    path = $selfStage.path
                    name = $selfStage.name
                    markerPath = $selfStage.markerPath
                    ownerToken = 'wrong-owner-token'
                    stageRoot = $selfStage.stageRoot
                }
                Assert-OwnedStageMarkerContract -Stage $wrongStage
            }
        $wrongCleanupStage = [pscustomobject]@{
            path = $selfStage.path
            name = $selfStage.name
            markerPath = $selfStage.markerPath
            ownerToken = 'wrong-cleanup-token'
            stageRoot = $selfStage.stageRoot
        }
        Assert-SelfTestThrows `
            -Message 'cleanup owner-token mismatch refusal' `
            -Action {
                Remove-ExactOwnedStageDirectory `
                    -Stage $wrongCleanupStage `
                    -StageRoot $tempRoot
            }
        Assert-SelfTestTrue `
            -Condition (Test-Path -LiteralPath $selfStage.path -PathType Container) `
            -Message 'cleanup owner-token refusal preserved stage'

        [byte[]]$selfMarkerOriginalBytes = [IO.File]::ReadAllBytes(
            $selfStage.markerPath)
        [IO.File]::WriteAllBytes(
            $selfStage.markerPath,
            $Utf8NoBom.GetBytes("{`"schema`":`"mutated`"}`n"))
        $selfMarkerNeedsRestore = $true
        Assert-SelfTestThrows `
            -Message 'mutated owner-marker cleanup refusal' `
            -Action {
                Remove-ExactOwnedStageDirectory `
                    -Stage $selfStage `
                    -StageRoot $tempRoot
            }
        Assert-SelfTestTrue `
            -Condition (Test-Path -LiteralPath $selfStage.path -PathType Container) `
            -Message 'mutated owner-marker cleanup preserved stage'
        [IO.File]::WriteAllBytes($selfStage.markerPath, $selfMarkerOriginalBytes)
        $selfMarkerNeedsRestore = $false
        [void](Assert-OwnedStageMarkerContract -Stage $selfStage)

        $sentinelPath = Join-Path $selfStage.path $ClassesSnapshotName
        [byte[]]$sentinelBytes = @(1, 3, 5, 7, 9)
        Write-CreateNewBytes `
            -Path $sentinelPath `
            -Bytes $sentinelBytes `
            -FileOwner 'self-test sentinel'
        Assert-SelfTestTrue `
            -Condition (
                (Get-BytesSha256 -Bytes ([IO.File]::ReadAllBytes($sentinelPath))) -ceq
                (Get-BytesSha256 -Bytes $sentinelBytes)) `
            -Message 'exact-owned stage CreateNew/readback'
        Assert-SelfTestThrows `
            -Message 'CreateNew overwrite rejection' `
            -Action {
                Write-CreateNewBytes `
                    -Path $sentinelPath `
                    -Bytes ([byte[]]@(2, 4, 6)) `
                    -FileOwner 'self-test overwrite sentinel'
            }
        Assert-SelfTestTrue `
            -Condition (
                (Get-BytesSha256 -Bytes ([IO.File]::ReadAllBytes($sentinelPath))) -ceq
                (Get-BytesSha256 -Bytes $sentinelBytes)) `
            -Message 'CreateNew rejection preserved sentinel'
        $exactSelfStageReports = @(Get-ExactStageInventoryReports `
                -StagePath $selfStage.path `
                -ExpectedFileNames @($OwnerMarkerName, $ClassesSnapshotName) `
                -ArtifactRoot $tempRoot)
        Assert-SelfTestTrue `
            -Condition ($exactSelfStageReports.Count -eq 2) `
            -Message 'exact stage inventory accepted expected files'

        if (($PSVersionTable.PSEdition -ceq 'Core') -and
            ($PSVersionTable.PSVersion.Major -ge 7)) {
            Set-Content `
                -LiteralPath $selfStage.path `
                -Stream 'finalizer-directory-selftest' `
                -Value 'directory-alternate' `
                -NoNewline
            $selfTestDirectoryAdsPresent = $true
            Assert-SelfTestThrows `
                -Message 'PS7 directory alternate data stream inventory rejection' `
                -Action {
                    Get-ExactStageInventoryReports `
                        -StagePath $selfStage.path `
                        -ExpectedFileNames @(
                            $OwnerMarkerName,
                            $ClassesSnapshotName) `
                        -ArtifactRoot $tempRoot
                }
            Assert-SelfTestThrows `
                -Message 'PS7 directory alternate data stream cleanup refusal' `
                -Action {
                    Remove-ExactOwnedStageDirectory `
                        -Stage $selfStage `
                        -StageRoot $tempRoot
                }
            Assert-SelfTestTrue `
                -Condition (
                    Test-Path -LiteralPath $selfStage.path -PathType Container) `
                -Message 'PS7 directory ADS cleanup preserved stage'
            [IO.File]::Delete(
                $selfStage.path + ':finalizer-directory-selftest')
            $selfTestDirectoryAdsPresent = $false
            $postDirectoryAdsReports = @(Get-ExactStageInventoryReports `
                    -StagePath $selfStage.path `
                    -ExpectedFileNames @(
                        $OwnerMarkerName,
                        $ClassesSnapshotName) `
                    -ArtifactRoot $tempRoot)
            Assert-SelfTestTrue `
                -Condition ($postDirectoryAdsReports.Count -eq 2) `
                -Message 'PS7 stage inventory accepted after directory ADS removal'
        }

        Set-Content `
            -LiteralPath $sentinelPath `
            -Stream 'finalizer-selftest' `
            -Value 'alternate' `
            -NoNewline
        $selfTestAdsPresent = $true
        Assert-SelfTestThrows `
            -Message 'alternate data stream inventory rejection' `
            -Action {
                Get-ExactStageInventoryReports `
                    -StagePath $selfStage.path `
                    -ExpectedFileNames @($OwnerMarkerName, $ClassesSnapshotName) `
                    -ArtifactRoot $tempRoot
            }
        Assert-SelfTestThrows `
            -Message 'alternate data stream cleanup refusal' `
            -Action {
                Remove-ExactOwnedStageDirectory `
                    -Stage $selfStage `
                    -StageRoot $tempRoot
            }
        Assert-SelfTestTrue `
            -Condition (Test-Path -LiteralPath $selfStage.path -PathType Container) `
            -Message 'alternate data stream cleanup preserved stage'
        Remove-Item `
            -LiteralPath $sentinelPath `
            -Stream 'finalizer-selftest' `
            -ErrorAction Stop
        $selfTestAdsPresent = $false
        $postAdsStageReports = @(Get-ExactStageInventoryReports `
                -StagePath $selfStage.path `
                -ExpectedFileNames @($OwnerMarkerName, $ClassesSnapshotName) `
                -ArtifactRoot $tempRoot)
        Assert-SelfTestTrue `
            -Condition ($postAdsStageReports.Count -eq 2) `
            -Message 'stage inventory accepted after exact ADS removal'
        $extraStagePath = Join-Path $selfStage.path 'unexpected.bin'
        Write-CreateNewBytes `
            -Path $extraStagePath `
            -Bytes ([byte[]]@(8, 8, 8)) `
            -FileOwner 'self-test unexpected stage file'
        Assert-SelfTestThrows `
            -Message 'extra stage file inventory rejection' `
            -Action {
                Get-ExactStageInventoryReports `
                    -StagePath $selfStage.path `
                    -ExpectedFileNames @($OwnerMarkerName, $ClassesSnapshotName) `
                    -ArtifactRoot $tempRoot
            }
        Assert-SelfTestThrows `
            -Message 'unknown cleanup file refusal' `
            -Action {
                Remove-ExactOwnedStageDirectory `
                    -Stage $selfStage `
                    -StageRoot $tempRoot
            }
        Assert-SelfTestTrue `
            -Condition (Test-Path -LiteralPath $extraStagePath -PathType Leaf) `
            -Message 'unknown cleanup file remained for manual review'
        [IO.File]::Delete($extraStagePath)
        $extraStagePath = $null
        $reports = @(Get-StageArtifactReports `
                -StagePath $selfStage.path `
                -FileNames @($ClassesSnapshotName) `
                -ArtifactRoot $tempRoot)
        Assert-ArtifactReportSequencesEqual `
            -Expected $reports `
            -Actual $reports `
            -SequenceOwner 'self-test unchanged artifact report'
        Assert-SelfTestTrue `
            -Condition $true `
            -Message 'artifact report unchanged sequence'
        $mutatedReport = @([pscustomobject]@{
                fileName = $reports[0].fileName
                bytes = $reports[0].bytes
                sha256 = ('F' * 64)
            })
        Assert-SelfTestThrows `
            -Message 'artifact report mutation' `
            -Action {
                Assert-ArtifactReportSequencesEqual `
                    -Expected $reports `
                    -Actual $mutatedReport `
                    -SequenceOwner 'self-test mutated artifact report'
            }

        $namedOrderedExpected = @([ordered]@{
                relativePath = 'fixture/report.bin'
                bytes = 3L
                sha256 = ('A' * 64)
            })
        $namedOrderedActual = @([ordered]@{
                relativePath = 'fixture/report.bin'
                bytes = 3L
                sha256 = ('A' * 64)
            })
        Assert-NamedIdentityReportSequencesEqual `
            -Expected $namedOrderedExpected `
            -Actual $namedOrderedActual `
            -NameProperty 'relativePath' `
            -SequenceOwner 'self-test ordered identity report'
        Assert-SelfTestTrue `
            -Condition $true `
            -Message 'ordered identity report exact sequence'
        $namedOrderedValueMismatch = @([ordered]@{
                relativePath = 'fixture/report.bin'
                bytes = 4L
                sha256 = ('A' * 64)
            })
        Assert-SelfTestThrows `
            -Message 'ordered identity report value mismatch' `
            -Action {
                Assert-NamedIdentityReportSequencesEqual `
                    -Expected $namedOrderedExpected `
                    -Actual $namedOrderedValueMismatch `
                    -NameProperty 'relativePath' `
                    -SequenceOwner 'self-test ordered value mismatch'
            }
        $namedOrderedMissing = @([ordered]@{
                bytes = 3L
                sha256 = ('A' * 64)
            })
        Assert-SelfTestThrows `
            -Message 'ordered identity report missing field' `
            -Action {
                Assert-NamedIdentityReportSequencesEqual `
                    -Expected $namedOrderedExpected `
                    -Actual $namedOrderedMissing `
                    -NameProperty 'relativePath' `
                    -SequenceOwner 'self-test ordered missing field'
            }
        $namedOrderedWrongCase = @([ordered]@{
                RelativePath = 'fixture/report.bin'
                bytes = 3L
                sha256 = ('A' * 64)
            })
        Assert-SelfTestThrows `
            -Message 'ordered identity report wrong-case field' `
            -Action {
                Assert-NamedIdentityReportSequencesEqual `
                    -Expected $namedOrderedExpected `
                    -Actual $namedOrderedWrongCase `
                    -NameProperty 'relativePath' `
                    -SequenceOwner 'self-test ordered wrong-case field'
            }

        $junctionTarget = Join-Path $tempRoot 'junction-target'
        [void][IO.Directory]::CreateDirectory($junctionTarget)
        $junctionPath = Join-Path $selfStage.path 'junction'
        [void](New-Item `
                -ItemType Junction `
                -Path $junctionPath `
                -Target $junctionTarget `
                -ErrorAction Stop)
        Assert-SelfTestThrows `
            -Message 'reparse-point ancestor rejection' `
            -Action {
                Assert-NoReparsePointChain `
                    -Root $tempRoot `
                    -Path (Join-Path $junctionPath 'child.bin') `
                    -PathOwner 'self-test junction child'
            }
        if (([IO.File]::GetAttributes($junctionPath) -band
                [IO.FileAttributes]::ReparsePoint) -eq 0) {
            throw 'self-test junction was not a reparse point.'
        }
        [IO.Directory]::Delete($junctionPath)
        $junctionPath = $null
        Remove-ExactOwnedStageDirectory `
            -Stage $selfStage `
            -StageRoot $tempRoot
        Assert-SelfTestTrue `
            -Condition (-not (Test-Path -LiteralPath $selfStage.path)) `
            -Message 'exact-owned stage cleanup left no residue'
        $selfStage = $null
    }
    finally {
        if ($selfTestDirectoryAdsPresent -and
            $null -ne $selfStage -and
            (Test-Path -LiteralPath $selfStage.path -PathType Container)) {
            [IO.File]::Delete(
                $selfStage.path + ':finalizer-directory-selftest')
            $selfTestDirectoryAdsPresent = $false
        }
        if ($selfTestAdsPresent -and
            $null -ne $selfStage -and
            (Test-Path -LiteralPath $sentinelPath -PathType Leaf)) {
            Remove-Item `
                -LiteralPath $sentinelPath `
                -Stream 'finalizer-selftest' `
                -ErrorAction SilentlyContinue
            $selfTestAdsPresent = $false
        }
        if ($null -ne $extraStagePath -and
            (Test-Path -LiteralPath $extraStagePath -PathType Leaf)) {
            [IO.File]::Delete($extraStagePath)
        }
        if ($selfMarkerNeedsRestore -and
            $null -ne $selfStage -and
            $null -ne $selfMarkerOriginalBytes -and
            (Test-Path -LiteralPath $selfStage.markerPath -PathType Leaf)) {
            [IO.File]::WriteAllBytes(
                $selfStage.markerPath,
                [byte[]]$selfMarkerOriginalBytes)
            $selfMarkerNeedsRestore = $false
        }
        if ($null -ne $junctionPath -and
            (Test-Path -LiteralPath $junctionPath)) {
            $junctionAttributes = [IO.File]::GetAttributes($junctionPath)
            if (($junctionAttributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                [IO.Directory]::Delete($junctionPath)
            }
        }
        if ($null -ne $selfStage -and
            (Test-Path -LiteralPath $selfStage.path -PathType Container)) {
            Remove-ExactOwnedStageDirectory `
                -Stage $selfStage `
                -StageRoot $tempRoot
        }
        if (Test-Path -LiteralPath $tempRoot -PathType Container) {
            if (-not (Test-PathInsideRoot `
                    -Root $tempBase `
                    -Path $tempRoot)) {
                throw 'self-test temp cleanup escaped the temp root.'
            }
            Assert-NoReparsePointDescendants `
                -Directory $tempRoot `
                -DirectoryOwner 'self-test temp cleanup'
            [IO.Directory]::Delete($tempRoot, $true)
        }
    }

    Write-Output (
        "PASS $Owner.SelfTest Positive=$script:selfTestPositive " +
        "Negative=$script:selfTestNegative")
}

try {
    if ($PSCmdlet.ParameterSetName -ceq 'SelfTest') {
        Invoke-FinalizerSelfTest
        exit 0
    }
    $productionEngine = Assert-PowerShell7FinalizationEngine `
        -Phase 'top-level production entry'
    $finalExitCode = Invoke-CandidateFinalization `
        -RequestedRepositoryRoot $RepositoryRoot `
        -RequestedLasalLogPath $LasalLogPath `
        -ProductionEngine $productionEngine
    exit $finalExitCode
}
catch {
    [Console]::Error.WriteLine("BLOCKED: $($_.Exception.Message)")
    exit 4
}

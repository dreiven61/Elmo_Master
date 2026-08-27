[CmdletBinding(DefaultParameterSetName = 'Export')]
param(
    [Parameter(ParameterSetName = 'Export')]
    [string]$JournalPath = '',

    [Parameter(Mandatory = $true, ParameterSetName = 'Export')]
    [string]$OutputPath,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Magic = 'ELMOASOM1'
$FormatVersion = [uint32]1
$MaximumFileLength = 16384
$KnownEvidenceMask = [uint32]0x3F
$VerifyEvidence = [uint32]0x0C
$TerminalEvidence = [uint32]0x30
$Utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
$Utf8 = New-Object System.Text.UTF8Encoding($false)

function Require {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw [IO.InvalidDataException]::new($Message)
    }
}

function Get-Sha256Hex {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    $algorithm = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString($algorithm.ComputeHash($Bytes))).Replace('-', '')
    }
    finally {
        $algorithm.Dispose()
    }
}

function Fixed-HexEquals {
    param([string]$Left, [string]$Right)

    if ($null -eq $Left -or $null -eq $Right -or $Left.Length -ne $Right.Length) {
        return $false
    }
    $difference = 0
    for ($index = 0; $index -lt $Left.Length; $index++) {
        $difference = $difference -bor ([int][char]::ToUpperInvariant($Left[$index]) -bxor [int][char]::ToUpperInvariant($Right[$index]))
    }
    return $difference -eq 0
}

function Read-Field {
    param(
        [Parameter(Mandatory = $true)][string[]]$Lines,
        [Parameter(Mandatory = $true)][ref]$Cursor,
        [Parameter(Mandatory = $true)][string]$Name
    )

    Require ($Cursor.Value -lt $Lines.Length) "SetOperationMode journal is truncated before $Name."
    $prefix = $Name + '='
    $line = $Lines[$Cursor.Value]
    $Cursor.Value++
    Require ($line.StartsWith($prefix, [StringComparison]::Ordinal)) "SetOperationMode journal field order is invalid: $Name."
    return $line.Substring($prefix.Length)
}

function Parse-UInt32 {
    param([string]$Text, [string]$Name)
    [uint32]$value = 0
    Require ([uint32]::TryParse($Text, [Globalization.NumberStyles]::None, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid UInt32 journal field: $Name."
    return $value
}

function Parse-UInt16 {
    param([string]$Text, [string]$Name)
    [uint16]$value = 0
    Require ([uint16]::TryParse($Text, [Globalization.NumberStyles]::None, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid UInt16 journal field: $Name."
    return $value
}

function Parse-Int32 {
    param([string]$Text, [string]$Name)
    [int32]$value = 0
    Require ([int32]::TryParse($Text, [Globalization.NumberStyles]::Integer, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid Int32 journal field: $Name."
    return $value
}

function Parse-Int16 {
    param([string]$Text, [string]$Name)
    [int16]$value = 0
    Require ([int16]::TryParse($Text, [Globalization.NumberStyles]::Integer, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid Int16 journal field: $Name."
    return $value
}

function Parse-SByte {
    param([string]$Text, [string]$Name)
    [sbyte]$value = 0
    Require ([sbyte]::TryParse($Text, [Globalization.NumberStyles]::Integer, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid SByte journal field: $Name."
    return $value
}

function Parse-Int64 {
    param([string]$Text, [string]$Name)
    [int64]$value = 0
    Require ([int64]::TryParse($Text, [Globalization.NumberStyles]::None, [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) "Invalid Int64 journal field: $Name."
    return $value
}

function Decode-String {
    param([string]$Text, [string]$Name)
    try {
        return $Utf8Strict.GetString([Convert]::FromBase64String($Text))
    }
    catch {
        throw [IO.InvalidDataException]::new("Invalid Base64/UTF-8 journal field: $Name.", $_.Exception)
    }
}

function Get-StateName {
    param([int]$Value)
    switch ($Value) {
        1 { return 'ArmedBeforeDispatch' }
        2 { return 'RecoveryRequired' }
        3 { return 'Resolved' }
        4 { return 'TerminalOutcomeObserved' }
        default { throw [IO.InvalidDataException]::new("Invalid SetOperationMode recovery state: $Value.") }
    }
}

function Get-RecordStateName {
    param([uint16]$Value)
    switch ($Value) {
        1 { return 'Running' }
        2 { return 'Succeeded' }
        3 { return 'Failed' }
        4 { return 'Aborted' }
        default { throw [IO.InvalidDataException]::new("Invalid SetOperationMode outcome record state: $Value.") }
    }
}

function Parse-JournalBytes {
    param([Parameter(Mandatory = $true)][byte[]]$Bytes)

    Require ($Bytes.Length -gt 0 -and $Bytes.Length -le $MaximumFileLength) 'SetOperationMode journal length is invalid.'
    $text = $Utf8Strict.GetString($Bytes)
    Require ($text.IndexOf("`r", [StringComparison]::Ordinal) -lt 0) 'SetOperationMode journal must use canonical LF framing.'
    Require ($text.EndsWith("`n", [StringComparison]::Ordinal)) 'SetOperationMode journal must end with LF.'

    $lines = $text.Split([char[]]@([char]10), [StringSplitOptions]::None)
    Require ($lines.Length -ge 4 -and $lines[$lines.Length - 1].Length -eq 0) 'SetOperationMode journal framing is invalid.'
    $checksumIndex = $lines.Length - 2
    $checksumLine = $lines[$checksumIndex]
    Require ($checksumLine.StartsWith('SHA256=', [StringComparison]::Ordinal) -and $checksumLine.Length -eq 71) 'SetOperationMode journal checksum line is invalid.'
    $expectedChecksum = $checksumLine.Substring(7)
    Require ([regex]::IsMatch($expectedChecksum, '^[0-9A-Fa-f]{64}$')) 'SetOperationMode journal checksum is not hexadecimal.'
    $payload = [string]::Join("`n", $lines[0..($checksumIndex - 1)]) + "`n"
    $actualChecksum = Get-Sha256Hex -Bytes $Utf8.GetBytes($payload)
    Require (Fixed-HexEquals $expectedChecksum $actualChecksum) 'SetOperationMode journal checksum mismatch.'

    # Framing validation intentionally includes the checksum line and final
    # empty LF sentinel above. Field parsing must not pass that empty string
    # through a mandatory string[] parameter under Windows PowerShell 5.1.
    $lines = [string[]]@($lines[0..($checksumIndex - 1)])

    [int]$cursorValue = 0
    $cursor = [ref]$cursorValue
    Require ($lines[$cursor.Value] -ceq $Magic) 'SetOperationMode journal magic is invalid.'
    $cursor.Value++

    $version = Parse-UInt32 (Read-Field $lines $cursor 'FormatVersion') 'FormatVersion'
    Require ($version -eq $FormatVersion) 'Unsupported SetOperationMode journal format version.'

    $identityText = Read-Field $lines $cursor 'Identity'
    [Guid]$identity = [Guid]::Empty
    Require ([Guid]::TryParseExact($identityText, 'N', [ref]$identity)) 'SetOperationMode journal identity is invalid.'

    $stateValue = Parse-Int32 (Read-Field $lines $cursor 'State') 'State'
    $stateName = Get-StateName $stateValue
    $revision = Parse-UInt32 (Read-Field $lines $cursor 'Revision') 'Revision'
    Require ($revision -ne 0) 'SetOperationMode journal revision must be nonzero.'
    $endpointIp = Decode-String (Read-Field $lines $cursor 'EndpointIp') 'EndpointIp'
    $endpointPort = Parse-Int32 (Read-Field $lines $cursor 'EndpointPort') 'EndpointPort'
    Require ($endpointPort -ge 1 -and $endpointPort -le 65535) 'SetOperationMode journal endpoint port is invalid.'
    $axisName = Decode-String (Read-Field $lines $cursor 'AxisName') 'AxisName'
    Require (-not [string]::IsNullOrWhiteSpace($axisName)) 'SetOperationMode journal axis name is empty.'
    $schemaVersion = Parse-UInt16 (Read-Field $lines $cursor 'SchemaVersion') 'SchemaVersion'
    Require ($schemaVersion -eq 1) 'SetOperationMode journal schema version must be 1.'
    $originalRequestId = Parse-UInt32 (Read-Field $lines $cursor 'OriginalRequestId') 'OriginalRequestId'
    Require ($originalRequestId -ne 0) 'SetOperationMode journal OriginalRequestId must be nonzero.'
    $diagnosticsBuild = Parse-UInt32 (Read-Field $lines $cursor 'DiagnosticsBuild') 'DiagnosticsBuild'
    $diagnosticsBootId = Parse-UInt32 (Read-Field $lines $cursor 'DiagnosticsBootId') 'DiagnosticsBootId'
    $mapRevision = Parse-UInt32 (Read-Field $lines $cursor 'MapRevision') 'MapRevision'
    Require ($diagnosticsBuild -ne 0 -and $diagnosticsBootId -ne 0 -and $mapRevision -ne 0) 'SetOperationMode journal Build/BootId/MapRevision must all be nonzero.'
    $intent0 = Parse-UInt32 (Read-Field $lines $cursor 'ClientIntentId0') 'ClientIntentId0'
    $intent1 = Parse-UInt32 (Read-Field $lines $cursor 'ClientIntentId1') 'ClientIntentId1'
    $intent2 = Parse-UInt32 (Read-Field $lines $cursor 'ClientIntentId2') 'ClientIntentId2'
    $intent3 = Parse-UInt32 (Read-Field $lines $cursor 'ClientIntentId3') 'ClientIntentId3'
    Require (($intent0 -bor $intent1 -bor $intent2 -bor $intent3) -ne 0) 'SetOperationMode journal ClientIntentId must not be all zero.'
    $axisReference = Parse-UInt16 (Read-Field $lines $cursor 'AxisReference') 'AxisReference'
    Require ($axisReference -ge 1 -and $axisReference -le 4) 'SetOperationMode journal axis reference is outside 1..4.'
    $requestedModeRaw = Parse-SByte (Read-Field $lines $cursor 'RequestedModeRaw') 'RequestedModeRaw'
    Require ($requestedModeRaw -eq 8) 'SetOperationMode journal requested mode must be CSP=8.'
    $timeout = Parse-UInt32 (Read-Field $lines $cursor 'TimeoutMilliseconds') 'TimeoutMilliseconds'
    Require ($timeout -ne 0) 'SetOperationMode journal timeout must be nonzero.'
    $flags = Parse-UInt32 (Read-Field $lines $cursor 'Flags') 'Flags'
    Require ($flags -eq 0) 'SetOperationMode journal flags must remain zero.'
    $createdTicks = Parse-Int64 (Read-Field $lines $cursor 'CreatedUtcTicks') 'CreatedUtcTicks'
    $updatedTicks = Parse-Int64 (Read-Field $lines $cursor 'UpdatedUtcTicks') 'UpdatedUtcTicks'
    Require ($createdTicks -ge [DateTime]::MinValue.Ticks -and $createdTicks -le [DateTime]::MaxValue.Ticks) 'SetOperationMode journal CreatedUtcTicks is invalid.'
    Require ($updatedTicks -ge $createdTicks -and $updatedTicks -le [DateTime]::MaxValue.Ticks) 'SetOperationMode journal UpdatedUtcTicks is invalid.'
    $retirementRequestId = Parse-UInt32 (Read-Field $lines $cursor 'RetirementRequestId') 'RetirementRequestId'
    $hasTerminalProof = Parse-Int32 (Read-Field $lines $cursor 'HasTerminalProof') 'HasTerminalProof'
    Require ($hasTerminalProof -eq 1) 'MODE-11 evidence export requires durable terminal proof.'

    $queryRequestId = Parse-UInt32 (Read-Field $lines $cursor 'QueryRequestId') 'QueryRequestId'
    Require ($queryRequestId -ne 0) 'Terminal outcome QueryRequestId must be nonzero.'
    $recordStateValue = Parse-UInt16 (Read-Field $lines $cursor 'RecordState') 'RecordState'
    $recordStateName = Get-RecordStateName $recordStateValue
    Require ($recordStateValue -ne 1) 'MODE-11 evidence export refuses a running outcome.'
    $observedModeRaw = Parse-SByte (Read-Field $lines $cursor 'ObservedModeRaw') 'ObservedModeRaw'
    $originalCommandStatus = Parse-UInt16 (Read-Field $lines $cursor 'OriginalCommandStatus') 'OriginalCommandStatus'
    $originalErrorId = Parse-Int16 (Read-Field $lines $cursor 'OriginalErrorId') 'OriginalErrorId'
    $originalDetailCode = Parse-UInt32 (Read-Field $lines $cursor 'OriginalDetailCode') 'OriginalDetailCode'
    $sdoExecutorToken = Parse-UInt32 (Read-Field $lines $cursor 'SdoExecutorToken') 'SdoExecutorToken'
    $evidenceFlags = Parse-UInt32 (Read-Field $lines $cursor 'EvidenceFlags') 'EvidenceFlags'
    Require (($evidenceFlags -band (-bnot $KnownEvidenceMask)) -eq 0) 'Terminal proof contains unknown EvidenceFlags bits.'
    $startCycle = Parse-UInt32 (Read-Field $lines $cursor 'StartCycle') 'StartCycle'
    $completionCycle = Parse-UInt32 (Read-Field $lines $cursor 'CompletionCycle') 'CompletionCycle'
    $nativeCommandState = Parse-UInt32 (Read-Field $lines $cursor 'NativeCommandState') 'NativeCommandState'
    $recordGeneration = Parse-UInt32 (Read-Field $lines $cursor 'RecordGeneration') 'RecordGeneration'
    $previousModeRaw = Parse-SByte (Read-Field $lines $cursor 'PreviousModeRaw') 'PreviousModeRaw'
    $quarantineReason = Parse-UInt32 (Read-Field $lines $cursor 'QuarantineReason') 'QuarantineReason'
    $ds402StatusWord = Parse-UInt16 (Read-Field $lines $cursor 'Ds402StatusWord') 'Ds402StatusWord'
    $contextCheck = Parse-UInt32 (Read-Field $lines $cursor 'ContextCheck') 'ContextCheck'

    Require ($recordGeneration -ne 0) 'Terminal proof RecordGeneration must be nonzero.'
    Require ($completionCycle -ne 0 -and $completionCycle -ge $startCycle) 'Terminal proof completion cycle must be at or after start.'
    Require ($nativeCommandState -eq 0) 'SetOperationMode SDO proof must not expose native command state.'
    Require (($evidenceFlags -band $TerminalEvidence) -eq $TerminalEvidence) 'Terminal proof requires OwnerReleased + ExecutorReusable evidence.'
    if ($recordStateValue -eq 2) {
        Require ($originalCommandStatus -eq 0 -and $originalErrorId -eq 0 -and $originalDetailCode -eq 0) 'Successful terminal proof contains a command error.'
        Require ($observedModeRaw -eq $requestedModeRaw) 'Successful terminal proof observed mode does not match requested mode.'
        Require (($evidenceFlags -band $VerifyEvidence) -eq $VerifyEvidence) 'Successful terminal proof requires completed verify-read evidence.'
    }

    if ($stateValue -eq 3) {
        Require ($retirementRequestId -ne 0) 'Resolved SetOperationMode journal requires nonzero RetirementRequestId.'
    }
    elseif ($stateValue -eq 4) {
        Require ($retirementRequestId -eq 0) 'TerminalOutcomeObserved journal must not already contain a retirement request id.'
    }
    else {
        throw [IO.InvalidDataException]::new('MODE-11 evidence export requires TerminalOutcomeObserved or Resolved journal state.')
    }

    Require ($cursor.Value -eq $checksumIndex) 'SetOperationMode journal has trailing or missing fields.'

    $createdUtc = [DateTime]::new($createdTicks, [DateTimeKind]::Utc)
    $updatedUtc = [DateTime]::new($updatedTicks, [DateTimeKind]::Utc)
    $journalSha = Get-Sha256Hex -Bytes $Bytes
    $retirementConfirmed = $stateValue -eq 3

    return [ordered]@{
        Format = 'Elmo.SetOperationMode.WpfJournalEvidence'
        FormatVersion = 1
        JournalSha256 = $journalSha
        JournalIdentity = $identity.ToString('N')
        JournalState = $stateName
        JournalRevision = $revision
        EndpointIp = $endpointIp
        EndpointPort = $endpointPort
        AxisName = $axisName
        AxisReference = $axisReference
        RecoveryKey = [ordered]@{
            SchemaVersion = $schemaVersion
            OriginalRequestId = $originalRequestId
            DiagnosticsBuild = $diagnosticsBuild
            DiagnosticsBootId = $diagnosticsBootId
            MapRevision = $mapRevision
            ClientIntentId = @($intent0, $intent1, $intent2, $intent3)
            RequestedModeRaw = [int]$requestedModeRaw
            TimeoutMilliseconds = $timeout
            Flags = $flags
        }
        CreatedUtc = $createdUtc.ToString('o')
        UpdatedUtc = $updatedUtc.ToString('o')
        TerminalOutcome = [ordered]@{
            QueryRequestId = $queryRequestId
            RecordState = $recordStateName
            RecordStateRaw = $recordStateValue
            ObservedModeRaw = [int]$observedModeRaw
            OriginalCommandStatus = $originalCommandStatus
            OriginalErrorId = $originalErrorId
            OriginalDetailCode = $originalDetailCode
            SdoExecutorToken = $sdoExecutorToken
            EvidenceFlags = $evidenceFlags
            StartCycle = $startCycle
            CompletionCycle = $completionCycle
            NativeCommandState = $nativeCommandState
            RecordGeneration = $recordGeneration
            PreviousModeRaw = [int]$previousModeRaw
            QuarantineReason = $quarantineReason
            Ds402StatusWord = $ds402StatusWord
            ContextCheck = $contextCheck
        }
        Retirement = [ordered]@{
            RetireRequestId = $retirementRequestId
            RetiredGeneration = $(if ($retirementConfirmed) { $recordGeneration } else { 0 })
            Confirmed = $retirementConfirmed
            JournalResolvedAfterRetire = $retirementConfirmed
        }
        RunFragment = [ordered]@{
            StartRequestId = $originalRequestId
            QueryRequestId = $queryRequestId
            RetireRequestId = $retirementRequestId
            ClientIntentId = @($intent0, $intent1, $intent2, $intent3)
            RecordState = $recordStateName
            ObservedModeRaw = [int]$observedModeRaw
            OriginalCommandStatus = $originalCommandStatus
            OriginalErrorId = $originalErrorId
            OriginalDetailCode = $originalDetailCode
            NativeCommandState = $nativeCommandState
            EvidenceFlags = $evidenceFlags
            StartCycle = $startCycle
            CompletionCycle = $completionCycle
            RecordGeneration = $recordGeneration
            RetiredGeneration = $(if ($retirementConfirmed) { $recordGeneration } else { 0 })
            RetirementConfirmed = $retirementConfirmed
            JournalResolvedAfterRetire = $retirementConfirmed
        }
    }
}

function New-FixtureBytes {
    param(
        [int]$State = 3,
        [uint32]$EvidenceFlags = 60,
        [uint32]$RetirementRequestId = 13
    )

    $endpoint = [Convert]::ToBase64String($Utf8.GetBytes('127.0.0.1'))
    $axis = [Convert]::ToBase64String($Utf8.GetBytes('_LMCAxis1'))
    $lines = @(
        $Magic,
        'FormatVersion=1',
        'Identity=00112233445566778899aabbccddeeff',
        ('State=' + $State),
        'Revision=4',
        ('EndpointIp=' + $endpoint),
        'EndpointPort=4000',
        ('AxisName=' + $axis),
        'SchemaVersion=1',
        'OriginalRequestId=11',
        'DiagnosticsBuild=101',
        'DiagnosticsBootId=202',
        'MapRevision=303',
        'ClientIntentId0=1',
        'ClientIntentId1=2',
        'ClientIntentId2=3',
        'ClientIntentId3=4',
        'AxisReference=1',
        'RequestedModeRaw=8',
        'TimeoutMilliseconds=5000',
        'Flags=0',
        'CreatedUtcTicks=638916480000000000',
        'UpdatedUtcTicks=638916480030000000',
        ('RetirementRequestId=' + $RetirementRequestId),
        'HasTerminalProof=1',
        'QueryRequestId=12',
        'RecordState=2',
        'ObservedModeRaw=8',
        'OriginalCommandStatus=0',
        'OriginalErrorId=0',
        'OriginalDetailCode=0',
        'SdoExecutorToken=99',
        ('EvidenceFlags=' + $EvidenceFlags),
        'StartCycle=100',
        'CompletionCycle=110',
        'NativeCommandState=0',
        'RecordGeneration=77',
        'PreviousModeRaw=8',
        'QuarantineReason=0',
        'Ds402StatusWord=4660',
        'ContextCheck=2864434397'
    )
    $payload = [string]::Join("`n", $lines) + "`n"
    $checksum = Get-Sha256Hex -Bytes $Utf8.GetBytes($payload)
    return $Utf8.GetBytes($payload + 'SHA256=' + $checksum + "`n")
}

function Expect-Failure {
    param([Parameter(Mandatory = $true)][scriptblock]$Action, [string]$Label)
    $failed = $false
    try { & $Action } catch { $failed = $true }
    Require $failed "Negative self-test unexpectedly passed: $Label"
    Write-Host "PASS negative self-test rejected: $Label"
}

if ($SelfTest) {
    $resolved = Parse-JournalBytes -Bytes (New-FixtureBytes)
    Require ($resolved.JournalState -ceq 'Resolved') 'Resolved fixture state mismatch.'
    Require ($resolved.TerminalOutcome.RecordState -ceq 'Succeeded') 'Resolved fixture terminal state mismatch.'
    Require ($resolved.TerminalOutcome.EvidenceFlags -eq 60) 'Resolved fixture evidence flags mismatch.'
    Require ($resolved.Retirement.Confirmed) 'Resolved fixture retirement should be confirmed.'
    Require ($resolved.RunFragment.RetiredGeneration -eq 77) 'Resolved fixture generation mismatch.'
    Write-Host 'PASS positive self-test: resolved durable terminal/retirement proof export'

    $terminalOnly = Parse-JournalBytes -Bytes (New-FixtureBytes -State 4 -RetirementRequestId 0)
    Require ($terminalOnly.JournalState -ceq 'TerminalOutcomeObserved') 'Terminal-only fixture state mismatch.'
    Require (-not $terminalOnly.Retirement.Confirmed) 'Terminal-only fixture must not claim retirement.'
    Write-Host 'PASS positive self-test: terminal proof export before retirement'

    $tampered = New-FixtureBytes
    $tampered[20] = $tampered[20] -bxor 1
    Expect-Failure { [void](Parse-JournalBytes -Bytes $tampered) } 'journal checksum tamper'

    Expect-Failure { [void](Parse-JournalBytes -Bytes (New-FixtureBytes -EvidenceFlags 12)) } 'terminal proof missing owner/executor evidence'
    Expect-Failure { [void](Parse-JournalBytes -Bytes (New-FixtureBytes -State 3 -RetirementRequestId 0)) } 'resolved journal missing retirement request id'

    Write-Host 'PASS SetOperationMode journal evidence exporter self-test'
    exit 0
}

if ([string]::IsNullOrWhiteSpace($JournalPath)) {
    $JournalPath = Join-Path $env:LOCALAPPDATA 'Elmo\LasalMotionControlApiExample\AxisSetOperationModeRecoveryJournal\v1\axis-set-operation-mode-recovery.journal'
}
$journalFull = [IO.Path]::GetFullPath($JournalPath)
$outputFull = [IO.Path]::GetFullPath($OutputPath)
Require (Test-Path -LiteralPath $journalFull -PathType Leaf) "SetOperationMode journal file not found: $journalFull"
$record = Parse-JournalBytes -Bytes ([IO.File]::ReadAllBytes($journalFull))
$json = ($record | ConvertTo-Json -Depth 8)
$json = $json.Replace("`r`n", "`n").Replace("`r", "`n") + "`n"
$outputDirectory = Split-Path -Parent $outputFull
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
[IO.File]::WriteAllText($outputFull, $json, $Utf8)
Write-Host "PASS SetOperationMode durable journal checksum/terminal proof validated"
Write-Host "PASS journal state: $($record.JournalState)"
Write-Host "PASS terminal state/generation: $($record.TerminalOutcome.RecordState) / $($record.TerminalOutcome.RecordGeneration)"
Write-Host "PASS EvidenceFlags: $($record.TerminalOutcome.EvidenceFlags)"
Write-Host "PASS journal evidence JSON: $outputFull"
if (-not $record.Retirement.Confirmed) {
    Write-Host 'REVIEW_REQUIRED terminal proof exists but exact-generation retirement is not yet durably resolved'
}
exit 0

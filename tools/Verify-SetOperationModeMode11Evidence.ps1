[CmdletBinding(DefaultParameterSetName = 'Verify')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Verify')]
    [string]$EvidencePath,

    [Parameter(Mandatory = $true, ParameterSetName = 'SelfTest')]
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$KnownEvidenceMask = [uint32]0x3F
$WriteRequested = [uint32]0x01
$WriteDispatched = [uint32]0x02
$VerifyReadDispatched = [uint32]0x04
$VerifyReadCompleted = [uint32]0x08
$OwnerReleased = [uint32]0x10
$ExecutorReusable = [uint32]0x20
$TerminalSuccessMask = $VerifyReadDispatched -bor $VerifyReadCompleted -bor $OwnerReleased -bor $ExecutorReusable
$AllMutationEvidence = $TerminalSuccessMask -bor $WriteRequested -bor $WriteDispatched

function Require {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Get-RequiredProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $property = $Object.PSObject.Properties[$Name]
    Require ($null -ne $property) "Required evidence property is missing: $Name"
    return $property.Value
}

function Require-NonEmptyString {
    param($Value, [string]$Name)
    Require (($Value -is [string]) -and (-not [string]::IsNullOrWhiteSpace($Value))) "$Name must be a non-empty string."
}

function Require-Sha256 {
    param($Value, [string]$Name)
    Require-NonEmptyString $Value $Name
    Require ([regex]::IsMatch($Value, '^[0-9A-Fa-f]{64}$')) "$Name must be exactly 64 hexadecimal SHA-256 characters."
}

function Require-NonzeroUInt32 {
    param($Value, [string]$Name)
    $number = [uint64]$Value
    Require (($number -gt 0) -and ($number -le [uint32]::MaxValue)) "$Name must be a nonzero UInt32."
    return [uint32]$number
}

function Require-BoolTrue {
    param($Value, [string]$Name)
    Require (($Value -is [bool]) -and $Value) "$Name must be true."
}

function Verify-CommonEvidence {
    param($Evidence)

    $schema = [int](Get-RequiredProperty $Evidence 'SchemaVersion')
    Require ($schema -eq 1) 'SchemaVersion must be 1.'

    $caseName = [string](Get-RequiredProperty $Evidence 'Case')
    Require (($caseName -ceq 'MODE-11A') -or ($caseName -ceq 'MODE-11B')) 'Case must be exactly MODE-11A or MODE-11B.'

    $candidate = Get-RequiredProperty $Evidence 'Candidate'
    Require-NonEmptyString (Get-RequiredProperty $candidate 'Branch') 'Candidate.Branch'
    Require (([string]$candidate.Branch) -ceq 'codex/setopmode-mode11-bench-activation') 'Candidate.Branch must be the isolated MODE-11 qualification branch.'
    Require-NonEmptyString (Get-RequiredProperty $candidate 'CommitSha') 'Candidate.CommitSha'
    Require ([regex]::IsMatch([string]$candidate.CommitSha, '^[0-9A-Fa-f]{40}$')) 'Candidate.CommitSha must be a 40-character Git SHA.'
    Require (([string](Get-RequiredProperty $candidate 'DiagnosticsGate')) -ceq 'TRUE') 'Candidate.DiagnosticsGate must be TRUE.'
    $featureMask = [uint32](Get-RequiredProperty $candidate 'AdminFeatureMask')
    Require ($featureMask -eq [uint32]0x717) 'Candidate.AdminFeatureMask must be exactly 0x00000717.'
    Require-Sha256 (Get-RequiredProperty $candidate 'ActiveSourceDiffSha256') 'Candidate.ActiveSourceDiffSha256'
    Require-Sha256 (Get-RequiredProperty $candidate 'DiagnosticsSourceSha256') 'Candidate.DiagnosticsSourceSha256'
    Require-Sha256 (Get-RequiredProperty $candidate 'ControlSourceSha256') 'Candidate.ControlSourceSha256'
    Require-Sha256 (Get-RequiredProperty $candidate 'ClassesSha256') 'Candidate.ClassesSha256'
    Require-Sha256 (Get-RequiredProperty $candidate 'ProjectSha256') 'Candidate.ProjectSha256'
    Require ([uint64](Get-RequiredProperty $candidate 'ClassesBytes') -gt 0) 'Candidate.ClassesBytes must be nonzero.'
    Require ([uint64](Get-RequiredProperty $candidate 'ProjectBytes') -gt 0) 'Candidate.ProjectBytes must be nonzero.'
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $candidate 'DiagnosticsBuild') 'Candidate.DiagnosticsBuild')
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $candidate 'DiagnosticsBootId') 'Candidate.DiagnosticsBootId')
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $candidate 'MapRevision') 'Candidate.MapRevision')
    Require (([int](Get-RequiredProperty $candidate 'AxisReference')) -eq 1) 'MODE-11 qualification must start with physical AxisReference 1.'
    Require-NonEmptyString (Get-RequiredProperty $candidate 'Endpoint') 'Candidate.Endpoint'
    Require-NonEmptyString (Get-RequiredProperty $candidate 'PlcLoadTimestamp') 'Candidate.PlcLoadTimestamp'
    Require-BoolTrue (Get-RequiredProperty $candidate 'C78BuildPassed') 'Candidate.C78BuildPassed'
    Require-BoolTrue (Get-RequiredProperty $candidate 'PlcLoadPassed') 'Candidate.PlcLoadPassed'

    $run = Get-RequiredProperty $Evidence 'Run'
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $run 'StartRequestId') 'Run.StartRequestId')
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $run 'QueryRequestId') 'Run.QueryRequestId')
    [void](Require-NonzeroUInt32 (Get-RequiredProperty $run 'RetireRequestId') 'Run.RetireRequestId')

    $intent = @(Get-RequiredProperty $run 'ClientIntentId')
    Require ($intent.Count -eq 4) 'Run.ClientIntentId must contain exactly four UInt32 words.'
    $intentNonzero = $false
    foreach ($word in $intent) {
        $value = [uint64]$word
        Require ($value -le [uint32]::MaxValue) 'Run.ClientIntentId contains a value outside UInt32.'
        if ($value -ne 0) { $intentNonzero = $true }
    }
    Require $intentNonzero 'Run.ClientIntentId must not be all zero.'

    Require-BoolTrue (Get-RequiredProperty $run 'StartAckAccepted') 'Run.StartAckAccepted'
    Require (([int](Get-RequiredProperty $run 'StartPacketCount')) -eq 1) 'Run.StartPacketCount must be exactly 1.'
    Require (([int](Get-RequiredProperty $run 'StartReplayCount')) -eq 0) 'Run.StartReplayCount must be 0.'
    Require (([int](Get-RequiredProperty $run 'QueryPacketCount')) -ge 1) 'Run.QueryPacketCount must be at least 1.'
    Require (([int](Get-RequiredProperty $run 'RetirePacketCount')) -eq 1) 'Run.RetirePacketCount must be exactly 1.'
    Require-NonEmptyString (Get-RequiredProperty $run 'TcpPacketReference') 'Run.TcpPacketReference'
    Require-NonEmptyString (Get-RequiredProperty $run 'SdoPacketReference') 'Run.SdoPacketReference'

    Require (([string](Get-RequiredProperty $run 'RecordState')) -ceq 'Succeeded') 'Run.RecordState must be Succeeded.'
    Require (([int](Get-RequiredProperty $run 'ObservedModeRaw')) -eq 8) 'Run.ObservedModeRaw must be 8.'
    Require (([int](Get-RequiredProperty $run 'PostRead6061')) -eq 8) 'Run.PostRead6061 must be 8.'
    Require (([int](Get-RequiredProperty $run 'OriginalCommandStatus')) -eq 0) 'Run.OriginalCommandStatus must be 0.'
    Require (([int](Get-RequiredProperty $run 'OriginalErrorId')) -eq 0) 'Run.OriginalErrorId must be 0.'
    Require (([uint64](Get-RequiredProperty $run 'OriginalDetailCode')) -eq 0) 'Run.OriginalDetailCode must be 0.'
    Require (([uint64](Get-RequiredProperty $run 'NativeCommandState')) -eq 0) 'Run.NativeCommandState must be 0.'

    $startCycle = Require-NonzeroUInt32 (Get-RequiredProperty $run 'StartCycle') 'Run.StartCycle'
    $completionCycle = Require-NonzeroUInt32 (Get-RequiredProperty $run 'CompletionCycle') 'Run.CompletionCycle'
    Require ($completionCycle -ge $startCycle) 'Run.CompletionCycle must be >= Run.StartCycle.'
    $generation = Require-NonzeroUInt32 (Get-RequiredProperty $run 'RecordGeneration') 'Run.RecordGeneration'
    $retiredGeneration = Require-NonzeroUInt32 (Get-RequiredProperty $run 'RetiredGeneration') 'Run.RetiredGeneration'
    Require ($retiredGeneration -eq $generation) 'Run.RetiredGeneration must exactly equal Run.RecordGeneration.'
    Require-BoolTrue (Get-RequiredProperty $run 'RetirementConfirmed') 'Run.RetirementConfirmed'
    Require-BoolTrue (Get-RequiredProperty $run 'JournalResolvedAfterRetire') 'Run.JournalResolvedAfterRetire'

    $flags = [uint32](Get-RequiredProperty $run 'EvidenceFlags')
    Require (($flags -band (-bnot $KnownEvidenceMask)) -eq 0) 'Run.EvidenceFlags contains unknown bits.'
    Require (($flags -band $TerminalSuccessMask) -eq $TerminalSuccessMask) 'Run.EvidenceFlags must contain VerifyReadDispatched, VerifyReadCompleted, OwnerReleased and ExecutorReusable.'

    return [pscustomobject]@{
        CaseName = $caseName
        Candidate = $candidate
        Run = $run
        Flags = $flags
    }
}

function Verify-Mode11A {
    param($Context)

    $run = $Context.Run
    Require (([int](Get-RequiredProperty $run 'PreRead6061')) -eq 8) 'MODE-11A requires PreRead6061 = 8.'
    Require (($Context.Flags -band $WriteRequested) -eq 0) 'MODE-11A EvidenceFlags must not contain WriteRequested.'
    Require (($Context.Flags -band $WriteDispatched) -eq 0) 'MODE-11A EvidenceFlags must not contain WriteDispatched.'
    Require (([int](Get-RequiredProperty $run 'Sdo6060WriteCount')) -eq 0) 'MODE-11A requires zero 0x6060 writes.'
    $payloads = @(Get-RequiredProperty $run 'Sdo6060WritePayloadHex')
    Require ($payloads.Count -eq 0) 'MODE-11A must contain no 0x6060 write payloads.'
    Require (([string](Get-RequiredProperty $run 'Verdict')) -ceq 'PASS') 'MODE-11A Verdict must be PASS.'
}

function Verify-Mode11B {
    param($Context)

    $run = $Context.Run
    $preRead = [int](Get-RequiredProperty $run 'PreRead6061')
    Require ($preRead -ne 8) 'MODE-11B requires a non-8 PreRead6061.'
    Require (($Context.Flags -band $AllMutationEvidence) -eq $AllMutationEvidence) 'MODE-11B EvidenceFlags must contain all six known evidence bits.'
    Require (([int](Get-RequiredProperty $run 'Sdo6060WriteCount')) -eq 1) 'MODE-11B requires exactly one 0x6060 write.'
    $payloads = @(Get-RequiredProperty $run 'Sdo6060WritePayloadHex')
    Require ($payloads.Count -eq 1) 'MODE-11B must contain exactly one 0x6060 write payload.'
    Require (([string]$payloads[0]).ToUpperInvariant() -ceq '08') 'MODE-11B 0x6060 write payload must be exactly one byte 08.'

    $preconditions = Get-RequiredProperty $run 'Preconditions'
    foreach ($name in @(
        'InitialNonCspSetupApproved',
        'PhysicalContextValid',
        'AxisStandstill',
        'Ds402FaultClear',
        'Ds402OperationEnabledClear',
        'Ds402HomeInactive',
        'EncoderMaintenanceInactive',
        'CompetingMutationInactive')) {
        Require-BoolTrue (Get-RequiredProperty $preconditions $name) "Run.Preconditions.$name"
    }
    Require-NonEmptyString (Get-RequiredProperty $preconditions 'InitialNonCspSetupReference') 'Run.Preconditions.InitialNonCspSetupReference'
    Require (([string](Get-RequiredProperty $run 'Verdict')) -ceq 'PASS') 'MODE-11B Verdict must be PASS.'
}

function Invoke-Verification {
    param([Parameter(Mandatory = $true)]$Evidence)

    $context = Verify-CommonEvidence $Evidence
    if ($context.CaseName -ceq 'MODE-11A') {
        Verify-Mode11A $context
    }
    else {
        Verify-Mode11B $context
    }
    return $context.CaseName
}

function New-SelfTestEvidence {
    param([Parameter(Mandatory = $true)][string]$CaseName)

    $flags = if ($CaseName -ceq 'MODE-11A') { 60 } else { 63 }
    $preRead = if ($CaseName -ceq 'MODE-11A') { 8 } else { 6 }
    $writeCount = if ($CaseName -ceq 'MODE-11A') { 0 } else { 1 }
    $payloads = if ($CaseName -ceq 'MODE-11A') { @() } else { @('08') }

    return [pscustomobject]@{
        SchemaVersion = 1
        Case = $CaseName
        Candidate = [pscustomobject]@{
            Branch = 'codex/setopmode-mode11-bench-activation'
            CommitSha = '0123456789abcdef0123456789abcdef01234567'
            DiagnosticsGate = 'TRUE'
            AdminFeatureMask = 0x717
            ActiveSourceDiffSha256 = ('A' * 64)
            DiagnosticsSourceSha256 = ('B' * 64)
            ControlSourceSha256 = ('C' * 64)
            ClassesSha256 = ('D' * 64)
            ProjectSha256 = ('E' * 64)
            ClassesBytes = 8635373
            ProjectBytes = 634865
            DiagnosticsBuild = 101
            DiagnosticsBootId = 202
            MapRevision = 303
            AxisReference = 1
            Endpoint = '192.0.2.10:10000'
            PlcLoadTimestamp = '2026-08-25T00:00:00Z'
            C78BuildPassed = $true
            PlcLoadPassed = $true
        }
        Run = [pscustomobject]@{
            PreRead6061 = $preRead
            PostRead6061 = 8
            StartRequestId = 11
            QueryRequestId = 12
            RetireRequestId = 13
            ClientIntentId = @(1, 2, 3, 4)
            StartAckAccepted = $true
            StartPacketCount = 1
            StartReplayCount = 0
            QueryPacketCount = 1
            RetirePacketCount = 1
            TcpPacketReference = 'tcp-capture-001'
            SdoPacketReference = 'sdo-capture-001'
            RecordState = 'Succeeded'
            ObservedModeRaw = 8
            OriginalCommandStatus = 0
            OriginalErrorId = 0
            OriginalDetailCode = 0
            NativeCommandState = 0
            EvidenceFlags = $flags
            StartCycle = 1000
            CompletionCycle = 1010
            RecordGeneration = 77
            RetiredGeneration = 77
            RetirementConfirmed = $true
            JournalResolvedAfterRetire = $true
            Sdo6060WriteCount = $writeCount
            Sdo6060WritePayloadHex = $payloads
            Verdict = 'PASS'
            Preconditions = [pscustomobject]@{
                InitialNonCspSetupApproved = $true
                InitialNonCspSetupReference = 'approved-home-mode-setup'
                PhysicalContextValid = $true
                AxisStandstill = $true
                Ds402FaultClear = $true
                Ds402OperationEnabledClear = $true
                Ds402HomeInactive = $true
                EncoderMaintenanceInactive = $true
                CompetingMutationInactive = $true
            }
        }
    }
}

function Expect-SelfTestFailure {
    param(
        [Parameter(Mandatory = $true)]$Evidence,
        [Parameter(Mandatory = $true)][string]$Label
    )

    $failed = $false
    try {
        [void](Invoke-Verification $Evidence)
    }
    catch {
        $failed = $true
    }
    Require $failed "Self-test negative case unexpectedly passed: $Label"
    Write-Host "PASS negative self-test rejected: $Label"
}

if ($SelfTest) {
    $a = New-SelfTestEvidence 'MODE-11A'
    Require ((Invoke-Verification $a) -ceq 'MODE-11A') 'MODE-11A positive self-test failed.'
    Write-Host 'PASS positive self-test: MODE-11A zero-write'

    $b = New-SelfTestEvidence 'MODE-11B'
    Require ((Invoke-Verification $b) -ceq 'MODE-11B') 'MODE-11B positive self-test failed.'
    Write-Host 'PASS positive self-test: MODE-11B exact-one-write'

    $badA = New-SelfTestEvidence 'MODE-11A'
    $badA.Run.Sdo6060WriteCount = 1
    Expect-SelfTestFailure $badA 'MODE-11A observed a 0x6060 write'

    $badB = New-SelfTestEvidence 'MODE-11B'
    $badB.Run.StartReplayCount = 1
    Expect-SelfTestFailure $badB 'MODE-11B replayed Start'

    $badGeneration = New-SelfTestEvidence 'MODE-11B'
    $badGeneration.Run.RetiredGeneration = 78
    Expect-SelfTestFailure $badGeneration 'retired generation mismatch'

    $badIdentity = New-SelfTestEvidence 'MODE-11A'
    $badIdentity.Candidate.DiagnosticsBootId = 0
    Expect-SelfTestFailure $badIdentity 'same-image BootId missing'

    Write-Host 'PASS MODE-11 hardware evidence verifier self-test'
    exit 0
}

$full = [IO.Path]::GetFullPath($EvidencePath)
Require (Test-Path -LiteralPath $full -PathType Leaf) "Evidence file does not exist: $full"
$text = [IO.File]::ReadAllText($full)
Require (-not [string]::IsNullOrWhiteSpace($text)) 'Evidence file is empty.'
$evidence = $text | ConvertFrom-Json
$caseName = Invoke-Verification $evidence
Write-Host "PASS $caseName hardware/packet evidence contract: $full"
Write-Host 'NOTE This verifier validates supplied evidence consistency; packet capture authenticity and physical observation remain operator evidence.'
exit 0

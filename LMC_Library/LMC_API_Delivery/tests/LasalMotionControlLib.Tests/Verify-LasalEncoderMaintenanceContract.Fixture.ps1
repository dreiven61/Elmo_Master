param(
    [switch]$RunSelfTest,
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'

if ($RunSelfTest) {
    if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        throw 'RepositoryRoot is required with RunSelfTest.'
    }
    $verifierPath = Join-Path $PSScriptRoot 'Verify-LasalContract.ps1'
    & $verifierPath `
        -RepositoryRoot $RepositoryRoot `
        -EncoderMaintenanceVerifierSelfTestOnly
    return
}

$diagnostics = @'
#define LMC_DIAG_ENCODER_TW20_ENABLED TRUE
#define LMC_DIAG_ENCODER_TW19_ENABLED TRUE
#define LMC_DIAG_ENCODER_RESET_VALUE 1
#define LMC_DIAG_OWNER_PHASE_RESERVED 1

EncoderMaintenanceState : ARRAY [0..191] OF DINT;
EncoderMaintenanceServiceMilliseconds : UDINT;
EncoderMaintenanceObservedLatchCycle : UDINT;
EncoderMaintenanceLatchAdvanceServiceMilliseconds : UDINT;
EncoderMaintenanceLatchFreshSampleCount : UINT;
AxisOwnership : CltChCmd_LMCControlCommandService;

FUNCTION GLOBAL LMCDiagnosticsService::ProcessOperations
    ProcessEncoderMaintenance();
END_FUNCTION

FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest
    if CommandId = 0x7E53 then
        ResponseSize := HandleEncoderMaintenanceStart(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSequence:=RequestSequence,
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration,
            RequestSize:=RequestSize);
    elsif CommandId = 0x7E54 then
        ResponseSize := HandleEncoderMaintenanceOutcome(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSize:=RequestSize);
    elsif CommandId = 0x7E55 then
        ResponseSize := HandleEncoderMaintenanceRetire(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSize:=RequestSize);
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceStart
    VAR
        maintenanceKind, maintenanceFeedbackSocket : UINT;
        maintenanceObjectIndex : UINT;
        maintenanceSubIndex : USINT;
        maintenanceCommandValue, timeoutMilliseconds : UDINT;
        maintenanceDataLength : UINT;
        controlWord, statusWord : UDINT;
        recordBase : DINT;
    END_VAR
    if RequestSize <> 72 then RETURN; end_if;
    diagnosticsBuild := (pRequest + 8)^$UDINT;
    diagnosticsBootId := (pRequest + 12)^$UDINT;
    mapRevision := (pRequest + 16)^$UDINT;
    intent0 := (pRequest + 20)^$UDINT;
    intent3 := (pRequest + 32)^$UDINT;
    maintenanceKind := (pRequest + 36)^$UINT;
    compatibilityProfileId := (pRequest + 38)^$UINT;
    driveReference := (pRequest + 40)^$UINT;
    maintenanceFeedbackSocket := (pRequest + 42)^$UINT;
    maintenanceCommandValue := (pRequest + 44)^$UDINT;
    timeoutMilliseconds := (pRequest + 48)^$UDINT;
    evidence0 := (pRequest + 52)^$UDINT;
    evidence3 := (pRequest + 64)^$UDINT;
    executeToken := (pRequest + 68)^$UDINT;
    if (timeoutMilliseconds < 1) | (timeoutMilliseconds > 60000) then
        RETURN;
    end_if;
    if (EncoderMaintenanceState[152] <> 0) |
       (Ds402HomeState[92] <> 0) |
       (OperationState = LMC_DIAG_SDO_STATE_QUEUED) |
       (OperationState = LMC_DIAG_SDO_STATE_RUNNING) |
       (SdoInternalDrainState <> 0) then
        RETURN;
    end_if;
    if maintenanceKind = 1 then
        maintenanceObjectIndex := 0x20FC;
        maintenanceSubIndex := 0x02;
    elsif maintenanceKind = 2 then
        maintenanceObjectIndex := 0x20FC;
        maintenanceSubIndex := 0x01;
    end_if;
    if (maintenanceCommandValue <> LMC_DIAG_ENCODER_RESET_VALUE) |
       (maintenanceFeedbackSocket < 1) |
       (maintenanceFeedbackSocket > 4) then
        RETURN;
    end_if;
    maintenanceDataLength := 2;
    if detailCode = 0 then
        ownerResult := AxisOwnership.ValidateAxisOwnershipIdentity(
            CommandId:=0x7E53,
            Reference:=driveReference,
            ExpectedAxisMask:=axisMask,
            OwnerKind:=LMC_DIAG_OWNER_KIND_ENCODER,
            ResourceKind:=LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO,
            AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSequence:=RequestSequence,
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration,
            RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED,
            pIdentity:=pRequest$^void,
            IdentitySize:=RequestSize);
        if ownerResult <> 0 then
            detailCode := 9;
        end_if;
    end_if;
    if ((controlWord and 0x00000008) <> 0) |
       ((statusWord and 0x00000004) <> 0) then
        RETURN;
    end_if;
    EncoderMaintenanceState[recordBase + 12]$UDINT := timeoutMilliseconds;
    EncoderMaintenanceState[recordBase + 13]$UDINT := ops.tAbsolute;
    if detailCode = 0 then
        ownerResult := AxisOwnership.CommitAxisOwnership(
            CommandId:=0x7E53,
            Reference:=driveReference,
            ExpectedAxisMask:=axisMask,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSequence:=RequestSequence,
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration);
        if ownerResult <> 0 then
            detailCode := 9;
        end_if;
    end_if;
    if detailCode <> 0 then
        rollbackResult := AxisOwnership.RollbackAxisOwnership(
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSequence:=RequestSequence,
            Reason:=0);
        (pResponse + 12)^$UDINT := detailCode;
        RETURN;
    end_if;
    _memset(dest:=#EncoderMaintenanceState[recordBase], usByte:=0, cntr:=152);
    if ResponseCapacity < 40 then RETURN; end_if;
    (pResponse + 36)^$UDINT := EncoderMaintenanceState[recordBase + 14]$UDINT;
    ResponseSize := 40;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleGenericSubmit
    0x7E50:
        if EncoderMaintenanceState[152] <> 0 then
            detailCode := 9;
        end_if;
    0x7E51:
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart
    if (Ds402HomeState[92] <> 0) |
       (EncoderMaintenanceState[152] <> 0) then
        detailCode := 10;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceOutcome
    if Reference <> 0 then RETURN; end_if;
    if RequestSize <> 72 then RETURN; end_if;
    diagnosticsBuild := (pRequest + 8)^$UDINT;
    diagnosticsBootId := (pRequest + 12)^$UDINT;
    mapRevision := (pRequest + 16)^$UDINT;
    originalRequestId := (pRequest + 20)^$UDINT;
    intent0 := (pRequest + 24)^$UDINT;
    intent1 := (pRequest + 28)^$UDINT;
    intent2 := (pRequest + 32)^$UDINT;
    intent3 := (pRequest + 36)^$UDINT;
    maintenanceKind := (pRequest + 40)^$UINT;
    compatibilityProfileId := (pRequest + 42)^$UINT;
    driveReference := (pRequest + 44)^$UINT;
    maintenanceFeedbackSocket := (pRequest + 46)^$UINT;
    maintenanceCommandValue := (pRequest + 48)^$UDINT;
    timeoutMilliseconds := (pRequest + 52)^$UDINT;
    evidence0 := (pRequest + 56)^$UDINT;
    evidence1 := (pRequest + 60)^$UDINT;
    evidence2 := (pRequest + 64)^$UDINT;
    evidence3 := (pRequest + 68)^$UDINT;
    if maintenanceCommandValue <> LMC_DIAG_ENCODER_RESET_VALUE then
        RETURN;
    end_if;
    if ResponseCapacity < 156 then RETURN; end_if;
    (pResponse + 152)^$UDINT := EncoderMaintenanceState[recordBase + 15]$UDINT;
    ResponseSize := 156;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceRetire
    if RequestSize <> 76 then RETURN; end_if;
    recordGeneration := (pRequest + 72)^$UDINT;
    rawState := EncoderMaintenanceState[recordBase]$UDINT;
    baseState := rawState and 0x00007FFF;
    if ResponseCapacity < 156 then RETURN; end_if;
    if rawState = 0 then
        detailCode := LMC_DIAG_ENCODER_DETAIL_NOT_FOUND;
    elsif rawState = LMC_DIAG_ENCODER_RECORD_RUNNING then
        detailCode := LMC_DIAG_ENCODER_DETAIL_INDETERMINATE;
    elsif stateValid = FALSE then
        detailCode := LMC_DIAG_ENCODER_DETAIL_STORE_CORRUPT;
    elsif EncoderMaintenanceState[recordBase + 36]$UDINT <>
       recordGeneration then
        detailCode := LMC_DIAG_ENCODER_DETAIL_KEY_MISMATCH;
    else
        ResponseSize := HandleEncoderMaintenanceOutcome(
            Reference:=Reference, pRequest:=pRequest,
            pResponse:=pResponse, ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=72);
        if (ResponseSize = 156) &
           ((pResponse + 148)^$UDINT = recordGeneration) then
            if rawState = baseState then
                EncoderMaintenanceState[recordBase] :=
                    TO_DINT(baseState OR LMC_DIAG_ENCODER_RECORD_RETIRED);
            end_if;
            RETURN;
        end_if;
        detailCode := LMC_DIAG_ENCODER_DETAIL_KEY_MISMATCH;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::ProcessEncoderMaintenance
    VAR
        maintenanceWriteValue : UINT;
    END_VAR
    EncoderMaintenanceState[recordBase + 27]$UDINT :=
        LMC_DIAG_ENCODER_VERIFY_DRIVE_TARGET OR
        LMC_DIAG_ENCODER_VERIFY_SDO_CONTRACT;
    maintenanceWriteValue := LMC_DIAG_ENCODER_RESET_VALUE;
    if maintenanceCommandValue <> LMC_DIAG_ENCODER_RESET_VALUE then
        failureDetail := 12;
    end_if;
    serviceNow := ops.tAbsolute;
    EncoderMaintenanceServiceMilliseconds := serviceNow;
    serviceStartMs := EncoderMaintenanceState[recordBase + 13]$UDINT;
    timeoutMilliseconds := EncoderMaintenanceState[recordBase + 12]$UDINT;
    elapsedMs := serviceNow - serviceStartMs;
    if elapsedMs >= timeoutMilliseconds then
        terminalState := 3;
    else
        remainingMs := timeoutMilliseconds - elapsedMs;
        if remainingMs < 1 then
            remainingMs := 1;
        end_if;
        if remainingMs > 60000 then
            remainingMs := 60000;
        end_if;
        if EncoderMaintenanceState[189]$UDINT <> 0 then
            failureDetail := 36;
        else
            EncoderMaintenanceState[189]$UDINT := encoderProcessOperationToken;
            startResult := ERROR;
            case axisReference of
                1:
                    startResult := SdoAxis1.TryStartWrite(
                        OperationToken:=encoderProcessOperationToken,
                        ObjectIndex:=maintenanceObjectIndex,
                        SubIndex:=maintenanceSubIndex,
                        pWriteData:=(#maintenanceWriteValue)$^USINT,
                        WriteLength:=2,
                        TimeoutMs:=remainingMs);
                2:
                    startResult := SdoAxis2.TryStartWrite(
                        OperationToken:=encoderProcessOperationToken,
                        ObjectIndex:=maintenanceObjectIndex,
                        SubIndex:=maintenanceSubIndex,
                        pWriteData:=(#maintenanceWriteValue)$^USINT,
                        WriteLength:=2,
                        TimeoutMs:=remainingMs);
                3:
                    startResult := SdoAxis3.TryStartWrite(
                        OperationToken:=encoderProcessOperationToken,
                        ObjectIndex:=maintenanceObjectIndex,
                        SubIndex:=maintenanceSubIndex,
                        pWriteData:=(#maintenanceWriteValue)$^USINT,
                        WriteLength:=2,
                        TimeoutMs:=remainingMs);
                4:
                    startResult := SdoAxis4.TryStartWrite(
                        OperationToken:=encoderProcessOperationToken,
                        ObjectIndex:=maintenanceObjectIndex,
                        SubIndex:=maintenanceSubIndex,
                        pWriteData:=(#maintenanceWriteValue)$^USINT,
                        WriteLength:=2,
                        TimeoutMs:=remainingMs);
            end_case;
        end_if;
    end_if;
    ownerResult := AxisOwnership.PublishAxisOwnership(
        AxisMask:=axisMask,
        AdmissionToken:=admissionToken,
        OwnerGeneration:=ownerGeneration,
        ReportValue0:=TO_UDINT(maintenanceObjectIndex),
        ReportValue1:=encoderProcessOperationToken,
        ObservationCycle:=currentCycle);
    orphanResult := -1;
    case axisReference of
        1: orphanResult := SdoAxis1.MarkOrphan(
            ExpectedToken:=encoderProcessOperationToken);
        2: orphanResult := SdoAxis2.MarkOrphan(
            ExpectedToken:=encoderProcessOperationToken);
        3: orphanResult := SdoAxis3.MarkOrphan(
            ExpectedToken:=encoderProcessOperationToken);
        4: orphanResult := SdoAxis4.MarkOrphan(
            ExpectedToken:=encoderProcessOperationToken);
    end_case;
    if (orphanResult = 0) | (orphanResult = -2) then
        EncoderMaintenanceState[152] := LMC_DIAG_ENCODER_STAGE_DRAIN;
    end_if;
    if EncoderMaintenanceState[152] = LMC_DIAG_ENCODER_STAGE_DRAIN then
        copyResult := -2;
        case axisReference of
            1: copyResult := SdoAxis1.CopyCompletion(
                ExpectedToken:=encoderProcessOperationToken,
                pDest:=#completion, DestSize:=sizeof(completion));
            2: copyResult := SdoAxis2.CopyCompletion(
                ExpectedToken:=encoderProcessOperationToken,
                pDest:=#completion, DestSize:=sizeof(completion));
            3: copyResult := SdoAxis3.CopyCompletion(
                ExpectedToken:=encoderProcessOperationToken,
                pDest:=#completion, DestSize:=sizeof(completion));
            4: copyResult := SdoAxis4.CopyCompletion(
                ExpectedToken:=encoderProcessOperationToken,
                pDest:=#completion, DestSize:=sizeof(completion));
        end_case;
        if copyResult = 0 then
            EncoderMaintenanceState[156] := 0;
        end_if;
        case axisReference of
            1: executorReusable := SdoAxis1.IsReusable();
            2: executorReusable := SdoAxis2.IsReusable();
            3: executorReusable := SdoAxis3.IsReusable();
            4: executorReusable := SdoAxis4.IsReusable();
        end_case;
        if (copyResult <> 0) & (copyResult <> -2) then
            failureDetail := 36;
        end_if;
    end_if;
    if (EncoderMaintenanceState[156]$UDINT = 0) & executorReusable then
        ownerResult := AxisOwnership.PublishAxisOwnership(
            AxisMask:=axisMask,
            AdmissionToken:=admissionToken,
            OwnerGeneration:=ownerGeneration,
            ReportKind:=LMC_DIAG_OWNER_REPORT_TERMINAL_SUCCESS,
            ReportValue0:=0,
            ReportValue1:=0,
            ObservationCycle:=currentCycle);
        if ownerResult = 0 then
            EncoderMaintenanceState[recordBase] :=
                LMC_DIAG_ENCODER_RECORD_SUCCEEDED;
        end_if;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::Capabilities
    if LMC_DIAG_ENCODER_TW20_ENABLED = TRUE then
        bits := bits or 0x00040000;
    end_if;
    if LMC_DIAG_ENCODER_TW19_ENABLED = TRUE then
        bits := bits or 0x00080000;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::GetSdoWritePolicyDetail
    DetailCode := 0;
END_FUNCTION
'@

$tcp = @'
FUNCTION TCPMotionInterface::MsgPaser
    admissionToken := 0;
    ownerGeneration := 0;
    if (CommandID = 0x7E53) & (Payload = 72) then
        ownerAxisReference := RequestBuf[48]$UINT;
        requestedAxisMask := TO_UDINT(1) shl
            TO_UDINT(ownerAxisReference - 1);
        ownerResult := ControlCommands.ReserveAxisOwnership(
            CommandId:=0x7E53,
            Reference:=ownerAxisReference,
            RequestedAxisMask:=requestedAxisMask,
            OwnerKind:=5,
            ResourceKind:=4,
            AdmissionMode:=4,
            CallerSessionEpoch:=ActiveRequest.SessionEpoch,
            RequestSequence:=ActiveRequest.Sequence,
            pIdentity:=(#RequestBuf[8])$^void,
            IdentitySize:=Payload$UDINT,
            pEffectiveAxisMask:=#effectiveAxisMask,
            pAdmissionToken:=#admissionToken,
            pOwnerGeneration:=#ownerGeneration);
    end_if;
    case CommandID of
        0x7E53, 0x7E54, 0x7E55:
            diagnosticsResponseSize := Diagnostics.HandleRequest(
                CommandId:=CommandID$UINT,
                CallerSessionEpoch:=ActiveRequest.SessionEpoch,
                RequestSequence:=ActiveRequest.Sequence,
                AdmissionToken:=admissionToken,
                OwnerGeneration:=ownerGeneration);
    end_case;
END_FUNCTION
'@

$csharpProtocol = @'
private static void WriteStartKey(byte[] buffer, int payloadOffset, RecoveryKey recoveryKey)
{
    WriteUInt32(buffer, payloadOffset + 48, recoveryKey.TimeoutMilliseconds);
}
private static void WriteOutcomeKey(byte[] buffer, int payloadOffset, RecoveryKey recoveryKey)
{
    WriteUInt32(buffer, payloadOffset + 52, recoveryKey.TimeoutMilliseconds);
}
private static void RequireExactOutcomeKey(byte[] payload, RecoveryKey expected)
{
    if (ReadUInt32(payload, 64) != expected.TimeoutMilliseconds) { throw new Exception(); }
}
'@

$csharpModels = @'
public static class LMCEncoderMaintenanceSdoContract
{
    public const ushort ObjectIndex = 0x20FC;
    public const byte Tw19MultiturnPositionResetSubIndex = 0x01;
    public const byte Tw20ErrorWarningResetSubIndex = 0x02;
    public const ushort WriteLength = 2;
    public const LMCSignalValueType ValueType = LMCSignalValueType.UInt16;
}
internal const int MaximumTimeoutMilliseconds = 60000;
private static void Validate(uint timeoutMilliseconds)
{
    if (timeoutMilliseconds == 0 ||
        timeoutMilliseconds > MaximumTimeoutMilliseconds) { throw new Exception(); }
}
public class EncoderMaintenanceRequest
{
    public uint TimeoutMilliseconds { get; private set; }
}
public class EncoderMaintenanceRecoveryKey
{
    public uint TimeoutMilliseconds { get; private set; }
}
'@

function New-NegativeFixture {
    param(
        [string]$Name,
        [string]$Diagnostics = $diagnostics,
        [string]$Tcp = $tcp,
        [string]$CSharpProtocol = $csharpProtocol,
        [string]$CSharpModels = $csharpModels
    )

    [pscustomobject]@{
        Name = $Name
        Diagnostics = $Diagnostics
        Tcp = $Tcp
        CSharpProtocol = $CSharpProtocol
        CSharpModels = $CSharpModels
    }
}

$encoderValidateFixtureCall = [regex]::Match(
    $diagnostics,
    ('(?is)ownerResult\s*:=\s*AxisOwnership\.ValidateAxisOwnershipIdentity\(\s*' +
     'CommandId:=0x7E53,.*?Reference:=driveReference,.*?' +
     'OwnerGeneration:=OwnerGeneration,\s*' +
     'RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED,\s*' +
     'pIdentity:=pRequest\$\^void,\s*IdentitySize:=RequestSize\);'))
$encoderCommitFixtureCall = [regex]::Match(
    $diagnostics,
    ('(?is)ownerResult\s*:=\s*AxisOwnership\.CommitAxisOwnership\(\s*' +
     'CommandId:=0x7E53,.*?Reference:=driveReference,.*?' +
     'OwnerGeneration:=OwnerGeneration\);'))
if (-not $encoderValidateFixtureCall.Success -or
    -not $encoderCommitFixtureCall.Success) {
    throw 'Encoder maintenance fixture cannot isolate owner activation calls.'
}
$duplicateEncoderValidateDiagnostics = $diagnostics.Insert(
    $encoderValidateFixtureCall.Index + $encoderValidateFixtureCall.Length,
    [Environment]::NewLine + $encoderValidateFixtureCall.Value)
$duplicateEncoderCommitDiagnostics = $diagnostics.Insert(
    $encoderCommitFixtureCall.Index + $encoderCommitFixtureCall.Length,
    [Environment]::NewLine + $encoderCommitFixtureCall.Value)
$encoderCommitBeforeValidateDiagnostics = $diagnostics.Remove(
    $encoderCommitFixtureCall.Index,
    $encoderCommitFixtureCall.Length)
$encoderCommitBeforeValidateDiagnostics =
    $encoderCommitBeforeValidateDiagnostics.Insert(
        $encoderValidateFixtureCall.Index,
        $encoderCommitFixtureCall.Value + [Environment]::NewLine)
$encoderCommitResultOverwriteDiagnostics = $diagnostics.Insert(
    $encoderCommitFixtureCall.Index + $encoderCommitFixtureCall.Length,
    [Environment]::NewLine + '        ownerResult := 0;')
$equivalentEncoderValidateCall = $encoderValidateFixtureCall.Value.Replace(
    'ExpectedAxisMask:=axisMask,',
    'ExpectedAxisMask:=TO_UDINT(axisMask),')
$equivalentEncoderCommitCall = $encoderCommitFixtureCall.Value.Replace(
    'ExpectedAxisMask:=axisMask,',
    'ExpectedAxisMask:=TO_UDINT(axisMask),')
if ($equivalentEncoderValidateCall -ceq $encoderValidateFixtureCall.Value -or
    $equivalentEncoderCommitCall -ceq $encoderCommitFixtureCall.Value) {
    throw 'Encoder maintenance equivalent-call fixtures did not mutate.'
}
$equivalentEncoderValidateDuplicateDiagnostics = $diagnostics.Insert(
    $encoderValidateFixtureCall.Index,
    $equivalentEncoderValidateCall + [Environment]::NewLine)
$equivalentEncoderCommitBeforeValidateDiagnostics = $diagnostics.Insert(
    $encoderValidateFixtureCall.Index,
    $equivalentEncoderCommitCall + [Environment]::NewLine)
$retireGenerationDeadBranchDiagnostics = ([regex]::new(
    ('(?is)elsif\s+EncoderMaintenanceState' +
     '\[recordBase\s*\+\s*36\]\$UDINT\s*<>\s*' +
     'recordGeneration\s+then'))).Replace(
        $diagnostics,
        ('elsif FALSE & (EncoderMaintenanceState[recordBase + 36]$UDINT ' +
         '<> recordGeneration) then'),
        1)
$tryStartWriteAxisSwapDiagnostics = $diagnostics.Replace(
    'startResult := SdoAxis1.TryStartWrite(',
    'startResult := SdoAxisSwap.TryStartWrite(').Replace(
    'startResult := SdoAxis4.TryStartWrite(',
    'startResult := SdoAxis1.TryStartWrite(').Replace(
    'startResult := SdoAxisSwap.TryStartWrite(',
    'startResult := SdoAxis4.TryStartWrite(')
$orphanAxisSwapDiagnostics = $diagnostics.Replace(
    '1: orphanResult := SdoAxis1.MarkOrphan(',
    '1: orphanResult := SdoAxisSwap.MarkOrphan(').Replace(
    '4: orphanResult := SdoAxis4.MarkOrphan(',
    '4: orphanResult := SdoAxis1.MarkOrphan(').Replace(
    '1: orphanResult := SdoAxisSwap.MarkOrphan(',
    '1: orphanResult := SdoAxis4.MarkOrphan(')
$copyAxisSwapDiagnostics = $diagnostics.Replace(
    '1: copyResult := SdoAxis1.CopyCompletion(',
    '1: copyResult := SdoAxisSwap.CopyCompletion(').Replace(
    '4: copyResult := SdoAxis4.CopyCompletion(',
    '4: copyResult := SdoAxis1.CopyCompletion(').Replace(
    '1: copyResult := SdoAxisSwap.CopyCompletion(',
    '1: copyResult := SdoAxis4.CopyCompletion(')
$reuseAxisSwapDiagnostics = $diagnostics.Replace(
    '1: executorReusable := SdoAxis1.IsReusable();',
    '1: executorReusable := SdoAxisSwap.IsReusable();').Replace(
    '4: executorReusable := SdoAxis4.IsReusable();',
    '4: executorReusable := SdoAxis1.IsReusable();').Replace(
    '1: executorReusable := SdoAxisSwap.IsReusable();',
    '1: executorReusable := SdoAxis4.IsReusable();')

$negativeFixtures = @(
    (New-NegativeFixture `
        -Name 'LasalTimeoutCyclesName' `
        -Diagnostics $diagnostics.Replace(
            'timeoutMilliseconds', 'timeoutCycles'))
    (New-NegativeFixture `
        -Name 'LasalTimeoutRangeExpanded' `
        -Diagnostics $diagnostics.Replace(
            'timeoutMilliseconds > 60000',
            'timeoutMilliseconds > 60001'))
    (New-NegativeFixture `
        -Name 'LasalServiceClockReplacedByLatchCycle' `
        -Diagnostics $diagnostics.Replace(
            'serviceNow := ops.tAbsolute;',
            'serviceNow := EncoderMaintenanceObservedLatchCycle;'))
    (New-NegativeFixture `
        -Name 'LasalTotalTimeoutPassedToExecutor' `
        -Diagnostics $diagnostics.Replace(
            'TimeoutMs:=remainingMs',
            'TimeoutMs:=timeoutMilliseconds'))
    (New-NegativeFixture `
        -Name 'LasalTimeoutGatedByFreshLatch' `
        -Diagnostics $diagnostics.Replace(
            'if elapsedMs >= timeoutMilliseconds then',
            'if newCycle & (elapsedMs >= timeoutMilliseconds) then'))
    (New-NegativeFixture `
        -Name 'LasalLatchCycleUsedAsElapsedTime' `
        -Diagnostics $diagnostics.Replace(
            'elapsedMs := serviceNow - serviceStartMs;',
            ('elapsedMs := EncoderMaintenanceObservedLatchCycle - ' +
             'serviceStartMs;')))
    (New-NegativeFixture `
        -Name 'LasalDedicatedTimeoutCyclesSymbol' `
        -Diagnostics $diagnostics.Replace(
            'serviceNow := ops.tAbsolute;',
            ('TimeoutCycles := 1000;' + [Environment]::NewLine +
             '    serviceNow := ops.tAbsolute;')))
    (New-NegativeFixture `
        -Name 'LasalObsoleteManifestReintroduced' `
        -Diagnostics $diagnostics.Replace(
            '#define LMC_DIAG_ENCODER_RESET_VALUE 1',
            ('#define LMC_DIAG_ENCODER_RESET_VALUE 1' +
             [Environment]::NewLine +
             '#define LMC_DIAG_ENCODER_TW19_AXIS4_EVIDENCE3 1')))
    (New-NegativeFixture `
        -Name 'LasalUsesTwArrayLongAlias' `
        -Diagnostics $diagnostics.Replace('0x20FC', '0x3204'))
    (New-NegativeFixture `
        -Name 'LasalTwSubindicesSwapped' `
        -Diagnostics $diagnostics.Replace(
            'maintenanceSubIndex := 0x02;',
            'maintenanceSubIndex := 0x01;'))
    (New-NegativeFixture `
        -Name 'LasalDedicatedWriteUsesFourBytes' `
        -Diagnostics $diagnostics.Replace(
            'WriteLength:=2', 'WriteLength:=4'))
    (New-NegativeFixture `
        -Name 'LasalUInt16WriteStagingRemoved' `
        -Diagnostics $diagnostics.Replace(
            'maintenanceWriteValue := LMC_DIAG_ENCODER_RESET_VALUE;',
            'maintenanceWriteValue := TO_UINT(maintenanceCommandValue);'))
    (New-NegativeFixture `
        -Name 'LasalFixedResetValueChanged' `
        -Diagnostics $diagnostics.Replace(
            '#define LMC_DIAG_ENCODER_RESET_VALUE 1',
            '#define LMC_DIAG_ENCODER_RESET_VALUE 2'))
    (New-NegativeFixture `
        -Name 'LasalCommandValueReboundToSocket' `
        -Diagnostics $diagnostics.Replace(
            'maintenanceCommandValue <> LMC_DIAG_ENCODER_RESET_VALUE',
            'maintenanceCommandValue <> TO_UDINT(maintenanceFeedbackSocket)'))
    (New-NegativeFixture `
        -Name 'LasalOneShotClaimCleared' `
        -Diagnostics $diagnostics.Replace(
            'EncoderMaintenanceState[189]$UDINT := encoderProcessOperationToken;',
            'EncoderMaintenanceState[189]$UDINT := 0;'))
    (New-NegativeFixture `
        -Name 'LasalAxis4WriteUsesAxis1Executor' `
        -Diagnostics $diagnostics.Replace(
            'startResult := SdoAxis4.TryStartWrite(',
            'startResult := SdoAxis1.TryStartWrite('))
    (New-NegativeFixture `
        -Name 'LasalTryStartWriteAxes1And4Swapped' `
        -Diagnostics $tryStartWriteAxisSwapDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalValidateAxisMaskOmitted' `
        -Diagnostics $diagnostics.Replace(
            'ExpectedAxisMask:=axisMask,',
            'ExpectedAxisMask:=0,'))
    (New-NegativeFixture `
        -Name 'LasalValidateResultOverwritten' `
        -Diagnostics ([regex]::new(
            ('(?is)(OwnerGeneration:=OwnerGeneration,\s*' +
             'RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED,\s*' +
             'pIdentity:=pRequest\$\^void,\s*' +
             'IdentitySize:=RequestSize\);\s*)' +
             '(if\s+ownerResult\s*<>\s*0\s+then)'))).Replace(
                $diagnostics,
                '${1}ownerResult := 0;${2}',
                1))
    (New-NegativeFixture `
        -Name 'LasalDuplicateValidateCall' `
        -Diagnostics $duplicateEncoderValidateDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalDuplicateCommitCall' `
        -Diagnostics $duplicateEncoderCommitDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalCommitBeforeValidate' `
        -Diagnostics $encoderCommitBeforeValidateDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalCommitResultOverwritten' `
        -Diagnostics $encoderCommitResultOverwriteDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalEquivalentValidateDuplicate' `
        -Diagnostics $equivalentEncoderValidateDuplicateDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalEquivalentCommitBeforeValidate' `
        -Diagnostics $equivalentEncoderCommitBeforeValidateDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalRollbackSequenceOmitted' `
        -Diagnostics $diagnostics.Replace(
            'RequestSequence:=RequestSequence,',
            'RequestSequence:=0,'))
    (New-NegativeFixture `
        -Name 'LasalOutcomeTimeoutIdentityOffsetChanged' `
        -Diagnostics $diagnostics.Replace(
            'timeoutMilliseconds := (pRequest + 52)^$UDINT;',
            'timeoutMilliseconds := (pRequest + 54)^$UDINT;'))
    (New-NegativeFixture `
        -Name 'LasalRetireClearsRecord' `
        -Diagnostics $diagnostics.Replace(
            'TO_DINT(baseState OR LMC_DIAG_ENCODER_RECORD_RETIRED);',
            '0;'))
    (New-NegativeFixture `
        -Name 'LasalRetireUsesLogicalPipe' `
        -Diagnostics $diagnostics.Replace(
            'TO_DINT(baseState OR LMC_DIAG_ENCODER_RECORD_RETIRED);',
            'TO_DINT(baseState | LMC_DIAG_ENCODER_RECORD_RETIRED);'))
    (New-NegativeFixture `
        -Name 'LasalVerificationMaskUsesLogicalPipe' `
        -Diagnostics $diagnostics.Replace(
            'LMC_DIAG_ENCODER_VERIFY_DRIVE_TARGET OR',
            'LMC_DIAG_ENCODER_VERIFY_DRIVE_TARGET |'))
    (New-NegativeFixture `
        -Name 'LasalRetireStoredGenerationSlotChanged' `
        -Diagnostics $diagnostics.Replace(
            'EncoderMaintenanceState[recordBase + 36]$UDINT <>',
            'EncoderMaintenanceState[recordBase + 35]$UDINT <>'))
    (New-NegativeFixture `
        -Name 'LasalRetireGenerationDeadBranch' `
        -Diagnostics $retireGenerationDeadBranchDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalTerminalSkipsOwnerResult' `
        -Diagnostics $diagnostics.Replace(
            'if ownerResult = 0 then',
            'if TRUE then'))
    (New-NegativeFixture `
        -Name 'LasalTerminalSkipsExecutorDrain' `
        -Diagnostics $diagnostics.Replace(
            'executorReusable := SdoAxis1.IsReusable();',
            'executorReusable := TRUE;'))
    (New-NegativeFixture `
        -Name 'LasalDrainAxis4CopyUsesAxis1Executor' `
        -Diagnostics $diagnostics.Replace(
            '4: copyResult := SdoAxis4.CopyCompletion(',
            '4: copyResult := SdoAxis1.CopyCompletion('))
    (New-NegativeFixture `
        -Name 'LasalDrainAxis4ReuseUsesAxis1Executor' `
        -Diagnostics $diagnostics.Replace(
            '4: executorReusable := SdoAxis4.IsReusable();',
            '4: executorReusable := SdoAxis1.IsReusable();'))
    (New-NegativeFixture `
        -Name 'LasalDrainCopyAxes1And4Swapped' `
        -Diagnostics $copyAxisSwapDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalDrainReuseAxes1And4Swapped' `
        -Diagnostics $reuseAxisSwapDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalOrphanAxis4UsesAxis1Executor' `
        -Diagnostics $diagnostics.Replace(
            '4: orphanResult := SdoAxis4.MarkOrphan(',
            '4: orphanResult := SdoAxis1.MarkOrphan('))
    (New-NegativeFixture `
        -Name 'LasalOrphanAxes1And4Swapped' `
        -Diagnostics $orphanAxisSwapDiagnostics)
    (New-NegativeFixture `
        -Name 'LasalOrphanReadyRaceNotDrained' `
        -Diagnostics $diagnostics.Replace(
            'if (orphanResult = 0) | (orphanResult = -2) then',
            'if orphanResult = 0 then'))
    (New-NegativeFixture `
        -Name 'LasalDrainDoesNotConsumeReadyCompletion' `
        -Diagnostics $diagnostics.Replace(
            'copyResult := SdoAxis1.CopyCompletion(',
            'copyResult := -2; // CopyCompletion('))
    (New-NegativeFixture `
        -Name 'LasalGenericSdoDropsEncoderExclusion' `
        -Diagnostics $diagnostics.Replace(
            'if EncoderMaintenanceState[152] <> 0 then',
            'if FALSE then'))
    (New-NegativeFixture `
        -Name 'LasalDs402DropsEncoderExclusion' `
        -Diagnostics $diagnostics.Replace(
            '       (EncoderMaintenanceState[152] <> 0) then',
            '       FALSE then'))
    (New-NegativeFixture `
        -Name 'LasalEncoderDropsDs402Exclusion' `
        -Diagnostics $diagnostics.Replace(
            '       (Ds402HomeState[92] <> 0) |',
            '       FALSE |'))
    (New-NegativeFixture `
        -Name 'TcpEncoderDriveOffsetChanged' `
        -Tcp $tcp.Replace(
            'ownerAxisReference := RequestBuf[48]$UINT;',
            'ownerAxisReference := RequestBuf[50]$UINT;'))
    (New-NegativeFixture `
        -Name 'TcpEncoderOwnerKindChanged' `
        -Tcp $tcp.Replace('OwnerKind:=5', 'OwnerKind:=1'))
    (New-NegativeFixture `
        -Name 'TcpEncoderOwnerGenerationOmitted' `
        -Tcp $tcp.Replace(
            '                OwnerGeneration:=ownerGeneration);',
            '                Reserved:=0);'))
    (New-NegativeFixture `
        -Name 'CSharpProtocolUsesTimeoutCycles' `
        -CSharpProtocol $csharpProtocol.Replace(
            'TimeoutMilliseconds', 'TimeoutCycles'))
    (New-NegativeFixture `
        -Name 'CSharpModelsUsesTimeoutCycles' `
        -CSharpModels $csharpModels.Replace(
            'TimeoutMilliseconds', 'TimeoutCycles'))
    (New-NegativeFixture `
        -Name 'CSharpMaximumExpanded' `
        -CSharpModels $csharpModels.Replace('60000', '60001'))
    (New-NegativeFixture `
        -Name 'CSharpUsesTwArrayLongAlias' `
        -CSharpModels $csharpModels.Replace('0x20FC', '0x3204'))
    (New-NegativeFixture `
        -Name 'CSharpTw20SubIndexChanged' `
        -CSharpModels $csharpModels.Replace(
            'Tw20ErrorWarningResetSubIndex = 0x02',
            'Tw20ErrorWarningResetSubIndex = 0x01'))
    (New-NegativeFixture `
        -Name 'CSharpDedicatedWriteLengthChanged' `
        -CSharpModels $csharpModels.Replace(
            'public const ushort WriteLength = 2',
            'public const ushort WriteLength = 4'))
    (New-NegativeFixture `
        -Name 'CSharpOutcomeKeyOffsetChanged' `
        -CSharpProtocol $csharpProtocol.Replace(
            'payloadOffset + 52', 'payloadOffset + 54'))
    (New-NegativeFixture `
        -Name 'CSharpOutcomeEchoOffsetChanged' `
        -CSharpProtocol $csharpProtocol.Replace(
            'ReadUInt32(payload, 64)', 'ReadUInt32(payload, 68)'))
)

@{
    Diagnostics = $diagnostics
    Tcp = $tcp
    CSharpProtocol = $csharpProtocol
    CSharpModels = $csharpModels
    NegativeFixtures = $negativeFixtures
}

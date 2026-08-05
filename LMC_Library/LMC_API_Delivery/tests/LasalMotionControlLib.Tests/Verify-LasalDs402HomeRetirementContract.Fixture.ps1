@{
    Diagnostics = @'
#define LMC_DIAG_DS402_HOME_ENABLED FALSE

FUNCTION GLOBAL LMCDiagnosticsService::HandleRequest
    if CommandId = 0x7D15 then
        ResponseSize := HandleAxisDs402HomeStart(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=RequestSize);
        RETURN;
    elsif CommandId = 0x7D16 then
        ResponseSize := HandleAxisDs402HomeOutcome(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=RequestSize);
        RETURN;
    elsif CommandId = 0x7D17 then
        ResponseSize := HandleAxisDs402HomeRetire(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=RequestSize);
        RETURN;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart
    VAR_INPUT
        Reference : UINT;
        pRequest : ^USINT;
        pResponse : ^USINT;
        ResponseCapacity : UDINT;
        CallerSessionEpoch : UDINT;
        RequestSequence : UDINT;
        AdmissionToken : UDINT;
        OwnerGeneration : UDINT;
        RequestSize : UDINT;
    END_VAR
    VAR_OUTPUT
        ResponseSize : DINT;
    END_VAR
    VAR
        rawStartState, baseStartState : UDINT;
        detailCode, nextGeneration : UDINT;
        recordBase, ownerResult, rollbackResult : DINT;
    END_VAR

    rawStartState := 0xFFFFFFFF;
    baseStartState := 0xFFFFFFFF;
    if (Reference >= 1) & (Reference <= 4) then
        recordBase := TO_DINT(Reference - 1) * 23;
        rawStartState := Ds402HomeState[recordBase]$UDINT;
        baseStartState := rawStartState and 0x00007FFF;
    end_if;
    if Reference = 0 then
        detailCode := 9;
    elsif ((rawStartState = 2) | (rawStartState = 3) |
           (rawStartState = 4)) then
        detailCode := 32;
    elsif (rawStartState <> 0) & (rawStartState <> 0x00008002) &
          (rawStartState <> 0x00008003) &
          (rawStartState <> 0x00008004) then
        detailCode := 10;
    else
        detailCode := 0;
    end_if;
    if detailCode = 0 then
        _memset(dest:=#Ds402HomeState[recordBase], usByte:=0, cntr:=92);
        _memset(dest:=#Ds402HomeState[92], usByte:=0, cntr:=68);
        _memset(dest:=#Ds402HomeState[110], usByte:=0, cntr:=72);
        Ds402HomeState[109] := nextGeneration$DINT;
        Ds402HomeState[125] := 1;
        Ds402HomeState[recordBase] := 1;
        Ds402HomeState[92] := 89;
    end_if;
    if detailCode = 0 then
        ownerResult := AxisOwnership.CommitAxisOwnership(
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration);
        if ownerResult = 0 then
            Ds402HomeState[125] := 2;
            Ds402HomeState[92] := 1;
        else
            detailCode := 10;
        end_if;
    end_if;
    if detailCode = 0 then
        (pResponse + 4)^$UINT := 0;
    end_if;
    if detailCode <> 0 then
        rollbackResult := -1;
        rollbackResult := AxisOwnership.RollbackAxisOwnership(
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration);
        if rollbackResult = 0 then
            _memset(dest:=#Ds402HomeState[recordBase], usByte:=0, cntr:=92);
            _memset(dest:=#Ds402HomeState[92], usByte:=0, cntr:=68);
            _memset(dest:=#Ds402HomeState[110], usByte:=0, cntr:=72);
        else
            detailCode := 42;
            Ds402HomeState[92] := 101;
        end_if;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeOutcome
    VAR_INPUT
        Reference : UINT;
        pRequest : ^USINT;
        pResponse : ^USINT;
        ResponseCapacity : UDINT;
        CallerSessionEpoch : UDINT;
        RequestSize : UDINT;
    END_VAR
    VAR_OUTPUT
        ResponseSize : DINT;
    END_VAR
    VAR
        schemaVersion, requestFlags : UINT;
        requestId, diagnosticsBuild, bootId, mapRevision : UDINT;
        originalRequestId, intent0, intent1, intent2, intent3 : UDINT;
        currentBootId : UDINT;
        rawOutcomeState, baseOutcomeState : UDINT;
        detailCode : UDINT;
        homingMethod, recordBase : DINT;
        outcomeStateValid, recordValid : BOOL;
    END_VAR

    if (Reference < 1) | (Reference > 4) then
        RETURN;
    end_if;
    schemaVersion := pRequest^$UINT;
    requestFlags := (pRequest + 2)^$UINT;
    requestId := (pRequest + 4)^$UDINT;
    diagnosticsBuild := (pRequest + 8)^$UDINT;
    bootId := (pRequest + 12)^$UDINT;
    mapRevision := (pRequest + 16)^$UDINT;
    originalRequestId := (pRequest + 20)^$UDINT;
    intent0 := (pRequest + 24)^$UDINT;
    intent1 := (pRequest + 28)^$UDINT;
    intent2 := (pRequest + 32)^$UDINT;
    intent3 := (pRequest + 36)^$UDINT;
    homingMethod := (pRequest + 40)^$DINT;
    currentBootId := GetDiagnosticsBootId();
    if (schemaVersion <> 1) | (requestFlags <> 0) | (requestId = 0) |
       (originalRequestId = 0) then
        detailCode := 28;
    elsif diagnosticsBuild <> 1 then
        detailCode := 16;
    elsif (currentBootId = 0) | (bootId <> currentBootId) then
        detailCode := 17;
    elsif mapRevision <> LMC_DIAG_MAP_REVISION then
        detailCode := 18;
    end_if;
    if (sizeof(Ds402HomeState) <> 512) |
       (CallerSessionEpoch = 0) then
        detailCode := 29;
        (pResponse + 12)^$UDINT := detailCode;
        ResponseSize := 16;
        RETURN;
    end_if;
    recordBase := TO_DINT(Reference - 1) * 23;
    rawOutcomeState := Ds402HomeState[recordBase]$UDINT;
    baseOutcomeState := rawOutcomeState and 0x00007FFF;
    outcomeStateValid :=
        (rawOutcomeState = 1) | (rawOutcomeState = 2) |
        (rawOutcomeState = 3) | (rawOutcomeState = 4) |
        (rawOutcomeState = 0x00008002) |
        (rawOutcomeState = 0x00008003) |
        (rawOutcomeState = 0x00008004);
    if rawOutcomeState = 0 then
        detailCode := 25;
    elsif outcomeStateValid = FALSE then
        detailCode := 27;
    elsif (baseOutcomeState = 1) & (Ds402HomeState[92] = 101) &
          (Ds402HomeState[93] = TO_DINT(Reference - 1)) then
        detailCode := 26;
    else
        detailCode := 0;
    end_if;
    if baseOutcomeState = 2 then
        recordValid :=
            (Ds402HomeState[recordBase + 18] = 0) &
            ((Ds402HomeState[recordBase + 17]$UDINT and 0x0008) = 0) &
            ((Ds402HomeState[recordBase + 17]$UDINT and 0x2000) = 0) &
            (((Ds402HomeState[recordBase + 17]$UDINT and 0x006F) = 0x0040) |
             ((Ds402HomeState[recordBase + 17]$UDINT and 0x006F) = 0x0021) |
             ((Ds402HomeState[recordBase + 17]$UDINT and 0x006F) = 0x0023) |
             ((Ds402HomeState[recordBase + 17]$UDINT and 0x006F) = 0x0027));
    end_if;
    if detailCode = 0 then
        (pResponse + 16)^$UINT := baseOutcomeState$UINT;
        ResponseSize := 92;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeRetire
    VAR_INPUT
        Reference : UINT;
        pRequest : ^USINT;
        pResponse : ^USINT;
        ResponseCapacity : UDINT;
        CallerSessionEpoch : UDINT;
        RequestSize : UDINT;
    END_VAR
    VAR_OUTPUT
        ResponseSize : DINT;
    END_VAR
    VAR
        schemaVersion, requestFlags : UINT;
        requestId, diagnosticsBuild, bootId, mapRevision : UDINT;
        originalRequestId, intent0, intent1, intent2, intent3 : UDINT;
        recordGeneration, detailCode, currentBootId : UDINT;
        rawRetireState, baseRetireState : UDINT;
        homingMethod, recordBase : DINT;
        retireStateValid : BOOL;
    END_VAR

    ResponseSize := -1;
    if (pResponse = NIL) | (ResponseCapacity < 16) then
        RETURN;
    end_if;
    _memset(dest:=pResponse, usByte:=0, cntr:=ResponseCapacity);
    pResponse^$UINT := LMC_DIAG_SCHEMA_VERSION;
    (pResponse + 4)^$UINT := 1;
    (pResponse + 6)^$INT := LMC_DIAG_ADMIN_ERROR_ID;
    requestId := 0;
    if (pRequest <> NIL) & (RequestSize >= 8) then
        requestId := (pRequest + 4)^$UDINT;
    end_if;
    (pResponse + 8)^$UDINT := requestId;
    ResponseSize := 16;
    detailCode := 28;
    if (pRequest = NIL) | (RequestSize <> 48) |
       (Reference < 1) | (Reference > 4) then
        (pResponse + 12)^$UDINT := detailCode;
        RETURN;
    end_if;
    if (sizeof(Ds402HomeState) <> 512) |
       (CallerSessionEpoch = 0) then
        detailCode := 29;
        (pResponse + 12)^$UDINT := detailCode;
        ResponseSize := 16;
        RETURN;
    end_if;

    schemaVersion := pRequest^$UINT;
    requestFlags := (pRequest + 2)^$UINT;
    requestId := (pRequest + 4)^$UDINT;
    diagnosticsBuild := (pRequest + 8)^$UDINT;
    bootId := (pRequest + 12)^$UDINT;
    mapRevision := (pRequest + 16)^$UDINT;
    originalRequestId := (pRequest + 20)^$UDINT;
    intent0 := (pRequest + 24)^$UDINT;
    intent1 := (pRequest + 28)^$UDINT;
    intent2 := (pRequest + 32)^$UDINT;
    intent3 := (pRequest + 36)^$UDINT;
    homingMethod := (pRequest + 40)^$DINT;
    recordGeneration := (pRequest + 44)^$UDINT;
    currentBootId := GetDiagnosticsBootId();
    if (schemaVersion <> 1) | (requestFlags <> 0) | (requestId = 0) |
       (originalRequestId = 0) | (recordGeneration = 0) then
        detailCode := 28;
    elsif diagnosticsBuild <> 1 then
        detailCode := 16;
    elsif (currentBootId = 0) | (bootId <> currentBootId) then
        detailCode := 17;
    elsif mapRevision <> LMC_DIAG_MAP_REVISION then
        detailCode := 18;
    end_if;
    recordBase := TO_DINT(Reference - 1) * 23;
    rawRetireState := Ds402HomeState[recordBase]$UDINT;
    baseRetireState := rawRetireState and 0x00007FFF;
    retireStateValid :=
        (rawRetireState = 2) | (rawRetireState = 3) |
        (rawRetireState = 4) | (rawRetireState = 0x00008002) |
        (rawRetireState = 0x00008003) |
        (rawRetireState = 0x00008004);

    if (schemaVersion <> 1) | (requestFlags <> 0) |
       (requestId = 0) | (recordGeneration = 0) then
        detailCode := 28;
    elsif rawRetireState = 0 then
        detailCode := 25;
    elsif rawRetireState = 1 then
        detailCode := 26;
    elsif retireStateValid = FALSE then
        detailCode := 27;
    elsif (Ds402HomeState[recordBase + 1]$UDINT <> diagnosticsBuild) |
          (Ds402HomeState[recordBase + 2]$UDINT <> bootId) |
          (Ds402HomeState[recordBase + 3]$UDINT <> mapRevision) |
          (Ds402HomeState[recordBase + 4]$UDINT <> originalRequestId) |
          (Ds402HomeState[recordBase + 5]$UDINT <> intent0) |
          (Ds402HomeState[recordBase + 6]$UDINT <> intent1) |
          (Ds402HomeState[recordBase + 7]$UDINT <> intent2) |
          (Ds402HomeState[recordBase + 8]$UDINT <> intent3) |
          (Ds402HomeState[recordBase + 9] <> homingMethod) |
          (Ds402HomeState[recordBase + 22]$UDINT <> recordGeneration) then
        detailCode := 28;
    else
        detailCode := 0;
    end_if;

    if detailCode = 0 then
        ResponseSize := HandleAxisDs402HomeOutcome(
            Reference:=Reference, pRequest:=pRequest, pResponse:=pResponse,
            ResponseCapacity:=ResponseCapacity,
            CallerSessionEpoch:=CallerSessionEpoch, RequestSize:=44);
        if ResponseSize <> 92 then
            RETURN;
        end_if;
        if (pResponse + 88)^$UDINT <> recordGeneration then
            detailCode := 28;
        end_if;
        if detailCode = 0 then
            if rawRetireState = baseRetireState then
                Ds402HomeState[recordBase] :=
                    TO_DINT(baseRetireState or 0x00008000);
            end_if;
        end_if;
    end_if;
    if detailCode <> 0 then
        (pResponse + 4)^$UINT := 1;
        (pResponse + 6)^$INT := LMC_DIAG_ADMIN_ERROR_ID;
        (pResponse + 12)^$UDINT := detailCode;
        ResponseSize := 16;
    end_if;
END_FUNCTION

FUNCTION LMCDiagnosticsService::ProcessAxisDs402Home
    VAR
        snapshot : ARRAY [0..303] OF USINT;
        preemptionSnapshot : ARRAY [0..215] OF USINT;
        serviceNow, serviceStart, cleanupStart, timeoutMs : UDINT;
        currentCycle, previousCycle, statusWord, baseState : UDINT;
        healthOffset, axisMask, admissionToken, ownerGeneration : UDINT;
        safetyAdmissionToken, safetyOwnerGeneration : UDINT;
        sessionEpoch, requestSequence, sdoToken : UDINT;
        axisReference : UINT;
        actualPosition, stage, activeIndex, recordBase, ownerResult : DINT;
        preemptionResult, orphanResult : DINT;
        newCycle, failure, cleanupExpired, preemptionCleanup : BOOL;
        preemptionSnapshotValid : BOOL;
    END_VAR

    stage := Ds402HomeState[92];
    activeIndex := Ds402HomeState[93];
    admissionToken := Ds402HomeState[122]$UDINT;
    ownerGeneration := Ds402HomeState[123]$UDINT;
    if stage = 89 then
        recordBase := activeIndex * 23;
        if ((Ds402HomeState[recordBase + 5] = 0) &
            (Ds402HomeState[recordBase + 6] = 0) &
            (Ds402HomeState[recordBase + 7] = 0) &
            (Ds402HomeState[recordBase + 8] = 0)) |
           (Ds402HomeState[94] <> 0) |
           (Ds402HomeState[113] <> 0) |
           (Ds402HomeState[116] <> 0) |
           (Ds402HomeState[117] <> 0) |
           (Ds402HomeState[127] <> 0) |
           ((Ds402HomeState[125] <> 1) &
            (Ds402HomeState[125] <> 2)) then
            Ds402HomeState[92] := 101;
            RETURN;
        end_if;
        ownerResult := AxisOwnership.ValidateAxisOwnership(
            RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
        if ownerResult = 0 then
            Ds402HomeState[125] := 2;
            Ds402HomeState[92] := 1;
            RETURN;
        end_if;
        ownerResult := AxisOwnership.ValidateAxisOwnership(
            RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED);
        if (ownerResult = 0) & (Ds402HomeState[125] = 1) then
            ownerResult := AxisOwnership.CommitAxisOwnership(
                AdmissionToken:=admissionToken);
            if ownerResult = 0 then
                Ds402HomeState[125] := 2;
                Ds402HomeState[92] := 1;
                RETURN;
            end_if;
            ownerResult := AxisOwnership.ValidateAxisOwnership(
                RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
            if ownerResult = 0 then
                Ds402HomeState[125] := 2;
                Ds402HomeState[92] := 1;
                RETURN;
            end_if;
        end_if;
        ownerResult := AxisOwnership.RollbackAxisOwnership(
            AdmissionToken:=admissionToken);
        if ownerResult = 0 then
            _memset(dest:=#Ds402HomeState[recordBase], usByte:=0, cntr:=92);
            _memset(dest:=#Ds402HomeState[92], usByte:=0, cntr:=68);
            _memset(dest:=#Ds402HomeState[110], usByte:=0, cntr:=72);
        else
            Ds402HomeState[92] := 101;
        end_if;
        RETURN;
    end_if;
    _memset(dest:=#preemptionSnapshot[0], usByte:=0,
        cntr:=sizeof(preemptionSnapshot));
    preemptionResult := AxisOwnership.CopyAxisOwnershipPreemption();
    if preemptionResult = LMC_DIAG_PREEMPT_PENDING_FREEZE then
        cleanupStart := Ds402HomeState[119]$UDINT;
        cleanupExpired := (cleanupStart <> 0) & (stage >= 90) &
            (stage <= 99) & ((ops.tAbsolute - cleanupStart) >= 1000);
        if cleanupExpired then
            Ds402HomeState[92] := 101;
        end_if;
        RETURN;
    end_if;
    if (Ds402HomeState[125] <> 2) then
        Ds402HomeState[92] := 101;
        RETURN;
    end_if;
    preemptionCleanup := FALSE;
    if preemptionResult <> 0 then
        preemptionSnapshotValid :=
            (preemptionSnapshot[0]$UDINT =
             LMC_DIAG_PREEMPT_SNAPSHOT_MAGIC) &
            (preemptionSnapshot[32]$UDINT = axisMask) &
            (preemptionSnapshot[36]$UDINT = admissionToken) &
            (preemptionSnapshot[40]$UDINT = ownerGeneration) &
            (preemptionSnapshot[108]$UDINT <> 0) &
            (preemptionSnapshot[112]$UDINT <> 0);
        if preemptionSnapshotValid = FALSE then
            Ds402HomeState[92] := 101;
            RETURN;
        end_if;
        safetyAdmissionToken := preemptionSnapshot[108]$UDINT;
        safetyOwnerGeneration := preemptionSnapshot[112]$UDINT;
        preemptionCleanup := TRUE;
    end_if;
    currentCycle := Ds402HomeState[110]$UDINT;
    previousCycle := currentCycle;
    currentCycle := snapshot[0]$UDINT;
    newCycle := currentCycle <> previousCycle;
    if newCycle then
        Ds402HomeState[110] := currentCycle$DINT;
    end_if;
    if snapshot[TO_DINT(healthOffset + 28)]$UDINT <> currentCycle then
        failure := TRUE;
    end_if;
    if (serviceNow - serviceStart) >= timeoutMs then
        Ds402HomeState[92] := 90;
    end_if;
    case Ds402HomeState[92] of
        32:
            Ds402HomeState[112] := currentCycle$DINT;
            Ds402HomeState[92] := 34;
        33:
            Ds402HomeState[112] := currentCycle$DINT;
            Ds402HomeState[92] := 34;
        34:
            if newCycle &
               (currentCycle <> Ds402HomeState[112]$UDINT) then
                if (Ds402HomeState[94] <> 0) |
                   (Ds402HomeState[113] <> 0) |
                   (Ds402HomeState[116] <> 0) |
                   (Ds402HomeState[117] <> 0) |
                   (Ds402HomeState[127] <> 0) |
                   ((statusWord and 0x0008) <> 0) |
                   ((statusWord and 0x2000) <> 0) |
                   ((baseState <> 0x0040) & (baseState <> 0x0021) &
                    (baseState <> 0x0023) & (baseState <> 0x0027)) |
                   (actualPosition <> 0) then
                    Ds402HomeState[92] := 90;
                else
                    ownerResult := AxisOwnership.PublishAxisOwnership(
                        ReportKind:=LMC_DIAG_OWNER_REPORT_TERMINAL_SUCCESS);
                    if ownerResult = 0 then
                        Ds402HomeState[recordBase] := 2;
                        Ds402HomeState[92] := 0;
                    end_if;
                end_if;
            end_if;
    end_case;
    cleanupStart := Ds402HomeState[119]$UDINT;
    cleanupExpired := (cleanupStart <> 0) & (stage >= 90) &
        (stage <= 99) & ((serviceNow - cleanupStart) >= 1000);
    if cleanupExpired then
        if Ds402HomeState[94] <> 0 then
            case axisReference of
                1: orphanResult := SdoAxis1.MarkOrphan(
                    Ds402HomeState[99]$UDINT);
                2: orphanResult := SdoAxis2.MarkOrphan(
                    Ds402HomeState[99]$UDINT);
                3: orphanResult := SdoAxis3.MarkOrphan(
                    Ds402HomeState[99]$UDINT);
                4: orphanResult := SdoAxis4.MarkOrphan(
                    Ds402HomeState[99]$UDINT);
            end_case;
            Ds402HomeState[127] := 1;
        end_if;
        if Ds402HomeState[113] <> 0 then
            Ds402HomeState[127] := 1;
        end_if;
        if preemptionCleanup then
            ownerResult := AxisOwnership.PublishAxisOwnershipPreemptionCleanup(
                ExpectedAxisMask:=axisMask,
                PreemptedAdmissionToken:=admissionToken,
                PreemptedOwnerGeneration:=ownerGeneration,
                SafetyAdmissionToken:=safetyAdmissionToken,
                SafetyOwnerGeneration:=safetyOwnerGeneration,
                CleanupKind:=
                    LMC_DIAG_PREEMPT_CLEANUP_INCOMPLETE_QUARANTINE);
        else
            ownerResult := AxisOwnership.PublishAxisOwnership(
                AxisMask:=axisMask,
                AdmissionToken:=admissionToken,
                OwnerGeneration:=ownerGeneration,
                ReportKind:=LMC_DIAG_OWNER_REPORT_QUARANTINE);
        end_if;
        Ds402HomeState[92] := 101;
        RETURN;
    end_if;
    case stage of
        90: Ds402HomeState[92] := 90;
    end_case;
END_FUNCTION
'@

    Tcp = @'
#define LMC_DIAG_DS402_PREFLIGHT_READY -2
FUNCTION TCPMotionInterface::MsgPaser
    VAR
        diagnosticsAxisMask : UDINT;
        diagnosticsAdmissionResult : DINT;
        diagnosticsResponseSize : DINT;
        diagnosticsOwnerReference : UINT;
        diagnosticsOwnerKind : UINT;
        diagnosticsResourceKind : UINT;
        diagnosticsDs402StartValid : BOOL;
        diagnosticsDs402PreflightAttempted : BOOL;
        diagnosticsDs402PreflightAccepted : BOOL;
    END_VAR
    diagnosticsDs402StartValid := FALSE;
    diagnosticsDs402PreflightAttempted := FALSE;
    diagnosticsDs402PreflightAccepted := FALSE;
    case CommandID of
        0x7D15, 0x7D16, 0x7D17,
        0x7E00:
            if (CommandID = 0x7E53) & (Payload = 72) then
                diagnosticsOwnerReference := RequestBuf[48]$UINT;
                diagnosticsOwnerKind := 5;
                diagnosticsResourceKind := 4;
            elsif (CommandID = 0x7D15) & (Payload = 72) then
                diagnosticsOwnerReference := AxisRef$UINT;
                diagnosticsOwnerKind := 4;
                diagnosticsResourceKind := 3;
            end_if;
            if (CommandID = 0x7D15) & (Payload = 72) &
               (diagnosticsAxisMask <> 0) then
                diagnosticsDs402StartValid :=
                    (RequestBuf[8]$UINT = 1) &
                    (RequestBuf[10]$UINT = 0) &
                    (RequestBuf[12]$UDINT <> 0) &
                    (RequestBuf[16]$UDINT <> 0) &
                    (RequestBuf[20]$UDINT <> 0) &
                    (RequestBuf[24]$UDINT <> 0) &
                    ((RequestBuf[28]$UDINT <> 0) |
                     (RequestBuf[32]$UDINT <> 0) |
                     (RequestBuf[36]$UDINT <> 0) |
                     (RequestBuf[40]$UDINT <> 0)) &
                    (RequestBuf[44]$DINT = 37) &
                    (RequestBuf[48]$DINT = 0) &
                    (RequestBuf[52]$DINT = 0) &
                    (RequestBuf[56]$DINT = 0) &
                    (RequestBuf[60]$DINT = 0) &
                    (RequestBuf[64]$DINT = 0) &
                    (RequestBuf[68]$UINT = 1) &
                    (RequestBuf[70]$UINT = 0) &
                    (RequestBuf[72]$UDINT <> 0) &
                    (RequestBuf[76]$UDINT = 0x32303448);
            end_if;
            if diagnosticsDs402StartValid &
               IsClientConnected(#Diagnostics) &
               IsClientConnected(#ControlCommands) then
                diagnosticsDs402PreflightAttempted := TRUE;
                diagnosticsResponseSize := Diagnostics.HandleRequest(
                    CommandId:=CommandID$UINT,
                    Reference:=AxisRef$UINT,
                    pRequest:=(#RequestBuf[8])$^USINT,
                    RequestSize:=Payload$UDINT,
                    pResponse:=(#Sendbuf[8])$^USINT,
                    ResponseCapacity:=2040,
                    CallerSessionEpoch:=ActiveRequest.SessionEpoch,
                    RequestSequence:=ActiveRequest.Sequence,
                    AdmissionToken:=0,
                    OwnerGeneration:=0);
                diagnosticsDs402PreflightAccepted :=
                    diagnosticsResponseSize = LMC_DIAG_DS402_PREFLIGHT_READY;
            end_if;
            if (diagnosticsAxisMask <> 0) &
               ((CommandID = 0x7E53) |
                (diagnosticsDs402StartValid &
                 diagnosticsDs402PreflightAccepted)) &
               IsClientConnected(#Diagnostics) &
               IsClientConnected(#ControlCommands) then
                diagnosticsAdmissionResult :=
                    ControlCommands.ReserveAxisOwnership(
                        CommandId:=CommandID$UINT);
            elsif diagnosticsDs402StartValid then
                diagnosticsAdmissionResult := -3;
            end_if;
            if diagnosticsDs402StartValid &
               (diagnosticsAdmissionResult <> 0) &
               ((diagnosticsDs402PreflightAttempted = FALSE) |
                diagnosticsDs402PreflightAccepted) then
                _memset(dest:=#Sendbuf[8], usByte:=0, cntr:=24);
                Sendbuf[8]$UINT := 1;
                Sendbuf[12]$UINT := 1;
                Sendbuf[14]$INT := -31000;
                Sendbuf[16]$UDINT := RequestBuf[12]$UDINT;
                if diagnosticsAdmissionResult = -2 then
                    Sendbuf[20]$UDINT := 41;
                else
                    Sendbuf[20]$UDINT := 42;
                end_if;
                Sendbuf[24]$DINT := RequestBuf[44]$DINT;
                Sendbuf[28]$UDINT := 0;
                diagnosticsResponseSize := 24;
            end_if;
            if ((diagnosticsDs402StartValid = FALSE) |
                (diagnosticsDs402PreflightAccepted &
                 (diagnosticsAdmissionResult = 0))) &
               IsClientConnected(#Diagnostics) then
                diagnosticsResponseSize := Diagnostics.HandleRequest(
                    CommandId:=CommandID$UINT,
                    Reference:=AxisRef$UINT,
                    pRequest:=(#RequestBuf[8])$^USINT,
                    RequestSize:=Payload$UDINT,
                    pResponse:=(#Sendbuf[8])$^USINT,
                    ResponseCapacity:=2040,
                    CallerSessionEpoch:=ActiveRequest.SessionEpoch,
                    RequestSequence:=ActiveRequest.Sequence,
                    AdmissionToken:=diagnosticsAdmissionToken,
                    OwnerGeneration:=diagnosticsOwnerGeneration);
            end_if;
    end_case;
END_FUNCTION
'@

    Control = @'
FUNCTION LMCControlCommandService::HandleAdminCommands
    case CommandId of
        0x7D00:
            (pResponseFrame + 24)^$UDINT := 0x00000007;
            (pResponseFrame + 44)^$UINT := 4;
        0x7D10:
            ResponseSize := 36;
    end_case;
END_FUNCTION
'@
}

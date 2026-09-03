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
        -OwnershipActivationGuardSelfTestOnly
    return
}

$control = @'
#define LMC_ADMIN_AXIS_HOME_ENABLED TRUE
#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE
#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9
#define LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F
#define LMC_OWNER_STARTUP_STATE_MAGIC 0x4F575350
#define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353
#define LMC_OWNER_STARTUP_LATCH_REQUIRED 0x0000001F
#define LMC_OWNER_STARTUP_DIAG_REQUIRED 0x0000001F
#define LMC_OWNER_STARTUP_STABLE_SAMPLES 3
#define LMC_OWNER_STARTUP_STABLE_MS 100
#define LMC_OWNER_STARTUP_AXIS_CLEAR_MASK 0x05028890
#define LMC_OWNER_STARTUP_AXIS_LOCK_MASK 0x01000800
#define LMC_OWNER_TABLE_MAGIC 0x4C4D434F
#define LMC_OWNER_STATE_IDLE 0
#define LMC_OWNER_STATE_RESERVED 1
#define LMC_OWNER_STATE_DIRECT_ACTIVE 2
#define LMC_OWNER_STATE_GROUP_LEASE 3
#define LMC_OWNER_STATE_GROUP_ACTIVE 4
#define LMC_OWNER_STATE_LMC_HOME_ACTIVE 5
#define LMC_OWNER_STATE_DS402_HOME_ACTIVE 6
#define LMC_OWNER_STATE_TW20_QUEUED 7
#define LMC_OWNER_STATE_TW20_RUNNING 8
#define LMC_OWNER_STATE_TW20_DRAINING 9
#define LMC_OWNER_STATE_SAFETY_PREEMPTING 10
#define LMC_OWNER_STATE_QUARANTINED 11
#define LMC_OWNER_KIND_DIRECT 1
#define LMC_OWNER_KIND_GROUP 2
#define LMC_OWNER_KIND_LMC_HOME 3
#define LMC_OWNER_KIND_DS402_HOME 4
#define LMC_OWNER_KIND_ENCODER 5
#define LMC_OWNER_RESOURCE_AXIS 1
#define LMC_OWNER_RESOURCE_LMC_HOME_ENGINE 2
#define LMC_OWNER_RESOURCE_DS402_HOME_ENGINE 3
#define LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE 4
#define LMC_OWNER_ADMISSION_ORDINARY 1
#define LMC_OWNER_ADMISSION_SAFETY 2
#define LMC_OWNER_ADMISSION_READ 3
#define LMC_OWNER_ADMISSION_LIFECYCLE 4
#define LMC_OWNER_PHASE_RESERVED 1
#define LMC_OWNER_PHASE_ACTIVE 2
#define LMC_OWNER_PHASE_SESSION_CLOSED_ROLLBACK 3
#define LMC_OWNER_PROFILE_AXIS_MASK 0x0000000F
#define LMC_OWNER_ROBOT_AXIS_MASK 0x000001FF
#define LMC_OWNER_OBSERVER_STRIDE 12
#define LMC_HOME_RECORD_RUNNING 1
#define LMC_HOME_RECORD_QUARANTINED 5
#define LMC_HOME_RECORD_MAGIC 0x4C4D4348
#define LMC_OWNER_AXIS_COUNT 9
#define LMC_OWNER_GLOBAL_SLOTS 28
#define LMC_OWNER_AXIS_STRIDE 36
#define LMC_OWNER_AXIS_RECORD_MAGIC 0x4F574E00
OwnershipState : ARRAY [0..351] OF DINT;
OwnershipStartupState : ARRAY [0..15] OF DINT;
OwnershipObserverState : ARRAY [0..107] OF DINT;
OwnershipLeaseState : ARRAY [0..323] OF DINT;
OwnershipPreemptedState : ARRAY [0..323] OF DINT;
OwnershipIdentityState : ARRAY [0..431] OF DINT;
OwnershipLeaseIdentityState : ARRAY [0..323] OF DINT;
OwnershipPreemptedIdentityState : ARRAY [0..431] OF DINT;
ZeroHomeState : ARRAY [0..63] OF DINT;
InputLatch : CltChCmd_LMCEcatInputLatch;
FUNCTION GLOBAL LMCControlCommandService::HandleRequest
ownershipArmed := FALSE;
ownershipInvokeHandler := TRUE;
ownershipValidationResult := -1;
ownershipAccepted := FALSE;
ownershipRequestShapeValid := FALSE;
ownershipManagedCommand := FALSE;
ownershipSafetyPumpRejected := FALSE;
if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED then
    case CommandId of
        0x2022, 0x2023, 0x2024, 0x209F, 0x20A0, 0x20A2,
        0x2047, 0x2048, 0x2049, 0x204A, 0x204B, 0x2085,
        0x20A4, 0x20E7, 0x7D22:
            ownershipManagedCommand := TRUE;
    else
    end_case;
    if ownershipManagedCommand then
        case CommandId of
            0x2023:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 8) &
                    (Reference >= 1) & (Reference <= 9);
            0x2024:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 1) &
                    (Reference >= 1) & (Reference <= 9);
            0x2022:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 16) &
                    (Reference >= 1) & (Reference <= 9);
            0x209F, 0x20A0:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 32) &
                    (Reference >= 1) & (Reference <= 9);
            0x20A2:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 24) &
                    (Reference >= 1) & (Reference <= 9);
            0x2047, 0x2048, 0x2049, 0x204A, 0x204B:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 1) &
                    (Reference = 0x0100);
            0x2085:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 16) &
                    (Reference = 0x0100);
            0x20A4:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 96) &
                    (Reference = 0x0100);
            0x7D22:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 104) &
                    (Reference = 0x0100);
            0x20E7:
                ownershipRequestShapeValid :=
                    (ownershipPayloadSize = 1320) &
                    (Reference = 0x0100);
        else
        end_case;
    end_if;
    ownershipArmed := ownershipManagedCommand &
        (ownershipRequestShapeValid |
         (AdmissionToken <> 0) | (OwnerGeneration <> 0));
    if ownershipManagedCommand & (ownershipRequestShapeValid = FALSE) then
        ownershipInvokeHandler := FALSE;
    end_if;
end_if;
if ownershipArmed then
    ownershipValidationResult := ValidateAxisOwnershipIdentity(
        CommandId:=CommandId,
        Reference:=Reference,
        ExpectedAxisMask:=ownershipAxisMask,
        OwnerKind:=ownershipOwnerKind,
        ResourceKind:=ownershipResourceKind,
        AdmissionMode:=ownershipAdmissionMode,
        CallerSessionEpoch:=CallerSessionEpoch,
        RequestSequence:=RequestSequence,
        AdmissionToken:=AdmissionToken,
        OwnerGeneration:=OwnerGeneration,
        RequiredPhase:=LMC_OWNER_PHASE_RESERVED,
        pIdentity:=(pRequestFrame + 8)$^void,
        IdentitySize:=ownershipPayloadSize);
    if ownershipValidationResult <> 0 then
        ownershipInvokeHandler := FALSE;
        (pResponseFrame + 10)^$INT := -9;
        (pResponseFrame + 14)^$INT := -9;
    end_if;
end_if;
if ownershipInvokeHandler then
    case CommandId of
        0x2022: ResponseSize := HandleAxisCommands();
        0x2047: ResponseSize := HandleGroupCommands();
        0x7D22: ResponseSize := HandleAdminCommands();
    else
    end_case;
end_if;
if ownershipArmed & (ownershipSafetyPumpRejected = FALSE) &
   (ownershipValidationResult = 0) then
    if CommandId = 0x7D22 then
        if (ResponseSize = 24) & (ResponseCapacity >= 24) then
            ownershipAccepted :=
                ((pResponseFrame + 12)^$UINT = 0) &
                ((pResponseFrame + 14)^$INT = 0) &
                ((pResponseFrame + 20)^$UDINT = 0);
        end_if;
    elsif CommandId = 0x20E7 then
        if (ResponseSize = 12) & (ResponseCapacity >= 12) then
            ownershipAccepted :=
                ((pResponseFrame + 8)^$UINT = 0) &
                ((pResponseFrame + 10)^$INT = 0);
        end_if;
    else
        if (ResponseSize = 16) & (ResponseCapacity >= 16) then
            ownershipAccepted :=
                ((pResponseFrame + 12)^$UINT = 0) &
                ((pResponseFrame + 14)^$INT = 0);
        end_if;
    end_if;
    if ownershipAccepted then
        ownershipCommitResult := CommitAxisOwnership(
            CommandId:=CommandId,
            Reference:=Reference,
            ExpectedAxisMask:=ownershipAxisMask,
            CallerSessionEpoch:=CallerSessionEpoch,
            RequestSequence:=RequestSequence,
            AdmissionToken:=AdmissionToken,
            OwnerGeneration:=OwnerGeneration);
    end_if;
end_if;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ReserveAxisOwnership
if (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
   ((OwnerKind = LMC_OWNER_KIND_DIRECT) |
    (OwnerKind = LMC_OWNER_KIND_GROUP)) then
    if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED = FALSE then
        Result := -3;
        RETURN;
    end_if;
    case Reference of
        1: referenceAxisMask := 0x00000001;
        2: referenceAxisMask := 0x00000002;
        3: referenceAxisMask := 0x00000004;
        4: referenceAxisMask := 0x00000008;
        5: referenceAxisMask := 0x00000010;
        6: referenceAxisMask := 0x00000020;
        7: referenceAxisMask := 0x00000040;
        8: referenceAxisMask := 0x00000080;
        9: referenceAxisMask := 0x00000100;
    else
    end_case;
    case CommandId of
        0x2022:
            if (OwnerKind <> LMC_OWNER_KIND_DIRECT) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_SAFETY) |
               (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
        0x2023:
            if (OwnerKind <> LMC_OWNER_KIND_DIRECT) |
               ((AdmissionMode <> LMC_OWNER_ADMISSION_ORDINARY) &
                (AdmissionMode <> LMC_OWNER_ADMISSION_SAFETY)) |
               (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
        0x2024, 0x209F, 0x20A0, 0x20A2:
            if (OwnerKind <> LMC_OWNER_KIND_DIRECT) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_ORDINARY) |
               (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
        0x2047, 0x20A4, 0x7D22:
            if (OwnerKind <> LMC_OWNER_KIND_GROUP) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_ORDINARY) |
               (RequestedAxisMask <> LMC_OWNER_PROFILE_AXIS_MASK) |
               (Reference <> 0x0100) then RETURN; end_if;
        0x2048, 0x2085:
            if (OwnerKind <> LMC_OWNER_KIND_GROUP) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_SAFETY) |
               (RequestedAxisMask <> LMC_OWNER_PROFILE_AXIS_MASK) |
               (Reference <> 0x0100) then RETURN; end_if;
        0x2049, 0x204A:
            if (OwnerKind <> LMC_OWNER_KIND_GROUP) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_ORDINARY) |
               (RequestedAxisMask <> LMC_OWNER_ROBOT_AXIS_MASK) |
               (Reference <> 0x0100) then RETURN; end_if;
        0x204B:
            if (OwnerKind <> LMC_OWNER_KIND_GROUP) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_SAFETY) |
               (RequestedAxisMask <> LMC_OWNER_ROBOT_AXIS_MASK) |
               (Reference <> 0x0100) then RETURN; end_if;
        0x20E7:
            if (OwnerKind <> LMC_OWNER_KIND_GROUP) |
               (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
               (RequestedAxisMask <> LMC_OWNER_ROBOT_AXIS_MASK) |
               (Reference <> 0x0100) then RETURN; end_if;
    else
        RETURN;
    end_case;
elsif ResourceKind = LMC_OWNER_RESOURCE_LMC_HOME_ENGINE then
    if (OwnerKind <> LMC_OWNER_KIND_LMC_HOME) |
       (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
       (CommandId <> 0x7D13) | (referenceAxisMask = 0) |
       (referenceAxisMask > 0x00000008) |
       (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
elsif ResourceKind = LMC_OWNER_RESOURCE_DS402_HOME_ENGINE then
    if (OwnerKind <> LMC_OWNER_KIND_DS402_HOME) |
       (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
       (CommandId <> 0x7D15) | (referenceAxisMask = 0) |
       (referenceAxisMask > 0x00000008) |
       (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
elsif ResourceKind = LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE then
    if (OwnerKind <> LMC_OWNER_KIND_ENCODER) |
       (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) |
       (CommandId <> 0x7E53) | (referenceAxisMask = 0) |
       (referenceAxisMask > 0x00000008) |
       (RequestedAxisMask <> referenceAxisMask) then RETURN; end_if;
else
    RETURN;
end_if;
if OwnershipState[recordBase + 1] <> LMC_OWNER_STATE_IDLE then RETURN; end_if;
OwnershipState[recordBase + 1] := LMC_OWNER_STATE_RESERVED;
OwnershipState[recordBase + 11]$UDINT := RequestedAxisMask;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership
VAR_INPUT
    RequiredPhase : UINT;
END_VAR
Result := -1;
if (RequiredPhase <> LMC_OWNER_PHASE_RESERVED) &
   (RequiredPhase <> LMC_OWNER_PHASE_ACTIVE) &
   (RequiredPhase <> LMC_OWNER_PHASE_SESSION_CLOSED_ROLLBACK) then
    RETURN;
end_if;
referenceAxisMask := 0;
case Reference of
    1: referenceAxisMask := 0x00000001;
    2: referenceAxisMask := 0x00000002;
    3: referenceAxisMask := 0x00000004;
    4: referenceAxisMask := 0x00000008;
    5: referenceAxisMask := 0x00000010;
    6: referenceAxisMask := 0x00000020;
    7: referenceAxisMask := 0x00000040;
    8: referenceAxisMask := 0x00000080;
    9: referenceAxisMask := 0x00000100;
else
end_case;
tupleValid := FALSE;
case CommandId of
    0x2022:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_DIRECT) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_SAFETY) &
            (referenceAxisMask <> 0) &
            (ExpectedAxisMask <= LMC_OWNER_ROBOT_AXIS_MASK) &
            ((ExpectedAxisMask and referenceAxisMask) <> 0);
    0x2023:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_DIRECT) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (referenceAxisMask <> 0) &
            (((AdmissionMode = LMC_OWNER_ADMISSION_ORDINARY) &
              (ExpectedAxisMask = referenceAxisMask)) |
             ((AdmissionMode = LMC_OWNER_ADMISSION_SAFETY) &
              (ExpectedAxisMask <= LMC_OWNER_ROBOT_AXIS_MASK) &
              ((ExpectedAxisMask and referenceAxisMask) <> 0)));
    0x2024, 0x209F, 0x20A0, 0x20A2:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_DIRECT) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_ORDINARY) &
            (referenceAxisMask <> 0) &
            (ExpectedAxisMask = referenceAxisMask);
    0x2047, 0x20A4, 0x7D22:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_ORDINARY) &
            (Reference = 0x0100) &
            (ExpectedAxisMask = LMC_OWNER_PROFILE_AXIS_MASK);
    0x2048:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_SAFETY) &
            (Reference = 0x0100) &
            (ExpectedAxisMask = LMC_OWNER_PROFILE_AXIS_MASK);
    0x2085:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_SAFETY) &
            (Reference = 0x0100) &
            ((ExpectedAxisMask = LMC_OWNER_PROFILE_AXIS_MASK) |
             (ExpectedAxisMask = LMC_OWNER_ROBOT_AXIS_MASK));
    0x2049, 0x204A:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_ORDINARY) &
            (Reference = 0x0100) &
            (ExpectedAxisMask = LMC_OWNER_ROBOT_AXIS_MASK);
    0x204B:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_SAFETY) &
            (Reference = 0x0100) &
            (ExpectedAxisMask = LMC_OWNER_ROBOT_AXIS_MASK);
    0x20E7:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_GROUP) &
            (ResourceKind = LMC_OWNER_RESOURCE_AXIS) &
            (AdmissionMode = LMC_OWNER_ADMISSION_LIFECYCLE) &
            (Reference = 0x0100) &
            (ExpectedAxisMask = LMC_OWNER_ROBOT_AXIS_MASK);
    0x7D13:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_LMC_HOME) &
            (ResourceKind = LMC_OWNER_RESOURCE_LMC_HOME_ENGINE) &
            (AdmissionMode = LMC_OWNER_ADMISSION_LIFECYCLE) &
            (referenceAxisMask <> 0) & (referenceAxisMask <= 0x00000008) &
            (ExpectedAxisMask = referenceAxisMask);
    0x7D15:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_DS402_HOME) &
            (ResourceKind = LMC_OWNER_RESOURCE_DS402_HOME_ENGINE) &
            (AdmissionMode = LMC_OWNER_ADMISSION_LIFECYCLE) &
            (referenceAxisMask <> 0) & (referenceAxisMask <= 0x00000008) &
            (ExpectedAxisMask = referenceAxisMask);
    0x7E53:
        tupleValid := (OwnerKind = LMC_OWNER_KIND_ENCODER) &
            (ResourceKind = LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE) &
            (AdmissionMode = LMC_OWNER_ADMISSION_LIFECYCLE) &
            (referenceAxisMask <> 0) & (referenceAxisMask <= 0x00000008) &
            (ExpectedAxisMask = referenceAxisMask);
else
end_case;
if tupleValid = FALSE then RETURN; end_if;
expectedState := LMC_OWNER_STATE_RESERVED;
if RequiredPhase = LMC_OWNER_PHASE_SESSION_CLOSED_ROLLBACK then
    if (CommandId <> 0x7D15) |
       (OwnerKind <> LMC_OWNER_KIND_DS402_HOME) |
       (ResourceKind <> LMC_OWNER_RESOURCE_DS402_HOME_ENGINE) |
       (AdmissionMode <> LMC_OWNER_ADMISSION_LIFECYCLE) then
        RETURN;
    end_if;
    expectedState := LMC_OWNER_STATE_QUARANTINED;
elsif RequiredPhase = LMC_OWNER_PHASE_ACTIVE then
    if AdmissionMode = LMC_OWNER_ADMISSION_SAFETY then
        if (OwnerKind = LMC_OWNER_KIND_DIRECT) |
           (OwnerKind = LMC_OWNER_KIND_GROUP) then
            expectedState := LMC_OWNER_STATE_SAFETY_PREEMPTING;
        else
            RETURN;
        end_if;
    else
        case OwnerKind of
            LMC_OWNER_KIND_DIRECT: expectedState := LMC_OWNER_STATE_DIRECT_ACTIVE;
            LMC_OWNER_KIND_GROUP: expectedState := LMC_OWNER_STATE_GROUP_ACTIVE;
            LMC_OWNER_KIND_LMC_HOME: expectedState := LMC_OWNER_STATE_LMC_HOME_ACTIVE;
            LMC_OWNER_KIND_DS402_HOME: expectedState := LMC_OWNER_STATE_DS402_HOME_ACTIVE;
            LMC_OWNER_KIND_ENCODER: expectedState := LMC_OWNER_STATE_TW20_QUEUED;
        else
            RETURN;
        end_case;
    end_if;
end_if;
if (OwnershipState[recordBase + 1] = LMC_OWNER_STATE_QUARANTINED) &
   (RequiredPhase <> LMC_OWNER_PHASE_SESSION_CLOSED_ROLLBACK) then
    Result := -3;
    RETURN;
elsif OwnershipState[recordBase + 1] <> expectedState then
    RETURN;
end_if;
check := AdmissionToken + OwnerGeneration + CallerSessionEpoch + RequestSequence;
if selected then
    exact := TRUE;
elsif (OwnershipState[recordBase + 4]$UDINT = AdmissionToken) |
      (OwnershipState[recordBase + 5]$UDINT = OwnerGeneration) then
    RETURN;
end_if;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnershipIdentity
Result := ValidateAxisOwnership(
    CommandId:=CommandId,
    Reference:=Reference,
    ExpectedAxisMask:=ExpectedAxisMask,
    OwnerKind:=OwnerKind,
    ResourceKind:=ResourceKind,
    AdmissionMode:=AdmissionMode,
    CallerSessionEpoch:=CallerSessionEpoch,
    RequestSequence:=RequestSequence,
    AdmissionToken:=AdmissionToken,
    OwnerGeneration:=OwnerGeneration,
    RequiredPhase:=RequiredPhase);
if Result <> 0 then RETURN; end_if;
identityValid := (pIdentity <> NIL) & (IdentitySize > 0);
if identityValid = FALSE then Result := -2; end_if;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::CommitAxisOwnership
axisIndex := 1;
while axisIndex <= LMC_OWNER_AXIS_COUNT do
    if selected then
        ownerKind := OwnershipState[recordBase + 2]$UINT;
        exit;
    end_if;
    axisIndex += 1;
end_while;
Result := ValidateAxisOwnership(
    CommandId:=CommandId,
    Reference:=Reference,
    ExpectedAxisMask:=ExpectedAxisMask,
    OwnerKind:=ownerKind,
    ResourceKind:=resourceKind,
    AdmissionMode:=admissionMode,
    CallerSessionEpoch:=CallerSessionEpoch,
    RequestSequence:=RequestSequence,
    AdmissionToken:=AdmissionToken,
    OwnerGeneration:=OwnerGeneration,
    RequiredPhase:=LMC_OWNER_PHASE_RESERVED);
if Result <> 0 then
    RETURN;
end_if;
axisIndex := 1;
while axisIndex <= LMC_OWNER_AXIS_COUNT do
    if selected then
        OwnershipState[recordBase + 1] := activeState;
    end_if;
    axisIndex += 1;
end_while;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::RollbackAxisOwnership
expectedAxisMask := OwnershipState[recordBase + 11]$UDINT;
if (expectedAxisMask and axisBit) <> 0 then
if (OwnershipState[recordBase + 6]$UDINT <> CallerSessionEpoch) |
       (OwnershipState[recordBase + 7]$UDINT <> RequestSequence) |
       (OwnershipState[recordBase + 11]$UDINT <> expectedAxisMask) then RETURN; end_if;
end_if;
expectedResourceKind := OwnershipState[recordBase + 13];
resourceValid := FALSE;
case expectedResourceKind of
1: resourceValid := TRUE;
else
end_case;
if Reason = 0 then clear := TRUE; end_if;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::PublishAxisOwnership
if OwnershipState[recordBase + 11]$UDINT <> AxisMask then RETURN; end_if;
if selected then
    expectedResourceKind := OwnershipState[recordBase + 13];
elsif (OwnershipState[recordBase + 4]$UDINT = AdmissionToken) |
      (OwnershipState[recordBase + 5]$UDINT = OwnerGeneration) then
    RETURN;
end_if;
resourceValid := FALSE;
case expectedResourceKind of
1: resourceValid := TRUE;
else
end_case;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ReconcileAxisOwnershipStartup
VAR_INPUT
    DiagnosticsBootId : UDINT;
    ObservationCycle : UDINT;
    ReportCycle : UDINT;
    DiagnosticsDrainFlags : UDINT;
END_VAR
VAR_OUTPUT
    Result : DINT;
END_VAR
Result := -1;
if (DiagnosticsBootId = 0) | (ObservationCycle = 0) | (ReportCycle = 0) then RETURN; end_if;
if (OwnershipState[0]$UDINT = LMC_OWNER_TABLE_MAGIC) &
   (OwnershipState[3]$UDINT = DiagnosticsBootId) then
    if (OwnershipState[4]$UDINT <> 0) &
       (OwnershipState[5]$UDINT = LMC_OWNER_STARTUP_PROOF_REQUIRED) &
       (OwnershipState[6] = 0) & (OwnershipState[24] = 0) then
        Result := 0;
    else
        Result := -3;
    end_if;
    RETURN;
end_if;
copyResult := InputLatch.CopyAxisOwnershipStartupSnapshot(
    pDest:=#startupSnapshot[0], DestSize:=48);
if (copyResult <> 0) |
   (startupSnapshot[0] <> LMC_OWNER_STARTUP_SNAPSHOT_MAGIC) |
   (startupSnapshot[1] = 0) |
   (startupSnapshot[1] <> ObservationCycle) |
   (startupSnapshot[11] <> 0) then
    Result := 1;
    RETURN;
end_if;
physicalIdle := ((startupSnapshot[10] and 0x00000001) <> 0) &
    ((startupSnapshot[2] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
    ((startupSnapshot[2] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0) &
    ((startupSnapshot[3] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
    ((startupSnapshot[3] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0) &
    ((startupSnapshot[4] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
    ((startupSnapshot[4] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0) &
    ((startupSnapshot[5] and LMC_AXIS_STATUS_STANDSTILL) <> 0) &
    ((startupSnapshot[5] and LMC_OWNER_STARTUP_AXIS_CLEAR_MASK) = 0);
axisClientsReady := (IsClientConnected(#LMCAxis1) <> 0) &
    (IsClientConnected(#LMCAxis2) <> 0) &
    (IsClientConnected(#LMCAxis3) <> 0) &
    (IsClientConnected(#LMCAxis4) <> 0);
robotConnected := IsClientConnected(#LMCRobot) <> 0;
robotState := LMCRobot.ReadRobotParameter(ParNo:=_ROBOT_STATE, Mode:=0);
profileLock := LMCRobot.ReadProfileParameter(ParNo:=_LMCPROF_LockState);
profileFinished := LMCRobot.ProfileInPosition(Mode:=_LMCPROF_ProfileFinished);
groupIdle := (profileLock = 0) &
    ((startupSnapshot[2] and LMC_OWNER_STARTUP_AXIS_LOCK_MASK) = 0) &
    ((startupSnapshot[3] and LMC_OWNER_STARTUP_AXIS_LOCK_MASK) = 0) &
    ((startupSnapshot[4] and LMC_OWNER_STARTUP_AXIS_LOCK_MASK) = 0) &
    ((startupSnapshot[5] and LMC_OWNER_STARTUP_AXIS_LOCK_MASK) = 0) &
    ((robotState = _ROBOT_PASSIVE) |
     ((robotState = _ROBOT_DIRECT) & profileFinished));
if (startupSnapshot[10] <> LMC_OWNER_STARTUP_LATCH_REQUIRED) |
   (DiagnosticsDrainFlags <> LMC_OWNER_STARTUP_DIAG_REQUIRED) |
   (ZeroHomeState[0] = LMC_HOME_RECORD_RUNNING) |
   (ZeroHomeState[0] = LMC_HOME_RECORD_QUARANTINED) then
    Result := 1;
    RETURN;
end_if;
proofFlags := 0x0000000F;
groupSignature := robotState$UDINT + profileLock$UDINT;
if OwnershipStartupState[2]$UDINT = ObservationCycle then
    Result := 1;
    RETURN;
end_if;
OwnershipStartupState[0]$UDINT := LMC_OWNER_STARTUP_STATE_MAGIC;
OwnershipStartupState[1]$UDINT := DiagnosticsBootId;
OwnershipStartupState[2]$UDINT := ObservationCycle;
OwnershipStartupState[3] += 1;
OwnershipStartupState[4]$UDINT := proofFlags;
OwnershipStartupState[5]$UDINT := groupSignature;
OwnershipStartupState[6]$UDINT := startupSnapshot[8]$UDINT;
OwnershipStartupState[7]$UDINT := startupSnapshot[12]$UDINT;
OwnershipStartupState[8]$UDINT := startupSnapshot[16]$UDINT;
OwnershipStartupState[9]$UDINT := startupSnapshot[20]$UDINT;
OwnershipStartupState[10]$UDINT := startupSnapshot[24]$UDINT;
OwnershipStartupState[11]$UDINT := startupSnapshot[28]$UDINT;
OwnershipStartupState[12]$UDINT := startupSnapshot[32]$UDINT;
OwnershipStartupState[13]$UDINT := startupSnapshot[36]$UDINT;
if OwnershipStartupState[14]$UDINT = 0 then OwnershipStartupState[14]$UDINT := ReportCycle; end_if;
OwnershipStartupState[15] := 0;
if OwnershipStartupState[3] < LMC_OWNER_STARTUP_STABLE_SAMPLES then Result := 1; RETURN; end_if;
if ReportCycle - OwnershipStartupState[14]$UDINT < LMC_OWNER_STARTUP_STABLE_MS then Result := 1; RETURN; end_if;
if proofFlags <> LMC_OWNER_STARTUP_PROOF_REQUIRED then Result := 1; RETURN; end_if;
_memset(dest:=#OwnershipState[0], usByte:=0, cntr:=1408);
OwnershipState[0]$UDINT := LMC_OWNER_TABLE_MAGIC;
OwnershipState[3]$UDINT := DiagnosticsBootId;
OwnershipState[4]$UDINT := ReportCycle;
OwnershipState[5]$UDINT := LMC_OWNER_STARTUP_PROOF_REQUIRED;
OwnershipState[24] := 0;
Result := 0;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::NotifyAxisOwnershipSessionClosed
if OwnershipState[recordBase + 1] = LMC_OWNER_STATE_RESERVED then
    OwnershipState[recordBase + 1] := LMC_OWNER_STATE_QUARANTINED;
end_if;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ProcessAxisOwnership
if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED = FALSE then
    RETURN;
end_if;
OwnershipObserverState[0] := 0;
END_FUNCTION
FUNCTION GLOBAL LMCControlCommandService::ProcessAxisZeroHome
InputLatch.SubmitAxisZeroHome(OperationToken:=token, AxisReference:=axis, ExpectedActualPosition:=expected);
homeAdmissionToken := ZeroHomeState[34]$UDINT;
homeOwnerGeneration := ZeroHomeState[35]$UDINT;
homeSessionEpoch := ZeroHomeState[36]$UDINT;
homeRequestSequence := ZeroHomeState[37]$UDINT;
homeAxisMask := ZeroHomeState[38]$UDINT;
homeIdentityValid :=
    (ZeroHomeState[51]$UDINT = LMC_HOME_RECORD_MAGIC) &
    (homeAdmissionToken <> 0) &
    (homeOwnerGeneration <> 0) &
    (homeSessionEpoch <> 0) &
    (homeRequestSequence <> 0) &
    (homeAxisMask <> 0);
if ZeroHomeState[51]$UDINT <> LMC_HOME_RECORD_MAGIC then
    RETURN;
else
    homeOwnerResult := ValidateAxisOwnership(
        CommandId:=0x7D13,
        Reference:=ZeroHomeState[9]$UINT,
        ExpectedAxisMask:=homeAxisMask,
        OwnerKind:=LMC_OWNER_KIND_LMC_HOME,
        ResourceKind:=LMC_OWNER_RESOURCE_LMC_HOME_ENGINE,
        AdmissionMode:=LMC_OWNER_ADMISSION_LIFECYCLE,
        CallerSessionEpoch:=homeSessionEpoch,
        RequestSequence:=homeRequestSequence,
        AdmissionToken:=homeAdmissionToken,
        OwnerGeneration:=homeOwnerGeneration,
        RequiredPhase:=LMC_OWNER_PHASE_ACTIVE);
    if homeOwnerResult <> 0 then
        homeCancelRequired := TRUE;
        homeFlags := homeFlags or LMC_HOME_FLAG_CANCEL_REQUIRED;
        ZeroHomeState[39] := LMC_HOME_ENGINE_CANCEL_DRAIN;
    elsif IsClientConnected(#InputLatch) = 0 then
        homeCancelRequired := TRUE;
        homeFlags := homeFlags or LMC_HOME_FLAG_CANCEL_REQUIRED;
        ZeroHomeState[39] := LMC_HOME_ENGINE_CANCEL_DRAIN;
    end_if;
end_if;
homeCancelResult := InputLatch.CancelAxisZeroHome(
    OperationToken:=homeAdmissionToken);
homeCopyResult := InputLatch.CopyAxisZeroHomeResult(
    OperationToken:=homeAdmissionToken,
    pDest:=#homeLatchResult[0],
    DestSize:=128);
homePublishResult := PublishAxisOwnership(
    AxisMask:=homeAxisMask,
    AdmissionToken:=homeAdmissionToken,
    OwnerGeneration:=homeOwnerGeneration);
END_FUNCTION
FUNCTION LMCControlCommandService::HandleAxisZeroHomeCommands
adminHomeOwnerResult := ValidateAxisOwnershipIdentity(
    CommandId:=0x7D13,
    Reference:=Reference,
    ExpectedAxisMask:=adminHomeAxisMask,
    OwnerKind:=LMC_OWNER_KIND_LMC_HOME,
    ResourceKind:=LMC_OWNER_RESOURCE_LMC_HOME_ENGINE,
    AdmissionMode:=LMC_OWNER_ADMISSION_LIFECYCLE,
    CallerSessionEpoch:=OwnershipState[16]$UDINT,
    RequestSequence:=OwnershipState[17]$UDINT,
    AdmissionToken:=OwnershipState[18]$UDINT,
    OwnerGeneration:=OwnershipState[19]$UDINT,
    RequiredPhase:=LMC_OWNER_PHASE_RESERVED,
    pIdentity:=pRequest$^void,
    IdentitySize:=RequestSize);
if adminHomeOwnerResult = -3 then
    detailCode := 42;
elsif adminHomeOwnerResult <> 0 then
    detailCode := 41;
else
    adminHomeSubmitResult := 1;
    if adminHomeSubmitResult = 1 then
    adminHomeOwnerResult := CommitAxisOwnership(
        CommandId:=0x7D13,
        Reference:=Reference,
        ExpectedAxisMask:=adminHomeAxisMask,
        CallerSessionEpoch:=OwnershipState[16]$UDINT,
        RequestSequence:=OwnershipState[17]$UDINT,
        AdmissionToken:=OwnershipState[18]$UDINT,
        OwnerGeneration:=OwnershipState[19]$UDINT);
    ZeroHomeState[47] := adminHomeOwnerResult;
    if adminHomeOwnerResult <> 0 then
        ZeroHomeState[53] := 1;
    end_if;
    end_if;
end_if;
END_FUNCTION
FUNCTION LMCControlCommandService::HandleAdminCommands
case CommandId of
0x7D00:
(pResponseFrame + 24)^$UDINT := 0x00000017;
(pResponseFrame + 44)^$UINT := 5;
0x7D10:
end_case;
END_FUNCTION
'@

$inputLatch = @'
#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE
#define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC  0x4C4D4353
#define LMC_OWNER_STARTUP_LATCH_PHYSICAL  0x00000001
#define LMC_OWNER_STARTUP_LATCH_ZERO_HOME 0x00000002
#define LMC_OWNER_STARTUP_LATCH_DS402     0x00000004
#define LMC_OWNER_STARTUP_LATCH_OWNER     0x00000008
#define LMC_OWNER_STARTUP_LATCH_START_LOW 0x00000010
FUNCTION VIRTUAL GLOBAL LMCEcatInputLatch::RtWork
homeStartupSweepActive := FALSE;
if LMC_DS402_HOME_STARTUP_SWEEP_ENABLED &
   (SnapshotBytes[464]$UDINT = LMC_OWNER_STARTUP_SNAPSHOT_MAGIC) &
   (homeStartupProofComplete = FALSE) &
   (homeDrainHasRequest = FALSE) &
   (homeHasRequest = FALSE) &
   homeStartupOwnerLedgerIdle &
   homeStartupAlignmentIdle then
    homeStartupSweepActive := TRUE;
end_if;
zeroHomeRequestSequence := sigclib_atomic_getU32(
    pValue:=#AxisZeroHomeRequestSequence);
zeroHomeAppliedSequence := sigclib_atomic_getU32(
    pValue:=#AxisZeroHomeAppliedSequence);
homeRequestSequence := sigclib_atomic_getU32(
    pValue:=#Ds402HomeRequestSequence);
homeAppliedSequence := sigclib_atomic_getU32(
    pValue:=#Ds402HomeAppliedSequence);
ownershipLatchDrainFlags := 0;
writeSequence := sigclib_atomic_getU32(pValue:=#PublishSequence) + 1;
sigclib_atomic_setU32(pValue:=#PublishSequence, value:=writeSequence);
if IsClientConnected(#LMCAxis1) then ownershipAxis1Status := LMCAxis1.ReadAxisStatus(); end_if;
if IsClientConnected(#LMCAxis2) then ownershipAxis2Status := LMCAxis2.ReadAxisStatus(); end_if;
if IsClientConnected(#LMCAxis3) then ownershipAxis3Status := LMCAxis3.ReadAxisStatus(); end_if;
if IsClientConnected(#LMCAxis4) then ownershipAxis4Status := LMCAxis4.ReadAxisStatus(); end_if;
if masterAndAxesHealthy then
    ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_PHYSICAL;
end_if;
if zeroHomeRequestSequence = zeroHomeAppliedSequence then
    if (zeroHomeRequestSequence = 0) &
       (AxisZeroHomeResult[0] = 0) &
       (AxisZeroHomeResult[1] = 0) &
       (AxisZeroHomeResult[22] = 0) then
        ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_ZERO_HOME;
    elsif (AxisZeroHomeMailbox[3]$UDINT = zeroHomeRequestSequence) &
          (AxisZeroHomeResult[0]$UDINT = AxisZeroHomeMailbox[0]$UDINT) &
          (AxisZeroHomeResult[3] = AxisZeroHomeMailbox[1]) &
          (AxisZeroHomeResult[4] = AxisZeroHomeMailbox[2]) &
          (AxisZeroHomeResult[22]$UDINT = zeroHomeAppliedSequence) &
          ((AxisZeroHomeResult[1] = LMC_ZERO_HOME_STATE_SUCCEEDED) |
           (AxisZeroHomeResult[1] = LMC_ZERO_HOME_STATE_FAILED)) then
        ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_ZERO_HOME;
    end_if;
end_if;
if homeRequestSequence = homeAppliedSequence then
    ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_DS402;
end_if;
if (Ds402HomeMailbox[0] = 0) & (Ds402HomeMailbox[1] = 0) &
   (Ds402HomeMailbox[10] = 0) & (Ds402HomeMailbox[11] = 0) &
   (Ds402HomeAlignmentState[0] = 0) then
    ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_OWNER;
end_if;
if ((SnapshotBytes[216]$UDINT and 0x00000010) = 0) &
   ((SnapshotBytes[240]$UDINT and 0x00000010) = 0) &
   ((SnapshotBytes[264]$UDINT and 0x00000010) = 0) &
   ((SnapshotBytes[288]$UDINT and 0x00000010) = 0) then
    ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_START_LOW;
end_if;
SnapshotBytes[464]$UDINT := LMC_OWNER_STARTUP_SNAPSHOT_MAGIC;
SnapshotBytes[468]$UDINT := cycleCounter;
SnapshotBytes[472]$UDINT := ownershipAxis1Status$UDINT;
SnapshotBytes[476]$UDINT := ownershipAxis2Status$UDINT;
SnapshotBytes[480]$UDINT := ownershipAxis3Status$UDINT;
SnapshotBytes[484]$UDINT := ownershipAxis4Status$UDINT;
SnapshotBytes[488]$UDINT := SnapshotBytes[84]$UDINT;
SnapshotBytes[492]$UDINT := SnapshotBytes[120]$UDINT;
SnapshotBytes[496]$UDINT := SnapshotBytes[156]$UDINT;
SnapshotBytes[500]$UDINT := SnapshotBytes[192]$UDINT;
SnapshotBytes[504]$UDINT := ownershipLatchDrainFlags;
SnapshotBytes[508]$UDINT := 0;
sigclib_atomic_setU32(pValue:=#PublishSequence, value:=finalSequence);
END_FUNCTION
FUNCTION GLOBAL LMCEcatInputLatch::CopyAxisOwnershipStartupSnapshot
VAR_INPUT
    pDest : ^void;
    DestSize : UDINT;
END_VAR
VAR_OUTPUT
    Result : DINT;
END_VAR
Result := -1;
if (pDest = NIL) | (DestSize < 48) then
    Result := -2;
    RETURN;
end_if;
retryCount := 0;
while retryCount < 3 do
    sequenceBefore := sigclib_atomic_getU32(pValue:=#PublishSequence);
    if (sequenceBefore and 1) = 0 then
        _memcpy(ptr1:=pDest, ptr2:=#SnapshotBytes[464], cntr:=48);
        sequenceAfter := sigclib_atomic_getU32(pValue:=#PublishSequence);
        if (sequenceBefore = sequenceAfter) &
           ((sequenceAfter and 1) = 0) & (sequenceAfter <> 0) then
            Result := 0;
            RETURN;
        end_if;
    end_if;
    retryCount += 1;
end_while;
END_FUNCTION
'@

$diagnostics = @'
#define LMC_DIAG_DS402_HOME_ENABLED FALSE
#define LMC_DIAG_ENCODER_TW20_ENABLED FALSE
#define LMC_DIAG_ENCODER_TW19_ENABLED FALSE
#define LMC_DIAG_D5_SDO_READ_ENABLED FALSE
#define LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED FALSE
#define LMC_OWNER_STARTUP_SNAPSHOT_MAGIC 0x4C4D4353
#define LMC_OWNER_STARTUP_DIAG_SNAPSHOT 0x00000001
#define LMC_OWNER_STARTUP_DIAG_DS402 0x00000002
#define LMC_OWNER_STARTUP_DIAG_ENCODER 0x00000004
#define LMC_OWNER_STARTUP_DIAG_GENERIC 0x00000008
#define LMC_OWNER_STARTUP_DIAG_EXECUTOR 0x00000010
#define LMC_DIAG_OWNER_KIND_ENCODER 5
#define LMC_DIAG_OWNER_KIND_DS402_HOME 4
#define LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO 4
#define LMC_DIAG_RESOURCE_DS402_HOME 3
#define LMC_DIAG_ADMISSION_LIFECYCLE 4
#define LMC_DIAG_OWNER_PHASE_RESERVED 1
#define LMC_DIAG_OWNER_PHASE_ACTIVE 2
AxisOwnership : CltChCmd_LMCControlCommandService;
FUNCTION GLOBAL LMCDiagnosticsService::ProcessOperations
ProcessEncoderMaintenance();
ProcessAxisDs402Home();
ProcessAxisOwnershipStartup();
if (LMC_DIAG_D5_SDO_READ_ENABLED = FALSE) &
   (LMC_DIAG_D5_SDO_WRITE_GLOBAL_ENABLED = FALSE) then
    RETURN;
end_if;
END_FUNCTION
FUNCTION LMCDiagnosticsService::ProcessAxisOwnershipStartup
copyResult := InputLatch.CopyAxisOwnershipStartupSnapshot(
    pDest:=#startupSnapshot[0], DestSize:=48);
diagnosticsDrainFlags := 0;
if (copyResult = 0) &
   (startupSnapshot[0] = LMC_OWNER_STARTUP_SNAPSHOT_MAGIC) &
   (startupSnapshot[1] <> 0) & (startupSnapshot[11] = 0) then
    diagnosticsDrainFlags := diagnosticsDrainFlags or LMC_OWNER_STARTUP_DIAG_SNAPSHOT;
end_if;
if Ds402HomeState[92] = 0 then diagnosticsDrainFlags := diagnosticsDrainFlags or LMC_OWNER_STARTUP_DIAG_DS402; end_if;
if EncoderMaintenanceState[152] = 0 then diagnosticsDrainFlags := diagnosticsDrainFlags or LMC_OWNER_STARTUP_DIAG_ENCODER; end_if;
if (OperationState <> LMC_DIAG_SDO_STATE_QUEUED) &
   (OperationState <> LMC_DIAG_SDO_STATE_RUNNING) &
   (SdoInternalDrainState = 0) then
    diagnosticsDrainFlags := diagnosticsDrainFlags or LMC_OWNER_STARTUP_DIAG_GENERIC;
end_if;
executorsReady := IsClientConnected(#SdoAxis1) <> 0;
executorsReady := executorsReady & SdoAxis1.IsReusable();
executorsReady := executorsReady & (IsClientConnected(#SdoAxis2) <> 0);
executorsReady := executorsReady & SdoAxis2.IsReusable();
executorsReady := executorsReady & (IsClientConnected(#SdoAxis3) <> 0);
executorsReady := executorsReady & SdoAxis3.IsReusable();
executorsReady := executorsReady & (IsClientConnected(#SdoAxis4) <> 0);
executorsReady := executorsReady & SdoAxis4.IsReusable();
if executorsReady then diagnosticsDrainFlags := diagnosticsDrainFlags or LMC_OWNER_STARTUP_DIAG_EXECUTOR; end_if;
reportCycle := ops.tAbsolute;
if reportCycle = 0 then reportCycle := 1; end_if;
startupResult := AxisOwnership.ReconcileAxisOwnershipStartup(
    DiagnosticsBootId:=DiagnosticsBootId,
    ObservationCycle:=observationCycle,
    ReportCycle:=reportCycle,
    DiagnosticsDrainFlags:=diagnosticsDrainFlags);
END_FUNCTION
FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceStart
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
END_FUNCTION
FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceOutcome
END_FUNCTION
FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceRetire
END_FUNCTION
FUNCTION LMCDiagnosticsService::ProcessEncoderMaintenance
ownerResult := AxisOwnership.ValidateAxisOwnership(
    CommandId:=0x7E53,
    Reference:=axisReference,
    ExpectedAxisMask:=axisMask,
    OwnerKind:=LMC_DIAG_OWNER_KIND_ENCODER,
    ResourceKind:=LMC_DIAG_RESOURCE_DIAGNOSTICS_SDO,
    AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
    CallerSessionEpoch:=callerSessionEpoch,
    RequestSequence:=requestSequence,
    AdmissionToken:=admissionToken,
    OwnerGeneration:=ownerGeneration,
    RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
if ownerResult <> 0 then
    quarantine := TRUE;
elsif EncoderMaintenanceState[189]$UDINT <> 0 then
    quarantine := TRUE;
else
    EncoderMaintenanceState[189]$UDINT := encoderProcessOperationToken;
    startResult := SdoAxis1.TryStartWrite();
end_if;
END_FUNCTION
FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart
if malformed then
    detailCode := 9;
elsif (LMC_DIAG_DS402_HOME_ENABLED = FALSE) |
      (CallerSessionEpoch = 0) then
    detailCode := 10;
else
    detailCode := 0;
end_if;
ownerResult := AxisOwnership.ValidateAxisOwnershipIdentity(
    CommandId:=0x7D15,
    Reference:=Reference,
    ExpectedAxisMask:=axisMask,
    OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME,
    ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
    AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
    CallerSessionEpoch:=CallerSessionEpoch,
    RequestSequence:=RequestSequence,
    AdmissionToken:=AdmissionToken,
    OwnerGeneration:=OwnerGeneration,
    RequiredPhase:=LMC_DIAG_OWNER_PHASE_RESERVED,
    pIdentity:=pRequest$^void,
    IdentitySize:=RequestSize);
ownerResult := AxisOwnership.ValidateAxisOwnership(
    CommandId:=0x7D15,
    Reference:=Reference,
    ExpectedAxisMask:=axisMask,
    OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME,
    ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
    AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
    CallerSessionEpoch:=CallerSessionEpoch,
    RequestSequence:=RequestSequence,
    AdmissionToken:=AdmissionToken,
    OwnerGeneration:=OwnerGeneration,
    RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
END_FUNCTION
FUNCTION LMCDiagnosticsService::ProcessAxisDs402Home
if preemptionCleanup & (stage < 90) then
    failure := TRUE;
elsif stage < 90 then
    if LMC_DIAG_DS402_HOME_ENABLED = FALSE then
        failure := TRUE;
        failureNative := 10;
    end_if;
end_if;
if startSdo then
    if IsClientConnected(#AxisOwnership) = FALSE then
        failure := TRUE;
    else
        ownerResult := AxisOwnership.ValidateAxisOwnership(
            CommandId:=0x7D15,
            Reference:=axisReference,
            ExpectedAxisMask:=axisMask,
            OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME,
            ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
            AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
            CallerSessionEpoch:=ds402ProcessOwnerSessionEpoch,
            RequestSequence:=ownerRequestSequence,
            AdmissionToken:=admissionToken,
            OwnerGeneration:=ownerGeneration,
            RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
        if ownerResult <> 0 then
            failure := TRUE;
        elsif Ds402HomeState[126] = 0 then
            ownerResult := AxisOwnership.PublishAxisOwnership();
        end_if;
    end_if;
end_if;
if startSdo & (failure = FALSE) then
    Ds402HomeState[99] := sdoToken$DINT;
    startResult := SdoAxis1.TryStartRead(sdoToken, 0x6061);
end_if;
case stage of
    94:
        if executorReusable then
            ownerResult := -3;
            if IsClientConnected(#AxisOwnership) then
                ownerResult := AxisOwnership.ValidateAxisOwnership(
                    CommandId:=0x7D15,
                    Reference:=axisReference,
                    ExpectedAxisMask:=axisMask,
                    OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME,
                    ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
                    AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
                    CallerSessionEpoch:=ds402ProcessOwnerSessionEpoch,
                    RequestSequence:=ownerRequestSequence,
                    AdmissionToken:=admissionToken,
                    OwnerGeneration:=ownerGeneration,
                    RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
            end_if;
            if ownerResult <> 0 then
                Ds402HomeState[108] := TO_DINT(0 - ownerResult);
                Ds402HomeState[92] := 98;
            else
                Ds402HomeState[99] := sdoToken$DINT;
                startResult := SdoAxis1.TryStartWrite(sdoToken, 0x6060);
            end_if;
        end_if;
    95:
        completionResult := 0;
    96:
        if executorReusable then
            ownerResult := -3;
            if IsClientConnected(#AxisOwnership) then
                ownerResult := AxisOwnership.ValidateAxisOwnership(
                    CommandId:=0x7D15,
                    Reference:=axisReference,
                    ExpectedAxisMask:=axisMask,
                    OwnerKind:=LMC_DIAG_OWNER_KIND_DS402_HOME,
                    ResourceKind:=LMC_DIAG_RESOURCE_DS402_HOME,
                    AdmissionMode:=LMC_DIAG_ADMISSION_LIFECYCLE,
                    CallerSessionEpoch:=ds402ProcessOwnerSessionEpoch,
                    RequestSequence:=ownerRequestSequence,
                    AdmissionToken:=admissionToken,
                    OwnerGeneration:=ownerGeneration,
                    RequiredPhase:=LMC_DIAG_OWNER_PHASE_ACTIVE);
            end_if;
            if ownerResult <> 0 then
                Ds402HomeState[108] := TO_DINT(0 - ownerResult);
                Ds402HomeState[92] := 98;
            else
                Ds402HomeState[99] := sdoToken$DINT;
                startResult := SdoAxis1.TryStartRead(sdoToken, 0x6061);
            end_if;
        end_if;
    97:
        completionResult := 0;
end_case;
END_FUNCTION
FUNCTION LMCDiagnosticsService::Capabilities
if LMC_DIAG_ENCODER_TW20_ENABLED = TRUE then bits := bits or 0x00040000; end_if;
if LMC_DIAG_ENCODER_TW19_ENABLED = TRUE then bits := bits or 0x00080000; end_if;
END_FUNCTION
FUNCTION LMCDiagnosticsService::GetSdoWritePolicyDetail
DetailCode := 0;
END_FUNCTION
'@

$tcp = @'
#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE
#define LMC_OWNER_PROFILE_AXIS_MASK 0x0000000F
#define LMC_OWNER_ROBOT_AXIS_MASK 0x000001FF
#define LMC_OWNER_KIND_DIRECT 1
#define LMC_OWNER_KIND_GROUP 2
#define LMC_OWNER_RESOURCE_AXIS 1
#define LMC_OWNER_ADMISSION_ORDINARY 1
#define LMC_OWNER_ADMISSION_SAFETY 2
#define LMC_OWNER_ADMISSION_LIFECYCLE 4
#define LMC_OWNER_ADAPTER_ERROR_CONFLICT -9
FUNCTION VIRTUAL GLOBAL TCPMotionInterface::CyWork
ControlCommands.ProcessAxisZeroHome();
ControlCommands.ProcessAxisOwnership();
Diagnostics.ProcessOperations();
END_FUNCTION
FUNCTION TCPMotionInterface::MsgPaser
if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED & (CommandID <> 0x7D13) then
    case CommandID of
        0x2023:
            controlClassifierValid :=
                (Payload = 8) & (AxisRef >= 1) & (AxisRef <= 9);
            controlOwnerKind := LMC_OWNER_KIND_DIRECT;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            if RequestBuf[12] = 1 then
                controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
            else
                controlAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;
            end_if;
        0x2024:
            controlClassifierValid :=
                (Payload = 1) & (AxisRef >= 1) & (AxisRef <= 9);
            controlOwnerKind := LMC_OWNER_KIND_DIRECT;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x2022:
            controlClassifierValid :=
                (Payload = 16) & (AxisRef >= 1) & (AxisRef <= 9);
            controlOwnerKind := LMC_OWNER_KIND_DIRECT;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;
        0x209F, 0x20A0:
            controlClassifierValid :=
                (Payload = 32) & (AxisRef >= 1) & (AxisRef <= 9);
            controlOwnerKind := LMC_OWNER_KIND_DIRECT;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x20A2:
            controlClassifierValid :=
                (Payload = 24) & (AxisRef >= 1) & (AxisRef <= 9);
            controlOwnerKind := LMC_OWNER_KIND_DIRECT;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x2047:
            controlClassifierValid :=
                (Payload = 1) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_PROFILE_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x2048:
            controlClassifierValid :=
                (Payload = 1) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_PROFILE_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;
        0x2049, 0x204A:
            controlClassifierValid :=
                (Payload = 1) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_ROBOT_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x204B:
            controlClassifierValid :=
                (Payload = 1) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_ROBOT_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;
        0x2085:
            controlClassifierValid :=
                (Payload = 16) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_PROFILE_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;
        0x20A4:
            controlClassifierValid :=
                (Payload = 96) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_PROFILE_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x7D22:
            controlClassifierValid :=
                (Payload = 104) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_PROFILE_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;
        0x20E7:
            controlClassifierValid :=
                (Payload = 1320) & (AxisRef = 0x0100);
            controlAxisMask := LMC_OWNER_ROBOT_AXIS_MASK;
            controlOwnerKind := LMC_OWNER_KIND_GROUP;
            controlResourceKind := LMC_OWNER_RESOURCE_AXIS;
            controlAdmissionMode := LMC_OWNER_ADMISSION_LIFECYCLE;
    else
    end_case;
    controlManagedCommand := controlOwnerKind <> 0;
    if controlClassifierValid & (controlOwnerKind = 1) then
        case AxisRef of
            1: controlAxisMask := 0x00000001;
            2: controlAxisMask := 0x00000002;
            3: controlAxisMask := 0x00000004;
            4: controlAxisMask := 0x00000008;
            5: controlAxisMask := 0x00000010;
            6: controlAxisMask := 0x00000020;
            7: controlAxisMask := 0x00000040;
            8: controlAxisMask := 0x00000080;
            9: controlAxisMask := 0x00000100;
        else
        end_case;
    end_if;
    if controlManagedCommand & (controlClassifierValid = FALSE) then
        controlInvokeService := FALSE;
    end_if;
    if controlClassifierValid & (controlAxisMask <> 0) then
        controlAdmissionResult := ControlCommands.ReserveAxisOwnership(
            CommandId:=CommandID$UINT,
            Reference:=AxisRef$UINT,
            RequestedAxisMask:=controlAxisMask,
            OwnerKind:=controlOwnerKind,
            ResourceKind:=controlResourceKind,
            AdmissionMode:=controlAdmissionMode,
            CallerSessionEpoch:=ActiveRequest.SessionEpoch,
            RequestSequence:=ActiveRequest.Sequence,
            pIdentity:=(#RequestBuf[8])$^void,
            IdentitySize:=Payload$UDINT,
            pEffectiveAxisMask:=#controlEffectiveAxisMask,
            pAdmissionToken:=#controlAdmissionToken,
            pOwnerGeneration:=#controlOwnerGeneration);
        if controlAdmissionResult < 0 then
            if CommandID = 0x20E7 then
                Sendbuf[10]$INT := -9;
            else
                Sendbuf[14]$INT := -9;
            end_if;
            controlInvokeService := FALSE;
        end_if;
    end_if;
end_if;
if controlInvokeService & IsClientConnected(#ControlCommands) then
    controlResponseSize := ControlCommands.HandleRequest(
        CommandId:=CommandID$UINT,
        Reference:=AxisRef$UINT,
        pRequestFrame:=(#RequestBuf[0])$^USINT,
        RequestFrameSize:=(Payload + 8)$UDINT,
        pResponseFrame:=(#Sendbuf[0])$^USINT,
        ResponseCapacity:=sizeof(Sendbuf),
        CallerSessionEpoch:=ActiveRequest.SessionEpoch,
        RequestSequence:=ActiveRequest.Sequence,
        AdmissionToken:=controlAdmissionToken,
        OwnerGeneration:=controlOwnerGeneration);
end_if;
ControlCommands.ReserveAxisOwnership(
    CallerSessionEpoch:=ActiveRequest.SessionEpoch,
    RequestSequence:=ActiveRequest.Sequence,
    pIdentity:=(#RequestBuf[8])$^void,
    IdentitySize:=Payload$UDINT);
controlExactAccepted := Sendbuf[16]$UDINT = RequestBuf[12]$UDINT;
controlExactFailure := TRUE;
if controlExactFailure then
    controlRollbackResult := ControlCommands.RollbackAxisOwnership(
        AdmissionToken:=controlAdmissionToken,
        OwnerGeneration:=controlOwnerGeneration,
        CallerSessionEpoch:=ActiveRequest.SessionEpoch,
        RequestSequence:=ActiveRequest.Sequence,
        Reason:=0);
    if controlRollbackResult <> 0 then
        controlPublishResult := ControlCommands.PublishAxisOwnership(
            AxisMask:=controlEffectiveAxisMask,
            AdmissionToken:=controlAdmissionToken,
            OwnerGeneration:=controlOwnerGeneration);
    end_if;
elsif controlExactAccepted = FALSE then
    controlRollbackResult := ControlCommands.RollbackAxisOwnership(
        AdmissionToken:=controlAdmissionToken,
        OwnerGeneration:=controlOwnerGeneration,
        CallerSessionEpoch:=ActiveRequest.SessionEpoch,
        RequestSequence:=ActiveRequest.Sequence,
        Reason:=-21);
    if controlRollbackResult <> 0 then
        controlPublishResult := ControlCommands.PublishAxisOwnership(
            AxisMask:=controlEffectiveAxisMask,
            AdmissionToken:=controlAdmissionToken,
            OwnerGeneration:=controlOwnerGeneration);
    end_if;
end_if;
END_FUNCTION
'@

$errorCatalog = @'
public static class LMCErrorCatalog
{
    public const uint CurrentCatalogVersion = 2;
    public const string AdapterSourceVersion =
        "Elmo_Master TCPMotionInterface local errors v2";

    private static Dictionary<long, LMCErrorDescription>
        CreateAdapterEntries()
    {
        var entries = new Dictionary<long, LMCErrorDescription>();
        Add(entries, LMCErrorDomain.AdapterCommand, -1,
            "RpcSessionStateInvalid", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -2,
            "ObjectOrClientUnavailable", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -3,
            "MalformedRequest", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -4,
            "UnknownCommand", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -5,
            "PayloadTooLarge", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -6,
            "NativeErrorNotRepresentable", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -7,
            "UnsupportedArgumentCombination", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -8,
            "QueueOrFramingError", "message", "action", AdapterSourceVersion);
        Add(entries, LMCErrorDomain.AdapterCommand, -9,
            "AxisOwnershipConflict", "message", "action", AdapterSourceVersion);
        return entries;
    }

    private static Dictionary<long, LMCErrorDescription>
        CreateAdminEntries()
    {
        return new Dictionary<long, LMCErrorDescription>();
    }
}
'@

$adminModels = @'
[Flags]
public enum LMCAdminFeature : uint
{
    None = 0,
    AxisDs402Home = 1u << 6
}
'@

function New-OwnershipFixture {
    param(
        [string]$Name,
        [string]$Control = $control,
        [string]$Diagnostics = $diagnostics,
        [string]$InputLatch = $inputLatch,
        [string]$Tcp = $tcp,
        [string]$ErrorCatalog = $errorCatalog,
        [string]$AdminModels = $adminModels,
        [string]$ExpectedMessage = ''
    )

    [pscustomobject]@{
        Name = $Name
        Control = $Control
        Diagnostics = $Diagnostics
        InputLatch = $InputLatch
        Tcp = $Tcp
        ErrorCatalog = $ErrorCatalog
        AdminModels = $AdminModels
        ExpectedMessage = $ExpectedMessage
    }
}

function Replace-OwnershipFixtureFirst {
    param(
        [string]$Text,
        [string]$Pattern,
        [string]$Replacement,
        [string]$Name
    )

    $regex = [regex]::new(
        $Pattern,
        [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
        [Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $regex.IsMatch($Text)) {
        throw "Ownership phase fixture mutation '$Name' did not match."
    }
    return $regex.Replace($Text, $Replacement, 1)
}

function Move-OwnershipFixtureStatementBefore {
    param(
        [string]$Text,
        [string]$RegionPattern,
        [string]$StatementPattern,
        [string]$BeforePattern,
        [string]$Name
    )

    $options = [Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
        [Text.RegularExpressions.RegexOptions]::Singleline -bor
        [Text.RegularExpressions.RegexOptions]::Multiline
    $regionMatch = [regex]::Match($Text, $RegionPattern, $options)
    if (-not $regionMatch.Success) {
        throw "Ownership phase fixture move '$Name' region did not match."
    }
    $statementMatch = [regex]::Match(
        $regionMatch.Value,
        $StatementPattern,
        $options)
    $beforeMatch = [regex]::Match(
        $regionMatch.Value,
        $BeforePattern,
        $options)
    if (-not $statementMatch.Success -or -not $beforeMatch.Success -or
        $statementMatch.Index -le $beforeMatch.Index) {
        throw "Ownership phase fixture move '$Name' ordering did not match."
    }

    $statement = $statementMatch.Value.TrimEnd("`r", "`n")
    $regionWithoutStatement = $regionMatch.Value.Remove(
        $statementMatch.Index,
        $statementMatch.Length)
    $beforeAfterRemoval = [regex]::Match(
        $regionWithoutStatement,
        $BeforePattern,
        $options)
    if (-not $beforeAfterRemoval.Success) {
        throw "Ownership phase fixture move '$Name' target disappeared."
    }
    $movedRegion = $regionWithoutStatement.Insert(
        $beforeAfterRemoval.Index,
        $statement + [Environment]::NewLine)
    return $Text.Remove($regionMatch.Index, $regionMatch.Length).Insert(
        $regionMatch.Index,
        $movedRegion)
}

$safetyRepeatRepositoryRoot = if (
    -not [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
}
else {
    (Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '..\..\..\..')).Path
}
$safetyRepeatControlPath = Join-Path $safetyRepeatRepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'LMCControlCommandService\LMCControlCommandService.st')
$safetyRepeatTcpPath = Join-Path $safetyRepeatRepositoryRoot (
    'Lasal_PRG\Elmo_EtherCAT_Test_4Axis\Class\' +
    'TCPMotionInterface\TCPMotionInterface.st')
$safetyRepeatControl = Get-Content -Raw -LiteralPath $safetyRepeatControlPath
$safetyRepeatTcp = Get-Content -Raw -LiteralPath $safetyRepeatTcpPath

function New-SafetyRepeatFixture {
    param(
        [string]$Name,
        [string]$Control = $safetyRepeatControl,
        [string]$Tcp = $safetyRepeatTcp,
        [string]$ExpectedMessage = ''
    )

    if (($Control -ceq $safetyRepeatControl) -and
        ($Tcp -ceq $safetyRepeatTcp)) {
        throw "Safety-repeat fixture mutation '$Name' did not change the source."
    }
    [pscustomobject]@{
        Name = $Name
        Control = $Control
        Tcp = $Tcp
        ExpectedMessage = $ExpectedMessage
    }
}

$repeatHelperGlobal = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat\b') `
    -Replacement ('FUNCTION GLOBAL LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat') `
    -Name 'RepeatHelperGlobal'
$repeatHelperCommitAdded = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat\b)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        'CommitAxisOwnership();') `
    -Name 'RepeatHelperCommitAdded'
$repeatCoalescedNativeAdded = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat.*?' +
        'if\s+repeatValid\s*&\s*repeatCoalesced\s+then)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        'nativeResponseSize := HandleAxisCommands();') `
    -Name 'RepeatCoalescedNativeAdded'
$repeatMarkerExactWeakened = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat.*?' +
        'markerExact\s*:=\s*\(OwnershipState\[25\]\s*=\s*)1') `
    -Replacement '${1}0' `
    -Name 'RepeatMarkerExactWeakened'
$repeatEvidenceResetDropped = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat.*?' +
        'OwnershipObserverState\[observerBase\s*\+\s*)8' +
        '(\]\s*:=\s*0\s*;)') `
    -Replacement '${1}9${2}' `
    -Name 'RepeatEvidenceResetDropped'
$repeatMarkerResetDropped = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat.*?' +
        'OwnershipState\[27\]\s*:=\s*)0(\s*;)') `
    -Replacement '${1}1${2}' `
    -Name 'RepeatMarkerResetDropped'
$repeatReserveOutputChanged = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ReserveAxisOwnership.*?' +
        'pAdmissionToken\^\s*:=\s*)repeatAdmissionToken') `
    -Replacement '${1}nextToken' `
    -Name 'RepeatReserveOutputChanged'
$repeatReserveReturnRemoved = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ReserveAxisOwnership.*?Result\s*:=\s*repeatMode\s*;)\s*RETURN\s*;') `
    -Replacement '${1}' `
    -Name 'RepeatReserveReturnRemoved'
$repeatReserveMutationAdded = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ReserveAxisOwnership.*?if\s+repeatMode\s*>\s*0\s+then)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        ('OwnershipObserverState[0]$UDINT := ' +
         'LMC_OWNER_OBSERVER_POWER_OFF_ESCALATED;')) `
    -Name 'RepeatReserveMutationAdded'
$repeatReserveRootCopyBypassed = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ReserveAxisOwnership.*?if\s+repeatRootPresent\s+then.*?' +
        'repeatPreemptCopyResult\s*:=\s*CopyAxisOwnershipPreemption\(.*?' +
        'DestSize:=)144') `
    -Replacement '${1}140' `
    -Name 'RepeatReserveRootCopyBypassed'
$repeatProcessAxisWeakened = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ProcessAxisOwnership.*?0x2022\s*:.*?' +
        'terminalCandidate\s*:=\s*)referencePowerOff') `
    -Replacement '${1}allStandstill' `
    -Name 'RepeatProcessAxisWeakened'
$repeatProcessGroupDisableWeakened = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ProcessAxisOwnership.*?0x2048\s*:.*?' +
        'terminalCandidate\s*:=\s*)' +
        '\(groupPowerState\s*=\s*0\)\s*&\s*allPowerOff') `
    -Replacement '${1}allStandstill' `
    -Name 'RepeatProcessGroupDisableWeakened'
$repeatProcessGroupStopWeakened = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ProcessAxisOwnership.*?0x2085\s*:.*?' +
        'terminalCandidate\s*:=\s*)' +
        '\(groupPowerState\s*=\s*0\)\s*&\s*allPowerOff') `
    -Replacement '${1}allStandstill' `
    -Name 'RepeatProcessGroupStopWeakened'
$repeatProcessCommandScopeExpanded = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'ProcessAxisOwnership.*?powerOffEscalated\s*&.*?' +
        '\(commandId\s*<>\s*)LMC_OWNER_COMMAND_GROUP_STOP') `
    -Replacement '${1}LMC_OWNER_COMMAND_GROUP_POWER_OFF' `
    -Name 'RepeatProcessCommandScopeExpanded'
$repeatTcpPositiveRejected = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatTcp `
    -Pattern ('(//\s*LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN.*?' +
        'if\s+controlAdmissionResult\s*)<\s*0') `
    -Replacement '${1}<> 0' `
    -Name 'RepeatTcpPositiveRejected'
$repeatTcpOrdinaryOwnsHome = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatTcp `
    -Pattern ('(//\s*LMC_OWNER_ORDINARY_CLASSIFIER_BEGIN\s*)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        'controlReserved := TRUE;') `
    -Name 'RepeatTcpOrdinaryOwnsHome'
$repeatTcpHomeReservationDropped = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatTcp `
    -Pattern ('(if\s+controlAdmissionResult\s*=\s*0\s+then\s*' +
        'controlReserved\s*:=\s*)TRUE') `
    -Replacement '${1}FALSE' `
    -Name 'RepeatTcpHomeReservationDropped'
$repeatHelperOversized = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+LMCControlCommandService::' +
        'HandleAxisOwnershipSafetyRepeat\b)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        ('X' * 40000)) `
    -Name 'RepeatHelperOversized'
$repeatHandleOversized = Replace-OwnershipFixtureFirst `
    -Text $safetyRepeatControl `
    -Pattern ('(FUNCTION\s+GLOBAL\s+LMCControlCommandService::' +
        'HandleRequest\b)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        ('X' * 40000)) `
    -Name 'RepeatHandleOversized'

$safetyRepeatNegativeFixtures = @(
    New-SafetyRepeatFixture -Name 'FreshPowerOnRejectedAsSafety' -Control (
        $safetyRepeatControl.Replace(
            'AdmissionMode:=firstDispatchAdmissionMode,',
            'AdmissionMode:=LMC_OWNER_ADMISSION_SAFETY,'))
    New-SafetyRepeatFixture -Name 'FreshPowerOnShapeNotRouted' -Control (
        $safetyRepeatControl.Replace(
            'repeatValid := firstDispatchShapeValid &',
            'repeatValid := repeatShapeValid &'))
    New-SafetyRepeatFixture -Name 'FreshPowerOnWrongMode' -Control (
        $safetyRepeatControl.Replace(
            'firstDispatchAdmissionMode := LMC_OWNER_ADMISSION_ORDINARY;',
            'firstDispatchAdmissionMode := LMC_OWNER_ADMISSION_SAFETY;'))
    New-SafetyRepeatFixture -Name 'PowerOnSafetyCoalescingAllowed' -Control (
        $safetyRepeatControl.Replace(
            'repeatValid := repeatValid & repeatShapeValid;',
            'repeatValid := repeatValid;'))
    New-SafetyRepeatFixture -Name 'RepeatSentinelDrift' -Control (
        $safetyRepeatControl.Replace(
            '#define LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE -11',
            '#define LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE -12'))
    New-SafetyRepeatFixture -Name 'RepeatObserverEscalationBitDrift' -Control (
        $safetyRepeatControl.Replace(
            '#define LMC_OWNER_OBSERVER_POWER_OFF_ESCALATED 0x00000100',
            '#define LMC_OWNER_OBSERVER_POWER_OFF_ESCALATED 0x00000200'))
    New-SafetyRepeatFixture -Name 'RepeatObserverKnownMaskDrift' -Control (
        $safetyRepeatControl.Replace(
            '#define LMC_OWNER_OBSERVER_KNOWN_MASK 0x000001FF',
            '#define LMC_OWNER_OBSERVER_KNOWN_MASK 0x000000FF'))
    New-SafetyRepeatFixture -Name 'RepeatObserverKnownMaskUseDropped' -Control (
        $safetyRepeatControl.Replace(
            '(0xFFFFFFFF xor LMC_OWNER_OBSERVER_KNOWN_MASK)',
            '0xFFFFFF00'))
    New-SafetyRepeatFixture -Name 'RepeatRootMaskDrift' -Control (
        ([regex]::new(
            '(?s)(FUNCTION LMCControlCommandService::' +
            'HandleAxisOwnershipSafetyRepeat.*?)0xFFFE0000')).Replace(
                $safetyRepeatControl,
                '${1}0xFFFC0000',
                1))
    New-SafetyRepeatFixture -Name 'RepeatOneByteIdentityMaskDrift' -Control (
        $safetyRepeatControl.Replace('0xFFFFFF00', '0xFFFFFE00'))
    New-SafetyRepeatFixture -Name 'RepeatHelperGlobal' -Control $repeatHelperGlobal
    New-SafetyRepeatFixture -Name 'RepeatHelperSentinelRemoved' -Control (
        $safetyRepeatControl.Replace(
            'Result := LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE;',
            'Result := -1;'))
    New-SafetyRepeatFixture -Name 'RepeatHandleEarlyReturnInverted' -Control (
        $safetyRepeatControl.Replace(
            ('if ResponseSize <> ' +
             'LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE then'),
            ('if ResponseSize = ' +
             'LMC_OWNER_SAFETY_REPEAT_NOT_APPLICABLE then')))
    New-SafetyRepeatFixture -Name 'RepeatHelperCommitAdded' -Control $repeatHelperCommitAdded
    New-SafetyRepeatFixture -Name 'RepeatCoalescedNativeAdded' -Control $repeatCoalescedNativeAdded
    New-SafetyRepeatFixture -Name 'RepeatMarkerExactWeakened' -Control $repeatMarkerExactWeakened
    New-SafetyRepeatFixture -Name 'RepeatEvidenceResetDropped' -Control $repeatEvidenceResetDropped
    New-SafetyRepeatFixture -Name 'RepeatMarkerResetDropped' -Control $repeatMarkerResetDropped
    New-SafetyRepeatFixture -Name 'RepeatReserveOutputChanged' -Control $repeatReserveOutputChanged
    New-SafetyRepeatFixture -Name 'RepeatReserveReturnRemoved' -Control $repeatReserveReturnRemoved
    New-SafetyRepeatFixture -Name 'RepeatReserveMutationAdded' -Control $repeatReserveMutationAdded
    New-SafetyRepeatFixture -Name 'RepeatReserveRootCopyBypassed' -Control $repeatReserveRootCopyBypassed
    New-SafetyRepeatFixture -Name 'RepeatProcessAxisWeakened' -Control $repeatProcessAxisWeakened
    New-SafetyRepeatFixture -Name 'RepeatProcessGroupDisableWeakened' -Control $repeatProcessGroupDisableWeakened
    New-SafetyRepeatFixture -Name 'RepeatProcessGroupStopWeakened' -Control $repeatProcessGroupStopWeakened
    New-SafetyRepeatFixture -Name 'RepeatProcessCommandScopeExpanded' -Control $repeatProcessCommandScopeExpanded
    New-SafetyRepeatFixture -Name 'RepeatTcpPositiveRejected' -Tcp $repeatTcpPositiveRejected
    New-SafetyRepeatFixture -Name 'RepeatTcpOrdinaryOwnsHome' -Tcp $repeatTcpOrdinaryOwnsHome
    New-SafetyRepeatFixture -Name 'RepeatTcpHomeReservationDropped' -Tcp $repeatTcpHomeReservationDropped
    New-SafetyRepeatFixture -Name 'RepeatHelperOversized' -Control $repeatHelperOversized
    New-SafetyRepeatFixture -Name 'RepeatHandleOversized' -Control $repeatHandleOversized)

$activeControl = $control.
    Replace(
        '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
        '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE').
    Replace(
        '(pResponseFrame + 24)^$UDINT := 0x00000017;',
        '(pResponseFrame + 24)^$UDINT := 0x00000057;')
$activeDiagnostics = $diagnostics.Replace(
    '#define LMC_DIAG_DS402_HOME_ENABLED FALSE',
    '#define LMC_DIAG_DS402_HOME_ENABLED TRUE')
$activeInputLatch = $inputLatch.Replace(
    '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE',
    '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED TRUE')
$activeTcp = $tcp.Replace(
    '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
    '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE')

$controlReserveGateRemoved = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::' +
        'ReserveAxisOwnership.*?if\s+)' +
        'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED\s*=\s*FALSE') `
    -Replacement '${1}FALSE' `
    -Name 'ControlReserveGateRemoved'
$controlProcessGateRemoved = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::' +
        'ProcessAxisOwnership.*?if\s+)' +
        'LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED\s*=\s*FALSE') `
    -Replacement '${1}FALSE' `
    -Name 'ControlProcessGateRemoved'
$diagnosticsStartGateRemoved = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern ('(FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart' +
        '.*?elsif\s*\()LMC_DIAG_DS402_HOME_ENABLED') `
    -Replacement '${1}FALSE' `
    -Name 'DiagnosticsStartGateRemoved'
$diagnosticsProcessGateRemoved = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern ('(FUNCTION LMCDiagnosticsService::ProcessAxisDs402Home' +
        '.*?elsif\s+stage\s*<\s*90\s+then\s*if\s+)' +
        'LMC_DIAG_DS402_HOME_ENABLED') `
    -Replacement '${1}FALSE' `
    -Name 'DiagnosticsProcessGateRemoved'

$reserveHomeWrongResourceControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ReserveAxisOwnership.*?' +
        'elsif\s+ResourceKind\s*=\s*)LMC_OWNER_RESOURCE_LMC_HOME_ENGINE') `
    -Replacement '${1}LMC_OWNER_RESOURCE_DS402_HOME_ENGINE' `
    -Name 'ReserveHomeWrongResource'
$reserveEncoderWrongCommandControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ReserveAxisOwnership.*?' +
        'elsif\s+ResourceKind\s*=\s*LMC_OWNER_RESOURCE_DIAGNOSTICS_SDO_ENGINE' +
        '.*?CommandId\s*<>\s*)0x7E53') `
    -Replacement '${1}0x7E52' `
    -Name 'ReserveEncoderWrongCommand'
$homeTupleWrongResourceControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x7D13\s*:.*?ResourceKind\s*=\s*)' +
        'LMC_OWNER_RESOURCE_LMC_HOME_ENGINE') `
    -Replacement '${1}LMC_OWNER_RESOURCE_DS402_HOME_ENGINE' `
    -Name 'HomeTupleWrongResource'
$ds402TupleReadAdmissionControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x7D15\s*:.*?AdmissionMode\s*=\s*)' +
        'LMC_OWNER_ADMISSION_LIFECYCLE') `
    -Replacement '${1}LMC_OWNER_ADMISSION_READ' `
    -Name 'Ds402TupleReadAdmission'
$encoderTupleWrongCommandControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        ')0x7E53(\s*:)') `
    -Replacement '${1}0x7E52${2}' `
    -Name 'EncoderTupleWrongCommand'
$encoderTupleWrongMaskControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x7E53\s*:.*?ExpectedAxisMask\s*)=\s*referenceAxisMask') `
    -Replacement '${1}<= LMC_OWNER_ROBOT_AXIS_MASK' `
    -Name 'EncoderTupleWrongMask'
$directStopExactMaskControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x2022\s*:.*?ExpectedAxisMask\s*)<=\s*LMC_OWNER_ROBOT_AXIS_MASK') `
    -Replacement '${1}= referenceAxisMask' `
    -Name 'DirectStopExactMask'
$directResetSafetyExactMaskControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x2023\s*:.*?AdmissionMode\s*=\s*LMC_OWNER_ADMISSION_SAFETY.*?' +
        'ExpectedAxisMask\s*)<=\s*LMC_OWNER_ROBOT_AXIS_MASK') `
    -Replacement '${1}= referenceAxisMask' `
    -Name 'DirectResetSafetyExactMask'
$groupDisableRobotMaskControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x2048\s*:.*?ExpectedAxisMask\s*=\s*)LMC_OWNER_PROFILE_AXIS_MASK') `
    -Replacement '${1}LMC_OWNER_ROBOT_AXIS_MASK' `
    -Name 'GroupDisableRobotMask'
$groupStopProfileOnlyControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership.*?' +
        '0x2085\s*:.*?\(ExpectedAxisMask\s*=\s*LMC_OWNER_PROFILE_AXIS_MASK\)\s*\|\s*' +
        '\(ExpectedAxisMask\s*=\s*)LMC_OWNER_ROBOT_AXIS_MASK') `
    -Replacement '${1}LMC_OWNER_PROFILE_AXIS_MASK' `
    -Name 'GroupStopProfileOnly'
$tupleValidBypassControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership' +
        '.*?tupleValid\s*:=\s*FALSE\s*;\s*case\s+CommandId\s+of' +
        '.*?end_case\s*;)') `
    -Replacement ('${1}' + [Environment]::NewLine + 'tupleValid := TRUE;') `
    -Name 'TupleValidBypassAfterClassifier'
$expectedStateBypassControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern ('(FUNCTION GLOBAL LMCControlCommandService::ValidateAxisOwnership' +
        '.*?LMC_OWNER_KIND_ENCODER\s*:\s*expectedState\s*:=\s*' +
        'LMC_OWNER_STATE_TW20_QUEUED\s*;.*?end_case\s*;\s*' +
        'end_if\s*;\s*end_if\s*;)') `
    -Replacement ('${1}' + [Environment]::NewLine +
        'expectedState := LMC_OWNER_STATE_RESERVED;') `
    -Name 'ExpectedStateBypassAfterActiveSelection'

$handleRequestActiveControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern '(FUNCTION GLOBAL LMCControlCommandService::HandleRequest.*?RequiredPhase:=)LMC_OWNER_PHASE_RESERVED' `
    -Replacement '${1}LMC_OWNER_PHASE_ACTIVE' `
    -Name 'HandleRequestUsesActive'
$commitActiveControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern '(FUNCTION GLOBAL LMCControlCommandService::CommitAxisOwnership.*?RequiredPhase:=)LMC_OWNER_PHASE_RESERVED' `
    -Replacement '${1}LMC_OWNER_PHASE_ACTIVE' `
    -Name 'CommitUsesActive'
$homeStartActiveControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern '(FUNCTION LMCControlCommandService::HandleAxisZeroHomeCommands.*?RequiredPhase:=)LMC_OWNER_PHASE_RESERVED' `
    -Replacement '${1}LMC_OWNER_PHASE_ACTIVE' `
    -Name 'HomeStartUsesActive'
$homeProcessReservedControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern '(FUNCTION GLOBAL LMCControlCommandService::ProcessAxisZeroHome.*?RequiredPhase:=)LMC_OWNER_PHASE_ACTIVE' `
    -Replacement '${1}LMC_OWNER_PHASE_RESERVED' `
    -Name 'HomeProcessUsesReserved'
$homeCopyBeforeActiveControl = Move-OwnershipFixtureStatementBefore `
    -Text $control `
    -RegionPattern ('FUNCTION GLOBAL LMCControlCommandService::' +
        'ProcessAxisZeroHome.*?END_FUNCTION') `
    -StatementPattern ('[ \t]*homeCopyResult\s*:=\s*' +
        'InputLatch\.CopyAxisZeroHomeResult\(.*?DestSize:=128\);\r?\n?') `
    -BeforePattern '[ \t]*homeOwnerResult\s*:=\s*ValidateAxisOwnership\(' `
    -Name 'HomeCopyBeforeActive'
$homeFailureNotQuarantinedControl = Replace-OwnershipFixtureFirst `
    -Text $control `
    -Pattern '(if\s+homeOwnerResult\s*<>\s*0\s+then\s*)homeCancelRequired\s*:=\s*TRUE' `
    -Replacement '${1}homeCancelRequired := FALSE' `
    -Name 'HomeFailureNotQuarantined'

$encoderStartActiveDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(FUNCTION LMCDiagnosticsService::HandleEncoderMaintenanceStart.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Name 'EncoderStartUsesActive'
$encoderProcessReservedDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(FUNCTION LMCDiagnosticsService::ProcessEncoderMaintenance.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Name 'EncoderProcessUsesReserved'
$encoderClaimBeforeActiveDiagnostics = Move-OwnershipFixtureStatementBefore `
    -Text $diagnostics `
    -RegionPattern ('FUNCTION LMCDiagnosticsService::' +
        'ProcessEncoderMaintenance.*?END_FUNCTION') `
    -StatementPattern ('[ \t]*EncoderMaintenanceState\[189\]\$UDINT\s*:=\s*' +
        'encoderProcessOperationToken\s*;\r?\n?') `
    -BeforePattern '[ \t]*ownerResult\s*:=\s*AxisOwnership\.ValidateAxisOwnership\(' `
    -Name 'EncoderClaimBeforeActive'
$ds402StartActiveDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(FUNCTION LMCDiagnosticsService::HandleAxisDs402HomeStart.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Name 'Ds402StartUsesActive'
$ds402MainReservedDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(FUNCTION LMCDiagnosticsService::ProcessAxisDs402Home.*?if\s+startSdo\s+then.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Name 'Ds402MainUsesReserved'
$ds402MainGuardBeforeActiveDiagnostics = Move-OwnershipFixtureStatementBefore `
    -Text $diagnostics `
    -RegionPattern ('FUNCTION LMCDiagnosticsService::' +
        'ProcessAxisDs402Home.*?END_FUNCTION') `
    -StatementPattern ('[ \t]*if\s+ownerResult\s*<>\s*0\s+then\s*' +
        'failure\s*:=\s*TRUE\s*;\s*elsif\s+Ds402HomeState\[126\]\s*=\s*0' +
        '\s+then.*?end_if\s*;\r?\n?') `
    -BeforePattern '[ \t]*ownerResult\s*:=\s*AxisOwnership\.ValidateAxisOwnership\(' `
    -Name 'Ds402MainGuardBeforeActive'
$ds402Cleanup94ReservedDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(94:.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Name 'Ds402Cleanup94UsesReserved'
$ds402Cleanup96ReservedDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(96:.*?RequiredPhase:=)LMC_DIAG_OWNER_PHASE_ACTIVE' `
    -Replacement '${1}LMC_DIAG_OWNER_PHASE_RESERVED' `
    -Name 'Ds402Cleanup96UsesReserved'
$ds402Cleanup94TokenBeforeActiveDiagnostics = Move-OwnershipFixtureStatementBefore `
    -Text $diagnostics `
    -RegionPattern '(?ms)^\s*94\s*:.*?(?=^\s*95\s*:)' `
    -StatementPattern '[ \t]*Ds402HomeState\[99\]\s*:=\s*sdoToken\$DINT\s*;\r?\n?' `
    -BeforePattern '[ \t]*ownerResult\s*:=\s*-3\s*;' `
    -Name 'Ds402Cleanup94TokenBeforeActive'
$ds402Cleanup96TokenBeforeActiveDiagnostics = Move-OwnershipFixtureStatementBefore `
    -Text $diagnostics `
    -RegionPattern '(?ms)^\s*96\s*:.*?(?=^\s*97\s*:)' `
    -StatementPattern '[ \t]*Ds402HomeState\[99\]\s*:=\s*sdoToken\$DINT\s*;\r?\n?' `
    -BeforePattern '[ \t]*ownerResult\s*:=\s*-3\s*;' `
    -Name 'Ds402Cleanup96TokenBeforeActive'
$ds402Cleanup94FailureContinuesDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(94:.*?if\s+ownerResult\s*<>\s*0\s+then.*?Ds402HomeState\[92\]\s*:=\s*)98' `
    -Replacement '${1}95' `
    -Name 'Ds402Cleanup94FailureContinues'
$ds402Cleanup96FailureContinuesDiagnostics = Replace-OwnershipFixtureFirst `
    -Text $diagnostics `
    -Pattern '(96:.*?if\s+ownerResult\s*<>\s*0\s+then.*?Ds402HomeState\[92\]\s*:=\s*)98' `
    -Replacement '${1}97' `
    -Name 'Ds402Cleanup96FailureContinues'

$negativeFixtures = @(
    New-OwnershipFixture `
        -Name 'MatrixDormantTcpSingleFlip' `
        -Tcp $activeTcp `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixDormantControlSingleFlip' `
        -Control ($control.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE')) `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixDormantDiagnosticsSingleFlip' `
        -Diagnostics $activeDiagnostics `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixDormantInputLatchSingleFlip' `
        -InputLatch $activeInputLatch `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixDormantFeatureSingleFlip' `
        -Control ($control.Replace('0x00000017', '0x00000057')) `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixActiveTcpSingleFlip' `
        -Control $activeControl `
        -Diagnostics $activeDiagnostics `
        -InputLatch $activeInputLatch `
        -Tcp ($activeTcp.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE')) `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixActiveControlSingleFlip' `
        -Control ($activeControl.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE')) `
        -Diagnostics $activeDiagnostics `
        -InputLatch $activeInputLatch `
        -Tcp $activeTcp `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixActiveDiagnosticsSingleFlip' `
        -Control $activeControl `
        -Diagnostics ($activeDiagnostics.Replace(
            '#define LMC_DIAG_DS402_HOME_ENABLED TRUE',
            '#define LMC_DIAG_DS402_HOME_ENABLED FALSE')) `
        -InputLatch $activeInputLatch `
        -Tcp $activeTcp `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixActiveInputLatchSingleFlip' `
        -Control $activeControl `
        -Diagnostics $activeDiagnostics `
        -InputLatch ($activeInputLatch.Replace(
            '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED TRUE',
            '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE')) `
        -Tcp $activeTcp `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixActiveFeatureSingleFlip' `
        -Control ($activeControl.Replace('0x00000057', '0x00000017')) `
        -Diagnostics $activeDiagnostics `
        -InputLatch $activeInputLatch `
        -Tcp $activeTcp `
        -ExpectedMessage 'only all-dormant'
    New-OwnershipFixture `
        -Name 'MatrixDormantWrongFeatureLiteral' `
        -Control ($control.Replace('0x00000017', '0x00000037')) `
        -ExpectedMessage 'FeatureBits must be exactly one literal'
    New-OwnershipFixture `
        -Name 'MatrixActiveWrongFeatureLiteral' `
        -Control ($activeControl.Replace('0x00000057', '0x00000077')) `
        -Diagnostics $activeDiagnostics `
        -InputLatch $activeInputLatch `
        -Tcp $activeTcp `
        -ExpectedMessage 'FeatureBits must be exactly one literal'
    New-OwnershipFixture `
        -Name 'MatrixTcpGateMissing' `
        -Tcp ($tcp.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE', '')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixControlGateMissing' `
        -Control ($control.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE', '')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixDiagnosticsGateMissing' `
        -Diagnostics ($diagnostics.Replace(
            '#define LMC_DIAG_DS402_HOME_ENABLED FALSE', '')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixInputLatchGateMissing' `
        -InputLatch ($inputLatch.Replace(
            '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE', '')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixTcpGateDuplicate' `
        -Tcp (('#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE' +
            [Environment]::NewLine + $tcp)) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixControlGateDuplicate' `
        -Control (('#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE' +
            [Environment]::NewLine + $control)) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixDiagnosticsGateDuplicate' `
        -Diagnostics (('#define LMC_DIAG_DS402_HOME_ENABLED FALSE' +
            [Environment]::NewLine + $diagnostics)) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixInputLatchGateDuplicate' `
        -InputLatch (('#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE' +
            [Environment]::NewLine + $inputLatch)) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixTcpGateNonliteral' `
        -Tcp ($tcp.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED (FALSE)')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixControlGateNonliteral' `
        -Control ($control.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED (FALSE)')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixDiagnosticsGateNonliteral' `
        -Diagnostics ($diagnostics.Replace(
            '#define LMC_DIAG_DS402_HOME_ENABLED FALSE',
            '#define LMC_DIAG_DS402_HOME_ENABLED (FALSE)')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixInputLatchGateNonliteral' `
        -InputLatch ($inputLatch.Replace(
            '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED FALSE',
            '#define LMC_DS402_HOME_STARTUP_SWEEP_ENABLED (FALSE)')) `
        -ExpectedMessage 'must be one exact TRUE/FALSE literal definition'
    New-OwnershipFixture `
        -Name 'MatrixTcpClassifierGuardRemoved' `
        -Tcp ($tcp.Replace(
            'if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED & (CommandID <> 0x7D13) then',
            'if (CommandID <> 0x7D13) then')) `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixControlHandleGuardRemoved' `
        -Control ($control.Replace(
            'if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED then',
            'if TRUE then')) `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixControlReserveGuardRemoved' `
        -Control $controlReserveGateRemoved `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixControlProcessGuardRemoved' `
        -Control $controlProcessGateRemoved `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixDiagnosticsStartGuardRemoved' `
        -Diagnostics $diagnosticsStartGateRemoved `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixDiagnosticsProcessGuardRemoved' `
        -Diagnostics $diagnosticsProcessGateRemoved `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixInputLatchSweepGuardRemoved' `
        -InputLatch ($inputLatch.Replace(
            'if LMC_DS402_HOME_STARTUP_SWEEP_ENABLED &',
            'if TRUE &')) `
        -ExpectedMessage 'gate-use inventory'
    New-OwnershipFixture `
        -Name 'MatrixSdkAxisDs402BitDrift' `
        -AdminModels ($adminModels.Replace(
            'AxisDs402Home = 1u << 6',
            'AxisDs402Home = 1u << 5')) `
        -ExpectedMessage 'AxisDs402Home must be exactly 1u << 6'
    New-OwnershipFixture -Name 'ReserveHomeWrongResource' -Control $reserveHomeWrongResourceControl
    New-OwnershipFixture -Name 'ReserveEncoderWrongCommand' -Control $reserveEncoderWrongCommandControl
    New-OwnershipFixture -Name 'HomeTupleWrongResource' -Control $homeTupleWrongResourceControl
    New-OwnershipFixture -Name 'Ds402TupleReadAdmission' -Control $ds402TupleReadAdmissionControl
    New-OwnershipFixture -Name 'EncoderTupleWrongCommand' -Control $encoderTupleWrongCommandControl
    New-OwnershipFixture -Name 'EncoderTupleWrongMask' -Control $encoderTupleWrongMaskControl
    New-OwnershipFixture -Name 'DirectStopExactMask' -Control $directStopExactMaskControl
    New-OwnershipFixture -Name 'DirectResetSafetyExactMask' -Control $directResetSafetyExactMaskControl
    New-OwnershipFixture -Name 'GroupDisableRobotMask' -Control $groupDisableRobotMaskControl
    New-OwnershipFixture -Name 'GroupStopProfileOnly' -Control $groupStopProfileOnlyControl
    New-OwnershipFixture `
        -Name 'TupleValidBypassAfterClassifier' `
        -Control $tupleValidBypassControl `
        -ExpectedMessage 'tupleValid assignment inventory drifted'
    New-OwnershipFixture `
        -Name 'ExpectedStateBypassAfterActiveSelection' `
        -Control $expectedStateBypassControl `
        -ExpectedMessage 'expectedState assignment inventory drifted'
    New-OwnershipFixture -Name 'PhaseReservedConstantDrift' -Control (
        $control.Replace('#define LMC_OWNER_PHASE_RESERVED 1', '#define LMC_OWNER_PHASE_RESERVED 3'))
    New-OwnershipFixture -Name 'PhaseActiveConstantDrift' -Control (
        $control.Replace('#define LMC_OWNER_PHASE_ACTIVE 2', '#define LMC_OWNER_PHASE_ACTIVE 3'))
    New-OwnershipFixture -Name 'DiagnosticsPhaseActiveConstantDrift' -Diagnostics (
        $diagnostics.Replace('#define LMC_DIAG_OWNER_PHASE_ACTIVE 2', '#define LMC_DIAG_OWNER_PHASE_ACTIVE 3'))
    New-OwnershipFixture -Name 'InvalidPhaseAccepted' -Control (
        $control.Replace(
            '(RequiredPhase <> LMC_OWNER_PHASE_RESERVED) &',
            '(RequiredPhase = LMC_OWNER_PHASE_RESERVED) &'))
    New-OwnershipFixture -Name 'ActiveComparisonHardcodedReserved' -Control (
        $control.Replace(
            'OwnershipState[recordBase + 1] <> expectedState',
            'OwnershipState[recordBase + 1] <> LMC_OWNER_STATE_RESERVED'))
    New-OwnershipFixture -Name 'GroupLeaseAcceptedAsActive' -Control (
        $control.Replace(
            'LMC_OWNER_KIND_GROUP: expectedState := LMC_OWNER_STATE_GROUP_ACTIVE;',
            'LMC_OWNER_KIND_GROUP: expectedState := LMC_OWNER_STATE_GROUP_LEASE;'))
    New-OwnershipFixture -Name 'EncoderRunningAcceptedAsActive' -Control (
        $control.Replace(
            'LMC_OWNER_KIND_ENCODER: expectedState := LMC_OWNER_STATE_TW20_QUEUED;',
            'LMC_OWNER_KIND_ENCODER: expectedState := LMC_OWNER_STATE_TW20_RUNNING;'))
    New-OwnershipFixture -Name 'SafetyNonDirectAccepted' -Control (
        $control.Replace(
            '(OwnerKind = LMC_OWNER_KIND_DIRECT) |',
            '(TRUE) |'))
    New-OwnershipFixture -Name 'HandleRequestUsesActive' -Control $handleRequestActiveControl
    New-OwnershipFixture -Name 'CommitUsesActive' -Control $commitActiveControl
    New-OwnershipFixture -Name 'HomeStartUsesActive' -Control $homeStartActiveControl
    New-OwnershipFixture -Name 'HomeProcessUsesReserved' -Control $homeProcessReservedControl
    New-OwnershipFixture `
        -Name 'HomeCopyBeforeActive' `
        -Control $homeCopyBeforeActiveControl `
        -ExpectedMessage 'Home normal ACTIVE validation must enter exact-token cancel/drain'
    New-OwnershipFixture -Name 'HomeFailureNotQuarantined' -Control $homeFailureNotQuarantinedControl
    New-OwnershipFixture -Name 'EncoderStartUsesActive' -Diagnostics $encoderStartActiveDiagnostics
    New-OwnershipFixture -Name 'EncoderProcessUsesReserved' -Diagnostics $encoderProcessReservedDiagnostics
    New-OwnershipFixture `
        -Name 'EncoderClaimBeforeActive' `
        -Diagnostics $encoderClaimBeforeActiveDiagnostics `
        -ExpectedMessage 'Encoder ACTIVE validation must precede claim and SDO dispatch'
    New-OwnershipFixture -Name 'Ds402StartUsesActive' -Diagnostics $ds402StartActiveDiagnostics
    New-OwnershipFixture -Name 'Ds402MainUsesReserved' -Diagnostics $ds402MainReservedDiagnostics
    New-OwnershipFixture `
        -Name 'Ds402MainGuardBeforeActive' `
        -Diagnostics $ds402MainGuardBeforeActiveDiagnostics `
        -ExpectedMessage 'DS402 ACTIVE failure or first-dispatch publication fence drifted'
    New-OwnershipFixture -Name 'Ds402Cleanup94UsesReserved' -Diagnostics $ds402Cleanup94ReservedDiagnostics
    New-OwnershipFixture -Name 'Ds402Cleanup96UsesReserved' -Diagnostics $ds402Cleanup96ReservedDiagnostics
    New-OwnershipFixture `
        -Name 'Ds402Cleanup94TokenBeforeActive' `
        -Diagnostics $ds402Cleanup94TokenBeforeActiveDiagnostics `
        -ExpectedMessage 'DS402 cleanup stage 94 ACTIVE validation must precede token mutation'
    New-OwnershipFixture `
        -Name 'Ds402Cleanup96TokenBeforeActive' `
        -Diagnostics $ds402Cleanup96TokenBeforeActiveDiagnostics `
        -ExpectedMessage 'DS402 cleanup stage 96 ACTIVE validation must precede token mutation'
    New-OwnershipFixture -Name 'Ds402Cleanup94FailureContinues' -Diagnostics $ds402Cleanup94FailureContinuesDiagnostics
    New-OwnershipFixture -Name 'Ds402Cleanup96FailureContinues' -Diagnostics $ds402Cleanup96FailureContinuesDiagnostics
    New-OwnershipFixture -Name 'RemovedReportAbiRestored' -Control (
        $control + "`nFUNCTION GLOBAL LMCControlCommandService::ReportAxisOwnershipStartup`nEND_FUNCTION")
    New-OwnershipFixture -Name 'TcpDirectReconcile' -Tcp (
        $tcp.Replace(
            'ControlCommands.ProcessAxisZeroHome();',
            "ControlCommands.ReconcileAxisOwnershipStartup();`nControlCommands.ProcessAxisZeroHome();"))
    New-OwnershipFixture -Name 'BootIdOnlySymbolRestored' -Tcp (
        "#define LMC_OWNER_STARTUP_PROOF_BOOT_ID 0x00000001`n$tcp")
    New-OwnershipFixture -Name 'SnapshotCopyWrongOffset' -InputLatch (
        $inputLatch.Replace('#SnapshotBytes[464], cntr:=48', '#SnapshotBytes[0], cntr:=48'))
    New-OwnershipFixture -Name 'SnapshotCopyWrongLength' -InputLatch (
        $inputLatch.Replace('DestSize < 48', 'DestSize < 47'))
    New-OwnershipFixture -Name 'SnapshotReservedNonZero' -InputLatch (
        $inputLatch.Replace('SnapshotBytes[508]$UDINT := 0', 'SnapshotBytes[508]$UDINT := 1'))
    New-OwnershipFixture -Name 'LatchDrainBit3Missing' -InputLatch (
        $inputLatch.Replace(
            'ownershipLatchDrainFlags := ownershipLatchDrainFlags or LMC_OWNER_STARTUP_LATCH_OWNER;',
            'ownershipLatchDrainFlags := ownershipLatchDrainFlags;'))
    New-OwnershipFixture -Name 'ControlWord4Ignored' -InputLatch (
        $inputLatch.Replace('SnapshotBytes[288]$UDINT and 0x00000010', 'SnapshotBytes[288]$UDINT and 0'))
    New-OwnershipFixture -Name 'SameCycleReplayAccepted' -Control (
        $control.Replace(
            'OwnershipStartupState[2]$UDINT = ObservationCycle',
            'OwnershipStartupState[2]$UDINT <> ObservationCycle'))
    New-OwnershipFixture -Name 'StableSamplesTwo' -Control (
        $control.Replace(
            '#define LMC_OWNER_STARTUP_STABLE_SAMPLES 3',
            '#define LMC_OWNER_STARTUP_STABLE_SAMPLES 2'))
    New-OwnershipFixture -Name 'StableMilliseconds99' -Control (
        $control.Replace(
            '#define LMC_OWNER_STARTUP_STABLE_MS 100',
            '#define LMC_OWNER_STARTUP_STABLE_MS 99'))
    New-OwnershipFixture -Name 'Axis4ObservationDropped' -InputLatch (
        $inputLatch.Replace(
            'ownershipAxis4Status := LMCAxis4.ReadAxisStatus();',
            'ownershipAxis4Status$UDINT := 0;'))
    New-OwnershipFixture -Name 'AxisErrorRequired' -Control (
        $control.Replace(
            'physicalIdle :=',
            "axisError1 := LMCAxis1.ReadAxisError();`nphysicalIdle :="))
    New-OwnershipFixture -Name 'GroupDirectWithoutFinished' -Control (
        $control.Replace(
            '(robotState = _ROBOT_DIRECT) & profileFinished',
            '(robotState = _ROBOT_DIRECT)'))
    New-OwnershipFixture -Name 'ZeroHomeMailboxBusyAccepted' -InputLatch (
        $inputLatch.Replace(
            '(AxisZeroHomeMailbox[3]$UDINT = zeroHomeRequestSequence)',
            '(TRUE)'))
    New-OwnershipFixture -Name 'Ds402MailboxBusyAccepted' -InputLatch (
        $inputLatch.Replace(
            '(Ds402HomeMailbox[10] = 0)',
            '(TRUE)'))
    New-OwnershipFixture -Name 'SameBootQuarantineDropped' -Control (
        $control.Replace('(OwnershipState[24] = 0)', '(TRUE)'))
    New-OwnershipFixture -Name 'DiagnosticsStartupAfterGate' -Diagnostics (
        $diagnostics.Replace(
            ('ProcessAxisOwnershipStartup();' +
             [Environment]::NewLine +
             'if (LMC_DIAG_D5_SDO_READ_ENABLED = FALSE)'),
            "if (LMC_DIAG_D5_SDO_READ_ENABLED = FALSE)"))
    New-OwnershipFixture -Name 'DiagnosticsDs402BusyAccepted' -Diagnostics (
        $diagnostics.Replace('Ds402HomeState[92] = 0', 'Ds402HomeState[92] <> 0'))
    New-OwnershipFixture -Name 'DiagnosticsEncoderBusyAccepted' -Diagnostics (
        $diagnostics.Replace('EncoderMaintenanceState[152] = 0', 'EncoderMaintenanceState[152] <> 0'))
    New-OwnershipFixture -Name 'DiagnosticsQueuedAccepted' -Diagnostics (
        $diagnostics.Replace(
            'OperationState <> LMC_DIAG_SDO_STATE_QUEUED',
            'OperationState = LMC_DIAG_SDO_STATE_QUEUED'))
    New-OwnershipFixture -Name 'Executor4NotReusableAccepted' -Diagnostics (
        $diagnostics.Replace('SdoAxis4.IsReusable()', 'TRUE'))
    New-OwnershipFixture -Name 'PartialDiagnosticsDrain' -Diagnostics (
        $diagnostics.Replace(
            'DiagnosticsDrainFlags:=diagnosticsDrainFlags)',
            'DiagnosticsDrainFlags:=diagnosticsDrainFlags and 0x00000001)'))
    New-OwnershipFixture -Name 'StartupHelperGlobal' -Diagnostics (
        $diagnostics.Replace(
            'FUNCTION LMCDiagnosticsService::ProcessAxisOwnershipStartup',
            'FUNCTION GLOBAL LMCDiagnosticsService::ProcessAxisOwnershipStartup'))
    New-OwnershipFixture -Name 'HomeGateDisabled' -Control (
        $control.Replace('#define LMC_ADMIN_AXIS_HOME_ENABLED TRUE', '#define LMC_ADMIN_AXIS_HOME_ENABLED FALSE'))
    New-OwnershipFixture -Name 'Ds402GateEnabled' -Diagnostics (
        $diagnostics.Replace('#define LMC_DIAG_DS402_HOME_ENABLED FALSE', '#define LMC_DIAG_DS402_HOME_ENABLED TRUE'))
    New-OwnershipFixture -Name 'RequiredProofDrift' -Control (
        $control.Replace('LMC_OWNER_STARTUP_PROOF_REQUIRED 0x0000000F', 'LMC_OWNER_STARTUP_PROOF_REQUIRED 0x00000001'))
    New-OwnershipFixture -Name 'OrdinaryControlGateEnabled' -Control (
        $control.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE'))
    New-OwnershipFixture -Name 'OrdinaryTcpGateEnabled' -Tcp (
        $tcp.Replace(
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED FALSE',
            '#define LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED TRUE'))
    New-OwnershipFixture -Name 'OrdinaryProfileMaskDrift' -Tcp (
        $tcp.Replace(
            '#define LMC_OWNER_PROFILE_AXIS_MASK 0x0000000F',
            '#define LMC_OWNER_PROFILE_AXIS_MASK 0x000001FF'))
    New-OwnershipFixture -Name 'OrdinaryRobotMaskDrift' -Tcp (
        $tcp.Replace(
            '#define LMC_OWNER_ROBOT_AXIS_MASK 0x000001FF',
            '#define LMC_OWNER_ROBOT_AXIS_MASK 0x0000000F'))
    New-OwnershipFixture -Name 'OrdinaryOpcodeDropped' -Tcp (
        $tcp.Replace('0x20A2:', '0x20A1:'))
    New-OwnershipFixture -Name 'OrdinaryReadMisclassified' -Tcp (
        $tcp.Replace('0x20A2:', '0x20A2, 0x2028:'))
    New-OwnershipFixture -Name 'OrdinaryPowerOffClassifiedOrdinary' -Tcp (
        ([regex]::new(
            '(?s)(0x2023:.*?else\s+controlAdmissionMode\s*:=\s*)' +
            'LMC_OWNER_ADMISSION_SAFETY')).Replace(
                $tcp,
                '${1}LMC_OWNER_ADMISSION_ORDINARY',
                1))
    New-OwnershipFixture -Name 'OrdinaryGroupProfileMaskDrift' -Tcp (
        ([regex]::new(
            '(?s)(0x2047:.*?controlAxisMask\s*:=\s*)' +
            'LMC_OWNER_PROFILE_AXIS_MASK')).Replace(
                $tcp,
                '${1}LMC_OWNER_ROBOT_AXIS_MASK',
                1))
    New-OwnershipFixture -Name 'OrdinaryServiceAxisMapDrift' -Control (
        $control.Replace(
            '9: referenceAxisMask := 0x00000100;',
            '9: referenceAxisMask := 0x00000080;'))
    New-OwnershipFixture -Name 'OrdinaryServiceGroupReferenceGuardRemoved' -Control (
        $control.Replace(
            '(Reference <> 0x0100)',
            '(Reference = 0x0100)'))
    New-OwnershipFixture -Name 'OrdinaryTcpUnconditionalReservationReject' -Tcp (
        $tcp.Replace(
            'if controlAdmissionResult < 0 then',
            'if TRUE then'))
    New-OwnershipFixture -Name 'OrdinaryTcpPositiveRepeatRejected' -Tcp (
        $tcp.Replace(
            'if controlAdmissionResult < 0 then',
            'if controlAdmissionResult <> 0 then'))
    New-OwnershipFixture -Name 'OrdinaryTcpConflictWritesBothShapes' -Tcp (
        $tcp.Replace(
            'Sendbuf[10]$INT := -9;',
            ('Sendbuf[10]$INT := -9;' + [Environment]::NewLine +
             '                Sendbuf[14]$INT := -9;')))
    New-OwnershipFixture -Name 'OrdinaryFinalPrewireTokenBypass' -Control (
        $control.Replace(
            'if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED then',
            ('if LMC_AXIS_OWNERSHIP_ORDINARY_ENABLED & ' +
             '(AdmissionToken <> 0) then')))
    New-OwnershipFixture -Name 'OrdinaryFinalPrewireShapeBypass' -Control (
        $control.Replace(
            '(ownershipPayloadSize = 24) &',
            'TRUE &'))
    New-OwnershipFixture -Name 'OrdinaryFinalPrewireNeverArms' -Control (
        $control.Replace(
            'ownershipRequestShapeValid :=',
            'ownershipRequestShapeValid := FALSE &'))
    New-OwnershipFixture -Name 'OrdinaryFinalPrewireValidationRemoved' -Control (
        $control.Replace(
            'ownershipValidationResult := ValidateAxisOwnershipIdentity(',
            'ownershipValidationResult := ValidateAxisOwnershipIdentityLate('))
    New-OwnershipFixture -Name 'OrdinaryValidationFailureDispatches' -Control (
        $control.Replace(
            'ownershipInvokeHandler := FALSE;',
            'ownershipInvokeHandler := TRUE;'))
    New-OwnershipFixture -Name 'OrdinaryHandlerFailureCommits' -Control (
        $control.Replace(
            'ownershipAccepted := FALSE;',
            'ownershipAccepted := TRUE;'))
    New-OwnershipFixture -Name 'OrdinaryAcceptedResponseTrueOrBypass' -Control (
        $control.Replace(
            'ownershipAccepted :=',
            'ownershipAccepted := TRUE |'))
    New-OwnershipFixture -Name 'OrdinaryObserverStorageWrongSize' -Control (
        $control.Replace(
            'OwnershipObserverState : ARRAY [0..107] OF DINT;',
            'OwnershipObserverState : ARRAY [0..106] OF DINT;'))
    New-OwnershipFixture -Name 'OrdinaryObserverGateRemoved' -Control (
        $control.Replace(
            'OwnershipObserverState[0] := 0;',
            'observerStateWasRemoved := TRUE;'))
    New-OwnershipFixture -Name 'OrdinaryObserverHardwareBeforeGate' -Control (
        $control.Replace(
            'FUNCTION GLOBAL LMCControlCommandService::ProcessAxisOwnership',
            ('FUNCTION GLOBAL LMCControlCommandService::ProcessAxisOwnership' +
             [Environment]::NewLine +
             'LMCAxis1.ReadAxisStatus();')))
    New-OwnershipFixture -Name 'OrdinaryObserverStateWriteBeforeGate' -Control (
        $control.Replace(
            'FUNCTION GLOBAL LMCControlCommandService::ProcessAxisOwnership',
            ('FUNCTION GLOBAL LMCControlCommandService::ProcessAxisOwnership' +
             [Environment]::NewLine +
             'OwnershipState[0] := 0;')))
    New-OwnershipFixture -Name 'SdkAdapterMinus9SymbolMissing' -ErrorCatalog (
        $errorCatalog.Replace(
            '"AxisOwnershipConflict"',
            '"AxisOwnershipConflictMissing"'))
    New-OwnershipFixture -Name 'SdkAdapterComputedMinus9' -ErrorCatalog (
        $errorCatalog.Replace(
            'LMCErrorDomain.AdapterCommand, -9,',
            'LMCErrorDomain.AdapterCommand, -(8 + 1),'))
    New-OwnershipFixture -Name 'SdkCatalogVersionDrift' -ErrorCatalog (
        $errorCatalog.Replace(
            'public const uint CurrentCatalogVersion = 2;',
            'public const uint CurrentCatalogVersion = 3;'))
    New-OwnershipFixture -Name 'SdkAdapterSourceVersionDrift' -ErrorCatalog (
        $errorCatalog.Replace(
            'Elmo_Master TCPMotionInterface local errors v2',
            'Elmo_Master TCPMotionInterface local errors v3'))
    New-OwnershipFixture -Name 'PlcAdvertisedCatalogVersionDrift' -Control (
        $control.Replace(
            '(pResponseFrame + 44)^$UINT := 5;',
            '(pResponseFrame + 44)^$UINT := 4;')))

[pscustomobject]@{
    Control = $control
    Diagnostics = $diagnostics
    InputLatch = $inputLatch
    Tcp = $tcp
    ErrorCatalog = $errorCatalog
    AdminModels = $adminModels
    NegativeFixtures = $negativeFixtures
    SafetyRepeatControl = $safetyRepeatControl
    SafetyRepeatTcp = $safetyRepeatTcp
    SafetyRepeatNegativeFixtures = $safetyRepeatNegativeFixtures
}

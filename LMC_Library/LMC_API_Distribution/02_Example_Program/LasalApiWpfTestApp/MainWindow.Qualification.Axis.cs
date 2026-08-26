using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const int AxisQualificationMotionTimeoutMilliseconds = 15000;
        private const int AxisQualificationFinalPositionSamples = 3;

        private bool suppressAxisQualificationSafetyInvalidation;

        internal Action AxisQualificationMotionObservedTestHook { get; set; }

        private async void ButtonRunAxisQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!AreAxisQualificationSafetyConfirmationsChecked())
            {
                TextAxisQualificationProgress.Text =
                    "BLOCKED: all three physical safety confirmations are required. No RPC was sent.";
                WriteLog(
                    "Single Axis qualification blocked by missing physical "
                    + "safety confirmation. No RPC was sent.");
                return;
            }

            AxisQualificationInput input;
            try
            {
                input = ReadAxisQualificationInput();
            }
            catch (Exception error)
            {
                TextAxisQualificationProgress.Text =
                    "BLOCKED: " + error.Message + " No RPC was sent.";
                WriteLog(
                    "Single Axis qualification input rejected before wire: "
                    + error.Message);
                return;
            }

            if (connection == null
                || !connection.IsConnected
                || axis == null)
            {
                TextAxisQualificationProgress.Text =
                    "BLOCKED: connect and load the exact Axis first. No RPC was sent.";
                WriteLog(
                    "Single Axis qualification requires a connected, loaded "
                    + "Axis. No RPC was sent.");
                return;
            }

            if (!CanStartMotionCommand("Single Axis qualification")
                || !AxisPowerOnRecoveryJournalCanArm
                || !AxisCommandRecoveryJournalCanArm
                || !AxisQualificationRecoveryJournalCanArm)
            {
                TextAxisQualificationProgress.Text =
                    "BLOCKED: durable Power, Motion, or Stop admission is unavailable. No qualification mutation was sent.";
                return;
            }

            try
            {
                await RunQualificationAsync(
                    "SingleAxisPowerMoveStopPowerOff",
                    cancellationToken => RunAxisQualificationAsync(
                        input,
                        cancellationToken));
            }
            finally
            {
                InvalidateAxisQualificationConfirmations();
            }
        }

        private void AxisQualificationSafetyConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            if (ButtonRunAxisQualification != null)
            {
                UpdateUiState();
            }
        }

        private void AxisQualificationInput_Changed(
            object sender,
            TextChangedEventArgs e)
        {
            if (suppressAxisQualificationSafetyInvalidation)
            {
                return;
            }

            InvalidateAxisQualificationConfirmations();
        }

        private bool AreAxisQualificationSafetyConfirmationsChecked()
        {
            return CheckAxisQualificationTravelSafe != null
                && CheckAxisQualificationTravelSafe.IsChecked == true
                && CheckAxisQualificationIdentitySafe != null
                && CheckAxisQualificationIdentitySafe.IsChecked == true
                && CheckAxisQualificationExclusiveOwner != null
                && CheckAxisQualificationExclusiveOwner.IsChecked == true;
        }

        private void InvalidateAxisQualificationConfirmations()
        {
            if (suppressAxisQualificationSafetyInvalidation)
            {
                return;
            }

            suppressAxisQualificationSafetyInvalidation = true;
            try
            {
                if (CheckAxisQualificationTravelSafe != null)
                {
                    CheckAxisQualificationTravelSafe.IsChecked = false;
                }
                if (CheckAxisQualificationIdentitySafe != null)
                {
                    CheckAxisQualificationIdentitySafe.IsChecked = false;
                }
                if (CheckAxisQualificationExclusiveOwner != null)
                {
                    CheckAxisQualificationExclusiveOwner.IsChecked = false;
                }
            }
            finally
            {
                suppressAxisQualificationSafetyInvalidation = false;
            }

            if (ButtonRunAxisQualification != null)
            {
                UpdateUiState();
            }
        }

        private AxisQualificationInput ReadAxisQualificationInput()
        {
            var delta = ParseQualificationInt32(
                TextAxisQualificationDelta.Text,
                "Axis qualification relative delta raw");
            if (delta == 0)
            {
                throw new InvalidOperationException(
                    "Axis qualification relative delta must be nonzero.");
            }
            if (Math.Abs((long)delta)
                > MaximumQualificationDeltaMagnitudeRaw)
            {
                throw new InvalidOperationException(
                    "Axis qualification relative delta is limited to +/-"
                    + MaximumQualificationDeltaMagnitudeRaw.ToString(
                        CultureInfo.InvariantCulture)
                    + " raw DINT.");
            }

            var velocity = ParseQualificationPositiveInt32(
                TextAxisQualificationVelocity.Text,
                "Axis qualification velocity raw");
            var acceleration = ParseQualificationPositiveInt32(
                TextAxisQualificationAcceleration.Text,
                "Axis qualification acceleration raw");
            var deceleration = ParseQualificationPositiveInt32(
                TextAxisQualificationDeceleration.Text,
                "Axis qualification deceleration raw");
            var jerk = ParseQualificationNonNegativeInt32(
                TextAxisQualificationJerk.Text,
                "Axis qualification jerk raw");
            if (jerk != 0)
            {
                throw new InvalidOperationException(
                    "The first live Single Axis qualification requires Jerk raw = 0.");
            }
            var tolerance = ParseQualificationPositiveInt32(
                TextAxisQualificationTolerance.Text,
                "Axis qualification final tolerance raw");

            return new AxisQualificationInput(
                delta,
                velocity,
                acceleration,
                deceleration,
                jerk,
                tolerance);
        }

        private async Task RunAxisQualificationAsync(
            AxisQualificationInput input,
            CancellationToken cancellationToken)
        {
            var currentConnection = RequireConnection();
            var currentAxis = RequireAxis();
            var counters = new AxisQualificationCounters();
            Exception primaryError = null;
            Exception stopError = null;
            Exception powerOffError = null;
            AxisQualificationIdentity identity = null;
            var externalSafetyState = AxisQualificationExternalSafetyState.None;
            var cleanupSafetyGeneration = qualificationSafetyGeneration;
            var verifiedExternalSafetyGeneration = 0L;

            try
            {
                SetQualificationProgress(4, "Single Axis identity preflight");
                identity = await CaptureAxisQualificationIdentityAsync(
                    currentConnection,
                    currentAxis,
                    cancellationToken);
                WriteAxisQualificationIdentity(identity, input);
                ArmAxisQualificationRecoveryBeforePowerOn(identity, input);

                SetQualificationProgress(12, "Axis Power On accepted-once");
                var powerOn = await ExecuteAxisQualificationPowerOnAsync(
                    identity,
                    currentAxis,
                    counters,
                    cancellationToken);
                WriteQualificationLog(
                    "event=AXIS_POWER_ON_PROOF",
                    "command=0x2023",
                    "commandCount=1",
                    "statusReads=" + powerOn.PollCount,
                    "stable=" + powerOn.StableSampleCount
                        + "/" + powerOn.RequiredStableSampleCount,
                    "state=0x" + powerOn.FinalStatus.State.ToString("X8"),
                    "axisError=0x" + powerOn.FinalStatus.AxisErrorId.ToString("X4"),
                    "statusWord=0x" + powerOn.FinalStatus.StatusWord.ToString("X4"));

                SetQualificationProgress(25, "Move preflight status and start position");
                await VerifyAxisQualificationReadyToMoveAsync(
                    identity,
                    currentAxis,
                    counters,
                    cancellationToken);
                var startPosition = await ReadAxisQualificationPositionAsync(
                    identity,
                    currentAxis,
                    counters,
                    "start",
                    cancellationToken);
                int targetPosition;
                checked
                {
                    targetPosition = startPosition + input.DeltaRaw;
                }
                counters.StartPositionRaw = startPosition;
                counters.TargetPositionRaw = targetPosition;
                WriteQualificationLog(
                    "event=AXIS_MOVE_PLAN",
                    "startRaw=" + startPosition,
                    "deltaRaw=" + input.DeltaRaw,
                    "targetRaw=" + targetPosition,
                    "toleranceRaw=" + input.ToleranceRaw);
                cancellationToken.ThrowIfCancellationRequested();
                SetQualificationProgress(38, "Move Relative accepted-once");
                await ExecuteAxisQualificationMoveRelativeAsync(
                    identity,
                    currentAxis,
                    input,
                    counters,
                    cancellationToken);

                SetQualificationProgress(52, "Observe motion then three stable Standstill samples");
                await WaitForAxisQualificationMotionAsync(
                    identity,
                    currentAxis,
                    counters,
                    cancellationToken);

                SetQualificationProgress(66, "Read and verify final position three times");
                await VerifyAxisQualificationFinalPositionAsync(
                    identity,
                    currentAxis,
                    input,
                    counters,
                    cancellationToken);
                CheckpointAxisQualificationMoveStable(
                    identity,
                    currentAxis,
                    "Single Axis qualification Move stable proof");
            }
            catch (Exception error)
            {
                primaryError = error;
                if (!counters.PowerOnDispatchStarted && identity != null)
                {
                    try
                    {
                        ResolveAxisQualificationKnownNoEffectBeforePowerOn(
                            identity,
                            currentAxis,
                            "Single Axis qualification pre-PowerOn known no-effect");
                    }
                    catch (Exception journalError)
                    {
                        primaryError = new AggregateException(
                            error,
                            journalError);
                    }
                }
                if (motionMayBeActive)
                {
                    RequireExplicitMotionRecoverySafety(
                        "Single Axis qualification incomplete");
                }
                WriteQualificationLog(
                    "event=AXIS_PRIMARY_PATH_INCOMPLETE",
                    "errorType=" + error.GetType().Name,
                    "error=" + QualificationValue(error.Message),
                    "moveReplayCount=0");
            }

            if (counters.PowerOnDispatchStarted)
            {
                if (qualificationExternalAxisSafetyKind
                    != AxisQualificationExternalSafetyKind.None)
                {
                    SetQualificationProgress(
                        75,
                        "Verify external safety command without replay");
                    try
                    {
                        externalSafetyState = await
                            VerifyExternalAxisQualificationSafetyAsync(
                                identity,
                                currentAxis,
                                counters);
                        verifiedExternalSafetyGeneration =
                            counters.VerifiedExternalSafetyGeneration;
                        cleanupSafetyGeneration =
                            verifiedExternalSafetyGeneration;
                        if (externalSafetyState
                            == AxisQualificationExternalSafetyState.PowerOffStandstill)
                        {
                            counters.StopStableProven = true;
                            counters.PowerOffStableProven = true;
                        }
                        else if (externalSafetyState
                            == AxisQualificationExternalSafetyState.Standstill)
                        {
                            counters.StopStableProven = true;
                        }
                    }
                    catch (Exception error)
                    {
                        powerOffError = error;
                        WriteQualificationLog(
                            "event=AXIS_EXTERNAL_SAFETY_PROOF_INCOMPLETE",
                            "operation=" + QualificationValue(
                                qualificationExternalSafetyOperation),
                            "automaticReplayCount=0",
                            "safeState=UNPROVEN",
                            "errorType=" + error.GetType().Name,
                            "error=" + QualificationValue(error.Message));
                    }
                }

                if (powerOffError == null
                    && counters.MoveDispatchStarted
                    && externalSafetyState
                        == AxisQualificationExternalSafetyState.None)
                {
                    SetQualificationProgress(78, "Axis Stop accepted-once and stable proof");
                    try
                    {
                        await ExecuteAxisQualificationStopAsync(
                            identity,
                            currentAxis,
                            input,
                            counters,
                            cleanupSafetyGeneration);
                    }
                    catch (Exception error)
                    {
                        stopError = error;
                        WriteQualificationLog(
                            "event=AXIS_STOP_PROOF_INCOMPLETE",
                            "command=0x2022",
                            "automaticReplayCount=0",
                            "errorType=" + error.GetType().Name,
                            "error=" + QualificationValue(error.Message));
                    }
                }

                if (powerOffError == null
                    && qualificationExternalAxisSafetyGeneration != 0
                    && qualificationExternalAxisSafetyGeneration
                        != verifiedExternalSafetyGeneration)
                {
                    SetQualificationProgress(
                        86,
                        "Re-verify newer external Axis safety before Power Off");
                    try
                    {
                        externalSafetyState = await
                            VerifyExternalAxisQualificationSafetyAsync(
                                identity,
                                currentAxis,
                                counters);
                        verifiedExternalSafetyGeneration =
                            counters.VerifiedExternalSafetyGeneration;
                        cleanupSafetyGeneration =
                            verifiedExternalSafetyGeneration;
                        if (externalSafetyState
                            == AxisQualificationExternalSafetyState.PowerOffStandstill)
                        {
                            counters.StopStableProven = true;
                            counters.PowerOffStableProven = true;
                        }
                        else if (externalSafetyState
                            == AxisQualificationExternalSafetyState.Standstill)
                        {
                            counters.StopStableProven = true;
                        }
                    }
                    catch (Exception error)
                    {
                        powerOffError = error;
                        WriteQualificationLog(
                            "event=AXIS_EXTERNAL_SAFETY_RECHECK_INCOMPLETE",
                            "operation=" + QualificationValue(
                                qualificationExternalSafetyOperation),
                            "automaticReplayCount=0",
                            "safeState=UNPROVEN",
                            "errorType=" + error.GetType().Name,
                            "error=" + QualificationValue(error.Message));
                    }
                }

                if (powerOffError == null
                    && externalSafetyState
                        != AxisQualificationExternalSafetyState.PowerOffStandstill)
                {
                    SetQualificationProgress(90, "Axis Power Off accepted-once and stable proof");
                    try
                    {
                        await ExecuteAxisQualificationPowerOffAsync(
                            identity,
                            currentAxis,
                            counters,
                            cleanupSafetyGeneration);
                    }
                    catch (Exception error)
                    {
                        powerOffError = error;
                        WriteQualificationLog(
                            "event=AXIS_POWER_OFF_PROOF_INCOMPLETE",
                            "command=0x2023",
                            "automaticReplayCount=0",
                            "safeState="
                                + (counters.PowerOffStableProven
                                    ? "POWER_OFF_STANDSTILL_JOURNAL_UNRESOLVED"
                                    : "UNPROVEN"),
                            "errorType=" + error.GetType().Name,
                            "error=" + QualificationValue(error.Message));
                    }
                }

                while (powerOffError != null
                    && qualificationExternalAxisSafetyGeneration != 0
                    && qualificationExternalAxisSafetyGeneration
                        != verifiedExternalSafetyGeneration)
                {
                    var priorPowerOffError = powerOffError;
                    try
                    {
                        externalSafetyState = await
                            VerifyExternalAxisQualificationSafetyAsync(
                                identity,
                                currentAxis,
                                counters);
                        verifiedExternalSafetyGeneration =
                            counters.VerifiedExternalSafetyGeneration;
                        cleanupSafetyGeneration =
                            verifiedExternalSafetyGeneration;
                    }
                    catch (Exception externalSafetyError)
                    {
                        powerOffError = new InvalidOperationException(
                            "A newer external Axis safety command could not be verified after qualification Power Off lost cleanup ownership.",
                            new AggregateException(
                                priorPowerOffError,
                                externalSafetyError));
                        break;
                    }

                    if (externalSafetyState
                        == AxisQualificationExternalSafetyState.PowerOffStandstill)
                    {
                        counters.StopStableProven = true;
                        counters.PowerOffStableProven = true;
                        powerOffError = null;
                        WriteQualificationLog(
                            "event=AXIS_EXTERNAL_SAFETY_RECONCILED",
                            "operation=" + QualificationValue(
                                qualificationExternalSafetyOperation),
                            "safeState=POWER_OFF_STANDSTILL",
                            "runnerPowerOffReplayCount=0",
                            "verdict=PASS");
                        break;
                    }

                    if (externalSafetyState
                            == AxisQualificationExternalSafetyState.Standstill
                        && counters.PowerOffCommands == 0)
                    {
                        try
                        {
                            await ExecuteAxisQualificationPowerOffAsync(
                                identity,
                                currentAxis,
                                counters,
                                cleanupSafetyGeneration);
                            powerOffError = null;
                            WriteQualificationLog(
                                "event=AXIS_EXTERNAL_STOP_REBASED_POWER_OFF",
                                "externalStopReplayCount=0",
                                "runnerPowerOffCommandCount=1",
                                "verdict=PASS");
                        }
                        catch (Exception retryError)
                        {
                            powerOffError = retryError;
                        }
                    }

                    if (powerOffError != null
                        && qualificationExternalAxisSafetyGeneration
                            == verifiedExternalSafetyGeneration)
                    {
                        break;
                    }
                }
            }

            WriteAxisQualificationCounts(counters, primaryError, stopError, powerOffError);

            if (powerOffError != null)
            {
                var durableEvidenceActive = motionMayBeActive
                    || GetActiveAxisPowerRecoveryRecord() != null
                    || GetActiveAxisCommandRecoveryRecord() != null;
                throw new InvalidOperationException(
                    counters.PowerOffStableProven
                        ? "Single Axis qualification failed after stable Power Off was proven because durable motion or Stop cleanup did not complete. Review the retained recovery evidence before another mutation."
                        : durableEvidenceActive
                        ? "Single Axis qualification failed and stable Power Off was not proven. Active command-level durable recovery evidence was retained; do not assume a safe state."
                        : "Single Axis qualification failed and stable Power Off was not proven. The whole-sequence recovery record remains active; reconnect only to the recorded PLC identity and use explicit safety recovery.",
                    CombineAxisQualificationErrors(
                        primaryError,
                        stopError,
                        powerOffError));
            }

            if (primaryError != null)
            {
                ExceptionDispatchInfo.Capture(primaryError).Throw();
            }

            if (stopError != null)
            {
                throw new InvalidOperationException(
                    "Single Axis Stop qualification was incomplete even though stable Power Off was proven.",
                    stopError);
            }

            cancellationToken.ThrowIfCancellationRequested();
            WriteQualificationLog(
                "event=AXIS_QUALIFICATION_RESULT",
                "verdict=PASS",
                "safeState=POWER_OFF_STANDSTILL",
                "evidenceRetention=keep_capture_running_at_least_2s_after_PASS",
                "inMotionStopProof=NOT_CLAIMED");
        }

        private async Task<AxisQualificationExternalSafetyState>
            VerifyExternalAxisQualificationSafetyAsync(
                AxisQualificationIdentity identity,
                LMCSingleAxis currentAxis,
                AxisQualificationCounters counters)
        {
            if (qualificationExternalAxisSafetyKind
                == AxisQualificationExternalSafetyKind.None)
            {
                return AxisQualificationExternalSafetyState.None;
            }

            var operation = qualificationExternalSafetyOperation
                ?? "external safety";
            WriteQualificationLog(
                "event=AXIS_EXTERNAL_SAFETY_VERIFY",
                "operation=" + QualificationValue(operation),
                "mode=status_only",
                "automaticReplayCount=0",
                "verdict=START");

            var dispatchDeadline = DateTime.UtcNow.AddSeconds(8);
            while ((safetyCommandRunning || safetyMonitorCount > 0)
                && DateTime.UtcNow < dispatchDeadline)
            {
                await Task.Delay(25);
            }

            if (safetyCommandRunning || safetyMonitorCount > 0)
            {
                throw new TimeoutException(
                    "External "
                    + operation
                    + " did not finish within 8 seconds. The qualification did not replay Stop or Power Off.");
            }

            var proofGeneration =
                qualificationExternalAxisSafetyGeneration;
            var powerOffStandstillSamples = 0;
            var standstillSamples = 0;
            var proofDeadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < proofDeadline)
            {
                if (proofGeneration
                    != qualificationExternalAxisSafetyGeneration)
                {
                    proofGeneration =
                        qualificationExternalAxisSafetyGeneration;
                    powerOffStandstillSamples = 0;
                    standstillSamples = 0;
                }
                var status = await SendQualificationCleanupCommandAsync(
                    "Single Axis external safety status-only verification",
                    async () =>
                    {
                        await RefreshAndValidateAxisQualificationIdentityAsync(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis external safety final identity");
                        counters.StatusReads++;
                        return await currentAxis.ReadStatusResultAsync(
                            CancellationToken.None);
                    });
                if (status == null || !status.IsSuccess)
                {
                    throw new InvalidOperationException(
                        "External "
                        + operation
                        + " status-only verification returned an invalid Axis status.");
                }
                if (proofGeneration
                    != qualificationExternalAxisSafetyGeneration)
                {
                    proofGeneration =
                        qualificationExternalAxisSafetyGeneration;
                    powerOffStandstillSamples = 0;
                    standstillSamples = 0;
                    continue;
                }

                powerOffStandstillSamples = !status.IsPowerOn
                        && status.IsStandstill
                    ? powerOffStandstillSamples + 1
                    : 0;
                standstillSamples = status.IsStandstill
                    ? standstillSamples + 1
                    : 0;

                if (powerOffStandstillSamples
                    >= QualificationStableSamples)
                {
                    EnsureExternalAxisQualificationRecoveryResolved(
                        operation,
                        true);
                    WriteQualificationLog(
                        "event=AXIS_EXTERNAL_SAFETY_VERIFY",
                        "operation=" + QualificationValue(operation),
                        "safeState=POWER_OFF_STANDSTILL",
                        "samples=" + powerOffStandstillSamples,
                        "automaticReplayCount=0",
                        "verdict=PASS");
                    counters.VerifiedExternalSafetyGeneration =
                        proofGeneration;
                    return AxisQualificationExternalSafetyState
                        .PowerOffStandstill;
                }

                var requiresPowerOff = qualificationExternalAxisSafetyKind
                    == AxisQualificationExternalSafetyKind.PowerOff;
                if (!requiresPowerOff
                    && standstillSamples >= QualificationStableSamples)
                {
                    EnsureExternalAxisQualificationRecoveryResolved(
                        operation,
                        false);
                    WriteQualificationLog(
                        "event=AXIS_EXTERNAL_SAFETY_VERIFY",
                        "operation=" + QualificationValue(operation),
                        "safeState=STANDSTILL",
                        "samples=" + standstillSamples,
                        "automaticReplayCount=0",
                        "verdict=PASS");
                    counters.VerifiedExternalSafetyGeneration =
                        proofGeneration;
                    return AxisQualificationExternalSafetyState.Standstill;
                }

                await Task.Delay(QualificationPollMilliseconds);
            }

            throw new TimeoutException(
                "External "
                + operation
                + (qualificationExternalAxisSafetyKind
                        == AxisQualificationExternalSafetyKind.PowerOff
                    ? " did not prove three stable PowerOff+Standstill samples."
                    : " did not prove three stable Standstill samples.")
                + " The qualification did not replay the external safety command.");
        }

        private void EnsureExternalAxisQualificationRecoveryResolved(
            string operation,
            bool powerOffProven)
        {
            if (motionMayBeActive
                || GetActiveAxisCommandRecoveryRecord() != null
                || (powerOffProven
                    && GetActiveAxisPowerRecoveryRecord() != null))
            {
                throw new InvalidOperationException(
                    "External "
                    + operation
                    + " reached a stable physical state, but the exact Axis durable recovery records are still active. The qualification will not issue a duplicate safety mutation.");
            }
        }

        private async Task<AxisQualificationIdentity>
            CaptureAxisQualificationIdentityAsync(
                LMCConnection currentConnection,
                LMCSingleAxis currentAxis,
                CancellationToken cancellationToken)
        {
            return await SendQualificationCommandAsync(
                "Single Axis qualification identity snapshot",
                cancellationToken,
                async () =>
                {
                    await RefreshDiagnosticsCapabilitiesAsync(
                        currentConnection);
                    var capabilities = diagnosticCapabilities;
                    if (capabilities == null
                        || capabilities.DiagnosticsBuild == 0
                        || capabilities.DiagnosticsBootId == 0
                        || capabilities.MapRevision == 0)
                    {
                        throw new InvalidOperationException(
                            "Single Axis qualification requires nonzero DiagnosticsBuild, BootId, and MapRevision.");
                    }

                    var lookupIdentity = RequireMotionLookupIdentity(
                        MotionUncertaintyTargetKind.Axis,
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        "Single Axis qualification identity snapshot");
                    if (!lookupIdentity.Matches(
                        MotionUncertaintyTargetKind.Axis,
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        capabilities.DiagnosticsBootId,
                        capabilities.MapRevision))
                    {
                        throw new InvalidOperationException(
                            "The loaded Axis lookup identity does not match the current PLC BootId and MapRevision. Reload the Axis before running the live qualification.");
                    }

                    var captured = new AxisQualificationIdentity(
                        currentConnection,
                        currentConnection.SessionGeneration,
                        RequiredConnectedRemoteIp(),
                        RequiredConnectedRemotePort(),
                        currentAxis.AxisName,
                        currentAxis.AxisReference,
                        capabilities.DiagnosticsBuild,
                        capabilities.DiagnosticsBootId,
                        capabilities.MapRevision);
                    ValidateAxisQualificationIdentityCached(
                        captured,
                        currentConnection,
                        currentAxis,
                        "Single Axis qualification identity snapshot");
                    return captured;
                });
        }

        private async Task RefreshAndValidateAxisQualificationIdentityAsync(
            AxisQualificationIdentity identity,
            LMCConnection currentConnection,
            LMCSingleAxis currentAxis,
            string operation)
        {
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            ValidateAxisQualificationIdentityCached(
                identity,
                currentConnection,
                currentAxis,
                operation);
        }

        private void ValidateAxisQualificationIdentityCached(
            AxisQualificationIdentity identity,
            LMCConnection currentConnection,
            LMCSingleAxis currentAxis,
            string operation)
        {
            if (identity == null)
            {
                throw new InvalidOperationException(
                    operation + " requires a captured Axis qualification identity.");
            }

            var capabilities = diagnosticCapabilities;
            var lookupIdentity = RequireMotionLookupIdentity(
                MotionUncertaintyTargetKind.Axis,
                identity.AxisName,
                identity.AxisReference,
                operation);
            if (currentConnection == null
                || !currentConnection.IsConnected
                || currentAxis == null
                || !ReferenceEquals(connection, currentConnection)
                || !ReferenceEquals(axis, currentAxis)
                || !ReferenceEquals(identity.ConnectionOwner, currentConnection)
                || !ReferenceEquals(currentAxis.Connection, currentConnection)
                || currentConnection.SessionGeneration != identity.SessionGeneration
                || currentAxis.SessionGeneration != identity.SessionGeneration
                || !string.Equals(
                    RequiredConnectedRemoteIp(),
                    identity.EndpointIp,
                    StringComparison.OrdinalIgnoreCase)
                || RequiredConnectedRemotePort() != identity.EndpointPort
                || !string.Equals(
                    currentAxis.AxisName,
                    identity.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != identity.AxisReference
                || capabilities == null
                || !ReferenceEquals(
                    capabilities.Owner,
                    currentConnection.Diagnostics)
                || capabilities.ConnectionSessionGeneration
                    != identity.SessionGeneration
                || capabilities.DiagnosticsBuild != identity.DiagnosticsBuild
                || capabilities.DiagnosticsBootId != identity.DiagnosticsBootId
                || capabilities.MapRevision != identity.MapRevision
                || !lookupIdentity.Matches(
                    MotionUncertaintyTargetKind.Axis,
                    identity.AxisName,
                    identity.AxisReference,
                    identity.DiagnosticsBootId,
                    identity.MapRevision))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the exact connection owner/session, endpoint, Axis name/reference, DiagnosticsBuild, BootId, or MapRevision changed. No new mutation may be inferred for the captured PLC identity.");
            }
        }

        private void WriteAxisQualificationIdentity(
            AxisQualificationIdentity identity,
            AxisQualificationInput input)
        {
            WriteQualificationLog(
                "event=AXIS_IDENTITY_PINNED",
                "endpoint=" + QualificationValue(
                    identity.EndpointIp + ":" + identity.EndpointPort),
                "session=" + identity.SessionGeneration,
                "axisName=" + QualificationValue(identity.AxisName),
                "axisReference=" + identity.AxisReference,
                "diagnosticsBuild=0x"
                    + identity.DiagnosticsBuild.ToString("X8"),
                "bootId=0x" + identity.DiagnosticsBootId.ToString("X8"),
                "mapRevision=0x" + identity.MapRevision.ToString("X8"),
                "deltaRaw=" + input.DeltaRaw,
                "velocityRaw=" + input.VelocityRaw,
                "accelerationRaw=" + input.AccelerationRaw,
                "decelerationRaw=" + input.DecelerationRaw,
                "jerkRaw=" + input.JerkRaw,
                "toleranceRaw=" + input.ToleranceRaw);
        }

        private async Task<LMCAxisPowerStateWaitResult>
            ExecuteAxisQualificationPowerOnAsync(
                AxisQualificationIdentity identity,
                LMCSingleAxis currentAxis,
                AxisQualificationCounters counters,
                CancellationToken cancellationToken)
        {
            AxisPowerOnRecoveryRecord verificationRecord = null;
            var dispatchStarted = false;
            var statusReadsBefore = counters.StatusReads;
            try
            {
                return await SendQualificationCommandAsync(
                    "Single Axis qualification Power On",
                    cancellationToken,
                    async () =>
                    {
                        await RefreshAndValidateAxisQualificationIdentityAsync(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Power On preflight");
                        EnsureNoUnresolvedDiagnosticMutation(
                            "Single Axis qualification Power On");
                        cancellationToken.ThrowIfCancellationRequested();
                        verificationRecord = await
                            ArmAxisPowerRecoveryBeforeDispatchAsync(
                                currentAxis,
                                true,
                                true);
                        dispatchStarted = true;
                        counters.PowerOnDispatchStarted = true;
                        counters.PowerOnCommands++;
                        var result = await currentAxis
                            .PowerOnAndWaitForStableStateAsync(
                                new LMCAxisPowerStateWaitOptions
                                {
                                    StableSampleCount = 3
                                },
                                accepted =>
                                    RunAxisQualificationAcceptedObserver(
                                        () =>
                                            PersistAxisPowerOnAcceptedForRecord(
                                                accepted,
                                                verificationRecord,
                                                "Single Axis qualification Power On accepted")),
                                cancellationToken);
                        counters.StatusReads += result.PollCount;
                        await CompleteAxisPowerRecoveryAfterStableProofAsync(
                            currentAxis,
                            true,
                            result.FinalStatus,
                            result.StableSampleCount,
                            result.RequiredStableSampleCount,
                            verificationRecord,
                            "Single Axis qualification Power On stable proof",
                            () => ValidateAxisQualificationIdentityCached(
                                identity,
                                identity.ConnectionOwner,
                                currentAxis,
                                "Single Axis qualification Power On final identity"));
                        return result;
                    });
            }
            catch (Exception error)
            {
                var evidence = GetAxisPowerOnWaitEvidence(error);
                if (counters.StatusReads == statusReadsBefore
                    && evidence != null)
                {
                    counters.StatusReads += evidence.StatusPollCount;
                }
                if (verificationRecord != null)
                {
                    PreserveAxisPowerOnWaitFailure(
                        currentAxis,
                        error,
                        verificationRecord,
                        dispatchStarted,
                        "Single Axis qualification Power On");
                }
                throw;
            }
        }

        private async Task VerifyAxisQualificationReadyToMoveAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationCounters counters,
            CancellationToken cancellationToken)
        {
            var status = await SendQualificationCommandAsync(
                "Single Axis qualification ready-to-move status",
                cancellationToken,
                async () =>
                {
                    await RefreshAndValidateAxisQualificationIdentityAsync(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification ready-to-move preflight");
                    counters.StatusReads++;
                    return await currentAxis.ReadStatusResultAsync(
                        cancellationToken);
                });
            if (!status.IsSuccess
                || !status.IsPowerOn
                || !status.IsReferenced
                || !status.IsStandstill)
            {
                throw new InvalidOperationException(
                    "Move preflight requires a successful, error-free PowerOn + Referenced + Standstill status. State=0x"
                    + status.State.ToString("X8")
                    + ", AxisError=0x"
                    + status.AxisErrorId.ToString("X4")
                    + ", StatusWord=0x"
                    + status.StatusWord.ToString("X4")
                    + ".");
            }

            WriteQualificationLog(
                "event=AXIS_MOVE_PREFLIGHT_PASS",
                "state=0x" + status.State.ToString("X8"),
                "axisError=0x" + status.AxisErrorId.ToString("X4"),
                "statusWord=0x" + status.StatusWord.ToString("X4"));
        }

        private async Task<LMCReadStatusResult>
            ReadAxisQualificationStatusAsync(
                AxisQualificationIdentity identity,
                LMCSingleAxis currentAxis,
                AxisQualificationCounters counters,
                string phase,
                CancellationToken cancellationToken)
        {
            return await SendQualificationCommandAsync(
                "Single Axis qualification status " + phase,
                cancellationToken,
                async () =>
                {
                    ValidateAxisQualificationIdentityCached(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification status " + phase);
                    counters.StatusReads++;
                    return await currentAxis.ReadStatusResultAsync(
                        cancellationToken);
                });
        }

        private async Task<int> ReadAxisQualificationPositionAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationCounters counters,
            string phase,
            CancellationToken cancellationToken)
        {
            var result = await SendQualificationCommandAsync(
                "Single Axis qualification position " + phase,
                cancellationToken,
                async () =>
                {
                    ValidateAxisQualificationIdentityCached(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification position " + phase);
                    counters.PositionReads++;
                    return await currentAxis.GetActualPositionResultAsync(
                        cancellationToken);
                });
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Single Axis qualification "
                    + phase
                    + " position read failed.");
            }
            WriteQualificationLog(
                "event=AXIS_POSITION_SAMPLE",
                "phase=" + QualificationValue(phase),
                "positionRaw=" + result.PositionRaw,
                "functionStatus=0x" + result.FunctionStatus.ToString("X4"),
                "errorId=" + result.ErrorId);
            return result.PositionRaw;
        }

        private async Task ExecuteAxisQualificationMoveRelativeAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationInput input,
            AxisQualificationCounters counters,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await DispatchTrackedMotionAsync(
                qualificationSafetyGeneration,
                MotionUncertaintyTargetKind.Axis,
                identity.AxisName,
                identity.AxisReference,
                "Single Axis qualification Move Relative",
                generation => counters.MotionTrackingGeneration = generation,
                async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    counters.MoveDispatchStarted = true;
                    counters.MoveRelativeCommands++;
                    return await currentAxis.MoveRelativeExAsync(
                        input.DeltaRaw,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        LMC_DIRECTION.Shortest,
                        cancellationToken);
                },
                async () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ValidateAxisQualificationIdentityCached(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification Move Relative final pre-wire identity");
                    PrepareAxisQualificationMoveRecovery(
                        identity,
                        currentAxis,
                        counters.StartPositionRaw,
                        counters.TargetPositionRaw,
                        "Single Axis qualification Move prepared");
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.FromResult(0);
                });
            ValidateAxisQualificationIdentityCached(
                identity,
                identity.ConnectionOwner,
                currentAxis,
                "Single Axis qualification Move Relative result");
            ClearMotionOnConfirmedRejection(
                currentAxis.AxisName,
                "Single Axis qualification Move Relative",
                response);
            EnsureResponseSuccess(
                "Single Axis qualification Move Relative",
                response);
            CheckpointAxisQualificationMoveAccepted(
                identity,
                currentAxis,
                "Single Axis qualification Move accepted");
            WriteQualificationLog(
                "event=AXIS_MOVE_RELATIVE_ACK",
                "command=0x20A0",
                "commandCount=1",
                "automaticReplayCount=0",
                "status=0x" + response.Status.ToString("X4"),
                "errorId=" + response.ErrorId,
                "trackingGeneration=" + counters.MotionTrackingGeneration);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task WaitForAxisQualificationMotionAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationCounters counters,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var stableSamples = 0;
            while (stopwatch.ElapsedMilliseconds
                < AxisQualificationMotionTimeoutMilliseconds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (counters.MotionTrackingGeneration <= 0
                    || !IsTrackedMotion(
                        identity.AxisName,
                        counters.MotionTrackingGeneration))
                {
                    throw new InvalidOperationException(
                        "Single Axis qualification motion tracking was cleared before completion proof.");
                }

                var status = await ReadAxisQualificationStatusAsync(
                    identity,
                    currentAxis,
                    counters,
                    "motion-monitor",
                    cancellationToken);
                if (!status.IsSuccess
                    || !status.IsPowerOn
                    || !status.IsReferenced)
                {
                    throw new InvalidOperationException(
                        "Motion monitor requires successful, error-free PowerOn + Referenced status. State=0x"
                        + status.State.ToString("X8")
                        + ", AxisError=0x"
                        + status.AxisErrorId.ToString("X4")
                        + ".");
                }

                if (!status.IsStandstill)
                {
                    counters.MotionObservedSamples++;
                    stableSamples = 0;
                    var motionObservedTestHook =
                        AxisQualificationMotionObservedTestHook;
                    if (motionObservedTestHook != null)
                    {
                        motionObservedTestHook();
                    }
                }
                else if (counters.MotionObservedSamples > 0)
                {
                    stableSamples++;
                    counters.MotionStableSamples = stableSamples;
                    if (stableSamples >= 3)
                    {
                        WriteQualificationLog(
                            "event=AXIS_MOTION_STABLE_PROOF",
                            "motionObservedSamples="
                                + counters.MotionObservedSamples,
                            "stableStandstillSamples=" + stableSamples,
                            "statusReads=" + counters.StatusReads,
                            "state=0x" + status.State.ToString("X8"));
                        return;
                    }
                }

                await Task.Delay(50, cancellationToken);
            }

            throw new TimeoutException(
                "Single Axis qualification did not observe both non-Standstill motion and three later stable Standstill samples within "
                + AxisQualificationMotionTimeoutMilliseconds
                + " ms.");
        }

        private async Task VerifyAxisQualificationFinalPositionAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationInput input,
            AxisQualificationCounters counters,
            CancellationToken cancellationToken)
        {
            var minimum = int.MaxValue;
            var maximum = int.MinValue;
            var finalPosition = 0;
            for (var sample = 0;
                sample < AxisQualificationFinalPositionSamples;
                sample++)
            {
                if (sample > 0)
                {
                    await Task.Delay(50, cancellationToken);
                }
                finalPosition = await ReadAxisQualificationPositionAsync(
                    identity,
                    currentAxis,
                    counters,
                    "final-" + (sample + 1),
                    cancellationToken);
                minimum = Math.Min(minimum, finalPosition);
                maximum = Math.Max(maximum, finalPosition);
                if (Math.Abs(
                        (long)finalPosition - counters.TargetPositionRaw)
                    > input.ToleranceRaw)
                {
                    throw new InvalidOperationException(
                        "Final position sample "
                        + (sample + 1)
                        + " is outside the target tolerance. Target="
                        + counters.TargetPositionRaw
                        + ", Actual="
                        + finalPosition
                        + ", Tolerance="
                        + input.ToleranceRaw
                        + ".");
                }
            }

            if ((long)maximum - minimum > input.ToleranceRaw)
            {
                throw new InvalidOperationException(
                    "The three final position samples are not stable within the configured tolerance. Minimum="
                    + minimum
                    + ", Maximum="
                    + maximum
                    + ", Tolerance="
                    + input.ToleranceRaw
                    + ".");
            }
            counters.FinalPositionRaw = finalPosition;
            WriteQualificationLog(
                "event=AXIS_FINAL_POSITION_PROOF",
                "sampleCount=" + AxisQualificationFinalPositionSamples,
                "targetRaw=" + counters.TargetPositionRaw,
                "minimumRaw=" + minimum,
                "maximumRaw=" + maximum,
                "finalRaw=" + finalPosition,
                "toleranceRaw=" + input.ToleranceRaw);
        }

        private async Task ExecuteAxisQualificationStopAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationInput input,
            AxisQualificationCounters counters,
            long expectedSafetyGeneration)
        {
            AxisStopDispatchPreparation preparation = null;
            LMCAxisStopWaitResult result = null;
            var statusReadsBefore = counters.StatusReads;
            try
            {
                result = await SendQualificationCleanupCommandAsync(
                    "Single Axis qualification Stop",
                    async () =>
                    {
                        EnsureNoNewSafetyRequest(
                            expectedSafetyGeneration,
                            "Single Axis qualification Stop cleanup ownership");
                        await RefreshAndValidateAxisQualificationIdentityAsync(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Stop preflight");
                        await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                            expectedSafetyGeneration,
                            MotionUncertaintyTargetKind.Axis,
                            identity.AxisName,
                            identity.AxisReference,
                            "Single Axis qualification Stop recovery");
                        ValidateAxisQualificationIdentityCached(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Stop final pre-wire identity");
                        var options = new LMCAxisStopWaitOptions
                        {
                            StableSampleCount = 3
                        };
                        preparation = await PrepareAxisStopBeforeDispatchAsync(
                            currentAxis,
                            input.DecelerationRaw,
                            input.JerkRaw,
                            options);
                        counters.StopCommands++;
                        var stop = await currentAxis
                            .StopAndWaitForStableStandstillAsync(
                                input.DecelerationRaw,
                                input.JerkRaw,
                                options,
                                accepted =>
                                {
                                    RunAxisQualificationAcceptedObserver(
                                        () =>
                                        {
                                            PersistAxisStopAccepted(
                                                accepted,
                                                preparation.Record);
                                            SupersedePendingGroupResetByMemberAxisMutation(
                                                currentAxis,
                                                "Accepted captured-member Single Axis qualification Stop");
                                            RecordMotionRecoverySafetyCommandAccepted(
                                                expectedSafetyGeneration,
                                                MotionUncertaintyTargetKind.Axis,
                                                identity.AxisName,
                                                identity.AxisReference,
                                                "Single Axis qualification Stop");
                                        });
                                },
                                CancellationToken.None);
                        counters.StatusReads += stop.StatusPollCount;
                        await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                            currentAxis,
                            preparation.Record,
                            "Single Axis qualification Stop final identity");
                        ValidateAxisQualificationIdentityCached(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Stop final captured identity");
                        return stop;
                    });
                counters.StopStableProven = result.FinalStatus != null
                    && result.FinalStatus.IsSuccess
                    && result.FinalStatus.IsStandstill
                    && result.StableStandstillSampleCount
                        >= result.RequiredStableSampleCount;
                await ClearMotionWarningAfterVerifiedStateAsync(
                    "Single Axis qualification Stop stable standstill proof",
                    null,
                    () => ResolveAxisCommandAfterStableProof(
                        preparation.Record,
                        AxisCommandRecoveryOperation.Stop,
                        "Single Axis qualification Stop stable proof"),
                    () => ValidateAxisQualificationIdentityCached(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification Stop motion-journal final identity"));
                WriteQualificationLog(
                    "event=AXIS_STOP_PROOF",
                    "command=0x2022",
                    "commandCount=1",
                    "automaticReplayCount=0",
                    "statusReads=" + result.StatusPollCount,
                    "stable=" + result.StableStandstillSampleCount
                        + "/" + result.RequiredStableSampleCount,
                    "state=0x" + result.FinalStatus.State.ToString("X8"),
                    "inMotionStopProof=NOT_CLAIMED");
            }
            catch (Exception error)
            {
                var evidence = GetAxisStopWaitEvidence(error);
                if (counters.StatusReads == statusReadsBefore
                    && evidence != null)
                {
                    counters.StatusReads += evidence.StatusPollCount;
                }
                if (preparation != null)
                {
                    await PreserveAxisCommandDispatchFailureAsync(
                        error,
                        preparation,
                        null,
                        currentAxis);
                }
                throw;
            }
        }

        private async Task ExecuteAxisQualificationPowerOffAsync(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            AxisQualificationCounters counters,
            long expectedSafetyGeneration)
        {
            var recoveryRecord = GetActiveAxisPowerRecoveryRecord();
            var powerOnToPowerOffTakeover = recoveryRecord != null
                && recoveryRecord.ExpectedPowerOn;
            var priorPowerOff = currentAxis.PendingPowerOffWaitContinuation;
            var verificationRecord = recoveryRecord;
            var dispatchStarted = false;
            var statusReadsBefore = counters.StatusReads;
            AxisCommandRecoveryRecord stopResolvedByPowerOff = null;
            LMCAxisPowerOffWaitResult result = null;
            try
            {
                result = await SendQualificationCleanupCommandAsync(
                    "Single Axis qualification Power Off",
                    async () =>
                    {
                        EnsureNoNewSafetyRequest(
                            expectedSafetyGeneration,
                            "Single Axis qualification Power Off cleanup ownership");
                        await RefreshAndValidateAxisQualificationIdentityAsync(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Power Off preflight");
                        await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                            expectedSafetyGeneration,
                            MotionUncertaintyTargetKind.Axis,
                            identity.AxisName,
                            identity.AxisReference,
                            "Single Axis qualification Power Off recovery");
                        ValidateAxisQualificationIdentityCached(
                            identity,
                            identity.ConnectionOwner,
                            currentAxis,
                            "Single Axis qualification Power Off final pre-wire identity");
                        verificationRecord = await
                            PrepareAxisPowerOffBeforeDispatchAsync(
                                currentAxis,
                                recoveryRecord,
                                false,
                                true,
                                "Single Axis qualification Power Off");
                        dispatchStarted = true;
                        counters.PowerOffCommands++;
                        var powerOff = await currentAxis
                            .PowerOffAndWaitForStableStateAsync(
                                new LMCAxisPowerStateWaitOptions
                                {
                                    StableSampleCount = 3
                                },
                                accepted =>
                                {
                                    RunAxisQualificationAcceptedObserver(
                                        () =>
                                        {
                                            MarkAxisPowerOffAcceptedForRecord(
                                                currentAxis,
                                                accepted,
                                                verificationRecord,
                                                "Single Axis qualification Power Off accepted");
                                            SupersedePendingGroupResetByMemberAxisMutation(
                                                currentAxis,
                                                "Accepted captured-member Single Axis qualification Power Off");
                                            RecordMotionRecoverySafetyCommandAccepted(
                                                expectedSafetyGeneration,
                                                MotionUncertaintyTargetKind.Axis,
                                                identity.AxisName,
                                                identity.AxisReference,
                                                "Single Axis qualification Power Off");
                                        });
                                },
                                CancellationToken.None);
                        counters.StatusReads += powerOff.StatusPollCount;
                        await CompleteAxisPowerRecoveryAfterStableProofAsync(
                            currentAxis,
                            false,
                            powerOff.FinalStatus,
                            powerOff.StablePowerOffStandstillSampleCount,
                            powerOff.RequiredStableSampleCount,
                            verificationRecord,
                            "Single Axis qualification Power Off stable proof",
                            () => ValidateAxisQualificationIdentityCached(
                                identity,
                                identity.ConnectionOwner,
                                currentAxis,
                                "Single Axis qualification Power Off final identity"));
                        counters.PowerOffStableProven = true;
                        stopResolvedByPowerOff = await
                            PrepareAxisCommandStopAfterStablePowerOffAsync(
                                currentAxis,
                                powerOff.Continuation,
                                powerOff.FinalStatus,
                                powerOff.StablePowerOffStandstillSampleCount,
                            powerOff.RequiredStableSampleCount,
                            "Single Axis qualification Power Off stable proof");
                        return powerOff;
                    });
                await ClearMotionWarningAfterVerifiedStateAsync(
                    "Single Axis qualification stable Power Off proof",
                    null,
                    () =>
                    {
                        if (stopResolvedByPowerOff != null)
                        {
                            ResolveAxisCommandAfterStableProof(
                                stopResolvedByPowerOff,
                                AxisCommandRecoveryOperation.Stop,
                                "Single Axis qualification Power Off retired Stop");
                        }
                    },
                    () => ValidateAxisQualificationIdentityCached(
                        identity,
                        identity.ConnectionOwner,
                        currentAxis,
                        "Single Axis qualification Power Off motion-journal final identity"));
                WriteQualificationLog(
                    "event=AXIS_POWER_OFF_PROOF",
                    "command=0x2023",
                    "commandCount=1",
                    "automaticReplayCount=0",
                    "statusReads=" + result.StatusPollCount,
                    "stable="
                        + result.StablePowerOffStandstillSampleCount
                        + "/" + result.RequiredStableSampleCount,
                    "state=0x" + result.FinalStatus.State.ToString("X8"),
                    "safeState=POWER_OFF_STANDSTILL");
            }
            catch (Exception error)
            {
                var evidence = GetAxisPowerOffWaitEvidence(error);
                if (counters.StatusReads == statusReadsBefore
                    && evidence != null)
                {
                    counters.StatusReads += evidence.StatusPollCount;
                }
                if (verificationRecord != null)
                {
                    PreserveAxisPowerOffWaitFailure(
                        currentAxis,
                        error,
                        verificationRecord,
                        powerOnToPowerOffTakeover,
                        false,
                        dispatchStarted,
                        priorPowerOff,
                        "Single Axis qualification Power Off");
                }
                throw;
            }
        }

        private void WriteAxisQualificationCounts(
            AxisQualificationCounters counters,
            Exception primaryError,
            Exception stopError,
            Exception powerOffError)
        {
            WriteQualificationLog(
                "event=AXIS_COMMAND_COUNTS",
                "powerOn2023=" + counters.PowerOnCommands,
                "moveRelative20A0=" + counters.MoveRelativeCommands,
                "moveAbsolute209F=0",
                "stop2022=" + counters.StopCommands,
                "powerOff2023=" + counters.PowerOffCommands,
                "status2028=" + counters.StatusReads,
                "position202E=" + counters.PositionReads,
                "moveReplayCount=0",
                "startRaw=" + counters.StartPositionRaw,
                "targetRaw=" + counters.TargetPositionRaw,
                "finalRaw=" + counters.FinalPositionRaw,
                "motionObservedSamples=" + counters.MotionObservedSamples,
                "motionStableSamples=" + counters.MotionStableSamples,
                "stopStableProven=" + counters.StopStableProven,
                "powerOffStableProven=" + counters.PowerOffStableProven,
                "primary=" + AxisQualificationErrorName(primaryError),
                "stop=" + AxisQualificationErrorName(stopError),
                "powerOff=" + AxisQualificationErrorName(powerOffError));
        }

        private static string AxisQualificationErrorName(Exception error)
        {
            return error == null ? "none" : error.GetType().Name;
        }

        private void RunAxisQualificationAcceptedObserver(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (Dispatcher.CheckAccess())
            {
                action();
                return;
            }
            Dispatcher.Invoke(action);
        }

        private static Exception CombineAxisQualificationErrors(
            Exception primaryError,
            Exception stopError,
            Exception powerOffError)
        {
            var errors = new List<Exception>();
            if (primaryError != null)
            {
                errors.Add(primaryError);
            }
            if (stopError != null)
            {
                errors.Add(stopError);
            }
            if (powerOffError != null)
            {
                errors.Add(powerOffError);
            }
            return errors.Count == 1
                ? errors[0]
                : new AggregateException(errors);
        }

        private enum AxisQualificationExternalSafetyKind
        {
            None,
            Stop,
            PowerOff
        }

        private enum AxisQualificationExternalSafetyState
        {
            None,
            Standstill,
            PowerOffStandstill
        }

        private sealed class AxisQualificationInput
        {
            internal AxisQualificationInput(
                int deltaRaw,
                int velocityRaw,
                int accelerationRaw,
                int decelerationRaw,
                int jerkRaw,
                int toleranceRaw)
            {
                DeltaRaw = deltaRaw;
                VelocityRaw = velocityRaw;
                AccelerationRaw = accelerationRaw;
                DecelerationRaw = decelerationRaw;
                JerkRaw = jerkRaw;
                ToleranceRaw = toleranceRaw;
            }

            internal int DeltaRaw { get; private set; }
            internal int VelocityRaw { get; private set; }
            internal int AccelerationRaw { get; private set; }
            internal int DecelerationRaw { get; private set; }
            internal int JerkRaw { get; private set; }
            internal int ToleranceRaw { get; private set; }
        }

        private sealed class AxisQualificationIdentity
        {
            internal AxisQualificationIdentity(
                LMCConnection connectionOwner,
                long sessionGeneration,
                string endpointIp,
                int endpointPort,
                string axisName,
                ushort axisReference,
                uint diagnosticsBuild,
                uint diagnosticsBootId,
                uint mapRevision)
            {
                ConnectionOwner = connectionOwner;
                SessionGeneration = sessionGeneration;
                EndpointIp = endpointIp;
                EndpointPort = endpointPort;
                AxisName = axisName;
                AxisReference = axisReference;
                DiagnosticsBuild = diagnosticsBuild;
                DiagnosticsBootId = diagnosticsBootId;
                MapRevision = mapRevision;
            }

            internal LMCConnection ConnectionOwner { get; private set; }
            internal long SessionGeneration { get; private set; }
            internal string EndpointIp { get; private set; }
            internal int EndpointPort { get; private set; }
            internal string AxisName { get; private set; }
            internal ushort AxisReference { get; private set; }
            internal uint DiagnosticsBuild { get; private set; }
            internal uint DiagnosticsBootId { get; private set; }
            internal uint MapRevision { get; private set; }
        }

        private sealed class AxisQualificationCounters
        {
            internal bool PowerOnDispatchStarted { get; set; }
            internal bool MoveDispatchStarted { get; set; }
            internal int PowerOnCommands { get; set; }
            internal int MoveRelativeCommands { get; set; }
            internal int StopCommands { get; set; }
            internal int PowerOffCommands { get; set; }
            internal int StatusReads { get; set; }
            internal int PositionReads { get; set; }
            internal int MotionTrackingGeneration { get; set; }
            internal long VerifiedExternalSafetyGeneration { get; set; }
            internal int StartPositionRaw { get; set; }
            internal int TargetPositionRaw { get; set; }
            internal int FinalPositionRaw { get; set; }
            internal int MotionObservedSamples { get; set; }
            internal int MotionStableSamples { get; set; }
            internal bool StopStableProven { get; set; }
            internal bool PowerOffStableProven { get; set; }
        }
    }
}

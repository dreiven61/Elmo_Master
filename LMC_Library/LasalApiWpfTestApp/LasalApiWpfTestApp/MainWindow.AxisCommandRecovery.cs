using System;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private string axisCommandRecoveryJournalDirectoryPath;
        private AxisCommandRecoveryJournal axisCommandRecoveryJournal;
        private string axisCommandRecoveryJournalOpenError;
        private string axisCommandRecoveryJournalRuntimeError;
        private bool axisStopAcceptedRestartRecovery;
        private bool axisResetAcceptedRestartRecovery;
        private bool axisCommandRecoveryRequired;
        private LMCConnection expectedAxisCommandSafetyAbortConnection;
        private Guid expectedAxisCommandSafetyAbortStopIdentity;
        private long expectedAxisCommandSafetyAbortSessionGeneration;
        private bool axisCommandSafetyReconnectOrchestrationActive;

        internal Action<AxisCommandRecoveryRecord>
            AxisCommandBeforeDurableResolveTestHook { get; set; }
        internal Action<AxisCommandRecoveryRecord>
            AxisCommandAcceptedBeforeDurableMarkTestHook { get; set; }
        internal Action<LMCConnection, AxisCommandRecoveryRecord>
            AxisStopBeforeBeginDispatchTestHook { get; set; }
        internal Func<LMCConnection, AxisCommandRecoveryRecord, Task>
            AxisStopBeforeSafetyAbortTestHook { get; set; }
        internal Action<LMCAxisResetWaitContinuation>
            AxisStopAfterSafetyReconnectTestHook { get; set; }
        internal Action<LMCAxisResetWaitContinuation>
            AxisResetAfterStatusPublicationTestHook { get; set; }

        private bool AxisCommandRecoveryJournalUnavailable
        {
            get
            {
                return axisCommandRecoveryJournal == null
                    || !string.IsNullOrEmpty(axisCommandRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(axisCommandRecoveryJournalRuntimeError);
            }
        }

        private bool AxisCommandRecoveryJournalCanArm
        {
            get
            {
                return !AxisCommandRecoveryJournalUnavailable
                    && !axisCommandRecoveryJournal.HasActiveRecord;
            }
        }

        private bool HasActiveAxisCommandRecoveryRecord
        {
            get { return GetActiveAxisCommandRecoveryRecord() != null; }
        }

        private bool HasUnresolvedAxisCommandState()
        {
            return HasActiveAxisCommandRecoveryRecord
                || (pendingAxisStopWaitContinuation != null
                    && pendingAxisStopWaitContinuation.IsPending)
                || (pendingAxisResetWaitContinuation != null
                    && pendingAxisResetWaitContinuation.IsPending);
        }

        private bool HasDurableAcceptedAxisStopSafetyEvidenceForMotionRecovery()
        {
            if (!motionMayBeActive
                || motionTargetKind != MotionUncertaintyTargetKind.Axis
                || motionUncertaintyJournal == null)
            {
                return false;
            }
            var motionRecord = motionUncertaintyJournal.CurrentRecord;
            var stopRecord = GetActiveAxisCommandRecoveryRecord();
            return motionRecord != null
                && motionRecord.IsActive
                && stopRecord != null
                && stopRecord.Operation == AxisCommandRecoveryOperation.Stop
                && stopRecord.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof
                && stopRecord.MatchesPhysicalIdentity(
                    motionRecord.EndpointIp,
                    motionRecord.EndpointPort,
                    motionRecord.TargetName,
                    motionRecord.TargetReference,
                    motionRecord.DiagnosticsBootId,
                    motionRecord.MapRevision);
        }

        private void InitializeAxisCommandRecoveryJournal()
        {
            try
            {
                axisCommandRecoveryJournal =
                    axisCommandRecoveryJournalDirectoryPath == null
                        ? AxisCommandRecoveryJournal.OpenDefault()
                        : AxisCommandRecoveryJournal.Open(
                            axisCommandRecoveryJournalDirectoryPath);
                axisCommandRecoveryJournalOpenError = null;
                axisCommandRecoveryJournalRuntimeError = null;
                TryFinalizeCommittedAxisCommandRetirementAtStartup();
                var record = axisCommandRecoveryJournal.CurrentRecord;
                if (record == null || !record.IsActive)
                {
                    return;
                }

                if (record.State
                    == AxisCommandRecoveryState.ArmedBeforeDispatch)
                {
                    record = axisCommandRecoveryJournal
                        .PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                }
                ApplyRecoveredAxisCommandRecord(record);
            }
            catch (Exception error)
            {
                var journal = axisCommandRecoveryJournal;
                axisCommandRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }
                axisCommandRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                ClearAxisCommandVolatileState();
                WriteLog(
                    "Axis Stop/Reset recovery journal is unavailable. New "
                    + "Reset and Stop are fail-closed; Power Off remains "
                    + "available: "
                    + axisCommandRecoveryJournalOpenError);
            }
        }

        private void DisposeAxisCommandRecoveryJournal()
        {
            var journal = axisCommandRecoveryJournal;
            axisCommandRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private AxisCommandRecoveryRecord GetActiveAxisCommandRecoveryRecord()
        {
            if (axisCommandRecoveryJournal == null)
            {
                return null;
            }
            var record = axisCommandRecoveryJournal.CurrentRecord;
            return record != null && record.IsActive ? record : null;
        }

        private void ApplyRecoveredAxisCommandRecord(
            AxisCommandRecoveryRecord record)
        {
            if (record == null || !record.IsActive)
            {
                return;
            }

            pendingAxisStopWaitContinuation = null;
            pendingAxisResetWaitContinuation = null;
            axisResetWaitInterferenceConfirmed = false;
            axisCommandRecoveryRequired = record.State
                == AxisCommandRecoveryState.RecoveryRequired;
            axisStopAcceptedRestartRecovery = record.Operation
                    == AxisCommandRecoveryOperation.Stop
                && record.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            axisResetAcceptedRestartRecovery = record.Operation
                    == AxisCommandRecoveryOperation.Reset
                && record.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;

            if (TextRemoteIp != null)
            {
                TextRemoteIp.Text = record.EndpointIp;
            }
            if (TextRemotePort != null)
            {
                TextRemotePort.Text = record.EndpointPort.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            if (TextAxisName != null)
            {
                TextAxisName.Text = record.AxisName;
            }
            WriteLog(
                "Recovered durable Axis "
                + record.Operation
                + " state="
                + record.State
                + ", Axis="
                + record.AxisName
                + ", Ref="
                + record.AxisReference
                + ".");
        }

        private async Task<AxisCommandRecoveryRecord>
            PrepareAxisResetBeforeDispatchAsync(
                LMCSingleAxis currentAxis,
                LMCAxisResetWaitOptions options)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var active = GetActiveAxisCommandRecoveryRecord();
            if (active != null)
            {
                if (active.Operation == AxisCommandRecoveryOperation.Stop)
                {
                    throw new InvalidOperationException(
                        "Reset is blocked while durable Axis Stop recovery is active. No 0x2024 was sent.");
                }
                if (active.State
                    != AxisCommandRecoveryState.RecoveryRequired)
                {
                    throw new InvalidOperationException(
                        "Accepted Axis Reset must use status-only verification; 0x2024 replay is blocked.");
                }
                await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                    currentAxis,
                    active,
                    "Axis Reset explicit retry");
                if (!active.MatchesOperation(
                    AxisCommandRecoveryOperation.Reset,
                    0,
                    0,
                    options.StableSampleCount))
                {
                    throw new InvalidOperationException(
                        "Axis Reset retry options do not match the durable record.");
                }
                return active;
            }

            EnsureAxisCommandRecoveryJournalCanArm("Axis Reset");
            var currentConnection = RequireConnection();
            if (diagnosticCapabilities == null
                || diagnosticCapabilities.DiagnosticsBootId == 0
                || diagnosticCapabilities.MapRevision == 0)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableAxisCommandRecoveryIdentity(
                "Axis Reset");
            try
            {
                return axisCommandRecoveryJournal.ArmBeforeDispatch(
                    AxisCommandRecoveryOperation.Reset,
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    0,
                    0,
                    options.StableSampleCount,
                    DateTime.UtcNow);
            }
            catch (Exception error)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "arm-before-Reset",
                    error);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Reset",
                    error);
            }
        }

        private async Task<AxisStopDispatchPreparation>
            PrepareAxisStopBeforeDispatchAsync(
                LMCSingleAxis currentAxis,
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var currentConnection = RequireConnection();
            if (diagnosticCapabilities == null
                || diagnosticCapabilities.DiagnosticsBootId == 0
                || diagnosticCapabilities.MapRevision == 0)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableAxisCommandRecoveryIdentity(
                "Axis Stop");
            var active = GetActiveAxisCommandRecoveryRecord();
            if (active != null)
            {
                if (!active.MatchesPhysicalIdentity(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision))
                {
                    throw new InvalidOperationException(
                        "Axis Stop target does not match the durable recovery identity. No 0x2022 was sent.");
                }

                if (active.Operation == AxisCommandRecoveryOperation.Stop)
                {
                    if (active.State
                        != AxisCommandRecoveryState.RecoveryRequired)
                    {
                        throw new InvalidOperationException(
                            "Accepted Axis Stop must use status-only verification; 0x2022 replay is blocked.");
                    }
                    if (!active.MatchesOperation(
                        AxisCommandRecoveryOperation.Stop,
                        deceleration,
                        jerk,
                        options.StableSampleCount))
                    {
                        throw new InvalidOperationException(
                            "Axis Stop retry parameters do not match the durable record.");
                    }
                    return new AxisStopDispatchPreparation(
                        active,
                        null,
                        null,
                        true);
                }

                var resetSnapshot = active.Copy();
                var resetContinuation = pendingAxisResetWaitContinuation;
                if (resetContinuation != null
                    && (!string.Equals(
                            resetContinuation.AxisName,
                            currentAxis.AxisName,
                            StringComparison.Ordinal)
                        || resetContinuation.AxisReference
                            != currentAxis.AxisReference))
                {
                    resetContinuation = null;
                }
                try
                {
                    var stop = axisCommandRecoveryJournal
                        .ReplaceActiveResetWithStopBeforeDispatch(
                            resetSnapshot.Identity,
                            RequiredConnectedRemoteIp(),
                            RequiredConnectedRemotePort(),
                            currentAxis.AxisName,
                            currentAxis.AxisReference,
                            capabilities.DiagnosticsBootId,
                            capabilities.MapRevision,
                            deceleration,
                            jerk,
                            options.StableSampleCount,
                            MonotonicUtcNow(resetSnapshot.UpdatedUtc));
                    axisCommandRecoveryRequired = false;
                    axisResetAcceptedRestartRecovery = false;
                    axisStopAcceptedRestartRecovery = false;
                    return new AxisStopDispatchPreparation(
                        stop,
                        resetSnapshot,
                        resetContinuation,
                        false);
                }
                catch (Exception error)
                {
                    SetAxisCommandRecoveryJournalRuntimeError(
                        "replace-Reset-with-Stop",
                        error);
                    throw CreateAxisCommandRecoveryJournalException(
                        "Axis Stop safety takeover",
                        error);
                }
            }

            EnsureAxisCommandRecoveryJournalCanArm("Axis Stop");
            try
            {
                var armed = axisCommandRecoveryJournal.ArmBeforeDispatch(
                    AxisCommandRecoveryOperation.Stop,
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    deceleration,
                    jerk,
                    options.StableSampleCount,
                    DateTime.UtcNow);
                return new AxisStopDispatchPreparation(
                    armed,
                    null,
                    null,
                    false);
            }
            catch (Exception error)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "arm-before-Stop",
                    error);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Stop",
                    error);
            }
        }

        private AxisStopDispatchPreparation
            PrepareAxisStopTakeoverBeforeSafetyAbort(
                LMCSingleAxis currentAxis,
                int deceleration,
                int jerk,
                LMCAxisStopWaitOptions options)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            EnsureAxisCommandRecoveryJournalCanArmForReplacement(
                "Axis Stop safety takeover");
            var reset = GetActiveAxisCommandRecoveryRecord();
            if (reset == null
                || reset.Operation != AxisCommandRecoveryOperation.Reset
                || !reset.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort())
                || !string.Equals(
                    reset.AxisName,
                    currentAxis.AxisName,
                    StringComparison.Ordinal)
                || reset.AxisReference != currentAxis.AxisReference)
            {
                throw new InvalidOperationException(
                    "Axis Stop safety takeover requires the exact durable Reset endpoint, name, and reference before aborting transport.");
            }

            var resetContinuation = pendingAxisResetWaitContinuation;
            if (resetContinuation != null
                && (!string.Equals(
                        resetContinuation.AxisName,
                        reset.AxisName,
                        StringComparison.Ordinal)
                    || resetContinuation.AxisReference != reset.AxisReference))
            {
                resetContinuation = null;
            }
            var abortConnection = RequireConnection();
            var expectedAbortSessionGeneration = resetContinuation == null
                ? abortConnection.SessionGeneration
                : resetContinuation.SessionGeneration;
            if (expectedAbortSessionGeneration <= 0)
            {
                throw new InvalidOperationException(
                    "Axis Stop safety takeover requires a positive pinned Reset transport session generation.");
            }
            try
            {
                var stop = axisCommandRecoveryJournal
                    .ReplaceActiveResetWithStopBeforeDispatch(
                        reset.Identity,
                        reset.EndpointIp,
                        reset.EndpointPort,
                        reset.AxisName,
                        reset.AxisReference,
                        reset.DiagnosticsBootId,
                        reset.MapRevision,
                        deceleration,
                        jerk,
                        options.StableSampleCount,
                        MonotonicUtcNow(reset.UpdatedUtc));
                var preparation = new AxisStopDispatchPreparation(
                    stop,
                    reset,
                    resetContinuation,
                    false);
                preparation.ExpectedAbortSessionGeneration =
                    expectedAbortSessionGeneration;
                return preparation;
            }
            catch (Exception error)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "arm-Stop-before-safety-abort",
                    error);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Stop safety takeover",
                    error);
            }
        }

        private async Task<LMCSingleAxis>
            AbortReconnectAndReloadAxisForStopTakeoverAsync(
                LMCConnection currentConnection,
                AxisStopDispatchPreparation preparation,
                long expectedSafetyGeneration)
        {
            if (currentConnection == null
                || preparation == null
                || preparation.ReplacedResetRecord == null)
            {
                throw new InvalidOperationException(
                    "Axis Stop safety reconnect requires an exact Reset predecessor.");
            }

            if (preparation.ResetContinuation != null
                && preparation.ResetContinuation.State
                    == LMCAxisResetWaitContinuationState.Completed)
            {
                var currentAxis = RequireAxis();
                if (!string.Equals(
                        currentAxis.AxisName,
                        preparation.Record.AxisName,
                        StringComparison.Ordinal)
                    || currentAxis.AxisReference
                        != preparation.Record.AxisReference)
                {
                    throw new InvalidOperationException(
                        "Axis Stop completed-Reset race does not match the durable axis identity.");
                }
                await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                    expectedSafetyGeneration,
                    MotionUncertaintyTargetKind.Axis,
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    "Axis Stop completed-Reset recovery");
                return currentAxis;
            }

            expectedAxisCommandSafetyAbortConnection = currentConnection;
            expectedAxisCommandSafetyAbortStopIdentity =
                preparation.Record.Identity;
            expectedAxisCommandSafetyAbortSessionGeneration =
                preparation.ExpectedAbortSessionGeneration;
            axisCommandSafetyReconnectOrchestrationActive = true;
            LMCSafetyPreemptionAbortEvidence abortEvidence;
            try
            {
                var beforeAbort = AxisStopBeforeSafetyAbortTestHook;
                if (beforeAbort != null)
                {
                    await beforeAbort(
                        currentConnection,
                        preparation.Record);
                }
                abortEvidence = currentConnection
                    .AbortTransportForSafetyPreemption(
                        preparation.ExpectedAbortSessionGeneration);
            }
            catch (LMCSafetyPreemptionSessionMismatchException)
            {
                preparation.ResetSessionInvalidated = true;
                ClearLoadedObjects();
                UpdateUiState();
                throw;
            }
            preparation.ResetSessionInvalidated = true;
            if (expectedAxisCommandSafetyAbortSessionGeneration == 0)
            {
                expectedAxisCommandSafetyAbortSessionGeneration =
                    abortEvidence.SessionGeneration;
            }
            else if (expectedAxisCommandSafetyAbortSessionGeneration
                != abortEvidence.SessionGeneration)
            {
                throw new InvalidOperationException(
                    "Axis Stop safety-abort session evidence changed unexpectedly.");
            }

            DetachConnection(currentConnection);
            if (ReferenceEquals(connection, currentConnection))
            {
                connection = null;
            }
            ClearLoadedObjects();
            currentConnection.Dispose();

            LMCConnection replacementConnection = null;
            try
            {
                replacementConnection = CreateCoordinatedConnection();
                preparation.ReplacementConnection = replacementConnection;
                connection = replacementConnection;
                UpdateUiState();
                await replacementConnection.RpcInitConnectionAsync(
                    preparation.Record.EndpointIp,
                    preparation.Record.EndpointPort,
                    RequiredText(TextLocalIp.Text, "PC local IPv4"),
                    ParsePort(
                        TextCallbackPort.Text,
                        "Callback UDP port",
                        true),
                    LMCConnection.DefaultEventMask,
                    System.Threading.CancellationToken.None);
                RememberConnectedRemoteEndpoint(
                    preparation.Record.EndpointIp,
                    preparation.Record.EndpointPort);
                await RefreshDiagnosticsCapabilitiesAsync(
                    replacementConnection);
                var capabilities = RequireStableAxisCommandRecoveryIdentity(
                    "Axis Stop safety reconnect");
                if (!preparation.Record.MatchesEndpoint(
                        RequiredConnectedRemoteIp(),
                        RequiredConnectedRemotePort())
                    || preparation.Record.DiagnosticsBootId
                        != capabilities.DiagnosticsBootId
                    || preparation.Record.MapRevision
                        != capabilities.MapRevision)
                {
                    throw new InvalidOperationException(
                        "Axis Stop safety reconnect BootId or MapRevision does not match the durable Stop record.");
                }

                var reloaded = await LMCSingleAxis.CreateAsync(
                    replacementConnection,
                    preparation.Record.AxisName,
                    System.Threading.CancellationToken.None);
                if (!string.Equals(
                        reloaded.AxisName,
                        preparation.Record.AxisName,
                        StringComparison.Ordinal)
                    || reloaded.AxisReference
                        != preparation.Record.AxisReference)
                {
                    throw new InvalidOperationException(
                        "Axis Stop safety reconnect lookup does not match the durable axis reference.");
                }

                axis = reloaded;
                TextAxisReference.Text = reloaded.AxisReference.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
                RememberMotionLookupIdentity(
                    MotionUncertaintyTargetKind.Axis,
                    reloaded.AxisName,
                    reloaded.AxisReference);

                await EnsureMotionRecoverySafetyDispatchIdentityAsync(
                    expectedSafetyGeneration,
                    MotionUncertaintyTargetKind.Axis,
                    reloaded.AxisName,
                    reloaded.AxisReference,
                    "Axis Stop safety-reconnect recovery");

                var afterReconnect =
                    AxisStopAfterSafetyReconnectTestHook;
                if (afterReconnect != null)
                {
                    afterReconnect(preparation.ResetContinuation);
                }

                return reloaded;
            }
            catch
            {
                if (ReferenceEquals(connection, replacementConnection))
                {
                    connection = null;
                }
                if (replacementConnection != null)
                {
                    if (preparation.ReplacementConnectionAttached)
                    {
                        DetachConnection(replacementConnection);
                    }
                    replacementConnection.Dispose();
                }
                ClearLoadedObjects();
                UpdateUiState();
                await HandleAxisStopDefinitelyNotAttemptedAsync(
                    preparation,
                    null);
                throw;
            }
        }

        private void CompleteAxisStopSafetyReplacementConnectionSetup(
            AxisStopDispatchPreparation preparation)
        {
            if (preparation == null
                || preparation.ReplacementConnection == null
                || preparation.ReplacementConnectionAttached)
            {
                return;
            }

            var replacement = preparation.ReplacementConnection;
            if (!ReferenceEquals(connection, replacement)
                || !replacement.IsConnected)
            {
                if (ReferenceEquals(connection, replacement))
                {
                    connection = null;
                }
                replacement.Dispose();
                ClearLoadedObjects();
                UpdateUiState();
                return;
            }

            AttachConnection(replacement);
            preparation.ReplacementConnectionAttached = true;
            UpdateUiState();
        }

        private bool ShouldSuppressAxisCommandSafetyAbortPromotion(
            LMCConnection eventConnection,
            Exception exception)
        {
            var current = GetActiveAxisCommandRecoveryRecord();
            if (!axisCommandSafetyReconnectOrchestrationActive
                || !ReferenceEquals(
                    eventConnection,
                    expectedAxisCommandSafetyAbortConnection)
                || current == null
                || current.Identity
                    != expectedAxisCommandSafetyAbortStopIdentity
                || current.Operation != AxisCommandRecoveryOperation.Stop
                || current.State
                    != AxisCommandRecoveryState.ArmedBeforeDispatch)
            {
                return false;
            }
            var aborted = exception as
                LMCSafetyPreemptionTransportAbortedException;
            if (aborted == null)
            {
                return true;
            }
            if (expectedAxisCommandSafetyAbortSessionGeneration == 0)
            {
                expectedAxisCommandSafetyAbortSessionGeneration =
                    aborted.SessionGeneration;
            }
            return expectedAxisCommandSafetyAbortSessionGeneration
                == aborted.SessionGeneration;
        }

        private void EndAxisCommandSafetyReconnectOrchestration()
        {
            axisCommandSafetyReconnectOrchestrationActive = false;
            expectedAxisCommandSafetyAbortConnection = null;
            expectedAxisCommandSafetyAbortStopIdentity = Guid.Empty;
            expectedAxisCommandSafetyAbortSessionGeneration = 0;
        }

        private void PreserveAxisCommandRecoveryAfterConnectionLoss(
            LMCConnection eventConnection,
            Exception exception,
            string reason)
        {
            var suppressPromotion =
                ShouldSuppressAxisCommandSafetyAbortPromotion(
                    eventConnection,
                    exception);
            var current = GetActiveAxisCommandRecoveryRecord();
            pendingAxisStopWaitContinuation = null;
            pendingAxisResetWaitContinuation = null;
            axisResetWaitInterferenceConfirmed = false;
            if (current == null)
            {
                return;
            }
            if (!suppressPromotion
                && current.State
                    == AxisCommandRecoveryState.ArmedBeforeDispatch)
            {
                try
                {
                    current = axisCommandRecoveryJournal
                        .PromoteToRecoveryRequired(
                            current.Identity,
                            MonotonicUtcNow(current.UpdatedUtc));
                }
                catch (Exception error)
                {
                    SetAxisCommandRecoveryJournalRuntimeError(
                        "connection-loss transition",
                        error);
                    return;
                }
            }
            axisCommandRecoveryRequired = current.State
                == AxisCommandRecoveryState.RecoveryRequired;
            axisStopAcceptedRestartRecovery = current.Operation
                    == AxisCommandRecoveryOperation.Stop
                && current.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            axisResetAcceptedRestartRecovery = current.Operation
                    == AxisCommandRecoveryOperation.Reset
                && current.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            WriteLog(
                reason
                + (suppressPromotion
                    ? " preserved the exact Armed Stop during expected safety reconnect."
                    : " preserved durable Axis Stop/Reset recovery."));
        }

        private void PersistAxisResetAccepted(
            LMCAxisResetWaitContinuation continuation,
            AxisCommandRecoveryRecord verificationRecord)
        {
            if (continuation == null
                || !continuation.IsPending
                || verificationRecord == null
                || verificationRecord.Operation
                    != AxisCommandRecoveryOperation.Reset
                || !string.Equals(
                    continuation.AxisName,
                    verificationRecord.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference != verificationRecord.AxisReference
                || continuation.RequiredStableSampleCount
                    != verificationRecord.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    "Accepted Axis Reset does not match its durable identity.");
            }
            PersistAxisCommandAccepted(verificationRecord, "Axis Reset");
            pendingAxisResetWaitContinuation = continuation;
            pendingAxisStopWaitContinuation = null;
            axisResetAcceptedRestartRecovery = false;
            axisStopAcceptedRestartRecovery = false;
            axisCommandRecoveryRequired = false;
            axisResetWaitInterferenceConfirmed = false;
        }

        private void PersistAxisStopAccepted(
            LMCAxisStopWaitContinuation continuation,
            AxisCommandRecoveryRecord verificationRecord)
        {
            if (continuation == null
                || !continuation.IsPending
                || verificationRecord == null
                || verificationRecord.Operation
                    != AxisCommandRecoveryOperation.Stop
                || !string.Equals(
                    continuation.AxisName,
                    verificationRecord.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference != verificationRecord.AxisReference
                || continuation.Deceleration
                    != verificationRecord.StopDeceleration
                || continuation.Jerk != verificationRecord.StopJerk
                || continuation.RequiredStableSampleCount
                    != verificationRecord.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    "Accepted Axis Stop does not match its durable identity.");
            }
            PersistAxisCommandAccepted(verificationRecord, "Axis Stop");
            CheckpointAxisQualificationStopAccepted(
                axis,
                "Axis Stop accepted sequence checkpoint");
            pendingAxisStopWaitContinuation = continuation;
            pendingAxisResetWaitContinuation = null;
            axisResetAcceptedRestartRecovery = false;
            axisStopAcceptedRestartRecovery = false;
            axisCommandRecoveryRequired = false;
            axisResetWaitInterferenceConfirmed = false;
            if (expectedAxisCommandSafetyAbortStopIdentity
                == verificationRecord.Identity)
            {
                EndAxisCommandSafetyReconnectOrchestration();
            }
        }

        private void PersistAxisCommandAccepted(
            AxisCommandRecoveryRecord verificationRecord,
            string operation)
        {
            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.Operation != verificationRecord.Operation)
            {
                throw new InvalidOperationException(
                    operation
                    + " accepted callback belongs to an older operation.");
            }
            try
            {
                if (current.State
                        == AxisCommandRecoveryState.ArmedBeforeDispatch
                    || current.State
                        == AxisCommandRecoveryState.RecoveryRequired)
                {
                    var hook = AxisCommandAcceptedBeforeDurableMarkTestHook;
                    if (hook != null)
                    {
                        hook(current.Copy());
                    }
                    axisCommandRecoveryJournal.MarkAccepted(
                        current.Identity,
                        MonotonicUtcNow(current.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "mark-accepted-" + operation,
                    error);
                throw CreateAxisCommandRecoveryJournalException(
                    operation + " accepted observer",
                    error);
            }
        }

        private static LMCAxisResetWaitContinuation
            GetExactPendingAxisResetContinuation(
                LMCSingleAxis currentAxis,
                AxisCommandRecoveryRecord record)
        {
            if (currentAxis == null
                || record == null
                || record.Operation != AxisCommandRecoveryOperation.Reset)
            {
                return null;
            }
            var continuation = currentAxis.PendingResetWaitContinuation;
            if (continuation == null
                || !continuation.IsPending
                || !string.Equals(
                    continuation.AxisName,
                    record.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference != record.AxisReference
                || continuation.RequiredStableSampleCount
                    != record.RequiredStableSampleCount)
            {
                return null;
            }
            return continuation;
        }

        private static LMCAxisStopWaitContinuation
            GetExactPendingAxisStopContinuation(
                LMCSingleAxis currentAxis,
                AxisCommandRecoveryRecord record)
        {
            if (currentAxis == null
                || record == null
                || record.Operation != AxisCommandRecoveryOperation.Stop)
            {
                return null;
            }
            var continuation = currentAxis.PendingStopWaitContinuation;
            if (continuation == null
                || !continuation.IsPending
                || !string.Equals(
                    continuation.AxisName,
                    record.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference != record.AxisReference
                || continuation.Deceleration != record.StopDeceleration
                || continuation.Jerk != record.StopJerk
                || continuation.RequiredStableSampleCount
                    != record.RequiredStableSampleCount)
            {
                return null;
            }
            return continuation;
        }

        private async Task PreserveAxisCommandDispatchFailureAsync(
            Exception error,
            AxisStopDispatchPreparation stopPreparation,
            AxisCommandRecoveryRecord resetRecord,
            LMCSingleAxis currentAxis)
        {
            var record = stopPreparation == null
                ? resetRecord
                : stopPreparation.Record;
            if (record == null)
            {
                return;
            }
            if (stopPreparation != null
                && IsAxisStopDefinitelyNotAttempted(error))
            {
                await HandleAxisStopDefinitelyNotAttemptedAsync(
                    stopPreparation,
                    currentAxis);
                return;
            }
            if (stopPreparation == null
                && IsAxisResetDefinitelyNotAttempted(error))
            {
                HandleAxisResetDefinitelyNotAttempted(record);
                return;
            }

            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null || current.Identity != record.Identity)
            {
                return;
            }
            if (current.State == AxisCommandRecoveryState.ArmedBeforeDispatch)
            {
                try
                {
                    axisCommandRecoveryJournal.PromoteToRecoveryRequired(
                        current.Identity,
                        MonotonicUtcNow(current.UpdatedUtc));
                    axisCommandRecoveryRequired = true;
                    axisStopAcceptedRestartRecovery = false;
                    axisResetAcceptedRestartRecovery = false;
                }
                catch (Exception journalError)
                {
                    SetAxisCommandRecoveryJournalRuntimeError(
                        "promote-after-dispatch-failure",
                        journalError);
                }
            }
        }

        private async Task HandleAxisStopDefinitelyNotAttemptedAsync(
            AxisStopDispatchPreparation preparation,
            LMCSingleAxis currentAxis)
        {
            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null
                || current.Identity != preparation.Record.Identity)
            {
                return;
            }
            try
            {
                if (preparation.ReplacedResetRecord != null)
                {
                    var resetContinuation = preparation.ResetContinuation;
                    if (resetContinuation != null
                        && resetContinuation.State
                            == LMCAxisResetWaitContinuationState.Completed)
                    {
                        try
                        {
                            await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                                currentAxis,
                                current,
                                "Axis Stop rejected after completed Reset final identity");
                        }
                        catch (Exception identityError)
                        {
                            PreserveAxisStopAfterCompletedResetIdentityFailure(
                                current,
                                identityError);
                            return;
                        }
                        axisCommandRecoveryJournal.Resolve(
                            current.Identity,
                            MonotonicUtcNow(current.UpdatedUtc));
                        ClearAxisCommandVolatileState();
                        return;
                    }
                    if (preparation.ResetSessionInvalidated
                        || resetContinuation == null
                        || resetContinuation.State
                            == LMCAxisResetWaitContinuationState.Pending)
                    {
                        var restored = axisCommandRecoveryJournal
                            .RestoreResetAfterStopNotAttempted(
                                current.Identity,
                                preparation.ReplacedResetRecord,
                                MonotonicUtcNow(current.UpdatedUtc));
                        axisCommandRecoveryRequired = restored.State
                            == AxisCommandRecoveryState.RecoveryRequired;
                        axisResetAcceptedRestartRecovery =
                            (preparation.ResetSessionInvalidated
                                || resetContinuation == null)
                            && restored.State
                                == AxisCommandRecoveryState
                                    .AcceptedAwaitingProof;
                        axisStopAcceptedRestartRecovery = false;
                        pendingAxisResetWaitContinuation =
                            !preparation.ResetSessionInvalidated
                                && resetContinuation != null
                                && resetContinuation.IsPending
                            ? resetContinuation
                            : null;
                        return;
                    }
                }
                if (!preparation.WasRecoveryRetry)
                {
                    axisCommandRecoveryJournal.Resolve(
                        current.Identity,
                        MonotonicUtcNow(current.UpdatedUtc));
                    ClearAxisCommandVolatileState();
                }
            }
            catch (Exception journalError)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "rollback-not-attempted-Stop",
                    journalError);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Stop not-attempted rollback",
                    journalError);
            }
        }

        private void PreserveAxisStopAfterCompletedResetIdentityFailure(
            AxisCommandRecoveryRecord current,
            Exception identityError)
        {
            try
            {
                if (current.State
                    != AxisCommandRecoveryState.RecoveryRequired)
                {
                    current = axisCommandRecoveryJournal
                        .PromoteToRecoveryRequired(
                            current.Identity,
                            MonotonicUtcNow(current.UpdatedUtc));
                }
                pendingAxisStopWaitContinuation = null;
                pendingAxisResetWaitContinuation = null;
                axisResetWaitInterferenceConfirmed = false;
                axisCommandRecoveryRequired = true;
                axisStopAcceptedRestartRecovery = false;
                axisResetAcceptedRestartRecovery = false;
                WriteLog(
                    "Axis Stop remains RecoveryRequired because the exact final "
                    + "identity check after completed Reset failed: "
                    + identityError.GetType().Name
                    + ": "
                    + identityError.Message);
            }
            catch (Exception journalError)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "preserve-Stop-after-completed-Reset-identity-failure",
                    journalError);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Stop completed-Reset identity failure preservation",
                    journalError);
            }
        }

        private void HandleAxisResetDefinitelyNotAttempted(
            AxisCommandRecoveryRecord record)
        {
            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null
                || current.Identity != record.Identity
                || current.State == AxisCommandRecoveryState.RecoveryRequired)
            {
                return;
            }
            try
            {
                axisCommandRecoveryJournal.Resolve(
                    current.Identity,
                    MonotonicUtcNow(current.UpdatedUtc));
                ClearAxisCommandVolatileState();
            }
            catch (Exception journalError)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "resolve-not-attempted-Reset",
                    journalError);
                throw CreateAxisCommandRecoveryJournalException(
                    "Axis Reset not-attempted resolution",
                    journalError);
            }
        }

        private static bool IsAxisStopDefinitelyNotAttempted(Exception error)
        {
            var evidence = GetAxisStopWaitEvidence(error);
            return evidence != null
                && (evidence.SubmissionOutcome
                        == LMCAxisStopSubmissionOutcome.NotAttempted
                    || evidence.SubmissionOutcome
                        == LMCAxisStopSubmissionOutcome.Rejected);
        }

        private static bool IsAxisResetDefinitelyNotAttempted(Exception error)
        {
            var evidence = GetAxisResetWaitEvidence(error);
            return evidence != null
                && (evidence.SubmissionOutcome
                        == LMCAxisResetSubmissionOutcome.NotAttempted
                    || evidence.SubmissionOutcome
                        == LMCAxisResetSubmissionOutcome.Rejected);
        }

        private async Task EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
            LMCSingleAxis currentAxis,
            AxisCommandRecoveryRecord record,
            string operation,
            bool refreshCapabilities = true)
        {
            if (currentAxis == null || record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation + " requires an active exact Axis record.");
            }
            var currentConnection = RequireConnection();
            if (refreshCapabilities)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableAxisCommandRecoveryIdentity(
                operation);
            if (!record.MatchesPhysicalIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentAxis.AxisName,
                currentAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw new InvalidOperationException(
                    operation
                    + " identity does not match endpoint, axis reference, BootId, or MapRevision.");
            }
        }

        private void PromoteAxisCommandAfterProofInterference(
            AxisCommandRecoveryRecord verificationRecord,
            Exception error)
        {
            if (!(error is LMCAxisResetInterferenceException)
                && !(error is
                    LMCAxisStableErrorClearanceInterferenceException)
                && !(error is LMCAxisStopInterferenceException)
                && !(error is LMCAxisStableStandstillInterferenceException))
            {
                return;
            }
            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.State
                    == AxisCommandRecoveryState.RecoveryRequired)
            {
                return;
            }
            try
            {
                axisCommandRecoveryJournal.PromoteToRecoveryRequired(
                    current.Identity,
                    MonotonicUtcNow(current.UpdatedUtc));
                pendingAxisStopWaitContinuation = null;
                pendingAxisResetWaitContinuation = null;
                axisResetWaitInterferenceConfirmed = true;
                axisCommandRecoveryRequired = true;
                axisStopAcceptedRestartRecovery = false;
                axisResetAcceptedRestartRecovery = false;
            }
            catch (Exception journalError)
            {
                SetAxisCommandRecoveryJournalRuntimeError(
                    "promote-after-proof-interference",
                    journalError);
            }
        }

        private void ResolveAxisCommandAfterStableProof(
            AxisCommandRecoveryRecord verificationRecord,
            AxisCommandRecoveryOperation expectedOperation,
            string operation)
        {
            var current = GetActiveAxisCommandRecoveryRecord();
            if (current == null
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.Operation != expectedOperation)
            {
                throw new InvalidOperationException(
                    operation + " cannot resolve a stale Axis command record.");
            }
            var hook = AxisCommandBeforeDurableResolveTestHook;
            if (hook != null)
            {
                hook(current.Copy());
            }
            try
            {
                if (expectedOperation == AxisCommandRecoveryOperation.Stop)
                {
                    CheckpointAxisQualificationStopStableBeforeChildResolve(
                        axis,
                        operation + " sequence checkpoint");
                }
                axisCommandRecoveryJournal.Resolve(
                    current.Identity,
                    MonotonicUtcNow(current.UpdatedUtc));
                axisCommandRecoveryJournalRuntimeError = null;
                ClearAxisCommandVolatileState();
            }
            catch (Exception error)
            {
                SetAxisCommandRecoveryJournalRuntimeError("resolve", error);
                throw CreateAxisCommandRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private async Task CompleteAxisCommandStopAfterStablePowerOffAsync(
            LMCSingleAxis currentAxis,
            LMCAxisPowerOffWaitContinuation powerOffContinuation,
            LMCReadStatusResult finalStatus,
            int stableSampleCount,
            int requiredStableSampleCount,
            string operation)
        {
            var record = await PrepareAxisCommandStopAfterStablePowerOffAsync(
                currentAxis,
                powerOffContinuation,
                finalStatus,
                stableSampleCount,
                requiredStableSampleCount,
                operation);
            if (record == null)
            {
                return;
            }
            ResolveAxisCommandAfterStableProof(
                record,
                AxisCommandRecoveryOperation.Stop,
                operation);
        }

        private async Task<AxisCommandRecoveryRecord>
            PrepareAxisCommandStopAfterStablePowerOffAsync(
                LMCSingleAxis currentAxis,
                LMCAxisPowerOffWaitContinuation powerOffContinuation,
                LMCReadStatusResult finalStatus,
                int stableSampleCount,
                int requiredStableSampleCount,
                string operation)
        {
            var record = GetActiveAxisCommandRecoveryRecord();
            if (record == null
                || record.Operation != AxisCommandRecoveryOperation.Stop)
            {
                return null;
            }
            if (finalStatus == null
                || !finalStatus.IsSuccess
                || finalStatus.IsPowerOn
                || !finalStatus.IsStandstill
                || stableSampleCount < requiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve Stop without stable PowerOff and Standstill proof.");
            }
            await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                currentAxis,
                record,
                operation + " final identity",
                false);
            var stopContinuation = pendingAxisStopWaitContinuation;
            if (stopContinuation != null
                && stopContinuation.IsPending)
            {
                if (powerOffContinuation != null)
                {
                    if (!currentAxis.TryRetirePendingStopAfterStablePowerOff(
                        stopContinuation,
                        powerOffContinuation))
                    {
                        throw new InvalidOperationException(
                            operation
                            + " could not retire the exact pending Stop continuation.");
                    }
                }
                else
                {
                    var resumedStop = await currentAxis
                        .ResumeStopWaitForStableStandstillAsync(
                            stopContinuation,
                            System.Threading.CancellationToken.None);
                    if (resumedStop.FinalStatus == null
                        || !resumedStop.FinalStatus.IsSuccess
                        || !resumedStop.FinalStatus.IsStandstill
                        || resumedStop.StableStandstillSampleCount
                            < resumedStop.RequiredStableSampleCount)
                    {
                        throw new InvalidOperationException(
                            operation
                            + " did not complete the same-session pending Stop after restart Power Off proof.");
                    }
                    await EnsureCurrentAxisMatchesAxisCommandRecoveryAsync(
                        currentAxis,
                        record,
                        operation + " post-PowerOff Stop proof identity",
                        false);
                }
            }
            return record;
        }

        private void EnsureAxisCommandRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            var record = GetActiveAxisCommandRecoveryRecord();
            if (record != null
                && !record.MatchesEndpoint(endpointIp, endpointPort))
            {
                throw new InvalidOperationException(
                    "Connect endpoint does not match the durable Axis Stop/Reset record.");
            }
        }

        private async Task EnsureAxisCommandRecoveryConnectionIdentityAsync(
            string operation)
        {
            var record = GetActiveAxisCommandRecoveryRecord();
            if (record == null)
            {
                return;
            }
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableAxisCommandRecoveryIdentity(
                operation);
            if (!record.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort())
                || record.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || record.MapRevision != capabilities.MapRevision)
            {
                throw CreateRecoveryConnectionIdentityMismatch(
                    operation,
                    "Axis Stop/Reset",
                    record.DiagnosticsBootId,
                    record.MapRevision,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }
        }

        private void EnsureAxisCommandRecoveryLookupAllowed(string axisName)
        {
            var record = GetActiveAxisCommandRecoveryRecord();
            if (record != null
                && !string.Equals(
                    record.AxisName,
                    axisName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A different axis cannot be loaded during Axis Stop/Reset recovery. No lookup RPC was sent.");
            }
        }

        private void EnsureLoadedAxisMatchesAxisCommandRecovery(
            LMCSingleAxis loadedAxis)
        {
            var record = GetActiveAxisCommandRecoveryRecord();
            if (record == null)
            {
                return;
            }
            var capabilities = RequireStableAxisCommandRecoveryIdentity(
                "Load Axis Stop/Reset recovery");
            if (!record.MatchesPhysicalIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                loadedAxis.AxisName,
                loadedAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw new InvalidOperationException(
                    "Loaded axis does not match the durable Axis Stop/Reset identity.");
            }
        }

        private void EnsureAxisCommandRecoveryJournalCanArm(string operation)
        {
            if (!AxisCommandRecoveryJournalCanArm)
            {
                throw CreateAxisCommandRecoveryJournalException(operation, null);
            }
        }

        private void EnsureAxisCommandRecoveryJournalCanArmForReplacement(
            string operation)
        {
            if (AxisCommandRecoveryJournalUnavailable)
            {
                throw CreateAxisCommandRecoveryJournalException(
                    operation,
                    null);
            }
        }

        private LMCDiagnosticCapabilities RequireStableAxisCommandRecoveryIdentity(
            string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires nonzero DiagnosticsBootId and MapRevision.");
            }
            return capabilities;
        }

        private InvalidOperationException
            CreateAxisCommandRecoveryJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    axisCommandRecoveryJournalRuntimeError)
                ? axisCommandRecoveryJournalRuntimeError
                : axisCommandRecoveryJournalOpenError;
            return new InvalidOperationException(
                operation
                + " is blocked because the durable Axis Stop/Reset journal "
                + "cannot safely preserve the operation"
                + (string.IsNullOrEmpty(detail) ? "." : ": " + detail),
                innerException);
        }

        private void SetAxisCommandRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            axisCommandRecoveryJournalRuntimeError = operation
                + ": "
                + (error == null
                    ? "unknown journal failure"
                    : error.GetType().Name + ": " + error.Message);
        }

        private void ClearAxisCommandVolatileState()
        {
            pendingAxisStopWaitContinuation = null;
            pendingAxisResetWaitContinuation = null;
            axisResetWaitInterferenceConfirmed = false;
            axisStopAcceptedRestartRecovery = false;
            axisResetAcceptedRestartRecovery = false;
            axisCommandRecoveryRequired = false;
        }

        private void InvalidateAxisCommandSessionContinuations()
        {
            pendingAxisStopWaitContinuation = null;
            pendingAxisResetWaitContinuation = null;
            axisResetWaitInterferenceConfirmed = false;
            var record = GetActiveAxisCommandRecoveryRecord();
            axisStopAcceptedRestartRecovery = record != null
                && record.Operation == AxisCommandRecoveryOperation.Stop
                && record.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            axisResetAcceptedRestartRecovery = record != null
                && record.Operation == AxisCommandRecoveryOperation.Reset
                && record.State
                    == AxisCommandRecoveryState.AcceptedAwaitingProof;
            axisCommandRecoveryRequired = record != null
                && record.State == AxisCommandRecoveryState.RecoveryRequired;
        }

        private sealed class AxisStopDispatchPreparation
        {
            internal AxisStopDispatchPreparation(
                AxisCommandRecoveryRecord record,
                AxisCommandRecoveryRecord replacedResetRecord,
                LMCAxisResetWaitContinuation resetContinuation,
                bool wasRecoveryRetry)
            {
                Record = record;
                ReplacedResetRecord = replacedResetRecord;
                ResetContinuation = resetContinuation;
                WasRecoveryRetry = wasRecoveryRetry;
            }

            internal AxisCommandRecoveryRecord Record { get; private set; }
            internal AxisCommandRecoveryRecord ReplacedResetRecord
            {
                get;
                private set;
            }
            internal LMCAxisResetWaitContinuation ResetContinuation
            {
                get;
                private set;
            }
            internal bool WasRecoveryRetry { get; private set; }
            internal bool ResetSessionInvalidated { get; set; }
            internal LMCConnection ReplacementConnection { get; set; }
            internal bool ReplacementConnectionAttached { get; set; }
            internal long ExpectedAbortSessionGeneration { get; set; }
        }
    }
}

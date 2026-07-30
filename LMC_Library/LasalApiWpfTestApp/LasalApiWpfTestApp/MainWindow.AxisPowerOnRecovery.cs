using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private string axisPowerOnRecoveryJournalDirectoryPath;
        private AxisPowerOnRecoveryJournal axisPowerOnRecoveryJournal;
        private string axisPowerOnRecoveryJournalOpenError;
        private string axisPowerOnRecoveryJournalRuntimeError;
        private bool axisPowerOnRecoveryRequired;
        private bool axisPowerOnAcceptedRestartRecovery;
        private bool axisPowerOffReplacementAllowed;
        private bool axisPowerDurabilityDegraded;
        private AxisPowerOnRecoveryRecord axisPowerDegradedSafetyOffRecord;
        private LMCAxisPowerOnWaitContinuation
            pendingAxisPowerOnWaitContinuation;
        private LMCAxisPowerOffWaitContinuation
            pendingAxisPowerOffWaitContinuation;
        internal Action<AxisPowerOnRecoveryRecord>
            AxisPowerAcceptedBeforeDurableMarkTestHook { get; set; }

        private bool AxisPowerOnRecoveryJournalCanArm
        {
            get
            {
                return axisPowerOnRecoveryJournal != null
                    && string.IsNullOrEmpty(
                        axisPowerOnRecoveryJournalOpenError)
                    && string.IsNullOrEmpty(
                        axisPowerOnRecoveryJournalRuntimeError)
                    && !axisPowerOnRecoveryJournal.HasActiveRecord;
            }
        }

        private bool AxisPowerOnRecoveryJournalUnavailable
        {
            get
            {
                return axisPowerOnRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        axisPowerOnRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        axisPowerOnRecoveryJournalRuntimeError);
            }
        }

        private bool HasActiveAxisPowerOnRecoveryRecord
        {
            get
            {
                return HasActiveAxisPowerRecoveryRecord;
            }
        }

        private bool HasActiveAxisPowerRecoveryRecord
        {
            get
            {
                return (axisPowerDegradedSafetyOffRecord != null
                        && axisPowerDegradedSafetyOffRecord.IsActive)
                    || (axisPowerOnRecoveryJournal != null
                        && axisPowerOnRecoveryJournal.HasActiveRecord);
            }
        }

        private bool HasUnresolvedAxisPowerOnState()
        {
            return HasUnresolvedAxisPowerState();
        }

        private bool HasUnresolvedAxisPowerState()
        {
            return HasActiveAxisPowerRecoveryRecord
                || (pendingAxisPowerOnWaitContinuation != null
                    && pendingAxisPowerOnWaitContinuation.IsPending)
                || (pendingAxisPowerOffWaitContinuation != null
                    && pendingAxisPowerOffWaitContinuation.IsPending);
        }

        private void InitializeAxisPowerOnRecoveryJournal()
        {
            try
            {
                axisPowerOnRecoveryJournal =
                    axisPowerOnRecoveryJournalDirectoryPath == null
                        ? AxisPowerOnRecoveryJournal.OpenDefault()
                        : AxisPowerOnRecoveryJournal.Open(
                            axisPowerOnRecoveryJournalDirectoryPath);
                axisPowerOnRecoveryJournalOpenError = null;
                axisPowerOnRecoveryJournalRuntimeError = null;
                axisPowerDegradedSafetyOffRecord = null;
                axisPowerDurabilityDegraded = false;

                var record = axisPowerOnRecoveryJournal.CurrentRecord;
                if (record == null || !record.IsActive)
                {
                    return;
                }

                if (record.State
                    == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
                {
                    record = axisPowerOnRecoveryJournal
                        .PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                }

                ApplyRecoveredAxisPowerRecord(record);
            }
            catch (Exception error)
            {
                var journal = axisPowerOnRecoveryJournal;
                axisPowerOnRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                axisPowerOnRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                axisPowerOnRecoveryRequired = false;
                axisPowerOnAcceptedRestartRecovery = false;
                pendingAxisPowerOnWaitContinuation = null;
                pendingAxisPowerOffWaitContinuation = null;
                axisPowerOffReplacementAllowed = false;
                axisPowerOffWaitInterferenceConfirmed = false;
                axisPowerDegradedSafetyOffRecord = null;
                axisPowerDurabilityDegraded = false;
                WriteLog(
                    "Axis Power recovery journal is unavailable. New Power On is "
                    + "fail-closed, but an explicit safety Power Off remains "
                    + "available with degraded process-local tracking: "
                    + axisPowerOnRecoveryJournalOpenError);
            }
        }

        private void DisposeAxisPowerOnRecoveryJournal()
        {
            var journal = axisPowerOnRecoveryJournal;
            axisPowerOnRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private async Task<AxisPowerOnRecoveryRecord>
            ArmAxisPowerOnRecoveryBeforeDispatchAsync(
                LMCSingleAxis currentAxis)
        {
            return await ArmAxisPowerRecoveryBeforeDispatchAsync(
                currentAxis,
                true);
        }

        private async Task<AxisPowerOnRecoveryRecord>
            ArmAxisPowerRecoveryBeforeDispatchAsync(
                LMCSingleAxis currentAxis,
                bool expectedPowerOn,
                bool identityAlreadyRefreshed = false)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }

            EnsureAxisPowerOnRecoveryJournalCanArm();
            var currentConnection = RequireConnection();
            if (!identityAlreadyRefreshed)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                expectedPowerOn ? "Axis Power On" : "Axis Power Off");
            try
            {
                var record = axisPowerOnRecoveryJournal.ArmBeforeDispatch(
                    expectedPowerOn,
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    DateTime.UtcNow);
                axisPowerDegradedSafetyOffRecord = null;
                axisPowerDurabilityDegraded = false;
                ApplyCurrentAxisPowerRecord(currentAxis, record);
                return record;
            }
            catch (Exception error)
            {
                SetAxisPowerOnRecoveryJournalRuntimeError(
                    expectedPowerOn
                        ? "arm-before-PowerOn"
                        : "arm-before-PowerOff",
                    error);
                throw CreateAxisPowerOnRecoveryJournalException(
                    expectedPowerOn ? "Axis Power On" : "Axis Power Off",
                    error);
            }
        }

        private void MarkAxisPowerOnAccepted(
            LMCAxisPowerOnWaitContinuation continuation,
            string operation)
        {
            var record = RequireActiveAxisPowerOnRecoveryRecord(operation);
            MarkAxisPowerOnAcceptedForRecord(
                axis,
                continuation,
                record,
                operation);
        }

        private void PersistAxisPowerOnAccepted(
            LMCAxisPowerOnWaitContinuation continuation,
            string operation)
        {
            var record = RequireActiveAxisPowerOnRecoveryRecord(operation);
            PersistAxisPowerOnAcceptedForRecord(
                continuation,
                record,
                operation);
        }

        private void PersistAxisPowerOnAcceptedForRecord(
            LMCAxisPowerOnWaitContinuation continuation,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            if (continuation == null
                || !continuation.IsPending
                || verificationRecord == null
                || !verificationRecord.ExpectedPowerOn
                || !string.Equals(
                    continuation.AxisName,
                    verificationRecord.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference
                    != verificationRecord.AxisReference)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted Axis Power On continuation that does not match the operation identity.");
            }

            PersistAxisPowerAcceptedState(
                verificationRecord,
                true,
                operation);
        }

        private void MarkAxisPowerOnAcceptedForRecord(
            LMCSingleAxis currentAxis,
            LMCAxisPowerOnWaitContinuation continuation,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            PersistAxisPowerOnAcceptedForRecord(
                continuation,
                verificationRecord,
                operation);
            var continuationReusable = connection != null
                && connection.IsConnected
                && currentAxis != null
                && ReferenceEquals(axis, currentAxis)
                && ReferenceEquals(
                    currentAxis.PendingPowerOnWaitContinuation,
                    continuation)
                && continuation.IsPending;
            pendingAxisPowerOnWaitContinuation = continuationReusable
                ? continuation
                : null;
            pendingAxisPowerOffWaitContinuation = null;
            axisPowerOnRecoveryRequired = false;
            axisPowerOnAcceptedRestartRecovery = !continuationReusable;
            SetAxisPowerOffReplacementAllowed(false);
            WriteLog(
                operation
                + " preserved one accepted Axis Power On ACK. Resume performs "
                + "status-only 0x2028 polling; 0x2023 will not be replayed.");
        }

        private void PersistAxisPowerOffAcceptedForRecord(
            LMCAxisPowerOffWaitContinuation continuation,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            if (continuation == null
                || !continuation.IsPending
                || verificationRecord == null
                || verificationRecord.ExpectedPowerOn
                || !continuation.Acknowledgement.IsSuccess
                || !string.Equals(
                    continuation.AxisName,
                    verificationRecord.AxisName,
                    StringComparison.Ordinal)
                || continuation.AxisReference
                    != verificationRecord.AxisReference)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted Axis Power Off continuation that does not match the operation identity.");
            }

            PersistAxisPowerAcceptedState(
                verificationRecord,
                false,
                operation);
        }

        private void MarkAxisPowerOffAcceptedForRecord(
            LMCSingleAxis currentAxis,
            LMCAxisPowerOffWaitContinuation continuation,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            PersistAxisPowerOffAcceptedForRecord(
                continuation,
                verificationRecord,
                operation);
            var continuationReusable = connection != null
                && connection.IsConnected
                && currentAxis != null
                && ReferenceEquals(axis, currentAxis)
                && ReferenceEquals(
                    currentAxis.PendingPowerOffWaitContinuation,
                    continuation)
                && continuation.IsPending;
            pendingAxisPowerOnWaitContinuation = null;
            pendingAxisPowerOffWaitContinuation = continuationReusable
                ? continuation
                : null;
            axisPowerOnRecoveryRequired = false;
            axisPowerOnAcceptedRestartRecovery = !continuationReusable;
            SetAxisPowerOffReplacementAllowed(false);
            WriteLog(
                operation
                + " preserved one accepted Axis Power Off ACK. Resume performs "
                + "status-only 0x2028 polling; 0x2023 will not be replayed.");
        }

        private void PersistAxisPowerAcceptedState(
            AxisPowerOnRecoveryRecord verificationRecord,
            bool expectedPowerOn,
            string operation)
        {
            var current = GetCurrentAxisPowerOperationRecord();
            if (current == null
                || !current.IsActive
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || verificationRecord.ExpectedPowerOn != expectedPowerOn
                || current.ExpectedPowerOn != expectedPowerOn)
            {
                ReapplyCurrentAxisPowerRecoveryState(axis);
                throw new InvalidOperationException(
                    operation
                    + " belongs to an older Axis Power operation. The newer recovery identity was preserved.");
            }

            try
            {
                if (IsDegradedAxisPowerRecord(verificationRecord))
                {
                    if (current.State
                            == AxisPowerOnRecoveryState.ArmedBeforeDispatch
                        || (!current.ExpectedPowerOn
                            && current.State
                                == AxisPowerOnRecoveryState.RecoveryRequired))
                    {
                        axisPowerDegradedSafetyOffRecord = current.TransitionTo(
                            AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                            MonotonicUtcNow(current.UpdatedUtc));
                    }
                    return;
                }

                if (AxisPowerOnRecoveryJournalUnavailable)
                {
                    axisPowerDurabilityDegraded = true;
                    return;
                }

                if (current.State
                        == AxisPowerOnRecoveryState.ArmedBeforeDispatch
                    || (!current.ExpectedPowerOn
                        && current.State
                            == AxisPowerOnRecoveryState.RecoveryRequired))
                {
                    var beforeDurableMark =
                        AxisPowerAcceptedBeforeDurableMarkTestHook;
                    if (beforeDurableMark != null)
                    {
                        beforeDurableMark(verificationRecord.Copy());
                    }
                    axisPowerOnRecoveryJournal.MarkAccepted(
                        verificationRecord.Identity,
                        MonotonicUtcNow(current.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                var latest = GetCurrentAxisPowerOperationRecord();
                if (latest != null
                    && latest.IsActive
                    && latest.Identity == verificationRecord.Identity
                    && latest.ExpectedPowerOn == expectedPowerOn
                    && expectedPowerOn
                    && latest.State
                        == AxisPowerOnRecoveryState.RecoveryRequired)
                {
                    ReapplyCurrentAxisPowerRecoveryState(axis);
                    throw new InvalidOperationException(
                        operation
                        + " acceptance raced with connection-loss safety promotion. The same durable Axis Power On identity remains RecoveryRequired and the journal remains available.",
                        error);
                }

                if (latest == null
                    || !latest.IsActive
                    || latest.Identity != verificationRecord.Identity
                    || latest.ExpectedPowerOn != expectedPowerOn)
                {
                    ReapplyCurrentAxisPowerRecoveryState(axis);
                    throw new InvalidOperationException(
                        operation
                        + " lost ownership to a newer Axis Power operation before durable acceptance. The newer recovery record was preserved and the journal remains available.",
                        error);
                }

                RecordAxisPowerOnRecoveryJournalRuntimeError(
                    "mark-accepted",
                    error);
                axisPowerDurabilityDegraded = true;
                throw CreateAxisPowerOnRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private void PromoteAxisPowerOnRecovery(string reason)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record != null)
            {
                PromoteAxisPowerDispatchOutcomeUncertain(record, reason);
            }
        }

        private void PromoteAxisPowerDispatchOutcomeUncertain(
            AxisPowerOnRecoveryRecord verificationRecord,
            string reason)
        {
            var current = GetCurrentAxisPowerOperationRecord();
            if (current == null
                || !current.IsActive
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.ExpectedPowerOn
                    != verificationRecord.ExpectedPowerOn)
            {
                ReapplyCurrentAxisPowerRecoveryState(axis);
                WriteLog(
                    reason
                    + " belongs to an older Axis Power operation. The newer recovery identity was preserved.");
                return;
            }

            try
            {
                if (current.State
                    == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
                {
                    if (IsDegradedAxisPowerRecord(current))
                    {
                        axisPowerDegradedSafetyOffRecord = current.TransitionTo(
                            AxisPowerOnRecoveryState.RecoveryRequired,
                            MonotonicUtcNow(current.UpdatedUtc));
                    }
                    else if (!AxisPowerOnRecoveryJournalUnavailable)
                    {
                        axisPowerOnRecoveryJournal.PromoteToRecoveryRequired(
                            current.Identity,
                            MonotonicUtcNow(current.UpdatedUtc));
                    }
                }
            }
            catch (Exception error)
            {
                SetAxisPowerOnRecoveryJournalRuntimeError(
                    "promote-to-recovery",
                    error);
            }

            ReapplyCurrentAxisPowerRecoveryState(axis);
            var active = GetActiveAxisPowerRecoveryRecord();
            if (active != null)
            {
                WriteLog(
                    "SAFETY: "
                    + reason
                    + " retained the Axis Power "
                    + (active.ExpectedPowerOn ? "On" : "Off")
                    + " recovery interlock. No power command will be replayed automatically.");
            }
        }

        private void ResolveAxisPowerOnRecoveryJournal(string operation)
        {
            var record = RequireActiveAxisPowerOnRecoveryRecord(operation);
            ResolveAxisPowerRecoveryJournalForRecord(record, operation);
        }

        private bool ResolveAxisPowerRecoveryJournalForRecord(
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            var current = GetCurrentAxisPowerOperationRecord();
            if (current == null
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.ExpectedPowerOn
                    != verificationRecord.ExpectedPowerOn)
            {
                ReapplyCurrentAxisPowerRecoveryState(axis);
                WriteLog(
                    operation
                    + " result belongs to an older Axis Power operation. The newer recovery identity was preserved unchanged.");
                return false;
            }

            if (!current.IsActive)
            {
                return false;
            }

            if (IsDegradedAxisPowerRecord(current)
                || AxisPowerOnRecoveryJournalUnavailable)
            {
                if (IsDegradedAxisPowerRecord(current))
                {
                    axisPowerDegradedSafetyOffRecord = current.TransitionTo(
                        AxisPowerOnRecoveryState.Resolved,
                        MonotonicUtcNow(current.UpdatedUtc));
                }
                axisPowerDurabilityDegraded = true;
                ClearAxisPowerSessionContinuation();
                WriteLog(
                    operation
                    + " proved the safe Axis Power state, but no durable recovery record was resolved because journal durability is degraded.");
                return false;
            }

            try
            {
                axisPowerOnRecoveryJournal.Resolve(
                    verificationRecord.Identity,
                    MonotonicUtcNow(current.UpdatedUtc));
            }
            catch (Exception error)
            {
                SetAxisPowerOnRecoveryJournalRuntimeError("resolve", error);
                throw CreateAxisPowerOnRecoveryJournalException(
                    operation,
                    error);
            }

            ClearAxisPowerSessionContinuation();
            WriteLog(operation + " resolved the durable Axis Power record.");
            return true;
        }

        private bool TryResolveAxisPowerOnKnownRejection(
            Exception error,
            string operation)
        {
            if (!(error is LMCAxisPowerOnRejectedException))
            {
                return false;
            }

            ResolveAxisPowerOnRecoveryJournal(
                operation + " valid rejection");
            return true;
        }

        private async Task EnsureAxisPowerOnRecoveryIdentityAsync(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                return;
            }

            await EnsureAxisPowerRecoveryIdentityAsync(
                currentAxis,
                record,
                operation);
        }

        private async Task EnsureAxisPowerRecoveryIdentityAsync(
            LMCSingleAxis currentAxis,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            if (verificationRecord == null)
            {
                throw new ArgumentNullException("verificationRecord");
            }

            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }

            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                operation);
            if (!verificationRecord.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentAxis.AxisName,
                currentAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                verificationRecord.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because endpoint, axis reference, BootId, or "
                    + "MapRevision does not match the Axis Power "
                    + "record. No recovery mutation was sent.");
            }
        }

        private void EnsureAxisPowerOnRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                return;
            }

            if (!record.MatchesEndpoint(endpointIp, endpointPort))
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked because the PLC endpoint does not match "
                    + "the Axis Power recovery record. No TCP connection "
                    + "was opened.");
            }
        }

        private async Task
            EnsureAxisPowerOnRecoveryConnectionIdentityAsync(
                string operation)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                return;
            }

            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                record.AxisName,
                record.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw CreateRecoveryConnectionIdentityMismatch(
                    operation,
                    "Axis Power",
                    record.DiagnosticsBootId,
                    record.MapRevision,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }
        }

        private void EnsureAxisPowerOnRecoveryLookupAllowed(string axisName)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                return;
            }

            if (!string.Equals(
                record.AxisName,
                axisName,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A different axis cannot be loaded while Axis Power "
                    + "recovery is active. No lookup RPC was sent.");
            }
        }

        private void EnsureLoadedAxisMatchesPowerOnRecovery(
            LMCSingleAxis loadedAxis)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                return;
            }

            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                "Load Axis recovery");
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                loadedAxis.AxisName,
                loadedAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    "The loaded axis does not match the durable endpoint, axis "
                    + "reference, BootId, and MapRevision recovery identity.");
            }
        }

        private async Task<AxisPowerOnRecoveryRecord>
            PrepareAxisPowerOffBeforeDispatchAsync(
                LMCSingleAxis currentAxis,
                AxisPowerOnRecoveryRecord recoveryRecord,
                bool confirmedReplacement,
                bool identityAlreadyRefreshed,
                string operation)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }

            var currentConnection = RequireConnection();
            if (!identityAlreadyRefreshed)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                operation);

            if (recoveryRecord != null)
            {
                if (!recoveryRecord.MatchesRecoveryIdentity(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    currentAxis.AxisReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    recoveryRecord.ExpectedPowerOn))
                {
                    throw new InvalidOperationException(
                        operation
                        + " is blocked because endpoint, axis reference, BootId, or MapRevision does not match the Axis Power recovery record. No 0x2023 was sent.");
                }

                if (recoveryRecord.ExpectedPowerOn)
                {
                    if (!AxisPowerOnRecoveryJournalUnavailable
                        && !IsDegradedAxisPowerRecord(recoveryRecord))
                    {
                        try
                        {
                            var replacement = axisPowerOnRecoveryJournal
                                .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                                    recoveryRecord.Identity,
                                    RequiredConnectedRemoteIp(),
                                    RequiredConnectedRemotePort(),
                                    currentAxis.AxisName,
                                    currentAxis.AxisReference,
                                    capabilities.DiagnosticsBootId,
                                    capabilities.MapRevision,
                                    MonotonicUtcNow(
                                        recoveryRecord.UpdatedUtc));
                            axisPowerDegradedSafetyOffRecord = null;
                            axisPowerDurabilityDegraded = false;
                            ApplyCurrentAxisPowerRecord(
                                currentAxis,
                                replacement);
                            WriteLog(
                                operation
                                + " atomically replaced the unresolved Power On record with a durable Power Off arm before 0x2023 dispatch.");
                            return replacement;
                        }
                        catch (Exception error)
                        {
                            SetAxisPowerOnRecoveryJournalRuntimeError(
                                "replace-PowerOn-with-PowerOff",
                                error);
                        }
                    }

                    return CreateDegradedAxisPowerOffRecord(
                        currentAxis,
                        capabilities,
                        recoveryRecord,
                        operation);
                }

                if (!confirmedReplacement
                    || !axisPowerOffReplacementAllowed
                    || recoveryRecord.State
                        != AxisPowerOnRecoveryState.RecoveryRequired)
                {
                    throw new InvalidOperationException(
                        operation
                        + " is blocked until typed interference or a successful exact-identity PowerOn=true status confirms that one explicit Power Off Again is required.");
                }

                if (AxisPowerOnRecoveryJournalUnavailable)
                {
                    axisPowerDurabilityDegraded = true;
                }
                return recoveryRecord;
            }

            if (!AxisPowerOnRecoveryJournalUnavailable)
            {
                try
                {
                    return await ArmAxisPowerRecoveryBeforeDispatchAsync(
                        currentAxis,
                        false,
                        true);
                }
                catch (Exception)
                {
                    // Safety Power Off is the narrow exception to journal
                    // fail-closed admission. Continue with process-local
                    // tracking after the durable failure was recorded.
                }
            }

            return CreateDegradedAxisPowerOffRecord(
                currentAxis,
                capabilities,
                null,
                operation);
        }

        private AxisPowerOnRecoveryRecord CreateDegradedAxisPowerOffRecord(
            LMCSingleAxis currentAxis,
            LMCDiagnosticCapabilities capabilities,
            AxisPowerOnRecoveryRecord previousRecord,
            string operation)
        {
            var createdUtc = previousRecord == null
                ? DateTime.UtcNow
                : MonotonicUtcNow(previousRecord.UpdatedUtc);
            var record = new AxisPowerOnRecoveryRecord(
                Guid.NewGuid(),
                false,
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentAxis.AxisName,
                currentAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                createdUtc,
                createdUtc);
            axisPowerDegradedSafetyOffRecord = record;
            axisPowerDurabilityDegraded = true;
            ApplyCurrentAxisPowerRecord(currentAxis, record);
            WriteLog(
                "SAFETY: "
                + operation
                + " will send one Power Off with process-local tracking because the Axis Power journal is unavailable. Stable proof may establish the safe state, but it will not resolve or replace any durable record.");
            return record;
        }

        private void ConfirmAxisPowerOffReplacementAllowed(
            LMCSingleAxis currentAxis,
            AxisPowerOnRecoveryRecord verificationRecord,
            string reason)
        {
            var current = GetCurrentAxisPowerOperationRecord();
            if (current == null
                || !current.IsActive
                || verificationRecord == null
                || current.Identity != verificationRecord.Identity
                || current.ExpectedPowerOn)
            {
                ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                return;
            }

            EnsureCurrentAxisMatchesPowerRecovery(
                currentAxis,
                current,
                reason);
            try
            {
                if (current.State
                        == AxisPowerOnRecoveryState.ArmedBeforeDispatch
                    || current.State
                        == AxisPowerOnRecoveryState.AcceptedAwaitingProof)
                {
                    if (IsDegradedAxisPowerRecord(current))
                    {
                        axisPowerDegradedSafetyOffRecord = current.TransitionTo(
                            AxisPowerOnRecoveryState.RecoveryRequired,
                            MonotonicUtcNow(current.UpdatedUtc));
                    }
                    else if (!AxisPowerOnRecoveryJournalUnavailable)
                    {
                        axisPowerOnRecoveryJournal.PromoteToRecoveryRequired(
                            current.Identity,
                            MonotonicUtcNow(current.UpdatedUtc));
                    }
                }
            }
            catch (Exception error)
            {
                SetAxisPowerOnRecoveryJournalRuntimeError(
                    "confirm-PowerOff-replacement",
                    error);
                axisPowerDurabilityDegraded = true;
            }

            pendingAxisPowerOffWaitContinuation = null;
            pendingAxisPowerOnWaitContinuation = null;
            axisPowerOnAcceptedRestartRecovery = false;
            axisPowerOnRecoveryRequired = true;
            SetAxisPowerOffReplacementAllowed(true);
            WriteLog(
                reason
                + " confirmed that the pending Axis Power Off attribution cannot be used. One explicit Power Off Again is now allowed; automatic 0x2023 replay remains forbidden.");
        }

        private void ObserveAxisPowerRecoveryStatus(
            LMCSingleAxis currentAxis,
            AxisPowerOnRecoveryRecord verificationRecord,
            LMCReadStatusResult status,
            string operation)
        {
            if (verificationRecord != null
                && !verificationRecord.ExpectedPowerOn
                && status != null
                && status.IsSuccess
                && status.IsPowerOn)
            {
                ConfirmAxisPowerOffReplacementAllowed(
                    currentAxis,
                    verificationRecord,
                    operation + " observed exact PowerOn=true status");
            }
        }

        private async Task CompleteAxisPowerRecoveryAfterStableProofAsync(
            LMCSingleAxis currentAxis,
            bool expectedPowerOn,
            LMCReadStatusResult finalStatus,
            int stableSampleCount,
            int requiredStableSampleCount,
            AxisPowerOnRecoveryRecord verificationRecord,
            string operation)
        {
            if (verificationRecord == null
                || verificationRecord.ExpectedPowerOn != expectedPowerOn
                || finalStatus == null
                || !finalStatus.IsSuccess
                || finalStatus.IsPowerOn != expectedPowerOn
                || (!expectedPowerOn && !finalStatus.IsStandstill)
                || stableSampleCount < requiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve Axis Power recovery without exact direction and stable target-state proof.");
            }

            var current = GetCurrentAxisPowerOperationRecord();
            if (current == null
                || !current.IsActive
                || current.Identity != verificationRecord.Identity)
            {
                ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                WriteLog(
                    operation
                    + " completion belongs to an older Axis Power operation. The newer recovery identity was preserved unchanged.");
                return;
            }

            await EnsureAxisPowerRecoveryIdentityAsync(
                currentAxis,
                verificationRecord,
                operation + " final identity");
            if (expectedPowerOn
                && current.State
                    == AxisPowerOnRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve an outcome-uncertain Power On arm from status alone. Explicit Power Off recovery is required.");
            }

            if (!expectedPowerOn)
            {
                var powerOnContinuation =
                    currentAxis.PendingPowerOnWaitContinuation;
                if (powerOnContinuation != null
                    && powerOnContinuation.IsPending)
                {
                    currentAxis.ResolvePowerOnWaitAfterStablePowerOff(
                        powerOnContinuation);
                }
            }

            ResolveAxisPowerRecoveryJournalForRecord(
                verificationRecord,
                operation);
        }

        private Task CompleteAxisPowerOnRecoveryAfterPowerOffAsync(
            LMCSingleAxis currentAxis,
            LMCAxisPowerOffWaitContinuation continuation,
            LMCAxisPowerOffWaitResult result,
            string operation)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            return CompleteAxisPowerRecoveryAfterStableProofAsync(
                currentAxis,
                false,
                result == null ? null : result.FinalStatus,
                result == null
                    ? 0
                    : result.StablePowerOffStandstillSampleCount,
                result == null ? 0 : result.RequiredStableSampleCount,
                record,
                operation + " stable Power Off proof");
        }

        private void PreserveAxisPowerOnWaitFailure(
            LMCSingleAxis currentAxis,
            Exception error,
            AxisPowerOnRecoveryRecord verificationRecord,
            bool powerCommandDispatchStarted,
            string operation)
        {
            if (!IsCurrentAxisPowerOperationRecord(verificationRecord))
            {
                ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                WriteLog(
                    operation
                    + " failure belongs to an older Axis Power operation. The newer recovery identity or resolved tombstone was preserved.");
                return;
            }

            var evidence = GetAxisPowerOnWaitEvidence(error);
            var accepted = currentAxis == null
                ? null
                : currentAxis.PendingPowerOnWaitContinuation;
            if (accepted != null && accepted.IsPending)
            {
                pendingAxisPowerOnWaitContinuation = accepted;
                if (!AxisPowerOnRecoveryJournalUnavailable)
                {
                    MarkAxisPowerOnAcceptedForRecord(
                        currentAxis,
                        accepted,
                        verificationRecord,
                        operation + " verification incomplete");
                }
                else
                {
                    axisPowerDurabilityDegraded = true;
                }
                return;
            }

            var knownRejected = error is LMCAxisPowerOnRejectedException;
            var definitelyNotSent = evidence != null
                && !evidence.CommandMayHaveBeenSent;
            var preemptedBeforeWire = error is LMCSendPreemptedException
                && ((LMCSendPreemptedException)error).Phase
                    == LMCSendPreemptionPhase.BeforeWire;
            if ((knownRejected || definitelyNotSent || preemptedBeforeWire)
                && verificationRecord != null
                && verificationRecord.State
                    == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
            {
                ResolveAxisPowerRecoveryJournalForRecord(
                    verificationRecord,
                    operation + " known no-effect outcome");
                return;
            }

            if (!powerCommandDispatchStarted
                && verificationRecord != null
                && verificationRecord.State
                    == AxisPowerOnRecoveryState.AcceptedAwaitingProof)
            {
                ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                return;
            }

            PromoteAxisPowerDispatchOutcomeUncertain(
                verificationRecord,
                operation + " completion was not proven");
        }

        private void PreserveAxisPowerOffWaitFailure(
            LMCSingleAxis currentAxis,
            Exception error,
            AxisPowerOnRecoveryRecord verificationRecord,
            bool powerOnToPowerOffTakeover,
            bool confirmedReplacement,
            bool powerCommandDispatchStarted,
            LMCAxisPowerOffWaitContinuation priorContinuation,
            string operation)
        {
            if (!IsCurrentAxisPowerOperationRecord(verificationRecord))
            {
                ReapplyCurrentAxisPowerRecoveryState(currentAxis);
                WriteLog(
                    operation
                    + " failure belongs to an older Axis Power operation. The newer recovery identity or resolved tombstone was preserved.");
                return;
            }

            if (confirmedReplacement && !powerCommandDispatchStarted)
            {
                pendingAxisPowerOffWaitContinuation =
                    priorContinuation != null && priorContinuation.IsPending
                        ? priorContinuation
                        : null;
                axisPowerOnAcceptedRestartRecovery = false;
                axisPowerOnRecoveryRequired = true;
                SetAxisPowerOffReplacementAllowed(true);
                return;
            }

            var evidence = GetAxisPowerOffWaitEvidence(error);
            var continuation = GetAxisPowerOffWaitContinuation(error);
            if (continuation == null && currentAxis != null)
            {
                var published = currentAxis.PendingPowerOffWaitContinuation;
                if (published != null && published.IsPending)
                {
                    continuation = published;
                }
            }
            if (continuation == null
                && pendingAxisPowerOffWaitContinuation != null
                && pendingAxisPowerOffWaitContinuation.IsPending)
            {
                continuation = pendingAxisPowerOffWaitContinuation;
            }
            if (continuation != null && continuation.IsPending)
            {
                pendingAxisPowerOffWaitContinuation = continuation;
            }

            if (error is LMCAxisPowerOffInterferenceException
                && continuation != null)
            {
                ConfirmAxisPowerOffReplacementAllowed(
                    currentAxis,
                    verificationRecord,
                    operation + " typed SDK interference");
                return;
            }

            if (evidence != null
                && evidence.LastObservedStatus != null
                && evidence.LastObservedStatus.IsSuccess
                && evidence.LastObservedStatus.IsPowerOn)
            {
                ConfirmAxisPowerOffReplacementAllowed(
                    currentAxis,
                    verificationRecord,
                    operation + " observed exact PowerOn=true status");
                return;
            }

            var knownRejected = error is LMCAxisPowerOffRejectedException;
            var definitelyNotSent = evidence != null
                && !evidence.CommandMayHaveBeenSent;
            var preemptedBeforeWire = error is LMCSendPreemptedException
                && ((LMCSendPreemptedException)error).Phase
                    == LMCSendPreemptionPhase.BeforeWire;
            if ((knownRejected || definitelyNotSent || preemptedBeforeWire)
                && confirmedReplacement)
            {
                pendingAxisPowerOffWaitContinuation =
                    priorContinuation != null && priorContinuation.IsPending
                        ? priorContinuation
                        : null;
                axisPowerOnAcceptedRestartRecovery = false;
                axisPowerOnRecoveryRequired = true;
                SetAxisPowerOffReplacementAllowed(true);
                return;
            }

            if ((knownRejected || definitelyNotSent || preemptedBeforeWire)
                && !powerOnToPowerOffTakeover
                && verificationRecord != null
                && verificationRecord.State
                    == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
            {
                ResolveAxisPowerRecoveryJournalForRecord(
                    verificationRecord,
                    operation + " known no-effect outcome");
                return;
            }

            if (evidence != null
                && evidence.CommandMayHaveBeenSent
                && evidence.SubmissionOutcome
                    == LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain)
            {
                pendingAxisPowerOffWaitContinuation = null;
                SetAxisPowerOffReplacementAllowed(false);
                PromoteAxisPowerDispatchOutcomeUncertain(
                    verificationRecord,
                    operation + " dispatch outcome is uncertain");
                return;
            }

            if (continuation != null && continuation.IsPending)
            {
                axisPowerOnRecoveryRequired = false;
                axisPowerOnAcceptedRestartRecovery = false;
                SetAxisPowerOffReplacementAllowed(false);
                return;
            }

            PromoteAxisPowerDispatchOutcomeUncertain(
                verificationRecord,
                operation + " completion was not proven");
        }

        private void PreserveAxisPowerRecoveryAfterConnectionLoss(
            string reason)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                ClearAxisPowerSessionContinuation();
                return;
            }

            var preserveReplacement = ShouldPreserveAxisPowerOffReplacement(
                record);
            if (record.State
                == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
            {
                PromoteAxisPowerDispatchOutcomeUncertain(record, reason);
                record = GetActiveAxisPowerRecoveryRecord();
            }

            pendingAxisPowerOnWaitContinuation = null;
            pendingAxisPowerOffWaitContinuation = null;
            if (record != null)
            {
                axisPowerOnAcceptedRestartRecovery = record.State
                    == AxisPowerOnRecoveryState.AcceptedAwaitingProof;
                axisPowerOnRecoveryRequired = record.State
                    == AxisPowerOnRecoveryState.RecoveryRequired;
                SetAxisPowerOffReplacementAllowed(
                    preserveReplacement
                    && !record.ExpectedPowerOn
                    && axisPowerOnRecoveryRequired);
            }
            WriteLog(
                reason
                + " invalidated session-bound Axis Power continuations. Exact-identity recovery remains status-only; no 0x2023 replay is automatic.");
        }

        private void ApplyRecoveredAxisPowerRecord(
            AxisPowerOnRecoveryRecord record)
        {
            pendingAxisPowerOnWaitContinuation = null;
            pendingAxisPowerOffWaitContinuation = null;
            axisPowerOnAcceptedRestartRecovery = record.State
                == AxisPowerOnRecoveryState.AcceptedAwaitingProof;
            axisPowerOnRecoveryRequired = record.State
                == AxisPowerOnRecoveryState.RecoveryRequired;
            SetAxisPowerOffReplacementAllowed(false);
            TextRemoteIp.Text = record.EndpointIp;
            TextRemotePort.Text = record.EndpointPort.ToString(
                CultureInfo.InvariantCulture);
            TextAxisName.Text = record.AxisName;

            if (axisPowerOnAcceptedRestartRecovery)
            {
                WriteLog(
                    "Recovered a durable accepted Axis Power "
                    + (record.ExpectedPowerOn ? "On" : "Off")
                    + " ACK for "
                    + record.AxisName
                    + ". Reconnect to the exact identity and run status-only verification; 0x2023 will not be replayed.");
            }
            else if (record.ExpectedPowerOn)
            {
                WriteLog(
                    "SAFETY: recovered an outcome-uncertain Axis Power On record for "
                    + record.AxisName
                    + ". Power On replay is blocked. Explicit Power Off takeover and stable PowerOn=false plus Standstill proof are required.");
            }
            else
            {
                WriteLog(
                    "SAFETY: recovered an outcome-uncertain Axis Power Off record for "
                    + record.AxisName
                    + ". First run status-only PowerOn=false plus Standstill verification. A new 0x2023 is blocked until interference or PowerOn=true is confirmed.");
            }
        }

        private void ApplyCurrentAxisPowerRecord(
            LMCSingleAxis currentAxis,
            AxisPowerOnRecoveryRecord record)
        {
            var preserveReplacement =
                ShouldPreserveAxisPowerOffReplacement(record);
            var powerOnContinuation = currentAxis == null
                ? null
                : currentAxis.PendingPowerOnWaitContinuation;
            var powerOffContinuation = currentAxis == null
                ? null
                : currentAxis.PendingPowerOffWaitContinuation;
            pendingAxisPowerOnWaitContinuation = record.ExpectedPowerOn
                    && powerOnContinuation != null
                    && powerOnContinuation.IsPending
                    && powerOnContinuation.AxisReference
                        == record.AxisReference
                    && string.Equals(
                        powerOnContinuation.AxisName,
                        record.AxisName,
                        StringComparison.Ordinal)
                ? powerOnContinuation
                : null;
            pendingAxisPowerOffWaitContinuation = !record.ExpectedPowerOn
                    && powerOffContinuation != null
                    && powerOffContinuation.IsPending
                    && powerOffContinuation.AxisReference
                        == record.AxisReference
                    && string.Equals(
                        powerOffContinuation.AxisName,
                        record.AxisName,
                        StringComparison.Ordinal)
                ? powerOffContinuation
                : null;
            axisPowerOnAcceptedRestartRecovery = record.State
                    == AxisPowerOnRecoveryState.AcceptedAwaitingProof
                && pendingAxisPowerOnWaitContinuation == null
                && pendingAxisPowerOffWaitContinuation == null;
            axisPowerOnRecoveryRequired = record.State
                == AxisPowerOnRecoveryState.RecoveryRequired;
            SetAxisPowerOffReplacementAllowed(
                preserveReplacement
                && !record.ExpectedPowerOn
                && axisPowerOnRecoveryRequired);
        }

        private void ReapplyCurrentAxisPowerRecoveryState(
            LMCSingleAxis currentAxis)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                ClearAxisPowerSessionContinuation();
                return;
            }

            ApplyCurrentAxisPowerRecord(currentAxis, record);
        }

        private void ClearAxisPowerSessionContinuation()
        {
            pendingAxisPowerOnWaitContinuation = null;
            pendingAxisPowerOffWaitContinuation = null;
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record != null)
            {
                var preserveReplacement =
                    ShouldPreserveAxisPowerOffReplacement(record);
                axisPowerOnAcceptedRestartRecovery = record.State
                    == AxisPowerOnRecoveryState.AcceptedAwaitingProof;
                axisPowerOnRecoveryRequired = record.State
                    == AxisPowerOnRecoveryState.RecoveryRequired;
                SetAxisPowerOffReplacementAllowed(
                    preserveReplacement
                    && !record.ExpectedPowerOn
                    && axisPowerOnRecoveryRequired);
                return;
            }

            axisPowerOnAcceptedRestartRecovery = false;
            axisPowerOnRecoveryRequired = false;
            SetAxisPowerOffReplacementAllowed(false);
        }

        private void SetAxisPowerOffReplacementAllowed(bool allowed)
        {
            axisPowerOffReplacementAllowed = allowed;
            axisPowerOffWaitInterferenceConfirmed = allowed;
        }

        private bool ShouldPreserveAxisPowerOffReplacement(
            AxisPowerOnRecoveryRecord record)
        {
            return axisPowerOffReplacementAllowed
                && record != null
                && record.IsActive
                && !record.ExpectedPowerOn
                && record.State
                    == AxisPowerOnRecoveryState.RecoveryRequired;
        }

        private AxisPowerOnRecoveryRecord GetActiveAxisPowerRecoveryRecord()
        {
            if (axisPowerDegradedSafetyOffRecord != null
                && axisPowerDegradedSafetyOffRecord.IsActive)
            {
                return axisPowerDegradedSafetyOffRecord.Copy();
            }

            var journal = axisPowerOnRecoveryJournal;
            var record = journal == null ? null : journal.CurrentRecord;
            return record != null && record.IsActive ? record : null;
        }

        private AxisPowerOnRecoveryRecord GetCurrentAxisPowerOperationRecord()
        {
            if (axisPowerDegradedSafetyOffRecord != null)
            {
                return axisPowerDegradedSafetyOffRecord.Copy();
            }

            var journal = axisPowerOnRecoveryJournal;
            return journal == null ? null : journal.CurrentRecord;
        }

        private bool IsCurrentAxisPowerOperationRecord(
            AxisPowerOnRecoveryRecord verificationRecord)
        {
            var current = GetCurrentAxisPowerOperationRecord();
            return verificationRecord != null
                && current != null
                && current.IsActive
                && current.Identity == verificationRecord.Identity
                && current.ExpectedPowerOn
                    == verificationRecord.ExpectedPowerOn;
        }

        private bool IsDegradedAxisPowerRecord(
            AxisPowerOnRecoveryRecord record)
        {
            return record != null
                && axisPowerDegradedSafetyOffRecord != null
                && record.Identity
                    == axisPowerDegradedSafetyOffRecord.Identity;
        }

        private void EnsureCurrentAxisMatchesPowerRecovery(
            LMCSingleAxis currentAxis,
            AxisPowerOnRecoveryRecord record,
            string operation)
        {
            if (currentAxis == null)
            {
                throw new ArgumentNullException("currentAxis");
            }

            var capabilities = RequireStableAxisPowerOnRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentAxis.AxisName,
                currentAxis.AxisReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot mutate Axis Power recovery because the exact endpoint, axis reference, BootId, or MapRevision does not match.");
            }
        }

        private AxisPowerOnRecoveryRecord
            RequireActiveAxisPowerOnRecoveryRecord(string operation)
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (record == null)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because no matching active Axis Power recovery record exists.");
            }

            return record;
        }

        private void EnsureAxisPowerOnRecoveryJournalCanArm()
        {
            if (AxisPowerOnRecoveryJournalCanArm)
            {
                return;
            }

            throw CreateAxisPowerOnRecoveryJournalException(
                "Axis Power On",
                null);
        }

        private LMCDiagnosticCapabilities
            RequireStableAxisPowerOnRecoveryIdentity(string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires nonzero DiagnosticsBootId and MapRevision "
                    + "before an Axis Power mutation or recovery proof is allowed.");
            }

            return capabilities;
        }

        private InvalidOperationException
            CreateAxisPowerOnRecoveryJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    axisPowerOnRecoveryJournalRuntimeError)
                ? axisPowerOnRecoveryJournalRuntimeError
                : (!string.IsNullOrEmpty(axisPowerOnRecoveryJournalOpenError)
                    ? axisPowerOnRecoveryJournalOpenError
                    : "An active durable Axis Power record blocks this operation.");
            return new InvalidOperationException(
                operation
                + " is blocked by the durable Axis Power recovery journal. "
                + detail,
                innerException);
        }

        private void SetAxisPowerOnRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            RecordAxisPowerOnRecoveryJournalRuntimeError(operation, error);
            WriteLog(
                "Axis Power On recovery journal faulted and remains fail-closed for Power On. Explicit safety Power Off uses degraded process-local tracking: "
                + axisPowerOnRecoveryJournalRuntimeError);
        }

        private void RecordAxisPowerOnRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            axisPowerOnRecoveryJournalRuntimeError =
                operation
                + ": "
                + error.GetType().Name
                + ": "
                + error.Message;
        }

        private string GetAxisPowerOnRecoveryGuidance()
        {
            var record = GetActiveAxisPowerRecoveryRecord();
            if (axisPowerDurabilityDegraded)
            {
                return "Axis Power journal durability is degraded. Explicit safety Power Off remains available through process-local tracking, but status proof cannot claim durable recovery resolution.";
            }

            if (AxisPowerOnRecoveryJournalUnavailable)
            {
                return "The Axis Power recovery journal is unavailable; new Power On is disabled. Explicit safety Power Off remains available, but its proof cannot claim a durable journal resolution.";
            }

            if (record == null)
            {
                return "No durable Axis Power recovery record is active.";
            }

            if (record.ExpectedPowerOn
                && record.State
                    == AxisPowerOnRecoveryState.RecoveryRequired)
            {
                return "Axis Power On outcome is uncertain. Do not replay Power On; send Power Off explicitly and verify three stable safe samples.";
            }

            if (!record.ExpectedPowerOn
                && record.State
                    == AxisPowerOnRecoveryState.RecoveryRequired
                && !axisPowerOffReplacementAllowed)
            {
                return "Axis Power Off outcome is uncertain. Run exact-identity status-only PowerOn=false plus Standstill verification before considering 0x2023 again.";
            }

            return "An accepted Axis Power transition is awaiting exact-identity status-only verification; do not replay 0x2023.";
        }
    }
}

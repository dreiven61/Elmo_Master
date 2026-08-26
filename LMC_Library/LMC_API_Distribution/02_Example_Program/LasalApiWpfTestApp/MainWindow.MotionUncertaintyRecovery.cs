using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly string motionUncertaintyJournalDirectoryPath;
        private MotionUncertaintyJournal motionUncertaintyJournal;
        private string motionUncertaintyJournalOpenError;
        private string motionUncertaintyJournalRuntimeError;
        private bool motionUncertaintyRecoveredAtStartup;
        private MotionUncertaintyTargetKind motionTargetKind;
        private ushort motionTargetReference;
        private bool motionRecoveryRequiresExplicitSafetyCommand;
        private int motionRecoverySafetyTrackingGeneration;
        private long motionRecoverySafetyGeneration;
        private MotionLookupIdentity axisMotionLookupIdentity;
        private MotionLookupIdentity groupMotionLookupIdentity;

        private bool MotionUncertaintyJournalCanArm
        {
            get
            {
                return motionUncertaintyJournal != null
                    && string.IsNullOrEmpty(motionUncertaintyJournalOpenError)
                    && string.IsNullOrEmpty(motionUncertaintyJournalRuntimeError)
                    && !motionUncertaintyJournal.HasActiveRecord;
            }
        }

        private bool MotionUncertaintyJournalUnavailable
        {
            get
            {
                return motionUncertaintyJournal == null
                    || !string.IsNullOrEmpty(motionUncertaintyJournalOpenError)
                    || !string.IsNullOrEmpty(motionUncertaintyJournalRuntimeError);
            }
        }

        private bool HasActiveMotionUncertaintyJournalRecord
        {
            get
            {
                return motionUncertaintyJournal != null
                    && motionUncertaintyJournal.HasActiveRecord;
            }
        }

        private bool MotionRecoveryReconnectAvailable
        {
            get
            {
                return motionMayBeActive
                    && HasActiveMotionUncertaintyJournalRecord;
            }
        }

        private void InitializeMotionUncertaintyJournal()
        {
            try
            {
                motionUncertaintyJournal =
                    motionUncertaintyJournalDirectoryPath == null
                        ? MotionUncertaintyJournal.OpenDefault()
                        : MotionUncertaintyJournal.Open(
                            motionUncertaintyJournalDirectoryPath);
                motionUncertaintyJournalOpenError = null;
                motionUncertaintyJournalRuntimeError = null;

                TryFinalizeCommittedMotionRetirementAtStartup();

                var record = motionUncertaintyJournal.CurrentRecord;
                motionUncertaintyRecoveredAtStartup =
                    record != null && record.IsActive;
                if (!motionUncertaintyRecoveredAtStartup)
                {
                    return;
                }

                ApplyMotionUncertaintyRecord(record);
                RequireExplicitMotionRecoverySafety(
                    "Startup restored an unresolved motion record");
                if (record.State
                    == MotionUncertaintyState.ArmedBeforeDispatch)
                {
                    try
                    {
                        record = motionUncertaintyJournal
                            .PromoteToRecoveryRequired(
                                record.Identity,
                                MotionRecoveryUtcNow(record.UpdatedUtc));
                    }
                    catch (Exception error)
                    {
                        // The exact record was already loaded and applied to
                        // the volatile interlock. Keep it and its writer lock
                        // even when the best-effort startup promotion cannot
                        // be persisted; Stop/PowerOff recovery can still
                        // resolve ArmedBeforeDispatch directly.
                        SetMotionUncertaintyJournalRuntimeError(
                            "startup-promote-to-recovery",
                            error);
                        WriteLog(
                            "SAFETY: Startup promotion failed after the exact "
                            + "motion record was loaded. The recovery interlock "
                            + "remains active; only the recorded endpoint and "
                            + "target may be used for Stop or PowerOff.");
                        return;
                    }
                }

                WriteLog(
                    "SAFETY: Restored durable motion uncertainty for "
                    + record.TargetKind
                    + " "
                    + record.TargetName
                    + " (Ref="
                    + record.TargetReference
                    + "). Connect only to the recorded endpoint, load only "
                    + "that target, then use Stop or PowerOff. No Move was replayed.");
            }
            catch (Exception error)
            {
                var journal = motionUncertaintyJournal;
                motionUncertaintyJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                motionUncertaintyRecoveredAtStartup = false;
                motionUncertaintyJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "Motion uncertainty journal is unavailable; all new Move "
                    + "commands are fail-closed: "
                    + motionUncertaintyJournalOpenError);
            }
        }

        private void DisposeMotionUncertaintyJournal()
        {
            var journal = motionUncertaintyJournal;
            motionUncertaintyJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private void ArmMotionUncertaintyBeforeDispatch(
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation)
        {
            EnsureMotionUncertaintyJournalCanArm(operation);
            var capabilities = RequireStableMotionRecoveryIdentity(operation);
            MotionUncertaintyRecord record;
            try
            {
                record = motionUncertaintyJournal.ArmBeforeDispatch(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    targetKind,
                    targetName,
                    targetReference,
                    operation,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    DateTime.UtcNow);
            }
            catch (Exception error)
            {
                SetMotionUncertaintyJournalRuntimeError(
                    "arm-before-Move",
                    error);
                throw CreateMotionUncertaintyJournalException(
                    operation,
                    error);
            }

            motionTargetKind = record.TargetKind;
            motionTargetReference = record.TargetReference;
        }

        private async Task<LMC_Response> DispatchTrackedMotionAsync(
            long expectedSafetyGeneration,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            Action<int> captureTrackingGeneration,
            Func<Task<LMC_Response>> send,
            Func<Task> validateImmediatelyBeforeTracking = null)
        {
            return await SendLiveCommandAsync(
                expectedSafetyGeneration,
                operation,
                async () =>
                {
                    await RefreshMotionIdentityBeforeDispatchAsync(
                        expectedSafetyGeneration,
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    if (validateImmediatelyBeforeTracking != null)
                    {
                        await validateImmediatelyBeforeTracking();
                    }
                    var trackingGeneration = MarkMotionUncertain(
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    captureTrackingGeneration?.Invoke(trackingGeneration);
                    try
                    {
                        var response = await send();
                        if (IsConfirmedRejected(response))
                        {
                            ClearMotionWarningAfterConfirmedNoMotion(
                                operation + " was rejected by a valid response",
                                trackingGeneration);
                        }
                        else if (IsConfirmedAccepted(response))
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " dispatch result is not a confirmed rejection");
                        }
                        else
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " returned an invalid or incomplete response",
                                true);
                        }

                        return response;
                    }
                    catch (Exception error)
                    {
                        HandleTrackedMotionDispatchException(
                            error,
                            operation,
                            trackingGeneration);
                        throw;
                    }
                });
        }

        private async Task<LMCAdminResponse> DispatchTrackedMotionAsync(
            long expectedSafetyGeneration,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            Action<int> captureTrackingGeneration,
            Func<Task<LMCAdminResponse>> send)
        {
            return await SendLiveCommandAsync(
                expectedSafetyGeneration,
                operation,
                async () =>
                {
                    await RefreshMotionIdentityBeforeDispatchAsync(
                        expectedSafetyGeneration,
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    var trackingGeneration = MarkMotionUncertain(
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    captureTrackingGeneration?.Invoke(trackingGeneration);
                    try
                    {
                        var response = await send();
                        if (IsConfirmedRejected(response))
                        {
                            ClearMotionWarningAfterConfirmedNoMotion(
                                operation + " was rejected by a valid response",
                                trackingGeneration);
                        }
                        else if (IsConfirmedAccepted(response))
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " dispatch result is not a confirmed rejection");
                        }
                        else
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " returned an invalid or incomplete response",
                                true);
                        }

                        return response;
                    }
                    catch (Exception error)
                    {
                        HandleTrackedMotionDispatchException(
                            error,
                            operation,
                            trackingGeneration);
                        throw;
                    }
                });
        }

        private async Task<LMCAdminResponse>
            DispatchTrackedQualificationMotionAsync(
                MotionUncertaintyTargetKind targetKind,
                string targetName,
                ushort targetReference,
                string operation,
                CancellationToken cancellationToken,
                Action<int> captureTrackingGeneration,
                Func<Task<LMCAdminResponse>> send)
        {
            var expectedSafetyGeneration = qualificationSafetyGeneration;
            return await SendQualificationCommandAsync(
                operation,
                cancellationToken,
                async () =>
                {
                    await RefreshMotionIdentityBeforeDispatchAsync(
                        expectedSafetyGeneration,
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    var trackingGeneration = MarkMotionUncertain(
                        targetKind,
                        targetName,
                        targetReference,
                        operation);
                    captureTrackingGeneration?.Invoke(trackingGeneration);
                    try
                    {
                        var response = await send();
                        if (IsConfirmedRejected(response))
                        {
                            ClearMotionWarningAfterConfirmedNoMotion(
                                operation + " was rejected by a valid response",
                                trackingGeneration);
                        }
                        else if (IsConfirmedAccepted(response))
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " dispatch result is not a confirmed rejection");
                        }
                        else
                        {
                            PromoteMotionUncertaintyJournal(
                                operation + " returned an invalid or incomplete response",
                                true);
                        }

                        return response;
                    }
                    catch (Exception error)
                    {
                        HandleTrackedMotionDispatchException(
                            error,
                            operation,
                            trackingGeneration);
                        throw;
                    }
                });
        }

        private void HandleTrackedMotionDispatchException(
            Exception error,
            string operation,
            int trackingGeneration)
        {
            var adminError = error as LMCAdminCommandException;
            if (adminError != null
                && IsConfirmedRejected(adminError.Response))
            {
                ClearMotionWarningAfterConfirmedNoMotion(
                    operation + " was rejected by a valid admin response",
                    trackingGeneration);
                return;
            }

            var preempted = error as LMCSendPreemptedException;
            if (preempted != null
                && preempted.Phase == LMCSendPreemptionPhase.BeforeWire)
            {
                ClearMotionWarningAfterConfirmedNoMotion(
                    operation + " was preempted before wire transmission",
                    trackingGeneration);
                return;
            }

            PromoteMotionUncertaintyJournal(
                operation + " dispatch threw " + error.GetType().Name,
                true);
        }

        private static bool IsConfirmedRejected(LMC_Response response)
        {
            return response != null
                && response.IsFrameValid
                && !response.IsSuccess;
        }

        private static bool IsConfirmedAccepted(LMC_Response response)
        {
            return response != null
                && response.IsFrameValid
                && response.IsSuccess;
        }

        private static bool IsConfirmedRejected(LMCAdminResponse response)
        {
            return response != null
                && response.TransportResponse != null
                && response.TransportResponse.IsFrameValid
                && !response.IsSuccess;
        }

        private static bool IsConfirmedAccepted(LMCAdminResponse response)
        {
            return response != null
                && response.TransportResponse != null
                && response.TransportResponse.IsFrameValid
                && response.IsSuccess;
        }

        private void PromoteMotionUncertaintyJournal(
            string reason,
            bool requireExplicitSafetyCommand = false)
        {
            if (requireExplicitSafetyCommand)
            {
                RequireExplicitMotionRecoverySafety(reason);
            }

            var journal = motionUncertaintyJournal;
            try
            {
                if (journal != null)
                {
                    var record = journal.CurrentRecord;
                    if (record != null
                        && record.IsActive
                        && record.State
                            == MotionUncertaintyState.ArmedBeforeDispatch)
                    {
                        journal.PromoteToRecoveryRequired(
                            record.Identity,
                            MotionRecoveryUtcNow(record.UpdatedUtc));
                    }
                }
            }
            catch (Exception error)
            {
                SetMotionUncertaintyJournalRuntimeError(
                    "promote-to-recovery",
                    error);
            }

            WriteLog(
                "SAFETY: "
                + reason
                + "; durable motion recovery remains active and no Move will be replayed.");
        }

        private void ResolveMotionUncertaintyJournal(string operation)
        {
            var journal = motionUncertaintyJournal;
            if (journal == null)
            {
                throw CreateMotionUncertaintyJournalException(
                    operation,
                    null);
            }

            try
            {
                var record = journal.CurrentRecord;
                if (record == null || !record.IsActive)
                {
                    throw new InvalidOperationException(
                        "The volatile motion interlock has no matching active durable record.");
                }

                journal.Resolve(
                    record.Identity,
                    MotionRecoveryUtcNow(record.UpdatedUtc));
                motionUncertaintyRecoveredAtStartup = false;
                motionUncertaintyJournalRuntimeError = null;
            }
            catch (Exception error)
            {
                SetMotionUncertaintyJournalRuntimeError("resolve", error);
                throw CreateMotionUncertaintyJournalException(
                    operation,
                    error);
            }
        }

        private async Task ClearMotionWarningAfterVerifiedStateAsync(
            string reason,
            int? expectedTrackingGeneration = null,
            Action afterMotionJournalResolvedBeforeVolatileClear = null,
            Action validateFinalIdentityBeforeJournalResolve = null)
        {
            if (!motionMayBeActive
                || (expectedTrackingGeneration.HasValue
                    && expectedTrackingGeneration.Value
                        != motionTrackingGeneration))
            {
                if (!motionMayBeActive
                    && afterMotionJournalResolvedBeforeVolatileClear != null)
                {
                    afterMotionJournalResolvedBeforeVolatileClear();
                }
                return;
            }

            var expectedSafetyGeneration = safetyRequestGeneration;
            var identityVerified = false;
            await commandSendGate.WaitAsync();
            try
            {
                EnsureNoNewSafetyRequest(
                    expectedSafetyGeneration,
                    reason + " resolution identity");
                var record = RequireActiveMotionUncertaintyRecord(reason);
                var currentConnection = RequireConnection();
                using (sendPriorityCoordinator.BeginPreemptibleScope(
                    expectedSafetyGeneration,
                    reason + " resolution identity"))
                {
                    await RefreshDiagnosticsCapabilitiesAsync(
                        currentConnection);
                    EnsureNoNewSafetyRequestBeforeResultApplication(
                        expectedSafetyGeneration,
                        reason + " resolution identity");
                }

                var capabilities = RequireStableMotionRecoveryIdentity(reason);
                var targetMatches = record.TargetKind
                        == MotionUncertaintyTargetKind.Axis
                    ? IsTrackedMotionTarget(axis)
                    : IsTrackedMotionTarget(group);
                if (!targetMatches
                    || !record.MatchesRecoveryIdentity(
                        RequiredConnectedRemoteIp(),
                        RequiredConnectedRemotePort(),
                        record.TargetKind,
                        record.TargetName,
                        record.TargetReference,
                        record.Operation,
                        capabilities.DiagnosticsBootId,
                        capabilities.MapRevision))
                {
                    throw new InvalidOperationException(
                        reason
                        + " cannot resolve the durable motion record because "
                        + "endpoint, target reference, DiagnosticsBootId, or "
                        + "MapRevision changed before final safe-state proof.");
                }

                if (validateFinalIdentityBeforeJournalResolve != null)
                {
                    validateFinalIdentityBeforeJournalResolve();
                }
                var axisCommandRecord =
                    GetActiveAxisCommandRecoveryRecord();
                if (HasActiveAxisQualificationRecoveryRecord
                    && record.TargetKind
                        == MotionUncertaintyTargetKind.Axis
                    && axisCommandRecord != null
                    && axisCommandRecord.Operation
                        == AxisCommandRecoveryOperation.Stop)
                {
                    CheckpointAxisQualificationStopStableBeforeChildResolve(
                        axis,
                        reason + " pre-motion-journal sequence checkpoint");
                }
                identityVerified = true;
                ClearMotionWarningCore(
                    reason,
                    expectedTrackingGeneration,
                    afterMotionJournalResolvedBeforeVolatileClear);
            }
            catch
            {
                if (!identityVerified && motionMayBeActive)
                {
                    RequireExplicitMotionRecoverySafety(
                        reason + " final identity verification failed");
                }

                throw;
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task RefreshMotionIdentityBeforeDispatchAsync(
            long expectedSafetyGeneration,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation)
        {
            EnsureMotionUncertaintyJournalCanArm(operation);
            var lookupIdentity = RequireMotionLookupIdentity(
                targetKind,
                targetName,
                targetReference,
                operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureNoNewSafetyRequest(
                expectedSafetyGeneration,
                operation + " fresh motion identity");
            var capabilities = RequireStableMotionRecoveryIdentity(operation);
            if (!lookupIdentity.Matches(
                targetKind,
                targetName,
                targetReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked before Move because BootId or MapRevision "
                    + "changed after target lookup. Reload the exact target "
                    + "under the fresh diagnostics identity first.");
            }
        }

        private async Task EnsureMotionRecoverySafetyDispatchIdentityAsync(
            long expectedSafetyGeneration,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord(operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureNoNewSafetyRequest(
                expectedSafetyGeneration,
                operation + " fresh recovery identity");
            var capabilities = RequireStableMotionRecoveryIdentity(operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                targetKind,
                targetName,
                targetReference,
                record.Operation,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                RequireExplicitMotionRecoverySafety(
                    operation + " fresh recovery identity mismatch");
                throw new InvalidOperationException(
                    operation
                    + " is blocked before the safety mutation because endpoint, "
                    + "target reference, DiagnosticsBootId, or MapRevision no "
                    + "longer matches the durable motion record.");
            }
        }

        private void RecordMotionRecoverySafetyCommandAccepted(
            long safetyGeneration,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord(operation);
            if (record.TargetKind != targetKind
                || record.TargetReference != targetReference
                || !string.Equals(
                    record.TargetName,
                    targetName,
                    StringComparison.Ordinal))
            {
                RequireExplicitMotionRecoverySafety(
                    operation + " acknowledgement target mismatch");
                return;
            }

            motionRecoverySafetyTrackingGeneration =
                motionTrackingGeneration;
            motionRecoverySafetyGeneration = safetyGeneration;
            WriteLog(
                "SAFETY: "
                + operation
                + " was accepted for the exact durable motion identity. "
                + "Stable safe-state proof is still required.");
        }

        private void RequireExplicitMotionRecoverySafety(string reason)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var newlyRequired =
                !motionRecoveryRequiresExplicitSafetyCommand;
            motionRecoveryRequiresExplicitSafetyCommand = true;
            motionRecoverySafetyTrackingGeneration = 0;
            motionRecoverySafetyGeneration = 0;
            if (newlyRequired)
            {
                WriteLog(
                    "SAFETY: "
                    + reason
                    + "; status-only observation cannot resolve this record. "
                    + "Send Stop or PowerOff to the exact identity, then verify "
                    + "the stable safe state.");
            }
        }

        private void EnsureExplicitMotionRecoverySafetyWasAccepted(
            string operation)
        {
            if (!motionRecoveryRequiresExplicitSafetyCommand)
            {
                return;
            }

            if (motionRecoverySafetyTrackingGeneration
                    == motionTrackingGeneration
                && motionRecoverySafetyGeneration
                    == safetyRequestGeneration)
            {
                return;
            }

            if (HasDurableAcceptedAxisStopSafetyEvidenceForMotionRecovery())
            {
                WriteLog(
                    "SAFETY: Durable accepted Axis Stop identity is coupled "
                    + "to the recovered motion record; the completed status-only "
                    + "standstill proof may resolve Motion before Axis Stop.");
                return;
            }

            throw new InvalidOperationException(
                operation
                + " cannot resolve startup or ambiguous motion recovery from "
                + "status-only evidence. An exact-identity Stop or PowerOff "
                + "must first return a valid success response.");
        }

        private void EnsureMotionRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            if (!motionMayBeActive
                && !HasActiveMotionUncertaintyJournalRecord)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord("Reconnect");
            var normalizedIp = NormalizeIPv4(endpointIp);
            if (!string.Equals(
                    record.EndpointIp,
                    normalizedIp,
                    StringComparison.Ordinal)
                || record.EndpointPort != endpointPort)
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked before TCP because the endpoint does not "
                    + "match the durable motion recovery record.");
            }
        }

        private async Task EnsureMotionRecoveryConnectionIdentityAsync(
            string operation)
        {
            if (!motionMayBeActive
                && !HasActiveMotionUncertaintyJournalRecord)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord(operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableMotionRecoveryIdentity(operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                record.TargetKind,
                record.TargetName,
                record.TargetReference,
                record.Operation,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw CreateRecoveryConnectionIdentityMismatch(
                    operation,
                    "Motion",
                    record.DiagnosticsBootId,
                    record.MapRevision,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }
        }

        private void EnsureMotionRecoveryLookupAllowed(
            MotionUncertaintyTargetKind targetKind,
            string targetName)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord("Target lookup");
            if (record.TargetKind != targetKind
                || !string.Equals(
                    record.TargetName,
                    targetName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Only the exact recorded "
                    + record.TargetKind
                    + " target "
                    + record.TargetName
                    + " can be looked up during motion recovery. No lookup RPC was sent.");
            }
        }

        private void EnsureLoadedAxisMatchesMotionRecovery(
            LMCSingleAxis loadedAxis)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord(
                "Load Axis recovery");
            var capabilities = RequireStableMotionRecoveryIdentity(
                "Load Axis recovery");
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                MotionUncertaintyTargetKind.Axis,
                loadedAxis.AxisName,
                loadedAxis.AxisReference,
                record.Operation,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw new InvalidOperationException(
                    "The loaded axis does not match the durable endpoint, reference, "
                    + "BootId, and MapRevision motion recovery identity.");
            }
        }

        private void EnsureLoadedGroupMatchesMotionRecovery(
            LMCGroupAxis loadedGroup)
        {
            if (!motionMayBeActive)
            {
                return;
            }

            var record = RequireActiveMotionUncertaintyRecord(
                "Load Group recovery");
            var capabilities = RequireStableMotionRecoveryIdentity(
                "Load Group recovery");
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                MotionUncertaintyTargetKind.Group,
                loadedGroup.GroupName,
                loadedGroup.GroupReference,
                record.Operation,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision))
            {
                throw new InvalidOperationException(
                    "The loaded group does not match the durable endpoint, reference, "
                    + "BootId, and MapRevision motion recovery identity.");
            }
        }

        private void RememberMotionLookupIdentity(
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference)
        {
            var capabilities = diagnosticCapabilities;
            MotionLookupIdentity identity = null;
            if (capabilities != null
                && capabilities.DiagnosticsBootId != 0
                && capabilities.MapRevision != 0)
            {
                identity = new MotionLookupIdentity(
                    targetKind,
                    targetName,
                    targetReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }

            if (targetKind == MotionUncertaintyTargetKind.Axis)
            {
                axisMotionLookupIdentity = identity;
            }
            else
            {
                groupMotionLookupIdentity = identity;
            }

            if (identity == null)
            {
                WriteLog(
                    "Motion target was loaded without a stable diagnostics "
                    + "BootId/MapRevision. Refresh capabilities and reload the "
                    + "target before Move is allowed.");
            }
        }

        private MotionLookupIdentity RequireMotionLookupIdentity(
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation)
        {
            var identity = targetKind == MotionUncertaintyTargetKind.Axis
                ? axisMotionLookupIdentity
                : groupMotionLookupIdentity;
            if (identity == null
                || identity.TargetKind != targetKind
                || identity.TargetReference != targetReference
                || !string.Equals(
                    identity.TargetName,
                    targetName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    operation
                    + " requires the exact target to be loaded under a stable "
                    + "DiagnosticsBootId and MapRevision before Move.");
            }

            return identity;
        }

        private void ClearMotionLookupIdentity(
            MotionUncertaintyTargetKind targetKind)
        {
            if (targetKind == MotionUncertaintyTargetKind.Axis)
            {
                axisMotionLookupIdentity = null;
            }
            else
            {
                groupMotionLookupIdentity = null;
            }
        }

        private void ClearMotionLookupIdentities()
        {
            axisMotionLookupIdentity = null;
            groupMotionLookupIdentity = null;
        }

        private bool IsTrackedMotionTarget(LMCSingleAxis currentAxis)
        {
            return currentAxis != null
                && motionTargetKind == MotionUncertaintyTargetKind.Axis
                && motionTargetReference == currentAxis.AxisReference
                && IsTrackedMotionAxis(currentAxis.AxisName);
        }

        private bool IsTrackedMotionTarget(LMCGroupAxis currentGroup)
        {
            return currentGroup != null
                && motionTargetKind == MotionUncertaintyTargetKind.Group
                && motionTargetReference == currentGroup.GroupReference
                && IsTrackedMotionAxis(currentGroup.GroupName);
        }

        private bool IsMotionRecoveryTargetKind(
            MotionUncertaintyTargetKind targetKind)
        {
            return motionMayBeActive
                && motionTargetKind == targetKind
                && HasActiveMotionUncertaintyJournalRecord;
        }

        private void ApplyMotionUncertaintyRecord(
            MotionUncertaintyRecord record)
        {
            if (record == null || !record.IsActive)
            {
                return;
            }

            motionTrackingGeneration++;
            motionMayBeActive = true;
            motionAxisName = record.TargetName;
            motionOperation = record.Operation;
            motionWasObserved = false;
            motionTargetKind = record.TargetKind;
            motionTargetReference = record.TargetReference;

            if (TextRemoteIp != null)
            {
                TextRemoteIp.Text = record.EndpointIp;
            }

            if (TextRemotePort != null)
            {
                TextRemotePort.Text = record.EndpointPort.ToString(
                    CultureInfo.InvariantCulture);
            }

            if (record.TargetKind == MotionUncertaintyTargetKind.Axis
                && TextAxisName != null)
            {
                TextAxisName.Text = record.TargetName;
            }
            else if (record.TargetKind
                    == MotionUncertaintyTargetKind.Group
                && TextGroupName != null)
            {
                TextGroupName.Text = record.TargetName;
            }
        }

        private MotionUncertaintyRecord RequireActiveMotionUncertaintyRecord(
            string operation)
        {
            if (motionUncertaintyJournal == null)
            {
                throw CreateMotionUncertaintyJournalException(
                    operation,
                    null);
            }

            var record = motionUncertaintyJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the volatile motion latch has no matching "
                    + "active durable identity record.");
            }

            return record;
        }

        private void EnsureMotionUncertaintyJournalCanArm(string operation)
        {
            if (MotionUncertaintyJournalCanArm)
            {
                return;
            }

            throw CreateMotionUncertaintyJournalException(operation, null);
        }

        private LMCDiagnosticCapabilities RequireStableMotionRecoveryIdentity(
            string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires nonzero DiagnosticsBootId and MapRevision before "
                    + "Move can be durably armed or recovery can proceed.");
            }

            return capabilities;
        }

        private InvalidOperationException
            CreateMotionUncertaintyJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    motionUncertaintyJournalRuntimeError)
                ? motionUncertaintyJournalRuntimeError
                : (!string.IsNullOrEmpty(motionUncertaintyJournalOpenError)
                    ? motionUncertaintyJournalOpenError
                    : "An active or unavailable durable motion record blocks this operation.");
            return new InvalidOperationException(
                operation
                + " is blocked by the durable motion uncertainty journal. "
                + detail,
                innerException);
        }

        private string GetMotionUncertaintyJournalGuidance()
        {
            if (motionMayBeActive)
            {
                return "Reconnect to the exact recorded endpoint and identity, load "
                    + "only the recorded target, then use Stop or PowerOff and wait "
                    + "for stable safe-state proof. No Move is replayed.";
            }

            return "New Move commands are disabled because the durable motion "
                + "journal is unavailable: "
                + (!string.IsNullOrEmpty(motionUncertaintyJournalRuntimeError)
                    ? motionUncertaintyJournalRuntimeError
                    : motionUncertaintyJournalOpenError);
        }

        private void SetMotionUncertaintyJournalRuntimeError(
            string operation,
            Exception error)
        {
            motionUncertaintyJournalRuntimeError =
                operation
                + ": "
                + error.GetType().Name
                + ": "
                + error.Message;
            WriteLog(
                "Motion uncertainty journal faulted and remains fail-closed: "
                + motionUncertaintyJournalRuntimeError);
        }

        private static DateTime MotionRecoveryUtcNow(DateTime minimumUtc)
        {
            var now = DateTime.UtcNow;
            return now < minimumUtc ? minimumUtc : now;
        }

        private async Task CloseRejectedMotionRecoveryConnectionAsync(
            LMCConnection rejectedConnection)
        {
            if (rejectedConnection == null)
            {
                return;
            }

            if (ReferenceEquals(connection, rejectedConnection))
            {
                connection = null;
            }

            DetachConnection(rejectedConnection);
            ClearLoadedObjects();
            try
            {
                await rejectedConnection.CloseConnectionAsync(
                    CancellationToken.None);
            }
            catch (Exception closeError)
            {
                WriteLog(
                    "Rejected motion-recovery connection cleanup warning: "
                    + closeError.Message);
            }
            finally
            {
                rejectedConnection.Dispose();
                UpdateUiState();
            }
        }

        private sealed class MotionLookupIdentity
        {
            internal MotionLookupIdentity(
                MotionUncertaintyTargetKind targetKind,
                string targetName,
                ushort targetReference,
                uint diagnosticsBootId,
                uint mapRevision)
            {
                TargetKind = targetKind;
                TargetName = targetName;
                TargetReference = targetReference;
                DiagnosticsBootId = diagnosticsBootId;
                MapRevision = mapRevision;
            }

            internal MotionUncertaintyTargetKind TargetKind { get; private set; }
            internal string TargetName { get; private set; }
            internal ushort TargetReference { get; private set; }
            internal uint DiagnosticsBootId { get; private set; }
            internal uint MapRevision { get; private set; }

            internal bool Matches(
                MotionUncertaintyTargetKind targetKind,
                string targetName,
                ushort targetReference,
                uint diagnosticsBootId,
                uint mapRevision)
            {
                return TargetKind == targetKind
                    && TargetReference == targetReference
                    && string.Equals(
                        TargetName,
                        targetName,
                        StringComparison.Ordinal)
                    && DiagnosticsBootId == diagnosticsBootId
                    && MapRevision == mapRevision;
            }
        }
    }
}

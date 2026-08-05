using System;
using System.Globalization;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private string groupPowerRecoveryJournalDirectoryPath;
        private GroupPowerRecoveryJournal groupPowerRecoveryJournal;
        private string groupPowerRecoveryJournalOpenError;
        private string groupPowerRecoveryJournalRuntimeError;
        private bool groupPowerAcceptedRestartRecovery;
        private bool groupPowerRecoveryRequired;
        private bool groupPowerOffReplacementAllowed;
        private LMCGroupPowerStateWaitContinuation
            pendingGroupPowerStateWaitContinuation;

        private bool GroupPowerRecoveryJournalCanArm
        {
            get
            {
                return groupPowerRecoveryJournal != null
                    && string.IsNullOrEmpty(
                        groupPowerRecoveryJournalOpenError)
                    && string.IsNullOrEmpty(
                        groupPowerRecoveryJournalRuntimeError)
                    && !groupPowerRecoveryJournal.HasActiveRecord;
            }
        }

        private bool GroupPowerRecoveryJournalUnavailable
        {
            get
            {
                return groupPowerRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        groupPowerRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        groupPowerRecoveryJournalRuntimeError);
            }
        }

        private bool HasActiveGroupPowerRecoveryRecord
        {
            get
            {
                return groupPowerRecoveryJournal != null
                    && groupPowerRecoveryJournal.HasActiveRecord;
            }
        }

        private bool HasUnresolvedGroupPowerState()
        {
            return HasActiveGroupPowerRecoveryRecord
                || (pendingGroupPowerStateWaitContinuation != null
                    && pendingGroupPowerStateWaitContinuation.IsPending);
        }

        private void InitializeGroupPowerRecoveryJournal()
        {
            try
            {
                groupPowerRecoveryJournal =
                    groupPowerRecoveryJournalDirectoryPath == null
                        ? GroupPowerRecoveryJournal.OpenDefault()
                        : GroupPowerRecoveryJournal.Open(
                            groupPowerRecoveryJournalDirectoryPath);
                groupPowerRecoveryJournalOpenError = null;
                groupPowerRecoveryJournalRuntimeError = null;

                TryFinalizeCommittedGroupPowerRetirementAtStartup();

                var record = groupPowerRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    if (record.State
                        == GroupPowerRecoveryState.ArmedBeforeDispatch)
                    {
                        record = groupPowerRecoveryJournal
                            .PromoteToRecoveryRequired(
                                record.Identity,
                                MonotonicUtcNow(record.UpdatedUtc));
                    }

                    ApplyRecoveredGroupPowerRecord(record);
                }
            }
            catch (Exception error)
            {
                var journal = groupPowerRecoveryJournal;
                groupPowerRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                groupPowerRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                ClearGroupPowerSessionContinuation();
                WriteLog(
                    "Group Power recovery journal is unavailable; new Group "
                    + "Power On and Power Off sends are fail-closed: "
                    + groupPowerRecoveryJournalOpenError);
            }
        }

        private void DisposeGroupPowerRecoveryJournal()
        {
            var journal = groupPowerRecoveryJournal;
            groupPowerRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private async Task<GroupPowerRecoveryRecord>
            ArmGroupPowerRecoveryBeforeDispatchAsync(
                LMCGroupAxis currentGroup,
                bool expectedPowerOn)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            EnsureGroupPowerRecoveryJournalCanArm();
            var currentConnection = RequireConnection();
            if (diagnosticCapabilities == null
                || diagnosticCapabilities.DiagnosticsBootId == 0
                || diagnosticCapabilities.MapRevision == 0)
            {
                await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            }
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                expectedPowerOn ? "Group Power On" : "Group Power Off");
            try
            {
                var record = groupPowerRecoveryJournal.ArmBeforeDispatch(
                    expectedPowerOn,
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentGroup.GroupName,
                    currentGroup.GroupReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    DateTime.UtcNow);
                SetGroupPowerPendingDirection(expectedPowerOn);
                groupPowerAcceptedRestartRecovery = false;
                groupPowerRecoveryRequired = false;
                groupPowerOffReplacementAllowed = false;
                return record;
            }
            catch (Exception error)
            {
                SetGroupPowerRecoveryJournalRuntimeError(
                    "arm-before-dispatch",
                    error);
                throw CreateGroupPowerRecoveryJournalException(
                    expectedPowerOn ? "Group Power On" : "Group Power Off",
                    error);
            }
        }

        private void MarkGroupPowerAccepted(
            LMCGroupAxis currentGroup,
            LMCGroupPowerStateWaitContinuation continuation,
            string operation)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (continuation == null
                || !continuation.IsPending
                || continuation.ExpectedPowerOn != record.ExpectedPowerOn
                || continuation.GroupReference != record.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    record.GroupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted Group Power continuation "
                    + "that does not match the durable group identity.");
            }

            try
            {
                if (record.State
                        == GroupPowerRecoveryState.ArmedBeforeDispatch
                    || (record.State
                            == GroupPowerRecoveryState.RecoveryRequired
                        && !record.ExpectedPowerOn))
                {
                    record = groupPowerRecoveryJournal.MarkAccepted(
                        record.Identity,
                        MonotonicUtcNow(record.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                RecordGroupPowerRecoveryJournalRuntimeError(
                    "mark-accepted",
                    error);
                throw CreateGroupPowerRecoveryJournalException(
                    operation,
                    error);
            }

            var continuationReusable = connection != null
                && connection.IsConnected
                && ReferenceEquals(group, currentGroup)
                && ReferenceEquals(
                    currentGroup.PendingGroupPowerStateWaitContinuation,
                    continuation)
                && continuation.IsPending;
            pendingGroupPowerStateWaitContinuation = continuationReusable
                ? continuation
                : null;
            SetGroupPowerPendingDirection(record.ExpectedPowerOn);
            groupPowerAcceptedRestartRecovery = !continuationReusable;
            groupPowerRecoveryRequired = false;
            groupPowerOffReplacementAllowed = false;
        }

        private void PromoteGroupPowerDispatchOutcomeUncertain(
            string reason)
        {
            var journal = groupPowerRecoveryJournal;
            try
            {
                if (journal != null)
                {
                    var record = journal.CurrentRecord;
                    if (record != null
                        && record.IsActive
                        && record.State
                            == GroupPowerRecoveryState.ArmedBeforeDispatch)
                    {
                        record = journal.PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                    }
                }
            }
            catch (Exception error)
            {
                SetGroupPowerRecoveryJournalRuntimeError(
                    "promote-to-recovery",
                    error);
            }

            var active = journal == null ? null : journal.CurrentRecord;
            if (active == null || !active.IsActive)
            {
                return;
            }

            pendingGroupPowerStateWaitContinuation = null;
            SetGroupPowerPendingDirection(active.ExpectedPowerOn);
            groupPowerAcceptedRestartRecovery = active.State
                == GroupPowerRecoveryState.AcceptedAwaitingProof;
            groupPowerRecoveryRequired = active.State
                == GroupPowerRecoveryState.RecoveryRequired;
            groupPowerOffReplacementAllowed = groupPowerRecoveryRequired
                && active.ExpectedPowerOn;
            WriteLog(
                "SAFETY: "
                + reason
                + " retained the durable Group Power "
                + (active.ExpectedPowerOn ? "On" : "Off")
                + " recovery interlock. No power command will be replayed.");
        }

        private void PreserveGroupPowerRecoveryAfterConnectionLoss(
            string reason)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                ClearGroupPowerSessionContinuation();
                return;
            }

            var record = groupPowerRecoveryJournal.CurrentRecord;
            var preserveConfirmedPowerOffReplacement =
                ShouldPreserveConfirmedPowerOffReplacement(record);
            if (record.State == GroupPowerRecoveryState.ArmedBeforeDispatch)
            {
                PromoteGroupPowerDispatchOutcomeUncertain(reason);
                return;
            }

            pendingGroupPowerStateWaitContinuation = null;
            SetGroupPowerPendingDirection(record.ExpectedPowerOn);
            groupPowerAcceptedRestartRecovery = record.State
                == GroupPowerRecoveryState.AcceptedAwaitingProof;
            groupPowerRecoveryRequired = record.State
                == GroupPowerRecoveryState.RecoveryRequired;
            groupPowerOffReplacementAllowed =
                preserveConfirmedPowerOffReplacement
                || (groupPowerRecoveryRequired && record.ExpectedPowerOn);
            WriteLog(
                reason
                + " invalidated the session-bound Group Power continuation. "
                + "Exact-identity recovery remains status-only; no 0x204A or "
                + "0x204B replay is automatic.");
        }

        private async Task EnsureGroupPowerRecoveryConnectionIdentityAsync(
            string operation)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                record.GroupName,
                record.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw CreateRecoveryConnectionIdentityMismatch(
                    operation,
                    "Group Power",
                    record.DiagnosticsBootId,
                    record.MapRevision,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }
        }

        private async Task EnsureGroupPowerRecoveryMutationIdentityAsync(
            LMCGroupAxis currentGroup,
            string operation)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentGroup.GroupName,
                currentGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because endpoint, group reference, BootId, "
                    + "or MapRevision does not match the durable Group Power "
                    + "record. No recovery mutation was sent.");
            }
        }

        private void EnsureGroupPowerRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord("Reconnect");
            if (!record.MatchesEndpoint(endpointIp, endpointPort))
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked because the PLC endpoint does not "
                    + "match the durable Group Power recovery record. No TCP "
                    + "connection was opened.");
            }
        }

        private void EnsureGroupPowerRecoveryLookupAllowed(string groupName)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(
                "Load Group recovery");
            if (!string.Equals(
                record.GroupName,
                groupName,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A different group cannot be loaded while Group Power "
                    + "recovery is active. No lookup RPC was sent.");
            }
        }

        private void EnsureLoadedGroupMatchesPowerRecovery(
            LMCGroupAxis loadedGroup)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(
                "Load Group recovery");
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                "Load Group recovery");
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                loadedGroup.GroupName,
                loadedGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    "The loaded group does not match the durable endpoint, "
                    + "group reference, BootId, and MapRevision recovery identity.");
            }
        }

        private void EnsureCurrentGroupMatchesPowerRecovery(
            LMCGroupAxis currentGroup,
            string operation)
        {
            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentGroup.GroupName,
                currentGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot apply proof because the current group identity "
                    + "does not match the durable Group Power record.");
            }
        }

        private async Task<GroupPowerRecoveryRecord>
            ReplaceUncertainGroupPowerOnWithPowerOffBeforeDispatchAsync(
                LMCGroupAxis currentGroup,
                string operation)
        {
            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (!record.ExpectedPowerOn)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires an active Group Power On recovery record.");
            }

            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupPowerRecoveryIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentGroup.GroupName,
                currentGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                true))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because endpoint, group reference, BootId, "
                    + "or MapRevision does not match the durable Group Power On "
                    + "record. No 0x204B was sent.");
            }

            try
            {
                var replacement = groupPowerRecoveryJournal
                    .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                        record.Identity,
                        RequiredConnectedRemoteIp(),
                        RequiredConnectedRemotePort(),
                        currentGroup.GroupName,
                        currentGroup.GroupReference,
                        capabilities.DiagnosticsBootId,
                        capabilities.MapRevision,
                        MonotonicUtcNow(record.UpdatedUtc));
                pendingGroupPowerStateWaitContinuation = null;
                SetGroupPowerPendingDirection(false);
                groupPowerAcceptedRestartRecovery = false;
                groupPowerRecoveryRequired = false;
                groupPowerOffReplacementAllowed = false;
                WriteLog(
                    operation
                    + " atomically replaced the uncertain Power On target with "
                    + "a durable Power Off arm before 0x204B dispatch.");
                return replacement;
            }
            catch (Exception error)
            {
                SetGroupPowerRecoveryJournalRuntimeError(
                    "replace-PowerOn-with-PowerOff",
                    error);
                throw CreateGroupPowerRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private async Task<GroupPowerRecoveryRecord>
            PrepareConfirmedGroupPowerOffReplacementAsync(
                LMCGroupAxis currentGroup,
                string operation)
        {
            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (record.ExpectedPowerOn
                || !groupPowerOffReplacementAllowed
                || record.State
                    != GroupPowerRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked until typed interference or a successful "
                    + "exact-identity PowerOn=true status is confirmed.");
            }

            await EnsureGroupPowerRecoveryMutationIdentityAsync(
                currentGroup,
                operation);
            return RequireActiveGroupPowerRecoveryRecord(operation);
        }

        private void ConfirmGroupPowerOffReplacementAllowed(
            LMCGroupAxis currentGroup,
            string reason)
        {
            var record = RequireActiveGroupPowerRecoveryRecord(reason);
            if (record.ExpectedPowerOn)
            {
                return;
            }

            EnsureCurrentGroupMatchesPowerRecovery(currentGroup, reason);
            try
            {
                if (record.State
                        == GroupPowerRecoveryState.ArmedBeforeDispatch
                    || record.State
                        == GroupPowerRecoveryState.AcceptedAwaitingProof)
                {
                    record = groupPowerRecoveryJournal
                        .PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                SetGroupPowerRecoveryJournalRuntimeError(
                    "confirm-PowerOff-replacement",
                    error);
                throw CreateGroupPowerRecoveryJournalException(
                    reason,
                    error);
            }

            pendingGroupPowerStateWaitContinuation = null;
            SetGroupPowerPendingDirection(false);
            groupPowerAcceptedRestartRecovery = false;
            groupPowerRecoveryRequired = true;
            groupPowerOffReplacementAllowed = true;
            WriteLog(
                reason
                + " confirmed that the pending Group Power Off proof cannot be "
                + "used. An explicit Power Off Again is now allowed; automatic "
                + "0x204B replay remains forbidden.");
        }

        private void ObserveGroupPowerRecoveryStatus(
            LMCGroupAxis currentGroup,
            LMCGroupReadStatusResult status,
            string operation)
        {
            if (!HasActiveGroupPowerRecoveryRecord
                || status == null
                || !status.IsSuccess)
            {
                return;
            }

            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (!record.ExpectedPowerOn && status.IsPowerOn)
            {
                ConfirmGroupPowerOffReplacementAllowed(
                    currentGroup,
                    operation + " observed PowerOn=true");
            }
        }

        private async Task<LMCGroupPowerStateWaitResult>
            ResumeOrObserveGroupPowerStateAsync(
                LMCGroupAxis currentGroup,
                bool expectedPowerOn,
                string operation)
        {
            EnsureCurrentGroupMatchesPowerRecovery(
                currentGroup,
                operation);
            var continuation = pendingGroupPowerStateWaitContinuation;
            if (continuation != null
                && continuation.IsPending
                && continuation.ExpectedPowerOn == expectedPowerOn
                && ReferenceEquals(
                    currentGroup.PendingGroupPowerStateWaitContinuation,
                    continuation))
            {
                return await currentGroup
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        System.Threading.CancellationToken.None);
            }

            return await currentGroup.WaitForPowerStateAsync(
                expectedPowerOn,
                System.Threading.CancellationToken.None);
        }

        private void CompleteGroupPowerRecoveryAfterStableProof(
            LMCGroupAxis currentGroup,
            bool expectedPowerOn,
            LMCGroupPowerStateWaitResult result,
            GroupPowerRecoveryRecord verificationRecord,
            string operation)
        {
            if (result == null
                || result.FinalStatus == null
                || !result.FinalStatus.IsSuccess
                || result.FinalStatus.IsPowerOn != expectedPowerOn
                || result.StableSampleCount
                    < result.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve Group Power recovery without the exact "
                    + "stable target-state proof.");
            }

            EnsureCurrentGroupMatchesPowerRecovery(
                currentGroup,
                operation + " final identity");
            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (verificationRecord == null
                || record.Identity != verificationRecord.Identity)
            {
                ReapplyCurrentGroupPowerRecoveryState(
                    currentGroup,
                    record);
                throw new InvalidOperationException(
                    operation
                    + " result belongs to an older Group Power operation. "
                    + "The newer durable record was preserved unchanged.");
            }

            if (record.ExpectedPowerOn != expectedPowerOn)
            {
                throw new InvalidOperationException(
                    operation
                    + " proof direction does not match the durable Group Power "
                    + "record.");
            }

            if (expectedPowerOn
                && record.State
                    == GroupPowerRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve an outcome-uncertain Power On arm from "
                    + "status alone. Explicit Power Off recovery is required.");
            }

            ResolveGroupPowerRecoveryJournal(
                verificationRecord,
                operation);
        }

        private void PreserveGroupPowerWaitFailure(
            LMCGroupAxis currentGroup,
            Exception error,
            GroupPowerRecoveryRecord verificationRecord,
            bool powerOnToPowerOffTakeover,
            bool confirmedPowerOffReplacement,
            bool powerCommandDispatchStarted,
            LMCGroupPowerStateWaitContinuation priorContinuation,
            string operation)
        {
            if (verificationRecord != null)
            {
                var currentRecord = groupPowerRecoveryJournal == null
                    ? null
                    : groupPowerRecoveryJournal.CurrentRecord;
                if (currentRecord == null
                    || !currentRecord.IsActive
                    || currentRecord.Identity
                        != verificationRecord.Identity)
                {
                    if (currentRecord != null && currentRecord.IsActive)
                    {
                        ReapplyCurrentGroupPowerRecoveryState(
                            currentGroup,
                            currentRecord);
                    }
                    else
                    {
                        ClearGroupPowerSessionContinuation();
                    }

                    WriteLog(
                        operation
                        + " failure belongs to an older Group Power operation. "
                        + "The newer durable record was preserved unchanged.");
                    return;
                }
            }

            if (confirmedPowerOffReplacement
                && !powerCommandDispatchStarted)
            {
                pendingGroupPowerStateWaitContinuation =
                    priorContinuation != null && priorContinuation.IsPending
                        ? priorContinuation
                        : null;
                SetGroupPowerPendingDirection(false);
                groupPowerAcceptedRestartRecovery = false;
                groupPowerRecoveryRequired = true;
                groupPowerOffReplacementAllowed = true;
                WriteLog(
                    operation
                    + " failed before a replacement 0x204B dispatch started. "
                    + "The confirmed Power Off replacement permission and prior "
                    + "continuation were preserved.");
                return;
            }

            var evidence = GetGroupPowerWaitEvidence(error);
            var continuation = GetGroupPowerWaitContinuation(error);
            if (continuation == null
                && currentGroup != null)
            {
                var published = currentGroup
                    .PendingGroupPowerStateWaitContinuation;
                if (published != null && published.IsPending)
                {
                    continuation = published;
                }
            }
            if (continuation == null
                && pendingGroupPowerStateWaitContinuation != null
                && pendingGroupPowerStateWaitContinuation.IsPending)
            {
                continuation = pendingGroupPowerStateWaitContinuation;
            }
            if (continuation != null && continuation.IsPending)
            {
                pendingGroupPowerStateWaitContinuation = continuation;
                SetGroupPowerPendingDirection(
                    continuation.ExpectedPowerOn);
            }

            if (error is LMCGroupPowerInterferenceException
                && continuation != null)
            {
                if (continuation.ExpectedPowerOn)
                {
                    PromoteGroupPowerOnInterferenceToRecovery(
                        currentGroup,
                        continuation,
                        operation + " typed SDK interference");
                }
                else
                {
                    ConfirmGroupPowerOffReplacementAllowed(
                        currentGroup,
                        operation + " typed SDK interference");
                }
                return;
            }

            if (evidence != null
                && evidence.LastObservedStatus != null
                && evidence.LastObservedStatus.IsSuccess
                && evidence.LastObservedStatus.IsPowerOn
                && HasActiveGroupPowerRecoveryRecord
                && !groupPowerRecoveryJournal.CurrentRecord.ExpectedPowerOn)
            {
                ConfirmGroupPowerOffReplacementAllowed(
                    currentGroup,
                    operation + " observed exact PowerOn=true status");
                return;
            }

            var knownRejected = error is LMCGroupPowerRejectedException;
            var definitelyNotSent = evidence != null
                && !evidence.CommandMayHaveBeenSent;
            var preemptedBeforeWire = error is LMCSendPreemptedException
                && ((LMCSendPreemptedException)error).Phase
                    == LMCSendPreemptionPhase.BeforeWire;
            if ((knownRejected || definitelyNotSent || preemptedBeforeWire)
                && confirmedPowerOffReplacement)
            {
                pendingGroupPowerStateWaitContinuation =
                    priorContinuation != null && priorContinuation.IsPending
                        ? priorContinuation
                        : null;
                SetGroupPowerPendingDirection(false);
                groupPowerAcceptedRestartRecovery = false;
                groupPowerRecoveryRequired = true;
                groupPowerOffReplacementAllowed = true;
                WriteLog(
                    operation
                    + " replacement had a known no-effect outcome. The prior "
                    + "Power Off recovery record and explicit replacement "
                    + "permission were preserved.");
                return;
            }

            if ((knownRejected || definitelyNotSent || preemptedBeforeWire)
                && !powerOnToPowerOffTakeover
                && verificationRecord != null
                && verificationRecord.State
                    == GroupPowerRecoveryState.ArmedBeforeDispatch)
            {
                ResolveGroupPowerRecoveryJournal(
                    verificationRecord,
                    operation + " known no-effect outcome");
                return;
            }

            if (evidence != null
                && evidence.CommandMayHaveBeenSent
                && evidence.SubmissionOutcome
                    == LMCGroupPowerSubmissionOutcome.OutcomeUncertain)
            {
                pendingGroupPowerStateWaitContinuation = null;
                PromoteGroupPowerDispatchOutcomeUncertain(
                    operation + " dispatch outcome is uncertain");
                return;
            }

            if (continuation != null && continuation.IsPending)
            {
                groupPowerAcceptedRestartRecovery = false;
                groupPowerRecoveryRequired = false;
                groupPowerOffReplacementAllowed = false;
                return;
            }

            if (HasActiveGroupPowerRecoveryRecord)
            {
                PromoteGroupPowerDispatchOutcomeUncertain(
                    operation + " completion was not proven");
            }
        }

        private void ReapplyCurrentGroupPowerRecoveryState(
            LMCGroupAxis currentGroup,
            GroupPowerRecoveryRecord record)
        {
            var preserveConfirmedPowerOffReplacement =
                ShouldPreserveConfirmedPowerOffReplacement(record);
            var currentContinuation = currentGroup == null
                ? null
                : currentGroup.PendingGroupPowerStateWaitContinuation;
            pendingGroupPowerStateWaitContinuation =
                currentContinuation != null
                    && currentContinuation.IsPending
                    && currentContinuation.ExpectedPowerOn
                        == record.ExpectedPowerOn
                    && currentContinuation.GroupReference
                        == record.GroupReference
                    && string.Equals(
                        currentContinuation.GroupName,
                        record.GroupName,
                        StringComparison.Ordinal)
                ? currentContinuation
                : null;
            SetGroupPowerPendingDirection(record.ExpectedPowerOn);
            groupPowerAcceptedRestartRecovery = record.State
                    == GroupPowerRecoveryState.AcceptedAwaitingProof
                && pendingGroupPowerStateWaitContinuation == null;
            groupPowerRecoveryRequired = record.State
                == GroupPowerRecoveryState.RecoveryRequired;
            groupPowerOffReplacementAllowed =
                preserveConfirmedPowerOffReplacement
                || (groupPowerRecoveryRequired && record.ExpectedPowerOn);
        }

        private void PromoteGroupPowerOnInterferenceToRecovery(
            LMCGroupAxis currentGroup,
            LMCGroupPowerStateWaitContinuation continuation,
            string reason)
        {
            var record = RequireActiveGroupPowerRecoveryRecord(reason);
            if (!record.ExpectedPowerOn
                || continuation == null
                || !continuation.ExpectedPowerOn
                || continuation.GroupReference != record.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    record.GroupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    reason
                    + " does not match the durable Group Power On identity.");
            }

            EnsureCurrentGroupMatchesPowerRecovery(currentGroup, reason);
            try
            {
                if (record.State
                    == GroupPowerRecoveryState.AcceptedAwaitingProof)
                {
                    record = groupPowerRecoveryJournal
                        .PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                }
            }
            catch (Exception journalError)
            {
                SetGroupPowerRecoveryJournalRuntimeError(
                    "PowerOn-interference",
                    journalError);
                throw CreateGroupPowerRecoveryJournalException(
                    reason,
                    journalError);
            }

            pendingGroupPowerStateWaitContinuation = null;
            SetGroupPowerPendingDirection(true);
            groupPowerAcceptedRestartRecovery = false;
            groupPowerRecoveryRequired = true;
            groupPowerOffReplacementAllowed = true;
            WriteLog(
                reason
                + " invalidated accepted Power On proof attribution. Power On "
                + "resume and replay are blocked; explicit Power Off takeover "
                + "is required.");
        }

        private static LMCGroupPowerStateWaitEvidence
            GetGroupPowerWaitEvidence(Exception error)
        {
            if (error is LMCGroupPowerStateWaitTimeoutException timeout)
            {
                return timeout.Evidence;
            }

            if (error is LMCGroupPowerStateWaitCanceledException canceled)
            {
                return canceled.Evidence;
            }

            if (error is LMCGroupPowerStateStatusException status)
            {
                return status.Evidence;
            }

            if (error is LMCGroupPowerRejectedException rejected)
            {
                return rejected.Evidence;
            }

            if (error is LMCGroupPowerSubmissionException submission)
            {
                return submission.Evidence;
            }

            if (error is LMCGroupPowerInterferenceException interference)
            {
                return interference.Evidence;
            }

            return null;
        }

        private static LMCGroupPowerStateWaitContinuation
            GetGroupPowerWaitContinuation(Exception error)
        {
            if (error is LMCGroupPowerStateWaitTimeoutException timeout)
            {
                return timeout.Continuation;
            }

            if (error is LMCGroupPowerStateWaitCanceledException canceled)
            {
                return canceled.Continuation;
            }

            if (error is LMCGroupPowerStateStatusException status)
            {
                return status.Continuation;
            }

            if (error is LMCGroupPowerInterferenceException interference)
            {
                return interference.Continuation;
            }

            if (error is LMCGroupPowerStateWaitPendingException pending)
            {
                return pending.Continuation;
            }

            if (error is LMCGroupPowerStateWaitResolvedException resolved)
            {
                return resolved.Continuation;
            }

            return null;
        }

        private void ResolveGroupPowerRecoveryJournal(
            GroupPowerRecoveryRecord verificationRecord,
            string operation)
        {
            var record = RequireActiveGroupPowerRecoveryRecord(operation);
            if (verificationRecord == null
                || record.Identity != verificationRecord.Identity)
            {
                ReapplyCurrentGroupPowerRecoveryState(group, record);
                throw new InvalidOperationException(
                    operation
                    + " belongs to an older Group Power operation. The newer "
                    + "durable record was preserved unchanged.");
            }

            try
            {
                groupPowerRecoveryJournal.Resolve(
                    verificationRecord.Identity,
                    MonotonicUtcNow(record.UpdatedUtc));
            }
            catch (Exception error)
            {
                SetGroupPowerRecoveryJournalRuntimeError("resolve", error);
                throw CreateGroupPowerRecoveryJournalException(
                    operation,
                    error);
            }

            ClearGroupPowerSessionContinuation();
            WriteLog(operation + " resolved the durable Group Power record.");
        }

        private void ApplyRecoveredGroupPowerRecord(
            GroupPowerRecoveryRecord record)
        {
            pendingGroupPowerStateWaitContinuation = null;
            SetGroupPowerPendingDirection(record.ExpectedPowerOn);
            groupPowerAcceptedRestartRecovery = record.State
                == GroupPowerRecoveryState.AcceptedAwaitingProof;
            groupPowerRecoveryRequired = record.State
                == GroupPowerRecoveryState.RecoveryRequired;
            groupPowerOffReplacementAllowed = groupPowerRecoveryRequired
                && record.ExpectedPowerOn;
            TextRemoteIp.Text = record.EndpointIp;
            TextRemotePort.Text = record.EndpointPort.ToString(
                CultureInfo.InvariantCulture);
            TextGroupName.Text = record.GroupName;

            if (groupPowerAcceptedRestartRecovery)
            {
                WriteLog(
                    "Recovered a durable accepted Group Power "
                    + (record.ExpectedPowerOn ? "On" : "Off")
                    + " ACK for "
                    + record.GroupName
                    + ". Reconnect to the exact identity and run status-only "
                    + "verification; the power command will not be replayed.");
            }
            else if (record.ExpectedPowerOn)
            {
                WriteLog(
                    "SAFETY: recovered an outcome-uncertain Group Power On for "
                    + record.GroupName
                    + ". Power On replay is blocked. Explicit Group Power Off "
                    + "takeover and stable PowerOn=false proof are required.");
            }
            else
            {
                WriteLog(
                    "SAFETY: recovered an outcome-uncertain Group Power Off for "
                    + record.GroupName
                    + ". First run status-only PowerOn=false verification. A "
                    + "new 0x204B is blocked until interference or PowerOn=true "
                    + "is confirmed.");
            }
        }

        private void SetGroupPowerPendingDirection(bool expectedPowerOn)
        {
            groupPowerVerificationPending = expectedPowerOn;
            groupPowerOffVerificationPending = !expectedPowerOn;
        }

        private void ClearGroupPowerSessionContinuation()
        {
            pendingGroupPowerStateWaitContinuation = null;
            var record = groupPowerRecoveryJournal == null
                ? null
                : groupPowerRecoveryJournal.CurrentRecord;
            if (record != null && record.IsActive)
            {
                var preserveConfirmedPowerOffReplacement =
                    ShouldPreserveConfirmedPowerOffReplacement(record);
                SetGroupPowerPendingDirection(record.ExpectedPowerOn);
                groupPowerAcceptedRestartRecovery = record.State
                    == GroupPowerRecoveryState.AcceptedAwaitingProof;
                groupPowerRecoveryRequired = record.State
                    == GroupPowerRecoveryState.RecoveryRequired;
                groupPowerOffReplacementAllowed =
                    preserveConfirmedPowerOffReplacement
                    || (groupPowerRecoveryRequired && record.ExpectedPowerOn);
                return;
            }

            groupPowerAcceptedRestartRecovery = false;
            groupPowerRecoveryRequired = false;
            groupPowerOffReplacementAllowed = false;
            groupPowerVerificationPending = false;
            groupPowerOffVerificationPending = false;
        }

        private bool ShouldPreserveConfirmedPowerOffReplacement(
            GroupPowerRecoveryRecord record)
        {
            return groupPowerOffReplacementAllowed
                && record != null
                && record.IsActive
                && !record.ExpectedPowerOn
                && record.State == GroupPowerRecoveryState.RecoveryRequired;
        }

        private GroupPowerRecoveryRecord
            RequireActiveGroupPowerRecoveryRecord(string operation)
        {
            var journal = groupPowerRecoveryJournal;
            var record = journal == null ? null : journal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because no matching active durable Group "
                    + "Power recovery record exists.");
            }

            return record;
        }

        private void EnsureGroupPowerRecoveryJournalCanArm()
        {
            if (GroupPowerRecoveryJournalCanArm)
            {
                return;
            }

            throw CreateGroupPowerRecoveryJournalException(
                "Group Power",
                null);
        }

        private LMCDiagnosticCapabilities
            RequireStableGroupPowerRecoveryIdentity(string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires nonzero DiagnosticsBootId and MapRevision "
                    + "before Group Power or a recovery mutation is allowed.");
            }

            return capabilities;
        }

        private InvalidOperationException
            CreateGroupPowerRecoveryJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    groupPowerRecoveryJournalRuntimeError)
                ? groupPowerRecoveryJournalRuntimeError
                : (!string.IsNullOrEmpty(groupPowerRecoveryJournalOpenError)
                    ? groupPowerRecoveryJournalOpenError
                    : "An active durable Group Power record blocks this operation.");
            return new InvalidOperationException(
                operation
                + " is blocked by the durable Group Power recovery journal. "
                + detail,
                innerException);
        }

        private void SetGroupPowerRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            RecordGroupPowerRecoveryJournalRuntimeError(operation, error);
            WriteLog(
                "Group Power recovery journal faulted and remains fail-closed: "
                + groupPowerRecoveryJournalRuntimeError);
        }

        private void RecordGroupPowerRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            groupPowerRecoveryJournalRuntimeError =
                operation
                + ": "
                + error.GetType().Name
                + ": "
                + error.Message;
        }

        private string GetGroupPowerRecoveryGuidance()
        {
            if (GroupPowerRecoveryJournalUnavailable)
            {
                return "The Group Power recovery journal is unavailable; new "
                    + "Group Power commands are disabled.";
            }

            if (!HasActiveGroupPowerRecoveryRecord)
            {
                return "No durable Group Power recovery record is active.";
            }

            var record = groupPowerRecoveryJournal.CurrentRecord;
            if (record.ExpectedPowerOn
                && record.State
                    == GroupPowerRecoveryState.RecoveryRequired)
            {
                return "Group Power On outcome is uncertain. Do not replay Power "
                    + "On; send Power Off explicitly and verify stable PowerOn=false.";
            }

            if (!record.ExpectedPowerOn
                && record.State
                    == GroupPowerRecoveryState.RecoveryRequired
                && !groupPowerOffReplacementAllowed)
            {
                return "Group Power Off outcome is uncertain. Run status-only "
                    + "PowerOn=false verification before considering 0x204B again.";
            }

            return "An accepted Group Power transition is awaiting exact-identity "
                + "status-only verification; do not replay its power command.";
        }
    }
}

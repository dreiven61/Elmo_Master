using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly string
            groupProfileLockRecoveryJournalDirectoryPath;
        private GroupProfileLockRecoveryJournal
            groupProfileLockRecoveryJournal;
        private string groupProfileLockRecoveryJournalOpenError;
        private string groupProfileLockRecoveryJournalRuntimeError;
        private bool groupProfileLockRecoveryRecoveredAtStartup;
        private bool groupProfileLockAcceptedRestartRecovery;
        private bool groupProfileUnlockAcceptedRestartRecovery;
        private ushort groupProfileLockRecoveryGroupReference;
        private string groupProfileLockRecoveryEndpointIp;
        private int groupProfileLockRecoveryEndpointPort;
        private uint groupProfileLockRecoveryDiagnosticsBootId;
        private uint groupProfileLockRecoveryMapRevision;
        private string connectedRemoteIp;
        private int connectedRemotePort;

        private bool GroupProfileLockRecoveryJournalCanArm
        {
            get
            {
                return groupProfileLockRecoveryJournal != null
                    && string.IsNullOrEmpty(
                        groupProfileLockRecoveryJournalOpenError)
                    && string.IsNullOrEmpty(
                        groupProfileLockRecoveryJournalRuntimeError)
                    && !groupProfileLockRecoveryJournal.HasActiveRecord;
            }
        }

        private bool GroupProfileLockRecoveryJournalUnavailable
        {
            get
            {
                return groupProfileLockRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        groupProfileLockRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        groupProfileLockRecoveryJournalRuntimeError);
            }
        }

        private bool HasActiveGroupProfileLockRecoveryJournalRecord
        {
            get
            {
                return groupProfileLockRecoveryJournal != null
                    && groupProfileLockRecoveryJournal.HasActiveRecord;
            }
        }

        private bool HasAcceptedGroupProfileLockRecoveryRecord
        {
            get
            {
                if (!groupProfileLockAcceptedRestartRecovery
                    || groupProfileLockRecoveryJournal == null)
                {
                    return false;
                }

                var record = groupProfileLockRecoveryJournal.CurrentRecord;
                return record != null
                    && record.IsActive
                    && record.ExpectedProfileLocked
                    && record.State
                        == GroupProfileLockRecoveryState
                            .AcceptedAwaitingProof;
            }
        }

        private bool HasAcceptedGroupProfileUnlockRecoveryRecord
        {
            get
            {
                if (!groupProfileUnlockAcceptedRestartRecovery
                    || groupProfileLockRecoveryJournal == null)
                {
                    return false;
                }

                var record = groupProfileLockRecoveryJournal.CurrentRecord;
                return record != null
                    && record.IsActive
                    && !record.ExpectedProfileLocked
                    && record.State
                        == GroupProfileLockRecoveryState
                            .AcceptedAwaitingProof;
            }
        }

        private void InitializeGroupProfileLockRecoveryJournal()
        {
            try
            {
                groupProfileLockRecoveryJournal =
                    groupProfileLockRecoveryJournalDirectoryPath == null
                        ? GroupProfileLockRecoveryJournal.OpenDefault()
                        : GroupProfileLockRecoveryJournal.Open(
                            groupProfileLockRecoveryJournalDirectoryPath);
                groupProfileLockRecoveryJournalOpenError = null;
                groupProfileLockRecoveryJournalRuntimeError = null;

                var record = groupProfileLockRecoveryJournal.CurrentRecord;
                groupProfileLockRecoveryRecoveredAtStartup =
                    record != null && record.IsActive;
                if (groupProfileLockRecoveryRecoveredAtStartup)
                {
                    if (record.State
                        == GroupProfileLockRecoveryState.ArmedBeforeDispatch)
                    {
                        record = groupProfileLockRecoveryJournal
                            .PromoteToRecoveryRequired(
                                record.Identity,
                                MonotonicUtcNow(record.UpdatedUtc));
                    }

                    ApplyGroupProfileLockRecoveryRecord(record);
                }
            }
            catch (Exception error)
            {
                var journal = groupProfileLockRecoveryJournal;
                groupProfileLockRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                groupProfileLockRecoveryRecoveredAtStartup = false;
                groupProfileLockAcceptedRestartRecovery = false;
                groupProfileUnlockAcceptedRestartRecovery = false;
                groupProfileLockRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "Group profile-lock recovery journal is unavailable; "
                    + "new Group Enable is fail-closed: "
                    + groupProfileLockRecoveryJournalOpenError);
            }
        }

        private void DisposeGroupProfileLockRecoveryJournal()
        {
            var journal = groupProfileLockRecoveryJournal;
            groupProfileLockRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private async Task<GroupProfileLockRecoveryRecord>
            ArmGroupProfileLockRecoveryBeforeEnableAsync(
                LMCGroupAxis currentGroup)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            EnsureGroupProfileLockRecoveryJournalCanArm();
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupProfileLockIdentity(
                "Group Enable");
            var endpointIp = RequiredConnectedRemoteIp();
            var endpointPort = RequiredConnectedRemotePort();

            try
            {
                return groupProfileLockRecoveryJournal.ArmBeforeDispatch(
                    true,
                    endpointIp,
                    endpointPort,
                    currentGroup.GroupName,
                    currentGroup.GroupReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    DateTime.UtcNow);
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "arm-before-Enable",
                    error);
                throw CreateGroupProfileLockRecoveryJournalException(
                    "Group Enable",
                    error);
            }
        }

        private async Task<GroupProfileLockRecoveryRecord>
            ArmGroupProfileLockRecoveryBeforeDisableAsync(
                LMCGroupAxis currentGroup)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            EnsureGroupProfileLockRecoveryJournalAvailableForMutation(
                "Group Disable");
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupProfileLockIdentity(
                "Group Disable");
            var endpointIp = RequiredConnectedRemoteIp();
            var endpointPort = RequiredConnectedRemotePort();

            try
            {
                var current = groupProfileLockRecoveryJournal == null
                    ? null
                    : groupProfileLockRecoveryJournal.CurrentRecord;
                if (current != null && current.IsActive)
                {
                    if (!current.ExpectedProfileLocked
                        || !current.MatchesRecoveryIdentity(
                            endpointIp,
                            endpointPort,
                            currentGroup.GroupName,
                            currentGroup.GroupReference,
                            capabilities.DiagnosticsBootId,
                            capabilities.MapRevision,
                            true))
                    {
                        throw new InvalidOperationException(
                            "Group Disable cannot replace a durable record unless it "
                            + "is the exact active Lock identity.");
                    }

                    var replacement = groupProfileLockRecoveryJournal
                        .ReplaceActiveLockWithUnlockBeforeDispatch(
                            current.Identity,
                            endpointIp,
                            endpointPort,
                            currentGroup.GroupName,
                            currentGroup.GroupReference,
                            capabilities.DiagnosticsBootId,
                            capabilities.MapRevision,
                            MonotonicUtcNow(current.UpdatedUtc));
                    groupProfileLocked = false;
                    return replacement;
                }

                EnsureGroupProfileLockRecoveryJournalCanArm(
                    "Group Disable");
                var armed = groupProfileLockRecoveryJournal.ArmBeforeDispatch(
                    false,
                    endpointIp,
                    endpointPort,
                    currentGroup.GroupName,
                    currentGroup.GroupReference,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    DateTime.UtcNow);
                groupProfileLocked = false;
                return armed;
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "arm-before-Disable",
                    error);
                throw CreateGroupProfileLockRecoveryJournalException(
                    "Group Disable",
                    error);
            }
        }

        private async Task EnsureGroupProfileLockRecoveryIdentityAsync(
            LMCGroupAxis currentGroup,
            string operation,
            bool expectedProfileLocked)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            GroupProfileLockRecoveryRecord record = null;
            if (groupProfileLockRecoveryJournal != null)
            {
                record = groupProfileLockRecoveryJournal.CurrentRecord;
            }

            if (record == null || !record.IsActive)
            {
                if (groupProfileLockRecoveryRequired)
                {
                    throw new InvalidOperationException(
                        operation
                        + " is blocked because the volatile recovery latch has no "
                        + "matching durable identity record.");
                }

                return;
            }

            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupProfileLockIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                currentGroup.GroupName,
                currentGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                expectedProfileLocked))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because endpoint, group reference, BootId, "
                    + "or MapRevision does not match the durable recovery record. "
                    + "No recovery mutation was sent.");
            }
        }

        private void MarkGroupProfileLockAccepted(
            LMCGroupAxis currentGroup,
            LMCGroupEnableWaitContinuation continuation,
            string operation)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            var record = RequireActiveGroupProfileLockRecoveryRecord(
                operation);
            if (!record.ExpectedProfileLocked
                || continuation == null
                || !continuation.IsPending
                || continuation.GroupReference != record.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    record.GroupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted Group Enable continuation "
                    + "that does not match the durable group identity.");
            }

            try
            {
                if (record.State
                    == GroupProfileLockRecoveryState.ArmedBeforeDispatch)
                {
                    record = groupProfileLockRecoveryJournal.MarkAccepted(
                        record.Identity,
                        MonotonicUtcNow(record.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "mark-accepted",
                    error);
                throw CreateGroupProfileLockRecoveryJournalException(
                    operation,
                    error);
            }

            var continuationReusable = connection != null
                && connection.IsConnected
                && ReferenceEquals(group, currentGroup)
                && ReferenceEquals(
                    currentGroup.PendingGroupEnableWaitContinuation,
                    continuation)
                && continuation.IsPending;
            pendingGroupEnableWaitContinuation = continuationReusable
                ? continuation
                : null;
            groupProfileLockAcceptedRestartRecovery =
                !continuationReusable;
            groupProfileUnlockAcceptedRestartRecovery = false;
            groupProfileLockVerificationPending = true;
            groupProfileUnlockVerificationPending = false;
            groupProfileLockRecoveryRequired = false;
            groupProfileLockRecoveryGroupName = record.GroupName;
            groupProfileLockRecoveryGroupReference = record.GroupReference;
            groupProfileLockRecoveryEndpointIp = record.EndpointIp;
            groupProfileLockRecoveryEndpointPort = record.EndpointPort;
            groupProfileLockRecoveryDiagnosticsBootId =
                record.DiagnosticsBootId;
            groupProfileLockRecoveryMapRevision = record.MapRevision;
            if (record.ExpectedProfileLocked)
            {
                groupProfileLocked = false;
            }
        }

        private void MarkGroupProfileUnlockAccepted(
            LMCGroupAxis currentGroup,
            LMCGroupDisableWaitContinuation continuation,
            string operation)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            var record = RequireActiveGroupProfileLockRecoveryRecord(
                operation);
            if (record.ExpectedProfileLocked
                || continuation == null
                || !continuation.IsPending
                || continuation.GroupReference != record.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    record.GroupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted Group Disable continuation "
                    + "that does not match the durable unlock identity.");
            }

            try
            {
                if (record.State
                        == GroupProfileLockRecoveryState.ArmedBeforeDispatch
                    || record.State
                        == GroupProfileLockRecoveryState.RecoveryRequired)
                {
                    record = groupProfileLockRecoveryJournal.MarkAccepted(
                        record.Identity,
                        MonotonicUtcNow(record.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "mark-disable-accepted",
                    error);
                throw CreateGroupProfileLockRecoveryJournalException(
                    operation,
                    error);
            }

            var continuationReusable = connection != null
                && connection.IsConnected
                && ReferenceEquals(group, currentGroup)
                && ReferenceEquals(
                    currentGroup.PendingGroupDisableWaitContinuation,
                    continuation)
                && continuation.IsPending;
            pendingGroupDisableWaitContinuation = continuationReusable
                ? continuation
                : null;
            groupProfileUnlockAcceptedRestartRecovery =
                !continuationReusable;
            groupProfileUnlockVerificationPending = true;
            pendingGroupEnableWaitContinuation = null;
            groupProfileLockAcceptedRestartRecovery = false;
            groupProfileLockVerificationPending = false;
            groupProfileLockRecoveryRequired = false;
            groupProfileLockRecoveryGroupName = record.GroupName;
            groupProfileLockRecoveryGroupReference = record.GroupReference;
            groupProfileLockRecoveryEndpointIp = record.EndpointIp;
            groupProfileLockRecoveryEndpointPort = record.EndpointPort;
            groupProfileLockRecoveryDiagnosticsBootId =
                record.DiagnosticsBootId;
            groupProfileLockRecoveryMapRevision = record.MapRevision;
            groupProfileLocked = false;
        }

        private LMCGroupDisableWaitContinuation
            GetPendingGroupDisableWaitContinuation(
                LMCGroupAxis currentGroup)
        {
            if (currentGroup == null)
            {
                pendingGroupDisableWaitContinuation = null;
                groupProfileUnlockVerificationPending =
                    groupProfileUnlockAcceptedRestartRecovery;
                return null;
            }

            pendingGroupDisableWaitContinuation = currentGroup
                .PendingGroupDisableWaitContinuation;
            groupProfileUnlockVerificationPending =
                groupProfileUnlockAcceptedRestartRecovery
                || pendingGroupDisableWaitContinuation != null;
            return pendingGroupDisableWaitContinuation;
        }

        private void PreservePendingGroupDisableWaitUi(
            LMCGroupAxis currentGroup,
            LMCGroupDisableWaitContinuation continuation,
            string reason)
        {
            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Preserve accepted Group Disable");
            if (record.ExpectedProfileLocked
                || record.State
                    != GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                throw new InvalidOperationException(
                    "An accepted Group Disable cannot be preserved without an "
                    + "AcceptedAwaitingProof unlock journal.");
            }

            var continuationReusable = currentGroup != null
                && continuation != null
                && continuation.IsPending
                && connection != null
                && connection.IsConnected
                && ReferenceEquals(group, currentGroup)
                && ReferenceEquals(
                    currentGroup.PendingGroupDisableWaitContinuation,
                    continuation);
            pendingGroupDisableWaitContinuation = continuationReusable
                ? continuation
                : null;
            groupProfileUnlockAcceptedRestartRecovery =
                !continuationReusable;
            groupProfileUnlockVerificationPending = true;
            groupProfileLockAcceptedRestartRecovery = false;
            groupProfileLockVerificationPending = false;
            groupProfileLockRecoveryRequired = false;
            groupProfileLocked = false;

            if (continuation != null
                && continuation.LastObservedStatus != null)
            {
                DisplayGroupStatus(continuation.LastObservedStatus);
            }
            TextGroupResult.Text += Environment.NewLine
                + "GroupDisable ACK is durably accepted; no automatic 0x2048 "
                + "replay is allowed. Resume status-only verification."
                + Environment.NewLine
                + "Pending reason: "
                + reason;
        }

        private void CompleteGroupDisableWaitUi(
            LMCGroupDisableWaitResult result)
        {
            ValidateStableGroupDisabledProof(
                result == null ? null : result.FinalStatus,
                result == null ? 0 : result.StableDisabledSampleCount,
                result == null ? 0 : result.RequiredStableSampleCount,
                "Group Disable");
            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Group Disable stable proof");
            if (record.ExpectedProfileLocked
                || record.State
                    != GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                throw new InvalidOperationException(
                    "Group Disable proof cannot resolve a non-accepted unlock journal.");
            }

            ResolveGroupProfileLockRecoveryJournal(
                "Group Disable verified stable Disabled");
            pendingGroupDisableWaitContinuation = null;
            pendingGroupEnableWaitContinuation = null;
            groupProfileUnlockVerificationPending = false;
            groupProfileLockVerificationPending = false;
            ClearGroupProfileLockRecovery();
            groupProfileLocked = false;
            groupStatusRefreshRequired = false;
            groupActiveVerified = true;
            DisplayGroupStatus(result.FinalStatus);
            TextGroupResult.Text += Environment.NewLine
                + "GroupDisable ACK accepted; Status polls="
                + result.StatusPollCount
                + ", StableDisabled="
                + result.StableDisabledSampleCount
                + "/"
                + result.RequiredStableSampleCount
                + ".";
        }

        private void CompleteGroupDisableStatusOnlyRecoveryUi(
            LMCGroupStableDisabledWaitResult result)
        {
            var required = result == null || result.Evidence == null
                ? 0
                : result.Evidence.RequiredStableSampleCount;
            ValidateStableGroupDisabledProof(
                result == null ? null : result.FinalStatus,
                result == null ? 0 : result.StableDisabledSampleCount,
                required,
                "Accepted Group Disable status-only recovery");
            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Accepted Group Disable status-only recovery");
            if (record.ExpectedProfileLocked
                || record.State
                    != GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                throw new InvalidOperationException(
                    "Status-only Disabled proof cannot resolve a journal that is "
                    + "not an accepted unlock operation.");
            }

            ResolveGroupProfileLockRecoveryJournal(
                "Accepted Group Disable status-only recovery");
            pendingGroupDisableWaitContinuation = null;
            pendingGroupEnableWaitContinuation = null;
            groupProfileUnlockVerificationPending = false;
            groupProfileLockVerificationPending = false;
            ClearGroupProfileLockRecovery();
            groupProfileLocked = false;
            groupStatusRefreshRequired = false;
            groupActiveVerified = true;
            DisplayGroupStatus(result.FinalStatus);
            TextGroupResult.Text += Environment.NewLine
                + "Accepted GroupDisable recovered with status-only proof; "
                + "0x2048 requests=0, Status polls="
                + result.StatusPollCount
                + ", StableDisabled="
                + result.StableDisabledSampleCount
                + "/"
                + required
                + ".";
        }

        private static void ValidateStableGroupDisabledProof(
            LMCGroupReadStatusResult finalStatus,
            int stableSampleCount,
            int requiredStableSampleCount,
            string operation)
        {
            if (finalStatus == null
                || !finalStatus.IsSuccess
                || !finalStatus.IsPowerOn
                || !finalStatus.IsDisabled
                || finalStatus.IsStandby
                || requiredStableSampleCount < 1
                || stableSampleCount < requiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    operation
                    + " returned no stable PowerOn + Disabled + !Standby proof.");
            }
        }

        private async Task RetirePendingGroupDisableAfterStablePowerOffAsync(
            LMCGroupAxis currentGroup,
            LMCGroupPowerStateWaitResult powerOffResult,
            string operation)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            var pendingDisable = currentGroup
                .PendingGroupDisableWaitContinuation;
            if (pendingDisable == null)
            {
                pendingGroupDisableWaitContinuation = null;
                return;
            }

            var powerOffContinuation = powerOffResult == null
                ? null
                : powerOffResult.Continuation;
            if (powerOffContinuation == null
                || !powerOffContinuation.IsCompleted
                || powerOffContinuation.ExpectedPowerOn)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot retire a pending Group Disable without the exact "
                    + "same-session completed Power Off continuation. The unlock "
                    + "journal remains unresolved.");
            }

            const int maximumRetireAttempts = 200;
            for (var attempt = 0; attempt < maximumRetireAttempts; attempt++)
            {
                if (currentGroup
                    .TryRetirePendingGroupDisableAfterStablePowerOff(
                        pendingDisable,
                        powerOffContinuation))
                {
                    pendingGroupDisableWaitContinuation = null;
                    groupProfileUnlockVerificationPending = false;
                    WriteLog(
                        operation
                        + " retired the superseded same-session Group Disable continuation "
                        + "without replaying 0x2048.");
                    return;
                }

                var latestPending = currentGroup
                    .PendingGroupDisableWaitContinuation;
                if (latestPending == null
                    && (pendingDisable.IsCompleted
                        || pendingDisable.IsSuperseded))
                {
                    pendingGroupDisableWaitContinuation = null;
                    groupProfileUnlockVerificationPending = false;
                    return;
                }
                if (latestPending != null
                    && !ReferenceEquals(latestPending, pendingDisable))
                {
                    break;
                }

                await Task.Delay(10);
            }

            throw new InvalidOperationException(
                operation
                + " failed to retire the exact pending Group Disable after "
                + "stable Power Off proof. The unlock journal remains unresolved.");
        }

        private void
            ReapplyActiveGroupProfileLockRecoveryAfterPowerOffFailure(
                string operation)
        {
            var journal = groupProfileLockRecoveryJournal;
            if (journal == null)
            {
                groupProfileLocked = false;
                return;
            }

            try
            {
                var record = journal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    ApplyGroupProfileLockRecoveryRecord(record);
                    WriteLog(
                        operation
                        + " reapplied the still-active durable profile-lock "
                        + "record after Power Off follow-up failed. Recovery "
                        + "admission remains fail-closed without 0x2048 replay.");
                }
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "reapply-after-PowerOff-failure",
                    error);
            }

            groupProfileLocked = false;
        }

        private void EnsureGroupProfileLockRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            if (!groupProfileLockRecoveryRequired
                && !HasActiveGroupProfileLockRecoveryJournalRecord)
            {
                return;
            }

            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Reconnect");
            var normalizedIp = NormalizeIPv4(endpointIp);
            if (!string.Equals(
                    record.EndpointIp,
                    normalizedIp,
                    StringComparison.Ordinal)
                || record.EndpointPort != endpointPort)
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked because the PLC endpoint does not match "
                    + "the durable group profile-lock recovery record. No RPC was sent.");
            }
        }

        private async Task
            EnsureGroupProfileLockRecoveryConnectionIdentityAsync(
                string operation)
        {
            if (!groupProfileLockRecoveryRequired
                && !HasActiveGroupProfileLockRecoveryJournalRecord)
            {
                return;
            }

            var record = RequireActiveGroupProfileLockRecoveryRecord(
                operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupProfileLockIdentity(
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                record.GroupName,
                record.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedProfileLocked))
            {
                throw CreateRecoveryConnectionIdentityMismatch(
                    operation,
                    "Group Profile Lock",
                    record.DiagnosticsBootId,
                    record.MapRevision,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision);
            }
        }

        private void EnsureLoadedGroupMatchesProfileLockRecovery(
            LMCGroupAxis loadedGroup)
        {
            if (!groupProfileLockRecoveryRequired
                && !HasActiveGroupProfileLockRecoveryJournalRecord)
            {
                return;
            }

            if (loadedGroup == null)
            {
                throw new ArgumentNullException("loadedGroup");
            }

            var record = RequireActiveGroupProfileLockRecoveryRecord(
                "Load Group recovery");
            var capabilities = RequireStableGroupProfileLockIdentity(
                "Load Group recovery");
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                loadedGroup.GroupName,
                loadedGroup.GroupReference,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.ExpectedProfileLocked))
            {
                throw new InvalidOperationException(
                    "The loaded group does not match the durable endpoint, reference, "
                    + "BootId, and MapRevision recovery identity. No recovery mutation "
                    + "is allowed.");
            }
        }

        private void PromoteGroupProfileLockRecoveryJournal(
            string reason,
            bool promoteAcceptedAwaitingProof = false)
        {
            var journal = groupProfileLockRecoveryJournal;
            GroupProfileLockRecoveryRecord record = null;
            try
            {
                if (journal != null)
                {
                    record = journal.CurrentRecord;
                    if (record != null
                        && record.IsActive
                        && (record.State
                                == GroupProfileLockRecoveryState
                                    .ArmedBeforeDispatch
                            || (promoteAcceptedAwaitingProof
                                && record.State
                                    == GroupProfileLockRecoveryState
                                        .AcceptedAwaitingProof)))
                    {
                        record = journal.PromoteToRecoveryRequired(
                            record.Identity,
                            MonotonicUtcNow(record.UpdatedUtc));
                    }
                }
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "promote-to-recovery",
                    error);
            }

            if (record != null && record.IsActive)
            {
                ApplyGroupProfileLockRecoveryRecord(record);
            }
            else
            {
                groupProfileLockAcceptedRestartRecovery = false;
                groupProfileUnlockAcceptedRestartRecovery = false;
                groupProfileLockVerificationPending = false;
                groupProfileUnlockVerificationPending = false;
                groupProfileLockRecoveryRequired = true;
            }
            groupProfileLocked = false;
            WriteLog(
                reason
                + (groupProfileLockAcceptedRestartRecovery
                    ? " retained the accepted Group Enable journal. Exact-identity "
                        + "recovery remains status-only; 0x2047 will not be replayed."
                    : " retained the durable group profile-lock recovery interlock."));
        }

        private bool TryDiscardGroupDisableOutcomeSupersededBySafety(
            Guid disableRecoveryIdentity,
            long operationSafetyGeneration,
            string operation)
        {
            if (operationSafetyGeneration == safetyRequestGeneration
                || disableRecoveryIdentity == Guid.Empty
                || groupProfileLockRecoveryJournal == null)
            {
                return false;
            }

            GroupProfileLockRecoveryRecord latest;
            try
            {
                latest = groupProfileLockRecoveryJournal.CurrentRecord;
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "read-after-superseded-Disable",
                    error);
                return false;
            }

            if (latest == null
                || (latest.IsActive
                    && latest.Identity == disableRecoveryIdentity))
            {
                return false;
            }

            if (!latest.IsActive
                && latest.Identity == disableRecoveryIdentity
                && latest.State == GroupProfileLockRecoveryState.Resolved)
            {
                pendingGroupDisableWaitContinuation = null;
                groupProfileUnlockVerificationPending = false;
                groupProfileLockVerificationPending = false;
                ClearGroupProfileLockRecovery();
            }
            else if (latest.IsActive)
            {
                ApplyGroupProfileLockRecoveryRecord(latest);
            }

            groupProfileLocked = false;
            WriteLog(
                operation
                + " discarded a stale completion/failure after a newer Stop or "
                + "Power Off durable state superseded unlock identity "
                + disableRecoveryIdentity.ToString("D")
                + ". The newer journal state was not changed.");
            return true;
        }

        private bool
            TryRestoreFreshVerifiedLockAfterKnownNoEffectGroupDisable(
                Exception error,
                Guid disableRecoveryIdentity,
                long operationSafetyGeneration,
                string operation)
        {
            if (!IsKnownNoEffectGroupDisableFailure(error)
                || disableRecoveryIdentity == Guid.Empty
                || operationSafetyGeneration != safetyRequestGeneration
                || groupProfileLockRecoveryJournal == null)
            {
                return false;
            }

            GroupProfileLockRecoveryRecord record;
            try
            {
                record = groupProfileLockRecoveryJournal.CurrentRecord;
            }
            catch (Exception journalError)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "read-before-no-effect-Disable-restore",
                    journalError);
                return false;
            }

            if (record == null
                || !record.IsActive
                || record.Identity != disableRecoveryIdentity
                || record.ExpectedProfileLocked
                || record.State
                    != GroupProfileLockRecoveryState.ArmedBeforeDispatch)
            {
                return false;
            }

            ResolveGroupProfileLockRecoveryJournal(
                operation + " known no-effect failure");
            pendingGroupDisableWaitContinuation = null;
            pendingGroupEnableWaitContinuation = null;
            groupProfileUnlockVerificationPending = false;
            groupProfileLockVerificationPending = false;
            ClearGroupProfileLockRecovery();
            groupProfileLocked = true;
            WriteLog(
                operation
                + " produced verified no-effect evidence before any accepted "
                + "unlock. The fresh unlock journal was resolved and the prior "
                + "verified profile lock was restored.");
            return true;
        }

        private static bool IsKnownNoEffectGroupDisableFailure(
            Exception error)
        {
            var submission = error as LMCGroupDisableSubmissionException;
            if (submission != null)
            {
                return submission.Evidence != null
                    && !submission.Evidence.CommandMayHaveBeenSent;
            }

            var timeout = error as LMCGroupDisableWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation == null
                    && timeout.Evidence != null
                    && !timeout.Evidence.CommandMayHaveBeenSent;
            }

            var canceled = error as LMCGroupDisableWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation == null
                    && canceled.Evidence != null
                    && !canceled.Evidence.CommandMayHaveBeenSent;
            }

            var preempted = error as LMCSendPreemptedException;
            return preempted != null
                && preempted.Phase == LMCSendPreemptionPhase.BeforeWire;
        }

        private void ResolveGroupProfileLockRecoveryJournal(
            string operation)
        {
            var journal = groupProfileLockRecoveryJournal;
            if (journal == null)
            {
                if (groupProfileLockRecoveryRequired)
                {
                    throw CreateGroupProfileLockRecoveryJournalException(
                        operation,
                        null);
                }

                return;
            }

            GroupProfileLockRecoveryRecord record;
            try
            {
                record = journal.CurrentRecord;
                if (record == null || !record.IsActive)
                {
                    return;
                }

                journal.Resolve(
                    record.Identity,
                    MonotonicUtcNow(record.UpdatedUtc));
                groupProfileLockRecoveryRecoveredAtStartup = false;
                groupProfileLockAcceptedRestartRecovery = false;
                groupProfileUnlockAcceptedRestartRecovery = false;
            }
            catch (Exception error)
            {
                SetGroupProfileLockRecoveryJournalRuntimeError(
                    "resolve",
                    error);
                PromoteGroupProfileLockRecoveryJournal(
                    operation + " durable resolve failure",
                    true);
                throw CreateGroupProfileLockRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private bool TryResolveGroupProfileLockRecoveryForKnownNoDispatch(
            Exception error,
            string operation)
        {
            var rejected = error as LMCGroupEnableRejectedException;
            var preempted = error as LMCSendPreemptedException;
            if (rejected == null
                && (preempted == null
                    || preempted.Phase
                        != LMCSendPreemptionPhase.BeforeWire))
            {
                return false;
            }

            ResolveGroupProfileLockRecoveryJournal(operation);
            return true;
        }

        private void RememberConnectedRemoteEndpoint(
            string endpointIp,
            int endpointPort)
        {
            connectedRemoteIp = NormalizeIPv4(endpointIp);
            connectedRemotePort = endpointPort;
        }

        private void ApplyGroupProfileLockRecoveryRecord(
            GroupProfileLockRecoveryRecord record)
        {
            if (record == null || !record.IsActive)
            {
                return;
            }

            var acceptedAwaitingProof = record.State
                == GroupProfileLockRecoveryState.AcceptedAwaitingProof;
            groupProfileLockAcceptedRestartRecovery = acceptedAwaitingProof
                && record.ExpectedProfileLocked;
            groupProfileUnlockAcceptedRestartRecovery =
                acceptedAwaitingProof
                && !record.ExpectedProfileLocked;
            groupProfileLockRecoveryRequired = record.State
                == GroupProfileLockRecoveryState.RecoveryRequired;
            groupProfileLockVerificationPending =
                groupProfileLockAcceptedRestartRecovery;
            groupProfileUnlockVerificationPending =
                groupProfileUnlockAcceptedRestartRecovery;
            groupProfileLockRecoveryGroupName = record.GroupName;
            groupProfileLockRecoveryGroupReference =
                record.GroupReference;
            groupProfileLockRecoveryEndpointIp = record.EndpointIp;
            groupProfileLockRecoveryEndpointPort = record.EndpointPort;
            groupProfileLockRecoveryDiagnosticsBootId =
                record.DiagnosticsBootId;
            groupProfileLockRecoveryMapRevision = record.MapRevision;
            groupProfileLocked = false;

            if (TextRemoteIp != null)
            {
                TextRemoteIp.Text = record.EndpointIp;
            }

            if (TextRemotePort != null)
            {
                TextRemotePort.Text = record.EndpointPort.ToString(
                    CultureInfo.InvariantCulture);
            }

            if (TextGroupName != null)
            {
                TextGroupName.Text = record.GroupName;
            }
        }

        private void ClearGroupProfileLockRecoveryIdentity()
        {
            groupProfileLockRecoveryGroupReference = 0;
            groupProfileLockRecoveryEndpointIp = null;
            groupProfileLockRecoveryEndpointPort = 0;
            groupProfileLockRecoveryDiagnosticsBootId = 0;
            groupProfileLockRecoveryMapRevision = 0;
        }

        private GroupProfileLockRecoveryRecord
            RequireActiveGroupProfileLockRecoveryRecord(string operation)
        {
            if (groupProfileLockRecoveryJournal == null)
            {
                throw CreateGroupProfileLockRecoveryJournalException(
                    operation,
                    null);
            }

            var record = groupProfileLockRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the volatile recovery latch has no "
                    + "matching durable identity record.");
            }

            return record;
        }

        private void EnsureGroupProfileLockRecoveryJournalCanArm()
        {
            EnsureGroupProfileLockRecoveryJournalCanArm("Group Enable");
        }

        private void EnsureGroupProfileLockRecoveryJournalCanArm(
            string operation)
        {
            if (GroupProfileLockRecoveryJournalCanArm)
            {
                return;
            }

            throw CreateGroupProfileLockRecoveryJournalException(
                operation,
                null);
        }

        private void
            EnsureGroupProfileLockRecoveryJournalAvailableForMutation(
                string operation)
        {
            if (!GroupProfileLockRecoveryJournalUnavailable)
            {
                return;
            }

            throw CreateGroupProfileLockRecoveryJournalException(
                operation,
                null);
        }

        private LMCDiagnosticCapabilities
            RequireStableGroupProfileLockIdentity(string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires nonzero DiagnosticsBootId and MapRevision before "
                    + "a group profile-lock command or recovery mutation is allowed.");
            }

            return capabilities;
        }

        private string RequiredConnectedRemoteIp()
        {
            if (string.IsNullOrWhiteSpace(connectedRemoteIp))
            {
                throw new InvalidOperationException(
                    "The connected PLC endpoint identity is unavailable.");
            }

            return connectedRemoteIp;
        }

        private int RequiredConnectedRemotePort()
        {
            if (connectedRemotePort < 1 || connectedRemotePort > 65535)
            {
                throw new InvalidOperationException(
                    "The connected PLC endpoint port identity is unavailable.");
            }

            return connectedRemotePort;
        }

        private InvalidOperationException
            CreateGroupProfileLockRecoveryJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    groupProfileLockRecoveryJournalRuntimeError)
                ? groupProfileLockRecoveryJournalRuntimeError
                : (!string.IsNullOrEmpty(
                        groupProfileLockRecoveryJournalOpenError)
                    ? groupProfileLockRecoveryJournalOpenError
                    : "An active or unavailable durable recovery record blocks this operation.");
            return new InvalidOperationException(
                operation
                + " is blocked by the durable group profile-lock recovery journal. "
                + detail,
                innerException);
        }

        private void SetGroupProfileLockRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            var runtimeError =
                operation
                + ": "
                + error.GetType().Name
                + ": "
                + error.Message;
            groupProfileLockRecoveryJournalRuntimeError = runtimeError;
            RunOnUi(
                () => WriteLog(
                    "Group profile-lock recovery journal faulted and remains fail-closed: "
                    + runtimeError));
        }

        private static DateTime MonotonicUtcNow(DateTime minimumUtc)
        {
            var now = DateTime.UtcNow;
            return now < minimumUtc ? minimumUtc : now;
        }

        private static string NormalizeIPv4(string value)
        {
            IPAddress address;
            if (string.IsNullOrWhiteSpace(value)
                || !IPAddress.TryParse(value.Trim(), out address)
                || address.AddressFamily != System.Net.Sockets
                    .AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "A numeric IPv4 endpoint is required.",
                    "value");
            }

            return address.ToString();
        }
    }
}

using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private string groupResetRecoveryJournalDirectoryPath;
        private GroupResetRecoveryJournal groupResetRecoveryJournal;
        private string groupResetRecoveryJournalOpenError;
        private string groupResetRecoveryJournalRuntimeError;
        private bool groupResetRecoveryConnectionIdentityVerified;

        internal Action GroupResetArmedBeforeDispatchTestHook { get; set; }
        internal Action GroupResetAcceptedBeforeDurableMarkTestHook
        {
            get;
            set;
        }
        internal Action GroupResetAfterAcceptedDurableMarkTestHook
        {
            get;
            set;
        }
        internal Action GroupResetBeforeDurableResolveTestHook
        {
            get;
            set;
        }

        private bool GroupResetRecoveryJournalCanArm
        {
            get
            {
                return groupResetRecoveryJournal != null
                    && string.IsNullOrEmpty(
                        groupResetRecoveryJournalOpenError)
                    && string.IsNullOrEmpty(
                        groupResetRecoveryJournalRuntimeError)
                    && !groupResetRecoveryJournal.HasActiveRecord;
            }
        }

        private bool GroupResetRecoveryJournalUnavailable
        {
            get
            {
                return groupResetRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        groupResetRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        groupResetRecoveryJournalRuntimeError);
            }
        }

        private bool HasActiveGroupResetRecoveryRecord
        {
            get
            {
                return groupResetRecoveryJournal != null
                    && groupResetRecoveryJournal.HasActiveRecord;
            }
        }

        private bool GroupResetRecoveryReconnectAvailable
        {
            get
            {
                var currentConnection = connection;
                return HasActiveGroupResetRecoveryRecord
                    && (currentConnection == null
                        || !currentConnection.IsConnected)
                    && (pendingGroupResetWaitContinuation == null
                        || !pendingGroupResetWaitContinuation.IsPending);
            }
        }

        private bool IsAttachedOutcomeUncertainGroupResetRecovery
        {
            get
            {
                if (!HasActiveGroupResetRecoveryRecord
                    || pendingGroupResetWaitContinuation == null
                    || !pendingGroupResetWaitContinuation.IsPending
                    || !pendingGroupResetWaitContinuation
                        .RecoveredFromDurableRecord)
                {
                    return false;
                }
                return groupResetRecoveryJournal.CurrentRecord.PriorOutcome
                    == GroupResetRecoveryPriorOutcome.OutcomeUncertain;
            }
        }

        private void InitializeGroupResetRecoveryJournal()
        {
            try
            {
                groupResetRecoveryJournal =
                    groupResetRecoveryJournalDirectoryPath == null
                        ? GroupResetRecoveryJournal.OpenDefault()
                        : GroupResetRecoveryJournal.Open(
                            groupResetRecoveryJournalDirectoryPath);
                groupResetRecoveryJournalOpenError = null;
                groupResetRecoveryJournalRuntimeError = null;

                TryFinalizeCommittedGroupResetRetirementAtStartup();

                var record = groupResetRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    if (record.State
                        != GroupResetRecoveryState.RecoveryRequired)
                    {
                        record = groupResetRecoveryJournal
                            .PromoteRecoveryRequired(
                                record,
                                MonotonicUtcNow(record.UpdatedUtc));
                    }

                    ApplyRecoveredGroupResetRecord(record);
                }
            }
            catch (Exception error)
            {
                var journal = groupResetRecoveryJournal;
                groupResetRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                groupResetRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                groupResetRecoveryConnectionIdentityVerified = false;
                WriteLog(
                    "Group Reset recovery journal is unavailable or corrupt; "
                    + "all new control and mutation sends are fail-closed: "
                    + groupResetRecoveryJournalOpenError);
            }
        }

        private void DisposeGroupResetRecoveryJournal()
        {
            var journal = groupResetRecoveryJournal;
            groupResetRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private GroupResetDispatchIdentityContext
            CaptureGroupResetDispatchIdentity(string operation)
        {
            var currentConnection = RequireConnection();
            var capabilities = RequireStableGroupResetRecoveryIdentity(
                currentConnection,
                operation);
            var callback = RequireGroupResetCallbackEndPoint(
                currentConnection,
                operation);
            var currentGroup = RequireGroup();
            TextLocalIp.Text = callback.Address.ToString();
            TextCallbackPort.Text = callback.Port.ToString(
                CultureInfo.InvariantCulture);
            return new GroupResetDispatchIdentityContext(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                callback.Address.ToString(),
                callback.Port,
                capabilities.DiagnosticsBuild,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                currentGroup.GroupName,
                currentGroup.GroupReference,
                currentConnection.SessionGeneration);
        }

        private GroupResetRecoveryRecord
            ArmGroupResetRecoveryBeforeDispatch(
                GroupResetDispatchIdentityContext identity,
                LMCGroupResetPreparedEvidence prepared,
                string operation)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (prepared == null)
            {
                throw new ArgumentNullException("prepared");
            }

            EnsurePreparedGroupResetIdentity(identity, prepared, operation);
            var members = ConvertGroupResetMembers(prepared.Members);
            try
            {
                var record = groupResetRecoveryJournal.ArmBeforeDispatch(
                    prepared.OperationId,
                    identity.PlcIp,
                    identity.PlcTcpPort,
                    identity.LocalIpv4,
                    identity.CallbackUdpPort,
                    identity.DiagnosticsBuild,
                    identity.DiagnosticsBootId,
                    identity.MapRevision,
                    identity.GroupName,
                    identity.GroupReference,
                    identity.OwnerSessionGeneration,
                    members,
                    prepared.RequiredStableSampleCount,
                    DateTime.UtcNow);

                var testHook = GroupResetArmedBeforeDispatchTestHook;
                if (testHook != null)
                {
                    testHook();
                }
                return record;
            }
            catch (Exception error)
            {
                RecordGroupResetRecoveryJournalRuntimeError(
                    "arm-before-dispatch",
                    error);
                throw CreateGroupResetRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private void MarkGroupResetRecoveryAccepted(
            GroupResetRecoveryRecord preparedRecord,
            LMCGroupAxis currentGroup,
            LMCGroupResetWaitContinuation continuation,
            string operation)
        {
            if (preparedRecord == null)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist Group Reset acceptance because the "
                    + "prepared durable record is unavailable.");
            }
            if (currentGroup == null || continuation == null)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an incomplete Group Reset identity.");
            }
            if (continuation.OperationId != preparedRecord.Identity
                || continuation.GroupReference
                    != preparedRecord.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    preparedRecord.GroupName,
                    StringComparison.Ordinal)
                || continuation.RequiredStableSampleCount
                    != preparedRecord.RequiredStableSampleCount
                || continuation.RecoveredFromDurableRecord
                || !continuation.CommandDispatchedInOwnerSession)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot persist an accepted continuation that does not "
                    + "match the exact fresh durable Group Reset identity.");
            }

            try
            {
                var beforeMarkHook =
                    GroupResetAcceptedBeforeDurableMarkTestHook;
                if (beforeMarkHook != null)
                {
                    beforeMarkHook();
                }

                var current = RequireExactActiveGroupResetOperation(
                    preparedRecord,
                    operation);
                if (current.State
                    == GroupResetRecoveryState.ArmedBeforeDispatch)
                {
                    groupResetRecoveryJournal.MarkAccepted(
                        current,
                        MonotonicUtcNow(current.UpdatedUtc));
                }

                var afterMarkHook =
                    GroupResetAfterAcceptedDurableMarkTestHook;
                if (afterMarkHook != null)
                {
                    afterMarkHook();
                }
            }
            catch (Exception error)
            {
                RecordGroupResetRecoveryJournalRuntimeError(
                    "mark-accepted",
                    error);
                throw CreateGroupResetRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private void ResolveGroupResetKnownNoEffect(
            GroupResetRecoveryRecord attemptRecord,
            string reason)
        {
            if (attemptRecord == null
                || !HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            try
            {
                var current = RequireExactActiveGroupResetOperation(
                    attemptRecord,
                    reason);
                if (current.State
                    != GroupResetRecoveryState.ArmedBeforeDispatch)
                {
                    throw new InvalidOperationException(
                        reason
                        + " cannot resolve a Group Reset that crossed the "
                        + "accepted or uncertain boundary.");
                }
                groupResetRecoveryJournal.Resolve(
                    current,
                    MonotonicUtcNow(current.UpdatedUtc));
            }
            catch (Exception error)
            {
                SetGroupResetRecoveryJournalRuntimeError(
                    "resolve-known-no-effect",
                    error);
                throw CreateGroupResetRecoveryJournalException(
                    reason,
                    error);
            }
        }

        private void PromoteGroupResetRecoveryRequired(
            GroupResetRecoveryRecord attemptRecord,
            string reason)
        {
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            try
            {
                var current = groupResetRecoveryJournal.CurrentRecord;
                if (attemptRecord != null
                    && current.Identity != attemptRecord.Identity)
                {
                    throw new InvalidOperationException(
                        reason
                        + " belongs to an older Group Reset operation. The "
                        + "newer durable record was preserved unchanged.");
                }
                if (current.State
                    != GroupResetRecoveryState.RecoveryRequired)
                {
                    groupResetRecoveryJournal.PromoteRecoveryRequired(
                        current,
                        MonotonicUtcNow(current.UpdatedUtc));
                }
            }
            catch (Exception error)
            {
                SetGroupResetRecoveryJournalRuntimeError(
                    "promote-to-recovery",
                    error);
                throw CreateGroupResetRecoveryJournalException(
                    reason,
                    error);
            }
        }

        private GroupResetRecoveryPriorOutcome
            ResolveGroupResetAfterStableProof(
                GroupResetRecoveryRecord verificationRecord,
                LMCGroupResetWaitResult result,
                string operation)
        {
            var current = RequireExactActiveGroupResetOperation(
                verificationRecord,
                operation);
            if (result == null
                || result.Evidence == null
                || result.Evidence.OperationId != current.Identity
                || result.RequiredStableSampleCount
                    != current.RequiredStableSampleCount
                || result.StableSampleCount
                    < current.RequiredStableSampleCount
                || result.FinalMemberStatuses == null
                || result.FinalMemberStatuses.Length
                    != current.Members.Length
                || result.RecoveredFromDurableRecord
                    == result.CommandDispatchedInOwnerSession)
            {
                throw new InvalidOperationException(
                    operation
                    + " does not match the active durable Group Reset proof "
                    + "identity and provenance.");
            }
            if (current.PriorOutcome
                    == GroupResetRecoveryPriorOutcome.OutcomeUncertain
                && !result.RecoveredFromDurableRecord)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot resolve an outcome-uncertain Group Reset from "
                    + "fresh-command provenance.");
            }

            try
            {
                var testHook = GroupResetBeforeDurableResolveTestHook;
                if (testHook != null)
                {
                    testHook();
                }
                groupResetRecoveryJournal.Resolve(
                    current,
                    MonotonicUtcNow(current.UpdatedUtc));
                return current.PriorOutcome;
            }
            catch (Exception error)
            {
                SetGroupResetRecoveryJournalRuntimeError("resolve", error);
                throw CreateGroupResetRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private void ResolveActiveGroupResetSafetySupersede(
            string operation)
        {
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            var current = groupResetRecoveryJournal.CurrentRecord;
            try
            {
                groupResetRecoveryJournal.Resolve(
                    current,
                    MonotonicUtcNow(current.UpdatedUtc));
            }
            catch (Exception error)
            {
                SetGroupResetRecoveryJournalRuntimeError(
                    "safety-supersede",
                    error);
                throw CreateGroupResetRecoveryJournalException(
                    operation,
                    error);
            }
        }

        private bool IsDurableGroupResetMember(LMCSingleAxis currentAxis)
        {
            if (currentAxis == null
                || !HasActiveGroupResetRecoveryRecord)
            {
                return false;
            }

            var members = groupResetRecoveryJournal.CurrentRecord.Members;
            for (var index = 0; index < members.Length; index++)
            {
                if (members[index].AxisReference
                    == currentAxis.AxisReference)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task EnsureGroupResetRecoveryConnectionIdentityAsync(
            string operation)
        {
            groupResetRecoveryConnectionIdentityVerified = false;
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupResetRecoveryRecord(operation);
            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableGroupResetRecoveryIdentity(
                currentConnection,
                operation);
            var callback = RequireGroupResetCallbackEndPoint(
                currentConnection,
                operation);
            if (!record.MatchesRecoveryIdentity(
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                callback.Address.ToString(),
                callback.Port,
                capabilities.DiagnosticsBuild,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                record.GroupName,
                record.GroupReference,
                record.OwnerSessionGeneration,
                record.Members,
                record.RequiredStableSampleCount))
            {
                throw CreateGroupResetRecoveryIdentityMismatch(
                    operation,
                    record,
                    capabilities,
                    callback);
            }

            groupResetRecoveryConnectionIdentityVerified = true;
        }

        private void EnsureGroupResetRecoveryEndpoint(
            string plcIp,
            int plcTcpPort,
            string localIpv4,
            int callbackUdpPort)
        {
            groupResetRecoveryConnectionIdentityVerified = false;
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupResetRecoveryRecord("Reconnect");
            if (!record.MatchesEndpoint(
                plcIp,
                plcTcpPort,
                localIpv4,
                callbackUdpPort))
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked because the PLC TCP endpoint or PC "
                    + "callback IPv4/UDP endpoint does not match the durable "
                    + "Group Reset recovery record. No TCP connection was opened.");
            }
        }

        private void EnsureGroupResetRecoveryLookupAllowed(string groupName)
        {
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupResetRecoveryRecord(
                "Load Group Reset recovery");
            if (!groupResetRecoveryConnectionIdentityVerified
                || !string.Equals(
                    record.GroupName,
                    groupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Load Group is blocked until the exact durable Group Reset "
                    + "endpoint/build/BootId/MapRevision and group name are "
                    + "verified. No group lookup RPC was sent.");
            }
        }

        private void EnsureLoadedGroupMatchesResetRecovery(
            LMCGroupAxis loadedGroup)
        {
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }
            if (loadedGroup == null)
            {
                throw new ArgumentNullException("loadedGroup");
            }

            var record = RequireActiveGroupResetRecoveryRecord(
                "Load Group Reset recovery");
            var currentConnection = RequireConnection();
            var capabilities = RequireStableGroupResetRecoveryIdentity(
                currentConnection,
                "Load Group Reset recovery");
            var callback = RequireGroupResetCallbackEndPoint(
                currentConnection,
                "Load Group Reset recovery");
            if (!groupResetRecoveryConnectionIdentityVerified
                || !record.MatchesRecoveryIdentity(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    callback.Address.ToString(),
                    callback.Port,
                    capabilities.DiagnosticsBuild,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    loadedGroup.GroupName,
                    loadedGroup.GroupReference,
                    record.OwnerSessionGeneration,
                    record.Members,
                    record.RequiredStableSampleCount))
            {
                throw new InvalidOperationException(
                    "The loaded group does not match the exact durable Group "
                    + "Reset endpoint/build/BootId/MapRevision/name/reference. "
                    + "No member or status recovery RPC was sent.");
            }
        }

        private async Task AttachGroupResetRecoveryAsync(
            LMCGroupAxis loadedGroup)
        {
            if (!HasActiveGroupResetRecoveryRecord)
            {
                return;
            }

            var record = RequireActiveGroupResetRecoveryRecord(
                "Attach Group Reset recovery");
            if (record.State != GroupResetRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "Restart/reconnect Group Reset attachment requires a "
                    + "RecoveryRequired durable record.");
            }

            EnsureLoadedGroupMatchesResetRecovery(loadedGroup);
            var sdkRecord = new LMCGroupResetDurableRecoveryRecord(
                record.Identity,
                ConvertGroupResetPriorOutcome(record.PriorOutcome),
                record.GroupName,
                record.GroupReference,
                record.OwnerSessionGeneration,
                ConvertGroupResetMembers(record.Members),
                record.RequiredStableSampleCount);
            var continuation = await loadedGroup
                .AttachGroupResetDurableRecoveryAsync(
                    sdkRecord,
                    new LMCGroupResetWaitOptions
                    {
                        StableSampleCount =
                            record.RequiredStableSampleCount
                    },
                    CancellationToken.None);
            if (continuation == null
                || !continuation.IsPending
                || continuation.OperationId != record.Identity
                || !continuation.RecoveredFromDurableRecord
                || continuation.CommandDispatchedInOwnerSession
                || continuation.GroupReference != record.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    record.GroupName,
                    StringComparison.Ordinal)
                || continuation.RequiredStableSampleCount
                    != record.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    "SDK Group Reset durable attachment returned mismatched "
                    + "identity or command provenance.");
            }

            pendingGroupResetWaitContinuation = continuation;
            groupResetVerificationPending = true;
            groupResetSubmissionUncertain = false;
            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = false;
            groupResetObservedLockedStandby = false;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();
            WriteLog(
                "Attached durable Group Reset recovery after one exact fresh "
                + "0x20D2 member snapshot. Resume sends only 0x2045/0x2028 "
                + "status reads; 0x2049 was not replayed.");
        }

        private bool GroupResetReconnectBlockedOnCurrentSession()
        {
            return HasUnresolvedGroupResetState()
                && !GroupResetRecoveryReconnectAvailable;
        }

        private bool GroupResetLookupBlockedOnCurrentSession()
        {
            if (pendingGroupResetWaitContinuation != null
                && pendingGroupResetWaitContinuation.IsPending)
            {
                return true;
            }
            if (HasActiveGroupResetRecoveryRecord)
            {
                return group != null;
            }
            return HasUnresolvedGroupResetState();
        }

        private void ReapplyCurrentGroupResetRecoveryState()
        {
            var record = groupResetRecoveryJournal == null
                ? null
                : groupResetRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                return;
            }

            if (connection == null || !connection.IsConnected)
            {
                groupResetRecoveryConnectionIdentityVerified = false;
            }
            var sdkContinuation = group == null
                ? null
                : group.PendingGroupResetWaitContinuation;
            pendingGroupResetWaitContinuation = sdkContinuation != null
                    && sdkContinuation.IsPending
                    && sdkContinuation.OperationId == record.Identity
                ? sdkContinuation
                : null;
            groupResetVerificationPending =
                pendingGroupResetWaitContinuation != null;
            groupResetSubmissionUncertain =
                pendingGroupResetWaitContinuation == null
                && record.PriorOutcome
                    == GroupResetRecoveryPriorOutcome.OutcomeUncertain;
            groupResetSessionContinuationDiscarded =
                pendingGroupResetWaitContinuation == null;
            groupResetSupersededByLaterMutation = false;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();
        }

        private void ApplyRecoveredGroupResetRecord(
            GroupResetRecoveryRecord record)
        {
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = record.PriorOutcome
                == GroupResetRecoveryPriorOutcome.OutcomeUncertain;
            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = true;
            groupResetObservedLockedStandby = false;
            groupStatusRefreshRequired = true;
            groupResetRecoveryConnectionIdentityVerified = false;
            InvalidateGroupPreparationAfterAcceptedReset();

            TextRemoteIp.Text = record.PlcIp;
            TextRemotePort.Text = record.PlcTcpPort.ToString(
                CultureInfo.InvariantCulture);
            TextLocalIp.Text = record.LocalIpv4;
            TextCallbackPort.Text = record.CallbackUdpPort.ToString(
                CultureInfo.InvariantCulture);
            TextGroupName.Text = record.GroupName;
            WriteLog(
                record.PriorOutcome
                        == GroupResetRecoveryPriorOutcome.Accepted
                    ? "Recovered a durable accepted Group Reset ACK. Reconnect "
                        + "to the exact identity, load the exact group, and run "
                        + "status-only verification; 0x2049 will not be replayed."
                    : "Recovered an outcome-uncertain Group Reset. The old "
                        + "0x2049 outcome remains unknown. Exact-identity "
                        + "recovery checks members once and sends status reads "
                        + "only; it never replays 0x2049.");
        }

        private void EnsureGroupResetRecoveryJournalCanArm()
        {
            if (!GroupResetRecoveryJournalCanArm)
            {
                throw CreateGroupResetRecoveryJournalException(
                    "Group Reset",
                    null);
            }
        }

        private GroupResetRecoveryRecord
            RequireActiveGroupResetRecoveryRecord(string operation)
        {
            var journal = groupResetRecoveryJournal;
            var record = journal == null ? null : journal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because no active durable Group Reset "
                    + "recovery record exists.");
            }
            return record;
        }

        private GroupResetRecoveryRecord
            RequireExactActiveGroupResetOperation(
                GroupResetRecoveryRecord expected,
                string operation)
        {
            var current = RequireActiveGroupResetRecoveryRecord(operation);
            if (expected == null || current.Identity != expected.Identity)
            {
                throw new InvalidOperationException(
                    operation
                    + " belongs to a different Group Reset operation. The "
                    + "current durable record was preserved unchanged.");
            }
            return current;
        }

        private LMCDiagnosticCapabilities
            RequireStableGroupResetRecoveryIdentity(
                LMCConnection currentConnection,
                string operation)
        {
            var capabilities = diagnosticCapabilities;
            if (currentConnection == null
                || capabilities == null
                || capabilities.DiagnosticsBuild == 0
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || !capabilities.IsBoundTo(
                    currentConnection.Diagnostics,
                    currentConnection.SessionGeneration))
            {
                throw new InvalidOperationException(
                    operation
                    + " requires session-bound nonzero DiagnosticsBuild, "
                    + "DiagnosticsBootId, and MapRevision before Group Reset "
                    + "dispatch or recovery.");
            }
            return capabilities;
        }

        private static IPEndPoint RequireGroupResetCallbackEndPoint(
            LMCConnection currentConnection,
            string operation)
        {
            var callback = currentConnection == null
                ? null
                : currentConnection.CallbackLocalEndPoint;
            if (callback == null
                || callback.Address == null
                || callback.Address.AddressFamily
                    != AddressFamily.InterNetwork
                || callback.Port < 1
                || callback.Port > 65535)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires the actual bound IPv4 callback endpoint.");
            }
            return callback;
        }

        private RecoveryConnectionIdentityMismatchException
            CreateGroupResetRecoveryIdentityMismatch(
                string operation,
                GroupResetRecoveryRecord stored,
                LMCDiagnosticCapabilities current,
                IPEndPoint callback)
        {
            return new RecoveryConnectionIdentityMismatchException(
                operation
                + " is blocked because endpoint, DiagnosticsBuild, BootId, or "
                + "MapRevision does not match the durable Group Reset record. "
                + "Stored="
                + stored.PlcIp
                + ":"
                + stored.PlcTcpPort.ToString(CultureInfo.InvariantCulture)
                + "/"
                + stored.LocalIpv4
                + ":"
                + stored.CallbackUdpPort.ToString(CultureInfo.InvariantCulture)
                + ", Build=0x"
                + stored.DiagnosticsBuild.ToString("X8")
                + ", BootId=0x"
                + stored.DiagnosticsBootId.ToString("X8")
                + ", Map=0x"
                + stored.MapRevision.ToString("X8")
                + "; Current="
                + RequiredConnectedRemoteIp()
                + ":"
                + RequiredConnectedRemotePort().ToString(
                    CultureInfo.InvariantCulture)
                + "/"
                + callback.Address
                + ":"
                + callback.Port.ToString(CultureInfo.InvariantCulture)
                + ", Build=0x"
                + current.DiagnosticsBuild.ToString("X8")
                + ", BootId=0x"
                + current.DiagnosticsBootId.ToString("X8")
                + ", Map=0x"
                + current.MapRevision.ToString("X8")
                + ".");
        }

        private InvalidOperationException
            CreateGroupResetRecoveryJournalException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    groupResetRecoveryJournalRuntimeError)
                ? groupResetRecoveryJournalRuntimeError
                : (!string.IsNullOrEmpty(groupResetRecoveryJournalOpenError)
                    ? groupResetRecoveryJournalOpenError
                    : "An active durable Group Reset record blocks this operation.");
            return new InvalidOperationException(
                operation
                + " is blocked by the durable Group Reset recovery journal. "
                + detail,
                innerException);
        }

        private void SetGroupResetRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            RecordGroupResetRecoveryJournalRuntimeError(operation, error);
            WriteLog(
                "Group Reset recovery journal faulted and remains fail-closed: "
                + groupResetRecoveryJournalRuntimeError);
        }

        private void RecordGroupResetRecoveryJournalRuntimeError(
            string operation,
            Exception error)
        {
            groupResetRecoveryJournalRuntimeError = operation
                + ": "
                + error.GetType().Name
                + ": "
                + error.Message;
        }

        private static void EnsurePreparedGroupResetIdentity(
            GroupResetDispatchIdentityContext identity,
            LMCGroupResetPreparedEvidence prepared,
            string operation)
        {
            if (prepared.OperationId == Guid.Empty
                || prepared.SessionGeneration
                    != identity.OwnerSessionGeneration
                || prepared.GroupReference != identity.GroupReference
                || !string.Equals(
                    prepared.GroupName,
                    identity.GroupName,
                    StringComparison.Ordinal)
                || prepared.RequiredStableSampleCount < 1
                || prepared.Members == null
                || prepared.Members.Length == 0)
            {
                throw new InvalidOperationException(
                    operation
                    + " prepared evidence does not match the immutable "
                    + "session/group identity captured before dispatch.");
            }
        }

        private static GroupResetRecoveryMember[] ConvertGroupResetMembers(
            LMCGroupResetDurableMemberIdentity[] source)
        {
            if (source == null || source.Length == 0)
            {
                throw new InvalidOperationException(
                    "Group Reset durable recovery requires member identities.");
            }

            var result = new GroupResetRecoveryMember[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                var member = source[index];
                if (member == null || member.Index != index)
                {
                    throw new InvalidOperationException(
                        "Group Reset members must be complete and exactly ordered.");
                }
                result[index] = new GroupResetRecoveryMember(
                    member.AxisName,
                    member.AxisReference,
                    member.DeviceId);
            }
            return result;
        }

        private static LMCGroupResetDurableMemberIdentity[]
            ConvertGroupResetMembers(GroupResetRecoveryMember[] source)
        {
            var result = new LMCGroupResetDurableMemberIdentity[source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                result[index] = new LMCGroupResetDurableMemberIdentity(
                    index,
                    source[index].AxisReference,
                    source[index].DeviceId,
                    source[index].AxisName);
            }
            return result;
        }

        private static LMCGroupResetSubmissionOutcome
            ConvertGroupResetPriorOutcome(
                GroupResetRecoveryPriorOutcome outcome)
        {
            if (outcome == GroupResetRecoveryPriorOutcome.Accepted)
            {
                return LMCGroupResetSubmissionOutcome.Accepted;
            }
            if (outcome == GroupResetRecoveryPriorOutcome.OutcomeUncertain)
            {
                return LMCGroupResetSubmissionOutcome.OutcomeUncertain;
            }
            throw new InvalidOperationException(
                "An active Group Reset recovery record cannot be attached "
                + "with NotAttempted prior outcome.");
        }

        private sealed class GroupResetDispatchIdentityContext
        {
            internal GroupResetDispatchIdentityContext(
                string plcIp,
                int plcTcpPort,
                string localIpv4,
                int callbackUdpPort,
                uint diagnosticsBuild,
                uint diagnosticsBootId,
                uint mapRevision,
                string groupName,
                ushort groupReference,
                long ownerSessionGeneration)
            {
                PlcIp = plcIp;
                PlcTcpPort = plcTcpPort;
                LocalIpv4 = localIpv4;
                CallbackUdpPort = callbackUdpPort;
                DiagnosticsBuild = diagnosticsBuild;
                DiagnosticsBootId = diagnosticsBootId;
                MapRevision = mapRevision;
                GroupName = groupName;
                GroupReference = groupReference;
                OwnerSessionGeneration = ownerSessionGeneration;
            }

            internal string PlcIp { get; private set; }
            internal int PlcTcpPort { get; private set; }
            internal string LocalIpv4 { get; private set; }
            internal int CallbackUdpPort { get; private set; }
            internal uint DiagnosticsBuild { get; private set; }
            internal uint DiagnosticsBootId { get; private set; }
            internal uint MapRevision { get; private set; }
            internal string GroupName { get; private set; }
            internal ushort GroupReference { get; private set; }
            internal long OwnerSessionGeneration { get; private set; }
        }
    }
}

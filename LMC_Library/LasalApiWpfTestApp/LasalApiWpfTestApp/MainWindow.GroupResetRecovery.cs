using System;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private LMCGroupResetWaitContinuation
            pendingGroupResetWaitContinuation;
        private bool groupResetVerificationPending;
        private bool groupResetSubmissionUncertain;
        private bool groupResetSupersededByLaterMutation;
        private bool groupResetSessionContinuationDiscarded;
        private bool groupResetObservedLockedStandby;
        private LMCGroupResetWaitEvidence
            deferredGroupResetSubmissionUncertainEvidence;
        private string deferredGroupResetSubmissionUncertainReason;

        private bool HasUnresolvedGroupResetState()
        {
            if (HasActiveGroupResetRecoveryRecord)
            {
                return true;
            }

            if (groupResetSupersededByLaterMutation
                || groupResetSessionContinuationDiscarded)
            {
                return false;
            }

            if (groupResetSubmissionUncertain
                || groupResetVerificationPending
                || (pendingGroupResetWaitContinuation != null
                    && pendingGroupResetWaitContinuation.IsPending))
            {
                return true;
            }

            var currentGroup = group;
            var sdkContinuation = currentGroup == null
                ? null
                : currentGroup.PendingGroupResetWaitContinuation;
            return sdkContinuation != null && sdkContinuation.IsPending;
        }

        private LMCGroupResetWaitContinuation
            GetPendingGroupResetWaitContinuation(
                LMCGroupAxis currentGroup)
        {
            if (currentGroup == null
                || groupResetSubmissionUncertain
                || groupResetSupersededByLaterMutation
                || groupResetSessionContinuationDiscarded)
            {
                return null;
            }

            var sdkContinuation =
                currentGroup.PendingGroupResetWaitContinuation;
            if (sdkContinuation != null
                && sdkContinuation.IsPending)
            {
                EnsureGroupResetContinuationIdentity(
                    currentGroup,
                    sdkContinuation,
                    "Pending Group Reset verification");
                pendingGroupResetWaitContinuation = sdkContinuation;
                groupResetVerificationPending = true;
                return sdkContinuation;
            }

            if (pendingGroupResetWaitContinuation != null
                && pendingGroupResetWaitContinuation.IsPending)
            {
                EnsureGroupResetContinuationIdentity(
                    currentGroup,
                    pendingGroupResetWaitContinuation,
                    "Pending Group Reset verification");
                groupResetVerificationPending = true;
                return pendingGroupResetWaitContinuation;
            }

            return null;
        }

        private static void EnsureGroupResetContinuationIdentity(
            LMCGroupAxis currentGroup,
            LMCGroupResetWaitContinuation continuation,
            string operation)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            if (continuation == null
                || continuation.GroupReference
                    != currentGroup.GroupReference
                || !string.Equals(
                    continuation.GroupName,
                    currentGroup.GroupName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    operation
                    + " does not match the loaded group identity.");
            }
        }

        private void MarkGroupResetAccepted(
            LMCGroupAxis currentGroup,
            LMCGroupResetWaitContinuation continuation,
            GroupResetRecoveryRecord preparedRecord,
            string operation)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    new Action(
                        () => MarkGroupResetAccepted(
                            currentGroup,
                            continuation,
                            preparedRecord,
                            operation)));
                return;
            }

            EnsureGroupResetContinuationIdentity(
                currentGroup,
                continuation,
                operation);
            if (!continuation.IsPending
                || !ReferenceEquals(group, currentGroup)
                || !ReferenceEquals(
                    currentGroup.PendingGroupResetWaitContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    operation
                    + " did not publish the exact pending Group Reset "
                    + "continuation before status verification.");
            }

            MarkGroupResetRecoveryAccepted(
                preparedRecord,
                currentGroup,
                continuation,
                operation);

            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = false;
            groupResetSubmissionUncertain = false;
            groupResetObservedLockedStandby = false;
            pendingGroupResetWaitContinuation = continuation;
            groupResetVerificationPending = true;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();
            UpdateUiState();
        }

        private void InvalidateGroupPreparationAfterAcceptedReset()
        {
            groupActiveVerified = false;
            groupIdentityConfigured = false;
            ResetIdentityHomeCheckState();
            groupProfileLocked = false;
        }

        private void HandleGroupResetWaitFailure(
            LMCGroupAxis currentGroup,
            GroupResetRecoveryRecord attemptRecord,
            Exception error)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    new Action(
                        () => HandleGroupResetWaitFailure(
                            currentGroup,
                            attemptRecord,
                            error)));
                return;
            }

            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (groupResetSessionContinuationDiscarded)
            {
                groupResetVerificationPending = false;
                pendingGroupResetWaitContinuation = null;
                WriteLog(
                    "The live Group Reset continuation ended with the connection. "
                    + "Its durable record remains RecoveryRequired; exact "
                    + "reconnect recovery is status-only and never replays 0x2049.");
                return;
            }

            if (groupResetSupersededByLaterMutation)
            {
                groupResetVerificationPending = false;
                pendingGroupResetWaitContinuation = null;
                WriteLog(
                    "The earlier Group Reset result was not applied because a "
                    + "later accepted or outcome-uncertain mutation superseded "
                    + "its completion attribution. No 0x2049 replay was sent.");
                return;
            }

            var continuation = GetGroupResetWaitContinuation(error);
            if (continuation == null && currentGroup != null)
            {
                continuation = currentGroup
                    .PendingGroupResetWaitContinuation;
            }
            if (continuation == null)
            {
                continuation = pendingGroupResetWaitContinuation;
            }

            if (continuation != null && continuation.IsPending)
            {
                PreservePendingGroupResetWaitUi(
                    currentGroup,
                    continuation,
                    error.Message);
                return;
            }

            if (continuation != null)
            {
                FinishTerminalGroupResetWaitFailure(
                    continuation,
                    error.Message);
                return;
            }

            var evidence = GetGroupResetWaitEvidence(error);
            if (evidence != null
                && evidence.SubmissionOutcome
                    == LMCGroupResetSubmissionOutcome.OutcomeUncertain)
            {
                PromoteGroupResetRecoveryRequired(
                    attemptRecord,
                    "Group Reset submission outcome is uncertain");
                MarkGroupResetSubmissionUncertain(
                    evidence,
                    error.Message);
                return;
            }

            if (error is LMCGroupResetRejectedException
                || (evidence != null
                    && evidence.SubmissionOutcome
                        == LMCGroupResetSubmissionOutcome.NotAttempted))
            {
                ResolveGroupResetKnownNoEffect(
                    attemptRecord,
                    error is LMCGroupResetRejectedException
                        ? "Group Reset valid NACK"
                        : "Group Reset not attempted");
            }
            else if (HasActiveGroupResetRecoveryRecord)
            {
                PromoteGroupResetRecoveryRequired(
                    attemptRecord,
                    "Group Reset failed without a reusable accepted continuation");
            }

            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = false;
            if (error is LMCGroupResetRejectedException)
            {
                WriteLog(
                    "Group Reset was rejected by a valid acknowledgement. "
                    + "The command did not take effect, so the existing "
                    + "preparation state was retained.");
            }
            else if (evidence != null
                && evidence.SubmissionOutcome
                    == LMCGroupResetSubmissionOutcome.NotAttempted)
            {
                WriteLog(
                    "Group Reset was not dispatched. Existing preparation "
                    + "state was retained. Reason: "
                    + error.Message);
            }
            else
            {
                WriteLog(
                    "Group Reset failed before an accepted or outcome-uncertain "
                    + "submission was established. Existing preparation state "
                    + "was retained. Reason: "
                    + error.Message);
            }
            UpdateUiState();
        }

        private void PreservePendingGroupResetWaitUi(
            LMCGroupAxis currentGroup,
            LMCGroupResetWaitContinuation continuation,
            string reason)
        {
            if (continuation == null || !continuation.IsPending)
            {
                return;
            }

            InvalidateGroupPreparationAfterAcceptedReset();
            EnsureGroupResetContinuationIdentity(
                currentGroup,
                continuation,
                "Preserve Group Reset verification");
            pendingGroupResetWaitContinuation = continuation;
            groupResetVerificationPending = true;
            groupStatusRefreshRequired = true;

            if (continuation.LastObservedGroupStatus != null)
            {
                DisplayGroupStatus(continuation.LastObservedGroupStatus);
            }
            else if (continuation.Acknowledgement != null)
            {
                TextGroupResult.Text =
                    FormatResponse(continuation.Acknowledgement);
            }

            var uncertainRecovery =
                IsAttachedOutcomeUncertainGroupResetRecovery;
            TextGroupResult.Text += Environment.NewLine
                + (uncertainRecovery
                    ? "Outcome-uncertain Group Reset recovery remains attached. "
                        + "Only current status is being verified; the prior "
                        + "0x2049 outcome remains unknown. "
                    : "Group Reset ACK is preserved for this live session. ")
                + "Resume Reset Verification sends status reads only; "
                + "0x2049 replay is blocked. Rounds="
                + continuation.StatusRoundCount
                + ", Stable="
                + continuation.StableSampleCount
                + "/"
                + continuation.RequiredStableSampleCount
                + "."
                + Environment.NewLine
                + "Power, identity/Home, and profile-lock readiness were "
                + "invalidated and will not be restored by Reset proof."
                + Environment.NewLine
                + "Pending reason: "
                + reason;
            WriteLog(
                (uncertainRecovery
                    ? "Outcome-uncertain Group Reset status-only recovery "
                        + "remains attached; the prior 0x2049 outcome is unknown. "
                    : "Group Reset ACK preserved in this session. ")
                + "Resume sends "
                + "0x2045/0x2028 status reads only; 0x2049 replay is blocked. "
                + "Rounds="
                + continuation.StatusRoundCount
                + ", Stable="
                + continuation.StableSampleCount
                + "/"
                + continuation.RequiredStableSampleCount
                + ".");
            UpdateUiState();
        }

        private void FinishTerminalGroupResetWaitFailure(
            LMCGroupResetWaitContinuation continuation,
            string reason)
        {
            var acceptedStateWasTracked = groupResetVerificationPending
                || ReferenceEquals(
                    pendingGroupResetWaitContinuation,
                    continuation)
                || continuation.Acknowledgement != null;
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = false;
            groupResetObservedLockedStandby = false;
            groupResetSupersededByLaterMutation = true;
            groupStatusRefreshRequired = acceptedStateWasTracked;
            if (acceptedStateWasTracked)
            {
                InvalidateGroupPreparationAfterAcceptedReset();
            }

            WriteLog(
                "Group Reset continuation is terminal ("
                + continuation.State
                + "). The status-only resume interlock was cleared; no 0x2049 "
                + "replay will occur. Preparation remains fail-closed. Reason: "
                + reason);
            UpdateUiState();
        }

        private void MarkGroupResetSubmissionUncertain(
            LMCGroupResetWaitEvidence evidence,
            string reason)
        {
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = true;
            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = false;
            groupResetObservedLockedStandby = false;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();

            if (connection == null || !connection.IsConnected)
            {
                DiscardPendingGroupResetAfterConnectionLoss(
                    "Reset acknowledgement boundary transport loss");
                UpdateUiState();
                return;
            }

            if (safetyCommandRunning)
            {
                deferredGroupResetSubmissionUncertainEvidence = evidence;
                deferredGroupResetSubmissionUncertainReason = reason;
                WriteLog(
                    "The response for command 0x2049 was discarded before "
                    + "result application. The stale Group Reset submission "
                    + "result is held until "
                    + "the in-flight safety command establishes its own "
                    + "accepted, rejected, or uncertain outcome.");
                UpdateUiState();
                return;
            }

            PublishDeferredGroupResetSubmissionUncertain(evidence, reason);
            UpdateUiState();
        }

        private void PublishDeferredGroupResetSubmissionUncertainIfAny()
        {
            if (deferredGroupResetSubmissionUncertainEvidence == null)
            {
                return;
            }

            PublishDeferredGroupResetSubmissionUncertain(
                deferredGroupResetSubmissionUncertainEvidence,
                deferredGroupResetSubmissionUncertainReason);
            UpdateUiState();
        }

        private void PublishDeferredGroupResetSubmissionUncertain(
            LMCGroupResetWaitEvidence evidence,
            string reason)
        {
            deferredGroupResetSubmissionUncertainEvidence = null;
            deferredGroupResetSubmissionUncertainReason = null;
            TextGroupResult.Text =
                "Group Reset submission outcome is uncertain. No accepted "
                + "status-only continuation exists, so 0x2049 replay is "
                + "blocked. Use Stop, Power Off, safe Disable, or disconnect."
                + Environment.NewLine
                + "Submission="
                + evidence.SubmissionOutcome
                + ", CommandMayHaveBeenSent="
                + evidence.CommandMayHaveBeenSent
                + "."
                + Environment.NewLine
                + "Reason: "
                + reason;
            WriteLog(
                "Group Reset may have been sent, but no accepted continuation "
                + "was published. Fresh 0x2049 is blocked. Only Stop, Power "
                + "Off, safe Disable, or disconnect may clear this live-session "
                + "interlock.");
        }

        private static LMCGroupResetWaitContinuation
            GetGroupResetWaitContinuation(Exception error)
        {
            var timeout = error as LMCGroupResetWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Continuation;
            }

            var canceled = error as LMCGroupResetWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Continuation;
            }

            var status = error as LMCGroupResetStatusException;
            if (status != null)
            {
                return status.Continuation;
            }

            var interference = error as LMCGroupResetInterferenceException;
            if (interference != null)
            {
                return interference.Continuation;
            }

            var pending = error as LMCGroupResetWaitPendingException;
            return pending == null ? null : pending.Continuation;
        }

        private static LMCGroupResetWaitEvidence
            GetGroupResetWaitEvidence(Exception error)
        {
            var rejected = error as LMCGroupResetRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCGroupResetSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCGroupResetWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCGroupResetWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var status = error as LMCGroupResetStatusException;
            if (status != null)
            {
                return status.Evidence;
            }

            var interference = error as LMCGroupResetInterferenceException;
            return interference == null ? null : interference.Evidence;
        }

        private void CompleteGroupResetWaitUi(
            LMCGroupResetWaitResult result,
            GroupResetRecoveryRecord verificationRecord)
        {
            if (result == null
                || result.FinalGroupStatus == null
                || !result.FinalGroupStatus.IsSuccess
                || result.StableSampleCount
                    < result.RequiredStableSampleCount)
            {
                throw new InvalidOperationException(
                    "Group Reset verification returned no stable full-clear "
                    + "group/member proof.");
            }

            var priorOutcome = ResolveGroupResetAfterStableProof(
                verificationRecord,
                result,
                "Group Reset stable error-clearance proof");
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = false;
            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = false;
            groupStatusRefreshRequired = false;
            InvalidateGroupPreparationAfterAcceptedReset();
            groupResetObservedLockedStandby =
                result.FinalGroupStatus.IsPowerOn
                && result.FinalGroupStatus.IsStandby;
            DisplayGroupStatus(result.FinalGroupStatus);
            TextGroupResult.Text += Environment.NewLine
                + (priorOutcome
                        == GroupResetRecoveryPriorOutcome.OutcomeUncertain
                    ? "Current group/member errors are stably clear; the prior "
                        + "Group Reset outcome remains unknown. Status-only rounds="
                    : "Group Reset ACK accepted once; status-only rounds=")
                + result.StatusRoundCount
                + ", Stable full-clear="
                + result.StableSampleCount
                + "/"
                + result.RequiredStableSampleCount
                + "."
                + Environment.NewLine
                + "Reset did not restore Power/Identity/Home/Profile readiness. "
                + "Run Power On, Set Identity, and Enable again before motion."
                + (groupResetObservedLockedStandby
                    ? Environment.NewLine
                        + "The final Reset proof observed LockedStandby. Safe "
                        + "Disable is available, but this observation does not "
                        + "authorize motion."
                    : string.Empty);
            WriteLog(
                priorOutcome
                    == GroupResetRecoveryPriorOutcome.OutcomeUncertain
                    ? "Group Reset durable recovery verified current stable "
                        + "group/member error clearance. The prior 0x2049 "
                        + "outcome remains unknown; no replay occurred. Old "
                        + "preparation flags remain invalid."
                    : "Group Reset verified stable group/member error clearance. "
                        + "0x2049 was accepted once. Old preparation flags remain "
                        + "invalid; Power On, Set Identity, and Enable are required "
                        + "again before motion.");
        }

        private void SupersedePendingGroupResetByLaterMutation(
            string operation)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    new Action(
                        () => SupersedePendingGroupResetByLaterMutation(
                            operation)));
                return;
            }

            var unresolvedReset = HasUnresolvedGroupResetState();
            if (!unresolvedReset && !groupResetObservedLockedStandby)
            {
                return;
            }

            groupResetObservedLockedStandby = false;
            deferredGroupResetSubmissionUncertainEvidence = null;
            deferredGroupResetSubmissionUncertainReason = null;
            if (!unresolvedReset)
            {
                return;
            }

            ResolveActiveGroupResetSafetySupersede(operation);

            groupResetSupersededByLaterMutation = true;
            groupResetSessionContinuationDiscarded = false;
            groupResetSubmissionUncertain = false;
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();
            WriteLog(
                operation
                + " superseded the pending Group Reset completion "
                + "attribution. The old Reset will not be resumed or replayed. "
                + "Preparation remains fail-closed.");
        }

        private static LMCGroupDisableWaitEvidence
            GetGroupDisableWaitEvidence(Exception error)
        {
            var rejected = error as LMCGroupDisableRejectedException;
            if (rejected != null)
            {
                return rejected.Evidence;
            }

            var submission = error as LMCGroupDisableSubmissionException;
            if (submission != null)
            {
                return submission.Evidence;
            }

            var timeout = error as LMCGroupDisableWaitTimeoutException;
            if (timeout != null)
            {
                return timeout.Evidence;
            }

            var canceled = error as LMCGroupDisableWaitCanceledException;
            if (canceled != null)
            {
                return canceled.Evidence;
            }

            var status = error as LMCGroupDisableStatusException;
            if (status != null)
            {
                return status.Evidence;
            }

            var interference = error as LMCGroupDisableInterferenceException;
            return interference == null ? null : interference.Evidence;
        }

        private void SupersedePendingGroupResetByMemberAxisMutation(
            LMCSingleAxis currentAxis,
            string operation)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    new Action(
                        () => SupersedePendingGroupResetByMemberAxisMutation(
                            currentAxis,
                            operation)));
                return;
            }

            if (currentAxis == null || !HasUnresolvedGroupResetState())
            {
                return;
            }

            var currentGroup = group;
            var continuation = pendingGroupResetWaitContinuation;
            if (continuation == null && currentGroup != null)
            {
                continuation = currentGroup.PendingGroupResetWaitContinuation;
            }
            if (continuation == null || currentGroup == null)
            {
                if (IsDurableGroupResetMember(currentAxis))
                {
                    SupersedePendingGroupResetByLaterMutation(operation);
                }
                return;
            }

            var members = continuation.Members;
            for (var index = 0; index < members.Length; index++)
            {
                if (members[index].AxisReference
                    == currentAxis.AxisReference)
                {
                    var terminalized = currentGroup
                        .SupersedePendingGroupResetAfterCapturedMemberSafetyMutation(
                            continuation,
                            currentAxis);
                    if (!terminalized && continuation.IsPending)
                    {
                        WriteLog(
                            operation
                            + " did not reconcile the SDK Group Reset "
                            + "continuation. The WPF Reset interlock remains "
                            + "fail-closed.");
                        return;
                    }

                    SupersedePendingGroupResetByLaterMutation(operation);
                    return;
                }
            }
        }

        private string GetGroupResetRecoveryGuidance()
        {
            if (GroupResetRecoveryJournalUnavailable)
            {
                return "The Group Reset recovery journal is unavailable or "
                    + "corrupt. New mutations are fail-closed; no 0x2049 is sent.";
            }

            if (HasActiveGroupResetRecoveryRecord
                && pendingGroupResetWaitContinuation == null)
            {
                return "A durable Group Reset recovery record is active. "
                    + "Reconnect with the exact PLC/local callback endpoint, "
                    + "DiagnosticsBuild, BootId, and MapRevision; then load the "
                    + "exact group. Recovery refreshes 0x20D2 once and sends only "
                    + "0x2045/0x2028 status reads. It never replays 0x2049.";
            }

            if (IsAttachedOutcomeUncertainGroupResetRecovery)
            {
                return "Outcome-uncertain Group Reset recovery is attached. "
                    + "Resume status-only group/member clearance proof; the "
                    + "prior 0x2049 outcome remains unknown and is never replayed.";
            }

            if (groupResetSubmissionUncertain)
            {
                return "Group Reset may have been sent, but no accepted "
                    + "continuation exists. Fresh 0x2049 and reconnect are "
                    + "blocked. Use Group Stop, Power Off, safe Disable, or "
                    + "disconnect; status reads are inspection only.";
            }

            return "Resume Reset Verification in the same live session to send "
                + "0x2045/0x2028 reads only, or use Group Stop, Power Off, or "
                + "safe Disable. Close and new mutations remain blocked while "
                + "the accepted Reset is pending. Its exact identity and member "
                + "snapshot are durably retained across disconnect or restart.";
        }

        private void DiscardPendingGroupResetAfterConnectionLoss(
            string reason)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    new Action(
                        () => DiscardPendingGroupResetAfterConnectionLoss(
                            reason)));
                return;
            }

            var discardedAcceptedReset =
                HasUnresolvedGroupResetState();
            if (!discardedAcceptedReset)
            {
                return;
            }

            PromoteGroupResetRecoveryRequired(
                null,
                reason);
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = false;
            groupResetSupersededByLaterMutation = false;
            groupResetObservedLockedStandby = false;
            groupResetSessionContinuationDiscarded = true;
            deferredGroupResetSubmissionUncertainEvidence = null;
            deferredGroupResetSubmissionUncertainReason = null;
            groupStatusRefreshRequired = true;
            InvalidateGroupPreparationAfterAcceptedReset();
            WriteLog(
                "Connection loss invalidated the session-bound Group Reset "
                + "continuation ("
                + reason
                + "). The durable recovery record remains fail-closed. Exact "
                + "reconnect recovery sends a fresh 0x20D2 and status reads "
                + "only; no 0x2049 replay will occur.");
        }

        private void ClearGroupResetSessionState()
        {
            pendingGroupResetWaitContinuation = null;
            groupResetVerificationPending = false;
            groupResetSubmissionUncertain = false;
            groupResetSupersededByLaterMutation = false;
            groupResetSessionContinuationDiscarded = false;
            groupResetObservedLockedStandby = false;
            deferredGroupResetSubmissionUncertainEvidence = null;
            deferredGroupResetSubmissionUncertainReason = null;
            ReapplyCurrentGroupResetRecoveryState();
        }
    }
}

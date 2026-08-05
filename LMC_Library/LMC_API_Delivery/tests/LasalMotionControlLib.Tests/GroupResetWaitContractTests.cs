using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupResetWaitContractTests
    {
        private const string GroupName = "_LMCRobotBase1";
        private const ushort GroupReference = 0x0100;
        private const ushort FirstAxisReference = 1;
        private const ushort SecondAxisReference = 2;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "GroupResetWait.Defaults.OneResetAndCompleteMemberRounds",
                DefaultsSendOneResetAndCompleteMemberRounds);
            tests.Add(
                "GroupResetWait.Stability.MemberErrorResetsProof",
                MemberErrorResetsStableProof);
            tests.Add(
                "GroupResetWait.Rejected.NoStatusAndNoPendingContinuation",
                RejectedResetDoesNotPublishContinuation);
            tests.Add(
                "GroupResetWait.Submission.ResponseLossIsUncertainAndNoPending",
                ResponseLossIsUncertainAndDoesNotPublishContinuation);
            tests.Add(
                "GroupResetWait.PreWire.CanceledIsZeroMembersAndResetWire",
                PreCanceledBeginIsZeroMembersAndResetWire);
            tests.Add(
                "GroupResetWait.Split.TimeoutPreservesAndResumeDoesNotReplay",
                TimeoutPreservesContinuationAndResumeDoesNotReplay);
            tests.Add(
                "GroupResetWait.Status.GroupFailureShortCircuitsMembers",
                GroupStatusFailureShortCircuitsMembers);
            tests.Add(
                "GroupResetWait.Status.MemberFailureShortCircuitsLaterMembers",
                MemberStatusFailureShortCircuitsLaterMembers);
            tests.Add(
                "GroupResetWait.Interference.MemberGenerationIsTypedAndZeroStatusWire",
                MemberMutationGenerationInterferenceIsTyped);
            tests.Add(
                "GroupResetWait.Pending.UnsafeBlockedAndStopSupersedes",
                UnsafeCommandsAreBlockedAndStopSupersedes);
            tests.Add(
                "GroupResetWait.Pending.AdminMoveSyncAsyncAreZeroWire",
                AdminMoveSyncAndAsyncAreZeroWireWhilePending);
            tests.Add(
                "GroupResetWait.SafetyNack.RawAndCompoundRestoreExactPending",
                RejectedRawAndCompoundSafetyCommandsRestorePendingReset);
            tests.Add(
                "GroupResetWait.SafetyNack.ResultDiscardPreservesRejectedEvidence",
                RejectedResultDiscardPreservesConclusiveEvidence);
            tests.Add(
                "GroupResetWait.Preemption.PowerOffWinsBetweenRounds",
                PowerOffPreemptsBetweenStatusRounds);
            tests.Add(
                "GroupResetWait.Preemption.DelayedPowerOffNackDoesNotLeakProvisionalState",
                DelayedPowerOffNackDoesNotLeakIntoActiveResume);
            tests.Add(
                "GroupResetWait.Observer.AcceptedRunsBeforeAnyStatus",
                AcceptedObserverRunsBeforeAnyStatus);
            tests.Add(
                "GroupResetWait.Observer.ReentrantDisableRejectedExternalSerialized",
                DisableSupersedesDuringAcceptedObserver);
            tests.Add(
                "GroupResetWait.Observer.GroupAsyncAndMemberMutationsAreZeroWire",
                ObserverReentrantGroupAsyncAndMemberMutationsAreZeroWire);
            tests.Add(
                "GroupResetWait.Observer.SafetyHandoffHonorsCompoundDeadlines",
                SafetyObserverHandoffHonorsCompoundDeadlines);
            tests.Add(
                "GroupResetWait.Members.InvalidSnapshotIsZeroResetWire",
                InvalidMemberSnapshotIsZeroResetWire);
            tests.Add(
                "GroupResetWait.Identity.SameReferenceDifferentNameIsZeroWire",
                SameReferenceDifferentNameResumeIsZeroWire);
            tests.Add(
                "GroupResetWait.MemberSafety.ExactReconciliationRequiresGenerationMismatch",
                ExactMemberSafetyReconciliationRequiresGenerationMismatch);
            tests.Add(
                "GroupResetWait.Durable.PreparedBoundaryThrowIsZeroResetAndReusable",
                PreparedBoundaryThrowIsZeroResetAndReusable);
            tests.Add(
                "GroupResetWait.Durable.PreparedBoundaryOperationCanceledIsSubmissionFailure",
                PreparedBoundaryOperationCanceledIsSubmissionFailure);
            tests.Add(
                "GroupResetWait.Durable.AttachExactResumeIsStatusOnly",
                DurableAttachExactResumeIsStatusOnly);
            tests.Add(
                "GroupResetWait.Durable.MismatchEveryIdentityFieldFailsClosed",
                DurableMismatchEveryIdentityFieldFailsClosed);
            tests.Add(
                "GroupResetWait.Durable.InvalidOutcomeAndDuplicateFailPreWire",
                DurableInvalidOutcomeAndDuplicateFailPreWire);
            tests.Add(
                "GroupResetWait.Durable.ConcurrentAttachIsSingleSnapshot",
                DurableConcurrentAttachIsSingleSnapshot);
            tests.Add(
                "GroupResetWait.Durable.ZeroBaselinesResumeAndDetectMutations",
                DurableZeroBaselinesResumeAndDetectMutations);
            tests.Add(
                "GroupResetWait.Durable.AttachCancelAndTimeoutAreTyped",
                DurableAttachCancelAndTimeoutAreTyped);
        }

        private static void DefaultsSendOneResetAndCompleteMemberRounds()
        {
            var defaults = new LMCGroupResetWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group
                    .GroupResetAndWaitForStableErrorClearanceAsync(
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.ResetAccepted);
                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.Equal(3, result.StatusRoundCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.Equal(2, result.FinalMemberStatuses.Length);
                AssertEx.Equal(
                    SecondAxisReference,
                    result.FinalMemberStatuses[1].AxisReference);
                AssertEx.False(result.FinalGroupStatus.HasGroupError);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);
                AssertEx.Equal(null, group.PendingGroupResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 3, 6);
            }
        }

        private static void MemberErrorResetsStableProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference, axisErrorId: 7),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group
                    .GroupResetAndWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(5, result.StatusRoundCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.False(
                    result.FinalMemberStatuses[1].Status.HasAxisError);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 5, 10);
            }
        }

        private static void RejectedResetDoesNotPublishContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(false), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                connection.GetGroupEnableWaitCoordinator(
                        group.SessionGeneration,
                        GroupReference)
                    .MarkMutationMayHaveBeenSent();
                connection.GetAxisPowerOnWaitCoordinator(
                        group.SessionGeneration,
                        FirstAxisReference)
                    .MarkMutationMayHaveBeenSent();
                connection.GetAxisPowerOnWaitCoordinator(
                        group.SessionGeneration,
                        SecondAxisReference)
                    .MarkMutationMayHaveBeenSent();
                var error = AssertEx.Throws<LMCGroupResetRejectedException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.Rejected,
                    error.Evidence.SubmissionOutcome);
                AssertEx.Equal(null, group.PendingGroupResetWaitContinuation);
                AssertEx.Equal(0, error.Evidence.StatusRoundCount);
                AssertEx.False(
                    error.Evidence.InterveningMutationDetected);
                AssertEx.Equal(0L,
                    error.Evidence.ResetMutationGeneration);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 0, 0);
            }
        }

        private static void PreCanceledBeginIsZeroMembersAndResetWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                cancellation.Cancel();
                var error = AssertEx.Throws<
                    LMCGroupResetWaitCanceledException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(null, error.Continuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0, 0, 0);
            }
        }

        private static void
            ResponseLossIsUncertainAndDoesNotPublishContinuation()
        {
            var responseLoss = new FakeRpcStep(0x2049, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupReset(GroupReference),
                    request)
            };
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCGroupResetSubmissionException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(), CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.OutcomeUncertain,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.Acknowledgement);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                server.Verify();
                AssertCommandCounts(server, 1, 1, 0, 0);
            }
        }

        private static void
            TimeoutPreservesContinuationAndResumeDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference, axisErrorId: 5),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var timeoutTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupResetWaitTimeoutException>(
                    () => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            new LMCGroupResetWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            timeoutTime.ElapsedMilliseconds,
                            timeoutTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.Equal(continuation,
                    group.PendingGroupResetWaitContinuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(1, continuation.StatusRoundCount);
                AssertEx.Equal(0, continuation.StableSampleCount);

                var resumeTime = new FakeWaitTime();
                var result = group
                    .ResumeGroupResetWaitForStableErrorClearanceAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(4, result.StatusRoundCount);
                AssertEx.Equal(3, result.StableSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 4, 8);
            }
        }

        private static void GroupStatusFailureShortCircuitsMembers()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(functionStatus: 0x0010, errorId: -31),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var failure = AssertEx.Throws<LMCGroupResetStatusException>(
                    () => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.NotNull(failure.FailedGroupStatus);
                AssertEx.Equal(1, failure.Evidence.StatusRoundCount);
                AssertEx.Equal(0, CountCommand(server, 0x2028));
                AssertEx.Equal(continuation,
                    group.PendingGroupResetWaitContinuation);

                var result = group
                    .ResumeGroupResetWaitForStableErrorClearanceAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(4, result.StatusRoundCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 4, 6);
            }
        }

        private static void MemberStatusFailureShortCircuitsLaterMembers()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(
                    FirstAxisReference,
                    functionStatus: 0x0010,
                    errorId: -32),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                var failure = AssertEx.Throws<LMCGroupResetStatusException>(
                    () => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.NotNull(failure.FailedMemberStatus);
                AssertEx.Equal(
                    FirstAxisReference,
                    failure.FailedMemberStatus.AxisReference);
                AssertEx.Equal(0,
                    CountAxisStatus(server, SecondAxisReference));

                var result = group
                    .ResumeGroupResetWaitForStableErrorClearanceAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.Equal(4, result.StatusRoundCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 4, 7);
            }
        }

        private static void MemberMutationGenerationInterferenceIsTyped()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                connection.GetAxisPowerOnWaitCoordinator(
                        group.SessionGeneration,
                        FirstAxisReference)
                    .MarkMutationMayHaveBeenSent();

                var interference = AssertEx.Throws<
                    LMCGroupResetInterferenceException>(
                    () => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.True(
                    interference.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededByInterveningMutation,
                    continuation.State);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 0, 0);
            }
        }

        private static void UnsafeCommandsAreBlockedAndStopSupersedes()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), StopStep(), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.Equal(
                    continuation,
                    AssertEx.Throws<LMCGroupResetWaitPendingException>(
                        () => group.GroupReset()).Continuation);
                AssertEx.Equal(
                    continuation,
                    AssertEx.Throws<LMCGroupResetWaitPendingException>(
                        () => group.GroupEnable()).Continuation);
                AssertEx.True(group.GroupStop(1000, 0).IsSuccess);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2085));
                AssertEx.Equal(0, CountCommand(server, 0x2047));
            }
        }

        private static void AdminMoveSyncAndAsyncAreZeroWireWhilePending()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                CapabilitiesStep(), MembersStep(), ResetStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var capabilities = connection.Admin.GetCapabilities();
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                var distances = new[] { 1, 2, 3, 4 };
                var options = new LMCGroupMotionOptions();

                AssertEx.Equal(
                    continuation,
                    AssertEx.Throws<LMCGroupResetWaitPendingException>(
                        () => group.MoveLinearRelativeEx(
                            distances,
                            100,
                            200,
                            300,
                            0,
                            options,
                            capabilities)).Continuation);
                AssertEx.Equal(
                    continuation,
                    AssertEx.Throws<LMCGroupResetWaitPendingException>(
                        () => group.MoveLinearRelativeExAsync(
                                distances,
                                100,
                                200,
                                300,
                                0,
                                options,
                                capabilities,
                                CancellationToken.None)
                            .GetAwaiter().GetResult()).Continuation);
                AssertEx.Equal(0, CountCommand(server, 0x7D22));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            RejectedRawAndCompoundSafetyCommandsRestorePendingReset()
        {
            AssertRejectedSafetyRestores(
                StopStep(false),
                group => AssertEx.False(
                    group.GroupStop(1000, 0).IsSuccess));
            AssertRejectedSafetyRestores(
                PowerOffStep(false),
                group => AssertEx.False(group.GroupPowerOff().IsSuccess));
            AssertRejectedSafetyRestores(
                DisableStep(false),
                group => AssertEx.False(group.GroupDisable().IsSuccess));
            AssertRejectedSafetyRestores(
                StopStep(false),
                group => AssertEx.Throws<LMCGroupStopRejectedException>(
                    () => group
                        .BeginGroupStopWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter().GetResult()));
            AssertRejectedSafetyRestores(
                PowerOffStep(false),
                group => AssertEx.Throws<LMCGroupPowerRejectedException>(
                    () => group
                        .BeginGroupPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter().GetResult()));
            AssertRejectedSafetyRestores(
                DisableStep(false),
                group => AssertEx.Throws<LMCGroupDisableRejectedException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            CancellationToken.None)
                        .GetAwaiter().GetResult()));
        }

        private static void AssertRejectedSafetyRestores(
            FakeRpcStep rejectedSafetyStep,
            Action<LMCGroupAxis> sendAndAssertRejected)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), rejectedSafetyStep, CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();

                sendAndAssertRejected(group);

                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState.Pending,
                    continuation.State);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupResetWaitContinuation);
                AssertEx.Equal(
                    continuation.ResetMutationGeneration,
                    connection.GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            GroupReference)
                        .MutationGeneration);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            RejectedResultDiscardPreservesConclusiveEvidence()
        {
            ResetNackResultDiscardPreservesRejectedEvidence();
            StopNackResultDiscardPreservesRejectedEvidence();
            PowerOffNackResultDiscardPreservesRejectedEvidence();
            DisableNackResultDiscardPreservesRejectedEvidence();
        }

        private static void
            ResetNackResultDiscardPreservesRejectedEvidence()
        {
            var priority = new LMCSendPriorityCoordinator();
            var connectionOptions = new LMCConnectionOptions
            {
                SendPriorityCoordinator = priority
            };
            using (var received = new ManualResetEventSlim(false))
            using (var release = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                DelayedStep(ResetStep(false), received, release),
                CloseStep()))
            using (var connection = new LMCConnection(connectionOptions))
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var expectedGeneration = priority.CurrentGeneration;
                    var operation = Task.Run(() =>
                    {
                        using (priority.BeginPreemptibleScope(
                            expectedGeneration,
                            "Rejected Group Reset"))
                        {
                            return group
                                .BeginGroupResetWaitForStableErrorClearanceAsync(
                                    LongOptions(),
                                    CancellationToken.None)
                                .GetAwaiter().GetResult();
                        }
                    });
                    AssertEx.True(received.Wait(2000));
                    priority.ReservePrioritySend();
                    release.Set();

                    var error = AssertEx.Throws<
                        LMCGroupResetSubmissionException>(
                        () => operation.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupResetSubmissionOutcome.Rejected,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.False(error.Evidence.Acknowledgement.IsSuccess);
                    AssertEx.Equal(0L,
                        error.Evidence.ResetMutationGeneration);
                    AssertResultDiscard(error.InnerException);
                    AssertEx.Equal(null,
                        group.PendingGroupResetWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                }
                finally
                {
                    release.Set();
                }
            }
        }

        private static void StopNackResultDiscardPreservesRejectedEvidence()
        {
            AssertSafetyNackResultDiscardPreservesPending(
                StopStep(false),
                (group, priority) =>
                {
                    var expectedGeneration = priority.CurrentGeneration;
                    return Task.Run(() =>
                    {
                        using (priority.BeginPreemptibleScope(
                            expectedGeneration,
                            "Rejected Group Stop"))
                        {
                            group.BeginGroupStopWaitForStableStandbyAsync(
                                    1000,
                                    0,
                                    CancellationToken.None)
                                .GetAwaiter().GetResult();
                        }
                    });
                },
                task =>
                {
                    var error = AssertEx.Throws<
                        LMCGroupStopSubmissionException>(
                        () => task.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.Rejected,
                        error.Evidence.SubmissionOutcome);
                    AssertResultDiscard(error.InnerException);
                });
        }

        private static void
            PowerOffNackResultDiscardPreservesRejectedEvidence()
        {
            AssertSafetyNackResultDiscardPreservesPending(
                PowerOffStep(false),
                (group, priority) =>
                {
                    var expectedGeneration = priority.CurrentGeneration;
                    return Task.Run(() =>
                    {
                        using (priority.BeginPreemptibleScope(
                            expectedGeneration,
                            "Rejected Group Power Off"))
                        {
                            group.BeginGroupPowerOffWaitForStableStateAsync(
                                    CancellationToken.None)
                                .GetAwaiter().GetResult();
                        }
                    });
                },
                task =>
                {
                    var error = AssertEx.Throws<
                        LMCGroupPowerSubmissionException>(
                        () => task.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupPowerSubmissionOutcome.Rejected,
                        error.Evidence.SubmissionOutcome);
                    AssertResultDiscard(error.InnerException);
                });
        }

        private static void
            DisableNackResultDiscardPreservesRejectedEvidence()
        {
            AssertSafetyNackResultDiscardPreservesPending(
                DisableStep(false),
                (group, priority) =>
                {
                    var expectedGeneration = priority.CurrentGeneration;
                    return Task.Run(() =>
                    {
                        using (priority.BeginPreemptibleScope(
                            expectedGeneration,
                            "Rejected Group Disable"))
                        {
                            group.BeginGroupDisableWaitForStableDisabledAsync(
                                    CancellationToken.None)
                                .GetAwaiter().GetResult();
                        }
                    });
                },
                task =>
                {
                    var error = AssertEx.Throws<
                        LMCGroupDisableSubmissionException>(
                        () => task.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupDisableSubmissionOutcome.Rejected,
                        error.Evidence.SubmissionOutcome);
                    AssertResultDiscard(error.InnerException);
                });
        }

        private static void AssertSafetyNackResultDiscardPreservesPending(
            FakeRpcStep rejectedStep,
            Func<LMCGroupAxis, LMCSendPriorityCoordinator, Task>
                startOperation,
            Action<Task> assertOperation)
        {
            var priority = new LMCSendPriorityCoordinator();
            var connectionOptions = new LMCConnectionOptions
            {
                SendPriorityCoordinator = priority
            };
            using (var received = new ManualResetEventSlim(false))
            using (var release = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                DelayedStep(rejectedStep, received, release),
                CloseStep()))
            using (var connection = new LMCConnection(connectionOptions))
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(), CancellationToken.None)
                        .GetAwaiter().GetResult();
                    var operation = startOperation(group, priority);
                    AssertEx.True(received.Wait(2000));
                    priority.ReservePrioritySend();
                    release.Set();
                    assertOperation(operation);

                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        continuation,
                        group.PendingGroupResetWaitContinuation);
                    AssertEx.Equal(
                        continuation.ResetMutationGeneration,
                        connection.GetGroupEnableWaitCoordinator(
                                group.SessionGeneration,
                                GroupReference)
                            .MutationGeneration);

                    connection.CloseConnection();
                    server.Verify();
                }
                finally
                {
                    release.Set();
                }
            }
        }

        private static void AssertResultDiscard(Exception exception)
        {
            AssertEx.True(exception is LMCSendPreemptedException);
            AssertEx.Equal(
                LMCSendPreemptionPhase.ResultDiscarded,
                ((LMCSendPreemptedException)exception).Phase);
        }

        private static void PowerOffPreemptsBetweenStatusRounds()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference, axisErrorId: 5),
                AxisStatusStep(SecondAxisReference),
                PowerOffStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                var time = new ActionWaitTime(
                    () => AssertEx.True(group.GroupPowerOff().IsSuccess));

                var interference = AssertEx.Throws<
                    LMCGroupResetInterferenceException>(
                    () => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter().GetResult());
                AssertEx.True(
                    interference.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.Equal(1, CountCommand(server, 0x2045));
                AssertEx.Equal(1, CountCommand(server, 0x204B));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            DelayedPowerOffNackDoesNotLeakIntoActiveResume()
        {
            using (var firstDelayEntered = new ManualResetEventSlim(false))
            using (var releaseFirstDelay = new ManualResetEventSlim(false))
            using (var powerOffReceived = new ManualResetEventSlim(false))
            using (var releasePowerOff = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                DelayedStep(
                    PowerOffStep(false),
                    powerOffReceived,
                    releasePowerOff),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(), CancellationToken.None)
                        .GetAwaiter().GetResult();
                    var time = new BlockingFirstDelayTime(
                        firstDelayEntered,
                        releaseFirstDelay);
                    var resume = Task.Run(() => group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter().GetResult());
                    AssertEx.True(firstDelayEntered.Wait(2000));

                    var powerOff = Task.Run(() => group.GroupPowerOff());
                    AssertEx.True(powerOffReceived.Wait(2000));
                    releaseFirstDelay.Set();
                    AssertEx.False(resume.Wait(50));

                    releasePowerOff.Set();
                    AssertEx.False(
                        powerOff.GetAwaiter().GetResult().IsSuccess);
                    var result = resume.GetAwaiter().GetResult();
                    AssertEx.Equal(3, result.StatusRoundCount);
                    AssertEx.Equal(3, result.StableSampleCount);
                    AssertEx.Equal(
                        LMCGroupResetWaitContinuationState.Completed,
                        continuation.State);
                    AssertEx.Equal(null,
                        group.PendingGroupResetWaitContinuation);
                    AssertEx.Equal(1, CountCommand(server, 0x204B));

                    connection.CloseConnection();
                    server.Verify();
                }
                finally
                {
                    releaseFirstDelay.Set();
                    releasePowerOff.Set();
                }
            }
        }

        private static void AcceptedObserverRunsBeforeAnyStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerCalled = false;
                var result = group
                    .GroupResetAndWaitForStableErrorClearanceAsync(
                        new LMCGroupResetWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 1
                        },
                        continuation =>
                        {
                            AssertEx.True(continuation.IsPending);
                            AssertEx.True(
                                continuation.Acknowledgement.IsSuccess);
                            AssertEx.Equal(1,
                                CountCommand(server, 0x2049));
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2045));
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2028));
                            observerCalled = true;
                        },
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.True(observerCalled);
                AssertEx.Equal(1, result.StatusRoundCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 1, 2);
            }
        }

        private static void DisableSupersedesDuringAcceptedObserver()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), DisableStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            using (var externalStarted = new ManualResetEvent(false))
            using (var externalCompleted = new ManualResetEvent(false))
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                Thread externalThread = null;
                LMC_Response externalResponse = null;
                Exception externalError = null;

                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        accepted =>
                        {
                            var reentrant = AssertEx.Throws<
                                InvalidOperationException>(
                                () => group.GroupDisable());
                            AssertEx.False(reentrant is
                                LMCGroupResetWaitPendingException);
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2048));

                            externalThread = new Thread(() =>
                            {
                                externalStarted.Set();
                                try
                                {
                                    externalResponse =
                                        group.GroupDisable();
                                }
                                catch (Exception ex)
                                {
                                    externalError = ex;
                                }
                                finally
                                {
                                    externalCompleted.Set();
                                }
                            });
                            externalThread.IsBackground = true;
                            using (ExecutionContext.SuppressFlow())
                            {
                                externalThread.Start();
                            }
                            AssertEx.True(externalStarted.WaitOne(1000));
                            AssertEx.False(externalCompleted.WaitOne(50));
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2048));
                        },
                        CancellationToken.None)
                    .GetAwaiter().GetResult();

                AssertEx.True(externalCompleted.WaitOne(1000));
                externalThread.Join();
                AssertEx.Equal<Exception>(null, externalError);
                AssertEx.True(externalResponse.IsSuccess);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.Equal(1, CountCommand(server, 0x2048));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            ObserverReentrantGroupAsyncAndMemberMutationsAreZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                AxisLookupStep(), AxisInfoStep(), MembersStep(),
                ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var axis = new LMCAxis(connection, "Axis1");

                group.BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        continuation =>
                        {
                            AssertEx.Throws<InvalidOperationException>(
                                () => group.GroupResetAsync(
                                        CancellationToken.None)
                                    .GetAwaiter().GetResult());
                            AssertEx.Throws<InvalidOperationException>(
                                () => axis.PowerOn());
                            AssertEx.Throws<InvalidOperationException>(
                                () => axis
                                    .BeginStopWaitForStableStandstillAsync(
                                        1000,
                                        0,
                                        CancellationToken.None)
                                    .GetAwaiter().GetResult());
                            AssertEx.Throws<InvalidOperationException>(
                                () => axis
                                    .BeginResetWaitForStableErrorClearanceAsync(
                                        CancellationToken.None)
                                    .GetAwaiter().GetResult());
                        },
                        CancellationToken.None)
                    .GetAwaiter().GetResult();

                AssertEx.Equal(1, CountCommand(server, 0x2049));
                AssertEx.Equal(0, CountCommand(server, 0x2023));
                AssertEx.Equal(0, CountCommand(server, 0x2022));
                AssertEx.Equal(0, CountCommand(server, 0x2024));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void SafetyObserverHandoffHonorsCompoundDeadlines()
        {
            using (var observerEntered = new ManualResetEventSlim(false))
            using (var releaseObserver = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var reset = Task.Run(() => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            new LMCGroupResetWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            continuation =>
                            {
                                observerEntered.Set();
                                AssertEx.True(releaseObserver.Wait(5000));
                            },
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                    AssertEx.True(observerEntered.Wait(2000));

                    var stopTimeout = AssertEx.Throws<
                        LMCGroupStopWaitTimeoutException>(
                        () => group
                            .BeginGroupStopWaitForStableStandbyAsync(
                                1000,
                                0,
                                new LMCGroupStopWaitOptions
                                {
                                    TimeoutMilliseconds = 25,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertEx.False(
                        stopTimeout.Evidence.CommandMayHaveBeenSent);

                    var powerTimeout = AssertEx.Throws<
                        LMCGroupPowerStateWaitTimeoutException>(
                        () => group
                            .BeginGroupPowerOffWaitForStableStateAsync(
                                new LMCGroupPowerStateWaitOptions
                                {
                                    TimeoutMilliseconds = 25,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertEx.False(
                        powerTimeout.Evidence.CommandMayHaveBeenSent);

                    var disableTimeout = AssertEx.Throws<
                        LMCGroupDisableWaitTimeoutException>(
                        () => group
                            .BeginGroupDisableWaitForStableDisabledAsync(
                                new LMCGroupDisableWaitOptions
                                {
                                    TimeoutMilliseconds = 25,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertEx.False(
                        disableTimeout.Evidence.CommandMayHaveBeenSent);
                    AssertEx.Equal(0, CountCommand(server, 0x2085));
                    AssertEx.Equal(0, CountCommand(server, 0x204B));
                    AssertEx.Equal(0, CountCommand(server, 0x2048));

                    releaseObserver.Set();
                    AssertEx.True(reset.GetAwaiter().GetResult().IsPending);
                    connection.CloseConnection();
                    server.Verify();
                }
                finally
                {
                    releaseObserver.Set();
                }
            }
        }

        private static void
            ExactMemberSafetyReconciliationRequiresGenerationMismatch()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                AxisLookupStep(), AxisInfoStep(), MembersStep(),
                ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var axis = new LMCAxis(connection, "Axis1");
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        group.SessionGeneration,
                        FirstAxisReference);

                var rejectedGeneration =
                    coordinator.MarkMutationMayHaveBeenSent();
                AssertEx.True(coordinator.TryRollbackRejectedMutation(
                    rejectedGeneration));
                AssertEx.False(group
                    .SupersedePendingGroupResetAfterCapturedMemberSafetyMutation(
                        continuation,
                        axis));
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupResetWaitContinuation);

                var groupCoordinator = connection
                    .GetGroupEnableWaitCoordinator(
                        group.SessionGeneration,
                        GroupReference);
                var rejectedGroupSafetyGeneration = groupCoordinator
                    .MarkMutationMayHaveBeenSent(true);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);

                coordinator.MarkMutationMayHaveBeenSent();
                AssertEx.True(group
                    .SupersedePendingGroupResetAfterCapturedMemberSafetyMutation(
                        continuation,
                        axis));
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                AssertEx.True(groupCoordinator
                    .TryRestoreGroupResetAfterRejectedSafetyMutation(
                        rejectedGroupSafetyGeneration));
                AssertEx.Equal(
                    continuation.ResetMutationGeneration,
                    groupCoordinator.MutationGeneration);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                AssertEx.Equal(
                    LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation,
                    continuation.State);
                AssertEx.False(group
                    .SupersedePendingGroupResetAfterCapturedMemberSafetyMutation(
                        continuation,
                        axis));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void InvalidMemberSnapshotIsZeroResetWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                MembersStep(FirstAxisReference, FirstAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCGroupResetSubmissionException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(), CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 0, 0);
            }
        }

        private static void SameReferenceDifferentNameResumeIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), LookupStep(),
                MembersStep(), ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var otherName = new LMCGroup(
                    connection,
                    "_LMCRobotBaseAlias");
                var continuation = group
                    .BeginGroupResetWaitForStableErrorClearanceAsync(
                        LongOptions(), CancellationToken.None)
                    .GetAwaiter().GetResult();

                AssertEx.Throws<InvalidOperationException>(
                    () => otherName
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(0, CountCommand(server, 0x2045));
                AssertEx.Equal(0, CountCommand(server, 0x2028));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreparedBoundaryThrowIsZeroResetAndReusable()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                MembersStep(), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var marker = new InvalidOperationException("journal failed");
                LMCGroupResetPreparedEvidence prepared = null;
                var error = AssertEx.Throws<
                    LMCGroupResetSubmissionException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            evidence =>
                            {
                                prepared = evidence;
                                AssertEx.Equal(1,
                                    CountCommand(server, 0x20D2));
                                AssertEx.Equal(0,
                                    CountCommand(server, 0x2049));
                                AssertEx.Equal(GroupName,
                                    evidence.GroupName);
                                AssertEx.Equal(GroupReference,
                                    evidence.GroupReference);
                                AssertEx.True(evidence.SessionGeneration > 0);
                                AssertEx.Equal(3,
                                    evidence.RequiredStableSampleCount);
                                AssertEx.Equal(
                                    LMCGroupResetSubmissionOutcome
                                        .NotAttempted,
                                    evidence.SubmissionOutcome);
                                AssertEx.False(
                                    evidence.RecoveredFromDurableRecord);
                                AssertEx.False(
                                    evidence.CommandDispatchedInOwnerSession);
                                AssertEx.Throws<InvalidOperationException>(
                                    () => group.GroupReset());
                                AssertEx.Equal(0,
                                    CountCommand(server, 0x2049));

                                var copy = evidence.Members;
                                copy[0] = null;
                                AssertEx.True(
                                    evidence.Members[0] != null);
                                throw marker;
                            },
                            null,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());

                AssertEx.True(ReferenceEquals(marker,
                    error.InnerException));
                AssertEx.True(prepared != null);
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(
                    error.Evidence.CommandDispatchedInOwnerSession);
                AssertEx.Equal(0, CountCommand(server, 0x2049));

                var supplied = prepared.Members;
                var record = new LMCGroupResetDurableRecoveryRecord(
                    prepared.OperationId,
                    LMCGroupResetSubmissionOutcome.Accepted,
                    prepared.GroupName,
                    prepared.GroupReference,
                    prepared.SessionGeneration,
                    supplied,
                    prepared.RequiredStableSampleCount);
                supplied[0] = null;
                AssertEx.True(record.Members[0] != null);
                var recordCopy = record.Members;
                recordCopy[0] = null;
                AssertEx.True(record.Members[0] != null);

                AssertEx.True(group.GetGroupMembersInfoResult().IsSuccess);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 2, 0, 0);
            }
        }

        private static void DurableAttachExactResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                GroupStatusStep(),
                AxisStatusStep(FirstAxisReference),
                AxisStatusStep(SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var record = CreateDurableRecord(
                    LMCGroupResetSubmissionOutcome.Accepted,
                    77,
                    1);
                var options = OneSampleOptions();
                var continuation = group
                    .AttachGroupResetDurableRecoveryAsync(
                        record,
                        options,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();

                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.RecoveredFromDurableRecord);
                AssertEx.False(
                    continuation.CommandDispatchedInOwnerSession);
                AssertEx.Equal(77L,
                    continuation.CommandOwnerSessionGeneration);
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.Accepted,
                    continuation.SubmissionOutcome);
                AssertEx.Equal(record.OperationId,
                    continuation.OperationId);
                AssertEx.Equal(0L,
                    continuation.ResetMutationGeneration);
                var attachedEvidence = continuation.CaptureEvidence(0);
                AssertEx.True(
                    attachedEvidence.MutationBaselineCaptured);
                AssertEx.Equal(2,
                    attachedEvidence.MemberMutations.Length);
                AssertEx.True(attachedEvidence
                    .MemberMutations[0].MutationBaselineCaptured);
                AssertEx.Equal(0L, attachedEvidence
                    .MemberMutations[0].ExpectedMutationGeneration);

                var result = group
                    .ResumeGroupResetWaitForStableErrorClearanceAsync(
                        continuation,
                        options,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.True(result.ResetAccepted);
                AssertEx.True(result.RecoveredFromDurableRecord);
                AssertEx.False(
                    result.CommandDispatchedInOwnerSession);
                AssertEx.Equal(1, result.StatusRoundCount);
                AssertEx.Equal(null, result.Acknowledgement);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 1, 2);
            }
        }

        private static void
            PreparedBoundaryOperationCanceledIsSubmissionFailure()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                MembersStep(), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var marker = new OperationCanceledException(
                    "journal observer canceled independently");
                var error = AssertEx.Throws<
                    LMCGroupResetSubmissionException>(
                    () => group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            evidence => { throw marker; },
                            null,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());

                AssertEx.True(ReferenceEquals(marker,
                    error.InnerException));
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(
                    error.Evidence.CommandDispatchedInOwnerSession);
                AssertEx.Equal(0, CountCommand(server, 0x2049));
                AssertEx.True(group.GetGroupMembersInfoResult().IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 2, 0, 0);
            }
        }

        private static void DurableMismatchEveryIdentityFieldFailsClosed()
        {
            AssertDurablePreWireFailure(
                new LMCGroupResetDurableRecoveryRecord(
                    Guid.NewGuid(),
                    LMCGroupResetSubmissionOutcome.Accepted,
                    GroupName + "Alias",
                    GroupReference,
                    1,
                    DurableMembers(),
                    3),
                LMCGroupResetDurableRecoveryFailureKind
                    .GroupIdentityMismatch);
            AssertDurablePreWireFailure(
                new LMCGroupResetDurableRecoveryRecord(
                    Guid.NewGuid(),
                    LMCGroupResetSubmissionOutcome.Accepted,
                    GroupName,
                    (ushort)(GroupReference + 1),
                    1,
                    DurableMembers(),
                    3),
                LMCGroupResetDurableRecoveryFailureKind
                    .GroupIdentityMismatch);

            AssertDurableMemberMismatch(new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, FirstAxisReference, 0x1101, "Axis1")
            });
            AssertDurableMemberMismatch(new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, SecondAxisReference, 0x1102, "Axis2"),
                new LMCGroupResetDurableMemberIdentity(
                    1, FirstAxisReference, 0x1101, "Axis1")
            });
            AssertDurableMemberMismatch(new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, 3, 0x1101, "Axis1"),
                new LMCGroupResetDurableMemberIdentity(
                    1, SecondAxisReference, 0x1102, "Axis2")
            });
            AssertDurableMemberMismatch(new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, FirstAxisReference, 0x2201, "Axis1"),
                new LMCGroupResetDurableMemberIdentity(
                    1, SecondAxisReference, 0x1102, "Axis2")
            });
            AssertDurableMemberMismatch(new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, FirstAxisReference, 0x1101, "AxisX"),
                new LMCGroupResetDurableMemberIdentity(
                    1, SecondAxisReference, 0x1102, "Axis2")
            });

            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                MembersStep(firstName: "AxisX"), MembersStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var record = CreateDurableRecord(
                    LMCGroupResetSubmissionOutcome.Accepted,
                    1);
                AssertEx.Throws<LMCGroupResetDurableRecoveryException>(
                    () => group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                var retry = group
                    .AttachGroupResetDurableRecoveryAsync(
                        record,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.True(retry.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 2, 0, 0);
            }
        }

        private static void DurableInvalidOutcomeAndDuplicateFailPreWire()
        {
            AssertDurablePreWireFailure(
                CreateDurableRecord(
                    LMCGroupResetSubmissionOutcome.NotAttempted,
                    1),
                LMCGroupResetDurableRecoveryFailureKind.InvalidRecord);
            AssertDurablePreWireFailure(
                CreateDurableRecord(
                    LMCGroupResetSubmissionOutcome.Rejected,
                    1),
                LMCGroupResetDurableRecoveryFailureKind.InvalidRecord);

            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var record = CreateDurableRecord(
                    LMCGroupResetSubmissionOutcome.OutcomeUncertain,
                    11);
                var continuation = group
                    .AttachGroupResetDurableRecoveryAsync(
                        record,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                AssertEx.Equal(
                    LMCGroupResetSubmissionOutcome.OutcomeUncertain,
                    continuation.SubmissionOutcome);

                var duplicate = AssertEx.Throws<
                    LMCGroupResetDurableRecoveryException>(
                    () => group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(
                    LMCGroupResetDurableRecoveryFailureKind
                        .DuplicateAttachment,
                    duplicate.FailureKind);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 0, 0);
            }
        }

        private static void DurableConcurrentAttachIsSingleSnapshot()
        {
            using (var received = new ManualResetEventSlim(false))
            using (var release = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(),
                DelayedStep(MembersStep(), received, release),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var record = CreateDurableRecord(
                        LMCGroupResetSubmissionOutcome.Accepted,
                        22);
                    var first = group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            LongOptions(),
                            CancellationToken.None);
                    AssertEx.True(received.Wait(2000));
                    var second = group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            LongOptions(),
                            CancellationToken.None);
                    AssertEx.False(second.IsCompleted);
                    release.Set();
                    AssertEx.True(first.GetAwaiter().GetResult().IsPending);
                    var duplicate = AssertEx.Throws<
                        LMCGroupResetDurableRecoveryException>(
                        () => second.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupResetDurableRecoveryFailureKind
                            .DuplicateAttachment,
                        duplicate.FailureKind);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 0, 1, 0, 0);
                }
                finally
                {
                    release.Set();
                }
            }
        }

        private static void DurableZeroBaselinesResumeAndDetectMutations()
        {
            AssertDurableZeroBaselineBehavior(-1);
            AssertDurableZeroBaselineBehavior(0);
            AssertDurableZeroBaselineBehavior(1);
            AssertDurableZeroBaselineBehavior(2);
        }

        private static void DurableAttachCancelAndTimeoutAreTyped()
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), CloseStep()))
            using (var connection = new LMCConnection())
            using (var canceled = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                canceled.Cancel();
                AssertEx.Throws<
                    LMCGroupResetDurableRecoveryCanceledException>(
                    () => group
                        .AttachGroupResetDurableRecoveryAsync(
                            CreateDurableRecord(
                                LMCGroupResetSubmissionOutcome.Accepted,
                                1),
                            canceled.Token)
                        .GetAwaiter().GetResult());
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0, 0, 0);
            }

            using (var observerEntered = new ManualResetEventSlim(false))
            using (var releaseObserver = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                ResetStep(true), CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var begin = group
                        .BeginGroupResetWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            continuation =>
                            {
                                observerEntered.Set();
                                AssertEx.True(
                                    releaseObserver.Wait(5000));
                            },
                            CancellationToken.None);
                    AssertEx.True(observerEntered.Wait(2000));

                    var timeoutOptions = new LMCGroupResetWaitOptions
                    {
                        TimeoutMilliseconds = 25,
                        PollIntervalMilliseconds = 10,
                        StableSampleCount = 3
                    };
                    AssertEx.Throws<
                        LMCGroupResetDurableRecoveryTimeoutException>(
                        () => group
                            .AttachGroupResetDurableRecoveryAsync(
                                CreateDurableRecord(
                                    LMCGroupResetSubmissionOutcome
                                        .Accepted,
                                    2),
                                timeoutOptions,
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertCommandCounts(server, 1, 1, 0, 0);
                    releaseObserver.Set();
                    AssertEx.True(begin.GetAwaiter().GetResult().IsPending);
                    connection.CloseConnection();
                    server.Verify();
                }
                finally
                {
                    releaseObserver.Set();
                }
            }
        }

        private static void AssertDurablePreWireFailure(
            LMCGroupResetDurableRecoveryRecord record,
            LMCGroupResetDurableRecoveryFailureKind expectedKind)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCGroupResetDurableRecoveryException>(
                    () => group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(expectedKind, error.FailureKind);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0, 0, 0);
            }
        }

        private static void AssertDurableMemberMismatch(
            LMCGroupResetDurableMemberIdentity[] members)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), LookupStep(), MembersStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var record = new LMCGroupResetDurableRecoveryRecord(
                    Guid.NewGuid(),
                    LMCGroupResetSubmissionOutcome.Accepted,
                    GroupName,
                    GroupReference,
                    1,
                    members,
                    3);
                var error = AssertEx.Throws<
                    LMCGroupResetDurableRecoveryException>(
                    () => group
                        .AttachGroupResetDurableRecoveryAsync(
                            record,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(
                    LMCGroupResetDurableRecoveryFailureKind
                        .MemberSnapshotMismatch,
                    error.FailureKind);
                AssertEx.Equal(null,
                    group.PendingGroupResetWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 0, 0);
            }
        }

        private static void AssertDurableZeroBaselineBehavior(
            int mutationTarget)
        {
            var steps = new List<FakeRpcStep>
            {
                InitStep(), CallbackStep(), LookupStep(), MembersStep()
            };
            if (mutationTarget < 0)
            {
                steps.Add(GroupStatusStep());
                steps.Add(AxisStatusStep(FirstAxisReference));
                steps.Add(AxisStatusStep(SecondAxisReference));
            }
            steps.Add(CloseStep());

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = OneSampleOptions();
                var continuation = group
                    .AttachGroupResetDurableRecoveryAsync(
                        CreateDurableRecord(
                            LMCGroupResetSubmissionOutcome.Accepted,
                            33 + mutationTarget,
                            1),
                        options,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var evidence = continuation.CaptureEvidence(0);
                AssertEx.True(evidence.MutationBaselineCaptured);
                AssertEx.Equal(0L, evidence.ResetMutationGeneration);
                AssertEx.Equal(0L,
                    evidence.ObservedGroupMutationGeneration);
                for (var index = 0;
                    index < evidence.MemberMutations.Length;
                    index++)
                {
                    AssertEx.True(evidence.MemberMutations[index]
                        .MutationBaselineCaptured);
                    AssertEx.Equal(0L, evidence.MemberMutations[index]
                        .ExpectedMutationGeneration);
                }

                if (mutationTarget < 0)
                {
                    var result = group
                        .ResumeGroupResetWaitForStableErrorClearanceAsync(
                            continuation,
                            options,
                            CancellationToken.None)
                        .GetAwaiter().GetResult();
                    AssertEx.Equal(1, result.StatusRoundCount);
                    AssertCommandCounts(server, 0, 1, 1, 2);
                }
                else
                {
                    if (mutationTarget == 0)
                    {
                        connection.GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            GroupReference)
                            .MarkMutationMayHaveBeenSent(true);
                    }
                    else
                    {
                        var axisReference = mutationTarget == 1
                            ? FirstAxisReference
                            : SecondAxisReference;
                        connection.GetAxisPowerOnWaitCoordinator(
                            group.SessionGeneration,
                            axisReference)
                            .MarkMutationMayHaveBeenSent();
                    }

                    var interference = AssertEx.Throws<
                        LMCGroupResetInterferenceException>(
                        () => group
                            .ResumeGroupResetWaitForStableErrorClearanceAsync(
                                continuation,
                                options,
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertEx.True(interference.Evidence
                        .InterveningMutationDetected);
                    AssertCommandCounts(server, 0, 1, 0, 0);
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static LMCGroupResetDurableRecoveryRecord
            CreateDurableRecord(
                LMCGroupResetSubmissionOutcome outcome,
                long ownerSessionGeneration,
                int requiredStableSampleCount = 3)
        {
            return new LMCGroupResetDurableRecoveryRecord(
                Guid.NewGuid(),
                outcome,
                GroupName,
                GroupReference,
                ownerSessionGeneration,
                DurableMembers(),
                requiredStableSampleCount);
        }

        private static LMCGroupResetDurableMemberIdentity[] DurableMembers()
        {
            return new[]
            {
                new LMCGroupResetDurableMemberIdentity(
                    0, FirstAxisReference, 0x1101, "Axis1"),
                new LMCGroupResetDurableMemberIdentity(
                    1, SecondAxisReference, 0x1102, "Axis2")
            };
        }

        private static LMCGroupResetWaitOptions OneSampleOptions()
        {
            return new LMCGroupResetWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 1
            };
        }

        private static LMCGroupAxis ConnectAndCreateGroup(
            LMCConnection connection,
            int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
            return new LMCGroup(connection, GroupName);
        }

        private static LMCGroupResetWaitOptions LongOptions()
        {
            return new LMCGroupResetWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep LookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, GroupReference);
            return new FakeRpcStep(0x1042, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisLookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, FirstAxisReference);
            return new FakeRpcStep(0x103C, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep()
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, FirstAxisReference);
            return new FakeRpcStep(0x202B, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CapabilitiesStep()
        {
            var payload = new byte[40];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, 1);
            TestFrame.WriteUInt32(
                payload,
                16,
                (uint)LMCAdminFeature.GroupLinearRelative);
            TestFrame.WriteUInt32(payload, 20, 0x3F);
            TestFrame.WriteUInt32(
                payload,
                24,
                (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, GroupReference);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 1);
            return new FakeRpcStep(
                0x7D00,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep MembersStep(
            ushort firstReference = FirstAxisReference,
            ushort secondReference = SecondAxisReference,
            ushort firstDeviceId = 0x1101,
            ushort secondDeviceId = 0x1102,
            string firstName = "Axis1",
            string secondName = "Axis2")
        {
            var payload = new byte[1350];
            TestFrame.WriteUInt16(payload, 0, firstReference);
            TestFrame.WriteUInt16(payload, 2, secondReference);
            TestFrame.WriteUInt16(payload, 32, firstDeviceId);
            TestFrame.WriteUInt16(payload, 34, secondDeviceId);
            WriteFixedAscii(payload, 68, firstName);
            WriteFixedAscii(payload, 148, secondName);
            payload[1348] = 2;
            return new FakeRpcStep(
                0x20D2,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep ResetStep(bool success)
        {
            return new FakeRpcStep(
                0x2049,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupReset(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep GroupStatusStep(
            ushort functionStatus = 0,
            short errorId = 0,
            ushort groupErrorId = 0)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, groupErrorId);
            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupReadStatus(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep AxisStatusStep(
            ushort axisReference,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort axisErrorId = 0)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, axisErrorId);
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReadStatus(axisReference),
                    request)
            };
        }

        private static FakeRpcStep StopStep(bool success = true)
        {
            return new FakeRpcStep(
                0x2085,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupStop(
                        GroupReference,
                        1000,
                        0),
                    request)
            };
        }

        private static FakeRpcStep PowerOffStep(bool success)
        {
            return new FakeRpcStep(
                0x204B,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupPowerOff(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep DisableStep(bool success)
        {
            return new FakeRpcStep(
                0x2048,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupDisable(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep DelayedStep(
            FakeRpcStep step,
            ManualResetEventSlim received,
            ManualResetEventSlim release)
        {
            var inspectRequest = step.InspectRequest;
            step.InspectRequest = request =>
            {
                if (inspectRequest != null)
                {
                    inspectRequest(request);
                }
                received.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                release.Wait(5000),
                "The delayed acknowledgement was not released.");
            return step;
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static void WriteFixedAscii(
            byte[] buffer,
            int offset,
            string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        }

        private static int CountCommand(
            FakeRpcServer server,
            ushort command)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == command)
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountAxisStatus(
            FakeRpcServer server,
            ushort axisReference)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == 0x2028
                    && TestFrame.ReadUInt16(request, 6)
                        == axisReference)
                {
                    count++;
                }
            }
            return count;
        }

        private static void AssertCommandCounts(
            FakeRpcServer server,
            int resetCount,
            int membersCount,
            int groupStatusCount,
            int axisStatusCount)
        {
            AssertEx.Equal(resetCount, CountCommand(server, 0x2049));
            AssertEx.Equal(membersCount, CountCommand(server, 0x20D2));
            AssertEx.Equal(groupStatusCount,
                CountCommand(server, 0x2045));
            AssertEx.Equal(axisStatusCount,
                CountCommand(server, 0x2028));
        }

        private sealed class FakeWaitTime
        {
            private long elapsedMilliseconds;

            internal long ElapsedMilliseconds()
            {
                return elapsedMilliseconds;
            }

            internal Task DelayAsync(
                int delayMilliseconds,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                elapsedMilliseconds += delayMilliseconds;
                return Task.CompletedTask;
            }
        }

        private sealed class ActionWaitTime
        {
            private readonly Action onFirstDelay;
            private long elapsedMilliseconds;
            private bool invoked;

            internal ActionWaitTime(Action onFirstDelay)
            {
                this.onFirstDelay = onFirstDelay;
            }

            internal long ElapsedMilliseconds()
            {
                return elapsedMilliseconds;
            }

            internal Task DelayAsync(
                int delayMilliseconds,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                elapsedMilliseconds += delayMilliseconds;
                if (!invoked)
                {
                    invoked = true;
                    onFirstDelay();
                }
                return Task.CompletedTask;
            }
        }

        private sealed class BlockingFirstDelayTime
        {
            private readonly ManualResetEventSlim entered;
            private readonly ManualResetEventSlim release;
            private long elapsedMilliseconds;
            private bool blocked;

            internal BlockingFirstDelayTime(
                ManualResetEventSlim entered,
                ManualResetEventSlim release)
            {
                this.entered = entered;
                this.release = release;
            }

            internal long ElapsedMilliseconds()
            {
                return elapsedMilliseconds;
            }

            internal Task DelayAsync(
                int delayMilliseconds,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                elapsedMilliseconds += delayMilliseconds;
                if (!blocked)
                {
                    blocked = true;
                    entered.Set();
                    AssertEx.True(release.Wait(5000));
                }
                return Task.CompletedTask;
            }
        }
    }
}

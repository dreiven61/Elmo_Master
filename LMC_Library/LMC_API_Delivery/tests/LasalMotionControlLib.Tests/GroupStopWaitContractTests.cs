using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupStopWaitContractTests
    {
        private const string GroupName = "_LMCRobotBase1";
        private const ushort GroupReference = 0x0100;
        private const string SecondGroupName = "_LMCRobotBase2";
        private const ushort SecondGroupReference = 0x0101;
        private const uint Standby = 0x00020000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "GroupStopWait.Defaults.OneStopThenThreeStatuses",
                DefaultsSendOneStopThenThreeStatuses);
            tests.Add(
                "GroupStopWait.Stability.MismatchResetsProof",
                MismatchResetsProof);
            tests.Add(
                "GroupStopWait.PreWire.InvalidAndCanceledAreZeroStopWire",
                InvalidAndCanceledAreZeroStopWire);
            tests.Add(
                "GroupStopWait.Rejected.NoStatusAndTypedEvidence",
                RejectedHasNoStatusAndTypedEvidence);
            tests.Add(
                "GroupStopWait.Timeout.AcceptedEvidenceAndNoReplay",
                TimeoutPreservesAcceptedEvidenceAndDoesNotReplay);
            tests.Add(
                "GroupStopWait.Timeout.NoAckInvalidatesTransport",
                NoAcknowledgementDeadlineInvalidatesTransport);
            tests.Add(
                "GroupStopWait.Timeout.NoStatusInvalidatesTransport",
                NoStatusDeadlinePreservesAcceptedEvidenceAndInvalidatesTransport);
            tests.Add(
                "GroupStopWait.Cancel.AcceptedEvidenceAndReusableConnection",
                CancellationPreservesAcceptedEvidenceAndConnection);
            tests.Add(
                "GroupStopWait.Cancel.PostStopWriteDrainsAcceptedAck",
                CancellationDuringStopResponseDrainsAcceptedAck);
            tests.Add(
                "GroupStopWait.PreWire.CommitCancelAndDeadlineAreZeroWire",
                CommitWindowCancelAndDeadlineAreZeroWireAndReusable);
            tests.Add(
                "GroupStopWait.StatusError.PreservesFailedStatus",
                StatusErrorPreservesFailedStatus);
            tests.Add(
                "GroupStopWait.Submission.ResponseLossIsUncertain",
                ResponseLossIsUncertainAndNotRetried);
            tests.Add(
                "GroupStopWait.Interference.GroupMutationInvalidatesProof",
                InterveningGroupMutationInvalidatesProof);
            tests.Add(
                "GroupStopWait.Interference.QueuedMutationGenerationStartsAtWire",
                QueuedMutationGenerationStartsAtWire);
            tests.Add(
                "GroupStopWait.Submission.ResponseLossResetsEnableProof",
                ResponseLossResetsPendingEnableProof);
            tests.Add(
                "GroupStopWait.Session.CloseDuringFinalStatusRejectsProof",
                CloseDuringFinalStatusRejectsStaleProof);
            tests.Add(
                "GroupStopWait.Split.BeginThenResumeIsStatusOnly",
                SplitBeginThenResumeIsStatusOnly);
            tests.Add(
                "GroupStopWait.Split.ConvenienceResumeInheritsStableCount",
                SplitConvenienceResumeInheritsStableCount);
            tests.Add(
                "GroupStopWait.Split.AcceptedPublicationDeadlinePreservesContinuation",
                SplitAcceptedPublicationDeadlinePreservesContinuation);
            tests.Add(
                "GroupStopWait.Split.TimeoutResumeDoesNotReplay",
                SplitTimeoutResumeDoesNotReplay);
            tests.Add(
                "GroupStopWait.Split.NewAcceptedStopSupersedesOld",
                SplitNewAcceptedStopSupersedesOldContinuation);
            tests.Add(
                "GroupStopWait.Split.ForeignAndStaleSessionAreZeroWire",
                SplitForeignAndStaleSessionResumeAreZeroWire);
            tests.Add(
                "GroupStopWait.Split.ConcurrentResumeSecondIsZeroWire",
                SplitConcurrentResumeSecondIsZeroWire);
            tests.Add(
                "GroupStopWait.Split.MutationInterferenceRemainsPending",
                SplitMutationInterferenceRemainsPending);
            tests.Add(
                "GroupStopWait.Split.NoStatusDeadlineInvalidatesTransport",
                SplitNoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "GroupStopWait.Split.PriorityPreemptsPollAndNewStopCompletes",
                SplitPriorityPreemptsPollAndNewStopCompletes);
            tests.Add(
                "GroupStopWait.Split.FailedResumeResetsEnableProof",
                SplitFailedResumeResetsPendingEnableProof);
            tests.Add(
                "GroupStopWait.Linearization.SameGroupMutationBeforeFinalDecisionStaysPending",
                SameGroupMutationBeforeFinalDecisionStaysPending);
            tests.Add(
                "GroupStopWait.Linearization.EarlyCancelAndDeadlineRetainPending",
                EarlyCancellationAndDeadlineRetainPending);
            tests.Add(
                "GroupStopWait.Linearization.FinalProofWinsLateCancelAndDeadline",
                FinalProofWinsLateCancellationAndDeadline);
            tests.Add(
                "GroupStopWait.Interference.ZeroWireAndDifferentGroupDoNotInterfere",
                ZeroWireAndDifferentGroupMutationsDoNotInterfere);
            tests.Add(
                "GroupStopWait.Split.PreCanceledResumeReturnsTypedAcceptedEvidence",
                PreCanceledResumeReturnsTypedAcceptedEvidence);
            tests.Add(
                "GroupStopWait.Compound.CancelAfterAcceptedPublicationReturnsTypedEvidence",
                CompoundCancellationAfterAcceptedPublicationReturnsTypedEvidence);
            tests.Add(
                "GroupStopWait.Deadline.TransportInvalidationUsesContinuationLock",
                TransportInvalidationUsesContinuationLock);
        }

        private static void DefaultsSendOneStopThenThreeStatuses()
        {
            var defaults = new LMCGroupStopWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group.GroupStopAndWaitForStableStandbyAsync(
                        1000,
                        0,
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.StopAccepted);
                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandbySampleCount);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void MismatchResetsProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(0),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group.GroupStopAndWaitForStableStandbyAsync(
                        1000,
                        0,
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandbySampleCount);
                AssertEx.Equal(50L, result.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 6);
            }
        }

        private static void InvalidAndCanceledAreZeroStopWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            -1,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 0,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 3
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                cancellation.Cancel();
                var canceled = AssertEx.Throws<
                    LMCGroupStopWaitCanceledException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.NotAttempted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.False(canceled.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(0, canceled.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
            }
        }

        private static void RejectedHasNoStatusAndTypedEvidence()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var exception = AssertEx.Throws<
                    LMCGroupStopRejectedException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.Rejected,
                    exception.Evidence.SubmissionOutcome);
                AssertEx.NotNull(exception.Acknowledgement);
                AssertEx.False(exception.Acknowledgement.IsSuccess);
                AssertEx.Equal(0, exception.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void TimeoutPreservesAcceptedEvidenceAndDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var exception = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.Accepted,
                    exception.Evidence.SubmissionOutcome);
                AssertEx.True(exception.Evidence.StopAccepted);
                AssertEx.True(exception.Evidence.StopAcknowledgement.IsSuccess);
                AssertEx.NotNull(exception.Evidence.LastObservedStatus);
                AssertEx.Equal(1, exception.Evidence.StatusPollCount);
                AssertEx.Equal(10L, exception.Evidence.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void NoAcknowledgementDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStop = StopStep(true);
                blockedStop.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked GroupStop response was not released.");
                blockedStop.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blockedStop))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupStopWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupStopWaitTimeoutException>(
                            () => group
                                .GroupStopAndWaitForStableStandbyAsync(
                                    1000,
                                    0,
                                    DeadlineOptions(),
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.OutcomeUncertain,
                        timeout.Evidence.SubmissionOutcome);
                    AssertEx.True(timeout.Evidence.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMC_Response>(
                        null,
                        timeout.Evidence.StopAcknowledgement);
                    AssertEx.Equal(0, timeout.Evidence.StatusPollCount);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 0);
                }
            }
        }

        private static void
            NoStatusDeadlinePreservesAcceptedEvidenceAndInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(Standby);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Group status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    StopStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupStopWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupStopWaitTimeoutException>(
                            () => group
                                .GroupStopAndWaitForStableStandbyAsync(
                                    1000,
                                    0,
                                    DeadlineOptions(),
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.Accepted,
                        timeout.Evidence.SubmissionOutcome);
                    AssertEx.True(timeout.Evidence.StopAccepted);
                    AssertEx.True(
                        timeout.Evidence.StopAcknowledgement.IsSuccess);
                    AssertEx.Equal<LMCGroupReadStatusResult>(
                        null,
                        timeout.Evidence.LastObservedStatus);
                    AssertEx.Equal(0, timeout.Evidence.StatusPollCount);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void CancellationPreservesAcceptedEvidenceAndConnection()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    StopStep(true),
                    StatusStep(0, 0, 0, 0, cancellation.Cancel),
                    StatusStep(Standby),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var exception = AssertEx.Throws<
                        LMCGroupStopWaitCanceledException>(
                        () => group.GroupStopAndWaitForStableStandbyAsync(
                                1000,
                                0,
                                LongOptions(),
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.Accepted,
                        exception.Evidence.SubmissionOutcome);
                    AssertEx.Equal(1, exception.Evidence.StatusPollCount);
                    AssertEx.NotNull(exception.Evidence.LastObservedStatus);
                    AssertEx.NotNull(exception.Continuation);
                    AssertEx.True(exception.Continuation.IsPending);

                    var reused = group.GroupReadStatusResult();
                    AssertEx.True(reused.IsSuccess);
                    AssertEx.True(reused.IsStandby);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 2);
                }
            }
        }

        private static void StatusErrorPreservesFailedStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby, 0x0010, -31, 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var exception = AssertEx.Throws<
                    LMCGroupStopStatusException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(exception.Evidence.StopAccepted);
                AssertEx.NotNull(exception.FailedStatus);
                AssertEx.True(ReferenceEquals(
                    exception.FailedStatus,
                    exception.Evidence.LastObservedStatus));
                AssertEx.False(exception.FailedStatus.IsSuccess);
                AssertEx.Equal(2, exception.Evidence.StatusPollCount);
                AssertEx.Equal(0, exception.Evidence.StableStandbySampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 2);
            }
        }

        private static void CancellationDuringStopResponseDrainsAcceptedAck()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var delayedStop = StopStep(true);
                delayedStop.ResponseDelayMilliseconds = 60;
                delayedStop.InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        LMC_Frame.LMCGroupStop(
                            GroupReference,
                            1000,
                            0),
                        request);
                    cancellation.Cancel();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    delayedStop,
                    StatusStep(Standby),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var exception = AssertEx.Throws<
                        LMCGroupStopWaitCanceledException>(
                        () => group.GroupStopAndWaitForStableStandbyAsync(
                                1000,
                                0,
                                LongOptions(),
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.Accepted,
                        exception.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        exception.Evidence.StopAcknowledgement.IsSuccess);
                    AssertEx.Equal(0, exception.Evidence.StatusPollCount);
                    AssertEx.False(
                        exception.Evidence.TransportInvalidatedAtDeadline);
                    AssertEx.NotNull(exception.Continuation);
                    AssertEx.True(exception.Continuation.IsPending);

                    var reused = group.GroupReadStatusResult();
                    AssertEx.True(reused.IsStandby);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void
            CommitWindowCancelAndDeadlineAreZeroWireAndReusable()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var canceled = AssertEx.Throws<
                    LMCGroupStopWaitCanceledException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.NotAttempted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.False(canceled.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(0L, canceled.Evidence.StopMutationGeneration);
                AssertEx.False(
                    canceled.Evidence.TransportInvalidatedAtDeadline);

                var reused = group.GroupReadStatusResult();
                AssertEx.True(reused.IsSuccess);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 40,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => Thread.Sleep(100))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.NotAttempted,
                    timeout.Evidence.SubmissionOutcome);
                AssertEx.False(timeout.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(0L, timeout.Evidence.StopMutationGeneration);
                AssertEx.False(timeout.TransportInvalidatedAtDeadline);

                var reused = group.GroupReadStatusResult();
                AssertEx.True(reused.IsSuccess);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void ResponseLossIsUncertainAndNotRetried()
        {
            var responseLoss = new FakeRpcStep(0x2085, new byte[0])
            {
                CloseAfterResponse = true
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var exception = AssertEx.Throws<
                    LMCGroupStopSubmissionException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.OutcomeUncertain,
                    exception.Evidence.SubmissionOutcome);
                AssertEx.True(exception.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    exception.Evidence.StopAcknowledgement);
                AssertEx.Equal(0, exception.Evidence.StatusPollCount);

                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void InterveningGroupMutationInvalidatesProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CommandStep(0x204A),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new InterferingWaitTime(
                    () => AssertEx.True(group.GroupPowerOn().IsSuccess));

                var exception = AssertEx.Throws<
                    LMCGroupStopInterferenceException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(exception.Evidence.StopAccepted);
                AssertEx.True(
                    exception.Evidence.InterveningMutationDetected);
                AssertEx.Equal(1, exception.Evidence.StatusPollCount);
                AssertEx.Equal(1, exception.Evidence.StableSampleCount);
                AssertEx.True(
                    exception.Evidence.ObservedMutationGeneration
                        > exception.Evidence.StopMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void QueuedMutationGenerationStartsAtWire()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            using (var mutationStarted = new ManualResetEventSlim(false))
            {
                var blockingStatus = StatusStep(
                    Standby,
                    0,
                    0,
                    0,
                    () =>
                    {
                        statusReceived.Set();
                        AssertEx.True(
                            releaseStatus.Wait(5000),
                            "The blocking status request was not released.");
                    });

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blockingStatus,
                    CommandStep(0x204A),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    try
                    {
                        var group = ConnectAndCreateGroup(
                            connection,
                            server.Port);
                        var coordinator =
                            connection.GetGroupEnableWaitCoordinator(
                                group.SessionGeneration,
                                group.GroupReference);
                        var beforeMutation =
                            coordinator.MutationGeneration;

                        var status = group.GroupReadStatusResultAsync(
                            CancellationToken.None);
                        AssertEx.True(
                            statusReceived.Wait(2000),
                            "The status request did not reach the wire.");

                        var mutation = Task.Run(
                            () =>
                            {
                                mutationStarted.Set();
                                return group.GroupPowerOn();
                            });
                        AssertEx.True(
                            mutationStarted.Wait(2000),
                            "The queued mutation did not start.");
                        AssertEx.False(
                            SpinWait.SpinUntil(
                                () => coordinator.MutationGeneration
                                    != beforeMutation,
                                250),
                            "A queued mutation changed generation before its write boundary.");

                        releaseStatus.Set();
                        AssertEx.True(
                            status.GetAwaiter().GetResult().IsStandby);
                        AssertEx.True(
                            mutation.GetAwaiter().GetResult().IsSuccess);
                        AssertEx.Equal(
                            beforeMutation + 1,
                            coordinator.MutationGeneration);

                        connection.CloseConnection();
                        server.Verify();
                        AssertCommandCounts(server, 0, 1);
                    }
                    finally
                    {
                        releaseStatus.Set();
                    }
                }
            }
        }

        private static void ResponseLossResetsPendingEnableProof()
        {
            var responseLoss = new FakeRpcStep(0x2085, new byte[0])
            {
                CloseAfterResponse = true
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = SeedPendingEnableProof(connection, group, 2);
                AssertEx.Equal(2, continuation.StableSampleCount);

                var exception = AssertEx.Throws<
                    LMCGroupStopSubmissionException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.OutcomeUncertain,
                    exception.Evidence.SubmissionOutcome);
                AssertEx.Equal(0, continuation.StableSampleCount);
                AssertEx.Equal(2, continuation.PollCount);
                AssertEx.True(continuation.IsPending);

                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void CloseDuringFinalStatusRejectsStaleProof()
        {
            using (var finalStatusRequested = new ManualResetEventSlim(false))
            {
                var finalStatus = StatusStep(
                    Standby,
                    0,
                    0,
                    0,
                    finalStatusRequested.Set);
                finalStatus.ResponseDelayMilliseconds = 250;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    StopStep(true),
                    StatusStep(Standby),
                    StatusStep(Standby),
                    finalStatus,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var operation = group
                        .GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            LongOptions(),
                            CancellationToken.None);

                    AssertEx.True(finalStatusRequested.Wait(2000));
                    var close = connection.CloseConnectionAsync(
                        CancellationToken.None);
                    AssertEx.True(SpinWait.SpinUntil(
                        () => connection.State == LMCConnectionState.Closing,
                        2000));

                    var exception = AssertEx.Throws<
                        LMCGroupStopStatusException>(
                        () => operation.GetAwaiter().GetResult());
                    close.GetAwaiter().GetResult();

                    AssertEx.True(exception.Evidence.StopAccepted);
                    AssertEx.Equal(2, exception.Evidence.StatusPollCount);
                    AssertEx.Equal(2, exception.Evidence.StableSampleCount);
                    AssertEx.NotNull(exception.InnerException);
                    AssertEx.Equal(
                        LMCConnectionState.Disconnected,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 3);
                }
            }
        }

        private static void SplitBeginThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        LongOptions(),
                        CancellationToken.None,
                        beginTime.ElapsedMilliseconds,
                        beginTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.Acknowledgement.IsSuccess);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var resumeTime = new FakeWaitTime();
                var result = group
                    .ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCGroupStopWaitContinuation>(
                    null,
                    group.PendingGroupStopWaitContinuation);
                AssertEx.Equal(3, result.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void SplitConvenienceResumeInheritsStableCount()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        new LMCGroupStopWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 5
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(5, continuation.RequiredStableSampleCount);
                AssertCommandCounts(server, 1, 0);

                var result = group
                    .ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.Equal(5, result.RequiredStableSampleCount);
                AssertEx.Equal(5, result.StableStandbySampleCount);
                AssertEx.Equal(5, result.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 5);
            }
        }

        private static void
            SplitAcceptedPublicationDeadlinePreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.BeginGroupStopWaitForStableStandbyAsync(
                            1000,
                            0,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            beginTime.ElapsedMilliseconds,
                            beginTime.DelayAsync,
                            () => beginTime.DelayAsync(
                                    10,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());
                var continuation = timeout.Continuation;

                AssertEx.NotNull(continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.True(timeout.Evidence.StopAccepted);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var resumeTime = new FakeWaitTime();
                var result = group
                    .ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void SplitTimeoutResumeDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.Equal(1, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(1, timeout.Evidence.StableSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StableStandbySampleCount);
                AssertCommandCounts(server, 1, 1);

                var resumeTime = new FakeWaitTime();
                var result = group
                    .ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandbySampleCount);
                AssertEx.False(continuation.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void
            SplitNewAcceptedStopSupersedesOldContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var first = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var second = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(first.IsSuperseded);
                AssertEx.False(first.IsPending);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(
                    second,
                    group.PendingGroupStopWaitContinuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.Cancel();
                    var canceled = AssertEx.Throws<
                        LMCGroupStopWaitCanceledException>(
                        () => group
                            .BeginGroupStopWaitForStableStandbyAsync(
                                1000,
                                0,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal<LMCGroupStopWaitContinuation>(
                        null,
                        canceled.Continuation);
                    AssertEx.Equal(
                        second,
                        group.PendingGroupStopWaitContinuation);
                }

                AssertCommandCounts(server, 2, 0);

                var result = group
                    .ResumeGroupStopWaitForStableStandbyAsync(
                        second,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(second, result.Continuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 3);
            }
        }

        private static void SplitForeignAndStaleSessionResumeAreZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true)))
            using (var foreignServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                CloseStep()))
            using (var staleServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                CloseStep()))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                var owner = ConnectAndCreateGroup(
                    ownerConnection,
                    firstServer.Port);
                var continuation = owner
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var foreign = ConnectAndCreateGroup(
                    foreignConnection,
                    foreignServer.Port);
                AssertEx.Throws<InvalidOperationException>(
                    () => foreign.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertCommandCounts(foreignServer, 0, 0);
                foreignConnection.CloseConnection();

                ownerConnection.RpcInitConnection(
                    "127.0.0.1",
                    staleServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var reopened = new LMCGroup(
                    ownerConnection,
                    GroupName);
                AssertEx.Throws<InvalidOperationException>(
                    () => reopened.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertCommandCounts(staleServer, 0, 0);

                ownerConnection.CloseConnection();
                firstServer.Verify();
                foreignServer.Verify();
                staleServer.Verify();
                AssertCommandCounts(firstServer, 1, 0);
            }
        }

        private static void SplitConcurrentResumeSecondIsZeroWire()
        {
            using (var firstStatusReceived =
                new ManualResetEventSlim(false))
            using (var releaseFirstStatus =
                new ManualResetEventSlim(false))
            using (var secondResumeStarted =
                new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                DelayedStatusStep(
                    firstStatusReceived,
                    releaseFirstStatus),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupStopWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var firstResume = Task.Run(
                        () => group
                            .ResumeGroupStopWaitForStableStandbyAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstStatusReceived.Wait(2000),
                        "The first GroupStop resume did not poll status.");

                    var secondResume = Task.Run(
                        () =>
                        {
                            secondResumeStarted.Set();
                            return group
                                .ResumeGroupStopWaitForStableStandbyAsync(
                                    continuation,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        });
                    AssertEx.True(
                        secondResumeStarted.Wait(2000),
                        "The second GroupStop resume did not start.");
                    AssertEx.True(
                        SpinWait.SpinUntil(
                            () => secondResume.IsCompleted,
                            2000),
                        "The concurrent GroupStop resume did not fail while the first resume remained active.");
                    AssertEx.Throws<InvalidOperationException>(
                        () => secondResume.GetAwaiter().GetResult());
                    releaseFirstStatus.Set();

                    var result = firstResume.GetAwaiter().GetResult();
                    AssertEx.Equal(continuation, result.Continuation);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 3);
                }
                finally
                {
                    releaseFirstStatus.Set();
                }
            }
        }

        private static void SplitMutationInterferenceRemainsPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                CommandStep(0x204A),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(group.GroupPowerOn().IsSuccess);

                var interference = AssertEx.Throws<
                    LMCGroupStopInterferenceException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, interference.Continuation);
                AssertEx.True(
                    interference.Evidence.InterveningMutationDetected);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x204A));
            }
        }

        private static void SplitNoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(Standby);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked split Stop status was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    StopStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupStopWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    LMCGroupStopWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupStopWaitTimeoutException>(
                            () => group
                                .ResumeGroupStopWaitForStableStandbyAsync(
                                    continuation,
                                    DeadlineOptions(),
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(continuation, timeout.Continuation);
                    AssertEx.True(timeout.Evidence.StopAccepted);
                    AssertEx.Equal(0, timeout.Evidence.StatusPollCount);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void
            SplitPriorityPreemptsPollAndNewStopCompletes()
        {
            const string firstMonitorOperation =
                "First GroupStop verification";
            var coordinator = new LMCSendPriorityCoordinator();
            var connectionOptions = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                DelayedStatusStep(statusReceived, releaseStatus),
                StopStep(true),
                StatusStep(Standby),
                StatusStep(Standby),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection(connectionOptions))
            {
                try
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var firstGeneration = coordinator.ReservePrioritySend();
                    LMCGroupStopWaitContinuation first;
                    using (coordinator.BeginPriorityScope(
                        firstGeneration,
                        "First GroupStop"))
                    {
                        first = group
                            .BeginGroupStopWaitForStableStandbyAsync(
                                1000,
                                0,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }

                    var firstMonitor = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                firstGeneration,
                                firstMonitorOperation))
                            {
                                return group
                                    .ResumeGroupStopWaitForStableStandbyAsync(
                                        first,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The first split GroupStop poll did not reach the server.");

                    var secondGeneration = coordinator.ReservePrioritySend();
                    releaseStatus.Set();

                    var preempted = AssertEx.Throws<
                        LMCGroupStopStatusException>(
                        () => firstMonitor.GetAwaiter().GetResult());
                    AssertEx.True(
                        preempted.InnerException
                            is LMCSendPreemptedException);
                    AssertEx.Equal(first, preempted.Continuation);
                    AssertEx.True(first.IsPending);
                    AssertCommandCounts(server, 1, 1);

                    LMCGroupStopWaitContinuation second;
                    using (coordinator.BeginPriorityScope(
                        secondGeneration,
                        "Second GroupStop"))
                    {
                        second = group
                            .BeginGroupStopWaitForStableStandbyAsync(
                                1000,
                                0,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }
                    AssertEx.True(first.IsSuperseded);
                    AssertEx.True(second.IsPending);

                    LMCGroupStopWaitResult result;
                    using (coordinator.BeginPreemptibleScope(
                        secondGeneration,
                        "Second GroupStop verification"))
                    {
                        result = group
                            .ResumeGroupStopWaitForStableStandbyAsync(
                                second,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }

                    AssertEx.Equal(second, result.Continuation);
                    AssertEx.False(second.IsPending);
                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 2, 4);
                }
                finally
                {
                    releaseStatus.Set();
                }
            }
        }

        private static void SplitFailedResumeResetsPendingEnableProof()
        {
            const uint poweredLockedStandby = 0x00060000u;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(poweredLockedStandby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var stop = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var enable = SeedPendingEnableProof(connection, group, 2);
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            stop,
                            new LMCGroupStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(stop, timeout.Continuation);
                AssertEx.Equal(1, timeout.Evidence.StableSampleCount);
                AssertEx.Equal(0, stop.StableStandbySampleCount);
                AssertEx.Equal(0, enable.StableSampleCount);
                AssertEx.Equal(3, enable.PollCount);
                AssertEx.True(enable.IsPending);
                AssertEx.True(stop.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            SameGroupMutationBeforeFinalDecisionStaysPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CommandStep(0x204A),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var stopGeneration = continuation.StopMutationGeneration;
                var time = new FakeWaitTime();

                var interference = AssertEx.Throws<
                    LMCGroupStopInterferenceException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            () => AssertEx.True(
                                group.GroupPowerOn().IsSuccess))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, interference.Continuation);
                AssertEx.Equal(
                    stopGeneration,
                    interference.Evidence.StopMutationGeneration);
                AssertEx.Equal(
                    stopGeneration + 1,
                    interference.Evidence.ObservedMutationGeneration);
                AssertEx.True(
                    interference.Evidence.InterveningMutationDetected);
                AssertEx.Equal(0, interference.Evidence.StatusPollCount);
                AssertEx.Equal(0, continuation.StableStandbySampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x204A));
            }
        }

        private static void EarlyCancellationAndDeadlineRetainPending()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();

                var canceled = AssertEx.Throws<
                    LMCGroupStopWaitCanceledException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            options,
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.Equal(1, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(1, canceled.Evidence.StableStandbySampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.Equal(0, continuation.StableStandbySampleCount);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                var timeout = AssertEx.Throws<
                    LMCGroupStopWaitTimeoutException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.Advance(1000))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.Equal(1, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(1, timeout.Evidence.StableStandbySampleCount);
                AssertEx.Equal(1000L, timeout.Evidence.ElapsedMilliseconds);
                AssertEx.False(timeout.TransportInvalidatedAtDeadline);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.Equal(0, continuation.StableStandbySampleCount);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void FinalProofWinsLateCancellationAndDeadline()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();

                var result = group.ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        options,
                        cancellation.Token,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        cancellation.Cancel)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(cancellation.IsCancellationRequested);
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.True(continuation.IsCompleted);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCGroupStopWaitContinuation>(
                    null,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                var result = group.ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () => time.Advance(1000))
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1000L, result.ElapsedMilliseconds);
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.True(continuation.IsCompleted);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCGroupStopWaitContinuation>(
                    null,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            ZeroWireAndDifferentGroupMutationsDoNotInterfere()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection.GetGroupEnableWaitCoordinator(
                    group.SessionGeneration,
                    group.GroupReference);
                var stopGeneration = continuation.StopMutationGeneration;
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => group.GroupPowerOnAsync(cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(stopGeneration, coordinator.MutationGeneration);

                var result = group.ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(continuation.IsCompleted);
                AssertEx.False(result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    stopGeneration,
                    result.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(0, CountCommand(server, 0x204A));
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                LookupStep(SecondGroupReference),
                StopStep(true),
                CommandStep(0x204A),
                StatusStep(Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var other = new LMCGroup(connection, SecondGroupName);
                var options = SingleSampleOptions(1000);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection.GetGroupEnableWaitCoordinator(
                    group.SessionGeneration,
                    group.GroupReference);
                var stopGeneration = continuation.StopMutationGeneration;

                AssertEx.True(other.GroupPowerOn().IsSuccess);
                AssertEx.Equal(stopGeneration, coordinator.MutationGeneration);

                var result = group.ResumeGroupStopWaitForStableStandbyAsync(
                        continuation,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(continuation.IsCompleted);
                AssertEx.False(result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    stopGeneration,
                    result.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x204A));
            }
        }

        private static void PreCanceledResumeReturnsTypedAcceptedEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                cancellation.Cancel();

                var canceled = AssertEx.Throws<
                    LMCGroupStopWaitCanceledException>(
                    () => group.ResumeGroupStopWaitForStableStandbyAsync(
                            continuation,
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.True(canceled.Evidence.StopAccepted);
                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.Accepted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.Equal(0, canceled.Evidence.StatusPollCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.Equal(
                    continuation,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void
            CompoundCancellationAfterAcceptedPublicationReturnsTypedEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();

                var canceled = AssertEx.Throws<
                    LMCGroupStopWaitCanceledException>(
                    () => group.GroupStopAndWaitForStableStandbyAsync(
                            1000,
                            0,
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(cancellation.IsCancellationRequested);
                AssertEx.NotNull(canceled.Continuation);
                AssertEx.True(canceled.Evidence.StopAccepted);
                AssertEx.Equal(
                    LMCGroupStopSubmissionOutcome.Accepted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.Equal(0, canceled.Evidence.StatusPollCount);
                AssertEx.True(canceled.Continuation.IsPending);
                AssertEx.False(canceled.Continuation.IsCompleted);
                AssertEx.Equal(
                    canceled.Continuation,
                    group.PendingGroupStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void TransportInvalidationUsesContinuationLock()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            using (var beforePublication = new ManualResetEventSlim(false))
            using (var releasePublication = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(Standby);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    StopStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupStopWaitForStableStandbyAsync(
                            1000,
                            0,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var coordinator = connection
                        .GetGroupEnableWaitCoordinator(
                            group.SessionGeneration,
                            group.GroupReference);
                    var stopwatch = Stopwatch.StartNew();
                    var time = new FakeWaitTime();
                    Task<LMCGroupStopWaitResult> resume = null;
                    LMCGroupStopWaitTimeoutException timeout = null;

                    try
                    {
                        resume = Task.Run(
                            () => group
                                .ResumeGroupStopWaitForStableStandbyAsync(
                                    continuation,
                                    DeadlineOptions(),
                                    CancellationToken.None,
                                    () => stopwatch.ElapsedMilliseconds,
                                    time.DelayAsync,
                                    null,
                                    null,
                                    null,
                                    () =>
                                    {
                                        beforePublication.Set();
                                        AssertEx.True(
                                            releasePublication.Wait(5000),
                                            "Transport invalidation publication was not released.");
                                    })
                                .GetAwaiter()
                                .GetResult());

                        AssertEx.True(
                            beforePublication.Wait(5000),
                            "Transport invalidation did not reach its publication boundary.");
                        lock (coordinator.Sync)
                        {
                            AssertEx.False(
                                continuation.CaptureEvidence(
                                        stopwatch.ElapsedMilliseconds)
                                    .TransportInvalidatedAtDeadline);
                            releasePublication.Set();
                            AssertEx.False(
                                SpinWait.SpinUntil(
                                    () => resume.IsCompleted,
                                    250),
                                "Transport invalidation bypassed the continuation coordinator lock.");
                            AssertEx.False(
                                continuation.CaptureEvidence(
                                        stopwatch.ElapsedMilliseconds)
                                    .TransportInvalidatedAtDeadline);
                        }

                        timeout = AssertEx.Throws<
                            LMCGroupStopWaitTimeoutException>(
                            () => resume.GetAwaiter().GetResult());
                    }
                    finally
                    {
                        releasePublication.Set();
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(continuation, timeout.Continuation);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.True(
                        continuation.CaptureEvidence(
                                stopwatch.ElapsedMilliseconds)
                            .TransportInvalidatedAtDeadline);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
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

        private static LMCGroupEnableWaitContinuation SeedPendingEnableProof(
            LMCConnection connection,
            LMCGroupAxis group,
            int stableSampleCount)
        {
            var coordinator = connection.GetGroupEnableWaitCoordinator(
                group.SessionGeneration,
                group.GroupReference);
            var acknowledgement = LMCConnection.ParseCommandAcknowledgement(
                TestFrame.Response(0, new byte[8]),
                "test GroupEnable");
            var continuation = new LMCGroupEnableWaitContinuation(
                coordinator,
                group.GroupName,
                group.GroupReference,
                group.SessionGeneration,
                acknowledgement,
                3);
            lock (coordinator.Sync)
            {
                coordinator.PendingContinuation = continuation;
            }

            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x00060000u);
            var status = LMCConnection.ParseGroupReadStatusResult(
                TestFrame.Response(0, payload));
            for (var index = 0; index < stableSampleCount; index++)
            {
                continuation.Observe(status);
            }

            return continuation;
        }

        private static LMCGroupStopWaitOptions LongOptions()
        {
            return new LMCGroupStopWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupStopWaitOptions DeadlineOptions()
        {
            return new LMCGroupStopWaitOptions
            {
                TimeoutMilliseconds = 200,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupStopWaitOptions SingleSampleOptions(
            int timeoutMilliseconds)
        {
            return new LMCGroupStopWaitOptions
            {
                TimeoutMilliseconds = timeoutMilliseconds,
                PollIntervalMilliseconds = 1,
                StableSampleCount = 1
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

        private static FakeRpcStep LookupStep(
            ushort groupReference = GroupReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, groupReference);
            return new FakeRpcStep(0x1042, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep StopStep(bool success)
        {
            return new FakeRpcStep(
                0x2085,
                TestFrame.Response(
                    success ? (ushort)0 : (ushort)1,
                    new byte[8]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupStop(GroupReference, 1000, 0),
                    request)
            };
        }

        private static FakeRpcStep CommandStep(ushort command)
        {
            return new FakeRpcStep(
                command,
                TestFrame.Response(0, new byte[8]));
        }

        private static FakeRpcStep DelayedStatusStep(
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus)
        {
            var step = StatusStep(Standby);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupReadStatus(GroupReference),
                    request);
                statusReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed group status response was not released.");
            return step;
        }

        private static FakeRpcStep StatusStep(
            uint state,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort groupErrorId = 0,
            Action afterRequest = null)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, groupErrorId);
            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    if (afterRequest != null)
                    {
                        afterRequest();
                    }
                }
            };
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static void AssertCommandCounts(
            FakeRpcServer server,
            int expectedStopCount,
            int expectedStatusCount)
        {
            var stopCount = 0;
            var statusCount = 0;
            foreach (var request in server.ReceivedRequests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                if (command == 0x2085)
                {
                    stopCount++;
                }
                else if (command == 0x2045)
                {
                    statusCount++;
                }
            }

            AssertEx.Equal(expectedStopCount, stopCount);
            AssertEx.Equal(expectedStatusCount, statusCount);
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

            internal void Advance(int milliseconds)
            {
                elapsedMilliseconds += milliseconds;
            }
        }

        private sealed class InterferingWaitTime
        {
            private readonly Action interfere;
            private long elapsedMilliseconds;
            private bool interfered;

            internal InterferingWaitTime(Action interfere)
            {
                this.interfere = interfere
                    ?? throw new ArgumentNullException("interfere");
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
                if (!interfered)
                {
                    interfered = true;
                    interfere();
                }

                return Task.CompletedTask;
            }
        }
    }
}

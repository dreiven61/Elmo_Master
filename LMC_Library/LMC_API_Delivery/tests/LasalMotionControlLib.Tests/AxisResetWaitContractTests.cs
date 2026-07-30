using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AxisResetWaitContractTests
    {
        private const string AxisName = "_LMCAxis1";
        private const ushort AxisReference = 1;
        private const string SecondAxisName = "_LMCAxis2";
        private const ushort SecondAxisReference = 2;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "AxisResetWait.Success.OneResetThenThreeClearStatuses",
                SuccessSendsExactlyOneResetThenThreeClearStatuses);
            tests.Add(
                "AxisResetWait.Rejected.ZeroStatusPolls",
                RejectedAcknowledgementHasZeroStatusPolls);
            tests.Add(
                "AxisResetWait.Timeout.PreservesAcceptedEvidence",
                TimeoutPreservesAcknowledgementStatusAndPollCount);
            tests.Add(
                "AxisResetWait.Timeout.NoAckInvalidatesTransport",
                NoAcknowledgementDeadlineInvalidatesTransport);
            tests.Add(
                "AxisResetWait.Timeout.NoStatusInvalidatesTransport",
                NoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "AxisResetWait.Cancel.PreservesAcceptedEvidence",
                CancellationPreservesAcknowledgementStatusAndPollCount);
            tests.Add(
                "AxisResetWait.AxisError.ResetsStability",
                AxisErrorResetsStabilityProof);
            tests.Add(
                "AxisResetWait.ReadFailure.ThrowsTypedStatus",
                UnsuccessfulReadThrowsTypedStatusException);
            tests.Add(
                "AxisResetWait.PreWireCommitCancel.IsNotAttempted",
                CommitWindowCancellationPreservesNotAttemptedEvidence);
            tests.Add(
                "AxisResetWait.Submission.ResponseLossIsUncertain",
                ResponseLossIsUncertainAndNotRetried);
            tests.Add(
                "AxisResetWait.SendPriority.ResultDiscardIsUncertain",
                SendPriorityResultDiscardIsUncertain);
            tests.Add(
                "AxisResetWait.Split.BeginThenResumeIsStatusOnly",
                SplitBeginThenResumeIsStatusOnly);
            tests.Add(
                "AxisResetWait.Split.AcceptedPublicationDeadlinePreservesContinuation",
                SplitAcceptedPublicationDeadlinePreservesContinuation);
            tests.Add(
                "AxisResetWait.Split.TimeoutResumeDoesNotReplayAndResetsEpoch",
                SplitTimeoutResumeDoesNotReplayAndResetsEpoch);
            tests.Add(
                "AxisResetWait.Split.CanceledResumeDoesNotReplay",
                SplitCanceledResumeDoesNotReplay);
            tests.Add(
                "AxisResetWait.Split.StatusFailureDoesNotReplay",
                SplitStatusFailureDoesNotReplay);
            tests.Add(
                "AxisResetWait.Split.NewAcceptedResetSupersedesOld",
                SplitNewAcceptedResetSupersedesOld);
            tests.Add(
                "AxisResetWait.Split.StaleSessionResumeIsZeroWire",
                SplitStaleSessionResumeIsZeroWire);
            tests.Add(
                "AxisResetWait.Session.CloseBeforeStatusPublicationRejectsProof",
                CloseBeforeStatusPublicationRejectsProof);
            tests.Add(
                "AxisResetWait.Interference.SameAxisStopIsZeroStatus",
                SameAxisStopInterferesBeforeResume);
            tests.Add(
                "AxisResetWait.Interference.StatusRaceDiscardsResult",
                MutationRacingStatusDiscardsResult);
            tests.Add(
                "AxisResetWait.Interference.ZeroWireMutationDoesNotAdvance",
                ZeroWireMutationDoesNotAdvanceGeneration);
            tests.Add(
                "AxisResetWait.Interference.DifferentAxisDoesNotAdvance",
                DifferentAxisMutationDoesNotInterfere);
            tests.Add(
                "AxisResetWait.Split.InvalidContinuationMatrixIsZeroWire",
                InvalidContinuationMatrixIsZeroWire);
            tests.Add(
                "AxisResetWait.Split.ConcurrentFailureSecondIsZeroWire",
                ConcurrentFailureSecondResumeIsZeroWire);
            tests.Add(
                "AxisResetWait.Deadline.MutationAndStatusGatesAreHard",
                MutationAndStatusGateDeadlinesAreHard);
            tests.Add(
                "AxisResetWait.Deadline.CompoundSharesOneDeadline",
                CompoundSharesOneTotalDeadline);
            tests.Add(
                "AxisResetWait.Submission.RejectedPreservesPriorPending",
                RejectedBeginPreservesPriorPending);
            tests.Add(
                "AxisResetWait.Submission.ResponseLossPreservesPriorButInvalidatesGeneration",
                ResponseLossPreservesPriorButInvalidatesGeneration);
            tests.Add(
                "AxisResetWait.Session.AcceptedAckCloseDoesNotPublishStale",
                AcceptedAcknowledgementCloseDoesNotPublishStale);
            tests.Add(
                "AxisResetWait.Race.FinalProofWinsLateCancelAndDeadline",
                FinalProofWinsLateCancellationAndDeadline);
            tests.Add(
                "AxisResetWait.Race.FinalProofDefersEarlyCancelAndDeadline",
                FinalProofDefersEarlyCancellationAndDeadline);
            tests.Add(
                "AxisResetWait.Race.NonfinalSupersedeCannotComplete",
                NonfinalSupersedeCannotCompleteOldWait);
            tests.Add(
                "AxisResetWait.Observer.AtomicBeforeStatusAndBlocksReentry",
                AcceptedObserverIsAtomicBeforeStatusAndBlocksReentry);
            tests.Add(
                "AxisResetWait.Observer.FailureLeavesRecoverablePending",
                AcceptedObserverFailureLeavesRecoverablePending);
            tests.Add(
                "AxisResetWait.StatusOnly.CrossSessionZeroReplay",
                StatusOnlyCrossSessionSendsOnlyStatus);
            tests.Add(
                "AxisResetWait.StatusOnly.CloseBeforePublicationRejectsProof",
                StatusOnlyCloseBeforePublicationRejectsProof);
            tests.Add(
                "AxisResetWait.StatusOnly.FinalProofRaceIsLinearized",
                StatusOnlyFinalProofRaceIsLinearized);
            tests.Add(
                "AxisResetWait.StatusOnly.UnstableSampleResetsCounter",
                StatusOnlyUnstableSampleResetsCounter);
            tests.Add(
                "AxisResetWait.StatusOnly.TimeoutPreservesEvidence",
                StatusOnlyTimeoutPreservesEvidence);
            tests.Add(
                "AxisResetWait.StatusOnly.PreCanceledIsZeroWire",
                StatusOnlyPreCanceledIsZeroWire);
            tests.Add(
                "AxisResetWait.StatusOnly.ReadFailureIsTyped",
                StatusOnlyReadFailureIsTyped);
            tests.Add(
                "AxisResetWait.StatusOnly.PostWriteDeadlineInvalidatesTransport",
                StatusOnlyPostWriteDeadlineInvalidatesTransport);
            tests.Add(
                "AxisResetWait.StatusOnly.ProcessLocalMutationRaceIsInconclusive",
                StatusOnlyProcessLocalMutationRaceIsInconclusive);
            tests.Add(
                "AxisResetWait.PendingStop.NewAndRawResetAreZeroWire",
                PendingStopBlocksNewAndRawReset);
        }

        private static void
            SuccessSendsExactlyOneResetThenThreeClearStatuses()
        {
            var defaults = new LMCAxisResetWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis
                    .ResetAndWaitForStableErrorClearanceAsync(
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.ResetAccepted);
                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.NotNull(result.FinalStatus);
                AssertEx.True(result.FinalStatus.IsReadSuccessful);
                AssertEx.False(result.FinalStatus.HasAxisError);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableErrorClearSampleCount);
                AssertEx.Equal(3, result.RequiredStableSampleCount);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);
                AssertEx.False(result.TransportInvalidatedAtDeadline);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void RejectedAcknowledgementHasZeroStatusPolls()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<LMCAxisResetRejectedException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.Rejected,
                    error.Evidence.SubmissionOutcome);
                AssertEx.NotNull(error.Acknowledgement);
                AssertEx.False(error.Acknowledgement.IsSuccess);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void
            TimeoutPreservesAcknowledgementStatusAndPollCount()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(axisErrorId: 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisResetWaitTimeoutException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
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
                    LMCAxisResetSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.ResetAccepted);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.HasAxisError);
                AssertEx.Equal(1, error.StatusPollCount);
                AssertEx.Equal(0, error.Evidence.StableErrorClearSampleCount);
                AssertEx.False(
                    error.TransportInvalidatedAtDeadline);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            CancellationPreservesAcknowledgementStatusAndPollCount()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(afterRequest: cancellation.Cancel),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisResetWaitCanceledException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.IsReadSuccessful);
                AssertEx.Equal(1, error.StatusPollCount);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsReadSuccessful);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 2);
            }
        }

        private static void NoAcknowledgementDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedReset = ResetStep(true);
                blockedReset.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Reset response was not released.");
                blockedReset.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedReset))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisResetWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisResetWaitTimeoutException>(
                            () => axis
                                .ResetAndWaitForStableErrorClearanceAsync(
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
                        LMCAxisResetSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMC_Response>(
                        null,
                        error.Acknowledgement);
                    AssertEx.Equal(0, error.StatusPollCount);
                    AssertEx.True(
                        error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    AssertEx.True(
                        error.Message.IndexOf(
                            "must be reconnected",
                            StringComparison.Ordinal) >= 0);

                    server.Verify();
                    AssertCommandCounts(server, 1, 0);
                }
            }
        }

        private static void NoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep();
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    ResetStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisResetWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisResetWaitTimeoutException>(
                            () => axis
                                .ResetAndWaitForStableErrorClearanceAsync(
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
                        LMCAxisResetSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.ResetAccepted);
                    AssertEx.True(error.Acknowledgement.IsSuccess);
                    AssertEx.Equal<LMCReadStatusResult>(
                        null,
                        error.LastObservedStatus);
                    AssertEx.Equal(0, error.StatusPollCount);
                    AssertEx.True(
                        error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void AxisErrorResetsStabilityProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(axisErrorId: 7),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis
                    .ResetAndWaitForStableErrorClearanceAsync(
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(3, result.StableErrorClearSampleCount);
                AssertEx.False(result.FinalStatus.HasAxisError);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 6);
            }
        }

        private static void UnsuccessfulReadThrowsTypedStatusException()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(
                    functionStatus: 0x0010,
                    errorId: -31,
                    axisErrorId: 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<LMCAxisResetStatusException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(error.Evidence.ResetAccepted);
                AssertEx.NotNull(error.FailedStatus);
                AssertEx.True(ReferenceEquals(
                    error.FailedStatus,
                    error.Evidence.LastObservedStatus));
                AssertEx.False(error.FailedStatus.IsReadSuccessful);
                AssertEx.Equal(2, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    0,
                    error.Evidence.StableErrorClearSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 2);
            }
        }

        private static void
            CommitWindowCancellationPreservesNotAttemptedEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<LMCAxisResetWaitCanceledException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.False(
                    error.Evidence.TransportInvalidatedAtDeadline);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.ResetAcknowledgement);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsReadSuccessful);
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
            var responseLoss = new FakeRpcStep(0x2024, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReset(AxisReference),
                    request)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisResetSubmissionException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.OutcomeUncertain,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.ResetAcknowledgement);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void SendPriorityResultDiscardIsUncertain()
        {
            const string resetOperation = "Axis Reset completion";
            const string stopOperation = "Priority Axis Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var resetReceived = new ManualResetEventSlim(false))
            using (var releaseReset = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                DelayedResetStep(resetReceived, releaseReset),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var prior = axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var priorGeneration =
                        prior.ResetMutationGeneration;
                    var expectedGeneration =
                        coordinator.CurrentGeneration;
                    var reset = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                resetOperation))
                            {
                                return axis
                                    .ResetAndWaitForStableErrorClearanceAsync(
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        resetReceived.Wait(2000),
                        "The Reset request did not reach the server.");

                    var reservedGeneration =
                        coordinator.ReservePrioritySend();
                    var stop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                stopOperation))
                            {
                                return axis.Stop(1000, 0);
                            }
                        });

                    releaseReset.Set();
                    var error = AssertEx.Throws<
                        LMCAxisResetSubmissionException>(
                        () => reset.GetAwaiter().GetResult());
                    var pending = AssertEx.Throws<
                        LMCAxisResetWaitPendingException>(
                        () => stop.GetAwaiter().GetResult());

                    AssertEx.Equal(
                        LMCAxisResetSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.Equal(prior, pending.Continuation);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);
                    AssertEx.True(prior.IsPending);
                    AssertEx.Equal(
                        prior,
                        axis.PendingResetWaitContinuation);
                    var interference = AssertEx.Throws<
                        LMCAxisResetInterferenceException>(
                        () => axis
                            .ResumeResetWaitForStableErrorClearanceAsync(
                                prior,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertResetInterference(
                        interference,
                        prior,
                        priorGeneration,
                        priorGeneration + 1,
                        0);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 2, 0);
                }
                finally
                {
                    releaseReset.Set();
                }
            }
        }

        private static void SplitBeginThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.Acknowledgement.IsSuccess);
                AssertEx.True(continuation.ResetMutationGeneration > 0);
                AssertEx.Equal(
                    continuation.ResetMutationGeneration,
                    continuation.ObservedMutationGeneration);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void
            SplitAcceptedPublicationDeadlinePreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisResetWaitTimeoutException>(
                    () => axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
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
                AssertEx.True(timeout.Evidence.ResetAccepted);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.False(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void
            SplitTimeoutResumeDoesNotReplayAndResetsEpoch()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisResetWaitTimeoutException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            new LMCAxisResetWaitOptions
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
                AssertEx.Equal(
                    1,
                    timeout.Evidence.StableErrorClearSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StableErrorClearSampleCount);
                AssertCommandCounts(server, 1, 1);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(3, result.StableErrorClearSampleCount);
                AssertEx.False(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void SplitCanceledResumeDoesNotReplay()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(afterRequest: cancellation.Cancel),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var canceled = AssertEx.Throws<
                    LMCAxisResetWaitCanceledException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(1, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(0, continuation.StableErrorClearSampleCount);
                AssertCommandCounts(server, 1, 1);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(3, result.StableErrorClearSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void SplitStatusFailureDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(functionStatus: 0x0010, errorId: -31),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var failure = AssertEx.Throws<LMCAxisResetStatusException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, failure.Continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.NotNull(failure.FailedStatus);
                AssertEx.Equal(1, failure.Evidence.StatusPollCount);
                AssertEx.Equal(0, continuation.StableErrorClearSampleCount);
                AssertCommandCounts(server, 1, 1);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(4, result.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void SplitNewAcceptedResetSupersedesOld()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var second = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(first.IsSuperseded);
                AssertEx.False(first.IsPending);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(
                    second,
                    axis.PendingResetWaitContinuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertCommandCounts(server, 2, 0);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
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

        private static void SplitStaleSessionResumeIsZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true)))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var oldAxis = ConnectAndCreateAxis(
                    connection,
                    firstServer.Port);
                var continuation = oldAxis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var newAxis = new LMCAxis(connection, AxisName);
                AssertEx.Throws<InvalidOperationException>(
                    () => newAxis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertEx.Equal(0, CountCommand(secondServer, 0x2024));
                AssertEx.Equal(0, CountCommand(secondServer, 0x2028));
            }
        }

        private static void CloseBeforeStatusPublicationRejectsProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        new LMCAxisResetWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 1
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                var failure = AssertEx.Throws<LMCAxisResetStatusException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            new LMCAxisResetWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            connection.CloseConnection)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, failure.Continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StatusPollCount);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    failure.Evidence.LastObservedStatus);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);

                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void SameAxisStopInterferesBeforeResume()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var pending = AssertEx.Throws<
                    LMCAxisResetWaitPendingException>(
                    () => axis.Stop(1000, 0));
                AssertEx.Equal(continuation, pending.Continuation);
                AssertEx.Equal(0, continuation.StatusPollCount);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(0, CountCommand(server, 0x2022));
            }
        }

        private static void MutationRacingStatusDiscardsResult()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var resetGeneration =
                    continuation.ResetMutationGeneration;
                var time = new FakeWaitTime();
                var interference = AssertEx.Throws<
                    LMCAxisResetInterferenceException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(
                                axis.PowerOff().IsSuccess))
                        .GetAwaiter()
                        .GetResult());

                AssertResetInterference(
                    interference,
                    continuation,
                    resetGeneration,
                    resetGeneration + 1,
                    0);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    interference.Evidence.LastObservedStatus);
                AssertEx.Equal(0, continuation.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void ZeroWireMutationDoesNotAdvanceGeneration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);
                var resetGeneration =
                    continuation.ResetMutationGeneration;
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => axis.StopAsync(
                            1000,
                            0,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => axis.Stop(0, 0));
                AssertEx.Equal(
                    resetGeneration,
                    coordinator.MutationGeneration);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    resetGeneration,
                    result.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(0, CountCommand(server, 0x2022));
            }
        }

        private static void DifferentAxisMutationDoesNotInterfere()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(AxisReference),
                AxisInfoStep(AxisReference),
                LookupStep(SecondAxisReference),
                AxisInfoStep(SecondAxisReference),
                ResetStep(true),
                StopStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, SecondAxisName);
                var continuation = first
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var firstCoordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        first.SessionGeneration,
                        first.AxisReference);
                var secondCoordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        second.SessionGeneration,
                        second.AxisReference);
                var resetGeneration =
                    continuation.ResetMutationGeneration;

                AssertEx.True(second.Stop(1000, 0).IsSuccess);
                AssertEx.Equal(
                    resetGeneration,
                    firstCoordinator.MutationGeneration);
                AssertEx.Equal(1L, secondCoordinator.MutationGeneration);

                var result = first
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    resetGeneration,
                    result.Evidence.ResetMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x2022));
            }
        }

        private static void AssertResetInterference(
            LMCAxisResetInterferenceException interference,
            LMCAxisResetWaitContinuation continuation,
            long expectedGeneration,
            long observedGeneration,
            int expectedStatusPollCount)
        {
            AssertEx.Equal(continuation, interference.Continuation);
            AssertEx.True(interference.Evidence.ResetAccepted);
            AssertEx.True(
                interference.Evidence.InterveningMutationDetected);
            AssertEx.Equal(
                expectedGeneration,
                interference.ExpectedMutationGeneration);
            AssertEx.Equal(
                observedGeneration,
                interference.ObservedMutationGeneration);
            AssertEx.Equal(
                expectedStatusPollCount,
                interference.Evidence.StatusPollCount);
            AssertEx.True(continuation.IsPending);
        }

        private static void InvalidContinuationMatrixIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(AxisReference),
                AxisInfoStep(AxisReference),
                LookupStep(SecondAxisReference),
                AxisInfoStep(SecondAxisReference),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var owner = ConnectAndCreateAxis(connection, server.Port);
                var foreign = new LMCAxis(connection, SecondAxisName);

                AssertEx.Throws<InvalidOperationException>(
                    () => owner
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            null,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                var continuation = owner
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Throws<InvalidOperationException>(
                    () => foreign
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertCommandCounts(server, 1, 0);

                var result = owner
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => owner
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void ConcurrentFailureSecondResumeIsZeroWire()
        {
            using (var firstStatusReceived =
                new ManualResetEventSlim(false))
            using (var releaseFirstStatus =
                new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                DelayedStatusStep(
                    firstStatusReceived,
                    releaseFirstStatus,
                    functionStatus: 0x0010,
                    errorId: -31),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var continuation = axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var first = Task.Run(
                        () => axis
                            .ResumeResetWaitForStableErrorClearanceAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstStatusReceived.Wait(2000),
                        "The first Reset resume did not reach status read.");

                    AssertEx.Throws<InvalidOperationException>(
                        () => axis
                            .ResumeResetWaitForStableErrorClearanceAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertCommandCounts(server, 1, 1);

                    releaseFirstStatus.Set();
                    var failure = AssertEx.Throws<
                        LMCAxisResetStatusException>(
                        () => first.GetAwaiter().GetResult());
                    AssertEx.Equal(continuation, failure.Continuation);
                    AssertEx.True(continuation.IsPending);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
                finally
                {
                    releaseFirstStatus.Set();
                }
            }
        }

        private static void MutationAndStatusGateDeadlinesAreHard()
        {
            MutationGateDeadlineIsZeroResetWire();
            StatusGateDeadlineIsZeroStatusWire();
        }

        private static void MutationGateDeadlineIsZeroResetWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);
                coordinator.MutationGate.Wait();
                try
                {
                    var timeout = AssertEx.Throws<
                        LMCAxisResetWaitTimeoutException>(
                        () => axis
                            .BeginResetWaitForStableErrorClearanceAsync(
                                new LMCAxisResetWaitOptions
                                {
                                    TimeoutMilliseconds = 50,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(
                        LMCAxisResetSubmissionOutcome.NotAttempted,
                        timeout.Evidence.SubmissionOutcome);
                    AssertEx.Equal<LMCAxisResetWaitContinuation>(
                        null,
                        timeout.Continuation);
                    AssertCommandCounts(server, 0, 0);
                }
                finally
                {
                    coordinator.MutationGate.Release();
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void StatusGateDeadlineIsZeroStatusWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);
                coordinator.StatusObservationGate.Wait();
                try
                {
                    var timeout = AssertEx.Throws<
                        LMCAxisResetWaitTimeoutException>(
                        () => axis
                            .ResumeResetWaitForStableErrorClearanceAsync(
                                continuation,
                                new LMCAxisResetWaitOptions
                                {
                                    TimeoutMilliseconds = 50,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(continuation, timeout.Continuation);
                    AssertEx.True(continuation.IsPending);
                    AssertCommandCounts(server, 1, 0);
                }
                finally
                {
                    coordinator.StatusObservationGate.Release();
                }

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(3, result.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void CompoundSharesOneTotalDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisResetWaitTimeoutException>(
                    () => axis
                        .ResetAndWaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.DelayAsync(
                                    9,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(timeout.Evidence.ResetAccepted);
                AssertEx.Equal(10L, timeout.Evidence.ElapsedMilliseconds);
                AssertEx.Equal(1, timeout.Evidence.StatusPollCount);
                AssertEx.NotNull(timeout.Continuation);
                AssertEx.True(timeout.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void RejectedBeginPreservesPriorPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                ResetStep(false),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var prior = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var rejected = AssertEx.Throws<
                    LMCAxisResetRejectedException>(
                    () => axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.Rejected,
                    rejected.Evidence.SubmissionOutcome);
                AssertEx.True(prior.IsPending);
                AssertEx.False(prior.IsSuperseded);
                AssertEx.Equal(prior, axis.PendingResetWaitContinuation);

                var resumed = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        prior,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(prior, resumed.Continuation);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState.Completed,
                    prior.State);
                AssertEx.Equal(3, resumed.StatusPollCount);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 3);
            }
        }

        private static void
            ResponseLossPreservesPriorButInvalidatesGeneration()
        {
            var responseLoss = new FakeRpcStep(0x2024, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReset(AxisReference),
                    request)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var prior = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var priorGeneration = prior.ResetMutationGeneration;
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);

                var uncertain = AssertEx.Throws<
                    LMCAxisResetSubmissionException>(
                    () => axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.OutcomeUncertain,
                    uncertain.Evidence.SubmissionOutcome);
                AssertEx.True(uncertain.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    uncertain.Evidence.ResetAcknowledgement);
                AssertEx.Equal(
                    priorGeneration + 1,
                    uncertain.Evidence.ResetMutationGeneration);
                AssertEx.Equal(
                    priorGeneration + 1,
                    uncertain.Evidence.ObservedMutationGeneration);
                AssertEx.Equal(
                    priorGeneration + 1,
                    coordinator.MutationGeneration);

                AssertEx.True(prior.IsPending);
                AssertEx.False(prior.IsSuperseded);
                AssertEx.Equal(priorGeneration, prior.ResetMutationGeneration);
                AssertEx.Equal(
                    priorGeneration,
                    prior.ObservedMutationGeneration);
                AssertEx.Equal(prior, axis.PendingResetWaitContinuation);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);

                AssertEx.Throws<InvalidOperationException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            prior,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                server.Verify();
                AssertCommandCounts(server, 2, 0);
            }
        }

        private static void
            AcceptedAcknowledgementCloseDoesNotPublishStale()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var failure = AssertEx.Throws<
                    LMCAxisResetSubmissionException>(
                    () => axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            connection.CloseConnection)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisResetSubmissionOutcome.OutcomeUncertain,
                    failure.Evidence.SubmissionOutcome);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);

                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void NonfinalSupersedeCannotCompleteOldWait()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                ResetStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAxisResetWaitContinuation second = null;
                var time = new FakeWaitTime();
                var failure = AssertEx.Throws<
                    LMCAxisResetStatusException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            first,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            () => second = axis
                                .BeginResetWaitForStableErrorClearanceAsync(
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(first, failure.Continuation);
                AssertEx.True(first.IsSuperseded);
                AssertEx.NotNull(second);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(second, axis.PendingResetWaitContinuation);
                AssertEx.Equal(1, failure.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 1);
            }
        }

        private static void FinalProofDefersEarlyCancellationAndDeadline()
        {
            FinalProofDefersEarlyCancellation();
            FinalProofDefersEarlyDeadline();
            FinalProofDefersCancellationAtCoordinatorBoundary();
        }

        private static void
            FinalProofDefersCancellationAtCoordinatorBoundary()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSampleOptions(1000);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                var canceled = AssertEx.Throws<
                    LMCAxisResetWaitCanceledException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            options,
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.Equal(1, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    canceled.Evidence.StableErrorClearSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void FinalProofDefersEarlyCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(afterRequest: cancellation.Cancel),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSampleOptions(1000);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var canceled = AssertEx.Throws<
                    LMCAxisResetWaitCanceledException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            options,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.Equal(1, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    canceled.Evidence.StableErrorClearSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void FinalProofDefersEarlyDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSampleOptions(10);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                var timeout = AssertEx.Throws<
                    LMCAxisResetWaitTimeoutException>(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.DelayAsync(
                                    10,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.Equal(1, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    timeout.Evidence.StableErrorClearSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void FinalProofWinsLateCancellationAndDeadline()
        {
            FinalProofWinsLateCancellation();
            FinalProofWinsLateDeadline();
        }

        private static void FinalProofWinsLateCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSampleOptions(1000);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
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
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void FinalProofWinsLateDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSampleOptions(10);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var continuation = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () => time.DelayAsync(
                                10,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult())
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(10L, result.ElapsedMilliseconds);
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            AcceptedObserverIsAtomicBeforeStatusAndBlocksReentry()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var observerCount = 0;
                var result = axis
                    .ResetAndWaitForStableErrorClearanceAsync(
                        options,
                        continuation =>
                        {
                            observerCount++;
                            AssertEx.Equal(
                                continuation,
                                axis.PendingResetWaitContinuation);
                            AssertEx.Equal(0, CountCommand(server, 0x2028));

                            var replay = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => axis
                                    .ResumeResetWaitForStableErrorClearanceAsync(
                                        continuation,
                                        options,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult());
                            AssertEx.Equal(
                                continuation,
                                replay.ResetContinuation);

                            var mutation = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => axis.PowerOff());
                            AssertEx.Equal(
                                continuation,
                                mutation.ResetContinuation);

                            var statusOnly = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => axis
                                    .WaitForStableErrorClearanceAsync(
                                        options,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult());
                            AssertEx.Equal(
                                continuation,
                                statusOnly.ResetContinuation);
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCount);
                AssertEx.Equal(1, result.StatusPollCount);
                AssertEx.False(result.Continuation.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(0, CountCommand(server, 0x2023));
            }
        }

        private static void AcceptedObserverFailureLeavesRecoverablePending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = SingleSampleOptions(1000);
                var observerCount = 0;
                AssertEx.Throws<InvalidOperationException>(
                    () => axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            options,
                            continuation =>
                            {
                                observerCount++;
                                throw new InvalidOperationException(
                                    "durable commit failed");
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                var pending = axis.PendingResetWaitContinuation;
                AssertEx.NotNull(pending);
                AssertEx.True(pending.IsPending);
                AssertEx.Equal(1, observerCount);
                AssertCommandCounts(server, 1, 0);

                var result = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        pending,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(1, result.StatusPollCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void StatusOnlyCrossSessionSendsOnlyStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                var result = axis.WaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableErrorClearSampleCount);
                AssertEx.False(result.FinalStatus.HasAxisError);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 3);
            }
        }

        private static void StatusOnlyCloseBeforePublicationRejectsProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceStatusException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(1000),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            connection.CloseConnection)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    error.Evidence.LastObservedStatus);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyFinalProofRaceIsLinearized()
        {
            StatusOnlyFinalProofWinsLateCancellationAndDeadline();
            StatusOnlyFinalProofDefersEarlyCancellation();
            StatusOnlyFinalProofDefersEarlyDeadline();
        }

        private static void
            StatusOnlyFinalProofWinsLateCancellationAndDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.WaitForStableErrorClearanceAsync(
                        SingleSampleOptions(10),
                        cancellation.Token,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () =>
                        {
                            cancellation.Cancel();
                            time.DelayAsync(10, CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        })
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(cancellation.IsCancellationRequested);
                AssertEx.Equal(1, result.StatusPollCount);
                AssertEx.Equal(10L, result.ElapsedMilliseconds);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyFinalProofDefersEarlyCancellation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceWaitCanceledException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(1000),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableErrorClearSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyFinalProofDefersEarlyDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceWaitTimeoutException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(10),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.DelayAsync(
                                    10,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableErrorClearSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyUnstableSampleResetsCounter()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                StatusStep(axisErrorId: 1),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var result = axis.WaitForStableErrorClearanceAsync(
                        new LMCAxisResetWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 2
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(2, result.StableErrorClearSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 4);
            }
        }

        private static void StatusOnlyTimeoutPreservesEvidence()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(axisErrorId: 1),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceWaitTimeoutException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 2
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(0, CountCommand(server, 0x2024));
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyPreCanceledIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                cancellation.Cancel();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceWaitCanceledException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
            }
        }

        private static void StatusOnlyReadFailureIsTyped()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(
                    functionStatus: 0x0010,
                    errorId: -31,
                    axisErrorId: 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceStatusException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(1000),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.NotNull(error.FailedStatus);
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void
            StatusOnlyPostWriteDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep();
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Reset status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    LMCAxisStableErrorClearanceWaitTimeoutException error =
                        null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisStableErrorClearanceWaitTimeoutException>(
                            () => axis.WaitForStableErrorClearanceAsync(
                                    SingleSampleOptions(200),
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }
                    AssertEx.True(
                        error.Evidence.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);
                    AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                    server.Verify();
                    AssertCommandCounts(server, 0, 1);
                }
            }
        }

        private static void PendingStopBlocksNewAndRawReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        1000,
                        0,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    stop,
                    AssertEx.Throws<LMCAxisStopWaitPendingException>(
                        () => axis.Reset())
                        .Continuation);
                AssertEx.Equal(
                    stop,
                    AssertEx.Throws<LMCAxisStopWaitPendingException>(
                        () => axis.ResetAsync(CancellationToken.None)
                            .GetAwaiter()
                            .GetResult())
                        .Continuation);
                AssertEx.Equal(
                    stop,
                    AssertEx.Throws<LMCAxisStopWaitPendingException>(
                        () => axis
                            .BeginResetWaitForStableErrorClearanceAsync(
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult())
                        .Continuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
            }
        }

        private static void
            StatusOnlyProcessLocalMutationRaceIsInconclusive()
        {
            StatusOnlyMutationBeforeFirstWireIsInconclusive();
            StatusOnlyMutationBetweenSamplesIsInconclusive();
            StatusOnlyMutationBeforeFinalPublicationIsInconclusive();
        }

        private static void StatusOnlyMutationBeforeFirstWireIsInconclusive()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceInterferenceException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(1000),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            () => AssertEx.True(axis.PowerOff().IsSuccess))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(
                    error.Evidence
                        .InterveningProcessLocalMutationDetected);
                AssertEx.Equal(0L, error.BaselineMutationGeneration);
                AssertEx.Equal(1L, error.ObservedMutationGeneration);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void StatusOnlyMutationBetweenSamplesIsInconclusive()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var publicationCount = 0;
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceInterferenceException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 2
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            () =>
                            {
                                publicationCount++;
                                if (publicationCount == 1)
                                {
                                    AssertEx.True(axis.PowerOff().IsSuccess);
                                }
                            })
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableErrorClearSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void
            StatusOnlyMutationBeforeFinalPublicationIsInconclusive()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableErrorClearanceInterferenceException>(
                    () => axis.WaitForStableErrorClearanceAsync(
                            SingleSampleOptions(1000),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(axis.PowerOff().IsSuccess))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.True(
                    error.Evidence
                        .InterveningProcessLocalMutationDetected);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static LMCAxisResetWaitOptions SingleSampleOptions(
            int timeoutMilliseconds)
        {
            return new LMCAxisResetWaitOptions
            {
                TimeoutMilliseconds = timeoutMilliseconds,
                PollIntervalMilliseconds = Math.Min(
                    10,
                    timeoutMilliseconds),
                StableSampleCount = 1
            };
        }

        private static LMCSingleAxis ConnectAndCreateAxis(
            LMCConnection connection,
            int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
            return new LMCAxis(connection, AxisName);
        }

        private static LMCAxisResetWaitOptions LongOptions()
        {
            return new LMCAxisResetWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCAxisResetWaitOptions DeadlineOptions()
        {
            return new LMCAxisResetWaitOptions
            {
                TimeoutMilliseconds = 200,
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
            return LookupStep(AxisReference);
        }

        private static FakeRpcStep LookupStep(ushort axisReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, axisReference);
            return new FakeRpcStep(0x103C, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep()
        {
            return AxisInfoStep(AxisReference);
        }

        private static FakeRpcStep AxisInfoStep(ushort axisReference)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            return new FakeRpcStep(0x202B, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep ResetStep(bool success)
        {
            return new FakeRpcStep(
                0x2024,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReset(AxisReference),
                    request)
            };
        }

        private static FakeRpcStep DelayedResetStep(
            ManualResetEventSlim resetReceived,
            ManualResetEventSlim releaseReset)
        {
            var step = ResetStep(true);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReset(AxisReference),
                    request);
                resetReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseReset.Wait(5000),
                "The delayed Reset response was not released.");
            return step;
        }

        private static FakeRpcStep StatusStep(
            uint state = 0,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort axisErrorId = 0,
            ushort statusWord = 0,
            Action afterRequest = null)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, axisErrorId);
            TestFrame.WriteUInt16(payload, 10, statusWord);
            return new FakeRpcStep(
                0x2028,
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

        private static FakeRpcStep DelayedStatusStep(
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus,
            uint state = 0,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort axisErrorId = 0,
            ushort statusWord = 0)
        {
            var step = StatusStep(
                state,
                functionStatus,
                errorId,
                axisErrorId,
                statusWord);
            step.InspectRequest = request => statusReceived.Set();
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed Reset status response was not released.");
            return step;
        }

        private static FakeRpcStep StopStep()
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep PowerOffStep()
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    request)
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
            int expectedResetCount,
            int expectedStatusCount)
        {
            AssertEx.Equal(
                expectedResetCount,
                CountCommand(server, 0x2024));
            AssertEx.Equal(
                expectedStatusCount,
                CountCommand(server, 0x2028));
        }

        private static int CountCommand(
            FakeRpcServer server,
            ushort command)
        {
            return server.ReceivedRequests.Count(
                request => TestFrame.ReadUInt16(request, 0) == command);
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
    }
}

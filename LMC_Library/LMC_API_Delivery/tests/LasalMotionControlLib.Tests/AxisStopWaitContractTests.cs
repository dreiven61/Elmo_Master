using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AxisStopWaitContractTests
    {
        private const string AxisName = "_LMCAxis1";
        private const ushort AxisReference = 1;
        private const string SecondAxisName = "_LMCAxis2";
        private const ushort SecondAxisReference = 2;
        private const int Deceleration = 1000;
        private const int Jerk = 25;
        private const int MovePosition = 1234;
        private const int MoveVelocity = 200;
        private const int MoveAcceleration = 300;
        private const int MoveDeceleration = 400;
        private const int MoveJerk = 5;
        private const uint PowerOnState = 0x00000001u;
        private const uint StandstillState = 0x02000000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "AxisStopWait.Success.OneStopThenThreeStandstillStatuses",
                SuccessSendsExactlyOneStopThenThreeStandstillStatuses);
            tests.Add(
                "AxisStopWait.Rejected.ZeroStatusPolls",
                RejectedAcknowledgementHasZeroStatusPolls);
            tests.Add(
                "AxisStopWait.Timeout.PreservesAcceptedEvidence",
                TimeoutPreservesAcknowledgementStatusAndPollCount);
            tests.Add(
                "AxisStopWait.Timeout.NoAckInvalidatesTransport",
                NoAcknowledgementDeadlineInvalidatesTransport);
            tests.Add(
                "AxisStopWait.Timeout.NoStatusInvalidatesTransport",
                NoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "AxisStopWait.Cancel.PreservesAcceptedEvidence",
                CancellationPreservesAcknowledgementStatusAndPollCount);
            tests.Add(
                "AxisStopWait.NonStandstill.ResetsStability",
                NonStandstillResetsStabilityProof);
            tests.Add(
                "AxisStopWait.AxisError.ResetsStability",
                AxisErrorResetsStabilityProof);
            tests.Add(
                "AxisStopWait.ReadFailure.ThrowsTypedStatus",
                UnsuccessfulReadThrowsTypedStatusException);
            tests.Add(
                "AxisStopWait.Status.ResponseLossPreservesAcceptedEvidence",
                StatusResponseLossPreservesAcceptedEvidence);
            tests.Add(
                "AxisStopWait.Status.SendPriorityDiscardPreservesAcceptedEvidence",
                StatusSendPriorityDiscardPreservesAcceptedEvidence);
            tests.Add(
                "AxisStopWait.PreWireCancel.IsNotAttempted",
                PreWireCancellationPreservesNotAttemptedEvidence);
            tests.Add(
                "AxisStopWait.PreWireCommitCancel.IsNotAttempted",
                CommitWindowCancellationPreservesNotAttemptedEvidence);
            tests.Add(
                "AxisStopWait.Submission.ResponseLossIsUncertain",
                ResponseLossIsUncertainAndNotRetried);
            tests.Add(
                "AxisStopWait.SendPriority.ResultDiscardIsUncertain",
                SendPriorityResultDiscardIsUncertain);
            tests.Add(
                "AxisStopWait.PendingPowerOn.ObservedButNotResolved",
                PendingPowerOnProofAccumulatesWithoutAutomaticResolution);
            tests.Add(
                "AxisStopWait.Split.BeginThenResumeIsStatusOnly",
                SplitBeginThenResumeIsStatusOnly);
            tests.Add(
                "AxisStopWait.Split.ConvenienceResumeInheritsStableCount",
                SplitConvenienceResumeInheritsStableCount);
            tests.Add(
                "AxisStopWait.Split.AcceptedPublicationDeadlinePreservesContinuation",
                SplitAcceptedPublicationDeadlinePreservesContinuation);
            tests.Add(
                "AxisStopWait.Split.TimeoutResumeDoesNotReplay",
                SplitTimeoutResumeDoesNotReplay);
            tests.Add(
                "AxisStopWait.Split.NoStatusDeadlineInvalidatesTransport",
                SplitNoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "AxisStopWait.Split.NewAcceptedStopSupersedesOld",
                SplitNewAcceptedStopSupersedesOldContinuation);
            tests.Add(
                "AxisStopWait.Split.PriorityPreemptsPollAndNewStopCompletes",
                SplitPriorityPreemptsPollAndNewStopCompletes);
            tests.Add(
                "AxisStopWait.Split.ConcurrentBeginPreservesOrder",
                SplitConcurrentBeginPreservesWirePublicationOrder);
            tests.Add(
                "AxisStopWait.Split.ConcurrentResumeSecondIsZeroWire",
                SplitConcurrentResumeSecondIsZeroWire);
            tests.Add(
                "AxisStopWait.Split.StaleSessionResumeIsZeroWire",
                SplitStaleSessionResumeIsZeroWire);
            tests.Add(
                "AxisStopWait.Split.TimeoutResetsPendingPowerOnProof",
                SplitTimeoutResetsPendingPowerOnProof);
            tests.Add(
                "AxisStopWait.Interference.SameReferenceHandleMoveIsZeroStatus",
                SameReferenceHandleMoveInterferesBeforeResume);
            tests.Add(
                "AxisStopWait.Interference.StatusRaceDiscardsResult",
                MutationRacingStatusDiscardsResult);
            tests.Add(
                "AxisStopWait.Interference.ZeroWireMutationDoesNotAdvance",
                ZeroWireMutationDoesNotAdvanceGeneration);
            tests.Add(
                "AxisStopWait.Interference.DifferentAxisDoesNotAdvance",
                DifferentAxisMutationDoesNotInterfere);
            tests.Add(
                "AxisStopWait.Interference.AcceptedWaitMutationsAdvance",
                AcceptedWaitMutationsInterfere);
            tests.Add(
                "AxisStopWait.Observer.AtomicBeforeStatusAndBlocksReentry",
                AcceptedObserverIsAtomicBeforeStatusAndBlocksReentry);
            tests.Add(
                "AxisStopWait.Observer.FailureLeavesRecoverablePending",
                AcceptedObserverFailureLeavesRecoverablePending);
            tests.Add(
                "AxisStopWait.StatusOnly.CrossSessionZeroReplay",
                StatusOnlyCrossSessionSendsOnlyStatus);
            tests.Add(
                "AxisStopWait.StatusOnly.CloseBeforePublicationRejectsProof",
                StatusOnlyCloseBeforePublicationRejectsProof);
            tests.Add(
                "AxisStopWait.StatusOnly.FinalProofRaceIsLinearized",
                StatusOnlyFinalProofRaceIsLinearized);
            tests.Add(
                "AxisStopWait.StatusOnly.UnstableSampleResetsCounter",
                StatusOnlyUnstableSampleResetsCounter);
            tests.Add(
                "AxisStopWait.StatusOnly.TimeoutPreservesEvidence",
                StatusOnlyTimeoutPreservesEvidence);
            tests.Add(
                "AxisStopWait.StatusOnly.PreCanceledIsZeroWire",
                StatusOnlyPreCanceledIsZeroWire);
            tests.Add(
                "AxisStopWait.StatusOnly.ReadFailureIsTyped",
                StatusOnlyReadFailureIsTyped);
            tests.Add(
                "AxisStopWait.StatusOnly.PostWriteDeadlineInvalidatesTransport",
                StatusOnlyPostWriteDeadlineInvalidatesTransport);
            tests.Add(
                "AxisStopWait.StatusOnly.ProcessLocalMutationRaceIsInconclusive",
                StatusOnlyProcessLocalMutationRaceIsInconclusive);
            tests.Add(
                "AxisStopWait.Takeover.AcceptedAtomicallySupersedesReset",
                ResetTakeoverAtomicallyInstallsStop);
            tests.Add(
                "AxisStopWait.Takeover.CompletedResetRaceIsZeroWire",
                CompletedResetRaceIsZeroWire);
            tests.Add(
                "AxisStopWait.Takeover.FailureMatrixPreservesExactState",
                ResetTakeoverFailureMatrixPreservesExactState);
            tests.Add(
                "AxisStopWait.PowerOffRetire.RequiresExactNewerProof",
                PowerOffRetireRequiresExactNewerProof);
            tests.Add(
                "AxisStopWait.PowerOffRetire.FalseMatrixAndResetIsolation",
                PowerOffRetireFalseMatrixAndResetIsolation);
            tests.Add(
                "AxisStopWait.RawReplay.SameAndCrossAreZeroWire",
                RawStopAndResetReplayAreZeroWire);
            tests.Add(
                "AxisStopWait.SafetyPreemption.HeldResetStatusReconnectsAndSendsOneStop",
                HeldResetStatusSafetyAbortReconnectsAndSendsExactlyOneStop);
            tests.Add(
                "AxisMutation.RawValidNackRollsBackExactGeneration",
                RawValidNackRollsBackExactMutationGeneration);
            tests.Add(
                "AxisStopWait.SafetyPreemption.ConcurrentCloseCannotBlockAbort",
                ConcurrentCloseCannotBlockSafetyAbort);
            tests.Add(
                "AxisStopWait.SafetyPreemption.ReconnectPublicationIsSessionPinned",
                ReconnectPublicationIsAtomicAgainstPinnedAbort);
        }

        private static void
            SuccessSendsExactlyOneStopThenThreeStandstillStatuses()
        {
            var defaults = new LMCAxisStopWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.StopAndWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.StopAccepted);
                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.NotNull(result.FinalStatus);
                AssertEx.True(result.FinalStatus.IsSuccess);
                AssertEx.True(result.FinalStatus.IsStandstill);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandstillSampleCount);
                AssertEx.Equal(3, result.RequiredStableSampleCount);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);
                AssertEx.False(result.TransportInvalidatedAtDeadline);
                AssertEx.Equal(Deceleration, result.Evidence.Deceleration);
                AssertEx.Equal(Jerk, result.Evidence.Jerk);

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
                StopStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<LMCAxisStopRejectedException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.Rejected,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
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
                StopStep(true),
                StatusStep(state: 0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<LMCAxisStopWaitTimeoutException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            new LMCAxisStopWaitOptions
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
                    LMCAxisStopSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.StopAccepted);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.IsSuccess);
                AssertEx.False(error.LastObservedStatus.IsStandstill);
                AssertEx.Equal(1, error.StatusPollCount);
                AssertEx.Equal(
                    0,
                    error.Evidence.StableStandstillSampleCount);
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
                StopStep(true),
                StatusStep(afterRequest: cancellation.Cancel),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<LMCAxisStopWaitCanceledException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.IsSuccess);
                AssertEx.True(error.LastObservedStatus.IsStandstill);
                AssertEx.Equal(1, error.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableStandstillSampleCount);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsSuccess);
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
                var blockedStop = StopStep(true);
                blockedStop.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Stop response was not released.");
                blockedStop.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedStop))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisStopWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisStopWaitTimeoutException>(
                            () => axis
                                .StopAndWaitForStableStandstillAsync(
                                    Deceleration,
                                    Jerk,
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
                        LMCAxisStopSubmissionOutcome.OutcomeUncertain,
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
                    StopStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisStopWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisStopWaitTimeoutException>(
                            () => axis
                                .StopAndWaitForStableStandstillAsync(
                                    Deceleration,
                                    Jerk,
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
                        LMCAxisStopSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.StopAccepted);
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

        private static void NonStandstillResetsStabilityProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(state: 0),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.StopAndWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandstillSampleCount);
                AssertEx.True(result.FinalStatus.IsSuccess);
                AssertEx.True(result.FinalStatus.IsStandstill);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 6);
            }
        }

        private static void AxisErrorResetsStabilityProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
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
                var result = axis.StopAndWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandstillSampleCount);
                AssertEx.True(result.FinalStatus.IsSuccess);
                AssertEx.True(result.FinalStatus.IsStandstill);

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
                StopStep(true),
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
                var error = AssertEx.Throws<LMCAxisStopStatusException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(error.Evidence.StopAccepted);
                AssertEx.NotNull(error.FailedStatus);
                AssertEx.True(ReferenceEquals(
                    error.FailedStatus,
                    error.Evidence.LastObservedStatus));
                AssertEx.False(error.FailedStatus.IsReadSuccessful);
                AssertEx.Equal(2, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    0,
                    error.Evidence.StableStandstillSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 2);
            }
        }

        private static void StatusResponseLossPreservesAcceptedEvidence()
        {
            var statusResponseLoss = new FakeRpcStep(0x2028, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReadStatus(AxisReference),
                    request)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                statusResponseLoss))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<LMCAxisStopStatusException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.True(error.Evidence.StopAccepted);
                AssertEx.NotNull(error.Evidence.StopAcknowledgement);
                AssertEx.True(error.Evidence.StopAcknowledgement.IsSuccess);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    error.Evidence.LastObservedStatus);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.Equal<LMCReadStatusResult>(null, error.FailedStatus);
                AssertEx.NotNull(error.InnerException);

                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            StatusSendPriorityDiscardPreservesAcceptedEvidence()
        {
            const string stopOperation = "Axis Stop status completion";
            const string powerOffOperation = "Priority Axis Power Off";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                DelayedStatusStep(statusReceived, releaseStatus),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var expectedGeneration =
                        coordinator.CurrentGeneration;
                    var stop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                stopOperation))
                            {
                                return axis
                                    .StopAndWaitForStableStandstillAsync(
                                        Deceleration,
                                        Jerk,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The post-ACK status request did not reach the server.");

                    var reservedGeneration =
                        coordinator.ReservePrioritySend();
                    var powerOff = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                powerOffOperation))
                            {
                                return axis.PowerOff();
                            }
                        });

                    releaseStatus.Set();
                    var error = AssertEx.Throws<LMCAxisStopStatusException>(
                        () => stop.GetAwaiter().GetResult());
                    var powerOffResponse = powerOff.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        LMCAxisStopSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                    AssertEx.True(error.Evidence.StopAccepted);
                    AssertEx.NotNull(error.Evidence.StopAcknowledgement);
                    AssertEx.True(
                        error.Evidence.StopAcknowledgement.IsSuccess);
                    AssertEx.Equal<LMCReadStatusResult>(
                        null,
                        error.Evidence.LastObservedStatus);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);
                    AssertEx.Equal<LMCReadStatusResult>(
                        null,
                        error.FailedStatus);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.True(powerOffResponse.IsSuccess);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                }
                finally
                {
                    releaseStatus.Set();
                }
            }
        }

        private static void
            PreWireCancellationPreservesNotAttemptedEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                cancellation.Cancel();
                var error = AssertEx.Throws<LMCAxisStopWaitCanceledException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.StopAcknowledgement);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.Equal(0L, error.Evidence.StopMutationGeneration);
                AssertEx.Equal(
                    0L,
                    error.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
            }
        }

        private static void ResponseLossIsUncertainAndNotRetried()
        {
            var responseLoss = new FakeRpcStep(0x2022, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk),
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
                var error = AssertEx.Throws<LMCAxisStopSubmissionException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.OutcomeUncertain,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.StopAcknowledgement);
                AssertEx.True(error.Evidence.StopMutationGeneration > 0);
                AssertEx.Equal(
                    error.Evidence.StopMutationGeneration,
                    error.Evidence.ObservedMutationGeneration);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                server.Verify();
                AssertCommandCounts(server, 1, 0);
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
                var error = AssertEx.Throws<LMCAxisStopWaitCanceledException>(
                    () => axis.StopAndWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.False(
                    error.Evidence.TransportInvalidatedAtDeadline);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.StopAcknowledgement);
                AssertEx.Equal(0L, error.Evidence.StopMutationGeneration);
                AssertEx.Equal(
                    0L,
                    error.Evidence.ObservedMutationGeneration);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsSuccess);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void SendPriorityResultDiscardIsUncertain()
        {
            const string stopOperation = "Axis Stop completion";
            const string powerOffOperation = "Priority Axis Power Off";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var stopReceived = new ManualResetEventSlim(false))
            using (var releaseStop = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                DelayedStopStep(stopReceived, releaseStop),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var expectedGeneration =
                        coordinator.CurrentGeneration;
                    var stop = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                stopOperation))
                            {
                                return axis
                                    .StopAndWaitForStableStandstillAsync(
                                        Deceleration,
                                        Jerk,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        stopReceived.Wait(2000),
                        "The Stop request did not reach the server.");

                    var reservedGeneration =
                        coordinator.ReservePrioritySend();
                    var powerOff = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                reservedGeneration,
                                powerOffOperation))
                            {
                                return axis.PowerOff();
                            }
                        });

                    releaseStop.Set();
                    var error = AssertEx.Throws<
                        LMCAxisStopSubmissionException>(
                        () => stop.GetAwaiter().GetResult());
                    var powerOffResponse = powerOff.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        LMCAxisStopSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.True(powerOffResponse.IsSuccess);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 0);
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                }
                finally
                {
                    releaseStop.Set();
                }
            }
        }

        private static void
            PendingPowerOnProofAccumulatesWithoutAutomaticResolution()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOnState | StandstillState),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var powerOnTime = new FakeWaitTime();
                var pendingError = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            powerOnTime.ElapsedMilliseconds,
                            powerOnTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                var continuation = pendingError.Continuation;

                AssertEx.NotNull(continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                var stopTime = new FakeWaitTime();
                var result = axis.StopAndWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        LongOptions(),
                        CancellationToken.None,
                        stopTime.ElapsedMilliseconds,
                        stopTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.StopAccepted);
                AssertEx.Equal(
                    3,
                    continuation.StablePowerOffStandstillSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void SplitBeginThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var continuation = axis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        LongOptions(),
                        CancellationToken.None,
                        beginTime.ElapsedMilliseconds,
                        beginTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.Acknowledgement.IsSuccess);
                AssertEx.Equal(Deceleration, continuation.Deceleration);
                AssertEx.Equal(Jerk, continuation.Jerk);
                AssertEx.Equal(
                    continuation,
                    axis.PendingStopWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var resumeTime = new FakeWaitTime();
                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
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
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        new LMCAxisStopWaitOptions
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

                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.Equal(5, result.RequiredStableSampleCount);
                AssertEx.Equal(5, result.StableStandstillSampleCount);
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
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisStopWaitTimeoutException>(
                    () => axis.BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            new LMCAxisStopWaitOptions
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
                    axis.PendingStopWaitContinuation);
                AssertCommandCounts(server, 1, 0);

                var resumeTime = new FakeWaitTime();
                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
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

        private static void SplitTimeoutResumeDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisStopWaitTimeoutException>(
                    () => axis.ResumeStopWaitForStableStandstillAsync(
                            continuation,
                            new LMCAxisStopWaitOptions
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
                    timeout.Evidence.StableStandstillSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StableStandstillSampleCount);
                AssertCommandCounts(server, 1, 1);

                var resumeTime = new FakeWaitTime();
                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandstillSampleCount);
                AssertEx.False(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void SplitNoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep();
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Stop status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    StopStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var continuation = axis
                        .BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    LMCAxisStopWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisStopWaitTimeoutException>(
                            () => axis
                                .ResumeStopWaitForStableStandstillAsync(
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

                    AssertEx.True(error.Evidence.StopAccepted);
                    AssertEx.Equal<LMCReadStatusResult>(
                        null,
                        error.LastObservedStatus);
                    AssertEx.Equal(0, error.StatusPollCount);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(continuation, error.Continuation);
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
            SplitNewAcceptedStopSupersedesOldContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var second = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(first.IsSuperseded);
                AssertEx.False(first.IsPending);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(second, axis.PendingStopWaitContinuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ResumeStopWaitForStableStandstillAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertCommandCounts(server, 2, 0);

                var result = axis.ResumeStopWaitForStableStandstillAsync(
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

        private static void
            SplitPriorityPreemptsPollAndNewStopCompletes()
        {
            const string firstMonitorOperation =
                "First Axis Stop verification";
            const string secondStopOperation = "Second Axis Stop";
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
                AxisInfoStep(),
                StopStep(true),
                DelayedStatusStep(statusReceived, releaseStatus),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection(connectionOptions))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var firstGeneration = coordinator.ReservePrioritySend();
                    LMCAxisStopWaitContinuation first;
                    using (coordinator.BeginPriorityScope(
                        firstGeneration,
                        "First Axis Stop"))
                    {
                        first = axis
                            .BeginStopWaitForStableStandstillAsync(
                                Deceleration,
                                Jerk,
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
                                return axis
                                    .ResumeStopWaitForStableStandstillAsync(
                                        first,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The first split Stop status poll did not reach the server.");

                    var secondGeneration = coordinator.ReservePrioritySend();
                    var secondBegin = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                secondGeneration,
                                secondStopOperation))
                            {
                                return axis
                                    .BeginStopWaitForStableStandstillAsync(
                                        Deceleration,
                                        Jerk,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    releaseStatus.Set();

                    var preempted = AssertEx.Throws<
                        LMCAxisStopStatusException>(
                        () => firstMonitor.GetAwaiter().GetResult());
                    AssertEx.True(
                        preempted.InnerException
                            is LMCSendPreemptedException);
                    AssertEx.Equal(first, preempted.Continuation);
                    var second = secondBegin.GetAwaiter().GetResult();
                    AssertEx.True(first.IsSuperseded);
                    AssertEx.True(second.IsPending);

                    LMCAxisStopWaitResult result;
                    using (coordinator.BeginPreemptibleScope(
                        secondGeneration,
                        "Second Axis Stop verification"))
                    {
                        result = axis
                            .ResumeStopWaitForStableStandstillAsync(
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

        private static void
            SplitConcurrentBeginPreservesWirePublicationOrder()
        {
            using (var firstAcknowledgementHeld =
                new ManualResetEventSlim(false))
            using (var releaseFirstPublication =
                new ManualResetEventSlim(false))
            using (var secondInvocationStarted =
                new ManualResetEventSlim(false))
            using (var secondStopReceived =
                new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                ObservedStopStep(secondStopReceived),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var firstTime = new FakeWaitTime();
                    var firstTask = Task.Run(
                        () => axis
                            .BeginStopWaitForStableStandstillAsync(
                                Deceleration,
                                Jerk,
                                LongOptions(),
                                CancellationToken.None,
                                firstTime.ElapsedMilliseconds,
                                firstTime.DelayAsync,
                                () =>
                                {
                                    firstAcknowledgementHeld.Set();
                                    AssertEx.True(
                                        releaseFirstPublication.Wait(5000),
                                        "The first accepted Stop publication was not released.");
                                })
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstAcknowledgementHeld.Wait(2000),
                        "The first Stop acknowledgement was not held before publication.");

                    var secondTask = Task.Run(
                        () =>
                        {
                            secondInvocationStarted.Set();
                            return axis
                                .BeginStopWaitForStableStandstillAsync(
                                    Deceleration,
                                    Jerk,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        });
                    AssertEx.True(
                        secondInvocationStarted.Wait(2000),
                        "The second Stop Begin did not start.");
                    AssertEx.False(
                        secondStopReceived.Wait(500),
                        "A newer Stop reached the wire before the earlier accepted continuation was published.");

                    releaseFirstPublication.Set();
                    var first = firstTask.GetAwaiter().GetResult();
                    var second = secondTask.GetAwaiter().GetResult();

                    AssertEx.True(first.IsSuperseded);
                    AssertEx.False(first.IsPending);
                    AssertEx.True(second.IsPending);
                    AssertEx.Equal(second, axis.PendingStopWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 2, 0);
                }
                finally
                {
                    releaseFirstPublication.Set();
                }
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
                AxisInfoStep(),
                StopStep(true),
                DelayedStatusStep(
                    firstStatusReceived,
                    releaseFirstStatus),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var axis = ConnectAndCreateAxis(
                        connection,
                        server.Port);
                    var continuation = axis
                        .BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var firstResume = Task.Run(
                        () => axis.ResumeStopWaitForStableStandstillAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstStatusReceived.Wait(2000),
                        "The first concurrent Stop resume did not poll status.");

                    var secondResume = Task.Run(
                        () =>
                        {
                            secondResumeStarted.Set();
                            return axis
                                .ResumeStopWaitForStableStandstillAsync(
                                    continuation,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        });
                    AssertEx.True(
                        secondResumeStarted.Wait(2000),
                        "The second concurrent Stop resume did not start.");
                    releaseFirstStatus.Set();

                    var result = firstResume.GetAwaiter().GetResult();
                    AssertEx.Equal(continuation, result.Continuation);
                    AssertEx.Throws<InvalidOperationException>(
                        () => secondResume.GetAwaiter().GetResult());

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

        private static void SplitStaleSessionResumeIsZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true)))
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
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
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
                    () => newAxis.ResumeStopWaitForStableStandstillAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertEx.Equal(0, CountCommand(secondServer, 0x2022));
                AssertEx.Equal(0, CountCommand(secondServer, 0x2028));
            }
        }

        private static void SplitTimeoutResetsPendingPowerOnProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOnState | StandstillState),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var powerOnTime = new FakeWaitTime();
                var powerOnTimeout = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            powerOnTime.ElapsedMilliseconds,
                            powerOnTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                var pendingPowerOn = powerOnTimeout.Continuation;
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var firstResumeTime = new FakeWaitTime();
                var stopTimeout = AssertEx.Throws<
                    LMCAxisStopWaitTimeoutException>(
                    () => axis.ResumeStopWaitForStableStandstillAsync(
                            stop,
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            firstResumeTime.ElapsedMilliseconds,
                            firstResumeTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(stop, stopTimeout.Continuation);
                AssertEx.Equal(
                    1,
                    stopTimeout.Evidence.StableStandstillSampleCount);
                AssertEx.Equal(0, stop.StableStandstillSampleCount);
                AssertEx.Equal(
                    0,
                    pendingPowerOn.StablePowerOffStandstillSampleCount);
                AssertCommandCounts(server, 1, 2);

                var secondResumeTime = new FakeWaitTime();
                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        stop,
                        LongOptions(),
                        CancellationToken.None,
                        secondResumeTime.ElapsedMilliseconds,
                        secondResumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(stop, result.Continuation);
                AssertEx.Equal(
                    3,
                    pendingPowerOn.StablePowerOffStandstillSampleCount);
                AssertCommandCounts(server, 1, 5);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void SameReferenceHandleMoveInterferesBeforeResume()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, AxisName);
                var continuation = first
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var stopGeneration = continuation.StopMutationGeneration;

                AssertEx.True(stopGeneration > 0);
                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                var interference = AssertEx.Throws<
                    LMCAxisStopInterferenceException>(
                    () => first.ResumeStopWaitForStableStandstillAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertStopInterference(
                    interference,
                    continuation,
                    stopGeneration,
                    stopGeneration + 1,
                    0);
                AssertEx.Equal(
                    continuation,
                    first.PendingStopWaitContinuation);
                AssertEx.Equal(
                    continuation,
                    second.PendingStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void MutationRacingStatusDiscardsResult()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var stopGeneration = continuation.StopMutationGeneration;
                var time = new FakeWaitTime();

                var interference = AssertEx.Throws<
                    LMCAxisStopInterferenceException>(
                    () => axis.ResumeStopWaitForStableStandstillAsync(
                            continuation,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(
                                SendMoveAbsolute(axis).IsSuccess))
                        .GetAwaiter()
                        .GetResult());

                AssertStopInterference(
                    interference,
                    continuation,
                    stopGeneration,
                    stopGeneration + 1,
                    0);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    interference.Evidence.LastObservedStatus);
                AssertEx.Equal(0, continuation.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void ZeroWireMutationDoesNotAdvanceGeneration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection.GetAxisPowerOnWaitCoordinator(
                    axis.SessionGeneration,
                    axis.AxisReference);
                var stopGeneration = continuation.StopMutationGeneration;
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => axis.MoveAbsoluteExAsync(
                            MovePosition,
                            MoveVelocity,
                            MoveAcceleration,
                            MoveDeceleration,
                            MoveJerk,
                            LMC_DIRECTION.Shortest,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => axis.MoveAbsoluteEx(
                        MovePosition,
                        MoveVelocity,
                        MoveAcceleration,
                        MoveDeceleration,
                        MoveJerk,
                        LMC_DIRECTION.Positive));
                AssertEx.Equal(
                    stopGeneration,
                    coordinator.MutationGeneration);

                var result = axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    stopGeneration,
                    result.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(0, CountCommand(server, 0x209F));
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
                StopStep(true),
                MoveAbsoluteStep(SecondAxisReference),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, SecondAxisName);
                var continuation = first
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
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
                var stopGeneration = continuation.StopMutationGeneration;

                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                AssertEx.Equal(
                    stopGeneration,
                    firstCoordinator.MutationGeneration);
                AssertEx.Equal(1L, secondCoordinator.MutationGeneration);

                var result = first.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    stopGeneration,
                    result.Evidence.StopMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
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
                StopStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = new LMCAxisStopWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var observerCount = 0;
                var result = axis.StopAndWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        options,
                        continuation =>
                        {
                            observerCount++;
                            AssertEx.Equal(
                                continuation,
                                axis.PendingStopWaitContinuation);
                            AssertEx.Equal(0, CountCommand(server, 0x2028));

                            var replay = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => axis
                                    .ResumeStopWaitForStableStandstillAsync(
                                        continuation,
                                        options,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult());
                            AssertEx.Equal(
                                continuation,
                                replay.StopContinuation);

                            var mutation = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => SendMoveAbsolute(axis));
                            AssertEx.Equal(
                                continuation,
                                mutation.StopContinuation);

                            var statusOnly = AssertEx.Throws<
                                LMCAxisAcceptedObserverInProgressException>(
                                () => axis.WaitForStableStandstillAsync(
                                        options,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult());
                            AssertEx.Equal(
                                continuation,
                                statusOnly.StopContinuation);
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
                AssertEx.Equal(0, CountCommand(server, 0x209F));
            }
        }

        private static void AcceptedObserverFailureLeavesRecoverablePending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = new LMCAxisStopWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var observerCount = 0;
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
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

                var pending = axis.PendingStopWaitContinuation;
                AssertEx.NotNull(pending);
                AssertEx.True(pending.IsPending);
                AssertEx.Equal(1, observerCount);
                AssertCommandCounts(server, 1, 0);

                var result = axis.ResumeStopWaitForStableStandstillAsync(
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
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                var result = axis.WaitForStableStandstillAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableStandstillSampleCount);
                AssertEx.True(result.FinalStatus.IsStandstill);

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
                    LMCAxisStableStandstillStatusException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
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
                var result = axis.WaitForStableStandstillAsync(
                        new LMCAxisStopWaitOptions
                        {
                            TimeoutMilliseconds = 10,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 1
                        },
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
                    LMCAxisStableStandstillWaitCanceledException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableStandstillSampleCount);
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
                    LMCAxisStableStandstillWaitTimeoutException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
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
                    error.Evidence.StableStandstillSampleCount);
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
                StatusStep(PowerOnState),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var result = axis.WaitForStableStandstillAsync(
                        new LMCAxisStopWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 2
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(2, result.StableStandstillSampleCount);
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
                StatusStep(PowerOnState),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableStandstillWaitTimeoutException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
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
                AssertEx.Equal(0, CountCommand(server, 0x2022));
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
                    LMCAxisStableStandstillWaitCanceledException>(
                    () => axis.WaitForStableStandstillAsync(
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
                    StandstillState,
                    functionStatus: 0x0010,
                    errorId: -31,
                    axisErrorId: 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisStableStandstillStatusException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
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
                    "The blocked status response was not released.");
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
                    LMCAxisStableStandstillWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisStableStandstillWaitTimeoutException>(
                            () => axis.WaitForStableStandstillAsync(
                                    new LMCAxisStopWaitOptions
                                    {
                                        TimeoutMilliseconds = 200,
                                        PollIntervalMilliseconds = 10,
                                        StableSampleCount = 1
                                    },
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
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableStandstillInterferenceException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            () => AssertEx.True(
                                SendMoveAbsolute(axis).IsSuccess))
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
                AssertEx.Equal(1, CountCommand(server, 0x209F));
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
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var publicationCount = 0;
                var error = AssertEx.Throws<
                    LMCAxisStableStandstillInterferenceException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
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
                                    AssertEx.True(
                                        SendMoveAbsolute(axis).IsSuccess);
                                }
                            })
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StableStandstillSampleCount);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
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
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisStableStandstillInterferenceException>(
                    () => axis.WaitForStableStandstillAsync(
                            new LMCAxisStopWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(
                                SendMoveAbsolute(axis).IsSuccess))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.True(
                    error.Evidence
                        .InterveningProcessLocalMutationDetected);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void AcceptedWaitMutationsInterfere()
        {
            AcceptedPowerOnWaitMutationInterferes();
            AcceptedPowerOffWaitMutationInterferes();
            AcceptedResetWaitMutationInterferes();
        }

        private static void AcceptedPowerOnWaitMutationInterferes()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                PowerStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAxisPowerOnWaitContinuation powerOn = null;

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions(),
                            continuation =>
                            {
                                powerOn = continuation;
                                throw new InvalidOperationException(
                                    "Stop after accepted Power On publication.");
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.NotNull(powerOn);
                AssertEx.True(powerOn.IsPending);
                AssertAcceptedWaitInterference(axis, stop);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void AcceptedPowerOffWaitMutationInterferes()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                PowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(powerOff.IsPending);
                AssertAcceptedWaitInterference(axis, stop);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void AcceptedResetWaitMutationInterferes()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var pending = AssertEx.Throws<
                    LMCAxisStopWaitPendingException>(
                    () => axis.ResetAndWaitForStableErrorClearanceAsync(
                            new LMCAxisResetWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 1
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(stop, pending.Continuation);
                AssertEx.True(stop.IsPending);
                AssertEx.Equal(
                    stop,
                    axis.PendingStopWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(0, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverAtomicallyInstallsStop()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                StopStep(true),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAxisStopWaitContinuation observedStop = null;
                var result = axis
                    .StopAndWaitForStableStandstillWithResetTakeoverAsync(
                        reset,
                        Deceleration,
                        Jerk,
                        new LMCAxisStopWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 1
                        },
                        stop =>
                        {
                            observedStop = stop;
                            AssertEx.Equal(
                                LMCAxisResetWaitContinuationState
                                    .SupersededBySafetyStop,
                                reset.State);
                            AssertEx.Equal(
                                stop,
                                reset.SupersedingSafetyStopContinuation);
                            AssertEx.Equal<LMCAxisResetWaitContinuation>(
                                null,
                                axis.PendingResetWaitContinuation);
                            AssertEx.Equal(
                                stop,
                                axis.PendingStopWaitContinuation);
                            AssertEx.Equal(0, CountCommand(server, 0x2028));
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(observedStop, result.Continuation);
                AssertEx.Equal(
                    LMCAxisStopWaitContinuationState.Completed,
                    result.Continuation.State);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void CompletedResetRaceIsZeroWire()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            using (var takeoverStarted = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                DelayedStatusStep(statusReceived, releaseStatus),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var resetOptions = new LMCAxisResetWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var stopOptions = new LMCAxisStopWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        resetOptions,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var resume = Task.Run(
                    () => axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            reset,
                            resetOptions,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(statusReceived.Wait(2000));

                var takeover = Task.Run(
                    () =>
                    {
                        takeoverStarted.Set();
                        return axis
                            .BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                                reset,
                                Deceleration,
                                Jerk,
                                stopOptions,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    });
                AssertEx.True(takeoverStarted.Wait(2000));
                releaseStatus.Set();
                var resetResult = resume.GetAwaiter().GetResult();
                var failure = AssertEx.Throws<
                    LMCAxisStopSubmissionException>(
                    () => takeover.GetAwaiter().GetResult());

                AssertEx.False(failure.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState.Completed,
                    reset.State);
                AssertEx.Equal(reset, resetResult.Continuation);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverFailureMatrixPreservesExactState()
        {
            ResetTakeoverNackPreservesReset();
            ResetTakeoverPrewireCancelPreservesReset();
            ResetTakeoverPrewireDeadlinePreservesReset();
            ResetTakeoverPostWriteLossPreservesReset();
            ResetTakeoverObserverFailureKeepsAcceptedStop();
        }

        private static void ResetTakeoverNackPreservesReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                StopStep(false),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var error = AssertEx.Throws<LMCAxisStopRejectedException>(
                    () => axis
                        .BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                            reset,
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.False(error.Evidence.StopAccepted);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState.Pending,
                    reset.State);
                AssertEx.Equal(reset, axis.PendingResetWaitContinuation);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                var resumed = axis
                    .ResumeResetWaitForStableErrorClearanceAsync(
                        reset,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(reset, resumed.Continuation);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState.Completed,
                    reset.State);
                AssertEx.Equal(3, resumed.StatusPollCount);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverPrewireCancelPreservesReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                cancellation.Cancel();
                var error = AssertEx.Throws<LMCAxisStopWaitCanceledException>(
                    () => axis
                        .BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                            reset,
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.Equal(reset, axis.PendingResetWaitContinuation);
                AssertEx.True(reset.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverPrewireDeadlinePreservesReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                time.DelayAsync(10, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisStopWaitOptions
                {
                    TimeoutMilliseconds = 10,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var error = AssertEx.Throws<LMCAxisStopWaitTimeoutException>(
                    () => axis.BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            null,
                            reset)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.Equal(reset, axis.PendingResetWaitContinuation);
                AssertEx.True(reset.IsPending);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverPostWriteLossPreservesReset()
        {
            var responseLoss = new FakeRpcStep(0x2022, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk),
                    request)
            };
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var error = AssertEx.Throws<LMCAxisStopSubmissionException>(
                    () => axis
                        .BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                            reset,
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCAxisStopSubmissionOutcome.OutcomeUncertain,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(reset, axis.PendingResetWaitContinuation);
                AssertEx.True(reset.IsPending);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void ResetTakeoverObserverFailureKeepsAcceptedStop()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAxisStopWaitContinuation stop = null;
                AssertEx.Throws<InvalidOperationException>(
                    () => axis
                        .BeginStopWaitForStableStandstillWithResetTakeoverAsync(
                            reset,
                            Deceleration,
                            Jerk,
                            LongOptions(),
                            continuation =>
                            {
                                stop = continuation;
                                throw new InvalidOperationException(
                                    "durable commit failed");
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.NotNull(stop);
                AssertEx.True(stop.IsPending);
                AssertEx.Equal(stop, axis.PendingStopWaitContinuation);
                AssertEx.Equal<LMCAxisResetWaitContinuation>(
                    null,
                    axis.PendingResetWaitContinuation);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState
                        .SupersededBySafetyStop,
                    reset.State);
                AssertEx.Equal(stop, reset.SupersedingSafetyStopContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void PowerOffRetireRequiresExactNewerProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                PowerOffStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                var powerOffResult = axis
                    .ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(powerOff, powerOffResult.Continuation);
                AssertEx.True(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                AssertEx.Equal(
                    LMCAxisStopWaitContinuationState
                        .SupersededByStablePowerOff,
                    stop.State);
                AssertEx.Equal(
                    powerOff,
                    stop.SupersedingPowerOffContinuation);
                AssertEx.Equal<LMCAxisStopWaitContinuation>(
                    null,
                    axis.PendingStopWaitContinuation);
                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void PowerOffRetireFalseMatrixAndResetIsolation()
        {
            StablePowerOffNeverRetiresReset();
            PowerOffRetireRejectsCurrentGenerationMismatch();
            PowerOffRetireRejectsWrongLatestStop();
            PowerOffRetireRejectsWrongAxis();
            PowerOffRetireRejectsOlderProofAndStaleSession();
        }

        private static void StablePowerOffNeverRetiresReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                PowerOffStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                axis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(reset.IsPending);
                AssertEx.Equal(reset, axis.PendingResetWaitContinuation);
                AssertEx.Equal(
                    LMCAxisResetWaitContinuationState.Pending,
                    reset.State);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void
            PowerOffRetireRejectsCurrentGenerationMismatch()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                PowerOffStep(),
                StatusStep(),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                axis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(SendMoveAbsolute(axis).IsSuccess);
                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                AssertEx.True(stop.IsPending);
                AssertEx.Equal(stop, axis.PendingStopWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void PowerOffRetireRejectsWrongLatestStop()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                StopStep(true),
                PowerOffStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var second = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                axis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        first,
                        powerOff));
                AssertEx.Equal(
                    LMCAxisStopWaitContinuationState
                        .SupersededByNewerStop,
                    first.State);
                AssertEx.Equal(second, axis.PendingStopWaitContinuation);
                AssertEx.True(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        second,
                        powerOff));
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 1);
            }
        }

        private static void PowerOffRetireRejectsWrongAxis()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                LookupStep(SecondAxisReference),
                AxisInfoStep(SecondAxisReference),
                StopStep(true),
                PowerOffStep(SecondAxisReference),
                StatusStep(axisReference: SecondAxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var firstAxis = ConnectAndCreateAxis(
                    connection,
                    server.Port);
                var secondAxis = new LMCAxis(connection, SecondAxisName);
                var stop = firstAxis
                    .BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = secondAxis
                    .BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                secondAxis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    firstAxis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                AssertEx.True(stop.IsPending);
                AssertEx.Equal(stop, firstAxis.PendingStopWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void
            PowerOffRetireRejectsOlderProofAndStaleSession()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerOffStep(),
                StatusStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 1
                };
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                axis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                AssertEx.True(stop.IsPending);
                connection.CloseConnection();
                AssertEx.False(
                    axis.TryRetirePendingStopAfterStablePowerOff(
                        stop,
                        powerOff));
                AssertEx.True(stop.IsPending);
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void RawStopAndResetReplayAreZeroWire()
        {
            RawCommandsAreZeroWireForPendingStop();
            RawCommandsAreZeroWireForPendingReset();
        }

        private static void RawCommandsAreZeroWireForPendingStop()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StopStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var stop = axis.BeginStopWaitForStableStandstillAsync(
                        Deceleration,
                        Jerk,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    stop,
                    AssertEx.Throws<LMCAxisStopWaitPendingException>(
                        () => axis.Stop(Deceleration, Jerk))
                        .Continuation);
                AssertEx.Equal(
                    stop,
                    AssertEx.Throws<LMCAxisStopWaitPendingException>(
                        () => axis.Reset())
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
                AssertCommandCounts(server, 1, 0);
                AssertEx.Equal(0, CountCommand(server, 0x2024));
            }
        }

        private static void RawCommandsAreZeroWireForPendingReset()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                ResetStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var reset = axis
                    .BeginResetWaitForStableErrorClearanceAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    reset,
                    AssertEx.Throws<LMCAxisResetWaitPendingException>(
                        () => axis.Reset())
                        .Continuation);
                AssertEx.Equal(
                    reset,
                    AssertEx.Throws<LMCAxisResetWaitPendingException>(
                        () => axis.Stop(Deceleration, Jerk))
                        .Continuation);
                AssertEx.Equal(
                    reset,
                    AssertEx.Throws<LMCAxisResetWaitPendingException>(
                        () => axis.BeginStopWaitForStableStandstillAsync(
                                Deceleration,
                                Jerk,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult())
                        .Continuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2024));
            }
        }

        private static void
            HeldResetStatusSafetyAbortReconnectsAndSendsExactlyOneStop()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            {
                var blockedStatus = DelayedStatusStep(
                    statusReceived,
                    releaseStatus);
                blockedStatus.AllowClientDisconnectAfterRequest = true;
                blockedStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
                blockedStatus.CloseClientAfterResponseAndContinue = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    ResetStep(),
                    blockedStatus,
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    StopStep(true),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var resetOptions = new LMCAxisResetWaitOptions
                    {
                        TimeoutMilliseconds = 10000,
                        PollIntervalMilliseconds = 10,
                        StableSampleCount = 1
                    };
                    var reset = axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            resetOptions,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var oldSessionGeneration = connection.SessionGeneration;
                    var resume = axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            reset,
                            resetOptions,
                            CancellationToken.None);
                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The held Reset status request was not written.");

                    var safetyFaultEvents = 0;
                    connection.ConnectionStateChanged += (sender, args) =>
                    {
                        if (args.CurrentState == LMCConnectionState.Faulted
                            && args.Exception is
                                LMCSafetyPreemptionTransportAbortedException)
                        {
                            Interlocked.Increment(ref safetyFaultEvents);
                        }
                    };

                    var abortElapsed = Stopwatch.StartNew();
                    var abort =
                        connection.AbortTransportForSafetyPreemption(
                            reset.SessionGeneration);
                    abortElapsed.Stop();

                    AssertEx.True(
                        abortElapsed.ElapsedMilliseconds < 1000,
                        "Safety preemption did not promptly detach the held status transport.");
                    AssertEx.Equal(oldSessionGeneration, abort.SessionGeneration);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        abort.StateBefore);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        abort.StateAfter);
                    AssertEx.True(abort.TransportDetached);
                    AssertEx.True(abort.AbortiveLingerApplied);
                    AssertEx.True(abort.FaultStatePublished);
                    AssertEx.True(abort.ReconnectRequired);
                    AssertEx.Equal(1, safetyFaultEvents);
                    AssertEx.True(
                        connection.LastTransportException is
                            LMCSafetyPreemptionTransportAbortedException);

                    var repeated =
                        connection.AbortTransportForSafetyPreemption(
                            reset.SessionGeneration);
                    AssertEx.False(repeated.TransportDetached);
                    AssertEx.False(repeated.AbortiveLingerApplied);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        repeated.StateBefore);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        repeated.StateAfter);
                    AssertEx.Equal(1, safetyFaultEvents);

                    releaseStatus.Set();
                    var reconnect = connection.RpcInitConnectionAsync(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask,
                        CancellationToken.None);
                    AssertEx.Throws<LMCAxisResetStatusException>(
                        () => resume.GetAwaiter().GetResult());
                    reconnect.GetAwaiter().GetResult();

                    AssertEx.True(connection.IsConnected);
                    AssertEx.True(
                        connection.SessionGeneration > oldSessionGeneration);
                    var mismatch = AssertEx.Throws<
                        LMCSafetyPreemptionSessionMismatchException>(
                        () => connection
                            .AbortTransportForSafetyPreemption(
                                oldSessionGeneration));
                    AssertEx.Equal(
                        oldSessionGeneration,
                        mismatch.ExpectedSessionGeneration);
                    AssertEx.Equal(
                        connection.SessionGeneration,
                        mismatch.ObservedSessionGeneration);
                    AssertEx.True(connection.IsConnected);
                    var freshAxis = new LMCAxis(connection, AxisName);
                    var stop = freshAxis
                        .BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(stop.IsPending);
                    AssertEx.True(stop.Acknowledgement.IsSuccess);
                    AssertEx.True(connection.IsConnected);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                    AssertEx.Equal(1, CountCommand(server, 0x405D));
                    AssertEx.Equal(2, server.AcceptedClientCount);

                    for (var i = 0; i < server.ReceivedRequests.Count; i++)
                    {
                        if (TestFrame.ReadUInt16(
                                server.ReceivedRequests[i],
                                0) == 0x2022)
                        {
                            AssertEx.Equal(
                                2,
                                server.ReceivedRequestSessionOrdinals[i]);
                        }
                    }
                }
            }
        }

        private static void ConcurrentCloseCannotBlockSafetyAbort()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            using (var closeEntered = new ManualResetEventSlim(false))
            {
                var blockedStatus = DelayedStatusStep(
                    statusReceived,
                    releaseStatus);
                blockedStatus.AllowClientDisconnectAfterRequest = true;
                blockedStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
                blockedStatus.CloseClientAfterResponseAndContinue = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    ResetStep(),
                    blockedStatus,
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    StopStep(true),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var resetOptions = new LMCAxisResetWaitOptions
                    {
                        TimeoutMilliseconds = 10000,
                        PollIntervalMilliseconds = 10,
                        StableSampleCount = 1
                    };
                    var reset = axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            resetOptions,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var oldSessionGeneration = reset.SessionGeneration;
                    var resume = axis
                        .ResumeResetWaitForStableErrorClearanceAsync(
                            reset,
                            resetOptions,
                            CancellationToken.None);
                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The held Reset status request was not written.");

                    connection.ConnectionStateChanged += (sender, args) =>
                    {
                        if (args.CurrentState == LMCConnectionState.Closing)
                        {
                            closeEntered.Set();
                        }
                    };
                    var close = connection.CloseConnectionAsync(
                        CancellationToken.None);
                    AssertEx.True(
                        closeEntered.Wait(2000),
                        "Concurrent Close did not enter the lifecycle owner state.");

                    var abortElapsed = Stopwatch.StartNew();
                    var abort =
                        connection.AbortTransportForSafetyPreemption(
                            oldSessionGeneration);
                    abortElapsed.Stop();
                    AssertEx.True(
                        abortElapsed.ElapsedMilliseconds < 1000,
                        "Concurrent Close blocked the safety transport abort.");
                    AssertEx.True(abort.TransportDetached);
                    AssertEx.True(abort.AbortiveLingerApplied);
                    AssertEx.False(abort.FaultStatePublished);
                    AssertEx.Equal(
                        LMCConnectionState.Closing,
                        abort.StateBefore);

                    releaseStatus.Set();
                    AssertEx.Throws<LMCAxisResetStatusException>(
                        () => resume.GetAwaiter().GetResult());
                    AssertEx.Throws<IOException>(
                        () => close.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCConnectionState.Disconnected,
                        connection.State);

                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask);
                    var freshAxis = new LMCAxis(connection, AxisName);
                    var stop = freshAxis
                        .BeginStopWaitForStableStandstillAsync(
                            Deceleration,
                            Jerk,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(stop.IsPending);
                    AssertEx.True(stop.Acknowledgement.IsSuccess);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                    AssertEx.Equal(1, CountCommand(server, 0x405D));
                    AssertEx.Equal(2, server.AcceptedClientCount);
                }
            }
        }

        private static void ReconnectPublicationIsAtomicAgainstPinnedAbort()
        {
            using (var sessionReserved = new ManualResetEventSlim(false))
            using (var releaseSessionReserved = new ManualResetEventSlim(false))
            using (var clientPublished = new ManualResetEventSlim(false))
            using (var releaseClientPublished = new ManualResetEventSlim(false))
            using (var publishAbortStarted = new ManualResetEventSlim(false))
            {
                var armReconnectHooks = 0;
                var firstClose = CloseStep();
                firstClose.CloseClientAfterResponseAndContinue = true;
                var options = new LMCConnectionOptions
                {
                    SessionReservedBeforeClientPublishObserver = () =>
                    {
                        if (Volatile.Read(ref armReconnectHooks) == 0)
                        {
                            return;
                        }

                        sessionReserved.Set();
                        AssertEx.True(
                            releaseSessionReserved.Wait(2000),
                            "The reserved-session publication hook was not released.");
                    },
                    ClientPublishedBeforeSessionBindObserver = () =>
                    {
                        if (Volatile.Read(ref armReconnectHooks) == 0)
                        {
                            return;
                        }

                        clientPublished.Set();
                        AssertEx.True(
                            releaseClientPublished.Wait(2000),
                            "The client/session binding hook was not released.");
                    }
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    ResetStep(),
                    firstClose,
                    InitStep(),
                    CallbackStep(),
                    CloseStep()))
                using (var connection = new LMCConnection(options))
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var reset = axis
                        .BeginResetWaitForStableErrorClearanceAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var oldSessionGeneration = reset.SessionGeneration;
                    connection.CloseConnection();
                    Volatile.Write(ref armReconnectHooks, 1);

                    var reconnect = connection.RpcInitConnectionAsync(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask,
                        CancellationToken.None);
                    AssertEx.True(
                        sessionReserved.Wait(2000),
                        "Reconnect did not reserve its session identity.");

                    var prePublishMismatch = AssertEx.Throws<
                        LMCSafetyPreemptionSessionMismatchException>(
                        () => connection
                            .AbortTransportForSafetyPreemption(
                                oldSessionGeneration));
                    AssertEx.Equal(
                        oldSessionGeneration,
                        prePublishMismatch.ExpectedSessionGeneration);
                    AssertEx.True(
                        prePublishMismatch.ObservedSessionGeneration
                            > oldSessionGeneration);

                    releaseSessionReserved.Set();
                    AssertEx.True(
                        clientPublished.Wait(2000),
                        "Reconnect did not enter atomic client/session publication.");
                    var publishAbort = Task.Run(() =>
                    {
                        publishAbortStarted.Set();
                        return connection.AbortTransportForSafetyPreemption(
                            oldSessionGeneration);
                    });
                    AssertEx.True(
                        publishAbortStarted.Wait(2000),
                        "Pinned abort did not start at the publication boundary.");
                    AssertEx.False(
                        publishAbort.Wait(100),
                        "Pinned abort bypassed atomic client/session publication.");

                    releaseClientPublished.Set();
                    var publishMismatch = AssertEx.Throws<
                        LMCSafetyPreemptionSessionMismatchException>(
                        () => publishAbort.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        oldSessionGeneration,
                        publishMismatch.ExpectedSessionGeneration);
                    AssertEx.True(
                        publishMismatch.ObservedSessionGeneration
                            > oldSessionGeneration);

                    reconnect.GetAwaiter().GetResult();
                    AssertEx.True(connection.IsConnected);
                    AssertEx.Equal(
                        publishMismatch.ObservedSessionGeneration,
                        connection.SessionGeneration);
                    connection.CloseConnection();

                    server.Verify();
                    AssertEx.Equal(2, server.AcceptedClientCount);
                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(2, CountCommand(server, 0x405D));
                }
            }
        }

        private static void RawValidNackRollsBackExactMutationGeneration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                RejectedAxisCommandStep(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk)),
                RejectedAxisCommandStep(
                    LMC_Frame.LMCAxisReset(AxisReference)),
                RejectedAxisCommandStep(
                    LMC_Frame.LMCAxisPower(AxisReference, false)),
                RejectedAxisCommandStep(
                    LMC_Frame.LMCAxisMoveAbsolute(
                        AxisReference,
                        MovePosition,
                        MoveVelocity,
                        MoveAcceleration,
                        MoveDeceleration,
                        MoveJerk,
                        LMC_DIRECTION.Shortest)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var coordinator = connection.GetAxisPowerOnWaitCoordinator(
                    axis.SessionGeneration,
                    axis.AxisReference);
                AssertEx.Equal(0L, coordinator.MutationGeneration);

                AssertEx.False(
                    axis.Stop(Deceleration, Jerk).IsSuccess);
                AssertEx.Equal(0L, coordinator.MutationGeneration);

                AssertEx.False(axis.ResetAsync(CancellationToken.None)
                    .GetAwaiter().GetResult().IsSuccess);
                AssertEx.Equal(0L, coordinator.MutationGeneration);

                AssertEx.False(axis.PowerOff().IsSuccess);
                AssertEx.Equal(0L, coordinator.MutationGeneration);

                AssertEx.False(axis.MoveAbsoluteExAsync(
                        MovePosition,
                        MoveVelocity,
                        MoveAcceleration,
                        MoveDeceleration,
                        MoveJerk,
                        LMC_DIRECTION.Shortest,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult()
                    .IsSuccess);
                AssertEx.Equal(0L, coordinator.MutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2022));
                AssertEx.Equal(1, CountCommand(server, 0x2024));
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void AssertAcceptedWaitInterference(
            LMCSingleAxis axis,
            LMCAxisStopWaitContinuation continuation)
        {
            var stopGeneration = continuation.StopMutationGeneration;
            var interference = AssertEx.Throws<
                LMCAxisStopInterferenceException>(
                () => axis.ResumeStopWaitForStableStandstillAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertStopInterference(
                interference,
                continuation,
                stopGeneration,
                stopGeneration + 1,
                0);
        }

        private static void AssertStopInterference(
            LMCAxisStopInterferenceException interference,
            LMCAxisStopWaitContinuation continuation,
            long expectedGeneration,
            long observedGeneration,
            int expectedStatusPollCount)
        {
            AssertEx.Equal(continuation, interference.Continuation);
            AssertEx.True(interference.Evidence.StopAccepted);
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

        private static LMC_Response SendMoveAbsolute(LMCSingleAxis axis)
        {
            return axis.MoveAbsoluteEx(
                MovePosition,
                MoveVelocity,
                MoveAcceleration,
                MoveDeceleration,
                MoveJerk,
                LMC_DIRECTION.Shortest);
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

        private static LMCAxisStopWaitOptions LongOptions()
        {
            return new LMCAxisStopWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCAxisStopWaitOptions DeadlineOptions()
        {
            return new LMCAxisStopWaitOptions
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

        private static FakeRpcStep StopStep(bool success)
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(
                    0,
                    success
                        ? TestFrame.Hex("00 00 00 00")
                        : TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk),
                    request)
            };
        }

        private static FakeRpcStep DelayedStopStep(
            ManualResetEventSlim stopReceived,
            ManualResetEventSlim releaseStop)
        {
            var step = StopStep(true);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk),
                    request);
                stopReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseStop.Wait(5000),
                "The delayed Stop response was not released.");
            return step;
        }

        private static FakeRpcStep ObservedStopStep(
            ManualResetEventSlim stopReceived)
        {
            var step = StopStep(true);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisStop(
                        AxisReference,
                        Deceleration,
                        Jerk),
                    request);
                stopReceived.Set();
            };
            return step;
        }

        private static FakeRpcStep DelayedStatusStep(
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus)
        {
            var step = StatusStep();
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReadStatus(AxisReference),
                    request);
                statusReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed status response was not released.");
            return step;
        }

        private static FakeRpcStep StatusStep(
            uint state = StandstillState,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort axisErrorId = 0,
            ushort statusWord = 0,
            Action afterRequest = null,
            ushort axisReference = AxisReference)
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
                    AssertEx.SequenceEqual(
                        LMC_Frame.LMCAxisReadStatus(axisReference),
                        request);
                    if (afterRequest != null)
                    {
                        afterRequest();
                    }
                }
            };
        }

        private static FakeRpcStep PowerOffStep()
        {
            return PowerOffStep(AxisReference);
        }

        private static FakeRpcStep PowerOffStep(ushort axisReference)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(axisReference, false),
                    request)
            };
        }

        private static FakeRpcStep PowerStep(bool enable)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, enable),
                    request)
            };
        }

        private static FakeRpcStep ResetStep()
        {
            return new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisReset(AxisReference),
                    request)
            };
        }

        private static FakeRpcStep RejectedAxisCommandStep(
            byte[] expectedRequest)
        {
            return new FakeRpcStep(
                TestFrame.ReadUInt16(expectedRequest, 0),
                TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    expectedRequest,
                    request)
            };
        }

        private static FakeRpcStep MoveAbsoluteStep(ushort axisReference)
        {
            return new FakeRpcStep(
                0x209F,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisMoveAbsolute(
                        axisReference,
                        MovePosition,
                        MoveVelocity,
                        MoveAcceleration,
                        MoveDeceleration,
                        MoveJerk,
                        LMC_DIRECTION.Shortest),
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
            int expectedStopCount,
            int expectedStatusCount)
        {
            AssertEx.Equal(
                expectedStopCount,
                CountCommand(server, 0x2022));
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

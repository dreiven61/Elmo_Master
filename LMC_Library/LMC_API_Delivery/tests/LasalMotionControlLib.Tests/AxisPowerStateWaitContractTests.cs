using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class AxisPowerStateWaitContractTests
    {
        private const string AxisName = "_LMCAxis1";
        private const ushort AxisReference = 1;
        private const string SecondAxisName = "_LMCAxis2";
        private const ushort SecondAxisReference = 2;
        private const int MovePosition = 1234;
        private const int MoveVelocity = 200;
        private const int MoveAcceleration = 300;
        private const int MoveDeceleration = 400;
        private const int MoveJerk = 5;
        private const uint PowerOn = 0x00000001u;
        private const uint Standstill = 0x02000000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "AxisPowerStateWait.AcceptedTimeout.ResumeIsStatusOnly",
                AcceptedTimeoutResumeIsStatusOnly);
            tests.Add(
                "AxisPowerStateWait.Stability.MismatchResetsProof",
                MismatchResetsProof);
            tests.Add(
                "AxisPowerStateWait.PowerOffProof.RequiresThreeAndReleases",
                PowerOffProofRequiresThreeAndReleases);
            tests.Add(
                "AxisPowerStateWait.Scope.ReconnectRejectsStaleContinuation",
                ReconnectRejectsStaleContinuation);
            tests.Add(
                "AxisPowerStateWait.PreWire.CancelIsZeroPowerWire",
                PreWireCancellationIsZeroPowerWire);
            tests.Add(
                "AxisPowerStateWait.PostWrite.CancelPublishesContinuationAndKeepsConnection",
                PostWriteCancellationPublishesContinuationAndKeepsConnection);
            tests.Add(
                "AxisPowerStateWait.Concurrency.ActivePollBlocksResolveAndQueuedCancelKeepsContinuation",
                ActivePollBlocksResolveAndQueuedCancelKeepsContinuation);
            tests.Add(
                "AxisPowerStateWait.AcceptedObserver.PrecedesPollingAndFailurePreservesContinuation",
                AcceptedObserverPrecedesPollingAndFailurePreservesContinuation);
            tests.Add(
                "AxisPowerStateWait.AcceptedObserver.SuccessOnceAndRejectedZero",
                AcceptedObserverSuccessOnceAndRejectedZero);
            tests.Add(
                "AxisPowerStateWait.Deadline.NoAckIsOutcomeUncertainAndFaulted",
                PowerOnNoAcknowledgementDeadlineIsUncertainAndFaulted);
            tests.Add(
                "AxisPowerStateWait.Deadline.NoStatusKeepsAcceptedContinuationAndFaults",
                PowerOnNoStatusDeadlineKeepsAcceptedContinuationAndFaults);
            tests.Add(
                "AxisPowerStateWait.PreWireCommit.CancelIsNotAttemptedAndReusable",
                PowerOnCommitWindowCancellationIsNotAttemptedAndReusable);
            tests.Add(
                "AxisPowerStateWait.SendPriority.AckResultDiscardIsOutcomeUncertain",
                PowerOnAcknowledgementResultDiscardIsOutcomeUncertain);
            tests.Add(
                "AxisPowerStateWait.SendPriority.StatusResultDiscardKeepsAcceptedContinuation",
                PowerOnStatusResultDiscardKeepsAcceptedContinuation);
            tests.Add(
                "AxisPowerStateWait.Publication.AcceptedGenerationAndContinuationAreAtomic",
                PowerOnAcceptedGenerationAndContinuationAreAtomic);
            tests.Add(
                "AxisPowerStateWait.Interference.AckParseMutationWaitsForGate",
                PowerOnMutationAfterAckParseBeforeContinuationPublication);
            tests.Add(
                "AxisPowerStateWait.Interference.SameReferenceHandleMoveIsZeroStatus",
                PowerOnSameReferenceHandleMoveInterferesBeforeResume);
            tests.Add(
                "AxisPowerStateWait.Interference.StatusRaceDiscardsResult",
                PowerOnMutationRacingStatusDiscardsResult);
            tests.Add(
                "AxisPowerStateWait.Interference.ZeroWireMutationDoesNotAdvance",
                PowerOnZeroWireMutationDoesNotAdvanceGeneration);
            tests.Add(
                "AxisPowerStateWait.Interference.DifferentAxisDoesNotAdvance",
                PowerOnDifferentAxisMutationDoesNotInterfere);
            tests.Add(
                "AxisPowerStateWait.Linearization.FinalCancelRetainsPending",
                PowerOnFinalPublicationCancellationRetainsPending);
            tests.Add(
                "AxisPowerStateWait.Linearization.FinalDeadlineRetainsPending",
                PowerOnFinalPublicationDeadlineRetainsPending);
            tests.Add(
                "AxisPowerStateWait.Race.FinalProofWinsLateCancellation",
                PowerOnFinalProofWinsLateCancellation);
            tests.Add(
                "AxisPowerStateWait.Race.FinalProofWinsLateDeadline",
                PowerOnFinalProofWinsLateDeadline);
            tests.Add(
                "AxisPowerStateWait.ReadOnly.NoStatusDeadlineInvalidatesTransport",
                ReadOnlyPowerStateNoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "AxisPowerOffWait.Success.OneCommandThenThreeStableStatuses",
                PowerOffSuccessSendsOneCommandThenThreeStableStatuses);
            tests.Add(
                "AxisPowerOffWait.Stability.MismatchResetsProof",
                PowerOffMismatchResetsStabilityProof);
            tests.Add(
                "AxisPowerOffWait.Stability.AxisErrorResetsWithoutStatusFailure",
                PowerOffAxisErrorResetsProofWithoutStatusFailure);
            tests.Add(
                "AxisPowerOffWait.Rejected.ZeroStatusPolls",
                PowerOffRejectedAcknowledgementHasZeroStatusPolls);
            tests.Add(
                "AxisPowerOffWait.PreWire.CancelIsZeroWire",
                PowerOffPreWireCancellationIsZeroWire);
            tests.Add(
                "AxisPowerOffWait.PreWireCommitCancel.IsNotAttempted",
                PowerOffCommitWindowCancellationIsNotAttempted);
            tests.Add(
                "AxisPowerOffWait.Timeout.NoAckInvalidatesTransport",
                PowerOffNoAcknowledgementDeadlineInvalidatesTransport);
            tests.Add(
                "AxisPowerOffWait.PostAck.TimeoutPreservesEvidence",
                PowerOffTimeoutPreservesAcceptedEvidence);
            tests.Add(
                "AxisPowerOffWait.PostAck.CancelPreservesEvidence",
                PowerOffCancellationPreservesAcceptedEvidence);
            tests.Add(
                "AxisPowerOffWait.Submission.ResponseLossIsUncertain",
                PowerOffResponseLossIsUncertainAndNotRetried);
            tests.Add(
                "AxisPowerOffWait.ReadFailure.ThrowsTypedStatus",
                PowerOffUnsuccessfulReadThrowsTypedStatusException);
            tests.Add(
                "AxisPowerOffWait.SendPriority.ResultDiscardIsUncertain",
                PowerOffSendPriorityResultDiscardIsUncertain);
            tests.Add(
                "AxisPowerOffWait.PendingPowerOn.AccumulatesButRemainsPending",
                PowerOffAccumulatesPendingPowerOnProofWithoutResolving);
            tests.Add(
                "AxisPowerOffWait.Split.BeginThenResumeIsStatusOnly",
                PowerOffSplitBeginThenResumeIsStatusOnly);
            tests.Add(
                "AxisPowerOffWait.AcceptedObserver.CompoundPrecedesStatus",
                PowerOffAcceptedObserverCompoundPrecedesStatus);
            tests.Add(
                "AxisPowerOffWait.AcceptedObserver.FailurePreservesPendingAndBlocksPower",
                PowerOffAcceptedObserverFailurePreservesPendingAndBlocksPower);
            tests.Add(
                "AxisPowerOffWait.AcceptedObserver.OperationCanceledIdentity",
                PowerOffAcceptedObserverOperationCanceledIdentity);
            tests.Add(
                "AxisPowerOffWait.AcceptedObserver.RejectAndResponseLossAreZero",
                PowerOffAcceptedObserverRejectAndResponseLossAreZero);
            tests.Add(
                "AxisPowerOffWait.AcceptedObserver.CancelAndDeadlinePreserveEvidence",
                PowerOffAcceptedObserverCancelAndDeadlinePreserveEvidence);
            tests.Add(
                "AxisPowerOffWait.Split.TimeoutResumeDoesNotReplay",
                PowerOffSplitTimeoutResumeDoesNotReplay);
            tests.Add(
                "AxisPowerOffWait.Split.NoStatusInvalidatesTransport",
                PowerOffSplitNoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "AxisPowerOffWait.Split.NewAcceptedPowerOffSupersedesOldContinuation",
                PowerOffSplitNewAcceptedPowerOffSupersedesOldContinuation);
            tests.Add(
                "AxisPowerOffWait.Split.PriorityPreemptsPollAndNewPowerOffCompletes",
                PowerOffSplitPriorityPreemptsPollAndNewPowerOffCompletes);
            tests.Add(
                "AxisPowerOffWait.Split.ConcurrentBeginPreservesWirePublicationOrder",
                PowerOffSplitConcurrentBeginPreservesWirePublicationOrder);
            tests.Add(
                "AxisPowerOffWait.Split.TimeoutResetsPendingPowerOnOffProof",
                PowerOffSplitTimeoutResetsPendingPowerOnOffProof);
            tests.Add(
                "AxisPowerOffWait.Split.ConcurrentResumeSecondIsZeroWire",
                PowerOffSplitConcurrentResumeSecondIsZeroWire);
            tests.Add(
                "AxisPowerOffWait.Split.StaleSessionResumeIsZeroWire",
                PowerOffSplitStaleSessionResumeIsZeroWire);
            tests.Add(
                "AxisPowerOffWait.Interference.SameReferenceHandleMoveIsZeroStatus",
                PowerOffSameReferenceHandleMoveInterferesBeforeResume);
            tests.Add(
                "AxisPowerOffWait.Interference.StatusRaceDiscardsResult",
                PowerOffMutationRacingStatusDiscardsResult);
            tests.Add(
                "AxisPowerOffWait.Interference.ZeroWireMutationDoesNotAdvance",
                PowerOffZeroWireMutationDoesNotAdvanceGeneration);
            tests.Add(
                "AxisPowerOffWait.Interference.DifferentAxisDoesNotAdvance",
                PowerOffDifferentAxisMutationDoesNotInterfere);
            tests.Add(
                "AxisPowerOffWait.Linearization.FinalCancelRetainsPending",
                PowerOffFinalPublicationCancellationRetainsPending);
            tests.Add(
                "AxisPowerOffWait.Linearization.FinalDeadlineRetainsPending",
                PowerOffFinalPublicationDeadlineRetainsPending);
            tests.Add(
                "AxisPowerOffWait.Split.AcceptedPublicationCancelPreservesContinuation",
                PowerOffAcceptedPublicationCancellationPreservesContinuation);
            tests.Add(
                "AxisPowerOffWait.Split.AcceptedPublicationDeadlinePreservesContinuation",
                PowerOffAcceptedPublicationDeadlinePreservesContinuation);
            tests.Add(
                "AxisPowerOffWait.Race.NonfinalSupersedeCannotComplete",
                PowerOffNonfinalSupersedeCannotCompleteOldWait);
            tests.Add(
                "AxisPowerOffWait.Submission.RejectedPreservesPriorPending",
                PowerOffRejectedBeginPreservesPriorPending);
            tests.Add(
                "AxisPowerOffWait.Submission.ResponseLossPreservesPriorButInvalidatesGeneration",
                PowerOffResponseLossPreservesPriorButInvalidatesGeneration);
            tests.Add(
                "AxisPowerOffWait.Race.FinalProofWinsLateCancellation",
                PowerOffFinalProofWinsLateCancellation);
            tests.Add(
                "AxisPowerOffWait.Race.FinalProofWinsLateDeadline",
                PowerOffFinalProofWinsLateDeadline);
        }

        private static void AcceptedObserverSuccessOnceAndRejectedZero()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var observerCount = 0;
                LMCAxisPowerOnWaitContinuation observed = null;
                var result = axis.PowerOnAndWaitForStableStateAsync(
                        new LMCAxisPowerStateWaitOptions
                        {
                            PollIntervalMilliseconds = 1
                        },
                        continuation =>
                        {
                            observerCount++;
                            observed = continuation;
                            AssertEx.True(continuation.IsPending);
                            AssertEx.Equal(0, CountCommand(server, 0x2028));
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCount);
                AssertEx.NotNull(observed);
                AssertEx.False(observed.IsPending);
                AssertEx.Equal(observed, result.Continuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(3, CountCommand(server, 0x2028));
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                RejectedPowerOnStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var observerCount = 0;
                AssertEx.Throws<LMCAxisPowerOnRejectedException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions(),
                            continuation => observerCount++,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(0, observerCount);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    axis.PendingPowerOnWaitContinuation);
                AssertEx.Equal(0, CountCommand(server, 0x2028));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void
            AcceptedObserverPrecedesPollingAndFailurePreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                LMCAxisPowerOnWaitContinuation observed = null;
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions(),
                            continuation =>
                            {
                                observed = continuation;
                                AssertEx.Equal(
                                    continuation,
                                    axis.PendingPowerOnWaitContinuation);
                                var mutationGateAvailable = continuation
                                    .Coordinator.MutationGate.Wait(0);
                                AssertEx.True(
                                    mutationGateAvailable,
                                    "The accepted observer ran while the Axis Power mutation gate was still held.");
                                if (mutationGateAvailable)
                                {
                                    continuation.Coordinator.MutationGate
                                        .Release();
                                }
                                AssertEx.Equal(
                                    0,
                                    CountCommand(server, 0x2028),
                                    "Status polling started before the accepted observer returned.");
                                AssertEx.Throws<
                                    LMCAxisPowerOnPendingException>(
                                    () => axis.PowerOn());
                                AssertEx.Throws<InvalidOperationException>(
                                    () => axis
                                        .ResumePowerOnWaitForStableStateAsync(
                                            continuation,
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult());
                                throw new InvalidOperationException(
                                    "durable observer failure");
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal("durable observer failure", error.Message);
                AssertEx.NotNull(observed);
                AssertEx.True(observed.IsPending);
                AssertEx.Equal(
                    observed,
                    axis.PendingPowerOnWaitContinuation);
                AssertEx.Equal(0, CountCommand(server, 0x2028));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
            }
        }

        private static void AcceptedTimeoutResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
                            {
                                TimeoutMilliseconds = 25,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.NotNull(timeout.Continuation);
                AssertEx.True(timeout.Continuation.IsPending);
                AssertEx.Throws<LMCAxisPowerOnPendingException>(
                    () => axis.PowerOn());
                AssertEx.Throws<LMCAxisPowerOnPendingException>(
                    () => axis.PowerOnAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                var secondTime = new FakeWaitTime();
                var resumed = axis.ResumePowerOnWaitForStableStateAsync(
                        timeout.Continuation,
                        LongOptions(),
                        CancellationToken.None,
                        secondTime.ElapsedMilliseconds,
                        secondTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                AssertEx.False(timeout.Continuation.IsPending);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(
                    1,
                    CountCommand(server, 0x2023));
                AssertEx.Equal(
                    6,
                    CountCommand(server, 0x2028));
            }
        }

        private static void MismatchResetsProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.PowerOnAndWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.PollCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.True(result.FinalStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PowerOffProofRequiresThreeAndReleases()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(Standstill),
                StatusStep(Standstill),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                PowerStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
                            {
                                TimeoutMilliseconds = 15,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                var pending = timeout.Continuation;

                var powerOff = axis.PowerOff();
                AssertEx.True(powerOff.IsSuccess);

                var secondTime = new FakeWaitTime();
                var proof = axis.WaitForPowerStateAsync(
                        false,
                        LongOptions(),
                        CancellationToken.None,
                        secondTime.ElapsedMilliseconds,
                        secondTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(5, proof.PollCount);
                AssertEx.Equal(3, proof.StableSampleCount);
                AssertEx.False(proof.ReusedAcceptedAcknowledgement);
                AssertEx.Equal<LMC_Response>(null, proof.Acknowledgement);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    proof.Continuation);
                AssertEx.Equal(
                    LMCAxisPowerOnSubmissionOutcome.NotAttempted,
                    proof.Evidence.SubmissionOutcome);

                axis.ResolvePowerOnWaitAfterStablePowerOff(pending);
                AssertEx.False(pending.IsPending);
                AssertEx.True(axis.PowerOn().IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(3, CountCommand(server, 0x2023));
            }
        }

        private static void ReconnectRejectsStaleContinuation()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(Standstill),
                StatusStep(Standstill)))
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
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => oldAxis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
                            {
                                TimeoutMilliseconds = 15,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var newAxis = new LMCAxis(connection, AxisName);
                AssertEx.Throws<InvalidOperationException>(
                    () => newAxis.ResumePowerOnWaitForStableStateAsync(
                            timeout.Continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertEx.Equal(0, CountCommand(secondServer, 0x2023));
                AssertEx.Equal(0, CountCommand(secondServer, 0x2028));
            }
        }

        private static void PreWireCancellationIsZeroPowerWire()
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
                var observerCount = 0;
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions(),
                            continuation => observerCount++,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, observerCount);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x2023));
            }
        }

        private static void
            PostWriteCancellationPublishesContinuationAndKeepsConnection()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var power = PowerStep(true);
                power.InspectRequest = request =>
                {
                    AssertEx.Equal((byte)1, request[12]);
                    cancellation.Cancel();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    power,
                    StatusStep(PowerOn | Standstill),
                    StatusStep(PowerOn | Standstill),
                    StatusStep(PowerOn | Standstill),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var observerCount = 0;
                    LMCAxisPowerOnWaitContinuation observed = null;
                    var canceled = AssertEx.Throws<
                        LMCAxisPowerStateWaitCanceledException>(
                            () => axis.PowerOnAndWaitForStableStateAsync(
                                    new LMCAxisPowerStateWaitOptions(),
                                    continuation =>
                                    {
                                        observerCount++;
                                        observed = continuation;
                                    },
                                    cancellation.Token)
                                .GetAwaiter()
                                .GetResult());

                    AssertEx.Equal(1, observerCount);
                    AssertEx.Equal(observed, canceled.Continuation);
                    AssertEx.NotNull(canceled.Continuation);
                    AssertEx.True(canceled.Continuation.IsPending);
                    AssertEx.Equal(
                        canceled.Continuation,
                        axis.PendingPowerOnWaitContinuation);
                    AssertEx.Throws<LMCAxisPowerOnPendingException>(
                        () => axis.PowerOn());

                    var resumeTime = new FakeWaitTime();
                    var resumed = axis.ResumePowerOnWaitForStableStateAsync(
                            canceled.Continuation,
                            LongOptions(),
                            CancellationToken.None,
                            resumeTime.ElapsedMilliseconds,
                            resumeTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                    AssertEx.False(canceled.Continuation.IsPending);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                }
            }
        }

        private static void
            ActivePollBlocksResolveAndQueuedCancelKeepsContinuation()
        {
            using (var requestObserved = new ManualResetEvent(false))
            using (var queuedCancellation = new CancellationTokenSource())
            {
                var delayedStatus = StatusStep(PowerOn | Standstill);
                delayedStatus.InspectRequest = request =>
                    requestObserved.Set();
                delayedStatus.ResponseDelayMilliseconds = 250;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    PowerStep(true),
                    StatusStep(Standstill),
                    StatusStep(Standstill),
                    StatusStep(Standstill),
                    StatusStep(Standstill),
                    delayedStatus,
                    StatusStep(PowerOn | Standstill),
                    StatusStep(PowerOn | Standstill),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var firstTime = new FakeWaitTime();
                    var timeout = AssertEx.Throws<
                        LMCAxisPowerStateWaitTimeoutException>(
                        () => axis.PowerOnAndWaitForStableStateAsync(
                                new LMCAxisPowerStateWaitOptions
                                {
                                    TimeoutMilliseconds = 1,
                                    PollIntervalMilliseconds = 1,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None,
                                firstTime.ElapsedMilliseconds,
                                firstTime.DelayAsync)
                            .GetAwaiter()
                            .GetResult());
                    var continuation = timeout.Continuation;

                    AssertEx.True(axis.ReadStatusResult().IsReadSuccessful);
                    AssertEx.True(axis.ReadStatusResult().IsReadSuccessful);
                    AssertEx.True(axis.ReadStatusResult().IsReadSuccessful);
                    AssertEx.Equal(
                        3,
                        continuation.StablePowerOffStandstillSampleCount);

                    var activeResume = axis
                        .ResumePowerOnWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None);
                    AssertEx.True(
                        requestObserved.WaitOne(1000),
                        "The active resume did not begin its status request.");

                    AssertEx.Throws<InvalidOperationException>(
                        () => axis.ResolvePowerOnWaitAfterStablePowerOff(
                            continuation));

                    queuedCancellation.Cancel();
                    var queuedCanceled = AssertEx.Throws<
                        LMCAxisPowerStateWaitCanceledException>(
                        () => axis.ResumePowerOnWaitForStableStateAsync(
                                continuation,
                                queuedCancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(
                        continuation,
                        queuedCanceled.Continuation);
                    AssertEx.True(continuation.IsPending);

                    var completed = activeResume.GetAwaiter().GetResult();
                    AssertEx.False(completed.Continuation.IsPending);
                    AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                        null,
                        axis.PendingPowerOnWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                }
            }
        }

        private static void
            PowerOnNoAcknowledgementDeadlineIsUncertainAndFaulted()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedPowerOn = PowerStep(true);
                blockedPowerOn.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Power On response was not released.");
                blockedPowerOn.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedPowerOn))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var observerCount = 0;
                    LMCAxisPowerStateWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisPowerStateWaitTimeoutException>(
                            () => axis.PowerOnAndWaitForStableStateAsync(
                                    DeadlineOptions(),
                                    continuation => observerCount++,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(
                        LMCAxisPowerOnSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMC_Response>(
                        null,
                        error.Evidence.PowerOnAcknowledgement);
                    AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                        null,
                        error.Continuation);
                    AssertEx.Equal(0, observerCount);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                    AssertEx.Equal(0, CountCommand(server, 0x2028));
                }
            }
        }

        private static void
            PowerOnNoStatusDeadlineKeepsAcceptedContinuationAndFaults()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(PowerOn | Standstill);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Power On status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    PowerStep(true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var observerCount = 0;
                    LMCAxisPowerOnWaitContinuation observed = null;
                    LMCAxisPowerStateWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisPowerStateWaitTimeoutException>(
                            () => axis.PowerOnAndWaitForStableStateAsync(
                                    DeadlineOptions(),
                                    continuation =>
                                    {
                                        observerCount++;
                                        observed = continuation;
                                    },
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(1, observerCount);
                    AssertEx.Equal(observed, error.Continuation);
                    AssertEx.Equal(
                        LMCAxisPowerOnSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.PowerOnAccepted);
                    AssertEx.True(error.Acknowledgement.IsSuccess);
                    AssertEx.Equal(0, error.PollCount);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.True(error.Continuation.IsPending);
                    AssertEx.Equal(
                        error.Continuation,
                        axis.PendingPowerOnWaitContinuation);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                }
            }
        }

        private static void
            PowerOnCommitWindowCancellationIsNotAttemptedAndReusable()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerStateWaitCanceledException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOnSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    error.Continuation);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);
                AssertEx.Equal(0, CountCommand(server, 0x2023));

                AssertEx.True(axis.ReadStatusResult().IsReadSuccessful);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOnAcknowledgementResultDiscardIsOutcomeUncertain()
        {
            const string powerOnOperation = "Axis Power On accepted-once";
            const string stopOperation = "Priority Axis Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var powerOnReceived = new ManualResetEventSlim(false))
            using (var releasePowerOn = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                DelayedPowerOnStep(powerOnReceived, releasePowerOn),
                StopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var observerCount = 0;
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var powerOn = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                powerOnOperation))
                            {
                                return axis
                                    .PowerOnAndWaitForStableStateAsync(
                                        new LMCAxisPowerStateWaitOptions(),
                                        continuation => observerCount++,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        powerOnReceived.Wait(2000),
                        "The Power On request did not reach the server.");
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

                    releasePowerOn.Set();
                    var error = AssertEx.Throws<
                        LMCAxisPowerOnSubmissionException>(
                        () => powerOn.GetAwaiter().GetResult());
                    var stopResponse = stop.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        LMCAxisPowerOnSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.Equal(
                        LMCSendPreemptionPhase.ResultDiscarded,
                        ((LMCSendPreemptedException)error.InnerException).Phase);
                    AssertEx.Equal(0, observerCount);
                    AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                        null,
                        axis.PendingPowerOnWaitContinuation);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                }
                finally
                {
                    releasePowerOn.Set();
                }
            }
        }

        private static void
            PowerOnStatusResultDiscardKeepsAcceptedContinuation()
        {
            const string powerOnOperation = "Axis Power On verification";
            const string stopOperation = "Priority Axis Stop";
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
                PowerStep(true),
                DelayedStatusStep(
                    PowerOn | Standstill,
                    statusReceived,
                    releaseStatus),
                StopStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var observerCount = 0;
                    LMCAxisPowerOnWaitContinuation observed = null;
                    var expectedGeneration = coordinator.CurrentGeneration;
                    var powerOn = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                powerOnOperation))
                            {
                                return axis
                                    .PowerOnAndWaitForStableStateAsync(
                                        new LMCAxisPowerStateWaitOptions(),
                                        continuation =>
                                        {
                                            observerCount++;
                                            observed = continuation;
                                        },
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The Power On status request did not reach the server.");
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

                    releaseStatus.Set();
                    var error = AssertEx.Throws<
                        LMCAxisPowerStateStatusException>(
                        () => powerOn.GetAwaiter().GetResult());
                    var stopResponse = stop.GetAwaiter().GetResult();

                    AssertEx.Equal(1, observerCount);
                    AssertEx.Equal(observed, error.Continuation);
                    AssertEx.Equal(
                        LMCAxisPowerOnSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.Equal(
                        LMCSendPreemptionPhase.ResultDiscarded,
                        ((LMCSendPreemptedException)error.InnerException).Phase);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);
                    AssertEx.True(observed.IsPending);
                    AssertEx.Equal(
                        observed,
                        axis.PendingPowerOnWaitContinuation);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2023));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                }
                finally
                {
                    releaseStatus.Set();
                }
            }
        }

        private static void
            ReadOnlyPowerStateNoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(PowerOn | Standstill);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked read-only Power State response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisPowerStateWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisPowerStateWaitTimeoutException>(
                            () => axis.WaitForPowerStateAsync(
                                    true,
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
                        LMCAxisPowerOnSubmissionOutcome.NotAttempted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertEx.Equal(0, CountCommand(server, 0x2023));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                }
            }
        }

        private static void
            PowerOffSuccessSendsOneCommandThenThreeStableStatuses()
        {
            var defaults = new LMCAxisPowerStateWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.PowerOffAndWaitForStableStateAsync(
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.Evidence.CommandMayHaveBeenSent);
                AssertEx.True(result.PowerOffAccepted);
                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.NotNull(result.FinalStatus);
                AssertEx.True(result.FinalStatus.IsReadSuccessful);
                AssertEx.False(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsStandstill);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(
                    3,
                    result.StablePowerOffStandstillSampleCount);
                AssertEx.Equal(3, result.RequiredStableSampleCount);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
            }
        }

        private static void PowerOffMismatchResetsStabilityProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.PowerOffAndWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(
                    3,
                    result.StablePowerOffStandstillSampleCount);
                AssertEx.False(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsStandstill);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 6);
            }
        }

        private static void
            PowerOffAxisErrorResetsProofWithoutStatusFailure()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                PowerOffStatusStep(
                    Standstill,
                    axisErrorId: 7),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var result = axis.PowerOffAndWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(
                    3,
                    result.StablePowerOffStandstillSampleCount);
                AssertEx.False(result.FinalStatus.HasAxisError);
                AssertEx.True(result.FinalStatus.IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 6);
            }
        }

        private static void
            PowerOffRejectedAcknowledgementHasZeroStatusPolls()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                RejectedPowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisPowerOffRejectedException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Rejected,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.NotNull(error.Acknowledgement);
                AssertEx.False(error.Acknowledgement.IsSuccess);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void PowerOffPreWireCancellationIsZeroWire()
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
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.PowerOffAcknowledgement);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 0, 0);
            }
        }

        private static void
            PowerOffCommitWindowCancellationIsNotAttempted()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.NotAttempted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.False(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.False(
                    error.Evidence.TransportInvalidatedAtDeadline);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.PowerOffAcknowledgement);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    error.Continuation);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsReadSuccessful);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 0, 1);
            }
        }

        private static void
            PowerOffNoAcknowledgementDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedPowerOff = PowerStep(false);
                blockedPowerOff.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Power Off response was not released.");
                blockedPowerOff.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    blockedPowerOff))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    LMCAxisPowerOffWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisPowerOffWaitTimeoutException>(
                            () => axis
                                .BeginPowerOffWaitForStableStateAsync(
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
                        LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMC_Response>(
                        null,
                        error.Acknowledgement);
                    AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                        null,
                        error.Continuation);
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
                    AssertPowerOffCommandCounts(server, 1, 0);
                }
            }
        }

        private static void PowerOffTimeoutPreservesAcceptedEvidence()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            new LMCAxisPowerStateWaitOptions
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
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.PowerOffAccepted);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.IsPowerOn);
                AssertEx.Equal(1, error.StatusPollCount);
                AssertEx.Equal(
                    0,
                    error.Evidence.StablePowerOffStandstillSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 1);
            }
        }

        private static void PowerOffCancellationPreservesAcceptedEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(
                    Standstill,
                    afterRequest: cancellation.Cancel),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            LongOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Acknowledgement.IsSuccess);
                AssertEx.NotNull(error.LastObservedStatus);
                AssertEx.True(error.LastObservedStatus.IsReadSuccessful);
                AssertEx.Equal(1, error.StatusPollCount);
                AssertEx.Equal(
                    1,
                    error.Evidence.StablePowerOffStandstillSampleCount);

                var reused = axis.ReadStatusResult();
                AssertEx.True(reused.IsReadSuccessful);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 2);
            }
        }

        private static void
            PowerOffResponseLossIsUncertainAndNotRetried()
        {
            var responseLoss = new FakeRpcStep(0x2023, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
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
                    LMCAxisPowerOffSubmissionException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain,
                    error.Evidence.SubmissionOutcome);
                AssertEx.True(error.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    error.Evidence.PowerOffAcknowledgement);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);

                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void
            PowerOffUnsuccessfulReadThrowsTypedStatusException()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                PowerOffStatusStep(
                    Standstill,
                    functionStatus: 0x0010,
                    errorId: -31),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerOffStatusException>(
                    () => axis.PowerOffAndWaitForStableStateAsync(
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(error.Evidence.PowerOffAccepted);
                AssertEx.NotNull(error.FailedStatus);
                AssertEx.True(ReferenceEquals(
                    error.FailedStatus,
                    error.Evidence.LastObservedStatus));
                AssertEx.False(error.FailedStatus.IsReadSuccessful);
                AssertEx.Equal(2, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    0,
                    error.Evidence.StablePowerOffStandstillSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 2);
            }
        }

        private static void PowerOffSendPriorityResultDiscardIsUncertain()
        {
            const string powerOffOperation = "Axis Power Off completion";
            const string stopOperation = "Priority Axis Stop";
            var coordinator = new LMCSendPriorityCoordinator();
            var options = new LMCConnectionOptions
            {
                SendPriorityCoordinator = coordinator
            };

            using (var powerOffReceived = new ManualResetEventSlim(false))
            using (var releasePowerOff = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                DelayedPowerOffStep(
                    powerOffReceived,
                    releasePowerOff),
                StopStep(),
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
                    var powerOff = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPreemptibleScope(
                                expectedGeneration,
                                powerOffOperation))
                            {
                                return axis
                                    .PowerOffAndWaitForStableStateAsync(
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });

                    AssertEx.True(
                        powerOffReceived.Wait(2000),
                        "The Power Off request did not reach the server.");

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

                    releasePowerOff.Set();
                    var error = AssertEx.Throws<
                        LMCAxisPowerOffSubmissionException>(
                        () => powerOff.GetAwaiter().GetResult());
                    var stopResponse = stop.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(
                        error.InnerException is LMCSendPreemptedException);
                    AssertEx.True(stopResponse.IsSuccess);
                    AssertEx.Equal(0, error.Evidence.StatusPollCount);

                    connection.CloseConnection();
                    server.Verify();
                    AssertPowerOffCommandCounts(server, 1, 0);
                }
                finally
                {
                    releasePowerOff.Set();
                }
            }
        }

        private static void
            PowerOffAccumulatesPendingPowerOnProofWithoutResolving()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var firstTime = new FakeWaitTime();
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
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                var continuation = pendingError.Continuation;

                AssertEx.NotNull(continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                var powerOffTime = new FakeWaitTime();
                var result = axis.PowerOffAndWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        powerOffTime.ElapsedMilliseconds,
                        powerOffTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.PowerOffAccepted);
                AssertEx.Equal(
                    3,
                    continuation.StablePowerOffStandstillSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(2, CountCommand(server, 0x2023));
                AssertEx.Equal(4, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOffAcceptedObserverCompoundPrecedesStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                LMCAxisPowerOffWaitContinuation observed = null;
                var observerCount = 0;
                var options = SingleSamplePowerOffOptions(1000);
                var result = axis.PowerOffAndWaitForStableStateAsync(
                        options,
                        continuation =>
                        {
                            observerCount++;
                            observed = continuation;
                            AssertEx.True(continuation.IsPending);
                            AssertEx.Equal(
                                continuation,
                                axis.PendingPowerOffWaitContinuation);
                            AssertEx.True(continuation.Coordinator
                                .PowerOffAcceptanceObserverInProgress);
                            AssertEx.Equal(1,
                                CountAxisPowerCommand(server, false));
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2028));

                            var mutationGateAvailable = continuation
                                .Coordinator.MutationGate.Wait(0);
                            AssertEx.True(
                                mutationGateAvailable,
                                "The accepted Axis Power Off observer ran while the mutation gate was held.");
                            if (mutationGateAvailable)
                            {
                                continuation.Coordinator.MutationGate
                                    .Release();
                            }

                            options.StableSampleCount = 2;
                            options.PollIntervalMilliseconds = 0;
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCount);
                AssertEx.Equal(observed, result.Continuation);
                AssertEx.Equal(1, result.RequiredStableSampleCount);
                AssertEx.True(observed.IsCompleted);
                AssertEx.False(observed.Coordinator
                    .PowerOffAcceptanceObserverInProgress);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 1);
            }
        }

        private static void
            PowerOffAcceptedObserverFailurePreservesPendingAndBlocksPower()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var observerFailure = new InvalidOperationException(
                    "durable Axis Power Off observer failure");
                LMCAxisPowerOffWaitContinuation observed = null;
                var actual = AssertEx.Throws<InvalidOperationException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            continuation =>
                            {
                                observed = continuation;
                                AssertEx.True(continuation.IsPending);
                                AssertEx.Throws<InvalidOperationException>(
                                    () => axis
                                        .ResumePowerOffWaitForStableStateAsync(
                                            continuation,
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult());
                                AssertEx.Throws<InvalidOperationException>(
                                    () => axis.PowerOn());
                                AssertEx.Throws<InvalidOperationException>(
                                    () => axis.PowerOff());
                                AssertEx.Throws<InvalidOperationException>(
                                    () => axis.PowerOffAsync(
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult());
                                AssertEx.Throws<
                                    LMCAxisPowerOffSubmissionException>(
                                    () => axis
                                        .BeginPowerOffWaitForStableStateAsync(
                                            CancellationToken.None)
                                        .GetAwaiter()
                                        .GetResult());
                                AssertEx.Equal(1,
                                    CountAxisPowerCommand(server, false));
                                AssertEx.Equal(0,
                                    CountAxisPowerCommand(server, true));
                                AssertEx.Equal(0,
                                    CountCommand(server, 0x2028));
                                throw observerFailure;
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(observerFailure, actual));
                AssertEx.NotNull(observed);
                AssertEx.True(observed.IsPending);
                AssertEx.Equal(
                    observed,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.False(observed.Coordinator
                    .PowerOffAcceptanceObserverInProgress);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void
            PowerOffAcceptedObserverOperationCanceledIdentity()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var observerFailure = new OperationCanceledException(
                    "durable observer cancellation");
                LMCAxisPowerOffWaitContinuation observed = null;
                var actual = AssertEx.Throws<OperationCanceledException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            continuation =>
                            {
                                observed = continuation;
                                throw observerFailure;
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(observerFailure, actual));
                AssertEx.True(observed.IsPending);
                AssertEx.Equal(
                    observed,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.False(observed.Coordinator
                    .PowerOffAcceptanceObserverInProgress);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void
            PowerOffAcceptedObserverRejectAndResponseLossAreZero()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                RejectedPowerOffStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var observerCount = 0;
                AssertEx.Throws<LMCAxisPowerOffRejectedException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            continuation => observerCount++,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(0, observerCount);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }

            var responseLoss = new FakeRpcStep(0x2023, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
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
                var observerCount = 0;
                var uncertain = AssertEx.Throws<
                    LMCAxisPowerOffSubmissionException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            continuation => observerCount++,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(0, observerCount);
                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain,
                    uncertain.Evidence.SubmissionOutcome);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void
            PowerOffAcceptedObserverCancelAndDeadlinePreserveEvidence()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                LMCAxisPowerOffWaitContinuation observed = null;
                var canceled = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel,
                            null,
                            continuation => observed = continuation)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.NotNull(observed);
                AssertEx.Equal(observed, canceled.Continuation);
                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.True(canceled.Acknowledgement.IsSuccess);
                AssertEx.True(canceled.Evidence
                    .PowerOffMutationGeneration > 0);
                AssertEx.True(observed.IsPending);
                AssertEx.Equal(
                    observed,
                    axis.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = LongOptions();
                var time = new FakeWaitTime();
                LMCAxisPowerOffWaitContinuation observed = null;
                var timeout = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.Advance(
                                options.TimeoutMilliseconds),
                            null,
                            continuation => observed = continuation)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.NotNull(observed);
                AssertEx.Equal(observed, timeout.Continuation);
                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    timeout.Evidence.SubmissionOutcome);
                AssertEx.True(timeout.Acknowledgement.IsSuccess);
                AssertEx.Equal(
                    options.TimeoutMilliseconds,
                    timeout.Evidence.ElapsedMilliseconds);
                AssertEx.True(observed.IsPending);
                AssertEx.Equal(
                    observed,
                    axis.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void PowerOffSplitBeginThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
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
                    axis.PendingPowerOffWaitContinuation);
                AssertPowerOffCommandCounts(server, 1, 0);

                var resumeTime = new FakeWaitTime();
                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(3, result.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
            }
        }

        private static void PowerOffSplitTimeoutResumeDoesNotReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var beginTime = new FakeWaitTime();
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        beginTime.ElapsedMilliseconds,
                        beginTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                var firstTime = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            new LMCAxisPowerStateWaitOptions
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
                    timeout.Evidence.StablePowerOffStandstillSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertPowerOffCommandCounts(server, 1, 1);

                var resumeTime = new FakeWaitTime();
                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        continuation,
                        LongOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(4, result.StatusPollCount);
                AssertEx.Equal(3, result.StablePowerOffStandstillSampleCount);
                AssertEx.False(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 4);
            }
        }

        private static void
            PowerOffSplitNoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(Standstill);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Power Off status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    AxisInfoStep(),
                    PowerStep(false),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var continuation = axis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    LMCAxisPowerOffWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCAxisPowerOffWaitTimeoutException>(
                            () => axis
                                .ResumePowerOffWaitForStableStateAsync(
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

                    AssertEx.Equal(
                        LMCAxisPowerOffSubmissionOutcome.Accepted,
                        error.Evidence.SubmissionOutcome);
                    AssertEx.True(error.Evidence.PowerOffAccepted);
                    AssertEx.True(error.Acknowledgement.IsSuccess);
                    AssertEx.Equal<LMCReadStatusResult>(
                        null,
                        error.LastObservedStatus);
                    AssertEx.Equal(0, error.StatusPollCount);
                    AssertEx.True(
                        error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(continuation, error.Continuation);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        continuation,
                        axis.PendingPowerOffWaitContinuation);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertPowerOffCommandCounts(server, 1, 1);
                }
            }
        }

        private static void
            PowerOffSplitNewAcceptedPowerOffSupersedesOldContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis.BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var second = axis.BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(first.IsSuperseded);
                AssertEx.False(first.IsPending);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(
                    second,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertPowerOffCommandCounts(server, 2, 0);

                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        second,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(second, result.Continuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 2, 3);
            }
        }

        private static void
            PowerOffSplitPriorityPreemptsPollAndNewPowerOffCompletes()
        {
            const string firstMonitorOperation =
                "First Axis Power Off verification";
            const string secondPowerOffOperation =
                "Second Axis Power Off";
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
                PowerStep(false),
                DelayedStatusStep(
                    Standstill,
                    statusReceived,
                    releaseStatus),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection(connectionOptions))
            {
                try
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var firstGeneration = coordinator.ReservePrioritySend();
                    LMCAxisPowerOffWaitContinuation first;
                    using (coordinator.BeginPriorityScope(
                        firstGeneration,
                        "First Axis Power Off"))
                    {
                        first = axis.BeginPowerOffWaitForStableStateAsync(
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
                                    .ResumePowerOffWaitForStableStateAsync(
                                        first,
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    AssertEx.True(
                        statusReceived.Wait(2000),
                        "The first split Power Off status poll did not reach the server.");

                    var secondGeneration = coordinator.ReservePrioritySend();
                    var secondBegin = Task.Run(
                        () =>
                        {
                            using (coordinator.BeginPriorityScope(
                                secondGeneration,
                                secondPowerOffOperation))
                            {
                                return axis
                                    .BeginPowerOffWaitForStableStateAsync(
                                        CancellationToken.None)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                        });
                    releaseStatus.Set();

                    var preempted = AssertEx.Throws<
                        LMCAxisPowerOffStatusException>(
                        () => firstMonitor.GetAwaiter().GetResult());
                    AssertEx.True(
                        preempted.InnerException
                            is LMCSendPreemptedException);
                    var second = secondBegin.GetAwaiter().GetResult();
                    AssertEx.True(first.IsSuperseded);
                    AssertEx.True(second.IsPending);

                    LMCAxisPowerOffWaitResult result;
                    using (coordinator.BeginPreemptibleScope(
                        secondGeneration,
                        "Second Axis Power Off verification"))
                    {
                        result = axis.ResumePowerOffWaitForStableStateAsync(
                                second,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult();
                    }

                    AssertEx.Equal(second, result.Continuation);
                    AssertEx.False(second.IsPending);
                    connection.CloseConnection();
                    server.Verify();
                    AssertPowerOffCommandCounts(server, 2, 4);
                }
                finally
                {
                    releaseStatus.Set();
                }
            }
        }

        private static void
            PowerOffSplitConcurrentBeginPreservesWirePublicationOrder()
        {
            using (var firstAcknowledgementHeld =
                new ManualResetEventSlim(false))
            using (var releaseFirstPublication =
                new ManualResetEventSlim(false))
            using (var secondInvocationStarted =
                new ManualResetEventSlim(false))
            using (var secondPowerOffReceived =
                new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                ObservedPowerOffStep(secondPowerOffReceived),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var firstTime = new FakeWaitTime();
                    var firstTask = Task.Run(
                        () => axis.BeginPowerOffWaitForStableStateAsync(
                                LongOptions(),
                                CancellationToken.None,
                                firstTime.ElapsedMilliseconds,
                                firstTime.DelayAsync,
                                () =>
                                {
                                    firstAcknowledgementHeld.Set();
                                    AssertEx.True(
                                        releaseFirstPublication.Wait(5000),
                                        "The first accepted Power Off publication was not released.");
                                })
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstAcknowledgementHeld.Wait(2000),
                        "The first Power Off acknowledgement was not held before publication.");

                    var secondTask = Task.Run(
                        () =>
                        {
                            secondInvocationStarted.Set();
                            return axis
                                .BeginPowerOffWaitForStableStateAsync(
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        });
                    AssertEx.True(
                        secondInvocationStarted.Wait(2000),
                        "The second Power Off Begin did not start.");
                    AssertEx.False(
                        secondPowerOffReceived.Wait(500),
                        "A newer Power Off reached the wire before the earlier accepted continuation was published.");

                    releaseFirstPublication.Set();
                    var first = firstTask.GetAwaiter().GetResult();
                    var second = secondTask.GetAwaiter().GetResult();

                    AssertEx.True(first.IsSuperseded);
                    AssertEx.False(first.IsPending);
                    AssertEx.True(second.IsPending);
                    AssertEx.Equal(
                        second,
                        axis.PendingPowerOffWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(
                        2,
                        CountAxisPowerCommand(server, false));
                }
                finally
                {
                    releaseFirstPublication.Set();
                }
            }
        }

        private static void
            PowerOffSplitTimeoutResetsPendingPowerOnOffProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                PowerStep(false),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
                StatusStep(Standstill),
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
                var beginTime = new FakeWaitTime();
                var powerOff = axis.BeginPowerOffWaitForStableStateAsync(
                        LongOptions(),
                        CancellationToken.None,
                        beginTime.ElapsedMilliseconds,
                        beginTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                var firstResumeTime = new FakeWaitTime();
                var powerOffTimeout = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            powerOff,
                            new LMCAxisPowerStateWaitOptions
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

                AssertEx.Equal(powerOff, powerOffTimeout.Continuation);
                AssertEx.Equal(
                    1,
                    powerOffTimeout.Evidence
                        .StablePowerOffStandstillSampleCount);
                AssertEx.Equal(
                    0,
                    powerOff.StablePowerOffStandstillSampleCount);
                AssertEx.Equal(
                    0,
                    pendingPowerOn.StablePowerOffStandstillSampleCount);
                AssertEx.Equal(
                    1,
                    CountAxisPowerCommand(server, false));

                var secondResumeTime = new FakeWaitTime();
                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        powerOff,
                        LongOptions(),
                        CancellationToken.None,
                        secondResumeTime.ElapsedMilliseconds,
                        secondResumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(powerOff, result.Continuation);
                AssertEx.Equal(
                    3,
                    pendingPowerOn.StablePowerOffStandstillSampleCount);
                AssertEx.Equal(
                    1,
                    CountAxisPowerCommand(server, false));

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(5, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOffSplitConcurrentResumeSecondIsZeroWire()
        {
            using (var firstStatusReceived = new ManualResetEventSlim(false))
            using (var releaseFirstStatus = new ManualResetEventSlim(false))
            using (var secondResumeStarted = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                DelayedStatusStep(
                    Standstill,
                    firstStatusReceived,
                    releaseFirstStatus),
                StatusStep(Standstill),
                StatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                try
                {
                    var axis = ConnectAndCreateAxis(connection, server.Port);
                    var continuation = axis
                        .BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var firstResume = Task.Run(
                        () => axis.ResumePowerOffWaitForStableStateAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(
                        firstStatusReceived.Wait(2000),
                        "The first concurrent resume did not reach status polling.");

                    var secondResume = Task.Run(
                        () =>
                        {
                            secondResumeStarted.Set();
                            return axis
                                .ResumePowerOffWaitForStableStateAsync(
                                    continuation,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();
                        });
                    AssertEx.True(
                        secondResumeStarted.Wait(2000),
                        "The second concurrent resume did not start.");
                    releaseFirstStatus.Set();

                    var result = firstResume.GetAwaiter().GetResult();
                    AssertEx.Equal(continuation, result.Continuation);
                    AssertEx.Throws<InvalidOperationException>(
                        () => secondResume.GetAwaiter().GetResult());

                    connection.CloseConnection();
                    server.Verify();
                    AssertPowerOffCommandCounts(server, 1, 3);
                }
                finally
                {
                    releaseFirstStatus.Set();
                }
            }
        }

        private static void PowerOffSplitStaleSessionResumeIsZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false)))
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
                    .BeginPowerOffWaitForStableStateAsync(
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
                    () => newAxis.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertEx.Equal(0, CountCommand(secondServer, 0x2023));
                AssertEx.Equal(0, CountCommand(secondServer, 0x2028));
            }
        }

        private static void
            PowerOnAcceptedGenerationAndContinuationAreAtomic()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var error = BeginAcceptedPowerOnWithPublicationCancellation(
                    axis);
                var continuation = error.Continuation;
                var coordinator = connection.GetAxisPowerOnWaitCoordinator(
                    axis.SessionGeneration,
                    axis.AxisReference);

                AssertEx.Equal(
                    LMCAxisPowerOnSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.NotNull(error.Evidence.PowerOnAcknowledgement);
                AssertEx.True(
                    error.Evidence.PowerOnAcknowledgement.IsSuccess);
                AssertEx.NotNull(continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.PowerOnMutationGeneration > 0);
                AssertEx.Equal(
                    continuation.PowerOnMutationGeneration,
                    continuation.ObservedMutationGeneration);
                AssertEx.Equal(
                    continuation.PowerOnMutationGeneration,
                    error.Evidence.PowerOnMutationGeneration);
                AssertEx.Equal(
                    continuation.PowerOnMutationGeneration,
                    error.Evidence.ObservedMutationGeneration);
                AssertEx.Equal(
                    continuation.PowerOnMutationGeneration,
                    coordinator.MutationGeneration);
                AssertEx.Equal(0, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(0, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOnMutationAfterAckParseBeforeContinuationPublication()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, AxisName);
                var time = new FakeWaitTime();
                Task<LMC_Response> queuedMove = null;

                var interference = AssertEx.Throws<
                    LMCAxisPowerOnInterferenceException>(
                    () => first.PowerOnAndWaitForStableStateAsync(
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            accepted => AssertEx.True(
                                queuedMove.GetAwaiter()
                                    .GetResult()
                                    .IsSuccess),
                            afterPowerOnAcknowledgementParsed:
                                () =>
                                {
                                    queuedMove = Task.Run(
                                        () => SendMoveAbsolute(second));
                                    AssertEx.False(
                                        queuedMove.Wait(100),
                                        "The raw Axis mutation crossed the accepted publication mutation gate.");
                                })
                        .GetAwaiter()
                        .GetResult());
                var continuation = interference.Continuation;
                var expectedGeneration =
                    interference.ExpectedMutationGeneration;

                AssertEx.NotNull(continuation);
                AssertEx.True(expectedGeneration > 0);
                AssertPowerOnInterference(
                    interference,
                    continuation,
                    expectedGeneration,
                    expectedGeneration + 1,
                    0);
                AssertEx.Equal(
                    continuation,
                    first.PendingPowerOnWaitContinuation);
                AssertEx.Equal(
                    continuation,
                    second.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(1, CountCommand(server, 0x209F));
                AssertEx.Equal(0, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOnSameReferenceHandleMoveInterferesBeforeResume()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, AxisName);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(first)
                        .Continuation;
                var powerOnGeneration =
                    continuation.PowerOnMutationGeneration;

                AssertEx.True(powerOnGeneration > 0);
                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                var interference = AssertEx.Throws<
                    LMCAxisPowerOnInterferenceException>(
                    () => first.ResumePowerOnWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertPowerOnInterference(
                    interference,
                    continuation,
                    powerOnGeneration,
                    powerOnGeneration + 1,
                    0);
                AssertEx.Equal(
                    continuation,
                    first.PendingPowerOnWaitContinuation);
                AssertEx.Equal(
                    continuation,
                    second.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(0, CountCommand(server, 0x2028));
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void PowerOnMutationRacingStatusDiscardsResult()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = SingleSamplePowerOnOptions(1000);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(axis)
                        .Continuation;
                var powerOnGeneration =
                    continuation.PowerOnMutationGeneration;
                var time = new FakeWaitTime();

                var interference = AssertEx.Throws<
                    LMCAxisPowerOnInterferenceException>(
                    () => axis.ResumePowerOnWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(
                                SendMoveAbsolute(axis).IsSuccess))
                        .GetAwaiter()
                        .GetResult());

                AssertPowerOnInterference(
                    interference,
                    continuation,
                    powerOnGeneration,
                    powerOnGeneration + 1,
                    0);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(1, CountCommand(server, 0x2028));
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOnZeroWireMutationDoesNotAdvanceGeneration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(axis)
                        .Continuation;
                var coordinator = connection.GetAxisPowerOnWaitCoordinator(
                    axis.SessionGeneration,
                    axis.AxisReference);
                var powerOnGeneration =
                    continuation.PowerOnMutationGeneration;
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
                    powerOnGeneration,
                    coordinator.MutationGeneration);

                var result = axis.ResumePowerOnWaitForStableStateAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    powerOnGeneration,
                    result.Evidence.ObservedMutationGeneration);
                AssertEx.Equal(
                    powerOnGeneration,
                    result.Evidence.PowerOnMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(3, CountCommand(server, 0x2028));
                AssertEx.Equal(0, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOnDifferentAxisMutationDoesNotInterfere()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(AxisReference),
                AxisInfoStep(AxisReference),
                LookupStep(SecondAxisReference),
                AxisInfoStep(SecondAxisReference),
                PowerStep(true),
                MoveAbsoluteStep(SecondAxisReference),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, SecondAxisName);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(first)
                        .Continuation;
                var firstCoordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        first.SessionGeneration,
                        first.AxisReference);
                var secondCoordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        second.SessionGeneration,
                        second.AxisReference);
                var powerOnGeneration =
                    continuation.PowerOnMutationGeneration;

                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                AssertEx.Equal(
                    powerOnGeneration,
                    firstCoordinator.MutationGeneration);
                AssertEx.Equal(1L, secondCoordinator.MutationGeneration);

                var result = first.ResumePowerOnWaitForStableStateAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    powerOnGeneration,
                    result.Evidence.PowerOnMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(3, CountCommand(server, 0x2028));
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOnFinalPublicationCancellationRetainsPending()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(axis)
                        .Continuation;
                var time = new FakeWaitTime();
                var publicationCount = 0;

                var error = AssertEx.Throws<
                    LMCAxisPowerStateWaitCanceledException>(
                    () => axis.ResumePowerOnWaitForStableStateAsync(
                            continuation,
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () =>
                            {
                                publicationCount++;
                                if (publicationCount == 3)
                                {
                                    cancellation.Cancel();
                                }
                            })
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, error.Continuation);
                AssertEx.Equal(3, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    error.Evidence.StablePowerOnSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);
                AssertEx.Equal(0, continuation.StablePowerOnSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(3, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOnFinalPublicationDeadlineRetainsPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(axis)
                        .Continuation;
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 100,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 3
                };
                var time = new FakeWaitTime();
                var publicationCount = 0;

                var error = AssertEx.Throws<
                    LMCAxisPowerStateWaitTimeoutException>(
                    () => axis.ResumePowerOnWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () =>
                            {
                                publicationCount++;
                                if (publicationCount == 3)
                                {
                                    time.Advance(80);
                                }
                            })
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, error.Continuation);
                AssertEx.Equal(3, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    error.Evidence.StablePowerOnSampleCount);
                AssertEx.Equal(100L, error.Evidence.ElapsedMilliseconds);
                AssertEx.False(error.TransportInvalidatedAtDeadline);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOnWaitContinuation);
                AssertEx.Equal(0, continuation.StablePowerOnSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(3, CountCommand(server, 0x2028));
            }
        }

        private static void PowerOnFinalProofWinsLateCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSamplePowerOnOptions(1000);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(
                        axis,
                        options)
                        .Continuation;
                var time = new FakeWaitTime();

                var result = axis.ResumePowerOnWaitForStableStateAsync(
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
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(1, CountCommand(server, 0x2028));
            }
        }

        private static void PowerOnFinalProofWinsLateDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(true),
                StatusStep(PowerOn | Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSamplePowerOnOptions(10);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation =
                    BeginAcceptedPowerOnWithPublicationCancellation(
                        axis,
                        options)
                        .Continuation;
                var time = new FakeWaitTime();

                var result = axis.ResumePowerOnWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () => time.Advance(10))
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(10L, result.Evidence.ElapsedMilliseconds);
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisPowerOnWaitContinuation>(
                    null,
                    axis.PendingPowerOnWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2023));
                AssertEx.Equal(1, CountCommand(server, 0x2028));
            }
        }

        private static void
            PowerOffSameReferenceHandleMoveInterferesBeforeResume()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                MoveAbsoluteStep(AxisReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, AxisName);
                var continuation = first
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var powerOffGeneration =
                    continuation.PowerOffMutationGeneration;

                AssertEx.True(powerOffGeneration > 0);
                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                var interference = AssertEx.Throws<
                    LMCAxisPowerOffInterferenceException>(
                    () => first.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertPowerOffInterference(
                    interference,
                    continuation,
                    powerOffGeneration,
                    powerOffGeneration + 1,
                    0);
                AssertEx.Equal(
                    continuation,
                    first.PendingPowerOffWaitContinuation);
                AssertEx.Equal(
                    continuation,
                    second.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void PowerOffMutationRacingStatusDiscardsResult()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                MoveAbsoluteStep(AxisReference),
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
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var powerOffGeneration =
                    continuation.PowerOffMutationGeneration;
                var time = new FakeWaitTime();

                var interference = AssertEx.Throws<
                    LMCAxisPowerOffInterferenceException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => AssertEx.True(
                                SendMoveAbsolute(axis).IsSuccess))
                        .GetAwaiter()
                        .GetResult());

                AssertPowerOffInterference(
                    interference,
                    continuation,
                    powerOffGeneration,
                    powerOffGeneration + 1,
                    0);
                AssertEx.Equal<LMCReadStatusResult>(
                    null,
                    interference.Evidence.LastObservedStatus);
                AssertEx.Equal(0, continuation.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 1);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOffZeroWireMutationDoesNotAdvanceGeneration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = connection.GetAxisPowerOnWaitCoordinator(
                    axis.SessionGeneration,
                    axis.AxisReference);
                var powerOffGeneration =
                    continuation.PowerOffMutationGeneration;
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
                    powerOffGeneration,
                    coordinator.MutationGeneration);

                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    powerOffGeneration,
                    result.Evidence.ObservedMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
                AssertEx.Equal(0, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOffDifferentAxisMutationDoesNotInterfere()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(AxisReference),
                AxisInfoStep(AxisReference),
                LookupStep(SecondAxisReference),
                AxisInfoStep(SecondAxisReference),
                PowerStep(false),
                MoveAbsoluteStep(SecondAxisReference),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = ConnectAndCreateAxis(connection, server.Port);
                var second = new LMCAxis(connection, SecondAxisName);
                var continuation = first
                    .BeginPowerOffWaitForStableStateAsync(
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
                var powerOffGeneration =
                    continuation.PowerOffMutationGeneration;

                AssertEx.True(SendMoveAbsolute(second).IsSuccess);
                AssertEx.Equal(
                    powerOffGeneration,
                    firstCoordinator.MutationGeneration);
                AssertEx.Equal(1L, secondCoordinator.MutationGeneration);

                var result = first.ResumePowerOffWaitForStableStateAsync(
                        continuation,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(
                    result.Evidence.InterveningMutationDetected);
                AssertEx.Equal(
                    powerOffGeneration,
                    result.Evidence.PowerOffMutationGeneration);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x209F));
            }
        }

        private static void
            PowerOffFinalPublicationCancellationRetainsPending()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                var publicationCount = 0;

                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () =>
                            {
                                publicationCount++;
                                if (publicationCount == 3)
                                {
                                    cancellation.Cancel();
                                }
                            })
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, error.Continuation);
                AssertEx.Equal(3, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    error.Evidence.StablePowerOffStandstillSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(0, continuation
                    .StablePowerOffStandstillSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
            }
        }

        private static void PowerOffFinalPublicationDeadlineRetainsPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 100,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 3
                };
                var time = new FakeWaitTime();
                var publicationCount = 0;

                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () =>
                            {
                                publicationCount++;
                                if (publicationCount == 3)
                                {
                                    time.Advance(80);
                                }
                            })
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, error.Continuation);
                AssertEx.Equal(3, error.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    error.Evidence.StablePowerOffStandstillSampleCount);
                AssertEx.Equal(100L, error.Evidence.ElapsedMilliseconds);
                AssertEx.False(error.TransportInvalidatedAtDeadline);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(0, continuation
                    .StablePowerOffStandstillSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 3);
            }
        }

        private static void
            PowerOffAcceptedPublicationCancellationPreservesContinuation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitCanceledException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.NotNull(error.Continuation);
                AssertEx.True(error.Continuation.IsPending);
                AssertEx.True(
                    error.Continuation.PowerOffMutationGeneration > 0);
                AssertEx.Equal(
                    error.Continuation,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void
            PowerOffAcceptedPublicationDeadlinePreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var options = new LMCAxisPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 10,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 3
                };
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCAxisPowerOffWaitTimeoutException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => time.Advance(10))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Accepted,
                    error.Evidence.SubmissionOutcome);
                AssertEx.NotNull(error.Continuation);
                AssertEx.True(error.Continuation.IsPending);
                AssertEx.Equal(10L, error.Evidence.ElapsedMilliseconds);
                AssertEx.False(error.TransportInvalidatedAtDeadline);
                AssertEx.Equal(
                    error.Continuation,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(
                    LMCConnectionState.Connected,
                    connection.State);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 0);
            }
        }

        private static void PowerOffNonfinalSupersedeCannotCompleteOldWait()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                PowerStep(false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var first = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAxisPowerOffWaitContinuation second = null;
                var time = new FakeWaitTime();
                var failure = AssertEx.Throws<
                    LMCAxisPowerOffStatusException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            first,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            () => second = axis
                                .BeginPowerOffWaitForStableStateAsync(
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult())
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(first, failure.Continuation);
                AssertEx.True(first.IsSuperseded);
                AssertEx.False(first.IsCompleted);
                AssertEx.NotNull(second);
                AssertEx.True(second.IsPending);
                AssertEx.Equal(
                    second,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(1, failure.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 2, 1);
            }
        }

        private static void PowerOffRejectedBeginPreservesPriorPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                RejectedPowerOffStep(),
                StatusStep(state: Standstill),
                StatusStep(state: Standstill),
                StatusStep(state: Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var prior = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var priorGeneration =
                    prior.PowerOffMutationGeneration;
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);

                var rejected = AssertEx.Throws<
                    LMCAxisPowerOffRejectedException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.Rejected,
                    rejected.Evidence.SubmissionOutcome);
                AssertEx.Equal(
                    priorGeneration + 1,
                    rejected.Evidence.PowerOffMutationGeneration);
                AssertEx.Equal(
                    priorGeneration,
                    coordinator.MutationGeneration);
                AssertEx.True(prior.IsPending);
                AssertEx.False(prior.IsSuperseded);
                AssertEx.Equal(
                    prior,
                    axis.PendingPowerOffWaitContinuation);

                var resumed = axis.ResumePowerOffWaitForStableStateAsync(
                        prior,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(prior, resumed.Continuation);
                AssertEx.True(prior.IsCompleted);
                AssertEx.Equal(3, resumed.StatusPollCount);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 2, 3);
            }
        }

        private static void
            PowerOffResponseLossPreservesPriorButInvalidatesGeneration()
        {
            var responseLoss = new FakeRpcStep(0x2023, new byte[0])
            {
                CloseClientBeforeResponse = true,
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    request)
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                responseLoss))
            using (var connection = new LMCConnection())
            {
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var prior = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var priorGeneration =
                    prior.PowerOffMutationGeneration;
                var coordinator = connection
                    .GetAxisPowerOnWaitCoordinator(
                        axis.SessionGeneration,
                        axis.AxisReference);

                var uncertain = AssertEx.Throws<
                    LMCAxisPowerOffSubmissionException>(
                    () => axis.BeginPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCAxisPowerOffSubmissionOutcome.OutcomeUncertain,
                    uncertain.Evidence.SubmissionOutcome);
                AssertEx.True(uncertain.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    uncertain.Evidence.PowerOffAcknowledgement);
                AssertEx.Equal(
                    priorGeneration + 1,
                    uncertain.Evidence.PowerOffMutationGeneration);
                AssertEx.Equal(
                    priorGeneration + 1,
                    uncertain.Evidence.ObservedMutationGeneration);
                AssertEx.Equal(
                    priorGeneration + 1,
                    coordinator.MutationGeneration);
                AssertEx.True(prior.IsPending);
                AssertEx.False(prior.IsSuperseded);
                AssertEx.Equal(
                    prior,
                    axis.PendingPowerOffWaitContinuation);
                AssertEx.Equal(
                    LMCConnectionState.Faulted,
                    connection.State);

                AssertEx.Throws<InvalidOperationException>(
                    () => axis.ResumePowerOffWaitForStableStateAsync(
                            prior,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                server.Verify();
                AssertPowerOffCommandCounts(server, 2, 0);
            }
        }

        private static void PowerOffFinalProofWinsLateCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSamplePowerOffOptions(1000);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();

                var result = axis.ResumePowerOffWaitForStableStateAsync(
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
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 1);
            }
        }

        private static void PowerOffFinalProofWinsLateDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                AxisInfoStep(),
                PowerStep(false),
                PowerOffStatusStep(Standstill),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var options = SingleSamplePowerOffOptions(10);
                var axis = ConnectAndCreateAxis(connection, server.Port);
                var time = new FakeWaitTime();
                var continuation = axis
                    .BeginPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                var result = axis.ResumePowerOffWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () => time.Advance(10))
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(10L, result.ElapsedMilliseconds);
                AssertEx.Equal(continuation, result.Continuation);
                AssertEx.True(continuation.IsCompleted);
                AssertEx.False(continuation.IsPending);
                AssertEx.Equal<LMCAxisPowerOffWaitContinuation>(
                    null,
                    axis.PendingPowerOffWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertPowerOffCommandCounts(server, 1, 1);
            }
        }

        private static LMCAxisPowerStateWaitCanceledException
            BeginAcceptedPowerOnWithPublicationCancellation(
                LMCSingleAxis axis,
                LMCAxisPowerStateWaitOptions options = null)
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var time = new FakeWaitTime();
                return AssertEx.Throws<
                    LMCAxisPowerStateWaitCanceledException>(
                    () => axis.PowerOnAndWaitForStableStateAsync(
                            options ?? LongOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());
            }
        }

        private static void AssertPowerOnInterference(
            LMCAxisPowerOnInterferenceException interference,
            LMCAxisPowerOnWaitContinuation continuation,
            long expectedGeneration,
            long observedGeneration,
            int statusPollCount)
        {
            AssertEx.Equal(continuation, interference.Continuation);
            AssertEx.Equal(
                expectedGeneration,
                interference.ExpectedMutationGeneration);
            AssertEx.Equal(
                observedGeneration,
                interference.ObservedMutationGeneration);
            AssertEx.True(
                interference.Evidence.InterveningMutationDetected);
            AssertEx.Equal(
                statusPollCount,
                interference.Evidence.StatusPollCount);
            AssertEx.True(continuation.IsPending);
            AssertEx.True(continuation.InterveningMutationDetected);
        }

        private static void AssertPowerOffInterference(
            LMCAxisPowerOffInterferenceException interference,
            LMCAxisPowerOffWaitContinuation continuation,
            long expectedGeneration,
            long observedGeneration,
            int statusPollCount)
        {
            AssertEx.Equal(continuation, interference.Continuation);
            AssertEx.Equal(
                expectedGeneration,
                interference.ExpectedMutationGeneration);
            AssertEx.Equal(
                observedGeneration,
                interference.ObservedMutationGeneration);
            AssertEx.True(
                interference.Evidence.InterveningMutationDetected);
            AssertEx.Equal(
                statusPollCount,
                interference.Evidence.StatusPollCount);
            AssertEx.True(continuation.IsPending);
            AssertEx.True(continuation.InterveningMutationDetected);
        }

        private static LMCAxisPowerStateWaitOptions LongOptions()
        {
            return new LMCAxisPowerStateWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCAxisPowerStateWaitOptions
            SingleSamplePowerOnOptions(int timeoutMilliseconds)
        {
            return new LMCAxisPowerStateWaitOptions
            {
                TimeoutMilliseconds = timeoutMilliseconds,
                PollIntervalMilliseconds = Math.Min(
                    10,
                    timeoutMilliseconds),
                StableSampleCount = 1
            };
        }

        private static LMCAxisPowerStateWaitOptions
            SingleSamplePowerOffOptions(int timeoutMilliseconds)
        {
            return new LMCAxisPowerStateWaitOptions
            {
                TimeoutMilliseconds = timeoutMilliseconds,
                PollIntervalMilliseconds = Math.Min(
                    10,
                    timeoutMilliseconds),
                StableSampleCount = 1
            };
        }

        private static LMCAxisPowerStateWaitOptions DeadlineOptions()
        {
            return new LMCAxisPowerStateWaitOptions
            {
                TimeoutMilliseconds = 200,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
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

        private static FakeRpcStep PowerStep(bool enable)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        LMC_Frame.LMCAxisPower(
                            AxisReference,
                            enable),
                        request);
                }
            };
        }

        private static FakeRpcStep RejectedPowerOnStep()
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request =>
                {
                    AssertEx.Equal((byte)1, request[12]);
                }
            };
        }

        private static FakeRpcStep RejectedPowerOffStep()
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 F9 FF")))
            {
                InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        LMC_Frame.LMCAxisPower(AxisReference, false),
                        request);
                }
            };
        }

        private static FakeRpcStep DelayedPowerOffStep(
            ManualResetEventSlim powerOffReceived,
            ManualResetEventSlim releasePowerOff)
        {
            var step = PowerStep(false);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    request);
                powerOffReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releasePowerOff.Wait(5000),
                "The delayed Power Off response was not released.");
            return step;
        }

        private static FakeRpcStep DelayedPowerOnStep(
            ManualResetEventSlim powerOnReceived,
            ManualResetEventSlim releasePowerOn)
        {
            var step = PowerStep(true);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, true),
                    request);
                powerOnReceived.Set();
            };
            step.BeforeResponse = () => AssertEx.True(
                releasePowerOn.Wait(5000),
                "The delayed Power On response was not released.");
            return step;
        }

        private static FakeRpcStep ObservedPowerOffStep(
            ManualResetEventSlim powerOffReceived)
        {
            var step = PowerStep(false);
            step.InspectRequest = request =>
            {
                AssertEx.SequenceEqual(
                    LMC_Frame.LMCAxisPower(AxisReference, false),
                    request);
                powerOffReceived.Set();
            };
            return step;
        }

        private static FakeRpcStep StatusStep(uint state)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            return new FakeRpcStep(0x2028, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DelayedStatusStep(
            uint state,
            ManualResetEventSlim statusReceived,
            ManualResetEventSlim releaseStatus)
        {
            var step = StatusStep(state);
            step.InspectRequest = request => statusReceived.Set();
            step.BeforeResponse = () => AssertEx.True(
                releaseStatus.Wait(5000),
                "The delayed Axis status response was not released.");
            return step;
        }

        private static FakeRpcStep PowerOffStatusStep(
            uint state,
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

        private static FakeRpcStep StopStep()
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
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

        private static int CountCommand(FakeRpcServer server, ushort command)
        {
            return server.ReceivedRequests.Count(
                request => TestFrame.ReadUInt16(request, 0) == command);
        }

        private static int CountAxisPowerCommand(
            FakeRpcServer server,
            bool enable)
        {
            return server.ReceivedRequests.Count(
                request => TestFrame.ReadUInt16(request, 0) == 0x2023
                    && request.Length > 12
                    && request[12] == (enable ? (byte)1 : (byte)0));
        }

        private static void AssertPowerOffCommandCounts(
            FakeRpcServer server,
            int expectedPowerOffCount,
            int expectedStatusCount)
        {
            AssertEx.Equal(
                expectedPowerOffCount,
                CountCommand(server, 0x2023));
            AssertEx.Equal(
                expectedStatusCount,
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

            internal void Advance(long milliseconds)
            {
                elapsedMilliseconds += milliseconds;
            }
        }
    }
}

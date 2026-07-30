using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupPowerStateWaitContractTests
    {
        private const string GroupName = "_LMCRobotBase1";
        private const string OtherGroupName = "_LMCRobotBase2";
        private const ushort GroupReference = 0x0100;
        private const ushort OtherGroupReference = 0x0101;
        private const uint PowerOn = 0x00040000u;
        private const uint Standby = 0x00020000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "GroupPowerStateWait.Defaults.PowerOffUsesOnlyStatusPolling",
                DefaultsAndPowerOffUseOnlyStatusPolling);
            tests.Add(
                "GroupPowerStateWait.Stability.MismatchResetsConsecutiveProof",
                MismatchResetsConsecutiveProof);
            tests.Add(
                "GroupPowerStateWait.PreWire.ValidationAndCancelAreZeroRequest",
                ValidationAndCancellationAreZeroRequest);
            tests.Add(
                "GroupPowerStateWait.StatusError.PreservesTypedEvidence",
                StatusErrorPreservesTypedEvidence);
            tests.Add(
                "GroupPowerStateWait.Timeout.DelayUsesTotalDeadline",
                DelayUsesTotalDeadline);
            tests.Add(
                "GroupPowerStateWait.Cancel.PostWriteDrainKeepsConnectionReusable",
                PostWriteCancellationDrainsAndKeepsConnectionReusable);
            tests.Add(
                "GroupPowerStateWait.Timeout.NoStatusInvalidatesTransport",
                NoStatusDeadlineInvalidatesTransport);
            tests.Add(
                "GroupPowerStateWait.Timeout.ConnectionGateIsZeroWireAndReusable",
                ConnectionGateTimeoutIsZeroWireAndReusable);
            tests.Add(
                "GroupPowerStateWait.Continuation.AckPublicationIsLinearized",
                AcceptedEnablePublicationIsLinearizedAndObservedOnce);
            tests.Add(
                "GroupPowerStateWait.Scope.ReconnectRejectsStaleHandle",
                ReconnectRejectsStaleHandleAndIsolatesContinuation);
            tests.Add(
                "GroupPowerStateWait.AcceptedOnce.SplitPowerOnIsStatusOnly",
                SplitPowerOnIsAcceptedOnceAndResumeIsStatusOnly);
            tests.Add(
                "GroupPowerStateWait.AcceptedOnce.SplitPowerOffIsStatusOnly",
                SplitPowerOffIsAcceptedOnceAndResumeIsStatusOnly);
            tests.Add(
                "GroupPowerStateWait.Observer.ThrowPreservesExactPending",
                ObserverThrowPreservesExactPendingAndSendsNoStatus);
            tests.Add(
                "GroupPowerStateWait.Submission.TypedOutcomes",
                SubmissionOutcomesAreTypedAndDoNotReplay);
            tests.Add(
                "GroupPowerStateWait.Pending.RawSafetyAndExplicitReplacement",
                RawSafetyAndExplicitPowerOffReplacementAreZeroReplay);
            tests.Add(
                "GroupPowerStateWait.Replacement.RejectedPreservesPrevious",
                RejectedPowerOffReplacementPreservesPreviousPending);
            tests.Add(
                "GroupPowerStateWait.Resume.CompletedAndForeignAreZeroWire",
                CompletedAndForeignResumeAreZeroWire);
            tests.Add(
                "GroupPowerStateWait.Resume.ConcurrentSecondIsZeroWire",
                ConcurrentSecondResumeIsZeroWire);
            tests.Add(
                "GroupPowerStateWait.Interference.SameGroupMutationStaysPending",
                SameGroupMutationProducesTypedInterferenceAndStaysPending);
            tests.Add(
                "GroupPowerStateWait.Linearization.FinalProofWinsLateCancel",
                FinalProofWinsLateCancellation);
            tests.Add(
                "GroupPowerStateWait.Noninterference.ZeroWireAndDifferentGroup",
                ZeroWireAndDifferentGroupDoNotInterfere);
            tests.Add(
                "GroupPowerStateWait.Timeout.AcceptedNoStatusFaultsTransport",
                AcceptedNoStatusDeadlineFaultsTransport);
            tests.Add(
                "GroupPowerStateWait.Publication.ReconnectRejectsAckGap",
                ReconnectBetweenAckParseAndPublicationCreatesNoPending);
            tests.Add(
                "GroupPowerStateWait.Compound.ObserverRunsBeforeFirstStatus",
                CompoundObserverRunsBeforeFirstStatusForOnAndOff);
            tests.Add(
                "GroupPowerStateWait.Timeout.NoCommandAckFaultsTransport",
                NoCommandAcknowledgementDeadlineFaultsTransport);
            tests.Add(
                "GroupPowerStateWait.Publication.MutationGapIsAttributed",
                MutationBetweenAckParseAndContinuationPublicationIsAttributed);
            tests.Add(
                "GroupPowerStateWait.Linearization.EarlyCancelStaysPending",
                EarlyCancellationAtFinalDecisionStaysPending);
            tests.Add(
                "GroupPowerStateWait.Scope.StaleSessionResumeIsZeroWire",
                StaleSessionResumeIsZeroWire);
            tests.Add(
                "GroupPowerStateWait.Replacement.PowerOffInterferenceCanReplace",
                InterferedPowerOffCanBeExplicitlyReplaced);
            tests.Add(
                "GroupPowerStateWait.Observer.ReentrantStatusAndSafetyOffComplete",
                ObserverReentrantStatusAndSafetyPowerOffComplete);
            tests.Add(
                "GroupPowerStateWait.Linearization.DeadlineSymmetry",
                EarlyAndLateDeadlineFinalDecisionIsLinearized);
            tests.Add(
                "GroupPowerStateWait.Interference.AdminSyncAndAsyncMove",
                AdminSyncAndAsyncMoveProduceTypedInterference);
            tests.Add(
                "GroupPowerStateWait.Publication.FinalStatusReconnectRejectsProof",
                ReconnectBeforeFinalStatusPublicationRejectsProof);
            tests.Add(
                "GroupPowerStateWait.Interaction.PendingEnableObservesPowerPolls",
                PendingEnableObservesAndResetsWithPowerPolling);
            tests.Add(
                "GroupPowerStateWait.Interaction.PowerInterferesWithPendingStop",
                AcceptedPowerLeavesPendingStopWithTypedInterference);
        }

        private static void DefaultsAndPowerOffUseOnlyStatusPolling()
        {
            var defaults = new LMCGroupPowerStateWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();

                var result = group.WaitForPowerStateAsync(
                        false,
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.False(result.ExpectedPowerOn);
                AssertEx.NotNull(result.FinalStatus);
                AssertEx.False(result.FinalStatus.IsPowerOn);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.Equal(3, result.RequiredStableSampleCount);
                AssertEx.Equal(100L, result.ElapsedMilliseconds);
                AssertEx.Equal(3, result.Evidence.StatusPollCount);
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.NotAttempted,
                    result.SubmissionOutcome);
                AssertEx.Equal<LMC_Response>(null, result.Acknowledgement);
                AssertEx.Equal(0L,
                    result.Evidence.PowerMutationGeneration);
                AssertEx.Equal(0L,
                    result.Evidence.ObservedMutationGeneration);
                AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                    null,
                    result.Continuation);

                connection.CloseConnection();
                server.Verify();
                AssertOnlyStatusWaitCommands(server, 3);
            }
        }

        private static void MismatchResetsConsecutiveProof()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                StatusStep(PowerOn),
                StatusStep(0),
                StatusStep(PowerOn),
                StatusStep(PowerOn),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group.WaitForPowerStateAsync(
                        true,
                        new LMCGroupPowerStateWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 3
                        },
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.Equal(6, result.StatusPollCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.Equal(50L, result.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
                AssertOnlyStatusWaitCommands(server, 6);
            }
        }

        private static void ValidationAndCancellationAreZeroRequest()
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
                    () => group.WaitForPowerStateAsync(
                            true,
                            new LMCGroupPowerStateWaitOptions
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
                    LMCGroupPowerStateWaitCanceledException>(
                    () => group.WaitForPowerStateAsync(
                            true,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, canceled.StatusPollCount);
                AssertEx.Equal<LMCGroupReadStatusResult>(
                    null,
                    canceled.LastObservedStatus);

                connection.CloseConnection();
                server.Verify();
                AssertOnlyStatusWaitCommands(server, 0);
            }
        }

        private static void StatusErrorPreservesTypedEvidence()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                StatusStep(PowerOn, 0x0010, -31, 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var exception = AssertEx.Throws<
                    LMCGroupPowerStateStatusException>(
                    () => group.WaitForPowerStateAsync(
                            true,
                            LongOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.NotNull(exception.FailedStatus);
                AssertEx.False(exception.FailedStatus.IsSuccess);
                AssertEx.True(ReferenceEquals(
                    exception.FailedStatus,
                    exception.LastObservedStatus));
                AssertEx.Equal(2, exception.StatusPollCount);
                AssertEx.Equal(0, exception.StableSampleCount);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DelayUsesTotalDeadline()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupPowerStateWaitTimeoutException>(
                    () => group.WaitForPowerStateAsync(
                            true,
                            new LMCGroupPowerStateWaitOptions
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

                AssertEx.Equal(1, timeout.StatusPollCount);
                AssertEx.Equal(0, timeout.StableSampleCount);
                AssertEx.Equal(10L, timeout.ElapsedMilliseconds);
                AssertEx.False(timeout.LastObservedStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PostWriteCancellationDrainsAndKeepsConnectionReusable()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var delayed = StatusStep(PowerOn, 0, 0, 0, cancellation.Cancel);
                delayed.ResponseDelayMilliseconds = 60;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    delayed,
                    StatusStep(PowerOn),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var canceled = AssertEx.Throws<
                        LMCGroupPowerStateWaitCanceledException>(
                        () => group.WaitForPowerStateAsync(
                                true,
                                new LMCGroupPowerStateWaitOptions
                                {
                                    TimeoutMilliseconds = 1000,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 2
                                },
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(1, canceled.StatusPollCount);
                    AssertEx.NotNull(canceled.LastObservedStatus);
                    AssertEx.True(canceled.LastObservedStatus.IsPowerOn);
                    AssertEx.False(
                        canceled.Evidence.TransportInvalidatedAtDeadline);

                    var reused = group.GroupReadStatusResult();
                    AssertEx.True(reused.IsSuccess);
                    AssertEx.True(reused.IsPowerOn);
                    AssertEx.Equal(LMCConnectionState.Connected, connection.State);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void NoStatusDeadlineInvalidatesTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(PowerOn);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked Group power status response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupPowerStateWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupPowerStateWaitTimeoutException>(
                            () => group.WaitForPowerStateAsync(
                                    true,
                                    new LMCGroupPowerStateWaitOptions
                                    {
                                        TimeoutMilliseconds = 200,
                                        PollIntervalMilliseconds = 10,
                                        StableSampleCount = 2
                                    },
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(0, timeout.StatusPollCount);
                    AssertEx.Equal<LMCGroupReadStatusResult>(
                        null,
                        timeout.LastObservedStatus);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.True(
                        timeout.Evidence.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertOnlyStatusWaitCommands(server, 1);
                }
            }
        }

        private static void ConnectionGateTimeoutIsZeroWireAndReusable()
        {
            using (var blockerStarted = new ManualResetEventSlim(false))
            {
                var blocker = StatusStep(PowerOn, 0, 0, 0, blockerStarted.Set);
                blocker.ResponseDelayMilliseconds = 160;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blocker,
                    StatusStep(PowerOn),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var rawBlocker = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(blockerStarted.Wait(2000));

                    var timeout = AssertEx.Throws<
                        LMCGroupPowerStateWaitTimeoutException>(
                        () => group.WaitForPowerStateAsync(
                                true,
                                new LMCGroupPowerStateWaitOptions
                                {
                                    TimeoutMilliseconds = 30,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 1
                                },
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(0, timeout.StatusPollCount);

                    LMCConnection.ParseGroupReadStatusResult(
                        rawBlocker.GetAwaiter().GetResult());
                    var reused = group.GroupReadStatusResult();
                    AssertEx.True(reused.IsPowerOn);
                    AssertEx.Equal(LMCConnectionState.Connected, connection.State);

                    connection.CloseConnection();
                    server.Verify();
                    AssertOnlyStatusWaitCommands(server, 2);
                }
            }
        }

        private static void AcceptedEnablePublicationIsLinearizedAndObservedOnce()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var enableStarted = new ManualResetEventSlim(false))
            {
                var delayedEnable = EnableStep(
                    SuccessLongAcknowledgement(),
                    () =>
                    {
                        cancellation.Cancel();
                        enableStarted.Set();
                    });
                delayedEnable.ResponseDelayMilliseconds = 100;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    delayedEnable,
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var enableTask = firstHandle
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongEnableOptions(),
                            cancellation.Token);
                    AssertEx.True(enableStarted.Wait(2000));

                    var powerTask = secondHandle.WaitForPowerStateAsync(
                        true,
                        LongOptions(),
                        CancellationToken.None);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => enableTask.GetAwaiter().GetResult());
                    var result = powerTask.GetAwaiter().GetResult();

                    AssertEx.Equal(3, result.StatusPollCount);
                    AssertEx.Equal(3, canceled.Continuation.PollCount);
                    AssertEx.Equal(3, canceled.Continuation.StableSampleCount);
                    AssertEx.True(canceled.Continuation.IsPending);

                    var time = new FakeWaitTime();
                    var resumed = secondHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            canceled.Continuation,
                            LongEnableOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        secondHandle.PendingGroupEnableWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                    AssertOnlyStatusWaitCommands(server, 3);
                }
            }
        }

        private static void ReconnectRejectsStaleHandleAndIsolatesContinuation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var oldHandle = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var pending = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => oldHandle
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongEnableOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var newHandle = new LMCGroup(connection, GroupName);

                AssertEx.Throws<InvalidOperationException>(
                    () => oldHandle.WaitForPowerStateAsync(
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                var result = newHandle.WaitForPowerStateAsync(
                        true,
                        new LMCGroupPowerStateWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 1
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.Equal(0, pending.Continuation.PollCount);
                AssertEx.True(pending.Continuation.IsPending);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void
            SplitPowerOnIsAcceptedOnceAndResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(2);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        true,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(continuation.ExpectedPowerOn);
                AssertEx.True(continuation.IsPending);
                AssertEx.True(continuation.Acknowledgement.IsSuccess);
                AssertEx.Equal(0, continuation.StatusPollCount);
                AssertEx.True(continuation.PowerMutationGeneration > 0);
                AssertEx.True(ReferenceEquals(
                    continuation,
                    group.PendingGroupPowerStateWaitContinuation));
                AssertCommandCounts(server, 1, 0, 0);

                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.PowerCommandAccepted);
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.Accepted,
                    result.SubmissionOutcome);
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.Equal(2, result.StatusPollCount);
                AssertEx.True(continuation.IsCompleted);
                AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                    null,
                    group.PendingGroupPowerStateWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 2);
            }
        }

        private static void
            SplitPowerOffIsAcceptedOnceAndResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(false, true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        false,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(continuation.ExpectedPowerOn);
                AssertEx.Equal(0, continuation.StatusPollCount);
                AssertCommandCounts(server, 0, 1, 0);

                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(result.FinalStatus.IsPowerOn);
                AssertEx.True(continuation.IsCompleted);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 1);
            }
        }

        private static void
            ObserverThrowPreservesExactPendingAndSendsNoStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                LMCGroupPowerStateWaitContinuation observed = null;
                var observerFailure = AssertEx.Throws<InvalidOperationException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            LongOptions(),
                            continuation =>
                            {
                                observed = continuation;
                                AssertEx.True(continuation.IsPending);
                                AssertEx.Equal(0, continuation.StatusPollCount);
                                throw new InvalidOperationException(
                                    "observer failure");
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal("observer failure", observerFailure.Message);
                AssertEx.NotNull(observed);
                AssertEx.True(observed.IsPending);
                AssertEx.True(ReferenceEquals(
                    observed,
                    group.PendingGroupPowerStateWaitContinuation));

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerFailure = new OperationCanceledException(
                    "observer cancellation");
                LMCGroupPowerStateWaitContinuation observed = null;
                var actual = AssertEx.Throws<OperationCanceledException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
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
                AssertEx.True(ReferenceEquals(
                    observed,
                    group.PendingGroupPowerStateWaitContinuation));

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
            }
        }

        private static void SubmissionOutcomesAreTypedAndDoNotReplay()
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
                cancellation.Cancel();
                var canceled = AssertEx.Throws<
                    LMCGroupPowerStateWaitCanceledException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.NotAttempted,
                    canceled.Evidence.SubmissionOutcome);
                AssertEx.False(canceled.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                    null,
                    canceled.Continuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 0, 0);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var rejected = AssertEx.Throws<
                    LMCGroupPowerRejectedException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.Rejected,
                    rejected.Evidence.SubmissionOutcome);
                AssertEx.NotNull(rejected.Acknowledgement);
                AssertEx.False(rejected.Acknowledgement.IsSuccess);
                AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                    null,
                    group.PendingGroupPowerStateWaitContinuation);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
            }

            var responseLoss = new FakeRpcStep(0x204A, new byte[0])
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
                var uncertain = AssertEx.Throws<
                    LMCGroupPowerSubmissionException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.OutcomeUncertain,
                    uncertain.Evidence.SubmissionOutcome);
                AssertEx.True(uncertain.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal<LMC_Response>(
                    null,
                    uncertain.Evidence.PowerAcknowledgement);
                AssertEx.Equal(0, uncertain.Evidence.StatusPollCount);
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
            }
        }

        private static void
            RawSafetyAndExplicitPowerOffReplacementAreZeroReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                PowerStep(false, true),
                PowerStep(false, true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var first = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        true,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Throws<LMCGroupPowerStateWaitPendingException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<LMCGroupPowerStateWaitPendingException>(
                    () => group.GroupPowerOn());
                AssertEx.True(group.GroupPowerOff().IsSuccess);
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group.ResumeGroupPowerStateWaitForStableStateAsync(
                            first,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(
                    first,
                    interference.Continuation));
                AssertEx.True(first.IsPending);

                var replacement = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        false,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(first.IsSuperseded);
                AssertEx.True(replacement.IsPending);
                AssertEx.Throws<LMCGroupPowerStateWaitPendingException>(
                    () => group.GroupPowerOff());
                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        replacement,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(result.FinalStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 2, 1);
            }
        }

        private static void
            RejectedPowerOffReplacementPreservesPreviousPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                PowerStep(false, false),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var first = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var rejected = AssertEx.Throws<
                    LMCGroupPowerRejectedException>(
                    () => group.BeginGroupPowerOffWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.Rejected,
                    rejected.Evidence.SubmissionOutcome);
                AssertEx.True(first.IsPending);
                AssertEx.True(ReferenceEquals(
                    first,
                    group.PendingGroupPowerStateWaitContinuation));
                AssertEx.True(
                    first.PowerMutationGeneration
                        < rejected.Evidence.PowerMutationGeneration);
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            first,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(
                    first,
                    interference.Continuation));
                AssertEx.Equal(0,
                    interference.Evidence.StatusPollCount);
                AssertEx.True(first.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 0);
            }
        }

        private static void CompletedAndForeignResumeAreZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(GroupReference),
                LookupStep(OtherGroupReference),
                PowerStep(true, true, GroupReference),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var other = new LMCGroup(connection, OtherGroupName);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Throws<LMCGroupPowerStateWaitResolvedException>(
                    () => other
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                group.ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Throws<LMCGroupPowerStateWaitResolvedException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 1);
            }
        }

        private static void ConcurrentSecondResumeIsZeroWire()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            {
                var delayedStatus = StatusStep(PowerOn);
                delayedStatus.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    statusReceived.Set();
                };
                delayedStatus.BeforeResponse = () => AssertEx.True(
                    releaseStatus.Wait(5000));

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    PowerStep(true, true),
                    delayedStatus,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var options = SingleSampleOptions(1);
                    var continuation = group
                        .BeginGroupPowerOnWaitForStableStateAsync(
                            options,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    var first = Task.Run(
                        () => group
                            .ResumeGroupPowerStateWaitForStableStateAsync(
                                continuation,
                                options,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(statusReceived.Wait(2000));
                    AssertEx.Throws<InvalidOperationException>(
                        () => group
                            .ResumeGroupPowerStateWaitForStableStateAsync(
                                continuation,
                                options,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    releaseStatus.Set();
                    AssertEx.True(first.GetAwaiter().GetResult()
                        .FinalStatus.IsPowerOn);

                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 0, 1);
                }
            }
        }

        private static void
            SameGroupMutationProducesTypedInterferenceAndStaysPending()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CommandStep(0x2049),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var generation = continuation.PowerMutationGeneration;
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            () => AssertEx.True(
                                group.GroupReset().IsSuccess))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(continuation.IsPending);
                AssertEx.True(interference.Evidence
                    .InterveningMutationDetected);
                AssertEx.Equal(generation,
                    interference.Evidence.PowerMutationGeneration);
                AssertEx.True(
                    interference.Evidence.ObservedMutationGeneration
                        > generation);
                AssertEx.Equal(0, interference.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2049));
            }
        }

        private static void FinalProofWinsLateCancellation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
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
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(continuation.IsCompleted);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ZeroWireAndDifferentGroupDoNotInterfere()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(GroupReference),
                LookupStep(OtherGroupReference),
                PowerStep(true, true, GroupReference),
                StatusStep(PowerOn, groupReference: GroupReference),
                PowerStep(true, true, OtherGroupReference),
                StatusStep(PowerOn, groupReference: GroupReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var other = new LMCGroup(connection, OtherGroupName);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var generation = continuation.PowerMutationGeneration;

                var readOnly = group.WaitForPowerStateAsync(
                        true,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(readOnly.FinalStatus.IsPowerOn);
                AssertEx.Equal(generation,
                    continuation.ObservedMutationGeneration);
                AssertEx.Equal(0, continuation.StatusPollCount);
                AssertEx.True(other.GroupPowerOn().IsSuccess);
                AssertEx.Equal(generation,
                    continuation.ObservedMutationGeneration);

                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.False(result.Evidence
                    .InterveningMutationDetected);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 0, 2);
            }
        }

        private static void AcceptedNoStatusDeadlineFaultsTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(PowerOn);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000));
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    PowerStep(true, true),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var options = new LMCGroupPowerStateWaitOptions
                    {
                        TimeoutMilliseconds = 200,
                        PollIntervalMilliseconds = 10,
                        StableSampleCount = 1
                    };
                    var continuation = group
                        .BeginGroupPowerOnWaitForStableStateAsync(
                            options,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    LMCGroupPowerStateWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupPowerStateWaitTimeoutException>(
                            () => group
                                .ResumeGroupPowerStateWaitForStableStateAsync(
                                    continuation,
                                    options,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.True(ReferenceEquals(
                        continuation,
                        timeout.Continuation));
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    server.Verify();
                    AssertCommandCounts(server, 1, 0, 1);
                }
            }
        }

        private static void
            ReconnectBetweenAckParseAndPublicationCreatesNoPending()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var oldGroup = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var time = new FakeWaitTime();
                var exception = AssertEx.Throws<
                    LMCGroupPowerSubmissionException>(
                    () => oldGroup
                        .BeginGroupPowerStateWaitForStableStateAsync(
                            true,
                            SingleSampleOptions(1),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            null,
                            () => connection.RpcInitConnection(
                                "127.0.0.1",
                                secondServer.Port,
                                "127.0.0.1",
                                0,
                                LMCConnection.DefaultEventMask))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    LMCGroupPowerSubmissionOutcome.OutcomeUncertain,
                    exception.Evidence.SubmissionOutcome);
                AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                    null,
                    oldGroup.PendingGroupPowerStateWaitContinuation);

                var newGroup = new LMCGroup(connection, GroupName);
                AssertEx.True(newGroup.GroupReadStatusResult().IsPowerOn);
                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void
            CompoundObserverRunsBeforeFirstStatusForOnAndOff()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerCount = 0;
                var options = SingleSampleOptions(1);
                var result = group
                    .GroupPowerOnAndWaitForStableStateAsync(
                        options,
                        continuation =>
                        {
                            observerCount++;
                            AssertEx.True(continuation.IsPending);
                            AssertEx.Equal(0,
                                continuation.StatusPollCount);
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2045));
                            AssertEx.True(ReferenceEquals(
                                continuation,
                                group.PendingGroupPowerStateWaitContinuation));
                            options.StableSampleCount = 2;
                            options.PollIntervalMilliseconds = 0;
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(1, observerCount);
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.Equal(1, result.RequiredStableSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 1);
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(false, true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerCount = 0;
                var result = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        SingleSampleOptions(1),
                        continuation =>
                        {
                            observerCount++;
                            AssertEx.False(continuation.ExpectedPowerOn);
                            AssertEx.Equal(0,
                                CountCommand(server, 0x2045));
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(1, observerCount);
                AssertEx.False(result.FinalStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 1, 1);
            }
        }

        private static void
            NoCommandAcknowledgementDeadlineFaultsTransport()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedPower = PowerStep(true, true);
                blockedPower.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000));
                blockedPower.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blockedPower))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupPowerStateWaitTimeoutException timeout = null;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupPowerStateWaitTimeoutException>(
                            () => group
                                .BeginGroupPowerOnWaitForStableStateAsync(
                                    new LMCGroupPowerStateWaitOptions
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

                    AssertEx.Equal(
                        LMCGroupPowerSubmissionOutcome.OutcomeUncertain,
                        timeout.Evidence.SubmissionOutcome);
                    AssertEx.True(timeout.Evidence.CommandMayHaveBeenSent);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.Equal<LMCGroupPowerStateWaitContinuation>(
                        null,
                        timeout.Continuation);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    server.Verify();
                    AssertCommandCounts(server, 1, 0, 0);
                }
            }
        }

        private static void
            MutationBetweenAckParseAndContinuationPublicationIsAttributed()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var coordinator = connection.GetGroupEnableWaitCoordinator(
                    group.SessionGeneration,
                    group.GroupReference);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        true,
                        SingleSampleOptions(1),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        null,
                        () => coordinator.MarkMutationMayHaveBeenSent())
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(continuation.IsPending);
                AssertEx.True(
                    continuation.ObservedMutationGeneration
                        > continuation.PowerMutationGeneration);
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(interference.Evidence
                    .InterveningMutationDetected);
                AssertEx.Equal(0,
                    interference.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
            }
        }

        private static void
            EarlyCancellationAtFinalDecisionStaysPending()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var canceled = AssertEx.Throws<
                    LMCGroupPowerStateWaitCanceledException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
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

                AssertEx.True(ReferenceEquals(
                    continuation,
                    canceled.Continuation));
                AssertEx.Equal(1, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(1, canceled.Evidence.StableSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StableSampleCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 1);
            }
        }

        private static void StaleSessionResumeIsZeroWire()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var oldGroup = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var continuation = oldGroup
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var newGroup = new LMCGroup(connection, GroupName);

                AssertEx.Throws<InvalidOperationException>(
                    () => oldGroup
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(continuation.IsPending);
                AssertEx.True(newGroup.GroupReadStatusResult().IsPowerOn);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
                AssertCommandCounts(firstServer, 1, 0, 0);
                AssertCommandCounts(secondServer, 0, 0, 1);
            }
        }

        private static void InterferedPowerOffCanBeExplicitlyReplaced()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(false, true),
                CommandStep(0x2049),
                PowerStep(false, true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = SingleSampleOptions(1);
                var time = new FakeWaitTime();
                var first = group
                    .BeginGroupPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(group.GroupReset().IsSuccess);
                AssertEx.Throws<LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            first,
                            options,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                var replacement = group
                    .BeginGroupPowerOffWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(first.IsSuperseded);
                AssertEx.True(replacement.IsPending);
                AssertEx.Throws<LMCGroupPowerStateWaitPendingException>(
                    () => group.BeginGroupPowerOnWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<LMCGroupPowerStateWaitPendingException>(
                    () => group.GroupPowerOn());
                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        replacement,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(result.FinalStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 2, 1);
                AssertEx.Equal(1, CountCommand(server, 0x2049));
            }
        }

        private static void
            ObserverReentrantStatusAndSafetyPowerOffComplete()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                PowerStep(false, true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                LMCGroupReadStatusResult observerStatus = null;
                LMC_Response observerPowerOff = null;
                var begin = Task.Run(
                    () => group
                        .BeginGroupPowerOnWaitForStableStateAsync(
                            SingleSampleOptions(1),
                            accepted =>
                            {
                                observerStatus =
                                    group.GroupReadStatusResult();
                                observerPowerOff = group.GroupPowerOff();
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(begin.Wait(2000),
                    "The accepted observer reentry deadlocked.");
                var continuation = begin.GetAwaiter().GetResult();
                AssertEx.True(observerStatus.IsPowerOn);
                AssertEx.True(observerPowerOff.IsSuccess);
                AssertEx.True(continuation.IsPending);
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(interference.Evidence
                    .InterveningMutationDetected);
                AssertEx.Equal(0,
                    interference.Evidence.StatusPollCount);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1, 1);
            }
        }

        private static void
            EarlyAndLateDeadlineFinalDecisionIsLinearized()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = new LMCGroupPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 10,
                    PollIntervalMilliseconds = 1,
                    StableSampleCount = 1
                };
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        true,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                var timeout = AssertEx.Throws<
                    LMCGroupPowerStateWaitTimeoutException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            null,
                            null,
                            () => time.Advance(10))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(
                    continuation,
                    timeout.Continuation));
                AssertEx.Equal(1, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(1, timeout.Evidence.StableSampleCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StableSampleCount);

                connection.CloseConnection();
                server.Verify();
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = new LMCGroupPowerStateWaitOptions
                {
                    TimeoutMilliseconds = 10,
                    PollIntervalMilliseconds = 1,
                    StableSampleCount = 1
                };
                var time = new FakeWaitTime();
                var continuation = group
                    .BeginGroupPowerStateWaitForStableStateAsync(
                        true,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                var result = group
                    .ResumeGroupPowerStateWaitForStableStateAsync(
                        continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync,
                        null,
                        () => time.Advance(10))
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(continuation.IsCompleted);
                AssertEx.Equal(10L, result.ElapsedMilliseconds);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AdminSyncAndAsyncMoveProduceTypedInterference()
        {
            VerifyAdminMoveProducesTypedInterference(false);
            VerifyAdminMoveProducesTypedInterference(true);
        }

        private static void VerifyAdminMoveProducesTypedInterference(
            bool useAsync)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                AdminCapabilitiesStep(1),
                AdminMoveStep(2),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                LMCAdminResponse response;
                if (useAsync)
                {
                    response = connection.Admin
                        .GroupMoveLinearRelativeAsync(
                            group,
                            new[] { 1, 2, 3, 4 },
                            100,
                            200,
                            300,
                            0,
                            new LMCGroupMotionOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
                else
                {
                    response = connection.Admin.GroupMoveLinearRelative(
                        group,
                        new[] { 1, 2, 3, 4 },
                        100,
                        200,
                        300,
                        0,
                        new LMCGroupMotionOptions());
                }

                AssertEx.True(response.IsSuccess);
                var interference = AssertEx.Throws<
                    LMCGroupPowerInterferenceException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(interference.Evidence
                    .InterveningMutationDetected);
                AssertEx.Equal(0,
                    interference.Evidence.StatusPollCount);
                AssertEx.True(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x7D22));
            }
        }

        private static void
            ReconnectBeforeFinalStatusPublicationRejectsProof()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(true, true),
                StatusStep(PowerOn),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var options = SingleSampleOptions(1);
                var continuation = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var time = new FakeWaitTime();
                var statusFailure = AssertEx.Throws<
                    LMCGroupPowerStateStatusException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => connection.RpcInitConnection(
                                "127.0.0.1",
                                secondServer.Port,
                                "127.0.0.1",
                                0,
                                LMCConnection.DefaultEventMask))
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(
                    continuation,
                    statusFailure.Continuation));
                AssertEx.Equal(0,
                    statusFailure.Evidence.StatusPollCount);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, continuation.StatusPollCount);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void PendingEnableObservesAndResetsWithPowerPolling()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                PowerStep(true, true),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var enablePending = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => group
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongEnableOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                var powerOptions = LongOptions();
                var time = new FakeWaitTime();
                var power = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        powerOptions,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                group.ResumeGroupPowerStateWaitForStableStateAsync(
                        power,
                        powerOptions,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(3, enablePending.Continuation.PollCount);
                AssertEx.Equal(3,
                    enablePending.Continuation.StableSampleCount);
                AssertEx.True(enablePending.Continuation.IsPending);
                var enableResult = group
                    .ResumeGroupEnableWaitForLockedStandbyAsync(
                        enablePending.Continuation,
                        LongEnableOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(enableResult.FinalStatus.IsStandby);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 3);
            }

            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                PowerStep(true, true),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby, 0x0010, -31, 7),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var enablePending = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => group
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongEnableOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                var powerOptions = LongOptions();
                var time = new FakeWaitTime();
                var power = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        powerOptions,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var statusFailure = AssertEx.Throws<
                    LMCGroupPowerStateStatusException>(
                    () => group
                        .ResumeGroupPowerStateWaitForStableStateAsync(
                            power,
                            powerOptions,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.NotNull(statusFailure.FailedStatus);
                AssertEx.Equal(3, enablePending.Continuation.PollCount);
                AssertEx.Equal(0,
                    enablePending.Continuation.StableSampleCount);
                AssertEx.True(enablePending.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 3);
            }
        }

        private static void
            AcceptedPowerLeavesPendingStopWithTypedInterference()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StopStep(true),
                PowerStep(true, true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var stop = group
                    .BeginGroupStopWaitForStableStandbyAsync(
                        1000,
                        0,
                        LongStopOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var power = group
                    .BeginGroupPowerOnWaitForStableStateAsync(
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(power.IsPending);
                AssertEx.True(stop.IsPending);
                AssertEx.True(ReferenceEquals(
                    stop,
                    group.PendingGroupStopWaitContinuation));
                var interference = AssertEx.Throws<
                    LMCGroupStopInterferenceException>(
                    () => group
                        .ResumeGroupStopWaitForStableStandbyAsync(
                            stop,
                            LongStopOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(
                    stop,
                    interference.Continuation));
                AssertEx.Equal(0,
                    interference.Evidence.StatusPollCount);
                AssertEx.True(stop.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0, 0);
                AssertEx.Equal(1, CountCommand(server, 0x2085));
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

        private static LMCGroupPowerStateWaitOptions LongOptions()
        {
            return new LMCGroupPowerStateWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupPowerStateWaitOptions SingleSampleOptions(
            int stableSampleCount)
        {
            return new LMCGroupPowerStateWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 1,
                StableSampleCount = stableSampleCount
            };
        }

        private static LMCGroupEnableWaitOptions LongEnableOptions()
        {
            return new LMCGroupEnableWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupStopWaitOptions LongStopOptions()
        {
            return new LMCGroupStopWaitOptions
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

        private static FakeRpcStep LookupStep(
            ushort groupReference = GroupReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, groupReference);
            return new FakeRpcStep(0x1042, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep PowerStep(
            bool expectedPowerOn,
            bool success,
            ushort groupReference = GroupReference)
        {
            var command = expectedPowerOn ? (ushort)0x204A : (ushort)0x204B;
            return new FakeRpcStep(
                command,
                TestFrame.Response(
                    success ? (ushort)0 : (ushort)1,
                    new byte[8]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    expectedPowerOn
                        ? LMC_Frame.LMCGroupPowerOn(groupReference)
                        : LMC_Frame.LMCGroupPowerOff(groupReference),
                    request)
            };
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
                    LMC_Frame.LMCGroupStop(
                        GroupReference,
                        1000,
                        0),
                    request)
            };
        }

        private static FakeRpcStep CommandStep(ushort command)
        {
            return new FakeRpcStep(
                command,
                TestFrame.Response(0, new byte[8]));
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            var payload = AdminCommonPayload(requestId, 40);
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

        private static FakeRpcStep AdminMoveStep(uint requestId)
        {
            return new FakeRpcStep(
                0x7D22,
                TestFrame.Response(
                    0,
                    AdminCommonPayload(requestId, 16)))
            {
                InspectRequest = request => AssertEx.Equal(
                    GroupReference,
                    TestFrame.ReadUInt16(request, 6))
            };
        }

        private static byte[] AdminCommonPayload(
            uint requestId,
            int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep EnableStep(
            byte[] response,
            Action afterRequest = null)
        {
            return new FakeRpcStep(0x2047, response)
            {
                InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x2047,
                            GroupReference,
                            new byte[] { 1 }),
                        request);
                    if (afterRequest != null)
                    {
                        afterRequest();
                    }
                }
            };
        }

        private static FakeRpcStep StatusStep(
            uint state,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort groupErrorId = 0,
            Action afterRequest = null,
            ushort groupReference = GroupReference)
        {
            return new FakeRpcStep(
                0x2045,
                GroupStatusResponse(
                    state,
                    functionStatus,
                    errorId,
                    groupErrorId))
            {
                InspectRequest = request =>
                {
                    AssertStatusRequest(request, groupReference);
                    if (afterRequest != null)
                    {
                        afterRequest();
                    }
                }
            };
        }

        private static byte[] GroupStatusResponse(
            uint state,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort groupErrorId = 0)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, groupErrorId);
            return TestFrame.Response(0, payload);
        }

        private static void AssertStatusRequest(
            byte[] request,
            ushort groupReference = GroupReference)
        {
            var payload = new byte[8];
            TestFrame.WriteInt32(payload, 0, groupReference);
            TestFrame.WriteInt32(payload, 4, 1);
            AssertEx.SequenceEqual(
                TestFrame.Request(0x2045, groupReference, payload),
                request);
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static byte[] SuccessLongAcknowledgement()
        {
            return TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00 00 00 00 00"));
        }

        private static void AssertOnlyStatusWaitCommands(
            FakeRpcServer server,
            int expectedStatusCount)
        {
            var statusCount = 0;
            foreach (var request in server.ReceivedRequests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                AssertEx.True(command != 0x204A && command != 0x204B);
                if (command == 0x2045)
                {
                    statusCount++;
                }
            }

            AssertEx.Equal(expectedStatusCount, statusCount);
        }

        private static void AssertCommandCounts(
            FakeRpcServer server,
            int expectedPowerOnCount,
            int expectedPowerOffCount,
            int expectedStatusCount)
        {
            AssertEx.Equal(
                expectedPowerOnCount,
                CountCommand(server, 0x204A));
            AssertEx.Equal(
                expectedPowerOffCount,
                CountCommand(server, 0x204B));
            AssertEx.Equal(
                expectedStatusCount,
                CountCommand(server, 0x2045));
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
    }
}

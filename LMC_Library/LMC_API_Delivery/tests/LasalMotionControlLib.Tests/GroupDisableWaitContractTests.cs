using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupDisableWaitContractTests
    {
        private const string GroupName = "_LMCRobotBase1";
        private const ushort GroupReference = 0x0100;
        private const uint PowerOnDisabled = 0x00050000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "GroupDisableWait.Compound.OneDisableThenThreeStatuses",
                CompoundSendsOneDisableThenThreeStatuses);
            tests.Add(
                "GroupDisableWait.Observer.BeforeFirstStatus",
                AcceptedObserverRunsBeforeFirstStatus);
            tests.Add(
                "GroupDisableWait.Observer.ConcurrentResumeIsZeroWire",
                ConcurrentResumeDuringObserverIsZeroWire);
            tests.Add(
                "GroupDisableWait.Resume.TimeoutDoesNotReplayDisable",
                TimeoutThenResumeIsStatusOnly);
            tests.Add(
                "GroupDisableWait.Resume.CancelDoesNotReplayDisable",
                CancellationThenResumeIsStatusOnly);
            tests.Add(
                "GroupDisableWait.Linearization.LateFinalDeadlineStaysPending",
                LateFinalDeadlineStaysPending);
            tests.Add(
                "GroupDisableWait.Linearization.LateFinalCancelStaysPending",
                LateFinalCancellationStaysPending);
            tests.Add(
                "GroupDisableWait.Pending.RawDisableIsZeroWire",
                RawDisablePendingGuardIsZeroWire);
            tests.Add(
                "GroupDisableWait.PowerOff.IsInterferenceNotCompletion",
                PowerOffIsInterferenceNotCompletion);
            tests.Add(
                "GroupDisableWait.Retire.StablePowerOffExactProof",
                StablePowerOffExactProofRetiresPendingDisable);
            tests.Add(
                "GroupDisableWait.Retire.ObserverRaceRequiresRetry",
                StablePowerOffDuringObserverRequiresRetireRetry);
            tests.Add(
                "GroupDisableWait.Retire.ActiveResumeRaceRequiresRetry",
                StablePowerOffDuringResumeRequiresRetireRetry);
            tests.Add(
                "GroupDisableWait.Retire.OlderProofIsZeroStateChange",
                OlderPowerOffProofDoesNotRetireNewerDisable);
            tests.Add(
                "GroupDisableWait.Retire.ForeignProofIsZeroStateChange",
                ForeignPowerOffProofDoesNotRetirePendingDisable);
            tests.Add(
                "GroupDisableWait.Retire.CompletedDisableIsZeroStateChange",
                StablePowerOffDoesNotRetireCompletedDisable);
            tests.Add(
                "GroupDisableWait.Retire.StaleSessionIsZeroStateChange",
                StablePowerOffDoesNotRetireStaleSessionDisable);
            tests.Add(
                "GroupDisableWait.StatusOnly.ThreeStatusesAndZeroDisable",
                StatusOnlyWaitSendsNoDisable);
            tests.Add(
                "GroupDisableWait.StatusOnly.LateFinalDeadlineIsNotSuccess",
                StatusOnlyLateFinalDeadlineIsNotSuccess);
            tests.Add(
                "GroupDisableWait.StatusOnly.LateFinalCancelIsNotSuccess",
                StatusOnlyLateFinalCancellationIsNotSuccess);
            tests.Add(
                "GroupDisableWait.StatusOnly.PostWriteDeadlineInvalidatesTransport",
                StatusOnlyPostWriteDeadlineInvalidatesTransport);
            tests.Add(
                "GroupDisableWait.Publication.CloseRejectsStaleContinuation",
                CloseBeforeAcceptedPublicationRejectsStaleContinuation);
            tests.Add(
                "GroupDisableWait.Publication.DeadlinePreservesAcceptedContinuation",
                AcceptedPublicationDeadlinePreservesContinuation);
            tests.Add(
                "GroupDisableWait.Publication.CancelPreservesAcceptedContinuation",
                AcceptedPublicationCancellationPreservesContinuation);
            tests.Add(
                "GroupDisableWait.Compound.ResumeOwnershipIsReserved",
                CompoundResumeOwnershipIsReserved);
            tests.Add(
                "GroupDisableWait.Resume.ConcurrentSecondIsZeroWire",
                ConcurrentResumeSecondIsZeroWire);
            tests.Add(
                "GroupDisableWait.Resume.PostWriteDeadlineInvalidatesTransport",
                ResumePostWriteDeadlineInvalidatesTransport);
            tests.Add(
                "GroupDisableWait.Resume.CloseRejectsStaleFinalSample",
                ResumeCloseRejectsStaleFinalSample);
            tests.Add(
                "GroupDisableWait.Linearization.CompletionGapBlocksNewDisable",
                CompletionGapBlocksNewDisable);
            tests.Add(
                "GroupDisableWait.StatusOnly.CloseRejectsStaleFinalSample",
                StatusOnlyCloseRejectsStaleFinalSample);
            tests.Add(
                "GroupDisableWait.StatusOnly.FinalPublicationWinsLateCloseAndCancel",
                StatusOnlyFinalPublicationWinsLateCloseAndCancel);
        }

        private static void CompoundSendsOneDisableThenThreeStatuses()
        {
            var defaults = new LMCGroupDisableWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var result = group
                    .GroupDisableAndWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.GroupDisableAccepted);
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsDisabled);
                AssertEx.False(result.FinalStatus.IsStandby);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableDisabledSampleCount);
                AssertEx.True(result.Continuation.IsCompleted);
                AssertEx.Equal(
                    null,
                    group.PendingGroupDisableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void AcceptedObserverRunsBeforeFirstStatus()
        {
            var observerCalls = 0;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(
                    PowerOnDisabled,
                    () => AssertEx.Equal(1, observerCalls)),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var result = group
                    .GroupDisableAndWaitForStableDisabledAsync(
                        FastOptions(),
                        continuation =>
                        {
                            observerCalls++;
                            AssertEx.True(continuation.IsPending);
                            AssertEx.Equal(
                                continuation,
                                group.PendingGroupDisableWaitContinuation);
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCalls);
                AssertEx.True(result.Continuation.IsCompleted);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void ConcurrentResumeDuringObserverIsZeroWire()
        {
            using (var observerEntered = new ManualResetEventSlim(false))
            using (var releaseObserver = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                LMCGroupDisableWaitContinuation accepted = null;
                var compound = group
                    .GroupDisableAndWaitForStableDisabledAsync(
                        FastOptions(),
                        continuation =>
                        {
                            accepted = continuation;
                            observerEntered.Set();
                            AssertEx.True(
                                releaseObserver.Wait(5000),
                                "The accepted observer was not released.");
                        },
                        CancellationToken.None);

                AssertEx.True(
                    observerEntered.Wait(5000),
                    "The accepted observer did not start.");
                var pending = AssertEx.Throws<
                    LMCGroupDisableWaitPendingException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            accepted,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(accepted, pending.Continuation);
                AssertCommandCounts(server, 1, 0);

                releaseObserver.Set();
                var result = compound.GetAwaiter().GetResult();
                AssertEx.True(result.Continuation.IsCompleted);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void TimeoutThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var timeoutClock = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupDisableWaitTimeoutException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 10,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            timeoutClock.ElapsedMilliseconds,
                            timeoutClock.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.True(continuation.IsPending);

                var resumeClock = new FakeWaitTime();
                var result = group
                    .ResumeGroupDisableWaitForStableDisabledAsync(
                        continuation,
                        FastOptions(),
                        CancellationToken.None,
                        resumeClock.ElapsedMilliseconds,
                        resumeClock.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.Continuation.IsCompleted);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
            }
        }

        private static void CancellationThenResumeIsStatusOnly()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.Cancel();
                    var canceled = AssertEx.Throws<
                        LMCGroupDisableWaitCanceledException>(
                        () => group
                            .ResumeGroupDisableWaitForStableDisabledAsync(
                                continuation,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(continuation, canceled.Continuation);
                }

                var clock = new FakeWaitTime();
                var result = group
                    .ResumeGroupDisableWaitForStableDisabledAsync(
                        continuation,
                        FastOptions(),
                        CancellationToken.None,
                        clock.ElapsedMilliseconds,
                        clock.DelayAsync)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.Continuation.IsCompleted);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void LateFinalDeadlineStaysPending()
        {
            using (var server = StableAcceptedServer())
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var clock = new FakeWaitTime();
                var publications = 0;
                var timeout = AssertEx.Throws<
                    LMCGroupDisableWaitTimeoutException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 100,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () =>
                            {
                                publications++;
                                if (publications == 3)
                                {
                                    clock.Advance(100);
                                }
                            })
                        .GetAwaiter().GetResult());

                AssertEx.Equal(continuation, timeout.Continuation);
                AssertEx.Equal(3, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(3, timeout.Evidence.StableDisabledSampleCount);
                AssertEx.True(timeout.Evidence.IsPending);
                AssertEx.True(timeout.ElapsedMilliseconds >= 100);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(
                    continuation.DisableMutationGeneration,
                    continuation.ObservedMutationGeneration);
                AssertEx.False(continuation.InterveningMutationDetected);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void LateFinalCancellationStaysPending()
        {
            using (var server = StableAcceptedServer())
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var clock = new FakeWaitTime();
                var publications = 0;
                var canceled = AssertEx.Throws<
                    LMCGroupDisableWaitCanceledException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            FastOptions(),
                            cancellation.Token,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () =>
                            {
                                publications++;
                                if (publications == 3)
                                {
                                    cancellation.Cancel();
                                }
                            })
                        .GetAwaiter().GetResult());

                AssertEx.Equal(continuation, canceled.Continuation);
                AssertEx.Equal(3, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(3, canceled.Evidence.StableDisabledSampleCount);
                AssertEx.True(canceled.Evidence.IsPending);
                AssertEx.True(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void RawDisablePendingGuardIsZeroWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                var pending = AssertEx.Throws<
                    LMCGroupDisableWaitPendingException>(
                    () => group.GroupDisable());
                AssertEx.Equal(continuation, pending.Continuation);
                var pendingAsync = AssertEx.Throws<
                    LMCGroupDisableWaitPendingException>(
                    () => group.GroupDisableAsync(CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(continuation, pendingAsync.Continuation);
                var pendingBegin = AssertEx.Throws<
                    LMCGroupDisableWaitPendingException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            FastOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(continuation, pendingBegin.Continuation);
                AssertEx.True(continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void PowerOffIsInterferenceNotCompletion()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var clock = new FakeWaitTime();
                var interference = AssertEx.Throws<
                    LMCGroupDisableInterferenceException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            FastOptions(),
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(continuation, interference.Continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.False(
                    interference.Evidence.LastObservedStatus.IsPowerOn);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void StablePowerOffExactProofRetiresPendingDisable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var disable = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var powerOff = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var powerOffContinuation = powerOff.Continuation;

                AssertEx.True(powerOffContinuation != null);
                AssertEx.True(powerOffContinuation.IsCompleted);
                AssertEx.False(powerOffContinuation.ExpectedPowerOn);
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;
                AssertEx.True(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        powerOffContinuation));
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);
                AssertEx.False(disable.IsPending);
                AssertEx.False(disable.IsCompleted);
                AssertEx.True(disable.IsSuperseded);
                AssertEx.Equal(
                    LMCGroupDisableWaitContinuationState
                        .SupersededByStablePowerOff,
                    disable.State);
                AssertEx.Equal(
                    powerOffContinuation,
                    disable.SupersedingPowerOffContinuation);
                AssertEx.Equal(
                    null,
                    group.PendingGroupDisableWaitContinuation);

                AssertEx.Throws<LMCGroupDisableWaitResolvedException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            disable,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);
                AssertEx.True(group.GroupDisable().IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 3);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void StablePowerOffDuringObserverRequiresRetireRetry()
        {
            using (var observerEntered = new ManualResetEventSlim(false))
            using (var releaseObserver = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                LMCGroupDisableWaitContinuation disable = null;
                var begin = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        continuation =>
                        {
                            disable = continuation;
                            observerEntered.Set();
                            AssertEx.True(
                                releaseObserver.Wait(5000),
                                "The Disable observer was not released.");
                        },
                        CancellationToken.None);
                AssertEx.True(
                    observerEntered.Wait(5000),
                    "The Disable observer did not start.");

                var powerOff = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;
                AssertEx.False(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        powerOff.Continuation));
                AssertEx.True(disable.IsPending);
                AssertEx.Equal(
                    disable,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);

                releaseObserver.Set();
                AssertEx.Equal(disable, begin.GetAwaiter().GetResult());
                AssertEx.True(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        powerOff.Continuation));
                AssertEx.True(disable.IsSuperseded);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void StablePowerOffDuringResumeRequiresRetireRetry()
        {
            using (var blockingDelay = new BlockingWaitDelay())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(0x00040000u),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var disable = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var clock = new FakeWaitTime();
                var resume = group
                    .ResumeGroupDisableWaitForStableDisabledAsync(
                        disable,
                        FastOptions(),
                        CancellationToken.None,
                        clock.ElapsedMilliseconds,
                        blockingDelay.DelayAsync);
                AssertEx.True(
                    blockingDelay.Entered.Wait(5000),
                    "The Disable resume delay did not start.");

                var powerOff = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;
                AssertEx.False(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        powerOff.Continuation));
                AssertEx.True(disable.IsPending);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);

                blockingDelay.Release();
                AssertEx.Throws<LMCGroupDisableInterferenceException>(
                    () => resume.GetAwaiter().GetResult());
                AssertEx.True(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        powerOff.Continuation));
                AssertEx.True(disable.IsSuperseded);
                AssertEx.Equal(
                    null,
                    group.PendingGroupDisableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 4);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void OlderPowerOffProofDoesNotRetireNewerDisable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var olderPowerOff = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var disable = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;

                AssertEx.False(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        olderPowerOff.Continuation));
                AssertEx.True(disable.IsPending);
                AssertEx.False(disable.IsSuperseded);
                AssertEx.Equal(
                    disable,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void ForeignPowerOffProofDoesNotRetirePendingDisable()
        {
            const ushort otherGroupReference = 0x0101;
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                LookupStep(otherGroupReference),
                PowerStep(false, true, otherGroupReference),
                StatusStep(0, null, otherGroupReference),
                StatusStep(0, null, otherGroupReference),
                StatusStep(0, null, otherGroupReference),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var disable = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var otherGroup = new LMCGroup(connection, "_OtherGroup");
                var foreignPowerOff = otherGroup
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;

                AssertEx.False(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        disable,
                        foreignPowerOff.Continuation));
                AssertEx.False(
                    otherGroup
                        .TryRetirePendingGroupDisableAfterStablePowerOff(
                            disable,
                            foreignPowerOff.Continuation));
                AssertEx.True(disable.IsPending);
                AssertEx.False(disable.IsSuperseded);
                AssertEx.Equal(
                    disable,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void StablePowerOffDoesNotRetireCompletedDisable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var completedDisable = group
                    .GroupDisableAndWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult().Continuation;
                var powerOff = group
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    server.ReceivedRequests.Count;

                AssertEx.False(
                    group.TryRetirePendingGroupDisableAfterStablePowerOff(
                        completedDisable,
                        powerOff.Continuation));
                AssertEx.True(completedDisable.IsCompleted);
                AssertEx.False(completedDisable.IsSuperseded);
                AssertEx.Equal(
                    LMCGroupDisableWaitContinuationState.Completed,
                    completedDisable.State);
                AssertEx.Equal(
                    null,
                    completedDisable.SupersedingPowerOffContinuation);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    server.ReceivedRequests.Count);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 6);
                AssertEx.Equal(1, CountCommand(server, 0x204B));
            }
        }

        private static void StablePowerOffDoesNotRetireStaleSessionDisable()
        {
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                PowerStep(false, true),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var staleGroup = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var staleDisable = staleGroup
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                connection.CloseConnection();
                firstServer.Verify();

                var currentGroup = ConnectAndCreateGroup(
                    connection,
                    secondServer.Port);
                var currentPowerOff = currentGroup
                    .GroupPowerOffAndWaitForStableStateAsync(
                        FastPowerOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var requestCountBeforeRetire =
                    secondServer.ReceivedRequests.Count;

                AssertEx.False(
                    currentGroup
                        .TryRetirePendingGroupDisableAfterStablePowerOff(
                            staleDisable,
                            currentPowerOff.Continuation));
                AssertEx.True(staleDisable.IsPending);
                AssertEx.False(staleDisable.IsSuperseded);
                AssertEx.Equal(
                    null,
                    currentGroup.PendingGroupDisableWaitContinuation);
                AssertEx.Equal(
                    requestCountBeforeRetire,
                    secondServer.ReceivedRequests.Count);

                connection.CloseConnection();
                secondServer.Verify();
                AssertCommandCounts(firstServer, 1, 0);
                AssertCommandCounts(secondServer, 0, 3);
                AssertEx.Equal(1, CountCommand(secondServer, 0x204B));
            }
        }

        private static void StatusOnlyWaitSendsNoDisable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var result = group.WaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsDisabled);
                AssertEx.False(result.FinalStatus.IsStandby);
                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.Equal(3, result.StableDisabledSampleCount);
                AssertEx.False(result.TransportInvalidatedAtDeadline);
                AssertEx.True(result.ElapsedMilliseconds >= 0);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 3);
            }
        }

        private static void StatusOnlyLateFinalDeadlineIsNotSuccess()
        {
            using (var server = StableStatusOnlyServer())
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var publications = 0;
                var timeout = AssertEx.Throws<
                    LMCGroupStableDisabledWaitTimeoutException>(
                    () => group.WaitForStableDisabledAsync(
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 100,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () =>
                            {
                                publications++;
                                if (publications == 3)
                                {
                                    clock.Advance(100);
                                }
                            })
                        .GetAwaiter().GetResult());

                AssertEx.Equal(3, timeout.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    timeout.Evidence.StableDisabledSampleCount);
                AssertEx.False(
                    timeout.Evidence.TransportInvalidatedAtDeadline);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 3);
            }
        }

        private static void StatusOnlyLateFinalCancellationIsNotSuccess()
        {
            using (var server = StableStatusOnlyServer())
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var publications = 0;
                var canceled = AssertEx.Throws<
                    LMCGroupStableDisabledWaitCanceledException>(
                    () => group.WaitForStableDisabledAsync(
                            FastOptions(),
                            cancellation.Token,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () =>
                            {
                                publications++;
                                if (publications == 3)
                                {
                                    cancellation.Cancel();
                                }
                            })
                        .GetAwaiter().GetResult());

                AssertEx.Equal(3, canceled.Evidence.StatusPollCount);
                AssertEx.Equal(
                    3,
                    canceled.Evidence.StableDisabledSampleCount);
                AssertEx.False(
                    canceled.Evidence.TransportInvalidatedAtDeadline);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 0, 3);
            }
        }

        private static void StatusOnlyPostWriteDeadlineInvalidatesTransport()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var delayedStatus = StatusStep(
                    PowerOnDisabled,
                    () => statusReceived.Set());
                delayedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The delayed status response was not released.");
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    delayedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var wait = group.WaitForStableDisabledAsync(
                        new LMCGroupDisableWaitOptions
                        {
                            TimeoutMilliseconds = 100,
                            PollIntervalMilliseconds = 10,
                            StableSampleCount = 3
                        },
                        CancellationToken.None);
                    AssertEx.True(
                        statusReceived.Wait(5000),
                        "The status request did not reach the server.");
                    LMCGroupStableDisabledWaitTimeoutException timeout;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupStableDisabledWaitTimeoutException>(
                            () => wait.GetAwaiter().GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.True(
                        timeout.Evidence.TransportInvalidatedAtDeadline);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    server.Verify();
                    AssertCommandCounts(server, 0, 1);
                }
            }
        }

        private static void CloseBeforeAcceptedPublicationRejectsStaleContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var failure = AssertEx.Throws<
                    LMCGroupDisableSubmissionException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            FastOptions(),
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            () => connection.CloseConnection())
                        .GetAwaiter().GetResult());

                AssertEx.True(failure.Evidence.CommandMayHaveBeenSent);
                AssertEx.Equal(
                    null,
                    group.PendingGroupDisableWaitContinuation);
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void AcceptedPublicationDeadlinePreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupDisableWaitTimeoutException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 100,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 3
                            },
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            () => clock.Advance(100))
                        .GetAwaiter().GetResult());

                AssertEx.True(timeout.Evidence.GroupDisableAccepted);
                AssertEx.True(timeout.Evidence.CommandMayHaveBeenSent);
                AssertEx.True(timeout.Evidence.IsPending);
                AssertEx.Equal(timeout.Continuation,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.True(timeout.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void AcceptedPublicationCancellationPreservesContinuation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var canceled = AssertEx.Throws<
                    LMCGroupDisableWaitCanceledException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            FastOptions(),
                            cancellation.Token,
                            clock.ElapsedMilliseconds,
                            () => cancellation.Cancel())
                        .GetAwaiter().GetResult());

                AssertEx.True(canceled.Evidence.GroupDisableAccepted);
                AssertEx.True(canceled.Evidence.CommandMayHaveBeenSent);
                AssertEx.True(canceled.Evidence.IsPending);
                AssertEx.Equal(canceled.Continuation,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.True(canceled.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 0);
            }
        }

        private static void CompoundResumeOwnershipIsReserved()
        {
            using (var gapEntered = new ManualResetEventSlim(false))
            using (var releaseGap = new ManualResetEventSlim(false))
            using (var server = StableAcceptedServer())
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var compound = group
                    .GroupDisableAndWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None,
                        clock.ElapsedMilliseconds,
                        clock.DelayAsync,
                        () =>
                        {
                            gapEntered.Set();
                            AssertEx.True(
                                releaseGap.Wait(5000),
                                "The compound handoff was not released.");
                        });
                AssertEx.True(
                    gapEntered.Wait(5000),
                    "The compound handoff gap was not reached.");
                var pending = group.PendingGroupDisableWaitContinuation;
                AssertEx.Throws<InvalidOperationException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            pending,
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertCommandCounts(server, 1, 0);

                releaseGap.Set();
                var result = compound.GetAwaiter().GetResult();
                AssertEx.True(result.Continuation.IsCompleted);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void ConcurrentResumeSecondIsZeroWire()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var delayedStatus = StatusStep(
                    PowerOnDisabled,
                    () => statusReceived.Set());
                delayedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The delayed status response was not released.");
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    DisableStep(true),
                    delayedStatus,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 5000,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 1
                            },
                            CancellationToken.None)
                        .GetAwaiter().GetResult();
                    var firstResume = group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 5000,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 1
                            },
                            CancellationToken.None);
                    AssertEx.True(
                        statusReceived.Wait(5000),
                        "The first resume status request did not arrive.");

                    AssertEx.Throws<InvalidOperationException>(
                        () => group
                            .ResumeGroupDisableWaitForStableDisabledAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter().GetResult());
                    AssertCommandCounts(server, 1, 1);

                    releaseResponse.Set();
                    var result = firstResume.GetAwaiter().GetResult();
                    AssertEx.True(result.Continuation.IsCompleted);
                    connection.CloseConnection();
                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void ResumePostWriteDeadlineInvalidatesTransport()
        {
            using (var statusReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var delayedStatus = StatusStep(
                    PowerOnDisabled,
                    () => statusReceived.Set());
                delayedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The delayed status response was not released.");
                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    DisableStep(true),
                    delayedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var continuation = group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            FastOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult();
                    var resume = group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 100,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None);
                    AssertEx.True(
                        statusReceived.Wait(5000),
                        "The status request did not reach the server.");

                    LMCGroupDisableWaitTimeoutException timeout;
                    try
                    {
                        timeout = AssertEx.Throws<
                            LMCGroupDisableWaitTimeoutException>(
                            () => resume.GetAwaiter().GetResult());
                    }
                    finally
                    {
                        releaseResponse.Set();
                    }

                    AssertEx.Equal(continuation, timeout.Continuation);
                    AssertEx.True(timeout.TransportInvalidatedAtDeadline);
                    AssertEx.True(timeout.Evidence.IsPending);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    server.Verify();
                    AssertCommandCounts(server, 1, 1);
                }
            }
        }

        private static void ResumeCloseRejectsStaleFinalSample()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = new LMCGroupDisableWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 1,
                    StableSampleCount = 1
                };
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        options,
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var clock = new FakeWaitTime();
                var failure = AssertEx.Throws<
                    LMCGroupDisableStatusException>(
                    () => group
                        .ResumeGroupDisableWaitForStableDisabledAsync(
                            continuation,
                            options,
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () => connection.CloseConnection())
                        .GetAwaiter().GetResult());

                AssertEx.Equal(continuation, failure.Continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(continuation.IsCompleted);
                AssertEx.Equal(0, failure.Evidence.StatusPollCount);
                server.Verify();
                AssertCommandCounts(server, 1, 1);
            }
        }

        private static void CompletionGapBlocksNewDisable()
        {
            using (var completionPublished = new ManualResetEventSlim(false))
            using (var releaseCompletion = new ManualResetEventSlim(false))
            using (var server = StableAcceptedServer())
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var continuation = group
                    .BeginGroupDisableWaitForStableDisabledAsync(
                        FastOptions(),
                        CancellationToken.None)
                    .GetAwaiter().GetResult();
                var clock = new FakeWaitTime();
                var resume = group
                    .ResumeGroupDisableWaitForStableDisabledAsync(
                        continuation,
                        FastOptions(),
                        CancellationToken.None,
                        clock.ElapsedMilliseconds,
                        clock.DelayAsync,
                        null,
                        () =>
                        {
                            if (continuation.IsCompleted)
                            {
                                completionPublished.Set();
                                AssertEx.True(
                                    releaseCompletion.Wait(5000),
                                    "The completion gap was not released.");
                            }
                        });

                AssertEx.True(
                    completionPublished.Wait(5000),
                    "The final completion was not published.");
                AssertEx.Equal(
                    null,
                    group.PendingGroupDisableWaitContinuation);
                AssertEx.Throws<InvalidOperationException>(
                    () => group.GroupDisable());
                AssertEx.Throws<InvalidOperationException>(
                    () => group.GroupDisableAsync(CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertEx.Throws<InvalidOperationException>(
                    () => group
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            FastOptions(),
                            CancellationToken.None)
                        .GetAwaiter().GetResult());
                AssertCommandCounts(server, 1, 3);

                releaseCompletion.Set();
                var result = resume.GetAwaiter().GetResult();
                AssertEx.True(result.Continuation.IsCompleted);
                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void StatusOnlyCloseRejectsStaleFinalSample()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                AssertEx.Throws<LMCGroupStableDisabledStatusException>(
                    () => group.WaitForStableDisabledAsync(
                            new LMCGroupDisableWaitOptions
                            {
                                TimeoutMilliseconds = 1000,
                                PollIntervalMilliseconds = 1,
                                StableSampleCount = 1
                            },
                            CancellationToken.None,
                            clock.ElapsedMilliseconds,
                            clock.DelayAsync,
                            () => connection.CloseConnection())
                        .GetAwaiter().GetResult());

                server.Verify();
                AssertCommandCounts(server, 0, 1);
            }
        }

        private static void StatusOnlyFinalPublicationWinsLateCloseAndCancel()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOnDisabled),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var clock = new FakeWaitTime();
                var result = group.WaitForStableDisabledAsync(
                        new LMCGroupDisableWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 1,
                            StableSampleCount = 1
                        },
                        cancellation.Token,
                        clock.ElapsedMilliseconds,
                        clock.DelayAsync,
                        null,
                        () =>
                        {
                            connection.CloseConnection();
                            cancellation.Cancel();
                            clock.Advance(1000);
                        })
                    .GetAwaiter().GetResult();

                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsDisabled);
                AssertEx.Equal(1, result.StatusPollCount);
                AssertEx.Equal(1, result.StableDisabledSampleCount);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);
                server.Verify();
                AssertCommandCounts(server, 0, 1);
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

        private static FakeRpcServer StableAcceptedServer()
        {
            return new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                DisableStep(true),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep());
        }

        private static FakeRpcServer StableStatusOnlyServer()
        {
            return new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                StatusStep(PowerOnDisabled),
                CloseStep());
        }

        private static LMCGroupDisableWaitOptions FastOptions()
        {
            return new LMCGroupDisableWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 1,
                StableSampleCount = 3
            };
        }

        private static LMCGroupPowerStateWaitOptions FastPowerOptions()
        {
            return new LMCGroupPowerStateWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 1,
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

        private static FakeRpcStep DisableStep(bool success)
        {
            return new FakeRpcStep(
                0x2048,
                TestFrame.Response(
                    success ? (ushort)0 : (ushort)1,
                    new byte[8]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    LMC_Frame.LMCGroupDisable(GroupReference),
                    request)
            };
        }

        private static FakeRpcStep PowerStep(
            bool expectedPowerOn,
            bool success,
            ushort groupReference = GroupReference)
        {
            return new FakeRpcStep(
                expectedPowerOn ? (ushort)0x204A : (ushort)0x204B,
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

        private static FakeRpcStep StatusStep(
            uint state,
            Action inspectRequest = null,
            ushort groupReference = GroupReference)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        LMC_Frame.LMCGroupReadStatus(groupReference),
                        request);
                    if (inspectRequest != null)
                    {
                        inspectRequest();
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
            int expectedDisableCount,
            int expectedStatusCount)
        {
            var disableCount = 0;
            var statusCount = 0;
            foreach (var request in server.ReceivedRequests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                if (command == 0x2048)
                {
                    disableCount++;
                }
                else if (command == 0x2045)
                {
                    statusCount++;
                }
            }
            AssertEx.Equal(expectedDisableCount, disableCount);
            AssertEx.Equal(expectedStatusCount, statusCount);
        }

        private static int CountCommand(
            FakeRpcServer server,
            ushort expectedCommand)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (TestFrame.ReadUInt16(request, 0) == expectedCommand)
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

        private sealed class BlockingWaitDelay : IDisposable
        {
            private readonly TaskCompletionSource<bool> completion =
                new TaskCompletionSource<bool>();

            internal BlockingWaitDelay()
            {
                Entered = new ManualResetEventSlim(false);
            }

            internal ManualResetEventSlim Entered { get; private set; }

            internal Task DelayAsync(
                int delayMilliseconds,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Entered.Set();
                return completion.Task;
            }

            internal void Release()
            {
                completion.TrySetResult(true);
            }

            public void Dispose()
            {
                Release();
                Entered.Dispose();
            }
        }
    }
}

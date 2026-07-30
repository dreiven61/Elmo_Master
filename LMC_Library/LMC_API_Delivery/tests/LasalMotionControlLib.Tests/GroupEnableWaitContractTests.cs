using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupEnableWaitContractTests
    {
        private const string GroupName = "_LMCRobotBase1";
        private const ushort GroupReference = 0x0100;
        private const uint PowerOn = 0x00040000u;
        private const uint Standby = 0x00020000u;
        private const uint Disabled = 0x00010000u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "GroupEnableWait.Success.OneEnableAndThreeStablePolls",
                OneEnableAndThreeStablePolls);
            tests.Add(
                "GroupEnableWait.AcceptedObserver.BeginOnceBeforeStatusResumeZeroReplay",
                AcceptedObserverBeginOnceBeforeStatusResumeZeroReplay);
            tests.Add(
                "GroupEnableWait.AcceptedObserver.CompoundOnceBeforeFirstStatus",
                AcceptedObserverCompoundOnceBeforeFirstStatus);
            tests.Add(
                "GroupEnableWait.AcceptedObserver.ExceptionPreservesContinuation",
                AcceptedObserverExceptionPreservesContinuation);
            tests.Add(
                "GroupEnableWait.LockedStandby.StatusOnlyStableEvidenceZeroEnable",
                LockedStandbyStatusOnlyStableEvidenceZeroEnable);
            tests.Add(
                "GroupEnableWait.LockedStandby.TimeoutAndCancelTypedZeroEnable",
                LockedStandbyTimeoutAndCancelTypedZeroEnable);
            tests.Add(
                "GroupEnableWait.Timeout.ResumeReusesAcceptedAck",
                TimeoutResumeReusesAcceptedAcknowledgement);
            tests.Add(
                "GroupEnableWait.Cancel.ResumeReusesAcceptedAck",
                CancellationResumeReusesAcceptedAcknowledgement);
            tests.Add(
                "GroupEnableWait.Release.DisabledProofAllowsNewEnable",
                DisabledProofAllowsNewEnable);
            tests.Add(
                "GroupEnableWait.Release.PowerOffProofRequiresStableStatus",
                PowerOffProofRequiresStableStatus);
            tests.Add(
                "GroupEnableWait.Release.DisableAckContract",
                DisableAcknowledgementContract);
            tests.Add(
                "GroupEnableWait.Validation.AndRejectedAckAreZeroPoll",
                ValidationAndRejectedAcknowledgementAreZeroPoll);
            tests.Add(
                "GroupEnableWait.StatusError.PreservesEvidence",
                StatusErrorPreservesEvidence);
            tests.Add(
                "GroupEnableWait.Guard.LegacyEnableBlockedWhileWaitActive",
                LegacyEnableBlockedWhileWaitActive);
            tests.Add(
                "GroupEnableWait.Timeout.LateTerminalStatusDoesNotWin",
                LateTerminalStatusDoesNotWinTimeout);
            tests.Add(
                "GroupEnableWait.Scope.SameConnectionHandlesShareContinuation",
                SameConnectionHandlesShareContinuation);
            tests.Add(
                "GroupEnableWait.Race.PreAckObservationCannotContaminate",
                PreAckObservationCannotContaminate);
            tests.Add(
                "GroupEnableWait.Race.ConcurrentStatusUsesWireOrder",
                ConcurrentStatusUsesWireOrder);
            tests.Add(
                "GroupEnableWait.Race.ManualThirdStatusDiscardedAfterPriorityReservation",
                ManualThirdStatusDiscardedAfterPriorityReservation);
            tests.Add(
                "GroupEnableWait.SafetyReservation.PublishedProofInvalidatedWithoutEnableReplay",
                PublishedProofInvalidatedWithoutEnableReplay);
            tests.Add(
                "GroupEnableWait.PreWire.CancelAndTimeoutAreZeroWire",
                PreWireCancelAndTimeoutAreZeroWire);
            tests.Add(
                "GroupEnableWait.Race.DisableWaitsForAckPublication",
                DisableWaitsForAcknowledgementPublication);
            tests.Add(
                "GroupEnableWait.Race.DirectProofCompletesDuringDelayZeroWire",
                DirectProofCompletesDuringDelayZeroWire);
            tests.Add(
                "GroupEnableWait.Cancel.PostWriteDrainKeepsConnectionReusable",
                PostWriteDrainKeepsConnectionReusable);
            tests.Add(
                "GroupEnableWait.Scope.ReconnectRejectsOldContinuation",
                ReconnectRejectsOldContinuation);
            tests.Add(
                "GroupEnableWait.Session.CloseDuringFinalStatusRejectsStaleProof",
                CloseDuringFinalStatusRejectsStaleProof);
            tests.Add(
                "GroupEnableWait.Race.DisableResolvesStatusGateWaitZeroWire",
                DisableResolvesStatusGateWaitZeroWire);
            tests.Add(
                "GroupEnableWait.Race.DirectProofCompletesAtStatusGateZeroWire",
                DirectProofCompletesAtStatusGateZeroWire);
            tests.Add(
                "GroupEnableWait.Race.HelperCompletionIsLatchedBeforeGateRelease",
                HelperCompletionIsLatchedBeforeGateRelease);
            tests.Add(
                "GroupEnableWait.Race.DisableConnectionHandoffIsLinearized",
                DisableConnectionHandoffIsLinearized);
            tests.Add(
                "GroupEnableWait.Race.DisableInFlightOwnsProofCompletion",
                DisableInFlightOwnsProofCompletion);
            tests.Add(
                "GroupEnableWait.Race.DeadlineSeesClearBeforeGateRelease",
                DeadlineSeesClearBeforeGateRelease);
            tests.Add(
                "GroupEnableWait.PreWire.CallbackDeadlineKeepsConnectionReusable",
                CallbackDeadlineKeepsConnectionReusable);
            tests.Add(
                "GroupEnableWait.PreAccept.MutationGateCancelAndTimeoutAreZeroWire",
                InitialMutationGateCancelAndTimeoutAreZeroWire);
            tests.Add(
                "GroupEnableWait.PreAccept.ConnectionGateCancelAndTimeoutAreZeroWire",
                InitialConnectionGateCancelAndTimeoutAreZeroWire);
            tests.Add(
                "GroupEnableWait.PostCommit.AcceptedAckPreservesContinuation",
                AcceptedAcknowledgementPreservesContinuationAfterCancelAndTimeout);
            tests.Add(
                "GroupEnableWait.Deadline.NoAckIsOutcomeUncertainAndFaulted",
                NoAcknowledgementDeadlineIsUncertainAndFaulted);
            tests.Add(
                "GroupEnableWait.Deadline.NoStatusKeepsAcceptedContinuationAndFaults",
                NoStatusDeadlineKeepsAcceptedContinuationAndFaults);
            tests.Add(
                "GroupEnableWait.PreWireCommit.CancelAndDeadlineAreNotAttempted",
                CommitWindowCancelAndDeadlineAreNotAttempted);
            tests.Add(
                "GroupEnableWait.TransportFault.MutationReentryFailsFast",
                MutationReentryFailsFastOnTransportFault);
        }

        private static void OneEnableAndThreeStablePolls()
        {
            var defaults = new LMCGroupEnableWaitOptions();
            AssertEx.Equal(5000, defaults.TimeoutMilliseconds);
            AssertEx.Equal(50, defaults.PollIntervalMilliseconds);
            AssertEx.Equal(3, defaults.StableSampleCount);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();

                var result = group.GroupEnableAndWaitForLockedStandbyAsync(
                        defaults,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.Acknowledgement.IsSuccess);
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.Equal(5, result.PollCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.Equal(200L, result.Evidence.ElapsedMilliseconds);
                AssertEx.False(result.ReusedAcceptedAcknowledgement);
                AssertEx.False(result.Continuation.IsPending);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void
            AcceptedObserverBeginOnceBeforeStatusResumeZeroReplay()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerCount = 0;
                LMCGroupEnableWaitContinuation observed = null;

                var continuation = group
                    .BeginGroupEnableWaitForLockedStandbyAsync(
                        LongWaitOptions(),
                        accepted =>
                        {
                            observerCount++;
                            observed = accepted;
                            AssertEx.True(accepted.IsPending);
                            AssertEx.True(ReferenceEquals(
                                accepted,
                                group.PendingGroupEnableWaitContinuation));
                            AssertEx.Equal(1, CountCommand(server, 0x2047));
                            AssertEx.Equal(0, CountCommand(server, 0x2045));
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCount);
                AssertEx.True(ReferenceEquals(observed, continuation));
                AssertEx.Equal(0, continuation.PollCount);

                var result = group
                    .ResumeGroupEnableWaitForLockedStandbyAsync(
                        continuation,
                        LongWaitOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.ReusedAcceptedAcknowledgement);
                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.Equal(1, observerCount);
                AssertEx.Equal(1, CountCommand(server, 0x2047));
                AssertEx.Equal(3, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AcceptedObserverCompoundOnceBeforeFirstStatus()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var observerCount = 0;
                LMCGroupEnableWaitContinuation observed = null;
                var result = group
                    .GroupEnableAndWaitForLockedStandbyAsync(
                        new LMCGroupEnableWaitOptions
                        {
                            TimeoutMilliseconds = 1000,
                            PollIntervalMilliseconds = 1,
                            StableSampleCount = 3
                        },
                        accepted =>
                        {
                            observerCount++;
                            observed = accepted;
                            AssertEx.True(ReferenceEquals(
                                accepted,
                                group.PendingGroupEnableWaitContinuation));
                            AssertEx.Equal(0, CountCommand(server, 0x2045));
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, observerCount);
                AssertEx.True(ReferenceEquals(observed, result.Continuation));
                AssertEx.Equal(1, CountCommand(server, 0x2047));
                AssertEx.Equal(3, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AcceptedObserverExceptionPreservesContinuation()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var marker = new InvalidOperationException(
                    "accepted observer failure");
                var observerCount = 0;
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => group.BeginGroupEnableWaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            accepted =>
                            {
                                observerCount++;
                                AssertEx.True(ReferenceEquals(
                                    accepted,
                                    group.PendingGroupEnableWaitContinuation));
                                throw marker;
                            },
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(ReferenceEquals(marker, error));
                AssertEx.Equal(1, observerCount);
                var continuation = group.PendingGroupEnableWaitContinuation;
                AssertEx.NotNull(continuation);
                AssertEx.True(continuation.IsPending);
                AssertEx.Equal(0, CountCommand(server, 0x2045));

                var result = group
                    .ResumeGroupEnableWaitForLockedStandbyAsync(
                        continuation,
                        LongWaitOptions(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.Equal(1, CountCommand(server, 0x2047));
                AssertEx.Equal(3, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void LockedStandbyStatusOnlyStableEvidenceZeroEnable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var result = group.WaitForLockedStandbyAsync(
                        LongWaitOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.FinalStatus.IsPowerOn);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.Equal(5, result.StatusPollCount);
                AssertEx.Equal(3, result.StableSampleCount);
                AssertEx.Equal(3, result.RequiredStableSampleCount);
                AssertEx.Equal(40L, result.ElapsedMilliseconds);
                AssertEx.False(result.TransportInvalidatedAtDeadline);
                AssertEx.Equal(0, CountCommand(server, 0x2047));
                AssertEx.Equal(5, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void LockedStandbyTimeoutAndCancelTypedZeroEnable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var time = new FakeWaitTime();
                var timeout = AssertEx.Throws<
                    LMCGroupLockedStandbyWaitTimeoutException>(
                    () => group.WaitForLockedStandbyAsync(
                            ShortWaitOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(1, timeout.StatusPollCount);
                AssertEx.Equal(0, timeout.StableSampleCount);
                AssertEx.NotNull(timeout.LastObservedStatus);
                AssertEx.False(timeout.TransportInvalidatedAtDeadline);
                AssertEx.Equal(0, CountCommand(server, 0x2047));
                AssertEx.Equal(1, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }

            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                cancellation.Cancel();
                var canceled = AssertEx.Throws<
                    LMCGroupLockedStandbyWaitCanceledException>(
                    () => group.WaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(0, canceled.StatusPollCount);
                AssertEx.Equal(0, canceled.StableSampleCount);
                AssertEx.Equal<LMCGroupReadStatusResult>(
                    null,
                    canceled.LastObservedStatus);
                AssertEx.Equal(0, CountCommand(server, 0x2047));
                AssertEx.Equal(0, CountCommand(server, 0x2045));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void TimeoutResumeReusesAcceptedAcknowledgement()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn),
                LookupStep(),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = ShortWaitOptions();
                var firstTime = new FakeWaitTime();

                var timeout = AssertEx.Throws<LMCGroupEnableWaitTimeoutException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            options,
                            CancellationToken.None,
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(timeout.Acknowledgement.IsSuccess);
                AssertEx.NotNull(timeout.LastObservedStatus);
                AssertEx.Equal(PowerOn, timeout.LastObservedStatus.State);
                AssertEx.Equal(1, timeout.PollCount);
                AssertEx.True(timeout.Continuation.IsPending);
                AssertEx.True(ReferenceEquals(
                    timeout.Continuation,
                    group.PendingGroupEnableWaitContinuation));

                var pending = AssertEx.Throws<LMCGroupEnablePendingException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            options,
                            CancellationToken.None,
                            new FakeWaitTime().ElapsedMilliseconds,
                            new FakeWaitTime().DelayAsync)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.True(ReferenceEquals(timeout.Continuation, pending.Continuation));
                AssertEx.Throws<LMCGroupEnablePendingException>(
                    () => group.GroupEnable());
                AssertEx.Throws<LMCGroupEnablePendingException>(
                    () => group.GroupEnableAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                var secondHandle = new LMCGroup(connection, GroupName);
                AssertEx.True(ReferenceEquals(
                    timeout.Continuation,
                    secondHandle.PendingGroupEnableWaitContinuation));
                AssertEx.Throws<LMCGroupEnablePendingException>(
                    () => secondHandle.GroupEnable());

                var resumeTime = new FakeWaitTime();
                var resumed = secondHandle.ResumeGroupEnableWaitForLockedStandbyAsync(
                        timeout.Continuation,
                        LongWaitOptions(),
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(ReferenceEquals(
                    timeout.Acknowledgement,
                    resumed.Acknowledgement));
                AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                AssertEx.Equal(4, resumed.PollCount);
                AssertEx.Equal(3, resumed.StableSampleCount);
                AssertEx.False(timeout.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CancellationResumeReusesAcceptedAcknowledgement()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(
                    SuccessLongAcknowledgement(),
                    cancellation.Cancel),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = LongWaitOptions();
                var firstTime = new FakeWaitTime();

                var canceled = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            options,
                            cancellation.Token,
                            firstTime.ElapsedMilliseconds,
                            firstTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.True(canceled.Acknowledgement.IsSuccess);
                AssertEx.Equal<LMCGroupReadStatusResult>(
                    null,
                    canceled.LastObservedStatus);
                AssertEx.Equal(0, canceled.PollCount);
                AssertEx.True(canceled.Continuation.IsPending);

                var resumeTime = new FakeWaitTime();
                var resumed = group.ResumeGroupEnableWaitForLockedStandbyAsync(
                        canceled.Continuation,
                        options,
                        CancellationToken.None,
                        resumeTime.ElapsedMilliseconds,
                        resumeTime.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                AssertEx.True(ReferenceEquals(
                    canceled.Acknowledgement,
                    resumed.Acknowledgement));
                AssertEx.Equal(3, resumed.PollCount);
                AssertEx.False(canceled.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DisabledProofAllowsNewEnable()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn),
                LookupStep(),
                StatusStep(PowerOn | Disabled),
                StatusStep(PowerOn | Disabled),
                MalformedStatusStep(),
                StatusStep(PowerOn | Disabled),
                StatusStep(PowerOn | Disabled),
                StatusStep(PowerOn | Disabled),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = ShortWaitOptions();
                var timeout = AssertEx.Throws<LMCGroupEnableWaitTimeoutException>(
                    () => RunNewWait(group, options, CancellationToken.None));
                var continuation = timeout.Continuation;
                var secondHandle = new LMCGroup(connection, GroupName);

                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(2, continuation.PollCount);
                AssertEx.False(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(3, continuation.PollCount);
                AssertEx.False(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                AssertEx.Throws<InvalidDataException>(
                    () => secondHandle.GroupReadStatusResult());
                AssertEx.Equal(
                    3,
                    continuation.PollCount,
                    "Malformed 0x2045 is not a successfully parsed observation.");
                AssertEx.False(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(4, continuation.PollCount);
                AssertEx.False(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(5, continuation.PollCount);
                AssertEx.False(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(6, continuation.PollCount);
                AssertEx.True(secondHandle.TryReleasePendingGroupEnableForRetry(continuation));
                AssertEx.False(continuation.IsPending);

                var result = RunNewWait(secondHandle, LongWaitOptions(), CancellationToken.None);
                AssertEx.False(result.ReusedAcceptedAcknowledgement);
                AssertEx.Equal(3, result.PollCount);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PowerOffProofRequiresStableStatus()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                PowerOffStep(SuccessLongAcknowledgement()),
                StatusStep(0),
                StatusStep(PowerOn | Disabled),
                StatusStep(0),
                StatusStep(0),
                StatusStep(0),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var canceled = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(group, LongWaitOptions(), cancellation.Token));
                var continuation = canceled.Continuation;

                AssertEx.True(group.GroupPowerOff().IsSuccess);
                AssertEx.True(continuation.IsPending);
                AssertEx.False(group.TryReleasePendingGroupEnableForRetry(continuation));

                group.GroupReadStatusResult();
                AssertEx.False(group.TryReleasePendingGroupEnableForRetry(continuation));
                group.GroupReadStatusResult();
                AssertEx.False(group.TryReleasePendingGroupEnableForRetry(continuation));
                group.GroupReadStatusResult();
                group.GroupReadStatusResult();
                AssertEx.False(
                    group.TryReleasePendingGroupEnableForRetry(continuation),
                    "Alternating proof categories must not be combined.");
                group.GroupReadStatusResult();
                AssertEx.True(group.TryReleasePendingGroupEnableForRetry(continuation));
                AssertEx.Equal(5, continuation.PollCount);

                var result = RunNewWait(group, LongWaitOptions(), CancellationToken.None);
                AssertEx.Equal(3, result.PollCount);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DisableAcknowledgementContract()
        {
            using (var firstCancellation = new CancellationTokenSource())
            using (var secondCancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), firstCancellation.Cancel),
                DisableStep(SuccessLongAcknowledgement()),
                EnableStep(SuccessLongAcknowledgement(), secondCancellation.Cancel),
                DisableStep(ErrorLongAcknowledgement(-7)),
                DisableStep(TestFrame.Response(0, new byte[] { 0, 0 })),
                DisableStep(SuccessLongAcknowledgement()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var first = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        firstCancellation.Token));

                AssertEx.True(group.GroupDisable().IsSuccess);
                AssertEx.False(first.Continuation.IsPending);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);

                var second = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        secondCancellation.Token));

                var error = group.GroupDisableAsync(CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.False(error.IsSuccess);
                AssertEx.True(second.Continuation.IsPending);

                AssertEx.Throws<InvalidDataException>(
                    () => group.GroupDisable());
                AssertEx.True(second.Continuation.IsPending);

                AssertEx.True(
                    group.GroupDisableAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                        .IsSuccess);
                AssertEx.False(second.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ValidationAndRejectedAcknowledgementAreZeroPoll()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(ErrorLongAcknowledgement(-3)),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var canceledSource = new CancellationTokenSource())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var invalid = LongWaitOptions();
                invalid.StableSampleCount = 0;

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => RunNewWait(group, invalid, CancellationToken.None));

                canceledSource.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            canceledSource.Token)
                        .GetAwaiter()
                        .GetResult());

                var observerCount = 0;
                var rejected = AssertEx.Throws<LMCGroupEnableRejectedException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            accepted => observerCount++,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.False(rejected.Acknowledgement.IsSuccess);
                AssertEx.Equal((short)-3, rejected.Acknowledgement.ErrorId);
                AssertEx.Equal(0, observerCount);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void StatusErrorPreservesEvidence()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn, 0x0010, -9, 0),
                DisableStep(SuccessLongAcknowledgement()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var statusError = AssertEx.Throws<LMCGroupEnableStatusException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        CancellationToken.None));

                AssertEx.True(statusError.Acknowledgement.IsSuccess);
                AssertEx.NotNull(statusError.FailedStatus);
                AssertEx.True(ReferenceEquals(
                    statusError.FailedStatus,
                    statusError.LastObservedStatus));
                AssertEx.Equal((short)-9, statusError.FailedStatus.ErrorId);
                AssertEx.Equal(1, statusError.PollCount);
                AssertEx.True(statusError.Continuation.IsPending);

                AssertEx.True(group.GroupDisable().IsSuccess);
                AssertEx.False(statusError.Continuation.IsPending);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void LegacyEnableBlockedWhileWaitActive()
        {
            var waitTime = new ControlledWaitTime();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var secondHandle = new LMCGroup(connection, GroupName);
                var waitTask = group.GroupEnableAndWaitForLockedStandbyAsync(
                    LongWaitOptions(),
                    CancellationToken.None,
                    waitTime.ElapsedMilliseconds,
                    waitTime.DelayAsync);

                AssertEx.True(waitTime.DelayEntered.Wait(2000));
                AssertEx.Throws<InvalidOperationException>(
                    () => secondHandle.GroupEnable());
                AssertEx.Throws<InvalidOperationException>(
                    () => secondHandle.GroupEnableAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                waitTime.AdvanceAndRelease(10);
                var result = waitTask.GetAwaiter().GetResult();
                AssertEx.Equal(3, result.PollCount);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void LateTerminalStatusDoesNotWinTimeout()
        {
            var time = new FakeWaitTime();
            var lateThirdStatus = StatusStep(
                PowerOn | Standby,
                0,
                0,
                0,
                () => time.Advance(100));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                lateThirdStatus,
                DisableStep(SuccessLongAcknowledgement()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var options = new LMCGroupEnableWaitOptions
                {
                    TimeoutMilliseconds = 100,
                    PollIntervalMilliseconds = 1,
                    StableSampleCount = 3
                };

                var timeout = AssertEx.Throws<LMCGroupEnableWaitTimeoutException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(3, timeout.PollCount);
                AssertEx.NotNull(timeout.LastObservedStatus);
                AssertEx.True(timeout.LastObservedStatus.IsStandby);
                AssertEx.True(timeout.Continuation.IsPending);
                AssertEx.Equal(0, timeout.Continuation.StableSampleCount);

                AssertEx.True(group.GroupDisable().IsSuccess);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void SameConnectionHandlesShareContinuation()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                StatusStep(PowerOn | Standby),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var firstHandle = ConnectAndCreateGroup(connection, server.Port);
                var secondHandle = new LMCGroup(connection, GroupName);
                var canceled = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        firstHandle,
                        LongWaitOptions(),
                        cancellation.Token));

                AssertEx.True(ReferenceEquals(
                    canceled.Continuation,
                    secondHandle.PendingGroupEnableWaitContinuation));
                AssertEx.Throws<LMCGroupEnablePendingException>(
                    () => secondHandle.GroupEnable());

                var time = new FakeWaitTime();
                var result = secondHandle
                    .ResumeGroupEnableWaitForLockedStandbyAsync(
                        canceled.Continuation,
                        LongWaitOptions(),
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.True(result.ReusedAcceptedAcknowledgement);
                AssertEx.Equal(3, result.PollCount);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    firstHandle.PendingGroupEnableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PreAckObservationCannotContaminate()
        {
            using (var firstCancellation = new CancellationTokenSource())
            using (var secondCancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), firstCancellation.Cancel),
                DisableStep(SuccessLongAcknowledgement()),
                EnableStep(SuccessLongAcknowledgement(), secondCancellation.Cancel),
                DisableStep(SuccessLongAcknowledgement()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var preAckTarget =
                    group.CaptureGroupEnableWaitObservationTarget();
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    preAckTarget);

                var first = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        firstCancellation.Token));
                var staleTarget =
                    group.CaptureGroupEnableWaitObservationTarget();
                AssertEx.True(ReferenceEquals(first.Continuation, staleTarget));
                AssertEx.True(group.GroupDisable().IsSuccess);

                var second = AssertEx.Throws<LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        secondCancellation.Token));
                var locked = LMCConnection.ParseGroupReadStatusResult(
                    GroupStatusResponse(PowerOn | Standby));

                group.ObserveGroupEnableWaitStatus(preAckTarget, locked);
                group.ObserveGroupEnableWaitStatus(preAckTarget, locked);
                group.ObserveGroupEnableWaitStatus(preAckTarget, locked);
                group.ObserveGroupEnableWaitStatus(staleTarget, locked);
                group.ResetPendingGroupEnableProof(staleTarget);

                AssertEx.Equal(0, second.Continuation.PollCount);
                AssertEx.Equal(0, second.Continuation.StableSampleCount);
                AssertEx.True(second.Continuation.IsPending);

                AssertEx.True(group.GroupDisable().IsSuccess);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ConcurrentStatusUsesWireOrder()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var firstStatus = StatusStep(PowerOn | Disabled);
                firstStatus.ResponseDelayMilliseconds = 50;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        cancellation.Cancel),
                    firstStatus,
                    StatusStep(PowerOn),
                    StatusStep(PowerOn | Disabled),
                    StatusStep(PowerOn | Disabled),
                    StatusStep(PowerOn | Disabled),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            cancellation.Token));

                    var reads = new[]
                    {
                        firstHandle.GroupReadStatusResultAsync(
                            CancellationToken.None),
                        secondHandle.GroupReadStatusResultAsync(
                            CancellationToken.None),
                        firstHandle.GroupReadStatusResultAsync(
                            CancellationToken.None),
                        secondHandle.GroupReadStatusResultAsync(
                            CancellationToken.None),
                        firstHandle.GroupReadStatusResultAsync(
                            CancellationToken.None)
                    };
                    Task.WaitAll(reads);

                    AssertEx.Equal(5, canceled.Continuation.PollCount);
                    AssertEx.Equal(
                        3,
                        canceled.Continuation.DisabledUnlockedSampleCount);
                    AssertEx.True(
                        secondHandle.TryReleasePendingGroupEnableForRetry(
                            canceled.Continuation));

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void ManualThirdStatusDiscardedAfterPriorityReservation()
        {
            const string operation = "Manual GroupReadStatus publication";
            using (var cancellation = new CancellationTokenSource())
            using (var thirdStatusStarted = new ManualResetEventSlim(false))
            using (var releaseThirdStatus = new ManualResetEventSlim(false))
            {
                var coordinator = new LMCSendPriorityCoordinator();
                var options = new LMCConnectionOptions
                {
                    SendPriorityCoordinator = coordinator
                };
                var delayedThirdStatus = StatusStep(
                    PowerOn | Standby,
                    0,
                    0,
                    0,
                    () =>
                    {
                        thirdStatusStarted.Set();
                        if (!releaseThirdStatus.Wait(5000))
                        {
                            throw new TimeoutException(
                                "The delayed third status response was not released.");
                        }
                    });

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        cancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    delayedThirdStatus,
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    CloseStep()))
                using (var connection = new LMCConnection(options))
                {
                    try
                    {
                        var group = ConnectAndCreateGroup(
                            connection,
                            server.Port);
                        var canceled = AssertEx.Throws<
                            LMCGroupEnableWaitCanceledException>(
                            () => RunNewWait(
                                group,
                                LongWaitOptions(),
                                cancellation.Token));
                        var continuation = canceled.Continuation;

                        group.GroupReadStatusResult();
                        group.GroupReadStatusResult();
                        AssertEx.Equal(2, continuation.PollCount);
                        AssertEx.Equal(2, continuation.StableSampleCount);
                        AssertEx.True(continuation.IsPending);

                        var expectedGeneration = coordinator.CurrentGeneration;
                        long actualGeneration;
                        LMCSendPreemptedException error;
                        using (coordinator.BeginPreemptibleScope(
                            expectedGeneration,
                            operation))
                        {
                            var delayedRead = group.GroupReadStatusResultAsync(
                                CancellationToken.None);
                            AssertEx.True(thirdStatusStarted.Wait(2000));
                            actualGeneration = coordinator.ReservePrioritySend();
                            releaseThirdStatus.Set();
                            error = AssertEx.Throws<LMCSendPreemptedException>(
                                () => delayedRead.GetAwaiter().GetResult());
                        }

                        AssertEx.Equal(operation, error.Operation);
                        AssertEx.Equal((ushort)0x2045, error.Command);
                        AssertEx.Equal(expectedGeneration, error.ExpectedGeneration);
                        AssertEx.Equal(actualGeneration, error.ActualGeneration);
                        AssertEx.Equal(
                            LMCSendPreemptionPhase.ResultDiscarded,
                            error.Phase);
                        AssertEx.Contains("response for command 0x2045 was discarded", error.Message);
                        AssertEx.True(connection.IsConnected);
                        AssertEx.True(ReferenceEquals(
                            continuation,
                            group.PendingGroupEnableWaitContinuation));
                        AssertEx.True(continuation.IsPending);
                        AssertEx.Equal(
                            2,
                            continuation.PollCount,
                            "The discarded third response must not be published as an observation.");
                        AssertEx.Equal(0, continuation.StableSampleCount);
                        AssertEx.Equal(0, continuation.DisabledUnlockedSampleCount);
                        AssertEx.Equal(0, continuation.PoweredOffSampleCount);

                        var resumeTime = new FakeWaitTime();
                        var resumed = group
                            .ResumeGroupEnableWaitForLockedStandbyAsync(
                                continuation,
                                LongWaitOptions(),
                                CancellationToken.None,
                                resumeTime.ElapsedMilliseconds,
                                resumeTime.DelayAsync)
                            .GetAwaiter()
                            .GetResult();

                        AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                        AssertEx.Equal(5, resumed.PollCount);
                        AssertEx.Equal(3, resumed.StableSampleCount);
                        AssertEx.False(continuation.IsPending);

                        var enableCount = 0;
                        var statusCount = 0;
                        foreach (var request in server.ReceivedRequests)
                        {
                            var command = TestFrame.ReadUInt16(request, 0);
                            if (command == 0x2047)
                            {
                                enableCount++;
                            }
                            else if (command == 0x2045)
                            {
                                statusCount++;
                            }
                        }

                        AssertEx.Equal(
                            1,
                            enableCount,
                            "Status-only resume must not replay GroupEnable.");
                        AssertEx.Equal(6, statusCount);

                        connection.CloseConnection();
                        server.Verify();
                    }
                    finally
                    {
                        releaseThirdStatus.Set();
                    }
                }
            }
        }

        private static void PublishedProofInvalidatedWithoutEnableReplay()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var coordinator = new LMCSendPriorityCoordinator();
                var options = new LMCConnectionOptions
                {
                    SendPriorityCoordinator = coordinator
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        cancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    CloseStep()))
                using (var connection = new LMCConnection(options))
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            group,
                            LongWaitOptions(),
                            cancellation.Token));
                    var continuation = canceled.Continuation;

                    group.GroupReadStatusResult();
                    group.GroupReadStatusResult();
                    group.GroupReadStatusResult();
                    AssertEx.Equal(3, continuation.PollCount);
                    AssertEx.Equal(3, continuation.StableSampleCount);
                    AssertEx.True(continuation.IsPending);

                    coordinator.ReservePrioritySend();
                    AssertEx.True(
                        group.InvalidatePendingGroupEnableWaitStatusProof());
                    AssertEx.True(ReferenceEquals(
                        continuation,
                        group.PendingGroupEnableWaitContinuation));
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(3, continuation.PollCount);
                    AssertEx.Equal(0, continuation.StableSampleCount);
                    AssertEx.Equal(0, continuation.DisabledUnlockedSampleCount);
                    AssertEx.Equal(0, continuation.PoweredOffSampleCount);

                    var resumeTime = new FakeWaitTime();
                    var resumed = group
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            continuation,
                            LongWaitOptions(),
                            CancellationToken.None,
                            resumeTime.ElapsedMilliseconds,
                            resumeTime.DelayAsync)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                    AssertEx.Equal(6, resumed.PollCount);
                    AssertEx.Equal(3, resumed.StableSampleCount);
                    AssertEx.False(continuation.IsPending);
                    AssertEx.False(
                        group.InvalidatePendingGroupEnableWaitStatusProof());

                    var enableCount = 0;
                    var statusCount = 0;
                    foreach (var request in server.ReceivedRequests)
                    {
                        var command = TestFrame.ReadUInt16(request, 0);
                        if (command == 0x2047)
                        {
                            enableCount++;
                        }
                        else if (command == 0x2045)
                        {
                            statusCount++;
                        }
                    }

                    AssertEx.Equal(
                        1,
                        enableCount,
                        "Safety invalidation must not replay GroupEnable.");
                    AssertEx.Equal(6, statusCount);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void PreWireCancelAndTimeoutAreZeroWire()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var statusStarted = new ManualResetEventSlim(false))
            using (var connectionStarted = new ManualResetEventSlim(false))
            {
                var statusGateBlocker = StatusStep(PowerOn);
                statusGateBlocker.ResponseDelayMilliseconds = 200;
                statusGateBlocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    statusStarted.Set();
                };

                var connectionGateBlocker = StatusStep(PowerOn);
                connectionGateBlocker.ResponseDelayMilliseconds = 200;
                connectionGateBlocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    connectionStarted.Set();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    statusGateBlocker,
                    connectionGateBlocker,
                    DisableStep(SuccessLongAcknowledgement()),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            group,
                            LongWaitOptions(),
                            initialCancellation.Token));

                    var activeStatus = group.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    AssertEx.True(statusStarted.Wait(2000));

                    using (var waitingCancellation =
                        new CancellationTokenSource(40))
                    {
                        var canceled = AssertEx.Throws<
                            LMCGroupEnableWaitCanceledException>(
                            () => group
                                .ResumeGroupEnableWaitForLockedStandbyAsync(
                                    pending.Continuation,
                                    LongWaitOptions(),
                                    waitingCancellation.Token)
                                .GetAwaiter()
                                .GetResult());
                        AssertEx.True(canceled.Continuation.IsPending);
                    }

                    activeStatus.GetAwaiter().GetResult();

                    var rawBlocker = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(connectionStarted.Wait(2000));

                    var timeoutOptions = new LMCGroupEnableWaitOptions
                    {
                        TimeoutMilliseconds = 40,
                        PollIntervalMilliseconds = 10,
                        StableSampleCount = 3
                    };
                    var timeout = AssertEx.Throws<
                        LMCGroupEnableWaitTimeoutException>(
                        () => group
                            .ResumeGroupEnableWaitForLockedStandbyAsync(
                                pending.Continuation,
                                timeoutOptions,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.True(timeout.Continuation.IsPending);
                    rawBlocker.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        1,
                        pending.Continuation.PollCount,
                        "Only the explicit status-gate blocker was observed; both resume attempts were zero-wire.");
                    AssertEx.True(group.GroupDisable().IsSuccess);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void DisableWaitsForAcknowledgementPublication()
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
                delayedEnable.ResponseDelayMilliseconds = 150;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    delayedEnable,
                    DisableStep(SuccessLongAcknowledgement()),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var enableTask = firstHandle
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            cancellation.Token);
                    AssertEx.True(enableStarted.Wait(2000));

                    var disableTask = secondHandle.GroupDisableAsync(
                        CancellationToken.None);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => enableTask.GetAwaiter().GetResult());
                    AssertEx.True(
                        disableTask.GetAwaiter().GetResult().IsSuccess);

                    AssertEx.False(canceled.Continuation.IsPending);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        firstHandle.PendingGroupEnableWaitContinuation);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        secondHandle.PendingGroupEnableWaitContinuation);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void DirectProofCompletesDuringDelayZeroWire()
        {
            var waitTime = new ControlledWaitTime();
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement()),
                StatusStep(PowerOn),
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
                var options = new LMCGroupEnableWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 3
                };

                var operation = firstHandle
                    .GroupEnableAndWaitForLockedStandbyAsync(
                        options,
                        CancellationToken.None,
                        waitTime.ElapsedMilliseconds,
                        waitTime.DelayAsync);
                AssertEx.True(waitTime.DelayEntered.Wait(2000));

                secondHandle.GroupReadStatusResult();
                secondHandle.GroupReadStatusResult();
                secondHandle.GroupReadStatusResult();
                AssertEx.Equal(
                    4,
                    firstHandle.PendingGroupEnableWaitContinuation.PollCount);
                AssertEx.Equal(
                    3,
                    firstHandle.PendingGroupEnableWaitContinuation.StableSampleCount);

                waitTime.AdvanceAndRelease(1000);
                var result = operation.GetAwaiter().GetResult();

                AssertEx.Equal(4, result.PollCount);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.False(result.ReusedAcceptedAcknowledgement);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    secondHandle.PendingGroupEnableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void PostWriteDrainKeepsConnectionReusable()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var cancelingStatus = StatusStep(PowerOn);
                cancelingStatus.ResponseDelayMilliseconds = 150;
                cancelingStatus.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    cancellation.Cancel();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(SuccessLongAcknowledgement()),
                    cancelingStatus,
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            group,
                            LongWaitOptions(),
                            cancellation.Token));

                    AssertEx.Equal(1, canceled.PollCount);
                    AssertEx.NotNull(canceled.LastObservedStatus);
                    AssertEx.Equal(PowerOn, canceled.LastObservedStatus.State);
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);

                    var time = new FakeWaitTime();
                    var resumed = group
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            canceled.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(resumed.ReusedAcceptedAcknowledgement);
                    AssertEx.Equal(4, resumed.PollCount);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void ReconnectRejectsOldContinuation()
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
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var oldHandle = ConnectAndCreateGroup(
                    connection,
                    firstServer.Port);
                var canceled = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        oldHandle,
                        LongWaitOptions(),
                        cancellation.Token));

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var newHandle = new LMCGroup(connection, GroupName);

                AssertEx.Throws<InvalidOperationException>(
                    () => oldHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            canceled.Continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Throws<ArgumentException>(
                    () => newHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            canceled.Continuation,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    newHandle.PendingGroupEnableWaitContinuation);
                AssertEx.True(canceled.Continuation.IsPending);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void CloseDuringFinalStatusRejectsStaleProof()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var finalStatusRequested = new ManualResetEventSlim(false))
            {
                var finalStatus = StatusStep(
                    PowerOn | Standby,
                    0,
                    0,
                    0,
                    finalStatusRequested.Set);
                finalStatus.ResponseDelayMilliseconds = 250;

                using (var firstServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    finalStatus,
                    CloseStep()))
                using (var secondServer = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var oldHandle = ConnectAndCreateGroup(
                        connection,
                        firstServer.Port);
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            oldHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));
                    var continuation = canceled.Continuation;

                    var operation = oldHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    AssertEx.True(finalStatusRequested.Wait(2000));

                    var close = connection.CloseConnectionAsync(
                        CancellationToken.None);
                    AssertEx.True(SpinWait.SpinUntil(
                        () => connection.State == LMCConnectionState.Closing,
                        2000));

                    var exception = AssertEx.Throws<
                        LMCGroupEnableStatusException>(
                        () => operation.GetAwaiter().GetResult());
                    close.GetAwaiter().GetResult();

                    AssertEx.True(ReferenceEquals(
                        continuation,
                        exception.Continuation));
                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.Accepted,
                        exception.SubmissionOutcome);
                    AssertEx.True(
                        exception.InnerException is InvalidOperationException);
                    AssertEx.Equal(2, exception.PollCount);
                    AssertEx.Equal(2, continuation.PollCount);
                    AssertEx.Equal(0, continuation.StableSampleCount);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(
                        LMCConnectionState.Disconnected,
                        connection.State);
                    firstServer.Verify();
                    AssertEx.Equal(1, CountCommand(firstServer, 0x2047));
                    AssertEx.Equal(3, CountCommand(firstServer, 0x2045));

                    connection.RpcInitConnection(
                        "127.0.0.1",
                        secondServer.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask);
                    var newHandle = new LMCGroup(connection, GroupName);

                    AssertEx.Throws<InvalidOperationException>(
                        () => oldHandle
                            .ResumeGroupEnableWaitForLockedStandbyAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Throws<ArgumentException>(
                        () => newHandle
                            .ResumeGroupEnableWaitForLockedStandbyAsync(
                                continuation,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(0, CountCommand(secondServer, 0x2047));
                    AssertEx.Equal(0, CountCommand(secondServer, 0x2045));
                    AssertEx.True(continuation.IsPending);

                    connection.CloseConnection();
                    secondServer.Verify();
                }
            }
        }

        private static void DisableResolvesStatusGateWaitZeroWire()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                LookupStep(),
                EnableStep(SuccessLongAcknowledgement(), cancellation.Cancel),
                DisableStep(SuccessLongAcknowledgement()),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var firstHandle = ConnectAndCreateGroup(
                    connection,
                    server.Port);
                var secondHandle = new LMCGroup(connection, GroupName);
                var pending = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        firstHandle,
                        LongWaitOptions(),
                        cancellation.Token));
                var coordinator = GetCoordinator(firstHandle);

                coordinator.MutationGate.Wait();
                var manualMutationGateHeld = true;
                try
                {
                    var disableTask = secondHandle.GroupDisableAsync(
                        CancellationToken.None);
                    AssertEx.Equal(
                        0,
                        coordinator.StatusObservationGate.CurrentCount);
                    var resumeTask = secondHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    coordinator.MutationGate.Release();
                    manualMutationGateHeld = false;

                    AssertEx.True(
                        disableTask.GetAwaiter().GetResult().IsSuccess);
                    var resolved = AssertEx.Throws<
                        LMCGroupEnableWaitResolvedException>(
                        () => resumeTask.GetAwaiter().GetResult());
                    AssertEx.True(ReferenceEquals(
                        pending.Continuation,
                        resolved.Continuation));
                    AssertEx.Equal(0, resolved.PollCount);
                    AssertEx.False(resolved.Continuation.IsPending);
                }
                catch
                {
                    if (manualMutationGateHeld)
                    {
                        coordinator.MutationGate.Release();
                    }

                    throw;
                }

                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    firstHandle.PendingGroupEnableWaitContinuation);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void DirectProofCompletesAtStatusGateZeroWire()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var thirdStatusStarted = new ManualResetEventSlim(false))
            using (var releaseThirdStatus = new ManualResetEventSlim(false))
            {
                var thirdStatus = StatusStep(PowerOn | Standby);
                thirdStatus.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    thirdStatusStarted.Set();
                    AssertEx.True(releaseThirdStatus.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    thirdStatus,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));

                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();
                    var directStatus = secondHandle.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    AssertEx.True(thirdStatusStarted.Wait(2000));

                    var resume = firstHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    AssertEx.True(
                        GetCoordinator(firstHandle).WaitInProgress);

                    releaseThirdStatus.Set();
                    AssertEx.True(directStatus.GetAwaiter().GetResult().IsStandby);
                    var result = resume.GetAwaiter().GetResult();

                    AssertEx.True(result.FinalStatus.IsStandby);
                    AssertEx.Equal(3, result.PollCount);
                    AssertEx.True(result.ReusedAcceptedAcknowledgement);
                    AssertEx.False(result.Continuation.IsPending);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void HelperCompletionIsLatchedBeforeGateRelease()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var helperStatusStarted = new ManualResetEventSlim(false))
            using (var releaseHelperStatus = new ManualResetEventSlim(false))
            {
                var helperStatus = StatusStep(PowerOn | Standby);
                helperStatus.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    helperStatusStarted.Set();
                    AssertEx.True(releaseHelperStatus.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    helperStatus,
                    StatusStep(PowerOn),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));

                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();
                    var resume = firstHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    AssertEx.True(helperStatusStarted.Wait(2000));

                    var queuedReset = secondHandle.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    releaseHelperStatus.Set();

                    var result = resume.GetAwaiter().GetResult();
                    var resetStatus = queuedReset.GetAwaiter().GetResult();
                    AssertEx.True(result.FinalStatus.IsStandby);
                    AssertEx.Equal(3, result.PollCount);
                    AssertEx.False(resetStatus.IsStandby);
                    AssertEx.False(result.Continuation.IsPending);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void DisableConnectionHandoffIsLinearized()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var blockerStarted = new ManualResetEventSlim(false))
            using (var releaseBlocker = new ManualResetEventSlim(false))
            {
                var blocker = StatusStep(PowerOn);
                blocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    blockerStarted.Set();
                    AssertEx.True(releaseBlocker.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    blocker,
                    DisableStep(SuccessLongAcknowledgement()),
                    StatusStep(PowerOn | Standby),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));

                    var activeConnectionRequest = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(blockerStarted.Wait(2000));

                    var coordinator = GetCoordinator(firstHandle);
                    var disable = secondHandle.GroupDisableAsync(
                        CancellationToken.None);
                    AssertEx.True(SpinWait.SpinUntil(
                        () => coordinator.MutationGate.CurrentCount == 0,
                        2000));
                    var resume = firstHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    AssertEx.Equal(0, coordinator.StatusObservationGate.CurrentCount);
                    var directStatus = secondHandle.GroupReadStatusResultAsync(
                        CancellationToken.None);

                    releaseBlocker.Set();
                    activeConnectionRequest.GetAwaiter().GetResult();
                    AssertEx.True(disable.GetAwaiter().GetResult().IsSuccess);
                    AssertEx.Throws<LMCGroupEnableWaitResolvedException>(
                        () => resume.GetAwaiter().GetResult());
                    AssertEx.True(
                        directStatus.GetAwaiter().GetResult().IsStandby);
                    AssertEx.Equal(0, pending.PollCount);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void DisableInFlightOwnsProofCompletion()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var disableStarted = new ManualResetEventSlim(false))
            using (var releaseDisable = new ManualResetEventSlim(false))
            {
                var disableStep = DisableStep(SuccessLongAcknowledgement());
                disableStep.InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x2048,
                            GroupReference,
                            new byte[] { 1 }),
                        request);
                    disableStarted.Set();
                    AssertEx.True(releaseDisable.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    disableStep,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));
                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();

                    var disable = secondHandle.GroupDisableAsync(
                        CancellationToken.None);
                    AssertEx.True(disableStarted.Wait(2000));
                    var resume = firstHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    releaseDisable.Set();

                    AssertEx.True(disable.GetAwaiter().GetResult().IsSuccess);
                    AssertEx.Throws<LMCGroupEnableWaitResolvedException>(
                        () => resume.GetAwaiter().GetResult());
                    AssertEx.Equal(3, pending.Continuation.PollCount);

                    connection.CloseConnection();
                    server.Verify();
                }
            }

            using (var initialCancellation = new CancellationTokenSource())
            using (var disableStarted = new ManualResetEventSlim(false))
            using (var releaseDisable = new ManualResetEventSlim(false))
            {
                var disableStep = DisableStep(
                    ErrorLongAcknowledgement(unchecked((short)0x8123)));
                disableStep.InspectRequest = request =>
                {
                    AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x2048,
                            GroupReference,
                            new byte[] { 1 }),
                        request);
                    disableStarted.Set();
                    AssertEx.True(releaseDisable.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    StatusStep(PowerOn | Standby),
                    disableStep,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var firstHandle = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    var secondHandle = new LMCGroup(connection, GroupName);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            firstHandle,
                            LongWaitOptions(),
                            initialCancellation.Token));
                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();
                    secondHandle.GroupReadStatusResult();

                    var disable = secondHandle.GroupDisableAsync(
                        CancellationToken.None);
                    AssertEx.True(disableStarted.Wait(2000));
                    var resume = firstHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None);
                    releaseDisable.Set();

                    AssertEx.False(disable.GetAwaiter().GetResult().IsSuccess);
                    var result = resume.GetAwaiter().GetResult();
                    AssertEx.True(result.FinalStatus.IsStandby);
                    AssertEx.Equal(3, result.PollCount);
                    AssertEx.True(result.ReusedAcceptedAcknowledgement);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void DeadlineSeesClearBeforeGateRelease()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(
                    SuccessLongAcknowledgement(),
                    initialCancellation.Cancel),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var pending = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        initialCancellation.Token));
                var coordinator = GetCoordinator(group);

                coordinator.StatusObservationGate.Wait();
                try
                {
                    var resume = group
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            pending.Continuation,
                            new LMCGroupEnableWaitOptions
                            {
                                TimeoutMilliseconds = 40,
                                PollIntervalMilliseconds = 10,
                                StableSampleCount = 3
                            },
                            CancellationToken.None);

                    lock (coordinator.Sync)
                    {
                        pending.Continuation.MarkCompleted();
                        coordinator.PendingContinuation = null;
                    }

                    var resolved = AssertEx.Throws<
                        LMCGroupEnableWaitResolvedException>(
                        () => resume.GetAwaiter().GetResult());
                    AssertEx.True(ReferenceEquals(
                        pending.Continuation,
                        resolved.Continuation));
                    AssertEx.False(resolved.Continuation.IsPending);
                    AssertEx.Equal(0, resolved.PollCount);
                    AssertEx.True(
                        resolved.Evidence.ElapsedMilliseconds > 0,
                        "Resolution evidence must retain the active wait elapsed time.");
                }
                finally
                {
                    coordinator.StatusObservationGate.Release();
                }

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CallbackDeadlineKeepsConnectionReusable()
        {
            using (var initialCancellation = new CancellationTokenSource())
            using (var blockerStarted = new ManualResetEventSlim(false))
            using (var releaseBlocker = new ManualResetEventSlim(false))
            {
                var blocker = StatusStep(PowerOn);
                blocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    blockerStarted.Set();
                    AssertEx.True(releaseBlocker.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(
                        SuccessLongAcknowledgement(),
                        initialCancellation.Cancel),
                    blocker,
                    DisableStep(SuccessLongAcknowledgement()),
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var pending = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => RunNewWait(
                            group,
                            LongWaitOptions(),
                            initialCancellation.Token));
                    var activeConnectionRequest = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(blockerStarted.Wait(2000));

                    var time = new FakeWaitTime();
                    var options = ShortWaitOptions();
                    var resume = group.ResumeGroupEnableWaitForLockedStandbyAsync(
                        pending.Continuation,
                        options,
                        CancellationToken.None,
                        time.ElapsedMilliseconds,
                        time.DelayAsync);
                    var coordinator = GetCoordinator(group);
                    AssertEx.True(SpinWait.SpinUntil(
                        () => coordinator.MutationGate.CurrentCount == 0,
                        2000));

                    time.Advance(options.TimeoutMilliseconds);
                    releaseBlocker.Set();
                    activeConnectionRequest.GetAwaiter().GetResult();
                    var timeout = AssertEx.Throws<
                        LMCGroupEnableWaitTimeoutException>(
                        () => resume.GetAwaiter().GetResult());

                    AssertEx.True(timeout.Continuation.IsPending);
                    AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                    AssertEx.Equal<Exception>(null, connection.LastTransportException);
                    AssertEx.True(group.GroupDisable().IsSuccess);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void InitialMutationGateCancelAndTimeoutAreZeroWire()
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
                var coordinator = GetCoordinator(group);
                coordinator.MutationGate.Wait();
                Task<LMCGroupEnableWaitResult> operation;
                try
                {
                    operation = group.GroupEnableAndWaitForLockedStandbyAsync(
                        LongWaitOptions(),
                        cancellation.Token);
                    cancellation.Cancel();
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => operation.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.NotAttempted,
                        canceled.SubmissionOutcome);
                    AssertEx.False(canceled.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        canceled.Continuation);
                }
                finally
                {
                    coordinator.MutationGate.Release();
                }

                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                connection.CloseConnection();
                server.Verify();
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var coordinator = GetCoordinator(group);
                coordinator.MutationGate.Wait();
                try
                {
                    var timeout = AssertEx.Throws<
                        LMCGroupEnableWaitTimeoutException>(
                        () => group.GroupEnableAndWaitForLockedStandbyAsync(
                                new LMCGroupEnableWaitOptions
                                {
                                    TimeoutMilliseconds = 40,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.NotAttempted,
                        timeout.SubmissionOutcome);
                    AssertEx.False(timeout.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        timeout.Continuation);
                }
                finally
                {
                    coordinator.MutationGate.Release();
                }

                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void InitialConnectionGateCancelAndTimeoutAreZeroWire()
        {
            using (var blockerStarted = new ManualResetEventSlim(false))
            using (var releaseBlocker = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            {
                var blocker = StatusStep(PowerOn);
                blocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    blockerStarted.Set();
                    AssertEx.True(releaseBlocker.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blocker,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var activeConnectionRequest = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(blockerStarted.Wait(2000));

                    var operation = group
                        .GroupEnableAndWaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            cancellation.Token);
                    AssertEx.True(SpinWait.SpinUntil(
                        () => GetCoordinator(group).MutationGate.CurrentCount == 0,
                        2000));
                    cancellation.Cancel();
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => operation.GetAwaiter().GetResult());
                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.NotAttempted,
                        canceled.SubmissionOutcome);
                    AssertEx.False(canceled.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        canceled.Continuation);

                    releaseBlocker.Set();
                    activeConnectionRequest.GetAwaiter().GetResult();
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        group.PendingGroupEnableWaitContinuation);
                    AssertEx.Equal(LMCConnectionState.Connected, connection.State);

                    connection.CloseConnection();
                    server.Verify();
                }
            }

            using (var blockerStarted = new ManualResetEventSlim(false))
            using (var releaseBlocker = new ManualResetEventSlim(false))
            {
                var blocker = StatusStep(PowerOn);
                blocker.InspectRequest = request =>
                {
                    AssertStatusRequest(request);
                    blockerStarted.Set();
                    AssertEx.True(releaseBlocker.Wait(2000));
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blocker,
                    CloseStep()))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(connection, server.Port);
                    var activeConnectionRequest = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        CancellationToken.None);
                    AssertEx.True(blockerStarted.Wait(2000));

                    var timeout = AssertEx.Throws<
                        LMCGroupEnableWaitTimeoutException>(
                        () => group.GroupEnableAndWaitForLockedStandbyAsync(
                                new LMCGroupEnableWaitOptions
                                {
                                    TimeoutMilliseconds = 40,
                                    PollIntervalMilliseconds = 10,
                                    StableSampleCount = 3
                                },
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.NotAttempted,
                        timeout.SubmissionOutcome);
                    AssertEx.False(timeout.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        timeout.Continuation);

                    releaseBlocker.Set();
                    activeConnectionRequest.GetAwaiter().GetResult();
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        group.PendingGroupEnableWaitContinuation);
                    AssertEx.Equal(LMCConnectionState.Connected, connection.State);

                    connection.CloseConnection();
                    server.Verify();
                }
            }
        }

        private static void
            AcceptedAcknowledgementPreservesContinuationAfterCancelAndTimeout()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var enable = EnableStep(
                    SuccessLongAcknowledgement(),
                    cancellation.Cancel);
                enable.ResponseDelayMilliseconds = 80;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    LookupStep(),
                    enable,
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
                    var canceled = AssertEx.Throws<
                        LMCGroupEnableWaitCanceledException>(
                        () => firstHandle
                            .GroupEnableAndWaitForLockedStandbyAsync(
                                LongWaitOptions(),
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(
                        LMCGroupEnableSubmissionOutcome.Accepted,
                        canceled.SubmissionOutcome);
                    AssertEx.True(canceled.CommandMayHaveBeenSent);
                    AssertEx.False(
                        canceled.TransportInvalidatedAtDeadline);
                    AssertEx.True(canceled.Acknowledgement.IsSuccess);
                    AssertEx.True(canceled.Continuation.IsPending);
                    var result = secondHandle
                        .ResumeGroupEnableWaitForLockedStandbyAsync(
                            canceled.Continuation,
                            LongWaitOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(result.FinalStatus.IsStandby);
                    AssertEx.True(result.ReusedAcceptedAcknowledgement);

                    connection.CloseConnection();
                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2047));
                    AssertEx.Equal(3, CountCommand(server, 0x2045));
                }
            }
        }

        private static void NoAcknowledgementDeadlineIsUncertainAndFaulted()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedEnable = EnableStep(
                    SuccessLongAcknowledgement());
                blockedEnable.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked GroupEnable response was not released.");
                blockedEnable.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    blockedEnable))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupEnableWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCGroupEnableWaitTimeoutException>(
                            () => group
                                .GroupEnableAndWaitForLockedStandbyAsync(
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
                        LMCGroupEnableSubmissionOutcome.OutcomeUncertain,
                        error.SubmissionOutcome);
                    AssertEx.True(error.CommandMayHaveBeenSent);
                    AssertEx.Equal<LMC_Response>(null, error.Acknowledgement);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        error.Continuation);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                        null,
                        group.PendingGroupEnableWaitContinuation);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2047));
                    AssertEx.Equal(0, CountCommand(server, 0x2045));
                }
            }
        }

        private static void
            NoStatusDeadlineKeepsAcceptedContinuationAndFaults()
        {
            using (var releaseResponse = new ManualResetEventSlim(false))
            {
                var blockedStatus = StatusStep(PowerOn | Standby);
                blockedStatus.BeforeResponse = () => AssertEx.True(
                    releaseResponse.Wait(5000),
                    "The blocked GroupStatus response was not released.");
                blockedStatus.AllowClientDisconnectAfterRequest = true;

                using (var server = new FakeRpcServer(
                    InitStep(),
                    CallbackStep(),
                    LookupStep(),
                    EnableStep(SuccessLongAcknowledgement()),
                    blockedStatus))
                using (var connection = new LMCConnection())
                {
                    var group = ConnectAndCreateGroup(
                        connection,
                        server.Port);
                    LMCGroupEnableWaitTimeoutException error = null;
                    try
                    {
                        error = AssertEx.Throws<
                            LMCGroupEnableWaitTimeoutException>(
                            () => group
                                .GroupEnableAndWaitForLockedStandbyAsync(
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
                        LMCGroupEnableSubmissionOutcome.Accepted,
                        error.SubmissionOutcome);
                    AssertEx.True(error.CommandMayHaveBeenSent);
                    AssertEx.True(error.Acknowledgement.IsSuccess);
                    AssertEx.True(error.TransportInvalidatedAtDeadline);
                    AssertEx.NotNull(error.Continuation);
                    AssertEx.True(error.Continuation.IsPending);
                    AssertEx.True(ReferenceEquals(
                        error.Continuation,
                        group.PendingGroupEnableWaitContinuation));
                    AssertEx.Equal(0, error.PollCount);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);

                    server.Verify();
                    AssertEx.Equal(1, CountCommand(server, 0x2047));
                    AssertEx.Equal(1, CountCommand(server, 0x2045));
                }
            }
        }

        private static void CommitWindowCancelAndDeadlineAreNotAttempted()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var coordinator = GetCoordinator(group);
                var mutationGeneration = coordinator.MutationGeneration;
                var time = new FakeWaitTime();
                var error = AssertEx.Throws<
                    LMCGroupEnableWaitCanceledException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            LongWaitOptions(),
                            cancellation.Token,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            cancellation.Cancel)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupEnableSubmissionOutcome.NotAttempted,
                    error.SubmissionOutcome);
                AssertEx.False(error.CommandMayHaveBeenSent);
                AssertEx.Equal(mutationGeneration, coordinator.MutationGeneration);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                AssertEx.Equal(0, CountCommand(server, 0x2047));

                AssertEx.True(group.GroupReadStatusResult().IsSuccess);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2045));
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                StatusStep(PowerOn),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var coordinator = GetCoordinator(group);
                var mutationGeneration = coordinator.MutationGeneration;
                var time = new FakeWaitTime();
                var options = new LMCGroupEnableWaitOptions
                {
                    TimeoutMilliseconds = 40,
                    PollIntervalMilliseconds = 10,
                    StableSampleCount = 3
                };
                var error = AssertEx.Throws<
                    LMCGroupEnableWaitTimeoutException>(
                    () => group.GroupEnableAndWaitForLockedStandbyAsync(
                            options,
                            CancellationToken.None,
                            time.ElapsedMilliseconds,
                            time.DelayAsync,
                            () => Thread.Sleep(80))
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    LMCGroupEnableSubmissionOutcome.NotAttempted,
                    error.SubmissionOutcome);
                AssertEx.False(error.CommandMayHaveBeenSent);
                AssertEx.Equal(mutationGeneration, coordinator.MutationGeneration);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);
                AssertEx.False(error.TransportInvalidatedAtDeadline);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                AssertEx.Equal(0, CountCommand(server, 0x2047));

                AssertEx.True(group.GroupReadStatusResult().IsSuccess);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x2045));
            }

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                EnableStep(ErrorLongAcknowledgement(-17)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var coordinator = GetCoordinator(group);
                var mutationGeneration = coordinator.MutationGeneration;
                var rejected = AssertEx.Throws<
                    LMCGroupEnableRejectedException>(
                    () => RunNewWait(
                        group,
                        LongWaitOptions(),
                        CancellationToken.None));

                AssertEx.Equal(
                    LMCGroupEnableSubmissionOutcome.Rejected,
                    rejected.SubmissionOutcome);
                AssertEx.Equal(
                    mutationGeneration + 1,
                    coordinator.MutationGeneration);
                AssertEx.Equal<LMCGroupEnableWaitContinuation>(
                    null,
                    group.PendingGroupEnableWaitContinuation);
                AssertEx.Equal(1, CountCommand(server, 0x2047));

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void MutationReentryFailsFastOnTransportFault()
        {
            var oversizedHeader = new byte[8];
            TestFrame.WriteUInt16(oversizedHeader, 2, 9);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                LookupStep(),
                new FakeRpcStep(0x2047, oversizedHeader)
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection())
            {
                var group = ConnectAndCreateGroup(connection, server.Port);
                var reentryExceptions = new List<Exception>();
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs args)
                {
                    if (args.CurrentState != LMCConnectionState.Faulted)
                    {
                        return;
                    }

                    var mutations = new Action[]
                    {
                        () => group.GroupEnable(),
                        () => group.GroupEnableAsync(CancellationToken.None)
                            .GetAwaiter().GetResult(),
                        () => group.GroupDisable(),
                        () => group.GroupDisableAsync(CancellationToken.None)
                            .GetAwaiter().GetResult()
                    };
                    foreach (var mutation in mutations)
                    {
                        try
                        {
                            mutation();
                        }
                        catch (Exception ex)
                        {
                            reentryExceptions.Add(ex);
                        }
                    }
                };

                var operation = Task.Run<Exception>(() =>
                {
                    try
                    {
                        group.GroupEnable();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                });
                AssertEx.True(operation.Wait(2000));
                AssertEx.True(operation.Result is InvalidDataException);
                AssertEx.Equal(4, reentryExceptions.Count);
                foreach (var exception in reentryExceptions)
                {
                    AssertEx.True(exception is InvalidOperationException);
                }

                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                server.Verify();
                connection.CloseConnection();
            }
        }

        private static LMCGroupEnableWaitResult RunNewWait(
            LMCGroupAxis group,
            LMCGroupEnableWaitOptions options,
            CancellationToken cancellationToken)
        {
            var time = new FakeWaitTime();
            return group.GroupEnableAndWaitForLockedStandbyAsync(
                    options,
                    cancellationToken,
                    time.ElapsedMilliseconds,
                    time.DelayAsync)
                .GetAwaiter()
                .GetResult();
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

        private static LMCGroupEnableWaitCoordinator GetCoordinator(
            LMCGroupAxis group)
        {
            var field = typeof(LMCGroupAxis).GetField(
                "groupEnableWaitCoordinator",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(field);
            var coordinator = field.GetValue(group)
                as LMCGroupEnableWaitCoordinator;
            AssertEx.NotNull(coordinator);
            return coordinator;
        }

        private static LMCGroupEnableWaitOptions ShortWaitOptions()
        {
            return new LMCGroupEnableWaitOptions
            {
                TimeoutMilliseconds = 10,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupEnableWaitOptions LongWaitOptions()
        {
            return new LMCGroupEnableWaitOptions
            {
                TimeoutMilliseconds = 1000,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static LMCGroupEnableWaitOptions DeadlineOptions()
        {
            return new LMCGroupEnableWaitOptions
            {
                TimeoutMilliseconds = 100,
                PollIntervalMilliseconds = 10,
                StableSampleCount = 3
            };
        }

        private static int CountCommand(FakeRpcServer server, ushort command)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
            {
                if (LMC_Frame.GetRequestCommand(request) == command)
                {
                    count++;
                }
            }

            return count;
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

        private static FakeRpcStep DisableStep(byte[] response)
        {
            return new FakeRpcStep(0x2048, response)
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2048,
                        GroupReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep PowerOffStep(byte[] response)
        {
            return new FakeRpcStep(0x204B, response)
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x204B,
                        GroupReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep StatusStep(
            uint state,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort groupErrorId = 0,
            Action afterRequest = null)
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
                    AssertStatusRequest(request);
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

        private static void AssertStatusRequest(byte[] request)
        {
            var requestPayload = new byte[8];
            TestFrame.WriteInt32(requestPayload, 0, GroupReference);
            TestFrame.WriteInt32(requestPayload, 4, 1);
            AssertEx.SequenceEqual(
                TestFrame.Request(
                    0x2045,
                    GroupReference,
                    requestPayload),
                request);
        }

        private static FakeRpcStep MalformedStatusStep()
        {
            var requestPayload = new byte[8];
            TestFrame.WriteInt32(requestPayload, 0, GroupReference);
            TestFrame.WriteInt32(requestPayload, 4, 1);

            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, new byte[10]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2045,
                        GroupReference,
                        requestPayload),
                    request)
            };
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

        private static byte[] ErrorLongAcknowledgement(short errorId)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt16(payload, 4, 0x0010);
            TestFrame.WriteInt16(payload, 6, errorId);
            return TestFrame.Response(0, payload);
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

        private sealed class ControlledWaitTime
        {
            private readonly TaskCompletionSource<bool> delayCompletion =
                new TaskCompletionSource<bool>();
            private long elapsedMilliseconds;

            internal ControlledWaitTime()
            {
                DelayEntered = new ManualResetEventSlim(false);
            }

            internal ManualResetEventSlim DelayEntered { get; private set; }

            internal long ElapsedMilliseconds()
            {
                return Interlocked.Read(ref elapsedMilliseconds);
            }

            internal Task DelayAsync(
                int delayMilliseconds,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DelayEntered.Set();
                return delayCompletion.Task;
            }

            internal void AdvanceAndRelease(int milliseconds)
            {
                Interlocked.Add(ref elapsedMilliseconds, milliseconds);
                delayCompletion.SetResult(true);
            }
        }
    }
}

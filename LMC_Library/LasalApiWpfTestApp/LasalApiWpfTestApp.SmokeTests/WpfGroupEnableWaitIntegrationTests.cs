using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        private const string GroupEnableWaitName = "_LMCRobotBase1";
        private const ushort GroupEnableWaitReference = 0x0100;
        private const uint GroupEnableWaitPowerOn = 0x00040000u;
        private const uint GroupEnableWaitStandby = 0x00020000u;
        private const uint GroupEnableWaitDisabled = 0x00010000u;

        private static void RegisterGroupEnableWaitTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupEnable.OneEnableThenThreeStableLockedStandbySamples",
                GroupEnableUsesOneRequestAndThreeStableSamples);
            tests.Add(
                "Wpf.GroupEnable.PreemptedVerificationResumesWithoutEnableReplay",
                PreemptedGroupEnableVerificationResumesWithoutReplay);
            tests.Add(
                "Wpf.GroupPowerOn.OneCommandThenThreeStablePowerOnSamples",
                GroupPowerOnUsesOneRequestAndThreeStableSamples);
            tests.Add(
                "Wpf.GroupPowerOn.PreemptedVerificationResumesWithoutPowerOnReplay",
                PreemptedGroupPowerOnVerificationResumesWithoutReplay);
            tests.Add(
                "Wpf.GroupPowerOff.PreemptedVerificationResumesWithoutPowerOffReplay",
                PreemptedGroupPowerOffVerificationResumesWithoutReplay);
            tests.Add(
                "Wpf.GroupPowerState.SingleReadDoesNotCompletePendingPowerOn",
                SingleStatusReadDoesNotCompletePendingPowerOn);
            tests.Add(
                "Wpf.GroupPowerState.SingleReadDoesNotPromoteUnverifiedPowerOn",
                SingleStatusReadDoesNotPromoteUnverifiedPowerOn);
            tests.Add(
                "Wpf.GroupPowerState.StaleReadResultIsDiscardedAfterSafetyReservation",
                StaleStatusReadIsDiscardedAfterSafetyReservation);
            tests.Add(
                "Wpf.GroupPowerState.PowerOffTakesOwnershipFromPendingPowerOn",
                PowerOffTakesOwnershipFromPendingPowerOn);
            tests.Add(
                "Wpf.GroupPowerState.GroupStopInterferenceRequiresPowerOffTakeover",
                GroupStopPreemptsThenPowerOnResumesWithoutReplay);
            tests.Add(
                "Wpf.GroupPowerRecovery.AcceptedRestartOnAndOffAreStatusOnly",
                GroupPowerAcceptedRestartOnAndOffAreStatusOnly);
            tests.Add(
                "Wpf.GroupPowerRecovery.OlderFailurePreservesNewerOffActiveAndResolved",
                GroupPowerOlderFailurePreservesNewerOffActiveAndResolved);
            tests.Add(
                "Wpf.GroupPowerRecovery.ArmedOffExactPowerOnPromotesAndRejectedReplacementPreserves",
                GroupPowerArmedOffExactPowerOnPromotesAndRejectedReplacementPreserves);
            tests.Add(
                "Wpf.GroupPowerRecovery.DirectDisableAndMissingGroupStatusOnlyAreZeroWire",
                GroupPowerDirectDisableAndMissingGroupStatusOnlyAreZeroWire);
            tests.Add(
                "Wpf.GroupPowerRecovery.ConfirmedOffPrecheckFailurePreservesReplacement",
                GroupPowerConfirmedOffPrecheckFailurePreservesReplacement);
            tests.Add(
                "Wpf.GroupPowerState.SingleReadDoesNotCompletePendingPowerOff",
                SingleStatusReadDoesNotCompletePendingPowerOff);
            tests.Add(
                "Wpf.GroupPowerState.SingleReadDoesNotPromoteUnverifiedLock",
                SingleStatusReadDoesNotPromoteUnverifiedLock);
            tests.Add(
                "Wpf.GroupEnable.LastSampleSafetyReservationKeepsPendingUntilDisableWithoutReplay",
                LastGroupEnableSampleSafetyReservationKeepsPendingUntilDisableWithoutReplay);
            tests.Add(
                "Wpf.GroupEnable.LastSampleSafetyReservationStablePowerOffClearsPendingWithoutReplay",
                LastGroupEnableSampleSafetyReservationStablePowerOffClearsPendingWithoutReplay);
            tests.Add(
                "Wpf.GroupEnable.ConnectionLossPreservesExactGroupRecoveryWithoutReplay",
                GroupEnableConnectionLossPreservesExactGroupRecoveryWithoutReplay);
            tests.Add(
                "Wpf.GroupEnable.AcceptedObserverJournalFailurePreservesPersistenceError",
                AcceptedObserverJournalFailurePreservesPersistenceError);
            tests.Add(
                "Wpf.GroupEnable.DurableStartupExactIdentityDisableResolvesWithoutReplay",
                DurableStartupExactIdentityDisableResolvesWithoutReplay);
            tests.Add(
                "Wpf.GroupEnable.DurableStartupAcceptedExactIdentityResumesStatusOnly",
                DurableStartupAcceptedExactIdentityResumesStatusOnly);
            tests.Add(
                "Wpf.GroupEnable.DurableStartupEndpointMismatchBlocksBeforeRpc",
                DurableStartupEndpointMismatchBlocksBeforeRpc);
            tests.Add(
                "Wpf.GroupEnable.DurableStartupReferenceMismatchAllowsLookupOnly",
                DurableStartupReferenceMismatchAllowsLookupOnly);
            tests.Add(
                "Wpf.GroupEnable.ArmedJournalWithoutVolatileStatePromotesForRecovery",
                ArmedJournalWithoutVolatileStatePromotesForRecovery);
            tests.Add(
                "Wpf.GroupEnable.PostIdentitySafetyReservationKeepsDurableRecovery",
                PostIdentitySafetyReservationKeepsDurableRecovery);
            tests.Add(
                "Wpf.GroupEnable.SafetyReservationInvalidatesPublishedProofWithoutReplay",
                SafetyReservationInvalidatesPublishedProofWithoutReplay);
            tests.Add(
                "Wpf.GroupDisable.FreshDurableArmAcceptedResolved",
                FreshGroupDisableDurableLifecycle);
            tests.Add(
                "Wpf.GroupDisable.FreshRejectedRemainsUncertain",
                FreshRejectedGroupDisableRemainsUncertain);
            tests.Add(
                "Wpf.GroupDisable.LockTakeoverRejectedRemainsUncertain",
                RejectedGroupDisableLockTakeoverRemainsUncertain);
            tests.Add(
                "Wpf.GroupDisable.AcceptedUnlockRestartIsStatusOnly",
                AcceptedGroupDisableRestartIsStatusOnly);
            tests.Add(
                "Wpf.GroupDisable.PowerOffDoesNotResolveUnlock",
                GroupDisablePowerOffDoesNotResolveUnlock);
            tests.Add(
                "Wpf.GroupDisable.ArmedStartupRequiresExplicitRecoveryRetry",
                ArmedGroupDisableStartupRequiresExplicitRetry);
            tests.Add(
                "Wpf.GroupDisable.JournalFaultBlocksActiveMutationsBeforeWire",
                GroupDisableJournalFaultBlocksActiveMutationsBeforeWire);
            tests.Add(
                "Wpf.GroupDisable.PostIdentitySafetyKeepsUnlockPending",
                GroupDisablePostIdentitySafetyKeepsUnlockPending);
            tests.Add(
                "Wpf.GroupDisable.NewerResolvedSafetyRejectsStaleOutcome",
                GroupDisableNewerResolvedSafetyRejectsStaleOutcome);
            tests.Add(
                "Wpf.GroupDisable.PowerOffSupersedesPendingWithoutReplay",
                GroupDisablePowerOffSupersedesPendingWithoutReplay);
        }

        internal static FakeRpcStep[] CreateGroupPowerProcessRpcSteps(
            bool expectedPowerOn,
            ManualResetEventSlim firstStatusRelease = null,
            ManualResetEventSlim firstStatusEntered = null)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(expectedPowerOn
                ? GroupPowerOnStep()
                : GroupPowerOffStep());
            var heldStatus = GroupEnableWaitStatusStep(
                expectedPowerOn ? 0 : GroupEnableWaitPowerOn);
            heldStatus.CloseClientAfterResponseAndContinue = true;
            if (firstStatusRelease != null)
            {
                heldStatus.BeforeResponse = () =>
                {
                    if (firstStatusEntered != null)
                    {
                        firstStatusEntered.Set();
                    }

                    if (!firstStatusRelease.Wait(15000))
                    {
                        throw new TimeoutException(
                            "The held Group Power status response was not released.");
                    }
                };
                heldStatus.AllowClientDisconnectAfterRequest = true;
                heldStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
            }
            steps.Add(heldStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        internal static FakeRpcStep[] CreateGroupEnableAcceptedProcessRpcSteps(
            ManualResetEventSlim firstStatusRelease,
            ManualResetEventSlim firstStatusEntered)
        {
            if (firstStatusRelease == null)
            {
                throw new ArgumentNullException("firstStatusRelease");
            }

            if (firstStatusEntered == null)
            {
                throw new ArgumentNullException("firstStatusEntered");
            }

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var lockedStandby = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitEnableStep());
            var heldStatus = GroupEnableWaitStatusStep(lockedStandby);
            heldStatus.CloseClientAfterResponseAndContinue = true;
            heldStatus.BeforeResponse = () =>
            {
                firstStatusEntered.Set();
                if (!firstStatusRelease.Wait(15000))
                {
                    throw new TimeoutException(
                        "The held Group Enable status response was not released.");
                }
            };
            heldStatus.AllowClientDisconnectAfterRequest = true;
            heldStatus.ContinueWithNextClientAfterResponseWriteDisconnect = true;
            steps.Add(heldStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        internal static FakeRpcStep[] CreateGroupDisableAcceptedProcessRpcSteps(
            ManualResetEventSlim firstStatusRelease,
            ManualResetEventSlim firstStatusEntered)
        {
            if (firstStatusRelease == null)
            {
                throw new ArgumentNullException("firstStatusRelease");
            }

            if (firstStatusEntered == null)
            {
                throw new ArgumentNullException("firstStatusEntered");
            }

            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupDisableStep());
            var heldStatus = GroupEnableWaitStatusStep(stableDisabled);
            heldStatus.CloseClientAfterResponseAndContinue = true;
            heldStatus.BeforeResponse = () =>
            {
                firstStatusEntered.Set();
                if (!firstStatusRelease.Wait(15000))
                {
                    throw new TimeoutException(
                        "The held Group Disable status response was not released.");
                }
            };
            heldStatus.AllowClientDisconnectAfterRequest = true;
            heldStatus.ContinueWithNextClientAfterResponseWriteDisconnect = true;
            steps.Add(heldStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        private static void
            PostIdentitySafetyReservationKeepsDurableRecovery()
        {
            var lockedState = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitEnableStep());
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));

            using (var postIdentityRequested = new ManualResetEventSlim(false))
            {
                var delayedPostIdentity = CapabilitiesStep(
                    12,
                    LMCDiagnosticCapability.EtherCATTopology);
                delayedPostIdentity.InspectRequest = request =>
                    postIdentityRequested.Set();
                delayedPostIdentity.ResponseDelayMilliseconds = 500;
                steps.Add(delayedPostIdentity);
                steps.Add(GroupStopStep());
                steps.Add(GroupEnableWaitStatusStep(lockedState));
                steps.Add(GroupEnableWaitStatusStep(lockedState));
                steps.Add(GroupEnableWaitStatusStep(lockedState));
                steps.Add(CapabilitiesStep(
                    13,
                    LMCDiagnosticCapability.EtherCATTopology));
                steps.Add(GroupDisableStep());
                AddStableGroupDisabledSteps(steps);
                steps.Add(CapabilitiesStep(
                    14,
                    LMCDiagnosticCapability.EtherCATTopology));
                steps.Add(CloseStep());

                var journalDirectory = CreateJournalDirectory();
                MainWindow window = null;
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreatePreparedGroupEnableWindow(
                            journalDirectory,
                            server.Port);

                        Click(window.ButtonGroupEnable);
                        try
                        {
                            WaitUntil(
                                () => postIdentityRequested.IsSet,
                                "The final profile-lock identity request did not start.");
                        }
                        catch (TimeoutException error)
                        {
                            throw new InvalidOperationException(
                                "Post-identity request timeout. Operation="
                                + window.TextOperationState.Text
                                + ", Enable="
                                + CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x2047)
                                + ", Status="
                                + CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x2045)
                                + ", Capabilities="
                                + CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x7E00)
                                + ".",
                                error);
                        }

                        Click(window.ButtonGroupStop);
                        WaitUntil(
                            () => (bool)GetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired")
                                && !((bool)GetPrivateField(
                                    window,
                                    "groupProfileLocked"))
                                && string.Equals(
                                    window.TextOperationState.Text,
                                    "Group Stop verified",
                                    StringComparison.Ordinal),
                            "A safety reservation during the final identity read did not retain durable recovery.");

                        AssertEx.Equal(
                            GroupProfileLockRecoveryState.RecoveryRequired,
                            GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State);
                        AssertEx.Equal(
                            1,
                            CountRequestCommand(
                                server.ReceivedRequests,
                                0x2047),
                            "The stale completion replayed Group Enable.");
                        AssertEx.True(window.ButtonGroupDisable.IsEnabled);
                        AssertEx.False(window.ButtonGroupEnable.IsEnabled);

                        Click(window.ButtonGroupDisable);
                        WaitUntil(
                            () => !((bool)GetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired"))
                                && GetGroupProfileLockRecoveryJournal(window)
                                    .CurrentRecord.State
                                    == GroupProfileLockRecoveryState.Resolved,
                            "Disable did not resolve post-identity safety recovery.");
                        AssertEx.Equal(
                            1,
                            CountRequestCommand(
                                server.ReceivedRequests,
                                0x2048));

                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowWithActiveRecoveryBestEffort(window);
                    DeleteJournalDirectory(journalDirectory);
                }
            }
        }

        private static void SafetyReservationInvalidatesPublishedProofWithoutReplay()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var steps = CreateConnectAndTopologySteps(
                    LMCDiagnosticCapability.EtherCATTopology);
                steps.Add(GroupEnableWaitLookupStep());
                var enable = GroupEnableWaitEnableStep();
                var inspectEnable = enable.InspectRequest;
                enable.InspectRequest = request =>
                {
                    inspectEnable(request);
                    cancellation.Cancel();
                };
                steps.Add(enable);
                steps.Add(GroupEnableWaitStatusStep(
                    GroupEnableWaitPowerOn | GroupEnableWaitStandby));
                steps.Add(GroupEnableWaitStatusStep(
                    GroupEnableWaitPowerOn | GroupEnableWaitStandby));
                steps.Add(GroupEnableWaitStatusStep(
                    GroupEnableWaitPowerOn | GroupEnableWaitStandby));
                steps.Add(GroupDisableStep());
                steps.Add(CloseStep());

                var journalDirectory = CreateJournalDirectory();
                MainWindow window = null;
                Exception testError = null;
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreatePreparedGroupPowerWindow(
                            journalDirectory,
                            server.Port,
                            false);
                        var currentGroup = (LMCGroupAxis)GetPrivateField(
                            window,
                            "group");
                        var canceled = AssertEx.Throws<
                            LMCGroupEnableWaitCanceledException>(
                            () => currentGroup
                                .GroupEnableAndWaitForLockedStandbyAsync(
                                    cancellation.Token)
                                .GetAwaiter()
                                .GetResult());
                        var continuation = canceled.Continuation;

                        currentGroup.GroupReadStatusResult();
                        currentGroup.GroupReadStatusResult();
                        currentGroup.GroupReadStatusResult();
                        AssertEx.Equal(3, continuation.StableSampleCount);
                        AssertEx.True(continuation.IsPending);

                        var safetyTask = (Task)InvokePrivate(
                            window,
                            "RunSafetyCommandAsync",
                            "Synthetic safety reservation",
                            new Func<long, Task>(
                                generation => Task.CompletedTask),
                            null,
                            false);
                        safetyTask.GetAwaiter().GetResult();

                        AssertEx.True(ReferenceEquals(
                            continuation,
                            currentGroup.PendingGroupEnableWaitContinuation));
                        AssertEx.True(continuation.IsPending);
                        AssertEx.Equal(3, continuation.PollCount);
                        AssertEx.Equal(0, continuation.StableSampleCount);
                        AssertEx.Equal(0, continuation.DisabledUnlockedSampleCount);
                        AssertEx.Equal(0, continuation.PoweredOffSampleCount);
                        AssertEx.Equal(
                            1,
                            CountRequestCommand(
                                server.ReceivedRequests,
                                0x2047));

                        currentGroup.GroupDisable();
                        InvokePrivate(window, "UpdateUiState");
                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                catch (Exception error)
                {
                    testError = error;
                    throw;
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    try
                    {
                        DeleteJournalDirectory(journalDirectory);
                    }
                    catch (IOException) when (testError != null)
                    {
                        // Preserve the test failure when cleanup is still
                        // waiting on a handle retained by the failed path.
                    }
                }
            }
        }

        private static void
            DurableStartupAcceptedExactIdentityResumesStatusOnly()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var lockedStandby = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            Guid identity = Guid.Empty;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    using (var journal =
                        GroupProfileLockRecoveryJournal.Open(
                            GetGroupProfileLockRecoveryJournalDirectory(
                                journalDirectory)))
                    {
                        var armed = journal.ArmBeforeDispatch(
                            "127.0.0.1",
                            server.Port,
                            GroupEnableWaitName,
                            GroupEnableWaitReference,
                            DiagnosticsBootId,
                            DiagnosticMapRevision,
                            DateTime.UtcNow);
                        identity = armed.Identity;
                        journal.MarkAccepted(
                            identity,
                            armed.UpdatedUtc.AddTicks(1));
                    }

                    window = CreateWindow(journalDirectory, server.Port);
                    var activeJournal =
                        GetGroupProfileLockRecoveryJournal(window);
                    AssertEx.True(
                        activeJournal.HasActiveRecord,
                        "The accepted durable journal was not active at MainWindow startup.");
                    AssertEx.Equal(
                        identity,
                        activeJournal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        activeJournal.CurrentRecord.State);
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockAcceptedRestartRecovery"),
                        "Startup did not select accepted status-only recovery.");
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"));
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockVerificationPending"),
                        "Startup did not retain pending accepted verification.");

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLookupGroup.IsEnabled,
                        "The accepted durable endpoint identity did not reconnect.");

                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                                window.TextGroupReference.Text,
                                GroupEnableWaitReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonGroupEnable.IsEnabled,
                        "The accepted durable group identity did not expose status-only recovery.");
                    AssertEx.Contains(
                        "No 0x2047 Replay",
                        Convert.ToString(
                            window.ButtonGroupEnable.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => !activeJournal.HasActiveRecord
                            && activeJournal.CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved
                            && (bool)GetPrivateField(
                                window,
                                "groupProfileLocked")
                            && window.ButtonCloseConnection.IsEnabled,
                        "Three status-only samples did not resolve the accepted Group Enable recovery.");

                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2047),
                        "Accepted restart recovery replayed Group Enable.");
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045),
                        "Accepted restart recovery did not use exactly three status-only samples.");
                    AssertEx.Equal(
                        identity,
                        activeJournal.CurrentRecord.Identity);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(
                        GetGroupProfileLockRecoveryJournalDirectory(
                            journalDirectory)))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            DurableStartupExactIdentityDisableResolvesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupDisableStep());
            AddStableGroupDisabledSteps(steps);
            steps.Add(CapabilitiesStep(
                13,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    ArmGroupProfileLockRecoveryJournal(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitReference);

                    window = CreateWindow(journalDirectory, server.Port);
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRecoveredAtStartup"),
                        "The new MainWindow did not restore the active journal.");
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"));
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State,
                        "Startup did not promote ArmedBeforeDispatch to RecoveryRequired.");
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockAcceptedRestartRecovery"),
                        "ArmedBeforeDispatch was incorrectly treated as accepted status-only recovery.");

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLookupGroup.IsEnabled,
                        "The exact durable endpoint identity did not reconnect.");

                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                                window.TextGroupReference.Text,
                                GroupEnableWaitReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonGroupDisable.IsEnabled,
                        "The exact durable group identity did not load.");
                    AssertEx.False(
                        window.ButtonGroupEnable.IsEnabled,
                        "ArmedBeforeDispatch exposed Group Enable instead of safety-only recovery.");

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved
                            || window.TextOperationState.Text.EndsWith(
                                "failed",
                                StringComparison.Ordinal),
                        "Disable did not resolve the durable recovery record.");
                    AssertEx.False(
                        window.TextOperationState.Text.EndsWith(
                            "failed",
                            StringComparison.Ordinal),
                        "Disable failed. Result="
                            + window.TextGroupResult.Text
                            + " Log="
                            + window.TextExecutionLog.Text);

                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRecoveredAtStartup"));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047),
                        "Startup recovery replayed Group Enable.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2048),
                        "Startup recovery did not send exactly one Disable.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = GroupProfileLockRecoveryJournal.Open(
                    GetGroupProfileLockRecoveryJournalDirectory(
                        journalDirectory)))
                {
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.Resolved,
                        reopened.CurrentRecord.State,
                        "The resolved startup recovery state did not survive reopen.");
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void DurableStartupEndpointMismatchBlocksBeforeRpc()
        {
            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer())
                {
                    ArmGroupProfileLockRecoveryJournal(
                        journalDirectory,
                        "127.0.0.2",
                        server.Port,
                        GroupEnableWaitReference);

                    window = CreateWindow(journalDirectory, server.Port);
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRecoveredAtStartup"));
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Connect failed",
                            StringComparison.Ordinal),
                        "A mismatched durable endpoint was not rejected.");

                    AssertEx.Equal(
                        0,
                        server.AcceptedClientCount,
                        "Endpoint mismatch reached TCP connect.");
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests.Count,
                        "Endpoint mismatch sent RPC traffic.");
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State,
                        "Endpoint mismatch changed the durable recovery state.");

                    window.Close();
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.IsLoaded,
                        "Active recovery did not block normal window close.");
                    ForceCloseWindowWithActiveRecovery(window);
                    window = null;
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            DurableStartupReferenceMismatchAllowsLookupOnly()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitLookupStep(
                checked((ushort)(GroupEnableWaitReference + 1))));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    ArmGroupProfileLockRecoveryJournal(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitReference);

                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLookupGroup.IsEnabled,
                        "The durable recovery connection did not become ready.");

                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Group failed",
                            StringComparison.Ordinal),
                        "A mismatched group reference was not rejected after lookup.");

                    AssertEx.True(
                        GetPrivateField(window, "group") == null,
                        "The mismatched group was retained by the window.");
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"));
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State,
                        "Reference mismatch changed the durable recovery state.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x1042),
                        "Reference mismatch did not stop after one group lookup.");
                    AssertEx.Equal(
                        0,
                        CountGroupMutationRequests(server.ReceivedRequests),
                        "Reference mismatch sent a group mutation.");

                    CloseConnectionWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            ArmedJournalWithoutVolatileStatePromotesForRecovery()
        {
            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(journalDirectory, 5000);
                window.TextGroupName.Text = GroupEnableWaitName;
                var journal = GetGroupProfileLockRecoveryJournal(window);

                AssertEx.False(journal.HasActiveRecord);
                AssertEx.False(
                    (bool)GetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired"));
                AssertEx.True(
                    GetPrivateField(
                        window,
                        "pendingGroupEnableWaitContinuation") == null);

                journal.ArmBeforeDispatch(
                    "127.0.0.1",
                    5000,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);

                AssertEx.Equal(
                    GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                    journal.CurrentRecord.State);
                AssertEx.True(
                    (bool)InvokePrivate(
                        window,
                        "HasUnresolvedGroupProfileLockState"),
                    "An armed journal was invisible to the unresolved-state gate.");

                InvokePrivate(
                    window,
                    "PromotePendingGroupProfileLockToRecovery",
                    "Synthetic connection-loss window");
                AssertEx.Equal(
                    GroupProfileLockRecoveryState.RecoveryRequired,
                    journal.CurrentRecord.State,
                    "The armed connection-loss window was not promoted.");
                AssertEx.True(
                    (bool)GetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired"));
                AssertEx.True(
                    GetPrivateField(
                        window,
                        "pendingGroupEnableWaitContinuation") == null,
                    "The journal-only recovery unexpectedly created a continuation.");

                window.Close();
                PumpDispatcherOnce();
                AssertEx.True(
                    window.IsLoaded,
                    "An armed recovery journal did not block normal window close.");
                ForceCloseWindowWithActiveRecovery(window);
                window = null;

                using (var reopened = GroupProfileLockRecoveryJournal.Open(
                    GetGroupProfileLockRecoveryJournalDirectory(
                        journalDirectory)))
                {
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        reopened.CurrentRecord.State,
                        "The promoted journal-only recovery did not survive reopen.");
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupEnableConnectionLossPreservesExactGroupRecoveryWithoutReplay()
        {
            var lockedStandby = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            var acceptedEnableThenConnectionLoss = GroupEnableWaitEnableStep();
            acceptedEnableThenConnectionLoss
                .CloseClientAfterResponseAndContinue = true;
            steps.Add(acceptedEnableThenConnectionLoss);

            var recoverySteps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.AddRange(recoverySteps);
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(GroupEnableWaitStatusStep(lockedStandby));
            steps.Add(CapabilitiesStep(
                13,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupEnableWindow(
                        journalDirectory,
                        server.Port);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupProfileLockAcceptedRestartRecovery")
                            && !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && (bool)GetPrivateField(
                                window,
                                "groupProfileLockVerificationPending")
                            && GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                                == GroupProfileLockRecoveryState
                                    .AcceptedAwaitingProof
                            && string.Equals(
                                Convert.ToString(
                                    GetPrivateField(
                                        window,
                                        "groupProfileLockRecoveryGroupName"),
                                    CultureInfo.InvariantCulture),
                                GroupEnableWaitName,
                                StringComparison.Ordinal)
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Enable (Lock Profile) failed",
                                StringComparison.Ordinal),
                        "Connection loss after the accepted Enable did not preserve exact-group recovery state.");

                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockVerificationPending"));
                    AssertEx.False(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.True(window.ButtonConnect.IsEnabled);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLookupGroup.IsEnabled,
                        "The recovery connection did not become ready for the exact group reload.");

                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"));
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockAcceptedRestartRecovery"));
                    AssertEx.Equal(
                        GroupEnableWaitName,
                        Convert.ToString(
                            GetPrivateField(
                                window,
                                "groupProfileLockRecoveryGroupName"),
                            CultureInfo.InvariantCulture));
                    AssertEx.Equal(GroupEnableWaitName, window.TextGroupName.Text);

                    var textChangedMethod = typeof(MainWindow).GetMethod(
                        "TextGroupName_TextChanged",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic);
                    AssertEx.NotNull(textChangedMethod);
                    var textChangedHandler =
                        (System.Windows.Controls.TextChangedEventHandler)
                            Delegate.CreateDelegate(
                                typeof(System.Windows.Controls.TextChangedEventHandler),
                                window,
                                textChangedMethod);
                    window.TextGroupName.TextChanged -= textChangedHandler;
                    try
                    {
                        window.TextGroupName.Text = "_DifferentGroup";
                    }
                    finally
                    {
                        window.TextGroupName.TextChanged += textChangedHandler;
                    }

                    var requestsBeforeDifferentGroupLookup =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonLookupGroup_Click",
                        window.ButtonLookupGroup,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Group failed",
                            StringComparison.Ordinal),
                        "A different recovery-group lookup was not rejected.");
                    AssertEx.Equal(
                        requestsBeforeDifferentGroupLookup,
                        server.ReceivedRequests.Count,
                        "The rejected different-group lookup sent RPC traffic.");
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockAcceptedRestartRecovery"),
                        "A rejected different-group lookup cleared accepted recovery.");
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"),
                        "A rejected different-group lookup downgraded accepted recovery to safety-only recovery.");
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State);
                    AssertEx.Equal(
                        GroupEnableWaitName,
                        Convert.ToString(
                            GetPrivateField(
                                window,
                                "groupProfileLockRecoveryGroupName"),
                            CultureInfo.InvariantCulture));

                    window.TextGroupName.Text = GroupEnableWaitName;
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                                window.TextGroupReference.Text,
                                GroupEnableWaitReference.ToString(
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal)
                            && window.ButtonGroupEnable.IsEnabled,
                        "The exact recovery group was not reloaded.");
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockAcceptedRestartRecovery"),
                        "Reloading the exact group cleared accepted recovery before status verification.");
                    AssertEx.Contains(
                        "No 0x2047 Replay",
                        Convert.ToString(
                            window.ButtonGroupEnable.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => !GetGroupProfileLockRecoveryJournal(window)
                                .HasActiveRecord
                            && GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved
                            && (bool)GetPrivateField(
                                window,
                                "groupProfileLocked"),
                        "Status-only verification did not resolve accepted connection-loss recovery.");
                    AssertEx.Equal(
                        0,
                        CountCommandInSession(
                            server,
                            2,
                            0x2047));
                    AssertEx.Equal(
                        3,
                        CountCommandInSession(
                            server,
                            2,
                            0x2045));
                    AssertEx.Equal(
                        0,
                        CountCommandInSession(
                            server,
                            2,
                            0x2048));

                    CloseConnectedWindow(window);
                    window = null;
                    AssertEx.Equal(2, server.AcceptedClientCount);
                    server.Verify();
                }
            }
            finally
            {
                if (window != null)
                {
                    try
                    {
                        InvokePrivate(window, "ResetGroupPreparationState");
                        InvokePrivate(window, "ClearGroupProfileLockRecovery");
                        InvokePrivate(window, "UpdateUiState");
                    }
                    catch
                    {
                    }
                }

                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            AcceptedObserverJournalFailurePreservesPersistenceError()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));

            var journalDirectory = CreateJournalDirectory();
            var uiThreadId = Thread.CurrentThread.ManagedThreadId;
            var journalLockThreadId = 0;
            FileStream journalBlocker = null;
            var enable = GroupEnableWaitEnableStep();
            enable.BeforeResponse = () =>
            {
                journalBlocker = new FileStream(
                    Path.Combine(
                        GetGroupProfileLockRecoveryJournalDirectory(
                            journalDirectory),
                        GroupProfileLockRecoveryJournal.JournalFileName),
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.None);
                journalLockThreadId = Thread.CurrentThread.ManagedThreadId;
            };
            enable.CloseAfterResponse = true;
            steps.Add(enable);

            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupEnableWindow(
                        journalDirectory,
                        server.Port);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "operationRunning"))
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Enable (Lock Profile) failed",
                                StringComparison.Ordinal)
                            && window.TextExecutionLog.Text.IndexOf(
                                "Group profile-lock recovery journal faulted and remains fail-closed: mark-accepted: IOException:",
                                StringComparison.Ordinal) >= 0,
                        "The accepted observer journal failure was not reported on the UI thread.");

                    AssertEx.NotNull(
                        journalBlocker,
                        "The durable journal file was not locked before the Enable ACK.");
                    AssertEx.True(
                        journalLockThreadId != uiThreadId,
                        "The journal persistence failure was not forced from a worker-thread ACK path.");
                    AssertEx.Contains(
                        "mark-accepted: IOException:",
                        Convert.ToString(
                            GetPrivateField(
                                window,
                                "groupProfileLockRecoveryJournalRuntimeError"),
                            CultureInfo.InvariantCulture));

                    var executionLog = window.TextExecutionLog.Text;
                    var failurePrefix =
                        "Group Enable (Lock Profile) FAILED: ";
                    var failureIndex = executionLog.LastIndexOf(
                        failurePrefix,
                        StringComparison.Ordinal);
                    AssertEx.True(
                        failureIndex >= 0,
                        "The Group Enable failure log entry was not written.");
                    var failureEnd = executionLog.IndexOf(
                        Environment.NewLine,
                        failureIndex,
                        StringComparison.Ordinal);
                    var failureLine = failureEnd < 0
                        ? executionLog.Substring(failureIndex)
                        : executionLog.Substring(
                            failureIndex,
                            failureEnd - failureIndex);
                    AssertEx.Contains(
                        "mark-accepted: IOException:",
                        failureLine);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State,
                        "A failed accepted transition changed the durable journal state.");
                    AssertEx.True(
                        GetGroupProfileLockRecoveryJournal(window)
                            .HasActiveRecord,
                        "A failed accepted transition cleared the durable interlock.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));
                    server.Verify();
                }
            }
            finally
            {
                if (journalBlocker != null)
                {
                    journalBlocker.Dispose();
                }

                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void LastGroupEnableSampleSafetyReservationStablePowerOffClearsPendingWithoutReplay()
        {
            var lockedState = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitEnableStep());
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            var delayedFinalStatus = GroupEnableWaitStatusStep(lockedState);
            delayedFinalStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedFinalStatus);
            steps.Add(GroupStopStep());
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CapabilitiesStep(
                13,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupEnableWindow(
                        journalDirectory,
                        server.Port);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 3,
                        "The delayed final Group Enable status sample did not start.");

                    Click(window.ButtonGroupStop);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupProfileLockVerificationPending")
                            && !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && window.ButtonGroupPowerOff.IsEnabled
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Stop verified",
                                StringComparison.Ordinal)
                            && CountRequestCommand(
                                server.ReceivedRequests,
                                0x2085) == 1,
                        "The safety reservation did not preserve a fresh-proof pending lock state.");

                    AssertEx.True(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.Contains(
                        "Resume Lock Verification",
                        Convert.ToString(
                            window.ButtonGroupEnable.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));

                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "groupProfileLockVerificationPending"))
                            && !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && ((LMCGroupAxis)GetPrivateField(
                                window,
                                "group")).PendingGroupEnableWaitContinuation == null
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Power Off verified",
                                StringComparison.Ordinal),
                        "Three stable PowerOn=False samples did not clear the pending Enable proof.");
                    AssertEx.True(window.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.Contains(
                        "Group Power Off verified",
                        window.TextGroupResult.Text);

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2048));
                    AssertEx.Equal(
                        9,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void LastGroupEnableSampleSafetyReservationKeepsPendingUntilDisableWithoutReplay()
        {
            var lockedState = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitEnableStep());
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            var delayedFinalStatus = GroupEnableWaitStatusStep(lockedState);
            delayedFinalStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedFinalStatus);
            steps.Add(GroupStopStep());
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(GroupEnableWaitStatusStep(lockedState));
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupDisableStep());
            AddStableGroupDisabledSteps(steps);
            steps.Add(CapabilitiesStep(
                13,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupEnableWindow(
                        journalDirectory,
                        server.Port);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 3,
                        "The delayed final Group Enable status sample did not start.");

                    Click(window.ButtonGroupStop);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupProfileLockVerificationPending")
                            && !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && window.ButtonGroupDisable.IsEnabled
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Stop verified",
                                StringComparison.Ordinal)
                            && CountRequestCommand(
                                server.ReceivedRequests,
                                0x2085) == 1,
                        "The safety reservation did not preserve a Disable-resolvable pending lock state.");

                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.True(window.ButtonGroupEnable.IsEnabled);
                    AssertEx.Contains(
                        "Resume Lock Verification",
                        Convert.ToString(
                            window.ButtonGroupEnable.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.False(window.TextGroupName.IsEnabled);
                    AssertEx.False(window.ButtonLookupGroup.IsEnabled);
                    window.Close();
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.IsLoaded,
                        "Window close must remain blocked while Enable verification is pending.");
                    window.TextGroupName.Text = "_DifferentGroup";
                    AssertEx.Equal(
                        GroupEnableWaitName,
                        window.TextGroupName.Text,
                        "Pending Enable must reject a programmatic group-name mutation.");
                    AssertEx.NotNull(
                        ((LMCGroupAxis)GetPrivateField(
                            window,
                            "group")).PendingGroupEnableWaitContinuation);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "groupProfileLockVerificationPending"))
                            && !((bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"))
                            && ((LMCGroupAxis)GetPrivateField(
                                window,
                                "group")).PendingGroupEnableWaitContinuation == null
                            && window.ButtonGroupEnable.IsEnabled,
                        "Disable did not resolve the pending lock state.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2048));
                    AssertEx.Equal(
                        9,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void SingleStatusReadDoesNotPromoteUnverifiedLock()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Group Status completed",
                                StringComparison.Ordinal)
                            && window.ButtonGroupReadStatus.IsEnabled,
                        "The unverified Locked Standby observation did not settle.");

                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.Contains(
                        "not promoted to a verified profile lock",
                        window.TextExecutionLog.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void SingleStatusReadDoesNotCompletePendingPowerOff()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(
                        window,
                        "groupPowerOffVerificationPending",
                        true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Group Status completed",
                                StringComparison.Ordinal)
                            && window.ButtonGroupPowerOff.IsEnabled,
                        "The single pending Power Off status read did not settle.");

                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupPowerOffVerificationPending"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.Contains(
                        "Three consecutive samples are required",
                        window.TextExecutionLog.Text);

                    SetPrivateField(
                        window,
                        "groupPowerOffVerificationPending",
                        false);
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupStopPreemptsThenPowerOnResumesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOnStep());
            var delayedPowerOn = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn);
            delayedPowerOn.ResponseDelayMilliseconds = 200;
            steps.Add(delayedPowerOn);
            steps.Add(GroupStopStep());
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The pending Power On status request did not start before Group Stop.");

                    Click(window.ButtonGroupStop);
                    try
                    {
                        WaitUntil(
                            () => window.ButtonGroupPowerOff.IsEnabled
                                && (int)GetPrivateField(
                                    window,
                                    "safetyMonitorCount") == 0
                                && CountRequestCommand(
                                    server.ReceivedRequests,
                                    0x2085) == 1
                                && Convert.ToString(
                                    window.ButtonGroupPowerOff.Content,
                                    CultureInfo.InvariantCulture).IndexOf(
                                        "Safety Takeover",
                                        StringComparison.Ordinal) >= 0,
                            "Group Stop interference did not expose explicit Power Off takeover.");
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                            + " State="
                            + window.TextOperationState.Text
                            + ", Result="
                            + window.TextGroupResult.Text
                            + ", Log="
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    Click(window.ButtonGroupPowerOff);
                    try
                    {
                        WaitUntil(
                            () => !((bool)GetPrivateField(
                                    window,
                                    "groupPowerOffVerificationPending"))
                                && window.TextGroupResult.Text.IndexOf(
                                    "Group Power Off verified",
                                    StringComparison.Ordinal) >= 0,
                            "Explicit Power Off takeover did not resolve recovery.");
                    }
                    catch (TimeoutException error)
                    {
                        throw new TimeoutException(
                            error.Message
                            + " State="
                            + window.TextOperationState.Text
                            + ", Result="
                            + window.TextGroupResult.Text
                            + ", Log="
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        7,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void PowerOffTakesOwnershipFromPendingPowerOn()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOnStep());
            var delayedPowerOn = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn);
            delayedPowerOn.ResponseDelayMilliseconds = 200;
            steps.Add(delayedPowerOn);
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The pending Group Power On status request did not start.");

                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "groupPowerOffVerificationPending"))
                            && CountRequestCommand(
                                server.ReceivedRequests,
                                0x204B) == 1
                            && window.TextGroupResult.Text.IndexOf(
                                "Group Power Off verified",
                                StringComparison.Ordinal) >= 0,
                        "Group Power Off did not take and complete transition ownership.");

                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupPowerVerificationPending"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void StaleStatusReadIsDiscardedAfterSafetyReservation()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            var delayedStatus = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn);
            delayedStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedStatus);
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The delayed manual group status request did not start.");

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    coordinator.ReservePrioritySend();

                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Group Status failed",
                                StringComparison.Ordinal)
                            && window.ButtonGroupReadStatus.IsEnabled,
                        "The stale group status response was not rejected after safety reservation.");
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.Contains(
                        "response for command 0x2045 was discarded",
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void SingleStatusReadDoesNotPromoteUnverifiedPowerOn()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Group Status completed",
                                StringComparison.Ordinal)
                            && window.ButtonGroupReadStatus.IsEnabled,
                        "The unverified PowerOn status observation did not settle.");

                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.Contains(
                        "not promoted to ACTIVE",
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void SingleStatusReadDoesNotCompletePendingPowerOn()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    SetPrivateField(
                        window,
                        "groupPowerVerificationPending",
                        true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Read Group Status completed",
                                StringComparison.Ordinal)
                            && window.ButtonGroupPowerOn.IsEnabled,
                        "The single pending Power On status read did not settle.");

                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupPowerVerificationPending"));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupActiveVerified"));
                    AssertEx.Contains(
                        "three consecutive samples",
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    SetPrivateField(
                        window,
                        "groupPowerVerificationPending",
                        false);
                    InvokePrivate(window, "UpdateUiState");
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void PreemptedGroupPowerOffVerificationResumesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOffStep());
            var delayedStatus = GroupEnableWaitStatusStep(0);
            delayedStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedStatus);
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);

                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The first Group Power Off status poll did not start.");

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    coordinator.ReservePrioritySend();

                    WaitUntil(
                        () => window.ButtonGroupPowerOff.IsEnabled
                            && Convert.ToString(
                                window.ButtonGroupPowerOff.Content,
                                CultureInfo.InvariantCulture).IndexOf(
                                    "Resume Power Off Verification",
                                    StringComparison.Ordinal) >= 0,
                        "The preempted Power Off verification was not resumable.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));

                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "groupPowerOffVerificationPending"))
                            && window.TextGroupResult.Text.IndexOf(
                                "0x204B was not replayed",
                                StringComparison.Ordinal) >= 0,
                        "The GUI did not resume stable Group Power Off verification.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void PreemptedGroupPowerOnVerificationResumesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOnStep());
            var delayedStatus = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn);
            delayedStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedStatus);
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The first Group Power On status poll did not start.");

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    coordinator.ReservePrioritySend();

                    WaitUntil(
                        () => window.ButtonGroupPowerOn.IsEnabled
                            && Convert.ToString(
                                window.ButtonGroupPowerOn.Content,
                                CultureInfo.InvariantCulture).IndexOf(
                                    "Resume Power On Verification",
                                    StringComparison.Ordinal) >= 0,
                        "The preempted Power On verification was not resumable.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupActiveVerified")
                            && window.TextGroupResult.Text.IndexOf(
                                "ResumedWithout0x204AReplay=True",
                                StringComparison.Ordinal) >= 0,
                        "The GUI did not resume stable Group Power On verification.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupPowerOnUsesOneRequestAndThreeStableSamples()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOnStep());
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupActiveVerified")
                            && window.TextGroupResult.Text.IndexOf(
                                "Stable=3/3",
                                StringComparison.Ordinal) >= 0,
                        "The GUI did not finish stable Group Power On verification.");

                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupPowerVerificationPending"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        5,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupPowerAcceptedRestartOnAndOffAreStatusOnly()
        {
            AssertGroupPowerAcceptedRestartStatusOnly(true);
            AssertGroupPowerAcceptedRestartStatusOnly(false);
        }

        private static void AssertGroupPowerAcceptedRestartStatusOnly(
            bool expectedPowerOn)
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(GroupEnableWaitStatusStep(
                expectedPowerOn ? GroupEnableWaitPowerOn : 0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateGroupPowerRecoveryRecord(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        expectedPowerOn,
                        GroupPowerRecoveryState.AcceptedAwaitingProof);
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupGroup.IsEnabled,
                        "Accepted Group Power restart did not reconnect.");
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                            window.TextGroupReference.Text,
                            GroupEnableWaitReference.ToString(
                                CultureInfo.InvariantCulture),
                            StringComparison.Ordinal),
                        "Accepted Group Power restart did not load the group.");

                    var recoveryButton = expectedPowerOn
                        ? window.ButtonGroupPowerOn
                        : window.ButtonGroupPowerOff;
                    WaitUntil(
                        () => recoveryButton.IsEnabled,
                        "Accepted Group Power status-only recovery was not enabled.");
                    AssertEx.Contains(
                        expectedPowerOn ? "No 0x204A Replay" : "No 0x204B Replay",
                        Convert.ToString(
                            recoveryButton.Content,
                            CultureInfo.InvariantCulture));

                    Click(recoveryButton);
                    var journal = (GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal");
                    WaitUntil(
                        () => !journal.HasActiveRecord
                            && window.ButtonCloseConnection.IsEnabled,
                        "Accepted Group Power restart did not resolve from status-only proof.");
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = GroupPowerRecoveryJournal.Open(
                    Path.Combine(
                        journalDirectory,
                        "GroupPowerRecovery")))
                {
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            GroupPowerOlderFailurePreservesNewerOffActiveAndResolved()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupPowerOnStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");
                    var oldContinuation = currentGroup
                        .BeginGroupPowerOnWaitForStableStateAsync(
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(oldContinuation.IsPending);

                    var journal = (GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal");
                    var oldRecord = journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    oldRecord = journal.MarkAccepted(
                        oldRecord.Identity,
                        oldRecord.UpdatedUtc.AddTicks(1));
                    SetPrivateField(
                        window,
                        "pendingGroupPowerStateWaitContinuation",
                        oldContinuation);

                    var newerOff = journal
                        .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            oldRecord.Identity,
                            oldRecord.EndpointIp,
                            oldRecord.EndpointPort,
                            oldRecord.GroupName,
                            oldRecord.GroupReference,
                            oldRecord.DiagnosticsBootId,
                            oldRecord.MapRevision,
                            oldRecord.UpdatedUtc.AddTicks(2));

                    InvokePrivate(
                        window,
                        "PreserveGroupPowerWaitFailure",
                        currentGroup,
                        new InvalidOperationException("older Power On failed"),
                        oldRecord,
                        false,
                        false,
                        true,
                        oldContinuation,
                        "Older Group Power On");

                    var active = journal.CurrentRecord;
                    AssertEx.True(active.IsActive);
                    AssertEx.Equal(newerOff.Identity, active.Identity);
                    AssertEx.False(active.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.ArmedBeforeDispatch,
                        active.State);
                    AssertEx.True(oldContinuation.IsPending);
                    AssertEx.Equal(
                        null,
                        GetPrivateField(
                            window,
                            "pendingGroupPowerStateWaitContinuation"));
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "groupPowerVerificationPending"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerOffVerificationPending"));

                    InvokePrivate(
                        window,
                        "ResolveGroupPowerRecoveryJournal",
                        newerOff,
                        "Newer Group Power Off proof");
                    var resolved = journal.CurrentRecord;
                    AssertEx.Equal(newerOff.Identity, resolved.Identity);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        resolved.State);

                    InvokePrivate(
                        window,
                        "PreserveGroupPowerWaitFailure",
                        currentGroup,
                        new InvalidOperationException(
                            "older Power On failed after resolution"),
                        oldRecord,
                        false,
                        false,
                        true,
                        oldContinuation,
                        "Older Group Power On after resolution");
                    var tombstone = journal.CurrentRecord;
                    AssertEx.Equal(newerOff.Identity, tombstone.Identity);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        tombstone.State);
                    AssertEx.Equal(
                        null,
                        GetPrivateField(
                            window,
                            "pendingGroupPowerStateWaitContinuation"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            GroupPowerArmedOffExactPowerOnPromotesAndRejectedReplacementPreserves()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(new FakeRpcStep(
                0x204B,
                TestFrame.Response(1, new byte[8]))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x204B,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            });
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    var journal = (GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal");
                    var armed = journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);

                    Click(window.ButtonGroupReadStatus);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "groupPowerOffReplacementAllowed")
                            && window.ButtonGroupPowerOff.IsEnabled,
                        "Exact PowerOn=true status did not promote the armed Power Off recovery.");

                    var promoted = journal.CurrentRecord;
                    AssertEx.Equal(armed.Identity, promoted.Identity);
                    AssertEx.False(promoted.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.RecoveryRequired,
                        promoted.State);
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerRecoveryRequired"));
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonGroupPowerOff.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonGroupPowerOff);
                    WaitUntil(
                        () => CountRequestCommand(
                                server.ReceivedRequests,
                                0x204B) == 1
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning"),
                        "The rejected replacement Group Power Off did not settle.");

                    var preserved = journal.CurrentRecord;
                    AssertEx.True(preserved.IsActive);
                    AssertEx.Equal(armed.Identity, preserved.Identity);
                    AssertEx.False(preserved.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.RecoveryRequired,
                        preserved.State);
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerOffReplacementAllowed"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerRecoveryRequired"));
                    AssertEx.Equal(
                        null,
                        GetPrivateField(
                            window,
                            "pendingGroupPowerStateWaitContinuation"));
                    AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonGroupPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            GroupPowerDirectDisableAndMissingGroupStatusOnlyAreZeroWire()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    var journal = (GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal");
                    var armed = journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.False(window.ButtonGroupDisable.IsEnabled);

                    var requestCountBeforeDirectDisable =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonGroupDisable_Click",
                        window.ButtonGroupDisable,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Group Disable (Unlock Profile) failed",
                            StringComparison.Ordinal),
                        "A direct Group Disable call was not rejected during Group Power recovery.");
                    AssertEx.Equal(
                        requestCountBeforeDirectDisable,
                        server.ReceivedRequests.Count,
                        "Direct Group Disable sent RPC traffic during Group Power recovery.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2048));
                    AssertEx.Equal(armed.Identity, journal.CurrentRecord.Identity);
                    AssertEx.True(journal.CurrentRecord.IsActive);

                    SetPrivateField(window, "group", null);
                    InvokePrivate(window, "UpdateUiState");
                    var requestCountBeforeMissingGroupResume =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonGroupPowerOff_Click",
                        window.ButtonGroupPowerOff,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        "Resume Group Power Off Verification blocked",
                        window.TextOperationState.Text);
                    AssertEx.Equal(
                        requestCountBeforeMissingGroupResume,
                        server.ReceivedRequests.Count,
                        "Missing-group status-only recovery sent RPC traffic.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(armed.Identity, journal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);

                    CloseRecoveryConnectionForTest(window);
                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            GroupPowerConfirmedOffPrecheckFailurePreservesReplacement()
        {
            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer())
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    var journal = (GroupPowerRecoveryJournal)GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal");
                    var armed = journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        armed.UpdatedUtc.AddTicks(1));
                    SetPrivateField(
                        window,
                        "groupPowerRecoveryRequired",
                        true);
                    SetPrivateField(
                        window,
                        "groupPowerOffReplacementAllowed",
                        true);
                    SetPrivateField(
                        window,
                        "groupPowerOffVerificationPending",
                        true);

                    InvokePrivate(
                        window,
                        "PreserveGroupPowerWaitFailure",
                        null,
                        new InvalidOperationException(
                            "replacement identity precheck failed"),
                        recovery,
                        false,
                        true,
                        false,
                        null,
                        "Confirmed Group Power Off replacement");

                    var preserved = journal.CurrentRecord;
                    AssertEx.True(preserved.IsActive);
                    AssertEx.Equal(recovery.Identity, preserved.Identity);
                    AssertEx.False(preserved.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.RecoveryRequired,
                        preserved.State);
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerRecoveryRequired"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerOffReplacementAllowed"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupPowerOffVerificationPending"));
                    AssertEx.Equal(
                        null,
                        GetPrivateField(
                            window,
                            "pendingGroupPowerStateWaitContinuation"));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(0, server.ReceivedRequests.Count);

                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void FreshGroupDisableDurableLifecycle()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));

            MainWindow window = null;
            var disable = GroupDisableStep();
            var inspectDisable = disable.InspectRequest;
            disable.InspectRequest = request =>
            {
                inspectDisable(request);
                var record = GetGroupProfileLockRecoveryJournal(window)
                    .CurrentRecord;
                AssertEx.False(record.ExpectedProfileLocked);
                AssertEx.Equal(
                    GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                    record.State,
                    "Unlock was not durably armed before 0x2048 dispatch.");
            };
            steps.Add(disable);

            var firstStatus = GroupEnableWaitStatusStep(stableDisabled);
            var inspectFirstStatus = firstStatus.InspectRequest;
            firstStatus.InspectRequest = request =>
            {
                inspectFirstStatus(request);
                var record = GetGroupProfileLockRecoveryJournal(window)
                    .CurrentRecord;
                AssertEx.False(record.ExpectedProfileLocked);
                AssertEx.Equal(
                    GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                    record.State,
                    "The accepted observer did not flush before status polling.");
            };
            steps.Add(firstStatus);
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () =>
                        {
                            var record =
                                GetGroupProfileLockRecoveryJournal(window)
                                    .CurrentRecord;
                            return record != null
                                && record.State
                                    == GroupProfileLockRecoveryState.Resolved;
                        },
                        "Fresh Group Disable did not resolve after stable proof.");

                    var resolved = GetGroupProfileLockRecoveryJournal(window)
                        .CurrentRecord;
                    AssertEx.False(resolved.ExpectedProfileLocked);
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"),
                        "The volatile profile state was not cleared after durable resolve.");
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileUnlockVerificationPending"));
                    AssertEx.Contains(
                        "StableDisabled=3/3",
                        window.TextGroupResult.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void FreshRejectedGroupDisableRemainsUncertain()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupDisableRejectedStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "operationRunning"))
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Disable (Unlock Profile) failed",
                                StringComparison.Ordinal),
                        "Rejected fresh Group Disable did not settle.");

                    var record = GetGroupProfileLockRecoveryJournal(window)
                        .CurrentRecord;
                    AssertEx.NotNull(record);
                    AssertEx.True(record.IsActive);
                    AssertEx.False(record.ExpectedProfileLocked);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        record.State,
                        "A rejected fresh Disable incorrectly restored Lock even though PLC side effects are possible.");
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"),
                        "A rejected Disable exposed a stale profile-lock proof.");
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired"));
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "groupProfileUnlockVerificationPending"));
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);
                    AssertEx.False(
                        window.ButtonGroupMoveLinear.IsEnabled,
                        "A rejected Disable with possible Unlock side effects exposed Move.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectionWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            RejectedGroupDisableLockTakeoverRemainsUncertain()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupDisableRejectedStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    var journal = GetGroupProfileLockRecoveryJournal(window);
                    var locked = journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    locked = journal.PromoteToRecoveryRequired(
                        locked.Identity,
                        locked.UpdatedUtc.AddTicks(1));
                    InvokePrivate(
                        window,
                        "ApplyGroupProfileLockRecoveryRecord",
                        locked);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(window.ButtonGroupDisable.IsEnabled);

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "operationRunning"))
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Disable (Unlock Profile) failed",
                                StringComparison.Ordinal),
                        "Rejected Lock-to-Unlock takeover did not settle.");

                    var record = journal.CurrentRecord;
                    AssertEx.True(record.IsActive);
                    AssertEx.False(record.ExpectedProfileLocked);
                    AssertEx.True(
                        record.Identity != locked.Identity,
                        "The Lock record was not replaced by an Unlock identity.");
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        record.State,
                        "A rejected takeover incorrectly restored a previously uncertain lock.");
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "groupProfileLocked"));
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectionWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void AcceptedGroupDisableRestartIsStatusOnly()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateGroupProfileLockRecoveryRecord(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        false,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof);
                    window = CreateWindow(journalDirectory, server.Port);
                    AssertEx.True(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileUnlockAcceptedRestartRecovery"));

                    Click(window.ButtonConnect);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                    window.TextConnectionState.Text,
                                    "Connected",
                                    StringComparison.Ordinal)
                                && window.ButtonLookupGroup.IsEnabled,
                            "Accepted Unlock recovery did not reconnect.");
                    }
                    catch (TimeoutException)
                    {
                        var record = GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord;
                        throw new TimeoutException(
                            "Accepted Unlock recovery did not reconnect. "
                            + "ConnectionState="
                            + window.TextConnectionState.Text
                            + ", OperationState="
                            + window.TextOperationState.Text
                            + ", Requests="
                            + server.ReceivedRequests.Count.ToString(
                                CultureInfo.InvariantCulture)
                            + ", JournalState="
                            + (record == null
                                ? "<null>"
                                : record.State.ToString())
                            + ", Log="
                            + window.TextExecutionLog.Text);
                    }
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => window.ButtonGroupDisable.IsEnabled,
                        "Accepted Unlock recovery did not expose status-only verification.");
                    AssertEx.Contains(
                        "No 0x2048 Replay",
                        Convert.ToString(
                            window.ButtonGroupDisable.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.Contains(
                        "profile unlock accepted, Disabled proof pending",
                        window.TextGroupPreparationState.Text);
                    AssertEx.Contains(
                        "Resume Unlock Verification (status reads only; no 0x2048 replay)",
                        window.TextGroupPreparationState.Text);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                            == GroupProfileLockRecoveryState.Resolved,
                        "Accepted Unlock status-only recovery did not resolve.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2048),
                        "Accepted Unlock recovery replayed Group Disable.");
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupDisablePowerOffDoesNotResolveUnlock()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupDisableStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => window.TextOperationState.Text.EndsWith(
                                "failed",
                                StringComparison.Ordinal)
                            && (bool)GetPrivateField(
                                window,
                                "groupProfileUnlockVerificationPending"),
                        "PowerOff did not retain accepted Unlock recovery.");

                    var active = GetGroupProfileLockRecoveryJournal(window)
                        .CurrentRecord;
                    AssertEx.False(active.ExpectedProfileLocked);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        active.State,
                        "PowerOff incorrectly resolved the accepted Unlock journal.");
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"),
                        "An unresolved Unlock left the volatile profile-lock proof enabled.");
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.False(
                        window.ButtonGroupMoveLinear.IsEnabled,
                        "An unresolved Unlock exposed Move Linear after a stale volatile lock value.");
                    var requestsBeforeForcedMove =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonGroupMoveLinear_Click",
                        window.ButtonGroupMoveLinear,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Move Linear Absolute Send failed",
                            StringComparison.Ordinal),
                        "The Move Linear runtime guard did not reject unresolved Unlock.");
                    AssertEx.Equal(
                        requestsBeforeForcedMove,
                        server.ReceivedRequests.Count,
                        "The unresolved Unlock runtime guard allowed Move RPC traffic.");
                    SetPrivateField(window, "groupProfileLocked", false);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectionWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void ArmedGroupDisableStartupRequiresExplicitRetry()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(GroupDisableStep());
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateGroupProfileLockRecoveryRecord(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        false,
                        GroupProfileLockRecoveryState.ArmedBeforeDispatch);
                    window = CreateWindow(journalDirectory, server.Port);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        GetGroupProfileLockRecoveryJournal(window)
                            .CurrentRecord.State,
                        "Startup did not promote an Armed Unlock to RecoveryRequired.");

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonLookupGroup.IsEnabled,
                        "Armed Unlock recovery did not reconnect.");
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => window.ButtonGroupDisable.IsEnabled,
                        "RecoveryRequired Unlock did not expose explicit retry.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2048),
                        "Startup or lookup automatically replayed Group Disable.");
                    AssertEx.Contains(
                        "Retry Disable Explicitly",
                        Convert.ToString(
                            window.ButtonGroupDisable.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonGroupDisable);
                    WaitUntil(
                        () => GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                            == GroupProfileLockRecoveryState.Resolved,
                        "Explicit RecoveryRequired Unlock retry did not resolve.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupDisablePostIdentitySafetyKeepsUnlockPending()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            var lockedStandby = GroupEnableWaitPowerOn
                | GroupEnableWaitStandby;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupDisableStep());
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));

            using (var postIdentityRequested =
                new ManualResetEventSlim(false))
            {
                var delayedPostIdentity = CapabilitiesStep(
                    12,
                    capabilities);
                delayedPostIdentity.InspectRequest = request =>
                    postIdentityRequested.Set();
                delayedPostIdentity.ResponseDelayMilliseconds = 500;
                steps.Add(delayedPostIdentity);
                steps.Add(GroupStopStep());
                steps.Add(GroupEnableWaitStatusStep(lockedStandby));
                steps.Add(GroupEnableWaitStatusStep(lockedStandby));
                steps.Add(GroupEnableWaitStatusStep(lockedStandby));
                steps.Add(CapabilitiesStep(13, capabilities));
                steps.Add(GroupEnableWaitStatusStep(stableDisabled));
                steps.Add(GroupEnableWaitStatusStep(stableDisabled));
                steps.Add(GroupEnableWaitStatusStep(stableDisabled));
                steps.Add(CapabilitiesStep(14, capabilities));
                steps.Add(CloseStep());

                var journalDirectory = CreateJournalDirectory();
                MainWindow window = null;
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreatePreparedGroupPowerWindow(
                            journalDirectory,
                            server.Port,
                            true);
                        SetPrivateField(window, "groupProfileLocked", true);
                        InvokePrivate(window, "UpdateUiState");

                        Click(window.ButtonGroupDisable);
                        WaitUntil(
                            () => postIdentityRequested.IsSet,
                            "The Group Disable post-identity request did not start.");
                        Click(window.ButtonGroupStop);
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Group Stop verified",
                                    StringComparison.Ordinal)
                                && GetGroupProfileLockRecoveryJournal(window)
                                        .CurrentRecord.State
                                    == GroupProfileLockRecoveryState
                                        .AcceptedAwaitingProof
                                && window.ButtonGroupDisable.IsEnabled,
                            "A post-identity Stop did not retain status-only Unlock recovery.");

                        AssertEx.False(
                            (bool)GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired"),
                            "The stale Disable result was promoted instead of preserving its accepted journal.");
                        AssertEx.False(
                            (bool)GetPrivateField(window, "groupProfileLocked"));
                        AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                        AssertEx.Contains(
                            "No 0x2048 Replay",
                            Convert.ToString(
                                window.ButtonGroupDisable.Content,
                                CultureInfo.InvariantCulture));

                        Click(window.ButtonGroupDisable);
                        WaitUntil(
                            () => GetGroupProfileLockRecoveryJournal(window)
                                    .CurrentRecord.State
                                == GroupProfileLockRecoveryState.Resolved,
                            "Status-only Unlock recovery did not resolve after Stop.");
                        AssertEx.Equal(
                            1,
                            CountRequestCommand(
                                server.ReceivedRequests,
                                0x2048),
                            "Post-identity safety caused a Group Disable replay.");
                        AssertEx.Equal(
                            1,
                            CountRequestCommand(
                                server.ReceivedRequests,
                                0x2085));

                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowWithActiveRecoveryBestEffort(window);
                    DeleteJournalDirectory(journalDirectory);
                }
            }
        }

        private static void GroupDisableNewerResolvedSafetyRejectsStaleOutcome()
        {
            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(journalDirectory, 5000);
                var journal = GetGroupProfileLockRecoveryJournal(window);
                var record = journal.ArmBeforeDispatch(
                    false,
                    "127.0.0.1",
                    5000,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                record = journal.MarkAccepted(
                    record.Identity,
                    record.UpdatedUtc.AddTicks(1));
                InvokePrivate(
                    window,
                    "ApplyGroupProfileLockRecoveryRecord",
                    record);

                var coordinator = (LMCSendPriorityCoordinator)
                    GetPrivateField(window, "sendPriorityCoordinator");
                var disableSafetyGeneration = coordinator.CurrentGeneration;
                coordinator.ReservePrioritySend();
                journal.Resolve(
                    record.Identity,
                    record.UpdatedUtc.AddTicks(2));
                InvokePrivate(window, "ClearGroupProfileLockRecovery");

                AssertEx.True(
                    (bool)InvokePrivate(
                        window,
                        "TryDiscardGroupDisableOutcomeSupersededBySafety",
                        record.Identity,
                        disableSafetyGeneration,
                        "Synthetic late Group Disable"),
                    "The newer resolved safety tombstone did not discard the stale Disable outcome.");
                AssertEx.Equal(
                    GroupProfileLockRecoveryState.Resolved,
                    journal.CurrentRecord.State);
                AssertEx.False(
                    (bool)GetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired"),
                    "A late Disable outcome resurrected recovery after newer stable safety proof.");
                AssertEx.False(
                    (bool)GetPrivateField(window, "groupProfileLocked"));

                window.Close();
                PumpDispatcherOnce();
                window = null;
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void GroupDisablePowerOffSupersedesPendingWithoutReplay()
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupDisableStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");
                    SetPrivateField(window, "groupProfileLocked", true);
                    var journal = GetGroupProfileLockRecoveryJournal(window);
                    var unlockRecord = journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    InvokePrivate(
                        window,
                        "ApplyGroupProfileLockRecoveryRecord",
                        unlockRecord);
                    var pendingDisable = currentGroup
                        .BeginGroupDisableWaitForStableDisabledAsync(
                            new LMCGroupDisableWaitOptions(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    InvokePrivate(
                        window,
                        "MarkGroupProfileUnlockAccepted",
                        currentGroup,
                        pendingDisable,
                        "Synthetic accepted Group Disable");
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.NotNull(pendingDisable);
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        journal.CurrentRecord.State);
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);

                    Click(window.ButtonGroupPowerOff);
                    try
                    {
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Group Power Off verified",
                                    StringComparison.Ordinal)
                                && GetGroupProfileLockRecoveryJournal(window)
                                        .CurrentRecord.State
                                    == GroupProfileLockRecoveryState.Resolved
                                && currentGroup
                                    .PendingGroupDisableWaitContinuation == null
                                && !((bool)GetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryRequired"))
                                && !((bool)GetPrivateField(
                                    window,
                                    "groupProfileUnlockVerificationPending")),
                            "Stable Group Power Off did not retire and resolve the superseded Disable.");
                    }
                    catch (TimeoutException error)
                    {
                        throw new InvalidOperationException(
                            "Stable Power Off supersession timeout. Operation="
                            + window.TextOperationState.Text
                            + ", Journal="
                            + GetGroupProfileLockRecoveryJournal(window)
                                .CurrentRecord.State
                            + ", PendingDisable="
                            + (currentGroup.PendingGroupDisableWaitContinuation
                                == null
                                ? "null"
                                : currentGroup
                                    .PendingGroupDisableWaitContinuation.State
                                    .ToString())
                            + ", RecoveryRequired="
                            + GetPrivateField(
                                window,
                                "groupProfileLockRecoveryRequired")
                            + ", UnlockPending="
                            + GetPrivateField(
                                window,
                                "groupProfileUnlockVerificationPending")
                            + ", 2048="
                            + CountRequestCommand(
                                server.ReceivedRequests,
                                0x2048)
                            + ", 204B="
                            + CountRequestCommand(
                                server.ReceivedRequests,
                                0x204B)
                            + ", 2045="
                            + CountRequestCommand(
                                server.ReceivedRequests,
                                0x2045)
                            + ", Log="
                            + window.TextExecutionLog.Text,
                            error);
                    }

                    AssertEx.True(
                        pendingDisable.IsSuperseded,
                        "The pending Group Disable was cleared without supersession evidence.");
                    AssertEx.NotNull(
                        pendingDisable.SupersedingPowerOffContinuation);
                    AssertEx.False(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x2048));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(server.ReceivedRequests, 0x204B));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(server.ReceivedRequests, 0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            GroupDisableJournalFaultBlocksActiveMutationsBeforeWire()
        {
            AssertGroupDisableJournalFaultBlocksMutation(false);
            AssertGroupDisableJournalFaultBlocksMutation(true);
            AssertGroupDisableOpenFaultBlocksMutation(false);
            AssertGroupDisableOpenFaultBlocksMutation(true);
        }

        private static void AssertGroupDisableJournalFaultBlocksMutation(
            bool expectedProfileLocked)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    var journal = GetGroupProfileLockRecoveryJournal(window);
                    var record = journal.ArmBeforeDispatch(
                        expectedProfileLocked,
                        "127.0.0.1",
                        server.Port,
                        GroupEnableWaitName,
                        GroupEnableWaitReference,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    record = journal.PromoteToRecoveryRequired(
                        record.Identity,
                        record.UpdatedUtc.AddTicks(1));
                    InvokePrivate(
                        window,
                        "ApplyGroupProfileLockRecoveryRecord",
                        record);
                    SetPrivateField(window, "groupProfileLocked", true);
                    SetPrivateField(
                        window,
                        "groupProfileLockRecoveryJournalRuntimeError",
                        "forced-test-runtime-error");
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.False(
                        window.ButtonGroupDisable.IsEnabled,
                        "A faulted durable journal exposed an active Group Disable mutation.");
                    var requestsBeforeAttempt = server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonGroupDisable_Click",
                        window.ButtonGroupDisable,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "operationRunning"))
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Disable (Unlock Profile) failed",
                                StringComparison.Ordinal),
                        "The direct Group Disable handler did not fail closed for a faulted journal.");

                    AssertEx.Equal(
                        requestsBeforeAttempt,
                        server.ReceivedRequests.Count,
                        "A faulted durable journal allowed RPC traffic before rejecting Group Disable.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2048),
                        "A faulted durable journal allowed 0x2048 onto the wire.");

                    CloseConnectionWithActiveRecovery(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowWithActiveRecoveryBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void AssertGroupDisableOpenFaultBlocksMutation(
            bool expectedProfileLocked)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateGroupProfileLockRecoveryRecord(
                        journalDirectory,
                        "127.0.0.1",
                        server.Port,
                        expectedProfileLocked,
                        GroupProfileLockRecoveryState.RecoveryRequired);
                    var journalPath = Path.Combine(
                        GetGroupProfileLockRecoveryJournalDirectory(
                            journalDirectory),
                        GroupProfileLockRecoveryJournal.JournalFileName);
                    var journalBytes = File.ReadAllBytes(journalPath);
                    journalBytes[journalBytes.Length - 1] ^= 0xFF;
                    File.WriteAllBytes(journalPath, journalBytes);

                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "groupProfileLockRecoveryJournal") == null,
                        "A corrupt durable journal unexpectedly opened.");
                    AssertEx.True(
                        !string.IsNullOrWhiteSpace(
                            Convert.ToString(
                                GetPrivateField(
                                    window,
                                    "groupProfileLockRecoveryJournalOpenError"),
                                CultureInfo.InvariantCulture)),
                        "A corrupt durable journal did not retain its open error.");
                    AssertEx.False(
                        window.ButtonGroupDisable.IsEnabled,
                        "An unavailable durable journal exposed Group Disable.");

                    var requestsBeforeAttempt = server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonGroupDisable_Click",
                        window.ButtonGroupDisable,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => !((bool)GetPrivateField(
                                window,
                                "operationRunning"))
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Group Disable (Unlock Profile) failed",
                                StringComparison.Ordinal),
                        "The direct Group Disable handler did not fail closed for an unavailable journal.");

                    AssertEx.Equal(
                        requestsBeforeAttempt,
                        server.ReceivedRequests.Count,
                        "An unavailable durable journal allowed RPC traffic before rejecting Group Disable.");
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(server.ReceivedRequests, 0x2048),
                        "An unavailable durable journal allowed 0x2048 onto the wire.");

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static MainWindow CreatePreparedGroupPowerWindow(
            string journalDirectory,
            int rpcPort,
            bool activeVerified)
        {
            var window = CreateWindow(journalDirectory, rpcPort);
            Click(window.ButtonConnect);
            WaitUntil(
                () => window.GridEtherCATTopology.Items.Count
                    == TopologyNodeCount,
                "Topology auto-load did not complete before the group power test.");

            window.TextGroupName.Text = GroupEnableWaitName;
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                    window.TextGroupReference.Text,
                    GroupEnableWaitReference.ToString(
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal),
                "The group lookup did not complete.");

            SetPrivateField(window, "groupActiveVerified", activeVerified);
            SetPrivateField(window, "groupIdentityConfigured", activeVerified);
            InvokePrivate(window, "UpdateUiState");
            if (activeVerified)
            {
                AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);
            }
            else
            {
                AssertEx.True(window.ButtonGroupPowerOn.IsEnabled);
            }
            return window;
        }

        private static void PreemptedGroupEnableVerificationResumesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitEnableStep());
            var delayedStatus = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn);
            delayedStatus.ResponseDelayMilliseconds = 200;
            steps.Add(delayedStatus);
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupEnableWindow(
                        journalDirectory,
                        server.Port);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The first Group Enable status poll did not start.");

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    coordinator.ReservePrioritySend();

                    WaitUntil(
                        () => window.ButtonGroupEnable.IsEnabled
                            && Convert.ToString(
                                window.ButtonGroupEnable.Content,
                                CultureInfo.InvariantCulture).IndexOf(
                                    "Resume Lock Verification",
                                    StringComparison.Ordinal) >= 0,
                        "The preempted accepted Enable was not exposed as resumable.");
                    AssertEx.Contains(
                        "no 0x2047 replay is allowed",
                        window.TextGroupResult.Text);
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));

                    AssertEx.False(window.ButtonConnect.IsEnabled);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.False(window.TextGroupName.IsEnabled);
                    AssertEx.False(window.ButtonLookupGroup.IsEnabled);
                    AssertEx.False(window.ButtonGroupPowerOn.IsEnabled);
                    window.Close();
                    PumpDispatcherOnce();
                    AssertEx.True(
                        window.IsLoaded,
                        "Window close must remain blocked while accepted Enable verification is pending.");
                    window.TextGroupName.Text = "_DifferentGroup";
                    AssertEx.Equal(
                        GroupEnableWaitName,
                        window.TextGroupName.Text,
                        "Pending Enable must reject a group-name mutation.");
                    var requestCountBeforeForcedExitAttempts =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonConnect_Click",
                        window.ButtonConnect,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Connect failed",
                            StringComparison.Ordinal),
                        "Forced reconnect was not rejected while Enable was pending.");
                    InvokePrivate(
                        window,
                        "ButtonLookupGroup_Click",
                        window.ButtonLookupGroup,
                        new RoutedEventArgs());
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Load Group failed",
                            StringComparison.Ordinal),
                        "Forced group reload was not rejected while Enable was pending.");
                    InvokePrivate(
                        window,
                        "ButtonCloseConnection_Click",
                        window.ButtonCloseConnection,
                        new RoutedEventArgs());
                    InvokePrivate(
                        window,
                        "ButtonGroupPowerOn_Click",
                        window.ButtonGroupPowerOn,
                        new RoutedEventArgs());
                    PumpDispatcherOnce();
                    AssertEx.Equal(
                        requestCountBeforeForcedExitAttempts,
                        server.ReceivedRequests.Count,
                        "A forced pending-state exit or Power On attempt sent RPC traffic.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => window.ButtonGroupDisable.IsEnabled
                            && window.TextGroupResult.Text.IndexOf(
                                "ReusedACK=True",
                                StringComparison.Ordinal) >= 0,
                        "The GUI did not resume the accepted Enable verification.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void ArmGroupProfileLockRecoveryJournal(
            string journalDirectory,
            string endpointIp,
            int endpointPort,
            ushort groupReference)
        {
            using (var journal = GroupProfileLockRecoveryJournal.Open(
                GetGroupProfileLockRecoveryJournalDirectory(
                    journalDirectory)))
            {
                journal.ArmBeforeDispatch(
                    endpointIp,
                    endpointPort,
                    GroupEnableWaitName,
                    groupReference,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
            }
        }

        private static GroupProfileLockRecoveryRecord
            CreateGroupProfileLockRecoveryRecord(
                string journalDirectory,
                string endpointIp,
                int endpointPort,
                bool expectedProfileLocked,
                GroupProfileLockRecoveryState state)
        {
            using (var journal = GroupProfileLockRecoveryJournal.Open(
                GetGroupProfileLockRecoveryJournalDirectory(
                    journalDirectory)))
            {
                var record = journal.ArmBeforeDispatch(
                    expectedProfileLocked,
                    endpointIp,
                    endpointPort,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                if (state
                    == GroupProfileLockRecoveryState.AcceptedAwaitingProof)
                {
                    record = journal.MarkAccepted(
                        record.Identity,
                        record.UpdatedUtc.AddTicks(1));
                }
                else if (state
                    == GroupProfileLockRecoveryState.RecoveryRequired)
                {
                    record = journal.PromoteToRecoveryRequired(
                        record.Identity,
                        record.UpdatedUtc.AddTicks(1));
                }
                else if (state
                    != GroupProfileLockRecoveryState.ArmedBeforeDispatch)
                {
                    throw new ArgumentOutOfRangeException("state");
                }

                return record;
            }
        }

        private static string GetGroupProfileLockRecoveryJournalDirectory(
            string journalDirectory)
        {
            return System.IO.Path.Combine(
                journalDirectory,
                "GroupProfileLockRecovery");
        }

        private static GroupProfileLockRecoveryJournal
            GetGroupProfileLockRecoveryJournal(MainWindow window)
        {
            var journal = GetPrivateField(
                window,
                "groupProfileLockRecoveryJournal")
                as GroupProfileLockRecoveryJournal;
            AssertEx.NotNull(journal);
            return journal;
        }

        private static int CountGroupMutationRequests(
            IList<byte[]> requests)
        {
            return CountRequestCommand(requests, 0x2047)
                + CountRequestCommand(requests, 0x2048)
                + CountRequestCommand(requests, 0x2049)
                + CountRequestCommand(requests, 0x204A)
                + CountRequestCommand(requests, 0x204B)
                + CountRequestCommand(requests, 0x2085);
        }

        private static void CloseConnectionWithActiveRecovery(
            MainWindow window)
        {
            var closeTask = (Task)InvokePrivate(
                window,
                "CloseCurrentConnectionAsync",
                false);
            WaitUntil(
                () => closeTask.IsCompleted,
                "The active-recovery close task did not complete.");
            closeTask.GetAwaiter().GetResult();
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The active-recovery connection did not close.");
            ForceCloseWindowWithActiveRecovery(window);
        }

        private static void ForceCloseWindowWithActiveRecovery(
            MainWindow window)
        {
            SetPrivateField(window, "allowWindowClose", true);
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The forced active-recovery window did not close.");
        }

        private static void CloseWindowWithActiveRecoveryBestEffort(
            MainWindow window)
        {
            if (window == null || !window.IsLoaded)
            {
                return;
            }

            try
            {
                if (GetPrivateField(window, "connection") != null)
                {
                    var closeTask = (Task)InvokePrivate(
                        window,
                        "CloseCurrentConnectionAsync",
                        false);
                    WaitUntil(
                        () => closeTask.IsCompleted,
                        "The active-recovery cleanup close did not complete.",
                        3000);
                    closeTask.GetAwaiter().GetResult();
                }
            }
            catch
            {
            }

            try
            {
                ForceCloseWindowWithActiveRecovery(window);
            }
            catch
            {
            }
        }

        private static MainWindow CreatePreparedGroupEnableWindow(
            string journalDirectory,
            int rpcPort)
        {
            var window = CreateWindow(journalDirectory, rpcPort);
            Click(window.ButtonConnect);
            WaitUntil(
                () => window.GridEtherCATTopology.Items.Count
                    == TopologyNodeCount,
                "Topology auto-load did not complete before the group test.");

            window.TextGroupName.Text = GroupEnableWaitName;
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                    window.TextGroupReference.Text,
                    GroupEnableWaitReference.ToString(
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal),
                "The group lookup did not complete.");

            SetPrivateField(window, "groupActiveVerified", true);
            SetPrivateField(window, "groupIdentityConfigured", true);
            InvokePrivate(window, "UpdateUiState");
            AssertEx.True(window.ButtonGroupEnable.IsEnabled);
            return window;
        }

        private static void GroupEnableUsesOneRequestAndThreeStableSamples()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(CapabilitiesStep(
                11,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(GroupEnableWaitEnableStep());
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(GroupEnableWaitPowerOn));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CapabilitiesStep(
                12,
                LMCDiagnosticCapability.EtherCATTopology));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(journalDirectory, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                            == TopologyNodeCount,
                        "Topology auto-load did not complete before the group test.");

                    window.TextGroupName.Text = GroupEnableWaitName;
                    Click(window.ButtonLookupGroup);
                    WaitUntil(
                        () => string.Equals(
                            window.TextGroupReference.Text,
                            GroupEnableWaitReference.ToString(
                                CultureInfo.InvariantCulture),
                            StringComparison.Ordinal),
                        "The group lookup did not complete.");

                    SetPrivateField(window, "groupActiveVerified", true);
                    SetPrivateField(window, "groupIdentityConfigured", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(window.ButtonGroupEnable.IsEnabled);

                    Click(window.ButtonGroupEnable);
                    WaitUntil(
                        () => window.ButtonGroupDisable.IsEnabled
                            && window.TextGroupResult.Text.IndexOf(
                                "Stable=3/3",
                                StringComparison.Ordinal) >= 0,
                        "The GUI did not finish stable Group Lock verification.");

                    AssertEx.Contains(
                        "0x2047 requests=1",
                        window.TextGroupResult.Text);
                    AssertEx.Contains(
                        "ReusedACK=False",
                        window.TextGroupResult.Text);
                    AssertEx.Equal(
                        "4 Enable (Lock Profile)",
                        Convert.ToString(
                            window.ButtonGroupEnable.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(
                        (bool)GetPrivateField(
                            window,
                            "groupProfileLockVerificationPending"));
                    AssertEx.True(
                        (bool)GetPrivateField(window, "groupProfileLocked"));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2047));
                    AssertEx.Equal(
                        5,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static FakeRpcStep GroupEnableWaitLookupStep()
        {
            return GroupEnableWaitLookupStep(GroupEnableWaitReference);
        }

        private static FakeRpcStep GroupEnableWaitLookupStep(
            ushort groupReference)
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(
                payload,
                4,
                groupReference);
            return new FakeRpcStep(
                0x1042,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep GroupEnableWaitEnableStep()
        {
            return new FakeRpcStep(
                0x2047,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2047,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupPowerOnStep()
        {
            return new FakeRpcStep(
                0x204A,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x204A,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupPowerOffStep()
        {
            return new FakeRpcStep(
                0x204B,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x204B,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupStopStep()
        {
            return new FakeRpcStep(
                0x2085,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")));
        }

        private static FakeRpcStep GroupDisableStep()
        {
            return new FakeRpcStep(
                0x2048,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2048,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupDisableRejectedStep()
        {
            return new FakeRpcStep(
                0x2048,
                TestFrame.Response(
                    1,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2048,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static void AddStableGroupDisabledSteps(
            ICollection<FakeRpcStep> steps)
        {
            var stableDisabled = GroupEnableWaitPowerOn
                | GroupEnableWaitDisabled;
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
            steps.Add(GroupEnableWaitStatusStep(stableDisabled));
        }

        private static FakeRpcStep GroupEnableWaitStatusStep(uint state)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);

            var requestPayload = new byte[8];
            TestFrame.WriteInt32(
                requestPayload,
                0,
                GroupEnableWaitReference);
            TestFrame.WriteInt32(requestPayload, 4, 1);

            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2045,
                        GroupEnableWaitReference,
                        requestPayload),
                    request)
                };
        }

        private static GroupPowerRecoveryRecord
            CreateGroupPowerRecoveryRecord(
                string journalRoot,
                string endpointIp,
                int endpointPort,
                bool expectedPowerOn,
                GroupPowerRecoveryState state)
        {
            using (var journal = GroupPowerRecoveryJournal.Open(
                Path.Combine(journalRoot, "GroupPowerRecovery")))
            {
                var record = journal.ArmBeforeDispatch(
                    expectedPowerOn,
                    endpointIp,
                    endpointPort,
                    GroupEnableWaitName,
                    GroupEnableWaitReference,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                if (state
                    == GroupPowerRecoveryState.AcceptedAwaitingProof)
                {
                    record = journal.MarkAccepted(
                        record.Identity,
                        record.UpdatedUtc.AddTicks(1));
                }
                else if (state
                    == GroupPowerRecoveryState.RecoveryRequired)
                {
                    record = journal.PromoteToRecoveryRequired(
                        record.Identity,
                        record.UpdatedUtc.AddTicks(1));
                }

                return record;
            }
        }
    }
}

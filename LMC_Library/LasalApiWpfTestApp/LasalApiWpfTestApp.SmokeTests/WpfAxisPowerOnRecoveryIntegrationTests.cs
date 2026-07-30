using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisPowerOnRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisPowerOnRecovery.AcceptedRestartIsStatusOnlyZeroReplay",
                AcceptedRestartIsStatusOnlyZeroReplay);
            tests.Add(
                "Wpf.AxisPowerRecovery.AcceptedPowerOffRestartIsStatusOnlyZeroReplay",
                AcceptedPowerOffRestartIsStatusOnlyZeroReplay);
            tests.Add(
                "Wpf.AxisPowerRecovery.RecoveryRequiredPowerOffRestartIsStatusOnlyZeroReplay",
                RecoveryRequiredPowerOffRestartIsStatusOnlyZeroReplay);
            tests.Add(
                "Wpf.AxisPowerRecovery.LatePowerOffFailureCannotReviveResolvedTombstone",
                LatePowerOffFailureCannotReviveResolvedTombstone);
            tests.Add(
                "Wpf.AxisPowerRecovery.LatePowerOffCompletionPreservesNewerReplacement",
                LatePowerOffCompletionPreservesNewerReplacement);
            tests.Add(
                "Wpf.AxisPowerRecovery.JournalLocksBlockMutationsButAllowSafetyOffReadAndClose",
                JournalLocksBlockMutationsButAllowSafetyOffReadAndClose);
            tests.Add(
                "Wpf.AxisPowerRecovery.AcceptedObserverTakeoverRacePreservesNewerOffJournal",
                AcceptedObserverTakeoverRacePreservesNewerOffJournal);
            tests.Add(
                "Wpf.AxisPowerRecovery.AcceptedObserverConnectionLossRacePreservesRecoveryJournal",
                AcceptedObserverConnectionLossRacePreservesRecoveryJournal);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.ArmedRestartUsesExplicitPowerOff",
                ArmedRestartUsesExplicitPowerOff);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.ConfirmedPowerOffInterferenceAgainResolvesAndAllowsClose",
                ConfirmedPowerOffInterferenceAgainResolvesAndAllowsClose);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.EndpointMismatchIsZeroTcp",
                AxisPowerOnEndpointMismatchIsZeroTcp);
            tests.Add(
                "Wpf.AxisPowerRecovery.BootIdMismatchRetainsReadOnlyConnectionAndJournal",
                AxisPowerRecoveryBootIdMismatchRetainsReadOnlyConnection);
            tests.Add(
                "Wpf.AxisPowerRecovery.MapRevisionMismatchRetainsReadOnlyConnectionAndJournal",
                AxisPowerRecoveryMapRevisionMismatchRetainsReadOnlyConnection);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.SameProcessDisconnectReconnectIsStatusOnly",
                SameProcessDisconnectReconnectIsStatusOnly);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.ConnectionLossJournalIoFailureStillRunsAllCleanup",
                ConnectionLossJournalIoFailureStillRunsAllCleanup);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.DiagnosticsAdmissionBlocksMutationsOnly",
                DiagnosticsAdmissionBlocksMutationsOnly);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.SameSessionCanceledContinuationResumesWithoutReplay",
                SameSessionCanceledContinuationResumesWithoutReplay);
            tests.Add(
                "Wpf.AxisReset.OneCommandThenThreeStableErrorClearSamples",
                AxisResetOneCommandThenThreeStableErrorClearSamples);
            tests.Add(
                "Wpf.AxisReset.StatusFailureSecondClickResumesWithoutReplay",
                AxisResetStatusFailureSecondClickResumesWithoutReplay);
            tests.Add(
                "Wpf.AxisReset.InterferenceRequiresLaterExplicitReset",
                AxisResetInterferenceRequiresLaterExplicitReset);
            tests.Add(
                "Wpf.AxisReset.RejectedReplacementPreservesConfirmedInterference",
                AxisResetRejectedReplacementPreservesConfirmedInterference);
            tests.Add(
                "Wpf.AxisReset.StatusOnlyResumePowerOffPreemptionRetainsPending",
                AxisResetStatusOnlyResumePowerOffPreemptionRetainsPending);
            tests.Add(
                "Wpf.AxisReset.AcceptedAckOuterSafetyPreemptionPublishesPending",
                AxisResetAcceptedAckOuterSafetyPreemptionPublishesPending);
            tests.Add(
                "Wpf.AxisReset.SafetyPreemptionRetainsPendingAndCleanupClears",
                AxisResetSafetyPreemptionRetainsPendingAndCleanupClears);
        }

        internal static FakeRpcStep[] CreateAxisPowerOnProcessRpcSteps(
            System.Threading.ManualResetEventSlim firstStatusRelease = null,
            System.Threading.ManualResetEventSlim firstStatusEntered = null)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisPowerCommandStep(true));
            var disconnectingStatus = AxisPowerStatusStep(false);
            disconnectingStatus.CloseClientAfterResponseAndContinue = true;
            if (firstStatusRelease != null)
            {
                disconnectingStatus.BeforeResponse = () =>
                {
                    if (firstStatusEntered != null)
                    {
                        firstStatusEntered.Set();
                    }

                    if (!firstStatusRelease.Wait(15000))
                    {
                        throw new TimeoutException(
                            "The held Axis Power On status response was not released.");
                    }
                };
                disconnectingStatus.AllowClientDisconnectAfterRequest = true;
                disconnectingStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
            }
            steps.Add(disconnectingStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        internal static FakeRpcStep[] CreateAxisPowerOffProcessRpcSteps(
            System.Threading.ManualResetEventSlim firstStatusRelease = null,
            System.Threading.ManualResetEventSlim firstStatusEntered = null)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisPowerCommandStep(false));
            var disconnectingStatus = AxisPowerStatusStep(true);
            disconnectingStatus.CloseClientAfterResponseAndContinue = true;
            if (firstStatusRelease != null)
            {
                disconnectingStatus.BeforeResponse = () =>
                {
                    if (firstStatusEntered != null)
                    {
                        firstStatusEntered.Set();
                    }

                    if (!firstStatusRelease.Wait(15000))
                    {
                        throw new TimeoutException(
                            "The held Axis Power Off status response was not released.");
                    }
                };
                disconnectingStatus.AllowClientDisconnectAfterRequest = true;
                disconnectingStatus
                    .ContinueWithNextClientAfterResponseWriteDisconnect = true;
            }
            steps.Add(disconnectingStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        private static void AcceptedRestartIsStatusOnlyZeroReplay()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerOnJournalRecord(
                        root,
                        server.Port,
                        true);
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "Accepted-restart connection identity did not complete.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled
                            && string.Equals(
                                window.TextAxisReference.Text,
                                "1",
                                StringComparison.Ordinal),
                        "The recovery axis did not load.");
                    AssertEx.True(
                        Convert.ToString(
                            window.ButtonPowerOn.Content,
                            CultureInfo.InvariantCulture)
                        .IndexOf(
                            "No 0x2023 Replay",
                            StringComparison.Ordinal) >= 0);

                    Click(window.ButtonPowerOn);
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    WaitUntil(
                        () => !journal.HasActiveRecord,
                        "Status-only accepted restart did not resolve the journal.");
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AcceptedPowerOffRestartIsStatusOnlyZeroReplay()
        {
            PowerOffRestartIsStatusOnlyZeroReplay(
                AxisPowerOnRecoveryState.AcceptedAwaitingProof);
        }

        private static void
            RecoveryRequiredPowerOffRestartIsStatusOnlyZeroReplay()
        {
            PowerOffRestartIsStatusOnlyZeroReplay(
                AxisPowerOnRecoveryState.RecoveryRequired);
        }

        private static void PowerOffRestartIsStatusOnlyZeroReplay(
            AxisPowerOnRecoveryState recoveryState)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerJournalRecord(
                        root,
                        server.Port,
                        false,
                        recoveryState);
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "Power Off restart identity did not complete.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled
                            && string.Equals(
                                window.TextAxisReference.Text,
                                "1",
                                StringComparison.Ordinal),
                        "The Power Off recovery axis did not load.");
                    AssertEx.Contains(
                        "No 0x2023 Replay",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);

                    Click(window.ButtonPowerOff);
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    WaitUntil(
                        () => !journal.HasActiveRecord
                            && window.ButtonCloseConnection.IsEnabled,
                        "Status-only Power Off restart did not resolve the journal and release Close.");
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.False(journal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(0, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.Contains(
                        recoveryState
                                == AxisPowerOnRecoveryState
                                    .AcceptedAwaitingProof
                            ? "Accepted Axis Power Off completed"
                            : "no accepted ACK is claimed",
                        window.TextAxisResult.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            LatePowerOffFailureCannotReviveResolvedTombstone()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(root, 4000);
                var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal");
                var armed = journal.ArmBeforeDispatch(
                    false,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                var accepted = journal.MarkAccepted(
                    armed.Identity,
                    armed.UpdatedUtc.AddTicks(1));
                journal.Resolve(
                    accepted.Identity,
                    accepted.UpdatedUtc.AddTicks(1));

                SetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOnAcceptedRestartRecovery",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed",
                    true);

                InvokePrivate(
                    window,
                    "PreserveAxisPowerOffWaitFailure",
                    null,
                    new InvalidOperationException("late old failure"),
                    accepted,
                    false,
                    true,
                    false,
                    null,
                    "Late Axis Power Off");

                AssertEx.False(journal.HasActiveRecord);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.Resolved,
                    journal.CurrentRecord.State);
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired"));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOnAcceptedRestartRecovery"));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed"));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed"));
                AssertEx.True(GetPrivateField(
                    window,
                    "pendingAxisPowerOffWaitContinuation") == null);

                SetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed",
                    true);
                InvokePrivate(
                    window,
                    "ConfirmAxisPowerOffReplacementAllowed",
                    null,
                    accepted,
                    "Late Axis Power Off status");
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired"));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed"));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed"));

                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Late Power Off failure test window did not close.");
                window = null;
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            LatePowerOffCompletionPreservesNewerReplacement()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(root, 4000);
                var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal");
                var oldArmed = journal.ArmBeforeDispatch(
                    false,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                var oldAccepted = journal.MarkAccepted(
                    oldArmed.Identity,
                    oldArmed.UpdatedUtc.AddTicks(1));
                journal.Resolve(
                    oldAccepted.Identity,
                    oldAccepted.UpdatedUtc.AddTicks(1));

                var newer = journal.ArmBeforeDispatch(
                    false,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    journal.CurrentRecord.UpdatedUtc.AddTicks(1));
                newer = journal.MarkAccepted(
                    newer.Identity,
                    newer.UpdatedUtc.AddTicks(1));
                newer = journal.PromoteToRecoveryRequired(
                    newer.Identity,
                    newer.UpdatedUtc.AddTicks(1));
                SetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed",
                    true);
                SetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed",
                    true);

                var payload = new byte[12];
                TestFrame.WriteUInt32(payload, 0, 0x02000000u);
                var parser = typeof(LMCConnection).GetMethod(
                    "ParseReadStatusResult",
                    System.Reflection.BindingFlags.Static
                        | System.Reflection.BindingFlags.NonPublic);
                AssertEx.NotNull(parser);
                var safeStatus = (LMCReadStatusResult)parser.Invoke(
                    null,
                    new object[] { TestFrame.Response(0, payload) });
                var completion = (System.Threading.Tasks.Task)InvokePrivate(
                    window,
                    "CompleteAxisPowerRecoveryAfterStableProofAsync",
                    null,
                    false,
                    safeStatus,
                    3,
                    3,
                    oldAccepted,
                    "Late Axis Power Off completion");
                completion.GetAwaiter().GetResult();

                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(
                    newer.Identity,
                    journal.CurrentRecord.Identity);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.RecoveryRequired,
                    journal.CurrentRecord.State);
                AssertEx.False(journal.CurrentRecord.ExpectedPowerOn);
                AssertEx.True((bool)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired"));
                AssertEx.True((bool)GetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed"));
                AssertEx.True((bool)GetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed"));

                journal.Resolve(
                    newer.Identity,
                    newer.UpdatedUtc.AddTicks(1));
                SetPrivateField(
                    window,
                    "axisPowerOnRecoveryRequired",
                    false);
                SetPrivateField(
                    window,
                    "axisPowerOffReplacementAllowed",
                    false);
                SetPrivateField(
                    window,
                    "axisPowerOffWaitInterferenceConfirmed",
                    false);
                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Late Power Off completion test window did not close.");
                window = null;
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            JournalLocksBlockMutationsButAllowSafetyOffReadAndClose()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisPowerCommandStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var heldAxisJournal =
                    AxisPowerOnRecoveryJournal.Open(
                        Path.Combine(root, "AxisPowerOnRecovery")))
                using (var heldGroupJournal =
                    GroupPowerRecoveryJournal.Open(
                        Path.Combine(root, "GroupPowerRecovery")))
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    AssertEx.True(GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal") == null);
                    AssertEx.True(GetPrivateField(
                        window,
                        "groupPowerRecoveryJournal") == null);

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The journal-lock admission test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled
                            && window.ButtonReadStatus.IsEnabled,
                        "The journal-lock admission test did not load its safety axis.");

                    var mutation = (DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.NewLiveOrMutation,
                            true);
                    AssertEx.False(mutation.IsAllowed);
                    AssertEx.Equal(
                        DiagnosticsAdmissionDenialReason
                            .PowerRecoveryJournalUnavailable,
                        mutation.DenialReason);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.SafetyControl,
                            true)).IsAllowed);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation
                                .NonD5ReadOnlyInspection,
                            true)).IsAllowed);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation
                                .TrackedD5ReadOnlyInspection,
                            true)).IsAllowed);
                    AssertEx.True(((DiagnosticsAdmissionDecision)
                        InvokePrivate(
                            window,
                            "EvaluateDiagnosticsAdmission",
                            DiagnosticsAdmissionOperation.CloseConnection,
                            true)).IsAllowed);
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonReset.IsEnabled);
                    AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);
                    AssertEx.True(window.ButtonPowerOff.IsEnabled);
                    AssertEx.True(window.ButtonReadStatus.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "Degraded safety Power Off did not prove the safe state while journal locks were held.");
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.False(heldAxisJournal.HasActiveRecord);
                    AssertEx.False(heldGroupJournal.HasActiveRecord);
                    AssertEx.Contains(
                        "process-local tracking",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "no durable recovery record was resolved",
                        window.TextExecutionLog.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AcceptedObserverTakeoverRacePreservesNewerOffJournal()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(root, 4000);
                var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal");
                var powerOn = journal.ArmBeforeDispatch(
                    true,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                AxisPowerOnRecoveryRecord replacement = null;
                window.AxisPowerAcceptedBeforeDurableMarkTestHook = record =>
                {
                    replacement = journal
                        .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            record.Identity,
                            record.EndpointIp,
                            record.EndpointPort,
                            record.AxisName,
                            record.AxisReference,
                            record.DiagnosticsBootId,
                            record.MapRevision,
                            record.UpdatedUtc.AddTicks(1));
                };

                var invocation = AssertEx.Throws<
                    System.Reflection.TargetInvocationException>(
                    () => InvokePrivate(
                        window,
                        "PersistAxisPowerAcceptedState",
                        powerOn,
                        true,
                        "accepted observer takeover race"));
                AssertEx.True(
                    invocation.InnerException
                        is InvalidOperationException);
                AssertEx.NotNull(replacement);
                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(
                    replacement.Identity,
                    journal.CurrentRecord.Identity);
                AssertEx.False(journal.CurrentRecord.ExpectedPowerOn);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                    journal.CurrentRecord.State);
                AssertEx.Equal(
                    null,
                    GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournalRuntimeError"));
                AssertEx.NotNull(GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal"));
                AssertEx.Equal(
                    null,
                    GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournalOpenError"));

                window.AxisPowerAcceptedBeforeDurableMarkTestHook = null;
                journal.Resolve(
                    replacement.Identity,
                    replacement.UpdatedUtc.AddTicks(1));
                InvokePrivate(window, "UpdateUiState");
                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Accepted observer takeover race window did not close.");
                window = null;
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AcceptedObserverConnectionLossRacePreservesRecoveryJournal()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateWindow(root, 4000);
                var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                    window,
                    "axisPowerOnRecoveryJournal");
                var powerOn = journal.ArmBeforeDispatch(
                    true,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                window.AxisPowerAcceptedBeforeDurableMarkTestHook = record =>
                    InvokePrivate(
                        window,
                        "PromoteAxisPowerDispatchOutcomeUncertain",
                        record,
                        "accepted observer connection-loss race");

                var invocation = AssertEx.Throws<
                    System.Reflection.TargetInvocationException>(
                    () => InvokePrivate(
                        window,
                        "PersistAxisPowerAcceptedState",
                        powerOn,
                        true,
                        "accepted observer connection-loss race"));
                AssertEx.True(
                    invocation.InnerException
                        is InvalidOperationException);
                AssertEx.Contains(
                    "connection-loss safety promotion",
                    invocation.InnerException.Message);
                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(
                    powerOn.Identity,
                    journal.CurrentRecord.Identity);
                AssertEx.True(journal.CurrentRecord.ExpectedPowerOn);
                AssertEx.Equal(
                    AxisPowerOnRecoveryState.RecoveryRequired,
                    journal.CurrentRecord.State);
                AssertEx.Equal(
                    null,
                    GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournalRuntimeError"));
                AssertEx.False((bool)typeof(MainWindow).GetProperty(
                    "AxisPowerOnRecoveryJournalUnavailable",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic)
                    .GetValue(window, null));
                AssertEx.False((bool)GetPrivateField(
                    window,
                    "axisPowerDurabilityDegraded"));

                window.AxisPowerAcceptedBeforeDurableMarkTestHook = null;
                journal.Resolve(
                    powerOn.Identity,
                    journal.CurrentRecord.UpdatedUtc.AddTicks(1));
                InvokePrivate(window, "UpdateUiState");
                window.Close();
                WaitUntil(
                    () => !window.IsLoaded,
                    "Accepted observer connection-loss race window did not close.");
                window = null;
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void SameProcessDisconnectReconnectIsStatusOnly()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(
                    CreateAxisPowerOnProcessRpcSteps()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The first Axis Power On session did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled,
                        "The first Axis Power On session did not load the axis.");

                    Click(window.ButtonPowerOn);
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    WaitUntil(
                        () => !string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal)
                            && window.ButtonConnect.IsEnabled
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning")
                            && (bool)GetPrivateField(
                                window,
                                "axisPowerOnAcceptedRestartRecovery"),
                        "Connection loss did not switch the accepted ACK to restart-style status-only recovery.");
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        journal.CurrentRecord.State);
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The exact Axis Power On recovery endpoint did not reconnect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled,
                        "The reconnected Axis Power On recovery axis did not expose status-only resume.");
                    AssertEx.True(
                        Convert.ToString(
                            window.ButtonPowerOn.Content,
                            CultureInfo.InvariantCulture)
                        .IndexOf(
                            "No 0x2023 Replay",
                            StringComparison.Ordinal) >= 0);

                    Click(window.ButtonPowerOn);
                    WaitUntil(
                        () => !journal.HasActiveRecord
                            && journal.CurrentRecord.State
                                == AxisPowerOnRecoveryState.Resolved
                            && window.ButtonCloseConnection.IsEnabled,
                        "Same-process status-only reconnect did not resolve the accepted Axis Power On journal.");

                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(
                        0,
                        CountCommandInSession(server, 2, 0x2023));
                    AssertEx.Equal(
                        3,
                        CountCommandInSession(server, 2, 0x2028));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            ConnectionLossJournalIoFailureStillRunsAllCleanup()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.GridEtherCATTopology.Items.Count
                            == TopologyNodeCount,
                        "The journal-failure cleanup test did not load topology.");

                    var now = DateTime.UtcNow;
                    var axisJournal =
                        (AxisPowerOnRecoveryJournal)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournal");
                    axisJournal.ArmBeforeDispatch(
                        "127.0.0.1",
                        server.Port,
                        "_LMCAxis1",
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        now);

                    var motionJournal =
                        (MotionUncertaintyJournal)GetPrivateField(
                            window,
                            "motionUncertaintyJournal");
                    var motionRecord = motionJournal.ArmBeforeDispatch(
                        Guid.NewGuid(),
                        "127.0.0.1",
                        server.Port,
                        MotionUncertaintyTargetKind.Axis,
                        "_LMCAxis1",
                        1,
                        "Move Absolute",
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        now.AddTicks(1));
                    InvokePrivate(
                        window,
                        "ApplyMotionUncertaintyRecord",
                        motionRecord);

                    var groupJournal =
                        (GroupProfileLockRecoveryJournal)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryJournal");
                    groupJournal.ArmBeforeDispatch(
                        "127.0.0.1",
                        server.Port,
                        "_LMCGroup1",
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        now.AddTicks(2));

                    var mutationJournal =
                        (DiagnosticsMutationJournal)GetPrivateField(
                            window,
                            "diagnosticsMutationJournal");
                    mutationJournal.Arm(
                        DiagnosticsMutationKind.DigitalOutputWrite,
                        Guid.NewGuid(),
                        now.AddTicks(3),
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        1,
                        "Node=test",
                        "Value=test");

                    InvokePrivate(window, "UpdateUiState");
                    window.TextGroupName.Text = "_LMCGroup1";
                    SetPrivateField(
                        window,
                        "groupProfileLockVerificationPending",
                        true);
                    SetPrivateField(window, "groupProfileLocked", true);
                    SetPrivateField(
                        window,
                        "groupProfileLockRecoveryRequired",
                        false);
                    window.TextConnectionState.Text = "SENTINEL";

                    var currentConnection =
                        (LMCConnection)GetPrivateField(window, "connection");
                    using (var journalLock = new FileStream(
                        axisJournal.JournalFilePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None))
                    {
                        var closeTask =
                            currentConnection.CloseConnectionAsync(
                                System.Threading.CancellationToken.None);
                        WaitUntil(
                            () => closeTask.IsCompleted,
                            "The journal-failure cleanup connection did not close.");
                        closeTask.GetAwaiter().GetResult();
                        WaitUntil(
                            () => string.Equals(
                                    window.TextConnectionState.Text,
                                    "Disconnected",
                                    StringComparison.Ordinal)
                                && motionJournal.CurrentRecord.State
                                    == MotionUncertaintyState.RecoveryRequired
                                && groupJournal.CurrentRecord.State
                                    == GroupProfileLockRecoveryState
                                        .RecoveryRequired
                                && mutationJournal.CurrentRecord.State
                                    == DiagnosticsMutationState
                                        .OutcomeUnverified,
                            "Connection-loss cleanup did not finish after the Axis journal I/O failure.");

                        var axisRuntimeError = (string)GetPrivateField(
                            window,
                            "axisPowerOnRecoveryJournalRuntimeError");
                        AssertEx.Contains(
                            "promote-to-recovery",
                            axisRuntimeError);
                        AssertEx.Contains("IOException", axisRuntimeError);
                        AssertEx.Equal(
                            AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                            axisJournal.CurrentRecord.State);
                        AssertEx.True((bool)GetPrivateField(
                            window,
                            "motionRecoveryRequiresExplicitSafetyCommand"));
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "groupProfileLockVerificationPending"));
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "groupProfileLocked"));
                        AssertEx.True((bool)GetPrivateField(
                            window,
                            "groupProfileLockRecoveryRequired"));
                        AssertEx.Equal(
                            null,
                            GetPrivateField(window, "etherCATTopology"));
                        AssertEx.Equal(
                            0,
                            window.GridEtherCATTopology.Items.Count);
                        AssertEx.True(window.ButtonConnect.IsEnabled);
                        AssertEx.Contains(
                            "Axis Power On recovery journal faulted and remains fail-closed",
                            window.TextExecutionLog.Text);
                    }

                    ForceCloseMotionRecoveryWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                ForceCloseMotionRecoveryWindow(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void DiagnosticsAdmissionBlocksMutationsOnly()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The diagnostics-admission test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled,
                        "The diagnostics-admission test did not load the axis.");

                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    var armed = journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        server.Port,
                        "_LMCAxis1",
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);
                    InvokePrivate(window, "UpdateUiState");

                    AssertAxisPowerOnDiagnosticsAdmission(window, false);
                    journal.MarkAccepted(
                        armed.Identity,
                        armed.UpdatedUtc.AddTicks(1));
                    InvokePrivate(window, "UpdateUiState");
                    AssertAxisPowerOnDiagnosticsAdmission(window, false);

                    journal.Resolve(
                        armed.Identity,
                        armed.UpdatedUtc.AddTicks(2));
                    InvokePrivate(window, "UpdateUiState");
                    AssertAxisPowerOnDiagnosticsAdmission(window, true);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            SameSessionCanceledContinuationResumesWithoutReplay()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(AxisPowerCommandStep(true));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(AxisPowerStatusStep(true));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The same-session continuation test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOn.IsEnabled,
                        "The same-session continuation test did not load the axis.");

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        server.Port,
                        "_LMCAxis1",
                        1,
                        DiagnosticsBootId,
                        DiagnosticMapRevision,
                        DateTime.UtcNow);

                    LMCAxisPowerOnWaitContinuation continuation = null;
                    using (var cancellation =
                        new System.Threading.CancellationTokenSource())
                    {
                        var canceled = AssertEx.Throws<
                            LMCAxisPowerStateWaitCanceledException>(
                            () => currentAxis
                                .PowerOnAndWaitForStableStateAsync(
                                    new LMCAxisPowerStateWaitOptions(),
                                    accepted =>
                                    {
                                        InvokePrivate(
                                            window,
                                            "PersistAxisPowerOnAccepted",
                                            accepted,
                                            "same-session test observer");
                                        cancellation.Cancel();
                                    },
                                    cancellation.Token)
                                .GetAwaiter()
                                .GetResult());
                        continuation = canceled.Continuation;
                    }

                    AssertEx.NotNull(continuation);
                    AssertEx.True(continuation.IsPending);
                    AssertEx.Equal(0, CountCommand(server, 0x2028));
                    InvokePrivate(
                        window,
                        "MarkAxisPowerOnAccepted",
                        continuation,
                        "same-session test UI publication");
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(window.ButtonPowerOn.IsEnabled);
                    AssertEx.True(
                        Convert.ToString(
                            window.ButtonPowerOn.Content,
                            CultureInfo.InvariantCulture)
                        .IndexOf(
                            "Resume Power On Verification",
                            StringComparison.Ordinal) >= 0);

                    Click(window.ButtonPowerOn);
                    WaitUntil(
                        () => !journal.HasActiveRecord
                            && journal.CurrentRecord.State
                                == AxisPowerOnRecoveryState.Resolved
                            && window.ButtonCloseConnection.IsEnabled,
                        "The same-session accepted continuation did not resolve by status-only resume.");
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AxisResetOneCommandThenThreeStableErrorClearSamples()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, new byte[8])));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The Axis Reset completion test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonReset.IsEnabled,
                        "The Axis Reset completion test did not load the axis.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset completed",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "Axis Reset did not finish its stable error-clear proof.");

                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.Contains(
                        "Reset submission=Accepted",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "Status polls=3, Stable AxisErrorId=0=3/3",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "DS402 Fault and drive error-register clearance are not proven",
                        window.TextAxisResult.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AxisResetStatusFailureSecondClickResumesWithoutReplay()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(AxisResetCommandStep());
            steps.Add(AxisResetStatusStep(
                functionStatus: 0x0010,
                errorId: -31,
                axisErrorId: 7));
            steps.Add(AxisResetStatusStep());
            steps.Add(AxisResetStatusStep());
            steps.Add(AxisResetStatusStep());
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The Axis Reset resume test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonReset.IsEnabled,
                        "The Axis Reset resume test did not load the axis.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset failed",
                                StringComparison.Ordinal)
                            && window.ButtonReset.IsEnabled,
                        "The first Axis Reset status failure did not settle.");

                    var accepted = (LMCAxisResetWaitContinuation)
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation");
                    AssertEx.NotNull(accepted);
                    AssertEx.True(accepted.IsPending);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisResetWaitInterferenceConfirmed"));
                    AssertEx.False(
                        window.ButtonLookupAxis.IsEnabled,
                        "Load Axis must not replace the owner handle while an accepted Reset continuation is pending.");
                    AssertEx.False(
                        window.TextAxisName.IsEnabled,
                        "Axis identity editing must remain blocked while an accepted Reset continuation is pending.");
                    var requestCountBeforeBlockedLookup =
                        server.ReceivedRequests.Count;
                    InvokePrivate(
                        window,
                        "ButtonLookupAxis_Click",
                        null,
                        new RoutedEventArgs());
                    AssertEx.Equal(
                        requestCountBeforeBlockedLookup,
                        server.ReceivedRequests.Count,
                        "The defensive Load Axis handler sent wire traffic while an accepted Reset continuation was pending.");
                    AssertEx.True(ReferenceEquals(
                        accepted,
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation")));
                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.Contains(
                        "Resume Reset Verification (No 0x2024 Replay)",
                        Convert.ToString(
                            window.ButtonReset.Content,
                            CultureInfo.InvariantCulture));

                    SetPrivateField(window, "motionMayBeActive", true);
                    InvokePrivate(window, "UpdateUiState");
                    AssertEx.True(
                        window.ButtonReset.IsEnabled,
                        "An accepted Reset continuation must remain available for status-only Resume while new live mutations are interlocked.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset completed",
                                StringComparison.Ordinal)
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The accepted Axis Reset did not complete by status-only resume.");
                    SetPrivateField(window, "motionMayBeActive", false);
                    InvokePrivate(window, "UpdateUiState");
                    WaitUntil(
                        () => window.ButtonCloseConnection.IsEnabled,
                        "The Axis Reset resume test did not return to the idle connection state.");

                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(4, CountCommand(server, 0x2028));
                    AssertEx.False(accepted.IsPending);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation") == null);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisResetWaitInterferenceConfirmed"));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisResetInterferenceRequiresLaterExplicitReset()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(AxisResetCommandStep());
            steps.Add(AxisResetStatusStep(
                functionStatus: 0x0010,
                errorId: -31,
                axisErrorId: 7));
            steps.Add(AxisPowerCommandStep(true));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisResetCommandStep());
            steps.Add(AxisResetStatusStep());
            steps.Add(AxisResetStatusStep());
            steps.Add(AxisResetStatusStep());
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The Axis Reset interference test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonReset.IsEnabled,
                        "The Axis Reset interference test did not load the axis.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset failed",
                                StringComparison.Ordinal)
                            && window.ButtonReset.IsEnabled,
                        "The accepted Axis Reset did not retain its failed status proof.");
                    var original = (LMCAxisResetWaitContinuation)
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation");
                    AssertEx.NotNull(original);
                    AssertEx.True(original.IsPending);

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    AssertEx.True(currentAxis.PowerOn().IsSuccess);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "axisResetWaitInterferenceConfirmed")
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The status-only Resume did not confirm same-axis interference.");

                    AssertEx.Equal(1, CountCommand(server, 0x2024));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.True(
                        original.IsPending,
                        "Original Reset state after confirmed interference="
                        + original.State);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation") == null,
                        "Confirmed interference retained a stale WPF Reset continuation pointer.");
                    AssertEx.Contains(
                        "InterveningMutationDetected=True",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "Reset Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonReset.Content,
                            CultureInfo.InvariantCulture));

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset completed",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "The explicit replacement Axis Reset did not complete.");

                    AssertEx.Equal(2, CountCommand(server, 0x2024));
                    AssertEx.Equal(4, CountCommand(server, 0x2028));
                    AssertEx.True(original.IsSuperseded);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation") == null);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisResetWaitInterferenceConfirmed"));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AxisResetRejectedReplacementPreservesConfirmedInterference()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(AxisResetCommandStep());
            steps.Add(AxisResetStatusStep(
                functionStatus: 0x0010,
                errorId: -31,
                axisErrorId: 7));
            steps.Add(AxisPowerCommandStep(true));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(AxisResetRejectedCommandStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The rejected replacement Reset test did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonReset.IsEnabled,
                        "The rejected replacement Reset test did not load the axis.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Reset failed",
                                StringComparison.Ordinal)
                            && window.ButtonReset.IsEnabled,
                        "The original accepted Reset did not retain its failed status proof.");
                    var original = (LMCAxisResetWaitContinuation)
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation");
                    AssertEx.NotNull(original);
                    AssertEx.True(original.IsPending);

                    var currentAxis = (LMCSingleAxis)GetPrivateField(
                        window,
                        "axis");
                    AssertEx.True(currentAxis.PowerOn().IsSuccess);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, true));

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "axisResetWaitInterferenceConfirmed")
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The original Reset did not reach confirmed interference.");
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation") == null,
                        "Confirmed interference retained a stale WPF Reset continuation pointer.");

                    Click(window.ButtonReset);
                    WaitUntil(
                        () => CountCommand(server, 0x2024) == 2
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The explicit replacement Reset rejection did not settle.");

                    AssertEx.True(original.IsPending);
                    AssertEx.False(original.IsSuperseded);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisResetWaitContinuation") == null,
                        "Rejected replacement revived the stale WPF Reset continuation pointer.");
                    AssertEx.True((bool)GetPrivateField(
                        window,
                        "axisResetWaitInterferenceConfirmed"));
                    AssertEx.Equal(2, CountCommand(server, 0x2024));
                    AssertEx.Equal(1, CountCommand(server, 0x2028));
                    AssertEx.Contains(
                        "Reset submission=Rejected",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "The durable record remains RecoveryRequired; no automatic 0x2024 replay is allowed.",
                        window.TextAxisResult.Text);
                    AssertEx.Contains(
                        "Reset Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonReset.Content,
                            CultureInfo.InvariantCulture));

                    AssertEx.False(
                        window.ButtonCloseConnection.IsEnabled,
                        "Confirmed Reset interference must remain fail-closed after replacement NACK.");
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AxisResetStatusOnlyResumePowerOffPreemptionRetainsPending()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            using (var releaseStatus = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                steps.Add(AxisResetStatusStep(
                    functionStatus: 0x0010,
                    errorId: -31,
                    axisErrorId: 7));
                var heldStatus = AxisResetStatusStep();
                heldStatus.BeforeResponse = () =>
                {
                    statusEntered.Set();
                    if (!releaseStatus.Wait(5000))
                    {
                        throw new TimeoutException(
                            "The held status-only Reset response was not released.");
                    }
                };
                steps.Add(heldStatus);
                steps.Add(CapabilitiesStep(11, capabilities));
                steps.Add(AxisPowerCommandStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(AxisPowerStatusStep(false));
                steps.Add(CapabilitiesStep(12, capabilities));

                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        Click(window.ButtonConnect);
                        WaitUntil(
                            () => window.ButtonLookupAxis.IsEnabled,
                            "The status-only Reset Power Off test did not connect.");
                        Click(window.ButtonLookupAxis);
                        WaitUntil(
                            () => window.ButtonReset.IsEnabled,
                            "The status-only Reset Power Off test did not load the axis.");

                        Click(window.ButtonReset);
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Reset failed",
                                    StringComparison.Ordinal)
                                && window.ButtonReset.IsEnabled,
                            "The accepted Reset did not retain a resumable continuation.");
                        var accepted = (LMCAxisResetWaitContinuation)
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation");
                        AssertEx.NotNull(accepted);
                        AssertEx.True(accepted.IsPending);
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2028));

                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The status-only Reset Resume request was not held.");
                        var coordinator =
                            (LMCSendPriorityCoordinator)GetPrivateField(
                                window,
                                "sendPriorityCoordinator");
                        var resetGeneration = coordinator.CurrentGeneration;
                        AssertEx.True(window.ButtonPowerOff.IsEnabled);
                        var capabilityReadsBeforePowerOff =
                            CountCommand(server, 0x7E00);
                        Click(window.ButtonPowerOff);
                        WaitUntil(
                            () => coordinator.CurrentGeneration
                                > resetGeneration,
                            "Axis Power Off did not reserve safety priority.");
                        releaseStatus.Set();

                        try
                        {
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Power Off verified",
                                        StringComparison.Ordinal)
                                    && !(bool)GetPrivateField(
                                        window,
                                        "safetyCommandRunning"),
                                "Axis Power Off did not preempt status-only Reset Resume and prove standstill.");
                        }
                        catch (TimeoutException error)
                        {
                            throw MotionRecoveryTimeout(
                                "Axis Power Off did not preempt status-only Reset Resume and prove standstill.",
                                window,
                                server.ReceivedRequests,
                                error);
                        }

                        AssertEx.True(accepted.IsPending);
                        AssertEx.True(ReferenceEquals(
                            accepted,
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation")));
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "axisResetWaitInterferenceConfirmed"));
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                        AssertEx.Equal(5, CountCommand(server, 0x2028));
                        AssertEx.Equal(
                            capabilityReadsBeforePowerOff + 2,
                            CountCommand(server, 0x7E00));
                        AssertEx.False(
                            window.ButtonCloseConnection.IsEnabled,
                            "An accepted Reset still requiring proof must block normal connection close after Power Off.");
                        server.Verify();
                    }
                }
                finally
                {
                    releaseStatus.Set();
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void
            AxisResetAcceptedAckOuterSafetyPreemptionPublishesPending()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var resetResponseWritten = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                var reset = AxisResetCommandStep();
                reset.AfterResponse = request => resetResponseWritten.Set();
                steps.Add(reset);
                steps.Add(new FakeRpcStep(0, null)
                {
                    RequireClientDisconnectBeforeRequest = true,
                    ContinueWithNextClientAfterDisconnect = true
                });
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(1, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisStopCommandStep());
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(2, capabilities));
                steps.Add(CloseStep());

                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        Click(window.ButtonConnect);
                        WaitUntil(
                            () => window.ButtonLookupAxis.IsEnabled,
                            "The Reset ACK publication boundary test did not connect.");
                        Click(window.ButtonLookupAxis);
                        WaitUntil(
                            () => window.ButtonReset.IsEnabled,
                            "The Reset ACK publication boundary test did not load the axis.");

                        var currentAxis = (LMCSingleAxis)GetPrivateField(
                            window,
                            "axis");
                        var originalConnection = GetPrivateField(
                            window,
                            "connection");
                        var coordinator =
                            (LMCSendPriorityCoordinator)GetPrivateField(
                                window,
                                "sendPriorityCoordinator");
                        var resetGeneration = coordinator.CurrentGeneration;

                        Click(window.ButtonReset);
                        AssertEx.True(
                            resetResponseWritten.Wait(5000),
                            "The Axis Reset ACK response was not written.");
                        AssertEx.True(
                            SpinWait.SpinUntil(
                                () => currentAxis.PendingResetWaitContinuation
                                    != null,
                                5000),
                            "The SDK did not atomically publish the accepted Reset continuation.");
                        var accepted =
                            currentAxis.PendingResetWaitContinuation;
                        AssertEx.NotNull(accepted);
                        AssertEx.True(accepted.IsPending);
                        AssertEx.True(
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation") == null,
                            "The WPF continuation unexpectedly ran before the Dispatcher was released.");
                        AssertEx.True(window.ButtonStop.IsEnabled);

                        Click(window.ButtonStop);
                        AssertEx.True(
                            coordinator.CurrentGeneration > resetGeneration,
                            "Axis Stop did not reserve safety priority before WPF Reset publication.");

                        try
                        {
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Stop verified",
                                        StringComparison.Ordinal)
                                    && window.ButtonCloseConnection.IsEnabled,
                                "Axis Stop did not finish after the Reset ACK publication boundary preemption.");
                        }
                        catch (TimeoutException error)
                        {
                            throw MotionRecoveryTimeout(
                                "Axis Stop did not finish after the Reset ACK publication boundary preemption.",
                                window,
                                server.ReceivedRequests,
                                error);
                        }

                        AssertEx.True(accepted.IsPending);
                        AssertEx.True(
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation") == null,
                            "The aborted old-session Reset continuation remained published in WPF state.");
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "axisResetWaitInterferenceConfirmed"));
                        AssertEx.False(ReferenceEquals(
                            originalConnection,
                            GetPrivateField(window, "connection")));
                        AssertEx.False(ReferenceEquals(
                            currentAxis,
                            GetPrivateField(window, "axis")));
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(3, CountCommand(server, 0x2028));
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(
                            0,
                            CountCommandInSession(server, 1, 0x2022));
                        AssertEx.Equal(
                            1,
                            CountCommandInSession(server, 2, 0x2022));

                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void
            AxisResetSafetyPreemptionRetainsPendingAndCleanupClears()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                var heldStatus = AxisResetStatusStep();
                heldStatus.BeforeResponse = () => statusEntered.Set();
                heldStatus.WaitForClientDisconnectBeforeResponseAndContinue =
                    true;
                steps.Add(heldStatus);
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(1, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisStopCommandStep());
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(2, capabilities));
                steps.Add(CloseStep());

                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        Click(window.ButtonConnect);
                        WaitUntil(
                            () => window.ButtonLookupAxis.IsEnabled,
                            "The Axis Reset preemption test did not connect.");
                        Click(window.ButtonLookupAxis);
                        WaitUntil(
                            () => window.ButtonReset.IsEnabled,
                            "The Axis Reset preemption test did not load the axis.");

                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The Axis Reset status request was not held.");
                        var originalAxis = GetPrivateField(window, "axis");
                        var originalConnection = GetPrivateField(
                            window,
                            "connection");
                        var accepted = ((LMCSingleAxis)originalAxis)
                            .PendingResetWaitContinuation;
                        AssertEx.NotNull(accepted);
                        AssertEx.True(accepted.IsPending);
                        var coordinator =
                            (LMCSendPriorityCoordinator)GetPrivateField(
                                window,
                                "sendPriorityCoordinator");
                        var resetGeneration = coordinator.CurrentGeneration;
                        AssertEx.True(window.ButtonStop.IsEnabled);
                        Click(window.ButtonStop);
                        WaitUntil(
                            () => coordinator.CurrentGeneration
                                > resetGeneration,
                            "Axis Stop did not reserve safety priority.");

                        try
                        {
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Stop verified",
                                        StringComparison.Ordinal)
                                    && window.ButtonCloseConnection.IsEnabled,
                                "Axis Stop did not preempt Reset verification and prove standstill.");
                        }
                        catch (TimeoutException error)
                        {
                            throw MotionRecoveryTimeout(
                                "Axis Stop did not preempt Reset verification and prove standstill.",
                                window,
                                server.ReceivedRequests,
                                error);
                        }

                        AssertEx.True(accepted.IsPending);
                        AssertEx.True(
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation") == null,
                            "The aborted old-session Reset continuation remained published in WPF state.");
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "axisResetWaitInterferenceConfirmed"));
                        AssertEx.False(ReferenceEquals(
                            originalConnection,
                            GetPrivateField(window, "connection")));
                        AssertEx.False(ReferenceEquals(
                            originalAxis,
                            GetPrivateField(window, "axis")));
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(4, CountCommand(server, 0x2028));
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(
                            0,
                            CountCommandInSession(server, 1, 0x2022));
                        AssertEx.Equal(
                            1,
                            CountCommandInSession(server, 2, 0x2022));

                        CloseConnectedWindow(window);
                        AssertEx.True(
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation") == null);
                        AssertEx.False((bool)GetPrivateField(
                            window,
                            "axisResetWaitInterferenceConfirmed"));
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void AssertAxisPowerOnDiagnosticsAdmission(
            MainWindow window,
            bool mutationsAllowed)
        {
            var mutation = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation.NewLiveOrMutation,
                true);
            AssertEx.Equal(mutationsAllowed, mutation.IsAllowed);
            AssertEx.Equal(
                mutationsAllowed
                    ? DiagnosticsAdmissionDenialReason.None
                    : DiagnosticsAdmissionDenialReason
                        .AxisPowerOnUnresolved,
                mutation.DenialReason);

            var trackedMutation =
                (DiagnosticsAdmissionDecision)InvokePrivate(
                    window,
                    "EvaluateDiagnosticsAdmission",
                    DiagnosticsAdmissionOperation.TrackedD5Submit,
                    true);
            AssertEx.Equal(mutationsAllowed, trackedMutation.IsAllowed);
            var trackedRead = (DiagnosticsAdmissionDecision)InvokePrivate(
                window,
                "EvaluateDiagnosticsAdmission",
                DiagnosticsAdmissionOperation
                    .TrackedD5ReadOnlyInspection,
                true);
            AssertEx.True(trackedRead.IsAllowed);

            AssertEx.Equal(
                mutationsAllowed,
                window.ComboQualificationGroupAxis.IsEnabled);
            AssertEx.True(window.ButtonDiagnosticsCapabilities.IsEnabled);
            AssertEx.True(window.ButtonReadDriveStatus.IsEnabled);
            AssertEx.True(window.ButtonReadStatus.IsEnabled);
            AssertEx.True(window.ButtonPowerOff.IsEnabled);
            AssertEx.Equal(
                mutationsAllowed,
                window.ButtonCloseConnection.IsEnabled);
        }

        private static void ArmedRestartUsesExplicitPowerOff()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerCommandStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerOnJournalRecord(
                        root,
                        server.Port,
                        false);
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "Armed-restart connection identity did not complete.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled
                            && string.Equals(
                                window.TextAxisReference.Text,
                                "1",
                                StringComparison.Ordinal),
                        "The armed recovery axis did not load.");
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);

                    Click(window.ButtonPowerOff);
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    WaitUntil(
                        () => !journal.HasActiveRecord,
                        "Explicit Power Off safe proof did not resolve the journal.");
                    AssertEx.Equal(0, CountAxisPowerCommand(server, true));
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            ConfirmedPowerOffInterferenceAgainResolvesAndAllowsClose()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisPowerCommandStep(false));
            steps.Add(AxisPowerStatusFailureStep());
            steps.Add(AxisStopCommandStep());
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(AxisPowerCommandStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(AxisPowerStatusStep(false));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(CloseStep());

            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerOnJournalRecord(
                        root,
                        server.Port,
                        false);
                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonLookupAxis.IsEnabled,
                        "The confirmed Power Off interference recovery did not connect.");
                    Click(window.ButtonLookupAxis);
                    WaitUntil(
                        () => window.ButtonPowerOff.IsEnabled,
                        "The confirmed Power Off interference recovery did not load the axis.");

                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verification failed",
                                StringComparison.Ordinal)
                            && window.ButtonStop.IsEnabled,
                        "The first accepted Power Off status failure did not retain recovery.");
                    var original = (LMCAxisPowerOffWaitContinuation)
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation");
                    AssertEx.NotNull(original);
                    AssertEx.True(original.IsPending);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));

                    Click(window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Stop verified",
                                StringComparison.Ordinal)
                            && window.ButtonPowerOff.IsEnabled,
                        "The explicit Stop did not complete before Power Off interference confirmation.");
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.False(window.ButtonCloseConnection.IsEnabled);

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                                window,
                                "axisPowerOffWaitInterferenceConfirmed")
                            && window.ButtonPowerOff.IsEnabled,
                        "The status-only Power Off Resume did not confirm Stop interference.");
                    AssertEx.Contains(
                        "Power Off Again (Confirmed Interference)",
                        Convert.ToString(
                            window.ButtonPowerOff.Content,
                            CultureInfo.InvariantCulture));
                    AssertEx.True(original.IsPending);
                    AssertEx.Equal(1, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(1, CountCommand(server, 0x2022));
                    AssertEx.Equal(4, CountCommand(server, 0x2028));

                    Click(window.ButtonPowerOff);
                    WaitUntil(
                        () => !journal.HasActiveRecord
                            && string.Equals(
                                window.TextOperationState.Text,
                                "Power Off verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "Power Off Again did not resolve Axis Power On recovery and release Close.");

                    AssertEx.Equal(2, CountAxisPowerCommand(server, false));
                    AssertEx.Equal(7, CountCommand(server, 0x2028));
                    AssertEx.Equal(7, CountCommand(server, 0x7E00));
                    AssertEx.True(original.IsSuperseded);
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingAxisPowerOffWaitContinuation") == null);
                    AssertEx.False((bool)GetPrivateField(
                        window,
                        "axisPowerOffWaitInterferenceConfirmed"));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisPowerOnEndpointMismatchIsZeroTcp()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer())
                {
                    var journalDirectory = Path.Combine(
                        root,
                        "AxisPowerOnRecovery");
                    using (var journal =
                        AxisPowerOnRecoveryJournal.Open(journalDirectory))
                    {
                        var armed = journal.ArmBeforeDispatch(
                            "127.0.0.2",
                            server.Port,
                            "_LMCAxis1",
                            1,
                            DiagnosticsBootId,
                            DiagnosticMapRevision,
                            DateTime.UtcNow);
                        journal.MarkAccepted(
                            armed.Identity,
                            armed.UpdatedUtc.AddTicks(1));
                    }

                    window = CreateWindow(root, server.Port);
                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => window.ButtonConnect.IsEnabled,
                        "Endpoint mismatch did not return the UI to idle.");
                    AssertEx.Equal(0, server.AcceptedClientCount);
                    AssertEx.Equal(0, server.ReceivedRequests.Count);

                    InvokePrivate(
                        window,
                        "ResolveAxisPowerOnRecoveryJournal",
                        "test cleanup");
                    InvokePrivate(window, "UpdateUiState");
                    window.Close();
                    WaitUntil(
                        () => !window.IsLoaded,
                        "Endpoint-mismatch test window did not close.");
                    window = null;
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void
            AxisPowerRecoveryBootIdMismatchRetainsReadOnlyConnection()
        {
            AssertAxisPowerRecoveryIdentityMismatchRetainsReadOnlyConnection(
                DiagnosticsBootId + 1,
                DiagnosticMapRevision,
                "BootId");
        }

        private static void
            AxisPowerRecoveryMapRevisionMismatchRetainsReadOnlyConnection()
        {
            AssertAxisPowerRecoveryIdentityMismatchRetainsReadOnlyConnection(
                DiagnosticsBootId,
                DiagnosticMapRevision + 1,
                "MapRevision");
        }

        private static void
            AssertAxisPowerRecoveryIdentityMismatchRetainsReadOnlyConnection(
                uint observedBootId,
                uint observedMapRevision,
                string mismatchName)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(MotionRecoveryCapabilitiesStep(
                11,
                capabilities,
                observedBootId,
                observedMapRevision));
            steps.Add(MotionRecoveryCapabilitiesStep(
                12,
                capabilities,
                observedBootId,
                observedMapRevision));
            steps.Add(CloseStep());

            var root = CreateAxisPowerOnTemporaryDirectory();
            var diagnosticsMutationIdentity = Guid.NewGuid();
            var preservedDiagnosticsState =
                DiagnosticsMutationState.ArmedBeforeDispatch;
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    CreateAxisPowerJournalRecord(
                        root,
                        server.Port,
                        false,
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof);
                    using (var diagnosticsJournal =
                        DiagnosticsMutationJournal.Open(root))
                    {
                        diagnosticsJournal.Arm(
                            DiagnosticsMutationKind.SdoWrite,
                            diagnosticsMutationIdentity,
                            DateTime.UtcNow,
                            DiagnosticsBootId,
                            DiagnosticMapRevision,
                            7,
                            "Slave=1,Object=0x2F00,SubIndex=24,Type=Int32,Length=4",
                            "WriteData=2A-00-00-00",
                            new DiagnosticsSdoWriteMutationMetadata(
                                1,
                                0x2F00,
                                24,
                                LMCSignalValueType.Int32,
                                4,
                                1000,
                                new byte[] { 0x2A, 0, 0, 0 }));
                    }
                    window = CreateWindow(root, server.Port);
                    var journal = (AxisPowerOnRecoveryJournal)GetPrivateField(
                        window,
                        "axisPowerOnRecoveryJournal");
                    var preserved = journal.CurrentRecord;
                    var diagnosticsJournalInWindow =
                        (DiagnosticsMutationJournal)GetPrivateField(
                            window,
                            "diagnosticsMutationJournal");

                    Click(window.ButtonConnect);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Connect completed",
                                StringComparison.Ordinal)
                            && string.Equals(
                                window.TextConnectionState.Text,
                                "Connected",
                                StringComparison.Ordinal),
                        mismatchName
                            + " mismatch did not retain a read-only connection.");
                    preservedDiagnosticsState =
                        diagnosticsJournalInWindow.CurrentRecord.State;

                    AssertEx.True(window.ButtonDiagnosticsCapabilities.IsEnabled);
                    AssertEx.True(window.ButtonCloseConnection.IsEnabled);
                    AssertEx.False(window.ButtonLookupAxis.IsEnabled);
                    AssertEx.False(window.ButtonPowerOn.IsEnabled);
                    AssertEx.False(window.ButtonPowerOff.IsEnabled);
                    AssertEx.False(
                        window.CheckPersistedMutationPhysicallyVerified.IsEnabled);
                    AssertEx.False(
                        window.ButtonAcknowledgePersistedMutation.IsEnabled);
                    Click(window.ButtonDiagnosticsCapabilities);
                    WaitUntil(
                        () => string.Equals(
                            window.TextOperationState.Text,
                            "Refresh Diagnostics Capabilities completed",
                            StringComparison.Ordinal),
                        "Read-only 0x7E00 inspection was not available in quarantine.");

                    AssertNoMotionMutationRequests(
                        server.ReceivedRequests,
                        mismatchName + " quarantine sent a mutation.");
                    AssertEx.Contains(
                        "RECOVERY IDENTITY READ-ONLY QUARANTINE",
                        window.TextExecutionLog.Text);
                    AssertEx.Contains(
                        "Stored BootId=0x",
                        window.TextExecutionLog.Text);
                    AssertEx.Equal(preserved.Identity, journal.CurrentRecord.Identity);
                    AssertEx.Equal(preserved.State, journal.CurrentRecord.State);
                    AssertEx.Equal(
                        preserved.ExpectedPowerOn,
                        journal.CurrentRecord.ExpectedPowerOn);
                    AssertEx.Equal(
                        preserved.DiagnosticsBootId,
                        journal.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(
                        preserved.MapRevision,
                        journal.CurrentRecord.MapRevision);
                    AssertEx.Equal(
                        preserved.UpdatedUtc,
                        journal.CurrentRecord.UpdatedUtc);
                    AssertEx.True(
                        diagnosticsJournalInWindow.HasActiveRecord);
                    AssertEx.Equal(
                        diagnosticsMutationIdentity,
                        diagnosticsJournalInWindow.CurrentRecord.Identity);
                    AssertEx.Equal(
                        preservedDiagnosticsState,
                        diagnosticsJournalInWindow.CurrentRecord.State);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }

                using (var reopened = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "AxisPowerOnRecovery")))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        reopened.CurrentRecord.State);
                    AssertEx.False(reopened.CurrentRecord.ExpectedPowerOn);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        reopened.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(
                        DiagnosticMapRevision,
                        reopened.CurrentRecord.MapRevision);
                }
                using (var reopened = DiagnosticsMutationJournal.Open(root))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        diagnosticsMutationIdentity,
                        reopened.CurrentRecord.Identity);
                    AssertEx.Equal(
                        DiagnosticsBootId,
                        reopened.CurrentRecord.DiagnosticsBootId);
                    AssertEx.Equal(
                        DiagnosticMapRevision,
                        reopened.CurrentRecord.IdentityRevision);
                    AssertEx.Equal(
                        preservedDiagnosticsState,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void CreateAxisPowerOnJournalRecord(
            string root,
            int port,
            bool markAccepted)
        {
            CreateAxisPowerJournalRecord(
                root,
                port,
                true,
                markAccepted
                    ? AxisPowerOnRecoveryState.AcceptedAwaitingProof
                    : AxisPowerOnRecoveryState.ArmedBeforeDispatch);
        }

        private static void CreateAxisPowerJournalRecord(
            string root,
            int port,
            bool expectedPowerOn,
            AxisPowerOnRecoveryState state)
        {
            using (var journal = AxisPowerOnRecoveryJournal.Open(
                Path.Combine(root, "AxisPowerOnRecovery")))
            {
                var current = journal.ArmBeforeDispatch(
                    expectedPowerOn,
                    "127.0.0.1",
                    port,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow);
                if (state == AxisPowerOnRecoveryState.ArmedBeforeDispatch)
                {
                    return;
                }

                current = journal.MarkAccepted(
                    current.Identity,
                    current.UpdatedUtc.AddTicks(1));
                if (state == AxisPowerOnRecoveryState.AcceptedAwaitingProof)
                {
                    return;
                }

                if (state == AxisPowerOnRecoveryState.RecoveryRequired)
                {
                    journal.PromoteToRecoveryRequired(
                        current.Identity,
                        current.UpdatedUtc.AddTicks(1));
                    return;
                }

                throw new ArgumentOutOfRangeException(
                    "state",
                    state,
                    "Only active Axis Power recovery states are supported.");
            }
        }

        private static FakeRpcStep AxisPowerStatusStep(bool powerOn)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(
                payload,
                0,
                0x02000000u | (powerOn ? 1u : 0u));
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisPowerStatusFailureStep()
        {
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(7, new byte[12]));
        }

        private static FakeRpcStep AxisResetCommandStep()
        {
            return new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep AxisResetRejectedCommandStep()
        {
            return new FakeRpcStep(
                0x2024,
                TestFrame.Response(0, TestFrame.Hex("01 00 F9 FF")));
        }

        private static FakeRpcStep AxisResetStatusStep(
            uint state = 0,
            ushort functionStatus = 0,
            short errorId = 0,
            ushort axisErrorId = 0,
            ushort statusWord = 0)
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, state);
            TestFrame.WriteUInt16(payload, 4, functionStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            TestFrame.WriteUInt16(payload, 8, axisErrorId);
            TestFrame.WriteUInt16(payload, 10, statusWord);
            return new FakeRpcStep(
                0x2028,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisStopCommandStep()
        {
            return new FakeRpcStep(
                0x2022,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep AxisPowerCommandStep(bool powerOn)
        {
            return new FakeRpcStep(
                0x2023,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")))
            {
                InspectRequest = request => AssertEx.Equal(
                    (byte)(powerOn ? 1 : 0),
                    request[12])
            };
        }

        private static int CountAxisPowerCommand(
            FakeRpcServer server,
            bool powerOn)
        {
            return server.ReceivedRequests.Count(request =>
                TestFrame.ReadUInt16(request, 0) == 0x2023
                && request.Length > 12
                && request[12] == (powerOn ? (byte)1 : (byte)0));
        }

        private static int CountCommand(FakeRpcServer server, ushort command)
        {
            return server.ReceivedRequests.Count(request =>
                TestFrame.ReadUInt16(request, 0) == command);
        }

        private static string CreateAxisPowerOnTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoAxisPowerOnWpfTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteAxisPowerOnTemporaryDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
